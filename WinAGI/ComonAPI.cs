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
    internal static bool IsValidMsg(string MsgText)
    {
      //this function will check MsgText, and returns TRUE if
      //it start with a dbl quote, AND ends with a valid dbl
      //quote, taking into account potential slash codes

      int lngSlashCount = 0;
      if (MsgText[0] != '"') {
        //not valid
        return false;
      }
      //need at least two chars to be a string
      if (MsgText.Length < 2) {
        //not valid
        return false;
      }
      //if no dbl quote at end, not a string
      if (MsgText[MsgText.Length - 1] != '"') {
        //not valid
        return false;
      }
      //just because it ends in a quote doesn't mean it's good;
      //it might be an embedded quote
      //(we know we have at least two chars, so we don't need
      //to worry about an error with Mid function)

      //check for an odd number of slashes immediately preceding
      //the end quote
      do {
        //if (Mid(MsgText, MsgText.Length - (lngSlashCount + 1), 1) == "\\") {
        if (MsgText[MsgText.Length - 1 - (lngSlashCount + 1)] == '\\') {
          lngSlashCount++;
        }
        else {
          break;
        }
      } while (true); // eventually, starting quote will be found, which will exit the loop
                      //while (MsgText.Length - (lngSlashCount + 1) >= 0);

      //if it IS odd, then it's not a valid quote
      if ((lngSlashCount % 2) == 1) {
        //it's embedded, and doesn't count
        return false;
      }

      //if passes all the tests, it's OK
      return true;
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
    internal static string ChangeExtension(ref string FileName, string Filter, int Index)
    {
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
}
