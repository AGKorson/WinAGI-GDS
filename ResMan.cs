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
using static WinAGI.Editor.BkgdTasks;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;

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
        public const bool DEFAULT_SKIPPRINTWARNING = false;
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
        public const int DEFAULT_RESLISTTYPE = 1; //treeview
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
        public static int PropSplitLoc;

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
        public enum UpdateModeType {
            umResList = 1,
            umProperty = 2,
            umPreview = 4,
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
            public bool SkipPrintWarning;  //if true, no warning shown if no printers found at startup
            public bool ShowPreview;  //
            public bool ShiftPreview;  // //brings preview window to front when something selected
            public bool HidePreview;  //
            public int ResListType;  //0=no tree; 1=treelist; 2=combo/list boxes
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
            public bool RenameDelRes;  //
            public int MaxSO;  //
            public int MaxVol0Size;
            public tResource ResFormat;
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
        public struct LoadGameResults {
            public int Mode;
            public string Source;
            public bool Warnings;
            public bool Failed;
            public string ErrorMsg;
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
        public static string CurGameFile;
        public static LoadGameResults LoadResults;
        public static AGIResType CurResType;
        public static agiSettings Settings;
        public static SettingsList GameSettings;
        public static frmPreview PreviewWin;
        public static frmProgress ProgressWin;
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
        public static int ScreenTWIPSX;
        public static int ScreenTWIPSY;
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
        public static int DefUpdateVal;
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
        public static IntPtr HelpParent;
        public static string TempFileDir;
        public static string BrowserStartDir = "";
        //printer variables
        public static bool NoPrinter;
        public static int PMLeft, PMTop;
        public static int PMRight, PMBottom;
        public static int PHeight, PWidth;
        public static int PDPIx, PDPIy;
        public static bool NoColor;
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
            intCount = GameSettings.GetSetting("ResDefOverrides", "Count", 0);
            if (intCount == 0) {
                return;
            }
            //ok, get the overrides, and apply them
            for (i = 1; i <= intCount; i++) {
                strIn = GameSettings.GetSetting("ResDefOverrides", "Override" + i, "");
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
            GameSettings.DeleteSection("ResDefOverrides");
            //now step through each type of define value; if name is not the default, then save it
            for (ResDefGroup grp = 0; (int)grp < 10; grp++) {
                dfTemp = LogicCompiler.ResDefByGrp(grp);
                for (i = 0; i < dfTemp.Length; i++) {
                    if (dfTemp[i].Default != dfTemp[i].Name) {
                        //save it
                        intCount++;
                        GameSettings.WriteSetting("ResDefOverrides", "Override" + intCount, (int)grp + ":" + i + ":" + dfTemp[i].Name);
                    }
                }
            }
            //write the count value
            GameSettings.WriteSetting("ResDefOverrides", "Count", intCount.ToString());
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
            Settings.EFontName = DEFAULT_EFONTNAME;
            Settings.PFontName = DEFAULT_PFONTNAME;
            // initialize settings arrays
            Settings.HBold = new bool[5];
            Settings.HItalic = new bool[5];
            Settings.HColor = new Color[6];
            //default gif options
            VGOptions.Cycle = true;
            VGOptions.Transparency = true;
            VGOptions.Zoom = 2;
            VGOptions.Delay = 15;
            VGOptions.HAlign = 0;
            VGOptions.VAlign = 1;
            //default value for updating logics is //checked//
            DefUpdateVal = 1;
            //initialize clipboard object if not already done
            GlobalsClipboard = [];
            //initialize code snippet array
            CodeSnippets = [];
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
                //if savefile exists
                if (File.Exists(ExportFile)) {
                    //delete it
                    File.Delete(ExportFile);
                }
                //copy tempfile to savefile
                File.Move(strTempFile, ExportFile);
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
                if (File.Exists(ExportFile)) {
                    try {
                        File.Delete(ExportFile);
                    }
                    catch (Exception) {
                        //ignore
                    }
                }
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
            //if no game file passed,
            if (ThisGameFile.Length == 0) {
                //get a wag file to open
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

            //now open this game file
            if (OpenGame(0, ThisGameFile)) {
                MessageBox.Show("Game opened. Check it for errors or warnings.");
                //MDIHasFocus = true;
                //tvwResources.Focus();
            }
            else {
                MessageBox.Show("did not load; not sure why");
                //if (retval == WINAGI_ERR + 636) {
                //  MessageBox.Show("Game opened, with warnings.");
                //}
                //else {
                //  MessageBox.Show($"opengame result: {Base.LoadResString(retval - WINAGI_ERR)}");
                //}
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
                if (!OpenGame(1, ThisGameDir)) {
                    // user cancelled closing of currently open game
                    return;
                }
                // check for error
                if (EditGame is null) {
                    return;
                }

                //set default directory
                BrowserStartDir = EditGame.GameDir;

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

            //if a game is currently open,
            if (EditGame is not null) {
                //close game, if user allows
                if (!CloseThisGame()) {
                    return false;
                }
            }

            //show wait cursor
            MDIMain.UseWaitCursor = true;
            MDIMain.Refresh();
            //show the progress window
            ProgressWin = new frmProgress {
                Text = "Loading Game"
            };
            ProgressWin.lblProgress.Text = "Checking WinAGI Game file ...";
            ProgressWin.StartPosition = FormStartPosition.CenterParent;
            ProgressWin.pgbStatus.Visible = false;
            //show loading msg in status bar
            MainStatusBar.Items[1].Text = (mode == 0 ? "Loading" : "Importing") + " game; please wait...";

            //set up the background worker to open the game
            bgwOpenGame = new BackgroundWorker();
            bgwOpenGame.DoWork += new DoWorkEventHandler(OpenGameBkgd);
            bgwOpenGame.ProgressChanged += new ProgressChangedEventHandler(bgw_ProgressChanged);
            bgwOpenGame.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
            bgwOpenGame.WorkerReportsProgress = true;
            // pass mode and source
            LoadResults = new() {
                Mode = mode,
                Source = gameSource,
                Failed = false,
                ErrorMsg = "",
                Warnings = false
            };
            bgwOpenGame.RunWorkerAsync(LoadResults);
            try {
                // now show progress form
                ProgressWin.ShowDialog(MDIMain);
            }
            catch (Exception e) {
                Debug.Assert(false);
            }
            // done with the background worker
            bgwOpenGame.Dispose();
            bgwOpenGame = null;
            //reset cursor
            MDIMain.UseWaitCursor = false;

            //add wag file to mru, if opened successfully
            if (EditGame is not null) {
                //attach  AGI game events
                EditGame.LoadGameStatus += MDIMain.GameEvents_LoadGameStatus;
                EditGame.CompileGameStatus += MDIMain.GameEvents_CompileGameStatus;
                EditGame.CompileLogicStatus += MDIMain.GameEvents_CompileLogicStatus;
                EditGame.DecodeLogicStatus += MDIMain.GameEvents_DecodeLogicStatus;

                AddToMRU(EditGame.GameFile);
            }
            else {
                //make sure warning grid is hidden
                if (MDIMain.pnlWarnings.Visible) {
                    MDIMain.HideWarningList(true);
                }
            }

            //clear status bar
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
            if (Settings.ShowPreview) {
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
            if (Settings.ResListType != 0) {
                MDIMain.HideResTree();
                MDIMain.ClearResourceList();
            }
            if (Settings.ShowPreview) {
                PreviewWin.Visible = false;
            }
            // now close the game
            // TODO: will a dispose function help???
            EditGame.CloseGame();
            //detach  AGI game events
            EditGame.LoadGameStatus -= MDIMain.GameEvents_LoadGameStatus;
            EditGame.CompileGameStatus -= MDIMain.GameEvents_CompileGameStatus;
            EditGame.CompileLogicStatus -= MDIMain.GameEvents_CompileLogicStatus;
            EditGame.DecodeLogicStatus -= MDIMain.GameEvents_DecodeLogicStatus;

            EditGame = null;
            // restore colors to AGI default when a game closes
            GetDefaultColors();
            // restore default resdef
            LogicDecoder.UseReservedNames = Settings.DefUseResDef;
            // update main form caption
            MDIMain.Text = "WinAGI GDS";
            // reset node marker so selection of resources
            // works correctly first time after another game loaded
            MDIMain.LastNodeName = "";
            // reset default text location to program dir
            DefaultResDir = ProgramDir;
            // game is closed
            return true;
        }

        public static void BuildResourceTree() {
            // builds the resource tree list
            // for the current open game
            int i;
            TreeNode tmpNode;

            switch (Settings.ResListType) {
            case 0:
                // no tree
                return;
            case 1:
                // treeview list
                if (EditGame.GameID.Length != 0) {
                    //update root
                    MDIMain.tvwResources.Nodes[0].Text = EditGame.GameID;
                    //add logics
                    if (EditGame.Logics.Count > 0) {
                        for (i = 0; i <= 255; i++) {
                            //if a valid resource
                            if (EditGame.Logics.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sLOGICS].Nodes.Add("l" + i, ResourceName(EditGame.Logics[(byte)i], true));
                                tmpNode.Tag = i;
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
                        for (i = 0; i <= 255; i++) {
                            //if a valid resource
                            if (EditGame.Pictures.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sPICTURES].Nodes.Add("p" + i, ResourceName(EditGame.Pictures[(byte)i], true));
                                tmpNode.Tag = i;
                                tmpNode.ForeColor = EditGame.Pictures[(byte)i].ErrLevel >= 0 ? Color.Black : Color.Red;
                            }
                        }
                    }
                    if (EditGame.Sounds.Count > 0) {
                        for (i = 0; i <= 255; i++) {
                            //if a valid resource
                            if (EditGame.Sounds.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sSOUNDS].Nodes.Add("s" + i, ResourceName(EditGame.Sounds[(byte)i], true));
                                tmpNode.Tag = i;
                                tmpNode.ForeColor = EditGame.Sounds[(byte)i].ErrLevel >= 0 ? Color.Black : Color.Red;
                            }
                        }
                    }
                    if (EditGame.Views.Count > 0) {
                        for (i = 0; i <= 255; i++) {
                            //if a valid resource
                            if (EditGame.Views.Contains((byte)i)) {
                                tmpNode = MDIMain.tvwResources.Nodes[0].Nodes[sVIEWS].Nodes.Add("v" + i, ResourceName(EditGame.Views[(byte)i], true));
                                tmpNode.Tag = i;
                                tmpNode.ForeColor = EditGame.Views[(byte)i].ErrLevel >= 0 ? Color.Black : Color.Red;
                            }
                        }
                    }
                }
                break;
            case 2:
                //combo/list boxes
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
                    RDefLookup[pos] = LogicCompiler.ResDefByGrp(grp)[i];
                    pos++;
                }
            }
            // then add game specific resdefs, if a game is loaded
            //TODO: if this is only called when mainform loads, then
            // game will never be loaded
            if (EditGame is not null) {
                for (int i = 0; i < 4; i++) {
                    RDefLookup[pos] = EditGame.ReservedGameDefines[i];
                    pos++;
                }
            }
            //then let open logic editors know
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
            retval = retval.Replace("%t", new String(' ', Settings.LogicTabWidth));

            //lastly, restore any forced percent signs
            retval = retval.Replace((char)255, '%');
            return retval;
        }
        public static void OpenMRUGame(int Index) {
            int i;

            //skip if this MRU is blank (probably due to user manually editing
            //the config file)
            if (strMRU[Index].Length == 0) {
                // TODO: if entry is blank, but there's text on the menu bar, should probably inform user
                return;
            }
            //attempt to open this game
            if (OpenGame(0, strMRU[Index])) {
                //reset browser start dir to this dir
                BrowserStartDir = JustPath(strMRU[Index]);
            }
            else {
                Debug.Assert(EditGame is null);
                //Debug.Assert(!EditGame.GameLoaded);
                //step through rest of mru entries
                for (i = Index + 1; i < 4; i++) {
                    //move this mru entry up
                    strMRU[i - 1] = strMRU[i];
                    //if blank
                    if (strMRU[i - 1].Length == 0) {
                        //hide this mru item
                        MDIMain.mnuGame.DropDownItems["mnuGMRU" + (i - 1)].Visible = false;
                    }
                    else {
                        //change this mru item
                        MDIMain.mnuGame.DropDownItems["mnuGMRU" + (i - 1)].Text = CompactPath(strMRU[i], 60);
                    }
                }
                //remove last entry
                strMRU[3] = "";
                MDIMain.mnuGMRU3.Visible = false;

                //if none left
                if (strMRU[0].Length == 0) {
                    //hide bar too
                    MDIMain.mnuGMRUBar.Visible = false;
                }
            }
        }

        public static void AddToMRU(string NewWAGFile) {
            //if NewWAGFile is already in the list,
            //it is moved to the top;
            //otherwise, it is added to the top, and other
            //entries are moved down
            int i, j;
            for (i = 0; i < 4; i++) {
                if (NewWAGFile == strMRU[i]) {
                    // if already at the top
                    if (i == 0) {
                        //nothing to change
                        return;
                    }
                    //move others down
                    for (j = i; j >= 1; j--) {
                        strMRU[j] = strMRU[j - 1];
                        MDIMain.mnuGame.DropDownItems["mnuGMRU" + j].Text = MDIMain.mnuGame.DropDownItems["mnuGMRU" + (j - 1)].Text;
                        MDIMain.mnuGame.DropDownItems["mnuGMRU" + j].Visible = true; // MDIMain.mnuGame.DropDownItems["mnuGMRU" + (j - 1)].Visible;
                    }
                    //move item i to top of list
                    strMRU[0] = NewWAGFile;
                    MDIMain.mnuGMRU0.Text = CompactPath(NewWAGFile, 60);
                    // done
                    return;
                }
            }
            //not found;
            //move all entries down
            for (j = 3; j >= 1; j--) {
                strMRU[j] = strMRU[j - 1];
                MDIMain.mnuGame.DropDownItems["mnuGMRU" + j].Text = MDIMain.mnuGame.DropDownItems["mnuGMRU" + (j - 1)].Text;
                MDIMain.mnuGame.DropDownItems["mnuGMRU" + j].Visible = strMRU[j].Length != 0;
            }
            //add new item 0
            strMRU[0] = NewWAGFile;
            MDIMain.mnuGMRU0.Text = CompactPath(NewWAGFile, 60);
            MDIMain.mnuGMRU0.Visible = true;
            //ensure bar is visible
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
        public static void DrawProp(Graphics gPic, string PropID, string PropValue, int RowNum, bool AllowSelect, int SelectedProp, int PropScroll, bool PropEnabled, EButtonFace ButtonFace = EButtonFace.bfNone) {
            bool blnIsSelected;
            //determine if this prop is selected
            blnIsSelected = (AllowSelect && (SelectedProp == RowNum));
            RowNum -= PropScroll;
            //if rownum is out of bounds
            if (RowNum < 1 || RowNum > (gPic.ClipBounds.Height - 2) / PropRowHeight) {
                return;
            }
            // pens and brushes
            SolidBrush brushDkGray = new(DkGray);
            SolidBrush brushBlack = new(Color.Black);
            SolidBrush brushWhite = new(Color.White);
            SolidBrush brushSelBlue = new(SelBlue);
            Font fontProp = new("MS Sans Serif", 8);
            //strip off any multilines
            if (PropValue.IndexOf((char)Keys.Enter) > 0) {
                PropValue = Left(PropValue, PropValue.IndexOf((char)Keys.Enter));
            }
            if (PropValue.IndexOf((char)Keys.LineFeed) > 0) {
                PropValue = Left(PropValue, PropValue.IndexOf((char)Keys.LineFeed));
            }
            if (blnIsSelected) {
                gPic.FillRectangle(brushSelBlue, 1, PropRowHeight * RowNum, PropSplitLoc - 1, PropRowHeight * RowNum + PropRowHeight - 2);
            }
            gPic.DrawString(PropID, fontProp, blnIsSelected ? brushWhite : brushBlack, 3, PropRowHeight * RowNum + 1);
            gPic.FillRectangle(brushWhite, PropSplitLoc, PropRowHeight * RowNum, gPic.ClipBounds.Width, (RowNum + 1) * PropRowHeight);
            gPic.DrawString(PropValue, fontProp, PropEnabled ? brushBlack : brushDkGray, PropSplitLoc + 3, PropRowHeight * RowNum + 1);
            //if this is selected property AND enabled,
            if (blnIsSelected && PropEnabled) {
                if (ButtonFace != EButtonFace.bfNone) {
                    gPic.DrawImage(MDIMain.imlPropButtons.Images[(int)ButtonFace], gPic.ClipBounds.Width - PropRowHeight, RowNum * PropRowHeight); //, 17, 17, DropDownDC, 0, 0, SRCCOPY);
                }
            }
        }
        public static void AddOrRemoveRes() {
            //*//
            /*
          int i;

          On Error GoTo ErrHandler

          //if no form is active,
          if (MDIMain.ActiveForm = null) {
          //can only mean that Settings.ShowPreview is false,
          //AND Settings.ResListType is non-zero, AND no editor window are open
          //use selected item method
          MDIMain.RemoveSelectedRes
          } else {
          //if active form is NOT the preview form
          //if any form other than preview is active
          if (MDIMain.ActiveForm.Name != "frmPreview") {
            //use the active form method
            MDIMain.ActiveForm.MenuClickInGame
          } else {
            //removing a preview resource; first check for an open
            //editor that matches resource being previewed
            switch (SelResType
            case rtLogic
              //if any logic editor matches this resource
              For i = 1 To LogicEditors.Count
               if (LogicEditors(i).FormMode = fmLogic) {
                 if (LogicEditors(i).LogicNumber = SelResNum) {
                    //use this form//s method
                    LogicEditors(i).MenuClickInGame
                    return;
                  }
                }
              Next i

            case rtPicture
              //if any Picture editor matches this resource
              For i = 1 To PictureEditors.Count
               if (PictureEditors(i).PicNumber = SelResNum) {
                  //use this form//s method
                  PictureEditors(i).MenuClickInGame
                  return;
                }
              Next i

            case rtSound
              //if any Sound editor matches this resource
              For i = 1 To SoundEditors.Count
               if (SoundEditors(i).SoundNumber = SelResNum) {
                  //use this form//s method
                  SoundEditors(i).MenuClickInGame
                  return;
                }
              Next i

            case rtView
              //if any View editor matches this resource
              For i = 1 To ViewEditors.Count
               if (ViewEditors(i).ViewNumber = SelResNum) {
                  //use this form//s method
                  ViewEditors(i).MenuClickInGame
                  return;
                }
              Next i

            default: //words, objects, game or none
              //InGame does not apply

            }

            //if no open editor is found, use the selected item method
            MDIMain.RemoveSelectedRes
          }
          }
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
            */
        }
        public static void CompileAGIGame(string CompGameDir = "", bool RebuildOnly = false) {
            /*
          DialogResult rtn;
          string strTemp;

          int i;
            bool blnDontAsk;

          On Error GoTo ErrHandler

          //if no game is loaded,
          if (!GameLoaded) {
          return;
          }

          //if global editor or layout editor open and unsaved, ask to continue
          if (GEInUse) {
          if (GlobalsEditor.IsDirty) {
            strTemp = "Do you want to save the Global Defines list before compiling?"
          }
          }

          if (LEInUse) {
          if (LayoutEditor.IsDirty) {
           if (Len(strTemp) != 0) {
              //if globals is also open, then adjust message
              strTemp = "Do you want to save the Global Defines list and " + Environment.NewLine + _
                        "Layout Editor before compiling?"
            } else {
              strTemp = "Do you want to save the Global Defines list before compiling?"
            }
          }
          }

          if (Len(strTemp) != 0) {
          rtn = MessageBox.Show(strTemp, MessageBoxButtons.YesNoCancel + MessageBoxIcon.Question, "Save Before Compile?")
          switch (rtn
          case DialogResult.Yes
           if (LEInUse) {
             if (LayoutEditor.IsDirty) {
                LayoutEditor.MenuClickSave
              }
            }
           if (GEInUse) {
             if (GlobalsEditor.IsDirty) {
                GlobalsEditor.MenuClickSave
              }
            }

          case DialogResult.Cancel
            return;
          }
          }

          //check for any open resources
          For i = 1 To LogicEditors.Count
          if (LogicEditors(i).FormMode = fmLogic) {
           if (LogicEditors(i).rtfLogic.Dirty) {
              //saveoncompile is in ask mode or yes mode
             if (Settings.SaveOnCompile != 1) {
                //if not automatic,
               if (Settings.SaveOnCompile = 0) {
                  LogicEditors(i).Focus()
                  //get user//s response
                  rtn = MsgBoxEx("Do you want to save this logic before compiling?", MessageBoxIcon.Question + MessageBoxButtons.YesNoCancel, "Update " + ResourceName(LogicEditors(i).LogicEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
                 if (blnDontAsk) {
                   if (rtn = DialogResult.Yes) {
                      Settings.SaveOnCompile = 2
                    } else {
                      Settings.SaveOnCompile = 1
                    }
                  }

                  //update settings list
                  WriteSetting GameSettings, sLOGICS, "SaveOnComp", Settings.SaveOnCompile

                } else {
                  //if on automatic, always say yes
                  rtn = DialogResult.Yes
                }

                switch (rtn
                case DialogResult.Cancel
                  return;
                case DialogResult.Yes
                  //save it
                  LogicEditors(i).MenuClickSave
                }
              }
            }
          }
          Next i

          For i = 1 To PictureEditors.Count
          if (PictureEditors(i).PicEdit.IsDirty) {
            //saveoncompile is in ask mode or yes mode
           if (Settings.SaveOnCompile != 1) {
              //if not automatic,
             if (Settings.SaveOnCompile = 0) {
                PictureEditors(i).Focus()
                //get user//s response
                rtn = MsgBoxEx("Do you want to save this picture before compiling?", MessageBoxIcon.Question + MessageBoxButtons.YesNoCancel, "Update " + ResourceName(PictureEditors(i).PicEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
               if (blnDontAsk) {
                 if (rtn = DialogResult.Yes) {
                    Settings.SaveOnCompile = 2
                  } else {
                    Settings.SaveOnCompile = 1
                  }
                }
              } else {
                //if on automatic, always say yes
                rtn = DialogResult.Yes
              }

              switch (rtn
              case DialogResult.Cancel
                return;
              case DialogResult.Yes
                //save it
                PictureEditors(i).MenuClickSave
              }
            }
          }
          Next

          For i = 1 To SoundEditors.Count
          if (SoundEditors(i).SoundEdit.IsDirty) {
            //saveoncompile is in ask mode or yes mode
           if (Settings.SaveOnCompile != 1) {
              //if not automatic,
             if (Settings.SaveOnCompile = 0) {
                SoundEditors(i).Focus()
                //get user//s response
                rtn = MsgBoxEx("Do you want to save this Sound before compiling?", MessageBoxIcon.Question + MessageBoxButtons.YesNoCancel, "Update " + ResourceName(SoundEditors(i).SoundEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
               if (blnDontAsk) {
                 if (rtn = DialogResult.Yes) {
                    Settings.SaveOnCompile = 2
                  } else {
                    Settings.SaveOnCompile = 1
                  }
                }
              } else {
                //if on automatic, always say yes
                rtn = DialogResult.Yes
              }

              switch (rtn
              case DialogResult.Cancel
                return;
              case DialogResult.Yes
                //save it
                SoundEditors(i).MenuClickSave
              }
            }
          }
          Next

          For i = 1 To ViewEditors.Count
          if (ViewEditors(i).ViewEdit.IsDirty) {
            //saveoncompile is in ask mode or yes mode
           if (Settings.SaveOnCompile != 1) {
              //if not automatic,
             if (Settings.SaveOnCompile = 0) {
                ViewEditors(i).Focus()
                //get user//s response
                rtn = MsgBoxEx("Do you want to save this View before compiling?", MessageBoxIcon.Question + MessageBoxButtons.YesNoCancel, "Update " + ResourceName(ViewEditors(i).ViewEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
               if (blnDontAsk) {
                 if (rtn = DialogResult.Yes) {
                    Settings.SaveOnCompile = 2
                  } else {
                    Settings.SaveOnCompile = 1
                  }
                }
              } else {
                //if on automatic, always say yes
                rtn = DialogResult.Yes
              }

              switch (rtn
              case DialogResult.Cancel
                return;
              case DialogResult.Yes
                //save it
                ViewEditors(i).MenuClickSave
              }
            }
          }
          Next

          if (OEInUse) {
          if (ObjectEditor.IsDirty) {
            //saveoncompile is in ask mode or yes mode
           if (Settings.SaveOnCompile != 1) {
              //if not automatic,
             if (Settings.SaveOnCompile = 0) {
                ObjectEditor.Focus()
                //get user//s response
                rtn = MsgBoxEx("Do you want to save OBJECT file before compiling?", MessageBoxIcon.Question + MessageBoxButtons.YesNoCancel, "Update OBJECT File?", , , "Always take this action when compiling a game.", blnDontAsk)
               if (blnDontAsk) {
                 if (rtn = DialogResult.Yes) {
                    Settings.SaveOnCompile = 2
                  } else {
                    Settings.SaveOnCompile = 1
                  }
                }
              } else {
                //if on automatic, always say yes
                rtn = DialogResult.Yes
              }

              switch (rtn
              case DialogResult.Cancel
                return;
              case DialogResult.Yes
                //save it
                ObjectEditor.MenuClickSave
              }
            }
          }
          }

          if (WEInUse) {
          if (WordEditor.IsDirty) {
            //saveoncompile is in ask mode or yes mode
           if (Settings.SaveOnCompile != 1) {
              //if not automatic,
             if (Settings.SaveOnCompile = 0) {
                WordEditor.Focus()
                //get user//s response
                rtn = MsgBoxEx("Do you want to save WORDS.TOK file before compiling?", MessageBoxIcon.Question + MessageBoxButtons.YesNoCancel, "Update WORDS.TOK File?", , , "Always take this action when compiling a game.", blnDontAsk)
               if (blnDontAsk) {
                 if (rtn = DialogResult.Yes) {
                    Settings.SaveOnCompile = 2
                  } else {
                    Settings.SaveOnCompile = 1
                  }
                }
              } else {
                //if on automatic, always say yes
                rtn = DialogResult.Yes
              }

              switch (rtn
              case DialogResult.Cancel
                return;
              case DialogResult.Yes
                //save it
                WordEditor.MenuClickSave
              }
            }
          }
          }

          //set default to replace any existing game files
          rtn = DialogResult.Yes

          Do
          //if no directory was passed to the function
          if (LenB(CompGameDir) = 0) {
            //get a new dir
            CompGameDir = GetNewDir(MDIMain.hWnd, "Choose target directory for compiled game:")

            //if not canceled
           if (LenB(CompGameDir) != 0) {
              //if directory already contains game files,
             if (Dir(CDir(CompGameDir) + "*VOL.*") != "") {
                //verify
                rtn = MessageBox.Show("This directory already contains AGI game files. Existing files will be renamed so they will not be lost. Continue with compile?", MessageBoxIcon.Question + MessageBoxButtons.YesNoCancel, "Compile Game")
                //if user said no
               if (rtn = DialogResult.No) {
                  //reset directory
                  CompGameDir = ""
                }
              }
            } else {
              rtn = DialogResult.Cancel
            }
          }
          Loop While rtn = DialogResult.No

          //if canceled
          if (rtn = DialogResult.Cancel) {
          //exit
          return;
          }

          //disable form until compile complete
          MDIMain.Enabled = false
          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //set up compile form
          CompStatusWin = New frmCompStatus
          CompStatusWin.MousePointer = vbArrow
          if (RebuildOnly) {
          CompStatusWin.SetMode 1
          } else {
          CompStatusWin.SetMode 0 //0 means full compile
          }
          CompStatusWin.Show

          //ensure dir has trailing backslash
          CompGameDir = CDir(CompGameDir)

          //setup and clear warning list
          MDIMain.ClearWarnings -1, 0

          //compile the game
          On Error Resume Next
          CompileGame RebuildOnly, CompGameDir

          //hide compile status window while dealing with results
          CompStatusWin.Hide

          //if major error,
          if (Err.Number != 0) {
          //restore cursor
          MDIMain.UseWaitCursor = false;
          //display error message
          strTemp = "An error occurred while building game files. Original files have" + Environment.NewLine
          strTemp = strTemp + "been restored, but you should check all files to make sure nothing" + Environment.NewLine
          strTemp = strTemp + "was lost or corrupted."

          //Error Information:
          ErrMsgBox(strTemp, "", IIf(RebuildOnly, "Rebuild VOL Files", "Compile Game Error")
          //show wait cursor again
          MDIMain.UseWaitCursor = true;

          //unload the compile staus form
          Unload CompStatusWin
          CompStatusWin = null

          //restore form state
          MDIMain.Enabled = true

          //clean up any leftover files
          if (Asc(InterpreterVersion) = 51) {
            Kill CompGameDir + GameID + "DIR.NEW"
          } else {
            Kill CompGameDir + "LOGDIR.NEW"
            Kill CompGameDir + "PICDIR.NEW"
            Kill CompGameDir + "SNDDIR.NEW"
            Kill CompGameDir + "VIEWDIR.NEW"
          }
          Kill CompGameDir + "NEW_VOL.*"

          //try to copy old files back - if this is a directory
          //with old copies of game files, then we should restore
          //them after an error
          //if this directory is new, and didnt have any game
          //files, then these commands won//t hurt anything -
          //nothing will be there to be deleted/copied

          //restore game files, if they got modified
          if (Asc(InterpreterVersion) = 51) {
           if (File.Exists(CompGameDir + GameID + "DIR.OLD")) {
             if (FileLastMod(CompGameDir + GameID + "DIR.OLD") != FileLastMod(CompGameDir + GameID + "DIR")) {
                Kill CompGameDir + GameID + "DIR"
                FileCopy CompGameDir + GameID + "DIR.OLD", CompGameDir + GameID + "DIR"
              }
            }
          } else {
           if (File.Exists(CompGameDir + "LOGDIR.OLD")) {
             if (FileLastMod(CompGameDir + "LOGDIR.OLD") != FileLastMod(CompGameDir + "LOGDIR")) {
                Kill CompGameDir + "LOGDIR"
                FileCopy CompGameDir + "LOGDIR.OLD", CompGameDir + "LOGDIR"
              }
            }
           if (File.Exists(CompGameDir + "PICDIR.OLD")) {
             if (FileLastMod(CompGameDir + "PICDIR.OLD") != FileLastMod(CompGameDir + "PICDIR")) {
                Kill CompGameDir + "PICDIR"
                FileCopy CompGameDir + "PICDIR.OLD", CompGameDir + "PICDIR"
              }
            }
           if (File.Exists(CompGameDir + "SNDDIR.OLD")) {
             if (FileLastMod(CompGameDir + "SNDDIR.OLD") != FileLastMod(CompGameDir + "SNDDIR")) {
                Kill CompGameDir + "SNDDIR"
                FileCopy CompGameDir + "SNDDIR.OLD", CompGameDir + "SNDDIR"
              }
            }
           if (File.Exists(CompGameDir + "VIEWDIR.OLD")) {
             if (FileLastMod(CompGameDir + "VIEWDIR.OLD") != FileLastMod(CompGameDir + "VIEWDIR")) {
                Kill CompGameDir + "VIEWDIR"
                FileCopy CompGameDir + "VIEWDIR.OLD", CompGameDir + "VIEWDIR"
              }
            }
          }

          For i = 0 To 15
           if (Asc(InterpreterVersion) = 51) {
              //v3
             if (File.Exists(CompGameDir + GameID + "VOL." + CStr(i) + ".OLD")) {
               if (FileLastMod(CompGameDir + GameID + "VOL." + CStr(i) + ".OLD") != FileLastMod(CompGameDir + GameID + "VOL." + CStr(i))) {
                  Kill CompGameDir + GameID + "VOL." + CStr(i)
                  FileCopy CompGameDir + GameID + "VOL." + CStr(i) + ".OLD", CompGameDir + GameID + "VOL." + CStr(i)
                }
              }
            } else {
              //v2
             if (File.Exists(CompGameDir + "VOL." + CStr(i) + ".OLD")) {
               if (FileLastMod(CompGameDir + "VOL." + CStr(i) + ".OLD") != FileLastMod(CompGameDir + "VOL." + CStr(i))) {
                  Kill CompGameDir + "VOL." + CStr(i)
                  FileCopy CompGameDir + "VOL." + CStr(i) + ".OLD", CompGameDir + "VOL." + CStr(i)
                }
              }
            }
          Next i

          //clear err
          Err.Clear

          //restore cursor
          MDIMain.UseWaitCursor = false;
          //exit
          return;
          }

          On Error GoTo ErrHandler

          //check for cancel
          if (CompStatusWin.CompCanceled) {
          //need to only restore words/object if
          //compile was to another directory AND not just rebuilding
          On Error Resume Next
          if (CompGameDir != GameDir && !RebuildOnly) {
            //delete any new files
            Kill CompGameDir + "WORDS.TOK"
            Kill CompGameDir + "OBJECT"
            //restore old files
            File.Move(CompGameDir + "WORDS.OLD", CompGameDir + "WORDS.TOK"
            File.Move(CompGameDir + "OBJECT.OLD", CompGameDir + "OBJECT"
          }
          //clean up any leftover files
          if (Asc(InterpreterVersion) = 51) {
            Kill CompGameDir + GameID + "DIR.NEW"
          } else {
            Kill CompGameDir + "LOGDIR.NEW"
            Kill CompGameDir + "PICDIR.NEW"
            Kill CompGameDir + "SNDDIR.NEW"
            Kill CompGameDir + "VIEWDIR.NEW"
          }
          Kill CompGameDir + "NEW_VOL.*"
          //clear errors
          Err.Clear

          //unload the compile staus form
          Unload CompStatusWin
          CompStatusWin = null

          //restore form state
          MDIMain.Enabled = true
          MDIMain.UseWaitCursor = false;

          //exit
          return;
          }

          On Error GoTo ErrHandler

          //check for errors and warnings
          if (CLng(CompStatusWin.lblErrors.Text) + CLng(CompStatusWin.lblWarnings.Text) > 0) {
          //restore cursor
          MDIMain.UseWaitCursor = false;

          //msgbox to user
          MessageBox.Show("Warnings were generated during game compile.", MessageBoxIcon.Information + MessageBoxButtons.OK, "Compile Game"

          //show wait cursor again
          MDIMain.UseWaitCursor = true;

          //if errors
          if (CLng(CompStatusWin.lblErrors.Text) > 0) {
            //rebuild resource list
            BuildResourceTree
          }

          if (CLng(CompStatusWin.lblWarnings.Text) > 0) {
           if (!MDIMain.picWarnings.Visible) {
              MDIMain.pnlWarnings.Visible = true;
            }
          }
          } else {
          //restore cursor
          MDIMain.UseWaitCursor = false;

          //everything is ok
          MessageBox.Show(IIf(RebuildOnly, "Rebuild", "Compile") + " completed successfully.", _
                 MessageBoxIcon.Information + MessageBoxButtons.OK, IIf(RebuildOnly, "Rebuild VOL Files", "Compile Game")

          //show wait cursor again
          MDIMain.UseWaitCursor = true;
          }

          //unload the compile staus form
          Unload CompStatusWin
          CompStatusWin = null

          UpdateSelection rtLogic, SelResNum, umPreview || umProperty

          //restore form state
          MDIMain.Enabled = true
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
            */
        }
        public static bool CompileDirtyLogics(bool NoMsg = false) {
            return false;
            //*//
            /*
          DialogResult rtn;
          bool blnDirtyResources;
          int i;
           AGILogic tmpLogic;
          string strTemp;
          bool blnLoaded;
          string ErrString;
          string[] strErrInfo;
          bool blnDontAsk;
          string[] strWarnings;
            bool blnErr;

          On Error GoTo ErrHandler

          //if no game is loaded,
          if (!GameLoaded) {
          Exit Function
          }

          //check for any open logic resources
          For i = 1 To LogicEditors.Count
          if (LogicEditors(i).FormMode = fmLogic) {
           if (LogicEditors(i).rtfLogic.Dirty) {
              switch (Settings.SaveOnCompile
              case 0  //ask user for input
                LogicEditors(i).Focus()
                //get user//s response
                rtn = MsgBoxEx("Do you want to save this logic before compiling?", MessageBoxIcon.Question + MessageBoxButtons.YesNoCancel, "Update " + ResourceName(LogicEditors(i).LogicEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
               if (blnDontAsk) {
                 if (rtn = DialogResult.Yes) {
                    Settings.SaveOnCompile = 2
                  } else if ( rtn = DialogResult.No) {
                    Settings.SaveOnCompile = 1
                  }
                }

              case 1  //no
                rtn = DialogResult.No

              case 2  //yes
                rtn = DialogResult.Yes
              }

              switch (rtn
              case DialogResult.Cancel
                Exit Function
              case DialogResult.Yes
                //save it
                LogicEditors(i).MenuClickSave
              }
            }
          }
          Next i

          //disable form until compile complete
          MDIMain.Enabled = false
          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //set up compile form
          CompStatusWin = New frmCompStatus
          CompStatusWin.MousePointer = vbArrow
          CompStatusWin.SetMode 2
          CompStatusWin.Show

          //setup and clear warning list
          MDIMain.ClearWarnings -1, 0

          //compile the logics
          foreach (tmpLogic In Logics
          blnLoaded = tmpLogic.Loaded
          if (!blnLoaded) {
            tmpLogic.Load
          }

          //checking this logic
          CompStatusWin.lblStatus.Text = "Checking " + ResourceName(tmpLogic, true, true)
          CompStatusWin.pgbStatus.Value = CompStatusWin.pgbStatus.Value + 1
          CompStatusWin.Refresh
          SafeDoEvents

          On Error Resume Next
          if (!tmpLogic.Compiled) {
            //refresh caption
            CompStatusWin.lblStatus.Text = "Compiling " + ResourceName(tmpLogic, true, true)
            //don't advance progress bar though; only caption gets updated
            CompStatusWin.Refresh
            SafeDoEvents
            //compile this logic
            tmpLogic.Compile

            //check for error
            switch (Err.Number
            case 0    //no error

            //compiler error
            case WINAGI_ERR + 635
              //get error string
              ErrString = Err.Description
              Err.Clear
              //extract error info
              strErrInfo = Split(ErrString, "|")

              With CompStatusWin
                .lblStatus.Text = "Compile error"
                //need to increment error counter, and store this error
                .lblErrors.Text = .lblErrors.Text + 1
              End With

              //add it to warning list
              MDIMain.AddError strErrInfo(0), Val(Left(strErrInfo(2), 4)), Right(strErrInfo(2), Len(strErrInfo(2)) - 6), tmpLogic.Number, strErrInfo(1)
             if (!MDIMain.picWarnings.Visible) {
                MDIMain.pnlWarnings.Visible = true;
              }

              //determine user response to the error
              switch (Settings.OpenOnErr
              case 0 //ask
                //restore cursor before showing msgbox
                MDIMain.UseWaitCursor = false;
                //get user//s response
                rtn = MsgBoxEx("An error occurred while attempting to compile " + ResourceName(tmpLogic, true, true) + ":" + Environment.NewLine + Environment.NewLine _
                         + ErrString + Environment.NewLine + Environment.NewLine + "Do you want to open the logic at the location of the error?", MessageBoxIcon.Question + MessageBoxButtons.YesNo, "Update Logics?", , , "Always take this action when a compile error occurs.", blnDontAsk)
                //show wait cursor again
                MDIMain.UseWaitCursor = true;
               if (blnDontAsk) {
                 if (rtn = DialogResult.Yes) {
                    Settings.OpenOnErr = 2
                  } else {
                    Settings.OpenOnErr = 1
                  }
                }

              case 1 //always yes
                rtn = DialogResult.Yes

              case 2 //always no
                rtn = DialogResult.No
                //restore cursor before showing msgbox
                MDIMain.UseWaitCursor = false;
                MessageBox.Show("An error in your code has been detected in logic //" + ResourceName(tmpLogic, true, true) + "//:" + Environment.NewLine + Environment.NewLine _
                           + "Line " + strErrInfo(0) + ", Error# " + strErrInfo(1), MessageBoxButtons.OK + MessageBoxIcon.Information, "Logic Compiler Error"
                //show wait cursor again
                MDIMain.UseWaitCursor = true;

              }

              //if yes,
             if (rtn = DialogResult.Yes) {
                //set error info for this file
                SetError CLng(strErrInfo(0)), strErrInfo(2), tmpLogic.Number, strErrInfo(1)
                Err.Clear
                //sound a tone
                Beep
              }

              //unload the logic
             if (!blnLoaded) {
                tmpLogic.Unload
              }
              blnErr = true
              Exit For

            default:
              MDIMain.UseWaitCursor = false;
              //some other error
              ErrMsgBox("Error occurred during compilation: ", "", "Compile Error"
              MDIMain.UseWaitCursor = true;
              blnErr = true
              Exit For
            }
            Err.Clear
          }

          On Error GoTo ErrHandler

          //reset dirty status in resource list
          switch (Settings.ResListType
          case 1
            MDIMain.tvwResources.Nodes["l" + CStr(tmpLogic.Number)).ForeColor = Colors.Black
          case 2
            //only update if logics are listed
           if (MDIMain.cmbResType.SelectedIndex = 1) {
              MDIMain.lstResources.ListItems("l" + CStr(tmpLogic.Number)).ForeColor = Colors.Black
            }
          }

          //unload the logic
          if (!blnLoaded) {
            tmpLogic.Unload
          }

          //next logic
          Next

          //if no error, finalize the compile operation
          if (!blnErr) {
          //restore cursor
          MDIMain.UseWaitCursor = false;
          CompStatusWin.Refresh
          //check for errors and warnings
          if (CLng(CompStatusWin.lblErrors.Text) + CLng(CompStatusWin.lblWarnings.Text) > 0) {
            //msgbox to user
            MessageBox.Show("Errors and/or warnings were generated during logic compilation.", MessageBoxIcon.Information + MessageBoxButtons.OK, "Compile Logics"
            MDIMain.UseWaitCursor = true;
            //if errors
           if (CLng(CompStatusWin.lblErrors.Text) > 0) {
              //rebuild resource list
              BuildResourceTree
            }

           if (CLng(CompStatusWin.lblWarnings.Text) > 0) {
             if (!MDIMain.picWarnings.Visible) {
                MDIMain.pnlWarnings.Visible = true;
              }
            }
          } else {
            //everything is ok
           if (!NoMsg) {
              MessageBox.Show("All logics compiled successfully.", MessageBoxIcon.Information + MessageBoxButtons.OK, "Compile Dirty Logics"
            }
            MDIMain.UseWaitCursor = true;
            //return true
            CompileDirtyLogics = true
          }

          //update previewwin, if being used
          if (Settings.ShowPreview) {
           if (SelResType = rtLogic) {
              //redraw the preview
              PreviewWin.LoadPreview rtLogic, SelResNum
            }
          }
          }

          //unload the compile staus form
          Unload CompStatusWin
          CompStatusWin = null

          //restore form state
          MDIMain.Enabled = true
          MDIMain.UseWaitCursor = false;
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
            */
        }
        public static void NewAGIGame(bool UseTemplate) {
            /*

          bool blnClosed;
          bool blnNoErrors;
          int lngErr;
           string strErr;
          string strVer;
           string strDescription;
          string strTemplateDir;
           int i;

          On Error GoTo ErrHandler

          //if using a template
          if (UseTemplate) {
          //have user choose a template
          Load frmTemplate
            frmTemplate.SetForm
            //if no templates available,
           if (frmTemplate.lstTemplates.ListCount = 0) {
              MsgBoxEx "There are no templates available. Unable to create new game.", MessageBoxIcon.Critical + MessageBoxButtons.OK + vbMsgBoxHelpButton, "No Templates Available", WinAGIHelp, "htm\winagi\Templates.htm"
              Unload frmTemplate
              return;
            }

            frmTemplate.ShowDialog(MDIMain);
           if (frmTemplate.Canceled) {
              Unload frmTemplate
              return;
            }
            strTemplateDir = App.Path + "\Templates\" + .lstTemplates.Text
            strDescription = frmTemplate.txtDescription.Text
            strVer = frmTemplate.lblVersion.Text
            Unload frmTemplate
          }

          Load frmGameProperties
          //show new game form
          frmGameProperties.WindowFunction = gsNew
          frmGameProperties.SetForm

          //if using template
          if (UseTemplate) {
            //version is preset based on template
            For i = 0 To frmGameProperties.cmbVersion.ListCount - 1
             if (frmGameProperties.cmbVersion.List(i) = strVer) {
                frmGameProperties.cmbVersion.SelectedIndex = i
                Exit For
              }
            Next i
            frmGameProperties.cmbVersion.Enabled = false
            //default description
            frmGameProperties.txtGameDescription.Text = strDescription
          }

          frmGameProperties.ShowDialog(MDIMain);
          //if canceled
          if (frmGameProperties.Canceled) {
            //unload it
            Unload frmGameProperties
            //exit
            return;
          }

          //if word or Objects Editor open
          if (WEInUse) {
            Unload WordEditor

            //if user canceled,
           if (WEInUse) {
              return;
            }
          }
          if (OEInUse) {
            Unload ObjectEditor
           if (OEInUse) {
              return;
            }
          }

          //if a game is currently open,
          if (GameLoaded) {
            //close game, if user allows
           if (!CloseThisGame()) {
              return;
            }
          }

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //inline error checking
          On Error Resume Next

          //create new game  (newID, version, directory resouredirname and template info)
          NewGame frmGameProperties.txtGameID.Text, frmGameProperties.cmbVersion.Text, frmGameProperties.DisplayDir, frmGameProperties.txtResDir.Text, strTemplateDir

          //check for errors or warnings
          lngErr = Err.Number
          strErr = Err.Description

          On Error GoTo ErrHandler
          if (lngErr = 0 || lngErr = WINAGI_ERR + 637) {
            //add rest of properties
            GameDescription = frmGameProperties.txtGameDescription.Text
            GameAuthor = frmGameProperties.txtGameAuthor.Text
            GameVersion = frmGameProperties.txtGameVersion.Text
            GameAbout = frmGameProperties.txtGameAbout.Text
            //set platform type if a file was provided
           if (Len(frmGameProperties.NewPlatformFile) > 0) {
             if (frmGameProperties.optDosBox.Value) {
                PlatformType = 1
                DosExec = frmGameProperties.txtExec.Text
                PlatformOpts = frmGameProperties.txtOptions.Text
              } else if ( frmGameProperties.optScummVM.Value) {
                PlatformType = 2
                PlatformOpts = frmGameProperties.txtOptions.Text
              } else if ( frmGameProperties.optNAGI.Value) {
                PlatformType = 3
              } else if ( frmGameProperties.optOther.Value) {
                PlatformType = 4
                PlatformOpts = frmGameProperties.txtOptions.Text
              }
            } else {
              PlatformType = 0
            }
           if (PlatformType > 0) {
              Platform = frmGameProperties.NewPlatformFile
            }

            //resdef
            LogicSourceSettings.UseReservedNames = (frmGameProperties.chkUseReserved.Checked)

            //layout editor
            UseLE = (frmGameProperties.chkUseLE.Checked)

            //force a save of the property file
            SaveProperties

            //created ok; maybe with warning
            MDIMain.Text = "WinAGI GDS - " + GameID

            //if there is a layout file
           if (LenB(Dir(GameDir + "*.wal")) != 0) {
              File.Move(GameDir + Dir(GameDir + "*.wal"), GameDir + GameID + ".wal"
            }

           if (Settings.ShowPreview) {
              PreviewWin.ClearPreviewWin
            }

            //build resource treelist
            BuildResourceTree

            switch (Settings.ResListType
            case 1
              //select root
              MDIMain.tvwResources.Nodes[1).Selected = true
              //force update
              MDIMain.SelectResource rtGame, -1
            case 2
              //select root
              MDIMain.cmbResType.SelectedIndex = 0
              //force update
              MDIMain.SelectResource rtGame, -1
            }

            //set default directory
            BrowserStartDir = GameDir

            //add wagfile to mru
            AddToMRU GameDir + GameID + ".wag"

            //adjust menus
            AdjustMenus rtGame, true, false, false

            //show preview window
           if (Settings.ShowPreview) {
              PreviewWin.Show
            }
            //show resource tree
           if (Settings.ResListType != 0) {
              MDIMain.ShowResTree
            }

            //if warnings
           if (lngErr = WINAGI_ERR + 637) {
              //warn about errors
              MessageBox.Show("Some minor errors occurred during game creation. See errlog.txt in the game directory for details.", MessageBoxIcon.Information, "Errors During Load"
            }
          } else {
            MessageBox.Show("Unable to create new game due to an error: " + Environment.NewLine + Environment.NewLine + strErr, MessageBoxIcon.Information + MessageBoxButtons.OK, "New AGI Game Error"
          }

          //unload game properties form
          Unload frmGameProperties

          //reset cursor
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
            */
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
        static void tmpResMan() {
            /*


          public static void ChangeGameID(string NewID)

          On Error GoTo ErrHandler

          //if property file is linked to game ID
          if (StrComp(GameFile, GameDir + GameID + ".wag", StringComparison.OrdinalIgnoreCase) = 0) {
          //update mru value
          RenameMRU GameFile, GameDir + NewID + ".wag"
          }

          //update name of layout file, if it exists (it always should
          //match GameID)
          On Error Resume Next
          File.Move(GameDir + GameID + ".wal", GameDir + NewID + ".wal"

          On Error GoTo ErrHandler

          //change id (which changes the game file automatically, if it
          //is linked to ID)
          GameID = NewID

          //update resource list
          switch (Settings.ResListType
          case 1
          MDIMain.tvwResources.Nodes[1).Text = GameID
          case 2
          MDIMain.cmbResType.List(0) = GameID
          }
          //update form caption
          MDIMain.Text = "WinAGI GDS - " + GameID

          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static int CheckLine(ref string strLine)

          //this function will examine a line of text to see if the
          //end pos is either in a comment or in a string

          // return values:
          //  0 = endpos for this line is NOT in quote or comment
          //  1 = endpos for this line is in a quote
          //  2 = endpos for this line is in a comment


          int lngPos;
          int rtn, i;
          bool blnInQuote, blnInComment;

          On Error GoTo ErrHandler

          //length
          rtn = Len(strLine)
          i = 1

          Do Until i > rtn
          //is this a string character or comment character?
          switch (AscB(Mid$(strLine, i))
          case 34 //double quote mark
           if (IsValidQuote(strLine, i)) {
              //if not already in a quote
             if (!blnInQuote) {
                //now we are...
                blnInQuote = true
              } else {
                //now we aren//t...
                blnInQuote = false
              }
            }

          case 47 //slash, to check for //////
            //if not in a quote
           if (!blnInQuote) {
              //check for dbl slash
             if (i < Len(strLine)) {
               if (AscB(Mid$(strLine, i + 1)) = 47) {
                  //this line has a comment at the end
                  blnInComment = true
                  break;
                }
              }
            }

          case 91 //open bracket //[// is also a comment marker
            //if not in a quote
           if (!blnInQuote) {
              //this line has a comment starting here
              blnInComment = true
              break;
            }
          }

          i = i + 1
          Loop

          // return the result
          if (blnInQuote) {
          CheckLine = 1
          } else if ( blnInComment) {
          CheckLine = 2
          } else {
          //neither
          CheckLine = 0
          }

          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }


          public static string EnCodeSnippet(string FullText)

          //converts full text into snippet text by inserting
          //control codes where needed

          //(does not handle argument values- they have to be
          // inserted manually by the user before calling this
          // function)

          int lngPos;

          On Error GoTo ErrHandler

          //convert special chars to control codes
          //  % to %%
          EnCodeSnippet = Replace(FullText, "%", Chr(9))

          //quote marks to %q
          EnCodeSnippet = Replace(EnCodeSnippet, QUOTECHAR, "%q")

          //indent space to %t
          //Debug.Assert InStr(1, EnCodeSnippet, Chr(10)) = 0

          lngPos = 0
          Do
          //are there spaces following this cr?
          if (InStr(lngPos + 1, EnCodeSnippet, Space(Settings.LogicTabWidth)) = lngPos + 1) {
            //replace the spaces
            EnCodeSnippet = Left(EnCodeSnippet, lngPos) + Replace(EnCodeSnippet, Space(Settings.LogicTabWidth), "%t", lngPos + 1, 1)
            //adjust position
            lngPos = lngPos + 2
          } else {
            //no more tabs for this line; get start of next line
            lngPos = InStr(lngPos + 1, EnCodeSnippet, Keys.Enter)
            //if none
           if (lngPos = 0) {
              //done
              break;
            }
          }
          Loop While true

          // cr to %n
          EnCodeSnippet = Replace(EnCodeSnippet, Keys.Enter, "%n")

          //re-insert percents
          EnCodeSnippet = Replace(EnCodeSnippet, Chr(9), "%")

          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static void UpdateLEStatus()

          With 
          //if layout editor is no longer in use
          if (!UseLE) {
            //disable the menubar and toolbar
            MDIMain.mnuTLayout.Enabled = false
            MDIMain.Toolbar1.Buttons("layout").Enabled = false
            //if using it, need to close it
           if (LEInUse) {
              MsgBoxEx "The current layout editor file will be closed. " + Environment.NewLine + Environment.NewLine + _
              "If you decide to use the layout editor again at a later" + Environment.NewLine + _
              "time, you will need to rebuild the layout to update it. ", MessageBoxButtons.OK + MessageBoxIcon.Information + vbMsgBoxHelpButton, "Closing Layout Editor", WinAGIHelp, "htm\winagi\Layout_Editor.htm"
              //save it, if dirty
             if (LayoutEditor.IsDirty) {
                LayoutEditor.MenuClickSave
              }
              //close it
              Unload LayoutEditor
              LEInUse = false
            }
          } else {
            //enable the menu and toolbar
            MDIMain.mnuTLayout.Enabled = true
            MDIMain.Toolbar1.Buttons("layout").Enabled = true
          }
          }


          public static bool IsResource(string strToken)

          //returns true if strToken is a valid resource ID in a game

          int i;

          On Error GoTo ErrHandler

          //if game not loaded, always return false
          if (!GameLoaded) {
          IsResource = false
          Exit Function
          }

          // blanks never match
          if (Len(strToken) = 0) {
          IsResource = false
          Exit Function
          }

          //step through all resources
          //(use globals list; it's going to be much faster)

          For i = 0 To 1023
          if (IDefLookup(i).Name = strToken) {
            IsResource = true
            Exit Function
          }
          Next i

          //not found, must not be a resource
          IsResource = false
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static void ChangeResDir(string NewResDir)

          On Error GoTo ErrHandler

          DialogResult rtn;

          //display wait cursor while copying
              MDIMain.UseWaitCursor = true;


          //if the folder already exists,
          if (LenB(Dir(GameDir + NewResDir, vbDirectory)) != 0) {
          //ask if replace is OK?
          rtn = MessageBox.Show("Existing files in new directory will be overwritten by " + Environment.NewLine + _
                       "resources with same name. Do you want to continue?", MessageBoxIcon.Question + MessageBoxButtons.YesNo, "Change Resource Directory")

          //if no, do nothing; don't change resdir
          if (rtn = DialogResult.Yes) {

            //show ProgressWin form
            Load ProgressWin
              ProgressWin.Text = "Changing Resource Directory"
              ProgressWin.lblProgress = "Depending on size of game, this may take awhile. Please wait..."
              ProgressWin.pgbStatus.Visible = false
              ProgressWin.Show
              ProgressWin.Refresh

            //move them
           if (!CopyFolder(ResDir, GameDir + NewResDir, true)) {
              //warn
              MessageBox.Show("!all files were able to be moved. Check your old and new directories" + Environment.NewLine + "and manually move any remaining resources.", MessageBoxIcon.Information, "File Move Error"
            }
            //change resdir
            ResDirName = NewResDir

            //done with ProgressWin form
            Unload ProgressWin
            MessageBox.Show("Done!", MessageBoxButtons.OK + MessageBoxIcon.Information, "Change Resource Directory"
          }

          } else {
          //show ProgressWin form
          Load ProgressWin
            ProgressWin.Text = "Changing Resource Directory"
            ProgressWin.lblProgress = "Depending on size of game, this may take awhile. Please wait..."
            ProgressWin.pgbStatus.Visible = false
            ProgressWin.Show
            ProgressWin.Refresh

          //need to create a new directory
          MkDir GameDir + NewResDir
          //and move the existing resdir to the new location

          if (!CopyFolder(ResDir, GameDir + NewResDir, true)) {
            //warn
            MessageBox.Show("!all files were able to be moved. Check your old and new directories" + Environment.NewLine + "and manually move any remaining resources.", MessageBoxIcon.Information, "File Move Error"
          }
          //change resdir
          ResDirName = NewResDir

          //done with ProgressWin form
          Unload ProgressWin
          MessageBox.Show("Done!", MessageBoxButtons.OK + MessageBoxIcon.Information, "Change Resource Directory"
          }

          //update the preview window, if previewing logics,
          UpdateSelection rtLogic, SelResNum, umPreview

          //reset cursor
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }





          public static bool CheckLogics()

          //checks all logics; if any found that are dirty
          //allow user to recompile game if desired before
          //running

          AGILogic tmpLogic;
            int i;
          DialogResult rtn;
            bool blnDontAsk;
          bool blnLoaded;

          On Error GoTo ErrHandler

          //assume ok
          CheckLogics = true

          //if not requiring recompile
          if (Settings.CompileOnRun = 1) {
          //don't even need to check
          //just exit
          Exit Function
          }

          //step through all logics
          foreach (tmpLogic In Logics
          blnLoaded = tmpLogic.Loaded
          if (!blnLoaded) {
            tmpLogic.Load
          }
          if (!tmpLogic.Compiled) {
            //not ok; skip checking and
            //determine if recompiling is appropriate
            CheckLogics = false
            //dont// forget to unload!!
           if (!blnLoaded) {
              tmpLogic.Unload
            }

            Exit For
          }
          if (!blnLoaded) {
            tmpLogic.Unload
          }
          Next

          //if no dirty logics found, check any existing logics that are being edited
          if (CheckLogics = true) {
          For i = 1 To LogicEditors.Count
           if (LogicEditors[i].FormMode = fmLogic) {
             if (LogicEditors[i].rtfLogic.Dirty) {
                //one dirty logic found
                CheckLogics = false
                Exit For
              }
            }
          Next i
          }

          //if still no dirty logics found
          if (CheckLogics = true) {
          //just exit
          Exit Function
          }

          //if CompileOnRun is in ask mode or yes mode, get user choice
          switch (Settings.CompileOnRun
          case 0 //ask for user input
          //get user//s response
          rtn = MsgBoxEx("One or more logics have changed since you last compiled." + Environment.NewLine + _
                         "Do you want to compile them before running?", MessageBoxIcon.Question + MessageBoxButtons.YesNoCancel, "Compile Before Running?", , , "Always take this action when compiling a game.", blnDontAsk)
          if (blnDontAsk) {
           if (rtn = DialogResult.Yes) {
              Settings.CompileOnRun = 2
            } else if ( rtn = DialogResult.No) {
              Settings.CompileOnRun = 1
            }

            //update settings list
            WriteSetting GameSettings, sLOGICS, "CompOnRun", Settings.CompileOnRun
          }

          case 1  //no
          rtn = DialogResult.No

          case 2 // yes
            rtn = DialogResult.Yes
          }

          switch (rtn
          case DialogResult.Cancel
          //return false, so run cmd is canceled
          CheckLogics = false

          case DialogResult.No
          //ok to exit; check is complete
          CheckLogics = true
          case DialogResult.Yes
          //if dirtylogics successfully compiled
          CheckLogics = CompileDirtyLogics(true)
          }
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }



          public static string CmdInfo(RichEdAGI rtfLogic)

          int rtn;
            RichEditAGI.Range tmpRange;
          int lngCmdPos;
          string strLine;

          On Error GoTo ErrHandler

          //set cmd start pos
          lngCmdPos = rtfLogic.Selection.Range.EndPos

          //get line where enter was pressed
          rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, rtfLogic.Selection.Range.StartPos, 0)
          //get the start of this line
          rtn = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, rtn, 0)
          tmpRange = rtfLogic.Range(rtn, rtn)
          tmpRange.Expand (reLine)
          strLine = Left$(tmpRange.Text, Len(tmpRange.Text) - 1)
          tmpRange = null
          CmdInfo = strLine
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static string FindPrevCmd(ref string strText, ref int lngStartPos, ref int lngArgCount, bool blnInQuote = false, bool blnPrev = false)

          //searches backwards through strText, starting at lngStartPos and returns the
          // command currently being edited; null string if there isn//t one

          //
          //cmd is found by looking for an open parenthesis "("; the word
          //in front of this parenthesis is the cmd of interest
          //
          //stop looking when:
          // - a cmd is found (a word separator is found in front of the char string
          //   that precedes the parenthesis)
          // - beginning of logic is reached
          // - cursor moves to previous line (that//s a semicolon ";" or bracket "{"
          //   or "}")
          //
          // lngArgCount is set by counting number of commas between tokens as the
          // search goes
          //
          //if quotes are found, ignore all chars inside as they are part of a string
          //declaration; blnInQuote is used to tell the function if the current search
          //is starting in a string

          //lngStartPos is adjusted to be at end of the found command
          //(basically the location of the //(// for this command)

          // use blnPrev to force the function to return the immediately preceding token
          // instead of searching for a command token

          int lngPos;
           string strCmd;

          On Error GoTo ErrHandler

          lngArgCount = 0
          lngPos = lngStartPos

          //find prev token
          strCmd = FindPrevToken(strText, lngPos, blnInQuote)

          Do Until Len(strCmd) = 0
          //if this is the command we want, return it
          if (blnPrev) {
            break;
          } else {
            //if a comma, advance the arg count
           if (strCmd = ",") {
              lngArgCount = lngArgCount + 1
            }
          }

          //single char cmds are only things we
          //have an interest in
          if (Len(strCmd) = 1) {
            switch (Asc(strCmd)
            case 40 //(//
              //next(prev) token is the command we are looking for
              blnPrev = true

            case 41, 59 To 62, 123, 125 //)//, //;//,<,=,>, //{//, //}//
              //always exit
              break;
            }
          }

          //if exactly two characters, we check for other math operators
          if (Len(strCmd) = 2) {
            switch (strCmd
            case "=="
              //always exit
              break;
            case "!="
              //always exit
              break;
            case "<=", "=<"
              //always exit
              break;
            case ">=", "=>"
              //always exit
              break;
            case "&&", "||", "++", "--"
              //always exit
              break;
            }
          }

          //get next(prev) cmd
          strCmd = FindPrevToken(strText, lngPos, false)
          Loop

          FindPrevCmd = strCmd
          lngStartPos = lngPos + Len(strCmd)
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }
          public static string FindPrevToken(ref string strLine, ref int lngStartPos, ref bool blnInQuote)

          //searches backwards through strLine and returns the token that precedes
          // the startpos
          //if startpos is within a token, that partial token is returned
          //
          //if beginning of line is reached, return empty string
          //
          //calling function must tell us if starting token is in a quote
          //calling function has already determined we are NOT inside a comment

          int lngPos, lngBOL; 
          string strToken;
           short intChar;

          On Error GoTo ErrHandler

          lngPos = lngStartPos

          //find start of line
          if (lngPos > 1) {
          lngBOL = InStrRev(strLine, Keys.Enter, lngPos) + 1
          } else {
          lngBOL = 1
          }


          //this function will return the PREVIOUS command, which is comprised
          //of command elements, and separated by element separators
          //command elements include:
          //  characters a-z, A-Z, numbers 0-9, and:  #$%.@_
          //  (and also, all extended characters [128-255])
          //  NOTE: inside quotations, ALL characters, including spaces
          //  are considered command elements
          //
          //element separators include:
          //  space, !"&//()*+,-/:;<=>?[\]^`{|}~
          //
          //element separators other than space are normally returned
          //as a single character command; there are some exceptions
          //where element separators will include additional characters:
          //  !=, &&, *=, ++, +=, --, -=, /=, //, <=, <>, =<, ==, =>, >=, ><, ||

          //find first char
          Do While lngPos >= lngBOL
          intChar = AscW(Mid$(strLine, lngPos))
          switch (intChar
          case 32
            //skip spaces

          case 39, 40, 41, 44, 58, 59, 63, 91, 92, 93, 94, 96, 123, 125, 126
            //   //(),:;?[\]^`{}~
            //these are ALWAYS a single character token, so we can just exit
            lngStartPos = lngPos - 1
            FindPrevToken = ChrW(intChar)
            Exit Function
          default:
            //start(end) of a token
            lngPos = lngPos - 1
            strToken = ChrW(intChar)
           if (intChar = 34) {
              //end of a string
              blnInQuote = true
            }
            break;
          }
          //keep looking
          lngPos = lngPos - 1
          Loop

          //now find beginning of the token
          Do While lngPos >= lngBOL
          intChar = AscW(Mid$(strLine, lngPos))

          //if in a quote, keep backing up until starting quote found
          if (blnInQuote) {
            //add the char
            strToken = ChrW(intChar) + strToken
            //then find the end(front) of the string
            Do
             if (intChar = 34) {
                //check for embedded quote (\")
                //toggle quote flag until something
                //other than a slash is found in front
                blnInQuote = false
               if (lngPos > lngBOL) {
                  Do Until Asc(Mid$(strLine, lngPos - 1)) != 92
                    blnInQuote = !blnInQuote
                    lngPos = lngPos - 1
                    strToken = "\" + strToken
                   if (lngPos < lngBOL) {
                      //shouldn//t be possible but just in case
                      //Debug.Assert true
                      blnInQuote = false
                      break;
                    }
                  Loop
                }
                //backup one more space
                lngPos = lngPos - 1
                //if no longer in quotes, token is found
               if (!blnInQuote) {
                  lngStartPos = lngPos
                  FindPrevToken = strToken
                  Exit Function
                }
              } else {
                //keep going
                lngPos = lngPos - 1
              }

              //get another character
             if (lngPos < lngBOL) {
                //if a bad string is encountered it is possible
                // forlgPos to go past BOL
                //Debug.Assert blnInQuote
                break;
              }
              intChar = AscW(Mid$(strLine, lngPos))
              //add the char
              strToken = ChrW(intChar) + strToken
            Loop Until lngPos < lngBOL
          } else {
            //if this character is a separator, we stop
            //  space, !"&//()*+,-/:;<=>?[\]^`{|}~
            switch (intChar
            case 32, 39, 40, 41, 44, 58, 59, 63, 91, 92, 93, 94, 96, 123, 125, 126
            //   //(),:;?[\]^`{}~
              //single character separators that ALWAYS mark end
              break;
            case 33, 38, 42, 43, 45, 47, 60, 61, 62, 124
              //   !&*+-/<=>|
              //possible multi-char separators; only treat as
              //a separator if it's not part of a two-char token
              switch (intChar
              case 33 // !  allowed: !=
               if (strToken != "=") {
                  //it's a separator
                  break;
                }
              case 38  // +  allowed: &&
               if (strToken != "&") {
                  //it's a separator
                  break;
                }
              case 42  // *  allowed: *=, *text
                switch (Asc(strToken)
                case 35 To 37, 46, 48 To 57, 61, 64 To 90, 95, 97 To 122
                  //it's not a separator
                default:
                  //it's a separator
                  break;
                }
              case 43  // +  allowed: ++, +=
               if (strToken != "+" && strToken != "=") {
                  //it's a separator
                  break;
                }
              case 45  // -  allowed: --, -=
               if (strToken != "-" && strToken != "=") {
                  //it's a separator
                  break;
                }
              case 47  // /  allowed: /=
               if (strToken != "=") {
                  //it's a separator
                  break;
                }
              case 60  // <  allowed: <=
               if (strToken != "=") {
                  //it's a separator
                  break;
                }
              case 61  // =  allowed: =>, =<, ==
               if (strToken != ">" && strToken != ">" && strToken != "=") {
                  //it's a separator
                  break;
                }
              case 62  // >  allowed: >=
               if (strToken != "=") {
                  //it's a separator
                  break;
                }
              case 124 // |  allowed: ||
               if (strToken != "|") {
                  //it's a separator
                  break;
                }
              }
            }
            //add the character to front of token
            strToken = ChrW(intChar) + strToken
            lngPos = lngPos - 1
          }
          Loop

          //return what we got
          lngStartPos = lngPos
          FindPrevToken = strToken
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static int LogicCmd(string strCmd)

          //returns the command number of the string
          //if strCmd matches an AGI command

          int i, j, k;

          if (LenB(strCmd) = 0) {
          LogicCmd = -1
          Exit Function
          }

          //index based on first letter
          switch (Asc(strCmd)
          case 97, 65 //a
          i = 0
          j = 7
          case 98, 66 //b
          i = 8
          j = 8
          case 99, 67 //c
          i = 9
          j = 20
          case 100, 68 //d
          i = 21
          j = 34
          case 101, 69 //e
          i = 35
          j = 39
          case 102, 70 //f
          i = 40
          j = 43
          case 103, 71 //g
          i = 44
          j = 53
          case 104, 72 //h
          i = 54
          j = 54
          case 105, 73 //i
          i = 55
          j = 60
          case 108, 76 //l
          i = 61
          j = 72
          case 109, 77 //m
          i = 73
          j = 77
          case 110, 78 //n
          i = 78
          j = 82
          case 111, 79 //o
          i = 83
          j = 92
          case 112, 80 //p
          i = 93
          j = 102
          case 113, 81 //q
          i = 103
          j = 103
          case 114, 82 //r
          i = 104
          j = 115
          case 115, 83 //s
          i = 116
          j = 153
          case 116, 84 //t
          i = 154
          j = 156
          case 119, 87 //w
          i = 157
          j = 158
          default:
          //not a command
          LogicCmd = -1
          Exit Function
          }

          For k = i To j
          if (StrComp(strCmd, LoadResString(ALPHACMDTEXT + k), StringComparison.OrdinalIgnoreCase) = 0) {
            LogicCmd = k
            Exit Function
          }
          Next k

          //not found
          LogicCmd = -1
          }

          public static void RenameMRU(string OldWAGFile, string NewWAGFile)


          //if NewWAGFile is already in the list,
          //remove it - it will take the place of
          //OldWAGFile

          //if OldWAGFile is NOT on list, add NewWAGFile;
          //otherwise just rename OldWAGFile to NewWAGFile

          int i, j;

          //first look for NewWAGFile, and delete it if found
          For i = 1 To 4
          if (NewWAGFile = strMRU[i]) {
            //delete it by moving others up
            For j = i To 3
              strMRU(j) = strMRU(j + 1)
              MDIMain.Controls("mnuGMRU" + CStr(j)).Text = CompactPath(strMRU(j), 60)
            Next j
            //now delete last entry
            strMRU(4) = ""
            MDIMain.mnuGMRU1.Text = ""
            Exit For
          }
          Next i

          //now check for OldWAGFile
          For i = 1 To 4
          if (strMRU[i] = OldWAGFile) {
          //rename it
            strMRU[i] = NewWAGFile
            MDIMain.Controls("mnuGMRU" + CStr(i)).Text = CompactPath(NewWAGFile, 60)
            Exit For
          }
          Next i

          //make sure NewWAGFile is at the top (use AddToMRU function to do this easily!)
          AddToMRU NewWAGFile

          }

          public static void SafeDoEvents()
          //this function calls the DoEvents function,
          //which allows out of sync methods, such as
          //changing focus, to work as intended
          //
          //the disabling of the app is important because
          //we need to keep user from doing something in
          //the time that the DoEvents method is called

          //HMMMM, disabling the main form does NOT prevent
          // user from using cursor keys to change selected
          // item in tree; I thought it would!

          // fixed by adding check in tvwResources key handler
          // but I wonder what other controls need a
          // similar check...

          On Error GoTo ErrHandler

          MDIMain.Enabled = false
          DoEvents
          MDIMain.Enabled = true
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static void SetLogicCompiledStatus(byte ResNum, bool Compiled)

          On Error GoTo ErrHandler

          // updates color of a resource list logic entry
          // to indicate compiled status

          switch (Settings.ResListType
          case 1
          MDIMain.tvwResources.Nodes["l" + ResNum).ForeColor = IIf(Compiled, Colors.Black, Colors.Red)
          case 2
          //only need to update if logics are selected
          if (MDIMain.cmbResType.SelectedIndex = 1) {
            MDIMain.lstResources.ListItems("l" + ResNum).ForeColor = IIf(Compiled, Colors.Black, Colors.Red)
          }
          }
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }


          public static void SetError(int lngErrLine, string strErrMsg, int LogicNumber, string strModule, bool blnWarning = false)
          //this procedure uses errinfo to open the file
          //with the error, if possible,
          //highlights the line with the error,
          //and displays the error message in the status bar

          short i;
           string strErrType;
          int rtn, lng1st;
          int lngStart, lngLength;
          int lngLineCount;
          Form frmTemp;
          int lngTopLine, lngThisLine;
          POINTAPI pPos;
            int lngBtmLine;
           RECT rtfRect;

          On Error GoTo ErrHandler

          //if an include module filename is passed,
          if (strModule != "") {
          //module info contains name- attempt to open include file

          OpenTextFile strModule, true
          //set focus to this file
          For i = 1 To LogicEditors.Count
           if (LogicEditors(i).FormMode = fmText) {
              //if this is the form we are looking for
             if (LogicEditors(i).FileName = strModule) {
                frmTemp = LogicEditors(i)
                Exit For
              }
            }
          Next i

          //if text editor not found,
          if (frmTemp = null) {
            //if just warning, exit
           if (blnWarning) {
              return;
            }

            //set error line to zero
            lngErrLine = 0
            //set error msg to indicate include file not opened
            strErrMsg = strErrMsg + " (in INCLUDE file)"
          }
          }

          //if no editor assigned yet,
          if (frmTemp = null) {
          //check if logic is already open
          For i = 1 To LogicEditors.Count
           if (LogicEditors(i).FormMode = fmLogic) {
             if (LogicEditors(i).LogicNumber = LogicNumber) {
                //this is it
                frmTemp = LogicEditors(i)
                Exit For
              }
            }
          Next i

          //if not found
          if (frmTemp = null) {
            //open a new one, if possible
            OpenLogic LogicNumber, true

            //now try to find the newly opened editor
             For i = 1 To LogicEditors.Count
             if (LogicEditors(i).FormMode = fmLogic) {
               if (LogicEditors(i).LogicNumber = LogicNumber) {
                  //this is it
                  frmTemp = LogicEditors(i)
                  Exit For
                }
              }
            Next i
          }
          }

          //if still nothing
          if (frmTemp = null) {
          //if just a warning
          if (blnWarning) {
            return;
          }

          //try a message box instead
          MessageBox.Show("ERROR in line " + CStr(lngErrLine + 1) + ": " + strErrMsg, MessageBoxButtons.OK + MessageBoxIcon.Critical, "Compile Logic Error"
          return;
          }

          if (frmTemp.Enabled) {
          //set focus to this editor NOT WORKING!!!
          frmTemp.Focus()
          } else {
          frmTemp.ZOrder
          }

          //get number of lines in this editor
          lngLineCount = SendMessage(frmTemp.rtfLogic.hWnd, EM_GETLINECOUNT, 0, 0)
          //get character position of first error line
          lngStart = SendMessage(frmTemp.rtfLogic.hWnd, EM_LINEINDEX, lngErrLine - 1, 0)
          frmTemp.rtfLogic.Selection.Range.StartPos = lngStart
          //if past end of input
          if (lngErrLine >= lngLineCount) {
            //set length to select to end of input
            lngLength = Len(.Text) - lngStart
            frmTemp.rtfLogic.Selection.Range.EndPos = frmTemp.rtfLogic.Selection.Range.StartPos + lngLength
          } else {
            //get character position of line immediately below last error line
            //(move cursor back one character to put it at end of last error line)
            lngLength = SendMessage(.hWnd, EM_LINEINDEX, lngErrLine, 0) - lngStart - 1
            frmTemp.rtfLogic.Selection.Range.EndPos = frmTemp.rtfLogic.Selection.Range.StartPos + lngLength
          }
          Do
            //determine top, bottom and current line numbers
            lngTopLine = SendMessage(frmTemp.rtfLogic.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
            lngThisLine = lngErrLine
            //find bottom line in steps

          //////    On Error Resume Next
          //////      frmTemp.rtfLogic.GetViewRect i, rtn, pPos.X, pPos.Y
          //////     if (Err.Number != 0) { break;
            //GetViewRect gives an error here; says sub not defined;
            //makes no sense, because it works just fine in FindInLogics

            //use api calls instead
            rtn = SendMessageRctL(frmTemp.rtfLogic.hWnd, EM_GETRECT, 0, rtfRect)
            pPos.X = rtfRect.Right
            pPos.Y = rtfRect.Bottom
            rtn = SendMessagePtL(.hWnd, EM_CHARFROMPOS, 0, pPos)
            lngBtmLine = SendMessage(.hWnd, EM_LINEFROMCHAR, rtn, 0)
            // if line NOT more than four above bottom, scroll up
           if (lngBtmLine - lngThisLine < 4) {
              //scroll so this line is four lines above bottom
              //get currently visible first line
              //determine amount of scrolling to do
              rtn = SendMessage(.hWnd, EM_LINESCROLL, 0, 4 - (lngBtmLine - lngThisLine))
            }
          Loop Until true
          //set focus to the text editor
          frmTemp.rtfLogic.Focus()
          //refresh the screen
          frmTemp.rtfLogic.Refresh

          //if not a warning
          if (!blnWarning) {
          strErrType = "ERROR "
          } else {
          strErrType = "WARNING "
          }

          //set status bar
          SettingError = true
          frmTemp.Tag = strErrType + "in line " + CStr(lngErrLine) + ": " + strErrMsg
          MainStatusBar.Panels(1).Text = frmTemp.Tag

          return;

          ErrHandler:
          if (Err.Number != 70) {
          //Debug.Assert false
          }
          Resume Next
          }

          public static int FindNextToken(string strText, ref int lngPos, ref strFindCmd = "", bool UpdatePos = true, bool ValidCmds = false)
          //this function returns the starting position of the
          //next instance of strFindCmd, and optionally adjusts lngPos to the
          //end of the token
          //
          //if no search string passed (strFindCmd=""), then position of next token
          //is returned and the token is passed back as strFindCmd
          //
          // lngPos is 0 based (i.e. first char of strText is at position 0)
          //
          //tokens are comprised of token elements, and separated by element separators
          //token elements include:
          //  characters a-z, A-Z, numbers 0-9, and:  #$%.@_
          //  NOTE: inside quotations, ALL characters, including spaces
          //  are considered token elements
          //
          //element separators include:
          //  space, !"&//()*+,-/:;<=>?[\]^`{|}~
          //
          //for compiling, element separators other than space are normally returned
          //as a single character token with some exceptions; this method
          //is only used to find word-Type tokens, so no checks are made
          //for non-alphanumeric multiple character tokens
            //  !=, &&, *=, ++, +=, --, -=, /=, //, /*, <=, <>, =<, ==, =>, >=, ><, ||
            //
          //when a token starts with a quote, the function returns
          //after a closing quote is found, regardless of characters
          //inbetween the quotes
          //
          //if strFindCmd not found, it returns 0
          //if strFindCmd is the keyphrase "##LE", find next LE exit marker

          //comments are returned as tokens; assumption is that starting
          //point (lngPos) is NOT within a commment to start with

          //this search is NOT case sensitive

          short intChar;
            bool blnContinue;
          bool blnEmbed, blnInQuotes;
          string strToken;
          int lngLen;
            bool blnNextToken;
          int lngTknPos;

          //use local copy of position
          lngTknPos = lngPos

          //if no search string passed, return next cmd
          blnNextToken = (LenB(strFindCmd) = 0)

          //quick check for the search string;
          //if it doesn't even exist in the text,
          //don't bother doing a search
          if (!blnNextToken) {
          if (InStr(lngTknPos + 1, strText, strFindCmd, StringComparison.OrdinalIgnoreCase) = 0) {
            //not found
           if (UpdatePos) {
              lngPos = 0
            }
            Exit Function
          }
          }

          //convert string length into variable for speed
          lngLen = Len(strText)

          //step through cmds until a match is found
          Do
          //find next non-blank character
          intChar = NextChar(strText, "", lngTknPos)
          //if at end of input,
          if (lngTknPos = 0) {
            //not found; just exit
           if (UpdatePos) {
              lngPos = 0
            }
            FindNextToken = 0
            Exit Function
          }

          On Error GoTo ErrHandler

          //assume this position begins a token
          FindNextToken = lngTknPos

          //extract the token at this point

          //if character is a special character
          switch (intChar
          case 34 //"
            //special case; quote means start of a string
            blnInQuotes = true

          case 47 ///
            //special case; "//" marks beginning of comment
            //if next char is a "/"
           if (Mid$(strText, lngTknPos + 1, 1) = "/") {
              //move to end of line
              lngTknPos = InStr(lngTknPos, strText, Keys.Enter) - 1
             if (lngTknPos = -1) {
                lngTknPos = lngLen
              }
            }

          case 91 //[
            //special case; "[" marks beginning of comment
            //move to end of line
            lngTknPos = InStr(lngTknPos, strText, Keys.Enter) - 1
           if (lngTknPos = -1) {
              lngTknPos = lngLen
            }

          }

          //if a string was found (inquotes is true)
          if (blnInQuotes) {
            //process a string

            //add characters until another true quote is found, a cr is found, or EOL is reached
            Do
              //increment position
              lngTknPos = lngTknPos + 1
              //check for end of line
             if (lngTknPos > lngLen) {
                //end  reached
                lngTknPos = lngLen
                //not in a quote anymore
                blnInQuotes = false
                //exit
                break;
              }

              //get next character
              intChar = AscW(Mid$(strText, lngTknPos, 1))

              switch (intChar
              case 13  //end of line
                //readjust lngpos
                lngTknPos = lngTknPos - 1
                //not in a quote anymore
                blnInQuotes = false
                break;
              case 34 //if the next character is a quotation mark,
               if (!blnEmbed) {
                  //reset quote flag
                  blnInQuotes = false
                }
              }

              //if this character is slash,
              //next character could be an embedded quote
              blnEmbed = (intChar = 92)
            Loop While blnInQuotes

          } else if ( !blnContinue) {
            //single character cmds need to be returned as individual cmds
            switch (intChar
            case 13, 39, 40, 41, 44, 58, 59, 63, 91, 92, 93, 94, 96, 123, 125, 126 //  //(),:;?[\]^`{}~
              //single character tokens

            case 61 //=
              //special case; "=", "=<" and "=>" returned as separate tokens
              switch (Mid$(strText, lngTknPos + 1, 1)
              case "<", ">"
                //increment pointer
                lngTknPos = lngTknPos + 1

              case "=" //"=="
                //increment pointer
                lngTknPos = lngTknPos + 1

              }

            case 43 //+
              //special case; "+", "++" and "+=" returned as separate tokens
             if (Mid$(strText, lngTknPos + 1, 1) = "+" || Mid$(strText, lngTknPos + 1, 1) = "=") {
                //increment pointer
                lngTknPos = lngTknPos + 1
              }

            case 45 //-
              //special case; "-", "--" and "-=" returned as separate tokens
             if (Mid$(strText, lngTknPos + 1, 1) = "-" || Mid$(strText, lngTknPos + 1, 1) = "=") {
                //increment pointer
                lngTknPos = lngTknPos + 1
              }

            case 33 //!
              //special case; "!" and "!=" returned as separate tokens
             if (Mid$(strText, lngTknPos + 1, 1) = "=") {
                //increment pointer
                lngTknPos = lngTknPos + 1
              }

            case 60 //<
              //special case; "<", "<=" and "<>" returned as separate tokens
             if (Mid$(strText, lngTknPos + 1, 1) = "=" || Mid$(strText, lngTknPos + 1, 1) = ">") {
                //increment pointer
                lngTknPos = lngTknPos + 1
              }

            case 62 //>
              //special case; ">", ">=" and "><" returned as separate tokens
             if (Mid$(strText, lngTknPos + 1, 1) = "=" || Mid$(strText, lngTknPos + 1, 1) = "<") {
                //increment pointer
                lngTknPos = lngTknPos + 1
              }

            case 42 //*
              //special case; "*" and "*=" returned as separate tokens;
             if (Mid$(strText, lngTknPos + 1, 1) = "=") {
                //increment pointer
                lngTknPos = lngTknPos + 1
              }

            case 47 ///
              //special case; "/", "/*" and "/=" returned as separate tokens
             if (Mid$(strText, lngTknPos + 1, 1) = "=") {
                lngTknPos = lngTknPos + 1
              }
             if (Mid$(strText, lngTknPos + 1, 1) = "*") {
                lngTknPos = lngTknPos + 1
              }

            case 124 //|
              //special case; "|" and "||" returned as separate tokens
             if (Mid$(strText, lngTknPos + 1, 1) = "|") {
                //increment pointer
                lngTknPos = lngTknPos + 1
              }

            case 38 //&
              //special case; "&" and "&&" returned as separate tokens
             if (Mid$(strText, lngTknPos + 1, 1) = "&") {
                //increment pointer
                lngTknPos = lngTknPos + 1
              }

            default:
              //continue adding characters until element separator or EOL is reached
              Do
                //increment position
                lngTknPos = lngTknPos + 1
                //check for end of line
               if (lngTknPos > lngLen) {
                  //end  reached
                  lngTknPos = lngLen
                  //exit
                  break;
                }

                //get next character
                intChar = Asc(Mid$(strText, lngTknPos, 1))

                switch (intChar
                case 13, 32, 33, 34, 38 To 45, 47, 58 To 63, 91 To 94, 96, 123 To 126
                  //  space, !"&//()*+,-/:;<=>?[\]^`{|}~
                  //end of token text found;
                  //readjust lngpos
                  lngTknPos = lngTknPos - 1
                  break;
                }
              Loop While true
            }
          }

          //if no continuation
          if (!blnContinue) {
            //get the cmd for comparison
            strToken = Mid$(strText, FindNextToken, lngTknPos - FindNextToken + 1)

            //if not searching for a specific cmd
           if (blnNextToken) {
              //if only valid cmds
             if (ValidCmds) {
                //skip comments and cr
                switch (Left$(strToken, 2)
                case "[", "//", Keys.Enter
                default:
                  //return the cmd that was found
                  strFindCmd = strToken
                 if (UpdatePos) {
                    lngPos = lngTknPos
                  }
                  Exit Function
                }
              } else {
                //return the cmd that was found
                strFindCmd = strToken
               if (UpdatePos) {
                  lngPos = lngTknPos
                }
                Exit Function
              }
            }

            //if searching for a LE marker,
           if (StrComp(strFindCmd, "##LE", StringComparison.OrdinalIgnoreCase) = 0) {
              //if the comment is a layout editor exit marker
             if (lngTknPos - FindNextToken = 8) {
                strToken = Mid$(strText, FindNextToken, 4)
               if (strToken = "##LE") {
                  //found
                 if (UpdatePos) {
                    lngPos = lngTknPos
                  }
                  Exit Function
                }
              }
            }

            //does this cmd match the search?
           if (LenB(strToken) = LenB(strFindCmd)) {
             if (StrComp(strToken, strFindCmd, StringComparison.OrdinalIgnoreCase) = 0) {
                //this is it
               if (UpdatePos) {
                  lngPos = lngTknPos
                }
                Exit Function
              }
            }
          } else {
            //reset flag
            blnContinue = false
          }
          Loop While true

          //loop should never be exited normally
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static bool CheckSnippet(ref string SnipText, int IndentAmt)

          //check all snippets; if a matchis found,
          //replace SnipText and return true

          int i, j;
          string strSnipName, strSnipValue;
          string[] strArgs;
           int lngArgCount, lngPos;
          string strNext;
          bool blnSnipOK, blnArgNext;

          On Error GoTo ErrHandler

          //if no snippets
          if (UBound(CodeSnippets()) = 0) {
          //just exit
          CheckSnippet = false
          Exit Function
          }

          //extract name and arguments (if any) from snippet text
          FindNextToken SnipText, lngPos, strSnipName, true

          //if there are arguments, lngPos will not be
          //equal to length of snippet text and next cmd will be "("
          if (lngPos < Len(SnipText)) {
          //if next cmd is "("
          FindNextToken SnipText, lngPos, strNext, true
          if (strNext = "(") {
            //now extract arguments, one after another
            blnArgNext = true
            Do
              strNext = ""
              FindNextToken SnipText, lngPos, strNext, true
              //if expecting an argument
             if (blnArgNext) {
                switch (strNext
                case ")"
                  //if it's closing parentheses AND at least one arg added
                 if (lngArgCount > 0) {
                    //last arg value missing; assume empty string
                    lngArgCount = lngArgCount + 1
                    ReDim Preserve strArgs(lngArgCount - 1)
                    strArgs(lngArgCount - 1) = ""
                  }
                  //make sure it's end of line
                  blnSnipOK = (lngPos >= Len(SnipText))
                  break;

                // if it's another comma
                case ","
                  //assume empty string argument
                  lngArgCount = lngArgCount + 1
                  ReDim Preserve strArgs(lngArgCount - 1)
                  strArgs(lngArgCount - 1) = ""
                  //still expecting an argument so don't
                  //change value of blnNextArg

                default:
                  //otherwise, add it as arg (anything goes!)
                  lngArgCount = lngArgCount + 1
                  ReDim Preserve strArgs(lngArgCount - 1)
                  strArgs(lngArgCount - 1) = strNext
                  //now expecting a comma or closing bracket
                  blnArgNext = false
                }

              } else {
                //next arg must be a comma or closing parenthesis
                //if it's closing parenthesis
               if (strNext = ")") {
                  //make sure it's end of line
                  blnSnipOK = (lngPos >= Len(SnipText))
                  break;
                }
                //otherise, if not a comma
               if (strNext != ",") {
                  //no good
                  break;
                }
                //after comma, argument is next
                blnArgNext = true
              }
            Loop Until lngPos >= Len(SnipText)
          }
          } else {
          //sniptext has no arguments, and is valid for checking
          blnSnipOK = true
          }

          //if not ok, just exit
          if (!blnSnipOK) {
          CheckSnippet = false
          Exit Function
          }

          For i = 1 To UBound(CodeSnippets())
          //if name matches
          if (CodeSnippets(i).Name = strSnipName && CodeSnippets(i).Type != -1) {
            //get value
            strSnipValue = CodeSnippets(i).Value

            //if any arguments, replace them
           if (lngArgCount > 0) {
              For j = 0 To lngArgCount - 1
                strSnipValue = Replace(strSnipValue, "%" + CStr(j + 1), strArgs(j))
              Next j
            }

            //if more than one line
           if (InStr(1, strSnipValue, Keys.Enter) > 0) {
              //insert tabs
              strSnipValue = Replace(strSnipValue, Keys.Enter, Keys.Enter + Space(IndentAmt))
            }
            //shouldn//t be any line feeds, but check, just in case
           if (InStr(1, strSnipValue, Keys.LineFeed) > 0) {
              //Debug.Assert false
              strSnipValue = Replace(strSnipValue, Keys.LineFeed, "")
            }

            SnipText = strSnipValue
            CheckSnippet = true
            Exit Function
          }
          Next i
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }


          int NextChar(string strText, string strCmd, ref int lngPos)
          //returns the ascii code for, and sets the position of the next
          //non-space character (tabs, and lf are considered //spaces//; a cr is not)

          int lngLen;

          On Error GoTo ErrHandler

          //if already at end of input
          lngLen = Len(strText)
          if (lngPos = lngLen) {
          //exit
          NextChar = 0
          lngPos = 0
          Exit Function
          }

          Do
          //first, increment position
          lngPos++;

          NextChar = AscW(Mid$(strText, lngPos, 1))
          switch (NextChar
          case 9, 10, 32
            //get next character
          default:
            //this is a non-space character
            Exit Function
          }

          //keep going, until end of input is reached
          Loop Until lngPos = lngLen

          //end reached; reset character and position
          NextChar = 0
          lngPos = 0
          Exit Function

          ErrHandler:
          Resume Next
          }
          public static bool ConcatenateString(ref string StringIn)

          //verify that the passed string is a valid string, including possible concatenation
          //string must be of the format "text" or "text1" "text2"

          bool blnInQuotes, blnEmbed;
          bool blnAmp;
          int lngPos;
           short intChar;
          string strIn, strOut;

          //start with input string
          strIn = Trim$(StringIn)

          //if nothing,
          if (Len(strIn) < 2) {
          //not valid
          ConcatenateString = false
          Exit Function
          }

          //starting char should be quote
          if (Asc(strIn) != 34) {
          //not valid
          ConcatenateString = false
          Exit Function
          }

          //begin check
          blnInQuotes = true
          lngPos = 2
          Do
          intChar = Asc(Mid$(strIn, lngPos, 1))

          //if not in quotes,
          if (!blnInQuotes) {
            //only spaces or another quote is allowed
            switch (intChar
            case 34
              //toggle flag
              blnInQuotes = true
            case 32
              //ignore blanks
            default:
              //error
              Exit Function
            }

          } else {
            //if a quote, need to ensure it is a real quote
           if (intChar = 34 && !blnEmbed) {
              //no longer in quotes
              blnInQuotes = false
              blnEmbed = false
            } else {
              //add the character
              strOut = strOut + ChrW$(intChar)
            }

            //check for embed code
            blnEmbed = (intChar = 92)
          }

          lngPos++;
          Loop Until lngPos > Len(StringIn)

          //should end up being NOT in quotes and NOT expecting ampersand
          if (blnInQuotes || blnAmp) {
          //error
          Exit Function
          }

          //return the concatenated string, with surrounding quotes
          StringIn = QUOTECHAR + strOut + QUOTECHAR
          ConcatenateString = true
          Exit Function

          ErrHandler:
          Resume Next
          }


          public static AGIExits ExtractExits(AGILogic ThisLogic)

          //analyzes a logic to find //new.room// commands
          //builds a new exits object that contains the
          //exit info for this logic
          //
          //NOTE: this analyzes an existing SAVED logic source; not
          //a source that is being edited
          //
          //
          //if the exit id for an exit is new or has changed,
          //the source code is updated, and SAVED
          //
          //transpt info is not addressed by the extractexits method
          //the calling method must deal with transpts on its own

          bool blnLogLoad;
          string strLogic;
          AGIExits RoomExits;
           int lngCmdLoc;
          int i, j;
          string strLine;
           int lngID;
          bool blnIDOK, blnSave;
          AGIExit tmpExit;
           int lngLastPos;
          EEStatus tmpStatus;

          //lngCmdLoc is location of 1st character of 'new.room' command

          On Error GoTo ErrHandler

          //ensure source is loaded
          blnLogLoad = ThisLogic.Loaded
          if (!blnLogLoad) {
          ThisLogic.Load
          }

          //get source code
          strLogic = ThisLogic.SourceText

          //create new room object
          RoomExits = New AGIExits

          //locate first instance of new.room command
          lngCmdLoc = FindNextToken(strLogic, 0, "new.room")

          //loop through exit extraction until all new.room commands are processed
          Do Until lngCmdLoc = 0
          //get exit info
          tmpExit = AnalyzeExit(strLogic, lngCmdLoc, lngLastPos)

          //find end of line by searching for crlf
          j = InStr(lngCmdLoc, strLogic, Keys.Enter)
          //if no line end found, means we are on last line; set
          //j to a value of lngCmdLoc+1 so we get the last char of the line
          if (j = 0) { j = lngCmdLoc + 1
          //get rest of line (to check against existing exits)
          strLine = Trim$(Mid$(strLogic, lngCmdLoc, j - lngCmdLoc))

          //check line for a ##LE marker:
          //  first, strip off comment marker
          if (Left$(strLine, 1) = "[") {
            strLine = Trim$(Right$(strLine, Len(strLine) - 1))
          } else if ( Left$(strLine, 2) = "//") {
            strLine = Trim$(Right$(strLine, Len(strLine) - 2))
          }
          //  next, look for ##LE tag
          if (Left$(strLine, 4) = "##LE") {
            //strip off leader to expose exit id number
            strLine = Right$(strLine, Len(strLine) - 4)
            //note we can ignore the trailing //##// marker; Val function doesn't care if they are there
          } else {
            //not an exit marker; reset the string
            strLine = ""
          }

          //if a valid id Value was found
          if (LenB(strLine) != 0) {
            //an id may exist
            //assum ok until proven otherwise
            blnIDOK = true
            //get the id number
            lngID = CLng(Val(strLine))
            //if not a number (val=0) then no marker
           if (lngID = 0) {
              blnIDOK = false
            } else {
              //check for this marker among current exits
              For i = 0 To RoomExits.Count - 1
               if (Val(Right$(RoomExits(i).ID, 3)) = CStr(lngID)) {
                  //this ID has already been added by the editor;
                  //it needs to be reset
                  blnIDOK = false
                  Exit For
                }
              Next i
            }
          } else {
            //no previous marker; assign id automatically
            blnIDOK = false
          }

          //if previous ID needs updating (or one not found)
          if (!blnIDOK) {
            //get next available id number
            lngID = 0
            i = 0
            Do
              lngID = lngID + 1
              For i = 0 To RoomExits.Count - 1
               if (CLng(Right$(RoomExits(i).ID, 3)) = lngID) {
                  Exit For
                }
              Next i
            Loop Until i = RoomExits.Count
          }

          //exit is ok
          tmpStatus = esOK

          //add exit to logic, and flag as in game and ok
          RoomExits.Add(lngID, tmpExit.Room, tmpExit.Reason, tmpExit.Style).Status = tmpStatus

          //if id is new or changed,
          if (!blnIDOK) {
            //lngCmdLoc marks end of commands on this line

            //find end of line with //new.room// command
            j = InStr(lngCmdLoc, strLogic, ChrW$(13))
            //use end of logic, if on last line (i=0)
           if (j = 0) { j = Len(strLogic) + 1

            //insert exit info into logic source
            strLogic = Left$(strLogic, lngCmdLoc - 1) + " [ ##LE" + format$(lngID, "000") + "##" + Right$(strLogic, Len(strLogic) - j + 1)

            //set save flag
            blnSave = true
          }

          //get next new.room cmd
          lngLastPos = lngCmdLoc
          lngCmdLoc = FindNextToken(strLogic, lngCmdLoc, "new.room")
          Loop

          //if changes made to exit ids
          if (blnSave) {
          //replace sourcecode
          ThisLogic.SourceText = strLogic
          //remember that logics that are being edited appear to be
          //not in a game, which causes the ID to be changed to match the filename
          //since only ingame logics can be rebuilt, need to check for ID change,
          //and change it back

          //WTF is this about logic not being in game? I should be working with
          //the actual ingame logic, right?  OH, I remember - if this is called from
          //a logic editor, it's the copy of the object, not the object; at least
          //it was; I think it's now the actual game logic; need to check that

          //Debug.Assert ThisLogic Is Logics(ThisLogic.Number)
          //Debug.Assert ThisLogic.InGame

          if (!ThisLogic.InGame) {
            //save id
            strLogic = ThisLogic.ID
          }
          ThisLogic.SaveSource

          if (!ThisLogic.InGame) {
            ThisLogic.ID = strLogic
          }

          //now need to make sure tree is uptodate (and preview window, if this
          //logic happens to be the one being previewed)
          UpdateSelection rtLogic, ThisLogic.Number, umPreview || umProperty
          }

          //unload OR load if necessary
          if (!blnLogLoad && ThisLogic.Loaded) {
          ThisLogic.Unload
          } else if ( blnLogLoad && !ThisLogic.Loaded) {
          ThisLogic.Load
          }

          //return the new exit list
          ExtractExits = RoomExits
          Exit Function

          ErrHandler:
          Resume Next
          }


          public static string ReplaceGlobal(string TextIn)

          //open the global file; check for existence of this item;
          //if found, replace it with correct Value;
          //if not found, return original Value

          string strLine;
            string[] strSplitLine;

          //default to original Value
          ReplaceGlobal = TextIn

          //check for existence of globals file
          if (!File.Exists(GameDir + "globals.txt")) {
          //no global list
          Exit Function
          }

          //open the list
          On Error GoTo ErrHandler

          intFile = FreeFile

          Open GameDir + "globals.txt"

          Do Until EOF(intFile)
          Line Input #intFile, strLine
          //trim it
          strLine = Trim$(strLine)

          //ignore blanks and comments (// or [)
          if (LenB(strLine) != 0 && Left$(strLine, 2) != "//" && Left$(strLine, 1) != ChrW$(91)) {
            //splitline into name and Value
            strSplitLine = Split(strLine, Keys.Tab)

            //if exactly two elements,
           if (UBound(strSplitLine) = 1) {
              //check it against input
             if (TextIn = Trim$(strSplitLine(0))) {
                ReplaceGlobal = Trim$(strSplitLine(1))
                Close intFile
                Exit Function
              }
            }
          }
          Loop

          //not found; exit
          Close intFile

          Exit Function

          ErrHandler:
          //just clear error and return input text
          Err.Clear
          }

          public static void ExportAll()

          //exports all logic, picture, sound and view resources into the resource directory
          //overwriting where necessary

          AGILogic tmpLog;
           AGIPicture tmpPic;
          AGISound tmpSnd;
           AGISound tmpView;
          bool blnLoaded;
          bool blnRepeat;
           DialogResult rtn;

          On Error GoTo ErrHandler

          //disable main form while exporting
          MDIMain.Enabled = false
          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //use ProgressWin form to track progress
          Load ProgressWin
          ProgressWin.Text = "Exporting All Resources"
          ProgressWin.pgbStatus.Max = Logics.Count + Pictures.Count + Sounds.Count + Views.Count
          ProgressWin.pgbStatus.Value = 0
          ProgressWin.lblProgress = "Exporting..."
          ProgressWin.Show
          ProgressWin.Refresh

          foreach (tmpLog In Logics
            ProgressWin.lblProgress.Text = "Exporting " + tmpLog.ID
            ProgressWin.pgbStatus.Value++;
            ProgressWin.Refresh
            SafeDoEvents

            blnLoaded = tmpLog.Loaded
           if (!blnLoaded) {
              tmpLog.Load
            }

            //if sourcefile already exists,
           if (File.Exists(tmpLog.SourceFile)) {
              //file exists; ask if it should be replaced
             if (!blnRepeat) {
                MDIMain.UseWaitCursor = false;
                //ask if should be replaced?
                rtn = MsgBoxEx("A source code file for " + tmpLog.ID + " already exists." + Environment.NewLine + "Do you want to replace it with the decompiled logic source?", MessageBoxIcon.Question + MessageBoxButtons.YesNo, "Export Logic", , , "Repeat this answer for all remaining logic resources", blnRepeat)
                Screen.MousePointer = vbHourglass
              }

              //if replacing (rtn = DialogResult.Yes)
             if (rtn = DialogResult.Yes) {
                //kill existing file
                Kill tmpLog.SourceFile
                //now save it
                tmpLog.SaveSource
              }
            } else {
              //export source
              tmpLog.SaveSource
            }

            //now export the compiled logic
            tmpLog.Export ResDir + tmpLog.ID + ".agl"

           if (!blnLoaded) {
              tmpLog.Unload
            }
          Next

          foreach (tmpPic In Pictures
            ProgressWin.lblProgress.Text = "Exporting " + tmpPic.ID
            ProgressWin.pgbStatus.Value++;
            ProgressWin.Refresh
            SafeDoEvents

            blnLoaded = tmpPic.Loaded
           if (!blnLoaded) {
              tmpPic.Load
            }
            tmpPic.Export ResDir + tmpPic.ID + ".agp"
           if (!blnLoaded) {
              tmpPic.Unload
            }
          Next

          foreach (tmpSnd In Sounds
            ProgressWin.lblProgress.Text = "Exporting " + tmpSnd.ID
            ProgressWin.pgbStatus.Value++;
            ProgressWin.Refresh
            SafeDoEvents

            blnLoaded = tmpSnd.Loaded
           if (!blnLoaded) {
              tmpSnd.Load
            }
            tmpSnd.Export ResDir + tmpSnd.ID + ".ags"
           if (!blnLoaded) {
              tmpSnd.Unload
            }
          Next

          foreach (tmpView In Views
            ProgressWin.lblProgress.Text = "Exporting " + tmpView.ID
            ProgressWin.pgbStatus.Value++;
            ProgressWin.Refresh
            SafeDoEvents

            blnLoaded = tmpView.Loaded
           if (!blnLoaded) {
              tmpView.Load
            }
            tmpView.Export ResDir + tmpView.ID + ".agv"
           if (!blnLoaded) {
              tmpView.Unload
            }
          Next

          //done with ProgressWin form
          Unload ProgressWin

          //reenable main form
          MDIMain.Enabled = true
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //ignore the system bitmap error for pictures, since it doesn't affect exporting
          if (Err.Number = WINAGI_ERR + 610) {
          Err.Clear
          Resume Next
          }

          //Debug.Assert false
          Resume Next
          }

          public static bool ExportObjects(AGIInventoryObjects ThisObjectsFile, bool InGame)

          string strFileName;
          DialogResult rtn;

          On Error GoTo ErrHandler

          //set up commondialog
          if (InGame) {
            MDIMain.SaveDlg.DialogTitle = "Export Inventory Objects File"
          } else {
            MDIMain.SaveDlg.DialogTitle = "Save Inventory Objects File As"
          }
          //if objectlist has a filename
          if (LenB(ThisObjectsFile.ResFile) != 0) {
            //use it
            MDIMain.SaveDlg.FullName = ThisObjectsFile.ResFile
          } else {
            //use default name
            MDIMain.SaveDlg.FullName = ResDir + "OBJECT"
          }
          .Filter = "WinAGI Inventory Objects Files (*.ago)|*.ago|OBJECT file|OBJECT|All files (*.*)|*.*"
          if (LCase$(Right$(.FullName, 4)) = ".ago") {
            MDIMain.SaveDlg.FilterIndex = 1
          } else {
            MDIMain.SaveDlg.FilterIndex = 2
          }
          MDIMain.SaveDlg.DefaultExt = ""
          MDIMain.SaveDlg.Flags = cdlOFNHideReadOnly || cdlOFNPathMustExist || cdlOFNExplorer
          MDIMain.SaveDlg.hWndOwner = MDIMain.hWnd

          Do
          On Error Resume Next
          MDIMain.SaveDlg.ShowSaveAs
          //if canceled,
          if (Err.Number = cdlCancel) {
            //exit without doing anything
            Exit Function
          }
          On Error GoTo ErrHandler

          //get filename
          strFileName = MDIMain.SaveDlg.FullName

          //if file exists,
          if (File.Exists(strFileName)) {
            //verify replacement
            rtn = MessageBox.Show(MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?", MessageBoxButtons.YesNoCancel + MessageBoxIcon.Question, "Overwrite file?")

           if (rtn = DialogResult.Yes) {
              break;
            } else if ( rtn = DialogResult.Cancel) {
              Exit Function
            }
          } else {
            break;
          }
          Loop While true

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          if (LCase$(Right$(MDIMain.SaveDlg.FullName, 4)) = ".ago") {
          //export with new filename
          ThisObjectsFile.Export strFileName, 1, !InGame
          } else {
          //export with new filename
          ThisObjectsFile.Export strFileName, 0, !InGame
          }

          //reset mouse pointer
          MDIMain.UseWaitCursor = false;

          //if error,
          if (Err.Number != 0) {
          ErrMsgBox("An error occurred while exporting this file:", "", "Export File Error"
          }

          ExportObjects = true
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }
          public static bool MakePicGif(AGIPicture GifPic, GifOptions GifOps, string ExportFile)

          string strTempFile;
          byte[] bytData;
           int lngPos; //data that will be written to the gif file
          int lngInPos;
           byte[] bytCmpData, bytPicData; //data used to build then compress pic data as gif Image
          byte[] bytFrameData;
          int lngPicPos;
           byte bytCmd;
          bool blnXYDraw, blnVisOn;

          short i, j;

          const int MaxH = 168;
          const int MaxW = 160;

          byte pX, pY;
          short zFacH, zFacW;
          int lngFramePos;
           byte bytTrans;

          //to use the aspect ratio property, set logical screen size to
          // 160 x 168, then set aspect ratio to 113; for each frame
          // added, set the size to 320 x 168 but only use the 160x168
          // data array - it works, but it also looks a little //fuzzy//
          // when viewed at small scales

          short intChunkSize;

          //use error handler to expand size of data array when it gets full
          On Error GoTo ErrHandler

          //build header
          ReDim bytData[255)
          bytData[0) = 71
          bytData[1) = 73
          bytData[2) = 70
          bytData[3) = 56
          bytData[4) = 57
          bytData[5) = 97

          //add logical screen size info
          bytData[6) = (byte)(MaxW * GifOps.Zoom * 2) & 0xFF)
          bytData[7) = (byte)(MaxW * GifOps.Zoom * 2) >> 8)
          bytData[8) = (byte)(MaxH * GifOps.Zoom) & 0xFF)
          bytData[9) = (byte)(MaxH * GifOps.Zoom) >> 8)
          //add color info
          bytData[10) = 243 //1-111-0-011 means:
                          //global color table,
                          //8 bits per channel,
                          //no sorting, and
                          //16 colors in the table
          //background color:
          bytData[11) = 0
          //pixel aspect ratio:
          //////  bytData[12) = 113 // (113+15)/64 = 2:1 ratio for pixels
          bytData[12) = 0 // no aspect ratio

          //add global color table
          For i = 0 To 15
          bytData[13 + 3 * i) = EGAColorLong(i) && 0xFF
          bytData[14 + 3 * i) = (EGAColorLong(i) && 0xFF00) >> 8
          bytData[15 + 3 * i) = (EGAColorLong(i) && 0xFF0000) >> 16
          Next i

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

          bytData[61) = 0x21
          bytData[62) = 0xFF
          bytData[63) = 0xB
          For i = 1 To 11
            bytData[i + 63) = Asc(Mid("NETSCAPE2.0", i, 1))
          Next i
          bytData[75) = 3
          bytData[76) = 1
          bytData[77) = 0 //255
          bytData[78) = 0 //255
          bytData[79) = 0

          //at this point, numbering is not absolute, so we need to begin tracking the data position
          lngPos = 80
          } else {
          //at this point, numbering is not absolute, so we need to begin tracking the data position
          lngPos = 61
          }

          //pic data array
          ReDim bytPicData(MaxH * MaxW * GifOps.Zoom ^ 2 * 2 - 1)

          //add each frame
          lngPicPos = -1
          Do
          Do
            //increment data pointer
            lngPicPos = lngPicPos + 1

            //if end is reached
           if (lngPicPos >= GifPic.Size) {
              break;
            }

            //get the value at this pos, and determine if it's
            //a draw command
            bytCmd = GifPic.Data(lngPicPos)

            switch (bytCmd
            case 240
              blnXYDraw = false
              blnVisOn = true
              lngPicPos = lngPicPos + 1
            case 241
              blnXYDraw = false
              blnVisOn = false
            case 242
              blnXYDraw = false
              lngPicPos = lngPicPos + 1
            case 243
              blnXYDraw = false
            case 244, 245
              blnXYDraw = true
              lngPicPos = lngPicPos + 2
            case 246, 247, 248, 250
              blnXYDraw = false
              lngPicPos = lngPicPos + 2
            case 249
              blnXYDraw = false
              lngPicPos = lngPicPos + 1
            default:
              //skip second coordinate byte, unless
              //currently drawing X or Y lines
             if (!blnXYDraw) {
                lngPicPos = lngPicPos + 1
              }
            }

          // exit if non-pen cmd found, and vis pen is active
          Loop While bytCmd >= 240 && bytCmd <= 244 || bytCmd = 249 || !blnVisOn

          //if end is reached
          if (lngPicPos >= GifPic.Size) {
            break;
          }

          //show pic drawn up to this point
          GifPic.DrawPos = lngPicPos
          bytFrameData = GifPic.VisData

          //add graphic control extension for this frame
          bytData[lngPos) = 0x21
              lngPos++;
          bytData[lngPos) = 0xF9
              lngPos++;
          bytData[lngPos) = 4
              lngPos++;
          bytData[lngPos) = 12   //000-011-0-0 = reserved-restore-no user input-no transparency
              lngPos++;
          bytData[lngPos) = GifOps.Delay && 0xFF
              lngPos++;
          bytData[lngPos) = (GifOps.Delay && 0xFF) >> 8
              lngPos++;
          bytData[lngPos) = 0
              lngPos++;
          bytData[lngPos) = 0
              lngPos++;

          //add the frame data (first create frame data in separate array
          //then compress the frame data, break it into 255 byte chunks,
          //and add the chunks to the output

          lngFramePos = 0

          For pY = 0 To MaxH - 1
            //repeat each row based on scale factor
            For zFacH = 1 To GifOps.Zoom
              //step through each pixel in this row
              For pX = 0 To MaxW - 1
                //repeat each pixel based on scale factor (x2 because AGI pixels are double-wide)
                For zFacW = 1 To GifOps.Zoom * 2
                  //use the actual pixel (adjusted for padding, if aligned to bottom or left)
                  bytPicData(lngFramePos) = bytFrameData(pX + pY * 160)
                  lngFramePos = lngFramePos + 1
                Next zFacW
              Next pX
            Next zFacH
          Next pY

          //now compress the pic data
          bytCmpData() = GifLZW(bytPicData())

          //add Image descriptor
          bytData[lngPos) = 0x2C
          lngPos++;
          bytData[lngPos) = 0
          lngPos++;
          bytData[lngPos) = 0
          lngPos++;
          bytData[lngPos) = 0
          lngPos++;
          bytData[lngPos) = 0
          lngPos++;

          bytData[lngPos) = (byte)(MaxW * GifOps.Zoom * 2) && 0xFF)
          lngPos++;
          bytData[lngPos) = (byte)(MaxW * GifOps.Zoom * 2) >> 8)
          lngPos++;

          bytData[lngPos) = (byte)(MaxH * GifOps.Zoom) && 0xFF)
          lngPos++;
          bytData[lngPos) = (byte)(MaxH * GifOps.Zoom) >> 8)
          lngPos++;
          bytData[lngPos) = 0
          lngPos++;
          //add byte for initial LZW code size
          bytData[lngPos) = 4 //5
          lngPos++;

          //add the compressed data to filestream
          lngInPos = 0
          intChunkSize = 0

          Do
           if (UBound(bytCmpData()) + 1 - lngInPos > 255) {
              intChunkSize = 255
            } else {
              intChunkSize = UBound(bytCmpData()) + 1 - lngInPos
            }
            //write chunksize
            bytData[lngPos) = (byte)intChunkSize)
            lngPos++;

            //add this chunk of data
            For j = 1 To intChunkSize
              bytData[lngPos) = bytCmpData(lngInPos)
              lngPos++;
              lngInPos = lngInPos + 1
            Next j
          Loop Until lngInPos >= UBound(bytCmpData())

          //end with a zero-length block
          bytData[lngPos) = 0
          lngPos++;

          //update progress
          ProgressWin.pgbStatus.Value = lngPicPos
          Debug.Print ProgressWin.pgbStatus.Value; "/"; ProgressWin.pgbStatus.Max
          SafeDoEvents

          Loop Until lngPicPos >= GifPic.Size

          //add trailer
          bytData[lngPos) = 0x3B

          ReDim Preserve bytData[lngPos)

          //now write file to output
          On Error Resume Next

          //get temporary file
          strTempFile = Path.GetTempFileName()

          //open file for output
          intFile = FreeFile()
          Open strTempFile
          //write data
          Put intFile, , bytData

          //if error,
          if (Err.Number != 0) {
          //close file
          Close intFile
          //erase the temp file
          Kill strTempFile
          //return error condition
          //Debug.Assert false
          Exit Function
          }

          //close file,
          Close intFile
          //if savefile exists
          if (File.Exists(ExportFile)) {
          //delete it
          Kill ExportFile
          Err.Clear
          }

          //copy tempfile to savefile
          FileCopy strTempFile, ExportFile

          //if error,
          if (Err.Number != 0) {
          //close file
          Close intFile
          //erase the temp file
          Kill strTempFile
          //return error condition
          //Debug.Assert false
          Exit Function
          }

          //delete temp file
          Kill strTempFile
          Err.Clear

          MakePicGif = true
          Exit Function

          ErrHandler:
          //if the error is because of trying to write to an array that is full,
          //expand it by 64K then try again
          if (Err.Number = 9) {
          //make sure it's because lngPos is past array boundary
          if (lngPos > UBound(bytData[))) {
            ReDim Preserve bytData[UBound(bytData[)) + 65536)
            Resume
          } else {
            //Debug.Assert false
            Resume Next
          }
          }

          //Debug.Assert false
          Resume Next
          }



          static void ExportPicGIF(byte[] ImgData(), string ExportFile, int Zoom = 1)

          //no longer needed, but i dont want to lose this code; it's a working gif generator
          //

          ////  //MUST specify a valid export file
          ////  //if export file already exists, it is overwritten
          ////  //caller is responsible for verifying overwrite is ok or not
          ////
          ////  //assumption is that calling function won//t pass invalid filename, or call
          ////  //this function if picture doesn't have valid data
          ////
          ////  int i, j, zh, zv;
          ////  string strTempFile;
          ////  byte[] bytData;
          ////  int lngPos; //data that will be written to the gif file
          ////  int lngInPos;
          ////  byte[] bytCmpData, bytImgData; //data used to build then compress img data as gif Image
          ////  int lngImgPos;
          ////  short intChunkSize;
          ////
          ////  //use error handler to expand size of data array when it gets full
          ////  On Error GoTo ErrHandler
          ////
          ////  //build header
          ////  ReDim bytData[255)
          ////  bytData[0) = 71
          ////  bytData[1) = 73
          ////  bytData[2) = 70
          ////  bytData[3) = 56
          ////  bytData[4) = 57
          ////  bytData[5) = 97
          ////
          ////  //add logical screen size info
          ////  bytData[6) = (byte)(320 * Zoom) && 0xFF)
          ////  bytData[7) = (byte)(320 * Zoom) >> 8)
          ////  bytData[8) = (byte)(168 * Zoom) && 0xFF)
          ////  bytData[9) = (byte)(168 * Zoom) >> 8)
          ////  //add color info
          ////  bytData[10) = 243 //1-111-0-011 means:
          ////                    //global color table,
          ////                    //8 bits per channel,
          ////                    //no sorting, and
          ////                    //16 colors in the table
          ////  //background color:
          ////  bytData[11) = 0
          ////  //pixel aspect ratio:
          ////  bytData[12) = 0 //113  //should give proper 2:1 ratio for pixels
          ////
          ////  //add global color table
          ////  For i = 0 To 15
          ////    bytData[13 + 3 * i) = EGAColorLong(i) && 0xFF
          ////    bytData[14 + 3 * i) = (EGAColorLong(i) && 0xFF00) >> 8
          ////    bytData[15 + 3 * i) = (EGAColorLong(i) && 0xFF0000) >> 16
          ////  Next i
          ////
          ////  //at this point, numbering is not absolute, so we need to begin tracking the data position
          ////  lngPos = 61
          ////
          ////  //add graphic control extension for this cel
          ////  bytData[lngPos) = 0x21
          ////  bytData[lngPos + 1) = 0xF9
          ////  bytData[lngPos + 2) = 4
          ////  bytData[lngPos + 3) = 12  //000-011-0-0 = reserved-restore-no user input-transparency included
          ////  bytData[lngPos + 4) = 0
          ////  bytData[lngPos + 5) = 0
          ////  bytData[lngPos + 6) = 0
          ////  bytData[lngPos + 7) = 0
          ////
          ////  lngPos = lngPos + 8
          ////
          ////  //img size is set to logical screen size
          ////  ReDim bytImgData(53760 * Zoom ^ 2 - 1)
          ////
          ////  //add the img data (first create cel data in separate array
          ////  //then compress the it, break it into 255 byte chunks,
          ////  //and add the chunks to the output
          ////
          ////  lngImgPos = 0
          ////
          ////  For j = 0 To 167
          ////    //repeat each row based on scale factor
          ////    For zv = 1 To Zoom
          ////      //step through each pixel in this row
          ////      For i = 0 To 159
          ////        //repeat each pixel based on scale factor (x2 because AGI pixels are double-wide)
          ////        For zh = 1 To Zoom * 2
          ////          bytImgData(lngImgPos) = ImgData(i + j * 160)
          ////          lngImgPos = lngImgPos + 1
          ////        Next zh
          ////      Next i
          ////    Next zv
          ////  Next j
          ////
          ////  //now compress the cel data
          ////  bytCmpData() = GifLZW(bytImgData())
          ////
          ////  //add Image descriptor
          ////  bytData[lngPos) = 0x2C
          ////  bytData[lngPos + 1) = 0
          ////  bytData[lngPos + 2) = 0
          ////  bytData[lngPos + 3) = 0
          ////  bytData[lngPos + 4) = 0
          ////  bytData[lngPos + 5) = (byte)(320 * Zoom) && 0xFF)
          ////  bytData[lngPos + 6) = (byte)(320 * Zoom) >> 8)
          ////  bytData[lngPos + 7) = (byte)(168 * Zoom) && 0xFF)
          ////  bytData[lngPos + 8) = (byte)(168 * Zoom) >> 8)
          ////  bytData[lngPos + 9) = 0
          ////  //add byte for initial LZW code size
          ////  bytData[lngPos + 10) = 4 //5
          ////  //adjust pointer
          ////  lngPos++;1
          ////
          ////  //add the compressed data to filestream
          ////  lngInPos = 0
          ////  intChunkSize = 0
          ////
          ////  Do
          ////   if (UBound(bytCmpData()) + 1 - lngInPos > 255) {
          ////      intChunkSize = 255
          ////    } else {
          ////      intChunkSize = UBound(bytCmpData()) + 1 - lngInPos
          ////    }
          ////    //write chunksize
          ////    bytData[lngPos) = (byte)intChunkSize)
          ////    lngPos++;
          ////
          ////    //add this chunk of data
          ////    For i = 1 To intChunkSize
          ////      bytData[lngPos) = bytCmpData(lngInPos)
          ////      lngPos++;
          ////      lngInPos = lngInPos + 1
          ////    Next i
          ////  Loop Until lngInPos >= UBound(bytCmpData())
          ////
          ////  //end with a zero-length block
          ////  bytData[lngPos) = 0
          ////  lngPos++;
          ////
          ////  //add trailer
          ////  bytData[lngPos) = 0x3B
          ////
          ////  ReDim Preserve bytData[lngPos)
          ////
          ////  //now write file to output
          ////  On Error Resume Next
          ////
          ////  //get temporary file
          ////  strTempFile = Path.GetTempFileName()
          ////
          ////  //open file for output
          ////  intFile = FreeFile()
          ////  Open strTempFile
          ////  //write data
          ////  Put intFile, , bytData
          ////
          ////  //if error,
          //// if (Err.Number != 0) {
          ////    //close file
          ////    Close intFile
          ////    //erase the temp file
          ////    Kill strTempFile
          ////    //return error condition
          ////    //Debug.Assert false
          ////    return;
          ////  }
          ////
          ////  //close file,
          ////  Close intFile
          ////  //if savefile exists
          //// if (File.Exists(ExportFile)) {
          ////    //delete it
          ////    Kill ExportFile
          ////    Err.Clear
          ////  }
          ////
          ////  //copy tempfile to savefile
          ////  FileCopy strTempFile, ExportFile
          ////
          ////  //if error,
          //// if (Err.Number != 0) {
          ////    //close file
          ////    Close intFile
          ////    //erase the temp file
          ////    Kill strTempFile
          ////    //return error condition
          ////    On Error GoTo 0: Err.Raise WINAGI_ERR + 584, "ExportPicGIF", "unable to export GIF file"
          ////  }
          ////
          ////  //delete temp file
          ////  Kill strTempFile
          ////  Err.Clear
          ////
          ////return;
          ////
          ////ErrHandler:
          ////  //if the error is because of trying to write to an array that is full,
          ////  //expand it by 1K then try again
          //// if (Err.Number = 9) {
          ////    ReDim Preserve bytData[UBound(bytData[)) + 1024)
          ////    Resume
          ////  }
          ////
          ////  //Debug.Assert false
          ////  //unspecified error...
          ////  //make sure file is closed
          ////  Close intFile
          ////  //erase the temp file
          ////  Kill strTempFile
          ////  //return error condition
          ////  On Error GoTo 0: Err.Raise WINAGI_ERR + 585, "ExportPicGIF", "unhandled error in ExportPicGIF"
          }

          public static bool ExportPicture(AGIPicture ThisPicture, bool InGame)

          //exports this picture
          //either as an AGI picture resource, bitmap, *gif or a *jpeg Image

          string strFileName;
            bool ExportImage;
          bool blnCanceled;
           DialogResult rtn;
          int lngZoom, lngMode, lngFormat;

          On Error GoTo ErrHandler

          //user chooses an export type
          Load frmPicExpOptions

          frmPicExpOptions.SetForm 0
          frmPicExpOptions.ShowDialog(MDIMain);

          ExportImage = frmPicExpOptions.optImage.Value
          blnCanceled = frmPicExpOptions.Canceled
          lngZoom = CLng(frmPicExpOptions.txtZoom.Text)
          lngFormat = frmPicExpOptions.cmbFormat.SelectedIndex + 1
          if (frmPicExpOptions.optVisual.Value) {
            lngMode = 0
          } else if ( frmPicExpOptions.optPriority.Value) {
            lngMode = 1
          } else {
            lngMode = 2
          }

          //done with the options form
          Unload frmPicExpOptions

          //if canceled
          if (blnCanceled) {
          //just exit
          Exit Function
          }

          //if exporting an image
          if (ExportImage) {
          //get a filename
          //set up commondialog
            MDIMain.SaveDlg.DialogTitle = "Save Picture Image As"
            MDIMain.SaveDlg.DefaultExt = "bmp"
           if (NoGDIPlus) {
              MDIMain.SaveDlg.Filter = "BMP files (*.bmp)|*.bmp|All files (*.*)|*.*"
            } else {
              MDIMain.SaveDlg.Filter = "BMP files (*.bmp)|*.bmp|JPEG files (*.jpg)|*.jpg|GIF files (*.gif)|*.gif|TIFF files (*.tif)|*.tif|PNG files (*.PNG)|*.png|All files (*.*)|*.*"
            }
            MDIMain.SaveDlg.Flags = cdlOFNHideReadOnly || cdlOFNPathMustExist || cdlOFNExplorer
            MDIMain.SaveDlg.FilterIndex = lngFormat
            MDIMain.SaveDlg.FullName = ""
            MDIMain.SaveDlg.hWndOwner = MDIMain.hWnd

          On Error Resume Next
          Do
            MDIMain.SaveDlg.ShowSaveAs
            //if canceled,
           if (Err.Number = cdlCancel) {
              //exit without doing anything
              Exit Function
            }

            //if file exists,
           if (File.Exists(MDIMain.SaveDlg.FullName)) {
              //verify replacement
              rtn = MessageBox.Show(MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?", MessageBoxButtons.YesNoCancel + MessageBoxIcon.Question, "Overwrite file?")

             if (rtn = DialogResult.Yes) {
                break;
              } else if ( rtn = DialogResult.Cancel) {
                Exit Function
              }
            } else {
              break;
            }
          Loop While true
          On Error GoTo ErrHandler

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //show ProgressWin form
          Load ProgressWin
            ProgressWin.Text = "Exporting Picture Image"
            ProgressWin.lblProgress = "Depending on export size, this may take awhile. Please wait..."
            ProgressWin.pgbStatus.Visible = false
            ProgressWin.Show
            ProgressWin.Refresh

          ExportImg ThisPicture, MDIMain.SaveDlg.FullName, lngFormat, lngMode, lngZoom

          //all done!
          Unload ProgressWin
          MessageBox.Show("Success!", MessageBoxIcon.Information + MessageBoxButtons.OK, "Export Picture Image"

          MDIMain.UseWaitCursor = false;
          Exit Function
          } else {
          // export the agi resource

          //set up commondialog
           if (InGame) {
              MDIMain.SaveDlg.DialogTitle = "Export Picture"
            } else {
              MDIMain.SaveDlg.DialogTitle = "Save Picture As"
            }
            MDIMain.SaveDlg.Filter = "WinAGI Picture Files (*.agp)|*.agp|All files (*.*)|*.*"
            MDIMain.SaveDlg.FilterIndex = GameSettings.GetSetting(sPICTURES, sEXPFILTER, 1)
            switch (MDIMain.SaveDlg.FilterIndex) {
            case 1
              MDIMain.SaveDlg.DefaultExt = "agp"
            }

            MDIMain.SaveDlg.Flags = cdlOFNHideReadOnly || cdlOFNPathMustExist || cdlOFNExplorer

            //if Picture has a filename
           if (LenB(ThisPicture.ResFile) != 0) {
              //use it
              MDIMain.SaveDlg.FullName = ThisPicture.ResFile
            } else {
              //use default name
              MDIMain.SaveDlg.FullName = ResDir + ThisPicture.ID
              switch (.FilterIndex
              case 1
                MDIMain.SaveDlg.FullName = MDIMain.SaveDlg.FullName + ".agp"
              }
            }
            MDIMain.SaveDlg.hWndOwner = MDIMain.hWnd

          Do
            On Error Resume Next
            MDIMain.SaveDlg.ShowSaveAs
            //if canceled,
           if (Err.Number = cdlCancel) {
              //exit without doing anything
              Exit Function
            }
            On Error GoTo ErrHandler
            //save filterindex
            WriteSetting GameSettings, sPICTURES, sEXPFILTER, MDIMain.SaveDlg.FilterIndex
            //get filename
            strFileName = MDIMain.SaveDlg.FullName

            //if file exists,
           if (File.Exists(strFileName)) {
              //verify replacement
              rtn = MessageBox.Show(MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?", MessageBoxButtons.YesNoCancel + MessageBoxIcon.Question, "Overwrite file?")

             if (rtn = DialogResult.Yes) {
                break;
              } else if ( rtn = DialogResult.Cancel) {
                Exit Function
              }
            } else {
              break;
            }
          Loop While true

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //export the resource as agi picture
          ThisPicture.Export strFileName, !InGame

          //if error,
          if (Err.Number != 0) {
            ErrMsgBox("An error occurred while exporting this file.", "", "Export File Error"
          }

          //reset mouse pointer
          MDIMain.UseWaitCursor = false;
          }

          ExportPicture = true
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }
          public static bool ExportSound(AGISound ThisSound, bool InGame)

          //exports this Sound

          string strFileName;
            int lngFilter;
          DialogResult rtn;
            int sFmt;
            SoundFormat eFmt;

          On Error GoTo ErrHandler

          //set up commondialog
          // strategy:
          //  .ags and .* available for all formats
          //  .mid for PC/PCjr and IIgs MIDI
          //  .wav only for IIgs PCM
          //  .ass only for PC/PCjr
          //
          // .wav and .mid are in same position so the
          // default will make sense when switching
          // between the two

          //many of the settings here depend on format of this sound
          sFmt = ThisSound.SndFormat

          if (InGame) {
            MDIMain.SaveDlg.DialogTitle = "Export Sound"
          } else {
            MDIMain.SaveDlg.DialogTitle = "Save Sound As"
          }
          switch (sFmt
          case 1
            MDIMain.SaveDlg.Filter = "WinAGI Sound Files (*.ags)|*.ags|MIDI Files (*.mid)|*.mid|AGI Sound Script Files (*.ass)|*.ass|All files (*.*)|*.*"
          case 2
            MDIMain.SaveDlg.Filter = "WinAGI Sound Files (*.ags)|*.ags|WAV Files (*.wav)|*.wav|All files (*.*)|*.*"
          case 3
            MDIMain.SaveDlg.Filter = "WinAGI Sound Files (*.ags)|*.ags|MIDI Files (*.mid)|*.mid|All files (*.*)|*.*"
          }

          //if Sound has a filename
          if (LenB(ThisSound.ResFile) != 0) {
            //use it
            MDIMain.SaveDlg.FullName = ThisSound.ResFile
          } else {
            //id should be filename
            MDIMain.SaveDlg.FullName = ResDir + ThisSound.ID
          }

          //set filter index (if it's the script option, change it to
          //default ags for non-PC/PCjr sounds
          lngFilter = GameSettings.GetSetting(sSOUNDS, sEXPFILTER, 1)
          switch (sFmt
          case 2, 3 //non-PC/PCjr
           if (lngFilter = 4) {
              lngFilter = 3
            }
          }
          MDIMain.SaveDlg.FilterIndex = lngFilter

          //now set default extension
          switch (.FilterIndex
          case 1
            MDIMain.SaveDlg.DefaultExt = "ags"
          case 2
           if (sFmt = 2) {
              MDIMain.SaveDlg.DefaultExt = "wav"
            } else {
              MDIMain.SaveDlg.DefaultExt = "mid"
            }
          case 3
           if (sFmt = 1) {
              MDIMain.SaveDlg.DefaultExt = "ass"
            } else {
              MDIMain.SaveDlg.DefaultExt = ""
            }
          default:
            MDIMain.SaveDlg.DefaultExt = ""
          }

          MDIMain.SaveDlg.Flags = cdlOFNHideReadOnly || cdlOFNPathMustExist || cdlOFNExplorer
          MDIMain.SaveDlg.hWndOwner = MDIMain.hWnd  

          Do
          On Error Resume Next
          MDIMain.SaveDlg.ShowSaveAs
          //if canceled,
          if (Err.Number = cdlCancel) {
            //exit without doing anything
            Exit Function
          }
          On Error GoTo ErrHandler

          //save default filter index
          WriteSetting GameSettings, sSOUNDS, sEXPFILTER, MDIMain.SaveDlg.FilterIndex

          //get filename
          strFileName = MDIMain.SaveDlg.FullName

          //if file exists,
          if (File.Exists(strFileName)) {
            //verify replacement
            rtn = MessageBox.Show(MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?", MessageBoxButtons.YesNoCancel + MessageBoxIcon.Question, "Overwrite file?")

           if (rtn = DialogResult.Yes) {
              break;
            } else if ( rtn = DialogResult.Cancel) {
              Exit Function
            }
          } else {
            break;
          }
          Loop While true

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //let export function use extension of file to determine format
          eFmt = sfUndefined

          //////  switch (sFmt
          //////  case 1 //pc/pcjr
          //////    eFmt = MDIMain.SaveDlg.FilterIndex
          //////
          //////  case 2 //IIgs PCM
          //////    switch (MDIMain.SaveDlg.FilterIndex
          //////    case 1  //.ags
          //////      eFmt = sfAGI
          //////    case 2  //.wav
          //////      eFmt = sfWAV
          //////    case 3  //*.*
          //////      eFmt = sfUndefined
          //////    }
          //////
          //////  case 3 //IIgs MIDI
          //////    switch (MDIMain.SaveDlg.FilterIndex
          //////    case 1  //.ags
          //////      eFmt = sfAGI
          //////    case 2  //.mid
          //////      eFmt = sfMIDI
          //////    case 3  //*.*
          //////      eFmt = sfUndefined
          //////    }
          //////  }

          On Error Resume Next
          //export the resource
          ThisSound.Export strFileName, eFmt, !InGame

          //if error,
          if (Err.Number != 0) {
          ErrMsgBox("An error occurred while exporting this file.", "", "Export File Error"
          }

          //reset mouse pointer
          MDIMain.UseWaitCursor = false;

          ExportSound = true
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }
          public static bool ExportView(AGIView ThisView, bool InGame)

          //exports this view

          string strFileName
          DialogResult rtn

          On Error GoTo ErrHandler

          //set up commondialog
          if (InGame) {
            MDIMain.SaveDlg.DialogTitle = "Export View"
          } else {
            MDIMain.SaveDlg.DialogTitle = "Save View As"
          }
          MDIMain.SaveDlg.Filter = "WinAGI View Files (*.agv)|*.agv|All files (*.*)|*.*"
          MDIMain.SaveDlg.FilterIndex = GameSettings.GetSetting(sVIEWS, sEXPFILTER, 1)
          if (MDIMain.SaveDlg.FilterIndex = 1) {
            MDIMain.SaveDlg.DefaultExt = "agv"
          }

          MDIMain.SaveDlg.Flags = cdlOFNHideReadOnly || cdlOFNPathMustExist || cdlOFNExplorer

          //if view has a filename
          if (LenB(ThisView.ResFile) != 0) {
            //use it
            MDIMain.SaveDlg.FullName = ThisView.ResFile
          } else {
            //use default name
            MDIMain.SaveDlg.FullName = ResDir + ThisView.ID
            //add extension, if necessary
           if (MDIMain.SaveDlg.FilterIndex = 1) {
              MDIMain.SaveDlg.FullName += ".agv"
            }
          }
          MDIMain.SaveDlg.hWndOwner = MDIMain.hWnd

          Do
          On Error Resume Next
          MDIMain.SaveDlg.ShowSaveAs
          //if canceled,
          if (Err.Number = cdlCancel) {
            //exit without doing anything
            Exit Function
          }
          On Error GoTo ErrHandler

          //save default filter index
          WriteSetting GameSettings, sVIEWS, sEXPFILTER, MDIMain.SaveDlg.FilterIndex
          //get filename
          strFileName = MDIMain.SaveDlg.FullName

          //if file exists,
          if (File.Exists(strFileName)) {
            //verify replacement
            rtn = MessageBox.Show(MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?", MessageBoxButtons.YesNoCancel + MessageBoxIcon.Question, "Overwrite file?")

           if (rtn = DialogResult.Yes) {
              break;
            } else if ( rtn = DialogResult.Cancel) {
              Exit Function
            }
          } else {
            break;
          }
          Loop While true

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //export the resource
          ThisView.Export strFileName, !InGame

          //if error,
          if (Err.Number != 0) {
          ErrMsgBox("An error occurred while exporting this file.", "", "Export File Error"
          Exit Function
          }

          //reset mouse pointer
          MDIMain.UseWaitCursor = false;

          ExportView = true
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }
          public static bool ExportWords(AGIWordList ThisWordList, bool InGame)

          string strFileName;
          DialogResult rtn;

          On Error GoTo ErrHandler

          //set up commondialog
          if (InGame) {
            MDIMain.SaveDlg.DialogTitle = "Export Word List"
          } else {
            MDIMain.SaveDlg.DialogTitle = "Save Word List As"
          }
          //if the list has a filename
          if (LenB(ThisWordList.ResFile) != 0) {
            //use it
            MDIMain.SaveDlg.FullName = ThisWordList.ResFile
          } else {
            //use default name
            MDIMain.SaveDlg.FullName = ResDir + "WORDS.TOK"
          }
          MDIMain.SaveDlg.Filter = "WinAGI Word List Files (*.agw)|*.agw|WORDS.TOK file|*.TOK|All files (*.*)|*.*"
          if (LCase$(Right$(.FullName, 4)) = ".agw") {
            MDIMain.SaveDlg.FilterIndex = 1
          } else {
            MDIMain.SaveDlg.FilterIndex = 2
          }
          MDIMain.SaveDlg.DefaultExt = ""
          MDIMain.SaveDlg.Flags = cdlOFNHideReadOnly || cdlOFNPathMustExist || cdlOFNExplorer
          MDIMain.SaveDlg.hWndOwner = MDIMain.hWnd

          Do
          On Error Resume Next
          MDIMain.SaveDlg.ShowSaveAs
          //if canceled,
          if (Err.Number = cdlCancel) {
            //exit without doing anything
            Exit Function
          }
          On Error GoTo ErrHandler

          //get filename
          strFileName = MDIMain.SaveDlg.FullName

          //if file exists,
          if (File.Exists(strFileName)) {
            //verify replacement
            rtn = MessageBox.Show(MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?", MessageBoxButtons.YesNoCancel + MessageBoxIcon.Question, "Overwrite file?")

           if (rtn = DialogResult.Yes) {
              break;
            } else if ( rtn = DialogResult.Cancel) {
              Exit Function
            }
          } else {
            break;
          }
          Loop While true

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          if (LCase$(Right$(MDIMain.SaveDlg.FullName, 4)) = ".agw") {
          //save with new filename
          ThisWordList.Export strFileName, 1, !InGame
          } else {
          ThisWordList.Export strFileName, 0, !InGame
          }

          //if error,
          if (Err.Number != 0) {
          ErrMsgBox("An error occurred while exporting this file:", "", "Export File Error"
          Exit Function
          }

          //reset mouse pointer
          MDIMain.UseWaitCursor = false;

          ExportWords = true
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }
          public static bool NewObjects()

          //creates a new object file

          frmObjectEdit frmNew;

          On Error GoTo ErrHandler

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //create a new file
          frmNew = New frmObjectEdit
          Load frmNew

          frmNew.NewObjects
          frmNew.Show

          //restore cursor while getting resnum
          MDIMain.UseWaitCursor = false;

          Exit Function

          ErrHandler:
          Resume Next
          }

          public static void OpenGlobals(bool ForceLoad = false)

          string strFileName;

          frmGlobals frmNew;
           Form tmpForm;

          On Error GoTo ErrHandler

          //if a game is loaded and NOT forcing...
          //   open editor if not yet in use
          //   or switch to it if it's already open
          if (GameLoaded && !ForceLoad) {
          if (GEInUse) {
            GlobalsEditor.Focus()
            //if minimized,
           if (GlobalsEditor.WindowState = vbMinimized) {
              //restore it
              GlobalsEditor.WindowState = vbNormal
            }

          } else {
            //load it

            MDIMain.UseWaitCursor = true;

            //use the game//s default globals file
            strFileName = GameDir + "globals.txt"
            //look for global file
           if (!File.Exists(strFileName)) {
              //look for a defines.txt file in the resource directory
             if (File.Exists(ResDir + "defines.txt")) {
                //copy it to globals.txt
                On Error Resume Next
                FileCopy ResDir + "defines.txt", strFileName
                On Error GoTo ErrHandler
              }
            }

            //now check again for globals file
           if (!File.Exists(strFileName)) {
              //create blank file
              intFile = FreeFile()
              Open strFileName
              Close intFile
            }

            //load it
            GlobalsEditor = New frmGlobals
            Load GlobalsEditor

            //set ingame status first, so caption will indicate correctly
            GlobalsEditor.InGame = true
            //loading function will handle any errors
            GlobalsEditor.LoadGlobalDefines strFileName //, false
            GlobalsEditor.Show
            //mark editor as in use
            GEInUse = true

            //reset cursor
            MDIMain.UseWaitCursor = false;
          }
          } else {
          //either a game is NOT loaded, OR we are forcing a load from file
          //get a globals file
            OpenDlg.Flags = cdlOFNHideReadOnly
            OpenDlg.DialogTitle = "Open Global Defines File"
            OpenDlg.DefaultExt = "txt"
            OpenDlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            OpenDlg.FilterIndex = GameSettings.GetSetting("Globals", sOPENFILTER, 1)
            OpenDlg.FileName = ""
            OpenDlg.InitDir = DefaultResDir

            OpenDlg.ShowOpen

            strFileName = OpenDlg.FileName

            //save filter
            WriteSetting GameSettings, "Globals", sOPENFILTER, .FilterIndex

            DefaultResDir = JustPath(OpenDlg.FileName)

          //check if already open
          foreach (tmpForm In Forms
           if (tmpForm.Name = "frmGlobals") {
             if (tmpForm.FileName = strFileName && !tmpForm.InGame) {
                //just shift focus
                tmpForm.Focus()
                return;
              }
            }
          Next

          //not open yet; create new form
          //and open this file into it
          MDIMain.UseWaitCursor = true;

          frmNew = New frmGlobals

          //open this file
          Load frmNew
          //loading function will handle any errors
          frmNew.LoadGlobalDefines strFileName
          frmNew.Show

          MDIMain.UseWaitCursor = false;
          }
          return;

          ErrHandler:
          //if user canceled the dialogbox,
          if (Err.Number = cdlCancel) {
          return;
          }

          //Debug.Assert false
          Resume Next
          }

          public static bool UpdatePrinterCaps(int Orientation, Optional bool Quiet = false)

          int rtn;
           bool blnError;

          //enable error trapping
          On Error GoTo ErrHandler

          //set printer flag
          NoPrinter = (Printers.Count = 0)
          if (NoPrinter) {
          Exit Function
          }

          //send endoc command to reset printer object and get new handle
          Printer.EndDoc

          //reset orientation
          Printer.Orientation = Orientation

          //reset printer scale mode so print area is measured in pixels
          Printer.ScaleMode = vbPixels

          //get printer offset values (in pixels)
          PMLeft = GetDeviceCaps(Printer.hDC, PHYSICALOFFSETX)
          PMTop = GetDeviceCaps(Printer.hDC, PHYSICALOFFSETY)

          //get physical page dimensions (in pixels)
          PMRight = GetDeviceCaps(Printer.hDC, PHYSICALWIDTH)
          PMBottom = GetDeviceCaps(Printer.hDC, PHYSICALHEIGHT)

          //right margin is page width minus left margin, minus printable width of page
          PMRight = PMRight - PMLeft - Printer.ScaleWidth

          //bottom margin is page height minus top margin minus printable height of page
          PMBottom = PMBottom - PMTop - Printer.ScaleHeight

          //determine if printer supports color
          if (GetDeviceCaps(Printer.hDC, BITSPIXEL) = 1) {
          //this is a black and white printer
          NoColor = true
          } else {
          NoColor = false
          }

          PDPIx = 1440 / Printer.TwipsPerPixelX
          PDPIy = 1440 / Printer.TwipsPerPixelY
          PWidth = Printer.Width / 1440 * PDPIx
          PHeight = Printer.Height / 1440 * PDPIy

          UpdatePrinterCaps = true
          Exit Function

          ErrHandler:
          //only display an error msg once per call to this function
          if (!blnError && !Quiet) {
          //some printers don't allow landscape printing
          if (Err.Number = 380) {
            MessageBox.Show("The selected printer does not support the selected orientation." + Environment.NewLine + "Your output may not be formatted correctly.", MessageBoxIcon.Critical + MessageBoxIcon.Information, "Printer Capability Error"
            Exit Function
          } else {
            ErrMsgBox("An error occurred accessing the printer:", "Print features may not work correctly.", "Printer Error"
          }
          }
          blnError = true
          Resume Next
          }

          public static void ChangeResDescription(AGIResType ResType, byte ResNum, string Description)

          //updates ingame description
          //for resource objects and the resource tree

          On Error GoTo ErrHandler

          switch (ResType
          case rtObjects
          InvObjects.Description = Description
          InvObjects.Save

          case rtWords
          WordList.Description = Description
          WordList.Save

          case rtLogic
          Logics[ResNum].Description = Description
          Logics[ResNum].Save

          case rtPicture
          Pictures[ResNum].Description = Description
          Pictures[ResNum].Save

          case rtSound
          Sounds[ResNum].Description = Description
          Sounds[ResNum].Save

          case rtView
          Views[ResNum].Description = Description
          Views[ResNum].Save
          }

          //update prop window
          UpdateSelection ResType, ResNum, umProperty
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }
          public static bool GetNewResID(AGIResType ResType, int ResNum, ref string ResID, ref string Description, bool InGame, int FirstProp)

          string strOldResFile, strOldDesc;
          string strOldID;
            bool blnReplace; //used when replacing IDs in logics
          int Index;
            Form tmpForm; //frmLogicEdit
          int rtn;
           string strErrMsg;

          //ResID and Description are passed ByRef, because the resource editors
          //need the updated values passed back to them

          On Error GoTo ErrHandler
          //Debug.Assert ResNum <= 256

          //should never get here with other restypes
          switch (ResType
          case rtGame, rtLayout, rtMenu, rtGlobals, rtGame, rtText, rtNone
          //Debug.Assert false
          Exit Function
          }

          //save incoming (old) ID and description
          strOldID = ResID
          strOldDesc = Description

          //if ingame,
          if (InGame) {
          //need to save current ressource filename
          switch (ResType
          case rtLogic
            //save old sourcefile name
            strOldResFile = ResDir + Logics[ResNum].ID + LogicSourceSettings.SourceExt
          case rtPicture
            strOldResFile = ResDir + Pictures[ResNum].ID + ".agp"
          case rtSound
            strOldResFile = ResDir + Sounds[ResNum].ID + ".ags"
          case rtView
            strOldResFile = ResDir + Views[ResNum].ID + ".agv"
          }
          }

          //if restype is word or object
          if (ResType = rtWords || ResType = rtObjects) {
          //force prop to description
          FirstProp = 2
          } else {
          //Debug.Assert ResNum >= 0
          }

          if (frmEditDescription.Tag = "loaded") {
          //Debug.Assert false
          }
          frmEditDescription.Tag = "loaded"
          //set current values
          frmEditDescription.SetMode ResType, ResNum, ResID, Description, InGame, FirstProp

          Do
            On Error GoTo ErrHandler

            //get new values
            frmEditDescription.ShowDialog(MDIMain);

            //if canceled,
           if (frmEditDescription.Canceled) {
              //just exit
              Unload frmEditDescription
              Exit Function
            }

            //validate return results
            switch (ResType
            case rtObjects, rtWords
              //only have description, so no need to validate
              break;

            default:
              //if ID changed (case sensitive check here - case matters for what gets displayed
             if (strOldID != frmEditDescription.txtID.Text) {

                //validate new id
                rtn = ValidateID(frmEditDescription.txtID.Text, strOldID)

                switch (rtn
                case 0 //ok
                case 1 // no ID
                  strErrMsg = "Resource ID cannot be blank."
                case 2 // ID is numeric
                  strErrMsg = "Resource IDs cannot be numeric."
                case 3 // ID is command
                  strErrMsg = ChrW$(39) + frmEditDescription.txtID.Text + "// is an AGI command, and cannot be used as a resource ID."
                case 4 // ID is test command
                  strErrMsg = ChrW$(39) + frmEditDescription.txtID.Text + "// is an AGI test command, and cannot be used as a resource ID."
                case 5 // ID is a compiler keyword
                  strErrMsg = ChrW$(39) + frmEditDescription.txtID.Text + "// is a compiler reserved word, and cannot be used as a resource ID."
                case 6 // ID is an argument marker
                  strErrMsg = "Resource IDs cannot be argument markers"
                case 14 // ID contains improper character
                  strErrMsg = "Invalid character in resource ID: + vbnewline + !" + QUOTECHAR + "&//()*+,-/:;<=>?[\]^`{|}~ and spaces" + Environment.NewLine + "are not allowed."
                case 15 // ID matches existing ResourceID
                  //only enforce if in a game
                 if (InGame) {
                    //check if this is same ID, same case
                    strErrMsg = ChrW$(39) + frmEditDescription.txtID.Text + "// is already in use as a resource ID."
                  } else {
                    //reset to no error
                    rtn = 0
                  }
                }

                //if there is an error
                if (rtn != 0) {
                  //error - show msgbox
                  MsgBoxEx strErrMsg, MessageBoxIcon.Information + MessageBoxButtons.OK + vbMsgBoxHelpButton, "Change Resource ID", WinAGIHelp, "htm\winagi\Managing Resources.htm#resourceids"
                } else {
                  //make the change
                  //update ID for the ingame resource
                  switch (ResType
                  case rtLogic
                    Logics[ResNum].ID = frmEditDescription.txtID.Text
                  case rtPicture
                    Pictures[ResNum].ID = frmEditDescription.txtID.Text
                  case rtSound
                    Sounds[ResNum].ID = frmEditDescription.txtID.Text
                  case rtView
                    Views[ResNum].ID = frmEditDescription.txtID.Text
                  }
                  break;
                }
              } else {
                //if ID was exactly the same, no change needed
                break;
              }
            }
          Loop While true

          On Error GoTo ErrHandler

          //id change is acceptable (or it didn//t change)
          //return new id and description
          if (strOldID != frmEditDescription.txtID.Text) {
            ResID = frmEditDescription.txtID.Text
          }

          //if description changed, update it
          if (strOldDesc != frmEditDescription.txtDescription.Text) {
            Description = frmEditDescription.txtDescription.Text

            //if in a game,
           if (InGame) {
              //update the description
              switch (ResType
              case rtLogic
                Logics[ResNum].Description = Description
              case rtPicture
                Pictures[ResNum].Description = Description
              case rtSound
                Sounds[ResNum].Description = Description
              case rtView
                Views[ResNum].Description = Description
              }
            }
          }

          //indicate success by returning TRUE
          GetNewResID = true

          //save replace flag value
          blnReplace = (frmEditDescription.chkUpdate.Checked)

          //save state of update logic flag
          DefUpdateVal = frmEditDescription.chkUpdate.Value

          //update the logic tooltip lookup table for log/pic/view/snd
          switch (ResType
          case rtLogic
          Index = ResNum
          case rtView
          Index = ResNum + 256
          case rtSound
          Index = ResNum + 512
          case rtPicture
          Index = ResNum + 768
          default:
          Index = -1
          }
          if (Index >= 0) {
          IDefLookup(Index).Name = ResID
          }

          //unload the form
          Unload frmEditDescription

          //for ingame resources, update resource objects, files and the treelist
          if (InGame) {
          switch (ResType
          case rtLogic, rtPicture, rtSound, rtView
           if (strOldID != ResID) {
              //if not just a change in text case
             if (StrComp(strOldID, ResID, StringComparison.OrdinalIgnoreCase) != 0) {
                //update resource file if ID has changed
                //this also updates the treelist, and property window
                UpdateResFile ResType, ResNum, strOldResFile
              } else {
                //just change the filename
                On Error Resume Next
                switch (ResType
                case rtLogic
                  File.Move(strOldResFile, ResDir + ResID + LogicSourceSettings.SourceExt
                case rtPicture
                  File.Move(strOldResFile, ResDir + ResID + ".agp"
                case rtSound
                  File.Move(strOldResFile, ResDir + ResID + ".ags"
                case rtView
                  File.Move(strOldResFile, ResDir + ResID + ".agv"
                }

                On Error GoTo ErrHandler
                //then update property window and resource list
                UpdateSelection ResType, ResNum, umProperty || umResList
              }

              //if OK to update in all logics, do so
             if (blnReplace) {
                //reset search flags
                FindingForm.ResetSearch

                //now replace the ID
                ReplaceAll strOldID, ResID, fdAll, true, true, flAll, ResType
              }

            } else if ( strOldDesc != Description) {
              //update the property window, since description changed
              //update property window
              UpdateSelection ResType, ResNum, umProperty
            }

          case rtWords
            WordList.Description = Description
            //update property window and tree
            UpdateSelection ResType, ResNum, umProperty

          case rtObjects
            InvObjects.Description = Description
            //update property window and tree
            UpdateSelection ResType, ResNum, umProperty

          }

          //set any open logics deflist flag to force a rebuild
          if (LogicEditors.Count > 0) {
            foreach (tmpForm In LogicEditors
             if (tmpForm.Name = "frmLogicEdit") {
               if (tmpForm.InGame) {
                  tmpForm.ListDirty = true
                }
              }
            Next
          }
          }
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static string NextMsg(string strText, ref int lngLoc, TDefine[] LDefList, string[] StrDefs)

          //starting at position lngPos, step through cmds until a match is found for
          //a cmd that has a msg argument:
          //  log(m#)
          //  print(m#)
          //  display(#,#,m#)
          //  get.num(m#, v#)
          //  print.at(m#, #, #, #)
          //  set.menu(m#)
          //  set.string(s#, m#)
          //  get.string(s#, m#, #, #, #)
          //  set.game.id(m#)
          //  set.menu.item(m#, c#)
          //  set.cursor.char(m#)
          //  also need to check for s#=m#; do this by building custom array
          //  of matching elements; all s## tokens, plus any defines that
          //  are stringtype

          // return values
          // - if OK, lngLoc is location; and strMsg is the string
          // - if error lngLoc is negative, still marking location
          //      and strMsg is an error code:
          //   0|##=message marker; msg is //m##//
          //   1=missing //(// after a command
          //   2=missing end quote
          //   3=no quotes; can//t find matching define (or bad define); not a marker (m##)

          string strCmd;
           int lngArg;
          string strNext;
           int lngNext;
          int lngQuotesOK, lngSlashCount;
          int i;
            bool blnDefFound;
          float sngVal;

          On Error GoTo ErrHandler

          //get next cmd
          FindNextToken strText, lngLoc, strCmd, true, true
          Do Until lngLoc = 0
          //assume not found
          lngArg = -1

          //check against list of cmds with msg arguments:
          switch (Len(strCmd)
          case 3  //check for log
           if (StrComp(strCmd, "log", StringComparison.OrdinalIgnoreCase) = 0) {
              //get arg 0
              lngArg = 0
            }

          case 5 //check for print
           if (StrComp(strCmd, "print", StringComparison.OrdinalIgnoreCase) = 0) {
              //get arg 0
              lngArg = 0
            }

          case 7  //display or get.num
           if (StrComp(strCmd, "display", StringComparison.OrdinalIgnoreCase) = 0) {
              lngArg = 2
            } else if ( StrComp(strCmd, "get.num", StringComparison.OrdinalIgnoreCase) = 0) {
              lngArg = 0
            }

          case 8  //print.at or set.menu
           if (StrComp(strCmd, "print.at", StringComparison.OrdinalIgnoreCase) = 0) {
              lngArg = 0
            } else if ( StrComp(strCmd, "set.menu", StringComparison.OrdinalIgnoreCase) = 0) {
              lngArg = 0
            }

          case 10 //set.string, get.string
           if (StrComp(strCmd, "set.string", StringComparison.OrdinalIgnoreCase) = 0) {
              lngArg = 1
            } else if ( StrComp(strCmd, "get.string", StringComparison.OrdinalIgnoreCase) = 0) {
              lngArg = 1
            }

          case 11 //set.game.id
           if (StrComp(strCmd, "set.game.id", StringComparison.OrdinalIgnoreCase) = 0) {
              lngArg = 0
            }

          case 13 //set.menu.item
           if (StrComp(strCmd, "set.menu.item", StringComparison.OrdinalIgnoreCase) = 0) {
              lngArg = 0
            }

          case 15 //set.cursor.char
           if (StrComp(strCmd, "set.cursor.char", StringComparison.OrdinalIgnoreCase) = 0) {
              lngArg = 0
            }
          }

          //if a cmd was found (lngArg<>-1)
          if (lngArg != -1) {
            //syntax expected will be
            // (arg0, arg1, ...
            strCmd = ""
            FindNextToken strText, lngLoc, strCmd, true, true
           if (lngLoc = 0) {
              //end of logic
              Exit Function
            }

           if (strCmd = "(") {
              //next one is arg0
              strCmd = ""
              FindNextToken strText, lngLoc, strCmd, true, true
             if (lngLoc = 0) {
                //end of logic
                Exit Function
              }
            } else {
              //not a valid cmd; return error
              lngLoc = -lngLoc
              NextMsg = "1"
              Exit Function
            }
          }

          //if not getting arg0
          if (lngArg > 0) {
            Do
              //next cmd is a comment
              strCmd = ""
              FindNextToken strText, lngLoc, strCmd, true, true
             if (lngLoc = 0) {
                //end of logic
                Exit Function
              }

             if (strCmd != ",") {
                //something not right; just ignore this command
                lngArg = -1
                break;
              }
              //now get next arg
              strCmd = ""
              FindNextToken strText, lngLoc, strCmd, true, true
             if (lngLoc = 0) {
                //end of logic
                Exit Function
              }
              //decrement Count
              lngArg = lngArg - 1
            Loop Until lngArg <= 0
          }

          //if not found, maybe it's a string assignment (s##="text"; or strdefine="text";)
          if (lngArg = -1) {
            //is it a defined string?
            For i = 0 To UBound(StrDefs())
             if (LCase(strCmd) = LCase(StrDefs(i))) {
                //good to go - we don't care what s##
                //just that it's a valid define value
                lngArg = 0
                Exit For
              }
            Next i

            //if not found as a define, maybe it's a string marker (s##)
           if (lngArg = -1 && Asc(LCase(strCmd)) = 115) {
              //strip off the //s//; if rest of cmd is a valid string number
              //then we found one!
              strCmd = Right(strCmd, Len(strCmd) - 1)
             if (IsNumeric(strCmd)) {
                //do we care what number? yes- must be 0-23
                //in the off chance the user is working with
                //a version that has a limit of 12 strings
                //we will let the compiler worry about it
               if (Val(strCmd) >= 0 && Val(strCmd) <= 23) {
                  //must be integer
                 if (Val(strCmd) = Int(Val(strCmd))) {
                    //we found one
                    lngArg = 0
                  }
                }
              }
            }

            //if we found a string assignment, now we need to get the msg value
           if (lngArg = 0) {
              //next cmd should be an equal sign
              strCmd = ""
              FindNextToken strText, lngLoc, strCmd, true, true
             if (lngLoc = 0) {
                //end of logic
                Exit Function
              }
             if (strCmd = "=") {
                //ok, now we know the very next thing is the assigned string!
                strCmd = ""
                FindNextToken strText, lngLoc, strCmd, true, true
               if (lngLoc = 0) {
                  //end of logic
                  Exit Function
                }
              } else {
                //false alarm! it's probably a string arg used in another
                //command
                lngArg = -1
              }
            }
          }

          //if we have a valid command and found it's arg value, continue
          if (lngArg != -1) {
            //strcmd is now the msg argument we are looking for;
            //it might be a message marker (//m##//) or it might be
            //a string; or it might be a local, global or reserved
            //define; need to validate it

            //first, is it a valid message string? if not,
           if (!IsValidMsg(strCmd)) {
              //does it start with a dbl quote? if so, it's a malformed string
             if (Asc(strCmd) = 34) {
                lngLoc = -lngLoc
                NextMsg = "2"
                Exit Function
              }

              //it's not a string (good or bad);
              //might be a message marker or a define value
              //check for message marker first
             if (LCase(Left(strCmd, 1)) = "m") {
                //if it's a non-zero number, less than 256, it's good
               if (IsNumeric(Right(strCmd, Len(strCmd) - 1))) {
                  sngVal = Val(Right(strCmd, Len(strCmd) - 1))
                 if ((Int(sngVal) = sngVal) && sngVal > 0 && sngVal < 256) {
                    //return the msg number
                    lngLoc = -lngLoc
                    NextMsg = "0|" + CStr(sngVal)
                    Exit Function
                  }
                }
              }

              //not a msg marker; try replacing with define
              blnDefFound = false
              Do
                //locals first
                For i = 0 To UBound(LDefList())
                  switch (LDefList(i).Type
                  case atMsg
                    //a msg define
                   if (StrComp(LDefList(i).Name, strCmd, StringComparison.OrdinalIgnoreCase) = 0) {
                      //we can//t assume loacal defines are valid
                      sngVal = Val(Right(LDefList(i).Value, Len(LDefList(i).Value) - 1))
                     if ((Int(sngVal) = sngVal) && sngVal > 0 && sngVal < 256) {
                        //return the msg number
                        lngLoc = -lngLoc
                        NextMsg = "0|" + CStr(sngVal)
                        Exit Function
                      }
                    }
                  case atDefStr
                    //a string
                   if (LDefList(i).Type = atDefStr) {
                      //does it match?
                     if (StrComp(LDefList(i).Name, strCmd, StringComparison.OrdinalIgnoreCase) = 0) {
                        //but we can//t assume it's a valid string
                       if (IsValidMsg(LDefList(i).Value)) {
                          //we don't replace the define value
                          //but just mark it as OK
                          blnDefFound = true
                          break;
                        }
                      }
                    }
                  }
                Next i

                //try globals next
                For i = 0 To UBound(GDefLookup())
                  switch (GDefLookup(i).Type
                  case atMsg
                    //a msg define
                   if (StrComp(GDefLookup(i).Name, strCmd, StringComparison.OrdinalIgnoreCase) = 0) {
                      //global defines are already validated
                      sngVal = Val(Right(strCmd, Len(strCmd) - 1))
                      //return the msg number
                      lngLoc = -lngLoc
                      NextMsg = "0|" + CStr(sngVal)
                      Exit Function
                    }
                  case atDefStr
                    //a string
                   if (GDefLookup(i).Type = atDefStr) {
                     if (StrComp(GDefLookup(i).Name, strCmd, StringComparison.OrdinalIgnoreCase) = 0) {
                        //global defines are already validated
                        //we don't replace the define name
                        //but just mark it as OK
                        blnDefFound = true
                        break;
                      }
                    }
                  }
                Next i

                //next check reserved defines
                //(we only check the three string values
                // as they are the only resefs that could
                // apply)
                For i = 90 To 92
                 if (StrComp(RDefLookup(i).Name, strCmd, StringComparison.OrdinalIgnoreCase) = 0) {
                    //reserved defines are already validated
                    //we don't replace the define value
                    //but just mark it as OK
                    blnDefFound = true
                    break;
                  }
                Next i
              Loop Until true

              //if a valid define was found, we keep the
              //define name as the message value
              //but if not found, just treat it as an error
             if (!blnDefFound) {
                //not a string, and not a msg marker - IDK what it is!
                lngLoc = -lngLoc
                NextMsg = "3"
                Exit Function
              }
            }

            //strCmd is validated to be a good string;
            //now check for concatenation (unless it's a define)
           if (blnDefFound) {
              lngNext = 0
            } else {
              lngNext = lngLoc
            }

            Do Until lngNext = 0
              strNext = ""
              FindNextToken strText, lngNext, strNext, true, true
             if (lngLoc = 0) {
                //end of logic; return what we found
                NextMsg = strCmd
                Exit Function
              }

              //next cmd may be another string
             if (IsValidMsg(strNext)) {
                //concatenat it
                strCmd = Left$(strCmd, Len(strCmd) - 1) + Right$(strNext, Len(strNext) - 1)
              } else {
                //no concatenation;
                //just exit with existing string
                lngNext = 0
              }
            Loop

            //return this string
            NextMsg = strCmd
            Exit Function
          }

          //get next cmd
          strCmd = ""
          FindNextToken strText, lngLoc, strCmd, true, true
          Loop
          Exit Function

          ErrHandler:
          Resume Next
          }
          public static bool ReadMsgs(string strText, string[] Messages, int[] MsgUsed(), TDefine[] LDefList)

          //all valid message declarations in strText are
          //put into the Messages() array; the MsgUsed array
          //is used to mark each declared message added

          //if an error in the logic is detected that would
          //make it impossible to accurately update the message
          //section, the function returns false, and Messages(0)
          //is populated with the error code, Messages(1)
          //is populated with the line where the error was found,
          //and subsequent elements are populated with any
          //additional information regarding the error
          //error codes:
          //  1 = invalid msg number
          //  2 = duplicate msg number
          //  3 = not a string
          //  4 = stuff not allowed after msg declaration

          short intMsgNum;
           int lngPos;
          string strMsgMarker;
           int lngMsgStart;
          int lngConcat;
           string strCmd;
          bool blnConcat, blnDefFound;
          int i;
          On Error GoTo ErrHandler

          strMsgMarker = "#message"

          //get first message marker position
          FindNextToken strText, lngPos, strMsgMarker

          Do Until lngPos = 0
          //next cmd should be msg number
          strCmd = ""
          FindNextToken strText, lngPos, strCmd

          //validate msg number
          if (Val(strCmd) < 1 || Val(strCmd) > 255) {
            //invalid msg number
            Messages(0) = "1"
            Messages(1) = CStr(lngPos)
            Exit Function
          }

          intMsgNum = CInt(Val(strCmd))
          if (Val(strCmd) != intMsgNum) {
            //invalid msg number
            Messages(0) = "1"
            Messages(1) = CStr(lngPos)
            Exit Function
          }

          //if msg is already assigned
          if (MsgUsed(intMsgNum) != 0) {
            //user needs to fix message section first;
            //return false, and use the Message structure to indicate
            //what the problem is, and on which line it occurred
            Messages(0) = "2" //"duplicate msg number"
            Messages(1) = CStr(lngPos)
            Messages(2) = CStr(intMsgNum)
            Exit Function
          }

          //next cmd should be a string
          strCmd = ""
          FindNextToken strText, lngPos, strCmd

          //msgval should be string
          if (!IsValidMsg(strCmd)) {
            //does it start with a dbl quote? if so, it's a malformed string
           if (Asc(strCmd) = 34) {
              Messages(0) = "5"
              Messages(1) = CStr(lngPos)
              Exit Function
            }

            //it's not a good (or bad) string;
            //try replacing with define (locals, then globals, then reserved
            blnDefFound = false
            Do
              For i = 0 To UBound(LDefList())
               if (LDefList(i).Type = atDefStr) {
                 if (LDefList(i).Name = strCmd) {
                    blnDefFound = true
                    break;
                  }
                }
              Next i
              //try globals next
              For i = 0 To UBound(GDefLookup())
               if (GDefLookup(i).Type = atDefStr) {
                 if (GDefLookup(i).Name = strCmd) {
                    blnDefFound = true
                    break;
                  }
                }
              Next i
              //then try reserved
              For i = 0 To UBound(LogicSourceSettings.ReservedDefines(atDefStr))
               if (LogicSourceSettings.ReservedDefines(atDefStr)(i).Type = atDefStr) {
                 if (LogicSourceSettings.ReservedDefines(atDefStr)(i).Name = strCmd) {
                    blnDefFound = true
                    break;
                  }
                }
              Next i
            Loop Until true

            //if it was replaced, we accept whatever was used as
            //the define name; if not replaced, it's error
           if (!blnDefFound) {
              //not a string, and not a msg marker - IDK what it is!
              Messages(0) = "3"
              Messages(1) = CStr(lngPos)
              Exit Function
            }
          }

          //copy string cmd
          Messages(intMsgNum) = strCmd

          //check for end of line/concatenation
          blnConcat = false
          lngConcat = lngPos
          Do
            strCmd = ""
            FindNextToken strText, lngConcat, strCmd
            //if end of input
           if (lngConcat = 0) {
              //exit the loop since no more text left to process
              break;
            }

           if (blnConcat) {
              //concatenation is optional; if string is found,
              //then concatenate it; if not, we//re done
             if (IsValidMsg(strCmd)) {
                //add the concat string
                Messages(intMsgNum) = Left$(Messages(intMsgNum), Len(Messages(intMsgNum)) - 1) + Right$(strCmd, Len(strCmd) - 1)

                //toggle concat flag
                blnConcat = false
              } else {
                //it's not a valid string, but we
                //do need to see if its an INVALID string
               if (Asc(strCmd) = 34) {
                  //it's bad!
                  Messages(0) = "3"
                  Messages(1) = CStr(lngPos)
                  Exit Function
                } else {
                  //any other command means no concatenation needed
                  break;
                }
              }
            } else {
              //only carriage return, or comments allowed after a string
             if (strCmd = Keys.Enter || Left$(strCmd, 2) = "//" || Asc(strCmd) = 91) {
                blnConcat = true
              } else {
                //stuff not allowed on line after msg declaration
                Messages(0) = "4"
                Messages(1) = CStr(lngConcat)
                Exit Function
              }
            }
          Loop While true

          //set flag to show message is declared
          MsgUsed(intMsgNum) = 1

          //check for end of input
          if (lngPos = 0) {
            break;
          }

          //get next msg marker
          FindNextToken strText, lngPos, strMsgMarker
          Loop

          //success
          ReadMsgs = true
          Exit Function

          ErrHandler:
          Resume Next
          }



          public static DialogResult MsgBoxEx(string Prompt, MessageBoxButtons Buttons = MessageBoxButtonsOKOnly, Optional Title, string HelpFile = "", string HelpTopic = "", string CheckString = "", ref bool Checked = false)
          //Title has to be a non-declared type for the IsMissing function to work

          int lngButtonCount;
          int lngBW, lngWidth1, lngWidth2;

          On Error GoTo ErrHandler

          //show custom msgbox
          //vbOKOnly =              0 1
          //vbOKCancel =            1 1-2
          //vbAbortRetryIgnore =    2 1-2-3
          //vbYesNoCancel =         3 1-2-3
          //MessageBoxButtons.YesNo =               4 1-2
          //vbRetryCancel =         5 1-2

          //vbApplicationModal =     0
          //vbSystemModal =     0x1000

          //vbDefaultButton1 =       0
          //vbDefaultButton2 =   0x100
          //vbDefaultButton3 =   0x200
          //vbDefaultButton4 =   0x300

          //MessageBoxIcon.Critical =          0x10
          //MessageBoxIcon.Question =          0x20
          //vbExclamation =       0x30
          //MessageBoxIcon.Information =       0x40

          //vbMsgBoxHelpButton =     0x4000
          //vbMsgBoxSetForeground = 0x10000  //?
          //vbMsgBoxRight =         0x80000  //right align the msg text
          //vbMsgBoxRtlReading =   0x100000  //?


          Load frmDialog
          //pass along help info
          frmDialog.HelpFile = HelpFile
          frmDialog.HelpTopic = HelpTopic

          //need to account for border
          lngBW = frmDialog.Width - frmDialog.ScaleWidth

          //if showing help button...
          frmDialog.cmdHelp.Visible = (Buttons && vbMsgBoxHelpButton)

          if ((Buttons && vbSystemModal)) {
          //      frmDialog.StartUpPosition = vbStartUpScreen
          } else {
          //      frmDialog.StartUpPosition = vbStartUpOwner
          }

          //set title
          if (IsMissing(Title)) {
            frmDialog.Text = App.Title
          } else {
            frmDialog.Text = CStr(Title)
          }

          //the checkstring (if visible) fit
          if (frmDialog.TextWidth(Prompt) > .TextWidth(CheckString) + 360) {
            .message.Width = .TextWidth(Prompt)
          } else {
            .message.Width = .TextWidth(CheckString) + 360
          }

          //set height of msg prompt
          .message.Height = .TextHeight(Prompt)

          //add icons (if requested)
          if (((Buttons / 0x10) && 0x7) > 0 && ((Buttons / 0x10) && 0x7) <= 4) {
            //load associated Image
            .Image1.Picture = .ImageList1.ListImages((Buttons / 0x10) && 0x7).Picture
            .Image1.Visible = true

            //adjust label position to account for icon
            .message.Left = .message.Left + 705

            //if text height of msg is <height of Image,
           if (.message.Height < .Image1.Height) {
              //center it
              .message.Top = .message.Top + ((.Image1.Height - .message.Height) / 2 / ScreenTWIPSX) * ScreenTWIPSX

              //buttons are below icon
              .Button1.Top = .Image1.Top + .Image1.Height + 255
            } else {
              //buttons are below msg
              .Button1.Top = .message.Top + .message.Height + 255
            }

          } else {
            //no icon; buttons are below msg
            .Button1.Top = .message.Top + .message.Height + 255
          }

          //now set height
          .Height = (.Height - .ScaleHeight) + .Button1.Top + .Button1.Height + 165

          //save width based on msg size
          //so it can be compared to button size
          lngWidth1 = lngBW + .message.Left + .message.Width + 105

          //if checkmark is needed
          if (LenB(CheckString) != 0) {
            //position checkbox under msg
            .Check1.Left = .message.Left
            .Check1.Top = .Button1.Top + 180

            //move buttons down to account for checkbox
            .Button1.Top = .Button1.Top + 600
            //adjust dialog height to account for checkbox
            .Height = .Height + 600

            //set check properties based on passed parameters
            .Check1.Width = .TextWidth(CheckString) + 600
            .Check1.Text = CheckString
            .Check1.Value = (vbChecked && Checked)

            //finally, show the checkbox
            .Check1.Visible = true
          }

          //move other buttons to correct height
          .Button2.Top = .Button1.Top
          .Button3.Top = .Button1.Top
          .cmdHelp.Top = .Button1.Top

          //set message text
          .message.Text = Prompt

          //set button captions
          switch (Buttons && 0x7
          case MessageBoxButtons.OK
            .Button1.Text = "OK"
            .Button1.Default = true

            lngButtonCount = 1

          case vbOKCancel
            .Button1.Text = "OK"
            .Button2.Text = "Cancel"
            .Button2.Visible = true
            .Button2.Cancel = true

            .Button2.Default = (Buttons && vbDefaultButton2)

            lngButtonCount = 2

          case vbAbortRetryIgnore
            .Button1.Text = "&Abort"
            .Button2.Text = "&Retry"
            .Button2.Visible = true
            .Button3.Text = "&Ignore"
            .Button3.Visible = true

            .Button3.Default = (Buttons && vbDefaultButton3)
            .Button2.Default = (Buttons && vbDefaultButton2)

            lngButtonCount = 3

          case MessageBoxButtons.YesNoCancel
            .Button1.Text = "&Yes"
            .Button2.Text = "&No"
            .Button2.Visible = true
            .Button3.Text = "Cancel"
            .Button3.Cancel = true
            .Button3.Visible = true

            .Button3.Default = (Buttons && vbDefaultButton3)
            .Button2.Default = (Buttons && vbDefaultButton2)

            lngButtonCount = 3

          case MessageBoxButtons.YesNo
            .Button1.Text = "&Yes"
            .Button2.Text = "&No"
            .Button2.Visible = true

            .Button2.Default = (Buttons && vbDefaultButton2)

            lngButtonCount = 2

          case vbRetryCancel
            .Button1.Text = "&Retry"
            .Button2.Text = "Cancel"
            .Button2.Cancel = true
            .Button2.Visible = true

            .Button2.Default = (Buttons && vbDefaultButton2)

            lngButtonCount = 2
          }

          //if help button is visible,
          if ((Buttons && vbMsgBoxHelpButton)) {
            lngButtonCount = lngButtonCount + 1
          }

          //width needs to be wide enough for all buttons
          lngWidth2 = lngButtonCount * 1215 + 345

          //if width needed for buttons is wider than
          //width needed for text
          if (lngWidth2 > lngWidth1) {
            .Width = lngWidth2
            //button1 is right where it needs to be
          } else {
            .Width = lngWidth1
            //move button1 to correct position based on button Count
            .Button1.Left = ((.Width - lngBW - (1125 + (lngButtonCount - 1) * 1215)) / 2 / ScreenTWIPSX) * ScreenTWIPSX
          }

          //move other buttons based on button1 pos
          .Button2.Left = .Button1.Left + .Button1.Width + 90
          .Button3.Left = .Button2.Left + .Button2.Width + 90

          //if showing help button
          if ((Buttons && vbMsgBoxHelpButton)) {
            //position it relative to button1, using button Count
            //to determine offset
            .cmdHelp.Left = .Button1.Left + (lngButtonCount - 1) * (.Button1.Width + 90)
          }

          //show it
          frmDialog.ShowDialog(MDIMain);

          //get result
          MsgBoxEx = frmDialog.Result
          //get check box status
          Checked = (frmDialog.Check1.Checked)

          //unload the form
          Unload frmDialog
          End With
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static void UpdateLayoutFile(EUReason Reason, int LogicNumber, AGIExits NewExits, int NewNum = 0)

          //updates the layout file to accurately show/hide
          //this room; the file entry uses the update code (U)
          //instead of the room code (R) so the layout editor
          //knows that the room needs to have its exits
          //refreshed based on the update
          //when hiding a room, must make sure it is visible
          //before adding update line; if room is not visible
          //adding an update line will cause an error when
          //the file is opened.

          //renumbering is a bit easier; we just add the renumber
          //entry, without doing anything else; while it's possible
          //that extra, unnecessary renumbering actions can happen
          //with this approach, it's worth it; it's a lot harder
          //to examine the file, and try to figure out chains of
          //renumbering that might be simplified


           List<string> stlLayout;
          string strLine, strTempFile;
          int i;
           bool blnRoomVis;
          int lngRoomLine, lngUpdateLine;
          int lngCode, lngNum;

          On Error GoTo ErrHandler

          //it is possible that file might not exist; if a layout was extracted without
          //being saved, then an update to the layout followed by a call to view
          //a logic would get us here...
          if (!File.Exists(GameDir + GameID + ".wal")) {
          return;
          }

          //open layout file for input
          intFile = FreeFile()
          Open GameDir + GameID + ".wal"
          //get all the text
          strLine = String$(LOF(intFile), 0)
          Get intFile, 1, strLine
          //done with the file
          Close intFile

          //assign to stringlist
          stlLayout = New StringList
          stlLayout.Assign strLine
          if (stlLayout(stlLayout.Count - 1) = "") {
          stlLayout.Delete stlLayout.Count - 1
          }
          strLine = ""

          lngRoomLine = -1
          lngUpdateLine = -1

          //if not renumbering, we need to find any current
          //room or update lines
          if (Reason != euRenumberRoom) {
          //locate any existing room or update lines for this room
          For i = 0 To stlLayout.Count - 1
           if (LenB(stlLayout(i)) != 0) {
             if (Asc(stlLayout(i)) = 85 && Val(Mid$(stlLayout(i), 3)) = LogicNumber) { //asc("U") = 85
                //this is the update line
                lngUpdateLine = i
              }
             if (Asc(stlLayout(i)) = 82 && Val(Mid$(stlLayout(i), 3)) = LogicNumber) { //asc("R") = 82
                //this is the room line
                lngRoomLine = i
                //determine if room is visible
                strLine = stlLayout(i)
                strLine = Right$(strLine, Len(strLine) - InStr(3, strLine, "|")) //strip off number
                //test remainder of line for //T// or //F//
                blnRoomVis = (Asc(strLine) = 84)
              }
            }
          Next i

          //should NEVER have room come before update
          if (lngRoomLine != -1 && lngUpdateLine != -1) {
            //Debug.Assert lngUpdateLine > lngRoomLine
          }

          //ALWAYS delete update line, if there is one
          if (lngUpdateLine != -1) {
            //remove the line
            stlLayout.Delete lngUpdateLine
          }
          }

          switch (Reason
          case euAddRoom, euShowRoom, euUpdateRoom
          //create the new update line
          strLine = "U|" + CStr(LogicNumber) + "|true"

          //if new exits were passed,
          if (!NewExits = null) {
            //add exits
            For i = 0 To NewExits.Count - 1
              With NewExits(i)
                strLine = strLine + "|" + Right$(.ID, 3) + ":" + .Room + ":" + .Reason + ":" + .Style
              End With
            Next i
          }

          case euRemoveRoom
          //if there is NOT a visible room
          if (!blnRoomVis) {
            //dont need to update, cuz room is already hidden
          } else {
            //create the new update line
            strLine = "U|" + CStr(LogicNumber) + "|false"
          }

          case euRenumberRoom
          //updates the layout file to indicate a room has changed its number
          //a file entry using the update code (N)
          //instead of the room code (R) indicates to the layout editor
          //that the room has changed

          //add the update line
          strLine = "N|" + CStr(LogicNumber) + "|" + CStr(NewNum)

          }

          if (Len(strLine) > 0) {
          //add the new update line
          stlLayout.Add strLine
          }

          //create temp file
          strTempFile = Path.GetTempFileName()

          //open temp file for output
          intFile = FreeFile()
          Open strTempFile

          //write data to the file
          Put intFile, 1, Join(stlLayout.AllLines, Keys.Enter)
          //done with file
          Close intFile

          //kill old file, copy new
          On Error Resume Next
          //Debug.Assert GameID != ""
          Kill GameDir + GameID + ".wal"
          FileCopy strTempFile, GameDir + GameID + ".wal"
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static AGIExit AnalyzeExit(string strSource, ref int lngLoc, int StartPos = 0)

          //analyzes the exit info associated with the //new.room// command
          //located at lngLoc in strSource
          //StartPos is a verified valid starting point to begin search for
          ////if// commands; usually set to end of the most recent //new.room//
          //command; this adds efficiency to the search by not requiring
          //the //if// search to have to start at beginning for each exit being
          //analyzed
          //
          //returns an agiexit object with exit info
          //
          //lngLoc is also changed to point to the end of the //new.room// command
          //to allow insertion of layouteditor marker

          int pos1, pos2, pos3;
          int i, j, lngEnd;
            bool blnGood;
          short intRoom;
           EUReason eReason;
          short intStyle;
           int lngLen;
          string strLine;

          //lngLoc is location of //new.room//
          //pos1 is location of //if//
          //pos2 is location of //then// or //)//
          //pos3 is location of //{//

          On Error GoTo ErrHandler

          //local copy of length (slight improvment in speed)
          lngLen = Len(strSource)

          //get room Value first

          //get next cmd pos
          i = lngLoc + 7
          FindNextToken strSource, i, strLine

          //if a parenthesis is found
          if (strLine = "(") {
          //ok; next cmd should be the Value we are looking for
          strLine = ""
          FindNextToken strSource, i, strLine
          } else {
          //if no parenthesis, set room arg to zero
          strLine = "0"
          }

          intRoom = CInt(Val(Logics.ConvertArg(Trim$(strLine), atNum)))

          //validate room
          if (intRoom < 0 || intRoom > 255) {
          intRoom = 0
          }

          Do
          //next token should be //)//
          strLine = ""
          FindNextToken strSource, i, strLine
          if (strLine != ")") {
            //syntax not good
            break;
          }

          //and then next token should be //;//
          strLine = ""
          lngEnd = FindNextToken(strSource, i, strLine)
          if (strLine = ";") {
            //move ahead one char
            lngEnd = lngEnd + 1
            blnGood = true
          } else {
            //syntax not good
            break;
          }

          Loop Until true

          //if syntax is bad, use end of line, or comment start
          if (!blnGood) {
          //if the next token was a comment, we need to back up so we can find it
          if (Left$(strLine, 1) = "[" || Left$(strLine, 2) = "//") {
            lngEnd = lngEnd - 1
          //if the next token is end line,
          } else if ( strLine = Keys.Enter) {

          } else {
            //find the end, or a comment
            Do
              strLine = ""
              lngEnd = FindNextToken(strSource, i, strLine)
             if (strLine = Keys.Enter) {
                //end of line; exit
                break;
              } else if ( Left$(strLine, 1) = "[" || Left$(strLine, 2) = "//") {
                //comment found; exit
                break;
              }
            Loop While lngEnd < lngLen && lngEnd != 0
          }

          }

          //locate the //then// that precedes this "new.room(" cmd
          //and determine style of exit (complex or simple)
          pos2 = StartPos
          Do
          //beginning at pos2; step through //if// cmds until
          //the one that precedes lngLoc is found
          //
          //use pos2 as cmdpointer, pos3 as next //if// loc
          //pos1 as final //if// loc
          Do
            pos3 = FindNextToken(strSource, pos2, "if")
            //if this pos is not past new.room cmd
           if (pos3 < lngLoc && pos3 != 0) {
              //update final //if// loc
              pos1 = pos3 + 1
            } else {
              //done search
              break;
            }
          //if no //if// found, exit the loop
          Loop Until pos3 = 0

          //if no //if// found,
          if (pos1 = 0) {
            //this is an //other// exit
            eReason = erOther
            break;
          }

          //now examine the //if// statement to determine exit Type
          //expected syntax:
          // if ({test}) {new.room(##)
          //
          //(agi syntax allows cr//s inbetween any of the elements)
          //comments could also exist between any elements either as
          //line comments in conjunction with a cr, or as a block comment
          //other commands could exist between //{// and //new.room//

          //expecting //(//
          strLine = ""
          FindNextToken strSource, pos1, strLine, true, true
          if (strLine != "(") {
            //unknown
            eReason = erOther
            break;
          }

          //expecting //v2//
          strLine = ""
          FindNextToken strSource, pos1, strLine, true, true
          if (Logics.ConvertArg(Trim$(strLine), atVar) != "v2") {
            //other
            eReason = erOther
            break;
          }

          //expecting //==//
          strLine = ""
          FindNextToken strSource, pos1, strLine, true, true
          if (strLine != "==") {
            //other
            eReason = erOther
            break;
          }

          //expecting valid exit reason
          strLine = ""
          FindNextToken strSource, pos1, strLine, true, true
          i = Val(Logics.ConvertArg(Trim$(strLine), atNum))
          if (i > 0 && i < 5) {
            eReason = i
          } else {
            eReason = erOther
          }

          //expecting //)//
          strLine = ""
          FindNextToken strSource, pos1, strLine, true, true
          if (strLine != ")") {
            //not a simple edge code test; call it unknown
            eReason = erOther
            break;
          }

          //expecting "{"
          strLine = ""
          FindNextToken strSource, pos1, strLine, true, true
          if (strLine != "{") {
            //not a simple edge code test; call it unknown
            eReason = erOther
            break;
          }

          //expecting //new.room//
          strLine = ""
          FindNextToken strSource, pos1, strLine, true, true
          if (strLine != "new.room") {
            //complex
            //intStyle = 1 //***Style is currently not implemented, so ignore this for now
            //get next //new.room// cmd
            FindNextToken strSource, pos1, "new.room"
          }

          //verify this new.room is same as one that started the analysis
          if (lngLoc != pos1 - 7) {
            eReason = erOther
          }

          //always exit loop
          Loop Until true

          //Syle is currently not implemented leaving it at 0 is fine
          intStyle = 0

          //return exit object
          AnalyzeExit = New AGIExit
          With AnalyzeExit
          .Reason = eReason
          .Room = intRoom
          .Style = intStyle
          if (Logics(intRoom).IsRoom) {
            .Status = esOK
          } else {
            .Status = esHidden
          }
          End With

          //return the end position to continue
          lngLoc = lngEnd
          Exit Function

          ErrHandler:
          Resume Next
          }

          public static void NewWords()

          //creates a new word list file

          frmWordsEdit frmNew;

          On Error GoTo ErrHandler

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //create new file
          frmNew = New frmWordsEdit
          Load frmNew

          frmNew.NewWords
          frmNew.Show

          //restore cursor while getting resnum
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          Resume Next
          }
          public static int NoteToFreq(int NoteIn)
          //converts a midinote into a freqdivisor Value

          //valid range of NoteIn is 45-127
          //note that 121, 124, 126 NoteIn values will actually return same freqdivisor
          //values as 120, 123, 127 respectively (due to loss of resolution in freq
          //values at the high end)

          float sngFreq;

          //validate input
          if (NoteIn < 45) {
          NoteIn = 45
          } else if ( NoteIn > 127) {
          NoteIn = 127
          }

          sngFreq = 111860# / 10 ^ ((NoteIn + 36.5) * LOG10_1_12)

          NoteToFreq = CLng(sngFreq)
          }

          public static string MIDILength(int lngDuration, int lngTPQN)
          //returns a string representation of midi note length

          float sngNoteLen;

          sngNoteLen = (lngDuration / lngTPQN) * 4

          switch (sngNoteLen
          case 1
          MIDILength = "1/16"
          case 2
          MIDILength = "1/8"
          case 3
          MIDILength = "1/8*"
          case 4
          MIDILength = "1/4"
          case 5
          MIDILength = "1/4 + 1/16"
          case 6
          MIDILength = "1/4*"
          case 7
          MIDILength = "1/4**"
          case 8
          MIDILength = "1/2"
          case 9
          MIDILength = "1/2 + 1/16"
          case 10
          MIDILength = "1/2 + 1/8"
          case 11
          MIDILength = "1/2 + 1/8*"
          case 12
          MIDILength = "1/2*"
          case 13
          MIDILength = "1/2* + 1/16"
          case 14
          MIDILength = "1/2**"
          case 15
          MIDILength = "1/2** + 1/16"
          case 16
          MIDILength = "1"
          default:
          MIDILength = "<undefined>"
          }
          }

          public static int MIDINote(int lngFreq)
          //converts AGI freq offset into a MIDI note
          double dblRawNote;

          //middle C is 261.6 HZ; from midi specs,
          //middle C is a note with a Value of 60
          //this requires a shift in freq of approx. 36.376
          //however, this offset results in crappy sounding music;
          //empirically, 36.5 seems to work best

          //note that since freq divisor is in range of 0 - 1023
          //resulting midinotes are in range of 45 - 127
          //and that midinotes of 126, 124, 121 are not possible (no corresponding freq divisor)


          //if freq divisor is 0
          if (lngFreq <= 0) {
          //set note to Max
          MIDINote = 127
          } else {

          MIDINote = CLng((Log10(111860# / CDbl(lngFreq)) / LOG10_1_12) - 36.5)

          //validate
          if (MIDINote < 0) {
            MIDINote = 0
          }
          //in case note is too high,
          if (MIDINote > 127) {
            MIDINote = 127
          }
          }
          }

          public static string NoteName(byte MIDINote, int Key = 0)
          //returns a string character representing the midi note

          int lngOctave;
          int lngNote;

          switch (MIDINote
          //  case 0
          //    //zero
          //    NoteName = "OFF"
          case Is < 0
          //invalid low
          NoteName = "OFF" //"ERR_LO"
          case Is > 127
          //invalid high
          NoteName = "ERR_HI"
          default:
          //get octave and note Value
          lngOctave = MIDINote / 12 - 1
          lngNote = MIDINote Mod 12

          switch (lngNote
          case 0
            NoteName = "C"
          case 1
            NoteName = IIf(Sgn(Key) = -1, "Db", "C#")
          case 2
            NoteName = "D"
          case 3
            NoteName = IIf(Sgn(Key) = -1, "Eb", "D#")
          case 4
            NoteName = "E"
          case 5
            NoteName = "F"
          case 6
            NoteName = IIf(Sgn(Key) = -1, "Gb", "F#")
          case 7
            NoteName = "G"
          case 8
            NoteName = IIf(Sgn(Key) = -1, "Ab", "G#")
          case 9
            NoteName = "A"
          case 10
            NoteName = IIf(Sgn(Key) = -1, "Bb", "A#")
          case 11
            NoteName = "B"
          }
          }
          }


          public static tDisplayNote DisplayNote(byte MIDINote, int Key)
          //returns a note position, relative to middle c
          //as either a positive or negative Value (negative meaning higher tone)
          //
          //the Value is the offset needed to draw the note correctly
          //on the staff; one unit is a half the distance between staff lines
          //
          //the returned Value is adjusted based on the key

          int lngOctave;
          int lngNote;

          if (MIDINote < 0 || MIDINote > 127) {
          Exit Function
          }

          //get octave and note Value
          lngOctave = MIDINote / 12 - 1
          lngNote = MIDINote - ((lngOctave + 1) * 12)

          switch (lngNote
          case 0 // "-C"
          if (Key = 7) {
            DisplayNote.Pos = 1
            DisplayNote.Tone = ntNone
          } else {
            DisplayNote.Pos = 0
           if ((Key >= -5 && Key <= 1)) {
              DisplayNote.Tone = ntNone
            } else {
              DisplayNote.Tone = ntNatural
            }
          }


          case 1 // "-C#"
          if (Key >= 0) {
          DisplayNote.Pos = 0
          } else {
          DisplayNote.Pos = -1
          }
          switch (Key
          case 0, 1
          DisplayNote.Tone = ntSharp
          case -1, -2, -3
          DisplayNote.Tone = ntFlat
          default:
          DisplayNote.Tone = ntNone
          }

          case 2 // "-D"
          DisplayNote.Pos = -1
          switch (Key
          case Is > 3, Is < -3
            DisplayNote.Tone = ntNatural
          default:
            DisplayNote.Tone = ntNone
          }

          case 3 // "-D#"
          if (Key >= 0) {
            DisplayNote.Pos = -1
          } else {
            DisplayNote.Pos = -2
          }
          switch (Key
          case 0 To 3
            DisplayNote.Tone = ntSharp
          case -1
            DisplayNote.Tone = ntFlat
          default:
            DisplayNote.Tone = ntNone
          }

          case 4 // "-E"
          if (Key = -7) {
            DisplayNote.Pos = -3
            DisplayNote.Tone = ntNone
          } else {
            DisplayNote.Pos = -2
           if (Key >= -1 && Key <= 5) {
              DisplayNote.Tone = ntNone
            } else {
              DisplayNote.Tone = ntNatural
            }
          }

          case 5 // "-F"
          if (Key >= 6) {
            DisplayNote.Pos = -2
            DisplayNote.Tone = ntNone
          } else {
            DisplayNote.Pos = -3
           if (Key <= 0 && Key >= -6) {
              DisplayNote.Tone = ntNone
            } else {
              DisplayNote.Tone = ntNatural
            }
          }

          case 6 // "-F#"
          if (Key >= 0) {
            DisplayNote.Pos = -3
          } else {
            DisplayNote.Pos = -4
          }
          switch (Key
          case 0
            DisplayNote.Tone = ntSharp
          case -4 To -1
            DisplayNote.Tone = ntFlat
          default:
            DisplayNote.Tone = ntNone
          }

          case 7 // "-G"
          DisplayNote.Pos = -4
          if (Key >= 3 || Key <= -5) {
            DisplayNote.Tone = ntNatural
          } else {
            DisplayNote.Tone = ntNone
          }

          case 8 // "-G#"
          if (Key >= 0) {
            DisplayNote.Pos = -4
          } else {
            DisplayNote.Pos = -5
          }
          switch (Key
          case 0 To 2
            DisplayNote.Tone = ntSharp
          case -1, -2
            DisplayNote.Tone = ntFlat
          default:
            DisplayNote.Tone = ntNone
          }

          case 9 // "-A"
          DisplayNote.Pos = -5
          if (Key >= 5 || Key <= -3) {
            DisplayNote.Tone = ntNatural
          } else {
            DisplayNote.Tone = ntNone
          }

          case 10 // "-A#"
          if (Key >= 0) {
            DisplayNote.Pos = -5
          } else {
            DisplayNote.Pos = -6
          }
          switch (Key
          case 0 To 4
            DisplayNote.Tone = ntSharp
          default:
            DisplayNote.Tone = ntNone
          }

          case 11 // "-B"
          if (Key <= -6) {
            DisplayNote.Pos = -7
            DisplayNote.Tone = ntNone
          } else {
            DisplayNote.Pos = -6
           if (Key <= -1 || Key = 7) {
              DisplayNote.Tone = ntNatural
            } else {
              DisplayNote.Tone = ntNone
            }
          }
          }

          //adust for octave
          DisplayNote.Pos = DisplayNote.Pos + (4 - lngOctave) * 7
          }

          public static bool ChangeIntVersion(string NewVersion)

          DialogResult rtn;
          string strTemp;


          //inline error checking
          On Error Resume Next

          //if change is major
          if (Asc(NewVersion) != Asc(InterpreterVersion)) {
          //ask for confirmation
          rtn = MessageBox.Show("Changing the target interpreter version may create problems" + Environment.NewLine + _
                       "with your logic resources, due to changes in the number of" + Environment.NewLine + _
                       "commands, and their argument counts." + Environment.NewLine + Environment.NewLine + _
                       "Also, your DIR and VOL files will need to be rebuilt to" + Environment.NewLine + _
                       "make a major version change. " + Environment.NewLine + Environment.NewLine + _
                       "Continue with version change?", MessageBoxIcon.Question + MessageBoxButtons.YesNo, _
                       "Change Interpreter Version")

          if (rtn = DialogResult.No) {
            //exit
            Exit Function
          }

          //disable form until compile complete
          MDIMain.Enabled = false
          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //set up compile form
          CompStatusWin = New frmCompStatus
          CompStatusWin.SetMode 1 //rebuild only
          CompStatusWin.Show

          //setup and clear warning list
          MDIMain.ClearWarnings -1, 0

          //change the version
          InterpreterVersion = NewVersion

          //if major error,
          if (Err.Number != 0) {
            //display error message
            ErrMsgBox("Error during version change: ", "Original version has been restored.", "Change Interpreter Version"
             GoTo ErrHandler
          }

          //check for cancel
          if (CompStatusWin.CompCanceled) {
            GoTo ErrHandler
          }

          //check for errors and warnings
          if (CLng(CompStatusWin.lblErrors.Text) + CLng(CompStatusWin.lblWarnings.Text) > 0) {
            //msgbox to user
            MessageBox.Show("Errors and/or warnings were generated during game compile.", MessageBoxIcon.Information + MessageBoxButtons.OK, "Compile Game Error"

            //if errors
           if (CLng(CompStatusWin.lblErrors.Text) > 0) {
              //reuild resource list
              BuildResourceTree
            }

           if (CLng(CompStatusWin.lblWarnings.Text) > 0) {
             if (!MDIMain.picWarnings.Visible) {
                MDIMain.pnlWarnings.Visible = true;
              }
            }
          } else {
            //everything is ok
            MessageBox.Show("Version change and rebuild completed successfully.", _
                   MessageBoxIcon.Information + MessageBoxButtons.OK, "Change Interpreter Version"
          }

          //unload the compile staus form
          Unload CompStatusWin
          CompStatusWin = null

          //restore form state
          MDIMain.Enabled = true
          MDIMain.UseWaitCursor = false;
          } else {
          //ask for confirmation
          rtn = MessageBox.Show("Changing the target interpreter version may create problems with your logic resources, " + _
                "due to changes in the number of commands, and their argument counts." + Environment.NewLine + Environment.NewLine + _
                "Continue with version change?", MessageBoxIcon.Question + MessageBoxButtons.YesNo, "Change Interpreter Version")

          if (rtn = DialogResult.No) {
            //exit
            Exit Function
          }
          //just change the version
          InterpreterVersion = NewVersion
          }

          //force wag file save/update
          ChangeIntVersion = true
          Exit Function

          ErrHandler:
          //unload the compile staus form
          Unload CompStatusWin
          CompStatusWin = null

          //restore form state
          MDIMain.Enabled = true
          MDIMain.UseWaitCursor = false;

          //clean up any leftover files
          if (Asc(NewVersion) = 51 >= 3) {
          Kill GameDir + GameID + "DIR.NEW"
          } else {
          Kill GameDir + "LOGDIR.NEW"
          Kill GameDir + "PICDIR.NEW"
          Kill GameDir + "SNDDIR.NEW"
          Kill GameDir + "VIEWDIR.NEW"
          }
          Kill GameDir + "NEW_VOL.*"

          //clear errors
          Err.Clear
          }


          public static bool CompileLogic(frmLogicEdit LogicEditor, int LogicNumber)
          //compiles an ingame logic
          //assumes calling function has validated
          //the logic is in fact in a game


           int i;
          string[] strWarnings;
           string[] strErrInfo;
          bool blnDontNotify;

          On Error Resume Next

          //set flag so compiling doesn't cause unnecessary updates in preview window
          Compiling = true

          if (!LogicEditor = null) {
          //Debug.Assert MDIMain.ActiveForm Is LogicEditor

          //if source is not clean,
          if (LogicEditor.rtfLogic.Dirty) {
            //first, save the source
            LogicEditor.MenuClickSave
          }
          }

          //clear warning list for this logic
          MDIMain.ClearWarnings LogicNumber, rtLogic
          MDIMain.fgWarnings.Refresh

          //unlike other resources, the ingame logic is referenced directly
          //when being edited; so, it's possible that the logic might get closed
          //such as when changing which logic is being previewed;
          //SO, we need to make sure the logic is loaded BEFORE compiling
          if (!Logics(LogicNumber).Loaded) {
          //reload it!
          Logics(LogicNumber).Load
          }

          //compile this logic
          Logics(LogicNumber).Compile

          //check for error
          switch (Err.Number
          case 0    //no error
          if (!LogicEditor = null) {
            LogicEditor.Tag = ResourceName(Logics(LogicNumber), true, true) + " successfully compiled."
            //update statusbar
            MainStatusBar.Panels("Status").Text = LogicEditor.Tag
          }
          if (Settings.NotifyCompSuccess) {
            MsgBoxEx "Logic successfully compiled.", MessageBoxIcon.Information + MessageBoxButtons.OK, "Compile Logic", , , "Don//t show this message again", blnDontNotify
            //save the setting
            Settings.NotifyCompSuccess = !blnDontNotify
            //if hiding, update settings file
           if (!Settings.NotifyCompSuccess) {
              WriteSetting GameSettings, sGENERAL, "NotifyCompSuccess", Settings.NotifyCompSuccess
            }
          }
          //return true
          CompileLogic = true

          case WINAGI_ERR + 635
          //extract error info
          strErrInfo = Split(Err.Description, "|")
          //set error    info            linenum        logic        module
          SetError CLng(strErrInfo(0)), strErrInfo(2), LogicNumber, strErrInfo(1)
          Err.Clear
          //sound a tone
          Beep

          With MDIMain
            .AddError strErrInfo(0), Val(Left(strErrInfo(2), 4)), Right(strErrInfo(2), Len(strErrInfo(2)) - 6), LogicNumber, strErrInfo(1)
           if (!.picWarnings.Visible) {
              .pnlWarnings.Visible = true;
            }
          End With

          if (Settings.NotifyCompFail) {
            //restore cursor when showing error message
            MDIMain.UseWaitCursor = false;

            MsgBoxEx "Error detected in source. Unable to compile this logic." + Environment.NewLine + Environment.NewLine + "ERROR  in line " + strErrInfo(0) + ": " + strErrInfo(2), vbExclamation + MessageBoxButtons.OK + vbMsgBoxHelpButton, "Logic Compiler", WinAGIHelp, "htm\winagi\compilererrors.htm#" + Left(strErrInfo(2), 4), "Do not show this message again", blnDontNotify
            //save the setting
            Settings.NotifyCompFail = !blnDontNotify
            //if now hiding update settings file
           if (!Settings.NotifyCompFail) {
              WriteSetting GameSettings, sGENERAL, "NotifyCompFail", Settings.NotifyCompFail
            }
          }

          case WINAGI_ERR + 618 //not in a game
          //should NEVER get here, but...
          MessageBox.Show("Only logics that are in a game can be compiled.", MessageBoxIcon.Information + MessageBoxButtons.OK, "Compile Error"

          case WINAGI_ERR + 546  //no data to compile
          MessageBox.Show("Nothing to compile!", MessageBoxIcon.Information + MessageBoxButtons.OK, "Compile Error"

          default:
          //some other error
          ErrMsgBox("Error occurred during compilation: ", "", "Compile Error"
          }
          Err.Clear

          if (!LogicEditor = null) {
          //copy it back
          LogicEditor.LogicEdit.SetLogic Logics(LogicNumber)
          }

          //all done
          Compiling = false
          }
          public static string NewSourceName(AGILogic ThisLogic, bool InGame)
          //this ONLY gets a new name; it does not change
          //anything; not the ID, not the source file name;
          //calling function has to use the name given
          //here to do whatever is necessary to actually
          //save and update a logic source file and/or editor

          //there isn//t an //ExportLogicSource// method, because
          //managing source code separate from the actual
          //logic resource is tricky; it's easier for
          //the logic editor and preview window to manage
          //exporting source separately
          //
          //but they both need a name, and that//s easy enough
          //to do as a separate function

          DialogResult rtn;
          string strFileName;

          On Error GoTo ErrHandler

          //set up commondialog
          With MDIMain.SaveDlg
          if (InGame) {
            .DialogTitle = "Export Source"
          } else {
            .DialogTitle = "Save Source"
          }
           //if logic already has a filename,
          if (LenB(ThisLogic.SourceFile) != 0) {
            //use it
            .FullName = ThisLogic.SourceFile
          } else {
            //use default
            //if this is a filename,
           if (InStr(1, ThisLogic.ID, ".") != 0) {
              .FullName = ResDir + Left$(ThisLogic.ID, InStrRev(ThisLogic.ID, ".") - 1) + LogicSourceSettings.SourceExt
            } else {
              .FullName = ResDir + ThisLogic.ID + LogicSourceSettings.SourceExt
            }
          }
          .Filter = "WinAGI Logic Source Files (*.lgc)|*.lgc|Text files (*.txt)|*.txt|All files (*.*)|*.*"
          if (LCase$(Right$(.FullName, 4)) = ".txt") {
             .FilterIndex = 2
           } else {
             .FilterIndex = 1
           }
          .DefaultExt = Right$(LogicSourceSettings.SourceExt, Len(LogicSourceSettings.SourceExt) - 1)
          .Flags = cdlOFNHideReadOnly || cdlOFNPathMustExist || cdlOFNExplorer
          .hWndOwner = MDIMain.hWnd
          End With

          Do
          On Error Resume Next
          MDIMain.SaveDlg.ShowSaveAs

          //if canceled,
          if (Err.Number = cdlCancel) {
            //exit without doing anything
            Exit Function
          }
          On Error GoTo ErrHandler

          //get filename
          strFileName = MDIMain.SaveDlg.FullName

          //if file exists,
          if (File.Exists(strFileName)) {
            //verify replacement
            rtn = MessageBox.Show(MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?", MessageBoxButtons.YesNoCancel + MessageBoxIcon.Question, "Overwrite file?")

           if (rtn = DialogResult.Yes) {
              break;
            } else if ( rtn = DialogResult.Cancel) {
              Exit Function
            }
          } else {
            break;
          }
          Loop While true

          //pass back this name
          NewSourceName = strFileName
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static bool ExportLogic(byte LogicNumber)
          //exports this logic

          //Logics are different from other resources in that
          //they can only be exported if they are in a game;
          //therefore, the passed argument is the resource number of
          //the logic to be exported, NOT the actual logic itself
          //
          //NOTE that this exports the compiled logic resource;
          //to export source code, use the ExportLogicSource function

          string strFileName;
          DialogResult rtn;
          bool blnLoaded;

          On Error GoTo ErrHandler

          With MDIMain.SaveDlg
          //set cmndlg properties for logic source file
          .DialogTitle = "Export Logic File"
          .Filter = "Logic Files (*.agl)|*.agl|All files (*.*)|*.*"
          .FilterIndex = GameSettings.GetSetting(sLOGICS, sEXPFILTER, 1)
          //set default name
          .DefaultExt = "agl"
          .FullName = ResDir + Logics(LogicNumber).ID + ".agl"

          .Flags = cdlOFNHideReadOnly || cdlOFNPathMustExist || cdlOFNExplorer
          .hWndOwner = MDIMain.hWnd
          End With

          On Error Resume Next
          Do
          MDIMain.SaveDlg.ShowSaveAs
          //if canceled
          if (Err.Number = cdlCancel) {
            //exit without doing anything
            Exit Function
          }

          //get file name
          strFileName = MDIMain.SaveDlg.FullName

          //if file exists,
          if (File.Exists(strFileName)) {
            //verify replacement
            rtn = MessageBox.Show(MDIMain.SaveDlg.FileName + " already exists. Do you want to overwrite it?", MessageBoxButtons.YesNoCancel + MessageBoxIcon.Question, "Overwrite file?")

           if (rtn = DialogResult.Yes) {
              break;
            } else if ( rtn = DialogResult.Cancel) {
              Exit Function
            }
          } else {
            break;
          }
          Loop While true

          //need to make sure it's loaded
          blnLoaded = Logics(LogicNumber).Loaded
          if (!blnLoaded) {
          Logics(LogicNumber).Load
          }
          //export the resource
          Logics(LogicNumber).Export strFileName

          //if error,
          if (Err.Number != 0) {
          ErrMsgBox("An error occurred while exporting this file: ", "", "Export File Error"
          Exit Function
          }

          //unload if not previously loaded
          if (!blnLoaded) {
          Logics(LogicNumber).Unload
          }

          ExportLogic = true
          Exit Function

          ErrHandler:
          //  //Debug.Assert false
          Resume Next
          }




          int FindInClosedLogics(string FindText, FindDirection FindDir, bool MatchWord, bool MatchCase, AGIResType SearchType = AGIResType.rtNone)

          //find next closed logic that has search text in it;
          //if found, return the logic number
          //if not found, return -1

          int i;
          static int LogNum;
          StringComparison vbcComp;
          bool blnLoaded;

          On Error GoTo ErrHandler

          //if this is first time through
          if (!ClosedLogics) {
          //start with first logic (which sets ClosedLogics flag)
          LogNum = NextClosedLogic(-1)
          SearchLogCount = Logics.Count
          SearchLogVal = LogicEditors.Count
          } else {
          LogNum = NextClosedLogic(LogNum)
          //search logic count and value are current
          }

          //show ProgressWin form
          Load ProgressWin
          With ProgressWin
          .Text = "Find"
          if (LogNum = -1) {
            .lblProgress.Text = "Searching..."
          } else {
            .lblProgress.Text = "Searching " + Logics(LogNum).ID + "..."
          }
          .pgbStatus.Max = SearchLogCount
          .pgbStatus.Value = SearchLogVal //- 1
          .Show vbModeless, MDIMain
          .Refresh
          End With
          //Debug.Assert Screen.MousePointer != Cursors.Default

          //set comparison method for string search,
          vbcComp = CLng(MatchCase) + 1 // CLng(true) + 1 = 0 = vbBinaryCompare; Clng(false) + 1 = 1 = StringComparison.OrdinalIgnoreCase

          Do Until LogNum = -1
          //update the ProgressWin form
          With ProgressWin
            .pgbStatus.Value = .pgbStatus.Value + 1
            .lblProgress.Text = "Searching " + Logics(LogNum).ID + "..."
            .Refresh
          End With

          //if not loaded,
          blnLoaded = Logics(LogNum).Loaded
          if (!blnLoaded) {
            Logics(LogNum).Load
          }

          //if searching up,
          //if direction is up,
          if (FindDir = fdUp) {
           if (MatchWord) {
             if (FindWholeWord(1, Logics(LogNum).SourceText, FindText, MatchCase, true, SearchType) != 0) {
                break;
              }
            } else {
             if (InStrRev(Logics(LogNum).SourceText, FindText, -1, vbcComp) != 0) {
                break;
              }
            }
          } else {
            //search strategy depends on synonym search value
           if (!GFindSynonym) {
             if (MatchWord) {
               if (FindWholeWord(1, Logics(LogNum).SourceText, FindText, MatchCase, false, SearchType) != 0) {
                  break;
                }
              } else {
               if (InStr(1, Logics(LogNum).SourceText, FindText, vbcComp) != 0) {
                  break;
                }
              }
            } else {
              //Matchword is always true; but since words are surrounded by quotes, it wont matter
              //so we use Instr

              //step through each word in the word group; if the word is found in this logic,
              //then we stop
              For i = 0 To WordEditor.WordsEdit.GroupN(GFindGrpNum).WordCount - 1
               if (InStr(1, Logics(LogNum).SourceText, QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).Word(i) + QUOTECHAR, vbcComp) != 0) {
                  break;
                }
              Next i
            }
          }

          //not found-unload this logic
          if (!blnLoaded) {
            Logics(LogNum).Unload
          }

          //try next logic
          LogNum = NextClosedLogic(LogNum)

          //loop is exited by finding the searchtext or reaching end of search area
          Loop

          //if not found,
          if (LogNum = -1) {
          //just exit
          FindInClosedLogics = LogNum
          //done with ProgressWin form; save current searchlog value
          SearchLogVal = ProgressWin.pgbStatus.Value
          Unload ProgressWin
          Exit Function
          }

          if (!blnLoaded) {
          Logics(LogNum).Unload
          }

          //open editor, if able (this will reset the cursor to normal;
          //we need to force it back to hourglass
          //Debug.Assert Screen.MousePointer = vbHourglass
          OpenLogic LogNum, true
          //Debug.Assert MDIMain.UseWaitCursor = false;
          Screen.MousePointer = vbHourglass

          //now locate this window
          For i = LogicEditors.Count To 1 Step -1
          if (LogicEditors(i).FormMode = fmLogic) {
           if (LogicEditors(i).LogicEdit.Number = LogNum) {
              Exit For
            }
          }
          Next i

          //if not found (i = 0)
          if (i = 0) {
          //must have been an error
          MessageBox.Show(QUOTECHAR + FindText + QUOTECHAR + " was found in logic " + CStr(LogNum) + " but an error occurred while opening the file. Try opening the logic manually and then try the search again.", MessageBoxIcon.Information, "Find In Logic"
          //unload logic
          Logics(LogNum).Unload
          //return -1
          FindInClosedLogics = -1
          //done with ProgressWin form
          SearchLogVal = ProgressWin.pgbStatus.Value
          Unload ProgressWin
          Exit Function
          }

          //hide ProgressWin form
          SearchLogVal = ProgressWin.pgbStatus.Value
          Unload ProgressWin

          //return this window number
          FindInClosedLogics = i

          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }


          int NextClosedLogic(int OldLogNum)

          static int[] LogWin;
          static int WinCount;
          bool Closed;
          int i;

          On Error GoTo ErrHandler

          //if this is first time through
          if (!ClosedLogics) {
          //build list of windows
          WinCount = LogicEditors.Count
          ReDim LogWin(WinCount)
          For i = 1 To WinCount
            //assume this window is not a logic in a game
            LogWin(i) = -1
            //if a logic editor
           if (LogicEditors(i).FormMode = fmLogic) {
              //if in a game
             if (LogicEditors(i).InGame) {
                //store this number
                LogWin(i) = LogicEditors(i).LogicNumber
              }
            }
          Next i
          //set flag
          ClosedLogics = true
          }

          //increment old log number
          OldLogNum = OldLogNum + 1

          //use do loop to find next closed logic
          //try until end of logics reached
          Do Until OldLogNum = 256
          //if this number is a valid logic
          if (Logics.Exists(OldLogNum)) {
            //assume closed
            Closed = true

            //check to see if logic is not already open
            For i = 1 To WinCount
              //if editor(i) is for this logic
             if (LogWin(i) = OldLogNum) {
                //it's not closed; it's open for editing
                Closed = false
                Exit For
              }
            Next i
            //if good, then exit
           if (Closed) {
              break;
            }
          } else {
            //not a valid logic;
            Closed = false
          }

          //increment old log number
          OldLogNum = OldLogNum + 1
          Loop

          //if end reached,
          if (OldLogNum = 256) {
          //return end code (-1)
          NextClosedLogic = -1
          } else {
          //return the number
          NextClosedLogic = OldLogNum
          }
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static void OpenLayout()

          if (!GameLoaded) {
          return;
          }

          //if layout editor is currently open
          if (LEInUse) {
          //just bring it in focus
          LayoutEditor.Focus()
          //if minimized
          if (LayoutEditor.WindowState = vbMinimized) {
            //restore it
            LayoutEditor.WindowState = vbNormal
          }

          } else {
          //open the layout for the current game
          LayoutEditor = New frmLayout
          LayoutEditor.LoadLayout
          }
          }


          public static bool OpenWords(bool InGame = true)

          string strFileName;

          frmWordsEdit frmNew;
           Form tmpForm;

          On Error GoTo ErrHandler

          //if a game is loaded AND looking for ingame editor
          if (GameLoaded && InGame) {
          //if it is in use,
          if (WEInUse) {
            //just switch to it
            WordEditor.Focus()
          } else {
            //load it
            WEInUse = true
            WordEditor = New frmWordsEdit
            Load WordEditor
            //set ingame status first, so caption will indicate correctly
            WordEditor.InGame = true

            On Error Resume Next
            WordEditor.LoadWords WordList.ResFile
           if (Err.Number != 0) {
              WordEditor = null
              WEInUse = false
              Exit Function
            }
            On Error GoTo ErrHandler
            WordEditor.Show
          }
          } else {
          //either a game is NOT loaded, OR we are forcing a load from file

          //get an word file
          With OpenDlg
            .Flags = cdlOFNHideReadOnly
            .DialogTitle = "Import Words File"
            .Filter = "AGI Word file|WORDS.TOK|WinAGI Words file (*.agw)|*.agw|All files (*.*)|*.*"
            .DefaultExt = ""
            .FilterIndex = GameSettings.GetSetting("Words", sOPENFILTER, 1)
            .FileName = ""
            .InitDir = DefaultResDir

            .ShowOpen
            strFileName = .FileName
            //save filter
            WriteSetting GameSettings, "Words", sOPENFILTER, .FilterIndex
            DefaultResDir = JustPath(.FileName)

          End With

          //check if already open
          foreach (tmpForm In Forms
           if (tmpForm.Name = "frmWordsEdit") {
             if (tmpForm.WordsEdit.ResFile = strFileName && !tmpForm.InGame) {
                //just shift focus
                tmpForm.Focus()
                OpenWords = true
                Exit Function
              }
            }
          Next

          //not open yet; create new form
          //and open this form into it
          frmNew = New frmWordsEdit

          Load frmNew
          On Error Resume Next
          frmNew.LoadWords strFileName
          if (Err.Number != 0) {
            frmNew = null
            Exit Function
          } else {
            frmNew.Show
          }
          }

          OpenWords = true
          Exit Function

          ErrHandler:
          //if user canceled the dialogbox,
          if (Err.Number = cdlCancel) {
          Exit Function
          }

          //Debug.Assert false
          Resume Next
          }

          public static bool OpenObjects(bool InGame = true)

          string strFileName;

          frmObjectEdit frmNew;
           Form tmpForm;

          On Error GoTo ErrHandler

          //if a game is loaded AND looking for ingame editor
          if (GameLoaded && InGame) {
          //if it is in use,
          if (OEInUse) {
            //just switch to it
            ObjectEditor.Focus()
          } else {
            //load it
            OEInUse = true
            ObjectEditor = New frmObjectEdit
            Load ObjectEditor
            ObjectEditor.InGame = true

            //set ingame status first, so caption will indicate correctly
            On Error Resume Next
            ObjectEditor.LoadObjects InvObjects.ResFile
           if (Err.Number != 0) {
              ObjectEditor = null
              OEInUse = false
              Exit Function
            }
            On Error GoTo ErrHandler
            ObjectEditor.Show
            ObjectEditor.fgObjects.Focus()
          }
          } else {
          //either a game is NOT loaded, OR we are forcing a load from file

          //get an object file
          With OpenDlg
            .Flags = cdlOFNHideReadOnly
            .DialogTitle = "Import Object File"
            .Filter = "WinAGI Objects file (*.ago)|*.ago|AGI OBJECT file|OBJECT|All files (*.*)|*.*"
            .DefaultExt = ""
            .FilterIndex = GameSettings.GetSetting("Objects", sOPENFILTER, 2)
            .FileName = ""
            .InitDir = DefaultResDir

            .ShowOpen

            strFileName = .FileName

            //save filter
            WriteSetting GameSettings, "Objects", sOPENFILTER, .FilterIndex
            DefaultResDir = JustPath(.FileName)

          End With

          //check if already open
          foreach (tmpForm In Forms
           if (tmpForm.Name = "frmObjectEdit") {
             if (tmpForm.ObjectsEdit.ResFile = strFileName && !tmpForm.InGame) {
                //just shift focus
                tmpForm.Focus()
                OpenObjects = true
                Exit Function
              }
            }
          Next

          //not open yet; create new form
          //and open this form into it
          frmNew = New frmObjectEdit

          Load frmNew
          On Error Resume Next
          frmNew.LoadObjects strFileName
          if (Err.Number != 0) {
            frmNew = null
            Exit Function
          } else {
            frmNew.Show
            frmNew.fgObjects.Focus()
          }
          }

          OpenObjects = true
          Exit Function

          ErrHandler:
          //if user canceled the dialogbox,
          if (Err.Number = cdlCancel) {
          Exit Function
          }

          Resume Next
          }


          public static void RemoveLogic(byte LogicNum)
          //removes a logic from the game, and updates
          //preview and resource windows
          //
          //it also updates layout editor, if it is open
          //and deletes the source code file from source directory

          int i;
           string strSourceFile;
          bool blnIsRoom;

          On Error GoTo ErrHandler

          //need to load logic to access sourccode
          //Debug.Assert Logics.Exists(LogicNum)
          if (!Logics.Exists(LogicNum)) {
          //raise error
          On Error GoTo 0: Err.Raise WINAGI_ERR + 501, "ResMan", "Invalid Logic number passed to RemoveLogic (logic does not exist)"
          return;
          }

          if (!Logics(LogicNum).Loaded) {
          Logics(LogicNum).Load
          }
          strSourceFile = Logics(LogicNum).SourceFile

          blnIsRoom = Logics(LogicNum).IsRoom

          //remove it from game
          Logics.Remove LogicNum

          //if using layout editor AND is a room,
          if (UseLE && blnIsRoom) {
          //update layout editor and layout data file to show this room is now gone
          UpdateExitInfo euRemoveRoom, LogicNum, null
          }

          switch (Settings.ResListType
          case 1
          With MDIMain.tvwResources
            //remove it from resource list
            .Nodes.Remove .Nodes["l" + CStr(LogicNum)).Index

          //last node marker is no longer accurate; reset
            MDIMain.LastNodeName = "";

           if (.SelectedItem.Parent = null) {
              //it's the game node
              MDIMain.SelectResource rtGame, -1
            } else if ( .SelectedItem.Parent.Parent = null) {
              //it's a resource header
              MDIMain.SelectResource .SelectedItem.Index - 2, -1
            } else {
              //it's a resource
              MDIMain.SelectResource .SelectedItem.Parent.Index - 2, CLng(.SelectedItem.Tag)
            }
          End With

          case 2
          //only need to remove if logics are listed
          if (MDIMain.cmbResType.SelectedIndex = 1) {
            //remove it
            MDIMain.lstResources.ListItems.Remove MDIMain.lstResources.ListItems("l" + CStr(LogicNum)).Index
            //use click event to update
            MDIMain.lstResources_Click
            MDIMain.lstResources.SelectedItem.Selected = true
          }
          }

          //if an editor is open
          For i = 1 To LogicEditors.Count
          if (LogicEditors(i).FormMode = fmLogic) {
           if (LogicEditors(i).InGame && LogicEditors(i).LogicNumber = LogicNum) {
              //set number to -1 to force close
              LogicEditors(i).LogicNumber = -1
              //close it
              Unload LogicEditors(i)
              Exit For
            }
          }
          Next i

          //disposition any existing resource file
          if (File.Exists(strSourceFile)) {
          KillCopyFile strSourceFile, Settings.RenameDelRes
          }

          //update the logic tooltip lookup table
          With IDefLookup(LogicNum)
          .Name = ""
          .Value = ""
          .Type = 11 //set to a value > highest type
          End With
          //then let open logic editors know
          if (LogicEditors.Count > 0) {
          For i = 1 To LogicEditors.Count
            LogicEditors(i).ListDirty = true
          Next i
          }
          return;

          ErrHandler:
          //if error is invalid resid,
          if (Err.Number = WINAGI_ERR + 617) {
          //pass it along
          On Error GoTo 0: Err.Raise Err.Number, Err.Source, Err.Description
          return;
          }
          Resume Next
          }

          public static void NewTextFile()
          //opens a new text editor

          frmTextEdit frmNew;

          On Error GoTo ErrHandler

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //open a new text editing window
          frmNew = New frmTextEdit

          With frmNew
          //set caption
          .Text = "New Text File"

          //clear undo buffer
          .rtfLogic.EmptyUndo
          //reset dirty flag
          .rtfLogic.Dirty = false

          //show the form
          .Show

          //maximize, if that//s the current setting
          if (Settings.MaximizeLogics) {
            .WindowState = vbMaximized
          }
          End With

          //add form to collection
          LogicEditors.Add frmNew

          //restore main form mousepointer and exit
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }





          public static void OpenTextFile(string strOpenFile, bool Quiet = false)
          //this method opens a text editor window

          short i;

           string strInput;
          bool blnWinOpen;
           frmTextEdit frmNew;
          int Cscore;

          On Error GoTo ErrHandler

          //show wait cursor
          MDIMain.UseWaitCursor = true;

          //step through all log windows
          For i = 1 To LogicEditors.Count
          //if a text editor
          if (LogicEditors(i).FormMode = fmText) {
            //if name matches
           if (LogicEditors(i).FileName = strOpenFile) {
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
          frmNew = New frmTextEdit

          //if file does not exist,
          if (!CanAccessFile(strOpenFile)) {
          //file does not exist,
          Unload frmNew
          if (!Quiet) {
            MessageBox.Show("File not found:" + Environment.NewLine + strOpenFile, , "File Open Error"
          }
          //restore mousepointer and exit
          MDIMain.UseWaitCursor = false;
          return;
          }

          With frmNew

          //assign text to text box
          .rtfLogic.LoadFile strOpenFile, reOpenSaveText + reOpenSaveOpenAlways, 437 // Text = strInput
          //if not using syntax highlighting
          if (!Settings.HighlightText) {
            //force refresh
            .rtfLogic.RefreshHighlight
          }

          //clear undo buffer
          .rtfLogic.EmptyUndo
          //clear dirty flag
          .rtfLogic.Dirty = false

          //set filename, caption
          .FileName = strOpenFile
          .Text = "Text editor - " + Path.GetFileName(strOpenFile)

          //reset dirty flag
          .rtfLogic.Dirty = false

          //maximize, if that//s the current setting
          if (Settings.MaximizeLogics) {
            .WindowState = vbMaximized
          }
          End With

          //add form to collection
          LogicEditors.Add frmNew

          //show it
          frmNew.Show

          //restore mousepointer and exit
          MDIMain.UseWaitCursor = false;
          return;

          ErrHandler:
          //show error msg
          if (!Quiet) {
          ErrMsgBox("Error while trying to open text file: ", "", "File Open Error"
          }
          Err.Clear
          //restore main form mousepointer and exit
          MDIMain.UseWaitCursor = false;
          }

          public static string TokenFromCursor(RichEdAGI rtfLogic, bool NoComment = true, bool FullString = false)

          //returns the token/separator at the current cursor location
          //ignores tokens in a comment if nocomment is true

          //preference is for a token; if a non-token is found, search
          //again one character back to see if there//s a token there
          //and if there is, return it

          // if FullString is set, only a full string (with starting/ending
          // quotes) will be returned

          int lngPos;
           string strToken;
           int lngCount;
          Range tmpRange;
            bool blnToken;

          bool blnQuote;

          On Error GoTo ErrHandler

          //get line (use range property of richedit)
          lngPos = rtfLogic.Selection.Range.StartPos
          tmpRange = rtfLogic.Range(lngPos, lngPos)
          tmpRange.StartOf reLine, true
          tmpRange.EndOf reLine, true
          strToken = tmpRange.Text
          // strip off ending CR, if there is one
          if (Asc(Right(strToken, 1)) = 13) {
          strToken = Left(strToken, Len(strToken) - 1)
          }

          //get position of cursor in the line
          //(need to add 1 because the rtf editor position
          // is zero-based, but string functions are 1 based)
          lngPos = lngPos - tmpRange.StartPos + 1
          tmpRange = null

          if (NoComment) {
          //strip off comments
          strToken = StripComments(strToken, "", true)
          //if cursor pos is greater than length of line without
          //comments, it means cursor was in a comment
          if (lngPos > Len(strToken) + 1) {
            Exit Function
          }
          }

          //check for zero length line
          if (Len(strToken) = 0) {
          //nothing to return
          TokenFromCursor = ""
          Exit Function
          }

          // if looking for a string
          if (FullString) {
          //find starting quote
          lngPos = InStrRev(strToken, QUOTECHAR, lngPos)
          if (lngPos = 0) {
            //no starting quote
            TokenFromCursor = ""
            Exit Function
          }
          strToken = Right(strToken, Len(strToken) - lngPos + 1)

          //find ending quote
          lngPos = InStr(2, strToken, QUOTECHAR)
          if (lngPos = 0) {
            //no ending quote
            TokenFromCursor = ""
            Exit Function
          }

          TokenFromCursor = Left(strToken, lngPos)
          Exit Function
          }

          //if at end
          if (lngPos = Len(strToken) + 1) {
          //only look left
          //previous char determines token/non-token
          blnToken = IsTokenChar(Asc(Mid$(strToken, lngPos - 1)), NoComment)

          //if at beginning
          } else if ( lngPos = 1) {
          //spaces always trump the token check
          if (Asc(Mid$(strToken, lngPos)) = 32) {
            //no token
            TokenFromCursor = ""
            Exit Function
          }

          //only look right
          blnToken = IsTokenChar(Asc(Mid$(strToken, lngPos)), NoComment)

          } else {
          //token or non-token?
          blnToken = IsTokenChar(Asc(Mid$(strToken, lngPos)), NoComment)
          //if current char is NOT a token char
          if (!blnToken) {
            //try char behind
           if (IsTokenChar(Asc(Mid$(strToken, lngPos - 1)), NoComment)) {
              //find this token
              blnToken = true
              lngPos = lngPos - 1
            }
          }
          }

          //now move cursor backward until beginning of line reached
          Do Until lngPos = 1
          //always stop at a space
          if (Asc(Mid$(strToken, lngPos - 1)) = 32) {
            break;
          } else {
           if (IsTokenChar(Asc(Mid$(strToken, lngPos - 1)), NoComment) != blnToken) {
              break;
            }
          }

          lngPos = lngPos - 1
          lngCount = lngCount + 1
          Loop

          //trim off front of string
          TokenFromCursor = Right$(strToken, Len(strToken) - lngPos + 1)
          lngPos = lngCount + 1

          //if trimmed string is empty,
          if (Len(TokenFromCursor) = 0) {
          //no need to keep looking
          Exit Function
          }

          //if already at end of command
          if (lngPos >= Len(TokenFromCursor)) {
          Exit Function
          }

          //if looking for a non-token, and first char is a space
          if (!blnToken && Asc(TokenFromCursor) = 32) {
          //means cursor is within whitespace
          TokenFromCursor = ""
          Exit Function
          }

          //now find end of the cmd
          Do
          //always stop at a space
          if (Asc(Mid$(TokenFromCursor, lngPos + 1)) = 32) {
            break;
          } else {
           if (IsTokenChar(Asc(Mid$(TokenFromCursor, lngPos + 1)), NoComment) != blnToken) {
              break;
            }
          }

          lngPos++;
          Loop Until lngPos >= Len(TokenFromCursor) // - 1

          //trim off end of cmd and return the token
          TokenFromCursor = Left$(TokenFromCursor, lngPos)

          //special case - quotes kinda mess things up
          if (TokenFromCursor = QUOTECHAR + QUOTECHAR) {
          //ok
          Exit Function
          } else if ( TokenFromCursor = QUOTECHAR) {
          //don't bother if a single quote
          TokenFromCursor = ""
          Exit Function
          }

          // if a non-token is next to a quote, the quote
          //gets stuck on the returned value; we don't want it
          if (!blnToken && Asc(Right(TokenFromCursor, 1)) = 34) {
          TokenFromCursor = Left(TokenFromCursor, Len(TokenFromCursor) - 1)
          }

          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static AGIColors GetAGIColor(int lngEGAColor)
            {
          //this function is used to quickly convert an EGA color
          //into corresponding AGI color index
          //
          //used primarily by View Editor when dealing with cutting/pasting/flipping
          //bitmap sections

          byte i;
          byte bytCloseColor;
            int lngDiff;
          byte bytRed, bytBlue, bytGreen;
          int newdiff;

          On Error GoTo ErrHandler

          bytRed = lngEGAColor >> 16
          bytGreen = (lngEGAColor && 0xFFFF) >> 8
          bytBlue = lngEGAColor && 0xFF

          //set initial difference to ensure first number resets it
          lngDiff = 0xFFFFFF
          i = 0

          //loop until exact match is found,
          //or until all colors are compared
          Do Until lngDiff = 0 || i = 16
          //get new diff
          bytRed = lngEGACol(i) >> 16
          bytGreen = (lngEGACol(i) && 0xFFFF) >> 8
          bytBlue = lngEGACol(i) && 0xFF
          newdiff = (Abs((bytRed - lngEGAColor) >> 16) + Abs(bytGreen - (lngEGAColor && 0xFFFF) >> 8) + Abs(bytBlue - lngEGAColor && 0xFF))

          //if difference between this color and color i is less than difference
          if (newdiff < lngDiff) {
            lngDiff = newdiff
            bytCloseColor = i
          }
          i = i + 1
          Loop

          //return best match
          GetAGIColor = bytCloseColor
          Exit Function

          ErrHandler:
          //Debug.Assert false
          //just return black
          GetAGIColor = agBlack
          }

          public static int CmdType(string CommandText)
          //converts a command string into a command Value
          //
          //0 = any draw command
          //1 = set vis color command
          //2 = set pri color command
          //3 = set plot pen parameters
          //4 = ERROR (an invalid command); these are very RARE

          On Error GoTo ErrHandler

          switch (Left$(CommandText, 3)
          case "Vis"
          CmdType = 1
          case "Pri"
          CmdType = 2
          case "Set"
          CmdType = 3
          case "ERR"
          CmdType = 4
          }
          Exit Function

          ErrHandler:
          //for any error, set Type to zero
          Err.Clear
          CmdType = 0

          }

          public static string CoordText(byte X, byte Y)
          //this function creates the coordinate text in the form
          // (X, Y)

          CoordText = "(" + CStr(X) + ", " + CStr(Y) + ")"
          }

          public static byte GetPriBand(byte Y, byte PriBase = 48)

          //convert Y Value into appropriate priority band

          if (Y < PriBase) {
          GetPriBand = 4
          } else {
          GetPriBand = Int((CLng(Y) - PriBase) / (168 - PriBase) * 10) + 5
          }
          }
          public static PT ExtractCoordinates(string TreeText)
          string[] strCoord;
          int lngPos;

          On Error GoTo ErrHandler

          //splits a treetext string into x and Y coordinates
          //input will always be "(#,#)" or "# -- (#,#)"

          //determine case of input
          if (InStr(1, TreeText, "-") != 0) {
          //need to skip pattern number and parentheses
          lngPos = InStr(5, TreeText, "(")
          strCoord = Split(Mid$(TreeText, lngPos + 1, Len(TreeText) - lngPos - 1), ",")
          } else {
          //strip off parentheses
          strCoord = Split(Mid$(TreeText, 2, Len(TreeText) - 2), ",")
          }

          //convert to long integers
          ExtractCoordinates.X = CLng(Val(strCoord(0)))
          ExtractCoordinates.Y = CLng(strCoord(1))
          Exit Function

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static void AssignItems(Control ctl, List<string> strlItems, bool blnNumbers = false)
          int i
          //ctl must be a list box or combobox
          if (!((TypeOf ctl Is ComboBox) || (TypeOf ctl Is ListBox))) {
          //need error statement here
          return;
          }

          //clear the control
          ctl.Clear
          //add all items  in stringlist into control
          if (blnNumbers) {
          For i = 0 To strlItems.Count - 1
            ctl.AddItem CStr(i + 1) + ". " + strlItems.StringLine(i)
          Next i
          } else {
          For i = 0 To strlItems.Count - 1
            ctl.AddItem strlItems.StringLine(i)
          Next i
          }
          }

          public static void ExpandSelection(RichEdAGI rtfLogic, bool blnInQuotes, bool Force = false)

          //if the current selection is an insertion point
          //(startpos = endpos), it will expand the selection
          //to encompass the current word (expanding both
          //forwards and backwards)

          // if Force is true, it will expand even if something is selected

          On Error GoTo ErrHandler

          int lngLineStart, lngStart, lngEnd;
          int i, j, lngChar;
          int rtn,;
           string strLine;
            int lngPos;

          if (rtfLogic.Selection.Range.Length != 0 && !Force) {
          return;
          }

          //separators are any character EXCEPT:
          // #, $, %, ., 0-9, @, A-Z, _, a-z
          //(codes 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122)
          //strings need to expand out to valid quote marks

          //only need to search on current line, so
          //extract it to simplify the text searching

          //get line where cursor is (don't forget to add 1
          lngPos = rtfLogic.Selection.Range.StartPos
          rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, lngPos, 0)
          //get the startpos of this line
          lngLineStart = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, rtn, 0)
          //get current row
          strLine = rtfLogic.Range(lngLineStart, lngLineStart).Expand(reLine).Text

          //set start to current start of selection
          lngStart = lngPos
          // and end to current end of selection
          lngEnd = rtfLogic.Selection.Range.EndPos

          //for rest of this function, lngPos is used by vb functions
          // so need to add one because vb functions are 1-based, and
          //rtf ranges are 0-based
          lngPos++;

          if (blnInQuotes) {
          //move startpos backward until separator found

          //i is relative position of starting point in current line;
          //start with i pointing to previous char, then enter do loop
          //don't forget to adjust startpos by 1 too)
          i = lngPos - lngLineStart - 1

          Do While i >= 1
            switch (AscW(Mid$(strLine, i))
            case 34
              //if valid quote, stop
             if (IsValidQuote(strLine, i)) {
                //starting quote found
                lngStart = lngStart - 1
                break;
              }

            default:
              //everything else is OK
            }
            i = i - 1
            lngStart = lngStart - 1
          Loop

          //move endpos forward until separator found
          j = lngPos - lngLineStart + 1
          lngEnd = lngPos
          Do While j < Len(strLine)
            switch (AscW(Mid$(strLine, j))
            case 34
              //if valid quote, stop
             if (IsValidQuote(strLine, i)) {
                //ending quote found
                lngEnd = lngEnd + 1
                break;
              }

            default:
              //everything else is ok
            }
            j = j + 1
            lngEnd = lngEnd + 1
          Loop

          } else {
          //i is relative position of starting point in current line;
          //start with i pointing to previous char, then enter do loop
          i = lngPos - lngLineStart //- 1

          // if forcing, only expand if starting char is a token char
          if (Force) {
            lngChar = AscW(Mid(strLine, i))
            switch (lngChar
            case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122, Is > 126
              //separators are any character EXCEPT:
              // #, $, %, ., 0-9, @, A-Z, _, a-z, and all extended characters
              //(codes 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122)
            default:
              return;
            }
          }
          //char is a word token; back up one more an start the loop
          i = i - 1

          //move startpos backward until separator found
          Do While i >= 1
            lngChar = AscW(Mid$(strLine, i, 1))
            switch (lngChar
            case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122, Is > 126
              //these are OK

            default:
              //everything else is no good
              break;
            }
            i = i - 1
            lngStart = lngStart - 1
          Loop

          //move endpos forward until separator found
          j = lngEnd - lngLineStart + 1
          Do While j < Len(strLine)
            lngChar = AscW(Mid$(strLine, j, 1))
            switch (AscW(Mid$(strLine, j))
            case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122, Is > 126
              //these are OK

            default:
              //everything else is no good
              break;
            }
            j = j + 1
            lngEnd = lngEnd + 1
          Loop
          }

          //set start and end pos (need to freeze the
          //control so it won//t scroll horizontally if
          //the new start location is at beginning of
          //a line where previous line extends past
          //the edge of visible text)

          //if no need to expand
          if (rtfLogic.Selection.Range.StartPos = lngStart && rtfLogic.Selection.Range.EndPos = lngEnd) {
          //just exit
          return;
          }

          rtfLogic.Freeze
          rtfLogic.Selection.Range.StartPos = lngStart
          rtfLogic.Selection.Range.EndPos = lngEnd
          //there is a very strange glitch here! if the original cursor
          // position is directly in front of a period, the startpos
          //does change when trying to set it to lngStart on first try
          // a second call will then set it
          if (rtfLogic.Selection.Range.StartPos != lngStart) {
          rtfLogic.Selection.Range.StartPos = lngStart
          }
          rtfLogic.Unfreeze
          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          public static void RemoveSound(byte SndNum)
          //removes a view from the game, and updates
          //preview and resource windows
          //
          //and deletes resource file in source directory

          int i;
           string strSndFile;

          On Error GoTo ErrHandler

          strSndFile = ResDir + Sounds(SndNum).ID + ".ags"

          if (!Sounds.Exists(SndNum)) {
          //raise error
          On Error GoTo 0: Err.Raise WINAGI_ERR + 503, "ResMan", "Invalid Sound number passed to RemoveSound (sound does not exist)"
          return;
          }

          //remove it from game
          Sounds.Remove SndNum

          switch (Settings.ResListType
          case 1
          With MDIMain.tvwResources
            //remove it from resource list
            .Nodes.Remove MDIMain.tvwResources.Nodes["s" + CStr(SndNum)).Index

          //last node marker is no longer accurate; reset
            MDIMain.LastNodeName = "";

           if (.SelectedItem.Parent = null) {
              //it's the game node
              MDIMain.SelectResource rtGame, -1
            } else if ( .SelectedItem.Parent.Parent = null) {
              //it's a resource header
              MDIMain.SelectResource .SelectedItem.Index - 2, -1
            } else {
              //it's a resource
              MDIMain.SelectResource .SelectedItem.Parent.Index - 2, CLng(.SelectedItem.Tag)
            }
          End With

          case 2
          //only need to remove if sounds are listed
          if (MDIMain.cmbResType.SelectedIndex = 3) {
            //remove it
            MDIMain.lstResources.ListItems.Remove MDIMain.lstResources.ListItems("s" + CStr(SndNum)).Index
            //use click event to update
            MDIMain.lstResources_Click
            MDIMain.lstResources.SelectedItem.Selected = true
          }
          }


          //if an editor is open
          For i = 1 To SoundEditors.Count
          if (SoundEditors(i).InGame && SoundEditors(i).SoundNumber = SndNum) {
            //set number to -1 to force close
            SoundEditors(i).SoundNumber = -1
            //close it
            Unload SoundEditors(i)
            Exit For
          }
          Next i

          if (File.Exists(strSndFile)) {
          KillCopyFile strSndFile, Settings.RenameDelRes
          }

          //update the logic tooltip lookup table
          With IDefLookup(SndNum + 512)
          .Name = ""
          .Value = ""
          .Type = 11 //set to a value > highest type
          End With
          //then let open logic editors know
          if (LogicEditors.Count > 0) {
          For i = 1 To LogicEditors.Count
            LogicEditors(i).ListDirty = true
          Next i
          }
          return;

          ErrHandler:
          //if error is invalid resid,
          if (Err.Number = WINAGI_ERR + 617) {
          //pass it along
          On Error GoTo 0: Err.Raise Err.Number, Err.Source, Err.Description
          return;
          }

          Resume Next
          }

          public static void RemoveView(byte ViewNum)
          //removes a view from the game, and updates
          //preview and resource windows
          //
          //and deletes resource file from source dir

          int i;
           string strViewFile;

          On Error GoTo ErrHandler

          strViewFile = ResDir + Views(ViewNum).ID + ".agv"

          if (!Views.Exists(ViewNum)) {
          //raise error
          On Error GoTo 0: Err.Raise WINAGI_ERR + 504, "ResMan", "Invalid View number passed to RemoveView (view does not exist)"
          return;
          }

          //remove it from game
          Views.Remove ViewNum

          switch (Settings.ResListType
          case 1
          With MDIMain.tvwResources
            //remove it from resource list
            .Nodes.Remove MDIMain.tvwResources.Nodes["v" + CStr(ViewNum)).Index

          //last node marker is no longer accurate; reset
            MDIMain.LastNodeName = "";

           if (.SelectedItem.Parent = null) {
              //it's the game node
              MDIMain.SelectResource rtGame, -1
            } else if ( .SelectedItem.Parent.Parent = null) {
              //it's a resource header
              MDIMain.SelectResource .SelectedItem.Index - 2, -1
            } else {
              //it's a resource
              MDIMain.SelectResource .SelectedItem.Parent.Index - 2, CLng(.SelectedItem.Tag)
            }
          End With

          case 2
          //only need to remove if views are listed
          if (MDIMain.cmbResType.SelectedIndex = 4) {
            //remove it
            MDIMain.lstResources.ListItems.Remove MDIMain.lstResources.ListItems("v" + CStr(ViewNum)).Index
            //use click event to update
            MDIMain.lstResources_Click
            MDIMain.lstResources.SelectedItem.Selected = true
          }
          }

          //if an editor is open
          For i = 1 To ViewEditors.Count
          if (ViewEditors(i).InGame && ViewEditors(i).ViewNumber = ViewNum) {
            //set number to -1 to force close
            ViewEditors(i).ViewNumber = -1
            //close it
            Unload ViewEditors(i)
            Exit For
          }
          Next i

          //disposition any existing resource file
          if (File.Exists(strViewFile)) {
          KillCopyFile strViewFile, Settings.RenameDelRes
          }

          //update the logic tooltip lookup table
          With IDefLookup(ViewNum + 256)
          .Name = ""
          .Value = ""
          .Type = 11 //set to a value > highest type
          End With
          //then let open logic editors know
          if (LogicEditors.Count > 0) {
          For i = 1 To LogicEditors.Count
            LogicEditors(i).ListDirty = true
          Next i
          }
          return;

          ErrHandler:
          //if error is invalid resid,
          if (Err.Number = WINAGI_ERR + 617) {
          //pass it along
          On Error GoTo 0: Err.Raise Err.Number, Err.Source, Err.Description
          return;
          }

          Resume Next
          }


          }
          public static byte RenumberResource(byte OldResNum, AGIResType ResType)

          //renumbers a resource; return Value is the new number

          Node tmpNode;
           TreeRelationshipConstants tvwRel;
          string strResType, strCaption;
          byte NewResNum;
            int i;

          On Error GoTo ErrHandler

          //default to old number, in case user cancels
          NewResNum = OldResNum

          //show renumber resoure form
          frmGetResourceNum.ResType = ResType
          frmGetResourceNum.OldResNum = OldResNum
          frmGetResourceNum.WindowFunction = grRenumber
          //setup before loading so ghosts don't show up
          frmGetResourceNum.FormSetup
          frmGetResourceNum.ShowDialog(MDIMain);
          //if user makes a choice AND number is different
          if (!frmGetResourceNum.Canceled && frmGetResourceNum.NewResNum != OldResNum) {
          //get new number
          NewResNum = frmGetResourceNum.NewResNum

          //change number for this resource
          switch (ResType
          case rtLogic
            //renumber it
            Logics.Renumber OldResNum, NewResNum
            strResType = "l"
            strCaption = ResourceName(Logics(NewResNum), true)

          case rtPicture
            //renumber it
            Pictures.Renumber OldResNum, NewResNum
            strResType = "p"
            strCaption = ResourceName(Pictures(NewResNum), true)

          case rtSound
            //renumber it
            Sounds.Renumber OldResNum, NewResNum
            strResType = "s"
            strCaption = ResourceName(Sounds(NewResNum), true)

          case rtView
            //renumber it
            Views.Renumber OldResNum, NewResNum
            strResType = "v"
            strCaption = ResourceName(Views(NewResNum), true)
          }

          //update resource list
          switch (Settings.ResListType
          case 1
            With MDIMain.tvwResources
              //remove the old node
              .Nodes.Remove strResType + CStr(OldResNum)

              //start with first node of this Type
              tmpNode = .Nodes[ResType + 2).Child

              //if there are no nodes
             if (tmpNode = null) {
                //add first child
                tmpNode = .Nodes[ResType + 2)
                tvwRel = tvwChild
              //if this node belongs at end of list
              } else if ( NewResNum > tmpNode.LastSibling.Tag) {
                //add to end
                tmpNode = tmpNode.LastSibling
                tvwRel = tvwNext
              } else {
                //get position which should immediately follow this resource
                //step through until a node is found that is past this new number
                Do Until (byte)tmpNode.Tag) > NewResNum
                  tmpNode = tmpNode.Next
                Loop
                tvwRel = tvwPrevious
              }

              //put the resource in it's new location
              .Nodes.Add(tmpNode.Index, tvwRel, strResType + CStr(NewResNum), strCaption).Selected = true
              .SelectedItem.Tag = NewResNum
              .SelectedItem.EnsureVisible
              //if node is a logic
             if (ResType = rtLogic) {
                //highlight in red if not compiled
               if (!Logics(NewResNum).Compiled) {
                  .SelectedItem.ForeColor = Colors.Red
                }
              }

          //last node marker is no longer accurate; reset
              MDIMain.LastNodeName = "";

             if (.SelectedItem.Parent = null) {
                //it's the game node
                MDIMain.SelectResource rtGame, -1
              } else if ( .SelectedItem.Parent.Parent = null) {
                //it's a resource header
                MDIMain.SelectResource .SelectedItem.Index - 2, -1
              } else {
                //it's a resource
                MDIMain.SelectResource .SelectedItem.Parent.Index - 2, CLng(.SelectedItem.Tag)
              }
            End With
          case 2
            //only update if the resource type is being listed
           if (MDIMain.cmbResType.SelectedIndex - 1 = ResType) {
              With MDIMain.lstResources.ListItems
                //remove it from current location
                .Remove strResType + CStr(OldResNum)

                //if nothing left
               if (.Count = 0) {
                  //add it as first item
                  tmpListItem = .Add(, strResType + CStr(NewResNum), ResourceName(Pictures(NewResNum), true))

                } else {
                  //get index position to add a new one
                  For i = 1 To .Count
                   if (NewResNum < (byte).Item(i).Tag)) {
                      Exit For
                    }
                  Next i
                  //add it at this index point
                  tmpListItem = .Add(i, strResType + CStr(NewResNum), ResourceName(Pictures(NewResNum), true))
                }

                //add  tag
                tmpListItem.Tag = NewResNum

                //if node is a logic
               if (ResType = rtLogic) {
                  //highlight in red if not compiled
                 if (!Logics(NewResNum).Compiled) {
                    tmpListItem.ForeColor = Colors.Red
                  }
                }

                //select it
                tmpListItem.Selected = true

                //use click event to update
                MDIMain.lstResources_Click
              End With
            }
          }
          }

          //unload the get resource number form
          Unload frmGetResourceNum

          //return the new number
          RenumberResource = NewResNum
          Exit Function

          ErrHandler:
          switch (Err.Number
          case WINAGI_ERR + 564
          MessageBox.Show("You attempted to change a resource to a number that is already in use. Try renumbering again, with an unused resource number.", MessageBoxIcon.Information, "Renumber Resource Error"

          default:
          ErrMsgBox("Error while renumbering:", "Resource list may not display correct numbers. Close/reopen game to refresh.", "Renumber Resource Error"
          }

          //unload the get resource number form
          Unload frmGetResourceNum

          Err.Clear
          }

          public static void ReplaceAll(string FindText, string ReplaceText, FindDirection FindDir, bool MatchWord, bool MatchCase, FindLocation LogicLoc, AGIResType SearchType = AGIResType.rtNone)
          // replace all doesn't use or need direction
          int i, LogNum;

          On Error GoTo ErrHandler

          //if search Type is defines, words or objects, the editor does progress status and msgs

          //if replacing and text is the same
          if ((StrComp(FindText, ReplaceText, IIf(MatchCase, vbBinaryCompare, StringComparison.OrdinalIgnoreCase)) = 0)) {
          //restore mouse, reneable main form, and exit
          //Debug.Assert MDIMain.UseWaitCursor = false;
          MDIMain.UseWaitCursor = false;
          //Debug.Assert MDIMain.Enabled = true
          MDIMain.Enabled = true
          return;
          }

          //find text can//t be blank
          if ((LenB(FindText) = 0)) {
          //restore mouse, reneable main form, and exit
          MDIMain.UseWaitCursor = false;
          MDIMain.Enabled = true
          return;
          }

          //not all searches use the progress bar
          switch (SearchType
          case rtNone, rtLogic, rtPicture, rtSound, rtView
          //show wait cursor
          MDIMain.UseWaitCursor = true;
          //disable main form
          MDIMain.Enabled = false
          //refresh (normal DoEvents here; otherwise SafeDoEvents will re-enable the form)
          DoEvents
          }

          switch (LogicLoc
          case flCurrent
          ReplaceAllWords FindText, ReplaceText, MatchWord, MatchCase, true, null, SearchForm

          case flOpen
          //replace in all open logic and text editors

          //show progress form
          Load ProgressWin
          With ProgressWin
            .Text = "Replace All"
            .lblProgress.Text = "Searching..."
            .pgbStatus.Max = LogicEditors.Count
            .Show vbModeless, MDIMain
            .Refresh
          End With

          //replace in all open editors
          For i = 1 To LogicEditors.Count
            //update progress bar
            ProgressWin.pgbStatus.Value = i - 1
           if (LogicEditors(i).Name = "frmLogicEdit") {
              //if a logic, show the logic ID
              ProgressWin.lblProgress.Text = "Searching " + LogicEditors(i).LogicEdit.ID + "..."
            } else {
              //if a text file, show the filename
              ProgressWin.lblProgress.Text = "Searching " + Path.GetFileName(LogicEditors(i).FileName) + "..."
            }
            //replace
            ReplaceAllWords FindText, ReplaceText, MatchWord, MatchCase, true, null, LogicEditors(i)
          Next i

          //close the progress form
          Unload ProgressWin

          case flAll
          //replace in all ingame logics; does NOT include any open editors
          //which are text files or !InGame

          //if replacing globals, don't use the progress form
          //it's already being used to track the globals being searched
          if (SearchType != rtGlobals) {
            //show progress form
            Load ProgressWin
            With ProgressWin
              switch (SearchType
              case rtNone
                .Text = "Replace All"
                .lblProgress.Text = "Searching..."
              default:
                .Text = "Updating Resource ID"
                .lblProgress.Text = "Searching..."
              }

              .pgbStatus.Max = Logics.Count
              .pgbStatus.Value = 0
              .Show vbModeless, MDIMain
              .Refresh
            End With
          }

          //replace in all open editors
          For i = 1 To LogicEditors.Count
            //only logic editors
           if (LogicEditors(i).Name = "frmLogicEdit") {
              //and only if in game
             if (LogicEditors(i).InGame) {
                //if not updating global defines,
               if (SearchType != rtGlobals) {
                  //update progress bar
                  With ProgressWin
                    .pgbStatus.Value = ProgressWin.pgbStatus.Value + 1
                    //show the logic ID
                    .lblProgress.Text = "Searching " + LogicEditors(i).LogicEdit.ID + "..."
                    .Refresh
                  End With
                }
                //replace
                ReplaceAllWords FindText, ReplaceText, MatchWord, MatchCase, true, null, LogicEditors(i), SearchType
              }
            }
          Next i

          //now do all closed logics

          //get first available logic number
          LogNum = NextClosedLogic(-1)

          Do Until LogNum = -1
            switch (SearchType
            case rtNone, rtLogic, rtPicture, rtSound, rtView
              //update progress bar
              ProgressWin.pgbStatus.Value = ProgressWin.pgbStatus.Value + 1
              ProgressWin.lblProgress.Text = "Searching " + Logics(LogNum).ID + "..."
              //refresh window
              ProgressWin.Refresh
            }

            //if not loaded
           if (!Logics(LogNum).Loaded) {
              Logics(LogNum).Load
            }
            ReplaceAllWords FindText, ReplaceText, MatchWord, MatchCase, false, Logics(LogNum), null, SearchType

            //if dirty
           if (Logics(LogNum).SourceDirty) {
              //save logic source
              Logics(LogNum).SaveSource
              UpdateSelection rtLogic, LogNum, umPreview
            }

            //unload it
            Logics(LogNum).Unload
            //get next logic
            LogNum = NextClosedLogic(LogNum)
            //increment counter
            i = i + 1
          Loop

          switch (SearchType
          case rtNone, rtLogic, rtPicture, rtSound, rtView
            //close the progress form
            Unload ProgressWin
          }
          }

          switch (SearchType
          case rtNone, rtLogic, rtPicture, rtSound, rtView
          if (SearchType = rtNone) {
            //if found,
           if (ReplaceCount > 0) {
              MessageBox.Show("The specified region has been searched. " + CStr(ReplaceCount) + " replacements were made.", MessageBoxIcon.Information, "Replace All"
            } else {
              MessageBox.Show("Search text not found.", MessageBoxIcon.Information, "Replace All"
            }
          }

          //enable form and reset cursor
          MDIMain.Enabled = true
          MDIMain.UseWaitCursor = false;

          // have to set focus to main form in order get the child forms
          // to properly switch focus (the searching logic should always
          // get the focus after a replace all; not the FindingForm)
          MDIMain.Focus()
          }

          //reset search flags
          FindingForm.ResetSearch

          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

          static void ReplaceAllWords(string FindText, string ReplaceText, bool MatchWord, bool MatchCase, bool InWindow, AGILogic SearchLogic = null, Form SearchWin = null, AGIResType SearchType = AGIResType.rtNone)


          //replaces text in either a logic, or a textbox
          //calling function MUST ensure a valid reference to a richtextbox
          //or a logic is passed

          int FoundPos;
          StringComparison vbcComp;
          int i;
          int lngStart, lngEnd;
          int lngOldFC;
            bool blnOldBold, blnOldItal;

          On Error GoTo ErrHandler

          //if NOT a regular search
          if (SearchType != rtNone) {
          //ignore text editors
          if (InWindow) {
           if (SearchWin.Name = "frmTextEdit") {
              return;
            }
          }
          }

          //set comparison method for string search,
          vbcComp = CLng(MatchCase) + 1 // CLng(true) + 1 = 0 = vbBinaryCompare; Clng(false) + 1 = 1 = StringComparison.OrdinalIgnoreCase

          //start at beginning
          FoundPos = 1

          Do
          //if searching whole word
          if (MatchWord) {
            //if in a window
           if (InWindow) {
              FoundPos = FindWholeWord(FoundPos, SearchWin.rtfLogic.Text, FindText, MatchCase, false, SearchType)
            } else {
              FoundPos = FindWholeWord(FoundPos, SearchLogic.SourceText, FindText, MatchCase, false, SearchType)
            }
          } else {
            //if in a window
           if (InWindow) {
              FoundPos = InStr(FoundPos, SearchWin.rtfLogic.Text, FindText, vbcComp)
            } else {
              FoundPos = InStr(FoundPos, SearchLogic.SourceText, FindText, vbcComp)
            }
          }

          if (FoundPos > 0) {
            //if replacing in a window
           if (InWindow) {
              //instr is 1 based; richedit boxes are 0 based
              //so need to subtract one from foundpos

              lngStart = FoundPos - 1
              lngEnd = FoundPos - 1 + Len(FindText)
              With SearchWin.rtfLogic.Range(lngStart, lngEnd)
                //if selected text does not match,
               if (LCase$(.Text) != LCase$(FindText)) {
                  //error
                  MessageBox.Show("An error was encountered in the replace feature of the rich text" + Environment.NewLine + "editor. !all instances of search text have been replaced.", MessageBoxIcon.Critical + MessageBoxButtons.OK, "Replace All Error"
                  return;
                }

          //////          // save font properties of current selection
          //////          lngOldFC = .Font.ForeColor
          //////          blnOldBold = .Font.Bold
          //////          blnOldItal = .Font.Italic

                // replace the text
                .Text = ReplaceText
                //refresh highlighting for the start line (other lines are
                // automatically highlighted during the replace action)
                i = SendMessage(SearchWin.rtfLogic.hWnd, EM_LINEFROMCHAR, lngStart, 0)
                SearchWin.rtfLogic.RefreshHighlight i, i
                //this still slows things down, but seems acceptable, so leave
                // it for now
          //////          // restore font properties to the selection
          //////          .Font.ForeColor = lngOldFC
          //////          .Font.Bold = blnOldBold
          //////          .Font.Italic = blnOldItal
              End With
            } else {
              //replace word in logic source
              SearchLogic.SourceText = Left$(SearchLogic.SourceText, FoundPos - 1) + ReplaceText + Right$(SearchLogic.SourceText, Len(SearchLogic.SourceText) - Len(FindText) - FoundPos + 1)
            }

            //reset foundpos
            FoundPos = FoundPos + Len(ReplaceText)
            //increment Count
            ReplaceCount = ReplaceCount + 1
          } else {
            break;
          }
          Loop

          return;

          ErrHandler:
          //Debug.Assert false
          Resume Next
          }

      */
        }

        public static void UpdateSelection(AGIResType ResType, int ResNum, UpdateModeType UpDateMode) {
            //updates the resource list, property box, and preview window for a given resource
            if (ResNum < 0 | ResNum > 255) {
                return;
            }

            switch (Settings.ResListType) {
            case 0:
                //no tree - do nothing
                break;
            case 1:
                //treeview list
                //if updating tree OR updating a logic
                if ((UpDateMode & UpdateModeType.umResList) == UpdateModeType.umResList || ResType == AGIResType.Logic) {
                    //update the node for this resource
                    switch (ResType) {
                    case AGIResType.Logic:
                        HdrNode[0].Nodes["l" + ResNum.ToString()].Text = ResourceName(EditGame.Logics[ResNum], true);
                        //also set compiled status
                        if (!EditGame.Logics[ResNum].Compiled) {
                            HdrNode[0].Nodes["l" + ResNum.ToString()].ForeColor = Color.Red;
                        }
                        else {
                            HdrNode[0].Nodes["l" + ResNum.ToString()].ForeColor = Color.Black;
                        }
                        break;
                    case AGIResType.Picture:
                        HdrNode[1].Nodes["p" + ResNum.ToString()].Text = ResourceName(EditGame.Pictures[ResNum], true);
                        break;
                    case AGIResType.Sound:
                        HdrNode[2].Nodes["s" + ResNum.ToString()].Text = ResourceName(EditGame.Sounds[ResNum], true);
                        break;
                    case AGIResType.View:
                        HdrNode[3].Nodes["v" + ResNum.ToString()].Text = ResourceName(EditGame.Views[ResNum], true);
                        break;
                    }
                }
                break;
            case 2:
                //combo/list boxes
                //only update if current type is listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)ResType) {
                    //if updating tree OR updating a logic (because color of
                    //logic text might need updating)
                    if ((UpDateMode & UpdateModeType.umResList) == UpdateModeType.umResList || ResType == AGIResType.Logic) {
                        //update the node for this resource
                        switch (ResType) {
                        case AGIResType.Logic:
                            ListViewItem tmpItem = MDIMain.lstResources.Items["l" + ResNum.ToString()];
                            tmpItem.Text = ResourceName(EditGame.Logics[ResNum], true);
                            //also set compiled status
                            tmpItem.ForeColor = EditGame.Logics[ResNum].Compiled ? Color.Black : Color.Red;
                            break;
                        case AGIResType.Picture:
                            MDIMain.lstResources.Items["p" + ResNum.ToString()].Text = ResourceName(EditGame.Pictures[ResNum], true);
                            break;
                        case AGIResType.Sound:
                            MDIMain.lstResources.Items["s" + ResNum.ToString()].Text = ResourceName(EditGame.Sounds[ResNum], true);
                            break;
                        case AGIResType.View:
                            MDIMain.lstResources.Items["v" + ResNum.ToString()].Text = ResourceName(EditGame.Views[ResNum], true);
                            break;
                        }
                        // //expand column width if necessary
                        //if (1.2 * MDIMain.picResources.TextWidth(tmpItem.Text) > MDIMain.lstResources.ColumnHeaders(1).Width) {
                        //   MDIMain.lstResources.ColumnHeaders(1).Width = 1.2 * MDIMain.picResources.TextWidth(tmpItem.Text)
                    }
                }
                break;
            }
            // if the selected item matches the update item
            if (SelResType == ResType && SelResNum == ResNum) {
                //if updating properties OR updating tree AND tree is visible
                if (((UpDateMode & UpdateModeType.umProperty) == UpdateModeType.umProperty || (UpDateMode & UpdateModeType.umResList) == UpdateModeType.umProperty) && Settings.ResListType != 0) {
                }

                //if updating preview
                if ((UpDateMode & UpdateModeType.umPreview) == UpdateModeType.umPreview && Settings.ShowPreview) {
                    //redraw the preview
                    PreviewWin.LoadPreview(ResType, ResNum);
                }
                else if (Settings.ShowPreview) {
                    PreviewWin.UpdateCaption(ResType, (byte)ResNum);
                }
            }
        }
        public static void UpdateResFile(AGIResType ResType, byte ResNum, string OldFileName) {
            //updates ingame id for resource files and the resource tree
            DialogResult rtn;

            switch (ResType) {
            case AGIResType.Logic:
                //if a file with this name already exists
                if (File.Exists(EditGame.ResDir + EditGame.Logics[ResNum].ID + EditGame.SourceExt)) {
                    //import existing, or overwrite it?
                    rtn = MessageBox.Show("There is already a source file with the name '" + EditGame.Logics[ResNum].ID +
                          EditGame.SourceExt + "' in your source file directory." + Environment.NewLine + Environment.NewLine +
                          "Do you want to import that file? Choose 'NO' to replace that file with the current logic source.",
                          "Import Existing Source File?", MessageBoxButtons.YesNo);
                }
                else {
                    //no existing file, so keep current source
                    rtn = DialogResult.No;
                }

                if (rtn == DialogResult.Yes) {
                    // keep old file with updated new name; basically import it by reloading, if currently loaded
                    if (EditGame.Logics[ResNum].Loaded) {
                        EditGame.Logics[ResNum].Unload();
                        EditGame.Logics[ResNum].Load();
                    }

                    //now update preview window, if previewing
                    if (Settings.ShowPreview) {
                        if (SelResType == AGIResType.Logic && SelResNum == ResNum) {
                            // TODO: reload? or just update properties??
                            PreviewWin.LoadPreview(AGIResType.Logic, ResNum);
                        }
                    }
                }
                else {
                    //if there is a file with the new ResID, rename it first
                    try {
                        //delete existing .OLD file (if there is one)
                        if (File.Exists(EditGame.ResDir + EditGame.Logics[ResNum].ID + EditGame.SourceExt + ".OLD")) {
                            {
                                File.Delete(EditGame.ResDir + EditGame.Logics[ResNum].ID + EditGame.SourceExt + ".OLD");
                            }
                            //rename old file with new ResID as .OLD
                            File.Move(EditGame.ResDir + EditGame.Logics[ResNum].ID + EditGame.SourceExt, EditGame.ResDir + EditGame.Logics[ResNum].ID + EditGame.SourceExt + ".OLD");
                            //then, if there is a file with the previous ID
                            //save it with the new ID
                            if (File.Exists(OldFileName)) {
                                File.Move(OldFileName, EditGame.ResDir + EditGame.Logics[ResNum].ID + EditGame.SourceExt);
                            }
                            else {
                                EditGame.Logics[ResNum].SaveSource();
                            }
                        }
                    }
                    catch (Exception) {
                        //
                    }
                }
                //if layouteditor is open
                if (LEInUse) {
                    //redraw to ensure correct ID is displayed
                    LayoutEditor.DrawLayout();
                }
                break;
            case AGIResType.Picture:
                //if autoexporting
                if (Settings.AutoExport) {
                    try {
                        //if a file with this name already exists
                        if (File.Exists(EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp")) {
                            // rename it, (remove existing old file first)
                            if (File.Exists(EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp" + ".OLD")) {
                                File.Delete(EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp" + ".OLD");
                            }
                            File.Move(EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp", EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp" + ".OLD");
                        }

                        if (File.Exists(OldFileName)) {
                            //rename resource file, if it exists
                            File.Move(OldFileName, EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp");
                        }
                        else {
                            //save it
                            EditGame.Pictures[ResNum].Export(EditGame.ResDir + EditGame.Pictures[ResNum].ID + ".agp");
                        }
                    }
                    catch (Exception) {
                        //
                    }
                }
                break;
            case AGIResType.Sound:
                //if autoexporting
                if (Settings.AutoExport) {
                    try {
                        //if a file with this name already exists
                        if (File.Exists(EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags")) {
                            // rename it, (remove existing old file first)
                            if (File.Exists(EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags" + ".OLD")) {
                                File.Delete(EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags" + ".OLD");
                            }
                            File.Move(EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags", EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags" + ".OLD");
                        }

                        if (File.Exists(OldFileName)) {
                            //rename resource file, if it exists
                            File.Move(OldFileName, EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags");
                        }
                        else {
                            EditGame.Sounds[ResNum].Export(EditGame.ResDir + EditGame.Sounds[ResNum].ID + ".ags");
                        }
                    }
                    catch (Exception) {
                        //
                    }
                }
                break;
            case AGIResType.View:
                //if autoexporting
                if (Settings.AutoExport) {
                    try {
                        //if a file with this name already exists
                        if (File.Exists(EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv")) {
                            // rename it, (remove existing old file first)
                            if (File.Exists(EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv" + ".OLD")) {
                                File.Delete(EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv" + ".OLD");
                            }
                            File.Move(EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv", EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv" + ".OLD");
                        }

                        //check to see if old file exists
                        if (File.Exists(OldFileName)) {
                            //rename resource file, if it exists
                            File.Move(OldFileName, EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv");
                        }
                        else {
                            EditGame.Views[ResNum].Export(EditGame.ResDir + EditGame.Views[ResNum].ID + ".agv");
                        }
                    }
                    catch (Exception) {
                        //
                    }
                }
                break;
            }//switch

            //update property window and resource list
            UpdateSelection(ResType, ResNum, UpdateModeType.umProperty | UpdateModeType.umResList);
        }
        public static void UpdateExitInfo(EUReason Reason, int LogicNumber, Logic ThisLogic, int NewNum = 0) {
            /*
          //   frmMDIMain|SelectedItemRenumber:  UpdateExitInfo euRenumberRoom, OldResNum, null, NewResNum
          //  frmMDIMain|lstProperty_LostFocus:  UpdateExitInfo Reason, SelResNum, Logics(SelResNum) //showroom or removeroom
          //frmMDIMain|picProperties_MouseDown:  UpdateExitInfo Reason, SelResNum, Logics(SelResNum) //showroom or removeroom
          //                ResMan|AddNewLogic:
          //                   ResMan|NewLogic:  UpdateExitInfo euShowRoom, .NewResNum, Logics(.NewResNum)
          //                ResMan|RemoveLogic:  UpdateExitInfo euRemoveRoom, LogicNum, null
          //        frmLogicEdit|MenuClickSave:  UpdateExitInfo euUpdateRoom, LogicEdit.Number, LogicEdit
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
            //rtLogic = 0
            //rtPicture = 1
            //rtSound = 2
            //rtView = 3
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
              case rtLogic:
              case rtText:
                tmpPanel = MainStatusBar.Items.Add(1, "Status", , sbrText);
                tmpPanel.AutoSize = sbrSpring;
                tmpPanel.Alignment = sbrLeft;
                //Debug.Assert !MDIMain.ActiveForm = null;
                tmpPanel.Text = MDIMain.ActiveForm.Tag;

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
              case rtPicture:
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
              case rtSound:
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

              case rtView;
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
              case rtLogic:
              case rtPicture:
              case rtSound:
              case rtView:

                break;
              }
            } */
        }
        public static void AddNewLogic(int NewLogicNumber, Logic NewLogic, bool blnTemplate, bool Importing) {
            string strLogic;
            int lngPos = 0;

            //add to logic collection in game
            EditGame.Logics.Add((byte)NewLogicNumber, NewLogic);
            //if not importing, we need to add boilerplate text
            if (!Importing) {
                //if using template,
                if (blnTemplate) {
                    //add template text to logic source
                    strLogic = LogTemplateText(EditGame.Logics[NewLogicNumber].ID, EditGame.Logics[NewLogicNumber].Description);
                }
                else {
                    //add default text
                    strLogic = "[ " + Keys.Enter + "[ " + EditGame.Logics[NewLogicNumber].ID + NEWLINE +
                               "[ " + Keys.Enter + NEWLINE + "return();" + NEWLINE + NEWLINE +
                               "[*****" + NEWLINE + "[ messages         [  declared messages go here" +
                               Keys.Enter + "[*****";
                }
                //for new resources, need to set the source text
                EditGame.Logics[NewLogicNumber].SourceText = strLogic;
            }
            //always save source to new name
            EditGame.Logics[NewLogicNumber].SaveSource();

            //if NOT importing AND default (not using template), compile the text
            if (!Importing && !blnTemplate) {
                EditGame.Logics[NewLogicNumber].Compile();
            }
            //set isroom status based on template
            if (NewLogicNumber != 0) {
                EditGame.Logics[NewLogicNumber].IsRoom = blnTemplate;
            }
            //if using layout editor AND isroom
            if (EditGame.UseLE && EditGame.Logics[NewLogicNumber].IsRoom) {
                //update layout editor and layout data file to show this room is in the game
                UpdateExitInfo(EUReason.euAddRoom, NewLogicNumber, EditGame.Logics[NewLogicNumber]);
            }
            //add to resource list
            switch (Settings.ResListType) {
            case 1:
                TreeNode tmpNode = HdrNode[0];
                //find place to insert this logic
                for (lngPos = 0; lngPos < HdrNode[0].Nodes.Count; lngPos++) {
                    if ((int)tmpNode.Nodes[lngPos].Tag > NewLogicNumber) {
                        break;
                    }
                }
                //add to tree
                tmpNode = HdrNode[0].Nodes.Insert(lngPos, "l" + NewLogicNumber, ResourceName(EditGame.Logics[NewLogicNumber], true));
                tmpNode.Tag = NewLogicNumber;
                //load source to set compiled status
                tmpNode.ForeColor = EditGame.Logics[NewLogicNumber].Compiled ? Color.Black : Color.Red;
                break;
            case 2:
                //only update if logics are being listed
                if (MDIMain.cmbResType.SelectedIndex == 1) {
                    ListViewItem tmpListItem;
                    //find a place to insert this logic in the box list
                    for (lngPos = 0; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((int)MDIMain.lstResources.Items[lngPos].Tag > NewLogicNumber) {
                            break;
                        }
                    }
                    //i is index position we are looking for
                    tmpListItem = MDIMain.lstResources.Items.Insert(lngPos, "l" + NewLogicNumber, ResourceName(EditGame.Logics[NewLogicNumber], true), 0);
                    tmpListItem.Tag = NewLogicNumber.ToString();
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
        public static void AddNewPicture(int NewPictureNumber, Picture NewPicture) {
            int lngPos = 0;
            //add picture to game collection
            EditGame.Pictures.Add((byte)NewPictureNumber, NewPicture);

            switch (Settings.ResListType) {
            case 1:
                //find place to insert this picture
                for (lngPos = 0; lngPos < HdrNode[1].Nodes.Count; lngPos++) {
                    if ((int)HdrNode[1].Nodes[lngPos].Tag > NewPictureNumber) {
                        break;
                    }
                }
                //add it to tree
                HdrNode[1].Nodes.Insert(lngPos, "p" + NewPictureNumber, ResourceName(EditGame.Pictures[NewPictureNumber], true)).Tag = NewPictureNumber;
                break;
            case 2:
                //only update if pictures are being listed
                if (MDIMain.cmbResType.SelectedIndex == 2) {
                    //find a place to add it
                    for (lngPos = 0; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((int)MDIMain.lstResources.Items[lngPos].Tag > NewPictureNumber) {
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
        public static void AddNewSound(int NewSoundNumber, Sound NewSound) {
            int lngPos = 0;
            //add sound to game collection
            EditGame.Sounds.Add((byte)NewSoundNumber, NewSound);

            switch (Settings.ResListType) {
            case 1:
                //find place to insert this sound
                for (lngPos = 0; lngPos < HdrNode[2].Nodes.Count; lngPos++) {
                    if ((int)HdrNode[2].Nodes[lngPos].Tag > NewSoundNumber) {
                        break;
                    }
                }
                //add it to tree
                HdrNode[2].Nodes.Insert(lngPos, "s" + NewSoundNumber, ResourceName(EditGame.Sounds[NewSoundNumber], true)).Tag = NewSoundNumber;
                break;
            case 2:
                //only update if sounds are being updated
                if (MDIMain.cmbResType.SelectedIndex == 3) {
                    //find a place to add it
                    for (lngPos = 0; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((int)MDIMain.lstResources.Items[lngPos].Tag > NewSoundNumber) {
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
        public static void AddNewView(int NewViewNumber, Engine.View NewView) {
            int lngPos = 0;
            //add view to game collection
            EditGame.Views.Add((byte)NewViewNumber, NewView);

            switch (Settings.ResListType) {
            case 1:
                //find place to insert this view
                for (lngPos = 0; lngPos < HdrNode[3].Nodes.Count; lngPos++) {
                    if ((int)HdrNode[3].Nodes[lngPos].Tag > NewViewNumber) {
                        break;
                    }
                }
                //add it to tree
                HdrNode[3].Nodes.Insert(lngPos, "v" + NewViewNumber, ResourceName(EditGame.Views[NewViewNumber], true)).Tag = NewViewNumber;
                break;
            case 2:
                //only update if views are being displayed
                if (MDIMain.cmbResType.SelectedIndex == 4) {
                    //find a place to add it
                    for (lngPos = 1; lngPos < MDIMain.lstResources.Items.Count; lngPos++) {
                        if ((int)MDIMain.lstResources.Items[lngPos].Tag > NewViewNumber) {
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
            frmGetResourceNum GetResNum = new() {
                ResType = AGIResType.Logic
            };
            if (blnImporting) {
                GetResNum.WindowFunction = EGetRes.grImport;
            }
            else {
                GetResNum.WindowFunction = EGetRes.grAddNew;
            }
            //setup before loading so ghosts don't show up
            GetResNum.FormSetup();
            // suggest ID based on filename
            if (ImportLogicFile.Length > 0) {
                GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportLogicFile).Replace(" ", "");
            }
            // restore cursor while getting resnum
            MDIMain.UseWaitCursor = false;
            GetResNum.ShowDialog(MDIMain);
            // show wait cursor while resource is added
            MDIMain.UseWaitCursor = true;

            // if canceled, release the temporary logic, restore mousepointer and exit
            if (GetResNum.Canceled) {
                tmpLogic = null;
                // restore mousepointer and exit
                GetResNum.Dispose();
                MDIMain.UseWaitCursor = false;
                return;
            }
            // if user wants logic added to current game
            else if (!GetResNum.DontImport) {
                // add ID and description to tmpLogic
                tmpLogic.ID = GetResNum.txtID.Text;
                tmpLogic.Description = GetResNum.txtDescription.Text;

                //add Logic
                AddNewLogic(GetResNum.NewResNum, tmpLogic, (GetResNum.chkRoom.Checked), blnImporting);
                // reset tmplogic to point to the new game logic
                tmpLogic = EditGame.Logics[GetResNum.NewResNum];

                // if using layout editor AND a room,
                if (EditGame.UseLE && (GetResNum.chkRoom.Checked)) {
                    // update editor and data file to show this room is now in the game
                    UpdateExitInfo(EUReason.euShowRoom, GetResNum.NewResNum, EditGame.Logics[GetResNum.NewResNum]);
                }
                //if including picture
                if (GetResNum.chkIncludePic.Checked) {
                    // if replacing an existing pic
                    if (EditGame.Pictures.Contains(GetResNum.NewResNum)) {
                        RemovePicture(GetResNum.NewResNum);
                    }
                    AddNewPicture(GetResNum.NewResNum, null);
                    // help user out if they chose a naming scheme
                    if (Left(GetResNum.txtID.Text, 3).Equals("rm.", StringComparison.OrdinalIgnoreCase) && GetResNum.txtID.Text.Length >= 4) {
                        // change ID (if able)
                        if (ValidateID("pic." + Right(GetResNum.txtID.Text, GetResNum.txtID.Text.Length - 3), "") == 0) {
                            // save old resfile name
                            strFile = EditGame.ResDir + EditGame.Pictures[GetResNum.NewResNum].ID + ".agp";
                            // change this picture//s ID
                            EditGame.Pictures[GetResNum.NewResNum].ID = "pic." + Right(GetResNum.txtID.Text, GetResNum.txtID.Text.Length - 3);
                            // update the resfile, tree and properties
                            UpdateResFile(AGIResType.Picture, GetResNum.NewResNum, strFile);
                            // update lookup table
                            IDefLookup[768 + GetResNum.NewResNum].Name = "pic." + Right(GetResNum.txtID.Text, GetResNum.txtID.Text.Length - 3);
                        }
                    }
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
                if (frmNew.EditLogic(tmpLogic)) {
                    frmNew.Show();
                    // add form to collection
                    LogicEditors.Add(frmNew);
                }
                else {
                    frmNew.Close();
                }
            }
            // save openres value
            Settings.OpenNew = blnOpen;
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
            bool blnOpen = false;

            MDIMain.UseWaitCursor = true;
            //create temporary picture
            tmpPic = new Picture();
            if (ImportPictureFile.Length != 0) {
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
            frmGetResourceNum GetResNum = new() {
                ResType = AGIResType.Picture
            };
            if (ImportPictureFile.Length == 0) {
                GetResNum.WindowFunction = EGetRes.grAddNew;
            }
            else {
                GetResNum.WindowFunction = EGetRes.grImport;
            }
            // setup before loading so ghosts don't show up
            GetResNum.FormSetup();
            // suggest ID based on filename
            if (ImportPictureFile.Length > 0) {
                GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportPictureFile).Replace(" ", "");
            }
            // restore cursor while getting resnum
            MDIMain.UseWaitCursor = false;
            GetResNum.ShowDialog(MDIMain);
            // show wait cursor while resource is added
            MDIMain.UseWaitCursor = true;

            // if canceled, release the temporary picture, restore cursor and exit method
            if (GetResNum.Canceled) {
                // restore mousepointer and exit
                GetResNum.Close();
                MDIMain.UseWaitCursor = false;
                return;
            }
            // if user wants picture added to current game
            else if (!GetResNum.DontImport) {
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
                if (frmNew.EditPicture(tmpPic)) {
                    frmNew.Show();
                    PictureEditors.Add(frmNew);
                }
                else {
                    // error
                    frmNew.Close();
                }
            }
            // save openres value
            Settings.OpenNew = blnOpen;
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
            bool blnOpen = false;

            // show wait cursor
            MDIMain.UseWaitCursor = true;
            // create temporary sound
            tmpSound = new Sound();
            // set default instrument settings;
            // if a sound is being imported, these may be overridden...
            tmpSound.Track(0).Instrument = Settings.DefInst0;
            tmpSound.Track(1).Instrument = Settings.DefInst1;
            tmpSound.Track(2).Instrument = Settings.DefInst2;
            tmpSound.Track(0).Muted = Settings.DefMute0;
            tmpSound.Track(1).Muted = Settings.DefMute1;
            tmpSound.Track(2).Muted = Settings.DefMute2;
            tmpSound.Track(3).Muted = Settings.DefMute3;

            // if an import filename was passed
            if (ImportSoundFile.Length != 0) {
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
            frmGetResourceNum GetResNum = new() {
                ResType = AGIResType.Sound
            };
            if (ImportSoundFile.Length == 0) {
                GetResNum.WindowFunction = EGetRes.grAddNew;
            }
            else {
                GetResNum.WindowFunction = EGetRes.grImport;
            }
            // setup before loading so ghosts don't show up
            GetResNum.FormSetup();
            // suggest ID based on filename
            if (ImportSoundFile.Length > 0) {
                GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportSoundFile).Replace(" ", "");
            }

            // restore cursor while getting resnum
            MDIMain.UseWaitCursor = false;
            GetResNum.ShowDialog(MDIMain);
            // show wait cursor again while finishing creating the new sound
            MDIMain.UseWaitCursor = true;

            // if canceled, release the temporary sound, restore cursor and exit method
            if (GetResNum.Canceled) {
                // restore mousepointer, unload form and exit
                GetResNum.Close();
                MDIMain.UseWaitCursor = false;
                return;
            }
            // if user wants sound added to current game
            else if (!GetResNum.DontImport) {
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
                if (frmNew.EditSound(tmpSound)) {
                    frmNew.Show();
                    SoundEditors.Add(frmNew);
                }
                else {
                    // error
                    frmNew.Close();
                }
            }
            // save openres value
            Settings.OpenNew = blnOpen;
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
            bool blnOpen = false;

            // show wait cursor
            MDIMain.UseWaitCursor = true;
            // create temporary view
            tmpView = new Engine.View();
            // if an import filename was passed
            if (ImportViewFile.Length != 0) {
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
                tmpView[0][0].Height = Settings.DefCelH;
                tmpView[0][0].Width = Settings.DefCelW;
            }
            // get picture number, id , description
            frmGetResourceNum GetResNum = new() {
                ResType = AGIResType.View
            };
            if (ImportViewFile.Length == 0) {
                GetResNum.WindowFunction = EGetRes.grAddNew;
            }
            else {
                GetResNum.WindowFunction = EGetRes.grImport;
            }
            // setup before loading so ghosts don't show up
            GetResNum.FormSetup();
            // suggest ID based on filename
            if (ImportViewFile.Length > 0) {
                GetResNum.txtID.Text = Path.GetFileNameWithoutExtension(ImportViewFile).Replace(" ", "");
            }
            // restore cursor while getting resnum
            MDIMain.UseWaitCursor = false;
            GetResNum.ShowDialog(MDIMain);
            // show wait cursor while resource is added
            MDIMain.UseWaitCursor = true;
            // if canceled, release the temporary view, restore cursor and exit method
            if (GetResNum.Canceled) {
                tmpView = null;
                // restore mousepointer and exit
                GetResNum.Close();
                MDIMain.UseWaitCursor = false;
                return;
            }
            // if user wants view added to current game
            else if (!GetResNum.DontImport) {
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
                if (frmNew.EditView(tmpView)) {
                    frmNew.Show();
                    ViewEditors.Add(frmNew);
                }
                else {
                    // error
                    frmNew.Close();
                }
            }
            // save openres value
            Settings.OpenNew = blnOpen;
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
                // error
                WinAGIException wex = new("Invalid Picture number passed to RemovePicture (picture does not exist)") {
                    HResult = WINAGI_ERR + 999,
                };
                throw wex;
            }

            string strPicFile = EditGame.ResDir + EditGame.Pictures[PicNum].ID + ".agp";
            //remove it from game
            EditGame.Pictures.Remove(PicNum);

            switch (Settings.ResListType) {
            case 1:
                //remove it from resource list
                MDIMain.tvwResources.Nodes.RemoveAt(MDIMain.tvwResources.Nodes["p" + PicNum.ToString()].Index);
                //update selection to whatever is now the selected node
                MDIMain.LastNodeName = "";
                if (MDIMain.tvwResources.SelectedNode == RootNode) {
                    //it's the game node
                    MDIMain.SelectResource(Game, -1);
                }
                else if (MDIMain.tvwResources.SelectedNode.Parent == RootNode) {
                    //it's a resource header
                    MDIMain.SelectResource((AGIResType)(MDIMain.tvwResources.SelectedNode.Index), -1);
                }
                else {
                    //it's a resource
                    MDIMain.SelectResource((AGIResType)MDIMain.tvwResources.SelectedNode.Parent.Index, (int)MDIMain.tvwResources.SelectedNode.Tag);
                }
                break;
            case 2:
                //only need to remove if pictures are listed
                if (MDIMain.cmbResType.SelectedIndex == 2) {
                    //remove it
                    MDIMain.lstResources.Items.RemoveAt(MDIMain.lstResources.Items["p" + PicNum.ToString()].Index);
                    //use click event to update?
                    //MDIMain.lstResources_Click(null, null);
                    //MDIMain.lstResources.SelectedItems[0].Selected = true;
                }
                break;
            }

            //if an editor is open
            for (int i = 0; i < PictureEditors.Count; i++) {
                if (PictureEditors[i].InGame && PictureEditors[i].PicNumber == PicNum) {
                    //set number to -1 to force close
                    PictureEditors[i].PicNumber = -1;
                    //close it
                    PictureEditors[i].Close();
                    break;
                }
            }

            //disposition any existing resource file
            if (File.Exists(strPicFile)) {
                KillCopyFile(strPicFile, Settings.RenameDelRes);
            }

            //update the logic tooltip lookup table

            IDefLookup[PicNum + 768].Name = "";
            IDefLookup[PicNum + 768].Value = "";
            IDefLookup[PicNum + 768].Type = (ArgTypeEnum)11; //set to a value > highest type

            //then let open logic editors know
            if (LogicEditors.Count > 0) {
                for (int i = 1; i < LogicEditors.Count; i++) {
                    LogicEditors[i].ListDirty = true;
                }
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
                strLogic = strLogic.Replace("~", MultStr(" ", Settings.LogicTabWidth));
                //id:
                strLogic = strLogic.Replace("%id", NewID);
                //description
                strLogic = strLogic.Replace("%desc", NewDescription);

                //horizon is a PicTest setting, which should always be retrieved everytime
                //it is used to make sure it's current
                strLogic = strLogic.Replace("%h", GameSettings.GetSetting(sPICTEST, "Horizon", DEFAULT_PICTEST_HORIZON).ToString());

                //if using reserved names, insert them
                if (LogicDecoder.UseReservedNames) {
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
        public static void CheckShortcuts(KeyEventArgs e) {
            //*// shouldn't need this anymore; key assignments included in menu definitions

            //check for game-wide shortcut keys
            if (e.Shift && e.Control && !e.Alt) { //CtrlMask + ShiftMask
                switch (e.KeyCode) {
                case Keys.A: //add/remove resource
                    if (MDIMain.mnuRAddRemove.Enabled) {
                        AddOrRemoveRes();
                    }
                    break;
                case Keys.B: //compile to
                    if (MDIMain.mnuGCompileTo.Enabled) {
                        //compile game to directory of user//s choice
                        CompileAGIGame();
                    }
                    break;
                case Keys.D: //compile dirty logics
                    if (MDIMain.mnuGCompileDirty.Enabled) {
                        CompileDirtyLogics();
                    }
                    break;
                case Keys.N: //new game from blank
                    if (MDIMain.mnuGNewBlank.Enabled) {
                        //create new blank game
                        NewAGIGame(false);
                    }
                    break;
                case Keys.R: //rebuild VOL
                    if (MDIMain.mnuGRebuild.Enabled) {
                        //rebuild volfiles only
                        CompileAGIGame(EditGame.GameDir, true);
                    }
                    break;
                case Keys.T: //insert a snippet
                    if (MDIMain.ActiveMdiChild is not null) {
                        if (MDIMain.ActiveMdiChild.Name == "frmLogicEdit" || MDIMain.ActiveMdiChild.Name == "frmTextEdit") {
                            //open snippet manager
                            SnipMode = 1;
                            SnippetForm.ShowDialog(MDIMain);
                            //force focus back to the editor
                            ((frmLogicEdit)(MDIMain.ActiveMdiChild)).rtfLogic.Focus();
                        }
                    }
                    break;
                }
            }
            if (!e.Shift && e.Control && e.Alt) { // Ctrl + Alt
                                                  //import resources
                                                  //but only if import menu is enabled
                if (MDIMain.mnuRImport.Enabled) {
                    switch (e.KeyCode) {
                    case Keys.D1:
                        MDIMain.mnuRILogic_Click(null, null);
                        break;
                    case Keys.D2:
                        MDIMain.mnuRIPicture_Click(null, null);
                        break;
                    case Keys.D3:
                        MDIMain.mnuRISound_Click(null, null);
                        break;
                    case Keys.D4:
                        MDIMain.mnuRIView_Click(null, null);
                        break;
                    case Keys.D5:
                        MDIMain.mnuRIObjects_Click(null, null);
                        break;
                    case Keys.D6:
                        MDIMain.mnuRIWords_Click(null, null);
                        break;
                    }
                }
            }
            if (!e.Shift && e.Control & !e.Alt) { // Ctrl
                switch (e.KeyCode) {
                case Keys.D1:
                    MDIMain.mnuRNLogic_Click(null, null);
                    break;
                case Keys.D2:
                    MDIMain.mnuRNPicture_Click(null, null);
                    break;
                case Keys.D3:
                    MDIMain.mnuRNSound_Click(null, null);
                    break;
                case Keys.D4:
                    MDIMain.mnuRNView_Click(null, null);
                    break;
                case Keys.D5:
                    MDIMain.mnuRNObjects_Click(null, null);
                    break;
                case Keys.D6:
                    MDIMain.mnuRNWords_Click(null, null);
                    break;
                case Keys.D7:
                    MDIMain.mnuRNText_Click(null, null);
                    break;
                }
            }
            if (!e.Shift && !e.Control && e.Alt) { // Alt
                switch (e.KeyCode) {
                case Keys.D1:
                    MDIMain.mnuROLogic_Click(null, null);
                    break;
                case Keys.D2:
                    MDIMain.mnuROPicture_Click(null, null);
                    break;
                case Keys.D3:
                    MDIMain.mnuROSound_Click(null, null);
                    break;
                case Keys.D4:
                    MDIMain.mnuROView_Click(null, null);
                    break;
                case Keys.D5:
                    MDIMain.mnuROObjects_Click(null, null);
                    break;
                case Keys.D6:
                    MDIMain.mnuROWords_Click(null, null);
                    break;
                case Keys.D7:
                    MDIMain.mnuROText_Click(null, null);
                    break;

                case Keys.X: //close game
                    if (MDIMain.mnuGClose.Enabled) {
                        MDIMain.mnuGClose_Click(null, null);
                    }
                    break;
                case Keys.N: //renumber
                    if (MDIMain.mnuRRenumber.Enabled) {
                        MDIMain.mnuRRenumber_Click(null, null);
                    }
                    break;
                case Keys.F1:  //logic command help
                               //select commands start page
                    _ = HtmlHelp(HelpParent, WinAGIHelp, HH_HELP_CONTEXT, 1001);
                    break;
                }
            }
            //no mask:
            // currently nothing to process
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
                //Debug.Print LogicEditors(lngNextLogWin).Name + " has focus (next editor in collection, begin search)"
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
            //Debug.Print MDIMain.ActiveForm.Name
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
                        strName = Left(ResFile, ResFile.Length - 4);
                        strExt = Right(ResFile, 4);
                        lngNextNum = 1;
                        //assign proposed rename
                        strOldName = strName + "_OLD" + strExt;
                        //Validate it
                        while (File.Exists(strOldName)) {
                            lngNextNum++;
                            strOldName = strName + "_OLD_" + lngNextNum.ToString() + strExt;
                        }
                        File.Move(ResFile, strOldName);
                        return;
                    }
                    // if not keeping old, just delete current file
                    File.Delete(ResFile);
                }
            }
            catch (Exception) {
                //ignore
            }
        }

        public static void ErrMsgBox(int ErrNum, string ErrMsg1, string ErrMsg2, string ErrCaption) {
            // show errmsg baed on agi resource error level
            string strErrMsg;

            // TODO: new string resources for load errors
            strErrMsg = ErrMsg1 + Environment.NewLine + Environment.NewLine + ErrNum + ": " + LoadResString(ErrNum);
            if (ErrMsg2.Length > 0) {
                strErrMsg = strErrMsg + Environment.NewLine + Environment.NewLine + ErrMsg2;
            }
            MessageBox.Show(MDIMain, strErrMsg, ErrCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ErrMsgBox(Exception e, string ErrMsg1, string ErrMsg2, string ErrCaption) {
            //displays a messagebox showing ErrMsg and includes error passed as AGIErrObj
            //Debug.Assert Err.Number != 0

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
            for (int i = 0; i < 16; i++) {  //TODO: is this right? use original value as default-default?
                                            // this is all wrong - need set of colors for the App to use; those colors get set to 
                                            //   -  the default colors (which should be set by reading winagi.config file) when no game loaded
                                            //   - to the game colors when a game is loaded
                DefaultColors[i] = GameSettings.GetSetting(sDEFCOLORS, "DefEGAColor" + i, DefaultColors[i]);
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
            if (Settings.ShowResNum && IsInGame) {
                switch (Settings.ResFormat.NameCase) {
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
                return retval + Settings.ResFormat.Separator + ThisResource.Number.ToString(Settings.ResFormat.NumFormat);
            }
            else {
                if (Settings.IncludeResNum && IsInGame && !NoNumber) {
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
        static void tmpCommon() {
            /*
      Option Explicit

      internal bool IsTokenChar(int intChar, bool Quotes = false)
        {
        // returns true if this character is a token character
        // false if it isn//t;
        // if Quotes is true, then dbl-quote is considered a token character
        // if Quotes is false, then dbl-quote is NOT considered a token character

        On Error GoTo ErrHandler

        switch (intChar
        { case 32
          //space is ALWAYS not a token
          IsTokenChar = false

        case 34
          //dbl quote depends on optional Quotes argument
          IsTokenChar = Quotes

        case 1 To 33, 38 To 45, 47, 58 To 63, 91 To 94, 96, 123 To 126
          // !&//()*+,-/:;<=>?[\]^`{|}~ and all control characters
          //non-token
          IsTokenChar = false
        default:    //35, 36, 37, 46, 48 - 57, 64, 65 - 90, 95, 97 - 122
          //a-z, A-Z, 0-9   @#$%_. and 127+
          //token
          IsTokenChar = true
        } // switch
      return

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      internal string StripComments(string strLine, ref string strComment, bool NoTrim = false)
      {
        //strips off any comments on the line
        //if NoTrim is false, the string is also
        //stripped of any blank space

        //if there is a comment, it is passed back in the strComment argument

        int lngPos
            int intROLIgnore
        bool blnDblSlash
        bool blnInQuotes, blnSlash

        On Error GoTo ErrHandler

        //reset rol ignore
        intROLIgnore = 0

        //reset comment start + char ptr, and inquotes
        lngPos = 0
        blnInQuotes = false

        //assume no comment
        strComment = ""

        //if this line is not empty,
        if (strLine.Length != 0) {
          while ( lngPos < strLine.Length) // Until lngPos >= strLine.Length
          {
            //get next character from string
            lngPos++;
            //if NOT inside a quotation,
            if (!blnInQuotes) {
              //check for comment characters at this position
              if ((Mid(strLine, lngPos, 2) == "//")) {
                intROLIgnore = lngPos + 1
                blnDblSlash = true
                break;
              } else if ( (Mid(strLine, lngPos, 1) == "[")) {
                intROLIgnore = lngPos
                break;
              }
              // slash codes never occur outside quotes
              blnSlash = false
              //if this character is a quote mark, it starts a string
              blnInQuotes = (AscW(Mid(strLine, lngPos)) = 34)
            } else {
              //if last character was a slash, ignore this character
              //because it's part of a slash code
              if (blnSlash) {
                //always reset  the slash
                blnSlash = false
              } else {
                //check for slash or quote mark
                switch (AscW(Mid(strLine, lngPos))
                { case 34 //quote mark
                  //a quote marks end of string
                  blnInQuotes = false
                case 92 //slash
                  blnSlash = true
                } // switch
              }
            }
          } //while
          //if any part of line should be ignored,
          if (intROLIgnore > 0) {
            //save the comment
            strComment = Trim(Right(strLine, strLine.Length - intROLIgnore))
            //strip off comment
            if (blnDblSlash) {
              strLine = Left(strLine, intROLIgnore - 2)
            } else {
              strLine = Left(strLine, intROLIgnore - 1)
            }
          }
        }

        if (!NoTrim) {
          //return the line, trimmed
          StripComments = strLine.Trim()
        } else {
          //return the string with just the comment removed
          StripComments = strLine
        }
      return

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      internal bool IsValidQuote(string strText, int QPos)
      {
        //returns true if the quote mark at position QPos is a valid quote mark
        //by checking for slash codes in front of it

        //if the character at QPos is not a quote mark, then function returns false

        int i

        On Error GoTo ErrHandler

        //assume not inquote at start
        IsValidQuote = false

        if (Asc(Mid(strText, QPos)) != 34) {
          return
        }

        //check for preceding slash marks
        //toggle the flag until no more
        //slash marks found
        do
        {
          IsValidQuote = !IsValidQuote
          QPos = QPos - 1
          if (QPos <= 0) {
            break;
          }
        } while (strText[QPos] == '\'); // Until Asc(Mid(strText, QPos)) != 92

      return

      ErrHandler:
        //Debug.Assert false
        Resume Next
      */
        }

        static void tmpStuff2() {
            /*

          internal bool IsInvObject(int lngStartPos, string strText)
          {
            On Error GoTo ErrHandler

            //check for has cmd
            //check for obj.in.room cmd
            //check for drop cmd
            //check for get cmd
            //check for put cmd




            //*****not implemented yet; always return true
            IsInvObject = true

          return

          ErrHandler:

          }


          internal bool IsVocabWord(int lngStartPos, string strText)
          {
            On Error GoTo ErrHandler

            //check for //said// cmd
            //check for  //word.to.string//

            //get line by backing up until CR, //;// or beginning of string reached

            //then move forward, finding the command


            //*****not implemented yet; always return true
            IsVocabWord = true



          return

          ErrHandler:

          }
          internal byte AGIVal(int IntIn)
          {  
              switch (IntIn
              { case Is < 0
                do
                {
                  IntIn = IntIn + 256
                } while (IntIn < 0); // Until IntIn >= 0
              case Is > 255
                do
                {
                  IntIn = IntIn - 256
                } while (IntIn > 255); // Until IntIn <= 255
              } // switch
              return (byte)IntIn;
          }



          internal int vCint(double InputNum)
          {  
            vCint = Int(InputNum) + CInt(InputNum - Int(InputNum) + 1) - 1
          }

          internal int FindWholeWord(int lngStartPos, string strText, string strFind, _
                                        bool MatchCase = false, _
                                        bool RevSearch = false, _
                                        AGIResType SearchType = rtNone)
          {                              
            //will return the character position of first occurence of strFind in strText,
            //only if it is a whole word
            //whole word is defined as a word where the character in front of the word is a
            //separator (or word is at beginning of string) AND character after word is a
            //separator (or word is at end of string)
            //
            //separators are any character EXCEPT:
            // #, $, %, ., 0-9, @, A-Z, _, a-z
            //(codes 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122)

            int lngPos
            bool blnFrontOK
            StringComparison StringCompare

            On Error GoTo ErrHandler

            //if no search string passed
            if (strFind.Length == 0) {
              //return zero
              FindWholeWord = 0
              return
            }

            //set compare method
            if (MatchCase) {
              StringCompare = vbBinaryCompare
            } else {
              StringCompare = vbTextCompare
            }

            //set position to start
            lngPos = lngStartPos
            do
            {
              //if doing a reverse search
              if (RevSearch) {
                lngPos = InStrRev(strText, strFind, lngPos, StringCompare)
              } else {
                //if lngPos=-1, it means start at end of string
                //get position of string in strtext
                lngPos = InStr(lngPos, strText, strFind, StringCompare)
              }

              //easy check is to see if strFind is even in strText
              if (lngPos == 0) {
                FindWholeWord = 0
                return
              }

              //check character in front
              if (lngPos > 1) {
                switch (AscW(Mid(strText, lngPos - 1))
                // #, $, %, 0-9, A-Z, _, a-z
                { case 35 To 37, 48 To 57, 64 To 90, 95, 97 To 122
                  //word is NOT whole word
                  blnFrontOK = false
                default:
                  blnFrontOK = true
                } // switch
              } else {
                blnFrontOK = true
              }

              //if front is ok,
              if (blnFrontOK) {
                //check character in back
                if (lngPos + strFind.Length <= strText.Length) {
                  switch (AscW(Mid(strText, lngPos + strFind.Length))
                  { case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122
                    //word is NOT whole word
                    //let loop try again at next position in string
                  default:
                    //if validation required
                    switch (SearchType
                    { case rtWords  //check against vocabword
                      if (IsVocabWord(lngPos, strText)) {
                        //word IS a whole word
                        FindWholeWord = lngPos
                        return
                      }
                    case rtObjects  //validate an inventory object
                      if (IsInvObject(lngPos, strText)) {
                        //word IS a whole word
                        FindWholeWord = lngPos
                        return
                      }
                    default: //no validation
                      //word IS a whole word
                      FindWholeWord = lngPos
                      return
                    } // switch
                  } // switch
                } else {
                  //word IS a whole word
                  FindWholeWord = lngPos
                  return
                }
              }

              //entire string not checked yet
              if (RevSearch) {
                lngPos = lngPos - 1
              } else {
                lngPos++;
              }
            } while (lngPos != 0); // Until lngPos = 0
            //if no position found,
            FindWholeWord = 0
          return

          ErrHandler:
            //Debug.Assert false
            Resume Next
          }


          internal int vClng(double InputNum)
          {  
            vClng = Int(InputNum) + CLng(InputNum - Int(InputNum) + 1) - 1
          }
                */
        }

    }
}
