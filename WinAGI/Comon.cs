using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using WinAGI.Engine;
using static WinAGI.Engine.Base;

// The Common namespace contains classes and member that are used by both
// the WinAGI Editor and the WinAGI Engine.

namespace WinAGI.Common {
    /// <summary>
    /// A class to provide access to Windows APIs needed to suppport 
    /// features in WinAGI that are not natively available in C#.
    /// </summary>
    public static class API {

        // APIs for MIDI sound handling
        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int mciSendString(string lpszCommand, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn, IntPtr hwndCallback);
        public const int MM_MCINOTIFY = 0x3B9;
        public const int MCI_NOTIFY_SUCCESSFUL = 0x1;

        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int mciGetErrorString(int errNum, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn);


        // APIs for Help functions
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

    }
    
    /// <summary>
    /// Represents all WinAGI specific errors that occur while running the WinAGI Engine or
    /// WinAGI Editor.
    /// </summary>
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
            if (!value.Loaded) {
                ThrowResourceNotLoaded();
            }
        }
        public static void ThrowIfNotLoaded(WordList value) {
            if (!value.Loaded) {
                ThrowResourceNotLoaded();
            }
        }
        public static void ThrowIfNotLoaded(InventoryList value) {
            if (!value.Loaded) {
                ThrowResourceNotLoaded();
            }
        }

        [DoesNotReturn]
        private static void ThrowResourceNotLoaded() {
            WinAGIException wex = new("Resource not loaded") {
                HResult = WINAGI_ERR + 563,
            };
            throw wex;
        }
    }

    /// <summary>
    /// The base class for the WinAGI.Common namespace. Contains members
    /// and classes that are needed by both the Editor and the Engine.
    /// </summary>
    public static partial class Base {
        #region Local Members
        public const char QUOTECHAR = '\"';
        public static readonly string NEWLINE = Environment.NewLine;
        public const double LOG10_1_12 = 2.50858329719984E-02; // = Log10(2 ^ (1/12))
        public const string ARG1 = "%1";
        public const string ARG2 = "%2";
        public const string ARG3 = "%3";
        public const string sAPPNAME = "WinAGI Game Development System 3.0 alpha";
        public const string COPYRIGHT_YEAR = "2024";
        internal static uint[] CRC32Table = new uint[256];
        internal static bool CRC32Loaded;
        public static readonly char[] INVALID_ID_CHARS;
        public static readonly char[] INVALID_FIRST_CHARS;
        public static readonly char[] INVALID_DEFNAME_CHARS;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for the WinAGI.Common Base class.
        /// </summary>
        static Base() {
            // invalid ID characters: these, plus control chars and extended chars
            //        3       4         5         6         7         8         9         0         1         2
            //        234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
            //NOT OK  .!"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~x
            //    OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    
            INVALID_ID_CHARS = " !\"&'()*+,-/:;<=>?[\\]^`{|}~".ToCharArray();
            INVALID_FIRST_CHARS = (INVALID_ID_CHARS.ToString() + "#$%.0123456789@").ToCharArray();

            // invalid Define Name characters: these, plus control chars and extended chars
            //        3       4         5         6         7         8         9         0         1         2
            //        234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
            //NOT OK  .!"#$%&'()*+,-           :;<=> @                           [\]^ `                          {|}~x
            //    OK                ./0123456789    ?  ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    
            // sierra syntax allows ' ?
            // sierra syntax allows / for anything but first char
            INVALID_DEFNAME_CHARS = " !\"#$%&'()*+,-:;<=>?@[\\]^`{|}~".ToCharArray();
            //INVALID_DEFNAME_CHARS = CTRL_CHARS + " !\"#$%&'()*+,-:;<=>?@[\\]^`{|}~" + EXT_CHARS;
        }
        #endregion

        #region Properties
        // none
        #endregion

        #region Methods
        /// <summary>
        /// Error-safe method that provides an easy way to accss string resources by
        /// number instead of a string key.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string LoadResString(int index) {
            try {
                return EngineResources.ResourceManager.GetString(index.ToString());
            }
            catch (Exception) {
                // return nothing if string doesn't exist
                return "";
            }
        }

        /// <summary>
        /// Converts a unicode string the specified code page.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="enc"></param>
        /// <returns></returns>
        public static string UnicodeToCP(string strIn, Encoding enc) {
            return enc.GetString(Encoding.Unicode.GetBytes(strIn));
        }

        /// <summary>
        /// Converts a string from the specified codepage into Unicode.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="oldCP"></param>
        /// <returns></returns>
        public static string CPToUnicode(string strIn, Encoding oldCP) {
            return Encoding.Unicode.GetString(oldCP.GetBytes(strIn));
        }

        /// <summary>
        /// Converts only ascii characters in a string to lower case. Extended
        /// characters are not adjusted.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string LowerAGI(string strIn) {
            StringBuilder sb = new(strIn);
            for (int i = 0; i < sb.Length; i++) {
                if (sb[i] >= 65 && sb[i] <= 90) {
                    sb[i] = (char)(sb[i] + 32);
                }
            }
            return sb.ToString();  
        }

        /// <summary>
        /// Converts only ascii characters in a string to upper case. Extended
        /// characters are not adjusted.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string UpperAGI(string strIn) {
            StringBuilder sb = new(strIn);
            for (int i = 0; i < sb.Length; i++) {
                if (sb[i] >= 97 && sb[i] <= 122) {
                    sb[i] = (char)(sb[i] - 32);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// A safe version of SubString(string.Length-length, string.Length) that avoids
        /// exception if length > string.Length.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(string strIn, int length) {
            if (length >= strIn.Length)
                return strIn;
            else
                return strIn[^length..];
        }

        /// <summary>
        /// A safe version of SubString(0, len) that avoids exception if len > string.Length.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(string strIn, int length) {
            if (length >= strIn.Length)
                return strIn;
            else
                return strIn[..length];
        }

        /// <summary>
        /// A safe version of SubString that avoids exception if pos+length > string.Length
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="pos"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Mid(string strIn, int pos, int length) {
            if (pos + length > strIn.Length)
                return strIn[pos..];
            return strIn.Substring(pos, length);
        }

        /// <summary>
        /// This method concatenates the specified string multiple times into a single string.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="NumCopies"></param>
        /// <returns></returns>
        public static string MultStr(string strIn, int NumCopies) {
            return new StringBuilder(strIn.Length * NumCopies).Insert(0, strIn, NumCopies).ToString();
        }

        /// <summary>
        /// Extension method that works out if a string is numeric or not.
        /// </summary>
        /// <param name="str">string that may be a number</param>
        /// <returns>true if numeric, false if not</returns>
        public static bool IsNumeric(string str) {
            if (double.TryParse(str, out _)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Extension that uses TryParse to get value of a string, returning 0
        /// if the string is null, empty or non-numeric.
        /// </summary>
        /// <param name="strIn">The string that will be converted to a number</param>
        /// <returns>Returns a double value of strIn; if strIn can't be converted
        /// to a double, it returns 0.</returns>
        public static double Val(string strIn) {
            if (double.TryParse(strIn, out double dResult)) {
                return dResult;
            }
            // not a valid number; return 0
            return 0;
        }

        /// <summary>
        /// Confirms that a directory has a terminating backslash, adding one if necessary.
        /// </summary>
        /// <param name="strDirIn"></param>
        /// <returns></returns>
        public static string FullDir(string strDirIn) {
            if (strDirIn.Length != 0) {
                return !strDirIn.EndsWith('\\') ? strDirIn + '\\' : strDirIn;
            }
            else {
                return strDirIn;
            }
        }

        /// <summary>
        /// Extracts just the path name by removing the filename. If optional NoSlash
        /// is true, the trailing backslash will be dropped.
        /// </summary>
        /// <param name="fullPathName"></param>
        /// <param name="noSlash"></param>
        /// <returns></returns>
        public static string JustPath(string fullPathName, bool noSlash = false) {
            // not quite the same as Path.GetDirectoryName(fullPathName);
            // this method allows the return value to include the trailing
            // slash; the GetDirectoryName method doesn't. Also, if the path
            // consists only of a root and file (e.g. 'C:\file.txt')
            // GetDirectoryName returns null instead of 'C:'

            if (fullPathName.Length == 0) {
                return "";
            }
            // split into directories and filename
            string[] strSplitName = fullPathName.Split("\\");
            if (strSplitName.Length == 1) {
                // return empty- no path information in this string
                return "";
            }
            else {
                // eliminate last entry (which is filename)
                Array.Resize(ref strSplitName, strSplitName.Length - 1);
                string sReturn = string.Join("\\", strSplitName);
                if (!noSlash) {
                    sReturn += "\\";
                }
                return sReturn;
            }
        }

        /// <summary>
        /// Calculates a CRC32 value for the specified byte array data stream.
        /// </summary>
        /// <param name="DataIn"></param>
        /// <returns></returns>
        public static uint CRC32(byte[] DataIn) {
            // system returns the hash as four-byte array
            byte[]  hashval = System.IO.Hashing.Crc32.Hash(DataIn);
            // convert to 32 bit unsigned int
            return hashval[0] + ((uint)hashval[1] << 8) + ((uint)hashval[2] << 16) + ((uint)hashval[3] << 24);
        }

        /// <summary>
        /// Copies contents of source directory to target, and optionally, all 
        /// subdirectories and their contents.
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs) {
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
                        CopyDirectory(subdir.FullName, tempPath, copySubDirs);
                    }
                }
            }
            catch (Exception) {
                throw new  Exception("directory copy error");
            }
        }
        
        /// <summary>
        /// Compacts a full filename by eliminating directories and replacing
        /// them with ellipse(...)
        /// </summary>
        /// <param name="LongPath"></param>
        /// <param name="MaxLength"></param>
        /// <returns></returns>
        public static string CompactPath(string LongPath, int MaxLength = 40) {
            string strDir, strFile;

            if (LongPath.Length <= MaxLength) {
                return LongPath;
            }
            if (!LongPath.Contains('\\')) {
                // return truncated path
                return Left(LongPath, MaxLength - 3) + "...";
            }
            int lngPos = LongPath.LastIndexOf('\\');
            // split into two strings
            strDir = LongPath[..lngPos];
            strFile = LongPath[(lngPos + 1)..];
            if (strFile.Length > MaxLength - 4) {
                // return truncated filename
                return Left(strFile, MaxLength - 3) + "...";
            }
            // truncate directory, pad with ... and return combined dir/filename
            return Left(strDir, MaxLength - 4) + "...\\" + strFile;
        }

        /// <summary>
        /// Splits a string into an List of lines using all forms of line separators
        /// (CR, CRLF, LF).
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        internal static List<string> SplitLines(string strText) {
            List<string> retval = [.. strText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')];
            return retval;
        }

        /// <summary>
        /// This method converts a relative path into a fully qualified path, using
        /// the specified directory as the start.
        /// </summary>
        /// <param name="startdir"></param>
        /// <param name="relpath"></param>
        /// <returns></returns>
        public static string FullFileName(string startdir, string relpath) {
            try {
                return Path.GetFullPath(Path.Combine(startdir, relpath));
            }
            catch {
                // pass along errors
                throw;
            }
        }

        /// <summary>
        /// This method extracts 'TODO' entries from the specified logic and returns them
        /// in a list of WinAGI event info objects.
        /// </summary>
        /// <param name="LogicNum"></param>
        /// <param name="SourceText"></param>
        /// <param name="Module"></param>
        /// <returns></returns>
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
            if (!SourceText.Contains("TODO:")) {
                return retval;
            }
            stlText = SplitLines(SourceText);
            for (lngLine = 0; lngLine < stlText.Count; lngLine++) {
                Tpos = stlText[lngLine].IndexOf("TODO:");
                if (Tpos >= 0) {
                    Cpos = stlText[lngLine].LastIndexOf('[', Tpos);
                    if (Cpos >= 0) {
                        // get text between the comment and the TODO
                        strTODO = Mid(stlText[lngLine], Cpos + 1, Tpos - Cpos - 1);
                        // only valid if empty spaces
                        if (strTODO.Trim().Length == 0) {
                            // get comment portion of text
                            strTODO = stlText[lngLine][(Tpos + 5)..].Trim();
                            if (strTODO.Length > 0) {
                                // add this TODO (adjust line by 1)
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
                                retval.Add(tmpInfo);
                            }
                        }
                    }
                }
            }
            return retval;
        }

        /// <summary>
        /// This method extracts decompile warnings from the specified logic and returns
        /// them in a list of WinAGI event info objects.
        /// </summary>
        /// <param name="LogicNum"></param>
        /// <param name="SourceText"></param>
        /// <param name="Module"></param>
        /// <returns></returns>
        public static List<TWinAGIEventInfo> ExtractDecompWarn(byte LogicNum, string SourceText, string Module) {
            // look for [ WARNING DC##
            int Tpos, Cpos, lngLine;
            string strDCWarn;
            List<string> stlText;
            List<TWinAGIEventInfo> retval = [];

            // if warning marker isn't in the logic, just exit
            if (!SourceText.Contains("[ WARNING DC")) {
                return retval;
            }
            stlText = SplitLines(SourceText);
            for (lngLine = 0; lngLine < stlText.Count; lngLine++) {
                Tpos = stlText[lngLine].IndexOf("WARNING DC", StringComparison.OrdinalIgnoreCase);
                if (Tpos >= 0) {
                    Cpos = stlText[lngLine].LastIndexOf('[', Tpos);
                    if (Cpos >= 0) {
                        strDCWarn = stlText[lngLine][(Cpos + 9)..].Trim();
                        if (strDCWarn.Length > 0) {
                            // add this warning (adjust line by 1)
                            TWinAGIEventInfo tmpInfo = new() {
                                ID = strDCWarn[..4],
                                Line = (lngLine + 1).ToString(),
                                Module = Module,
                                ResNum = LogicNum,
                                ResType = AGIResType.rtLogic,
                                Text = strDCWarn[6..],
                                Type = EventType.etWarning
                            };
                            retval.Add(tmpInfo);
                        }
                    }
                }
            }
            return retval;
        }
        #endregion
    }
}
