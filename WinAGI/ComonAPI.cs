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

    //constant value for number of valid warnings
    internal const int WARNCOUNT = 107;

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
  }
}
