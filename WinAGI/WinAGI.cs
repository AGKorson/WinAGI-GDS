using System;
using System.IO;
using System.Linq;
using System.Text;
using static WinAGI.Common.Base;
using System.Diagnostics;
using WinAGI.Common;
using System.Diagnostics.Eventing.Reader;

namespace WinAGI.Engine {
    /***************************************************************
    WinAGI Game Engine
    Copyright (C) 2005 - 2025 Andrew Korson

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
        TextScreen = 11,
        None = 255
    };

    public enum AGIColorIndex {
        Black,
        Blue,
        Green,
        Cyan,
        Red,
        Magenta,
        Brown,
        LtGray,
        DkGray,
        LtBlue,
        LtGreen,
        LtCyan,
        LtRed,
        LtMagenta,
        Yellow,
        White,
        None
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

    public enum PlotShape {
        Circle,
        Rectangle
    };

    public enum PlotStyle {
        Solid,
        Splatter
    };

    public enum DrawFunction {
        EnableVis = 0xf0,    // Change picture color and enable picture draw.
        DisableVis = 0xF1,   // Disable picture draw.
        EnablePri = 0xF2,    // Change priority color and enable priority draw.
        DisablePri = 0xF3,   // Disable priority draw.
        YCorner = 0xF4,      // Draw a Y corner.
        XCorner = 0xF5,      // Draw an X corner.
        AbsLine = 0xF6,      // Absolute line (long lines).
        RelLine = 0xF7,      // Relative line (short lines).
        Fill = 0xF8,         // Fill.
        ChangePen = 0xF9,    // Change pen size and style.
        PlotPen = 0xFA,      // Plot with pen.
        End = 0xFF           // end of drawing
    };

    public enum LogicErrorLevel {
        Low,       // errors that prevent compilation/decompiliation
                   // are passed; minimal warnings are given
        Medium,    // errors that prevent compilation/decompilation
                   // are passed; warnings embedded in source
                   // code on compilation
    };

    public enum GameCompileStatus {
        // used to update editor as components are completed,
        CompileWords,
        CompileObjects,
        AddResource,
        DoneAdding,
        CompileComplete,
        Warning,
        ResError,
        LogicError,
        FatalError,
        Canceled
    };

    public enum SoundFormat {
        Undefined,
        AGI,    // native agi format
        WAV,    // only IIgs pcm sounds can be exported as wav
        MIDI,   // only pc and IIgs can be saved as midi
        Script, // only pc can be exported as script
    };

    public enum SoundPlaybackMode {
        PCSpeaker, // not implemented yet
        WAV,
        MIDI
    }
    
    public enum ArgType {
        Num = 0,          // i.e. numeric Value
        Var = 1,          // v##
        Flag = 2,         // f##
        Msg = 3,          // m##
        SObj = 4,         // o##
        InvItem = 5,      // i##
        Str = 6,          // s##
        Word = 7,         // w## -- word argument (that user types in)
        Ctrl = 8,         // c##
        DefStr = 9,       // defined string; could be msg, inv obj, or vocword
        VocWrd = 10,      // vocabulary word; NOT word argument
        ActionCmd = 11,   // action command synonym
        TestCmd = 12,     // test command synonym
        Obj = 13,         // dual object type for Sierra syntax
        View = 14,        // view type is number, only in Sierra syntax
        None = 15,        // used when a non-valid value is needed
        // if using Sierra syntax, only valid types are:
        //   Num, Var, Flag, DefStr[msg only], VocWrd,
        //   ActionCmd, TestCmd, Obj, View
    }

    public enum PlatformType {
        None,
        DosBox,
        ScummVM,
        NAGI,
        Other
    }

    public enum ResDefGroup {
        Variable,
        Flag,
        EdgeCode,
        ObjectDir,
        VideoMode,
        ComputerType,
        Color,
        Object,
        String,
        GameInfo,
    }

    public enum DefineNameCheck {
        OK,            // 0 = name is valid
        Empty,         // 1 = no name
        Numeric,       // 2 = name is numeric
        ActionCommand, // 3 = name is command
        TestCommand,   // 4 = name is test command
        KeyWord,       // 5 = name is a compiler keyword
        ArgMarker,     // 6 = name is an argument marker
        BadChar,       // 7 = name contains improper character
        Global,        // 8 = name is already globally defined
        ReservedVar,   // 9 = name is reserved variable name
        ReservedFlag,  // 10 = name is reserved flag name
        ReservedNum,   // 11 = name is reserved number constant
        ReservedObj,   // 12 = name is reserved object
        ReservedStr,   // 13 = name is reserved string
        ReservedMsg,   // 14 = name is reserved message
        ReservedGameInfo, // 15 = name is reserved gameinfo
        ResourceID,    // 16 = name is a resourceID
    }

    public enum DefineValueCheck {
        OK,           // 0 = value is valid
        Empty,        // 1 = no Value
        OutofBounds,  // 2 = Value is not byte(0-255) or marker value is not byte
        BadArgNumber, // 3 = Value contains an invalid argument Value (controller, string, word)
        NotAValue,    // 4 = Value is not a string, number or argument marker
        Reserved,     // 5 = Value is already defined by a reserved name
        Global,       // 6 = Value is already defined by a global name
    }

    public enum OpenGameMode {
        File,
        Directory,
        New,
    }

    public enum EventType {
        Info,
        GameLoadError,
        GameCompileError,
        LogicCompileError,
        LogicCompileWarning,
        ResourceError,
        ResourceWarning,
        DecompWarning,
        TODO
    }

    public enum InfoType {
        //used to update editor during a game load or compile
        Initialize,
        Validating,     //add check for changed source code
        PropertyFile,
        ClearWarnings,
        Resources,
        CheckLogic,
        Compiling,      // compiling a logic during game compilation
        Compiled,       // logic is successfully compiled
        Decompiling,    // decompiling logics during an import
        CheckCRC,       // checking CRCs during load
        Finalizing,
        Done            // pass-back value indicating all is well
    };

    public enum CompileStatus {
        OK,
        Canceled,
        Error,
    }

    #endregion

    #region Structures  
    public struct PenStatus {
        public AGIColorIndex VisColor;
        public AGIColorIndex PriColor;
        public PlotShape PlotShape;
        public PlotStyle PlotStyle;
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
        public string Name = "";
        public string Default = ""; //for reserved, this is default name; not used for other defines
        public string Value = "";
        public ArgType Type = ArgType.None;
        public string Comment = "";
        public DefineNameCheck NameCheck = DefineNameCheck.OK;
        public DefineValueCheck ValueCheck = DefineValueCheck.OK;
        public TDefine() {
        }
    }

    public struct TWinAGIEventInfo {
        public EventType Type = EventType.Info;              // type of data being reported - error/warning/info/TODO
        public InfoType InfoType = InfoType.Initialize;           // sub-type for game load warnings ??? is this even needed anymore???
        public AGIResType ResType = AGIResType.Game;          // resource type, if applicable to warning or error
        public byte ResNum = 0;                 // resource number for logics,pics,views,sounds
        public string ID = "";                   // warning or error number (could be alphanumeric)
        public string Text = "";                 // info, warning or error msg
        public string Line = "";                 // line number for comp/decomp errors and warnings
        public string Module = "";               // module filename, if comp error occurs in an #include file or resID
        public string Filename = "";             // full name of file, including path, if an include file
        public int Data = 0;                    // number field used for various data purposes
        public TWinAGIEventInfo() {

        }
    }

    public struct CommandStruct {
        public string Name;
        public ArgType[] ArgType;
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
        internal const string DEFRESDIR = "src";
        public const string WINAGI_VERSION = "3.0";
        internal static readonly byte[] bytEncryptKey;
        // member values
        internal static EGAColors defaultPalette = new();
        internal static string agTemplateDir = "";
        internal static string agDefResDir = DEFRESDIR;
        internal static int defMaxVol0 = MAX_VOLSIZE;
        internal static byte defMaxSO = 16;
        internal static Encoding defCodePage;
        internal static readonly int[] validcodepages = [437, 737, 775, 850, 852, 855, 857, 858, 860, 861, 862, 863, 865, 866, 869];
        public static ReservedDefineList DefaultReservedDefines;
        #endregion

        #region Constructors
        /// <summary>
        /// WinAGI Base constructor.
        /// </summary>
        static Base() {

            // TODO: why aren't these in InitWinAGI?
            // this makes the codepages used in WinAGI available
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            defCodePage = Encoding.GetEncoding(437);
            bytEncryptKey = Encoding.GetEncoding(437).GetBytes("Avis Durgan");
            InitWinAGI();
        }
        #endregion

        #region Base Properties
        /// <summary>
        /// Gets or sets the default colors that are used to display pictures and views in AGI.
        /// </summary>
        public static EGAColors DefaultPalette {
            get { return defaultPalette; }
            internal set { defaultPalette = value.CopyPalette(); }
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

        /// <summary>
        /// Gets or sets the default max screen object value for new OBJECT files.
        /// </summary>
        public static byte DefMaxSO {
            get { return defMaxSO; }
            set {
                if (value < 1) {
                    defMaxSO = 1;
                }
                else {
                    defMaxSO = value;
                }
            }
        }

        public static Encoding CodePage {
            get => defCodePage;
            set {
                // confirm new codepage is supported; error if it is not
                if (validcodepages.Contains(value.CodePage)) {
                    defCodePage = Encoding.GetEncoding(value.CodePage);
                }
                else {
                    throw new ArgumentOutOfRangeException("CodePage", "Unsupported or invalid CodePage value");
                }
            }
        }

        /// <summary>
        /// Gets an array of valid code page vales. These are the only codepages
        /// that WinAGI can support.
        /// </summary>
        public static int[] ValidCodePages {
            get => validcodepages;
        }
        #endregion

        #region Base Methods
        /// <summary>
        /// Handles initialization of the WinAGI base class when it is instantiated. 
        /// </summary>
        private static void InitWinAGI() {
            // TEMP CHECK - verify the string resource file is still working correctly
            try {
                Debug.Assert(LoadResString(505) == "Invalid resource location (%1) in %2.");
            }
            catch (Exception e) {
                Debug.Assert(false);
            }

            // calling this forces the module to load and initialize
            LogicCompiler.compGame = null;
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
        
        /// <summary>
        /// Error-safe method that provides an easy way to accss WinAGI.Engine
        /// string resources by number instead of a string key.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string LoadResString(int index) {
            try {
                return EngineResources.ResourceManager.GetString(index.ToString());
            }
            catch (Exception) {
                // return nothing if string doesn't exist
                return "";
            }
        }
    }
    #endregion
}