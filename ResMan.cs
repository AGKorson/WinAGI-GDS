using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.WinAGI;
using static WinAGI.AGICommands;

namespace WinAGI_GDS
{
  static class ResMan
  {
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
    public const bool DEFAULT_USETXT = false;
    public static string DEFAULT_EFONTNAME; //no constants; value will be set in InitializeResMan
    public const int DEFAULT_EFONTSIZE = 14;
    public static string DEFAULT_PFONTNAME; //no constants; value will be set in InitializeResMan
    public const int DEFAULT_PFONTSIZE = 12;
    public const int DEFAULT_HNRMCOLOR = 0x0; //black
    public const int DEFAULT_HKEYCOLOR = 0x7F0000;
    public const int DEFAULT_HIDTCOLOR = 0x40;
    public const int DEFAULT_HSTRCOLOR = 0x5050;
    public const int DEFAULT_HCMTCOLOR = 0x8000;
    public const int DEFAULT_HBKGCOLOR = 0xFFFFFF; //white
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
    public const LogicErrorLevel DEFAULT_ERRORLEVEL = LogicErrorLevel.leMedium;
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
    public const AGIColors DEFAULT_DEFVCOLOR1 = AGIColors.agBlack;
    public const AGIColors DEFAULT_DEFVCOLOR2 = AGIColors.agWhite;
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
    public const int DEFAULT_LEROOM_EDGE = 0x55AA;
    public const int DEFAULT_LEROOM_FILL = 0x55FFFF;
    public const int DEFAULT_LETRANSPT_EDGE = 0x626200;
    public const int DEFAULT_LETRANSPT_FILL = 0xFFFF91;
    public const int DEFAULT_LEERR_EDGE = 0x62;
    public const int DEFAULT_LEERR_FILL = 0x9191FF;
    public const int DEFAULT_LECMT_EDGE = 0x6200;
    public const int DEFAULT_LECMT_FILL = 0x91FF91;
    public const int DEFAULT_LEEXIT_EDGE = 0xAA0000;
    public const int DEFAULT_LEEXIT_OTHERS = 0xFF55FF;
    public const int DEFAULT_LEZOOM = 6;
    //other default settings
    public const int DEFAULT_NAMECASE = (int)StringComparison.OrdinalIgnoreCase;
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
    public const int PropRowHeight = 17; //in pixels
    public const int LtGray = 0xC0C0C0;
    public const int DKGray = 0xD8E9EC;
    public const int SelBlue = 0xC56A31;
    public const double LG_SCROLL = 0.9; //0.8
    public const double SM_SCROLL = 0.225; //0.2
    //instead of a constant value, set this based on chosen font
    // (in frmMDIMain_Load)
    //public const in PropSplitLoc = 65
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
    //ENUMERATIONS
    //***************************************************
    #region
    public enum UndoNameID
    {
      UID_UNKNOWN = 0,
      UID_TYPING = 1,
      UID_DELETE = 2,
      UID_DRAGDROP = 3,
      UID_CUT = 4,
      UID_PASTE = 5,
    }
    public enum ReplaceMode
    {
      rpYes,
      rpYesToAll,
      rpNo,
      rpNoToAll,
    }
    public enum EGetRes
    {
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
    }
    public enum GameSettingFunction
    {
      gsEdit,
      gsNew,
    }
    public enum UpdateModeType
    {
      umResList = 1,
      umProperty = 2,
      umPreview = 4,
    }
    public enum ViewEditMode
    {
      vmBitmap,
      vmView,
      vmLoop,
      vmCel,
    }
    public enum ELogicFormMode
    {
      fmLogic,
      fmText,
    }
    public enum FindDirection
    {
      fdAll,
      fdDown,
      fdUp,
    }
    public enum FindLocation
    {
      flCurrent,
      flOpen,
      flAll,
    }
    public enum FindFormFunction
    {
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
    public enum FindFormAction
    {
      faFind,
      faReplace,
      faReplaceAll,
      faCancel,
    }
    public enum TPicToolTypeEnum
    {
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
    public enum TPicDrawOpEnum
    {
      doNone = 0,          //indicates no drawing op is in progress; mouse operations generally don't do anything
      doLine = 1,          //lines being drawn; mouse ops set start/end points
      doFill = 2,          //fill or plot commands being drawn; mouse ops set starting point of fill operations
      doShape = 3,         //shape being drawn; mouse ops set bounds of the shape
      doSelectArea = 4,    //to select an area of the graphic
      doMoveCmds = 5,      //to move a set of commands
      doMovePt = 6,        //to move a single coordinate point
    }
    public enum EPicMode
    {
      pmEdit,
      pmTest,
    }
    public enum EPicCur
    {
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
    public enum EPicCursorMode
    {
      pcmWinAGI,
      pcmXMode,
    }
    public enum EButtonFace
    {
      bfNone,
      bfDown,
      bfOver,
      bfDialog,
    }
    public enum ENoteTone
    {
      ntNone,
      ntNatural,
      ntSharp,
      ntFlat,
    }
    public enum ELSelection
    {
      lsNone,
      lsRoom,
      lsExit,
      lsTransPt,
      lsComment,
      lsErrPt,
      lsMultiple,
    }
    public enum ELLeg
    {
      llNoTrans,   //  0 means no trans pt
      llFirst,     //  1 means first leg of trans pt
      llSecond,    //  2 means second leg of trans pt
    }
    public enum ELTwoWay
    {
      ltwSingle,    //  0 means a single direction exit
      ltwOneWay,    //  1 means a two way exit but only one way selected
      ltwBothWays,  //  2 means a two way exit and both are considered //selected//
    }
    public enum ELayoutTool
    {
      ltNone,
      ltSelect,
      ltEdge1,
      ltEdge2,
      ltOther,
      ltRoom,
      ltComment,
    }
    public enum EEReason
    {
      erNone,      //no exit reason specified (reason not yet assigned)
      erHorizon,   //exit is //if ego=horizon// Type
      erRight,     //exit is //if ego=right// Type
      erBottom,    //exit is //if ego=bottom// Type
      erLeft,      //exit is //if ego=left// Type
      erOther,     //exit can//t be easily determined to be a simple edge exit
    }
    public enum EEStatus
    {
      esOK,        //exit is drawn in layout editor, and exists in source code correctly
      esNew,       //exit that is drawn in layout editor, but hasn//t been saved to source code yet
      esChanged,   //exit that has been changed in the layout editor, but not updated in source code
      esDeleted,   //exit that has been deleted in layout editor, but not updated in source code
      esHidden,    //exit is valid, but to a logic currently marked as not IsRoom
    }
    public enum EUReason
    {
      euAddRoom,         //new room added in layout editor
      euShowRoom,        //existing room toggled to show in layout editor
      euRemoveRoom,      //room removed by hiding (IsRoom to false), or actual removal from game
      euRenumberRoom,    //room//s logic number is changed
      euUpdateRoom,      //existing room updated in logic editor
    }
    public enum EArgListType
    {
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
    public enum EImgFormat
    {
      effBMP = 0,
      effGIF = 1,
      effPNG = 2,
      effJPG = 3,
    }
    #endregion
    //***************************************************
    //TYPE DEFINITIONS
    //***************************************************
    #region
    public struct tDefaultScale
    {
      public int Edit;
      public int Preview;
    }
    public struct tDisplayNote
    {
      public int Pos;
      public ENoteTone Tone;
    }
    public struct TPicTest
    {
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
    public struct LEObjColor
    {
      public int Edge;
      public int Fill;
    }
    public struct TLEColors
    {
      public LEObjColor Room;
      public LEObjColor ErrPt;
      public LEObjColor TransPt;
      public LEObjColor Cmt;
      public int Edge;
      public int Other;
    }
    public struct GifOptions
    {
      public int Zoom;
      public bool Transparency;
      public int Delay;
      public bool Cycle;
      public int VAlign;
      public int HAlign;
    }
    public struct agiSettings
    {
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
      // logics
      public bool HighlightLogic;
      public bool HighlightText;
      public int LogicTabWidth;
      public bool MaximizeLogics;
      public bool AutoQuickInfo;
      public bool ShowDefTips;
      public bool UseTxt;
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
      public int[] HColor; // (6) OLE_COLOR
      public bool[] HItalic; // (5)
      public bool[] HBold; // (5)
      public int OpenOnErr;  // //0 = ask; 1 = no; 2 = yes
      public int SaveOnCompile;  // //0 = ask; 1 = no; 2 = yes
      public int CompileOnRun;  // //0 = ask; 1 = no; 2 = yes
      public int LogicUndo;  //
      public int WarnMsgs;  //  //0 = ask; 1 = keep all; 2 = keep only used
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
    public struct LCoord
    {
      public double X;
      public double Y;
    }
    public struct PT
    {
      public byte X;
      public byte Y;
    }
    public struct tResource
    {
      public StringComparison NameCase;
      public string Separator;
      public string NumFormat;
    }
    #endregion
    //***************************************************
    //GLOBAL VARIABLES
    //***************************************************
    #region
    public static frmMDIMain MDIMain;
    public static string ProgramDir;
    public static string DefaultResDir; //this is the location that the file dialog box uses as the initial directory
    public static string CurGameFile;
    public static AGIResType CurResType;
    public static agiSettings Settings;
    public static List<string> SettingsList;
    public static frmPreview PreviewWin;
    //static public StatusBar MainStatusBar
    //static public CommonDialog MainDialog
    //static public SaveDialog MainSaveDlg
    //static public frmCompStatus CompStatusWin
    //static public PictureBox NotePictures
    public static int SelResNum;
    public static AGIResType SelResType;
    //static public OSVERSIONINFO WinVer
    public static bool SettingError;
    public static bool Compiling;
    public static string WinAGIHelp;
    public static int PrevWinBColor; //background color for preview window when showing views
    public static int ScreenTWIPSX;
    public static int ScreenTWIPSY;
    //navigation queue
    //public static int[] ResQueue;
    //public static int ResQPtr;
    public static Stack<int> ResQueue = new Stack<int>();
    public static bool DontQueue;
    //editor variables
    public static List<frmLogicEdit> LogicEditors;
    public static int LogCount;
    public static List<frmPicEdit> PictureEditors;
    public static int PicCount;
    public static List<frmSoundEdit> SoundEditors;
    public static int SoundCount;
    //static public List<frmViewEdit> ViewEditors;
    public static int ViewCount;
    static public frmLayout LayoutEditor;
    public static bool LEInUse;
    //static public frmMenuEdit MenuEditor;
    public static bool MEInUse;
    //static public frmObjectEdit ObjectEditor;
    public static bool OEInUse;
    public static int ObjCount;
    //public static frmWordsEdit WordEditor;
    public static bool WEInUse;
    public static int WrdCount;
    //public frmGlobals GlobalsEditor;
    public static bool GEInUse;
    //lookup lists for logic editor
    //tooltips and define lists
    public static TDefine[] RDefLookup = new TDefine[95]; //(94)
    public static TDefine[] GDefLookup;
    public static TDefine[] IDefLookup = new TDefine[1024]; // (1023)
    //  //for now we will not do lookups
    //  // on words and invObjects
    //  // if performance is good enough
    //  // I might consider adding them
    //  public static TDefine[] // ODefLookup()
    //  public static TDefine[] // WDefLookup()
    public static TDefine[] CodeSnippets;
    public static int SnipMode; //0=create, 1=manage
    public static GifOptions VGOptions;
    public static int lngMainTopBorder;
    public static int lngMainLeftBorder;
    public static int DefUpdateVal;
    //mru variables
    public static string[] strMRU = new string[5]; // (4)
                                            //clipboard variables
    public static AGINotes SoundClipboard;
    public static int SoundCBMode;
    public static AGILoop ClipViewLoop;
    public static AGICel ClipViewCel;
    //public static PictureUndo PicClipBoardObj;
    public static ViewEditMode ViewCBMode;
    //public static PictureBox ViewClipboard;
    //public static WordsUndo WordsClipboard;
    public static bool DroppingWord;
    public static bool DroppingObj;
    public static bool DroppingGlobal;
    public static TDefine[] GlobalsClipboard;
    //default colors
    public static int[] DefEGAColor = new int[16]; // (15)
    //local copy of ega colors, to speed color matching
    public static int[] lngEGACol = new int[16]; // (15)
    //find/replace variables
    //public static frmFind FindForm;
    //public static Form SearchForm;
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
    public static bool SearchStartDlg; //true if search started by clicking //find// or //find next//
                                // on FindForm
    internal static int SearchLogCount;
    internal static int SearchLogVal;
    //property window variables
    public static int DropDownDC;
    public static int DropOverDC;
    public static int DropDlgDC;
    //others
    public static int HelpParent;
    public static string TempFileDir;
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
    #endregion
    public static void AddToQueue(AGIResType ResType, int ResNum)
    {
      //adds this resource to the navigation queue
      // Restype is either
      //   0-3 for regular resources, with number = 0-255 or
      //    4  for non resource nodes, with number = restype
      //       (game, objects, words, or res header)
      //
      // if currently displaying something by navigating the queue
      // don't add

      int lngRes;

     if (DontQueue)
      {
        return;
      }

      //if resum is invalid, or if restype is invalid
      if (ResType < 0 || (int)ResType > 4 || ResNum < 0 || ResNum > 255)
      {
        //error
        //Debug.Assert false
        return;
      }

      //build combined number/resource
      lngRes = (int)ResType * 256 + ResNum;

      //don't add if the current resource matches
      if (ResQueue.Count > 0)
      {
        if (ResQueue.Peek() == lngRes)
        {
          return;
        }
      }
      //add the res info
      ResQueue.Push(lngRes);
    }

    public static void ResetQueue()
    {
      ResQueue = new Stack<int>();
      MDIMain.cmdBack.Enabled = false;
      MDIMain.cmdForward.Enabled = false;
    }
    static void tmpwork()
    {
      /*


      */
    }
    static void tmpResMan()
    {
      /*
public Function ValidateID(NewID As String, OldID As String) As Long
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
  
  Dim i As Long
  Dim tmpDefines() As TDefine
  
  On Error GoTo ErrHandler
  
  //ignore if it//s old id, or a different case of old id
  If StrComp(NewID, OldID, vbTextCompare) = 0 Then
    //it is OK
    ValidateID = 0
    Exit Function
  End If
  
  //if no name,
  If LenB(NewID) = 0 Then
    ValidateID = 1
    Exit Function
  End If
  
  //name cant be numeric
  If IsNumeric(NewID) Then
    ValidateID = 2
    Exit Function
  End If
  
  //check against regular commands
  For i = 0 To Commands.Count
    If StrComp(NewID, Commands(i).Name, vbTextCompare) = 0 Then
      ValidateID = 3
      Exit Function
    End If
  Next i
  
  //check against test commands
  For i = 0 To TestCommands.Count
    If StrComp(NewID, TestCommands(i).Name, vbTextCompare) = 0 Then
      ValidateID = 4
      Exit Function
    End If
  Next i
  
  //check against keywords
  Select Case LCase$(NewID)
  Case "if", "else", "goto"
    ValidateID = 5
    Exit Function
  End Select
      
  //check against variable/flag/controller/string/message names
  Select Case Asc(LCase$(NewID))
  //     v    f    m    o    i    s    w    c
  Case 118, 102, 109, 111, 105, 115, 119, 99
    If IsNumeric(Right$(NewID, Len(NewID) - 1)) Then
      ValidateID = 6
      Exit Function
    End If
  End Select
  
//////  //check against globals
//////  For i = 1 To global count
//////    If (StrComp(NewID, globalname(i), vbTextCompare) = 0) Then
//////      ValidateID = 7
//////      Exit Function
//////    End If
//////  Next i
  
//////  If LogicSourceSettings.UseReservedNames Then
//////    //check against reserved names
//////    tmpDefines = LogicSourceSettings.ReservedDefines(atVar)
//////    For i = 0 To UBound(tmpDefines)
//////      If NewID = tmpDefines(i).Name Then
//////        ValidateID = 8
//////        Exit Function
//////      End If
//////    Next i
//////    tmpDefines = LogicSourceSettings.ReservedDefines(atFlag)
//////    For i = 0 To UBound(tmpDefines)
//////      If NewID = tmpDefines(i).Name Then
//////        ValidateID = 9
//////        Exit Function
//////      End If
//////    Next i
//////    tmpDefines = LogicSourceSettings.ReservedDefines(atNum)
//////    For i = 0 To UBound(tmpDefines)
//////      If NewID = tmpDefines(i).Name Then
//////        ValidateID = 10
//////        Exit Function
//////      End If
//////    Next i
//////    tmpDefines = LogicSourceSettings.ReservedDefines(atSObj)
//////    For i = 0 To UBound(tmpDefines)
//////      If NewID = tmpDefines(i).Name Then
//////        ValidateID = 11
//////        Exit Function
//////      End If
//////    Next i
//////    tmpDefines = LogicSourceSettings.ReservedDefines(atDefStr)
//////    For i = 0 To UBound(tmpDefines)
//////      If NewID = tmpDefines(i).Name Then
//////        ValidateID = 12
//////        Exit Function
//////      End If
//////    Next i
//////    tmpDefines = LogicSourceSettings.ReservedDefines(atStr)
//////    For i = 0 To UBound(tmpDefines)
//////      If NewID = tmpDefines(i).Name Then
//////        ValidateID = 13
//////        Exit Function
//////      End If
//////    Next i
//////  End If
  
  //check name against improper character list
  For i = 1 To Len(NewID)
    Select Case Asc(Mid$(NewID, i, 1))
//                                                                            1         1         1
//        3       4    4    5         6         7         8         9         0         1         2
//        234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
//NOT OK  x!"   &//()*+,- /          :;<=>?                           [\]^ `                          {|}~x
//    OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    
    Case 32 To 34, 38 To 45, 47, 58 To 63, 91 To 94, 96, Is >= 123
      ValidateID = 14
      Exit Function
    End Select
  Next i
  
  //check against existing IDs
  For i = 0 To 1023
    If IDefLookup(i).Type < 11 Then
      If StrComp(NewID, IDefLookup(i).Name, vbTextCompare) = 0 Then
        ValidateID = 15
        Exit Function
      End If
    End If
  Next i
  
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub AddOrRemoveRes()

  Dim i As Long
  
  On Error GoTo ErrHandler
  
  //if no form is active,
  If frmMDIMain.ActiveForm Is Nothing Then
    //can only mean that Settings.ShowPreview is false,
    //AND Settings.ResListType is non-zero, AND no editor window are open
    //use selected item method
    frmMDIMain.RemoveSelectedRes
  Else
    //if active form is NOT the preview form
    //if any form other than preview is active
    If frmMDIMain.ActiveForm.Name != "frmPreview" Then
      //use the active form method
      frmMDIMain.ActiveForm.MenuClickInGame
    Else
      //removing a preview resource; first check for an open
      //editor that matches resource being previewed
      Select Case SelResType
      Case rtLogic
        //if any logic editor matches this resource
        For i = 1 To LogicEditors.Count
          If LogicEditors(i).FormMode = fmLogic Then
            If LogicEditors(i).LogicNumber = SelResNum Then
              //use this form//s method
              LogicEditors(i).MenuClickInGame
              Exit Sub
            End If
          End If
        Next i
        
      Case rtPicture
        //if any Picture editor matches this resource
        For i = 1 To PictureEditors.Count
          If PictureEditors(i).PicNumber = SelResNum Then
            //use this form//s method
            PictureEditors(i).MenuClickInGame
            Exit Sub
          End If
        Next i
        
      Case rtSound
        //if any Sound editor matches this resource
        For i = 1 To SoundEditors.Count
          If SoundEditors(i).SoundNumber = SelResNum Then
            //use this form//s method
            SoundEditors(i).MenuClickInGame
            Exit Sub
          End If
        Next i
        
      Case rtView
        //if any View editor matches this resource
        For i = 1 To ViewEditors.Count
          If ViewEditors(i).ViewNumber = SelResNum Then
            //use this form//s method
            ViewEditors(i).MenuClickInGame
            Exit Sub
          End If
        Next i
      
      Case Else //words, objects, game or none
        //InGame does not apply
        
      End Select
      
      //if no open editor is found, use the selected item method
      frmMDIMain.RemoveSelectedRes
    End If
  End If
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub ChangeGameID(ByVal NewID As String)

  On Error GoTo ErrHandler

  //if property file is linked to game ID
  If StrComp(GameFile, GameDir + GameID + ".wag", vbTextCompare) = 0 Then
    //update mru value
    RenameMRU GameFile, GameDir + NewID + ".wag"
  End If
  
  //update name of layout file, if it exists (it always should
  //match GameID)
  On Error Resume Next
  Name GameDir + GameID + ".wal" As GameDir + NewID + ".wal"
  
  On Error GoTo ErrHandler
  
  //change id (which changes the game file automatically, if it
  //is linked to ID)
  GameID = NewID
  
  //update resource list
  Select Case Settings.ResListType
  Case 1
    frmMDIMain.tvwResources.Nodes(1).Text = GameID
  Case 2
    frmMDIMain.cmbResType.List(0) = GameID
  End Select
  //update form caption
  frmMDIMain.Caption = "WinAGI GDS - " + GameID

Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Function CheckLine(ByRef strLine As String) As Long

  //this function will examine a line of text to see if the
  //end pos is either in a comment or in a string
  
  // return values:
  //  0 = endpos for this line is NOT in quote or comment
  //  1 = endpos for this line is in a quote
  //  2 = endpos for this line is in a comment
  
  
  Dim lngPos As Long
  Dim rtn As Long, i As Long
  Dim blnInQuote As Boolean, blnInComment As Boolean
  
  On Error GoTo ErrHandler
  
  //length
  rtn = Len(strLine)
  i = 1
  
  Do Until i > rtn
    //is this a string character or comment character?
    Select Case AscB(Mid$(strLine, i))
    Case 34 //double quote mark
      If IsValidQuote(strLine, i) Then
        //if not already in a quote
        If Not blnInQuote Then
          //now we are...
          blnInQuote = true
        Else
          //now we aren//t...
          blnInQuote = false
        End If
      End If
      
    Case 47 //slash, to check for //////
      //if not in a quote
      If Not blnInQuote Then
        //check for dbl slash
        If i < Len(strLine) Then
          If AscB(Mid$(strLine, i + 1)) = 47 Then
            //this line has a comment at the end
            blnInComment = true
            Exit Do
          End If
        End If
      End If
    
    Case 91 //open bracket //[// is also a comment marker
      //if not in a quote
      If Not blnInQuote Then
        //this line has a comment starting here
        blnInComment = true
        Exit Do
      End If
    End Select
    
    i = i + 1
  Loop
  
  // return the result
  If blnInQuote Then
    CheckLine = 1
  ElseIf blnInComment Then
    CheckLine = 2
  Else
    //neither
    CheckLine = 0
  End If
  
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub BuildGDefLookup()

  //loads all global defines into single list for use by
  //the logic tooltip lookup function
  
  //we don't have to worry about errors (we just ignore them)
  //or order (add them as they are in the file)
  //or duplicate ("just leave it!!!") :-)
  //or resdef overrides (it//s actually easier for the tootip function anyway)

  Dim intFile As Integer, strFileName As String
  Dim strLine As String, strSplitLine() As String
  Dim tmpDef As TDefine, blnTry As Boolean
  Dim i As Long, NumDefs As Long
    
  On Error Resume Next
  //open file for input
  strFileName = GameDir + "globals.txt"
  intFile = FreeFile()
  Open strFileName For Input As intFile
  //if error opening file, just exit
  If Err.Number != 0 Then
    Err.Clear
    Exit Sub
  End If
  
  //trap errors rest of the way
  //so we load as many as we are able
  On Error GoTo ErrHandler
  
  //clear the lookup list
  ReDim GDefLookup(0)
  
  //read in globals
  Do Until EOF(intFile)
    //get line
    Line Input #intFile, strLine
    //trim it - also, skip comments
    strLine = StripComments(strLine, "")
    
    //ignore blanks
    If LenB(strLine) != 0 Then
      //even though new format is to match standard #define format,
      //still need to look for old format first just in case;
      //when saved, the file will be in the new format
      
      //assume not valid until we prove otherwise
      blnTry = false
    
      //splitline into name and Value
      strSplitLine = Split(strLine, vbTab)
      
      //if exactly two elements,
      If UBound(strSplitLine) = 1 Then
        tmpDef.Name = Trim$(strSplitLine(0))
        tmpDef.Value = Trim$(strSplitLine(1))
        blnTry = true
        
      //not a valid global.txt; check for defines.txt
      Else
        //tabs need to be replaced with spaces first
        strLine = Trim$(Replace(strLine, vbTab, " "))
        If Left$(strLine, 8) = "#define " Then
          //strip off the define statement
          strLine = Trim$(Right$(strLine, Len(strLine) - 8))
          //extract define name
          i = InStr(1, strLine, " ")
          If i != 0 Then
            tmpDef.Name = Left$(strLine, i - 1)
            strLine = Right$(strLine, Len(strLine) - i)
            tmpDef.Value = Trim$(strLine)
            blnTry = true
          End If
        End If
      End If
    
      //if the line contains a define, add it to list
      //here we don't bother validating; if it//s a bad
      //define, then user will have to deal with it;
      //it//s only a tooltip at ths point
      If blnTry Then
        tmpDef.Type = DefTypeFromValue(tmpDef.Value)
        
        ReDim Preserve GDefLookup(NumDefs)
        GDefLookup(NumDefs) = tmpDef
        //increment count
        NumDefs = NumDefs + 1
      End If
    //done with blank line
    End If
  Loop
  
  //close file
  Close intFile
  
  //don't need to worry about open editors; the initial build is
  //only called when a game is first loaded; changes to the
  //global lookup list are handled by the Global Editor
Exit Sub

ErrHandler:
  //Debug.Assert false
  //ignore it and try to carry on
  //that way we (hopefully) get as many
  //valid defines added as possible
  Resume Next
End Sub

public Function DecodeSnippet(ByVal SnipText As String) As String

  //replaces control codes in SnipText and returns
  //the full expanded text
  
  //(does not handle argument values; they are left in
  // place until needed when a snippet is inserted into
  // a logic)
  
  //first check for //%%// - temporarily replace them
  // with tabs
  DecodeSnippet = Replace(SnipText, "%%", Chr(9))
  
  // carriage returns/new lines
  DecodeSnippet = Replace(DecodeSnippet, "%n", vbCr)
  
  //quote marks
  DecodeSnippet = Replace(DecodeSnippet, "%q", QUOTECHAR)
  
  //tabs
  DecodeSnippet = Replace(DecodeSnippet, "%t", Space(Settings.LogicTabWidth))
  
  //lastly, restore any forced percent signs
  DecodeSnippet = Replace(DecodeSnippet, Chr(9), "%")
End Function

public Function EnCodeSnippet(ByVal FullText As String) As String

  //converts full text into snippet text by inserting
  //control codes where needed
  
  //(does not handle argument values- they have to be
  // inserted manually by the user before calling this
  // function)
  
  Dim lngPos As Long
  
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
    If InStr(lngPos + 1, EnCodeSnippet, Space(Settings.LogicTabWidth)) = lngPos + 1 Then
      //replace the spaces
      EnCodeSnippet = Left(EnCodeSnippet, lngPos) + Replace(EnCodeSnippet, Space(Settings.LogicTabWidth), "%t", lngPos + 1, 1)
      //adjust position
      lngPos = lngPos + 2
    Else
      //no more tabs for this line; get start of next line
      lngPos = InStr(lngPos + 1, EnCodeSnippet, vbCr)
      //if none
      If lngPos = 0 Then
        //done
        Exit Do
      End If
    End If
  Loop While true
    
  // cr to %n
  EnCodeSnippet = Replace(EnCodeSnippet, vbCr, "%n")
  
  //re-insert percents
  EnCodeSnippet = Replace(EnCodeSnippet, Chr(9), "%")
  
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub UpdateLEStatus()

  With frmMDIMain
    //if layout editor is no longer in use
    If Not UseLE Then
      //disable the menubar and toolbar
      .mnuTLayout.Enabled = false
      .Toolbar1.Buttons("layout").Enabled = false
      //if using it, need to close it
      If LEInUse Then
        MsgBoxEx "The current layout editor file will be closed. " + vbCrLf + vbCrLf + _
        "If you decide to use the layout editor again at a later" + vbCrLf + _
        "time, you will need to rebuild the layout to update it. ", vbOKOnly + vbInformation + vbMsgBoxHelpButton, "Closing Layout Editor", WinAGIHelp, "htm\winagi\Layout_Editor.htm"
        //save it, if dirty
        If LayoutEditor.IsDirty Then
          LayoutEditor.MenuClickSave
        End If
        //close it
        Unload LayoutEditor
        LEInUse = false
      End If
    Else
      //enable the menu and toolbar
      .mnuTLayout.Enabled = true
      .Toolbar1.Buttons("layout").Enabled = true
    End If
  End With
End Sub

public Function DefTypeFromValue(ByVal strValue As String) As ArgTypeEnum

  On Error GoTo ErrHandler
  
  If IsNumeric(strValue) Then
    DefTypeFromValue = atNum
  ElseIf Asc(strValue) = 34 Then
    DefTypeFromValue = atDefStr
  Else
    Select Case Asc(LCase(strValue))
    Case 99 //"c"
      DefTypeFromValue = atCtrl
    Case 102 //"f"
      DefTypeFromValue = atFlag
    Case 105 //"i"
      DefTypeFromValue = atIObj
    Case 109 //"m"
      DefTypeFromValue = atMsg
    Case 111 //"o"
      DefTypeFromValue = atSObj
    Case 115 //"s"
      DefTypeFromValue = atStr
    Case 118 //"v"
      DefTypeFromValue = atVar
    Case 119 //"w"
      DefTypeFromValue = atWord
    Case Else
      //assume a defined string
      DefTypeFromValue = atDefStr
    End Select
  End If
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Function IsResource(ByVal strToken As String) As Boolean
  
  //returns true if strToken is a valid resource ID in a game
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  //if game not loaded, always return false
  If Not GameLoaded Then
    IsResource = false
    Exit Function
  End If
  
  // blanks never match
  If Len(strToken) = 0 Then
    IsResource = false
    Exit Function
  End If
    
  //step through all resources
  //(use globals list; it//s going to be much faster)
  
  For i = 0 To 1023
    If IDefLookup(i).Name = strToken Then
      IsResource = true
      Exit Function
    End If
  Next i
  
  //not found, must not be a resource
  IsResource = false
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Function LogTemplateText(ByVal NewID As String, ByVal NewDescription As String) As String

  On Error GoTo ErrHandler
  
  Dim strLogic As String, intFile As Integer
  Dim rtn As VbMsgBoxResult, blnNoFile As Boolean
  
  //first, get the default file, if there is one
  If FileExists(ProgramDir + "deflog.txt") Then
    On Error Resume Next
    intFile = FreeFile()
    Open ProgramDir + "deflog.txt" For Binary As intFile
    strLogic = String$(LOF(intFile), 0)
    Get intFile, 1, strLogic
    Close intFile
    If Err.Number != 0 Then
      // problem with the file
      blnNoFile = true
    End If
  Else
    // no file
    blnNoFile = true
  End If
  
  //if no template file
  If blnNoFile Then
    //something didn//t work; let user know
    rtn = MsgBoxEx("The default logic template file (//deflog.txt//) is missing" + vbCrLf + _
                   "from the WinAGI program directory. Using the WinAGI default" + vbCrLf + _
                   "template instead.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Missing Template File", WinAGIHelp, "htm\winagi\newres.htm#logtemplate")
    strLogic = LoadResString(101)
    //insert line breaks
    strLogic = Replace(strLogic, "|", vbCr)
  End If

  // trap any other errors
  On Error GoTo ErrHandler
      
  //substitute correct values for the various place holders
  
  //add the tabs
  strLogic = Replace(strLogic, "~", Space$(Settings.LogicTabWidth))
  
  //id:
  strLogic = Replace(strLogic, "%id", NewID)
  
  //description
  strLogic = Replace(strLogic, "%desc", NewDescription)
        
  //horizon is a PicTest setting, which should always be retrieved everytime
  //it is used to make sure it//s current
  strLogic = Replace(strLogic, "%h", ReadSettingLong(SettingsList, sPICTEST, "Horizon", DEFAULT_PICTEST_HORIZON))
  
  //if using reserved names, insert them
  If LogicSourceSettings.UseReservedNames Then
   //f5, v0, f2, f4, v9
    strLogic = Replace(strLogic, "f5", LogicSourceSettings.ReservedDefines(atFlag)(5).Name)
    strLogic = Replace(strLogic, "f2", LogicSourceSettings.ReservedDefines(atFlag)(2).Name)
    strLogic = Replace(strLogic, "f4", LogicSourceSettings.ReservedDefines(atFlag)(4).Name)
    strLogic = Replace(strLogic, "v0", LogicSourceSettings.ReservedDefines(atVar)(0).Name)
    strLogic = Replace(strLogic, "v9", LogicSourceSettings.ReservedDefines(atVar)(9).Name)
  End If
  
  //return the formatted text
  LogTemplateText = strLogic
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub BuildIDefLookup()

  //adds all resource IDs to the table, making sure
  //anything that//s blank gets reset
  
  //***NOTE that order of resources is
  // LOGIC-VIEW-SOUND-PIC; not the normal L-P-S-V
  // this is because logics are most likely to be
  // referenced, followed by views, then sounds,
  // and pictures are least likely to be referenced
  // by their IDs
  
  Dim i As Long, tmpDef As TDefine, tmpBlank As TDefine
  Dim last As Long
  
  On Error GoTo ErrHandler
  
  //blanks have a type of 11 (>highest available)
  tmpBlank.Type = 11
  
  //on initial build, populate all the values,
  //because they never change
  
  //logics first
  With Logics
    last = .Max
    For i = 0 To last
      If .Exists(i) Then
        tmpDef.Name = .Item(i).ID
        tmpDef.Type = atNum
        tmpDef.Value = i
        IDefLookup(i) = tmpDef
      Else
        tmpBlank.Value = i
        IDefLookup(i) = tmpBlank
      End If
    Next i
    For i = last + 1 To 255
      tmpBlank.Value = i
      IDefLookup(i) = tmpBlank
    Next i
  End With
  
  //views next
  With Views
    last = .Max
    For i = 0 To last
      If .Exists(i) Then
        tmpDef.Name = .Item(i).ID
        tmpDef.Type = atNum
        tmpDef.Value = i
        IDefLookup(i + 256) = tmpDef
      Else
        tmpBlank.Value = i
        IDefLookup(i + 256) = tmpBlank
      End If
    Next i
    For i = last + 1 To 255
      tmpBlank.Value = i
      IDefLookup(i + 256) = tmpBlank
    Next i
  End With
    
  //then sounds next
  With Sounds
    last = .Max
    For i = 0 To last
      If .Exists(i) Then
        tmpDef.Name = .Item(i).ID
        tmpDef.Type = atNum
        tmpDef.Value = i
        IDefLookup(i + 512) = tmpDef
      Else
        tmpBlank.Value = i
        IDefLookup(i + 512) = tmpBlank
      End If
    Next i
    For i = last + 1 To 255
      tmpBlank.Value = i
      IDefLookup(i + 512) = tmpBlank
    Next i
  End With
  
  //pictures last (least likely to be used in a logic by ID)
  With Pictures
    last = .Max
    For i = 0 To last
      If .Exists(i) Then
        tmpDef.Name = .Item(i).ID
        tmpDef.Type = atNum
        tmpDef.Value = i
        IDefLookup(i + 768) = tmpDef
      Else
        tmpBlank.Value = i
        IDefLookup(i + 768) = tmpBlank
      End If
    Next i
    For i = last + 1 To 255
      tmpBlank.Value = i
      IDefLookup(i + 768) = tmpBlank
    Next i
  End With

  //don't need to worry about open editors; the initial build is
  //only called when a game is first loaded; changes to the
  //ID lookup list are handled by the add/remove resource functions
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub BuildRDefLookup()
  
  //populate the lookup list that logics will
  //use to support tooltips and define list lookups
  
  Dim i As Long, tmpDef() As TDefine
  
  On Error GoTo ErrHandler
  
  With LogicSourceSettings
    // step through each type of define value, add it to list
    
    //27 variables
    tmpDef = .ResDefByGrp(1)
    For i = 0 To 26
      RDefLookup(i) = tmpDef(i)
    Next i
    
    //18 flags
    tmpDef = .ResDefByGrp(2)
    For i = 0 To 17
      RDefLookup(i + 27) = tmpDef(i)
    Next i
    
    //5 edge codes
    tmpDef = .ResDefByGrp(3)
    For i = 0 To 4
      RDefLookup(i + 45) = tmpDef(i)
    Next i
    
    //9 directions
    tmpDef = .ResDefByGrp(4)
    For i = 0 To 8
      RDefLookup(i + 50) = tmpDef(i)
    Next i
    
    //5 video modes
    tmpDef = .ResDefByGrp(5)
    For i = 0 To 4
      RDefLookup(i + 59) = tmpDef(i)
    Next i
    
    //9 computer types
    tmpDef = .ResDefByGrp(6)
    For i = 0 To 8
      RDefLookup(i + 64) = tmpDef(i)
    Next i
    
    //16 colors
    tmpDef = .ResDefByGrp(7)
    For i = 0 To 15
      RDefLookup(i + 73) = tmpDef(i)
    Next i
    
    //6 others
    tmpDef = .ResDefByGrp(8)
    For i = 0 To 5
      RDefLookup(i + 89) = tmpDef(i)
    Next i
  End With

  //then let open logic editors know
  If LogicEditors.Count > 1 Then
    For i = 1 To LogicEditors.Count
      LogicEditors(i).ListDirty = true
    Next i
  End If
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub BuildSnippets()
  
  //loads snippet file, and creates array of
  //snippets
  
  Dim intFile As Long, strFileName As String, SnipList As StringList
  Dim strLine As String, lngCount As Long
  Dim i As Long, lngAdded As Long
  
  On Error GoTo ErrHandler
  
  //open the snippet file
  strFileName = ProgramDir + "snippets.txt"
  If Dir(strFileName) = "" Then
    //no snippets
    Exit Sub
  End If
  
  //open layout file for input
  intFile = FreeFile()
  Open strFileName For Binary As intFile
  //get all the text
  strLine = String$(LOF(intFile), 0)
  Get intFile, 1, strLine
  //done with the file
  Close intFile
  
  //assign to stringlist
  Set SnipList = New StringList
  SnipList.Assign strLine
  //insert line where filename is stored
  SnipList.Add "", 0
  
  // get count
  lngCount = ReadSettingLong(SnipList, "General", "Count", 0)
  //if none
  If lngCount <= 0 Then
    //no Snippets
    Set SnipList = Nothing
    Exit Sub
  End If
  
  //create snippet array (array is 1-based; this makes it
  // easier to keep track of count by just checking Ubound
  ReDim CodeSnippets(lngCount)
  //retrieve each snippet (no error checking is done
  // except for blank value or blank name; in that case
  // the snippet is ignored; if duplicate names exist,
  // they are added, and user will just have to deal
  // with it...
  
  lngAdded = 0
  For i = 1 To lngCount
    With CodeSnippets(lngAdded + 1)
      //name
      .Name = ReadSettingString(SnipList, "Snippet" + CStr(lngAdded + 1), "Name", "")
      //value
      .Value = ReadSettingString(SnipList, "Snippet" + CStr(lngAdded + 1), "Value", "")
    
      //if name and value are non-null
      If Len(.Name) > 0 And Len(.Value) > 0 Then
        //decode the snippet (replaces control codes)
        .Value = DecodeSnippet(.Value)
        
        //count it as added
        lngAdded = lngAdded + 1
        
      Else
        //one or both of name/value are blank - not a
        //valid snippet so ignore it
      End If
    End With
  Next i
  
  //if some were skipped
  If lngAdded < lngCount Then
    //shrink the array
    ReDim Preserve CodeSnippets(lngAdded)
  End If
  
  // done!
  Set SnipList = Nothing
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub


public Sub ChangeResDir(ByVal NewResDir As String)

  On Error GoTo ErrHandler
  
  Dim rtn As VbMsgBoxResult
  
  //display wait cursor while copying
  WaitCursor
   
  //if the folder already exists,
  If LenB(Dir(GameDir + NewResDir, vbDirectory)) != 0 Then
    //ask if replace is OK?
    rtn = MsgBox("Existing files in new directory will be overwritten by " + vbNewLine + _
                 "resources with same name. Do you want to continue?", vbQuestion + vbYesNo, "Change Resource Directory")
    
    //if no, do nothing; don't change resdir
    If rtn = vbYes Then
    
      //show progress form
      Load frmProgress
      With frmProgress
        .Caption = "Changing Resource Directory"
        .lblProgress = "Depending on size of game, this may take awhile. Please wait..."
        .pgbStatus.Visible = false
        .Show
        .Refresh
      End With
      
      //move them
      If Not CopyFolder(ResDir, GameDir + NewResDir, true) Then
        //warn
        MsgBox "Not all files were able to be moved. Check your old and new directories" + vbNewLine + "and manually move any remaining resources.", vbInformation, "File Move Error"
      End If
      //change resdir
      ResDirName = NewResDir
      
      //done with progress form
      Unload frmProgress
      MsgBox "Done!", vbOKOnly + vbInformation, "Change Resource Directory"
    End If
    
  Else
    //show progress form
    Load frmProgress
    With frmProgress
      .Caption = "Changing Resource Directory"
      .lblProgress = "Depending on size of game, this may take awhile. Please wait..."
      .pgbStatus.Visible = false
      .Show
      .Refresh
    End With
      
    //need to create a new directory
    MkDir GameDir + NewResDir
    //and move the existing resdir to the new location
    
    If Not CopyFolder(ResDir, GameDir + NewResDir, true) Then
      //warn
      MsgBox "Not all files were able to be moved. Check your old and new directories" + vbNewLine + "and manually move any remaining resources.", vbInformation, "File Move Error"
    End If
    //change resdir
    ResDirName = NewResDir
    
    //done with progress form
    Unload frmProgress
    MsgBox "Done!", vbOKOnly + vbInformation, "Change Resource Directory"
  End If
  
  //update the preview window, if previewing logics,
  UpdateSelection rtLogic, SelResNum, umPreview
  
  //reset cursor
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub ErrMsgBox(ByVal ErrMsg1 As String, ByVal ErrMsg2 As String, ByVal ErrCaption As String)

  //displays a messagebox showing ErrMsg and includes error passed as AGIErrObj
  //Debug.Assert Err.Number != 0
  
  Dim lngErrNum As Long, strErrMsg As String
  
  //determine if ErrNum is an AGI number:
  If (Err.Number And vbObjectError) = vbObjectError Then
    lngErrNum = Err.Number - vbObjectError
  Else
    lngErrNum = Err.Number
  End If
  
  strErrMsg = ErrMsg1 + vbCrLf + vbCrLf + lngErrNum + ": " + Err.Description
  If Len(ErrMsg2) > 0 Then
    strErrMsg = strErrMsg + vbCrLf + vbCrLf + ErrMsg2
  End If
  
  MsgBox strErrMsg, vbCritical + vbOKOnly, ErrCaption
End Sub

public Sub ExportAllPicImgs()

  //exports all picture images as one format in src dir
  
  Dim blnCanceled As Boolean, rtn As Long
  Dim lngZoom As Long, lngMode As Long, lngFormat As Long
  Dim ThisPic As AGIPicture, strExt As String
  Dim blnLoaded As Boolean
  
  On Error GoTo ErrHandler
  
  //show options form
  Load frmPicExpOptions
  
  With frmPicExpOptions
    // force image only
    .SetForm 1
    .Caption = "Export All Picture Images"
    .Show vbModal, frmMDIMain
    blnCanceled = .Canceled
    lngZoom = CLng(.txtZoom.Text)
    lngFormat = .cmbFormat.ListIndex + 1
    If .optVisual.Value Then
      lngMode = 0
    ElseIf .optPriority.Value Then
      lngMode = 1
    Else
      lngMode = 2
    End If
  End With
  
  //done with the options form
  Unload frmPicExpOptions
  
  //show wait cursor
  WaitCursor
  
  //need to get correct file extension
  Select Case lngFormat
  Case 1
    strExt = ".bmp"
  Case 2
    strExt = ".jpg"
  Case 3
    strExt = ".gif"
  Case 4
    strExt = ".tif"
  Case 5
    strExt = ".png"
  End Select
  
  //if not canceled, export them all
  If Not blnCanceled Then
    //setup progress form
    Load frmProgress
    With frmProgress
      .Caption = "Exporting All Picture Images"
      .pgbStatus.Max = Pictures.Count
      .pgbStatus.Value = 0
      .lblProgress.Caption = "Exporting..."
      .Show
      .Refresh
      
      For Each ThisPic In Pictures
        .lblProgress.Caption = "Exporting " + ThisPic.ID + " Image..."
        .pgbStatus.Value = .pgbStatus.Value + 1
        .Refresh
        SafeDoEvents
        
        //load pic if necessary
        blnLoaded = ThisPic.Loaded
        If Not blnLoaded Then
          ThisPic.Load
        End If
        ExportImg ThisPic, ResDir + ThisPic.ID + strExt, lngFormat, lngMode, lngZoom
        If Not blnLoaded Then
          ThisPic.Unload
        End If
      Next
    End With
    //done with progress form
    Unload frmProgress
  End If

  //restore cursor
  Screen.MousePointer = vbDefault
  
Exit Sub

ErrHandler:
  
  //Debug.Assert false
  Resume Next
End Sub

Private Sub ExportImg(ExportPic As AGIPicture, ByVal ExportFile As String, ByVal ImgFormat As Long, ByVal ImgMode As Long, ByVal ImgZoom As Long)

  //exports pic gdpImg

  Dim strTmpFile As String, Count As Long
  Dim gdpImg As Long, encoderCLSID As CLSID
  Dim lngQuality As Long, encoderParams As EncoderParameters
  Dim stat As GpStatus
  
  //make sure existing file is deleted
  On Error Resume Next
  Kill ExportFile
  On Error GoTo ErrHandler

  //get tmp file for holding temp BMP images
  strTmpFile = TempFileName()
  
  Count = 0
  //mode:  0=vis
  //       1=pri
  //       2=both
  Do
    //set up source gdpImg and filename
    //
    //if second time through, adjust output filename
    If Count = 1 Then
      //get name for pri gdpImg
      ExportFile = Left(ExportFile, Len(ExportFile) - 4) + "_P" + Right(ExportFile, 4)
    End If
    
    //if 1st time through AND mode is 0 or 2: vis
    //if second time through OR mode is 1: pri
    If Count = 0 And ImgMode != 1 Then
      //save vis as temporary BMP
      ExportPicBMP ExportPic.VisData, strTmpFile, ImgZoom
    Else
      //save vis as temporary BMP
      ExportPicBMP ExportPic.PriData, strTmpFile, ImgZoom
    End If
    
    //readjust format choice based on file extension
    Select Case UCase(Right(ExportFile, 4))
    Case ".BMP"
      ImgFormat = 1
    Case ".JPG"
      ImgFormat = 2
    Case ".GIF"
      ImgFormat = 3
    Case ".GIF"
      ImgFormat = 4
    Case ".PNG"
      ImgFormat = 5
    Case Else
      //use what was already chosen
    End Select
    
    //if gdiplus not available, always save as bmp
    If ImgFormat = 1 Or NoGDIPlus Then
      //just copy temp file to desired exportfile
      FileCopy strTmpFile, ExportFile
    Else
      //load bmp into gdi+
      GdipLoadImageFromFile StrConv(strTmpFile, vbUnicode), gdpImg
      
      Select Case ImgFormat
      Case 2 //JPG
      
        // Save as a JPEG file. This format requires encoder parameters.
        // Get the CLSID of the PNG encoder
        Call GetEncoderClsid("Image/jpeg", encoderCLSID)
      
        lngQuality = 100   // Quality is 100% of original
        // Setup the encoder paramters
        encoderParams.Count = 1    // Only one element in this Parameter array
        With encoderParams.Parameter
          .NumberOfValues = 1     // Should be one
          .Type = EncoderParameterValueTypeLong
          // Set the GUID to EncoderQuality
          .GUID = DEFINE_GUID(EncoderQuality)
          .Value = VarPtr(lngQuality)  // Remember: The Value expects only pointers!
        End With
 
        // Now save the bitmap as a jpeg at 10% compression
        stat = GdipSaveImageToFile(gdpImg, StrConv(ExportFile, vbUnicode), encoderCLSID, encoderParams)
      
      Case 3 //GIF
        // Get the CLSID of the PNG encoder
        Call GetEncoderClsid("Image/gif", encoderCLSID)
      
        // Save as a gif file. There are no encoder parameters for PNG images, so we pass a NULL.
        // NOTE: The NULL (aka 0) must be passed byval, as the function declaration would get a pointer to the number 0.
        stat = GdipSaveImageToFile(gdpImg, StrConv(ExportFile, vbUnicode), encoderCLSID, ByVal 0)
        
      Case 4 //TIF
        // Get the CLSID of the PNG encoder
        Call GetEncoderClsid("Image/tiff", encoderCLSID)
      
        // Save as a gif file. There are no encoder parameters for PNG images, so we pass a NULL.
        // NOTE: The NULL (aka 0) must be passed byval, as the function declaration would get a pointer to the number 0.
        stat = GdipSaveImageToFile(gdpImg, StrConv(ExportFile, vbUnicode), encoderCLSID, ByVal 0)
        
      Case 5 //PNG
        // Get the CLSID of the PNG encoder
        Call GetEncoderClsid("Image/png", encoderCLSID)
      
        // Save as a PNG file. There are no encoder parameters for PNG images, so we pass a NULL.
        // NOTE: The NULL (aka 0) must be passed byval, as the function declaration would get a pointer to the number 0.
        stat = GdipSaveImageToFile(gdpImg, StrConv(ExportFile, vbUnicode), encoderCLSID, ByVal 0)
      
      End Select
      
      // See if it was created
      If stat != gpsOk Then
        //make sure Image cleaned up
        Call GdipDisposeImage(gdpImg)
        //return error condition
        On Error GoTo 0: Err.Raise vbObjectError + 586, "ExportImg", "unable to export PNG file: Status Code=" + stat
      End If
       
      // Cleanup
      Call GdipDisposeImage(gdpImg)
    End If
    
    Count = Count + 1
    //if only one Image being exported OR both are done
    If ImgMode < 2 Or Count = 2 Then
      Exit Do
    End If
  Loop While true
  
  //done with temp file; delete it
  Kill strTmpFile
Exit Sub

ErrHandler:

  //Debug.Assert false
  Resume Next
End Sub

public Sub ExportLoop(ThisLoop As AGILoop)

  //export a loop as a gif
  
  Dim blnCanceled As Boolean, rtn As Long
  
  On Error GoTo ErrHandler
  
  //show options form
  Load frmViewGifOptions
  With frmViewGifOptions
    //set up form to export a view loop
    .InitForm 0, ThisLoop
    .Show vbModal, frmMDIMain
    blnCanceled = .Canceled
    
    //if not canceled, get a filename
    If Not blnCanceled Then
    
      //set up commondialog
      With MainSaveDlg
        .DialogTitle = "Export Loop GIF"
        .DefaultExt = "gif"
        .Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*"
        .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
        .FilterIndex = 1
        .FullName = ""
        .hWndOwner = frmMDIMain.hWnd
      End With
      
      Do
        On Error Resume Next
        MainSaveDlg.ShowSaveAs
        //if canceled,
        If Err.Number = cdlCancel Then
          //cancel the export
          blnCanceled = true
          Exit Do
        End If
        
        //if file exists,
        If FileExists(MainSaveDlg.FullName) Then
          //verify replacement
          rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
          
          If rtn = vbYes Then
            Exit Do
          ElseIf rtn = vbCancel Then
            blnCanceled = true
            Exit Do
          End If
        Else
          Exit Do
        End If
      Loop While true
      On Error GoTo ErrHandler
    End If
    
    //if NOT canceled, then export!
    If Not blnCanceled Then
      //show progress form
      Load frmProgress
      With frmProgress
        .Caption = "Exporting Loop as GIF"
        .lblProgress = "Depending in size of loop, this may take awhile. Please wait..."
        .pgbStatus.Visible = false
        .Show
      End With
      
      //show wait cursor
      WaitCursor

      MakeLoopGif ThisLoop, VGOptions, MainSaveDlg.FullName
      
      //all done!
      Unload frmProgress
      MsgBox "Success!", vbInformation + vbOKOnly, "Export Loop as GIF"
      
      Screen.MousePointer = vbDefault
    End If
    
    //done with the options form
    Unload frmViewGifOptions
    
  End With
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Function CheckLogics() As Boolean

  //checks all logics; if any found that are dirty
  //allow user to recompile game if desired before
  //running
  
  Dim tmpLogic As AGILogic, i As Long
  Dim rtn As VbMsgBoxResult, blnDontAsk As Boolean
  Dim blnLoaded As Boolean
  
  On Error GoTo ErrHandler
  
  //assume ok
  CheckLogics = true
  
  //if not requiring recompile
  If Settings.CompileOnRun = 1 Then
    //don't even need to check
    //just exit
    Exit Function
  End If
  
  //step through all logics
  For Each tmpLogic In Logics
    blnLoaded = tmpLogic.Loaded
    If Not blnLoaded Then
      tmpLogic.Load
    End If
    If Not tmpLogic.Compiled Then
      //not ok; skip checking and
      //determine if recompiling is appropriate
      CheckLogics = false
      //dont// forget to unload!!
      If Not blnLoaded Then
        tmpLogic.Unload
      End If

      Exit For
    End If
    If Not blnLoaded Then
      tmpLogic.Unload
    End If
  Next
  
  //if no dirty logics found, check any existing logics that are being edited
  If CheckLogics = true Then
    For i = 1 To LogicEditors.Count
      If LogicEditors(i).FormMode = fmLogic Then
        If LogicEditors(i).rtfLogic.Dirty Then
          //one dirty logic found
          CheckLogics = false
          Exit For
        End If
      End If
    Next i
  End If
  
  //if still no dirty logics found
  If CheckLogics = true Then
    //just exit
    Exit Function
  End If
  
  //if CompileOnRun is in ask mode or yes mode, get user choice
  Select Case Settings.CompileOnRun
  Case 0 //ask for user input
    //get user//s response
    rtn = MsgBoxEx("One or more logics have changed since you last compiled." + vbNewLine + _
                   "Do you want to compile them before running?", vbQuestion + vbYesNoCancel, "Compile Before Running?", , , "Always take this action when compiling a game.", blnDontAsk)
    If blnDontAsk Then
      If rtn = vbYes Then
        Settings.CompileOnRun = 2
      ElseIf rtn = vbNo Then
        Settings.CompileOnRun = 1
      End If
      
      //update settings list
      WriteAppSetting SettingsList, sLOGICS, "CompOnRun", Settings.CompileOnRun
    End If
    
  Case 1  //no
    rtn = vbNo
    
  Case 2 // yes
      rtn = vbYes
  End Select
  
  Select Case rtn
  Case vbCancel
    //return false, so run cmd is canceled
    CheckLogics = false
    
  Case vbNo
    //ok to exit; check is complete
    CheckLogics = true
  Case vbYes
    //if dirtylogics successfully compiled
    CheckLogics = CompileDirtyLogics(true)
  End Select
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub CheckShortcuts(ByRef KeyCode As Integer, ByRef Shift As Integer)

  //check for game-wide shortcut keys
  Select Case Shift
  Case 3 //vbCtrlMask + vbShiftMask
    Select Case KeyCode
    Case vbKeyA //add/remove resource
      If frmMDIMain.mnuRInGame.Enabled Then
        AddOrRemoveRes
        KeyCode = 0
      End If
      
    Case vbKeyB //compile to
      If frmMDIMain.mnuGCompileTo.Enabled Then
        //compile game to directory of user//s choice
        CompileAGIGame
        KeyCode = 0
      End If
      
    Case vbKeyD //compile dirty logics
      If frmMDIMain.mnuGCompDirty.Enabled Then
        CompileDirtyLogics
        KeyCode = 0
      End If
      
    Case vbKeyN //new game from blank
      If frmMDIMain.mnuGNBlank.Enabled Then
        //create new blank game
        NewAGIGame false
        KeyCode = 0
      End If
      
    Case vbKeyR //rebuild VOL
      If frmMDIMain.mnuGRebuild.Enabled Then
        //rebuild volfiles only
        CompileAGIGame GameDir, true
        KeyCode = 0
      End If
    
    Case vbKeyT //insert a snippet
      If Not frmMDIMain.ActiveForm Is Nothing Then
        If frmMDIMain.ActiveForm.Name = "frmLogicEdit" Or frmMDIMain.ActiveForm.Name = "frmTextEdit" Then
          //open snippet manager
          SnipMode = 1
          frmSnippets.Show vbModal, frmMDIMain
          //force focus back to the editor
          frmMDIMain.ActiveForm.rtfLogic.SetFocus
        End If
      End If
      
    End Select
    
  Case vbCtrlMask + vbAltMask
    //import resources
    With frmMDIMain
      //but only if import menu is enabled
      If .mnuRImport.Enabled Then
        Select Case KeyCode
        Case vbKey1
          .RILogic
        Case vbKey2
          .RIPicture
        Case vbKey3
          .RISound
        Case vbKey4
          .RIView
        Case vbKey5
          .RIObjects
        Case vbKey6
          .RIWords
        End Select
      End If
    End With
    
  Case vbCtrlMask
    Select Case KeyCode
    Case vbKey1
      frmMDIMain.RNLogic
    Case vbKey2
      frmMDIMain.RNPicture
    Case vbKey3
      frmMDIMain.RNSound
    Case vbKey4
      frmMDIMain.RNView
    Case vbKey5
      frmMDIMain.RNObjects
    Case vbKey6
      frmMDIMain.RNWords
    Case vbKey7
      frmMDIMain.RNText
    End Select
    
  Case vbAltMask
    Select Case KeyCode
    Case vbKey1
      frmMDIMain.ROLogic
    Case vbKey2
      frmMDIMain.ROPicture
    Case vbKey3
      frmMDIMain.ROSound
    Case vbKey4
      frmMDIMain.ROView
    Case vbKey5
      frmMDIMain.ROObjects
    Case vbKey6
      frmMDIMain.ROWords
    Case vbKey7
      frmMDIMain.ROText
      
    Case vbKeyX //close game
      If frmMDIMain.mnuGClose.Enabled Then
        frmMDIMain.mnuGClose_Click
        KeyCode = 0
      End If
      
    Case vbKeyN //renumber
      If frmMDIMain.mnuRRenumber.Enabled Then
        frmMDIMain.mnuRRenumber_Click
        KeyCode = 0
      End If
      
    Case vbKeyF1  //logic command help
      //select commands start page
      HtmlHelp HelpParent, WinAGIHelp, HH_HELP_CONTEXT, 1001
      KeyCode = 0
    End Select
    
  Case 0 //no mask
  End Select
End Sub


public Function CmdInfo(rtfLogic As RichEdAGI) As String

  Dim rtn As Long, tmpRange As RichEditAGI.Range
  Dim lngCmdPos As Long
  Dim strLine As String
  
  On Error GoTo ErrHandler
  
  //set cmd start pos
  lngCmdPos = rtfLogic.Selection.Range.EndPos
  
    //get line where enter was pressed
    rtn = SendMessage(rtfLogic.hWnd, EM_LINEFROMCHAR, rtfLogic.Selection.Range.StartPos, 0)
    //get the start of this line
    rtn = SendMessage(rtfLogic.hWnd, EM_LINEINDEX, rtn, 0)
    Set tmpRange = rtfLogic.Range(rtn, rtn)
    tmpRange.Expand (reLine)
    strLine = Left$(tmpRange.Text, Len(tmpRange.Text) - 1)
    Set tmpRange = Nothing
    CmdInfo = strLine
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub ExportOnePicImg(ThisPicture As AGIPicture)

  //exports a picture vis screen and/or pri screen as either bmp or gif, or png
  
  Dim blnCanceled As Boolean, rtn As Long
  Dim lngZoom As Long, lngMode As Long, lngFormat As Long
  
  On Error GoTo ErrHandler
  
  //show options form
  Load frmPicExpOptions
  
  With frmPicExpOptions
    //save image only
    .SetForm 1
    .Show vbModal, frmMDIMain
    
    blnCanceled = .Canceled
    lngZoom = CLng(.txtZoom.Text)
    lngFormat = .cmbFormat.ListIndex + 1
    If .optVisual.Value Then
      lngMode = 0
    ElseIf .optPriority.Value Then
      lngMode = 1
    Else
      lngMode = 2
    End If
  End With
  
  //done with the options form
  Unload frmPicExpOptions
  
  //if not canceled, get a filename
  If Not blnCanceled Then
    //set up commondialog
    With MainSaveDlg
      .DialogTitle = "Save Picture Image As"
      .DefaultExt = "bmp"
      If NoGDIPlus Then
        .Filter = "BMP files (*.bmp)|*.bmp|All files (*.*)|*.*"
      Else
        .Filter = "BMP files (*.bmp)|*.bmp|JPEG files (*.jpg)|*.jpg|GIF files (*.gif)|*.gif|TIFF files (*.tif)|*.tif|PNG files (*.PNG)|*.png|All files (*.*)|*.*"
      End If
      .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
      .FilterIndex = lngFormat
      .FullName = ""
      .hWndOwner = frmMDIMain.hWnd
    End With
  
    Do
      On Error Resume Next
      MainSaveDlg.ShowSaveAs
      //if canceled,
      If Err.Number = cdlCancel Then
        //cancel the export
        Err.Clear
        blnCanceled = true
        Exit Do
      End If
      
      //if file exists,
      If FileExists(MainSaveDlg.FullName) Then
        //verify replacement
        rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
        
        If rtn = vbYes Then
          Exit Do
        ElseIf rtn = vbCancel Then
          blnCanceled = true
          Exit Do
        End If
      Else
        Exit Do
      End If
    Loop While true
    On Error GoTo ErrHandler
  End If
  
  //if NOT canceled, then export!
  If Not blnCanceled Then
    //show wait cursor
    WaitCursor

    //show progress form
    Load frmProgress
    With frmProgress
      .Caption = "Exporting Picture Image"
      .lblProgress = "Depending on export size, this may take awhile. Please wait..."
      .pgbStatus.Visible = false
      .Show
      .Refresh
    End With
    
    ExportImg ThisPicture, MainSaveDlg.FullName, lngFormat, lngMode, lngZoom
      
    //all done!
    Unload frmProgress
    MsgBox "Success!", vbInformation + vbOKOnly, "Export Picture Image"
    
    Screen.MousePointer = vbDefault
    
  End If
  
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Function FindPrevCmd(ByRef strText As String, ByRef lngStartPos As Long, ByRef lngArgCount As Long, Optional ByVal blnInQuote As Boolean = false, Optional ByVal blnPrev As Boolean = false) As String

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
  
  Dim lngPos As Long, strCmd As String
  
  On Error GoTo ErrHandler
  
  lngArgCount = 0
  lngPos = lngStartPos
  
  //find prev token
  strCmd = FindPrevToken(strText, lngPos, blnInQuote)
  
  Do Until Len(strCmd) = 0
    //if this is the command we want, return it
    If blnPrev Then
      Exit Do
    Else
      //if a comma, advance the arg count
      If strCmd = "," Then
        lngArgCount = lngArgCount + 1
      End If
    End If
    
    //single char cmds are only things we
    //have an interest in
    If Len(strCmd) = 1 Then
      Select Case Asc(strCmd)
      Case 40 //(//
        //next(prev) token is the command we are looking for
        blnPrev = true
      
      Case 41, 59 To 62, 123, 125 //)//, //;//,<,=,>, //{//, //}//
        //always exit
        Exit Do
      End Select
    End If
    
    //if exactly two characters, we check for other math operators
    If Len(strCmd) = 2 Then
      Select Case strCmd
      Case "=="
        //always exit
        Exit Do
      Case "!="
        //always exit
        Exit Do
      Case "<=", "=<"
        //always exit
        Exit Do
      Case ">=", "=>"
        //always exit
        Exit Do
      Case "&&", "||", "++", "--"
        //always exit
        Exit Do
      End Select
    End If
    
    //get next(prev) cmd
    strCmd = FindPrevToken(strText, lngPos, false)
  Loop
  
  FindPrevCmd = strCmd
  lngStartPos = lngPos + Len(strCmd)
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function
public Function FindPrevToken(ByRef strLine As String, ByRef lngStartPos As Long, ByRef blnInQuote As Boolean) As String

  //searches backwards through strLine and returns the token that precedes
  // the startpos
  //if startpos is within a token, that partial token is returned
  //
  //if beginning of line is reached, return empty string
  //
  //calling function must tell us if starting token is in a quote
  //calling function has already determined we are NOT inside a comment
  
  Dim lngPos As Long, lngBOL As Long // blnInToken As Boolean
  Dim strToken As String, intChar As Integer
  
  On Error GoTo ErrHandler
  
  lngPos = lngStartPos
  
  //find start of line
  If lngPos > 1 Then
    lngBOL = InStrRev(strLine, vbCr, lngPos) + 1
  Else
    lngBOL = 1
  End If

  
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
    Select Case intChar
    Case 32
      //skip spaces
      
    Case 39, 40, 41, 44, 58, 59, 63, 91, 92, 93, 94, 96, 123, 125, 126
      //   //(),:;?[\]^`{}~
      //these are ALWAYS a single character token, so we can just exit
      lngStartPos = lngPos - 1
      FindPrevToken = ChrW(intChar)
      Exit Function
    Case Else
      //start(end) of a token
      lngPos = lngPos - 1
      strToken = ChrW(intChar)
      If intChar = 34 Then
        //end of a string
        blnInQuote = true
      End If
      Exit Do
    End Select
    //keep looking
    lngPos = lngPos - 1
  Loop
  
  //now find beginning of the token
  Do While lngPos >= lngBOL
    intChar = AscW(Mid$(strLine, lngPos))
    
    //if in a quote, keep backing up until starting quote found
    If blnInQuote Then
      //add the char
      strToken = ChrW(intChar) + strToken
      //then find the end(front) of the string
      Do
        If intChar = 34 Then
          //check for embedded quote (\")
          //toggle quote flag until something
          //other than a slash is found in front
          blnInQuote = false
          If lngPos > lngBOL Then
            Do Until Asc(Mid$(strLine, lngPos - 1)) != 92
              blnInQuote = Not blnInQuote
              lngPos = lngPos - 1
              strToken = "\" + strToken
              If lngPos < lngBOL Then
                //shouldn//t be possible but just in case
                //Debug.Assert true
                blnInQuote = false
                Exit Do
              End If
            Loop
          End If
          //backup one more space
          lngPos = lngPos - 1
          //if no longer in quotes, token is found
          If Not blnInQuote Then
            lngStartPos = lngPos
            FindPrevToken = strToken
            Exit Function
          End If
        Else
          //keep going
          lngPos = lngPos - 1
        End If
        
        //get another character
        If lngPos < lngBOL Then
          //if a bad string is encountered it is possible
          // forlgPos to go past BOL
          //Debug.Assert blnInQuote
          Exit Do
        End If
        intChar = AscW(Mid$(strLine, lngPos))
        //add the char
        strToken = ChrW(intChar) + strToken
      Loop Until lngPos < lngBOL
    Else
      //if this character is a separator, we stop
      //  space, !"&//()*+,-/:;<=>?[\]^`{|}~
      Select Case intChar
      Case 32, 39, 40, 41, 44, 58, 59, 63, 91, 92, 93, 94, 96, 123, 125, 126
      //   //(),:;?[\]^`{}~
        //single character separators that ALWAYS mark end
        Exit Do
      Case 33, 38, 42, 43, 45, 47, 60, 61, 62, 124
        //   !&*+-/<=>|
        //possible multi-char separators; only treat as
        //a separator if it//s not part of a two-char token
        Select Case intChar
        Case 33 // !  allowed: !=
          If strToken != "=" Then
            //it//s a separator
            Exit Do
          End If
        Case 38  // +  allowed: &&
          If strToken != "&" Then
            //it//s a separator
            Exit Do
          End If
        Case 42  // *  allowed: *=, *text
          Select Case Asc(strToken)
          Case 35 To 37, 46, 48 To 57, 61, 64 To 90, 95, 97 To 122
            //it//s not a separator
          Case Else
            //it//s a separator
            Exit Do
          End Select
        Case 43  // +  allowed: ++, +=
          If strToken != "+" And strToken != "=" Then
            //it//s a separator
            Exit Do
          End If
        Case 45  // -  allowed: --, -=
          If strToken != "-" And strToken != "=" Then
            //it//s a separator
            Exit Do
          End If
        Case 47  // /  allowed: /=
          If strToken != "=" Then
            //it//s a separator
            Exit Do
          End If
        Case 60  // <  allowed: <=
          If strToken != "=" Then
            //it//s a separator
            Exit Do
          End If
        Case 61  // =  allowed: =>, =<, ==
          If strToken != ">" And strToken != ">" And strToken != "=" Then
            //it//s a separator
            Exit Do
          End If
        Case 62  // >  allowed: >=
          If strToken != "=" Then
            //it//s a separator
            Exit Do
          End If
        Case 124 // |  allowed: ||
          If strToken != "|" Then
            //it//s a separator
            Exit Do
          End If
        End Select
      End Select
      //add the character to front of token
      strToken = ChrW(intChar) + strToken
      lngPos = lngPos - 1
    End If
  Loop

  //return what we got
  lngStartPos = lngPos
  FindPrevToken = strToken
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub LastForm(ThisForm As Form)
  
  //checks if this is last visible form; if so, resets menus
  
  Dim tmpForm As Form
  
  On Error GoTo ErrHandler
  
  For Each tmpForm In Forms
    If Not (tmpForm Is ThisForm) Then
      If tmpForm.Name != "frmMDIMain" Then
        If tmpForm.MDIChild Then
          //another open form found
          Exit Sub
        End If
      End If
    End If
  Next
  
  //no other form found; this is last form
  AdjustMenus rtNone, false, false, false
  
  //disable windows menu (done here, because no other easy way
  //to tell if no forms are left)
  frmMDIMain.mnuWindow.Enabled = false
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub


public Function LogicCmd(strCmd As String) As Long

  //returns the command number of the string
  //if strCmd matches an AGI command
  
  Dim i As Long, j As Long, k As Long
  
  If LenB(strCmd) = 0 Then
    LogicCmd = -1
    Exit Function
  End If
  
  //index based on first letter
  Select Case Asc(strCmd)
  Case 97, 65 //a
    i = 0
    j = 7
  Case 98, 66 //b
    i = 8
    j = 8
  Case 99, 67 //c
    i = 9
    j = 20
  Case 100, 68 //d
    i = 21
    j = 34
  Case 101, 69 //e
    i = 35
    j = 39
  Case 102, 70 //f
    i = 40
    j = 43
  Case 103, 71 //g
    i = 44
    j = 53
  Case 104, 72 //h
    i = 54
    j = 54
  Case 105, 73 //i
    i = 55
    j = 60
  Case 108, 76 //l
    i = 61
    j = 72
  Case 109, 77 //m
    i = 73
    j = 77
  Case 110, 78 //n
    i = 78
    j = 82
  Case 111, 79 //o
    i = 83
    j = 92
  Case 112, 80 //p
    i = 93
    j = 102
  Case 113, 81 //q
    i = 103
    j = 103
  Case 114, 82 //r
    i = 104
    j = 115
  Case 115, 83 //s
    i = 116
    j = 153
  Case 116, 84 //t
    i = 154
    j = 156
  Case 119, 87 //w
    i = 157
    j = 158
  Case Else
    //not a command
    LogicCmd = -1
    Exit Function
  End Select
  
  For k = i To j
    If StrComp(strCmd, LoadResString(ALPHACMDTEXT + k), vbTextCompare) = 0 Then
      LogicCmd = k
      Exit Function
    End If
  Next k

  //not found
  LogicCmd = -1
End Function

public Sub RenameMRU(ByVal OldWAGFile As String, ByVal NewWAGFile As String)


  //if NewWAGFile is already in the list,
  //remove it - it will take the place of
  //OldWAGFile
  
  //if OldWAGFile is NOT on list, add NewWAGFile;
  //otherwise just rename OldWAGFile to NewWAGFile
   
  Dim i As Long, j As Long
  
  //first look for NewWAGFile, and delete it if found
  For i = 1 To 4
    If NewWAGFile = strMRU(i) Then
      //delete it by moving others up
      For j = i To 3
        strMRU(j) = strMRU(j + 1)
        frmMDIMain.Controls("mnuGMRU" + CStr(j)).Caption = CompactPath(strMRU(j), 60)
      Next j
      //now delete last entry
      strMRU(4) = ""
      frmMDIMain.mnuGMRU1.Caption = ""
      Exit For
    End If
  Next i
  
  //now check for OldWAGFile
  For i = 1 To 4
    If strMRU(i) = OldWAGFile Then
    //rename it
      strMRU(i) = NewWAGFile
      frmMDIMain.Controls("mnuGMRU" + CStr(i)).Caption = CompactPath(NewWAGFile, 60)
      Exit For
    End If
  Next i
  
  //make sure NewWAGFile is at the top (use AddToMRU function to do this easily!)
  AddToMRU NewWAGFile
  
End Sub

public Sub SafeDoEvents()
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
  
  frmMDIMain.Enabled = false
  DoEvents
  frmMDIMain.Enabled = true
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub GetDefaultColors()

  // reads default custom colors from winagi.confg
  
  Dim i As Long, strLine As String
  
  On Error Resume Next
  
  For i = 0 To 15
    //validate it//s a good number before writing it
    strLine = Trim(ReadSettingString(SettingsList, sDEFCOLORS, "DefEGAColor" + CStr(i), "0x" + PadHex(DefEGAColor(i), 8)))
    If Left(strLine, 2) != "0x" Or Len(strLine) != 10 Then
      //keep default
    Else
      //if color is not the default
      If EGAColor(i) != CLng(strLine) Then
        EGAColor(i) = CLng(strLine)
      End If
    End If
  Next i
  
End Sub

public Sub SetLogicCompiledStatus(ByVal ResNum As Byte, ByVal Compiled As Boolean)

  On Error GoTo ErrHandler
  
  // updates color of a resource list logic entry
  // to indicate compiled status
  
  Select Case Settings.ResListType
  Case 1
    frmMDIMain.tvwResources.Nodes("l" + ResNum).ForeColor = IIf(Compiled, vbBlack, vbRed)
  Case 2
    //only need to update if logics are selected
    If frmMDIMain.cmbResType.ListIndex = 1 Then
      frmMDIMain.lstResources.ListItems("l" + ResNum).ForeColor = IIf(Compiled, vbBlack, vbRed)
    End If
  End Select
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub UpdateSelection(ByVal ResType As AGIResType, ByVal ResNum As Long, ByVal UpDateMode As UpdateModeType)

  //updates the resource list, property box, and preview window for a given resource
  
  Dim tmpItem As ListItem
  
  On Error GoTo ErrHandler
  
  If ResNum < 0 Or ResNum > 255 Then
    Exit Sub
  End If
  
  Select Case Settings.ResListType
  Case 0 //no tree
    //do nothing
    
  Case 1 //treeview list
    //if updating tree OR updating a logic
    If ((UpDateMode And umResList) Or ResType = rtLogic) Then
      //update the node for this resource
      Select Case ResType
      Case rtLogic
        frmMDIMain.tvwResources.Nodes("l" + CStr(ResNum)).Text = ResourceName(Logics(ResNum), true)
        //also set compiled status
        If Not Logics(ResNum).Compiled Then
          frmMDIMain.tvwResources.Nodes("l" + CStr(ResNum)).ForeColor = vbRed
        Else
          frmMDIMain.tvwResources.Nodes("l" + CStr(ResNum)).ForeColor = vbBlack
        End If
      Case rtPicture
        frmMDIMain.tvwResources.Nodes("p" + CStr(ResNum)).Text = ResourceName(Pictures(ResNum), true)
      Case rtSound
        frmMDIMain.tvwResources.Nodes("s" + CStr(ResNum)).Text = ResourceName(Sounds(ResNum), true)
      Case rtView
        frmMDIMain.tvwResources.Nodes("v" + CStr(ResNum)).Text = ResourceName(Views(ResNum), true)
      End Select
    End If
  
  Case 2 //combo/list boxes
    //only update if current type is listed
    If frmMDIMain.cmbResType.ListIndex - 1 = ResType Then
      //if updating tree OR updating a logic (because color of
      //logic text might need updating)
      If ((UpDateMode And umResList) Or ResType = rtLogic) Then
        //update the node for this resource
        Select Case ResType
        Case rtLogic
          Set tmpItem = frmMDIMain.lstResources.ListItems("l" + CStr(ResNum))
          tmpItem.Text = ResourceName(Logics(ResNum), true)
          //also set compiled status
          If Not Logics(ResNum).Compiled Then
            tmpItem.ForeColor = vbRed
          Else
            tmpItem.ForeColor = vbBlack
          End If
        Case rtPicture
          Set tmpItem = frmMDIMain.lstResources.ListItems("p" + CStr(ResNum))
          tmpItem.Text = ResourceName(Pictures(ResNum), true)
        Case rtSound
          Set tmpItem = frmMDIMain.lstResources.ListItems("s" + CStr(ResNum))
          tmpItem.Text = ResourceName(Sounds(ResNum), true)
        Case rtView
          Set tmpItem = frmMDIMain.lstResources.ListItems("v" + CStr(ResNum))
          tmpItem.Text = ResourceName(Views(ResNum), true)
        End Select
        //expand column width if necessary
        If 1.2 * frmMDIMain.picResources.TextWidth(tmpItem.Text) > frmMDIMain.lstResources.ColumnHeaders(1).Width Then
          frmMDIMain.lstResources.ColumnHeaders(1).Width = 1.2 * frmMDIMain.picResources.TextWidth(tmpItem.Text)
        End If
      End If
    End If
  End Select
  
  //if the selected item matches the update item
  If SelResType = ResType And SelResNum = ResNum Then
    //if updating properties OR updating tree AND tree is visible
    If ((UpDateMode And umProperty) Or (UpDateMode And umResList)) And Settings.ResListType != 0 Then
      //redraw property window
      frmMDIMain.PaintPropertyWindow
    End If
    
    //if updating preview
    If (UpDateMode And umPreview) And Settings.ShowPreview Then
      //redraw the preview
      PreviewWin.LoadPreview ResType, ResNum
    ElseIf Settings.ShowPreview Then
      PreviewWin.UpdateCaption ResType, ResNum
    End If
  End If
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub SetError(ByVal lngErrLine As Long, strErrMsg As String, ByVal LogicNumber As Long, strModule As String, Optional ByVal blnWarning As Boolean = false)
  //this procedure uses errinfo to open the file
  //with the error, if possible,
  //highlights the line with the error,
  //and displays the error message in the status bar
  
  Dim i As Integer, strErrType As String
  Dim rtn As Long, lng1st As Long
  Dim lngStart As Long, lngLength As Long
  Dim lngLineCount As Long
  Dim frmTemp As Form
  Dim lngTopLine As Long, lngThisLine As Long
  Dim pPos As POINTAPI, lngBtmLine As Long, rtfRect As RECT
  
  On Error GoTo ErrHandler
  
  //if an include module filename is passed,
  If strModule != "" Then
    //module info contains name- attempt to open include file
    
    OpenTextFile strModule, true
    //set focus to this file
    For i = 1 To LogicEditors.Count
      If LogicEditors(i).FormMode = fmText Then
        //if this is the form we are looking for
        If LogicEditors(i).FileName = strModule Then
          Set frmTemp = LogicEditors(i)
          Exit For
        End If
      End If
    Next i
    
    //if text editor not found,
    If frmTemp Is Nothing Then
      //if just warning, exit
      If blnWarning Then
        Exit Sub
      End If
      
      //set error line to zero
      lngErrLine = 0
      //set error msg to indicate include file not opened
      strErrMsg = strErrMsg + " (in INCLUDE file)"
    End If
  End If
  
  //if no editor assigned yet,
  If frmTemp Is Nothing Then
    //check if logic is already open
    For i = 1 To LogicEditors.Count
      If LogicEditors(i).FormMode = fmLogic Then
        If LogicEditors(i).LogicNumber = LogicNumber Then
          //this is it
          Set frmTemp = LogicEditors(i)
          Exit For
        End If
      End If
    Next i
    
    //if not found
    If frmTemp Is Nothing Then
      //open a new one, if possible
      OpenLogic LogicNumber, true
      
      //now try to find the newly opened editor
       For i = 1 To LogicEditors.Count
        If LogicEditors(i).FormMode = fmLogic Then
          If LogicEditors(i).LogicNumber = LogicNumber Then
            //this is it
            Set frmTemp = LogicEditors(i)
            Exit For
          End If
        End If
      Next i
    End If
  End If
  
  //if still nothing
  If frmTemp Is Nothing Then
    //if just a warning
    If blnWarning Then
      Exit Sub
    End If
    
    //try a message box instead
    MsgBox "ERROR in line " + CStr(lngErrLine + 1) + ": " + strErrMsg, vbOKOnly + vbCritical, "Compile Logic Error"
    Exit Sub
  End If
  
  If frmTemp.Enabled Then
    //set focus to this editor NOT WORKING!!!
    frmTemp.SetFocus
  Else
    frmTemp.ZOrder
  End If
  
  With frmTemp.rtfLogic
    //get number of lines in this editor
    lngLineCount = SendMessage(.hWnd, EM_GETLINECOUNT, 0, 0)
    //get character position of first error line
    lngStart = SendMessage(.hWnd, EM_LINEINDEX, lngErrLine - 1, 0)
    .Selection.Range.StartPos = lngStart
    //if past end of input
    If lngErrLine >= lngLineCount Then
      //set length to select to end of input
      lngLength = Len(.Text) - lngStart
      .Selection.Range.EndPos = .Selection.Range.StartPos + lngLength
    Else
      //get character position of line immediately below last error line
      //(move cursor back one character to put it at end of last error line)
      lngLength = SendMessage(.hWnd, EM_LINEINDEX, lngErrLine, 0) - lngStart - 1
      .Selection.Range.EndPos = .Selection.Range.StartPos + lngLength
    End If
    Do
      //determine top, bottom and current line numbers
      lngTopLine = SendMessage(.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
      lngThisLine = lngErrLine
      //find bottom line in steps
      
//////    On Error Resume Next
//////      .GetViewRect i, rtn, pPos.X, pPos.Y
//////      If Err.Number != 0 Then Exit Do
      //GetViewRect gives an error here; says sub not defined;
      //makes no sense, because it works just fine in FindInLogics
      
      //use api calls instead
      rtn = SendMessageRctL(.hWnd, EM_GETRECT, 0, rtfRect)
      pPos.X = rtfRect.Right
      pPos.Y = rtfRect.Bottom
      rtn = SendMessagePtL(.hWnd, EM_CHARFROMPOS, 0, pPos)
      lngBtmLine = SendMessage(.hWnd, EM_LINEFROMCHAR, rtn, 0)
      // if line NOT more than four above bottom, scroll up
      If lngBtmLine - lngThisLine < 4 Then
        //scroll so this line is four lines above bottom
        //get currently visible first line
        //determine amount of scrolling to do
        rtn = SendMessage(.hWnd, EM_LINESCROLL, 0, 4 - (lngBtmLine - lngThisLine))
      End If
    Loop Until true
    //set focus to the text editor
    .SetFocus
    //refresh the screen
    .Refresh
  End With
  
  //if not a warning
  If Not blnWarning Then
    strErrType = "ERROR "
  Else
    strErrType = "WARNING "
  End If
  
  //set status bar
  SettingError = true
  frmTemp.Tag = strErrType + "in line " + CStr(lngErrLine) + ": " + strErrMsg
  MainStatusBar.Panels(1).Text = frmTemp.Tag
  
Exit Sub

ErrHandler:
  If Err.Number != 70 Then
    //Debug.Assert false
  End If
  Resume Next
End Sub

public Function FindNextToken(strText As String, ByRef lngPos As Long, Optional ByRef strFindCmd As String = "", Optional ByVal UpdatePos As Boolean = true, Optional ByVal ValidCmds As Boolean = false) As Long
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
  
  Dim intChar As Integer, blnContinue As Boolean
  Dim blnEmbed As Boolean, blnInQuotes As Boolean
  Dim strToken As String
  Dim lngLen As Long, blnNextToken As Boolean
  Dim lngTknPos As Long
  
  //use local copy of position
  lngTknPos = lngPos
  
  //if no search string passed, return next cmd
  blnNextToken = (LenB(strFindCmd) = 0)
  
  //quick check for the search string;
  //if it doesn//t even exist in the text,
  //don't bother doing a search
  If Not blnNextToken Then
    If InStr(lngTknPos + 1, strText, strFindCmd, vbTextCompare) = 0 Then
      //not found
      If UpdatePos Then
        lngPos = 0
      End If
      Exit Function
    End If
  End If
  
  //convert string length into variable for speed
  lngLen = Len(strText)
  
  //step through cmds until a match is found
  Do
    //find next non-blank character
    intChar = NextChar(strText, "", lngTknPos)
    //if at end of input,
    If lngTknPos = 0 Then
      //not found; just exit
      If UpdatePos Then
        lngPos = 0
      End If
      FindNextToken = 0
      Exit Function
    End If
    
    On Error GoTo ErrHandler
    
    //assume this position begins a token
    FindNextToken = lngTknPos
    
    //extract the token at this point
    
    //if character is a special character
    Select Case intChar
    Case 34 //"
      //special case; quote means start of a string
      blnInQuotes = true
    
    Case 47 ///
      //special case; "//" marks beginning of comment
      //if next char is a "/"
      If Mid$(strText, lngTknPos + 1, 1) = "/" Then
        //move to end of line
        lngTknPos = InStr(lngTknPos, strText, vbCr) - 1
        If lngTknPos = -1 Then
          lngTknPos = lngLen
        End If
      End If
    
    Case 91 //[
      //special case; "[" marks beginning of comment
      //move to end of line
      lngTknPos = InStr(lngTknPos, strText, vbCr) - 1
      If lngTknPos = -1 Then
        lngTknPos = lngLen
      End If
      
    End Select
    
    //if a string was found (inquotes is true)
    If blnInQuotes Then
      //process a string
      
      //add characters until another true quote is found, a cr is found, or EOL is reached
      Do
        //increment position
        lngTknPos = lngTknPos + 1
        //check for end of line
        If lngTknPos > lngLen Then
          //end  reached
          lngTknPos = lngLen
          //not in a quote anymore
          blnInQuotes = false
          //exit
          Exit Do
        End If
          
        //get next character
        intChar = AscW(Mid$(strText, lngTknPos, 1))
        
        Select Case intChar
        Case 13  //end of line
          //readjust lngpos
          lngTknPos = lngTknPos - 1
          //not in a quote anymore
          blnInQuotes = false
          Exit Do
        Case 34 //if the next character is a quotation mark,
          If Not blnEmbed Then
            //reset quote flag
            blnInQuotes = false
          End If
        End Select
        
        //if this character is slash,
        //next character could be an embedded quote
        blnEmbed = (intChar = 92)
      Loop While blnInQuotes
      
    ElseIf Not blnContinue Then
      //single character cmds need to be returned as individual cmds
      Select Case intChar
      Case 13, 39, 40, 41, 44, 58, 59, 63, 91, 92, 93, 94, 96, 123, 125, 126 //  //(),:;?[\]^`{}~
        //single character tokens
      
      Case 61 //=
        //special case; "=", "=<" and "=>" returned as separate tokens
        Select Case Mid$(strText, lngTknPos + 1, 1)
        Case "<", ">"
          //increment pointer
          lngTknPos = lngTknPos + 1
        
        Case "=" //"=="
          //increment pointer
          lngTknPos = lngTknPos + 1
        
        End Select
        
      Case 43 //+
        //special case; "+", "++" and "+=" returned as separate tokens
        If Mid$(strText, lngTknPos + 1, 1) = "+" Or Mid$(strText, lngTknPos + 1, 1) = "=" Then
          //increment pointer
          lngTknPos = lngTknPos + 1
        End If
    
      Case 45 //-
        //special case; "-", "--" and "-=" returned as separate tokens
        If Mid$(strText, lngTknPos + 1, 1) = "-" Or Mid$(strText, lngTknPos + 1, 1) = "=" Then
          //increment pointer
          lngTknPos = lngTknPos + 1
        End If
        
      Case 33 //!
        //special case; "!" and "!=" returned as separate tokens
        If Mid$(strText, lngTknPos + 1, 1) = "=" Then
          //increment pointer
          lngTknPos = lngTknPos + 1
        End If
        
      Case 60 //<
        //special case; "<", "<=" and "<>" returned as separate tokens
        If Mid$(strText, lngTknPos + 1, 1) = "=" Or Mid$(strText, lngTknPos + 1, 1) = ">" Then
          //increment pointer
          lngTknPos = lngTknPos + 1
        End If
    
      Case 62 //>
        //special case; ">", ">=" and "><" returned as separate tokens
        If Mid$(strText, lngTknPos + 1, 1) = "=" Or Mid$(strText, lngTknPos + 1, 1) = "<" Then
          //increment pointer
          lngTknPos = lngTknPos + 1
        End If
    
      Case 42 //*
        //special case; "*" and "*=" returned as separate tokens;
        If Mid$(strText, lngTknPos + 1, 1) = "=" Then
          //increment pointer
          lngTknPos = lngTknPos + 1
        End If
    
      Case 47 ///
        //special case; "/", "/*" and "/=" returned as separate tokens
        If Mid$(strText, lngTknPos + 1, 1) = "=" Then
          lngTknPos = lngTknPos + 1
        End If
        If Mid$(strText, lngTknPos + 1, 1) = "*" Then
          lngTknPos = lngTknPos + 1
        End If
    
      Case 124 //|
        //special case; "|" and "||" returned as separate tokens
        If Mid$(strText, lngTknPos + 1, 1) = "|" Then
          //increment pointer
          lngTknPos = lngTknPos + 1
        End If
    
      Case 38 //&
        //special case; "&" and "&&" returned as separate tokens
        If Mid$(strText, lngTknPos + 1, 1) = "&" Then
          //increment pointer
          lngTknPos = lngTknPos + 1
        End If
        
      Case Else
        //continue adding characters until element separator or EOL is reached
        Do
          //increment position
          lngTknPos = lngTknPos + 1
          //check for end of line
          If lngTknPos > lngLen Then
            //end  reached
            lngTknPos = lngLen
            //exit
            Exit Do
          End If
            
          //get next character
          intChar = Asc(Mid$(strText, lngTknPos, 1))
          
          Select Case intChar
          Case 13, 32, 33, 34, 38 To 45, 47, 58 To 63, 91 To 94, 96, 123 To 126
            //  space, !"&//()*+,-/:;<=>?[\]^`{|}~
            //end of token text found;
            //readjust lngpos
            lngTknPos = lngTknPos - 1
            Exit Do
          End Select
        Loop While true
      End Select
    End If
    
    //if no continuation
    If Not blnContinue Then
      //get the cmd for comparison
      strToken = Mid$(strText, FindNextToken, lngTknPos - FindNextToken + 1)
      
      //if not searching for a specific cmd
      If blnNextToken Then
        //if only valid cmds
        If ValidCmds Then
          //skip comments and cr
          Select Case Left$(strToken, 2)
          Case "[", "//", vbCr
          Case Else
            //return the cmd that was found
            strFindCmd = strToken
            If UpdatePos Then
              lngPos = lngTknPos
            End If
            Exit Function
          End Select
        Else
          //return the cmd that was found
          strFindCmd = strToken
          If UpdatePos Then
            lngPos = lngTknPos
          End If
          Exit Function
        End If
      End If
      
      //if searching for a LE marker,
      If StrComp(strFindCmd, "##LE", vbTextCompare) = 0 Then
        //if the comment is a layout editor exit marker
        If lngTknPos - FindNextToken = 8 Then
          strToken = Mid$(strText, FindNextToken, 4)
          If strToken = "##LE" Then
            //found
            If UpdatePos Then
              lngPos = lngTknPos
            End If
            Exit Function
          End If
        End If
      End If
      
      //does this cmd match the search?
      If LenB(strToken) = LenB(strFindCmd) Then
        If StrComp(strToken, strFindCmd, vbTextCompare) = 0 Then
          //this is it
          If UpdatePos Then
            lngPos = lngTknPos
          End If
          Exit Function
        End If
      End If
    Else
      //reset flag
      blnContinue = false
    End If
  Loop While true
  
  //loop should never be exited normally
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Function CheckSnippet(ByRef SnipText As String, ByVal IndentAmt As Long) As Boolean

  //check all snippets; if a matchis found,
  //replace SnipText and return true
  
  Dim i As Long, j As Long
  Dim strSnipName As String, strSnipValue As String
  Dim strArgs() As String, lngArgCount As Long, lngPos As Long
  Dim strNext As String
  Dim blnSnipOK As Boolean, blnArgNext As Boolean
  
  On Error GoTo ErrHandler
  
  //if no snippets
  If UBound(CodeSnippets()) = 0 Then
    //just exit
    CheckSnippet = false
    Exit Function
  End If
  
  //extract name and arguments (if any) from snippet text
  FindNextToken SnipText, lngPos, strSnipName, true
  
  //if there are arguments, lngPos will not be
  //equal to length of snippet text and next cmd will be "("
  If lngPos < Len(SnipText) Then
    //if next cmd is "("
    FindNextToken SnipText, lngPos, strNext, true
    If strNext = "(" Then
      //now extract arguments, one after another
      blnArgNext = true
      Do
        strNext = ""
        FindNextToken SnipText, lngPos, strNext, true
        //if expecting an argument
        If blnArgNext Then
          Select Case strNext
          Case ")"
            //if it//s closing parentheses AND at least one arg added
            If lngArgCount > 0 Then
              //last arg value missing; assume empty string
              lngArgCount = lngArgCount + 1
              ReDim Preserve strArgs(lngArgCount - 1)
              strArgs(lngArgCount - 1) = ""
            End If
            //make sure it//s end of line
            blnSnipOK = (lngPos >= Len(SnipText))
            Exit Do
            
          // if it//s another comma
          Case ","
            //assume empty string argument
            lngArgCount = lngArgCount + 1
            ReDim Preserve strArgs(lngArgCount - 1)
            strArgs(lngArgCount - 1) = ""
            //still expecting an argument so don't
            //change value of blnNextArg

          Case Else
            //otherwise, add it as arg (anything goes!)
            lngArgCount = lngArgCount + 1
            ReDim Preserve strArgs(lngArgCount - 1)
            strArgs(lngArgCount - 1) = strNext
            //now expecting a comma or closing bracket
            blnArgNext = false
          End Select
          
        Else
          //next arg must be a comma or closing parenthesis
          //if it//s closing parenthesis
          If strNext = ")" Then
            //make sure it//s end of line
            blnSnipOK = (lngPos >= Len(SnipText))
            Exit Do
          End If
          //otherise, if not a comma
          If strNext != "," Then
            //no good
            Exit Do
          End If
          //after comma, argument is next
          blnArgNext = true
        End If
      Loop Until lngPos >= Len(SnipText)
    End If
  Else
    //sniptext has no arguments, and is valid for checking
    blnSnipOK = true
  End If
  
  //if not ok, just exit
  If Not blnSnipOK Then
    CheckSnippet = false
    Exit Function
  End If
  
  For i = 1 To UBound(CodeSnippets())
    //if name matches
    If CodeSnippets(i).Name = strSnipName And CodeSnippets(i).Type != -1 Then
      //get value
      strSnipValue = CodeSnippets(i).Value
      
      //if any arguments, replace them
      If lngArgCount > 0 Then
        For j = 0 To lngArgCount - 1
          strSnipValue = Replace(strSnipValue, "%" + CStr(j + 1), strArgs(j))
        Next j
      End If
      
      //if more than one line
      If InStr(1, strSnipValue, vbCr) > 0 Then
        //insert tabs
        strSnipValue = Replace(strSnipValue, vbCr, vbCr + Space(IndentAmt))
      End If
      //shouldn//t be any line feeds, but check, just in case
      If InStr(1, strSnipValue, vbLf) > 0 Then
        //Debug.Assert false
        strSnipValue = Replace(strSnipValue, vbLf, "")
      End If
      
      SnipText = strSnipValue
      CheckSnippet = true
      Exit Function
    End If
  Next i
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function


Private Function NextChar(strText As String, strCmd As String, ByRef lngPos As Long) As Long
  //returns the ascii code for, and sets the position of the next
  //non-space character (tabs, and lf are considered //spaces//; a cr is not)
  
  Dim lngLen As Long
  
  On Error GoTo ErrHandler
  
  //if already at end of input
  lngLen = Len(strText)
  If lngPos = lngLen Then
    //exit
    NextChar = 0
    lngPos = 0
    Exit Function
  End If
  
  Do
    //first, increment position
    lngPos = lngPos + 1
    
    NextChar = AscW(Mid$(strText, lngPos, 1))
    Select Case NextChar
    Case 9, 10, 32
      //get next character
    Case Else
      //this is a non-space character
      Exit Function
    End Select
    
  //keep going, until end of input is reached
  Loop Until lngPos = lngLen
  
  //end reached; reset character and position
  NextChar = 0
  lngPos = 0
Exit Function

ErrHandler:
  Resume Next
End Function
public Function ConcatenateString(ByRef StringIn As String) As Boolean

  //verify that the passed string is a valid string, including possible concatenation
  //string must be of the format "text" or "text1" "text2"
  
  Dim blnInQuotes As Boolean, blnEmbed As Boolean
  Dim blnAmp As Boolean
  Dim lngPos As Long, intChar As Integer
  Dim strIn As String, strOut As String
  
  //start with input string
  strIn = Trim$(StringIn)
  
  //if nothing,
  If Len(strIn) < 2 Then
    //not valid
    ConcatenateString = false
    Exit Function
  End If
  
  //starting char should be quote
  If Asc(strIn) != 34 Then
    //not valid
    ConcatenateString = false
    Exit Function
  End If
  
  //begin check
  blnInQuotes = true
  lngPos = 2
  Do
    intChar = Asc(Mid$(strIn, lngPos, 1))
    
    //if not in quotes,
    If Not blnInQuotes Then
      //only spaces or another quote is allowed
      Select Case intChar
      Case 34
        //toggle flag
        blnInQuotes = true
      Case 32
        //ignore blanks
      Case Else
        //error
        Exit Function
      End Select
          
    Else
      //if a quote, need to ensure it is a real quote
      If intChar = 34 And Not blnEmbed Then
        //no longer in quotes
        blnInQuotes = false
        blnEmbed = false
      Else
        //add the character
        strOut = strOut + ChrW$(intChar)
      End If
      
      //check for embed code
      blnEmbed = (intChar = 92)
    End If
    
    lngPos = lngPos + 1
  Loop Until lngPos > Len(StringIn)
  
  //should end up being NOT in quotes and NOT expecting ampersand
  If blnInQuotes Or blnAmp Then
    //error
    Exit Function
  End If
  
  //return the concatenated string, with surrounding quotes
  StringIn = QUOTECHAR + strOut + QUOTECHAR
  ConcatenateString = true
Exit Function

ErrHandler:
  Resume Next
End Function


public Function ExtractExits(ThisLogic As AGILogic) As AGIExits
  
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
  
  Dim blnLogLoad As Boolean
  Dim strLogic As String
  Dim RoomExits As AGIExits, lngCmdLoc As Long
  Dim i As Long, j As Long
  Dim strLine As String, lngID As Long
  Dim blnIDOK As Boolean, blnSave As Boolean
  Dim tmpExit As AGIExit, lngLastPos As Long
  Dim tmpStatus As EEStatus
  
  //lngCmdLoc is location of 1st character of //new.room// command
  
  On Error GoTo ErrHandler
  
  //ensure source is loaded
  blnLogLoad = ThisLogic.Loaded
  If Not blnLogLoad Then
    ThisLogic.Load
  End If
  
  //get source code
  strLogic = ThisLogic.SourceText
  
  //create new room object
  Set RoomExits = New AGIExits
  
  //locate first instance of new.room command
  lngCmdLoc = FindNextToken(strLogic, 0, "new.room")
  
  //loop through exit extraction until all new.room commands are processed
  Do Until lngCmdLoc = 0
    //get exit info
    Set tmpExit = AnalyzeExit(strLogic, lngCmdLoc, lngLastPos)
    
    //find end of line by searching for crlf
    j = InStr(lngCmdLoc, strLogic, vbCr)
    //if no line end found, means we are on last line; set
    //j to a value of lngCmdLoc+1 so we get the last char of the line
    If j = 0 Then j = lngCmdLoc + 1
    //get rest of line (to check against existing exits)
    strLine = Trim$(Mid$(strLogic, lngCmdLoc, j - lngCmdLoc))
    
    //check line for a ##LE marker:
    //  first, strip off comment marker
    If Left$(strLine, 1) = "[" Then
      strLine = Trim$(Right$(strLine, Len(strLine) - 1))
    ElseIf Left$(strLine, 2) = "//" Then
      strLine = Trim$(Right$(strLine, Len(strLine) - 2))
    End If
    //  next, look for ##LE tag
    If Left$(strLine, 4) = "##LE" Then
      //strip off leader to expose exit id number
      strLine = Right$(strLine, Len(strLine) - 4)
      //note we can ignore the trailing //##// marker; Val function doesn//t care if they are there
    Else
      //not an exit marker; reset the string
      strLine = ""
    End If
    
    //if a valid id Value was found
    If LenB(strLine) != 0 Then
      //an id may exist
      //assum ok until proven otherwise
      blnIDOK = true
      //get the id number
      lngID = CLng(Val(strLine))
      //if not a number (val=0) then no marker
      If lngID = 0 Then
        blnIDOK = false
      Else
        //check for this marker among current exits
        For i = 0 To RoomExits.Count - 1
          If Val(Right$(RoomExits(i).ID, 3)) = CStr(lngID) Then
            //this ID has already been added by the editor;
            //it needs to be reset
            blnIDOK = false
            Exit For
          End If
        Next i
      End If
    Else
      //no previous marker; assign id automatically
      blnIDOK = false
    End If
      
    //if previous ID needs updating (or one not found)
    If Not blnIDOK Then
      //get next available id number
      lngID = 0
      i = 0
      Do
        lngID = lngID + 1
        For i = 0 To RoomExits.Count - 1
          If CLng(Right$(RoomExits(i).ID, 3)) = lngID Then
            Exit For
          End If
        Next i
      Loop Until i = RoomExits.Count
    End If
    
    //exit is ok
    tmpStatus = esOK
    
    //add exit to logic, and flag as in game and ok
    RoomExits.Add(lngID, tmpExit.Room, tmpExit.Reason, tmpExit.Style).Status = tmpStatus
    
    //if id is new or changed,
    If Not blnIDOK Then
      //lngCmdLoc marks end of commands on this line
      
      //find end of line with //new.room// command
      j = InStr(lngCmdLoc, strLogic, ChrW$(13))
      //use end of logic, if on last line (i=0)
      If j = 0 Then j = Len(strLogic) + 1
      
      //insert exit info into logic source
      strLogic = Left$(strLogic, lngCmdLoc - 1) + " [ ##LE" + format$(lngID, "000") + "##" + Right$(strLogic, Len(strLogic) - j + 1)
      
      //set save flag
      blnSave = true
    End If
    
    //get next new.room cmd
    lngLastPos = lngCmdLoc
    lngCmdLoc = FindNextToken(strLogic, lngCmdLoc, "new.room")
  Loop
  
  //if changes made to exit ids
  If blnSave Then
    //replace sourcecode
    ThisLogic.SourceText = strLogic
    //remember that logics that are being edited appear to be
    //not in a game, which causes the ID to be changed to match the filename
    //since only ingame logics can be rebuilt, need to check for ID change,
    //and change it back
    
    //WTF is this about logic not being in game? I should be working with
    //the actual ingame logic, right?  OH, I remember - if this is called from
    //a logic editor, it//s the copy of the object, not the object; at least
    //it was; I think it//s now the actual game logic; need to check that
    
    //Debug.Assert ThisLogic Is Logics(ThisLogic.Number)
    //Debug.Assert ThisLogic.Resource.InGame
    
    If Not ThisLogic.Resource.InGame Then
      //save id
      strLogic = ThisLogic.ID
    End If
    ThisLogic.SaveSource
    
    If Not ThisLogic.Resource.InGame Then
      ThisLogic.ID = strLogic
    End If
    
    //now need to make sure tree is uptodate (and preview window, if this
    //logic happens to be the one being previewed)
    UpdateSelection rtLogic, ThisLogic.Number, umPreview Or umProperty
  End If
  
  //unload OR load if necessary
  If Not blnLogLoad And ThisLogic.Loaded Then
    ThisLogic.Unload
  ElseIf blnLogLoad And Not ThisLogic.Loaded Then
    ThisLogic.Load
  End If
  
  //return the new exit list
  Set ExtractExits = RoomExits
Exit Function

ErrHandler:
  Resume Next
End Function


public Function ReplaceGlobal(TextIn As String) As String

  //open the global file; check for existence of this item;
  //if found, replace it with correct Value;
  //if not found, return original Value
  
  Dim intFile As Integer
  Dim strLine As String, strSplitLine() As String
  
  //default to original Value
  ReplaceGlobal = TextIn
  
  //check for existence of globals file
  If Not FileExists(GameDir + "globals.txt") Then
    //no global list
    Exit Function
  End If
  
  //open the list
  On Error GoTo ErrHandler
  
  intFile = FreeFile
  
  Open GameDir + "globals.txt" For Input As intFile
  
  Do Until EOF(intFile)
    Line Input #intFile, strLine
    //trim it
    strLine = Trim$(strLine)
      
    //ignore blanks and comments (// or [)
    If LenB(strLine) != 0 And Left$(strLine, 2) != "//" And Left$(strLine, 1) != ChrW$(91) Then
      //splitline into name and Value
      strSplitLine = Split(strLine, vbTab)
      
      //if exactly two elements,
      If UBound(strSplitLine) = 1 Then
        //check it against input
        If TextIn = Trim$(strSplitLine(0)) Then
          ReplaceGlobal = Trim$(strSplitLine(1))
          Close intFile
          Exit Function
        End If
      End If
    End If
 Loop
 
  //not found; exit
  Close intFile
 
Exit Function

ErrHandler:
  //just clear error and return input text
  Err.Clear
End Function

public Sub ExportAll()

  //exports all logic, picture, sound and view resources into the resource directory
  //overwriting where necessary
  
  Dim tmpLog As AGILogic, tmpPic As AGIPicture
  Dim tmpSnd As AGISound, tmpView As AGIView
  Dim blnLoaded As Boolean
  Dim blnRepeat As Boolean, rtn As VbMsgBoxResult
  
  On Error GoTo ErrHandler
  
  //disable main form while exporting
  frmMDIMain.Enabled = false
  //show wait cursor
  WaitCursor
  
  //use progress form to track progress
  Load frmProgress
  With frmProgress
    .Caption = "Exporting All Resources"
    .pgbStatus.Max = Logics.Count + Pictures.Count + Sounds.Count + Views.Count
    .pgbStatus.Value = 0
    .lblProgress = "Exporting..."
    .Show
    .Refresh
    
    For Each tmpLog In Logics
      .lblProgress.Caption = "Exporting " + tmpLog.ID
      .pgbStatus.Value = .pgbStatus.Value + 1
      .Refresh
      SafeDoEvents
      
      blnLoaded = tmpLog.Loaded
      If Not blnLoaded Then
        tmpLog.Load
      End If
      
      //if sourcefile already exists,
      If FileExists(tmpLog.SourceFile) Then
        //file exists; ask if it should be replaced
        If Not blnRepeat Then
          Screen.MousePointer = vbDefault
          //ask if should be replaced?
          rtn = MsgBoxEx("A source code file for " + tmpLog.ID + " already exists." + vbNewLine + "Do you want to replace it with the decompiled logic source?", vbQuestion + vbYesNo, "Export Logic", , , "Repeat this answer for all remaining logic resources", blnRepeat)
          Screen.MousePointer = vbHourglass
        End If
        
        //if replacing (rtn = vbYes)
        If rtn = vbYes Then
          //kill existing file
          Kill tmpLog.SourceFile
          //now save it
          tmpLog.SaveSource
        End If
      Else
        //export source
        tmpLog.SaveSource
      End If
      
      //now export the compiled logic
      tmpLog.Export ResDir + tmpLog.ID + ".agl"
      
      If Not blnLoaded Then
        tmpLog.Unload
      End If
    Next
    
    For Each tmpPic In Pictures
      .lblProgress.Caption = "Exporting " + tmpPic.ID
      .pgbStatus.Value = .pgbStatus.Value + 1
      .Refresh
      SafeDoEvents
      
      blnLoaded = tmpPic.Loaded
      If Not blnLoaded Then
        tmpPic.Load
      End If
      tmpPic.Export ResDir + tmpPic.ID + ".agp"
      If Not blnLoaded Then
        tmpPic.Unload
      End If
    Next
    
    For Each tmpSnd In Sounds
      .lblProgress.Caption = "Exporting " + tmpSnd.ID
      .pgbStatus.Value = .pgbStatus.Value + 1
      .Refresh
      SafeDoEvents
      
      blnLoaded = tmpSnd.Loaded
      If Not blnLoaded Then
        tmpSnd.Load
      End If
      tmpSnd.Export ResDir + tmpSnd.ID + ".ags"
      If Not blnLoaded Then
        tmpSnd.Unload
      End If
    Next
    
    For Each tmpView In Views
      .lblProgress.Caption = "Exporting " + tmpView.ID
      .pgbStatus.Value = .pgbStatus.Value + 1
      .Refresh
      SafeDoEvents
      
      blnLoaded = tmpView.Loaded
      If Not blnLoaded Then
        tmpView.Load
      End If
      tmpView.Export ResDir + tmpView.ID + ".agv"
      If Not blnLoaded Then
        tmpView.Unload
      End If
    Next
  End With
  
  //done with progress form
  Unload frmProgress
  
  //reenable main form
  frmMDIMain.Enabled = true
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //ignore the system bitmap error for pictures, since it doesn//t affect exporting
  If Err.Number = vbObjectError + 610 Then
    Err.Clear
    Resume Next
  End If
  
  //Debug.Assert false
  Resume Next
End Sub

public Function ExportObjects(ThisObjectsFile As AGIInventoryObjects, ByVal InGame As Boolean) As Boolean

  Dim strFileName As String
  Dim rtn As VbMsgBoxResult
  
  On Error GoTo ErrHandler
  
  //set up commondialog
  With MainSaveDlg
    If InGame Then
      .DialogTitle = "Export Inventory Objects File"
    Else
      .DialogTitle = "Save Inventory Objects File As"
    End If
    //if objectlist has a filename
    If LenB(ThisObjectsFile.ResFile) != 0 Then
      //use it
      .FullName = ThisObjectsFile.ResFile
    Else
      //use default name
      .FullName = ResDir + "OBJECT"
    End If
    .Filter = "WinAGI Inventory Objects Files (*.ago)|*.ago|OBJECT file|OBJECT|All files (*.*)|*.*"
    If LCase$(Right$(.FullName, 4)) = ".ago" Then
      .FilterIndex = 1
    Else
      .FilterIndex = 2
    End If
    .DefaultExt = ""
    .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
    .hWndOwner = frmMDIMain.hWnd
  End With
  
  Do
    On Error Resume Next
    MainSaveDlg.ShowSaveAs
    //if canceled,
    If Err.Number = cdlCancel Then
      //exit without doing anything
      Exit Function
    End If
    On Error GoTo ErrHandler
    
    //get filename
    strFileName = MainSaveDlg.FullName
    
    //if file exists,
    If FileExists(strFileName) Then
      //verify replacement
      rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
      
      If rtn = vbYes Then
        Exit Do
      ElseIf rtn = vbCancel Then
        Exit Function
      End If
    Else
      Exit Do
    End If
  Loop While true
  
  //show wait cursor
  WaitCursor
  
  If LCase$(Right$(MainSaveDlg.FullName, 4)) = ".ago" Then
    //export with new filename
    ThisObjectsFile.Export strFileName, 1, Not InGame
  Else
    //export with new filename
    ThisObjectsFile.Export strFileName, 0, Not InGame
  End If

  //reset mouse pointer
  Screen.MousePointer = vbDefault
  
  //if error,
  If Err.Number != 0 Then
    ErrMsgBox "An error occurred while exporting this file:", "", "Export File Error"
  End If
  
  ExportObjects = true
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function
public Function MakeLoopGif(GifLoop As AGILoop, GifOps As GifOptions, ByVal ExportFile As String) As Boolean

  Dim strTempFile As String, intFile As Integer
  Dim bytData() As Byte, lngPos As Long //data that will be written to the gif file
  Dim lngInPos As Long, bytCmpData() As Byte, bytCelData() As Byte //data used to build then compress cel data as gif Image
  Dim i As Integer, j As Integer
  Dim MaxH As Integer, MaxW As Integer
  Dim hVal As Byte, wVal As Byte
  Dim hPad As Byte, wPad As Byte
  Dim pX As Byte, pY As Byte
  Dim zFacH As Integer, zFacW As Integer
  Dim lngCelPos As Long, bytTrans As Byte
  Dim celH As Byte, celW As Byte
  Dim intChunkSize As Integer
  
  //use error handler to expand size of data array when it gets full
  On Error GoTo ErrHandler
  
  //build header
  ReDim bytData(255)
  bytData(0) = 71
  bytData(1) = 73
  bytData(2) = 70
  bytData(3) = 56
  bytData(4) = 57
  bytData(5) = 97
  
  //determine size of logical screen by checking size of each cel in loop, and using Max of h/w
  For i = 0 To GifLoop.Cels.Count - 1
    If GifLoop.Cels(i).Height > MaxH Then
      MaxH = GifLoop.Cels(i).Height
    End If
    If GifLoop.Cels(i).Width > MaxW Then
      MaxW = GifLoop.Cels(i).Width
    End If
  Next i
  
  //add logical screen size info
  bytData(6) = CByte((MaxW * GifOps.Zoom * 2) And 0xFF)
  bytData(7) = CByte((MaxW * GifOps.Zoom * 2) / 0x100)
  bytData(8) = CByte((MaxH * GifOps.Zoom) And 0xFF)
  bytData(9) = CByte((MaxH * GifOps.Zoom) / 0x100)
  //add color info
  bytData(10) = 243 //1-111-0-011 means:
                    //global color table,
                    //8 bits per channel,
                    //no sorting, and
                    //16 colors in the table
  //background color:
  bytData(11) = 0
  //pixel aspect ratio:
  bytData(12) = 0 //113  //should give proper 2:1 ratio for pixels
  
  //add global color table
  For i = 0 To 15
    bytData(13 + 3 * i) = EGAColor(i) And 0xFF
    bytData(14 + 3 * i) = (EGAColor(i) And 0xFF00) / 0x100
    bytData(15 + 3 * i) = (EGAColor(i) And 0xFF0000) / 0x10000
  Next i
  
  //if cycling, add netscape extension to allow continuous looping
  If GifOps.Cycle Then
    //byte   1       : 33 (hex 0x21) GIF Extension code
    //byte   2       : 255 (hex 0xFF) Application Extension Label
    //byte   3       : 11 (hex 0x0B) Length of Application Block
    //                 (eleven bytes of data to follow)
    //bytes  4 to 11 : "NETSCAPE"
    //bytes 12 to 14 : "2.0"
    //byte  15       : 3 (hex 0x03) Length of Data Sub-Block
    //                 (three bytes of data to follow)
    //byte  16       : 1 (hex 0x01)
    //bytes 17 to 18 : 0 to 65535, an unsigned integer in
    //                 lo-hi byte format. This indicate the
    //                 number of iterations the loop should
    //                 be executed.
    //byte  19       : 0 (hex 0x00) a Data Sub-Block Terminator.
  
    bytData(61) = 0x21
    bytData(62) = 0xFF
    bytData(63) = 0xB
    For i = 1 To 11
      bytData(i + 63) = Asc(Mid("NETSCAPE2.0", i, 1))
    Next i
    bytData(75) = 3
    bytData(76) = 1
    bytData(77) = 0 //255
    bytData(78) = 0 //255
    bytData(79) = 0
    
    //at this point, numbering is not absolute, so we need to begin tracking the data position
    lngPos = 80
  Else
    //at this point, numbering is not absolute, so we need to begin tracking the data position
    lngPos = 61
  End If
  
  //cel size is set to logical screen size
  //(if cell is smaller than logical screen size, it will be padded with transparent cells)
  ReDim bytCelData(MaxH * MaxW * GifOps.Zoom ^ 2 * 2 - 1)
  
  //add each cel
  For i = 0 To GifLoop.Cels.Count - 1
    //add graphic control extension for this cel
    bytData(lngPos) = 0x21
        lngPos = lngPos + 1
    bytData(lngPos) = 0xF9
        lngPos = lngPos + 1
    bytData(lngPos) = 4
        lngPos = lngPos + 1
    bytData(lngPos) = (GifOps.Transparency And 1) Or 12  //000-011-0-x = reserved-restore-no user input-transparency included
        lngPos = lngPos + 1
    bytData(lngPos) = GifOps.Delay And 0xFF
        lngPos = lngPos + 1
    bytData(lngPos) = (GifOps.Delay And 0xFF) / 0x100
        lngPos = lngPos + 1
    If GifOps.Transparency Then
      bytData(lngPos) = CByte(GifLoop.Cels(i).TransColor)
    Else
      bytData(lngPos) = 0
    End If
        lngPos = lngPos + 1
    bytData(lngPos) = 0
    
    lngPos = lngPos + 1
    
    //add the cel data (first create cel data in separate array
    //then compress the cell data, break it into 255 byte chunks,
    //and add the chunks to the output
    
    //determine pad values
    With GifLoop.Cels(i)
      celH = .Height
      celW = .Width
      hPad = MaxH - celH
      wPad = MaxW - celW
      bytTrans = CByte(.TransColor)
    End With
    lngCelPos = 0
    
    For hVal = 0 To MaxH - 1
      //repeat each row based on scale factor
      For zFacH = 1 To GifOps.Zoom
        //step through each pixel in this row
        For wVal = 0 To MaxW - 1
          //repeat each pixel based on scale factor (x2 because AGI pixels are double-wide)
          For zFacW = 1 To GifOps.Zoom * 2
            //depending on alignment, may need to pad:
            If ((hVal < hPad) And (GifOps.VAlign = 1)) Or ((hVal > celH - 1) And (GifOps.VAlign = 0)) Then
              //use a transparent pixel
              bytCelData(lngCelPos) = bytTrans
            Else
              If ((wVal < wPad) And (GifOps.HAlign = 1)) Or ((wVal > celW - 1) And (GifOps.HAlign = 0)) Then
                //use a transparent pixel
                bytCelData(lngCelPos) = bytTrans
              Else
                If GifOps.HAlign = 1 Then
                  pX = wVal - wPad
                Else
                  pX = wVal
                End If
                If GifOps.VAlign = 1 Then
                  pY = hVal - hPad
                Else
                  pY = hVal
                End If
                //use the actual pixel (adjusted for padding, if aligned to bottom or left)
                bytCelData(lngCelPos) = CByte(GifLoop.Cels(i).CelData(pX, pY))
              End If
            End If
            lngCelPos = lngCelPos + 1
          Next zFacW
        Next wVal
      Next zFacH
    Next hVal
  
    //now compress the cel data
    bytCmpData() = GifLZW(bytCelData())
    
    //add Image descriptor
    bytData(lngPos) = 0x2C
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    bytData(lngPos) = CByte((MaxW * GifOps.Zoom * 2) And 0xFF)
    lngPos = lngPos + 1
    bytData(lngPos) = CByte((MaxW * GifOps.Zoom * 2) / 0x100)
    lngPos = lngPos + 1
    bytData(lngPos) = CByte((MaxH * GifOps.Zoom) And 0xFF)
    lngPos = lngPos + 1
    bytData(lngPos) = CByte((MaxH * GifOps.Zoom) / 0x100)
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    //add byte for initial LZW code size
    bytData(lngPos) = 4 //5
    lngPos = lngPos + 1
    
    //add the compressed data to filestream
    lngInPos = 0
    intChunkSize = 0
    
    Do
      If UBound(bytCmpData()) + 1 - lngInPos > 255 Then
        intChunkSize = 255
      Else
        intChunkSize = UBound(bytCmpData()) + 1 - lngInPos
      End If
      //write chunksize
      bytData(lngPos) = CByte(intChunkSize)
      lngPos = lngPos + 1
      
      //add this chunk of data
      For j = 1 To intChunkSize
        bytData(lngPos) = bytCmpData(lngInPos)
        lngPos = lngPos + 1
        lngInPos = lngInPos + 1
      Next j
    Loop Until lngInPos >= UBound(bytCmpData())
    
    //end with a zero-length block
    bytData(lngPos) = 0
    lngPos = lngPos + 1
  Next i
  
  //add trailer
  bytData(lngPos) = 0x3B
  
  ReDim Preserve bytData(lngPos)
  
  //now write file to output
  On Error Resume Next

  //get temporary file
  strTempFile = TempFileName()
  
  //open file for output
  intFile = FreeFile()
  Open strTempFile For Binary As intFile
  //write data
  Put intFile, , bytData
  
  //if error,
  If Err.Number != 0 Then
    //close file
    Close intFile
    //erase the temp file
    Kill strTempFile
    //return error condition
    //Debug.Assert false
    Exit Function
  End If
  
  //close file,
  Close intFile
  //if savefile exists
  If FileExists(ExportFile) Then
    //delete it
    Kill ExportFile
    Err.Clear
  End If
  
  //copy tempfile to savefile
  FileCopy strTempFile, ExportFile
  
  //if error,
  If Err.Number != 0 Then
    //close file
    Close intFile
    //erase the temp file
    Kill strTempFile
    //return error condition
    //Debug.Assert false
    Exit Function
  End If
    
  //delete temp file
  Kill strTempFile
  Err.Clear
  
  MakeLoopGif = true
Exit Function

ErrHandler:
  //if the error is because of trying to write to an array that is full,
  //expand it by 1K then try again
  If Err.Number = 9 Then
    //make sure it//s because lngPos is past array boundary
    If lngPos > UBound(bytData()) Then
      ReDim Preserve bytData(UBound(bytData()) + 1024)
      Resume
    Else
      //Debug.Assert false
      Resume Next
    End If
  End If
  
  //Debug.Assert false
  Resume Next
End Function
public Function MakePicGif(GifPic As AGIPicture, GifOps As GifOptions, ByVal ExportFile As String) As Boolean

  Dim strTempFile As String, intFile As Integer
  Dim bytData() As Byte, lngPos As Long //data that will be written to the gif file
  Dim lngInPos As Long, bytCmpData() As Byte, bytPicData() As Byte //data used to build then compress pic data as gif Image
  Dim bytFrameData() As Byte
  Dim lngPicPos As Long, bytCmd As Byte
  Dim blnXYDraw As Boolean, blnVisOn As Boolean
  
  Dim i As Integer, j As Integer
  
  const MaxH As Integer = 168
  const MaxW As Integer = 160
  
  Dim pX As Byte, pY As Byte
  Dim zFacH As Integer, zFacW As Integer
  Dim lngFramePos As Long, bytTrans As Byte
  
  //to use the aspect ratio property, set logical screen size to
  // 160 x 168, then set aspect ratio to 113; for each frame
  // added, set the size to 320 x 168 but only use the 160x168
  // data array - it works, but it also looks a little //fuzzy//
  // when viewed at small scales
  
  Dim intChunkSize As Integer
  
  //use error handler to expand size of data array when it gets full
  On Error GoTo ErrHandler
  
  //build header
  ReDim bytData(255)
  bytData(0) = 71
  bytData(1) = 73
  bytData(2) = 70
  bytData(3) = 56
  bytData(4) = 57
  bytData(5) = 97
  
  //add logical screen size info
  bytData(6) = CByte((MaxW * GifOps.Zoom * 2) And 0xFF)
  bytData(7) = CByte((MaxW * GifOps.Zoom * 2) / 0x100)
  bytData(8) = CByte((MaxH * GifOps.Zoom) And 0xFF)
  bytData(9) = CByte((MaxH * GifOps.Zoom) / 0x100)
  //add color info
  bytData(10) = 243 //1-111-0-011 means:
                    //global color table,
                    //8 bits per channel,
                    //no sorting, and
                    //16 colors in the table
  //background color:
  bytData(11) = 0
  //pixel aspect ratio:
//////  bytData(12) = 113 // (113+15)/64 = 2:1 ratio for pixels
  bytData(12) = 0 // no aspect ratio
  
  //add global color table
  For i = 0 To 15
    bytData(13 + 3 * i) = EGAColor(i) And 0xFF
    bytData(14 + 3 * i) = (EGAColor(i) And 0xFF00) / 0x100
    bytData(15 + 3 * i) = (EGAColor(i) And 0xFF0000) / 0x10000
  Next i
  
  //if cycling, add netscape extension to allow continuous looping
  If GifOps.Cycle Then
    //byte   1       : 33 (hex 0x21) GIF Extension code
    //byte   2       : 255 (hex 0xFF) Application Extension Label
    //byte   3       : 11 (hex 0x0B) Length of Application Block
    //                 (eleven bytes of data to follow)
    //bytes  4 to 11 : "NETSCAPE"
    //bytes 12 to 14 : "2.0"
    //byte  15       : 3 (hex 0x03) Length of Data Sub-Block
    //                 (three bytes of data to follow)
    //byte  16       : 1 (hex 0x01)
    //bytes 17 to 18 : 0 to 65535, an unsigned integer in
    //                 lo-hi byte format. This indicate the
    //                 number of iterations the loop should
    //                 be executed.
    //byte  19       : 0 (hex 0x00) a Data Sub-Block Terminator.
  
    bytData(61) = 0x21
    bytData(62) = 0xFF
    bytData(63) = 0xB
    For i = 1 To 11
      bytData(i + 63) = Asc(Mid("NETSCAPE2.0", i, 1))
    Next i
    bytData(75) = 3
    bytData(76) = 1
    bytData(77) = 0 //255
    bytData(78) = 0 //255
    bytData(79) = 0
    
    //at this point, numbering is not absolute, so we need to begin tracking the data position
    lngPos = 80
  Else
    //at this point, numbering is not absolute, so we need to begin tracking the data position
    lngPos = 61
  End If
  
  //pic data array
  ReDim bytPicData(MaxH * MaxW * GifOps.Zoom ^ 2 * 2 - 1)

  //add each frame
  lngPicPos = -1
  Do
    Do
      //increment data pointer
      lngPicPos = lngPicPos + 1
      
      //if end is reached
      If lngPicPos >= GifPic.Resource.Size Then
        Exit Do
      End If
      
      //get the value at this pos, and determine if it//s
      //a draw command
      bytCmd = GifPic.Resource.Data(lngPicPos)
      
      Select Case bytCmd
      Case 240
        blnXYDraw = false
        blnVisOn = true
        lngPicPos = lngPicPos + 1
      Case 241
        blnXYDraw = false
        blnVisOn = false
      Case 242
        blnXYDraw = false
        lngPicPos = lngPicPos + 1
      Case 243
        blnXYDraw = false
      Case 244, 245
        blnXYDraw = true
        lngPicPos = lngPicPos + 2
      Case 246, 247, 248, 250
        blnXYDraw = false
        lngPicPos = lngPicPos + 2
      Case 249
        blnXYDraw = false
        lngPicPos = lngPicPos + 1
      Case Else
        //skip second coordinate byte, unless
        //currently drawing X or Y lines
        If Not blnXYDraw Then
          lngPicPos = lngPicPos + 1
        End If
      End Select
      
    // exit if non-pen cmd found, and vis pen is active
    Loop While bytCmd >= 240 And bytCmd <= 244 Or bytCmd = 249 Or Not blnVisOn
    
    //if end is reached
    If lngPicPos >= GifPic.Resource.Size Then
      Exit Do
    End If
    
    //show pic drawn up to this point
    GifPic.DrawPos = lngPicPos
    bytFrameData = GifPic.VisData
    
    //add graphic control extension for this frame
    bytData(lngPos) = 0x21
        lngPos = lngPos + 1
    bytData(lngPos) = 0xF9
        lngPos = lngPos + 1
    bytData(lngPos) = 4
        lngPos = lngPos + 1
    bytData(lngPos) = 12   //000-011-0-0 = reserved-restore-no user input-no transparency
        lngPos = lngPos + 1
    bytData(lngPos) = GifOps.Delay And 0xFF
        lngPos = lngPos + 1
    bytData(lngPos) = (GifOps.Delay And 0xFF) / 0x100
        lngPos = lngPos + 1
    bytData(lngPos) = 0
        lngPos = lngPos + 1
    bytData(lngPos) = 0
        lngPos = lngPos + 1
    
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
    bytData(lngPos) = 0x2C
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    
    bytData(lngPos) = CByte((MaxW * GifOps.Zoom * 2) And 0xFF)
    lngPos = lngPos + 1
    bytData(lngPos) = CByte((MaxW * GifOps.Zoom * 2) / 0x100)
    lngPos = lngPos + 1
    
    bytData(lngPos) = CByte((MaxH * GifOps.Zoom) And 0xFF)
    lngPos = lngPos + 1
    bytData(lngPos) = CByte((MaxH * GifOps.Zoom) / 0x100)
    lngPos = lngPos + 1
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    //add byte for initial LZW code size
    bytData(lngPos) = 4 //5
    lngPos = lngPos + 1
    
    //add the compressed data to filestream
    lngInPos = 0
    intChunkSize = 0
    
    Do
      If UBound(bytCmpData()) + 1 - lngInPos > 255 Then
        intChunkSize = 255
      Else
        intChunkSize = UBound(bytCmpData()) + 1 - lngInPos
      End If
      //write chunksize
      bytData(lngPos) = CByte(intChunkSize)
      lngPos = lngPos + 1
      
      //add this chunk of data
      For j = 1 To intChunkSize
        bytData(lngPos) = bytCmpData(lngInPos)
        lngPos = lngPos + 1
        lngInPos = lngInPos + 1
      Next j
    Loop Until lngInPos >= UBound(bytCmpData())
    
    //end with a zero-length block
    bytData(lngPos) = 0
    lngPos = lngPos + 1
    
    //update progress
    frmProgress.pgbStatus.Value = lngPicPos
    Debug.Print frmProgress.pgbStatus.Value; "/"; frmProgress.pgbStatus.Max
    SafeDoEvents
    
  Loop Until lngPicPos >= GifPic.Resource.Size
  
  //add trailer
  bytData(lngPos) = 0x3B
  
  ReDim Preserve bytData(lngPos)
  
  //now write file to output
  On Error Resume Next

  //get temporary file
  strTempFile = TempFileName()
  
  //open file for output
  intFile = FreeFile()
  Open strTempFile For Binary As intFile
  //write data
  Put intFile, , bytData
  
  //if error,
  If Err.Number != 0 Then
    //close file
    Close intFile
    //erase the temp file
    Kill strTempFile
    //return error condition
    //Debug.Assert false
    Exit Function
  End If
  
  //close file,
  Close intFile
  //if savefile exists
  If FileExists(ExportFile) Then
    //delete it
    Kill ExportFile
    Err.Clear
  End If
  
  //copy tempfile to savefile
  FileCopy strTempFile, ExportFile
  
  //if error,
  If Err.Number != 0 Then
    //close file
    Close intFile
    //erase the temp file
    Kill strTempFile
    //return error condition
    //Debug.Assert false
    Exit Function
  End If
    
  //delete temp file
  Kill strTempFile
  Err.Clear
  
  MakePicGif = true
Exit Function

ErrHandler:
  //if the error is because of trying to write to an array that is full,
  //expand it by 64K then try again
  If Err.Number = 9 Then
    //make sure it//s because lngPos is past array boundary
    If lngPos > UBound(bytData()) Then
      ReDim Preserve bytData(UBound(bytData()) + 65536)
      Resume
    Else
      //Debug.Assert false
      Resume Next
    End If
  End If
  
  //Debug.Assert false
  Resume Next
End Function


Private Sub ExportPicBMP(ImgData() As Byte, ExportFile As String, Optional ByVal Zoom As Long = 1)

  //MUST specify a valid export file
  //if export file already exists, it is overwritten
  //caller is responsible for verifying overwrite is ok or not
  
  //assumption is that calling function won//t pass invalid filename, or call
  //this function if picture doesn//t have valid data
  
  Dim i As Long, j As Long, zh As Long, zv As Long
  Dim intFile As Integer
  
  On Error Resume Next
  
////  //get temporary file
////  strTempFile = TempFileName()
//   this function creates a temp file that//s
//   used elsewhere, so don't need temp file

  //open file for output
  intFile = FreeFile()
  Open ExportFile For Binary As intFile
  
  //write bmp file header
  Put intFile, , CByte(0x42) //B
  Put intFile, , CByte(0x4D) //M
  Put intFile, , CLng(118 + 26880 * Zoom ^ 2) //filesize?
  Put intFile, , CLng(0)     //reserved
  Put intFile, , CLng(118)   //offet to data
  //write bmpheaderinfo
  Put intFile, , CLng(40)    //size of header
  Put intFile, , CLng(320 * Zoom)  //width
  Put intFile, , CLng(-168 * Zoom)  //height (negative cuz pics are built bottom to top)
  Put intFile, , CInt(1)     //planes
  Put intFile, , CInt(4)     //bits per pixel (NOTE: this is different from Value used in memory(8); we//re intentionally doubling width for making a file)
  Put intFile, , CLng(0)     //compression
  Put intFile, , CLng(26880 * Zoom ^ 2) //data size
  Put intFile, , CLng(0)     //reserved
  Put intFile, , CLng(0)     //reserved
  Put intFile, , CLng(16)    //number of colors in color table
  Put intFile, , CLng(0)     //reserved
  
  //color table
  For i = 0 To 15
    Put intFile, , EGARevColor(i)
  Next i
  
  //now add data
  For j = 0 To 167
    For zv = 1 To Zoom
      For i = 0 To 159
        For zh = 1 To Zoom
          Put intFile, , CByte(ImgData(i + j * 160) Or 16 * ImgData(i + j * 160))
        Next zh
      Next i
    Next zv
  Next j
  //close file,
  Close intFile
  
////  //if savefile exists
////  If FileExists(ExportFile) Then
////    //delete it
////    Kill ExportFile
////    Err.Clear
////  End If
////
////  //copy tempfile to savefile
////  FileCopy strTempFile, ExportFile
  
  //if error,
  If Err.Number != 0 Then
    //close file
    Close intFile
////    //erase the temp file
////    Kill strTempFile
    //return error condition
    On Error GoTo 0: Err.Raise vbObjectError + 582, "ExportPicBMP", "unable to export BMP file"
    Exit Sub
  End If
    
////  //delete temp file
////  Kill strTempFile
////  Err.Clear
Exit Sub

ErrHandler:
  //unspecified error...
  //make sure file is closed
  Close intFile
////  //erase the temp file
////  Kill strTempFile
  //return error condition
  On Error GoTo 0: Err.Raise vbObjectError + 583, "ExportPicBMP", "unhandled error in ExportPicBMP"
End Sub

Private Sub ExportPicGIF(ImgData() As Byte, ExportFile As String, Optional ByVal Zoom As Long = 1)

//no longer needed, but i dont want to lose this code; it//s a working gif generator
//

////  //MUST specify a valid export file
////  //if export file already exists, it is overwritten
////  //caller is responsible for verifying overwrite is ok or not
////
////  //assumption is that calling function won//t pass invalid filename, or call
////  //this function if picture doesn//t have valid data
////
////  Dim i As Long, j As Long, zh As Long, zv As Long
////  Dim strTempFile As String, intFile As Integer
////  Dim bytData() As Byte, lngPos As Long //data that will be written to the gif file
////  Dim lngInPos As Long, bytCmpData() As Byte, bytImgData() As Byte //data used to build then compress img data as gif Image
////  Dim lngImgPos As Long
////  Dim intChunkSize As Integer
////
////  //use error handler to expand size of data array when it gets full
////  On Error GoTo ErrHandler
////
////  //build header
////  ReDim bytData(255)
////  bytData(0) = 71
////  bytData(1) = 73
////  bytData(2) = 70
////  bytData(3) = 56
////  bytData(4) = 57
////  bytData(5) = 97
////
////  //add logical screen size info
////  bytData(6) = CByte((320 * Zoom) And 0xFF)
////  bytData(7) = CByte((320 * Zoom) / 0x100)
////  bytData(8) = CByte((168 * Zoom) And 0xFF)
////  bytData(9) = CByte((168 * Zoom) / 0x100)
////  //add color info
////  bytData(10) = 243 //1-111-0-011 means:
////                    //global color table,
////                    //8 bits per channel,
////                    //no sorting, and
////                    //16 colors in the table
////  //background color:
////  bytData(11) = 0
////  //pixel aspect ratio:
////  bytData(12) = 0 //113  //should give proper 2:1 ratio for pixels
////
////  //add global color table
////  For i = 0 To 15
////    bytData(13 + 3 * i) = EGAColor(i) And 0xFF
////    bytData(14 + 3 * i) = (EGAColor(i) And 0xFF00) / 0x100
////    bytData(15 + 3 * i) = (EGAColor(i) And 0xFF0000) / 0x10000
////  Next i
////
////  //at this point, numbering is not absolute, so we need to begin tracking the data position
////  lngPos = 61
////
////  //add graphic control extension for this cel
////  bytData(lngPos) = 0x21
////  bytData(lngPos + 1) = 0xF9
////  bytData(lngPos + 2) = 4
////  bytData(lngPos + 3) = 12  //000-011-0-0 = reserved-restore-no user input-transparency included
////  bytData(lngPos + 4) = 0
////  bytData(lngPos + 5) = 0
////  bytData(lngPos + 6) = 0
////  bytData(lngPos + 7) = 0
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
////  bytData(lngPos) = 0x2C
////  bytData(lngPos + 1) = 0
////  bytData(lngPos + 2) = 0
////  bytData(lngPos + 3) = 0
////  bytData(lngPos + 4) = 0
////  bytData(lngPos + 5) = CByte((320 * Zoom) And 0xFF)
////  bytData(lngPos + 6) = CByte((320 * Zoom) / 0x100)
////  bytData(lngPos + 7) = CByte((168 * Zoom) And 0xFF)
////  bytData(lngPos + 8) = CByte((168 * Zoom) / 0x100)
////  bytData(lngPos + 9) = 0
////  //add byte for initial LZW code size
////  bytData(lngPos + 10) = 4 //5
////  //adjust pointer
////  lngPos = lngPos + 11
////
////  //add the compressed data to filestream
////  lngInPos = 0
////  intChunkSize = 0
////
////  Do
////    If UBound(bytCmpData()) + 1 - lngInPos > 255 Then
////      intChunkSize = 255
////    Else
////      intChunkSize = UBound(bytCmpData()) + 1 - lngInPos
////    End If
////    //write chunksize
////    bytData(lngPos) = CByte(intChunkSize)
////    lngPos = lngPos + 1
////
////    //add this chunk of data
////    For i = 1 To intChunkSize
////      bytData(lngPos) = bytCmpData(lngInPos)
////      lngPos = lngPos + 1
////      lngInPos = lngInPos + 1
////    Next i
////  Loop Until lngInPos >= UBound(bytCmpData())
////
////  //end with a zero-length block
////  bytData(lngPos) = 0
////  lngPos = lngPos + 1
////
////  //add trailer
////  bytData(lngPos) = 0x3B
////
////  ReDim Preserve bytData(lngPos)
////
////  //now write file to output
////  On Error Resume Next
////
////  //get temporary file
////  strTempFile = TempFileName()
////
////  //open file for output
////  intFile = FreeFile()
////  Open strTempFile For Binary As intFile
////  //write data
////  Put intFile, , bytData
////
////  //if error,
////  If Err.Number != 0 Then
////    //close file
////    Close intFile
////    //erase the temp file
////    Kill strTempFile
////    //return error condition
////    //Debug.Assert false
////    Exit Sub
////  End If
////
////  //close file,
////  Close intFile
////  //if savefile exists
////  If FileExists(ExportFile) Then
////    //delete it
////    Kill ExportFile
////    Err.Clear
////  End If
////
////  //copy tempfile to savefile
////  FileCopy strTempFile, ExportFile
////
////  //if error,
////  If Err.Number != 0 Then
////    //close file
////    Close intFile
////    //erase the temp file
////    Kill strTempFile
////    //return error condition
////    On Error GoTo 0: Err.Raise vbObjectError + 584, "ExportPicGIF", "unable to export GIF file"
////  End If
////
////  //delete temp file
////  Kill strTempFile
////  Err.Clear
////
////Exit Sub
////
////ErrHandler:
////  //if the error is because of trying to write to an array that is full,
////  //expand it by 1K then try again
////  If Err.Number = 9 Then
////    ReDim Preserve bytData(UBound(bytData()) + 1024)
////    Resume
////  End If
////
////  //Debug.Assert false
////  //unspecified error...
////  //make sure file is closed
////  Close intFile
////  //erase the temp file
////  Kill strTempFile
////  //return error condition
////  On Error GoTo 0: Err.Raise vbObjectError + 585, "ExportPicGIF", "unhandled error in ExportPicGIF"
End Sub

public Function ExportPicture(ThisPicture As AGIPicture, ByVal InGame As Boolean) As Boolean

  //exports this picture
  //either as an AGI picture resource, bitmap, *gif or a *jpeg Image
  
  Dim strFileName As String, ExportImage As Boolean
  Dim blnCanceled As Boolean, rtn As VbMsgBoxResult
  Dim lngZoom As Long, lngMode As Long, lngFormat As Long
  
  On Error GoTo ErrHandler
  
  //user chooses an export type
  Load frmPicExpOptions
  
  With frmPicExpOptions
    .SetForm 0
    .Show vbModal, frmMDIMain
    
    ExportImage = .optImage.Value
    blnCanceled = .Canceled
    lngZoom = CLng(.txtZoom.Text)
    lngFormat = .cmbFormat.ListIndex + 1
    If .optVisual.Value Then
      lngMode = 0
    ElseIf .optPriority.Value Then
      lngMode = 1
    Else
      lngMode = 2
    End If
  End With
  
  //done with the options form
  Unload frmPicExpOptions
  
  //if canceled
  If blnCanceled Then
    //just exit
    Exit Function
  End If
  
  //if exporting an image
  If ExportImage Then
    //get a filename
    //set up commondialog
    With MainSaveDlg
      .DialogTitle = "Save Picture Image As"
      .DefaultExt = "bmp"
      If NoGDIPlus Then
        .Filter = "BMP files (*.bmp)|*.bmp|All files (*.*)|*.*"
      Else
        .Filter = "BMP files (*.bmp)|*.bmp|JPEG files (*.jpg)|*.jpg|GIF files (*.gif)|*.gif|TIFF files (*.tif)|*.tif|PNG files (*.PNG)|*.png|All files (*.*)|*.*"
      End If
      .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
      .FilterIndex = lngFormat
      .FullName = ""
      .hWndOwner = frmMDIMain.hWnd
    End With
  
    On Error Resume Next
    Do
      MainSaveDlg.ShowSaveAs
      //if canceled,
      If Err.Number = cdlCancel Then
        //exit without doing anything
        Exit Function
      End If
      
      //if file exists,
      If FileExists(MainSaveDlg.FullName) Then
        //verify replacement
        rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
        
        If rtn = vbYes Then
          Exit Do
        ElseIf rtn = vbCancel Then
          Exit Function
        End If
      Else
        Exit Do
      End If
    Loop While true
    On Error GoTo ErrHandler
  
    //show wait cursor
    WaitCursor

    //show progress form
    Load frmProgress
    With frmProgress
      .Caption = "Exporting Picture Image"
      .lblProgress = "Depending on export size, this may take awhile. Please wait..."
      .pgbStatus.Visible = false
      .Show
      .Refresh
    End With
    
    ExportImg ThisPicture, MainSaveDlg.FullName, lngFormat, lngMode, lngZoom
      
    //all done!
    Unload frmProgress
    MsgBox "Success!", vbInformation + vbOKOnly, "Export Picture Image"
    
    Screen.MousePointer = vbDefault
    Exit Function
  Else
    // export the agi resource
  
    //set up commondialog
    With MainSaveDlg
      If InGame Then
        .DialogTitle = "Export Picture"
      Else
        .DialogTitle = "Save Picture As"
      End If
      .Filter = "WinAGI Picture Files (*.agp)|*.agp|All files (*.*)|*.*"
      .FilterIndex = ReadSettingLong(SettingsList, sPICTURES, sEXPFILTER, 1)
      Select Case .FilterIndex
      Case 1
        .DefaultExt = "agp"
      End Select
      
      .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
      
      //if Picture has a filename
      If LenB(ThisPicture.Resource.ResFile) != 0 Then
        //use it
        .FullName = ThisPicture.Resource.ResFile
      Else
        //use default name
        .FullName = ResDir + ThisPicture.ID
        Select Case .FilterIndex
        Case 1
          .FullName = .FullName + ".agp"
        End Select
      End If
      .hWndOwner = frmMDIMain.hWnd
    End With
  
    Do
      On Error Resume Next
      MainSaveDlg.ShowSaveAs
      //if canceled,
      If Err.Number = cdlCancel Then
        //exit without doing anything
        Exit Function
      End If
      On Error GoTo ErrHandler
      //save filterindex
      WriteAppSetting SettingsList, sPICTURES, sEXPFILTER, MainSaveDlg.FilterIndex
      //get filename
      strFileName = MainSaveDlg.FullName
      
      //if file exists,
      If FileExists(strFileName) Then
        //verify replacement
        rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
        
        If rtn = vbYes Then
          Exit Do
        ElseIf rtn = vbCancel Then
          Exit Function
        End If
      Else
        Exit Do
      End If
    Loop While true
    
    //show wait cursor
    WaitCursor
    
    //export the resource as agi picture
    ThisPicture.Export strFileName, Not InGame
    
    //if error,
    If Err.Number != 0 Then
      ErrMsgBox "An error occurred while exporting this file.", "", "Export File Error"
    End If
    
    //reset mouse pointer
    Screen.MousePointer = vbDefault
  End If
  
  ExportPicture = true
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function
public Function ExportSound(ThisSound As AGISound, ByVal InGame As Boolean) As Boolean

  //exports this Sound
  
  Dim strFileName As String, lngFilter As Long
  Dim rtn As VbMsgBoxResult, sFmt As Long, eFmt As SoundFormat
  
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
  
  With MainSaveDlg
    If InGame Then
      .DialogTitle = "Export Sound"
    Else
      .DialogTitle = "Save Sound As"
    End If
    Select Case sFmt
    Case 1
      .Filter = "WinAGI Sound Files (*.ags)|*.ags|MIDI Files (*.mid)|*.mid|AGI Sound Script Files (*.ass)|*.ass|All files (*.*)|*.*"
    Case 2
      .Filter = "WinAGI Sound Files (*.ags)|*.ags|WAV Files (*.wav)|*.wav|All files (*.*)|*.*"
    Case 3
      .Filter = "WinAGI Sound Files (*.ags)|*.ags|MIDI Files (*.mid)|*.mid|All files (*.*)|*.*"
    End Select
    
    //if Sound has a filename
    If LenB(ThisSound.Resource.ResFile) != 0 Then
      //use it
      .FullName = ThisSound.Resource.ResFile
    Else
      //id should be filename
      .FullName = ResDir + ThisSound.ID
    End If
    
    //set filter index (if it//s the script option, change it to
    //default ags for non-PC/PCjr sounds
    lngFilter = ReadSettingLong(SettingsList, sSOUNDS, sEXPFILTER, 1)
    Select Case sFmt
    Case 2, 3 //non-PC/PCjr
      If lngFilter = 4 Then
        lngFilter = 3
      End If
    End Select
    .FilterIndex = lngFilter
    
    //now set default extension
    Select Case .FilterIndex
    Case 1
      .DefaultExt = "ags"
    Case 2
      If sFmt = 2 Then
        .DefaultExt = "wav"
      Else
        .DefaultExt = "mid"
      End If
    Case 3
      If sFmt = 1 Then
        .DefaultExt = "ass"
      Else
        .DefaultExt = ""
      End If
    Case Else
      .DefaultExt = ""
    End Select
    
    .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
    .hWndOwner = frmMDIMain.hWnd
  End With
  
  
  Do
    On Error Resume Next
    MainSaveDlg.ShowSaveAs
    //if canceled,
    If Err.Number = cdlCancel Then
      //exit without doing anything
      Exit Function
    End If
    On Error GoTo ErrHandler
    
    //save default filter index
    WriteAppSetting SettingsList, sSOUNDS, sEXPFILTER, MainSaveDlg.FilterIndex
    
    //get filename
    strFileName = MainSaveDlg.FullName
      
    //if file exists,
    If FileExists(strFileName) Then
      //verify replacement
      rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
      
      If rtn = vbYes Then
        Exit Do
      ElseIf rtn = vbCancel Then
        Exit Function
      End If
    Else
      Exit Do
    End If
  Loop While true
  
  //show wait cursor
  WaitCursor
  
  //let export function use extension of file to determine format
  eFmt = sfUndefined
  
//////  Select Case sFmt
//////  Case 1 //pc/pcjr
//////    eFmt = MainSaveDlg.FilterIndex
//////
//////  Case 2 //IIgs PCM
//////    Select Case MainSaveDlg.FilterIndex
//////    Case 1  //.ags
//////      eFmt = sfAGI
//////    Case 2  //.wav
//////      eFmt = sfWAV
//////    Case 3  //*.*
//////      eFmt = sfUndefined
//////    End Select
//////
//////  Case 3 //IIgs MIDI
//////    Select Case MainSaveDlg.FilterIndex
//////    Case 1  //.ags
//////      eFmt = sfAGI
//////    Case 2  //.mid
//////      eFmt = sfMIDI
//////    Case 3  //*.*
//////      eFmt = sfUndefined
//////    End Select
//////  End Select
  
  On Error Resume Next
  //export the resource
  ThisSound.Export strFileName, eFmt, Not InGame
  
  //if error,
  If Err.Number != 0 Then
    ErrMsgBox "An error occurred while exporting this file.", "", "Export File Error"
  End If
  
  //reset mouse pointer
  Screen.MousePointer = vbDefault
  
  ExportSound = true
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function
public Function ExportView(ThisView As AGIView, ByVal InGame As Boolean) As Boolean

  //exports this view
  
  Dim strFileName As String
  Dim rtn As VbMsgBoxResult
  
  On Error GoTo ErrHandler
  
  //set up commondialog
  With MainSaveDlg
    If InGame Then
      .DialogTitle = "Export View"
    Else
      .DialogTitle = "Save View As"
    End If
    .Filter = "WinAGI View Files (*.agv)|*.agv|All files (*.*)|*.*"
    .FilterIndex = ReadSettingLong(SettingsList, sVIEWS, sEXPFILTER, 1)
    If .FilterIndex = 1 Then
      .DefaultExt = "agv"
    End If
    
    .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
    
    //if view has a filename
    If LenB(ThisView.Resource.ResFile) != 0 Then
      //use it
      .FullName = ThisView.Resource.ResFile
    Else
      //use default name
      .FullName = ResDir + ThisView.ID
      //add extension, if necessary
      If .FilterIndex = 1 Then
        .FullName = .FullName + ".agv"
      End If
    End If
    .hWndOwner = frmMDIMain.hWnd
  End With
  
  Do
    On Error Resume Next
    MainSaveDlg.ShowSaveAs
    //if canceled,
    If Err.Number = cdlCancel Then
      //exit without doing anything
      Exit Function
    End If
    On Error GoTo ErrHandler
    
    //save default filter index
    WriteAppSetting SettingsList, sVIEWS, sEXPFILTER, MainSaveDlg.FilterIndex
    //get filename
    strFileName = MainSaveDlg.FullName
    
    //if file exists,
    If FileExists(strFileName) Then
      //verify replacement
      rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
      
      If rtn = vbYes Then
        Exit Do
      ElseIf rtn = vbCancel Then
        Exit Function
      End If
    Else
      Exit Do
    End If
  Loop While true
  
  //show wait cursor
  WaitCursor
  
  //export the resource
  ThisView.Export strFileName, Not InGame
  
  //if error,
  If Err.Number != 0 Then
    ErrMsgBox "An error occurred while exporting this file.", "", "Export File Error"
    Exit Function
  End If
  
  //reset mouse pointer
  Screen.MousePointer = vbDefault
  
  ExportView = true
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function
public Function ExportWords(ThisWordList As AGIWordList, ByVal InGame As Boolean) As Boolean

  Dim strFileName As String
  Dim rtn As VbMsgBoxResult
  
  On Error GoTo ErrHandler
  
  //set up commondialog
  With MainSaveDlg
    If InGame Then
      .DialogTitle = "Export Word List"
    Else
      .DialogTitle = "Save Word List As"
    End If
    //if the list has a filename
    If LenB(ThisWordList.ResFile) != 0 Then
      //use it
      .FullName = ThisWordList.ResFile
    Else
      //use default name
      .FullName = ResDir + "WORDS.TOK"
    End If
    .Filter = "WinAGI Word List Files (*.agw)|*.agw|WORDS.TOK file|*.TOK|All files (*.*)|*.*"
    If LCase$(Right$(.FullName, 4)) = ".agw" Then
      .FilterIndex = 1
    Else
      .FilterIndex = 2
    End If
    .DefaultExt = ""
    .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
    .hWndOwner = frmMDIMain.hWnd
  End With
  
  Do
    On Error Resume Next
    MainSaveDlg.ShowSaveAs
    //if canceled,
    If Err.Number = cdlCancel Then
      //exit without doing anything
      Exit Function
    End If
    On Error GoTo ErrHandler
    
    //get filename
    strFileName = MainSaveDlg.FullName
    
    //if file exists,
    If FileExists(strFileName) Then
      //verify replacement
      rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
      
      If rtn = vbYes Then
        Exit Do
      ElseIf rtn = vbCancel Then
        Exit Function
      End If
    Else
      Exit Do
    End If
  Loop While true
  
  //show wait cursor
  WaitCursor
  
  If LCase$(Right$(MainSaveDlg.FullName, 4)) = ".agw" Then
    //save with new filename
    ThisWordList.Export strFileName, 1, Not InGame
  Else
    ThisWordList.Export strFileName, 0, Not InGame
  End If
  
  //if error,
  If Err.Number != 0 Then
    ErrMsgBox "An error occurred while exporting this file:", "", "Export File Error"
    Exit Function
  End If
  
  //reset mouse pointer
  Screen.MousePointer = vbDefault

  ExportWords = true
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function
public Function NewObjects() As Boolean

  //creates a new object file
  
  Dim frmNew As frmObjectEdit
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor

  //create a new file
  Set frmNew = New frmObjectEdit
  Load frmNew
  
  frmNew.NewObjects
  frmNew.Show
  
  //restore cursor while getting resnum
  Screen.MousePointer = vbDefault
  
Exit Function

ErrHandler:
  Resume Next
End Function

public Sub OpenGlobals(Optional ByVal ForceLoad As Boolean = false)

  Dim strFileName As String
  Dim intFile As Integer
  Dim frmNew As frmGlobals, tmpForm As Form
  
  On Error GoTo ErrHandler
  
  //if a game is loaded and NOT forcing...
  //   open editor if not yet in use
  //   or switch to it if it//s already open
  If GameLoaded And Not ForceLoad Then
    If GEInUse Then
      GlobalsEditor.SetFocus
      //if minimized,
      If GlobalsEditor.WindowState = vbMinimized Then
        //restore it
        GlobalsEditor.WindowState = vbNormal
      End If
      
    Else
      //load it
      
      WaitCursor
      
      //use the game//s default globals file
      strFileName = GameDir + "globals.txt"
      //look for global file
      If Not FileExists(strFileName) Then
        //look for a defines.txt file in the resource directory
        If FileExists(ResDir + "defines.txt") Then
          //copy it to globals.txt
          On Error Resume Next
          FileCopy ResDir + "defines.txt", strFileName
          On Error GoTo ErrHandler
        End If
      End If
      
      //now check again for globals file
      If Not FileExists(strFileName) Then
        //create blank file
        intFile = FreeFile()
        Open strFileName For Binary As intFile
        Close intFile
      End If
      
      //load it
      Set GlobalsEditor = New frmGlobals
      Load GlobalsEditor
      
      //set ingame status first, so caption will indicate correctly
      GlobalsEditor.InGame = true
      //loading function will handle any errors
      GlobalsEditor.LoadGlobalDefines strFileName //, false
      GlobalsEditor.Show
      //mark editor as in use
      GEInUse = true
      
      //reset cursor
      Screen.MousePointer = vbDefault
    End If
  Else
  //either a game is NOT loaded, OR we are forcing a load from file
    //get a globals file
    With MainDialog
      .Flags = cdlOFNHideReadOnly
      .DialogTitle = "Open Global Defines File"
      .DefaultExt = "txt"
      .Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
      .FilterIndex = ReadSettingLong(SettingsList, "Globals", sOPENFILTER, 1)
      .FileName = ""
      .InitDir = DefaultResDir
  
      .ShowOpen
      
      strFileName = .FileName
      
      //save filter
      WriteAppSetting SettingsList, "Globals", sOPENFILTER, .FilterIndex
      
      DefaultResDir = JustPath(.FileName)
    End With
  
    //check if already open
    For Each tmpForm In Forms
      If tmpForm.Name = "frmGlobals" Then
        If tmpForm.FileName = strFileName And Not tmpForm.InGame Then
          //just shift focus
          tmpForm.SetFocus
          Exit Sub
        End If
      End If
    Next
    
    //not open yet; create new form
    //and open this file into it
    WaitCursor
    
    Set frmNew = New frmGlobals
    
    //open this file
    Load frmNew
    //loading function will handle any errors
    frmNew.LoadGlobalDefines strFileName
    frmNew.Show
    
    Screen.MousePointer = vbDefault
  End If
Exit Sub

ErrHandler:
  //if user canceled the dialogbox,
  If Err.Number = cdlCancel Then
    Exit Sub
  End If
  
  //Debug.Assert false
  Resume Next
End Sub

public Function UpdatePrinterCaps(ByVal Orientation As Long, Optional ByVal Quiet As Boolean = false) As Boolean

  Dim rtn As Long, blnError As Boolean
  
  //enable error trapping
  On Error GoTo ErrHandler
  
  //set printer flag
  NoPrinter = (Printers.Count = 0)
  If NoPrinter Then
    Exit Function
  End If
  
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
  If GetDeviceCaps(Printer.hDC, BITSPIXEL) = 1 Then
    //this is a black and white printer
    NoColor = true
  Else
    NoColor = false
  End If

  PDPIx = 1440 / Printer.TwipsPerPixelX
  PDPIy = 1440 / Printer.TwipsPerPixelY
  PWidth = Printer.Width / 1440 * PDPIx
  PHeight = Printer.Height / 1440 * PDPIy
  
  UpdatePrinterCaps = true
Exit Function

ErrHandler:
  //only display an error msg once per call to this function
  If Not blnError And Not Quiet Then
    //some printers don't allow landscape printing
    If Err.Number = 380 Then
      MsgBox "The selected printer does not support the selected orientation." + vbNewLine + "Your output may not be formatted correctly.", vbCritical + vbInformation, "Printer Capability Error"
      Exit Function
    Else
      ErrMsgBox "An error occurred accessing the printer:", "Print features may not work correctly.", "Printer Error"
    End If
  End If
  blnError = true
  Resume Next
End Function

public Sub UpdateResFile(ByVal ResType As AGIResType, ByVal ResNum As Byte, ByVal OldFileName As String)
    
  //updates ingame id for resource files and the resource tree
  Dim tmpNode As Node
  
  Dim rtn As VbMsgBoxResult
  
  On Error GoTo ErrHandler
  
  Select Case ResType
  Case rtLogic
    //if a file with this name already exists
    If FileExists(ResDir + Logics(ResNum).ID + LogicSourceSettings.SourceExt) Then
      //make sure the change is not just a change in text case
     
    
      //import existing, or overwrite it?
      rtn = MsgBox("There is already a source file with the name //" + Logics(ResNum).ID + _
            LogicSourceSettings.SourceExt + "// in your source file directory." + vbCrLf + vbCrLf + _
            "Do you want to import that file? Choose //NO// to replace that file with the current logic source.", _
            vbYesNo, "Import Existing Source File?")
    Else
      //no existing file, so keep current source
      rtn = vbNo
    End If
    
    If rtn = vbYes Then
      //keep old file with new name as new name; basically import it by reloading, if currently loaded
        With Logics(ResNum)
        If .Loaded Then
          .Unload
          .Load
        End If
        End With
        
      //now update preview window, if previewing
      If Settings.ShowPreview Then
        If SelResType = rtLogic And SelResNum = ResNum Then
          PreviewWin.LoadPreview rtLogic, ResNum
        End If
      End If
    Else
      //if there is a file with the new ResID, rename it first
      On Error Resume Next
      //delete existing .OLD file (if there is one)
      Kill ResDir + Logics(ResNum).ID + LogicSourceSettings.SourceExt + ".OLD"
      //rename old file with new ResID as .OLD
      Name ResDir + Logics(ResNum).ID + LogicSourceSettings.SourceExt As ResDir + Logics(ResNum).ID + LogicSourceSettings.SourceExt + ".OLD"
      On Error GoTo ErrHandler
      //then, if there is a file with the previous ID
      //save it with the new ID
      If FileExists(OldFileName) Then
        Name OldFileName As ResDir + Logics(ResNum).ID + LogicSourceSettings.SourceExt
      Else
        Logics(ResNum).SaveSource
      End If
    End If
    
    //if layouteditor is open
    If LEInUse Then
      //redraw to ensure correct ID is displayed
      LayoutEditor.DrawLayout
    End If
    
  Case rtPicture
    //if autoexporting
    If Settings.AutoExport Then
      //if a file with this name already exists
      If FileExists(ResDir + Pictures(ResNum).ID + ".agp") Then
        //rename it
        Name ResDir + Pictures(ResNum).ID + ".agp" As ResDir + Pictures(ResNum).ID + ".agp" + ".OLD"
      End If
      
      If FileExists(OldFileName) Then
        //rename resource file, if it exists
        Name OldFileName As ResDir + Pictures(ResNum).ID + ".agp"
      Else
        //save it
        Pictures(ResNum).Export ResDir + Pictures(ResNum).ID + ".agp"
      End If
    End If
    
  Case rtSound
    //if autoexporting
    If Settings.AutoExport Then
      //if a file with this name already exists
      If FileExists(ResDir + Sounds(ResNum).ID + ".ags") Then
        //rename it
        Name ResDir + Sounds(ResNum).ID + ".ags" As ResDir + Sounds(ResNum).ID + ".ags" + ".OLD"
      End If
      
      If FileExists(OldFileName) Then
        //rename resource file, if it exists
        Name OldFileName As ResDir + Sounds(ResNum).ID + ".ags"
      Else
        Sounds(ResNum).Export ResDir + Sounds(ResNum).ID + ".ags"
      End If
    End If
    
  Case rtView
    //if autoexporting
    If Settings.AutoExport Then
      //if a file with this name already exists
      If FileExists(ResDir + Views(ResNum).ID + ".agv") Then
        //rename it
        Name ResDir + Views(ResNum).ID + ".agv" As ResDir + Views(ResNum).ID + ".agv" + ".OLD"
      End If
      
      //check to see if old file exists
      If FileExists(OldFileName) Then
        //rename resource file, if it exists
        Name OldFileName As ResDir + Views(ResNum).ID + ".agv"
      Else
        Views(ResNum).Export ResDir + Views(ResNum).ID + ".agv"
      End If
    End If
  End Select
      
  //update property window and resource list
  UpdateSelection ResType, ResNum, umProperty Or umResList
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub
public Sub ChangeResDescription(ByVal ResType As AGIResType, ByVal ResNum As Byte, ByVal Description As String)
    
  //updates ingame description
  //for resource objects and the resource tree
  
  On Error GoTo ErrHandler
  
  Select Case ResType
  Case rtObjects
    InventoryObjects.Description = Description
    InventoryObjects.Save
  
  Case rtWords
    VocabularyWords.Description = Description
    VocabularyWords.Save
    
  Case rtLogic
    Logics(ResNum).Description = Description
    Logics(ResNum).Save
    
  Case rtPicture
    Pictures(ResNum).Description = Description
    Pictures(ResNum).Save
    
  Case rtSound
    Sounds(ResNum).Description = Description
    Sounds(ResNum).Save
    
  Case rtView
    Views(ResNum).Description = Description
    Views(ResNum).Save
  End Select
  
  //update prop window
  UpdateSelection ResType, ResNum, umProperty
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub
public Function GetNewResID(ByVal ResType As AGIResType, ByVal ResNum As Long, ByRef ResID As String, ByRef Description As String, ByVal InGame As Boolean, ByVal FirstProp As Long) As Boolean
  
  Dim strOldResFile As String, strOldDesc As String
  Dim strOldID As String, blnReplace As Boolean //used when replacing IDs in logics
  Dim Index As Long, tmpForm As Form //frmLogicEdit
  Dim rtn As Long, strErrMsg As String
  
  //ResID and Description are passed ByRef, because the resource editors
  //need the updated values passed back to them
  
  On Error GoTo ErrHandler
  //Debug.Assert ResNum <= 256
  
  //should never get here with other restypes
  Select Case ResType
  Case rtGame, rtLayout, rtMenu, rtGlobals, rtGame, rtText, rtNone
    //Debug.Assert false
    Exit Function
  End Select
  
  //save incoming (old) ID and description
  strOldID = ResID
  strOldDesc = Description
  
  //if ingame,
  If InGame Then
    //need to save current ressource filename
    Select Case ResType
    Case rtLogic
      //save old sourcefile name
      strOldResFile = ResDir + Logics(ResNum).ID + LogicSourceSettings.SourceExt
    Case rtPicture
      strOldResFile = ResDir + Pictures(ResNum).ID + ".agp"
    Case rtSound
      strOldResFile = ResDir + Sounds(ResNum).ID + ".ags"
    Case rtView
      strOldResFile = ResDir + Views(ResNum).ID + ".agv"
    End Select
  End If
  
  //if restype is word or object
  If ResType = rtWords Or ResType = rtObjects Then
    //force prop to description
    FirstProp = 2
  Else
    //Debug.Assert ResNum >= 0
  End If
  
  With frmEditDescription
  If .Tag = "loaded" Then
    //Debug.Assert false
  End If
  .Tag = "loaded"
    //set current values
    .SetMode ResType, ResNum, ResID, Description, InGame, FirstProp
    
    Do
      On Error GoTo ErrHandler
      
      //get new values
      .Show vbModal, frmMDIMain
      
      //if canceled,
      If .Canceled Then
        //just exit
        Unload frmEditDescription
        Exit Function
      End If
    
      //validate return results
      Select Case ResType
      Case rtObjects, rtWords
        //only have description, so no need to validate
        Exit Do
        
      Case Else
        //if ID changed (case sensitive check here - case matters for what gets displayed
        If strOldID != .txtID.Text Then
        
          //validate new id
          rtn = ValidateID(.txtID.Text, strOldID)
        
          Select Case rtn
          Case 0 //ok
          Case 1 // no ID
            strErrMsg = "Resource ID cannot be blank."
          Case 2 // ID is numeric
            strErrMsg = "Resource IDs cannot be numeric."
          Case 3 // ID is command
            strErrMsg = ChrW$(39) + .txtID.Text + "// is an AGI command, and cannot be used as a resource ID."
          Case 4 // ID is test command
            strErrMsg = ChrW$(39) + .txtID.Text + "// is an AGI test command, and cannot be used as a resource ID."
          Case 5 // ID is a compiler keyword
            strErrMsg = ChrW$(39) + .txtID.Text + "// is a compiler reserved word, and cannot be used as a resource ID."
          Case 6 // ID is an argument marker
            strErrMsg = "Resource IDs cannot be argument markers"
          Case 14 // ID contains improper character
            strErrMsg = "Invalid character in resource ID: + vbnewline + !" + QUOTECHAR + "&//()*+,-/:;<=>?[\]^`{|}~ and spaces" + vbNewLine + "are not allowed."
          Case 15 // ID matches existing ResourceID
            //only enforce if in a game
            If InGame Then
              //check if this is same ID, same case
              strErrMsg = ChrW$(39) + .txtID.Text + "// is already in use as a resource ID."
            Else
              //reset to no error
              rtn = 0
            End If
          End Select
        
          //if there is an error
          If rtn != 0 Then
            //error - show msgbox
            MsgBoxEx strErrMsg, vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Change Resource ID", WinAGIHelp, "htm\winagi\Managing Resources.htm#resourceids"
          Else
            //make the change
            //update ID for the ingame resource
            Select Case ResType
            Case rtLogic
              Logics(ResNum).ID = .txtID.Text
            Case rtPicture
              Pictures(ResNum).ID = .txtID.Text
            Case rtSound
              Sounds(ResNum).ID = .txtID.Text
            Case rtView
              Views(ResNum).ID = .txtID.Text
            End Select
            Exit Do
          End If
        Else
          //if ID was exactly the same, no change needed
          Exit Do
        End If
      End Select
    Loop While true
    
    On Error GoTo ErrHandler
    
    //id change is acceptable (or it didn//t change)
    //return new id and description
    If strOldID != .txtID.Text Then
      ResID = .txtID.Text
    End If
    
    //if description changed, update it
    If strOldDesc != .txtDescription.Text Then
      Description = .txtDescription.Text
      
      //if in a game,
      If InGame Then
        //update the description
        Select Case ResType
        Case rtLogic
          Logics(ResNum).Description = Description
        Case rtPicture
          Pictures(ResNum).Description = Description
        Case rtSound
          Sounds(ResNum).Description = Description
        Case rtView
          Views(ResNum).Description = Description
        End Select
      End If
    End If
    
    //indicate success by returning TRUE
    GetNewResID = true
    
    //save replace flag value
    blnReplace = (.chkUpdate.Value = vbChecked)
    
    //save state of update logic flag
    DefUpdateVal = .chkUpdate.Value
  End With
  
  //update the logic tooltip lookup table for log/pic/view/snd
  Select Case ResType
  Case rtLogic
    Index = ResNum
  Case rtView
    Index = ResNum + 256
  Case rtSound
    Index = ResNum + 512
  Case rtPicture
    Index = ResNum + 768
  Case Else
    Index = -1
  End Select
  If Index >= 0 Then
    IDefLookup(Index).Name = ResID
  End If
  
  //unload the form
  Unload frmEditDescription
      
  //for ingame resources, update resource objects, files and the treelist
  If InGame Then
    Select Case ResType
    Case rtLogic, rtPicture, rtSound, rtView
      If strOldID != ResID Then
        //if not just a change in text case
        If StrComp(strOldID, ResID, vbTextCompare) != 0 Then
          //update resource file if ID has changed
          //this also updates the treelist, and property window
          UpdateResFile ResType, ResNum, strOldResFile
        Else
          //just change the filename
          On Error Resume Next
          Select Case ResType
          Case rtLogic
            Name strOldResFile As ResDir + ResID + LogicSourceSettings.SourceExt
          Case rtPicture
            Name strOldResFile As ResDir + ResID + ".agp"
          Case rtSound
            Name strOldResFile As ResDir + ResID + ".ags"
          Case rtView
            Name strOldResFile As ResDir + ResID + ".agv"
          End Select
          
          On Error GoTo ErrHandler
          //then update property window and resource list
          UpdateSelection ResType, ResNum, umProperty Or umResList
        End If
        
        //if OK to update in all logics, do so
        If blnReplace Then
          //reset search flags
          FindForm.ResetSearch
          
          //now replace the ID
          ReplaceAll strOldID, ResID, fdAll, true, true, flAll, ResType
        End If
        
      ElseIf strOldDesc != Description Then
        //update the property window, since description changed
        //update property window
        UpdateSelection ResType, ResNum, umProperty
      End If
            
    Case rtWords
      VocabularyWords.Description = Description
      //update property window and tree
      UpdateSelection ResType, ResNum, umProperty
      
    Case rtObjects
      InventoryObjects.Description = Description
      //update property window and tree
      UpdateSelection ResType, ResNum, umProperty
    
    End Select
    
    //set any open logics deflist flag to force a rebuild
    If LogicEditors.Count > 0 Then
      For Each tmpForm In LogicEditors
        If tmpForm.Name = "frmLogicEdit" Then
          If tmpForm.InGame Then
            tmpForm.ListDirty = true
          End If
        End If
      Next
    End If
  End If
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function


public Sub AddNewLogic(ByVal NewLogicNumber As Long, NewLogic As AGILogic, ByVal blnTemplate As Boolean, ByVal Importing As Boolean)

  Dim tmpNode As Node, tmpRel As TreeRelationshipConstants
  Dim tmpListItem As ListItem
  
  Dim strLogic As String, intFile As Integer
  Dim i As Long
  
  On Error GoTo ErrHandler
  //add to logic collection in game
  Logics.Add NewLogicNumber, NewLogic
  
  If Err.Number != 0 Then
    ////Debug.Print "why the heck doesn//t the !@#$ error handler work???"
    GoTo ErrHandler
  End If
  
  With Logics(NewLogicNumber)
    //if not importing, we need to add boilerplate text
    If Not Importing Then
      //if using template,
      If blnTemplate Then
        //add template text to logic source
        strLogic = LogTemplateText(Logics(NewLogicNumber).ID, Logics(NewLogicNumber).Description)
      Else
        //add default text
        strLogic = "[ " + vbCr + "[ " + Logics(NewLogicNumber).ID + vbCr + "[ " + vbCr + vbCr + "return();" + vbCr + vbCr + "[*****" + vbCr + "[ messages         [  declared messages go here" + vbCr + "[*****"
      End If
      //for new resources, need to set the source text
      .SourceText = strLogic
    End If
    
    //always save source to new name
    .SaveSource
    
    //if NOT importing AND default (not using template), compile the text
    If Not Importing And Not blnTemplate Then
      .Compile
    End If
    
    
    //set isroom status based on template
    If NewLogicNumber != 0 Then
      .IsRoom = blnTemplate
    End If
  
  End With
  
  //if using layout editor AND isroom
  If UseLE And Logics(NewLogicNumber).IsRoom Then
    //update layout editor and layout data file to show this room is in the game
    UpdateExitInfo euAddRoom, NewLogicNumber, Logics(NewLogicNumber)
  End If
  
  //add to resource list
  Select Case Settings.ResListType
  Case 1
    Set tmpNode = frmMDIMain.tvwResources.Nodes(2)
    //if no logics
    If tmpNode.Children = 0 Then
      //add it as first logic
      tmpRel = tvwChild
    Else
      //find place to insert this logic
      Set tmpNode = tmpNode.Child
      tmpRel = tvwPrevious
      Do Until tmpNode.Tag > NewLogicNumber
        If tmpNode.Next Is Nothing Then
          tmpRel = tvwNext
          Exit Do
        End If
        Set tmpNode = tmpNode.Next
      Loop
    End If
    
    //add to tree
    Set tmpNode = frmMDIMain.tvwResources.Nodes.Add(tmpNode.Index, tmpRel, "l" + CStr(NewLogicNumber), ResourceName(Logics(NewLogicNumber), true))
    tmpNode.Tag = NewLogicNumber
    
    If Not Logics(NewLogicNumber).Compiled Then
      tmpNode.ForeColor = vbRed
    End If
    
  Case 2
    //only update if logics are being listed
    If frmMDIMain.cmbResType.ListIndex = 1 Then
      //find a place to insert this logic in the box list
      If Logics.Count = 0 Then
        //add it as first item
        Set tmpListItem = frmMDIMain.lstResources.ListItems.Add(, "l" + CStr(NewLogicNumber), ResourceName(Logics(NewLogicNumber), true))
      Else
        //find a place to add it
        For i = 1 To frmMDIMain.lstResources.ListItems.Count
          If CLng(frmMDIMain.lstResources.ListItems(i).Tag) > NewLogicNumber Then
            Exit For
          End If
        Next i
        //i is index position we are looking for
        Set tmpListItem = frmMDIMain.lstResources.ListItems.Add(i, "l" + CStr(NewLogicNumber), ResourceName(Logics(NewLogicNumber), true))
      End If
      tmpListItem.Tag = NewLogicNumber
      If Not Logics(NewLogicNumber).Compiled Then
        tmpListItem.ForeColor = vbRed
      End If
      //expand column width if necessary
      If 1.2 * frmMDIMain.picResources.TextWidth(tmpListItem.Text) > frmMDIMain.lstResources.ColumnHeaders(1).Width Then
        frmMDIMain.lstResources.ColumnHeaders(1).Width = 1.2 * frmMDIMain.picResources.TextWidth(tmpListItem.Text)
      End If
    End If
  
  End Select
  //update the logic tooltip lookup table
  With IDefLookup(NewLogicNumber)
    .Name = Logics(NewLogicNumber).ID
    .Type = atNum
  End With
  //then let open logic editors know
  If LogicEditors.Count > 0 Then
    For i = 1 To LogicEditors.Count
      LogicEditors(i).ListDirty = true
    Next i
  End If
  
  //last index is no longer accurate; reset
  frmMDIMain.LastIndex = -1
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Function NextMsg(strText As String, ByRef lngLoc As Long, LDefList() As TDefine, StrDefs() As String) As String

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
  
  Dim strCmd As String, lngArg As Long
  Dim strNext As String, lngNext As Long
  Dim lngQuotesOK As Long, lngSlashCount As Long
  Dim i As Long, blnDefFound As Boolean
  Dim sngVal As Single
  
  On Error GoTo ErrHandler
 
  //get next cmd
  FindNextToken strText, lngLoc, strCmd, true, true
  Do Until lngLoc = 0
    //assume not found
    lngArg = -1
    
    //check against list of cmds with msg arguments:
    Select Case Len(strCmd)
    Case 3  //check for log
      If StrComp(strCmd, "log", vbTextCompare) = 0 Then
        //get arg 0
        lngArg = 0
      End If
    
    Case 5 //check for print
      If StrComp(strCmd, "print", vbTextCompare) = 0 Then
        //get arg 0
        lngArg = 0
      End If
    
    Case 7  //display or get.num
      If StrComp(strCmd, "display", vbTextCompare) = 0 Then
        lngArg = 2
      ElseIf StrComp(strCmd, "get.num", vbTextCompare) = 0 Then
        lngArg = 0
      End If
    
    Case 8  //print.at or set.menu
      If StrComp(strCmd, "print.at", vbTextCompare) = 0 Then
        lngArg = 0
      ElseIf StrComp(strCmd, "set.menu", vbTextCompare) = 0 Then
        lngArg = 0
      End If
    
    Case 10 //set.string, get.string
      If StrComp(strCmd, "set.string", vbTextCompare) = 0 Then
        lngArg = 1
      ElseIf StrComp(strCmd, "get.string", vbTextCompare) = 0 Then
        lngArg = 1
      End If
    
    Case 11 //set.game.id
      If StrComp(strCmd, "set.game.id", vbTextCompare) = 0 Then
        lngArg = 0
      End If
      
    Case 13 //set.menu.item
      If StrComp(strCmd, "set.menu.item", vbTextCompare) = 0 Then
        lngArg = 0
      End If
      
    Case 15 //set.cursor.char
      If StrComp(strCmd, "set.cursor.char", vbTextCompare) = 0 Then
        lngArg = 0
      End If
    End Select
    
    //if a cmd was found (lngArg<>-1)
    If lngArg != -1 Then
      //syntax expected will be
      // (arg0, arg1, ...
      strCmd = ""
      FindNextToken strText, lngLoc, strCmd, true, true
      If lngLoc = 0 Then
        //end of logic
        Exit Function
      End If
      
      If strCmd = "(" Then
        //next one is arg0
        strCmd = ""
        FindNextToken strText, lngLoc, strCmd, true, true
        If lngLoc = 0 Then
          //end of logic
          Exit Function
        End If
      Else
        //not a valid cmd; return error
        lngLoc = -lngLoc
        NextMsg = "1"
        Exit Function
      End If
    End If
    
    //if not getting arg0
    If lngArg > 0 Then
      Do
        //next cmd is a comment
        strCmd = ""
        FindNextToken strText, lngLoc, strCmd, true, true
        If lngLoc = 0 Then
          //end of logic
          Exit Function
        End If
        
        If strCmd != "," Then
          //something not right; just ignore this command
          lngArg = -1
          Exit Do
        End If
        //now get next arg
        strCmd = ""
        FindNextToken strText, lngLoc, strCmd, true, true
        If lngLoc = 0 Then
          //end of logic
          Exit Function
        End If
        //decrement Count
        lngArg = lngArg - 1
      Loop Until lngArg <= 0
    End If
    
    //if not found, maybe it//s a string assignment (s##="text"; or strdefine="text";)
    If lngArg = -1 Then
      //is it a defined string?
      For i = 0 To UBound(StrDefs())
        If LCase(strCmd) = LCase(StrDefs(i)) Then
          //good to go - we don't care what s##
          //just that it//s a valid define value
          lngArg = 0
          Exit For
        End If
      Next i
      
      //if not found as a define, maybe it//s a string marker (s##)
      If lngArg = -1 And Asc(LCase(strCmd)) = 115 Then
        //strip off the //s//; if rest of cmd is a valid string number
        //then we found one!
        strCmd = Right(strCmd, Len(strCmd) - 1)
        If IsNumeric(strCmd) Then
          //do we care what number? yes- must be 0-23
          //in the off chance the user is working with
          //a version that has a limit of 12 strings
          //we will let the compiler worry about it
          If Val(strCmd) >= 0 And Val(strCmd) <= 23 Then
            //must be integer
            If Val(strCmd) = Int(Val(strCmd)) Then
              //we found one
              lngArg = 0
            End If
          End If
        End If
      End If
      
      //if we found a string assignment, now we need to get the msg value
      If lngArg = 0 Then
        //next cmd should be an equal sign
        strCmd = ""
        FindNextToken strText, lngLoc, strCmd, true, true
        If lngLoc = 0 Then
          //end of logic
          Exit Function
        End If
        If strCmd = "=" Then
          //ok, now we know the very next thing is the assigned string!
          strCmd = ""
          FindNextToken strText, lngLoc, strCmd, true, true
          If lngLoc = 0 Then
            //end of logic
            Exit Function
          End If
        Else
          //false alarm! it//s probably a string arg used in another
          //command
          lngArg = -1
        End If
      End If
    End If
    
    //if we have a valid command and found it//s arg value, continue
    If lngArg != -1 Then
      //strcmd is now the msg argument we are looking for;
      //it might be a message marker (//m##//) or it might be
      //a string; or it might be a local, global or reserved
      //define; need to validate it
      
      //first, is it a valid message string? if not,
      If Not IsValidMsg(strCmd) Then
        //does it start with a dbl quote? if so, it//s a malformed string
        If Asc(strCmd) = 34 Then
          lngLoc = -lngLoc
          NextMsg = "2"
          Exit Function
        End If
        
        //it//s not a string (good or bad);
        //might be a message marker or a define value
        //check for message marker first
        If LCase(Left(strCmd, 1)) = "m" Then
          //if it//s a non-zero number, less than 256, it//s good
          If IsNumeric(Right(strCmd, Len(strCmd) - 1)) Then
            sngVal = Val(Right(strCmd, Len(strCmd) - 1))
            If (Int(sngVal) = sngVal) And sngVal > 0 And sngVal < 256 Then
              //return the msg number
              lngLoc = -lngLoc
              NextMsg = "0|" + CStr(sngVal)
              Exit Function
            End If
          End If
        End If
        
        //not a msg marker; try replacing with define
        blnDefFound = false
        Do
          //locals first
          For i = 0 To UBound(LDefList())
            Select Case LDefList(i).Type
            Case atMsg
              //a msg define
              If StrComp(LDefList(i).Name, strCmd, vbTextCompare) = 0 Then
                //we can//t assume loacal defines are valid
                sngVal = Val(Right(LDefList(i).Value, Len(LDefList(i).Value) - 1))
                If (Int(sngVal) = sngVal) And sngVal > 0 And sngVal < 256 Then
                  //return the msg number
                  lngLoc = -lngLoc
                  NextMsg = "0|" + CStr(sngVal)
                  Exit Function
                End If
              End If
            Case atDefStr
              //a string
              If LDefList(i).Type = atDefStr Then
                //does it match?
                If StrComp(LDefList(i).Name, strCmd, vbTextCompare) = 0 Then
                  //but we can//t assume it//s a valid string
                  If IsValidMsg(LDefList(i).Value) Then
                    //we don't replace the define value
                    //but just mark it as OK
                    blnDefFound = true
                    Exit Do
                  End If
                End If
              End If
            End Select
          Next i
          
          //try globals next
          For i = 0 To UBound(GDefLookup())
            Select Case GDefLookup(i).Type
            Case atMsg
              //a msg define
              If StrComp(GDefLookup(i).Name, strCmd, vbTextCompare) = 0 Then
                //global defines are already validated
                sngVal = Val(Right(strCmd, Len(strCmd) - 1))
                //return the msg number
                lngLoc = -lngLoc
                NextMsg = "0|" + CStr(sngVal)
                Exit Function
              End If
            Case atDefStr
              //a string
              If GDefLookup(i).Type = atDefStr Then
                If StrComp(GDefLookup(i).Name, strCmd, vbTextCompare) = 0 Then
                  //global defines are already validated
                  //we don't replace the define name
                  //but just mark it as OK
                  blnDefFound = true
                  Exit Do
                End If
              End If
            End Select
          Next i
          
          //next check reserved defines
          //(we only check the three string values
          // as they are the only resefs that could
          // apply)
          For i = 90 To 92
            If StrComp(RDefLookup(i).Name, strCmd, vbTextCompare) = 0 Then
              //reserved defines are already validated
              //we don't replace the define value
              //but just mark it as OK
              blnDefFound = true
              Exit Do
            End If
          Next i
        Loop Until true
        
        //if a valid define was found, we keep the
        //define name as the message value
        //but if not found, just treat it as an error
        If Not blnDefFound Then
          //not a string, and not a msg marker - IDK what it is!
          lngLoc = -lngLoc
          NextMsg = "3"
          Exit Function
        End If
      End If
      
      //strCmd is validated to be a good string;
      //now check for concatenation (unless it//s a define)
      If blnDefFound Then
        lngNext = 0
      Else
        lngNext = lngLoc
      End If
      
      Do Until lngNext = 0
        strNext = ""
        FindNextToken strText, lngNext, strNext, true, true
        If lngLoc = 0 Then
          //end of logic; return what we found
          NextMsg = strCmd
          Exit Function
        End If
        
        //next cmd may be another string
        If IsValidMsg(strNext) Then
          //concatenat it
          strCmd = Left$(strCmd, Len(strCmd) - 1) + Right$(strNext, Len(strNext) - 1)
        Else
          //no concatenation;
          //just exit with existing string
          lngNext = 0
        End If
      Loop
      
      //return this string
      NextMsg = strCmd
      Exit Function
    End If
    
    //get next cmd
    strCmd = ""
    FindNextToken strText, lngLoc, strCmd, true, true
  Loop
Exit Function

ErrHandler:
  Resume Next
End Function
public Function ReadMsgs(strText As String, Messages() As String, MsgUsed() As Long, LDefList() As TDefine) As Boolean

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
  
  Dim intMsgNum As Integer, lngPos As Long
  Dim strMsgMarker As String, lngMsgStart As Long
  Dim lngConcat As Long, strCmd As String
  Dim blnConcat As Boolean, i As Long, blnDefFound As Boolean
  
  On Error GoTo ErrHandler
  
  strMsgMarker = "#message"
  
  //get first message marker position
  FindNextToken strText, lngPos, strMsgMarker
  
  Do Until lngPos = 0
    //next cmd should be msg number
    strCmd = ""
    FindNextToken strText, lngPos, strCmd
    
    //validate msg number
    If Val(strCmd) < 1 Or Val(strCmd) > 255 Then
      //invalid msg number
      Messages(0) = "1"
      Messages(1) = CStr(lngPos)
      Exit Function
    End If
    
    intMsgNum = CInt(Val(strCmd))
    If Val(strCmd) != intMsgNum Then
      //invalid msg number
      Messages(0) = "1"
      Messages(1) = CStr(lngPos)
      Exit Function
    End If
    
    //if msg is already assigned
    If MsgUsed(intMsgNum) != 0 Then
      //user needs to fix message section first;
      //return false, and use the Message structure to indicate
      //what the problem is, and on which line it occurred
      Messages(0) = "2" //"duplicate msg number"
      Messages(1) = CStr(lngPos)
      Messages(2) = CStr(intMsgNum)
      Exit Function
    End If
        
    //next cmd should be a string
    strCmd = ""
    FindNextToken strText, lngPos, strCmd
    
    //msgval should be string
    If Not IsValidMsg(strCmd) Then
      //does it start with a dbl quote? if so, it//s a malformed string
      If Asc(strCmd) = 34 Then
        Messages(0) = "5"
        Messages(1) = CStr(lngPos)
        Exit Function
      End If
        
      //it//s not a good (or bad) string;
      //try replacing with define (locals, then globals, then reserved
      blnDefFound = false
      Do
        For i = 0 To UBound(LDefList())
          If LDefList(i).Type = atDefStr Then
            If LDefList(i).Name = strCmd Then
              blnDefFound = true
              Exit Do
            End If
          End If
        Next i
        //try globals next
        For i = 0 To UBound(GDefLookup())
          If GDefLookup(i).Type = atDefStr Then
            If GDefLookup(i).Name = strCmd Then
              blnDefFound = true
              Exit Do
            End If
          End If
        Next i
        //then try reserved
        For i = 0 To UBound(LogicSourceSettings.ReservedDefines(atDefStr))
          If LogicSourceSettings.ReservedDefines(atDefStr)(i).Type = atDefStr Then
            If LogicSourceSettings.ReservedDefines(atDefStr)(i).Name = strCmd Then
              blnDefFound = true
              Exit Do
            End If
          End If
        Next i
      Loop Until true
      
      //if it was replaced, we accept whatever was used as
      //the define name; if not replaced, it//s error
      If Not blnDefFound Then
        //not a string, and not a msg marker - IDK what it is!
        Messages(0) = "3"
        Messages(1) = CStr(lngPos)
        Exit Function
      End If
    End If
    
    //copy string cmd
    Messages(intMsgNum) = strCmd
    
    //check for end of line/concatenation
    blnConcat = false
    lngConcat = lngPos
    Do
      strCmd = ""
      FindNextToken strText, lngConcat, strCmd
      //if end of input
      If lngConcat = 0 Then
        //exit the loop since no more text left to process
        Exit Do
      End If
      
      If blnConcat Then
        //concatenation is optional; if string is found,
        //then concatenate it; if not, we//re done
        If IsValidMsg(strCmd) Then
          //add the concat string
          Messages(intMsgNum) = Left$(Messages(intMsgNum), Len(Messages(intMsgNum)) - 1) + Right$(strCmd, Len(strCmd) - 1)
          
          //toggle concat flag
          blnConcat = false
        Else
          //it//s not a valid string, but we
          //do need to see if its an INVALID string
          If Asc(strCmd) = 34 Then
            //it//s bad!
            Messages(0) = "3"
            Messages(1) = CStr(lngPos)
            Exit Function
          Else
            //any other command means no concatenation needed
            Exit Do
          End If
        End If
      Else
        //only carriage return, or comments allowed after a string
        If strCmd = vbCr Or Left$(strCmd, 2) = "//" Or Asc(strCmd) = 91 Then
          blnConcat = true
        Else
          //stuff not allowed on line after msg declaration
          Messages(0) = "4"
          Messages(1) = CStr(lngConcat)
          Exit Function
        End If
      End If
    Loop While true
    
    //set flag to show message is declared
    MsgUsed(intMsgNum) = 1
    
    //check for end of input
    If lngPos = 0 Then
      Exit Do
    End If
    
    //get next msg marker
    FindNextToken strText, lngPos, strMsgMarker
  Loop
  
  //success
  ReadMsgs = true
Exit Function

ErrHandler:
  Resume Next
End Function

public Sub AddNewPicture(ByVal NewPictureNumber As Long, NewPicture As AGIPicture)

  Dim tmpNode As Node, tmpRel As TreeRelationshipConstants
  Dim tmpListItem As ListItem
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  //add picture to game collection
  Pictures.Add NewPictureNumber, NewPicture
  
  //add to resource list
  Select Case Settings.ResListType
  Case 1
    Set tmpNode = frmMDIMain.tvwResources.Nodes(3)
    //if no sounds
    If tmpNode.Children = 0 Then
      //add it as first picture
      tmpRel = tvwChild
    Else
      //find place to insert this picture
      Set tmpNode = tmpNode.Child
      tmpRel = tvwPrevious
      Do Until tmpNode.Tag > NewPictureNumber
        If tmpNode.Next Is Nothing Then
          tmpRel = tvwNext
          Exit Do
        End If
        Set tmpNode = tmpNode.Next
      Loop
    End If
    
    //add it to tree
    frmMDIMain.tvwResources.Nodes.Add(tmpNode.Index, tmpRel, "p" + CStr(NewPictureNumber), ResourceName(Pictures(NewPictureNumber), true)).Tag = NewPictureNumber
    
  Case 2
    //only update if pictures are being listed
    If frmMDIMain.cmbResType.ListIndex = 2 Then
      //if no pictures yet
      If Pictures.Count = 0 Then
        //add it as first item
        Set tmpListItem = frmMDIMain.lstResources.ListItems.Add(, "p" + CStr(NewPictureNumber), ResourceName(Pictures(NewPictureNumber), true))
      Else
        //find a place to add it
        For i = 1 To frmMDIMain.lstResources.ListItems.Count
          If CLng(frmMDIMain.lstResources.ListItems(i).Tag) > NewPictureNumber Then
            Exit For
          End If
        Next i
        //i is index position we are looking for
        Set tmpListItem = frmMDIMain.lstResources.ListItems.Add(i, "p" + CStr(NewPictureNumber), ResourceName(Pictures(NewPictureNumber), true))
      End If
      tmpListItem.Tag = NewPictureNumber
      //expand column width if necessary
      If 1.2 * frmMDIMain.picResources.TextWidth(tmpListItem.Text) > frmMDIMain.lstResources.ColumnHeaders(1).Width Then
        frmMDIMain.lstResources.ColumnHeaders(1).Width = 1.2 * frmMDIMain.picResources.TextWidth(tmpListItem.Text)
      End If
    End If
  End Select
    
  //update the logic tooltip lookup table
  With IDefLookup(NewPictureNumber + 768)
    .Name = Pictures(NewPictureNumber).ID
    .Type = atNum
  End With
  //then let open logic editors know
  If LogicEditors.Count > 0 Then
    For i = 1 To LogicEditors.Count
      LogicEditors(i).ListDirty = true
    Next i
  End If
  
  //last index is no longer accurate; reset
  frmMDIMain.LastIndex = -1
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub AddNewSound(ByVal NewSoundNumber As Long, NewSound As AGISound)

  Dim tmpNode As Node, tmpRel As TreeRelationshipConstants
  Dim tmpListItem As ListItem
  Dim i As Long
  
  //add sound to game collection
  Sounds.Add NewSoundNumber, NewSound
  
  Select Case Settings.ResListType
  Case 1
    //add to treelist
    Set tmpNode = frmMDIMain.tvwResources.Nodes(4)
    //if no sounds
    If tmpNode.Children = 0 Then
      //add it as first sound
      tmpRel = tvwChild
    Else
      //find place to insert this sound
      Set tmpNode = tmpNode.Child
      tmpRel = tvwPrevious
      Do Until tmpNode.Tag > NewSoundNumber
        If tmpNode.Next Is Nothing Then
          tmpRel = tvwNext
          Exit Do
        End If
        Set tmpNode = tmpNode.Next
      Loop
    End If
    
    //add it to tree
    frmMDIMain.tvwResources.Nodes.Add(tmpNode.Index, tmpRel, "s" + CStr(NewSoundNumber), ResourceName(Sounds(NewSoundNumber), true)).Tag = NewSoundNumber
        
  Case 2
    //only update if sounds are being updated
    If frmMDIMain.cmbResType.ListIndex = 3 Then
      //if no sounds yet
      If Sounds.Count = 0 Then
        //add it as first item
        Set tmpListItem = frmMDIMain.lstResources.ListItems.Add(, "s" + CStr(NewSoundNumber), ResourceName(Sounds(NewSoundNumber), true))
      Else
        //find a place to add it
        For i = 1 To frmMDIMain.lstResources.ListItems.Count
          If CLng(frmMDIMain.lstResources.ListItems(i).Tag) > NewSoundNumber Then
            Exit For
          End If
        Next i
        //i is index position we are looking for
        Set tmpListItem = frmMDIMain.lstResources.ListItems.Add(i, "s" + CStr(NewSoundNumber), ResourceName(Sounds(NewSoundNumber), true))
      End If
      tmpListItem.Tag = NewSoundNumber
      //expand column width if necessary
      If 1.2 * frmMDIMain.picResources.TextWidth(tmpListItem.Text) > frmMDIMain.lstResources.ColumnHeaders(1).Width Then
        frmMDIMain.lstResources.ColumnHeaders(1).Width = 1.2 * frmMDIMain.picResources.TextWidth(tmpListItem.Text)
      End If
    End If
  End Select
  
  //update the logic tooltip lookup table
  With IDefLookup(NewSoundNumber + 512)
    .Name = Sounds(NewSoundNumber).ID
    .Type = atNum
  End With
  //then let open logic editors know
  If LogicEditors.Count > 0 Then
    For i = 1 To LogicEditors.Count
      LogicEditors(i).ListDirty = true
    Next i
  End If
  
  //last index is no longer accurate; reset
  frmMDIMain.LastIndex = -1
End Sub
public Sub AddNewView(ByVal NewViewNumber As Long, NewView As AGIView)

  Dim tmpNode As Node, tmpRel As TreeRelationshipConstants
  Dim tmpListItem As ListItem
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  //add view to game collection
  Views.Add NewViewNumber, NewView
  
  Select Case Settings.ResListType
  Case 1
    //add to treelist
    Set tmpNode = frmMDIMain.tvwResources.Nodes(5)
    //if no views
    If tmpNode.Children = 0 Then
      //add it as first view
      tmpRel = tvwChild
    Else
      //find place to insert this view
      Set tmpNode = tmpNode.Child
      tmpRel = tvwPrevious
      Do Until tmpNode.Tag > NewViewNumber
        If tmpNode.Next Is Nothing Then
          tmpRel = tvwNext
          Exit Do
        End If
        Set tmpNode = tmpNode.Next
      Loop
    End If
  
    //add it to tree
    frmMDIMain.tvwResources.Nodes.Add(tmpNode.Index, tmpRel, "v" + CStr(NewViewNumber), ResourceName(Views(NewViewNumber), true)).Tag = NewViewNumber
  
  Case 2
    //only update if views are being displayed
    If frmMDIMain.cmbResType.ListIndex = 4 Then
      //if no views yet
      If Views.Count = 0 Then
        //add it as first item
        Set tmpListItem = frmMDIMain.lstResources.ListItems.Add(, "v" + CStr(NewViewNumber), ResourceName(Views(NewViewNumber), true))
      Else
        //find a place to add it
        For i = 1 To frmMDIMain.lstResources.ListItems.Count
          If CLng(frmMDIMain.lstResources.ListItems(i).Tag) > NewViewNumber Then
            Exit For
          End If
        Next i
        //i is index position we are looking for
        Set tmpListItem = frmMDIMain.lstResources.ListItems.Add(i, "v" + CStr(NewViewNumber), ResourceName(Views(NewViewNumber), true))
      End If
      tmpListItem.Tag = NewViewNumber
      //expand column width if necessary
      If 1.2 * frmMDIMain.picResources.TextWidth(tmpListItem.Text) > frmMDIMain.lstResources.ColumnHeaders(1).Width Then
        frmMDIMain.lstResources.ColumnHeaders(1).Width = 1.2 * frmMDIMain.picResources.TextWidth(tmpListItem.Text)
      End If
    End If
  End Select
  
  //update the logic tooltip lookup table
  With IDefLookup(NewViewNumber + 256)
    .Name = Views(NewViewNumber).ID
    .Type = atNum
  End With
  //then let open logic editors know
  If LogicEditors.Count > 0 Then
    For i = 1 To LogicEditors.Count
      LogicEditors(i).ListDirty = true
    Next i
  End If
  
  //last index is no longer accurate; reset
  frmMDIMain.LastIndex = -1
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub
public Function MsgBoxEx(ByVal Prompt As String, Optional ByVal Buttons As VbMsgBoxStyle = vbOKOnly, Optional Title, Optional ByVal HelpFile As String, Optional ByVal HelpTopic As String, Optional ByVal CheckString As String = "", Optional ByRef Checked As Boolean = false) As VbMsgBoxResult
  //Title has to be a non-declared type for the IsMissing function to work
  
  Dim lngButtonCount As Long
  Dim lngBW As Long, lngWidth1 As Long, lngWidth2 As Long
  
  On Error GoTo ErrHandler
  
  //show custom msgbox
//vbOKOnly =              0 1
//vbOKCancel =            1 1-2
//vbAbortRetryIgnore =    2 1-2-3
//vbYesNoCancel =         3 1-2-3
//vbYesNo =               4 1-2
//vbRetryCancel =         5 1-2

//vbApplicationModal =     0
//vbSystemModal =     0x1000

//vbDefaultButton1 =       0
//vbDefaultButton2 =   0x100
//vbDefaultButton3 =   0x200
//vbDefaultButton4 =   0x300

//vbCritical =          0x10
//vbQuestion =          0x20
//vbExclamation =       0x30
//vbInformation =       0x40

//vbMsgBoxHelpButton =     0x4000
//vbMsgBoxSetForeground = 0x10000  //?
//vbMsgBoxRight =         0x80000  //right align the msg text
//vbMsgBoxRtlReading =   0x100000  //?
  
  
  Load frmDialog
  With frmDialog
    //pass along help info
    .HelpFile = HelpFile
    .HelpTopic = HelpTopic
    
    //need to account for border
    lngBW = .Width - .ScaleWidth
    
    //if showing help button...
    .cmdHelp.Visible = (Buttons And vbMsgBoxHelpButton)
    
    If (Buttons And vbSystemModal) Then
//      .StartUpPosition = vbStartUpScreen
    Else
//      .StartUpPosition = vbStartUpOwner
    End If
    
    //set title
    If IsMissing(Title) Then
      .Caption = App.Title
    Else
      .Caption = CStr(Title)
    End If
        
    //the checkstring (if visible) fit
    If .TextWidth(Prompt) > .TextWidth(CheckString) + 360 Then
      .message.Width = .TextWidth(Prompt)
    Else
      .message.Width = .TextWidth(CheckString) + 360
    End If
    
    //set height of msg prompt
    .message.Height = .TextHeight(Prompt)
    
    //add icons (if requested)
    If ((Buttons / 0x10) And 0x7) > 0 And ((Buttons / 0x10) And 0x7) <= 4 Then
      //load associated Image
      Set .Image1.Picture = .ImageList1.ListImages((Buttons / 0x10) And 0x7).Picture
      .Image1.Visible = true
      
      //adjust label position to account for icon
      .message.Left = .message.Left + 705
      
      //if text height of msg is <height of Image,
      If .message.Height < .Image1.Height Then
        //center it
        .message.Top = .message.Top + ((.Image1.Height - .message.Height) / 2 / ScreenTWIPSX) * ScreenTWIPSX
        
        //buttons are below icon
        .Button1.Top = .Image1.Top + .Image1.Height + 255
      Else
        //buttons are below msg
        .Button1.Top = .message.Top + .message.Height + 255
      End If
      
    Else
      //no icon; buttons are below msg
      .Button1.Top = .message.Top + .message.Height + 255
    End If
    
    //now set height
    .Height = (.Height - .ScaleHeight) + .Button1.Top + .Button1.Height + 165
    
    //save width based on msg size
    //so it can be compared to button size
    lngWidth1 = lngBW + .message.Left + .message.Width + 105
    
    //if checkmark is needed
    If LenB(CheckString) != 0 Then
      //position checkbox under msg
      .Check1.Left = .message.Left
      .Check1.Top = .Button1.Top + 180
      
      //move buttons down to account for checkbox
      .Button1.Top = .Button1.Top + 600
      //adjust dialog height to account for checkbox
      .Height = .Height + 600
      
      //set check properties based on passed parameters
      .Check1.Width = .TextWidth(CheckString) + 600
      .Check1.Caption = CheckString
      .Check1.Value = (vbChecked And Checked)
      
      //finally, show the checkbox
      .Check1.Visible = true
    End If
    
    //move other buttons to correct height
    .Button2.Top = .Button1.Top
    .Button3.Top = .Button1.Top
    .cmdHelp.Top = .Button1.Top
    
    //set message text
    .message.Caption = Prompt
    
    //set button captions
    Select Case Buttons And 0x7
    Case vbOKOnly
      .Button1.Caption = "OK"
      .Button1.Default = true
      
      lngButtonCount = 1
            
    Case vbOKCancel
      .Button1.Caption = "OK"
      .Button2.Caption = "Cancel"
      .Button2.Visible = true
      .Button2.Cancel = true
      
      .Button2.Default = (Buttons And vbDefaultButton2)
      
      lngButtonCount = 2
      
    Case vbAbortRetryIgnore
      .Button1.Caption = "&Abort"
      .Button2.Caption = "&Retry"
      .Button2.Visible = true
      .Button3.Caption = "&Ignore"
      .Button3.Visible = true
      
      .Button3.Default = (Buttons And vbDefaultButton3)
      .Button2.Default = (Buttons And vbDefaultButton2)
     
      lngButtonCount = 3
            
    Case vbYesNoCancel
      .Button1.Caption = "&Yes"
      .Button2.Caption = "&No"
      .Button2.Visible = true
      .Button3.Caption = "Cancel"
      .Button3.Cancel = true
      .Button3.Visible = true
      
      .Button3.Default = (Buttons And vbDefaultButton3)
      .Button2.Default = (Buttons And vbDefaultButton2)
      
      lngButtonCount = 3
      
    Case vbYesNo
      .Button1.Caption = "&Yes"
      .Button2.Caption = "&No"
      .Button2.Visible = true
      
      .Button2.Default = (Buttons And vbDefaultButton2)
           
      lngButtonCount = 2
      
    Case vbRetryCancel
      .Button1.Caption = "&Retry"
      .Button2.Caption = "Cancel"
      .Button2.Cancel = true
      .Button2.Visible = true
      
      .Button2.Default = (Buttons And vbDefaultButton2)
      
      lngButtonCount = 2
    End Select
    
    //if help button is visible,
    If (Buttons And vbMsgBoxHelpButton) Then
      lngButtonCount = lngButtonCount + 1
    End If
    
    //width needs to be wide enough for all buttons
    lngWidth2 = lngButtonCount * 1215 + 345

    //if width needed for buttons is wider than
    //width needed for text
    If lngWidth2 > lngWidth1 Then
      .Width = lngWidth2
      //button1 is right where it needs to be
    Else
      .Width = lngWidth1
      //move button1 to correct position based on button Count
      .Button1.Left = ((.Width - lngBW - (1125 + (lngButtonCount - 1) * 1215)) / 2 / ScreenTWIPSX) * ScreenTWIPSX
    End If
    
    //move other buttons based on button1 pos
    .Button2.Left = .Button1.Left + .Button1.Width + 90
    .Button3.Left = .Button2.Left + .Button2.Width + 90
    
    //if showing help button
    If (Buttons And vbMsgBoxHelpButton) Then
      //position it relative to button1, using button Count
      //to determine offset
      .cmdHelp.Left = .Button1.Left + (lngButtonCount - 1) * (.Button1.Width + 90)
    End If
    
    //show it
    frmDialog.Show vbModal, frmMDIMain
    
    //get result
    MsgBoxEx = frmDialog.Result
    //get check box status
    Checked = (frmDialog.Check1.Value = vbChecked)
    
    //unload the form
    Unload frmDialog
  End With
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub UpdateLayoutFile(ByVal Reason As EUReason, LogicNumber As Long, NewExits As AGIExits, Optional ByVal NewNum As Long)

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
  //entry, without doing anything else; while it//s possible
  //that extra, unnecessary renumbering actions can happen
  //with this approach, it//s worth it; it//s a lot harder
  //to examine the file, and try to figure out chains of
  //renumbering that might be simplified
  
  Dim intFile As Integer, stlLayout As StringList
  Dim strLine As String, strTempFile As String
  Dim i As Long, blnRoomVis As Boolean
  Dim lngRoomLine As Long, lngUpdateLine As Long
  Dim lngCode As Long, lngNum As Long
  
  On Error GoTo ErrHandler
  
  //it is possible that file might not exist; if a layout was extracted without
  //being saved, then an update to the layout followed by a call to view
  //a logic would get us here...
  If Not FileExists(GameDir + GameID + ".wal") Then
    Exit Sub
  End If
  
  //open layout file for input
  intFile = FreeFile()
  Open GameDir + GameID + ".wal" For Binary As intFile
  //get all the text
  strLine = String$(LOF(intFile), 0)
  Get intFile, 1, strLine
  //done with the file
  Close intFile
  
  //assign to stringlist
  Set stlLayout = New StringList
  stlLayout.Assign strLine
  If stlLayout(stlLayout.Count - 1) = "" Then
    stlLayout.Delete stlLayout.Count - 1
  End If
  strLine = ""
  
  lngRoomLine = -1
  lngUpdateLine = -1
  
  //if not renumbering, we need to find any current
  //room or update lines
  If Reason != euRenumberRoom Then
    //locate any existing room or update lines for this room
    For i = 0 To stlLayout.Count - 1
      If LenB(stlLayout(i)) != 0 Then
        If Asc(stlLayout(i)) = 85 And Val(Mid$(stlLayout(i), 3)) = LogicNumber Then //asc("U") = 85
          //this is the update line
          lngUpdateLine = i
        End If
        If Asc(stlLayout(i)) = 82 And Val(Mid$(stlLayout(i), 3)) = LogicNumber Then //asc("R") = 82
          //this is the room line
          lngRoomLine = i
          //determine if room is visible
          strLine = stlLayout(i)
          strLine = Right$(strLine, Len(strLine) - InStr(3, strLine, "|")) //strip off number
          //test remainder of line for //T// or //F//
          blnRoomVis = (Asc(strLine) = 84)
        End If
      End If
    Next i
      
    //should NEVER have room come before update
    If lngRoomLine != -1 And lngUpdateLine != -1 Then
      //Debug.Assert lngUpdateLine > lngRoomLine
    End If
    
    //ALWAYS delete update line, if there is one
    If lngUpdateLine != -1 Then
      //remove the line
      stlLayout.Delete lngUpdateLine
    End If
  End If
    
  Select Case Reason
  Case euAddRoom, euShowRoom, euUpdateRoom
    //create the new update line
    strLine = "U|" + CStr(LogicNumber) + "|true"
    
    //if new exits were passed,
    If Not NewExits Is Nothing Then
      //add exits
      For i = 0 To NewExits.Count - 1
        With NewExits(i)
          strLine = strLine + "|" + Right$(.ID, 3) + ":" + .Room + ":" + .Reason + ":" + .Style
        End With
      Next i
    End If
  
  Case euRemoveRoom
    //if there is NOT a visible room
    If Not blnRoomVis Then
      //dont need to update, cuz room is already hidden
    Else
      //create the new update line
      strLine = "U|" + CStr(LogicNumber) + "|false"
    End If
    
  Case euRenumberRoom
    //updates the layout file to indicate a room has changed its number
    //a file entry using the update code (N)
    //instead of the room code (R) indicates to the layout editor
    //that the room has changed
    
    //add the update line
    strLine = "N|" + CStr(LogicNumber) + "|" + CStr(NewNum)
    
  End Select
  
  If Len(strLine) > 0 Then
    //add the new update line
    stlLayout.Add strLine
  End If
  
  //create temp file
  strTempFile = TempFileName()
  
  //open temp file for output
  intFile = FreeFile()
  Open strTempFile For Binary As intFile
  
  //write data to the file
  Put intFile, 1, Join(stlLayout.AllLines, vbCr)
  //done with file
  Close intFile
  
  //kill old file, copy new
  On Error Resume Next
  //Debug.Assert GameID != ""
  Kill GameDir + GameID + ".wal"
  FileCopy strTempFile, GameDir + GameID + ".wal"
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Function AnalyzeExit(strSource As String, ByRef lngLoc As Long, Optional ByVal StartPos As Long = 0) As AGIExit

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
  
  Dim pos1 As Long, pos2 As Long, pos3 As Long
  Dim i As Long, j As Long, lngEnd As Long, blnGood As Boolean
  Dim intRoom As Integer, eReason As EEReason
  Dim intStyle As Integer, lngLen As Long
  Dim strLine As String
  
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
  If strLine = "(" Then
    //ok; next cmd should be the Value we are looking for
    strLine = ""
    FindNextToken strSource, i, strLine
  Else
    //if no parenthesis, set room arg to zero
    strLine = "0"
  End If
  
  intRoom = CInt(Val(Logics.ConvertArg(Trim$(strLine), atNum)))
  
  //validate room
  If intRoom < 0 Or intRoom > 255 Then
    intRoom = 0
  End If
  
  Do
    //next token should be //)//
    strLine = ""
    FindNextToken strSource, i, strLine
    If strLine != ")" Then
      //syntax not good
      Exit Do
    End If
    
    //and then next token should be //;//
    strLine = ""
    lngEnd = FindNextToken(strSource, i, strLine)
    If strLine = ";" Then
      //move ahead one char
      lngEnd = lngEnd + 1
      blnGood = true
    Else
      //syntax not good
      Exit Do
    End If
    
  Loop Until true
  
  //if syntax is bad, use end of line, or comment start
  If Not blnGood Then
    //if the next token was a comment, we need to back up so we can find it
    If Left$(strLine, 1) = "[" Or Left$(strLine, 2) = "//" Then
      lngEnd = lngEnd - 1
    //if the next token is end line,
    ElseIf strLine = vbCr Then
      
    Else
      //find the end, or a comment
      Do
        strLine = ""
        lngEnd = FindNextToken(strSource, i, strLine)
        If strLine = vbCr Then
          //end of line; exit
          Exit Do
        ElseIf Left$(strLine, 1) = "[" Or Left$(strLine, 2) = "//" Then
          //comment found; exit
          Exit Do
        End If
      Loop While lngEnd < lngLen And lngEnd != 0
    End If
    
  End If
  
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
      If pos3 < lngLoc And pos3 != 0 Then
        //update final //if// loc
        pos1 = pos3 + 1
      Else
        //done search
        Exit Do
      End If
    //if no //if// found, exit the loop
    Loop Until pos3 = 0
    
    //if no //if// found,
    If pos1 = 0 Then
      //this is an //other// exit
      eReason = erOther
      Exit Do
    End If
    
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
    If strLine != "(" Then
      //unknown
      eReason = erOther
      Exit Do
    End If
    
    //expecting //v2//
    strLine = ""
    FindNextToken strSource, pos1, strLine, true, true
    If Logics.ConvertArg(Trim$(strLine), atVar) != "v2" Then
      //other
      eReason = erOther
      Exit Do
    End If
    
    //expecting //==//
    strLine = ""
    FindNextToken strSource, pos1, strLine, true, true
    If strLine != "==" Then
      //other
      eReason = erOther
      Exit Do
    End If
  
    //expecting valid exit reason
    strLine = ""
    FindNextToken strSource, pos1, strLine, true, true
    i = Val(Logics.ConvertArg(Trim$(strLine), atNum))
    If i > 0 And i < 5 Then
      eReason = i
    Else
      eReason = erOther
    End If
    
    //expecting //)//
    strLine = ""
    FindNextToken strSource, pos1, strLine, true, true
    If strLine != ")" Then
      //not a simple edge code test; call it unknown
      eReason = erOther
      Exit Do
    End If
    
    //expecting "{"
    strLine = ""
    FindNextToken strSource, pos1, strLine, true, true
    If strLine != "{" Then
      //not a simple edge code test; call it unknown
      eReason = erOther
      Exit Do
    End If
    
    //expecting //new.room//
    strLine = ""
    FindNextToken strSource, pos1, strLine, true, true
    If strLine != "new.room" Then
      //complex
      //intStyle = 1 //***Style is currently not implemented, so ignore this for now
      //get next //new.room// cmd
      FindNextToken strSource, pos1, "new.room"
    End If
      
    //verify this new.room is same as one that started the analysis
    If lngLoc != pos1 - 7 Then
      eReason = erOther
    End If
    
  //always exit loop
  Loop Until true
  
  //Syle is currently not implemented leaving it at 0 is fine
  intStyle = 0
  
  //return exit object
  Set AnalyzeExit = New AGIExit
  With AnalyzeExit
    .Reason = eReason
    .Room = intRoom
    .Style = intStyle
    If Logics(intRoom).IsRoom Then
      .Status = esOK
    Else
      .Status = esHidden
    End If
  End With
  
  //return the end position to continue
  lngLoc = lngEnd
Exit Function

ErrHandler:
  Resume Next
End Function

public Sub NewWords()
  
  //creates a new word list file
  
  Dim frmNew As frmWordsEdit
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor
  
  //create new file
  Set frmNew = New frmWordsEdit
  Load frmNew
  
  frmNew.NewWords
  frmNew.Show
  
  //restore cursor while getting resnum
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  Resume Next
End Sub
public Function NoteToFreq(ByVal NoteIn As Long) As Long
  //converts a midinote into a freqdivisor Value
  
  //valid range of NoteIn is 45-127
  //note that 121, 124, 126 NoteIn values will actually return same freqdivisor
  //values as 120, 123, 127 respectively (due to loss of resolution in freq
  //values at the high end)
    
  Dim sngFreq As Single
  
  //validate input
  If NoteIn < 45 Then
    NoteIn = 45
  ElseIf NoteIn > 127 Then
    NoteIn = 127
  End If
  
  sngFreq = 111860# / 10 ^ ((NoteIn + 36.5) * LOG10_1_12)
  
  NoteToFreq = CLng(sngFreq)
End Function

public Function MIDILength(ByVal lngDuration As Long, ByVal lngTPQN As Long) As String
  //returns a string representation of midi note length
  
  Dim sngNoteLen As Single
  
  sngNoteLen = (lngDuration / lngTPQN) * 4
  
  Select Case sngNoteLen
  Case 1
    MIDILength = "1/16"
  Case 2
    MIDILength = "1/8"
  Case 3
    MIDILength = "1/8*"
  Case 4
    MIDILength = "1/4"
  Case 5
    MIDILength = "1/4 + 1/16"
  Case 6
    MIDILength = "1/4*"
  Case 7
    MIDILength = "1/4**"
  Case 8
    MIDILength = "1/2"
  Case 9
    MIDILength = "1/2 + 1/16"
  Case 10
    MIDILength = "1/2 + 1/8"
  Case 11
    MIDILength = "1/2 + 1/8*"
  Case 12
    MIDILength = "1/2*"
  Case 13
    MIDILength = "1/2* + 1/16"
  Case 14
    MIDILength = "1/2**"
  Case 15
    MIDILength = "1/2** + 1/16"
  Case 16
    MIDILength = "1"
  Case Else
    MIDILength = "<undefined>"
  End Select
End Function

public Function MIDINote(ByVal lngFreq As Long) As Long
  //converts AGI freq offset into a MIDI note
  Dim dblRawNote As Double
  
  //middle C is 261.6 HZ; from midi specs,
  //middle C is a note with a Value of 60
  //this requires a shift in freq of approx. 36.376
  //however, this offset results in crappy sounding music;
  //empirically, 36.5 seems to work best
  
  //note that since freq divisor is in range of 0 - 1023
  //resulting midinotes are in range of 45 - 127
  //and that midinotes of 126, 124, 121 are not possible (no corresponding freq divisor)
  
  
  //if freq divisor is 0
  If lngFreq <= 0 Then
    //set note to Max
    MIDINote = 127
  Else
  
    MIDINote = CLng((Log10(111860# / CDbl(lngFreq)) / LOG10_1_12) - 36.5)
    
    //validate
    If MIDINote < 0 Then
      MIDINote = 0
    End If
    //in case note is too high,
    If MIDINote > 127 Then
      MIDINote = 127
    End If
  End If
End Function

public Function NoteName(ByVal MIDINote As Byte, Optional ByVal Key As Long = 0) As String
  //returns a string character representing the midi note
  
  Dim lngOctave As Long
  Dim lngNote As Long
  
  Select Case MIDINote
//  Case 0
//    //zero
//    NoteName = "OFF"
  Case Is < 0
    //invalid low
    NoteName = "OFF" //"ERR_LO"
  Case Is > 127
    //invalid high
    NoteName = "ERR_HI"
  Case Else
    //get octave and note Value
    lngOctave = MIDINote / 12 - 1
    lngNote = MIDINote Mod 12
    
    Select Case lngNote
    Case 0
      NoteName = "C"
    Case 1
      NoteName = IIf(Sgn(Key) = -1, "Db", "C#")
    Case 2
      NoteName = "D"
    Case 3
      NoteName = IIf(Sgn(Key) = -1, "Eb", "D#")
    Case 4
      NoteName = "E"
    Case 5
      NoteName = "F"
    Case 6
      NoteName = IIf(Sgn(Key) = -1, "Gb", "F#")
    Case 7
      NoteName = "G"
    Case 8
      NoteName = IIf(Sgn(Key) = -1, "Ab", "G#")
    Case 9
      NoteName = "A"
    Case 10
      NoteName = IIf(Sgn(Key) = -1, "Bb", "A#")
    Case 11
      NoteName = "B"
    End Select
  End Select
End Function


public Function DisplayNote(ByVal MIDINote As Byte, ByVal Key As Long) As tDisplayNote
  //returns a note position, relative to middle c
  //as either a positive or negative Value (negative meaning higher tone)
  //
  //the Value is the offset needed to draw the note correctly
  //on the staff; one unit is a half the distance between staff lines
  //
  //the returned Value is adjusted based on the key
  
  Dim lngOctave As Long
  Dim lngNote As Long
  
  If MIDINote < 0 Or MIDINote > 127 Then
    Exit Function
  End If
  
  //get octave and note Value
  lngOctave = MIDINote / 12 - 1
  lngNote = MIDINote - ((lngOctave + 1) * 12)
  
  Select Case lngNote
  Case 0 // "-C"
    If Key = 7 Then
      DisplayNote.Pos = 1
      DisplayNote.Tone = ntNone
    Else
      DisplayNote.Pos = 0
      If (Key >= -5 And Key <= 1) Then
        DisplayNote.Tone = ntNone
      Else
        DisplayNote.Tone = ntNatural
      End If
    End If
    
  
  Case 1 // "-C#"
  If Key >= 0 Then
    DisplayNote.Pos = 0
  Else
    DisplayNote.Pos = -1
  End If
  Select Case Key
  Case 0, 1
    DisplayNote.Tone = ntSharp
  Case -1, -2, -3
    DisplayNote.Tone = ntFlat
  Case Else
    DisplayNote.Tone = ntNone
  End Select
  
  Case 2 // "-D"
    DisplayNote.Pos = -1
    Select Case Key
    Case Is > 3, Is < -3
      DisplayNote.Tone = ntNatural
    Case Else
      DisplayNote.Tone = ntNone
    End Select
    
  Case 3 // "-D#"
    If Key >= 0 Then
      DisplayNote.Pos = -1
    Else
      DisplayNote.Pos = -2
    End If
    Select Case Key
    Case 0 To 3
      DisplayNote.Tone = ntSharp
    Case -1
      DisplayNote.Tone = ntFlat
    Case Else
      DisplayNote.Tone = ntNone
    End Select
    
  Case 4 // "-E"
    If Key = -7 Then
      DisplayNote.Pos = -3
      DisplayNote.Tone = ntNone
    Else
      DisplayNote.Pos = -2
      If Key >= -1 And Key <= 5 Then
        DisplayNote.Tone = ntNone
      Else
        DisplayNote.Tone = ntNatural
      End If
    End If
    
  Case 5 // "-F"
    If Key >= 6 Then
      DisplayNote.Pos = -2
      DisplayNote.Tone = ntNone
    Else
      DisplayNote.Pos = -3
      If Key <= 0 And Key >= -6 Then
        DisplayNote.Tone = ntNone
      Else
        DisplayNote.Tone = ntNatural
      End If
    End If
    
  Case 6 // "-F#"
    If Key >= 0 Then
      DisplayNote.Pos = -3
    Else
      DisplayNote.Pos = -4
    End If
    Select Case Key
    Case 0
      DisplayNote.Tone = ntSharp
    Case -4 To -1
      DisplayNote.Tone = ntFlat
    Case Else
      DisplayNote.Tone = ntNone
    End Select
  
  Case 7 // "-G"
    DisplayNote.Pos = -4
    If Key >= 3 Or Key <= -5 Then
      DisplayNote.Tone = ntNatural
    Else
      DisplayNote.Tone = ntNone
    End If
    
  Case 8 // "-G#"
    If Key >= 0 Then
      DisplayNote.Pos = -4
    Else
      DisplayNote.Pos = -5
    End If
    Select Case Key
    Case 0 To 2
      DisplayNote.Tone = ntSharp
    Case -1, -2
      DisplayNote.Tone = ntFlat
    Case Else
      DisplayNote.Tone = ntNone
    End Select
    
  Case 9 // "-A"
    DisplayNote.Pos = -5
    If Key >= 5 Or Key <= -3 Then
      DisplayNote.Tone = ntNatural
    Else
      DisplayNote.Tone = ntNone
    End If
    
  Case 10 // "-A#"
    If Key >= 0 Then
      DisplayNote.Pos = -5
    Else
      DisplayNote.Pos = -6
    End If
    Select Case Key
    Case 0 To 4
      DisplayNote.Tone = ntSharp
    Case Else
      DisplayNote.Tone = ntNone
    End Select
    
  Case 11 // "-B"
    If Key <= -6 Then
      DisplayNote.Pos = -7
      DisplayNote.Tone = ntNone
    Else
      DisplayNote.Pos = -6
      If Key <= -1 Or Key = 7 Then
        DisplayNote.Tone = ntNatural
      Else
        DisplayNote.Tone = ntNone
      End If
    End If
  End Select
  
  //adust for octave
  DisplayNote.Pos = DisplayNote.Pos + (4 - lngOctave) * 7
End Function
public Sub DrawProp(picProps As PictureBox, ByVal PropID As String, ByVal PropValue As String, ByVal RowNum As Long, ByVal CanSelect As Boolean, ByVal SelectedProp As Long, ByVal PropScroll As Long, ByVal PropEnabled As Boolean, Optional ByVal ButtonFace As EButtonFace = bfNone)
                      
  Dim lngForeColor As Long
  Dim rtn As Long, blnIsSelected As Boolean
  
  //determine if this prop is selected
  blnIsSelected = (CanSelect And (SelectedProp = RowNum))
  
  RowNum = RowNum - PropScroll
  
  //if rownum is out of bounds
  If RowNum < 1 Or RowNum > (picProps.ScaleHeight - 2) / PropRowHeight Then
    Exit Sub
  End If
  
  //set forecolor depending on whether or not properties are enabled
  lngForeColor = IIf(PropEnabled, vbBlack, RGB(0x80, 0x80, 0x80))
   
  //strip off any multilines
  If InStr(1, PropValue, vbCr) != 0 Then
    PropValue = Left$(PropValue, InStr(1, PropValue, vbCr))
  End If
  If InStr(1, PropValue, vbLf) != 0 Then
    PropValue = Left$(PropValue, InStr(1, PropValue, vbLf))
  End If
  
  With picProps
    If blnIsSelected Then
     picProps.Line (1, PropRowHeight * RowNum)-(PropSplitLoc - 1, PropRowHeight * RowNum + PropRowHeight - 2), SelBlue, BF
    End If
  
    .CurrentX = 3
    .CurrentY = PropRowHeight * RowNum + 1
    .ForeColor = IIf(blnIsSelected, vbWhite, vbBlack)
    picProps.Print PropID;
    
    picProps.Line (PropSplitLoc, PropRowHeight * RowNum)-(picProps.Width, (RowNum + 1) * PropRowHeight), vbWhite, BF
    .CurrentY = PropRowHeight * RowNum + 1
    .CurrentX = PropSplitLoc + 3
    .ForeColor = lngForeColor
    picProps.Print PropValue
  
    //if this is selected property AND enabled,
    If blnIsSelected And PropEnabled Then
      Select Case ButtonFace
      Case 1  //drop down
        rtn = BitBlt(.hDC, .ScaleWidth - 17, RowNum * PropRowHeight, 17, 17, DropDownDC, 0, 0, SRCCOPY)
      Case 2  //drop over
        rtn = BitBlt(.hDC, .ScaleWidth - 17, RowNum * PropRowHeight, 17, 17, DropOverDC, 0, 0, SRCCOPY)
      Case 3  //drop dialog
        rtn = BitBlt(.hDC, .ScaleWidth - 17, RowNum * PropRowHeight, 17, 17, DropDlgDC, 0, 0, SRCCOPY)
      End Select
    End If
  End With
End Sub



public Function ChangeIntVersion(ByVal NewVersion As String) As Boolean

  Dim rtn As VbMsgBoxResult
  Dim strTemp As String
  Dim intFile As Integer
  
  //inline error checking
  On Error Resume Next
  
  //if change is major
  If Asc(NewVersion) != Asc(InterpreterVersion) Then
    //ask for confirmation
    rtn = MsgBox("Changing the target interpreter version may create problems" + vbCrLf + _
                 "with your logic resources, due to changes in the number of" + vbCrLf + _
                 "commands, and their argument counts." + vbNewLine + vbNewLine + _
                 "Also, your DIR and VOL files will need to be rebuilt to" + vbCrLf + _
                 "make a major version change. " + vbNewLine + vbNewLine + _
                 "Continue with version change?", vbQuestion + vbYesNo, _
                 "Change Interpreter Version")
    
    If rtn = vbNo Then
      //exit
      Exit Function
    End If
    
    //disable form until compile complete
    frmMDIMain.Enabled = false
    //show wait cursor
    WaitCursor
    
    //set up compile form
    Set CompStatusWin = New frmCompStatus
    CompStatusWin.SetMode 1 //rebuild only
    CompStatusWin.Show
    
    //setup and clear warning list
    frmMDIMain.ClearWarnings -1, 0
    
    //change the version
    InterpreterVersion = NewVersion
    
    //if major error,
    If Err.Number != 0 Then
      //display error message
      ErrMsgBox "Error during version change: ", "Original version has been restored.", "Change Interpreter Version"
       GoTo ErrHandler
    End If
    
    //check for cancel
    If CompStatusWin.CompCanceled Then
      GoTo ErrHandler
    End If
    
    //check for errors and warnings
    If CLng(CompStatusWin.lblErrors.Caption) + CLng(CompStatusWin.lblWarnings.Caption) > 0 Then
      //msgbox to user
      MsgBox "Errors and/or warnings were generated during game compile.", vbInformation + vbOKOnly, "Compile Game Error"
      
      //if errors
      If CLng(CompStatusWin.lblErrors.Caption) > 0 Then
        //reuild resource list
        BuildResourceTree
      End If
          
      If CLng(CompStatusWin.lblWarnings.Caption) > 0 Then
        If Not frmMDIMain.picWarnings.Visible Then
          frmMDIMain.ShowWarningList
        End If
      End If
    Else
      //everything is ok
      MsgBox "Version change and rebuild completed successfully.", _
             vbInformation + vbOKOnly, "Change Interpreter Version"
    End If
    
    //unload the compile staus form
    Unload CompStatusWin
    Set CompStatusWin = Nothing
    
    //restore form state
    frmMDIMain.Enabled = true
    Screen.MousePointer = vbDefault
  Else
    //ask for confirmation
    rtn = MsgBox("Changing the target interpreter version may create problems with your logic resources, " + _
          "due to changes in the number of commands, and their argument counts." + vbNewLine + vbNewLine + _
          "Continue with version change?", vbQuestion + vbYesNo, "Change Interpreter Version")
    
    If rtn = vbNo Then
      //exit
      Exit Function
    End If
    //just change the version
    InterpreterVersion = NewVersion
  End If
  
  //force wag file save/update
  ChangeIntVersion = true
Exit Function

ErrHandler:
  //unload the compile staus form
  Unload CompStatusWin
  Set CompStatusWin = Nothing

  //restore form state
  frmMDIMain.Enabled = true
  Screen.MousePointer = vbDefault
  
  //clean up any leftover files
  If Asc(NewVersion) = 51 >= 3 Then
    Kill GameDir + GameID + "DIR.NEW"
  Else
    Kill GameDir + "LOGDIR.NEW"
    Kill GameDir + "PICDIR.NEW"
    Kill GameDir + "SNDDIR.NEW"
    Kill GameDir + "VIEWDIR.NEW"
  End If
  Kill GameDir + "NEW_VOL.*"
  
  //clear errors
  Err.Clear
End Function

public Sub CompileAGIGame(Optional ByVal CompGameDir As String = "", Optional ByVal RebuildOnly As Boolean = false)

  Dim rtn As VbMsgBoxResult
  Dim strTemp As String
  Dim intFile As Integer
  Dim i As Long, blnDontAsk As Boolean
  
  On Error GoTo ErrHandler
  
  //if no game is loaded,
  If Not GameLoaded Then
    Exit Sub
  End If
  
  //if global editor or layout editor open and unsaved, ask to continue
  If GEInUse Then
    If GlobalsEditor.IsDirty Then
      strTemp = "Do you want to save the Global Defines list before compiling?"
    End If
  End If
  
  If LEInUse Then
    If LayoutEditor.IsDirty Then
      If Len(strTemp) != 0 Then
        //if globals is also open, then adjust message
        strTemp = "Do you want to save the Global Defines list and " + vbNewLine + _
                  "Layout Editor before compiling?"
      Else
        strTemp = "Do you want to save the Global Defines list before compiling?"
      End If
    End If
  End If
      
  If Len(strTemp) != 0 Then
    rtn = MsgBox(strTemp, vbYesNoCancel + vbQuestion, "Save Before Compile?")
    Select Case rtn
    Case vbYes
      If LEInUse Then
        If LayoutEditor.IsDirty Then
          LayoutEditor.MenuClickSave
        End If
      End If
      If GEInUse Then
        If GlobalsEditor.IsDirty Then
          GlobalsEditor.MenuClickSave
        End If
      End If
      
    Case vbCancel
      Exit Sub
    End Select
  End If
     
  //check for any open resources
  For i = 1 To LogicEditors.Count
    If LogicEditors(i).FormMode = fmLogic Then
      If LogicEditors(i).rtfLogic.Dirty Then
        //saveoncompile is in ask mode or yes mode
        If Settings.SaveOnCompile != 1 Then
          //if not automatic,
          If Settings.SaveOnCompile = 0 Then
            LogicEditors(i).SetFocus
            //get user//s response
            rtn = MsgBoxEx("Do you want to save this logic before compiling?", vbQuestion + vbYesNoCancel, "Update " + ResourceName(LogicEditors(i).LogicEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
            If blnDontAsk Then
              If rtn = vbYes Then
                Settings.SaveOnCompile = 2
              Else
                Settings.SaveOnCompile = 1
              End If
            End If
            
            //update settings list
            WriteAppSetting SettingsList, sLOGICS, "SaveOnComp", Settings.SaveOnCompile
            
          Else
            //if on automatic, always say yes
            rtn = vbYes
          End If
          
          Select Case rtn
          Case vbCancel
            Exit Sub
          Case vbYes
            //save it
            LogicEditors(i).MenuClickSave
          End Select
        End If
      End If
    End If
  Next i
  
  For i = 1 To PictureEditors.Count
    If PictureEditors(i).PicEdit.IsDirty Then
      //saveoncompile is in ask mode or yes mode
      If Settings.SaveOnCompile != 1 Then
        //if not automatic,
        If Settings.SaveOnCompile = 0 Then
          PictureEditors(i).SetFocus
          //get user//s response
          rtn = MsgBoxEx("Do you want to save this picture before compiling?", vbQuestion + vbYesNoCancel, "Update " + ResourceName(PictureEditors(i).PicEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
          If blnDontAsk Then
            If rtn = vbYes Then
              Settings.SaveOnCompile = 2
            Else
              Settings.SaveOnCompile = 1
            End If
          End If
        Else
          //if on automatic, always say yes
          rtn = vbYes
        End If
      
        Select Case rtn
        Case vbCancel
          Exit Sub
        Case vbYes
          //save it
          PictureEditors(i).MenuClickSave
        End Select
      End If
    End If
  Next
  
  For i = 1 To SoundEditors.Count
    If SoundEditors(i).SoundEdit.IsDirty Then
      //saveoncompile is in ask mode or yes mode
      If Settings.SaveOnCompile != 1 Then
        //if not automatic,
        If Settings.SaveOnCompile = 0 Then
          SoundEditors(i).SetFocus
          //get user//s response
          rtn = MsgBoxEx("Do you want to save this Sound before compiling?", vbQuestion + vbYesNoCancel, "Update " + ResourceName(SoundEditors(i).SoundEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
          If blnDontAsk Then
            If rtn = vbYes Then
              Settings.SaveOnCompile = 2
            Else
              Settings.SaveOnCompile = 1
            End If
          End If
        Else
          //if on automatic, always say yes
          rtn = vbYes
        End If
      
        Select Case rtn
        Case vbCancel
          Exit Sub
        Case vbYes
          //save it
          SoundEditors(i).MenuClickSave
        End Select
      End If
    End If
  Next
  
  For i = 1 To ViewEditors.Count
    If ViewEditors(i).ViewEdit.IsDirty Then
      //saveoncompile is in ask mode or yes mode
      If Settings.SaveOnCompile != 1 Then
        //if not automatic,
        If Settings.SaveOnCompile = 0 Then
          ViewEditors(i).SetFocus
          //get user//s response
          rtn = MsgBoxEx("Do you want to save this View before compiling?", vbQuestion + vbYesNoCancel, "Update " + ResourceName(ViewEditors(i).ViewEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
          If blnDontAsk Then
            If rtn = vbYes Then
              Settings.SaveOnCompile = 2
            Else
              Settings.SaveOnCompile = 1
            End If
          End If
        Else
          //if on automatic, always say yes
          rtn = vbYes
        End If
      
        Select Case rtn
        Case vbCancel
          Exit Sub
        Case vbYes
          //save it
          ViewEditors(i).MenuClickSave
        End Select
      End If
    End If
  Next
  
  If OEInUse Then
    If ObjectEditor.IsDirty Then
      //saveoncompile is in ask mode or yes mode
      If Settings.SaveOnCompile != 1 Then
        //if not automatic,
        If Settings.SaveOnCompile = 0 Then
          ObjectEditor.SetFocus
          //get user//s response
          rtn = MsgBoxEx("Do you want to save OBJECT file before compiling?", vbQuestion + vbYesNoCancel, "Update OBJECT File?", , , "Always take this action when compiling a game.", blnDontAsk)
          If blnDontAsk Then
            If rtn = vbYes Then
              Settings.SaveOnCompile = 2
            Else
              Settings.SaveOnCompile = 1
            End If
          End If
        Else
          //if on automatic, always say yes
          rtn = vbYes
        End If
      
        Select Case rtn
        Case vbCancel
          Exit Sub
        Case vbYes
          //save it
          ObjectEditor.MenuClickSave
        End Select
      End If
    End If
  End If
  
  If WEInUse Then
    If WordEditor.IsDirty Then
      //saveoncompile is in ask mode or yes mode
      If Settings.SaveOnCompile != 1 Then
        //if not automatic,
        If Settings.SaveOnCompile = 0 Then
          WordEditor.SetFocus
          //get user//s response
          rtn = MsgBoxEx("Do you want to save WORDS.TOK file before compiling?", vbQuestion + vbYesNoCancel, "Update WORDS.TOK File?", , , "Always take this action when compiling a game.", blnDontAsk)
          If blnDontAsk Then
            If rtn = vbYes Then
              Settings.SaveOnCompile = 2
            Else
              Settings.SaveOnCompile = 1
            End If
          End If
        Else
          //if on automatic, always say yes
          rtn = vbYes
        End If
      
        Select Case rtn
        Case vbCancel
          Exit Sub
        Case vbYes
          //save it
          WordEditor.MenuClickSave
        End Select
      End If
    End If
  End If
    
  //set default to replace any existing game files
  rtn = vbYes
  
  Do
    //if no directory was passed to the function
    If LenB(CompGameDir) = 0 Then
      //get a new dir
      CompGameDir = GetNewDir(frmMDIMain.hWnd, "Choose target directory for compiled game:")
      
      //if not canceled
      If LenB(CompGameDir) != 0 Then
        //if directory already contains game files,
        If Dir(cDir(CompGameDir) + "*VOL.*") != "" Then
          //verify
          rtn = MsgBox("This directory already contains AGI game files. Existing files will be renamed so they will not be lost. Continue with compile?", vbQuestion + vbYesNoCancel, "Compile Game")
          //if user said no
          If rtn = vbNo Then
            //reset directory
            CompGameDir = ""
          End If
        End If
      Else
        rtn = vbCancel
      End If
    End If
  Loop While rtn = vbNo
  
  //if canceled
  If rtn = vbCancel Then
    //exit
    Exit Sub
  End If
  
  //disable form until compile complete
  frmMDIMain.Enabled = false
  //show wait cursor
  WaitCursor
  
  //set up compile form
  Set CompStatusWin = New frmCompStatus
  CompStatusWin.MousePointer = vbArrow
  If RebuildOnly Then
    CompStatusWin.SetMode 1
  Else
    CompStatusWin.SetMode 0 //0 means full compile
  End If
  CompStatusWin.Show
  
  //ensure dir has trailing backslash
  CompGameDir = cDir(CompGameDir)
  
  //setup and clear warning list
  frmMDIMain.ClearWarnings -1, 0
  
  //compile the game
  On Error Resume Next
  CompileGame RebuildOnly, CompGameDir
  
  //hide compile status window while dealing with results
  CompStatusWin.Hide
  
  //if major error,
  If Err.Number != 0 Then
    //restore cursor
    Screen.MousePointer = vbDefault
    //display error message
    strTemp = "An error occurred while building game files. Original files have" + vbCrLf
    strTemp = strTemp + "been restored, but you should check all files to make sure nothing" + vbCrLf
    strTemp = strTemp + "was lost or corrupted."
    
    //Error Information:
    ErrMsgBox strTemp, "", IIf(RebuildOnly, "Rebuild VOL Files", "Compile Game Error")
    //show wait cursor again
    WaitCursor
    
    //unload the compile staus form
    Unload CompStatusWin
    Set CompStatusWin = Nothing
  
    //restore form state
    frmMDIMain.Enabled = true
    
    //clean up any leftover files
    If Asc(InterpreterVersion) = 51 Then
      Kill CompGameDir + GameID + "DIR.NEW"
    Else
      Kill CompGameDir + "LOGDIR.NEW"
      Kill CompGameDir + "PICDIR.NEW"
      Kill CompGameDir + "SNDDIR.NEW"
      Kill CompGameDir + "VIEWDIR.NEW"
    End If
    Kill CompGameDir + "NEW_VOL.*"
    
    //try to copy old files back - if this is a directory
    //with old copies of game files, then we should restore
    //them after an error
    //if this directory is new, and didnt have any game
    //files, then these commands won//t hurt anything -
    //nothing will be there to be deleted/copied
    
    //restore game files, if they got modified
    If Asc(InterpreterVersion) = 51 Then
      If FileExists(CompGameDir + GameID + "DIR.OLD") Then
        If FileLastMod(CompGameDir + GameID + "DIR.OLD") != FileLastMod(CompGameDir + GameID + "DIR") Then
          Kill CompGameDir + GameID + "DIR"
          FileCopy CompGameDir + GameID + "DIR.OLD", CompGameDir + GameID + "DIR"
        End If
      End If
    Else
      If FileExists(CompGameDir + "LOGDIR.OLD") Then
        If FileLastMod(CompGameDir + "LOGDIR.OLD") != FileLastMod(CompGameDir + "LOGDIR") Then
          Kill CompGameDir + "LOGDIR"
          FileCopy CompGameDir + "LOGDIR.OLD", CompGameDir + "LOGDIR"
        End If
      End If
      If FileExists(CompGameDir + "PICDIR.OLD") Then
        If FileLastMod(CompGameDir + "PICDIR.OLD") != FileLastMod(CompGameDir + "PICDIR") Then
          Kill CompGameDir + "PICDIR"
          FileCopy CompGameDir + "PICDIR.OLD", CompGameDir + "PICDIR"
        End If
      End If
      If FileExists(CompGameDir + "SNDDIR.OLD") Then
        If FileLastMod(CompGameDir + "SNDDIR.OLD") != FileLastMod(CompGameDir + "SNDDIR") Then
          Kill CompGameDir + "SNDDIR"
          FileCopy CompGameDir + "SNDDIR.OLD", CompGameDir + "SNDDIR"
        End If
      End If
      If FileExists(CompGameDir + "VIEWDIR.OLD") Then
        If FileLastMod(CompGameDir + "VIEWDIR.OLD") != FileLastMod(CompGameDir + "VIEWDIR") Then
          Kill CompGameDir + "VIEWDIR"
          FileCopy CompGameDir + "VIEWDIR.OLD", CompGameDir + "VIEWDIR"
        End If
      End If
    End If
    
    For i = 0 To 15
      If Asc(InterpreterVersion) = 51 Then
        //v3
        If FileExists(CompGameDir + GameID + "VOL." + CStr(i) + ".OLD") Then
          If FileLastMod(CompGameDir + GameID + "VOL." + CStr(i) + ".OLD") != FileLastMod(CompGameDir + GameID + "VOL." + CStr(i)) Then
            Kill CompGameDir + GameID + "VOL." + CStr(i)
            FileCopy CompGameDir + GameID + "VOL." + CStr(i) + ".OLD", CompGameDir + GameID + "VOL." + CStr(i)
          End If
        End If
      Else
        //v2
        If FileExists(CompGameDir + "VOL." + CStr(i) + ".OLD") Then
          If FileLastMod(CompGameDir + "VOL." + CStr(i) + ".OLD") != FileLastMod(CompGameDir + "VOL." + CStr(i)) Then
            Kill CompGameDir + "VOL." + CStr(i)
            FileCopy CompGameDir + "VOL." + CStr(i) + ".OLD", CompGameDir + "VOL." + CStr(i)
          End If
        End If
      End If
    Next i
    
    //clear err
    Err.Clear
    
    //restore cursor
    Screen.MousePointer = vbDefault
    //exit
    Exit Sub
  End If
  
  On Error GoTo ErrHandler
  
  //check for cancel
  If CompStatusWin.CompCanceled Then
  //need to only restore words/object if
  //compile was to another directory AND not just rebuilding
    On Error Resume Next
    If CompGameDir != GameDir And Not RebuildOnly Then
      //delete any new files
      Kill CompGameDir + "WORDS.TOK"
      Kill CompGameDir + "OBJECT"
      //restore old files
      Name CompGameDir + "WORDS.OLD" As CompGameDir + "WORDS.TOK"
      Name CompGameDir + "OBJECT.OLD" As CompGameDir + "OBJECT"
    End If
    //clean up any leftover files
    If Asc(InterpreterVersion) = 51 Then
      Kill CompGameDir + GameID + "DIR.NEW"
    Else
      Kill CompGameDir + "LOGDIR.NEW"
      Kill CompGameDir + "PICDIR.NEW"
      Kill CompGameDir + "SNDDIR.NEW"
      Kill CompGameDir + "VIEWDIR.NEW"
    End If
    Kill CompGameDir + "NEW_VOL.*"
    //clear errors
    Err.Clear
    
    //unload the compile staus form
    Unload CompStatusWin
    Set CompStatusWin = Nothing
  
    //restore form state
    frmMDIMain.Enabled = true
    Screen.MousePointer = vbDefault
    
    //exit
    Exit Sub
  End If
  
  On Error GoTo ErrHandler
  
  //check for errors and warnings
  If CLng(CompStatusWin.lblErrors.Caption) + CLng(CompStatusWin.lblWarnings.Caption) > 0 Then
    //restore cursor
    Screen.MousePointer = vbDefault
    
    //msgbox to user
    MsgBox "Warnings were generated during game compile.", vbInformation + vbOKOnly, "Compile Game"
    
    //show wait cursor again
    WaitCursor
    
    //if errors
    If CLng(CompStatusWin.lblErrors.Caption) > 0 Then
      //rebuild resource list
      BuildResourceTree
    End If
    
    If CLng(CompStatusWin.lblWarnings.Caption) > 0 Then
      If Not frmMDIMain.picWarnings.Visible Then
        frmMDIMain.ShowWarningList
      End If
    End If
  Else
    //restore cursor
    Screen.MousePointer = vbDefault
    
    //everything is ok
    MsgBox IIf(RebuildOnly, "Rebuild", "Compile") + " completed successfully.", _
           vbInformation + vbOKOnly, IIf(RebuildOnly, "Rebuild VOL Files", "Compile Game")
    
    //show wait cursor again
    WaitCursor
  End If
  
  //unload the compile staus form
  Unload CompStatusWin
  Set CompStatusWin = Nothing
  
  UpdateSelection rtLogic, SelResNum, umPreview Or umProperty
  
  //restore form state
  frmMDIMain.Enabled = true
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub
public Function CompileDirtyLogics(Optional ByVal NoMsg As Boolean = false) As Boolean

  Dim rtn As VbMsgBoxResult
  Dim blnDirtyResources As Boolean
  Dim i As Long, tmpLogic As AGILogic
  Dim strTemp As String
  Dim blnLoaded As Boolean
  Dim ErrString As String, strErrInfo() As String
  Dim blnDontAsk As Boolean, intFile As Integer
  Dim strWarnings() As String, blnErr As Boolean
  
  On Error GoTo ErrHandler
  
  //if no game is loaded,
  If Not GameLoaded Then
    Exit Function
  End If
  
  //check for any open logic resources
  For i = 1 To LogicEditors.Count
    If LogicEditors(i).FormMode = fmLogic Then
      If LogicEditors(i).rtfLogic.Dirty Then
        Select Case Settings.SaveOnCompile
        Case 0  //ask user for input
          LogicEditors(i).SetFocus
          //get user//s response
          rtn = MsgBoxEx("Do you want to save this logic before compiling?", vbQuestion + vbYesNoCancel, "Update " + ResourceName(LogicEditors(i).LogicEdit, true, true) + "?", , , "Always take this action when compiling a game.", blnDontAsk)
          If blnDontAsk Then
            If rtn = vbYes Then
              Settings.SaveOnCompile = 2
            ElseIf rtn = vbNo Then
              Settings.SaveOnCompile = 1
            End If
          End If
        
        Case 1  //no
          rtn = vbNo
          
        Case 2  //yes
          rtn = vbYes
        End Select
        
        Select Case rtn
        Case vbCancel
          Exit Function
        Case vbYes
          //save it
          LogicEditors(i).MenuClickSave
        End Select
      End If
    End If
  Next i
  
  //disable form until compile complete
  frmMDIMain.Enabled = false
  //show wait cursor
  WaitCursor
  
  //set up compile form
  Set CompStatusWin = New frmCompStatus
  CompStatusWin.MousePointer = vbArrow
  CompStatusWin.SetMode 2
  CompStatusWin.Show
  
  //setup and clear warning list
  frmMDIMain.ClearWarnings -1, 0

  //compile the logics
  For Each tmpLogic In Logics
    blnLoaded = tmpLogic.Loaded
    If Not blnLoaded Then
      tmpLogic.Load
    End If
    
    //checking this logic
    CompStatusWin.lblStatus.Caption = "Checking " + ResourceName(tmpLogic, true, true)
    CompStatusWin.pgbStatus.Value = CompStatusWin.pgbStatus.Value + 1
    CompStatusWin.Refresh
    SafeDoEvents
    
    On Error Resume Next
    If Not tmpLogic.Compiled Then
      //refresh caption
      CompStatusWin.lblStatus.Caption = "Compiling " + ResourceName(tmpLogic, true, true)
      //don't advance progress bar though; only caption gets updated
      CompStatusWin.Refresh
      SafeDoEvents
      //compile this logic
      tmpLogic.Compile
      
      //check for error
      Select Case Err.Number
      Case 0    //no error
      
      //compiler error
      Case vbObjectError + 635
        //get error string
        ErrString = Err.Description
        Err.Clear
        //extract error info
        strErrInfo = Split(ErrString, "|")
        
        With CompStatusWin
          .lblStatus.Caption = "Compile error"
          //need to increment error counter, and store this error
          .lblErrors.Caption = .lblErrors.Caption + 1
        End With
        
        //add it to warning list
        frmMDIMain.AddError strErrInfo(0), Val(Left(strErrInfo(2), 4)), Right(strErrInfo(2), Len(strErrInfo(2)) - 6), tmpLogic.Number, strErrInfo(1)
        If Not frmMDIMain.picWarnings.Visible Then
          frmMDIMain.ShowWarningList
        End If
        
        //determine user response to the error
        Select Case Settings.OpenOnErr
        Case 0 //ask
          //restore cursor before showing msgbox
          Screen.MousePointer = vbDefault
          //get user//s response
          rtn = MsgBoxEx("An error occurred while attempting to compile " + ResourceName(tmpLogic, true, true) + ":" + vbNewLine + vbNewLine _
                   + ErrString + vbNewLine + vbNewLine + "Do you want to open the logic at the location of the error?", vbQuestion + vbYesNo, "Update Logics?", , , "Always take this action when a compile error occurs.", blnDontAsk)
          //show wait cursor again
          WaitCursor
          If blnDontAsk Then
            If rtn = vbYes Then
              Settings.OpenOnErr = 2
            Else
              Settings.OpenOnErr = 1
            End If
          End If
          
        Case 1 //always yes
          rtn = vbYes
          
        Case 2 //always no
          rtn = vbNo
          //restore cursor before showing msgbox
          Screen.MousePointer = vbDefault
          MsgBox "An error in your code has been detected in logic //" + ResourceName(tmpLogic, true, true) + "//:" + vbNewLine + vbNewLine _
                     + "Line " + strErrInfo(0) + ", Error# " + strErrInfo(1), vbOKOnly + vbInformation, "Logic Compiler Error"
          //show wait cursor again
          WaitCursor
          
        End Select
        
        //if yes,
        If rtn = vbYes Then
          //set error info for this file
          SetError CLng(strErrInfo(0)), strErrInfo(2), tmpLogic.Number, strErrInfo(1)
          Err.Clear
          //sound a tone
          Beep
        End If
        
        //unload the logic
        If Not blnLoaded Then
          tmpLogic.Unload
        End If
        blnErr = true
        Exit For
      
      Case Else
        Screen.MousePointer = vbDefault
        //some other error
        ErrMsgBox "Error occurred during compilation: ", "", "Compile Error"
        WaitCursor
        blnErr = true
        Exit For
      End Select
      Err.Clear
    End If
    
    On Error GoTo ErrHandler
    
    //reset dirty status in resource list
    Select Case Settings.ResListType
    Case 1
      frmMDIMain.tvwResources.Nodes("l" + CStr(tmpLogic.Number)).ForeColor = vbBlack
    Case 2
      //only update if logics are listed
      If frmMDIMain.cmbResType.ListIndex = 1 Then
        frmMDIMain.lstResources.ListItems("l" + CStr(tmpLogic.Number)).ForeColor = vbBlack
      End If
    End Select
    
    //unload the logic
    If Not blnLoaded Then
      tmpLogic.Unload
    End If
    
    //next logic
  Next
  
  //if no error, finalize the compile operation
  If Not blnErr Then
    //restore cursor
    Screen.MousePointer = vbDefault
    CompStatusWin.Refresh
    //check for errors and warnings
    If CLng(CompStatusWin.lblErrors.Caption) + CLng(CompStatusWin.lblWarnings.Caption) > 0 Then
      //msgbox to user
      MsgBox "Errors and/or warnings were generated during logic compilation.", vbInformation + vbOKOnly, "Compile Logics"
      WaitCursor
      //if errors
      If CLng(CompStatusWin.lblErrors.Caption) > 0 Then
        //rebuild resource list
        BuildResourceTree
      End If
        
      If CLng(CompStatusWin.lblWarnings.Caption) > 0 Then
        If Not frmMDIMain.picWarnings.Visible Then
          frmMDIMain.ShowWarningList
        End If
      End If
    Else
      //everything is ok
      If Not NoMsg Then
        MsgBox "All logics compiled successfully.", vbInformation + vbOKOnly, "Compile Dirty Logics"
      End If
      WaitCursor
      //return true
      CompileDirtyLogics = true
    End If

    //update previewwin, if being used
    If Settings.ShowPreview Then
      If SelResType = rtLogic Then
        //redraw the preview
        PreviewWin.LoadPreview rtLogic, SelResNum
      End If
    End If
  End If
  
  //unload the compile staus form
  Unload CompStatusWin
  Set CompStatusWin = Nothing
  
  //restore form state
  frmMDIMain.Enabled = true
  Screen.MousePointer = vbDefault
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Function CompileLogic(LogicEditor As frmLogicEdit, ByVal LogicNumber As Long) As Boolean
  //compiles an ingame logic
  //assumes calling function has validated
  //the logic is in fact in a game
  
  Dim intFile As Integer, i As Long
  Dim strWarnings() As String, strErrInfo() As String
  Dim blnDontNotify As Boolean
  
  On Error Resume Next
  
  //set flag so compiling doesn//t cause unnecessary updates in preview window
  Compiling = true
  
  If Not LogicEditor Is Nothing Then
    //Debug.Assert frmMDIMain.ActiveForm Is LogicEditor
    
    //if source is not clean,
    If LogicEditor.rtfLogic.Dirty Then
      //first, save the source
      LogicEditor.MenuClickSave
    End If
  End If
  
  //clear warning list for this logic
  frmMDIMain.ClearWarnings LogicNumber, rtLogic
  frmMDIMain.fgWarnings.Refresh
  
  //unlike other resources, the ingame logic is referenced directly
  //when being edited; so, it//s possible that the logic might get closed
  //such as when changing which logic is being previewed;
  //SO, we need to make sure the logic is loaded BEFORE compiling
  If Not Logics(LogicNumber).Loaded Then
    //reload it!
    Logics(LogicNumber).Load
  End If
  
  //compile this logic
  Logics(LogicNumber).Compile
  
  //check for error
  Select Case Err.Number
  Case 0    //no error
    If Not LogicEditor Is Nothing Then
      LogicEditor.Tag = ResourceName(Logics(LogicNumber), true, true) + " successfully compiled."
      //update statusbar
      MainStatusBar.Panels("Status").Text = LogicEditor.Tag
    End If
    If Settings.NotifyCompSuccess Then
      MsgBoxEx "Logic successfully compiled.", vbInformation + vbOKOnly, "Compile Logic", , , "Don//t show this message again", blnDontNotify
      //save the setting
      Settings.NotifyCompSuccess = Not blnDontNotify
      //if hiding, update settings file
      If Not Settings.NotifyCompSuccess Then
        WriteAppSetting SettingsList, sGENERAL, "NotifyCompSuccess", Settings.NotifyCompSuccess
      End If
    End If
    //return true
    CompileLogic = true
  
  Case vbObjectError + 635
    //extract error info
    strErrInfo = Split(Err.Description, "|")
    //set error    info            linenum        logic        module
    SetError CLng(strErrInfo(0)), strErrInfo(2), LogicNumber, strErrInfo(1)
    Err.Clear
    //sound a tone
    Beep
    
    With frmMDIMain
      .AddError strErrInfo(0), Val(Left(strErrInfo(2), 4)), Right(strErrInfo(2), Len(strErrInfo(2)) - 6), LogicNumber, strErrInfo(1)
      If Not .picWarnings.Visible Then
        .ShowWarningList
      End If
    End With

    If Settings.NotifyCompFail Then
      //restore cursor when showing error message
      Screen.MousePointer = vbDefault
      
      MsgBoxEx "Error detected in source. Unable to compile this logic." + vbCrLf + vbCrLf + "ERROR  in line " + strErrInfo(0) + ": " + strErrInfo(2), vbExclamation + vbOKOnly + vbMsgBoxHelpButton, "Logic Compiler", WinAGIHelp, "htm\winagi\compilererrors.htm#" + Left(strErrInfo(2), 4), "Do not show this message again", blnDontNotify
      //save the setting
      Settings.NotifyCompFail = Not blnDontNotify
      //if now hiding update settings file
      If Not Settings.NotifyCompFail Then
        WriteAppSetting SettingsList, sGENERAL, "NotifyCompFail", Settings.NotifyCompFail
      End If
    End If
  
  Case vbObjectError + 618 //not in a game
    //should NEVER get here, but...
    MsgBox "Only logics that are in a game can be compiled.", vbInformation + vbOKOnly, "Compile Error"
  
  Case vbObjectError + 546  //no data to compile
    MsgBox "Nothing to compile!", vbInformation + vbOKOnly, "Compile Error"
    
  Case Else
    //some other error
    ErrMsgBox "Error occurred during compilation: ", "", "Compile Error"
  End Select
  Err.Clear
  
  If Not LogicEditor Is Nothing Then
    //copy it back
    LogicEditor.LogicEdit.SetLogic Logics(LogicNumber)
  End If
  
  //all done
  Compiling = false
End Function
public Function NewSourceName(ThisLogic As AGILogic, ByVal InGame As Boolean) As String
  //this ONLY gets a new name; it does not change
  //anything; not the ID, not the source file name;
  //calling function has to use the name given
  //here to do whatever is necessary to actually
  //save and update a logic source file and/or editor
  
  //there isn//t an //ExportLogicSource// method, because
  //managing source code separate from the actual
  //logic resource is tricky; it//s easier for
  //the logic editor and preview window to manage
  //exporting source separately
  //
  //but they both need a name, and that//s easy enough
  //to do as a separate function
  
  Dim rtn As VbMsgBoxResult
  Dim strFileName As String
  
  On Error GoTo ErrHandler
  
  //set up commondialog
  With MainSaveDlg
    If InGame Then
      .DialogTitle = "Export Source"
    Else
      .DialogTitle = "Save Source"
    End If
     //if logic already has a filename,
    If LenB(ThisLogic.SourceFile) != 0 Then
      //use it
      .FullName = ThisLogic.SourceFile
    Else
      //use default
      //if this is a filename,
      If InStr(1, ThisLogic.ID, ".") != 0 Then
        .FullName = ResDir + Left$(ThisLogic.ID, InStrRev(ThisLogic.ID, ".") - 1) + LogicSourceSettings.SourceExt
      Else
        .FullName = ResDir + ThisLogic.ID + LogicSourceSettings.SourceExt
      End If
    End If
    .Filter = "WinAGI Logic Source Files (*.lgc)|*.lgc|Text files (*.txt)|*.txt|All files (*.*)|*.*"
    If LCase$(Right$(.FullName, 4)) = ".txt" Then
       .FilterIndex = 2
     Else
       .FilterIndex = 1
     End If
    .DefaultExt = Right$(LogicSourceSettings.SourceExt, Len(LogicSourceSettings.SourceExt) - 1)
    .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
    .hWndOwner = frmMDIMain.hWnd
  End With
  
  Do
    On Error Resume Next
    MainSaveDlg.ShowSaveAs
    
    //if canceled,
    If Err.Number = cdlCancel Then
      //exit without doing anything
      Exit Function
    End If
    On Error GoTo ErrHandler
    
    //get filename
    strFileName = MainSaveDlg.FullName
    
    //if file exists,
    If FileExists(strFileName) Then
      //verify replacement
      rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
      
      If rtn = vbYes Then
        Exit Do
      ElseIf rtn = vbCancel Then
        Exit Function
      End If
    Else
      Exit Do
    End If
  Loop While true
  
  //pass back this name
  NewSourceName = strFileName
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Function ExportLogic(ByVal LogicNumber As Byte) As Boolean
  //exports this logic
  
  //Logics are different from other resources in that
  //they can only be exported if they are in a game;
  //therefore, the passed argument is the resource number of
  //the logic to be exported, NOT the actual logic itself
  //
  //NOTE that this exports the compiled logic resource;
  //to export source code, use the ExportLogicSource function
  
  Dim strFileName As String
  Dim rtn As VbMsgBoxResult
  Dim blnLoaded As Boolean
  
  On Error GoTo ErrHandler
  
  With MainSaveDlg
    //set cmndlg properties for logic source file
    .DialogTitle = "Export Logic File"
    .Filter = "Logic Files (*.agl)|*.agl|All files (*.*)|*.*"
    .FilterIndex = ReadSettingLong(SettingsList, sLOGICS, sEXPFILTER, 1)
    //set default name
    .DefaultExt = "agl"
    .FullName = ResDir + Logics(LogicNumber).ID + ".agl"
    
    .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
    .hWndOwner = frmMDIMain.hWnd
  End With
  
  On Error Resume Next
  Do
    MainSaveDlg.ShowSaveAs
    //if canceled
    If Err.Number = cdlCancel Then
      //exit without doing anything
      Exit Function
    End If
    
    //get file name
    strFileName = MainSaveDlg.FullName
    
    //if file exists,
    If FileExists(strFileName) Then
      //verify replacement
      rtn = MsgBox(MainSaveDlg.FileName + " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
      
      If rtn = vbYes Then
        Exit Do
      ElseIf rtn = vbCancel Then
        Exit Function
      End If
    Else
      Exit Do
    End If
  Loop While true
  
  //need to make sure it//s loaded
  blnLoaded = Logics(LogicNumber).Loaded
  If Not blnLoaded Then
    Logics(LogicNumber).Load
  End If
  //export the resource
  Logics(LogicNumber).Export strFileName
  
  //if error,
  If Err.Number != 0 Then
    ErrMsgBox "An error occurred while exporting this file: ", "", "Export File Error"
    Exit Function
  End If
  
  //unload if not previously loaded
  If Not blnLoaded Then
    Logics(LogicNumber).Unload
  End If
  
  ExportLogic = true
Exit Function

ErrHandler:
//  //Debug.Assert false
  Resume Next
End Function


public Sub AddToMRU(ByVal NewWAGFile As String)

  //if NewWAGFile is already in the list,
  //it is moved to the top;
  //otherwise, it is added to the top, and other
  //entries are moved down
  
  Dim i As Long, j As Long
  Dim strTemp As String
  
  For i = 1 To 4
    If NewWAGFile = strMRU(i) Then
      //move others down
      For j = i To 2 Step -1
        strMRU(j) = strMRU(j - 1)
        frmMDIMain.Controls("mnuGMRU" + CStr(j)).Caption = CompactPath(strMRU(j), 60)
      Next j
      //move to top of list
      strMRU(1) = NewWAGFile
      frmMDIMain.mnuGMRU1.Caption = CompactPath(NewWAGFile, 60)
      //we//re done
      Exit Sub
    End If
  Next i
  
  //not found;
  
  //move all entries down
  For j = 4 To 2 Step -1
    strMRU(j) = strMRU(j - 1)
    //if this entry is valid
    If LenB(strMRU(j)) != 0 Then
      //update menu
      frmMDIMain.Controls("mnuGMRU" + CStr(j)).Caption = CompactPath(strMRU(j), 60)
      frmMDIMain.Controls("mnuGMRU" + CStr(j)).Visible = true
    Else
      //hide it
      frmMDIMain.Controls("mnuGMRU" + CStr(j)).Visible = false
    End If
  Next j
  //add new item 1
  strMRU(1) = NewWAGFile
  frmMDIMain.mnuGMRU1.Caption = CompactPath(NewWAGFile, 60)
  frmMDIMain.mnuGMRU1.Visible = true
  //ensure bar is visible
  frmMDIMain.mnuGMRUBar.Visible = true
End Sub

public Sub BuildResourceTree()
  //builds the resource tree list
  //for the current open game
  
  Dim i As Long, tmpNode As Node
  
  On Error GoTo ErrHandler
  
  Select Case Settings.ResListType
  Case 0  // no tree
    Exit Sub
  
  Case 1 // treeview list
    //clear the treelist
    frmMDIMain.ClearResourceList
    
    //add the base nodes
    With frmMDIMain.tvwResources
      //if a game id was passed
      If LenB(GameID) != 0 Then
        //update root
        .Nodes(1).Text = GameID
        
        //add logics
        If Logics.Count > 0 Then
          For i = 0 To 255
            //if a valid resource
            If Logics.Exists(i) Then
              Set tmpNode = .Nodes.Add(sLOGICS, tvwChild, "l" + CStr(i), ResourceName(Logics(i), true))
              tmpNode.Tag = i
              //load source to set compiled status
              If Logics(i).Compiled Then
                tmpNode.ForeColor = vbBlack
              Else
                tmpNode.ForeColor = vbRed
              End If
            End If
          Next i
        End If
        
        If Pictures.Count > 0 Then
          For i = 0 To 255
            //if a valid resource
            If Pictures.Exists(i) Then
              .Nodes.Add(sPICTURES, tvwChild, "p" + CStr(i), ResourceName(Pictures(i), true)).Tag = i
            End If
          Next i
        End If
        
        If Sounds.Count > 0 Then
          For i = 0 To 255
            //if a valid resource
            If Sounds.Exists(i) Then
              .Nodes.Add(sSOUNDS, tvwChild, "s" + CStr(i), ResourceName(Sounds(i), true)).Tag = i
            End If
          Next i
        End If
        
        If Views.Count > 0 Then
          For i = 0 To 255
            //if a valid resource
            If Views.Exists(i) Then
              .Nodes.Add(sVIEWS, tvwChild, "v" + CStr(i), ResourceName(Views(i), true)).Tag = i
            End If
          Next i
        End If
      End If
    End With
    
  Case 2 //combo/list boxes
   //update root
   frmMDIMain.cmbResType.List(0) = GameID
   //select root
   frmMDIMain.cmbResType.ListIndex = 0
  End Select
  
  // always update the property window
  frmMDIMain.PaintPropertyWindow
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub ChangeGameDir(strNewGameDir As String, Optional Quiet As Boolean = false)
  //changes the game directory
  //strNewDir is validated prior to calling this method
  
  //changing directory disabled for now- it is just too complicated
  Exit Sub
  
//////  Dim blnOverwrite As Boolean, strFile As String, strOldGameDir As String
//////
//////  //verify that no resources are open-only allow directory to change
//////  //if ALL resources are closed
//////
//////  If GEInUse Or LEInUse Or LogicEditors.Count > 0 Or PictureEditors.Count > 0 Or SoundEditors.Count > 0 Or ViewEditors.Count > 0 Or OEInUse Or WEInUse Then
//////    MsgBox "Game Directory cannot be changed if any resources are being edited. This includes global defines, layout, and all in-game resources. Close all open editors and try again.", vbCritical + vbInformation + vbOKOnly, "Game Directory NOT Changed"
//////    Exit Sub
//////  End If
//////
//////  //save current dir value for later use
//////  strOldGameDir = GameDir
//////
//////  //check for files in destination
//////  If FileExists(strNewGameDir, vbDirectory) Then
//////    //check for files in this folder
//////    strFile = Dir(strNewGameDir + "*.*")
//////    Do Until Len(strFile) = 0
//////      strFile = Dir()
//////    Loop
//////
//////    If Len(strFile) > 0 Then
//////      //ask user if overwrite is ok
//////      If MsgBox("Existing files in new directory will be overwritten by " + vbNewLine + _
//////        "resources with same name. Do you want to continue?", vbQuestion + vbYesNo, "Change Resource Directory") = vbNo Then
//////        //exit
//////        Exit Sub
//////      End If
//////    End If
//////  Else
//////    //create it
//////    MkDir strNewGameDir
//////  End If
//////
//////  //show progress form
//////  Load frmProgress
//////  With frmProgress
//////    .Caption = "Changing Game Directory"
//////    .lblProgress = "Depending on size of game, this may take awhile. Please wait..."
//////    .pgbStatus.Visible = false
//////    .Show
//////    .Refresh
//////  End With
//////
//////  //show wait cursor
//////  WaitCursor
//////
//////  //catch any errors
//////  On Error GoTo ErrHandler
//////
//////  If CopyFolder(GameDir, strNewGameDir, true) Then
//////    //success
//////    MsgBox "all files moved"
//////  Else
//////    //failure
//////    MsgBox "not all files moved; check your directory carefully..."
//////  End If
//////
////// //reset browser start dir to this dir
//////  BrowserStartDir = strNewGameDir
//////
//////  //update the MRU menu
//////  RenameMRU GameFile, strNewGameDir + JustFileName(GameFile)
//////
//////  //change gamedir property (this also updates the gamefile and resource dir)
//////  GameDir = strNewGameDir
//////
//////  //done with progress form
//////  Unload frmProgress
//////
//////  If Not Quiet Then
//////    MsgBox "Done", vbOKOnly + vbInformation, "Change Game Directory"
//////  End If
//////
//////  //restore cursor
//////  Screen.MousePointer = vbDefault
//////Exit Sub
//////
//////ErrHandler:
//////  //warn user
//////  ErrMsgBox "Error occured during file copy: ", "", "Change Game Directory"
//////  Resume Next
//////
End Sub

Private Function FindInClosedLogics(ByVal FindText As String, ByVal FindDir As FindDirection, ByVal MatchWord As Boolean, _
                ByVal MatchCase As Boolean, Optional ByVal SearchType As AGIResType = rtNone) As Long

  //find next closed logic that has search text in it;
  //if found, return the logic number
  //if not found, return -1
  
  Dim i As Long
  Static LogNum As Long
  Dim vbcComp As VbCompareMethod
  Dim blnLoaded As Boolean
  
  On Error GoTo ErrHandler
  
  //if this is first time through
  If Not ClosedLogics Then
    //start with first logic (which sets ClosedLogics flag)
    LogNum = NextClosedLogic(-1)
    SearchLogCount = Logics.Count
    SearchLogVal = LogicEditors.Count
  Else
    LogNum = NextClosedLogic(LogNum)
    //search logic count and value are current
  End If
  
  //show progress form
  Load frmProgress
  With frmProgress
    .Caption = "Find"
    If LogNum = -1 Then
      .lblProgress.Caption = "Searching..."
    Else
      .lblProgress.Caption = "Searching " + Logics(LogNum).ID + "..."
    End If
    .pgbStatus.Max = SearchLogCount
    .pgbStatus.Value = SearchLogVal //- 1
    .Show vbModeless, frmMDIMain
    .Refresh
  End With
  //Debug.Assert Screen.MousePointer != vbDefault
  
  //set comparison method for string search,
  vbcComp = CLng(MatchCase) + 1 // CLng(true) + 1 = 0 = vbBinaryCompare; Clng(false) + 1 = 1 = vbTextCompare
  
  Do Until LogNum = -1
    //update the progress form
    With frmProgress
      .pgbStatus.Value = .pgbStatus.Value + 1
      .lblProgress.Caption = "Searching " + Logics(LogNum).ID + "..."
      .Refresh
    End With
    
    //if not loaded,
    blnLoaded = Logics(LogNum).Loaded
    If Not blnLoaded Then
      Logics(LogNum).Load
    End If
    
    //if searching up,
    //if direction is up,
    If FindDir = fdUp Then
      If MatchWord Then
        If FindWholeWord(1, Logics(LogNum).SourceText, FindText, MatchCase, true, SearchType) != 0 Then
          Exit Do
        End If
      Else
        If InStrRev(Logics(LogNum).SourceText, FindText, -1, vbcComp) != 0 Then
          Exit Do
        End If
      End If
    Else
      //search strategy depends on synonym search value
      If Not GFindSynonym Then
        If MatchWord Then
          If FindWholeWord(1, Logics(LogNum).SourceText, FindText, MatchCase, false, SearchType) != 0 Then
            Exit Do
          End If
        Else
          If InStr(1, Logics(LogNum).SourceText, FindText, vbcComp) != 0 Then
            Exit Do
          End If
        End If
      Else
        //Matchword is always true; but since words are surrounded by quotes, it wont matter
        //so we use Instr
        
        //step through each word in the word group; if the word is found in this logic,
        //then we stop
        For i = 0 To WordEditor.WordsEdit.GroupN(GFindGrpNum).WordCount - 1
          If InStr(1, Logics(LogNum).SourceText, QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).Word(i) + QUOTECHAR, vbcComp) != 0 Then
            Exit Do
          End If
        Next i
      End If
    End If
    
    //not found-unload this logic
    If Not blnLoaded Then
      Logics(LogNum).Unload
    End If
    
    //try next logic
    LogNum = NextClosedLogic(LogNum)
    
  //loop is exited by finding the searchtext or reaching end of search area
  Loop
  
  //if not found,
  If LogNum = -1 Then
    //just exit
    FindInClosedLogics = LogNum
    //done with progress form; save current searchlog value
    SearchLogVal = frmProgress.pgbStatus.Value
    Unload frmProgress
    Exit Function
  End If
  
  If Not blnLoaded Then
    Logics(LogNum).Unload
  End If
    
  //open editor, if able (this will reset the cursor to normal;
  //we need to force it back to hourglass
  //Debug.Assert Screen.MousePointer = vbHourglass
  OpenLogic LogNum, true
  //Debug.Assert Screen.MousePointer = vbDefault
  Screen.MousePointer = vbHourglass
  
  //now locate this window
  For i = LogicEditors.Count To 1 Step -1
    If LogicEditors(i).FormMode = fmLogic Then
      If LogicEditors(i).LogicEdit.Number = LogNum Then
        Exit For
      End If
    End If
  Next i
  
  //if not found (i = 0)
  If i = 0 Then
    //must have been an error
    MsgBox QUOTECHAR + FindText + QUOTECHAR + " was found in logic " + CStr(LogNum) + " but an error occurred while opening the file. Try opening the logic manually and then try the search again.", vbInformation, "Find In Logic"
    //unload logic
    Logics(LogNum).Unload
    //return -1
    FindInClosedLogics = -1
    //done with progress form
    SearchLogVal = frmProgress.pgbStatus.Value
    Unload frmProgress
    Exit Function
  End If
  
  //hide progress form
  SearchLogVal = frmProgress.pgbStatus.Value
  Unload frmProgress
  
  //return this window number
  FindInClosedLogics = i
  
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub NewAGIGame(ByVal UseTemplate As Boolean)
  
  Dim blnClosed As Boolean
  Dim blnNoErrors As Boolean
  Dim lngErr As Long, strErr As String
  Dim strVer As Single, strDescription As String
  Dim strTemplateDir As String, i As Long
  
  On Error GoTo ErrHandler
  
  //if using a template
  If UseTemplate Then
    //have user choose a template
    Load frmTemplate
    With frmTemplate
      .SetForm
      //if no templates available,
      If .lstTemplates.ListCount = 0 Then
        MsgBoxEx "There are no templates available. Unable to create new game.", vbCritical + vbOKOnly + vbMsgBoxHelpButton, "No Templates Available", WinAGIHelp, "htm\winagi\Templates.htm"
        Unload frmTemplate
        Exit Sub
      End If
      
      .Show vbModal, frmMDIMain
      If .Canceled Then
        Unload frmTemplate
        Exit Sub
      End If
      strTemplateDir = App.Path + "\Templates\" + .lstTemplates.Text
      strDescription = .txtDescription.Text
      strVer = .lblVersion.Caption
      Unload frmTemplate
    End With
  End If
  
  Load frmGameProperties
  With frmGameProperties
    //show new game form
    .WindowFunction = gsNew
    .SetForm
    
    //if using template
    If UseTemplate Then
      //version is preset based on template
      For i = 0 To .cmbVersion.ListCount - 1
        If .cmbVersion.List(i) = strVer Then
          .cmbVersion.ListIndex = i
          Exit For
        End If
      Next i
      .cmbVersion.Enabled = false
      //default description
      .txtGameDescription.Text = strDescription
    End If
    
    .Show vbModal, frmMDIMain
  
    //if canceled
    If .Canceled Then
      //unload it
      Unload frmGameProperties
      //exit
      Exit Sub
    End If
    
    //if word or Objects Editor open
    If WEInUse Then
      Unload WordEditor
      
      //if user canceled,
      If WEInUse Then
        Exit Sub
      End If
    End If
    If OEInUse Then
      Unload ObjectEditor
      If OEInUse Then
        Exit Sub
      End If
    End If
  
    //if a game is currently open,
    If GameLoaded Then
      //close game, if user allows
      If Not CloseThisGame() Then
        Exit Sub
      End If
    End If
    
    //show wait cursor
    WaitCursor

    //inline error checking
    On Error Resume Next
    
    //create new game  (newID, version, directory resouredirname and template info)
    NewGame .txtGameID.Text, .cmbVersion.Text, .DisplayDir, .txtResDir.Text, strTemplateDir
    
    //check for errors or warnings
    lngErr = Err.Number
    strErr = Err.Description
    
    On Error GoTo ErrHandler
    If lngErr = 0 Or lngErr = vbObjectError + 637 Then
      //add rest of properties
      GameDescription = .txtGameDescription.Text
      GameAuthor = .txtGameAuthor.Text
      GameVersion = .txtGameVersion.Text
      GameAbout = .txtGameAbout.Text
      //set platform type if a file was provided
      If Len(.NewPlatformFile) > 0 Then
        If .optDosBox.Value Then
          PlatformType = 1
          DosExec = .txtExec.Text
          PlatformOpts = .txtOptions.Text
        ElseIf .optScummVM.Value Then
          PlatformType = 2
          PlatformOpts = .txtOptions.Text
        ElseIf .optNAGI.Value Then
          PlatformType = 3
        ElseIf .optOther.Value Then
          PlatformType = 4
          PlatformOpts = .txtOptions.Text
        End If
      Else
        PlatformType = 0
      End If
      If PlatformType > 0 Then
        Platform = .NewPlatformFile
      End If
      
      //resdef
      LogicSourceSettings.UseReservedNames = (.chkUseReserved.Value = vbChecked)
      
      //layout editor
      UseLE = (.chkUseLE.Value = vbChecked)
      
      //force a save of the property file
      SaveProperties
      
      //created ok; maybe with warning
      frmMDIMain.Caption = "WinAGI GDS - " + GameID
      
      //if there is a layout file
      If LenB(Dir(GameDir + "*.wal")) != 0 Then
        Name GameDir + Dir(GameDir + "*.wal") As GameDir + GameID + ".wal"
      End If
      
      If Settings.ShowPreview Then
        PreviewWin.ClearPreviewWin
      End If
      
      //build resource treelist
      BuildResourceTree
      
      Select Case Settings.ResListType
      Case 1
        //select root
        frmMDIMain.tvwResources.Nodes(1).Selected = true
        //force update
        frmMDIMain.SelectResource rtGame, -1
      Case 2
        //select root
        frmMDIMain.cmbResType.ListIndex = 0
        //force update
        frmMDIMain.SelectResource rtGame, -1
      End Select
      
      //set default directory
      BrowserStartDir = GameDir
  
      //add wagfile to mru
      AddToMRU GameDir + GameID + ".wag"
      
      //adjust menus
      AdjustMenus rtGame, true, false, false
      
      //show preview window
      If Settings.ShowPreview Then
        PreviewWin.Show
      End If
      //show resource tree
      If Settings.ResListType != 0 Then
        frmMDIMain.ShowResTree
      End If
      
      //if warnings
      If lngErr = vbObjectError + 637 Then
        //warn about errors
        MsgBox "Some minor errors occurred during game creation. See errlog.txt in the game directory for details.", vbInformation, "Errors During Load"
      End If
    Else
      MsgBox "Unable to create new game due to an error: " + vbNewLine + vbNewLine + strErr, vbInformation + vbOKOnly, "New AGI Game Error"
    End If
    
    //unload game properties form
    Unload frmGameProperties
    
    //reset cursor
    Screen.MousePointer = vbDefault
  End With
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

Private Function NextClosedLogic(ByVal OldLogNum As Long) As Long

  Static LogWin() As Long
  Static WinCount As Long
  Dim Closed As Boolean
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  //if this is first time through
  If Not ClosedLogics Then
    //build list of windows
    WinCount = LogicEditors.Count
    ReDim LogWin(WinCount)
    For i = 1 To WinCount
      //assume this window is not a logic in a game
      LogWin(i) = -1
      //if a logic editor
      If LogicEditors(i).FormMode = fmLogic Then
        //if in a game
        If LogicEditors(i).InGame Then
          //store this number
          LogWin(i) = LogicEditors(i).LogicNumber
        End If
      End If
    Next i
    //set flag
    ClosedLogics = true
  End If
  
  //increment old log number
  OldLogNum = OldLogNum + 1
    
  //use do loop to find next closed logic
  //try until end of logics reached
  Do Until OldLogNum = 256
    //if this number is a valid logic
    If Logics.Exists(OldLogNum) Then
      //assume closed
      Closed = true
      
      //check to see if logic is not already open
      For i = 1 To WinCount
        //if editor(i) is for this logic
        If LogWin(i) = OldLogNum Then
          //it//s not closed; it//s open for editing
          Closed = false
          Exit For
        End If
      Next i
      //if good, then exit
      If Closed Then
        Exit Do
      End If
    Else
      //not a valid logic;
      Closed = false
    End If
    
    //increment old log number
    OldLogNum = OldLogNum + 1
  Loop
  
  //if end reached,
  If OldLogNum = 256 Then
    //return end code (-1)
    NextClosedLogic = -1
  Else
    //return the number
    NextClosedLogic = OldLogNum
  End If
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function

public Sub OpenLayout()
  
  If Not GameLoaded Then
    Exit Sub
  End If
  
  //if layout editor is currently open
  If LEInUse Then
    //just bring it in focus
    LayoutEditor.SetFocus
    //if minimized
    If LayoutEditor.WindowState = vbMinimized Then
      //restore it
      LayoutEditor.WindowState = vbNormal
    End If
    
  Else
    //open the layout for the current game
    Set LayoutEditor = New frmLayout
    LayoutEditor.LoadLayout
  End If
End Sub

public Sub OpenMRUGame(ByVal Index As Long)
  
  Dim i As Long
  
  //if a game is currently open,
  If GameLoaded Then
    //close game, if user allows
    If Not CloseThisGame() Then
      Exit Sub
    End If
  End If
  
  //skip if this MRU is blank (probably due to user manually editing
  //the config file)
  If Len(strMRU(Index)) > 0 Then
    //attempt to open this game
    OpenWAG strMRU(Index)
  End If
  
  //if not successful
  If Not GameLoaded Then
    //step through previous mru//s
    For i = Index + 1 To 4
      //move this mru entry up
      strMRU(i - 1) = strMRU(i)
      //if blank
      If LenB(strMRU(i)) = 0 Then
        //hide this mru item
        frmMDIMain.Controls("mnuGMRU" + CStr(i - 1)).Visible = false
      Else
        //change this mru item
        frmMDIMain.Controls("mnuGMRU" + CStr(i - 1)).Caption = CompactPath(strMRU(i), 60)
      End If
    Next i
    //remove last entry
    strMRU(4) = ""
    frmMDIMain.mnuGMRU4.Visible = false
    
    //if none left
    If LenB(strMRU(1)) = 0 Then
      //hide bar too
      frmMDIMain.mnuGMRUBar.Visible = false
    End If
  Else
    //reset browser start dir to this dir
    BrowserStartDir = JustPath(strMRU(Index))
  End If
End Sub

public Function OpenWords(Optional ByVal InGame As Boolean = true) As Boolean

  Dim strFileName As String
  Dim intFile As Integer
  Dim frmNew As frmWordsEdit, tmpForm As Form
  
  On Error GoTo ErrHandler
  
  //if a game is loaded AND looking for ingame editor
  If GameLoaded And InGame Then
    //if it is in use,
    If WEInUse Then
      //just switch to it
      WordEditor.SetFocus
    Else
      //load it
      WEInUse = true
      Set WordEditor = New frmWordsEdit
      Load WordEditor
      //set ingame status first, so caption will indicate correctly
      WordEditor.InGame = true
      
      On Error Resume Next
      WordEditor.LoadWords VocabularyWords.ResFile
      If Err.Number != 0 Then
        Set WordEditor = Nothing
        WEInUse = false
        Exit Function
      End If
      On Error GoTo ErrHandler
      WordEditor.Show
    End If
  Else
  //either a game is NOT loaded, OR we are forcing a load from file
    
    //get an word file
    With MainDialog
      .Flags = cdlOFNHideReadOnly
      .DialogTitle = "Import Words File"
      .Filter = "AGI Word file|WORDS.TOK|WinAGI Words file (*.agw)|*.agw|All files (*.*)|*.*"
      .DefaultExt = ""
      .FilterIndex = ReadSettingLong(SettingsList, "Words", sOPENFILTER, 1)
      .FileName = ""
      .InitDir = DefaultResDir
      
      .ShowOpen
      strFileName = .FileName
      //save filter
      WriteAppSetting SettingsList, "Words", sOPENFILTER, .FilterIndex
      DefaultResDir = JustPath(.FileName)

    End With
    
    //check if already open
    For Each tmpForm In Forms
      If tmpForm.Name = "frmWordsEdit" Then
        If tmpForm.WordsEdit.ResFile = strFileName And Not tmpForm.InGame Then
          //just shift focus
          tmpForm.SetFocus
          OpenWords = true
          Exit Function
        End If
      End If
    Next
    
    //not open yet; create new form
    //and open this form into it
    Set frmNew = New frmWordsEdit
    
    Load frmNew
    On Error Resume Next
    frmNew.LoadWords strFileName
    If Err.Number != 0 Then
      Set frmNew = Nothing
      Exit Function
    Else
      frmNew.Show
    End If
  End If
  
  OpenWords = true
Exit Function

ErrHandler:
  //if user canceled the dialogbox,
  If Err.Number = cdlCancel Then
    Exit Function
  End If
  
  //Debug.Assert false
  Resume Next
End Function

public Function OpenObjects(Optional ByVal InGame As Boolean = true) As Boolean

  Dim strFileName As String
  Dim intFile As Integer
  Dim frmNew As frmObjectEdit, tmpForm As Form
  
  On Error GoTo ErrHandler
  
  //if a game is loaded AND looking for ingame editor
  If GameLoaded And InGame Then
    //if it is in use,
    If OEInUse Then
      //just switch to it
      ObjectEditor.SetFocus
    Else
      //load it
      OEInUse = true
      Set ObjectEditor = New frmObjectEdit
      Load ObjectEditor
      ObjectEditor.InGame = true
      
      //set ingame status first, so caption will indicate correctly
      On Error Resume Next
      ObjectEditor.LoadObjects InventoryObjects.ResFile
      If Err.Number != 0 Then
        Set ObjectEditor = Nothing
        OEInUse = false
        Exit Function
      End If
      On Error GoTo ErrHandler
      ObjectEditor.Show
      ObjectEditor.fgObjects.SetFocus
    End If
  Else
  //either a game is NOT loaded, OR we are forcing a load from file
    
    //get an object file
    With MainDialog
      .Flags = cdlOFNHideReadOnly
      .DialogTitle = "Import Object File"
      .Filter = "WinAGI Objects file (*.ago)|*.ago|AGI OBJECT file|OBJECT|All files (*.*)|*.*"
      .DefaultExt = ""
      .FilterIndex = ReadSettingLong(SettingsList, "Objects", sOPENFILTER, 2)
      .FileName = ""
      .InitDir = DefaultResDir
      
      .ShowOpen
      
      strFileName = .FileName
      
      //save filter
      WriteAppSetting SettingsList, "Objects", sOPENFILTER, .FilterIndex
      DefaultResDir = JustPath(.FileName)
      
    End With
    
    //check if already open
    For Each tmpForm In Forms
      If tmpForm.Name = "frmObjectEdit" Then
        If tmpForm.ObjectsEdit.ResFile = strFileName And Not tmpForm.InGame Then
          //just shift focus
          tmpForm.SetFocus
          OpenObjects = true
          Exit Function
        End If
      End If
    Next
    
    //not open yet; create new form
    //and open this form into it
    Set frmNew = New frmObjectEdit
    
    Load frmNew
    On Error Resume Next
    frmNew.LoadObjects strFileName
    If Err.Number != 0 Then
      Set frmNew = Nothing
      Exit Function
    Else
      frmNew.Show
      frmNew.fgObjects.SetFocus
    End If
  End If
  
  OpenObjects = true
Exit Function

ErrHandler:
  //if user canceled the dialogbox,
  If Err.Number = cdlCancel Then
    Exit Function
  End If
  
  Resume Next
End Function


public Sub RemoveLogic(ByVal LogicNum As Byte)
  //removes a logic from the game, and updates
  //preview and resource windows
  //
  //it also updates layout editor, if it is open
  //and deletes the source code file from source directory
  
  Dim i As Long, strSourceFile As String
  Dim blnIsRoom As Boolean
  
  On Error GoTo ErrHandler
  
  //need to load logic to access sourccode
  //Debug.Assert Logics.Exists(LogicNum)
  If Not Logics.Exists(LogicNum) Then
    //raise error
    On Error GoTo 0: Err.Raise vbObjectError + 501, "ResMan", "Invalid Logic number passed to RemoveLogic (logic does not exist)"
    Exit Sub
  End If
  
  If Not Logics(LogicNum).Loaded Then
    Logics(LogicNum).Load
  End If
  strSourceFile = Logics(LogicNum).SourceFile
  
  blnIsRoom = Logics(LogicNum).IsRoom
  
  //remove it from game
  Logics.Remove LogicNum
  
  //if using layout editor AND is a room,
  If UseLE And blnIsRoom Then
    //update layout editor and layout data file to show this room is now gone
    UpdateExitInfo euRemoveRoom, LogicNum, Nothing
  End If
  
  Select Case Settings.ResListType
  Case 1
    With frmMDIMain.tvwResources
      //remove it from resource list
      .Nodes.Remove .Nodes("l" + CStr(LogicNum)).Index
      
      //update selection to whatever is now the selected node
      frmMDIMain.LastIndex = -1
      
      If .SelectedItem.Parent Is Nothing Then
        //it//s the game node
        frmMDIMain.SelectResource rtGame, -1
      ElseIf .SelectedItem.Parent.Parent Is Nothing Then
        //it//s a resource header
        frmMDIMain.SelectResource .SelectedItem.Index - 2, -1
      Else
        //it//s a resource
        frmMDIMain.SelectResource .SelectedItem.Parent.Index - 2, CLng(.SelectedItem.Tag)
      End If
    End With
    
  Case 2
    //only need to remove if logics are listed
    If frmMDIMain.cmbResType.ListIndex = 1 Then
      //remove it
      frmMDIMain.lstResources.ListItems.Remove frmMDIMain.lstResources.ListItems("l" + CStr(LogicNum)).Index
      //use click event to update
      frmMDIMain.lstResources_Click
      frmMDIMain.lstResources.SelectedItem.Selected = true
    End If
  End Select
  
  //if an editor is open
  For i = 1 To LogicEditors.Count
    If LogicEditors(i).FormMode = fmLogic Then
      If LogicEditors(i).InGame And LogicEditors(i).LogicNumber = LogicNum Then
        //set number to -1 to force close
        LogicEditors(i).LogicNumber = -1
        //close it
        Unload LogicEditors(i)
        Exit For
      End If
    End If
  Next i
  
  //disposition any existing resource file
  If FileExists(strSourceFile) Then
    KillCopyFile strSourceFile, Settings.RenameDelRes
  End If
  
  //update the logic tooltip lookup table
  With IDefLookup(LogicNum)
    .Name = ""
    .Value = ""
    .Type = 11 //set to a value > highest type
  End With
  //then let open logic editors know
  If LogicEditors.Count > 0 Then
    For i = 1 To LogicEditors.Count
      LogicEditors(i).ListDirty = true
    Next i
  End If
Exit Sub

ErrHandler:
  //if error is invalid resid,
  If Err.Number = vbObjectError + 617 Then
    //pass it along
    On Error GoTo 0: Err.Raise Err.Number, Err.Source, Err.Description
    Exit Sub
  End If
  Resume Next
End Sub

public Sub InitializeResMan()
  
  Dim i As Long
  Dim blnCourier As Boolean, blnArial As Boolean
  Dim blnTimes As Boolean, blnConsolas As Boolean
  
  On Error Resume Next
  
  //set default fonts
  
  //priority is consolas, courier new, arial, then times new roman
  For i = 0 To Screen.FontCount - 1
    If Screen.Fonts(i) = "Consolas" Then
      blnConsolas = true
    End If
    If Screen.Fonts(i) = "Courier New" Then
      blnCourier = true
    End If
    If Screen.Fonts(i) = "Arial" Then
      blnArial = true
    End If
    If Screen.Fonts(i) = "Times New Roman" Then
      blnTimes = true
    End If
  Next i
  
  If blnConsolas Then
    DEFAULT_PFONTNAME = "Consolas"
  ElseIf blnCourier Then
    DEFAULT_PFONTNAME = "Courier New"
  ElseIf blnArial Then
    DEFAULT_PFONTNAME = "Arial"
  ElseIf blnTimes Then
    DEFAULT_PFONTNAME = "Times New Roman"
  Else
    //use first font in list
    DEFAULT_PFONTNAME = Screen.Fonts(0)
  End If
  
  DEFAULT_EFONTNAME = DEFAULT_PFONTNAME
  Settings.EFontName = DEFAULT_EFONTNAME
  Settings.PFontName = DEFAULT_PFONTNAME
  
  //set default color values by copying
  //from WinAGI game object
  For i = 0 To 15
    DefEGAColor(i) = EGAColor(i)
  Next i

  //default gif options
  With VGOptions
    .Cycle = true
    .Transparency = true
    .Zoom = 2
    .Delay = 15
    .HAlign = 0
    .VAlign = 1
  End With
  
  //default value for updating logics is //checked//
  DefUpdateVal = vbChecked
  
  //initialize clipboard object if not already done
  ReDim GlobalsClipboard(0)
  
  //initialize code snippet array
  ReDim CodeSnippets(0)
End Sub
public Function InstrumentName(ByVal bytInstrument As Byte) As String
  //returns a string Value of an instrument
  //if too big
  //Debug.Assert bytInstrument <= 127
    
  InstrumentName = LoadResString(INSTRUMENTNAMETEXT + bytInstrument)
End Function


public Sub NewSound(Optional ImportSoundFile As String)
  //creates a new sound resource and opens and editor
  
  Dim frmNew As frmSoundEdit, blnInGame As Boolean
  Dim tmpSound As AGISound, blnOpen As Boolean
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor

  Do
    //create temporary sound
    Set tmpSound = New AGISound
    //set default instrument settings;
    //if a sound is being imported, these may be overridden...
    With tmpSound
      .Track(0).Instrument = Settings.DefInst0
      .Track(1).Instrument = Settings.DefInst1
      .Track(2).Instrument = Settings.DefInst2
      .Track(0).Muted = Settings.DefMute0
      .Track(1).Muted = Settings.DefMute1
      .Track(2).Muted = Settings.DefMute2
      .Track(3).Muted = Settings.DefMute3
    End With
    
    //if an import filename was passed
    If LenB(ImportSoundFile) != 0 Then
      //import the sound
      //(and check for error)
      On Error Resume Next
      tmpSound.Import ImportSoundFile
      If Err.Number != 0 Then
        //something wrong
        ErrMsgBox "Error occurred while importing sound:", "", "Import Sound Error"
        Exit Do
      End If
      //now check to see if it//s a valid sound resource (by trying to reload it)
      tmpSound.Load
      If Err.Number != 0 Then
        ErrMsgBox "Error reading Sound data:", "This is not a valid sound resource.", "Invalid Sound Resource"
        Exit Do
      End If
      On Error GoTo ErrHandler
    End If
    
    //if a game is loaded
    If GameLoaded Then
      //get sound number
      //show add resource form
      With frmGetResourceNum
        .ResType = rtSound
        If LenB(ImportSoundFile) != 0 Then
          .WindowFunction = grImport
        Else
          .WindowFunction = grAddNew
        End If
        //setup before loading so ghosts don't show up
        .FormSetup
        //suggest ID based on filename
        If Len(ImportSoundFile) > 0 Then
          .txtID.Text = Replace(FileNameNoExt(ImportSoundFile), " ", "")
        End If
        
        //restore cursor while getting resnum
        Screen.MousePointer = vbDefault
        .Show vbModal, frmMDIMain
        
        //show wait cursor again while finishing creating the new sound
        WaitCursor
      
        //if canceled, release the temporary sound, restore cursor and exit method
        If .Canceled Then
          Set tmpSound = Nothing
          //restore mousepointer, unload form and exit
          Unload frmGetResourceNum
          Screen.MousePointer = vbDefault
          Exit Sub
        
        //if user wants sound added to current game
        ElseIf Not .DontImport Then
          //add new id and description
          tmpSound.ID = .txtID.Text
          tmpSound.Description = .txtDescription.Text
        
          //add sound
          AddNewSound .NewResNum, tmpSound
          //reset tmpSound to point to the new game sound
          Set tmpSound = Nothing
          Set tmpSound = Sounds(.NewResNum)
          
          //set flag
          blnInGame = true
        End If
        
        blnOpen = (.chkOpenRes.Value = vbChecked)
      End With
      
      //make sure resource form is unloaded
      Unload frmGetResourceNum
    End If
      
    //only open if user wants it open (or if not in a game or if opening/not importing)
    If blnOpen Or Not GameLoaded Or Not blnInGame Then
      
      //open a new sound editing window
      Set frmNew = New frmSoundEdit
      
      //pass the sound to the editor
      If frmNew.EditSound(tmpSound) Then
        //show form
        frmNew.Show
        //add to collection
        SoundEditors.Add frmNew
      Else
        //error
        Set frmNew = Nothing
      End If
    End If
    
    If GameLoaded Then
      //save openres value
      Settings.OpenNew = blnOpen
    End If
    
    //if added to a game
    If blnInGame Then
      //unload it
      Sounds(tmpSound.Number).Unload
    End If
  Loop Until true
  
  //restore mousepointer and exit
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub NewTextFile()
  //opens a new text editor
  
  Dim frmNew As frmTextEdit
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor
  
  //open a new text editing window
  Set frmNew = New frmTextEdit
  
  With frmNew
    //set caption
    .Caption = "New Text File"
    
    //clear undo buffer
    .rtfLogic.EmptyUndo
    //reset dirty flag
    .rtfLogic.Dirty = false
  
    //show the form
    .Show
  
    //maximize, if that//s the current setting
    If Settings.MaximizeLogics Then
      .WindowState = vbMaximized
    End If
  End With
  
  //add form to collection
  LogicEditors.Add frmNew
  
  //restore main form mousepointer and exit
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub NewLogic(Optional ImportLogicFile As String)
  //creates a new logic resource and opens an editor
  
  Dim frmNew As frmLogicEdit, blnInGame As Boolean
  Dim tmpLogic As AGILogic, blnOpen As Boolean
  Dim intFile As Integer, strFile As String
  Dim blnSource As Boolean, blnImporting As Boolean
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor

  Do
    //create temporary logic
    Set tmpLogic = New AGILogic
    
    //if an import filename passed,
    If LenB(ImportLogicFile) != 0 Then
      blnImporting = true
      
      //open file to see if it is sourcecode or compiled logic
      intFile = FreeFile()
      Open ImportLogicFile For Binary As intFile
      strFile = String$(LOF(intFile), 32)
      Get intFile, 1, strFile
      Close intFile
      
      //if logic appears to be compiled logic:
      //(check for existence of characters <8)
      If InStr(1, strFile, ChrW$(0)) != 0 Then
        blnSource = false
      ElseIf InStr(1, strFile, ChrW$(1)) != 0 Then
        blnSource = false
      ElseIf InStr(1, strFile, ChrW$(2)) != 0 Then
        blnSource = false
      ElseIf InStr(1, strFile, ChrW$(3)) != 0 Then
        blnSource = false
      ElseIf InStr(1, strFile, ChrW$(4)) != 0 Then
        blnSource = false
      ElseIf InStr(1, strFile, ChrW$(5)) != 0 Then
        blnSource = false
      ElseIf InStr(1, strFile, ChrW$(6)) != 0 Then
        blnSource = false
      ElseIf InStr(1, strFile, ChrW$(7)) != 0 Then
        blnSource = false
      Else
        //probably source code
        blnSource = true
      End If
      
      //import the logic
      //(and check for error)
      On Error Resume Next
      tmpLogic.Import ImportLogicFile, blnSource
      //if a compile error occurred,
      If Err.Number = vbObjectError + 567 Then
        //can//t open this resource
        ErrMsgBox "An error occurred while trying to decompile this logic resource:", "Unable to open this logic.", "Invalid Logic Resource"
        Exit Do
        
      ElseIf Err.Number != 0 Then
        //maybe we assumed source status incorrectly- try again
        Err.Clear
        tmpLogic.Import ImportLogicFile, Not blnSource
        
        //if STILL error
        If Err.Number != 0 Then
          //something wrong
          ErrMsgBox "Unable to load this logic resource. It can//t be decompiled, and does not appear to be a text file.", "", "Invalid Logic Resource"
          Exit Do
        End If
      End If
    End If
    
    //if a game is loaded,
    If GameLoaded Then
      //get logic number
      //show add resource form
      With frmGetResourceNum
        .ResType = rtLogic
        If blnImporting Then
          .WindowFunction = grImport
        Else
          .WindowFunction = grAddNew
        End If
        //setup before loading so ghosts don't show up
        .FormSetup
        //suggest ID based on filename
        If Len(ImportLogicFile) > 0 Then
          .txtID.Text = Replace(FileNameNoExt(ImportLogicFile), " ", "")
        End If
        
        //restore cursor while getting resnum
        Screen.MousePointer = vbDefault
        .Show vbModal, frmMDIMain
      
        //show wait cursor while resource is added
        WaitCursor
        
        //if canceled, release the temporary logic, restore mousepointer and exit
        If .Canceled Then
          Set tmpLogic = Nothing
          //restore mousepointer and exit
          Unload frmGetResourceNum
          Screen.MousePointer = vbDefault
          Exit Sub
        
        //if user wants logic added to current game
        ElseIf Not .DontImport Then
          //add ID and description to tmpLogic
          tmpLogic.ID = .txtID.Text
          tmpLogic.Description = .txtDescription.Text
          
          //add Logic
          AddNewLogic .NewResNum, tmpLogic, (.chkRoom.Value = vbChecked), blnImporting
          //reset tmplogic to point to the new game logic
          Set tmpLogic = Nothing
          Set tmpLogic = Logics(.NewResNum)
          
          //if using layout editor AND a room,
          If UseLE And (.chkRoom.Value = vbChecked) Then
            //update editor and data file to show this room is now in the game
            UpdateExitInfo euShowRoom, .NewResNum, Logics(.NewResNum)
          End If
  
          //if including picture
          If .chkIncludePic.Value = vbChecked Then
            //if replacing an existing pic
            If Pictures.Exists(.NewResNum) Then
              RemovePicture .NewResNum
            End If
            AddNewPicture .NewResNum, Nothing
            //help user out if they chose a naming scheme
            If StrComp(Left(.txtID.Text, 3), "rm.", vbTextCompare) = 0 And Len(.txtID.Text) >= 4 Then
              //change ID (if able)
              If ValidateID("pic." + Right(.txtID.Text, Len(.txtID.Text) - 3), Chr(255)) = 0 Then
                //save old resfile name
                strFile = ResDir + Pictures(.NewResNum).ID + ".agp"
                //change this picture//s ID
                Pictures(.NewResNum).ID = "pic." + Right(.txtID.Text, Len(.txtID.Text) - 3)
                //update the resfile, tree and properties
                UpdateResFile rtPicture, .NewResNum, strFile
                //update lookup table
                IDefLookup(768 + .NewResNum).Name = "pic." + Right(.txtID.Text, Len(.txtID.Text) - 3)
              End If
            End If
            
            //pic is still loaded so we need to unload it now
            Pictures(.NewResNum).Unload
          End If
          
          //set ingame flag
          blnInGame = true
        Else
          // not adding to game; still allowed to use template
          If .chkRoom.Value = vbChecked Then
            //add template text
            tmpLogic.SourceText = LogTemplateText(.txtID.Text, .txtDescription.Text)
          Else
            //add default text
            tmpLogic.SourceText = "[ " + vbCr + "[ " + .txtID.Text + vbCr + "[ " + vbCr + vbCr + "return();" + vbCr + vbCr + "[*****" + vbCr + "[ messages         [  declared messages go here" + vbCr + "[*****"
          End If
        End If
        
        blnOpen = (.chkOpenRes.Value = vbChecked)
      End With
      
      //make sure resource form is unloaded
      Unload frmGetResourceNum
    End If
    
    //only open if user wants it open (or if not in a game or if opening/not importing)
    If blnOpen Or Not GameLoaded Or Not blnInGame Then
      //open a new logic editing window
      Set frmNew = New frmLogicEdit
      
      //pass the logic to the editor
      If frmNew.EditLogic(tmpLogic) Then
        //show the form
        frmNew.Show
        //add form to collection
        LogicEditors.Add frmNew
      Else
        Set frmNew = Nothing
      End If
    End If
    
    If GameLoaded Then
      //save openres value
      Settings.OpenNew = blnOpen
    End If
    
    //if logic was added to game
    If blnInGame Then
      //unload it
      Logics(tmpLogic.Number).Unload
    End If
  Loop Until true
  
  //restore mousepointer and exit
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub NewPicture(Optional ImportPictureFile As String)
  //creates a new picture resource and opens an editor
  
  Dim frmNew As frmPictureEdit, blnInGame As Boolean
  Dim tmpPic As AGIPicture, blnOpen As Boolean
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor

  Do
    //create temporary picture
    Set tmpPic = New AGIPicture
    
    //if an import filename was passed
    If LenB(ImportPictureFile) != 0 Then
      //import the picture
      //(and check for error)
      On Error Resume Next
      tmpPic.Import ImportPictureFile
      If Err.Number != 0 Then
        //something wrong
        ErrMsgBox "Error while opening picture:", "Unable to load this picture resource.", "Invalid Picture Resource"
        Exit Do
      End If
      //now check to see if it//s a valid picture resource (by trying to reload it)
      tmpPic.Load
      If Err.Number != 0 Then
        ErrMsgBox "Error reading Picture data:", "This is not a valid picture resource.", "Invalid Picture Resource"
        Exit Do
      End If
      On Error GoTo ErrHandler
    End If
    
    //if a game is loaded
    If GameLoaded Then
      //get picture number
      //show add resource form
      With frmGetResourceNum
        .ResType = rtPicture
        If LenB(ImportPictureFile) = 0 Then
          .WindowFunction = grAddNew
        Else
          .WindowFunction = grImport
        End If
        //setup before loading so ghosts don't show up
        .FormSetup
        //suggest ID based on filename
        If Len(ImportPictureFile) > 0 Then
          .txtID.Text = Replace(FileNameNoExt(ImportPictureFile), " ", "")
        End If
        
        //restore cursor while getting resnum
        Screen.MousePointer = vbDefault
        .Show vbModal, frmMDIMain
        
        //show wait cursor while resource is added
        WaitCursor
        
        //if canceled, release the temporary picture, restore cursor and exit method
        If .Canceled Then
          Set tmpPic = Nothing
          //restore mousepointer and exit
          Unload frmGetResourceNum
          Screen.MousePointer = vbDefault
          Exit Sub
        
        //if user wants picture added to current game
        ElseIf Not .DontImport Then
          //add new id and description
          tmpPic.ID = .txtID.Text
          tmpPic.Description = .txtDescription.Text
        
          //add picture
          AddNewPicture .NewResNum, tmpPic
          //reset tmpPic to point to the new game picture
          Set tmpPic = Nothing
          Set tmpPic = Pictures(.NewResNum)
          //set ingame flag
          blnInGame = true
        End If
      
        blnOpen = (.chkOpenRes.Value = vbChecked)
      End With
      
      //make sure resource form is unloaded
      Unload frmGetResourceNum
    End If
    
    //only open if user wants it open (or if not in a game or if opening/not importing)
    If blnOpen Or Not GameLoaded Or Not blnInGame Then
      //open a new picture editing window
      Set frmNew = New frmPictureEdit
        
      //pass the picture to the editor
      If frmNew.EditPicture(tmpPic) Then
        //show form
        frmNew.Show
        //add to collection
        PictureEditors.Add frmNew
      Else
        //error
        Set frmNew = Nothing
      End If
    End If
    
    If GameLoaded Then
      //save openres value
      Settings.OpenNew = blnOpen
    End If
    
    //if added to a game
    If blnInGame Then
      //unload it
      Pictures(tmpPic.Number).Unload
    End If
  Loop Until true
  
  //restore main form mousepointer and exit
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub
public Sub NewView(Optional ImportViewFile As String)
  //creates a new view editor with a view that is
  //not attached to a game
  
  Dim frmNew As frmViewEdit, blnInGame As Boolean
  Dim tmpView As AGIView, blnOpen As Boolean
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor

  Do
    //create temporary view
    Set tmpView = New AGIView
    //set first cel to default height/width
    tmpView.Loops(0).Cels(0).Height = Settings.DefCelH
    tmpView.Loops(0).Cels(0).Width = Settings.DefCelW
    
    //if an import filename was passed
    If LenB(ImportViewFile) != 0 Then
      //import the view
      //(and check for error)
      On Error Resume Next
      tmpView.Import ImportViewFile
      If Err.Number != 0 Then
        //something wrong
        ErrMsgBox "An error occurred during import:", "", "Import View Error"
        Exit Do
      End If
      //now check to see if it//s a valid picture resource (by trying to reload it)
      tmpView.Load
      If Err.Number != 0 Then
        ErrMsgBox "Error reading View data:", "This is not a valid view resource.", "Invalid View Resource"
        Exit Do
      End If
      On Error GoTo ErrHandler
    End If
    
    //if a game is loaded
    If GameLoaded Then
      //get view number
      //show add resource form
      With frmGetResourceNum
        .ResType = rtView
        If LenB(ImportViewFile) != 0 Then
          .WindowFunction = grImport
        Else
          .WindowFunction = grAddNew
        End If
        //setup before loading so ghosts don't show up
        .FormSetup
        //suggest ID based on filename
        If Len(ImportViewFile) > 0 Then
          .txtID.Text = Replace(FileNameNoExt(ImportViewFile), " ", "")
        End If
      
        //restore cursor while getting resnum
        Screen.MousePointer = vbDefault
        .Show vbModal, frmMDIMain
        
        //show wait cursor while resource is added
        WaitCursor
        
        //if canceled, release the temporary view, restore cursor and exit method
        If .Canceled Then
          Set tmpView = Nothing
          //restore mousepointer and exit
          Unload frmGetResourceNum
          Screen.MousePointer = vbDefault
          Exit Sub
        
        //if user wants view added to current game
        ElseIf Not .DontImport Then
          //add new id and description
          tmpView.ID = .txtID.Text
          tmpView.Description = .txtDescription.Text
          
          //add view
          AddNewView .NewResNum, tmpView
          //reset tmpView to point to the new game view
          Set tmpView = Nothing
          Set tmpView = Views(.NewResNum)
          //set ingame flag
          blnInGame = true
        End If
        
        blnOpen = (.chkOpenRes.Value = vbChecked)
      End With
      
      //make sure resource form is unloaded
      Unload frmGetResourceNum
    End If
    
    //only open if user wants it open (or if not in a game or if opening/not importing)
    If blnOpen Or Not GameLoaded Or Not blnInGame Then
      //open a new view editing window
      Set frmNew = New frmViewEdit
      
      //pass the view to the editor
      If frmNew.EditView(tmpView) Then
        //show form
        frmNew.Show
        frmNew.Refresh
        //add to collection
        ViewEditors.Add frmNew
      Else
        //error
        Set frmNew = Nothing
      End If
    End If
    
    If GameLoaded Then
      //save openres value
      Settings.OpenNew = blnOpen
    End If
    
    //if added to game
    If blnInGame Then
      //unload the game resource
      Views(tmpView.Number).Unload
    End If
  Loop Until true
  
  //restore main form mousepointer and exit
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub OpenWAG(Optional ThisGameFile As String)

  Dim strError As String
  Dim blnWarnings As Boolean
  Dim lngErr As Long, i As Long
  Dim strMsg As String
  
  On Error GoTo ErrHandler
  
  //if no game file passed,
  If LenB(ThisGameFile) = 0 Then
    //get a wag file to open
    With MainDialog
      .Flags = cdlOFNHideReadOnly
      .DialogTitle = "Open WinAGI Game File"
      .Filter = "WinAGI Game file (*.wag)|*.wag|All files (*.*)|*.*"
      .DefaultExt = ""
      .FilterIndex = 1
      .FileName = ""
      .InitDir = BrowserStartDir
      .ShowOpen
      //if error
      ThisGameFile = .FileName
    End With
  End If
  
  //if a game is currently open,
  If GameLoaded Then
    //close game, if user allows
    If Not CloseThisGame() Then
      Exit Sub
    End If
  End If
  
  //all resource editors should now be closed
  //Debug.Assert Not WEInUse
  //Debug.Assert Not OEInUse
  //Debug.Assert Not MEInUse
  //Debug.Assert Not GEInUse
      
  //show wait cursor
  WaitCursor
  
  //show load progress bar
  Load frmProgress
  With frmProgress
    .Caption = "Loading Game"
    .lblProgress.Caption = "Checking WinAGI Game file ..."
    .pgbStatus.Visible = false
    .Show
    .Move frmMDIMain.Left + (frmMDIMain.Width - .Width) / 2, frmMDIMain.Top + (frmMDIMain.Height - .Height) / 2
    .Refresh
  End With
  
  //show loading msg in status bar
  MainStatusBar.Panels(1).Text = "Loading game; please wait..."
  
  //inline error trapping rest of the way
  On Error Resume Next
  
  //and load the game!
  OpenGameWAG ThisGameFile
  
  //catch any errors/warnings that were returned
  lngErr = Err.Number
  strError = Err.Description
  Err.Clear
  On Error GoTo ErrHandler
  
  blnWarnings = (lngErr = vbObjectError + 636)
  
  Select Case lngErr
  Case 0, vbObjectError + 636 //no error, or warnings only
    //loaded ok; maybe with warning
    frmProgress.lblProgress.Caption = "Game loaded successfully, setting up editors"
    
    frmMDIMain.Caption = "WinAGI GDS - " + GameID
    
    //build resource list
    BuildResourceTree
    
    Select Case Settings.ResListType
    Case 1 //tree
      //select root
      frmMDIMain.tvwResources.Nodes(1).Selected = true
      //update selected resource
      frmMDIMain.SelectResource rtGame, -1
      //set LastIndex property
      frmMDIMain.LastIndex = 1
    Case 2
      //select root
      frmMDIMain.cmbResType.ListIndex = 0
      //update selected resource
      frmMDIMain.SelectResource rtGame, -1
    End Select
    
    //set default directory
    BrowserStartDir = GameDir
    
    //add wag file to mru
    AddToMRU ThisGameFile
    
    //save game file name
    CurGameFile = ThisGameFile
    
    //adjust menus
    AdjustMenus rtGame, true, false, false
    
    //show preview window
    If Settings.ShowPreview Then
      Load PreviewWin
      PreviewWin.Show
    End If
    // show resource tree pane
    If Settings.ResListType != 0 Then
      frmMDIMain.ShowResTree
    End If
    
    //set default text file directory to game source file directory
    DefaultResDir = GameDir + ResDirName + "\"
    
    // after a game loads, colors may be different
    //update local copy of colors
    //(used to speed up color matching)
    For i = 0 To 15
      lngEGACol(i) = EGAColor(i)
    Next i

    //done with progress form
    Unload frmProgress
    
    //if warnings
    If blnWarnings Then
      //warn about errors
      MsgBox "Some errors in resource data were encountered. See errlog.txt in the game directory for details.", vbInformation + vbOKOnly, "Errors During Load"
    End If
    
    //build the lookup tables for logic tooltips
    BuildIDefLookup
    BuildGDefLookup
    //update the reserved lookup values
    RDefLookup(90).Value = QUOTECHAR + GameVersion + QUOTECHAR
    RDefLookup(91).Value = QUOTECHAR + GameAbout + QUOTECHAR
    RDefLookup(92).Value = QUOTECHAR + GameID + QUOTECHAR
    RDefLookup(93).Value = InventoryObjects.Count - 1
  
  Case Else
    frmProgress.lblProgress.Caption = "Error encountered, game not loaded"
    
    //error
    Select Case lngErr - vbObjectError
    Case 501
      strError = "A game is already loaded. Close it before opening another game."
    
    Case 502
      strError = "A file access error occurred while trying to open this game: " + vbNewLine + vbNewLine + strError
    
    Case 524
      strError = "A critical game file (" + JustFileName(strError) + " is missing."
      
    Case 541
      strError = "Missing or invalid directory " + ChrW$(39) + strError + "//"
    
    Case 542
      strError = ChrW$(39) + Left$(strError, Len(strError) - 31) + "// is an invalid directory file."
    
    Case 543
      //invalid interpreter version - couldn//t find correct version from the AGI
      //files; current error string is sufficient
      
    Case 545
      //resource loading error; current error string is sufficient
    
    Case 597
      strError = "WinAGI GDS only supports version 2 and 3 of AGI."
    
    Case 655
      strError = "Missing game property file (" + strError + ")."
      
    Case 665
      strError = "Invalid or corrupt game property file (" + strError + ")."
      
    Case 690
      //invalid gameID in wag file
      strError = "Game property file does not contain a valid GameID."
      
    Case 691
      //invalid intVersion in wag file
      strError = "Game property file does not contain a valid Interpreter Version."
      
    Case Else
      //can//t get any other errors
      //Debug.Assert false
    End Select
    
    //done with progress form
    Unload frmProgress
  
    //show error message
    MsgBox strError, vbCritical + vbOKOnly, "Unable to Open Game"
  End Select
    
  //reset cursor
  Screen.MousePointer = vbDefault
  
  //clear status bar
  MainStatusBar.Panels(1).Text = ""
Exit Sub

ErrHandler:
  //if user canceled the dialogbox,
  If Err.Number = cdlCancel Then
    Exit Sub
  End If
  
  //Debug.Assert false
  Resume Next
End Sub

public Sub OpenDIR()

  Dim strError As String
  Dim blnWarnings As Boolean
  Dim lngErr As Long, i As Long
  Dim strMsg As String
  Dim ThisGameDir As String
  
  On Error GoTo ErrHandler
  
  //get a directory for importing
  ThisGameDir = GetNewDir(frmMDIMain.hWnd, "Select the directory of the game you wish to import:")
  
  //if still nothing (user canceled),
  If LenB(ThisGameDir) = 0 Then
    //user canceled
    Exit Sub
  End If
  
  //ensure trailing backslash
  ThisGameDir = cDir(ThisGameDir)
  
  //if a game is currently open,
  If GameLoaded Then
    //close game, if user allows
    If Not CloseThisGame() Then
      Exit Sub
    End If
  End If
  
  //all resource editors should now be closed
  //Debug.Assert Not WEInUse
  //Debug.Assert Not OEInUse
  //Debug.Assert Not MEInUse
  //Debug.Assert Not GEInUse
      
  //show wait cursor
  WaitCursor
  
  //show load progress bar
  Load frmProgress
  With frmProgress
    .Caption = "Importing Game"
    .lblProgress.Caption = "Importing AGI Game ..."
    .pgbStatus.Visible = false
    .Show
    .Move frmMDIMain.Left + (frmMDIMain.Width - .Width) / 2, frmMDIMain.Top + (frmMDIMain.Height - .Height) / 2
    .Refresh
  End With
  
  //show loading msg in status bar
  MainStatusBar.Panels(1).Text = "Importing game; please wait..."
  
  //if a game file exists
  If LenB(Dir(ThisGameDir + "*.wag")) > 0 Then
    //confirm the import
    strMsg = "This directory already has a WinAGI game file. Do you still want to import the game in this directory?"
    strMsg = strMsg + vbCrLf + vbCrLf + "The existing WinAGI game file will be overwritten if it has the same name as the GameID found in this directory//s AGI VOL and DIR files."
    
    If MsgBox(strMsg, vbOKCancel + vbQuestion, "WinAGI Game File Already Exists") = vbCancel Then
      //get rid of progress form
      Unload frmProgress
  
      //reset cursor
      Screen.MousePointer = vbDefault
      
      //clear status bar
      MainStatusBar.Panels(1).Text = ""
      
      //then exit
      Exit Sub
    End If
  End If
  
  //inline error trapping
  On Error Resume Next
  
  //import the game in this directory
  OpenGameDir ThisGameDir
  
  //catch any errors/warnings that were returned
  lngErr = Err.Number
  strError = Err.Description
  Err.Clear
  On Error GoTo ErrHandler
  
  blnWarnings = (lngErr = vbObjectError + 636)
  
  Select Case lngErr
  Case 0, vbObjectError + 636 //no error, or warnings only
    //loaded ok; maybe with warning
    frmProgress.lblProgress.Caption = "Game loaded successfully, setting up editors"
    
    frmMDIMain.Caption = "WinAGI GDS - " + GameID
    
    //build resource list
    BuildResourceTree
    
    Select Case Settings.ResListType
    Case 1 //tree
      //select root
      frmMDIMain.tvwResources.Nodes(1).Selected = true
      //update selected resource
      frmMDIMain.SelectResource rtGame, -1
      //set LastIndex property
      frmMDIMain.LastIndex = 1
    Case 2
      //select root
      frmMDIMain.cmbResType.ListIndex = 0
      //update selected resource
      frmMDIMain.SelectResource rtGame, -1
    End Select
    
    //set default directory
    BrowserStartDir = GameDir
    
    //add game file to mru
    AddToMRU GameDir + GameID + ".wag"
    
    //adjust menus
    AdjustMenus rtGame, true, false, false
    
    //show preview window
    If Settings.ShowPreview Then
      Load PreviewWin
      PreviewWin.Show
    End If
    
    // show resource tree pane
    If Settings.ResListType != 0 Then
      frmMDIMain.ShowResTree
    End If
    
    //set default text file directory to game source file directory
    DefaultResDir = GameDir + ResDirName + "\"
    
    // after a game loads, colors may be different
    //update local copy of colors
    //(used to speed up color matching)
    For i = 0 To 15
      lngEGACol(i) = EGAColor(i)
    Next i

    //done with progress form
    Unload frmProgress
    
    //did the resource directory change? (is this even possible?)
    //YES it is; if only one dir exists, and it has a different name,
    //it//s assumed to be the resource directory
    strMsg = "Game file //" + GameID + ".wag//  has been created." + vbNewLine + vbNewLine
    If ResDirName != DefResDir Then
      strMsg = strMsg + "The existing subdirectory //" + ResDirName + "// will be used "
    Else
      strMsg = strMsg + "The subdirectory //" + ResDirName + "// has been created "
    End If
    strMsg = strMsg + "to store logic " + _
    "source files and exported resources. You can change the " + _
    "source directory for this game on the Game Properties dialog."
    
    //warn user that resource dir set to default
    MsgBox strMsg, vbInformation, "Open Game"
    
    //does the game have an Amiga OBJECT file?
    //very rare, but we check for it anyway
    If InventoryObjects.AmigaOBJ Then
      MsgBox "The OBJECT file for this game is formatted" + vbCrLf + _
             "for the Amiga." + vbCrLf + vbCrLf + _
             "If you intend to run this game on a DOS " + vbCrLf + _
             "platform, you will need to convert the file" + vbCrLf + _
             "to DOS format (use the Convert menu option" + vbCrLf + _
             "on the OBJECT Editor//s Resource menu)", vbInformation + vbOKOnly, "Amiga OBJECT File detected"
    End If
    
    //if warnings
    If blnWarnings Then
      //warn about errors
      MsgBox "Some errors in resource data were encountered. See errlog.txt in the game directory for details.", vbInformation + vbOKOnly, "Errors During Load"
    End If
    
    //build the lookup tables for logic tooltips
    BuildIDefLookup
    BuildGDefLookup
    //update the reserved lookup values
    RDefLookup(90).Value = QUOTECHAR + GameVersion + QUOTECHAR
    RDefLookup(91).Value = QUOTECHAR + GameAbout + QUOTECHAR
    RDefLookup(92).Value = QUOTECHAR + GameID + QUOTECHAR
    RDefLookup(93).Value = InventoryObjects.Count - 1
  
  Case Else
    frmProgress.lblProgress.Caption = "Error encountered, game not loaded"
    
    //error
    Select Case lngErr - vbObjectError
    Case 501
      strError = "A game is already loaded. Close it before opening another game."
    
    Case 502
      strError = "A file access error occurred while trying to open this game: " + vbNewLine + vbNewLine + strError
    
    Case 524
      strError = "A critical game file (" + JustFileName(strError) + " is missing."
    Case 541
      strError = ChrW$(39) + ThisGameDir + "// is not a valid AGI game directory."
    
    Case 542
      strError = ChrW$(39) + Left$(strError, Len(strError) - 31) + "// is an invalid directory file."
    
    Case 543
      //invalid interpreter version - couldn//t find correct version from the AGI
      //files; current error string is sufficient
      
    Case 545
      //resource loading error; current error string is sufficient
    
    Case 597
      strError = "WinAGI GDS only supports version 2 and 3 of AGI."
    
    Case Else
      //can//t get any other errors
      //Debug.Assert false
    End Select
    
    //done with progress form
    Unload frmProgress
  
    //show error message
    MsgBox strError, vbCritical + vbOKOnly, "Unable to Open Game"

  End Select
    
  //reset cursor
  Screen.MousePointer = vbDefault
  
  //clear status bar
  MainStatusBar.Panels(1).Text = ""
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub


public Sub OpenSound(ByVal ResNum As Byte, Optional ByVal Quiet As Boolean = false)
  //this method opens a standard agi Sound editor window

  // if the passed resource is an Apple IIgs sound, just exit
  
  Dim i As Integer, blnLoaded As Boolean
  Dim blnWinOpen As Boolean
  Dim frmNew As frmSoundEdit
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor
  
  //step through all Sound editor windows
  For i = 1 To SoundEditors.Count
    //if Sound number matches
    If SoundEditors(i).SoundNumber = ResNum Then
      blnWinOpen = true
      //exit
      Exit For
    End If
  Next i
  
  //if found,
  If blnWinOpen Then
    //file is already open in another window so set focus to it
    //if minimized,
    If SoundEditors(i).WindowState = vbMinimized Then
      SoundEditors(i).WindowState = vbNormal
    End If
    SoundEditors(i).SetFocus
    
    //set mousepointer to hourglass
    Screen.MousePointer = vbDefault
    Exit Sub
  End If

  //open a new Sound editing window
  Set frmNew = New frmSoundEdit
  
  //if the resource is not loaded,
  blnLoaded = Sounds(ResNum).Loaded
  If Not blnLoaded Then
    //load the Sound
    Sounds(ResNum).Load
    If Err.Number != 0 Then
      If Not Quiet Then
        ErrMsgBox "An error occurred while loading Sound resource:", "", "Load Sound Error"
      End If
      Err.Clear
      Unload frmNew
      
      //set mousepointer to default
      Screen.MousePointer = vbDefault
      Exit Sub
    End If
  End If
  
  //check format
  If Sounds(ResNum).SndFormat = 1 Then
    //load the Sound into the editor
    If frmNew.EditSound(Sounds(ResNum)) Then
      //show the form
      frmNew.Show
      
      //add to collection
      SoundEditors.Add frmNew
    Else
      //error
      Unload frmNew
      Set frmNew = Nothing
    End If
  Else
    If Not Quiet Then
      MsgBox "WinAGI does not currently support editing of Aplle IIgs sounds.", vbInformation + vbOKOnly, "Can//t Edit Apple IIgs Sounds"
    End If
  End If
  
  //unload if necessary
  If Not blnLoaded Then
    Sounds(ResNum).Unload
  End If
  
  //reset main form mousepointer to default
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub
public Sub OpenTextFile(strOpenFile As String, Optional ByVal Quiet As Boolean = false)
  //this method opens a text editor window
  
  Dim i As Integer
  Dim intFile As Integer, strInput As String
  Dim blnWinOpen As Boolean, frmNew As frmTextEdit
  Dim Cscore As Long
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor
  
  //step through all log windows
  For i = 1 To LogicEditors.Count
    //if a text editor
    If LogicEditors(i).FormMode = fmText Then
      //if name matches
      If LogicEditors(i).FileName = strOpenFile Then
        blnWinOpen = true
        //exit
        Exit For
      End If
    End If
  Next i
  
  //if found,
  If blnWinOpen Then
    //file is already open in another window so set focus to it
    //if minimized,
    If LogicEditors(i).WindowState = vbMinimized Then
      LogicEditors(i).WindowState = vbNormal
    End If
    LogicEditors(i).SetFocus
    
    //restore mousepointer and exit
    Screen.MousePointer = vbDefault
    Exit Sub
  End If

  //open a new logic editing window
  Set frmNew = New frmTextEdit
  
  //if file does not exist,
  If Not CanAccessFile(strOpenFile) Then
    //file does not exist,
    Unload frmNew
    If Not Quiet Then
      MsgBox "File not found:" + vbNewLine + strOpenFile, , "File Open Error"
    End If
    //restore mousepointer and exit
    Screen.MousePointer = vbDefault
    Exit Sub
  End If
  
  With frmNew

    //assign text to text box
    .rtfLogic.LoadFile strOpenFile, reOpenSaveText + reOpenSaveOpenAlways, 437 // Text = strInput
    //if not using syntax highlighting
    If Not Settings.HighlightText Then
      //force refresh
      .rtfLogic.RefreshHighlight
    End If
    
    //clear undo buffer
    .rtfLogic.EmptyUndo
    //clear dirty flag
    .rtfLogic.Dirty = false

    //set filename, caption
    .FileName = strOpenFile
    .Caption = "Text editor - " + JustFileName(strOpenFile)
  
    //reset dirty flag
    .rtfLogic.Dirty = false
      
    //maximize, if that//s the current setting
    If Settings.MaximizeLogics Then
      .WindowState = vbMaximized
    End If
  End With
  
  //add form to collection
  LogicEditors.Add frmNew
  
  //show it
  frmNew.Show
  
  //restore mousepointer and exit
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //show error msg
  If Not Quiet Then
    ErrMsgBox "Error while trying to open text file: ", "", "File Open Error"
  End If
  Err.Clear
  //restore main form mousepointer and exit
  Screen.MousePointer = vbDefault
End Sub

public Sub OpenLogic(ByVal ResNum As Byte, Optional ByVal Quiet As Boolean = false)
  //this method opens a logic editor window

  Dim i As Integer, blnLoaded As Boolean
  Dim blnWinOpen As Boolean
  Dim frmNew As frmLogicEdit
  
  On Error GoTo ErrHandler

  //show wait cursor
  WaitCursor
  
  //step through all logic editor windows
  For i = 1 To LogicEditors.Count
    //if window is a logic editor
    If LogicEditors(i).FormMode = fmLogic Then
      //if logic number and ingame status matches
      If LogicEditors(i).LogicNumber = ResNum And LogicEditors(i).InGame Then
        blnWinOpen = true
        //exit
        Exit For
      End If
    End If
  Next i
  
  //if found,
  If blnWinOpen Then
    //file is already open in another window so set focus to it
    //if minimized,
    If LogicEditors(i).WindowState = vbMinimized Then
      LogicEditors(i).WindowState = vbNormal
    End If
    LogicEditors(i).SetFocus
  
    //restore mousepointer and exit
    Screen.MousePointer = vbDefault
    Exit Sub
  End If

  //open a new logic editing window
  Set frmNew = New frmLogicEdit
  
  //if the resource is not loaded,
  blnLoaded = Logics(ResNum).Loaded
  If Not blnLoaded Then
    Logics(ResNum).Load
  End If
  
  //load the logic into the editor
  If frmNew.EditLogic(Logics(ResNum)) Then
    //show the form
    frmNew.Show
    
    //add to collection
    LogicEditors.Add frmNew
    
  Else
    //error
    Unload frmNew
    Set frmNew = Nothing
  End If
  
  //unload if necessary
  If Not blnLoaded Then
    Logics(ResNum).Unload
  End If
  
  //restore mousepointer and exit
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub
public Function TokenFromCursor(rtfLogic As RichEdAGI, Optional NoComment As Boolean = true, Optional ByVal FullString As Boolean = false) As String

  //returns the token/separator at the current cursor location
  //ignores tokens in a comment if nocomment is true
  
  //preference is for a token; if a non-token is found, search
  //again one character back to see if there//s a token there
  //and if there is, return it
  
  // if FullString is set, only a full string (with starting/ending
  // quotes) will be returned
  
  Dim lngPos As Long, strToken As String, lngCount As Long
  Dim tmpRange As Range, blnToken As Boolean
  
  Dim blnQuote As Boolean
  
  On Error GoTo ErrHandler
  
  //get line (use range property of richedit)
  lngPos = rtfLogic.Selection.Range.StartPos
  Set tmpRange = rtfLogic.Range(lngPos, lngPos)
  tmpRange.StartOf reLine, true
  tmpRange.EndOf reLine, true
  strToken = tmpRange.Text
  // strip off ending CR, if there is one
  If Asc(Right(strToken, 1)) = 13 Then
    strToken = Left(strToken, Len(strToken) - 1)
  End If
  
  //get position of cursor in the line
  //(need to add 1 because the rtf editor position
  // is zero-based, but string functions are 1 based)
  lngPos = lngPos - tmpRange.StartPos + 1
  Set tmpRange = Nothing
  
  If NoComment Then
    //strip off comments
    strToken = StripComments(strToken, "", true)
    //if cursor pos is greater than length of line without
    //comments, it means cursor was in a comment
    If lngPos > Len(strToken) + 1 Then
      Exit Function
    End If
  End If
  
  //check for zero length line
  If Len(strToken) = 0 Then
    //nothing to return
    TokenFromCursor = ""
    Exit Function
  End If
  
  // if looking for a string
  If FullString Then
    //find starting quote
    lngPos = InStrRev(strToken, QUOTECHAR, lngPos)
    If lngPos = 0 Then
      //no starting quote
      TokenFromCursor = ""
      Exit Function
    End If
    strToken = Right(strToken, Len(strToken) - lngPos + 1)
    
    //find ending quote
    lngPos = InStr(2, strToken, QUOTECHAR)
    If lngPos = 0 Then
      //no ending quote
      TokenFromCursor = ""
      Exit Function
    End If
    
    TokenFromCursor = Left(strToken, lngPos)
    Exit Function
  End If
  
  //if at end
  If lngPos = Len(strToken) + 1 Then
    //only look left
    //previous char determines token/non-token
    blnToken = IsTokenChar(Asc(Mid$(strToken, lngPos - 1)), NoComment)
    
  //if at beginning
  ElseIf lngPos = 1 Then
    //spaces always trump the token check
    If Asc(Mid$(strToken, lngPos)) = 32 Then
      //no token
      TokenFromCursor = ""
      Exit Function
    End If
    
    //only look right
    blnToken = IsTokenChar(Asc(Mid$(strToken, lngPos)), NoComment)
    
  Else
    //token or non-token?
    blnToken = IsTokenChar(Asc(Mid$(strToken, lngPos)), NoComment)
    //if current char is NOT a token char
    If Not blnToken Then
      //try char behind
      If IsTokenChar(Asc(Mid$(strToken, lngPos - 1)), NoComment) Then
        //find this token
        blnToken = true
        lngPos = lngPos - 1
      End If
    End If
  End If
  
  //now move cursor backward until beginning of line reached
  Do Until lngPos = 1
    //always stop at a space
    If Asc(Mid$(strToken, lngPos - 1)) = 32 Then
      Exit Do
    Else
      If IsTokenChar(Asc(Mid$(strToken, lngPos - 1)), NoComment) != blnToken Then
        Exit Do
      End If
    End If
    
    lngPos = lngPos - 1
    lngCount = lngCount + 1
  Loop
  
  //trim off front of string
  TokenFromCursor = Right$(strToken, Len(strToken) - lngPos + 1)
  lngPos = lngCount + 1
  
  //if trimmed string is empty,
  If Len(TokenFromCursor) = 0 Then
    //no need to keep looking
    Exit Function
  End If
  
  //if already at end of command
  If lngPos >= Len(TokenFromCursor) Then
    Exit Function
  End If
  
  //if looking for a non-token, and first char is a space
  If Not blnToken And Asc(TokenFromCursor) = 32 Then
    //means cursor is within whitespace
    TokenFromCursor = ""
    Exit Function
  End If
  
  //now find end of the cmd
  Do
    //always stop at a space
    If Asc(Mid$(TokenFromCursor, lngPos + 1)) = 32 Then
      Exit Do
    Else
      If IsTokenChar(Asc(Mid$(TokenFromCursor, lngPos + 1)), NoComment) != blnToken Then
        Exit Do
      End If
    End If
    
    lngPos = lngPos + 1
  Loop Until lngPos >= Len(TokenFromCursor) // - 1
  
  //trim off end of cmd and return the token
  TokenFromCursor = Left$(TokenFromCursor, lngPos)
  
  //special case - quotes kinda mess things up
  If TokenFromCursor = QUOTECHAR + QUOTECHAR Then
    //ok
    Exit Function
  ElseIf TokenFromCursor = QUOTECHAR Then
    //don't bother if a single quote
    TokenFromCursor = ""
    Exit Function
  End If
  
  // if a non-token is next to a quote, the quote
  //gets stuck on the returned value; we don't want it
  If Not blnToken And Asc(Right(TokenFromCursor, 1)) = 34 Then
    TokenFromCursor = Left(TokenFromCursor, Len(TokenFromCursor) - 1)
  End If
  
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function


public Sub OpenPicture(ByVal ResNum As Byte, Optional ByVal Quiet As Boolean = false)
  //this method opens a picture editor window
  
  Dim i As Integer, blnLoaded As Boolean
  Dim blnWinOpen As Boolean
  Dim frmNew As frmPictureEdit
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor
  
  //step through all Picture windows
  For i = 1 To PictureEditors.Count
    //if number matches
    If PictureEditors(i).PicNumber = ResNum Then
      blnWinOpen = true
      Exit For
    End If
  Next
  
  //if found,
  If blnWinOpen Then
    //Picture is already open
    If PictureEditors(i).WindowState = vbMinimized Then
      PictureEditors(i).WindowState = vbNormal
    End If
    PictureEditors(i).SetFocus
    //restore mousepointer and exit
    Screen.MousePointer = vbDefault
    Exit Sub
  End If
  
  //open a new edit Picture form
  Set frmNew = New frmPictureEdit
  
  //if the resource is not loaded
  blnLoaded = Pictures(ResNum).Loaded
  If Not blnLoaded Then
    Pictures(ResNum).Load
    If Err.Number != 0 Then
      If Not Quiet Then
        ErrMsgBox "An error occurred while loading logic resource:", "", "Logic Load Error"
      End If
      Err.Clear
      Unload frmNew
      //restore mousepointer and exit
      Screen.MousePointer = vbDefault
      Exit Sub
    End If
  End If
  
  //load Picture into editor
  If frmNew.EditPicture(Pictures(ResNum)) Then
    //show the form
    frmNew.Show
    
    //add to collection
    PictureEditors.Add frmNew
  Else
    //error
    Unload frmNew
    Set frmNew = Nothing
  End If
  
  //unload if necessary
  If Not blnLoaded Then
    Pictures(ResNum).Unload
  End If
  
  //restore mousepointer and exit
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub
public Sub OpenView(ByVal ResNum As Byte, Optional ByVal Quiet As Boolean = false)
  //this method opens a view editor window
  
  Dim i As Integer, blnLoaded As Boolean
  Dim blnWinOpen As Boolean
  Dim frmNew As frmViewEdit
  
  On Error GoTo ErrHandler
  
  //show wait cursor
  WaitCursor
  
  //step through all view windows
  For i = 1 To ViewEditors.Count
    //if number matches
    If ViewEditors(i).ViewNumber = ResNum Then
      blnWinOpen = true
      Exit For
    End If
  Next
  
  //if found,
  If blnWinOpen Then
    //view is already open
    If ViewEditors(i).WindowState = vbMinimized Then
      ViewEditors(i).WindowState = vbNormal
    End If
    ViewEditors(i).SetFocus
    
    //restore mousepointer and exit
    Screen.MousePointer = vbDefault
    Exit Sub
  End If
  
  //open a new edit view form
  Set frmNew = New frmViewEdit
  
  //if the resource is not loaded
  blnLoaded = Views(ResNum).Loaded
  If Not blnLoaded Then
    //use inline error handling
    On Error Resume Next
    //load the view
    Views(ResNum).Load
    If Err.Number != 0 Then
      If Not Quiet Then
        ErrMsgBox "An error occurred while loading view resource:", "", "Logic Load Error"
      End If
      Err.Clear
      Unload frmNew
      //restore mousepointer and exit
      Screen.MousePointer = vbDefault
      Exit Sub
    End If
    On Error GoTo ErrHandler
  End If
  
  //load view into editor
  If frmNew.EditView(Views(ResNum)) Then
    //show form
    frmNew.Show
    
    //add to collection
    ViewEditors.Add frmNew
  Else
    //error
    Unload frmNew
    Set frmNew = Nothing
  End If
  
  //unload if necessary
  If Not blnLoaded Then
    Views(ResNum).Unload
  End If
  
  //restore mousepointer and exit
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Function CloseThisGame() As Boolean

  Dim i As Integer, j As Integer
  Dim tmpPic As AGIPicture, tmpView As AGIView
  Dim tmpSnd As AGISound, tmpLog As AGILogic
  Dim rtn As VbMsgBoxResult, tmpForm As Form
  Dim strError As String, strErrSrc As String, lngError As Long
  
  On Error GoTo ErrHandler
  
  //if no game is open
  If Not GameLoaded Then
    //just return success
    CloseThisGame = true
    Exit Function
  End If
  
  //unload any sound editors
  For i = SoundEditors.Count To 1 Step -1
    //if in game
    If SoundEditors(i).InGame Then
      //save Count
      j = SoundEditors.Count
      Unload SoundEditors(i)
      //check for cancellation
      If j = SoundEditors.Count Then
        Exit Function
      End If
    End If
  Next i
  
  //unload any view edit windows
  For i = ViewEditors.Count To 1 Step -1
    //if in game
    If ViewEditors(i).InGame Then
      //save Count
      j = ViewEditors.Count
      Unload ViewEditors(i)
      //check for cancellation
      If j = ViewEditors.Count Then
        Exit Function
      End If
    End If
  Next i
  
  //unload any picture edit windows
  For i = PictureEditors.Count To 1 Step -1
    //if in game
    If PictureEditors(i).InGame Then
      //save Count
      j = PictureEditors.Count
      Unload PictureEditors(i)
      //check for cancellation
      If j = PictureEditors.Count Then
        Exit Function
      End If
    End If
  Next i
  
  //unload any logic editors
  For i = LogicEditors.Count To 1 Step -1
    If LogicEditors(i).FormMode = fmLogic Then
      //if in game
      If LogicEditors(i).InGame Then
        //save Count
        j = LogicEditors.Count
        Unload LogicEditors(i)
        //check for cancellation
        If j = LogicEditors.Count Then
          Exit Function
        End If
      End If
    End If
  Next i
  
  //always clear and hide warning list if it is showing
  If frmMDIMain.picWarnings.Visible Then
    frmMDIMain.ClearWarnings -1, rtLogic
    frmMDIMain.HideWarningList
  End If
  
  //always hide find dialog if it//s showing
  If FindForm.Visible Then
    FindForm.Visible = false
  End If
  
  If Settings.ShowPreview Then
    //clear preview window
    PreviewWin.ClearPreviewWin
  End If
  
  //unload Objects Editor
  If OEInUse Then
    Unload ObjectEditor
    If OEInUse Then
      Exit Function
    End If
  End If
  
  //unload word editor
  If WEInUse Then
    Unload WordEditor
    //if it is still there,
    If WEInUse Then
      //user canceled
      Exit Function
    End If
  End If
  
  //unload layout editor
  If LEInUse Then
    Unload LayoutEditor
    If LEInUse Then
      Exit Function
    End If
  End If
  
  //unload globals editor
  If GEInUse Then
    Unload GlobalsEditor
    If GEInUse Then
      Exit Function
    End If
  End If
  
  //unload the menu editor
  If MEInUse Then
    Unload MenuEditor
    If MEInUse Then
      Exit Function
    End If
  End If
  
  //if nothing is being edited, and nothing is being previewed,
  //all resources should be unloaded, but just in case...
  
  //unload all resources
  For Each tmpLog In Logics
    //Debug.Assert Not tmpLog.Loaded
    If tmpLog.Loaded Then
      //Debug.Print tmpLog.ID
      //if dirty
      If tmpLog.IsDirty Then
        //ask about saving
        rtn = MsgBox(tmpLog.ID + " has changed. Do you want to save the changes?", vbQuestion + vbYesNoCancel, "Save Changes")
        Select Case rtn
        Case vbYes
          //save the resource
          tmpLog.Save
        Case vbCancel
          //exit
          Exit Function
        End Select
      End If
      tmpLog.Unload
    End If
  Next
  
  For Each tmpPic In Pictures
    //Debug.Assert Not tmpPic.Loaded
    If tmpPic.Loaded Then
      //if dirty
      If tmpPic.IsDirty Then
        //ask about saving
        rtn = MsgBox(tmpPic.ID + " has changed. Do you want to save the changes?", vbQuestion + vbYesNoCancel, "Save Changes")
        Select Case rtn
        Case vbYes
          //save the resource
          tmpPic.Save
        Case vbCancel
          //exit
          Exit Function
        End Select
      End If
      tmpPic.Unload
    End If
  Next
  
  For Each tmpSnd In Sounds
    //Debug.Assert Not tmpSnd.Loaded
    If tmpSnd.Loaded Then
      //if dirty
      If tmpSnd.IsDirty Then
        //ask about saving
        rtn = MsgBox(tmpSnd.ID + " has changed. Do you want to save the changes?", vbQuestion + vbYesNoCancel, "Save Changes")
        Select Case rtn
        Case vbYes
          //save the resource
          tmpSnd.Save
        Case vbCancel
          //exit
          Exit Function
        End Select
      End If
      tmpSnd.Unload
    End If
  Next
  
  For Each tmpView In Views
    //Debug.Assert Not tmpView.Loaded
    If tmpView.Loaded Then
      //if dirty
      If tmpView.IsDirty Then
        //ask about saving
        rtn = MsgBox(tmpView.ID + " has changed. Do you want to save the changes?", vbQuestion + vbYesNoCancel, "Save Changes")
        Select Case rtn
        Case vbYes
          //save the resource
          tmpView.Save
        Case vbCancel
          //exit
          Exit Function
        End Select
      End If
      tmpView.Unload
    End If
  Next
  
  //if using resource tree
  If Settings.ResListType != 0 Then
    frmMDIMain.HideResTree
    
    //clear resource list
    frmMDIMain.ClearResourceList
  End If
  
  //if using preview window
  If Settings.ShowPreview Then
    PreviewWin.Hide
    Unload PreviewWin
  End If
  
  //now close the game
  CloseGame

  //colors get restored to AGI default when a game closes
  //change them to match preferred defaults
  GetDefaultColors
  
  // restore default resdef
  LogicSourceSettings.UseReservedNames = Settings.DefUseResDef
  
  //update menus, toolbars, and caption
  AdjustMenus rtNone, false, false, false
  frmMDIMain.Caption = "WinAGI GDS"
  
  //reset index marker so selection of resources
  //works correctly first time after another game loaded
  frmMDIMain.LastIndex = -1
  
  //reset default text location to program dir
  DefaultResDir = ProgramDir
  
  //game is closed
  CloseThisGame = true
Exit Function
  
ErrHandler:
  //Debug.Assert false
  
  //pass on the error
  On Error GoTo 0: Err.Raise Err.Number, Err.Source, Err.Description
End Function

public Sub GetResDefOverrides()

  Dim strIn As String, strDef() As String
  Dim intCount As Integer, lngGrp As Long
  Dim i As Long
  
  On Error GoTo ErrHandler // Resume Next
  
  //check to see if there are any overrides:
  intCount = CInt(ReadSettingString(SettingsList, "ResDefOverrides", "Count", "0"))
  
  If intCount = 0 Then
    Exit Sub
  End If
  
  //ok, get the overrides, and apply them
  For i = 1 To intCount
    strIn = ReadSettingString(SettingsList, "ResDefOverrides", "Override" + CStr(i))
    
    //split it to get the def value and def name
    //(0)=group
    //(1)=index
    //(2)=newname
    strDef() = Split(strIn, ":")
    
    If UBound(strDef()) = 2 Then
      //get the new name, if a valid entry
      If Val(strDef(1)) <= UBound(LogicSourceSettings.ResDefByGrp(Val(strDef(0)))) Then
        LogicSourceSettings.ResDef(Val(strDef(0)), Val(strDef(1))) = strDef(2)
      End If
    End If
  Next i
  
  //need to make sure we don't have any bad overrides (where overridden name matches
  //another name); if a duplicate is found, just reset the follow on name back to its
  //default value
  
  //we check AFTER all overrides are made just in case a swap is desired- checking in
  //realtime would not allow a swap
  If Not LogicSourceSettings.ValidateResDefs() Then
    //if any were changed, re-write the WinAGI.config file
    SaveResDefOverrides
  End If
  
Exit Sub

ErrHandler:
  //Debug.Assert false
  //always clear errors
  Err.Clear
  Resume Next
End Sub

public Sub SaveResDefOverrides()

  //if any reserved define names are different from the default values,
  //write them to the app settings;
  
  Dim intCount As Integer
  Dim i As Long, dfTemp() As TDefine
  
  //need to make string comparisons case sensitive, in case user
  //wants to change case of a define (even though it really doesn//t matter; compiler is not case sensitive)
  
  On Error GoTo ErrHandler
  
  //first, delete any previous overrides
  DeleteSettingSection SettingsList, "ResDefOverrides"
  
  With LogicSourceSettings
    //now step through each type of define value; if name is not the default, then save it
    
    //checks 27 variables
    dfTemp = .ResDefByGrp(1)
    For i = 0 To 26
      If StrComp(dfTemp(i).Default, dfTemp(i).Name, vbBinaryCompare) != 0 Then
        //save it
        intCount = intCount + 1
        WriteAppSetting SettingsList, "ResDefOverrides", "Override" + CStr(intCount), "1:" + CStr(i) + ":" + dfTemp(i).Name
      End If
    Next i
    
    //checks 18 flags
    dfTemp = .ResDefByGrp(2)
    For i = 0 To 17
      If StrComp(dfTemp(i).Default, dfTemp(i).Name, vbBinaryCompare) != 0 Then
        //save it
        intCount = intCount + 1
        WriteAppSetting SettingsList, "ResDefOverrides", "Override" + CStr(intCount), "2:" + CStr(i) + ":" + dfTemp(i).Name
      End If
    Next i
    
    //checks 5 edge codes
    dfTemp = .ResDefByGrp(3)
    For i = 0 To 4
      If StrComp(dfTemp(i).Default, dfTemp(i).Name, vbBinaryCompare) != 0 Then
        //save it
        intCount = intCount + 1
        WriteAppSetting SettingsList, "ResDefOverrides", "Override" + CStr(intCount), "3:" + CStr(i) + ":" + dfTemp(i).Name
      End If
    Next i
    
    //checks 9 directions
    dfTemp = .ResDefByGrp(4)
    For i = 0 To 8
      If StrComp(dfTemp(i).Default, dfTemp(i).Name, vbBinaryCompare) != 0 Then
        //save it
        intCount = intCount + 1
        WriteAppSetting SettingsList, "ResDefOverrides", "Override" + CStr(intCount), "4:" + CStr(i) + ":" + dfTemp(i).Name
      End If
    Next i
    
    //adds 5 video modes
    dfTemp = .ResDefByGrp(5)
    For i = 0 To 4
      If StrComp(dfTemp(i).Default, dfTemp(i).Name, vbBinaryCompare) != 0 Then
        //save it
        intCount = intCount + 1
        WriteAppSetting SettingsList, "ResDefOverrides", "Override" + CStr(intCount), "5:" + CStr(i) + ":" + dfTemp(i).Name
      End If
    Next i
    
    //adds 9 computer types
    dfTemp = .ResDefByGrp(6)
    For i = 0 To 5
      If StrComp(dfTemp(i).Default, dfTemp(i).Name, vbBinaryCompare) != 0 Then
        //save it
        intCount = intCount + 1
        WriteAppSetting SettingsList, "ResDefOverrides", "Override" + CStr(intCount), "6:" + CStr(i) + ":" + dfTemp(i).Name
      End If
    Next i
    
    //adds 16 colors
    dfTemp = .ResDefByGrp(7)
    For i = 0 To 15
      If StrComp(dfTemp(i).Default, dfTemp(i).Name, vbBinaryCompare) != 0 Then
        //save it
        intCount = intCount + 1
        WriteAppSetting SettingsList, "ResDefOverrides", "Override" + CStr(intCount), "7:" + CStr(i) + ":" + dfTemp(i).Name
      End If
    Next i
    
    dfTemp = .ResDefByGrp(8)
    For i = 0 To 5
      If StrComp(dfTemp(i).Default, dfTemp(i).Name, vbBinaryCompare) != 0 Then
        //save it
        intCount = intCount + 1
        WriteAppSetting SettingsList, "ResDefOverrides", "Override" + CStr(intCount), "8:" + CStr(i) + ":" + dfTemp(i).Name
      End If
    Next i //adds 6 others
  End With
  
  //write the count value
  WriteAppSetting SettingsList, "ResDefOverrides", "Count", CStr(intCount)
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub



public Function GetAGIColor(lngEGAColor As Long) As AGIColors
  //this function is used to quickly convert an EGA color
  //into corresponding AGI color index
  //
  //used primarily by View Editor when dealing with cutting/pasting/flipping
  //bitmap sections
  
  Dim i As Byte
  Dim bytCloseColor As Byte, lngDiff As Long
  Dim bytRed As Byte, bytBlue As Byte, bytGreen As Byte
  Dim newdiff As Long
  
  On Error GoTo ErrHandler
  
  bytRed = lngEGAColor / 0x10000
  bytGreen = (lngEGAColor And 0xFFFF) / 0x100
  bytBlue = lngEGAColor And 0xFF
  
  //set initial difference to ensure first number resets it
  lngDiff = 0xFFFFFF
  i = 0

  //loop until exact match is found,
  //or until all colors are compared
  Do Until lngDiff = 0 Or i = 16
    //get new diff
    bytRed = lngEGACol(i) / 0x10000
    bytGreen = (lngEGACol(i) And 0xFFFF) / 0x100
    bytBlue = lngEGACol(i) And 0xFF
    newdiff = (Abs(bytRed - lngEGAColor / 0x10000) + Abs(bytGreen - (lngEGAColor And 0xFFFF) / 0x100) + Abs(bytBlue - lngEGAColor And 0xFF))
    
    //if difference between this color and color i is less than difference
    If newdiff < lngDiff Then
      lngDiff = newdiff
      bytCloseColor = i
    End If
    i = i + 1
  Loop
  
  //return best match
  GetAGIColor = bytCloseColor
Exit Function

ErrHandler:
  //Debug.Assert false
  //just return black
  GetAGIColor = agBlack
End Function

public Function CmdType(ByVal CommandText As String) As Long
  //converts a command string into a command Value
  //
  //0 = any draw command
  //1 = set vis color command
  //2 = set pri color command
  //3 = set plot pen parameters
  //4 = ERROR (an invalid command); these are very RARE
  
  On Error GoTo ErrHandler
  
  Select Case Left$(CommandText, 3)
  Case "Vis"
    CmdType = 1
  Case "Pri"
    CmdType = 2
  Case "Set"
    CmdType = 3
  Case "ERR"
    CmdType = 4
  End Select
Exit Function

ErrHandler:
  //for any error, set Type to zero
  Err.Clear
  CmdType = 0

End Function

public Function CoordText(ByVal X As Byte, ByVal Y As Byte) As String
  //this function creates the coordinate text in the form
  // (X, Y)
  
  CoordText = "(" + CStr(X) + ", " + CStr(Y) + ")"
End Function

public Function GetPriBand(ByVal Y As Byte, Optional PriBase As Byte = 48) As Byte

  //convert Y Value into appropriate priority band
  
  If Y < PriBase Then
    GetPriBand = 4
  Else
    GetPriBand = Int((CLng(Y) - PriBase) / (168 - PriBase) * 10) + 5
  End If
End Function
public Function ExtractCoordinates(TreeText As String) As PT
  Dim strCoord() As String
  Dim lngPos As Long
  
  On Error GoTo ErrHandler
  
  //splits a treetext string into x and Y coordinates
  //input will always be "(#,#)" or "# -- (#,#)"
  
  //determine case of input
  If InStr(1, TreeText, "-") != 0 Then
    //need to skip pattern number and parentheses
    lngPos = InStr(5, TreeText, "(")
    strCoord = Split(Mid$(TreeText, lngPos + 1, Len(TreeText) - lngPos - 1), ",")
  Else
    //strip off parentheses
    strCoord = Split(Mid$(TreeText, 2, Len(TreeText) - 2), ",")
  End If
  
  //convert to long integers
  ExtractCoordinates.X = CLng(Val(strCoord(0)))
  ExtractCoordinates.Y = CLng(strCoord(1))
Exit Function

ErrHandler:
  //Debug.Assert false
  Resume Next
End Function
Sub FindInLogic(ByVal FindText As String, ByVal FindDir As FindDirection, ByVal MatchWord As Boolean, _
                ByVal MatchCase As Boolean, ByVal LogicLoc As FindLocation, _
                Optional ByVal Replacing As Boolean = false, Optional ByVal ReplaceText As String = "")

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
  
  Dim FoundPos As Long
  Dim SearchPos As Long, blnReplaced As Boolean, blnSelMatch As Boolean
  Dim BeginClosedLogics As Boolean, blnNewSearch As Boolean
  Dim lngNextLogWin As Long, lngFirstLogWin As Long
  Dim lngLogWinCount As Long, lngSearchSource As Long
  Dim vbcComp As VbCompareMethod
  Dim i As Long, blnFrmVis As Boolean
  Dim rtn As VbMsgBoxResult, lngCheck As Long
  Dim lngPossFind As Long
  Dim blnSkipEd As Boolean
  Dim lngTopLine As Long, lngThisLine As Long
  Dim pPos As POINTAPI, lngBtmLine As Long
  
  On Error GoTo ErrHandler
  
  MainStatusBar.Panels(1).Text = ""
  
  //set comparison method for string search,
  vbcComp = CLng(MatchCase) + 1 // CLng(true) + 1 = 0 = vbBinaryCompare; Clng(false) + 1 = 1 = vbTextCompare
  
  //if replacing and new text is the same
  If Replacing And (StrComp(FindText, ReplaceText, vbcComp) = 0) Then
    //exit
    Exit Sub
  End If
  
  //show wait cursor
  WaitCursor
  
  Select Case SearchForm.Name
  Case "frmLogicEdit", "frmTextEdit"
    lngSearchSource = 0
  Case "frmMDIMain"
    lngSearchSource = 1
  Case "frmObjectEdit"
    lngSearchSource = 2
  Case "frmWordsEdit"
    lngSearchSource = 3
  Case "frmGlobals"
    lngSearchSource = 4
  Case Else
    //Debug.Assert false
  End Select
  
  Select Case lngSearchSource
  Case 0
    //if starting from a logic or text, determine form
    //number of starting logic
    lngLogWinCount = LogicEditors.Count
    For i = 1 To lngLogWinCount
      If LogicEditors(i) Is SearchForm Then
        lngNextLogWin = i
        Exit For
      End If
    Next i
    
    //set first logic to current logic
    lngFirstLogWin = lngNextLogWin
    
    //does selection match search term
    blnSelMatch = (StrComp(LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.Text, FindText, vbcComp) = 0)
    
    //if replacing, we need to first check the current selection
    If Replacing Then
      With LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range
        //is searchword currently selected?
        If blnSelMatch Then
          //selection is highlighted with search text; just reset it
          .Text = ReplaceText
          // set flag so we know to update starting search position
          blnReplaced = true
          //the richtext box handles undo automatically
          
          //need to re-apply context highlighting
          i = SendMessage(LogicEditors(lngNextLogWin).rtfLogic.hWnd, EM_LINEFROMCHAR, .StartPos, 0)
          LogicEditors(lngNextLogWin).rtfLogic.RefreshHighlight i, i
        End If
      End With
    End If
    
    //if starting position not set, it means starting a new search
    // set startpos and start logic
    If SearchStartPos = -1 Then
      blnNewSearch = true
      
      //save this logic and position to support find again
      SearchStartLog = lngFirstLogWin
      
      //if searching up
      If FindDir = fdUp Then
        //always use current location
        SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos + 1
      Else
        //searching down or all both use the same strategy
        
        // if something was replaced, search starts one character before
        // start of selection;
        // if nothing replaced, start depends on whether something is
        // selected - if nothing selected, search starts one character
        // before cursor (in case cursor is right in front of the
        // word being searched for); if there is a selection, search
        // starts at cursor location if selection doesn//t match search
        // text; if selected text matches, then the saved search start
        // is set to current location, but the current search start has
        // to start one character AFTER search start AND sets FirstFind
        // flag; (that makes the selection count as the //first find//,
        // and this trip through the function will then look for the
        //next instance
      
        //(don't forget to adjust by +1)
        If blnReplaced Then
          SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos
        Else
          //if nothing selected
          If LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.Length = 0 Then
            //start in front of current selection
            SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos
          Else
            //does selection match?
            If blnSelMatch Then
              // use current cursor position
              SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos + 1
              // and set firstfind flag
              FirstFind = true
            Else
              // use current cursor position-1
              SearchStartPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos //+ 1
            End If
          End If
        End If
      End If
    End If
    
    //intial SearchPos also depends on direction
    If FindDir = fdUp Then
      // if a new search
      If blnNewSearch Then
        // need to adjust position in case FindText is only partially in front of start point
        SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos + Len(FindText) - 1
      Else
        SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos
      End If
      
      //if at start of text box
      If SearchPos <= 0 Then
        //reset to end
        SearchPos = -1
      End If
    Else
      //when searching forward, add one because instr and instrrev are //1//
      //based, but textbox positions are //0// based
      
      // if something just got replaced
      If blnReplaced Then
        //use end of selection
        SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.EndPos + 1
      Else
        //if selection matches, and this is a new search
        If blnSelMatch And blnNewSearch Then
          //increment saved start position
          SearchPos = SearchStartPos + 1
        Else
          //selection doesn//t match; if this is a new search
          If blnNewSearch Then
            //use beginning of selection
            SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.StartPos + 1
          Else
            //otherwise, use end of selection
            SearchPos = LogicEditors(lngFirstLogWin).rtfLogic.Selection.Range.EndPos + 1
          End If
        End If
      End If
    End If
    
  Case 1, 2, 3, 4
    //no distinction (yet) between words, objects, resIDs, globals
    //Debug.Assert FindDir = fdAll
    //Debug.Assert LogicLoc = flAll
    
    // if no logic editors are open
    If LogicEditors.Count = 0 Then
      //begin search in closed logics
      
      //first disable main form
      frmMDIMain.Enabled = false
      //set closelogics flag to false
      ClosedLogics = false
      //get number of next logic
      lngNextLogWin = FindInClosedLogics(FindText, FindDir, MatchWord, MatchCase, SearchType)
      //reenable form
      frmMDIMain.Enabled = true
      //if nothing found (lngnextlogwin =-1)
      If lngNextLogWin = -1 Then
        //show msgbox and exit
        MsgBox "Search text not found.", vbInformation, "Find in Logic"
        //restore cursor
        Screen.MousePointer = vbDefault
        Exit Sub
      End If
    
      //this editor is only one open
      lngFirstLogWin = 1
      lngNextLogWin = 1
      SearchStartLog = 1
    Else
      //start the current logic editor
      lngFirstLogWin = 1
      lngNextLogWin = 1
      SearchStartLog = 1
    End If
    
    //always start the search at beginning of the logic
    SearchStartPos = 1
    SearchPos = 1
    
  Case Else
    //Debug.Assert false
  End Select
  
  //main search routine; at this point, a logic editor is open and available for searching
  Do
    //just in case we get stuck in a loop!
    lngCheck = lngCheck + 1
    
    //only do a search if not STARTING a closed logic search or not doing
    //CONTINUING a closed logic search
    If Not BeginClosedLogics Or Not ClosedLogics Then
      //reset the skip editor flag
      blnSkipEd = false
      
      //if all logics, skip any text editors or non ingame logics
      If LogicLoc = flAll Then
        If LogicEditors(lngNextLogWin).Name = "frmTextEdit" Then
          //skip it
          blnSkipEd = true
        ElseIf LogicEditors(lngNextLogWin).Name = "frmLogicEdit" Then
          If Not LogicEditors(lngNextLogWin).InGame Then
            //skip it
            blnSkipEd = true
          End If
        End If
      End If
      
      If blnSkipEd Then
        // set result to //nothing found//
        FoundPos = 0
      Else
        // search the target logic, from the starting search position
        
        //if direction is up,
        If FindDir = fdUp Then
          //if searching whole word
          If MatchWord Then
            FoundPos = FindWholeWord(SearchPos, LogicEditors(lngNextLogWin).rtfLogic.Text, FindText, MatchCase, FindDir, SearchType)
          Else
            //use instrrev
            FoundPos = InStrRev(LogicEditors(lngNextLogWin).rtfLogic.Text, FindText, SearchPos, vbcComp)
          End If
          // always reset SearchPos
          SearchPos = -1
        Else
          //search strategy depends on synonym search value
          If Not GFindSynonym Then
            //if searching whole word
            If MatchWord Then
              FoundPos = FindWholeWord(SearchPos, LogicEditors(lngNextLogWin).rtfLogic.Text, FindText, MatchCase, FindDir = fdUp, SearchType)
            Else
              //use instr
              FoundPos = InStr(SearchPos, LogicEditors(lngNextLogWin).rtfLogic.Text, FindText, vbcComp)
            End If
          Else
            //Matchword is always true; but since words are surrounded by quotes, it wont matter
            //so we use Instr
            
            //step through each word in the word group; if the word is found in this logic,
            //check if it occurs before the current found position
            FoundPos = 0
            For i = 0 To WordEditor.WordsEdit.GroupN(GFindGrpNum).WordCount - 1
              lngPossFind = InStr(SearchPos, LogicEditors(lngNextLogWin).rtfLogic.Text, QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).Word(i) + QUOTECHAR, vbcComp)
              If lngPossFind > 0 Then
                If FoundPos = 0 Then
                  FoundPos = lngPossFind
                  FindText = QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).Word(i) + QUOTECHAR
                Else
                  If lngPossFind < FoundPos Then
                    FoundPos = lngPossFind
                    FindText = QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).Word(i) + QUOTECHAR
                  End If
                End If
              End If
            Next i
          End If
          // always reset SearchPos
          SearchPos = 1
        End If
      End If
    End If
    
    //if found,
    If FoundPos > 0 Then
      //if searching forward or all
      If FindDir = fdAll Or FindDir = fdDown Then
        //if back at search start (whether anything found or not)
        // OR PAST search start(after previously finding something)
        If ((FoundPos = SearchStartPos) And (lngNextLogWin = SearchStartLog)) Or _
           ((FoundPos > SearchStartPos) And (lngNextLogWin = SearchStartLog) And RestartSearch) Then
          //if NOT searching all logics
          If LogicLoc != flAll Then
            //back at start; reset foundpos
            FoundPos = 0
            //exit loop
            Exit Do
          Else
            //set flag to begin searching closed logics
            BeginClosedLogics = true
            //and also reset the actual closelogic search flag
            ClosedLogics = false
          End If
        Else
          //exit loop; search text found
          Exit Do
        End If
      Else
        //searching up: if atstarting point exactly, or if above starting point
        // and reset flag is set
        If ((FoundPos = SearchStartPos) And (lngNextLogWin = SearchStartLog)) Or _
           ((FoundPos < SearchStartPos) And (lngNextLogWin = SearchStartLog) And RestartSearch) Then
          //if NOT searching all logics
          If LogicLoc != flAll Then
            //back at start; reset foundpos
            FoundPos = 0
            //exit loop
            Exit Do
          Else
            //set flag to begin searching closed logics
            BeginClosedLogics = true
            //and also reset the actual closelogic search flag
            ClosedLogics = false
          End If
        Else
          //exit loop; search text found
          Exit Do
        End If
      End If
    End If
    
    //if not found, action depends on search mode
    Select Case LogicLoc
    Case flCurrent
      //ask if user wants to continue search for up or down search
      Select Case FindDir
      Case fdUp
        //if nothing was found yet
        If Not RestartSearch Then
          rtn = MsgBox("Beginning of search scope reached. Do you want to continue from the end?", vbQuestion + vbYesNo, "Find in Logic")
          If rtn = vbNo Then
            //reset cursor
            Screen.MousePointer = vbDefault
            Exit Sub
          End If
        Else
          // if restartsearch is true, it means this is second time through;
          // since nothing found, just exit the loop
          Exit Do
        End If
        
      Case fdDown
        //if nothing found yet
        If Not RestartSearch Then
          rtn = MsgBox("End of search scope reached. Do you want to continue from the beginning?", vbQuestion + vbYesNo, "Find in Logic")
          If rtn = vbNo Then
            //reset cursor
            Screen.MousePointer = vbDefault
            Exit Sub
          End If
        Else
          // if resetsearch is true, means this is second time through;
          // since nothing found, just exit the loop
          Exit Do
        End If
        
      Case fdAll
        //if restartsearch is true, means this is second time through;
        // since nothing found, just exit the loop
        If RestartSearch Then
          //not found; exit
          Exit Do
        End If
      End Select
    
    Case flOpen
      //if back on start, and search already reset
      If (lngNextLogWin = SearchStartLog) And RestartSearch Then
        //not found- exit
        Exit Do
      End If
    
     //increment logic number
      lngNextLogWin = lngNextLogWin + 1
      If lngNextLogWin > LogicEditors.Count Then
        lngNextLogWin = 1
      End If
      
    Case flAll
      
      //since nothing found in this logic, try the next
      
      // if closed logics need to start, or already searching closed logics
      If BeginClosedLogics Or ClosedLogics Then
      
        //first disable main form
        frmMDIMain.Enabled = false
        //get number of next logic
        lngNextLogWin = FindInClosedLogics(FindText, FindDir, MatchWord, MatchCase, SearchType)
        // clear the start-closed-logic-search flag
        BeginClosedLogics = false
        
        //reenable main form
        frmMDIMain.Enabled = true
        //if nothing found (lngnextlogwin =-1 or -2)
        If lngNextLogWin < 0 Then
          FoundPos = 0
          Exit Do
        End If
        
        // if search started in editor (by pressing F3 or using menu option)
        If Not SearchStartDlg Then
          // select the newly opened logic
          LogicEditors(lngNextLogWin).SetFocus
          //Debug.Print LogicEditors(lngNextLogWin).Name + " has focus (next editor in collection, begin search)"
        End If
      Else
        // NOT starting or continuing a closed logic search
        
        //if back to starting logic, and search already reset
        If (lngNextLogWin = SearchStartLog) And RestartSearch Then
          //not found- set flag to search closed logics
          BeginClosedLogics = true
        Else
          //not back at original start pos, so try the next open logic
          //increment logic number
          lngNextLogWin = lngNextLogWin + 1
          If lngNextLogWin > LogicEditors.Count Then
            lngNextLogWin = 1
          End If
        End If
      End If
    End Select
    
      //set reset search flag so when we are back to starting logic,
      // the search will end
      RestartSearch = true
    
  //loop is exited by finding the searchtext or reaching end of search area
  Loop Until lngCheck >= 256
  
  //if exited by exceeding the count, something went wrong
  If lngCheck >= 256 Then
    //Debug.Assert false
  End If
  
  //if search string was found,
  If FoundPos > 0 Then
    //if first found word in a new search
    If Not FirstFind Then
      //set firstfind to true
      FirstFind = true
    End If
  
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
      If lngBtmLine - lngThisLine < 1 Then
        //scroll so this line is four lines above bottom
        //get currently visible first line
        //determine amount of scrolling to do
        rtn = SendMessage(.hWnd, EM_LINESCROLL, 0, 4 - (lngBtmLine - lngThisLine))
      End If
      //refresh the screen
      .Refresh
    End With
    
    //bring the selected window to the top of the order (restore if minimized)
    If LogicEditors(lngNextLogWin).WindowState = vbMinimized Then
      LogicEditors(lngNextLogWin).WindowState = vbNormal
    End If
    //if search was started from the editor (by pressing F3 or using menu option)
    If Not SearchStartDlg Then
      //set focus to the editor
      LogicEditors(lngNextLogWin).SetFocus
      // and then force focus to the rtf control
      LogicEditors(lngNextLogWin).rtfLogic.SetFocus
      
      //force the form to activate, in case we need to add a statusbar update
      SafeDoEvents
      //Debug.Print frmMDIMain.ActiveForm.Name
    Else
      //when searching from the dialog, make sure the logic is at top of zorder, but
      //don't need to give it focus
      LogicEditors(lngNextLogWin).ZOrder
    End If
    
    //if a synonym was found, note it on status bar
    If GFindSynonym Then
      If FindText != QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).GroupName + QUOTECHAR Then
        MainStatusBar.Panels(1).Text = FindText + " is a synonym for " + QUOTECHAR + WordEditor.WordsEdit.GroupN(GFindGrpNum).GroupName + QUOTECHAR
        //flash the status bar
        frmMDIMain.tmrFlash.Enabled = true
      End If
    End If
    
    //this form is now the search form
    Set SearchForm = LogicEditors(lngNextLogWin)
    
  Else  //search string was NOT found (or couldn//t open a window)
  
  
    // if not opened due to anything else
    If lngNextLogWin != -2 Then
    
      //if something previously found (firstfind=true)
      If FirstFind Then
        //search complete; no new instances found
        blnFrmVis = FindForm.Visible
        If blnFrmVis Then
          FindForm.Visible = false
        End If
        MsgBox "The specified region has been searched. No more matches found.", vbInformation, "Find in Logic"
        If blnFrmVis Then
          FindForm.Visible = true
        End If
      Else
        blnFrmVis = FindForm.Visible
        If blnFrmVis Then
          FindForm.Visible = false
        End If
        //show not found msg
        MsgBox "Search text not found.", vbInformation, "Find in Logic"
        If blnFrmVis Then
          FindForm.Visible = true
        End If
      End If
      
      //restore focus to correct form
      If SearchStartDlg Then
        //assume it//s visible
        //Debug.Assert FindForm.Visible
        //it//s already got focus
      Else
        //set focus to searchform
        If Not SearchForm Is Nothing Then
          SearchForm.SetFocus
        End If
      End If
    End If
    
    //reset search flags
    FindForm.ResetSearch
  End If
  
  //reset cursor
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub


public Sub AssignItems(ctl As Control, strlItems As StringList, Optional blnNumbers As Boolean = false)
  Dim i As Long
  //ctl must be a list box or combobox
  If Not ((TypeOf ctl Is ComboBox) Or (TypeOf ctl Is ListBox)) Then
    //need error statement here
    Exit Sub
  End If
  
  //clear the control
  ctl.Clear
  //add all items  in stringlist into control
  If blnNumbers Then
    For i = 0 To strlItems.Count - 1
      ctl.AddItem CStr(i + 1) + ". " + strlItems.StringLine(i)
    Next i
  Else
    For i = 0 To strlItems.Count - 1
      ctl.AddItem strlItems.StringLine(i)
    Next i
  End If
End Sub



public Sub AdjustMenus(ByVal NewMode As AGIResType, ByVal InGame As Boolean, ByVal Editing As Boolean, ByVal IsDirty As Boolean)
  
  Dim tmpPanel As Panel
  Dim tmpFrm As Form
  
  On Error GoTo ErrHandler
  
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
  
  With frmMDIMain
    //commands that are only available if a game is loaded
    .mnuGClose.Enabled = GameLoaded
    .mnuGCompile.Enabled = GameLoaded
    .mnuGCompileTo.Enabled = GameLoaded
    .mnuGRun.Enabled = GameLoaded
    .mnuGRebuild.Enabled = GameLoaded
    .mnuGCompDirty.Enabled = GameLoaded
    .mnuGProperties.Enabled = GameLoaded
    .mnuTLayout.Enabled = (GameLoaded And UseLE)
    .mnuRImport.Enabled = GameLoaded
    
    //window menu is enabled by default; if it needs to be
    //disabled, the LastForm method will handle it
    .mnuWindow.Enabled = true
    
    //export available for anything except rtNone, rtLayout, rtMenu, rtWarnings
    // if an lpsv resource, number must be valid
    Select Case NewMode
    Case rtNone, rtLayout, rtMenu, rtWarnings
      .mnuRExport.Enabled = false
    Case rtLogic, rtPicture, rtSound, rtView
      .mnuRExport.Enabled = SelResNum != -1
    Case Else //rtObjects, rtWords, rtGlobals, rtGame, rtText
      .mnuRExport.Enabled = true
    End Select
    //hide if not enabled
    .mnuRExport.Visible = .mnuRExport.Enabled
    
    //save available if editing, and resource is dirty
    .mnuRSave.Enabled = (Editing And IsDirty)
    
    //renumber only available if in game
    .mnuRRenumber.Enabled = InGame
    //default is renumber visible
    .mnuRRenumber.Visible = true
    
    If InGame Then
      //remove is available if logic, pic, snd, or view resource is selected
      .mnuRInGame.Enabled = GameLoaded And (NewMode <= 3) And SelResNum != -1
      //set correct caption
      .mnuRInGame.Caption = "&Remove from Game"
      .Toolbar1.Buttons("remove").Image = 10
      .Toolbar1.Buttons("remove").ToolTipText = "Remove from Game"
    Else
      //add may be available if logic, pic, snd, or view resource is selected
      .mnuRInGame.Enabled = GameLoaded And (NewMode <= 3)
      //////ignore SelResNum
      //set correct caption
      .mnuRInGame.Caption = "&Add to Game"
      //add available only if game loaded AND this res type isn//t
      //already maxed out - check for full resources
      Select Case NewMode
      Case rtLogic
        If Logics.Count = 256 Then
          .mnuRInGame.Enabled = false
        End If
      Case rtPicture
        If Pictures.Count = 256 Then
          .mnuRInGame.Enabled = false
        End If
      Case rtSound
        If Sounds.Count = 256 Then
          .mnuRInGame.Enabled = false
        End If
      Case rtView
        If Views.Count = 256 Then
          .mnuRInGame.Enabled = false
        End If
      End Select
      .Toolbar1.Buttons("remove").Image = 3
      .Toolbar1.Buttons("remove").ToolTipText = "Add to Game"
    End If
    
    //default is to show description
    .mnuRDescription.Caption = "ID/&Description..."
    .mnuRDescription.Visible = true
    
    //if a printer is available
    If Not NoPrinter Then
      //printing disabled for none and game and menu
      .mnuRPrint.Enabled = (NewMode != rtGame And NewMode != rtNone And NewMode != rtMenu)
    Else
      .mnuRPrint.Enabled = false
    End If
    
    //hide custom menus
    .mnuRBar2.Visible = false
    .mnuRCustom1.Visible = false
    .mnuRCustom2.Visible = false
    .mnuRCustom3.Visible = false
    .mnuEBar2.Visible = false
    .mnuECustom1.Visible = false
    .mnuECustom2.Visible = false
    .mnuECustom3.Visible = false
    
    //resource custom 2,3 uses check mark for some windows, so reset them
    .mnuRCustom2.Checked = false
    .mnuRCustom3.Checked = false
    
    //default is to enable the property scrollbar
    .fsbProperty.Enabled = true
    
    
    Select Case NewMode
    Case rtNone
      //no game loaded
      .mnuEdit.Visible = false
      //reset save and export
      .mnuRSave.Caption = "&Save Resource"
      .mnuRExport.Caption = "&Export Resource"
      .mnuRRenumber.Caption = "&Renumber Resource"
      .mnuRRenumber.Enabled = false
      //hide description
      .mnuRDescription.Visible = false
      //use generic caption for ingame
      .mnuRInGame.Caption = "&Add to Game"
      //disable property scrollbar
      .fsbProperty.Enabled = false
    
    Case rtGame
      //game is loaded, windows available
      .mnuEdit.Visible = false
      .mnuRSave.Caption = "&Save Resource"
      .mnuRSave.Enabled = false
      //export becomes //export all//
      .mnuRExport.Caption = "&Export All Resources"
      .mnuRRenumber.Caption = "&Renumber Resource"
      .mnuRRenumber.Enabled = false
      .mnuRInGame.Caption = "&Add to Game"
      //hide description
      .mnuRDescription.Visible = false
      
    Case rtLayout
      //game is loaded, layout is active
      .mnuEdit.Visible = true
      //set captions
      .mnuRSave.Caption = "&Save Layout"
      .mnuRExport.Visible = false
      //hide renumber
      .mnuRRenumber.Visible = false
      .mnuRBar2.Visible = true
      .mnuRCustom1.Visible = true
      .mnuRCustom1.Enabled = true
      .mnuRCustom1.Caption = "R&epair Layout" + vbTab + "Alt+R"
      .mnuRCustom2.Visible = true
      .mnuRCustom2.Enabled = true
      If Settings.LEShowPics Then
        .mnuRCustom2.Caption = "Hide All &Pics" + vbTab + "Ctrl+Alt+H"
      Else
        .mnuRCustom2.Caption = "Show All &Pics" + vbTab + "Ctrl+Alt+S"
      End If
      //hide description
      .mnuRDescription.Visible = false
      
    Case rtGlobals
      //game may or may not be loaded, globals are active
      .mnuEdit.Visible = true
      //set captions
      .mnuRSave.Caption = "&Save"
      .mnuRExport.Caption = "Save &As..."
      //hide renumber
      .mnuRRenumber.Visible = false
      //hide description
      .mnuRDescription.Visible = false
      .mnuRBar2.Visible = true
      .mnuRCustom1.Visible = true
      .mnuRCustom1.Enabled = true
      .mnuRCustom1.Caption = "Add from File..." + vbTab + "Alt+F"
      
    Case rtLogic
      //if the header
      If SelResNum = -1 Then
        .mnuEdit.Visible = false
        .mnuEdit.Visible = false
        .mnuRSave.Caption = "&Save Resource"
        .mnuRSave.Enabled = false
        .mnuRPrint.Enabled = false
        //export becomes //export all//
        .mnuRExport.Caption = "&Export All Resources"
        .mnuRRenumber.Caption = "&Renumber Resource"
        .mnuRRenumber.Enabled = false
        //hide description
        .mnuRDescription.Visible = false
      Else
        //enable edit menu if editing
        .mnuEdit.Visible = Editing
        //set captions
        .mnuRSave.Caption = "&Save Logic Source"
        //export ingame logics; save as for non-ingame logics
        If InGame Then
          .mnuRExport.Caption = "&Export Logic"
        Else
          .mnuRExport.Caption = "Save &As..."
        End If
        .mnuRRenumber.Caption = "&Renumber Logic" + vbTab + "Alt+N"
        //set custom menus
        .mnuRBar2.Visible = Editing
        .mnuRCustom1.Visible = Editing And InGame
        .mnuRCustom1.Enabled = true
        .mnuRCustom1.Caption = "Compile Source" + vbTab + "F8"
        .mnuRCustom2.Visible = Editing
        .mnuRCustom2.Enabled = true
        .mnuRCustom2.Caption = "Message Cleanup" + vbTab + "Alt+M"
        .mnuRCustom3.Visible = Editing And InGame
        .mnuRCustom3.Enabled = true
        .mnuRCustom3.Caption = "Logic Is a Room"
        //.mnuRCustom3.Checked=??
        // checked value is set by SetEditMenu on logic form
      End If
      
    Case rtPicture
      //if the header
      If SelResNum = -1 Then
        .mnuEdit.Visible = false
        .mnuRSave.Caption = "&Save Resource"
        .mnuRSave.Enabled = false
        .mnuRPrint.Enabled = false
        //export becomes //export all//
        .mnuRExport.Caption = "&Export All Resources"
        .mnuRRenumber.Caption = "&Renumber Resource"
        .mnuRRenumber.Enabled = false
        //hide description
        .mnuRDescription.Visible = false
        // show the export all images option
        .mnuRBar2.Visible = true
        .mnuRCustom1.Visible = true
        .mnuRCustom1.Enabled = true
        .mnuRCustom1.Caption = "Export All Picture Images..."
      Else
        //enable edit menu if editing
        .mnuEdit.Visible = Editing
        //set captions
        .mnuRSave.Caption = "&Save Picture"
        //export ingame pictures; same as for non-ingame pictures
        If InGame Then
          .mnuRExport.Caption = "&Export Picture"
        Else
          .mnuRExport.Caption = "Save &As..."
        End If
        .mnuRRenumber.Caption = "&Renumber Picture" + vbTab + "Alt+N"
        .mnuRBar2.Visible = true
      
        //if editing
        If Editing Then
          .mnuRCustom1.Visible = true
          .mnuRCustom1.Enabled = true
          .mnuRCustom1.Caption = "&Background..." + vbTab + "Ctrl+Alt+B"
        Else
          .mnuRCustom1.Visible = true
          .mnuRCustom1.Enabled = true
          .mnuRCustom1.Caption = "Save Picture &Image As..." + vbTab + "Shift+Ctrl+S"
          .mnuRCustom2.Visible = false
          .mnuRCustom3.Visible = false
        End If
      End If
    
    Case rtSound
      //if the header
      If SelResNum = -1 Then
        .mnuEdit.Visible = false
        .mnuRSave.Caption = "&Save Resource"
        .mnuRSave.Enabled = false
        .mnuRPrint.Enabled = false
        //export becomes //export all//
        .mnuRExport.Caption = "&Export All Resources"
        .mnuRRenumber.Caption = "&Renumber Resource"
        .mnuRRenumber.Enabled = false
        //hide description
        .mnuRDescription.Visible = false
      Else
        //enable edit menu if editing
        .mnuEdit.Visible = Editing
        //one custom item to allow switching track display mode
        .mnuRBar2.Visible = Editing
        .mnuRCustom1.Visible = Editing
        .mnuRCustom1.Enabled = true
        .mnuRCustom1.Caption = "Show Only Selected Track"
        
        //set captions
        .mnuRSave.Caption = "&Save Sound"
        //export ingame sounds; save as for non-ingame sounds
        If InGame Then
          .mnuRExport.Caption = "&Export Sound"
        Else
          .mnuRExport.Caption = "Save &As..."
        End If
        .mnuRRenumber.Caption = "&Renumber Sound" + vbTab + "Alt+N"
      End If
      
    Case rtView
      //if the header
      If SelResNum = -1 Then
        .mnuEdit.Visible = false
        .mnuRSave.Caption = "&Save Resource"
        .mnuRSave.Enabled = false
        .mnuRPrint.Enabled = false
        //export becomes //export all//
        .mnuRExport.Caption = "&Export All Resources"
        .mnuRRenumber.Caption = "&Renumber Resource"
        .mnuRRenumber.Enabled = false
        //hide description
        .mnuRDescription.Visible = false
      Else
        //enable edit menu if editing
        .mnuEdit.Visible = Editing
        //set captions
        .mnuRSave.Caption = "&Save View"
        //export ingame views; save as for non-ingame views
        If InGame Then
          .mnuRExport.Caption = "&Export View"
        Else
          .mnuRExport.Caption = "Save &As..."
        End If
        .mnuRRenumber.Caption = "&Renumber View" + vbTab + "Alt+N"
        //custom1 used for loop exporting
        .mnuRBar2.Visible = true
        .mnuRCustom1.Visible = true
        //enable if in preview mode
        .mnuRCustom1.Enabled = Not Editing
        .mnuRCustom1.Caption = "Export Loop as &GIF"
      End If
      
    Case rtText
      //enable edit menu if editing
      .mnuEdit.Visible = Editing
      //set captions
      .mnuRSave.Caption = "&Save"
      .mnuRExport.Caption = "Save &As..."
      .mnuRRenumber.Visible = false
      //ingame never allowed for text
      .mnuRInGame.Enabled = false
      .mnuRInGame.Caption = "Add to Game"
      
      //text files have no description
      .mnuRDescription.Visible = false
      
      //show syntax highlight menu item
      .mnuRBar2.Visible = true
      .mnuRCustom2.Visible = true
      .mnuRCustom2.Enabled = true
      .mnuRCustom2.Caption = "Highlight Syntax" + vbTab + "Shift+Ctrl+H"
      .mnuRCustom3.Visible = false
      
    Case rtWarnings
      .mnuEdit.Visible = false
      //set captions and disable save/save as
      .mnuRSave.Caption = "&Save"
      .mnuRSave.Enabled = false
      .mnuRExport.Visible = false
      .mnuRRenumber.Visible = false
      //ingame never allowed for text
      .mnuRInGame.Enabled = false
      .mnuRInGame.Caption = "Add to Game"
      
      //text files have no description
      .mnuRDescription.Visible = false
      
    Case rtWords
      //enable edit menu if editing
      .mnuEdit.Visible = Editing
      //set captions
      .mnuRSave.Caption = "&Save WORDS.TOK"
      //export ingame word lists; save as for non-ingame word lists
      If InGame Then
        .mnuRExport.Caption = "&Export WORDS.TOK"
      Else
        .mnuRExport.Caption = "Save &As..."
      End If
      //renumber and ingame always disabled
      .mnuRRenumber.Visible = false
      .mnuRInGame.Enabled = false
      .mnuRInGame.Caption = "Add to Game"
      
      .mnuRBar2.Visible = Editing
      //set custom menus
      .mnuRBar2.Visible = Editing
      .mnuRCustom1.Visible = Editing
      .mnuRCustom1.Enabled = true
      .mnuRCustom1.Caption = "Merge from File..." + vbTab + "Alt+F"
      .mnuRDescription.Caption = "&Description..."
      .mnuRDescription.Enabled = GameLoaded Or Editing
      
    Case rtObjects
      //enable edit menu if editing
      .mnuEdit.Visible = Editing
      //set captions
      .mnuRSave.Caption = "&Save OBJECT"
      //export ingame object lists; save as for non-ingame object lists
      If InGame Then
        .mnuRExport.Caption = "&Export OBJECT"
      Else
        .mnuRExport.Caption = "Save &As..."
      End If
      //renumber and ingame always disabled
      .mnuRRenumber.Visible = false
      .mnuRInGame.Enabled = false
      .mnuRInGame.Caption = "Add to Game"
      .mnuRBar2.Visible = Editing
      .mnuRCustom2.Visible = (Editing And InventoryObjects.AmigaOBJ)
      .mnuRCustom2.Caption = "Convert AMIGA Format to DOS"
      .mnuRCustom3.Visible = Editing
      .mnuRCustom3.Enabled = true
      .mnuRCustom3.Caption = "Encrypt File" + vbTab + "Shift+Ctrl+E"
      .mnuRDescription.Caption = "&Description..."
      .mnuRDescription.Enabled = GameLoaded Or Editing
      
    Case rtMenu
      //enable editing
      .mnuEdit.Visible = true
      .mnuRDescription.Visible = false
      .mnuRInGame.Enabled = false
      .mnuRInGame.Caption = "Add to Game"
      .mnuRRenumber.Visible = false
      .mnuRExport.Visible = false
      .mnuRSave.Caption = "Save Menu"
      If Not GameLoaded Then
        .mnuRSave.Enabled = false
      End If
      .mnuRCustom1.Visible = true
      .mnuRCustom1.Enabled = true
      .mnuRCustom1.Caption = "Change Background" + vbTab + "Alt+B"
    End Select
  End With
    
  With frmMDIMain.Toolbar1
    //set toolbar buttons
    .Buttons("close").Enabled = GameLoaded
    .Buttons("run").Enabled = GameLoaded
    .Buttons("import_r").Enabled = GameLoaded
    .Buttons("layout").Enabled = (GameLoaded And UseLE)
    .Buttons("menu").Enabled = frmMDIMain.mnuTMenuEditor.Enabled
    .Buttons("globals").Enabled = frmMDIMain.mnuTGlobals.Enabled
    .Buttons("print").Enabled = frmMDIMain.mnuRPrint.Enabled
    .Buttons("remove").Enabled = frmMDIMain.mnuRInGame.Enabled
    .Buttons("export").Enabled = frmMDIMain.mnuRExport.Enabled
    .Buttons("save").Enabled = frmMDIMain.mnuRSave.Enabled
    Select Case NewMode
    Case rtNone
      .Buttons("save").ToolTipText = "Save"
      .Buttons("print").ToolTipText = "Print"
    Case rtLogic
      .Buttons("save").ToolTipText = "Save Logic"
      .Buttons("print").ToolTipText = "Print Logic"
    Case rtPicture
      .Buttons("save").ToolTipText = "Save Picture"
      .Buttons("print").ToolTipText = "Print Picture"
    Case rtSound
      .Buttons("save").ToolTipText = "Save Sound"
      .Buttons("print").ToolTipText = "Print Sound"
    Case rtView
      .Buttons("save").ToolTipText = "Save View"
      .Buttons("print").ToolTipText = "Print View"
    Case rtObjects
      .Buttons("save").ToolTipText = "Save Objects"
      .Buttons("print").ToolTipText = "Print Objects"
    Case rtWords
      .Buttons("save").ToolTipText = "Save Word List"
      .Buttons("print").ToolTipText = "Print Word List"
    Case rtLayout
      .Buttons("save").ToolTipText = "Save Layout"
      .Buttons("print").ToolTipText = "Print Layout"
    Case rtMenu
      .Buttons("save").ToolTipText = "Save Menu"
      .Buttons("print").ToolTipText = "Print"
    Case rtGlobals
      .Buttons("save").ToolTipText = "Save Global List"
      .Buttons("print").ToolTipText = "Print Global List"
    Case rtGame
      .Buttons("save").ToolTipText = "Save Game"
      .Buttons("print").ToolTipText = "Print"
    Case rtText
      .Buttons("save").ToolTipText = "Save Text File"
      .Buttons("print").ToolTipText = "Print Text File"
    Case rtWarnings
      .Buttons("save").ToolTipText = "Save"
      .Buttons("print").ToolTipText = "Print"
    End Select
  End With
  
  //adjust statusbar
  With MainStatusBar
    .Panels.Clear
    If Editing Then
      .Tag = CStr(NewMode)
    Else
      .Tag = ""
    End If
    
    //if editing,
    If Editing Then
      Select Case NewMode
      Case rtNone, rtGame, rtGlobals, rtWarnings
        //only caps, num, and insert
        Set tmpPanel = .Panels.Add(1, , , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Bevel = sbrNoBevel
          tmpPanel.MinWidth = 132
         
         Set tmpPanel = .Panels.Add(2, , , sbrCaps)
           tmpPanel.Alignment = sbrCenter
           tmpPanel.MinWidth = 750
           tmpPanel.Width = 750
         
         Set tmpPanel = .Panels.Add(3, , , sbrNum)
           tmpPanel.Alignment = sbrCenter
           tmpPanel.MinWidth = 750
           tmpPanel.Width = 750
    
         Set tmpPanel = .Panels.Add(4, , , sbrIns)
           tmpPanel.Alignment = sbrCenter
           tmpPanel.MinWidth = 750
           tmpPanel.Width = 750
           
      Case rtLogic, rtText
        Set tmpPanel = .Panels.Add(1, "Status", , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Alignment = sbrLeft
          //Debug.Assert Not frmMDIMain.ActiveForm Is Nothing
          tmpPanel.Text = frmMDIMain.ActiveForm.Tag
          
         Set tmpPanel = .Panels.Add(2, "Row", , sbrText)
           tmpPanel.MinWidth = 1587
           tmpPanel.Width = 1587
         
         Set tmpPanel = .Panels.Add(3, "Col", , sbrText)
           tmpPanel.MinWidth = 1323
           tmpPanel.Width = 1323
         
         Set tmpPanel = .Panels.Add(4, , , sbrCaps)
           tmpPanel.Alignment = sbrCenter
           tmpPanel.MinWidth = 750
           tmpPanel.Width = 750
         
         Set tmpPanel = .Panels.Add(5, , , sbrNum)
           tmpPanel.Alignment = sbrCenter
           tmpPanel.MinWidth = 750
           tmpPanel.Width = 750
    
         Set tmpPanel = .Panels.Add(6, , , sbrIns)
           tmpPanel.Alignment = sbrCenter
           tmpPanel.MinWidth = 750
           tmpPanel.Width = 750
         
      Case rtPicture
        Set tmpPanel = .Panels.Add(1, "Scale", , sbrText)
          tmpPanel.Width = 1720
          tmpPanel.MinWidth = 1720
          
        Set tmpPanel = .Panels.Add(2, "Mode", , sbrText)
          tmpPanel.Width = 1323
          tmpPanel.MinWidth = 1323
          
        Set tmpPanel = .Panels.Add(3, "Tool", , sbrText)
          tmpPanel.MinWidth = 1323
          tmpPanel.Width = 2646
          
        Set tmpPanel = .Panels.Add(4, "Anchor", , sbrText)
          tmpPanel.MinWidth = 1335
          tmpPanel.Width = 1335
          tmpPanel.Visible = false
          
        Set tmpPanel = .Panels.Add(5, "Block", , sbrText)
          tmpPanel.MinWidth = 1935
          tmpPanel.Width = 1935
          tmpPanel.Visible = false
          
        Set tmpPanel = .Panels.Add(6, , , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Bevel = sbrNoBevel
          tmpPanel.MinWidth = 132
          
        Set tmpPanel = .Panels.Add(7, "CurX", , sbrText)
          tmpPanel.MinWidth = 1111
          tmpPanel.Width = 1111
          
        Set tmpPanel = .Panels.Add(8, "CurY", , sbrText)
          tmpPanel.MinWidth = 1111
          tmpPanel.Width = 1111
          
        Set tmpPanel = .Panels.Add(9, "PriBand", , sbrText)
          tmpPanel.Alignment = sbrRight
          tmpPanel.MinWidth = 1587
          tmpPanel.Width = 1587
          
      Case rtSound
        Set tmpPanel = .Panels.Add(1, "Scale", , sbrText)
          tmpPanel.Width = 860
          tmpPanel.MinWidth = 860
          
        Set tmpPanel = .Panels.Add(2, "Time", , sbrText)
          tmpPanel.Width = 1323
          tmpPanel.MinWidth = 1323
          
        Set tmpPanel = .Panels.Add(3, , , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Bevel = sbrNoBevel
          tmpPanel.MinWidth = 132
          
        Set tmpPanel = .Panels.Add(4, , , sbrCaps)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
        Set tmpPanel = .Panels.Add(5, , , sbrNum)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
        Set tmpPanel = .Panels.Add(6, , , sbrIns)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
              
      Case rtView
        Set tmpPanel = .Panels.Add(1, "Scale", , sbrText)
          tmpPanel.Width = 1720
          tmpPanel.MinWidth = 1720
          
        Set tmpPanel = .Panels.Add(2, "Tool", , sbrText)
          tmpPanel.Width = 1984
          tmpPanel.MinWidth = 1984
          
        Set tmpPanel = .Panels.Add(3, , , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Bevel = sbrNoBevel
          tmpPanel.MinWidth = 132
          
        Set tmpPanel = .Panels.Add(4, "CurX", , sbrText)
          tmpPanel.MinWidth = 1111
          tmpPanel.Width = 1111
          
        Set tmpPanel = .Panels.Add(5, "CurY", , sbrText)
          tmpPanel.MinWidth = 1111
          tmpPanel.Width = 1111
            
      Case rtObjects
        Set tmpPanel = .Panels.Add(1, "Count", , sbrText)
          tmpPanel.MinWidth = 1587
          tmpPanel.Width = 1587
       
        Set tmpPanel = .Panels.Add(2, "Encrypt", , sbrText)
          tmpPanel.MinWidth = 1587
          tmpPanel.Width = 1587
       
        Set tmpPanel = .Panels.Add(3, , , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Bevel = sbrNoBevel
          tmpPanel.MinWidth = 132
         
        Set tmpPanel = .Panels.Add(4, , , sbrCaps)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
        Set tmpPanel = .Panels.Add(5, , , sbrNum)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
        Set tmpPanel = .Panels.Add(6, , , sbrIns)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
       
      Case rtWords
        Set tmpPanel = .Panels.Add(1, "GroupCount", , sbrText)
          tmpPanel.MinWidth = 1587
          tmpPanel.Width = 1587
        
        Set tmpPanel = .Panels.Add(2, "WordCount", , sbrText)
          tmpPanel.MinWidth = 1587
          tmpPanel.Width = 1587
        
        Set tmpPanel = .Panels.Add(3, , , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Bevel = sbrNoBevel
          tmpPanel.MinWidth = 132
          
        Set tmpPanel = .Panels.Add(4, , , sbrCaps)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
        Set tmpPanel = .Panels.Add(5, , , sbrNum)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
    
        Set tmpPanel = .Panels.Add(6, , , sbrIns)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
          
      Case rtLayout
        Set tmpPanel = .Panels.Add(1, "Scale", , sbrText)
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
          
        Set tmpPanel = .Panels.Add(2, "Tool", , sbrText)
          tmpPanel.MinWidth = 2000
          tmpPanel.Width = 2000
          
        Set tmpPanel = .Panels.Add(3, "ID", , sbrText)
          tmpPanel.Width = 1323
          tmpPanel.MinWidth = 1323
          
        Set tmpPanel = .Panels.Add(4, "Type", , sbrText)
          tmpPanel.Width = 1323
          tmpPanel.MinWidth = 1323
          
        Set tmpPanel = .Panels.Add(5, "Room1", , sbrText)
          tmpPanel.Width = 2646
          tmpPanel.MinWidth = 2646 //1323
          
        Set tmpPanel = .Panels.Add(6, "Room2", , sbrText)
          tmpPanel.Width = 2646
          tmpPanel.MinWidth = 2646 //1323
          
        Set tmpPanel = .Panels.Add(7, , , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Bevel = sbrNoBevel
          tmpPanel.MinWidth = 132
          
        Set tmpPanel = .Panels.Add(8, "CurX", , sbrText)
          tmpPanel.MinWidth = 1111
          tmpPanel.Width = 1111
          
        Set tmpPanel = .Panels.Add(9, "CurY", , sbrText)
          tmpPanel.MinWidth = 1111
          tmpPanel.Width = 1111
      End Select
    //not editing
    Else
      Select Case NewMode
      Case rtLogic, rtPicture, rtSound, rtView
        Set tmpPanel = .Panels.Add(1, , , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Bevel = sbrNoBevel
          tmpPanel.MinWidth = 132
        
        Set tmpPanel = .Panels.Add(2, , , sbrCaps)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
        Set tmpPanel = .Panels.Add(3, , , sbrNum)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
        Set tmpPanel = .Panels.Add(4, , , sbrIns)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
      Case Else
        //same as game/none
        Set tmpPanel = .Panels.Add(1, , , sbrText)
          tmpPanel.AutoSize = sbrSpring
          tmpPanel.Bevel = sbrNoBevel
          tmpPanel.MinWidth = 132
        
        Set tmpPanel = .Panels.Add(2, , , sbrCaps)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
        Set tmpPanel = .Panels.Add(3, , , sbrNum)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
        
        Set tmpPanel = .Panels.Add(4, , , sbrIns)
          tmpPanel.Alignment = sbrCenter
          tmpPanel.MinWidth = 750
          tmpPanel.Width = 750
      End Select
    End If
  End With
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub ExpandSelection(rtfLogic As RichEdAGI, ByVal blnInQuotes As Boolean, Optional ByVal Force As Boolean = false)
  
  //if the current selection is an insertion point
  //(startpos = endpos), it will expand the selection
  //to encompass the current word (expanding both
  //forwards and backwards)
  
  // if Force is true, it will expand even if something is selected
  
  On Error GoTo ErrHandler
  
  Dim lngLineStart As Long, lngStart As Long, lngEnd As Long
  Dim i As Long, j As Long, lngChar As Long
  Dim rtn As Long, strLine As String, lngPos As Long
  
  If rtfLogic.Selection.Range.Length != 0 And Not Force Then
    Exit Sub
  End If
  
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
  lngPos = lngPos + 1
  
  If blnInQuotes Then
    //move startpos backward until separator found
    
    //i is relative position of starting point in current line;
    //start with i pointing to previous char, then enter do loop
    //don't forget to adjust startpos by 1 too)
    i = lngPos - lngLineStart - 1
    
    Do While i >= 1
      Select Case AscW(Mid$(strLine, i))
      Case 34
        //if valid quote, stop
        If IsValidQuote(strLine, i) Then
          //starting quote found
          lngStart = lngStart - 1
          Exit Do
        End If
        
      Case Else
        //everything else is OK
      End Select
      i = i - 1
      lngStart = lngStart - 1
    Loop
    
    //move endpos forward until separator found
    j = lngPos - lngLineStart + 1
    lngEnd = lngPos
    Do While j < Len(strLine)
      Select Case AscW(Mid$(strLine, j))
      Case 34
        //if valid quote, stop
        If IsValidQuote(strLine, i) Then
          //ending quote found
          lngEnd = lngEnd + 1
          Exit Do
        End If
        
      Case Else
        //everything else is ok
      End Select
      j = j + 1
      lngEnd = lngEnd + 1
    Loop
  
  Else
    //i is relative position of starting point in current line;
    //start with i pointing to previous char, then enter do loop
    i = lngPos - lngLineStart //- 1
    
    // if forcing, only expand if starting char is a token char
    If Force Then
      lngChar = AscW(Mid(strLine, i))
      Select Case lngChar
      Case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122, Is > 126
        //separators are any character EXCEPT:
        // #, $, %, ., 0-9, @, A-Z, _, a-z, and all extended characters
        //(codes 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122)
      Case Else
        Exit Sub
      End Select
    End If
    //char is a word token; back up one more an start the loop
    i = i - 1
    
    //move startpos backward until separator found
    Do While i >= 1
      lngChar = AscW(Mid$(strLine, i, 1))
      Select Case lngChar
      Case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122, Is > 126
        //these are OK
        
      Case Else
        //everything else is no good
        Exit Do
      End Select
      i = i - 1
      lngStart = lngStart - 1
    Loop
    
    //move endpos forward until separator found
    j = lngEnd - lngLineStart + 1
    Do While j < Len(strLine)
      lngChar = AscW(Mid$(strLine, j, 1))
      Select Case AscW(Mid$(strLine, j))
      Case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122, Is > 126
        //these are OK
        
      Case Else
        //everything else is no good
        Exit Do
      End Select
      j = j + 1
      lngEnd = lngEnd + 1
    Loop
  End If
  
  //set start and end pos (need to freeze the
  //control so it won//t scroll horizontally if
  //the new start location is at beginning of
  //a line where previous line extends past
  //the edge of visible text)
  
  //if no need to expand
  If rtfLogic.Selection.Range.StartPos = lngStart And rtfLogic.Selection.Range.EndPos = lngEnd Then
    //just exit
    Exit Sub
  End If

  rtfLogic.Freeze
  rtfLogic.Selection.Range.StartPos = lngStart
  rtfLogic.Selection.Range.EndPos = lngEnd
  //there is a very strange glitch here! if the original cursor
  // position is directly in front of a period, the startpos
  //does change when trying to set it to lngStart on first try
  // a second call will then set it
  If rtfLogic.Selection.Range.StartPos != lngStart Then
    rtfLogic.Selection.Range.StartPos = lngStart
  End If
  rtfLogic.Unfreeze
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Sub RemovePicture(ByVal PicNum As Byte)
  //removes a picture from the game, and updates
  //preview and resource windows
  //
  //and deletes resource file from source directory
  
  Dim i As Long, strPicFile As String
  
  On Error GoTo ErrHandler
  
  strPicFile = ResDir + Pictures(PicNum).ID + ".agp"
  
  If Not Pictures.Exists(PicNum) Then
    //raise error
    On Error GoTo 0: Err.Raise vbObjectError + 502, "ResMan", "Invalid Picture number passed to RemovePicture (picture does not exist)"
    Exit Sub
  End If
  
  //remove it from game
  Pictures.Remove PicNum
  
  Select Case Settings.ResListType
  Case 1
    With frmMDIMain.tvwResources
      //remove it from resource list
      .Nodes.Remove frmMDIMain.tvwResources.Nodes("p" + CStr(PicNum)).Index
      
      //update selection to whatever is now the selected node
      frmMDIMain.LastIndex = -1
      
      If .SelectedItem.Parent Is Nothing Then
        //it//s the game node
        frmMDIMain.SelectResource rtGame, -1
      ElseIf .SelectedItem.Parent.Parent Is Nothing Then
        //it//s a resource header
        frmMDIMain.SelectResource .SelectedItem.Index - 2, -1
      Else
        //it//s a resource
        frmMDIMain.SelectResource .SelectedItem.Parent.Index - 2, CLng(.SelectedItem.Tag)
      End If
    End With
    
  Case 2
    //only need to remove if pictures are listed
    If frmMDIMain.cmbResType.ListIndex = 2 Then
      //remove it
      frmMDIMain.lstResources.ListItems.Remove frmMDIMain.lstResources.ListItems("p" + CStr(PicNum)).Index
      //use click event to update
      frmMDIMain.lstResources_Click
      frmMDIMain.lstResources.SelectedItem.Selected = true
    End If
  End Select
    
  //if an editor is open
  For i = 1 To PictureEditors.Count
    If PictureEditors(i).InGame And PictureEditors(i).PicNumber = PicNum Then
      //set number to -1 to force close
      PictureEditors(i).PicNumber = -1
      //close it
      Unload PictureEditors(i)
      Exit For
    End If
  Next i
  
  //disposition any existing resource file
  If FileExists(strPicFile) Then
    KillCopyFile strPicFile, Settings.RenameDelRes
  End If

  //update the logic tooltip lookup table
  With IDefLookup(PicNum + 768)
    .Name = ""
    .Value = ""
    .Type = 11 //set to a value > highest type
  End With
  //then let open logic editors know
  If LogicEditors.Count > 0 Then
    For i = 1 To LogicEditors.Count
      LogicEditors(i).ListDirty = true
    Next i
  End If
Exit Sub

ErrHandler:
  //if error is invalid resid,
  If Err.Number = vbObjectError + 617 Then
    //pass it along
    On Error GoTo 0: Err.Raise Err.Number, Err.Source, Err.Description
    Exit Sub
  End If
  Resume Next
End Sub
public Sub RemoveSound(ByVal SndNum As Byte)
  //removes a view from the game, and updates
  //preview and resource windows
  //
  //and deletes resource file in source directory
  
  Dim i As Long, strSndFile As String
  
  On Error GoTo ErrHandler
  
  strSndFile = ResDir + Sounds(SndNum).ID + ".ags"
  
  If Not Sounds.Exists(SndNum) Then
    //raise error
    On Error GoTo 0: Err.Raise vbObjectError + 503, "ResMan", "Invalid Sound number passed to RemoveSound (sound does not exist)"
    Exit Sub
  End If
  
  //remove it from game
  Sounds.Remove SndNum
  
  Select Case Settings.ResListType
  Case 1
    With frmMDIMain.tvwResources
      //remove it from resource list
      .Nodes.Remove frmMDIMain.tvwResources.Nodes("s" + CStr(SndNum)).Index
      
      //update selection to whatever is now the selected node
      frmMDIMain.LastIndex = -1
      
      If .SelectedItem.Parent Is Nothing Then
        //it//s the game node
        frmMDIMain.SelectResource rtGame, -1
      ElseIf .SelectedItem.Parent.Parent Is Nothing Then
        //it//s a resource header
        frmMDIMain.SelectResource .SelectedItem.Index - 2, -1
      Else
        //it//s a resource
        frmMDIMain.SelectResource .SelectedItem.Parent.Index - 2, CLng(.SelectedItem.Tag)
      End If
    End With
  
  Case 2
    //only need to remove if sounds are listed
    If frmMDIMain.cmbResType.ListIndex = 3 Then
      //remove it
      frmMDIMain.lstResources.ListItems.Remove frmMDIMain.lstResources.ListItems("s" + CStr(SndNum)).Index
      //use click event to update
      frmMDIMain.lstResources_Click
      frmMDIMain.lstResources.SelectedItem.Selected = true
    End If
  End Select
  
  
  //if an editor is open
  For i = 1 To SoundEditors.Count
    If SoundEditors(i).InGame And SoundEditors(i).SoundNumber = SndNum Then
      //set number to -1 to force close
      SoundEditors(i).SoundNumber = -1
      //close it
      Unload SoundEditors(i)
      Exit For
    End If
  Next i
  
  If FileExists(strSndFile) Then
    KillCopyFile strSndFile, Settings.RenameDelRes
  End If
  
  //update the logic tooltip lookup table
  With IDefLookup(SndNum + 512)
    .Name = ""
    .Value = ""
    .Type = 11 //set to a value > highest type
  End With
  //then let open logic editors know
  If LogicEditors.Count > 0 Then
    For i = 1 To LogicEditors.Count
      LogicEditors(i).ListDirty = true
    Next i
  End If
Exit Sub

ErrHandler:
  //if error is invalid resid,
  If Err.Number = vbObjectError + 617 Then
    //pass it along
    On Error GoTo 0: Err.Raise Err.Number, Err.Source, Err.Description
    Exit Sub
  End If

  Resume Next
End Sub

public Sub RemoveView(ByVal ViewNum As Byte)
  //removes a view from the game, and updates
  //preview and resource windows
  //
  //and deletes resource file from source dir
  
  Dim i As Long, strViewFile As String
  
  On Error GoTo ErrHandler
  
  strViewFile = ResDir + Views(ViewNum).ID + ".agv"
  
  If Not Views.Exists(ViewNum) Then
    //raise error
    On Error GoTo 0: Err.Raise vbObjectError + 504, "ResMan", "Invalid View number passed to RemoveView (view does not exist)"
    Exit Sub
  End If
  
  //remove it from game
  Views.Remove ViewNum
  
  Select Case Settings.ResListType
  Case 1
    With frmMDIMain.tvwResources
      //remove it from resource list
      .Nodes.Remove frmMDIMain.tvwResources.Nodes("v" + CStr(ViewNum)).Index
      
      //update selection to whatever is now the selected node
      frmMDIMain.LastIndex = -1
      
      If .SelectedItem.Parent Is Nothing Then
        //it//s the game node
        frmMDIMain.SelectResource rtGame, -1
      ElseIf .SelectedItem.Parent.Parent Is Nothing Then
        //it//s a resource header
        frmMDIMain.SelectResource .SelectedItem.Index - 2, -1
      Else
        //it//s a resource
        frmMDIMain.SelectResource .SelectedItem.Parent.Index - 2, CLng(.SelectedItem.Tag)
      End If
    End With
  
  Case 2
    //only need to remove if views are listed
    If frmMDIMain.cmbResType.ListIndex = 4 Then
      //remove it
      frmMDIMain.lstResources.ListItems.Remove frmMDIMain.lstResources.ListItems("v" + CStr(ViewNum)).Index
      //use click event to update
      frmMDIMain.lstResources_Click
      frmMDIMain.lstResources.SelectedItem.Selected = true
    End If
  End Select
  
  //if an editor is open
  For i = 1 To ViewEditors.Count
    If ViewEditors(i).InGame And ViewEditors(i).ViewNumber = ViewNum Then
      //set number to -1 to force close
      ViewEditors(i).ViewNumber = -1
      //close it
      Unload ViewEditors(i)
      Exit For
    End If
  Next i

  //disposition any existing resource file
  If FileExists(strViewFile) Then
    KillCopyFile strViewFile, Settings.RenameDelRes
  End If
  
  //update the logic tooltip lookup table
  With IDefLookup(ViewNum + 256)
    .Name = ""
    .Value = ""
    .Type = 11 //set to a value > highest type
  End With
  //then let open logic editors know
  If LogicEditors.Count > 0 Then
    For i = 1 To LogicEditors.Count
      LogicEditors(i).ListDirty = true
    Next i
  End If
Exit Sub

ErrHandler:
  //if error is invalid resid,
  If Err.Number = vbObjectError + 617 Then
    //pass it along
    On Error GoTo 0: Err.Raise Err.Number, Err.Source, Err.Description
    Exit Sub
  End If

  Resume Next
End Sub

public Sub KillCopyFile(ByVal ResFile As String, ByVal KeepOld As Boolean)

  Dim strOldName As String, lngNextNum As Long, strName As String, strExt As String
  
  On Error Resume Next
  
  If FileExists(ResFile) Then
    If KeepOld Then
      strName = Left(ResFile, Len(ResFile) - 4)
      strExt = Right(ResFile, 4)
      lngNextNum = 1
      //assign proposed rename
      strOldName = strName + "_OLD" + strExt
      //Validate it
      Do Until Not FileExists(strOldName)
        lngNextNum = lngNextNum + 1
        strOldName = strName + "_OLD_" + CStr(lngNextNum) + strExt
      Loop
      FileCopy ResFile, strOldName
    End If
    //kill the file in source directory (if it//s not there, error just gets ignored...)
    Kill ResFile
  End If

End Sub
public Function RenumberResource(ByVal OldResNum As Byte, ByVal ResType As AGIResType) As Byte

  //renumbers a resource; return Value is the new number
  
  Dim tmpNode As Node, tmpListItem As ListItem, tvwRel As TreeRelationshipConstants
  Dim strResType As String, strCaption As String
  Dim NewResNum As Byte, i As Long
  
  On Error GoTo ErrHandler
  
  //default to old number, in case user cancels
  NewResNum = OldResNum
  
  //show renumber resoure form
  With frmGetResourceNum
    .ResType = ResType
    .OldResNum = OldResNum
    .WindowFunction = grRenumber
    //setup before loading so ghosts don't show up
    .FormSetup
    .Show vbModal, frmMDIMain
  End With
  
  //if user makes a choice AND number is different
  If Not frmGetResourceNum.Canceled And frmGetResourceNum.NewResNum != OldResNum Then
    //get new number
    NewResNum = frmGetResourceNum.NewResNum
      
    //change number for this resource
    Select Case ResType
    Case rtLogic
      //renumber it
      Logics.Renumber OldResNum, NewResNum
      strResType = "l"
      strCaption = ResourceName(Logics(NewResNum), true)
      
    Case rtPicture
      //renumber it
      Pictures.Renumber OldResNum, NewResNum
      strResType = "p"
      strCaption = ResourceName(Pictures(NewResNum), true)
      
    Case rtSound
      //renumber it
      Sounds.Renumber OldResNum, NewResNum
      strResType = "s"
      strCaption = ResourceName(Sounds(NewResNum), true)
      
    Case rtView
      //renumber it
      Views.Renumber OldResNum, NewResNum
      strResType = "v"
      strCaption = ResourceName(Views(NewResNum), true)
    End Select
    
    //update resource list
    Select Case Settings.ResListType
    Case 1
      With frmMDIMain.tvwResources
        //remove the old node
        .Nodes.Remove strResType + CStr(OldResNum)
      
        //start with first node of this Type
        Set tmpNode = .Nodes(ResType + 2).Child
        
        //if there are no nodes
        If tmpNode Is Nothing Then
          //add first child
          Set tmpNode = .Nodes(ResType + 2)
          tvwRel = tvwChild
        //if this node belongs at end of list
        ElseIf NewResNum > tmpNode.LastSibling.Tag Then
          //add to end
          Set tmpNode = tmpNode.LastSibling
          tvwRel = tvwNext
        Else
          //get position which should immediately follow this resource
          //step through until a node is found that is past this new number
          Do Until CByte(tmpNode.Tag) > NewResNum
            Set tmpNode = tmpNode.Next
          Loop
          tvwRel = tvwPrevious
        End If
        
        //put the resource in it//s new location
        .Nodes.Add(tmpNode.Index, tvwRel, strResType + CStr(NewResNum), strCaption).Selected = true
        .SelectedItem.Tag = NewResNum
        .SelectedItem.EnsureVisible
        //if node is a logic
        If ResType = rtLogic Then
          //highlight in red if not compiled
          If Not Logics(NewResNum).Compiled Then
            .SelectedItem.ForeColor = vbRed
          End If
        End If
        
        //update by re-selecting
        frmMDIMain.LastIndex = -1
        
        If .SelectedItem.Parent Is Nothing Then
          //it//s the game node
          frmMDIMain.SelectResource rtGame, -1
        ElseIf .SelectedItem.Parent.Parent Is Nothing Then
          //it//s a resource header
          frmMDIMain.SelectResource .SelectedItem.Index - 2, -1
        Else
          //it//s a resource
          frmMDIMain.SelectResource .SelectedItem.Parent.Index - 2, CLng(.SelectedItem.Tag)
        End If
      End With
    Case 2
      //only update if the resource type is being listed
      If frmMDIMain.cmbResType.ListIndex - 1 = ResType Then
        With frmMDIMain.lstResources.ListItems
          //remove it from current location
          .Remove strResType + CStr(OldResNum)
          
          //if nothing left
          If .Count = 0 Then
            //add it as first item
            Set tmpListItem = .Add(, strResType + CStr(NewResNum), ResourceName(Pictures(NewResNum), true))
        
          Else
            //get index position to add a new one
            For i = 1 To .Count
              If NewResNum < CByte(.Item(i).Tag) Then
                Exit For
              End If
            Next i
            //add it at this index point
            Set tmpListItem = .Add(i, strResType + CStr(NewResNum), ResourceName(Pictures(NewResNum), true))
          End If
          
          //add  tag
          tmpListItem.Tag = NewResNum
          
          //if node is a logic
          If ResType = rtLogic Then
            //highlight in red if not compiled
            If Not Logics(NewResNum).Compiled Then
              tmpListItem.ForeColor = vbRed
            End If
          End If
           
          //select it
          tmpListItem.Selected = true
          
          //use click event to update
          frmMDIMain.lstResources_Click
        End With
      End If
    End Select
  End If
  
  //unload the get resource number form
  Unload frmGetResourceNum
  
  //return the new number
  RenumberResource = NewResNum
Exit Function

ErrHandler:
  Select Case Err.Number
  Case vbObjectError + 564
    MsgBox "You attempted to change a resource to a number that is already in use. Try renumbering again, with an unused resource number.", vbInformation, "Renumber Resource Error"
    
  Case Else
    ErrMsgBox "Error while renumbering:", "Resource list may not display correct numbers. Close/reopen game to refresh.", "Renumber Resource Error"
  End Select
  
  //unload the get resource number form
  Unload frmGetResourceNum
  
  Err.Clear
End Function

public Sub ReplaceAll(ByVal FindText As String, ByVal ReplaceText As String, ByVal FindDir As FindDirection, _
                      ByVal MatchWord As Boolean, ByVal MatchCase As Boolean, ByVal LogicLoc As FindLocation, Optional ByVal SearchType As AGIResType = rtNone)
// replace all doesn//t use or need direction
  Dim i As Long, LogNum As Long
  
  On Error GoTo ErrHandler
  
  //if search Type is defines, words or objects, the editor does progress status and msgs
  
  //if replacing and text is the same
  If (StrComp(FindText, ReplaceText, IIf(MatchCase, vbBinaryCompare, vbTextCompare)) = 0) Then
    //restore mouse, reneable main form, and exit
    //Debug.Assert Screen.MousePointer = vbDefault
    Screen.MousePointer = vbDefault
    //Debug.Assert frmMDIMain.Enabled = true
    frmMDIMain.Enabled = true
    Exit Sub
  End If
  
  //find text can//t be blank
  If (LenB(FindText) = 0) Then
    //restore mouse, reneable main form, and exit
    Screen.MousePointer = vbDefault
    frmMDIMain.Enabled = true
    Exit Sub
  End If
  
  //not all searches use the progress bar
  Select Case SearchType
  Case rtNone, rtLogic, rtPicture, rtSound, rtView
    //show wait cursor
    WaitCursor
    //disable main form
    frmMDIMain.Enabled = false
    //refresh (normal DoEvents here; otherwise SafeDoEvents will re-enable the form)
    DoEvents
  End Select

  Select Case LogicLoc
  Case flCurrent
    ReplaceAllWords FindText, ReplaceText, MatchWord, MatchCase, true, Nothing, SearchForm
  
  Case flOpen
    //replace in all open logic and text editors
    
    //show progress form
    Load frmProgress
    With frmProgress
      .Caption = "Replace All"
      .lblProgress.Caption = "Searching..."
      .pgbStatus.Max = LogicEditors.Count
      .Show vbModeless, frmMDIMain
      .Refresh
    End With

    //replace in all open editors
    For i = 1 To LogicEditors.Count
      //update progress bar
      frmProgress.pgbStatus.Value = i - 1
      If LogicEditors(i).Name = "frmLogicEdit" Then
        //if a logic, show the logic ID
        frmProgress.lblProgress.Caption = "Searching " + LogicEditors(i).LogicEdit.ID + "..."
      Else
        //if a text file, show the filename
        frmProgress.lblProgress.Caption = "Searching " + JustFileName(LogicEditors(i).FileName) + "..."
      End If
      //replace
      ReplaceAllWords FindText, ReplaceText, MatchWord, MatchCase, true, Nothing, LogicEditors(i)
    Next i
    
    //close the progress form
    Unload frmProgress
    
  Case flAll
    //replace in all ingame logics; does NOT include any open editors
    //which are text files or Not InGame
  
    //if replacing globals, don't use the progress form
    //it//s already being used to track the globals being searched
    If SearchType != rtGlobals Then
      //show progress form
      Load frmProgress
      With frmProgress
        Select Case SearchType
        Case rtNone
          .Caption = "Replace All"
          .lblProgress.Caption = "Searching..."
        Case Else
          .Caption = "Updating Resource ID"
          .lblProgress.Caption = "Searching..."
        End Select
        
        .pgbStatus.Max = Logics.Count
        .pgbStatus.Value = 0
        .Show vbModeless, frmMDIMain
        .Refresh
      End With
    End If
    
    //replace in all open editors
    For i = 1 To LogicEditors.Count
      //only logic editors
      If LogicEditors(i).Name = "frmLogicEdit" Then
        //and only if in game
        If LogicEditors(i).InGame Then
          //if not updating global defines,
          If SearchType != rtGlobals Then
            //update progress bar
            With frmProgress
              .pgbStatus.Value = frmProgress.pgbStatus.Value + 1
              //show the logic ID
              .lblProgress.Caption = "Searching " + LogicEditors(i).LogicEdit.ID + "..."
              .Refresh
            End With
          End If
          //replace
          ReplaceAllWords FindText, ReplaceText, MatchWord, MatchCase, true, Nothing, LogicEditors(i), SearchType
        End If
      End If
    Next i
    
    //now do all closed logics
    
    //get first available logic number
    LogNum = NextClosedLogic(-1)
    
    Do Until LogNum = -1
      Select Case SearchType
      Case rtNone, rtLogic, rtPicture, rtSound, rtView
        //update progress bar
        frmProgress.pgbStatus.Value = frmProgress.pgbStatus.Value + 1
        frmProgress.lblProgress.Caption = "Searching " + Logics(LogNum).ID + "..."
        //refresh window
        frmProgress.Refresh
      End Select
      
      //if not loaded
      If Not Logics(LogNum).Loaded Then
        Logics(LogNum).Load
      End If
      ReplaceAllWords FindText, ReplaceText, MatchWord, MatchCase, false, Logics(LogNum), Nothing, SearchType
      
      //if dirty
      If Logics(LogNum).SourceDirty Then
        //save logic source
        Logics(LogNum).SaveSource
        UpdateSelection rtLogic, LogNum, umPreview
      End If
      
      //unload it
      Logics(LogNum).Unload
      //get next logic
      LogNum = NextClosedLogic(LogNum)
      //increment counter
      i = i + 1
    Loop
    
    Select Case SearchType
    Case rtNone, rtLogic, rtPicture, rtSound, rtView
      //close the progress form
      Unload frmProgress
    End Select
  End Select
    
  Select Case SearchType
  Case rtNone, rtLogic, rtPicture, rtSound, rtView
    If SearchType = rtNone Then
      //if found,
      If ReplaceCount > 0 Then
        MsgBox "The specified region has been searched. " + CStr(ReplaceCount) + " replacements were made.", vbInformation, "Replace All"
      Else
        MsgBox "Search text not found.", vbInformation, "Replace All"
      End If
    End If
    
    //enable form and reset cursor
    frmMDIMain.Enabled = true
    Screen.MousePointer = vbDefault
    
    // have to set focus to main form in order get the child forms
    // to properly switch focus (the searching logic should always
    // get the focus after a replace all; not the FindForm)
    frmMDIMain.SetFocus
  End Select
    
  //reset search flags
  FindForm.ResetSearch
  
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

Private Sub ReplaceAllWords(ByVal FindText As String, ByVal ReplaceText As String, _
                            ByVal MatchWord As Boolean, ByVal MatchCase As Boolean, _
                            Optional ByVal InWindow As Boolean, _
                            Optional SearchLogic As AGILogic, _
                            Optional SearchWin As Form, _
                            Optional ByVal SearchType As AGIResType = rtNone)


  //replaces text in either a logic, or a textbox
  //calling function MUST ensure a valid reference to a richtextbox
  //or a logic is passed

  Dim FoundPos As Long
  Dim vbcComp As VbCompareMethod
  Dim i As Long
  Dim lngStart As Long, lngEnd As Long
  Dim lngOldFC As Long, blnOldBold As Boolean, blnOldItal As Boolean

  On Error GoTo ErrHandler

  //if NOT a regular search
  If SearchType != rtNone Then
    //ignore text editors
    If InWindow Then
      If SearchWin.Name = "frmTextEdit" Then
        Exit Sub
      End If
    End If
  End If
  
  //set comparison method for string search,
  vbcComp = CLng(MatchCase) + 1 // CLng(true) + 1 = 0 = vbBinaryCompare; Clng(false) + 1 = 1 = vbTextCompare

  //start at beginning
  FoundPos = 1

  Do
    //if searching whole word
    If MatchWord Then
      //if in a window
      If InWindow Then
        FoundPos = FindWholeWord(FoundPos, SearchWin.rtfLogic.Text, FindText, MatchCase, false, SearchType)
      Else
        FoundPos = FindWholeWord(FoundPos, SearchLogic.SourceText, FindText, MatchCase, false, SearchType)
      End If
    Else
      //if in a window
      If InWindow Then
        FoundPos = InStr(FoundPos, SearchWin.rtfLogic.Text, FindText, vbcComp)
      Else
        FoundPos = InStr(FoundPos, SearchLogic.SourceText, FindText, vbcComp)
      End If
    End If

    If FoundPos > 0 Then
      //if replacing in a window
      If InWindow Then
        //instr is 1 based; richedit boxes are 0 based
        //so need to subtract one from foundpos
        
        lngStart = FoundPos - 1
        lngEnd = FoundPos - 1 + Len(FindText)
        With SearchWin.rtfLogic.Range(lngStart, lngEnd)
          //if selected text does not match,
          If LCase$(.Text) != LCase$(FindText) Then
            //error
            MsgBox "An error was encountered in the replace feature of the rich text" + vbNewLine + "editor. Not all instances of search text have been replaced.", vbCritical + vbOKOnly, "Replace All Error"
            Exit Sub
          End If
          
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
      Else
        //replace word in logic source
        SearchLogic.SourceText = Left$(SearchLogic.SourceText, FoundPos - 1) + ReplaceText + Right$(SearchLogic.SourceText, Len(SearchLogic.SourceText) - Len(FindText) - FoundPos + 1)
      End If

      //reset foundpos
      FoundPos = FoundPos + Len(ReplaceText)
      //increment Count
      ReplaceCount = ReplaceCount + 1
    Else
      Exit Do
    End If
  Loop
  
Exit Sub

ErrHandler:
  //Debug.Assert false
  Resume Next
End Sub

public Function ResourceName(ThisResource As Object, ByVal IsInGame As Boolean, Optional ByVal NoNumber As Boolean = false) As String

  //formats resource name based on user preference
  //format includes: option for upper, lower or title case of Type;
  //                 space or period for separator;
  //                 forcing number to include leading zeros
                 
  //if the resource is not part of a game,
  //the ID is returned regardless of ID/number setting
  
  On Error GoTo ErrHandler
  
  //if using numbers AND resource is ingame,
  If Settings.ShowResNum And IsInGame Then
    With Settings.ResFormat
      ResourceName = StrConv(ResTypeName(CInt(ThisResource.Resource.ResType)), .NameCase) + .Separator + format$(ThisResource.Number, .NumFormat)
    End With
  Else
    If Settings.IncludeResNum And IsInGame And Not NoNumber Then
      ResourceName = ThisResource.Number + " - "
    End If
    ResourceName = ResourceName + ThisResource.ID
  End If
Exit Function

ErrHandler:
  Resume Next
End Function


public Sub UpdateExitInfo(ByVal Reason As EUReason, ByVal LogicNumber As Long, ThisLogic As AGILogic, Optional ByVal NewNum As Long)
//   frmMDIMain|SelectedItemRenumber:  UpdateExitInfo euRenumberRoom, OldResNum, Nothing, NewResNum
//  frmMDIMain|lstProperty_LostFocus:  UpdateExitInfo Reason, SelResNum, Logics(SelResNum) //showroom or removeroom
//frmMDIMain|picProperties_MouseDown:  UpdateExitInfo Reason, SelResNum, Logics(SelResNum) //showroom or removeroom
//                ResMan|AddNewLogic:
//                   ResMan|NewLogic:  UpdateExitInfo euShowRoom, .NewResNum, Logics(.NewResNum)
//                ResMan|RemoveLogic:  UpdateExitInfo euRemoveRoom, LogicNum, Nothing
//        frmLogicEdit|MenuClickSave:  UpdateExitInfo euUpdateRoom, LogicEdit.Number, LogicEdit
//    frmLogicEdit|MenuClickRenumber:  UpdateExitInfo euRenumberRoom, OldResNum, Nothing, NewResNum
  
  //updates the layout editor (if it is open) and the layout file
  //(if there is one) whenever exit info for a room is changed
  //(including when IsRoom property is changed, or when a room is
  //deleted from the game)
   
  Dim tmpExits As AGIExits
  Dim blnSave As Boolean, blnShow As Boolean
  
  On Error GoTo ErrHandler
  
  //is there an existing layout editor file?
  blnSave = FileExists(GameDir + GameID + ".wal")
  
  //if layout file does not exist AND not editing layout
  If Not blnSave And Not LEInUse Then
    //no file, and editor is not in use;
    //no updates are necessary
    Exit Sub
  End If
  
  //if adding new room, showing existing room, or updating an existing room,
  If Reason = euAddRoom Or Reason = euShowRoom Or Reason = euUpdateRoom Then
    //get new exits from the logic that was passed
    Set tmpExits = ExtractExits(ThisLogic)
  End If
  
  //if a layout file exists, it needs to be updated too
  If blnSave Then
    //add line to output file
    UpdateLayoutFile Reason, LogicNumber, tmpExits, NewNum
  End If
  
  //if layout editor is open
  If LEInUse Then
    //use layout editor update method
    LayoutEditor.UpdateLayout Reason, LogicNumber, tmpExits
    //and redraw to refresh the editor
    LayoutEditor.DrawLayout true
  End If
Exit Sub

ErrHandler:
  Resume Next
End Sub

public Function ParseExits(strExitInfo As String, strVer As String) As AGIExits
  //parses the string that contains exit info that comes from the layout editor
  //data file
  //if error is encountered,
  
  Dim strExit() As String, strData() As String
  Dim i As Long, offset As Long, Size As Long
  
  Set ParseExits = New AGIExits
  
  On Error GoTo ErrHandler
  
  //ver 10,11: R|##|v|o|x|y|index:room:reason:style:xfer:leg:spx:spy:epx:epy|...
  //ver 12:    R|##|v|o|p|x|y|index:room:reason:style:xfer:leg|...
  
  Select Case strVer
  Case "10", "11"
    offset = 6
    Size = 9
  Case "12", "21"
    offset = 7
    Size = 5
  End Select
  
  strData = Split(strExitInfo, "|")
  
  //get exit info
  For i = 0 To UBound(strData) - offset
    strExit = Split(strData(i + offset), ":")
    //ver 10 and 11 have ten elements,
    //ver 12 needs just 6
    If UBound(strExit) = Size Then
      
      //add new exit, and flag as new, in source
      ParseExits.Add(strExit(0), strExit(1), strExit(2), strExit(3), strExit(4), strExit(5)).Status = esOK
      
//      With ParseExits.Item(ParseExits.Count - 1)
//        .SPX = CSng(strExit(6))
//        .SPY = CSng(strExit(7))
//        .EPX = CSng(strExit(8))
//        .EPY = CSng(strExit(9))
//      End With
      
    End If
  Next i
Exit Function

ErrHandler:
  //error!
  //Debug.Assert false
  Resume Next
  Set ParseExits = Nothing
End Function
      */
    }
public static void WaitCursor()
 {
      //called whenever a wait cursor is needed due to a long-running process
      //use screen object//s cursor, which covers entire app

      MDIMain.Cursor = Cursors.WaitCursor;
}
    public static void ShowAGIBitmap(PictureBox pic, Bitmap agiBMP, int scale = 1)
    {
      //to scale the picture without blurring, need to use NearestNeighbor interpolation
      // that can't be set directly, so a graphics object is needed to draw the
      // the picture
      int bWidth = agiBMP.Width * scale * 2, bHeight = agiBMP.Height * scale;
      // first, create new image in the picture box that is desired size
      pic.Image = new Bitmap(bWidth, bHeight);
      // intialize a graphics object for the image just created
      using Graphics g = Graphics.FromImage(pic.Image);
      // set correct interpolation mode
      g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
      // draw the bitmap, at correct resolution
      g.DrawImage(agiBMP, 0, 0, bWidth, bHeight);
    }
  }
}
