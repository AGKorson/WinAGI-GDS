using System;

namespace WinAGI.Engine
{
  public static class AGITestCommands
  {
    internal static CommandStruct[] agTestCmds = new CommandStruct[20]; //19  //PC version has 18 test commands; Amiga appears to have 19
    public static string strErrSource;

    internal static byte agNumTestCmds;
    static AGITestCommands()
    {
      strErrSource = "AGITestCommands";

      agNumTestCmds = 20;

      agTestCmds[0].Name = "return.false";
      agTestCmds[0].ArgType = Array.Empty<ArgTypeEnum>();

      agTestCmds[1].Name = "equaln";
      agTestCmds[1].ArgType = new ArgTypeEnum[2];
      agTestCmds[1].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[1].ArgType[1] = ArgTypeEnum.atNum;

      agTestCmds[2].Name = "equalv";
      agTestCmds[2].ArgType = new ArgTypeEnum[2];
      agTestCmds[2].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[2].ArgType[1] = ArgTypeEnum.atVar;

      agTestCmds[3].Name = "lessn";
      agTestCmds[3].ArgType = new ArgTypeEnum[2];
      agTestCmds[3].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[3].ArgType[1] = ArgTypeEnum.atNum;

      agTestCmds[4].Name = "lessv";
      agTestCmds[4].ArgType = new ArgTypeEnum[2];
      agTestCmds[4].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[4].ArgType[1] = ArgTypeEnum.atVar;

      agTestCmds[5].Name = "greatern";
      agTestCmds[5].ArgType = new ArgTypeEnum[2];
      agTestCmds[5].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[5].ArgType[1] = ArgTypeEnum.atNum;

      agTestCmds[6].Name = "greaterv";
      agTestCmds[6].ArgType = new ArgTypeEnum[2];
      agTestCmds[6].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[6].ArgType[1] = ArgTypeEnum.atVar;

      agTestCmds[7].Name = "isset";
      agTestCmds[7].ArgType = new ArgTypeEnum[1];
      agTestCmds[7].ArgType[0] = ArgTypeEnum.atFlag;

      agTestCmds[8].Name = "issetv";
      agTestCmds[8].ArgType = new ArgTypeEnum[1];
      agTestCmds[8].ArgType[0] = ArgTypeEnum.atVar;

      agTestCmds[9].Name = "has";
      agTestCmds[9].ArgType = new ArgTypeEnum[1];
      agTestCmds[9].ArgType[0] = ArgTypeEnum.atIObj;

      agTestCmds[10].Name = "obj.in.room";
      agTestCmds[10].ArgType = new ArgTypeEnum[2];
      agTestCmds[10].ArgType[0] = ArgTypeEnum.atIObj;
      agTestCmds[10].ArgType[1] = ArgTypeEnum.atVar;

      agTestCmds[11].Name = "posn";
      agTestCmds[11].ArgType = new ArgTypeEnum[5];
      agTestCmds[11].ArgType[0] = ArgTypeEnum.atSObj;
      agTestCmds[11].ArgType[1] = ArgTypeEnum.atNum;
      agTestCmds[11].ArgType[2] = ArgTypeEnum.atNum;
      agTestCmds[11].ArgType[3] = ArgTypeEnum.atNum;
      agTestCmds[11].ArgType[4] = ArgTypeEnum.atNum;

      agTestCmds[12].Name = "controller";
      agTestCmds[12].ArgType = new ArgTypeEnum[1];
      agTestCmds[12].ArgType[0] = ArgTypeEnum.atCtrl;

      agTestCmds[13].Name = "have.key";
      agTestCmds[13].ArgType = Array.Empty<ArgTypeEnum>();

      agTestCmds[14].Name = "said";
      //special command so we don't need to set the right argument types for it
      agTestCmds[14].ArgType = Array.Empty<ArgTypeEnum>();

      agTestCmds[15].Name = "compare.strings";
      agTestCmds[15].ArgType = new ArgTypeEnum[2];
      agTestCmds[15].ArgType[0] = ArgTypeEnum.atStr;
      agTestCmds[15].ArgType[1] = ArgTypeEnum.atStr;

      agTestCmds[16].Name = "obj.in.box";
      agTestCmds[16].ArgType = new ArgTypeEnum[5];
      agTestCmds[16].ArgType[0] = ArgTypeEnum.atSObj;
      agTestCmds[16].ArgType[1] = ArgTypeEnum.atNum;
      agTestCmds[16].ArgType[2] = ArgTypeEnum.atNum;
      agTestCmds[16].ArgType[3] = ArgTypeEnum.atNum;
      agTestCmds[16].ArgType[4] = ArgTypeEnum.atNum;

      agTestCmds[17].Name = "center.posn";
      agTestCmds[17].ArgType = new ArgTypeEnum[5];
      agTestCmds[17].ArgType[0] = ArgTypeEnum.atSObj;
      agTestCmds[17].ArgType[1] = ArgTypeEnum.atNum;
      agTestCmds[17].ArgType[2] = ArgTypeEnum.atNum;
      agTestCmds[17].ArgType[3] = ArgTypeEnum.atNum;
      agTestCmds[17].ArgType[4] = ArgTypeEnum.atNum;

      agTestCmds[18].Name = "right.posn";
      agTestCmds[18].ArgType = new ArgTypeEnum[5];
      agTestCmds[18].ArgType[0] = ArgTypeEnum.atSObj;
      agTestCmds[18].ArgType[1] = ArgTypeEnum.atNum;
      agTestCmds[18].ArgType[2] = ArgTypeEnum.atNum;
      agTestCmds[18].ArgType[3] = ArgTypeEnum.atNum;
      agTestCmds[18].ArgType[4] = ArgTypeEnum.atNum;

      agTestCmds[19].Name = "unknowntest19";
      agTestCmds[19].ArgType = Array.Empty<ArgTypeEnum>();

      strErrSource = "WinAGI.agiTestCommands";
    }
    public static CommandStruct[] TestCommands
    {
      get { return agTestCmds; }
    }
    public static byte Count
    { get { return agNumTestCmds; } private set { } }
  }
}