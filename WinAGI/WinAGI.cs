﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static WinAGI.AGIGame;

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
    public byte ArgCount;
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
    internal static int[] lngEGARevCol = new int[16]; //15
    internal static int[] lngEGACol = new int[16]; //15;
    internal static AGIColors[] agColor = new AGIColors[16]; //15;
    internal static byte[] bytEncryptKey = { (byte)'A', (byte)'v', (byte)'i',
                             (byte)'s', (byte)' ', (byte)'D',
                             (byte)'u', (byte)'r', (byte)'g',
                             (byte)'a', (byte)'n' }; //10; //' = "Avis Durgan"

    // string arrays that are 'enums'
    public static readonly string[] ResTypeAbbrv = { "LOG", "PIC", "VIEW", "SND" };
    public static readonly string[] ResTypeName = { "Logic", "Picture", "View", "Sound" };
    public static readonly string[] IntVersions = new string[16]; //15; // (2.917, etc)

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

    //user defined global arguments
    internal static TDefine[] agGlobal; //dynamic size
    internal static int agGlobalCount;
    internal static bool agGlobalsSet;
    internal static uint agGlobalCRC;

    //warning count value stored in Common file, so it can be used by the IDE as well as the engine
    internal static bool[] agNoCompWarn; // WARNCOUNT

    static WinAGI()
    {
      for (int i = 1; i < 32; i++)
        CTRL_CHARS += ((char)i).ToString();

      CRC32Setup();
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

    //static internal uint CRC32(byte[] DataIn)
    internal static uint CRC32(char[] DataIn)
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
      uint index = 0, z;
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

      //Debug.Assert agGameLoaded

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

          if (strLine == null) break;


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
                //Select Case lngTest
                //Case 0, 8, 9, 10, 11, 12 'name is valid or is overriding a reserved define
                lngTest = ValidateDefName(tdNewDefine.Name);
                lngTest = ValidateDefValue(tdNewDefine);
                if (lngTest == 0 || lngTest == 5 || lngTest == 6)
                {
                  //Select Case lngTest
                  //Case 0, 5, 6 //Value is valid, or is overriding a reserved Value
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
        agGlobalCRC = WinAGI.CRC32(dtFileMod.ToString().ToCharArray());
        //ErrHandler:
        //        '*'Debug.Assert False

        //'ensure file is closed
        //Close intFile
        //'return false
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

      //On Error GoTo ErrHandler

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

      // use LINQ - if the name starts with any of these letters
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

      //On Error GoTo ErrHandler

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

      //ErrHandler:
      //          strError = Err.Description
      //strErrSrc = Err.Source
      //lngError = Err.Number

      //On Error GoTo 0: Err.Raise vbObjectError +660, strErrSrc, Replace(LoadResString(660), ARG1, CStr(lngError) & ":" & strError)
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
        lngPos += 1;
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
      int lngSectionStart = 0, lngSectionEnd = 0;
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
      if (Value.Contains("\r\n", StringComparison.CurrentCulture))
      {
        Value = Value.Replace("\r\n", "\\n");
      }
      if (Value.Contains("\r", StringComparison.CurrentCulture))
      {
        Value = Value.Replace("\r", "\\n");
      }
      if (Value.Contains("\n", StringComparison.CurrentCulture))
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
                if (strLine.Equals("[::BEGIN " + Group + "::]", StringComparison.CurrentCultureIgnoreCase))
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
                if (Left(strLine, 9).Equals("[::BEGIN ", StringComparison.CurrentCultureIgnoreCase))
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
              if (strLine.Equals("[::END " + Group + "::]", StringComparison.CurrentCultureIgnoreCase))
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
      for (i = 0; i <= ConfigList.Count - 1; i++)
      {
        //skip blanks, and lines starting with a comment
        strLine = ConfigList[i].Replace("\t", " ").Trim();
        if (strLine.Length > 0)
        { //skip empty lines
          if (strLine[0] != '#')
          { //skip comments
            //look for a bracket
            if (strLine[0] == '[')
            {
              //find end bracket
              lngPos = strLine.IndexOf("]", 2);
              if (lngPos > 0)
              {
                strCheck = strLine.Substring(2, lngPos - 2);
              }
              else
              {
                strCheck = Right(strLine, strLine.Length - 1);
              }
              if (strCheck.Equals(Section, StringComparison.CurrentCultureIgnoreCase))
              {
                //found it
                lngSectionStart = i;
                break;
              }
            }
          }
        }
      }


      //if not found, create it at end of group (if group is provided)
      //otherwise at end of list
      if (lngSectionStart == 0)
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
        for (i = lngSectionStart + 1; i <= ConfigList.Count - 1; i++)
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
                  for (lngPos = i - 1; i >= lngSectionStart; i--)
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
                if (Left(strLine, lenKey).Equals(Key, StringComparison.CurrentCultureIgnoreCase) && (strLine.Substring(lenKey + 1, 1) == " " || strLine.Substring(lenKey + 1, 1) == "="))
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
                      lngPos = strLine.IndexOf('"', 2);
                    }
                    else
                    {
                      //look for a space as a delimiter
                      lngPos = strLine.IndexOf(" ", 2);
                    }
                    if (lngPos == 0)
                    {
                      //could be a case where a comment is at end of text, without a space
                      //if so we need to keep the delimiter
                      lngPos = strLine.IndexOf("#", 2) - 1;
                    }
                    //no delimiter found; assume entire line
                    if (lngPos <= 0)
                    {
                      //no adjustment; we want to keep delimiter and anything after
                      lngPos = strLine.Length;
                    }
                    //now strip off the old value
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
        //*'Debug.Assert lngGrpStart >= 0


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
      string TempFile = System.IO.Path.GetTempFileName();

      try
      {
        using StreamWriter cfgSR = new StreamWriter(TempFile);

        //now output the results to the file
        foreach (string line in ConfigList)
          cfgSR.WriteLine(line);

        // close it
        cfgSR.Close();
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
        fsConfig = new FileStream(ConfigFile, FileMode.Create);
        long lngLen = fsConfig.Length;
        swConfig = new StreamWriter(fsConfig);
        //if this is an empty file (either previously empty or created by this call)
        if (lngLen == 0)
        {
          //add a single comment to the file
          stlConfig.Add("#");
          // and write it to the file
          swConfig.WriteLine("#");
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
        swConfig.Dispose();
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

    internal static string ReadAppSetting(List<string> ConfigList, string Section, string Key, string Default = "")
    {
      int lngSection = -1;


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
                strCheck = Mid(strLine, 2, lngPos - 2);
              }
              else
              {
                strCheck = Right(strLine, strLine.Length - 1);
              }
              if (strCheck.Equals(Section, StringComparison.CurrentCultureIgnoreCase))
              {
                //found it
                lngSection = i;
                break;
              }
            }
          }
        }
      }

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
        for (i = lngSection + 1; i <= ConfigList.Count - 1; i++)
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
              if (Left(strLine, lenKey).Equals(Key, StringComparison.CurrentCultureIgnoreCase) && (strLine.Substring(lenKey, 1) == " " || strLine.Substring(lenKey, 1) == "="))
              {
                //validate that this is an exact match, and not a key that starts with
                //the same letters by verifying next char is either a space, or an equal sign

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
                  if (strLine[0] == '"')
                  {
                    //string delimiter; find ending delimiter
                    lngPos = strLine.IndexOf("\"", 1);
                  }
                  else
                  {
                    //look for comment marker
                    lngPos = strLine.IndexOf("#", 1);
                  }
                  //no delimiter found; assume entire line
                  if (lngPos < 0)
                  {
                    //adjust by one so last char doesn't get chopped off
                    lngPos = strLine.Length + 1;
                  }
                  //now strip off anything past value (including delimiter
                  strLine = Left(strLine, lngPos - 1).Trim();
                  if (strLine.Length > 0)
                  {
                    //if in quotes, remove them
                    if (strLine[0] == '"')
                    {
                      strLine = Right(strLine, strLine.Length - 1);
                    }
                  }
                  //should never have an end quote; it will be caught as the ending delimiter
                  if (strLine.Length > 0)
                  {
                    if (Right(strLine, 0)[0] == '"')
                    {
                      //*'Debug.Assert False
                      strLine = Left(strLine, strLine.Length - 1);
                    }
                  }


                  if (strLine.IndexOf("\\n", 0) >= 0)
                  {
                    //replace any newline control characters
                    strLine = strLine.Replace("\\n", "\r\n");
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
        if (Default.IndexOf("\n", 1) >= 0)
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
    static int ReadSettingLong(List<string> ConfigList, string Section, string Key, int Default = 0)
    {
      //get the setting value; if it converts to long value, use it;
      //if any kind of error, return the default value
      string strValue = ReadAppSetting(ConfigList, Section, Key, Default.ToString());

      if (strValue.Length == 0)
      {
        return Default;
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

    static byte ReadSettingByte(List<string> ConfigList, string Section, string Key, byte Default = 0)
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
    internal static double ReadSettingSingle(List<string> ConfigList, string Section, string Key, double Default = 0)
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
    static internal bool ReadSettingBool(List<string> ConfigList, string Section, string Key, bool Default = false)
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

    internal static string ReadSettingString(List<string> ConfigList, string Section, string Key, string Default = "")
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
      int dirCount = Directory.EnumerateFiles(strDir, "*DIR").Count();
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

      byte[] bytData = new byte[0];
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
  }
}
