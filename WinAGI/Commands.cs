using System;
using System.Linq;
using static WinAGI.Engine.ArgTypeEnum;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class to hold all members related to AGI's action and test commands.
    /// </summary>
    public static class Commands {
        #region Local Members
        internal static readonly string[] agArgTypPref = ["", "v", "f", "m", "o", "i", "s", "w", "c"];
        internal static readonly string[] agArgTypName =
            [
            "number", "variable", "flag", "message", "object",
            "inventory item", "string", "word", "controller"
            ];
        internal const int MAX_CMDS = 182;
        internal static byte agNumCmds;
        internal static byte agNumTestCmds;
        internal static CommandStruct[] agCmds = new CommandStruct[MAX_CMDS];
        internal static CommandStruct[] agTestCmds = new CommandStruct[20];

        // last command 'adjust.ego.x.y', is not supported so it's not added
        // to the list of commands
        #endregion

        #region Constructors
        /// <summary>
        /// Creates and initializes the Commands object.
        /// </summary>
        static Commands() {
            // default is to make all 182 commands visible
            agNumCmds = MAX_CMDS;

            // agi commands:
            agCmds[0].Name = "return";
            agCmds[0].ArgType = [];

            agCmds[1].Name = "increment";
            agCmds[1].ArgType = new ArgTypeEnum[1];
            agCmds[1].ArgType[0] = atVar;

            agCmds[2].Name = "decrement";
            agCmds[2].ArgType = new ArgTypeEnum[1];
            agCmds[2].ArgType[0] = atVar;

            agCmds[3].Name = "assignn";
            agCmds[3].ArgType = new ArgTypeEnum[2];
            agCmds[3].ArgType[0] = atVar;
            agCmds[3].ArgType[1] = atNum;

            agCmds[4].Name = "assignv";
            agCmds[4].ArgType = new ArgTypeEnum[2];
            agCmds[4].ArgType[0] = atVar;
            agCmds[4].ArgType[1] = atVar;

            agCmds[5].Name = "addn";
            agCmds[5].ArgType = new ArgTypeEnum[2];
            agCmds[5].ArgType[0] = atVar;
            agCmds[5].ArgType[1] = atNum;

            agCmds[6].Name = "addv";
            agCmds[6].ArgType = new ArgTypeEnum[2];
            agCmds[6].ArgType[0] = atVar;
            agCmds[6].ArgType[1] = atVar;

            agCmds[7].Name = "subn";
            agCmds[7].ArgType = new ArgTypeEnum[2];
            agCmds[7].ArgType[0] = atVar;
            agCmds[7].ArgType[1] = atNum;

            agCmds[8].Name = "subv";
            agCmds[8].ArgType = new ArgTypeEnum[2];
            agCmds[8].ArgType[0] = atVar;
            agCmds[8].ArgType[1] = atVar;

            agCmds[9].Name = "lindirectv";
            agCmds[9].ArgType = new ArgTypeEnum[2];
            agCmds[9].ArgType[0] = atVar;
            agCmds[9].ArgType[1] = atVar;

            agCmds[10].Name = "rindirect";
            agCmds[10].ArgType = new ArgTypeEnum[2];
            agCmds[10].ArgType[0] = atVar;
            agCmds[10].ArgType[1] = atVar;

            agCmds[11].Name = "lindirectn";
            agCmds[11].ArgType = new ArgTypeEnum[2];
            agCmds[11].ArgType[0] = atVar;
            agCmds[11].ArgType[1] = atNum;

            agCmds[12].Name = "set";
            agCmds[12].ArgType = new ArgTypeEnum[1];
            agCmds[12].ArgType[0] = atFlag;

            agCmds[13].Name = "reset";
            agCmds[13].ArgType = new ArgTypeEnum[1];
            agCmds[13].ArgType[0] = atFlag;

            agCmds[14].Name = "toggle";
            agCmds[14].ArgType = new ArgTypeEnum[1];
            agCmds[14].ArgType[0] = atFlag; ;

            agCmds[15].Name = "set.v";
            agCmds[15].ArgType = new ArgTypeEnum[1];
            agCmds[15].ArgType[0] = atVar;

            agCmds[16].Name = "reset.v";
            agCmds[16].ArgType = new ArgTypeEnum[1];
            agCmds[16].ArgType[0] = atVar;

            agCmds[17].Name = "toggle.v";
            agCmds[17].ArgType = new ArgTypeEnum[1];
            agCmds[17].ArgType[0] = atVar;

            agCmds[18].Name = "new.room";
            agCmds[18].ArgType = new ArgTypeEnum[1];
            agCmds[18].ArgType[0] = atNum;

            agCmds[19].Name = "new.room.v";
            agCmds[19].ArgType = new ArgTypeEnum[1];
            agCmds[19].ArgType[0] = atVar;

            agCmds[20].Name = "load.logics";
            agCmds[20].ArgType = new ArgTypeEnum[1];
            agCmds[20].ArgType[0] = atNum;

            agCmds[21].Name = "load.logics.v";
            agCmds[21].ArgType = new ArgTypeEnum[1];
            agCmds[21].ArgType[0] = atVar;

            agCmds[22].Name = "call";
            agCmds[22].ArgType = new ArgTypeEnum[1];
            agCmds[22].ArgType[0] = atNum;

            agCmds[23].Name = "call.v";
            agCmds[23].ArgType = new ArgTypeEnum[1];
            agCmds[23].ArgType[0] = atVar;

            agCmds[24].Name = "load.pic";
            agCmds[24].ArgType = new ArgTypeEnum[1];
            agCmds[24].ArgType[0] = atVar;

            agCmds[25].Name = "draw.pic";
            agCmds[25].ArgType = new ArgTypeEnum[1];
            agCmds[25].ArgType[0] = atVar;

            agCmds[26].Name = "show.pic";
            agCmds[26].ArgType = [];

            agCmds[27].Name = "discard.pic";
            agCmds[27].ArgType = new ArgTypeEnum[1];
            agCmds[27].ArgType[0] = atVar;

            agCmds[28].Name = "overlay.pic";
            agCmds[28].ArgType = new ArgTypeEnum[1];
            agCmds[28].ArgType[0] = atVar;

            agCmds[29].Name = "show.pri.screen";
            agCmds[29].ArgType = [];

            agCmds[30].Name = "load.view";
            agCmds[30].ArgType = new ArgTypeEnum[1];
            agCmds[30].ArgType[0] = atNum;

            agCmds[31].Name = "load.view.v";
            agCmds[31].ArgType = new ArgTypeEnum[1];
            agCmds[31].ArgType[0] = atVar;

            agCmds[32].Name = "discard.view";
            agCmds[32].ArgType = new ArgTypeEnum[1];
            agCmds[32].ArgType[0] = atNum;

            agCmds[33].Name = "animate.obj";
            agCmds[33].ArgType = new ArgTypeEnum[1];
            agCmds[33].ArgType[0] = atSObj;

            agCmds[34].Name = "unanimate.all";
            agCmds[34].ArgType = [];

            agCmds[35].Name = "draw";
            agCmds[35].ArgType = new ArgTypeEnum[1];
            agCmds[35].ArgType[0] = atSObj;

            agCmds[36].Name = "erase";
            agCmds[36].ArgType = new ArgTypeEnum[1];
            agCmds[36].ArgType[0] = atSObj;

            agCmds[37].Name = "position";
            agCmds[37].ArgType = new ArgTypeEnum[3];
            agCmds[37].ArgType[0] = atSObj;
            agCmds[37].ArgType[1] = atNum;
            agCmds[37].ArgType[2] = atNum;

            agCmds[38].Name = "position.v";
            agCmds[38].ArgType = new ArgTypeEnum[3];
            agCmds[38].ArgType[0] = atSObj;
            agCmds[38].ArgType[1] = atVar;
            agCmds[38].ArgType[2] = atVar;

            agCmds[39].Name = "get.posn";
            agCmds[39].ArgType = new ArgTypeEnum[3];
            agCmds[39].ArgType[0] = atSObj;
            agCmds[39].ArgType[1] = atVar;
            agCmds[39].ArgType[2] = atVar;

            agCmds[40].Name = "reposition";
            agCmds[40].ArgType = new ArgTypeEnum[3];
            agCmds[40].ArgType[0] = atSObj;
            agCmds[40].ArgType[1] = atVar;
            agCmds[40].ArgType[2] = atVar;

            agCmds[41].Name = "set.view";
            agCmds[41].ArgType = new ArgTypeEnum[2];
            agCmds[41].ArgType[0] = atSObj;
            agCmds[41].ArgType[1] = atNum;

            agCmds[42].Name = "set.view.v";
            agCmds[42].ArgType = new ArgTypeEnum[2];
            agCmds[42].ArgType[0] = atSObj;
            agCmds[42].ArgType[1] = atVar;

            agCmds[43].Name = "set.loop";
            agCmds[43].ArgType = new ArgTypeEnum[2];
            agCmds[43].ArgType[0] = atSObj;
            agCmds[43].ArgType[1] = atNum;

            agCmds[44].Name = "set.loop.v";
            agCmds[44].ArgType = new ArgTypeEnum[2];
            agCmds[44].ArgType[0] = atSObj;
            agCmds[44].ArgType[1] = atVar;

            agCmds[45].Name = "fix.loop";
            agCmds[45].ArgType = new ArgTypeEnum[1];
            agCmds[45].ArgType[0] = atSObj;

            agCmds[46].Name = "release.loop";
            agCmds[46].ArgType = new ArgTypeEnum[1];
            agCmds[46].ArgType[0] = atSObj;

            agCmds[47].Name = "set.cel";
            agCmds[47].ArgType = new ArgTypeEnum[2];
            agCmds[47].ArgType[0] = atSObj;
            agCmds[47].ArgType[1] = atNum;

            agCmds[48].Name = "set.cel.v";
            agCmds[48].ArgType = new ArgTypeEnum[2];
            agCmds[48].ArgType[0] = atSObj;
            agCmds[48].ArgType[1] = atVar;

            agCmds[49].Name = "last.cel";
            agCmds[49].ArgType = new ArgTypeEnum[2];
            agCmds[49].ArgType[0] = atSObj;
            agCmds[49].ArgType[1] = atVar;

            agCmds[50].Name = "current.cel";
            agCmds[50].ArgType = new ArgTypeEnum[2];
            agCmds[50].ArgType[0] = atSObj;
            agCmds[50].ArgType[1] = atVar;

            agCmds[51].Name = "current.loop";
            agCmds[51].ArgType = new ArgTypeEnum[2];
            agCmds[51].ArgType[0] = atSObj;
            agCmds[51].ArgType[1] = atVar;

            agCmds[52].Name = "current.view";
            agCmds[52].ArgType = new ArgTypeEnum[2];
            agCmds[52].ArgType[0] = atSObj;
            agCmds[52].ArgType[1] = atVar;

            agCmds[53].Name = "number.of.loops";
            agCmds[53].ArgType = new ArgTypeEnum[2];
            agCmds[53].ArgType[0] = atSObj;
            agCmds[53].ArgType[1] = atVar;

            agCmds[54].Name = "set.priority";
            agCmds[54].ArgType = new ArgTypeEnum[2];
            agCmds[54].ArgType[0] = atSObj;
            agCmds[54].ArgType[1] = atNum;

            agCmds[55].Name = "set.priority.v";
            agCmds[55].ArgType = new ArgTypeEnum[2];
            agCmds[55].ArgType[0] = atSObj;
            agCmds[55].ArgType[1] = atVar;

            agCmds[56].Name = "release.priority";
            agCmds[56].ArgType = new ArgTypeEnum[1];
            agCmds[56].ArgType[0] = atSObj;

            agCmds[57].Name = "get.priority";
            agCmds[57].ArgType = new ArgTypeEnum[2];
            agCmds[57].ArgType[0] = atSObj;
            agCmds[57].ArgType[1] = atVar;

            agCmds[58].Name = "stop.update";
            agCmds[58].ArgType = new ArgTypeEnum[1];
            agCmds[58].ArgType[0] = atSObj;

            agCmds[59].Name = "start.update";
            agCmds[59].ArgType = new ArgTypeEnum[1];
            agCmds[59].ArgType[0] = atSObj;

            agCmds[60].Name = "force.update";
            agCmds[60].ArgType = new ArgTypeEnum[1];
            agCmds[60].ArgType[0] = atSObj;

            agCmds[61].Name = "ignore.horizon";
            agCmds[61].ArgType = new ArgTypeEnum[1];
            agCmds[61].ArgType[0] = atSObj;

            agCmds[62].Name = "observe.horizon";
            agCmds[62].ArgType = new ArgTypeEnum[1];
            agCmds[62].ArgType[0] = atSObj;

            agCmds[63].Name = "set.horizon";
            agCmds[63].ArgType = new ArgTypeEnum[1];
            agCmds[63].ArgType[0] = atNum;

            agCmds[64].Name = "object.on.water";
            agCmds[64].ArgType = new ArgTypeEnum[1];
            agCmds[64].ArgType[0] = atSObj;

            agCmds[65].Name = "object.on.land";
            agCmds[65].ArgType = new ArgTypeEnum[1];
            agCmds[65].ArgType[0] = atSObj;

            agCmds[66].Name = "object.on.anything";
            agCmds[66].ArgType = new ArgTypeEnum[1];
            agCmds[66].ArgType[0] = atSObj;

            agCmds[67].Name = "ignore.objs";
            agCmds[67].ArgType = new ArgTypeEnum[1];
            agCmds[67].ArgType[0] = atSObj;

            agCmds[68].Name = "observe.objs";
            agCmds[68].ArgType = new ArgTypeEnum[1];
            agCmds[68].ArgType[0] = atSObj;

            agCmds[69].Name = "distance";
            agCmds[69].ArgType = new ArgTypeEnum[3];
            agCmds[69].ArgType[0] = atSObj;
            agCmds[69].ArgType[1] = atSObj;
            agCmds[69].ArgType[2] = atVar;

            agCmds[70].Name = "stop.cycling";
            agCmds[70].ArgType = new ArgTypeEnum[1];
            agCmds[70].ArgType[0] = atSObj;

            agCmds[71].Name = "start.cycling";
            agCmds[71].ArgType = new ArgTypeEnum[1];
            agCmds[71].ArgType[0] = atSObj;

            agCmds[72].Name = "normal.cycle";
            agCmds[72].ArgType = new ArgTypeEnum[1];
            agCmds[72].ArgType[0] = atSObj;

            agCmds[73].Name = "end.of.loop";
            agCmds[73].ArgType = new ArgTypeEnum[2];
            agCmds[73].ArgType[0] = atSObj;
            agCmds[73].ArgType[1] = atFlag;

            agCmds[74].Name = "reverse.cycle";
            agCmds[74].ArgType = new ArgTypeEnum[1];
            agCmds[74].ArgType[0] = atSObj; ;

            agCmds[75].Name = "reverse.loop";
            agCmds[75].ArgType = new ArgTypeEnum[2];
            agCmds[75].ArgType[0] = atSObj;
            agCmds[75].ArgType[1] = atFlag;

            agCmds[76].Name = "cycle.time";
            agCmds[76].ArgType = new ArgTypeEnum[2];
            agCmds[76].ArgType[0] = atSObj;
            agCmds[76].ArgType[1] = atVar;

            agCmds[77].Name = "stop.motion";
            agCmds[77].ArgType = new ArgTypeEnum[1];
            agCmds[77].ArgType[0] = atSObj;

            agCmds[78].Name = "start.motion";
            agCmds[78].ArgType = new ArgTypeEnum[1];
            agCmds[78].ArgType[0] = atSObj;

            agCmds[79].Name = "step.size";
            agCmds[79].ArgType = new ArgTypeEnum[2];
            agCmds[79].ArgType[0] = atSObj;
            agCmds[79].ArgType[1] = atVar;

            agCmds[80].Name = "step.time";
            agCmds[80].ArgType = new ArgTypeEnum[2];
            agCmds[80].ArgType[0] = atSObj;
            agCmds[80].ArgType[1] = atVar;

            agCmds[81].Name = "move.obj";
            agCmds[81].ArgType = new ArgTypeEnum[5];
            agCmds[81].ArgType[0] = atSObj;
            agCmds[81].ArgType[1] = atNum;
            agCmds[81].ArgType[2] = atNum;
            agCmds[81].ArgType[3] = atNum;
            agCmds[81].ArgType[4] = atFlag;

            agCmds[82].Name = "move.obj.v";
            agCmds[82].ArgType = new ArgTypeEnum[5];
            agCmds[82].ArgType[0] = atSObj;
            agCmds[82].ArgType[1] = atVar;
            agCmds[82].ArgType[2] = atVar;
            agCmds[82].ArgType[3] = atVar;
            agCmds[82].ArgType[4] = atFlag;

            agCmds[83].Name = "follow.ego";
            agCmds[83].ArgType = new ArgTypeEnum[3];
            agCmds[83].ArgType[0] = atSObj;
            agCmds[83].ArgType[1] = atNum;
            agCmds[83].ArgType[2] = atFlag;

            agCmds[84].Name = "wander";
            agCmds[84].ArgType = new ArgTypeEnum[1];
            agCmds[84].ArgType[0] = atSObj;

            agCmds[85].Name = "normal.motion";
            agCmds[85].ArgType = new ArgTypeEnum[1];
            agCmds[85].ArgType[0] = atSObj;

            agCmds[86].Name = "set.dir";
            agCmds[86].ArgType = new ArgTypeEnum[2];
            agCmds[86].ArgType[0] = atSObj;
            agCmds[86].ArgType[1] = atVar;

            agCmds[87].Name = "get.dir";
            agCmds[87].ArgType = new ArgTypeEnum[2];
            agCmds[87].ArgType[0] = atSObj;
            agCmds[87].ArgType[1] = atVar;

            agCmds[88].Name = "ignore.blocks";
            agCmds[88].ArgType = new ArgTypeEnum[1];
            agCmds[88].ArgType[0] = atSObj;

            agCmds[89].Name = "observe.blocks";
            agCmds[89].ArgType = new ArgTypeEnum[1];
            agCmds[89].ArgType[0] = atSObj;

            agCmds[90].Name = "block";
            agCmds[90].ArgType = new ArgTypeEnum[4];
            agCmds[90].ArgType[0] = atNum;
            agCmds[90].ArgType[1] = atNum;
            agCmds[90].ArgType[2] = atNum;
            agCmds[90].ArgType[3] = atNum;

            agCmds[91].Name = "unblock";
            agCmds[91].ArgType = [];

            agCmds[92].Name = "get";
            agCmds[92].ArgType = new ArgTypeEnum[1];
            agCmds[92].ArgType[0] = atInvItem;

            agCmds[93].Name = "get.v";
            agCmds[93].ArgType = new ArgTypeEnum[1];
            agCmds[93].ArgType[0] = atVar;

            agCmds[94].Name = "drop";
            agCmds[94].ArgType = new ArgTypeEnum[1];
            agCmds[94].ArgType[0] = atInvItem;

            agCmds[95].Name = "put";
            agCmds[95].ArgType = new ArgTypeEnum[2];
            agCmds[95].ArgType[0] = atInvItem;
            agCmds[95].ArgType[1] = atVar;

            agCmds[96].Name = "put.v";
            agCmds[96].ArgType = new ArgTypeEnum[2];
            agCmds[96].ArgType[0] = atVar;
            agCmds[96].ArgType[1] = atVar;

            agCmds[97].Name = "get.room.v";
            agCmds[97].ArgType = new ArgTypeEnum[2];
            agCmds[97].ArgType[0] = atVar;
            agCmds[97].ArgType[1] = atVar;

            agCmds[98].Name = "load.sound";
            agCmds[98].ArgType = new ArgTypeEnum[1];
            agCmds[98].ArgType[0] = atNum;

            agCmds[99].Name = "sound";
            agCmds[99].ArgType = new ArgTypeEnum[2];
            agCmds[99].ArgType[0] = atNum;
            agCmds[99].ArgType[1] = atFlag;

            agCmds[100].Name = "stop.sound";
            agCmds[100].ArgType = [];

            agCmds[101].Name = "print";
            agCmds[101].ArgType = new ArgTypeEnum[1];
            agCmds[101].ArgType[0] = atMsg;

            agCmds[102].Name = "print.v";
            agCmds[102].ArgType = new ArgTypeEnum[1];
            agCmds[102].ArgType[0] = atVar;

            agCmds[103].Name = "display";
            agCmds[103].ArgType = new ArgTypeEnum[3];
            agCmds[103].ArgType[0] = atNum;
            agCmds[103].ArgType[1] = atNum;
            agCmds[103].ArgType[2] = atMsg;

            agCmds[104].Name = "display.v";
            agCmds[104].ArgType = new ArgTypeEnum[3];
            agCmds[104].ArgType[0] = atVar;
            agCmds[104].ArgType[1] = atVar;
            agCmds[104].ArgType[2] = atVar;

            agCmds[105].Name = "clear.lines";
            agCmds[105].ArgType = new ArgTypeEnum[3];
            agCmds[105].ArgType[0] = atNum;
            agCmds[105].ArgType[1] = atNum;
            agCmds[105].ArgType[2] = atNum;

            agCmds[106].Name = "text.screen";
            agCmds[106].ArgType = [];

            agCmds[107].Name = "graphics";
            agCmds[107].ArgType = [];

            agCmds[108].Name = "set.cursor.char";
            agCmds[108].ArgType = new ArgTypeEnum[1];
            agCmds[108].ArgType[0] = atMsg;

            agCmds[109].Name = "set.text.attribute";
            agCmds[109].ArgType = new ArgTypeEnum[2];
            agCmds[109].ArgType[0] = atNum;
            agCmds[109].ArgType[1] = atNum;

            agCmds[110].Name = "shake.screen";
            agCmds[110].ArgType = new ArgTypeEnum[1];
            agCmds[110].ArgType[0] = atNum;

            agCmds[111].Name = "configure.screen";
            agCmds[111].ArgType = new ArgTypeEnum[3];
            agCmds[111].ArgType[0] = atNum;
            agCmds[111].ArgType[1] = atNum;
            agCmds[111].ArgType[2] = atNum;

            agCmds[112].Name = "status.line.on";
            agCmds[112].ArgType = [];

            agCmds[113].Name = "status.line.off";
            agCmds[113].ArgType = [];

            agCmds[114].Name = "set.string";
            agCmds[114].ArgType = new ArgTypeEnum[2];
            agCmds[114].ArgType[0] = atStr;
            agCmds[114].ArgType[1] = atMsg;

            agCmds[115].Name = "get.string";
            agCmds[115].ArgType = new ArgTypeEnum[5];
            agCmds[115].ArgType[0] = atStr;
            agCmds[115].ArgType[1] = atMsg;
            agCmds[115].ArgType[2] = atNum;
            agCmds[115].ArgType[3] = atNum;
            agCmds[115].ArgType[4] = atNum;

            agCmds[116].Name = "word.to.string";
            //  the word arguments are zero-based [i.e.
            //  first word is w0, second word is w1, etc
            //  but when using format codes, the codes are 1-based
            //  i.e. %w1 is first word, %w2 is second word, etc
            //  to eliminate confusion, compiler automatically compensates
            agCmds[116].ArgType = new ArgTypeEnum[2];
            agCmds[116].ArgType[0] = atStr;
            agCmds[116].ArgType[1] = atWord;

            agCmds[117].Name = "parse";
            agCmds[117].ArgType = new ArgTypeEnum[1];
            agCmds[117].ArgType[0] = atStr;

            agCmds[118].Name = "get.num";
            agCmds[118].ArgType = new ArgTypeEnum[2];
            agCmds[118].ArgType[0] = atMsg;
            agCmds[118].ArgType[1] = atVar;

            agCmds[119].Name = "prevent.input";
            agCmds[119].ArgType = [];

            agCmds[120].Name = "accept.input";
            agCmds[120].ArgType = [];

            agCmds[121].Name = "set.key";
            agCmds[121].ArgType = new ArgTypeEnum[3];
            agCmds[121].ArgType[0] = atNum;
            agCmds[121].ArgType[1] = atNum;
            agCmds[121].ArgType[2] = atCtrl;

            agCmds[122].Name = "add.to.pic";
            agCmds[122].ArgType = new ArgTypeEnum[7];
            agCmds[122].ArgType[0] = atNum;
            agCmds[122].ArgType[1] = atNum;
            agCmds[122].ArgType[2] = atNum;
            agCmds[122].ArgType[3] = atNum;
            agCmds[122].ArgType[4] = atNum;
            agCmds[122].ArgType[5] = atNum;
            agCmds[122].ArgType[6] = atNum;

            agCmds[123].Name = "add.to.pic.v";
            agCmds[123].ArgType = new ArgTypeEnum[7];
            agCmds[123].ArgType[0] = atVar;
            agCmds[123].ArgType[1] = atVar;
            agCmds[123].ArgType[2] = atVar;
            agCmds[123].ArgType[3] = atVar;
            agCmds[123].ArgType[4] = atVar;
            agCmds[123].ArgType[5] = atVar;
            agCmds[123].ArgType[6] = atVar;

            agCmds[124].Name = "status";
            agCmds[124].ArgType = [];

            agCmds[125].Name = "save.game";
            agCmds[125].ArgType = [];

            agCmds[126].Name = "restore.game";
            agCmds[126].ArgType = [];

            agCmds[127].Name = "init.disk";
            agCmds[127].ArgType = [];

            agCmds[128].Name = "restart.game";
            agCmds[128].ArgType = [];

            agCmds[129].Name = "show.obj";
            agCmds[129].ArgType = new ArgTypeEnum[1];
            agCmds[129].ArgType[0] = atNum;

            agCmds[130].Name = "random";
            agCmds[130].ArgType = new ArgTypeEnum[3];
            agCmds[130].ArgType[0] = atNum;
            agCmds[130].ArgType[1] = atNum;
            agCmds[130].ArgType[2] = atVar;

            agCmds[131].Name = "program.control";
            agCmds[131].ArgType = [];

            agCmds[132].Name = "player.control";
            agCmds[132].ArgType = [];

            agCmds[133].Name = "obj.status.v";
            agCmds[133].ArgType = new ArgTypeEnum[1];
            agCmds[133].ArgType[0] = atVar;

            agCmds[134].Name = "quit";
            agCmds[134].ArgType = new ArgTypeEnum[1];
            agCmds[134].ArgType[0] = atNum;

            agCmds[135].Name = "show.mem";
            agCmds[135].ArgType = [];

            agCmds[136].Name = "pause";
            agCmds[136].ArgType = [];

            agCmds[137].Name = "echo.line";
            agCmds[137].ArgType = [];

            agCmds[138].Name = "cancel.line";
            agCmds[138].ArgType = [];

            agCmds[139].Name = "init.joy";
            agCmds[139].ArgType = [];

            agCmds[140].Name = "toggle.monitor";
            agCmds[140].ArgType = [];

            agCmds[141].Name = "version";
            agCmds[141].ArgType = [];

            agCmds[142].Name = "script.size";
            agCmds[142].ArgType = new ArgTypeEnum[1];
            agCmds[142].ArgType[0] = atNum;

            agCmds[143].Name = "set.game.id";
            agCmds[143].ArgType = new ArgTypeEnum[1];
            agCmds[143].ArgType[0] = atMsg;

            agCmds[144].Name = "log";
            agCmds[144].ArgType = new ArgTypeEnum[1];
            agCmds[144].ArgType[0] = atMsg;

            agCmds[145].Name = "set.scan.start";
            agCmds[145].ArgType = [];

            agCmds[146].Name = "reset.scan.start";
            agCmds[146].ArgType = [];

            agCmds[147].Name = "reposition.to";
            agCmds[147].ArgType = new ArgTypeEnum[3];
            agCmds[147].ArgType[0] = atSObj;
            agCmds[147].ArgType[1] = atNum;
            agCmds[147].ArgType[2] = atNum;

            agCmds[148].Name = "reposition.to.v";
            agCmds[148].ArgType = new ArgTypeEnum[3];
            agCmds[148].ArgType[0] = atSObj;
            agCmds[148].ArgType[1] = atVar;
            agCmds[148].ArgType[2] = atVar;

            agCmds[149].Name = "trace.on";
            agCmds[149].ArgType = [];

            agCmds[150].Name = "trace.info";
            agCmds[150].ArgType = new ArgTypeEnum[3];
            agCmds[150].ArgType[0] = atNum;
            agCmds[150].ArgType[1] = atNum;
            agCmds[150].ArgType[2] = atNum;

            agCmds[151].Name = "print.at";
            agCmds[151].ArgType = new ArgTypeEnum[4];
            agCmds[151].ArgType[0] = atMsg;
            agCmds[151].ArgType[1] = atNum;
            agCmds[151].ArgType[2] = atNum;
            agCmds[151].ArgType[3] = atNum;

            agCmds[152].Name = "print.at.v";
            agCmds[152].ArgType = new ArgTypeEnum[4];
            agCmds[152].ArgType[0] = atVar;
            agCmds[152].ArgType[1] = atNum;
            agCmds[152].ArgType[2] = atNum;
            agCmds[152].ArgType[3] = atNum;

            agCmds[153].Name = "discard.view.v";
            agCmds[153].ArgType = new ArgTypeEnum[1];
            agCmds[153].ArgType[0] = atVar;

            agCmds[154].Name = "clear.text.rect";
            agCmds[154].ArgType = new ArgTypeEnum[5];
            agCmds[154].ArgType[0] = atNum;
            agCmds[154].ArgType[1] = atNum;
            agCmds[154].ArgType[2] = atNum;
            agCmds[154].ArgType[3] = atNum;
            agCmds[154].ArgType[4] = atNum;

            agCmds[155].Name = "set.upper.left";
            agCmds[155].ArgType = new ArgTypeEnum[2];
            agCmds[155].ArgType[0] = atNum;
            agCmds[155].ArgType[1] = atNum;

            agCmds[156].Name = "set.menu";
            agCmds[156].ArgType = new ArgTypeEnum[1];
            agCmds[156].ArgType[0] = atMsg;

            agCmds[157].Name = "set.menu.item";
            agCmds[157].ArgType = new ArgTypeEnum[2];
            agCmds[157].ArgType[0] = atMsg;
            agCmds[157].ArgType[1] = atCtrl;

            agCmds[158].Name = "submit.menu";
            agCmds[158].ArgType = [];

            agCmds[159].Name = "enable.item";
            agCmds[159].ArgType = new ArgTypeEnum[1];
            agCmds[159].ArgType[0] = atCtrl;

            agCmds[160].Name = "disable.item";
            agCmds[160].ArgType = new ArgTypeEnum[1];
            agCmds[160].ArgType[0] = atCtrl;

            agCmds[161].Name = "menu.input";
            agCmds[161].ArgType = [];

            agCmds[162].Name = "show.obj.v";
            agCmds[162].ArgType = new ArgTypeEnum[1];
            agCmds[162].ArgType[0] = atVar;

            agCmds[163].Name = "open.dialogue";
            agCmds[163].ArgType = [];

            agCmds[164].Name = "close.dialogue";
            agCmds[164].ArgType = [];

            agCmds[165].Name = "mul.n";
            agCmds[165].ArgType = new ArgTypeEnum[2];
            agCmds[165].ArgType[0] = atVar;
            agCmds[165].ArgType[1] = atNum;

            agCmds[166].Name = "mul.v";
            agCmds[166].ArgType = new ArgTypeEnum[2];
            agCmds[166].ArgType[0] = atVar;
            agCmds[166].ArgType[1] = atVar;

            agCmds[167].Name = "div.n";
            agCmds[167].ArgType = new ArgTypeEnum[2];
            agCmds[167].ArgType[0] = atVar;
            agCmds[167].ArgType[1] = atNum;

            agCmds[168].Name = "div.v";
            agCmds[168].ArgType = new ArgTypeEnum[2];
            agCmds[168].ArgType[0] = atVar;
            agCmds[168].ArgType[1] = atVar;

            agCmds[169].Name = "close.window";
            agCmds[169].ArgType = [];

            agCmds[170].Name = "set.simple";
            agCmds[170].ArgType = new ArgTypeEnum[1];
            agCmds[170].ArgType[0] = atStr;

            agCmds[171].Name = "push.script";
            agCmds[171].ArgType = [];

            agCmds[172].Name = "pop.script";
            agCmds[172].ArgType = [];

            agCmds[173].Name = "hold.key";
            agCmds[173].ArgType = [];

            agCmds[174].Name = "set.pri.base";
            agCmds[174].ArgType = new ArgTypeEnum[1];
            agCmds[174].ArgType[0] = atNum;

            agCmds[175].Name = "discard.sound";
            agCmds[175].ArgType = new ArgTypeEnum[1];
            agCmds[175].ArgType[0] = atNum;

            agCmds[176].Name = "hide.mouse";
            // i think arg count  should always be 1
            agCmds[176].ArgType = new ArgTypeEnum[1];
            agCmds[176].ArgType[0] = atNum;

            agCmds[177].Name = "allow.menu";
            agCmds[177].ArgType = new ArgTypeEnum[1];
            agCmds[177].ArgType[0] = atNum;

            agCmds[178].Name = "show.mouse";
            // i think ArgType.Length should be 1
            agCmds[178].ArgType = new ArgTypeEnum[1];
            agCmds[178].ArgType[0] = atNum;

            agCmds[179].Name = "fence.mouse";
            agCmds[179].ArgType = new ArgTypeEnum[4];
            agCmds[179].ArgType[0] = atNum;
            agCmds[179].ArgType[1] = atNum;
            agCmds[179].ArgType[2] = atNum;
            agCmds[179].ArgType[3] = atNum;

            agCmds[180].Name = "mouse.posn";
            agCmds[180].ArgType = new ArgTypeEnum[2];
            agCmds[180].ArgType[0] = atNum;
            agCmds[180].ArgType[1] = atNum;

            agCmds[181].Name = "release.key";
            agCmds[181].ArgType = [];

            // currently, this command is not supported, and 
            // can never be accessed
            //agCmds[182].Name = "adj.ego.move.to.x.y";
            //agCmds[182].ArgType = new ArgTypeEnum[2];
            //agCmds[182].ArgType[0] = ArgTypeEnum.atNum;
            //agCmds[182].ArgType[1] = ArgTypeEnum.atNum;

            // test commands
            agNumTestCmds = 20;

            agTestCmds[0].Name = "return.false";
            agTestCmds[0].ArgType = [];

            agTestCmds[1].Name = "equaln";
            agTestCmds[1].ArgType = new ArgTypeEnum[2];
            agTestCmds[1].ArgType[0] = atVar;
            agTestCmds[1].ArgType[1] = atNum;

            agTestCmds[2].Name = "equalv";
            agTestCmds[2].ArgType = new ArgTypeEnum[2];
            agTestCmds[2].ArgType[0] = atVar;
            agTestCmds[2].ArgType[1] = atVar;

            agTestCmds[3].Name = "lessn";
            agTestCmds[3].ArgType = new ArgTypeEnum[2];
            agTestCmds[3].ArgType[0] = atVar;
            agTestCmds[3].ArgType[1] = atNum;

            agTestCmds[4].Name = "lessv";
            agTestCmds[4].ArgType = new ArgTypeEnum[2];
            agTestCmds[4].ArgType[0] = atVar;
            agTestCmds[4].ArgType[1] = atVar;

            agTestCmds[5].Name = "greatern";
            agTestCmds[5].ArgType = new ArgTypeEnum[2];
            agTestCmds[5].ArgType[0] = atVar;
            agTestCmds[5].ArgType[1] = atNum;

            agTestCmds[6].Name = "greaterv";
            agTestCmds[6].ArgType = new ArgTypeEnum[2];
            agTestCmds[6].ArgType[0] = atVar;
            agTestCmds[6].ArgType[1] = atVar;

            agTestCmds[7].Name = "isset";
            agTestCmds[7].ArgType = new ArgTypeEnum[1];
            agTestCmds[7].ArgType[0] = atFlag;

            agTestCmds[8].Name = "issetv";
            agTestCmds[8].ArgType = new ArgTypeEnum[1];
            agTestCmds[8].ArgType[0] = atVar;

            agTestCmds[9].Name = "has";
            agTestCmds[9].ArgType = new ArgTypeEnum[1];
            agTestCmds[9].ArgType[0] = atInvItem;

            agTestCmds[10].Name = "obj.in.room";
            agTestCmds[10].ArgType = new ArgTypeEnum[2];
            agTestCmds[10].ArgType[0] = atInvItem;
            agTestCmds[10].ArgType[1] = atVar;

            agTestCmds[11].Name = "posn";
            agTestCmds[11].ArgType = new ArgTypeEnum[5];
            agTestCmds[11].ArgType[0] = atSObj;
            agTestCmds[11].ArgType[1] = atNum;
            agTestCmds[11].ArgType[2] = atNum;
            agTestCmds[11].ArgType[3] = atNum;
            agTestCmds[11].ArgType[4] = atNum;

            agTestCmds[12].Name = "controller";
            agTestCmds[12].ArgType = new ArgTypeEnum[1];
            agTestCmds[12].ArgType[0] = atCtrl;

            agTestCmds[13].Name = "have.key";
            agTestCmds[13].ArgType = [];

            agTestCmds[14].Name = "said";
            // said is special because it has variable number of arguments;
            // set it blank here, compiler adjusts it as needed
            agTestCmds[14].ArgType = [];

            agTestCmds[15].Name = "compare.strings";
            agTestCmds[15].ArgType = new ArgTypeEnum[2];
            agTestCmds[15].ArgType[0] = atStr;
            agTestCmds[15].ArgType[1] = atStr;

            agTestCmds[16].Name = "obj.in.box";
            agTestCmds[16].ArgType = new ArgTypeEnum[5];
            agTestCmds[16].ArgType[0] = atSObj;
            agTestCmds[16].ArgType[1] = atNum;
            agTestCmds[16].ArgType[2] = atNum;
            agTestCmds[16].ArgType[3] = atNum;
            agTestCmds[16].ArgType[4] = atNum;

            agTestCmds[17].Name = "center.posn";
            agTestCmds[17].ArgType = new ArgTypeEnum[5];
            agTestCmds[17].ArgType[0] = atSObj;
            agTestCmds[17].ArgType[1] = atNum;
            agTestCmds[17].ArgType[2] = atNum;
            agTestCmds[17].ArgType[3] = atNum;
            agTestCmds[17].ArgType[4] = atNum;

            agTestCmds[18].Name = "right.posn";
            agTestCmds[18].ArgType = new ArgTypeEnum[5];
            agTestCmds[18].ArgType[0] = atSObj;
            agTestCmds[18].ArgType[1] = atNum;
            agTestCmds[18].ArgType[2] = atNum;
            agTestCmds[18].ArgType[3] = atNum;
            agTestCmds[18].ArgType[4] = atNum;

            // in.motion.using.mouse
            // This command is not confirmed; I *think* I found it
            // described in an Apple or Atari version, but I can't
            // seem to find it again.
            agTestCmds[19].Name = "in.motion.using.mouse";
            agTestCmds[19].ArgType = [];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the test commands collection.
        /// </summary>
        public static CommandStruct[] TestCommands {
            get { return agTestCmds; }
        }

        /// <summary>
        /// Gets the number of test commands.
        /// </summary>
        public static byte TestCount {
            get { return agNumTestCmds; }
        }

        /// <summary>
        /// Gets the action commands collection.
        /// </summary>
        public static CommandStruct[] ActionCommands {
            get { return agCmds; }
        }

        /// <summary>
        /// Gets the number of action commands.
        /// </summary>
        public static byte ActionCount {
            get { return agNumCmds; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// This method adjusts the commands to conform to the given 
        /// interpreter version.
        /// </summary>
        /// <param name="Version"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static void CorrectCommands(string Version) {
            if (!IntVersions.Contains(Version)) {
                throw new ArgumentOutOfRangeException(nameof(Version));
            }
            double verNum = double.Parse(Version);
            // quit command
            if (verNum <= 2.089) {
                agCmds[134].ArgType = [];
            }
            else {
                agCmds[134].ArgType = new ArgTypeEnum[1];
                agCmds[134].ArgType[0] = atNum;
            }

            // adjust number of available commands
            if (verNum <= 2.089)
                agNumCmds = 156;
            else if (verNum <= 2.272)
                agNumCmds = 162;
            else if (verNum <= 2.44)
                agNumCmds = 170;
            else if (verNum <= 2.917)
                agNumCmds = 174;
            else if (verNum <= 2.936)
                agNumCmds = 176;
            else if (verNum <= 3.002086)
                agNumCmds = 178;
            else
                // all are available
                agNumCmds = MAX_CMDS;
        }
        #endregion
    }
}
