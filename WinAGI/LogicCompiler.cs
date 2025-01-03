using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static WinAGI.Common.Base;
using static WinAGI.Engine.LogicDecoder;
using static WinAGI.Engine.ArgType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
using static WinAGI.Engine.LogicErrorLevel;
using System.Diagnostics;
using System.Net.Quic;
using WinAGI.Common;
using System.Windows.Forms;

namespace WinAGI.Engine {
    /// <summary>
    /// This class contains all the members and methods needed to compile logic 
    /// source code.
    /// </summary>
    public static partial class LogicCompiler {
        // reminder: in VB6 version, the compiler was not case sensitive
        // EXCEPT strings in messages; in this version ALL tokens are
        // case sensitive

        #region Structs
        private struct LogicGoto {
            internal byte LabelNum;
            internal int DataLoc;
        }

        internal struct LogicLabel {
            internal string Name;
            internal int Loc;
        }

        internal struct CompilerBlockType {
            internal bool IsIf = false;
            internal int StartPos = 0;
            internal int Length = 0;
            public CompilerBlockType() {
            }
        }
        #endregion

        #region Enums
        enum DefineType {
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
        internal static AGIGame compGame;
        private static Logic compLogic;
        // compiler warnings
        public const int WARNCOUNT = 117;
        internal static bool[] agNoCompWarn = new bool[WARNCOUNT + 1];
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

        internal static Logic tmpLogRes;
        internal const int MAX_BLOCK_DEPTH = 64;
        internal const int MAX_LABELS = 255;
        internal const string NOT_TOKEN = "!";
        internal const string OR_TOKEN = "||";
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
        internal static string errorLine = "";
        internal static string errorModule = "";
        internal static bool blnCriticalError, blnMinorError;
        internal static int lngQuoteAdded;
        internal static int intCtlCount;
        internal static int endingCmd = 0; // 1 = return, 2 = new.room, 3 = quit
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
        internal static bool[] blnMsg = new bool[256];
        internal static int[] intMsgWarn = new int[256]; //to track warnings found during msgread function
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
        /// <summary>
        /// Gets or sets how the compiler responds to errors and unusual conditions
        /// encountered in logic source code when compiling.
        /// </summary>
        public static LogicErrorLevel ErrorLevel { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// This method initializes all reserved defines used by the logic compiler.
        /// </summary>
        public static void AssignReservedDefines() {
            // predefined variables, flags, and objects
            // Variables v0 - v26
            // Flags f0 - f16, f20 [in version 3.102 and above]
            // object variable, o0, gameversion string, gameabout string
            //and numberofitems
            // also various numerical constants

            // variables
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

            // flags
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

            // edge codes
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

            // object direction
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

            // video modes
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

            // computer types
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

            // colors
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

            // set types and defaults
            int i;
            for (i = 0; i <= 26; i++) {
                agResVar[i].Type = ArgType.Var;
                agResVar[i].Default = agResVar[i].Name;
            }
            for (i = 0; i <= 17; i++) {
                agResFlag[i].Type = ArgType.Flag;
                agResFlag[i].Default = agResFlag[i].Name;
            }
            for (i = 0; i <= 4; i++) {
                agEdgeCodes[i].Type = ArgType.Num;
                agEdgeCodes[i].Default = agEdgeCodes[i].Name;
            }
            for (i = 0; i <= 8; i++) {
                agEgoDir[i].Type = ArgType.Num;
                agEgoDir[i].Default = agEgoDir[i].Name;
            }
            for (i = 0; i <= 4; i++) {
                agVideoMode[i].Type = ArgType.Num;
                agVideoMode[i].Default = agVideoMode[i].Name;
            }
            for (i = 0; i <= 8; i++) {
                agCompType[i].Type = ArgType.Num;
                agCompType[i].Default = agCompType[i].Name;
            }
            for (i = 0; i <= 15; i++) {
                agResColor[i].Type = ArgType.Num;
                agResColor[i].Default = agResColor[i].Name;
            }

            // objects
            agResObj[0].Type = ArgType.SObj;
            agResObj[0].Default = agResObj[0].Name;
            // strings
            agResStr[0].Type = ArgType.Str;
            agResStr[0].Default = agResStr[0].Name;
        }

        /// <summary>
        /// Returns the single letter argument type prefix for the specified
        /// argument type ('v', 'f', etc.).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static string ArgTypePrefix(byte index) {
            if (index > 8) {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            return agArgTypPref[index];
        }
        
        /// <summary>
        /// Returns the name of the specified argument type ("number", "variable", 
        /// "flag", etc.).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static string ArgTypeName(byte index) {
            if (index > 8) {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            return agArgTypName[index];
        }
        
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
            agNoCompWarn[WarningNumber - 5000] = NewVal;
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
            return agNoCompWarn[WarningNumber - 5000];
        }

        /// <summary>
        /// Returns the reserved defines that match the specified argument
        /// type as an array of defines. NOT the same as returning by 'group'
        /// (which is used for saving changes to reserved define names).
        /// </summary>
        /// <param name="ArgType"></param>
        /// <returns></returns>
        public static TDefine[] ReservedDefines(ArgType ArgType) {
            TDefine[] tmpDefines = [];

            switch (ArgType) {
            case Num:
                // return all numerical reserved defines
                tmpDefines = [];
                tmpDefines = [.. tmpDefines, .. agEdgeCodes];
                tmpDefines = [.. tmpDefines, .. agEgoDir];
                tmpDefines = [.. tmpDefines, .. agVideoMode];
                tmpDefines = [.. tmpDefines, .. agCompType];
                tmpDefines = [.. tmpDefines, .. agResColor];
                break;
            case Var:
                // return all variable reserved defines
                tmpDefines = agResVar;
                break;
            case Flag:
                // return all flag reserved defines
                tmpDefines = agResFlag;
                break;
            case Msg:
                // none
                tmpDefines = [];
                break;
            case SObj:
                // one - ego
                tmpDefines = agResObj;
                break;
            case InvItem:
                // none
                tmpDefines = [];
                break;
            case Str:
                // one - input prompt
                tmpDefines = agResStr;
                break;
            case Word:
                // none
                tmpDefines = [];
                break;
            case Ctrl:
                // none
                tmpDefines = [];
                break;
            case DefStr:
                // none
                tmpDefines = [];
                break;
            case VocWrd:
                // none
                tmpDefines = [];
                break;
            }
            return tmpDefines;
        }

        /// <summary>
        /// Returns reserved defines by their 'group' instead by by variable type.
        /// </summary>
        /// <param name="Group"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static TDefine[] ResDefByGrp(ResDefGroup Group) {
            switch (Group) {
            case ResDefGroup.Variable:
                return agResVar;
            case ResDefGroup.Flag:
                return agResFlag;
            case ResDefGroup.EdgeCode:
                return agEdgeCodes;
            case ResDefGroup.ObjectDir:
                return agEgoDir;
            case ResDefGroup.VideoMode:
                return agVideoMode;
            case ResDefGroup.ComputerType:
                return agCompType;
            case ResDefGroup.Color:
                return agResColor;
            case ResDefGroup.Object:
                return agResObj;
            case ResDefGroup.String:
                return agResStr;
            default:
                throw new IndexOutOfRangeException("bad form");
            }
        }

        /// <summary>
        /// This method lets user update a reserved define name. It is up
        /// to calling procedure to make sure there are no conflicts. If the
        /// define value doesn't match an actual reserved item, an exception
        /// is thrown.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="newname"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static void SetResDef(int type, int index, string newname) {
            // type is a numeric value that maps to the six different types
            // (catgories) of reserved defines
            switch (type) {
            case 1:
                // variable: value must be 0-26
                if (index < 0 || index > 27) {
                    throw new IndexOutOfRangeException(nameof(index));
                }
                agResVar[index].Name = newname;
                break;
            case 2:
                // flag: value must be 0-17
                if (index < 0 || index > 17) {
                    throw new IndexOutOfRangeException(nameof(index));
                }
                agResFlag[index].Name = newname;
                break;
            case 3:
                // edgecode: value must be 0-4
                if (index < 0 || index > 4) {
                    throw new IndexOutOfRangeException(nameof(index));
                }
                agEdgeCodes[index].Name = newname;
                break;
            case 4:
                // direction: value must be 0-8
                if (index < 0 || index > 8) {
                    throw new IndexOutOfRangeException(nameof(index));
                }
                agEgoDir[index].Name = newname;
                break;
            case 5:
                // video: value must be 0-4
                if (index < 0 || index > 4) {
                    throw new IndexOutOfRangeException(nameof(index));
                }
                agVideoMode[index].Name = newname;
                break;
            case 6:
                // computer: value must be 0-8
                if (index < 0 || index > 8) {
                    throw new IndexOutOfRangeException(nameof(index));
                }
                agCompType[index].Name = newname;
                break;
            case 7:
                // color: value must be 0-15
                if (index < 0 || index > 15) {
                    throw new IndexOutOfRangeException(nameof(index));
                }
                agResColor[index].Name = newname;
                break;
            case 8:
                // only 0 (ego)
                if (index != 0) {
                    throw new IndexOutOfRangeException(nameof(index));
                }
                agResObj[index].Name = newname;
                break;
            case 9:
                // only 0 (input prompt)
                if (index != 0) {
                    throw new IndexOutOfRangeException(nameof(index));
                }
                agResStr[index].Name = newname;
                break;
            default:
                throw new IndexOutOfRangeException(nameof(type));
            }
        }

        /// <summary>
        /// This method checks all reserved defines to confirm they are all valid. Any
        /// invalid define names are replaced with their defaults.
        /// </summary>
        /// <returns>true if all defines are OK, false if one or  more are invalid.</returns>
        public static bool ValidateResDefs() {
            // assume OK
            bool retval = true;
            int i;

            for (i = 0; i < agResVar.Length; i++) {
                if (ValidateName(agResVar[i], true) != DefineNameCheck.OK) {
                    agResVar[i].Name = agResVar[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agResFlag.Length; i++) {
                if (ValidateName(agResFlag[i], true) != DefineNameCheck.OK) {
                    agResFlag[i].Name = agResFlag[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agEdgeCodes.Length; i++) {
                if (ValidateName(agEdgeCodes[i], true) != DefineNameCheck.OK) {
                    agEdgeCodes[i].Name = agEdgeCodes[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agEgoDir.Length; i++) {
                if (ValidateName(agEgoDir[i], true) != DefineNameCheck.OK) {
                    agEgoDir[i].Name = agEgoDir[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agVideoMode.Length; i++) {
                if (ValidateName(agVideoMode[i], true) != DefineNameCheck.OK) {
                    agVideoMode[i].Name = agVideoMode[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agCompType.Length; i++) {
                if (ValidateName(agCompType[i], true) != DefineNameCheck.OK) {
                    agCompType[i].Name = agCompType[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agResColor.Length; i++) {
                if (ValidateName(agResColor[i], true) != DefineNameCheck.OK) {
                    agResColor[i].Name = agResColor[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agResObj.Length; i++) {
                if (ValidateName(agResObj[i], true) != DefineNameCheck.OK) {
                    agResObj[i].Name = agResObj[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agResStr.Length; i++) {
                if (ValidateName(agResStr[i], true) != DefineNameCheck.OK) {
                    agResStr[i].Name = agResStr[i].Default;
                    retval = false;
                }
            }
            return retval;
        }

        /// <summary>
        /// This method determines if the specified define name is valid or not.
        /// </summary>
        /// <param name="TestDef"></param>
        /// <param name="Reserved">true if testing a reserved define name</param>
        /// <returns>OK if name is valid, error type if it is not.</returns>
        internal static DefineNameCheck ValidateName(TDefine TestDef, bool Reserved = false) {
            int i;
            TDefine[] tmpDefines;

            // if already at default, just exit
            if (TestDef.Name == TestDef.Default) {
                return DefineNameCheck.OK;
            }
            // if no name,
            if (TestDef.Name.Length == 0) {
                return DefineNameCheck.Empty;
            }
            // name can't be numeric
            if (TestDef.Name.IsNumeric()) {
                return Numeric;
            }
            // check against regular commands
            for (i = 0; i < ActionCount; i++) {
                if (TestDef.Name == ActionCommands[i].Name) {
                    return ActionCommand;
                }
            }
            // check against test commands
            for (i = 0; i < TestCount; i++) {
                if (TestDef.Name == TestCommands[i].Name) {
                    return TestCommand;
                }
            }
            // check against compiler keywords
            if (
                TestDef.Name == "if" || TestDef.Name == "else" || TestDef.Name == "goto") {
                return KeyWord;
            }
            // if the name starts with any of these letters
            // (OK for sierra syntax)
            if (!agSierraSyntax) {
                if ("vfmoiswc".Any(TestDef.Name.ToLower().StartsWith)) {
                    if (TestDef.Name.Right(TestDef.Name.Length - 1).IsNumeric()) {
                      // can't have a name that's a valid marker
                        return ArgMarker;
                    }
                }
            }
            // check against reserved names if this game is using them,
            // OR if testing the list of reserved names
            if (Reserved || compGame.agIncludeReserved) {
                // reserved variables
                tmpDefines = ReservedDefines(Var);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name ==tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ReservedVar;
                        }
                    }
                }
                // reserved flags
                tmpDefines = ReservedDefines(Flag);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name == tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ReservedFlag;
                        }
                    }
                }
                // reserved numbers
                tmpDefines = ReservedDefines(Num);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name == tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ReservedNum;
                        }
                    }
                }
                // reserved objects
                tmpDefines = ReservedDefines(SObj);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name == tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ReservedObj;
                        }
                    }
                }
                // reserved strings
                tmpDefines = ReservedDefines(Str);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name == tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ReservedStr;
                        }
                    }
                }
            }
            // check name against improper character list
            if ((INVALID_DEFNAME_CHARS).Any(TestDef.Name.Contains)) {
                // bad
                return BadChar;
            }
            if (TestDef.Name.Any(ch => ch > 127 || ch < 32)) {
                // bad
                return BadChar;
            }
            // sierra syntax allows '?' 
            if (("'?").Any(TestDef.Name.Contains)) {
                if (!agSierraSyntax) {
                    return BadChar;
                }
            }
            // sierra syntax allows / for anything but first char
            if (("/").Any(TestDef.Name.Contains)) {
                if (!agSierraSyntax || i == 1) {
                    return BadChar;
                }
            }
            // must be OK!
            return DefineNameCheck.OK;
        }

        /// <summary>
        /// This method determines if the specified define name is valid, including a check
        /// against this game's current global defines.
        /// </summary>
        /// <param name="CheckDef"></param>
        /// <returns>OK if define is valid, error value if not.</returns>
        internal static DefineNameCheck ValidateNameGlobal(TDefine CheckDef) {
            int i;
            DefineNameCheck tmpResult;

            // check name against non-globals first
            tmpResult = ValidateName(CheckDef);
            if (tmpResult != DefineNameCheck.OK) {
                return tmpResult;
            }
            // check against current globals
            if (compGame is not null && compGame.agIncludeGlobals) {
                for (i = 0; i < compGame.GlobalDefines.Count; i++) {
                    if (CheckDef.Name == compGame.GlobalDefines[i].Name)
                        return DefineNameCheck.Global;
                }
            }
            // check against ingame reserved defines:
            if (compGame is not null && compGame.agIncludeReserved) {
                for (i = 0; i < compGame.agResGameDef.Length; i++) {
                    if (CheckDef.Name == compGame.agResGameDef[i].Name)
                        //invobj count is number; rest are msgstrings
                        return i == 3 ? ReservedNum : ReservedMsg;
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
        internal static DefineNameCheck ValidateDefName(string CheckName) {
            TDefine CheckDef = new() {
                Name = CheckName
            };
            return ValidateNameGlobal(CheckDef);
        }

        /// <summary>
        /// This method determines if the specified define has a valid value. It also
        /// sets the 
        /// </summary>
        /// <param name="TestDefine"></param>
        /// <returns></returns>
        internal static DefineValueCheck ValidateDefValue(ref TDefine TestDefine) {
            if (TestDefine.Value.Length == 0) {
                return DefineValueCheck.Empty;
            }
            // values must be an AGI argument marker (variable/flag/etc), string, or a number

            // if NOT a number:
            if (!int.TryParse(TestDefine.Value, out int intVal)) {
                char ch = TestDefine.Value[0];
                if ("vfmoiswc".Contains(ch)) {
                    string strVal = TestDefine.Value[1..];
                    if (int.TryParse(strVal, out intVal)) {
                        if (intVal < 0 || intVal > 255) {
                            return OutofBounds;
                        }
                        // determine type
                        switch (TestDefine.Value[0]) {
                        case 'f':
                            TestDefine.Type = Flag;
                            break;
                        case 'v':
                            TestDefine.Type = Var;
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
                        // check defined globals
                        for (int i = 0; i < compGame.GlobalDefines.Count; i++) {
                            if (compGame.GlobalDefines[i].Value == TestDefine.Value)
                                return DefineValueCheck.Global;
                        }
                        // check if Value is already assigned
                        switch (TestDefine.Type) {
                        case Flag:
                            if (compGame.agIncludeReserved) {
                                if (intVal <= 15)
                                    return Reserved;
                                if (intVal == 20) {
                                    switch (compGame.agIntVersion) {
                                    case "3.002.098" or "3.002.102" or "3.002.107" or "3.002.149":
                                        return Reserved;
                                    }
                                }
                            }
                            break;
                        case Var:
                            if (compGame.agIncludeReserved) {
                                if (intVal <= 26)
                                    return Reserved;
                            }
                            break;
                        case Msg:
                            break;
                        case SObj:
                            if (compGame.agIncludeReserved) {
                                // can't be ego
                                if (intVal == 0)
                                    return Reserved;
                            }
                            break;
                        case InvItem:
                            break;
                        case Str:
                            if (intVal > 23 || (intVal > 11 &&
                              (compGame.agIntVersion == "2.089" ||
                              compGame.agIntVersion == "2.272" ||
                              compGame.agIntVersion == "3.002149"))) {
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
            else {
                // numeric
                // unsigned byte (0-255) or signed byte (-128 to 127) are OK
                if (intVal > -128 && intVal < 256) {
                    TestDefine.Type = Num;
                    return DefineValueCheck.OK;
                }
                else {
                    return OutofBounds;
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
        /// Errors and warnings are added as they are encountered. If 
        /// successful, the method returns true. If not successful, false
        /// is returned.
        /// </summary>
        /// <param name="SourceLogic"></param>
        /// <returns></returns>
        internal static bool CompileLogic(Logic SourceLogic) {
            // if a major error occurs, return false
            // line is adjusted because
            // editor rows(lines) start at '1', but the compiler starts at line '0'
            bool blnCompiled;
            List<string> stlSource;
            compLogic = SourceLogic;

            if (compGame.IncludeGlobals && compGame.GlobalDefines.IsChanged) {
                try {
                    compGame.GlobalDefines.LoadGlobalDefines(compGame.agResDir + "globals.txt");
                }
                catch (Exception ex) {
                    // if no file or empty file, then continue, with no global entries
                    if (ex.HResult == WINAGI_ERR + 524 || ex.HResult == WINAGI_ERR + 605) {
                    }
                    else {
                        throw;
                    }
                }
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
            stlSource = SourceLogic.SourceText.SplitLines();
            bytLogComp = SourceLogic.Number;
            strLogCompID = SourceLogic.ID;
            // setup error info
            blnCriticalError = false;
            blnMinorError = false;
            intCtlCount = 0;
            intFileCount = 0;
            // add includes
            if (!AddIncludes(stlSource)) {
                return false;
            }
            // remove any blank lines from end
            while (stlInput[^1].Length == 0 && stlInput.Count > 0) {
                stlInput.RemoveAt(stlInput.Count - 1);
            }
            // if nothing to compile, return major error
            if (stlInput.Count == 0) {
                AddError(4159, true);
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
            // TODO: why not cast as AGIResource? the data object is only thing accessed
            tmpLogRes = new Logic {
                Data = []
            };
            // write a place holder 2-byte (word) value for offset to msg section start
            tmpLogRes.WriteWord(0, 0);
            // main agi compiler
            blnCompiled = CompileAGI();
            if (!blnCompiled) {
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
            if (!blnMinorError) {
                // no errors, save compiled data
                SourceLogic.Data = tmpLogRes.Data;
                SourceLogic.CompiledCRC = SourceLogic.CRC;
                compGame.WriteGameSetting("Logic" + (SourceLogic.Number).ToString(), "CRC32", "0x" + SourceLogic.CRC.ToString("x8"), "Logics");
                compGame.WriteGameSetting("Logic" + (SourceLogic.Number).ToString(), "CompCRC32", "0x" + SourceLogic.CompiledCRC.ToString("x8"), "Logics");
            }
            // done with the temp resource
            tmpLogRes.Unload();

            // if minor errors, report them as a compile fail
            if (blnMinorError) {
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
            if (blnSetIDs) {
                return;
            }
            StringList resIDlist = [
                "[ Resource ID Defines for " + game.agGameID,
                "[",
                "[ WinAGI generated code required for IncludeResourceIDs support - ",
                "[ do not modify the contents of this file with the code editor.",
                ""];
            strLogID = new string[256];
            strPicID = new string[256];
            strSndID = new string[256];
            strViewID = new string[256];
            Array.Fill(strLogID, "");
            Array.Fill(strPicID, "");
            Array.Fill(strSndID, "");
            Array.Fill(strViewID, "");
            resIDlist.Add("[ Logics");
            foreach (Logic tmpLog in game.agLogs) {
                strLogID[tmpLog.Number] = tmpLog.ID;
                resIDlist.Add("#define " + tmpLog.ID.PadRight(17) + tmpLog.Number.ToString().PadLeft(5));
            }
            resIDlist.Add("");
            resIDlist.Add("[ Pictures");
            foreach (Picture tmpPic in game.agPics) {
                strPicID[tmpPic.Number] = tmpPic.ID;
                resIDlist.Add("#define " + tmpPic.ID.PadRight(17) + tmpPic.Number.ToString().PadLeft(5));
            }
            resIDlist.Add("");
            resIDlist.Add("[ Sounds");
            foreach (Sound tmpSnd in game.agSnds) {
                strSndID[tmpSnd.Number] = tmpSnd.ID;
                resIDlist.Add("#define " + tmpSnd.ID.PadRight(17) + tmpSnd.Number.ToString().PadLeft(5));
            }
            resIDlist.Add("");
            resIDlist.Add("[ Views");
            foreach (View tmpView in game.agViews) {
                strViewID[tmpView.Number] = tmpView.ID;
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
            blnSetIDs = true;
        }

        /// <summary>
        /// This method updates the reserved defines file for the specified game.
        /// </summary>
        /// <param name="game"></param>
        internal static void SetReserved(AGIGame game, bool force) {
            if (!game.IncludeReserved) {
                return;
            }
            // only need to update if file is missing, OR if any of the defines
            // have changed, OR if game properties have changed
            if (!File.Exists(game.agResDir + "reserved.txt") || force) {
                StringList resList = [
                "[ Reserved Defines",
                "[",
                "[ WinAGI generated code required for IncludeReserved support - ",
                "[ do not modify the contents of this file with the code editor.",
                "",
                "[ Reserved Variables"];
                for (int i = 0; i < agResVar.Length; i++) {
                    resList.Add("#define " + agResVar[i].Name.PadRight(17) + agResVar[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Reserved Flags");
                for (int i = 0; i < agResFlag.Length; i++) {
                    resList.Add("#define " + agResFlag[i].Name.PadRight(17) + agResFlag[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Edge Codes");
                for (int i = 0; i < agEdgeCodes.Length; i++) {
                    resList.Add("#define " + agEdgeCodes[i].Name.PadRight(17) + agEdgeCodes[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Object Direction");
                for (int i = 0; i < agEgoDir.Length; i++) {
                    resList.Add("#define " + agEgoDir[i].Name.PadRight(17) + agEgoDir[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Video Modes");
                for (int i = 0; i < agVideoMode.Length; i++) {
                    resList.Add("#define " + agVideoMode[i].Name.PadRight(17) + agVideoMode[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Computer Types");
                for (int i = 0; i < agCompType.Length; i++) {
                    resList.Add("#define " + agCompType[i].Name.PadRight(17) + agCompType[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Colors");
                for (int i = 0; i < agResColor.Length; i++) {
                    resList.Add("#define " + agResColor[i].Name.PadRight(17) + agResColor[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Other Defines");
                resList.Add("#define " + agResObj[0].Name.PadRight(17) + agResObj[0].Value.PadLeft(5));
                resList.Add("#define " + agResStr[0].Name.PadRight(17) + agResStr[0].Value.PadLeft(5));
                resList.Add("");
                resList.Add("[ Game Properties");
                game.agResGameDef[0].Value = game.agGameID;
                game.agResGameDef[1].Value = game.agGameVersion;
                game.agResGameDef[2].Value = game.agGameAbout;
                bool loaded = game.InvObjects.Loaded;
                if (!loaded) {
                    game.InvObjects.Load();
                }
                game.agResGameDef[3].Value = game.InvObjects.Count.ToString();
                if (!loaded) {
                    game.InvObjects.Unload();
                }
                for (int i = 0; i < game.agResGameDef.Length; i++) {
                    resList.Add("#define " + game.agResGameDef[i].Name.PadRight(17) + game.agResGameDef[i].Value.PadLeft(5));
                }
                // save defines file
                try {
                    using FileStream fsList = new FileStream(game.agResDir + "reserved.txt", FileMode.Create);
                    using StreamWriter swList = new StreamWriter(fsList);
                    foreach (string line in resList) {
                        swList.WriteLine(line);
                    }
                }
                catch {
                    // ignore errors for now
                }
            }
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
        internal static bool ConvertArgument(ref string strArgIn, ArgType ArgType) {
            bool nullval = false;
            return ConvertArgument(ref strArgIn, ArgType, ref nullval);
        }

        /// <summary>
        /// This method converts an unkonwn token read from the input into the
        /// appropriate argument token. If the token can validly represent a 
        /// number or a variable, pass the input variable varOrnum as true; if
        /// a valid argument is found, varOrnum is updated to indicate which
        /// type was found. If the conversion fails, the method returns false.
        /// </summary>
        /// <param name="checktoken"></param>
        /// <param name="argtype"></param>
        /// <param name="varOrnum"></param>
        /// <returns>true if the token is valid (checktoken is changed to the correct
        /// token, varOrnum adjusted accordingly); false if the token is invalid</returns>
        internal static bool ConvertArgument(ref string checktoken, ArgType argtype, ref bool varOrnum) {
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
                if (argtype == VocWrd) {
                    if (checktoken[0] == '\"') {
                        //error
                        AddError(6001, false);
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
                case Num:
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
                    if (checktoken[0] == '&' && checktoken.Length > 1) {
                        int ptrVal = PointerValue(checktoken[1..]);
                        if (ptrVal != -1) {
                            checktoken = ptrVal.ToString();
                            // arg is number, not a variable
                            varOrnum = false;
                            return true;
                        }
                    }
                    break;

                case Var:
                    if (checktoken[0] == 'v') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case Flag:
                    if (checktoken[0] == 'f') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case Ctrl:
                    if (checktoken[0] == 'c') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case SObj:
                    if (checktoken[0] == 'o') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case Str:
                    //if first char matches
                    if (checktoken[0] == 's') {
                        //if this arg returns a valid Value
                        if (int.TryParse(checktoken[1..], out _)) {
                            //ok
                            return true;
                        }
                    }
                    break;
                case Word:
                    // NOTE: this is NOT vocab word; this is word arg type
                    // (used in command word.to.string)
                    if (checktoken[0] == 'w') {
                        if (int.TryParse(checktoken[1..], out _)) {
                            return true;
                        }
                    }
                    break;
                case Msg:
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
                case InvItem:
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
                case VocWrd:
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
                        if (argtype == Num) {
                            // argument is numeric
                            varOrnum = false;
                        }
                        return true;
                    }
                    // special case - looking for number, but var also OK
                    if (varOrnum && tdDefines[i].Type == Var) {
                        checktoken = tdDefines[i].Value;
                        return true;
                    }
                    // special case - message or item can be a string in quotes
                    if (argtype == Msg || argtype == InvItem) {
                        if (tdDefines[i].Type == DefStr) {
                            checktoken = tdDefines[i].Value;
                            return true;
                        }
                    }
                    // special case - vocab words are numbers or strings
                    if (argtype == VocWrd && (tdDefines[i].Type == Num || tdDefines[i].Type == DefStr)) {
                        checktoken = tdDefines[i].Value;
                        return true;
                    }
                    // the define value is not valid
                    return false;
                }
            }

            // if not sierra syntax check global defines, ResIDs and reserved
            if (!agSierraSyntax) {
                for (i = 0; i < compGame.GlobalDefines.Count; i++) {
                    if (checktoken == compGame.GlobalDefines[i].Name) {
                        if (compGame.GlobalDefines[i].Type == argtype) {
                            checktoken = compGame.GlobalDefines[i].Value;
                            if (argtype == Num) {
                                // argument is numeric
                                varOrnum = false;
                            }
                            // ok
                            return true;
                        }
                        // special case - looking for number, but var also OK
                        if (varOrnum && compGame.GlobalDefines[i].Type == Var) {
                            checktoken = compGame.GlobalDefines[i].Value;
                            return true;
                        }
                        // special case - message, item can be a string in quotes
                        if (argtype == Msg || argtype == InvItem) {
                            if (compGame.GlobalDefines[i].Type == DefStr) {
                                checktoken = compGame.GlobalDefines[i].Value;
                                return true;
                            }
                        }
                        // special case - vocab words are numbers or strings
                        if (argtype == VocWrd && (compGame.GlobalDefines[i].Type == Num || compGame.GlobalDefines[i].Type == DefStr)) {
                            checktoken = compGame.GlobalDefines[i].Value;
                            return true;
                        }
                        // the define value is not valid
                        return false;
                    }
                }
                // check numbers against list of resource IDs (can only be
                // numeric)
                if (argtype == Num) {
                    for (i = 0; i < 256; i++) {
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
                if (compGame.agIncludeReserved) {
                    switch (argtype) {
                    case Num:
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
                    case Var:
                        for (i = 0; i <= 26; i++) {
                            if (checktoken == agResVar[i].Name) {
                                checktoken = agResVar[i].Value;
                                return true;
                            }
                        }
                        break;
                    case Flag:
                        for (i = 0; i <= 17; i++) {
                            if (checktoken == agResFlag[i].Name) {
                                checktoken = agResFlag[i].Value;
                                return true;
                            }
                        }
                        break;
                    case Msg:
                        for (i = 1; i <= 2; i++) {
                            if (checktoken == compGame.agResGameDef[i].Name) {
                                checktoken = compGame.agResGameDef[i].Value;
                                return true;
                            }
                        }
                        break;
                    case SObj:
                        if (checktoken == agResObj[0].Name) {
                            checktoken = agResObj[0].Value;
                            return true;
                        }
                        break;
                    case Str:
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
                    switch (compGame.GlobalDefines[i].Value[0]) {
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
            if (compGame.agIncludeReserved) {
                for (int i = 0; i <= 26; i++) {
                    if (strArgIn == agResVar[i].Name) {
                        return i;
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
            }
            // if not found or error, return false
            return -1;
        }

        /// <summary>
        /// Reports errors back to calling thread. Minor errors don't cancel
        /// the compile method, but major (critical) errors do.
        /// </summary>
        /// <param name="ErrorNum"></param>
        /// <param name="ErrorText"></param>
        static void AddError(int ErrorNum, string ErrorText, bool critical) {
            TWinAGIEventInfo errInfo = new() {
                Type = EventType.LogicCompileError,
                Line = errorLine,
                ID = ErrorNum.ToString(),
                Text = ErrorText,
                ResType = AGIResType.Logic,
                ResNum = bytLogComp,
                Module = errorModule.Length > 0? Path.GetFileName(errorModule) : compLogic.ID,
                Filename = errorModule,
            };
            AGIGame.OnCompileLogicStatus(errInfo);
            if (critical) {
                blnCriticalError = true;
            }
            else {
                blnMinorError = true;
            }
        }

        static void AddError(int ErrorNum, bool critical) {
            AddError(ErrorNum, LoadResString(ErrorNum), critical);
        }

        /// <summary>
        /// This method extracts an argument byte code value from an argument token. For 
        /// example, it converts 'v##' into just '##'.
        /// </summary>
        /// <param name="argtoken"></param>
        /// <returns>The argument number if successful. -1 if unsuccessful.</returns>
        internal static int VariableValue(string argtoken) {
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
        /// This method passes through the input logic and replaces all include statements
        /// with the contents of the specified include file.
        /// </summary>
        /// <param name="stlLogicText"></param>
        /// <returns>true if include files added successfully. false if unable to 
        /// add include files.</returns>
        internal static bool AddIncludes(List<string> stlLogicText) {
            // include file lines are given a marker to identify them as belonging to
            // an include file
            string strLineText;
            bool includesID = false, includesReserved = false, includesGlobals = false;
            // begin with empty array of source lines
            stlInput = [];
            for (lngLine = 0; lngLine < stlLogicText.Count; lngLine++) {
                // cache error line
                errorLine = lngLine.ToString();
                if (agSierraSyntax) {
                    // get next line, convert tabs to spaces  but don't
                    // trim whitespace from start of line
                    strLineText = stlLogicText[lngLine].Replace('\t', ' ');
                }
                else {
                    // get next line, minus tabs and spaces
                    strLineText = stlLogicText[lngLine].Replace('\t', ' ').TrimStart();
                }
                if (strLineText.Left(2) == INCLUDE_MARK) {
                    // check for any instances of the marker, since these will
                    // interfere with include line handling ! SHOULD NEVER
                    // HAPPEN, but just in case
                    AddError(4069, true);
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
                case 2:
                    // resourceids.txt - skip this include file
                    includesID = true;
                    stlInput.Add("");
                    break;
                case 3:
                    // reserved.txt - skip this include file
                    includesReserved = true;
                    stlInput.Add("");
                    break;
                case 4:
                    // globals.txt - skip this include file
                    includesGlobals = true;
                    stlInput.Add("");
                    break;
                default: // -1 = error
                    return false;
                }
            }
            // confirm auto-includes were added
            if (compGame.IncludeIDs && !includesID) {
                AddError(4167, LoadResString(4167).Replace(ARG1, "resourceids.txt"), true);
                return false;
            }
            if (compGame.IncludeReserved && !includesReserved) {
                AddError(4167, LoadResString(4167).Replace(ARG1, "reserved.txt"), true);
                return false;
            }
            if (compGame.IncludeGlobals && !includesGlobals) {
                AddError(4167, LoadResString(4167).Replace(ARG1, "globals.txt"), true);
                return false;
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
        internal static int CheckInclude(string strLineText) {
            List<string> IncludeLines;
            string strIncludeFilename, strIncludeText;
            int intIncludeNum, CurIncludeLine;
            int retval, i;
            if (strLineText.Left(8) != "#include" && (strLineText.Left(8) != "%include" || !agSierraSyntax)) {
                // not an include line
                return 0;
            }
            // check for a missing filename
            if (strLineText.Trim().Length == 8) {
                AddError(4060, true);
                return -1;
            }
            // proper format requires a space after include mark
            if (strLineText[8] != ' ') {
                // generate critical error
                AddError(4103, true);
                return -1;
            }
            // build include filename
            strIncludeFilename = strLineText[9..].Trim();
            //check for a missing filename
            if (strIncludeFilename.Length == 0) {
                AddError(4060, true);
                return -1;
            }
            // check for single quote
            if (strIncludeFilename == "\"") {
                AddError(4060, true);
                return -1;
            }
            // check for correct quotes used 
            if (strIncludeFilename[0] != QUOTECHAR || strIncludeFilename[^1] != QUOTECHAR) {
                switch (ErrorLevel) {
                case High:
                    AddError(4059, true);
                    return -1;
                case Medium or Low:
                    AddWarning(5028, LoadResString(5028).Replace(ARG1, strIncludeFilename));
                    break;
                }
            }
            // strip off quotes
            if (strIncludeFilename[0] == QUOTECHAR) {
                strIncludeFilename = strIncludeFilename[1..];
            }
            if (strIncludeFilename[^1] == QUOTECHAR) {
                strIncludeFilename = strIncludeFilename[..^1];
            }
            strIncludeFilename = strIncludeFilename.Trim();
            if (strIncludeFilename.Length == 0) {
                AddError(4060, true);
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
                AddError(4050, LoadResString(4055).Replace(ARG1, strIncludeFilename), true);
                return -1;
            }
            // should never happen, but...
            if (strIncludeFilename == compLogic.SourceFile) {
                // !! don't allow this - it will hang up the compiler
                AddError(4166, true);
                return -1;
            }
            // now verify file exists
            if (!File.Exists(strIncludeFilename)) {
                AddError(4050, LoadResString(4050).Replace(ARG1, strIncludeFilename), true);
                return -1;
            }
            if (compGame.IncludeIDs && strIncludeFilename == compGame.agResDir + "resourceids.txt") {
                // skip it- IDs handled separately
                return 2;
            }
            if (compGame.IncludeReserved && strIncludeFilename == compGame.agResDir + "reserved.txt") {
                // skip it- reserved defines handled separately
                return 3;
            }
            if (compGame.IncludeGlobals && strIncludeFilename == compGame.agResDir + "globals.txt") {
                // skip it- globals handled separately
                return 4;
            }
            // check all loaded includes
            for (i = 0; i < intFileCount; i++) {
                if (strIncludeFilename == strIncludeFile[i]) {
                    // if the include file has already been added, don't add it again (to prevent include recursion!)
                    AddWarning(5117, LoadResString(5117).Replace(ARG1, strIncludeFilename));
                    return 1;
                }
            }
            // now open the include file, and get the text
            try {
                strIncludeText = compLogic.CodePage.GetString(File.ReadAllBytes(strIncludeFilename));
            }
            catch (Exception) {
                AddError(4055, LoadResString(4055).Replace(ARG1, strIncludeFilename), true);
                return -1;
            }
            IncludeLines = strIncludeText.SplitLines();
            if (IncludeLines.Count > 0) {
                // save file name to allow for error checking
                intIncludeNum = intFileCount++;
                Array.Resize(ref strIncludeFile, intFileCount);
                strIncludeFile[intIncludeNum] = strIncludeFilename;
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
                    if (strIncludeText.Length > 1 && strIncludeText[..2] == INCLUDE_MARK) {
                        errorLine = CurIncludeLine.ToString();
                        AddError(4069, true);
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
        internal static string ArgTypeName(ArgType ArgType) {
            switch (ArgType) {
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
            if (ErrorLevel == Low) {
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
        internal static void CheckResVarUse(byte ArgNum, byte ArgVal) {
            if (ErrorLevel == Low) {
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
        /// <param name="argtype"></param>
        /// <param name="ArgPos"></param>
        /// <returns>If successful, the function returns the Value of the argument<br />
        /// if unsuccessful, the function returns a negative value:<br />
        ///  -1 = invalid conversion<br />
        ///  -2 = ')' encountered<br />
        ///  -3 = ',' encountered
        /// </returns>
        internal static int GetNextArg(ArgType argtype, int ArgPos) {
            // used when not looking for a number/variable combo)
            bool nullval = false;
            return GetNextArg(argtype, ArgPos, ref nullval);
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
        /// <returns>If successful, the function returns the Value of the argument<br />
        /// if unsuccessful, the function returns a negative value:<br />
        ///  -1 = invalid conversion<br />
        ///  -2 = ')' encountered<br />
        ///  -3 = ',' encountered
        ///</returns>
        internal static int GetNextArg(ArgType argtype, int argpos, ref bool varOrnum) {
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
                    // backup so comma/bracket can be retrieved
                    lngPos--;
                }
                else {
                    // invalid conversion
                    AddError(4063, LoadResString(4063).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, ArgTypeName(argtype)).Replace(ARG3, strArg), false);
                    retval = -1;
                }
                return retval;
            }

            switch (argtype) {
            case Num:
                // check for negative number
                if (strArg.Val() < 0) {
                    //valid negative numbers are -1 to -128
                    if (strArg.Val() < -128) {
                        AddError(4157, false);
                        // use dummy value to continue
                        strArg = "1";
                    }
                    else {
                        //convert it to 2s-compliment unsigned value by adding it to 256
                        strArg = (256 + strArg.Val()).ToString();
                        switch (ErrorLevel) {
                        case High or Medium:
                            AddWarning(5098);
                            break;
                        }
                    }
                }
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    AddError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()), false);
                }
                break;
            case Var or Flag:
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    AddError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()), false);
                }
                break;
            case Ctrl:
                // controllers should be  0 - 49
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4136, LoadResString(4136).Replace(ARG1, (argpos + 1).ToString()), false);
                        break;
                    case Medium or Low:
                        AddError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()), false);
                        break;
                    }
                    return -1;
                }
                else {
                    if (lngArg > 49) {
                        switch (ErrorLevel) {
                        case High:
                            AddError(4136, LoadResString(4136).Replace(ARG1, (argpos + 1).ToString()), false);
                            break;
                        case Medium:
                            AddWarning(5060);
                            break;
                        }
                    }
                }
                break;
            case SObj:
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    AddError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()), false);
                }
                // check against max screen object Value
                if (lngArg > compGame.InvObjects.MaxScreenObjects) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4119, LoadResString(4119).Replace(ARG1, (compGame.InvObjects.MaxScreenObjects).ToString()), false);
                        break;
                    case Medium:
                        AddWarning(5006, LoadResString(5006).Replace(ARG1, compGame.InvObjects.MaxScreenObjects.ToString()));
                        break;
                    }
                }
                break;
            case Str:
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    switch (ErrorLevel) {
                    case High:
                        // for version 2.089, 2.272, and 3.002149 only 12 strings
                        switch (compGame.agIntVersion) {
                        case "2.089" or "2.272" or "3.002149":
                            AddError(4079, LoadResString(4079).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, "11"), false);
                            break;
                        default:
                            AddError(4079, LoadResString(4079).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, "23"), false);
                            break;
                        }
                        break;
                    case Medium or Low:
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
                else {
                    // if outside expected bounds (strings should be limited to 0-23)
                    if ((lngArg > 23) || (lngArg > 11 && (compGame.agIntVersion == "2.089" || compGame.agIntVersion == "2.272" || compGame.agIntVersion == "3.002149"))) {
                        switch (ErrorLevel) {
                        case High:
                            // for version 2.089, 2.272, and 3.002149 only 12 strings
                            switch (compGame.agIntVersion) {
                            case "2.089" or "2.272" or "3.002149":
                                AddError(4079, LoadResString(4079).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, "11"), false);
                                break;
                            default:
                                AddError(4079, LoadResString(4079).Replace(ARG1, (argpos + 1).ToString()).Replace(ARG2, "23"), false);
                                break;
                            }
                            break;
                        case Medium:
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
            case Word:
                // word type is NOT words from word.tok
                // word args should be limited to 0-9)
                lngArg = VariableValue(strArg);
                if (lngArg == -1) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4090, LoadResString(4090).Replace(ARG1, (argpos + 1).ToString()), false);
                        break;
                    case Medium or Low:
                        AddError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()), false);
                        break;
                    }
                }
                else {
                    if (lngArg > 9) {
                        switch (ErrorLevel) {
                        case High:
                            AddError(4090, LoadResString(4090).Replace(ARG1, (argpos + 1).ToString()), false);
                            break;
                        case Medium:
                            AddWarning(5008);
                            break;
                        }
                    }
                }
                break;
            case Msg:
                // arg token is either m## or "msg" [or ## for sierra syntax]
                if (agSierraSyntax) {
                    lngArg = VariableValue(strArg);
                }
                else {
                    switch (strArg[0]) {
                    case 'm':
                        lngArg = VariableValue(strArg);
                        if (lngArg == -1) {
                            AddError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()), false);
                        }
                        break;
                    case '\"':
                        // concatenate, if applicable
                        strArg = ConcatArg(strArg);
                        // strip off quotes
                        strArg = strArg.Mid(1, strArg.Length - 2);
                        // convert to msg number
                        lngArg = MessageNum(strArg);
                        // if unallowed characters found, error was raised; exit
                        if (lngArg == -1) {
                            return -1;
                        }
                        // if too many messages
                        if (lngArg == 0) {
                            AddError(4092, false);
                            return -1;
                        }
                        break;
                    }
                }
                // m0 is not allowed
                if (lngArg == 0) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4107, false);
                        // make this a null msg
                        blnMsg[lngArg] = true;
                        strMsg[lngArg] = "";
                        return -1;
                    case Medium:
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
                    case High:
                        AddError(4113, LoadResString(4113).Replace(ARG1, lngArg.ToString()), false);
                        //make this a null msg
                        blnMsg[lngArg] = true;
                        strMsg[lngArg] = "";
                        break;
                    case Medium:
                        AddWarning(5090, LoadResString(5090).Replace(ARG1, lngArg.ToString()));
                        //make this a null msg
                        blnMsg[lngArg] = true;
                        strMsg[lngArg] = "";
                        break;
                    }
                }
                break;
            case InvItem:
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
                                errorLine = lngLine.ToString();
                            }
                            AddError(4075, LoadResString(4075).Replace(ARG1, (argpos + 1).ToString()), false);
                            return -1;
                        }
                        // check for valid, but non-unique object (if passed by
                        // text string, it can't be one that is not unique)
                        if (lngArg != -1 && !compGame.InvObjects[(byte)lngArg].Unique) {
                            switch (ErrorLevel) {
                            case High:
                                AddError(4036, LoadResString(4036).Replace(ARG1, (argpos + 1).ToString()), false);
                                break;
                            case Medium:
                                AddWarning(5003, LoadResString(5003).Replace(ARG1, (argpos + 1).ToString()));
                                break;
                            }
                        }
                        break;
                    }
                }
                // check for valid match
                if (lngArg == -1) {
                    AddError(4066, LoadResString(4066).Replace(ARG1, (argpos + 1).ToString()), false);
                }
                else {
                    if (lngArg >= compGame.InvObjects.Count) {
                        switch (ErrorLevel) {
                        case High:
                            AddError(4112, LoadResString(4112).Replace(ARG1, (argpos + 1).ToString()), false);
                            break;
                        case Medium:
                            AddWarning(5005, LoadResString(5005).Replace(ARG1, (argpos + 1).ToString()));
                            break;
                        }
                    }
                    else {
                        // check for question mark
                        if (compGame.InvObjects[(byte)lngArg].ItemName == "?") {
                            switch (ErrorLevel) {
                            case High:
                                AddError(4111, LoadResString(4111).Replace(ARG1, (argpos + 1).ToString()), false);
                                return -1;
                            case Medium:
                                AddWarning(5004);
                                break;
                            }
                        }
                    }
                }
                break;
            case VocWrd:
                // words can be ## or "word" [or no quotes, dollar sign for spaces for
                // Sierra syntax]
                if (strArg.IsNumeric() && !agSierraSyntax) {
                    if (!int.TryParse(strArg, out lngArg)) {
                        // use value of  as placdholder and raise error
                        lngArg = 1;
                        AddError(4162, LoadResString(4162).Replace(ARG1, strArg), false);
                    }
                    if (lngArg > 65535 || lngArg < 0) {
                        // use value of  as placdholder and raise error
                        lngArg = 1;
                        AddError(4162, LoadResString(4162).Replace(ARG1, strArg), false);
                    }
                    else {
                        // valldate the group
                        if (!compGame.agVocabWords.GroupExists(lngArg)) {
                            switch (ErrorLevel) {
                            case High:
                                AddError(4114, LoadResString(4114).Replace(ARG1, strArg), false);
                                break;
                            case Medium:
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
                            AddError(4114, LoadResString(4114).Replace(ARG1, strArg), false);
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
                            // DO NOT convert to lower case ; if user entered
                            // upper case words let the error handler catch it
                            strArg = strArg[1..^1];
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
                            case High or Medium:
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
                            case High:
                                AddError(4114, LoadResString(4114).Replace(ARG1, strArg), false);
                                break;
                            case Medium:
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
                    case High:
                        AddError(4035, LoadResString(4035).Replace(ARG1, strArg), false);
                        return -1;
                    case Medium:
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
        internal static void IncrementLine() {
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
                lngPos = -1;
                if (lngLine >= stlInput.Count) {
                    lngLine = -1;
                    return;
                }
                // check for include lines
                if (stlInput[lngLine].Left(2) == INCLUDE_MARK) {
                    lngIncludeOffset++;
                    // set module
                    int mod = int.Parse(stlInput[lngLine][2..(stlInput[lngLine].IndexOf(':'))]);
                    errorModule = strIncludeFile[mod];
                    // set error line
                    errorLine = stlInput[lngLine][(stlInput[lngLine].IndexOf(':') + 1)..stlInput[lngLine].IndexOf('#')];
                    // get the line without include tag
                    strCurrentLine = stlInput[lngLine][(stlInput[lngLine].IndexOf('#') + 1)..];
                }
                else {
                    errorModule = "";
                    errorLine = (lngLine - lngIncludeOffset).ToString();
                    // get the next line
                    strCurrentLine = stlInput[lngLine];
                }
                // skip over blank lines
            } while (strCurrentLine.Length == 0);
        }

        /// <summary>
        /// Gets the next token and compares it to the specified value. The 
        /// method returns true if the token matches. If it does not match,
        /// the token is left in the input stream.
        /// </summary>
        /// <param name="tokenval"></param>
        /// <param name="nonewline"></param>
        /// <returns>true if next token matches the specified value, false if not.</returns>
        internal static bool CheckToken(string tokenval, bool nonewline = false) {
            string testToken = NextToken(nonewline);
            if (testToken == tokenval) {
                return true;
            }
            // no match; backup so this token stays in the stream
            lngPos -= testToken.Length;
            return false;
        }

        /// <summary>
        /// Gets the next character and compares it to the specified value. The
        /// method returns true if the character matches. The next character is 
        /// returned to the input stream if the character does not match.
        /// </summary>
        /// <param name="charval"></param>
        /// <param name="blnNoNewLine"></param>
        /// <returns></returns>
        internal static bool CheckChar(char charval, bool blnNoNewLine = false) {
            char testChar;

            testChar = NextChar(blnNoNewLine);
            if (testChar != charval) {
                // no match; need to back up
                // unless nothing was found (meaning end of line or input)
                if (testChar != 0) {
                    lngPos--;
                }
            }
            return testChar == charval;
        }

        /// <summary>
        /// Gets the next token from the source code input stream but does not advance
        /// the line or position, i.e. the token remains in the input stream.
        /// </summary>
        /// <param name="blnNoNewLine"></param>
        /// <returns></returns>
        internal static string PeekToken(bool blnNoNewLine = false) {
            // save compiler state
            int tmpPos = lngPos;
            int tmpLine = lngLine;
            int tmpInclOffset = lngIncludeOffset;
            string tmpModule = errorModule;
            string tmpCurLine = strCurrentLine;
            //int tmpErrLine = int.Parse(errorLine);
            string tmpErrLine = errorLine;
            // get next token
            string peekcmd = NextToken(blnNoNewLine);
            // restore compiler state
            lngPos = tmpPos;
            lngLine = tmpLine;
            lngIncludeOffset = tmpInclOffset;
            errorModule = tmpModule;
            strCurrentLine = tmpCurLine;
            //errorLine = tmpErrLine.ToString();
            errorLine = tmpErrLine;
            // return the token
            return peekcmd;
        }

        /// <summary>
        /// Gets the next non-space character from the source code input stream. If 
        /// the NoNewLine flag is true, the method will not look past the current
        /// line.
        /// </summary>
        /// <param name="blnNoNewLine"></param>
        /// <returns>The next character, or a null character if end of input reached.</returns>
        internal static char NextChar(bool blnNoNewLine = false) {
            // If the NoNewLine flag is passed, the function will not look past
            // current line for next character. If no character on current line,
            // lngPos is set to end of current line, and an empty string is
            // returned.

            char nextchar = '\0';

            // if already at end of input (lngLine=-1)
            if (lngLine == -1) {
                return (char)0;
            }
            do {
                lngPos++;
                if (lngPos >= strCurrentLine.Length) {
                    if (blnNoNewLine) {
                        // move pointer back
                        lngPos--;
                        // return empty char
                        return (char)0;
                    }
                    IncrementLine();
                    if (lngLine == -1) {
                        // exit with no character
                        return (char)0;
                    }
                    lngPos++;
                }
                nextchar = strCurrentLine[lngPos];
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
        /// <param name="blnNoNewLine"></param>
        /// <returns>The next token, or null string if end of input reached.</returns>
        internal static string NextToken(bool blnNoNewLine = false) {
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

            // find next non-blank character
            string retval = NextChar(blnNoNewLine).ToString();
            //if at end of input,
            if (lngLine == -1) {
                // return empty string
                return "";
            }
            // if no character returned
            if (retval == "" || retval == "\0") {
                // return empty string
                return "";
            }
            // single character separators:
            if ("(),:;[\\]^`{}~".Any(retval.Contains)) {
                return retval;
            }
            // check for other characters
            switch (retval[0]) {
            case '\'' or '?':
                // sierra syntax doesn't treat these as a single character token
                if (!agSierraSyntax) {
                    // fan AGI does
                    return retval;
                }
                break;
            case '=':
                // special case; "=", "==", "=<" and "=>" returned as separate
                // tokens (also "=@" for sierra syntax)
                if (lngPos + 1 < strCurrentLine.Length) {
                    switch (strCurrentLine[lngPos + 1]) {
                    case '<' or '>':
                        lngPos++;
                        // swap so we get ">=" and "<=" instead of "=>" and "=<"
                        retval = strCurrentLine[lngPos].ToString() + retval;
                        break;
                    case '=':
                        lngPos++;
                        retval = "==";
                        break;
                    case '@':
                        if (agSierraSyntax) {
                            lngPos++;
                            retval = "=@";
                        }
                        break;
                    }
                }
                return retval;
            case '\"':
                // special case; quote means start of a string
                blnInQuotes = true;
                break;
            case '+':
                // special case; "+", "++" and "+=" returned as separate tokens
                if (lngPos + 1 < strCurrentLine.Length) {
                    if (strCurrentLine[lngPos + 1] == '+') {
                        lngPos++;
                        retval = "++";
                    }
                    else if (strCurrentLine[lngPos + 1] == '=') {
                        lngPos++;
                        retval = "+=";
                    }
                }
                return retval;
            case '-':
                // special case; "-", "--" and "-=" returned as separate tokens
                // also check for negative numbers ("-##")
                if (lngPos + 1 < strCurrentLine.Length) {
                    if (strCurrentLine[lngPos + 1] == '-') {
                        lngPos++;
                        retval = "--";
                    }
                    else if (strCurrentLine[lngPos + 1] == '=') {
                        lngPos++;
                        retval = "-=";
                    }
                    else if (strCurrentLine[lngPos + 1] >= 48 && strCurrentLine[lngPos + 1] <= 57) {
                        // return a negative number
                        while (lngPos + 1 < strCurrentLine.Length) {
                            char aChar = strCurrentLine[lngPos + 1];
                            if (aChar < 48 || aChar > 57) {
                                // anything other than a digit (0-9)
                                break;
                            }
                            else {
                                // add the digit
                                retval += aChar;
                                lngPos++;
                            }
                        }
                    }
                }
                return retval;
            case '!':
                // special case; "!" and "!=" returned as separate tokens
                if (lngPos + 1 < strCurrentLine.Length) {
                    if (strCurrentLine[lngPos + 1] == '=') {
                        lngPos++;
                        retval = "!=";
                    }
                }
                return retval;
            case '<':
                // special case; "<", "<=" returned as separate tokens
                if (lngPos + 1 < strCurrentLine.Length) {
                    if (strCurrentLine[lngPos + 1] == '=') {
                        lngPos++;
                        retval = "<=";
                    }
                }
                return retval;
            case '>':
                // special case; ">", ">=" returned as separate tokens
                if (lngPos + 1 < strCurrentLine.Length) {
                    if (strCurrentLine[lngPos + 1] == '=') {
                        lngPos++;
                        retval = ">=";
                    }
                }
                return retval;
            case '*':
                // special case; "*" and "*=" returned as separate tokens;
                if (lngPos + 1 < strCurrentLine.Length) {
                    if (strCurrentLine[lngPos + 1] == '=') {
                        lngPos++;
                        retval = "*=";
                    }
                    // since block tokens are no longer supported, check for them
                    // in order to provide a meaningful error message
                    else if (strCurrentLine[lngPos + 1] == '/') {
                        lngPos++;
                        retval = "*/";
                    }
                }
                return retval;
            case '/':
                // special case; "/" , "//" and "/=" returned as separate tokens
                if (lngPos + 1 < strCurrentLine.Length) {
                    if (strCurrentLine[lngPos + 1] == '=') {
                        lngPos++;
                        retval = "/=";
                    }
                    else if (strCurrentLine[lngPos + 1] == '/') {
                        lngPos++;
                        retval = "//";
                    }
                    // since block tokens are no longer supported, check for them
                    // in order to provide a meaningful error message
                    else if (strCurrentLine[lngPos + 1] == '*') {
                        lngPos++;
                        retval = "/*";
                    }
                }
                return retval;
            case '|':
                // special case; "|" and "||" returned as separate tokens
                if (lngPos + 1 < strCurrentLine.Length) {
                    if (strCurrentLine[lngPos + 1] == '|') {
                        lngPos++;
                        retval = "||";
                    }
                }
                return retval;
            case '&':
                // special case; "&&" returned as separate token
                if (lngPos + 1 < strCurrentLine.Length) {
                    if (strCurrentLine[lngPos + 1] == '&') {
                        lngPos++;
                        retval = "&&";
                    }
                }
                break;
            case '@':
                // special case; "@=" returned as separate token
                if (agSierraSyntax) {
                    if (lngPos + 1 < strCurrentLine.Length) {
                        if (strCurrentLine[lngPos + 1] == '=') {
                            lngPos++;
                            retval = "@=";
                            return retval;
                        }
                    }
                }
                // '@' isn't a single-char token
                break;
            }
            if (!blnInQuotes) {
                // continue adding characters until element separator or EOL is reached
                while (lngPos + 1 < strCurrentLine.Length) {
                    char nextChar = strCurrentLine[lngPos + 1];
                    //  space, !"&'() * +,-/:;<=>?[\] ^`{|}~
                    // always marks end of token
                    if (" !\"&()*+,-:;<=>[\\]^`{|}~".Any(nextChar.ToString().Contains)) {
                        // end of token text found
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
                        // add character
                        retval += nextChar;
                        lngPos++;
                    }
                }
            }
            else {
                // a text string - 
                // if past end of line (which could only happen if a line contains
                // a single double quote on it)
                if (lngPos + 1 >= strCurrentLine.Length) {
                    // return the single quote
                    return retval;
                }
                // add characters until another TRUE quote is found
                do {
                    char nextchar = strCurrentLine[lngPos + 1];
                    //increment position
                    lngPos++;
                    // if last char was a slash, next char is just added as-is
                    if (blnSlash) {
                        //always reset  the slash
                        blnSlash = false;
                    }
                    else {
                        //check for slash or quote mark
                        if (nextchar == '"') {
                            // a quote marks end of string
                            blnInQuotes = false;
                        }
                        else if (nextchar == '\\') {
                            blnSlash = true;
                        }
                    }
                    retval += nextchar;
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
            // return the token
            return retval;
        }

        /// <summary>
        /// Checks for an end-of-line marker that matches the current syntax.
        /// </summary>
        internal static void CheckForEOL() {
            // FAN syntax requires an eol mark;
            // sierra syntax does not, but it's recommended
            string newLine, oldLine;

            // cache error line, in case error drops down one or more lines
            oldLine = errorLine;
            if (!CheckChar(';')) {
                // temporarily set to line where error really is
                newLine = errorLine;
                errorLine = oldLine;
                if (agSierraSyntax) {
                    AddWarning(5111);
                }
                else {
                    AddError(4007, false);
                }
                // restore errline
                errorLine = newLine;
            }
        }

        /// <summary>
        /// This method concatenates strings, i.e. text surrounded by quotes.
        /// It assumes the input string has just been read into the compiler
        /// and checks if there are additional elements of this string to add
        /// to it.
        /// </summary>
        /// <param name="strText"></param>
        /// <returns>The complete string, with lngPos and lngLine updated
        /// accordingly. If there is nothing to concatenate, the original
        /// string is returned.</returns>
        internal static string ConcatArg(string strText) {
            // input is a potential string, the first character has been
            // confirmed to be a quote mark
            string strTextContinue;
            int lngLastPos, lngLastLine;
            string strLastLine;
            int lngSlashCount, lngQuotesOK;
            string retval;

            // verify at least two characters
            // start/end quotes is true?
            if (strText.Length < 2) {
                AddError(4081, false);
                return "\"\"";
            }
            retval = strText;
            // confirm starting string has ending quote
            if (strText[^1] != '\"') {
                // missing end quote - add it
                retval += "\"";
                switch (ErrorLevel) {
                case High:
                    AddError(4080, false);
                    break;
                case Medium:
                    AddWarning(5002);
                    break;
                }
                // note which line had quotes added, in case it results
                // in an error caused by a missing end ')' or whatever
                // the next required element is
                lngQuoteAdded = lngLine;
            }
            // save current position info
            lngLastPos = lngPos;
            lngLastLine = lngLine;
            strLastLine = strCurrentLine;
            if (lngLastPos == strLastLine.Length - 1) {
                strTextContinue = NextToken();
                // add strings until concatenation is complete
                while (strTextContinue.Length > 0 && strTextContinue[0] == QUOTECHAR) {
                    // if a continuation string is found, we need to reset
                    // the quote checker
                    lngQuotesOK = 0;
                    if (strTextContinue.Length == 1) {
                        // single quote - treat as end of concatenation;
                        // main compiler will catch it as a syntax error
                        break;
                    }
                    //check for end quote
                    if (strTextContinue[^1] != QUOTECHAR) {
                        // bad end quote (set end quote marker, overriding error
                        // that might happen on a previous line)
                        lngQuotesOK = 2;
                    }
                    else {
                        // just because it ends in a quote doesn't mean it's good;
                        // it might be an embedded quote - check for an odd number
                        // of slashes immediately preceding this quote
                        lngSlashCount = 0;
                        do {
                            if (retval[^(lngSlashCount + 2)] == '\\') {
                                lngSlashCount++;
                            }
                            else {
                                break;
                            }
                        } while (retval.Length - (lngSlashCount + 1) >= 0);
                        if (lngSlashCount % 2 == 1) {
                            // it's embedded, and doesn't count - 
                            // bad end quote (set end quote marker, overriding error
                            // that might happen on a previous line)
                            lngQuotesOK = 2;
                        }
                    }
                    // if end quote is missing, deal with it
                    if (lngQuotesOK > 0) {
                        // note which line had quotes added, in case it results
                        // in an error caused by a missing end ')' or whatever
                        // the next required element is
                        lngQuoteAdded = lngLine;
                        switch (ErrorLevel) {
                        case High:
                            AddError(4080, false);
                            return "";
                        case Medium:
                            strTextContinue += QUOTECHAR;
                            AddWarning(5002);
                            break;
                        case Low:
                            strTextContinue += QUOTECHAR;
                            break;
                        }
                    }
                    // strip off ending quote of current string
                    retval = retval[..^1];
                    // add continuation string without its starting quote
                    retval += strTextContinue[1..];
                    // save current position info
                    lngLastPos = lngPos;
                    lngLastLine = lngLine;
                    strLastLine = strCurrentLine;
                    // get next token
                    strTextContinue = NextToken();
                }
                // after end of string found, move back to correct position
                lngPos = lngLastPos;
                lngLine = lngLastLine;
                errorLine = lngLastLine.ToString();
                strCurrentLine = strLastLine;
            }
            return retval;
        }

        /// <summary>
        /// Strips comments from the input text and trims off leading
        /// and trailing spaces.
        /// </summary>
        /// <returns>true if all comments removed without error, otherwise
        /// false.</returns>
        internal static bool RemoveComments() {
            // fanAGI syntax:
            //      // - rest of line is ignored (not for Sierra syntax)
            //      [ - rest of line is ignored
            //
            // Sierra syntax - only [ 
            int lngPos;
            bool blnInQuotes = false, blnSlash = false;
            int intROLIgnore;

            ResetCompiler();
            do {
                intROLIgnore = -1;
                lngPos = 0;
                if (!agSierraSyntax) {
                    //??? TODO: why only non-Sierra syntax?
                    blnInQuotes = false;
                }
                if (strCurrentLine.Length != 0) {
                    while (lngPos < strCurrentLine.Length) {
                        if (!blnInQuotes) {
                            // check for comment characters at this position
                            if ((strCurrentLine.Mid(lngPos, 2) == CMT2_TOKEN && !agSierraSyntax) || strCurrentLine[lngPos] == CMT1_TOKEN[0]) {
                                intROLIgnore = lngPos;
                                break;
                            }
                            // slash codes never occur outside quotes
                            blnSlash = false;
                            // if this character is a quote mark, it starts a string
                            blnInQuotes = strCurrentLine[lngPos] == QUOTECHAR;
                        }
                        else {
                            // if last character was a slash, ignore this character
                            // because it's part of a slash code
                            if (blnSlash) {
                                // always reset  the slash
                                blnSlash = false;
                            }
                            else {
                                switch (strCurrentLine[lngPos]) {
                                case QUOTECHAR:
                                    blnInQuotes = false;
                                    break;
                                case '\\':
                                    blnSlash = true;
                                    break;
                                }
                            }
                        }
                        lngPos++;
                    }
                    if (intROLIgnore >= 0) {
                        strCurrentLine = strCurrentLine[..intROLIgnore];
                    }
                }
                if (!agSierraSyntax) {
                    strCurrentLine = strCurrentLine.Trim();
                }
                // update the line with comments removed
                ReplaceLine(lngLine, strCurrentLine);
                IncrementLine();
            } while (lngLine != -1);
            return true;
        }

        /// <summary>
        /// Replaces the specified line in the input stream with new text, while
        /// preserving include header information.
        /// </summary>
        /// <param name="LineNum"></param>
        /// <param name="strNewLine"></param>
        internal static void ReplaceLine(int LineNum, string strNewLine) {
            string strInclude;

            if (stlInput[LineNum].Left(2) == INCLUDE_MARK) {
                strInclude = stlInput[LineNum][..(stlInput[LineNum].IndexOf('#') + 1)];
            }
            else {
                strInclude = "";
            }
            // replace the line
            stlInput[LineNum] = strInclude + strNewLine;
        }

        /// <summary>
        /// Resets the compiler so it points to beginning of input stream.
        /// </summary>
        internal static void ResetCompiler() {
            lngIncludeOffset = 0;
            blnCriticalError = false;
            lngQuoteAdded = -1;
            // set line pointer to -2 so first call to IncrementLine gets first line
            lngLine = -2;
            IncrementLine();
        }

        /// <summary>
        /// This method scans the entire input stream and extracts all define 
        /// statements.
        /// </summary>
        /// <returns>true if defines are extracted without error, otherwise false.</returns>
        internal static bool ReadDefines() {
            // for normal syntax, #define and #message are only valid preprocessors
            // for sierra syntax, valid preprocessors include:
            //   #define
            //   #message
            //   #action
            //   #test
            //   #var
            //   #flag
            //   #view
            //   #object
            //   #tokens
            int i;
            TDefine tdNewDefine = new() { Name = "", Value = "" };
            DefineNameCheck checkName;
            DefineValueCheck checkValue;
            int lngErrNum;
            string strError = "", strToken;
            DefineType lngDefType;

            ResetCompiler();
            lngDefineCount = 0;
            do {
                strToken = NextToken(true);
                // with Sierra syntax, blank lines return a null token
                // check for preprocessor mark '#' (or '%' in Sierra syntax only)
                if (strToken.Length > 0 && strToken[0] == '#' || (strToken[0] == '%' && agSierraSyntax)) {
                    strToken = strToken[1..];
                    if (agSierraSyntax) {
                        switch (strToken) {
                        case "tokens":
                            // deprecated; just ignore (don't allow different WORDS.TOK file)
                            lngDefType = DefineType.Ignore;
                            // clear line so compiler will ignore it
                            ReplaceLine(lngLine, "");
                            break;
                        case "test":
                            // allow renaming/redefining test commands
                            lngDefType = DefineType.TestCmd;
                            break;
                        case "action":
                            lngDefType = DefineType.ActionCmd;
                            break;
                        case "flag":
                            // same as 'define name f##', but with a number
                            lngDefType = DefineType.Flag;
                            break;
                        case "var":
                            // same as 'define name v##, but with a number
                            lngDefType = DefineType.Variable;
                            break;
                        case "object":
                            // same as 'define name o##' or 'define name 'i##', but with a number
                            lngDefType = DefineType.Object;
                            break;
                        case "define":
                            lngDefType = DefineType.Default;
                            break;
                        case "message":
                            // skip - these are handled by ReadMsgs, after defines
                            lngDefType = DefineType.Ignore;
                            break;
                        case "view":
                            // same as 'define name ##', but value must be valid view number
                            lngDefType = DefineType.View;
                            break;
                        default:
                            // invalid - ignore it; main compiler will catch it
                            // (there are some sierra source files that have
                            // multi-line string assignments that look like
                            // invalid preprocessor)
                            lngDefType = DefineType.Ignore;
                            break;
                        }
                    }
                    else {
                        // in fanAGI syntax, only 'define', 'message' allowed
                        switch (strToken) {
                        case "define":
                            lngDefType = DefineType.Default;
                            break;
                        case "message":
                            // skip; these are handled in ReadMsgs, afer defines
                            lngDefType = DefineType.Ignore;
                            break;
                        default:
                            // invalid preprocessor token; ignore it; main
                            // compiler will handle it
                            lngDefType = DefineType.Ignore;
                            break;
                        }
                    }
                    if (lngDefType >= 0) {
                        //tdNewDefine.Type = Num;
                        tdNewDefine.Name = NextToken(true);
                        // for test/action commands, the value may include
                        // args; WinAGI ignores all that; only the number matters
                        if (lngDefType == DefineType.TestCmd || lngDefType == DefineType.ActionCmd) {
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
                        // nothing else allowed on the line (comments have already been removed)
                        if (NextToken(true).Length != 0) {
                            AddError(4163, false);
                            // skip to end
                            lngPos = strCurrentLine.Length;
                        }
                        // check for redefines
                        // #define avar  v99
                        // #define bvar  avar  --> same as #define bvar  v99
                        for (i = 0; i < lngDefineCount; i++) {
                            if (tdNewDefine.Value == tdDefines[i].Name) {
                                tdNewDefine.Value = tdDefines[i].Value;
                                tdNewDefine.Type = tdDefines[i].Type;
                                // TODO: add warning for redefines
                                break;
                            }
                        }
                        // validate define name
                        checkName = ValidateDefName(tdNewDefine.Name);
                        if (agSierraSyntax) {
                            // ignore overrides 
                            if (checkName > BadChar) {
                                checkName = DefineNameCheck.OK;
                            }
                            // set type based on token used
                            switch (lngDefType) {
                            case DefineType.Flag:
                                tdNewDefine.Type = Flag;
                                break;
                            case DefineType.Variable:
                                // variable only
                                tdNewDefine.Type = Var;
                                break;
                            case DefineType.Object:
                                // generiic obj (sobj or invitem) only
                                tdNewDefine.Type = Obj;
                                break;
                            case DefineType.View:
                                // view only
                                if (compGame is not null && byte.TryParse(tdNewDefine.Value, out _) && compGame.agViews.Contains(byte.Parse(tdNewDefine.Value))) {
                                    tdNewDefine.Type = ArgType.View;
                                }
                                else {
                                    // default to number
                                    tdNewDefine.Type = Num;
                                    if (compGame is not null) {
                                        switch (ErrorLevel) {
                                        case High:
                                            AddError(6003, false);
                                            break;
                                        case Medium:
                                            AddWarning(5110);
                                            break;
                                        }
                                    }
                                }
                                break;
                            case DefineType.TestCmd:
                                // rename test command
                                if (byte.TryParse(tdNewDefine.Value, out byte tmp) && tmp <= agNumTestCmds) {
                                    agTestCmds[byte.Parse(tdNewDefine.Value)].Name = tdNewDefine.Name;
                                }
                                else {
                                    AddError(6004, LoadResString(6004).Replace(ARG1, tdNewDefine.Value), false);
                                }
                                break;
                            case DefineType.ActionCmd:
                                // rename action command
                                if (byte.TryParse(tdNewDefine.Value, out tmp) && tmp <= agNumCmds) {
                                    agCmds[byte.Parse(tdNewDefine.Value)].Name = tdNewDefine.Name;
                                }
                                else {
                                    AddError(6005, LoadResString(6005).Replace(ARG1, tdNewDefine.Value), false);
                                }
                                break;
                            }
                        }
                        else {
                            // override name errors (8-13) are only warnings if errorlevel is medium or low
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
                        }
                        // now check for errors
                        if (checkName != DefineNameCheck.OK) {
                            lngErrNum = 0;
                            switch (checkName) {
                            case DefineNameCheck.Empty:
                                lngErrNum = 4070;
                                strError = LoadResString(4070);
                                break;
                            case Numeric:
                                lngErrNum = 4072;
                                strError = LoadResString(4072);
                                break;
                            case ActionCommand:
                                if (agSierraSyntax) {
                                    // OK if defining a command name with '#define/#action/#test'
                                    switch (lngDefType) {
                                    case DefineType.Default:
                                        lngDefType = DefineType.Redefine;
                                        break;
                                    case DefineType.TestCmd or DefineType.ActionCmd:
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
                            case TestCommand:
                                if (agSierraSyntax) {
                                    // OK if defining a command name with #test/#action/#define
                                    switch (lngDefType) {
                                    case DefineType.Default:
                                        // mark it as a redefine
                                        lngDefType = DefineType.Redefine;
                                        break;
                                    case DefineType.TestCmd or DefineType.ActionCmd:
                                        // OK
                                        break;
                                    default:
                                        lngErrNum = 6002;
                                        strError = LoadResString(6002).Replace(ARG1, tdNewDefine.Value).Replace(ARG2, "test");
                                        break;
                                    }
                                }
                                else {
                                    // not allowed for fanAGI syntax
                                    lngErrNum = 4022;
                                    strError = LoadResString(4022).Replace(ARG1, tdNewDefine.Name);
                                }
                                break;
                            case KeyWord:
                                lngErrNum = 4013;
                                strError = LoadResString(4013).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ArgMarker:
                                lngErrNum = 4071;
                                strError = LoadResString(4071);
                                break;
                            case BadChar:
                                lngErrNum = 4067;
                                strError = LoadResString(4067);
                                break;
                            case DefineNameCheck.Global:
                                lngErrNum = 4019;
                                strError = LoadResString(4019).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ReservedVar:
                                lngErrNum = 4018;
                                strError = LoadResString(4018).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ReservedFlag:
                                lngErrNum = 4014;
                                strError = LoadResString(4014).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ReservedNum:
                                //name is reserved number constant
                                lngErrNum = 4016;
                                strError = LoadResString(4016).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ReservedObj:
                                lngErrNum = 4017;
                                strError = LoadResString(4015).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ReservedStr:
                                // name is reserved object constant
                                lngErrNum = 4017;
                                strError = LoadResString(4015).Replace(ARG1, tdNewDefine.Name);
                                break;
                            case ReservedMsg:
                                lngErrNum = 4015;
                                strError = LoadResString(4015).Replace(ARG1, tdNewDefine.Name);
                                break;
                            }
                            if (lngErrNum > 0) {
                                AddError(lngErrNum, strError, false);
                            }
                        }
                        // validate define value
                        lngErrNum = 0;
                        // type is already set for sierra syntax; value validation
                        // sets it for fanAGI
                        checkValue = ValidateDefValue(ref tdNewDefine);
                        // value errors 4-6 are only warnings if errorlevel is medium or low
                        switch (ErrorLevel) {
                        case Medium:
                            switch (checkValue) {
                            case NotAValue:
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
                                checkValue = DefineValueCheck.OK;
                                break;
                            case Reserved:
                                AddWarning(5032, LoadResString(5032).Replace(ARG1, tdNewDefine.Value));
                                checkValue = DefineValueCheck.OK;
                                break;
                            case DefineValueCheck.Global:
                                AddWarning(5031, LoadResString(5031).Replace(ARG1, tdNewDefine.Value));
                                checkValue = DefineValueCheck.OK;
                                break;
                            }
                            break;
                        case Low:
                            switch (checkValue) {
                            case NotAValue:
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
                                checkValue = DefineValueCheck.OK;
                                break;
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
                                lngErrNum = 4073;
                                strError = LoadResString(4073);
                                break;
                            case OutofBounds:
                                lngErrNum = 4042;
                                strError = LoadResString(4042);
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
                                    // only an error if errorlevel is high
                                    switch (ErrorLevel) {
                                    case High:
                                        lngErrNum = 4079;
                                        switch (compGame.agIntVersion) {
                                        case "2.089" or "2.272" or "3.002149":
                                            strError = LoadResString(4079).Replace(ARG1, "").Replace(ARG2, "11");
                                            break;
                                        default:
                                            strError = LoadResString(4079).Replace(ARG1, "").Replace(ARG2, "23");
                                            break;
                                        }
                                        break;
                                    case Medium or Low:
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
                                    break;
                                case Word:
                                    lngErrNum = 4090;
                                    strError = LoadResString(4090);
                                    break;
                                }
                                break;
                            case NotAValue:
                                // value is not a string, number, or arg value
                                lngErrNum = 4082;
                                strError = LoadResString(4082);
                                break;
                            case Reserved:
                                lngErrNum = 4041;
                                strError = LoadResString(4041).Replace(ARG1, tdNewDefine.Value);
                                break;
                            case DefineValueCheck.Global:
                                lngErrNum = 4040;
                                strError = LoadResString(4040).Replace(ARG1, tdNewDefine.Value);
                                break;
                            }
                        }
                        if (lngErrNum != 0) {
                            AddError(lngErrNum, strError, false);
                        }
                        // for Sierra syntax-specific tokens (not #defines)
                        if (lngDefType > 0) {
                            // numbers must be byte(0-255)
                            if (int.TryParse(tdNewDefine.Value, out int tmp)) {
                                if (tmp > 256 || tmp < 0) {
                                    AddError(6013, false);
                                }
                            }
                            else {
                                AddError(6013, false);
                            }
                        }
                        else {
                            // check all previous defines
                            for (i = 0; i < lngDefineCount; i++) {
                                if (tdNewDefine.Name == tdDefines[i].Name) {
                                    AddError(4012, LoadResString(4012).Replace(ARG1, tdDefines[i].Name), false);
                                }
                                if (tdNewDefine.Value == tdDefines[i].Value) {
                                    // numeric duplicates are OK
                                    if (!int.TryParse(tdNewDefine.Value, out _)) {
                                        switch (ErrorLevel) {
                                        case High:
                                            AddError(4023, LoadResString(4023).Replace(ARG1, tdDefines[i].Value).Replace(ARG2, tdDefines[i].Name), false);
                                            break;
                                        case Medium:
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
                                        AddError(4020, LoadResString(4020).Replace(ARG1, tdNewDefine.Name), false);
                                    }
                                }
                            }
                        }
                        // save this define
                        lngDefineCount++;
                        Array.Resize(ref tdDefines, lngDefineCount);
                        tdDefines[lngDefineCount - 1] = tdNewDefine;
                        // now set this line to empty so Compiler doesn"t try to read it
                        ReplaceLine(lngLine, "");
                    }
                }
                IncrementLine();
            } while (lngLine != -1);
            return true;
        }

        /// <summary>
        /// Extracts defined messages from the source code data stream.
        /// </summary>
        /// <returns>true if messages extracted without errors, otherwise false.</returns>
        internal static bool ReadMsgs() {
            // note that stripped message lines also strip out the include header string
            // this doesn't matter since they are only blank lines anyway only need
            // to include header info if error occurs, and errors never occur on
            // blank line
            bool blnDef = false;
            int intMsgNum, lngSlashCount, lngQuotesOK, lngMsgStart;
            string strToken;

            // reset message list
            for (intMsgNum = 0; intMsgNum < 256; intMsgNum++) {
                strMsg[intMsgNum] = "";
                blnMsg[intMsgNum] = false;
                intMsgWarn[intMsgNum] = 0;
            }
            ResetCompiler();
            do {
                strToken = NextToken(true);
                if (strToken == MSG_TOKEN || agSierraSyntax && strToken == "%message") {
                    // save starting line number (incase this msg uses multiple lines)
                    lngMsgStart = lngLine;
                    // get next token as message number (on this line if not Sierra syntax)
                    strToken = NextToken(!agSierraSyntax);
                    // this should be a msg number
                    if (!int.TryParse(strToken, out _)) {
                        // critical error because no way to know what number
                        // this is supposed to be
                        AddError(4077, true);
                        return false;
                    }
                    else {
                        // validate msgnum
                        intMsgNum = VariableValue(strToken);
                        if (intMsgNum <= 0) {
                            // critical error because no way to know what number
                            // this is supposed to be
                            AddError(4077, true);
                            return false;
                        }
                        // msg is already assigned
                        if (blnMsg[intMsgNum]) {
                            AddError(4094, LoadResString(4094).Replace(ARG1, (intMsgNum).ToString()), false);
                        }
                    }
                    // next token should be the message text
                    strToken = NextToken(false);
                    if (agSierraSyntax) {
                        // in Sierra syntax msgtext can only be a string, with quotes required
                        if (strToken.Length == 0) {
                            // could only happen if at end of logic
                            AddError(6006, false);
                            // use a placeholder to continue
                            strToken = "\" \"";
                        }
                        if (strToken[0] != '\"' || strToken[^1] != '\"' || strToken.Length == 2) {
                            AddError(6007, false);
                        }
                        if (!IsAGIString(strToken)) {
                            // try concatenation
                            strToken = ConcatSierraStr(strToken);
                            if (blnCriticalError) {
                                AddError(6008, true);
                                return false;
                            }
                        }
                    }
                    else {
                        // fanAGI syntax
                        if (!IsAGIString(strToken)) {
                            // maybe it's a define
                            // TODO: if converted, it should be confirmed to
                            // be a correctly formatted agi string
                            if (ConvertArgument(ref strToken, Msg)) {
                                // defined strings never get concatenated
                                blnDef = true;
                            }
                        }
                        // always reset the 'addquote' flag (this is the flag that
                        // notes if/where a line had an end quote added by the
                        // compiler; if this causes problems later in the
                        // compilation of this token, we can then mark this error
                        // as the culprit
                        lngQuoteAdded = -1;
                        // check msg for quotes (note ending quote has to be checked
                        // to make sure it's not an embedded quote)
                        // assume OK until we learn otherwise (0=OK; 1=bad start
                        // quote; 2=bad end quote; 3=bad both)
                        lngQuotesOK = 0;
                        if (strToken[0] != '"') {
                            // bad start quote
                            lngQuotesOK = 1;
                        }
                        else {
                            // rare but check for single quote
                            if (strToken.Length < 2) {
                                AddError(4081, false);
                                // add placeholder to continue
                                strToken = "\"\"";
                            }
                        }
                        // check for end quote
                        if (strToken[^1] != '"') {
                            //bad end quote
                            lngQuotesOK &= 2;
                        }
                        else {
                            // just because it ends in a quote doesn't mean it's good;
                            // it might be an embedded quote
                            // check for an odd number of slashes immediately preceding
                            // this quote
                            lngSlashCount = 0;
                            do {
                                if (strToken[^(lngSlashCount + 1)] == '\\') {
                                    lngSlashCount++;
                                }
                                else {
                                    break;
                                }
                            } while (strToken.Length - (lngSlashCount + 1) >= 0);
                            // if it is odd, then it's not a valid quote
                            if (lngSlashCount % 2 == 1) {
                                lngQuotesOK &= 2;
                            }
                        }
                        // if either (or both) quote is missing, deal with it
                        if (lngQuotesOK > 0) {
                            switch (ErrorLevel) {
                            case High:
                                AddError(4051, true);
                                return false;
                            case Medium:
                                AddWarning(5002);
                                break;
                            }
                            // note which line had quotes added, in case it results
                            // in an error caused by a missing end ')' or whatever
                            // the next required element is
                            lngQuoteAdded = lngLine;
                            // add missing quotes
                            if ((lngQuotesOK & 1) == 1) {
                                strToken = '\"' + strToken;
                            }
                            if ((lngQuotesOK & 2) == 2) {
                                strToken += '\"';
                            }
                        }
                        // concatenate, if necessary
                        if (!blnDef) {
                            strToken = ConcatArg(strToken);
                        }
                        // always reset blnDef for next msg
                        blnDef = false;
                    }
                    // nothing allowed after msg declaration
                    if (lngPos != strCurrentLine.Length - 1) {
                        char nextchar = NextChar(true);
                        if (nextchar != 0) {
                            if (agSierraSyntax) {
                                // ';' at end is OK
                                if (strToken[0] != ';' || NextChar(true) != 0) {
                                    AddError(4099, false);
                                    // ignore rest of line
                                    lngPos = strCurrentLine.Length;
                                }
                            }
                            else {
                                //error
                                AddError(4099, false);
                                // ignore rest of line
                                lngPos = strCurrentLine.Length;
                            }
                        }
                    }
                    // strip off quotes (we know that the string
                    // is properly enclosed by quotes because
                    // ConcatArg function validates they are there
                    // or adds them if they aren't [or raises an
                    // error, in which case it doesn't even matter])
                    strToken = strToken[1..^1];
                    strMsg[intMsgNum] = strToken;
                    ValidateMsgChars(strToken, intMsgNum);
                    blnMsg[intMsgNum] = true;
                    do {
                        // set the msg line (and any concatenated lines) to empty so
                        // compiler doesn't try to read it
                        ReplaceLine(lngMsgStart, "");
                        // increment the counter (to get multiple lines, if string is
                        // concatenated over more than one line)
                        lngMsgStart++;
                        // continue until back to current line
                    } while (lngMsgStart <= lngLine);
                }
                else {
                    if (strToken == "%message" && !agSierraSyntax) {
                        AddError(4061, LoadResString(4061).Replace(ARG1, strToken), false);
                    }
                }
                IncrementLine();
            } while (lngLine != -1);
            // done
            return true;
        }

        /// <summary>
        /// Sends the specified warning to the calling program as an event.
        /// </summary>
        /// <param name="WarningNum"></param>
        /// <param name="WarningText"></param>
        internal static void AddWarning(int WarningNum, string WarningText = "") {
            // (number, line and module only have meaning for logic warnings
            // other warnings generated during a game compile will use
            // same format, but use -- for warning number, line and module)

            //if no text passed, use the default resource string
            if (WarningText.Length == 0) {
                WarningText = LoadResString(WarningNum);
            }
            // only add if not ignoring
            if (!agNoCompWarn[WarningNum - 5000]) {
                TWinAGIEventInfo errInfo = new() {
                    Type = EventType.LogicCompileWarning,
                    ID = WarningNum.ToString(),
                    Text = WarningText,
                    Line = errorLine,
                    Module = errorModule.Length > 0 ? Path.GetFileName(errorModule) : compLogic.ID,
                    Filename = errorModule,
                    ResNum = bytLogComp,
                    ResType = AGIResType.Logic,
                };
                _ = AGIGame.OnCompileLogicStatus(errInfo);
            }
        }

        /// <summary>
        /// This method reads and validates the input for an 'if' command.
        /// </summary>
        /// <returns></returns>
        internal static bool CompileIf() {
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
            //      fn        ==> isset(fn)
            //      vn == m   ==> equaln(vn,m)
            //      etc
            //
            // valid special comparison expressions are ==, !=, <>, >, =<, <, >=
            // OR//ed tests must always be enclosed in parenthesis; AND//ed tests
            // must never be enclosed in parentheses (this ensures the compiled code
            // will be compatible with the AGI interpreter)
            //
            // any test token may have the negation operator (!) placed directly
            // in front of it
            string strTestCmd, strArg;
            byte bytTestCmd;
            byte[] bytArg = new byte[8];
            int lngArg;
            int[] lngWord;
            int intWordCount;
            int i;
            bool blnOrBlock = false;
            bool blnNeedNextCmd = true;
            int intNumTestCmds = 0;
            int intNumCmdsInBlock = 0;
            bool blnNOT;

            // 'if' starting bytecode
            tmpLogRes.WriteByte(0xFF);

            // next character should be "("
            if (!CheckChar('(')) {
                AddError(4002, false);
                // keep going - maybe the missing '(' is the only problem
            }

            // step through input, until final ')' is found:
            do {
                strTestCmd = NextToken();
                if (lngLine == -1) {
                    // nothing left, return critical error
                    AddError(4106, true);
                    return false;
                }
                if (blnNeedNextCmd) {
                    switch (strTestCmd) {
                    case "(":
                        if (blnOrBlock) {
                            AddError(4045, false);
                        }
                        // add 'or' block starting bytecode
                        tmpLogRes.WriteByte(0xFC);
                        blnOrBlock = true;
                        intNumCmdsInBlock = 0;
                        break;
                    case ")":
                        if (blnOrBlock) {
                            if (intNumCmdsInBlock == 0) {
                                // or block with no commands
                                switch (ErrorLevel) {
                                case High or Medium:
                                    AddWarning(5113);
                                    break;
                                }
                            }
                            else {
                                AddError(4056, false);
                                // done with if
                                tmpLogRes.WriteByte(0xFF);
                                return true;
                            }
                            // close the block
                            blnOrBlock = false;
                            blnNeedNextCmd = false;
                        }
                        else if (intNumTestCmds == 0) {
                            // if block with no commands
                            switch (ErrorLevel) {
                            case High:
                                AddError(4057, false);
                                break;
                            case Medium:
                                AddWarning(5114);
                                break;
                            }
                            // done with if
                            tmpLogRes.WriteByte(0xFF);
                            return true;
                        }
                        else {
                            // unexpected closer
                            AddError(4056, false);
                            // done with if
                            tmpLogRes.WriteByte(0xFF);
                            return true;
                        }
                        break;
                    default:
                        // check for NOT
                        blnNOT = (strTestCmd == NOT_TOKEN);
                        if (blnNOT) {
                            tmpLogRes.WriteByte(0xFD);
                            strTestCmd = NextToken();
                            //check for end of input
                            if (lngLine == -1) {
                                AddError(4106, LoadResString(4106), true);
                                return false;
                            }
                        }
                        bytTestCmd = CommandNum(true, strTestCmd);
                        if (bytTestCmd == 255) {
                            // no command found - check for special syntax
                            if (!CompileSpecialIf(strTestCmd, blnNOT)) {
                                // CompileSpecialIf sets the error -
                                // return true to continue
                                return true;
                            }
                        }
                        else {
                            tmpLogRes.WriteByte(bytTestCmd);
                            // next command should be "("
                            if (!CheckChar('(')) {
                                AddError(4048, false);
                            }
                            // check for return.false() token
                            if (bytTestCmd == 0) {
                                // warn user that it's not compatible with AGI Studio
                                switch (ErrorLevel) {
                                case High or Medium:
                                    AddWarning(5081);
                                    break;
                                }
                            }
                            if (bytTestCmd == 0xE) {
                                // process a said command
                                intWordCount = 0;
                                lngWord = [];
                                do {
                                    lngArg = GetNextArg(VocWrd, intWordCount);
                                    if (lngArg < 0) {
                                        // not a valid word
                                        switch (lngArg) {
                                        case -1:
                                            // error info already added
                                            break;
                                        default:
                                            // -2, -3
                                            AddError(4054, LoadResString(4054).Replace(ARG1, (intWordCount + 1).ToString()).Replace(ARG3, ArgTypeName(VocWrd)), false);
                                            break;
                                        }
                                        // use placeholder number, value doesn't matter
                                        lngArg = 1;
                                    }
                                    // if too many words
                                    if (intWordCount == 10) {
                                        AddError(4093, false);
                                        break;
                                    }
                                    else if (intWordCount < 10) {
                                        // add this word number
                                        // to array of word numbers
                                        Array.Resize(ref lngWord, intWordCount + 1);
                                        lngWord[intWordCount] = lngArg;
                                    }
                                    intWordCount++;
                                    // next character should be a comma, or close parenthesis
                                    strArg = NextChar().ToString();
                                    if (strArg.Length != 0) {
                                        bool exitDo = false;
                                        switch (strArg[0]) {
                                        case ')':
                                            // move pointer back one space so the ')' 
                                            // will be found at end of command
                                            lngPos--;
                                            exitDo = true;
                                            break;
                                        case ',':
                                            // expected; continue check for next word argument
                                            break;
                                        default:
                                            // missing comma or colse parenthesis
                                            AddError(4047, LoadResString(4047).Replace(ARG1, (intWordCount + 1).ToString()), false);
                                            // assume comma and continue
                                            lngPos -= strArg.Length;
                                            break;
                                        }
                                        if (exitDo) {
                                            break;
                                        }
                                    }
                                    else {
                                        // this should normally never happen, since changing the function
                                        // to allow splitting over multiple lines, unless this is the last
                                        // line of the logic (an EXTREMELY rare edge case)
                                        // check for added quotes; they are the problem
                                        if (lngQuoteAdded >= 0) {
                                            lngLine = lngQuoteAdded;
                                            errorLine = (lngLine - lngIncludeOffset).ToString();
                                        }
                                        AddError(4047, LoadResString(4047).Replace(ARG1, (intWordCount + 1).ToString()), false);
                                        return false;
                                    }
                                } while (true);
                                // reset the quotemark error flag after ')' is found
                                lngQuoteAdded = -1;
                                // add number of arguments
                                tmpLogRes.WriteByte((byte)intWordCount);
                                // add words
                                for (i = 0; i < intWordCount; i++) {
                                    tmpLogRes.WriteWord((ushort)lngWord[i]);
                                }
                            }
                            else {
                                // extract arguments
                                for (i = 0; i < TestCommands[bytTestCmd].ArgType.Length; i++) {
                                    // after first argument, verify comma separates arguments
                                    if (i > 0) {
                                        if (!CheckChar(',')) {
                                            AddError(4047, LoadResString(4047).Replace(ARG1, (i + 1).ToString()), false);
                                        }
                                    }
                                    // reset the quotemark error flag after comma is found
                                    lngQuoteAdded = -1;
                                    lngArg = GetNextArg(TestCommands[bytTestCmd].ArgType[i], i);
                                    if (lngArg >= 0) {
                                        bytArg[i] = (byte)lngArg;
                                    }
                                    else {
                                        if (lngArg == -1) {
                                            // invalid arg
                                            // error info already added
                                        }
                                        else {
                                            // -2, -3: missing arg
                                            AddError(4054, LoadResString(4054).Replace(ARG1, (i + 1).ToString()).Replace(ARG3, ArgTypeName(TestCommands[bytTestCmd].ArgType[i])).Replace(ARG2, TestCommands[bytTestCmd].Name), false);
                                        }
                                        // use a placeholder
                                        bytArg[i] = 0;
                                    }
                                    // add argument
                                    tmpLogRes.WriteByte(bytArg[i]);
                                }
                            }
                            // next character should be ")"
                            if (!CheckChar(')')) {
                                AddError(4160, false);
                            }
                            // reset the quotemark error flag
                            lngQuoteAdded = -1;
                            if (!ValidateIfArgs(bytTestCmd, ref bytArg)) {
                                // error assigned by called function
                                return false;
                            }
                        }
                        intNumTestCmds++;
                        if (blnOrBlock) {
                            intNumCmdsInBlock++;
                        }
                        blnNeedNextCmd = false;
                        break;
                    }
                }
                else {
                    // not awaiting a test command
                    switch (strTestCmd) {
                    case NOT_TOKEN:
                        // 'not' token is not allowed 
                        AddError(4097, false);
                        break;
                    case AND_TOKEN:
                        if (blnOrBlock) {
                            // 'and' not allowed if inside brackets
                            AddError(4037, false);
                        }
                        blnNeedNextCmd = true;
                        break;
                    case OR_TOKEN:
                        if (!blnOrBlock) {
                            // 'or' not allowed UNLESS inside brackets
                            AddError(4100, false);
                            // assume a valid test
                            intNumCmdsInBlock++;
                            // force orblock
                            blnOrBlock = false;
                        }
                        blnNeedNextCmd = true;
                        break;
                    case ")":
                        if (blnOrBlock) {
                            blnOrBlock = false;
                            tmpLogRes.WriteByte(0xFC);
                            if (intNumCmdsInBlock == 1) {
                                // or block with one command
                                switch (ErrorLevel) {
                                case High or Medium:
                                    AddWarning(5109);
                                    break;
                                }
                            }
                        }
                        else {
                            // end of if block found
                            tmpLogRes.WriteByte(0xFF);
                            return true;
                        }
                        break;
                    default:
                        AddError(blnOrBlock ? 4101 : 4038, false);
                        // assume it was ok
                        blnNeedNextCmd = true;
                        // 'and if it is a close bracket, assume
                        // the 'if' block is now closed
                        if (strTestCmd == "{") {
                            lngPos--;
                            // write ending if byte
                            tmpLogRes.WriteByte(0xFF);
                            return true;
                        }
                        break;
                    }
                }
                // never leave loop normally; error, end of input, or successful
                // compilation of test commands will all exit loop directly
            } while (true);
        }

        /// <summary>
        /// Concatenates string from multiple lines that are formatted in Sierra syntax.
        /// </summary>
        /// <param name="strText"></param>
        static string ConcatSierraStr(string strText) {
            // sierra syntax allows concatenation over multiple lines, with
            // ending quote only at end of last line
            bool blnInQuotes = true, blnSlash = false;
            char theChar;
            string retval = strText;
            // strText is first line, without closing quote, and goes to
            // end of current line, so first time through the loop the
            // line will automatically increment

            // add characters until another TRUE quote is found
            do {
                if (lngPos == strCurrentLine.Length) {
                    IncrementLine();
                    if (lngLine == -1) {
                        blnCriticalError = true;
                        return retval;
                    }
                }
                theChar = strCurrentLine[lngPos++];
                if (blnSlash) {
                    // next char is just added as-is; no checking it
                    // always reset  the slash
                    blnSlash = false;
                }
                else {
                    // regular char; check for slash or quote mark
                    switch (theChar) {
                    case '\"':
                        // quote mark - end of string
                        blnInQuotes = false;
                        break;
                    case '\\':
                        blnSlash = true;
                        break;
                    }
                }
                retval.Append(theChar);
                if (!blnInQuotes) {
                    return retval;
                }
            } while (true);
        }

        /// <summary>
        /// Returns the number of the specified label, or zero if a match
        /// is not found.
        /// </summary>
        /// <param name="LabelName"></param>
        /// <returns></returns>
        internal static int LabelNum(string LabelName) {
            //
            int i;
            for (i = 0; i < bytLabelCount; i++) {
                if (llLabel[i].Name == LabelName) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// This method checks the values passed as arguments to the specified action
        /// command and adds warnings or errors as needed.
        /// </summary>
        /// <param name="CmdNum"></param>
        /// <param name="ArgVal"></param>
        /// <returns></returns>
        internal static bool ValidateArgs(int CmdNum, ref byte[] ArgVal) {
            // for commands that can affect variable values, need to check against
            // reserved variables
            // for commands that can affect flags, need to check against reserved
            // flags
            // for other commands, check the passed arguments to see if values
            // are appropriate
            bool blnUnload = false, blnWarned = false;

            switch (CmdNum) {
            case 0:
                // return - expect no more commands
                endingCmd = 1;
                break;

            case 1 or 2 or (>= 4 and <= 8) or 10 or (>= 165 and <= 168):
                // increment, decrement, assignv, addn, addv, subn, subv
                // rindirect, mul.n, mul.v, div.n, div.v
                // check for reserved variables that should never be manipulated
                // (assume arg Value is zero to avoid tripping other checks)
                CheckResVarUse(ArgVal[0], 0);
                // for div.n(vA, B) only, check for divide-by-zero
                if (CmdNum == 167) {
                    if (ArgVal[1] == 0) {
                        switch (ErrorLevel) {
                        case High:
                            AddError(4149, false);
                            return false;
                        case Medium:
                            AddWarning(5030);
                            break;
                        }
                    }
                }
                break;
            case 3:
                // assignn
                CheckResVarUse(ArgVal[0], ArgVal[1]);
                break;
            case >= 12 and <= 14:
                // set, reset, toggle
                CheckResFlagUse(ArgVal[0]);
                break;
            case 18:
                // new.room(A)
                if (!compGame.agLogs.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4120, false);
                        return false;
                    case Medium:
                        AddWarning(5053);
                        break;
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
                if (!compGame.agLogs.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4121, LoadResString(4121).Replace(ARG1, ArgVal[0].ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5013);
                        break;
                    }
                }
                break;
            case 22:
                // call(A)
                if (ArgVal[0] == 0) {
                    // calling logic0 is a BAD idea
                    switch (ErrorLevel) {
                    case High:
                        AddError(4118, false);
                        return false;
                    case Medium:
                        AddWarning(5010);
                        break;
                    }
                }
                if (!compGame.agLogs.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4156, LoadResString(4156).Replace(ARG1, (ArgVal[0]).ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5076);
                        break;
                    }
                }
                if (ArgVal[0] == bytLogComp) {
                    // recursive calling is usually BAD
                    switch (ErrorLevel) {
                    case High:
                        AddError(4117, false);
                        return false;
                    case Medium:
                        AddWarning(5089);
                        break;
                    }
                }
                break;
            case 30:
                // load.view(A)
                if (!compGame.agViews.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4122, LoadResString(4122).Replace(ARG1, (ArgVal[0]).ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5015);
                        break;
                    }
                }
                break;
            case 32:
                // discard.view(A)
                if (!compGame.agViews.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4123, LoadResString(4123).Replace(ARG1, ArgVal[0].ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5024);
                        break;
                    }
                }
                break;
            case 37:
                // position(oA, X,Y)
                if (ArgVal[1] > 159 || ArgVal[2] > 167) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4128, false);
                        return false;
                    case Medium:
                        AddWarning(5023);
                        break;
                    }
                }
                break;
            case 39:
                // get.posn
                if (ArgVal[1] <= 26 || ArgVal[2] <= 26) {
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 41:
                // set.view(oA, B)
                if (!compGame.agViews.Contains(ArgVal[1])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4124, LoadResString(4124).Replace(ARG1, (ArgVal[1]).ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5037);
                        break;
                    }
                }
                break;
            case (>= 49 and <= 53) or 97 or 118:
                // last.cel, current.cel, current.loop,
                // current.view, number.of.loops, get.room.v
                // get.num
                if (ArgVal[1] <= 26) {
                    // variable arg is second and should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 54:
                // set.priority(oA, B)
                if (ArgVal[1] > 15) {
                    // invalid priority Value
                    switch (ErrorLevel) {
                    case High:
                        AddError(4125, false);
                        return false;
                    case Medium:
                        AddWarning(5050);
                        break;
                    }
                }
                break;
            case 57:
                // get.priority
                if (ArgVal[1] <= 26) {
                    // variable is second argument and should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 63:
                // set.horizon(A)
                switch (ErrorLevel) {
                case High:
                    if (ArgVal[0] >= 167) {
                        // >=167 will cause AGI to freeze/crash
                        AddError(4126, false);
                        return false;
                    }
                    // >120 or <16 is unusual
                    if (ArgVal[0] > 120) {
                        AddWarning(5042);
                    }
                    else if (ArgVal[0] < 16) {
                        AddWarning(5041);
                    }
                    break;
                case Medium:
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
                // object.on.water, object.on.land, object.on.anything
                if (ArgVal[0] == 0) {
                    // warn if used on ego
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5082);
                        break;
                    }
                }
                break;
            case 69:
                // distance(oA, oB, vC)
                if (ArgVal[2] <= 26) {
                    // variable is third arg and should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 73 or 75:
                // end.of.loop, reverse.loop
                if (ArgVal[1] <= 15) {
                    // flag arg should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 81:
                // move.obj(oA, X,Y,STEP,fDONE)
                if (ArgVal[1] > 159 || ArgVal[2] > 167) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4127, false);
                        return false;
                    case Medium:
                        AddWarning(5062);
                        break;
                    }
                }
                if (ArgVal[0] == 0) {
                    // ego object forces program mode
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5045);
                        break;
                    }
                }
                if (ArgVal[4] <= 15) {
                    // flag arg should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 82:
                // move.obj.v
                if (ArgVal[0] == 0) {
                    // ego object forces program mode
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5045);
                        break;
                    }
                }
                if (ArgVal[4] <= 15) {
                    // flag arg should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 83:
                // follow.ego(oA, DISTANCE, fDONE)
                if (ArgVal[1] <= 1) {
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5102);
                        break;
                    }
                }
                if (ArgVal[0] == 0) {
                    // ego can't follow ego
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5027);
                        break;
                    }
                }
                if (ArgVal[2] <= 15) {
                    // flag arg should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                CheckResFlagUse(ArgVal[2]);
                break;
            case 86:
                // set.dir(oA, vB)
                if (ArgVal[0] == 0) {
                    // has no effect on ego object
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5026);
                        break;
                    }
                }
                break;
            case 87:
                // get.dir (oA, vB)
                if (ArgVal[1] <= 26) {
                    // variable arg should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 90:
                // block(x1,y1,x2,y2)
                if (ArgVal[0] > 159 || ArgVal[1] > 167 || ArgVal[2] > 159 || ArgVal[3] > 167) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4129, false);
                        return false;
                    case Medium:
                        AddWarning(5020);
                        break;
                    }
                }
                if ((ArgVal[2] - ArgVal[0] < 2) || (ArgVal[3] - ArgVal[1] < 2)) {
                    // invalid arguments
                    switch (ErrorLevel) {
                    case High:
                        AddError(4129, false);
                        return false;
                    case Medium:
                        AddWarning(5051);
                        break;
                    }
                }
                break;
            case 98:
                // load.sound(A)
                if (!compGame.agSnds.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4130, LoadResString(4130).Replace(ARG1, ArgVal[0].ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5014);
                        break;
                    }
                }
                break;
            case 99:
                // sound(A, fB)
                if (!compGame.agSnds.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4137, LoadResString(4137).Replace(ARG1, ArgVal[0].ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5084);
                        break;
                    }
                }
                if (ArgVal[1] <= 15) {
                    // flag arg should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5078, LoadResString(5078).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                CheckResFlagUse(ArgVal[1]);
                break;
            case 103:
                // display(ROW, COL, mC)
                if (ArgVal[0] > 24 || ArgVal[1] > 39) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4131, false);
                        return false;
                    case Medium:
                        AddWarning(5059);
                        break;
                    }
                }
                break;
            case 105:
                // clear.lines(TOP, BTM, C)
                if (ArgVal[0] > 24 || ArgVal[1] > 24 || ArgVal[0] > ArgVal[1]) {
                    // top must be >btm; both must be <=24
                    switch (ErrorLevel) {
                    case High:
                        AddError(4132, false);
                        return false;
                    case Medium:
                        AddWarning(5011);
                        break;
                    }
                }
                if (ArgVal[2] > 0 && ArgVal[2] != 15) {
                    // color value should be 0 or 15 /(but it doesn't
                    // hurt to be anything else)
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5100);
                        break;
                    }
                }
                break;
            case 109:
                // set.text.attribute(A,B)
                if (ArgVal[0] > 15 || ArgVal[1] > 15) {
                    // color value should be 0 or 15 /(but it doesn't
                    // hurt to be anything else)
                    switch (ErrorLevel) {
                    case High:
                        AddError(4133, false);
                        return false;
                    case Medium:
                        AddWarning(5029);
                        break;
                    }
                }
                break;
            case 110:
                // shake.screen(A)
                if (ArgVal[0] == 0) {
                    // zero is BAD
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddError(4134, false);
                        return false;
                    }
                }
                else if (ArgVal[0] > 15) {
                    if (ArgVal[0] >= 100 && ArgVal[0] <= 109) {
                        // could be a palette change
                        switch (ErrorLevel) {
                        case High or Medium:
                            AddWarning(5058);
                            break;
                        }
                    }
                    else {
                        // shouldn't normally have more than a few shakes
                        switch (ErrorLevel) {
                        case High or Medium:
                            AddWarning(5057);
                            break;
                        }
                    }
                }
                break;
            case 111:
                // configure.screen(TOP,INPUT,STATUS)
                if (ArgVal[0] > 3) {
                    // top should be <=3
                    switch (ErrorLevel) {
                    case High:
                        AddError(4135, false);
                        return false;
                    case Medium:
                        AddWarning(5044);
                        break;
                    }
                }
                if (ArgVal[1] > 24 || ArgVal[2] > 24) {
                    // input or status are invalid
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5099);
                        break;
                    }
                }
                if (ArgVal[1] == ArgVal[2]) {
                    // input and status should not be equal
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5048);
                        break;
                    }
                }
                if ((ArgVal[1] >= ArgVal[0] && ArgVal[1] <= ArgVal[0] + 20) || (ArgVal[2] >= ArgVal[0] && ArgVal[2] <= ArgVal[0] + 20)) {
                    // input and status should be <top or >=top+21
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5049);
                        break;
                    }
                }
                break;
            case 114:
                // set.string(sA, mB)
                if (ArgVal[0] == 0) {
                    if (strMsg[ArgVal[1]].Length > 10) {
                        // warn user if setting input prompt to unusually long value
                        switch (ErrorLevel) {
                        case High or Medium:
                            AddWarning(5096);
                            break;
                        }
                    }
                }
                break;
            case 115:
                // get.string(sA, mB, ROW,COL,LEN)
                if (ArgVal[2] > 24) {
                    // if row>24, both row/col are ignored
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5052);
                        break;
                    }
                }
                if (ArgVal[3] > 39) {
                    // if col>39, len is limited automatically to <=40
                    switch (ErrorLevel) {
                    case High:
                        AddError(4004, false);
                        return false;
                    case Medium:
                        AddWarning(5080);
                        break;
                    }
                }
                if (ArgVal[4] > 40) {
                    // invalid len value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5056);
                        break;
                    }
                }
                break;
            case 121:
                // set.key(A,B,cC)
                // increment controller Count
                intCtlCount++;
                if (ArgVal[0] > 0 && ArgVal[1] > 0 && ArgVal[0] != 1) {
                    // A or B must be zero to be valid ascii or keycode
                    // (A can be 1 to mean joystick)
                    switch (ErrorLevel) {
                    case High:
                        AddError(4154, false);
                        return false;
                    case Medium:
                        AddWarning(5065);
                        break;
                    }
                }
                // check for improper ASCII assignments
                if (ArgVal[1] == 0) {
                    if (ArgVal[0] == 8 || ArgVal[0] == 13 || ArgVal[0] == 32) {
                        // ascii codes for bkspace, enter, spacebar
                        switch (ErrorLevel) {
                        case High:
                            AddError(4155, false);
                            return false;
                        case Medium:
                            AddWarning(5066);
                            break;
                        }
                    }
                }
                // check for improper KEYCODE assignments
                if (ArgVal[0] == 0) {
                    if ((ArgVal[1] >= 71 && ArgVal[1] <= 73) ||
                        (ArgVal[1] >= 75 && ArgVal[1] <= 77) ||
                        (ArgVal[1] >= 79 && ArgVal[1] <= 83)) {
                        // ascii codes arrow keys can't be assigned to controller
                        switch (ErrorLevel) {
                        case High:
                            AddError(4155, false);
                            return false;
                        case Medium:
                            AddWarning(5066);
                            break;
                        }
                    }
                }
                break;
            case 122:
                // add.to.pic(VIEW,LOOP,CEL,X,Y,PRI,MGN)
                if (!compGame.agViews.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4138, LoadResString(4138).Replace(ARG1, (ArgVal[0]).ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5064);
                        blnWarned = true;
                        break;
                    }
                }
                if (!blnWarned) {
                    try {
                        blnUnload = !compGame.agViews[ArgVal[0]].Loaded;
                        if (blnUnload) {
                            compGame.agViews[ArgVal[0]].Load();
                        }
                        // if view is valid, check loop
                        if (ArgVal[1] >= compGame.agViews[ArgVal[0]].Loops.Count) {
                            switch (ErrorLevel) {
                            case High:
                                AddError(4139, LoadResString(4139).Replace(ARG1, ArgVal[1].ToString()).Replace(ARG2, ArgVal[0].ToString()), false);
                                if (blnUnload) {
                                    compGame.agViews[ArgVal[0]].Unload();
                                }
                                return false;
                            case Medium:
                                AddWarning(5085);
                                blnWarned = true;
                                break;
                            }
                        }
                        // if loop is valid, check cel
                        if (!blnWarned) {
                            if (ArgVal[2] >= compGame.agViews[ArgVal[0]].Loops[ArgVal[1]].Cels.Count) {
                                switch (ErrorLevel) {
                                case High:
                                    AddError(4140, LoadResString(4140).Replace(ARG1, ArgVal[2].ToString()).Replace(ARG2, ArgVal[1].ToString()).Replace(ARG3, ArgVal[0].ToString()), false);
                                    if (blnUnload) {
                                        compGame.agViews[ArgVal[0]].Unload();
                                    }
                                    return false;
                                case Medium:
                                    AddWarning(5086);
                                    break;
                                }
                            }
                            if (compGame.agViews[ArgVal[0]].Loops[ArgVal[1]].Cels[ArgVal[2]].Width < 3 && ArgVal[6] < 4) {
                                // CEL width must be >=3
                                switch (ErrorLevel) {
                                case High:
                                    if (compGame.agViews[ArgVal[0]].Loops[ArgVal[1]].Cels[ArgVal[2]].Width == 2) {
                                        AddError(4165, false);
                                        return false;
                                    }
                                    else {
                                        AddWarning(5115);
                                        break;
                                    }
                                case Medium:
                                    AddWarning(5115);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception) {
                        // error trying to load add a warning
                        AddWarning(5021, LoadResString(5021).Replace(ARG1, ArgVal[0].ToString()));
                    }
                    if (blnUnload) {
                        compGame.agViews[ArgVal[0]].Unload();
                    }
                }
                if (ArgVal[3] > 159 || ArgVal[4] > 167) {
                    // invalid x or y value
                    switch (ErrorLevel) {
                    case High:
                        AddError(4141, false);
                        return false;
                    case Medium:
                        AddWarning(5038);
                        break;
                    }
                }
                if (ArgVal[5] > 15) {
                    // invalid priority value
                    switch (ErrorLevel) {
                    case High:
                        AddError(4142, false);
                        return false;
                    case Medium:
                        AddWarning(5079);
                        break;
                    }
                }
                if (ArgVal[5] < 4 && ArgVal[5] != 0) {
                    // unusual priority value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5079);
                        break;
                    }
                }
                if (ArgVal[6] > 15) {
                    // MGN values >15 will only use lower nibble
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5101);
                        break;
                    }
                }
                break;
            case 129:
                // show.obj(VIEW)
                if (!compGame.agViews.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4144, LoadResString(4144).Replace(ARG1, ArgVal[0].ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5061);
                        break;
                    }
                }
                break;
            case 127 or 176 or 178:
                // init.disk, hide.mouse, show.mouse
                // these commands have no usefulness
                switch (ErrorLevel) {
                case High or Medium:
                    AddWarning(5087, LoadResString(5087).Replace(ARG1, ActionCommands[CmdNum].Name));
                    break;
                }
                break;
            case 175 or 179 or 180:
                // discard.sound, fence.mouse, mouse.posn
                // these commands not valid in MSDOS AGI
                switch (ErrorLevel) {
                case High:
                    AddError(4152, LoadResString(4152).Replace(ARG1, ActionCommands[CmdNum].Name), false);
                    return false;
                case Medium:
                    AddWarning(5088, LoadResString(5088).Replace(ARG1, ActionCommands[CmdNum].Name));
                    break;
                }
                break;
            case 130:
                // random(LOWER,UPPER,vRESULT)
                if (ArgVal[0] > ArgVal[1]) {
                    // lower should be < upper
                    switch (ErrorLevel) {
                    case High:
                        AddError(4145, false);
                        return false;
                    case Medium:
                        AddWarning(5054);
                        break;
                    }
                }
                if (ArgVal[0] == ArgVal[1]) {
                    // lower=upper means result=lower=upper
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5106);
                        break;
                    }
                }
                if (ArgVal[0] == ArgVal[1] + 1) {
                    // this causes divide by 0!
                    switch (ErrorLevel) {
                    case High:
                        AddError(4158, false);
                        return false;
                    case Medium:
                        AddWarning(5107);
                        break;
                    }
                }
                if (ArgVal[2] <= 26) {
                    // variable arg should not be a reserved Value
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5077, LoadResString(5077).Replace(ARG1, ActionCommands[CmdNum].Name));
                        break;
                    }
                }
                break;
            case 134:
                // quit -
                // if v2.089 or earlier OR if arg is non-zero
                // no other commands will be processed
                if (compGame.agIntVersion == "2.089" || ArgVal[0] > 0) {
                    endingCmd = 3;
                }
                break;
            case 142:
                // script.size
                if (bytLogComp != 0) {
                    //warn if in other than logic0
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5039);
                        break;
                    }
                }
                if (ArgVal[0] < 10) {
                    // absurdly low value for script size
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5009);
                        break;
                    }
                }
                break;
            case 147:
                // reposition.to(oA, B,C)
                if (ArgVal[1] > 159 || ArgVal[2] > 167) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4128, false);
                        return false;
                    case Medium:
                        AddWarning(5023);
                        break;
                    }
                }
                break;
            case 150:
                // trace.info(LOGIC,ROW,HEIGHT)
                if (!compGame.agLogs.Contains(ArgVal[0])) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4153, LoadResString(4153).Replace(ARG1, ArgVal[0].ToString()), false);
                        return false;
                    case Medium:
                        AddWarning(5040);
                        break;
                    }
                }
                if (ArgVal[2] < 2) {
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5046);
                        break;
                    }
                }
                if (ArgVal[1] + ArgVal[2] > 23) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4146, false);
                        return false;
                    case Medium:
                        AddWarning(5063);
                        break;
                    }
                }
                break;
            case 151 or 152:
                // print.at(mA, ROW, COL, MAXWIDTH)
                // print.at.v(vMSG, ROW, COL, MAXWIDTH)
                if (ArgVal[1] > 22) {
                    switch (ErrorLevel) {
                    case High:
                        AddError(4147, false);
                        return false;
                    case Medium:
                        AddWarning(5067);
                        break;
                    }
                }
                switch (ArgVal[3]) {
                case 0:
                    //maxwidth=0 defaults to 30
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5105);
                        break;
                    }
                    break;
                case 1:
                    //maxwidth=1 crashes AGI
                    switch (ErrorLevel) {
                    case High:
                        AddError(4043, false);
                        return false;
                    case Medium:
                        AddWarning(5103);
                        break;
                    }
                    break;
                default:
                    if (ArgVal[3] > 36) {
                        //maxwidth >36 won't work
                        switch (ErrorLevel) {
                        case High:
                            AddError(4043, false);
                            return false;
                        case Medium:
                            AddWarning(5104);
                            break;
                        }
                    }
                    break;
                }
                if (ArgVal[2] < 2 || ArgVal[2] + ArgVal[3] > 39) {
                    // invalid COL value
                    switch (ErrorLevel) {
                    case High:
                        AddError(4148, false);
                        return false;
                    case Medium:
                        AddWarning(5068);
                        break;
                    }
                }
                break;
            case 154:
                // clear.text.rect(R1,C1,R2,C2,COLOR)
                if (ArgVal[0] > 24 || ArgVal[1] > 39 ||
                   ArgVal[2] > 24 || ArgVal[3] > 39 ||
                   ArgVal[2] < ArgVal[0] || ArgVal[3] < ArgVal[1]) {
                    // invalid items
                    switch (ErrorLevel) {
                    case High:
                        AddError(4150, false);
                        return false;
                    case Medium:
                        if (ArgVal[2] < ArgVal[0] || ArgVal[3] < ArgVal[1]) {
                            // pos2 < pos1
                            AddWarning(5069);
                        }
                        if (ArgVal[0] > 24 || ArgVal[1] > 39 ||
                           ArgVal[2] > 24 || ArgVal[3] > 39) {
                            // variables outside limits
                            AddWarning(5070);
                        }
                        break;
                    }
                }
                if (ArgVal[4] > 0 && ArgVal[4] != 15) {
                    // color value should be 0 or 15  (but
                    // it doesn't hurt to be anything else)
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5100);
                        break;
                    }
                }
                break;
            case 158:
                // submit.menu()
                if (bytLogComp != 0) {
                    // should only be called in logic0
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5047);
                        break;
                    }
                }
                break;
            case 174:
                // set.pri.base(A)
                if (ArgVal[0] > 167) {
                    // value >167 doesn't make sense
                    switch (ErrorLevel) {
                    case High or Medium:
                        AddWarning(5071);
                        break;
                    }
                }
                break;
            }
            // success
            return true;
        }

        /// <summary>
        /// This method checks the values passed as arguments to the specified test
        /// command and adds warnings or errors as needed.
        /// </summary>
        /// <param name="CmdNum"></param>
        /// <param name="ArgVal"></param>
        /// <returns></returns>
        internal static bool ValidateIfArgs(int CmdNum, ref byte[] ArgVal) {
            switch (CmdNum) {
            case 11 or 16 or 17 or 18:
                // posn(oA, X1, Y1, X2, Y2)
                // obj.in.box(oA, X1, Y1, X2, Y2)
                // center.posn(oA, X1, Y1, X2, Y2)
                // right.posn(oA, X1, Y1, X2, Y2)
                if (ArgVal[1] > 159 || ArgVal[2] > 167 || ArgVal[3] > 159 || ArgVal[4] > 167) {
                    // invalid position
                    switch (ErrorLevel) {
                    case High:
                        AddError(4151, false);
                        return false;
                    case Medium:
                        AddWarning(5072);
                        break;
                    }
                }
                if ((ArgVal[1] > ArgVal[3]) || (ArgVal[2] > ArgVal[4])) {
                    // start and stop are backwards
                    switch (ErrorLevel) {
                    case High:
                        AddError(4151, false);
                        return false;
                    case Medium:
                        AddWarning(5073);
                        break;
                    }
                }
                break;
            }
            // success
            return true;
        }

        /// <summary>
        /// Checks the specified message text for presence of extended characters.
        /// Adds a warning event if any are found.
        /// </summary>
        /// <param name="strMsg"></param>
        /// <param name="MsgNum"></param>
        internal static void ValidateMsgChars(string strMsg, int MsgNum) {
            if (ErrorLevel == Low) {
                return;
            }
            if (strMsg.Any(ch => ch > 127)) {
                switch (ErrorLevel) {
                case High or Medium:
                    AddWarning(5094);
                    // need to track warning in case this msg is
                    // also included in body of logic
                    intMsgWarn[MsgNum] |= 2;
                    break;
                }
            }
        }

        /// <summary>
        /// Writes the messages for a logic at the end of the resource.
        /// Messages are encrypted with the string 'Avis Durgan'. No gaps
        /// are allowed, so messages that are skipped must be included as
        /// zero length messages.
        /// </summary>
        /// <returns></returns>
        internal static bool WriteMsgs() {
            int lngMsgSecStart = tmpLogRes.Size;
            int[] lngMsgPos = new int[256];
            int intCharPos;
            byte bytCharVal;
            byte[] bMessage;
            int lngMsg;
            int lngMsgCount = 256;
            int lngCryptStart;
            int lngMsgLen;
            int i;
            string strHex;
            bool blnSkipChar = false;

            // find last message by counting backwards until a msg is found
            do {
                lngMsgCount--;
            } while (!blnMsg[lngMsgCount] && (lngMsgCount != 0));
            // write msg count
            tmpLogRes.WriteByte((byte)lngMsgCount);
            // write place holder for msg end
            tmpLogRes.WriteWord(0);
            // write place holders for msg pointers
            for (i = 0; i < lngMsgCount; i++) {
                tmpLogRes.WriteWord(0);
            }
            lngCryptStart = tmpLogRes.Size;
            for (lngMsg = 1; lngMsg <= lngMsgCount; lngMsg++) {
                lngMsgLen = strMsg[lngMsg].Length;
                if (blnMsg[lngMsg]) {
                    // calculate offset to start of this message (adjust by one byte,
                    // which is the byte that indicates how many msgs there are)
                    lngMsgPos[lngMsg] = tmpLogRes.Pos - (lngMsgSecStart + 1);
                }
                else {
                    // for unused messages need to write a null value for offset;
                    // (when it gets added after all messages are written it gets
                    // set to the beginning of message section)
                    lngMsgPos[lngMsg] = 0;
                }
                if (lngMsgLen > 0) {
                    // convert to byte array based on codepage
                    bMessage = compLogic.CodePage.GetBytes(strMsg[lngMsg]);
                    Debug.Assert(bMessage.Length == strMsg[lngMsg].Length);
                    // step through all characters in this msg
                    intCharPos = 0;
                    while (intCharPos < bMessage.Length) {
                        bytCharVal = bMessage[intCharPos];
                        switch (bytCharVal) {
                        case 8 or 9:
                            // convert these to space (' ') to avoid trouble
                            bytCharVal = 32;
                            break;
                        case 13:
                            // convert to new line
                            bytCharVal = 10;
                            break;
                        case 92:
                            // '\'
                            // check next character for special codes
                            if (intCharPos < lngMsgLen - 1) {
                                switch (bMessage[intCharPos + 1]) {
                                case 110:
                                    //  '\n' = new line
                                    bytCharVal = 0xA;
                                    intCharPos++;
                                    break;
                                case 34:
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
                                    // '\x' - look for a hex value
                                    // (make sure at least two more characters)
                                    if (intCharPos < lngMsgLen - 3) {
                                        // get next 2 chars and hexify them
                                        strHex = "0x" + strMsg[lngMsg][(intCharPos + 2)..(intCharPos + 4)];
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
                                    // if no special char found, the single slash
                                    // should be dropped
                                    blnSkipChar = true;
                                    break;
                                }
                            }
                            else {
                                // '\' is the last char, skip it
                                blnSkipChar = true;
                            }
                            break;
                        }
                        // write the encrypted byte (need to adjust for previous messages,
                        // and current position)
                        if (!blnSkipChar) {
                            tmpLogRes.WriteByte((byte)(bytCharVal ^ bytEncryptKey[(tmpLogRes.Pos - lngCryptStart) % 11]));
                        }
                        intCharPos++;
                        blnSkipChar = false;
                    }
                }
                if (blnMsg[lngMsg]) {
                    // add trailing zero to terminate message (if msg was zero length,
                    // terminator is still needed)
                    tmpLogRes.WriteByte((byte)(0 ^ bytEncryptKey[(tmpLogRes.Pos - lngCryptStart) % 11]));
                }
            }
            // calculate length of msgs, and write it at beginning of msg section
            // (adjust by one byte, which is the byte that indicates number of
            // msgs written)
            tmpLogRes.WriteWord((ushort)(tmpLogRes.Pos - (lngMsgSecStart + 1)), lngMsgSecStart + 1);
            // write msg section start value at beginning of resource (correct by
            // two so it gives position relative to byte 2 of the logic resource
            // start
            tmpLogRes.WriteWord((ushort)(lngMsgSecStart - 2), 0);
            // write all the msg pointers (start with msg1 since msg0 is never allowed)
            for (lngMsg = 1; lngMsg <= lngMsgCount; lngMsg++) {
                tmpLogRes.WriteWord((ushort)lngMsgPos[lngMsg], lngMsgSecStart + 1 + lngMsg * 2);
            }
            // success
            return true;
        }

        /// <summary>
        /// Scans the source code to identify all valid labels. This has to be done
        /// before starting the main compiler to correctly calculate jumps.
        /// </summary>
        internal static void ReadLabels() {
            // fanAGI syntax is either 'label:' or ':label', with nothing
            // else in front of or after the label declaration
            // in Sierra syntax, only ':label' is allowed
            byte i;
            string strLabel;

            bytLabelCount = 0;
            ResetCompiler();
            do {
                if (strCurrentLine.Contains(':')) {
                    strLabel = strCurrentLine;
                    // check for 'label:' (FanAGI syntax only)
                    if (strLabel[^1] == ':' && !agSierraSyntax) {
                        strLabel = strLabel[..^1];
                        if (strLabel[^1] == ' ') {
                            // no spaces allowed between ':' and label name
                            // ignore it here; error will be caught in main compiler
                            strLabel = "";
                        }
                    }
                    // check for ':label'
                    else if (strLabel[0] == ':') {
                        strLabel = strLabel[1..];
                        if (strLabel[0] == ' ') {
                            // no spaces allowed between label name and :
                            // ignore it here; error will be caught in main compiler
                            strLabel = "";
                        }
                    }
                    else {
                        // not a label
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
                            AddError(4109, LoadResString(4109).Replace(ARG1, MAX_LABELS.ToString()), false);
                            // ignore it
                            strLabel = "";
                        }
                    }
                    if (strLabel.Length != 0) {
                        DefineNameCheck chkLabel = ValidateDefName(strLabel);
                        switch (chkLabel) {
                        case Numeric:
                            // numbers are ok for labels
                            break;
                        case DefineNameCheck.Empty:
                            AddError(4096, false);
                            break;
                        case ActionCommand:
                            AddError(4025, LoadResString(4025).Replace(ARG1, strLabel), false);
                            break;
                        case TestCommand:
                            AddError(4026, LoadResString(4026).Replace(ARG1, strLabel), false);
                            break;
                        case KeyWord:
                            AddError(4028, LoadResString(4028).Replace(ARG1, strLabel), false);
                            break;
                        case ArgMarker:
                            AddError(4091, false);
                            break;
                        case DefineNameCheck.Global:
                            AddError(4024, LoadResString(4024).Replace(ARG1, strLabel), false);
                            break;
                        case ReservedVar:
                            AddError(4033, LoadResString(4033).Replace(ARG1, strLabel), false);
                            break;
                        case ReservedFlag:
                            AddError(4030, LoadResString(4030).Replace(ARG1, strLabel), false);
                            break;
                        case ReservedNum:
                            AddError(4029, LoadResString(4029).Replace(ARG1, strLabel), false);
                            break;
                        case ReservedObj or ReservedStr:
                            AddError(4032, LoadResString(4032).Replace(ARG1, strLabel), false);
                            break;
                        case ReservedMsg:
                            AddError(4031, LoadResString(4031).Replace(ARG1, strLabel), false);
                            break;
                        case BadChar:
                            AddError(4068, false);
                            break;
                        }
                        // check against existing labels
                        for (i = 0; i < bytLabelCount; i++) {
                            if (strLabel == llLabel[i].Name) {
                                AddError(4027, LoadResString(4027).Replace(ARG1, strLabel), false);
                                break;
                            }
                        }
                        // increment number of labels, and save
                        bytLabelCount++;
                        llLabel[bytLabelCount - 1].Name = strLabel;
                        llLabel[bytLabelCount - 1].Loc = 0;
                    }
                }
                IncrementLine();
            } while (lngLine != -1);
        }

        /// <summary>
        /// This is the main compiler function. It steps through input source
        /// code one token at a time and converts it to AGI logic byte code.
        /// </summary>
        /// <returns>true if source is successfully compiled, false if any 
        /// errors are encountered.</returns>
        internal static bool CompileAGI() {
            // Note that we don't need to set blnCriticalError flag here;
            // an error will cause this function to return a value of false
            // which causes the compiler to display error info
            const int MAXGOTOS = 255;
            string nextToken, prevToken = "", strArg;
            int i, tmpVal, intCmdNum, BlockDepth = 0, intLabelNum;
            int NumGotos = 0, GotoData, CurGoto, lngReturnLine = 0;
            int lngQuitLine = -1, lngNewroomLine = -1;
            bool blnLastCmdRtn = false, blnArgsOK;
            byte[] bytArg = new byte[8];
            LogicGoto[] Gotos = new LogicGoto[MAXGOTOS + 1];
            CompilerBlockType[] Block = new CompilerBlockType[256];

            ResetCompiler();
            endingCmd = 0;
            nextToken = NextToken();
            // process tokens from the input string list until finished
            while (lngLine != -1) {
                blnLastCmdRtn = false;
                if (endingCmd > 0) {
                    if (nextToken != "}") {
                        switch (ErrorLevel) {
                        case High or Medium:
                            switch (endingCmd) {
                            case 1:
                                // return
                                AddWarning(5097);
                                break;
                            case 2:
                                // new.room
                                if (nextToken == "return") {
                                    lngNewroomLine = lngLine;
                                }
                                else {
                                    AddWarning(5095);
                                }
                                break;
                            case 3:
                                // unconditional quit
                                if (nextToken == "return") {
                                    // possible end of logic; in 
                                    // that case, no need for a warning here
                                    // UNLESS this isn't the end, so use a
                                    // flag to check for the condition
                                    lngQuitLine = lngLine;
                                }
                                else {
                                    AddWarning(5116);
                                }
                                break;
                            }
                            break;
                        }
                    }
                    endingCmd = 0;
                }
                switch (nextToken) {
                case "{":
                    // can't have a '{' token, unless it follows an 'if' or 'else'
                    if (prevToken != "if" && prevToken != "else") {
                        AddError(4008, false);
                    }
                    break;
                case "}":
                    if (BlockDepth == 0) {
                        // no block currently open
                        AddError(4010, false);
                    }
                    else {
                        if (tmpLogRes.Size == Block[BlockDepth].StartPos + 2) {
                            // block is only two bytes long, meaning no commands
                            switch (ErrorLevel) {
                            case High:
                                AddError(4049, false);
                                break;
                            case Medium:
                                AddWarning(5001);
                                break;
                            }
                        }
                        // calculate and write block length
                        Block[BlockDepth].Length = tmpLogRes.Size - Block[BlockDepth].StartPos - 2;
                        tmpLogRes.WriteWord((ushort)Block[BlockDepth].Length, Block[BlockDepth].StartPos);
                        // remove block from stack
                        BlockDepth--;
                    }
                    break;
                case "if":
                    if (!CompileIf()) {
                        // error in 'if' block
                        return false;
                    }
                    if (BlockDepth >= MAX_BLOCK_DEPTH) {
                        // block stack exceeded
                        blnCriticalError = true;
                        AddError(4110, LoadResString(4110).Replace(ARG1, MAX_BLOCK_DEPTH.ToString()), false);
                        return false;
                    }
                    BlockDepth++;
                    Block[BlockDepth].StartPos = tmpLogRes.Pos;
                    Block[BlockDepth].IsIf = true;
                    // write placeholder for block length
                    tmpLogRes.WriteWord(0);
                    // next token should be a bracket
                    if (!CheckChar('{')) {
                        AddError(4053, false);
                        // if a stray ')' just ignore it
                        if (CheckChar(')')) {
                            // and check for bracket
                            _ = CheckChar('{');
                        }
                    }
                    break;
                case "else":
                    // else can only follow a close bracket
                    if (prevToken != "}") {
                        AddError(4011, false);
                        // if there is a block, assume there was a '}'
                        // and continue
                    }
                    if (!Block[BlockDepth + 1].IsIf) {
                        // previous block was an 'else' - can't be followed
                        // by another 'else'
                        AddError(4083, false);
                        return false;
                    }
                    // adjust blockdepth to match the 'if' block directly before
                    // this 'else'
                    BlockDepth++;
                    // adjust the block length to accomodate the 'else' statement
                    Block[BlockDepth].Length += 3;
                    tmpLogRes.WriteWord((ushort)Block[BlockDepth].Length, Block[BlockDepth].StartPos);
                    // the block is now an 'else' block
                    Block[BlockDepth].IsIf = false;
                    // write the 'else' bytecode
                    tmpLogRes.WriteByte(0xFE);
                    Block[BlockDepth].StartPos = tmpLogRes.Pos;
                    // placeholder for block length
                    tmpLogRes.WriteWord(0);
                    // next token better be a bracket
                    if (!CheckChar('{')) {
                        AddError(4053, false);
                    }
                    break;
                case "goto":
                    if (!agSierraSyntax) {
                        // next token should be "("
                        if (!CheckChar('(')) {
                            AddError(4006, false);
                        }
                    }
                    strArg = NextToken();
                    int lblnum = LabelNum(strArg);
                    if (lblnum == -1) {
                        // argument is NOT a valid label
                        AddError(4074, LoadResString(4074).Replace(ARG1, strArg), false);
                    }
                    else {
                        if (NumGotos < MAXGOTOS) {
                            //save this goto info on goto stack
                            Gotos[NumGotos].LabelNum = (byte)lblnum;
                            // write goto command byte
                            tmpLogRes.WriteByte(0xFE);
                            Gotos[NumGotos].DataLoc = tmpLogRes.Pos;
                            // write placeholder for amount of offset
                            tmpLogRes.WriteWord(0);
                            NumGotos++;
                        }
                        else {
                            // too many gotos
                            AddError(4108, LoadResString(4108).Replace(ARG1, MAXGOTOS.ToString()), false);
                        }
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
                        // next character should be ")"
                        if (NextChar() != ')') {
                            AddError(4003, false);
                        }
                    }
                    // verify command is correct end-of-line marker
                    CheckForEOL();
                    break;
                case "/*" or "*/":
                    //since block tokens are no longer supported, check for markers in
                    //order to provide a meaningful error message
                    AddError(4052, false);
                    break;
                case "++" or "--":
                    //unary operators
                    if (nextToken == "++") {
                        tmpLogRes.WriteByte(1);
                    }
                    else {
                        tmpLogRes.WriteByte(2);
                    }
                    // get variable
                    strArg = NextToken();
                    if (!ConvertArgument(ref strArg, Var)) {
                        AddError(4046, false);
                        // use a temp placeholder
                        strArg = "v255";
                    }
                    // convert variable to byte code value
                    intCmdNum = VariableValue(strArg);
                    if (intCmdNum == -1) {
                        AddError(4066, LoadResString(4066).Replace(ARG1, ""), false);
                        // use a temp placeholder
                        intCmdNum = 0;
                    }
                    // write the variable value
                    tmpLogRes.WriteByte((byte)intCmdNum);
                    // verify next token is correct end of line marker
                    CheckForEOL();
                    break;
                case ":":
                    // alternate label syntax - next token should be the label
                    nextToken = NextToken();
                    intLabelNum = LabelNum(nextToken);
                    if (intLabelNum == -1) {
                        // not a valid label
                        AddError(4076, false);
                    }
                    else {
                        // save position of label
                        llLabel[intLabelNum].Loc = tmpLogRes.Size;
                    }
                    // sierra syntax allows optional trailing ';'
                    if (agSierraSyntax) {
                        _ = CheckChar(';', true);
                    }
                    break;
                default:
                    // must be a label:, command, or special syntax
                    if (strCurrentLine.Length > lngPos + 1 && strCurrentLine[lngPos + 1] == ':') {
                        intLabelNum = LabelNum(nextToken);
                        if (intLabelNum == -1) {
                            // not a valid label
                            AddError(4076, false);
                        }
                        else {
                            // save position of label
                            llLabel[intLabelNum].Loc = tmpLogRes.Size;
                        }
                        // skip past the colon
                        lngPos++;
                    }
                    else {
                        // get  command opcode number
                        intCmdNum = CommandNum(false, nextToken);
                        if (intCmdNum != 255) {
                            // write the command code,
                            tmpLogRes.WriteByte((byte)intCmdNum);
                            // next character should be "("
                            if (!CheckChar('(')) {
                                AddError(4048, false);
                            }
                            lngQuoteAdded = -1;
                            // now extract arguments (assume all OK)
                            blnArgsOK = true;
                            for (i = 0; i < ActionCommands[intCmdNum].ArgType.Length; i++) {
                                if (i > 0) {
                                    if (!CheckChar(',')) {
                                        // check for added quotes; they are usually the problem
                                        if (lngQuoteAdded >= 0) {
                                            errorLine = (lngLine - lngIncludeOffset).ToString();
                                        }
                                        // use 1-base arg values (but since referring to
                                        // previous arg, don't increment arg index)
                                        AddError(4047, LoadResString(4047).Replace(ARG1, i.ToString()), false);
                                    }
                                }
                                tmpVal = GetNextArg(ActionCommands[(byte)intCmdNum].ArgType[i], i);
                                if (tmpVal >= 0) {
                                    bytArg[i] = (byte)tmpVal;
                                }
                                else {
                                    switch (tmpVal) {
                                    case -1:
                                        // -1 => bad arg conversion
                                        // error info already added
                                        break;
                                    default:
                                        // -3 => ',' found
                                        // -2 => ')' found
                                        AddError(4054, LoadResString(4054).Replace(ARG1, (i + 1).ToString()).Replace(ARG3, ArgTypeName(ActionCommands[(byte)intCmdNum].ArgType[i])).Replace(ARG2, Commands.agCmds[intCmdNum].Name), false);
                                        break;
                                    }
                                    // use a placeholder value
                                    bytArg[i] = 0;
                                    blnArgsOK = false;
                                }
                                // write argument
                                tmpLogRes.WriteByte(bytArg[i]);
                            }
                            if (blnArgsOK) {
                                // validate arguments for this command
                                if (!ValidateArgs(intCmdNum, ref bytArg)) {
                                    return false;
                                }
                            }
                            // next character must be ")"
                            if (!CheckChar(')')) {
                                // check for added quotes; they are usually the problem
                                if (lngQuoteAdded >= 0) {
                                    errorLine = (lngQuoteAdded - lngIncludeOffset).ToString();
                                }
                                AddError(4160, false);
                            }
                            // check for return command
                            if (intCmdNum == 0) {
                                blnLastCmdRtn = true;
                                if (lngReturnLine == 0) {
                                    lngReturnLine = int.Parse(errorLine);
                                }
                                if (lngNewroomLine >= 0 || lngQuitLine >= 0) {
                                    // only ok if last cmd in block and block is 0 
                                    if (BlockDepth != 0) {
                                        int oldline = lngLine;
                                        // means not last return...........??????
                                        if (lngNewroomLine >= 0) {
                                            lngLine = lngNewroomLine;
                                            AddWarning(5095);
                                        }
                                        if (lngQuitLine >= 0) {
                                            lngLine = lngQuitLine;
                                            AddWarning(5116);
                                        }
                                        lngLine = oldline;
                                    }
                                }
                                lngQuitLine = -1;
                                lngNewroomLine = -1;
                            }
                        }
                        else {
                            // try to parse special syntax
                            if (!CompileSpecial(nextToken)) {
                                // unknown command - check for preprocessor symbol
                                if (nextToken[0] == '#' || nextToken[0] == '%') {
                                    AddError(4061, LoadResString(4061).Replace(ARG1, nextToken), false);
                                }
                                else {
                                    // check for possible misspelled command
                                    if (PeekToken(true) == "(") {
                                        // assume bad cmd name; skip to ')'
                                        AddError(4116, LoadResString(4116).Replace(ARG1, nextToken), false);
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
                                        AddError(4084, false);
                                        // skip to end of line
                                        lngPos = strCurrentLine.Length;
                                    }
                                }
                            }
                        }
                        // verify next token is correct end of line marker
                        CheckForEOL();
                    }
                    break;
                }
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
                    case High:
                        AddError(4102, false);
                        blnCriticalError = false;
                        return false;
                    case Medium:
                        // add the missing return code
                        tmpLogRes.WriteByte(0);
                        AddWarning(5016);
                        break;
                    case Low:
                        // add the missing return code
                        tmpLogRes.WriteByte(0);
                        break;
                    }
                }
            }
            // check to see if everything was wrapped up properly
            if (BlockDepth > 0) {
                errorLine = lngReturnLine.ToString();
                AddError(4009, false);
                return false;
            }
            // write in goto values
            for (CurGoto = 0; CurGoto < NumGotos; CurGoto++) {
                GotoData = llLabel[Gotos[CurGoto].LabelNum].Loc - Gotos[CurGoto].DataLoc - 2;
                if (GotoData < 0) {
                    // need to convert it to an unsigned short Value
                    GotoData = 0x10000 + GotoData;
                }
                tmpLogRes.WriteWord((ushort)GotoData, Gotos[CurGoto].DataLoc);
            }
            return true;
        }

        /// <summary>
        /// Returns the index number of the specified message, or creates a new
        /// message number index if the message is not currently in this logic's
        /// list of messages.
        /// </summary>
        /// <param name="strMsgIn"></param>
        /// <returns></returns>
        internal static int MessageNum(string strMsgIn) {
            int lngMsg;

            if (strMsgIn.Length == 0) {
                // blank messages are not common
                switch (ErrorLevel) {
                case High or Medium:
                    AddWarning(5074);
                    break;
                }
            }
            for (lngMsg = 1; lngMsg <= 255; lngMsg++) {
                if (strMsg[lngMsg] == strMsgIn) {
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
                    return lngMsg;
                }
            }
            // message isn't in list yet, find an empty spot
            for (lngMsg = 1; lngMsg <= 255; lngMsg++) {
                if (!blnMsg[lngMsg]) {
                    blnMsg[lngMsg] = true;
                    strMsg[lngMsg] = strMsgIn;
                    // validate it check for extended characters
                    ValidateMsgChars(strMsgIn, lngMsg);
                    return lngMsg;
                }
            }
            // no room found, return zero
            return 0;
        }

        /// <summary>
        /// Gets the AGI byte code for the specified command.
        /// </summary>
        /// <param name="blnIF"></param>
        /// <param name="strCmdName"></param>
        /// <returns></returns>
        internal static byte CommandNum(bool blnIF, string strCmdName) {
            if (blnIF) {
                // look for test command
                for (byte retval = 0; retval < agNumTestCmds; retval++) {
                    if (strCmdName == TestCommands[retval].Name) {
                        return retval;
                    }
                }
                // check defines
                for (int i = 0; i < lngDefineCount; i++) {
                    if (tdDefines[i].Type == TestCmd) {
                        if (tdDefines[i].Name == strCmdName) {
                            return byte.Parse(tdDefines[i].Value);
                        }
                    }
                }
            }
            else {
                // look for action command
                for (byte retval = 0; retval < agNumCmds; retval++) {
                    if (strCmdName == ActionCommands[retval].Name) {
                        return retval;
                    }
                }
                // maybe the command is a valid agi command, but
                // just not supported in this agi version
                for (byte retval = agNumCmds; retval < MAX_CMDS; retval++) {
                    if (strCmdName == ActionCommands[retval].Name) {
                        switch (ErrorLevel) {
                        case High:
                            AddError(4065, LoadResString(4065).Replace(ARG1, strCmdName), false);
                            break;
                        case Medium:
                            AddWarning(5075, LoadResString(5075).Replace(ARG1, strCmdName));
                            break;
                        }
                        // don't worry about command validity; return the extracted command num
                        return retval;
                    }
                }
                // check defines
                for (int i = 0; i < lngDefineCount; i++) {
                    if (tdDefines[i].Type == ActionCmd) {
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
        /// This method determines if the specified token represents the start
        /// of a properly formatted special test syntax, and if so, parses it.
        /// </summary>
        /// <param name="strArg1"></param>
        /// <param name="blnNOT"></param>
        /// <returns></returns>
        internal static bool CompileSpecialIf(string strArg1, bool blnNOT) {
            // proprerly formatted special IF syntax will be one of following:
            //    v## expr v##
            //    v## expr ##
            //    f##
            //    v##
            //
            // where ## is a number from 1-255
            // and expr is //"==", "!=", "<", ">=", "=>", ">", "<=", "=<"
            //
            // none of the possible test commands in special syntax format need to be
            // validated, so no call to ValidateIfArgs
            string strArg2;
            int intArg1, intArg2;
            bool blnIsVar = true, blnArg2Var, blnAddNOT = false;
            byte bytCmdNum;

            if (!ConvertArgument(ref strArg1, Var)) {
                // first token is not a variable
                blnIsVar = false;
                // check for flag argument
                if (!ConvertArgument(ref strArg1, Flag)) {
                    // first token is not a flag
                    AddError(4039, LoadResString(4039).Replace(ARG1, strArg1), false);
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
            // first token has been evaluated as valid variable or flag
            if (blnIsVar) {
                // arg 1 is 'v#'
                intArg1 = VariableValue(strArg1);
                if (intArg1 == -1) {
                    // invalid number
                    AddError(4086, false);
                    // use place holder
                    intArg1 = 255;
                }
                // next token should be the expression
                strArg2 = NextToken();
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
                case "<=" or "=<":
                    bytCmdNum = 0x5;
                    blnAddNOT = true;
                    break;
                case "<":
                    bytCmdNum = 0x3;
                    break;
                case ">=" or "=>":
                    bytCmdNum = 0x3;
                    blnAddNOT = true;
                    break;
                case ")" or "&&" or "||":
                    // means we are doing a boolean test of the variable;
                    // use greatern with zero as arg
                    tmpLogRes.WriteByte(0x5);
                    tmpLogRes.WriteByte((byte)intArg1);
                    tmpLogRes.WriteByte(0);
                    // restore the token to the stream so main compiler
                    // can handle it
                    lngPos -= strArg2.Length;
                    return true;
                default:
                    // unknown expression token
                    AddError(4078, false);
                    // use placeholder cmd
                    bytCmdNum = 0x1;
                    break;
                }
                // get second argument (numerical or variable)
                blnArg2Var = true;
                lngQuoteAdded = -1;
                intArg2 = GetNextArg(Num, -1, ref blnArg2Var);
                if (intArg2 < 0) {
                    // invalid arg value found
                    switch (intArg2) {
                    case -1:
                        // TODO: generic syntax error; change error message to
                        // more meaningful value
                        //errInfo.Text = errInfo.Text[49..(errInfo.Text.LastIndexOf('\'') - 1)];
                        //errInfo.Text = LoadResString(4089).Replace(ARG1, errInfo.Text);
                        break;
                    default:
                        AddError(4089, LoadResString(4089).Replace(ARG1, ""), false);
                        // skip the next char and keep processing the line
                        lngPos++;
                        break;
                    }
                }
                if (blnArg2Var) {
                    // use variable version of command
                    bytCmdNum++;
                }
                if (blnAddNOT) {
                    tmpLogRes.WriteByte(0xFD);
                }
                // write command, and arguments
                tmpLogRes.WriteByte(bytCmdNum);
                tmpLogRes.WriteByte((byte)intArg1);
                tmpLogRes.WriteByte((byte)intArg2);
            }
            else {
                // first token is a flag
                intArg1 = VariableValue(strArg1);
                if (intArg1 == -1) {
                    // invalid flag number
                    AddError(4002, LoadResString(4066).Replace(ARG1, "1"), false);
                    return false;
                }
                // write isset cmd
                tmpLogRes.WriteByte(0x7);
                tmpLogRes.WriteByte((byte)intArg1);
            }
            return true;
        }

        /// <summary>
        /// This method determines if the specified token represents the start of
        /// a properly formatted special assignment syntax, and if so, parses it. 
        /// </summary>
        /// <param name="strArgIn"></param>
        /// <returns></returns>
        internal static bool CompileSpecial(string strArgIn) {
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
            // in Sierra syntax, @= and =@ are also valid for exprC
            // if arg1 is a flag, only valid exprC is '=', and only valid
            // arg2 value is 'true' or 'false';
            //
            // if arg1 is a string, only valid exprC is '=' and only valid
            // arg2 value is m# (or a literal string)
            string strArg1, strArg2;
            int intArg1 = -1, intArg2 = -1, lngLineType = 0, maxStr;
            int intDir = 0; //0 = no indirection; 1 = left; 2 = right
            bool blnArg2Var = false, blnGuess = false;
            byte bytCmd = 0;
            byte[] bytArgs;

            // assume OK until proven otherwise
            strArg1 = strArgIn;
            if (strArg1[0] == INDIR_CHAR) {
                if (agSierraSyntax) {
                    // not allowed in Sierra syntax
                    AddError(4105, false);
                }
                else {
                    // left indirection
                    //     *v# = #
                    //     *v# = v#
                    intDir = 1;
                    // next char can't be a space or newline
                    if (lngPos + 1 >= strCurrentLine.Length || strCurrentLine[lngPos + 1]== ' ') {
                        AddError(4105, false);
                    }
                }
                // get next token (should be variable for indirection)
                strArg1 = NextToken();
                if (!ConvertArgument(ref strArg1, Var)) {
                    AddError(4064, false);
                    // use placeholder
                    strArg1 = "v255";
                }
                // type is variable assignment
                lngLineType = 2;
            }
            else {
                // check for string assignment
                if (ConvertArgument(ref strArg1, Str)) {
                    lngLineType = 1;
                }
                // check for variable assignment/math
                else if (ConvertArgument(ref strArg1, Var)) {
                    lngLineType = 2;
                }
                // check for flag assignment
                else if (ConvertArgument(ref strArg1, Flag)) {
                    lngLineType = 3;
                }
                // if nothing found, assume something
                if (lngLineType == 0) {
                    // sytnax error in string/ flag / variable assignment
                    AddError(4044, LoadResString(4044).Replace(ARG1, strArg1), false);
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
                        lngPos = strCurrentLine.Length - 1;
                        if (strCurrentLine[^1] == ';') {
                            lngPos--;
                        }
                        return true;
                    }
                }
            }
            switch (lngLineType) {
            case 1:
                // string assignment
                //     s# = m#
                //     s# = "<string>"

                // not allowed in Sierra syntax
                if (agSierraSyntax) {
                    AddError(6009, false);
                    // assume rest of syntax is correct even though not allowed
                }
                intArg1 = VariableValue(strArg1);
                switch (ErrorLevel) {
                case High or Medium:
                    // for version 2.089, 2.272, and 3.002149 only 12 strings
                    maxStr = compGame.agIntVersion switch {
                        "2.089" or "2.272" or "3.002149" => 11,
                        _ => 23,
                    };
                    if (intArg1 > maxStr) {
                        switch (ErrorLevel) {
                        case High:
                            AddError(4079, LoadResString(4079).Replace(ARG1, "1").Replace(ARG2, maxStr.ToString()), false);
                            break;
                        case Medium:
                            AddWarning(5007, LoadResString(5007).Replace(ARG1, maxStr.ToString()));
                            break;
                        }
                    }
                    break;
                }
                // next token must be "="
                if (!CheckToken("=")) {
                    AddError(4034, false);
                    // check for "=="; it's a common error
                    if (!CheckToken("==")) {
                        // line is seriously messed; skip it
                        lngPos = strCurrentLine.Length;
                        return true;
                    }
                }
                // check for premature eol marker
                if (PeekToken() == ";") {
                    AddError(4058, false);
                    return true;
                }
                //get actual second variable
                // (use argument extractor in case second variable is a literal string)
                intArg2 = GetNextArg(Msg, -1);
                if (intArg2 < 0) {
                    // error - not a valid message
                    AddError(4058, false);
                    switch (intArg2) {
                    case -1:
                        // if next token is '(', it means assignment probably just missing
                        if (PeekToken(true) == "(") {
                            // assume rest of line is bad
                            lngPos = strCurrentLine.Length;
                            return true;
                        }
                        break;
                    default:
                        // -1, -2:
                        // skip the bad char and keep going
                        lngPos++;
                        break;
                    }
                    // use a placeholder
                    intArg2 = 0;
                }
                // command is set.string
                bytCmd = 0x72;
                break;
            case 2:
                //variable assignment or arithmetic operation
                // (strArg1 is confirmed to be 'v#')
                intArg1 = VariableValue(strArg1);
                if (intArg1 == -1) {
                    AddError(4088, LoadResString(4088).Replace(ARG1, strArg1), false);
                    // use a placeholder
                    intArg1 = 255;
                }
                // need next token to determine what kind of assignment/operation
                strArg2 = NextToken();
                // for indirection only '=' is valid
                if (intDir == 1) {
                    switch (strArg2) {
                    case "=":
                        // ok
                        break;
                    case "==":
                        // error but treat as '='
                        AddError(4034, false);
                        // if end of line is next, just exit
                        if (PeekToken(false) == ";") {
                            return true;
                        }
                        break;
                    case "+=" or "-=" or "*=" or "/=" or "++" or "--":
                        // error- math ops not allowed on indirect variables
                        AddError(4105, false);
                        break;
                    default:
                        // this line is messed up
                        AddError(4034, false);
                        if (strArg2 == ";") {
                            lngPos = 0;
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
                        AddError(4034, false);
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
                            bytCmd = 0xB;
                        }
                        else {
                            AddError(4164, false);
                            // use 'add.n' as placeholder
                            bytCmd = 0x5;
                        }
                        break;
                    case "=@":
                        // Sierra syntax only
                        // v# =@ v#;
                        if (agSierraSyntax) {
                            bytCmd = 0xA;
                        }
                        else {
                            // error
                            AddError(4164, false);
                            // use 'add.v' as a placeholder
                            bytCmd = 0x6;
                        }
                        break;
                    default:
                        // no idea what's going on in this line
                        AddError(4034, false);
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
                    AddError(6010, false);
                    // continue anyway
                }
                intArg1 = VariableValue(strArg1);
                // next token must be equal sign
                if (!CheckToken("=")) {
                    AddError(4034, false);
                    // check for '=='; it's a common error
                    if (!CheckToken("==")) {
                        if (PeekToken(false) == ";") {
                            // exit if end-of-line marker is found
                            return true;
                        }
                    }
                }
                strArg2 = NextToken();
                switch (strArg2) {
                case "true":
                    // set this flag
                    bytCmd = 0xC;
                    break;
                case "false":
                    // reset this flag
                    bytCmd = 0xD;
                    break;
                default:
                    AddError(4034, false);
                    // if it was ';', back up to use it
                    if (strArg2 == ";") {
                        lngPos--;
                    }
                    // if next cmd is '(', it means assignment probably just missing
                    if (PeekToken(true) == "(") {
                        // assume rest of line is bad
                        lngPos = strCurrentLine.Length;
                    }
                    return true;
                }
                break;
            }
            // get second argument
            switch (bytCmd) {
            case 0x1 or 0x2 or 0xC or 0xD or 0x72:
                // skip check for second argument if cmd is known to be a single arg
                // commands: increment, decrement, reset, set
                // (set string is also skipped because second arg is already determined)
                break;
            default:
                strArg2 = NextToken();
                if (strArg2 == "*") {
                    // indirection
                    if (agSierraSyntax) {
                        // this form not allowed in Sierra syntax
                        AddError(4105, false);
                    }
                    else {
                        if (intDir == 0) {
                            // if cmd already set, it's an error
                            if (bytCmd != 0) {
                                AddError(4105, false);
                            }
                            else {
                                // right indirection
                                intDir = 2;
                                // next char can't be a space or end of line
                                if (lngPos + 1 >= strCurrentLine.Length || strCurrentLine[lngPos + 1] == ' ') {
                                    AddError(4105, false);
                                }
                            }
                        }
                        else {
                            // no double-indirection
                            AddError(4105, false);
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
                // arg2 may be v##, or ## (or f##/s## if already an error)
                blnArg2Var = true;
                if (!ConvertArgument(ref strArg2, Num, ref blnArg2Var)) {
                    // maybe it was an invalid string or flag assignment
                    if (blnGuess) {
                        // check for msg
                        if (ConvertArgument(ref strArg2, Msg)) {
                            // adjust type, use placeholders (since there's already
                            // an error encountered)
                            lngLineType = 1;
                            blnArg2Var = false;
                            bytCmd = 0x72;
                            intArg1 = 1;
                        }
                        // check for flag assignment
                        else if (strArg2 == "true" || strArg2 == "false") {
                            lngLineType = 3;
                            // use place holders since error already encountered
                            bytCmd = 0xC;
                            intArg1 = 255;
                        }
                    }
                    else {
                        AddError(4088, LoadResString(4088).Replace(ARG1, strArg2), false);
                        // use placeholder
                        strArg2 = "0";
                        if (PeekToken(true) == "(") {
                            // assume rest of line is bad
                            lngPos = strCurrentLine.Length;
                            return true;
                        }
                    }
                }
                // if guessing, type may have changed; only need to continue if
                // still processing a variable assignment
                if (lngLineType == 2) {
                    if (int.TryParse(strArg2, out intArg2)) {
                        // if it's a number, check for negative value
                        if (intArg2 < 0) {
                            if (intArg2 < -128) {
                                AddError(4095, false);
                                // use a placeholder
                                intArg2 = 255;
                            }
                            // convert to unsigned byte
                            intArg2 = 256 + intArg2;
                            switch (ErrorLevel) {
                            case High or Medium:
                                AddWarning(5098);
                                break;
                            }
                        }
                        else {
                            if (intArg2 > 255) {
                                //AddError(4088, false);
                                AddError(4088, LoadResString(4088).Replace(ARG1, strArg2), false);
                                // use a place holder
                                intArg2 = 1;
                            }
                        }
                    }
                    else {
                        // it's a variable
                        intArg2 = VariableValue(strArg2);
                        if (intArg2 == -1) {
                            AddError(4088, LoadResString(4088).Replace(ARG1, strArg2), false);
                            //AddError(4088, false);
                            // use a placeholder
                            intArg2 = 1;
                        }

                    }
                    // if indirection (only true if using sierra syntax)
                    if (bytCmd == 0xA) {
                        // rindirect only allowed if arg2 is a variable
                        if (!blnArg2Var) {
                            AddError(6011, false);
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
            if (bytCmd == 0) {
                // command is still not known
                if ((intArg1 == intArg2) && (intDir == 0)) {
                    // arg values are the same (already know arg2 is a variable)
                    // and no indirection - check for long arithmetic
                    strArg2 = NextToken();
                    if (strArg2 == ";") {
                        // this is a simple assign (with a variable being assigned
                        // to itself!!)
                        switch (ErrorLevel) {
                        case High or Medium:
                            AddWarning(5036);
                            break;
                        }
                        // assign.v
                        bytCmd = 0x3;
                        // move pointer back one space so eol check in main compiler
                        // works correctly
                        lngPos--;
                    }
                    else {
                        // this may be long arithmetic
                        if (agSierraSyntax) {
                            // not allowed in Sierra syntax
                            AddError(6012, false);
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
                            AddError(4087, false);
                            return true;
                        }
                        // now get actual second argument (number or variable)
                        blnArg2Var = true;
                        intArg2 = GetNextArg(Num, -1, ref blnArg2Var);
                        if (intArg2 < 0) {
                            // TODO: need to find new way to change error message
                            //switch (intArg2) {
                            //case -1:
                            //    errInfo.Text = LoadResString(4161).Replace(ARG1, errInfo.Text.Substring(55, errInfo.Text.LastIndexOf('\'') - 53));
                            //    break;
                            //default:
                            //    errInfo.Text = LoadResString(4161).Replace(ARG1, "");
                            //    break;
                            //}
                            //AddError(4161, errInfo.Text, false);
                            // use a placeholder
                            intArg2 = 0;
                        }
                    }
                }
                else {
                    // the second variable argument is different, or indirection
                    // must be assignment
                    //     v# = v#
                    //     *v# = v#
                    //     v# = *v#
                    switch (intDir) {
                    case 0:
                        // assign.v
                        bytCmd = 0x4;
                        // special check for improper math
                        // vA = vB + # for example
                        strArg2 = NextChar().ToString();
                        switch (strArg2) {
                        case "+" or "-" or "*" or "/":
                            // bad math
                            AddError(4005, false);
                            // ignore the symbol and next
                            strArg2 = NextToken(true);
                            break;
                        default:
                            // assume ok, put char back on stream
                            lngPos--;
                            break;
                        }
                        break;
                    case 1:
                        // lindirect.v
                        bytCmd = 0x9;
                        break;
                    case 2:
                        // rindirect
                        bytCmd = 0xA;
                        break;
                    }
                    // always reset arg2var flag so
                    // command won't be adjusted later
                    blnArg2Var = false;
                }
            }
            // if second argument is a variable
            if (blnArg2Var) {
                // use variable version of command
                bytCmd++;
            }
            // need to validate arguments for this command
            switch (bytCmd) {
            case 0x1 or 0x2 or 0xC or 0xD:
                // single arg commands
                bytArgs = new byte[1];
                bytArgs[0] = (byte)intArg1;
                break;
            default:
                // two arg commands
                bytArgs = new byte[2];
                bytArgs[0] = (byte)intArg1;
                bytArgs[1] = (byte)intArg2;
                break;
            }
            // validate commands before writing
            if (!ValidateArgs(bytCmd, ref bytArgs)) {
                return false;
            }
            // write command and arg1
            tmpLogRes.WriteByte(bytCmd);
            tmpLogRes.WriteByte((byte)intArg1);
            // write second argument for all cmds except 0x1, 0x2, 0xC, 0xD
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
