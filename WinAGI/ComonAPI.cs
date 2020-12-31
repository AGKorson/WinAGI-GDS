using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    //[DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [DllImport("kernel32.dll")]
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
    [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "RtlMoveMemory", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    [ResourceExposure(ResourceScope.None)]
    public static extern void CopyMemory(HandleRef destData, HandleRef srcData, int size);


    [DllImport("gdi32.dll")]
    internal static extern int SetBkColor(int hDC, int crColor);
  ////api//s for file stuff
  //internal static extern int GetTempPath Lib "kernel32" Alias "GetTempPathA" (ByVal nBufferLength As Long, ByVal lpBuffer As String) As Long
  //internal static extern int GetTempFileName Lib "kernel32" Alias "GetTempFileNameA" (ByVal lpszPath As String, ByVal lpPrefixString As String, ByVal wUnique As Long, ByVal lpTempFileName As String) As Long
  //internal static extern int GetShortPathName Lib "kernel32" Alias "GetShortPathNameA" (ByVal lpszLongPath As String, ByVal lpszShortPath As String, ByVal cchBuffer As Long) As Long
  //internal static extern int GetFileAttributesEx Lib "kernel32" Alias "GetFileAttributesExA" (ByVal FileName As String, ByVal InfoLevel As GET_FILEEX_INFO_LEVELS, FileInfo As W32FileAttributeData) As Long
  //internal static extern int FileTimeToSystemTime Lib "kernel32" (inFT As FileTime, outST As SystemTime) As Long
  ////api for extchar manipulations
  //internal static extern int WideCharToMultiByte Lib "kernel32" (ByVal CodePage As Long, ByVal dwFlags As Long, ByVal lpWideCharStr As String, ByVal cchWideChar As Long, ByVal lpMultiByteStr As String, ByVal cbMultiByte As Long, ByVal lpDefaultChar As String, ByRef lpUsedDefaultChar As Long) As Long
  //internal static extern int MultiByteToWideChar Lib "kernel32" (ByVal CodePage As Long, ByVal dwFlags As Long, lpMultiByteStr As String, ByVal cbMultiByte As Long, lpWideCharStr As String, cchWideChar As Long) As Long
  //Private intExtChar(127) As Integer
  static void tmpCommon()
    {
      /*
Option Explicit
  
  Private CRC32Table(255) As Long
  Private CRC32Loaded As Boolean
  
  Public Const LOG10_1_12 = 2.50858329719984E-02 ' = Log10(2 ^ (1/12))
  Public Const QUOTECHAR = """"
  Public Const ARG1 = "%1"
  Public Const ARG2 = "%2"
  Public Const ARG3 = "%3"
  
  Public Const WINAGI_VERSION = "2.1.1"
  Public Const WINAGI_VERSION_1_2 = "WINAGI v1.2     "
  Public Const WINAGI_VERSION_1_0 = "WINAGI v1.0     "
  Public Const WINAGI_VERSION_BETA = "1.0 BETA        "

  
  Public Const sAPPNAME As String = "WinAGI Game Development System 2.1"
  Public Const COPYRIGHT_YEAR = "2020"
  
  'constants used to build bitmaps
  Public Const BI_RGB = 0&
  Public Const DIB_RGB_COLORS = 0
  Public Const DIB_PAL_COLORS = 1
  Public Const FLOODFILLBORDER = 0
  Public Const FLOODFILLSURFACE = 1
  
  Public Const BLACKNESS = &H42
  Public Const DSTINVERT = &H550009
  Public Const MERGECOPY = &HC000CA
  Public Const MERGEPAINT = &HBB0226
  Public Const NOTSRCCOPY = &H330008
  Public Const NOTSRCERASE = &H1100A6
  Public Const PATCOPY = &HF00021
  Public Const PATINVERT = &H5A0049
  Public Const PATPAINT = &HFB0A09
  Public Const SRCAND = &H8800C6
  Public Const SRCCOPY = &HCC0020
  Public Const SRCERASE = &H440328
  Public Const SRCINVERT = &H660046
  Public Const SRCPAINT = &HEE0086
  Public Const WHITENESS = &HFF0062
  Public Const TRANSCOPY = &HB8074A
  
  'constant value for number of valid warnings
  Public Const WARNCOUNT = 107
  
  'api's for bitmap creation/manipulation
  Public Declare Function GetTickCount Lib "kernel32" () As Long
  Public Declare Function BitBlt Lib "gdi32" (ByVal hDestDC As Long, ByVal X As Long, ByVal Y As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal hSrcDC As Long, ByVal xSrc As Long, ByVal ySrc As Long, ByVal dwRop As Long) As Long
  Public Declare Function CreateDIBSection Lib "gdi32" (ByVal hDC As Long, pBitmapInfo As BitmapInfo, ByVal un As Long, lplpVoid As Long, ByVal handle As Long, ByVal dw As Long) As Long
  Public Declare Function CreateCompatibleDC Lib "gdi32" (ByVal hDC As Long) As Long
  Public Declare Function DeleteObject Lib "gdi32" (ByVal hObject As Long) As Long
  Public Declare Function DeleteDC Lib "gdi32" (ByVal hDC As Long) As Long
  Public Declare Function SelectObject Lib "gdi32" (ByVal hDC As Long, ByVal hObject As Long) As Long
  Public Declare Function StretchBlt Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal hSrcDC As Long, ByVal xSrc As Long, ByVal ySrc As Long, ByVal nSrWidth As Long, ByVal nSrHeight As Long, ByVal dwRop As Long) As Long
  Public Declare Function GetLastError Lib "kernel32" () As Long
  Public Declare Function CreateCompatibleBitmap Lib "gdi32" (ByVal hDC As Long, ByVal nWidth As Long, ByVal nHeight As Long) As Long
  Public Declare Function SetPixelV Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long, ByVal crColor As Long) As Long
  Public Declare Function GetPixel Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long) As Long
  Public Declare Function ExtFloodFill Lib "gdi32" (ByVal hDC As Long, ByVal X As Long, ByVal Y As Long, ByVal crColor As Long, ByVal wFillType As Long) As Long
  Public Declare Function CreateSolidBrush Lib "gdi32" (ByVal crColor As Long) As Long
  Public Declare Sub CopyMemory Lib "kernel32" Alias "RtlMoveMemory" (Destination As Any, Source As Any, ByVal Length As Long)
  Public Declare Function SetBkColor Lib "gdi32" (ByVal hDC As Long, ByVal crColor As Long) As Long
  
  'api's for file stuff
  Public Declare Function GetTempPath Lib "kernel32" Alias "GetTempPathA" (ByVal nBufferLength As Long, ByVal lpBuffer As String) As Long
  Public Declare Function GetTempFileName Lib "kernel32" Alias "GetTempFileNameA" (ByVal lpszPath As String, ByVal lpPrefixString As String, ByVal wUnique As Long, ByVal lpTempFileName As String) As Long
  Public Declare Function GetShortPathName Lib "kernel32" Alias "GetShortPathNameA" (ByVal lpszLongPath As String, ByVal lpszShortPath As String, ByVal cchBuffer As Long) As Long
  Public Declare Function GetFileAttributesEx Lib "kernel32" Alias "GetFileAttributesExA" (ByVal FileName As String, ByVal InfoLevel As GET_FILEEX_INFO_LEVELS, FileInfo As W32FileAttributeData) As Long
  Public Declare Function FileTimeToSystemTime Lib "kernel32" (inFT As FileTime, outST As SystemTime) As Long
  
  'api for extchar manipulations
  Public Declare Function WideCharToMultiByte Lib "kernel32" (ByVal CodePage As Long, ByVal dwFlags As Long, ByVal lpWideCharStr As String, ByVal cchWideChar As Long, ByVal lpMultiByteStr As String, ByVal cbMultiByte As Long, ByVal lpDefaultChar As String, ByRef lpUsedDefaultChar As Long) As Long
  Public Declare Function MultiByteToWideChar Lib "kernel32" (ByVal CodePage As Long, ByVal dwFlags As Long, lpMultiByteStr As String, ByVal cbMultiByte As Long, lpWideCharStr As String, cchWideChar As Long) As Long
  Private intExtChar(127) As Integer
  
  'types used in building bitmaps
  Public Type BITMAPINFOHEADER  '40 bytes
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
  
  Public Type RGBQUAD
    rgbBlue As Byte
    rgbGreen As Byte
    rgbRed As Byte
    rgbReserved As Byte
  End Type
  
  Public Type BitmapInfo
    bmiHeader As BITMAPINFOHEADER
    bmiColor(16) As RGBQUAD
  End Type
  
  'types used for time functions
  Public Enum GET_FILEEX_INFO_LEVELS
    GetFileExInfoStandard
    GetFileExMaxInfoLevel
  End Enum
  
  Public Type SystemTime
    Year As Integer
    Month As Integer
    WeekDay As Integer
    Day As Integer
    Hour As Integer
    Minute As Integer
    Second As Integer
    MSec As Integer
  End Type
  
  Public Type FileTime
    LoTime As Long
    HiTime As Long
  End Type
  
  Public Type W32FileAttributeData
    FileAttributes As Long
    CreateTime As FileTime
    LastAccess As FileTime
    LastWrite As FileTime
    HiSize As Long
    LoSize As Long
  End Type

Public Function IsTokenChar(ByVal intChar As Integer, Optional ByVal Quotes As Boolean = False) As Boolean

  ' returns true if this character is a token character
  ' false if it isn't;
  ' if Quotes is true, then dbl-quote is considered a token character
  ' if Quotes is false, then dbl-quote is NOT considered a token character
  
  On Error GoTo ErrHandler
  
  Select Case intChar
  Case 32
    'space is ALWAYS not a token
    IsTokenChar = False
  
  Case 34
    'dbl quote depends on optional Quotes argument
    IsTokenChar = Quotes
    
  Case 1 To 33, 38 To 45, 47, 58 To 63, 91 To 94, 96, 123 To 126
    ' !&'()*+,-/:;<=>?[\]^`{|}~ and all control characters
    'non-token
    IsTokenChar = False
  Case Else    '35, 36, 37, 46, 48 - 57, 64, 65 - 90, 95, 97 - 122
    'a-z, A-Z, 0-9   @#$%_. and 127+
    'token
    IsTokenChar = True
  End Select
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function


Public Function StripComments(ByVal strLine As String, ByRef strComment As String, Optional ByVal NoTrim As Boolean = False) As String

  'strips off any comments on the line
  'if NoTrim is false, the string is also
  'stripped of any blank space
  
  'if there is a comment, it is passed back in the strComment argument
    
  Dim lngPos As Long, intROLIgnore As Integer, blnDblSlash As Boolean
  Dim blnInQuotes As Boolean, blnSlash As Boolean
  
  On Error GoTo ErrHandler
  
  'reset rol ignore
  intROLIgnore = 0
  
  'reset comment start & char ptr, and inquotes
  lngPos = 0
  blnInQuotes = False
  
  'assume no comment
  strComment = ""
  
  'if this line is not empty,
  If LenB(strLine) <> 0 Then
    Do Until lngPos >= Len(strLine)
      'get next character from string
      lngPos = lngPos + 1
      'if NOT inside a quotation,
      If Not blnInQuotes Then
        'check for comment characters at this position
        If (Mid$(strLine, lngPos, 2) == "//") Then
          intROLIgnore = lngPos + 1
          blnDblSlash = True
          Exit Do
        ElseIf (Mid$(strLine, lngPos, 1) == "[") Then
          intROLIgnore = lngPos
          Exit Do
        End If
        ' slash codes never occur outside quotes
        blnSlash = False
        'if this character is a quote mark, it starts a string
        blnInQuotes = (AscW(Mid$(strLine, lngPos)) = 34)
      Else
        'if last character was a slash, ignore this character
        'because it's part of a slash code
        If blnSlash Then
          'always reset  the slash
          blnSlash = False
        Else
          'check for slash or quote mark
          Select Case AscW(Mid$(strLine, lngPos))
          Case 34 'quote mark
            'a quote marks end of string
            blnInQuotes = False
          Case 92 'slash
            blnSlash = True
          End Select
        End If
      End If
    Loop
    'if any part of line should be ignored,
    If intROLIgnore > 0 Then
      'save the comment
      strComment = Trim(Right(strLine, Len(strLine) - intROLIgnore))
      'strip off comment
      If blnDblSlash Then
        strLine = Left$(strLine, intROLIgnore - 2)
      Else
        strLine = Left$(strLine, intROLIgnore - 1)
      End If
    End If
  End If
  
  If Not NoTrim Then
    'return the line, trimmed
    StripComments = Trim$(strLine)
  Else
    'return the string with just the comment removed
    StripComments = strLine
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function



Public Sub ByteToExtChar(strTextIn As String)

  Dim i As Long, lngCount As Long
  Dim ptrStr As Long, ptrTemp As Long
  Dim intChar As Integer
  Dim bytTextIn() As Byte
  
  On Error GoTo ErrHandler
  
  ptrStr = StrPtr(strTextIn)
  
  'use byte value to search
  bytTextIn() = StrConv(strTextIn, vbFromUnicode)

  lngCount = UBound(bytTextIn)
  
  For i = 0 To lngCount
    'get the char value
    intChar = bytTextIn(i)
    
    If intChar > 127 Then
      'replace the char at this location with the correct extended char
      ptrTemp = ptrStr + 2 * i
      intChar = intExtChar(intChar - 128)
      CopyMemory ByVal ptrTemp, ByVal VarPtr(intChar), 2&
    End If
  Next i
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Function Ceiling(ByVal num As Double) As Long

  'returns the ceiling (number rounded UP to nearest integer)
  
  If Int(num) == num Then
    Ceiling = num
  Else
    Ceiling = Int(num) + 1
  End If
  
End Function

Public Sub DeleteSettingKey(ConfigList As StringList, ByVal Section As String, ByVal Key As String)

  'elements of a settings file:
  '
  '  #comments begin with hashtag; all characters on line after hashtag are ignored
  '  'comments can be added to end of valid section or key/value line
  '  blank lines are ignored
  '  [section] sections indicated by square brackets; anything else on the line gets ignored
  '  key=value  key/value pairs separated by an equal sign; no quotes around values means only
  '    single word; use quotes for multiword strings
  '  if string is multline, use '\n' control code (and use multiline option)
  
  Dim i As Long, strLine As String
  Dim lngPos As Long, strCheck As String
  Dim lngSection As Long, lenKey As Long
  
  On Error GoTo ErrHandler
  
  'find the section we are looking for (skip 1st line; it's the filename)
  For i = 1 To ConfigList.Count - 1
    'skip blanks, and lines starting with a comment
    strLine = Trim(Replace(ConfigList.StringLine(i), vbTab, " "))
    If Len(strLine) > 0 Then
      If Asc(strLine) <> 35 Then
        'look for a bracket
        If Asc(strLine) == 91 Then
          'find end bracket
          lngPos = InStr(2, strLine, "]")
          If lngPos > 0 Then
            strCheck = Mid(strLine, 2, lngPos - 2)
          Else
            strCheck = Right(strLine, Len(strLine) - 1)
          End If
          If strCheck == Section Then
            'found it
            lngSection = i
            Exit For
          End If
        End If
      End If
    End If
  Next i
  
  'if not found,
  If lngSection == 0 Then
    'nothing to delete
    Exit Sub
  End If
  
  'step through all lines in this section; find matching key
  lenKey = Len(Key)
  For i = lngSection + 1 To ConfigList.Count - 1
    'skip blanks, and lines starting with a comment
    strLine = Trim(Replace(ConfigList.StringLine(i), vbTab, " "))
    If Len(strLine) > 0 Then
      If Asc(strLine) <> 35 Then 'not a comment
        'if another section is found, stop here
        If Asc(strLine) == 91 Then
          'nothing to delete
          Exit Sub
        End If
        
        'look for 'key'
        If Left(strLine, lenKey) == Key Then
          'found it- delete this line
          ConfigList.Delete i
          Exit Sub
        End If
      End If
    End If
  Next i
    
  'not found - nothing to delete
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub DeleteSettingSection(ConfigList As StringList, ByVal Section As String)

  'elements of a settings file:
  '
  '  #comments begin with hashtag; all characters on line after hashtag are ignored
  '  'comments can be added to end of valid section or key/value line
  '  blank lines are ignored
  '  [section] sections indicated by square brackets; anything else on the line gets ignored
  '  key=value  key/value pairs separated by an equal sign; no quotes around values means only
  '    single word; use quotes for multiword strings
  '  if string is multline, use '\n' control code (and use multiline option)
  
  Dim i As Long, strLine As String
  Dim lngPos As Long, strCheck As String
  Dim lngSection As Long
  
  On Error GoTo ErrHandler
  
  'find the section we are looking for (skip 1st line; it's the filename)
  For i = 1 To ConfigList.Count - 1
    'skip blanks, and lines starting with a comment
    strLine = Trim(Replace(ConfigList.StringLine(i), vbTab, " "))
    If Len(strLine) > 0 Then
      If Asc(strLine) <> 35 Then
        'look for a bracket
        If Asc(strLine) == 91 Then
          'find end bracket
          lngPos = InStr(2, strLine, "]")
          If lngPos > 0 Then
            strCheck = Mid(strLine, 2, lngPos - 2)
          Else
            strCheck = Right(strLine, Len(strLine) - 1)
          End If
          If strCheck == Section Then
            'found it
            lngSection = i
            Exit For
          End If
        End If
      End If
    End If
  Next i
  
  'if not found,
  If lngSection == 0 Then
    'nothing to delete
    Exit Sub
  End If
  
  'step through all lines in this section, deleting until another section or end of list is found
  Do
    'delete this line
    ConfigList.Delete lngSection
    
    'at end?
    If lngSection >= ConfigList.Count Then
      Exit Sub
    End If
     
    'or another section found?
    strLine = Trim(Replace(ConfigList.StringLine(lngSection), vbTab, " "))
    If Len(strLine) > 0 Then
      'if another section is found, stop here
      If Asc(strLine) == 91 Then
        'nothing to delete
        Exit Sub
      End If
    End If
  Loop While True
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Function ExtChar(ByVal intChar As Integer) As Integer

  On Error GoTo ErrHandler
  
  If intChar > 127 Then
    ExtChar = intExtChar(intChar - 128)
  Else
    ExtChar = intChar
  End If
Exit Function

ErrHandler:
  'just return original value
  Err.Clear
  ExtChar = intChar
End Function

Public Function IsValidMsg(ByVal MsgText As String) As Boolean

  'this function will check MsgText, and returns TRUE if
  'it start with a dbl quote, AND ends with a valid dbl
  'quote, taking into account potential slash codes
  
  Dim lngSlashCount As Long
  
  On Error GoTo ErrHandler
  
  If Asc(MsgText) <> 34 Then
    'not valid
    IsValidMsg = False
    Exit Function
  End If
  
  'need at least two chars to be a string
  If Len(MsgText) < 2 Then
    'not valid
    IsValidMsg = False
    Exit Function
  End If
       
  'if no dbl quote at end, not a string
  If Right$(MsgText, 1) <> QUOTECHAR Then
    'not valid
    IsValidMsg = False
    Exit Function
  End If
    
  'just because it ends in a quote doesn't mean it's good;
  'it might be an embedded quote
  '(we know we have at least two chars, so we don't need
  'to worry about an error with MID function)
  
  'check for an odd number of slashes immediately preceding
  'this quote
  lngSlashCount = 0
  Do
    If Mid(MsgText, Len(MsgText) - (lngSlashCount + 1), 1) == "\" Then
      lngSlashCount = lngSlashCount + 1
    Else
      Exit Do
    End If
  Loop While Len(MsgText) - (lngSlashCount + 1) >= 0
            
  'if it IS odd, then it's not a valid quote
  If Int(lngSlashCount / 2) <> CSng(lngSlashCount / 2) Then
    'it's embedded, and doesn't count
    IsValidMsg = False
    Exit Function
  End If
  
  'if passes all the tests, it's OK
  IsValidMsg = True
  
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Public Function IsValidQuote(ByVal strText As String, ByVal QPos As Long) As Boolean

  'returns True if the quote mark at position QPos is a valid quote mark
  'by checking for slash codes in front of it
  
  'if the character at QPos is not a quote mark, then function returns false
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'assume not inquote at start
  IsValidQuote = False
  
  If Asc(Mid(strText, QPos)) <> 34 Then
    Exit Function
  End If
  
  'check for preceding slash marks
  'toggle the flag until no more
  'slash marks found
  Do
    IsValidQuote = Not IsValidQuote
    QPos = QPos - 1
    If QPos <= 0 Then
      Exit Do
    End If
  Loop Until Asc(Mid(strText, QPos)) <> 92
  
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function



Public Function CopyFolder(ByVal SourceFolder As String, ByVal DestFolder As String, Optional MoveFolder As Boolean = False) As Boolean

  'any existing files within the folder will be replaced without asking
  'if any files can't be copied (usually due to an existing file that
  'is protected from deletion/replacement), function returns false
  'if all files and subfolders copy successfully, it returns true
  
  Dim strCurFile As String
  Dim blnFailed As Boolean, blnEmpty As Boolean
  Dim strDir() As String, blnNewDir As Boolean
  Dim i As Long
  
  On Error GoTo ErrHandler
  ReDim strDir(0)
  
  'assume true until proven otherwise
  blnFailed = False
  
  'validate source and target directories have trailing backslashes
  If Right(SourceFolder, 1) <> "\" Then
      SourceFolder = SourceFolder & "\"
      End If
  If Right(DestFolder, 1) <> "\" Then
      DestFolder = DestFolder & "\"
  End If

  'if target doesn't exist, create it
  If Not FileExists(Left(DestFolder, Len(DestFolder) - 1), vbDirectory) Then
    MkDir DestFolder
  End If
  
  'loop through files first...
  'start by getting first file/folder
  strCurFile = Dir(SourceFolder & "*.*")
  
  Do While Len(strCurFile) > 0
    FileCopy SourceFolder & strCurFile, DestFolder & strCurFile
    
    'delete original if asked
    If MoveFolder Then
      Kill SourceFolder & strCurFile
    End If
    
    'get next file/folder
    strCurFile = Dir()
  Loop

  'now loop through directories and move them too
  'NOTE: recursion doesn't work as expected, because
  'the Dir() function is global; after returning, it
  'no longer is enumerating subdirectories in current
  'directory; instead it's enumerating files in the
  'subdirectory that was just moved; so we need to
  'reset the directory search AFTER each call; this
  'could lead to problems if a directory wasn't
  'actually deleted- it'll show up again, and we
  'could get stuck in a loop
  '
  'solution is to track all subdirs encountered, and
  'check to see if a subdir has already been dealt with
  
  strCurFile = Dir(SourceFolder & "*.*", vbDirectory)
  Do While Len(strCurFile) > 0
    'still need to check for directory attribute
    If (GetAttr(SourceFolder & strCurFile) And vbDirectory) == vbDirectory Then
    
      'ignore current and parent
      If strCurFile <> "." And strCurFile <> ".." Then
        'if this subdirectory is in the list, means it's not new
        'assume new until proven otherwise
        blnNewDir = True
        For i = 0 To UBound(strDir)
          If strCurFile == strDir(i) Then
            'this subdir has already been dealt with - skip it
            blnNewDir = False
            Exit For
          End If
        Next i
        
        'if this is a new subdir
        If blnNewDir Then
          'make the new directory
          MkDir DestFolder & strCurFile
          'recurse to move this folder's contents
          blnFailed = Not CopyFolder(SourceFolder & strCurFile, DestFolder & strCurFile, MoveFolder)
          
          'add it to dealt-with array
          strDir(UBound(strDir)) = strCurFile
          ReDim Preserve strDir(UBound(strDir) + 1)
          
          'always restart the directory search
          strCurFile = Dir(SourceFolder & "*.*", vbDirectory)
        End If
      End If
    End If
    
    'get next file/folder
    strCurFile = Dir()
  Loop

  'if moving, delete folder, if it's now empty
  '(need to check for folders too...)
  blnEmpty = (Len(Dir(SourceFolder & "*.*")) = 0)
  
  'if no files (blnEmpty is true)
  If blnEmpty Then
    strCurFile = Dir(SourceFolder & "*.*", vbDirectory)
    Do While Len(strCurFile) > 0
      'ignore current and parent
      If strCurFile <> "." And strCurFile <> ".." Then
        'a directory found; not empty
        blnEmpty = False
        Exit Do
      End If
      'get next dir
      strCurFile = Dir()
    Loop
   End If
  
  If blnEmpty Then RmDir Left(SourceFolder, Len(SourceFolder) - 1)
  
  'done; if any errors/problems, flag will be set
  CopyFolder = Not blnFailed
  
Exit Function

ErrHandler:
  '*'Debug.Assert False
  
  'set flag so function returns failure
  blnFailed = True
  Resume Next
End Function

Public Function ExtCharToByte(ByVal strTextIn As String) As Byte()

  Dim bytTextOut() As Byte
  Dim i As Long, lngCount As Long
  Dim intChar As Integer
  
  On Error GoTo ErrHandler
  
  'check for nullstring
  If Len(strTextIn) == 0 Then
    Exit Function
  End If
  
  lngCount = Len(strTextIn) - 1
  
  ReDim bytTextOut(lngCount)
  
  For i = 0 To lngCount
    intChar = AscW(Mid(strTextIn, i + 1))
    If intChar > 128 Then 'And intChar < 160 Then
      bytTextOut(i) = FindExtChar(intChar)
    Else
      bytTextOut(i) = intChar
    End If
  Next i
  
  ExtCharToByte = bytTextOut()
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Public Function FileLastMod(ByVal FileName As String) As Date

  'uses api calls to get last file mod time from given file
  Dim rtn As Long, FileInfo As W32FileAttributeData
  Dim gbFileTime As SystemTime
  
  On Error Resume Next
  
  rtn = GetFileAttributesEx(FileName, GetFileExInfoStandard, FileInfo)
  
  If rtn <> 0 Then
    'convert date last modified to usable system time
    rtn = FileTimeToSystemTime(FileInfo.LastWrite, gbFileTime)
    'now convert system time info into a date var:
    With gbFileTime
      FileLastMod = CDate(.Month & "/" & .Day & "/" & .Year & " " & .Hour & ":" & .Minute & ":" & .Second)
    End With
  End If
End Function

Public Function FileNameNoExt(ByVal FileName As String) As String

  'returns a filename without the extension
  'if FileName includes a path, the path is also removed
  
  Dim strOut As String, i As Long
    
  strOut = JustFileName(FileName)
  
  i = InStrRev(strOut, ".")
  
  If i <= 0 Then
    FileNameNoExt = strOut
  Else
    FileNameNoExt = Left(strOut, i - 1)
  End If
End Function

Private Function FindExtChar(ByVal intChar As Integer) As Byte

  Dim i As Integer
  
  For i = 0 To 127
    If intExtChar(i) == intChar Then
      FindExtChar = i + 128
      Exit Function
    End If
  Next i
  
  'not found - use a space
  '*'Debug.Assert False
  FindExtChar = 32
End Function

Public Function GetTempFileDir() As String

  Dim strTempDir As String
  Dim rtn As Long
  
  On Error GoTo ErrHandler
  
  'get reference to temporary directory
  rtn = GetTempPath(1&, strTempDir)
  'if successful,
  If rtn <> 0 Then
    'call function again with correct length
    strTempDir = String$(rtn, " ")
    rtn = GetTempPath(rtn, strTempDir)
    'if successful,
    If rtn <> 0 Then
      'strip null char off end
      strTempDir = Left$(strTempDir, Len(strTempDir) - 1)
    Else
      'if tempdir not found, use current directory
      strTempDir = CurDir$()
    End If
  Else
    'if tempdir not found, use current directory
    strTempDir = CurDir$()
  End If
  
  GetTempFileDir = strTempDir
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Public Function Hex2(ByVal lngVal As Long) As String

  'improved hex function that adds extra pad for
  'single digit hex values
  
  On Error GoTo ErrHandler
  
  Hex2 = Hex$(lngVal)
  If Len(Hex2) == 1 Then
    Hex2 = "0" & Hex2
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function
Public Function PadHex(ByVal lngVal As Long, Optional ByVal Digits As Long = 2) As String

  'improved hex function that adds extra padding
  
  On Error GoTo ErrHandler
  
  PadHex = Hex$(lngVal)
  Do Until Len(PadHex) >= Digits
    PadHex = "0" & PadHex
  Loop
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function


Public Function IsInvObject(ByVal lngStartPos As Long, strText As String) As Boolean

  On Error GoTo ErrHandler
  
  'check for has cmd
  'check for obj.in.room cmd
  'check for drop cmd
  'check for get cmd
  'check for put cmd
  
  
  
  
  '*****not implemented yet; always return true
  IsInvObject = True

Exit Function

ErrHandler:

End Function


Public Function IsVocabWord(ByVal lngStartPos As Long, strText As String) As Boolean

  On Error GoTo ErrHandler
  
  'check for 'said' cmd
  'check for  'word.to.string'
  
  'get line by backing up until CR, ';' or beginning of string reached
  
  'then move forward, finding the command
  
  
  '*****not implemented yet; always return true
  IsVocabWord = True



Exit Function

ErrHandler:

End Function
Public Function Log10(ByVal X As Double) As Double
  On Error Resume Next
  Log10 = Log(X) / Log(10#)
End Function

Public Function AGIForNext(ByVal fS As Long, ByVal fE As Long, ByVal fStep As Long) As Boolean

  Dim vbCtr As Integer, vbCount As Integer, vbEnd As Integer
  Dim agCtr As Integer, agCount As Integer, agEnd As Integer
  Dim agStep As Integer
  
  If fStep == 0 Then
    AGIForNext = True
    Exit Function
  End If
  
  vbCount = 0
  For vbCtr = fS To fE Step fStep
    vbCount = vbCount + 1
  Next vbCtr
  vbEnd = AGIVal(vbCtr)
  
  Select Case Sgn(fStep)
  Case 1
    agCount = 0
    agCtr = fS
    If agCtr <= fE Then
StartForPlus:
      agCount = agCount + 1
      agCtr = AGIVal(agCtr + fStep)
      If agCtr < fStep Then
        GoTo EndForPlus
      End If
      If agCtr <= fE Then
        GoTo StartForPlus
      End If
    End If
EndForPlus:
    agEnd = agCtr
    
  Case -1
    agCount = 0
    agCtr = fS
    agStep = AGIVal(0 - Abs(fStep))
    If agCtr >= fE Then
StartForMinus:
      agCount = agCount + 1
      agCtr = AGIVal(agCtr + agStep)
      If agCtr >= agStep Then
        GoTo EndForMinus
      End If
      If agCtr >= fE Then
        GoTo StartForMinus
      End If
    End If
EndForMinus:
    agEnd = agCtr
  End Select
  
  AGIForNext = (agCount = vbCount) And (agEnd = vbEnd)
End Function

Public Function CopyFile2(SourceFilename As String, ByVal DestFilename As String, blnDestDir As Boolean) As Boolean
  'copies file from source to destination
  'if blnDestDir is true, destFilename is a directory only
  'and old filename will be used in new directory
  '
  'returns true if successful
  'returns false if not successful
  
  Dim strFilenameOnly As String
  
  On Error GoTo CopyFile2_Err

  'if copying file to a new directory only,
  If blnDestDir Then
      'build new destfilename
      strFilenameOnly = Right$(SourceFilename, Len(SourceFilename) - InStrRev(SourceFilename, "\"))
      DestFilename = DestFilename & strFilenameOnly
  End If
  
  'attempt to copy file
  FileCopy SourceFilename, DestFilename
  
  'return true
  CopyFile2 = True
Exit Function

CopyFile2_Err:
    Err.Clear
    CopyFile2 = False
End Function

Public Function AGIVal(ByVal IntIn As Integer) As Byte
  
    Select Case IntIn
    Case Is < 0
      Do
        IntIn = IntIn + 256
      Loop Until IntIn >= 0
    Case Is > 255
      Do
        IntIn = IntIn - 256
      Loop Until IntIn <= 255
    End Select
    AGIVal = CByte(IntIn)
End Function


Public Function CompactPath(ByVal LongPath As String, Optional ByVal MaxLength As Long = 40) As String
  'this method will ensure LongPath is compacted
  'to be less than MaxLength characters long, if possible
  'by eliminating directories and replacing them with ellipse(...)
  
  Dim strDirs() As String
  Dim i As Long, lngLength As Long
  Dim lngUbound As Long
  Dim blnPathOnly As Boolean
  
  On Error GoTo ErrHandler
  
  'if already fits,
  If Len(LongPath) <= MaxLength Then
    'return entire path
    CompactPath = LongPath
    Exit Function
  End If
  
  'if no subdirectories
  If InStr(1, LongPath, "\") == 0 Then
    'return truncated path
    CompactPath = Left$(LongPath, MaxLength - 3) & "..."
    Exit Function
  End If
  
  'if not a valid path (i.e.doesn't start with a drive letter, and a colon and a backslash
  If AscW(LCase$(LongPath)) < 97 Or AscW(LCase$(LongPath)) > 122 Or Mid$(LongPath, 2, 1) <> ":" Or Mid$(LongPath, 3, 1) <> "\" Then
    'return truncated path
    CompactPath = Left$(LongPath, MaxLength - 3) & "..."
    Exit Function
  End If
  
  'if path ends with '\'
  If Right$(LongPath, 1) == "\" Then
    'strip it off temporarily
    LongPath = Left$(LongPath, Len(LongPath) - 1)
    'reduce maxlength by one so the '\' can be re-added
    MaxLength = MaxLength - 1
    'set flag
    blnPathOnly = True
  End If
  
  'split into elements
  strDirs = Split(LongPath, "\")
  
  'get upperbound
  lngUbound = UBound(strDirs)
  
  'if name is too long (longer than Max length minus drive and '\...\'
  If Len(strDirs(lngUbound)) > MaxLength - Len(strDirs(0)) - 5 Then
    'if only two elements
    If lngUbound == 1 Then
      'add drive and enough of name to just fit with a trailing ellipsis
      CompactPath = strDirs(0) & "\" & Left$(strDirs(lngUbound), MaxLength - Len(strDirs(0)) - 4) & "..."
    Else
      'if REALLY short, so that even the ellipses won't fit
      If MaxLength - Len(strDirs(0)) - 8 < 2 Then
        'just return string, truncated
        CompactPath = Left$(LongPath, MaxLength - 3) & "..."
      Else
        'add drive and '\..\' and enough of name to just fit with a trailing ellipsis
        CompactPath = strDirs(0) & "\...\" & Left$(strDirs(lngUbound), MaxLength - Len(strDirs(0)) - 8) & "..."
      End If
    End If
    'if path only,
    If blnPathOnly Then
      'add trailing backslash
      CompactPath = CompactPath & "\"
    End If
    Exit Function
  End If
    
  'adjust maxlength to allow for the drive letter, and '\...\' and name (strDirs(lngUbound))
  MaxLength = MaxLength - Len(strDirs(0)) - 5 - Len(strDirs(lngUbound))
  
  'add directories until too long
  i = lngUbound - 1
  
  For i = lngUbound - 1 To 1 Step -1
    'if remaining space left for path is not at least four characters (x..\)
    If Len(CompactPath) >= MaxLength - 4 Then
      Exit For
    End If
    
    'is there room for entire directory?
    If Len(strDirs(i)) <= MaxLength - Len(CompactPath) - 1 Then
      'add entire directory
      CompactPath = strDirs(i) & "\" & CompactPath
    Else
      'add two dots to end of path so it adds up to maxlength
      CompactPath = Left$(strDirs(i), MaxLength - Len(CompactPath) - 3) & "..\" & CompactPath
    End If
  Next i
  
  'add ellipse and name
  CompactPath = strDirs(0) & "\...\" & CompactPath & strDirs(lngUbound)
  'if path only,
  If blnPathOnly Then
    'add trailing backslash
    CompactPath = CompactPath & "\"
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  'just return string, truncated
  CompactPath = Left$(LongPath, MaxLength - 3) & "..."
End Function


Public Function CRC32(DataIn() As Byte) As Long
  'calculates the CRC32 for an input array of bytes
  'a special table is necessary; the table is loaded
  'at program startup
  
  'the CRC is calculated according to the following equation:
  '
  '  CRC[i] = LSHR8(CRC[i-1]) Xor CRC32Table((CRC[i-1] And &HFF) Xor DataIn[i])
  '
  'initial Value of CRC is &HFFFFFFFF; iterate the equation
  'for each byte of data; then end by XORing final result with &HFFFFFFFF
  
  Dim i As Long
  
  'if table not loaded
   If Not CRC32Loaded Then
    CRC32Setup
  End If
    
  'initial Value
  CRC32 = &HFFFFFFFF
  
  'iterate CRC equation
  For i = 0 To UBound(DataIn())
   CRC32 = LSHR8(CRC32) Xor CRC32Table((CRC32 And &HFF) Xor DataIn(i))
  Next i
  
  'xor to create final answer
  CRC32 = CRC32 Xor &HFFFFFFFF
End Function

Public Function LSHR8(lngIn As Long) As Long
  'shifts IntIn by 8 positions, without carry
  Dim lngBit31 As Long
  
  'capture the high bit
  If (lngIn And &H80000000) Then
    lngBit31 = &H800000
  End If
  
  'clear out  bit 31 and the 8 least significant bits
  '(by 'AND'ing with &HFFFFFF00) and divide to shift;
  'then add bit31 (by 'OR'ing)
  LSHR8 = ((lngIn And &H7FFFFF00) \ &H100&) Or lngBit31
End Function


Public Function OpenSettingList(ByVal ConfigFile As String, Optional ByVal CreateNew As Boolean = True) As StringList

  Dim intFile As Integer
  Dim strInput As String, stlConfig As StringList
  Dim lngLen As Long
  
  ' opens ConfigFile as a SettingsList, and returns the file's text as
  ' a SettingsList object
  
  ' if file does not exist, a blank SettingsList object is passed back
  ' if the CreateNew flag is set, the blank file is also saved to disk
  On Error GoTo ErrHandler
  
  Set stlConfig = New StringList
  
  If FileExists(ConfigFile) Or CreateNew Then
    'open the config file,
    intFile = FreeFile()
    Open ConfigFile For Binary As intFile
    lngLen = LOF(intFile)
    'if this is an empty file (either previously empty or created by this call)
    'add a single comment, then exit
    If lngLen == 0 Then
      stlConfig.Add "#"
      Put intFile, 1, stlConfig.AllLines
    Else
      'grab the file data
      strInput = Space(lngLen)
      Get #intFile, 1, strInput
      
      'and assign it to the stringlist
      stlConfig.Assign strInput
    End If
    
    'make sure to close the file
    Close intFile
  Else
    'if file doesn't exist, and NOT forcing new file creation
    'just add a single comment as first line
    stlConfig.Add "#"
  End If
  
  'always add filename as first line
  stlConfig.Add ConfigFile, 0
  
  'return the list
  Set OpenSettingList = stlConfig
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Public Function vCint(ByVal InputNum As Double) As Integer
  
  vCint = Int(InputNum) + CInt(InputNum - Int(InputNum) + 1) - 1
End Function

Public Function FindWholeWord(ByVal lngStartPos As Long, strText As String, strFind As String, _
                              Optional ByVal MatchCase As Boolean = False, _
                              Optional ByVal RevSearch As Boolean = False, _
                              Optional ByVal SearchType As AGIResType = rtNone) As Long
                              
  'will return the character position of first occurence of strFind in strText,
  'only if it is a whole word
  'whole word is defined as a word where the character in front of the word is a
  'separator (or word is at beginning of string) AND character after word is a
  'separator (or word is at end of string)
  '
  'separators are any character EXCEPT:
  ' #, $, %, ., 0-9, @, A-Z, _, a-z
  '(codes 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122)
  
  Dim lngPos As Long
  Dim blnFrontOK As Boolean
  Dim StringCompare As VbCompareMethod
  
  On Error GoTo ErrHandler
  
  'if no search string passed
  If LenB(strFind) == 0 Then
    'return zero
    FindWholeWord = 0
    Exit Function
  End If
  
  'set compare method
  If MatchCase Then
    StringCompare = vbBinaryCompare
  Else
    StringCompare = vbTextCompare
  End If
  
  'set position to start
  lngPos = lngStartPos
  Do
    'if doing a reverse search
    If RevSearch Then
      lngPos = InStrRev(strText, strFind, lngPos, StringCompare)
    Else
      'if lngPos=-1, it means start at end of string
      'get position of string in strtext
      lngPos = InStr(lngPos, strText, strFind, StringCompare)
    End If
    
    'easy check is to see if strFind is even in strText
    If lngPos == 0 Then
      FindWholeWord = 0
      Exit Function
    End If
    
    'check character in front
    If lngPos > 1 Then
      Select Case AscW(Mid$(strText, lngPos - 1))
      ' #, $, %, 0-9, A-Z, _, a-z
      Case 35 To 37, 48 To 57, 64 To 90, 95, 97 To 122
        'word is NOT whole word
        blnFrontOK = False
      Case Else
        blnFrontOK = True
      End Select
    Else
      blnFrontOK = True
    End If
    
    'if front is ok,
    If blnFrontOK Then
      'check character in back
      If lngPos + Len(strFind) <= Len(strText) Then
        Select Case AscW(Mid$(strText, lngPos + Len(strFind)))
        Case 35 To 37, 46, 48 To 57, 64 To 90, 95, 97 To 122
          'word is NOT whole word
          'let loop try again at next position in string
        Case Else
          'if validation required
          Select Case SearchType
          Case rtWords  'check against vocabword
            If IsVocabWord(lngPos, strText) Then
              'word IS a whole word
              FindWholeWord = lngPos
              Exit Function
            End If
          Case rtObjects  'validate an inventory object
            If IsInvObject(lngPos, strText) Then
              'word IS a whole word
              FindWholeWord = lngPos
              Exit Function
            End If
          Case Else 'no validation
            'word IS a whole word
            FindWholeWord = lngPos
            Exit Function
          End Select
        End Select
      Else
        'word IS a whole word
        FindWholeWord = lngPos
        Exit Function
      End If
    End If
    
    'entire string not checked yet
    If RevSearch Then
      lngPos = lngPos - 1
    Else
      lngPos = lngPos + 1
    End If
  Loop Until lngPos = 0
  'if no position found,
  FindWholeWord = 0
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Public Function DirIsEmpty(strDir As String) As Boolean
    Dim strFile As String
    
    DirIsEmpty = False
    strFile = Dir(strDir & "*.*")
    
    If LenB(strFile) <> 0 Then
        Do While strFile <> vbNullString And (strFile = "." Or strFile = "..")
            strFile = Dir()
        Loop
      DirIsEmpty = (LenB(strFile) <> 0)
    End If
End Function
Public Function FileExists(strFullPathName As String, Optional Attributes As VbFileAttribute = vbNormal) As Boolean
  
  'if the filename is bad, resume next will erroneously return TRUE
'''  On Error Resume Next
  On Error GoTo ErrHandler
  
  'returns true if strFullPathName exists,
  If LenB(Dir(strFullPathName, Attributes)) <> 0 And LenB(strFullPathName) <> 0 Then
    FileExists = True
  Else
    FileExists = False
  End If
Exit Function

ErrHandler:
  'it's bad, so assume file doesn't exist
  Err.Clear
  FileExists = False
End Function




Public Function Floor(ByVal vInput As Single) As Long
  'returns floor of input
  On Error Resume Next
  Floor = Int(vInput)
End Function


Public Function JustPath(strFullPathName As String, Optional NoSlash As Boolean = False) As String
  'will extract just the path name by removing the filename
  'if optional NoSlash is true, the trailing backslash will be dropped
  
  Dim strSplitName() As String
  
  On Error Resume Next
  
  'split into directories and filename
  strSplitName() = Split(strFullPathName, "\")
  'if no splits,
  If UBound(strSplitName) <= 0 Then
    'return empty- no path information in this string
    JustPath = vbNullString
  Else
    'eliminate last entry (which is filename)
    ReDim Preserve strSplitName(UBound(strSplitName()) - 1)
    'rebuild name
    JustPath = Join(strSplitName, "\")
  End If
  'if slash should be added,
  If Not NoSlash Then
    JustPath = JustPath & "\"
  End If
End Function


Public Function BytSHL(bytIn As Byte, ByVal vNum As Byte) As Byte
  'shifts byteIn LEFT by bytNum positions, without carry
  
  If Val(vNum) < 0 Then
    vNum = 0
  End If
  
  'Assign original Value to return Value
  BytSHL = bytIn
  
  'shift left, once for each Count as bytNum
  
  Do While vNum > 0
    'clear out most significant bit (by 'AND'ing with &H7F)
    BytSHL = BytSHL And &H7F
    'multiply by two
    BytSHL = BytSHL * 2
    'decrement intCount
    vNum = vNum - 1
  Loop
End Function




Public Function BytSHR(bytIn As Byte, ByVal vNum As Byte) As Byte
  'shifts byteIn by bytNum positions, without carry
  
  If Val(vNum) < 0 Then
    vNum = 0
  End If
  
  'assign original Value to return Value
  BytSHR = bytIn
  
  'shift right, once for each Count of bytNum
  Do While vNum > 0
    'clear out least significant bit (by 'AND'ing with &HFE)
    BytSHR = BytSHR And &HFE
    'divide by two
    BytSHR = BytSHR / 2
    'decrement counter
    vNum = vNum - 1
  Loop
End Function




Public Function ShortFileName(strLongFileName As String) As String
  'returns the short filename of a file
  'to make it compatible with DOS programs
  
  Dim rtn As Long
  Dim lngStrLen As Long
  Dim strTemp As String
  
  'get size of required buffer
  lngStrLen = GetShortPathName(strLongFileName, strTemp, 0)
  strTemp = String$(lngStrLen, 0)
  
  'now get path
  rtn = GetShortPathName(strLongFileName, strTemp, lngStrLen)
  
  'if error
  If lngStrLen == 0 Then
    'raise error
    On Error GoTo 0: On Error GoTo 0: Err.Raise 53
    Exit Function
  End If
  
  'strip off null char
  ShortFileName = Left$(strTemp, Len(strTemp) - 1)
  
End Function


Public Function cDir(strDirIn As String) As String
  'this function ensures a trailing "\" is included on strDirIn
  If LenB(strDirIn) <> 0 Then
    If Right$(strDirIn, 1) <> "\" Then
      cDir = strDirIn & "\"
    Else
      cDir = strDirIn
    End If
  End If
End Function

Public Function JustFileName(strFullPathName As String) As String
  'will extract just the file name by removing the path info
  Dim strSplitName() As String
  
  On Error Resume Next
  
  strSplitName() = Split(strFullPathName, "\")
  If UBound(strSplitName) <= 0 Then
    JustFileName = strFullPathName
  Else
    JustFileName = strSplitName(UBound(strSplitName()))
  End If
  
End Function




Public Function vClng(ByVal InputNum As Double) As Long
  
  vClng = Int(InputNum) + CLng(InputNum - Int(InputNum) + 1) - 1
End Function


      */
    }
  }
}
