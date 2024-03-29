using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using WinAGI.Engine;
using System.Diagnostics.CodeAnalysis;

namespace WinAGI.Common {
    public static partial class API {
        //constants used to build bitmaps
        public const int BI_RGB = 0;
        public const int DIB_RGB_COLORS = 0;
        public const int DIB_PAL_COLORS = 1;
        public const int FLOODFILLBORDER = 0;
        public const int FLOODFILLSURFACE = 1;
        public const int BLACKNESS = 0x42;
        public const int DSTINVERT = 0x550009;
        public const int MERGECOPY = 0xC000CA;
        public const int MERGEPAINT = 0xBB0226;
        public const int NOTSRCCOPY = 0x330008;
        public const int NOTSRCERASE = 0x1100A6;
        public const int PATCOPY = 0xF00021;
        public const int PATINVERT = 0x5A0049;
        public const int PATPAINT = 0xFB0A09;
        public const int SRCAND = 0x8800C6;
        public const int SRCCOPY = 0xCC0020;
        public const int SRCERASE = 0x440328;
        public const int SRCINVERT = 0x660046;
        public const int SRCPAINT = 0xEE0086;
        public const int WHITENESS = 0xFF0062;
        public const int TRANSCOPY = 0xB8074A;

        //apis for midi sound handling
        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int mciSendString(string lpszCommand, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn, IntPtr hwndCallback);
        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int mciGetErrorString(int errNum, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn);
        [DllImport("hhctrl.ocx")]
        public static extern int HtmlHelpS(IntPtr hwndCaller, string pszFile, int uCommand, string dwData);
        [DllImport("hhctrl.ocx")]
        public static extern int HtmlHelp(IntPtr hwndCaller, string pszFile, int uCommand, int dwData);
        public const int HH_DISPLAY_TOPIC = 0x0;
        public const int HH_DISPLAY_INDEX = 0x2;
        public const int HH_DISPLAY_SEARCH = 0x3;
        public const int HH_DISPLAY_TOC = 0x1;
        public const int HH_SET_WIN_TYPE = 0x4;
        public const int HH_GET_WIN_TYPE = 0x5;
        public const int HH_GET_WIN_HANDLE = 0x6;
        // Display string resource ID or text in a popupwin.
        public const int HH_DISPLAY_TEXT_POPUP = 0xE;
        // Display mapped numeric Value in dwdata
        public const int HH_HELP_CONTEXT = 0xF;
        // Text pop-up help, similar to WinHelp's HELP_CONTEXTMENU
        public const int HH_TP_HELP_CONTEXTMENU = 0x10;
        // Text pop-up help, similar to WinHelp's HELP_WM_HELP
        public const int HH_TP_HELP_WM_HELP = 0x11;

        [DllImport("kernel32", EntryPoint = "GetShortPathName", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetShortPathName(string longPath, StringBuilder shortPath, int bufSize);
        static void tmpAPIFunctions() {
            /*
        //subclassing variables
        internal PrevLEWndProc As Long
        internal PrevTbarWndProc As Long
        internal PrevPropWndProc As Long

        internal TBCmd As Long //identifies which menu command is chosen

        //savefile configuration
        internal ofn As OPENFILENAME

        //start directory for directory browser
        internal string BrowserStartDir

        internal string SelectedFolder

        //gdiplus token
        internal gdiToken As Long
        internal NoGDIPlus As Boolean

        internal const WM_USER = 0x400
        //edit box messages
        internal const EM_GETSEL = 0xB0
        internal const EM_SETSEL = 0xB1
        internal const EM_GETRECT = 0xB2
        internal const EM_SETRECT = 0xB3
        internal const EM_SETRECTNP = 0xB4
        internal const EM_SCROLL = 0xB5
        internal const EM_LINESCROLL = 0xB6
        internal const EM_SCROLLCARET = 0xB7
        internal const EM_GETMODIFY = 0xB8
        internal const EM_SETMODIFY = 0xB9
        internal const EM_GETLINECOUNT = 0xBA
        internal const EM_LINEINDEX = 0xBB
        internal const EM_SETHANDLE = 0xBC
        internal const EM_GETHANDLE = 0xBD
        internal const EM_GETTHUMB = 0xBE
        internal const EM_LINELENGTH = 0xC1
        internal const EM_REPLACESEL = 0xC2
        internal const EM_GETLINE = 0xC4
        internal const EM_LIMITTEXT = 0xC5
        internal const EM_CANUNDO = 0xC6
        internal const EM_UNDO = 0xC7
        internal const EM_REDO = 0x400 + 84
        internal const EM_CANREDO = 0x400 + 85
        internal const EM_FORMATRANGE = WM_USER + 57
        internal const EM_FMTLINES = 0xC8
        internal const EM_LINEFROMCHAR = 0xC9
        internal const EM_SETTABSTOPS = 0xCB
        internal const EM_SETPASSWORDCHAR = 0xCC
        internal const EM_EMPTYUNDOBUFFER = 0xCD
        internal const EM_GETFIRSTVISIBLELINE = 0xCE
        internal const EM_SETREADONLY = 0xCF
        internal const EM_SETWORDBREAKPROC = 0xD0
        internal const EM_GETWORDBREAKPROC = 0xD1
        internal const EM_GETPASSWORDCHAR = 0xD2
        internal const EM_SETMARGINS = 0xD3
        internal const EM_GETMARGINS = 0xD4
        internal const EM_SETLIMITTEXT = EM_LIMITTEXT  //win40 Name change
        internal const EM_GETLIMITTEXT = 0xD5
        internal const EM_POSFROMCHAR = 0xD6
        internal const EM_CHARFROMPOS = 0xD7
        internal const EM_SETIMESTATUS = 0xD8
        internal const EM_GETIMESTATUS = 0xD9
        internal const EM_HIDESELECTION As Long = (WM_USER + 63)
        internal const WM_MOUSEFIRST = 0x200
        internal const WM_MOUSELAST = 0x209
        internal const WM_MOUSEMOVE = 0x200
        internal const WM_NCMOUSEMOVE = 0xA0

        internal const WM_LBUTTONDOWN = 0x201
        internal const WM_LBUTTONUP = 0x202
        internal const WM_LBUTTONDBLCLK = 0x203
        internal const WM_RBUTTONDOWN = 0x204
        internal const WM_RBUTTONUP = 0x205
        internal const WM_RBUTTONDBLCLK = 0x206
        internal const WM_MBUTTONDOWN = 0x207
        internal const WM_MBUTTONUP = 0x208
        internal const WM_MBUTTONDBLCLK = 0x209
        internal const WM_MOUSEWHEEL = 0x20A
        internal const WM_NCLBUTTONDOWN = 0xA1
        internal const WM_NCLBUTTONUP = 0xA2
        internal const WM_NCLBUTTONDBLCLK = 0xA3
        internal const WM_NCRBUTTONDOWN = 0xA4
        internal const WM_NCRBUTTONUP = 0xA5
        internal const WM_NCRBUTTONDBLCLK = 0xA6
        internal const WM_NCMBUTTONDOWN = 0xA7
        internal const WM_NCMBUTTONUP = 0xA8
        internal const WM_NCMBUTTONDBLCLK = 0xA9
        internal const WM_SETREDRAW = 0xB
        internal const WM_HSCROLL As Long = 0x114
        internal const WM_VSCROLL As Long = 0x115
        internal const WM_ACTIVATEAPP As Long = 0x1C
        internal const WM_ACTIVATE As Long = 0x6
        internal const WA_ACTIVE As Long = 1
        internal const WM_KEYDOWN As Long = 0x100
        internal const WM_KEYUP As Long = 0x101
        internal const WM_CHAR As Long = 0x102
        internal const WM_SYSCHAR As Long = 0x106
        internal const WM_SETFOCUS As Long = 0x7
        internal const MK_CONTROL = 0x8
        internal const MK_LBUTTON = 0x1
        internal const MK_RBUTTON = 0x2
        internal const MK_MBUTTON = 0x10
        internal const MK_SHIFT = 0x4

        internal const WM_NOTIFY As Long = 0x4E
        internal const WM_SYSCOMMAND As Long = 0x112
        internal const SC_NEXTWINDOW As Long = 0xF040
        internal const SC_PREVWINDOW As Long = 0xF050

        internal const CDN_FIRST As Long = (-601)
        internal const CDN_FILEOK As Long = (CDN_FIRST - 0x5)
        internal const CDN_FOLDERCHANGE As Long = (CDN_FIRST - 0x2)
        internal const CDN_HELP As Long = (CDN_FIRST - 0x4)
        internal const CDN_INCLUDEITEM As Long = (CDN_FIRST - 0x7)
        internal const CDN_INITDONE As Long = (CDN_FIRST - 0x0)
        internal const CDN_SELCHANGE As Long = (CDN_FIRST - 0x1)
        internal const CDN_SHAREVIOLATION As Long = (CDN_FIRST - 0x3)
        internal const CDN_TYPECHANGE As Long = (CDN_FIRST - 0x6)

        internal const LB_GETITEMHEIGHT = 0x1A1
        internal const SB_LINELEFT As Long = 0
        internal const SB_LINERIGHT As Long = 1
        internal const SB_LINEUP As Long = 0
        internal const SB_LINEDOWN As Long = 1
        internal const SB_PAGEUP = 0x2
        internal const GWL_WNDPROC = (-4)
        internal const EM_GETOPTIONS = (0x400 + 78)
        internal const EM_SETOPTIONS = (0x400 + 77)
        internal const EM_GETUNDONAME = (0x400 + 86)
        internal const WM_UNDO = 0x304

        internal const BITSPIXEL = 12
        internal const Planes = 14

        internal const HALFTONE As Long = 4
        internal const COLORONCOLOR = 3

        //directory browser constants
        internal const MAX_PATH = 260
        internal const BFFM_INITIALIZED = 1
        internal const BFFM_SELCHANGED = 2
        internal const BFFM_VALIDATEFAILEDA = 3
        internal const BFFM_VALIDATEFAILEDW = 4
        internal const BFFM_IUNKNOWN = 5
        internal const BFFM_SETSTATUSTEXT = (WM_USER + 100)
        internal const BFFM_ENABLEOK = (WM_USER + 101)
        internal const BFFM_SETSELECTION = (WM_USER + 102)
        internal const BIF_RETURNONLYFSDIRS = 1
        internal const BIF_DONTGOBELOWDOMAIN = 2
        internal const BIF_STATUSTEXT = 4
        internal const BIF_USENEWUI = 64

        internal const SWP_NOACTIVATE As Long = 0x10
        internal const SWP_NOMOVE As Long = 0x2
        internal const SWP_NOOWNERZORDER As Long = 0x200
        internal const SWP_NOSIZE As Long = 0x1
        internal const HWND_TOPMOST As Long = -1
        internal const HWND_TOP As Long = 0
        internal const HWND_NOTOPMOST As Long = -2


        internal const ALTERNATE = 1

        //printer capabilities
        internal const PHYSICALHEIGHT As Long = 111
        internal const PHYSICALOFFSETX As Long = 112
        internal const PHYSICALOFFSETY As Long = 113
        internal const PHYSICALWIDTH As Long = 110
        internal const RASTERCAPS As Long = 38
        internal const RC_BANDING As Long = 2
        internal const RC_BITBLT As Long = 1
        internal const RC_BITMAP64 As Long = 8
        internal const RC_DI_BITMAP As Long = 0x80
        internal const RC_DIBTODEV As Long = 0x200
        internal const RC_FLOODFILL As Long = 0x1000
        internal const RC_GDI20_OUTPUT As Long = 0x10
        internal const RC_PALETTE As Long = 0x100
        internal const RC_SCALING As Long = 4
        internal const RC_STRETCHBLT As Long = 0x800
        internal const RC_STRETCHDIB As Long = 0x2000
        internal const LOGPIXELSX As Long = 88
        internal const LOGPIXELSY As Long = 90
        internal const STANDARD_RIGHTS_REQUIRED As Long = 0xF0000
        internal const PRINTER_ACCESS_ADMINISTER As Long = 0x4
        internal const PRINTER_ACCESS_USE As Long = 0x8
        internal const PRINTER_ALL_ACCESS As Long = (STANDARD_RIGHTS_REQUIRED Or PRINTER_ACCESS_ADMINISTER Or PRINTER_ACCESS_USE)
        internal const DM_PROMPT As Long = 4
        internal const DM_IN_PROMPT As Long = DM_PROMPT

        //open/save dialog constants
        internal const OFN_ALLOWMULTISELECT As Long = 0x200
        internal const OFN_CREATEPROMPT As Long = 0x2000
        internal const OFN_EXPLORER As Long = 0x80000
        internal const OFN_EXTENSIONDIFFERENT As Long = 0x400
        internal const OFN_FILEMUSTEXIST As Long = 0x1000
        internal const OFN_HIDEREADONLY As Long = 0x4
        internal const OFN_LONGNAMES As Long = 0x200000
        internal const OFN_NOCHANGEDIR As Long = 0x8
        internal const OFN_NODEREFERENCELINKS As Long = 0x100000
        internal const OFN_NOLONGNAMES As Long = 0x40000
        internal const OFN_NOREADONLYRETURN As Long = 0x8000
        internal const OFN_NOVALIDATE As Long = 0x100
        internal const OFN_OVERWRITEPROMPT As Long = 0x2
        internal const OFN_PATHMUSTEXIST As Long = 0x800
        internal const OFN_READONLY As Long = 0x1
        internal const OFN_SHAREAWARE As Long = 0x4000
        internal const OFN_SHOWHELP As Long = 0x10
        internal const OFN_USEMONIKERS As Long = 0x1000000
        internal const OFN_DONTADDTORECENT As Long = 0x2000000
        internal const OFN_ENABLEINCLUDENOTIFY As Long = 0x400000
        internal const OFN_ENABLESIZING As Long = 0x800000
        internal const OFN_ENABLETEMPLATE As Long = 0x40
        internal const OFN_ENABLETEMPLATEHANDLE As Long = 0x80
        internal const OFN_FORCESHOWHIDDEN As Long = 0x10000000
        internal const OFN_NONETWORKBUTTON As Long = 0x20000
        internal const OFN_NOTESTFILECREATE As Long = 0x10000
        internal const OFN_ENABLEHOOK As Long = 0x20
        internal const OFN_EX_NOPLACESBAR As Long = 0x1
        internal const OFN_SHAREFALLTHROUGH As Long = 2
        internal const OFN_SHARENOWARN As Long = 1
        internal const OFN_SHAREWARN As Long = 0
        internal const FNERR_BUFFERTOOSMALL As Long = 0x3003
        internal const FNERR_FILENAMECODES As Long = 0x3000
        internal const FNERR_INVALIDFILENAME As Long = 0x3002
        internal const FNERR_SUBCLASSFAILURE As Long = 0x3001
        internal const CDERR_DIALOGFAILURE As Long = 0xFFFF
        internal const CDERR_FINDRESFAILURE As Long = 0x6
        internal const CDERR_GENERALCODES As Long = 0x0
        internal const CDERR_INITIALIZATION As Long = 0x2
        internal const CDERR_LOADRESFAILURE As Long = 0x7
        internal const CDERR_LOADSTRFAILURE As Long = 0x5
        internal const CDERR_LOCKRESFAILURE As Long = 0x8
        internal const CDERR_MEMALLOCFAILURE As Long = 0x9
        internal const CDERR_MEMLOCKFAILURE As Long = 0xA
        internal const CDERR_NOHINSTANCE As Long = 0x4
        internal const CDERR_NOHOOK As Long = 0xB
        internal const CDERR_NOTEMPLATE As Long = 0x3
        internal const CDERR_REGISTERMSGFAIL As Long = 0xC
        internal const CDERR_STRUCTSIZE As Long = 0x1

        internal const CDM_FIRST As Long = (WM_USER + 100)
        internal const CDM_GETSPEC As Long = (CDM_FIRST + 0x0)
        internal const CDM_SETCONTROLTEXT As Long = (CDM_FIRST + 0x4)
        internal const CDM_SETDEFEXT As Long = (CDM_FIRST + 0x6)
        internal const edt1 As Long = 0x480
        internal const VK_RETURN As Long = 0xD

        internal const VER_PLATFORM_WIN32_NT As Long = 2
        internal const VER_PLATFORM_WIN32_WINDOWS As Long = 1
        internal const VER_PLATFORM_WIN32s As Long = 0

        //directory browser types
        internal Type BrowseInfo
          hWndOwner      As Long
          pIDLRoot       As Long
          string pszDisplayName
          string lpszTitle     
          ulFlags        As Long
          lpfnCallback   As Long
          lParam         As Long
          iImage         As Long
        End Type

        //types used in building bitmaps
        internal Type BITMAPINFOHEADER  //40 bytes
          biSize As Long
          biWidth As Long
          biHeight As Long
          biPlanes As Integer
          biBitCount As Integer
          biCompression As Long
          biSizeImage As Long
          biXPelsPerMeter As Long
          biYPelsPerMeter As Long
          biClrUsed As Long
          biClrImportant As Long
        End Type

        internal Type RGBQUAD
          rgbBlue As Byte
          rgbGreen As Byte
          rgbRed As Byte
          rgbReserved As Byte
        End Type

        internal Type BitmapInfo
          bmiHeader As BITMAPINFOHEADER
          bmiColor(16) As RGBQUAD
        End Type

        internal Type POINTAPI
          X As Long
          Y As Long
        End Type

        internal Type vbRECT
          Left As Long
          Top As Long
          Right As Long
          Bottom As Long
        End Type

        Type CHARRANGE
          cpMin As Long
          cpMax As Long
        End Type

      //checking the WinAPI webpages, it looks like
      //hDC and hdcTarget are backwards in previous
      //versions (the version that's commented out)
      //but the print functions all worked?
      //  internal Type vbFORMATRANGE
      //    hDC As Long
      //    hdcTarget As Long
      //    rc As vbRECT
      //    rcPage As vbRECT
      //    chrg As CHARRANGE
      //  End Type


      //in v1.2.8+ I swapped them to what I think is
      //correct; print function still works the same
      //so I don't know what's going on---
        internal Type vbFORMATRANGE
          hdcTarget As Long
          hDC As Long
          rc As vbRECT
          rcPage As vbRECT
          chrg As CHARRANGE
        End Type

        internal Type NMHDR
          hwndFrom As Long
          idFrom As Long
          code As Long
          ptrOFN As Long
        End Type


        //api declarations
        internal Declare Function ShellExecute Lib "shell32.dll" Alias "ShellExecuteA" (ByVal hWnd As Long, string lpOperation, string lpFile, string lpParameters, string lpDirectory, ByVal nShowCmd As Long) As Long
        internal Declare Function SetWindowPos Lib "user32.dll" (ByVal hWnd As Long, ByVal hWndInsertAfter As Long, ByVal X As Long, ByVal Y As Long, ByVal cx As Long, ByVal cy As Long, ByVal wFlags As Long) As Long
        internal Declare Function GetCurrentDirectory Lib "kernel32" Alias "GetCurrentDirectoryA" (ByVal nBufferLength As Long, string lpBuffer) As Long
        internal Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
        internal Declare Function SendMessageAny Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, lParam As Any) As Long
        internal Declare Function SendMessagePtW Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, wParam As POINTAPI, ByVal lParam As Long) As Long
        internal Declare Function SendMessagePtL Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, lParam As POINTAPI) As Long
        internal Declare Function SendMessageRctL Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, lParam As RECT) As Long
        internal Declare Function SendMessageByString Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, string lParam) As Long

        internal Declare Function GetDeviceCaps Lib "gdi32" (ByVal hDC As Long, ByVal nIndex As Long) As Long
        internal Declare Function GetDC Lib "user32" (ByVal hWnd As Long) As Long
        internal Declare Function GetTickCount Lib "kernel32" () As Long
        internal Declare Function SetClipboardData Lib "user32" Alias "SetClipboardDataA" (ByVal wFormat As Long, ByVal hMem As Long) As Long
        internal Declare Function BitBlt Lib "gdi32" (ByVal hDestDC As Long, ByVal X As Long, ByVal Y As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal hSrcDC As Long, ByVal xSrc As Long, ByVal ySrc As Long, ByVal dwRop As Long) As Long
        internal Declare Function MaskBlt Lib "gdi32" (ByVal hdcDest As Long, ByVal nXDest As Long, ByVal nYDest As Long, ByVal nWidth, ByVal nHeight As Long, ByVal hdcSrc As Long, ByVal nXSrc As Long, ByVal nYSrc As Long, ByVal hbmMask As Long, ByVal xMask As Long, ByVal yMask As Long, ByVal dwRop As Long) As Long
        internal Declare Function TransparentBlt Lib "msimg32.dll" (ByVal hdcDest As Long, ByVal xoriginDest As Long, ByVal yoriginDest As Long, ByVal wDest As Long, ByVal hDest As Long, ByVal hdcSrc As Long, ByVal xoriginSrc As Long, ByVal yoriginSrc As Long, ByVal wSrc As Long, ByVal hSrc As Long, ByVal crTransparent As Long) As Long
        internal Declare Function AlphaBlend Lib "msimg32.dll" (ByVal hdcDest As Long, ByVal xoriginDest As Long, ByVal yoriginDest As Long, ByVal wDest As Long, ByVal hDest As Long, ByVal hdcSrc As Long, ByVal xoriginSrc As Long, ByVal yoriginSrc As Long, ByVal wSrc As Long, ByVal hSrc As Long, ByVal blendf As Long) As Long
        //the BLENDFUNCTION Type is comprised of 4 bytes; so essentially a Long value
        //only non-zero value is SourceConstantAlpha, so it's easy to pass
        //the Type as a long with any given SourceConstantAlpha value where
        //BF_Value = Clng(SCA * 0x10000)

      //  internal Type BLENDFUNCTION
      //    BlendOp As Byte
      //    BlendFlags As Byte
      //    SourceConstantAlpha As Byte
      //    AlphaFormat As Byte
      //  End Type
      //
      //  internal const AC_SRC_OVER = 0x0                //(0)
      //  // alpha format flags
      //  internal const AC_SRC_NO_PREMULT_ALPHA = 0x1    //(1)
      //  internal const AC_SRC_NO_ALPHA = 0x2            //(2)
      //  internal const AC_DST_NO_PREMULT_ALPHA = 0x10   //(16)
      //  internal const AC_DST_NO_ALPHA = 0x20           //(32)

        internal Declare Function ExtFloodFill Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long, ByVal crColor As Long, ByVal wFillType As Long) As Long
        internal Declare Function StretchBlt Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal hSrcDC As Long, ByVal xSrc As Long, ByVal ySrc As Long, ByVal nSrcWidth As Long, ByVal nSrcHeight As Long, ByVal dwRop As Long) As Long
        internal Declare Function SetStretchBltMode Lib "gdi32.dll" (ByVal hDC As Long, ByVal nStretchMode As Long) As Long
        internal Declare Function GetShortPathName Lib "kernel32" Alias "GetShortPathNameA" (string lpszLongPath, string lpszShortPath, ByVal cchBuffer As Long) As Long
        internal Declare Function SelectObject Lib "gdi32" (ByVal hDC As Long, ByVal hObject As Long) As Long
        internal Declare Function DeleteObject Lib "gdi32" (ByVal hObject As Long) As Long
        internal Declare Function SetFocusAPI Lib "user32.dll" Alias "SetFocus" (ByVal hWnd As Long) As Long

        internal Declare Function CreateSolidBrush Lib "gdi32" (ByVal crColor As Long) As Long
        internal Declare Function GetPixel Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long) As Long
        internal Declare Function SetPixelV Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long, ByVal crColor As Long) As Long
        internal Declare Function GetCursorPos Lib "user32" (lpPoint As POINTAPI) As Long
        internal Declare Function WindowFromPoint Lib "user32" (ByVal xPoint As Long, ByVal yPoint As Long) As Long
        internal Declare Function WindowFromDC Lib "user32" (ByVal hDC As Long) As Long

        internal Declare Function SetCapture Lib "user32" (ByVal hWnd As Long) As Long
        internal Declare Function GetCapture Lib "user32" () As Long
        internal Declare Function SetWindowLong Lib "user32" Alias "SetWindowLongA" (ByVal hWnd As Long, ByVal nIndex As Long, ByVal dwNewLong As Long) As Long
        internal Declare Function CallWindowProc Lib "user32" Alias "CallWindowProcA" (ByVal lpPrevWndFunc As Long, ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
        internal Declare Function CreateDIBSection Lib "gdi32" (ByVal hDC As Long, pBitmapInfo As BitmapInfo, ByVal un As Long, lplpVoid As Long, ByVal handle As Long, ByVal dw As Long) As Long
        internal Declare Function CreateCompatibleDC Lib "gdi32" (ByVal hDC As Long) As Long
        internal Declare Function DeleteDC Lib "gdi32" (ByVal hDC As Long) As Long
        internal Declare Function CopyMemory Lib "kernel32" Alias "RtlMoveMemory" (Destination As Any, Source As Any, ByVal Length As Long) As Long
        internal Declare Function CreateBitmap Lib "gdi32" (ByVal nWidth As Long, ByVal nHeight As Long, ByVal nPlanes As Long, ByVal nBitCount As Long, lpBits As Any) As Long
        internal Declare Function CreateCompatibleBitmap Lib "gdi32" (ByVal hDC As Long, ByVal nWidth As Long, ByVal nHeight As Long) As Long
        internal Declare Function BringWindowToTop Lib "user32" (ByVal hWnd As Long) As Long

      internal const RDW_INVALIDATE = 0x1
      internal const RDW_INTERNALPAINT = 0x2
      internal const RDW_ERASE = 0x4
      internal const RDW_VALIDATE = 0x8
      internal const RDW_NOINTERNALPAINT = 0x10
      internal const RDW_NOERASE = 0x20
      internal const RDW_NOCHILDREN = 0x40
      internal const RDW_ALLCHILDREN = 0x80
      internal const RDW_UPDATENOW = 0x100
      internal const RDW_ERASENOW = 0x200
      internal const RDW_FRAME = 0x400
      internal const RDW_NOFRAME = 0x800

        internal Declare Function SetParent Lib "user32" (ByVal hWndChild As Long, ByVal hWndNewParent As Long) As Long
        internal Declare Function ReleaseCapture Lib "user32" () As Long
        internal Declare Function LockWindowUpdate Lib "user32" (ByVal hWndLock As Long) As Long
        internal Declare Function GetBitmapDimensionEx Lib "gdi32" (ByVal hBitmap As Long, lpDimension As POINTAPI) As Long

        internal Declare Function StartDoc Lib "gdi32" Alias "StartDocA" (ByVal hDC As Long, doc As DOCINFO) As Long
        internal Declare Function EndDoc Lib "gdi32" (ByVal hDC As Long) As Long
        internal Declare Function StartPage Lib "gdi32" (ByVal hDC As Long) As Long
        internal Declare Function EndPage Lib "gdi32" (ByVal hDC As Long) As Long

        internal Declare Function SaveDC Lib "gdi32" (ByVal hDC As Long) As Long
        internal Declare Function RestoreDC Lib "gdi32" (ByVal hDC As Long, ByVal SavedDC As Long) As Long

        internal Declare Function GetSaveFileName Lib "comdlg32.dll" Alias "GetSaveFileNameA" (pOpenfilename As OPENFILENAME) As Long
        internal Declare Function CommDlgExtendedError Lib "comdlg32.dll" () As Long

        internal Declare Function GetParent Lib "user32.dll" (ByVal hWnd As Long) As Long
        internal Declare Function GetLastError Lib "kernel32.dll" () As Long

        internal Declare Function GetVersionEx Lib "kernel32.dll" Alias "GetVersionExA" (lpVersionInformation As OSVERSIONINFO) As Long
        internal Declare Function GetDesktopWindow Lib "user32.dll" () As Long

        //monitor functions
        internal Declare Function GetMonitorInfo Lib "user32.dll" Alias "GetMonitorInfoA" (ByVal hMonitor As Long, lpmi As MONITORINFO) As Long
        internal Declare Function MonitorFromWindow Lib "user32.dll" (ByVal hWnd As Long, ByVal dwFlags As Long) As Long
        internal Declare Function GetWindowPlacement Lib "user32.dll" (ByVal hWnd As Long, lpwndpl As WINDOWPLACEMENT) As Long
        internal Declare Function SetWindowPlacement Lib "user32.dll" (ByVal hWnd As Long, lpwndpl As WINDOWPLACEMENT) As Long
        internal wpWinAGI As WINDOWPLACEMENT

        internal const MONITOR_DEFAULTTONULL = 0
        internal const MONITOR_DEFAULTTOPRIMARY = 1
        internal const MONITOR_DEFAULTTONEAREST = 2
        internal const MONITORINFOF_PRIMARY = 1

        internal const WPF_ASYNCWINDOWPLACEMENT = 4
        internal const WPF_RESTORETOMAXIMIZED = 2
        internal const WPF_SETMINPOSITION = 1
        internal const SW_HIDE = 0
        internal const SW_MAXIMIZE = 3
        internal const SW_MINIMIZE = 6
        internal const SW_RESTORE = 9
        internal const SW_SHOW = 5
        internal const SW_SHOWMAXIMIZED = 3
        internal const SW_SHOWMINIMIZED = 2
        internal const SW_SHOWMINNOACTIVE = 7
        internal const SW_SHOWNA = 8
        internal const SW_SHOWNOACTIVATE = 4
        internal const SW_SHOWNORMAL = 1


      // Display string resource ID or text in a popupwin.
        internal const HH_DISPLAY_TEXT_POPUP = 0xE
      // Display mapped numeric Value in dwdata
        internal const HH_HELP_CONTEXT = 0xF
      // Text pop-up help, similar to WinHelp's HELP_CONTEXTMENU
        internal const HH_TP_HELP_CONTEXTMENU = 0x10
      // Text pop-up help, similar to WinHelp's HELP_WM_HELP
        internal const HH_TP_HELP_WM_HELP = 0x11

      // From www.mvps.org/vbnet...i think
      //   Dereferences an ANSI or Unicode string pointer
      //   and returns a normal VB BSTR
      internal string PtrToStrW(ByVal lpsz As Long)
          string sOut
          Dim lLen As Long

          lLen = lstrlenW(lpsz)

          if ((lLen > 0)) {
              sOut = StrConv(String$(lLen, vbNullChar), vbUnicode)
              Call CopyMemory(ByVal sOut, ByVal lpsz, lLen * 2)
              PtrToStrW = StrConv(sOut, vbFromUnicode)
          }
      End Function

      internal Function MetsToPix(ByVal Mets As Long, Optional ByVal Vert As Boolean = False) As Long

        if (Vert) {
          MetsToPix = Mets * 1440 / ScreenTWIPSY / 2540
        } else {
          MetsToPix = Mets * 1440 / ScreenTWIPSX / 2540
        }
      End Function

      internal Function PixToMets(ByVal Pixels As Long, Optional ByVal Vert As Boolean = False) As Long

        if (Vert) {
          PixToMets = Pixels * ScreenTWIPSY * 2540 / 1440
        } else {
          PixToMets = Pixels * ScreenTWIPSX * 2540 / 1440
        }
      End Function


            */
        }

    }
    public static partial class Base {
        public const char QUOTECHAR = '\"';
        public static readonly string CTRL_CHARS; // contains all chars <32; used for comparisons
        public static readonly string NEWLINE = Environment.NewLine;

        public const double LOG10_1_12 = 2.50858329719984E-02; // = Log10(2 ^ (1/12))
        public const string ARG1 = "%1";
        public const string ARG2 = "%2";
        public const string ARG3 = "%3";
        public const string sAPPNAME = "WinAGI Game Development System 3.0 alpha";
        public const string COPYRIGHT_YEAR = "2024";
        public static uint[] CRC32Table = new uint[256];
        public static bool CRC32Loaded;
        public static readonly string EXT_CHARS; // all extended chars; used for comparisons
        public static readonly string INVALID_ID_CHARS;
        public static readonly string INVALID_DEFNAME_CHARS;
        public static readonly string INVALID_FILENAME_CHARS;
        public static readonly string TOKEN_CHARS;

        static Base() {
            // create invalid control char string
            CTRL_CHARS = "";
            for (int i = 1; i < 32; i++) {
                CTRL_CHARS += ((char)i).ToString();
            }
            // create extended char string
            EXT_CHARS = "";
            for (int i = 127; i < 256; i++) {
                EXT_CHARS += ((char)i).ToString();
            }

            // invalid ID characters: these, plus control chars and extended chars
            //        3       4    4    5         6         7         8         9         0         1         2
            //        234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
            //NOT OK  x!"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~x
            //    OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    
            INVALID_ID_CHARS = CTRL_CHARS + " !\"&'()*+,-/:;<=>?[\\]^`{|}~" + EXT_CHARS;

            // invalid Define Name characters: these, plus control chars and extended chars
            //        3       4    4    5         6         7         8         9         0         1         2
            //        234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
            //NOT OK  x!"#$%&'()*+,-           :;<=> @                           [\]^ `                          {|}~x
            //    OK                ./0123456789    ?  ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    
            // sierra syntax allows ' ?
            // sierra syntax allows / for anything but first char
            INVALID_DEFNAME_CHARS = CTRL_CHARS + " !\"#$%&'()*+,-:;<=>?@[\\]^`{|}~" + ((char)127).ToString() + EXT_CHARS;
            INVALID_FILENAME_CHARS = CTRL_CHARS + " \\/:*?\"<>|";
        }

        public class WinAGIException : Exception {
            public WinAGIException() {
            }

            public WinAGIException(string message)
                : base(message) {
            }

            public WinAGIException(string message, Exception inner)
                : base(message, inner) {
            }

            /// <summary>Throws a WinAGIException if resource is not loaded.</summary>
            public static void ThrowIfNotLoaded(AGIResource value) {
                if (!value.Loaded)
                    ThrowResourceNotLoaded();
            }
            [DoesNotReturn]
            private static void ThrowResourceNotLoaded() {
                WinAGIException wex = new("Resource not loaded") {
                    HResult = 563,
                };
                throw wex;
            }
        }

        public static string LoadResString(int index) {
            // this function is just a handy way to get resource strings by number
            // instead of by stringkey
            try {
                return EngineResources.ResourceManager.GetString(index.ToString());
            }
            catch (Exception) {
                // return nothing if string doesn't exist
                return "";
            }
        }
        public static string UnicodeToCP(string strIn, Encoding enc) {
            return enc.GetString(Encoding.Unicode.GetBytes(strIn));
        }

        public static string CPToUnicode(string strIn, Encoding oldCP) {
            return Encoding.Unicode.GetString(oldCP.GetBytes(strIn));
        }

        public static Array ResizeArray(Array arr, int[] newSizes) {
            if (newSizes.Length != arr.Rank)
                throw new ArgumentException("arr must have the same number of dimensions " +
                                            "as there are elements in newSizes", nameof(newSizes));
            var temp = Array.CreateInstance(arr.GetType().GetElementType(), newSizes);
            int length = arr.Length <= temp.Length ? arr.Length : temp.Length;
            Array.ConstrainedCopy(arr, 0, temp, 0, length);
            return temp;
        }

        public static string Right(string strIn, int length) {
            if (length >= strIn.Length)
                return strIn;
            else
                return strIn[^length..];
        }

        public static string Left(string strIn, int length) {
            if (length >= strIn.Length)
                return strIn;
            else
                return strIn[..length];
        }

        public static string Mid(string strIn, int pos, int length) {
            // mimic VB mid function; if length is too long, return
            // max amount
            if (pos + length > strIn.Length)
                return strIn[pos..];
            return strIn.Substring(pos, length);
        }

        public static string MultStr(string strIn, int NumCopies) {
            return new StringBuilder(strIn.Length * NumCopies).Insert(0, strIn, NumCopies).ToString();
            //string retval = "";
            //for (int i = 1; i <= NumCopies; i++)
            //  retval += strIn;
            //return retval;
        }

        /// <summary>
        /// Extension method that works out if a string is numeric or not
        /// </summary>
        /// <param name="str">string that may be a number</param>
        /// <returns>true if numeric, false if not</returns>
        public static bool IsNumeric(string str) { // TODO: do I need another test specifically for int values?
            if (Double.TryParse(str, out _)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Extension that mimics the VB Val() function; returns 0
        /// if the string is non-numeric
        /// </summary>
        /// <param name="strIn">The string that will be converted to a number</param>
        /// <returns>Returns a double value of strIn; if strIn can't be converted
        /// to a double, it returns 0</returns>
        public static double Val(string strIn) {
            if (double.TryParse(strIn, out double dResult)) {
                //return this value
                return dResult;
            }
            // not a valid number; return 0
            return 0;
        }

        /// <summary>
        /// Confirms that a directory has a terminating backslash,
        /// adding one if necessary
        /// </summary>
        /// <param name="strDirIn"></param>
        /// <returns></returns>
        public static string CDir(string strDirIn) {
            //this function ensures a trailing "\" is included on strDirIn
            if (strDirIn.Length != 0) {
                return !strDirIn.EndsWith('\\') ? strDirIn + '\\' : strDirIn;
            }
            else {
                return strDirIn;
            }
        }

        public static string JustPath(string strFullPathName, bool NoSlash = false) {  //will extract just the path name by removing the filename
                                                                                       //if optional NoSlash is true, the trailing backslash will be dropped

            //return Path.GetFullPath(strFullPathName);

            // if nothing
            if (strFullPathName.Length == 0) {
                return "";
            }

            //split into directories and filename
            string[] strSplitName = strFullPathName.Split("\\");
            //if no splits,
            if (strSplitName.Length == 1) {
                //return empty- no path information in this string
                return "";
            }
            else {
                //eliminate last entry (which is filename)
                Array.Resize(ref strSplitName, strSplitName.Length - 1);
                //rebuild name
                string sReturn = String.Join("\\", strSplitName);
                if (!NoSlash) {
                    sReturn += "\\";
                }
                return sReturn;
            }
        }

        public static uint CRC32(byte[] DataIn) {
            //calculates the CRC32 for an input array of bytes
            //a special table is necessary; the table is loaded
            //at program startup

            //the CRC is calculated according to the following equation:
            //
            //  CRC[i] = LSHR8(CRC[i-1]) Xor CRC32Table[(CRC[i-1] && 0xFF) Xor DataIn[i])
            //
            //initial Value of CRC is 0xFFFFFFFF; iterate the equation
            //for each byte of data; then end by XORing final result with 0xFFFFFFFF

            int i;
            //initial Value
            uint result = 0xffffffff;

            //if table not loaded
            if (!CRC32Loaded)
                CRC32Setup();

            //iterate CRC equation
            for (i = 0; i < DataIn.Length; i++)
                result = (result >> 8) ^ CRC32Table[(result & 0xFF) ^ DataIn[i]];

            //xor to create final answer
            return result ^ 0xFFFFFFFF;
        }

        public static void CRC32Setup() {
            //build the CRC table
            uint z;
            uint index;
            for (index = 0; index < 256; index++) {
                CRC32Table[index] = index;
                for (z = 8; z != 0; z--) {
                    if ((CRC32Table[index] & 1) == 1) {
                        CRC32Table[index] = (CRC32Table[index] >> 1) ^ 0xEDB88320;
                    }
                    else {
                        CRC32Table[index] = CRC32Table[index] >> 1;
                    }
                }
            }
            CRC32Loaded = true;

            // keep the real values until I'm sure the calculated table is
            // 100% correct

            //CRC32Table[0] = 0x0;
            //CRC32Table[1] = 0x77073096;
            //CRC32Table[2] = 0xEE0E612C;
            //CRC32Table[3] = 0x990951BA;
            //CRC32Table[4] = 0x76DC419;
            //CRC32Table[5] = 0x706AF48F;
            //CRC32Table[6] = 0xE963A535;
            //CRC32Table[7] = 0x9E6495A3;
            //CRC32Table[8] = 0xEDB8832;
            //CRC32Table[9] = 0x79DCB8A4;
            //CRC32Table[10] = 0xE0D5E91E;
            //CRC32Table[11] = 0x97D2D988;
            //CRC32Table[12] = 0x9B64C2B;
            //CRC32Table[13] = 0x7EB17CBD;
            //CRC32Table[14] = 0xE7B82D07;
            //CRC32Table[15] = 0x90BF1D91;
            //CRC32Table[16] = 0x1DB71064;
            //CRC32Table[17] = 0x6AB020F2;
            //CRC32Table[18] = 0xF3B97148;
            //CRC32Table[19] = 0x84BE41DE;
            //CRC32Table[20] = 0x1ADAD47D;
            //CRC32Table[21] = 0x6DDDE4EB;
            //CRC32Table[22] = 0xF4D4B551;
            //CRC32Table[23] = 0x83D385C7;
            //CRC32Table[24] = 0x136C9856;
            //CRC32Table[25] = 0x646BA8C0;
            //CRC32Table[26] = 0xFD62F97A;
            //CRC32Table[27] = 0x8A65C9EC;
            //CRC32Table[28] = 0x14015C4F;
            //CRC32Table[29] = 0x63066CD9;
            //CRC32Table[30] = 0xFA0F3D63;
            //CRC32Table[31] = 0x8D080DF5;
            //CRC32Table[32] = 0x3B6E20C8;
            //CRC32Table[33] = 0x4C69105E;
            //CRC32Table[34] = 0xD56041E4;
            //CRC32Table[35] = 0xA2677172;
            //CRC32Table[36] = 0x3C03E4D1;
            //CRC32Table[37] = 0x4B04D447;
            //CRC32Table[38] = 0xD20D85FD;
            //CRC32Table[39] = 0xA50AB56B;
            //CRC32Table[40] = 0x35B5A8FA;
            //CRC32Table[41] = 0x42B2986C;
            //CRC32Table[42] = 0xDBBBC9D6;
            //CRC32Table[43] = 0xACBCF940;
            //CRC32Table[44] = 0x32D86CE3;
            //CRC32Table[45] = 0x45DF5C75;
            //CRC32Table[46] = 0xDCD60DCF;
            //CRC32Table[47] = 0xABD13D59;
            //CRC32Table[48] = 0x26D930AC;
            //CRC32Table[49] = 0x51DE003A;
            //CRC32Table[50] = 0xC8D75180;
            //CRC32Table[51] = 0xBFD06116;
            //CRC32Table[52] = 0x21B4F4B5;
            //CRC32Table[53] = 0x56B3C423;
            //CRC32Table[54] = 0xCFBA9599;
            //CRC32Table[55] = 0xB8BDA50F;
            //CRC32Table[56] = 0x2802B89E;
            //CRC32Table[57] = 0x5F058808;
            //CRC32Table[58] = 0xC60CD9B2;
            //CRC32Table[59] = 0xB10BE924;
            //CRC32Table[60] = 0x2F6F7C87;
            //CRC32Table[61] = 0x58684C11;
            //CRC32Table[62] = 0xC1611DAB;
            //CRC32Table[63] = 0xB6662D3D;
            //CRC32Table[64] = 0x76DC4190;
            //CRC32Table[65] = 0x1DB7106;
            //CRC32Table[66] = 0x98D220BC;
            //CRC32Table[67] = 0xEFD5102A;
            //CRC32Table[68] = 0x71B18589;
            //CRC32Table[69] = 0x6B6B51F;
            //CRC32Table[70] = 0x9FBFE4A5;
            //CRC32Table[71] = 0xE8B8D433;
            //CRC32Table[72] = 0x7807C9A2;
            //CRC32Table[73] = 0xF00F934;
            //CRC32Table[74] = 0x9609A88E;
            //CRC32Table[75] = 0xE10E9818;
            //CRC32Table[76] = 0x7F6A0DBB;
            //CRC32Table[77] = 0x86D3D2D;
            //CRC32Table[78] = 0x91646C97;
            //CRC32Table[79] = 0xE6635C01;
            //CRC32Table[80] = 0x6B6B51F4;
            //CRC32Table[81] = 0x1C6C6162;
            //CRC32Table[82] = 0x856530D8;
            //CRC32Table[83] = 0xF262004E;
            //CRC32Table[84] = 0x6C0695ED;
            //CRC32Table[85] = 0x1B01A57B;
            //CRC32Table[86] = 0x8208F4C1;
            //CRC32Table[87] = 0xF50FC457;
            //CRC32Table[88] = 0x65B0D9C6;
            //CRC32Table[89] = 0x12B7E950;
            //CRC32Table[90] = 0x8BBEB8EA;
            //CRC32Table[91] = 0xFCB9887C;
            //CRC32Table[92] = 0x62DD1DDF;
            //CRC32Table[93] = 0x15DA2D49;
            //CRC32Table[94] = 0x8CD37CF3;
            //CRC32Table[95] = 0xFBD44C65;
            //CRC32Table[96] = 0x4DB26158;
            //CRC32Table[97] = 0x3AB551CE;
            //CRC32Table[98] = 0xA3BC0074;
            //CRC32Table[99] = 0xD4BB30E2;
            //CRC32Table[100] = 0x4ADFA541;
            //CRC32Table[101] = 0x3DD895D7;
            //CRC32Table[102] = 0xA4D1C46D;
            //CRC32Table[103] = 0xD3D6F4FB;
            //CRC32Table[104] = 0x4369E96A;
            //CRC32Table[105] = 0x346ED9FC;
            //CRC32Table[106] = 0xAD678846;
            //CRC32Table[107] = 0xDA60B8D0;
            //CRC32Table[108] = 0x44042D73;
            //CRC32Table[109] = 0x33031DE5;
            //CRC32Table[110] = 0xAA0A4C5F;
            //CRC32Table[111] = 0xDD0D7CC9;
            //CRC32Table[112] = 0x5005713C;
            //CRC32Table[113] = 0x270241AA;
            //CRC32Table[114] = 0xBE0B1010;
            //CRC32Table[115] = 0xC90C2086;
            //CRC32Table[116] = 0x5768B525;
            //CRC32Table[117] = 0x206F85B3;
            //CRC32Table[118] = 0xB966D409;
            //CRC32Table[119] = 0xCE61E49F;
            //CRC32Table[120] = 0x5EDEF90E;
            //CRC32Table[121] = 0x29D9C998;
            //CRC32Table[122] = 0xB0D09822;
            //CRC32Table[123] = 0xC7D7A8B4;
            //CRC32Table[124] = 0x59B33D17;
            //CRC32Table[125] = 0x2EB40D81;
            //CRC32Table[126] = 0xB7BD5C3B;
            //CRC32Table[127] = 0xC0BA6CAD;
            //CRC32Table[128] = 0xEDB88320;
            //CRC32Table[129] = 0x9ABFB3B6;
            //CRC32Table[130] = 0x3B6E20C;
            //CRC32Table[131] = 0x74B1D29A;
            //CRC32Table[132] = 0xEAD54739;
            //CRC32Table[133] = 0x9DD277AF;
            //CRC32Table[134] = 0x4DB2615;
            //CRC32Table[135] = 0x73DC1683;
            //CRC32Table[136] = 0xE3630B12;
            //CRC32Table[137] = 0x94643B84;
            //CRC32Table[138] = 0xD6D6A3E;
            //CRC32Table[139] = 0x7A6A5AA8;
            //CRC32Table[140] = 0xE40ECF0B;
            //CRC32Table[141] = 0x9309FF9D;
            //CRC32Table[142] = 0xA00AE27;
            //CRC32Table[143] = 0x7D079EB1;
            //CRC32Table[144] = 0xF00F9344;
            //CRC32Table[145] = 0x8708A3D2;
            //CRC32Table[146] = 0x1E01F268;
            //CRC32Table[147] = 0x6906C2FE;
            //CRC32Table[148] = 0xF762575D;
            //CRC32Table[149] = 0x806567CB;
            //CRC32Table[150] = 0x196C3671;
            //CRC32Table[151] = 0x6E6B06E7;
            //CRC32Table[152] = 0xFED41B76;
            //CRC32Table[153] = 0x89D32BE0;
            //CRC32Table[154] = 0x10DA7A5A;
            //CRC32Table[155] = 0x67DD4ACC;
            //CRC32Table[156] = 0xF9B9DF6F;
            //CRC32Table[157] = 0x8EBEEFF9;
            //CRC32Table[158] = 0x17B7BE43;
            //CRC32Table[159] = 0x60B08ED5;
            //CRC32Table[160] = 0xD6D6A3E8;
            //CRC32Table[161] = 0xA1D1937E;
            //CRC32Table[162] = 0x38D8C2C4;
            //CRC32Table[163] = 0x4FDFF252;
            //CRC32Table[164] = 0xD1BB67F1;
            //CRC32Table[165] = 0xA6BC5767;
            //CRC32Table[166] = 0x3FB506DD;
            //CRC32Table[167] = 0x48B2364B;
            //CRC32Table[168] = 0xD80D2BDA;
            //CRC32Table[169] = 0xAF0A1B4C;
            //CRC32Table[170] = 0x36034AF6;
            //CRC32Table[171] = 0x41047A60;
            //CRC32Table[172] = 0xDF60EFC3;
            //CRC32Table[173] = 0xA867DF55;
            //CRC32Table[174] = 0x316E8EEF;
            //CRC32Table[175] = 0x4669BE79;
            //CRC32Table[176] = 0xCB61B38C;
            //CRC32Table[177] = 0xBC66831A;
            //CRC32Table[178] = 0x256FD2A0;
            //CRC32Table[179] = 0x5268E236;
            //CRC32Table[180] = 0xCC0C7795;
            //CRC32Table[181] = 0xBB0B4703;
            //CRC32Table[182] = 0x220216B9;
            //CRC32Table[183] = 0x5505262F;
            //CRC32Table[184] = 0xC5BA3BBE;
            //CRC32Table[185] = 0xB2BD0B28;
            //CRC32Table[186] = 0x2BB45A92;
            //CRC32Table[187] = 0x5CB36A04;
            //CRC32Table[188] = 0xC2D7FFA7;
            //CRC32Table[189] = 0xB5D0CF31;
            //CRC32Table[190] = 0x2CD99E8B;
            //CRC32Table[191] = 0x5BDEAE1D;
            //CRC32Table[192] = 0x9B64C2B0;
            //CRC32Table[193] = 0xEC63F226;
            //CRC32Table[194] = 0x756AA39C;
            //CRC32Table[195] = 0x26D930A;
            //CRC32Table[196] = 0x9C0906A9;
            //CRC32Table[197] = 0xEB0E363F;
            //CRC32Table[198] = 0x72076785;
            //CRC32Table[199] = 0x5005713;
            //CRC32Table[200] = 0x95BF4A82;
            //CRC32Table[201] = 0xE2B87A14;
            //CRC32Table[202] = 0x7BB12BAE;
            //CRC32Table[203] = 0xCB61B38;
            //CRC32Table[204] = 0x92D28E9B;
            //CRC32Table[205] = 0xE5D5BE0D;
            //CRC32Table[206] = 0x7CDCEFB7;
            //CRC32Table[207] = 0xBDBDF21;
            //CRC32Table[208] = 0x86D3D2D4;
            //CRC32Table[209] = 0xF1D4E242;
            //CRC32Table[210] = 0x68DDB3F8;
            //CRC32Table[211] = 0x1FDA836E;
            //CRC32Table[212] = 0x81BE16CD;
            //CRC32Table[213] = 0xF6B9265B;
            //CRC32Table[214] = 0x6FB077E1;
            //CRC32Table[215] = 0x18B74777;
            //CRC32Table[216] = 0x88085AE6;
            //CRC32Table[217] = 0xFF0F6A70;
            //CRC32Table[218] = 0x66063BCA;
            //CRC32Table[219] = 0x11010B5C;
            //CRC32Table[220] = 0x8F659EFF;
            //CRC32Table[221] = 0xF862AE69;
            //CRC32Table[222] = 0x616BFFD3;
            //CRC32Table[223] = 0x166CCF45;
            //CRC32Table[224] = 0xA00AE278;
            //CRC32Table[225] = 0xD70DD2EE;
            //CRC32Table[226] = 0x4E048354;
            //CRC32Table[227] = 0x3903B3C2;
            //CRC32Table[228] = 0xA7672661;
            //CRC32Table[229] = 0xD06016F7;
            //CRC32Table[230] = 0x4969474D;
            //CRC32Table[231] = 0x3E6E77DB;
            //CRC32Table[232] = 0xAED16A4A;
            //CRC32Table[233] = 0xD9D65ADC;
            //CRC32Table[234] = 0x40DF0B66;
            //CRC32Table[235] = 0x37D83BF0;
            //CRC32Table[236] = 0xA9BCAE53;
            //CRC32Table[237] = 0xDEBB9EC5;
            //CRC32Table[238] = 0x47B2CF7F;
            //CRC32Table[239] = 0x30B5FFE9;
            //CRC32Table[240] = 0xBDBDF21C;
            //CRC32Table[241] = 0xCABAC28A;
            //CRC32Table[242] = 0x53B39330;
            //CRC32Table[243] = 0x24B4A3A6;
            //CRC32Table[244] = 0xBAD03605;
            //CRC32Table[245] = 0xCDD70693;
            //CRC32Table[246] = 0x54DE5729;
            //CRC32Table[247] = 0x23D967BF;
            //CRC32Table[248] = 0xB3667A2E;
            //CRC32Table[249] = 0xC4614AB8;
            //CRC32Table[250] = 0x5D681B02;
            //CRC32Table[251] = 0x2A6F2B94;
            //CRC32Table[252] = 0xB40BBE37;
            //CRC32Table[253] = 0xC30C8EA1;
            //CRC32Table[254] = 0x5A05DF1B;
            //CRC32Table[255] = 0x2D02EF8D;

        }

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new(sourceDirName);
            if (!dir.Exists) {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }
            DirectoryInfo[] dirs = dir.GetDirectories();
            try {
                // if (the destination directory doesn't exist, create it.       
                Directory.CreateDirectory(destDirName);
                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files) {
                    string tempPath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(tempPath, false);
                }
                // if (copying subdirectories, copy them and their contents to new location.
                if (copySubDirs) {
                    foreach (DirectoryInfo subdir in dirs) {
                        string tempPath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                    }
                }
            }
            catch (Exception) {
                throw new Exception("directory copy error");
            }
        }
        /// <summary>
        /// this methodcompacts a full filename by eliminating directories and replacing
        /// them with ellipse(...)
        /// </summary>
        /// <param name="LongPath"></param>
        /// <param name="MaxLength"></param>
        /// <returns></returns>
        public static string CompactPath(string LongPath, int MaxLength = 40) {
            string strDir, strFile;

            //if already fits,
            if (LongPath.Length <= MaxLength) {
                //return entire path
                return LongPath;
            }
            // if no subdirectories
            if (!LongPath.Contains('\\')) {
                //return truncated path
                return Left(LongPath, MaxLength - 3) + "...";
            }
            // position of last backslash
            int lngPos = LongPath.LastIndexOf('\\');
            // split into two strings
            strDir = Left(LongPath, lngPos);
            strFile = Right(LongPath, LongPath.Length - lngPos - 1);
            // if file name is too long
            if (strFile.Length > MaxLength - 4) {
                // return truncated filename
                return Left(strFile, MaxLength - 3) + "...";
            }
            // truncate directory, pad with ... and return combined dir/filename
            return Left(strDir, MaxLength - 4) + "...\\" + strFile;
        }
        /// <summary>
        /// gets the short filename of a file to make it compatible with MSDOS programs
        /// </summary>
        /// <param name="strLongFileName"></param>
        /// <returns>short path</returns>
        public static string ShortFileName(string strLongFileName) {
            int rtn;
            int lngStrLen;
            StringBuilder strTemp = new(0);
            try {
                // get size of required buffer
                lngStrLen = API.GetShortPathName(strLongFileName, strTemp, 0);
                strTemp = new StringBuilder(lngStrLen);
                // now get path
                rtn = API.GetShortPathName(strLongFileName, strTemp, lngStrLen);
                // if error
                if (lngStrLen == 0) {
                    // ignore error
                    return "";
                }
                return strTemp.ToString();
            }
            catch (Exception) {
                // ignore errors
                return "";
            }
        }

        internal static List<string> SplitLines(string strText) {
            // splits the input text into lines, by CR, LF, or CRLF
            // strategy is to replace CRLFs with CRs, then LFs with CRs,
            // and then slpit by CRs
            List<string> retval = [.. strText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')];
            return retval;
        }

        public static string ChangeExtension(ref string FileName, string Filter, int Index) {
            //compares the extension on Filename to the extension belonging to
            //first extension for filter that numbered as Index
            //
            //if the filter extension is unique (no // or '?') the function returns
            //the extension (without '*.' leader)
            //
            //if they don't match, filename is modified to use the correct extension,
            //
            //if the filter is not unique, function returns empty string)
            //
            //filename is assumed to be valid;
            //filter is assumed to be pairs of description/file filters separated by
            //null character (ChrW$(0)) with an extra null character at the end
            //
            //Index is assumed valid; it is not checked for error here
            string strFileExt, strFilterExt;
            string[] strExt;
            int lngPos;
            //get extension of desired filter
            strExt = Filter.Split((char)0);
            if (Index >= strExt.Length) {
                // invalid index
                return "";
            }
            strFilterExt = strExt[Index * 2 + 1];
            //filter should be in form //*.xxx//
            if (Left(strFilterExt, 2) != "*.") {
                //no extension change required
                return "";
            }
            //strip off the first two characters
            strFilterExt = Right(strFilterExt, strFilterExt.Length - 2);

            //no asterisks or question marks or periods
            if ("*?.".Any(strFilterExt.Contains)) {
                //invalid filter; no extension change required
                return "";
            }

            //only 1 2 or three characters
            if (strFilterExt.Length > 3 || strFilterExt.Length < 1) {
                //invalid filter; no extension change required
                return "";
            }

            //strFilterExt is the extension to pass back
            string retval = strFilterExt;

            //if no filename yet
            if (FileName.Length == 0) {
                //no extension change required
                return "";
            }

            //now get extension of filename
            lngPos = FileName.LastIndexOf('.');

            //if not found,
            if (lngPos == -1) {
                //no extension; add filter extension and return true
                FileName += "." + strFilterExt;
                return retval;
            }

            //get currentextension
            strFileExt = Right(FileName, FileName.Length - lngPos);

            //compare
            if (strFileExt.Equals(strFilterExt, StringComparison.OrdinalIgnoreCase)) {
                //extension has changed; change filename to match
                FileName = Left(FileName, lngPos - 1) + "." + strFilterExt;
            }
            return retval;
        }

        public static string FullFileName(string startdir, string relpath) {
            // TODO: add error checking
            try {
                return Path.GetFullPath(Path.Combine(startdir, relpath));
            }
            catch (Exception) {
                // for now, do nothing
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
        public static List<TWinAGIEventInfo> ExtractTODO(byte LogicNum, string SourceText, string Module) {

            // update the warning list with TODO items from this logic
            // valid TODO entries must be the string 'TODO:' within a comment, and must be
            // first thing after the comment marker, except for spaces
            // everything on the line following will be considered the text
            // marker is not case-sensitive
            //
            // example:    [ TODO: text
            // OK:         [ todo: text
            // not OK:     [ something TODO: text
            // not OK:     [ TODO text

            int Tpos, lngLine, Cpos;
            string strTODO;
            List<string> stlText;
            List<TWinAGIEventInfo> retval = [];

            // if TODO marker isn't in the logic, just exit
            if (!SourceText.Contains("TODO:"))
                return retval;

            //split into lines
            stlText = SplitLines(SourceText);

            //step through all lines
            for (lngLine = 0; lngLine < stlText.Count; lngLine++) {
                Tpos = stlText[lngLine].IndexOf("TODO:");
                if (Tpos >= 0) {
                    Cpos = stlText[lngLine].LastIndexOf('[', Tpos);
                    if (Cpos >= 0) {
                        //get text between the comment and the TODO
                        strTODO = Mid(stlText[lngLine], Cpos + 1, Tpos - Cpos - 1);
                        //only valid if empty spaces
                        if (strTODO.Trim().Length == 0) {
                            //get comment portion of text
                            strTODO = stlText[lngLine][(Tpos + 5)..].Trim();
                            if (strTODO.Length > 0) {
                                //add this TODO (adjust line by 1)
                                TWinAGIEventInfo tmpInfo = new() {
                                    ID = "TODO",
                                    //InfoType = EInfoType.itInitialize,
                                    Line = (lngLine + 1).ToString(),
                                    Module = Module,
                                    ResNum = LogicNum,
                                    ResType = AGIResType.rtLogic,
                                    Text = strTODO,
                                    Type = EventType.etTODO
                                };
                                // add this one to the list
                                retval.Add(tmpInfo);
                            }
                        }
                    }
                }
            }
            return retval;
        }
        public static List<TWinAGIEventInfo> ExtractDecompWarn(byte LogicNum, string SourceText, string Module) {
            // extracts decompiler warnings from logics
            //
            // look for WARNING DC## comments
            int Tpos, Cpos, lngLine;
            string strDCWarn;
            List<string> stlText;
            List<TWinAGIEventInfo> retval = [];

            // if warning marker isn't in the logic, just exit
            if (!SourceText.Contains("WARNING DC", StringComparison.OrdinalIgnoreCase))
                return retval;

            //split into lines
            stlText = SplitLines(SourceText);
            //step through all lines
            for (lngLine = 0; lngLine < stlText.Count; lngLine++) {
                Tpos = stlText[lngLine].IndexOf("WARNING DC", StringComparison.OrdinalIgnoreCase);
                if (Tpos >= 0) {
                    Cpos = stlText[lngLine].LastIndexOf('[', Tpos);
                    if (Cpos >= 0) {
                        //get warning portion of text
                        strDCWarn = stlText[lngLine][(Cpos + 9)..].Trim();
                        if (strDCWarn.Length > 0) {
                            //add this TODO (adjust line by 1)
                            TWinAGIEventInfo tmpInfo = new() {
                                ID = strDCWarn[..4],
                                //InfoType = EInfoType.itInitialize,
                                Line = (lngLine + 1).ToString(),
                                Module = Module,
                                ResNum = LogicNum,
                                ResType = AGIResType.rtLogic,
                                Text = strDCWarn[6..],
                                Type = EventType.etWarning
                            };
                            // add to the list
                            retval.Add(tmpInfo);
                        }
                    }
                }
            }
            return retval;
        }
    }
}
