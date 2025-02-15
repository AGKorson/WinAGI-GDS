using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Engine.Base;

// The Common namespace contains classes and member that are used by both
// the WinAGI Editor and the WinAGI Engine.

namespace WinAGI.Common {
    /// <summary>
    /// A class to provide access to Windows APIs needed to suppport 
    /// features in WinAGI that are not natively available in C#.
    /// </summary>
    public class API {

        // APIs for MIDI sound handling
        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int mciSendString(string lpszCommand, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn, IntPtr hwndCallback);
        public const int MM_MCINOTIFY = 0x3B9;
        public const int MCI_NOTIFY_SUCCESSFUL = 0x1;

        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int mciGetErrorString(int errNum, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        public const int WM_SETREDRAW = 11;
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
        public const string COPYRIGHT_YEAR = "2025";
        internal static uint[] CRC32Table = new uint[256];
        internal static bool CRC32Loaded;
        public static readonly char[] INVALID_DEFINE_CHARS;
        public static readonly char[] INVALID_FIRST_CHARS;
        public static readonly char[] INVALID_SIERRA_CHARS;
        public static readonly char[] INVALID_SIERRA_1ST_CHARS;
        public static readonly string s_INVALID_DEFINE_CHARS;
        public static readonly string s_INVALID_FIRST_CHARS;
        public static readonly string s_INVALID_SIERRA_CHARS;
        public static readonly string s_INVALID_SIERRA_1ST_CHARS;

        #endregion

        public enum EResListType {
            None,
            TreeList,
            ComboList
        }
        public enum AskOption {
            Ask,
            No,
            Yes
        }

        #region Constructors
        /// <summary>
        /// Constructor for the WinAGI.Common Base class.
        /// </summary>
        static Base() {
            // invalid DEFINE/ID characters: these, plus control chars and extended chars
            //        3       4         5         6         7         8         9         0         1         2
            //        234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
            //NOT OK  .!"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~x
            //ANY OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz
            //1ST OK                                   ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz
            INVALID_DEFINE_CHARS = " !\"&'()*+,-/:;<=>?[\\]^`{|}~".ToCharArray();
            INVALID_FIRST_CHARS = [.. INVALID_DEFINE_CHARS, .. "#$%.0123456789@".ToCharArray()];
            // sierra syntax allows ' / and ?
            // sierra syntax allows any valid symbol for first char except /
            INVALID_SIERRA_CHARS = " !\"&()*+,-:;<=>[\\]^`{|}~".ToCharArray();
            INVALID_SIERRA_1ST_CHARS = [.. INVALID_SIERRA_CHARS, .. "/".ToCharArray()];
            s_INVALID_DEFINE_CHARS = new string(INVALID_DEFINE_CHARS);
            s_INVALID_FIRST_CHARS = new string(INVALID_FIRST_CHARS);
            s_INVALID_SIERRA_CHARS = new string(INVALID_SIERRA_CHARS);
            s_INVALID_SIERRA_1ST_CHARS = new string(INVALID_SIERRA_1ST_CHARS);
        }
        #endregion

        #region Properties
        // none
        #endregion

        #region Methods

        /// <summary>
        /// Confirms that a directory has a terminating backslash, adding one if necessary.
        /// </summary>
        /// <param name="strDirIn"></param>
        /// <returns></returns>
        public static string FullDir(string strDirIn) {
            if (strDirIn.Length == 0) {
                return strDirIn;
            }
            else {
                return strDirIn.EndsWith('\\') ? strDirIn : strDirIn + '\\';
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
        /// Returns true if the specified string value represents a valid file name.
        /// </summary>
        /// <param name="checkname"></param>
        /// <returns></returns>
        public static bool IsValidFilename(string checkname) {
            // check for illegal file characters
            return checkname.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) == -1;
        }

        /// <summary>
        /// Calculates a CRC32 value for the specified byte array data stream.
        /// </summary>
        /// <param name="DataIn"></param>
        /// <returns></returns>
        public static uint CRC32(byte[] DataIn) {
            // system returns the hash as four-byte array
            byte[] hashval = System.IO.Hashing.Crc32.Hash(DataIn);
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
                throw new Exception("directory copy error");
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
                return LongPath.Left(MaxLength - 3) + "...";
            }
            int lngPos = LongPath.LastIndexOf('\\');
            // split into two strings
            strDir = LongPath[..lngPos];
            strFile = LongPath[(lngPos + 1)..];
            if (strFile.Length > MaxLength - 4) {
                // return truncated filename
                return strFile.Left(MaxLength - 3) + "...";
            }
            // truncate directory, pad with ... and return combined dir/filename
            return strDir.Left(MaxLength - 4) + "...\\" + strFile;
        }

        public static string CheckIncludes(string logicsource, AGIGame game, ref bool changed) {
            // For ingame logics only, this verifies the corrct include lines are
            // at the start of the file. They should be the first three lines of the
            // file, following any comment header.
            //
            // If the lines are not there and they should be, add them. If there
            // and not neeeded, remove them.
            int line, insertpos = -1, idpos = -1, reservedpos = -1, globalspos = -1;
            int badidpos = -1, badreservedpos = -1, badglobalspos = -1;
            changed = false;
            StringList src = [];
            src.Add(logicsource);
            string includefile;
            Regex goodinclude;
            Regex badinclude;

            for (line = 0; line < src.Count; line++) {
                if (insertpos < 0 && src[line].Trim().Left(1) != "[" && src[line].Trim().Left(2) != "//") {
                    insertpos = line;
                }
                if (src[line].Trim().Left(8) == "#include") {
                    // resourceids.txt
                    includefile = @"resourceids\.txt";
                    goodinclude = new Regex(@"#include\s*(?i)""" + includefile + @"""");
                    badinclude = new Regex(@"(#include|\binclude)\s*(?i)(""" + includefile + @"(?!"")\b|" + includefile + @"""|" + includefile + @"\b)");
                    if (idpos == -1 && goodinclude.Match(src[line]).Success) {
                        idpos = line;
                    }
                    else if (badidpos == -1 && badinclude.Match(src[line]).Success) {
                        badidpos = line;
                    }
                    // reserved.txt
                    includefile = @"reserved\.txt";
                    goodinclude = new Regex(@"#include\s*(?i)""" + includefile + @"""");
                    badinclude = new Regex(@"(#include|\binclude)\s*(?i)(""" + includefile + @"(?!"")\b|" + includefile + @"""|" + includefile + @"\b)");
                    if (reservedpos == -1 && goodinclude.Match(src[line]).Success) {
                        reservedpos = line;
                    }
                    else if (badreservedpos == -1 && badinclude.Match(src[line]).Success) {
                        badreservedpos = line;
                    }
                    // globals.txt
                    includefile = @"globals\.txt";
                    goodinclude = new Regex(@"#include\s*(?i)""" + includefile + @"""");
                    badinclude = new Regex(@"(#include|\binclude)\s*(?i)(""" + includefile + @"(?!"")\b|" + includefile + @"""|" + includefile + @"\b)");
                    if (globalspos == -1 && goodinclude.Match(src[line]).Success) {
                        globalspos = line;
                    }
                    else if (badglobalspos == -1 && badinclude.Match(src[line]).Success) {
                        badglobalspos = line;
                    }
                }
                if (idpos >= 0 && reservedpos >= 0 && globalspos >= 0) {
                    break;
                }
            }
            if (game.IncludeIDs) {
                if (idpos >= 0) {
                    if (idpos != insertpos) {
                        // move it
                        if (reservedpos >= 0 && reservedpos < idpos) reservedpos++;
                        if (badreservedpos >= 0 && badreservedpos < idpos) badreservedpos++;
                        if (globalspos >= 0 && globalspos < idpos) globalspos++;
                        if (badglobalspos >= 0 && badglobalspos < idpos) badglobalspos++;
                        src.Insert(insertpos, src[idpos]);
                        src.RemoveAt(++idpos);
                        changed = true;
                    }
                }
                else if (badidpos >= 0) {
                    // move it, but update to correct text
                    if (reservedpos >= 0 && reservedpos < idpos) reservedpos++;
                    if (badreservedpos >= 0 && badreservedpos < idpos) badreservedpos++;
                    if (globalspos >= 0 && globalspos < idpos) globalspos++;
                    if (badglobalspos >= 0 && badglobalspos < idpos) badglobalspos++;
                    src.Insert(insertpos, "#include \"resourceids.txt\"");
                    src.RemoveAt(++badidpos);
                    changed = true;
                }
                else {
                    // insert it
                    if (reservedpos >= 0) reservedpos++;
                    if (badreservedpos >= 0) reservedpos++;
                    if (globalspos >= 0) globalspos++;
                    if (badglobalspos >= 0) globalspos++;
                    src.Insert(insertpos, "#include \"resourceids.txt\"");
                    changed = true;
                }
                insertpos++;
            }
            else {
                if (idpos >= 0) {
                    if (reservedpos > idpos) reservedpos--;
                    if (badreservedpos > idpos) reservedpos--;
                    if (globalspos > idpos) globalspos--;
                    if (badglobalspos > idpos) globalspos--;
                    src.RemoveAt(idpos);
                    changed = true;
                }
                else if (badidpos >= 0) {
                    if (reservedpos > badidpos) reservedpos--;
                    if (badreservedpos > badidpos) reservedpos--;
                    if (globalspos > badidpos) globalspos--;
                    if (badglobalspos > badidpos) globalspos--;
                    src.RemoveAt(badidpos);
                    changed = true;
                }
            }
            if (game.IncludeReserved) {
                if (reservedpos >= 0) {
                    if (reservedpos != insertpos) {
                        // move it
                        if (globalspos >= 0 && globalspos < reservedpos) globalspos++;
                        if (badglobalspos >= 0 && badglobalspos < reservedpos) badglobalspos++;
                        src.Insert(insertpos, src[reservedpos]);
                        src.RemoveAt(++reservedpos);
                        changed = true;
                    }
                }
                else if (badreservedpos >= 0) {
                    // move it, but update to correct text
                    if (globalspos >= 0 && globalspos < badreservedpos) globalspos++;
                    if (badglobalspos >= 0 && badglobalspos < badreservedpos) badglobalspos++;
                    src.Insert(insertpos, "#include \"reserved.txt\"");
                    src.RemoveAt(++badreservedpos);
                    changed = true;
                }
                else {
                    // insert it
                    if (globalspos >= 0) globalspos++;
                    if (badglobalspos >= 0) globalspos++;
                    src.Insert(insertpos, "#include \"reserved.txt\"");
                    changed = true;
                }
                insertpos++;
            }
            else {
                if (reservedpos >= 0) {
                    if (globalspos > reservedpos) globalspos--;
                    if (badglobalspos > reservedpos) badglobalspos--;
                    src.RemoveAt(reservedpos);
                    changed = true;
                }
                else if (badreservedpos >= 0) {
                    if (globalspos > badreservedpos) globalspos--;
                    if (badglobalspos > badreservedpos) badglobalspos--;
                    src.RemoveAt(badreservedpos);
                    changed = true;
                }
            }
            if (game.IncludeGlobals) {
                if (globalspos >= 0) {
                    if (globalspos != insertpos) {
                        // move it
                        src.Insert(insertpos, src[globalspos]);
                        src.RemoveAt(++globalspos);
                        changed = true;
                    }
                }
                else if (badglobalspos >= 0) {
                    // move it, but update to correct text
                    src.Insert(insertpos, "#include \"globals.txt\"");
                    src.RemoveAt(++badglobalspos);
                    changed = true;
                }
                else {
                    // insert it
                    src.Insert(insertpos, "#include \"globals.txt\"");
                    changed = true;
                }
            }
            else {
                if (globalspos >= 0) {
                    src.RemoveAt(globalspos);
                    changed = true;
                }
                else if (badglobalspos >= 0) {
                    src.RemoveAt(badglobalspos);
                    changed = true;
                }
            }
            if (changed) {
                return string.Join(NEWLINE, [.. src]);
            }
            else {
                return logicsource;
            }
        }

        /// <summary>
        /// Strips off any comments on the line. If trimline is true, the string
        /// is also stripped of any leading or trailing blank space. If there is a
        /// comment, it is passed back in the comment argument.
        /// </summary>
        /// <param name="strLine"></param>
        /// <param name="strComment"></param>
        /// <param name="NoTrim"></param>
        /// <returns>The passed string with any comment stripped off.</returns>
        public static string StripComments(string line, ref string comment, bool trimline = true) {
            int lngPos = -1;
            bool blnInQuotes = false;
            bool blnSlash = false;
            bool blnDblSlash = false;
            int intROLIgnore = -1;

            if (line.Length == 0) {
                return "";
            }

            // assume no comment
            string strOut = line;
            comment = "";
            while (lngPos < line.Length - 1) {
                lngPos++;
                if (!blnInQuotes) {
                    // check for comment characters at this position
                    if (line.Mid(lngPos, 2) == "//") {
                        intROLIgnore = lngPos + 1;
                        blnDblSlash = true;
                        break;
                    }
                    else if (line.Substring(lngPos, 1) == "[") {
                        intROLIgnore = lngPos;
                        break;
                    }
                    // slash codes never occur outside quotes
                    blnSlash = false;
                    // if this character is a quote mark, it starts a string
                    blnInQuotes = line.ElementAt(lngPos) == '"';
                }
                else {
                    // if last character was a slash, ignore this character
                    // because it's part of a slash code
                    if (blnSlash) {
                        // always reset  the slash
                        blnSlash = false;
                    }
                    else {
                        // check for slash or quote mark
                        switch (line[lngPos]) {
                        case '"':
                            // a quote marks end of string
                            blnInQuotes = false;
                            break;
                        case '\\':
                            blnSlash = true;
                            break;
                        }
                    }
                }
            }
            if (intROLIgnore >= 0) {
                // save the comment
                comment = line[intROLIgnore..].Trim();
                // strip off the comment
                if (blnDblSlash) {
                    strOut = line[..(intROLIgnore - 1)];
                }
                else {
                    strOut = line[..intROLIgnore];
                }
            }
            if (trimline) {
                // return the line, trimmed
                strOut = strOut.Trim();
            }
            return strOut;
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
        /// Deletes the specified file. Ignores all errors including file missing,
        /// file access errors, permissions, etc.
        /// </summary>
        /// <param name="path"></param>
        public static void SafeFileDelete(string path) {
            try {
                File.Delete(path);
            }
            catch {
                // ignore errors
            }
        }

        /// <summary>
        /// Moves the specified file to a new location/name. Ignores all errors.
        /// </summary>
        /// <param name="oldpath"></param>
        /// <param name="newpath"></param>
        /// <param name="overwrite"></param>
        public static void SafeFileMove(string oldpath, string newpath, bool overwrite) {
            // caller should confirm oldpath exists, but check again just in case
            if (!File.Exists(oldpath)) {
                return;
            }
            try {
                File.Move(oldpath, newpath, overwrite);
            }
            catch {
                // ignore errors
            }
        }

        public static bool IsFontInstalled(string fontName) {
            bool installed = IsFontInstalled(fontName, FontStyle.Regular);
            if (!installed) { installed = IsFontInstalled(fontName, FontStyle.Bold); }
            if (!installed) { installed = IsFontInstalled(fontName, FontStyle.Italic); }
            return installed;
        }

        public static bool IsFontInstalled(string fontName, FontStyle style) {
            bool installed = false;
            const float emSize = 8.0f;
            try {
                using (var testFont = new Font(fontName, emSize, style)) {
                    installed = (0 == string.Compare(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase));
                }
            }
            catch {
            }
            return installed;
        }

        public static bool FontIsMonospace(FontFamily testfontfamily) {
            Font testfont = new Font(testfontfamily, 9.75f);
            return FontIsMonospace(testfont);
        }

        public static bool FontIsMonospace(Font testfont) {
            //check monospace font
            SizeF sizeM = GetCharSize(testfont, 'M');
            SizeF sizeDot = GetCharSize(testfont, '.');
            return sizeM == sizeDot;
        }

        public static SizeF GetCharSize(Font font, char c) {
            Size sz2 = TextRenderer.MeasureText("<" + c.ToString() + ">", font);
            Size sz3 = TextRenderer.MeasureText("<>", font);
            return new SizeF(sz2.Width - sz3.Width + 1, font.Height);
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
            stlText = SourceText.SplitLines();
            for (lngLine = 0; lngLine < stlText.Count; lngLine++) {
                Tpos = stlText[lngLine].IndexOf("TODO:");
                if (Tpos >= 0) {
                    Cpos = stlText[lngLine].LastIndexOf('[', Tpos);
                    if (Cpos >= 0) {
                        // get text between the comment and the TODO
                        strTODO = stlText[lngLine].Mid(Cpos + 1, Tpos - Cpos - 1);
                        // only valid if empty spaces
                        if (strTODO.Trim().Length == 0) {
                            // get comment portion of text
                            strTODO = stlText[lngLine][(Tpos + 5)..].Trim();
                            if (strTODO.Length > 0) {
                                // add this TODO (adjust line by 1)
                                TWinAGIEventInfo tmpInfo = new() {
                                    ID = "TODO",
                                    Line = lngLine.ToString(),
                                    Module = Module,
                                    Filename = "",
                                    ResNum = LogicNum,
                                    ResType = AGIResType.Logic,
                                    Text = strTODO,
                                    Type = EventType.TODO
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
            stlText = SourceText.SplitLines();
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
                                Line = lngLine.ToString(),
                                Module = Module,
                                Filename = "",
                                ResNum = LogicNum,
                                ResType = AGIResType.Logic,
                                Text = strDCWarn[6..],
                                Type = EventType.DecompWarning
                            };
                            retval.Add(tmpInfo);
                        }
                    }
                }
            }
            return retval;
        }

        /// <summary>
        /// Checks a define name to confirm it meets basic requirements (non-numeric,
        /// no invalid characters, etc.)
        /// </summary>
        /// <param name="checkname"></param>
        /// <param name="sierrasyntax"></param>
        /// <returns></returns>
        public static DefineNameCheck BaseNameCheck(string checkname, bool sierrasyntax = false) {
            // basic checks
            // if no name,
            if (checkname.Length == 0) {
                return DefineNameCheck.Empty;
            }
            // name can't be numeric
            if (checkname.IsNumeric()) {
                return DefineNameCheck.Numeric;
            }
            // check name against improper character list
            if (sierrasyntax) {
                if (INVALID_FIRST_CHARS.Any(ch => ch == checkname[0])) {
                    return DefineNameCheck.BadChar;
                }
                if (checkname[1..].Any(INVALID_DEFINE_CHARS.Contains)) {
                    return DefineNameCheck.BadChar;
                }
            }
            else {
                if (INVALID_SIERRA_1ST_CHARS.Any(ch => ch == checkname[0])) {
                    return DefineNameCheck.BadChar;
                }
                if (checkname[1..].Any(INVALID_SIERRA_CHARS.Contains)) {
                    return DefineNameCheck.BadChar;
                }
            }
            if (checkname.Any(ch => ch > 127 || ch < 32)) {
                return DefineNameCheck.BadChar;
            }
            // check against regular commands
            for (int i = 0; i < Commands.ActionCount; i++) {
                if (checkname == Commands.ActionCommands[i].Name) {
                    return DefineNameCheck.ActionCommand;
                }
            }
            // check against test commands
            // TODO: for sierra syntax, skip cmdname check?
            for (int i = 0; i < Commands.TestCount; i++) {
                if (checkname == Commands.TestCommands[i].Name) {
                    return DefineNameCheck.TestCommand;
                }
            }
            // check against compiler keywords
            if (checkname is "if" or "else" or "goto") {
                return DefineNameCheck.KeyWord;
            }
            // if the name starts with any of these letters
            // (OK for sierra syntax)
            if (!sierrasyntax) {
                if ("vfmoiswc".Any(checkname.StartsWith)) {
                    if (checkname.Right(checkname.Length - 1).IsNumeric()) {
                        // can't have a name that's a valid marker
                        return DefineNameCheck.ArgMarker;
                    }
                }
            }
            // if no error conditions, it's OK
            return DefineNameCheck.OK;
        }

        #endregion

    }

    public static class ExtensionMethods {
        /// <summary>
        /// Copies the source palette to an identical new palette.
        /// </summary>
        /// <param name="palette"></param>
        /// <returns></returns>
        public static EGAColors CopyPalette(this EGAColors palette) {
            EGAColors retval = new();
            for (int i = 0; i < 16; i++) {
                retval[i] = palette[i];
            }
            return retval;
        }

        /// <summary>
        /// Converts only ascii characters in a string to lower case. Extended
        /// characters are not adjusted.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string LowerAGI(this string strIn) {
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
        public static string UpperAGI(this string strIn) {
            StringBuilder sb = new(strIn);
            for (int i = 0; i < sb.Length; i++) {
                if (sb[i] >= 97 && sb[i] <= 122) {
                    sb[i] = (char)(sb[i] - 32);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// A safe version of SubString(0, len) that avoids exception if len > string.Length.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string strIn, int length) {
            if (length >= strIn.Length)
                return strIn;
            else
                return strIn[..length];
        }

        /// <summary>
        /// A safe version of SubString(string.Length-length, string.Length) that avoids
        /// exception if length > string.Length.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(this string strIn, int length) {
            if (length >= strIn.Length)
                return strIn;
            else
                return strIn[^length..];
        }

        /// <summary>
        /// A safe version of SubString that avoids exception if pos+length > string.Length
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="pos"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Mid(this string strIn, int pos, int length) {
            if (pos > strIn.Length || length <= 0) {
                return "";
            }
            if (pos + length > strIn.Length)
                return strIn[pos..];
            return strIn.Substring(pos, length);
        }

        /// <summary>
        /// Replaces the fist intance of a search string in the target string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string ReplaceFirst(this string text, string search, string replace) {
            int pos = text.IndexOf(search);
            if (pos < 0) {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        /// <summary>
        /// Replaces the first instance of a search string in the target string
        /// beginning at the specified position.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <param name="startpos"></param>
        /// <returns></returns>
        public static string ReplaceFirst(this string text, string search, string replace, int startpos) {
            int pos = text.IndexOf(search, startpos);
            if (pos < 0) {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        /// <summary>
        /// This method concatenates the specified string multiple times into a single string.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="NumCopies"></param>
        /// <returns></returns>
        public static string MultStr(this string strIn, int NumCopies) {
            return new StringBuilder(strIn.Length * NumCopies).Insert(0, strIn, NumCopies).ToString();
        }

        /// <summary>
        /// Determines if a string is numeric or not.
        /// </summary>
        /// <param name="str">string that may be a number</param>
        /// <returns>true if numeric, false if not</returns>
        public static bool IsNumeric(this string str) {
            if (double.TryParse(str, out _)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns numeric value of a string. If non-numeric,null, or empty
        /// it returns 0.
        /// </summary>
        /// <param name="strIn">The string that will be converted to a number</param>
        /// <returns>Returns a double value of strIn; if strIn can't be converted
        /// to a double, it returns 0.</returns>
        public static double Val(this string strIn) {
            if (double.TryParse(strIn, out double dResult)) {
                return dResult;
            }
            // not a valid number; return 0
            return 0;
        }

        public static int IntVal(this string strIn) {
            if (int.TryParse(strIn, out int dResult)) {
                return dResult;
            }
            // not a valid integer; return 0
            return 0;
        }

        /// <summary>
        /// Splits a string into an List of lines using all forms of line separators
        /// (CR, CRLF, LF).
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        internal static List<string> SplitLines(this string strText) {
            return strText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Cast<string>().ToList();
        }

        public static string SingleSpace(this string input) {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }
            // Use regular expression to replace all whitespace sequences with a single space
            return Regex.Replace(input, @"\s+", " ");
        }
    }

    /// <summary>
    /// An overload version of List<string> that detects multiple lines in the 
    /// Add method.
    /// </summary>
    public class StringList : List<string> {
        public string Text => string.Join(Environment.NewLine, this);

        public new void Add(string item) {
            // check for multiple lines in item
            if (item.Contains('\n') || item.Contains('\r')) {
                item = item.Replace("\r\n", "\n");
                item = item.Replace('\r', '\n');
                // split it
                string[] items = item.Split('\n');
                foreach (string subitem in items) {
                    base.Add(subitem);
                }
            }
            else {
                base.Add(item);
            }
        }
    }

}
