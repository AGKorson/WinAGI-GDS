using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.frmPicEdit;
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
        public static extern int midiOutShortMsg(nint hMidiOut, int dwMsg);

        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int midiOutOpen(ref nint lphMidiOut, int uDeviceID, int dwCallback, int dwInstance, int dwFlags);

        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int midiOutClose(nint hMidiOut);

        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int midiOutReset(nint hMidiOut);

        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int mciSendString(string lpszCommand, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn, IntPtr hwndCallback);

        [DllImport("Winmm.dll", SetLastError = true)]
        public static extern int mciGetErrorString(int errNum, [MarshalAs(UnmanagedType.LPStr)] StringBuilder lpszReturnString, int cchReturn);


        public const int MM_MCINOTIFY = 0x3B9;
        public const int MCI_NOTIFY_SUCCESSFUL = 0x1;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, Int32 lParam);
        public const int WM_SETREDRAW = 11;

        // virtual key codes
        public const byte VK_INSERT = 0x2D; // Virtual key code for Insert
        public const uint KEYEVENTF_KEYUP = 0x0002; // Key up flag
        public const uint KEYEVENTF_KEYDOWN = 0x0000; // Key down flag
        public const uint INPUT_KEYBOARD = 1;
        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT {
            public uint type;
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
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
            WinAGIException wex = new(EngineResourceByNum(501)) {
                HResult = WINAGI_ERR + 501,
            };
            throw wex;
        }
    }

    /// <summary>
    /// The base class for the WinAGI.Common namespace. Contains members
    /// and classes that are needed by both the Editor and the Engine.
    /// </summary>
    public static partial class Base {
        #region Constants
        public const char QUOTECHAR = '\"';
        public const double LOG10_1_12 = 2.50858329719984E-02; // = Log10(2 ^ (1/12))
        public const string ARG1 = "%1";
        public const string ARG2 = "%2";
        public const string ARG3 = "%3";
        public const string sAPPNAME = "WinAGI Game Development System 3.0 alpha";
        public const string COPYRIGHT_YEAR = "2026";
        #endregion

        #region Enums
        public enum ResListType {
            None,
            TreeList,
            ComboList
        }
        public enum AskOption {
            Ask,
            No,
            Yes
        }
        #endregion

        #region Fields
        public static readonly string NEWLINE = Environment.NewLine;
        internal static uint[] CRC32Table = new uint[256];
        public static readonly char[] INVALID_DEFINE_CHARS;
        public static readonly char[] INVALID_FIRST_CHARS;
        public static readonly char[] INVALID_SIERRA_CHARS;
        public static readonly char[] INVALID_SIERRA_1ST_CHARS;
        public static readonly string s_INVALID_DEFINE_CHARS;
        public static readonly string s_INVALID_FIRST_CHARS;
        public static readonly string s_INVALID_SIERRA_CHARS;
        public static readonly string s_INVALID_SIERRA_1ST_CHARS;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for the WinAGI.Common Base class.
        /// </summary>
        static Base() {
            // invalid DEFINE/ID characters: these, plus control chars and extended chars
            //         3       4         5         6         7         8         9         0         1         2
            //         234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
            // NOT OK  x!"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~x
            // ANY OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz
            // 1ST OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz
            s_INVALID_DEFINE_CHARS = " !\"&'()*+,-/:;<=>?[\\]^`{|}~";
            INVALID_DEFINE_CHARS = s_INVALID_DEFINE_CHARS.ToCharArray();
            s_INVALID_FIRST_CHARS = s_INVALID_DEFINE_CHARS;
            INVALID_FIRST_CHARS = s_INVALID_FIRST_CHARS.ToCharArray();

            // sierra syntax allows ' / and ?
            // sierra syntax allows any valid symbol for first char except /
            s_INVALID_SIERRA_CHARS = " !\"&()*+,-:;<=>[\\]^`{|}~";
            INVALID_SIERRA_CHARS = s_INVALID_SIERRA_CHARS.ToCharArray();
            s_INVALID_SIERRA_1ST_CHARS = s_INVALID_SIERRA_CHARS + "/";
            INVALID_SIERRA_1ST_CHARS = s_INVALID_SIERRA_1ST_CHARS.ToCharArray();

        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if the specified string value represents a valid file name.
        /// </summary>
        /// <param name="checkname"></param>
        /// <returns></returns>
        public static bool IsValidFilename(string checkname) {
            // check for illegal file characters
            return checkname.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
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
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <param name="copySubDirs"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public static void CopyDirectory(string sourceDir, string destDir) {
            try {
                if (!Directory.Exists(sourceDir)) {
                    throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDir);
                }
                Directory.CreateDirectory(destDir);
                // copy files
                foreach (var file in Directory.GetFiles(sourceDir)) {
                    string destFile = Path.Combine(destDir, Path.GetFileName(file));
                    File.Copy(file, destFile, false);
                }
                // recurse to get subdirectories
                foreach (var subDir in Directory.GetDirectories(sourceDir)) {
                    string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                    CopyDirectory(subDir, destSubDir);
                }
            }
            catch (Exception) {
                WinAGIException wex = new(EngineResourceByNum(548).Replace(ARG1, destDir)) {
                    HResult = WINAGI_ERR + 548,
                };
                wex.Data["targetdir"] = destDir;
                throw wex;
            }
        }

        /// <summary>
        /// Compacts a full filename by eliminating directories and replacing
        /// them with ellipse(...)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string CompactPath(string path, int maxLength = 40) {
            string directory, filename;

            if (path.Length <= maxLength) {
                return path;
            }
            if (!path.Contains('\\')) {
                // return truncated path
                return path.Left(maxLength - 3) + "...";
            }
            int pos = path.LastIndexOf('\\');
            // split into two strings
            directory = path[..pos];
            filename = path[(pos + 1)..];
            if (filename.Length > maxLength - 4) {
                // return truncated filename
                return filename.Left(maxLength - 3) + "...";
            }
            // truncate directory, pad with ... and return combined dir/filename
            return directory.Left(maxLength - 4) + "...\\" + filename;
        }

        public static string RefreshAutoIncludes(string logicsource, AGIGame game, ref bool changed) {
            // For ingame logics only, this verifies the corrct include lines are
            // at the start of the file. They should be the first three lines of the
            // file, following any comment header.
            //
            // If the lines are not there and they should be, add them. If there
            // and not neeeded, remove them.
            //
            // only fan syntax uses auto includes
            if (game.SierraSyntax) {
                return logicsource;
            }

            int line, insertpos = -1, idpos = -1, reservedpos = -1, globalspos = -1;
            int badidpos = -1, badreservedpos = -1, badglobalspos = -1;
            changed = false;
            List<string> src = [];
            src.AddLines(logicsource);
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
                        if (reservedpos >= 0 && reservedpos < idpos)
                            reservedpos++;
                        if (badreservedpos >= 0 && badreservedpos < idpos)
                            badreservedpos++;
                        if (globalspos >= 0 && globalspos < idpos)
                            globalspos++;
                        if (badglobalspos >= 0 && badglobalspos < idpos)
                            badglobalspos++;
                        src.Insert(insertpos, src[idpos]);
                        src.RemoveAt(++idpos);
                        changed = true;
                    }
                }
                else if (badidpos >= 0) {
                    // move it, but update to correct text
                    if (reservedpos >= 0 && reservedpos < idpos)
                        reservedpos++;
                    if (badreservedpos >= 0 && badreservedpos < idpos)
                        badreservedpos++;
                    if (globalspos >= 0 && globalspos < idpos)
                        globalspos++;
                    if (badglobalspos >= 0 && badglobalspos < idpos)
                        badglobalspos++;
                    src.Insert(insertpos, "#include \"resourceids.txt\"");
                    src.RemoveAt(++badidpos);
                    changed = true;
                }
                else {
                    // insert it
                    if (reservedpos >= 0)
                        reservedpos++;
                    if (badreservedpos >= 0)
                        reservedpos++;
                    if (globalspos >= 0)
                        globalspos++;
                    if (badglobalspos >= 0)
                        globalspos++;
                    src.Insert(insertpos, "#include \"resourceids.txt\"");
                    changed = true;
                }
                insertpos++;
            }
            else {
                if (idpos >= 0) {
                    if (reservedpos > idpos)
                        reservedpos--;
                    if (badreservedpos > idpos)
                        reservedpos--;
                    if (globalspos > idpos)
                        globalspos--;
                    if (badglobalspos > idpos)
                        globalspos--;
                    src.RemoveAt(idpos);
                    changed = true;
                }
                else if (badidpos >= 0) {
                    if (reservedpos > badidpos)
                        reservedpos--;
                    if (badreservedpos > badidpos)
                        reservedpos--;
                    if (globalspos > badidpos)
                        globalspos--;
                    if (badglobalspos > badidpos)
                        globalspos--;
                    src.RemoveAt(badidpos);
                    changed = true;
                }
            }
            if (game.IncludeReserved) {
                if (reservedpos >= 0) {
                    if (reservedpos != insertpos) {
                        // move it
                        if (globalspos >= 0 && globalspos < reservedpos)
                            globalspos++;
                        if (badglobalspos >= 0 && badglobalspos < reservedpos)
                            badglobalspos++;
                        src.Insert(insertpos, src[reservedpos]);
                        src.RemoveAt(++reservedpos);
                        changed = true;
                    }
                }
                else if (badreservedpos >= 0) {
                    // move it, but update to correct text
                    if (globalspos >= 0 && globalspos < badreservedpos)
                        globalspos++;
                    if (badglobalspos >= 0 && badglobalspos < badreservedpos)
                        badglobalspos++;
                    src.Insert(insertpos, "#include \"reserved.txt\"");
                    src.RemoveAt(++badreservedpos);
                    changed = true;
                }
                else {
                    // insert it
                    if (globalspos >= 0)
                        globalspos++;
                    if (badglobalspos >= 0)
                        globalspos++;
                    src.Insert(insertpos, "#include \"reserved.txt\"");
                    changed = true;
                }
                insertpos++;
            }
            else {
                if (reservedpos >= 0) {
                    if (globalspos > reservedpos)
                        globalspos--;
                    if (badglobalspos > reservedpos)
                        badglobalspos--;
                    src.RemoveAt(reservedpos);
                    changed = true;
                }
                else if (badreservedpos >= 0) {
                    if (globalspos > badreservedpos)
                        globalspos--;
                    if (badglobalspos > badreservedpos)
                        badglobalspos--;
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
            int pos = -1;
            bool inQuotes = false;
            bool slash = false;
            bool dblSlash = false;
            int rolIgnore = -1;

            if (line.Length == 0) {
                return "";
            }

            // assume no comment
            string retval = line;
            comment = "";
            while (pos < line.Length - 1) {
                pos++;
                if (!inQuotes) {
                    // check for comment characters at this position
                    if (line.Mid(pos, 2) == "//") {
                        rolIgnore = pos + 1;
                        dblSlash = true;
                        break;
                    }
                    else if (line.Substring(pos, 1) == "[") {
                        rolIgnore = pos;
                        break;
                    }
                    // slash codes never occur outside quotes
                    slash = false;
                    // if this character is a quote mark, it starts a string
                    inQuotes = line.ElementAt(pos) == '"';
                }
                else {
                    // if last character was a slash, ignore this character
                    // because it's part of a slash code
                    if (slash) {
                        // always reset  the slash
                        slash = false;
                    }
                    else {
                        // check for slash or quote mark
                        switch (line[pos]) {
                        case '"':
                            // a quote marks end of string
                            inQuotes = false;
                            break;
                        case '\\':
                            slash = true;
                            break;
                        }
                    }
                }
            }
            if (rolIgnore >= 0) {
                // save the comment
                comment = line[rolIgnore..].Trim();
                // strip off the comment
                if (dblSlash) {
                    retval = line[..(rolIgnore - 1)];
                }
                else {
                    retval = line[..rolIgnore];
                }
            }
            if (trimline) {
                // return the line, trimmed
                retval = retval.Trim();
            }
            return retval;
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
            if (!installed) {
                installed = IsFontInstalled(fontName, FontStyle.Bold);
            }
            if (!installed) {
                installed = IsFontInstalled(fontName, FontStyle.Italic);
            }
            return installed;
        }

        public static bool IsFontInstalled(string fontName, FontStyle style) {
            bool installed = false;
            const float emSize = 8.0f;
            try {
                using var testFont = new Font(fontName, emSize, style);
                installed = string.Compare(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase) == 0;
            }
            catch {
            }
            return installed;
        }

        public static bool FontIsMonospace(FontFamily testfontfamily) {
            Font testfont = new(testfontfamily, 10, FontStyle.Regular);
            return FontIsMonospace(testfont);
        }

        public static bool FontIsMonospace(Font testfont) {
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
        public static List<WinAGIEventInfo> ExtractTODO(byte LogicNum, string SourceText, string Module) {
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
            int todoPos, linenum, commentPos;
            string todoText;
            List<string> lines;
            List<WinAGIEventInfo> retval = [];

            // if TODO marker isn't in the logic, just exit
            if (!SourceText.Contains("TODO:")) {
                return retval;
            }
            lines = SourceText.SplitLines();
            for (linenum = 0; linenum < lines.Count; linenum++) {
                todoPos = lines[linenum].IndexOf("TODO:");
                if (todoPos >= 0) {
                    commentPos = lines[linenum].LastIndexOf('[', todoPos);
                    if (commentPos >= 0) {
                        // get text between the comment and the TODO
                        todoText = lines[linenum].Mid(commentPos + 1, todoPos - commentPos - 1);
                        // only valid if empty spaces
                        if (todoText.Trim().Length == 0) {
                            // get comment portion of text
                            todoText = lines[linenum][(todoPos + 5)..].Trim();
                            if (todoText.Length > 0) {
                                // add this TODO (adjust line by 1)
                                WinAGIEventInfo tmpInfo = new() {
                                    ID = "TODO",
                                    Line = linenum,
                                    Module = Module,
                                    Filename = "",
                                    ResNum = LogicNum,
                                    ResType = AGIResType.Logic,
                                    Text = todoText,
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
        public static List<WinAGIEventInfo> ExtractDecompWarn(byte LogicNum, string SourceText, string Module) {
            // look for [ WARNING DC##
            int warnPos, commentPos, linenum;
            string decompWarning;
            List<string> sourceLines;
            List<WinAGIEventInfo> retval = [];

            // if warning marker isn't in the logic, just exit
            if (!SourceText.Contains("[ WARNING DC")) {
                return retval;
            }
            sourceLines = SourceText.SplitLines();
            for (linenum = 0; linenum < sourceLines.Count; linenum++) {
                warnPos = sourceLines[linenum].IndexOf("WARNING DC", StringComparison.OrdinalIgnoreCase);
                if (warnPos >= 0) {
                    commentPos = sourceLines[linenum].LastIndexOf('[', warnPos);
                    if (commentPos >= 0) {
                        decompWarning = sourceLines[linenum][(commentPos + 9)..].Trim();
                        if (decompWarning.Length > 0) {
                            // add this warning (adjust line by 1)
                            WinAGIEventInfo tmpInfo = new() {
                                ID = decompWarning[..4],
                                Line = linenum,
                                Module = Module,
                                Filename = "",
                                ResNum = LogicNum,
                                ResType = AGIResType.Logic,
                                Text = decompWarning[6..],
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
                if (INVALID_SIERRA_1ST_CHARS.Any(ch => ch == checkname[0])) {
                    return DefineNameCheck.BadChar;
                }
                if (checkname.Length > 1 && checkname[1..].Any(INVALID_SIERRA_CHARS.Contains)) {
                    return DefineNameCheck.BadChar;
                }
            }
            else {
                // INVALID_DEFINE_CHARS + "#$%.0123456789@"
                if (INVALID_FIRST_CHARS.Any(ch => ch == checkname[0])) {
                    return DefineNameCheck.BadChar;
                }
                // " !"&'()*+,-/:;<=>?[\]^`{|}~";
                if (checkname.Length > 1 && checkname[1..].Any(INVALID_DEFINE_CHARS.Contains)) {
                    return DefineNameCheck.BadChar;
                }
            }
            if (checkname.Any(ch => ch > 127 || ch < 32)) {
                return DefineNameCheck.BadChar;
            }
            if (!sierrasyntax) {
                // check against regular commands
                for (int i = 0; i < Commands.ActionCount; i++) {
                    if (checkname == Commands.ActionCommands[i].FanName) {
                        return DefineNameCheck.ActionCommand;
                    }
                }
                // check against test commands
                for (int i = 0; i < Commands.TestCount; i++) {
                    if (checkname == Commands.TestCommands[i].FanName) {
                        return DefineNameCheck.TestCommand;
                    }
                }
                // check or argument markers
                if ("vfmoiswc".Any(checkname.StartsWith)) {
                    if (checkname.Right(checkname.Length - 1).IsNumeric()) {
                        // can't have a name that's a valid marker
                        return DefineNameCheck.ArgMarker;
                    }
                }
            }
            // check against compiler keywords
            if (checkname is "if" or "else" or "goto") {
                return DefineNameCheck.KeyWord;
            }
            // if no error conditions, it's OK
            return DefineNameCheck.OK;
        }

        public static byte GetRandomByte(byte lower, byte upper) {
            Random random = new Random();
            return (byte)random.Next(lower, upper + 1);
        }

        public static byte FreqDivToMidiNote(int freqdiv) {
            // converts AGI freq offset into a MIDI note

            // middle C is 261.6 HZ; from midi specs,
            // middle C is a note with a Value of 60
            // this requires a shift in freq of approx. 36.376
            // however, this offset results in crappy sounding music;
            // empirically, 36.5 seems to work best

            // note that since freq divisor is in range of 0 - 1023
            // resulting midinotes are in range of 45 - 127
            // and that midinotes of 126, 124, 121 are not possible (no corresponding freq divisor)

            if (freqdiv <= 0) {
                // set note to Max
                return 127;
            }
            else {
                byte retval = (byte)Math.Round((Math.Log10(111860 / (double)freqdiv) / LOG10_1_12) - 36.5);
                // validate
                if (retval < 0) {
                    return 0;
                }
                if (retval > 127) {
                    return 127;
                }
                return retval;
            }
        }

        public static int MidiNoteToFreqDiv(int NoteIn) {
            // converts a midinote into a freqdivisor Value

            // valid range of NoteIn is 45-127
            // note that 121, 124, 126 NoteIn values will actually return
            // same freqdivisor values as 120, 123, 127 respectively (due
            // to loss of resolution in freqdivisor values at the high end)

            // validate input
            if (NoteIn < 45) {
                NoteIn = 45;
            }
            else if (NoteIn > 127) {
                NoteIn = 127;
            }
            // alternate calc:
            double freq = 440.0 * Math.Exp((NoteIn - 69) * Math.Log(2.0) / 12.0);
            if (freq - (int)freq >= 0.5) {
                freq = (int)freq + 1;
            }
            else {
                //freq = (int)freq;
            }
            return (int)(111860.0 / freq);
            //return (int)(111860.0 / Math.Round(440.0 * Math.Exp((NoteIn - 69) * Math.Log(2.0) / 12.0)));
            //double sngFreq = 111860D / Math.Pow(10, (NoteIn + 36.5) * LOG10_1_12);
            //return (int)Math.Round(sngFreq, 0);

        }
        #endregion
    }

    public static class ExtensionMethods {
        /// <summary>
        /// Converts only ascii characters in a string to lower case. Extended
        /// characters are not adjusted.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string LowerAGI(this string text) {
            StringBuilder sb = new(text);
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
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string UpperAGI(this string text) {
            StringBuilder sb = new(text);
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
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string text, int length) {
            if (length >= text.Length)
                return text;
            else if (length <= 0)
                return "";
            else
                return text[..length];
        }

        /// <summary>
        /// A safe version of SubString(string.Length-length, string.Length) that avoids
        /// exception if length > string.Length.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(this string text, int length) {
            if (length >= text.Length)
                return text;
            else if (length <= 0)
                return "";
            else
                return text[^length..];
        }

        /// <summary>
        /// A safe version of SubString that avoids exception if pos+length > string.Length
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pos"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Mid(this string text, int pos, int length) {
            if (pos > text.Length || length <= 0) {
                return "";
            }
            if (pos + length > text.Length)
                return text[pos..];
            return text.Substring(pos, length);
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
        /// <param name="text"></param>
        /// <param name="copyCount"></param>
        /// <returns></returns>
        public static string MultStr(this string text, int copyCount) {
            return new StringBuilder(text.Length * copyCount).Insert(0, text, copyCount).ToString();
        }

        /// <summary>
        /// Determines if a string is numeric or not.
        /// </summary>
        /// <param name="text">string that may be a number</param>
        /// <returns>true if numeric, false if not</returns>
        public static bool IsNumeric(this string text) {
            if (double.TryParse(text, out _)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if a string is an integer.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>true if integer, false if not</returns>
        public static bool IsInt(this string text) {
            if (int.TryParse(text, out _)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns numeric value of a string. If non-numeric, null, or empty
        /// it returns 0.
        /// </summary>
        /// <param name="text">The string that will be converted to a number</param>
        /// <returns>Returns a double value of strIn; if strIn can't be converted
        /// to a double, it returns 0.</returns>
        public static double Val(this string text) {
            if (double.TryParse(text, out double result)) {
                return result;
            }
            // not a valid number; return 0
            return 0;
        }

        public static int IntVal(this string text) {
            if (int.TryParse(text, out int result)) {
                return result;
            }
            // not a valid integer; return 0
            return 0;
        }

        /// <summary>
        /// Splits a string into an List of lines using all forms of line separators
        /// (CR, CRLF, LF).
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static List<string> SplitLines(this string text) {
            return text.Split(["\r\n", "\r", "\n"], StringSplitOptions.None).Cast<string>().ToList();
        }

        public static string SingleSpace(this string input) {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }
            // Use regular expression to replace all whitespace sequences with a single space
            return Regex.Replace(input, @"\s+", " ");
        }

        public static bool IsEven(this int input) {
            return input % 2 == 0;
        }

        public static bool IsOdd(this int input) {
            return input % 2 != 0;
        }

        public static bool IsEven(this byte input) {
            return input % 2 == 0;
        }

        public static bool IsOdd(this byte input) {
            return input % 2 != 0;
        }

        /// <summary>
        /// Expands the rectangle to include the given point defined by location.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static Rectangle Expand(this Rectangle rect, Point location) {
            if (rect.X == 0 && rect.Y == 0 && rect.Width == 0 && rect.Height == 0) {
                // initial point
                rect.Location = location;
            }
            else if (!rect.Contains(location)) {
                rect = Rectangle.Union(rect, new(location, new(0, 0)));
            }
            return rect;
        }

        /// <summary>
        /// Expands the rectangle to include the given point defined by x an y.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Rectangle Expand(this Rectangle rect, int x, int y) {
            Point p = new(x, y);
            if (rect.Width < 0 || rect.Height < 0) {
                // initial point
                rect.Location = new(x, y);
                rect.Width = 0;
                rect.Height = 0;
            }
            else if (!rect.Contains(p)) {
                rect = Rectangle.Union(rect, new(p, new(0, 0)));
            }
            return rect;
        }

        /// <summary>
        /// Adds text to the list. If the text has line separators, the text
        /// is split and multiple lines are added.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="text"></param>
        public static void AddLines(this List<string> list, string text) {
            // check for multiple lines in input text
            if (text.Contains('\n') || text.Contains('\r')) {
                text = text.Replace("\r\n", "\n");
                text = text.Replace('\r', '\n');
                // split it
                string[] items = text.Split('\n');
                foreach (string subitem in items) {
                    list.Add(subitem);
                }
            }
            else {
                list.Add(text);
            }
        }

        /// <summary>
        /// Concatenates the strings in the list and returns a single string using
        /// the specified .
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string Text(this List<string> list, string separator) {
            return string.Join(separator, list);
        }

        /// <summary>
        /// Converts the color value of the specified color to a comma delimited
        /// string of its rgb components as hex values.
        /// </summary>
        /// <param name="aColor"></param>
        /// <returns></returns>
        public static string ColorText(this Color aColor) {
            // Converts a color into a useful string value for storing in configuration files.
            return "0x" + aColor.R.ToString("x2") + ", 0x" + aColor.G.ToString("x2") + ", 0x" + aColor.B.ToString("x2");
        }

        public static string CommandName(this DrawFunction drawfunction) {
            return drawfunction switch {
                DrawFunction.EnableVis => "Vis: ON",    // Change picture color and enable picture draw.
                DrawFunction.DisableVis => "Vis: OFF",   // Disable picture draw.
                DrawFunction.EnablePri => "Pri: ON",    // Change priority color and enable priority draw.
                DrawFunction.DisablePri => "Pri: OFF",   // Disable priority draw.
                DrawFunction.YCorner => "Y Corner",      // Draw a Y corner.
                DrawFunction.XCorner => "X Corner",      // Draw an X corner.
                DrawFunction.AbsLine => "Line",      // Absolute line (long lines).
                DrawFunction.RelLine => "Short Line",      // Relative line (short lines).
                DrawFunction.Fill => "Fill",         // Fill.
                DrawFunction.ChangePen => "Set Plot Pen",    // Change pen size and style.
                DrawFunction.PlotPen => "Plot",      // Plot with pen.
                DrawFunction.End => "End",           // end of drawing
                _ => "undefined"
            };
        }
    }
}
