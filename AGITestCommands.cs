using System;

namespace WinAGI
{
  static public class AGITestCommands
  {
    static internal CommandStruct[] agTestCmds = new CommandStruct[20]; //19  //PC version has 18 test commands; Amiga appears to have 19
    static public string strErrSource;

    static internal byte agNumTestCmds;
    static AGITestCommands()
    {
      agNumTestCmds = 19; //not counting return.false()

      agTestCmds[0].Name = "return.false";
      agTestCmds[0].ArgCount = 0;

      agTestCmds[1].Name = "equaln";
      agTestCmds[1].ArgCount = 2;
      agTestCmds[1].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[1].ArgType[1] = ArgTypeEnum.atNum;



      agTestCmds[2].Name = "equalv";
      agTestCmds[2].ArgCount = 2;
      agTestCmds[2].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[2].ArgType[1] = ArgTypeEnum.atVar;

      agTestCmds[3].Name = "lessn";
      agTestCmds[3].ArgCount = 2;
      agTestCmds[3].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[3].ArgType[1] = ArgTypeEnum.atNum;

      agTestCmds[4].Name = "lessv";
      agTestCmds[4].ArgCount = 2;
      agTestCmds[4].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[4].ArgType[1] = ArgTypeEnum.atVar;

      agTestCmds[5].Name = "greatern";
      agTestCmds[5].ArgCount = 2;
      agTestCmds[5].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[5].ArgType[1] = ArgTypeEnum.atNum;

      agTestCmds[6].Name = "greaterv";
      agTestCmds[6].ArgCount = 2;
      agTestCmds[6].ArgType[0] = ArgTypeEnum.atVar;
      agTestCmds[6].ArgType[1] = ArgTypeEnum.atVar;

      agTestCmds[7].Name = "isset";
      agTestCmds[7].ArgCount = 1;
      agTestCmds[7].ArgType[0] = ArgTypeEnum.atFlag;

      agTestCmds[8].Name = "issetv";
      agTestCmds[8].ArgCount = 1;
      agTestCmds[8].ArgType[0] = ArgTypeEnum.atVar;

      agTestCmds[9].Name = "has";
      agTestCmds[9].ArgCount = 1;
      agTestCmds[9].ArgType[0] = ArgTypeEnum.atIObj;

      agTestCmds[10].Name = "obj.in.room";
      agTestCmds[10].ArgCount = 2;
      agTestCmds[10].ArgType[0] = ArgTypeEnum.atIObj;
      agTestCmds[10].ArgType[1] = ArgTypeEnum.atVar;

      agTestCmds[11].Name = "posn";
      agTestCmds[11].ArgCount = 5;
      agTestCmds[11].ArgType[0] = ArgTypeEnum.atSObj;
      agTestCmds[11].ArgType[1] = ArgTypeEnum.atNum;
      agTestCmds[11].ArgType[2] = ArgTypeEnum.atNum;
      agTestCmds[11].ArgType[3] = ArgTypeEnum.atNum;
      agTestCmds[11].ArgType[4] = ArgTypeEnum.atNum;

      agTestCmds[12].Name = "controller";
      agTestCmds[12].ArgCount = 1;
      agTestCmds[12].ArgType[0] = ArgTypeEnum.atCtrl;

      agTestCmds[13].Name = "have.key";
      agTestCmds[13].ArgCount = 0;

      agTestCmds[14].Name = "said";
      agTestCmds[14].ArgCount = 0; //special command so we don't need to set the right argument types for it

      agTestCmds[15].Name = "compare.strings";
      agTestCmds[15].ArgCount = 2;
      agTestCmds[15].ArgType[0] = ArgTypeEnum.atStr;
      agTestCmds[15].ArgType[1] = ArgTypeEnum.atStr;

      agTestCmds[16].Name = "obj.in.box";
      agTestCmds[16].ArgCount = 5;
      agTestCmds[16].ArgType[0] = ArgTypeEnum.atSObj;
      agTestCmds[16].ArgType[1] = ArgTypeEnum.atNum;
      agTestCmds[16].ArgType[2] = ArgTypeEnum.atNum;
      agTestCmds[16].ArgType[3] = ArgTypeEnum.atNum;
      agTestCmds[16].ArgType[4] = ArgTypeEnum.atNum;

      agTestCmds[17].Name = "center.posn";
      agTestCmds[17].ArgCount = 5;
      agTestCmds[17].ArgType[0] = ArgTypeEnum.atSObj;
      agTestCmds[17].ArgType[1] = ArgTypeEnum.atNum;
      agTestCmds[17].ArgType[2] = ArgTypeEnum.atNum;
      agTestCmds[17].ArgType[3] = ArgTypeEnum.atNum;
      agTestCmds[17].ArgType[4] = ArgTypeEnum.atNum;

      agTestCmds[18].Name = "right.posn";
      agTestCmds[18].ArgCount = 5;
      agTestCmds[18].ArgType[0] = ArgTypeEnum.atSObj;
      agTestCmds[18].ArgType[1] = ArgTypeEnum.atNum;
      agTestCmds[18].ArgType[2] = ArgTypeEnum.atNum;
      agTestCmds[18].ArgType[3] = ArgTypeEnum.atNum;
      agTestCmds[18].ArgType[4] = ArgTypeEnum.atNum;

      agTestCmds[19].Name = "unknowntest19";
      agTestCmds[19].ArgCount = 0;
      strErrSource = "WinAGI.agiTestCommands";
    }

    static public CommandStruct TestCmd(byte index)
    {
      //validate index
      if (index > agNumTestCmds)
      {
        throw new NotImplementedException();
      }

      return agTestCmds[index];
    }

    static public byte Count
    { get { return agNumTestCmds; } private set { } }
  }
}