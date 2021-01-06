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
    internal const string COPYRIGHT_YEAR = "2020";
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

    //api//s for bitmap creation/manipulation
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
    internal static extern int CreateCompatibleDC(int hDC);
    [DllImport("gdi32.dll")]
    internal static extern int DeleteObject(int hObject);
    [DllImport("gdi32.dll")]
    internal static extern int DeleteDC(int hDC);
    [DllImport("gdi32.dll")]
    internal static extern int SelectObject(int hDC, int hObject);
    [DllImport("gdi32.dll")]
    internal static extern int StretchBlt(int hDC, int X, int Y, int nWidth, int nHeight, int hSrcDC, int xSrc, int ySrc, int nSrWidth, int nSrHeight, int dwRop);
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
      lngPos = lngPos + 1
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
  //(we know we have at least two chars, so we don//t need
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
      if (!dir.Exists)
      {
        throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
      }
      DirectoryInfo[] dirs = dir.GetDirectories();
      try
      {
        // if (the destination directory doesn't exist, create it.       
        Directory.CreateDirectory(destDirName);
        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
          string tempPath = Path.Combine(destDirName, file.Name);
          file.CopyTo(tempPath, false);
        }
        // if (copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
          foreach (DirectoryInfo subdir in dirs)
          {
            string tempPath = Path.Combine(destDirName, subdir.Name);
            DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
          }
        }
      }
      catch (Exception)
      {
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


internal string CompactPath(string LongPath, int MaxLength = 40)
{
  //this method will ensure LongPath is compacted
  //to be less than MaxLength characters long, if possible
  //by eliminating directories and replacing them with ellipse(...)
  
  string[] strDirs()
  int i, lngLength
  int lngUbound
  bool blnPathOnly
  
  On Error GoTo ErrHandler
  
  //if already fits,
  if (LongPath.Length <= MaxLength) {
    //return entire path
    CompactPath = LongPath
    return
  }
  
  //if no subdirectories
  if (InStr(1, LongPath, "\\") == 0) {
    //return truncated path
    CompactPath = Left(LongPath, MaxLength - 3) + "..."
    return
  }
  
  //if not a valid path (i.e.doesn//t start with a drive letter, and a colon and a backslash
  if (AscW(LongPath.toLower()) < 97 || AscW(LongPath.ToLower()) > 122 || Mid(LongPath, 2, 1) != ":" || Mid(LongPath, 3, 1) != "\\") {
    //return truncated path
    CompactPath = Left(LongPath, MaxLength - 3) + "..."
    return
  }
  
  //if path ends with //\//
  if (Right(LongPath, 1) == "\\") {
    //strip it off temporarily
    LongPath = Left(LongPath, LongPath.Length - 1)
    //reduce maxlength by one so the //\// can be re-added
    MaxLength = MaxLength - 1
    //set flag
    blnPathOnly = true
  }
  
  //split into elements
  strDirs = Split(LongPath, "\\")
  
  //get upperbound
  lngUbound = UBound(strDirs)
  
  //if name is too long (longer than Max length minus drive and //\...\//
  if (strDirs(lngUbound).Length > MaxLength - strDirs(0).Length - 5) {
    //if only two elements
    if (lngUbound == 1) {
      //add drive and enough of name to just fit with a trailing ellipsis
      CompactPath = strDirs(0) + "\\" + Left(strDirs(lngUbound), MaxLength - strDirs(0).Length - 4) + "..."
    } else {
      //if REALLY short, so that even the ellipses won//t fit
      if (MaxLength - strDirs(0).Length - 8 < 2) {
        //just return string, truncated
        CompactPath = Left(LongPath, MaxLength - 3) + "..."
      } else {
        //add drive and //\..\// and enough of name to just fit with a trailing ellipsis
        CompactPath = strDirs(0) + "\...\" + Left(strDirs(lngUbound), MaxLength - strDirs(0).Length - 8) + "..."
      }
    }
    //if path only,
    if (blnPathOnly) {
      //add trailing backslash
      CompactPath = CompactPath + "\\"
    }
    return
  }
    
  //adjust maxlength to allow for the drive letter, and //\...\// and name (strDirs(lngUbound))
  MaxLength = MaxLength - strDirs(0).Length - 5 - strDirs(lngUbound).Length
  
  //add directories until too long
  i = lngUbound - 1
  
  For i = lngUbound - 1 To 1 Step -1
    //if remaining space left for path is not at least four characters (x..\)
    if (CompactPath.Length >= MaxLength - 4) {
      Exit For
    }
    
    //is there room for entire directory?
    if (strDirs(i).Length <= MaxLength - CompactPath.Length - 1) {
      //add entire directory
      CompactPath = strDirs(i) + "\\" + CompactPath
    } else {
      //add two dots to end of path so it adds up to maxlength
      CompactPath = Left(strDirs(i), MaxLength - CompactPath.Length - 3) + "..\" + CompactPath
    }
  Next i
  
  //add ellipse and name
  CompactPath = strDirs(0) + "\...\" + CompactPath + strDirs(lngUbound)
  //if path only,
  if (blnPathOnly) {
    //add trailing backslash
    CompactPath = CompactPath + "\\"
  }
return

ErrHandler:
  //Debug.Assert false
  //just return string, truncated
  CompactPath = Left(LongPath, MaxLength - 3) + "..."
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
      lngPos = lngPos + 1
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
    internal static string ShortFileName(string strLongFileName)
    {
      //returns the short filename of a file
      //to make it compatible with DOS programs
      int rtn;
      int lngStrLen;
      StringBuilder strTemp = new StringBuilder(0);
      try
      {
        //get size of required buffer
        lngStrLen = GetShortPathName(strLongFileName, strTemp, 0);
        strTemp = new StringBuilder((char)0, lngStrLen);
        //now get path
        rtn = GetShortPathName(strLongFileName, strTemp, lngStrLen);
        //if error
        if (lngStrLen == 0)
        {
          //ignore error
          return "";
        }
        ////strip off null char
        //strTemp = Left(strTemp, strTemp.Length - 1);
        return strTemp.ToString();
      }
      catch (Exception)
      {
        //ignore errors
        return "";
      }
    }
  }
}
