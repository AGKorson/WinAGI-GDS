using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
}
