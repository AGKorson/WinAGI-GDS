using System;
using System.IO;

namespace WinAGI
{
  public class AGICommands
  {
    // agiCommandInfo Class
    //
    //this class holds objects that contain information relating to
    //current game commands
    //test commands are in a separate object class
    //
    internal string[] agArgTypPref = new string[9]; //8
    internal string[] agArgTypName = new string[9]; //8

    internal byte agNumCmds;

    internal CommandStruct[] agCmds = new CommandStruct[181]; //182 

    //predefined arguments
    internal bool agResAsText;  //if true, reserved variables and flags show up as text when decompiling
                                //not used if agUseRes is FALSE
    internal bool agUseRes;     //if true, predefined variables and flags are used during compilation
    internal TDefine[] agResVar = new TDefine[27];    //26 //text name of built in variables
    internal TDefine[] agResFlag = new TDefine[18];   //17) //text name of built in flags
    internal TDefine[] agEdgeCodes = new TDefine[5]; //4 //text of edge codes
    internal TDefine[] agEgoDir = new TDefine[9];    //8 //text of ego direction codes
    internal TDefine[] agVideoMode = new TDefine[5]; //4 //text of video mode codes
    internal TDefine[] agCompType = new TDefine[9];  //8 //computer type values
    internal TDefine[] agResDef = new TDefine[6];    //5 //defines for ego, gamever, gameabout, gameid, invobj Count
    internal TDefine[] agResColor = new TDefine[16];  //15 //predefined color values

    //user defined global arguments
    internal TDefine[] agGlobal; //dynamic size
    internal int agGlobalCount;
    internal bool agGlobalsSet;
    internal uint agGlobalCRC;

    //warning count value stored in Common file, so it can be used by the IDE as well as the engine
    internal bool[] agNoCompWarn; // WARNCOUNT

    internal string strErrSource = "WinAGI.agiCommandInfo";


    public CommandStruct AGICommand(byte index)
    {
      ////validate index
      //if (Index > agNumCmds)
      //  On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
      //End If

      return agCmds[index];
    }

    public byte Count
    { get { return agNumCmds; } private set { } }


    /*
Option Compare Text
    */
    public AGICommands()
    {
      agNumCmds = 182;   // not counting return()

      //agi commands:
      agCmds[0].Name = "return";
      agCmds[0].ArgCount = 0;

      agCmds[1].Name = "increment";
      agCmds[1].ArgCount = 1;
      agCmds[1].ArgType[0] = ArgTypeEnum.atVar;

      agCmds[2].Name = "decrement";
      agCmds[2].ArgCount = 1;
      agCmds[2].ArgType[0] = ArgTypeEnum.atVar;

      agCmds[3].Name = "assignn";
      agCmds[3].ArgCount = 2;
      agCmds[3].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[3].ArgType[1] = ArgTypeEnum.atNum;

      agCmds[4].Name = "assignv";
      agCmds[4].ArgCount = 2;
      agCmds[4].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[4].ArgType[1] = ArgTypeEnum.atVar;

      agCmds[5].Name = "addn";
      agCmds[5].ArgCount = 2;
      agCmds[5].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[5].ArgType[1] = ArgTypeEnum.atNum;

      agCmds[6].Name = "addv";
      agCmds[6].ArgCount = 2;
      agCmds[6].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[6].ArgType[1] = ArgTypeEnum.atVar;

      agCmds[7].Name = "subn";
      agCmds[7].ArgCount = 2;
      agCmds[7].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[7].ArgType[1] = ArgTypeEnum.atNum;

      agCmds[8].Name = "subv";
      agCmds[8].ArgCount = 2;
      agCmds[8].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[8].ArgType[1] = ArgTypeEnum.atVar;

      agCmds[9].Name = "lindirectv";
      agCmds[9].ArgCount = 2;
      agCmds[9].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[9].ArgType[1] = ArgTypeEnum.atVar;

      agCmds[10].Name = "rindirect";
      agCmds[10].ArgCount = 2;
      agCmds[10].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[10].ArgType[1] = ArgTypeEnum.atVar;

      agCmds[11].Name = "lindirectn";
      agCmds[11].ArgCount = 2;
      agCmds[11].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[11].ArgType[1] = ArgTypeEnum.atNum;

      agCmds[12].Name = "set";
      agCmds[12].ArgCount = 1;
      agCmds[12].ArgType[0] = ArgTypeEnum.atFlag;

      agCmds[13].Name = "reset";
      agCmds[13].ArgCount = 1;
      agCmds[13].ArgType[0] = ArgTypeEnum.atFlag;

      agCmds[14].Name = "toggle";
      agCmds[14].ArgCount = 1;
      agCmds[14].ArgType[0] = ArgTypeEnum.atFlag; ;


      agCmds[15].Name = "set.v";
      agCmds[15].ArgCount = 1;
      agCmds[15].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[16].Name = "reset.v";
      agCmds[16].ArgCount = 1;
      agCmds[16].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[17].Name = "toggle.v";
      agCmds[17].ArgCount = 1;
      agCmds[17].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[18].Name = "new.room";
      agCmds[18].ArgCount = 1;
      agCmds[18].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[19].Name = "new.room.v";
      agCmds[19].ArgCount = 1;
      agCmds[19].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[20].Name = "load.logics";
      agCmds[20].ArgCount = 1;
      agCmds[20].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[21].Name = "load.logics.v";
      agCmds[21].ArgCount = 1;
      agCmds[21].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[22].Name = "call";
      agCmds[22].ArgCount = 1;
      agCmds[22].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[23].Name = "call.v";
      agCmds[23].ArgCount = 1;
      agCmds[23].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[24].Name = "load.pic";
      agCmds[24].ArgCount = 1;
      agCmds[24].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[25].Name = "draw.pic";
      agCmds[25].ArgCount = 1;
      agCmds[25].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[26].Name = "show.pic";
      agCmds[26].ArgCount = 0;



      agCmds[27].Name = "discard.pic";
      agCmds[27].ArgCount = 1;
      agCmds[27].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[28].Name = "overlay.pic";
      agCmds[28].ArgCount = 1;
      agCmds[28].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[29].Name = "show.pri.screen";
      agCmds[29].ArgCount = 0;



      agCmds[30].Name = "load.view";
      agCmds[30].ArgCount = 1;
      agCmds[30].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[31].Name = "load.view.v";
      agCmds[31].ArgCount = 1;
      agCmds[31].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[32].Name = "discard.view";
      agCmds[32].ArgCount = 1;
      agCmds[32].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[33].Name = "animate.obj";
      agCmds[33].ArgCount = 1;
      agCmds[33].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[34].Name = "unanimate.all";
      agCmds[34].ArgCount = 0;



      agCmds[35].Name = "draw";
      agCmds[35].ArgCount = 1;
      agCmds[35].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[36].Name = "erase";
      agCmds[36].ArgCount = 1;
      agCmds[36].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[37].Name = "position";
      agCmds[37].ArgCount = 3;
      agCmds[37].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[37].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[37].ArgType[2] = ArgTypeEnum.atNum;


      agCmds[38].Name = "position.v";
      agCmds[38].ArgCount = 3;
      agCmds[38].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[38].ArgType[1] = ArgTypeEnum.atVar;
      agCmds[38].ArgType[2] = ArgTypeEnum.atVar;


      agCmds[39].Name = "get.posn";
      agCmds[39].ArgCount = 3;
      agCmds[39].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[39].ArgType[1] = ArgTypeEnum.atVar;
      agCmds[39].ArgType[2] = ArgTypeEnum.atVar;


      agCmds[40].Name = "reposition";
      agCmds[40].ArgCount = 3;
      agCmds[40].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[40].ArgType[1] = ArgTypeEnum.atVar;
      agCmds[40].ArgType[2] = ArgTypeEnum.atVar;


      agCmds[41].Name = "set.view";
      agCmds[41].ArgCount = 2;
      agCmds[41].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[41].ArgType[1] = ArgTypeEnum.atNum;


      agCmds[42].Name = "set.view.v";
      agCmds[42].ArgCount = 2;
      agCmds[42].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[42].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[43].Name = "set.loop";
      agCmds[43].ArgCount = 2;
      agCmds[43].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[43].ArgType[1] = ArgTypeEnum.atNum;


      agCmds[44].Name = "set.loop.v";
      agCmds[44].ArgCount = 2;
      agCmds[44].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[44].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[45].Name = "fix.loop";
      agCmds[45].ArgCount = 1;
      agCmds[45].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[46].Name = "release.loop";
      agCmds[46].ArgCount = 1;
      agCmds[46].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[47].Name = "set.cel";
      agCmds[47].ArgCount = 2;
      agCmds[47].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[47].ArgType[1] = ArgTypeEnum.atNum;


      agCmds[48].Name = "set.cel.v";
      agCmds[48].ArgCount = 2;
      agCmds[48].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[48].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[49].Name = "last.cel";
      agCmds[49].ArgCount = 2;
      agCmds[49].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[49].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[50].Name = "current.cel";
      agCmds[50].ArgCount = 2;
      agCmds[50].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[50].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[51].Name = "current.loop";
      agCmds[51].ArgCount = 2;
      agCmds[51].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[51].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[52].Name = "current.view";
      agCmds[52].ArgCount = 2;
      agCmds[52].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[52].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[53].Name = "number.of.loops";
      agCmds[53].ArgCount = 2;
      agCmds[53].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[53].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[54].Name = "set.priority";
      agCmds[54].ArgCount = 2;
      agCmds[54].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[54].ArgType[1] = ArgTypeEnum.atNum;


      agCmds[55].Name = "set.priority.v";
      agCmds[55].ArgCount = 2;
      agCmds[55].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[55].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[56].Name = "release.priority";
      agCmds[56].ArgCount = 1;
      agCmds[56].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[57].Name = "get.priority";
      agCmds[57].ArgCount = 2;
      agCmds[57].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[57].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[58].Name = "stop.update";
      agCmds[58].ArgCount = 1;
      agCmds[58].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[59].Name = "start.update";
      agCmds[59].ArgCount = 1;
      agCmds[59].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[60].Name = "force.update";
      agCmds[60].ArgCount = 1;
      agCmds[60].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[61].Name = "ignore.horizon";
      agCmds[61].ArgCount = 1;
      agCmds[61].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[62].Name = "observe.horizon";
      agCmds[62].ArgCount = 1;
      agCmds[62].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[63].Name = "set.horizon";
      agCmds[63].ArgCount = 1;
      agCmds[63].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[64].Name = "object.on.water";
      agCmds[64].ArgCount = 1;
      agCmds[64].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[65].Name = "object.on.land";
      agCmds[65].ArgCount = 1;
      agCmds[65].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[66].Name = "object.on.anything";
      agCmds[66].ArgCount = 1;
      agCmds[66].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[67].Name = "ignore.objs";
      agCmds[67].ArgCount = 1;
      agCmds[67].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[68].Name = "observe.objs";
      agCmds[68].ArgCount = 1;
      agCmds[68].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[69].Name = "distance";
      agCmds[69].ArgCount = 3;
      agCmds[69].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[69].ArgType[1] = ArgTypeEnum.atSObj;
      agCmds[69].ArgType[2] = ArgTypeEnum.atVar;


      agCmds[70].Name = "stop.cycling";
      agCmds[70].ArgCount = 1;
      agCmds[70].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[71].Name = "start.cycling";
      agCmds[71].ArgCount = 1;
      agCmds[71].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[72].Name = "normal.cycle";
      agCmds[72].ArgCount = 1;
      agCmds[72].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[73].Name = "end.of.loop";
      agCmds[73].ArgCount = 2;
      agCmds[73].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[73].ArgType[1] = ArgTypeEnum.atFlag;


      agCmds[74].Name = "reverse.cycle";
      agCmds[74].ArgCount = 1;
      agCmds[74].ArgType[0] = ArgTypeEnum.atSObj; ;


      agCmds[75].Name = "reverse.loop";
      agCmds[75].ArgCount = 2;
      agCmds[75].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[75].ArgType[1] = ArgTypeEnum.atFlag;


      agCmds[76].Name = "cycle.time";
      agCmds[76].ArgCount = 2;
      agCmds[76].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[76].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[77].Name = "stop.motion";
      agCmds[77].ArgCount = 1;
      agCmds[77].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[78].Name = "start.motion";
      agCmds[78].ArgCount = 1;
      agCmds[78].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[79].Name = "step.size";
      agCmds[79].ArgCount = 2;
      agCmds[79].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[79].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[80].Name = "step.time";
      agCmds[80].ArgCount = 2;
      agCmds[80].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[80].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[81].Name = "move.obj";
      agCmds[81].ArgCount = 5;
      agCmds[81].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[81].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[81].ArgType[2] = ArgTypeEnum.atNum;
      agCmds[81].ArgType[3] = ArgTypeEnum.atNum;
      agCmds[81].ArgType[4] = ArgTypeEnum.atFlag;


      agCmds[82].Name = "move.obj.v";
      agCmds[82].ArgCount = 5;
      agCmds[82].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[82].ArgType[1] = ArgTypeEnum.atVar;
      agCmds[82].ArgType[2] = ArgTypeEnum.atVar;
      agCmds[82].ArgType[3] = ArgTypeEnum.atVar;
      agCmds[82].ArgType[4] = ArgTypeEnum.atFlag;


      agCmds[83].Name = "follow.ego";
      agCmds[83].ArgCount = 3;
      agCmds[83].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[83].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[83].ArgType[2] = ArgTypeEnum.atFlag;


      agCmds[84].Name = "wander";
      agCmds[84].ArgCount = 1;
      agCmds[84].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[85].Name = "normal.motion";
      agCmds[85].ArgCount = 1;
      agCmds[85].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[86].Name = "set.dir";
      agCmds[86].ArgCount = 2;
      agCmds[86].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[86].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[87].Name = "get.dir";
      agCmds[87].ArgCount = 2;
      agCmds[87].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[87].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[88].Name = "ignore.blocks";
      agCmds[88].ArgCount = 1;
      agCmds[88].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[89].Name = "observe.blocks";
      agCmds[89].ArgCount = 1;
      agCmds[89].ArgType[0] = ArgTypeEnum.atSObj;


      agCmds[90].Name = "block";
      agCmds[90].ArgCount = 4;
      agCmds[90].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[90].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[90].ArgType[2] = ArgTypeEnum.atNum;
      agCmds[90].ArgType[3] = ArgTypeEnum.atNum;


      agCmds[91].Name = "unblock";
      agCmds[91].ArgCount = 0;


      agCmds[92].Name = "get";
      agCmds[92].ArgCount = 1;
      agCmds[92].ArgType[0] = ArgTypeEnum.atIObj;


      agCmds[93].Name = "get.v";
      agCmds[93].ArgCount = 1;
      agCmds[93].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[94].Name = "drop";
      agCmds[94].ArgCount = 1;
      agCmds[94].ArgType[0] = ArgTypeEnum.atIObj;


      agCmds[95].Name = "put";
      agCmds[95].ArgCount = 2;
      agCmds[95].ArgType[0] = ArgTypeEnum.atIObj;
      agCmds[95].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[96].Name = "put.v";
      agCmds[96].ArgCount = 2;
      agCmds[96].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[96].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[97].Name = "get.room.v";
      agCmds[97].ArgCount = 2;
      agCmds[97].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[97].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[98].Name = "load.sound";
      agCmds[98].ArgCount = 1;
      agCmds[98].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[99].Name = "sound";
      agCmds[99].ArgCount = 2;
      agCmds[99].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[99].ArgType[1] = ArgTypeEnum.atFlag;


      agCmds[100].Name = "stop.sound";
      agCmds[100].ArgCount = 0;


      agCmds[101].Name = "print";
      agCmds[101].ArgCount = 1;
      agCmds[101].ArgType[0] = ArgTypeEnum.atMsg;



      agCmds[102].Name = "print.v";
      agCmds[102].ArgCount = 1;
      agCmds[102].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[103].Name = "display";
      agCmds[103].ArgCount = 3;
      agCmds[103].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[103].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[103].ArgType[2] = ArgTypeEnum.atMsg;


      agCmds[104].Name = "display.v";
      agCmds[104].ArgCount = 3;
      agCmds[104].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[104].ArgType[1] = ArgTypeEnum.atVar;
      agCmds[104].ArgType[2] = ArgTypeEnum.atVar;


      agCmds[105].Name = "clear.lines";
      agCmds[105].ArgCount = 3;
      agCmds[105].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[105].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[105].ArgType[2] = ArgTypeEnum.atNum;


      agCmds[106].Name = "text.screen";
      agCmds[106].ArgCount = 0;


      agCmds[107].Name = "graphics";
      agCmds[107].ArgCount = 0;


      agCmds[108].Name = "set.cursor.char";
      agCmds[108].ArgCount = 1;
      agCmds[108].ArgType[0] = ArgTypeEnum.atMsg;


      agCmds[109].Name = "set.text.attribute";
      agCmds[109].ArgCount = 2;
      agCmds[109].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[109].ArgType[1] = ArgTypeEnum.atNum;


      agCmds[110].Name = "shake.screen";
      agCmds[110].ArgCount = 1;
      agCmds[110].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[111].Name = "configure.screen";
      agCmds[111].ArgCount = 3;
      agCmds[111].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[111].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[111].ArgType[2] = ArgTypeEnum.atNum;


      agCmds[112].Name = "status.line.on";
      agCmds[112].ArgCount = 0;


      agCmds[113].Name = "status.line.off";
      agCmds[113].ArgCount = 0;


      agCmds[114].Name = "set.string";
      agCmds[114].ArgCount = 2;
      agCmds[114].ArgType[0] = ArgTypeEnum.atStr;
      agCmds[114].ArgType[1] = ArgTypeEnum.atMsg;


      agCmds[115].Name = "get.string";
      agCmds[115].ArgCount = 5;
      agCmds[115].ArgType[0] = ArgTypeEnum.atStr;
      agCmds[115].ArgType[1] = ArgTypeEnum.atMsg;
      agCmds[115].ArgType[2] = ArgTypeEnum.atNum;
      agCmds[115].ArgType[3] = ArgTypeEnum.atNum;
      agCmds[115].ArgType[4] = ArgTypeEnum.atNum;


      agCmds[116].Name = "word.to.string";
      agCmds[116].ArgCount = 2;
      //  the word arguments are zero-based [i.e.
      //  first word is w0, second word is w1, etc
      //  but when using format codes, the codes are 1-based
      //  i.e. %w1 is first word, %w2 is second word, etc
      //  to eliminate confusion, compiler automatically compensates
      agCmds[116].ArgType[0] = ArgTypeEnum.atStr;
      agCmds[116].ArgType[1] = ArgTypeEnum.atWord;


      agCmds[117].Name = "parse";
      agCmds[117].ArgCount = 1;
      agCmds[117].ArgType[0] = ArgTypeEnum.atStr;


      agCmds[118].Name = "get.num";
      agCmds[118].ArgCount = 2;
      agCmds[118].ArgType[0] = ArgTypeEnum.atMsg;
      agCmds[118].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[119].Name = "prevent.input";
      agCmds[119].ArgCount = 0;


      agCmds[120].Name = "accept.input";
      agCmds[120].ArgCount = 0;


      agCmds[121].Name = "set.key";
      agCmds[121].ArgCount = 3;
      agCmds[121].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[121].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[121].ArgType[2] = ArgTypeEnum.atCtrl;



      agCmds[122].Name = "add.to.pic";
      agCmds[122].ArgCount = 7;
      agCmds[122].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[122].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[122].ArgType[2] = ArgTypeEnum.atNum;
      agCmds[122].ArgType[3] = ArgTypeEnum.atNum;
      agCmds[122].ArgType[4] = ArgTypeEnum.atNum;
      agCmds[122].ArgType[5] = ArgTypeEnum.atNum;
      agCmds[122].ArgType[6] = ArgTypeEnum.atNum;


      agCmds[123].Name = "add.to.pic.v";
      agCmds[123].ArgCount = 7;
      agCmds[123].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[123].ArgType[1] = ArgTypeEnum.atVar;
      agCmds[123].ArgType[2] = ArgTypeEnum.atVar;
      agCmds[123].ArgType[3] = ArgTypeEnum.atVar;
      agCmds[123].ArgType[4] = ArgTypeEnum.atVar;
      agCmds[123].ArgType[5] = ArgTypeEnum.atVar;
      agCmds[123].ArgType[6] = ArgTypeEnum.atVar;


      agCmds[124].Name = "status";
      agCmds[124].ArgCount = 0;


      agCmds[125].Name = "save.game";
      agCmds[125].ArgCount = 0;


      agCmds[126].Name = "restore.game";
      agCmds[126].ArgCount = 0;


      agCmds[127].Name = "init.disk";
      agCmds[127].ArgCount = 0;


      agCmds[128].Name = "restart.game";
      agCmds[128].ArgCount = 0;


      agCmds[129].Name = "show.obj";
      agCmds[129].ArgCount = 1;
      agCmds[129].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[130].Name = "random";
      agCmds[130].ArgCount = 3;
      agCmds[130].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[130].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[130].ArgType[2] = ArgTypeEnum.atVar;


      agCmds[131].Name = "program.control";
      agCmds[131].ArgCount = 0;


      agCmds[132].Name = "player.control";
      agCmds[132].ArgCount = 0;


      agCmds[133].Name = "obj.status.v";
      agCmds[133].ArgCount = 1;
      agCmds[133].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[134].Name = "quit";
      agCmds[134].ArgCount = 1;
      agCmds[134].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[135].Name = "show.mem";
      agCmds[135].ArgCount = 0;


      agCmds[136].Name = "pause";
      agCmds[136].ArgCount = 0;


      agCmds[137].Name = "echo.line";
      agCmds[137].ArgCount = 0;


      agCmds[138].Name = "cancel.line";
      agCmds[138].ArgCount = 0;


      agCmds[139].Name = "init.joy";
      agCmds[139].ArgCount = 0;


      agCmds[140].Name = "toggle.monitor";
      agCmds[140].ArgCount = 0;


      agCmds[141].Name = "version";
      agCmds[141].ArgCount = 0;


      agCmds[142].Name = "script.size";
      agCmds[142].ArgCount = 1;
      agCmds[142].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[143].Name = "set.game.id";
      agCmds[143].ArgCount = 1;
      agCmds[143].ArgType[0] = ArgTypeEnum.atMsg;


      agCmds[144].Name = "log";
      agCmds[144].ArgCount = 1;
      agCmds[144].ArgType[0] = ArgTypeEnum.atMsg;


      agCmds[145].Name = "set.scan.start";
      agCmds[145].ArgCount = 0;


      agCmds[146].Name = "reset.scan.start";
      agCmds[146].ArgCount = 0;


      agCmds[147].Name = "reposition.to";
      agCmds[147].ArgCount = 3;
      agCmds[147].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[147].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[147].ArgType[2] = ArgTypeEnum.atNum;


      agCmds[148].Name = "reposition.to.v";
      agCmds[148].ArgCount = 3;
      agCmds[148].ArgType[0] = ArgTypeEnum.atSObj;
      agCmds[148].ArgType[1] = ArgTypeEnum.atVar;
      agCmds[148].ArgType[2] = ArgTypeEnum.atVar;


      agCmds[149].Name = "trace.on";
      agCmds[149].ArgCount = 0;


      agCmds[150].Name = "trace.info";
      agCmds[150].ArgCount = 3;
      agCmds[150].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[150].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[150].ArgType[2] = ArgTypeEnum.atNum;


      agCmds[151].Name = "print.at";
      agCmds[151].ArgCount = 4;
      agCmds[151].ArgType[0] = ArgTypeEnum.atMsg;
      agCmds[151].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[151].ArgType[2] = ArgTypeEnum.atNum;
      agCmds[151].ArgType[3] = ArgTypeEnum.atNum;


      agCmds[152].Name = "print.at.v";
      agCmds[152].ArgCount = 4;
      agCmds[152].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[152].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[152].ArgType[2] = ArgTypeEnum.atNum;
      agCmds[152].ArgType[3] = ArgTypeEnum.atNum;


      agCmds[153].Name = "discard.view.v";
      agCmds[153].ArgCount = 1;
      agCmds[153].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[154].Name = "clear.text.rect";
      agCmds[154].ArgCount = 5;
      agCmds[154].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[154].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[154].ArgType[2] = ArgTypeEnum.atNum;
      agCmds[154].ArgType[3] = ArgTypeEnum.atNum;
      agCmds[154].ArgType[4] = ArgTypeEnum.atNum;


      agCmds[155].Name = "set.upper.left";
      agCmds[155].ArgCount = 2;
      agCmds[155].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[155].ArgType[1] = ArgTypeEnum.atNum;


      agCmds[156].Name = "set.menu";
      agCmds[156].ArgCount = 1;
      agCmds[156].ArgType[0] = ArgTypeEnum.atMsg;


      agCmds[157].Name = "set.menu.item";
      agCmds[157].ArgCount = 2;
      agCmds[157].ArgType[0] = ArgTypeEnum.atMsg;
      agCmds[157].ArgType[1] = ArgTypeEnum.atCtrl;


      agCmds[158].Name = "submit.menu";
      agCmds[158].ArgCount = 0;


      agCmds[159].Name = "enable.item";
      agCmds[159].ArgCount = 1;
      agCmds[159].ArgType[0] = ArgTypeEnum.atCtrl;


      agCmds[160].Name = "disable.item";
      agCmds[160].ArgCount = 1;
      agCmds[160].ArgType[0] = ArgTypeEnum.atCtrl;


      agCmds[161].Name = "menu.input";
      agCmds[161].ArgCount = 0;


      agCmds[162].Name = "show.obj.v";
      agCmds[162].ArgCount = 1;
      agCmds[162].ArgType[0] = ArgTypeEnum.atVar;


      agCmds[163].Name = "open.dialogue";
      agCmds[163].ArgCount = 0;


      agCmds[164].Name = "close.dialogue";
      agCmds[164].ArgCount = 0;


      agCmds[165].Name = "mul.n";
      agCmds[165].ArgCount = 2;
      agCmds[165].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[165].ArgType[1] = ArgTypeEnum.atNum;


      agCmds[166].Name = "mul.v";
      agCmds[166].ArgCount = 2;
      agCmds[166].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[166].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[167].Name = "div.n";
      agCmds[167].ArgCount = 2;
      agCmds[167].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[167].ArgType[1] = ArgTypeEnum.atNum;


      agCmds[168].Name = "div.v";
      agCmds[168].ArgCount = 2;
      agCmds[168].ArgType[0] = ArgTypeEnum.atVar;
      agCmds[168].ArgType[1] = ArgTypeEnum.atVar;


      agCmds[169].Name = "close.window";
      agCmds[169].ArgCount = 0;


      agCmds[170].Name = "set.simple";
      agCmds[170].ArgCount = 1;
      agCmds[170].ArgType[0] = ArgTypeEnum.atStr;


      agCmds[171].Name = "push.script";
      agCmds[171].ArgCount = 0;


      agCmds[172].Name = "pop.script";
      agCmds[172].ArgCount = 0;


      agCmds[173].Name = "hold.key";
      agCmds[173].ArgCount = 0;


      agCmds[174].Name = "set.pri.base";
      agCmds[174].ArgCount = 1;
      agCmds[174].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[175].Name = "discard.sound";
      agCmds[175].ArgCount = 1;
      agCmds[175].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[176].Name = "hide.mouse";
      //////  agCmds[176].ArgCount = 0
      //i think arg count  should always be 1
      agCmds[176].ArgCount = 1;
      agCmds[176].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[177].Name = "allow.menu";
      agCmds[177].ArgCount = 1;
      agCmds[177].ArgType[0] = ArgTypeEnum.atNum;


      agCmds[178].Name = "show.mouse";
      //////  agCmds[178].ArgCount = 0
      //i think ArgCount should be 1
      agCmds[178].ArgCount = 1;


      agCmds[179].Name = "fence.mouse";
      agCmds[179].ArgCount = 4;
      agCmds[179].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[179].ArgType[1] = ArgTypeEnum.atNum;
      agCmds[179].ArgType[2] = ArgTypeEnum.atNum;
      agCmds[179].ArgType[3] = ArgTypeEnum.atNum;


      agCmds[180].Name = "mouse.posn";
      agCmds[180].ArgCount = 2;
      agCmds[180].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[180].ArgType[1] = ArgTypeEnum.atNum;


      agCmds[181].Name = "release.key";
      agCmds[181].ArgCount = 0;


      agCmds[182].Name = "adj.ego.move.to.x.y";
      agCmds[182].ArgCount = 2;
      agCmds[182].ArgType[0] = ArgTypeEnum.atNum;
      agCmds[182].ArgType[1] = ArgTypeEnum.atNum;
    }



    internal void AssignReservedDefines()
    // predefined variables, flags, and objects
    // Variables v0 - v26
    // Flags f0 - f16, f20 [in version 3.102 and above]

    //create default variables, flags and constants
    //NOTE: object variable, o0, is considered a predefined
    //variable, as well as game version string, game about string
    //and inventory object Count
    {
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

    internal void CorrectCommands(string Version)
    // This procedure adjusts the logic commands for a given int. version
    {
      double verNum;

      if (!double.TryParse(Version, out verNum))
      {
        //error
        throw new NotImplementedException();
        return;
      }

      //now adjust for version
      if (verNum <= 2.089)
        agCmds[134].ArgCount = 0;  // quit command
      else
        agCmds[134].ArgCount = 1;

      if (verNum <= 2.4)
      {
        agCmds[151].ArgCount = 3; // print.at
        agCmds[152].ArgCount = 3; // print.at.v
      }
      else
      {
        agCmds[151].ArgCount = 4; // print.at
        agCmds[152].ArgCount = 4; // print.at.v
      }

      if (verNum <= 2.089)
        agNumCmds = 155;
      else if (verNum <= 2.272)
        agNumCmds = 161;
      else if (verNum <= 2.44)
        agNumCmds = 169;
      else if (verNum <= 2.917)
        agNumCmds = 173;
      else if (verNum <= 2.936)
        agNumCmds = 175;
      else if (verNum <= 3.002086)
        agNumCmds = 177;
      else
        agNumCmds = 181;
    }

    internal void GetGlobalDefines()
    {
      string strLine, strTmp;
      string[] strSplitLine;
      int i, lngTest;
      TDefine tdNewDefine = new TDefine();
      DateTime dtFileMod;

      agGlobalsSet = false;

      //clear global list
      agGlobal = new TDefine[0];
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


          //strip off comment
          strLine = AGIGame.StripComments(strLine, "", false);
          //ignore blanks '''and comments(// or ' or Rem)
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
              lngTest = AGIGame.ValidateDefName(tdNewDefine.Name);

              if (lngTest == 0 || (lngTest >= 8 && lngTest <= 12))
              {
                //Select Case lngTest
                //Case 0, 8, 9, 10, 11, 12 'name is valid or is overriding a reserved define
                lngTest = AGIGame.ValidateDefName(tdNewDefine.Name);
                lngTest = AGIGame.ValidateDefValue(tdNewDefine);
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
        agGlobalCRC = AGIGame.CRC32(dtFileMod.ToString().ToCharArray());
        //ErrHandler:
        //        '*'Debug.Assert False

        //'ensure file is closed
        //Close intFile
        //'return false
      }
    }
  }
}