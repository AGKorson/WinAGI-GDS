using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  //types used in building bitmaps
  internal struct BITMAPINFOHEADER  //40 bytes
  {
    internal int biSize;
    internal int biWidth;
    internal int biHeight;
    internal short biPlanes;
    internal short biBitCount;
    internal int biCompression;
    internal int biSizeImage;
    internal int biXPelsPerMeter;
    internal int biYPelsPerMeter;
    internal int biClrUsed;
    internal int biClrImportant;
  }
  internal struct RGBQUAD
  {
    internal byte rgbBlue;
    internal byte rgbGreen;
    internal byte rgbRed;
    internal byte rgbReserved;
  }
  internal struct BitmapInfo
  {
    internal BITMAPINFOHEADER bmiHeader;
    internal RGBQUAD[] bmiColor;// = new RGBQUAD[16];
  }
  //types used for time functions
  internal enum GET_FILEEX_INFO_LEVELS
  {
    GetFileExInfoStandard,
    GetFileExMaxInfoLevel,
  }
  internal struct SystemTime
  {
    internal short Year;
    internal short Month;
    internal short WeekDay;
    internal short Day;
    internal short Hour;
    internal short Minute;
    internal short Second;
    internal short MSec;
  }
  internal struct FileTime
  {
    internal int LoTime;
    internal int HiTime;
  }
  internal struct W32FileAttributeData
  {
    internal int FileAttributes;
    internal FileTime CreateTime;
    internal FileTime LastAccess;
    internal FileTime LastWrite;
    internal int HiSize;
    internal int LoSize;
  }
  public static partial class WinAGI
  {
    internal const double LOG10_1_12 = 2.50858329719984E-02; // = Log10(2 ^ (1/12))
    internal const string QUOTECHAR = "\"";
    internal const string ARG1 = "%1";
    internal const string ARG2 = "%2";
    internal const string ARG3 = "%3";
    internal const string sAPPNAME = "WinAGI Game Development System 2.1";
    internal const string COPYRIGHT_YEAR = "2021";
    //constants used to build bitmaps
    internal const int BI_RGB = 0;
    internal const int DIB_RGB_COLORS = 0;
    internal const int DIB_PAL_COLORS = 1;
    internal const int FLOODFILLBORDER = 0;
    internal const int FLOODFILLSURFACE = 1;
    internal const int BLACKNESS = 0x42;
    internal const int DSTINVERT = 0x550009;
    internal const int MERGECOPY = 0xC000CA;
    internal const int MERGEPAINT = 0xBB0226;
    internal const int NOTSRCCOPY = 0x330008;
    internal const int NOTSRCERASE = 0x1100A6;
    internal const int PATCOPY = 0xF00021;
    internal const int PATINVERT = 0x5A0049;
    internal const int PATPAINT = 0xFB0A09;
    internal const int SRCAND = 0x8800C6;
    internal const int SRCCOPY = 0xCC0020;
    internal const int SRCERASE = 0x440328;
    internal const int SRCINVERT = 0x660046;
    internal const int SRCPAINT = 0xEE0086;
    internal const int WHITENESS = 0xFF0062;
    internal const int TRANSCOPY = 0xB8074A;

    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, Int32 lParam);
    public const int WM_SETREDRAW = 0xB;

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    public static extern short GetKeyState(int keyCode);
    //apis for bitmap creation/manipulation
    [DllImport("Winmm.dll", SetLastError = true)]
    internal static extern int mciSendString(string lpszCommand, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn, IntPtr hwndCallback);
    [DllImport("Winmm.dll", SetLastError = true)]
    internal static extern int mciGetErrorString(int errNum, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn);
    [DllImport("gdi32.dll")]
    internal static extern int GetTickCount();
    [DllImport("gdi32.dll")]
    internal static extern int BitBlt(int hDestDC, int X, int Y, int nWidth, int nHeight, int hSrcDC, int xSrc, int ySrc, int dwRop);
    [DllImport("gdi32.dll")]
    internal static extern int CreateDIBSection(int hDC, BitmapInfo pBitmapInfo, int un, int lplpVoid, int handle, int dw);
    [DllImport("gdi32.dll")]
    internal static extern IntPtr CreateCompatibleDC(IntPtr hDC);
    [DllImport("gdi32.dll")]
    internal static extern int DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll")]
    internal static extern int DeleteDC(IntPtr hDC);
    [DllImport("gdi32.dll")]
    internal static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    [DllImport("gdi32.dll")]
    internal static extern int StretchBlt(IntPtr destDC, int destX, int destY, int destW, int destH, IntPtr srcDC, int srcX, int srcY, int srcW, int srcH, int dwRop);
    [DllImport("msimg32.dll")]
    internal static extern int TransparentBlt(IntPtr destDC, int destX, int destY, int destW, int destH, IntPtr srcDC, int srcX, int srcY, int srcW, int srcH, int crTransparent);
    [DllImport("hhctrl.ocx")]
    internal static extern int HtmlHelpS(IntPtr hwndCaller, string pszFile, int uCommand, string dwData);
    [DllImport("hhctrl.ocx")]
    internal static extern int HtmlHelp(IntPtr hwndCaller, string pszFile, int uCommand, int dwData);
    internal const int HH_DISPLAY_TOPIC = 0x0;
    internal const int HH_DISPLAY_INDEX = 0x2;
    internal const int HH_DISPLAY_SEARCH = 0x3;
    internal const int HH_DISPLAY_TOC = 0x1;
    internal const int HH_SET_WIN_TYPE = 0x4;
    internal const int HH_GET_WIN_TYPE = 0x5;
    internal const int HH_GET_WIN_HANDLE = 0x6;
    // Display string resource ID or text in a popupwin.
    internal const int HH_DISPLAY_TEXT_POPUP = 0xE;
    // Display mapped numeric Value in dwdata
    internal const int HH_HELP_CONTEXT = 0xF;
    // Text pop-up help, similar to WinHelp's HELP_CONTEXTMENU
    internal const int HH_TP_HELP_CONTEXTMENU = 0x10;
    // Text pop-up help, similar to WinHelp's HELP_WM_HELP
    internal const int HH_TP_HELP_WM_HELP = 0x11;

    internal struct BLENDFUNCTION
    {
      internal byte BlendOp;
      internal byte BlendFlags;
      internal byte SourceConstantAlpha;
      internal byte AlphaFormat;
    }
    internal const int AC_SRC_OVER = 0x00;
    internal const int AC_SRC_ALPHA = 0x01;
    internal const int AC_SRC_NO_PREMULT_ALPHA = 0x01;
    internal const int AC_SRC_NO_ALPHA = 0x02;
    internal const int AC_DST_NO_PREMULT_ALPHA = 0x10;
    internal const int AC_DST_NO_ALPHA = 0x20;

    [DllImport("msimg32.dll")]
    internal static extern int AlphaBlend(IntPtr destDC, int destX, int destY, int destW, int destH, IntPtr srcDC, int srcX, int srcY, int srcW, int srcH, BLENDFUNCTION ftn);
    [DllImport("gdi32.dll")]
    internal static extern int GetLastError();
    [DllImport("gdi32.dll")]
    internal static extern int CreateCompatibleBitmap(int hDC, int nWidth, int nHeight);
    [DllImport("gdi32.dll")]
    internal static extern int SetPixelV(int hDC, int X, int Y, int crColor);
    [DllImport("gdi32.dll")]
    internal static extern int GetPixel(int hDC, int X, int Y);
    [DllImport("gdi32.dll")]
    internal static extern int ExtFloodFill(int hDC, int X, int Y, int crColor, int wFillType);
    [DllImport("gdi32.dll")]
    internal static extern int CreateSolidBrush(int crColor);
    //[DllImport("kernel32.dll")]
    //internal static extern void CopyMemory(dynamic Destination, dynamic Source, int Length) alias RtlMoveMemory;
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "RtlMoveMemory", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    [ResourceExposure(ResourceScope.None)]
    internal static extern void CopyMemory(HandleRef destData, HandleRef srcData, int size);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int GetShortPathName(string pathName, StringBuilder shortName, int cbShortName);

    [DllImport("gdi32.dll")]
    internal static extern int SetBkColor(int hDC, int crColor);
    internal static Array ResizeArray(Array arr, int[] newSizes)
    {
      if (newSizes.Length != arr.Rank)
        throw new ArgumentException("arr must have the same number of dimensions " +
                                    "as there are elements in newSizes", "newSizes");
      var temp = Array.CreateInstance(arr.GetType().GetElementType(), newSizes);
      int length = arr.Length <= temp.Length ? arr.Length : temp.Length;
      Array.ConstrainedCopy(arr, 0, temp, 0, length);
      return temp;
    }
    static void tmpCommon()
    {
      /*
Option Explicit
  
  internal Type RGBQUAD
    byte rgbBlue
    byte rgbGreen
    byte rgbRed
    byte rgbReserved
  End Type
  
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
        //because it//s part of a slash code
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

internal bool IsValidMsg(string MsgText)
{
  //this function will check MsgText, and returns TRUE if
  //it start with a dbl quote, AND ends with a valid dbl
  //quote, taking into account potential slash codes
  
  int lngSlashCount
  
  On Error GoTo ErrHandler
  
  if (Asc(MsgText) != 34) {
    //not valid
    IsValidMsg = false
    return
  }
  
  //need at least two chars to be a string
  if (MsgText.Length < 2) {
    //not valid
    IsValidMsg = false
    return
  }
       
  //if no dbl quote at end, not a string
  if (Right(MsgText, 1) != QUOTECHAR) {
    //not valid
    IsValidMsg = false
    return
  }
    
  //just because it ends in a quote doesn//t mean it//s good;
  //it might be an embedded quote
  //(we know we have at least two chars, so we don't need
  //to worry about an error with MID function)
  
  //check for an odd number of slashes immediately preceding
  //this quote
  lngSlashCount = 0
  do
  {
    if (Mid(MsgText, MsgText.Length - (lngSlashCount + 1), 1) == "\\") {
      lngSlashCount = lngSlashCount + 1
    } else {
      break;
    }
  } while (MsgText.Length - (lngSlashCount + 1) >= 0);
            
  //if it IS odd, then it//s not a valid quote
  if (Int(lngSlashCount / 2) != CSng(lngSlashCount / 2)) {
    //it//s embedded, and doesn//t count
    IsValidMsg = false
    return
  }
  
  //if passes all the tests, it//s OK
  IsValidMsg = true
  
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

    internal static bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
      // Get the subdirectories for the specified directory.
      DirectoryInfo dir = new DirectoryInfo(sourceDirName);
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
      // success
      return true;
    }
    static void tmpStuff2()
    {
      /*
    internal string FileNameNoExt(string FileName)
    {
      //returns a filename without the extension
      //if FileName includes a path, the path is also removed

      string strOut
          int i

      strOut = JustFileName(FileName)

      i = InStrRev(strOut, ".")

      if (i <= 0) {
        FileNameNoExt = strOut
      } else {
        FileNameNoExt = Left(strOut, i - 1)
      }
    }

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

    static void APIFunctions()
    {
      /*
Option Explicit

  'subclassing variables
  internal PrevLEWndProc As Long
  internal PrevTbarWndProc As Long
  internal PrevPropWndProc As Long
  
  internal TBCmd As Long 'identifies which menu command is chosen
  
  'savefile configuration
  internal ofn As OPENFILENAME
  
  'start directory for directory browser
  internal BrowserStartDir As String
  
  internal SelectedFolder As String
  
  'gdiplus token
  internal gdiToken As Long
  internal NoGDIPlus As Boolean
  
  internal const WM_USER = 0x400
  'edit box messages
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
  internal const EM_SETLIMITTEXT = EM_LIMITTEXT  'win40 Name change
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

  'directory browser constants
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
  
  'printer capabilities
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
    
  'open/save dialog constants
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

  'directory browser types
  internal Type BrowseInfo
    hWndOwner      As Long
    pIDLRoot       As Long
    pszDisplayName As String
    lpszTitle      As String
    ulFlags        As Long
    lpfnCallback   As Long
    lParam         As Long
    iImage         As Long
  End Type

  'types used in building bitmaps
  internal Type BITMAPINFOHEADER  '40 bytes
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
  
  internal Type MIDIHDR
    lpData As String
    dwBufferLength As Long
    dwBytesRecorded As Long
    dwUser As Long
    dwFlags As Long
    lpNext As Long
    Reserved As Long
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

'checking the WinAPI webpages, it looks like
'hDC and hdcTarget are backwards in previous
'versions (the version that's commented out)
'but the print functions all worked?
'  internal Type vbFORMATRANGE
'    hDC As Long
'    hdcTarget As Long
'    rc As vbRECT
'    rcPage As vbRECT
'    chrg As CHARRANGE
'  End Type


'in v1.2.8+ I swapped them to what I think is
'correct; print function still works the same
'so I don't know what's going on---
  internal Type vbFORMATRANGE
    hdcTarget As Long
    hDC As Long
    rc As vbRECT
    rcPage As vbRECT
    chrg As CHARRANGE
  End Type
  
  internal Type DOCINFO
    cbSize As Long
    lpszDocName As String
    lpszOutput As String
    lpszDatatype As String
    fwType As Long
  End Type
  
  internal Type NMHDR
    hwndFrom As Long
    idFrom As Long
    code As Long
    ptrOFN As Long
  End Type
  
  internal Type OPENFILENAME
    lStructSize As Long
    hWndOwner As Long
    hInstance As Long
    lpstrFilter As String
    lpstrCustomFilter As String
    nMaxCustFilter As Long
    nFilterIndex As Long
    lpstrFile As String
    nMaxFile As Long
    lpstrFileTitle As String
    nMaxFileTitle As Long
    lpstrInitialDir As String
    lpstrTitle As String
    Flags As Long
    nFileOffset As Integer
    nFileExtension As Integer
    lpstrDefExt As String
    lCustData As Long
    lpfnHook As Long
    lpTemplateName As String
    pvReserved As Long
    dwReserved As Long
    FlagsEx As Long
  End Type
  
  internal Type OFNOTIFY
    hdr As NMHDR
    lpOFN As OPENFILENAME
    pszFile As String ' May be NULL
  End Type
  
  internal Type OSVERSIONINFO
    dwOSVersionInfoSize As Long
    dwMajorVersion As Long
    dwMinorVersion As Long
    dwBuildNumber As Long
    dwPlatformId As Long
    szCSDVersion As String * 128 ' Maintenance string for PSS usage
  End Type
  
  internal Type MONITORINFO
    cbSize As Long
    rcMonitor As vbRECT
    rcWork As vbRECT
    dwFlags As Long
  End Type
  
  internal Type WINDOWPLACEMENT
    Length As Long 'actual length is 60, but it changes to 44 after running - why?
    Flags As Long
    showCmd As Long
    ptMinPosition As POINTAPI
    ptMaxPosition As POINTAPI
    rcNormalPosition As vbRECT
'''    rcDevice As vbRECT              'BECAUSE this element doesn't really exist
  End Type
    
  
  'api declarations
  internal Declare Function SHBrowseForFolder Lib "shell32" (lpbi As BrowseInfo) As Long
  internal Declare Function SHGetPathFromIDList Lib "shell32" (ByVal pidList As Long, ByVal lpBuffer As String) As Long

  'according to most recent MSDN, use CoTaskMemFree in place of ITMalloc.Free
  Private Declare Sub CoTaskMemFree Lib "ole32" (pv As Long)

  internal Declare Function GetWindowText Lib "user32" (ByVal hWnd As Long, ByVal lpString As String, ByVal nMaxCount As Long) As Long

  internal Declare Function ShellExecute Lib "shell32.dll" Alias "ShellExecuteA" (ByVal hWnd As Long, ByVal lpOperation As String, ByVal lpFile As String, ByVal lpParameters As String, ByVal lpDirectory As String, ByVal nShowCmd As Long) As Long
  internal Declare Function SetWindowPos Lib "user32.dll" (ByVal hWnd As Long, ByVal hWndInsertAfter As Long, ByVal X As Long, ByVal Y As Long, ByVal cx As Long, ByVal cy As Long, ByVal wFlags As Long) As Long
  internal Declare Function lstrcat Lib "kernel32" Alias "lstrcatA" (ByVal lpString1 As String, ByVal lpString2 As String) As Long
  internal Declare Function GetCurrentDirectory Lib "kernel32" Alias "GetCurrentDirectoryA" (ByVal nBufferLength As Long, ByVal lpBuffer As String) As Long
  internal Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
  internal Declare Function SendMessageAny Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, lParam As Any) As Long
  internal Declare Function SendMessagePtW Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, wParam As POINTAPI, ByVal lParam As Long) As Long
  internal Declare Function SendMessagePtL Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, lParam As POINTAPI) As Long
  internal Declare Function SendMessageRctL Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, lParam As RECT) As Long
  internal Declare Function SendMessageByString Lib "user32" Alias "SendMessageA" (ByVal hWnd As Long, ByVal wMsg As Long, ByVal wParam As Long, ByVal lParam As String) As Long
  
  internal Declare Function GetDeviceCaps Lib "gdi32" (ByVal hDC As Long, ByVal nIndex As Long) As Long
  internal Declare Function GetDC Lib "user32" (ByVal hWnd As Long) As Long
  internal Declare Function GetTickCount Lib "kernel32" () As Long
  internal Declare Function SetClipboardData Lib "user32" Alias "SetClipboardDataA" (ByVal wFormat As Long, ByVal hMem As Long) As Long
  internal Declare Function BitBlt Lib "gdi32" (ByVal hDestDC As Long, ByVal X As Long, ByVal Y As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal hSrcDC As Long, ByVal xSrc As Long, ByVal ySrc As Long, ByVal dwRop As Long) As Long
  internal Declare Function MaskBlt Lib "gdi32" (ByVal hdcDest As Long, ByVal nXDest As Long, ByVal nYDest As Long, ByVal nWidth, ByVal nHeight As Long, ByVal hdcSrc As Long, ByVal nXSrc As Long, ByVal nYSrc As Long, ByVal hbmMask As Long, ByVal xMask As Long, ByVal yMask As Long, ByVal dwRop As Long) As Long
  internal Declare Function TransparentBlt Lib "msimg32.dll" (ByVal hdcDest As Long, ByVal xoriginDest As Long, ByVal yoriginDest As Long, ByVal wDest As Long, ByVal hDest As Long, ByVal hdcSrc As Long, ByVal xoriginSrc As Long, ByVal yoriginSrc As Long, ByVal wSrc As Long, ByVal hSrc As Long, ByVal crTransparent As Long) As Long
  internal Declare Function AlphaBlend Lib "msimg32.dll" (ByVal hdcDest As Long, ByVal xoriginDest As Long, ByVal yoriginDest As Long, ByVal wDest As Long, ByVal hDest As Long, ByVal hdcSrc As Long, ByVal xoriginSrc As Long, ByVal yoriginSrc As Long, ByVal wSrc As Long, ByVal hSrc As Long, ByVal blendf As Long) As Long
  'the BLENDFUNCTION Type is comprised of 4 bytes; so essentially a Long value
  'only non-zero value is SourceConstantAlpha, so it's easy to pass
  'the Type as a long with any given SourceConstantAlpha value where
  'BF_Value = Clng(SCA * 0x10000)

'  internal Type BLENDFUNCTION
'    BlendOp As Byte
'    BlendFlags As Byte
'    SourceConstantAlpha As Byte
'    AlphaFormat As Byte
'  End Type
'
'  internal const AC_SRC_OVER = 0x0                '(0)
'  ' alpha format flags
'  internal const AC_SRC_NO_PREMULT_ALPHA = 0x1    '(1)
'  internal const AC_SRC_NO_ALPHA = 0x2            '(2)
'  internal const AC_DST_NO_PREMULT_ALPHA = 0x10   '(16)
'  internal const AC_DST_NO_ALPHA = 0x20           '(32)

  internal Declare Function ExtFloodFill Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long, ByVal crColor As Long, ByVal wFillType As Long) As Long
  internal Declare Function StretchBlt Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal hSrcDC As Long, ByVal xSrc As Long, ByVal ySrc As Long, ByVal nSrcWidth As Long, ByVal nSrcHeight As Long, ByVal dwRop As Long) As Long
  internal Declare Function SetStretchBltMode Lib "gdi32.dll" (ByVal hDC As Long, ByVal nStretchMode As Long) As Long
  internal Declare Function GetShortPathName Lib "kernel32" Alias "GetShortPathNameA" (ByVal lpszLongPath As String, ByVal lpszShortPath As String, ByVal cchBuffer As Long) As Long
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
  
  internal Declare Function midiOutClose Lib "winmm.dll" (ByVal hMidiOut As Long) As Long
  internal Declare Function midiOutLongMsg Lib "winmm.dll" (ByVal hMidiOut As Long, lpMidiOutHdr As MIDIHDR, ByVal uSize As Long) As Long
  internal Declare Function midiOutOpen Lib "winmm.dll" (lphMidiOut As Long, ByVal uDeviceID As Long, ByVal dwCallback As Long, ByVal dwInstance As Long, ByVal dwFlags As Long) As Long
  internal Declare Function midiOutPrepareHeader Lib "winmm.dll" (ByVal hMidiOut As Long, lpMidiOutHdr As MIDIHDR, ByVal uSize As Long) As Long
  internal Declare Function midiOutReset Lib "winmm.dll" (ByVal hMidiOut As Long) As Long
  internal Declare Function midiOutShortMsg Lib "winmm.dll" (ByVal hMidiOut As Long, ByVal dwMsg As Long) As Long
  internal Declare Function midiOutUnprepareHeader Lib "winmm.dll" (ByVal hMidiOut As Long, lpMidiOutHdr As MIDIHDR, ByVal uSize As Long) As Long
  internal Declare Function mciSendString Lib "winmm.dll" Alias "mciSendStringA" (ByVal lpstrCommand As String, ByVal lpstrReturnString As String, ByVal uReturnLength As Long, ByVal hwndCallback As Long) As Long

  internal Declare Function BeginPath Lib "gdi32" (ByVal hDC As Long) As Long
  internal Declare Function EndPath Lib "gdi32" (ByVal hDC As Long) As Long
  internal Declare Function WidenPath Lib "gdi32" (ByVal hDC As Long) As Long
  internal Declare Function PathToRegion Lib "gdi32" (ByVal hDC As Long) As Long
  
  internal Declare Function FillRgn Lib "gdi32" (ByVal hDC As Long, ByVal hRgn As Long, ByVal hBrush As Long) As Long
  internal Declare Function PaintRgn Lib "gdi32" (ByVal hDC As Long, ByVal hRgn As Long) As Long
  internal Declare Function CreatePolygonRgn Lib "gdi32" (lpPoint As POINTAPI, ByVal nCount As Long, ByVal nPolyFillMode As Long) As Long
  internal Declare Function FrameRgn Lib "gdi32" (ByVal hDC As Long, ByVal hRgn As Long, ByVal hBrush As Long, ByVal nWidth As Long, ByVal nHeight As Long) As Long
  internal Declare Function CreateRoundRectRgn Lib "gdi32" (ByVal X1 As Long, ByVal Y1 As Long, ByVal X2 As Long, ByVal Y2 As Long, ByVal X3 As Long, ByVal Y3 As Long) As Long
  internal Declare Function Polygon Lib "gdi32" (ByVal hDC As Long, lpPoint As POINTAPI, ByVal nCount As Long) As Long
  internal Declare Function PtInRegion Lib "gdi32" (ByVal hRgn As Long, ByVal X As Long, ByVal Y As Long) As Long
  internal Declare Function LineTo Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long) As Long
  internal Declare Function MoveToEx Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long, lpPoint As Long) As Long
  internal Declare Function ScreenToClient Lib "user32.dll" (ByVal hWnd As Long, lpPoint As POINTAPI) As Long

''' can't get APIs to draw wider dotted lines; can't create a brush - don't know why
'''internal Type LOGBRUSH
'''  lbStyle As Long
'''  lbColor As Long
'''  lbHatch As Long
'''End Type
'''internal const BS_SOLID = 0
'''internal Declare Function ExtCreatePen Lib "gdi32" (ByVal dwPenStyle As Long, ByVal dwWidth As Long, lplb As LOGBRUSH, ByVal dwStyleCount As Long, lpStyle As Long) As Long
'''internal const PS_COSMETIC = 0
'''internal const PS_GEOMETRIC = 65536
'''internal const PS_SOLID = 0
'''internal const PS_DOT = 2
'''internal const PS_DASH = 1
'''internal const PS_ENDCAP_FLAT = 512
'''internal const PS_JOIN_BEVEL = 4096

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
  
  'monitor functions
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
  

' Display string resource ID or text in a popupwin.
  internal const HH_DISPLAY_TEXT_POPUP = 0xE
' Display mapped numeric Value in dwdata
  internal const HH_HELP_CONTEXT = 0xF
' Text pop-up help, similar to WinHelp's HELP_CONTEXTMENU
  internal const HH_TP_HELP_CONTEXTMENU = 0x10
' Text pop-up help, similar to WinHelp's HELP_WM_HELP
  internal const HH_TP_HELP_WM_HELP = 0x11

  internal Declare Function GdiplusStartup Lib "gdiplus" (token As Long, inputbuf As GdiplusStartupInput, Optional ByVal outputbuf As Long = 0) As GpStatus
  internal Declare Sub GdiplusShutdown Lib "gdiplus" (ByVal token As Long)
  internal Declare Function GdipGetImageEncoders Lib "gdiplus" (ByVal numEncoders As Long, ByVal Size As Long, ByRef encoders As Any) As GpStatus
  internal Declare Function GdipGetImageEncodersSize Lib "gdiplus" (ByRef numEncoders As Long, ByRef Size As Long) As GpStatus
  internal Declare Function GdipSaveImageToFile Lib "gdiplus" (ByVal Image As Long, ByVal FileName As String, clsidEncoder As CLSID, encoderParams As Any) As GpStatus
  internal Declare Function GdipLoadImageFromFile Lib "gdiplus" (ByVal FileName As String, Image As Long) As GpStatus
  internal Declare Function GdipDisposeImage Lib "gdiplus" (ByVal Image As Long) As GpStatus
  internal Declare Function CLSIDFromString Lib "ole32.dll" (ByVal lpszProgID As Long, pCLSID As CLSID) As Long

  internal Type GdiplusStartupInput
     GdiplusVersion As Long              ' Must be 1 for GDI+ v1.0, the current version as of this writing.
     DebugEventCallback As Long          ' Ignored on free builds
     SuppressBackgroundThread As Long    ' FALSE unless you're prepared to call
                                         ' the hook/unhook functions properly
     SuppressExternalCodecs As Long      ' FALSE unless you want GDI+ only to use
                                         ' its internal Image codecs.
  End Type
  
  'clsid Type used by gdiplus
  internal Type CLSID
    Data1 As Long
    Data2 As Integer
    Data3 As Integer
    Data4(7) As Byte
  End Type
  
  'gdiplus api results status codes
  internal Enum GpStatus
    gpsOk = 0
    gpsGenericError = 1
    gpsInvalidParameter = 2
    gpsOutOfMemory = 3
    gpsObjectBusy = 4
    gpsInsufficientBuffer = 5
    gpsNotImplemented = 6
    gpsWin32Error = 7
    gpsWrongState = 8
    gpsAborted = 9
    gpsFileNotFound = 10
    gpsValueOverflow = 11
    gpsAccessDenied = 12
    gpsUnknownImageFormat = 13
    gpsFontFamilyNotFound = 14
    gpsFontStyleNotFound = 15
    gpsNotTrueTypeFont = 16
    gpsUnsupportedGdiplusVersion = 17
    gpsGdiplusNotInitialized = 18
    gpsPropertyNotFound = 19
    gpsPropertyNotSupported = 20
    gpsProfileNotFound = 21
  End Enum

  'codec Type for images
  internal Type ImageCodecInfo
    ClassID As CLSID
    FormatID As CLSID
    CodecName As Long
    DllName As Long
    FormatDescription As Long
    FilenameExtension As Long
    MimeType As Long
    Flags As ImageCodecFlags
    Version As Long
    SigCount As Long
    SigSize As Long
    SigPattern As Long
    SigMask As Long
  End Type
      
  ' Encoder Parameter structure
  internal Type EncoderParameter
    GUID As CLSID                          ' GUID of the parameter
    NumberOfValues As Long                 ' Number of the parameter values; usually 1
    Type As EncoderParameterValueType      ' Value Type, like ValueTypeLONG  etc.
    Value As Long                          ' A pointer to the parameter values
  End Type
  
  ' Encoder Parameters structure
  internal Type EncoderParameters
    Count As Long                          ' Number of parameters in this structure; Should be 1
    Parameter As EncoderParameter          ' Parameter values; this CAN be an array!!!! (Use CopyMemory and a string or byte array as workaround)
  End Type

  ' Information flags about Image codecs
  internal Enum ImageCodecFlags
     ImageCodecFlagsEncoder = 0x1
     ImageCodecFlagsDecoder = 0x2
     ImageCodecFlagsSupportBitmap = 0x4
     ImageCodecFlagsSupportVector = 0x8
     ImageCodecFlagsSeekableEncode = 0x10
     ImageCodecFlagsBlockingDecode = 0x20
  
     ImageCodecFlagsBuiltin = 0x10000
     ImageCodecFlagsSystem = 0x20000
     ImageCodecFlagsUser = 0x40000
  End Enum

  ' Image encoder parameter related types
  internal Enum EncoderParameterValueType
    EncoderParameterValueTypeByte = 1              ' 8-bit unsigned int
    EncoderParameterValueTypeASCII = 2             ' 8-bit byte containing one 7-bit ASCII
                                                    ' code. NULL terminated.
    EncoderParameterValueTypeShort = 3             ' 16-bit unsigned int
    EncoderParameterValueTypeLong = 4              ' 32-bit unsigned int
    EncoderParameterValueTypeRational = 5          ' Two Longs. The first Long is the
                                                    ' numerator the second Long expresses the
                                                    ' denomintor.
    EncoderParameterValueTypeLongRange = 6         ' Two longs which specify a range of
                                                    ' integer values. The first Long specifies
                                                    ' the lower end and the second one
                                                    ' specifies the higher end. All values
                                                    ' are inclusive at both ends
    EncoderParameterValueTypeUndefined = 7         ' 8-bit byte that can take any Value
                                                    ' depending on field definition
    EncoderParameterValueTypeRationalRange = 8      ' Two Rationals. The first Rational
                                                    ' specifies the lower end and the second
                                                    ' specifies the higher end. All values
                                                    ' are inclusive at both ends
  End Enum

  ' ---------------------------------------------------------------------------
  ' Encoder parameter sets
  ' ---------------------------------------------------------------------------
  internal const EncoderCompression       As String = "{E09D739D-CCD4-44EE-8EBA-3FBF8BE4FC58}"
  internal const EncoderColorDepth        As String = "{66087055-AD66-4C7C-9A18-38A2310B8337}"
  internal const EncoderScanMethod        As String = "{3A4E2661-3109-4E56-8536-42C156E7DCFA}"
  internal const EncoderVersion           As String = "{24D18C76-814A-41A4-BF53-1C219CCCF797}"
  internal const EncoderRenderMethod      As String = "{6D42C53A-229A-4825-8BB7-5C99E2B9A8B8}"
  internal const EncoderQuality           As String = "{1D5BE4B5-FA4A-452D-9CDD-5DB35105E7EB}"
  internal const EncoderTransformation    As String = "{8D0EB2D1-A58E-4EA8-AA14-108074B7B6F9}"
  internal const EncoderLuminanceTable    As String = "{EDB33BCE-0266-4A77-B904-27216099E717}"
  internal const EncoderChrominanceTable  As String = "{F2E455DC-09B3-4316-8260-676ADA32481C}"
  internal const EncoderSaveFlag          As String = "{292266FC-AC40-47BF-8CFC-A85B89A655DE}"
  internal const CodecIImageBytes         As String = "{025D1823-6C7D-447B-BBDB-A3CBC3DFA2FC}"
  
'''internal Type COLORSTRUC
'''  lStructSize As Long
'''  hWnd As Long
'''  hInstance As Long
'''  rgbResult As Long
'''  lpCustColors As Long 'pointer to an array of 16 Long integers
'''  Flags As Long
'''  lCustData As Long
'''  lpfnHook As Long
'''  lpTemplateName As String
'''End Type
'''internal CC_CustomColors(15) As Long
'''
'''internal const CC_RGBINIT = 0x1
'''internal const CC_FULLOPEN = 0x2
'''internal const CC_PREVENTFULLOPEN = 0x4
'''internal const CC_SHOWHELP = 0x8
'''internal const CC_ENABLEHOOK = 0x10
'''internal const CC_ENABLETEMPLATE = 0x20
'''internal const CC_ENABLETEMPLATEHANDLE = 0x40
'''internal const CC_SOLIDCOLOR = 0x80
'''internal const CC_ANYCOLOR = 0x100
'''
'''Private Declare Function ChooseColor Lib "comdlg32.dll" Alias "ChooseColorA" (pChoosecolor As COLORSTRUC) As Long
'''
'''internal Function aDialogColor(ByRef NewColor As Long, ByVal hWndOwner As Long) As Boolean
'''
'''  Dim X As Long, CS As COLORSTRUC, CustColor(16) As Long
'''
''''  If CC_CustomColors(0) = 0 Then
''''    For X = 0 To 15
''''      CC_CustomColors(X) = 0xFFFFFF
''''    Next X
''''  End If
'''
'''  With CS
'''    .lStructSize = Len(CS)
'''    .hWnd = hWndOwner
'''    .Flags = CC_SOLIDCOLOR Or CC_RGBINIT Or CC_FULLOPEN
'''
'''    .lpCustColors = VarPtr(CC_CustomColors(0)) 'String$(16 * 4, 0)
'''    X = ChooseColor(CS)
'''    If X = 0 Then
'''      ' ERROR - don't change color
'''      aDialogColor = False
'''      Exit Function
'''    Else
'''      ' Normal processing
'''       NewColor = .rgbResult
'''    End If
'''  End With
'''
'''  aDialogColor = True
'''End Function
' Courtesy of: Dana Seaman
' Helper routine to convert a CLSID(aka GUID) string to a structure
internal Function DEFINE_GUID(ByVal sGuid As String) As CLSID
   ' Example ImageFormatBMP = {B96B3CAB-0728-11D3-9D7B-0000F81EF32E}
   Call CLSIDFromString(StrPtr(sGuid), DEFINE_GUID)
End Function

internal Sub EndGDIPlus()

  On Error GoTo ErrHandler
  
  ' Unload the GDI+ Dll
  Call GdiplusShutdown(gdiToken)
Exit Sub

ErrHandler:
  'no action needed- we're quitting anyway
  '*'Debug.Assert False
  Err.Clear
End Sub

' Built-in encoders for saving: (You can *try* to get other types also)
'   Image/bmp
'   Image/jpeg
'   Image/gif
'   Image/tiff
'   Image/png
'
' Notes When Saving:
'The JPEG encoder supports the Transformation, Quality, LuminanceTable, and ChrominanceTable parameter categories.
'The TIFF encoder supports the Compression, ColorDepth, and SaveFlag parameter categories.
'The BMP, PNG, and GIF encoders no do not support additional parameters.
'
' Purpose:
'The function calls GetImageEncoders to get an array of ImageCodecInfo objects. If one of the
'ImageCodecInfo objects in that array represents the requested encoder, the function returns
'the index of the ImageCodecInfo object and copies the CLSID into the variable pointed to by
'pClsid. If the function fails, it returns –1.
internal Function GetEncoderClsid(strMimeType As String, ClassID As CLSID)
   Dim num As Long, Size As Long, i As Long
   Dim ICI() As ImageCodecInfo
   Dim buffer() As Byte
   
   GetEncoderClsid = -1 'Failure flag

   ' Get the encoder array size
   Call GdipGetImageEncodersSize(num, Size)
   If Size = 0 Then Exit Function ' Failed!

   ' Allocate room for the arrays dynamically
   ReDim ICI(1 To num) As ImageCodecInfo
   ReDim buffer(1 To Size) As Byte

   ' Get the array and string data
   Call GdipGetImageEncoders(num, Size, buffer(1))
   ' Copy the class headers
   Call CopyMemory(ICI(1), buffer(1), (Len(ICI(1)) * num))

   ' Loop through all the codecs
   For i = 1 To num
      ' Must convert the pointer into a usable string
      If StrComp(PtrToStrW(ICI(i).MimeType), strMimeType, vbTextCompare) = 0 Then
         ClassID = ICI(i).ClassID   ' Save the class id
         GetEncoderClsid = i        ' return the index number for success
         Exit For
      End If
   Next
   ' Free the memory
   Erase ICI
   Erase buffer
End Function

internal Sub InitGDIPlus()

  ' Load the GDI+ Dll
  Dim GpInput As GdiplusStartupInput
  
  On Error GoTo ErrHandler
  
  GpInput.GdiplusVersion = 1
  If GdiplusStartup(gdiToken, GpInput) <> gpsOk Then
    MsgBox "Unspecified error in graphics module for Image handling." + vbCrLf + _
           "Exporting images to JPG, PNG and GIF formats will be unavailable.", _
           vbExclamation + vbOKOnly, "Error loading GDI+"
    NoGDIPlus = True
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Err.Clear
End Sub


' From www.mvps.org/vbnet...i think
'   Dereferences an ANSI or Unicode string pointer
'   and returns a normal VB BSTR
internal Function PtrToStrW(ByVal lpsz As Long) As String
    Dim sOut As String
    Dim lLen As Long

    lLen = lstrlenW(lpsz)

    If (lLen > 0) Then
        sOut = StrConv(String$(lLen, vbNullChar), vbUnicode)
        Call CopyMemory(ByVal sOut, ByVal lpsz, lLen * 2)
        PtrToStrW = StrConv(sOut, vbFromUnicode)
    End If
End Function

internal Function MetsToPix(ByVal Mets As Long, Optional ByVal Vert As Boolean = False) As Long

  If Vert Then
    MetsToPix = Mets * 1440 / ScreenTWIPSY / 2540
  Else
    MetsToPix = Mets * 1440 / ScreenTWIPSX / 2540
  End If
End Function

internal Function PixToMets(ByVal Pixels As Long, Optional ByVal Vert As Boolean = False) As Long
  
  If Vert Then
    PixToMets = Pixels * ScreenTWIPSY * 2540 / 1440
  Else
    PixToMets = Pixels * ScreenTWIPSX * 2540 / 1440
  End If
End Function


Private Function ChangeExtension(ByRef FileName As String, Filter As String, Index As Long) As String

  'compares the extension on Filename to the extension belonging to
  'first extension for filter that numbered as Index
  '
  'if the filter extension is unique (no '*' or '?') the function returns
  'the extension (without '*.' leader)
  '
  'if they don't match, filename is modified to use the correct extension,
  '
  'if the filter is not unique, function returns empty string)
  '
  'filename is assumed to be valid;
  'filter is assumed to be pairs of description/file filters separated by
  'null character (ChrW$(0)) with an extra null character at the end
  '
  'Index is assumed valid; it is not checked for error here
  
  Dim strFileExt As String, strFilterExt As String
  Dim strExt() As String
  Dim lngPos As Long
  
  On Error GoTo ErrHandler
  
  'get extension of desired filter
  strExt = Split(Filter, ChrW$(0))
  
  strFilterExt = strExt(Index * 2 + 1)
  'filter should be in form '*.xxx'
  If Left$(strFilterExt, 2) <> "*." Then
    'no extension change required
    Exit Function
  End If
  'strip off the first two characters
  strFilterExt = Right$(strFilterExt, Len(strFilterExt) - 2)
  
  'no asterisks or question marks or periods
  If InStr(1, strFilterExt, ChrW$(42)) <> 0 Or InStr(1, strFilterExt, ChrW$(63)) <> 0 Or InStr(1, strFilterExt, ChrW$(46)) <> 0 Then
    'invalid filter; no extension change required
    Exit Function
  End If
  
  'no more than three characters
  If Len(strFilterExt) > 3 Or Len(strFilterExt) < 1 Then
    'invalid filter; no extension change required
    Exit Function
  End If
  
  'strFilterExt is the extension
  ChangeExtension = strFilterExt
  
  'if no filename yet
  If LenB(FileName) = 0 Then
    'no extension change required
    Exit Function
  End If
  
  'now get extension of filename
  lngPos = InStrRev(FileName, ChrW$(46))
  
  'if not found,
  If lngPos = 0 Then
    'no extension; add filter extension and return true
    FileName = FileName + "." + strFilterExt
    Exit Function
  End If
  
  'get extension
  strFileExt = Right$(FileName, Len(FileName) - lngPos)
  
  'compare
  If StrComp(strFileExt, strFilterExt, vbTextCompare) <> 0 Then
    'extension has changed; change filename to match
    FileName = Left$(FileName, lngPos - 1) + "." + strFilterExt
  End If
Exit Function

ErrHandler:
  'ignore any errors
  Err.Clear
  ChangeExtension = vbNullString
End Function

internal Function SaveDlgHook(ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long

  On Error Resume Next
  
  'change of filter occurs when notify msg contains a typechange parameter
  
  Dim strFile As String, strExt As String
  Dim rtn As Long, hPCDWin As Long
  Dim ofnNotify As OFNOTIFY
  
  Select Case uMsg
  Case WM_NOTIFY
    'copy lParam to ofn structure
    CopyMemory ofnNotify, ByVal lParam, 16
    Select Case ofnNotify.hdr.code
    Case CDN_TYPECHANGE
      'changing Type
      
      'copy enough of the OFN structure to obtain the new index number
      CopyMemory ofnNotify.lpOFN, ByVal ofnNotify.hdr.ptrOFN, 28
      
      'handle to parent dialog
      hPCDWin = GetParent(hWnd)
  
      'get current filename
      strFile = Space$(256)
      rtn = SendMessageByString(hPCDWin, CDM_GETSPEC, 256, ByVal strFile)
      'if error
      If rtn = 0 Then
        Exit Function
      End If
      'trim off trailing null character, and any spaces
      strFile = Trim$(strFile)
      If Asc(Right$(strFile, 1)) = 0 Then
        strFile = Left$(strFile, Len(strFile) - 1)
      End If
  
      'change extension to match new filter
      'if extension does not match selected filter
      strExt = ChangeExtension(strFile, ofn.lpstrFilter, ofnNotify.lpOFN.nFilterIndex - 1)
  
      'if an extension was selected
      If LenB(strExt) <> 0 Then
        'change filename displayed in the textbox
        rtn = SendMessageByString(hPCDWin, CDM_SETCONTROLTEXT, edt1, strFile)
        
        'change default extension
        'add trailing null char
        strExt = strExt + ChrW$(0)
        rtn = SendMessageByString(hPCDWin, CDM_SETDEFEXT, 0, strExt)
      End If
    End Select
  End Select
End Function




internal Function LEMainWndProc(ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
  
  On Error Resume Next
  
  'two things to check for- mouse wheel, which should zoom in or out
  'and clicking on scrollbars when they are at extremes - VB click event doesn't fire
  'so have to catch the message here
  
  Select Case uMsg
  Case WM_MOUSEWHEEL
    'if mousewheel button is NOT pressed, wheel changes scale
    'if mousewheel button IS pressed, wheel does a small scroll
    If (wParam And 0xFFFF) = 0x10 Then
      With LayoutEditor.VScroll1
        If wParam > 0 Then
          If .Value - .SmallChange >= .Min Then
            .Value = .Value - .SmallChange
          ElseIf .Value <> .Min Then
            .Value = .Min
          End If
        Else
          If .Value + .SmallChange <= .Max Then
            .Value = .Value + .SmallChange
          ElseIf .Value <> .Max Then
            .Value = .Max
          End If
        End If
      End With
    Else
      LayoutEditor.ChangeScale Sgn(wParam)
    End If
  
    
  Case WM_HSCROLL
    LayoutEditor.ExtendHScroll wParam And 0xFFFF
    
  Case WM_VSCROLL
    LayoutEditor.ExtendVScroll wParam And 0xFFFF
  End Select
  
  'pass along
  LEMainWndProc = CallWindowProc(PrevLEWndProc, hWnd, uMsg, wParam, lParam)
  
'when the wheel button is pressed and released we get this sequence of messages:
'''LE mouse:   0x210 (WM_PARENTNOTIFY)                0x00000207      0x015B040D
'''LE mouse:    0x21 (WM_MOUSEACTIVATE)               0x00950358      0x02070001 '0x207 means WM_MBUTTONDOWN?
'''LE mouse:    0x20 (WM_SETCURSOR)                   0x000A051C      0x02070001
'''LE mouse:  0x105A (unknown)                        0x0             0x0
'''LE mouse:    0x20 (WM_SETCURSOR)                   0x000A051C      0x02000001 '0x200 means WM_MOUSE_FIRST?

'wheel UP no button:
'wParam: 0x78 0000

'wheel UP wheel button:
'wParam: 0x78 0010

'wheel UP left button:
'wParam: 0x78 0001

'wheel UP right button: (not possible- context menu instead)

'wheel DOWN no button:
'wParam: 0xFF88 0000

'wheel DOWN wheel button:
'wParam: 0xFF88 0010

'wheel DOWN left button:
'wParam: 0xFF88 0001

End Function
internal Function ScrollWndProc(ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
  
  Dim cliPT As POINTAPI, lngTarget As Long
  
  On Error Resume Next
  
  Dim frm As Form, lngPrevWndProc As Long
  'find the correct window
  For Each frm In Forms
    If frm.Name = "frmGlobals" Then
      If frm.fgGlobals.hWnd = hWnd Then
        lngTarget = 1
        lngPrevWndProc = frm.PrevFGWndProc
        Exit For
      End If
    End If
    
    If frm.Name = "frmReserved" Then
      If frm.fgReserved.hWnd = hWnd Then
        lngTarget = 2
        lngPrevWndProc = frm.PrevFGWndProc
        Exit For
      End If
    End If
    
    If frm.Name = "frmObjectEdit" Then
      If frm.fgObjects.hWnd = hWnd Then
        lngTarget = 3
        lngPrevWndProc = frm.PrevFGWndProc
        Exit For
      End If
    End If
    
    If frm.Name = "frmMDIMain" Then
      If frm.fgWarnings.hWnd = hWnd Then
        lngTarget = 4
        lngPrevWndProc = frm.PrevFGWndProc
        Exit For
      End If
    End If

    If frm.Name = "frmPictureEdit" Then
      If frm.hWnd = hWnd Then
        lngTarget = 5
        lngPrevWndProc = frm.PrevPEWndProc
        Exit For
      End If
    End If
    
    If frm.Name = "frmSoundEdit" Then
      If frm.hWnd = hWnd Then
        lngTarget = 6
        lngPrevWndProc = frm.PrevSEWndProc
        Exit For
      End If
    End If
    
    If frm.Name = "frmViewEdit" Then
      ' check the form
      If frm.hWnd = hWnd Then
        lngTarget = 7
        lngPrevWndProc = frm.PrevVEWndProc
        Exit For
      End If
      ' and also the combo box
      If frm.cmbMotion.hWnd = hWnd Then
        lngTarget = 8
        lngPrevWndProc = frm.PrevCBWndProc
        Exit For
      End If
    End If
    '
  Next
  
  'if no form found
  If lngPrevWndProc = 0 Then
    Dim rtn As Long, strText As String
    strText = Space(50)
    rtn = GetWindowText(hWnd, strText, 50)
    '*'Debug.Print "unknown mw window??", strText
    Exit Function
  End If
  
  'need to trap wheel events
  Select Case uMsg
  Case WM_MOUSEWHEEL
    'convert x and y
    cliPT.X = lParam And 0xFFFF
    cliPT.Y = lParam / 0x10000
    ScreenToClient hWnd, cliPT
    
    Select Case lngTarget
    Case 5, 7 ' frmPictureEdit, frmViewEdit
      frm.MouseWheel wParam And 0xFFFF, wParam / 0x10000, cliPT.X, cliPT.Y
      ' for these editors, don't pass along mouse wheel events that
      ' aren't eplicitly handled by the form
      Exit Function
      
    Case 8 ' cmbMotion on frmViewEdit
      'ignore these
      '*'Debug.Print "no combo scroll!"
      Exit Function
      
    Case Else
      ' all other forms use the same procedure (for now)
      frm.MouseWheel wParam And 0xFFFF, wParam / 0x10000, cliPT.X, cliPT.Y
    End Select
        
  Case Else
    'mouse events for the grid?
    ' need to check for scroll msg when mouse pointer is on top row
    If uMsg = WM_MOUSEMOVE Then
      If frm.Name = "frmGlobals" Then
        If frm.NoScroll Then
          'extract x and y
          cliPT.X = (lParam And 0xFFFF)
          cliPT.Y = (lParam \ 0x10000)
          
          'raise the event manually in order to prevent
          'the grid from automatically scrolling
          'when clicking on the header to adjust split
          ' (convert pixels to twips)
          frm.MouseMove vbLeftButton, 0, CSng(cliPT.X * ScreenTWIPSX), CSng(cliPT.Y * ScreenTWIPSY)
          Exit Function
        End If
      End If
    End If
    
    'pass along
    ScrollWndProc = CallWindowProc(lngPrevWndProc, hWnd, uMsg, wParam, lParam)
  
  End Select
End Function

internal Function LBWndProc(ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
  
  On Error Resume Next
  
  Dim frm As Form, lngPrevWndProc As Long
  Dim lngTarget As Long
  
  'find the correct window
  For Each frm In Forms
    If frm.Name = "frmWordsEdit" Then
      If frm.lstWords.hWnd = hWnd Then
        lngPrevWndProc = frm.PrevWLBWndProc
        lngTarget = 1
        Exit For
      End If
      If frm.lstGroups.hWnd = hWnd Then
        lngPrevWndProc = frm.PrevGLBWndProc
        lngTarget = 2
        Exit For
      End If
    End If
    If frm.Name = "frmPictureEdit" Then
      If frm.lstCommands.hWnd = hWnd Then
        lngPrevWndProc = frm.PrevLBWndProc
        Exit For
      End If
    End If
  Next
  
  If lngPrevWndProc = 0 Then
    '*'Debug.Print "unknown lb window??"
    Exit Function
  End If
  
  'for groups box, ignore mousewheel if editing
  
  Select Case uMsg
  Case WM_CHAR
    'ignore char characters
    'this prevents Ctrl+key combination
    'from automatically scrolling the
    'list box
    Exit Function
    
  Case WM_LBUTTONDOWN, WM_LBUTTONUP
    'if control is pressed
    If (wParam And 0x8) Then
      Exit Function
    End If
    
  Case WM_MOUSEWHEEL
    If lngTarget = 1 Then
      'word editor/wordlist; ignore wheel movement if editbox showing
      If frm.txtWord.Visible Then
        Exit Function
      End If
      
    End If
    
    If lngTarget = 2 Then
      'word editor/grouplist; ignore wheel movement if editbox showing
      If frm.txtGrpNum.Visible Then
        Exit Function
      End If
    End If
    
  Case Else
  End Select
  
  'pass along
  LBWndProc = CallWindowProc(lngPrevWndProc, hWnd, uMsg, wParam, lParam)
End Function

internal Function TBWndProc(ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
  
  On Error GoTo ErrHandler
  
  ' need to override the builtin edit menu for these textboxes
  
  Dim frm As Form, lngPrevWndProc As Long
  Dim lngTarget As Long, Shift As Integer, X As Single, Y As Single
  
  'find the correct window
  For Each frm In Forms
    If frm.Name = "frmWordsEdit" Then
      If frm.txtWord.hWnd = hWnd Then
        lngPrevWndProc = frm.PrevWrdTBWndProc
        lngTarget = 1
        Exit For
      End If
      If frm.txtGrpNum.hWnd = hWnd Then
        lngPrevWndProc = frm.PrevGrpTBWndProc
        lngTarget = 2
        Exit For
      End If
    End If
    If frm.Name = "frmObjectEdit" Then
      If frm.txtMaxScreenObj.hWnd = hWnd Then
        lngPrevWndProc = frm.PrevTBMOWndProc
        lngTarget = 3
        Exit For
      End If
      If frm.txtRoomNo.hWnd = hWnd Then
        lngPrevWndProc = frm.PrevTBRmWndProc
        lngTarget = 4
        Exit For
      End If
    End If
  Next
  
  If lngPrevWndProc = 0 Then
    '*'Debug.Print "unknown TB window??"
    Exit Function
  End If
  
  'catch right mouse down, so builtin context menu can be
  ' ignored
  Select Case uMsg
  Case 0x204 'right mouse down
    'raise the mouse down event manually
    
    'buttons come from wParam
    If (wParam And 4) = 4 Then
      Shift = 1
    End If
    If (wParam And 8) = 8 Then
      Shift = Shift + 2
    End If
    'lParam has x,y
    X = (lParam And 0xFFFF) * ScreenTWIPSX
    Y = (lParam \ 0x10000) * ScreenTWIPSY
    
    Select Case lngTarget
    Case 1 'word editor, word text box
      frm.txtWord_MouseDown 2, Shift, X, Y
    Case 2 'word editor, group text box
      frm.txtGrpNum_MouseDown 2, Shift, X, Y
    Case 3 'object editor, max obj text box
      frm.txtMaxScreenObj_MouseDown 2, Shift, X, Y
    Case 4 'object editor, room text box
      frm.txtRoomNo_MouseDown 2, Shift, X, Y
    End Select
    Exit Function
    
  End Select
  
  'pass along
  TBWndProc = CallWindowProc(lngPrevWndProc, hWnd, uMsg, wParam, lParam)
Exit Function

ErrHandler:
  '*'Debug.Print "err: "; Err.Number, Err.Description
  Resume Next
End Function


internal Function TbarWndProc(ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
  
  Dim i As Long
  Dim tmpX As Long, tmpY As Long
  
  On Error Resume Next
  
'''  If uMsg = 0x210 Then
'''    Debug.Print "wParam: "; wParam, , "lParam: "; lParam
'''  End If
  
  'if user is clicking on a button
  If uMsg = 0x210 And (wParam And 0xFFFF) = WM_LBUTTONDOWN Then
    tmpX = lParam And 0xFFFF
    tmpY = lParam \ 0x10000
    
    'determine if a drawing tool button is being clicked
    For i = 2 To 6
      With LayoutEditor.Toolbar1.Buttons(i)
        If tmpX >= .Left And tmpX <= .Left + .Width And tmpY >= .Top And tmpY <= .Top + .Height Then
          'this is the button
          LayoutEditor.TBClicked i
          Exit For
        End If
      End With
    Next i
  End If
  
  'pass along all messages
  TbarWndProc = CallWindowProc(PrevTbarWndProc, hWnd, uMsg, wParam, lParam)
End Function

internal Function ByValAddressOf(ByRef AddressOfIn As Long)
  'a workaround function to allow
  'the Value of AddressOf to be assigned to a variable
  'VB doesn't allow it to be done directly
  

  ByValAddressOf = AddressOfIn
End Function


internal Function BrowseCallbackProc(ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long

  Dim rtn As Long
  Dim lpDir As Long
  Dim shCurDir As String
  
  Select Case uMsg
  Case BFFM_INITIALIZED
    'set directory by passing string pointer by Value
    '(pass 1(true) for wParam)
    rtn = SendMessageByString(hWnd, BFFM_SETSELECTION, 1, BrowserStartDir)
 
 Case BFFM_SELCHANGED
    shCurDir = String$(MAX_PATH, 0)
    If (SHGetPathFromIDList(wParam, shCurDir)) Then
      shCurDir = Left$(shCurDir, InStr(shCurDir, vbNullChar) - 1)
      lpDir = lstrcat(shCurDir, vbNullString)
      rtn = SendMessage(hWnd, BFFM_SETSTATUSTEXT, 1, ByVal lpDir)
    End If
    SelectedFolder = shCurDir
  End Select
End Function

internal Function DirFromID(idl As Long) As String
      
  'converts a pidl to a string
  
  Dim rtn As Long
  
  'create buffer to hold string
  DirFromID = Space$(MAX_PATH)
  'call conversion function
  rtn = SHGetPathFromIDList(idl, DirFromID)
  'trim the returned string
  DirFromID = Left$(DirFromID, InStr(DirFromID, vbNullChar) - 1)
End Function


internal Function GetNewDir(ByVal hWnd As Long, DialogMsg As String)

  On Error GoTo ErrHandler
  
  Dim lpIDList As Long
  Dim biNewDir As BrowseInfo
'  Dim ShellMalloc As ITMalloc
  Dim rtn As Long
  
  'build browser info structure
  With biNewDir
    'handle of parent window
    .hWndOwner = hWnd
    'message that appears above treelist
    .lpszTitle = DialogMsg
    'set flags
    .ulFlags = BIF_RETURNONLYFSDIRS + BIF_DONTGOBELOWDOMAIN + BIF_USENEWUI + BIF_STATUSTEXT
    'set pointer to callback address
    .lpfnCallback = ByValAddressOf(AddressOf BrowseCallbackProc)
    .pszDisplayName = String$(MAX_PATH, 32)
  End With

  'show browser, get pidl
  lpIDList = SHBrowseForFolder(biNewDir)

  'if not canceled (valid pidl returned)
  If lpIDList Then
    'last msg sent to callback function
    'has gotten us the name of the chosen folder
    GetNewDir = SelectedFolder
    'according to most recent MSDN info, use CoTaskMemFree
    'to free up the lpIDList handle and the root handle
    CoTaskMemFree lpIDList
  End If
  ' Whether successful or not, free the PIDL which was used to
  ' identify the My Computer virtual folder.
  CoTaskMemFree biNewDir.pIDLRoot
Exit Function

ErrHandler:
  '*'Debug.Print "bad thing happened: "; Err.Number, Err.Description
  Resume Next
End Function


internal Function PropWndProc(ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long

  Dim cliPT As POINTAPI
  
  On Error Resume Next
  
  'need to trap wheel events
  Select Case uMsg
  Case WM_MOUSEWHEEL
    
    'convert x and y
    cliPT.X = lParam And 0xFFFF
    cliPT.Y = lParam \ 0x10000
    ScreenToClient hWnd, cliPT
    
    frmMDIMain.PropMouseWheel wParam And 0xFFFF, wParam / 0x10000, cliPT.X, cliPT.Y
  End Select
  
  'pass along all messages
  PropWndProc = CallWindowProc(PrevPropWndProc, hWnd, uMsg, wParam, lParam)
End Function


      */
    }
    internal static string CompactPath(string LongPath, int MaxLength = 40)
    {
      //this method will ensure LongPath is compacted
      //to be less than MaxLength characters long, if possible
      //by eliminating directories and replacing them with ellipse(...)

      string strDir, strFile;

      //if already fits,
      if (LongPath.Length <= MaxLength) {
        //return entire path
        return LongPath;
      }
      //if no subdirectories
      if (!LongPath.Contains("\\")) {
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
      //truncate directory, pad with ... and return combined dir/filename
      return Left(strDir, MaxLength - 4) + "...\\" + strFile;
    }
    internal static string ShortFileName(string strLongFileName)
    {
      //returns the short filename of a file
      //to make it compatible with DOS programs
      int rtn;
      int lngStrLen;
      StringBuilder strTemp = new StringBuilder(0);
      try {
        //get size of required buffer
        lngStrLen = GetShortPathName(strLongFileName, strTemp, 0);
        strTemp = new StringBuilder((char)0, lngStrLen);
        //now get path
        rtn = GetShortPathName(strLongFileName, strTemp, lngStrLen);
        //if error
        if (lngStrLen == 0) {
          //ignore error
          return "";
        }
        ////strip off null char
        //strTemp = Left(strTemp, strTemp.Length - 1);
        return strTemp.ToString();
      }
      catch (Exception) {
        //ignore errors
        return "";
      }
    }
    internal static List<string> SplitLines(string strText)
    {
      // splits the input text into lines, by CR, LF, or CRLF
      // strategy is to replace CRLFs with CRs, then LFs with CRs,
      // and then slpit by CRs
      List<string> retval = new List<string>();
      retval.AddRange(strText.Replace("\n\r", "\n").Replace('\r', '\n').Split('\n'));
      return retval;
    }
  }
}
