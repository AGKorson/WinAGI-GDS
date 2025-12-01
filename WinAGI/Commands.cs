using System;
using System.Linq;
using static WinAGI.Engine.ArgType;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class to hold all members related to AGI's action and test commands using fan
    /// syntax.
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
            agCmds[0].FanName = "return";
            agCmds[0].ArgType = [];

            agCmds[1].FanName = "increment";
            agCmds[1].ArgType = new ArgType[1];
            agCmds[1].ArgType[0] = Var;

            agCmds[2].FanName = "decrement";
            agCmds[2].ArgType = new ArgType[1];
            agCmds[2].ArgType[0] = Var;

            agCmds[3].FanName = "assignn";
            agCmds[3].ArgType = new ArgType[2];
            agCmds[3].ArgType[0] = Var;
            agCmds[3].ArgType[1] = Num;

            agCmds[4].FanName = "assignv";
            agCmds[4].ArgType = new ArgType[2];
            agCmds[4].ArgType[0] = Var;
            agCmds[4].ArgType[1] = Var;

            agCmds[5].FanName = "addn";
            agCmds[5].ArgType = new ArgType[2];
            agCmds[5].ArgType[0] = Var;
            agCmds[5].ArgType[1] = Num;

            agCmds[6].FanName = "addv";
            agCmds[6].ArgType = new ArgType[2];
            agCmds[6].ArgType[0] = Var;
            agCmds[6].ArgType[1] = Var;

            agCmds[7].FanName = "subn";
            agCmds[7].ArgType = new ArgType[2];
            agCmds[7].ArgType[0] = Var;
            agCmds[7].ArgType[1] = Num;

            agCmds[8].FanName = "subv";
            agCmds[8].ArgType = new ArgType[2];
            agCmds[8].ArgType[0] = Var;
            agCmds[8].ArgType[1] = Var;

            agCmds[9].FanName = "lindirectv";
            agCmds[9].ArgType = new ArgType[2];
            agCmds[9].ArgType[0] = Var;
            agCmds[9].ArgType[1] = Var;

            agCmds[10].FanName = "rindirect";
            agCmds[10].ArgType = new ArgType[2];
            agCmds[10].ArgType[0] = Var;
            agCmds[10].ArgType[1] = Var;

            agCmds[11].FanName = "lindirectn";
            agCmds[11].ArgType = new ArgType[2];
            agCmds[11].ArgType[0] = Var;
            agCmds[11].ArgType[1] = Num;

            agCmds[12].FanName = "set";
            agCmds[12].ArgType = new ArgType[1];
            agCmds[12].ArgType[0] = Flag;

            agCmds[13].FanName = "reset";
            agCmds[13].ArgType = new ArgType[1];
            agCmds[13].ArgType[0] = Flag;

            agCmds[14].FanName = "toggle";
            agCmds[14].ArgType = new ArgType[1];
            agCmds[14].ArgType[0] = Flag; ;

            agCmds[15].FanName = "set.v";
            agCmds[15].ArgType = new ArgType[1];
            agCmds[15].ArgType[0] = Var;

            agCmds[16].FanName = "reset.v";
            agCmds[16].ArgType = new ArgType[1];
            agCmds[16].ArgType[0] = Var;

            agCmds[17].FanName = "toggle.v";
            agCmds[17].ArgType = new ArgType[1];
            agCmds[17].ArgType[0] = Var;

            agCmds[18].FanName = "new.room";
            agCmds[18].ArgType = new ArgType[1];
            agCmds[18].ArgType[0] = Num;

            agCmds[19].FanName = "new.room.v";
            agCmds[19].ArgType = new ArgType[1];
            agCmds[19].ArgType[0] = Var;

            agCmds[20].FanName = "load.logics";
            agCmds[20].ArgType = new ArgType[1];
            agCmds[20].ArgType[0] = Num;

            agCmds[21].FanName = "load.logics.v";
            agCmds[21].ArgType = new ArgType[1];
            agCmds[21].ArgType[0] = Var;

            agCmds[22].FanName = "call";
            agCmds[22].ArgType = new ArgType[1];
            agCmds[22].ArgType[0] = Num;

            agCmds[23].FanName = "call.v";
            agCmds[23].ArgType = new ArgType[1];
            agCmds[23].ArgType[0] = Var;

            agCmds[24].FanName = "load.pic";
            agCmds[24].ArgType = new ArgType[1];
            agCmds[24].ArgType[0] = Var;

            agCmds[25].FanName = "draw.pic";
            agCmds[25].ArgType = new ArgType[1];
            agCmds[25].ArgType[0] = Var;

            agCmds[26].FanName = "show.pic";
            agCmds[26].ArgType = [];

            agCmds[27].FanName = "discard.pic";
            agCmds[27].ArgType = new ArgType[1];
            agCmds[27].ArgType[0] = Var;

            agCmds[28].FanName = "overlay.pic";
            agCmds[28].ArgType = new ArgType[1];
            agCmds[28].ArgType[0] = Var;

            agCmds[29].FanName = "show.pri.screen";
            agCmds[29].ArgType = [];

            agCmds[30].FanName = "load.view";
            agCmds[30].ArgType = new ArgType[1];
            agCmds[30].ArgType[0] = Num;

            agCmds[31].FanName = "load.view.v";
            agCmds[31].ArgType = new ArgType[1];
            agCmds[31].ArgType[0] = Var;

            agCmds[32].FanName = "discard.view";
            agCmds[32].ArgType = new ArgType[1];
            agCmds[32].ArgType[0] = Num;

            agCmds[33].FanName = "animate.obj";
            agCmds[33].ArgType = new ArgType[1];
            agCmds[33].ArgType[0] = SObj;

            agCmds[34].FanName = "unanimate.all";
            agCmds[34].ArgType = [];

            agCmds[35].FanName = "draw";
            agCmds[35].ArgType = new ArgType[1];
            agCmds[35].ArgType[0] = SObj;

            agCmds[36].FanName = "erase";
            agCmds[36].ArgType = new ArgType[1];
            agCmds[36].ArgType[0] = SObj;

            agCmds[37].FanName = "position";
            agCmds[37].ArgType = new ArgType[3];
            agCmds[37].ArgType[0] = SObj;
            agCmds[37].ArgType[1] = Num;
            agCmds[37].ArgType[2] = Num;

            agCmds[38].FanName = "position.v";
            agCmds[38].ArgType = new ArgType[3];
            agCmds[38].ArgType[0] = SObj;
            agCmds[38].ArgType[1] = Var;
            agCmds[38].ArgType[2] = Var;

            agCmds[39].FanName = "get.posn";
            agCmds[39].ArgType = new ArgType[3];
            agCmds[39].ArgType[0] = SObj;
            agCmds[39].ArgType[1] = Var;
            agCmds[39].ArgType[2] = Var;

            agCmds[40].FanName = "reposition";
            agCmds[40].ArgType = new ArgType[3];
            agCmds[40].ArgType[0] = SObj;
            agCmds[40].ArgType[1] = Var;
            agCmds[40].ArgType[2] = Var;

            agCmds[41].FanName = "set.view";
            agCmds[41].ArgType = new ArgType[2];
            agCmds[41].ArgType[0] = SObj;
            agCmds[41].ArgType[1] = Num;

            agCmds[42].FanName = "set.view.v";
            agCmds[42].ArgType = new ArgType[2];
            agCmds[42].ArgType[0] = SObj;
            agCmds[42].ArgType[1] = Var;

            agCmds[43].FanName = "set.loop";
            agCmds[43].ArgType = new ArgType[2];
            agCmds[43].ArgType[0] = SObj;
            agCmds[43].ArgType[1] = Num;

            agCmds[44].FanName = "set.loop.v";
            agCmds[44].ArgType = new ArgType[2];
            agCmds[44].ArgType[0] = SObj;
            agCmds[44].ArgType[1] = Var;

            agCmds[45].FanName = "fix.loop";
            agCmds[45].ArgType = new ArgType[1];
            agCmds[45].ArgType[0] = SObj;

            agCmds[46].FanName = "release.loop";
            agCmds[46].ArgType = new ArgType[1];
            agCmds[46].ArgType[0] = SObj;

            agCmds[47].FanName = "set.cel";
            agCmds[47].ArgType = new ArgType[2];
            agCmds[47].ArgType[0] = SObj;
            agCmds[47].ArgType[1] = Num;

            agCmds[48].FanName = "set.cel.v";
            agCmds[48].ArgType = new ArgType[2];
            agCmds[48].ArgType[0] = SObj;
            agCmds[48].ArgType[1] = Var;

            agCmds[49].FanName = "last.cel";
            agCmds[49].ArgType = new ArgType[2];
            agCmds[49].ArgType[0] = SObj;
            agCmds[49].ArgType[1] = Var;

            agCmds[50].FanName = "current.cel";
            agCmds[50].ArgType = new ArgType[2];
            agCmds[50].ArgType[0] = SObj;
            agCmds[50].ArgType[1] = Var;

            agCmds[51].FanName = "current.loop";
            agCmds[51].ArgType = new ArgType[2];
            agCmds[51].ArgType[0] = SObj;
            agCmds[51].ArgType[1] = Var;

            agCmds[52].FanName = "current.view";
            agCmds[52].ArgType = new ArgType[2];
            agCmds[52].ArgType[0] = SObj;
            agCmds[52].ArgType[1] = Var;

            agCmds[53].FanName = "number.of.loops";
            agCmds[53].ArgType = new ArgType[2];
            agCmds[53].ArgType[0] = SObj;
            agCmds[53].ArgType[1] = Var;

            agCmds[54].FanName = "set.priority";
            agCmds[54].ArgType = new ArgType[2];
            agCmds[54].ArgType[0] = SObj;
            agCmds[54].ArgType[1] = Num;

            agCmds[55].FanName = "set.priority.v";
            agCmds[55].ArgType = new ArgType[2];
            agCmds[55].ArgType[0] = SObj;
            agCmds[55].ArgType[1] = Var;

            agCmds[56].FanName = "release.priority";
            agCmds[56].ArgType = new ArgType[1];
            agCmds[56].ArgType[0] = SObj;

            agCmds[57].FanName = "get.priority";
            agCmds[57].ArgType = new ArgType[2];
            agCmds[57].ArgType[0] = SObj;
            agCmds[57].ArgType[1] = Var;

            agCmds[58].FanName = "stop.update";
            agCmds[58].ArgType = new ArgType[1];
            agCmds[58].ArgType[0] = SObj;

            agCmds[59].FanName = "start.update";
            agCmds[59].ArgType = new ArgType[1];
            agCmds[59].ArgType[0] = SObj;

            agCmds[60].FanName = "force.update";
            agCmds[60].ArgType = new ArgType[1];
            agCmds[60].ArgType[0] = SObj;

            agCmds[61].FanName = "ignore.horizon";
            agCmds[61].ArgType = new ArgType[1];
            agCmds[61].ArgType[0] = SObj;

            agCmds[62].FanName = "observe.horizon";
            agCmds[62].ArgType = new ArgType[1];
            agCmds[62].ArgType[0] = SObj;

            agCmds[63].FanName = "set.horizon";
            agCmds[63].ArgType = new ArgType[1];
            agCmds[63].ArgType[0] = Num;

            agCmds[64].FanName = "object.on.water";
            agCmds[64].ArgType = new ArgType[1];
            agCmds[64].ArgType[0] = SObj;

            agCmds[65].FanName = "object.on.land";
            agCmds[65].ArgType = new ArgType[1];
            agCmds[65].ArgType[0] = SObj;

            agCmds[66].FanName = "object.on.anything";
            agCmds[66].ArgType = new ArgType[1];
            agCmds[66].ArgType[0] = SObj;

            agCmds[67].FanName = "ignore.objs";
            agCmds[67].ArgType = new ArgType[1];
            agCmds[67].ArgType[0] = SObj;

            agCmds[68].FanName = "observe.objs";
            agCmds[68].ArgType = new ArgType[1];
            agCmds[68].ArgType[0] = SObj;

            agCmds[69].FanName = "distance";
            agCmds[69].ArgType = new ArgType[3];
            agCmds[69].ArgType[0] = SObj;
            agCmds[69].ArgType[1] = SObj;
            agCmds[69].ArgType[2] = Var;

            agCmds[70].FanName = "stop.cycling";
            agCmds[70].ArgType = new ArgType[1];
            agCmds[70].ArgType[0] = SObj;

            agCmds[71].FanName = "start.cycling";
            agCmds[71].ArgType = new ArgType[1];
            agCmds[71].ArgType[0] = SObj;

            agCmds[72].FanName = "normal.cycle";
            agCmds[72].ArgType = new ArgType[1];
            agCmds[72].ArgType[0] = SObj;

            agCmds[73].FanName = "end.of.loop";
            agCmds[73].ArgType = new ArgType[2];
            agCmds[73].ArgType[0] = SObj;
            agCmds[73].ArgType[1] = Flag;

            agCmds[74].FanName = "reverse.cycle";
            agCmds[74].ArgType = new ArgType[1];
            agCmds[74].ArgType[0] = SObj; ;

            agCmds[75].FanName = "reverse.loop";
            agCmds[75].ArgType = new ArgType[2];
            agCmds[75].ArgType[0] = SObj;
            agCmds[75].ArgType[1] = Flag;

            agCmds[76].FanName = "cycle.time";
            agCmds[76].ArgType = new ArgType[2];
            agCmds[76].ArgType[0] = SObj;
            agCmds[76].ArgType[1] = Var;

            agCmds[77].FanName = "stop.motion";
            agCmds[77].ArgType = new ArgType[1];
            agCmds[77].ArgType[0] = SObj;

            agCmds[78].FanName = "start.motion";
            agCmds[78].ArgType = new ArgType[1];
            agCmds[78].ArgType[0] = SObj;

            agCmds[79].FanName = "step.size";
            agCmds[79].ArgType = new ArgType[2];
            agCmds[79].ArgType[0] = SObj;
            agCmds[79].ArgType[1] = Var;

            agCmds[80].FanName = "step.time";
            agCmds[80].ArgType = new ArgType[2];
            agCmds[80].ArgType[0] = SObj;
            agCmds[80].ArgType[1] = Var;

            agCmds[81].FanName = "move.obj";
            agCmds[81].ArgType = new ArgType[5];
            agCmds[81].ArgType[0] = SObj;
            agCmds[81].ArgType[1] = Num;
            agCmds[81].ArgType[2] = Num;
            agCmds[81].ArgType[3] = Num;
            agCmds[81].ArgType[4] = Flag;

            agCmds[82].FanName = "move.obj.v";
            agCmds[82].ArgType = new ArgType[5];
            agCmds[82].ArgType[0] = SObj;
            agCmds[82].ArgType[1] = Var;
            agCmds[82].ArgType[2] = Var;
            agCmds[82].ArgType[3] = Var;
            agCmds[82].ArgType[4] = Flag;

            agCmds[83].FanName = "follow.ego";
            agCmds[83].ArgType = new ArgType[3];
            agCmds[83].ArgType[0] = SObj;
            agCmds[83].ArgType[1] = Num;
            agCmds[83].ArgType[2] = Flag;

            agCmds[84].FanName = "wander";
            agCmds[84].ArgType = new ArgType[1];
            agCmds[84].ArgType[0] = SObj;

            agCmds[85].FanName = "normal.motion";
            agCmds[85].ArgType = new ArgType[1];
            agCmds[85].ArgType[0] = SObj;

            agCmds[86].FanName = "set.dir";
            agCmds[86].ArgType = new ArgType[2];
            agCmds[86].ArgType[0] = SObj;
            agCmds[86].ArgType[1] = Var;

            agCmds[87].FanName = "get.dir";
            agCmds[87].ArgType = new ArgType[2];
            agCmds[87].ArgType[0] = SObj;
            agCmds[87].ArgType[1] = Var;

            agCmds[88].FanName = "ignore.blocks";
            agCmds[88].ArgType = new ArgType[1];
            agCmds[88].ArgType[0] = SObj;

            agCmds[89].FanName = "observe.blocks";
            agCmds[89].ArgType = new ArgType[1];
            agCmds[89].ArgType[0] = SObj;

            agCmds[90].FanName = "block";
            agCmds[90].ArgType = new ArgType[4];
            agCmds[90].ArgType[0] = Num;
            agCmds[90].ArgType[1] = Num;
            agCmds[90].ArgType[2] = Num;
            agCmds[90].ArgType[3] = Num;

            agCmds[91].FanName = "unblock";
            agCmds[91].ArgType = [];

            agCmds[92].FanName = "get";
            agCmds[92].ArgType = new ArgType[1];
            agCmds[92].ArgType[0] = InvItem;

            agCmds[93].FanName = "get.v";
            agCmds[93].ArgType = new ArgType[1];
            agCmds[93].ArgType[0] = Var;

            agCmds[94].FanName = "drop";
            agCmds[94].ArgType = new ArgType[1];
            agCmds[94].ArgType[0] = InvItem;

            agCmds[95].FanName = "put";
            agCmds[95].ArgType = new ArgType[2];
            agCmds[95].ArgType[0] = InvItem;
            agCmds[95].ArgType[1] = Var;

            agCmds[96].FanName = "put.v";
            agCmds[96].ArgType = new ArgType[2];
            agCmds[96].ArgType[0] = Var;
            agCmds[96].ArgType[1] = Var;

            agCmds[97].FanName = "get.room.v";
            agCmds[97].ArgType = new ArgType[2];
            agCmds[97].ArgType[0] = Var;
            agCmds[97].ArgType[1] = Var;

            agCmds[98].FanName = "load.sound";
            agCmds[98].ArgType = new ArgType[1];
            agCmds[98].ArgType[0] = Num;

            agCmds[99].FanName = "sound";
            agCmds[99].ArgType = new ArgType[2];
            agCmds[99].ArgType[0] = Num;
            agCmds[99].ArgType[1] = Flag;

            agCmds[100].FanName = "stop.sound";
            agCmds[100].ArgType = [];

            agCmds[101].FanName = "print";
            agCmds[101].ArgType = new ArgType[1];
            agCmds[101].ArgType[0] = Msg;

            agCmds[102].FanName = "print.v";
            agCmds[102].ArgType = new ArgType[1];
            agCmds[102].ArgType[0] = Var;

            agCmds[103].FanName = "display";
            agCmds[103].ArgType = new ArgType[3];
            agCmds[103].ArgType[0] = Num;
            agCmds[103].ArgType[1] = Num;
            agCmds[103].ArgType[2] = Msg;

            agCmds[104].FanName = "display.v";
            agCmds[104].ArgType = new ArgType[3];
            agCmds[104].ArgType[0] = Var;
            agCmds[104].ArgType[1] = Var;
            agCmds[104].ArgType[2] = Var;

            agCmds[105].FanName = "clear.lines";
            agCmds[105].ArgType = new ArgType[3];
            agCmds[105].ArgType[0] = Num;
            agCmds[105].ArgType[1] = Num;
            agCmds[105].ArgType[2] = Num;

            agCmds[106].FanName = "text.screen";
            agCmds[106].ArgType = [];

            agCmds[107].FanName = "graphics";
            agCmds[107].ArgType = [];

            agCmds[108].FanName = "set.cursor.char";
            agCmds[108].ArgType = new ArgType[1];
            agCmds[108].ArgType[0] = Msg;

            agCmds[109].FanName = "set.text.attribute";
            agCmds[109].ArgType = new ArgType[2];
            agCmds[109].ArgType[0] = Num;
            agCmds[109].ArgType[1] = Num;

            agCmds[110].FanName = "shake.screen";
            agCmds[110].ArgType = new ArgType[1];
            agCmds[110].ArgType[0] = Num;

            agCmds[111].FanName = "configure.screen";
            agCmds[111].ArgType = new ArgType[3];
            agCmds[111].ArgType[0] = Num;
            agCmds[111].ArgType[1] = Num;
            agCmds[111].ArgType[2] = Num;

            agCmds[112].FanName = "status.line.on";
            agCmds[112].ArgType = [];

            agCmds[113].FanName = "status.line.off";
            agCmds[113].ArgType = [];

            agCmds[114].FanName = "set.string";
            agCmds[114].ArgType = new ArgType[2];
            agCmds[114].ArgType[0] = Str;
            agCmds[114].ArgType[1] = Msg;

            agCmds[115].FanName = "get.string";
            agCmds[115].ArgType = new ArgType[5];
            agCmds[115].ArgType[0] = Str;
            agCmds[115].ArgType[1] = Msg;
            agCmds[115].ArgType[2] = Num;
            agCmds[115].ArgType[3] = Num;
            agCmds[115].ArgType[4] = Num;

            agCmds[116].FanName = "word.to.string";
            //  the word arguments are zero-based [i.e.
            //  first word is w0, second word is w1, etc
            //  but when using format codes, the codes are 1-based
            //  i.e. %w1 is first word, %w2 is second word, etc
            //  to eliminate confusion, compiler automatically compensates
            agCmds[116].ArgType = new ArgType[2];
            agCmds[116].ArgType[0] = Str;
            agCmds[116].ArgType[1] = Word;

            agCmds[117].FanName = "parse";
            agCmds[117].ArgType = new ArgType[1];
            agCmds[117].ArgType[0] = Str;

            agCmds[118].FanName = "get.num";
            agCmds[118].ArgType = new ArgType[2];
            agCmds[118].ArgType[0] = Msg;
            agCmds[118].ArgType[1] = Var;

            agCmds[119].FanName = "prevent.input";
            agCmds[119].ArgType = [];

            agCmds[120].FanName = "accept.input";
            agCmds[120].ArgType = [];

            agCmds[121].FanName = "set.key";
            agCmds[121].ArgType = new ArgType[3];
            agCmds[121].ArgType[0] = Num;
            agCmds[121].ArgType[1] = Num;
            agCmds[121].ArgType[2] = Ctrl;

            agCmds[122].FanName = "add.to.pic";
            agCmds[122].ArgType = new ArgType[7];
            agCmds[122].ArgType[0] = Num;
            agCmds[122].ArgType[1] = Num;
            agCmds[122].ArgType[2] = Num;
            agCmds[122].ArgType[3] = Num;
            agCmds[122].ArgType[4] = Num;
            agCmds[122].ArgType[5] = Num;
            agCmds[122].ArgType[6] = Num;

            agCmds[123].FanName = "add.to.pic.v";
            agCmds[123].ArgType = new ArgType[7];
            agCmds[123].ArgType[0] = Var;
            agCmds[123].ArgType[1] = Var;
            agCmds[123].ArgType[2] = Var;
            agCmds[123].ArgType[3] = Var;
            agCmds[123].ArgType[4] = Var;
            agCmds[123].ArgType[5] = Var;
            agCmds[123].ArgType[6] = Var;

            agCmds[124].FanName = "status";
            agCmds[124].ArgType = [];

            agCmds[125].FanName = "save.game";
            agCmds[125].ArgType = [];

            agCmds[126].FanName = "restore.game";
            agCmds[126].ArgType = [];

            agCmds[127].FanName = "init.disk";
            agCmds[127].ArgType = [];

            agCmds[128].FanName = "restart.game";
            agCmds[128].ArgType = [];

            agCmds[129].FanName = "show.obj";
            agCmds[129].ArgType = new ArgType[1];
            agCmds[129].ArgType[0] = Num;

            agCmds[130].FanName = "random";
            agCmds[130].ArgType = new ArgType[3];
            agCmds[130].ArgType[0] = Num;
            agCmds[130].ArgType[1] = Num;
            agCmds[130].ArgType[2] = Var;

            agCmds[131].FanName = "program.control";
            agCmds[131].ArgType = [];

            agCmds[132].FanName = "player.control";
            agCmds[132].ArgType = [];

            agCmds[133].FanName = "obj.status.v";
            agCmds[133].ArgType = new ArgType[1];
            agCmds[133].ArgType[0] = Var;

            agCmds[134].FanName = "quit";
            agCmds[134].ArgType = new ArgType[1];
            agCmds[134].ArgType[0] = Num;

            agCmds[135].FanName = "show.mem";
            agCmds[135].ArgType = [];

            agCmds[136].FanName = "pause";
            agCmds[136].ArgType = [];

            agCmds[137].FanName = "echo.line";
            agCmds[137].ArgType = [];

            agCmds[138].FanName = "cancel.line";
            agCmds[138].ArgType = [];

            agCmds[139].FanName = "init.joy";
            agCmds[139].ArgType = [];

            agCmds[140].FanName = "toggle.monitor";
            agCmds[140].ArgType = [];

            agCmds[141].FanName = "version";
            agCmds[141].ArgType = [];

            agCmds[142].FanName = "script.size";
            agCmds[142].ArgType = new ArgType[1];
            agCmds[142].ArgType[0] = Num;

            agCmds[143].FanName = "set.game.id";
            agCmds[143].ArgType = new ArgType[1];
            agCmds[143].ArgType[0] = Msg;

            agCmds[144].FanName = "log";
            agCmds[144].ArgType = new ArgType[1];
            agCmds[144].ArgType[0] = Msg;

            agCmds[145].FanName = "set.scan.start";
            agCmds[145].ArgType = [];

            agCmds[146].FanName = "reset.scan.start";
            agCmds[146].ArgType = [];

            agCmds[147].FanName = "reposition.to";
            agCmds[147].ArgType = new ArgType[3];
            agCmds[147].ArgType[0] = SObj;
            agCmds[147].ArgType[1] = Num;
            agCmds[147].ArgType[2] = Num;

            agCmds[148].FanName = "reposition.to.v";
            agCmds[148].ArgType = new ArgType[3];
            agCmds[148].ArgType[0] = SObj;
            agCmds[148].ArgType[1] = Var;
            agCmds[148].ArgType[2] = Var;

            agCmds[149].FanName = "trace.on";
            agCmds[149].ArgType = [];

            agCmds[150].FanName = "trace.info";
            agCmds[150].ArgType = new ArgType[3];
            agCmds[150].ArgType[0] = Num;
            agCmds[150].ArgType[1] = Num;
            agCmds[150].ArgType[2] = Num;

            agCmds[151].FanName = "print.at";
            agCmds[151].ArgType = new ArgType[4];
            agCmds[151].ArgType[0] = Msg;
            agCmds[151].ArgType[1] = Num;
            agCmds[151].ArgType[2] = Num;
            agCmds[151].ArgType[3] = Num;

            agCmds[152].FanName = "print.at.v";
            agCmds[152].ArgType = new ArgType[4];
            agCmds[152].ArgType[0] = Var;
            agCmds[152].ArgType[1] = Num;
            agCmds[152].ArgType[2] = Num;
            agCmds[152].ArgType[3] = Num;

            agCmds[153].FanName = "discard.view.v";
            agCmds[153].ArgType = new ArgType[1];
            agCmds[153].ArgType[0] = Var;

            agCmds[154].FanName = "clear.text.rect";
            agCmds[154].ArgType = new ArgType[5];
            agCmds[154].ArgType[0] = Num;
            agCmds[154].ArgType[1] = Num;
            agCmds[154].ArgType[2] = Num;
            agCmds[154].ArgType[3] = Num;
            agCmds[154].ArgType[4] = Num;

            agCmds[155].FanName = "set.upper.left";
            agCmds[155].ArgType = new ArgType[2];
            agCmds[155].ArgType[0] = Num;
            agCmds[155].ArgType[1] = Num;

            agCmds[156].FanName = "set.menu";
            agCmds[156].ArgType = new ArgType[1];
            agCmds[156].ArgType[0] = Msg;

            agCmds[157].FanName = "set.menu.item";
            agCmds[157].ArgType = new ArgType[2];
            agCmds[157].ArgType[0] = Msg;
            agCmds[157].ArgType[1] = Ctrl;

            agCmds[158].FanName = "submit.menu";
            agCmds[158].ArgType = [];

            agCmds[159].FanName = "enable.item";
            agCmds[159].ArgType = new ArgType[1];
            agCmds[159].ArgType[0] = Ctrl;

            agCmds[160].FanName = "disable.item";
            agCmds[160].ArgType = new ArgType[1];
            agCmds[160].ArgType[0] = Ctrl;

            agCmds[161].FanName = "menu.input";
            agCmds[161].ArgType = [];

            agCmds[162].FanName = "show.obj.v";
            agCmds[162].ArgType = new ArgType[1];
            agCmds[162].ArgType[0] = Var;

            agCmds[163].FanName = "open.dialogue";
            agCmds[163].ArgType = [];

            agCmds[164].FanName = "close.dialogue";
            agCmds[164].ArgType = [];

            agCmds[165].FanName = "mul.n";
            agCmds[165].ArgType = new ArgType[2];
            agCmds[165].ArgType[0] = Var;
            agCmds[165].ArgType[1] = Num;

            agCmds[166].FanName = "mul.v";
            agCmds[166].ArgType = new ArgType[2];
            agCmds[166].ArgType[0] = Var;
            agCmds[166].ArgType[1] = Var;

            agCmds[167].FanName = "div.n";
            agCmds[167].ArgType = new ArgType[2];
            agCmds[167].ArgType[0] = Var;
            agCmds[167].ArgType[1] = Num;

            agCmds[168].FanName = "div.v";
            agCmds[168].ArgType = new ArgType[2];
            agCmds[168].ArgType[0] = Var;
            agCmds[168].ArgType[1] = Var;

            agCmds[169].FanName = "close.window";
            agCmds[169].ArgType = [];

            agCmds[170].FanName = "set.simple";
            agCmds[170].ArgType = new ArgType[1];
            agCmds[170].ArgType[0] = Str;

            agCmds[171].FanName = "push.script";
            agCmds[171].ArgType = [];

            agCmds[172].FanName = "pop.script";
            agCmds[172].ArgType = [];

            agCmds[173].FanName = "hold.key";
            agCmds[173].ArgType = [];

            agCmds[174].FanName = "set.pri.base";
            agCmds[174].ArgType = new ArgType[1];
            agCmds[174].ArgType[0] = Num;

            agCmds[175].FanName = "discard.sound";
            agCmds[175].ArgType = new ArgType[1];
            agCmds[175].ArgType[0] = Num;

            agCmds[176].FanName = "hide.mouse";
            // i think arg count  should always be 1
            agCmds[176].ArgType = new ArgType[1];
            agCmds[176].ArgType[0] = Num;

            agCmds[177].FanName = "allow.menu";
            agCmds[177].ArgType = new ArgType[1];
            agCmds[177].ArgType[0] = Num;

            agCmds[178].FanName = "show.mouse";
            // i think ArgType.Length should be 1
            agCmds[178].ArgType = new ArgType[1];
            agCmds[178].ArgType[0] = Num;

            agCmds[179].FanName = "fence.mouse";
            agCmds[179].ArgType = new ArgType[4];
            agCmds[179].ArgType[0] = Num;
            agCmds[179].ArgType[1] = Num;
            agCmds[179].ArgType[2] = Num;
            agCmds[179].ArgType[3] = Num;

            agCmds[180].FanName = "mouse.posn";
            agCmds[180].ArgType = new ArgType[2];
            agCmds[180].ArgType[0] = Num;
            agCmds[180].ArgType[1] = Num;

            agCmds[181].FanName = "release.key";
            agCmds[181].ArgType = [];

            // currently, this command is not supported, and 
            // can never be accessed
            //agCmds[182].Name = "adj.ego.move.to.x.y";
            //agCmds[182].ArgType = new ArgTypeEnum[2];
            //agCmds[182].ArgType[0] = ArgTypeEnum.atNum;
            //agCmds[182].ArgType[1] = ArgTypeEnum.atNum;

            // test commands
            agNumTestCmds = 20;

            agTestCmds[0].FanName = "return.false";
            agTestCmds[0].ArgType = [];

            agTestCmds[1].FanName = "equaln";
            agTestCmds[1].ArgType = new ArgType[2];
            agTestCmds[1].ArgType[0] = Var;
            agTestCmds[1].ArgType[1] = Num;

            agTestCmds[2].FanName = "equalv";
            agTestCmds[2].ArgType = new ArgType[2];
            agTestCmds[2].ArgType[0] = Var;
            agTestCmds[2].ArgType[1] = Var;

            agTestCmds[3].FanName = "lessn";
            agTestCmds[3].ArgType = new ArgType[2];
            agTestCmds[3].ArgType[0] = Var;
            agTestCmds[3].ArgType[1] = Num;

            agTestCmds[4].FanName = "lessv";
            agTestCmds[4].ArgType = new ArgType[2];
            agTestCmds[4].ArgType[0] = Var;
            agTestCmds[4].ArgType[1] = Var;

            agTestCmds[5].FanName = "greatern";
            agTestCmds[5].ArgType = new ArgType[2];
            agTestCmds[5].ArgType[0] = Var;
            agTestCmds[5].ArgType[1] = Num;

            agTestCmds[6].FanName = "greaterv";
            agTestCmds[6].ArgType = new ArgType[2];
            agTestCmds[6].ArgType[0] = Var;
            agTestCmds[6].ArgType[1] = Var;

            agTestCmds[7].FanName = "isset";
            agTestCmds[7].ArgType = new ArgType[1];
            agTestCmds[7].ArgType[0] = Flag;

            agTestCmds[8].FanName = "issetv";
            agTestCmds[8].ArgType = new ArgType[1];
            agTestCmds[8].ArgType[0] = Var;

            agTestCmds[9].FanName = "has";
            agTestCmds[9].ArgType = new ArgType[1];
            agTestCmds[9].ArgType[0] = InvItem;

            agTestCmds[10].FanName = "obj.in.room";
            agTestCmds[10].ArgType = new ArgType[2];
            agTestCmds[10].ArgType[0] = InvItem;
            agTestCmds[10].ArgType[1] = Var;

            agTestCmds[11].FanName = "posn";
            agTestCmds[11].ArgType = new ArgType[5];
            agTestCmds[11].ArgType[0] = SObj;
            agTestCmds[11].ArgType[1] = Num;
            agTestCmds[11].ArgType[2] = Num;
            agTestCmds[11].ArgType[3] = Num;
            agTestCmds[11].ArgType[4] = Num;

            agTestCmds[12].FanName = "controller";
            agTestCmds[12].ArgType = new ArgType[1];
            agTestCmds[12].ArgType[0] = Ctrl;

            agTestCmds[13].FanName = "have.key";
            agTestCmds[13].ArgType = [];

            agTestCmds[14].FanName = "said";
            // said is special because it has variable number of arguments;
            // set it blank here, compiler adjusts it as needed
            agTestCmds[14].ArgType = [];

            agTestCmds[15].FanName = "compare.strings";
            agTestCmds[15].ArgType = new ArgType[2];
            agTestCmds[15].ArgType[0] = Str;
            agTestCmds[15].ArgType[1] = Str;

            agTestCmds[16].FanName = "obj.in.box";
            agTestCmds[16].ArgType = new ArgType[5];
            agTestCmds[16].ArgType[0] = SObj;
            agTestCmds[16].ArgType[1] = Num;
            agTestCmds[16].ArgType[2] = Num;
            agTestCmds[16].ArgType[3] = Num;
            agTestCmds[16].ArgType[4] = Num;

            agTestCmds[17].FanName = "center.posn";
            agTestCmds[17].ArgType = new ArgType[5];
            agTestCmds[17].ArgType[0] = SObj;
            agTestCmds[17].ArgType[1] = Num;
            agTestCmds[17].ArgType[2] = Num;
            agTestCmds[17].ArgType[3] = Num;
            agTestCmds[17].ArgType[4] = Num;

            agTestCmds[18].FanName = "right.posn";
            agTestCmds[18].ArgType = new ArgType[5];
            agTestCmds[18].ArgType[0] = SObj;
            agTestCmds[18].ArgType[1] = Num;
            agTestCmds[18].ArgType[2] = Num;
            agTestCmds[18].ArgType[3] = Num;
            agTestCmds[18].ArgType[4] = Num;

            // in.motion.using.mouse
            // This command is not confirmed; I *think* I found it
            // described in an Apple or Atari version, but I can't
            // seem to find it again.
            agTestCmds[19].FanName = "in.motion.using.mouse";
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
            int verIndex = Array.IndexOf(IntVersions, Version);
            // quit command
            if (verIndex == 0) {
                agCmds[134].ArgType = [];
            }
            else {
                agCmds[134].ArgType = new ArgType[1];
                agCmds[134].ArgType[0] = Num;
            }

            // adjust number of available commands
            switch (verIndex) {
            case 0:
                // 2.089
                agNumCmds = 156;
                break;
            case 1:
                // 2.272
                agNumCmds = 162;
                break;
            case >= 2 and <= 7:
                // 2.411, 2.425, 2.426, 2.435, 2.439, 2.440
                agNumCmds = 170;
                break;
            case >= 8 and <= 12:
                // 2.903, 2.911, 2.912, 2.915, 2.917
                agNumCmds = 174;
                break;
            case 13:
                // 2.936
                agNumCmds = 176;
                break;
            case 14:
                // 3.002086
                agNumCmds = 178;
                break;
            default:
                // all are available
                agNumCmds = MAX_CMDS;
                break;
            }
        }
        #endregion
    }
}
