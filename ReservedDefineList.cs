using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.ArgType;

namespace WinAGI.Engine {

    /// <summary>
    /// Represents a list of reserved defines that are used for this game.
    /// </summary>
    public class ReservedDefineList {

        #region Local Members
        readonly AGIGame parent;
        string filename = "";
        SettingsFile propfile = null;
        bool IsChanged = false;
        Define[] agResVar = new Define[27];      // 27: text name of built in variables
        Define[] agResFlag = new Define[18];     // 18: text name of built in flags
        Define[] agResObj = new Define[1];       //  1: just ego object (o0)
        Define[] agResStr = new Define[1];       //  1: just prompt (s0)  
        Define[] agEdgeCodes = new Define[5];    //  5: text of edge codes
        Define[] agObjDirs = new Define[9];      //  9: text of object direction codes
        Define[] agVideoModes = new Define[5];   //  5: text of video mode codes
        Define[] agCompTypes = new Define[9];    //  9: computer type codes
        Define[] agColorNames = new Define[16];  // 16: text of color indices
        Define[] agGameInfo = new Define[4];     //  4: gameID, gameversion, gameabout, invcount
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes this defines list by assigning it to the specified AGI game.
        /// </summary>
        /// <param name="parent"></param>
        public ReservedDefineList(AGIGame parent) {
            ArgumentNullException.ThrowIfNull(parent, "parent");
            this.parent = parent;
            propfile = parent.agGameProps;
            filename = Path.Combine(parent.agSrcResDir, "reserved.txt");
            // set up defaults
            InitReservedDefineList();
            GetResDefOverrides();
            if (!File.Exists(filename)) {
                SaveList(true);
            }
        }

        /// <summary>
        /// Initializes a default defines list, with the app's settings file passed
        /// as an argument.
        /// </summary>
        /// <param name="propfile"></param>
        public ReservedDefineList(SettingsFile propfile) {
            ArgumentNullException.ThrowIfNull(propfile, "propfile");
            this.parent = null;
            this.propfile = propfile;
            filename = Path.Combine(Path.GetDirectoryName(propfile.Filename), "reserved.txt");
            InitReservedDefineList();
            GetResDefOverrides();
        }
        #endregion

        #region Properties
        public Define[] ReservedVariables => agResVar;
        public Define[] ReservedFlags => agResFlag;
        public Define[] ReservedObjects => agResObj;
        public Define[] ReservedStrings => agResStr;
        public Define[] EdgeCodes => agEdgeCodes;
        public Define[] ObjDirections => agObjDirs;
        public Define[] VideoModes => agVideoModes;
        public Define[] ComputerTypes => agCompTypes;
        public Define[] ColorNames => agColorNames;

        /// <summary>
        /// Gets the reserved defines that are game-specific(GameID, GameVersion, GameAbout,
        /// InvItem Count).
        /// </summary>
        public Define[] GameInfo {
            get {
                // refresh before returning
                agGameInfo[0].Value = "\"" + parent.agGameID + "\"";
                agGameInfo[1].Value = "\"" + parent.agGameVersion + "\"";
                agGameInfo[2].Value = "\"" + parent.agGameAbout + "\"";
                Debug.Assert(parent.agInvObj.Loaded);
                agGameInfo[3].Value = parent.agInvObj.Count.ToString();
                return agGameInfo;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// This method initializes all reserved defines used by the logic compiler.
        /// </summary>
        private void InitReservedDefineList() {
            // predefined variables, flags, and objects
            // Variables v0 - v26
            // Flags f0 - f16, f20 [in version 3.102 and above]
            // object variable, o0, gameversion string, gameabout string
            // and numberofitems
            // also various numerical constants

            // variables
            agResVar[0].DefaultName = "currentRoom";
            agResVar[0].Value = "v0";
            agResVar[1].DefaultName = "previousRoom";
            agResVar[1].Value = "v1";
            agResVar[2].DefaultName = "edgeEgoHit";
            agResVar[2].Value = "v2";
            agResVar[3].DefaultName = "currentScore";
            agResVar[3].Value = "v3";
            agResVar[4].DefaultName = "objHitEdge";
            agResVar[4].Value = "v4";
            agResVar[5].DefaultName = "edgeObjHit";
            agResVar[5].Value = "v5";
            agResVar[6].DefaultName = "egoDir";
            agResVar[6].Value = "v6";
            agResVar[7].DefaultName = "maxScore";
            agResVar[7].Value = "v7";
            agResVar[8].DefaultName = "memoryLeft";
            agResVar[8].Value = "v8";
            agResVar[9].DefaultName = "unknownWordNum";
            agResVar[9].Value = "v9";
            agResVar[10].DefaultName = "animationInterval";
            agResVar[10].Value = "v10";
            agResVar[11].DefaultName = "elapsedSeconds";
            agResVar[11].Value = "v11";
            agResVar[12].DefaultName = "elapsedMinutes";
            agResVar[12].Value = "v12";
            agResVar[13].DefaultName = "elapsedHours";
            agResVar[13].Value = "v13";
            agResVar[14].DefaultName = "elapsedDays";
            agResVar[14].Value = "v14";
            agResVar[15].DefaultName = "dblClickDelay";
            agResVar[15].Value = "v15";
            agResVar[16].DefaultName = "currentEgoView";
            agResVar[16].Value = "v16";
            agResVar[17].DefaultName = "errorNumber";
            agResVar[17].Value = "v17";
            agResVar[18].DefaultName = "errorParameter";
            agResVar[18].Value = "v18";
            agResVar[19].DefaultName = "lastChar";
            agResVar[19].Value = "v19";
            agResVar[20].DefaultName = "machineType";
            agResVar[20].Value = "v20";
            agResVar[21].DefaultName = "printTimeout";
            agResVar[21].Value = "v21";
            agResVar[22].DefaultName = "numberOfVoices";
            agResVar[22].Value = "v22";
            agResVar[23].DefaultName = "attenuation";
            agResVar[23].Value = "v23";
            agResVar[24].DefaultName = "inputLength";
            agResVar[24].Value = "v24";
            agResVar[25].DefaultName = "selectedItem";
            agResVar[25].Value = "v25";
            agResVar[26].DefaultName = "monitorType";
            agResVar[26].Value = "v26";
            for (int i = 0; i <= 26; i++) {
                agResVar[i].Name = agResVar[i].DefaultName;
                agResVar[i].Type = ArgType.Var;
            }

            // flags
            agResFlag[0].DefaultName = "onWater";
            agResFlag[0].Value = "f0";
            agResFlag[1].DefaultName = "egoHidden";
            agResFlag[1].Value = "f1";
            agResFlag[2].DefaultName = "haveInput";
            agResFlag[2].Value = "f2";
            agResFlag[3].DefaultName = "egoHitSpecial";
            agResFlag[3].Value = "f3";
            agResFlag[4].DefaultName = "haveMatch";
            agResFlag[4].Value = "f4";
            agResFlag[5].DefaultName = "newRoom";
            agResFlag[5].Value = "f5";
            agResFlag[6].DefaultName = "gameRestarted";
            agResFlag[6].Value = "f6";
            agResFlag[7].DefaultName = "noScript";
            agResFlag[7].Value = "f7";
            agResFlag[8].DefaultName = "enableDblClick";
            agResFlag[8].Value = "f8";
            agResFlag[9].DefaultName = "soundOn";
            agResFlag[9].Value = "f9";
            agResFlag[10].DefaultName = "enableTrace";
            agResFlag[10].Value = "f10";
            agResFlag[11].DefaultName = "hasNoiseChannel";
            agResFlag[11].Value = "f11";
            agResFlag[12].DefaultName = "gameRestored";
            agResFlag[12].Value = "f12";
            agResFlag[13].DefaultName = "enableItemSelect";
            agResFlag[13].Value = "f13";
            agResFlag[14].DefaultName = "enableMenu";
            agResFlag[14].Value = "f14";
            agResFlag[15].DefaultName = "leaveWindow";
            agResFlag[15].Value = "f15";
            agResFlag[16].DefaultName = "noPromptRestart";
            agResFlag[16].Value = "f16";
            agResFlag[17].DefaultName = "forceAutoloop";
            agResFlag[17].Value = "f20";
            for (int i = 0; i <= 17; i++) {
                agResFlag[i].Name = agResFlag[i].DefaultName;
                agResFlag[i].Type = ArgType.Flag;
            }

            // edge codes
            agEdgeCodes[0].DefaultName = "NOT_HIT";
            agEdgeCodes[0].Value = "0";
            agEdgeCodes[1].DefaultName = "TOP_EDGE";
            agEdgeCodes[1].Value = "1";
            agEdgeCodes[2].DefaultName = "RIGHT_EDGE";
            agEdgeCodes[2].Value = "2";
            agEdgeCodes[3].DefaultName = "BOTTOM_EDGE";
            agEdgeCodes[3].Value = "3";
            agEdgeCodes[4].DefaultName = "LEFT_EDGE";
            agEdgeCodes[4].Value = "4";
            for (int i = 0; i <= 4; i++) {
                agEdgeCodes[i].Name = agEdgeCodes[i].DefaultName;
                agEdgeCodes[i].Type = ArgType.Num;
            }

            // object direction
            agObjDirs[0].DefaultName = "STOPPED";
            agObjDirs[0].Value = "0";
            agObjDirs[1].DefaultName = "UP";
            agObjDirs[1].Value = "1";
            agObjDirs[2].DefaultName = "UP_RIGHT";
            agObjDirs[2].Value = "2";
            agObjDirs[3].DefaultName = "RIGHT";
            agObjDirs[3].Value = "3";
            agObjDirs[4].DefaultName = "DOWN_RIGHT";
            agObjDirs[4].Value = "4";
            agObjDirs[5].DefaultName = "DOWN";
            agObjDirs[5].Value = "5";
            agObjDirs[6].DefaultName = "DOWN_LEFT";
            agObjDirs[6].Value = "6";
            agObjDirs[7].DefaultName = "LEFT";
            agObjDirs[7].Value = "7";
            agObjDirs[8].DefaultName = "UP_LEFT";
            agObjDirs[8].Value = "8";
            for (int i = 0; i <= 8; i++) {
                agObjDirs[i].Name = agObjDirs[i].DefaultName;
                agObjDirs[i].Type = ArgType.Num;
            }

            // video modes
            agVideoModes[0].DefaultName = "CGA";
            agVideoModes[0].Value = "0";
            agVideoModes[1].DefaultName = "RGB";
            agVideoModes[1].Value = "1";
            agVideoModes[2].DefaultName = "MONO";
            agVideoModes[2].Value = "2";
            agVideoModes[3].DefaultName = "EGA";
            agVideoModes[3].Value = "3";
            agVideoModes[4].DefaultName = "VGA";
            agVideoModes[4].Value = "4";
            for (int i = 0; i <= 4; i++) {
                agVideoModes[i].Name = agVideoModes[i].DefaultName;
                agVideoModes[i].Type = ArgType.Num;
            }

            // computer types
            agCompTypes[0].DefaultName = "PC";
            agCompTypes[0].Value = "0";
            agCompTypes[1].DefaultName = "PCJR";
            agCompTypes[1].Value = "1";
            agCompTypes[2].DefaultName = "TANDY";
            agCompTypes[2].Value = "2";
            agCompTypes[3].DefaultName = "APPLEII";
            agCompTypes[3].Value = "3";
            agCompTypes[4].DefaultName = "ATARI";
            agCompTypes[4].Value = "4";
            agCompTypes[5].DefaultName = "AMIGA";
            agCompTypes[5].Value = "5";
            agCompTypes[6].DefaultName = "MACINTOSH";
            agCompTypes[6].Value = "6";
            agCompTypes[7].DefaultName = "CORTLAND";
            agCompTypes[7].Value = "7";
            agCompTypes[8].DefaultName = "PS2";
            agCompTypes[8].Value = "8";
            for (int i = 0; i <= 8; i++) {
                agCompTypes[i].Name = agCompTypes[i].DefaultName;
                agCompTypes[i].Type = ArgType.Num;
            }

            // colors
            agColorNames[0].DefaultName = "BLACK";
            agColorNames[0].Value = "0";
            agColorNames[1].DefaultName = "BLUE";
            agColorNames[1].Value = "1";
            agColorNames[2].DefaultName = "GREEN";
            agColorNames[2].Value = "2";
            agColorNames[3].DefaultName = "CYAN";
            agColorNames[3].Value = "3";
            agColorNames[4].DefaultName = "RED";
            agColorNames[4].Value = "4";
            agColorNames[5].DefaultName = "MAGENTA";
            agColorNames[5].Value = "5";
            agColorNames[6].DefaultName = "BROWN";
            agColorNames[6].Value = "6";
            agColorNames[7].DefaultName = "LT_GRAY";
            agColorNames[7].Value = "7";
            agColorNames[8].DefaultName = "DK_GRAY";
            agColorNames[8].Value = "8";
            agColorNames[9].DefaultName = "LT_BLUE";
            agColorNames[9].Value = "9";
            agColorNames[10].DefaultName = "LT_GREEN";
            agColorNames[10].Value = "10";
            agColorNames[11].DefaultName = "LT_CYAN";
            agColorNames[11].Value = "11";
            agColorNames[12].DefaultName = "LT_RED";
            agColorNames[12].Value = "12";
            agColorNames[13].DefaultName = "LT_MAGENTA";
            agColorNames[13].Value = "13";
            agColorNames[14].DefaultName = "YELLOW";
            agColorNames[14].Value = "14";
            agColorNames[15].DefaultName = "WHITE";
            agColorNames[15].Value = "15";
            for (int i = 0; i <= 15; i++) {
                agColorNames[i].Name = agColorNames[i].DefaultName;
                agColorNames[i].Type = ArgType.Num;
            }

            // objects
            agResObj[0].DefaultName = agResObj[0].Name = "ego";
            agResObj[0].Type = ArgType.SObj;
            agResObj[0].Value = "o0";

            // strings
            agResStr[0].DefaultName = agResStr[0].Name = "inputPrompt";
            agResStr[0].Type = ArgType.Str;
            agResStr[0].Value = "s0";

            // game specific
            agGameInfo[0].DefaultName = agGameInfo[0].Name = "gameID";
            agGameInfo[0].Type = ArgType.DefStr;
            agGameInfo[0].Value = "<<gameid>>";
            agGameInfo[1].DefaultName = agGameInfo[1].Name = "gameVersionMsg";
            agGameInfo[1].Type = ArgType.DefStr;
            agGameInfo[1].Value = "<<game version msg>>";
            agGameInfo[2].DefaultName = agGameInfo[2].Name = "gameAboutMsg";
            agGameInfo[2].Type = ArgType.DefStr;
            agGameInfo[2].Value = "<<game about msg>>";
            agGameInfo[3].DefaultName = agGameInfo[3].Name = "numberOfItems";
            agGameInfo[3].Type = ArgType.Num;
            agGameInfo[3].Value = "<<invobj_count>>";
        }

        private void GetResDefOverrides() {
            if (!File.Exists(propfile.Filename)) {
                return;
            }
            int count = propfile.GetSetting("ResDefOverrides", "Count", 0);
            if (count == 0) {
                return;
            }
            for (int i = 1; i <= count; i++) {
                // split the text to get the def value and def name
                //    (0) = group
                //    (1) = index
                //    (2) = newname
                string[] define = propfile.GetSetting("ResDefOverrides", "Override" + i, "").Split(":");
                if (define.Length == 3) {
                    if (!int.TryParse(define[0], out int group)) {
                        continue;
                    }
                    if (!int.TryParse(define[1], out int index)) {
                        continue;
                    }
                    define[2] = define[2].Trim();
                    if (define[2].Length == 0) {
                        continue;
                    }
                    // get the new name, if a valid entry
                    if (index < ByGroup((ResDefGroup)group).Length) {
                        SetResDef((ResDefGroup)group, index, define[2]);
                    }
                }
            }
            // need to make sure there aren't have any bad overrides (where overridden name
            // matches another name); if a duplicate is found, just reset the name back to
            // its default value
            // check AFTER all overrides are made just in case a swap is desired- checking in
            // realtime would not allow a swap
            if (!ValidateResDefs()) {
                // if any were changed back to default re-write the prop file
                SaveResDefOverrides();
            }
        }

        public void SaveResDefOverrides() {
            // if any reserved define names are different from the default values,
            // write them to the app settings;
            int count = 0;
            Define[] dfTemp;
            // need to make string comparisons case sensitive, in case user
            // wants to change case of a define (even though it really doesn't matter; compiler is not case sensitive)

            // first, delete any previous overrides
            propfile.DeleteSection("ResDefOverrides");
            // now step through each type of define value; if name is not the default, then save it
            for (ResDefGroup grp = 0; grp <= ResDefGroup.GameInfo; grp++) {
                dfTemp = ByGroup(grp);
                for (int i = 0; i < dfTemp.Length; i++) {
                    if (dfTemp[i].DefaultName != dfTemp[i].Name) {
                        // save it
                        count++;
                        propfile.WriteSetting("ResDefOverrides", "Override" + count, (int)grp + ":" + i + ":" + dfTemp[i].Name);
                    }
                }
            }
            // write the count value
            propfile.WriteSetting("ResDefOverrides", "Count", count.ToString());
        }

        /// <summary>
        /// Returns the reserved defines that match the specified argument
        /// type as an array of defines. NOT the same as returning by 'group'
        /// (which is used for saving changes to reserved define names).
        /// </summary>
        /// <param name="ArgType"></param>
        /// <returns></returns>
        public Define[] ByArgType(ArgType ArgType) {
            Define[] tmpDefines = [];

            switch (ArgType) {
            case Num:
                // return all numerical reserved defines
                tmpDefines = [];
                tmpDefines = [.. tmpDefines, .. agEdgeCodes];
                tmpDefines = [.. tmpDefines, .. agObjDirs];
                tmpDefines = [.. tmpDefines, .. agVideoModes];
                tmpDefines = [.. tmpDefines, .. agCompTypes];
                tmpDefines = [.. tmpDefines, .. agColorNames];
                // number of items
                Array.Resize(ref tmpDefines, tmpDefines.Length + 1);
                tmpDefines[^1] = agGameInfo[3];
                break;
            case Var:
                // return all variable reserved defines
                tmpDefines = agResVar;
                break;
            case Flag:
                // return all flag reserved defines
                tmpDefines = agResFlag;
                break;
            case MsgNum:
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
                // gameid, gameversion, gameabout
                tmpDefines = new Define[3];
                tmpDefines[0] = agGameInfo[0];
                tmpDefines[1] = agGameInfo[1];
                tmpDefines[2] = agGameInfo[2];
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
        public Define[] ByGroup(ResDefGroup Group) {
            switch (Group) {
            case ResDefGroup.Variable:
                return agResVar;
            case ResDefGroup.Flag:
                return agResFlag;
            case ResDefGroup.EdgeCode:
                return agEdgeCodes;
            case ResDefGroup.ObjectDir:
                return agObjDirs;
            case ResDefGroup.VideoMode:
                return agVideoModes;
            case ResDefGroup.ComputerType:
                return agCompTypes;
            case ResDefGroup.Color:
                return agColorNames;
            case ResDefGroup.Object:
                return agResObj;
            case ResDefGroup.String:
                return agResStr;
            case ResDefGroup.GameInfo:
                return agGameInfo;
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
        /// <param name="group"></param>
        /// <param name="index"></param>
        /// <param name="newname"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void SetResDef(ResDefGroup group, int index, string newname) {

            switch (group) {
            case ResDefGroup.Variable:
                // variable: value must be 0-26
                if (index < 0 || index > 27) {
                    throw new IndexOutOfRangeException("ReservedDefineList.Variable");
                }
                agResVar[index].Name = newname;
                break;
            case ResDefGroup.Flag:
                // flag: value must be 0-17
                if (index < 0 || index > 17) {
                    throw new IndexOutOfRangeException("ReservedDefineList.Flag");
                }
                agResFlag[index].Name = newname;
                break;
            case ResDefGroup.EdgeCode:
                // edgecode: value must be 0-4
                if (index < 0 || index > 4) {
                    throw new IndexOutOfRangeException("ReservedDefineList.EdgeCode");
                }
                agEdgeCodes[index].Name = newname;
                break;
            case ResDefGroup.ObjectDir:
                // direction: value must be 0-8
                if (index < 0 || index > 8) {
                    throw new IndexOutOfRangeException("ReservedDefineList.ObjectDir");
                }
                agObjDirs[index].Name = newname;
                break;
            case ResDefGroup.VideoMode:
                // video: value must be 0-4
                if (index < 0 || index > 4) {
                    throw new IndexOutOfRangeException("ReservedDefineList.VideoMode");
                }
                agVideoModes[index].Name = newname;
                break;
            case ResDefGroup.ComputerType:
                // computer: value must be 0-8
                if (index < 0 || index > 8) {
                    throw new IndexOutOfRangeException("ReservedDefineList.ComputerType");
                }
                agCompTypes[index].Name = newname;
                break;
            case ResDefGroup.Color:
                // color: value must be 0-15
                if (index < 0 || index > 15) {
                    throw new IndexOutOfRangeException("ReservedDefineList.Color");
                }
                agColorNames[index].Name = newname;
                break;
            case ResDefGroup.Object:
                // only 0 (ego)
                if (index != 0) {
                    throw new IndexOutOfRangeException("ReservedDefineList.Object");
                }
                agResObj[index].Name = newname;
                break;
            case ResDefGroup.String:
                // only 0 (input prompt)
                if (index != 0) {
                    throw new IndexOutOfRangeException("ReservedDefineList.String");
                }
                agResStr[index].Name = newname;
                break;
            case ResDefGroup.GameInfo:
                // index must be 0-3
                if (index < 0 || index > 15) {
                    throw new IndexOutOfRangeException("ReservedDefineList.GameInfo");
                }
                agGameInfo[index].Name = newname;
                break;
            }
            IsChanged = true;
        }

        /// <summary>
        /// This method checks all reserved defines to confirm they are all valid. Any
        /// invalid define names are replaced with their defaults.
        /// </summary>
        /// <returns>true if all defines are OK, false if one or  more are invalid.</returns>
        public bool ValidateResDefs() {
            // assume OK
            bool retval = true;
            int i;

            for (i = 0; i < agResVar.Length; i++) {
                if (ValidateReservedName(agResVar[i], ResDefGroup.Variable, i) != DefineNameCheck.OK) {
                    agResVar[i].Name = agResVar[i].DefaultName;
                    retval = false;
                }
            }
            for (i = 0; i < agResFlag.Length; i++) {
                if (ValidateReservedName(agResFlag[i], ResDefGroup.Flag, i) != DefineNameCheck.OK) {
                    agResFlag[i].Name = agResFlag[i].DefaultName;
                    retval = false;
                }
            }
            for (i = 0; i < agEdgeCodes.Length; i++) {
                if (ValidateReservedName(agEdgeCodes[i], ResDefGroup.EdgeCode, i) != DefineNameCheck.OK) {
                    agEdgeCodes[i].Name = agEdgeCodes[i].DefaultName;
                    retval = false;
                }
            }
            for (i = 0; i < agObjDirs.Length; i++) {
                if (ValidateReservedName(agObjDirs[i], ResDefGroup.ObjectDir, i) != DefineNameCheck.OK) {
                    agObjDirs[i].Name = agObjDirs[i].DefaultName;
                    retval = false;
                }
            }
            for (i = 0; i < agVideoModes.Length; i++) {
                if (ValidateReservedName(agVideoModes[i], ResDefGroup.VideoMode, i) != DefineNameCheck.OK) {
                    agVideoModes[i].Name = agVideoModes[i].DefaultName;
                    retval = false;
                }
            }
            for (i = 0; i < agCompTypes.Length; i++) {
                if (ValidateReservedName(agCompTypes[i], ResDefGroup.ComputerType, i) != DefineNameCheck.OK) {
                    agCompTypes[i].Name = agCompTypes[i].DefaultName;
                    retval = false;
                }
            }
            for (i = 0; i < agColorNames.Length; i++) {
                if (ValidateReservedName(agColorNames[i], ResDefGroup.Color, i) != DefineNameCheck.OK) {
                    agColorNames[i].Name = agColorNames[i].DefaultName;
                    retval = false;
                }
            }
            for (i = 0; i < agResObj.Length; i++) {
                if (ValidateReservedName(agResObj[i], ResDefGroup.Object, i) != DefineNameCheck.OK) {
                    agResObj[i].Name = agResObj[i].DefaultName;
                    retval = false;
                }
            }
            for (i = 0; i < agResStr.Length; i++) {
                if (ValidateReservedName(agResStr[i], ResDefGroup.String, i) != DefineNameCheck.OK) {
                    agResStr[i].Name = agResStr[i].DefaultName;
                    retval = false;
                }
            }
            for (i = 0; i < agGameInfo.Length; i++) {
                if (ValidateReservedName(agGameInfo[i], ResDefGroup.GameInfo, i) != DefineNameCheck.OK) {
                    agGameInfo[i].Name = agGameInfo[i].DefaultName;
                    retval = false;
                }
            }
            return retval;
        }

        public DefineNameCheck ValidateReservedName(Define CheckDef, ResDefGroup group, int index) {
            // if already at default, just exit
            if (CheckDef.Name == CheckDef.DefaultName) {
                return DefineNameCheck.OK;
            }
            // basic checks
            bool sierrasyntax = parent is not null && parent.SierraSyntax;
            DefineNameCheck retval = BaseNameCheck(CheckDef.Name, sierrasyntax);
            if (retval != DefineNameCheck.OK) {
                return retval;
            }
            if (index >= 0) {
                for (int i = 0; i < 10; i++) {
                    Define[] tmpDefines = ByGroup((ResDefGroup)i);
                    for (int j = 0; j < tmpDefines.Length; j++) {
                        if ((ResDefGroup)i == group && index == j) {
                            continue;
                        }
                        if (CheckDef.Name == tmpDefines[j].Name) {
                            switch ((ResDefGroup)i) {
                            case ResDefGroup.Variable:
                                return DefineNameCheck.ReservedVar;
                            case ResDefGroup.Flag:
                                return DefineNameCheck.ReservedFlag;
                            case ResDefGroup.EdgeCode:
                            case ResDefGroup.ObjectDir:
                            case ResDefGroup.VideoMode:
                            case ResDefGroup.ComputerType:
                            case ResDefGroup.Color:
                                return DefineNameCheck.ReservedNum;
                            case ResDefGroup.Object:
                                return DefineNameCheck.ReservedObj;
                            case ResDefGroup.String:
                                return DefineNameCheck.ReservedStr;
                            case ResDefGroup.GameInfo:
                                return DefineNameCheck.ReservedGameInfo;
                            }
                        }
                    }
                }
            }
            // if no error conditions, it's OK
            return DefineNameCheck.OK;
        }

        public DefineNameCheck ValidateReservedName(Define CheckDef) {
            return ValidateReservedName(CheckDef, 0, -1);
        }

        /// <summary>
        /// This method updates the reserved defines file for the specified game.
        /// </summary>
        public void SaveList(bool force) {

            if (parent is not null && !parent.IncludeReserved) {
                // don't save if not auto-including
                return;
            }
            // only need to update if file is missing, OR if any of the defines
            // have changed, OR if game properties have changed OR if force is true
            if (!File.Exists(filename) || force || IsChanged) {
                List<string> resList = [
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
                for (int i = 0; i < agObjDirs.Length; i++) {
                    resList.Add("#define " + agObjDirs[i].Name.PadRight(17) + agObjDirs[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Video Modes");
                for (int i = 0; i < agVideoModes.Length; i++) {
                    resList.Add("#define " + agVideoModes[i].Name.PadRight(17) + agVideoModes[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Computer Types");
                for (int i = 0; i < agCompTypes.Length; i++) {
                    resList.Add("#define " + agCompTypes[i].Name.PadRight(17) + agCompTypes[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Colors");
                for (int i = 0; i < agColorNames.Length; i++) {
                    resList.Add("#define " + agColorNames[i].Name.PadRight(17) + agColorNames[i].Value.PadLeft(5));
                }
                resList.Add("");
                resList.Add("[ Other Defines");
                resList.Add("#define " + agResObj[0].Name.PadRight(17) + agResObj[0].Value.PadLeft(5));
                resList.Add("#define " + agResStr[0].Name.PadRight(17) + agResStr[0].Value.PadLeft(5));
                resList.Add("");
                resList.Add("[ Game Properties");
                agGameInfo[0].Value = parent.agGameID;
                agGameInfo[1].Value = parent.agGameVersion;
                agGameInfo[2].Value = parent.agGameAbout;
                bool loaded = parent.InvObjects.Loaded;
                if (!loaded) {
                    parent.InvObjects.Load();
                }
                agGameInfo[3].Value = parent.InvObjects.Count.ToString();
                if (!loaded) {
                    parent.InvObjects.Unload();
                }
                for (int i = 0; i < agGameInfo.Length; i++) {
                    if (agGameInfo[i].Type == ArgType.DefStr) {
                        resList.Add("#define " + agGameInfo[i].Name.PadRight(17) + '"' + agGameInfo[i].Value + '"');
                    }
                    else {
                        resList.Add("#define " + agGameInfo[i].Name.PadRight(17) + agGameInfo[i].Value.PadLeft(5));
                    }
                }
                // save defines file
                try {
                    using FileStream fsList = new FileStream(filename, FileMode.Create);
                    using StreamWriter swList = new StreamWriter(fsList);
                    foreach (string line in resList) {
                        swList.WriteLine(line);
                    }
                }
                catch {
                    // ignore errors for now
                }
                IsChanged = false;
            }
        }

        public Define[] All() {
            // join all define lists into one
            Define[] tmpDefines = [];
            tmpDefines = [.. tmpDefines, .. agResVar];
            tmpDefines = [.. tmpDefines, .. agResFlag];
            tmpDefines = [.. tmpDefines, .. agResObj];
            tmpDefines = [.. tmpDefines, .. agResStr];
            tmpDefines = [.. tmpDefines, .. agEdgeCodes];
            tmpDefines = [.. tmpDefines, .. agObjDirs];
            tmpDefines = [.. tmpDefines, .. agVideoModes];
            tmpDefines = [.. tmpDefines, .. agCompTypes];
            tmpDefines = [.. tmpDefines, .. agColorNames];
            tmpDefines = [.. tmpDefines, .. agGameInfo];
            return tmpDefines;
        }
        #endregion
    }
}
