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
    }

    public static partial class Base {
        public const char QUOTECHAR = '\"';
        public static readonly string NEWLINE = Environment.NewLine;
        public const double LOG10_1_12 = 2.50858329719984E-02; // = Log10(2 ^ (1/12))
        public const string ARG1 = "%1";
        public const string ARG2 = "%2";
        public const string ARG3 = "%3";
        public const string sAPPNAME = "WinAGI Game Development System 3.0 alpha";
        public const string COPYRIGHT_YEAR = "2024";
        public static uint[] CRC32Table = new uint[256];
        public static bool CRC32Loaded;
        public static readonly char[] INVALID_ID_CHARS;
        public static readonly char[] INVALID_DEFNAME_CHARS;

        static Base() {
            // TODO: first char restictions are (should be) different than rest
            // invalid ID characters: these, plus control chars and extended chars
            //        3       4         5         6         7         8         9         0         1         2
            //        234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
            //NOT OK  .!"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~x
            //    OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    
            INVALID_ID_CHARS = " !\"&'()*+,-/:;<=>?[\\]^`{|}~".ToCharArray();

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
        /// <summary>
        /// Converts only ascii characters in a string to lower case. Extended
        /// characters are not adjusted.
        /// </summary>
        /// <param name="strIn"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string LowerAGI(string strIn) {
            StringBuilder sb = new StringBuilder(strIn);
            for (int i = 0; i < sb.Length; i++) {
                if (sb[i] >= 65 && sb[i] <= 90) {
                    sb[i] |= ' ';
                }
            }
            return sb.ToString();  
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
        }

        /// <summary>
        /// Extension method that works out if a string is numeric or not
        /// </summary>
        /// <param name="str">string that may be a number</param>
        /// <returns>true if numeric, false if not</returns>
        public static bool IsNumeric(string str) {
            // TODO: do I need another test specifically for int values?
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
                return dResult;
            }
            // not a valid number; return 0
            return 0;
        }

        /// <summary>
        /// Confirms that a directory has a terminating backslash,
        /// adding one if necessary.
        /// </summary>
        /// <param name="strDirIn"></param>
        /// <returns></returns>
        public static string CDir(string strDirIn) {
            if (strDirIn.Length != 0) {
                return !strDirIn.EndsWith('\\') ? strDirIn + '\\' : strDirIn;
            }
            else {
                return strDirIn;
            }
        }
        /// <summary>
        /// Extracts just the path name by removing the filename. 
        /// If optional NoSlash is true, the trailing backslash will be dropped.
        /// </summary>
        /// <param name="fullPathName"></param>
        /// <param name="noSlash"></param>
        /// <returns></returns>
        public static string JustPath(string fullPathName, bool noSlash = false) {
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

        public static uint CRC32(byte[] DataIn) {
            // calculates the CRC32 for an input array of bytes
            // a special table is necessary; the table is loaded
            // at program startup

            // the CRC is calculated according to the following equation:
            //
            //   CRC[i] = LSHR8(CRC[i-1]) Xor CRC32Table[(CRC[i-1] && 0xFF) Xor DataIn[i])
            //
            // initial Value of CRC is 0xFFFFFFFF; iterate the equation
            // for each byte of data; then end by XORing final result with 0xFFFFFFFF
            uint result = 0xffffffff;

            if (!CRC32Loaded) {
                CRC32Setup();
            }
            for (int i = 0; i < DataIn.Length; i++) {
                result = (result >> 8) ^ CRC32Table[(result & 0xFF) ^ DataIn[i]];
            }
            // xor to create final answer
            return result ^ 0xFFFFFFFF;
        }

        public static void CRC32Setup() {
            // build the CRC table
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
        }
        /// <summary>
        /// Copies contents of source directory to target. If copySubDirs is true,
        /// all subdirectories in source will also be copied.
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="Exception"></exception>
        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs) {

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
            // if already fits,
            if (LongPath.Length <= MaxLength) {
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
            strDir = LongPath[..lngPos];
            strFile = LongPath[(lngPos + 1)..];
            // if file name is too long
            if (strFile.Length > MaxLength - 4) {
                // return truncated filename
                return Left(strFile, MaxLength - 3) + "...";
            }
            // truncate directory, pad with ... and return combined dir/filename
            return Left(strDir, MaxLength - 4) + "...\\" + strFile;
        }

        internal static List<string> SplitLines(string strText) {
            // splits the input text into lines, by CR, LF, or CRLF
            // strategy is to replace CRLFs with CRs, then LFs with CRs,
            // and then slpit by CRs
            List<string> retval = [.. strText.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')];
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
            if (!SourceText.Contains("WARNING DC")) {
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
