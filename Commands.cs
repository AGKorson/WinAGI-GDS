using System;
using static WinAGI.Engine.ArgType;

namespace WinAGI.Engine {
    /// <summary>
    /// A class to hold all members related to AGI's action and test commands using fan
    /// syntax.
    /// </summary>
    public static class Commands {
        #region Fields
        internal static readonly string[] agArgTypPref = ["", "v", "f", "m", "o", "i", "s", "w", "c"];
        internal static readonly string[] agArgTypName =
            [
            "number", "variable", "flag", "message", "object",
            "inventory item", "string", "word", "controller"
            ];
        internal const int MAX_CMDS = 182, MAX_TESTCMDS = 19;
        internal static byte agNumCmds;
        internal static byte agNumTestCmds;
        internal static CommandStruct[] agCmds = new CommandStruct[MAX_CMDS];
        internal static CommandStruct[] agTestCmds = new CommandStruct[MAX_TESTCMDS];

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
            agCmds[0].ArgList = [];

            agCmds[1].FanName = "increment";
            agCmds[1].ArgList = [Var];

            agCmds[2].FanName = "decrement";
            agCmds[2].ArgList = [Var];

            agCmds[3].FanName = "assignn";
            agCmds[3].ArgList = [Var, Num];

            agCmds[4].FanName = "assignv";
            agCmds[4].ArgList = [Var, Var];

            agCmds[5].FanName = "addn";
            agCmds[5].ArgList = [Var, Num];

            agCmds[6].FanName = "addv";
            agCmds[6].ArgList = [Var, Var];

            agCmds[7].FanName = "subn";
            agCmds[7].ArgList = [Var, Num];

            agCmds[8].FanName = "subv";
            agCmds[8].ArgList = [Var, Var];

            agCmds[9].FanName = "lindirectv";
            agCmds[9].ArgList = [Var, Var];

            agCmds[10].FanName = "rindirect";
            agCmds[10].ArgList = [Var, Var];

            agCmds[11].FanName = "lindirectn";
            agCmds[11].ArgList = [Var, Num];

            agCmds[12].FanName = "set";
            agCmds[12].ArgList = [Flag];

            agCmds[13].FanName = "reset";
            agCmds[13].ArgList = [Flag];

            agCmds[14].FanName = "toggle";
            agCmds[14].ArgList = [Flag];

            agCmds[15].FanName = "set.v";
            agCmds[15].ArgList = [Var];

            agCmds[16].FanName = "reset.v";
            agCmds[16].ArgList = [Var];

            agCmds[17].FanName = "toggle.v";
            agCmds[17].ArgList = [Var];

            agCmds[18].FanName = "new.room";
            agCmds[18].ArgList = [Num];

            agCmds[19].FanName = "new.room.v";
            agCmds[19].ArgList = [Var];

            agCmds[20].FanName = "load.logics";
            agCmds[20].ArgList = [Num];

            agCmds[21].FanName = "load.logics.v";
            agCmds[21].ArgList = [Var];

            agCmds[22].FanName = "call";
            agCmds[22].ArgList = [Num];

            agCmds[23].FanName = "call.v";
            agCmds[23].ArgList = [Var];

            agCmds[24].FanName = "load.pic";
            agCmds[24].ArgList = [Var];

            agCmds[25].FanName = "draw.pic";
            agCmds[25].ArgList = [Var];

            agCmds[26].FanName = "show.pic";
            agCmds[26].ArgList = [];

            agCmds[27].FanName = "discard.pic";
            agCmds[27].ArgList = [Var];

            agCmds[28].FanName = "overlay.pic";
            agCmds[28].ArgList = [Var];

            agCmds[29].FanName = "show.pri.screen";
            agCmds[29].ArgList = [];

            agCmds[30].FanName = "load.view";
            agCmds[30].ArgList = [Num];

            agCmds[31].FanName = "load.view.v";
            agCmds[31].ArgList = [Var];

            agCmds[32].FanName = "discard.view";
            agCmds[32].ArgList = [Num];

            agCmds[33].FanName = "animate.obj";
            agCmds[33].ArgList = [SObj];

            agCmds[34].FanName = "unanimate.all";
            agCmds[34].ArgList = [];

            agCmds[35].FanName = "draw";
            agCmds[35].ArgList = [SObj];

            agCmds[36].FanName = "erase";
            agCmds[36].ArgList = [SObj];

            agCmds[37].FanName = "position";
            agCmds[37].ArgList = [SObj, Num, Num];

            agCmds[38].FanName = "position.v";
            agCmds[38].ArgList = [SObj, Var, Var];

            agCmds[39].FanName = "get.posn";
            agCmds[39].ArgList = [SObj, Var, Var];

            agCmds[40].FanName = "reposition";
            agCmds[40].ArgList = [SObj, Var, Var];

            agCmds[41].FanName = "set.view";
            agCmds[41].ArgList = [SObj, Num];

            agCmds[42].FanName = "set.view.v";
            agCmds[42].ArgList = [SObj, Var];

            agCmds[43].FanName = "set.loop";
            agCmds[43].ArgList = [SObj, Num];

            agCmds[44].FanName = "set.loop.v";
            agCmds[44].ArgList = [SObj, Var];

            agCmds[45].FanName = "fix.loop";
            agCmds[45].ArgList = [SObj];

            agCmds[46].FanName = "release.loop";
            agCmds[46].ArgList = [SObj];

            agCmds[47].FanName = "set.cel";
            agCmds[47].ArgList = [SObj, Num];

            agCmds[48].FanName = "set.cel.v";
            agCmds[48].ArgList = [SObj, Var];

            agCmds[49].FanName = "last.cel";
            agCmds[49].ArgList = [SObj, Var];

            agCmds[50].FanName = "current.cel";
            agCmds[50].ArgList = [SObj, Var];

            agCmds[51].FanName = "current.loop";
            agCmds[51].ArgList = [SObj, Var];

            agCmds[52].FanName = "current.view";
            agCmds[52].ArgList = [SObj, Var];

            agCmds[53].FanName = "number.of.loops";
            agCmds[53].ArgList = [SObj, Var];

            agCmds[54].FanName = "set.priority";
            agCmds[54].ArgList = [SObj, Num];

            agCmds[55].FanName = "set.priority.v";
            agCmds[55].ArgList = [SObj, Var];

            agCmds[56].FanName = "release.priority";
            agCmds[56].ArgList = [SObj];

            agCmds[57].FanName = "get.priority";
            agCmds[57].ArgList = [SObj, Var];

            agCmds[58].FanName = "stop.update";
            agCmds[58].ArgList = [SObj];

            agCmds[59].FanName = "start.update";
            agCmds[59].ArgList = [SObj];

            agCmds[60].FanName = "force.update";
            agCmds[60].ArgList = [SObj];

            agCmds[61].FanName = "ignore.horizon";
            agCmds[61].ArgList = [SObj];

            agCmds[62].FanName = "observe.horizon";
            agCmds[62].ArgList = [SObj];

            agCmds[63].FanName = "set.horizon";
            agCmds[63].ArgList = [Num];

            agCmds[64].FanName = "object.on.water";
            agCmds[64].ArgList = [SObj];

            agCmds[65].FanName = "object.on.land";
            agCmds[65].ArgList = [SObj];

            agCmds[66].FanName = "object.on.anything";
            agCmds[66].ArgList = [SObj];

            agCmds[67].FanName = "ignore.objs";
            agCmds[67].ArgList = [SObj];

            agCmds[68].FanName = "observe.objs";
            agCmds[68].ArgList = [SObj];

            agCmds[69].FanName = "distance";
            agCmds[69].ArgList = [SObj, SObj, Var];

            agCmds[70].FanName = "stop.cycling";
            agCmds[70].ArgList = [SObj];

            agCmds[71].FanName = "start.cycling";
            agCmds[71].ArgList = [SObj];

            agCmds[72].FanName = "normal.cycle";
            agCmds[72].ArgList = [SObj];

            agCmds[73].FanName = "end.of.loop";
            agCmds[73].ArgList = [SObj, Flag];

            agCmds[74].FanName = "reverse.cycle";
            agCmds[74].ArgList = [SObj];

            agCmds[75].FanName = "reverse.loop";
            agCmds[75].ArgList = [SObj, Flag];

            agCmds[76].FanName = "cycle.time";
            agCmds[76].ArgList = [SObj, Var];

            agCmds[77].FanName = "stop.motion";
            agCmds[77].ArgList = [SObj];

            agCmds[78].FanName = "start.motion";
            agCmds[78].ArgList = [SObj];

            agCmds[79].FanName = "step.size";
            agCmds[79].ArgList = [SObj, Var];

            agCmds[80].FanName = "step.time";
            agCmds[80].ArgList = [SObj, Var];

            agCmds[81].FanName = "move.obj";
            agCmds[81].ArgList = [SObj, Num, Num, Num, Flag];

            agCmds[82].FanName = "move.obj.v";
            agCmds[82].ArgList = [SObj, Var, Var, Var, Flag];

            agCmds[83].FanName = "follow.ego";
            agCmds[83].ArgList = [SObj, Num, Flag];

            agCmds[84].FanName = "wander";
            agCmds[84].ArgList = [SObj];

            agCmds[85].FanName = "normal.motion";
            agCmds[85].ArgList = [SObj];

            agCmds[86].FanName = "set.dir";
            agCmds[86].ArgList = [SObj, Var];

            agCmds[87].FanName = "get.dir";
            agCmds[87].ArgList = [SObj, Var];

            agCmds[88].FanName = "ignore.blocks";
            agCmds[88].ArgList = [SObj];

            agCmds[89].FanName = "observe.blocks";
            agCmds[89].ArgList = [SObj];

            agCmds[90].FanName = "block";
            agCmds[90].ArgList = [Num, Num, Num, Num];

            agCmds[91].FanName = "unblock";
            agCmds[91].ArgList = [];

            agCmds[92].FanName = "get";
            agCmds[92].ArgList = [InvItem];

            agCmds[93].FanName = "get.v";
            agCmds[93].ArgList = [Var];

            agCmds[94].FanName = "drop";
            agCmds[94].ArgList = [InvItem];

            agCmds[95].FanName = "put";
            agCmds[95].ArgList = [InvItem, Var];

            agCmds[96].FanName = "put.v";
            agCmds[96].ArgList = [Var, Var];

            agCmds[97].FanName = "get.room.v";
            agCmds[97].ArgList = [Var, Var];

            agCmds[98].FanName = "load.sound";
            agCmds[98].ArgList = [Num];

            agCmds[99].FanName = "sound";
            agCmds[99].ArgList = [Num, Flag];

            agCmds[100].FanName = "stop.sound";
            agCmds[100].ArgList = [];

            agCmds[101].FanName = "print";
            agCmds[101].ArgList = [MsgNum];

            agCmds[102].FanName = "print.v";
            agCmds[102].ArgList = [Var];

            agCmds[103].FanName = "display";
            agCmds[103].ArgList = [Num, Num, MsgNum];

            agCmds[104].FanName = "display.v";
            agCmds[104].ArgList = [Var, Var, Var];

            agCmds[105].FanName = "clear.lines";
            agCmds[105].ArgList = [Num, Num, Num];

            agCmds[106].FanName = "text.screen";
            agCmds[106].ArgList = [];

            agCmds[107].FanName = "graphics";
            agCmds[107].ArgList = [];

            agCmds[108].FanName = "set.cursor.char";
            agCmds[108].ArgList = [MsgNum];

            agCmds[109].FanName = "set.text.attribute";
            agCmds[109].ArgList = [Num, Num];

            agCmds[110].FanName = "shake.screen";
            agCmds[110].ArgList = [Num];

            agCmds[111].FanName = "configure.screen";
            agCmds[111].ArgList = [Num, Num, Num];

            agCmds[112].FanName = "status.line.on";
            agCmds[112].ArgList = [];

            agCmds[113].FanName = "status.line.off";
            agCmds[113].ArgList = [];

            agCmds[114].FanName = "set.string";
            agCmds[114].ArgList = [Str, MsgNum];

            agCmds[115].FanName = "get.string";
            agCmds[115].ArgList = [Str, MsgNum, Num, Num, Num];

            agCmds[116].FanName = "word.to.string";
            //  the word arguments are zero-based [i.e.
            //  first word is w0, second word is w1, etc
            //  but when using format codes, the codes are 1-based
            //  i.e. %w1 is first word, %w2 is second word, etc
            //  to eliminate confusion, compiler automatically compensates
            agCmds[116].ArgList = [Str, Word];

            agCmds[117].FanName = "parse";
            agCmds[117].ArgList = [Str];

            agCmds[118].FanName = "get.num";
            agCmds[118].ArgList = [MsgNum, Var];

            agCmds[119].FanName = "prevent.input";
            agCmds[119].ArgList = [];

            agCmds[120].FanName = "accept.input";
            agCmds[120].ArgList = [];

            agCmds[121].FanName = "set.key";
            agCmds[121].ArgList = [Num, Num, Ctrl];

            agCmds[122].FanName = "add.to.pic";
            agCmds[122].ArgList = [Num, Num, Num, Num, Num, Num, Num];

            agCmds[123].FanName = "add.to.pic.v";
            agCmds[123].ArgList = [Var, Var, Var, Var, Var, Var, Var];

            agCmds[124].FanName = "status";
            agCmds[124].ArgList = [];

            agCmds[125].FanName = "save.game";
            agCmds[125].ArgList = [];

            agCmds[126].FanName = "restore.game";
            agCmds[126].ArgList = [];

            agCmds[127].FanName = "init.disk";
            agCmds[127].ArgList = [];

            agCmds[128].FanName = "restart.game";
            agCmds[128].ArgList = [];

            agCmds[129].FanName = "show.obj";
            agCmds[129].ArgList = [Num];

            agCmds[130].FanName = "random";
            agCmds[130].ArgList = [Num, Num, Var];

            agCmds[131].FanName = "program.control";
            agCmds[131].ArgList = [];

            agCmds[132].FanName = "player.control";
            agCmds[132].ArgList = [];

            agCmds[133].FanName = "obj.status.v";
            agCmds[133].ArgList = [Var];

            agCmds[134].FanName = "quit";
            agCmds[134].ArgList = [Num];

            agCmds[135].FanName = "show.mem";
            agCmds[135].ArgList = [];

            agCmds[136].FanName = "pause";
            agCmds[136].ArgList = [];

            agCmds[137].FanName = "echo.line";
            agCmds[137].ArgList = [];

            agCmds[138].FanName = "cancel.line";
            agCmds[138].ArgList = [];

            agCmds[139].FanName = "init.joy";
            agCmds[139].ArgList = [];

            agCmds[140].FanName = "toggle.monitor";
            agCmds[140].ArgList = [];

            agCmds[141].FanName = "version";
            agCmds[141].ArgList = [];

            agCmds[142].FanName = "script.size";
            agCmds[142].ArgList = [Num];

            agCmds[143].FanName = "set.game.id";
            agCmds[143].ArgList = [MsgNum];

            agCmds[144].FanName = "log";
            agCmds[144].ArgList = [MsgNum];

            agCmds[145].FanName = "set.scan.start";
            agCmds[145].ArgList = [];

            agCmds[146].FanName = "reset.scan.start";
            agCmds[146].ArgList = [];

            agCmds[147].FanName = "reposition.to";
            agCmds[147].ArgList = [SObj, Num, Num];

            agCmds[148].FanName = "reposition.to.v";
            agCmds[148].ArgList = [SObj, Var, Var];

            agCmds[149].FanName = "trace.on";
            agCmds[149].ArgList = [];

            agCmds[150].FanName = "trace.info";
            agCmds[150].ArgList = [Num, Num, Num];

            agCmds[151].FanName = "print.at";
            agCmds[151].ArgList = [MsgNum, Num, Num, Num];

            agCmds[152].FanName = "print.at.v";
            agCmds[152].ArgList = [Var, Num, Num, Num];

            agCmds[153].FanName = "discard.view.v";
            agCmds[153].ArgList = [Var];

            agCmds[154].FanName = "clear.text.rect";
            agCmds[154].ArgList = [Num, Num, Num, Num, Num];

            agCmds[155].FanName = "set.upper.left";
            agCmds[155].ArgList = [Num, Num];

            agCmds[156].FanName = "set.menu";
            agCmds[156].ArgList = [MsgNum];

            agCmds[157].FanName = "set.menu.item";
            agCmds[157].ArgList = [MsgNum, Ctrl];

            agCmds[158].FanName = "submit.menu";
            agCmds[158].ArgList = [];

            agCmds[159].FanName = "enable.item";
            agCmds[159].ArgList = [Ctrl];

            agCmds[160].FanName = "disable.item";
            agCmds[160].ArgList = [Ctrl];

            agCmds[161].FanName = "menu.input";
            agCmds[161].ArgList = [];

            agCmds[162].FanName = "show.obj.v";
            agCmds[162].ArgList = [Var];

            agCmds[163].FanName = "open.dialogue";
            agCmds[163].ArgList = [];

            agCmds[164].FanName = "close.dialogue";
            agCmds[164].ArgList = [];

            agCmds[165].FanName = "mul.n";
            agCmds[165].ArgList = [Var, Num];

            agCmds[166].FanName = "mul.v";
            agCmds[166].ArgList = [Var, Var];

            agCmds[167].FanName = "div.n";
            agCmds[167].ArgList = [Var, Num];

            agCmds[168].FanName = "div.v";
            agCmds[168].ArgList = [Var, Var];

            agCmds[169].FanName = "close.window";
            agCmds[169].ArgList = [];

            agCmds[170].FanName = "set.simple";
            agCmds[170].ArgList = [Str];

            agCmds[171].FanName = "push.script";
            agCmds[171].ArgList = [];

            agCmds[172].FanName = "pop.script";
            agCmds[172].ArgList = [];

            agCmds[173].FanName = "hold.key";
            agCmds[173].ArgList = [];

            agCmds[174].FanName = "set.pri.base";
            agCmds[174].ArgList = [Num];

            agCmds[175].FanName = "discard.sound";
            agCmds[175].ArgList = [Num];

            agCmds[176].FanName = "hide.mouse";
            // i think arg count  should always be 1
            agCmds[176].ArgList = [Num];

            agCmds[177].FanName = "allow.menu";
            agCmds[177].ArgList = [Num];

            agCmds[178].FanName = "show.mouse";
            // i think ArgType.Length should be 1
            agCmds[178].ArgList = [Num];

            agCmds[179].FanName = "fence.mouse";
            agCmds[179].ArgList = [Num, Num, Num, Num];

            agCmds[180].FanName = "mouse.posn";
            agCmds[180].ArgList = [Num, Num];

            agCmds[181].FanName = "release.key";
            agCmds[181].ArgList = [];

            // currently, this command is not supported, and 
            // can never be accessed
            //agCmds[182].Name = "adj.ego.move.to.x.y";
            //agCmds[182].ArgType = new ArgTypeEnum[2];
            //agCmds[182].ArgType[0] = ArgTypeEnum.atNum;
            //agCmds[182].ArgType[1] = ArgTypeEnum.atNum;

            // test commands
            agNumTestCmds = MAX_TESTCMDS;

            agTestCmds[0].FanName = "return.false";
            agTestCmds[0].ArgList = [];

            agTestCmds[1].FanName = "equaln";
            agTestCmds[1].ArgList = [Var, Num];

            agTestCmds[2].FanName = "equalv";
            agTestCmds[2].ArgList = [Var, Var];

            agTestCmds[3].FanName = "lessn";
            agTestCmds[3].ArgList = [Var, Num];

            agTestCmds[4].FanName = "lessv";
            agTestCmds[4].ArgList = [Var, Var];

            agTestCmds[5].FanName = "greatern";
            agTestCmds[5].ArgList = [Var, Num];

            agTestCmds[6].FanName = "greaterv";
            agTestCmds[6].ArgList = [Var, Var];

            agTestCmds[7].FanName = "isset";
            agTestCmds[7].ArgList = [Flag];

            agTestCmds[8].FanName = "issetv";
            agTestCmds[8].ArgList = [Var];

            agTestCmds[9].FanName = "has";
            agTestCmds[9].ArgList = [InvItem];

            agTestCmds[10].FanName = "obj.in.room";
            agTestCmds[10].ArgList = [InvItem, Var];

            agTestCmds[11].FanName = "posn";
            agTestCmds[11].ArgList = [SObj, Num, Num, Num, Num];

            agTestCmds[12].FanName = "controller";
            agTestCmds[12].ArgList = [Ctrl];

            agTestCmds[13].FanName = "have.key";
            agTestCmds[13].ArgList = [];

            agTestCmds[14].FanName = "said";
            // said is special because it has variable number of arguments;
            // set it blank here, compiler adjusts it as needed
            agTestCmds[14].ArgList = [];

            agTestCmds[15].FanName = "compare.strings";
            agTestCmds[15].ArgList = [Str, Str];

            agTestCmds[16].FanName = "obj.in.box";
            agTestCmds[16].ArgList = [SObj, Num, Num, Num, Num];

            agTestCmds[17].FanName = "center.posn";
            agTestCmds[17].ArgList = [SObj, Num, Num, Num, Num];

            agTestCmds[18].FanName = "right.posn";
            agTestCmds[18].ArgList = [SObj, Num, Num, Num, Num];

            // in.motion.using.mouse
            // This command is not confirmed; I *think* I found it
            // described in an Apple or Atari version, but I can't
            // seem to find it again.
            //agTestCmds[19].FanName = "in.motion.using.mouse";
            //agTestCmds[19].ArgList = [];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the test commands collection.
        /// </summary>
        public static CommandStruct[] TestCommands {
            get {
                return agTestCmds;
            }
        }

        /// <summary>
        /// Gets the number of test commands.
        /// </summary>
        public static byte TestCount {
            get {
                return agNumTestCmds;
            }
        }

        /// <summary>
        /// Gets the action commands collection.
        /// </summary>
        public static CommandStruct[] ActionCommands {
            get {
                return agCmds;
            }
        }

        /// <summary>
        /// Gets the number of action commands.
        /// </summary>
        public static byte ActionCount {
            get {
                return agNumCmds;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// This method adjusts the commands to conform to the given 
        /// interpreter version.
        /// </summary>
        /// <param name="Version"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static void CorrectCommands(AGIVersion Version) {
            // quit command
            if (Version == AGIVersion.v2089 || Version == AGIVersion.v2230) {
                agCmds[134].ArgList = [];
            }
            else {
                agCmds[134].ArgList = new ArgType[1];
                agCmds[134].ArgList[0] = Num;
            }

            // adjust number of available commands
            switch (Version) {
            case AGIVersion.v2089 or AGIVersion.v2230:
                agNumCmds = 155;
                break;
            case AGIVersion.v2272:
                agNumCmds = 161;
                break;
            case >= AGIVersion.v2411 and <= AGIVersion.v2440:
                // 2.411, 2.425, 2.426, 2.435, 2.439, 2.440
                agNumCmds = 170;
                break;
            case >= AGIVersion.v2903 and <= AGIVersion.v2917:
                // 2.903, 2.911, 2.912, 2.915, 2.917
                agNumCmds = 174;
                break;
            case AGIVersion.v2936:
                agNumCmds = 176;
                break;
            case AGIVersion.v3002086:
                agNumCmds = 178;
                break;
            default:
                // all are available for rest of v3
                agNumCmds = MAX_CMDS;
                break;
            }
        }
        #endregion
    }
}
