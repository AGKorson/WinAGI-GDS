using System;
using System.Linq;
using static WinAGI.Common.Base;
using static WinAGI.Engine.ArgTypeEnum;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;

namespace WinAGI.Engine {
    /// <summary>
    /// This class contains all the members and methods needed to compile logic 
    /// source code. It also contains members and methods for decompiling logics.
    /// </summary>
    public static partial class Compiler {
        #region Members
        /// <summary>
        /// The parent game attached to the compiler.
        /// </summary>
        private static AGIGame game;
        // compiler warnings
        public const int WARNCOUNT = 107;
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
        internal static AGICodeStyle mCodeStyle = AGICodeStyle.cstDefault;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets how the compiler responds to errors and unusual conditions
        /// encountered in logic source code when compiling.
        /// </summary>
        public static LogicErrorLevel ErrorLevel { get; set; }

        /// <summary>
        /// Gets or sets the formatting style that theh decompilier uses. Currently supports
        /// three formats, Default, Visual Studio, and Modified Visual Studio.
        /// </summary>
        public static AGICodeStyle CodeStyle {
            get { return mCodeStyle; }
            set {
                mCodeStyle = value;
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
        /// Gets or sets a value that determines if 'goto' commands that function as 'else'
        /// statements are displayed as 'else' or as 'goto'.
        /// </summary>
        public static bool ElseAsGoto { get; set; }

        /// <summary>
        /// Gets or sets the default source extension that WinAGI will use when 
        /// creating decompiled source code files from logics UNLESS overridden
        /// by this game's SourceExt property.
        /// </summary>
        public static string DefaultSrcExt {
            get {
                return agDefSrcExt;
            }
            set {
                // must start with a period
                if (value[0] != 46) {
                    agDefSrcExt = "." + value;
                }
                else {
                    agDefSrcExt = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether this game will compile and 
        /// decompile logics using the AGI community established syntax, or Sierra's
        /// original syntax.
        /// </summary>
        public static bool SpecialSyntax { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if reserved variables and flag are 
        /// displayed in decompiled logics as define names or as argument numbers.
        /// </summary>
        public static bool ReservedAsText { get; set; }

        /// <summary>
        /// Gets or sets a value that determines if reserved variables and flags are 
        /// considered automatically defined, or if they must be manually defined.
        /// </summary>
        public static bool UseReservedNames { get; set; }
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
        public static TDefine[] ReservedDefines(ArgTypeEnum ArgType) {
            TDefine[] tmpDefines = [];

            switch (ArgType) {
            case atNum:
                // return all numerical reserved defines
                tmpDefines = [];
                tmpDefines = (TDefine[])tmpDefines.Concat(agEdgeCodes);
                tmpDefines = (TDefine[])tmpDefines.Concat(agEgoDir);
                tmpDefines = (TDefine[])tmpDefines.Concat(agVideoMode);
                tmpDefines = (TDefine[])tmpDefines.Concat(agCompType);
                tmpDefines = (TDefine[])tmpDefines.Concat(agResColor);
                break;
            case atVar:
                // return all variable reserved defines
                tmpDefines = agResVar;
                break;
            case atFlag:
                // return all flag reserved defines
                tmpDefines = agResFlag;
                break;
            case atMsg:
                // none
                tmpDefines = [];
                break;
            case atSObj:
                // one - ego
                tmpDefines = agResObj;
                break;
            case atInvItem:
                // none
                tmpDefines = [];
                break;
            case atStr:
                // one - input prompt
                tmpDefines = agResStr;
                break;
            case atWord:
                // none
                tmpDefines = [];
                break;
            case atCtrl:
                // none
                tmpDefines = [];
                break;
            case atDefStr:
                // none
                tmpDefines = [];
                break;
            case atVocWrd:
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
            case ResDefGroup.rgVariable:
                return agResVar;
            case ResDefGroup.rgFlag:
                return agResFlag;
            case ResDefGroup.rgEdgeCode:
                return agEdgeCodes;
            case ResDefGroup.rgObjectDir:
                return agEgoDir;
            case ResDefGroup.rgVideoMode:
                return agVideoMode;
            case ResDefGroup.rgComputerType:
                return agCompType;
            case ResDefGroup.rgColor:
                return agResColor;
            case ResDefGroup.rgObject:
                return agResObj;
            case ResDefGroup.rgString:
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
                if (ValidateName(agResVar[i], true) != ncOK) {
                    agResVar[i].Name = agResVar[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agResFlag.Length; i++) {
                if (ValidateName(agResFlag[i], true) != ncOK) {
                    agResFlag[i].Name = agResFlag[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agEdgeCodes.Length; i++) {
                if (ValidateName(agEdgeCodes[i], true) != ncOK) {
                    agEdgeCodes[i].Name = agEdgeCodes[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agEgoDir.Length; i++) {
                if (ValidateName(agEgoDir[i], true) != ncOK) {
                    agEgoDir[i].Name = agEgoDir[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agVideoMode.Length; i++) {
                if (ValidateName(agVideoMode[i], true) != ncOK) {
                    agVideoMode[i].Name = agVideoMode[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agCompType.Length; i++) {
                if (ValidateName(agCompType[i], true) != ncOK) {
                    agCompType[i].Name = agCompType[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agResColor.Length; i++) {
                if (ValidateName(agResColor[i], true) != ncOK) {
                    agResColor[i].Name = agResColor[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agResObj.Length; i++) {
                if (ValidateName(agResObj[i], true) != ncOK) {
                    agResObj[i].Name = agResObj[i].Default;
                    retval = false;
                }
            }
            for (i = 0; i < agResStr.Length; i++) {
                if (ValidateName(agResStr[i], true) != ncOK) {
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
                return ncOK;
            }
            // if no name,
            if (TestDef.Name.Length == 0) {
                return ncEmpty;
            }
            // name can't be numeric
            if (IsNumeric(TestDef.Name)) {
                return ncNumeric;
            }
            // check against regular commands
            for (i = 0; i < ActionCount; i++) {
                if (TestDef.Name == ActionCommands[i].Name) {
                    return ncActionCommand;
                }
            }
            // check against test commands
            for (i = 0; i < TestCount; i++) {
                if (TestDef.Name == TestCommands[i].Name) {
                    return ncTestCommand;
                }
            }
            // check against compiler keywords
            if (
                TestDef.Name == "if" || TestDef.Name == "else" || TestDef.Name == "goto") {
                return ncKeyWord;
            }
            // if the name starts with any of these letters
            // (OK for sierra syntax)
            if (!agSierraSyntax) {
                if ("vfmoiswc".Any(TestDef.Name.ToLower().StartsWith)) {
                    if (IsNumeric(Right(TestDef.Name, TestDef.Name.Length - 1))) {
                      // can't have a name that's a valid marker
                        return ncArgMarker;
                    }
                }
            }
            // check against reserved names if this game is using them,
            // OR if testing the list of reserved names
            if (Reserved || UseReservedNames) {
                // reserved variables
                tmpDefines = ReservedDefines(atVar);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name ==tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ncReservedVar;
                        }
                    }
                }
                // reserved flags
                tmpDefines = ReservedDefines(atFlag);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name == tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ncReservedFlag;
                        }
                    }
                }
                // reserved numbers
                tmpDefines = ReservedDefines(atNum);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name == tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ncReservedNum;
                        }
                    }
                }
                // reserved objects
                tmpDefines = ReservedDefines(atSObj);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name == tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ncReservedObj;
                        }
                    }
                }
                // reserved strings
                tmpDefines = ReservedDefines(atStr);
                for (i = 0; i < tmpDefines.Length; i++) {
                    if (TestDef.Name == tmpDefines[i].Name) {
                        // if testing a reserved define AND values match, it's OK
                        // if NOT testing a reserved define OR values DON'T match, it's invalid
                        if (!Reserved || tmpDefines[i].Value != TestDef.Value) {
                            return ncReservedStr;
                        }
                    }
                }
            }
            // check name against improper character list
            if ((INVALID_DEFNAME_CHARS).Any(TestDef.Name.Contains)) {
                // bad
                return ncBadChar;
            }
            if (TestDef.Name.Any(ch => ch > 127 || ch < 32)) {
                // bad
                return ncBadChar;
            }
            // sierra syntax allows '?' 
            if (("'?").Any(TestDef.Name.Contains)) {
                if (!agSierraSyntax) {
                    return ncBadChar;
                }
            }
            // sierra syntax allows / for anything but first char
            if (("/").Any(TestDef.Name.Contains)) {
                if (!agSierraSyntax || i == 1) {
                    return ncBadChar;
                }
            }
            // must be OK!
            return ncOK;
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
            if (tmpResult != ncOK) {
                return tmpResult;
            }
            // check against current globals
            if (compGame is not null) {
                for (i = 0; i < compGame.GlobalDefines.Count; i++) {
                    if (CheckDef.Name == compGame.GlobalDefines[i].Name)
                        return ncGlobal;
                }
            }
            // check against ingame reserved defines:
            if (compGame is not null && UseReservedNames) {
                for (i = 0; i < compGame.agResGameDef.Length; i++) {
                    if (CheckDef.Name == compGame.agResGameDef[i].Name)
                        //invobj count is number; rest are msgstrings
                        return i == 3 ? ncReservedNum : ncReservedMsg;
                }
            }
            // if no error conditions, it's OK
            return ncOK;
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
        internal static DefineValueCheck ValidateDefValue(TDefine TestDefine) {
            if (TestDefine.Value.Length == 0) {
                return vcEmpty;
            }
            // values must be an AGI argument marker (variable/flag/etc), string, or a number

            // if NOT a number:
            if (!int.TryParse(TestDefine.Value, out int intVal)) {
                if ("vfmoiswc".Any(ch => ch == TestDefine.Value[0])) {
                    string strVal = TestDefine.Value[1..];
                    if (int.TryParse(strVal, out intVal)) {
                        if (intVal < 0 || intVal > 255)
                            return vcOutofBounds;
                        // check defined globals
                        for (int i = 0; i < compGame.GlobalDefines.Count; i++) {
                            if (compGame.GlobalDefines[i].Value == TestDefine.Value)
                                return vcGlobal;
                        }
                        // check if Value is already assigned
                        switch (TestDefine.Value[0]) {
                        case 'f':
                            TestDefine.Type = atFlag;
                            if (UseReservedNames) {
                                if (intVal <= 15)
                                    return vcReserved;
                                if (intVal == 20) {
                                    switch (compGame.agIntVersion) {
                                    case "3.002.098" or "3.002.102" or "3.002.107" or "3.002.149":
                                        return vcReserved;
                                    }
                                }
                            }
                            break;
                        case 'v':
                            TestDefine.Type = atVar;
                            if (UseReservedNames) {
                                if (intVal <= 26)
                                    return vcReserved;
                            }

                            break;
                        case 'm':
                            TestDefine.Type = atMsg;
                            break;
                        case 'o':
                            TestDefine.Type = atSObj;
                            if (UseReservedNames) {
                                // can't be ego
                                if (intVal == 0)
                                    return vcReserved;
                            }

                            break;
                        case 'i':
                            TestDefine.Type = atInvItem;
                            break;
                        case 's':
                            TestDefine.Type = atStr;
                            if (intVal > 23 || (intVal > 11 &&
                              (compGame.agIntVersion == "2.089" ||
                              compGame.agIntVersion == "2.272" ||
                              compGame.agIntVersion == "3.002149"))) {
                                return vcBadArgNumber;
                            }

                            break;
                        case 'w':
                            // valid from w1 to w10
                            // applies to fanAGI syntax only;
                            TestDefine.Type = atWord;
                            // base is 1 because of how msg formatting
                            // uses words; compiler will automatically
                            // convert it to base zero when used - see
                            // WinAGI help file for more details
                            if (intVal < 1 || intVal > 10) {
                                return vcBadArgNumber;
                            }
                            break;
                        case 'c':
                            // controllers limited to 0-49
                            TestDefine.Type = atCtrl;
                            if (intVal > 49) {
                                return vcBadArgNumber;
                            }
                            break;
                        }
                        return vcOK;
                    }
                }
                // non-numeric, non-marker and most likely a string
                if (IsAGIString(TestDefine.Value)) {
                    TestDefine.Type = atDefStr;
                    return vcOK;
                }
                else {
                    return vcNotAValue;
                }
            }
            else {
                // numeric
                // unsigned byte (0-255) or signed byte (-128 to 127) are OK
                if (intVal > -128 && intVal < 256) {
                    TestDefine.Type = atNum;
                    return vcOK;
                }
                else {
                    return vcOutofBounds;
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
        /// This method strips off any comments on the specified line, returning the
        /// line without comment. If trimline is false, the string is also stripped
        /// of any blank space. If there is a comment, it is passed back in the
        /// strComment argument.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="comment"></param>
        /// <param name="trimline"></param>
        /// <returns></returns>
        public static string StripComments(string line, ref string comment, bool trimline = true) {
            int lngPos = -1;
            bool blnInQuotes = false;
            bool blnSlash = false;
            bool blnDblSlash = false;
            int intROLIgnore = -1;

            if (line.Length == 0) {
                return "";
            }

            // assume no comment
            string strOut = line;
            comment = "";
            while (lngPos < line.Length - 1) {
                lngPos++;
                if (!blnInQuotes) {
                    // check for comment characters at this position
                    if (Mid(line, lngPos, 2) == "//") {
                        intROLIgnore = lngPos + 1;
                        blnDblSlash = true;
                        break;
                    }
                    else if (line.Substring(lngPos, 1) == "[") {
                        intROLIgnore = lngPos;
                        break;
                    }
                    // slash codes never occur outside quotes
                    blnSlash = false;
                    // if this character is a quote mark, it starts a string
                    blnInQuotes = line.ElementAt(lngPos) == '"';
                }
                else {
                    // if last character was a slash, ignore this character
                    // because it's part of a slash code
                    if (blnSlash) {
                        // always reset  the slash
                        blnSlash = false;
                    }
                    else {
                        // check for slash or quote mark
                        switch (line[lngPos]) {
                        case '"':
                            // a quote marks end of string
                            blnInQuotes = false;
                            break;
                        case '\\':
                            blnSlash = true;
                            break;
                        }
                    }
                }
            }
            if (intROLIgnore >= 0) {
                // save the comment
                comment = line[intROLIgnore..].Trim();
                // strip off the comment
                if (blnDblSlash) {
                    strOut = line[..(intROLIgnore - 1)];
                }
                else {
                    strOut = line[..intROLIgnore];
                }
            }
            if (trimline) {
                // return the line, trimmed
                strOut = strOut.Trim();
            }
            return strOut;
        }
        #endregion
    }
}
