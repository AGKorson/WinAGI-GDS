using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace WinAGI.Engine {
    /***************************************************************
    WinAGI Game Engine
    Copyright (C) 2005 - 2026 Andrew Korson

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
        Include = 6,
        Layout = 7,
        Menu = 8,
        Globals = 9,
        Game = 10,
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
        Square
    };

    public enum PlotStyle {
        Solid,
        Splatter
    };

    public enum ResourceErrorType {
        NoError,
        FileNotFound,
        FileIsReadonly,
        FileAccessError,
        InvalidLocation,
        InvalidHeader,
        DecompressionError,
        LogicSourceIsReadonly,
        LogicSourceAccessError,
        LogicSourceDecompileError,
        SoundNoData,
        SoundBadTracks,
        SoundCantConvert,
        ViewNoData,
        ViewNoLoops,
        ObjectNoFile,
        ObjectIsReadOnly,
        ObjectAccessError,
        ObjectNoData,
        ObjectDecryptError,
        ObjectBadHeader,
        WordsTokNoFile,
        WordsTokIsReadOnly,
        WordsTokAccessError,
        WordsTokNoData,
        WordsTokBadIndex,
        DefinesNoFile,
        DefinesReadOnly,
        DefinesAccessError,
        DefinesLevelLimit,
        DefinesCircularReference,
        SierraResourceError,
    }

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
        //              // FAN syntax           SIERRA syntax
        Num,            // number(byte)         NUM
        Var,            // v##                  VAR
        Flag,           // f##                  FLAG
        MsgNum,         // m##                  MSGNUM
        SObj,           // o##                  --
        InvItem,        // i##                  --
        Str,            // s##                  --
        Word,           // w##                  --
        Ctrl,           // c##                  --
        DefStr,         // literal string       literal string
        VocWrd,         // number(word)         --
        ActionCmd,      // ActionCommand        #action
        TestCmd,        // TestCommand          #test
        Object,         // --                   OBJECT
        View,           // --                   VIEW
        MSG,            // --                   MSG (not implemented)
        WORD,           // --                   WORD (not implemented)
        ANY,            // --                   ANY (not implemented)
        WORDLIST,       // --                   WORDLIST
        SierraArgToken, //                      allowed Sierra argument token
        // these types are used during parsing only
        Symbol,
        AssignOperator,
        TestOperator,
        Separator,
        BadString,
        Keyword,
        Label,
        Comment,
        Preprocessor,
        None,           // no token - indicates end of input
        Unknown,        // used when type has not yet been determined
        BadSymbol,      // used for tokens starting with '&' or '|'
    }

    public enum PlatformType {
        None,
        DosBox,
        ScummVM,
        NAGI,
        AGILE,
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
        Reserved,     // 5 = Value is already defined by a reserved name (FAN syntax only)
        Global,       // 6 = Value is already defined by a global name (FAN syntax only)
    }

    public enum OpenGameMode {
        /// <summary>
        /// Opens an existing game using a WAG file
        /// </summary>
        File,
        /// <summary>
        /// Opens an existing game by importing AGI game files
        /// </summary>
        Directory,
        /// <summary>
        /// Creates and opens a new a game 
        /// </summary>
        New,
    }

    public enum EventType {
        Info,
        GameLoadError,
        GameCompileError,
        ResourceError,
        ResourceWarning,
        LogicCompileError,
        LogicCompileWarning,
        DecompError,
        DecompWarning,
        TODO
    }

    public enum InfoType {
        // used to update editor during a game load or compile
        Initialize,
        Validating,     // add check for changed source code
        PropertyFile,
        ClearWarnings,
        Resources,
        Compiling,      // compiling a logic during game compilation
        Compiled,       // logic is successfully compiled
        CompileError,   // logic failed to compile
        DecodingAllLogics, // decoding all logics during an import
        Decompiling,    // decompiling logics during an import
        Decompiled,     // logic is successfully decompiled
        CheckCRC,       // checking CRCs during load
        Finalizing,
        Done,           // pass-back value indicating all is well
    };

    public enum CompileStatus {
        OK,
        Canceled,
        ResourceError,
        LogicCompileError,
    }

    public enum AGIVersion {
        v2089,
        v2272,
        v2411,
        v2425,
        v2426,
        v2435,
        v2439,
        v2440,
        v2903,
        v2911,
        v2912,
        v2915,
        v2917,
        v2936,
        v3002086,
        v3002098,
        v3002102,
        v3002107,
        v3002149
    }
    #endregion

    #region Structures 
    [Serializable]
    public struct PenStatus {
        public AGIColorIndex VisColor = AGIColorIndex.None;
        public AGIColorIndex PriColor = AGIColorIndex.None;
        public PlotShape PlotShape = PlotShape.Circle;
        public PlotStyle PlotStyle = PlotStyle.Solid;
        public int PlotSize = 0;
        public PenStatus() {
        }
        public readonly byte PlotData => (byte)((int)PlotStyle * 0x20 +
                                (int)PlotShape * 0x10 +
                                PlotSize);
    }

    public struct AGIVersionInfo {
        public AGIVersion Index = AGIVersion.v2917;
        public string VersionString {
            get {
                return Base.IntVersions[(int)Index];
            }
        }
        public bool IsV3 {
            get {
                return Index >= AGIVersion.v3002086;
            }
        }

        public AGIVersionInfo() {
        }
    }

    public struct AGIWord {
        public string WordText;
        public int Group;
    }

    /// <summary>
    /// structure used for defined names
    /// </summary>
    public struct Define {
        public string Name = "";
        public string DefaultName = ""; // used by reserved and globals editors to track changes in name
        public string Value = "";
        public string DefaultValue = ""; // used by reserved and globals editors to track changes in value
        public int IntValue = 0;
        public ArgType Type = ArgType.None;
        public string Comment = "";
        public DefineNameCheck NameType = DefineNameCheck.OK;
        public DefineValueCheck ValueType = DefineValueCheck.OK;
        public int Line = 0; // line number in source file where defined
        public int UID; // unique ID for this define; only used by global defines editor

        public Define() {
        }
    }

    public struct WinAGIEventInfo {
        /// <summary>
        /// type of data being reported - error/warning/info/TODO
        /// </summary>
        public EventType Type = EventType.Info;
        /// <summary>
        /// sub-type for game load warnings
        /// </summary>
        public InfoType InfoType = InfoType.Initialize;
        /// <summary>
        /// resource type, if applicable to warning or error
        /// </summary>
        public AGIResType ResType = AGIResType.Game;
        /// <summary>
        /// resource number for logics,pics,views,sounds
        /// </summary>
        public int ResNum = 0;
        /// <summary>
        /// warning or error number (could be alphanumeric)
        /// </summary>
        public string ID = "";
        /// <summary>
        /// info, warning or error msg
        /// </summary>
        public string Text = "";
        /// <summary>
        /// line number for comp/decomp errors and warnings
        /// </summary>
        public int Line = -1;
        /// <summary>
        /// module filename, if comp error occurs in an #include file or resID
        /// </summary>
        public string Module = "";
        /// <summary>
        /// full name of file, including path, if an include file
        /// </summary>
        public string Filename = "";
        /// <summary>
        /// generic field used for various data purposes
        /// </summary>
        public object Data;
        public WinAGIEventInfo() {

        }
    }

    public struct CommandStruct {
        public string FanName = "";
        public ArgType[] ArgList;
        public CommandStruct() {
            ArgList = [];
        }
    }

    public struct PictureBackgroundSettings {
        public string FileName;
        public bool Visible;
        public bool ShowVis;
        public bool ShowPri;
        public byte Transparency;
        public bool DefaultAlwaysTransparent;
        public RectangleF SourceRegion;
        public Size SourceSize;
        public Point TargetPos;
        public PictureBackgroundSettings() {
            FileName = "";
            Transparency = 50;
            DefaultAlwaysTransparent = true;
            SourceRegion = new(0, 0, 0, 0);
            TargetPos = new(0, 0);
            SourceSize = new(320, 168);
            Visible = false;
            ShowVis = true;
            ShowPri = false;
        }
    }
    #endregion

    /// <summary>
    /// This is the WinAGI Base class. It sets up the WinAGI engine
    /// and initializes supporting elements.
    /// </summary>
    public static partial class Base {
        #region Fields
        // combine severity (unchecked((int)0x80000000); // bit 31
        //      CustomerBit (0x20000000;                 // bit 29
        // and     Facility (0x0001 << 16)               // bits 16..26
        // custom WinAGI errors are then unique 16 bit numbers, in bits 0..15
        public const int WINAGI_ERR = unchecked((int)0xA0010000);
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
        internal static bool defaultSierraSyntax = false;
        // member values
        internal static EGAColors defaultPalette = new();
        internal static string agTemplateDir = "";
        internal static string agDefResDir = DEFRESDIR;
        internal static int defMaxVol0 = MAX_VOLSIZE;
        internal static byte defMaxSO = 16;
        internal static int defCodePage = 437;
        internal static readonly int[] validcodepages = [437, 737, 775, 850, 852, 855, 857, 858, 860, 861, 862, 863, 865, 866, 869];
        public static ReservedDefineList DefaultReservedDefines;
        internal static List<string> AddedIncludes = [];
        // compiler warnings
        // 120 warnings, numbered 5001 to 5120 (index   0 to 119) for FAN syntax
        //  20 warnings, numbered 7001 to 7020 (index 120 to 139) for SIERRA syntax
        public const int WARNCOUNT = 140;
        public static bool[] NoCompWarn = new bool[WARNCOUNT];
        #endregion

        #region Constructors
        /// <summary>
        /// WinAGI Base constructor.
        /// </summary>
        static Base() {
            // this makes the codepages used in WinAGI available
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            bytEncryptKey = Encoding.GetEncoding(437).GetBytes("Avis Durgan");
        }
        #endregion

        #region Base Properties
        /// <summary>
        /// Gets or sets the default colors that are used to display pictures and views in AGI.
        /// </summary>
        public static EGAColors DefaultPalette {
            get {
                return defaultPalette;
            }
            internal set {
                defaultPalette = value.Clone();
            }
        }

        /// <summary>
        /// Gets or sets the default resource directory used for storing logic source
        /// files and stand alone resource files in a game.
        /// </summary>
        public static string DefResDir {
            get {
                return agDefResDir;
            }
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
            get {
                return defMaxVol0;
            }
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
            get {
                return defMaxSO;
            }
            set {
                if (value < 1) {
                    defMaxSO = 1;
                }
                else {
                    defMaxSO = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets how the compiler responds to errors and unusual conditions
        /// encountered in logic source code when compiling.
        /// </summary>
        public static LogicErrorLevel ErrorLevel {
            get; set;
        }

        public static int CodePage {
            get => defCodePage;
            set {
                // confirm new codepage is supported; error if it is not
                if (validcodepages.Contains(value)) {
                    defCodePage = value;
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
        /// This method attempts to extract interpreter version by examining the 
        /// AGIDATA.OVL file, if present.
        /// </summary>
        /// <param name="gameDir"></param>
        /// <param name="isV3"></param>
        /// <returns>version number found in AGIDATA.OVL<br />
        ///  default value (2.917 or 3.002149) if not found</returns>
        internal static AGIVersionInfo GetIntVersion(string gameDir, bool isV3) {
            // this function gets the version number of a Sierra AGI game
            // if found, it is validated against list of versions
            // that WinAGI recognizes
            AGIVersionInfo retval = new();
            byte[] bytBuffer = [0];

            string fileName = Path.Combine(gameDir, "AGIDATA.OVL");
            if (File.Exists(fileName)) {
                try {
                    using FileStream fsVer = new(fileName, FileMode.Open, FileAccess.Read);
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
                    retval.Index = AGIVersion.v3002149;
                }
                else {
                    retval.Index = AGIVersion.v2917;
                }
                return retval;
            }
            // go until a '2' or '3' is found, then look for rest of version string
            for (long pos = 0; pos >= bytBuffer.Length; pos++) {
                string version;
                int i;
                if (bytBuffer[pos] == 50 && !isV3) {
                    // 2.xxx
                    version = "2";
                    // get next four chars
                    for (i = 1; i <= 4; i++) {
                        pos++;
                        if (pos >= bytBuffer.Length) {
                            break;
                        }
                        version += bytBuffer[pos].ToString();
                    }
                    int verIndex = Array.IndexOf(IntVersions, version);
                    if (verIndex != -1) {
                        retval.Index = (AGIVersion)verIndex;
                        return retval;
                    }
                }
                else if (bytBuffer[pos] == 51 && isV3) {
                    // 3.xxx.xxx format
                    version = "3";
                    // get next seven chars
                    for (i = 1; i <= 7; i++) {
                        pos++;
                        if (pos >= bytBuffer.Length) {
                            break;
                        }
                        // add this char (unless it's the second period)
                        if (pos != 4) {
                            version += bytBuffer[pos].ToString();
                        }
                    }
                    int verIndex = Array.IndexOf(IntVersions, version);
                    if (verIndex != -1) {
                        retval.Index = (AGIVersion)verIndex;
                        return retval;
                    }
                }
            }
            // if version info not found in AGIDATA.OVL, return default
            if (isV3) {
                retval.Index = AGIVersion.v3002149;
            }
            else {
                retval.Index = AGIVersion.v2917;
            }
            return retval;
        }

        /// <summary>
        /// Error-safe method that provides an easy way to accss WinAGI.Engine
        /// string resources by number instead of a string key.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string EngineResourceByNum(int index) {
            string retval;
            try {
                retval = EngineResources.ResourceManager.GetString(index.ToString());
                if (retval is null) {
                    retval = "";
                }
            }
            catch (Exception) {
                // return nothing if error
                return "";
            }
            return retval;
        }

        /// <summary>
        /// Compresses the specified string using GZip and returns the result as a Base64-encoded string.
        /// </summary>
        /// <remarks>Throws ArgumentNullException if input is null. Uses GZip with
        /// CompressionLevel.Optimal. The output may be larger than the input for very small or incompressible
        /// data.</remarks>
        /// <param name="input">The string to compress and encode to Base64; encoded to UTF-8 prior to compression.</param>
        /// <returns>A Base64-encoded representation of the GZip-compressed UTF-8 bytes of the input.</returns>
        public static string CompressToBase64(string input) {
            var bytes = Encoding.UTF8.GetBytes(input);
            using var output = new MemoryStream();
            using (var gzip = new GZipStream(output, CompressionLevel.Optimal)) {
                gzip.Write(bytes, 0, bytes.Length);
            }
            return Convert.ToBase64String(output.ToArray());
        }

        /// <summary>
        /// Decodes a Base64-encoded string and decompresses it to a regular string.
        /// </summary>
        /// <param name="base64">The Base64-encoded string to decompress.</param>
        /// <returns>The decompressed string.</returns>
        public static string DecompressFromBase64(string base64) {
            var bytes = Convert.FromBase64String(base64);
            using var input = new MemoryStream(bytes);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// This method tells the logic compiler to ignore the specified warning
        /// if the condition is encountered while compiling.
        /// </summary>
        /// <param name="WarningNumber"></param>
        /// <param name="NewVal"></param>
        public static void SetIgnoreWarning(int WarningNumber, bool NewVal) {
            int index;
            if (WarningNumber > 7000) {
                // 7001 = index 120 .. 7007 = index 126
                index = WarningNumber - 7001 + 120;
            }
            else if (WarningNumber > 5000) {
                // 5001 = index 0 .. 5120 = index 5119
                index = WarningNumber - 5001;
            }
            else {
                Debug.Assert(false);
                return;
            }
            if (index < 0 || index >= NoCompWarn.Length) {
                Debug.Assert(false);
                return;
            }
            NoCompWarn[index] = NewVal;
        }

        /// <summary>
        /// Gets the value of the specified warning number's ignore status.
        /// </summary>
        /// <param name="WarningNumber"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static bool IgnoreWarning(int WarningNumber) {
            int index;
            if (WarningNumber > 7000) {
                // 7001 = index 120 .. 7017 = index 136
                index = WarningNumber - 7001 + 120;
            }
            else if (WarningNumber > 5000) {
                // 5001 = index 0 .. 5120 = index 5119
                index = WarningNumber - 5001;
            }
            else {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            if (index < 0 || index >= NoCompWarn.Length) {
                throw new IndexOutOfRangeException("subscript out of range");
            }
            return NoCompWarn[index];
        }

        /// <summary>
        /// Compiles the specified logic using the appropriate compiler based on its syntax.
        /// </summary>
        /// <param name="SourceLogic">The logic to compile.</param>
        /// <returns>True if compilation is successful, otherwise false.</returns>
        internal static bool CompileLogic(Logic SourceLogic) {
            if (SourceLogic.Parent.SierraSyntax) {
                return SierraLogicCompiler.CompileLogic(SourceLogic);
            }
            else {
                return FanLogicCompiler.CompileLogic(SourceLogic);
            }
        }
        #endregion
    }
}