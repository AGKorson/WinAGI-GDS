using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Resources;
using WinAGI.Engine;
using WinAGI.Common;
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Engine.ArgTypeEnum;
using static WinAGI.Common.BkgdTasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net.Http.Headers;

namespace WinAGI.Editor {
    //***************************************************
    // Resource Manager Class
    //
    // contains global constants, structures, enumerations
    // methods, functions, that are used by the user 
    // interface
    //
    //***************************************************

    public static class Base {
        //***************************************************
        // GLOBAL CONSTANTS
        //***************************************************
        #region
        // default setting values - global
        public const bool DEFAULT_SHOWSPLASHSCREEN = true;
        public const bool DEFAULT_WARNCOMPILE = true;
        public const bool DEFAULT_NOTIFYCOMPSUCCESS = true;
        public const bool DEFAULT_NOTIFYCOMPWARN = true;
        public const bool DEFAULT_NOTIFYCOMPFAIL = true;
        public const bool DEFAULT_WARNDUPGNAME = true;
        public const bool DEFAULT_WARNDUPGVAL = true;
        public const bool DEFAULT_WARNSTRVAL = true;
        public const bool DEFAULT_WARNCTLVAL = true;
        public const bool DEFAULT_WARNRESOVRD = true;
        public const bool DEFAULT_WARNDUPOBJ = true;
        public const int DEFAULT_DELBLANKG = 0;
        public const bool DEFAULT_SHOWPREVIEW = true;
        public const bool DEFAULT_SHIFTPREVIEW = true;
        public const bool DEFAULT_HIDEPREVIEW = false; //true
        public const agiSettings.EResListType DEFAULT_RESLISTTYPE = agiSettings.EResListType.TreeList;
        public const bool DEFAULT_AUTOOPEN = true;
        public const bool DEFAULT_OPENNEW = true;
        public const bool DEFAULT_AUTOEXPORT = true;
        public const int DEFAULT_AUTOUPDATEDEFINES = 0;
        public const int DEFAULT_AUTOUPDATERESDEFS = 0;
        public const bool DEFAULT_ASKEXPORT = true;
        public const bool DEFAULT_ASKREMOVE = true;
        ////  public const DEFAULT_AUTOUPDATEOBJECTS = 0
        ////  public const DEFAULT_AUTOUPDATEWORDS = 0
        public const bool DEFAULT_SHOWRESNUM = false;
        public const bool DEFAULT_INCLUDERESNUM = true;
        public const bool DEFAULT_RENAMEDELRES = true;
        public const int DEFAULT_MAXSO = 16;
        public const int DEFAULT_MAXVOL0SIZE = 1047552;
        public const bool DEFAULT_HILITELOG = true;
        public const bool DEFAULT_HILITETEXT = false;
        //default settings - logics
        public const int DEFAULT_LOGICTABWIDTH = 4;
        public const bool DEFAULT_MAXIMIZELOGICS = true;
        public const bool DEFAULT_AUTOQUICKINFO = true;
        public const bool DEFAULT_SHOWDEFTIPS = true;
        public const string DEFAULT_DEFSRCEXT = "lgc";
        public static string DEFAULT_EFONTNAME; //no constants; value will be set in InitializeResMan
        public const int DEFAULT_EFONTSIZE = 14;
        public static string DEFAULT_PFONTNAME; //no constants; value will be set in InitializeResMan
        public const int DEFAULT_PFONTSIZE = 12;
        public static readonly Color DEFAULT_HNRMCOLOR = Color.Black;
        public static readonly Color DEFAULT_HKEYCOLOR = Color.FromArgb(0x7F, 0, 0);
        public static readonly Color DEFAULT_HIDTCOLOR = Color.FromArgb(0, 0, 0x40);
        public static readonly Color DEFAULT_HSTRCOLOR = Color.FromArgb(0, 0x50, 0x50);
        public static readonly Color DEFAULT_HCMTCOLOR = Color.FromArgb(0, 0x80, 0);
        public static readonly Color DEFAULT_HBKGCOLOR = Color.White;
        public const bool DEFAULT_HNRMBOLD = false;
        public const bool DEFAULT_HKEYBOLD = false;
        public const bool DEFAULT_HIDTBOLD = false;
        public const bool DEFAULT_HSTRBOLD = false;
        public const bool DEFAULT_HCMTBOLD = false;
        public const bool DEFAULT_HNRMITALIC = false;
        public const bool DEFAULT_HKEYITALIC = false;
        public const bool DEFAULT_HIDTITALIC = false;
        public const bool DEFAULT_HSTRITALIC = false;
        public const bool DEFAULT_HCMTITALIC = false;
        public const int DEFAULT_OPENONERR = 0;
        public const int DEFAULT_SAVEONCOMP = 0;
        public const int DEFAULT_COMPONRUN = 0;
        public const int DEFAULT_LOGICUNDO = -1;
        public const int DEFAULT_WARNMSGS = 0;
        public const bool DEFAULT_DEFUSERESDEF = true;
        public const bool DEFAULT_SNIPPETS = true;
        public const bool DEFAULT_AUTOWARN = true;
        public const LogicErrorLevel DEFAULT_ERRORLEVEL = LogicErrorLevel.Medium;
        // default settings - pictures
        public const int DEFAULT_PICSCALE_EDIT = 2;
        public const int DEFAULT_PICSCALE_PREVIEW = 1;
        public const int DEFAULT_PICTEST_HORIZON = 36;
        public const int DEFAULT_PICTEST_OBJSPEED = 1;
        public const int DEFAULT_PICTEST_OBJPRIORITY = 16; //automatic
        public const int DEFAULT_PICTEST_OBJRESTRICTION = 0; //none
        public const bool DEFAULT_PICTEST_IGNOREHORIZON = false;
        public const bool DEFAULT_PICTEST_IGNOREBLOCKS = false;
        public const bool DEFAULT_PICTEST_CYCLEATREST = false;
        public const int DEFAULT_PICUNDO = -1;
        public const bool DEFAULT_SHOWBANDS = true;
        public const bool DEFAULT_SPLITWINDOW = true;
        public const int DEFAULT_CURSORMODE = 1; //pcmXMode
                                                 // default settings - sounds
        public const bool DEFAULT_SHOWKYBD = true;
        public const bool DEFAULT_SHOWNOTES = true;
        public const bool DEFAULT_ONETRACK = false;
        public const int DEFAULT_SNDUNDO = -1;
        public const int DEFAULT_SNDZOOM = 2;
        public const bool DEFAULT_NOMIDI = false;
        public const byte DEFAULT_DEFINST = 80; //Lead 1 (Square)
        public const bool DEFAULT_DEFMUTE = false;
        // default settings - views
        public const int DEFAULT_VIEWSCALE_EDIT = 6;
        public const int DEFAULT_VIEWSCALE_PREVIEW = 3;
        public const int DEFAULT_VIEWALIGNH = 0;
        public const int DEFAULT_VIEWALIGNV = 0;
        public const int DEFAULT_VIEWUNDO = -1;
        public const int DEFAULT_DEFCELH = 32;
        public const int DEFAULT_DEFCELW = 16;
        public const AGIColorIndex DEFAULT_DEFVCOLOR1 = AGIColorIndex.agBlack;
        public const AGIColorIndex DEFAULT_DEFVCOLOR2 = AGIColorIndex.agWhite;
        public const bool DEFAULT_SHOWVEPREV = true;
        public const bool DEFAULT_SHOWGRID = true;
        // default settings - object
        //object
        public const bool DEFAULT_WARNITEM0 = true;
        // default settings - globals
        public const int DEFAULT_GLBUNDO = -1;
        //object
        public const bool DEFAULT_GESHOWCMT = true;
        // default settings - decompile
        public const bool DEFAULT_SHOWALLMSGS = true;
        public const bool DEFAULT_MSGSBYNUM = false;
        public const bool DEFAULT_ELSEASGOTO = false;
        public const bool DEFAULT_SPECIALSYNTAX = true;
        public const bool DEFAULT_SHOWRESVARS = true;
        public const bool DEFAULT_USERESERVED = true;
        // default settings - layout editor
        public const bool DEFAULT_DEFUSELE = true;
        public const bool DEFAULT_LEPAGES = true;
        public const int DEFAULT_LEWARNDELETE = 0;
        public const bool DEFAULT_LEUSEGRID = true;
        public const double DEFAULT_LEGRID = 0.1;
        public const bool DEFAULT_LESHOWPICS = true;
        public const bool DEFAULT_LESYNC = true;
        public static readonly Color DEFAULT_LEROOM_EDGE = Color.FromArgb(0, 0x55, 0xAA);       //   0x55AA;
        public static readonly Color DEFAULT_LEROOM_FILL = Color.FromArgb(0x55, 0xFF, 0xFF);    // 0x55FFFF;
        public static readonly Color DEFAULT_LETRANSPT_EDGE = Color.FromArgb(0x62, 0x62, 0);    // 0x626200;
        public static readonly Color DEFAULT_LETRANSPT_FILL = Color.FromArgb(0xFF, 0xFF, 0x91); // 0xFFFF91;
        public static readonly Color DEFAULT_LEERR_EDGE = Color.FromArgb(0, 0, 0x62);           //     0x62;
        public static readonly Color DEFAULT_LEERR_FILL = Color.FromArgb(0x91, 0x91, 0xFF);     // 0x9191FF;
        public static readonly Color DEFAULT_LECMT_EDGE = Color.FromArgb(0, 0x62, 0);           //   0x6200;
        public static readonly Color DEFAULT_LECMT_FILL = Color.FromArgb(0x91, 0xFF, 0x91);     // 0x91FF91;
        public static readonly Color DEFAULT_LEEXIT_EDGE = Color.FromArgb(0xA0, 0, 0);          // 0xAA0000;
        public static readonly Color DEFAULT_LEEXIT_OTHERS = Color.FromArgb(0xFF, 0x55, 0xFF);  // 0xFF55FF;
        public const int DEFAULT_LEZOOM = 6;
        //other default settings
        public const int DEFAULT_NAMECASE = 2; //proper
        public const string DEFAULT_NUMFORMAT = "000";
        public const string DEFAULT_SEPARATOR = ".";
        //string constants
        public const string sLOGED = "Logic Editor - ";
        public const string sPICED = "Picture Editor - ";
        public const string sSNDED = "Sound Editor - ";
        public const string SVIEWED = "View Editor - ";
        public const string sDM = "* ";   //dirty file marker//
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
        internal static int MAX_PROPGRID_HEIGHT;
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
        #endregion
        //***************************************************
        // ENUMERATIONS
        //***************************************************
        #region
        public enum UndoNameID {
            UID_UNKNOWN = 0,
            UID_TYPING = 1,
            UID_DELETE = 2,
            UID_DRAGDROP = 3,
            UID_CUT = 4,
            UID_PASTE = 5,
        }
        public enum ReplaceMode {
            rpYes,
            rpYesToAll,
            rpNo,
            rpNoToAll,
        }
        public enum EGetRes {
            grAddNew,
            grRenumber,
            grOpen,
            grTestView,
            grAddLayout,
            grShowRoom,
            grMenu,
            grImport,
            grMenuBkgd,
            grAddInGame,
            grRenumberRoom
        }
        public enum GameSettingFunction {
            gsEdit,
            gsNew,
        }
        public enum ViewEditMode {
            vmBitmap,
            vmView,
            vmLoop,
            vmCel,
        }
        public enum ELogicFormMode {
            fmLogic,
            fmText,
        }
        public enum FindDirection {
            fdAll,
            fdDown,
            fdUp,
        }
        public enum FindLocation {
            flCurrent,
            flOpen,
            flAll,
        }
        public enum FindFormFunction {
            ffFindWord,
            ffReplaceWord,
            ffFindObject,
            ffReplaceObject,
            ffFindLogic,
            ffReplaceLogic,
            ffFindText,
            ffReplaceText,
            ffFindWordsLogic, //used when searching for words or synonyms
            ffFindObjsLogic,  //used when searching for inv objects
            ffFindNone,      // used to temporarily disable find form
                             // when active form is not searchable
        }
        public enum FindFormAction {
            faFind,
            faReplace,
            faReplaceAll,
            faCancel,
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
        public enum EPicCursorMode {
            pcmWinAGI,
            pcmXMode,
        }
        public enum EButtonFace {
            bfDown,
            bfOver,
            bfDialog,
            bfNone = -1,
        }
        public enum ENoteTone {
            ntNone,
            ntNatural,
            ntSharp,
            ntFlat,
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
        public enum EUReason {
            euAddRoom,         //new room added in layout editor
            euShowRoom,        //existing room toggled to show in layout editor
            euRemoveRoom,      //room removed by hiding (IsRoom to false), or actual removal from game
            euRenumberRoom,    //room//s logic number is changed
            euUpdateRoom,      //existing room updated in logic editor
        }
        public enum EArgListType {
            alNone = -2,
            alAll = -1,
            alByte = 0,
            alVar = 1,
            alFlag = 2,
            alMsg = 3,
            alSObj = 4,
            alIObj = 5,
            alStr = 6,
            alWord = 7,
            alCtl = 8,
            alDefStr = 9,
            alVocWrd = 10,
            alIfArg = 11,  //variables and flags
            alOthArg = 12, //variables and strings
            alValues = 13, //variables and bytes
            alLogic = 14,
            alSound = 15,
            alView = 16,
            alPicture = 17,
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
            DirtyLogics,
            ChangeVersion,
        }
        #endregion

        //***************************************************
        // STRUCTS
        //***************************************************
        #region
        //property accessors
        public struct tDefaultScale {
            public int Edit;
            public int Preview;
        }
        public struct tDisplayNote {
            public int Pos;
            public ENoteTone Tone;
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
        }//need to define type here so it is available publicly
        public struct LEObjColor {
            public Color Edge;
            public Color Fill;
        }
        public struct TLEColors {
            public LEObjColor Room;
            public LEObjColor ErrPt;
            public LEObjColor TransPt;
            public LEObjColor Cmt;
            public Color Edge;
            public Color Other;
        }
        public struct GifOptions {
            public int Zoom;
            public bool Transparency;
            public int Delay;
            public bool Cycle;
            public int VAlign;
            public int HAlign;
        }
        public struct agiSettings {
            //general
            public bool ShowSplashScreen;  //if true, the splash screen is shown at startup
            public bool ShowPreview;  //
            public bool ShiftPreview;  // //brings preview window to front when something selected
            public bool HidePreview;  //
            public EResListType ResListType;  //0=no tree; 1=treelist; 2=combo/list boxes
            public enum EResListType {
                None,
                TreeList,
                ComboList
            }
            public bool AutoOpen;  //
            public bool OpenNew;  // opens newly added/imported resources in an editor after being added
            public bool AutoExport;  //
            public bool AskExport;  //if not asking, assume no export
            public bool AskRemove;  //if not asking, assume OK to remove
            public int AutoUpdateDefines;  //0 = ask; 1 = no; 2 = yes
            public int AutoUpdateResDefs;  //0 = ask; 1 = no; 2 = yes
                                           ////    int AutoUpdateWords;  //0 = ask; 1 = no; 2 = yes
                                           ////    int AutoUpdateObjects;  //0 = ask; 1 = no; 2 = yes
            public bool ShowResNum;  //
            public bool IncludeResNum;  //
            public int RenameWAG;  // 0 = ask; 1 = no; 2 = yes
            public bool RenameDelRes;  //
            public int MaxSO;  //
            public int MaxVol0Size;
            public tResource ResFormat;
            public int DefCP;
            // compile
            public bool WarnCompile;  //when true, a warning is shown when a logic is closed that isn//t compiled
            public bool NotifyCompSuccess;  //when true, a message is shown after logic is compiled
            public bool NotifyCompWarn;  //
            public bool NotifyCompFail;  //
            public bool WarnDupGName;  //
            public bool WarnDupGVal;  //
            public bool WarnInvalidStrVal;
            public bool WarnInvalidCtlVal;
            public bool WarnResOvrd;  // //warn user if attempting to override definition of a reserved var/flag/etc
            public int DelBlankG;  //0 = ask; 1 = no; 2 = yes
            public bool WarnDupObj;
            public bool AutoWarn;
            // logics
            public bool HighlightLogic;
            public bool HighlightText;
            public int LogicTabWidth;
            public bool MaximizeLogics;
            public bool AutoQuickInfo;
            public bool ShowDefTips;
            public string DefaultExt;
            public string EFontName;
            public int EFontSize;
            public string PFontName;
            public int PFontSize;
            //Normal=0
            //Keyword=1
            //Identifier=2
            //String=3
            //Comment=4
            //Background=5 (color only)
            public Color[] HColor; // (6) OLE_COLOR
            public bool[] HItalic; // (5)
            public bool[] HBold; // (5)
            public int OpenOnErr;  // //0 = ask; 1 = no; 2 = yes
            public int SaveOnCompile;  // //0 = ask; 1 = no; 2 = yes
            public int CompileOnRun;  // //0 = ask; 1 = no; 2 = yes
            public int LogicUndo;  //
            public int WarnMsgs;  //  //0 = ask; 1 = keep all; 2 = keep only used
            //public LogicErrorLevel ErrorLevel; //default error level for new games
            public bool DefUseResDef;  //default value for UseResDef
            public bool Snippets;  // determines if Snippets are used in logic/text code
                                   //pictures
            public tDefaultScale PicScale;
            public TPicTest PicTest;
            public int PicUndo;
            public bool ShowBands;
            public bool SplitWindow;
            public EPicCursorMode CursorMode;
            // sounds
            public bool ShowKybd;
            public bool ShowNotes;
            public bool OneTrack;
            public int SndUndo;
            public int SndZoom;
            public bool NoMIDI;
            public byte DefInst0;
            public byte DefInst1;
            public byte DefInst2;
            public bool DefMute0;
            public bool DefMute1;
            public bool DefMute2;
            public bool DefMute3;
            // views
            public tDefaultScale ViewScale;
            public int ViewAlignH;
            public int ViewAlignV;
            public int ViewUndo;
            public byte DefCelH;
            public byte DefCelW;
            public int DefVColor1;
            public int DefVColor2;
            public bool ShowVEPrev;
            public bool ShowGrid;
            //object
            public bool WarnItem0;  //give warning if item 0 is edited
                                    //words.tok
                                    // none
                                    // layout
                                    // use of layout editor is a game property;
                                    // all other layout properties are application settings
            public bool DefUseLE;  //default value for new games
            public bool LEPages;
            public int LEDelPicToo;  // //0 = ask; 1 = no; 2 = yes
            public bool LEShowPics;  //false=no pics on rooms when drawn
            public bool LEUseGrid;
            public double LEGrid;
            public TLEColors LEColors;
            public int LEZoom;
            public bool LESync;
            // globals
            public int GlobalUndo;
            public bool GEShowComment;
            public double GENameFrac;
            public double GEValFrac;
            // menus
            public bool AutoAlignHotKey;
            // platform
            public bool AutoFill;
            public int PlatformType;
            public string PlatformFile;
            public string DOSExec;
            public string PlatformOpts;
        }
        public struct LCoord {
            public double X;
            public double Y;
        }
        public struct PT {
            public byte X;
            public byte Y;
        }
        public struct tResource {
            public int NameCase; //0=lower, 1=upper, 2=proper
            public string Separator;
            public string NumFormat;
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
            public CompStatus Status;
            public Exception CompExc;
            public string parm;
        }

        #endregion
        //***************************************************
        // GLOBAL VARIABLES
        //***************************************************
        #region
        public static AGIGame EditGame;

        public static frmMDIMain MDIMain;
        public static string ProgramDir;
        public static string DefaultResDir; //this is the location that the file dialog box uses as the initial directory
        public static NewGameResults NewResults;
        public static LoadGameResults LoadResults;
        public static CompileGameResults CompGameResults;
        public static Encoding SessionCodePage;
        public static AGIResType CurResType;
        public static agiSettings WinAGISettings;
        public static SettingsList WinAGISettingsList;
        public static frmPreview PreviewWin;
        public static frmProgress ProgressWin;
        public static frmCompStatus CompStatusWin;
        public static StatusStrip MainStatusBar;
        //static public CommonDialog OpenDlg
        //static public SaveDialog MDIMain.SaveDlg
        //static public frmCompStatus CompStatusWin
        //static public PictureBox NotePictures
        public static TreeNode RootNode;
        public static TreeNode[] HdrNode;
        public static int SelResNum;
        public static AGIResType SelResType;
        //static public OSVERSIONINFO WinVer
        public static bool SettingError;
        public static bool Compiling;
        public static string WinAGIHelp;
        public static Color PrevWinBColor; //background color for preview window when showing views
        //public static int ScreenTWIPSX;
        //public static int ScreenTWIPSY;
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
        public static bool OEInUse;
        public static int ObjCount;
        public static frmWordsEdit WordEditor;
        public static bool WEInUse;
        public static int WrdCount;
        public static frmGlobals GlobalsEditor;
        public static bool GEInUse;
        //lookup lists for logic editor
        //tooltips and define lists
        public static TDefine[] RDefLookup = new TDefine[95];
        public static TDefine[] GDefLookup;
        public static TDefine[] IDefLookup = new TDefine[1024];
        //  //for now we will not do lookups
        //  // on words and invObjects
        //  // if performance is good enough
        //  // I might consider adding them
        //  public static TDefine[] // ODefLookup()
        //  public static TDefine[] // WDefLookup()
        public static TDefine[] CodeSnippets;
        public static frmSnippets SnippetForm;
        public static int SnipMode; //0=create, 1=manage
        public static GifOptions VGOptions;
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
        public static TDefine[] GlobalsClipboard;
        //default colors
        public static Color[] DefEGAColor = new Color[16];
        //find/replace variables
        public static frmFind FindingForm;
        public static Form SearchForm;
        //global copy of search parameters used by the find form
        public static string GFindText;
        public static string GReplaceText;
        public static FindDirection GFindDir;
        public static bool GMatchWord;
        public static bool GMatchCase;
        public static FindLocation GLogFindLoc;
        public static bool GFindSynonym;
        public static int GFindGrpNum;
        public static int SearchStartPos;
        public static int SearchStartLog;
        public static AGIResType SearchType;
        public static int ObjStartPos;
        public static int StartWord;
        public static int StartGrp;
        public static bool FirstFind;
        public static bool RestartSearch;
        public static bool ClosedLogics;
        public static int ReplaceCount;
        public static bool SearchStartDlg; // true if search started by clicking 'find' or 'find next'
                                           // on FindingForm
        internal static int SearchLogCount;
        internal static int SearchLogVal;
        //property window variables
        public static int DropDownDC;
        public static int DropOverDC;
        public static int DropDlgDC;
        //others
        public static Form HelpParent = null;
        public static string TempFileDir;
        public static string BrowserStartDir = "";
        //workaround to force selection pt to update
        //after showing find form...
        public static bool FixSel;
        //keep track of the global window position
        public static double GWHeight, GWWidth;
        public static double GWLeft, GWTop;
        public static int GWState;
        public static bool GWShowComment;
        public static double GWNameFrac, GWValFrac;
        // graphics variables
        public static bool NoGDIPlus = false;
        #endregion
        //***************************************************
        // GLOBAL STATIC FUNCTIONS
        //***************************************************

        //static Base() {
        //
        //}

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
        public static void GetResDefOverrides() {
            string strIn;
            string[] strDef;
            int intCount, lngGrp;
            int i;
            //check to see if there are any overrides:
            intCount = WinAGISettingsList.GetSetting("ResDefOverrides", "Count", 0);
            if (intCount == 0) {
                return;
            }
            //ok, get the overrides, and apply them
            for (i = 1; i <= intCount; i++) {
                strIn = WinAGISettingsList.GetSetting("ResDefOverrides", "Override" + i, "");
                //split it to get the def value and def name
                //(0)=group
                //(1)=index
                //(2)=newname
                strDef = strIn.Split(":");
                if (strDef.Length == 3) {
                    //get the new name, if a valid entry
                    if (Val(strDef[1]) < LogicCompiler.ResDefByGrp((ResDefGroup)Val(strDef[0])).Length) {
                        LogicCompiler.SetResDef((int)Val(strDef[0]), (int)Val(strDef[1]), strDef[2]);
                    }
                }
            }
            //need to make sure we don't have any bad overrides (where overridden name matches
            //another name); if a duplicate is found, just reset the follow on name back to its
            //default value
            //we check AFTER all overrides are made just in case a swap is desired- checking in
            //realtime would not allow a swap
            if (!LogicCompiler.ValidateResDefs()) {
                //if any were changed, re-write the WinAGI.config file
                SaveResDefOverrides();
            }
        }
        public static void SaveResDefOverrides() {
            //if any reserved define names are different from the default values,
            //write them to the app settings;
            int intCount = 0, i, j;
            TDefine[] dfTemp;
            //need to make string comparisons case sensitive, in case user
            //wants to change case of a define (even though it really doesn't matter; compiler is not case sensitive)

            //first, delete any previous overrides
            WinAGISettingsList.DeleteSection("ResDefOverrides");
            //now step through each type of define value; if name is not the default, then save it
            for (ResDefGroup grp = 0; (int)grp < 10; grp++) {
                dfTemp = LogicCompiler.ResDefByGrp(grp);
                for (i = 0; i < dfTemp.Length; i++) {
                    if (dfTemp[i].Default != dfTemp[i].Name) {
                        //save it
                        intCount++;
                        WinAGISettingsList.WriteSetting("ResDefOverrides", "Override" + intCount, (int)grp + ":" + i + ":" + dfTemp[i].Name);
                    }
                }
            }
            //write the count value
            WinAGISettingsList.WriteSetting("ResDefOverrides", "Count", intCount.ToString());
        }
        public static void InitializeResMan() {
            bool blnCourier = false, blnArial = false;
            bool blnTimes = false, blnConsolas = false;
            //set default fonts
            //priority is consolas, courier new, arial, then times new roman
            foreach (FontFamily font in System.Drawing.FontFamily.Families) {
                if (font.Name.Equals("Consolas", StringComparison.OrdinalIgnoreCase)) {
                    blnConsolas = true;
                }
                if (font.Name.Equals("Courier New", StringComparison.OrdinalIgnoreCase)) {
                    blnCourier = true;
                }
                if (font.Name.Equals("Arial", StringComparison.OrdinalIgnoreCase)) {
                    blnArial = true;
                }
                if (font.Name.Equals("Times New Roman", StringComparison.OrdinalIgnoreCase)) {
                    blnTimes = true;
                }
            }
            if (blnConsolas) {
                DEFAULT_PFONTNAME = "Consolas";
            }
            else if (blnCourier) {
                DEFAULT_PFONTNAME = "Courier New";
            }
            else if (blnArial) {
                DEFAULT_PFONTNAME = "Arial";
            }
            else if (blnTimes) {
                DEFAULT_PFONTNAME = "Times New Roman";
            }
            else {
                //use first font in list
                DEFAULT_PFONTNAME = System.Drawing.FontFamily.Families[0].Name;
            }
            DEFAULT_EFONTNAME = DEFAULT_PFONTNAME;
            WinAGISettings.EFontName = DEFAULT_EFONTNAME;
            WinAGISettings.PFontName = DEFAULT_PFONTNAME;
            // initialize settings arrays
            WinAGISettings.HBold = new bool[5];
            WinAGISettings.HItalic = new bool[5];
            WinAGISettings.HColor = new Color[6];
            //default gif options
            VGOptions.Cycle = true;
            VGOptions.Transparency = true;
            VGOptions.Zoom = 2;
            VGOptions.Delay = 15;
            VGOptions.HAlign = 0;
            VGOptions.VAlign = 1;
            //default value for updating logics is 'checked'
            DefUpdateVal = true;
            //initialize clipboard object if not already done
            GlobalsClipboard = [];
            //initialize code snippet array
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

        public static void ExportLoop(Loop ThisLoop) {
            //export a loop as a gif
            bool blnCanceled;
            DialogResult rtn;
            //show options form
            frmViewGifOptions frmVGO = new();
            //set up form to export a view loop
            frmVGO.InitForm(0, ThisLoop);
            frmVGO.ShowDialog(MDIMain);
            blnCanceled = frmVGO.Canceled;
            //if not canceled, get a filename
            if (!blnCanceled) {
                //set up commondialog
                MDIMain.SaveDlg.Title = "Export Loop GIF";
                MDIMain.SaveDlg.DefaultExt = "gif";
                MDIMain.SaveDlg.Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*";
                MDIMain.SaveDlg.CheckPathExists = true;
                MDIMain.SaveDlg.AddExtension = true;
                MDIMain.SaveDlg.DefaultExt = "gif";
                MDIMain.SaveDlg.FilterIndex = 1;
                MDIMain.SaveDlg.FileName = "";
                do {
                    DialogResult result = MDIMain.SaveDlg.ShowDialog();
                    //if canceled,
                    if (result == DialogResult.Cancel) {
                        //cancel the export
                        return;
                    }
                    //if file exists,
                    if (File.Exists(MDIMain.SaveDlg.FileName)) {
                        //verify replacement
                        rtn = MessageBox.Show(MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?", "Overwrite file?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                        if (rtn == DialogResult.Yes) {
                            break;
                        }
                        else if (rtn == DialogResult.Cancel) {
                            blnCanceled = true;
                            break;
                        }
                    }
                    else {
                        break;
                    }
                } while (true);
            }
            //if NOT canceled, then export!
            if (!blnCanceled) {
                //show ProgressWin form
                ProgressWin = new frmProgress {
                    Text = "Exporting Loop as GIF"
                };
                ProgressWin.lblProgress.Text = "Depending in size of loop, this may take awhile. Please wait...";
                ProgressWin.pgbStatus.Visible = false;
                ProgressWin.Show(MDIMain);
                //show wait cursor
                MDIMain.UseWaitCursor = true;
                MakeLoopGif(ThisLoop, VGOptions, MDIMain.SaveDlg.FileName);
                //all done!
                ProgressWin.Close();
                MessageBox.Show(MDIMain, "Success!", "Export Loop as GIF", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MDIMain.UseWaitCursor = false;
            }
            //done with the options form
            frmVGO.Close();
        }

        public static bool MakeLoopGif(Loop GifLoop, GifOptions GifOps, string ExportFile) {
            string strTempFile;
            byte[] bytData;
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
            //build header
            bytData = new byte[255];
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
            //add color info
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
                bytData[13 + 3 * i] = EditGame.AGIColors[i].B;
                bytData[14 + 3 * i] = EditGame.AGIColors[i].G;
                bytData[15 + 3 * i] = EditGame.AGIColors[i].R;
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
                for (i = 1; i < 12; i++) {
                    bytData[i + 63] = (byte)"NETSCAPE2.0"[i];
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
            bytCelData = new byte[MaxH * MaxW * GifOps.Zoom ^ 2 * 2];
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
                        for (wVal = 0; i < MaxW; wVal++) {
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
                 //now compress the cel data
                bytCmpData = LZW.GifLZW(ref bytCelData);
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
            //add trailer
            bytData[lngPos] = 0x3B;
            //resize 
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
        public static void ExportAllPicImgs() {
            //exports all picture images as one format in src dir
            int lngZoom, lngMode, lngFormat;
            string strExt = "";
            bool blnLoaded;
            //show options form, force image only
            frmPicExpOptions frmPEO = new(1) {
                Text = "Export All Picture Images"
            };
            frmPEO.ShowDialog(MDIMain);
            if (frmPEO.Canceled) {
                //nothing to do
                frmPEO.Close();
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
            ProgressWin = new frmProgress {
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
            //restore cursor
            MDIMain.UseWaitCursor = false;
        }
        static void ExportImg(Picture ExportPic, string ExportFile, int ImgFormat, int ImgMode, int ImgZoom) {
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
                    ExportFile = Left(ExportFile, ExportFile.Length - 4) + "_P" + Right(ExportFile, 4);
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
                //if only one Image being exported OR both are done
                if (ImgMode < 2 || Count == 2) {
                    break;
                }
            } while (true);
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
        public static void ExportOnePicImg(Picture ThisPicture) {
            //exports a picture vis screen and/or pri screen as either bmp or gif, or png
            int lngZoom, lngMode, lngFormat;
            //show options form, save image only
            frmPicExpOptions frmPEO = new(1);
            frmPEO.ShowDialog(MDIMain);
            if (frmPEO.Canceled) {
                //nothing to do
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
            //set up commondialog
            MDIMain.SaveDlg.Title = "Save Picture Image As";
            MDIMain.SaveDlg.DefaultExt = "bmp";
            if (NoGDIPlus) {
                MDIMain.SaveDlg.Filter = "BMP files (*.bmp)|*.bmp|All files (*.*)|*.*";
            }
            else {
                MDIMain.SaveDlg.Filter = "BMP files (*.bmp)|*.bmp|JPEG files (*.jpg)|*.jpg|GIF files (*.gif)|*.gif|TIFF files (*.tif)|*.tif|PNG files (*.PNG)|*.png|All files (*.*)|*.*";
            }
            MDIMain.SaveDlg.OverwritePrompt = true;
            //MDIMain.SaveDlg.Flags = cdlOFNHideReadOnly || cdlOFNPathMustExist || cdlOFNExplorer;
            MDIMain.SaveDlg.FilterIndex = lngFormat;
            MDIMain.SaveDlg.FileName = "";
            DialogResult rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);
            //if NOT canceled, then export!
            if (rtn != DialogResult.Cancel) {
                //show wait cursor
                MDIMain.UseWaitCursor = true;
                ExportImg(ThisPicture, MDIMain.SaveDlg.FileName, lngFormat, lngMode, lngZoom);
                MessageBox.Show("Image saved successfully.", "Export Picture Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MDIMain.UseWaitCursor = false;
            }
        }
        public static void OpenWAGFile(string ThisGameFile = "") {
            // opens a wag file for editing

            if (ThisGameFile.Length == 0) {
                // get a wag file to open
                MDIMain.OpenDlg.Title = "Open WinAGI Game File";
                MDIMain.OpenDlg.Filter = "WinAGI Game file (*.wag)|*.wag|All files (*.*)|*.*";
                MDIMain.OpenDlg.DefaultExt = "";
                MDIMain.OpenDlg.FilterIndex = 1;
                MDIMain.OpenDlg.FileName = "";
                MDIMain.OpenDlg.InitialDirectory = BrowserStartDir;
                if (MDIMain.OpenDlg.ShowDialog() == DialogResult.Cancel) {
                    // user canceled
                    return;
                }
                // use the selected file
                ThisGameFile = MDIMain.OpenDlg.FileName;
            }

            // now open this game file
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
            ProgressWin = new frmProgress {
                Text = "Loading Game"
            };
            ProgressWin.lblProgress.Text = "Checking WinAGI Game file ...";
            ProgressWin.StartPosition = FormStartPosition.CenterParent;
            ProgressWin.pgbStatus.Visible = false;
            // show loading msg in status bar
            MainStatusBar.Items[1].Text = (mode == 0 ? "Loading" : "Importing") + " game; please wait...";
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

            // add wag file to mru, if opened successfully
            if (EditGame is not null) {
                AddToMRU(EditGame.GameFile);
                // set default directory
                BrowserStartDir = EditGame.GameDir;
                //set default text file directory to game source file directory
                DefaultResDir = EditGame.GameDir + EditGame.ResDirName + "\\";
                // build the lookup tables for logic tooltips
                BuildIDefLookup();
                BuildGDefLookup();
                // add game specific resdefs
                int pos = 91;
                for (int i = 0; i < 4; i++) {
                    RDefLookup[pos++] = EditGame.ReservedGameDefines[i];
                }
            }
            else {
                //make sure warning grid is hidden
                if (MDIMain.pnlWarnings.Visible) {
                    MDIMain.HideWarningList(true);
                }
            }
            // refresh main toolbar
            UpdateToolbar();
            // clear status bar
            MainStatusBar.Items[1].Text = "";
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

            // unload in-game sound editors
            for (i = SoundEditors.Count - 1; i >= 0; i--) {
                if (SoundEditors[i].InGame) {
                    j = SoundEditors.Count;
                    SoundEditors[i].Close();
                    // check for cancellation
                    if (j == SoundEditors.Count) {
                        return false;
                    }
                }
            }
            // unload ingame view edit windows
            for (i = ViewEditors.Count - 1; i >= 0; i--) {
                if (ViewEditors[i].InGame) {
                    j = ViewEditors.Count;
                    ViewEditors[i].Close();
                    //check for cancellation
                    if (j == ViewEditors.Count) {
                        return false;
                    }
                }
            }
            // unload ingame picture edit windows
            for (i = PictureEditors.Count - 1; i >= 0; i--) {
                if (PictureEditors[i].InGame) {
                    j = PictureEditors.Count;
                    PictureEditors[i].Close();
                    // check for cancellation
                    if (j == PictureEditors.Count) {
                        return false;
                    }
                }
            }
            // unload ingame logic editors
            for (i = LogicEditors.Count - 1; i >= 0; i--) {
                if (LogicEditors[i].FormMode == ELogicFormMode.fmLogic) {
                    if (LogicEditors[i].InGame) {
                        j = LogicEditors.Count;
                        LogicEditors[i].Close();
                        // check for cancellation
                        if (j == LogicEditors.Count) {
                            return false;
                        }
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
            }
            //unload ingame word editor
            if (WEInUse) {
                WordEditor.Close();
                if (WEInUse) {
                    return false;
                }
            }
            // unload layout editor
            if (LEInUse) {
                LayoutEditor.Close();
                if (LEInUse) {
                    return false;
                }
            }
            // unload globals editor
            if (GEInUse) {
                GlobalsEditor.Close();
                if (GEInUse) {
                }
            }
            // unload the menu editor
            if (MEInUse) {
                MenuEditor.Close();
                if (MEInUse) {
                    return false;
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
            if (WinAGISettings.ShowPreview) {
                // clear preview window
                PreviewWin.ClearPreviewWin();
            }
            // resource editors and preview are closed so all resources
            // should now be unloaded, but just in case...

            // unload all resources
            foreach (Logic tmpLog in EditGame.Logics) {
                if (tmpLog.Loaded) {
                    if (tmpLog.IsDirty) {
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
                    if (tmpPic.IsDirty) {
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
                    if (tmpSnd.IsDirty) {
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
                    if (tmpView.IsDirty) {
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
            if (WinAGISettings.ResListType != agiSettings.EResListType.None) {
                MDIMain.HideResTree();
                MDIMain.ClearResourceList();
            }
            if (WinAGISettings.ShowPreview) {
                PreviewWin.Visible = false;
            }
            // now close the game
            EditGame.CloseGame();
            EditGame = null;
            // restore colors to AGI default when a game closes
            GetDefaultColors();
            // restore default resdef
            LogicCompiler.UseReservedNames = WinAGISettings.DefUseResDef;
            // update main form caption
            MDIMain.Text = "WinAGI GDS";
            // refresh main toolbar
            UpdateToolbar();
            // reset node marker so selection of resources
            // works correctly first time after another game loaded
            MDIMain.LastNodeName = "";
            // reset default text location to program dir
            DefaultResDir = ProgramDir;
            // game is closed
            return true;
        }

        private static void UpdateToolbar() {
            // enable/disable buttons based on current game/editor state
            MDIMain.toolStrip1.Items["btnCloseGame"].Enabled = EditGame != null;
            MDIMain.toolStrip1.Items["btnRun"].Enabled = EditGame != null;
            MDIMain.toolStrip1.Items["btnImportRes"].Enabled = EditGame != null;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = EditGame != null;
            MDIMain.toolStrip1.Items["btnAddRemove"].Enabled = EditGame != null;
            MDIMain.toolStrip1.Items["btnExportRes"].Enabled = EditGame != null;
            MDIMain.toolStrip1.Items["btnLayoutEd"].Enabled = EditGame != null;
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
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.None:
                return;
            case agiSettings.EResListType.TreeList:
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
            case agiSettings.EResListType.ComboList:
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
            if (WinAGISettings.ShowPreview && !nopreview) {
                if (restype == SelResType && resnum == SelResNum) {
                    //redraw the preview
                    PreviewWin.LoadPreview(SelResType, SelResNum);
                }
            }
        }
        // TODO: adjust compilers to refresh tree as needed
        /// <summary>
        /// refreshes all resources in the tree
        /// </summary>
        public static void RefreshTree() {

            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.None:
                return;
            case agiSettings.EResListType.TreeList:
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
            case agiSettings.EResListType.ComboList:
                //update root
                MDIMain.cmbResType.Items[0] = EditGame.GameID;
                switch (MDIMain.cmbResType.SelectedIndex) {
                case 1:
                    foreach (ListViewItem tmpItem in MDIMain.cmbResType.Items) {
                        if (EditGame.Logics[(byte)tmpItem.Tag].Compiled && EditGame.Logics[(byte)tmpItem.Tag].ErrLevel >= 0) {
                            tmpItem.ForeColor = Color.Black;
                        }
                        else {
                            tmpItem.ForeColor = Color.Red;
                        }
                    }
                    break;
                case 2:
                    foreach (ListViewItem tmpItem in MDIMain.cmbResType.Items) {
                        tmpItem.ForeColor = EditGame.Pictures[(byte)tmpItem.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                    }
                    break;
                case 3:
                    foreach (ListViewItem tmpItem in MDIMain.cmbResType.Items) {
                        tmpItem.ForeColor = EditGame.Sounds[(byte)tmpItem.Tag].ErrLevel >= 0 ? Color.Black : Color.Red;
                    }
                    break;
                case 4:
                    foreach (ListViewItem tmpItem in MDIMain.cmbResType.Items) {
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

            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.None:
                return;
            case agiSettings.EResListType.TreeList:
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
            case agiSettings.EResListType.ComboList:
                //update root
                MDIMain.cmbResType.Items[0] = EditGame.GameID;
                //select root
                MDIMain.cmbResType.SelectedIndex = 0;
                break;
            }
            return;
        }

        public static void BuildRDefLookup() {
            //populate the lookup list that logics will
            //use to support tooltips and define list lookups
            //reserve define count:
            // group 0:      27 variables
            // group 1:      18 flags
            // group 2:       5 edge codes
            // group 3:       9 object direction codes
            // group 4:       5 video mode codes
            // group 5:       9 computer type codes
            // group 6:      16 color indices
            // group 7:       1 object (o0)
            // group 8:       1 input prompt (s0)  
            // game specific: 4 (id, about, description, invObjcount)
            // Total of 95

            int pos = 0;
            // first add gloal resdefs
            for (ResDefGroup grp = 0; (int)grp < 9; grp++) {
                for (int i = 0; i < LogicCompiler.ResDefByGrp(grp).Length; i++) {
                    RDefLookup[pos++] = LogicCompiler.ResDefByGrp(grp)[i];
                }
            }
            // then let open logic editors know
            if (LogicEditors.Count > 1) {
                foreach (frmLogicEdit tmpEd in LogicEditors) {
                    tmpEd.ListDirty = true;
                }
            }
        }

        public static void BuildSnippets() {
            // loads snippet file, and creates array of snippets
            SettingsList SnipList = new(ProgramDir + "snippets.txt", FileMode.OpenOrCreate);
            int lngCount;
            int i, lngAdded;

            // if nothing returned (meaning file was empty)
            if (SnipList.Lines.Count == 0) {
                return;
            }
            // get count
            lngCount = SnipList.GetSetting("General", "Count", 0);
            //if none
            if (lngCount <= 0) {
                //no Snippets
                return;
            }
            //create snippet array
            CodeSnippets = new TDefine[lngCount];
            //retrieve each snippet (no error checking is done
            // except for blank value or blank name; in that case
            // the snippet is ignored; if duplicate names exist,
            // they are added, and user will just have to deal
            // with it...
            lngAdded = 0;
            for (i = 0; i < lngCount; i++) {
                //name
                CodeSnippets[lngAdded].Name = SnipList.GetSetting("Snippet" + (lngAdded + 1), "Name", "");
                //value
                CodeSnippets[lngAdded].Value = SnipList.GetSetting("Snippet" + (lngAdded + 1), "Value", "");

                //if name and value are non-null
                if (CodeSnippets[lngAdded].Name.Length > 0 && CodeSnippets[lngAdded].Value.Length > 0) {
                    //decode the snippet (replaces control codes)
                    CodeSnippets[lngAdded].Value = DecodeSnippet(CodeSnippets[lngAdded].Value);
                    //count it as added
                    lngAdded++;
                }
                else {
                    //one or both of name/value are blank - not a
                    //valid snippet so ignore it
                }
            }
            //if some were skipped
            if (lngAdded < lngCount) {
                //shrink the array
                Array.Resize(ref CodeSnippets, lngAdded);
            }
        }
        public static string DecodeSnippet(string SnipText) {
            //replaces control codes in SnipText and returns
            //the full expanded text
            //(does not handle argument values; they are left in
            // place until needed when a snippet is inserted into
            // a logic)

            //first check for '%%' - temporarily replace them
            // with char255
            string retval = SnipText.Replace("%%", ((char)255).ToString());
            // carriage returns/new lines
            retval = retval.Replace("%n", (Keys.Enter).ToString());

            //quote marks
            retval = retval.Replace("%q", QUOTECHAR.ToString());

            //tabs
            retval = retval.Replace("%t", new String(' ', WinAGISettings.LogicTabWidth));

            //lastly, restore any forced percent signs
            retval = retval.Replace((char)255, '%');
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
            //adds all resource IDs to the table, making sure
            //anything that//s blank gets reset

            //***NOTE that order of resources is
            // LOGIC-VIEW-SOUND-PIC; not the normal L-P-S-V
            // this is because logics are most likely to be
            // referenced, followed by views, then sounds,
            // and pictures are least likely to be referenced
            // by their IDs

            int i;
            int last;
            TDefine tmpDef = new();
            TDefine tmpBlank = new() {
                //blanks have a type of 11 (>highest available)
                Type = (ArgTypeEnum)11
            };

            //on initial build, populate all the values,
            //because they never change

            //logics first
            last = EditGame.Logics.Max;
            for (i = 0; i <= last; i++) {
                if (EditGame.Logics.Contains((byte)i)) {
                    tmpDef.Name = EditGame.Logics[(byte)i].ID;
                    tmpDef.Type = atNum;
                    tmpDef.Value = i.ToString();
                    IDefLookup[i] = tmpDef;
                }
                else {
                    tmpBlank.Value = i.ToString();
                    IDefLookup[i] = tmpBlank;
                }
            }
            for (i = last + 1; i < 256; i++) {
                tmpBlank.Value = i.ToString();
                IDefLookup[i] = tmpBlank;
            }

            //views next
            last = EditGame.Views.Max;
            for (i = 0; i <= last; i++) {
                if (EditGame.Views.Contains((byte)i)) {
                    tmpDef.Name = EditGame.Views[(byte)i].ID;
                    tmpDef.Type = atNum;
                    tmpDef.Value = i.ToString();
                    IDefLookup[i + 256] = tmpDef;
                }
                else {
                    tmpBlank.Value = i.ToString();
                    IDefLookup[i + 256] = tmpBlank;
                }
            }
            for (i = last + 1; i < 256; i++) {
                tmpBlank.Value = i.ToString();
                IDefLookup[i + 256] = tmpBlank;
            }
            //then sounds next
            last = EditGame.Sounds.Max;
            for (i = 0; i <= last; i++) {
                if (EditGame.Sounds.Contains((byte)i)) {
                    tmpDef.Name = EditGame.Sounds[(byte)i].ID;
                    tmpDef.Type = atNum;
                    tmpDef.Value = i.ToString();
                    IDefLookup[i + 512] = tmpDef;
                }
                else {
                    tmpBlank.Value = i.ToString();
                    IDefLookup[i + 512] = tmpBlank;
                }
            }
            for (i = last + 1; i < 256; i++) {
                tmpBlank.Value = i.ToString();
                IDefLookup[i + 512] = tmpBlank;
            }
            //pictures last (least likely to be used in a logic by ID)
            last = EditGame.Pictures.Max;
            for (i = 0; i <= last; i++) {
                if (EditGame.Pictures.Contains((byte)i)) {
                    tmpDef.Name = EditGame.Pictures[(byte)i].ID;
                    tmpDef.Type = atNum;
                    tmpDef.Value = i.ToString();
                    IDefLookup[i + 768] = tmpDef;
                }
                else {
                    tmpBlank.Value = i.ToString();
                    IDefLookup[i + 768] = tmpBlank;
                }
            }
            for (i = last + 1; i < 256; i++) {
                tmpBlank.Value = i.ToString();
                IDefLookup[i + 768] = tmpBlank;
            }

            //don't need to worry about open editors; the initial build is
            //only called when a game is first loaded; changes to the
            //ID lookup list are handled by the add/remove resource functions
        }

        public static void BuildGDefLookup() {
            //loads all global defines into single list for use by
            //the logic tooltip lookup function

            //we don't have to worry about errors (we just ignore them)
            //or order (add them as they are in the file)
            //or duplicate ("just leave it!!!") :-)
            //or resdef overrides (it's actually easier for the tootip function anyway)
            string strFileName;
            string strLine;
            string[] strSplitLine;
            TDefine tmpDef = new();
            bool blnTry;
            int i, NumDefs = 0;

            //clear the lookup list
            GDefLookup = [];
            try {
                strFileName = EditGame.GameDir + "globals.txt";
                // if no global file, just exit
                if (!File.Exists(strFileName)) {
                    return;
                }
                //open file for input
                using FileStream fsGlobal = new(strFileName, FileMode.Open);
                using StreamReader srGlobal = new(fsGlobal);

                //read in globals
                while (!srGlobal.EndOfStream) {
                    //get line
                    strLine = srGlobal.ReadLine();
                    //trim it - also, skip comments
                    string a = "";
                    strLine = LogicCompiler.StripComments(strLine, ref a, true);
                    //ignore blanks
                    if (strLine.Length != 0) {
                        //even though new format is to match standard #define format,
                        //still need to look for old format first just in case;
                        //when saved, the file will be in the new format

                        //assume not valid until we prove otherwise
                        blnTry = false;

                        //splitline into name and Value
                        strSplitLine = strLine.Split((char)Keys.Tab);

                        //if exactly two elements,
                        if (strSplitLine.Length == 2) {
                            tmpDef.Name = strSplitLine[0].Trim();
                            tmpDef.Value = strSplitLine[1].Trim();
                            blnTry = true;

                            //not a valid global.txt; check for defines.txt
                        }
                        else {
                            //tabs need to be replaced with spaces first
                            strLine = strLine.Replace((char)Keys.Tab, ' ').Trim();
                            if (Left(strLine, 8).Equals("#define ", StringComparison.OrdinalIgnoreCase)) {
                                //strip off the define statement
                                strLine = Right(strLine, strLine.Length - 8).Trim();
                                //extract define name
                                i = strLine.IndexOf(' ');
                                if (i > 0) {
                                    tmpDef.Name = Left(strLine, i - 1);
                                    strLine = Right(strLine, strLine.Length - i);
                                    tmpDef.Value = strLine.Trim();
                                    blnTry = true;
                                }
                            }
                        }

                        //if the line contains a define, add it to list
                        //here we don't bother validating; if it's a bad
                        //define, then user will have to deal with it;
                        //it's only a tooltip at this point
                        if (blnTry) {
                            tmpDef.Type = DefTypeFromValue(tmpDef.Value);
                            //increment count
                            NumDefs++;
                            Array.Resize(ref GDefLookup, NumDefs);
                            GDefLookup[NumDefs - 1] = tmpDef;
                        }
                    }
                }
                //close file
                fsGlobal.Dispose();
                srGlobal.Dispose();
            }
            catch (Exception) {
                //if error opening file, just exit
                return;
            }
            //don't need to worry about open editors; the initial build is
            //only called when a game is first loaded; changes to the
            //global lookup list are handled by the Global Editor
        }

        public static ArgTypeEnum DefTypeFromValue(string strValue) {
            if (IsNumeric(strValue)) {
                return atNum;
            }
            else if (strValue[0] == 34) {
                return atDefStr;
            }
            else {
                switch ((int)strValue.ToLower()[0]) {
                case 99: //"c"
                    return atCtrl;
                case 102: //"f"
                    return atFlag;
                case 105: //"i"
                    return atInvItem;
                case 109: //"m"
                    return atMsg;
                case 111: //"o"
                    return atSObj;
                case 115: //"s"
                    return atStr;
                case 118: //"v"
                    return atVar;
                case 119: //"w"
                    return atWord;
                default:
                    //assume a defined string
                    return atDefStr;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void CompileAGIGame(string CompGameDir = "", bool RebuildOnly = false) {
            DialogResult rtn = DialogResult.Cancel;
            string strTemp = "";
            bool blnDontAsk = false;

            if (EditGame == null) {
                return;
            }
            //if global editor or layout editor open and unsaved, ask to continue
            if (GEInUse && GlobalsEditor.IsDirty) {
                strTemp = "Do you want to save the Global Defines list before compiling?";
            }
            if (LEInUse && LayoutEditor.IsDirty) {
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
                        if (LayoutEditor.IsDirty) {
                            LayoutEditor.MenuClickSave();
                        }
                    }
                    if (GEInUse) {
                        if (GlobalsEditor.IsDirty) {
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
                if (frm.FormMode == ELogicFormMode.fmLogic) {
                    if (frm.rtfLogic.IsChanged) {
                        switch (WinAGISettings.SaveOnCompile) {
                        case 0:
                            // ask user for input
                            // get user's response
                            frm.Focus();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this logic before compiling?",
                                "Update " + ResourceName(frm.EditLogic, true, true) + "?",
                                 MessageBoxButtons.YesNoCancel,
                                 MessageBoxIcon.Question,
                                 "Always take this action when compiling a game.", ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile = 2;
                                }
                                else if (rtn == DialogResult.No) {
                                    WinAGISettings.SaveOnCompile = 1;
                                }
                                WinAGISettingsList.WriteSetting(sLOGICS, "SaveOnComp", WinAGISettings.SaveOnCompile);
                            }
                            break;
                        case 1:
                            rtn = DialogResult.No;
                            break;
                        case 2:
                            rtn = DialogResult.Yes;
                            break;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return;
                        case DialogResult.Yes:
                            // save it
                            frm.MenuClickSave();
                            break;
                        }
                    }
                }
            }
            foreach (frmPicEdit frm in PictureEditors) {
                if (frm.EditPicture.IsDirty) {
                    if (WinAGISettings.SaveOnCompile != 1) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile == 0) {
                            // get user's response
                            frm.Focus();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this picture before compiling?",
                                "Update " + ResourceName(frm.EditPicture, true, true) + "?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile = 2;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile = 1;
                                }
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
                            frm.MenuClickSave();
                            break;
                        }
                    }
                }
            }
            foreach (frmSoundEdit frm in SoundEditors) {
                if (frm.EditSound.IsDirty) {
                    if (WinAGISettings.SaveOnCompile != 1) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile == 0) {
                            // get user's response
                            frm.Focus();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this Sound before compiling?",
                                "Update " + ResourceName(frm.EditSound, true, true) + "?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile = 2;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile = 1;
                                }
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
                            frm.MenuClickSave();
                            break;
                        }
                    }
                }
            }
            foreach (frmViewEdit frm in ViewEditors) {
                if (frm.EditView.IsDirty) {
                    if (WinAGISettings.SaveOnCompile != 1) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile == 0) {
                            // get user's response
                            frm.Focus();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this View before compiling?",
                                "Update " + ResourceName(frm.EditView, true, true) + "?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile = 2;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile = 1;
                                }
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
                            frm.MenuClickSave();
                            break;
                        }
                    }
                }
            }
            if (OEInUse) {
                if (ObjectEditor.IsDirty) {
                    if (WinAGISettings.SaveOnCompile != 1) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile == 0) {
                            // get user's response
                            ObjectEditor.Focus();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save OBJECT file before compiling?",
                                "Update OBJECT File?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile = 2;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile = 1;
                                }
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
                            ObjectEditor.MenuClickSave();
                            break;
                        }
                    }
                }
            }
            if (WEInUse) {
                if (WordEditor.IsDirty) {
                    if (WinAGISettings.SaveOnCompile != 1) {
                        // saveoncompile is in ask mode or yes mode
                        if (WinAGISettings.SaveOnCompile == 0) {
                            //get user's response
                            WordEditor.Focus();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save WORDS.TOK file before compiling?",
                                "Update WORDS.TOK File?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.",
                                ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile = 2;
                                }
                                else {
                                    WinAGISettings.SaveOnCompile = 1;
                                }
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
                            WordEditor.MenuClickSave();
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
            MainStatusBar.Items[1].Text = RebuildOnly ? "Rebuilding game files, please wait..." : "Compiling game, please wait...";
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
            // clear status bar
            MainStatusBar.Items[1].Text = "";
        }

        public static bool CompileDirtyLogics(bool NoMsg = false) {
            DialogResult rtn = DialogResult.Cancel;
            bool blnDontAsk = false;

            // if no game is loaded,
            if (EditGame == null) {
                return false;
            }
            // check for any open logic resources
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == ELogicFormMode.fmLogic) {
                    if (frm.rtfLogic.IsChanged) {
                        switch (WinAGISettings.SaveOnCompile) {
                        case 0:
                            // ask user for input
                            frm.Focus();
                            rtn = MsgBoxEx.Show(MDIMain,
                                "Do you want to save this logic before compiling?",
                                "Update " + ResourceName(frm.EditLogic, true, true) + "?",
                                MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question,
                                "Always take this action when compiling a game.", ref blnDontAsk);
                            if (blnDontAsk) {
                                if (rtn == DialogResult.Yes) {
                                    WinAGISettings.SaveOnCompile = 2;
                                }
                                else if (rtn == DialogResult.No) {
                                    WinAGISettings.SaveOnCompile = 1;
                                }
                                WinAGISettingsList.WriteSetting(sLOGICS, "SaveOnComp", WinAGISettings.SaveOnCompile);
                            }
                            break;
                        case 1:
                            rtn = DialogResult.No;
                            break;
                        case 2:
                            rtn = DialogResult.Yes;
                            break;
                        }
                        switch (rtn) {
                        case DialogResult.Cancel:
                            return false;
                        case DialogResult.Yes:
                            // save it
                            frm.MenuClickSave();
                            break;
                        }
                    }
                }
            }
            // check for no dirty files; nothing to do in that case
            bool nodirty = true;
            foreach (Logic logres in EditGame.Logics) {
                if (!logres.Compiled) {
                    nodirty = false;
                    break;
                }
            }
            if (nodirty) {
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
            CompStatusWin = new frmCompStatus(CompileMode.DirtyLogics);
            CompStatusWin.StartPosition = FormStartPosition.CenterParent;
            // update status bar to show game is being rebuilt
            MainStatusBar.Items[1].Text = "Compiling dirty logics, please wait...";
            // pass mode & source
            CompGameResults = new CompileGameResults() {
                Mode = CompileMode.DirtyLogics,
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
            // clear status bar
            MainStatusBar.Items[1].Text = "";
            return CompGameResults.Status == CompStatus.OK;
        }

        public static void NewAGIGame(bool UseTemplate) {
            string strVer = "";
            string strDescription = "";
            string strTemplateDir = "";
            int i;

            frmGameProperties propform = new(GameSettingFunction.gsNew);
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
                // version is preset based on template
                propform.cmbVersion.Text = strVer;
                propform.cmbVersion.Enabled = false;
                // default description
                propform.txtGameDescription.Text = strDescription;
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
                ProgressWin = new frmProgress {
                    Text = "Creating New Game"
                };
                ProgressWin.lblProgress.Text = "Creating new game resources ...";
                ProgressWin.StartPosition = FormStartPosition.CenterParent;
                ProgressWin.pgbStatus.Visible = false;
                // show newgame msg in status bar
                MainStatusBar.Items[1].Text = "Creating new game" + (UseTemplate ? " from template" : "") + "; please wait...";
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
                            EditGame.PlatformType = PlatformTypeEnum.DosBox;
                            EditGame.DOSExec = propform.txtExec.Text;
                            EditGame.PlatformOpts = propform.txtOptions.Text;
                        }
                        else if (propform.optScummVM.Checked) {
                            EditGame.PlatformType = PlatformTypeEnum.ScummVM;
                            EditGame.PlatformOpts = propform.txtOptions.Text;
                        }
                        else if (propform.optNAGI.Checked) {
                            EditGame.PlatformType = PlatformTypeEnum.NAGI;
                        }
                        else if (propform.optOther.Checked) {
                            EditGame.PlatformType = PlatformTypeEnum.Other;
                            EditGame.PlatformOpts = propform.txtOptions.Text;
                        }
                    }
                    else {
                        EditGame.PlatformType = PlatformTypeEnum.None;
                    }
                    if (EditGame.PlatformType > 0) {
                        EditGame.Platform = propform.NewPlatformFile;
                    }
                    LogicCompiler.UseReservedNames = propform.chkUseReserved.Checked;
                    EditGame.UseLE = propform.chkUseLE.Checked;
                    // force a save of the property file
                    WinAGISettingsList.Save();
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
                    // build the lookup tables for logic tooltips
                    BuildIDefLookup();
                    BuildGDefLookup();
                    // add game specific resdefs
                    int pos = 91;
                    for (i = 0; i < 4; i++) {
                        RDefLookup[pos++] = EditGame.ReservedGameDefines[i];
                    }
                }
                else {
                    //make sure warning grid is hidden
                    if (MDIMain.pnlWarnings.Visible) {
                        MDIMain.HideWarningList(true);
                    }
                }
                // refresh main toolbar
                UpdateToolbar();
                // clear status bar
                MainStatusBar.Items[1].Text = "";
                // reset cursor
                MDIMain.UseWaitCursor = false;
            }
            propform.Dispose();

          return;
        }

        public static int ValidateID(string NewID, string OldID) {
            //validates if a resource ID is agreeable or not
            //returns zero if ok;
            //error Value if not

            //1 = no ID
            //2 = ID is numeric
            //3 = ID is command
            //4 = ID is test command
            //5 = ID is a compiler keyword
            //6 = ID is an argument marker
            //////  //7 = ID is globally defined
            //////  //8 = ID is reserved variable name
            //////  //9 = ID is reserved flag name
            //////  //10 = ID is reserved number constant
            //////  //11 = ID is reserved object constant
            //////  //12 = ID is reserved message constant
            //////  //13 = ID is reserved string constant
            //14 = ID contains improper character
            //15 = ID matches existing ResourceID (not OldID)

            //ignore if it's old id, or a different case of old id
            if (NewID.Equals(OldID, StringComparison.OrdinalIgnoreCase)) {
                //it is OK
                return 0;
            }

            //if no name,
            if (NewID.Length == 0) {
                return 1;
            }

            //name cant be numeric
            if (IsNumeric(NewID)) {
                return 2;
            }

            //check against regular commands
            for (int i = 0; i < Commands.ActionCount; i++) {
                if (NewID.Equals(ActionCommands[i].Name, StringComparison.OrdinalIgnoreCase)) {
                    return 3;
                }
            }

            //check against test commands
            for (int i = 0; i < TestCount; i++) {
                if (NewID.Equals(TestCommands[i].Name, StringComparison.OrdinalIgnoreCase)) {
                    return 4;
                }
            }

            //check against keywords
            if (NewID == "if" || NewID == "else" || NewID == "goto") {
                return 5;
            }

            //check against variable/flag/controller/string/message names
            // if the name starts with any of these letters
            if ("vfmoiswc".Any(NewID.ToLower().StartsWith)) {
                if (IsNumeric(Right(NewID, NewID.Length - 1))) {
                    return 6;
                }
            }

            //check name against improper character lists
            if (INVALID_FIRST_CHARS.Any(ch => ch == NewID[0])) {
                return 14;
            }
            if (NewID.Any(INVALID_ID_CHARS.Contains)) {
                return 14;
            }
            if (NewID.Any(ch => ch > 127 || ch < 32)) {
                return 14;
            }

            //check against existing IDs
            for (int i = 0; i < 1024; i++) {
                if ((int)IDefLookup[i].Type < 11) {
                    if (NewID.Equals(IDefLookup[i].Name, StringComparison.OrdinalIgnoreCase)) {
                        return 15;
                    }
                }
            }
            //ok - 
            return 0;
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

                switch (WinAGISettings.RenameWAG) {
                case 0:
                    //ask for user input
                    rtn = MsgBoxEx.Show(MDIMain, "Do you want to rename your game file to match the new GameID?",
                                        "Rename Game File",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question,
                    "Always take this action when changing GameID.", ref dontAsk);
                    if (dontAsk) {
                        if (rtn == DialogResult.Yes) {
                            WinAGISettings.RenameWAG = 2;
                        }
                        else if (rtn == DialogResult.No) {
                            WinAGISettings.RenameWAG = 1;
                        }
                        WinAGISettingsList.WriteSetting(sGENERAL, "RenameWAG", WinAGISettings.RenameWAG);
                    }
                    break;
                case 1:
                    rtn = DialogResult.No;
                    break;
                case 2:
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
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                break;
            case agiSettings.EResListType.ComboList:
                MDIMain.cmbResType.Items[0] = EditGame.GameID;
                break;
            }
            MDIMain.Text = "WinAGI GDS - " + EditGame.GameID;
            return true;
        }

        public static void ChangeResDir(string NewDir) {
            // validate new dir before changing it
            if ( string.Compare(EditGame.ResDirName, NewDir, true) == 0 || NewDir.Length == 0) {
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
                MainStatusBar.Items[1].Text = "Rebuilding game with new game ID, please wait...";
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
                // clear status bar
                MainStatusBar.Items[1].Text = "";
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
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                if (MDIMain.tvwResources.Nodes[0].Text != EditGame.GameID) {
                    MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                }
                break;
            case agiSettings.EResListType.ComboList:
                if ((string)MDIMain.cmbResType.Items[0] != EditGame.GameID) {
                    MDIMain.cmbResType.Items[0] = EditGame.GameID;
                }
                break;
            }
            return true;
        }

        public static void UpdateReservedToken(string strToken) {
            // checks all logics; if the token is in use in a logic, it gets marked as dirty

            foreach (Logic tmpLogic in EditGame.Logics) {
                if (!tmpLogic.Loaded) {
                    tmpLogic.Load();
                }
                if (FindWholeWord(0, tmpLogic.SourceText, strToken, true, false, AGIResType.None) != -1) {
                    EditGame.Logics.MarkAsDirty(tmpLogic.Number);
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
                    //save it, if dirty
                    if (LayoutEditor != null || LayoutEditor.IsDirty) {
                        LayoutEditor.MenuClickSave();
                    }
                    //close it
                    LayoutEditor.Close();
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
                    strFileName = EditGame.GameDir + "globals.txt";
                    // look for global file
                    if (!File.Exists(strFileName)) {
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
                        // create blank file
                        // TODO: open global file
                    }
                    //load it
                    GlobalsEditor = new frmGlobals(strFileName);
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
                MDIMain.OpenDlg.ShowReadOnly = false;
                MDIMain.OpenDlg.Title = "Open Global Defines File";
                MDIMain.OpenDlg.DefaultExt = "txt";
                MDIMain.OpenDlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                MDIMain.OpenDlg.FilterIndex = WinAGISettingsList.GetSetting("Globals", sOPENFILTER, 1);
                MDIMain.OpenDlg.FileName = "";
                MDIMain.OpenDlg.InitialDirectory = DefaultResDir;
                if (MDIMain.OpenDlg.ShowDialog() == DialogResult.Cancel) {
                    return;
                }
                strFileName = MDIMain.OpenDlg.FileName;
                // save filter
                WinAGISettingsList.WriteSetting("Globals", sOPENFILTER, MDIMain.OpenDlg.FilterIndex);

                DefaultResDir = JustPath(MDIMain.OpenDlg.FileName);
                // check if already open
                foreach (Form tmpForm in MDIMain.MdiChildren) {
                    if (tmpForm.Name == "frmGlobals") {
                        frmGlobals tmpGlobal = tmpForm as frmGlobals;
                        if (tmpGlobal.FileName == strFileName && !tmpGlobal.InGame) {
                            //just shift focus
                            tmpForm.Focus();
                            return;
                        }
                    }
                }
                // not open yet; create new form
                // and open this file into it
                MDIMain.UseWaitCursor = true;
                frmNew = new(strFileName);
                frmNew.Activate();
                MDIMain.UseWaitCursor = false;
            }
            return;
          }

        public static void GetNewNumber(AGIResType ResType, byte OldResNum) {
            byte newnum;
            frmGetResourceNum _frmGetResNum = new(EGetRes.grRenumber, ResType, OldResNum);
            if (_frmGetResNum.ShowDialog(MDIMain) != DialogResult.Cancel) {
                newnum = _frmGetResNum.NewResNum;
                if (newnum != OldResNum) {
                    RenumberResource(ResType, OldResNum, newnum);
                }
            }
            else {
                // canceled
                return;
            }
        }

        public static void RenumberRoom(byte OldResNum, byte NewResNum) {
            RenumberResource(AGIResType.Logic, OldResNum, NewResNum);
            RenumberResource(AGIResType.Picture, OldResNum, NewResNum);
        }

        public static void RenumberResource(AGIResType ResType, byte OldResNum, byte NewResNum) {
            // renumbers a resource

            string strResType = "", strCaption = "", newID = "";
            string oldID = "", oldResFile = "";
            int i;

            //change number for this resource
            switch (ResType) {
            case AGIResType.Logic:
                oldID = EditGame.Logics[OldResNum].ID;
                oldResFile = EditGame.ResDir + EditGame.Logics[OldResNum].ID + ".agl";
                EditGame.Logics.Renumber(OldResNum, NewResNum);
                strResType = "l";
                strCaption = ResourceName(EditGame.Logics[NewResNum], true);
                newID = EditGame.Logics[NewResNum].ID;
                break;
            case AGIResType.Picture:
                oldID = EditGame.Pictures[OldResNum].ID;
                oldResFile = EditGame.ResDir + EditGame.Pictures[OldResNum].ID + ".agp";
                EditGame.Pictures.Renumber(OldResNum, NewResNum);
                strResType = "p";
                strCaption = ResourceName(EditGame.Pictures[NewResNum], true);
                newID = EditGame.Pictures[NewResNum].ID;
                break;
            case AGIResType.Sound:
                oldID = EditGame.Sounds[OldResNum].ID;
                oldResFile = EditGame.ResDir + EditGame.Sounds[OldResNum].ID + ".ags";
                EditGame.Sounds.Renumber(OldResNum, NewResNum);
                strResType = "s";
                strCaption = ResourceName(EditGame.Sounds[NewResNum], true);
                newID = EditGame.Sounds[NewResNum].ID;
                break;
            case AGIResType.View:
                oldID = EditGame.Views[OldResNum].ID;
                oldResFile = EditGame.ResDir + EditGame.Views[OldResNum].ID + ".agv";
                EditGame.Views.Renumber(OldResNum, NewResNum);
                strResType = "v";
                strCaption = ResourceName(EditGame.Views[NewResNum], true);
                newID = EditGame.Views[NewResNum].ID;
                break;
            }
            if (oldID != newID) {
                //update resource file if ID has changed
                UpdateResFile(ResType, NewResNum, oldResFile);
            }
            // update resource list
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                int lngPos = 0;
                TreeNode tmpNode = HdrNode[(int)ResType];

                // add in new position
                // (add it before removing current, to minimize changes in resource list)
                for (lngPos = 0; lngPos < tmpNode.Nodes.Count; lngPos++) {
                    if ((byte)tmpNode.Nodes[lngPos].Tag > NewResNum) {
                        break;
                    }
                }
                //add to tree
                tmpNode = tmpNode.Nodes.Insert(lngPos, strResType + NewResNum, strCaption);
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
                HdrNode[(int)ResType].Nodes[strResType + OldResNum].Remove();
                break;
            case agiSettings.EResListType.ComboList:
                // only update if the resource type is being listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)ResType) {
                    // remove it from current location
                    MDIMain.lstResources.Items[strResType + OldResNum.ToString()].Remove();
                    ListViewItem tmpListItem;
                    // find a place to insert this logic in the box list
                    for (lngPos = 0; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((byte)MDIMain.lstResources.Items[lngPos].Tag > NewResNum) {
                            break;
                        }
                    }
                    tmpListItem = MDIMain.lstResources.Items.Insert(lngPos, strResType + NewResNum, strCaption, 0);
                    tmpListItem.Tag = NewResNum;
                    if (ResType == AGIResType.Logic) {
                        tmpListItem.ForeColor = EditGame.Logics[NewResNum].Compiled ? Color.Black : Color.Red;
                    }
                }
                break;
            }
            //update the logic tooltip lookup table
            IDefLookup[OldResNum].Name = "";
            IDefLookup[OldResNum].Type = ArgTypeEnum.atVar;
            IDefLookup[NewResNum].Name = newID;
            IDefLookup[NewResNum].Type = atNum;
            //then let open logic editors know
            if (LogicEditors.Count > 0) {
                for (i = 0; i < LogicEditors.Count; i++) {
                    LogicEditors[i].ListDirty = true;
                }
            }
        }

        public static bool GetNewResID(AGIResType ResType, int ResNum, ref string ResID, ref string Description, bool InGame, int FirstProp) {
            // ResID and Description are passed ByRef, because the resource editors
            // need the updated values passed back to them

            string strOldResFile = "", strOldDesc;
            string strOldID;
            bool blnReplace; //used when replacing IDs in logics
            int Index;
            int rtn;
            string strErrMsg = "";

            // should never get here with other restypes
            switch (ResType) {
            case Game:
            case Layout:
            case Menu:
            case Globals:
            case Text:
            case None:
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
            frmEditDescription _frmEditDesc;
            while (true) {
                // get new values
                _frmEditDesc = new(ResType, (byte)ResNum, ResID, Description, InGame, FirstProp);
                if (_frmEditDesc.ShowDialog(MDIMain) == DialogResult.Cancel) {
                    _frmEditDesc.Dispose();
                    return false;
                }
                //validate return results
                // TODO: why not validate on the form, before returning?
                if (ResType == Objects || ResType == Words) {
                    //only have description, so no need to validate
                    break;
                }
                else {
                    if (!strOldID.Equals(_frmEditDesc.NewID, StringComparison.OrdinalIgnoreCase)) {
                        //validate new id
                        rtn = ValidateID(_frmEditDesc.NewID, strOldID);
                        switch (rtn) {
                        case 0:
                            //ok
                            break;
                        case 1:
                            // no ID
                            strErrMsg = "Resource ID cannot be blank.";
                            break;
                        case 2: // ID is numeric
                            strErrMsg = "Resource ID cannot be numeric.";
                            break;
                        case 3:
                            // ID is command
                            strErrMsg = "'" + _frmEditDesc.txtID.Text + "' is an AGI command, and cannot be used as a resource ID.";
                            break;
                        case 4:
                            // ID is test command
                            strErrMsg = "'" + _frmEditDesc.txtID.Text + "' is an AGI test command, and cannot be used as a resource ID.";
                            break;
                        case 5:
                            // ID is a compiler keyword
                            strErrMsg = "'" + _frmEditDesc.txtID.Text + "' is a compiler reserved word, and cannot be used as a resource ID.";
                            break;
                        case 6:
                            // ID is an argument marker
                            strErrMsg = "Resource IDs cannot be argument markers";
                            break;
                        case 14:
                            // ID contains improper character
                            strErrMsg = "Invalid character in resource ID:" + Environment.NewLine + "   !" + QUOTECHAR + "&//()*+,-/:;<=>?[\\]^`{|}~ and spaces" + Environment.NewLine + "are not allowed.";
                            break;
                        case 15:
                            // ID matches existing ResourceID
                            //only enforce if in a game
                            if (InGame) {
                                strErrMsg = "'" + _frmEditDesc.txtID.Text + "' is already in use as a resource ID.";
                            }
                            else {
                                //reset to no error
                                rtn = 0;
                            }
                            break;
                        }
                        // if there is an error
                        if (rtn != 0) {
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
                            //make the change
                            //update ID for the ingame resource
                            switch (ResType) {
                            case AGIResType.Logic:
                                DialogResult result;
                                if (File.Exists(EditGame.ResDir + _frmEditDesc.NewID + EditGame.SourceExt)) {
                                    // import existing, or overwrite it?
                                    result = MessageBox.Show(MDIMain,
                                       "There is already a source file with the name '" + _frmEditDesc.NewID +
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
                                    SafeFileMove(EditGame.ResDir + _frmEditDesc.NewID + EditGame.SourceExt, EditGame.Logics[ResNum].SourceFile, true);
                                }
                                // change id (which automatically renames source file,
                                // including overwriting an existing file
                                EditGame.Logics[ResNum].ID = _frmEditDesc.NewID;
                                break;
                            case AGIResType.Picture:
                                EditGame.Pictures[ResNum].ID = _frmEditDesc.NewID;
                                break;
                            case AGIResType.Sound:
                                EditGame.Sounds[ResNum].ID = _frmEditDesc.NewID;
                                break;
                            case AGIResType.View:
                                EditGame.Views[ResNum].ID = _frmEditDesc.NewID;
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
            if (strOldID != _frmEditDesc.NewID) {
                ResID = _frmEditDesc.NewID;
            }
            // if description changed, update it
            if (strOldDesc != _frmEditDesc.NewDescription) {
                Description = _frmEditDesc.NewDescription;
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
            // save replace flag value
            // save state of update logic flag
            blnReplace = DefUpdateVal = _frmEditDesc.chkUpdate.Checked;
            // update the logic tooltip lookup table for log/pic/view/snd
            switch (ResType) {
            case AGIResType.Logic:
                Index = ResNum;
                break;
            case AGIResType.View:
                Index = ResNum + 256;
                break;
            case AGIResType.Sound:
                Index = ResNum + 512;
                break;
            case AGIResType.Picture:
                Index = ResNum + 768;
                break;
            default:
                Index = -1;
                break;
            }
            if (Index >= 0) {
                IDefLookup[Index].Name = ResID;
            }
            _frmEditDesc.Dispose();

            // for ingame resources, update resource files, preview, treelist
            if (InGame) {
                switch (ResType) {
                case AGIResType.Logic:
                case AGIResType.Picture:
                case AGIResType.Sound:
                case AGIResType.View:
                    if (strOldID != ResID) {
                        // if not just a change in text case
                        if (!strOldID.Equals(ResID, StringComparison.OrdinalIgnoreCase)) {
                            //update resource file if ID has changed
                            UpdateResFile(ResType, (byte)ResNum, strOldResFile);
                            // logic ID change may result in an import so refresh preview if active
                            if (SelResType == AGIResType.Logic && SelResNum == ResNum) {
                                if (WinAGISettings.ShowPreview) {
                                    PreviewWin.LoadPreview(AGIResType.Logic, ResNum);
                                }
                            }
                        }
                        else {
                            // just change the filename
                                switch (ResType) {
                                case AGIResType.Logic:
                                    SafeFileMove(strOldResFile, EditGame.ResDir + ResID + EditGame.SourceExt, true);
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
                                    frm.ListDirty = true;
                                }
                            }
                        }
                        // if OK to update in all logics, do so
                        if (blnReplace) {
                            // TODO: add find/replace
                            ////reset search flags
                            //FindingForm.ResetSearch
                            ////now replace the ID
                            //ReplaceAll(strOldID, ResID, fdAll, true, true, flAll, ResType);
                        }
                    }
                    break;
                }
            }
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
            string strFileName;

            //set up commondialog
            if (InGame) {
                MDIMain.SaveDlg.Title = "Export Source";
            }
            else {
                MDIMain.SaveDlg.Title = "Save Source";
            }
            //if logic already has a filename,
            if (ThisLogic.SourceFile.Length != 0) {
                //use it
                MDIMain.SaveDlg.FileName = ThisLogic.SourceFile;
            }
            else {
                //use default
                //if this is a filename,
                if (ThisLogic.ID.Contains('.')) {
                    MDIMain.SaveDlg.FileName = EditGame.ResDir + Left(ThisLogic.ID, ThisLogic.ID.LastIndexOf('.') - 1) + EditGame.SourceExt;
                }
                else {
                    MDIMain.SaveDlg.FileName = EditGame.ResDir + ThisLogic.ID + EditGame.SourceExt;
                }
            }
            MDIMain.SaveDlg.Filter = "WinAGI Logic Source Files (*.lgc)|*.lgc|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (Right(MDIMain.SaveDlg.FileName, 4).ToLower() == ".txt") {
                MDIMain.SaveDlg.FilterIndex = 2;
            }
            else {
                MDIMain.SaveDlg.FilterIndex = 1;
            }
            MDIMain.SaveDlg.DefaultExt = Right(EditGame.SourceExt, EditGame.SourceExt.Length - 1);
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.ExpandedMode = true;
            MDIMain.SaveDlg.ShowHiddenFiles = false;

            do {
                rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);

                //if canceled,
                if (rtn == DialogResult.Cancel) {
                    // nothing selected
                    return "";
                }

                //get filename
                strFileName = MDIMain.SaveDlg.FileName;

                //if file exists,
                if (File.Exists(strFileName)) {
                    //verify replacement
                    rtn = MessageBox.Show(MDIMain,
                        MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?",
                        "Overwrite file?",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (rtn == DialogResult.Yes) {
                        break;
                    }
                    else if (rtn == DialogResult.Cancel) {
                        // cancel with nothing chosen
                        return "";
                    }
                }
                else {
                    // OK to continue
                    break;
                }
            } while (true);

            //pass back this name
            return strFileName;
        }

        public static void UpdateResFile(AGIResType ResType, byte ResNum, string OldFileName) {
            DialogResult rtn;

            switch (ResType) {
            case AGIResType.Logic:
                // logic sourcefile already handled by ID change
                SafeFileMove(EditGame.ResDir + EditGame.Logics[ResNum].ID + ".agl", EditGame.ResDir + EditGame.Logics[ResNum].ID + "_OLD" + ".agl", true);
                try {
                    if (File.Exists(OldFileName)) {
                        File.Move(OldFileName, EditGame.ResDir + EditGame.Logics[ResNum].ID + ".agl");
                    }
                    else {
                        if (WinAGISettings.AutoExport) {
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
                if (WinAGISettings.AutoExport) {
                    SafeFileMove(EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp", EditGame.ResDir + EditGame.Pictures[ResNum].ID + "_OLD" + ".agp", true);
                    try {
                        if (File.Exists(OldFileName)) {
                            File.Move(OldFileName, EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp");
                        }
                        else {
                            if (WinAGISettings.AutoExport) {
                                EditGame.Pictures[ResNum].Export(EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp");
                            }
                        }
                    }
                    catch (Exception e) {
                        // something went wrong
                        ErrMsgBox(e, "Unable to update Picture Resource File", "", "Update Picture ID Error");
                    }
                }
                break;
            case AGIResType.Sound:
                if (WinAGISettings.AutoExport) {
                    SafeFileMove(EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags", EditGame.ResDir + EditGame.Sounds[ResNum].ID + "_OLD" + ".ags", true);
                    try {
                        if (File.Exists(OldFileName)) {
                            File.Move(OldFileName, EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags");
                        }
                        else {
                            if (WinAGISettings.AutoExport) {
                                EditGame.Sounds[ResNum].Export(EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags");
                            }
                        }
                    }
                    catch (Exception e) {
                        // something went wrong
                        ErrMsgBox(e, "Unable to update Sound Resource File", "", "Update Sound ID Error");
                    }
                }
                break;
            case AGIResType.View:
                if (WinAGISettings.AutoExport) {
                    SafeFileMove(EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv", EditGame.ResDir + EditGame.Views[ResNum].ID + "_OLD" + ".agv", true);
                    try {
                        // if file already exists, rename it,
                        // otherwise use export to create it
                        if (File.Exists(OldFileName)) {
                            File.Move(OldFileName, EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv", true);
                        }
                        else {
                            if (WinAGISettings.AutoExport) {
                                EditGame.Views[ResNum].Export(EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv");
                            }
                        }
                    }
                    catch (Exception e) {
                        // something went wrong
                        ErrMsgBox(e, "Unable to update View Resource File", "", "Update View ID Error");
                    }
                }
                break;
            }
        }
        public static void UpdateExitInfo(EUReason Reason, int LogicNumber, Logic ThisLogic, int NewNum = 0) {
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
        public static void AdjustMenus(AGIResType NewMode, bool InGame, bool Editing, bool IsDirty) {
            ToolStripItem tmpPanel;

            //possible choices:
            //AGIResType.Logic = 0
            //AGIResType.Picture = 1
            //AGIResType.Sound = 2
            //AGIResType.View = 3
            //rtObjects
            //rtWords
            //rtText
            //rtGame
            //rtGlobals
            //rtLayout
            //rtMenu
            //rtWarnings = 11
            //rtNone = 255

            //adjust statusbar
            MainStatusBar.Items.Clear();
            if (Editing) {
                MainStatusBar.Tag = (NewMode.ToString());
            }
            else {
                MainStatusBar.Tag = "";
            }
            //*//

            /*
            //if editing,
            if (Editing) {
              switch (NewMode) {
              case rtNone:
              case rtGame:
              case rtGlobals:
              case rtWarnings:
                //only caps, num, and insert
                tmpPanel = MainStatusBar.Items.Add("");// sbrText
                tmpPanel.DisplayStyle = ToolStripItemDisplayStyle.Text;
                tmpPanel.AutoSize = true; //  sbrSpring;
                tmpPanel.  .Bevel = sbrNoBevel;
                tmpPanel.MinWidth = 132;

                tmpPanel = MainStatusBar.Items.Add(2, , , sbrCaps);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(3, , , sbrNum);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(4, , , sbrIns);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;
                break;
              case AGIResType.Logic:
              case rtText:
                tmpPanel = MainStatusBar.Items.Add(1, "Status", , sbrText);
                tmpPanel.AutoSize = sbrSpring;
                tmpPanel.Alignment = sbrLeft;
                //Debug.Assert !MDIMain.ActiveMdiChild = null;
                tmpPanel.Text = MDIMain.ActiveMdiChild.Tag;

                tmpPanel = MainStatusBar.Items.Add(2, "Row", , sbrText);
                tmpPanel.MinWidth = 1587;
                tmpPanel.Width = 1587;

                tmpPanel = MainStatusBar.Items.Add(3, "Col", , sbrText);
                tmpPanel.MinWidth = 1323;
                tmpPanel.Width = 1323;

                tmpPanel = MainStatusBar.Items.Add(4, , , sbrCaps);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(5, , , sbrNum);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(6, , , sbrIns);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;
                break;
              case AGIResType.Picture:
                tmpPanel = MainStatusBar.Items.Add(1, "Scale", , sbrText);
                tmpPanel.Width = 1720;
                tmpPanel.MinWidth = 1720;

                tmpPanel = MainStatusBar.Items.Add(2, "Mode", , sbrText);
                tmpPanel.Width = 1323;
                tmpPanel.MinWidth = 1323;

                tmpPanel = MainStatusBar.Items.Add(3, "Tool", , sbrText);
                tmpPanel.MinWidth = 1323;
                tmpPanel.Width = 2646;

                tmpPanel = MainStatusBar.Items.Add(4, "Anchor", , sbrText);
                tmpPanel.MinWidth = 1335;
                tmpPanel.Width = 1335;
                tmpPanel.Visible = false;

                tmpPanel = MainStatusBar.Items.Add(5, "Block", , sbrText);
                tmpPanel.MinWidth = 1935;
                tmpPanel.Width = 1935;
                tmpPanel.Visible = false;

                tmpPanel = MainStatusBar.Items.Add(6, , , sbrText);
                tmpPanel.AutoSize = sbrSpring;
                tmpPanel.Bevel = sbrNoBevel;
                tmpPanel.MinWidth = 132;

                tmpPanel = MainStatusBar.Items.Add(7, "CurX", , sbrText);
                tmpPanel.MinWidth = 1111;
                tmpPanel.Width = 1111;

                tmpPanel = MainStatusBar.Items.Add(8, "CurY", , sbrText);
                tmpPanel.MinWidth = 1111;
                tmpPanel.Width = 1111;

                tmpPanel = MainStatusBar.Items.Add(9, "PriBand", , sbrText);
                tmpPanel.Alignment = sbrRight;
                tmpPanel.MinWidth = 1587;
                tmpPanel.Width = 1587;
                break;
              case AGIResType.Sound:
                tmpPanel = MainStatusBar.Items.Add(1, "Scale", , sbrText);
                tmpPanel.Width = 860;
                tmpPanel.MinWidth = 860;

                tmpPanel = MainStatusBar.Items.Add(2, "Time", , sbrText);
                tmpPanel.Width = 1323;
                tmpPanel.MinWidth = 1323;

                tmpPanel = MainStatusBar.Items.Add(3, , , sbrText);
                tmpPanel.AutoSize = sbrSpring;
                tmpPanel.Bevel = sbrNoBevel;
                tmpPanel.MinWidth = 132;

                tmpPanel = MainStatusBar.Items.Add(4, , , sbrCaps);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(5, , , sbrNum)
                  tmpPanel.Alignment = sbrCenter
                  tmpPanel.MinWidth = 750
                  tmpPanel.Width = 750

              tmpPanel = MainStatusBar.Items.Add(6, , , sbrIns);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

              case AGIResType.View;
                tmpPanel = MainStatusBar.Items.Add(1, "Scale", , sbrText);
                tmpPanel.Width = 1720;
                tmpPanel.MinWidth = 1720;

                tmpPanel = MainStatusBar.Items.Add(2, "Tool", , sbrText);
                tmpPanel.Width = 1984;
                tmpPanel.MinWidth = 1984;

                tmpPanel = MainStatusBar.Items.Add(3, , , sbrText);
                tmpPanel.AutoSize = sbrSpring;
                tmpPanel.Bevel = sbrNoBevel;
                tmpPanel.MinWidth = 132;

                tmpPanel = MainStatusBar.Items.Add(4, "CurX", , sbrText);
                tmpPanel.MinWidth = 1111;
                tmpPanel.Width = 1111;

                tmpPanel = MainStatusBar.Items.Add(5, "CurY", , sbrText);
                tmpPanel.MinWidth = 1111;
                tmpPanel.Width = 1111;
                break;
              case rtObjects:
                tmpPanel = MainStatusBar.Items.Add(1, "Count", , sbrText);
                tmpPanel.MinWidth = 1587;
                tmpPanel.Width = 1587;

                tmpPanel = MainStatusBar.Items.Add(2, "Encrypt", , sbrText);
                tmpPanel.MinWidth = 1587;
                tmpPanel.Width = 1587;

                tmpPanel = MainStatusBar.Items.Add(3, , , sbrText);
                tmpPanel.AutoSize = sbrSpring;
                tmpPanel.Bevel = sbrNoBevel;
                tmpPanel.MinWidth = 132;

                tmpPanel = MainStatusBar.Items.Add(4, , , sbrCaps);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(5, , , sbrNum);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(6, , , sbrIns);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;
                break;
              case rtWords:
                tmpPanel = MainStatusBar.Items.Add(1, "GroupCount", , sbrText);
                tmpPanel.MinWidth = 1587;
                tmpPanel.Width = 1587;

                tmpPanel = MainStatusBar.Items.Add(2, "WordCount", , sbrText);
                tmpPanel.MinWidth = 1587;
                tmpPanel.Width = 1587;

                tmpPanel = MainStatusBar.Items.Add(3, , , sbrText);
                tmpPanel.AutoSize = sbrSpring;
                tmpPanel.Bevel = sbrNoBevel;
                tmpPanel.MinWidth = 132;

                tmpPanel = MainStatusBar.Items.Add(4, , , sbrCaps);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(5, , , sbrNum);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(6, , , sbrIns);
                tmpPanel.Alignment = sbrCenter;
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;
                break;
              case rtLayout:
                tmpPanel = MainStatusBar.Items.Add(1, "Scale", , sbrText);
                tmpPanel.MinWidth = 750;
                tmpPanel.Width = 750;

                tmpPanel = MainStatusBar.Items.Add(2, "Tool", , sbrText);
                tmpPanel.MinWidth = 2000;
                tmpPanel.Width = 2000;

                tmpPanel = MainStatusBar.Items.Add(3, "ID", , sbrText);
                tmpPanel.Width = 1323;
                tmpPanel.MinWidth = 1323;

                tmpPanel = MainStatusBar.Items.Add(4, "Type", , sbrText);
                tmpPanel.Width = 1323;
                tmpPanel.MinWidth = 1323;

                tmpPanel = MainStatusBar.Items.Add(5, "Room1", , sbrText);
                tmpPanel.Width = 2646;
                tmpPanel.MinWidth = 2646 //1323;

                tmpPanel = MainStatusBar.Items.Add(6, "Room2", , sbrText);
                tmpPanel.Width = 2646;
                tmpPanel.MinWidth = 2646; //1323

                tmpPanel = MainStatusBar.Items.Add(7, , , sbrText);
                tmpPanel.AutoSize = sbrSpring;
                tmpPanel.Bevel = sbrNoBevel;
                tmpPanel.MinWidth = 132;

                tmpPanel = MainStatusBar.Items.Add(8, "CurX", , sbrText);
                tmpPanel.MinWidth = 1111;
                tmpPanel.Width = 1111;

                tmpPanel = MainStatusBar.Items.Add(9, "CurY", , sbrText);
                tmpPanel.MinWidth = 1111;
                tmpPanel.Width = 1111;
                break;
              }
              //not editing
            } else {
              switch (NewMode) {
              case AGIResType.Logic:
              case AGIResType.Picture:
              case AGIResType.Sound:
              case AGIResType.View:

                break;
              }
            } */
        }
        public static void AddNewLogic(byte NewLogicNumber, Logic NewLogic) {
            int lngPos = 0;
            // TODO: what if importing a compiled logic? maybe don't allow it anymore?

            // add to logic collection in game
            EditGame.Logics.Add((byte)NewLogicNumber, NewLogic);
            // save properties to update the wag file
            EditGame.Logics[NewLogicNumber].SaveProps();
            // always save source to new name
            EditGame.Logics[NewLogicNumber].SaveSource();

            //if using layout editor AND isroom
            if (EditGame.UseLE && EditGame.Logics[NewLogicNumber].IsRoom) {
                // update layout editor and layout data file to show this room is in the game
                UpdateExitInfo(EUReason.euAddRoom, NewLogicNumber, EditGame.Logics[NewLogicNumber]);
            }
            //add to resource list
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                TreeNode tmpNode = HdrNode[0];
                //find place to insert this logic
                for (lngPos = 0; lngPos < HdrNode[0].Nodes.Count; lngPos++) {
                    if ((byte)tmpNode.Nodes[lngPos].Tag > NewLogicNumber) {
                        break;
                    }
                }
                //add to tree
                tmpNode = HdrNode[0].Nodes.Insert(lngPos, "l" + NewLogicNumber, ResourceName(EditGame.Logics[NewLogicNumber], true));
                tmpNode.Tag = NewLogicNumber;
                //load source to set compiled status
                tmpNode.ForeColor = EditGame.Logics[NewLogicNumber].Compiled ? Color.Black : Color.Red;
                break;
            case agiSettings.EResListType.ComboList:
                //only update if logics are being listed
                if (MDIMain.cmbResType.SelectedIndex == 1) {
                    ListViewItem tmpListItem;
                    //find a place to insert this logic in the box list
                    for (lngPos = 0; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((byte)MDIMain.lstResources.Items[lngPos].Tag > NewLogicNumber) {
                            break;
                        }
                    }
                    tmpListItem = MDIMain.lstResources.Items.Insert(lngPos, "l" + NewLogicNumber, ResourceName(EditGame.Logics[NewLogicNumber], true), 0);
                    tmpListItem.Tag = NewLogicNumber;
                    tmpListItem.ForeColor = EditGame.Logics[NewLogicNumber].Compiled ? Color.Black : Color.Red;
                }
                break;
            }
            //update the logic tooltip lookup table
            IDefLookup[NewLogicNumber].Name = EditGame.Logics[NewLogicNumber].ID;
            IDefLookup[NewLogicNumber].Type = atNum;
            //then let open logic editors know
            if (LogicEditors.Count > 0) {
                for (int i = 0; i < LogicEditors.Count; i++) {
                    LogicEditors[i].ListDirty = true;
                }
            }

            //last node marker is no longer accurate; reset
            MDIMain.LastNodeName = "";
            // unload it once all done getting it added
            EditGame.Logics[NewLogicNumber].Unload();
        }

        public static void AddNewPicture(byte NewPictureNumber, Picture NewPicture) {
            int lngPos = 0;
            //add picture to game collection
            EditGame.Pictures.Add((byte)NewPictureNumber, NewPicture);

            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                //find place to insert this picture
                for (lngPos = 0; lngPos < HdrNode[1].Nodes.Count; lngPos++) {
                    if ((byte)HdrNode[1].Nodes[lngPos].Tag > NewPictureNumber) {
                        break;
                    }
                }
                //add it to tree
                HdrNode[1].Nodes.Insert(lngPos, "p" + NewPictureNumber, ResourceName(EditGame.Pictures[NewPictureNumber], true)).Tag = NewPictureNumber;
                break;
            case agiSettings.EResListType.ComboList:
                //only update if pictures are being listed
                if (MDIMain.cmbResType.SelectedIndex == 2) {
                    //find a place to add it
                    for (lngPos = 0; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((byte)MDIMain.lstResources.Items[lngPos].Tag > NewPictureNumber) {
                            break;
                        }
                    }
                    //i is index position we are looking for
                    MDIMain.lstResources.Items.Insert(lngPos, "p" + NewPictureNumber, ResourceName(EditGame.Pictures[NewPictureNumber], true)).Tag = NewPictureNumber;
                    // //expand column width if necessary
                    //if (1.2 * MDIMain.picResources.TextWidth(tmpListItem.Text) > MDIMain.lstResources.ColumnHeaders(1).Width) {
                    //   MDIMain.lstResources.ColumnHeaders(1).Width = 1.2 * MDIMain.picResources.TextWidth(tmpListItem.Text)
                    // }
                }
                break;
            }

            //update the logic tooltip lookup table
            IDefLookup[NewPictureNumber + 768].Name = EditGame.Pictures[NewPictureNumber].ID;
            IDefLookup[NewPictureNumber + 768].Type = atNum;
            //then let open logic editors know
            if (LogicEditors.Count > 0) {
                for (int i = 0; i < LogicEditors.Count; i++) {
                    LogicEditors[i].ListDirty = true;
                }
            }

            //last node marker is no longer accurate; reset
            MDIMain.LastNodeName = "";
        }
        public static void AddNewSound(byte NewSoundNumber, Sound NewSound) {
            int lngPos = 0;
            //add sound to game collection
            EditGame.Sounds.Add((byte)NewSoundNumber, NewSound);
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                //find place to insert this sound
                for (lngPos = 0; lngPos < HdrNode[2].Nodes.Count; lngPos++) {
                    if ((byte)HdrNode[2].Nodes[lngPos].Tag > NewSoundNumber) {
                        break;
                    }
                }
                //add it to tree
                HdrNode[2].Nodes.Insert(lngPos, "s" + NewSoundNumber, ResourceName(EditGame.Sounds[NewSoundNumber], true)).Tag = NewSoundNumber;
                break;
            case agiSettings.EResListType.ComboList:
                //only update if sounds are being updated
                if (MDIMain.cmbResType.SelectedIndex == 3) {
                    //find a place to add it
                    for (lngPos = 0; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((byte)MDIMain.lstResources.Items[lngPos].Tag > NewSoundNumber) {
                            break;
                        }
                    }
                    //i is index position we are looking for
                    MDIMain.lstResources.Items.Insert(lngPos, "s" + NewSoundNumber, ResourceName(EditGame.Sounds[NewSoundNumber], true)).Tag = NewSoundNumber;
                    // //expand column width if necessary
                    //if (1.2 * MDIMain.picResources.TextWidth(tmpListItem.Text) > MDIMain.lstResources.ColumnHeaders(1).Width) {
                    //   MDIMain.lstResources.ColumnHeaders(1).Width = 1.2 * MDIMain.picResources.TextWidth(tmpListItem.Text)
                    // }
                }
                break;
            }

            //update the logic tooltip lookup table
            IDefLookup[NewSoundNumber + 512].Name = EditGame.Sounds[NewSoundNumber].ID;
            IDefLookup[NewSoundNumber + 512].Type = atNum;
            //then let open logic editors know
            if (LogicEditors.Count > 0) {
                for (int i = 0; i < LogicEditors.Count; i++) {
                    LogicEditors[i].ListDirty = true;
                }
            }

            //last node marker is no longer accurate; reset
            MDIMain.LastNodeName = "";
        }
        public static void AddNewView(byte NewViewNumber, Engine.View NewView) {
            int lngPos = 0;
            //add view to game collection
            EditGame.Views.Add((byte)NewViewNumber, NewView);

            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                //find place to insert this view
                for (lngPos = 0; lngPos < HdrNode[3].Nodes.Count; lngPos++) {
                    if ((byte)HdrNode[3].Nodes[lngPos].Tag > NewViewNumber) {
                        break;
                    }
                }
                //add it to tree
                HdrNode[3].Nodes.Insert(lngPos, "v" + NewViewNumber, ResourceName(EditGame.Views[NewViewNumber], true)).Tag = NewViewNumber;
                break;
            case agiSettings.EResListType.ComboList:
                //only update if views are being displayed
                if (MDIMain.cmbResType.SelectedIndex == 4) {
                    //find a place to add it
                    for (lngPos = 1; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((byte)MDIMain.lstResources.Items[lngPos].Tag > NewViewNumber) {
                            break;
                        }
                    }
                    //i is index position we are looking for
                    MDIMain.lstResources.Items.Insert(lngPos, "v" + NewViewNumber, ResourceName(EditGame.Views[NewViewNumber], true)).Tag = NewViewNumber;
                    // //expand column width if necessary
                    //if (1.2 * MDIMain.picResources.TextWidth(tmpListItem.Text) > MDIMain.lstResources.ColumnHeaders(1).Width) {
                    //   MDIMain.lstResources.ColumnHeaders(1).Width = 1.2 * MDIMain.picResources.TextWidth(tmpListItem.Text)
                }
                break;
            }
            //update the logic tooltip lookup table
            IDefLookup[NewViewNumber + 256].Name = EditGame.Views[NewViewNumber].ID;
            IDefLookup[NewViewNumber + 256].Type = atNum;
            //then let open logic editors know
            if (LogicEditors.Count > 0) {
                for (int i = 1; i < LogicEditors.Count; i++) {
                    LogicEditors[i].ListDirty = true;
                }
            }

            //last node marker is no longer accurate; reset
            MDIMain.LastNodeName = "";
        }
        public static void NewLogic(string ImportLogicFile = "") {
            // creates a new logic resource and opens an editor
            frmLogicEdit frmNew;
            bool blnInGame = false;
            Logic tmpLogic;
            bool blnOpen = false;
            string strFile = "";
            bool blnSource = false, blnImporting = false;

            // show wait cursor
            MDIMain.UseWaitCursor = true;
            // create temporary logic
            tmpLogic = new Logic();
            if (ImportLogicFile.Length != 0) {
                blnImporting = true;
                // open file to see if it is sourcecode or compiled logic
                try {
                    using FileStream fsNewLog = new(ImportLogicFile, FileMode.Open);
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
                    blnSource = !strFile.Any(lChars.Contains);
                }
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
                        ErrMsgBox(e, "An error occurred while trying to decompile this logic resource:", "Unable to open this logic.", "Invalid Logic Resource");
                        // restore main form mousepointer and exit
                        MDIMain.UseWaitCursor = false;
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
                            ErrMsgBox(e, "Unable to load this logic resource. It can't be decompiled, and does not appear to be a text file.", "", "Invalid Logic Resource");
                            // restore main form mousepointer and exit
                            MDIMain.UseWaitCursor = false;
                            return;
                        }
                    }
                }
            }
            // get logic number, id , description
            frmGetResourceNum GetResNum = new(blnImporting ? EGetRes.grImport : EGetRes.grAddNew, AGIResType.Logic);
            // suggest ID based on filename
            if (blnImporting) {
                GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportLogicFile).Replace(" ", "");
            }
            // restore cursor while getting resnum
            MDIMain.UseWaitCursor = false;

            // if canceled, release the temporary logic, restore mousepointer and exit
            if (GetResNum.ShowDialog(MDIMain) == DialogResult.Cancel) {
                tmpLogic = null;
                // restore mousepointer and exit
                GetResNum.Dispose();
                return;
            }
            // if user wants logic added to current game
            else if (!GetResNum.DontImport) {
                // show wait cursor while resource is added
                MDIMain.UseWaitCursor = true;
                // add ID and description to tmpLogic
                tmpLogic.ID = GetResNum.txtID.Text;
                tmpLogic.Description = GetResNum.txtDescription.Text;
                bool blnTemplate = GetResNum.chkRoom.Checked;
                if (!blnImporting) {
                    string strLogic;
                    //if not importing, we need to add boilerplate text
                    if (blnTemplate) {
                        // add template text to logic source
                        strLogic = LogTemplateText(tmpLogic.ID, tmpLogic.Description);
                    }
                    else {
                        //add default text
                        strLogic = "[ " + NEWLINE + "[ " + tmpLogic.ID + NEWLINE +
                                   "[ " + NEWLINE + NEWLINE + "return();" + NEWLINE + NEWLINE +
                                   "[*****" + NEWLINE + "[ messages         [  declared messages go here" +
                                   NEWLINE + "[*****";
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

                // if using layout editor AND a room,
                if (EditGame.UseLE && (GetResNum.chkRoom.Checked)) {
                    // update editor and data file to show this room is now in the game
                    UpdateExitInfo(EUReason.euShowRoom, GetResNum.NewResNum, EditGame.Logics[GetResNum.NewResNum]);
                }
                // if including picture
                if (GetResNum.chkIncludePic.Checked) {
                    Picture tmpPic = new();
                    // help user out if they chose a naming scheme
                    if (Left(GetResNum.txtID.Text, 3).Equals("rm.", StringComparison.OrdinalIgnoreCase) && GetResNum.txtID.Text.Length >= 4) {
                        // change ID (if able)
                        if (ValidateID("pic." + Right(GetResNum.txtID.Text, GetResNum.txtID.Text.Length - 3), "") == 0) {
                            //// save old resfile name
                            //strFile = EditGame.ResDir + EditGame.Pictures[GetResNum.NewResNum].ID + ".agp";
                            // change this picture's ID
                            tmpPic.ID = "pic." + Right(GetResNum.txtID.Text, GetResNum.txtID.Text.Length - 3);
                            //// update the resfile, tree and properties
                            //UpdateResFile(AGIResType.Picture, GetResNum.NewResNum, strFile);
                            //// update lookup table
                            //IDefLookup[768 + GetResNum.NewResNum].Name = "pic." + Right(GetResNum.txtID.Text, GetResNum.txtID.Text.Length - 3);
                        }
                        else {
                            tmpPic.ID = GetResNum.txtID.Text;
                        }
                    }
                    // if replacing an existing pic
                    if (EditGame.Pictures.Contains(GetResNum.NewResNum)) {
                        RemovePicture(GetResNum.NewResNum);
                    }
                    AddNewPicture(GetResNum.NewResNum, tmpPic);
                    // pic is still loaded so we need to unload it now
                    EditGame.Pictures[GetResNum.NewResNum].Unload();
                }
                // set ingame flag
                blnInGame = true;
            }
            else {
                // not adding to game; still allowed to use template
                if (GetResNum.chkRoom.Checked) {
                    // add template text
                    tmpLogic.SourceText = LogTemplateText(GetResNum.txtID.Text, GetResNum.txtDescription.Text);
                }
                else {
                    // add default text
                    tmpLogic.SourceText = "[ " + Keys.Enter + "[ " + GetResNum.txtID.Text + Keys.Enter + "[ " + Keys.Enter + Keys.Enter + "return();" + Keys.Enter + Keys.Enter + "[*****" + Keys.Enter + "[ messages         [  declared messages go here" + Keys.Enter + "[*****";
                }
            }
            blnOpen = (GetResNum.chkOpenRes.Checked);
            //make sure resource form is unloaded
            GetResNum.Dispose();
            // only open if user wants it open (or if not in a game or if opening/not importing)
            if (blnOpen || !blnInGame) {
                // open a new logic editing window
                frmNew = new frmLogicEdit {
                    MdiParent = MDIMain
                };
                // pass the logic to the editor
                if (frmNew.InitLogicEditor(tmpLogic)) {
                    frmNew.Show();
                    // add form to collection
                    LogicEditors.Add(frmNew);
                }
                else {
                    frmNew.Close();
                }
            }
            // save openres value
            WinAGISettings.OpenNew = blnOpen;
            // if logic was added to game
            if (blnInGame) {
                // unload it
                EditGame.Logics[tmpLogic.Number].Unload();
            }

            //restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }
        public static void NewPicture(string ImportPictureFile = "") {
            //creates a new picture resource and opens an editor
            frmPicEdit frmNew;
            bool blnInGame = false;
            Picture tmpPic;
            bool blnOpen = false, blnImporting = false;

            MDIMain.UseWaitCursor = true;
            //create temporary picture
            tmpPic = new Picture();
            if (ImportPictureFile.Length != 0) {
                blnImporting = true;
                // import the picture (and check for error)
                try {
                    tmpPic.Import(ImportPictureFile);
                }
                catch (Exception e) {
                    //something wrong
                    ErrMsgBox(e, "Error while importing picture:", "Unable to load this picture resource.", "Import Picture Error");
                    //restore main form mousepointer and exit
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                // now check to see if it's a valid picture resource (by trying to reload it)
                tmpPic.Load();
                if (tmpPic.ErrLevel < 0) {
                    ErrMsgBox(tmpPic.ErrLevel, "Error reading Picture data:", "This is not a valid picture resource.", "Invalid Picture Resource");
                    //restore main form mousepointer and exit
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            // get picture number, id , description
            frmGetResourceNum GetResNum = new(blnImporting ? EGetRes.grAddNew : EGetRes.grImport, AGIResType.Picture);
            // suggest ID based on filename
            if (blnImporting) {
                GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportPictureFile).Replace(" ", "");
            }
            // restore cursor while getting resnum
            MDIMain.UseWaitCursor = false;
            ;
            // if canceled, release the temporary picture, restore cursor and exit method
            if (GetResNum.ShowDialog(MDIMain) == DialogResult.Cancel) {
                // restore mousepointer and exit
                GetResNum.Close();
                return;
            }
            // if user wants picture added to current game
            else if (!GetResNum.DontImport) {
                // show wait cursor while resource is added
                MDIMain.UseWaitCursor = true;
                //add new id and description
                tmpPic.ID = GetResNum.txtID.Text;
                tmpPic.Description = GetResNum.txtDescription.Text;

                // add picture
                AddNewPicture(GetResNum.NewResNum, tmpPic);
                // reset tmpPic to point to the new game picture
                tmpPic = EditGame.Pictures[GetResNum.NewResNum];
                // set ingame flag
                blnInGame = true;
            }
            blnOpen = (GetResNum.chkOpenRes.Checked);

            // make sure resource form is unloaded
            GetResNum.Close();
            // only open if user wants it open (or if not in a game or if opening/not importing)
            if (blnOpen || !blnInGame) {
                // open a new picture editing window
                frmNew = new frmPicEdit() {
                    MdiParent = MDIMain
                };
                // pass the picture to the editor
                if (frmNew.LoadPicture(tmpPic)) {
                    frmNew.Show();
                    PictureEditors.Add(frmNew);
                }
                else {
                    // error
                    frmNew.Close();
                }
            }
            // save openres value
            WinAGISettings.OpenNew = blnOpen;
            // if added to a game
            if (blnInGame) {
                // unload it
                EditGame.Pictures[tmpPic.Number].Unload();
            }
            // restore main form mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void NewSound(string ImportSoundFile = "") {
            //creates a new sound resource and opens and editor
            frmSoundEdit frmNew;
            bool blnInGame = false;
            Sound tmpSound;
            bool blnOpen = false, blnImporting = false;

            // show wait cursor
            MDIMain.UseWaitCursor = true;
            // create temporary sound
            tmpSound = new Sound();
            // set default instrument settings;
            // if a sound is being imported, these may be overridden...
            tmpSound.Track(0).Instrument = WinAGISettings.DefInst0;
            tmpSound.Track(1).Instrument = WinAGISettings.DefInst1;
            tmpSound.Track(2).Instrument = WinAGISettings.DefInst2;
            tmpSound.Track(0).Muted = WinAGISettings.DefMute0;
            tmpSound.Track(1).Muted = WinAGISettings.DefMute1;
            tmpSound.Track(2).Muted = WinAGISettings.DefMute2;
            tmpSound.Track(3).Muted = WinAGISettings.DefMute3;

            // if an import filename was passed
            if (ImportSoundFile.Length != 0) {
                blnImporting = true;
                // import the sound and (and check for error)
                try {
                    tmpSound.Import(ImportSoundFile);
                }
                catch (Exception e) {
                    // something wrong
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
                if (tmpSound.SndFormat != 0) {
                    MessageBox.Show("Error reading Picture data:" + NEWLINE + NEWLINE + "This is not a valid picture resource.", "Invalid Picture Resource", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // restore main form mousepointer and exit
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            // get picture number, id , description
            frmGetResourceNum GetResNum = new(blnImporting ? EGetRes.grAddNew : EGetRes.grImport, AGIResType.Sound);
            // suggest ID based on filename
            if (blnImporting) {
                GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportSoundFile).Replace(" ", "");
            }

            // restore cursor while getting resnum
            MDIMain.UseWaitCursor = false;
            // if canceled, release the temporary sound, restore cursor and exit method
            if (GetResNum.ShowDialog(MDIMain) == DialogResult.Cancel) {
                // restore mousepointer, unload form and exit
                GetResNum.Close();
                return;
            }
            // if user wants sound added to current game
            else if (!GetResNum.DontImport) {
                // show wait cursor again while finishing creating the new sound
                MDIMain.UseWaitCursor = true;
                // add new id and description
                tmpSound.ID = GetResNum.txtID.Text;
                tmpSound.Description = GetResNum.txtDescription.Text;
                // add sound
                AddNewSound(GetResNum.NewResNum, tmpSound);
                // reset tmpSound to point to the new game sound
                tmpSound = EditGame.Sounds[GetResNum.NewResNum];
                blnInGame = true;
            }
            blnOpen = (GetResNum.chkOpenRes.Checked);

            // make sure resource form is unloaded
            GetResNum.Close();
            // only open if user wants it open (or if not in a game or if opening/not importing)
            if (blnOpen || !blnInGame) {
                // open a new sound editing window
                frmNew = new frmSoundEdit() {
                    MdiParent = MDIMain
                };
                // pass the sound to the editor
                if (frmNew.LoadSound(tmpSound)) {
                    frmNew.Show();
                    SoundEditors.Add(frmNew);
                }
                else {
                    // error
                    frmNew.Close();
                }
            }
            // save openres value
            WinAGISettings.OpenNew = blnOpen;
            if (blnInGame) {
                EditGame.Sounds[tmpSound.Number].Unload();
            }
            //restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void NewView(string ImportViewFile = "") {
            // creates a new view editor with a view that is not attached to a game
            frmViewEdit frmNew;
            bool blnInGame = false;
            Engine.View tmpView;
            bool blnOpen = false, blnImporting = false;

            // show wait cursor
            MDIMain.UseWaitCursor = true;
            // create temporary view
            tmpView = new Engine.View();
            // if an import filename was passed
            if (ImportViewFile.Length != 0) {
                blnImporting = true;
                // import the view and (and check for error)
                try {
                    tmpView.Import(ImportViewFile);
                }
                catch (Exception e) {
                    // something wrong
                    ErrMsgBox(e, "An error occurred during import:", "", "Import View Error");
                    // restore main form mousepointer and exit
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                // now check to see if it's a valid picture resource (by trying to reload it)
                tmpView.Load();
                if (tmpView.ErrLevel < 0) {
                    ErrMsgBox(tmpView.ErrLevel, "Error reading View data:", "This is not a valid view resource.", "Invalid View Resource");
                    // restore main form mousepointer and exit
                    MDIMain.UseWaitCursor = false;
                    return;
                }
            }
            else {
                // for new view, add first cel with default height/width
                tmpView.Loops.Add(0);
                tmpView[0].Cels.Add(0);
                tmpView[0][0].Height = WinAGISettings.DefCelH;
                tmpView[0][0].Width = WinAGISettings.DefCelW;
            }
            // get picture number, id , description
            frmGetResourceNum GetResNum = new(blnImporting ? EGetRes.grAddNew : EGetRes.grImport, AGIResType.View);
            // suggest ID based on filename
            if (blnImporting) {
                GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportViewFile).Replace(" ", "");
            }
            // restore cursor while getting resnum
            MDIMain.UseWaitCursor = false;
            // if canceled, release the temporary view, restore cursor and exit method
            if (GetResNum.ShowDialog(MDIMain) == DialogResult.Cancel) {
                tmpView = null;
                // restore mousepointer and exit
                GetResNum.Close();
                return;
            }
            // if user wants view added to current game
            else if (!GetResNum.DontImport) {
                // show wait cursor while resource is added
                MDIMain.UseWaitCursor = true;
                // add new id and description
                tmpView.ID = GetResNum.txtID.Text;
                tmpView.Description = GetResNum.txtDescription.Text;
                // add view
                AddNewView(GetResNum.NewResNum, tmpView);
                // reset tmpView to point to the new game view
                tmpView = EditGame.Views[GetResNum.NewResNum];
                blnInGame = true;
            }
            blnOpen = (GetResNum.chkOpenRes.Checked);
            GetResNum.Close();
            // only open if user wants it open (or if not in a game or if opening/not importing)
            if (blnOpen || !blnInGame) {
                // open a new view editing window
                frmNew = new frmViewEdit() {
                    MdiParent = MDIMain
                };
                // pass the view to the editor
                if (frmNew.LoadView(tmpView)) {
                    frmNew.Show();
                    ViewEditors.Add(frmNew);
                }
                else {
                    // error
                    frmNew.Close();
                }
            }
            // save openres value
            WinAGISettings.OpenNew = blnOpen;
            //if added to game
            if (blnInGame) {
                // unload the game resource
                EditGame.Views[tmpView.Number].Unload();
            }
            // restore main form mousepointer and exit
            MDIMain.UseWaitCursor = false;
        }

        public static void OpenLogic(byte ResNum, bool Quiet = false) {
            /*
            //this method opens a logic editor window

          short i;
            bool blnLoaded;
          bool blnWinOpen;
          frmLogicEdit frmNew;

          On Error GoTo ErrHandler

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //step through all logic editor windows
          For i = 1 To LogicEditors.Count
          //if window is a logic editor
          if (LogicEditors(i).FormMode = fmLogic) {
            //if logic number and ingame status matches
           if (LogicEditors(i).LogicNumber = ResNum && LogicEditors(i).InGame) {
              blnWinOpen = true
              //exit
              Exit For
            }
          }
          Next i

          //if found,
          if (blnWinOpen) {
          //file is already open in another window so set focus to it
          //if minimized,
          if (LogicEditors(i).WindowState = vbMinimized) {
            LogicEditors(i).WindowState = vbNormal
          }
          LogicEditors(i).Focus()

          //restore mousepointer and exit
          MDIMain.UseWaitCursor = false;
          return;
          }

          //open a new logic editing window
          frmNew = New frmLogicEdit

          //if the resource is not loaded,
          blnLoaded = Logics[ResNum].Loaded
          if (!blnLoaded) {
          Logics[ResNum].Load
          }

          //load the logic into the editor
          if (frmNew.EditLogic(Logics[ResNum])) {
          //show the form
          frmNew.Show

          //add to collection
          LogicEditors.Add frmNew

          } else {
          //error
          Unload frmNew
          frmNew = null
          }

          //unload if necessary
          if (!blnLoaded) {
          Logics[ResNum].Unload
          }

          //restore mousepointer and exit
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
            */
        }
        public static void OpenPicture(byte ResNum, bool Quiet = false) {
            //*//
            /*
            //this method opens a picture editor window

          short i;
            bool blnLoaded;
          bool blnWinOpen;
          frmPictureEdit frmNew;

          On Error GoTo ErrHandler

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //step through all Picture windows
          For i = 1 To PictureEditors.Count
          //if number matches
          if (PictureEditors(i).PicNumber = ResNum) {
            blnWinOpen = true
            Exit For
          }
          Next

          //if found,
          if (blnWinOpen) {
          //Picture is already open
          if (PictureEditors(i).WindowState = vbMinimized) {
            PictureEditors(i).WindowState = vbNormal
          }
          PictureEditors(i).Focus()
          //restore mousepointer and exit
          MDIMain.UseWaitCursor = false;
          return;
          }

          //open a new edit Picture form
          frmNew = New frmPictureEdit

          //if the resource is not loaded
          blnLoaded = Pictures[ResNum].Loaded
          if (!blnLoaded) {
          Pictures[ResNum].Load
          if (Err.Number != 0) {
           if (!Quiet) {
              ErrMsgBox("An error occurred while loading logic resource:", "", "Logic Load Error"
            }
            Err.Clear
            Unload frmNew
            //restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
            return;
          }
          }

          //load Picture into editor
          if (frmNew.EditPicture(Pictures[ResNum])) {
          //show the form
          frmNew.Show

          //add to collection
          PictureEditors.Add frmNew
          } else {
          //error
          Unload frmNew
          frmNew = null
          }

          //unload if necessary
          if (!blnLoaded) {
          Pictures[ResNum].Unload
          }

          //restore mousepointer and exit
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
            */
        }
        public static void OpenSound(byte ResNum, bool Quiet = false) {
            /*
            //this method opens a standard agi Sound editor window

          // if the passed resource is an Apple IIgs sound, just exit

          short i;
            bool blnLoaded;
          bool blnWinOpen;
          frmSoundEdit frmNew;

          On Error GoTo ErrHandler

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //step through all Sound editor windows
          For i = 1 To SoundEditors.Count
          //if Sound number matches
          if (SoundEditors(i).SoundNumber = ResNum) {
            blnWinOpen = true
            //exit
            Exit For
          }
          Next i

          //if found,
          if (blnWinOpen) {
          //file is already open in another window so set focus to it
          //if minimized,
          if (SoundEditors(i).WindowState = vbMinimized) {
            SoundEditors(i).WindowState = vbNormal
          }
          SoundEditors(i).Focus()

          //set mousepointer to hourglass
          MDIMain.UseWaitCursor = false;
          return;
          }

          //open a new Sound editing window
          frmNew = New frmSoundEdit

          //if the resource is not loaded,
          blnLoaded = Sounds[ResNum].Loaded
          if (!blnLoaded) {
          //load the Sound
          Sounds[ResNum].Load
          if (Err.Number != 0) {
           if (!Quiet) {
              ErrMsgBox("An error occurred while loading Sound resource:", "", "Load Sound Error"
            }
            Err.Clear
            Unload frmNew

            //set mousepointer to default
            MDIMain.UseWaitCursor = false;
            return;
          }
          }

          //check format
          if (Sounds[ResNum].SndFormat == 1) {
          //load the Sound into the editor
          if (frmNew.EditSound(Sounds[ResNum])) {
            //show the form
            frmNew.Show

            //add to collection
            SoundEditors.Add frmNew
          } else {
            //error
            Unload frmNew
            frmNew = null
          }
          } else {
          if (!Quiet) {
            MessageBox.Show("WinAGI does not currently support editing of Aplle IIgs sounds.", MessageBoxIcon.Information + MessageBoxButtons.OK, "Can//t Edit Apple IIgs Sounds"
          }
          }

          //unload if necessary
          if (!blnLoaded) {
          Sounds[ResNum].Unload
          }

          //reset main form mousepointer to default
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
            */
        }
        public static void OpenView(byte ResNum, bool Quiet = false) {
            //*//
            /*
          //this method opens a view editor window

          short i;
            bool blnLoaded;
          bool blnWinOpen;
          frmViewEdit frmNew;

          On Error GoTo ErrHandler

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //step through all view windows
          For i = 1 To ViewEditors.Count
          //if number matches
          if (ViewEditors(i).ViewNumber = ResNum) {
            blnWinOpen = true
            Exit For
          }
          Next

          //if found,
          if (blnWinOpen) {
          //view is already open
          if (ViewEditors(i).WindowState = vbMinimized) {
            ViewEditors(i).WindowState = vbNormal
          }
          ViewEditors(i).Focus()

          //restore mousepointer and exit
          MDIMain.UseWaitCursor = false;
          return;
          }

          //open a new edit view form
          frmNew = New frmViewEdit

          //if the resource is not loaded
          blnLoaded = Views[ResNum].Loaded
          if (!blnLoaded) {
          //use inline error handling
          On Error Resume Next
          //load the view
          Views[ResNum].Load
          if (Err.Number != 0) {
           if (!Quiet) {
              ErrMsgBox("An error occurred while loading view resource:", "", "Logic Load Error"
            }
            Err.Clear
            Unload frmNew
            //restore mousepointer and exit
            MDIMain.UseWaitCursor = false;
            return;
          }
          On Error GoTo ErrHandler
          }

          //load view into editor
          if (frmNew.EditView(Views[ResNum])) {
          //show form
          frmNew.Show

          //add to collection
          ViewEditors.Add frmNew
          } else {
          //error
          Unload frmNew
          frmNew = null
          }

          //unload if necessary
          if (!blnLoaded) {
          Views[ResNum].Unload
          }

          //restore mousepointer and exit
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
            */
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

            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.Picture];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["p" + PicNum]) {
                    // deselect the resource beforev removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["p" + PicNum].Remove();
                break;
            case agiSettings.EResListType.ComboList:
                //only need to remove if pictures are listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)AGIResType.Picture) {
                    MDIMain.lstResources.Items["p" + PicNum.ToString()].Remove();
                }
                break;
            }
            foreach (frmPicEdit frm in PictureEditors) {
                if (frm.InGame && frm.PicNumber == PicNum) {
                    //set number to -1 to force close
                    frm.PicNumber = -1;
                    //close it
                    frm.Close();
                    frm.Dispose();
                    break;
                }
            }

            //disposition any existing resource file
            if (File.Exists(strPicFile)) {
                KillCopyFile(strPicFile, WinAGISettings.RenameDelRes);
            }

            //update the logic tooltip lookup table
            IDefLookup[PicNum + 768].Name = "";
            IDefLookup[PicNum + 768].Value = "";
            IDefLookup[PicNum + 768].Type = atNone;
            //then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListDirty = true;
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

            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.Sound];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["s" + SoundNum]) {
                    // deselect the resource before removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["s" + SoundNum].Remove();
                break;
            case agiSettings.EResListType.ComboList:
                //only need to remove if sounds are listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)AGIResType.Sound) {
                    MDIMain.lstResources.Items["s" + SoundNum.ToString()].Remove();
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
                KillCopyFile(strSoundFile, WinAGISettings.RenameDelRes);
            }

            //update the logic tooltip lookup table
            IDefLookup[SoundNum + 512].Name = "";
            IDefLookup[SoundNum + 512].Value = "";
            IDefLookup[SoundNum + 512].Type = atNone;
            //then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListDirty = true;
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
            //remove it from game
            EditGame.Views.Remove(ViewNum);

            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.View];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["v" + ViewNum]) {
                    // deselect the resource before removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["v" + ViewNum].Remove();
                break;
            case agiSettings.EResListType.ComboList:
                //only need to remove if views are listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)AGIResType.View) {
                    MDIMain.lstResources.Items["v" + ViewNum.ToString()].Remove();
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
                KillCopyFile(strViewFile, WinAGISettings.RenameDelRes);
            }

            //update the logic tooltip lookup table
            IDefLookup[ViewNum + 256].Name = "";
            IDefLookup[ViewNum + 256].Value = "";
            IDefLookup[ViewNum + 256].Type = atNone;
            //then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListDirty = true;
            }
        }

        public static string LogTemplateText(string NewID, string NewDescription) {
            string strLogic = "";
            bool blnNoFile = false;
            //first, get the default file, if there is one
            if (File.Exists(ProgramDir + "deflog.txt")) {
                try {
                    using FileStream fsLogTempl = new(ProgramDir + "deflog.txt", FileMode.Open);
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
                strLogic = strLogic.Replace("~", MultStr(" ", WinAGISettings.LogicTabWidth));
                //id:
                strLogic = strLogic.Replace("%id", NewID);
                //description
                strLogic = strLogic.Replace("%desc", NewDescription);

                //horizon is a PicTest setting, which should always be retrieved everytime
                //it is used to make sure it's current
                strLogic = strLogic.Replace("%h", WinAGISettingsList.GetSetting(sPICTEST, "Horizon", DEFAULT_PICTEST_HORIZON).ToString());

                //if using reserved names, insert them
                if (LogicCompiler.UseReservedNames) {
                    //f5, v0, f2, f4, v9
                    strLogic = strLogic.Replace("f5", LogicCompiler.ReservedDefines(atFlag)[5].Name);
                    strLogic = strLogic.Replace("f2", LogicCompiler.ReservedDefines(atFlag)[2].Name);
                    strLogic = strLogic.Replace("f4", LogicCompiler.ReservedDefines(atFlag)[4].Name);
                    strLogic = strLogic.Replace("v0", LogicCompiler.ReservedDefines(atVar)[0].Name);
                    strLogic = strLogic.Replace("v9", LogicCompiler.ReservedDefines(atVar)[9].Name);
                }
            }
            catch (Exception) {
                //ignore errors return whatever is left
            }
            //return the formatted text
            return strLogic;
        }

        internal static void FindInLogic(string FindText, FindDirection FindDir, bool MatchWord, bool MatchCase, FindLocation LogicLoc, bool Replacing = false, string ReplaceText = "") {
            //*//
            /*
          //logic search strategy:
          //
          //determine current starting position; can be in a logic currently being edited
          //or from the globals, words or objects editor
          //
          //if from a logic editor, begin search at current position in current editor
          //if from a non-logic editor, begin search in closed logics
          //(note that findlocation will ALWAYS be flAll in this case)
          //
          //if this is a new search, set start logic and start pos
          //if this is NOT a new search, continue where left off
          //if search gets all the way back to beginning, stop

          //source values:
          // 0 = logic or text editor
          // 1 = main window/preview (no search form)
          // 2 = object editor
          // 3 = word editor

          int FoundPos;
          int SearchPos;
            bool blnReplaced, blnSelMatch;
          bool BeginClosedLogics, blnNewSearch;
          int lngNextLogWin, lngFirstLogWin;
          int lngLogWinCount, lngSearchSource;
          StringComparison vbcComp;
          int i;
            bool blnFrmVis;
          DialogResult rtn;
           int lngCheck;
          int lngPossFind;
          bool blnSkipEd;
          int lngTopLine, lngThisLine;
          POINTAPI pPos;
           int lngBtmLine;

          On Error GoTo ErrHandler

          MainStatusBar.Panels(1).Text = ""

          //set comparison method for string search,
          vbcComp = CLng(MatchCase) + 1 // CLng(true) + 1 = 0 = vbBinaryCompare; Clng(false) + 1 = 1 = StringComparison.OrdinalIgnoreCase

          //if replacing and new text is the same
          if (Replacing && (StrComp(FindText, ReplaceText, vbcComp) = 0)) {
          //exit
          return;
          }

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          switch (SearchForm.Name) {
          case "frmLogicEdit", "frmTextEdit"
          lngSearchSource = 0
          case "frmMDIMain"
          lngSearchSource = 1
          case "frmObjectEdit"
          lngSearchSource = 2
          case "frmWordsEdit"
          lngSearchSource = 3
          case "frmGlobals"
          lngSearchSource = 4
          default:
          //Debug.Assert false
          }

          switch (lngSearchSource
          case 0
          //if starting from a logic or text, determine form
          //number of starting logic
          lngLogWinCount = LogicEditors.Count
          For i = 1 To lngLogWinCount
           if (LogicEditors(i) == SearchForm) {
              lngNextLogWin = i
              Exit For
            }
          Next i

          //set first logic to current logic
          lngFirstLogWin = lngNextLogWin

          //does selection match search term
          blnSelMatch = (StrComp(LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.Text, FindText, vbcComp) = 0)

          //if replacing, we need to first check the current selection
          if (Replacing) {
            With LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range
              //is searchword currently selected?
             if (blnSelMatch) {
                //selection is highlighted with search text; just reset it
                .Text = ReplaceText
                // set flag so we know to update starting search position
                blnReplaced = true
                //the richtext box handles undo automatically

                //need to re-apply context highlighting
                i = SendMessage(LogicEditors(lngNextLogWin).rtfLogic.hWnd, EM_LINEFROMCHAR, .StartPos, 0)
                LogicEditors(lngNextLogWin).rtfLogic.RefreshHighlight i, i
              }
            End With
          }

          //if starting position not set, it means starting a new search
          // set startpos and start logic
          if (SearchStartPos = -1) {
            blnNewSearch = true

            //save this logic and position to support find again
            SearchStartLog = lngFirstLogWin

            //if searching up
           if (FindDir = fdUp) {
              //always use current location
              SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos + 1
            } else {
              //searching down or all both use the same strategy

              // if something was replaced, search starts one character before
              // start of selection;
              // if nothing replaced, start depends on whether something is
              // selected - if nothing selected, search starts one character
              // before cursor (in case cursor is right in front of the
              // word being searched for); if there is a selection, search
              // starts at cursor location if selection doesn't match search
              // text; if selected text matches, then the saved search start
              // is set to current location, but the current search start has
              // to start one character AFTER search start AND sets FirstFind
              // flag; (that makes the selection count as the //first find//,
              // and this trip through the function will then look for the
              //next instance

              //(don't forget to adjust by +1)
             if (blnReplaced) {
                SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos
              } else {
                //if nothing selected
               if (LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.Length = 0) {
                  //start in front of current selection
                  SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos
                } else {
                  //does selection match?
                 if (blnSelMatch) {
                    // use current cursor position
                    SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos + 1
                    // and set firstfind flag
                    FirstFind = true
                  } else {
                    // use current cursor position-1
                    SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos //+ 1
                  }
                }
              }
            }
          }

          //intial SearchPos also depends on direction
          if (FindDir = fdUp) {
            // if a new search
           if (blnNewSearch) {
              // need to adjust position in case FindText is only partially in front of start point
              SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos + Len(FindText) - 1
            } else {
              SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos
            }

            //if at start of text box
           if (SearchPos <= 0) {
              //reset to end
              SearchPos = -1
            }
          } else {
            //when searching forward, add one because instr and instrrev are //1//
            //based, but textbox positions are //0// based

            // if something just got replaced
           if (blnReplaced) {
              //use end of selection
              SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.EndPos + 1
            } else {
              //if selection matches, and this is a new search
             if (blnSelMatch && blnNewSearch) {
                //increment saved start position
                SearchPos = SearchStartPos + 1
              } else {
                //selection doesn't match; if this is a new search
               if (blnNewSearch) {
                  //use beginning of selection
                  SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos + 1
                } else {
                  //otherwise, use end of selection
                  SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.EndPos + 1
                }
              }
            }
          }

          case 1, 2, 3, 4
          //no distinction (yet) between words, objects, resIDs, globals
          //Debug.Assert FindDir = fdAll
          //Debug.Assert LogicLoc = flAll

          // if no logic editors are open
          if (LogicEditors.Count = 0) {
            //begin search in closed logics

            //first disable main form
            MDIMain.Enabled = false
            //set closelogics flag to false
            ClosedLogics = false
            //get number of next logic
            lngNextLogWin = FindInClosedLogics(FindText, FindDir, MatchWord, MatchCase, SearchType)
            //reenable form
            MDIMain.Enabled = true
            //if nothing found (lngnextlogwin =-1)
           if (lngNextLogWin = -1) {
              //show msgbox and exit
              MessageBox.Show("Search text not found.", MessageBoxIcon.Information, "Find in Logic"
              //restore cursor
              MDIMain.UseWaitCursor = false;
              return;
            }

            //this editor is only one open
            lngFirstLogWin = 1
            lngNextLogWin = 1
            SearchStartLog = 1
          } else {
            //start the current logic editor
            lngFirstLogWin = 1
            lngNextLogWin = 1
            SearchStartLog = 1
          }

          //always start the search at beginning of the logic
          SearchStartPos = 1
          SearchPos = 1

          default:
          //Debug.Assert false
          }

          //main search routine; at this point, a logic editor is open and available for searching
          Do
          //just in case we get stuck in a loop!
          lngCheck = lngCheck + 1

          //only do a search if not STARTING a closed logic search or not doing
          //CONTINUING a closed logic search
          if (!BeginClosedLogics || !ClosedLogics) {
            //reset the skip editor flag
            blnSkipEd = false

            //if all logics, skip any text editors or non ingame logics
           if (LogicLoc = flAll) {
             if (LogicEditors(lngNextLogWin).Name = "frmTextEdit") {
                //skip it
                blnSkipEd = true
              } else if ( LogicEditors(lngNextLogWin).Name = "frmLogicEdit") {
               if (!LogicEditors(lngNextLogWin).InGame) {
                  //skip it
                  blnSkipEd = true
                }
              }
            }

           if (blnSkipEd) {
              // set result to //nothing found//
              FoundPos = 0
            } else {
              // search the target logic, from the starting search position

              //if direction is up,
             if (FindDir = fdUp) {
                //if searching whole word
               if (MatchWord) {
                  FoundPos = FindWholeWord(SearchPos, LogicEditors(lngNextLogWin).rtfLogic.Text, FindText, MatchCase, FindDir, SearchType)
                } else {
                  //use instrrev
                  FoundPos = InStrRev(LogicEditors(lngNextLogWin).rtfLogic.Text, FindText, SearchPos, vbcComp)
                }
                // always reset SearchPos
                SearchPos = -1
              } else {
                //search strategy depends on synonym search value
               if (!GFindSynonym) {
                  //if searching whole word
                 if (MatchWord) {
                    FoundPos = FindWholeWord(SearchPos, LogicEditors(lngNextLogWin).rtfLogic.Text, FindText, MatchCase, FindDir = fdUp, SearchType)
                  } else {
                    //use instr
                    FoundPos = InStr(SearchPos, LogicEditors(lngNextLogWin).rtfLogic.Text, FindText, vbcComp)
                  }
                } else {
                  //Matchword is always true; but since words are surrounded by quotes, it wont matter
                  //so we use Instr

                  //step through each word in the word group; if the word is found in this logic,
                  //check if it occurs before the current found position
                  FoundPos = 0
                  For i = 0 To WordEditor.WordsEdit.GroupN(GFindGrpNum).WordCount - 1
                    lngPossFind = InStr(SearchPos, LogicEditors(lngNextLogWin).rtfLogic.Text, QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).Word(i) + QUOTECHAR, vbcComp)
                   if (lngPossFind > 0) {
                     if (FoundPos = 0) {
                        FoundPos = lngPossFind
                        FindText = QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).Word(i) + QUOTECHAR
                      } else {
                       if (lngPossFind < FoundPos) {
                          FoundPos = lngPossFind
                          FindText = QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).Word(i) + QUOTECHAR
                        }
                      }
                    }
                  Next i
                }
                // always reset SearchPos
                SearchPos = 1
              }
            }
          }

          //if found,
          if (FoundPos > 0) {
            //if searching forward or all
           if (FindDir = fdAll || FindDir = fdDown) {
              //if back at search start (whether anything found or not)
              // OR PAST search start(after previously finding something)
             if (((FoundPos = SearchStartPos) && (lngNextLogWin = SearchStartLog)) || _
                 ((FoundPos > SearchStartPos) && (lngNextLogWin = SearchStartLog) && RestartSearch)) {
                //if NOT searching all logics
               if (LogicLoc != flAll) {
                  //back at start; reset foundpos
                  FoundPos = 0
                  //exit loop
                  break;
                } else {
                  //set flag to begin searching closed logics
                  BeginClosedLogics = true
                  //and also reset the actual closelogic search flag
                  ClosedLogics = false
                }
              } else {
                //exit loop; search text found
                break;
              }
            } else {
              //searching up: if atstarting point exactly, or if above starting point
              // and reset flag is set
             if (((FoundPos = SearchStartPos) && (lngNextLogWin = SearchStartLog)) || _
                 ((FoundPos < SearchStartPos) && (lngNextLogWin = SearchStartLog) && RestartSearch)) {
                //if NOT searching all logics
               if (LogicLoc != flAll) {
                  //back at start; reset foundpos
                  FoundPos = 0
                  //exit loop
                  break;
                } else {
                  //set flag to begin searching closed logics
                  BeginClosedLogics = true
                  //and also reset the actual closelogic search flag
                  ClosedLogics = false
                }
              } else {
                //exit loop; search text found
                break;
              }
            }
          }

          //if not found, action depends on search mode
          switch (LogicLoc
          case flCurrent
            //ask if user wants to continue search for up or down search
            switch (FindDir
            case fdUp
              //if nothing was found yet
             if (!RestartSearch) {
                rtn = MessageBox.Show("Beginning of search scope reached. Do you want to continue from the end?", MessageBoxIcon.Question + MessageBoxButtons.YesNo, "Find in Logic")
               if (rtn = DialogResult.No) {
                  //reset cursor
                  MDIMain.UseWaitCursor = false;
                  return;
                }
              } else {
                // if restartsearch is true, it means this is second time through;
                // since nothing found, just exit the loop
                break;
              }

            case fdDown
              //if nothing found yet
             if (!RestartSearch) {
                rtn = MessageBox.Show("End of search scope reached. Do you want to continue from the beginning?", MessageBoxIcon.Question + MessageBoxButtons.YesNo, "Find in Logic")
               if (rtn = DialogResult.No) {
                  //reset cursor
                  MDIMain.UseWaitCursor = false;
                  return;
                }
              } else {
                // if resetsearch is true, means this is second time through;
                // since nothing found, just exit the loop
                break;
              }

            case fdAll
              //if restartsearch is true, means this is second time through;
              // since nothing found, just exit the loop
             if (RestartSearch) {
                //not found; exit
                break;
              }
            }

          case flOpen
            //if back on start, and search already reset
           if ((lngNextLogWin = SearchStartLog) && RestartSearch) {
              //not found- exit
              break;
            }

           //increment logic number
            lngNextLogWin = lngNextLogWin + 1
           if (lngNextLogWin > LogicEditors.Count) {
              lngNextLogWin = 1
            }

          case flAll

            //since nothing found in this logic, try the next

            // if closed logics need to start, or already searching closed logics
           if (BeginClosedLogics || ClosedLogics) {

              //first disable main form
              MDIMain.Enabled = false
              //get number of next logic
              lngNextLogWin = FindInClosedLogics(FindText, FindDir, MatchWord, MatchCase, SearchType)
              // clear the start-closed-logic-search flag
              BeginClosedLogics = false

              //reenable main form
              MDIMain.Enabled = true
              //if nothing found (lngnextlogwin =-1 or -2)
             if (lngNextLogWin < 0) {
                FoundPos = 0
                break;
              }

              // if search started in editor (by pressing F3 or using menu option)
             if (!SearchStartDlg) {
                // select the newly opened logic
                LogicEditors(lngNextLogWin).Focus()
              }
            } else {
              // NOT starting or continuing a closed logic search

              //if back to starting logic, and search already reset
             if ((lngNextLogWin = SearchStartLog) && RestartSearch) {
                //not found- set flag to search closed logics
                BeginClosedLogics = true
              } else {
                //not back at original start pos, so try the next open logic
                //increment logic number
                lngNextLogWin = lngNextLogWin + 1
               if (lngNextLogWin > LogicEditors.Count) {
                  lngNextLogWin = 1
                }
              }
            }
          }

            //set reset search flag so when we are back to starting logic,
            // the search will end
            RestartSearch = true

          //loop is exited by finding the searchtext or reaching end of search area
          Loop Until lngCheck >= 256

          //if exited by exceeding the count, something went wrong
          if (lngCheck >= 256) {
          //Debug.Assert false
          }

          //if search string was found,
          if (FoundPos > 0) {
          //if first found word in a new search
          if (!FirstFind) {
            //set firstfind to true
            FirstFind = true
          }

          //highlight searchtext and show editor
          With LogicEditors(lngNextLogWin).rtfLogic

            .Selection.Range.StartPos = FoundPos - 1
            .Selection.Range.EndPos = FoundPos - 1 + Len(FindText)

            //determine top, bottom and current line numbers
            lngTopLine = SendMessage(.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
            lngThisLine = SendMessage(.hWnd, EM_LINEFROMCHAR, FoundPos, 0)
            //find bottom line in steps
            .GetViewRect i, rtn, pPos.X, pPos.Y
            rtn = SendMessagePtL(.hWnd, EM_CHARFROMPOS, 0, pPos)
            lngBtmLine = SendMessage(.hWnd, EM_LINEFROMCHAR, rtn, 0)
            // if line NOT more than one above bottom, scroll up
           if (lngBtmLine - lngThisLine < 1) {
              //scroll so this line is four lines above bottom
              //get currently visible first line
              //determine amount of scrolling to do
              rtn = SendMessage(.hWnd, EM_LINESCROLL, 0, 4 - (lngBtmLine - lngThisLine))
            }
            //refresh the screen
            .Refresh
          End With

          //bring the selected window to the top of the order (restore if minimized)
          if (LogicEditors(lngNextLogWin).WindowState = vbMinimized) {
            LogicEditors(lngNextLogWin).WindowState = vbNormal
          }
          //if search was started from the editor (by pressing F3 or using menu option)
          if (!SearchStartDlg) {
            //set focus to the editor
            LogicEditors(lngNextLogWin).Focus()
            // and then force focus to the rtf control
            LogicEditors(lngNextLogWin).rtfLogic.Focus()

            //force the form to activate, in case we need to add a statusbar update
            SafeDoEvents
          } else {
            //when searching from the dialog, make sure the logic is at top of zorder, but
            //don't need to give it focus
            LogicEditors(lngNextLogWin).ZOrder
          }

          //if a synonym was found, note it on status bar
          if (GFindSynonym) {
           if (FindText != QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).GroupName + QUOTECHAR) {
              MainStatusBar.Panels(1).Text = FindText + " is a synonym for " + QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).GroupName + QUOTECHAR
              //flash the status bar
              MDIMain.tmrFlash.Enabled = true
            }
          }

          //this form is now the search form
          SearchForm = LogicEditors(lngNextLogWin)

          } else {  //search string was NOT found (or couldn//t open a window)


          // if not opened due to anything else
          if (lngNextLogWin != -2) {

            //if something previously found (firstfind=true)
           if (FirstFind) {
              //search complete; no new instances found
              blnFrmVis = FindingForm.Visible
             if (blnFrmVis) {
                FindingForm.Visible = false
              }
              MessageBox.Show("The specified region has been searched. No more matches found.", MessageBoxIcon.Information, "Find in Logic"
             if (blnFrmVis) {
                FindingForm.Visible = true
              }
            } else {
              blnFrmVis = FindingForm.Visible
             if (blnFrmVis) {
                FindingForm.Visible = false
              }
              //show not found msg
              MessageBox.Show("Search text not found.", MessageBoxIcon.Information, "Find in Logic"
             if (blnFrmVis) {
                FindingForm.Visible = true
              }
            }

            //restore focus to correct form
           if (SearchStartDlg) {
              //assume it's visible
              //Debug.Assert FindingForm.Visible
              //it's already got focus
            } else {
              //set focus to searchform
             if (SearchForm is not null) {
                SearchForm.Focus()
              }
            }
          }

          //reset search flags
          FindingForm.ResetSearch
          }

          //reset cursor
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
            */
        }
        public static void KillCopyFile(string ResFile, bool KeepOld) {
            string strOldName;
            int lngNextNum;
            string strName, strExt;

            // ignore any errors - if it deletes, that's great; if not
            // we don't really care...
            try {
                if (File.Exists(ResFile)) {
                    if (KeepOld) {
                        strName =Path.GetDirectoryName(ResFile) + "\\" + Path.GetFileNameWithoutExtension(ResFile);
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
                DefaultColors[i] = WinAGISettingsList.GetSetting(sDEFCOLORS, "DefEGAColor" + i, DefaultColors[i]);
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
            if (WinAGISettings.ShowResNum && IsInGame) {
                switch (WinAGISettings.ResFormat.NameCase) {
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
                return retval + WinAGISettings.ResFormat.Separator + ThisResource.Number.ToString(WinAGISettings.ResFormat.NumFormat);
            }
            else {
                if (WinAGISettings.IncludeResNum && IsInGame && !NoNumber) {
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
        public static void ShowAGIBitmap(PictureBox pic, Bitmap agiBMP, int tgtX, int tgtY, int tgtW, int tgtH, InterpolationMode mode = InterpolationMode.NearestNeighbor) {
            // draws the agi bitmap in target picture box using passed target size/location

            //to scale the picture without blurring, need to use NearestNeighbor interpolation
            // that can't be set directly, so a graphics object is needed to draw the the picture
            int bWidth, bHeight;
            bWidth = pic.Width;
            bHeight = pic.Height;
            // first, create new image in the picture box that is desired size
            pic.Image = new Bitmap(bWidth, bHeight);
            // intialize a graphics object for the image just created
            using Graphics g = Graphics.FromImage(pic.Image);
            //always clear the background first
            g.Clear(pic.BackColor);
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
                                     AGIResType SearchType = None) {
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
                    lngPos = strText.LastIndexOf(strFind, lngPos, lngPos + 1, StringCompare);
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

        public static void RemoveLogic(byte LogicNum) {
            // removes a logic from the game, and updates
            // preview and resource windows
            //
            // it also updates layout editor, if it is open
            // and deletes the source code file from source directory

            bool blnIsRoom;

            // need to load logic to access sourccode
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
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)AGIResType.Logic];
                if (MDIMain.tvwResources.SelectedNode == tmpNode.Nodes["l" + LogicNum]) {
                    // deselect the resource beforev removing it
                    SelResNum = -1;
                }
                tmpNode.Nodes["l" + LogicNum].Remove();
                break;
            case agiSettings.EResListType.ComboList:
                // only update if the resource type is being listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)AGIResType.Logic) {
                    MDIMain.lstResources.Items["l" + LogicNum.ToString()].Remove();
                }
                break;
            }
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == ELogicFormMode.fmLogic) {
                    if (frm.InGame && frm.LogicNumber == LogicNum) {
                        // set number to -1 to force close
                        frm.LogicNumber = -1;
                        //close it
                        frm.Close();
                        frm.Dispose();
                        break;
                    }
                }
            }
            // disposition any existing resource file
            if (File.Exists(strSourceFile)) {
                KillCopyFile(strSourceFile, WinAGISettings.RenameDelRes);
            }
            // update the logic tooltip lookup table
            IDefLookup[LogicNum].Name = "";
            IDefLookup[LogicNum].Type = atNone;
            // then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListDirty = true;
            }
        }

        public static bool CheckLogics() {
            // checks all logics; if any found that are dirty
            // allow user to recompile game if desired before
            // running
            bool retval = true;
            // if not requiring recompile
            if (WinAGISettings.CompileOnRun == 1) {
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
            // if no dirty logics found, check any existing logics that are being edited
            if (retval) {
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.FormMode == ELogicFormMode.fmLogic) {
                        if (frm.rtfLogic.IsChanged == true) {
                            // one dirty logic found
                            retval = false;
                            break;
                        }
                    }
                }
            }
            // if still no dirty logics found
            if (retval) {
                // just exit
                return true;
            }
            DialogResult rtn = DialogResult.Cancel;
            bool blnDontAsk = false;
            // if CompileOnRun is in ask mode or yes mode, get user choice
            switch (WinAGISettings.CompileOnRun) {
            case 0:
                // ask for user input
                rtn = MsgBoxEx.Show(MDIMain,
                    "One or more logics have changed since you last compiled.\n\n" +
                    "Do you want to compile them before running?",
                    "Compile Before Running?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    "Always take this action when compiling a game.", ref blnDontAsk);
                if (blnDontAsk) {
                    if (rtn == DialogResult.Yes) {
                        WinAGISettings.CompileOnRun = 2;
                    }
                    else if (rtn == DialogResult.No) {
                        WinAGISettings.CompileOnRun = 1;
                    }
                    // update settings list
                    WinAGISettingsList.WriteSetting(sLOGICS, "CompOnRun", WinAGISettings.CompileOnRun);
                }
                break;
            case 1:
                rtn = DialogResult.No;
                break;
            case 2:
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
            return CompileDirtyLogics(true);
        }
    }
}
