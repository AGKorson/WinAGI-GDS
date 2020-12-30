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
  
  If Int(num) = num Then
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
        If Asc(strLine) = 91 Then
          'find end bracket
          lngPos = InStr(2, strLine, "]")
          If lngPos > 0 Then
            strCheck = Mid(strLine, 2, lngPos - 2)
          Else
            strCheck = Right(strLine, Len(strLine) - 1)
          End If
          If strCheck = Section Then
            'found it
            lngSection = i
            Exit For
          End If
        End If
      End If
    End If
  Next i
  
  'if not found,
  If lngSection = 0 Then
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
        If Asc(strLine) = 91 Then
          'nothing to delete
          Exit Sub
        End If
        
        'look for 'key'
        If Left(strLine, lenKey) = Key Then
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
        If Asc(strLine) = 91 Then
          'find end bracket
          lngPos = InStr(2, strLine, "]")
          If lngPos > 0 Then
            strCheck = Mid(strLine, 2, lngPos - 2)
          Else
            strCheck = Right(strLine, Len(strLine) - 1)
          End If
          If strCheck = Section Then
            'found it
            lngSection = i
            Exit For
          End If
        End If
      End If
    End If
  Next i
  
  'if not found,
  If lngSection = 0 Then
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
      If Asc(strLine) = 91 Then
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
    If Mid(MsgText, Len(MsgText) - (lngSlashCount + 1), 1) = "\" Then
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


Private Function ReadAppSetting(ConfigList As StringList, ByVal Section As String, Key As String, Optional ByVal Default As String = "") As String

  'elements of a settings file:
  '
  '  #comments begin with hashtag; all characters on line after hashtag are ignored
  '  'comments can be added to end of valid section or key/value line
  '  blank lines are ignored
  '  [::BEGIN group::] marker to indicate a group of sections
  '  [::END group::] marker to indicate end of a group
  '  [section] sections indicated by square brackets; anything else on the line gets ignored
  '  key=value  key/value pairs separated by an equal sign; no quotes around values means only
  '    single word; use quotes for multiword strings
  '  if string is multline, use '\n' control code (and use multiline option)
  
  Dim i As Long, strLine As String
  Dim lngPos As Long, strCheck As String
  Dim lngSection As Long, lngLast As Long
  Dim lenKey As Long
  
  On Error GoTo ErrHandler
  
  'need to make sure there is a list to read from
  If ConfigList Is Nothing Then
    'return the default
    ReadAppSetting = Default
    Exit Function
  End If
  
  'find the section we are looking for (skip 1st line; it's the filename)
  For i = 1 To ConfigList.Count - 1
    'skip blanks, and lines starting with a comment
    strLine = Trim(Replace(ConfigList.StringLine(i), vbTab, " "))
    If Len(strLine) > 0 Then
      If Asc(strLine) <> 35 Then
        'look for a bracket
        If Asc(strLine) = 91 Then
          'find end bracket
          lngPos = InStr(2, strLine, "]")
          If lngPos > 0 Then
            strCheck = Mid(strLine, 2, lngPos - 2)
          Else
            strCheck = Right(strLine, Len(strLine) - 1)
          End If
          If StrComp(strCheck, Section, vbTextCompare) = 0 Then
            'found it
            lngSection = i
            Exit For
          End If
        End If
      End If
    End If
  Next i
  
  'if not found,
  If lngSection = 0 Then
    'add the section and the value
    WriteAppSetting ConfigList, Section, Key, Default
    'and return the default value
    ReadAppSetting = Default
    Exit Function
  Else
    'step through all lines in this section; find matching key
    lenKey = Len(Key)
    For i = lngSection + 1 To ConfigList.Count - 1
      'skip blanks, and lines starting with a comment
      strLine = Trim(Replace(ConfigList.StringLine(i), vbTab, " "))
      If Len(strLine) > 0 Then
        If Asc(strLine) <> 35 Then 'not a comment
          'if another section is found, stop here
          If Asc(strLine) = 91 Then
            Exit For
          End If
          
          'look for 'key'
          If StrComp(Left(strLine, lenKey), Key, vbTextCompare) = 0 And (Mid(strLine, lenKey + 1, 1) = " " Or Mid(strLine, lenKey + 1, 1) = "=") Then
            'validate that this is an exact match, and not a key that starts with
            'the same letters by verifying next char is either a space, or an equal sign
            
            'found it- extract value (if there is a comment on the end, drop it)
            'strip off key
            strLine = Trim(Right(strLine, Len(strLine) - lenKey))
            'check for nullstring, incase line has ONLY the key and nothing else
            If Len(strLine) > 0 Then
              'expect an equal sign
              If Asc(strLine) = 61 Then
                'remove it
                strLine = Trim(Right(strLine, Len(strLine) - 1))
              End If
                
              If Asc(strLine) = 34 Then
                'string delimiter; find ending delimiter
                lngPos = InStr(2, strLine, QUOTECHAR)
              Else
                'look for comment marker
                lngPos = InStr(2, strLine, "#")
              End If
              'no delimiter found; assume entire line
              If lngPos = 0 Then
                'adjust by one so last char doesn't get chopped off
                lngPos = Len(strLine) + 1
              End If
              'now strip off anything past value (including delimiter
              strLine = Trim(Left(strLine, lngPos - 1))
              If Len(strLine) > 0 Then
                'if in quotes, remove them
                If Asc(strLine) = 34 Then
                  strLine = Right(strLine, Len(strLine) - 1)
                End If
              End If
              'should never have an end quote; it will be caught as the ending delimiter
              If Len(strLine) > 0 Then
                If Asc(Right(strLine, 1)) = 34 Then
                  '*'Debug.Assert False
                  strLine = Left(strLine, Len(strLine) - 1)
                End If
              End If
          
              If InStr(1, strLine, "\n") > 0 Then
                'replace any newline control characters
                strLine = Replace(strLine, "\n", vbNewLine)
              End If
            End If
            ReadAppSetting = strLine
            Exit Function
          End If
        End If
      End If
    Next i
    
    'not found' add it here
    'back up until a nonblank line is found
    For lngPos = i - 1 To lngSection Step -1
      If Len(Trim(ConfigList.StringLine(lngPos))) > 0 Then
        Exit For
      End If
    Next lngPos
    
    'return the default value
    ReadAppSetting = Default
    
    'add the key and default value at this pos
    'if value contains spaces, it must be enclosed in quotes
    If InStr(1, Default, " ") > 0 Then
      If Asc(Default) <> 34 Then Default = QUOTECHAR & Default
      If Asc(Right(Default, 1)) <> 34 Then Default = Default & QUOTECHAR
    End If
  
    'if Default contains any carriage returns, replace them with control characters
    If InStr(1, Default, vbNewLine) Then
      Default = Replace(Default, vbNewLine, "\n")
    End If
    If InStr(1, Default, vbCr) Then
      Default = Replace(Default, vbCr, "\n")
    End If
    If InStr(1, Default, vbLf) Then
      Default = Replace(Default, vbLf, "\n")
    End If
    If Len(Default) = 0 Then
      Default = QUOTECHAR & QUOTECHAR
    End If
      
    ConfigList.Add "   " & Key & " = " & Default, lngPos + 1
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function


Public Function ReadSettingLong(ConfigList As StringList, ByVal Section As String, Key As String, Optional ByVal Default As Long = 0) As Long

  'get the setting value; if it converts to long value, use it;
  'if any kind of error, return the default value
  
  Dim strValue As String
  
  On Error GoTo ErrHandler
  
  strValue = ReadAppSetting(ConfigList, Section, Key, Default)
  If Len(strValue) = 0 Then
    ReadSettingLong = Default
  Else
    ReadSettingLong = CLng(strValue)
  End If
  Exit Function

ErrHandler:
  'clear the error, and return the default value
  '*'Debug.Assert False
  Err.Clear
  ReadSettingLong = Default
End Function

Public Function ReadSettingByte(ConfigList As StringList, ByVal Section As String, Key As String, Optional ByVal Default As Byte = 0) As Byte

  'get the setting value; if it converts to byte value, use it;
  'if any kind of error, return the default value
  
  Dim strValue As String
  
  On Error GoTo ErrHandler
  
  strValue = ReadAppSetting(ConfigList, Section, Key, Default)
  If Len(strValue) = 0 Then
    ReadSettingByte = Default
  Else
    ReadSettingByte = CByte(strValue)
  End If
  Exit Function

ErrHandler:
  'clear the error, and return the default value
  '*'Debug.Assert False
  Err.Clear
  ReadSettingByte = Default
End Function


Public Function ReadSettingSingle(ConfigList As StringList, ByVal Section As String, Key As String, Optional ByVal Default As Single = 0) As Single

  'get the setting value; if it converts to single value, use it;
  'if any kind of error, return the default value
  
  Dim strValue As String
  
  On Error GoTo ErrHandler
  
  strValue = ReadAppSetting(ConfigList, Section, Key, Default)
  If Len(strValue) = 0 Then
    ReadSettingSingle = Default
  Else
    ReadSettingSingle = CSng(strValue)
  End If
  
  Exit Function

ErrHandler:
  'clear the error, and return the default value
  '*'Debug.Assert False
  Err.Clear
  ReadSettingSingle = Default
End Function

Public Function ReadSettingBool(ConfigList As StringList, ByVal Section As String, Key As String, Optional ByVal Default As Boolean = False) As Boolean

  'get the setting value; if it converts to boolean value, use it;
  'if any kind of error, return the default value
  
  Dim strValue As String
  
  On Error GoTo ErrHandler
  
  strValue = ReadAppSetting(ConfigList, Section, Key, Default)
  If Len(strValue) = 0 Then
    ReadSettingBool = Default
  Else
    ReadSettingBool = CBool(strValue)
  End If
  Exit Function

ErrHandler:
  'clear the error, and return the default value
  '*'Debug.Assert False
  Err.Clear
  ReadSettingBool = Default
End Function

Public Function ReadSettingString(ConfigList As StringList, ByVal Section As String, Key As String, Optional ByVal Default As String = "") As String

  'read a string value from the configlist
  
  ReadSettingString = ReadAppSetting(ConfigList, Section, Key, Default)
End Function


Public Sub SaveSettingList(ConfigList As StringList)

  'filename is first line
  Dim strFileName As String
  Dim intFile As Integer, bytData() As Byte, TempFile As String
  
  On Error GoTo ErrHandler
  'get filename (and remove it; we don't need to save that)
  strFileName = ConfigList.StringLine(0)
  ConfigList.Delete 0
  
  'open temp file
  TempFile = TempFileName()
  intFile = FreeFile()
  Open TempFile For Binary As intFile
  
  'now output the results to the file
  Put intFile, 1, Join(ConfigList.AllLines, vbNewLine)
  'close the file
  Close intFile
  
  On Error Resume Next
  'kill the old file
  Kill strFileName
  Err.Clear
  'copy from tempfile
  FileCopy TempFile, strFileName
  'kill the tempfile
  Kill TempFile
  Err.Clear
  
  'add filename back
  ConfigList.Add strFileName, 0
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

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
  If Right(SourceFolder, 1) <> "\" Then SourceFolder = SourceFolder & "\"
  If Right(DestFolder, 1) <> "\" Then DestFolder = DestFolder & "\"
  
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
          If strCurFile = strDir(i) Then
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
  If Len(strTextIn) = 0 Then
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
    If intExtChar(i) = intChar Then
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
  If Len(Hex2) = 1 Then
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


Public Sub InitExtChars()

intExtChar(0) = 199
intExtChar(1) = 252
intExtChar(2) = 233
intExtChar(3) = 226
intExtChar(4) = 228
intExtChar(5) = 224
intExtChar(6) = 229
intExtChar(7) = 231
intExtChar(8) = 234
intExtChar(9) = 235
intExtChar(10) = 232
intExtChar(11) = 239
intExtChar(12) = 238
intExtChar(13) = 236
intExtChar(14) = 196
intExtChar(15) = 197
intExtChar(16) = 201
intExtChar(17) = 230
intExtChar(18) = 198
intExtChar(19) = 244
intExtChar(20) = 246
intExtChar(21) = 242
intExtChar(22) = 251
intExtChar(23) = 249
intExtChar(24) = 255
intExtChar(25) = 214
intExtChar(26) = 220
intExtChar(27) = 162
intExtChar(28) = 163
intExtChar(29) = 165
intExtChar(30) = 8359
intExtChar(31) = 402
intExtChar(32) = 225
intExtChar(33) = 237
intExtChar(34) = 243
intExtChar(35) = 250
intExtChar(36) = 241
intExtChar(37) = 209
intExtChar(38) = 170
intExtChar(39) = 186
intExtChar(40) = 191
intExtChar(41) = 8976
intExtChar(42) = 172
intExtChar(43) = 189
intExtChar(44) = 188
intExtChar(45) = 161
intExtChar(46) = 171
intExtChar(47) = 187
intExtChar(48) = 9617
intExtChar(49) = 9618
intExtChar(50) = 9619
intExtChar(51) = 9474
intExtChar(52) = 9508
intExtChar(53) = 9569
intExtChar(54) = 9570
intExtChar(55) = 9558
intExtChar(56) = 9557
intExtChar(57) = 9571
intExtChar(58) = 9553
intExtChar(59) = 9559
intExtChar(60) = 9565
intExtChar(61) = 9564
intExtChar(62) = 9563
intExtChar(63) = 9488
intExtChar(64) = 9492
intExtChar(65) = 9524
intExtChar(66) = 9516
intExtChar(67) = 9500
intExtChar(68) = 9472
intExtChar(69) = 9532
intExtChar(70) = 9566
intExtChar(71) = 9567
intExtChar(72) = 9562
intExtChar(73) = 9556
intExtChar(74) = 9577
intExtChar(75) = 9574
intExtChar(76) = 9568
intExtChar(77) = 9552
intExtChar(78) = 9580
intExtChar(79) = 9575
intExtChar(80) = 9576
intExtChar(81) = 9572
intExtChar(82) = 9573
intExtChar(83) = 9561
intExtChar(84) = 9560
intExtChar(85) = 9554
intExtChar(86) = 9555
intExtChar(87) = 9579
intExtChar(88) = 9578
intExtChar(89) = 9496
intExtChar(90) = 9484
intExtChar(91) = 9608
intExtChar(92) = 9604
intExtChar(93) = 9612
intExtChar(94) = 9616
intExtChar(95) = 9600
intExtChar(96) = 945
intExtChar(97) = 223
intExtChar(98) = 915
intExtChar(99) = 960
intExtChar(100) = 931
intExtChar(101) = 963
intExtChar(102) = 181
intExtChar(103) = 964
intExtChar(104) = 934
intExtChar(105) = 920
intExtChar(106) = 937
intExtChar(107) = 948
intExtChar(108) = 8734
intExtChar(109) = 966
intExtChar(110) = 949
intExtChar(111) = 8745
intExtChar(112) = 8801
intExtChar(113) = 177
intExtChar(114) = 8805
intExtChar(115) = 8804
intExtChar(116) = 8992
intExtChar(117) = 8993
intExtChar(118) = 247
intExtChar(119) = 8776
intExtChar(120) = 176
intExtChar(121) = 8729
intExtChar(122) = 183
intExtChar(123) = 8730
intExtChar(124) = 8319
intExtChar(125) = 178
intExtChar(126) = 9632
intExtChar(127) = 255
End Sub

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
  
  If fStep = 0 Then
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
  If InStr(1, LongPath, "\") = 0 Then
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
  If Right$(LongPath, 1) = "\" Then
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
    If lngUbound = 1 Then
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

Public Sub CRC32Setup()
  'build the CRC table
  
  CRC32Table(0) = &H0&
  CRC32Table(1) = &H77073096
  CRC32Table(2) = &HEE0E612C
  CRC32Table(3) = &H990951BA
  CRC32Table(4) = &H76DC419
  CRC32Table(5) = &H706AF48F
  CRC32Table(6) = &HE963A535
  CRC32Table(7) = &H9E6495A3
  CRC32Table(8) = &HEDB8832
  CRC32Table(9) = &H79DCB8A4
  CRC32Table(10) = &HE0D5E91E
  CRC32Table(11) = &H97D2D988
  CRC32Table(12) = &H9B64C2B
  CRC32Table(13) = &H7EB17CBD
  CRC32Table(14) = &HE7B82D07
  CRC32Table(15) = &H90BF1D91
  CRC32Table(16) = &H1DB71064
  CRC32Table(17) = &H6AB020F2
  CRC32Table(18) = &HF3B97148
  CRC32Table(19) = &H84BE41DE
  CRC32Table(20) = &H1ADAD47D
  CRC32Table(21) = &H6DDDE4EB
  CRC32Table(22) = &HF4D4B551
  CRC32Table(23) = &H83D385C7
  CRC32Table(24) = &H136C9856
  CRC32Table(25) = &H646BA8C0
  CRC32Table(26) = &HFD62F97A
  CRC32Table(27) = &H8A65C9EC
  CRC32Table(28) = &H14015C4F
  CRC32Table(29) = &H63066CD9
  CRC32Table(30) = &HFA0F3D63
  CRC32Table(31) = &H8D080DF5
  CRC32Table(32) = &H3B6E20C8
  CRC32Table(33) = &H4C69105E
  CRC32Table(34) = &HD56041E4
  CRC32Table(35) = &HA2677172
  CRC32Table(36) = &H3C03E4D1
  CRC32Table(37) = &H4B04D447
  CRC32Table(38) = &HD20D85FD
  CRC32Table(39) = &HA50AB56B
  CRC32Table(40) = &H35B5A8FA
  CRC32Table(41) = &H42B2986C
  CRC32Table(42) = &HDBBBC9D6
  CRC32Table(43) = &HACBCF940
  CRC32Table(44) = &H32D86CE3
  CRC32Table(45) = &H45DF5C75
  CRC32Table(46) = &HDCD60DCF
  CRC32Table(47) = &HABD13D59
  CRC32Table(48) = &H26D930AC
  CRC32Table(49) = &H51DE003A
  CRC32Table(50) = &HC8D75180
  CRC32Table(51) = &HBFD06116
  CRC32Table(52) = &H21B4F4B5
  CRC32Table(53) = &H56B3C423
  CRC32Table(54) = &HCFBA9599
  CRC32Table(55) = &HB8BDA50F
  CRC32Table(56) = &H2802B89E
  CRC32Table(57) = &H5F058808
  CRC32Table(58) = &HC60CD9B2
  CRC32Table(59) = &HB10BE924
  CRC32Table(60) = &H2F6F7C87
  CRC32Table(61) = &H58684C11
  CRC32Table(62) = &HC1611DAB
  CRC32Table(63) = &HB6662D3D
  CRC32Table(64) = &H76DC4190
  CRC32Table(65) = &H1DB7106
  CRC32Table(66) = &H98D220BC
  CRC32Table(67) = &HEFD5102A
  CRC32Table(68) = &H71B18589
  CRC32Table(69) = &H6B6B51F
  CRC32Table(70) = &H9FBFE4A5
  CRC32Table(71) = &HE8B8D433
  CRC32Table(72) = &H7807C9A2
  CRC32Table(73) = &HF00F934
  CRC32Table(74) = &H9609A88E
  CRC32Table(75) = &HE10E9818
  CRC32Table(76) = &H7F6A0DBB
  CRC32Table(77) = &H86D3D2D
  CRC32Table(78) = &H91646C97
  CRC32Table(79) = &HE6635C01
  CRC32Table(80) = &H6B6B51F4
  CRC32Table(81) = &H1C6C6162
  CRC32Table(82) = &H856530D8
  CRC32Table(83) = &HF262004E
  CRC32Table(84) = &H6C0695ED
  CRC32Table(85) = &H1B01A57B
  CRC32Table(86) = &H8208F4C1
  CRC32Table(87) = &HF50FC457
  CRC32Table(88) = &H65B0D9C6
  CRC32Table(89) = &H12B7E950
  CRC32Table(90) = &H8BBEB8EA
  CRC32Table(91) = &HFCB9887C
  CRC32Table(92) = &H62DD1DDF
  CRC32Table(93) = &H15DA2D49
  CRC32Table(94) = &H8CD37CF3
  CRC32Table(95) = &HFBD44C65
  CRC32Table(96) = &H4DB26158
  CRC32Table(97) = &H3AB551CE
  CRC32Table(98) = &HA3BC0074
  CRC32Table(99) = &HD4BB30E2
  CRC32Table(100) = &H4ADFA541
  CRC32Table(101) = &H3DD895D7
  CRC32Table(102) = &HA4D1C46D
  CRC32Table(103) = &HD3D6F4FB
  CRC32Table(104) = &H4369E96A
  CRC32Table(105) = &H346ED9FC
  CRC32Table(106) = &HAD678846
  CRC32Table(107) = &HDA60B8D0
  CRC32Table(108) = &H44042D73
  CRC32Table(109) = &H33031DE5
  CRC32Table(110) = &HAA0A4C5F
  CRC32Table(111) = &HDD0D7CC9
  CRC32Table(112) = &H5005713C
  CRC32Table(113) = &H270241AA
  CRC32Table(114) = &HBE0B1010
  CRC32Table(115) = &HC90C2086
  CRC32Table(116) = &H5768B525
  CRC32Table(117) = &H206F85B3
  CRC32Table(118) = &HB966D409
  CRC32Table(119) = &HCE61E49F
  CRC32Table(120) = &H5EDEF90E
  CRC32Table(121) = &H29D9C998
  CRC32Table(122) = &HB0D09822
  CRC32Table(123) = &HC7D7A8B4
  CRC32Table(124) = &H59B33D17
  CRC32Table(125) = &H2EB40D81
  CRC32Table(126) = &HB7BD5C3B
  CRC32Table(127) = &HC0BA6CAD
  CRC32Table(128) = &HEDB88320
  CRC32Table(129) = &H9ABFB3B6
  CRC32Table(130) = &H3B6E20C
  CRC32Table(131) = &H74B1D29A
  CRC32Table(132) = &HEAD54739
  CRC32Table(133) = &H9DD277AF
  CRC32Table(134) = &H4DB2615
  CRC32Table(135) = &H73DC1683
  CRC32Table(136) = &HE3630B12
  CRC32Table(137) = &H94643B84
  CRC32Table(138) = &HD6D6A3E
  CRC32Table(139) = &H7A6A5AA8
  CRC32Table(140) = &HE40ECF0B
  CRC32Table(141) = &H9309FF9D
  CRC32Table(142) = &HA00AE27
  CRC32Table(143) = &H7D079EB1
  CRC32Table(144) = &HF00F9344
  CRC32Table(145) = &H8708A3D2
  CRC32Table(146) = &H1E01F268
  CRC32Table(147) = &H6906C2FE
  CRC32Table(148) = &HF762575D
  CRC32Table(149) = &H806567CB
  CRC32Table(150) = &H196C3671
  CRC32Table(151) = &H6E6B06E7
  CRC32Table(152) = &HFED41B76
  CRC32Table(153) = &H89D32BE0
  CRC32Table(154) = &H10DA7A5A
  CRC32Table(155) = &H67DD4ACC
  CRC32Table(156) = &HF9B9DF6F
  CRC32Table(157) = &H8EBEEFF9
  CRC32Table(158) = &H17B7BE43
  CRC32Table(159) = &H60B08ED5
  CRC32Table(160) = &HD6D6A3E8
  CRC32Table(161) = &HA1D1937E
  CRC32Table(162) = &H38D8C2C4
  CRC32Table(163) = &H4FDFF252
  CRC32Table(164) = &HD1BB67F1
  CRC32Table(165) = &HA6BC5767
  CRC32Table(166) = &H3FB506DD
  CRC32Table(167) = &H48B2364B
  CRC32Table(168) = &HD80D2BDA
  CRC32Table(169) = &HAF0A1B4C
  CRC32Table(170) = &H36034AF6
  CRC32Table(171) = &H41047A60
  CRC32Table(172) = &HDF60EFC3
  CRC32Table(173) = &HA867DF55
  CRC32Table(174) = &H316E8EEF
  CRC32Table(175) = &H4669BE79
  CRC32Table(176) = &HCB61B38C
  CRC32Table(177) = &HBC66831A
  CRC32Table(178) = &H256FD2A0
  CRC32Table(179) = &H5268E236
  CRC32Table(180) = &HCC0C7795
  CRC32Table(181) = &HBB0B4703
  CRC32Table(182) = &H220216B9
  CRC32Table(183) = &H5505262F
  CRC32Table(184) = &HC5BA3BBE
  CRC32Table(185) = &HB2BD0B28
  CRC32Table(186) = &H2BB45A92
  CRC32Table(187) = &H5CB36A04
  CRC32Table(188) = &HC2D7FFA7
  CRC32Table(189) = &HB5D0CF31
  CRC32Table(190) = &H2CD99E8B
  CRC32Table(191) = &H5BDEAE1D
  CRC32Table(192) = &H9B64C2B0
  CRC32Table(193) = &HEC63F226
  CRC32Table(194) = &H756AA39C
  CRC32Table(195) = &H26D930A
  CRC32Table(196) = &H9C0906A9
  CRC32Table(197) = &HEB0E363F
  CRC32Table(198) = &H72076785
  CRC32Table(199) = &H5005713
  CRC32Table(200) = &H95BF4A82
  CRC32Table(201) = &HE2B87A14
  CRC32Table(202) = &H7BB12BAE
  CRC32Table(203) = &HCB61B38
  CRC32Table(204) = &H92D28E9B
  CRC32Table(205) = &HE5D5BE0D
  CRC32Table(206) = &H7CDCEFB7
  CRC32Table(207) = &HBDBDF21
  CRC32Table(208) = &H86D3D2D4
  CRC32Table(209) = &HF1D4E242
  CRC32Table(210) = &H68DDB3F8
  CRC32Table(211) = &H1FDA836E
  CRC32Table(212) = &H81BE16CD
  CRC32Table(213) = &HF6B9265B
  CRC32Table(214) = &H6FB077E1
  CRC32Table(215) = &H18B74777
  CRC32Table(216) = &H88085AE6
  CRC32Table(217) = &HFF0F6A70
  CRC32Table(218) = &H66063BCA
  CRC32Table(219) = &H11010B5C
  CRC32Table(220) = &H8F659EFF
  CRC32Table(221) = &HF862AE69
  CRC32Table(222) = &H616BFFD3
  CRC32Table(223) = &H166CCF45
  CRC32Table(224) = &HA00AE278
  CRC32Table(225) = &HD70DD2EE
  CRC32Table(226) = &H4E048354
  CRC32Table(227) = &H3903B3C2
  CRC32Table(228) = &HA7672661
  CRC32Table(229) = &HD06016F7
  CRC32Table(230) = &H4969474D
  CRC32Table(231) = &H3E6E77DB
  CRC32Table(232) = &HAED16A4A
  CRC32Table(233) = &HD9D65ADC
  CRC32Table(234) = &H40DF0B66
  CRC32Table(235) = &H37D83BF0
  CRC32Table(236) = &HA9BCAE53
  CRC32Table(237) = &HDEBB9EC5
  CRC32Table(238) = &H47B2CF7F
  CRC32Table(239) = &H30B5FFE9
  CRC32Table(240) = &HBDBDF21C
  CRC32Table(241) = &HCABAC28A
  CRC32Table(242) = &H53B39330
  CRC32Table(243) = &H24B4A3A6
  CRC32Table(244) = &HBAD03605
  CRC32Table(245) = &HCDD70693
  CRC32Table(246) = &H54DE5729
  CRC32Table(247) = &H23D967BF
  CRC32Table(248) = &HB3667A2E
  CRC32Table(249) = &HC4614AB8
  CRC32Table(250) = &H5D681B02
  CRC32Table(251) = &H2A6F2B94
  CRC32Table(252) = &HB40BBE37
  CRC32Table(253) = &HC30C8EA1
  CRC32Table(254) = &H5A05DF1B
  CRC32Table(255) = &H2D02EF8D
  
  'set flag
  CRC32Loaded = True
  
End Sub

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
    If lngLen = 0 Then
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

Public Sub WriteAppSetting(ConfigList As StringList, ByVal Section As String, ByVal Key As String, ByVal Value As String, Optional ByVal Group As String = "")

  'elements of a settings file:
  '
  '  #comments begin with hashtag; all characters on line after hashtag are ignored
  '  'comments can be added to end of valid section or key/value line
  '  blank lines are ignored
  '  [::BEGIN group::] marker to indicate a group of sections
  '  [::END group::]   marker to indicate end of a group
  '  [section]         sections indicated by square brackets; anything else on the line gets ignored
  '  key=value         key/value pairs separated by an equal sign; no quotes around values means only
  '                      single word; use quotes for multiword strings
  '  if string is multline, use '\n' control code (and use multiline option)
  
  Dim i As Long, strLine As String
  Dim lngPos As Long, strCheck As String
  Dim lngSectionStart As Long, lngSectionEnd As Long
  Dim lenKey As Long, blnFound As Boolean
  Dim lngGrpStart As Long, lngGrpEnd As Long
  Dim lngInsertLine As Long
  
  On Error GoTo ErrHandler
  
  'if value contains spaces, it must be enclosed in quotes
  If InStr(1, Value, " ") > 0 Then
    If Asc(Value) <> 34 Then Value = QUOTECHAR & Value
    If Asc(Right(Value, 1)) <> 34 Then Value = Value & QUOTECHAR
  End If

  'if value contains any carriage returns, replace them with control characters
  If InStr(1, Value, vbNewLine) Then
    Value = Replace(Value, vbNewLine, "\n")
  End If
  If InStr(1, Value, vbCr) Then
    Value = Replace(Value, vbCr, "\n")
  End If
  If InStr(1, Value, vbLf) Then
    Value = Replace(Value, vbLf, "\n")
  End If
  
  'if nullstring, include quotes
  If Len(Value) = 0 Then
    Value = QUOTECHAR & QUOTECHAR
  End If
  
  'if a group is provided, we will add new items within the group;
  'existing items, even if within the group, will be left where they are
  lngGrpStart = -1
  lngGrpEnd = -1
  lngPos = -1
  
  If Len(Group) > 0 Then
    '********** we will have to make adjustments to group start
    '           and end positions later on as we add lines
    '           during the key update! don't forget to do that!!
    For i = 1 To ConfigList.Count - 1
      'skip blanks, and lines starting with a comment
      strLine = Trim(Replace(ConfigList.StringLine(i), vbTab, " "))
      If Len(strLine) > 0 Then 'skip empty lines
        If Asc(strLine) <> 35 Then 'skip comments
          'if not found yet, look for the starting marker
          If Not blnFound Then
            'is this the group marker?
            If StrComp(strLine, "[::BEGIN " & Group & "::]") = 0 Then
              lngGrpStart = i
              blnFound = True
              'was the end found earlier? if so, we are done
              If lngGrpEnd >= 0 Then
                Exit For
              End If
            End If
          Else
            'start is found; make sure we find end before
            'finding another start
            If StrComp(Left(strLine, 9), "[::BEGIN ") = 0 Then
              'mark position of first new start, so we can move the end marker here
              If lngPos < 0 Then
                lngPos = i
              End If
            End If
          End If
          'we check for end marker here even if start not found
          'just in case they are backwards
          If StrComp(strLine, "[::END " & Group & "::]") = 0 Then
            lngGrpEnd = i
            'and if we also have a start, we can exit the loop
            If blnFound Then
              Exit For
            End If
          End If
        End If
      End If
    Next i
    'possible outcomes:
    ' - start and end both found; start before end
    '   this is what we want
    '
    ' - start and end both found, but end before start
    '   this is backwards; we fix by moving end to
    '   the line after start
    '
    ' - start found, but no end; we add end
    '   to just before next group start, or
    '   to end of file if no other group start
    '
    ' - end found, but no start; we fix by putting
    '   start right in front of end

    If lngGrpStart >= 0 And lngGrpEnd >= 0 Then
      'if backwards, move end to line after start
      If lngGrpEnd < lngGrpStart Then
        ConfigList.Add ConfigList(lngGrpEnd), lngGrpStart + 1
        ConfigList.Delete lngGrpEnd
        lngGrpStart = lngGrpStart - 1
        lngGrpEnd = lngGrpStart + 1
      End If
      
    ElseIf lngGrpStart >= 0 Then
      'means end not found
      'if there was another start found, insert end there
      If lngPos > 0 Then
        ConfigList.Add "[::END " & Group & "::]", lngPos
        ConfigList.Add "", lngPos + 1
        lngGrpEnd = lngPos
      Else
        'otherwise insert group end at end of file
        lngGrpEnd = ConfigList.Count
        ConfigList.Add "[::END " & Group & "::]"
      End If
    ElseIf lngGrpEnd >= 0 Then
      'means start not found
      'insert start in front of end
      lngGrpStart = lngGrpEnd
      ConfigList.Add "[::START " & Group & "::]", lngGrpStart
      lngGrpEnd = lngGrpStart + 1
    Else
      'means neither found
      'make sure at least one blank line
      If Len(Trim(ConfigList(ConfigList.Count - 1))) >= 0 Then
        ConfigList.Add ""
      End If
      lngGrpStart = ConfigList.Count
      ConfigList.Add "[::BEGIN " & Group & "::]"
      ConfigList.Add "[::END " & Group & "::]"
      lngGrpEnd = lngGrpStart + 1
    End If
  End If
  
  'reset the found flag
  blnFound = False
  
  'find the section we are looking for
  For i = 0 To ConfigList.Count - 1
    'skip blanks, and lines starting with a comment
    strLine = Trim(Replace(ConfigList.StringLine(i), vbTab, " "))
    If Len(strLine) > 0 Then 'skip empty lines
      If Asc(strLine) <> 35 Then 'skip comments
        'look for a bracket
        If Asc(strLine) = 91 Then
          'find end bracket
          lngPos = InStr(2, strLine, "]")
          If lngPos > 0 Then
            strCheck = Mid(strLine, 2, lngPos - 2)
          Else
            strCheck = Right(strLine, Len(strLine) - 1)
          End If
          If StrComp(strCheck, Section) = 0 Then
            'found it
            lngSectionStart = i
            Exit For
          End If
        End If
      End If
    End If
  Next i
  
  'if not found, create it at end of group (if group is provided)
  'otherwise at end of list
  If lngSectionStart = 0 Then
    If lngGrpStart >= 0 Then
     lngInsertLine = lngGrpEnd
    Else
      lngInsertLine = ConfigList.Count
    End If
    'make sure there is at least one blank line (unless this is first line in list)
    If lngInsertLine > 0 Then
      If Len(Trim(ConfigList.StringLine(lngInsertLine - 1))) <> 0 Then
        ConfigList.Add "", lngInsertLine
        lngInsertLine = lngInsertLine + 1
        lngGrpEnd = lngGrpEnd + 1
      End If
    End If
    ConfigList.Add "[" & Section & "]", lngInsertLine
    ConfigList.Add "   " & Key & " = " & Value, lngInsertLine + 1
    
    'no need to check for location of section within group;
    'we just added it to the group (if one is needed)
  Else
    'now step through all lines in this section; find matching key
    lenKey = Len(Key)
    For i = lngSectionStart + 1 To ConfigList.Count - 1
      'skip blanks, and lines starting with a comment
      strLine = Trim(Replace(ConfigList.StringLine(i), vbTab, " "))
      If Len(strLine) > 0 Then
        If Asc(strLine) <> 35 Then
          'if another section is found, stop here
          If Asc(strLine) = 91 Then
            'if part of a group; last line of the section
            'is line prior to the new section
            If lngGrpStart >= 0 Then
              lngSectionEnd = i - 1
            End If
            'if not already added, add it now
            If Not blnFound Then
              'back up until a nonblank line is found
              For lngPos = i - 1 To lngSectionStart Step -1
                If Len(Trim(ConfigList.StringLine(lngPos))) > 0 Then
                  Exit For
                End If
              Next lngPos
              'add the key and value at this pos
              ConfigList.Add "   " & Key & " = " & Value, lngPos + 1
              'this also bumps down the section end
              lngSectionEnd = lngSectionEnd + 1
              'it also may bump down group start/end
              If lngGrpStart >= lngPos + 1 Then
                lngGrpStart = lngGrpStart + 1
              End If
              If lngGrpEnd >= lngPos + 1 Then
                lngGrpEnd = lngGrpEnd + 1
              End If
            End If
            'we are done, but if part of a group
            'we need to verify the section is in
            'the group
            If lngGrpStart >= 0 Then
              blnFound = True
              Exit For
            Else
              Exit Sub
            End If
          End If
          
          'if not already found,  look for 'key'
          If Not blnFound Then
            If StrComp(Left(strLine, lenKey), Key, vbTextCompare) = 0 And (Mid(strLine, lenKey + 1, 1) = " " Or Mid(strLine, lenKey + 1, 1) = "=") Then
              'found it- change key value to match new value
              '(if there is a comment on the end, save it)
              strLine = Trim(Right(strLine, Len(strLine) - lenKey))
              If Len(strLine) > 0 Then
                'expect an equal sign
                If Asc(strLine) = 61 Then
                  'remove it
                  strLine = Trim(Right(strLine, Len(strLine) - 1))
                End If
              End If
              lngPos = 0
              If Len(strLine) > 0 Then
                If Asc(strLine) = 34 Then
                  'string delimiter; find ending delimiter
                  lngPos = InStr(2, strLine, QUOTECHAR)
                Else
                  'look for a space as a delimiter
                  lngPos = InStr(2, strLine, " ")
                End If
                If lngPos = 0 Then
                  'could be a case where a comment is at end of text, without a space
                  'if so we need to keep the delimiter
                  lngPos = InStr(2, strLine, "#") - 1
                End If
                'no delimiter found; assume entire line
                If lngPos <= 0 Then
                  'no adjustment; we want to keep delimiter and anything after
                  lngPos = Len(strLine)
                End If
                'now strip off the old value
                strLine = Trim(Right(strLine, Len(strLine) - lngPos))
              End If
              
              'if something left, maks sure it's a comment
              If Len(strLine) > 0 Then
                If Asc(strLine) <> 35 Then
                  strLine = "#" & strLine
                End If
                'make sure it starts with a space
                strLine = "   " & strLine
              End If
              
              strLine = "   " & Key & " = " & Value & strLine
              ConfigList.StringLine(i) = strLine
              'we are done, but if part of a group
              'we need to keep going to find end so
              'we can validate section is in the group
              If lngGrpStart >= 0 Then
                blnFound = True
              Else
                Exit Sub
              End If
            End If
          End If
        End If
      End If
    Next i
    
    'if not found (will only happen if this the last section in the
    'list, probably NOT in a group, but still possible (if the
    'section is outside the defined group)
    If Not blnFound Then
      'back up until a nonblank line is found
      For lngPos = i - 1 To lngSectionStart Step -1
        If Len(Trim(ConfigList.StringLine(lngPos))) > 0 Then
          Exit For
        End If
      Next lngPos
      'add the key and value at this pos
      ConfigList.Add "   " & Key & " = " & Value, lngPos + 1
      'we SHOULD be done, but just in case this section is
      'out of position, we still check for the group
      If lngGrpStart < 0 Then
        'no group - all done!
        Exit Sub
      End If
      'note that we don't need to bother adjusting group
      'start/end, because we only added a line to the
      'end of the file, and we know that the group
      'start/end markers MUST be before the start
      'of this section
    End If
    
    'found marker ONLY set if part a group so let's verify
    'the section is in the group, moving it if necessary
    '*'Debug.Assert lngGrpStart >= 0
    
    'if this was last section, AND section is NOT in its group
    'then then section end won't be set yet
    If lngSectionEnd <= 0 Then
      lngSectionEnd = ConfigList.Count - 1
    End If
    
    'if the section is not in the group, then move it
    '(depends on whether section is BEFORE or AFTER group start)
    If lngSectionStart < lngGrpStart Then
      'make sure at least one blank line above the group end
      If Len(Trim(ConfigList(lngGrpEnd - 1))) > 0 Then
        ConfigList.Add "", lngGrpEnd
        lngGrpEnd = lngGrpEnd + 1
      End If
      'add the section to end of group
      For i = lngSectionStart To lngSectionEnd
        ConfigList.Add ConfigList(i), lngGrpEnd
        lngGrpEnd = lngGrpEnd + 1
      Next i
      'then delete the section from it's current location
      For i = lngSectionStart To lngSectionEnd
        ConfigList.Delete lngSectionStart
      Next i
      
    ElseIf lngSectionStart > lngGrpEnd Then
      'make sure at least one blank line above the group end
      If Len(Trim(ConfigList(lngGrpEnd - 1))) > 0 Then
        ConfigList.Add "", lngGrpEnd
        lngGrpEnd = lngGrpEnd + 1
        lngSectionStart = lngSectionStart + 1
        lngSectionEnd = lngSectionEnd + 1
      End If
      'add the section to end of group
      For i = lngSectionEnd To lngSectionStart Step -1
        ConfigList.Add ConfigList(lngSectionEnd), lngGrpEnd
        'delete the line in current location
        ConfigList.Delete lngSectionEnd + 1
      Next i
    End If
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

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
  If LenB(strFind) = 0 Then
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
    If lngPos = 0 Then
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
  If lngStrLen = 0 Then
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
