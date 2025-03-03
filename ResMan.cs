using EnvDTE;
using FastColoredTextBoxNS;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Common.BkgdTasks;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Engine.ArgType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;

namespace WinAGI.Editor {
    /***************************************************************
    WinAGI Game Engine
    Copyright (C) 2005 - 2025 Andrew Korson

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 
    ***************************************************************/

    public static class Base {
        #region Global Constants
        //***************************************************
        // GLOBAL CONSTANTS
        //***************************************************

        // default settings - decompile
        public const int DEFAULT_CODESTYLE = 0;
        // default settings - layout editor

        //string constants
        public const string sLOGED = "Logic Editor - ";
        public const string sPICED = "Picture Editor - ";
        public const string sSNDED = "Sound Editor - ";
        public const string sVIEWED = "View Editor - ";
        public const string sDM = "* ";   //changed file marker//
        public const string sPOSITION = "Position";
        public const string sMRULIST = "MRUList";
        public const string sTOOLS = "Tools";
        public const string sGENERAL = "General";
        public const string sDEFCOLORS = "DefaultColors";
        public const string sRESFMT = "ResFormat";
        public const string sLAYOUT = "Layout";
        public const string sLOGICS = "Logics";
        public const string sSHFORMAT = "SyntaxHighlightFormat";
        public const string sPICTURES = "Pictures";
        public const string sPICTEST = "PicTest";
        public const string sSOUNDS = "Sounds";
        public const string sVIEWS = "Views";
        public const string sDECOMPILER = "Decompiler";
        public const string sOPENFILTER = "OpenFilter";
        public const string sEXPFILTER = "ExportFilter";

        //other app level constants
        internal static int PropRowHeight;
        internal static int PropPanelMaxSize;
        public static Color LtGray = Color.FromArgb(0xC0, 0xC0, 0xC0);
        public static Color DkGray = Color.FromArgb(0x80, 0x80, 0x80);
        public static Color PropGray = Color.FromArgb(0xEC, 0xE9, 0xD8);
        public static Color SelBlue = Color.FromArgb(0x31, 0x6A, 0xC5);
        public const double LG_SCROLL = 0.9; //0.8
        public const double SM_SCROLL = 0.225; //0.2

        //string resource offsets
        public const int COLORNAME = 1000;
        public const int KEYSIGNATURE = 1100;
        public const int VIEWUNDOTEXT = 1200;
        public const int PICUNDOTEXT = 1300;
        public const int SNDUNDOTEXT = 1400;
        public const int WORDSUNDOTEXT = 1500;
        public const int OBJUNDOTEXT = 1600;
        public const int PICTOOLTYPETEXT = 1800;
        public const int VIEWTOOLTYPETEXT = 1700;
        public const int DRAWFUNCTIONTEXT = 1900;
        public const int STOPREASONTEXT = 2000;
        public const int GLBUNDOTEXT = 2100;
        public const int INSTRUMENTNAMETEXT = 3000;
        public const int ALPHACMDTEXT = 4000;

        // RegEx searches for syntax styles:
        //Comment
        //String
        //keywords
        //action commands
        //test commands
        //invalid commands
        //numbers
        //argument identifier: @"[vfscimo]\d{1,3}\b"
        //generic identifier: @"(\w|\%|\$|\#|\.|[^\x00-\x7F])+"


        public static string CommentStyleRegEx1 = @"\[.*$";
        public static string CommentStyleRegEx2 = @"//.*$";
        public static string StringStyleRegEx = @"""""|"".*?[^\\\n]""|"".*";
        public static string KeyWordStyleRegEx = @"\bif\b|\belse\b|\bgoto\b|#define\b|#include\b|#message\b";
        public static string TestCmdStyleRegex = @"\b(" +
                @"center\.posn|compare\.strings|controller|equaln|equalv|greatern|greaterv|has|" +
                @"have\.key|isset|issetv|lessn|lessv|obj\.in\.box|obj\.in\.room|posn|right\.posn|" +
                @"said)\b";
        public static string ActionCmdStyleRegEx = @"\b(" +
                @"accept\.input|add\.to\.pic|add\.to\.pic\.v|addn|addv|adj\.ego\.move\.to\.x\.y|" +
                @"allow\.menu|animate\.obj|assignn|assignv|block|call\.v|call|cancel\.line|" +
                @"clear\.lines|clear\.text\.rect|close\.dialogue|close\.window|configure\.screen|" +
                @"current\.cel|current\.loop|current\.view|cycle\.time|decrement|disable\.item|" +
                @"discard\.pic|discard\.sound|discard\.view\.v|discard\.view|display\.v|display|" +
                @"distance|div\.n|div\.v|draw\.pic|draw|drop|echo\.line|enable\.item|" +
                @"end\.of\.loop|erase|fence\.mouse|fix\.loop|follow\.ego|force\.update|get\.dir|" +
                @"get\.num|get\.posn|get\.priority|get\.room\.v|get\.string|get\.v|get|graphics|" +
                @"hide\.mouse|hold\.key|ignore\.blocks|ignore\.horizon|ignore\.objs|increment|" +
                @"init\.disk|init\.joy|last\.cel|lindirectn|lindirectv|load\.logics\.v|" +
                @"load\.logics|load\.pic|load\.sound|load\.view\.v|load\.view|log|menu\.input|" +
                @"mouse\.posn|move\.obj\.v|move\.obj|mul\.n|mul\.v|new\.room\.v|new\.room|" +
                @"normal\.cycle|normal\.motion|number\.of\.loops|obj\.status\.v|" +
                @"object\.on\.anything|object\.on\.land|object\.on\.water|observe\.blocks|" +
                @"observe\.horizon|observe\.objs|open\.dialogue|overlay\.pic|parse|pause|" +
                @"player\.control|pop\.script|position\.v|position|prevent\.input|print\.at\.v|" +
                @"print\.at|print\.v|print|program\.control|push\.script|put\.v|put|quit|random|" +
                @"release\.key|release\.loop|release\.priority|reposition\.to\.v|reposition\.to|" +
                @"reposition|reset\.v|reset\.scan\.start|reset|restart\.game|restore\.game|" +
                @"return\.false|return|reverse\.cycle|reverse\.loop|rindirect|save\.game|" +
                @"script\.size|set\.cel\.v|set\.cel|set\.cursor\.char|set\.dir|set\.game\.id|" +
                @"set\.horizon|set\.key|set\.loop\.v|set\.loop|set\.menu\.item|set\.menu|" +
                @"set\.pri\.base|set\.priority\.v|set\.priority|set\.scan\.start|set\.simple|" +
                @"set\.string|set\.text\.attribute|set\.upper\.left|set\.view\.v|set\.view|set\.v|" +
                @"set|shake\.screen|show\.mem|show\.mouse|show\.obj\.v|show\.obj|show\.pic|" +
                @"show\.pri\.screen|sound|start\.cycling|start\.motion|start\.update|" +
                @"status\.line\.off|status\.line\.on|status|step\.size|step\.time|stop\.cycling|" +
                @"stop\.motion|stop\.sound|stop\.update|submit\.menu|subn|subv|text\.screen|" +
                @"toggle\.v|toggle\.monitor|toggle|trace\.info|trace\.on|unanimate\.all|unblock|" +
                @"unknowntest19|version|wander|word\.to\.string)\b";
        public static string InvalidCmdStyleRegEx = @"";
        public static string NumberStyleRegEx = @"\b\d+\b";
        public static string ArgIdentifierStyleRegEx = @"\b[vfscimo]\d{1,3}\b";
        //public static string DefIdentifierStyleRegEx = @"(\w|\%|\$|\#|\.|[^\x00-\x7F])+";
        public static string DefIdentifierStyleRegEx = @"(\w|\%|\$|\#|[^\x00-\x7F])(\w|\!|\%|\$|\#|\.|[^\x00-\x7F])+";
        #endregion

        #region Global Enumerations
        //***************************************************
        // ENUMERATIONS
        //***************************************************
        public enum UndoNameID {
            UID_UNKNOWN = 0,
            UID_TYPING = 1,
            UID_DELETE = 2,
            UID_DRAGDROP = 3,
            UID_CUT = 4,
            UID_PASTE = 5,
        }
        public enum ReplaceMode {
            Yes,
            YesToAll,
            No,
            NoToAll,
        }
        public enum GetRes {
            AddNew,
            Renumber,
            Open,
            TestView,
            AddLayout,
            ShowRoom,
            Menu,
            Import,
            MenuBkgd,
            AddInGame,
            RenumberRoom
        }
        public enum GameSettingFunction {
            Edit,
            New,
        }
        public enum ViewEditMode {
            Bitmap,
            View,
            Loop,
            Cel,
        }
        public enum LogicFormMode {
            Logic,
            Text,
        }
        public enum FindDirection {
            All,
            Down,
            Up,
        }
        public enum FindLocation {
            Current,
            Open,
            All,
        }
        public enum FindFormFunction {
            FindWord,
            ReplaceWord,
            FindObject,
            ReplaceObject,
            FindLogic,
            ReplaceLogic,
            FindText,
            ReplaceText,
            FindWordsLogic, //used when searching for words or synonyms
            FindObjsLogic,  //used when searching for inv objects
            FindNone,       // used to temporarily disable find form
                            // when active form is not searchable
        }
        public enum FindFormAction {
            Find,
            Replace,
            ReplaceAll,
            Cancel,
        }
        public enum TPicToolTypeEnum {
            ttEdit = 0,      //indicates edit tool is selected; mouse ops move coords and commands
            ttSetPen = 1,    //not used, but included for possible updated capabilities
            ttLine = 2,      //line drawing tool; mouse ops set start/end points
            ttRelLine = 3,   //short line tool; mouse ops set start/end points
            ttCorner = 4,    //corner line tool; mouse ops set start/end points
            ttFill = 5,      //fill tool; mouse ops set starting point of fill operations
            ttPlot = 6,      //plot tool
            ttRectangle = 7, //for drawing rectangles
            ttTrapezoid = 8, //for drawing trapezoids
            ttEllipse = 9,   //for drawing ellipses
            ttSelectArea = 10, //for selecting bitmap areas of the Image
        }
        public enum TPicDrawOpEnum {
            doNone = 0,          //indicates no drawing op is in progress; mouse operations generally don't do anything
            doLine = 1,          //lines being drawn; mouse ops set start/end points
            doFill = 2,          //fill or plot commands being drawn; mouse ops set starting point of fill operations
            doShape = 3,         //shape being drawn; mouse ops set bounds of the shape
            doSelectArea = 4,    //to select an area of the graphic
            doMoveCmds = 5,      //to move a set of commands
            doMovePt = 6,        //to move a single coordinate point
        }
        public enum EPicMode {
            pmEdit,
            pmTest,
        }
        public enum EPicCur {
            pcEdit,
            pcCross,
            pcMove,
            pcNO,
            pcDefault,
            pcPaint,
            pcBrush,
            pcSelect,
            pcNormal,
            pcEditSel,
        }
        public enum NoteTone {
            None,
            Natural,
            Sharp,
            Flat,
        }
        public enum ELSelection {
            lsNone,
            lsRoom,
            lsExit,
            lsTransPt,
            lsComment,
            lsErrPt,
            lsMultiple,
        }
        public enum ELLeg {
            llNoTrans,   //  0 means no trans pt
            llFirst,     //  1 means first leg of trans pt
            llSecond,    //  2 means second leg of trans pt
        }
        public enum ELTwoWay {
            ltwSingle,    //  0 means a single direction exit
            ltwOneWay,    //  1 means a two way exit but only one way selected
            ltwBothWays,  //  2 means a two way exit and both are considered //selected//
        }
        public enum ELayoutTool {
            ltNone,
            ltSelect,
            ltEdge1,
            ltEdge2,
            ltOther,
            ltRoom,
            ltComment,
        }
        public enum EEReason {
            erNone,      //no exit reason specified (reason not yet assigned)
            erHorizon,   //exit is //if ego=horizon// Type
            erRight,     //exit is //if ego=right// Type
            erBottom,    //exit is //if ego=bottom// Type
            erLeft,      //exit is //if ego=left// Type
            erOther,     //exit can//t be easily determined to be a simple edge exit
        }
        public enum EEStatus {
            esOK,        //exit is drawn in layout editor, and exists in source code correctly
            esNew,       //exit that is drawn in layout editor, but hasn//t been saved to source code yet
            esChanged,   //exit that has been changed in the layout editor, but not updated in source code
            esDeleted,   //exit that has been deleted in layout editor, but not updated in source code
            esHidden,    //exit is valid, but to a logic currently marked as not IsRoom
        }
        public enum UpdateReason {
            AddRoom,         //new room added in layout editor
            ShowRoom,        //existing room toggled to show in layout editor
            RemoveRoom,      //room removed by hiding (IsRoom to false), or actual removal from game
            RenumberRoom,    //room's logic number is changed
            UpdateRoom,      //existing room updated in logic editor
        }
        public enum ArgListType {
            // same as ArgTypeEnum for 0-10; 
            // other values used when building define lists
            None = -2,
            All = -1,
            Byte = 0,
            Var = 1,
            Flag = 2,
            Msg = 3,
            SObj = 4,
            IObj = 5,
            Str = 6,
            Word = 7,
            Ctl = 8,
            DefStr = 9,
            VocWrd = 10,
            IfArg = 11,  //variables and flags
            OthArg = 12, //variables and strings
            Values = 13, //variables and bytes
            Logic = 14,
            Picture = 15,
            Sound = 16,
            View = 17,
        }
        public enum EImgFormat {
            effBMP = 0,
            effGIF = 1,
            effPNG = 2,
            effJPG = 3,
        }

        public enum CompileMode {
            Full,
            RebuildOnly,
            ChangedLogics,
            ChangeVersion,
        }

        public enum AGITokenType {
            None,
            Comment,
            String,
            Number,
            Symbol,
            Identifier,
        }

        public enum TokenSubtype {
            Undefined = -1,
            LogicID,
            PictureID,
            SoundID,
            ViewID,
            LocalDefine,
            GlobalDefine,
            ReservedDefine,
            TestCmd,
            ActionCmd,
            Label,
            Snippet,
        }

        #endregion

        #region Global Structs
        //***************************************************
        // STRUCTS
        //***************************************************
        public struct tDisplayNote {
            public int Pos;
            public NoteTone Tone;
        }
        public struct TPicTest {
            public int ObjSpeed;
            public int ObjPriority;  //16 means auto; 4-15 correspond to priority bands
            public int ObjRestriction;  //0 = no restriction
                                        //1 = restrict to water
                                        //2 = restrict to land
            public int Horizon;
            public bool IgnoreHorizon;
            public bool IgnoreBlocks;
            public int TestLoop;
            public int TestCel;
            public bool CycleAtRest;
        }
        public struct LEObjColor {
            public Color Edge;
            public Color Fill;
        }
        public struct TLEColors {
            public LEObjColor Room;
            public LEObjColor ErrPt;
            public LEObjColor TransPt;
            public LEObjColor Cmt;
            public Color ExitEdge;
            public Color ExitOther;
        }
        public struct GifOptions {
            public int Zoom;
            public bool Transparency;
            public int Delay;
            public bool Cycle;
            public int VAlign;
            public int HAlign;
        }
        public struct SyntaxStyleType {
            public SettingColor Color;
            public SettingFontStyle FontStyle;
        }
        //public enum EPicCursorMode {
        //    pcmWinAGI,
        //    pcmXMode,
        //}

        public struct agiSettings {
            public agiSettings() {
                // initialize settings arrays and defaults
                SyntaxStyle = new SyntaxStyleType[10];
                // DefaultStyle = 0
                SyntaxStyle[0].Color = new("EditorForeColor", Color.White, sSHFORMAT);
                SyntaxStyle[0].FontStyle = new("EditorDefaultFontStyle", FontStyle.Bold, sSHFORMAT);
                // CommentStyle = 1
                SyntaxStyle[1].Color = new("SyntaxColor1", Color.FromArgb(143, 143, 143), sSHFORMAT);
                SyntaxStyle[1].FontStyle = new("SyntaxStyle1", FontStyle.Bold | FontStyle.Italic, sSHFORMAT);
                // StringStyle = 2
                SyntaxStyle[2].Color = new("SyntaxColor2", Color.FromArgb(0, 240, 0), sSHFORMAT);
                SyntaxStyle[2].FontStyle = new("SyntaxStyle2", FontStyle.Regular, sSHFORMAT);
                // KeyWordStyle = 3
                SyntaxStyle[3].Color = new("SyntaxColor3", Color.FromArgb(132, 132, 255), sSHFORMAT);
                SyntaxStyle[3].FontStyle = new("SyntaxStyle3", FontStyle.Bold, sSHFORMAT);
                // TestCmdStyle = 4
                SyntaxStyle[4].Color = new("SyntaxColor4", Color.FromArgb(155, 226, 252), sSHFORMAT);
                SyntaxStyle[4].FontStyle = new("SyntaxStyle4", FontStyle.Bold, sSHFORMAT);
                // ActionCmdStyle = 5
                SyntaxStyle[5].Color = new("SyntaxColor5", Color.FromArgb(132, 132, 255), sSHFORMAT);
                SyntaxStyle[5].FontStyle = new("SyntaxStyle5", FontStyle.Bold, sSHFORMAT);
                // InvalidCmdStyle = 6
                SyntaxStyle[6].Color = new("SyntaxColor6", Color.FromArgb(247, 89, 62), sSHFORMAT);
                SyntaxStyle[6].FontStyle = new("SyntaxStyle6", FontStyle.Bold, sSHFORMAT);
                // NumberStyle = 7
                SyntaxStyle[7].Color = new("SyntaxColor7", Color.White, sSHFORMAT);
                SyntaxStyle[7].FontStyle = new("SyntaxStyle7", FontStyle.Regular, sSHFORMAT);
                // ArgIdentifierStyle = 8
                SyntaxStyle[8].Color = new("SyntaxColor8", Color.FromArgb(244, 165, 0), sSHFORMAT);
                SyntaxStyle[8].FontStyle = new("SyntaxStyle8", FontStyle.Bold, sSHFORMAT);
                // DefinesIdentifierStyle = 9
                SyntaxStyle[9].Color = new("SyntaxColor9", Color.FromArgb(255, 128, 128), sSHFORMAT);
                SyntaxStyle[9].FontStyle = new("SyntaxStyle9", FontStyle.Regular, sSHFORMAT);
                DefInst = new SettingByte[3];
                for (int i = 0; i < 3; i++) {
                    DefInst[i] = new("Instrument" + i, 80, sSOUNDS);
                }
                DefMute = new SettingBool[4];
                for (int i = 0; i < 4; i++) {
                    DefMute[i] = new("Mute" + i, false, sSOUNDS);
                }
            }
            // ************************************************
            // WARNINGS:
            // ************************************************
            // RenameWAG: 
            public SettingAskOption RenameWAG = new(nameof(RenameWAG), AskOption.Ask, "Warnings");
            // OpenNew: opens newly added/imported resources in an editor after being added
            public SettingBool OpenNew = new(nameof(OpenNew), true, "Warnings");
            // AskExport: if not asking, assume no export
            public SettingBool AskExport = new(nameof(AskExport), true, "Warnings");
            // AskRemove: if not asking, assume OK to remove
            public SettingBool AskRemove = new(nameof(AskRemove), true, "Warnings");
            // AutoUpdateDefines: when true logics are updated automatically when defines change
            public SettingAskOption AutoUpdateDefines = new(nameof(AutoUpdateDefines), AskOption.Ask, "Warnings");
            // AutoUpdateResDefs: when true logics are updated automatically when reserved defines change
            public SettingAskOption AutoUpdateResDefs = new(nameof(AutoUpdateResDefs), AskOption.Ask, "Warnings");
            // AutoUpdateWords:
            public SettingAskOption AutoUpdateWords = new(nameof(AutoUpdateWords), AskOption.Ask, sGENERAL);
            // AutoUpdateObjects:
            public SettingAskOption AutoUpdateObjects = new(nameof(AutoUpdateObjects), AskOption.Ask, sGENERAL);
            // WarnDupGName: 
            public SettingAskOption WarnDupGName = new(nameof(WarnDupGName), AskOption.Ask, "Warnings");
            // WarnDupGVal: 
            public SettingBool WarnDupGVal = new(nameof(WarnDupGVal), true, "Warnings");
            // WarnInvalidStrVal: 
            public SettingBool WarnInvalidStrVal = new(nameof(WarnInvalidStrVal), true, "Warnings");
            // WarnInvalidCtlVal: 
            public SettingBool WarnInvalidCtlVal = new(nameof(WarnInvalidCtlVal), true, "Warnings");
            // WarnResOvrd: warn user if attempting to override definition of a reserved var/flag/etc
            public SettingBool WarnResOverride = new(nameof(WarnResOverride), true, "Warnings");
            // WarnDupObj: 
            public SettingBool WarnDupObj = new(nameof(WarnDupObj), true, "Warnings");
            // WarnCompile: when true, a warning is shown when a logic is closed that isn't compiled
            public SettingAskOption WarnCompile = new(nameof(WarnCompile), AskOption.Ask, "Warnings");
            // DelBlankG: 
            public SettingAskOption DelBlankG = new(nameof(DelBlankG), AskOption.Ask, "Warnings");
            // NotifyCompSuccess: when true, a message is shown after logic is compiled
            public SettingBool NotifyCompSuccess = new(nameof(NotifyCompSuccess), true, "Warnings");
            // TODO: maybe delete this warning- it doesn't add much
            //// NotifyCompWarn: 
            //public SettingBool NotifyCompWarn = new(nameof(NotifyCompWarn), true, "Warnings");
            // NotifyCompFail: 
            public SettingBool NotifyCompFail = new(nameof(NotifyCompFail), true, "Warnings");
            // WarnItem0: give warning if item 0 is edited
            public SettingBool WarnItem0 = new(nameof(WarnItem0), true, "Warnings");
            // OpenOnErr: 
            public SettingAskOption OpenOnErr = new(nameof(OpenOnErr), AskOption.Ask, "Warnings");
            // SaveOnCompile: 
            public SettingAskOption SaveOnCompile = new(nameof(SaveOnCompile), AskOption.Ask, "Warnings");
            // CompileOnRun: 
            public SettingAskOption CompileOnRun = new(nameof(CompileOnRun), AskOption.Ask, "Warnings");
            // WarnMsgs: determines how to handle message cleanup warnings (0 = ask; 1 = keep all; 2 = keep only used)
            public SettingInt WarnMsgs = new(nameof(WarnMsgs), 0, "Warnings");
            // WarnEncrypt: 
            public SettingBool WarnEncrypt = new(nameof(WarnEncrypt), true, "Warnings");
            // LEDelPicToo: 
            public SettingAskOption LEDelPicToo = new(nameof(LEDelPicToo), AskOption.Ask, "Warnings");

            // ************************************************
            // GENERAL SETTINGS:
            // ************************************************
            // ShowSplashScreen: when true, splash screen is shown at startup
            public SettingBool ShowSplashScreen = new(nameof(ShowSplashScreen), true, sGENERAL);
            // ShowPreview:  when true, use preview window
            public SettingBool ShowPreview = new(nameof(ShowPreview), true, sGENERAL);
            // ShiftPreview: brings preview window to front when something selected
            public SettingBool ShiftPreview = new(nameof(ShiftPreview), true, sGENERAL);
            // HidePreview: hides preview window when other form has focus
            public SettingBool HidePreview = new(nameof(HidePreview), false, sGENERAL);
            // ResListType: determines the style of resource list
            public SettingEResListType ResListType = new(nameof(ResListType), EResListType.TreeList, sGENERAL);
            // AutoExport:
            public SettingBool AutoExport = new(nameof(AutoExport), true, sGENERAL);
            // RenameDelRes: 
            public SettingBool BackupResFile = new(nameof(BackupResFile), true, sGENERAL);
            // DefMaxSO: 
            public SettingByte DefMaxSO = new(nameof(DefMaxSO), 16, sGENERAL);
            // DefMaxVol0: 
            public SettingInt DefMaxVol0 = new(nameof(DefMaxVol0), 1047552, sGENERAL);
            // DefCP: the default codepage that handles display of extended characters
            public SettingInt DefCP = new(nameof(DefCP), 437, sGENERAL);
            // DefResDir: 
            public SettingString DefResDir = new(nameof(DefResDir), "src", sGENERAL);

            // ************************************************
            // MRU MENU OPTIONS
            // ************************************************
            // AutoOpen: when true, the game file from last session is opened whwn WinAGI starts
            public SettingBool AutoOpen = new(nameof(AutoOpen), true, "MRUList");

            // ************************************************
            // RESOURCE TREE LABEL FORMAT
            // ************************************************
            // ShowResNum: 
            public SettingBool ShowResNum = new(nameof(ShowResNum), false, sRESFMT);
            // IncludeResNum: 
            public SettingBool IncludeResNum = new(nameof(IncludeResNum), true, sRESFMT);
            // ResFormatNameCase: used to format resource names when displayed in the resource list (0=lower, 1=upper, 2=proper)
            public SettingInt ResFormatNameCase = new("NameCase", 2, sRESFMT);
            // ResFormatSeparator: used to format resource names when displayed in the resource list
            public SettingString ResFormatSeparator = new("Separator", ".", sRESFMT);
            // ResFormatNumFormat: used to format resource names when displayed in the resource
            public SettingString ResFormatNumFormat = new("NumFormat", "000", sRESFMT);

            // ************************************************
            // LOGIC SETTINGS
            // ************************************************
            // AutoWarn: 
            public SettingBool AutoWarn = new(nameof(AutoWarn), true, sLOGICS);
            // HighlightLogic:
            public SettingBool HighlightLogic = new(nameof(HighlightLogic), true, sLOGICS);
            // HighlightText: 
            public SettingBool HighlightText = new(nameof(HighlightText), false, sLOGICS);
            // LogicTabWidth
            public SettingInt LogicTabWidth = new(nameof(LogicTabWidth), 4, sLOGICS);
            // MaximizeLogics: 
            public SettingBool MaximizeLogics = new(nameof(MaximizeLogics), true, sLOGICS);
            // AutoQuickInfo: 
            public SettingBool AutoQuickInfo = new(nameof(AutoQuickInfo), true, sLOGICS);
            // ShowDefTips: 
            public SettingBool ShowDefTips = new(nameof(ShowDefTips), true, sLOGICS);
            // ShowDocMap: 
            public SettingBool ShowDocMap = new(nameof(ShowDocMap), true, sLOGICS);
            // ShowLineNumbers: 
            public SettingBool ShowLineNumbers = new(nameof(ShowLineNumbers), true, sLOGICS);
            // DefaultExt: 
            public SettingString DefaultExt = new(nameof(DefaultExt), "lgc", sLOGICS);
            // EditorFontName: 
            public SettingString EditorFontName = new(nameof(EditorFontName), "Consolas", sLOGICS);
            // EditorFontSize: 
            public SettingInt EditorFontSize = new(nameof(EditorFontSize), 14, sLOGICS);
            // PreviewFontName: 
            public SettingString PreviewFontName = new(nameof(PreviewFontName), "Consolas", sLOGICS);
            // PreviewFontSize: 
            public SettingInt PreviewFontSize = new(nameof(PreviewFontSize), 12, sLOGICS);
            // ErrorLevel: 
            public SettingLogicErrorLevel ErrorLevel = new(nameof(ErrorLevel), LogicErrorLevel.Medium, sLOGICS);
            // DefIncludeIDs: default value for IncludeIDs property
            public SettingBool DefIncludeIDs = new(nameof(DefIncludeIDs), true, sLOGICS);
            // DefIncludeReserved: default value for IncludeReserved property
            public SettingBool DefIncludeReserved = new(nameof(DefIncludeReserved), true, sLOGICS);
            // DefIncludeGlobals: default value for IncludeGlobals property
            public SettingBool DefIncludeGlobals = new(nameof(DefIncludeGlobals), true, sLOGICS);
            // UseSnippets: 
            public SettingBool UseSnippets = new(nameof(UseSnippets), true, sLOGICS);

            // ************************************************
            // SYNTAX HIGHLIGHT SETTINGS
            // ************************************************
            // EditorBackColor: 
            public SettingColor EditorBackColor = new(nameof(EditorBackColor), Color.FromArgb(63, 63, 63), sSHFORMAT);
            // SyntaxStyle[]: color and font styles for syntax highlighting
            public SyntaxStyleType[] SyntaxStyle; // initialized in constructor

            // ************************************************
            // LOGIC DECOMPILER SETTINGS
            // ************************************************
            // MsgsByNumber: 
            public SettingBool MsgsByNumber = new(nameof(MsgsByNumber), false, sDECOMPILER);
            // IObjsByNumber: 
            public SettingBool IObjsByNumber = new(nameof(IObjsByNumber), false, sDECOMPILER);
            // WordsByNumber: 
            public SettingBool WordsByNumber = new(nameof(WordsByNumber), false, sDECOMPILER);
            // ShowAllMessages:
            public SettingBool ShowAllMessages = new(nameof(ShowAllMessages), true, sDECOMPILER);
            // SpecialSyntax: 
            public SettingBool SpecialSyntax = new(nameof(SpecialSyntax), true, sDECOMPILER);
            // ReservedAsText:
            public SettingBool ReservedAsText = new(nameof(ReservedAsText), true, sDECOMPILER);
            // CodeStyle:
            public SettingAGICodeStyle CodeStyle = new(nameof(CodeStyle), LogicDecoder.AGICodeStyle.cstDefaultStyle, sDECOMPILER);

            // ************************************************
            // PICTURE SETTINGS
            // ************************************************
            // ShowBands: 
            public SettingBool ShowBands = new(nameof(ShowBands), true, sPICTURES);
            // SplitWindow: 
            public SettingBool SplitWindow = new(nameof(SplitWindow), true, sPICTURES);
            // PicScalePreview: 
            public SettingInt PicScalePreview = new("PreviewScale", 1, sPICTURES);
            // PicScaleEdit: 
            public SettingInt PicScaleEdit = new("EditorScale", 2, sPICTURES);
            //// CursorMode: 
            //public EPicCursorMode CursorMode;

            // ************************************************
            // PICTURETEST SETTINGS
            // ************************************************
            // PicTest.ObjSpeed: 
            public SettingInt PTObjSpeed = new("Speed", 1, sPICTEST);
            // PicTest.ObjPriority: 16 means auto; 4-15 correspond to priority bands
            public SettingInt PTObjPriority = new("Priority", 16, sPICTEST);
            // PicTest.ObjRestriction: 0 = no restriction, 1 = restrict to water, 2 = restrict to land
            public SettingInt PTObjRestriction = new("Restriction", 0, sPICTEST);
            // PicTest.Horizon:
            public SettingInt PTHorizon = new("Horizon", 36, sPICTEST);
            // PicTest.IgnoreHorizon
            public SettingBool PTIgnoreHorizon = new("IgnoreHorizon", false, sPICTEST);
            // PicTest.IgnoreBlocks
            public SettingBool PTIgnoreBlocks = new("IgnoreBlocks", false, sPICTEST);
            // PicTest.CycleAtRest
            public SettingBool PTCycleAtRest = new("CycleAtRest", false, sPICTEST);

            // ************************************************
            // SOUND SETTINGS
            // ************************************************
            // ShowKybd
            public SettingBool ShowKeyboard = new(nameof(ShowKeyboard), true, sSOUNDS);
            // ShowNotes
            public SettingBool ShowNotes = new(nameof(ShowNotes), true, sSOUNDS);
            // OneTrack
            public SettingBool OneTrack = new(nameof(OneTrack), false, sSOUNDS);
            // DefInst: 
            public SettingByte[] DefInst; // initialized in constructor
            // DefMute: 
            public SettingBool[] DefMute; // initialized in constructor
            // SndZoom:
            public SettingInt SndZoom = new(nameof(SndZoom), 2, sSOUNDS);

            // ************************************************
            // VIEW SETTINGS
            // ************************************************
            // ShowVEPrev: 
            public SettingBool ShowVEPreview = new("ShowPreview", true, sVIEWS);
            // DefPrevPlay: 
            public SettingBool DefPrevPlay = new(nameof(DefPrevPlay), true, sVIEWS);
            // ShowGrid: 
            public SettingBool ShowGrid = new(nameof(ShowGrid), true, sVIEWS);
            // ViewAlignH: 
            public SettingInt ViewAlignH = new(nameof(ViewAlignH), 0, sVIEWS);
            // ViewAlignV: 
            public SettingInt ViewAlignV = new(nameof(ViewAlignV), 0, sVIEWS);
            // DefCelH: 
            public SettingByte DefCelH = new(nameof(DefCelH), 32, sVIEWS);
            // DefCelW: 
            public SettingByte DefCelW = new(nameof(DefCelW), 16, sVIEWS);
            // ViewScalePreview: 
            public SettingInt ViewScalePreview = new("PreviewScale", 3, sVIEWS);
            // ViewScaleEdit: 
            public SettingInt ViewScaleEdit = new("EditScale", 6, sVIEWS);
            // DefVColor1: 
            public SettingInt DefVColor1 = new("Color1", 0, sVIEWS);
            // DefVColor2: 
            public SettingInt DefVColor2 = new("Color2", 15, sVIEWS);

            // ************************************************
            // OBJECT SETTINGS
            // ************************************************
            // none

            // ************************************************
            // WORDS.TOK SETTINGS
            // ************************************************
            // none

            // ************************************************
            // LAYOUT EDITOR SETTINGS
            // ************************************************
            // DefUseLE: default value for new games
            public SettingBool DefUseLE = new(nameof(DefUseLE), true, sLAYOUT);
            // LEPages:
            public SettingBool LEPages = new("PageBoundaries", true, sLAYOUT);
            // LEShowPics: false=no pics on rooms when drawn
            public SettingBool LEShowPics = new("ShowPics", true, sLAYOUT);
            // LEUseGrid: 
            public SettingBool LEUseGrid = new("UseGrid", true, sLAYOUT);
            // LEGrid: 
            public SettingDouble LEGrid = new("GridSize", 0.1, sLAYOUT);
            // LESync: 
            public SettingBool LESync = new("Sync", true, sLAYOUT);
            // LEZoom: 
            public SettingInt LEZoom = new("Zoom", 6, sLAYOUT);
            // RoomEdgeColor: 
            public SettingColor RoomEdgeColor = new SettingColor(nameof(RoomEdgeColor), Color.FromArgb(0, 0x55, 0xAA), sLAYOUT);
            // RoomFillColor:
            public SettingColor RoomFillColor = new SettingColor(nameof(RoomFillColor), Color.FromArgb(0x55, 0xFF, 0xFF), sLAYOUT);
            // TransPtEdgeColor: 
            public SettingColor TransPtEdgeColor = new SettingColor(nameof(TransPtEdgeColor), Color.FromArgb(0x62, 0x62, 0), sLAYOUT);
            // TransPtFillColor: 
            public SettingColor TransPtFillColor = new SettingColor(nameof(TransPtFillColor), Color.FromArgb(0xFF, 0xFF, 0x91), sLAYOUT);
            // CmtEdgeColor: 
            public SettingColor CmtEdgeColor = new SettingColor(nameof(CmtEdgeColor), Color.FromArgb(0, 0x62, 0), sLAYOUT);
            // CmtFillColor: 
            public SettingColor CmtFillColor = new SettingColor(nameof(CmtFillColor), Color.FromArgb(0x91, 0xFF, 0x91), sLAYOUT);
            // ErrPtEdgeColor: 
            public SettingColor ErrPtEdgeColor = new SettingColor(nameof(ErrPtEdgeColor), Color.FromArgb(0, 0, 0x62), sLAYOUT);
            // ErrPtFillColor: 
            public SettingColor ErrPtFillColor = new SettingColor(nameof(ErrPtFillColor), Color.FromArgb(0x91, 0x91, 0xFF), sLAYOUT);
            // ExitEdgeColor: 
            public SettingColor ExitEdgeColor = new SettingColor(nameof(ExitEdgeColor), Color.FromArgb(0xA0, 0, 0), sLAYOUT);
            // ExitOtherColor: 
            public SettingColor ExitOtherColor = new SettingColor(nameof(ExitOtherColor), Color.FromArgb(0xFF, 0x55, 0xFF), sLAYOUT);


            // ************************************************
            // GLOBALS EDITOR SETTINGS
            // ************************************************
            // GEShowComment: 
            public SettingBool GEShowComment = new(nameof(GEShowComment), true, "Globals");
            // GENameFrac: 
            public SettingDouble GENameFrac = new(nameof(GENameFrac), 0, "Globals");
            // GEValFrac: 
            public SettingDouble GEValFrac = new(nameof(GEValFrac), 0, "Globals");


            // ************************************************
            // MENU EDITOR SETTINGS (store in GENERAL)
            // ************************************************
            // AutoAlignHotKey: 
            public SettingBool AutoAlignHotKey = new(nameof(AutoAlignHotKey), true, "MenuEditor");


            // ************************************************
            // PLATFORM SETTINGS
            // ************************************************
            // AutoFill:
            public SettingBool AutoFill = new(nameof(AutoFill), false, "Platform");
            // PlatformType: 
            public SettingInt PlatformType = new(nameof(PlatformType), 0, "Platform");
            // PlatformFile: 
            public SettingString PlatformFile = new(nameof(PlatformFile), "", "Platform");
            // DOSExec;
            public SettingString DOSExec = new(nameof(DOSExec), "", "Platform");
            // PlatformOpts:
            public SettingString PlatformOpts = new(nameof(PlatformOpts), "", "Platform");

            internal agiSettings Clone() {
                agiSettings clonesettings = new();
                // WARNINGS
                clonesettings.RenameWAG = new(RenameWAG);
                clonesettings.OpenNew = new(OpenNew);
                clonesettings.AskExport = new(AutoExport);
                clonesettings.AskRemove = new(AutoExport);
                clonesettings.AutoUpdateDefines = new(AutoUpdateResDefs);
                clonesettings.AutoUpdateResDefs = new(AutoUpdateResDefs);
                clonesettings.AutoUpdateWords = new(AutoUpdateWords);
                clonesettings.AutoUpdateObjects = new(AutoUpdateObjects);
                clonesettings.WarnDupGName = new(WarnDupGName);
                clonesettings.WarnDupGVal = new(WarnDupGVal);
                clonesettings.WarnInvalidStrVal = new(WarnInvalidStrVal);
                clonesettings.WarnInvalidCtlVal = new(WarnInvalidCtlVal);
                clonesettings.WarnResOverride = new(WarnResOverride);
                clonesettings.WarnDupObj = new(WarnDupObj);
                clonesettings.WarnCompile = new(WarnCompile);
                clonesettings.DelBlankG = new(DelBlankG);
                clonesettings.NotifyCompSuccess = new(NotifyCompSuccess);
                //clonesettings.NotifyCompWarn = new(NotifyCompWarn);
                clonesettings.NotifyCompFail = new(NotifyCompFail);
                clonesettings.WarnItem0 = new(WarnItem0);
                clonesettings.OpenOnErr = new(OpenOnErr);
                clonesettings.SaveOnCompile = new(SaveOnCompile);
                clonesettings.CompileOnRun = new(CompileOnRun);
                clonesettings.WarnMsgs = new(WarnMsgs);
                clonesettings.WarnEncrypt = new(WarnEncrypt);
                clonesettings.LEDelPicToo = new(LEDelPicToo);
                // GENERAL SETTINGS
                clonesettings.ShowSplashScreen = new(ShowSplashScreen);
                clonesettings.ShowPreview = new(ShowPreview);
                clonesettings.ShiftPreview = new(ShiftPreview);
                clonesettings.HidePreview = new(HidePreview);
                clonesettings.ResListType = new(ResListType);
                clonesettings.AutoExport = new(AutoExport);
                clonesettings.BackupResFile = new(BackupResFile);
                clonesettings.DefMaxSO = new(DefMaxSO);
                clonesettings.DefMaxVol0 = new(DefMaxVol0);
                clonesettings.DefCP = new(DefCP);
                clonesettings.DefResDir = new(DefResDir);
                // MRU MENU LIST
                clonesettings.AutoOpen = new(AutoOpen);
                // RESOURCE TREE LABEL
                clonesettings.ShowResNum = new(ShowResNum);
                clonesettings.IncludeResNum = new(IncludeResNum);
                clonesettings.ResFormatNameCase = new(ResFormatNameCase);
                clonesettings.ResFormatSeparator = new(ResFormatSeparator);
                clonesettings.ResFormatNumFormat = new(ResFormatNumFormat);
                // LOGICS
                clonesettings.AutoWarn = new(AutoWarn);
                clonesettings.HighlightLogic = new(HighlightLogic);
                clonesettings.HighlightText = new(HighlightText);
                clonesettings.LogicTabWidth = new(LogicTabWidth);
                clonesettings.MaximizeLogics = new(MaximizeLogics);
                clonesettings.AutoQuickInfo = new(AutoQuickInfo);
                clonesettings.ShowDefTips = new(ShowDefTips);
                clonesettings.ShowDocMap = new(ShowDocMap);
                clonesettings.ShowLineNumbers = new(ShowLineNumbers);
                clonesettings.DefaultExt = new(DefaultExt);
                clonesettings.EditorFontName = new(EditorFontName);
                clonesettings.EditorFontSize = new(EditorFontSize);
                clonesettings.PreviewFontName = new(PreviewFontName);
                clonesettings.PreviewFontSize = new(PreviewFontSize);
                clonesettings.ErrorLevel = new(ErrorLevel);
                clonesettings.DefIncludeReserved = new(DefIncludeReserved);
                clonesettings.UseSnippets = new(UseSnippets);
                // SYNTAX HIGHLIGHTING
                clonesettings.EditorBackColor = new(EditorBackColor);
                for (int i = 0; i < 10; i++) {
                    clonesettings.SyntaxStyle[i].Color = new(SyntaxStyle[i].Color);
                    clonesettings.SyntaxStyle[i].FontStyle = new(SyntaxStyle[i].FontStyle);
                }
                // LOGIC DECOMPILER
                clonesettings.MsgsByNumber = new(MsgsByNumber);
                clonesettings.IObjsByNumber = new(IObjsByNumber);
                clonesettings.WordsByNumber = new(WordsByNumber);
                clonesettings.ShowAllMessages = new(ShowAllMessages);
                clonesettings.SpecialSyntax = new(SpecialSyntax);
                clonesettings.ReservedAsText = new(ReservedAsText);
                clonesettings.CodeStyle = new(CodeStyle);
                // PICTURES
                clonesettings.ShowBands = new(ShowBands);
                clonesettings.SplitWindow = new(SplitWindow);
                clonesettings.PicScalePreview = new(PicScalePreview);
                clonesettings.PicScaleEdit = new(PicScaleEdit);
                //clonesettings.CursorMode = new(CursorMode);
                // PICTEST 
                clonesettings.PTObjSpeed = new(PTObjSpeed);
                clonesettings.PTObjPriority = new(PTObjPriority);
                clonesettings.PTObjRestriction = new(PTObjRestriction);
                clonesettings.PTHorizon = new(PTHorizon);
                clonesettings.PTIgnoreHorizon = new(PTIgnoreHorizon);
                clonesettings.PTIgnoreBlocks = new(PTIgnoreBlocks);
                clonesettings.PTCycleAtRest = new(PTCycleAtRest);
                // SOUNDS
                clonesettings.ShowKeyboard = new(ShowKeyboard);
                clonesettings.ShowNotes = new(ShowNotes);
                clonesettings.OneTrack = new(OneTrack);
                for (int i = 0; i < 3; i++) {
                    clonesettings.DefInst[i] = new(DefInst[i]);
                    clonesettings.DefMute[i] = new(DefMute[i]);
                }
                clonesettings.DefMute[3] = new(DefMute[3]);
                clonesettings.SndZoom = new(SndZoom);
                // VIEWS
                clonesettings.ShowVEPreview = new(ShowVEPreview);
                clonesettings.DefPrevPlay = new(DefPrevPlay);
                clonesettings.ShowGrid = new(ShowGrid);
                clonesettings.ViewAlignH = new(ViewAlignH);
                clonesettings.ViewAlignV = new(ViewAlignV);
                clonesettings.DefCelH = new(DefCelH);
                clonesettings.DefCelW = new(DefCelW);
                clonesettings.ViewScalePreview = new(ViewScalePreview);
                clonesettings.ViewScaleEdit = new(ViewScaleEdit);
                clonesettings.DefVColor1 = new(DefVColor1);
                clonesettings.DefVColor2 = new(DefVColor2);
                // LAYOUT EDITOR
                clonesettings.DefUseLE = new(DefUseLE);
                clonesettings.LEPages = new(LEPages);
                clonesettings.LEShowPics = new(LEShowPics);
                clonesettings.LEUseGrid = new(LEUseGrid);
                clonesettings.LEGrid = new(LEGrid);
                clonesettings.LESync = new(LESync);
                clonesettings.LEZoom = new(LEZoom);
                clonesettings.RoomEdgeColor = new(RoomEdgeColor);
                clonesettings.RoomFillColor = new(RoomFillColor);
                clonesettings.TransPtEdgeColor = new(TransPtEdgeColor);
                clonesettings.TransPtFillColor = new(TransPtFillColor);
                clonesettings.CmtEdgeColor = new(CmtEdgeColor);
                clonesettings.CmtFillColor = new(CmtFillColor);
                clonesettings.ErrPtEdgeColor = new(ErrPtEdgeColor);
                clonesettings.ErrPtFillColor = new(ErrPtFillColor);
                clonesettings.ExitEdgeColor = new(ExitEdgeColor);
                clonesettings.ExitOtherColor = new(ExitOtherColor);
                // GLOBALS EDITOR
                clonesettings.GEShowComment = new(GEShowComment);
                clonesettings.GENameFrac = new(GENameFrac);
                clonesettings.GEValFrac = new(GEValFrac);
                // MENU EDITOR
                clonesettings.AutoAlignHotKey = new(AutoAlignHotKey);
                // PLATFORM
                clonesettings.AutoFill = new(AutoFill);
                clonesettings.PlatformType = new(PlatformType);
                clonesettings.PlatformFile = new(PlatformFile);
                clonesettings.DOSExec = new(DOSExec);
                clonesettings.PlatformOpts = new(PlatformOpts);
                return clonesettings;
            }
        }

        public struct LCoord {
            public double X;
            public double Y;
        }

        public struct PT {
            public byte X;
            public byte Y;
        }

        public struct NewGameResults {
            public string NewID;
            public string Version;
            public string GameDir;
            public string ResDir;
            public string SrcExt;
            public string TemplateDir;
            public bool Failed;
            public string ErrorMsg;
            public bool Warnings;
        }

        public struct LoadGameResults {
            public int Mode;
            public string Source;
            public bool Failed;
            public string ErrorMsg;
            public bool Warnings;
        }

        public struct CompileGameResults {
            public CompileMode Mode;
            public bool Warnings;
            public CompileStatus Status;
            public Exception CompExc;
            public string parm;
        }

        public struct AGIToken {
            public AGITokenType Type;
            public int Line;
            public int StartPos;
            public int EndPos;
            public string Text = "";
            public TokenSubtype SubType = TokenSubtype.Undefined;
            public int Number = -1;
            public string[] ArgList = [];
            public int ArgIndex = -1;
            public AGIToken() {
            }
            public readonly Place Start => new(StartPos, Line);
            public readonly Place End => new(StartPos + Text.Length, Line);
        }

        public struct Snippet {
            public string Name = "";
            public string Value = "";
            public string ArgTips = "";
            public Snippet() {
            }
            public readonly override string ToString() {
                return Name;
            }
        }

        #endregion

        #region Global Variables
        //***************************************************
        // GLOBAL VARIABLES
        //***************************************************
        public static AGIGame EditGame;

        public static frmMDIMain MDIMain;
        public static string ProgramDir;
        public static string DefaultResDir; // location to start searches for resources
        public static string BrowserStartDir = "";  // location to start searches for game files
        public static NewGameResults NewResults;
        public static LoadGameResults LoadResults;
        public static CompileGameResults CompGameResults;
        public static AGIResType CurResType;
        public static agiSettings WinAGISettings = new();
        public static SettingsFile WinAGISettingsFile;
        public static frmPreview PreviewWin;
        public static frmProgress ProgressWin;
        public static frmCompStatus CompStatusWin;
        public static StatusStrip MainStatusBar;
        public static TreeNode RootNode;
        public static TreeNode[] HdrNode;
        public static int SelResNum = -1;
        public static AGIResType SelResType;
        public static bool Compiling;
        public static string WinAGIHelp;
        public static Color PrevWinBColor; //background color for preview window when showing views
        //navigation queue
        public static int[] ResQueue;
        public static int ResQPtr = -1;
        //public static Stack<int> ResQueue = new Stack<int>();
        public static bool DontQueue;
        //editor variables
        public static List<frmLogicEdit> LogicEditors;
        public static int LogCount;
        public static List<frmPicEdit> PictureEditors;
        public static int PicCount;
        public static List<frmSoundEdit> SoundEditors;
        public static int SoundCount;
        static public List<frmViewEdit> ViewEditors;
        public static int ViewCount;
        static public frmLayout LayoutEditor;
        public static bool LEInUse;
        static public frmMenuEdit MenuEditor;
        public static bool MEInUse;
        static public frmObjectEdit ObjectEditor;
        public static bool OEInUse = false;
        public static bool DragObject = false;
        public static int ObjCount;
        public static frmWordsEdit WordEditor;
        public static bool WEInUse = false;
        public static bool DragWord = false;
        public static int WrdCount;
        public static WordsUndo WordsClipboard = new();
        public static frmGlobals GlobalsEditor;
        public static bool GEInUse;
        public static int TextCount;
        //lookup lists for logic editor
        //tooltips and define lists
        public static TDefine[,] IDefLookup = new TDefine[4, 256];
        //  //for now we will not do lookups
        //  // on words and invObjects
        //  // if performance is good enough
        //  // I might consider adding them
        //  public static TDefine[] // ODefLookup()
        //  public static TDefine[] // WDefLookup()
        public static Snippet[] CodeSnippets;
        public static frmSnippets SnippetForm;
        public static int SnipMode; //0=create, 1=manage
        public static GifOptions DefaultVGOptions;
        public static int lngMainTopBorder;
        public static int lngMainLeftBorder;
        public static bool DefUpdateVal;
        //mru variables
        public static string[] strMRU = ["", "", "", ""];
        //clipboard variables
        public static Notes SoundClipboard;
        public static int SoundCBMode;
        public static Loop ClipViewLoop;
        public static Cel ClipViewCel;
        //public static PictureUndo PicClipBoardObj;
        public static ViewEditMode ViewCBMode;
        //public static PictureBox ViewClipboard;
        //public static WordsUndo WordsClipboard;
        public static bool DroppingWord;
        public static bool DroppingObj;
        public static bool DroppingGlobal;
        //default colors
        public static Color[] DefEGAColor = new Color[16];

        //find/replace variables
        // FindForm is the actual dialog used to set search parameters
        public static frmFind FindingForm;
        //global copy of search parameters used by the find form
        public static string GFindText = "";
        public static string GReplaceText = "";
        public static FindDirection GFindDir = FindDirection.All;
        public static bool GMatchWord = false;
        public static bool GMatchCase = false;
        public static FindLocation GLogFindLoc = FindLocation.Current;
        public static bool GFindSynonym = false;
        public static int GFindGrpNum;
        public static int SearchStartPos;
        public static int SearchStartLog;
        public static AGIResType SearchType;
        public static int ObjStartPos;
        public static int StartWord;
        public static int StartGrp;
        public static bool FoundOnce;

        public static bool RestartSearch;
        public static bool ClosedLogics;
        public static int ReplaceCount;
        public static bool SearchStartDlg; // true if search started by clicking 'find' or 'find next'
                                           // on FindingForm
        private static int LogNum = -1; // last logic number found when searching closed logics 
        private static int[] LogWin = new int[0];

        //others
        public static Form HelpParent = null;
        public static string TempFileDir;
        //workaround to force selection pt to update
        //after showing find form...
        public static bool FixSel;
        //keep track of the global window position
        public static double GWHeight, GWWidth;
        public static double GWLeft, GWTop;
        public static int GWState;
        public static bool GWShowComment;
        public static double GWNameFrac, GWValFrac;
        //// graphics variables
        //public static bool NoGDIPlus = false;

        #endregion

        //***************************************************
        // GLOBAL STATIC FUNCTIONS
        //***************************************************

        public static void AddToQueue(AGIResType ResType, int ResNum) {
            //adds this resource to the navigation queue
            // ResNum is 256 for non-collection types (game, objects, words)
            //
            // if currently displaying something by navigating the queue
            // don't add

            int lngRes;

            if (DontQueue) {
                return;
            }
            // save type/number as combinted 32bit word
            lngRes = ((int)ResType << 16) + ResNum;
            if (ResQPtr >= 0) {
                //don't add if the current resource matches
                if (ResQueue[ResQPtr] == lngRes) {
                    return;
                }
            }
            //add the res info
            ResQPtr++;
            Array.Resize(ref ResQueue, ResQPtr + 1);
            ResQueue[ResQPtr] = lngRes;

        }

        public static void ResetQueue() {
            ResQueue = [];
            ResQPtr = -1;
            MDIMain.cmdBack.Enabled = false;
            MDIMain.cmdForward.Enabled = false;
        }

        public static void InitializeResMan() {
            string defaulteditorfont;
            string defaultprevfontname;

            // set default fonts - priority is consolas, courier new, arial, then times new roman
            if (IsFontInstalled("Consolas")) {
                defaultprevfontname = "Consolas";
            }
            else if (IsFontInstalled("Courier New")) {
                defaultprevfontname = "Courier New";
            }
            else if (IsFontInstalled("Arial")) {
                defaultprevfontname = "Arial";
            }
            else {
                //use first font in list
                defaultprevfontname = System.Drawing.FontFamily.Families[0].Name;
            }
            defaulteditorfont = defaultprevfontname;
            WinAGISettings.EditorFontName.DefaultValue = defaulteditorfont;
            WinAGISettings.EditorFontName.Value = defaulteditorfont;
            WinAGISettings.PreviewFontName.Value = defaultprevfontname;
            WinAGISettings.ResListType.SaveText = true;

            // default gif options
            DefaultVGOptions.Cycle = true;
            DefaultVGOptions.Transparency = true;
            DefaultVGOptions.Zoom = 2;
            DefaultVGOptions.Delay = 15;
            DefaultVGOptions.HAlign = 0;
            DefaultVGOptions.VAlign = 1;
            // default value for updating logics is 'checked'
            DefUpdateVal = true;
            // initialize code snippet array
            CodeSnippets = [];
            // set up background worker to create new games
            bgwNewGame = new BackgroundWorker();
            bgwNewGame.DoWork += new DoWorkEventHandler(NewGameDoWork);
            bgwNewGame.ProgressChanged += new ProgressChangedEventHandler(NewGameProgressChanged);
            bgwNewGame.RunWorkerCompleted += new RunWorkerCompletedEventHandler(NewGameWorkerCompleted);
            bgwNewGame.WorkerReportsProgress = true;
            // set up the background worker to open games
            bgwOpenGame = new BackgroundWorker();
            bgwOpenGame.DoWork += new DoWorkEventHandler(OpenGameDoWork);
            bgwOpenGame.ProgressChanged += new ProgressChangedEventHandler(OpenGameProgressChanged);
            bgwOpenGame.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OpenGameWorkerCompleted);
            bgwOpenGame.WorkerReportsProgress = true;
            // set up game compiler background worker
            bgwCompGame = new BackgroundWorker();
            bgwCompGame.DoWork += new DoWorkEventHandler(CompileGameDoWork);
            bgwCompGame.ProgressChanged += new ProgressChangedEventHandler(CompileGameProgressChanged);
            bgwCompGame.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompileGameWorkerCompleted);
            bgwCompGame.WorkerReportsProgress = true;
        }

        public static void ExportLoop(Engine.View view, int loopnum) {
            // export a loop as a gif

            using frmExportViewLoopOptions frmVGO = new(view, loopnum);
            if (frmVGO.ShowDialog(MDIMain) == DialogResult.Cancel) {
                return;
            }
            GifOptions options = frmVGO.SelectedGifOptions;

            MDIMain.SaveDlg.Title = "Export Loop GIF";
            MDIMain.SaveDlg.DefaultExt = "gif";
            MDIMain.SaveDlg.Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*";
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.AddExtension = true;
            MDIMain.SaveDlg.DefaultExt = "gif";
            MDIMain.SaveDlg.FilterIndex = 1;
            MDIMain.SaveDlg.FileName = "";
            MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            MDIMain.SaveDlg.OkRequiresInteraction = true;
            MDIMain.SaveDlg.OverwritePrompt = true;
            if (MDIMain.SaveDlg.ShowDialog() == DialogResult.Cancel) {
                return;
            }
            DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
            ProgressWin = new () {
                Text = "Exporting Loop as GIF"
            };
            ProgressWin.lblProgress.Text = "Depending on size of loop, this may take awhile. Please wait...";
            ProgressWin.pgbStatus.Visible = false;
            ProgressWin.Show(MDIMain);
            MDIMain.UseWaitCursor = true;
            MakeLoopGif(view[loopnum], options, MDIMain.SaveDlg.FileName);
            ProgressWin.Close();
            ProgressWin.Dispose();
            MessageBox.Show(MDIMain, "Success!", "Export Loop as GIF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MDIMain.UseWaitCursor = false;
        }

        public static bool MakeLoopGif(Loop GifLoop, GifOptions GifOps, string ExportFile) {
            string strTempFile;
            int lngPos; //data that will be written to the gif file
            int lngInPos;
            byte[] bytCmpData, bytCelData; //data used to build then compress cel data as gif Image
            short i, j;
            short MaxH = 0, MaxW = 0;
            byte hVal, wVal;
            byte hPad, wPad;
            byte pX, pY;
            short zFacH, zFacW;
            int lngCelPos;
            byte bytTrans;
            byte celH, celW;
            short intChunkSize;
            // build header
            byte[] bytData = new byte[255];
            bytData[0] = 71;
            bytData[1] = 73;
            bytData[2] = 70;
            bytData[3] = 56;
            bytData[4] = 57;
            bytData[5] = 97;
            //determine size of logical screen by checking size of each cel in loop, and using Max of h/w
            for (i = 0; i < GifLoop.Cels.Count; i++) {
                if (GifLoop.Cels[i].Height > MaxH) {
                    MaxH = GifLoop.Cels[i].Height;
                }
                if (GifLoop.Cels[i].Width > MaxW) {
                    MaxW = GifLoop.Cels[i].Width;
                }
            }
            //add logical screen size info
            bytData[6] = (byte)((MaxW * GifOps.Zoom * 2) & 0xFF);
            bytData[7] = (byte)((MaxW * GifOps.Zoom * 2) >> 8);
            bytData[8] = (byte)((MaxH * GifOps.Zoom) & 0xFF);
            bytData[9] = (byte)((MaxH * GifOps.Zoom) >> 8);
            // add color info
            bytData[10] = 243; //1-111-0-011 means:
                               //global color table,
                               //8 bits per channel,
                               //no sorting, and
                               //16 colors in the table
                               //background color:
            bytData[11] = 0;
            //pixel aspect ratio:
            bytData[12] = 0; //should give proper 2:1 ratio for pixels

            //add global color table
            for (i = 0; i < 16; i++) {
                // TODO: need access to parent view palette
                bytData[13 + 3 * i] = GifLoop.Parent.Palette[i].R;
                bytData[14 + 3 * i] = GifLoop.Parent.Palette[i].G;
                bytData[15 + 3 * i] = GifLoop.Parent.Palette[i].B;
            }
            //if cycling, add netscape extension to allow continuous looping
            if (GifOps.Cycle) {
                //byte   1       : 33 (hex 0x21) GIF Extension code
                //byte   2       : 255 (hex 0xFF) Application Extension Label
                //byte   3       : 11 (hex 0x0B) Length of Application Block
                //                 (eleven bytes of data to follow)
                //bytes  4 to 11 : "NETSCAPE"
                //bytes 12 to 14 : "2.0"
                //byte  15       : 3 (hex 0x03) Length of Data static void-Block
                //                 (three bytes of data to follow)
                //byte  16       : 1 (hex 0x01)
                //bytes 17 to 18 : 0 to 65535, an unsigned integer in
                //                 lo-hi byte format. This indicate the
                //                 number of iterations the loop should
                //                 be executed.
                //byte  19       : 0 (hex 0x00) a Data static void-Block Terminator.
                bytData[61] = 0x21;
                bytData[62] = 0xFF;
                bytData[63] = 0xB;
                for (i = 0; i < 11; i++) {
                    bytData[i + 64] = (byte)"NETSCAPE2.0"[i];
                }
                bytData[75] = 3;
                bytData[76] = 1;
                bytData[77] = 0; //255
                bytData[78] = 0; //255
                bytData[79] = 0;
                //at this point, numbering is not absolute, so we need to begin tracking the data position
                lngPos = 80;
            }
            else {
                //at this point, numbering is not absolute, so we need to begin tracking the data position
                lngPos = 61;
            }
            //cel size is set to logical screen size
            //(if cell is smaller than logical screen size, it will be padded with transparent cells)
            bytCelData = new byte[(int)(MaxH * MaxW * Math.Pow(GifOps.Zoom, 2) * 2)];

            // make output array large enough to hold all cel data without compression
            Array.Resize(ref bytData, lngPos + (bytCelData.Length + 10) * GifLoop.Cels.Count);
            //add each cel
            for (i = 0; i < GifLoop.Cels.Count; i++) {
                //add graphic control extension for this cel
                bytData[lngPos] = 0x21;
                lngPos++;
                bytData[lngPos] = 0xF9;
                lngPos++;
                bytData[lngPos] = 4;
                lngPos++;
                bytData[lngPos] = (byte)(GifOps.Transparency ? 13 : 12);  //000-011-0-x = reserved-restore-no user input-transparency included
                lngPos++;
                bytData[lngPos] = (byte)(GifOps.Delay & 0xFF);
                lngPos++;
                bytData[lngPos] = (byte)((GifOps.Delay & 0xFF) >> 8);
                lngPos++;
                if (GifOps.Transparency) {
                    bytData[lngPos] = (byte)GifLoop.Cels[i].TransColor;
                }
                else {
                    bytData[lngPos] = 0;
                }
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                //add the cel data (first create cel data in separate array
                //then compress the cell data, break it into 255 byte chunks,
                //and add the chunks to the output
                //determine pad values
                celH = GifLoop.Cels[i].Height;
                celW = GifLoop.Cels[i].Width;
                hPad = (byte)(MaxH - celH);
                wPad = (byte)(MaxW - celW);
                bytTrans = (byte)GifLoop.Cels[i].TransColor;
                lngCelPos = 0;

                for (hVal = 0; hVal < MaxH; hVal++) {
                    //repeat each row based on scale factor
                    for (zFacH = 1; zFacH <= GifOps.Zoom; zFacH++) {
                        //step through each pixel in this row
                        for (wVal = 0; wVal < MaxW; wVal++) {
                            //repeat each pixel based on scale factor (x2 because AGI pixels are double-wide)
                            for (zFacW = 1; zFacW <= GifOps.Zoom * 2; zFacW++) {
                                //depending on alignment, may need to pad:
                                if (((hVal < hPad) && (GifOps.VAlign == 1)) || ((hVal > celH - 1) && (GifOps.VAlign == 0))) {
                                    //use a transparent pixel
                                    bytCelData[lngCelPos] = bytTrans;
                                }
                                else {
                                    if (((wVal < wPad) && (GifOps.HAlign == 1)) || ((wVal > celW - 1) && (GifOps.HAlign == 0))) {
                                        //use a transparent pixel
                                        bytCelData[lngCelPos] = bytTrans;
                                    }
                                    else {
                                        if (GifOps.HAlign == 1) {
                                            pX = (byte)(wVal - wPad);
                                        }
                                        else {
                                            pX = wVal;
                                        }
                                        if (GifOps.VAlign == 1) {
                                            pY = (byte)(hVal - hPad);
                                        }
                                        else {
                                            pY = hVal;
                                        }
                                        //use the actual pixel (adjusted for padding, if aligned to bottom or left)
                                        bytCelData[lngCelPos] = (byte)GifLoop.Cels[i][pX, pY];
                                    }
                                }
                                lngCelPos++;
                            }// zFacW
                        }// wVal
                    }// zFacH
                }// hVal
                // now compress the cel data
                bytCmpData = LZW.GifLZW(bytCelData);
                //add Image descriptor
                bytData[lngPos] = 0x2C;
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                bytData[lngPos] = (byte)((byte)(MaxW * GifOps.Zoom * 2) & 0xFF);
                lngPos++;
                bytData[lngPos] = (byte)((byte)(MaxW * GifOps.Zoom * 2) >> 8);
                lngPos++;
                bytData[lngPos] = (byte)((byte)(MaxH * GifOps.Zoom) & 0xFF);
                lngPos++;
                bytData[lngPos] = (byte)((byte)(MaxH * GifOps.Zoom) >> 8);
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                //add byte for initial LZW code size
                bytData[lngPos] = 4; //5
                lngPos++;
                //add the compressed data to filestream
                lngInPos = 0;
                intChunkSize = 0;
                do {
                    if (bytCmpData.Length - lngInPos > 255) {
                        intChunkSize = 255;
                    }
                    else {
                        intChunkSize = (short)(bytCmpData.Length - lngInPos);
                    }
                    //write chunksize
                    bytData[lngPos] = (byte)intChunkSize;
                    lngPos++;
                    //add this chunk of data
                    for (j = 1; j <= intChunkSize; j++) {
                        bytData[lngPos] = bytCmpData[lngInPos];
                        lngPos++;
                        lngInPos++;
                    }
                }
                while (lngInPos < bytCmpData.Length);// Until lngInPos >= UBound(bytCmpData())
                //end with a zero-length block
                bytData[lngPos] = 0;
                lngPos++;
            }
            // add trailer
            bytData[lngPos] = 0x3B;
            // resize 
            Array.Resize(ref bytData, lngPos + 1);

            //get temporary file
            strTempFile = Path.GetTempFileName();
            try {
                //open file for output
                using FileStream fsGif = new(strTempFile, FileMode.Open);
                //write data
                fsGif.Write(bytData);
                fsGif.Dispose();
                // move tempfile to savefile
                File.Move(strTempFile, ExportFile, true);
            }
            catch (Exception) {
                throw;
            }
            return true;
        }

        public static void ExportPicAsGif(Picture picture) {
            using frmExportViewLoopOptions frmVGO = new(picture);
            if (frmVGO.ShowDialog(MDIMain) == DialogResult.Cancel) {
                return;
            }
            GifOptions options = frmVGO.SelectedGifOptions;
            MDIMain.SaveDlg.Title = "Export Picture GIF";
            MDIMain.SaveDlg.DefaultExt = "gif";
            MDIMain.SaveDlg.Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*";
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.AddExtension = true;
            MDIMain.SaveDlg.DefaultExt = "gif";
            MDIMain.SaveDlg.FilterIndex = 1;
            MDIMain.SaveDlg.FileName = "";
            MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            MDIMain.SaveDlg.OkRequiresInteraction = true;
            MDIMain.SaveDlg.OverwritePrompt = true;
            if (MDIMain.SaveDlg.ShowDialog() == DialogResult.Cancel) {
                return;
            }
            DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
            ProgressWin = new () {
                Text = "Exporting Loop as GIF"
            };
            ProgressWin.lblProgress.Text = "Depending on size of picture, this may take awhile. Please wait...";
            ProgressWin.pgbStatus.Visible = true;
            ProgressWin.pgbStatus.Maximum = picture.Size;
            ProgressWin.Show(MDIMain);
            ProgressWin.Refresh();
            MDIMain.UseWaitCursor = true;
            MDIMain.Refresh();
            MakePicGif(picture, options, MDIMain.SaveDlg.FileName);
            ProgressWin.Close();
            ProgressWin.Dispose();
            MessageBox.Show(MDIMain, "Success!", "Export Picture as GIF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MDIMain.UseWaitCursor = false;
        }

        public static bool MakePicGif(Picture picture, GifOptions options, string filename) {

            string strTempFile;
            int lngPos; //data that will be written to the gif file
            int lngInPos;
            byte[] bytCmpData, bytPicData; //data used to build then compress pic data as gif Image
            byte bytCmd;
            bool blnXYDraw = false, blnVisOn = false;
            const int MaxH = 168;
            const int MaxW = 160;
            byte pX, pY;
            int lngFramePos;
            int intChunkSize;
            bool loaded = picture.Loaded;
            if (!loaded) {
                picture.Load();
            }
            picture.StepDraw = true;

            //build header
            byte[] bytData = new byte[255];
            bytData[0] = 71;
            bytData[1] = 73;
            bytData[2] = 70;
            bytData[3] = 56;
            bytData[4] = 57;
            bytData[5] = 97;
            // add logical screen size info
            bytData[6] = (byte)((MaxW * options.Zoom * 2) & 0xFF);
            bytData[7] = (byte)((MaxW * options.Zoom * 2) >> 8);
            bytData[8] = (byte)((MaxH * options.Zoom) & 0xFF);
            bytData[9] = (byte)((MaxH * options.Zoom) >> 8);
            // add color info
            bytData[10] = 243; //1-111-0-011 means:
                               //global color table,
                               //8 bits per channel,
                               //no sorting, and
                               //16 colors in the table
                               //background color:
            bytData[11] = 0;
            //pixel aspect ratio:
            bytData[12] = 0; //should give proper 2:1 ratio for pixels
            //  bytData[12] = 113; // '(113 + 15) / 64 = 2:1 ratio for pixels
            for (int i = 0; i < 16; i++) {
                bytData[13 + 3 * i] = picture.Palette[i].R;
                bytData[14 + 3 * i] = picture.Palette[i].G;
                bytData[15 + 3 * i] = picture.Palette[i].B;
            }
            //if cycling, add netscape extension to allow continuous looping
            if (options.Cycle) {
                //byte   1       : 33 (hex 0x21) GIF Extension code
                //byte   2       : 255 (hex 0xFF) Application Extension Label
                //byte   3       : 11 (hex 0x0B) Length of Application Block
                //                 (eleven bytes of data to follow)
                //bytes  4 to 11 : "NETSCAPE"
                //bytes 12 to 14 : "2.0"
                //byte  15       : 3 (hex 0x03) Length of Data static void-Block
                //                 (three bytes of data to follow)
                //byte  16       : 1 (hex 0x01)
                //bytes 17 to 18 : 0 to 65535, an unsigned integer in
                //                 lo-hi byte format. This indicate the
                //                 number of iterations the loop should
                //                 be executed.
                //byte  19       : 0 (hex 0x00) a Data static void-Block Terminator.
                bytData[61] = 0x21;
                bytData[62] = 0xFF;
                bytData[63] = 0xB;
                for (int i = 0; i < 11; i++) {
                    bytData[i + 64] = (byte)"NETSCAPE2.0"[i];
                }
                bytData[75] = 3;
                bytData[76] = 1;
                bytData[77] = 0; //255
                bytData[78] = 0; //255
                bytData[79] = 0;
                //at this point, numbering is not absolute, so we need to begin tracking the data position
                lngPos = 80;
            }
            else {
                //at this point, numbering is not absolute, so we need to begin tracking the data position
                lngPos = 61;
            }
            // pic data array
            bytPicData = new byte[(int)(MaxH * MaxW * Math.Pow(options.Zoom, 2) * 2)];

            // add frames
            int lngPicPos = -1;
            do {
                do {
                    lngPicPos++;
                    if (lngPicPos >= picture.Size) {
                        break;
                    }
                    bytCmd = picture.Data[lngPicPos];
                    switch (bytCmd) {
                    case 240:
                        blnXYDraw = false;
                        blnVisOn = true;
                        lngPicPos++;
                        break;
                    case 241:
                        blnXYDraw = false;
                        blnVisOn = false;
                        break;
                    case 242:
                        blnXYDraw = false;
                        lngPicPos++;
                        break;
                    case 243:
                        blnXYDraw = false;
                        break;
                    case 244 or 245:
                        blnXYDraw = true;
                        lngPicPos += 2;
                        break;
                    case 246 or 247 or 248 or 250:
                        blnXYDraw = false;
                        lngPicPos += 2;
                        break;
                    case 249:
                        blnXYDraw = false;
                        lngPicPos++;
                        break;
                    default:
                        // skip second coordinate byte, unless
                        // currently drawing X or Y lines
                        if (!blnXYDraw) {
                            lngPicPos++;
                        }
                        break;
                    }
                }
                // exit if non-pen cmd found, and vis pen is active
                while ((bytCmd >= 240 && bytCmd <= 244) || bytCmd == 249 || !blnVisOn);
                if (lngPicPos >= picture.Size) {
                    break;
                }
                // add picture drawn up to this point
                picture.DrawPos = lngPicPos;
                byte[] bytFrameData = picture.VisData;

                // expand data array if it might run out of room
                if (bytData.Length < lngPos + 53760 * Math.Pow(options.Zoom, 2) + 256) {
                    Array.Resize(ref bytData, (int)(lngPos + (53760 * Math.Pow(options.Zoom, 2)) + 256));
                }
                // add graphic control extension for this frame
                bytData[lngPos] = 0x21;
                lngPos++;
                bytData[lngPos] = 0xF9;
                lngPos++;
                bytData[lngPos] = 4;
                lngPos++;
                bytData[lngPos] = 12;   // 000-011-0-0 = reserved-restore-no user input-no transparency
                lngPos++;
                bytData[lngPos] = (byte)(options.Delay & 0xFF);
                lngPos++;
                bytData[lngPos] = (byte)((options.Delay & 0xFF) >> 8);
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                // add the frame data (first create frame data in separate array
                // then compress the frame data, break it into 255 byte chunks,
                // and add the chunks to the output
                lngFramePos = 0;
                for (pY = 0; pY < MaxH; pY++) {
                    // repeat each row based on scale factor
                    for (int zFacH = 1; zFacH <= options.Zoom; zFacH++) {
                        // step through each pixel in this row
                        for (pX = 0; pX < MaxW; pX++) {
                            // repeat each pixel based on scale factor (x2 because AGI pixels are double-wide)
                            for (int zFacW = 1; zFacW <= options.Zoom * 2; zFacW++) {
                                bytPicData[lngFramePos] = bytFrameData[pX + pY * 160];
                                lngFramePos++;
                            }
                        }
                    }
                }
                // compress the pic data
                bytCmpData = LZW.GifLZW(bytPicData);
                //add Image descriptor
                bytData[lngPos] = 0x2C;
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                bytData[lngPos] = (byte)((MaxW * options.Zoom * 2) & 0xFF);
                lngPos++;
                bytData[lngPos] = (byte)((MaxW * options.Zoom * 2) >> 8);
                lngPos++;
                bytData[lngPos] = (byte)((MaxH * options.Zoom) & 0xFF);
                lngPos++;
                bytData[lngPos] = (byte)((MaxH * options.Zoom) >> 8);
                lngPos++;
                bytData[lngPos] = 0;
                lngPos++;
                //add byte for initial LZW code size
                bytData[lngPos] = 4; //5
                lngPos++;
                //add the compressed data to filestream
                lngInPos = 0;
                intChunkSize = 0;
                do {
                    if (bytCmpData.Length - lngInPos > 255) {
                        intChunkSize = 255;
                    }
                    else {
                        intChunkSize = (short)(bytCmpData.Length - lngInPos);
                    }
                    //write chunksize
                    bytData[lngPos] = (byte)intChunkSize;
                    lngPos++;
                    //add this chunk of data
                    for (int j = 1; j <= intChunkSize; j++) {
                        bytData[lngPos] = bytCmpData[lngInPos];
                        lngPos++;
                        lngInPos++;
                    }
                }
                while (lngInPos < bytCmpData.Length);// Until lngInPos >= UBound(bytCmpData())
                //end with a zero-length block
                bytData[lngPos] = 0;
                lngPos++;
                // update progress
                ProgressWin.pgbStatus.Value = lngPicPos;
                ProgressWin.Refresh();
            }
            while (lngPicPos < picture.Size);
            // add trailer
            bytData[lngPos] = 0x3B;
            // resize 
            Array.Resize(ref bytData, lngPos + 1);
            //get temporary file
            strTempFile = Path.GetTempFileName();
            try {
                //open file for output
                using FileStream fsGif = new(strTempFile, FileMode.Open);
                //write data
                fsGif.Write(bytData);
                fsGif.Dispose();
                // move tempfile to savefile
                File.Move(strTempFile, filename, true);
            }
            catch (Exception) {
                throw;
            }
            if (!loaded) {
                picture.Unload();
            }
            return true;
        }

        public static void OpenWAGFile(string ThisGameFile = "") {
            // opens a wag file to edit a game

            if (ThisGameFile.Length == 0) {
                ThisGameFile = GetOpenResourceFilename("Open ", AGIResType.Game);
                if (ThisGameFile.Length == 0) {
                    return;
                }
            }
            if (OpenGame(0, ThisGameFile)) {
                // reset browser start dir to this dir
                // BrowserStartDir = JustPath(ThisGameFile);
            }
        }

        public static void OpenDIR() {
            string strMsg;
            string ThisGameDir;

            //get a directory for importing
            MDIMain.FolderDlg.Description = "Select the directory of the game you wish to import:";
            MDIMain.FolderDlg.AutoUpgradeEnabled = false;
            DialogResult result = MDIMain.FolderDlg.ShowDialog(MDIMain);
            if (result == DialogResult.OK) {
                //if not canceled;
                ThisGameDir = MDIMain.FolderDlg.SelectedPath;
                //if still nothing (user canceled),
                if (ThisGameDir.Length == 0) {
                    //user canceled
                    return;
                }
                //ensure trailing backslash
                ThisGameDir = FullDir(ThisGameDir);

                //if a game file exists
                if (File.Exists(ThisGameDir + "*.wag")) {
                    //confirm the import
                    strMsg = "This directory already has a WinAGI game file. Do you still want to import the game in this directory?" +
                             Environment.NewLine + Environment.NewLine + "The existing WinAGI game file will be overwritten if it has the same name as the GameID found in this directory//s AGI VOL and DIR files.";

                    if (MessageBox.Show(strMsg, "WinAGI Game File Already Exists", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) {
                        //then exit
                        return;
                    }
                }
                // open the game in this directory
                if (OpenGame(1, ThisGameDir)) {
                    // reset browser start dir to this dir
                    BrowserStartDir = ThisGameDir;
                }
                else {
                    // user cancelled closing of currently open game
                    return;
                }
                // check for error
                if (EditGame is null) {
                    return;
                }

                //set default text file directory to game source file directory
                DefaultResDir = EditGame.GameDir + EditGame.ResDirName + "\\";

                //did the resource directory change? (is this even possible?)
                //YES it is; if only one dir exists, and it has a different name,
                //it's assumed to be the resource directory
                strMsg = "Game file '" + EditGame.GameID + ".wag'  has been created." + Environment.NewLine + Environment.NewLine;
                if (EditGame.ResDirName != DefResDir) {
                    strMsg = strMsg + "The existing subdirectory '" + EditGame.ResDirName + "' will be used ";
                }
                else {
                    strMsg = strMsg + "The subdirectory '" + EditGame.ResDirName + "' has been created ";
                }
                strMsg = strMsg + "to store logic " +
                "source files and exported resources. You can change the " +
                "source directory for this game on the Game Properties dialog.";

                //warn user that resource dir set to default
                MessageBox.Show(strMsg, "Import Game", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //does the game have an Amiga OBJECT file?
                //very rare, but we check for it anyway
                if (EditGame.InvObjects.AmigaOBJ) {
                    MessageBox.Show("The OBJECT file for this game is formatted" + Environment.NewLine +
                           "for the Amiga." + Environment.NewLine + Environment.NewLine +
                           "If you intend to run this game on a DOS " + Environment.NewLine +
                           "platform, you will need to convert the file" + Environment.NewLine +
                           "to DOS format (use the Convert menu option" + Environment.NewLine +
                           "on the OBJECT Editor's Resource menu)", "Amiga OBJECT File detected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
        }

        public static bool OpenGame(int mode, string gameSource) {
            // opens a game by directory or wag file depending on mode
            // mode 0 == open source as a wag file
            // mode 1 == open source as a sierra game directory;

            if (EditGame is not null) {
                //close game, if user allows
                if (!CloseThisGame()) {
                    return false;
                }
            }

            // show wait cursor
            MDIMain.UseWaitCursor = true;
            MDIMain.Refresh();
            // show the progress window
            ProgressWin = new () {
                Text = "Loading Game"
            };
            ProgressWin.lblProgress.Text = "Checking WinAGI Game file ...";
            ProgressWin.StartPosition = FormStartPosition.CenterParent;
            ProgressWin.pgbStatus.Visible = false;
            // show loading msg in status bar
            MainStatusBar.Items["spStatus"].Text = (mode == 0 ? "Loading" : "Importing") + " game; please wait...";
            // pass mode and source
            LoadResults = new() {
                Mode = mode,
                Source = gameSource,
                Failed = false,
                ErrorMsg = "",
                Warnings = false
            };
            bgwOpenGame.RunWorkerAsync(LoadResults);
            // now show progress form
            ProgressWin.ShowDialog(MDIMain);
            // reset cursor
            MDIMain.UseWaitCursor = false;

            if (EditGame is not null) {
                AddToMRU(EditGame.GameFile);
                BrowserStartDir = EditGame.GameDir;
                DefaultResDir = EditGame.GameDir + EditGame.ResDirName + "\\";
                // build ID lookup table
                BuildIDefLookup();
                if (ActionCount < 182) {
                    InvalidCmdStyleRegEx = @"\b(";
                    for (int i = ActionCount; i < 182; i++) {
                        InvalidCmdStyleRegEx += Engine.Commands.ActionCommands[i].Name.Replace(".", "\\.");
                        if (i != 181) {
                            InvalidCmdStyleRegEx += @"|";
                        }
                    }
                    InvalidCmdStyleRegEx += @")\b";
                }
                else {
                    InvalidCmdStyleRegEx = "";
                }
            }
            else {
                //make sure warning grid is hidden
                if (MDIMain.pnlWarnings.Visible) {
                    MDIMain.HideWarningList(true);
                }
            }
            UpdateTBGameBtns();
            MainStatusBar.Items["spStatus"].Text = "";
            return !LoadResults.Failed;
        }

        public static bool CloseThisGame() {
            int i, j;
            DialogResult rtn;
            //if no game is open
            if (EditGame is null) {
                //just return success
                return true;
            }
            // unload all ingame resource editors;
            // if any editors cancel closing, CloseGame
            // returns false

            // unload ingame logic editors
            for (i = LogicEditors.Count - 1; i >= 0; i--) {
                if (LogicEditors[i].FormMode == LogicFormMode.Logic) {
                    if (LogicEditors[i].InGame) {
                        j = LogicEditors.Count;
                        Form form = LogicEditors[i];
                        form.Close();
                        if (j == LogicEditors.Count) {
                            return false;
                        }
                        else {
                            form?.Dispose();
                        }
                    }
                }
            }
            // unload ingame picture edit windows
            for (i = PictureEditors.Count - 1; i >= 0; i--) {
                if (PictureEditors[i].InGame) {
                    j = PictureEditors.Count;
                    Form form = PictureEditors[i];
                    form.Close();
                    // check for cancellation
                    if (j == PictureEditors.Count) {
                        return false;
                    }
                    else {
                        form?.Dispose();
                    }
                }
            }
            // unload in-game sound editors
            for (i = SoundEditors.Count - 1; i >= 0; i--) {
                if (SoundEditors[i].InGame) {
                    j = SoundEditors.Count;
                    Form form = SoundEditors[i];
                    form.Close();
                    // check for cancellation
                    if (j == SoundEditors.Count) {
                        return false;
                    }
                    else {
                        form?.Dispose();
                    }
                }
            }
            // unload ingame view edit windows
            for (i = ViewEditors.Count - 1; i >= 0; i--) {
                if (ViewEditors[i].InGame) {
                    j = ViewEditors.Count;
                    Form form = ViewEditors[i];
                    form.Close();
                    //check for cancellation
                    if (j == ViewEditors.Count) {
                        return false;
                    }
                    else {
                        form?.Dispose();
                    }
                }
            }
            // unload ingame Objects Editor
            if (OEInUse) {
                ObjectEditor.Close();
                // if user canceled, 
                if (OEInUse) {
                    return false;
                }
                else {
                    ObjectEditor?.Dispose();
                }
            }
            //unload ingame word editor
            if (WEInUse) {
                WordEditor.Close();
                if (WEInUse) {
                    return false;
                }
                else {
                    WordEditor?.Dispose();
                }
            }
            // unload layout editor
            if (LEInUse) {
                LayoutEditor.Close();
                if (LEInUse) {
                    return false;
                }
                else {
                    LayoutEditor?.Dispose();
                }
            }
            // unload globals editor
            if (GEInUse) {
                GlobalsEditor.Close();
                if (GEInUse) {
                    return false;
                }
                else {
                    GlobalsEditor?.Dispose();
                }
            }
            // unload the menu editor
            if (MEInUse) {
                MenuEditor.Close();
                if (MEInUse) {
                    return false;
                }
                else {
                    MenuEditor?.Dispose();
                }
            }
            // always clear and hide warning list if it is showing
            if (MDIMain.pnlWarnings.Visible) {
                MDIMain.HideWarningList(true);
            }
            // always hide find dialog if it's showing
            if (FindingForm.Visible) {
                FindingForm.Visible = false;
            }
            if (WinAGISettings.ShowPreview.Value) {
                // clear preview window
                PreviewWin.ClearPreviewWin();
            }
            // resource editors and preview are closed so all resources
            // should now be unloaded, but just in case...

            // unload all resources
            foreach (Logic tmpLog in EditGame.Logics) {
                if (tmpLog.Loaded) {
                    if (tmpLog.IsChanged) {
                        rtn = MessageBox.Show(tmpLog.ID + " has changed. Do you want to save the changes?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        switch (rtn) {
                        case DialogResult.Yes:
                            tmpLog.Save();
                            break;
                        case DialogResult.Cancel:
                            return false;
                        }
                    }
                    tmpLog.Unload();
                }
            }
            foreach (Picture tmpPic in EditGame.Pictures) {
                if (tmpPic.Loaded) {
                    if (tmpPic.IsChanged) {
                        rtn = MessageBox.Show(tmpPic.ID + " has changed. Do you want to save the changes?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        switch (rtn) {
                        case DialogResult.Yes:
                            tmpPic.Save();
                            break;
                        case DialogResult.Cancel:
                            return false;
                        }
                    }
                    tmpPic.Unload();
                }
            }
            foreach (Sound tmpSnd in EditGame.Sounds) {
                if (tmpSnd.Loaded) {
                    if (tmpSnd.IsChanged) {
                        rtn = MessageBox.Show(tmpSnd.ID + " has changed. Do you want to save the changes?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        switch (rtn) {
                        case DialogResult.Yes:
                            tmpSnd.Save();
                            break;
                        case DialogResult.Cancel:
                            return false;
                        }
                    }
                    tmpSnd.Unload();
                }
            }
            foreach (Engine.View tmpView in EditGame.Views) {
                if (tmpView.Loaded) {
                    if (tmpView.IsChanged) {
                        rtn = MessageBox.Show(tmpView.ID + " has changed. Do you want to save the changes?", "Save Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        switch (rtn) {
                        case DialogResult.Yes:
                            tmpView.Save();
                            break;
                        case DialogResult.Cancel:
                            return false;
                        }
                    }
                    tmpView.Unload();
                }
            }
            // clear resource list and preview window
            if (WinAGISettings.ResListType.Value != EResListType.None) {
                MDIMain.HideResTree();
                MDIMain.ClearResourceList();
            }
            if (WinAGISettings.ShowPreview.Value) {
                PreviewWin.Hide();
            }
            // now close the game
            EditGame.CloseGame();
            EditGame = null;
            // restore colors to AGI default when a game closes
            GetDefaultColors();
            InvalidCmdStyleRegEx = @"";
            // update main form caption
            MDIMain.Text = "WinAGI GDS";
            // refresh main toolbar
            UpdateTBGameBtns();
            // reset node marker so selection of resources
            // works correctly first time after another game loaded
            MDIMain.LastNodeName = "";
            // reset default text location to program dir
            DefaultResDir = ProgramDir;
            // game is closed
            return true;
        }

        private static void UpdateTBGameBtns() {
            // enable/disable buttons based on current game/editor state
            MDIMain.toolStrip1.Items["btnCloseGame"].Enabled = EditGame != null;
            MDIMain.toolStrip1.Items["btnRun"].Enabled = EditGame != null;
            MDIMain.toolStrip1.Items["btnImportRes"].Enabled = EditGame != null;
            MDIMain.toolStrip1.Items["btnLayoutEd"].Enabled = EditGame != null;
        }

        internal static void UpdateTBResourceBtns(AGIResType restype, bool ingame, bool changed) {
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = changed;
            switch (restype) {
            case AGIResType.Game:
                MDIMain.toolStrip1.Items["btnSaveResource"].Text = "Save Resource";
                MDIMain.toolStrip1.Items["btnExportRes"].Enabled = true;
                MDIMain.toolStrip1.Items["btnExportRes"].Text = "Export All Resources";
                MDIMain.toolStrip1.Items["btnAddRemove"].Enabled = false;
                MDIMain.toolStrip1.Items["btnAddRemove"].Image = MDIMain.imageList1.Images[19];
                MDIMain.toolStrip1.Items["btnAddRemove"].Text = "Add/Remove Resource";
                break;
            case AGIResType.Logic:
            case AGIResType.Picture:
            case AGIResType.Sound:
            case AGIResType.View:
                MDIMain.toolStrip1.Items["btnSaveResource"].Text = "Save " + restype.ToString();
                MDIMain.toolStrip1.Items["btnExportRes"].Enabled = true;
                MDIMain.toolStrip1.Items["btnExportRes"].Text = "Export " + restype.ToString();
                MDIMain.toolStrip1.Items["btnAddRemove"].Enabled = true;
                if (ingame) {
                    MDIMain.toolStrip1.Items["btnAddRemove"].Image = MDIMain.imageList1.Images[20];
                    MDIMain.toolStrip1.Items["btnAddRemove"].Text = "Remove " + restype.ToString();
                }
                else {
                    MDIMain.toolStrip1.Items["btnAddRemove"].Image = MDIMain.imageList1.Images[19];
                    MDIMain.toolStrip1.Items["btnAddRemove"].Text = "Add " + restype.ToString();
                }
                break;
            case AGIResType.Objects:
            case AGIResType.Words:
                MDIMain.toolStrip1.Items["btnSaveResource"].Text = "Save " + restype.ToString();
                MDIMain.toolStrip1.Items["btnExportRes"].Enabled = true;
                MDIMain.toolStrip1.Items["btnExportRes"].Text = "Export " + restype.ToString();
                MDIMain.toolStrip1.Items["btnAddRemove"].Enabled = false;
                MDIMain.toolStrip1.Items["btnAddRemove"].Image = MDIMain.imageList1.Images[19];
                MDIMain.toolStrip1.Items["btnAddRemove"].Text = "Add/Remove Resource";
                break;
            case AGIResType.Globals:
                MDIMain.toolStrip1.Items["btnSaveResource"].Text = "Save " + restype.ToString();
                MDIMain.toolStrip1.Items["btnExportRes"].Enabled = true;
                MDIMain.toolStrip1.Items["btnExportRes"].Text = "Export " + restype.ToString();
                MDIMain.toolStrip1.Items["btnAddRemove"].Enabled = false;
                MDIMain.toolStrip1.Items["btnAddRemove"].Image = MDIMain.imageList1.Images[19];
                MDIMain.toolStrip1.Items["btnAddRemove"].Text = "Add/Remove Resource";
                break;
            default:
                MDIMain.toolStrip1.Items["btnSaveResource"].Text = "Save Resource";
                MDIMain.toolStrip1.Items["btnExportRes"].Enabled = false;
                MDIMain.toolStrip1.Items["btnExportRes"].Text = "Export Resource";
                MDIMain.toolStrip1.Items["btnAddRemove"].Enabled = false;
                MDIMain.toolStrip1.Items["btnAddRemove"].Image = MDIMain.imageList1.Images[19];
                MDIMain.toolStrip1.Items["btnAddRemove"].Text = "Add/Remove Resource";
                break;
            }
        }

        /// <summary>
        /// Refreshes the specifiednode/list item. Also refreshes the
        /// preview window if specified item is selected.
        /// </summary>
        /// <param name="restype"></param>
        /// <param name="resnum"></param>
        public static void RefreshTree(AGIResType restype, int resnum, bool nopreview = false) {
            // if no resource selected (resnum = -1), do nothing
            if (resnum < 0) {
                return;
            }
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.None:
                return;
            case EResListType.TreeList:
                switch (restype) {
                case AGIResType.Logic:
                    TreeNode tmpNode = HdrNode[(int)AGIResType.Logic].Nodes["l" + resnum];
                    if (EditGame.Logics[(byte)tmpNode.Tag].Compiled && EditGame.Logics[(byte)tmpNode.Tag].ErrLevel >= 0) {
                        tmpNode.ForeColor = Color.Black;
                    }
                    else {
                        tmpNode.ForeColor = Color.Red;
                    }
                    tmpNode.Text = ResourceName(EditGame.Logics[(byte)tmpNode.Tag], true);
                    break;
                case AGIResType.Picture:
                    tmpNode = HdrNode[(int)AGIResType.Picture].Nodes["p" + resnum];
                    tmpNode.ForeColor = EditGame.Pictures[(byte)tmpNode.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                    tmpNode.Text = ResourceName(EditGame.Pictures[(byte)tmpNode.Tag], true);
                    break;
                case AGIResType.Sound:
                    tmpNode = HdrNode[(int)AGIResType.Sound].Nodes["s" + resnum];
                    tmpNode.ForeColor = EditGame.Sounds[(byte)tmpNode.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                    tmpNode.Text = ResourceName(EditGame.Sounds[(byte)tmpNode.Tag], true);
                    break;
                case AGIResType.View:
                    tmpNode = HdrNode[(int)AGIResType.View].Nodes["v" + resnum];
                    tmpNode.ForeColor = EditGame.Views[(byte)tmpNode.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                    tmpNode.Text = ResourceName(EditGame.Views[(byte)tmpNode.Tag], true);
                    break;
                }
                break;
            case EResListType.ComboList:
                if (MDIMain.cmbResType.SelectedIndex == (int)(restype + 1)) {
                    switch (restype) {
                    case AGIResType.Logic:
                        ListViewItem tmpItem = MDIMain.lstResources.Items["l" + resnum];
                        if (EditGame.Logics[(byte)tmpItem.Tag].Compiled && EditGame.Logics[(byte)tmpItem.Tag].ErrLevel >= 0) {
                            tmpItem.ForeColor = Color.Black;
                        }
                        else {
                            tmpItem.ForeColor = Color.Red;
                        }
                        tmpItem.Text = ResourceName(EditGame.Logics[(byte)tmpItem.Tag], true);
                        break;
                    case AGIResType.Picture:
                        tmpItem = MDIMain.lstResources.Items["p" + resnum];
                        tmpItem.ForeColor = EditGame.Pictures[(byte)tmpItem.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                        tmpItem.Text = ResourceName(EditGame.Pictures[(byte)tmpItem.Tag], true);
                        break;
                    case AGIResType.Sound:
                        tmpItem = MDIMain.lstResources.Items["s" + resnum];
                        tmpItem.ForeColor = EditGame.Sounds[(byte)tmpItem.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                        tmpItem.Text = ResourceName(EditGame.Pictures[(byte)tmpItem.Tag], true);
                        break;
                    case AGIResType.View:
                        tmpItem = MDIMain.lstResources.Items["v" + resnum];
                        tmpItem.ForeColor = EditGame.Views[(byte)tmpItem.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                        tmpItem.Text = ResourceName(EditGame.Views[(byte)tmpItem.Tag], true);
                        break;
                    }
                }
                break;
            }
            if (WinAGISettings.ShowPreview.Value && !nopreview) {
                if (restype == SelResType && resnum == SelResNum) {
                    //redraw the preview
                    PreviewWin.LoadPreview(SelResType, SelResNum);
                }
            }
            MDIMain.RefreshPropertyGrid(SelResType, SelResNum);
        }

        /// <summary>
        /// Refreshes all resources in the tree.
        /// </summary>
        public static void RefreshTree() {

            switch (WinAGISettings.ResListType.Value) {
            case EResListType.None:
                return;
            case EResListType.TreeList:
                Debug.Assert(EditGame.GameID.Length != 0);
                //update root
                MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                // refresh logics
                foreach (TreeNode tmpNode in HdrNode[(int)AGIResType.Logic].Nodes) {
                    if (EditGame.Logics[(byte)tmpNode.Tag].Compiled && EditGame.Logics[(byte)tmpNode.Tag].ErrLevel >= 0) {
                        tmpNode.ForeColor = Color.Black;
                    }
                    else {
                        tmpNode.ForeColor = Color.Red;
                    }
                }
                // refresh pictures
                foreach (TreeNode tmpNode in HdrNode[(int)AGIResType.Picture].Nodes) {
                    tmpNode.ForeColor = EditGame.Pictures[(byte)tmpNode.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                }
                // refresh sounds
                foreach (TreeNode tmpNode in HdrNode[(int)AGIResType.Sound].Nodes) {
                    tmpNode.ForeColor = EditGame.Sounds[(byte)tmpNode.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                }
                // refresh views
                foreach (TreeNode tmpNode in HdrNode[(int)AGIResType.View].Nodes) {
                    tmpNode.ForeColor = EditGame.Views[(byte)tmpNode.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                }
                break;
            case EResListType.ComboList:
                //update root
                MDIMain.cmbResType.Items[0] = EditGame.GameID;
                switch (MDIMain.cmbResType.SelectedIndex) {
                case 1:
                    foreach (ListViewItem tmpItem in MDIMain.lstResources.Items) {
                            if (EditGame.Logics[(byte)tmpItem.Tag].Compiled && EditGame.Logics[(byte)tmpItem.Tag].ErrLevel >= 0) {
                            tmpItem.ForeColor = Color.Black;
                        }
                        else {
                            tmpItem.ForeColor = Color.Red;
                        }
                    }
                    break;
                case 2:
                    foreach (ListViewItem tmpItem in MDIMain.lstResources.Items) {
                        tmpItem.ForeColor = EditGame.Pictures[(byte)tmpItem.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                    }
                    break;
                case 3:
                    foreach (ListViewItem tmpItem in MDIMain.lstResources.Items) {
                        tmpItem.ForeColor = EditGame.Sounds[(byte)tmpItem.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                    }
                    break;
                case 4:
                    foreach (ListViewItem tmpItem in MDIMain.lstResources.Items) {
                        tmpItem.ForeColor = EditGame.Views[(byte)tmpItem.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                    }
                    break;
                }
                break;
            }
        }

        public static void BuildResourceTree() {
            // builds the resource tree list
            // for the current open game
            //int i;
            TreeNode tmpNode;

            switch (WinAGISettings.ResListType.Value) {
            case EResListType.None:
                return;
            case EResListType.TreeList:
                // remove existing resources
                MDIMain.tvwResources.Nodes[0].Nodes[sLOGICS].Nodes.Clear();
                MDIMain.tvwResources.Nodes[0].Nodes[sPICTURES].Nodes.Clear();
                MDIMain.tvwResources.Nodes[0].Nodes[sSOUNDS].Nodes.Clear();
                MDIMain.tvwResources.Nodes[0].Nodes[sVIEWS].Nodes.Clear();
                if (EditGame.GameID.Length != 0) {
                    //update root
                    MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                    //add logics
                    if (EditGame.Logics.Count > 0) {
                        for (int i = 0; i <= 255; i++) {
                            //if a valid resource
                            if (EditGame.Logics.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sLOGICS].Nodes.Add("l" + i, ResourceName(EditGame.Logics[(byte)i], true));
                                tmpNode.Tag = (byte)i;
                                // get compiled status
                                if (EditGame.Logics[(byte)i].Compiled && EditGame.Logics[(byte)i].ErrLevel >= 0) {
                                    tmpNode.ForeColor = Color.Black;
                                }
                                else {
                                    tmpNode.ForeColor = Color.Red;
                                }
                            }
                        }
                    }
                    if (EditGame.Pictures.Count > 0) {
                        for (int i = 0; i <= 255; i++) {
                            //if a valid resource
                            if (EditGame.Pictures.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sPICTURES].Nodes.Add("p" + i, ResourceName(EditGame.Pictures[(byte)i], true));
                                tmpNode.Tag = (byte)i;
                                tmpNode.ForeColor = EditGame.Pictures[(byte)i].ErrLevel >= 0 ? Color.Black : Color.Red;
                            }
                        }
                    }
                    if (EditGame.Sounds.Count > 0) {
                        for (int i = 0; i <= 255; i++) {
                            //if a valid resource
                            if (EditGame.Sounds.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sSOUNDS].Nodes.Add("s" + i, ResourceName(EditGame.Sounds[(byte)i], true));
                                tmpNode.Tag = (byte)i;
                                tmpNode.ForeColor = EditGame.Sounds[(byte)i].ErrLevel >= 0 ? Color.Black : Color.Red;
                            }
                        }
                    }
                    if (EditGame.Views.Count > 0) {
                        for (int i = 0; i <= 255; i++) {
                            //if a valid resource
                            if (EditGame.Views.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sVIEWS].Nodes.Add("v" + i, ResourceName(EditGame.Views[(byte)i], true));
                                tmpNode.Tag = (byte)i;
                                tmpNode.ForeColor = EditGame.Views[(byte)i].ErrLevel >= 0 ? Color.Black : Color.Red;
                            }
                        }
                    }
                }
                break;
            case EResListType.ComboList:
                //update root
                MDIMain.cmbResType.Items[0] = EditGame.GameID;
                //select root
                MDIMain.cmbResType.SelectedIndex = 0;
                break;
            }
            return;
        }

        public static void BuildSnippets() {
            // loads snippet file, and creates array of snippets
            SettingsFile SnipList = new(ProgramDir + "snippets.txt", FileMode.OpenOrCreate);

            // if nothing returned (meaning file was empty)
            if (SnipList.Lines.Count == 0) {
                SnipList.Lines.Add("#");
                SnipList.Lines.Add("# WinAGI Snippets");
                SnipList.Lines.Add("#");
                SnipList.Lines.Add("# control codes:");
                SnipList.Lines.Add("#   %n = new line");
                SnipList.Lines.Add("#   %q = '\"'");
                SnipList.Lines.Add("#   %t = tab (based on current tab setting)");
                SnipList.Lines.Add("#   %% = '%'");
                SnipList.Lines.Add("#   %1, %2, etc snippet argument value");
                SnipList.Lines.Add("");
                SnipList.Save();
                return;
            }
            // retrieve each snippet (no error checking is done
            // except for blank value or blank name; in that case
            // the snippet is ignored; if duplicate names exist,
            // they are added, and user will just have to deal
            // with it...
            int pos = 0;
            do {
                Snippet addsnippet = new();
                KeyValuePair<string, string>[] snipinfo = SnipList.GetNextSection("Snippet", ref pos);
                for (int i = 0; i < snipinfo.Length; i++) {
                    switch (snipinfo[i].Key) {
                    case "Name":
                        addsnippet.Name = snipinfo[i].Value;
                        break;
                    case "Value":
                        addsnippet.Value = DecodeSnippet(snipinfo[i].Value);
                        break;
                    case "ArgTips":
                        addsnippet.ArgTips = DecodeSnippet(snipinfo[i].Value);
                        break;
                    }
                }
                if (addsnippet.Name.Length > 0 && addsnippet.Value.Length > 0) {
                    Array.Resize(ref CodeSnippets, CodeSnippets.Length + 1);
                    CodeSnippets[^1] = addsnippet;
                }
            } while (pos < SnipList.Lines.Count);
        }

        public static string DecodeSnippet(string SnipText) {
            //replaces control codes in SnipText and returns
            //the full expanded text
            //(does not handle argument values; they are left in
            // place until needed when a snippet is inserted into
            // a logic)

            //first check for '%%' - temporarily replace them
            // with char 25
            string retval = SnipText.Replace("%%", ((char)25).ToString());
            // carriage returns/new lines
            retval = retval.Replace("%n", Environment.NewLine);

            //quote marks
            retval = retval.Replace("%q", QUOTECHAR.ToString());

            //tabs
            retval = retval.Replace("%t", new String(' ', WinAGISettings.LogicTabWidth.Value));

            //lastly, restore any forced percent signs
            retval = retval.Replace((char)25, '%');
            return retval;
        }

        public static void OpenMRUGame(int Index) {
            // skip if this MRU is blank (probably due to user manually editing)
            // the config file)
            if (strMRU[Index].Length == 0) {
                // if entry is blank, but there's text on the menu bar, inform user
                MessageBox.Show("Invalid MRU file name, deleting this entry.");
                return;
            }
            // attempt to open this game
            if (OpenGame(0, strMRU[Index])) {
                // reset browser start dir to this dir
                // BrowserStartDir = JustPath(strMRU[Index]);
            }
            else {
                // step through rest of mru entries
                for (int i = Index + 1; i < 4; i++) {
                    // move this mru entry up
                    strMRU[i - 1] = strMRU[i];
                    if (strMRU[i - 1].Length == 0) {
                        // hide this mru item
                        MDIMain.mnuGame.DropDownItems["mnuGMRU" + (i - 1)].Visible = false;
                    }
                    else {
                        // change this mru item
                        MDIMain.mnuGame.DropDownItems["mnuGMRU" + (i - 1)].Text = CompactPath(strMRU[i], 60);
                    }
                }
                // remove last entry
                strMRU[3] = "";
                MDIMain.mnuGMRU3.Visible = false;
                if (strMRU[0].Length == 0) {
                    // if none left, hide bar too
                    MDIMain.mnuGMRUBar.Visible = false;
                }
            }
        }

        public static void AddToMRU(string NewWAGFile) {
            // if NewWAGFile is already in the list,
            // it is moved to the top;
            // otherwise, it is added to the top, and other
            // entries are moved down
            int i, j;
            // if already at the top
            if (NewWAGFile == strMRU[0]) {
                // nothing to change
                return;
            }
            for (i = 0; i < 4; i++) {
                if (NewWAGFile == strMRU[i]) {
                    // move others down
                    for (j = i; j >= 1; j--) {
                        strMRU[j] = strMRU[j - 1];
                        MDIMain.mnuGame.DropDownItems["mnuGMRU" + j].Text = MDIMain.mnuGame.DropDownItems["mnuGMRU" + (j - 1)].Text;
                        MDIMain.mnuGame.DropDownItems["mnuGMRU" + j].Visible = true; // MDIMain.mnuGame.DropDownItems["mnuGMRU" + (j - 1)].Visible;
                    }
                    // move item i to top of list
                    strMRU[0] = NewWAGFile;
                    MDIMain.mnuGMRU0.Text = CompactPath(NewWAGFile, 60);
                    // done
                    return;
                }
            }
            // not found; move all entries down
            for (j = 3; j >= 1; j--) {
                strMRU[j] = strMRU[j - 1];
                MDIMain.mnuGame.DropDownItems["mnuGMRU" + j].Text = MDIMain.mnuGame.DropDownItems["mnuGMRU" + (j - 1)].Text;
                MDIMain.mnuGame.DropDownItems["mnuGMRU" + j].Visible = strMRU[j].Length != 0;
            }
            // add new item 0
            strMRU[0] = NewWAGFile;
            MDIMain.mnuGMRU0.Text = CompactPath(NewWAGFile, 60);
            MDIMain.mnuGMRU0.Visible = true;
            MDIMain.mnuGMRUBar.Visible = true;
        }

        public static void BuildIDefLookup() {
            // adds all resource IDs to the table, making sure
            // anything that's blank gets reset
            TDefine tmpDef = new();
            for (int i = 0; i <= 255; i++) {
                tmpDef.Value = i.ToString();
                // add logics
                if (EditGame.Logics.Contains((byte)i)) {
                    tmpDef.Name = EditGame.Logics[(byte)i].ID;
                    tmpDef.Type = Num;
                }
                else {
                    tmpDef.Name = "";
                    tmpDef.Type = ArgType.None;
                }
                IDefLookup[(int)AGIResType.Logic, i] = tmpDef;
                // then pictures
                if (EditGame.Pictures.Contains((byte)i)) {
                    tmpDef.Name = EditGame.Pictures[(byte)i].ID;
                    tmpDef.Type = Num;
                }
                else {
                    tmpDef.Name = "";
                    tmpDef.Type = ArgType.None;
                }
                IDefLookup[(int)AGIResType.Picture, i] = tmpDef;
                // and sounds
                if (EditGame.Sounds.Contains((byte)i)) {
                    tmpDef.Name = EditGame.Sounds[(byte)i].ID;
                    tmpDef.Type = Num;
                }
                else {
                    tmpDef.Name = "";
                    tmpDef.Type = ArgType.None;
                }
                IDefLookup[(int)AGIResType.Sound, i] = tmpDef;
                // and finally, views
                if (EditGame.Views.Contains((byte)i)) {
                    tmpDef.Name = EditGame.Views[(byte)i].ID;
                    tmpDef.Type = Num;
                }
                else {
                    tmpDef.Name = "";
                    tmpDef.Type = ArgType.None;
                }
                IDefLookup[(int)AGIResType.View, i] = tmpDef;
            }

            //don't need to worry about open editors; the initial build is
            //only called when a game is first loaded; changes to the
            //ID lookup list are handled by the add/remove resource functions
        }

        public static ArgType DefTypeFromValue(string strValue) {
            if (strValue.Length == 0) {
                //strValue = "\"\"";
                return DefStr;
            }
            if (strValue[0] == 34) {
                return DefStr;
            }
            if (strValue.IsNumeric()) {
                return Num;
            }
            else {
                switch ((int)strValue[0]) {
                case 99: //"c"
                    return Ctrl;
                case 102: //"f"
                    return Flag;
                case 105: //"i"
                    return InvItem;
                case 109: //"m"
                    return Msg;
                case 111: //"o"
                    return SObj;
                case 115: //"s"
                    return Str;
                case 118: //"v"
                    return Var;
                case 119: //"w"
                    return Word;
                default:
                    // assume a defined string
                    //strValue = "\"" + strValue;
                    //if (strValue[^1] != '\"') {
                    //    strValue += "\"";
                    //}
                    return DefStr;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void CompileGame(string CompGameDir = "", bool RebuildOnly = false) {
            DialogResult rtn = DialogResult.Cancel;
            string strTemp = "";
            bool blnDontAsk = false;

            if (EditGame == null) {
                return;
            }
            //if global editor or layout editor open and unsaved, ask to continue
            if (GEInUse && GlobalsEditor.IsChanged) {
                strTemp = "Do you want to save the Global Defines list before compiling?";
            }
            if (LEInUse && LayoutEditor.IsChanged) {
                if (strTemp.Length > 0) {
                    strTemp = "Do you want to save the Global Defines list and \nLayout Editor before compiling?";
                }
                else {
                    strTemp = "Do you want to save the Global Defines list before compiling?";
                }
            }
            if (strTemp.Length > 0) {
                rtn = MessageBox.Show(MDIMain,
                         strTemp,
                         "Save Before Compile?",
                         MessageBoxButtons.YesNoCancel,
                         MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    if (LEInUse) {
                        if (LayoutEditor.IsChanged) {
                            LayoutEditor.MenuClickSave();
                        }
                    }
                    if (GEInUse) {
                        if (GlobalsEditor.IsChanged) {
                            GlobalsEditor.MenuClickSave();
                        }
                    }
                    break;
                case DialogResult.Cancel:
                    return;
                }
            }
            // check for any open resources
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == LogicFormMode.Logic) {
                    if (frm.rtfLogic2.IsChanged) {
                        switch (WinAGISettings.SaveOnCompile.Value) {
                        case AskOption.Ask:
                            // ask user for input
                            // get user's response
                            frm.Select();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this logic before compiling?",
                                "Update " + ResourceName(frm.EditLogic, true, true) + "?",
                                 MessageBoxButtons.YesNoCancel,
                                 MessageBoxIcon.Question,
                                 "Always take this action when compiling a game.", ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.Yes;
                                }
                                else if (rtn == DialogResult.No) {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.No;
                                }
                                WinAGISettings.SaveOnCompile.WriteSetting(WinAGISettingsFile);
                            }
                            break;
                        case AskOption.No:
                            rtn = DialogResult.No;
                            break;
                        case AskOption.Yes:
                            rtn = DialogResult.Yes;
                            break;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            // save it
                            frm.SaveLogicSource();
                            break;
                        }
                    }
                }
            }
            foreach (frmPicEdit frm in PictureEditors) {
                if (frm.EditPicture.IsChanged) {
                    if (WinAGISettings.SaveOnCompile.Value != AskOption.No) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile.Value == AskOption.Ask) {
                            // get user's response
                            frm.Select();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this picture before compiling?",
                                "Update " + ResourceName(frm.EditPicture, true, true) + "?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.Yes;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.No;
                                }
                                WinAGISettings.SaveOnCompile.WriteSetting(WinAGISettingsFile);
                            }
                        }
                        else {
                            // if on automatic, always say yes
                            rtn = DialogResult.Yes;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            frm.SavePicture();
                            break;
                        }
                    }
                }
            }
            foreach (frmSoundEdit frm in SoundEditors) {
                if (frm.EditSound.IsChanged) {
                    if (WinAGISettings.SaveOnCompile.Value != AskOption.No) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile.Value == AskOption.Ask) {
                            // get user's response
                            frm.Select();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this Sound before compiling?",
                                "Update " + ResourceName(frm.EditSound, true, true) + "?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.Yes;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.No;
                                }
                                WinAGISettings.SaveOnCompile.WriteSetting(WinAGISettingsFile);
                            }
                        }
                        else {
                            //if on automatic, always say yes
                            rtn = DialogResult.Yes;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            frm.SaveSound();
                            break;
                        }
                    }
                }
            }
            foreach (frmViewEdit frm in ViewEditors) {
                if (frm.EditView.IsChanged) {
                    if (WinAGISettings.SaveOnCompile.Value != AskOption.No) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile.Value == AskOption.Ask) {
                            // get user's response
                            frm.Select();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this View before compiling?",
                                "Update " + ResourceName(frm.EditView, true, true) + "?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.Yes;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.No;
                                }
                                WinAGISettings.SaveOnCompile.WriteSetting(WinAGISettingsFile);
                            }
                        }
                        else {
                            // if on automatic, always say yes
                            rtn = DialogResult.Yes;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            // save it
                            frm.SaveView();
                            break;
                        }
                    }
                }
            }
            if (OEInUse) {
                if (ObjectEditor.IsChanged) {
                    if (WinAGISettings.SaveOnCompile.Value != AskOption.No) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile.Value == AskOption.Ask) {
                            // get user's response
                            ObjectEditor.Select();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save OBJECT file before compiling?",
                                "Update OBJECT File?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.Yes;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.No;
                                }
                                WinAGISettings.SaveOnCompile.WriteSetting(WinAGISettingsFile);
                            }
                        }
                        else {
                            // if on automatic, always say yes
                            rtn = DialogResult.Yes;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            ObjectEditor.SaveObjects();
                            break;
                        }
                    }
                }
            }
            if (WEInUse) {
                if (WordEditor.IsChanged) {
                    if (WinAGISettings.SaveOnCompile.Value != AskOption.No) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile.Value == AskOption.Ask) {
                            //get user's response
                            WordEditor.Select();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save WORDS.TOK file before compiling?",
                                "Update WORDS.TOK File?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.Yes;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.No;
                                }
                                WinAGISettings.SaveOnCompile.WriteSetting(WinAGISettingsFile);
                            }
                        }
                        else {
                            //if on automatic, always say yes
                            rtn = DialogResult.Yes;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            WordEditor.SaveWords();
                            break;
                        }
                    }
                }
            }
            // default is to replace any existing game files
            rtn = DialogResult.Yes;
            do {
                if (CompGameDir.Length == 0) {
                    //get a new directory
                    MDIMain.FolderDlg.Description = "Choose target directory for compiled game:";
                    MDIMain.FolderDlg.AddToRecent = false;
                    MDIMain.FolderDlg.InitialDirectory = BrowserStartDir;
                    MDIMain.FolderDlg.OkRequiresInteraction = true;
                    MDIMain.FolderDlg.ShowNewFolderButton = true;
                    if (MDIMain.FolderDlg.ShowDialog() == DialogResult.OK) {
                        // ensure trailing backslash
                        string checkDir = FullDir(MDIMain.FolderDlg.SelectedPath);
                        //if directory already contains game files,
                        if (Directory.GetFiles(checkDir, "*VOL.*").Length != 0) {
                            rtn = MessageBox.Show(MDIMain,
                                "This directory already contains AGI game files. Existing files will be renamed so they will not be lost. Continue with compile?",
                                "Compile Game",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question);
                            if (rtn == DialogResult.Yes) {
                                //keep directory
                                CompGameDir = checkDir;
                            }
                        }
                        else {
                            // new directory is ok
                            CompGameDir = checkDir;
                        }
                    }
                    else {
                        rtn = DialogResult.Cancel;
                    }
                }
            } while (rtn == DialogResult.No);
            if (rtn == DialogResult.Cancel) {
                //exit
                return;
            }
            // ensure dir has trailing backslash
            CompGameDir = FullDir(CompGameDir);
            // set browser dir
            BrowserStartDir = CompGameDir;
            MDIMain.UseWaitCursor = true;
            MDIMain.Refresh();
            // show compile status window
            CompStatusWin = new frmCompStatus(RebuildOnly ? CompileMode.RebuildOnly : CompileMode.Full);
            CompStatusWin.StartPosition = FormStartPosition.CenterParent;
            // update status bar to show game is being rebuilt
            MainStatusBar.Items["spStatus"].Text = RebuildOnly ? "Rebuilding game files, please wait..." : "Compiling game, please wait...";
            // pass mode & source
            CompGameResults = new CompileGameResults() {
                Mode = RebuildOnly ? CompileMode.RebuildOnly : CompileMode.Full,
                parm = CompGameDir,
            };
            bgwCompGame.RunWorkerAsync(CompGameResults);
            // show dialog while rebuild is in progress
            CompStatusWin.ShowDialog(MDIMain);
            // TODO: let any open logic editors know to update compiled status
            // before hiding status form? or have CompStatusWin do it in real time? 

            // done with the compile staus form
            CompStatusWin.Dispose();
            CompStatusWin = null;
            // reset cursor
            MDIMain.UseWaitCursor = false;
            MainStatusBar.Items["spStatus"].Text = "";
        }

        internal static bool CompileLogic(frmLogicEdit editor, byte logicnum) {
            // compiles an ingame logic
            // assumes calling function has validated the logic is in fact in a game
            //   if an editor object is also passed, it gets updated based on results
            //   of the compile operation
            //   if no editor, success msg is skipped

            // set flag so compiling doesn't cause unnecessary updates in preview window
            Compiling = true;
            if (editor != null) {
                if (editor.IsChanged) {
                    // first, save the source
                    editor.SaveLogicSource();
                }
            }
            MDIMain.ClearWarnings(AGIResType.Logic, logicnum, [EventType.LogicCompileError, EventType.LogicCompileWarning]);
            // unlike other resources, the ingame logic is referenced directly
            // when being edited; so, it's possible that the logic might get closed
            // such as when changing which logic is being previewed;
            // SO, we need to make sure the logic is loaded BEFORE compiling
            if (EditGame.Logics[logicnum].Loaded) {
                EditGame.Logics[logicnum].Load();
            }
            bool loaded = false;
            try {
                loaded = EditGame.Logics[logicnum].Loaded;
                if (!loaded) {
                    EditGame.Logics[logicnum].Load();
                }
                if (!EditGame.Logics[logicnum].Compile()) {
                    // one or more minor errors
                    if (WinAGISettings.NotifyCompFail.Value) {
                        bool blnDontNotify = false;
                        MsgBoxEx.Show(MDIMain,
                        "One or more syntax errors encountered. Logic was not compiled. Correct the errors and try again.",
                        "Compile Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        "Do not show this message again",
                        ref blnDontNotify,
                        WinAGIHelp,
                        "htm\\winagi\\compilererrors.htm");
                        WinAGISettings.NotifyCompFail.Value = !blnDontNotify;
                        if (!WinAGISettings.NotifyCompFail.Value) {
                            WinAGISettings.NotifyCompFail.WriteSetting(WinAGISettingsFile);
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex) {
                switch (ex.HResult) {
                case WINAGI_ERR + 618:
                    // not in a game
                    // should NEVER get here, but...
                    MessageBox.Show(MDIMain,
                        "Only logics that are in a game can be compiled.",
                        "Compile Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                case WINAGI_ERR + 546:
                    // no data to compile
                    MessageBox.Show(MDIMain,
                        "Nothing to compile!",
                        "Compile Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                case WINAGI_ERR + 698:
                    // unable to write to file (VOL, or DIR)
                    MessageBox.Show(MDIMain,
                        "Error occurred during compilation:\n\n" +
                        "The logic has not been updated. Verify " +
                        "your VOL and DIR files are not marked read-only.",
                        "File Access Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                default:
                    long errNum = ex.HResult - WINAGI_ERR;
                    // some other error
                    ErrMsgBox(ex, "Error occurred during compilation: ", ex.StackTrace, "Compile Error");
                    break;
                }
                return false;
            }
            finally {
                if (loaded) {
                    EditGame.Logics[logicnum].Unload();
                }
            }
            // no error
            if (editor != null) {
                MainStatusBar.Items["spStatus"].Text = ResourceName(EditGame.Logics[logicnum], true, true) + " successfully compiled.";
            }
            // hmm, currently no easy way to tell if there are warnings; 
            //if (warnings) {
            //    if (WinAGISettings.NotifyCompWarn.Value) {
            //        bool blnDontNotify = false;
            //        MsgBoxEx.Show(MDIMain,
            //            "Logic successfully compiled. One or more coditions were found that you may need to double check.",
            //            "Compile Logic",
            //            MessageBoxButtons.OK,
            //            MessageBoxIcon.Information,
            //            "Don't show this message again", ref blnDontNotify);
            //        WinAGISettings.NotifyCompWarn.Value = !blnDontNotify;
            //        if (!WinAGISettings.NotifyCompWarn.Value) {
            //            WinAGISettings.NotifyCompWarn.WriteSetting(WinAGISettingsFile);
            //        }
            //    }
            //}
            //else {
            if (WinAGISettings.NotifyCompSuccess.Value) {
                bool blnDontNotify = false;
                MsgBoxEx.Show(MDIMain,
                    "Logic successfully compiled.",
                    "Compile Logic",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    "Don't show this message again", ref blnDontNotify);
                WinAGISettings.NotifyCompSuccess.Value = !blnDontNotify;
                if (!WinAGISettings.NotifyCompSuccess.Value) {
                    WinAGISettings.NotifyCompSuccess.WriteSetting(WinAGISettingsFile);
                }
            }
            //}
            RefreshTree(AGIResType.Logic, logicnum);
            //all done
            Compiling = false;
            return true;
        }

        public static bool CompileChangedLogics(bool NoMsg = false) {
            DialogResult rtn = DialogResult.Cancel;
            bool blnDontAsk = false;

            // if no game is loaded,
            if (EditGame == null) {
                return false;
            }
            // check for any open logic resources
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == LogicFormMode.Logic) {
                    if (frm.rtfLogic2.IsChanged) {
                        switch (WinAGISettings.SaveOnCompile.Value) {
                        case AskOption.Ask:
                            frm.Select();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this logic before compiling?",
                                "Update " + ResourceName(frm.EditLogic, true, true) + "?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.", ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.Yes;
                                }
                                else if (rtn == DialogResult.No) {
                                    WinAGISettings.SaveOnCompile.Value = AskOption.No;
                                }
                                WinAGISettings.SaveOnCompile.WriteSetting(WinAGISettingsFile);
                            }
                            break;
                        case AskOption.No:
                            rtn = DialogResult.No;
                            break;
                        case AskOption.Yes:
                            rtn = DialogResult.Yes;
                            break;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return false;
                        case DialogResult.Yes:
                            // save it
                            frm.SaveLogicSource();
                            break;
                        }
                    }
                }
            }
            // check for no changed files; nothing to do in that case
            bool nochange = true;
            foreach (Logic logres in EditGame.Logics) {
                if (!logres.Compiled) {
                    nochange = false;
                    break;
                }
            }
            if (nochange) {
                if (NoMsg) {
                    MessageBox.Show(MDIMain,
                        "There are no uncompiled logics in this game.",
                        "No Logics to Compile.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                return true;
            }
            MDIMain.UseWaitCursor = true;
            MDIMain.Refresh();
            // show compile status window
            CompStatusWin = new frmCompStatus(CompileMode.ChangedLogics);
            CompStatusWin.StartPosition = FormStartPosition.CenterParent;
            // update status bar to show game is being rebuilt
            MainStatusBar.Items["spStatus"].Text = "Compiling changed logics, please wait...";
            // pass mode & source
            CompGameResults = new CompileGameResults() {
                Mode = CompileMode.ChangedLogics,
                parm = "",
            };
            bgwCompGame.RunWorkerAsync(CompGameResults);
            // show dialog while rebuild is in progress
            CompStatusWin.ShowDialog(MDIMain);
            // TODO: let any open logic editors know to update compiled status
            // before hiding status form? or have CompStatusWin do it in real time? 

            // done with the compile staus form
            CompStatusWin.Dispose();
            CompStatusWin = null;
            // reset cursor
            MDIMain.UseWaitCursor = false;
            MainStatusBar.Items["spStatus"].Text = "";
            return CompGameResults.Status == CompileStatus.OK;
        }

        public static void NewAGIGame(bool UseTemplate) {
            string strVer = "";
            string strDescription = "";
            string strTemplateDir = "";
            int i;

            frmGameProperties propform = new(GameSettingFunction.New);
            if (UseTemplate) {
                // have user choose a template
                frmTemplates templateform = new();
                if (templateform.lstTemplates.Items.Count == 0) {
                    MessageBox.Show(MDIMain,
                        "There are no templates available. Unable to create new game.",
                        "No Templates Available",
                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                        0, 0, WinAGIHelp, "htm\\winagi\\Templates.htm");
                    templateform.Dispose();
                    return;
                }
                if (templateform.ShowDialog(MDIMain) == DialogResult.OK) {
                    strTemplateDir = Application.StartupPath + "\\Templates\\" + templateform.lstTemplates.Text;
                    strDescription = templateform.txtDescription.Text;
                    strVer = templateform.txtVersion.Text;
                }
                templateform.Dispose();
                if (strTemplateDir.Length == 0) {
                    return;
                }
                // some properties are preset based on template
                propform.cmbVersion.Text = strVer;
                propform.cmbVersion.Enabled = false;
                propform.txtGameDescription.Text = strDescription;
                for (i = 0; i < propform.cmbCodePage.Items.Count; i++) {
                    if (int.Parse(((string)propform.cmbCodePage.Items[i])[..3]) == templateform.CodePage) {
                        propform.cmbCodePage.SelectedIndex = i;
                        break;
                    }
                }
                propform.chkResourceIDs.Checked = templateform.IncludeIDs;
                propform.chkResDefs.Checked = templateform.IncludeReserved;
                propform.chkGlobals.Checked = templateform.IncludeGlobals;
                propform.NewCodePage = templateform.CodePage;
                propform.chkUseLE.Checked = templateform.UseLayoutEd;
                propform.chkSierraSyntax.Checked = templateform.SierraSyntax;
            }
            // now get properties from user
            if (propform.ShowDialog() == DialogResult.OK) {
                //if word or Objects Editor open
                if (EditGame != null) {
                    // close game, if user allows
                    if (!CloseThisGame()) {
                        return;
                    }
                }
                //show wait cursor
                MDIMain.UseWaitCursor = true;
                MDIMain.Refresh();
                // show the progress window
                ProgressWin = new () {
                    Text = "Creating New Game"
                };
                ProgressWin.lblProgress.Text = "Creating new game resources ...";
                ProgressWin.StartPosition = FormStartPosition.CenterParent;
                ProgressWin.pgbStatus.Visible = false;
                // show newgame msg in status bar
                MainStatusBar.Items["spStatus"].Text = "Creating new game" + (UseTemplate ? " from template" : "") + "; please wait...";
                // pass game info and template info
                NewResults = new() {
                    NewID = propform.txtGameID.Text,
                    Version = propform.cmbVersion.Text,
                    GameDir = propform.DisplayDir,
                    ResDir = propform.txtResDir.Text,
                    SrcExt = propform.txtSrcExt.Text,
                    TemplateDir = strTemplateDir,
                    Failed = false,
                    ErrorMsg = "",
                    Warnings = false
                };
                // run the worker to create the new game
                bgwNewGame.RunWorkerAsync(NewResults);
                // idle until the worker is done;
                ProgressWin.ShowDialog(MDIMain);
                // reset cursor
                MDIMain.UseWaitCursor = false;
                // add wag file to mru, if opened successfully
                if (EditGame is not null) {
                    AddToMRU(EditGame.GameFile);
                    // add rest of properties
                    EditGame.GameDescription = propform.txtGameDescription.Text;
                    EditGame.GameAuthor = propform.txtGameAuthor.Text;
                    EditGame.GameVersion = propform.txtGameVersion.Text;
                    EditGame.GameAbout = propform.txtGameAbout.Text;
                    // set platform type if info was provided
                    if (propform.NewPlatformFile.Length > 0) {
                        if (propform.optDosBox.Checked) {
                            EditGame.PlatformType = Engine.PlatformType.DosBox;
                            EditGame.DOSExec = propform.txtExec.Text;
                            EditGame.PlatformOpts = propform.txtOptions.Text;
                        }
                        else if (propform.optScummVM.Checked) {
                            EditGame.PlatformType = Engine.PlatformType.ScummVM;
                            EditGame.PlatformOpts = propform.txtOptions.Text;
                        }
                        else if (propform.optNAGI.Checked) {
                            EditGame.PlatformType = Engine.PlatformType.NAGI;
                        }
                        else if (propform.optOther.Checked) {
                            EditGame.PlatformType = Engine.PlatformType.Other;
                            EditGame.PlatformOpts = propform.txtOptions.Text;
                        }
                    }
                    else {
                        EditGame.PlatformType = Engine.PlatformType.None;
                    }
                    if (EditGame.PlatformType > 0) {
                        EditGame.Platform = propform.NewPlatformFile;
                    }
                    EditGame.IncludeIDs = propform.chkResourceIDs.Checked;
                    EditGame.IncludeReserved = propform.chkResDefs.Checked;
                    EditGame.IncludeGlobals = propform.chkGlobals.Checked;
                    EditGame.UseLE = propform.chkUseLE.Checked;
                    // force a save of the property file
                    WinAGISettingsFile.Save();
                    try {
                        //if there is a layout file
                        if (Directory.GetFiles(EditGame.GameDir + "*.wal").Length == 1) {
                            File.Move(EditGame.GameDir + Directory.GetFiles(EditGame.GameDir + "*.wal")[0], EditGame.GameDir + EditGame.GameID + ".wal");
                        }
                    }
                    catch {
                        // ignore errors
                    }
                    // set default directory
                    BrowserStartDir = EditGame.GameDir;
                    //set default text file directory to game source file directory
                    DefaultResDir = EditGame.GameDir + EditGame.ResDirName + "\\";
                    // build ID lookup table
                    BuildIDefLookup();
                }
                else {
                    //make sure warning grid is hidden
                    if (MDIMain.pnlWarnings.Visible) {
                        MDIMain.HideWarningList(true);
                    }
                }
                UpdateTBGameBtns();
                MainStatusBar.Items["spStatus"].Text = "";
                MDIMain.UseWaitCursor = false;
            }
            propform.Dispose();
            return;
        }

        public static DefineNameCheck ValidateID(string NewID, string OldID) {
            //validates if a resource ID is agreeable or not

            if (NewID == OldID) {
                return DefineNameCheck.OK;
            }
            bool sierrasyntax = EditGame != null && EditGame.SierraSyntax;
            DefineNameCheck retval = BaseNameCheck(NewID, sierrasyntax);
            if (retval != DefineNameCheck.OK) {
                return retval;
            }
            // check against existing IDs
            for (int restype = 0; restype <= 3; restype++) {
                for (int i = 0; i <= 255; i++) {
                    if (IDefLookup[restype, i].Type != ArgType.None) {
                        if (NewID ==IDefLookup[restype, i].Name) {
                            return DefineNameCheck.ResourceID;
                        }
                    }
                }
            }
            return DefineNameCheck.OK;
        }

        public static bool ChangeGameID(string NewID) {
            // validate new id before changing it
            if (EditGame.GameID == NewID || NewID.Length == 0) {
                return false;
            }
            if (NewID.Length > 5) {
                NewID = NewID[..5];
            }
            foreach (char ch in NewID.ToCharArray()) {
                // alphanumeric only
                if (ch < 'A' || ch > 'z' || (ch >= 91 && ch <= 96)) {
                    MessageBox.Show("The specified gameID contains invalid characters. No change made.", "Invalid Game ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            if (Path.GetFileNameWithoutExtension(EditGame.GameFile) == EditGame.GameID) {
                // if RenameWAG is in ask mode or yes mode, get user choice
                DialogResult rtn = DialogResult.Yes;
                bool dontAsk = false;

                switch (WinAGISettings.RenameWAG.Value) {
                case AskOption.Ask:
                    rtn = MsgBoxEx.Show(MDIMain, "Do you want to rename your game file to match the new GameID?",
                                        "Rename Game File",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question,
                    "Always take this action when changing GameID.", ref dontAsk);
                    if (dontAsk) {
                        if (rtn == DialogResult.Yes) {
                            WinAGISettings.RenameWAG.Value = AskOption.Yes;
                        }
                        else if (rtn == DialogResult.No) {
                            WinAGISettings.RenameWAG.Value = AskOption.No;
                        }
                        WinAGISettings.RenameWAG.WriteSetting(WinAGISettingsFile);
                    }
                    break;
                case AskOption.No:
                    rtn = DialogResult.No;
                    break;
                case AskOption.Yes:
                    rtn = DialogResult.Yes;
                    break;
                }
                if (rtn == DialogResult.Yes) {
                    // rename the game file
                    if (File.Exists(EditGame.GameDir + NewID + ".wag")) {
                        if (MessageBox.Show(MDIMain, $"'{EditGame.GameDir + NewID}.wag' already exists. OK to overwrite it?", "Delete Existing File?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) {
                            try {
                                File.Delete(EditGame.GameDir + NewID + ".wag");
                            }
                            catch {
                                MessageBox.Show("Unable to delete existing wag file. GameID is not changed.", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }
                        }
                        else {
                            // cancel
                            return false;
                        }
                    }
                    RenameMRU(EditGame.GameFile, EditGame.GameDir + NewID + ".wag");
                    EditGame.GameFile = EditGame.GameDir + NewID + ".wag";
                }
            }
            // update name of layout file, if it exists (it always should match GameID)
            try {
                File.Move(EditGame.GameDir + EditGame.GameID + ".wal", EditGame.GameDir + NewID + ".wal");
            }
            catch {
                // ignore errors- let user figure it out
            }
            // lastly change id
            EditGame.GameID = NewID;
            //update resource list
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                break;
            case EResListType.ComboList:
                MDIMain.cmbResType.Items[0] = EditGame.GameID;
                break;
            }
            MDIMain.Text = "WinAGI GDS - " + EditGame.GameID;
            return true;
        }

        public static void ChangeResDir(string NewDir) {
            // validate new dir before changing it
            if (string.Compare(EditGame.ResDirName, NewDir, true) == 0 || NewDir.Length == 0) {
                return;
            }
            char[] charlist = Path.GetInvalidFileNameChars();
            foreach (char ch in charlist) {
                if (NewDir.ToCharArray().Contains(ch)) {
                    MessageBox.Show("The specified path contains invalid characters. No change made.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            // check for existing folder
            if (Path.Exists(EditGame.GameDir + NewDir)) {
                MessageBox.Show($"The folder '{NewDir}' already exists. Existing resource direcory cannot be moved. No change made.", "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // try renaming the resource dir
            try {
                Directory.Move(EditGame.ResDir, EditGame.GameDir + NewDir);
                EditGame.ResDirName = NewDir;
            }
            catch (Exception ex) {
                ErrMsgBox(ex, "An exception occurred when trying to move the resource directory.", "No change made.", "Unable to change ResDir");
            }
        }

        public static void RenameMRU(string OldWAGFile, string NewWAGFile) {
            // if NewWAGFile is already in the list,
            // remove it - it will take the place of
            // OldWAGFile

            // if OldWAGFile is NOT on list, add NewWAGFile;
            // otherwise just rename OldWAGFile to NewWAGFile
            int i, j;

            // first look for NewWAGFile, and delete it if found
            for (i = 1; i < 4; i++) {
                if (NewWAGFile == strMRU[i]) {
                    // delete it by moving others up
                    for (j = i; j <= 3; j++) {
                        strMRU[j] = strMRU[j + 1];
                        MDIMain.mnuGame.DropDownItems["mnuGMRU" + j].Text = CompactPath(strMRU[j], 60);
                    }
                    // now delete last entry
                    strMRU[3] = "";
                    MDIMain.mnuGMRU3.Text = "";
                    break;
                }
            }
            // now check for OldWAGFile
            for (i = 0; i < 4; i++) {
                if (strMRU[i] == OldWAGFile) {
                    // rename it
                    strMRU[i] = NewWAGFile;
                    MDIMain.mnuGame.DropDownItems["mnuGMRU" + i].Text = CompactPath(NewWAGFile, 60);
                    break;
                }
            }

            //make sure NewWAGFile is at the top (use AddToMRU function to do this easily!)
            AddToMRU(NewWAGFile);
        }

        public static bool ChangeIntVersion(string NewVersion) {
            if (NewVersion[0] != EditGame.InterpreterVersion[0]) {
                // ask for confirmation
                DialogResult rtn = MessageBox.Show(MDIMain, "Changing the target interpreter version may create problems" + Environment.NewLine +
                             "with your logic resources, due to changes in the number of" + Environment.NewLine +
                             "commands, and their argument counts." + Environment.NewLine + Environment.NewLine +
                             "Also, your DIR and VOL files will need to be rebuilt to" + Environment.NewLine + Environment.NewLine +
                             "Continue with version change?", "Change Interpreter Version", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (rtn == DialogResult.No) {
                    return false;
                }
                // show wait cursor
                MDIMain.UseWaitCursor = true;
                MDIMain.Refresh();
                // show compile status window
                CompStatusWin = new frmCompStatus(CompileMode.RebuildOnly); // rebuild only
                CompStatusWin.StartPosition = FormStartPosition.CenterParent;
                // update status bar to show game is being rebuilt
                MainStatusBar.Items["spStatus"].Text = "Rebuilding game with new game ID, please wait...";
                // pass mode & source
                CompGameResults = new CompileGameResults() {
                    Mode = CompileMode.RebuildOnly,
                    parm = NewVersion,
                };
                bgwCompGame.RunWorkerAsync(CompGameResults);
                // show dialog while rebuild is in progress
                CompStatusWin.ShowDialog(MDIMain);
                // done with the compile staus form
                CompStatusWin.Dispose();
                CompStatusWin = null;
                // reset cursor
                MDIMain.UseWaitCursor = false;
                MainStatusBar.Items["spStatus"].Text = "";
            }
            else {
                // ask for confirmation
                DialogResult rtn = MessageBox.Show("Changing the target interpreter version may create problems with your logic resources, " +
                                      "due to changes in the number of commands, and their argument counts." + Environment.NewLine + Environment.NewLine +
                                      "Continue with version change?", "Change Interpreter Version",
                                      MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (rtn == DialogResult.No) {
                    return false;
                }
                // change the version
                EditGame.InterpreterVersion = NewVersion;
            }
            // check if change in version affected ID
            // TODO: shouldn't need this anymore because ID now limited to 5 characters for 
            // all versions
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                if (MDIMain.tvwResources.Nodes[0].Text != EditGame.GameID) {
                    MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                }
                break;
            case EResListType.ComboList:
                if ((string)MDIMain.cmbResType.Items[0] != EditGame.GameID) {
                    MDIMain.cmbResType.Items[0] = EditGame.GameID;
                }
                break;
            }
            return true;
        }

        /// <summary>
        /// Checks all logics; if the token is in use in a logic, it gets marked as changed.
        /// </summary>
        /// <param name="strToken"></param>
        public static void UpdateReservedToken(string strToken) {
            foreach (Logic tmpLogic in EditGame.Logics) {
                if (!tmpLogic.Loaded) {
                    tmpLogic.Load();
                }
                if (FindWholeWord(0, tmpLogic.SourceText, strToken, true, false, AGIResType.None) != -1) {
                    EditGame.Logics.MarkAsChanged(tmpLogic.Number);
                    RefreshTree(AGIResType.Logic, tmpLogic.Number);
                }
                tmpLogic.Unload();
            }
        }

        public static void UpdateLEStatus() {
            if (!EditGame.UseLE) {
                //if using it, need to close it
                if (LEInUse) {
                    MessageBox.Show(MDIMain,
                          "The current layout editor file will be closed. " + Environment.NewLine + Environment.NewLine +
                            "If you decide to use the layout editor again at a later" + Environment.NewLine +
                            "time, you will need to rebuild the layout to update it. ",
                            "Closing Layout Editor",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information,
                          MessageBoxDefaultButton.Button1,
                          0,
                          WinAGIHelp, @"htm\winagi\Layout_Editor.htm");
                    //save it, if changed
                    if (LayoutEditor != null || LayoutEditor.IsChanged) {
                        LayoutEditor.MenuClickSave();
                    }
                    //close it
                    LayoutEditor.Close();
                    LayoutEditor.Dispose();
                    LEInUse = false;
                }
            }
            // adjust the menubar and toolbar
            MDIMain.mnuTLayout.Enabled = EditGame.UseLE;
            MDIMain.toolStrip1.Items["btnLayoutEd"].Enabled = EditGame.UseLE;
        }

        public static void OpenGlobals(bool ForceLoad = false) {
            string strFileName;
            frmGlobals frmNew;

            // if a game is loaded and NOT forcing...
            // open editor if not yet in use
            // or switch to it if it's already open
            if (EditGame != null && !ForceLoad) {
                if (GEInUse) {
                    GlobalsEditor.Activate();
                    if (GlobalsEditor.WindowState == FormWindowState.Minimized) {
                        // if minimized, restore it
                        GlobalsEditor.WindowState = FormWindowState.Normal;
                    }
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    // use the game's default globals file
                    strFileName = EditGame.ResDir + "globals.txt";
                    // look for global file
                    if (!File.Exists(strFileName)) {
                        // TODO: move defines.txt conversion to the GlobalList object
                        // look for a defines.txt file in the resource directory
                        if (File.Exists(EditGame.ResDir + "defines.txt")) {
                            // copy it to globals.txt
                            try {
                                File.Copy(EditGame.ResDir + "defines.txt", strFileName);
                            }
                            catch {
                                //ignore if error (a new file will be created)
                            }
                        }
                    }
                    //now check again for globals file
                    if (!File.Exists(strFileName)) {
                        // TODO: create blank file and open it
                    }
                    GlobalsEditor = new frmGlobals();
                    if (GlobalsEditor.LoadGlobalDefines(strFileName, true)) {
                        // TODO: deal with errors
                    }
                    GlobalsEditor.Show();
                    GlobalsEditor.Activate();
                    // mark editor as in use
                    GEInUse = true;
                    // reset cursor
                    MDIMain.UseWaitCursor = false;
                }
            }
            else {
                // either a game is NOT loaded, OR we are forcing a load from file
                //get a globals file
                strFileName = GetOpenResourceFilename("Open ", AGIResType.Globals);
                if (strFileName.Length == 0) {
                    return;
                }
                // save filter
                WinAGISettingsFile.WriteSetting("Globals", sOPENFILTER, MDIMain.OpenDlg.FilterIndex);
                DefaultResDir = JustPath(MDIMain.OpenDlg.FileName);

                // check if already open
                foreach (Form tmpForm in MDIMain.MdiChildren) {
                    if (tmpForm.Name == "frmGlobals") {
                        frmGlobals tmpGlobal = tmpForm as frmGlobals;
                        if (tmpGlobal.FileName == strFileName && !tmpGlobal.InGame) {
                            //just shift focus
                            tmpForm.Select();
                            return;
                        }
                    }
                }
                // not open yet; create new form
                // and open this file into it
                MDIMain.UseWaitCursor = true;
                frmNew = new();
                if (frmNew.LoadGlobalDefines(strFileName, false)) {
                    // TODO: handle error
                }
                frmNew.Show();
                frmNew.Activate();
                MDIMain.UseWaitCursor = false;
            }
            return;
        }

        public static byte GetNewNumber(AGIResType ResType, byte OldResNum) {
            byte newnum;
            using frmGetResourceNum frm = new(GetRes.Renumber, ResType, OldResNum);
            if (frm.ShowDialog(MDIMain) != DialogResult.Cancel) {
                newnum = frm.NewResNum;
                if (newnum != OldResNum) {
                    RenumberResource(ResType, OldResNum, newnum);
                }
                return newnum;
            }
            else {
                return OldResNum;
            }
        }

        public static void RenumberRoom(byte OldResNum, byte NewResNum) {
            RenumberResource(AGIResType.Logic, OldResNum, NewResNum);
            RenumberResource(AGIResType.Picture, OldResNum, NewResNum);
        }

        public static void RenumberResource(AGIResType ResType, byte OldResNum, byte NewResNum) {
            // renumbers a resource

            string strCaption = "", newID = "";
            string oldID = "", oldResFile = "";
            int i;

            //change number for this resource
            switch (ResType) {
            case AGIResType.Logic:
                oldID = EditGame.Logics[OldResNum].ID;
                oldResFile = EditGame.ResDir + EditGame.Logics[OldResNum].ID + ".agl";
                EditGame.Logics.Renumber(OldResNum, NewResNum);
                strCaption = ResourceName(EditGame.Logics[NewResNum], true);
                newID = EditGame.Logics[NewResNum].ID;
                break;
            case AGIResType.Picture:
                oldID = EditGame.Pictures[OldResNum].ID;
                oldResFile = EditGame.ResDir + EditGame.Pictures[OldResNum].ID + ".agp";
                EditGame.Pictures.Renumber(OldResNum, NewResNum);
                strCaption = ResourceName(EditGame.Pictures[NewResNum], true);
                newID = EditGame.Pictures[NewResNum].ID;
                break;
            case AGIResType.Sound:
                oldID = EditGame.Sounds[OldResNum].ID;
                oldResFile = EditGame.ResDir + EditGame.Sounds[OldResNum].ID + ".ags";
                EditGame.Sounds.Renumber(OldResNum, NewResNum);
                strCaption = ResourceName(EditGame.Sounds[NewResNum], true);
                newID = EditGame.Sounds[NewResNum].ID;
                break;
            case AGIResType.View:
                oldID = EditGame.Views[OldResNum].ID;
                oldResFile = EditGame.ResDir + EditGame.Views[OldResNum].ID + ".agv";
                EditGame.Views.Renumber(OldResNum, NewResNum);
                strCaption = ResourceName(EditGame.Views[NewResNum], true);
                newID = EditGame.Views[NewResNum].ID;
                break;
            }
            if (oldID != newID) {
                //update resource file if ID has changed
                UpdateResFile(ResType, NewResNum, oldResFile);
            }
            // update resource list
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                int lngPos;
                TreeNode tmpNode = HdrNode[(int)ResType];
                // add in new position
                // (add it before removing current, to minimize changes in resource list)
                for (lngPos = 0; lngPos < tmpNode.Nodes.Count; lngPos++) {
                    if ((byte)tmpNode.Nodes[lngPos].Tag > NewResNum) {
                        break;
                    }
                }
                //add to tree
                tmpNode = tmpNode.Nodes.Insert(lngPos, KeyText(ResType, NewResNum), strCaption);
                tmpNode.Tag = NewResNum;
                if (ResType == AGIResType.Logic) {
                    // highlight in red if not compiled
                    tmpNode.ForeColor = EditGame.Logics[NewResNum].Compiled ? Color.Black : Color.Red;
                }
                if (SelResNum == OldResNum && SelResType == ResType) {
                    // select the new numbered resource
                    // (mark current selection as invalid, since it points to the old
                    // resource, which isn't in the list anymore)
                    SelResNum = -1;
                    if (SelResNum == OldResNum) {
                    }
                    MDIMain.tvwResources.SelectedNode = tmpNode;
                }
                // now remove the old node
                HdrNode[(int)ResType].Nodes[KeyText(ResType, OldResNum)].Remove();
                break;
            case EResListType.ComboList:
                // only update if the resource type is being listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)ResType) {
                    // remove it from current location
                    MDIMain.lstResources.Items[KeyText(ResType, OldResNum)].Remove();
                    ListViewItem tmpListItem;
                    // find a place to insert this resource in the box list
                    for (lngPos = 0; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((byte)MDIMain.lstResources.Items[lngPos].Tag > NewResNum) {
                            break;
                        }
                    }
                    tmpListItem = MDIMain.lstResources.Items.Insert(lngPos, KeyText(ResType, NewResNum), strCaption, 0);
                    tmpListItem.Tag = NewResNum;
                    if (ResType == AGIResType.Logic) {
                        tmpListItem.ForeColor = EditGame.Logics[NewResNum].Compiled ? Color.Black : Color.Red;
                    }
                }
                break;
            }
            //update the logic tooltip lookup table
            IDefLookup[(int)AGIResType.Logic, OldResNum].Name = "";
            IDefLookup[(int)AGIResType.Logic, OldResNum].Type = ArgType.None;
            IDefLookup[(int)AGIResType.Logic, NewResNum].Name = newID;
            IDefLookup[(int)AGIResType.Logic, NewResNum].Type = Num;
            //then let open logic editors know
            if (LogicEditors.Count > 0) {
                for (i = 0; i < LogicEditors.Count; i++) {
                    LogicEditors[i].ListChanged = true;
                }
            }
        }

        private static string KeyText(AGIResType resType, byte newResNum) {
            return resType.ToString().ToLower()[0].ToString() + newResNum;
        }

        public static bool GetNewResID(AGIResType ResType, int ResNum, ref string ResID, ref string Description, bool InGame, int FirstProp) {
            // ResID and Description are passed by ref, because the resource editors
            // need the updated values passed back to them

            string strOldResFile = "", strOldDesc;
            string strOldID;
            bool blnReplace; //used when replacing IDs in logics
            string strErrMsg = "";

            // should never get here with other restypes
            switch (ResType) {
            case Game:
            case Layout:
            case Menu:
            case AGIResType.Globals:
            case Text:
            case AGIResType.None:
                return false;
            }

            // save incoming (old) ID and description
            strOldID = ResID;
            strOldDesc = Description;

            if (InGame) {
                // need to save current ressource filename
                switch (ResType) {
                case AGIResType.Logic:
                    // logic source file handled automatically when ID changes,
                    // this function handles the compiled resource file
                    strOldResFile = EditGame.ResDir + EditGame.Logics[ResNum].ID + ".agl";
                    break;
                case AGIResType.Picture:
                    strOldResFile = EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp";
                    break;
                case AGIResType.Sound:
                    strOldResFile = EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags";
                    break;
                case AGIResType.View:
                    strOldResFile = EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv";
                    break;
                }
            }
            if (ResType == Words || ResType == Objects) {
                // force prop to description
                FirstProp = 2;
            }
            frmEditResourceProperties frmeditprop;
            while (true) {
                // get new values
                frmeditprop = new(ResType, (byte)ResNum, ResID, Description, InGame, FirstProp);
                if (frmeditprop.ShowDialog(MDIMain) == DialogResult.Cancel) {
                    frmeditprop.Dispose();
                    return false;
                }
                //validate return results
                // TODO: why not validate on the form, before returning?
                if (ResType == Objects || ResType == Words) {
                    //only have description, so no need to validate
                    break;
                }
                else {
                    if (strOldID != frmeditprop.NewID) {
                        //validate new id
                        DefineNameCheck rtn = ValidateID(frmeditprop.NewID, strOldID);
                        switch (rtn) {
                        case DefineNameCheck.OK:
                            break;
                        case DefineNameCheck.Empty:
                            strErrMsg = "Resource ID cannot be blank.";
                            break;
                        case DefineNameCheck.Numeric:
                            strErrMsg = "Resource ID cannot be numeric.";
                            break;
                        case DefineNameCheck.ActionCommand:
                            strErrMsg = "'" + frmeditprop.txtID.Text + "' is an AGI command, and cannot be used as a resource ID.";
                            break;
                        case DefineNameCheck.TestCommand:
                            strErrMsg = "'" + frmeditprop.txtID.Text + "' is an AGI test command, and cannot be used as a resource ID.";
                            break;
                        case DefineNameCheck.KeyWord:
                            strErrMsg = "'" + frmeditprop.txtID.Text + "' is a compiler reserved word, and cannot be used as a resource ID.";
                            break;
                        case DefineNameCheck.ArgMarker:
                            strErrMsg = "Resource IDs cannot be argument markers";
                            break;
                        case DefineNameCheck.BadChar:
                            strErrMsg = "Invalid character in resource ID:" + Environment.NewLine + "   !" + QUOTECHAR + "&//()*+,-/:;<=>?[\\]^`{|}~ and spaces" + Environment.NewLine + "are not allowed.";
                            break;
                        case DefineNameCheck.ResourceID:
                            // only enforce if in a game
                            if (InGame) {
                                strErrMsg = "'" + frmeditprop.txtID.Text + "' is already in use as a resource ID.";
                            }
                            else {
                                rtn = DefineNameCheck.OK;
                            }
                            break;
                        }
                        // if there is an error
                        if (rtn != DefineNameCheck.OK) {
                            MessageBox.Show(MDIMain,
                                strErrMsg,
                                "Change Resource ID",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information,
                                0, 0,
                                WinAGIHelp,
                                "htm\\winagi\\Managing Resources.htm#resourceids");
                        }
                        else {
                            switch (ResType) {
                            case AGIResType.Logic:
                                DialogResult result;
                                if (!strOldID.Equals(frmeditprop.NewID, StringComparison.OrdinalIgnoreCase) && File.Exists(EditGame.ResDir + frmeditprop.NewID + "." + EditGame.SourceExt)) {
                                    // import existing, or overwrite it?
                                    result = MessageBox.Show(MDIMain,
                                       "There is already a source file with the name '" + frmeditprop.NewID + "." +
                                         EditGame.SourceExt + "' in your source file directory.\n\n" +
                                         "Do you want to import that file? Choose 'NO' to replace that file with the current logic source.",
                                         "Import Existing Source File?",
                                         MessageBoxButtons.YesNo);
                                }
                                else {
                                    // no existing file, so keep current source
                                    result = DialogResult.No;
                                }
                                if (result == DialogResult.Yes) {
                                    // bit of a hack..
                                    // move the new file to the old file
                                    // then ID change will move it back
                                    SafeFileMove(EditGame.ResDir + frmeditprop.NewID + "." + EditGame.SourceExt, EditGame.Logics[ResNum].SourceFile, true);
                                }
                                // change id (which automatically renames source file,
                                // including overwriting an existing file
                                EditGame.Logics[ResNum].ID = frmeditprop.NewID;
                                break;
                            case AGIResType.Picture:
                                EditGame.Pictures[ResNum].ID = frmeditprop.NewID;
                                break;
                            case AGIResType.Sound:
                                EditGame.Sounds[ResNum].ID = frmeditprop.NewID;
                                break;
                            case AGIResType.View:
                                EditGame.Views[ResNum].ID = frmeditprop.NewID;
                                break;
                            }
                            break;
                        }
                    }
                    else {
                        //if ID was exactly the same, no change needed
                        break;
                    }
                }
            }
            // id change is acceptable (or it didn't change)
            // return new id
            if (strOldID != frmeditprop.NewID) {
                ResID = frmeditprop.NewID;
                blnReplace = DefUpdateVal = frmeditprop.chkUpdate.Checked;
                // for ingame resources, update resource files, preview, treelist
                if (InGame) {
                    // update the logic tooltip lookup table for log/pic/view/snd
                    switch (ResType) {
                    case AGIResType.Logic:
                    case AGIResType.Picture:
                    case AGIResType.Sound:
                    case AGIResType.View:
                        IDefLookup[(int)ResType, ResNum].Name = ResID;

                        // if not just a change in text case
                        if (!strOldID.Equals(ResID, StringComparison.OrdinalIgnoreCase)) {
                            //update resource file if ID has changed
                            UpdateResFile(ResType, (byte)ResNum, strOldResFile);
                            // logic ID change may result in an import so refresh preview if active
                            if (SelResType == AGIResType.Logic && SelResNum == ResNum) {
                                if (WinAGISettings.ShowPreview.Value) {
                                    PreviewWin.LoadPreview(AGIResType.Logic, ResNum);
                                }
                            }
                        }
                        else {
                            // just change the filename
                            switch (ResType) {
                            case AGIResType.Logic:
                                SafeFileMove(strOldResFile, EditGame.ResDir + ResID + "." + EditGame.SourceExt, true);
                                break;
                            case AGIResType.Picture:
                                SafeFileMove(strOldResFile, EditGame.ResDir + ResID + ".agp", true);
                                break;
                            case AGIResType.Sound:
                                SafeFileMove(strOldResFile, EditGame.ResDir + ResID + ".ags", true);
                                break;
                            case AGIResType.View:
                                SafeFileMove(strOldResFile, EditGame.ResDir + ResID + ".agv", true);
                                break;
                            }
                        }
                        // then update resource list
                        RefreshTree(ResType, ResNum);
                        // set any open logics deflist flag to force a rebuild
                        foreach (frmLogicEdit frm in LogicEditors) {
                            if (frm.Name == "frmLogicEdit") {
                                if (frm.InGame) {
                                    frm.ListChanged = true;
                                }
                            }
                        }
                        // if OK to update in all logics, do so
                        if (blnReplace) {
                            // TODO: add find/replace
                            FindingForm.ResetSearch();
                            ReplaceAll(MDIMain, strOldID, ResID, FindDirection.All, true, true, FindLocation.All, ResType);
                        }
                        break;
                    }
                    // refresh the property page if visible
                    if (MDIMain.propertyGrid1.Visible) {
                        MDIMain.propertyGrid1.Refresh();
                    }
                }
            }
            // if description changed, update it
            if (strOldDesc != frmeditprop.NewDescription) {
                Description = frmeditprop.NewDescription;
                if (InGame) {
                    //update the description
                    switch (ResType) {
                    case AGIResType.Logic:
                        EditGame.Logics[ResNum].Description = Description;
                        break;
                    case AGIResType.Picture:
                        EditGame.Pictures[ResNum].Description = Description;
                        break;
                    case AGIResType.Sound:
                        EditGame.Sounds[ResNum].Description = Description;
                        break;
                    case AGIResType.View:
                        EditGame.Views[ResNum].Description = Description;
                        break;
                    case Words:
                        EditGame.WordList.Description = Description;
                        break;
                    case Objects:
                        EditGame.InvObjects.Description = Description;
                        break;
                    }
                }
            }
            frmeditprop.Dispose();
            return true;
        }

        public static string NewSourceName(Logic ThisLogic, bool InGame) {
            // this ONLY gets a new name; it does not change
            // anything; not the ID, not the source file name;
            // calling function has to use the name given
            // here to do whatever is necessary to actually
            // save and update a logic source file and/or editor

            // there isn't an 'ExportLogicSource' method, because
            // managing source code separate from the actual
            // logic resource is tricky; it's easier for
            // the logic editor and preview window to manage
            // exporting source separately
            //
            // but they both need a name, and that's easy enough
            // to do as a separate function

            DialogResult rtn;

            if (InGame) {
                MDIMain.SaveDlg.Title = "Export Source";
                MDIMain.SaveDlg.DefaultExt = EditGame.SourceExt;
            }
            else {
                MDIMain.SaveDlg.Title = "Save Source";
                MDIMain.SaveDlg.DefaultExt = WinAGISettings.DefaultExt.Value;
            }
            if (ThisLogic.SourceFile.Length != 0) {
                MDIMain.SaveDlg.FileName = Path.GetFileName(ThisLogic.SourceFile);
                MDIMain.SaveDlg.InitialDirectory = Path.GetDirectoryName(ThisLogic.SourceFile);
            }
            else {
                if (InGame) {
                    MDIMain.SaveDlg.FileName = Path.GetFileName(EditGame.ResDir + ThisLogic.ID + "." + EditGame.SourceExt);
                }
                else {
                    // non-game IDs are filenames
                    MDIMain.SaveDlg.FileName = Path.GetFileName(DefaultResDir + ThisLogic.ID);
                }
                MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            }
            string defext;
            if (EditGame == null) {
                defext = WinAGISettings.DefaultExt.Value;
            }
            else {
                defext = EditGame.SourceExt;
            }
            string textext;
            if (defext == "txt") {
                textext = "";
            }
            else {
                textext = "|Text files (*.txt)|*.txt";
            }
            MDIMain.SaveDlg.Filter = $"WinAGI Logic Source Files (*.{defext})|*.{defext}{textext}|All files (*.*)|*.*";
            if (MDIMain.SaveDlg.FileName.Right(4).ToLower() == ".txt") {
                MDIMain.SaveDlg.FilterIndex = 2;
            }
            else {
                MDIMain.SaveDlg.FilterIndex = 1;
            }
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.ExpandedMode = true;
            MDIMain.SaveDlg.ShowHiddenFiles = false;
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.OkRequiresInteraction = true;
            rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);
            if (rtn == DialogResult.Cancel) {
                // nothing selected
                return "";
            }
            DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
            return MDIMain.SaveDlg.FileName;
        }

        public static string NewResourceName(AGIResource resource, bool InGame) {
            DialogResult rtn;
            string filename;

            if (InGame) {
                MDIMain.SaveDlg.Title = "Export " + resource.ResType.ToString() + " Resource";
            }
            else {
                MDIMain.SaveDlg.Title = "Save " + resource.ResType.ToString() + " Resource As";
            }
            if (resource.ResFile.Length > 0) {
                MDIMain.SaveDlg.FileName = Path.GetFileName(resource.ResFile);
            }
            else {
                MDIMain.SaveDlg.FileName = Path.GetFileName(DefaultResDir + resource.ID);
            }
            switch (resource.ResType) {
            case AGIResType.Logic:
                MDIMain.SaveDlg.Filter = "WinAGI Logic Resource Files (*.agl)|*.agl|All files (*.*)|*.*";
                MDIMain.SaveDlg.FilterIndex = 0;
                MDIMain.SaveDlg.DefaultExt = "agl";
                //MDIMain.SaveDlg.FileName += ".agl";
                break;
            case AGIResType.Picture:
                MDIMain.SaveDlg.Filter = "WinAGI Picture Resource Files (*.agp)|*.agp|All files (*.*)|*.*";
                MDIMain.SaveDlg.FilterIndex = 0;
                MDIMain.SaveDlg.DefaultExt = "agp";
                //MDIMain.SaveDlg.FileName += ".agp";
                break;
            case AGIResType.Sound:
                MDIMain.SaveDlg.Filter = "WinAGI Sound Resource Files (*.ags)|*.ags|All files (*.*)|*.*";
                MDIMain.SaveDlg.FilterIndex = 0;
                MDIMain.SaveDlg.DefaultExt = "ags";
                //MDIMain.SaveDlg.FileName += ".ags";
                break;
            case AGIResType.View:
                MDIMain.SaveDlg.Filter = "WinAGI View Resource Files (*.agv)|*.agv|All files (*.*)|*.*";
                MDIMain.SaveDlg.FilterIndex = 0;
                MDIMain.SaveDlg.DefaultExt = "agv";
                //MDIMain.SaveDlg.FileName += ".agv";
                break;
            }
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.ExpandedMode = true;
            MDIMain.SaveDlg.ShowHiddenFiles = false;
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);
            if (rtn == DialogResult.Cancel) {
                // nothing selected
                return "";
            }
            filename = MDIMain.SaveDlg.FileName;
            return filename;
        }

        public static void UpdateResFile(AGIResType ResType, byte ResNum, string OldFileName) {
            switch (ResType) {
            case AGIResType.Logic:
                // logic sourcefile already handled by ID change
                SafeFileMove(EditGame.ResDir + EditGame.Logics[ResNum].ID + ".agl", EditGame.ResDir + EditGame.Logics[ResNum].ID + "_OLD" + ".agl", true);
                try {
                    if (File.Exists(OldFileName)) {
                        File.Move(OldFileName, EditGame.ResDir + EditGame.Logics[ResNum].ID + ".agl");
                    }
                    else {
                        if (WinAGISettings.AutoExport.Value) {
                            EditGame.Logics[ResNum].Export(EditGame.ResDir + EditGame.Logics[ResNum].ID + ".agl");
                        }
                    }
                }
                catch (Exception e) {
                    // something went wrong
                    ErrMsgBox(e, "Unable to update Logic Resource File", "", "Update Logic ID Error");
                }
                if (LEInUse) {
                    // redraw to ensure correct ID is displayed
                    LayoutEditor.DrawLayout();
                }
                break;
            case AGIResType.Picture:
                SafeFileMove(EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp", EditGame.ResDir + EditGame.Pictures[ResNum].ID + "_OLD" + ".agp", true);
                try {
                    if (File.Exists(OldFileName)) {
                        File.Move(OldFileName, EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp");
                    }
                    else {
                        if (WinAGISettings.AutoExport.Value) {
                            EditGame.Pictures[ResNum].Export(EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp");
                        }
                    }
                }
                catch (Exception e) {
                    // something went wrong
                    ErrMsgBox(e, "Unable to update Picture Resource File", "", "Update Picture ID Error");
                }
                break;
            case AGIResType.Sound:
                SafeFileMove(EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags", EditGame.ResDir + EditGame.Sounds[ResNum].ID + "_OLD" + ".ags", true);
                try {
                    if (File.Exists(OldFileName)) {
                        File.Move(OldFileName, EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags");
                    }
                    else {
                        if (WinAGISettings.AutoExport.Value) {
                            EditGame.Sounds[ResNum].Export(EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags");
                        }
                    }
                }
                catch (Exception e) {
                    // something went wrong
                    ErrMsgBox(e, "Unable to update Sound Resource File", "", "Update Sound ID Error");
                }
                break;
            case AGIResType.View:
                SafeFileMove(EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv", EditGame.ResDir + EditGame.Views[ResNum].ID + "_OLD" + ".agv", true);
                try {
                    // if file already exists, rename it,
                    // otherwise use export to create it
                    if (File.Exists(OldFileName)) {
                        File.Move(OldFileName, EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv", true);
                    }
                    else {
                        if (WinAGISettings.AutoExport.Value) {
                            EditGame.Views[ResNum].Export(EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv");
                        }
                    }
                }
                catch (Exception e) {
                    // something went wrong
                    ErrMsgBox(e, "Unable to update View Resource File", "", "Update View ID Error");
                }
                break;
            }
        }

        public static void UpdateExitInfo(UpdateReason Reason, int LogicNumber, Logic ThisLogic, int NewNum = 0) {
            /*
          //   frmMDIMain|SelectedItemRenumber:  UpdateExitInfo euRenumberRoom, OldResNum, null, NewResNum
          //  frmMDIMain|lstProperty_LostFocus:  UpdateExitInfo Reason, SelResNum, Logics(SelResNum) //showroom or removeroom
          //frmMDIMain|picProperties_MouseDown:  UpdateExitInfo Reason, SelResNum, Logics(SelResNum) //showroom or removeroom
          //                ResMan|RemoveLogic:  UpdateExitInfo euRemoveRoom, LogicNum, null
          //        frmLogicEdit|MenuClickSave:  UpdateExitInfo euUpdateRoom, EditLogic.Number, EditLogic
          //    frmLogicEdit|MenuClickRenumber:  UpdateExitInfo euRenumberRoom, OldResNum, null, NewResNum

          //updates the layout editor (if it is open) and the layout file
          //(if there is one) whenever exit info for a room is changed
          //(including when IsRoom property is changed, or when a room is
          //deleted from the game)

          AGIExits tmpExits;
          bool blnSave, blnShow;

          On Error GoTo ErrHandler

          //is there an existing layout editor file?
          blnSave = File.Exists(GameDir + GameID + ".wal")

          //if layout file does not exist AND not editing layout
          if (!blnSave && !LEInUse) {
          //no file, and editor is not in use;
          //no updates are necessary
          return;
          }

          //if adding new room, showing existing room, or updating an existing room,
          if (Reason = euAddRoom || Reason = euShowRoom || Reason = euUpdateRoom) {
          //get new exits from the logic that was passed
          tmpExits = ExtractExits(ThisLogic)
          }

          //if a layout file exists, it needs to be updated too
          if (blnSave) {
          //add line to output file
          UpdateLayoutFile Reason, LogicNumber, tmpExits, NewNum
          }

          //if layout editor is open
          if (LEInUse) {
          //use layout editor update method
          LayoutEditor.UpdateLayout Reason, LogicNumber, tmpExits
          //and redraw to refresh the editor
          LayoutEditor.DrawLayout true
          }
          return;

          ErrHandler:
          Resume Next
          }

          public static AGIExits ParseExits(string strExitInfo, string strVer)
          //parses the string that contains exit info that comes from the layout editor
          //data file
          //if error is encountered,

          string[] strExit;
           string[] strData;
          int i, offset, Size;

          ParseExits = New AGIExits

          On Error GoTo ErrHandler

          //ver 10,11: R|##|v|o|x|y|index:room:reason:style:xfer:leg:spx:spy:epx:epy|...
          //ver 12:    R|##|v|o|p|x|y|index:room:reason:style:xfer:leg|...

          switch (strVer
          case "10", "11"
          offset = 6
          Size = 9
          case "12", "21"
          offset = 7
          Size = 5
          }

          strData = Split(strExitInfo, "|")

          //get exit info
          For i = 0 To UBound(strData) - offset
          strExit = Split(strData(i + offset), ":")
          //ver 10 and 11 have ten elements,
          //ver 12 needs just 6
          if (UBound(strExit) = Size) {

            //add new exit, and flag as new, in source
            ParseExits.Add(strExit(0), strExit(1), strExit(2), strExit(3), strExit(4), strExit(5)).Status = esOK

          //      With ParseExits.Item(ParseExits.Count - 1)
          //        .SPX = CSng(strExit(6))
          //        .SPY = CSng(strExit(7))
          //        .EPX = CSng(strExit(8))
          //        .EPY = CSng(strExit(9))
          //      End With

          }
          Next i
          Exit Function

          ErrHandler:
          //error!
          //Debug.Assert false
          Resume Next
          ParseExits = null
            */
        }

        public static void AddNewLogic(byte NewLogicNumber, Logic NewLogic) {
            // TODO: what if importing a compiled logic? maybe don't allow it anymore?

            EditGame.Logics.Add((byte)NewLogicNumber, NewLogic);
            EditGame.Logics[NewLogicNumber].SaveProps();
            // always save source to new name
            EditGame.Logics[NewLogicNumber].SaveSource();
            //if using layout editor AND isroom
            if (EditGame.UseLE && EditGame.Logics[NewLogicNumber].IsRoom) {
                // update layout editor and layout data file to show this room is in the game
                UpdateExitInfo(UpdateReason.AddRoom, NewLogicNumber, EditGame.Logics[NewLogicNumber]);
            }
            MDIMain.AddResourceToList(AGIResType.Logic, NewLogicNumber);
            // unload it once all done getting it added
            EditGame.Logics[NewLogicNumber].Unload();
        }

        public static void AddNewPicture(byte NewPictureNumber, Picture NewPicture) {
            EditGame.Pictures.Add((byte)NewPictureNumber, NewPicture);
            EditGame.Pictures[NewPictureNumber].SaveProps();
            MDIMain.AddResourceToList(AGIResType.Picture, NewPictureNumber);
            EditGame.Pictures[NewPictureNumber].Unload();
        }

        public static void AddNewSound(byte NewSoundNumber, Sound NewSound) {
            EditGame.Sounds.Add((byte)NewSoundNumber, NewSound);
            EditGame.Sounds[NewSoundNumber].SaveProps();
            MDIMain.AddResourceToList(AGIResType.Sound, NewSoundNumber);
            EditGame.Sounds[NewSoundNumber].Unload();
        }

        public static void AddNewView(byte NewViewNumber, Engine.View NewView) {
            EditGame.Views.Add((byte)NewViewNumber, NewView);
            EditGame.Views[NewViewNumber].SaveProps();
            MDIMain.AddResourceToList(AGIResType.View, NewViewNumber);
            EditGame.Views[NewViewNumber].Unload();
        }

        public static void NewLogic() {
            NewLogic("");
        }

        public static void NewLogic(string ImportLogicFile) {
            // if game is open, give player option to add new logic to game
            // or create a new standalone logic
            //
            // if no game open, just create a new standalone logic

            // creates a new logic resource and opens an editor
            frmLogicEdit frmNew;
            Logic tmpLogic;
            bool blnOpen = false, blnImporting = false;
            bool blnSource = false;
            string strFile = "";

            MDIMain.UseWaitCursor = true;
            // create temporary logic
            tmpLogic = new Logic();
            if (ImportLogicFile.Length != 0) {
                blnImporting = true;
                // open file to see if it is sourcecode or compiled logic
                try {
                    using FileStream fsNewLog = new(ImportLogicFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using StreamReader srNewLog = new(fsNewLog);
                    strFile = srNewLog.ReadToEnd();
                    srNewLog.Dispose();
                    fsNewLog.Dispose();
                }
                catch (Exception) {
                    // ignore errors; import method will have to handle it
                }
                // check if logic is a compiled logic:
                // (check for existence of characters <8)
                string lChars = "";
                for (int i = 1; i <= 8; i++) {
                    lChars += ((char)i).ToString();
                }
                blnSource = !strFile.Any(lChars.Contains);
                // import the logic
                // (and check for error)
                try {
                    if (blnSource) {
                        tmpLogic.ImportSource(ImportLogicFile);
                    }
                    else {
                        tmpLogic.Import(ImportLogicFile);
                    }
                }
                catch (Exception e) {
                    // if a compile error occurred,
                    if (e.HResult == WINAGI_ERR + 567) {
                        // can't open this resource
                        MDIMain.UseWaitCursor = false;
                        ErrMsgBox(e, "An error occurred while trying to decompile this logic resource:", "Unable to open this logic.", "Invalid Logic Resource");
                        // restore main form mousepointer and exit
                        return;
                    }
                    else {
                        // maybe we assumed source status incorrectly- try again
                        try {
                            if (blnSource) {
                                tmpLogic.Import(ImportLogicFile);
                            }
                            else {
                                tmpLogic.ImportSource(ImportLogicFile);
                            }
                        }
                        catch (Exception) {
                            // if STILL error, something wrong
                            MDIMain.UseWaitCursor = false;
                            ErrMsgBox(e, "Unable to load this logic resource. It can't be decompiled, and does not appear to be a text file.", "", "Invalid Logic Resource");
                            return;
                        }
                    }
                }
                if (tmpLogic.SrcErrLevel < 0) {
                    MDIMain.UseWaitCursor = true;
                    switch (tmpLogic.SrcErrLevel) {
                    case -1:
                        MessageBox.Show(MDIMain,
                            "Unable to access this logic source file, file not found.",
                            "Missing Source File",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        break;
                    case -2:
                        MessageBox.Show(MDIMain,
                            "This logic source file is marked 'readonly'. WinAGI requires write-access to edit source files.",
                            "Read only Files not Allowed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        break;
                    case -3:
                        MessageBox.Show(MDIMain,
                            "A file access error has occurred. Unable to read this file.",
                            "File Access Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        break;
                    }
                    return;
                }
            }
            if (EditGame != null) {
                // get logic number, id , description
                using frmGetResourceNum GetResNum = new(blnImporting ? GetRes.Import : GetRes.AddNew, AGIResType.Logic);
                if (blnImporting) {
                    // suggest ID based on filename
                    GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportLogicFile).Replace(" ", "");
                }
                // restore cursor while getting resnum
                MDIMain.UseWaitCursor = false;
                // if canceled, release the temporary logic, restore mousepointer and exit
                if (GetResNum.ShowDialog(MDIMain) == DialogResult.Cancel) {
                    tmpLogic = null;
                    GetResNum.Dispose();
                    return;
                }
                tmpLogic.Description = GetResNum.txtDescription.Text;
                if (GetResNum.DontImport) {
                    blnOpen = true;
                    // not adding to game; still allowed to use template
                    if (GetResNum.chkRoom.Checked) {
                        // add template text
                        tmpLogic.SourceText = LogTemplateText(GetResNum.txtID.Text, GetResNum.txtDescription.Text);
                    }
                    else {
                        // add default text
                        StringList src =
                        [
                            "[*********************************************************************",
                            "[",
                            "[ " + tmpLogic.ID,
                            "[",
                            "[*********************************************************************",
                            "",
                            "return();",
                            "",
                            "[***************************************",
                            "[ DECLARED MESSAGES",
                            "[***************************************",
                            "[  declared messages go here"
                        ];
                        tmpLogic.SourceText = string.Join(NEWLINE, [.. src]);
                    }
                }
                else {
                    // show wait cursor while resource is added
                    MDIMain.UseWaitCursor = true;
                    // update id for ingame resources
                    tmpLogic.ID = GetResNum.txtID.Text;
                    bool blnTemplate = GetResNum.chkRoom.Checked;
                    if (!blnImporting) {
                        string strLogic;
                        //if not importing, we need to add boilerplate text
                        if (blnTemplate) {
                            // add template text to logic source
                            bool changed = false;
                            strLogic = CheckIncludes(LogTemplateText(tmpLogic.ID, tmpLogic.Description), EditGame, ref changed);
                        }
                        else {
                            //add default text
                            StringList src =
                            [
                                "[*********************************************************************",
                                "[",
                                "[ " + tmpLogic.ID,
                                "[",
                                "[*********************************************************************",
                            ];
                            // add standard include files
                            if (EditGame.IncludeIDs) {
                                src.Add("#include \"resourceids.txt\"");
                            }
                            if (EditGame.IncludeReserved) {
                                src.Add("#include \"reserved.txt\"");
                            }
                            if (EditGame.IncludeGlobals) {
                                src.Add("#include \"globals.txt\"");
                            }
                            src.Add("");
                            src.Add("return();");
                            src.Add("");
                            src.Add("[***************************************");
                            src.Add("[ DECLARED MESSAGES");
                            src.Add("[***************************************");
                            src.Add("[  declared messages go here");
                            strLogic = string.Join(NEWLINE, [.. src]);
                        }
                        //for new resources, need to set the source text
                        tmpLogic.SourceText = strLogic;
                    }
                    // set isroom status based on template
                    tmpLogic.IsRoom = GetResNum.NewResNum == 0 ? false : blnTemplate;
                    //add Logic
                    AddNewLogic(GetResNum.NewResNum, tmpLogic);
                    // reset tmplogic to point to the new game logic
                    tmpLogic = EditGame.Logics[GetResNum.NewResNum];

                    // if including picture
                    if (GetResNum.chkIncludePic.Checked) {
                        Picture tmpPic = new();
                        // help user out if they chose a naming scheme
                        if (GetResNum.txtID.Text.Length >= 3 && GetResNum.txtID.Text[..2].Equals("rm", StringComparison.OrdinalIgnoreCase)) {
                            // change ID (if able)
                            if (ValidateID("pic" + GetResNum.txtID.Text[2..], "") == 0) {
                                if (EditGame.Pictures.Contains(GetResNum.NewResNum)) {
                                    // save old resfile name
                                    strFile = EditGame.ResDir + EditGame.Pictures[GetResNum.NewResNum].ID + ".agp";
                                }
                                tmpPic.ID = "pic" + GetResNum.txtID.Text[2..];
                                if (EditGame.Pictures.Contains(GetResNum.NewResNum)) {
                                    UpdateResFile(AGIResType.Picture, GetResNum.NewResNum, strFile);
                                }
                            }
                            else {
                                tmpPic.ID = "Picture" + GetResNum.NewResNum;
                            }
                        }
                        else {
                            tmpPic.ID = "Picture" + GetResNum.NewResNum;
                        }
                        // if replacing an existing pic
                        if (EditGame.Pictures.Contains(GetResNum.NewResNum)) {
                            RemovePicture(GetResNum.NewResNum);
                        }
                        AddNewPicture(GetResNum.NewResNum, tmpPic);
                    }
                    blnOpen = (GetResNum.chkOpenRes.Checked);
                }
            }
            else {
                blnOpen = true;
                // TODO: should there be an option to use template for logics
                // when no game is open?
            }
            // only open if user wants it open (or if not in a game or if opening/not importing)
            if (blnOpen) {
                frmNew = new(LogicFormMode.Logic);
                // pass the logic to the editor
                if (frmNew.LoadLogic(tmpLogic)) {
                    frmNew.Show();
                    LogicEditors.Add(frmNew);
                }
                else {
                    // TODO: handle error
                    MessageBox.Show(MDIMain, "error, can't open this logic");
                    frmNew.Close();
                    frmNew.Dispose();
                }
            }
            WinAGISettings.OpenNew.Value = blnOpen;
            MDIMain.UseWaitCursor = false;
        }

        public static void NewTextFile() {
            MDIMain.UseWaitCursor = true;
            // open a new text file editing window
            frmLogicEdit frmNew = new(LogicFormMode.Text);
            if (frmNew.LoadText("")) {
                frmNew.Show();
            }
            else {
                // TODO: add error handler
                MessageBox.Show(MDIMain, "error, unable to create new text file");
                frmNew.Close();
                frmNew.Dispose();
            }
            MDIMain.UseWaitCursor = false;
        }

        public static bool OpenTextFile(string filename, bool quiet = false) {
            MDIMain.UseWaitCursor = true;
            for (int i = 0; i < LogicEditors.Count; i++) {
                if (LogicEditors[i].FormMode == LogicFormMode.Text && LogicEditors[i].TextFilename == filename) {
                    // alreay open
                    if (LogicEditors[i].WindowState == FormWindowState.Minimized) {
                        LogicEditors[i].WindowState = FormWindowState.Normal;
                    }
                    LogicEditors[i].BringToFront();
                    LogicEditors[i].Select();
                    MDIMain.UseWaitCursor = false;
                    return true;
                }
            }

            frmLogicEdit frmNew = new(LogicFormMode.Text);
            if (frmNew.LoadText(filename)) {
                frmNew.Show();
                LogicEditors.Add(frmNew);
            }
            else {
                // TODO: add error handler
                if (!quiet) {
                    MessageBox.Show(MDIMain, "error, unable to open text file");
                }
                frmNew.Close();
                frmNew.Dispose();
                return false;
            }
            MDIMain.UseWaitCursor = false;
            return true;
        }

        public static void NewPicture() {
            NewPicture("");
        }

        public static void NewPicture(string ImportPictureFile) {
            //creates a new picture resource and optionally opens an editor
            frmPicEdit frmNew;
            Picture tmpPic;
            bool blnOpen = false, blnImporting = false;

            MDIMain.UseWaitCursor = true;
            tmpPic = new Picture();
            if (ImportPictureFile.Length != 0) {
                blnImporting = true;
                try {
                    tmpPic.Import(ImportPictureFile);
                }
                catch (Exception e) {
                    MDIMain.UseWaitCursor = false;
                    ErrMsgBox(e, "Error while importing picture:", "Unable to load this picture resource.", "Import Picture Error");
                    return;
                }
                // now check to see if it's a valid picture resource (by trying to reload it)
                tmpPic.Load();
                if (tmpPic.ErrLevel < 0) {
                    ErrMsgBox(tmpPic.ErrLevel, "Error reading Picture data:", "This is not a valid picture resource.", "Invalid Picture Resource");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            if (EditGame != null) {
                frmGetResourceNum GetResNum = new(blnImporting ? GetRes.Import : GetRes.AddNew, AGIResType.Picture);
                if (blnImporting) {
                    GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportPictureFile).Replace(" ", "");
                }
                MDIMain.UseWaitCursor = false;
                if (GetResNum.ShowDialog(MDIMain) == DialogResult.Cancel) {
                    // restore mousepointer and exit
                    GetResNum.Close();
                    GetResNum.Dispose();
                    return;
                }
                tmpPic.Description = GetResNum.txtDescription.Text;
                if (GetResNum.DontImport) {
                    blnOpen = true;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    tmpPic.ID = GetResNum.txtID.Text;
                    AddNewPicture(GetResNum.NewResNum, tmpPic);
                    tmpPic = EditGame.Pictures[GetResNum.NewResNum];
                    blnOpen = (GetResNum.chkOpenRes.Checked);
                }
                GetResNum.Dispose();
            }
            else {
                blnOpen = true;
            }
            // only open if user wants it open (or if not in a game or if opening/not importing)
            if (blnOpen) {
                frmNew = new frmPicEdit();
                if (frmNew.LoadPicture(tmpPic)) {
                    frmNew.Show();
                    PictureEditors.Add(frmNew);
                }
                else {
                    // TODO: handle error
                    MessageBox.Show(MDIMain, "error, can't open this picture");
                    frmNew.Close();
                    frmNew.Dispose();
                }
            }
            WinAGISettings.OpenNew.Value = blnOpen;
            MDIMain.UseWaitCursor = false;
        }

        public static void NewSound() {
            NewSound("");
        }

        public static void NewSound(string ImportSoundFile) {
            // creates a new sound resource and opens an editor
            frmSoundEdit frmNew;
            Sound tmpSound;
            bool blnOpen = false, blnImporting = false;

            MDIMain.UseWaitCursor = true;
            tmpSound = new Sound();
            if (ImportSoundFile.Length != 0) {
                blnImporting = true;
                // determine if sound is a script or an AGI sound resource
                string filedata = "";
                bool isAGI = true;
                try {
                    using FileStream fsSnd = new(ImportSoundFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using StreamReader srSnd = new(fsSnd);
                    filedata = srSnd.ReadToEnd();
                    fsSnd.Dispose();
                    srSnd.Dispose();
                }
                catch (Exception e) {
                    ErrMsgBox(e, "Error occurred while importing sound:", "Unable to load this sound resource", "Import Sound Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                if (filedata.Contains("tone") && filedata.Contains("noise")) {
                    isAGI = false;
                }
                try {
                    if (isAGI) {
                        tmpSound.Import(ImportSoundFile);
                    }
                    else {
                        tmpSound.ImportScript(ImportSoundFile);
                    }
                }
                catch (Exception e) {
                    ErrMsgBox(e, "Error occurred while importing sound:", "Unable to load this sound resource", "Import Sound Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                // now check to see if it's a valid sound resource (by trying to reload it)
                tmpSound.Load();
                if (tmpSound.ErrLevel < 0) {
                    ErrMsgBox(tmpSound.ErrLevel, "Error reading Sound data:", "This is not a valid sound resource.", "Invalid Sound Resource");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                // only PC sounds are editable
                if (tmpSound.SndFormat != SoundFormat.AGI) {
                    if (MessageBox.Show(MDIMain,
                        "This sound is an Apple IIgs sound.\n\nYou can still add it to your game, but you will not be able to edit it.\n\nDo you want to continue?.",
                        "Unsupported Sound Format",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question) == DialogResult.Cancel) {
                        MDIMain.UseWaitCursor = false;
                        return;
                    }
                }
            }
            else {
                // default instrument settings
                tmpSound.Tracks[0].Instrument = WinAGISettings.DefInst[0].Value;
                tmpSound.Tracks[1].Instrument = WinAGISettings.DefInst[1].Value;
                tmpSound.Tracks[2].Instrument = WinAGISettings.DefInst[2].Value;
                tmpSound.Tracks[0].Muted = WinAGISettings.DefMute[0].Value;
                tmpSound.Tracks[1].Muted = WinAGISettings.DefMute[1].Value;
                tmpSound.Tracks[2].Muted = WinAGISettings.DefMute[2].Value;
                tmpSound.Tracks[3].Muted = WinAGISettings.DefMute[3].Value;
            }
            if (EditGame != null) {
                frmGetResourceNum GetResNum = new(blnImporting ? GetRes.Import : GetRes.AddNew, AGIResType.Sound);
                if (blnImporting) {
                    GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportSoundFile).Replace(" ", "");
                }
                MDIMain.UseWaitCursor = false;
                if (GetResNum.ShowDialog(MDIMain) == DialogResult.Cancel) {
                    GetResNum.Close();
                    GetResNum.Dispose();
                    return;
                }
                tmpSound.Description = GetResNum.txtDescription.Text;
                if (GetResNum.DontImport) {
                    blnOpen = true;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    tmpSound.ID = GetResNum.txtID.Text;
                    AddNewSound(GetResNum.NewResNum, tmpSound);
                    tmpSound = EditGame.Sounds[GetResNum.NewResNum];
                    blnOpen = (GetResNum.chkOpenRes.Checked);
                }
                GetResNum.Dispose();
            }
            else {
                blnOpen = true;
            }
            // only open if user wants it open (or if not in a game or if opening/not importing)
            if (blnOpen) {
                frmNew = new();
                if (frmNew.LoadSound(tmpSound)) {
                    frmNew.Show();
                    SoundEditors.Add(frmNew);
                }
                else {
                    // TODO: handle error
                    MessageBox.Show(MDIMain, "error, can't open this sound");
                    frmNew.Close();
                    frmNew.Dispose();
                }
            }
            WinAGISettings.OpenNew.Value = blnOpen;
            MDIMain.UseWaitCursor = false;
        }

        public static void NewView() {
            NewView("");
        }

        public static void NewView(string ImportViewFile) {
            // creates a new view and opens an editor
            frmViewEdit frmNew;
            Engine.View tmpView;
            bool blnOpen = false, blnImporting = false;

            MDIMain.UseWaitCursor = true;
            tmpView = new Engine.View();
            if (ImportViewFile.Length != 0) {
                blnImporting = true;
                try {
                    tmpView.Import(ImportViewFile);
                }
                catch (Exception e) {
                    ErrMsgBox(e, "An error occurred during import:", "", "Import View Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                // now check to see if it's a valid picture resource (by trying to reload it)
                tmpView.Load();
                if (tmpView.ErrLevel < 0) {
                    ErrMsgBox(tmpView.ErrLevel, "Error reading View data:", "This is not a valid view resource.", "Invalid View Resource");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            else {
                // for new view, add first cel with default height/width
                tmpView.Loops.Add(0);
                tmpView[0].Cels.Add(0);
                tmpView[0][0].Height = WinAGISettings.DefCelH.Value;
                tmpView[0][0].Width = WinAGISettings.DefCelW.Value;
            }
            if (EditGame != null) {
                frmGetResourceNum GetResNum = new(blnImporting ? GetRes.Import : GetRes.AddNew, AGIResType.View);
                if (blnImporting) {
                    GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportViewFile).Replace(" ", "_");
                }
                MDIMain.UseWaitCursor = false;
                if (GetResNum.ShowDialog(MDIMain) == DialogResult.Cancel) {
                    tmpView = null;
                    GetResNum.Close();
                    GetResNum.Dispose();
                    return;
                }
                tmpView.Description = GetResNum.txtDescription.Text;
                if (GetResNum.DontImport) {
                    blnOpen = true;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    tmpView.ID = GetResNum.txtID.Text;
                    AddNewView(GetResNum.NewResNum, tmpView);
                    tmpView = EditGame.Views[GetResNum.NewResNum];
                    blnOpen = (GetResNum.chkOpenRes.Checked);
                }
                GetResNum.Dispose();
            }
            else {
                blnOpen = true;
            }
            // only open if user wants it open (or if not in a game or if opening/not importing)
            if (blnOpen) {
                frmNew = new();
                if (frmNew.LoadView(tmpView)) {
                    frmNew.Show();
                    ViewEditors.Add(frmNew);
                }
                else {
                    // TODO: handle error
                    MessageBox.Show(MDIMain, "error, can't open this view");
                    frmNew.Close();
                    frmNew.Dispose();
                }
            }
            WinAGISettings.OpenNew.Value = blnOpen;
            MDIMain.UseWaitCursor = false;
        }

        public static void NewInvObjList() {
            NewInvObjList("");
        }

        public static void NewInvObjList(string ImportObjFile, bool skipcheck = false) {
            // create a new InvObject list and opens an editor
            frmObjectEdit frmNew;
            InventoryList tmpList;

            MDIMain.UseWaitCursor = true;
            if (ImportObjFile.Length != 0) {
                try {
                    tmpList = new(ImportObjFile);
                }
                catch (Exception e) {
                    ErrMsgBox(e, "An error occurred during import:", "", "Import Object File Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            else {
                tmpList = [];
            }
            frmNew = new();
            if (frmNew.LoadOBJECT(tmpList)) {
                frmNew.Show();
            }
            else {
                // TODO: handle errors
                MessageBox.Show(MDIMain, "error, can't open this OBJECT file");
                frmNew.Close();
                frmNew.Dispose();
                MDIMain.UseWaitCursor = false;
                return;
            }
            // a game is loaded; find out if user wants this object file to replace existing
            if (!skipcheck && EditGame != null) {
                if (MessageBox.Show(MDIMain,
                    "Do you want to replace the existing OBJECT file with this one?",
                    "Replace OBJECT File",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes) {
                    if (OEInUse) {
                        DialogResult rtn;
                        if (ObjectEditor.IsChanged) {
                            rtn = MessageBox.Show(MDIMain,
                               "Do you want to save your changes and export the existing OBJECT file before you replace it?",
                               "Replace OBJECT File",
                               MessageBoxButtons.YesNoCancel,
                               MessageBoxIcon.Question);
                        }
                        else {
                            rtn = MessageBox.Show(MDIMain,
                                "Do you want to export the existing OBJECT file before you replace it?",
                                "Replace OBJECT File",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question);
                        }
                        if (rtn == DialogResult.Cancel) {
                            MDIMain.UseWaitCursor = false;
                            return;
                        }
                        if (rtn == DialogResult.Yes) {
                            if (ObjectEditor.IsChanged) {
                                ObjectEditor.SaveObjects();
                                // if a problem, changed flag will still be set
                                if (ObjectEditor.IsChanged) {
                                    MDIMain.UseWaitCursor = false;
                                    return;
                                }
                            }
                            // then export it(ignoring errors)
                            ObjectEditor.ExportObjects();
                        }
                        ObjectEditor.Close();
                        ObjectEditor.Dispose();
                    }
                    // active form is now the object editor
                    OEInUse = true;
                    ObjectEditor = frmNew;
                    ObjectEditor.InGame = true;
                    if (EditGame.InterpreterVersion == "2.089" || EditGame.InterpreterVersion == "2.272") {
                        ObjectEditor.EditInvList.Encrypted = false;
                    }
                    else {
                        ObjectEditor.EditInvList.Encrypted = true;
                    }
                    ObjectEditor.EditInvList.ResFile = EditGame.GameDir + "OBJECT";
                    ObjectEditor.Text = "Object Editor - " + EditGame.GameID;
                    ObjectEditor.IsChanged = false;
                    EditGame.InvObjects.CloneFrom(ObjectEditor.EditInvList);
                    EditGame.InvObjects.Save();
                    MDIMain.RefreshPropertyGrid();
                }
            }
            MDIMain.UseWaitCursor = false;
        }

        public static void NewWordList() {
            NewWordList("");
        }

        public static void NewWordList(string ImportWordFile, bool skipcheck = false) {
            // create a new WordList object and open an editor
            frmWordsEdit frmNew;
            WordList tmpList;

            // show wait cursor
            MDIMain.UseWaitCursor = true;
            if (ImportWordFile.Length != 0) {
                try {
                    tmpList = new(ImportWordFile);
                }
                catch (Exception e) {
                    ErrMsgBox(e, "An error occurred during import:", "", "Import WORDS.TOK File Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            else {
                tmpList = new();
            }
            frmNew = new();
            if (frmNew.LoadWords(tmpList)) {
                frmNew.Show();
            }
            else {
                // TODO: handle errors
                MessageBox.Show(MDIMain, "error, can't load this file");
                frmNew.Close();
                frmNew.Dispose();
                MDIMain.UseWaitCursor = false;
                return;
            }
            // a game is loaded; find out if user wants this words.tok file to replace existing
            if (!skipcheck && EditGame != null) {
                if (MessageBox.Show(MDIMain,
                    "Do you want to replace the existing WORDS.TOK file with this one?",
                    "Replace WORDS.TOK File",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes) {
                    if (WEInUse) {
                        DialogResult rtn;
                        if (WordEditor.IsChanged) {
                            rtn = MessageBox.Show(MDIMain,
                               "Do you want to save your changes and export the existing WORDS.TOK file before you replace it?",
                               "Replace WORDS.TOK File",
                               MessageBoxButtons.YesNoCancel,
                               MessageBoxIcon.Question);
                        }
                        else {
                            rtn = MessageBox.Show(MDIMain,
                                "Do you want to export the existing WORDS.TOK file before you replace it?",
                                "Replace WORDS.TOK File",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question);
                        }
                        if (rtn == DialogResult.Cancel) {
                            MDIMain.UseWaitCursor = false;
                            return;
                        }
                        if (rtn == DialogResult.Yes) {
                            if (WordEditor.IsChanged) {
                                WordEditor.SaveWords();
                                // if a problem, changed flag will still be set
                                if (WordEditor.IsChanged) {
                                    MDIMain.UseWaitCursor = false;
                                    return;
                                }
                            }
                            // then export it(ignoring errors)
                            WordEditor.ExportWords();
                        }
                        // close the old WordEditor
                        WordEditor.Close();
                        WordEditor.Dispose();
                    }
                    // active form is now the word editor
                    WEInUse = true;
                    WordEditor = frmNew;
                    WordEditor.InGame = true;
                    WordEditor.EditWordList.ResFile = EditGame.GameDir + "WORDS.TOK";
                    WordEditor.Text = "Word Editor - " + EditGame.GameID;
                    WordEditor.IsChanged = false;
                    EditGame.WordList.CloneFrom(WordEditor.EditWordList);
                    EditGame.WordList.Save();
                    MDIMain.RefreshPropertyGrid();
                }
            }
            MDIMain.UseWaitCursor = false;
        }

        public static string GetOpenResourceFilename(string mode, AGIResType restype) {
            // get a file to open as a resource
            MDIMain.OpenDlg.FileName = "";
            MDIMain.OpenDlg.InitialDirectory = DefaultResDir;
            switch (restype) {
            case AGIResType.Game:
                MDIMain.OpenDlg.Title = mode + "WinAGI Game File";
                MDIMain.OpenDlg.Filter = "WinAGI Game file (*.wag)|*.wag|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "";
                MDIMain.OpenDlg.FilterIndex = 1;
                MDIMain.OpenDlg.InitialDirectory = BrowserStartDir;
                break;
            case AGIResType.Logic:
                MDIMain.OpenDlg.Title = mode + "Logic Source File";
                string defext;
                if (EditGame == null) {
                    defext = WinAGISettings.DefaultExt.Value;
                }
                else {
                    defext = EditGame.SourceExt;
                }
                string textext;
                if (defext == "txt") {
                    textext = "";
                }
                else {
                    textext = "|Text files (*.txt)|*.txt";
                }
                MDIMain.OpenDlg.Filter = $"WinAGI Logic Source files (*.{defext})|*.{defext}{textext}|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "";
                MDIMain.OpenDlg.FilterIndex = WinAGISettingsFile.GetSetting("Logics", sOPENFILTER, 1);
                break;
            case AGIResType.Picture:
                MDIMain.OpenDlg.Title = mode + "Picture Resource File";
                MDIMain.OpenDlg.Filter = "WinAGI Picture Resource files (*.agp)|*.agp|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "";
                MDIMain.OpenDlg.FilterIndex = WinAGISettingsFile.GetSetting("Pictures", sOPENFILTER, 1);
                break;
            case AGIResType.Sound:
                MDIMain.OpenDlg.Title = mode + "Sound Resource File";
                MDIMain.OpenDlg.Filter = "WinAGI Sound Resource files (*.ags)|*.ags|AGI Sound Script files (*.ass)|*.ass|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "";
                MDIMain.OpenDlg.FilterIndex = WinAGISettingsFile.GetSetting("Sounds", sOPENFILTER, 1);
                break;
            case AGIResType.View:
                MDIMain.OpenDlg.Title = mode + "View Resource File";
                MDIMain.OpenDlg.Filter = "WinAGI View Resource files (*.agv)|*.agv|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "";
                MDIMain.OpenDlg.FilterIndex = WinAGISettingsFile.GetSetting("Views", sOPENFILTER, 1);
                break;
            case AGIResType.Objects:
                MDIMain.OpenDlg.Title = mode + "OBJECT File";
                MDIMain.OpenDlg.Filter = "AGI OBJECT files|OBJECT|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "";
                MDIMain.OpenDlg.FilterIndex = WinAGISettingsFile.GetSetting("Objects", sOPENFILTER, 1);
                break;
            case AGIResType.Words:
                MDIMain.OpenDlg.Title = mode + "WORDS.TOK File";
                MDIMain.OpenDlg.Filter = "AGI WORDS.TOK files|WORDS.TOK|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "";
                MDIMain.OpenDlg.FilterIndex = WinAGISettingsFile.GetSetting("Words", sOPENFILTER, 1);
                break;
            case AGIResType.Globals:
                MDIMain.OpenDlg.Title = "Global Defines File";
                MDIMain.OpenDlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "txt";
                MDIMain.OpenDlg.FilterIndex = WinAGISettingsFile.GetSetting("Globals", sOPENFILTER, 1);
                break;
            case AGIResType.Text:
                MDIMain.OpenDlg.Title = "Text File";
                MDIMain.OpenDlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                MDIMain.OpenDlg.FilterIndex = 1;
                MDIMain.OpenDlg.DefaultExt = "txt";
                break;
            }
            if (MDIMain.OpenDlg.ShowDialog() == DialogResult.Cancel) {
                // user canceled
                return "";
            }
            else {
                string section = "";
                switch (restype) {
                case AGIResType.Logic:
                    section = "Logics";
                    break;
                case AGIResType.Picture:
                    section = "Pictures";
                    break;
                case AGIResType.Sound:
                    section = "Sounds";
                    break;
                case AGIResType.View:
                    section = "Views";
                    break;
                case AGIResType.Objects:
                    section = "Objects";
                    break;
                case AGIResType.Words:
                    section = "Words";
                    break;
                case AGIResType.Globals:
                    section = "Globals";
                    break;
                }
                if (section.Length > 0) {
                    WinAGISettingsFile.WriteSetting(section, sOPENFILTER, MDIMain.OpenDlg.FilterIndex);
                    DefaultResDir = JustPath(MDIMain.OpenDlg.FileName);
                }
                else {
                    BrowserStartDir = Path.GetDirectoryName(MDIMain.OpenDlg.FileName);
                }
                return MDIMain.OpenDlg.FileName;
            }
        }

        /// <summary>
        /// Opens an in-game logic for editing. A dialog is displayed to the
        /// user to select which logic to open.
        /// </summary>
        public static void OpenGameLogic() {
            Debug.Assert(EditGame != null);

            frmGetResourceNum getresnum = new(GetRes.Open, AGIResType.Logic);
            if (getresnum.ShowDialog() == DialogResult.OK) {
                OpenGameLogic(getresnum.NewResNum, false);
            }
            getresnum.Dispose();
        }

        /// <summary>
        /// Opens the specified in-game logic for editing. If Quiet is true,
        /// no messagebox or error messages are displayed.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="Quiet"></param>
        public static bool OpenGameLogic(byte ResNum, bool Quiet = false) {
            MDIMain.UseWaitCursor = true;
            // check for existing editor
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == LogicFormMode.Logic) {
                    if (frm.InGame && frm.LogicNumber == ResNum) {
                        // logic is already open in another window 
                        if (frm.WindowState == FormWindowState.Minimized) {
                            frm.WindowState = FormWindowState.Normal;
                        }
                        frm.BringToFront();
                        frm.Select();
                        MDIMain.UseWaitCursor = false;
                        return true;
                    }
                }
            }
            frmLogicEdit frmOpen = new(LogicFormMode.Logic);
            if (frmOpen.LoadLogic(EditGame.Logics[ResNum])) {
                frmOpen.Show();
                LogicEditors.Add(frmOpen);
            }
            else {
                // TODO: handle error
                if (!Quiet) {
                    MessageBox.Show(MDIMain, "error, can't open this logic");
                }
                frmOpen.Close();
                frmOpen.Dispose();
                MDIMain.UseWaitCursor = false;
                return false;
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
            return true;
        }

        /// <summary>
        /// Opens a non-game logic for editing. If no file is specified
        /// user is prompted to get one.
        /// </summary>
        /// <param name="filename"></param>
        public static void OpenLogic(string filename = "") {

            if (filename.Length == 0) {
                // get a file to open
                filename = GetOpenResourceFilename("Open ", AGIResType.Logic);
                if (filename.Length == 0) {
                    // user canceled
                    return;
                }
            }
            MDIMain.UseWaitCursor = true;
            Logic loadlogic = new();
            loadlogic.ImportSource(filename);
            // TODO: add an error flag, then move loadlogic to 
            // the constructor
            frmLogicEdit frmOpen = new(LogicFormMode.Logic);
            if (frmOpen.LoadLogic(loadlogic)) {
                frmOpen.Show();
                LogicEditors.Add(frmOpen);
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this logic");
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenGamePicture() {
            Debug.Assert(EditGame != null);

            frmGetResourceNum getresnum = new(GetRes.Open, AGIResType.Picture);
            if (getresnum.ShowDialog() == DialogResult.OK) {
                OpenGamePicture(getresnum.NewResNum, false);
            }
            getresnum.Dispose();
        }

        public static void OpenGamePicture(byte ResNum, bool Quiet = false) {
            MDIMain.UseWaitCursor = true;
            // check for existing editor
            foreach (frmPicEdit frm in PictureEditors) {
                if (frm.InGame && frm.PictureNumber == ResNum) {
                    // picture is already open in another window 
                    if (frm.WindowState == FormWindowState.Minimized) {
                        frm.WindowState = FormWindowState.Normal;
                    }
                    frm.Select();
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            frmPicEdit frmOpen = new();
            if (frmOpen.LoadPicture(EditGame.Pictures[ResNum])) {
                frmOpen.Show();
                PictureEditors.Add(frmOpen);
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this picture");
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenPicture(string filename = "") {
            if (filename.Length == 0) {
                // get a file to open
                filename = GetOpenResourceFilename("Open ", AGIResType.Picture);
                if (filename.Length == 0) {
                    // user canceled
                    return;
                }
            }
            MDIMain.UseWaitCursor = true;
            Picture loadpic = new();
            loadpic.Import(filename);
            frmPicEdit frmOpen = new();
            if (frmOpen.LoadPicture(loadpic)) {
                frmOpen.Show();
                PictureEditors.Add(frmOpen);
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this picture");
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenGameSound() {
            Debug.Assert(EditGame != null);

            frmGetResourceNum getresnum = new(GetRes.Open, AGIResType.Sound);
            if (getresnum.ShowDialog() == DialogResult.OK) {
                OpenGameSound(getresnum.NewResNum, false);
            }
            getresnum.Dispose();
        }

        public static void OpenGameSound(byte ResNum, bool Quiet = false) {
            MDIMain.UseWaitCursor = true;
            // check for existing editor
            foreach (frmSoundEdit frm in SoundEditors) {
                if (frm.InGame && frm.SoundNumber == ResNum) {
                    // sound is already open in another window 
                    if (frm.WindowState == FormWindowState.Minimized) {
                        frm.WindowState = FormWindowState.Normal;
                    }
                    frm.Select();
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            frmSoundEdit frmOpen = new();
            if (frmOpen.LoadSound(EditGame.Sounds[ResNum])) {
                frmOpen.Show();
                SoundEditors.Add(frmOpen);
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this view");
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenSound(string filename = "") {
            if (filename.Length == 0) {
                // get a file to open
                filename = GetOpenResourceFilename("Open ", AGIResType.Sound);
                if (filename.Length == 0) {
                    // user canceled
                    return;
                }
            }
            MDIMain.UseWaitCursor = true;
            Sound loadsound = new();
            loadsound.Import(filename);
            frmSoundEdit frmOpen = new();
            if (frmOpen.LoadSound(loadsound)) {
                frmOpen.Show();
                SoundEditors.Add(frmOpen);
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this sound");
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenGameView() {
            Debug.Assert(EditGame != null);

            frmGetResourceNum getresnum = new(GetRes.Open, AGIResType.View);
            if (getresnum.ShowDialog() == DialogResult.OK) {
                OpenGameView(getresnum.NewResNum, false);
            }
            getresnum.Dispose();
        }

        public static void OpenGameView(byte ResNum, bool Quiet = false) {
            MDIMain.UseWaitCursor = true;
            // check for existing editor
            foreach (frmViewEdit frm in ViewEditors) {
                if (frm.InGame && frm.ViewNumber == ResNum) {
                    // view is already open in another window 
                    if (frm.WindowState == FormWindowState.Minimized) {
                        frm.WindowState = FormWindowState.Normal;
                    }
                    frm.Select();
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            frmViewEdit frmOpen = new();
            if (frmOpen.LoadView(EditGame.Views[ResNum])) {
                frmOpen.Show();
                ViewEditors.Add(frmOpen);
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this view");
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenView(string filename = "") {
            if (filename.Length == 0) {
                // get a file to open
                filename = GetOpenResourceFilename("Open ", AGIResType.View);
                if (filename.Length == 0) {
                    // user canceled
                    return;
                }
            }
            MDIMain.UseWaitCursor = true;
            Engine.View loadview = new();
            loadview.Import(filename);
            frmViewEdit frmOpen = new();
            if (frmOpen.LoadView(loadview)) {
                frmOpen.Show();
                ViewEditors.Add(frmOpen);
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this view");
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenGameOBJECT() {
            // check if already open
            if (OEInUse) {
                ObjectEditor.Select();
                return;
            }
            ObjectEditor = new frmObjectEdit();
            if (ObjectEditor.LoadOBJECT(EditGame.InvObjects)) {
                ObjectEditor.Show();
                OEInUse = true;
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this file");
            }
        }

        public static void OpenOBJECT(string filename) {
            InventoryList invlist;

            try {
                invlist = new(filename);
            }
            catch (Exception ex) {
                ErrMsgBox(ex, "File error, can't load this OBJECT file.", "", "Unable to Open OBJECT File");
                return;
            }
            frmObjectEdit frm = new();
            if (frm.LoadOBJECT(invlist)) {
                frm.Show();
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this file");
                frm.Close();
                frm.Dispose();
            }
        }

        public static void OpenGameWORDSTOK() {
            // check if already open
            if (WEInUse) {
                WordEditor.Select();
                return;
            }
            WordEditor = new frmWordsEdit();
            if (WordEditor.LoadWords(EditGame.WordList)) {
                WordEditor.Show();
                WEInUse = true;
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this file");
            }
        }

        public static void OpenWORDSTOK(string filename) {
            WordList wordlist;

            try {
                wordlist = new(filename);
            }
            catch (Exception ex) {
                ErrMsgBox(ex, "File error, can't load this WORDS.TOK file.", "", "Unable to Open WORDS.TOK File");
                return;
            }
            frmWordsEdit frm = new();
            if (frm.LoadWords(wordlist)) {
                frm.Show();
            }
            else {
                // TODO: handle error
                MessageBox.Show(MDIMain, "error, can't open this file");
                frm.Close();
                frm.Dispose();
            }
        }

        public static void RemoveLogic(byte LogicNum) {
            // removes a logic from the game, and updates
            // preview and resource windows
            //
            // it also updates layout editor, if it is open
            // and deletes the source code file from source directory
            // 
            // if the logic is currently open, it is closed
            bool blnIsRoom;

            // need to load logic to access sourcecode
            if (!EditGame.Logics.Contains(LogicNum)) {
                //raise error?
                MessageBox.Show(MDIMain,
                    $"Logic {LogicNum} passed to RemoveLogic does not exist.",
                    "RemoveLogic Error");
                return;
            }

            if (!EditGame.Logics[LogicNum].Loaded) {
                EditGame.Logics[LogicNum].Load();
            }
            string strSourceFile = EditGame.Logics[LogicNum].SourceFile;
            blnIsRoom = EditGame.Logics[LogicNum].IsRoom;
            // remove it from game
            EditGame.Logics.Remove(LogicNum);
            if (EditGame.UseLE && blnIsRoom) {
                // update layout editor and layout data file to show this room is now gone
                // TODO: UpdateExitInfo(euRemoveRoom, LogicNum, null);
            }
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.Logic];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["l" + LogicNum]) {
                    // deselect the resource beforev removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["l" + LogicNum].Remove();
                break;
            case EResListType.ComboList:
                // only update if the resource type is being listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)AGIResType.Logic) {
                    if (LogicNum == SelResNum) {
                        // deselect the resource before removing it
                        SelResNum = -1;
                        MDIMain.lstResources.Items["l" + LogicNum.ToString()].Remove();
                        // removing from the listbox does NOT re-select the next view
                        // fix is to select the header
                        MDIMain.SelectResource(AGIResType.Logic, -1, true);
                    }
                    else {
                        // just remove it
                        MDIMain.lstResources.Items["l" + LogicNum.ToString()].Remove();
                    }
                }
                break;
            }
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == LogicFormMode.Logic) {
                    if (frm.InGame && frm.LogicNumber == LogicNum) {
                        // set number to -1 to force close
                        frm.LogicNumber = -1;
                        frm.InGame = false;
                        //close it
                        frm.Close();
                        frm.Dispose();
                        break;
                    }
                }
            }
            // disposition any existing resource file
            if (File.Exists(strSourceFile)) {
                KillCopyFile(strSourceFile, WinAGISettings.BackupResFile.Value);
            }
            // update the logic tooltip lookup table
            IDefLookup[(int)AGIResType.Logic, LogicNum].Name = "";
            IDefLookup[(int)AGIResType.Logic, LogicNum].Type = ArgType.None;
            // then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListChanged = true;
            }
        }

        public static void RemovePicture(byte PicNum) {
            //removes a picture from the game, and updates
            //preview and resource windows
            //and deletes resource file from source directory
            if (!EditGame.Pictures.Contains(PicNum)) {
                // error?
                MessageBox.Show(MDIMain,
                    $"Picture {PicNum} passed to RemovePicture does not exist.",
                    "RemovePicture Error");
                return;
            }

            string strPicFile = EditGame.ResDir + EditGame.Pictures[PicNum].ID + ".agp";
            //remove it from game
            EditGame.Pictures.Remove(PicNum);

            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.Picture];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["p" + PicNum]) {
                    // deselect the resource beforev removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["p" + PicNum].Remove();
                break;
            case EResListType.ComboList:
                //only need to remove if pictures are listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)AGIResType.Picture) {
                    if (PicNum == SelResNum) {
                        // deselect the resource before removing it
                        SelResNum = -1;
                        MDIMain.lstResources.Items["p" + PicNum.ToString()].Remove();
                        // removing from the listbox does NOT re-select the next view
                        // fix is to select the header
                        MDIMain.SelectResource(AGIResType.Picture, -1, true);
                    }
                    else {
                        // just remove it
                        MDIMain.lstResources.Items["p" + PicNum.ToString()].Remove();
                    }
                }
                break;
            }
            foreach (frmPicEdit frm in PictureEditors) {
                if (frm.InGame && frm.PictureNumber == PicNum) {
                    //set number to -1 to force close
                    frm.PictureNumber = -1;
                    //close it
                    frm.Close();
                    frm.Dispose();
                    break;
                }
            }

            //disposition any existing resource file
            if (File.Exists(strPicFile)) {
                KillCopyFile(strPicFile, WinAGISettings.BackupResFile.Value);
            }

            //update the logic tooltip lookup table
            IDefLookup[(int)AGIResType.Picture, PicNum].Name = "";
            IDefLookup[(int)AGIResType.Picture, PicNum].Type = ArgType.None;
            // then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListChanged = true;
            }
        }

        public static void RemoveSound(byte SoundNum) {
            //removes a sound from the game, and updates
            //preview and resource windows
            //and deletes resource file from source directory
            if (!EditGame.Sounds.Contains(SoundNum)) {
                // error?
                MessageBox.Show(MDIMain,
                    $"Sound {SoundNum} passed to RemoveSound does not exist.",
                    "RemoveSound Error");
                return;
            }

            string strSoundFile = EditGame.ResDir + EditGame.Sounds[SoundNum].ID + ".ags";
            //remove it from game
            EditGame.Sounds.Remove(SoundNum);

            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.Sound];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["s" + SoundNum]) {
                    // deselect the resource before removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["s" + SoundNum].Remove();
                break;
            case EResListType.ComboList:
                //only need to remove if sounds are listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)AGIResType.Sound) {
                    if (SoundNum == SelResNum) {
                        // deselect the resource before removing it
                        SelResNum = -1;
                    }
                    MDIMain.lstResources.Items["s" + SoundNum.ToString()].Remove();
                    // removing from the listbox does NOT re-select the next view
                    // fix is to select the header
                    MDIMain.SelectResource(AGIResType.Sound, -1, true);
                }
                break;
            }
            foreach (frmSoundEdit frm in SoundEditors) {
                if (frm.InGame && frm.SoundNumber == SoundNum) {
                    //set number to -1 to force close
                    frm.SoundNumber = -1;
                    //close it
                    frm.Close();
                    frm.Dispose();
                    break;
                }
            }

            //disposition any existing resource file
            if (File.Exists(strSoundFile)) {
                KillCopyFile(strSoundFile, WinAGISettings.BackupResFile.Value);
            }

            //update the logic tooltip lookup table
            IDefLookup[(int)AGIResType.Sound, SoundNum].Name = "";
            IDefLookup[(int)AGIResType.Sound, SoundNum].Type = ArgType.None;
            //then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListChanged = true;
            }
        }

        public static void RemoveView(byte ViewNum) {
            //removes a sound from the game, and updates
            //preview and resource windows
            //and deletes resource file from source directory
            if (!EditGame.Views.Contains(ViewNum)) {
                // error?
                MessageBox.Show(MDIMain,
                    $"View {ViewNum} passed to RemoveView does not exist.",
                    "RemoveView Error");
                return;
            }

            string strViewFile = EditGame.ResDir + EditGame.Views[ViewNum].ID + ".agv";
            EditGame.Views.Remove(ViewNum);
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.View];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["v" + ViewNum]) {
                    // deselect the resource before removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["v" + ViewNum].Remove();
                break;
            case EResListType.ComboList:
                //only need to remove if views are listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)AGIResType.View) {
                    if (ViewNum == SelResNum) {
                        // deselect the resource before removing it
                        SelResNum = -1;
                        MDIMain.lstResources.Items["v" + ViewNum.ToString()].Remove();
                        // removing from the listbox does NOT re-select the next view
                        // fix is to select the header
                        MDIMain.SelectResource(AGIResType.View, -1, true);
                    }
                    else {
                        // just remove it
                        MDIMain.lstResources.Items["v" + ViewNum.ToString()].Remove();
                    }
                }
                break;
            }
            foreach (frmViewEdit frm in ViewEditors) {
                if (frm.InGame && frm.ViewNumber == ViewNum) {
                    //set number to -1 to force close
                    frm.ViewNumber = -1;
                    //close it
                    frm.Close();
                    frm.Dispose();
                    break;
                }
            }

            //disposition any existing resource file
            if (File.Exists(strViewFile)) {
                KillCopyFile(strViewFile, WinAGISettings.BackupResFile.Value);
            }

            //update the logic tooltip lookup table
            IDefLookup[(int)AGIResType.View, ViewNum].Name = "";
            IDefLookup[(int)AGIResType.View, ViewNum].Type = ArgType.None;
            //then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListChanged = true;
            }
        }

        public static void MakeAllChanged() {
            EditGame.Logics.MarkAllAsChanged();
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                foreach (TreeNode tmpNode in HdrNode[0].Nodes) {
                    if (File.Exists(EditGame.ResDir + EditGame.Logics[(byte)tmpNode.Tag].ID + "." + EditGame.SourceExt)) {
                        tmpNode.ForeColor = Color.Red;
                    }
                }
                break;
            case EResListType.ComboList:
                if (MDIMain.cmbResType.SelectedIndex == 1) {
                    foreach (ListViewItem tmpListItem in MDIMain.lstResources.Items) {
                        tmpListItem.ForeColor = Color.Red;
                    }
                }
                break;
            }
            RefreshTree(SelResType, SelResNum);
        }

        public static void RefreshLogicIncludes() {
            // updates the include files in all logic source and open ingame editors.
            // this is done when the include files are changed
            // in the game settings
            if (EditGame.IncludeReserved) {
                EditGame.ReservedDefines.SaveList(true);
            }
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == LogicFormMode.Logic) {
                    if (frm.InGame) {
                        frm.UpdateIncludes();
                    }
                }
            }
            foreach (Logic logic in EditGame.Logics) {
                logic.UpdateIncludes();
            }
            // now refresh reslist to indicate correct changed status
            RefreshTree();
        }

        public static string LogTemplateText(string NewID, string NewDescription) {
            string strLogic = "";
            bool blnNoFile = false;
            //first, get the default file, if there is one
            if (File.Exists(ProgramDir + "deflog.txt")) {
                try {
                    using FileStream fsLogTempl = new(ProgramDir + "deflog.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using StreamReader srLogTempl = new(fsLogTempl);
                    strLogic = srLogTempl.ReadToEnd();
                }
                catch (Exception) {
                    // problem with the file
                    blnNoFile = true;
                }
            }
            else {
                // no file
                blnNoFile = true;
            }
            //if no template file
            if (blnNoFile) {
                //something didn't work; let user know
                //DialogResult rtn = MsgBoxEx("The default logic template file ('deflog.txt') is missing" + Environment.NewLine +
                //               "from the WinAGI program directory. Using the WinAGI default" + Environment.NewLine +
                //               "template instead.", "Missing Template File", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ////      WinAGIHelp, "htm\winagi\newres.htm#logtemplate")
                strLogic = LoadResString(101);
                //insert line breaks
                strLogic = strLogic.Replace('|', (char)Keys.Enter);
            }

            try {
                //substitute correct values for the various place holders
                //add the tabs
                strLogic = strLogic.Replace("~", " ".MultStr(WinAGISettings.LogicTabWidth.Value));
                //id:
                strLogic = strLogic.Replace("%id", NewID);
                //description
                strLogic = strLogic.Replace("%desc", NewDescription);

                //horizon
                strLogic = strLogic.Replace("%h", WinAGISettings.PTHorizon.Value.ToString());

                //if using reserved names, insert them
                if (EditGame.IncludeReserved) {
                    //f5, v0, f2, f4, v9
                    strLogic = strLogic.Replace("f5", EditGame.ReservedDefines.ReservedFlags[5].Name);
                    strLogic = strLogic.Replace("f2", EditGame.ReservedDefines.ReservedFlags[2].Name);
                    strLogic = strLogic.Replace("f4", EditGame.ReservedDefines.ReservedFlags[4].Name);
                    strLogic = strLogic.Replace("v0", EditGame.ReservedDefines.ReservedVariables[0].Name);
                    strLogic = strLogic.Replace("v9", EditGame.ReservedDefines.ReservedVariables[9].Name);
                }
            }
            catch (Exception) {
                //ignore errors return whatever is left
            }
            //return the formatted text
            return strLogic;
        }

        internal static void FindInLogic(Form startform, string FindText, FindDirection FindDir, bool MatchWord, bool MatchCase, FindLocation LogicLoc, bool Replacing = false, string ReplaceText = "") {
            // logic search strategy:
            //
            // determine current starting position; can be in a logic currently being edited
            // or from the globals, words or objects editor
            //
            // if from a logic editor, begin search at current position in current editor
            // if from a non-logic editor, begin search in 1) logic editor that currently has 
            // focus, or 2) the first open logic editor, and lastly 3) begin in closed logics
            //
            // if this is a new search, set starting logic and position
            // if this is NOT a new search, continue where previous search left off
            // if search gets all the way back to beginning, stop

            int FoundPos;
            int SearchPos = -1;
            int nextLogicIndex = 0;
            int lngSearchSource = 0;
            bool blnFrmVis;
            int lngCheck = 0;
            int lngPossFind;
            bool blnSkipEd;
            WinAGIFCTB searchFCTB = null;

            MainStatusBar.Items["spStatus"].Text = "";

            StringComparison strComp = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (Replacing && FindText.Equals(ReplaceText, strComp)) {
                return;
            }
            MDIMain.UseWaitCursor = true;
            switch (startform.Name) {
            case "frmLogicEdit":
                lngSearchSource = 0;
                break;
            case "frmMDIMain":
                lngSearchSource = 1;
                break;
            case "frmObjectEdit":
                lngSearchSource = 2;
                break;
            case "frmWordsEdit":
                lngSearchSource = 3;
                break;
            case "frmGlobals":
                lngSearchSource = 4;
                break;
            }
            switch (lngSearchSource) {
            case 0:
                int i;
                for (i = 0; i < LogicEditors.Count; i++) {
                    if (LogicEditors[i] == startform) {
                        break;
                    }
                }
                nextLogicIndex = i;
                searchFCTB = LogicEditors[i].fctb;
                searchFCTB.Selection.Normalize();
                // if replacing, first check the current selection
                if (Replacing) {
                    if (searchFCTB.Selection.Text.Equals(FindText, strComp)) {
                        searchFCTB.InsertText(ReplaceText, true);
                    }
                }
                // if starting position not set, it means starting a new search
                if (SearchStartPos == -1) {
                    SearchStartLog = nextLogicIndex;
                    if (FindDir == FindDirection.Up) {
                        // when searching backwards, need to adjust back one so a match at 
                        // current position correctly skips when searching all the way around
                        SearchStartPos = searchFCTB.PlaceToPosition(searchFCTB.Selection.Start) - 1;
                        if (SearchStartPos < 0) {
                            SearchStartPos = searchFCTB.TextLength;
                        }
                    }
                    else {
                        SearchStartPos = searchFCTB.PlaceToPosition(searchFCTB.Selection.End);
                    }
                }
                // intial SearchPos also depends on direction
                if (FindDir == FindDirection.Up) {
                    SearchPos = searchFCTB.PlaceToPosition(searchFCTB.Selection.Start);
                    if (SearchPos == 0) {
                        SearchPos = searchFCTB.TextLength;
                    }
                }
                else {
                    SearchPos = searchFCTB.PlaceToPosition(searchFCTB.Selection.End);
                }
                break;
            case 1:
            case 2:
            case 3:
            case 4:
                //no distinction (yet) between words, objects, resIDs, globals
                if (LogicEditors.Count != 0) {
                    if (MDIMain.ActiveMdiChild.Name == "frmLogicEdit") {
                        for (i = 0; i < LogicEditors.Count; i++) {
                            if (LogicEditors[i] == MDIMain.ActiveMdiChild) {
                                break;
                            }
                        }
                        nextLogicIndex = i;
                    }
                    else {
                        // start the first logic editor
                        nextLogicIndex = 0;
                    }
                }
                else {
                    nextLogicIndex = FindInClosedLogics(FindText, FindDir, MatchWord, MatchCase, SearchType);
                    if (nextLogicIndex == -1) {
                        MessageBox.Show(MDIMain,
                            "Search text not found.",
                            "Find in Logic",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        MDIMain.UseWaitCursor = false;
                        return;
                    }
                }
                SearchStartLog = nextLogicIndex;
                searchFCTB = LogicEditors[nextLogicIndex].fctb;
                //always start non-logic searches at beginning
                SearchStartPos = 0;
                SearchPos = 0;
                break;
            }

            // main search routine; at this point, a logic editor is open and
            // available for searching
            do {
                // just in case we get stuck in a loop!
                lngCheck++;
                blnSkipEd = false;
                //if all logics, skip any text editors or non ingame logics
                if (LogicLoc == FindLocation.All) {
                    if (LogicEditors[nextLogicIndex].FormMode == LogicFormMode.Text || !LogicEditors[nextLogicIndex].InGame) {
                        //skip it
                        blnSkipEd = true;
                    }
                }
                if (blnSkipEd) {
                    // set result to 'nothing found'
                    FoundPos = -1;
                }
                else {
                    // search the target logic, from the starting search position
                    if (FindDir == FindDirection.Up) {
                        // if searching whole word
                        if (MatchWord) {
                            FoundPos = FindWholeWord(SearchPos, searchFCTB.Text, FindText, MatchCase, FindDir == FindDirection.Up, SearchType);
                        }
                        else {
                            FoundPos = searchFCTB.Text.LastIndexOf(FindText, SearchPos, strComp);
                        }
                        // always reset SearchPos
                        SearchPos = searchFCTB.TextLength;
                    }
                    else {
                        //search strategy depends on synonym search value
                        if (!GFindSynonym) {
                            if (MatchWord) {
                                FoundPos = FindWholeWord(SearchPos, searchFCTB.Text, FindText, MatchCase, FindDir == FindDirection.Up, SearchType);
                            }
                            else {
                                FoundPos = searchFCTB.Text.IndexOf(FindText, SearchPos, strComp);
                            }
                        }
                        else {
                            //Matchword is always true; but since words are surrounded by quotes, it wont matter
                            //so we use IndexOf
                            //step through each word in the word group; if the word is found in this logic,
                            //check if it occurs before the current found position
                            FoundPos = -1;
                            for (int i = 0; i < WordEditor.EditWordList.GroupByNumber(GFindGrpNum).WordCount; i++) {
                                lngPossFind = searchFCTB.Text.IndexOf(QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum)[i] + QUOTECHAR, SearchPos);
                                if (lngPossFind > 0) {
                                    if (FoundPos == -1) {
                                        FoundPos = lngPossFind;
                                        FindText = QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum)[i] + QUOTECHAR;
                                    }
                                    else {
                                        if (lngPossFind < FoundPos) {
                                            FoundPos = lngPossFind;
                                            FindText = QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum)[i] + QUOTECHAR;
                                        }
                                    }
                                }
                            }
                        }
                        // always reset SearchPos
                        SearchPos = 0;
                    }
                }
                if (FoundPos >= 0) {
                    if (FindDir == FindDirection.All || FindDir == FindDirection.Down) {
                        //if back at search start (whether anything found or not)
                        // OR PAST search start(after previously finding something)
                        if (((FoundPos == SearchStartPos) && (nextLogicIndex == SearchStartLog)) ||
                            ((FoundPos > SearchStartPos) && (nextLogicIndex == SearchStartLog) && RestartSearch)) {
                            if (LogicLoc != FindLocation.All) {
                                // back at start
                                FoundPos = -1;
                                break;
                            }
                            else {
                                nextLogicIndex = FindInClosedLogics(FindText, FindDir, MatchWord, MatchCase, SearchType);
                                if (nextLogicIndex < 0) {
                                    FoundPos = -1;
                                    break;
                                }
                                searchFCTB = LogicEditors[nextLogicIndex].fctb;
                                if (FindDir == FindDirection.Up) {
                                    SearchPos = searchFCTB.TextLength;
                                }
                                continue;
                            }
                        }
                        else {
                            // search text found so exit loop
                            break;
                        }
                    }
                    else {
                        //searching up
                        if (((FoundPos == SearchStartPos) && (nextLogicIndex == SearchStartLog)) ||
                            ((FoundPos < SearchStartPos) && (nextLogicIndex == SearchStartLog) && RestartSearch)) {
                            if (LogicLoc != FindLocation.All) {
                                // back at start
                                FoundPos = -1;
                                break;
                            }
                            else {
                                nextLogicIndex = FindInClosedLogics(FindText, FindDir, MatchWord, MatchCase, SearchType);
                                if (nextLogicIndex < 0) {
                                    FoundPos = -1;
                                    break;
                                }
                                searchFCTB = LogicEditors[nextLogicIndex].fctb;
                                SearchPos = searchFCTB.TextLength;
                            }
                        }
                        else {
                            //exit loop; search text found
                            break;
                        }
                    }
                }
                //if not found, action depends on search mode
                if (LogicLoc == FindLocation.Current) {
                    if (FindDir == FindDirection.Up) {
                        if (!RestartSearch) {
                            DialogResult rtn = MessageBox.Show(MDIMain,
                                "Beginning of search scope reached. Do you want to continue from the end?",
                                "Find in Logic",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                            if (rtn == DialogResult.No) {
                                //reset cursor
                                MDIMain.UseWaitCursor = false;
                                return;
                            }
                            // reset searchpos to end
                            SearchPos = searchFCTB.TextLength;
                        }
                        else {
                            // if restartsearch is true, it means this is second time through;
                            // since nothing found, just exit the loop
                            break;
                        }
                    }
                    else if (FindDir == FindDirection.Down) {
                        // if nothing found yet
                        if (!RestartSearch) {
                            DialogResult rtn = MessageBox.Show(MDIMain,
                                "End of search scope reached. Do you want to continue from the beginning?",
                                "Find in Logic",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                            if (rtn == DialogResult.No) {
                                MDIMain.UseWaitCursor = false;
                                return;
                            }
                        }
                        else {
                            // if resetsearch is true, means this is second time through;
                            // since nothing found, just exit the loop
                            break;
                        }
                    }
                    else if (FindDir == FindDirection.All) {
                        //if restartsearch is true, means this is second time through;
                        // since nothing found, just exit the loop
                        if (RestartSearch) {
                            //not found; exit
                            break;
                        }
                    }
                }
                else if (LogicLoc == FindLocation.Open) {
                    //if back on start, and search already reset
                    if ((nextLogicIndex == SearchStartLog) && RestartSearch) {
                        //not found- exit
                        break;
                    }
                    nextLogicIndex++;
                    if (nextLogicIndex >= LogicEditors.Count) {
                        nextLogicIndex = 0;
                    }
                    searchFCTB = LogicEditors[nextLogicIndex].fctb;
                    if (FindDir == FindDirection.Up) {
                        SearchPos = searchFCTB.TextLength;
                    }
                }
                else if (LogicLoc == FindLocation.All) {
                    //since nothing found in this logic, try the next
                    if (ClosedLogics) {
                        nextLogicIndex = FindInClosedLogics(FindText, FindDir, MatchWord, MatchCase, SearchType);
                        if (nextLogicIndex < 0) {
                            FoundPos = -1;
                            break;
                        }
                    }
                    else {
                        if ((nextLogicIndex == SearchStartLog) && RestartSearch) {
                            nextLogicIndex = FindInClosedLogics(FindText, FindDir, MatchWord, MatchCase, SearchType);
                            if (nextLogicIndex < 0) {
                                FoundPos = -1;
                                break;
                            }
                        }
                        else {
                            // not back to starting logic, so try the next open logic
                            nextLogicIndex++;
                            if (nextLogicIndex >= LogicEditors.Count) {
                                nextLogicIndex = 0;
                            }
                        }
                    }
                    searchFCTB = LogicEditors[nextLogicIndex].fctb;
                    if (FindDir == FindDirection.Up) {
                        SearchPos = searchFCTB.TextLength;
                    }
                }
                // set reset search flag so when we are back to starting logic,
                // the search will end
                RestartSearch = true;

                // loop is exited by finding the searchtext or reaching end of search area
                // (or if loopcheck fails)
            } while (lngCheck <= 256);
            Debug.Assert(lngCheck < 256);

            // if found update the selection in the correct editor window
            if (FoundPos >= 0) {
                if (!FoundOnce) {
                    FoundOnce = true;
                }
                // bring the selected window to the top of the order (restore if minimized)
                if (LogicEditors[nextLogicIndex].WindowState == FormWindowState.Minimized) {
                    LogicEditors[nextLogicIndex].WindowState = FormWindowState.Normal;
                }
                //if search was started from the editor (by pressing F3 or using menu option)
                if (!SearchStartDlg) {
                    //set focus to the editor
                    LogicEditors[nextLogicIndex].Select();
                    LogicEditors[nextLogicIndex].fctb.Select();
                }
                else {
                    //when searching from the dialog, make sure the logic is at top of
                    //zorder, but don't need to give it focus
                    LogicEditors[nextLogicIndex].BringToFront();
                }
                // highlight searchtext
                Place start = searchFCTB.PositionToPlace(FoundPos);
                Place end = new(start.iChar + FindText.Length, start.iLine);
                searchFCTB.Selection.Start = start;
                searchFCTB.Selection.End = end;
                searchFCTB.DoSelectionVisible();
                searchFCTB.Refresh();
                //if a synonym was found, note it on status bar
                if (GFindSynonym) {
                    if (FindText != QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum).GroupName + QUOTECHAR) {
                        MainStatusBar.Items["spStatus"].Text = FindText + " is a synonym for " + QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum).GroupName + QUOTECHAR;
                        // TODO: flash the status bar
                        //MDIMain.tmrFlash.Enabled = true;
                    }
                }
            }
            else {
                //search string was NOT found (or couldn't open a logic editor window)

                if (FoundOnce) {
                    //search complete; no new instances found
                    blnFrmVis = FindingForm.Visible;
                    if (blnFrmVis) {
                        FindingForm.Visible = false;
                    }
                    MessageBox.Show(MDIMain,
                        "The specified region has been searched. No more matches found.",
                        "Find in Logic",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    if (blnFrmVis) {
                        FindingForm.Visible = true;
                    }
                }
                else {
                    blnFrmVis = FindingForm.Visible;
                    if (blnFrmVis) {
                        FindingForm.Visible = false;
                    }
                    MessageBox.Show(MDIMain,
                        "Search text not found.",
                        "Find in Logic",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    if (blnFrmVis) {
                        FindingForm.Visible = true;
                    }
                }
                // restore focus to correct form
                if (SearchStartDlg) {
                    //assume it's visible
                    Debug.Assert(FindingForm.Visible);
                }
                else {
                    if (nextLogicIndex >= 0) {
                        LogicEditors[nextLogicIndex].Select();
                    }
                }
                // reset search flags
                FindingForm.ResetSearch();
            }
            MDIMain.UseWaitCursor = false;
        }

        private static int FindInClosedLogics(string FindText, FindDirection FindDir, bool MatchWord, bool MatchCase, AGIResType SearchType = AGIResType.None) {
            // find next closed logic that has search text in it;
            // if found, return the logic number
            // if not found, return -1
            StringComparison vbcComp;
            bool blnLoaded = false;

            if (!ClosedLogics) {
                // first time through - start with first logic (which sets ClosedLogics flag)
                LogNum = NextClosedLogic(-1);
                if (LogNum != -1) {
                    ProgressWin = new () {
                        Text = "Find in Logic"
                    };
                    ProgressWin.lblProgress.Text = "Searching " + EditGame.Logics[LogNum].ID + "...";
                    ProgressWin.pgbStatus.Maximum = EditGame.Logics.Count + 1;
                    ProgressWin.pgbStatus.Value = LogicEditors.Count;
                    ProgressWin.Show(MDIMain);
                    ProgressWin.Refresh();
                }
                else {
                    // no other logics to search
                    return -1;
                }
            }
            else {
                ProgressWin.Show(MDIMain);
                ProgressWin.Refresh();
                LogNum = NextClosedLogic(LogNum);
            }
            vbcComp = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            while (LogNum != -1) {
                ProgressWin.lblProgress.Text = "Searching " + EditGame.Logics[LogNum].ID + "...";
                ProgressWin.Refresh();
                blnLoaded = EditGame.Logics[LogNum].Loaded;
                if (!blnLoaded) {
                    EditGame.Logics[LogNum].Load();
                }
                if (FindDir == FindDirection.Up) {
                    if (MatchWord) {
                        if (FindWholeWord(EditGame.Logics[LogNum].SourceText.Length, EditGame.Logics[LogNum].SourceText, FindText, MatchCase, true, SearchType) != -1) {
                            break;
                        }
                    }
                    else {
                        if (EditGame.Logics[LogNum].SourceText.LastIndexOf(FindText, vbcComp) != 0) {
                            break;
                        }
                    }
                }
                else {
                    // searching down -  strategy depends on synonym search value
                    if (!GFindSynonym) {
                        if (MatchWord) {
                            if (FindWholeWord(0, EditGame.Logics[LogNum].SourceText, FindText, MatchCase, false, SearchType) != -1) {
                                break;
                            }
                        }
                        else {
                            if (EditGame.Logics[LogNum].SourceText.IndexOf(FindText, vbcComp) != -1) {
                                break;
                            }
                        }
                    }
                    else {
                        // Matchword is always true; but since words are surrounded by quotes, it wont matter
                        // so use Instr
                        // step through each word in the word group; if any word is found in this logic,
                        // then stop
                        bool found = false;
                        for (int i = 0; i < WordEditor.EditWordList.GroupByNumber(GFindGrpNum).WordCount; i++) {
                            if (EditGame.Logics[LogNum].SourceText.IndexOf(QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum)[i] + QUOTECHAR, vbcComp) != -1) {
                                found = true;
                                break;
                            }
                        }
                        if (found) {
                            break;
                        }
                    }
                }
                // not found
                if (!blnLoaded) {
                    EditGame.Logics[LogNum].Unload();
                }
                LogNum = NextClosedLogic(LogNum);
                ProgressWin.pgbStatus.Value++;
            }
            if (LogNum == -1) {
                ProgressWin.Close();
                ProgressWin.Dispose();
                return -1;
            }
            if (!blnLoaded) {
                EditGame.Logics[LogNum].Unload();
            }
            // open editor, if able (this will reset the cursor to normal so force it
            // back to hourglass)
            OpenGameLogic((byte)LogNum, true);
            MDIMain.UseWaitCursor = true;
            int index;
            for (index = LogicEditors.Count - 1; index >= 0; index--) {
                if (LogicEditors[index].FormMode == LogicFormMode.Logic && LogicEditors[index].LogicNumber == LogNum) {
                    break;
                }
            }
            if (index < 0) {
                // must have been an error
                MessageBox.Show(MDIMain,
                    QUOTECHAR + FindText + QUOTECHAR + " was found in logic " + LogNum + " but an error occurred while opening the file. Try opening the logic manually and then try the search again.",
                    "Find In Logic",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                if (!blnLoaded) {
                    EditGame.Logics[LogNum].Unload();
                }
                ProgressWin.Hide();
                return -1;
            }
            ProgressWin.Hide();
            return index;
        }

        private static int NextClosedLogic(int OldLogNum) {
            // need a separate array of logics that are open BEFORE beginning search
            // so we can check if the logic was open prior to the search starting
            // (LogicEditors collection will changes as logics are opened by the search)

            if (!ClosedLogics) {
                // build list of currently open logics
                LogWin = new int[LogicEditors.Count];
                for (int i = 0; i < LogicEditors.Count; i++) {
                    LogWin[i] = -1;
                    if (LogicEditors[i].FormMode == LogicFormMode.Logic && LogicEditors[i].InGame) {
                        LogWin[i] = LogicEditors[i].LogicNumber;
                    }
                    else {
                        LogWin[i] = -1;
                    }
                }
                ClosedLogics = true;
            }
            // start with next number
            OldLogNum++;
            while (OldLogNum < 256) {
                // if this number is a valid logic
                if (EditGame.Logics.Contains((byte)OldLogNum) && !LogWin.Contains(OldLogNum)) {
                    // found a  closed logic
                    return OldLogNum;
                }
                // increment old log number
                OldLogNum++;
            }
            // not found; all logics searched
            return -1;
        }

        public static void ReplaceAll(Form startform, string FindText, string ReplaceText, FindDirection FindDir, bool MatchWord, bool MatchCase, FindLocation LogicLoc, AGIResType SearchType = AGIResType.None) {
            //  replace all doesn't use or need direction
            // if search Type is defines, words or objects, the editor does progress status
            // and msgs
            StringComparison CompMode = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (FindText.Equals(ReplaceText, CompMode)) {
                MDIMain.UseWaitCursor = false;
                return;
            }
            // find text can't be blank
            if (FindText.Length == 0) {
                MDIMain.UseWaitCursor = false;
                return;
            }
            ProgressWin = new();
            // not all searches use the progress bar
            switch (SearchType) {
            case AGIResType.None:
            case AGIResType.Logic:
            case AGIResType.Picture:
            case AGIResType.Sound:
            case AGIResType.View:
                MDIMain.UseWaitCursor = true;
                break;
            }
            switch (LogicLoc) {
            case FindLocation.Current:
                ReplaceAllText(FindText, ReplaceText, MatchWord, MatchCase, true, null, (frmLogicEdit)startform);
                break;
            case FindLocation.Open:
                // replace in all open logic and text editors
                ProgressWin.Text = "Replace All";
                ProgressWin.lblProgress.Text = "Searching...";
                ProgressWin.pgbStatus.Maximum = LogicEditors.Count;
                ProgressWin.pgbStatus.Value = 0;
                ProgressWin.Show(MDIMain);
                ProgressWin.Refresh();
                for (int i = 0; i < LogicEditors.Count; i++) {
                    ProgressWin.pgbStatus.Value = i - 1;
                    if (LogicEditors[i].FormMode == LogicFormMode.Logic) {
                        // show the logic ID
                        ProgressWin.lblProgress.Text = "Searching " + LogicEditors[i].EditLogic.ID + "...";
                    }
                    else {
                        // show the filename
                        ProgressWin.lblProgress.Text = "Searching " + Path.GetFileName(LogicEditors[i].TextFilename) + "...";
                    }
                    ProgressWin.Refresh();
                    ReplaceAllText(FindText, ReplaceText, MatchWord, MatchCase, true, null, LogicEditors[i]);
                }
                ProgressWin.Hide();
                break;
            case FindLocation.All:
                // if replacing globals, don't use the progress form
                // it's already being used to track the globals being searched
                if (SearchType != AGIResType.Globals) {
                    if (SearchType == AGIResType.None) {
                        ProgressWin.Text = "Replace All";
                        ProgressWin.lblProgress.Text = "Searching...";
                    }
                    else {
                        ProgressWin.Text = "Updating Resource ID";
                        ProgressWin.lblProgress.Text = "Searching...";
                    }
                    ProgressWin.pgbStatus.Maximum = EditGame.Logics.Count;
                    ProgressWin.pgbStatus.Value = 0;
                    ProgressWin.Show(MDIMain);
                    ProgressWin.Refresh();
                }
                // first replace in all open editors
                for (int i = 0; i < LogicEditors.Count; i++) {
                    //only logic editors, and ingame
                    if (LogicEditors[i].FormMode == LogicFormMode.Logic && LogicEditors[i].InGame) {
                        if (SearchType != AGIResType.Globals) {
                            ProgressWin.pgbStatus.Value++;
                            ProgressWin.lblProgress.Text = "Searching " + LogicEditors[i].EditLogic.ID + "...";
                            ProgressWin.Refresh();
                        }
                        ReplaceAllText(FindText, ReplaceText, MatchWord, MatchCase, true, null, LogicEditors[i], SearchType);
                    }
                }
                // then do all closed logics
                LogNum = NextClosedLogic(-1);
                while (LogNum != -1) {
                    switch (SearchType) {
                    case AGIResType.None:
                    case AGIResType.Logic:
                    case AGIResType.Picture:
                    case AGIResType.Sound:
                    case AGIResType.View:
                        ProgressWin.pgbStatus.Value++;
                        ProgressWin.lblProgress.Text = "Searching " + EditGame.Logics[LogNum].ID + "...";
                        ProgressWin.Refresh();
                        break;
                    }
                    bool loaded = EditGame.Logics[LogNum].Loaded;
                    if (!loaded) {
                        EditGame.Logics[LogNum].Load();
                    }
                    ReplaceAllText(FindText, ReplaceText, MatchWord, MatchCase, false, EditGame.Logics[LogNum], null, SearchType);
                    if (EditGame.Logics[LogNum].SourceChanged) {
                        EditGame.Logics[LogNum].SaveSource();
                        // refresh preview and tree as applicable
                        RefreshTree(AGIResType.Logic, LogNum);
                        if (MDIMain.propertyGrid1.Visible) {
                            MDIMain.propertyGrid1.Refresh();
                        }
                    }
                    if (!loaded) {
                        EditGame.Logics[LogNum].Unload();
                    }
                    LogNum = NextClosedLogic(LogNum);
                }
                switch (SearchType) {
                case AGIResType.None:
                case AGIResType.Logic:
                case AGIResType.Picture:
                case AGIResType.Sound:
                case AGIResType.View:
                    ProgressWin.Hide();
                    break;
                }
                break;
            }
            switch (SearchType) {
            case AGIResType.None:
            case AGIResType.Logic:
            case AGIResType.Picture:
            case AGIResType.Sound:
            case AGIResType.View:
                if (SearchType == AGIResType.None) {
                    if (ReplaceCount > 0) {
                        MessageBox.Show(MDIMain,
                            "The specified region has been searched. " + ReplaceCount + " replacements were made.",
                            "Replace All",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else {
                        MessageBox.Show(MDIMain,
                            "Search text not found.",
                            "Replace All",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                    }
                }
                MDIMain.UseWaitCursor = false;
                break;
            }
            ProgressWin.Close();
            ProgressWin.Dispose();
            FindingForm.ResetSearch();
        }

        private static void ReplaceAllText(string FindText, string ReplaceText, bool MatchWord, bool MatchCase, bool InWindow = false, Logic SearchLogic = null, frmLogicEdit SearchWin = null, AGIResType SearchType = AGIResType.None) {
            // replaces text in either a logic, or a textbox
            // calling function MUST ensure a valid reference to a richtextbox
            // or a logic is passed

            if (SearchType != AGIResType.None) {
                // ignore text editors
                if (InWindow) {
                    if (SearchWin.FormMode == LogicFormMode.Text) {
                        return;
                    }
                }
            }
            StringComparison vbcComp = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            //int FoundPos = 0;
            if (MatchWord) {
                string pattern = @"\b" + FindText + @"\b";
                if (InWindow) {
                    SearchWin.fctb.Text = Regex.Replace(SearchWin.fctb.Text, pattern, x => {
                        ReplaceCount++;
                        return ReplaceText;
                    }, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
                }
                else {
                    SearchLogic.SourceText = Regex.Replace(SearchLogic.SourceText, pattern, x => {
                        ReplaceCount++;
                        return ReplaceText;
                    }, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
                }
            }
            else {
                if (InWindow) {
                    SearchWin.fctb.Text = Regex.Replace(SearchWin.fctb.Text, FindText, x => {
                        ReplaceCount++;
                        return ReplaceText;
                    }, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
                }
                else {
                    SearchLogic.SourceText = Regex.Replace(SearchLogic.SourceText, FindText, x => {
                        ReplaceCount++;
                        return ReplaceText;
                    }, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
                }
            }
        }


        /// <summary>
        /// Safely deletes a resource file, and optionally copies it to a backup file
        /// with '_OLD' added to the file name.
        /// </summary>
        /// <param name="ResFile"></param>
        /// <param name="KeepOld"></param>
        public static void KillCopyFile(string ResFile, bool KeepOld) {
            string strOldName;
            int lngNextNum;
            string strName, strExt;

            // ignore any errors - if it deletes, that's great; if not
            // we don't really care...
            try {
                if (File.Exists(ResFile)) {
                    if (KeepOld) {
                        strName = Path.GetDirectoryName(ResFile) + "\\" + Path.GetFileNameWithoutExtension(ResFile);
                        strExt = Path.GetExtension(ResFile);
                        lngNextNum = 1;
                        //assign proposed rename
                        strOldName = strName + "_OLD" + strExt;
                        //Validate it
                        while (File.Exists(strOldName)) {
                            lngNextNum++;
                            strOldName = strName + "_OLD_" + lngNextNum.ToString() + strExt;
                        }
                        SafeFileMove(ResFile, strOldName, true);
                        return;
                    }
                    // if not keeping old, just delete current file
                    SafeFileDelete(ResFile);
                }
            }
            catch (Exception) {
                //ignore
            }
        }

        /// <summary>
        /// Displays an extended WinAGI message error
        /// </summary>
        /// <param name="ErrNum"></param>
        /// <param name="ErrMsg1"></param>
        /// <param name="ErrMsg2"></param>
        /// <param name="ErrCaption"></param>
        public static void ErrMsgBox(int ErrNum, string ErrMsg1, string ErrMsg2, string ErrCaption) {
            // show errmsg baed on agi resource error level
            string strErrMsg;

            // TODO: need to confirm all error message calls
            strErrMsg = ErrMsg1 + Environment.NewLine + Environment.NewLine + ErrNum + ": " + LoadResString(ErrNum);
            if (ErrMsg2.Length > 0) {
                strErrMsg = strErrMsg + Environment.NewLine + Environment.NewLine + ErrMsg2;
            }
            MessageBox.Show(MDIMain, strErrMsg, ErrCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ErrMsgBox(Exception e, string ErrMsg1, string ErrMsg2, string ErrCaption) {
            //displays a messagebox showing ErrMsg and includes error passed as AGIErrObj
            int lngErrNum;
            string strErrMsg;

            //determine if ErrNum is an AGI number:
            if ((e.HResult & WINAGI_ERR) == WINAGI_ERR) {
                lngErrNum = e.HResult - WINAGI_ERR;
            }
            else {
                lngErrNum = e.HResult;
            }

            strErrMsg = ErrMsg1 + Environment.NewLine + Environment.NewLine + lngErrNum + ": " + e.Message;
            if (ErrMsg2.Length > 0) {
                strErrMsg = strErrMsg + Environment.NewLine + Environment.NewLine + ErrMsg2;
            }
            MessageBox.Show(MDIMain, strErrMsg, ErrCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void GetDefaultColors() {
            // reads default custom colors from winagi.confg
            for (int i = 0; i < 16; i++) {
                DefaultPalette[i] = WinAGISettingsFile.GetSetting(sDEFCOLORS, "DefEGAColor" + i, DefaultPalette[i]);
            }

        }

        public static string ResourceName(AGIResource ThisResource, bool IsInGame, bool NoNumber = false) {
            //formats resource name based on user preference
            //format includes: option for upper, lower or title case of Type;
            //                 space or period for separator;
            //                 forcing number to include leading zeros

            //if the resource is not part of a game,
            //the ID is returned regardless of ID/number setting
            string retval = "";
            //if using numbers AND resource is ingame,
            if (WinAGISettings.ShowResNum.Value && IsInGame) {
                switch (WinAGISettings.ResFormatNameCase.Value) {
                case 0:
                    retval = ThisResource.ResType.ToString().ToLower();
                    break;
                case 1:
                    retval = ThisResource.ResType.ToString().ToUpper();
                    break;
                case 2:
                default:
                    retval = ThisResource.ResType.ToString();
                    break;
                }
                return retval + WinAGISettings.ResFormatSeparator.Value + ThisResource.Number.ToString(WinAGISettings.ResFormatNumFormat.Value);
            }
            else {
                if (WinAGISettings.IncludeResNum.Value && IsInGame && !NoNumber) {
                    retval = ThisResource.Number + " - ";
                }
                retval += ThisResource.ID;
            }
            return retval;
        }

        public static string InstrumentName(int instrument) {

            //returns a string Value of an instrument
            return LoadResString(INSTRUMENTNAMETEXT + instrument);
        }

        public static void DrawTransGrid(Control surface, int offsetX, int offsetY) {
            using Graphics gp = surface.CreateGraphics();

            for (int i = 0; i <= surface.Width + 1; i += 10) {
                for (int j = 0; j < surface.Height + 1; j += 10) {
                    gp.FillRectangle(Brushes.Black, new Rectangle(i + offsetX, j + offsetY, 1, 1));
                }
            }
        }

        public static void ShowAGIBitmap(PictureBox pic, Bitmap agiBMP, int tgtX, int tgtY, int tgtW, int tgtH, InterpolationMode mode = InterpolationMode.NearestNeighbor) {
            // draws the agi bitmap in target picture box using passed target size/location

            //to scale the picture without blurring, need to use NearestNeighbor interpolation
            // that can't be set directly, so a graphics object is needed to draw the the picture
            int bWidth, bHeight;
            bWidth = pic.Width;
            bHeight = pic.Height;
            if (bWidth == 0 || bHeight == 0) {
                return;
            }

            /*------------------------------------------------------------*/
            // see comment in frmPreview.DisplayCel - this does not work;
            // using the pic graphics causes bad flicker- can't find a way
            // around it...
            /*------------------------------------------------------------*/
            /*
            using Graphics g = pic.CreateGraphics();
            */
            /*------------------------------------------------------------*/

            /*------------------------------------------------------------*/
            // this works without flicker
            /*------------------------------------------------------------*/
            // first, create new image in the picture box that is desired size
            pic.Image = new Bitmap(bWidth, bHeight);
            // intialize a graphics object for the image just created
            using Graphics g = Graphics.FromImage(pic.Image);
            //always clear the background first
            g.Clear(pic.BackColor);
            /*------------------------------------------------------------*/

            // set correct interpolation mode
            g.InterpolationMode = mode;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            // draw the bitmap, at correct resolution
            g.DrawImage(agiBMP, tgtX, tgtY, tgtW, tgtH);
        }

        /// <summary>
        /// Draws the agi bitmap in target picture box, using scale factor provided
        /// </summary>
        /// <param name="pic"></param>
        /// <param name="agiBMP"></param>
        /// <param name="scale"></param>
        /// <param name="mode"></param>
        public static void ShowAGIBitmap(PictureBox pic, Bitmap agiBMP, double scale = 1, InterpolationMode mode = InterpolationMode.NearestNeighbor) {
            Debug.Assert(scale > 0);
            if (agiBMP is null) {
                // clear the pic
                pic.CreateGraphics().Clear(pic.BackColor);
                return;
            }
            int bWidth = (int)(agiBMP.Width * scale * 2), bHeight = (int)(agiBMP.Height * scale);
            // pictures and views with errors will pass null value
            // first, create new image in the picture box that is desired size
            pic.Image = new Bitmap(bWidth, bHeight);
            //intialize a graphics object for the image just created
            using Graphics g = Graphics.FromImage(pic.Image);
            //always clear the background first
            g.Clear(pic.BackColor);
            // set correct interpolation mode
            // draw the bitmap, at correct resolution
            g.InterpolationMode = mode;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(agiBMP, 0, 0, bWidth, bHeight);
        }

        static Bitmap ResizeAGIBitmap(Bitmap agiBmp, int scale = 1, InterpolationMode mode = InterpolationMode.NearestNeighbor) {
            //resizes a bitmap using APIs so it can be exported
            // NearestNeighbor, with PixelMode set to Half, gives clean scaling of pixels with 
            // no blurring

            Bitmap newBmp;
            Size bSize = new() {
                Width = agiBmp.Width * scale * 2,
                Height = agiBmp.Height * scale
            };
            newBmp = new Bitmap(bSize.Width, bSize.Height);
            using Graphics g = Graphics.FromImage(newBmp);
            // need to adjust pixel offset so edges are drawn correctly
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.InterpolationMode = mode;
            g.DrawImage(agiBmp, 0, 0, bSize.Width, bSize.Height);
            return newBmp;
        }

        /// <summary>
        /// Error-safe method that provides an easy way to accss WinAGI.Editor
        /// string resources by number instead of a string key.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string LoadResString(int index) {
            // this function is just a handy way to get resource strings by number
            // instead of by stringkey
            try {
                return Editor.EditorResources.ResourceManager.GetString(index.ToString());
            }
            catch (Exception) {
                // return nothing if string doesn't exist
                return "";
            }
        }

        public static int FindWholeWord(int lngStartPos, string strText, string strFind,
                                     bool MatchCase = false,
                                     bool RevSearch = false,
                                     AGIResType SearchType = AGIResType.None) {
            // will return the character position of first occurence of strFind in strText,
            // only if it is a whole word
            // whole word is defined as a word where the character in front of the word is a
            // separator (or word is at beginning of string) AND character after word is a
            // separator (or word is at end of string)
            //
            // separators are any character EXCEPT:
            // #, $, %, ., 0-9, @, A-Z, _, a-z
            // (codes 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122)
            int lngPos;
            bool blnFrontOK;
            StringComparison StringCompare;

            if (strFind.Length == 0) {
                return 0;
            }
            if (MatchCase) {
                StringCompare = StringComparison.Ordinal;
            }
            else {
                StringCompare = StringComparison.OrdinalIgnoreCase;
            }
            // set position to start
            lngPos = lngStartPos;
            do {
                //if doing a reverse search
                if (RevSearch) {
                    lngPos = strText.LastIndexOf(strFind, lngPos, StringCompare);
                }
                else {
                    lngPos = strText.IndexOf(strFind, lngPos, StringCompare);
                }
                // easy check is to see if strFind is even in strText
                if (lngPos == -1) {
                    return -1;
                }
                // check character in front
                if (lngPos > 0) {
                    switch (strText[lngPos - 1]) {
                    case '#' or '$' or '%' or '_' or (>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z'):
                        // word is NOT whole word
                        blnFrontOK = false;
                        break;
                    default:
                        blnFrontOK = true;
                        break;
                    }
                }
                else {
                    blnFrontOK = true;
                }
                if (blnFrontOK) {
                    // check character in back
                    if (lngPos + strFind.Length < strText.Length) {
                        switch (strText[lngPos + strFind.Length]) {
                        case '#' or '$' or '%' or '_' or (>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z'):
                            // word is NOT whole word
                            //let loop try again at next position in string
                            break;
                        default:
                            // is validation required
                            switch (SearchType) {
                            case Words:
                                //validate vocab word
                                if (IsVocabWord(lngPos, strText)) {
                                    // word IS a whole word
                                    return lngPos;
                                }
                                break;
                            case AGIResType.Objects:
                                // validate an inventory object
                                if (IsInvObject(lngPos, strText)) {
                                    // word IS a whole word
                                    return lngPos;
                                }
                                break;
                            default:
                                // no validation - word IS a whole word
                                return lngPos;
                            }
                            break;
                        }
                    }
                    else {
                        // word IS a whole word
                        return lngPos;
                    }
                }
                // entire string not checked yet - try again
                if (RevSearch) {
                    lngPos--;
                }
                else {
                    lngPos++;
                }
            } while (lngPos != -1);
            // no position found
            return -1;
        }

        public static bool CheckLogics() {
            // checks all logics; if any found that are changed
            // allow user to recompile game if desired before
            // running
            bool retval = true;
            // if not requiring recompile
            if (WinAGISettings.CompileOnRun.Value == AskOption.No) {
                // don't even need to check
                return true;
            }
            // check compile status of all logics
            foreach (Logic tmpLogic in EditGame.Logics) {
                bool blnLoaded = tmpLogic.Loaded;
                if (!blnLoaded) {
                    tmpLogic.Load();
                }
                if (!tmpLogic.Compiled) {
                    // not ok; skip checking and
                    // determine if recompiling is appropriate
                    retval = false;
                    // don't forget to unload!!
                    if (!blnLoaded) {
                        tmpLogic.Unload();
                    }
                    break;
                }
                if (!blnLoaded) {
                    tmpLogic.Unload();
                }
            }
            // if no changed logics found, check any existing logics that are being edited
            if (retval) {
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.FormMode == LogicFormMode.Logic) {
                        if (frm.rtfLogic2.IsChanged == true) {
                            // one changed logic found
                            retval = false;
                            break;
                        }
                    }
                }
            }
            // if still no changed logics found
            if (retval) {
                // just exit
                return true;
            }
            DialogResult rtn = DialogResult.Cancel;
            bool blnDontAsk = false;
            // if CompileOnRun is in ask mode or yes mode, get user choice
            switch (WinAGISettings.CompileOnRun.Value) {
            case AskOption.Ask:
                rtn = MsgBoxEx.Show(MDIMain,
                    "One or more logics have changed since you last compiled.\n\n" +
                    "Do you want to compile them before running?",
                    "Compile Before Running?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    "Always take this action when compiling a game.", ref blnDontAsk);
                if (blnDontAsk) {
                    if (rtn == DialogResult.Yes) {
                        WinAGISettings.CompileOnRun.Value = AskOption.Yes;
                    }
                    else if (rtn == DialogResult.No) {
                        WinAGISettings.CompileOnRun.Value = AskOption.No;
                    }
                    WinAGISettings.CompileOnRun.WriteSetting(WinAGISettingsFile);
                }
                break;
            case AskOption.No:
                rtn = DialogResult.No;
                break;
            case AskOption.Yes:
                rtn = DialogResult.Yes;
                break;
            }
            switch (rtn) {
            case DialogResult.Cancel:
                // return false, so run cmd is canceled
                return false;
            case DialogResult.No:
                // not compiling; check is complete
                return true;
            case DialogResult.Yes:
                // ok to compile
                break;
            }
            return CompileChangedLogics(true);
        }

        internal static bool IsInvObject(int lngStartPos, string strText) {
            //check for 'has' cmd
            //check for 'obj.in.room' cmd
            //check for 'drop' cmd
            //check for 'get' cmd
            //check for 'put' cmd

            // TODO: not implemented yet; always return true
            return true;
        }

        internal static bool IsVocabWord(int lngStartPos, string strText) {
            //check for 'said' cmd
            //check for  'word.to.string'
            //get line by backing up until CR, ';' or beginning of string reached
            //then move forward, finding the command

            // TODO: not implemented yet; always return true
            return true;
        }

        public static AGIToken FindPrevCmd(WinAGIFCTB fctb, AGIToken starttoken, ref int argCount) {
            // searches backwards through text, starting with passed token and returns the
            //  command currently being edited; null string if there isn't one
            //
            // cmd is found by looking for an open parenthesis "("; the token
            // in front of this parenthesis is the cmd of interest
            //
            // stop looking when:
            //    - a cmd is found (a word separator is found in front of the char string
            //      that precedes the parenthesis)
            //    - beginning of logic is reached
            //    - cursor moves to previous line (that's a semicolon ";" or bracket "{"
            //      or "}")
            //
            // lngArgCount is set by counting number of commas between tokens as the
            // search goes
            bool next = starttoken.Text == "(", failed = false;
            // TODO: should argcount be a member of AGIToken? maybe
            // a data or tag field?
            int lngArgCount = 0;
            AGIToken token = fctb.PreviousToken(starttoken, true);
            while (token.Type != AGITokenType.None) {
                if (next) {
                    break;
                }
                if (token.Type == AGITokenType.Symbol) {
                    switch (token.Text) {
                    case ",":
                        lngArgCount++;
                        break;
                    case "(":
                        next = true;
                        break;
                    default:
                        failed = true;
                        break;
                    }
                    if (failed) {
                        break;
                    }
                }
                token = fctb.PreviousToken(token, true);
            }
            // validate it
            if (token.Type == AGITokenType.Identifier) {
                if (token.Text[0] == '#') {
                    // check against snippets
                    if (WinAGISettings.UseSnippets.Value) {
                        for (int i = 0; i < CodeSnippets.Length; i++) {
                            if (token.Text[1..] == CodeSnippets[i].Name && CodeSnippets[i].ArgTips.Length > 0) {
                                token.SubType = TokenSubtype.Snippet;
                                token.Number = i;
                                token.ArgList = CodeSnippets[i].ArgTips.Split(",");
                                argCount = lngArgCount;
                                return token;
                            }
                        }
                    }
                }
                else {
                    // check this command against list
                    int cmdNum = CommandNum(token.Text);
                    if (cmdNum >= 0) {
                        if (cmdNum >= 200) {
                            cmdNum -= 200;
                            token.SubType = TokenSubtype.TestCmd;
                        }
                        else {
                            token.SubType = TokenSubtype.ActionCmd;
                        }
                        token.Number = cmdNum;
                        argCount = lngArgCount;
                        return token;
                    }
                }
            }
            // not editing a command arg list
            token.Type = AGITokenType.None;
            token.StartPos = -1;
            token.EndPos = -1;
            token.Line = 0;
            return token;
        }

        /// <summary>
        /// If a valid agi command, returns the number.
        /// Test commands are offset by 200.
        /// Returns -1 if not a command.
        /// </summary>
        /// <param name="strCmdName"></param>
        /// <returns></returns>
        public static int CommandNum(string strCmdName) {
            // check for test command
            for (int retval = 0; retval < 20; retval++) {
                if (strCmdName == TestCommands[retval].Name) {
                    return retval + 200;
                }
            }
            // look for action command
            for (int retval = 0; retval < 182; retval++) {
                if (strCmdName == ActionCommands[retval].Name) {
                    return retval;
                }
            }
            // TODO: what if Sierra syntax?
            // not found
            return -1;
        }

        public static Snippet CheckSnippet(string sniptext) {
            // check all snippets; if a match is found, return the snippet
            // if no match, set name to empty string
            Snippet retval = new();
            if (CodeSnippets.Length == 0) {
                return retval;
            }
            int pos = 0, lngArgCount = 0;
            bool blnArgNext = false, blnSnipOK = false;
            string[] strArgs = [];
            AGIToken sniptoken = WinAGIFCTB.TokenFromPos(sniptext, pos);
            AGIToken argtoken = new();
            if (sniptoken.Text != sniptext) {
                argtoken = WinAGIFCTB.NextToken(sniptext, sniptoken);
                if (argtoken.Text == "(") {
                    blnArgNext = true;
                    do {
                        argtoken = WinAGIFCTB.NextToken(sniptext, argtoken);
                        if (blnArgNext) {
                            if (argtoken.Text == ")") {
                                if (lngArgCount > 0) {
                                    // last arg value missing; assume empty string
                                    lngArgCount++;
                                    Array.Resize(ref strArgs, lngArgCount);
                                    strArgs[lngArgCount - 1] = "";
                                }
                                // make sure it's end of line
                                blnSnipOK = argtoken.EndPos == sniptext.Length;
                                break;
                            }
                            else if (argtoken.Text == ",") {
                                // missing arg, assume empty string
                                lngArgCount++;
                                Array.Resize(ref strArgs, lngArgCount);
                                strArgs[lngArgCount - 1] = "";
                                // still expecting an argument so don't
                                // change value of blnNextArg
                            }
                            else {
                                lngArgCount++;
                                Array.Resize(ref strArgs, lngArgCount);
                                strArgs[lngArgCount - 1] = argtoken.Text;
                                // now expecting a comma or closing bracket
                                blnArgNext = false;
                            }
                        }
                        else {
                            // next arg should be a comma or closing parenthesis
                            if (argtoken.Text == ")") {
                                blnSnipOK = argtoken.EndPos == sniptext.Length;
                                break;
                            }
                            if (argtoken.Text != ",") {
                                // if not a comma or closing parenthesis
                                // expand arg value to include the additional text
                                strArgs[lngArgCount - 1] = strArgs[lngArgCount - 1] + " " + argtoken.Text;
                                // try again to find comma or closing parenthesis
                            }
                            else {
                                // after comma, another argument is expected
                                blnArgNext = true;
                            }
                        }
                    } while (argtoken.Type != AGITokenType.None);
                }
            }
            else {
                // sniptext has no arguments, and is valid for checking
                blnSnipOK = true;
            }
            if (!blnSnipOK) {
                return retval;
            }
            for (int i = 0; i < CodeSnippets.Length; i++) {
                if (CodeSnippets[i].Name == sniptoken.Text) {
                    retval.Name = CodeSnippets[i].Name;
                    retval.Value = CodeSnippets[i].Value;
                    if (lngArgCount > 0) {
                        for (int j = 0; j < lngArgCount; j++) {
                            retval.Value = retval.Value.Replace("%" + (j + 1).ToString(), strArgs[j]);
                        }
                    }
                    return retval;
                }
            }
            // no match
            return retval;
        }

        #region Export Functions
        public static void ExportGameResource(AGIResType restype, int resnum) {
            // default filename is always resource ID and restype extension
            switch (restype) {
            case AGIResType.Game:
                // export all
                ExportAll(false);
                break;
            case AGIResType.Logic:
                _ = ExportLogic(EditGame.Logics[resnum], true);
                break;
            case AGIResType.Picture:
                _ = ExportPicture(EditGame.Pictures[resnum], true);
                break;
            case AGIResType.Sound:
                ExportSound(EditGame.Sounds[resnum], true);
                break;
            case AGIResType.View:
                ExportView(EditGame.Views[SelResNum], true);
                break;
            case AGIResType.Objects:
                ExportObjects(EditGame.InvObjects, true);
                break;
            case AGIResType.Words:
                ExportWords(EditGame.WordList, true);
                break;
            }
        }

        public static void ExportAll(bool defaultdir = true) {
            // exports all logic, picture, sound and view resources into a
            // directory overwriting where necessary
            // if defaultdir is true, the target directory is the game's 
            // resource directory; otherwise user is prompted for a location
            bool blnLoaded = false;
            string exportdir;

            if (!defaultdir) {
                MDIMain.FolderDlg.InitialDirectory = DefaultResDir;
                MDIMain.FolderDlg.AddToRecent = false;
                MDIMain.FolderDlg.Description = "Select a directory to export all game resources.";
                MDIMain.FolderDlg.ShowHiddenFiles = false;
                MDIMain.FolderDlg.ShowNewFolderButton = true;
                if (MDIMain.FolderDlg.ShowDialog(MDIMain) == DialogResult.Cancel) {
                    return;
                }
                DefaultResDir = JustPath(MDIMain.FolderDlg.SelectedPath);
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Existing resources with same names as game resources will be overwritten. Continue?",
                    "Confirm Export All",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question);
                if (rtn == DialogResult.Cancel) {
                    return;
                }
                exportdir = FullDir(MDIMain.FolderDlg.SelectedPath);
            }
            else {
                exportdir = EditGame.ResDir;
            }
            MDIMain.UseWaitCursor = true;
            ProgressWin = new() {
                Text = "Exporting All Resources"
            };
            ProgressWin.pgbStatus.Maximum = EditGame.Logics.Count + EditGame.Pictures.Count + EditGame.Sounds.Count + EditGame.Views.Count;
            ProgressWin.pgbStatus.Value = 0;
            ProgressWin.lblProgress.Text = "Exporting...";
            ProgressWin.Show();
            ProgressWin.Refresh();
            foreach (Logic logic in EditGame.Logics) {
                ProgressWin.lblProgress.Text = "Exporting " + logic.ID;
                ProgressWin.pgbStatus.Value++;
                ProgressWin.Refresh();
                blnLoaded = logic.Loaded;
                if (!blnLoaded) {
                    logic.Load();
                }
                // source code (if not resourcedir)
                if (!exportdir.Equals(EditGame.ResDir)) {
                    if (logic.SrcErrLevel >= 0) {
                        logic.ExportSource(exportdir + logic.ID + "." + EditGame.SourceExt);
                    }
                }
                // compiled logic
                if (logic.ErrLevel >= 0) {
                    logic.Export(exportdir + logic.ID + ".agl");
                }
                if (!blnLoaded) {
                    logic.Unload();
                }
            }
            foreach (Picture tmpPic in EditGame.Pictures) {
                ProgressWin.lblProgress.Text = "Exporting " + tmpPic.ID;
                ProgressWin.pgbStatus.Value++;
                ProgressWin.Refresh();
                blnLoaded = tmpPic.Loaded;
                if (!blnLoaded) {
                    tmpPic.Load();
                }
                if (tmpPic.ErrLevel >= 0) {
                    tmpPic.Export(exportdir + tmpPic.ID + ".agp");
                }
                if (!blnLoaded) {
                    tmpPic.Unload();
                }
            }
            foreach (Sound tmpSnd in EditGame.Sounds) {
                ProgressWin.lblProgress.Text = "Exporting " + tmpSnd.ID;
                ProgressWin.pgbStatus.Value++;
                ProgressWin.Refresh();
                blnLoaded = tmpSnd.Loaded;
                if (!blnLoaded) {
                    tmpSnd.Load();
                }
                if (tmpSnd.ErrLevel >= 0) {
                    tmpSnd.Export(exportdir + tmpSnd.ID + ".ags");
                }
                if (!blnLoaded) {
                    tmpSnd.Unload();
                }
            }
            foreach (Engine.View tmpView in EditGame.Views) {
                ProgressWin.lblProgress.Text = "Exporting " + tmpView.ID;
                ProgressWin.pgbStatus.Value++;
                ProgressWin.Refresh();
                blnLoaded = tmpView.Loaded;
                if (!blnLoaded) {
                    tmpView.Load();
                }
                if (tmpView.ErrLevel >= 0) {
                    tmpView.Export(exportdir + tmpView.ID + ".agv");
                }
                if (!blnLoaded) {
                    tmpView.Unload();
                }
            }
            ProgressWin.Close();
            ProgressWin.Dispose();
            MDIMain.UseWaitCursor = false;
        }

        /// <summary>
        /// Exports this logic as a source text file or an AGI logic resource file.
        /// </summary>
        /// <param name="logic"></param>
        /// <param name="ingame"></param>
        /// <returns>1 if the source code was exported, 0 otherwise</returns>
        public static int ExportLogic(Logic logic, bool ingame) {
            string filename;
            bool loaded = logic.Loaded;

            frmExportLogicOptions frm = new frmExportLogicOptions();
            if (frm.ShowDialog(MDIMain) == DialogResult.Cancel) {
                return 0;
            }
            if (frm.optSourceCode.Checked || frm.optBoth.Checked) {
                filename = NewSourceName(logic, ingame);
                if (filename.Length == 0) {
                    return 0;
                }
                if (!loaded) {
                    logic.Load();
                }
                try {
                    logic.ExportSource(filename);
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "An error occurred while exporting this file: ", "", "Export File Error");
                    if (!loaded) {
                        logic.Unload();
                    }
                }
            }
            if (frm.optResource.Checked || frm.optBoth.Checked) {
                filename = NewResourceName(logic, ingame);
                if (filename.Length == 0) {
                    if (!loaded) {
                        logic.Unload();
                    }
                    return 0;
                }
                if (logic.ErrLevel >= 0) {
                    try {
                        logic.Export(filename);
                    }
                    catch (Exception ex) {
                        ErrMsgBox(ex, "An error occurred while exporting this logic: ", "", "Export Logic Error");
                        if (!loaded) {
                            logic.Unload();
                        }
                    }
                }
                if (!loaded) {
                    logic.Unload();
                }
            }
            if (frm.optSourceCode.Checked || frm.optBoth.Checked) {
                return 1;
            }
            else {
                return 0;
            }
        }

        /// <summary>
        /// Exports this picture as an AGI picture resource file or as an image
        /// file (bmp, jpg, png, tiff, gif).
        /// </summary>
        /// <param name="picture"></param>
        /// <param name="ingame"></param>
        /// <returns>1 if exported as an AGI resource, 0 otherwise</returns>
        public static int ExportPicture(Picture picture, bool ingame) {

            using frmExportPictureOptions frmPEO = new(0);
            if (frmPEO.ShowDialog(MDIMain) == DialogResult.Cancel) {
                return 0;
            }
            bool loaded = picture.Loaded;
            if (!loaded) {
                picture.Load();
            }
            bool ExportImage = frmPEO.optImage.Checked;
            int lngZoom = (int)frmPEO.udZoom.Value;
            int lngFormat = frmPEO.cmbFormat.SelectedIndex + 1;
            string filename;
            int lngMode;
            if (frmPEO.optVisual.Checked) {
                lngMode = 0;
            }
            else if (frmPEO.optPriority.Checked) {
                lngMode = 1;
            }
            else {
                lngMode = 2;
            }
            frmPEO.Dispose();
            int retval = 0;
            if (ExportImage) {
                // get a filename
                MDIMain.SaveDlg.Title = "Save Picture Image As";
                MDIMain.SaveDlg.DefaultExt = "bmp";
                MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
                MDIMain.SaveDlg.Filter = "BMP files (*.bmp)|*.bmp|JPEG files (*.jpg)|*.jpg|GIF files (*.gif)|*.gif|TIFF files (*.tif)|*.tif|PNG files (*.PNG)|*.png|All files (*.*)|*.*";
                MDIMain.SaveDlg.CheckPathExists = true;
                MDIMain.SaveDlg.CheckWriteAccess = true;
                MDIMain.SaveDlg.FilterIndex = lngFormat;
                MDIMain.SaveDlg.OverwritePrompt = true;
                MDIMain.SaveDlg.OkRequiresInteraction = true;
                MDIMain.SaveDlg.ShowHiddenFiles = false;
                MDIMain.SaveDlg.FileName = "";
                if (MDIMain.SaveDlg.ShowDialog(MDIMain) != DialogResult.Cancel) {
                    filename = MDIMain.SaveDlg.FileName;
                    DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
                    MDIMain.UseWaitCursor = true;
                    ProgressWin = new () {
                        Text = "Exporting Picture Image"
                    };
                    ProgressWin.lblProgress.Text = "Depending on export size, this may take awhile. Please wait...";
                    ProgressWin.pgbStatus.Visible = false;
                    ProgressWin.Show();
                    ProgressWin.Refresh();
                    ExportImg(picture, filename, lngFormat, lngMode, lngZoom);
                    // all done!
                    ProgressWin.Close();
                    ProgressWin.Dispose();
                    MDIMain.UseWaitCursor = false;
                }
                retval = 0;
            }
            else {
                filename = NewResourceName(picture, ingame);
                if (filename.Length != 0) {
                    WinAGISettingsFile.WriteSetting(sPICTURES, sEXPFILTER, MDIMain.SaveDlg.FilterIndex);
                    filename = MDIMain.SaveDlg.FileName;
                    DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
                    MDIMain.UseWaitCursor = true;
                    try {
                        picture.Export(filename);
                        retval = 1;
                    }
                    catch (Exception ex) {
                        ErrMsgBox(ex, "An error occurred while exporting this picture:", "", "Export Sound Error");
                        retval = 0;
                    }
                }
                else {
                    retval = 0;
                }
            }
            if (!loaded) {
                picture.Unload();
            }
            MDIMain.UseWaitCursor = false;
            return retval;
        }

        /// <summary>
        /// Exports this sound as an AGI sound resource file or as a PCM wave file 
        /// or MIDI file.
        /// </summary>
        /// <param name="sound"></param>
        /// <param name="ingame"></param>
        /// <returns>1 if exported as an AGI resource, 0 otherwise</returns>
        public static int ExportSound(Sound sound, bool ingame) {
            SoundFormat exportformat = SoundFormat.Undefined;

            using frmExportSoundOptions frmSEO = new(sound.SndFormat);
            if (frmSEO.ShowDialog(MDIMain) == DialogResult.Cancel) {
                return 0;
            }
            bool loaded = sound.Loaded;
            if (!loaded) {
                sound.Load();
            }
            if (frmSEO.optNative.Checked) {
                exportformat = SoundFormat.AGI;
            }
            else if (frmSEO.optMidi.Checked) {
                exportformat = SoundFormat.MIDI;
            }
            else if (frmSEO.optWAV.Checked) {
                exportformat = SoundFormat.WAV;
            }
            else if (frmSEO.optASS.Checked) {
                exportformat = SoundFormat.Script;
            }
            frmSEO.Dispose();
            string filename;
            MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            if (ingame) {
                MDIMain.SaveDlg.FileName = sound.ID;
            }
            else {
                MDIMain.SaveDlg.FileName = Path.GetFileNameWithoutExtension(sound.ID);
            }
            switch (exportformat) {
            case SoundFormat.AGI:
                if (ingame) {
                    MDIMain.SaveDlg.Title = "Export Sound";
                }
                else {
                    MDIMain.SaveDlg.Title = "Save Sound As";
                }
                MDIMain.SaveDlg.DefaultExt = "ags";
                MDIMain.SaveDlg.Filter = "WinAGI Sound Files|*.ags|All files (*.*)|*.*";
                MDIMain.SaveDlg.FileName += ".ags";
                break;
            case SoundFormat.MIDI:
                MDIMain.SaveDlg.Title = "Save Sound As MIDI";
                MDIMain.SaveDlg.DefaultExt = "mid";
                MDIMain.SaveDlg.Filter = "MIDI Files|*.mid|All files (*.*)|*.*";
                MDIMain.SaveDlg.FileName += ".mid";
                break;
            case SoundFormat.WAV:
                MDIMain.SaveDlg.Title = "Save Sound As WAV";
                MDIMain.SaveDlg.Filter = "WAV Sound Files|*.wav|All files (*.*)|*.*";
                MDIMain.SaveDlg.DefaultExt = "wav";
                MDIMain.SaveDlg.FileName += ".wav";
                break;
            case SoundFormat.Script:
                MDIMain.SaveDlg.Title = "Save Sound As Script";
                MDIMain.SaveDlg.Filter = "AGI Sound Script Files|*.ass|All files (*.*)|*.*";
                MDIMain.SaveDlg.DefaultExt = "ass";
                MDIMain.SaveDlg.FileName += ".ass";
                break;
            }
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.CheckWriteAccess = true;
            MDIMain.SaveDlg.FilterIndex = 0;
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.ShowHiddenFiles = false;
            int retval = 0;
            if (MDIMain.SaveDlg.ShowDialog(MDIMain) != DialogResult.Cancel) {
                filename = MDIMain.SaveDlg.FileName;
                DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
                MDIMain.UseWaitCursor = true;
                try {
                    sound.Export(filename, exportformat);
                    retval = exportformat == SoundFormat.AGI ? 1 : 0;
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "An error occurred while exporting this sound:", "", "Export Sound Error");
                }
            }
            if (!loaded) {
                sound.Load();
            }
            MDIMain.UseWaitCursor = false;
            return retval;
        }

        /// <summary>
        /// Exports this view as an AGI view resource file.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="ingame"></param>
        /// <returns>1 if exported as an AGI resource, 0 otherwise</returns>
        public static int ExportView(Engine.View view, bool ingame) {
            // views can only be exported as AGI resources, so no
            // options form is needed
            string filename = NewResourceName(view, ingame);
            bool loaded = view.Loaded;
            int retval = 0;

            if (filename.Length != 0) {
                MDIMain.UseWaitCursor = true;
                DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
                if (!loaded) {
                    view.Load();
                }
                try {
                    view.Export(filename);
                    retval = 1;
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "An error occurred while exporting this view:", "", "Export View Error");
                }
            }
            if (!loaded) {
                view.Unload();
            }
            MDIMain.UseWaitCursor = false;
            return retval;
        }

        public static bool ExportObjects(InventoryList invObjects, bool ingame) {
            string filename;
            bool retval = false;

            if (ingame) {
                MDIMain.SaveDlg.Title = "Export OBJECT";
            }
            else {
                MDIMain.SaveDlg.Title = "Save OBJECT As";
            }
            if (invObjects.ResFile.Length > 0) {
                MDIMain.SaveDlg.FileName = Path.GetFileName(invObjects.ResFile);
                MDIMain.SaveDlg.InitialDirectory = Path.GetDirectoryName(invObjects.ResFile);
            }
            else {
                MDIMain.SaveDlg.FileName = "";
                MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            }
            MDIMain.SaveDlg.Filter = "AGI OBJECT Files|OBJECT|All files (*.*)|*.*";
            MDIMain.SaveDlg.FilterIndex = WinAGISettingsFile.GetSetting("Objects", sOPENFILTER, 1);
            MDIMain.SaveDlg.DefaultExt = "";
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.ExpandedMode = true;
            MDIMain.SaveDlg.ShowHiddenFiles = false;
            if (MDIMain.SaveDlg.ShowDialog(MDIMain) == DialogResult.Cancel) {
                // nothing selected
                return false;
            }
            filename = MDIMain.SaveDlg.FileName;
            DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
            bool loaded = invObjects.Loaded;
            if (!loaded) {
                invObjects.Load();
            }
            MDIMain.UseWaitCursor = true;
            try {
                invObjects.Export(filename);
                retval = true;
            }
            catch (Exception ex) {
                ErrMsgBox(ex, "An error occurred while exporting this OBJECT file:", "", "Export OBJECT Error");
            }
            finally {
                if (!loaded) {
                    invObjects.Unload();
                }
            }
            MDIMain.UseWaitCursor = false;
            return retval;
        }

        public static bool ExportWords(WordList wordList, bool ingame) {
            string filename;
            bool retval = false;

            if (ingame) {
                MDIMain.SaveDlg.Title = "Export WORDS.TOK";
            }
            else {
                MDIMain.SaveDlg.Title = "Save WORDS.TOK As";
            }
            if (wordList.ResFile.Length > 0) {
                MDIMain.SaveDlg.FileName = Path.GetFileName(wordList.ResFile);
                MDIMain.SaveDlg.InitialDirectory = Path.GetDirectoryName(wordList.ResFile);
            }
            else {
                MDIMain.SaveDlg.FileName = "";
                MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            }
            MDIMain.SaveDlg.Filter = "AGI WORDS.TOK Files|WORDS.TOK|All files (*.*)|*.*";
            MDIMain.SaveDlg.FilterIndex = 0;
            MDIMain.SaveDlg.DefaultExt = "";
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.ExpandedMode = true;
            MDIMain.SaveDlg.ShowHiddenFiles = false;
            if (MDIMain.SaveDlg.ShowDialog(MDIMain) == DialogResult.Cancel) {
                // nothing selected
                return false;
            }
            filename = MDIMain.SaveDlg.FileName;
            DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
            bool loaded = wordList.Loaded;
            if (!loaded) {
                wordList.Load();
            }
            MDIMain.UseWaitCursor = true;
            try {
                wordList.Export(filename);
                retval = true;
            }
            catch (Exception ex) {
                ErrMsgBox(ex, "An error occurred while exporting this WORDS.TOK file:", "", "Export WORDS.TOK Error");
            }
            finally {
                if (!loaded) {
                    wordList.Unload();
                }
            }
            MDIMain.UseWaitCursor = false;
            return retval;
        }

        public static void ExportAllPicImgs() {
            //exports all picture images as one format in src dir
            int lngZoom, lngMode, lngFormat;
            string strExt = "";
            bool blnLoaded;
            //show options form, force image only
            frmExportPictureOptions frmPEO = new(1) {
                Text = "Export All Picture Images"
            };
            ;
            if (frmPEO.ShowDialog(MDIMain) == DialogResult.Cancel) {
                //nothing to do
                frmPEO.Close();
                frmPEO.Dispose();
                return;
            }
            lngZoom = (int)frmPEO.udZoom.Value;
            lngFormat = frmPEO.cmbFormat.SelectedIndex + 1;
            if (frmPEO.optVisual.Checked) {
                lngMode = 0;
            }
            else if (frmPEO.optPriority.Checked) {
                lngMode = 1;
            }
            else {
                lngMode = 2;
            }
            //done with the options form
            frmPEO.Close();
            frmPEO.Dispose();
            //show wait cursor
            MDIMain.UseWaitCursor = true;
            //need to get correct file extension
            switch (lngFormat) {
            case 1:
                strExt = ".bmp";
                break;
            case 2:
                strExt = ".jpg";
                break;
            case 3:
                strExt = ".gif";
                break;
            case 4:
                strExt = ".tif";
                break;
            case 5:
                strExt = ".png";
                break;
            }
            //if not canceled, export them all
            //setup ProgressWin form
            ProgressWin = new () {
                Text = "Exporting All Picture Images"
            };
            ProgressWin.pgbStatus.Maximum = EditGame.Pictures.Count;
            ProgressWin.pgbStatus.Value = 0;
            ProgressWin.lblProgress.Text = "Exporting...";
            ProgressWin.Show();
            ProgressWin.Refresh();

            foreach (Picture ThisPic in EditGame.Pictures) {
                ProgressWin.lblProgress.Text = "Exporting " + ThisPic.ID + " Image...";
                ProgressWin.pgbStatus.Value++;
                ProgressWin.Refresh();
                // load pic if necessary
                blnLoaded = ThisPic.Loaded;
                if (!blnLoaded) {
                    ThisPic.Load();
                }
                // skip if errors
                if (ThisPic.ErrLevel >= 0) {
                    ExportImg(ThisPic, EditGame.ResDir + ThisPic.ID + strExt, lngFormat, lngMode, lngZoom);
                }
                if (!blnLoaded) {
                    ThisPic.Unload();
                }
            }
            //done with ProgressWin form
            ProgressWin.Close();
            ProgressWin.Dispose();
            //restore cursor
            MDIMain.UseWaitCursor = false;
        }

        public static void ExportImg(Picture ExportPic, string ExportFile, int ImgFormat, int ImgMode, int ImgZoom) {
            //exports pic gdpImg
            Bitmap ExportBMP;
            int Count = 0;

            //mode:  0=vis
            //       1=pri
            //       2=both
            do {
                //if second time through, adjust output filename
                if (Count == 1) {
                    //get name for pri gdpImg
                    ExportFile = ExportFile.Left(ExportFile.Length - 4) + "_P" + ExportFile.Right(4);
                }
                //if 1st time through AND mode is 0 or 2: vis
                //if second time through OR mode is 1: pri
                if (Count == 0 && ImgMode != 1) {
                    //save vis as temporary BMP
                    ExportBMP = ResizeAGIBitmap(ExportPic.VisualBMP, ImgZoom);
                }
                else {
                    //save vis as temporary BMP
                    ExportBMP = ResizeAGIBitmap(ExportPic.PriorityBMP, ImgZoom);
                }
                //make sure existing file is deleted
                SafeFileDelete(ExportFile);
                //save based on format choice
                switch (ImgFormat) {
                case 1: // bmp
                    ExportBMP.Save(ExportFile);
                    break;
                case 2: // jpg
                    ExportBMP.Save(ExportFile, ImageFormat.Jpeg);
                    break;
                case 3: // gif
                    ExportBMP.Save(ExportFile, ImageFormat.Gif);
                    break;
                case 4: // tif
                    ExportBMP.Save(ExportFile, ImageFormat.Tiff);
                    break;
                case 5: // png
                    ExportBMP.Save(ExportFile, ImageFormat.Png);
                    break;
                }
                Count++;
                if (ImgMode < 2 || Count == 2) {
                    break;
                }
            } while (true);
        }

        public static void ExportOnePicImg(Picture ThisPicture) {
            //exports a picture vis screen and/or pri screen as either bmp or gif, or png
            int lngZoom, lngMode, lngFormat;
            //show options form, save image only
            frmExportPictureOptions frmPEO = new(1);
            if (frmPEO.ShowDialog(MDIMain) == DialogResult.Cancel) {
                frmPEO.Close();
                frmPEO.Dispose();
                return;
            }
            lngZoom = (int)frmPEO.udZoom.Value;
            lngFormat = frmPEO.cmbFormat.SelectedIndex + 1;
            if (frmPEO.optVisual.Checked) {
                lngMode = 0;
            }
            else if (frmPEO.optPriority.Checked) {
                lngMode = 1;
            }
            else {
                lngMode = 2;
            }
            //done with the options form
            frmPEO.Close();
            frmPEO.Dispose();
            //set up commondialog
            MDIMain.SaveDlg.Title = "Save Picture Image As";
            MDIMain.SaveDlg.DefaultExt = "bmp";
            MDIMain.SaveDlg.Filter = "BMP files (*.bmp)|*.bmp|JPEG files (*.jpg)|*.jpg|GIF files (*.gif)|*.gif|TIFF files (*.tif)|*.tif|PNG files (*.PNG)|*.png|All files (*.*)|*.*";
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            MDIMain.SaveDlg.FilterIndex = lngFormat;
            MDIMain.SaveDlg.FileName = "";
            DialogResult rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);
            //if NOT canceled, then export!
            if (rtn != DialogResult.Cancel) {
                DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
                MDIMain.UseWaitCursor = true;
                ExportImg(ThisPicture, MDIMain.SaveDlg.FileName, lngFormat, lngMode, lngZoom);
                MessageBox.Show("Image saved successfully.", "Export Picture Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MDIMain.UseWaitCursor = false;
            }
        }

        public static DialogResult ShowInputDialog(Form owner, string title, string info, ref string input) {
            int offset = 0;
            Form inputBox = new Form();
            inputBox.StartPosition = FormStartPosition.CenterParent;
            inputBox.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            inputBox.MinimizeBox = false;
            inputBox.MaximizeBox = false;
            inputBox.ShowInTaskbar = false;
            if (info.Length > 0) {
                offset = 20;
                inputBox.ClientSize = new(200, 70 + offset);
                Label labelInfo = new() {
                    Size = new System.Drawing.Size(inputBox.ClientSize.Width - 10, 15),
                    Location = new System.Drawing.Point(5, 5),
                    Text = info
                };
                inputBox.Controls.Add(labelInfo);
            }
            else {
                inputBox.ClientSize = new(200, 70);
            }
            inputBox.Text = title;

            TextBox textBox = new() {
                Size = new System.Drawing.Size(inputBox.ClientSize.Width - 10, 23 + offset),
                Location = new System.Drawing.Point(5, 5+ offset),
                Text = input
            };
            inputBox.Controls.Add(textBox);

            Button okButton = new() {
                DialogResult = System.Windows.Forms.DialogResult.OK,
                Name = "okButton",
                Size = new System.Drawing.Size(75, 23),
                Text = "&OK",
                Location = new System.Drawing.Point(inputBox.ClientSize.Width - 160, 39 + offset)
            };
            inputBox.Controls.Add(okButton);

            Button cancelButton = new() {
                DialogResult = System.Windows.Forms.DialogResult.Cancel,
                Name = "cancelButton",
                Size = new System.Drawing.Size(75, 23),
                Text = "&Cancel",
                Location = new System.Drawing.Point(inputBox.ClientSize.Width - 80, 39 + offset)
            };
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            DialogResult result = inputBox.ShowDialog(owner);
            input = textBox.Text;
            return result;
        }
        #endregion
    }
}
