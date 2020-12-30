using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static WinAGI.AGIGame;
using static WinAGI.AGICommands;
using static WinAGI.AGITestCommands;

namespace WinAGI
{
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
  public enum AGIColors
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
    leHigh,    //all compile/decompile problems are returned as errors
    leMedium,  //only errors that prevent compilation/decompilation
               //are passed; warnings embedded in
               //source code on compilation
    leLow     //only errors that prevent compilation/decompiliation
              //are passed; no warnings are given
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
    lsFinalizing
  };
  public enum SoundFormat
  {
    sfUndefined,
    sfAGI,    //all sounds
    sfMIDI,    //pc and IIgs midi
    sfScript,  //pc only
    sfWAV     //only for IIgs pcm sounds
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
    atFlag = 2,      //f##
    atMsg = 3,      //m##
    atSObj = 4,     //o##
    atIObj = 5,     //i##
    atStr = 6,     //s##
    atWord = 7,     //w## -- word argument (that user types in)
    atCtrl = 8,     //c##
    atDefStr = 9,   //defined string; could be msg, inv obj, or vocword
    atVocWrd = 10   //vocabulary word; NOT word argument
  }

  //structs
  public struct PenStatus
  {
    public AGIColors VisColor;
    public AGIColors PriColor;
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
  public static partial class WinAGI
  {
    // this class is for all the global stuff that was previously in separate modules in VB6
    internal static uint[] CRC32Table = new uint[256];
    internal static bool CRC32Loaded;

    //arrays which will be treated as constants
    //rev colors have red and blue components switched
    //so api functions using colors work correctly
    internal static uint[] lngEGARevCol = new uint[16]; //15
    internal static uint[] lngEGACol = new uint[16]; //15;
    //internal static AGIColors[] agColor = new AGIColors[16]; //15;
    internal static byte[] bytEncryptKey = { (byte)'A', (byte)'v', (byte)'i',
                             (byte)'s', (byte)' ', (byte)'D',
                             (byte)'u', (byte)'r', (byte)'g',
                             (byte)'a', (byte)'n' }; //10; //' = "Avis Durgan"

    // string arrays that are 'enums'
    public static readonly string[] ResTypeAbbrv = { "LOG", "PIC", "SND", "VIEW" };
    public static readonly string[] ResTypeName = { "Logic", "Picture", "Sound", "View" };
    public static readonly string[] IntVersions = new string[16]
    { //load versions
      "2.089", "2.272", "2.411", "2.425", "2.426", "2.435", "2.439", "2.440",
      "2.915", "2.917", "2.936",
      "3.002086", "3.002098", "3.002102", "3.002107", "3.002149"
    };

    //game constants
    internal const int MAX_RES_SIZE = 65530;
    internal const int MAX_LOOPS = 255;
    internal const int MAX_CELS = 255;
    internal const int MAX_ITEMS = 256;
    internal const int MAX_VOL_FILES = 65;
    internal const int MAX_CEL_WIDTH = 160;
    internal const int MAX_CEL_HEIGHT = 168;
    internal const int MAX_GROUP_NUM = 65535;
    internal const int MAX_WORD_GROUPS = 65535;
    internal const int MAX_VOLSIZE = 1047552;
    //MAX_VOLSIZE = 1047552  '= 1024 * 1023
    internal const string WORD_SEPARATOR = " | ";
    internal static readonly string CTRL_CHARS;
    internal static readonly string NEWLINE = Environment.NewLine;
    //current version
    internal const string WINAGI_VERSION = "3.0.1";
    // old versions
    internal const string WINAGI_VERSION_1_2 = "WINAGI v1.2     ";
    internal const string WINAGI_VERSION_1_0 = "WINAGI v1.0     ";
    internal const string WINAGI_VERSION_BETA = "1.0 BETA        ";
    // old version property constants for loading/saving game & resource properties
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

    //predefined arguments
    internal static bool agResAsText;  //if true, reserved variables and flags show up as text when decompiling
                                       //not used if agUseRes is FALSE
    internal static bool agUseRes;     //if true, predefined variables and flags are used during compilation
    internal static TDefine[] agResVar = new TDefine[27];    //26 //text name of built in variables
    internal static TDefine[] agResFlag = new TDefine[18];   //17) //text name of built in flags
    internal static TDefine[] agEdgeCodes = new TDefine[5]; //4 //text of edge codes
    internal static TDefine[] agEgoDir = new TDefine[9];    //8 //text of ego direction codes
    internal static TDefine[] agVideoMode = new TDefine[5]; //4 //text of video mode codes
    internal static TDefine[] agCompType = new TDefine[9];  //8 //computer type values
    internal static TDefine[] agResDef = new TDefine[6];    //5 //defines for ego, gamever, gameabout, gameid, invobj Count
    internal static TDefine[] agResColor = new TDefine[16];  //15 //predefined color values

    //warning count value stored in Common file, so it can be used by the IDE as well as the engine
    internal const int WARNCOUNT = 107;
    internal static bool[] agNoCompWarn = new bool[WARNCOUNT];

    //user defined global arguments
    internal static TDefine[] agGlobal; //dynamic size
    internal static int agGlobalCount;
    internal static bool agGlobalsSet;
    internal static uint agGlobalCRC;

    //error number and string to return error values
    //from various functions/subroutines
    internal static int lngError = 0;
    internal static string strError = "";
    internal static string strErrSrc = "";
    internal static void InitWinAGI()
    {
      // calling this forces the module to load and initialize!
      //// non-readonly startup stuff goes here
      //RestoreDefaultColors();
      //CRC32Setup();
      ////get max vol size
      //agMaxVolSize = 1023 * 1024;

      ////set max vol0 size
      //agMaxVol0 = agMaxVolSize;
    }
    static WinAGI()
    {
      // initialize all winagi stuff here
      RestoreDefaultColors();
      CRC32Setup();
      //get max vol size
      agMaxVolSize = 1023 * 1024;

      //set max vol0 size
      agMaxVol0 = agMaxVolSize;

      for (int i = 1; i < 32; i++)
        CTRL_CHARS += ((char)i).ToString();
    }
    internal static string Right(string strIn, int length)
    {
      if (length >= strIn.Length)
        return strIn;
      else
        return strIn.Substring(strIn.Length - length);
    }
    internal static string Left(string strIn, int length)
    {
      if (length >= strIn.Length)
        return strIn;
      else
        return strIn.Substring(0, length);
    }
    internal static string Mid(string strIn, int pos, int length)
    {
      // mimic VB mid function; if length is too long, return
      // max amount
      if (pos + length > strIn.Length)
        return strIn.Substring(pos, strIn.Length - pos);
      return strIn.Substring(pos, length);
    }
    internal static string MultStr(string strIn, int NumCopies)
    {
      string retval = "";
      for (int i = 1; i <= NumCopies; i++)
        retval += strIn;
      return retval;
    }
    /// <summary>
    /// Extension method that works out if a string is numeric or not
    /// </summary>
    /// <param name="str">string that may be a number</param>
    /// <returns>true if numeric, false if not</returns>
    internal static bool IsNumeric(string str)
    {
      if (Double.TryParse(str, out _))
      {
        return true;
      }
      return false;
    }
    /// <summary>
    /// Extension that mimics the VB Val() function; returns 0
    /// if the string is non-numeric
    /// </summary>
    /// <param name="strIn">The string that will be converted to a number</param>
    /// <returns>Returns a double value of strIn; if strIn can't be converted
    /// to a double, it returns 0</returns>
    internal static double Val(string strIn)
    {
      if (double.TryParse(strIn, out double dResult))
      {
        //return this value
        return dResult;
      }
      // not a valid number; return 0
      return 0;
    }
    /// <summary>
    /// Confirms that a directory has a terminating backslash,
    /// adding one if necessary
    /// </summary>
    /// <param name="strDirIn"></param>
    /// <returns></returns>
    internal static string CDir(string strDirIn)
    {
      //this function ensures a trailing "\" is included on strDirIn
      if (strDirIn.Length != 0)
        if (!strDirIn.EndsWith(@"\"))
          return strDirIn + @"\";
        else
          return strDirIn;
      else
        return strDirIn;
    }
    internal static string JustFileName(string strFullPathName)
    {
      //will extract just the file name by removing the path info
      string[] strSplitName;

      //On Error Resume Next

      strSplitName = strFullPathName.Split(@"\");
      if (strSplitName.Length == 1)
        return strFullPathName;
      else
        return strSplitName[strSplitName.Length - 1];
    }
    internal static string JustPath(string strFullPathName, bool NoSlash = false)
    {  //will extract just the path name by removing the filename
       //if optional NoSlash is true, the trailing backslash will be dropped

      // if nothing
      if (strFullPathName.Length == 0)
      {
        return "";
      }

      //split into directories and filename
      string[] strSplitName = strFullPathName.Split("\\");
      //if no splits,
      if (strSplitName.Length == 1)
      {
        //return empty- no path information in this string
        return "";
      }
      else
      {
        //eliminate last entry (which is filename)
        Array.Resize(ref strSplitName, strSplitName.Length - 1);
        //rebuild name
        string sReturn = String.Join("\\", strSplitName);
        if (!NoSlash)
        {
          sReturn += "\\";
        }
        return sReturn;
      }
      //if slash should be added,
    }
    internal static string FileNameNoExt(string FileName)
    {
      //returns a filename without the extension
      //if FileName includes a path, the path is also removed
      string strOut = JustFileName(FileName);
      int i = strOut.LastIndexOf(".");
      if (i <= 0)
      {
        return strOut;
      }
      else
      {
        return Left(strOut, i - 1);
      }
    }
    internal static uint CRC32(byte[] DataIn)
    {

      //Public Function CRC32(DataIn() As Byte) As Long
      //calculates the CRC32 for an input array of bytes
      //a special table is necessary; the table is loaded
      //at program startup

      //the CRC is calculated according to the following equation:
      //
      //  CRC[i] = LSHR8(CRC[i-1]) Xor CRC32Table[(CRC[i-1] And &HFF) Xor DataIn[i])
      //
      //initial Value of CRC is &HFFFFFFFF; iterate the equation
      //for each byte of data; then end by XORing final result with &HFFFFFFFF

      int i;
      //initial Value
      uint result = 0xffffffff;

      //if table not loaded
      if (!CRC32Loaded)
        CRC32Setup();

      //iterate CRC equation
      for (i = 0; i < DataIn.Length; i++)
        result = (result >> 8) ^ CRC32Table[(result & 0xFF) ^ DataIn[i]];

      //xor to create final answer
      return result ^ 0xFFFFFFFF;
    }
    internal static void CRC32Setup()
    {
      //build the CRC table
      uint z;
      uint index;
      for (index = 0; index < 256; index++)
      {
        CRC32Table[index] = index;
        for (z = 8; z != 0; z--)
        {
          if ((CRC32Table[index] & 1) == 1)
          {
            CRC32Table[index] = (CRC32Table[index] >> 1) ^ 0xEDB88320;
          }
          else
          {
            CRC32Table[index] = CRC32Table[index] >> 1;
          }
        }
      }

      //set flag
      CRC32Loaded = true;

      // keep the real values until I'm sure the calculated table is
      // 100% correct

      //CRC32Table[0] = 0x0;
      //CRC32Table[1] = 0x77073096;
      //CRC32Table[2] = 0xEE0E612C;
      //CRC32Table[3] = 0x990951BA;
      //CRC32Table[4] = 0x76DC419;
      //CRC32Table[5] = 0x706AF48F;
      //CRC32Table[6] = 0xE963A535;
      //CRC32Table[7] = 0x9E6495A3;
      //CRC32Table[8] = 0xEDB8832;
      //CRC32Table[9] = 0x79DCB8A4;
      //CRC32Table[10] = 0xE0D5E91E;
      //CRC32Table[11] = 0x97D2D988;
      //CRC32Table[12] = 0x9B64C2B;
      //CRC32Table[13] = 0x7EB17CBD;
      //CRC32Table[14] = 0xE7B82D07;
      //CRC32Table[15] = 0x90BF1D91;
      //CRC32Table[16] = 0x1DB71064;
      //CRC32Table[17] = 0x6AB020F2;
      //CRC32Table[18] = 0xF3B97148;
      //CRC32Table[19] = 0x84BE41DE;
      //CRC32Table[20] = 0x1ADAD47D;
      //CRC32Table[21] = 0x6DDDE4EB;
      //CRC32Table[22] = 0xF4D4B551;
      //CRC32Table[23] = 0x83D385C7;
      //CRC32Table[24] = 0x136C9856;
      //CRC32Table[25] = 0x646BA8C0;
      //CRC32Table[26] = 0xFD62F97A;
      //CRC32Table[27] = 0x8A65C9EC;
      //CRC32Table[28] = 0x14015C4F;
      //CRC32Table[29] = 0x63066CD9;
      //CRC32Table[30] = 0xFA0F3D63;
      //CRC32Table[31] = 0x8D080DF5;
      //CRC32Table[32] = 0x3B6E20C8;
      //CRC32Table[33] = 0x4C69105E;
      //CRC32Table[34] = 0xD56041E4;
      //CRC32Table[35] = 0xA2677172;
      //CRC32Table[36] = 0x3C03E4D1;
      //CRC32Table[37] = 0x4B04D447;
      //CRC32Table[38] = 0xD20D85FD;
      //CRC32Table[39] = 0xA50AB56B;
      //CRC32Table[40] = 0x35B5A8FA;
      //CRC32Table[41] = 0x42B2986C;
      //CRC32Table[42] = 0xDBBBC9D6;
      //CRC32Table[43] = 0xACBCF940;
      //CRC32Table[44] = 0x32D86CE3;
      //CRC32Table[45] = 0x45DF5C75;
      //CRC32Table[46] = 0xDCD60DCF;
      //CRC32Table[47] = 0xABD13D59;
      //CRC32Table[48] = 0x26D930AC;
      //CRC32Table[49] = 0x51DE003A;
      //CRC32Table[50] = 0xC8D75180;
      //CRC32Table[51] = 0xBFD06116;
      //CRC32Table[52] = 0x21B4F4B5;
      //CRC32Table[53] = 0x56B3C423;
      //CRC32Table[54] = 0xCFBA9599;
      //CRC32Table[55] = 0xB8BDA50F;
      //CRC32Table[56] = 0x2802B89E;
      //CRC32Table[57] = 0x5F058808;
      //CRC32Table[58] = 0xC60CD9B2;
      //CRC32Table[59] = 0xB10BE924;
      //CRC32Table[60] = 0x2F6F7C87;
      //CRC32Table[61] = 0x58684C11;
      //CRC32Table[62] = 0xC1611DAB;
      //CRC32Table[63] = 0xB6662D3D;
      //CRC32Table[64] = 0x76DC4190;
      //CRC32Table[65] = 0x1DB7106;
      //CRC32Table[66] = 0x98D220BC;
      //CRC32Table[67] = 0xEFD5102A;
      //CRC32Table[68] = 0x71B18589;
      //CRC32Table[69] = 0x6B6B51F;
      //CRC32Table[70] = 0x9FBFE4A5;
      //CRC32Table[71] = 0xE8B8D433;
      //CRC32Table[72] = 0x7807C9A2;
      //CRC32Table[73] = 0xF00F934;
      //CRC32Table[74] = 0x9609A88E;
      //CRC32Table[75] = 0xE10E9818;
      //CRC32Table[76] = 0x7F6A0DBB;
      //CRC32Table[77] = 0x86D3D2D;
      //CRC32Table[78] = 0x91646C97;
      //CRC32Table[79] = 0xE6635C01;
      //CRC32Table[80] = 0x6B6B51F4;
      //CRC32Table[81] = 0x1C6C6162;
      //CRC32Table[82] = 0x856530D8;
      //CRC32Table[83] = 0xF262004E;
      //CRC32Table[84] = 0x6C0695ED;
      //CRC32Table[85] = 0x1B01A57B;
      //CRC32Table[86] = 0x8208F4C1;
      //CRC32Table[87] = 0xF50FC457;
      //CRC32Table[88] = 0x65B0D9C6;
      //CRC32Table[89] = 0x12B7E950;
      //CRC32Table[90] = 0x8BBEB8EA;
      //CRC32Table[91] = 0xFCB9887C;
      //CRC32Table[92] = 0x62DD1DDF;
      //CRC32Table[93] = 0x15DA2D49;
      //CRC32Table[94] = 0x8CD37CF3;
      //CRC32Table[95] = 0xFBD44C65;
      //CRC32Table[96] = 0x4DB26158;
      //CRC32Table[97] = 0x3AB551CE;
      //CRC32Table[98] = 0xA3BC0074;
      //CRC32Table[99] = 0xD4BB30E2;
      //CRC32Table[100] = 0x4ADFA541;
      //CRC32Table[101] = 0x3DD895D7;
      //CRC32Table[102] = 0xA4D1C46D;
      //CRC32Table[103] = 0xD3D6F4FB;
      //CRC32Table[104] = 0x4369E96A;
      //CRC32Table[105] = 0x346ED9FC;
      //CRC32Table[106] = 0xAD678846;
      //CRC32Table[107] = 0xDA60B8D0;
      //CRC32Table[108] = 0x44042D73;
      //CRC32Table[109] = 0x33031DE5;
      //CRC32Table[110] = 0xAA0A4C5F;
      //CRC32Table[111] = 0xDD0D7CC9;
      //CRC32Table[112] = 0x5005713C;
      //CRC32Table[113] = 0x270241AA;
      //CRC32Table[114] = 0xBE0B1010;
      //CRC32Table[115] = 0xC90C2086;
      //CRC32Table[116] = 0x5768B525;
      //CRC32Table[117] = 0x206F85B3;
      //CRC32Table[118] = 0xB966D409;
      //CRC32Table[119] = 0xCE61E49F;
      //CRC32Table[120] = 0x5EDEF90E;
      //CRC32Table[121] = 0x29D9C998;
      //CRC32Table[122] = 0xB0D09822;
      //CRC32Table[123] = 0xC7D7A8B4;
      //CRC32Table[124] = 0x59B33D17;
      //CRC32Table[125] = 0x2EB40D81;
      //CRC32Table[126] = 0xB7BD5C3B;
      //CRC32Table[127] = 0xC0BA6CAD;
      //CRC32Table[128] = 0xEDB88320;
      //CRC32Table[129] = 0x9ABFB3B6;
      //CRC32Table[130] = 0x3B6E20C;
      //CRC32Table[131] = 0x74B1D29A;
      //CRC32Table[132] = 0xEAD54739;
      //CRC32Table[133] = 0x9DD277AF;
      //CRC32Table[134] = 0x4DB2615;
      //CRC32Table[135] = 0x73DC1683;
      //CRC32Table[136] = 0xE3630B12;
      //CRC32Table[137] = 0x94643B84;
      //CRC32Table[138] = 0xD6D6A3E;
      //CRC32Table[139] = 0x7A6A5AA8;
      //CRC32Table[140] = 0xE40ECF0B;
      //CRC32Table[141] = 0x9309FF9D;
      //CRC32Table[142] = 0xA00AE27;
      //CRC32Table[143] = 0x7D079EB1;
      //CRC32Table[144] = 0xF00F9344;
      //CRC32Table[145] = 0x8708A3D2;
      //CRC32Table[146] = 0x1E01F268;
      //CRC32Table[147] = 0x6906C2FE;
      //CRC32Table[148] = 0xF762575D;
      //CRC32Table[149] = 0x806567CB;
      //CRC32Table[150] = 0x196C3671;
      //CRC32Table[151] = 0x6E6B06E7;
      //CRC32Table[152] = 0xFED41B76;
      //CRC32Table[153] = 0x89D32BE0;
      //CRC32Table[154] = 0x10DA7A5A;
      //CRC32Table[155] = 0x67DD4ACC;
      //CRC32Table[156] = 0xF9B9DF6F;
      //CRC32Table[157] = 0x8EBEEFF9;
      //CRC32Table[158] = 0x17B7BE43;
      //CRC32Table[159] = 0x60B08ED5;
      //CRC32Table[160] = 0xD6D6A3E8;
      //CRC32Table[161] = 0xA1D1937E;
      //CRC32Table[162] = 0x38D8C2C4;
      //CRC32Table[163] = 0x4FDFF252;
      //CRC32Table[164] = 0xD1BB67F1;
      //CRC32Table[165] = 0xA6BC5767;
      //CRC32Table[166] = 0x3FB506DD;
      //CRC32Table[167] = 0x48B2364B;
      //CRC32Table[168] = 0xD80D2BDA;
      //CRC32Table[169] = 0xAF0A1B4C;
      //CRC32Table[170] = 0x36034AF6;
      //CRC32Table[171] = 0x41047A60;
      //CRC32Table[172] = 0xDF60EFC3;
      //CRC32Table[173] = 0xA867DF55;
      //CRC32Table[174] = 0x316E8EEF;
      //CRC32Table[175] = 0x4669BE79;
      //CRC32Table[176] = 0xCB61B38C;
      //CRC32Table[177] = 0xBC66831A;
      //CRC32Table[178] = 0x256FD2A0;
      //CRC32Table[179] = 0x5268E236;
      //CRC32Table[180] = 0xCC0C7795;
      //CRC32Table[181] = 0xBB0B4703;
      //CRC32Table[182] = 0x220216B9;
      //CRC32Table[183] = 0x5505262F;
      //CRC32Table[184] = 0xC5BA3BBE;
      //CRC32Table[185] = 0xB2BD0B28;
      //CRC32Table[186] = 0x2BB45A92;
      //CRC32Table[187] = 0x5CB36A04;
      //CRC32Table[188] = 0xC2D7FFA7;
      //CRC32Table[189] = 0xB5D0CF31;
      //CRC32Table[190] = 0x2CD99E8B;
      //CRC32Table[191] = 0x5BDEAE1D;
      //CRC32Table[192] = 0x9B64C2B0;
      //CRC32Table[193] = 0xEC63F226;
      //CRC32Table[194] = 0x756AA39C;
      //CRC32Table[195] = 0x26D930A;
      //CRC32Table[196] = 0x9C0906A9;
      //CRC32Table[197] = 0xEB0E363F;
      //CRC32Table[198] = 0x72076785;
      //CRC32Table[199] = 0x5005713;
      //CRC32Table[200] = 0x95BF4A82;
      //CRC32Table[201] = 0xE2B87A14;
      //CRC32Table[202] = 0x7BB12BAE;
      //CRC32Table[203] = 0xCB61B38;
      //CRC32Table[204] = 0x92D28E9B;
      //CRC32Table[205] = 0xE5D5BE0D;
      //CRC32Table[206] = 0x7CDCEFB7;
      //CRC32Table[207] = 0xBDBDF21;
      //CRC32Table[208] = 0x86D3D2D4;
      //CRC32Table[209] = 0xF1D4E242;
      //CRC32Table[210] = 0x68DDB3F8;
      //CRC32Table[211] = 0x1FDA836E;
      //CRC32Table[212] = 0x81BE16CD;
      //CRC32Table[213] = 0xF6B9265B;
      //CRC32Table[214] = 0x6FB077E1;
      //CRC32Table[215] = 0x18B74777;
      //CRC32Table[216] = 0x88085AE6;
      //CRC32Table[217] = 0xFF0F6A70;
      //CRC32Table[218] = 0x66063BCA;
      //CRC32Table[219] = 0x11010B5C;
      //CRC32Table[220] = 0x8F659EFF;
      //CRC32Table[221] = 0xF862AE69;
      //CRC32Table[222] = 0x616BFFD3;
      //CRC32Table[223] = 0x166CCF45;
      //CRC32Table[224] = 0xA00AE278;
      //CRC32Table[225] = 0xD70DD2EE;
      //CRC32Table[226] = 0x4E048354;
      //CRC32Table[227] = 0x3903B3C2;
      //CRC32Table[228] = 0xA7672661;
      //CRC32Table[229] = 0xD06016F7;
      //CRC32Table[230] = 0x4969474D;
      //CRC32Table[231] = 0x3E6E77DB;
      //CRC32Table[232] = 0xAED16A4A;
      //CRC32Table[233] = 0xD9D65ADC;
      //CRC32Table[234] = 0x40DF0B66;
      //CRC32Table[235] = 0x37D83BF0;
      //CRC32Table[236] = 0xA9BCAE53;
      //CRC32Table[237] = 0xDEBB9EC5;
      //CRC32Table[238] = 0x47B2CF7F;
      //CRC32Table[239] = 0x30B5FFE9;
      //CRC32Table[240] = 0xBDBDF21C;
      //CRC32Table[241] = 0xCABAC28A;
      //CRC32Table[242] = 0x53B39330;
      //CRC32Table[243] = 0x24B4A3A6;
      //CRC32Table[244] = 0xBAD03605;
      //CRC32Table[245] = 0xCDD70693;
      //CRC32Table[246] = 0x54DE5729;
      //CRC32Table[247] = 0x23D967BF;
      //CRC32Table[248] = 0xB3667A2E;
      //CRC32Table[249] = 0xC4614AB8;
      //CRC32Table[250] = 0x5D681B02;
      //CRC32Table[251] = 0x2A6F2B94;
      //CRC32Table[252] = 0xB40BBE37;
      //CRC32Table[253] = 0xC30C8EA1;
      //CRC32Table[254] = 0x5A05DF1B;
      //CRC32Table[255] = 0x2D02EF8D;

    }
    internal static void AssignReservedDefines()
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

      // others
      agResDef[0].Name = "ego";
      agResDef[0].Value = "o0";
      agResDef[1].Name = "gameVersionMsg";
      //agResDef[1].Value = vbNullString; //will be assigned by compiler
      agResDef[2].Name = "gameAboutMsg";
      //agResDef[2].Value = vbNullString; //will be assigned by compiler
      agResDef[3].Name = "gameID";
      //agResDef[3].Value = vbNullString; //will be assigned by compiler
      agResDef[4].Name = "numberOfItems";
      //agResDef[4].Value = vbNullString; //will be assigned by compiler
      agResDef[5].Name = "inputPrompt";
      agResDef[5].Value = "s0";

      //set types and defaults
      int i;
      for (i = 0; i <= 26; i++)
      {
        agResVar[i].Type = ArgTypeEnum.atVar;
        agResVar[i].Default = agResVar[i].Name;
      }
      for (i = 0; i <= 17; i++)
      {
        agResFlag[i].Type = ArgTypeEnum.atFlag;
        agResFlag[i].Default = agResFlag[i].Name;
      }
      for (i = 0; i <= 4; i++)
      {
        agEdgeCodes[i].Type = ArgTypeEnum.atNum;
        agEdgeCodes[i].Default = agEdgeCodes[i].Name;
      }
      for (i = 0; i <= 8; i++)
      {
        agEgoDir[i].Type = ArgTypeEnum.atNum;
        agEgoDir[i].Default = agEgoDir[i].Name;
      }
      for (i = 0; i <= 4; i++)
      {
        agVideoMode[i].Type = ArgTypeEnum.atNum;
        agVideoMode[i].Default = agVideoMode[i].Name;
      }
      for (i = 0; i <= 8; i++)
      {
        agCompType[i].Type = ArgTypeEnum.atNum;
        agCompType[i].Default = agCompType[i].Name;
      }
      for (i = 0; i <= 15; i++)
      {
        agResColor[i].Type = ArgTypeEnum.atNum;
        agResColor[i].Default = agResColor[i].Name;
      }

      // others
      agResDef[0].Type = ArgTypeEnum.atSObj;
      agResDef[0].Default = agResDef[0].Name;
      agResDef[1].Type = ArgTypeEnum.atDefStr;
      agResDef[1].Default = agResDef[1].Name;
      agResDef[2].Type = ArgTypeEnum.atDefStr;
      agResDef[2].Default = agResDef[2].Name;
      agResDef[3].Type = ArgTypeEnum.atDefStr;
      agResDef[3].Default = agResDef[3].Name;
      agResDef[4].Type = ArgTypeEnum.atNum;
      agResDef[4].Default = agResDef[4].Name;
      agResDef[5].Type = ArgTypeEnum.atStr;
      agResDef[5].Default = agResDef[5].Name;
    }
    internal static void GetGlobalDefines()
    {
      string strLine, strTmp;
      string[] strSplitLine;
      int i, lngTest;
      TDefine tdNewDefine = new TDefine();
      DateTime dtFileMod;
      agGlobalsSet = false;

      //clear global list
      agGlobal = Array.Empty<TDefine>();
      agGlobalCount = 0;
      //look for global file
      if (!File.Exists(AGIGame.agGameDir + "globals.txt"))
        return;
      //open file for input
      using var glbSR = new StringReader(AGIGame.agGameDir + "globals.txt");
      {
        //read in globals
        while (true)
        {
          //get next line
          strLine = glbSR.ReadLine();
          if (strLine == null) 
            break;
          //strip off comment (need a throwaway string for comment)
          string s = "";
          strLine = WinAGI.StripComments(strLine, ref s, false);
          //ignore blanks '''and comments(// or [)
          if (strLine.Length != 0)
          {
            //splitline into name and Value
            strSplitLine = strLine.Split("\t");//     vbTab
            //if not exactly two elements
            if (strSplitLine.Length != 1)
            {
              //not a valid global.txt; check for defines.txt
              strTmp = strLine.Substring(0, 8).ToLower();
              if (strTmp == "#define ")
                //set up splitline array
                Array.Resize(ref strSplitLine, 2);
              //strip off the define statement
              strLine = strLine.Substring(8, strLine.Length - 8);
              //extract define name
              i = strLine.IndexOf(" ");
              if (i != 0)
              {
                strSplitLine[0] = strLine.Substring(0, i - 1);
                strLine = strLine.Substring(i, strLine.Length - i);
                strSplitLine[1] = strLine.Trim();
              }
              else
              {
                //invalid; reset splitline so this line
                //gets ignored
                Array.Resize(ref strSplitLine, 0);
              }
            }
            //if exactly two elements
            if (strSplitLine.Length == 2)
            {
              //strSplitLine(0] has name
              tdNewDefine.Name = strSplitLine[0].Trim();
              //strSplitLine(1] has Value
              tdNewDefine.Value = strSplitLine[1].Trim();
              //validate define name
              lngTest = ValidateDefName(tdNewDefine.Name);
              if (lngTest == 0 || (lngTest >= 8 && lngTest <= 12))
              {
                lngTest = ValidateDefName(tdNewDefine.Name);
                lngTest = ValidateDefValue(tdNewDefine);
                if (lngTest == 0 || lngTest == 5 || lngTest == 6)
                {
                  //increment Count
                  agGlobalCount++;
                  //add it
                  Array.Resize(ref agGlobal, agGlobalCount);
                  agGlobal[agGlobalCount] = tdNewDefine;
                }
              }
            }
          }
        }
        //save crc for this file
        //get datemodified property
        dtFileMod = File.GetLastWriteTime(AGIGame.agGameDir + "globals.txt");
        agGlobalCRC = WinAGI.CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(dtFileMod.ToString()));
      }
    }
    internal static int ValidateDefName(string DefName)
    {
      //Public Function ValidateDefName(DefName As String) As Long
      // validates that DefValue is a valid define Value
      // 
      // returns 0 if successful
      // 
      // returns an error code on failure:
      // 1 = no name
      // 2 = name is numeric
      // 3 = name is command
      // 4 = name is test command
      // 5 = name is a compiler keyword
      // 6 = name is an argument marker
      // 7 = name is already globally defined
      // 8 = name is reserved variable name
      // 9 = name is reserved flag name
      // 10 = name is reserved number constant
      // 11 = name is reserved object constant
      // 12 = name is reserved message constant
      // 13 = name contains improper character
      int i;

      // if no name
      if (DefName.Length == 0)
        return 1;
      // name cant be numeric
      if (WinAGI.IsNumeric(DefName))
        return 2;
      // check against regular commands
      for (i = 0; i <= AGICommands.agCmds.Length; i++)
      {
        if (DefName == AGICommands.agCmds[i].Name)
          return 3;
      }
      // check against test commands
      for (i = 0; i <= AGITestCommands.agTestCmds.Length; i++)
      {
        if (DefName == AGITestCommands.agTestCmds[i].Name)
          return 4;
      }
      // check against keywords
      if ((DefName.ToLower() == "if") || (DefName.ToLower() == "else") ||
         (DefName.ToLower() == "goto"))
        return 5;
      // if the name starts with any of these letters
      if ("vfmoiswc".Any(DefName.ToLower().StartsWith))
        //if rest of string is numeric
        if (IsNumeric(Right(DefName, DefName.Length - 1)))
          // can't have a name that's a valid marker
          return 6;
      // check against current globals
      for (i = 0; i < WinAGI.agGlobalCount; i++)
      {
        if (DefName == agGlobal[i].Name)
          return 7;
      }
      // check against reserved defines:
      if (agUseRes)
      {
        // check against reserved variables
        for (i = 0; i <= 26; i++)
        {
          if (DefName == agResVar[i].Name)
            return 8;
        }
        // check against reserved flags
        for (i = 0; i <= 17; i++)
        {
          if (DefName == agResFlag[i].Name)
            return 9;
        }
        // check against other reserved constants
        for (i = 0; i <= 4; i++)
        {
          if (DefName == agEdgeCodes[i].Name)
            return 10;
        }
        for (i = 0; i <= 8; i++)
        {
          if (DefName == agEgoDir[i].Name)
            return 10;
        }
        for (i = 0; i <= 4; i++)
        {
          if (DefName == agVideoMode[i].Name)
            return 10;
        }
        // check against other reserved defines
        if (DefName == agResDef[4].Name)
          return 10;
        if (DefName == agResDef[0].Name)
          return 11;
        if (DefName == agResDef[5].Name)
          return 11;
        for (i = 1; i <= 3; i++)
        {
          if (DefName == agResDef[i].Name)
            return 12;
        }
      }
      // I am not sure exactly how this works, but it does; uses LINQ?
      // Any seems to apply the test inside to each element of the source
      // so testList.Any(checkItem.Op) returns true if checkItem.Op is true
      // for any element in testList!
      // to get control chars, I use a pre-built string
      if ((CTRL_CHARS + " !\"#$%&'()*+,-/:;<=>?@[\\]^`{|}~").Any(DefName.Contains))
        // bad
        return 13;
      // if no error conditions, it's OK
      return 0;
    }
    internal static int ValidateDefValue(TDefine TestDefine)
    {
      //validates that TestDefine.Value is a valid define Value
      //
      //returns 0 if successful
      //
      //returns an error code on failure:
      //1 = no Value
      //2 = Value is an invalid argument marker (not used anymore]
      //3 = Value contains an invalid argument Value
      //4 = Value is not a string, number or argument marker
      //5 = Value is already defined by a reserved name
      //6 = Value is already defined by a global name

      string strVal;
      int intVal;

      if (TestDefine.Value.Length == 0)
        return 1;
      //values must be a variable/flag/etc, string, or a number
      if (!IsNumeric(TestDefine.Value))
      {
        //if Value is an argument marker
        // use LINQ - if the name starts with any of these letters
        if ("vfmoiswc".Any(TestDefine.Value.ToLower().StartsWith))
        {
          //if rest of Value is numeric,
          strVal = TestDefine.Value.Substring(0, TestDefine.Value.Length - 1);
          if (IsNumeric(strVal))
          {
            //if Value is not between 0-255
            intVal = int.Parse(strVal);
            if (intVal < 0 || intVal > 255)
              return 3;
            //check defined globals
            for (int i = 0; i <= agGlobalCount - 1; i++)
            {
              //if this define has same Value
              if (agGlobal[i].Value == TestDefine.Value)
                return 6;
            }
            //verify that the Value is not already assigned
            switch ((int)TestDefine.Value.ToLower().ToCharArray()[0])
            {
              case 102: //flag
                TestDefine.Type = ArgTypeEnum.atFlag;
                if (WinAGI.agUseRes)
                  //if already defined as a reserved flag
                  if (intVal <= 15)
                    return 5;
                break;
              case 118: //variable
                TestDefine.Type = ArgTypeEnum.atVar;
                if (WinAGI.agUseRes)
                  //if already defined as a reserved variable
                  if (intVal <= 26)
                    return 5;
                break;
              case 109: //message
                TestDefine.Type = ArgTypeEnum.atMsg;
                break;
              case 111: //screen object
                TestDefine.Type = ArgTypeEnum.atSObj;
                if (WinAGI.agUseRes)
                  //can't be ego
                  if (TestDefine.Value == "o0")
                    return 5;
                break;
              case 105: //inv object
                TestDefine.Type = ArgTypeEnum.atIObj;
                break;
              case 115: //string
                TestDefine.Type = ArgTypeEnum.atStr;
                break;
              case 119: //word
                TestDefine.Type = ArgTypeEnum.atWord;
                break;
              case 99: //controller
                       //controllers limited to 0-49
                if (intVal > 49)
                  return 3;
                TestDefine.Type = ArgTypeEnum.atCtrl;
                break;
            }
            //Value is ok
            return 0;
          }
        }
        //non-numeric, non-marker and most likely a string
        TestDefine.Type = ArgTypeEnum.atDefStr;
        //check Value for string delimiters in Value
        if (TestDefine.Value.Substring(0, 1) != "\"" || TestDefine.Value.Substring(TestDefine.Value.Length - 1, 1) != "\"")
          return 4;
        else
          return 0;
      }
      else
      {
        // numeric
        TestDefine.Type = ArgTypeEnum.atNum;
        return 0;
      }
    }
    internal static string StripComments(string strLine, ref string strComment, bool NoTrim)
    {
      //Public Function StripComments(ByVal strLine As String, ByRef strComment As String, Optional ByVal NoTrim As Boolean = False) As String

      //strips off any comments on the line
      //if NoTrim is false, the string is also
      //stripped of any blank space

      //if there is a comment, it is passed back in the strComment argument

      //On Error GoTo ErrHandler

      // if line is empty, nothing to do
      if (strLine.Length == 0) return "";

      //reset rol ignore
      int intROLIgnore = -1;

      //reset comment start & char ptr, and inquotes
      int lngPos = -1;
      bool blnInQuotes = false;
      bool blnSlash = false;
      bool blnDblSlash = false;

      //assume no comment
      string strOut = strLine;
      strComment = "";

      //Do Until lngPos >= Len(strLine)
      while (lngPos < strLine.Length - 1)
      {
        //get next character from string
        lngPos++;
        //if NOT inside a quotation,
        if (!blnInQuotes)
        {
          //check for comment characters at this position
          if (Mid(strLine, lngPos, 2) == "//")
          {
            intROLIgnore = lngPos + 1;
            blnDblSlash = true;
            break;
          }
          else if (strLine.Substring(lngPos, 1) == "[")
          {
            intROLIgnore = lngPos;
            break;
          }
          // slash codes never occur outside quotes
          blnSlash = false;
          //if this character is a quote mark, it starts a string
          blnInQuotes = strLine.ElementAt(lngPos) == '"';
        }
        else
        {
          //if last character was a slash, ignore this character
          //because it's part of a slash code
          if (blnSlash)
          {
            //always reset  the slash
            blnSlash = false;
          }
          else
          {
            //check for slash or quote mark
            switch (strLine.ElementAt(lngPos))
            {
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
      if (intROLIgnore >= 0)
      {
        //save the comment
        strComment = Right(strLine, strLine.Length - intROLIgnore).Trim();

        //strip off the comment (intROLIgnore is 0 based, so add one)
        if (blnDblSlash)
        {
          strOut = Left(strLine, intROLIgnore - 1);
        }
        else
        {
          strOut = Left(strLine, intROLIgnore);
        }
      }

      if (!NoTrim)
        //return the line, trimmed
        strOut = strOut.Trim();

      return strOut;
      //Exit Function

      //ErrHandler:
      ////*'Debug.Assert False
      //  Resume Next
    }
    internal static void WriteGameSetting(string Section, string Key, string Value, string Group = "")
    {
      WriteAppSetting(agGameProps, Section, Key, Value, Group);

      if (Key.ToLower() != "lastedit" && Key.ToLower() != "winagiversion" && Key.ToLower() != "palette")
        agLastEdit = DateTime.Now;
    }
    internal static void WriteAppSetting(List<string> ConfigList, string Section, string Key, string Value, string Group)
    {
      //   Public Sub WriteAppSetting(ConfigList As StringList, ByVal Section As String, ByVal Key As String, ByVal Value As String, Optional ByVal Group As String = "")

      //elements of a settings file:
      //
      //  #comments begin with hashtag; all characters on line after hashtag are ignored
      //  comments can be added to end of valid section or key / value line
      //  blank lines are ignored
      //  [::BEGIN group::] marker to indicate a group of sections
      //  [::END group::]   marker to indicate end of a group
      //  [section]         sections indicated by square brackets; anything else on the line gets ignored
      //  key=value         key/value pairs separated by an equal sign; no quotes around values means only
      //                      single word; use quotes for multiword strings
      //  if string is multline, use '\n' control code (and use multiline option)
      int lngPos, i;
      string strLine, strCheck;
      int lngSectionEnd = 0;
      int lenKey; bool blnFound = false;
      int lngGrpStart, lngGrpEnd, lngInsertLine;

      //if value contains spaces, it must be enclosed in quotes
      if (Value.IndexOf(" ") > 0)
      {
        if (Value[0] != '"')
        {
          Value = "\"" + Value;
        }
        if ((Value[Value.Length - 1] != '"'))
        {
          Value += "\"";
        }
      }
      //if value contains any carriage returns, replace them with control characters
      if (Value.Contains("\r\n", StringComparison.OrdinalIgnoreCase))
      {
        Value = Value.Replace("\r\n", "\\n");
      }
      if (Value.Contains("\r", StringComparison.OrdinalIgnoreCase))
      {
        Value = Value.Replace("\r", "\\n");
      }
      if (Value.Contains("\n", StringComparison.OrdinalIgnoreCase))
      {
        Value = Value.Replace("\n", "\\n");
      }
      //if nullstring, include quotes
      if (Value.Length == 0)
      {
        Value = "\"\"";
      }
      //if a group is provided, we will add new items within the group;
      //existing items, even if within the group, will be left where they are
      lngGrpStart = -1;
      lngGrpEnd = -1;
      lngPos = -1;
      if (Group.Length > 0)
      {
        //********** we will have to make adjustments to group start
        //           and end positions later on as we add lines
        //           during the key update! don't forget to do that!!
        for (i = 1; i <= ConfigList.Count - 1; i++)
        {
          //skip blanks, and lines starting with a comment
          strLine = ConfigList[i].Replace("\t", " ").Trim();
          if (strLine.Length > 0)
          {
            //skip empty lines
            if (strLine[0] != '#')
            {
              //skip comments
              //if not found yet, look for the starting marker
              if (!blnFound)
              {
                //is this the group marker?
                if (strLine.Equals("[::BEGIN " + Group + "::]", StringComparison.OrdinalIgnoreCase))
                {
                  lngGrpStart = i;
                  blnFound = true;
                  //was the end found earlier? if so, we are done
                  if (lngGrpEnd >= 0)
                  {
                    break;
                  }
                }
              }
              else
              {
                //start is found; make sure we find end before
                //finding another start
                if (Left(strLine, 9).Equals("[::BEGIN ", StringComparison.OrdinalIgnoreCase))
                {
                  //mark position of first new start, so we can move the end marker here
                  if (lngPos < 0)
                  {
                    lngPos = i;
                  }
                }
              }
              //we check for end marker here even if start not found
              //just in case they are backwards
              if (strLine.Equals("[::END " + Group + "::]", StringComparison.OrdinalIgnoreCase))
              {
                lngGrpEnd = i;
                //and if we also have a start, we can exit the loop
                if (blnFound)
                {
                  break;
                }
              }
            }
          }
        }

        //possible outcomes:
        // - start and end both found; start before end
        //   this is what we want
        //
        // - start and end both found, but end before start
        //   this is backwards; we fix by moving end to
        //   the line after start
        //
        // - start found, but no end; we add end
        //   to just before next group start, or
        //   to end of file if no other group start
        //
        // - end found, but no start; we fix by putting
        //   start right in front of end

        // if both found
        if (lngGrpStart >= 0 && lngGrpEnd >= 0)
        {
          //if backwards, move end to line after start
          if (lngGrpEnd < lngGrpStart)
          {
            ConfigList.Insert(lngGrpStart + 1, ConfigList[lngGrpEnd]);
            ConfigList.RemoveAt(lngGrpEnd);
            lngGrpStart -= 1;
            lngGrpEnd = lngGrpStart + 1;
          }
        }
        // if only start found
        else if (lngGrpStart >= 0)
        {
          //means end not found
          //if there was another start found, insert end there
          if (lngPos > 0)
          {
            ConfigList.Insert(lngPos, "[::END " + Group + "::]");
            ConfigList.Insert(lngPos + 1, "");
            lngGrpEnd = lngPos;
          }
          else
          {
            //otherwise insert group end at end of file
            lngGrpEnd = ConfigList.Count;
            ConfigList.Add("[::END " + Group + "::]");
          }
        }
        // if end found but no start
        else if (lngGrpEnd >= 0)
        {
          //means start not found
          //insert start in front of end
          lngGrpStart = lngGrpEnd;
          ConfigList.Insert(lngGrpStart, "[::START " + Group + "::]");
          lngGrpEnd = lngGrpStart + 1;
        }
        else
        {
          //means neither found
          //make sure at least one blank line
          if (ConfigList[ConfigList.Count - 1].Trim().Length > 0)
          {
            ConfigList.Add("");
          }
          lngGrpStart = ConfigList.Count;
          ConfigList.Add("[::BEGIN " + Group + "::]");
          ConfigList.Add("[::END " + Group + "::]");
          lngGrpEnd = lngGrpStart + 1;
        }
      }
      //reset the found flag
      blnFound = false;
      //find the section we are looking for
      int lngSectionStart = FindSettingSection(ConfigList, Section);
      //if not found, create it at end of group (if group is provided)
      //otherwise at end of list
      if (lngSectionStart <= 0)
      {
        if (lngGrpStart >= 0)
        {
          lngInsertLine = lngGrpEnd;
        }
        else
        {
          lngInsertLine = ConfigList.Count;
        }
        //make sure there is at least one blank line (unless this is first line in list)
        if (lngInsertLine > 0)
        {
          if (ConfigList[lngInsertLine - 1].Trim().Length != 0)
          {
            ConfigList.Insert(lngInsertLine, "");
            lngInsertLine++;
          }
        }
        ConfigList.Insert(lngInsertLine, "[" + Section + "]");
        ConfigList.Insert(lngInsertLine + 1, "   " + Key + " = " + Value);
        //no need to check for location of section within group;
        //we just added it to the group (if one is needed)
      }
      else
      {
        //now step through all lines in this section; find matching key
        lenKey = Key.Length;
        for (i = lngSectionStart + 1; i < ConfigList.Count; i++)
        {
          //skip blanks, and lines starting with a comment
          strLine = ConfigList[i].Replace("\t", " ").Trim();
          if (strLine.Length > 0)
          {
            if (strLine[0] != '#')
            {
              //if another section is found, stop here
              if (strLine[0] == '[')
              {
                //if part of a group; last line of the section
                //is line prior to the new section
                if (lngGrpStart >= 0)
                {
                  lngSectionEnd = i - 1;
                }
                //if not already added, add it now
                if (!blnFound)
                {
                  //back up until a nonblank line is found
                  for (lngPos = i - 1; lngPos >= lngSectionStart; lngPos--)
                  {
                    if (ConfigList[lngPos].Trim().Length > 0)
                    {
                      break;
                    }
                  }
                  //add the key and value at this pos
                  ConfigList.Insert(lngPos + 1, "   " + Key + " = " + Value);
                  //this also bumps down the section end
                  lngSectionEnd++;
                  //it also may bump down group start/end
                  if (lngGrpStart >= lngPos + 1)
                  {
                    lngGrpStart++;
                  }
                  if (lngGrpEnd >= lngPos + 1)
                  {
                    lngGrpEnd++;
                  }
                }
                //we are done, but if part of a group
                //we need to verify the section is in
                //the group
                if (lngGrpStart >= 0)
                {
                  blnFound = true;
                  break;
                }
                else
                {
                  return;
                }
              }
              //if not already found,  look for 'key'
              if (!blnFound)
              {
                if (Left(strLine, lenKey).Equals(Key, StringComparison.OrdinalIgnoreCase) && (strLine.Substring(lenKey + 1, 1) == " " || strLine.Substring(lenKey + 1, 1) == "="))
                {
                  //found it- change key value to match new value
                  //(if there is a comment on the end, save it)
                  strLine = Right(strLine, strLine.Length - lenKey).Trim();
                  if (strLine.Length > 0)
                  {
                    //expect an equal sign
                    if (strLine[0] == '=')
                    {
                      //remove it
                      strLine = Right(strLine, strLine.Length - 1).Trim();
                    }
                  }
                  if (strLine.Length > 0)
                  {
                    if (strLine[0] == '"')
                    {
                      //string delimiter; find ending delimiter
                      lngPos = strLine.IndexOf('"', 1) + 1;
                    }
                    else
                    {
                      //look for comment marker
                      lngPos = strLine.IndexOf("#", 1);
                      // if none found, look for space delimiter
                      if (lngPos == -1)
                      {
                        //look for a space as a delimiter
                        lngPos = strLine.IndexOf(" ", 1);
                      }
                      ////look for a space as a delimiter
                      //lngPos = strLine.IndexOf(" ", 1);
                      //if (lngPos == -1)
                      //{
                      //  //could be a case where a comment is at end of text, without a space
                      //  //if so we need to keep the delimiter
                      //  lngPos = strLine.IndexOf("#", 2) - 1;
                      //}
                    }
                    //no delimiter found; assume entire line
                    if (lngPos <= 0)
                    {
                      lngPos = strLine.Length;
                    }
                    //now strip off the old value, leaving potential comment
                    strLine = Right(strLine, strLine.Length - lngPos).Trim();
                  }
                  //if something left, maks sure it's a comment
                  if (strLine.Length > 0)
                  {
                    if (strLine[0] != '#')
                    {
                      strLine = "#" + strLine;
                    }
                    //make sure it starts with a space
                    strLine = "   " + strLine;
                  }
                  strLine = "   " + Key + " = " + Value + strLine;
                  ConfigList[i] = strLine;
                  //we are done, but if part of a group
                  //we need to keep going to find end so
                  //we can validate section is in the group
                  if (lngGrpStart >= 0)
                  {
                    blnFound = true;
                  }
                  else
                  {
                    return;
                  }
                }
              }
            }
          }
        }
        //if not found (will only happen if this the last section in the
        //list, probably NOT in a group, but still possible (if the
        //section is outside the defined group)
        if (!blnFound)
        {
          //back up until a nonblank line is found
          for (lngPos = i - 1; i >= lngSectionStart; i--)
          {
            if (ConfigList[lngPos].Trim().Length > 0)
            {
              break;
            }
          }
          //add the key and value at this pos
          ConfigList.Insert(lngPos + 1, "   " + Key + " = " + Value);
          //we SHOULD be done, but just in case this section is
          //out of position, we still check for the group
          if (lngGrpStart < 0)
          {
            //no group - all done!
            return;
          }
          //note that we don't need to bother adjusting group
          //start/end, because we only added a line to the
          //end of the file, and we know that the group
          //start/end markers MUST be before the start
          //of this section
        }
        //found marker ONLY set if part a group so let's verify
        //the section is in the group, moving it if necessary

        //if this was last section, AND section is NOT in its group
        //then then section end won't be set yet
        if (lngSectionEnd <= 0)
        {
          lngSectionEnd = ConfigList.Count - 1;
        }
        //if the section is not in the group, then move it
        //(depends on whether section is BEFORE or AFTER group start)
        if (lngSectionStart < lngGrpStart)
        {
          //make sure at least one blank line above the group end
          if (ConfigList[lngGrpEnd - 1].Trim().Length > 0)
          {
            ConfigList.Insert(lngGrpEnd, "");
            lngGrpEnd++;
          }
          //add the section to end of group
          for (i = lngSectionStart; i <= lngSectionEnd; i++)
          {
            ConfigList.Insert(lngGrpEnd, ConfigList[i]);
            lngGrpEnd++;
          }
          //then delete the section from it's current location
          for (i = lngSectionStart; i <= lngSectionEnd; i++)
          {
            ConfigList.RemoveAt(lngSectionStart);
          }
        }
        else if (lngSectionStart > lngGrpEnd)
        {
          //make sure at least one blank line above the group end
          if (ConfigList[lngGrpEnd - 1].Trim().Length > 0)
          {
            ConfigList.Insert(lngGrpEnd, "");
            lngGrpEnd++;
            lngSectionStart++;
            lngSectionEnd++;
          }
          //add the section to end of group
          for (i = lngSectionEnd; i >= lngSectionStart; i--)
          {
            ConfigList.Insert(lngGrpEnd, ConfigList[lngSectionEnd]);
            //delete the line in current location
            ConfigList.RemoveAt(lngSectionEnd + 1);
          }
        }
      }
    }
    internal static void SaveSettingList(List<string> ConfigList)
    {
      //filename is first line
      //Dim strFileName As String
      //Dim intFile As Integer, bytData() As Byte, TempFile As String

      //get filename (and remove it; we don't need to save that)
      string strFileName = ConfigList[0];
      ConfigList.RemoveAt(0);
      //open temp file
      string TempFile = Path.GetTempFileName();
      try
      {
        using StreamWriter cfgSR = new StreamWriter(TempFile);
        //now output the results to the file
        foreach (string line in ConfigList)
          cfgSR.WriteLine(line);
        //// close it
        //cfgSR.Close();
        //dispose it
        cfgSR.Dispose();

        // delete current file
        File.Delete(strFileName);
        // now copy new to final destination
        File.Move(TempFile, strFileName);

        //add filename back
        ConfigList.Insert(0, TempFile);
      }
      catch (Exception)
      {
        // do we care if there is a file error?
        throw;
      }
    }
    internal static List<string> OpenSettingList(string ConfigFile, bool CreateNew = true)
    {
      List<string> stlConfig = new List<string> { };
      FileStream fsConfig;
      StreamWriter swConfig;
      StreamReader srConfig;

      if (File.Exists(ConfigFile) || CreateNew)
      {
        //open the config file for create/write
        fsConfig = new FileStream(ConfigFile, FileMode.OpenOrCreate);
        long lngLen = fsConfig.Length;
        //if this is an empty file (either previously empty or created by this call)
        if (lngLen == 0)
        {
          swConfig = new StreamWriter(fsConfig);
          //add a single comment to the file
          stlConfig.Add("#");
          // and write it to the file
          swConfig.WriteLine("#");
          swConfig.Dispose();
        }
        else
        {
          //grab the file data
          srConfig = new StreamReader(fsConfig);
          while (!srConfig.EndOfStream)
          {
            // opens ConfigFile as a SettingsList, and returns the file's text as
            // a SettingsList object

            // if file does not exist, a blank SettingsList object is passed back
            // if the CreateNew flag is set, the blank file is also saved to disk

            string strInput = srConfig.ReadLine();
            stlConfig.Add(strInput);
          }
          srConfig.Dispose();
        }
        fsConfig.Dispose();
      }
      else
      {
        //if file doesn't exist, and NOT forcing new file creation
        //just add a single comment as first line
        stlConfig.Add("#");
      }

      //always add filename as first line
      stlConfig.Insert(0, ConfigFile);
      //return the list
      return stlConfig;
    }
    public static string ReadAppSetting(List<string> ConfigList, string Section, string Key, string Default = "")
    {
      //need to make sure there is a list to read from
      if (ConfigList.Count == 0)
      {
        //return the default
        return Default;
      }
      //elements of a settings file:
      //
      //  #comments begin with hashtag; all characters on line after hashtag are ignored
      //  //comments can be added to end of valid section or key/value line
      //  blank lines are ignored
      //  [::BEGIN group::] marker to indicate a group of sections
      //  [::END group::] marker to indicate end of a group
      //  [section] sections indicated by square brackets; anything else on the line gets ignored
      //  key=value  key/value pairs separated by an equal sign; no quotes around values means only
      //    single word; use quotes for multiword strings
      //  if string is multline, use '\n' control code (and use multiline option)
      string strLine;
      int i;
      int lngPos;
      //find the section we are looking for (skip 1st line; it's the filename)
      int lngSection = FindSettingSection(ConfigList, Section);
      //if not found,
      if (lngSection < 0)
      {
        //add the section and the value
        WriteAppSetting(ConfigList, Section, Key, Default, "");
        //and return the default value
        return Default;
      }
      else
      {
        //step through all lines in this section; find matching key
        int lenKey = Key.Length;
        for (i = lngSection + 1; i < ConfigList.Count; i++)
        {
          //skip blanks, and lines starting with a comment
          strLine = ConfigList[i].Replace("\t", " ").Trim();
          if (strLine.Length > 0)
          {
            if (strLine[0] != '#')
            { //not a comment
              //if another section is found, stop here
              if (strLine[0] == '[')
              {
                break;
              }
              //look for 'key'
              //validate that this is an exact match, and not a key that starts with
              //the same letters by verifying next char is either a space, or an equal sign
              if (Left(strLine, lenKey).Equals(Key, StringComparison.OrdinalIgnoreCase) && (strLine.Substring(lenKey, 1) == " " || strLine.Substring(lenKey, 1) == "="))
              {
                //found it- extract value (if there is a comment on the end, drop it)
                //strip off key
                strLine = Right(strLine, strLine.Length - lenKey).Trim();
                //check for nullstring, incase line has ONLY the key and nothing else
                if (strLine.Length > 0)
                {
                  //expect an equal sign
                  if (strLine[0] == '=')
                  {
                    //remove it
                    strLine = Right(strLine, strLine.Length - 1).Trim();
                  }
                  if (strLine.Length > 0)
                  {
                    if (strLine[0] == '"')
                    {
                      //string delimiter; find ending delimiter
                      // (don't add 1; we want to drop the ending quote)
                      lngPos = strLine.IndexOf("\"", 1);
                    }
                    else
                    {
                      //look for comment marker (don't add to result -
                      // the coment marker gets stripped off)
                      lngPos = strLine.IndexOf("#", 1);
                    }
                    //no delimiter found; assume entire line
                    if (lngPos <= 0)
                    {
                      lngPos = strLine.Length;
                    }
                    //now strip off anything past value (including quote delimiter)
                    strLine = Left(strLine, lngPos).Trim();
                    if (strLine.Length > 0)
                    {
                      //if a leading quote, remove it
                      if (strLine[0] == '"')
                      {
                        strLine = Right(strLine, strLine.Length - 1);
                      }
                    }
                    //should never have an end quote; it will be caught as the ending delimiter
                    if (strLine.Length > 0)
                    {
                      if (Right(strLine, 1)[0] == '"')
                      {
                        strLine = Left(strLine, strLine.Length - 1);
                      }
                    }
                    if (strLine.IndexOf("\\n", 0) >= 0)
                    {
                      //replace any newline control characters
                      strLine = strLine.Replace("\\n", "\r\n");
                    }
                  }
                }
                return strLine;
              }
            }
          }
        }
        //not found// add it here
        //back up until a nonblank line is found
        for (lngPos = i - 1; i >= lngSection; i--)
        {
          if (ConfigList[lngPos].Trim().Length > 0)
          {
            break;
          }
        }
        //return the default value
        string sReturn = Default;
        //add the key and default value at this pos
        //if value contains spaces, it must be enclosed in quotes
        if (Default.IndexOf(" ", 0) >= 0)
        {
          if (Default[0] != '"')
          {
            Default = "\"" + Default;
          }
          if (Right(Default, 1)[0] != '"')
          {
            Default += "\"";
          }
        }
        //if Default contains any carriage returns, replace them with control characters
        if (Default.IndexOf("\r\n", 0) >= 0)
        {
          Default = Default.Replace("\r\n", "\\n");
        }
        if (Default.IndexOf("\r", 0) >= 0)
        {
          Default = Default.Replace("\r", "\\n");
        }
        if (Default.IndexOf("\n", 0) >= 0)
        {
          Default = Default.Replace("\n", "\\n");
        }
        if (Default.Length == 0)
        {
          Default = "\"\"";
        }
        ConfigList.Insert(lngPos + 1, "   " + Key + " = " + Default);
        return sReturn;
      }
    }
    public static int ReadSettingLong(List<string> ConfigList, string Section, string Key, int Default = 0)
    {
      //get the setting value; if it converts to long value, use it;
      //if any kind of error, return the default value
      string strValue = ReadAppSetting(ConfigList, Section, Key, Default.ToString());

      if (strValue.Length == 0)
      {
        return Default;
      }
      else if (Left(strValue, 2).Equals("0x", StringComparison.OrdinalIgnoreCase))
      {
        try
        {
          return Convert.ToInt32(strValue, 16);
        }
        catch (Exception)
        {
          return Default;
        }
      }
      else if (Left(strValue, 2).Equals("&H", StringComparison.OrdinalIgnoreCase))
      {
        try
        {
          return Convert.ToInt32(Right(strValue, strValue.Length - 2), 16);
        }
        catch (Exception)
        {
          return Default;
        }
      }
      else
      {
        if (int.TryParse(strValue, out int iResult))
        {
          return iResult;
        }
        else
        {
          return Default;
        }
      }
    }
    public static uint ReadSettingUint32(List<string> ConfigList, string Section, string Key, uint Default = 0)
    {
      //get the setting value; if it converts to long value, use it;
      //if any kind of error, return the default value
      string strValue = ReadAppSetting(ConfigList, Section, Key, Default.ToString());

      if (strValue.Length == 0)
      {
        return Default;
      }
      else if (Left(strValue, 2).Equals("0x", StringComparison.OrdinalIgnoreCase))
      {
        try
        {
          return Convert.ToUInt32(strValue, 16);
        }
        catch (Exception)
        {
          return Default;
        }
      }
      else if (Left(strValue, 2).Equals("&H", StringComparison.OrdinalIgnoreCase))
      {
        try
        {
          return Convert.ToUInt32(Right(strValue, strValue.Length - 2), 16);
        }
        catch (Exception)
        {
          return Default;
        }
      }
      else
      {

        if (uint.TryParse(strValue, out uint iResult))
        {
          return iResult;
        }
        else
        {
          return Default;
        }
      }
    }
    public static byte ReadSettingByte(List<string> ConfigList, string Section, string Key, byte Default = 0)
    {
      //get the setting value; if it converts to byte value, use it;
      //if any kind of error, return the default value
      string strValue = ReadAppSetting(ConfigList, Section, Key, Default.ToString());
      if (strValue.Length == 0)
      {
        return Default;
      }
      else
      {
        if (byte.TryParse(strValue, out byte bResult))
        {
          return bResult;
        }
        else
        {
          return Default;
        }
      }
    }
    public static double ReadSettingSingle(List<string> ConfigList, string Section, string Key, double Default = 0)
    {
      //get the setting value; if it converts to single value, use it;
      //if any kind of error, return the default value
      string strValue = ReadAppSetting(ConfigList, Section, Key, Default.ToString());

      if (strValue.Length == 0)
      {
        return Default;
      }
      else
      {
        if (double.TryParse(strValue, out double sResult))
        {
          return sResult;
        }
        else
        {
          return Default;
        }
      }
    }
    public static bool ReadSettingBool(List<string> ConfigList, string Section, string Key, bool Default = false)
    {
      //get the setting value; if it converts to boolean value, use it;
      //if any kind of error, return the default value
      string strValue = ReadAppSetting(ConfigList, Section, Key, Default.ToString());
      if (strValue.Length == 0)
      {
        return Default;
      }
      else
      {
        if (bool.TryParse(strValue, out bool bResult))
        {
          return bResult;
        }
        else
        {
          return Default;
        }
      }
    }
    public static string ReadSettingString(List<string> ConfigList, string Section, string Key, string Default = "")
    {
      //read a string value from the configlist


      return ReadAppSetting(ConfigList, Section, Key, Default);
    }
    public static void WriteProperty(string Section, string Key, string Value, string Group = "", bool ForceSave = false)
    {
      // this procedure provides calling programs a way to write property
      // values to the WAG file

      // no validation of section or newval is done, so calling function
      // needs to be careful

      try
      {
        WriteGameSetting(Section, Key, Value, Group);

        // if forcing a save
        if (ForceSave)
        {
          SaveProperties();
        }
      }
      catch (Exception)
      {

        //ignore if error?
      }
      return;
    }
    public static void DeleteSettingSection(List<string> ConfigList, string Section)
    {
      //elements of a settings file:
      //
      //  #comments begin with hashtag; all characters on line after hashtag are ignored
      //  //comments can be added to end of valid section or key/value line
      //  blank lines are ignored
      //  [section] sections indicated by square brackets; anything else on the line gets ignored
      //  key=value  key/value pairs separated by an equal sign; no quotes around values means only
      //    single word; use quotes for multiword strings
      //  if string is multline, use '\n' control code (and use multiline option)

      int lngPos, lngSection = 0, i;
      string strLine, strCheck;
      //find the section we are looking for (skip 1st line; it's the filename)
      for (i = 1; i < ConfigList.Count; i++)
      {
        //skip blanks, and lines starting with a comment
        strLine = ConfigList[i].Replace("\t", " ").Trim();
        if (strLine.Length > 0) 
        {
          if (strLine[0] != '#') 
          {
            //look for a bracket
            if (strLine[0] == '[')
            {
              //find end bracket
              lngPos = strLine.IndexOf("]", 1);
              if (lngPos >= 0) 
              {
                strCheck = Mid(strLine, 1, lngPos - 2);
              } 
              else
              {
                strCheck = Right(strLine, strLine.Length - 1);
              }
              if (strCheck.Equals(Section, StringComparison.OrdinalIgnoreCase)) 
              {
                //found it
                lngSection = i;
                  break;
                }
              }
            }
          }
        } //Next i

        //if not found,
        if (lngSection == 0) {
          //nothing to delete
          return;
        }

      //step through all lines in this section, deleting until another section or end of list is found
      do
      {  //delete this line
        ConfigList.RemoveAt(lngSection);

        //at end?
        if (lngSection >= ConfigList.Count) {
          return;
        }

        //or another section found?
        strLine = ConfigList[lngSection].Replace('\t', ' ').Trim();
        if (strLine.Length > 0)
        {
          //if another section is found, stop here
          if (strLine[0] == (char)91)
          {
            //nothing to delete
            return;
          }
        }
      } while (true);
    }
    public static void DeleteSettingKey(List<string> ConfigList, string Section, string Key)
    {
      //elements of a settings file:
      //
      //  #comments begin with hashtag; all characters on line after hashtag are ignored
      //  //comments can be added to end of valid section or key/value line
      //  blank lines are ignored
      //  [section] sections indicated by square brackets; anything else on the line gets ignored
      //  key=value  key/value pairs separated by an equal sign; no quotes around values means only
      //    single word; use quotes for multiword strings
      //  if string is multline, use '\n' control code (and use multiline option)

      int i;
      string strLine;
      int lngPos;
      string strCheck;
      int lngSection, lenKey;
      //find the section we are looking for (skip 1st line; it's the filename)
      lngSection = FindSettingSection(ConfigList, Section);
      //if not found,
      if (lngSection <= 0)
      {
        //nothing to delete
        return;
      }
      //step through all lines in this section; find matching key
      lenKey = Key.Length;
      for (i = lngSection + 1; i < ConfigList.Count; i++)
      {
        //skip blanks, and lines starting with a comment
        strLine = ConfigList[i].Replace("\t", " ").Trim();
        if (strLine.Length > 0)
        {
          if (strLine[0] != '#')
          { //not a comment
            //if another section is found, stop here
            if (strLine[0] == '[')
            {
              //nothing to delete
              return;
            }
            //look for key
            if (Left(strLine, lenKey) == Key)
            {
              //found it- delete this line
              ConfigList.RemoveAt(i);
              return;
            }
          }
        }
      }
      //not found - nothing to delete
    }
    private static int FindSettingSection(List<string> ConfigList, string Section)
    {
      int i, lngPos;
      string strLine;
      //find the section we are looking for (skip 1st line; it's the filename)
      for (i = 1; i <= ConfigList.Count - 1; i++)
      {
        //skip blanks, and lines starting with a comment
        strLine = ConfigList[i].Replace("\t", " ").Trim();
        if (strLine.Length > 0)
        {
          if (strLine[0] != '#')
          {
            //look for a bracket
            if (strLine[0] == '[')
            {
              //find end bracket
              lngPos = strLine.IndexOf("]", 1);
              string strCheck;
              if (lngPos > 0)
              {
                strCheck = Mid(strLine, 1, lngPos - 1);
              }
              else
              {
                strCheck = Right(strLine, strLine.Length - 1);
              }
              if (strCheck.Equals(Section, StringComparison.OrdinalIgnoreCase))
              {
                //found it
                return i;
              }
            }
          }
        }
      }
      // not found
      return -1;
    }
    internal static string GetIntVersion()
    {
      byte[] bytBuffer = new byte[] { 0 };
      FileStream fsVer;

      // version is in OVL file
      string strFileName = agGameDir + "AGIDATA.OVL";
      if (File.Exists(strFileName))
      {
        try
        {
          //open AGIDATA.OVL, copy to buffer, and close
          fsVer = new FileStream(strFileName, FileMode.Open);
          // get all the data
          bytBuffer = new byte[fsVer.Length];
          try
          {
            fsVer.Read(bytBuffer, 0, (int)fsVer.Length);
          }
          catch (Exception)
          {
            // ignore, treat as invalid
          }
          fsVer.Dispose();
        }
        catch (Exception)
        {
          //invalid - return a default
        }
      }


      // if no data (either no file, or bad data
      if (bytBuffer.Length == 0)
      {
        //no agidata.ovl
        //if version3 is set
        if (agIsVersion3)
        {
          //use default v3
          return "3.002149"; //most common version 3
        }
        else
        {
          //use default version  2.917
          return "2.917";
        }
      }

      // now try to extract the version
      long lngPos = 0;
      //go until a '2' or '3' is found
      while (lngPos >= bytBuffer.Length)
      {
        //this function gets the version number of a Sierra AGI game
        //if found, it is validated against list of versions
        //that WinAGI recognizes
        //
        //returns version number for a valid number
        //returns null string for invalid number


        string strVersion;
        int i;
        //check char
        switch (bytBuffer[lngPos])
        {
          case 50: //2.xxx format
            strVersion = "2";
            //get next four chars
            for (i = 1; i <= 4; i++)
            {
              lngPos++;
              //just in case, check for end of buffer
              if (lngPos >= bytBuffer.Length)
              {
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
            for (i = 1; i <= 7; i++)
            {
              lngPos++;
              //just in case, check for end of buffer
              if (lngPos >= bytBuffer.Length)
              {
                break;
              }

              //add this char (unless it's the second period)
              if (lngPos != 4)
              {
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
      if (agIsVersion3)
      {
        return "3.002149"; //most common version 3?
      }
      else
      {
        return "2.917";  // This is what we use if we can't find the version number.
                         // Version 2.917 is the most common interpreter and
                         // the one that all the "new" AGI games should be based on.
      }
    }
    internal static void RestoreDefaultColors()
    {
      //(note that reverse colors  are in RRGGBB format)
      lngEGARevCol[0] = 0; //black
      lngEGARevCol[1] = 0xA0; //blue
      lngEGARevCol[2] = 0xA000; //green
      lngEGARevCol[3] = 0xA0A0; //cyan
      lngEGARevCol[4] = 0xA00000; //red
      lngEGARevCol[5] = 0x8000A0; //magenta
      lngEGARevCol[6] = 0xA05000; //brown
      lngEGARevCol[7] = 0xA0A0A0; //light gray
      lngEGARevCol[8] = 0x505050; //dark gray
      lngEGARevCol[9] = 0x5050FF; //light blue
      lngEGARevCol[10] = 0xFF50; //light green
      lngEGARevCol[11] = 0x50FFFF; //light cyan
      lngEGARevCol[12] = 0xFF5050; //light red
      lngEGARevCol[13] = 0xFF50FF; //light magenta
      lngEGARevCol[14] = 0xFFFF50; //yellow
      lngEGARevCol[15] = 0xFFFFFF; //white
                                   //note regular colors are; //bbggrr' format
      lngEGACol[0] = 0; //black
      lngEGACol[1] = 0xA00000; //blue
      lngEGACol[2] = 0xA000; //green
      lngEGACol[3] = 0xA0A000; //cyan
      lngEGACol[4] = 0xA0; //red
      lngEGACol[5] = 0xA00080; //magenta
      lngEGACol[6] = 0x50A0; //brown
      lngEGACol[7] = 0xA0A0A0; //light gray
      lngEGACol[8] = 0x505050; //dark gray
      lngEGACol[9] = 0xFF5050; //light blue
      lngEGACol[10] = 0x50FF00; //light green
      lngEGACol[11] = 0xFFFF50; //light cyan
      lngEGACol[12] = 0x5050FF; //light red
      lngEGACol[13] = 0xFF50FF; //light magenta
      lngEGACol[14] = 0x50FFFF; //yellow
      lngEGACol[15] = 0xFFFFFF; //white
    }
    public static bool IsValidGameDir(string strDir)
    {
      string strFile;
      byte[] bChunk = new byte[6];
      FileStream fsCOM;

      //this function will determine if the strDir is a
      //valid sierra AGI game directory
      //it also sets the gameID, if one is found and the version3 flag
      //search for 'DIR' files
      int dirCount;
      try
      {
        dirCount = Directory.EnumerateFiles(strDir, "*DIR").Count();
      }
      catch (Exception)
      {
        // if error, assume NOT a directory
        return false;
      }

      if (dirCount > 0)
      {
        //this might be an AGI game directory-
        // if exactly four dir files
        if (dirCount == 4)
        {
          // assume it's a v2 game

          // check for at least one VOL file
          if (File.Exists(strDir + "VOL.0"))
          {
            //clear version3 flag
            agIsVersion3 = false;

            //clear ID
            agGameID = "";

            //look for loader file to find ID
            foreach (string strLoader in Directory.EnumerateFiles(strDir, "*.COM"))
            {
              //open file and get chunk
              string strChunk = new string(' ', 6);
              using (fsCOM = new FileStream(strLoader, FileMode.Open))
              {
                // see if the word 'LOADER' is at position 3 of the file
                fsCOM.Position = 3;
                fsCOM.Read(bChunk, 0, 6);
                strChunk = Encoding.UTF8.GetString(bChunk);
                fsCOM.Dispose();

                //if this is a Sierra loader
                if (strChunk == "LOADER")
                {
                  // determine ID to use
                  //if not SIERRA.COM
                  strFile = JustFileName(strLoader);
                  if (strLoader != "SIERRA.COM")
                  {
                    //use this filename as ID
                    agGameID = Left(strFile, strFile.Length - 4).ToUpper();
                    return true;
                  }
                }
              }
            }

            //if no loader file found (looped through all files, no luck)
            //use default
            agGameID = "AGI";
            return true;
          }
        }
        else if (dirCount == 1)
        {
          //if only one, it's probably v3 game
          strFile = Directory.GetFiles(strNewDir, "*DIR")[0].ToUpper();
          agGameID = Left(strFile, strFile.IndexOf("DIR"));

          // check for matching VOL file;
          if (File.Exists(strDir + agGameID + "VOL.0"))
          {
            //set version3 flag
            agIsVersion3 = true;
            return true;
          }

          //if no vol file, assume not valid
          agGameID = "";
          return false;
        }
      }

      // no valid files/loader found; not an AGI directory
      return false;
    }
    internal static void ConvertWag()
    {

      //converts a v1.2.1 propfile to current version proplist
      // TODO: should return a bool; true if success, fals if not

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


      //remove everything except first line in wag file
      if (agGameProps.Count > 1)
      {
        agGameProps.RemoveRange(1, agGameProps.Count - 2);
      }
      agGameProps.Add("#");
      agGameProps.Add("# WinAGI Game Property File");
      agGameProps.Add("# converted from version 1.2.1");
      agGameProps.Add("#");
      agGameProps.Add("[General]");
      agGameProps.Add("   WinAGIVersion = " + WINAGI_VERSION);

      //open old file
      FileStream fsOldWag = new FileStream(agGameFile, FileMode.Open);
      //verify version
      bytData = new byte[16];
      //adjust position to compensate for length of variable
      //: fsOldWag.Length - 16;
      fsOldWag.Read(bytData, (int)fsOldWag.Length - 16, 16);
      strValue = Encoding.UTF8.GetString(bytData);

      //if version is incompatible
      switch (strValue)
      {
        case WINAGI_VERSION_1_2:
        case WINAGI_VERSION_1_0:
        case WINAGI_VERSION_BETA:
          break;
        //ok
        default:
          //return nothing
          fsOldWag.Dispose();
          agGameProps = new List<string> { };
          return;
      }

      //don't copy version line into buffer
      lngCount = (int)fsOldWag.Length - 16;
      if (lngCount > 0)
      {
        bytData = new byte[lngCount];
        fsOldWag.Read(bytData, 0, lngCount);
      }
      else
      {
        //set to zero
        lngCount = 0;
      }
      lngPos = 0;

      //get codes
      while (lngPos < lngCount)
      {
        //reset propval
        strValue = "";
        //get prop data
        PropCode = bytData[lngPos];
        PropType = bytData[lngPos + 1];
        ResNum = bytData[lngPos + 2];
        PropSize = bytData[lngPos + 3] + 256 * bytData[lngPos + 4];
        for (i = 1; i < PropSize; i++)
        {
          strValue += (char)bytData[lngPos + 4 + i];
        }


        if (PropCode >= PC_LOGIC)
        {
          switch (PropCode)
          {
            case PC_LOGIC:
              switch (PropType)
              {
                case PT_ID:
                  WriteGameSetting("Logic" + ResNum.ToString(), "ID", strValue, "Logics");
                  break;
                case PT_DESC:
                  WriteGameSetting("Logic" + ResNum.ToString(), "Description", strValue, "Logics");
                  break;
                case PT_CRC32:
                  WriteGameSetting("Logic" + ResNum.ToString(), "CRC32", "&H" + strValue, "Logics");
                  break;
                case PT_COMPCRC32:
                  WriteGameSetting("Logic" + ResNum.ToString(), "CompCRC32", "&H" + strValue, "Logics");
                  break;
                case PT_ROOM:
                  if (ResNum == 0)
                  {
                    //force to false
                    strValue = "False";
                  }
                  WriteGameSetting("Logic" + ResNum.ToString(), "IsRoom", strValue, "Logics");
                  break;
                case PT_SIZE:
                  WriteGameSetting("Logic" + ResNum.ToString(), "Size", strValue, "Logics");
                  break;

                default:
                  //unknown code; ignore it
                  //*'Debug.Assert False
                  break;
              }
              break;

            case PC_PICTURE:
              switch (PropType)
              {
                case PT_ID:
                  WriteGameSetting("Picture" + ResNum.ToString(), "ID", strValue, "Pictures");
                  break;
                case PT_DESC:
                  WriteGameSetting("Picture" + ResNum.ToString(), "Description", strValue, "Pictures");
                  break;
                case PT_SIZE:
                  WriteGameSetting("Picture" + ResNum.ToString(), "Size", strValue, "Pictures");
                  break;
                case PT_BKIMG:
                  WriteGameSetting("Picture" + ResNum.ToString(), "BkgdImg", strValue, "Pictures");
                  break;
                case PT_BKPOS:
                  WriteGameSetting("Picture" + ResNum.ToString(), "BkgdPosn", strValue, "Pictures");
                  break;
                case PT_BKSZ:
                  WriteGameSetting("Picture" + ResNum.ToString(), "BkgdSize", strValue, "Pictures");
                  break;
                case PT_BKTRANS:
                  WriteGameSetting("Picture" + ResNum.ToString(), "BkgdTrans", strValue, "Pictures");
                  break;
                default:
                  //unknown code; ignore it
                  //*'Debug.Assert False
                  break;
              }
              break;


            case PC_SOUND:
              switch (PropType)
              {
                case PT_ID:
                  WriteGameSetting("Sound" + ResNum.ToString(), "ID", strValue, "Sounds");
                  break;
                case PT_DESC:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Description", strValue, "Sounds");
                  break;
                case PT_SIZE:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Size", strValue, "Sounds");
                  break;
                case PT_KEY:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Key", strValue, "Sounds");
                  break;
                case PT_TPQN:
                  WriteGameSetting("Sound" + ResNum.ToString(), "TQPN", strValue, "Sounds");
                  break;
                case PT_INST0:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Inst0", strValue, "Sounds");
                  break;
                case PT_INST1:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Inst1", strValue, "Sounds");
                  break;
                case PT_INST2:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Inst2", strValue, "Sounds");
                  break;
                case PT_MUTE0:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Mute0", strValue, "Sounds");
                  break;
                case PT_MUTE1:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Mute1", strValue, "Sounds");
                  break;
                case PT_MUTE2:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Mute2", strValue, "Sounds");
                  break;
                case PT_MUTE3:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Mute3", strValue, "Sounds");
                  break;
                case PT_VIS0:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Visible0", strValue, "Sounds");
                  break;
                case PT_VIS1:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Visible1", strValue, "Sounds");
                  break;
                case PT_VIS2:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Visible2", strValue, "Sounds");
                  break;
                case PT_VIS3:
                  WriteGameSetting("Sound" + ResNum.ToString(), "Visible3", strValue, "Sounds");
                  break;
                default:
                  //unknown code; ignore it
                  //*'Debug.Assert False
                  break;
              }
              break;

            case PC_VIEW:
              switch (PropType)
              {
                case PT_ID:
                  WriteGameSetting("View" + ResNum.ToString(), "ID", strValue, "Views");
                  break;
                case PT_DESC:
                  WriteGameSetting("View" + ResNum.ToString(), "Description", strValue, "Views");
                  break;
                case PT_SIZE:
                  WriteGameSetting("View" + ResNum.ToString(), "Size", strValue, "Views");
                  break;
                default:
                  //unknown code; ignore it
                  //*'Debug.Assert False
                  break;
              }
              break;
          }

        }
        else
        {
          switch (PropCode)
          {
            case PC_GAMEDESC:
              WriteGameSetting("General", "Description", strValue);
              break;

            case PC_GAMEAUTHOR:
              WriteGameSetting("General", "Author", strValue);
              break;
            case PC_GAMEID:
              blnFoundID = strValue.Length > 0;
              WriteGameSetting("General", "GameID", strValue);
              break;
            case PC_INTVERSION:
              WriteGameSetting("General", "Interpreter", strValue);
              blnFoundVer = strValue.Length > 0;
              break;
            case PC_GAMEABOUT:
              WriteGameSetting("General", "About", strValue);
              break;
            case PC_GAMEVERSION:
              WriteGameSetting("General", "GameVersion", strValue);
              break;
            case PC_RESDIR:
              WriteGameSetting("General", "ResDir", strValue);
              break;
            case PC_GAMELAST:
              WriteGameSetting("General", "LastEdit", strValue);
              break;
            case PC_INVOBJDESC:
              WriteGameSetting("OBJECT", "Description", strValue);
              break;
            case PC_VOCABWORDDESC:
              WriteGameSetting("WORDS.TOK", "Description", strValue);
              break;
            case PC_GAMEEXEC:
              //////WriteGameSetting("General", "Exec", strValue);
              //Exec property no longer supported
              break;

            case PC_PALETTE:  //TODO: all hex strings need to be stored as '0x00', not '&H00'
                              //convert the color bytes into long values
              for (i = 0; i < 16; i++)
              {
                strValue = "0x";
                strValue += bytData[lngPos + 5 + 4 * i].ToString("x2") +
                            bytData[lngPos + 6 + 4 * i].ToString("x2") +
                            bytData[lngPos + 7 + 4 * i].ToString("x2") +
                            bytData[lngPos + 8 + 4 * i].ToString("x2");
                WriteGameSetting("Palette", "Color" + i.ToString(), strValue);
              }
              break;
            case PC_USERESNAMES:
              WriteGameSetting("General", "UseResNames", strValue);
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
      if (!blnFoundID || blnFoundVer)
      {
        agGameProps = new List<string> { };
      }
    }
    internal static void RecordLogEvent(LogEventType leType, string strMessage)
    {
      //open the log file and write the message
      //leType =0 means warning
      //inttype =1 means error
      string strType = "";

      //set type of msg
      strType = leType == LogEventType.leWarning ? "WARNING: " : "ERROR: ";
      if (leType == LogEventType.leWarning)
      {
        strType = "WARNING: ";
      }
      else
      {
        strType = "ERROR: ";
      }

      using (FileStream fsErrLog = new FileStream(agGameDir, FileMode.Append))
      {
        using (StreamWriter swErrLog = new StreamWriter(fsErrLog))
        {
          swErrLog.WriteLine(DateTime.Now.ToString("Medium Date") + ": " + strType + strMessage);
        }
      }
    }
    internal static void GetGameProperties()
    {
      //what's loaded BEFORE we get here:
      // General:
      //  GameID
      //  Interpreter
      //  ResDir

      //ASSUMES a valid game property file has been loaded
      //loads only these properties:
      //
      //  Palette:
      //     all colors
      //
      //  General:
      //     description
      //     author
      //     about
      //     game version
      //     last date
      //     platform, platform program, platform options, dos executable
      //     use res names property
      //     use layout editor

      string strSection, strLine;
      string[] strColor = new string[15];
      uint tmpColor;

      //Palette: (make sure AGI defaults set first)
      RestoreDefaultColors();
      for (int i = 0; i < 16; i++)
      {
        //validate it//s a good number before writing it
        strLine = ReadSettingString(agGameProps, "Palette", "Color" + i.ToString(), "0x" + lngEGACol[i].ToString("x8")).Trim();
        switch (Left(strLine, 2))
        {
          case "0x":
            // convert hex to int
            try
            {
              lngEGACol[i] = Convert.ToUInt32(strLine, 16);
            }
            catch (Exception)
            {
              // keep default
            }
            break;
          case "&H":
            // strip off '&H' and convert hex to int
            try
            {
              lngEGACol[i] = Convert.ToUInt32(Right(strLine, 8), 16);
            }
            catch (Exception)
            {
              // maybe it's an old (&H0,&H0,&H0,&H0) format
              bool bIsOK = false;
              tmpColor = 0;
              //split into individual color components
              strColor = strLine.Split(",");
              if (strColor.Length == 4)
              {
                bIsOK = true;
                tmpColor = 0;
                for (int j = 0; j < 4; j++)
                {
                  if (Left(strColor[j].Trim(), 2) == "&H")
                  {
                    strColor[j] = "0x" + Right(strColor[j].Trim(), 2);
                    if (IsNumeric(strColor[j]))
                    {
                      tmpColor += Convert.ToUInt32(strColor[j], 16) << (8 * (3 - j));
                    }
                    else
                    {
                      //no good
                      bIsOK = false;
                      break;
                    }
                  }
                  else
                  {
                    //no good
                    bIsOK = false;
                    break;
                  }
                }
              }
              // if still ok,
              if (bIsOK)
              {
                // keep the temp color
                lngEGACol[i] = tmpColor;
              }
            }
            // update the file to current format
            WriteGameSetting("Palette", "Color" + i.ToString(), "0x" + lngEGACol[i].ToString("x8"));
            break;
          default:
            // no good; keep default
            break;
        }
        //invert red and blue components for revcolor
        lngEGARevCol[i] = lngEGACol[i] & 0xFF000000;
        lngEGARevCol[i] += ((lngEGACol[i] & 0xFF) << 16) + (lngEGACol[i] & 0xFF00) + ((lngEGACol[i] & 0xFF0000) >> 16);
      }
      //description
      agDescription = ReadSettingString(agGameProps, "General", "Description");

      //author
      agAuthor = ReadSettingString(agGameProps, "General", "Author");

      //about
      agAbout = ReadSettingString(agGameProps, "General", "About");

      //game version
      agGameVersion = ReadSettingString(agGameProps, "General", "GameVersion");


      if (!DateTime.TryParse(ReadSettingString(agGameProps, "General", "LastEdit", DateTime.Now.ToString()), out agLastEdit))
      {
        // default to now
        agLastEdit = DateTime.Now;
      }

      //platform
      agPlatformType = ReadSettingLong(agGameProps, "General", "PlatformType", 0);

      //platform program
      agPlatformFile = ReadSettingString(agGameProps, "General", "Platform");

      //dos executable
      agDOSExec = ReadSettingString(agGameProps, "General", "DOSExec");

      //platform options
      agPlatformOpts = ReadSettingString(agGameProps, "General", "PlatformOpts");

      //use res names property (use current value, if one not found in property file)
      agUseRes = ReadSettingBool(agGameProps, "General", "UseResNames", agUseRes);

      // use layout editor property
      agUseLE = ReadSettingBool(agGameProps, "General", "UseLE");
    }
    internal static void CompleteCancel(bool NoEvent = false)
    {
      //cleans up after a compile game cancel or error

      if (!NoEvent)
      {
        Raise_CompileGameEvent(ECStatus.csCanceled, 0, 0, "");
      }
      agCompGame = false;
      fsDIR.Dispose();
      fsVOL.Dispose();
      bwVOL.Dispose();
      bwDIR.Dispose();
    }
  }
}
