using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static WinAGI.Common.Base;
namespace WinAGI.Engine
{
  using System.Diagnostics;
  using System.Drawing;

  /***************************************************************
  WinAGI Game Engine
  Copyright (C) 2020 Andrew Korson

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
    csLogicError,
    csCanceled
  };
  public enum ELStatus
  { //used to update editor during a game load
    lsInitialize,
    lsDecompiling,
    lsPropertyFile,
    lsResources,
    lsWarning,
    lsError,
    lsFinalizing
  };
  public enum SoundFormat
  {
    sfUndefined,
    sfAGI,    //native agi format (all)
    sfMIDI,   //only pc and IIgs can be saved as midi
    sfScript, //only pc can be exported as script
    sfWAV     //only IIgs pcm sounds can be exported as wav
  };
  public enum LogEventType
  {
    leWarning,
    leError
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
    atVocWrd = 10   //vocabulary word; NOT word argument
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
    ncReservedMsg,   // 13(12) = name is reserved message
    ncBadChar,       // 14(13) = name contains improper character
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
  #endregion
  //structs
  #region
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
      { //load versions
        "2.089", "2.272", "2.411", "2.425", "2.426", "2.435", "2.439", "2.440",
        "2.915", "2.917", "2.936",
        "3.002086", "3.002098", "3.002102", "3.002107", "3.002149"
      };
    internal static readonly byte[] bytEncryptKey =
      { (byte)'A', (byte)'v', (byte)'i', (byte)'s', (byte)' ',
        (byte)'D', (byte)'u', (byte)'r', (byte)'g', (byte)'a', (byte)'n'
      }; //' = "Avis Durgan"
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
    internal const string WINAGI_VERSION = "3.0.1";
    // old versions
    internal const string WINAGI_VERSION_1_2 = "WINAGI v1.2     ";
    internal const string WINAGI_VERSION_1_0 = "WINAGI v1.0     ";
    internal const string WINAGI_VERSION_BETA = "1.0 BETA        ";
    // old version property constants for loading/saving game + resource properties
    internal const int SP_SEPARATOR = 128;
    internal const int PC_GAMEDESC = 129;
    internal const int PC_GAMEAUTHOR = 130;
    internal const int PC_GAMEID = 131;
    internal const int PC_INTVERSION = 132;
    internal const int PC_GAMELAST = 133;
    internal const int PC_GAMEVERSION = 134;
    internal const int PC_GAMEABOUT = 135;
    internal const int PC_GAMEEXEC = 136;
    internal const int PC_RESDIR = 137;
    internal const int PC_DEFSYNTAX = 138; //not used anymore...
    internal const int PC_INVOBJDESC = 144;
    internal const int PC_VOCABWORDDESC = 160;
    internal const int PC_PALETTE = 172;
    internal const int PC_USERESNAMES = 180;
    internal const int PC_LOGIC = 192;
    internal const int PC_PICTURE = 208;
    internal const int PC_SOUND = 224;
    internal const int PC_VIEW = 240;
    internal const int PT_ID = 0;
    internal const int PT_DESC = 1;
    internal const int PT_COMPCRC32 = 2;
    internal const int PT_CRC32 = 3;
    internal const int PT_KEY = 4;
    internal const int PT_INST0 = 5;
    internal const int PT_INST1 = 6;
    internal const int PT_INST2 = 7;
    internal const int PT_MUTE0 = 8;
    internal const int PT_MUTE1 = 9;
    internal const int PT_MUTE2 = 10;
    internal const int PT_MUTE3 = 11;
    internal const int PT_TPQN = 12;
    internal const int PT_ROOM = 13;
    internal const int PT_VIS0 = 14;
    internal const int PT_VIS1 = 15;
    internal const int PT_VIS2 = 16;
    internal const int PT_VIS3 = 17;
    internal const int PT_BKIMG = 18;
    internal const int PT_BKTRANS = 19;
    internal const int PT_BKPOS = 20;
    internal const int PT_BKSZ = 21;
    internal const int PT_SIZE = 254;
    internal const int PT_ALL = 255;
    // error offset is public
    public const int WINAGI_ERR = 0x100000;
    #endregion
    //global variables
    #region
    internal static string agDefResDir = "";
    internal static int defMaxVol0 = MAX_VOLSIZE;
    internal static EGAColors defaultColorEGA = new EGAColors();
    internal static string agSrcExt = ".lgc";

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
          throw new Exception("Replace(LoadResString(630), ARG1, NewDir)");

        agTemplateDir = CDir(value);
      }
    }

    static Base()
    {
      // initialize all winagi stuff here? no, put it in a method that can be called
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
          throw new Exception("Invalid property Value");
        }
        if (NewDir.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
          //throw new Exception("380, strErrSource, "Invalid property Value"
          throw new Exception("Invalid property Value");
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
      defaultColorEGA[1] = Color.FromArgb(0, 0, 0xA0);        // 0000A0 = blue
      defaultColorEGA[2] = Color.FromArgb(0, 0xA0, 0);        // 00A000 = green
      defaultColorEGA[3] = Color.FromArgb(0, 0xA0, 0xA0);     // 00A0A0 = cyan
      defaultColorEGA[4] = Color.FromArgb(0xA0, 0, 0);        // A00000 = red
      defaultColorEGA[5] = Color.FromArgb(0x80, 0, 0xA0);     // 8000A0 = magenta
      defaultColorEGA[6] = Color.FromArgb(0xA0, 0x50, 0);     // A05000 = brown
      defaultColorEGA[7] = Color.FromArgb(0xA0, 0xA0, 0xA0);  // A0A0A0 = light gray
      defaultColorEGA[8] = Color.FromArgb(0x50, 0x50, 0x50);  // 505050 = dark gray
      defaultColorEGA[9] = Color.FromArgb(0x50, 0x50, 0xFF);  // 5050FF = light blue
      defaultColorEGA[10] = Color.FromArgb(0, 0xFF, 0x50);    // 00FF50 = light green
      defaultColorEGA[11] = Color.FromArgb(0x50, 0xFF, 0xFF); // 50FFFF = light cyan
      defaultColorEGA[12] = Color.FromArgb(0xFF, 0x50, 0x50); // FF5050 = light red
      defaultColorEGA[13] = Color.FromArgb(0xFF, 0x50, 0xFF); // FF50FF = light magenta
      defaultColorEGA[14] = Color.FromArgb(0xFF, 0xFF, 0x50); // FFFF50 = yellow
      defaultColorEGA[15] = Color.FromArgb(0xFF, 0xFF, 0xFF); // FFFFFF = white
    }
    internal static SettingsList ConvertWag(AGIGame game, string oldWAGfile)
    {
      //converts a v1.2.1 propfile to current version proplist
      //1.2.1 properties use the following format for the property file:
      // CPRLL<data>
      //where C= PropCode, P=PropNum, R=ResNum, LL=length of data (as integer)
      //      <data>= property data
      //last line of file should be version code

      byte[] bytData = Array.Empty<byte>();
      int lngCount, lngPos;
      string strValue;
      int i, PropSize;
      byte ResNum, PropType;
      byte PropCode;
      bool blnFoundID = false, blnFoundVer = false;
      SettingsList retval = new SettingsList(oldWAGfile);

      //open old file
      FileStream fsOldWAG = new FileStream(oldWAGfile, FileMode.Open);
      //verify version
      bytData = new byte[16];
      //adjust position to compensate for length of variable
      //: fsretval.Length - 16;
      fsOldWAG.Read(bytData, (int)fsOldWAG.Length - 16, 16);
      strValue = Encoding.UTF8.GetString(bytData);

      //if version is incompatible
      switch (strValue) {
      case WINAGI_VERSION_1_2:
      case WINAGI_VERSION_1_0:
      case WINAGI_VERSION_BETA:
        break;
      //ok
      default:
        //return nothing
        fsOldWAG.Dispose();
        return retval;
      }
      //add header
      retval.Lines.Add("#");
      retval.Lines.Add("# WinAGI Game Property File");
      retval.Lines.Add("# converted from version 1.2.1");
      retval.Lines.Add("#");
      retval.Lines.Add("[General]");
      retval.Lines.Add("   WinAGIVersion = " + WINAGI_VERSION);

      //don't copy version line into buffer
      lngCount = (int)fsOldWAG.Length - 16;
      if (lngCount > 0) {
        bytData = new byte[lngCount];
        fsOldWAG.Read(bytData, 0, lngCount);
      }
      else {
        //set to zero
        lngCount = 0;
      }
      lngPos = 0;
      //get codes
      while (lngPos < lngCount) {
        //reset propval
        strValue = "";
        //get prop data
        PropCode = bytData[lngPos];
        PropType = bytData[lngPos + 1];
        ResNum = bytData[lngPos + 2];
        PropSize = bytData[lngPos + 3] + 256 * bytData[lngPos + 4];
        for (i = 1; i < PropSize; i++) {
          strValue += (char)bytData[lngPos + 4 + i];
        }
        if (PropCode >= PC_LOGIC) {
          switch (PropCode) {
          case PC_LOGIC:
            switch (PropType) {
            case PT_ID:
              game.WriteGameSetting("Logic" + ResNum.ToString(), "ID", strValue, "Logics");
              break;
            case PT_DESC:
              game.WriteGameSetting("Logic" + ResNum.ToString(), "Description", strValue, "Logics");
              break;
            case PT_CRC32:
              game.WriteGameSetting("Logic" + ResNum.ToString(), "CRC32", "0x" + strValue, "Logics");
              break;
            case PT_COMPCRC32:
              game.WriteGameSetting("Logic" + ResNum.ToString(), "CompCRC32", "0x" + strValue, "Logics");
              break;
            case PT_ROOM:
              if (ResNum == 0) {
                //force to false
                strValue = "false";
              }
              game.WriteGameSetting("Logic" + ResNum.ToString(), "IsRoom", strValue, "Logics");
              break;
            case PT_SIZE:
              game.WriteGameSetting("Logic" + ResNum.ToString(), "Size", strValue, "Logics");
              break;

            default:
              //unknown code; ignore it
              //*'Debug.Throw exception
              break;
            }
            break;
          case PC_PICTURE:
            switch (PropType) {
            case PT_ID:
              game.WriteGameSetting("Picture" + ResNum.ToString(), "ID", strValue, "Pictures");
              break;
            case PT_DESC:
              game.WriteGameSetting("Picture" + ResNum.ToString(), "Description", strValue, "Pictures");
              break;
            case PT_SIZE:
              game.WriteGameSetting("Picture" + ResNum.ToString(), "Size", strValue, "Pictures");
              break;
            case PT_BKIMG:
              game.WriteGameSetting("Picture" + ResNum.ToString(), "BkgdImg", strValue, "Pictures");
              break;
            case PT_BKPOS:
              game.WriteGameSetting("Picture" + ResNum.ToString(), "BkgdPosn", strValue, "Pictures");
              break;
            case PT_BKSZ:
              game.WriteGameSetting("Picture" + ResNum.ToString(), "BkgdSize", strValue, "Pictures");
              break;
            case PT_BKTRANS:
              game.WriteGameSetting("Picture" + ResNum.ToString(), "BkgdTrans", strValue, "Pictures");
              break;
            default:
              //unknown code; ignore it
              //*'Debug.Throw exception
              break;
            }
            break;
          case PC_SOUND:
            switch (PropType) {
            case PT_ID:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "ID", strValue, "Sounds");
              break;
            case PT_DESC:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Description", strValue, "Sounds");
              break;
            case PT_SIZE:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Size", strValue, "Sounds");
              break;
            case PT_KEY:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Key", strValue, "Sounds");
              break;
            case PT_TPQN:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "TQPN", strValue, "Sounds");
              break;
            case PT_INST0:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Inst0", strValue, "Sounds");
              break;
            case PT_INST1:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Inst1", strValue, "Sounds");
              break;
            case PT_INST2:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Inst2", strValue, "Sounds");
              break;
            case PT_MUTE0:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Mute0", strValue, "Sounds");
              break;
            case PT_MUTE1:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Mute1", strValue, "Sounds");
              break;
            case PT_MUTE2:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Mute2", strValue, "Sounds");
              break;
            case PT_MUTE3:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Mute3", strValue, "Sounds");
              break;
            case PT_VIS0:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Visible0", strValue, "Sounds");
              break;
            case PT_VIS1:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Visible1", strValue, "Sounds");
              break;
            case PT_VIS2:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Visible2", strValue, "Sounds");
              break;
            case PT_VIS3:
              game.WriteGameSetting("Sound" + ResNum.ToString(), "Visible3", strValue, "Sounds");
              break;
            default:
              //unknown code; ignore it
              //*'Debug.Throw exception
              break;
            }
            break;
          case PC_VIEW:
            switch (PropType) {
            case PT_ID:
              game.WriteGameSetting("View" + ResNum.ToString(), "ID", strValue, "Views");
              break;
            case PT_DESC:
              game.WriteGameSetting("View" + ResNum.ToString(), "Description", strValue, "Views");
              break;
            case PT_SIZE:
              game.WriteGameSetting("View" + ResNum.ToString(), "Size", strValue, "Views");
              break;
            default:
              //unknown code; ignore it
              //*'Debug.Throw exception
              break;
            }
            break;
          }

        }
        else {
          switch (PropCode) {
          case PC_GAMEDESC:
            game.WriteGameSetting("General", "Description", strValue);
            break;

          case PC_GAMEAUTHOR:
            game.WriteGameSetting("General", "Author", strValue);
            break;
          case PC_GAMEID:
            blnFoundID = strValue.Length > 0;
            game.WriteGameSetting("General", "GameID", strValue);
            break;
          case PC_INTVERSION:
            game.WriteGameSetting("General", "Interpreter", strValue);
            blnFoundVer = strValue.Length > 0;
            break;
          case PC_GAMEABOUT:
            game.WriteGameSetting("General", "About", strValue);
            break;
          case PC_GAMEVERSION:
            game.WriteGameSetting("General", "GameVersion", strValue);
            break;
          case PC_RESDIR:
            game.WriteGameSetting("General", "ResDir", strValue);
            break;
          case PC_GAMELAST:
            game.WriteGameSetting("General", "LastEdit", strValue);
            break;
          case PC_INVOBJDESC:
            game.WriteGameSetting("OBJECT", "Description", strValue);
            break;
          case PC_VOCABWORDDESC:
            game.WriteGameSetting("WORDS.TOK", "Description", strValue);
            break;
          case PC_GAMEEXEC:
            //////game.WriteGameSetting("General", "Exec", strValue);
            //Exec property no longer supported
            break;

          case PC_PALETTE:  //TODO: all hex strings need to be stored as '0x00', not '0x00'
                            //convert the color bytes into long values
            for (i = 0; i < 16; i++) {
              strValue = "0x";
              strValue += bytData[lngPos + 5 + 4 * i].ToString("x2") +
                          bytData[lngPos + 6 + 4 * i].ToString("x2") +
                          bytData[lngPos + 7 + 4 * i].ToString("x2") +
                          bytData[lngPos + 8 + 4 * i].ToString("x2");
              game.WriteGameSetting("Palette", "Color" + i.ToString(), strValue);
            }
            break;
          case PC_USERESNAMES:
            game.WriteGameSetting("General", "UseResNames", strValue);
            break;


          default:
            //ignore
            break;
          }
        } //end if propcode>=pclogic

        //add offset to next code (length +5)
        lngPos += PropSize + 5;
      }
      //if no id and no intver
      if (!blnFoundID || blnFoundVer) {
        // return an empty list
        retval = new SettingsList(oldWAGfile);
      }
      return retval;
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