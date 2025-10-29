using EnvDTE;
using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using static WinAGI.Common.BkgdTasks;
using static WinAGI.Editor.frmPicEdit;
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
        // default settings - decompile
        public const int DEFAULT_CODESTYLE = 0;
        // default settings - layout editor

        // string constants
        public const string sLOGED = "Logic Editor - ";
        public const string sPICED = "Picture Editor - ";
        public const string sSNDED = "Sound Editor - ";
        public const string sVIEWED = "View Editor - ";
        public const string CHG_MARKER = "* ";   // changed file marker
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

        // other app level constants
        public const double LG_SCROLL = 0.9; // 0.8
        public const double SM_SCROLL = 0.225; // 0.2

        // string resource offsets
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
        public static string DefIdentifierStyleRegEx = @"(\w|\%|\$|\#|[^\x00-\x7F])(\w|\!|\%|\$|\#|\.|[^\x00-\x7F])+";
        #endregion

        #region Global Enumerations
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
            View,
            Loop,
            EndLoop,
            Cel,
            EndCel,
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
            FindLogic,
            ReplaceLogic,
            FindText,
            ReplaceText,
            FindObject,
            ReplaceObject,
            FindWord,
            ReplaceWord,
            FindObjsLogic,
            ReplaceObjsLogic,
            FindWordsLogic,
            ReplaceWordsLogic, 
            FindNone,       // used to temporarily disable find form
                            // when active form is not searchable
        }
        public enum FindFormAction {
            Find,
            Replace,
            ReplaceAll,
            Cancel,
        }
        public enum NoteTone {
            None,
            Natural,
            Sharp,
            Flat,
        }
        public enum LayoutSelection {
            None,
            Room,
            Exit,
            TransPt,
            Comment,
            ErrPt,
            Multiple,
        }
        public enum ExitLeg {
            NoTransPt = -1,
            FirstLeg,
            SecondLeg,
        }
        public enum ExitDirection {
            Single,
            OneWay,
            BothWays,
        }
        public enum LayoutTool {
            None,
            Select,
            Edge1,
            Edge2,
            Other,
            Room,
            Comment,
        }
        public enum ExitReason {
            None,
            Horizon,
            Right,
            Bottom,
            Left,
            Other,
        }
        public enum ExitType {
            Normal,
            Transfer,
            Error,
        }
        public enum ExitStatus {
            /// <summary>
            /// Exit is drawn in layout editor, and exists in source code correctly.
            /// </summary>
            OK,
            /// <summary>
            /// Exit is drawn in layout editor, but doesn't exist in source code yet.
            /// </summary>
            NewExit,
            /// <summary>
            /// Exit has been changed in the layout editor, but not updated in source code.
            /// </summary>
            Changed,
            /// <summary>
            /// Exit has been deleted in layout editor, but not updated in source code.
            /// </summary>
            Deleted,
        }
        public enum UpdateReason {
            /// <summary>
            /// Room's logid ID is changed.
            /// </summary>
            ChangeID,
            /// <summary>
            /// Room's logic number is changed.
            /// </summary>
            RenumberRoom,
            /// <summary>
            /// Existing room updated in logic editor.
            /// </summary>
            UpdateRoom,
            /// <summary>
            /// New room added or existing room toggled to show in layout editor.
            /// </summary>
            ShowRoom,
            /// <summary>
            /// Room removed by hiding (IsRoom to false), or actual removal from game.
            /// </summary>
            HideRoom,
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
            IfArg = 11,  // variables and flags
            OthArg = 12, // variables and strings
            Values = 13, // variables and bytes
            Logic = 14,
            Picture = 15,
            Sound = 16,
            View = 17,
            ActionCmds = 18,
            TestCmds = 19,
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
            LineBreak,
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
        public struct DisplayNote {
            public int Pos;
            public NoteTone Tone;
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

        public struct MakeGifParams {
            public int Mode; // 0=picture, 1=loop
            public Picture Picture;
            public Loop Loop;
            public GifOptions GifOptions;
            public string Filename;
        }

        public struct SyntaxStyleType {
            public SettingColor Color;
            public SettingFontStyle FontStyle;
        }

        public struct AGISettings {
            public AGISettings() {
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
            // NotifyCompWarn: 
            public SettingBool NotifyCompWarn = new(nameof(NotifyCompWarn), true, "Warnings");
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
            //// WarnPlotPaste
            //public SettingBool WarnPlotPaste = new(nameof(WarnPlotPaste), true, "Warnings");
            
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
            public SettingEResListType ResListType = new(nameof(ResListType), Common.Base.ResListType.TreeList, sGENERAL);
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
            public SettingFloat PicScalePreview = new("PreviewScale", 1, sPICTURES);
            // PicScaleEdit: 
            public SettingFloat PicScaleEdit = new("EditorScale", 2, sPICTURES);
            // CursorMode (as int; conert to enum as needed): 
            public SettingInt CursorMode = new("CursorMode", 0, sPICTURES);

            // ************************************************
            // SOUND SETTINGS
            // ************************************************
            // PlaybackMode
            public SettingInt PlaybackMode = new(nameof(PlaybackMode), 1, sSOUNDS); // 0=PCspeaker, 1=PCjr, 2=Midi
            // ShowKybd
            public SettingBool ShowKeyboard = new(nameof(ShowKeyboard), true, sSOUNDS);
            // NoKeyboardSound
            public SettingBool NoKeyboardSound = new(nameof(NoKeyboardSound), false, sSOUNDS);
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
            public SettingInt ViewAlignV = new(nameof(ViewAlignV), 2, sVIEWS);
            // DefCelH: 
            public SettingByte DefCelH = new(nameof(DefCelH), 32, sVIEWS);
            // DefCelW: 
            public SettingByte DefCelW = new(nameof(DefCelW), 16, sVIEWS);
            // ViewScalePreview: 
            public SettingFloat ViewScalePreview = new("PreviewScale", 3, sVIEWS);
            // ViewScaleEdit: 
            public SettingFloat ViewScaleEdit = new("EditScale", 6, sVIEWS);
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
            // LEUseGrid:
            public SettingBool LEUseGrid = new("UseGrid", true, sLAYOUT);
            // LEShowGrid:
            public SettingBool LEShowGrid = new("PageBoundaries", true, sLAYOUT);
            // LEGridMinor:
            public SettingFloat LEGridMinor = new("GridSizeMinor", 0.1f, sLAYOUT);
            // LEGridMajor:
            public SettingFloat LEGridMajor = new("GridSizeMajor", 1f, sLAYOUT);
            // LEShowPics: false=no pics on rooms when drawn
            public SettingBool LEShowPics = new("ShowPics", true, sLAYOUT);
            // LEShowHidden: false= hidden exits are not shown in layout editor
            public SettingBool LEShowHidden = new("ShowHidden", true, sLAYOUT);
            // LESync: 
            public SettingBool LESync = new("Sync", true, sLAYOUT);
            // LEScale: 
            public SettingInt LEScale = new("Zoom", 6, sLAYOUT);
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

            internal AGISettings Clone() {
                AGISettings clonesettings = new();
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
                clonesettings.NotifyCompWarn = new(NotifyCompWarn);
                clonesettings.NotifyCompFail = new(NotifyCompFail);
                clonesettings.WarnItem0 = new(WarnItem0);
                clonesettings.OpenOnErr = new(OpenOnErr);
                clonesettings.SaveOnCompile = new(SaveOnCompile);
                clonesettings.CompileOnRun = new(CompileOnRun);
                clonesettings.WarnMsgs = new(WarnMsgs);
                clonesettings.WarnEncrypt = new(WarnEncrypt);
                clonesettings.LEDelPicToo = new(LEDelPicToo);
                //clonesettings.WarnPlotPaste = new(WarnPlotPaste);
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
                clonesettings.CursorMode = new(CursorMode);
                // SOUNDS
                clonesettings.ShowKeyboard = new(ShowKeyboard);
                clonesettings.NoKeyboardSound = new(NoKeyboardSound);
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
                clonesettings.LEUseGrid = new(LEUseGrid);
                clonesettings.LEShowGrid = new(LEShowGrid);
                clonesettings.LEGridMinor = new(LEGridMinor);
                clonesettings.LEGridMajor = new(LEGridMajor);
                clonesettings.LEShowPics = new(LEShowPics);
                clonesettings.LEShowHidden = new(LEShowHidden);
                clonesettings.LESync = new(LESync);
                clonesettings.LEScale = new(LEScale);
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

        public struct PicTestInfo {
            // ************************************************
            // PICTURETEST SETTINGS
            // ************************************************
            // PicTest.ObjSpeed: 
            public SettingInt ObjSpeed = new("Speed", 1, sPICTEST);
            // PicTest.ObjPriority: 16 means auto; 4-15 correspond to priority bands
            public SettingInt ObjPriority = new("Priority", 16, sPICTEST);
            // PicTest.ObjRestriction: 0 = no restriction, 1 = restrict to water, 2 = restrict to land
            public SettingInt ObjRestriction = new("Restriction", 0, sPICTEST);
            // PicTest.Horizon:
            public SettingInt Horizon = new("Horizon", 36, sPICTEST);
            // PicTest.IgnoreHorizon
            public SettingBool IgnoreHorizon = new("IgnoreHorizon", false, sPICTEST);
            // PicTest.IgnoreBlocks
            public SettingBool IgnoreBlocks = new("IgnoreBlocks", false, sPICTEST);
            // PicTest.CycleAtRest
            public SettingBool CycleAtRest = new("CycleAtRest", false, sPICTEST);
            public int TestLoop;
            public int TestCel;

            public PicTestInfo() {
            }

            internal PicTestInfo Clone() {
                PicTestInfo clonesettings = new();
                // PICTEST 
                clonesettings.ObjSpeed = new(ObjSpeed);
                clonesettings.ObjPriority = new(ObjPriority);
                clonesettings.ObjRestriction = new(ObjRestriction);
                clonesettings.Horizon = new(Horizon);
                clonesettings.IgnoreHorizon = new(IgnoreHorizon);
                clonesettings.IgnoreBlocks = new(IgnoreBlocks);
                clonesettings.CycleAtRest = new(CycleAtRest);
                clonesettings.TestLoop = TestLoop;
                clonesettings.TestCel = TestCel;
                return clonesettings;
            }
        }

        public struct NewGameResults {
            public string NewID;
            public string Version;
            public string GameDir;
            public string ResDir;
            public string SrcExt;
            public string TemplateDir;
            public bool Failed;
            public Exception Error;
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

            public AGIToken(Place start) {
                Line = start.iLine;
                StartPos = start.iChar;
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

        [Serializable]
        public struct TextChar {
            public byte CharVal;
            public AGIColorIndex BG;
            public AGIColorIndex FG;
        }
        #endregion

        #region Global Variables
        internal static AGIGame EditGame;
        internal static frmMDIMain MDIMain;
        internal static string ProgramDir;
        internal static string DefaultResDir; // location to start searches for resources
        internal static string BrowserStartDir = "";  // location to start searches for game files
        internal static NewGameResults NewResults;
        internal static LoadGameResults LoadResults;
        internal static CompileGameResults CompGameResults;
        internal static AGISettings WinAGISettings = new();
        internal static PicTestInfo PicEditTestSettings = new();
        internal static SettingsFile WinAGISettingsFile;
        internal static frmPreview PreviewWin;
        internal static frmProgress ProgressWin;
        internal static frmCompStatus CompStatusWin;
        internal static StatusStrip MainStatusBar;
        internal static TreeNode RootNode;
        internal static TreeNode[] HdrNode;
        internal static int SelResNum = -1;
        internal static AGIResType SelResType;
        internal static bool Compiling, CompWarnings;
        internal static string WinAGIHelp;
        // navigation queue
        internal static int[] ResQueue;
        internal static int ResQPtr = -1;
        internal static bool DontQueue;
        // editor variables
        internal static List<frmLogicEdit> LogicEditors;
        internal static int LogCount;
        internal static List<frmPicEdit> PictureEditors;
        internal static int PicCount;
        internal static List<frmSoundEdit> SoundEditors;
        internal static int SoundCount;
        internal static List<frmViewEdit> ViewEditors;
        internal static int ViewCount;
        internal static frmObjectEdit ObjectEditor;
        internal static bool OEInUse = false;
        internal static bool DragObject = false;
        internal static int ObjEdCount;
        internal static frmWordsEdit WordEditor;
        internal static bool WEInUse = false;
        internal static bool DragWord = false;
        internal static int WordEdCount;
        internal static frmGlobals GlobalsEditor;
        internal static bool GEInUse;
        internal static frmLayout LayoutEditor;
        internal static bool LEInUse;
        internal static frmMenuEdit MenuEditor;
        internal static bool MEInUse;
        internal static frmTextScreenEdit TextScreenEditor;
        internal static bool TSEInUse;
        internal static int TextCount;
        // lookup lists for logic editor
        // tooltips and define lists
        internal static TDefine[,] IDefLookup = new TDefine[4, 256];
        // for now we will not do lookups on words and invObjects - 
        // if performance is good enough I might consider adding them
        //internal static TDefine[] // ODefLookup()
        //internal static TDefine[] // WDefLookup()
        internal static Snippet[] CodeSnippets;
        internal static GifOptions DefaultVGOptions;
        internal static bool DefUpdateLogics;
        // mru variables
        internal static string[] strMRU = ["", "", "", ""];
        // clipboard variables
        internal const string PICTURE_CB_FMT = "WinAGIPictureData";
        internal const string SOUND_CB_FMT = "WinAGISoundData";
        internal const string VIEW_CB_FMT = "WinAGIViewData";
        internal const string WORDSTOK_CB_FMT = "WinAGIWordData";
        internal const string TXTSCREEN_CB_FMT = "WinAGITextscreenData";
        // default colors
        internal static Color[] DefEGAColor = new Color[16];

        // find/replace variables
        // FindingForm is the actual dialog used to set search parameters
        internal static frmFind FindingForm;
        // global copy of search parameters used by the find form
        internal static string GFindText = "";
        internal static string GReplaceText = "";
        internal static FindDirection GFindDir = FindDirection.All;
        internal static bool GMatchWord = false;
        internal static bool GMatchCase = false;
        internal static FindLocation GLogFindLoc = FindLocation.Current;
        internal static bool GFindSynonym = false;
        internal static int GFindGrpNum;
        internal static int SearchStartPos;
        internal static int SearchStartLog;
        internal static AGIResType SearchType;
        internal static int ObjStartPos;
        internal static int StartWord;
        internal static int StartGrp;
        internal static bool FoundOnce;
        internal static bool ChangingID;
        internal static bool RestartSearch;
        internal static bool ClosedLogics;
        internal static int ReplaceCount;
        internal static bool SearchStartDlg; // true if search started by clicking 'find' or 'find next'
                                             // on FindingForm
        private static int[] LogWin = new int[0];

        // others
        internal static Form HelpParent = null;

        #endregion

        #region Global Static Methods
        public static void AddToQueue(AGIResType ResType, int ResNum) {
            // adds this resource to the navigation queue
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
                // don't add if the current resource matches
                if (ResQueue[ResQPtr] == lngRes) {
                    return;
                }
            }
            // add the res info
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
                // use first font in list
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
            DefUpdateLogics = true;
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

        public static void ExportLoopGIF(Engine.View view, int loopnum) {
            // export a loop as a gif

            using frmExportAnimatedGIF frmExportGIF = new(view, loopnum);
            if (frmExportGIF.ShowDialog(MDIMain) == DialogResult.Cancel) {
                return;
            }
            GifOptions options = frmExportGIF.SelectedGifOptions;

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
            ProgressWin = new (MDIMain) {
                Text = "Exporting Loop as GIF"
            };
            ProgressWin.lblProgress.Text = "Depending on size of loop, this may take awhile. Please wait...";
            ProgressWin.pgbStatus.Visible = false;
            // the build method can be very time consuming so use
            // a background worker
            bgwMakePicGif = new BackgroundWorker();
            bgwMakePicGif.DoWork += new DoWorkEventHandler(BuildPicGifDoWork);
            bgwMakePicGif.ProgressChanged += new ProgressChangedEventHandler(BuildPicGifProgressChanged);
            bgwMakePicGif.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BuildPicGifWorkerCompleted);
            bgwMakePicGif.WorkerReportsProgress = true;
            MakeGifParams gifparams = new() {
                Mode = 1,
                Loop = view[loopnum],
                GifOptions = options,
                Filename = MDIMain.SaveDlg.FileName
            };
            bgwMakePicGif.RunWorkerAsync(gifparams);
            MDIMain.UseWaitCursor = true;
            MDIMain.Refresh();
            ProgressWin.ShowDialog();
        }

        public static bool MakeLoopGif(Loop GifLoop, GifOptions GifOps, string ExportFile) {
            string strTempFile;
            int lngPos; // data that will be written to the gif file
            int lngInPos;
            byte[] bytCmpData, bytCelData; // data used to build then compress cel data as gif Image
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
            // determine size of logical screen by checking size of each cel
            // in loop, and using Max of h/w
            for (i = 0; i < GifLoop.Cels.Count; i++) {
                if (GifLoop[i].Height > MaxH) {
                    MaxH = GifLoop[i].Height;
                }
                if (GifLoop[i].Width > MaxW) {
                    MaxW = GifLoop[i].Width;
                }
            }
            // add logical screen size info
            bytData[6] = (byte)((MaxW * GifOps.Zoom * 2) & 0xFF);
            bytData[7] = (byte)((MaxW * GifOps.Zoom * 2) >> 8);
            bytData[8] = (byte)((MaxH * GifOps.Zoom) & 0xFF);
            bytData[9] = (byte)((MaxH * GifOps.Zoom) >> 8);
            // add color info
            bytData[10] = 243; // 1-111-0-011 means:
                               // global color table,
                               // 8 bits per channel,
                               // no sorting, and
                               // 16 colors in the table
                               // background color:
            bytData[11] = 0;
            // pixel aspect ratio:
            bytData[12] = 0; // should give proper 2:1 ratio for pixels

            // add global color table
            for (i = 0; i < 16; i++) {
                bytData[13 + 3 * i] = GifLoop.Parent.Palette[i].R;
                bytData[14 + 3 * i] = GifLoop.Parent.Palette[i].G;
                bytData[15 + 3 * i] = GifLoop.Parent.Palette[i].B;
            }
            // if cycling, add netscape extension to allow continuous looping
            if (GifOps.Cycle) {
                // byte   1       : 33 (hex 0x21) GIF Extension code
                // byte   2       : 255 (hex 0xFF) Application Extension Label
                // byte   3       : 11 (hex 0x0B) Length of Application Block
                //                  (eleven bytes of data to follow)
                // bytes  4 to 11 : "NETSCAPE"
                // bytes 12 to 14 : "2.0"
                // byte  15       : 3 (hex 0x03) Length of Data static void-Block
                //                  (three bytes of data to follow)
                // byte  16       : 1 (hex 0x01)
                // bytes 17 to 18 : 0 to 65535, an unsigned integer in
                //                  lo-hi byte format. This indicate the
                //                  number of iterations the loop should
                //                  be executed.
                // byte  19       : 0 (hex 0x00) a Data static void-Block Terminator.
                bytData[61] = 0x21;
                bytData[62] = 0xFF;
                bytData[63] = 0xB;
                for (i = 0; i < 11; i++) {
                    bytData[i + 64] = (byte)"NETSCAPE2.0"[i];
                }
                bytData[75] = 3;
                bytData[76] = 1;
                bytData[77] = 0;
                bytData[78] = 0;
                bytData[79] = 0;
                // at this point, numbering is not absolute, so we need to begin
                // tracking the data position
                lngPos = 80;
            }
            else {
                // at this point, numbering is not absolute, so we need to begin
                // tracking the data position
                lngPos = 61;
            }
            // cel size is set to logical screen size
            // (if cell is smaller than logical screen size, it will be padded with
            // transparent cells)
            bytCelData = new byte[(int)(MaxH * MaxW * Math.Pow(GifOps.Zoom, 2) * 2)];

            // make output array large enough to hold all cel data without compression
            Array.Resize(ref bytData, lngPos + (bytCelData.Length + 10) * GifLoop.Cels.Count);
            // add each cel
            for (i = 0; i < GifLoop.Cels.Count; i++) {
                // add graphic control extension for this cel
                bytData[lngPos++] = 0x21;
                bytData[lngPos++] = 0xF9;
                bytData[lngPos++] = 4;
                bytData[lngPos++] = (byte)(GifOps.Transparency ? 13 : 12);  // 000-011-0-x = reserved-restore-no user input-transparency included
                bytData[lngPos++] = (byte)(GifOps.Delay & 0xFF);
                bytData[lngPos++] = (byte)((GifOps.Delay & 0xFF) >> 8);
                if (GifOps.Transparency) {
                    bytData[lngPos++] = (byte)GifLoop[i].TransColor;
                }
                else {
                    bytData[lngPos++] = 0;
                }
                bytData[lngPos++] = 0;
                // add the cel data (first create cel data in separate array
                // then compress the cell data, break it into 255 byte chunks,
                // and add the chunks to the output
                // determine pad values
                celH = GifLoop[i].Height;
                celW = GifLoop[i].Width;
                hPad = (byte)(MaxH - celH);
                wPad = (byte)(MaxW - celW);
                bytTrans = (byte)GifLoop[i].TransColor;
                lngCelPos = 0;

                for (hVal = 0; hVal < MaxH; hVal++) {
                    // repeat each row based on scale factor
                    for (zFacH = 1; zFacH <= GifOps.Zoom; zFacH++) {
                        // step through each pixel in this row
                        for (wVal = 0; wVal < MaxW; wVal++) {
                            // repeat each pixel based on scale factor (x2 because AGI pixels are double-wide)
                            for (zFacW = 1; zFacW <= GifOps.Zoom * 2; zFacW++) {
                                // depending on alignment, may need to pad:
                                if (((hVal < hPad) && (GifOps.VAlign == 1)) || ((hVal > celH - 1) && (GifOps.VAlign == 0))) {
                                    // use a transparent pixel
                                    bytCelData[lngCelPos++] = bytTrans;
                                }
                                else {
                                    if (((wVal < wPad) && (GifOps.HAlign == 1)) || ((wVal > celW - 1) && (GifOps.HAlign == 0))) {
                                        // use a transparent pixel
                                        bytCelData[lngCelPos++] = bytTrans;
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
                                        // use the actual pixel (adjusted for padding, if aligned to bottom or left)
                                        bytCelData[lngCelPos++] = (byte)GifLoop[i][pX, pY];
                                    }
                                }
                            }
                        }
                    }
                }
                // now compress the cel data
                bytCmpData = LZW.GifLZW(bytCelData);

                // add Image descriptor
                bytData[lngPos++] = 0x2C;
                bytData[lngPos++] = 0;
                bytData[lngPos++] = 0;
                bytData[lngPos++] = 0;
                bytData[lngPos++] = 0;
                bytData[lngPos++] = (byte)((byte)(MaxW * GifOps.Zoom * 2) & 0xFF);
                bytData[lngPos++] = (byte)((byte)(MaxW * GifOps.Zoom * 2) >> 8);
                bytData[lngPos++] = (byte)((byte)(MaxH * GifOps.Zoom) & 0xFF);
                bytData[lngPos++] = (byte)((byte)(MaxH * GifOps.Zoom) >> 8);
                bytData[lngPos++] = 0;
                // add byte for initial LZW code size
                bytData[lngPos++] = 4;
                // add the compressed data to filestream
                lngInPos = 0;
                intChunkSize = 0;
                do {
                    if (bytCmpData.Length - lngInPos > 255) {
                        intChunkSize = 255;
                    }
                    else {
                        intChunkSize = (short)(bytCmpData.Length - lngInPos);
                    }
                    // write chunksize
                    bytData[lngPos++] = (byte)intChunkSize;
                    // add this chunk of data
                    for (j = 1; j <= intChunkSize; j++) {
                        bytData[lngPos++] = bytCmpData[lngInPos++];
                    }
                }
                while (lngInPos < bytCmpData.Length);
                // end with a zero-length block
                bytData[lngPos++] = 0;
                // update progress
                bgwMakePicGif.ReportProgress(i);
            }
            // add trailer
            bytData[lngPos++] = 0x3B;
            // resize 
            Array.Resize(ref bytData, lngPos);

            // get temporary file
            strTempFile = Path.GetTempFileName();
            try {
                // open file for output
                using FileStream fsGif = new(strTempFile, FileMode.Open);
                fsGif.Write(bytData);
                fsGif.Dispose();
                // move tempfile to savefile
                File.Move(strTempFile, ExportFile, true);
            }
            catch (Exception e) {
                bgwMakePicGif.ReportProgress(-2, e);
                return false;
            }
            return true;
        }

        public static void ExportPicAsGif(Picture picture) {
            using frmExportAnimatedGIF frmExportGIF = new(picture);
            if (frmExportGIF.ShowDialog(MDIMain) == DialogResult.Cancel) {
                return;
            }
            GifOptions options = frmExportGIF.SelectedGifOptions;
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
            ProgressWin = new (MDIMain) {
                Text = "Exporting Picture as GIF"
            };
            ProgressWin.lblProgress.Text = "Depending on size of picture, this may take awhile. Please wait...";
            ProgressWin.pgbStatus.Visible = true;
            ProgressWin.pgbStatus.Maximum = picture.Size;

            // the build method can be very time consuming so use
            // a background worker
            bgwMakePicGif = new BackgroundWorker();
            bgwMakePicGif.DoWork += new DoWorkEventHandler(BuildPicGifDoWork);
            bgwMakePicGif.ProgressChanged += new ProgressChangedEventHandler(BuildPicGifProgressChanged);
            bgwMakePicGif.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BuildPicGifWorkerCompleted);
            bgwMakePicGif.WorkerReportsProgress = true;
            MakeGifParams gifparams = new() {
                Mode = 0,
                Picture = picture,
                GifOptions = options,
                Filename = MDIMain.SaveDlg.FileName
            };
            bgwMakePicGif.RunWorkerAsync(gifparams);
            MDIMain.UseWaitCursor = true;
            MDIMain.Refresh();
            ProgressWin.ShowDialog();
        }

        public static bool MakePicGif(Picture picture, GifOptions options, string filename) {

            string strTempFile;
            int lngPos; // data that will be written to the gif file
            int lngInPos;
            byte[] bytCmpData, bytPicData; // data used to build then compress pic data as gif Image
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

            // build header
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
            bytData[10] = 243; // 1-111-0-011 means:
                               // global color table,
                               // 8 bits per channel,
                               // no sorting, and
                               // 16 colors in the table
                               // background color:
            bytData[11] = 0;
            // pixel aspect ratio:
            bytData[12] = 0; // should give proper 2:1 ratio for pixels
            for (int i = 0; i < 16; i++) {
                bytData[13 + 3 * i] = picture.Palette[i].R;
                bytData[14 + 3 * i] = picture.Palette[i].G;
                bytData[15 + 3 * i] = picture.Palette[i].B;
            }
            // if cycling, add netscape extension to allow continuous looping
            if (options.Cycle) {
                // byte   1       : 33 (hex 0x21) GIF Extension code
                // byte   2       : 255 (hex 0xFF) Application Extension Label
                // byte   3       : 11 (hex 0x0B) Length of Application Block
                //                  (eleven bytes of data to follow)
                // bytes  4 to 11 : "NETSCAPE"
                // bytes 12 to 14 : "2.0"
                // byte  15       : 3 (hex 0x03) Length of Data static void-Block
                //                  (three bytes of data to follow)
                // byte  16       : 1 (hex 0x01)
                // bytes 17 to 18 : 0 to 65535, an unsigned integer in
                //                  lo-hi byte format. This indicate the
                //                  number of iterations the loop should
                //                  be executed.
                // byte  19       : 0 (hex 0x00) a Data static void-Block Terminator.
                bytData[61] = 0x21;
                bytData[62] = 0xFF;
                bytData[63] = 0xB;
                for (int i = 0; i < 11; i++) {
                    bytData[i + 64] = (byte)"NETSCAPE2.0"[i];
                }
                bytData[75] = 3;
                bytData[76] = 1;
                bytData[77] = 0;
                bytData[78] = 0;
                bytData[79] = 0;
                // at this point, numbering is not absolute, so we need to begin tracking the data position
                lngPos = 80;
            }
            else {
                // at this point, numbering is not absolute, so we need to begin tracking the data position
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
                bytData[lngPos++] = 0x21;
                bytData[lngPos++] = 0xF9;
                bytData[lngPos++] = 4;
                bytData[lngPos++] = 12;   // 000-011-0-0 = reserved-restore-no user input-no transparency
                bytData[lngPos++] = (byte)(options.Delay & 0xFF);
                bytData[lngPos++] = (byte)((options.Delay & 0xFF) >> 8);
                bytData[lngPos++] = 0;
                bytData[lngPos++] = 0;
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

                // add Image descriptor
                bytData[lngPos++] = 0x2C;
                bytData[lngPos++] = 0;
                bytData[lngPos++] = 0;
                bytData[lngPos++] = 0;
                bytData[lngPos++] = 0;
                bytData[lngPos++] = (byte)((MaxW * options.Zoom * 2) & 0xFF);
                bytData[lngPos++] = (byte)((MaxW * options.Zoom * 2) >> 8);
                bytData[lngPos++] = (byte)((MaxH * options.Zoom) & 0xFF);
                bytData[lngPos++] = (byte)((MaxH * options.Zoom) >> 8);
                bytData[lngPos++] = 0;
                // add byte for initial LZW code size
                bytData[lngPos++] = 4;
                // add the compressed data to filestream
                lngInPos = 0;
                intChunkSize = 0;
                do {
                    if (bytCmpData.Length - lngInPos > 255) {
                        intChunkSize = 255;
                    }
                    else {
                        intChunkSize = (short)(bytCmpData.Length - lngInPos);
                    }
                    // write chunksize
                    bytData[lngPos++] = (byte)intChunkSize;
                    // add this chunk of data
                    for (int j = 1; j <= intChunkSize; j++) {
                        bytData[lngPos++] = bytCmpData[lngInPos++];
                    }
                }
                while (lngInPos < bytCmpData.Length);
                // end with a zero-length block
                bytData[lngPos++] = 0;
                // update progress
                bgwMakePicGif.ReportProgress(lngPicPos);
            }
            while (lngPicPos < picture.Size);
            // add trailer
            bytData[lngPos++] = 0x3B;
            // resize 
            Array.Resize(ref bytData, lngPos);
            // get temporary file
            strTempFile = Path.GetTempFileName();
            try {
                using FileStream fsGif = new(strTempFile, FileMode.Open);
                fsGif.Write(bytData);
                fsGif.Dispose();
                // move tempfile to savefile
                File.Move(strTempFile, filename, true);
            }
            catch (Exception e) {
                bgwMakePicGif.ReportProgress(-1, e);
                return false;
            }
            finally {
                if (!loaded) {
                    picture.Unload();
                }
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

            // get a directory for importing
            MDIMain.FolderDlg.Description = "Select the directory of the game you wish to import:";
            MDIMain.FolderDlg.AutoUpgradeEnabled = false;
            DialogResult result = MDIMain.FolderDlg.ShowDialog(MDIMain);
            if (result == DialogResult.OK) {
                ThisGameDir = MDIMain.FolderDlg.SelectedPath;
                if (ThisGameDir.Length == 0) {
                    // user canceled
                    return;
                }
                // ensure trailing backslash
                ThisGameDir = FullDir(ThisGameDir);

                // if a game file exists
                if (File.Exists(ThisGameDir + "*.wag")) {
                    // confirm the import
                    strMsg = "This directory already has a WinAGI game file. Do you still want to import the game in this directory?" +
                             Environment.NewLine + Environment.NewLine + "The existing WinAGI game file will be overwritten if it has the same name as the GameID found in this directory's AGI VOL and DIR files.";

                    if (MessageBox.Show(strMsg, "WinAGI Game File Already Exists", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel) {
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

                // set default resource file directory to game source file directory
                DefaultResDir = EditGame.ResDir;

                strMsg = "Game file '" + EditGame.GameID + ".wag'  has been created.\n\n";
                if (EditGame.ResDirName == "") {
                    // means resdir couldn't be created
                    strMsg += "Unable to create a resource subdirectory. " +
                        "Logic source files and exported resources will be " +
                        "stored in the game directory.";
                }
                else {
                    if (EditGame.ResDirName != DefResDir) {
                        strMsg += "The existing subdirectory '" +
                            EditGame.ResDirName + "' will be used ";
                    }
                    else {
                        strMsg += "The subdirectory '" + EditGame.ResDirName +
                            "' has been created ";
                    }
                    strMsg += "to store logic source files and exported resources. " +
                    "You can change the source directory for this game on the Game " +
                    "Properties dialog.";
                }
                MessageBox.Show(MDIMain,
                    strMsg,
                    "Import Game",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                // does the game have an Amiga OBJECT file? (very rare)
                if (EditGame.InvObjects.AmigaOBJ) {
                    MessageBox.Show(MDIMain,
                        "The OBJECT file for this game is formatted " +
                        "for the Amiga.\n\n" +
                        "If you intend to run this game on a DOS " +
                        "platform, you will need to convert the file " +
                        "to DOS format (use the Convert menu option " +
                        "on the OBJECT Editor's Resource menu)",
                        "Amiga OBJECT File detected",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

            }
        }

        public static bool OpenGame(int mode, string gameSource) {
            // opens a game by directory or wag file depending on mode
            // mode 0 == open source as a wag file
            // mode 1 == open source as a sierra game directory;

            if (EditGame is not null) {
                // close game, if user allows
                if (!CloseThisGame()) {
                    return false;
                }
            }

            // show wait cursor
            MDIMain.UseWaitCursor = true;
            MDIMain.Refresh();
            // show the progress window
            ProgressWin = new (MDIMain) {
                Text = "Loading Game"
            };
            ProgressWin.lblProgress.Text = "Checking WinAGI Game file ...";
            ProgressWin.StartPosition = FormStartPosition.CenterParent;
            ProgressWin.pgbStatus.Visible = false;
            // show loading msg in status bar
            MDIMain.spStatus.Text = (mode == 0 ? "Loading" : "Importing") + " game; please wait...";
            // pass mode and source
            LoadResults = new() {
                Mode = mode,
                Source = gameSource,
                Failed = false,
                ErrorMsg = "",
                Warnings = false
            };
            bgwOpenGame.RunWorkerAsync(LoadResults);
            // idle until the worker is done;
            ProgressWin.ShowDialog();
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
                // make sure warning grid is hidden
                if (MDIMain.pnlWarnings.Visible) {
                    MDIMain.HideWarningList(true);
                }
            }
            UpdateTBGameBtns();
            MDIMain.spStatus.Text = "";
            return !LoadResults.Failed;
        }

        public static bool CloseThisGame() {
            int i, j;
            DialogResult rtn;
            // if no game is open
            if (EditGame is null) {
                // return success
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
                    // check for cancellation
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
            // unload ingame word editor
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
                MenuEditor?.Close();
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
            if (WinAGISettings.ResListType.Value != ResListType.None) {
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
            MDIMain.btnCloseGame.Enabled = EditGame is not null;
            MDIMain.btnRun.Enabled = EditGame is not null;
            MDIMain.btnImportRes.Enabled = EditGame is not null;
            MDIMain.btnLayoutEd.Enabled = EditGame is not null;
        }

        internal static void UpdateTBResourceBtns(AGIResType restype, bool ingame, bool changed, int resnum) {
            MDIMain.btnSaveResource.Enabled = changed;
            switch (restype) {
            case AGIResType.Game:
                MDIMain.btnSaveResource.Text = "Save Resource";
                MDIMain.btnExportRes.Enabled = true;
                MDIMain.btnExportRes.Text = "Export All Resources";
                MDIMain.btnAddRemove.Enabled = false;
                MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[19];
                MDIMain.btnAddRemove.Text = "Add/Remove Resource";
                break;
            case AGIResType.Logic:
            case AGIResType.Picture:
            case AGIResType.Sound:
            case AGIResType.View:
                MDIMain.btnSaveResource.Text = "Save " + restype.ToString();
                MDIMain.btnExportRes.Enabled = true;
                MDIMain.btnExportRes.Text = "Export " + restype.ToString();
                MDIMain.btnAddRemove.Enabled = true;
                if (ingame) {
                    MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[20];
                    MDIMain.btnAddRemove.Text = "Remove " + restype.ToString();
                }
                else {
                    MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[19];
                    MDIMain.btnAddRemove.Text = "Add " + restype.ToString();
                }
                break;
            case AGIResType.Objects:
            case AGIResType.Words:
                MDIMain.btnSaveResource.Text = "Save " + restype.ToString();
                MDIMain.btnExportRes.Enabled = true;
                MDIMain.btnExportRes.Text = "Export " + restype.ToString();
                MDIMain.btnAddRemove.Enabled = false;
                MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[19];
                MDIMain.btnAddRemove.Text = "Add/Remove Resource";
                break;
            case AGIResType.Globals:
                MDIMain.btnSaveResource.Text = "Save " + restype.ToString();
                MDIMain.btnExportRes.Enabled = true;
                MDIMain.btnExportRes.Text = "Export " + restype.ToString();
                MDIMain.btnAddRemove.Enabled = false;
                MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[19];
                MDIMain.btnAddRemove.Text = "Add/Remove Resource";
                break;
            case AGIResType.Menu:
                MDIMain.btnSaveResource.Text = "Update Source Logic";
                MDIMain.btnExportRes.Enabled = false;
                MDIMain.btnExportRes.Text = "Export Resource";
                MDIMain.btnAddRemove.Enabled = false;
                MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[19];
                MDIMain.btnAddRemove.Text = "Add/Remove Resource";
                break;
            default:
                MDIMain.btnSaveResource.Text = "Save Resource";
                MDIMain.btnExportRes.Enabled = false;
                MDIMain.btnExportRes.Text = "Export Resource";
                MDIMain.btnAddRemove.Enabled = false;
                MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[19];
                MDIMain.btnAddRemove.Text = "Add/Remove Resource";
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
            case ResListType.None:
                return;
            case ResListType.TreeList:
                switch (restype) {
                case AGIResType.Logic:
                    TreeNode tmpNode = HdrNode[(int)AGIResType.Logic].Nodes["l" + resnum];
                    if (EditGame.Logics[(byte)tmpNode.Tag].Compiled && 
                        EditGame.Logics[(byte)tmpNode.Tag].Error == ResourceErrorType.NoError) {
                        tmpNode.ForeColor = Color.Black;
                    }
                    else {
                        tmpNode.ForeColor = Color.Red;
                    }
                    tmpNode.Text = ResourceName(EditGame.Logics[(byte)tmpNode.Tag], true);
                    break;
                case AGIResType.Picture:
                    tmpNode = HdrNode[(int)AGIResType.Picture].Nodes["p" + resnum];
                    tmpNode.ForeColor = EditGame.Pictures[(byte)tmpNode.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                    tmpNode.Text = ResourceName(EditGame.Pictures[(byte)tmpNode.Tag], true);
                    break;
                case AGIResType.Sound:
                    tmpNode = HdrNode[(int)AGIResType.Sound].Nodes["s" + resnum];
                    tmpNode.ForeColor = EditGame.Sounds[(byte)tmpNode.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                    tmpNode.Text = ResourceName(EditGame.Sounds[(byte)tmpNode.Tag], true);
                    break;
                case AGIResType.View:
                    tmpNode = HdrNode[(int)AGIResType.View].Nodes["v" + resnum];
                    tmpNode.ForeColor = EditGame.Views[(byte)tmpNode.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                    tmpNode.Text = ResourceName(EditGame.Views[(byte)tmpNode.Tag], true);
                    break;
                }
                break;
            case ResListType.ComboList:
                if (MDIMain.cmbResType.SelectedIndex == (int)(restype + 1)) {
                    switch (restype) {
                    case AGIResType.Logic:
                        ListViewItem tmpItem = MDIMain.lstResources.Items["l" + resnum];
                        if (EditGame.Logics[(byte)tmpItem.Tag].Compiled && 
                            EditGame.Logics[(byte)tmpItem.Tag].Error == ResourceErrorType.NoError) {
                            tmpItem.ForeColor = Color.Black;
                        }
                        else {
                            tmpItem.ForeColor = Color.Red;
                        }
                        tmpItem.Text = ResourceName(EditGame.Logics[(byte)tmpItem.Tag], true);
                        break;
                    case AGIResType.Picture:
                        tmpItem = MDIMain.lstResources.Items["p" + resnum];
                        tmpItem.ForeColor = EditGame.Pictures[(byte)tmpItem.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                        tmpItem.Text = ResourceName(EditGame.Pictures[(byte)tmpItem.Tag], true);
                        break;
                    case AGIResType.Sound:
                        tmpItem = MDIMain.lstResources.Items["s" + resnum];
                        tmpItem.ForeColor = EditGame.Sounds[(byte)tmpItem.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                        tmpItem.Text = ResourceName(EditGame.Pictures[(byte)tmpItem.Tag], true);
                        break;
                    case AGIResType.View:
                        tmpItem = MDIMain.lstResources.Items["v" + resnum];
                        tmpItem.ForeColor = EditGame.Views[(byte)tmpItem.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                        tmpItem.Text = ResourceName(EditGame.Views[(byte)tmpItem.Tag], true);
                        break;
                    }
                }
                break;
            }
            if (WinAGISettings.ShowPreview.Value && !nopreview) {
                if (restype == SelResType && resnum == SelResNum) {
                    // redraw the preview
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
            case ResListType.None:
                return;
            case ResListType.TreeList:
                Debug.Assert(EditGame.GameID.Length != 0);
                // update root
                MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                // refresh logics
                foreach (TreeNode tmpNode in HdrNode[(int)AGIResType.Logic].Nodes) {
                    if (EditGame.Logics[(byte)tmpNode.Tag].Compiled &&
                        EditGame.Logics[(byte)tmpNode.Tag].Error == ResourceErrorType.NoError) {
                        tmpNode.ForeColor = Color.Black;
                    }
                    else {
                        tmpNode.ForeColor = Color.Red;
                    }
                }
                // refresh pictures
                foreach (TreeNode tmpNode in HdrNode[(int)AGIResType.Picture].Nodes) {
                    tmpNode.ForeColor = EditGame.Pictures[(byte)tmpNode.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                }
                // refresh sounds
                foreach (TreeNode tmpNode in HdrNode[(int)AGIResType.Sound].Nodes) {
                    tmpNode.ForeColor = EditGame.Sounds[(byte)tmpNode.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                }
                // refresh views
                foreach (TreeNode tmpNode in HdrNode[(int)AGIResType.View].Nodes) {
                    tmpNode.ForeColor = EditGame.Views[(byte)tmpNode.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                }
                break;
            case ResListType.ComboList:
                // update root
                MDIMain.cmbResType.Items[0] = EditGame.GameID;
                switch (MDIMain.cmbResType.SelectedIndex) {
                case 1:
                    foreach (ListViewItem tmpItem in MDIMain.lstResources.Items) {
                            if (EditGame.Logics[(byte)tmpItem.Tag].Compiled &&
                            EditGame.Logics[(byte)tmpItem.Tag].Error == ResourceErrorType.NoError) {
                            tmpItem.ForeColor = Color.Black;
                        }
                        else {
                            tmpItem.ForeColor = Color.Red;
                        }
                    }
                    break;
                case 2:
                    foreach (ListViewItem tmpItem in MDIMain.lstResources.Items) {
                        tmpItem.ForeColor = EditGame.Pictures[(byte)tmpItem.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                    }
                    break;
                case 3:
                    foreach (ListViewItem tmpItem in MDIMain.lstResources.Items) {
                        tmpItem.ForeColor = EditGame.Sounds[(byte)tmpItem.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                    }
                    break;
                case 4:
                    foreach (ListViewItem tmpItem in MDIMain.lstResources.Items) {
                        tmpItem.ForeColor = EditGame.Views[(byte)tmpItem.Tag].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                    }
                    break;
                }
                break;
            }
        }

        public static void BuildResourceTree() {
            // builds the resource tree list
            // for the current open game
            TreeNode tmpNode;

            switch (WinAGISettings.ResListType.Value) {
            case ResListType.None:
                return;
            case ResListType.TreeList:
                // remove existing resources
                MDIMain.tvwResources.Nodes[0].Nodes[sLOGICS].Nodes.Clear();
                MDIMain.tvwResources.Nodes[0].Nodes[sPICTURES].Nodes.Clear();
                MDIMain.tvwResources.Nodes[0].Nodes[sSOUNDS].Nodes.Clear();
                MDIMain.tvwResources.Nodes[0].Nodes[sVIEWS].Nodes.Clear();
                if (EditGame.GameID.Length != 0) {
                    // update root
                    MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                    // add logics
                    if (EditGame.Logics.Count > 0) {
                        for (int i = 0; i <= 255; i++) {
                            // if a valid resource
                            if (EditGame.Logics.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sLOGICS].Nodes.Add("l" + i, ResourceName(EditGame.Logics[(byte)i], true));
                                tmpNode.Tag = (byte)i;
                                // get compiled status
                                if (EditGame.Logics[(byte)i].Compiled && EditGame.Logics[(byte)i].Error == ResourceErrorType.NoError) {
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
                            // if a valid resource
                            if (EditGame.Pictures.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sPICTURES].Nodes.Add("p" + i, ResourceName(EditGame.Pictures[(byte)i], true));
                                tmpNode.Tag = (byte)i;
                                tmpNode.ForeColor = EditGame.Pictures[(byte)i].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                            }
                        }
                    }
                    if (EditGame.Sounds.Count > 0) {
                        for (int i = 0; i <= 255; i++) {
                            // if a valid resource
                            if (EditGame.Sounds.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sSOUNDS].Nodes.Add("s" + i, ResourceName(EditGame.Sounds[(byte)i], true));
                                tmpNode.Tag = (byte)i;
                                tmpNode.ForeColor = EditGame.Sounds[(byte)i].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                            }
                        }
                    }
                    if (EditGame.Views.Count > 0) {
                        for (int i = 0; i <= 255; i++) {
                            // if a valid resource
                            if (EditGame.Views.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sVIEWS].Nodes.Add("v" + i, ResourceName(EditGame.Views[(byte)i], true));
                                tmpNode.Tag = (byte)i;
                                tmpNode.ForeColor = EditGame.Views[(byte)i].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                            }
                        }
                    }
                }
                break;
            case ResListType.ComboList:
                // update root
                MDIMain.cmbResType.Items[0] = EditGame.GameID;
                // select root
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
            // replaces control codes in SnipText and returns
            // the full expanded text
            // (does not handle argument values; they are left in
            // place until needed when a snippet is inserted into
            // a logic)

            // first check for '%%' - temporarily replace them
            // with char 25
            string retval = SnipText.Replace("%%", ((char)25).ToString());
            // carriage returns/new lines
            retval = retval.Replace("%n", Environment.NewLine);

            // quote marks
            retval = retval.Replace("%q", QUOTECHAR.ToString());

            // tabs
            retval = retval.Replace("%t", new String(' ', WinAGISettings.LogicTabWidth.Value));

            // lastly, restore any forced percent signs
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

            // don't need to worry about open editors; the initial build is
            // only called when a game is first loaded; changes to the
            // ID lookup list are handled by the add/remove resource functions
        }

        public static ArgType DefTypeFromValue(string strValue) {
            if (strValue.Length == 0) {
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
                case 99:
                    // "c"
                    return Ctrl;
                case 102:
                    // "f"
                    return Flag;
                case 105:
                    // "i"
                    return InvItem;
                case 109:
                    // "m"
                    return Msg;
                case 111:
                    // "o"
                    return SObj;
                case 115:
                    // "s"
                    return Str;
                case 118:
                    // "v"
                    return Var;
                case 119:
                    // "w"
                    return Word;
                default:
                    return DefStr;
                }
            }
        }

        public static void CompileGame(string CompGameDir = "", bool RebuildOnly = false) {
            DialogResult rtn = DialogResult.Cancel;
            string strTemp = "";
            bool blnDontAsk = false;

            if (EditGame is null) {
                return;
            }
            // if global editor or layout editor open and unsaved, ask to continue
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
                            LayoutEditor.SaveLayout();
                        }
                    }
                    if (GEInUse) {
                        if (GlobalsEditor.IsChanged) {
                            GlobalsEditor.SaveDefinesList();
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
                            // if on automatic, always say yes
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
                            // get user's response
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
                            // if on automatic, always say yes
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
                    // get a new directory
                    MDIMain.FolderDlg.Description = "Choose target directory for compiled game:";
                    MDIMain.FolderDlg.AddToRecent = false;
                    MDIMain.FolderDlg.InitialDirectory = BrowserStartDir;
                    MDIMain.FolderDlg.OkRequiresInteraction = true;
                    MDIMain.FolderDlg.ShowNewFolderButton = true;
                    if (MDIMain.FolderDlg.ShowDialog() == DialogResult.OK) {
                        // ensure trailing backslash
                        string checkDir = FullDir(MDIMain.FolderDlg.SelectedPath);
                        // if directory already contains game files,
                        if (Directory.GetFiles(checkDir, "*VOL.*").Length != 0) {
                            rtn = MessageBox.Show(MDIMain,
                                "This directory already contains AGI game files. Existing files will be renamed so they will not be lost. Continue with compile?",
                                "Compile Game",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question);
                            if (rtn == DialogResult.Yes) {
                                // keep directory
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
            MDIMain.spStatus.Text = RebuildOnly ? "Rebuilding game files, please wait..." : "Compiling game, please wait...";
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
            MDIMain.spStatus.Text = "";
        }

        internal static bool CompileLogic(frmLogicEdit editor, byte logicnum) {
            // compiles an ingame logic
            // assumes calling function has validated the logic is in fact in a game
            //   if an editor object is also passed, it gets updated based on results
            //   of the compile operation
            //   if no editor, success msg is skipped

            // set flag so compiling doesn't cause unnecessary updates in preview window
            Compiling = true;
            CompWarnings = false;
            if (editor is not null) {
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
                    // TODO: errors that cancel compile need a stronger notification...
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
                        "htm\\winagi\\errors_compiler.htm");
                        WinAGISettings.NotifyCompFail.Value = !blnDontNotify;
                        if (!WinAGISettings.NotifyCompFail.Value) {
                            WinAGISettings.NotifyCompFail.WriteSetting(WinAGISettingsFile);
                        }
                    }
                    MDIMain.spStatus.Text = "ERRORS ENCOUNTERED. " + ResourceName(EditGame.Logics[logicnum], true, true) + " compilation FAILED.";
                    Compiling = false;
                    return false;
                }
            }
            catch (Exception ex) {
                switch (ex.HResult) {
                case WINAGI_ERR + 507:
                    // no data to compile
                    MessageBox.Show(MDIMain,
                        "Nothing to compile!",
                        "Compile Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                case WINAGI_ERR + 517:
                    // no room
                    MessageBox.Show(MDIMain,
                        "All available VOL files are full. There is no room for " +
                        "the compiled logic.",
                        "Compile Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                case WINAGI_ERR + 525:
                    // not in a game (should NEVER get here)
                    MessageBox.Show(MDIMain,
                        "Only logics that are in a game can be compiled.",
                        "Compile Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                case WINAGI_ERR + 528:
                    // unable to read and/or write to VOL file
                    MessageBox.Show(MDIMain,
                        "Error acessing VOL file occurred during compilation:\n" +
                        ((Exception)ex.Data["exception"]).Message + "\n\n" +
                        ((Exception)ex.Data["exception"]).StackTrace + "\n\n" +
                        "The logic has not been updated.",
                        "File Access Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                case WINAGI_ERR + 541:
                    // unable to read and/or write to DIR file
                    MessageBox.Show(MDIMain,
                        "Error acessing DIR file occurred during compilation:\n" +
                        ((Exception)ex.Data["exception"]).Message + "\n\n" +
                        ((Exception)ex.Data["exception"]).StackTrace + "\n\n" +
                        "The logic has not been updated.",
                        "File Access Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    break;
                default:
                    // some other error
                    // shouldn't happen, but just in case...
                    ErrMsgBox(ex,
                        "Error occurred during logic compilation: ",
                        ex.StackTrace,
                        "Compile Error");
                    break;
                }
                Compiling = false;
                return false;
            }
            finally {
                if (loaded) {
                    EditGame.Logics[logicnum].Unload();
                }
            }
            // no error
            if (editor is not null) {
                MDIMain.spStatus.Text = ResourceName(EditGame.Logics[logicnum], true, true) + " successfully compiled.";
            }
            if (CompWarnings) {
                if (WinAGISettings.NotifyCompWarn.Value) {
                    bool blnDontNotify = false;
                    MsgBoxEx.Show(MDIMain,
                        "Logic successfully compiled. One or more conditions were found that you may need to double check.",
                        "Compile Logic",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "Don't show this message again", ref blnDontNotify);
                    WinAGISettings.NotifyCompWarn.Value = !blnDontNotify;
                    if (!WinAGISettings.NotifyCompWarn.Value) {
                        WinAGISettings.NotifyCompWarn.WriteSetting(WinAGISettingsFile);
                    }
                }
            }
            else {
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
            }
            RefreshTree(AGIResType.Logic, logicnum);
            Compiling = false;
            return true;
        }

        public static bool CompileChangedLogics(bool NoMsg = false) {
            DialogResult rtn = DialogResult.Cancel;
            bool blnDontAsk = false;

            // if no game is loaded,
            if (EditGame is null) {
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
            MDIMain.spStatus.Text = "Compiling changed logics, please wait...";
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
            MDIMain.spStatus.Text = "";
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
                        0, 0, WinAGIHelp, "htm\\winagi\\templates.htm");
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
                // if word or Objects Editor open
                if (EditGame is not null) {
                    // close game, if user allows
                    if (!CloseThisGame()) {
                        return;
                    }
                }
                MDIMain.UseWaitCursor = true;
                MDIMain.Refresh();
                ProgressWin = new (MDIMain) {
                    Text = "Creating New Game"
                };
                ProgressWin.lblProgress.Text = "Creating new game resources ...";
                ProgressWin.StartPosition = FormStartPosition.CenterParent;
                ProgressWin.pgbStatus.Visible = false;
                // show newgame msg in status bar
                MDIMain.spStatus.Text = "Creating new game" + (UseTemplate ? " from template" : "") + "; please wait...";
                // pass game info and template info
                NewResults = new() {
                    NewID = propform.txtGameID.Text,
                    Version = propform.cmbVersion.Text,
                    GameDir = propform.DisplayDir,
                    ResDir = propform.txtResDir.Text,
                    SrcExt = propform.txtSrcExt.Text,
                    TemplateDir = strTemplateDir,
                    Failed = false,
                    Error = null,
                    Warnings = false
                };
                // run the worker to create the new game
                bgwNewGame.RunWorkerAsync(NewResults);
                // idle until the worker is done;
                ProgressWin.ShowDialog();
                // reset cursor
                MDIMain.UseWaitCursor = false;
                if (EditGame is not null) {
                    // add wag file to mru, if opened successfully
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
                    if (EditGame.PlatformType != Engine.PlatformType.None) {
                        EditGame.Platform = propform.NewPlatformFile;
                    }
                    EditGame.IncludeIDs = propform.chkResourceIDs.Checked;
                    EditGame.IncludeReserved = propform.chkResDefs.Checked;
                    EditGame.IncludeGlobals = propform.chkGlobals.Checked;
                    EditGame.UseLE = propform.chkUseLE.Checked;
                    // force a save of the property file
                    WinAGISettingsFile.Save();
                    if (EditGame.UseLE) {
                        if (UseTemplate) {
                            if (!File.Exists(EditGame.GameDir + EditGame.GameID + ".wal")) {
                                // create default layout file
                                JsonSerializerOptions jOptions = new JsonSerializerOptions { WriteIndented = true };
                                LayoutFileHeader layoutfile = new() {
                                    Version = frmLayout.LAYOUT_FMT_VERSION,
                                    DrawScale = WinAGISettings.LEScale.Value,
                                    Offset = new()
                                };
                                string output = JsonSerializer.Serialize(layoutfile, jOptions);
                                try {
                                    using FileStream fs = new(EditGame.GameDir + EditGame.GameID + ".wal", FileMode.Create, FileAccess.Write);
                                    fs.Write(Encoding.Default.GetBytes(output));
                                }
                                catch {
                                    // ignore errors
                                }
                            }
                        }
                        else {
                            try {
                                // if template included a layout file, delete it
                                string[] files = Directory.GetFiles(EditGame.GameDir, "*.wal");
                                foreach (string file in files) {
                                    SafeFileDelete(file);
                                }
                            }
                            catch {
                                // ignore errors
                            }
                        }
                    }

                    // set default directory
                    BrowserStartDir = EditGame.GameDir;
                    // set default text file directory to game source file directory
                    DefaultResDir = EditGame.GameDir + EditGame.ResDirName + "\\";
                    // build ID lookup table
                    BuildIDefLookup();

                    // if resource tree is in use, refresh properties of root node
                    if (WinAGISettings.ResListType.Value != ResListType.None) {
                        MDIMain.propertyGrid1.Refresh();
                    }
                }
                else {
                    // make sure warning grid is hidden
                    if (MDIMain.pnlWarnings.Visible) {
                        MDIMain.HideWarningList(true);
                    }
                }
                UpdateTBGameBtns();
                MDIMain.spStatus.Text = "";
                MDIMain.UseWaitCursor = false;
            }
            propform.Dispose();
            return;
        }

        public static DefineNameCheck ValidateID(string NewID, string OldID) {
            // validates if a resource ID is agreeable or not

            if (NewID == OldID) {
                return DefineNameCheck.OK;
            }
            bool sierrasyntax = EditGame is not null && EditGame.SierraSyntax;
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
                if (!char.IsLetterOrDigit(ch)) {
                // if (ch < 'A' || ch > 'z' || (ch >= 91 && ch <= 96)) {
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
            // update resource list
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                break;
            case ResListType.ComboList:
                MDIMain.cmbResType.Items[0] = EditGame.GameID;
                break;
            }
            MDIMain.Text = "WinAGI GDS - " + EditGame.GameID;
            return true;
        }

        public static void ChangeResDir(string newDirName) {
            // validate new dir before changing it
            if (string.Compare(EditGame.ResDirName, newDirName, true) == 0 || newDirName.Length == 0) {
                return;
            }
            if (Path.GetInvalidFileNameChars().Any(newDirName.Contains)) {
                MessageBox.Show(MDIMain,
                    "The specified path contains invalid characters. No change made.",
                    "Invalid Path",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            if (EditGame.GameDir == EditGame.ResDir) {
                // copy resource files, source files, defines (txt) to
                // new directory
                try {
                    // moving from gamedir to a new resdir
                    if (!Directory.Exists(EditGame.GameDir + newDirName)) {
                        // create the new subdirectory
                        Directory.CreateDirectory(EditGame.GameDir + newDirName);
                    }

                    string pattern = "*.lgc";
                    MoveFiles(newDirName, pattern);
                    pattern = "*.txt";
                    MoveFiles(newDirName, pattern);
                    pattern = "*.agl";
                    MoveFiles(newDirName, pattern);
                    pattern = "*.agp";
                    MoveFiles(newDirName, pattern);
                    pattern = "*.ags";
                    MoveFiles(newDirName, pattern);
                    pattern = "*.agv";
                    MoveFiles(newDirName, pattern);
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "An exception occurred when trying to move the " +
                        "logic source files and other resources to the new directory.",
                        ex.StackTrace + "\n\nNo change made.",
                        "Unable to change ResDir");
                    return;
                }
            }
            else {
                // check for existing folder
                if (Path.Exists(EditGame.GameDir + newDirName)) {
                    MessageBox.Show(MDIMain,
                        $"The folder '{newDirName}' already exists. Existing resource " +
                        "direcory cannot be moved. No change made.",
                        "Invalid Path",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                // try renaming the resource dir
                try {
                    Directory.Move(EditGame.ResDir, EditGame.GameDir + newDirName);
                }
                catch (Exception ex) {
                    ErrMsgBox(ex,
                        "An exception occurred when trying to move the resource directory.",
                        ex.StackTrace + "\n\nNo change made.",
                        "Unable to change ResDir");
                }
            }
            EditGame.ResDirName = newDirName;

            static void MoveFiles(string newDirName, string pattern) {
                foreach (var srcFile in Directory.EnumerateFiles(EditGame.GameDir, pattern, SearchOption.TopDirectoryOnly)) {
                    string destFile = Path.Combine(EditGame.GameDir + newDirName, Path.GetFileName(srcFile));
                    File.Move(srcFile, destFile);
                }
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

            // make sure NewWAGFile is at the top (use AddToMRU function to do this easily!)
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
                MDIMain.spStatus.Text = "Rebuilding game with new game ID, please wait...";
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
                MDIMain.spStatus.Text = "";
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
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                if (MDIMain.tvwResources.Nodes[0].Text != EditGame.GameID) {
                    MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                }
                break;
            case ResListType.ComboList:
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
                // if using it, need to close it
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
                          WinAGIHelp, @"htm\winagi\editor_layout.htm");
                    if (LayoutEditor is not null || LayoutEditor.IsChanged) {
                        LayoutEditor.SaveLayout();
                    }
                    LayoutEditor.Close();
                    LayoutEditor.Dispose();
                    LEInUse = false;
                }
            }
            // adjust the menubar and toolbar
            MDIMain.mnuTLayout.Enabled = EditGame.UseLE;
            MDIMain.btnLayoutEd.Enabled = EditGame.UseLE;
        }

        public static void OpenGlobals(bool ForceLoad = false) {
            string strFileName;
            frmGlobals frmNew;

            // if a game is loaded and NOT forcing...
            // open editor if not yet in use
            // or switch to it if it's already open
            if (EditGame is not null && !ForceLoad) {
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
                                // ignore if error (a new file will be created)
                            }
                        }
                    }
                    GlobalsEditor = new frmGlobals();
                    if (GlobalsEditor.LoadGlobalDefines(strFileName, true)) {
                        GlobalsEditor.Show();
                        GlobalsEditor.Activate();
                        GEInUse = true;
                    }
                    else {
                        GlobalsEditor.Close();
                        GlobalsEditor.Dispose();
                        GEInUse = false;
                    }
                    // reset cursor
                    MDIMain.UseWaitCursor = false;
                }
            }
            else {
                // either a game is NOT loaded, OR we are forcing a load from file
                // get a globals file
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
                            // just shift focus
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
                    frmNew.Show();
                    frmNew.Activate();
                }
                else {
                    frmNew.Close();
                    frmNew.Dispose();
                }
                    MDIMain.UseWaitCursor = false;
            }
        }

        public static void OpenLayout() {
            if (EditGame is null) {
                return;
            }
            if (LEInUse) {
                // just bring it in focus
                LayoutEditor.Focus();

                if (LayoutEditor.WindowState == FormWindowState.Minimized) {
                    // if minimized, restore it
                    LayoutEditor.WindowState = FormWindowState.Normal;
                }
            }
            else {
                // open the layout for the current game
                LayoutEditor = new frmLayout();
                LayoutEditor.Text = EditGame.GameID + " - Room Layout";

                // open layout data file
                if (!LayoutEditor.GetLayoutData()) {
                    // there was a problem
                    LayoutEditor.Close();
                    LayoutEditor = null;
                    return;
                }
                LayoutEditor.Show();
                LayoutEditor.Activate();
                // mark editor as in use
                LEInUse = true;
            }
        }

        public static void OpenMenuEditor() {
            if (MEInUse) {
                // just bring it in focus
                MenuEditor.Select();
                if (MenuEditor.WindowState == FormWindowState.Minimized) {
                    // if minimized, restore it
                    MenuEditor.WindowState = FormWindowState.Normal;
                }
            }
            else {
                // open the menu editor for the current game
                MenuEditor = new frmMenuEdit();
                MenuEditor.Text = EditGame is not null ? EditGame.GameID + " - Menu Editor" : "Menu Editor";
                if (MenuEditor.Canceled) {
                    MEInUse = false;
                    MenuEditor.Close();
                    MenuEditor.Dispose();
                    MenuEditor = null;
                    return;
                }
                // resize for optimum viewing
                int hborder = MenuEditor.Width - MenuEditor.splitContainer1.Panel2.Width;
                int vborder = MenuEditor.Height - MenuEditor.splitContainer1.Panel2.Height;
                MenuEditor.Size = new(320 * MenuEditor.PicScale + hborder, 200 * MenuEditor.PicScale + vborder);
                MenuEditor.Show();
                MenuEditor.Activate();
                // mark editor as in use
                MEInUse = true;
            }
        }

        public static void OpenTextscreenEditor() {
            if (TSEInUse) {
                // just bring it in focus
                TextScreenEditor.Select();
                if (TextScreenEditor.WindowState == FormWindowState.Minimized) {
                    // if minimized, restore it
                    TextScreenEditor.WindowState = FormWindowState.Normal;
                }
            }
            else {
                TextScreenEditor = new frmTextScreenEdit();
                TextScreenEditor.Show();
                TextScreenEditor.Activate();
                TSEInUse = true;
            }
        }

        internal static void OpenReservedEditor() {
            frmReserved frm = new();
            frm.ShowDialog(MDIMain);
            frm.Dispose();
            if (MDIMain.ActiveMdiChild.Name == "frmLogicEdit") {
                ((frmLogicEdit)MDIMain.ActiveMdiChild).RestoreFocusHack();
            }
        }

        public static byte GetNewNumber(AGIResType ResType, byte OldResNum) {
            byte newnum;
            bool isroom;
            if (ResType == AGIResType.Logic) {
                isroom = EditGame.Logics[OldResNum].IsRoom;
            }
            else {
                isroom = false;
            }
            using frmGetResourceNum frm = new(isroom ? GetRes.RenumberRoom : GetRes.Renumber, ResType, OldResNum);
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

            // change number for this resource
            switch (ResType) {
            case AGIResType.Logic:
                oldID = EditGame.Logics[OldResNum].ID;
                oldResFile = EditGame.ResDir + EditGame.Logics[OldResNum].ID + ".agl";
                EditGame.Logics.Renumber(OldResNum, NewResNum);
                strCaption = ResourceName(EditGame.Logics[NewResNum], true);
                newID = EditGame.Logics[NewResNum].ID;
                if (EditGame.UseLE) {
                    UpdateExitInfo(UpdateReason.RenumberRoom, OldResNum, null, NewResNum);
                }
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
                // update resource file if ID has changed
                UpdateResFile(ResType, NewResNum, oldResFile);
            }
            // update resource list
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                int lngPos;
                TreeNode tmpNode = HdrNode[(int)ResType];
                // add in new position
                // (add it before removing current, to minimize changes in resource list)
                for (lngPos = 0; lngPos < tmpNode.Nodes.Count; lngPos++) {
                    if ((byte)tmpNode.Nodes[lngPos].Tag > NewResNum) {
                        break;
                    }
                }
                // add to tree
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
            case ResListType.ComboList:
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
            // update the logic tooltip lookup table
            IDefLookup[(int)AGIResType.Logic, OldResNum].Name = "";
            IDefLookup[(int)AGIResType.Logic, OldResNum].Type = ArgType.None;
            IDefLookup[(int)AGIResType.Logic, NewResNum].Name = newID;
            IDefLookup[(int)AGIResType.Logic, NewResNum].Type = Num;
            // then let open logic editors know
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
            bool blnReplace; // used when replacing IDs in logics
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
                // validate return results
                // TODO: why not validate on the form, before returning?
                if (ResType == Objects || ResType == Words) {
                    // only have description, so no need to validate
                    break;
                }
                else {
                    if (strOldID != frmeditprop.NewID) {
                        // validate new id
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
                            strErrMsg = "'" + frmeditprop.NewID + "' is an AGI command, and cannot be used as a resource ID.";
                            break;
                        case DefineNameCheck.TestCommand:
                            strErrMsg = "'" + frmeditprop.NewID + "' is an AGI test command, and cannot be used as a resource ID.";
                            break;
                        case DefineNameCheck.KeyWord:
                            strErrMsg = "'" + frmeditprop.NewID + "' is a compiler reserved word, and cannot be used as a resource ID.";
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
                                strErrMsg = "'" + frmeditprop.NewID + "' is already in use as a resource ID.";
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
                                "htm\\winagi\\managingresources.htm#resourceids");
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
                        // if ID was exactly the same, no change needed
                        break;
                    }
                }
            }
            // id change is acceptable (or it didn't change)
            // return new id
            if (strOldID != frmeditprop.NewID) {
                ResID = frmeditprop.NewID;
                blnReplace = DefUpdateLogics = frmeditprop.chkUpdate.Checked;
                // for ingame resources, update resource files, preview, treelist
                if (InGame) {
                    UpdateGameResID(ResType, ResNum, ResID, strOldResFile, strOldID, blnReplace);
                }
            }
            // if description changed, update it
            if (strOldDesc != frmeditprop.NewDescription) {
                Description = frmeditprop.NewDescription;
                if (InGame) {
                    // update the description
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

        /// <summary>
        /// For ingame resources, update resource files, preview,
        /// treelist.
        /// </summary>
        /// <param name="ResType"></param>
        /// <param name="ResNum"></param>
        /// <param name="ResID"></param>
        /// <param name="strOldResFile"></param>
        /// <param name="strOldID"></param>
        /// <param name="blnReplace"></param>
        public static void UpdateGameResID(AGIResType ResType, int ResNum, string ResID, string strOldResFile, string strOldID, bool blnReplace) {
            // update the logic tooltip lookup table for log/pic/view/snd
            switch (ResType) {
            case AGIResType.Logic:
            case AGIResType.Picture:
            case AGIResType.Sound:
            case AGIResType.View:
                IDefLookup[(int)ResType, ResNum].Name = ResID;

                // if not just a change in text case
                if (!strOldID.Equals(ResID, StringComparison.OrdinalIgnoreCase)) {
                    // update resource file if ID has changed
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
                    FindingForm.ResetSearch();
                    ChangingID = true;
                    ReplaceAll(MDIMain, strOldID, ResID, FindDirection.All, true, true, FindLocation.All, ResType);
                    ChangingID = false;
                }
                break;
            }
            // refresh the property page if visible
            if (MDIMain.propertyGrid1.Visible) {
                MDIMain.propertyGrid1.Refresh();
            }
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
            if (EditGame is null) {
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
                break;
            case AGIResType.Picture:
                MDIMain.SaveDlg.Filter = "WinAGI Picture Resource Files (*.agp)|*.agp|All files (*.*)|*.*";
                MDIMain.SaveDlg.FilterIndex = 0;
                MDIMain.SaveDlg.DefaultExt = "agp";
                break;
            case AGIResType.Sound:
                MDIMain.SaveDlg.Filter = "WinAGI Sound Resource Files (*.ags)|*.ags|All files (*.*)|*.*";
                MDIMain.SaveDlg.FilterIndex = 0;
                MDIMain.SaveDlg.DefaultExt = "ags";
                break;
            case AGIResType.View:
                MDIMain.SaveDlg.Filter = "WinAGI View Resource Files (*.agv)|*.agv|All files (*.*)|*.*";
                MDIMain.SaveDlg.FilterIndex = 0;
                MDIMain.SaveDlg.DefaultExt = "agv";
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
                catch (Exception ex) {
                    // something went wrong
                    ErrMsgBox(ex,
                        "Unable to update Logic Resource File",
                        ex.StackTrace,
                        "Update Logic ID Error");
                }
                if (LEInUse) {
                    // redraw to ensure correct ID is displayed
                    LayoutEditor.DrawLayout(LayoutSelection.Room, ResNum);
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
                catch (Exception ex) {
                    // something went wrong
                    ErrMsgBox(ex,
                        "Unable to update Picture Resource File",
                        ex.StackTrace,
                        "Update Picture ID Error");
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
                catch (Exception ex) {
                    // something went wrong
                    ErrMsgBox(ex,
                        "Unable to update Sound Resource File",
                        ex.StackTrace,
                        "Update Sound ID Error");
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
                catch (Exception ex) {
                    // something went wrong
                    ErrMsgBox(ex,
                        "Unable to update View Resource File",
                        ex.StackTrace,
                        "Update View ID Error");
                }
                break;
            }
        }

        public static string ReadNextBlock(StreamReader sr) {
            // read next line until '\x01', '\x02' or EOF
            StringBuilder sb = new();
            int next = sr.Read();
            while (next != -1 && next != 1 && next != 2) {
                sb.Append((char)next);
                next = sr.Read();
            }
            return sb.Length > 0 ? sb.ToString() : null;
        }

        public static string ReadNextBlock(StreamReader sr, ref long count) {
            // read next line until '\x01', '\x02' or EOF, and return the
            // characters read as a string; count is set to the number of
            // characters read
            StringBuilder sb = new();
            int next = sr.Read();
            count = 0;
            while (next != -1 && next != 1 && next != 2) {
                sb.Append((char)next);
                next = sr.Read();
                count++;
            }
            return sb.Length > 0 ? sb.ToString() : null;
        }

        public static void UpdateLayoutFile(UpdateReason Reason, int LogicNumber, AGIExits NewExits, int NewNum = 0) {
            // updates the layout file with new exit information based on the
            // change indicated by Reason

            // it is possible that file might not exist; if a layout was extracted without
            // being saved, then an update to the layout followed by a call to view
            // a logic would get us here...
            if (!File.Exists(EditGame.GameDir + EditGame.GameID + ".wal")) {
                return;
            }
            // file update strategy depends on reason:
            // RenumberRoom: room's logic number is changed
            //      add new RenumberRoom entry (unless changing back)
            //
            // UpdateRoom: add or replace an update to a room's exits
            //      must have a Room or ShowRoom entry
            //      if an update exists, remove it
            //      then add a new UpdateRoom entry
            //
            // ShowRoom: new room added or existing room toggled to show
            //      must have no entry OR a Room/ShowRoom with a HideRoom entry
            //      if HideRoom exists, remove it, then add UpdateRoom entry
            //      otherwise, add a ShowRoom entry
            //
            // HideRoom: room removed by hiding (IsRoom to false), or actual removal from game
            //      must have a Room entry OR a ShowRoom entry, may have an UpdateRoom entry
            //      if an UpdateRoom entry exists, remove it
            //      then add a HideRoom entry

            FileStream fs;
            try {
                fs = new(EditGame.GameDir + EditGame.GameID + ".wal", FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                // first check for renumber, because it just
                // adds a new entry

                using StreamReader sr = new(fs, Encoding.Default);
                // skip to first update block (find first '\x02')
                int next = sr.Read();
                long pos = 1, count = 0;
                while (next != -1 && next != 2) {
                    next = sr.Read();
                    pos++;
                }
                // pos indicates start of block, not including the separator
                // character; count is length of block, also without the separator

                // read first block
                string strLine = ReadNextBlock(sr, ref count);
                bool found = false;
                while (strLine is not null) {
                    // read each line, finding blocks that belong to this logic number
                    LayoutFileData layoutobj = null;
                    try {
                        // convert to update block
                        layoutobj = JsonSerializer.Deserialize<LayoutFileData>(strLine);
                        switch (Reason) {
                        case UpdateReason.RenumberRoom:
                            if (layoutobj is LFDRenumber renumber) {
                                if (NewNum == renumber.OldNumber && LogicNumber == renumber.Index) {
                                    RemoveBlock(fs, pos - 1, count + 1);
                                    found = true;
                                }
                            }
                            break;
                        case UpdateReason.UpdateRoom:
                        case UpdateReason.HideRoom:
                            // if an update exists, remove it
                            if (layoutobj is LFDUpdate update) {
                                // if this is a room block, check if it matches
                                if (update.Index == LogicNumber) {
                                    // remove this entry
                                    RemoveBlock(fs, pos - 1, count + 1);
                                    found = true;
                                }
                            }
                            break;
                        case UpdateReason.ShowRoom:
                            // must have no entry OR a Room/ShowRoom with a HideRoom entry
                            // if HideRoom exists, remove it
                            if (layoutobj is LFDHideRoom hide) {
                                // if this is a hide block, check if it matches
                                if (hide.Index == LogicNumber) {
                                    // remove this entry
                                    RemoveBlock(fs, pos - 1, count + 1);
                                    // now instead of a showroom, we need an update
                                    found = true;
                                }
                            }
                            break;
                        }
                    }
                    catch (Exception e) {
                        // just ignore blocks with errors
                    }
                    if (found) {
                        break;
                    }
                    // try next block (add one to account for 0x02 code)
                    pos += count + 1;
                    strLine = ReadNextBlock(sr, ref count);
                }
                switch (Reason) {
                case UpdateReason.RenumberRoom:
                    if (!found) {
                        // add a new RenumberRoom entry
                        LFDRenumber renumber = new() {
                            Index = NewNum,
                            OldNumber = LogicNumber,
                        };
                        string renum = '\x02' + JsonSerializer.Serialize(renumber as LayoutFileData, new JsonSerializerOptions { WriteIndented = true });
                        byte[] bytData = Encoding.Default.GetBytes(renum);
                        fs.Seek(0, SeekOrigin.End);
                        fs.Write(bytData, 0, bytData.Length);
                    }
                    break;
                case UpdateReason.UpdateRoom:
                    // add new update block
                    LFDUpdate newupdate = new() {
                        Index = LogicNumber,
                        Visible = true,
                        ShowPic = WinAGISettings.LEShowPics.Value,
                        Exits = NewExits?.ToArray() ?? []
                    };
                    byte[] bytUpdateData = Encoding.Default.GetBytes(
                        '\x02' + JsonSerializer.Serialize(
                            newupdate as LayoutFileData,
                            new JsonSerializerOptions { WriteIndented = true }));
                    fs.Seek(0, SeekOrigin.End);
                    fs.Write(bytUpdateData, 0, bytUpdateData.Length);
                    break;
                case UpdateReason.HideRoom:
                    // add a HideRoom entry
                    LFDHideRoom hide = new() {
                        Index = LogicNumber,
                        Visible = false
                    };
                    string hideStr = '\x02' + JsonSerializer.Serialize(hide as LayoutFileData, new JsonSerializerOptions { WriteIndented = true });
                    byte[] hideData = Encoding.Default.GetBytes(hideStr);
                    fs.Seek(0, SeekOrigin.End);
                    fs.Write(hideData, 0, hideData.Length);
                    break;
                case UpdateReason.ShowRoom:
                    if (found) {
                        LFDUpdate update = new() {
                            Index = LogicNumber,
                            Visible = true,
                            ShowPic = WinAGISettings.LEShowPics.Value
                        };
                        if (NewExits is null) {
                            // if no exits passed, use empty exits
                            update.Exits = [];
                        }
                        else {
                            // use the exits passed
                            update.Exits = NewExits.ToArray();
                        }
                        strLine = '\x02' + JsonSerializer.Serialize(update as LayoutFileData, new JsonSerializerOptions { WriteIndented = true });
                    }
                    else {
                        LFDShowRoom show = new() {
                            Index = LogicNumber,
                            Visible = true,
                            ShowPic = WinAGISettings.LEShowPics.Value
                        };
                        if (NewExits is null) {
                            // if no exits passed, use empty exits
                            show.Exits = [];
                        }
                        else {
                            // use the exits passed
                            show.Exits = NewExits.ToArray();
                        }
                        strLine = '\x02' + JsonSerializer.Serialize(show as LayoutFileData, new JsonSerializerOptions { WriteIndented = true });
                    }
                    byte[] buffer = Encoding.Default.GetBytes(strLine);
                    fs.Seek(0, SeekOrigin.End);
                    fs.Write(buffer, 0, buffer.Length);
                    break;
                }
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception e) {
                // something went wrong -ignore it
            }

            static void ReplaceBlock(FileStream fs, long oldPos, long oldSize, byte[] bytNew) {
                long tailStart = oldPos + oldSize;
                long tailLen = fs.Length - tailStart;
                int newSize = bytNew.Length;

                if (newSize == oldSize) {
                    // Overwrite in place
                    fs.Position = oldPos;
                    fs.Write(bytNew, 0, newSize);
                }
                else if (newSize < oldSize) {
                    // Overwrite, shift tail left, truncate
                    fs.Position = oldPos;
                    fs.Write(bytNew, 0, newSize);

                    // Move tail data left
                    byte[] buffer = new byte[8192];
                    long readPos = tailStart;
                    long writePos = oldPos + newSize;
                    long bytesLeft = tailLen;
                    while (bytesLeft > 0) {
                        fs.Position = readPos;
                        int toRead = (int)Math.Min(buffer.Length, bytesLeft);
                        int bytesRead = fs.Read(buffer, 0, toRead);

                        fs.Position = writePos;
                        fs.Write(buffer, 0, bytesRead);

                        readPos += bytesRead;
                        writePos += bytesRead;
                        bytesLeft -= bytesRead;
                    }
                    fs.SetLength(fs.Length - (oldSize - newSize));
                }
                else // newSize > oldSize
                {
                    // Shift tail right to make room
                    byte[] buffer = new byte[8192];
                    long readPos = tailStart + tailLen;
                    long writePos = readPos + (newSize - oldSize);
                    while (tailLen > 0) {
                        int toMove = (int)Math.Min(buffer.Length, tailLen);
                        readPos -= toMove;
                        writePos -= toMove;

                        fs.Position = readPos;
                        fs.Read(buffer, 0, toMove);

                        fs.Position = writePos;
                        fs.Write(buffer, 0, toMove);

                        tailLen -= toMove;
                    }
                    // Write new data
                    fs.Position = oldPos;
                    fs.Write(bytNew, 0, newSize);
                    fs.SetLength(fs.Length + (newSize - oldSize));
                }
            }
            
            static void RemoveBlock(FileStream fs, long pos, long len) {
                // remove a block of data from the file
                // by shifting the tail left
                long bytesToMove = fs.Length - (pos + len);
                if (bytesToMove > 0) {
                    byte[] buffer = new byte[8192];
                    long readPos = pos + len;
                    long writePos = pos;
                    int bytesRead;
                    while (bytesToMove > 0) {
                        fs.Position = readPos;
                        int toRead = (int)Math.Min(buffer.Length, bytesToMove);
                        bytesRead = fs.Read(buffer, 0, toRead);
                        fs.Position = writePos;
                        fs.Write(buffer, 0, bytesRead);
                        readPos += bytesRead;
                        writePos += bytesRead;
                        bytesToMove -= bytesRead;
                    }
                }
                // Truncate the file to remove the leftover bytes at the end
                fs.SetLength(fs.Length - len);
            }
        }

        public static void UpdateExitInfo(UpdateReason Reason, int LogicNumber, Logic ThisLogic, int NewNum = 0) {
            // updates the layout editor (if it is open) and the layout file
            // (if there is one) whenever exit info for a room is changed
            // 
            // changes that should be captured are:
            //    - saving changes to logic source from the editor (Update)
            //    - changing IsRoom property for existing logic from property
            //      window or logic editor (Show, Hide)
            //    - changing Number of a logic that is a room in resource
            //      list or logic editor (Renumber)
            //    - adding or removing a logic that is a room from
            //      resource menu or editor (Show, Delete)
            // TODO: should ErrPts include the text of the exit room? so
            // it can be checked against globals and local defines?

            AGIExits tmpExits = null;

            // is there an existing layout editor file?
            bool blnSaveWAL = File.Exists(EditGame.GameDir + EditGame.GameID + ".wal");

            // if layout file does not exist AND not editing layout
            if (!blnSaveWAL && !LEInUse) {
                // no file, and editor is not in use;
                // no updates are necessary
                return;
            }
            // id changes only matter if editor is open
            if (Reason == UpdateReason.ChangeID) {
                if (LEInUse) {
                    LayoutEditor.UpdateLayout(Reason, LogicNumber, null, 0);
                }
                return;
            }
            // if showing new/existing room, or updating an existing room,
            if (Reason == UpdateReason.ShowRoom || Reason == UpdateReason.UpdateRoom) {
                // get new exits from the logic that was passed
                tmpExits = ExtractExits(ThisLogic);
            }

            // if a layout file exists, it needs to be updated too
            if (blnSaveWAL) {
                // add line to output file
                UpdateLayoutFile(Reason, LogicNumber, tmpExits, NewNum);
            }

            // if layout editor is open
            if (LEInUse) {
                // use layout editor update method
                LayoutEditor.UpdateLayout(Reason, LogicNumber, tmpExits, NewNum);
                // and redraw to refresh the editor
                LayoutEditor.DrawLayout();
            }
        }

        public static AGIExits ExtractExits(Logic ThisLogic) {
            // extracts the exits from a logic resource
            // and returns them as an AGIExits object
            // (this is used to update the layout editor
            // and the layout data file)

            // analyzes a logic to find 'new.room' commands and builds a new
            // exits object that contains the exit info for the logic
            //
            // NOTE: this analyzes an existing SAVED logic source; not
            // a source that is being edited
            //
            // if the exit id for an exit is new or has changed,
            // the source code is updated, and SAVED
            //
            // transfer point info is not addressed by the extractexits method
            // the calling method must deal with transpts on its own

            // lngCmdLoc is location of 1st character of 'new.room' command

            bool blnIDOK, blnSave = false;
            int lngID = 0;

            // ensure source is loaded
            bool blnLogLoad = ThisLogic.Loaded;
            if (!blnLogLoad) {
                ThisLogic.Load();
            }

            // get source code
            string strLogic = ThisLogic.SourceText;

            AGIExits RoomExits = new();

            // locate first instance of new.room command
            int lngCmdLoc = WinAGIFCTB.FindTokenPos(strLogic, "new.room", 0);

            // loop through exit extraction until all new.room commands are processed
            while (lngCmdLoc != -1) {
                // get exit info
                AGIExit tmpExit = AnalyzeExit(strLogic, ref lngCmdLoc);

                // find end of line by searching for crlf
                int lineend = strLogic.IndexOf(Environment.NewLine, lngCmdLoc);
                // if no line end found, means we are on last line; set
                // j to a value of lngCmdLoc+1 so we get the last char of the line
                if (lineend == 0)
                    lineend = lngCmdLoc + 1;
                // get rest of line (to check against existing exits)
                string strLine = strLogic[lngCmdLoc..lineend].Trim();

                // check line for a ##LE marker:
                //  first, strip off comment marker
                bool hascomment = false;
                if (strLine.Left(1) == "[") {
                    strLine = strLine.Right(strLine.Length - 1).Trim();
                    hascomment = true;
                }
                else if (strLine.Left(2) == "//") {
                    strLine = strLine.Right(strLine.Length - 2).Trim();
                    hascomment = true;
                }
                //  next, look for ##LE tag
                if (hascomment && strLine.Left(4) == "##LE") {
                    // strip off leader to expose exit id number
                    strLine = strLine[4..];//   .Right(strLine.Length - 4);
                    if (strLine.Right(2) == "##") {
                        strLine = strLine[..^2];
                    }
                }
                else {
                    // not an exit marker; reset the string
                    strLine = "";
                }
                // if a valid id Value was found
                if (strLine.Length != 0) {
                    // an id may exist
                    // assum ok until proven otherwise
                    blnIDOK = true;
                    // get the id number
                    lngID = strLine.IntVal();
                    // if not a number (val=0) then no marker
                    if (lngID == 0) {
                        blnIDOK = false;
                    }
                    else {
                        // check for this marker among current exits
                        for (int i = 0; i < RoomExits.Count; i++) {
                            if (RoomExits[i].ID.Right(3).IntVal() == lngID) {
                                // this ID has already been added by the editor;
                                // it needs to be reset
                                blnIDOK = false;
                                break;
                            }
                        }
                    }
                }
                else {
                    // no previous marker; assign id automatically
                    blnIDOK = false;
                }
                // if previous ID needs updating (or one not found)
                if (!blnIDOK) {
                    // get next available id number
                    lngID = 0;
                    bool found;
                    do {
                        lngID++;
                        found = true;
                        for (int i = 0; i < RoomExits.Count; i++) {
                            if (RoomExits[i].ID.Right(3).IntVal() == lngID) {
                                found = false;
                                break;
                            }
                        }
                    } while (!found);
                }
                // exit is ok
                ExitStatus tmpStatus = ExitStatus.OK;
                // add exit to logic, and flag as in game and ok
                RoomExits.Add(lngID, tmpExit.Room, tmpExit.Reason, tmpExit.Style).Status = tmpStatus;

                // if id is new or changed,
                if (!blnIDOK) {
                    // lngCmdLoc marks end of new.line command
                    // lineend marks end of the line
                    lineend = strLogic.IndexOf('\r', lngCmdLoc);
                    // use end of logic, if on last line (i=0)
                    // insert exit info into logic source
                    strLogic = strLogic.Left(lngCmdLoc) + " [ ##LE" + lngID.ToString("000") + "##" + strLogic.Right(strLogic.Length - lineend);
                    // set save flag
                    blnSave = true;
                }
                
                // get next new.room cmd
                lngCmdLoc = WinAGIFCTB.FindTokenPos(strLogic, "new.room", ++lngCmdLoc);
            }
            // if changes made to exit ids
            if (blnSave) {
                // replace sourcecode
                ThisLogic.SourceText = strLogic;
                Debug.Assert(ThisLogic == EditGame.Logics[ThisLogic.Number]);
                Debug.Assert(ThisLogic.InGame);

                if (!ThisLogic.InGame) {
                    // save id
                    strLogic = ThisLogic.ID;
                }
                ThisLogic.SaveSource();
                if (!ThisLogic.InGame) {
                    ThisLogic.ID = strLogic;
                }
                // now need to make sure tree is up to date (and preview window, if this
                // logic happens to be the one being previewed)
                RefreshTree(AGIResType.Logic, ThisLogic.Number);
            }
            // unload OR load if necessary
            if (!blnLogLoad && ThisLogic.Loaded) {
                ThisLogic.Unload();
            }
            else if (blnLogLoad && !ThisLogic.Loaded) {
                ThisLogic.Load();
            }

            // return the new exit list
            return RoomExits;
        }

        internal static AGIExit AnalyzeExit(string strSource, ref int lngLoc) {
            // analyzes the exit info associated with the 'new.room' command
            // located at lngLoc in strSource
            //
            // returns an agiexit object with exit info
            //
            // lngLoc is also changed to point to the end of the 'new.room' command
            // to allow insertion of layouteditor marker (at the position of an
            // existing comment, or at end of line following the command)

            ExitReason reason = ExitReason.None;
            int intStyle = 0;
            int lngEnd = 0;
            int room = 0;
            bool blnGood = false;
            AGIToken token = new();

            // get room Value first:
            token.EndPos = lngLoc + 8; // length of 'new.room
            token = WinAGIFCTB.NextToken(strSource, token);
            if (token.Text == "(") {
                // next cmd should be the Value we are looking for
                token = WinAGIFCTB.NextToken(strSource, token);
                room = ArgFromToken(token.Text).IntVal();
                // next token should be ')'
                token = WinAGIFCTB.NextToken(strSource, token);
                if (token.Text == ")") {
                    // and then next token should be ';'
                    token = WinAGIFCTB.NextToken(strSource, token);
                    if (token.Text == ";") {
                        blnGood = true;
                        lngEnd = token.EndPos;
                    }
                }
            }
            else {
                // if no parenthesis, set room arg to zero
                room = 0;
            }
            // validate room
            if (room < 0 || room > 255) {
                room = 0;
            }
            // if syntax is bad, use end of line, or comment start
            if (!blnGood) {
                // find first comment after the 'new.room' command
                // or end of line/end if text if no comment found
                token.EndPos = lngLoc + 8; // length of 'new.room
                do {
                    token = WinAGIFCTB.NextToken(strSource, token);
                } while (token.Type != AGITokenType.None &&
                        token.Type != AGITokenType.LineBreak &&
                        token.Type != AGITokenType.Comment);
                lngEnd = token.StartPos;
            }
            // next step, go backwards to find the 'if' statement that
            // precedes this 'new.room' command and determine style of
            // exit (complex or simple)
            token.StartPos = lngLoc;
            token = WinAGIFCTB.PreviousToken(strSource, token);
            while (token.Text != "if") {
                if (token.Type == AGITokenType.None) {
                    break;
                }
                token = WinAGIFCTB.PreviousToken(strSource, token);
            }
            // token = 'if' or it's at beginning of logic
            if (token.Type == AGITokenType.None || token.Text != "if") {
                // no 'if' found, so this is an 'other' exit
                reason = ExitReason.Other;
            }
            else {
                // now examine the 'if' statement to determine exit Type
                // expected syntax:
                //    if ({test}) {new.room(##)
                // (agi syntax allows cr's inbetween any of the elements)
                // comments could also exist between any elements either as
                // line comments in conjunction with a cr, or as a block comment
                // other commands could exist between '{' and 'new.room'

                // expecting '('
                token = WinAGIFCTB.NextToken(strSource, token);
                if (token.Text != "(") {
                    // unknown exit
                    reason = ExitReason.Other;
                }
                else {
                    // expecting 'v2'
                    token = WinAGIFCTB.NextToken(strSource, token);
                    if (ArgFromToken(token.Text) != "v2") {
                        // unknown reason
                        reason = ExitReason.Other;
                    }
                    else {
                        // expecting '=='
                        token = WinAGIFCTB.NextToken(strSource, token);
                        if (token.Text != "==") {
                            reason = ExitReason.Other;
                        }
                        else {
                            // expecting valid exit reason (1 to 4)
                            token = WinAGIFCTB.NextToken(strSource, token);
                            int argval = ArgFromToken(token.Text).IntVal();
                            if (argval > 0 && argval < 5) {
                                reason = (ExitReason)argval;
                                // expecting ')'
                                token = WinAGIFCTB.NextToken(strSource, token);
                                if (token.Text != ")") {
                                    reason = ExitReason.Other;
                                }
                            }
                            else {
                                // unknown reason
                                reason = ExitReason.Other;
                            }
                        }
                    }
                }
                // Style is currently not implemented leaving it at 0 is fine
                //intStyle = 0;
            }
            AGIExit retval = new() {
                Reason = reason,
                Room = room,
                Style = intStyle,
            };
            if (room > 0 && EditGame.Logics.Contains(room)) {
                retval.Status = ExitStatus.OK;
                if (!EditGame.Logics[room].IsRoom) {
                    retval.Hidden = true;
                }
            }
            else {
                // ok, but will be marked as an error
                retval.Status = ExitStatus.OK;
            }
            // update end pos
            lngLoc = lngEnd;
            // return exit info
            return retval;
        }

        public static string ArgFromToken(string text) {
            // checks the reserved defines and global defines lists 
            // for the passed string and returns the argument value
            // if found (returns original string if not found)

            // check resource IDs first
            for (int j = 0; j < 4; j++) {
                for (int i = 0; i < 256; i++) {
                    if (IDefLookup[j, i].Type == Engine.ArgType.Num &&
                        IDefLookup[j, i].Name == text) {
                        return IDefLookup[j, i].Value;
                    }
                }
            }
            // then global defines
            if (EditGame is not null) {
                for (int i = 0; i < EditGame.GlobalDefines.Count; i++) {
                    if (EditGame.GlobalDefines[i].Name == text) {
                        return EditGame.GlobalDefines[i].Value;
                    }
                }
            }
            // lastly, check for reserved defines option (if not looking for a resourceID)
            if (EditGame is null && WinAGISettings.DefIncludeReserved.Value) {
                Debug.Assert(false);
            }
            if (EditGame is not null && EditGame.IncludeReserved) {
                TDefine[] tmpDefines = EditGame.ReservedDefines.All();
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (tmpDefines[i].Name == text) {
                        return tmpDefines[i].Value;
                    }
                }
            }
            // if not found, return the input
            return text;
        }

        public static void AddNewLogic(byte NewLogicNumber, Logic NewLogic) {
            EditGame.Logics.Add((byte)NewLogicNumber, NewLogic);
            EditGame.Logics[NewLogicNumber].SaveProps();
            // always save source to new name
            EditGame.Logics[NewLogicNumber].SaveSource();
            // if using layout editor AND isroom
            if (EditGame.UseLE && EditGame.Logics[NewLogicNumber].IsRoom) {
                // update layout editor and layout data file to show this room is in the game
                UpdateExitInfo(UpdateReason.ShowRoom, NewLogicNumber, EditGame.Logics[NewLogicNumber]);
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
            if (LEInUse) {
                LayoutEditor.UpdatePictureStatus(NewPictureNumber, true);
            }
        }

        public static void AddNewSound(byte NewSoundNumber, Sound NewSound) {
            EditGame.Sounds.Add((byte)NewSoundNumber, NewSound);
            EditGame.Sounds[NewSoundNumber].SaveProps();
            MDIMain.AddResourceToList(AGIResType.Sound, NewSoundNumber);
            EditGame.Sounds[NewSoundNumber].Unload();
        }

        public static void AddNewView(byte NewViewNumber, Engine.View NewView) {
            EditGame.Views.Add(NewViewNumber, NewView);
            EditGame.Views[NewViewNumber].SaveProps();
            MDIMain.AddResourceToList(AGIResType.View, NewViewNumber);
            EditGame.Views[NewViewNumber].Unload();
        }

        public static void NewLogic(string ImportLogicFile = "") {
            // if game is open, give player option to add new logic to game
            // or create a new standalone logic
            //
            // if no game open, just create a new standalone logic

            // creates a new logic resource and opens an editor
            frmLogicEdit frmNew;
            Logic tmpLogic;
            bool blnOpen = false, blnImporting = false;
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
                // check if logic is a compiled logic
                // (check for existence of char '0')
                bool blnSource = !strFile.Contains('\0');
                // import the logic (and check for error)
                if (blnSource) {
                    tmpLogic.ImportSource(ImportLogicFile);
                    if (tmpLogic.SourceError != ResourceErrorType.NoError) {
                        MDIMain.UseWaitCursor = true;
                        switch (tmpLogic.SourceError) {
                        case ResourceErrorType.LogicSourceIsReadonly:
                            MessageBox.Show(MDIMain,
                                "This logic source file is marked 'readonly'. WinAGI requires write-access to edit source files.",
                                "Read only Files not Allowed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            break;
                        case ResourceErrorType.LogicSourceAccessError:
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
                else {
                    try {
                        tmpLogic.Import(ImportLogicFile);
                    }
                    catch (Exception ex) {
                        MDIMain.UseWaitCursor = false;
                        ErrMsgBox(ex,
                            "Unable to load this logic resource. It can't be decompiled, " +
                            "and does not appear to be a text file.",
                            ex.StackTrace,
                            "Invalid Logic Resource");
                        return;
                    }
                    // check for errors
                    if (tmpLogic.SourceError == ResourceErrorType.LogicSourceDecompileError) {
                        MessageBox.Show(MDIMain,
                            "Errors were encountered when decompiling this logic. " +
                            "The logic may be corrupt. Check the output carefully and make" +
                            "any corrections as needed.",
                            "Logic Decompilation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            if (EditGame is not null) {
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
                    if (!blnImporting) {
                        // for new logics not being added,
                        // build default logic source
                        if (GetResNum.chkRoom.Checked) {
                            // add template text
                            tmpLogic.SourceText = LogTemplateText(GetResNum.txtID.Text, GetResNum.txtDescription.Text);
                        }
                        else {
                            // add default text
                            List<string> src =
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
                }
                else {
                    // show wait cursor while resource is added
                    MDIMain.UseWaitCursor = true;
                    tmpLogic.ID = GetResNum.txtID.Text;

                    // update id for ingame resources
                    bool blnTemplate = GetResNum.chkRoom.Checked;
                    if (!blnImporting) {
                        // for new resources, need to set the source text
                        tmpLogic.SourceText = NewLogicSourceText(tmpLogic, blnTemplate);
                    }

                    // set isroom status based on template
                    tmpLogic.IsRoom = GetResNum.NewResNum != 0 && blnTemplate;

                    // add Logic
                    AddNewLogic(GetResNum.NewResNum, tmpLogic);
                    // reset tmplogic to point to the new game logic
                    tmpLogic = EditGame.Logics[GetResNum.NewResNum];

                    // if including picture
                    if (GetResNum.chkIncludePic.Checked) {
                        AddRoomPicture(GetResNum.NewResNum, GetResNum.txtID.Text);
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

        public static string NewLogicSourceText(Logic tmpLogic, bool blnTemplate) {
            // create the new logic text
            string strLogic;
            // if not importing, we need to add boilerplate text
            if (blnTemplate) {
                // add template text to logic source
                bool changed = false;
                strLogic = CheckIncludes(LogTemplateText(tmpLogic.ID, tmpLogic.Description), EditGame, ref changed);
            }
            else {
                // add default text
                List<string> src =
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

            return strLogic;
        }

        public static void AddRoomPicture(byte picnumber, string idtext) {
            Picture tmpPic = new();
            string strFile = "";
            // help user out if they chose a naming scheme
            if (idtext.Length >= 3 && idtext[..2].Equals("rm", StringComparison.OrdinalIgnoreCase)) {
                // change ID (if able)
                if (ValidateID("pic" + idtext[2..], "") == 0) {
                    tmpPic.ID = "pic" + idtext[2..];
                    if (EditGame.Pictures.Contains(picnumber)) {
                        strFile = EditGame.ResDir + EditGame.Pictures[picnumber].ID + ".agp";
                        UpdateResFile(AGIResType.Picture, picnumber, strFile);
                    }
                }
                else {
                    tmpPic.ID = "Picture" + picnumber;
                }
            }
            else {
                tmpPic.ID = "Picture" + picnumber;
            }
            // if replacing an existing pic
            if (EditGame.Pictures.Contains(picnumber)) {
                RemovePicture(picnumber);
            }
            AddNewPicture(picnumber, tmpPic);
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

        public static void NewPicture(string ImportPictureFile = "") {
            // creates a new picture resource and optionally opens an editor
            frmPicEdit frmNew;
            Picture tmpPic;
            bool blnOpen = false, blnImporting = false;

            MDIMain.UseWaitCursor = true;
            tmpPic = new Picture();
            if (ImportPictureFile.Length != 0) {
                blnImporting = true;
                if (!ImportPicture(ImportPictureFile, tmpPic)) {
                    return;
                }
            }
            if (EditGame is not null) {
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
                    frmNew.Close();
                    frmNew.Dispose();
                }
            }
            WinAGISettings.OpenNew.Value = blnOpen;
            MDIMain.UseWaitCursor = false;
        }

        internal static bool ImportPicture(string importfile, Picture tmpPic) {
            // TODO: importing loads the resource; need to replace the
            // exceptions in import with error numbers
            try {
                tmpPic.Import(importfile);
            }
            catch (Exception ex) {
                // something wrong
                MDIMain.UseWaitCursor = false;
                ErrMsgBox(ex,
                    "Error while importing picture:",
                    ex.StackTrace + "\n\nUnable to load this picture resource.",
                    "Import Picture Error");
                return false;
            }
            // now check to see if it's a valid picture resource
            if (tmpPic.Error != ResourceErrorType.NoError) {
                string errmsg = "";
                switch (tmpPic.Error) {
                case ResourceErrorType.FileNotFound:
                    errmsg = "File not found.";
                    break;
                case ResourceErrorType.FileIsReadonly:
                    errmsg = "File access is readonly. WinAGI requires full access.";
                    break;
                case ResourceErrorType.FileAccessError:
                    errmsg = "File access error. Unable to read the import file.";
                    break;
                default:
                    // no other errors should be possible
                    Debug.Assert(false);
                    break;
                }
                MessageBox.Show(MDIMain,
                    errmsg,
                    "Unable to Import Picture",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                // restore main form mousepointer and exit
                MDIMain.UseWaitCursor = false;
                return false;
            }
            return true;
        }

        public static void NewSound() {
            NewSound("", SoundImportFormat.AGI, null);
        }

        public static void NewSound(string importfile, SoundImportFormat format, SoundImportOptions options) {
            // creates a new sound resource and opens an editor
            frmSoundEdit frmNew;
            Sound tmpSound;
            bool blnOpen = false, blnImporting = false;

            MDIMain.UseWaitCursor = true;
            tmpSound = new Sound();
            if (importfile.Length != 0) {
                blnImporting = true;
                if (!ImportSound(importfile, tmpSound, format, options)) {
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
            if (EditGame is not null) {
                frmGetResourceNum GetResNum = new(blnImporting ? GetRes.Import : GetRes.AddNew, AGIResType.Sound);
                if (blnImporting) {
                    GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(importfile).Replace(" ", "");
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
                    frmNew.Close();
                    frmNew.Dispose();
                }
            }
            WinAGISettings.OpenNew.Value = blnOpen;
            MDIMain.UseWaitCursor = false;
        }

        internal static bool ImportSound(string importfile, Sound tmpSound, SoundImportFormat format, SoundImportOptions options) {
            // TODO: importing loads the resource; need to replace the
            // exceptions in import with error numbers
            // import the sound and (and check for error)
            try {
                tmpSound.Import(importfile, format, options);
            }
            catch (Exception ex) {
                // something wrong
                ErrMsgBox(ex,
                    "Error occurred while importing sound:",
                    ex.StackTrace + "\n\nUnable to load this sound resource", "Import Sound Error");
                MDIMain.UseWaitCursor = false;
                return false;
            }
            if (tmpSound.Error != ResourceErrorType.NoError) {
                string errmsg = "";
                switch (tmpSound.Error) {
                case ResourceErrorType.FileNotFound:
                    errmsg = "File not found.";
                    break;
                case ResourceErrorType.FileIsReadonly:
                    errmsg = "File access is readonly. WinAGI requires full access.";
                    break;
                case ResourceErrorType.FileAccessError:
                    errmsg = "File access error. Unable to read the import file.";
                    break;
                case ResourceErrorType.SoundNoData:
                case ResourceErrorType.SoundBadTracks:
                    errmsg = "Import file does not contain a valid sound resource.";
                    break;
                default:
                    // no other errors should be possible
                    Debug.Assert(false);
                    break;
                }
                MDIMain.UseWaitCursor = false;
                MessageBox.Show(MDIMain,
                    errmsg,
                    "Unable to Import Sound",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public static void NewView(string ImportViewFile = "") {
            // creates a new view and opens an editor
            frmViewEdit frmNew;
            Engine.View tmpView;
            bool blnOpen = false, blnImporting = false;

            MDIMain.UseWaitCursor = true;
            tmpView = new Engine.View();
            if (ImportViewFile.Length != 0) {
                blnImporting = true;
                if (!ImportView(ImportViewFile, tmpView)) {
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
            if (EditGame is not null) {
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
                    frmNew.Close();
                    frmNew.Dispose();
                }
            }
            WinAGISettings.OpenNew.Value = blnOpen;
            MDIMain.UseWaitCursor = false;
        }

        internal static bool ImportView(string importfile, Engine.View tmpView) {
            // TODO: importing loads the resource; need to replace the
            // exceptions in import with error numbers
            try {
                tmpView.Import(importfile);
            }
            catch (Exception ex) {
                // something wrong
                MDIMain.UseWaitCursor = false;
                ErrMsgBox(ex,
                    "Error while importing view:",
                    ex.StackTrace + "\n\nUnable to load this view resource.",
                    "Import View Error");
                return false;
            }
            // now check to see if it's a valid view resource (by trying to reload it)
            if (tmpView.Error != ResourceErrorType.NoError) {
                string errmsg = "";
                switch (tmpView.Error) {
                case ResourceErrorType.FileNotFound:
                    errmsg = "File not found.";
                    break;
                case ResourceErrorType.FileIsReadonly:
                    errmsg = "File access is readonly. WinAGI requires full access.";
                    break;
                case ResourceErrorType.FileAccessError:
                    errmsg = "File access error. Unable to read the import file.";
                    break;
                case ResourceErrorType.ViewNoData:
                case ResourceErrorType.ViewNoLoops:
                    errmsg = "Import file does not contain a valid view resource.";
                    break;
                default:
                    // no other errors should be possible
                    Debug.Assert(false);
                    break;
                }
                MessageBox.Show(MDIMain,
                    errmsg,
                    "Unable to Import View",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                // restore main form mousepointer and exit
                MDIMain.UseWaitCursor = false;
                return false;
            }
            return true;
        }

        public static void NewInvObjList(string ImportObjFile = "", bool skipcheck = false) {
            // create a new InvObject list and opens an editor
            frmObjectEdit frmNew;
            InventoryList tmpList;

            MDIMain.UseWaitCursor = true;
            if (ImportObjFile.Length != 0) {
                tmpList = new(ImportObjFile);
            }
            else {
                tmpList = [];
            }
            frmNew = new();
            if (frmNew.LoadOBJECT(tmpList)) {
                frmNew.Show();
            }
            else {
                frmNew.Close();
                frmNew.Dispose();
                MDIMain.UseWaitCursor = false;
                return;
            }
            // a game is loaded; find out if user wants this object file to replace existing
            if (!skipcheck && EditGame is not null) {
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

        public static void NewWordList(string ImportWordFile = "", bool skipcheck = false) {
            // create a new WordList object and open an editor
            frmWordsEdit frmNew;
            WordList tmpList;

            // show wait cursor
            MDIMain.UseWaitCursor = true;
            if (ImportWordFile.Length != 0) {
                try {
                    tmpList = new(ImportWordFile);
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, 
                        "An error occurred during import:",
                        ex.StackTrace,
                        "Import WORDS.TOK File Error");
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
                frmNew.Close();
                frmNew.Dispose();
                MDIMain.UseWaitCursor = false;
                return;
            }
            // a game is loaded; find out if user wants this words.tok file to replace existing
            if (!skipcheck && EditGame is not null) {
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
                if (EditGame is null) {
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
                MDIMain.OpenDlg.Filter = $"WinAGI Logic Source files (*.{defext})|*.{defext}{textext}|Logic Resources (*.agl)|*.agl|All files (*.*)|*.*";
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
                MDIMain.OpenDlg.Filter = "WinAGI Sound Resource files (*.ags)|*.ags|" + 
                                         "MIDI files (*.mid)|*.mid|" +
                                         "Impulse Tracker files (*.it)|*.it|" +
                                         "Protracker files (*.mod)|*.mod|" +
                                         "AGI Sound Script files (*.ass)|*.ass|" + 
                                         "All files (*.*)|*.*";
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
            Debug.Assert(EditGame is not null);

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
            if (frmOpen.LoadLogic(EditGame.Logics[ResNum], Quiet)) {
                frmOpen.Show();
                LogicEditors.Add(frmOpen);
            }
            else {
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
            frmLogicEdit frmOpen = new(LogicFormMode.Logic);
            if (frmOpen.LoadLogic(loadlogic)) {
                frmOpen.Show();
                LogicEditors.Add(frmOpen);
            }
            else {
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenGamePicture() {
            Debug.Assert(EditGame is not null);

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
            if (frmOpen.LoadPicture(EditGame.Pictures[ResNum], Quiet)) {
                frmOpen.Show();
                frmOpen.Refresh();
                frmOpen.ForceRefresh();
                PictureEditors.Add(frmOpen);
            }
            else {
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
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenGameSound() {
            Debug.Assert(EditGame is not null);

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
            if (frmOpen.LoadSound(EditGame.Sounds[ResNum], Quiet)) {
                frmOpen.Show();
                SoundEditors.Add(frmOpen);
            }
            else {
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
            SoundImportFormat format = SoundImport.GetSoundImportFormat(filename);
            SoundImportOptions options;
            switch (format) {
            case SoundImportFormat.IT:
            case SoundImportFormat.MOD:
            case SoundImportFormat.MIDI:
                // need to get import options
                var frm = new frmImportSoundOptions(format);
                if (frm.ShowDialog(MDIMain) == DialogResult.OK) {
                    options = frm.Options;
                    frm.Dispose();
                }
                else {
                    frm.Close();
                    frm.Dispose();
                    return;
                }
                break;
            default:
                // no options needed
                options = null;
                break;
            }
            MDIMain.UseWaitCursor = true;
            Sound loadsound = new();
            loadsound.Import(filename, format, options);
            frmSoundEdit frmOpen = new();
            if (frmOpen.LoadSound(loadsound)) {
                frmOpen.Show();
                SoundEditors.Add(frmOpen);
            }
            else {
                frmOpen.Close();
                frmOpen.Dispose();
            }
            // restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenGameView() {
            Debug.Assert(EditGame is not null);

            frmGetResourceNum getresnum = new(GetRes.Open, AGIResType.View);
            if (getresnum.ShowDialog() == DialogResult.OK) {
                OpenGameView(getresnum.NewResNum, false);
            }
            getresnum.Dispose();
        }

        public static void OpenGameView(byte ResNum, bool Quiet = false, int StartLoop = 0, int StartCel = 0) {
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
            if (frmOpen.LoadView(EditGame.Views[ResNum], StartLoop, StartCel, Quiet)) {
                frmOpen.Show();
                ViewEditors.Add(frmOpen);
            }
            else {
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
                ObjectEditor.Close();
                ObjectEditor.Dispose();
            }
        }

        public static void OpenOBJECT(string filename) {
            InventoryList invlist;

            invlist = new(filename);
            frmObjectEdit frm = new();
            if (frm.LoadOBJECT(invlist)) {
                frm.Show();
            }
            else {
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
                WordEditor.Close();
                WordEditor.Dispose();
            }
        }

        public static void OpenWORDSTOK(string filename) {
            WordList wordlist;

            wordlist = new(filename);
            frmWordsEdit frm = new();
            if (frm.LoadWords(wordlist)) {
                frm.Show();
            }
            else {
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
                UpdateExitInfo(UpdateReason.HideRoom, LogicNum, null);
            }
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.Logic];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["l" + LogicNum]) {
                    // deselect the resource beforev removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["l" + LogicNum].Remove();
                break;
            case ResListType.ComboList:
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
                        frm.Close();
                        frm.Dispose();
                        break;
                    }
                }
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
            // removes a picture from the game, and updates
            // preview and resource windows
            // and deletes resource file from source directory
            if (!EditGame.Pictures.Contains(PicNum)) {
                // error?
                MessageBox.Show(MDIMain,
                    $"Picture {PicNum} passed to RemovePicture does not exist.",
                    "RemovePicture Error");
                return;
            }

            string strPicFile = EditGame.ResDir + EditGame.Pictures[PicNum].ID + ".agp";
            // remove it from game
            EditGame.Pictures.Remove(PicNum);

            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.Picture];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["p" + PicNum]) {
                    // deselect the resource beforev removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["p" + PicNum].Remove();
                break;
            case ResListType.ComboList:
                // only need to remove if pictures are listed
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
                    // set number to -1 to force close
                    frm.PictureNumber = -1;
                    frm.Close();
                    frm.Dispose();
                    break;
                }
            }

            // update the logic tooltip lookup table
            IDefLookup[(int)AGIResType.Picture, PicNum].Name = "";
            IDefLookup[(int)AGIResType.Picture, PicNum].Type = ArgType.None;
            // then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListChanged = true;
            }
            if (LEInUse) {
                LayoutEditor.UpdatePictureStatus(PicNum, false);
            }
        }

        public static void RemoveSound(byte SoundNum) {
            // removes a sound from the game, and updates
            // preview and resource windows
            // and deletes resource file from source directory
            if (!EditGame.Sounds.Contains(SoundNum)) {
                // error?
                MessageBox.Show(MDIMain,
                    $"Sound {SoundNum} passed to RemoveSound does not exist.",
                    "RemoveSound Error");
                return;
            }

            string strSoundFile = EditGame.ResDir + EditGame.Sounds[SoundNum].ID + ".ags";
            // remove it from game
            EditGame.Sounds.Remove(SoundNum);

            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.Sound];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["s" + SoundNum]) {
                    // deselect the resource before removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["s" + SoundNum].Remove();
                break;
            case ResListType.ComboList:
                // only need to remove if sounds are listed
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
                    // set number to -1 to force close
                    frm.SoundNumber = -1;
                    frm.Close();
                    frm.Dispose();
                    break;
                }
            }

            // update the logic tooltip lookup table
            IDefLookup[(int)AGIResType.Sound, SoundNum].Name = "";
            IDefLookup[(int)AGIResType.Sound, SoundNum].Type = ArgType.None;
            // then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListChanged = true;
            }
        }

        public static void RemoveView(byte ViewNum) {
            // removes a sound from the game, and updates
            // preview and resource windows
            // and deletes resource file from source directory
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
            case ResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.View];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["v" + ViewNum]) {
                    // deselect the resource before removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["v" + ViewNum].Remove();
                break;
            case ResListType.ComboList:
                // only need to remove if views are listed
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
                    // set number to -1 to force close
                    frm.ViewNumber = -1;
                    frm.Close();
                    frm.Dispose();
                    break;
                }
            }

            // update the logic tooltip lookup table
            IDefLookup[(int)AGIResType.View, ViewNum].Name = "";
            IDefLookup[(int)AGIResType.View, ViewNum].Type = ArgType.None;
            // then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListChanged = true;
            }
        }

        public static void MakeAllChanged() {
            EditGame.Logics.MarkAllAsChanged();
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                foreach (TreeNode tmpNode in HdrNode[0].Nodes) {
                    if (File.Exists(EditGame.ResDir + EditGame.Logics[(byte)tmpNode.Tag].ID + "." + EditGame.SourceExt)) {
                        tmpNode.ForeColor = Color.Red;
                    }
                }
                break;
            case ResListType.ComboList:
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
            // first, get the default file, if there is one
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
            if (blnNoFile) {
                // use the default template
                strLogic = LoadResString(101);
                // apply code style
                //       if (test())F0{
                //           action();
                //       F1}F2elseF3{
                //       F1}
                //              F0      F1      F2      F3
                //  default:    \r\n~   ~       \r\n    \r\n~
                //  alt1:       " "     ""      \r\n    " "    
                //  alt2:       " "     ""      " "     " "
                // (template doesn't use 'else')
                switch (WinAGISettings.CodeStyle.Value) {
                case LogicDecoder.AGICodeStyle.cstDefaultStyle:
                    strLogic = strLogic.Replace("F0", Environment.NewLine + "~");
                    strLogic = strLogic.Replace("F1", "~");
                    break;
                case LogicDecoder.AGICodeStyle.cstAltStyle1:
                    strLogic = strLogic.Replace("F0", " ");
                    strLogic = strLogic.Replace("F1", "");
                    break;
                case LogicDecoder.AGICodeStyle.cstAltStyle2:
                    strLogic = strLogic.Replace("F0", " ");
                    strLogic = strLogic.Replace("F1", "");
                    break;
                }
                // insert line breaks
                strLogic = strLogic.Replace("|", Environment.NewLine);
            }

            try {
                // substitute correct values for the various place holders:
                // add the tabs
                strLogic = strLogic.Replace("~", "".PadRight(WinAGISettings.LogicTabWidth.Value));
                // id:
                strLogic = strLogic.Replace("%id", NewID);
                // description
                strLogic = strLogic.Replace("%desc", NewDescription);
                // horizon
                strLogic = strLogic.Replace("%h", PicEditTestSettings.Horizon.Value.ToString());

                // if using reserved names, insert them
                if (EditGame.IncludeReserved) {
                    // f5, v0, f2, f4, v9
                    strLogic = strLogic.Replace("f5", EditGame.ReservedDefines.ReservedFlags[5].Name);
                    strLogic = strLogic.Replace("f2", EditGame.ReservedDefines.ReservedFlags[2].Name);
                    strLogic = strLogic.Replace("f4", EditGame.ReservedDefines.ReservedFlags[4].Name);
                    strLogic = strLogic.Replace("v0", EditGame.ReservedDefines.ReservedVariables[0].Name);
                    strLogic = strLogic.Replace("v9", EditGame.ReservedDefines.ReservedVariables[9].Name);
                }
            }
            catch (Exception) {
                // ignore errors return whatever is left
            }
            // return the formatted text
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
            bool blnFrmVis;
            int lngCheck = 0;
            int lngPossFind;
            bool blnSkipEd;
            WinAGIFCTB searchFCTB = null;

            MDIMain.spStatus.Text = "";

            StringComparison strComp = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (Replacing && FindText.Equals(ReplaceText, strComp)) {
                return;
            }
            MDIMain.UseWaitCursor = true;
            switch (startform.Name) {
            case "frmLogicEdit":
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
            case "frmMDIMain":
            case "frmObjectEdit":
            case "frmWordsEdit":
            case "frmGlobals":
                // no distinction (yet) between words, objects, resIDs, globals
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
                // always start non-logic searches at beginning
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
                // if all logics, skip any text editors or non ingame logics
                if (LogicLoc == FindLocation.All) {
                    if (LogicEditors[nextLogicIndex].FormMode == LogicFormMode.Text || !LogicEditors[nextLogicIndex].InGame) {
                        // skip it
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
                        // search strategy depends on synonym search value
                        if (!GFindSynonym) {
                            if (MatchWord) {
                                FoundPos = FindWholeWord(SearchPos, searchFCTB.Text, FindText, MatchCase, FindDir == FindDirection.Up, SearchType);
                            }
                            else {
                                FoundPos = searchFCTB.Text.IndexOf(FindText, SearchPos, strComp);
                            }
                        }
                        else {
                            // in synonym search, Matchword is always true; but since words are
                            // surrounded by quotes, it wont matter so we use IndexOf
                            // step through each word in the word group; if the word is found in this logic,
                            // check if it occurs before the current found position
                            FoundPos = -1;
                            for (int i = 0; i < WordEditor.EditWordList.GroupByNumber(GFindGrpNum).WordCount; i++) {
                                lngPossFind = searchFCTB.Text.IndexOf(QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum)[i] + QUOTECHAR, SearchPos);
                                // validate it's a word arg
                                if (lngPossFind > 0) {
                                    if (IsVocabWord(lngPossFind, searchFCTB.Text)) {
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
                        }
                        // always reset SearchPos
                        SearchPos = 0;
                    }
                }
                if (FoundPos >= 0) {
                    if (FindDir == FindDirection.All || FindDir == FindDirection.Down) {
                        // if back at search start (whether anything found or not)
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
                        // searching up
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
                            // exit loop; search text found
                            break;
                        }
                    }
                }
                // if not found, action depends on search mode
                if (LogicLoc == FindLocation.Current) {
                    if (FindDir == FindDirection.Up) {
                        if (!RestartSearch) {
                            DialogResult rtn = MessageBox.Show(MDIMain,
                                "Beginning of search scope reached. Do you want to continue from the end?",
                                "Find in Logic",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                            if (rtn == DialogResult.No) {
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
                        // if restartsearch is true, means this is second time through;
                        // since nothing found, just exit the loop
                        if (RestartSearch) {
                            // not found; exit
                            break;
                        }
                    }
                }
                else if (LogicLoc == FindLocation.Open) {
                    // if back on start, and search already reset
                    if ((nextLogicIndex == SearchStartLog) && RestartSearch) {
                        // not found- exit
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
                    // since nothing found in this logic, try the next
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
                // if search was started from the editor (by pressing F3 or using
                // menu option)
                if (!SearchStartDlg) {
                    // set focus to the editor
                    LogicEditors[nextLogicIndex].Select();
                    LogicEditors[nextLogicIndex].fctb.Select();
                }
                else {
                    // when searching from the dialog, make sure the logic is
                    // at top of zorder, but don't need to give it focus
                    LogicEditors[nextLogicIndex].BringToFront();
                }
                // highlight searchtext
                Place start = searchFCTB.PositionToPlace(FoundPos);
                Place end = new(start.iChar + FindText.Length, start.iLine);
                searchFCTB.Selection.Start = start;
                searchFCTB.Selection.End = end;
                searchFCTB.DoSelectionVisible();
                searchFCTB.Refresh();
                // if a synonym was found, note it on status bar
                if (GFindSynonym) {
                    if (FindText != QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum).GroupName + QUOTECHAR) {
                        MDIMain.spStatus.Text = FindText + " is a synonym for " + QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum).GroupName + QUOTECHAR;
                        // TODO: flash the status bar
                        //MDIMain.tmrFlash.Enabled = true;
                    }
                }
            }
            else {
                // search string was NOT found (or couldn't open a logic editor window)
                if (FoundOnce) {
                    // search complete; no new instances found
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
                if (!SearchStartDlg) {
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
            int LogNum = -1; 

            if (!ClosedLogics) {
                // first time through - start with first logic (which sets ClosedLogics flag)
                LogNum = NextClosedLogic(-1);
                if (LogNum != -1) {
                    ProgressWin = new (MDIMain) {
                        Text = "Find in Logic"
                    };
                    ProgressWin.lblProgress.Text = "Searching " + EditGame.Logics[LogNum].ID + "...";
                    ProgressWin.pgbStatus.Maximum = EditGame.Logics.Count + 1;
                    ProgressWin.pgbStatus.Value = LogicEditors.Count;
                    ProgressWin.Show();
                    ProgressWin.Refresh();
                }
                else {
                    // no other logics to search
                    return -1;
                }
            }
            else {
                ProgressWin.Show();
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

        public static void ReplaceAll(Form startform, string FindText, string ReplaceText, 
            FindDirection FindDir, bool MatchWord, bool MatchCase, FindLocation LogicLoc, 
            AGIResType SearchType = AGIResType.None) {
            //  replace all doesn't use or need direction

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
            ReplaceCount = 0;

            ProgressWin = new(startform);
            MDIMain.UseWaitCursor = true;

            switch (LogicLoc) {
            case FindLocation.Current:
                if (SearchType == AGIResType.Words && GFindSynonym) {
                    // replace all synonyms
                    for (int i = 0; i < WordEditor.EditWordList.GroupByNumber(GFindGrpNum).WordCount; i++) {
                        ReplaceAllText(QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum)[i] + QUOTECHAR,
                            ReplaceText, true, MatchCase, SearchType, (frmLogicEdit)startform);
                    }
                }
                else {
                    ReplaceAllText(FindText, ReplaceText, MatchWord, MatchCase, SearchType, (frmLogicEdit)startform);
                }
                break;
            case FindLocation.Open:
                // replace in all open logic and text editors
                ProgressWin.Text = "Replace All";
                ProgressWin.lblProgress.Text = "Searching...";
                ProgressWin.pgbStatus.Maximum = LogicEditors.Count;
                ProgressWin.pgbStatus.Value = 0;
                ProgressWin.Show();
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
                    if (SearchType == AGIResType.Words && GFindSynonym) {
                        // replace all synonyms
                        for (int j = 0; j < WordEditor.EditWordList.GroupByNumber(GFindGrpNum).WordCount; j++) {
                            ReplaceAllText(QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum)[j] + QUOTECHAR,
                                ReplaceText, true, MatchCase, SearchType, LogicEditors[i]);
                        }
                    }
                    else {
                        ReplaceAllText(FindText, ReplaceText, MatchWord, MatchCase, SearchType, LogicEditors[i]);
                    }
                }
                ProgressWin.Hide();
                break;
            case FindLocation.All:
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
                ProgressWin.Show();
                ProgressWin.Refresh();

                // first replace in all open editors
                for (int i = 0; i < LogicEditors.Count; i++) {
                    // only logic editors, and ingame
                    if (LogicEditors[i].FormMode == LogicFormMode.Logic && LogicEditors[i].InGame) {
                        ProgressWin.pgbStatus.Value++;
                        ProgressWin.lblProgress.Text = "Searching " + LogicEditors[i].EditLogic.ID + "...";
                        ProgressWin.Refresh();
                        if (SearchType == AGIResType.Words && GFindSynonym) {
                            // replace all synonyms
                            for (int j = 0; j < WordEditor.EditWordList.GroupByNumber(GFindGrpNum).WordCount; j++) {
                                ReplaceAllText(QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum)[j] + QUOTECHAR,
                                    ReplaceText, true, MatchCase, SearchType, LogicEditors[i]);
                            }
                        }
                        else {
                            ReplaceAllText(FindText, ReplaceText, MatchWord, MatchCase, SearchType, LogicEditors[i]);
                        }
                    }
                }
                // then do all logics
                foreach (Logic logic in EditGame.Logics) {
                    ProgressWin.pgbStatus.Value++;
                    ProgressWin.lblProgress.Text = "Searching " + logic.ID + "...";
                    ProgressWin.Refresh();
                    bool loaded = logic.Loaded;
                    if (!loaded) {
                        logic.Load();
                    }
                    if (SearchType == AGIResType.Words && GFindSynonym) {
                        // replace all synonyms
                        for (int i = 0; i < WordEditor.EditWordList.GroupByNumber(GFindGrpNum).WordCount; i++) {
                            ReplaceAllText(QUOTECHAR + WordEditor.EditWordList.GroupByNumber(GFindGrpNum)[i] + QUOTECHAR,
                                ReplaceText, true, MatchCase, SearchType, logic);
                        }
                    }
                    else {
                        ReplaceAllText(FindText, ReplaceText, MatchWord, MatchCase, SearchType, logic);
                    }
                    if (logic.SourceChanged) {
                        logic.SaveSource();
                        // update exits if this room is marked as a room and not making
                        // a resource ID change
                        if (logic.IsRoom && !ChangingID) {
                            UpdateExitInfo(UpdateReason.ChangeID, logic.Number, logic);
                        }
                        // refresh preview and tree as applicable
                        RefreshTree(AGIResType.Logic, logic.Number);
                        if (MDIMain.propertyGrid1.Visible) {
                            MDIMain.propertyGrid1.Refresh();
                        }
                    }
                    if (!loaded) {
                        logic.Unload();
                    }
                }
                ProgressWin.Hide();
                break;
            }
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
            ProgressWin.Close();
            ProgressWin.Dispose();
            FindingForm.ResetSearch();
        }

        private static void ReplaceAllText(string FindText, string ReplaceText, bool MatchWord, bool MatchCase, AGIResType SearchType, frmLogicEdit SearchWin) {
            // replaces text in a logic editor

            if (SearchType != AGIResType.None) {
                // ignore text editors
                if (SearchWin.FormMode == LogicFormMode.Text) {
                    return;
                }
            }
            if (MatchWord) {
                string pattern = FindText;
                // if not surrounded by quotes, add word boundaries
                if (FindText.Length > 0 && FindText[0] != '\"') {
                    pattern = @"\b" + pattern;
                }
                if (FindText.Length > 0 && FindText[^1] != '\"') {
                    pattern += @"\b";
                }
                MatchCollection matches = Regex.Matches(SearchWin.fctb.Text, pattern, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
                for (int i = matches.Count - 1; i >= 0; i--) {
                    switch (SearchType) {
                    case Words:
                        // validate it's a word arg
                        if (!IsVocabWord(matches[i].Index, SearchWin.fctb.Text)) {
                            continue;
                        }
                        break;
                    case Objects:
                        // validate it's an object arg
                        if (!IsInvObject(matches[i].Index, SearchWin.fctb.Text)) {
                            continue;
                        }
                        break;
                    default:
                        break;
                    }
                    SearchWin.fctb.ReplaceText(matches[i].Index, matches[i].Length, ReplaceText);
                    ReplaceCount++;
                }
            }
            else {
                // if matchword is false, searchtype will always be default,
                // so no need to check for words/objects replacements
                SearchWin.fctb.Text = Regex.Replace(SearchWin.fctb.Text, FindText, x => {
                    ReplaceCount++;
                    return ReplaceText;
                }, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
            }
        }

        private static void ReplaceAllText(string FindText, string ReplaceText, bool MatchWord, bool MatchCase, AGIResType SearchType, Logic SearchLogic) {
            // replaces text in a logic source file

            if (MatchWord) {
                string pattern = FindText;
                // if not surrounded by quotes, add word boundaries
                if (FindText.Length > 0 && FindText[0] != '\"') {
                    pattern = @"\b" + pattern;
                }
                if (FindText.Length > 0 && FindText[^1] != '\"') {
                    pattern += @"\b";
                }
                MatchCollection matches = Regex.Matches(SearchLogic.SourceText, pattern, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
                for (int i = matches.Count - 1; i >= 0; i--) {
                    switch (SearchType) {
                    case Words:
                        // validate it's a word arg
                        if (!IsVocabWord(matches[i].Index, SearchLogic.SourceText)) {
                            continue;
                        }
                        break;
                    case Objects:
                        // validate it's an object arg
                        if (!IsInvObject(matches[i].Index, SearchLogic.SourceText)) {
                            continue;
                        }
                        break;
                    default:
                        break;
                    }
                    SearchLogic.SourceText = SearchLogic.SourceText.ReplaceFirst(FindText, ReplaceText, matches[i].Index);
                    ReplaceCount++;
                }

            }
            else {
                // if matchword is false, searchtype will always be default,
                // so no need to check for words/objects replacements
                SearchLogic.SourceText = Regex.Replace(SearchLogic.SourceText, FindText, x => {
                    ReplaceCount++;
                    return ReplaceText;
                }, MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
            }
        }

        public static string InstrumentName(int instrument) {
            // returns the name of an instrument as a string
            return LoadResString(INSTRUMENTNAMETEXT + instrument);
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
                        // assign proposed rename
                        strOldName = strName + "_OLD" + strExt;
                        // validate it
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
                // ignore
            }
        }

        /// <summary>
        /// Displays detailed error information in a MessageBox.
        /// </summary>
        /// <param name="ErrNum"></param>
        /// <param name="ErrMsg1"></param>
        /// <param name="ErrMsg2"></param>
        /// <param name="ErrCaption"></param>
        public static void ErrMsgBox(int ErrNum, string ErrMsg1, string ErrMsg2, string ErrCaption) {
            // show errmsg baed on agi resource error level
            string strErrMsg;

            strErrMsg = ErrMsg1 + Environment.NewLine + Environment.NewLine + ErrNum + ": " + LoadResString(ErrNum);
            if (ErrMsg2.Length > 0) {
                strErrMsg = strErrMsg + Environment.NewLine + Environment.NewLine + ErrMsg2;
            }
            MessageBox.Show(MDIMain,
                strErrMsg,
                ErrCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        /// <summary>
        /// Displays detailed error information in a MessageBox.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="ErrMsg1"></param>
        /// <param name="ErrMsg2"></param>
        /// <param name="ErrCaption"></param>
        public static void ErrMsgBox(Exception e, string ErrMsg1, string ErrMsg2, string ErrCaption) {
            // displays a messagebox showing ErrMsg and includes error passed as AGIErrObj
            int lngErrNum;
            string strErrMsg;

            // determine if ErrNum is an AGI number:
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
            MessageBox.Show(MDIMain,
                strErrMsg,
                ErrCaption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        public static void GetDefaultColors() {
            // reads default custom colors from winagi.confg
            for (int i = 0; i < 16; i++) {
                DefaultPalette[i] = WinAGISettingsFile.GetSetting(sDEFCOLORS, "DefEGAColor" + i, DefaultPalette[i]);
            }

        }

        public static string ResourceName(AGIResource ThisResource, bool IsInGame, bool NoNumber = false) {
            // formats resource name based on user preference
            // format includes: option for upper, lower or title case of Type;
            //                 space or period for separator;
            //                 forcing number to include leading zeros

            // if the resource is not part of a game,
            // the ID is returned regardless of ID/number setting
            string retval = "";
            // if using numbers AND resource is ingame,
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

        public static void DrawTransGrid(Control surface, int offsetX, int offsetY) {
            surface.BackgroundImage = new Bitmap(surface.Width, surface.Height);
            using Graphics gs = Graphics.FromImage(surface.BackgroundImage);

            for (int i = 0; i <= gs.VisibleClipBounds.Width + 1; i += 10) {
                for (int j = 0; j < gs.VisibleClipBounds.Height + 1; j += 10) {
                    gs.FillRectangle(Brushes.Black, new Rectangle(i + offsetX, j + offsetY, 1, 1));
                }
            }
            surface.Refresh();
        }

        /// <summary>
        /// Draws the agi bitmap in target picture box, using scale factor provided
        /// </summary>
        /// <param name="pic"></param>
        /// <param name="agiBMP"></param>
        /// <param name="scale"></param>
        /// <param name="mode"></param>
        public static void ShowAGIBitmap(PictureBox pic, Bitmap agiBMP, double scale = 1, InterpolationMode mode = InterpolationMode.NearestNeighbor) {
            // pictures and views with errors will pass null value
            if (agiBMP is null) {
                // clear the pic
                pic.CreateGraphics().Clear(pic.BackColor);
                return;
            }
            int bWidth = (int)(agiBMP.Width * scale * 2), bHeight = (int)(agiBMP.Height * scale);
            // first, create new image in the picture box that is desired size
            pic.Image = new Bitmap(bWidth, bHeight);
            // intialize a graphics object for the image just created
            using Graphics g = Graphics.FromImage(pic.Image);
            // always clear the background first
            g.Clear(pic.BackColor);
            // set correct interpolation mode
            // draw the bitmap, at correct resolution
            g.InterpolationMode = mode;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(agiBMP, 0, 0, bWidth, bHeight);
            _ = SendMessage(pic.Handle, WM_SETREDRAW, true, 0);
        }

        static Bitmap ResizeAGIBitmap(Bitmap agiBmp, int scale = 1, InterpolationMode mode = InterpolationMode.NearestNeighbor) {
            // resizes a bitmap using APIs so it can be exported
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
                return EditorResources.ResourceManager.GetString(index.ToString());
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
                // if doing a reverse search
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
                            // let loop try again at next position in string
                            break;
                        default:
                            // is validation required
                            switch (SearchType) {
                            case Words:
                                // validate vocab word
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
            // check for 'has' cmd
            // check for 'obj.in.room' cmd
            // check for 'drop' cmd
            // check for 'get' cmd
            // check for 'put' cmd

            int argcount = 0;
            AGIToken cmd = FindPrevCmd(strText, WinAGIFCTB.TokenFromPos(strText, lngStartPos), ref argcount);
            if (cmd.Type != AGITokenType.Identifier) {
                return false;
            }
            switch (cmd.SubType) {
            case TokenSubtype.ActionCmd:
                switch (cmd.Number) {
                case 94: // drop
                case 92: // get
                case 95: // put
                    return true;
                }
                break;
            case TokenSubtype.TestCmd:
                switch (cmd.Number) {
                case 9: // has
                case 10: // obj.in.room
                    return true;
                }
                break;
            }
            return false;
        }

        internal static bool IsVocabWord(int lngStartPos, string strText) {
            int argcount = 0;
            AGIToken cmd = FindPrevCmd(strText, WinAGIFCTB.TokenFromPos(strText, lngStartPos), ref argcount);
            return cmd.Type == AGITokenType.Identifier && cmd.SubType == TokenSubtype.TestCmd && cmd.Number == 14;
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

        public static AGIToken FindPrevCmd(string sourcetext, AGIToken starttoken, ref int argCount) {
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
            int lngArgCount = 0;

            AGIToken token = WinAGIFCTB.PreviousToken(sourcetext, starttoken);
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
                token = WinAGIFCTB.PreviousToken(sourcetext, token);
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

        public static List<string> WordWrapLines(string text, Font font, float maxWidth, Graphics g, char[] breakChars = null) {
            breakChars ??= new char[] { ' ', '\t', '-', '/', ',' };
            List<string> lines = new();
            string[] paragraphs = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            foreach (string paragraph in paragraphs) {
                string remaining = paragraph;
                do {
                    int len = remaining.Length;
                    // Find the maximum substring that fits
                    while (len > 0) {
                        string substr = remaining[..len];
                        SizeF size = g.MeasureString(substr, font);
                        if (size.Width <= maxWidth)
                            break;
                        len--;
                    }
                    if (len == 0 && remaining.Length > 0) len = 1; // fallback to at least one char

                    // Try to break at last space
                    int breakAt = -1;
                    for (int i = len - 1; i >= 0; i--) {
                        if (breakChars.Contains(remaining[i])) {
                            breakAt = i;
                            break;
                        }
                    }
                    if (breakAt > 0 && len != remaining.Length)
                        len = breakAt + 1;
                    string line = remaining[..len].TrimEnd();
                    lines.Add(line);
                    remaining = remaining[len..].TrimStart();
                } while (!string.IsNullOrEmpty(remaining));
            }
            return lines;
        }

        public static List<string> GetWrappedLines(TextBox textBox) {
            const int EM_GETLINECOUNT = 0x00BA;
            int lineCount = SendMessage(textBox.Handle, EM_GETLINECOUNT, 0, 0);

            var lines = new List<string>();
            for (int i = 0; i < lineCount; i++) {
                int start = textBox.GetFirstCharIndexFromLine(i);
                int end = (i + 1 < lineCount)
                    ? textBox.GetFirstCharIndexFromLine(i + 1)
                    : textBox.TextLength;
                lines.Add(textBox.Text.Substring(start, end - start));
            }
            return lines;
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
            ProgressWin = new(MDIMain) {
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
                    // TODO: need to add back sourceerror property
                    // TODO: should readonly resources be exportable?
                    if (logic.Error == ResourceErrorType.NoError) {
                        logic.ExportSource(exportdir + logic.ID + "." + EditGame.SourceExt);
                    }
                }
                // compiled logic
                if (logic.Error == ResourceErrorType.NoError) {
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
                if (tmpPic.Error == ResourceErrorType.NoError) {
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
                if (tmpSnd.Error == ResourceErrorType.NoError) {
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
                if (tmpView.Error == ResourceErrorType.NoError) {
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
                    ErrMsgBox(ex,
                        "An error occurred while exporting this file: ",
                        ex.StackTrace,
                        "Export File Error");
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
                if (logic.Error == ResourceErrorType.NoError ||
                    logic.Error == ResourceErrorType.FileIsReadonly) {
                    try {
                        logic.Export(filename);
                    }
                    catch (Exception ex) {
                        ErrMsgBox(ex,
                            "An error occurred while exporting this logic: ",
                            ex.StackTrace,
                            "Export Logic Error");
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
                    ProgressWin = new (MDIMain) {
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
                        ErrMsgBox(ex,
                            "An error occurred while exporting this picture:",
                            ex.StackTrace,
                            "Export Sound Error");
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
                    ErrMsgBox(ex,
                        "An error occurred while exporting this sound:",
                        ex.StackTrace,
                        "Export Sound Error");
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
                    ErrMsgBox(ex,
                        "An error occurred while exporting this view:",
                        ex.StackTrace,
                        "Export View Error");
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
                ErrMsgBox(ex,
                    "An error occurred while exporting this OBJECT file:",
                    ex.StackTrace,
                    "Export OBJECT Error");
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
                ErrMsgBox(ex,
                    "An error occurred while exporting this WORDS.TOK file:",
                    ex.StackTrace,
                    "Export WORDS.TOK Error");
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
            // exports all picture images as one format in src dir
            int lngZoom, lngMode, lngFormat;
            string strExt = "";
            bool blnLoaded;
            // show options form, force image only
            frmExportPictureOptions frmPEO = new(1) {
                Text = "Export All Picture Images"
            };
            ;
            if (frmPEO.ShowDialog(MDIMain) == DialogResult.Cancel) {
                // nothing to do
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
            // done with the options form
            frmPEO.Close();
            frmPEO.Dispose();
            // show wait cursor
            MDIMain.UseWaitCursor = true;
            // need to get correct file extension
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
            // if not canceled, export them all
            // setup ProgressWin form
            ProgressWin = new (MDIMain) {
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
                // skip if errors (readonly is OK)
                if (ThisPic.Error == ResourceErrorType.NoError ||
                    ThisPic.Error == ResourceErrorType.FileIsReadonly) {
                    ExportImg(ThisPic, EditGame.ResDir + ThisPic.ID + strExt, lngFormat, lngMode, lngZoom);
                }
                if (!blnLoaded) {
                    ThisPic.Unload();
                }
            }
            // done with ProgressWin form
            ProgressWin.Close();
            ProgressWin.Dispose();
            MDIMain.UseWaitCursor = false;
        }

        public static void ExportImg(Picture ExportPic, string ExportFile, int ImgFormat, int ImgMode, int ImgZoom) {
            // exports pic gdpImg
            Bitmap ExportBMP;
            int Count = 0;

            // mode:  0 = vis
            //        1 = pri
            //        2 = both
            do {
                // if second time through, adjust output filename
                if (Count == 1) {
                    // get name for pri gdpImg
                    ExportFile = ExportFile.Left(ExportFile.Length - 4) + "_P" + ExportFile.Right(4);
                }
                // if 1st time through AND mode is 0 or 2: vis
                // if second time through OR mode is 1: pri
                if (Count == 0 && ImgMode != 1) {
                    // save vis as temporary BMP
                    ExportBMP = ResizeAGIBitmap(ExportPic.VisualBMP, ImgZoom);
                }
                else {
                    // save vis as temporary BMP
                    ExportBMP = ResizeAGIBitmap(ExportPic.PriorityBMP, ImgZoom);
                }
                // make sure existing file is deleted
                SafeFileDelete(ExportFile);
                // save based on format choice
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
            // exports a picture vis screen and/or pri screen as either bmp or gif, or png
            int lngZoom, lngMode, lngFormat;
            // show options form, save image only
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
            // done with the options form
            frmPEO.Close();
            frmPEO.Dispose();
            // show save file dialog
            MDIMain.SaveDlg.Title = "Save Picture Image As";
            MDIMain.SaveDlg.DefaultExt = "bmp";
            MDIMain.SaveDlg.Filter = "BMP files (*.bmp)|*.bmp|JPEG files (*.jpg)|*.jpg|GIF files (*.gif)|*.gif|TIFF files (*.tif)|*.tif|PNG files (*.PNG)|*.png|All files (*.*)|*.*";
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            MDIMain.SaveDlg.FilterIndex = lngFormat;
            MDIMain.SaveDlg.FileName = "";
            DialogResult rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);
            // if NOT canceled, then export
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
                DialogResult = DialogResult.OK,
                Name = "okButton",
                Size = new System.Drawing.Size(75, 23),
                Text = "&OK",
                Location = new System.Drawing.Point(inputBox.ClientSize.Width - 160, 39 + offset)
            };
            inputBox.Controls.Add(okButton);

            Button cancelButton = new() {
                DialogResult = DialogResult.Cancel,
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
        #endregion

        #region Editor Extension Methods
        public static string CommandName(this DrawFunction drawfunction) {
            return drawfunction switch {
                DrawFunction.EnableVis => "Vis: ON",    // Change picture color and enable picture draw.
                DrawFunction.DisableVis => "Vis: OFF",   // Disable picture draw.
                DrawFunction.EnablePri => "Pri: ON",    // Change priority color and enable priority draw.
                DrawFunction.DisablePri => "Pri: OFF",   // Disable priority draw.
                DrawFunction.YCorner => "Y Corner",      // Draw a Y corner.
                DrawFunction.XCorner => "X Corner",      // Draw an X corner.
                DrawFunction.AbsLine => "Line",      // Absolute line (long lines).
                DrawFunction.RelLine => "Short Line",      // Relative line (short lines).
                DrawFunction.Fill => "Fill",         // Fill.
                DrawFunction.ChangePen => "Set Plot Pen",    // Change pen size and style.
                DrawFunction.PlotPen => "Plot",      // Plot with pen.
                DrawFunction.End => "End",           // end of drawing
                _ => "undefined"
            };
        }

        /// <summary>
        /// Colorizes a black and white bitmap with the specified foreground and
        /// background colors. Black pixels will be converted to foreground,
        /// white pixels will be converted to background. Don't use this on a
        /// bitmap with colors other than black and white.
        /// </summary>
        /// <param name="originalBitmap"></param>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        /// <returns>A new bitmap with the desired foreground and background colors.</returns>
        public static Bitmap Colorize(this Bitmap originalBitmap, Color foreground, Color background) {

            Bitmap newBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);

            // Lock the bits of the original bitmap
            BitmapData originalData = originalBitmap.LockBits(new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height), ImageLockMode.ReadOnly, originalBitmap.PixelFormat);
            BitmapData newData = newBitmap.LockBits(new Rectangle(0, 0, newBitmap.Width, newBitmap.Height), ImageLockMode.WriteOnly, newBitmap.PixelFormat);

            unsafe {
                byte* originalPtr = (byte*)originalData.Scan0;
                byte* newPtr = (byte*)newData.Scan0;

                int bytesPerPixel = Image.GetPixelFormatSize(originalBitmap.PixelFormat) / 8;
                for (int y = 0; y < originalBitmap.Height; y++) {
                    for (int x = 0; x < originalBitmap.Width; x++) {
                        int pixelIndex = (y * originalData.Stride) + (x * bytesPerPixel);
                        Color pixelColor = Color.FromArgb(originalPtr[pixelIndex + 3], originalPtr[pixelIndex + 2], originalPtr[pixelIndex + 1], originalPtr[pixelIndex]);

                        // if it's black (foreground)
                        if (pixelColor == Color.Black) {
                            newPtr[pixelIndex] = foreground.B;
                            newPtr[pixelIndex + 1] = foreground.G;
                            newPtr[pixelIndex + 2] = foreground.R;
                            newPtr[pixelIndex + 3] = foreground.A;
                        }
                        else {
                            // assume it's white (background)
                            newPtr[pixelIndex] = background.B;
                            newPtr[pixelIndex + 1] = background.G;
                            newPtr[pixelIndex + 2] = background.R;
                            newPtr[pixelIndex + 3] = background.A;
                        }
                    }
                }
            }

            // Unlock the bits
            originalBitmap.UnlockBits(originalData);
            newBitmap.UnlockBits(newData);

            return newBitmap;
        }
        #endregion
    }
}
