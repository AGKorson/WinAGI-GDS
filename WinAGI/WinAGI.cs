using System;
using System.IO;
using System.Linq;
using System.Text;
using static WinAGI.Common.Base;
using System.Diagnostics;

namespace WinAGI.Engine {
    /***************************************************************
    WinAGI Game Engine
    Copyright (C) 2005 - 2024 Andrew Korson

    This program is free software; you can redistribute it and/or 
    modify it under the terms of the GNU General Public License as
    published by the Free Software Foundation; either version 2 of
    the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public
    License along with this program; if not, write to the Free
    Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston,
    MA  02110-1301  USA
    ***************************************************************/

    #region Enums
    public enum AGIResType {
        Logic = 0,
        Picture = 1,
        Sound = 2,
        View = 3,
        Objects = 4,
        Words = 5,
        Layout = 6,
        Menu = 7,
        Globals = 8,
        Game = 9,
        Text = 10,
        Warnings = 11,
        TextScreen = 12,
        TODOEntry = 99, //refactor error/warnings functions
        DecompWarn = 100, // add decompile warnings to warning list
        None = 255
    };

    public enum AGIColorIndex {
        agBlack,
        agBlue,
        agGreen,
        agCyan,
        agRed,
        agMagenta,
        agBrown,
        agLtGray,
        agDkGray,
        agLtBlue,
        agLtGreen,
        agLtCyan,
        agLtRed,
        agLtMagenta,
        agYellow,
        agWhite,
        agNone
    };

    public enum ObjDirection {
        odStopped,
        odUp,
        odUpRight,
        odRight,
        odDownRight,
        odDown,
        odDownLeft,
        odLeft,
        odUpLeft
    };

    public enum EPlotShape {
        psCircle,
        psRectangle
    };

    public enum EPlotStyle {
        psSolid,
        psSplatter
    };

    public enum DrawFunction {
        dfEnableVis = 0xf0,    // Change picture color and enable picture draw.
        dfDisableVis = 0xF1,   // Disable picture draw.
        dfEnablePri = 0xF2,    // Change priority color and enable priority draw.
        dfDisablePri = 0xF3,   // Disable priority draw.
        dfYCorner = 0xF4,      // Draw a Y corner.
        dfXCorner = 0xF5,      // Draw an X corner.
        dfAbsLine = 0xF6,      // Absolute line (long lines).
        dfRelLine = 0xF7,      // Relative line (short lines).
        dfFill = 0xF8,         // Fill.
        dfChangePen = 0xF9,    // Change pen size and style.
        dfPlotPen = 0xFA,      // Plot with pen.
        dfEnd = 0xFF           // end of drawing
    };

    public enum LogicErrorLevel {
        Low,     // only errors that prevent compilation/decompiliation
                   // are passed; no warnings are given
        Medium,  // only errors that prevent compilation/decompilation
                   // are passed; warnings embedded in
                   // source code on compilation
        High,    // all compile/decompile problems are returned as errors
    };

    public enum ECStatus { 
        // used to update editor as components are completed,
        csCompWords,
        csCompObjects,
        csAddResource,
        csDoneAdding,
        csCompileComplete,
        csWarning,
        csResError,
        csLogicCompiled,
        csLogicError,
        csCanceled
    };

    public enum SoundFormat {
        sfUndefined,
        sfAGI,    // native agi format
        sfWAV,    // only IIgs pcm sounds can be exported as wav
        sfMIDI,   // only pc and IIgs can be saved as midi
        sfScript, // only pc can be exported as script
    };

    public enum ArgTypeEnum {
        atNum = 0,          // i.e. numeric Value
        atVar = 1,          // v##
        atFlag = 2,         // f##
        atMsg = 3,          // m##
        atSObj = 4,         // o##
        atInvItem = 5,      // i##
        atStr = 6,          // s##
        atWord = 7,         // w## -- word argument (that user types in)
        atCtrl = 8,         // c##
        atDefStr = 9,       // defined string; could be msg, inv obj, or vocword
        atVocWrd = 10,      // vocabulary word; NOT word argument
        atActionCmd = 11,   // action command synonym
        atTestCmd = 12,     // test command synonym
        atObj = 13,         // dual object type for Sierra syntax
        atView = 14         // view type is number, only in Sierra syntax
        // if using Sierra syntax, only valid types are:
        //   atNum, atVar, atFlag, atDefStr[msg only], atVocWrd,
        //   atActionCmd, atTestCmd, atObj, atView
    }

    public enum PlatformTypeEnum {
        None,
        DosBox,
        ScummVM,
        NAGI,
        Other
    }

    public enum ResDefGroup {
        rgVariable,
        rgFlag,
        rgEdgeCode,
        rgObjectDir,
        rgVideoMode,
        rgComputerType,
        rgColor,
        rgObject,
        rgString,
    }

    public enum DefineNameCheck {
        ncOK,            // 0 = name is valid
        ncEmpty,         // 1 = no name
        ncNumeric,       // 2 = name is numeric
        ncActionCommand, // 3 = name is command
        ncTestCommand,   // 4 = name is test command
        ncKeyWord,       // 5 = name is a compiler keyword
        ncArgMarker,     // 6 = name is an argument marker
        ncBadChar,       // 7 = name contains improper character
        ncGlobal,        // 8 = name is already globally defined
        ncReservedVar,   // 9 = name is reserved variable name
        ncReservedFlag,  // 10 = name is reserved flag name
        ncReservedNum,   // 11 = name is reserved number constant
        ncReservedObj,   // 12 = name is reserved object
        ncReservedStr,   // 13 = name is reserved string
        ncReservedMsg,   // 14 = name is reserved message
    }

    public enum DefineValueCheck {
        vcOK,           // 0 = value is valid
        vcEmpty,        // 1 = no Value
        vcOutofBounds,  // 2 = Value is not byte(0-255) or marker value is not byte
        vcBadArgNumber, // 3 = Value contains an invalid argument Value (controller, string, word)
        vcNotAValue,    // 4 = Value is not a string, number or argument marker
        vcReserved,     // 5 = Value is already defined by a reserved name
        vcGlobal,       // 6 = Value is already defined by a global name
    }

    public enum OpenGameMode {
        File,
        Directory,
    }

    public enum EventType {
        etInfo,
        etError,
        etWarning,
        etTODO
    }

    public enum EInfoType { //used to update editor during a game load
        itInitialize,
        itValidating,     //add check for dirty source code
        itPropertyFile,
        itResources,
        itDecompiling,    // decompiling logics during an import
        itCheckCRC,       // checking CRCs during load
        itFinalizing,
        itDone            // pass-back value indicating all is well
    };

    #endregion

    #region Structures  
    public struct PenStatus {
        public AGIColorIndex VisColor;
        public AGIColorIndex PriColor;
        public EPlotShape PlotShape;
        public EPlotStyle PlotStyle;
        public int PlotSize;
    }

    public struct AGIWord {
        public string WordText;
        public int Group;
    }

    public struct FreeSpaceInfo {
        public byte VOL;
        public int Start;
        public int End;
    }

    /// <summary>
    /// structure used for defined names
    /// </summary>
    public struct TDefine {
        public string Name;
        public string Default; //for reserved, this is default name; not used for other defines
        public string Value;
        public ArgTypeEnum Type;
        public string Comment;
    }

    public struct TWinAGIEventInfo {
        public EventType Type;              // type of data being reported - error/warning/info/TODO
        public EInfoType InfoType;          // sub-type for game load warnings ??? is this even needed anymore???
        public AGIResType ResType;          // resource type, if applicable to warning or error
        public byte ResNum;                 // resource number for logics,pics,views,sounds
        public string ID;                   // warning or error number (could be alphanumeric)
        public string Text;                 // info, warning or error msg
        public string Line;                 // line number for comp/decomp errors and warnings
        public string Module;               // module name, if comp error occurs in an #include file
    }

    public struct CommandStruct {
        public string Name;
        public ArgTypeEnum[] ArgType; //7
    }
    #endregion

    /// <summary>
    /// This is the WinAGI Base class. It sets up the WinAGI engine
    /// and initializes supporting elements.
    /// </summary>
    public static partial class Base {
        #region Local Members
        public const int WINAGI_ERR = 0x100000;
        public static readonly string[] ResTypeAbbrv = ["LOG", "PIC", "SND", "VIEW"];
        public static readonly string[] IntVersions =
        [
        "2.089", "2.272", "2.411", "2.425", "2.426", "2.435", "2.439",
        "2.440", "2.903", "2.911", "2.912", "2.915", "2.917", "2.936",
        "3.002086", "3.002098", "3.002102", "3.002107", "3.002149"
        ];
        internal const int MAX_RES_SIZE = 65530;
        internal const int MAX_LOOPS = 255;
        internal const int MAX_CELS = 255;
        internal const int MAX_ITEMS = 256;
        internal const int MAX_VOL_FILES = 65;
        internal const int MAX_CEL_WIDTH = 160;
        internal const int MAX_CEL_HEIGHT = 168;
        internal const int MAX_GROUP_NUM = 65535;
        internal const int MAX_WORD_GROUPS = 65535;
        internal const int MAX_VOLSIZE = 1047552; // '= 1024 * 1023
        internal const string WINAGI_VERSION = "3.0";
        internal static readonly byte[] bytEncryptKey;
        // member values
        internal static EGAColors defaultColorEGA = new();
        internal static string agTemplateDir = "";
        internal static string agDefResDir = "";
        internal static int defMaxVol0 = MAX_VOLSIZE;
        #endregion

        #region Constructors
        /// <summary>
        /// WinAGI Base constructor.
        /// </summary>
        static Base() {
            // this makes the codepages used in WinAGI available
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            bytEncryptKey = Encoding.GetEncoding(437).GetBytes("Avis Durgan");
            defaultColorEGA = new();
            InitWinAGI();
        }
        #endregion

        #region Base Properties
        /// <summary>
        /// Gets or sets the default colors that are used to display pictures and views in AGI.
        /// </summary>
        public static EGAColors DefaultColors {
            get { return defaultColorEGA; }
            internal set { defaultColorEGA = value; }
        }

        /// <summary>
        /// Gets or sets the default resource directory used for storing logic source files and stand alone resource files in a game.
        /// </summary>
        public static string DefResDir {
            get { return agDefResDir; }
            set {
                string NewDir = value;

                NewDir = NewDir.Trim();

                if (NewDir.Length == 0) {
                    throw new ArgumentOutOfRangeException("DefResDir", "Empty string not allowed");
                }
                if (NewDir.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
                    throw new ArgumentOutOfRangeException("Invalid characters in path name");
                }
                agDefResDir = NewDir;
            }
        }

        /// <summary>
        /// Gets or sets the default maximum size for VOL.0 when a game is compiled or rebuilt.
        /// </summary>
        public static int DefMaxVol0Size {
            get { return defMaxVol0; }
            set {
                if (value < 32768) {
                    defMaxVol0 = 32768;
                }
                else if (value >= MAX_VOLSIZE) {
                    defMaxVol0 = MAX_VOLSIZE;
                }
                else {
                    defMaxVol0 = value;
                }
            }
        }

        #endregion

        #region Base Methods
        /// <summary>
        /// Handles initialization of the WinAGI base class when it is instantiated. 
        /// </summary>
        private static void InitWinAGI() {
            // TEMP CHECK - verify the string resource file is still working correctly
            try {
                Debug.Print(LoadResString(505));
            }
            catch (Exception e) {
                Debug.Assert(false);
            }

            // calling this forces the module to load and initialize
            LogicCompiler.AssignReservedDefines();
        }

        /// <summary>
        /// This method attempts to extract interpreter version by examining the 
        /// AGIDATA.OVL file, if present.
        /// </summary>
        /// <param name="gameDir"></param>
        /// <param name="isV3"></param>
        /// <returns>version number found in AGIDATA.OVL<br />
        ///  default value (2.917 or 3.002149) if not found</returns>
        internal static string GetIntVersion(string gameDir, bool isV3) {
            // this function gets the version number of a Sierra AGI game
            // if found, it is validated against list of versions
            // that WinAGI recognizes
            byte[] bytBuffer = [0];

            string strFileName = gameDir + "AGIDATA.OVL";
            if (File.Exists(strFileName)) {
                try {
                    using FileStream fsVer = new(strFileName, FileMode.Open);
                    bytBuffer = new byte[fsVer.Length];
                    fsVer.Read(bytBuffer, 0, (int)fsVer.Length);
                    fsVer.Dispose();
                }
                catch (Exception) {
                    // invalid - return a default
                }
            }
            // if no data (either no file, or bad data
            if (bytBuffer.Length == 0) {
                // no agidata.ovl, use default
                if (isV3) {
                    return "3.002149";
                }
                else {
                    return "2.917";
                }
            }
            // go until a '2' or '3' is found, then look for rest of version string
            for (long pos = 0; pos >= bytBuffer.Length; pos++) {
                string strVersion;
                int i;
                if (bytBuffer[pos] == 50 && !isV3) {
                    // 2.xxx
                    strVersion = "2";
                    // get next four chars
                    for (i = 1; i <= 4; i++) {
                        pos++;
                        if (pos >= bytBuffer.Length) {
                            break;
                        }
                        strVersion += bytBuffer[pos].ToString();
                    }
                    if (IntVersions.Contains(strVersion)) {
                        return strVersion;
                    }
                }
                else if (bytBuffer[pos] == 51 && isV3) {
                    // 3.xxx.xxx format (for easier manipulation, the second '.' is
                    // removed, so result can be converted to a single precision number)
                    strVersion = "3";
                    //get next seven chars
                    for (i = 1; i <= 7; i++) {
                        pos++;
                        if (pos >= bytBuffer.Length) {
                            break;
                        }
                        // add this char (unless it's the second period)
                        if (pos != 4) {
                            strVersion += bytBuffer[pos].ToString();
                        }
                    }
                    if (IntVersions.Contains(strVersion)) {
                        return strVersion;
                    }
                }
            }
            // if version info not found in AGIDATA.OVL, return default
            if (isV3) {
                return "3.002149";
            }
            else {
                return "2.917";
            }
        }
    }
    #endregion
}