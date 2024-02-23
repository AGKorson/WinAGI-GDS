using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static System.Windows.Forms.DataFormats;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;
namespace WinAGI.Engine
{
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;

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

    //enums 
    #region
    public enum AGIResType
    {
        rtLogic = 0,
        rtPicture = 1,
        rtSound = 2,
        rtView = 3,
        rtObjects = 4,
        rtWords = 5,
        rtLayout = 6,
        rtMenu = 7,
        rtGlobals = 8,
        rtGame = 9,
        rtText = 10,
        rtWarnings = 11,
        rtTextScreen = 12,
        rtTODOEntry = 99, //refactor error/warnings functions
        rtDecompWarn = 100, // add decompile warnings to warning list
        rtNone = 255
    };
    public enum AGIColorIndex
    {
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
    public enum ObjDirection
    {
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
    public enum EPlotShape
    {
        psCircle,
        psRectangle
    };
    public enum EPlotStyle
    {
        psSolid,
        psSplatter
    };
    public enum DrawFunction
    {
        dfEnableVis = 0xf0,   //Change picture color and enable picture draw.
        dfDisableVis = 0xF1,   //Disable picture draw.
        dfEnablePri = 0xF2,    //Change priority color and enable priority draw.
        dfDisablePri = 0xF3,   //Disable priority draw.
        dfYCorner = 0xF4,      //Draw a Y corner.
        dfXCorner = 0xF5,      //Draw an X corner.
        dfAbsLine = 0xF6,      //Absolute line (long lines).
        dfRelLine = 0xF7,      //Relative line (short lines).
        dfFill = 0xF8,         //Fill.
        dfChangePen = 0xF9,    //Change pen size and style.
        dfPlotPen = 0xFA,      //Plot with pen.
        dfEnd = 0xFF          //end of drawing
    };
    public enum LogicErrorLevel
    {
        leLow,     //only errors that prevent compilation/decompiliation
                   //are passed; no warnings are given
        leMedium,  //only errors that prevent compilation/decompilation
                   //are passed; warnings embedded in
                   //source code on compilation
        leHigh,    //all compile/decompile problems are returned as errors
    };
    public enum ECStatus
    { //used to update editor as components are completed,
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
    public enum ELStatus
    { //used to update editor during a game load
        lsInitialize,
        lsValidating, //add check for dirty source code
        lsPropertyFile,
        lsResources,
        lsDecompiling,// decompiling logics during an import
        lsCheckCRC,    // checking CRCs during load
        lsFinalizing,
        lsLoadWarning //non-critical error/warning encountered during loading
    };
    public enum ELoadWarningSource
    {
        lwNA,            // not applicable
        lwDIR,           // directory related error/warning
        lwVol,           // VOL related error/warning
        lwResource,      // resource related error
        lwOther          // any other error/warning
    }
    public enum SoundFormat
    {
        sfUndefined,
        sfAGI,    //native agi format (all)
        sfMIDI,   //only pc and IIgs can be saved as midi
        sfScript, //only pc can be exported as script
        sfWAV     //only IIgs pcm sounds can be exported as wav
    };
    public enum ArgTypeEnum
    {
        atNum = 0,      //i.e. numeric Value
        atVar = 1,      //v##
        atFlag = 2,     //f##
        atMsg = 3,      //m##
        atSObj = 4,     //o##
        atIObj = 5,     //i##
        atStr = 6,      //s##
        atWord = 7,     //w## -- word argument (that user types in)
        atCtrl = 8,     //c##
        atDefStr = 9,   //defined string; could be msg, inv obj, or vocword
        atVocWrd = 10,   //vocabulary word; NOT word argument
        atActionCmd = 11, //action command synonym
        atTestCmd = 12,  //test command synonym
        atObj = 13      // dual object type for Sierra Syntax

        //if using Sierra syntax, only valid types are:
        // atNum, atVar, atFlag,
        //atDefStr[msg only], atVocWrd, atActionCmd, atTestCmd
    }
    public enum ResDefGroup
    {
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
    public enum DefineNameCheck
    {
        ncOK,            // 0 = name is valid
        ncEmpty,         // 1 = no name
        ncNumeric,       // 2 = name is numeric
        ncActionCommand, // 3 = name is command
        ncTestCommand,   // 4 = name is test command
        ncKeyWord,       // 5 = name is a compiler keyword
        ncArgMarker,     // 6 = name is an argument marker
        ncGlobal,        // 7 = name is already globally defined
        ncReservedVar,   // 8 = name is reserved variable name
        ncReservedFlag,  // 9 = name is reserved flag name
        ncReservedNum,   // 10 = name is reserved number constant
        ncReservedObj,   // 11 = name is reserved object
        ncReservedStr,   // 12 = name is reserved string
        ncReservedMsg,   // 13 = name is reserved message
        ncBadChar,       // 14 = name contains improper character
    }
    public enum DefineValueCheck
    {
        vcOK,           // 0 = value is valid
        vcEmpty,        // 1 = no Value
                        // 2 = Value is an invalid argument marker (not used anymore]
        vcBadArgNumber, // 3 = Value contains an invalid argument Value (controller, string, out of bounds for example)
        vcNotAValue,    // 4 = Value is not a string, number or argument marker
        vcReserved,     // 5 = Value is already defined by a reserved name
        vcGlobal,       // 6 = Value is already defined by a global name
    }
    public enum OpenGameMode
    {
        File,
        Directory,
    }
    public enum EventType
    {
        ecLoadOK,
        ecCompOK,
        ecDecompOK,
        ecLoadError,
        ecCompError,
        ecDecompError,
        ecLoadWarn,
        ecCompWarn,
        ecDecompWarn,
        ecTODO,
    }
    #endregion
    //structs
    #region Structures  
    public struct PenStatus
    {
        public AGIColorIndex VisColor;
        public AGIColorIndex PriColor;
        public EPlotShape PlotShape;
        public EPlotStyle PlotStyle;
        public int PlotSize;
    }
    public struct AGIWord
    {
        public string WordText;
        public int Group;
    }
    public struct FreeSpaceInfo
    {
        public byte VOL;
        public int Start;
        public int End;
    }
    //type used for defined names
    public struct TDefine
    {
        public string Name;
        public string Default; //for reserved, this is default name; not used for other defines
        public string Value;
        public ArgTypeEnum Type;
        public string Comment;
    }

    public struct TWinAGIEventInfo
    {
        public EventType Type;              // type of data being reported - error/warning/info/TODO
        public AGIResType ResType;          // resource type, if applicable to warning or error
        public byte ResNum;                 // resource number for logics,pics,views,sounds
        public string ID;                   // warning or error number (could be alphanumeric)
        public string Text;                 // info, warning or error msg
        public ELoadWarningSource LWType;   // sub-type for game load warnings ??? is this even needed anymore???
        public int Line;                    // line number for comp/decomp errors and warnings
        public string Module;               // module name, if comp error occurs in an #include file
    }
        
        public struct CommandStruct
    {
        public string Name;
        public ArgTypeEnum[] ArgType; //7
    }
    #endregion
    //***************************************************
    //
    // the base class exposes all the static variables 
    // and methods that the engine uses
    //
    //***************************************************
    public static partial class Base
    {
        // arrays that hold constant values
        #region
        public static readonly string[] ResTypeAbbrv = { "LOG", "PIC", "SND", "VIEW" };
        public static readonly string[] ResTypeName = { "Logic", "Picture", "Sound", "View" };
        public static readonly string[] IntVersions =
          [ //load versions
        "2.089", "2.272", "2.411", "2.425", "2.426", "2.435", "2.439",
        "2.440", "2.903", "2.911", "2.912", "2.915", "2.917", "2.936",
        "3.002086", "3.002098", "3.002102", "3.002107", "3.002149"
      ];
        internal static readonly byte[] bytEncryptKey =
          [ (byte)'A', (byte)'v', (byte)'i', (byte)'s', (byte)' ',
        (byte)'D', (byte)'u', (byte)'r', (byte)'g', (byte)'a', (byte)'n'
      ]; //' = "Avis Durgan"
        #endregion
        //game constants
        #region
        internal const int MAX_RES_SIZE = 65530;
        internal const int MAX_LOOPS = 255;
        internal const int MAX_CELS = 255;
        internal const int MAX_ITEMS = 256;
        internal const int MAX_VOL_FILES = 65;
        internal const int MAX_CEL_WIDTH = 160;
        internal const int MAX_CEL_HEIGHT = 168;
        internal const int MAX_GROUP_NUM = 65535;
        internal const int MAX_WORD_GROUPS = 65535;
        internal const int MAX_VOLSIZE = 1047552;// '= 1024 * 1023
        internal const string WORD_SEPARATOR = " | ";
        //current version
        internal const string WINAGI_VERSION = "3.0";
        // error offset is public
        public const int WINAGI_ERR = 0x100000;
        #endregion
        //global variables
        #region
        internal static string agDefResDir = "";
        internal static int defMaxVol0 = MAX_VOLSIZE;
        internal static EGAColors defaultColorEGA = new();
        internal static string agSrcFileExt = ".lgc";

        //error number and string to return error values
        //from various functions/subroutines
        internal static int lngError = 0;
        internal static string strError = "";
        internal static string strErrSrc = "";
        #endregion
        //temp file location
        internal static string agTemplateDir = "";
        internal static string TempFileDir = "";

        public static EGAColors DefaultColors
        {
            get { return defaultColorEGA; }
            //set { colorEGA = value; }
        }
        public static string TemplateDir
        {
            get { return agTemplateDir; }
            set
            {
                // directory has to exist
                if (!Directory.Exists(value))
                    throw new Exception(LoadResString(630).Replace(ARG1, value));

                agTemplateDir = CDir(value);
            }
        }

        static Base()
        {
            // initialize all winagi stuff here? no, put it in a method that can be called
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        internal static void InitWinAGI()
        {
            // calling this forces the module to load and initialize
            ResetDefaultColors();
            CRC32Setup();
            Compiler.AssignReservedDefines();
        }

        public static string DefResDir
        {
            get { return agDefResDir; }
            set
            {
                string NewDir = value;

                NewDir = NewDir.Trim();

                if (NewDir.Length == 0) {
                    //throw new Exception("380, strErrSource, "Invalid property Value"
                    throw new ArgumentOutOfRangeException("DefResDir","Empty string not allowed");
                }
                if (NewDir.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
                    //throw new Exception("380, strErrSource, "Invalid property Value"
                    throw new ArgumentOutOfRangeException("Invalid characters in path name");
                }
                //save new resdir name
                agDefResDir = NewDir;
            }
        }

        public static int DefMaxVol0Size
        {
            get { return defMaxVol0; }
            set
            {
                //validate
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
        internal static string GetIntVersion(string gameDir, bool isV3)
        {
            byte[] bytBuffer = new byte[] { 0 };
            FileStream fsVer;

            // version is in OVL file
            string strFileName = gameDir + "AGIDATA.OVL";
            if (File.Exists(strFileName)) {
                try {
                    //open AGIDATA.OVL, copy to buffer, and close
                    fsVer = new FileStream(strFileName, FileMode.Open);
                    // get all the data
                    bytBuffer = new byte[fsVer.Length];
                    try {
                        fsVer.Read(bytBuffer, 0, (int)fsVer.Length);
                    }
                    catch (Exception) {
                        // ignore, treat as invalid
                    }
                    fsVer.Dispose();
                }
                catch (Exception) {
                    //invalid - return a default
                }
            }
            // if no data (either no file, or bad data
            if (bytBuffer.Length == 0) {
                //no agidata.ovl
                //if version3 is set
                if (isV3) {
                    //use default v3
                    return "3.002149"; //most common version 3
                }
                else {
                    //use default version  2.917
                    return "2.917";
                }
            }

            // now try to extract the version
            long lngPos = 0;
            //go until a '2' or '3' is found
            while (lngPos >= bytBuffer.Length) {
                //this function gets the version number of a Sierra AGI game
                //if found, it is validated against list of versions
                //that WinAGI recognizes
                //
                //returns version number for a valid number
                //returns null string for invalid number


                string strVersion;
                int i;
                //check char
                switch (bytBuffer[lngPos]) {
                case 50: //2.xxx format
                    strVersion = "2";
                    //get next four chars
                    for (i = 1; i <= 4; i++) {
                        lngPos++;
                        //just in case, check for end of buffer
                        if (lngPos >= bytBuffer.Length) {
                            break;
                        }
                        //add this char
                        strVersion += bytBuffer[lngPos].ToString();
                    }

                    //validate this version
                    if (IntVersions.Contains(strVersion))
                    //if (ValidateVersion(strVersion))
                    {
                        //return it
                        return strVersion;
                    }
                    break;

                case 51: //3.xxx.xxx format (for easier manipulation, the second '.' is
                         //removed, so result can be converted to a single precision number)
                    strVersion = "3";
                    //get next seven chars
                    for (i = 1; i <= 7; i++) {
                        lngPos++;
                        //just in case, check for end of buffer
                        if (lngPos >= bytBuffer.Length) {
                            break;
                        }

                        //add this char (unless it's the second period)
                        if (lngPos != 4) {
                            strVersion += bytBuffer[lngPos].ToString();
                        }
                    }

                    //validate this version
                    if (IntVersions.Contains(strVersion))
                    //if (ValidateVersion(strVersion))
                    {
                        //return it
                        return strVersion;
                    }
                    break;
                }

                //increment pointer
                lngPos++;
            }

            //if version info not found in AGIDATA.OVL

            //if version3 is set
            if (isV3) {
                return "3.002149"; //most common version 3?
            }
            else {
                return "2.917";  // This is what we use if we can't find the version number.
                                 // Version 2.917 is the most common interpreter and
                                 // the one that all the "new" AGI games should be based on.
            }
        }
        internal static void ResetDefaultColors()
        {
            defaultColorEGA[0] = Color.FromArgb(0, 0, 0);           // 000000 = black
            defaultColorEGA[1] = Color.FromArgb(0, 0, 0xAA);        // 0000AA = blue
            defaultColorEGA[2] = Color.FromArgb(0, 0xAA, 0);        // 00AA00 = green
            defaultColorEGA[3] = Color.FromArgb(0, 0xAA, 0xAA);     // 00AAAA = cyan
            defaultColorEGA[4] = Color.FromArgb(0xAA, 0, 0);        // AA0000 = red
            defaultColorEGA[5] = Color.FromArgb(0xAA, 0, 0xAA);     // AA00AA = magenta
            defaultColorEGA[6] = Color.FromArgb(0xAA, 0x55, 0);     // AA5500 = brown
            defaultColorEGA[7] = Color.FromArgb(0xAA, 0xAA, 0xAA);  // AAAAAA = light gray
            defaultColorEGA[8] = Color.FromArgb(0x55, 0x55, 0x55);  // 555555 = dark gray
            defaultColorEGA[9] = Color.FromArgb(0x55, 0x55, 0xFF);  // 5555FF = light blue
            defaultColorEGA[10] = Color.FromArgb(0, 0xFF, 0x55);    // 00FF55 = light green
            defaultColorEGA[11] = Color.FromArgb(0x55, 0xFF, 0xFF); // 55FFFF = light cyan
            defaultColorEGA[12] = Color.FromArgb(0xFF, 0x55, 0x55); // FF5555 = light red
            defaultColorEGA[13] = Color.FromArgb(0xFF, 0x55, 0xFF); // FF55FF = light magenta
            defaultColorEGA[14] = Color.FromArgb(0xFF, 0xFF, 0x55); // FFFF55 = yellow
            defaultColorEGA[15] = Color.FromArgb(0xFF, 0xFF, 0xFF); // FFFFFF = white
        }
        public static string LoadResString(int index)
        {
            // this function is just a handy way to get resource strings by number
            // instead of by stringkey
            try {
                return EngineResources.ResourceManager.GetString(index.ToString());
            }
            catch (Exception) {
                // return nothing if string doesn't exist
                return "";
            }
        }
    }
}