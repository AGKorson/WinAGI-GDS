using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.Base;
using WinAGI.Common;
using static WinAGI.Common.Base;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WinAGI.Engine {
    /// <summary>
    /// Collection of words (agiWord objects).
    /// </summary>
    public class WordList : IEnumerable<AGIWord> {
        #region Members
        SortedList<string, AGIWord> mWordCol;
        SortedList<int, WordGroup> mGroupCol;
        string mResFile = "";
        string mDescription = "";
        bool mInGame;
        AGIGame parent;
        int mCodePage = Base.CodePage;
        bool mIsChanged;
        bool mLoaded;

        #endregion

        #region Constructors
        /// <summary>
        /// Instantiates a list of AGI words and attaches it to an AGI game.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Loaded"></param>
        internal WordList(AGIGame parent, bool Loaded = false) {
            mWordCol = new(new AGIWordComparer());
            mGroupCol = [];

            mInGame = true;
            this.parent = parent;
            // if loaded property is passed, set loaded flag as well
            mLoaded = Loaded;
            // also sets the default name to 'WORDS.TOK'
            mResFile = this.parent.agGameDir + "WORDS.TOK";
            mCodePage = parent.agCodePage;
        }

        /// <summary>
        /// Instantiates a blank list of AGI words that are not attached to a game.
        /// </summary>
        public WordList() {
            mInGame = false;
            mResFile = "";
            mWordCol = new(new AGIWordComparer());
            mGroupCol = [];
            mIsChanged = true;
            mLoaded = true;
            // add default words
            AGIWord tmpWord = new() { WordText = "a", Group = 0 };
            mWordCol.Add("a", tmpWord);
            tmpWord.WordText = "anyword";
            tmpWord.Group = 1;
            mWordCol.Add("anyword", tmpWord);
            tmpWord.WordText = "rol";
            tmpWord.Group = 9999;
            mWordCol.Add("rol", tmpWord);
            // and default groups
            WordGroup tmpGroup = new() { mGroupNum = 0 };
            tmpGroup.AddWordToGroup("a");
            mGroupCol.Add(0, tmpGroup);
            tmpGroup = new WordGroup { mGroupNum = 1 };
            tmpGroup.AddWordToGroup("any");
            mGroupCol.Add(1, tmpGroup);
            tmpGroup = new WordGroup { mGroupNum = 9999 };
            tmpGroup.AddWordToGroup("rol");
            mGroupCol.Add(9999, tmpGroup);

        }

        /// <summary>
        /// Instantiates a list of AGI words from a file that are not attached
        /// to a game.
        /// </summary>
        public WordList(string filename) {
            mInGame = false;
            mResFile = filename;
            mIsChanged = true;
            Load(filename);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the word object with the specified index from this list.
        /// </summary>
        /// <param name="vKeyIndex"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public AGIWord this[int index] {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (index < 0 || index > mWordCol.Count - 1) {
                    throw new IndexOutOfRangeException();
                }
                return mWordCol.Values[index];
            }
        }

        /// <summary>
        /// Gets the word object with the specified key from this list. The
        /// key is always the word text.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public AGIWord this[string key] {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mWordCol.TryGetValue(key, out AGIWord value)) {
                    return value;
                }
                else {
                    throw new ArgumentException("invalid key");
                }
            }
        }

        /// <summary>
        /// Gets the number of word groups in this word list.
        /// </summary>
        public int GroupCount {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mGroupCol.Count;
            }
        }

        /// <summary>
        /// Gets the number of words in this word list.
        /// </summary>
        public int WordCount {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mWordCol.Count;
            }
        }

        /// <summary>
        /// Gets or sets the code page to use when displaying extended
        /// characters in word text.
        /// </summary>
        public int CodePage {
            get => parent is null ? mCodePage : parent.agCodePage;
            set {
                if (parent is null) {
                    // confirm new codepage is supported
                    if (validcodepages.Contains(value)) {
                        mCodePage = value;
                    }
                    else {
                        throw new ArgumentOutOfRangeException("CodePage", "Unsupported or invalid CodePage value");
                    }
                }
                else {
                    // ignore; the game sets codepage
                }
            }
        }

        /// <summary>
        /// Gets the load status of this word list. The items and other
        /// properties are not available if the list is not loaded. If not in a game,
        /// the list is always loaded.
        /// </summary>
        public bool Loaded {
            get {
                return mLoaded;
            }
        }

        /// <summary>
        /// Gets the current error level of the WordList. Only used during first game
        /// load event. Once a list is saved in WinAGI, it will always be 0.
        /// </summary>
        public ResourceErrorType Error { get; internal set; } = ResourceErrorType.NoError;

        public string[] ErrData { get; internal set; } = ["", "", "", "", "", ""];

        /// <summary>
        /// Bitfield that contains warnings applicable to this wordlist:<br />
        /// 1 = abnormal index table<br />
        /// 2 = unexpected end of file<br />
        /// 4 = upper case characters detected<br />
        /// 8 = invalid characters detected<br />
        /// 16 = duplicate words found<br />
        /// 32 = multiple group 1 words<br />
        /// 64 = multiple group 9999 words
        /// </summary>
        public int Warnings { get; internal set; } = 0;

        /// <summary>
        /// Gets individualized warning tags for this resource. Varies for
        /// each type of derived resource.
        /// </summary>
        public string[] WarnData { get; internal set; } = ["", "", "", "", "", ""];

        /// <summary>
        /// Gets or sets a text field that can be used for any purpose. The description
        /// property is stored in the games' WinAGI Game file, but is not used in any other
        /// way. If not in a game, no use is made of the description property.
        /// </summary>
        public string Description {
            get {
                return mDescription;
            }
            set {
                value = value.Left(1024);
                if (value != mDescription) {
                    mDescription = value;
                    if (mInGame) {
                        parent.WriteGameSetting("WORDS.TOK", "Description", mDescription);
                        parent.agLastEdit = DateTime.Now;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the in-game status of this inventory item list, i.e. whether
        /// it's in a game or a stand alone resource.
        /// </summary>
        public bool InGame {
            get {
                return mInGame;
            }
            internal set {
                mInGame = value;
            }
        }

        /// <summary>
        /// Gets or sets the WORDS.TOK filename associated with this word list. If in a game, 
        /// the resfile name is readonly.
        /// </summary>
        public string ResFile {
            get {
                return mResFile;
            }
            set {
                // resfile cannot be changed if resource is part of a game
                if (mInGame) {
                    // ignore
                    return;
                }
                else {
                    mResFile = value;
                }
            }
        }

        /// <summary>
        /// Gets a a value that indicates if this list's words do not match what is 
        /// stored its assigned WORDS.TOK file.
        /// </summary>
        public bool IsChanged {
            get {
                if (Loaded) {
                    return mIsChanged;
                }
                else {
                    return false;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads words into this list from a WORDS.TOK file. If not in a game,
        /// LoadFile must be specified.
        /// </summary>
        /// <param name="LoadFile"></param>
        public virtual void Load(string LoadFile = "") {
            if (mLoaded) {
                return;
            }
            if (mInGame) {
                LoadFile = mResFile;
            }
            // clear error info before loading
            ErrData = ["", "", "", "", "", ""];
            WarnData = ["", "", "", "", "", ""];

            // set loaded flag before extracting data
            mLoaded = true;
            (Error, Warnings) = LoadWordsTokFile(LoadFile);
            if (mInGame) {
                mDescription = parent.agGameProps.GetSetting("WORDS.TOK", "Description", "", true);
            }
            else {
                mResFile = LoadFile;
            }
        }

        /// <summary>
        /// Extracts the words and group information from this word list's file.
        /// Returns an error code specifying whether any warnings or errors were
        /// encountered when the file is loaded.
        /// </summary>
        /// <param name="LoadFile"></param>
        /// <returns></returns>
        private (ResourceErrorType, int) LoadWordsTokFile(string LoadFile) {
            byte bytExt;
            int pos = 0, retwarn = 0;
            ResourceErrorType reterr = ResourceErrorType.NoError;
            byte[] worddata = [];
            StringBuilder sbThisWord;
            string sThisWord;
            byte[] bytVal = new byte[1];
            int groupnumber;
            FileStream fsWords;

            mWordCol = new(new AGIWordComparer());
            mGroupCol = [];
            if (!File.Exists(LoadFile)) {
                // clear to a blank list
                Clear();
                return (ResourceErrorType.WordsTokNoFile, 0);
            }
            if ((File.GetAttributes(LoadFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                reterr = ResourceErrorType.WordsTokIsReadOnly;
            }
            try {
                fsWords = new(LoadFile, FileMode.Open, FileAccess.Read);
            }
            catch (Exception ex) {
                // file access error
                ErrData[0] = ex.Message;
                Clear();
                return (ResourceErrorType.WordsTokAccessError, 0);
            }
            // a properly formatted WORDS.TOK has a 52 byte index
            // followed by word entries, with words in alphabetical
            // order. Each word entry includes 1 byte for count of
            // previous word characters, one byte for each additional
            // character in the word, and 2 bytes for the word group
            //
            // minimum size (if no words) is 52 bytes, all zero
            // all known AGI games have at least one word starting
            // with 'a', so first index pointer is always 52
            // BUT it is possible for first word to start with a
            // different letter; the index values of preceding
            // letters will be zero, and first non-zero index
            // should be 52
            //
            // if at least one word, minimum size is 56: 52 for
            // index, 1 for prevcount, 1 for singlechar, 2 for
            // group (52 is ok, >=56 is ok)
            if (fsWords.Length != 52 && fsWords.Length < 56) {
                // be sure to release the stream
                fsWords.Dispose();
                Clear();
                return (ResourceErrorType.WordsTokNoData, 0);
            }
            // read in entire resource
            Array.Resize(ref worddata, (int)fsWords.Length);
            fsWords.Read(worddata);
            // done with the stream
            fsWords.Dispose();
            fsWords = null;
            // validate index- first nonzero index offset should
            // be 52; all other nonzeros should be greater than
            // the previous
            // (words.tok file uses MSLS format for two byte-word data)
            for (int i = 0; i < 52; i++) {
                pos = (worddata[i++] << 8) + worddata[i];
                if (pos > 0) {
                    // 52 is expected; greater than 52 might be OK
                    if (pos != 52) {
                        if (pos > 52) {
                            // index is corrupt or invalid;
                            // don't assume it's bad just yet - if the target
                            // byte is zero, give it a try
                            if (worddata[pos] == 0) {
                                // note the problem in a warning
                                retwarn = 1;
                            }
                            else {
                                // bad index, unable to read this file
                                Clear();
                                return (ResourceErrorType.WordsTokBadIndex, 0);
                            }
                        }
                        else {
                            // bad index
                            Clear();
                            return (ResourceErrorType.WordsTokBadIndex, 0);
                        }
                    }
                    else {
                        // verify first byte is zero (first word can't copy any
                        // characters from previous)
                        if (worddata[pos] != 0) {
                            // bad index, unable to read this file
                            Clear();
                            return (ResourceErrorType.WordsTokBadIndex, 0);
                        }
                    }
                    break;
                }
            }
            // if offset found, it means there are words to add
            if (pos != 0) {
                // for first word, there is no previous word
                string previousword = "";
                byte prevWordCharCount = 0;


                // start with first word
                // read character Count for first word
                prevWordCharCount = worddata[pos++];

                // continue loading words until end of resource is reached
                while (pos < worddata.Length) {
                    if (prevWordCharCount > 0) {
                        if (prevWordCharCount > previousword.Length) {
                            // error
                            // keep the words that have already been read
                            WarnData[0] = pos.ToString();
                            retwarn |= 128;
                            sbThisWord = new(previousword);
                        }
                        else {
                            sbThisWord = new(previousword[..prevWordCharCount]);
                        }
                    }
                    else {
                        sbThisWord = new();
                    }
                    do {
                        bytVal[0] = worddata[pos];
                        // EXTENDED CHARACTER FORMAT CHECK:
                        if (bytVal[0] == 0) {
                            // the next char is the actual character,
                            // and is extended
                            pos++;
                            bytVal[0] = worddata[pos];
                            bytExt = 128;
                        }
                        else {
                            bytExt = 0;
                        }
                        if (bytVal[0] < 0x80) {
                            byte[] charbyte = [(byte)(bytVal[0] ^ 0x7F + bytExt)];
                            sbThisWord.Append(Encoding.GetEncoding(CodePage).GetString(charbyte));
                        }
                        pos++;
                        // continue until last character (indicated by flag) or
                        // endofresource is reached
                    }
                    while ((bytVal[0] < 0x80) && (pos < worddata.Length));
                    // check for end of file
                    if (pos >= worddata.Length) {
                        // invalid words.tok file ending
                        retwarn |= 2;
                        break;
                    }
                    // add last character (after stripping off flag)
                    bytVal[0] ^= 0xFF;
                    bytVal[0] += bytExt;
                    sbThisWord.Append(Encoding.GetEncoding(CodePage).GetString(bytVal));
                    sThisWord = sbThisWord.ToString();
                    // check for ascii upper case (allowed, but not useful)
                    if (sThisWord.Any(ch => ch >= 65 && ch <= 90)) {
                        retwarn |= 4;
                    }
                    // check for invalid characters ,.?!();:[]{} and '`-"
                    const string inv = ",.?!();:[]{}'`-\"";
                    if (sThisWord.Any(inv.Contains)) {
                        retwarn |= 8;
                    }
                    sThisWord = sThisWord.LowerAGI();
                    groupnumber = (worddata[pos++] << 8) + worddata[pos++];
                    // skip if same as previous word (unlikely, but 
                    // it would cause an exception, if it did happen)
                    if (sThisWord != previousword) {
                        if (mWordCol.ContainsKey(sThisWord)) {
                            // remove the old one so a duplicate isn't
                            // added, which would cause an exception
                            RemoveWord(sThisWord);
                            // add a warning
                            retwarn |= 16;
                        }
                        AddWord(sThisWord, groupnumber);
                        // this word is now the previous word
                        previousword = sThisWord;
                        // check for multiple words in group 1 or 9999
                        if (groupnumber == 1) {
                            if (mGroupCol[groupnumber].mWords.Count > 1) {
                                retwarn |= 32;
                            }
                        }
                        if (groupnumber == 9999) {
                            if (mGroupCol[groupnumber].mWords.Count > 1) {
                                retwarn |= 64;
                            }
                        }
                    }
                    prevWordCharCount = worddata[pos++];
                }
            }
            else {
                // no words- should only happen if list is empty
                if (worddata.Length != 52) {
                    // bad index, unable to read this file
                    Clear();
                    return (ResourceErrorType.WordsTokBadIndex, 0);
                }
            }
            // if groups 0, 1, or 9999 not added, add them here
            if (!GroupExists(0)) {
                AddGroup(0);
            }
            if (!GroupExists(1)) {
                AddGroup(1);
            }
            if (!GroupExists(9999)) {
                AddGroup(9999);
            }
            mIsChanged = reterr != ResourceErrorType.NoError;
            return (reterr, retwarn);
        }

        /// <summary>
        /// Unloads this word list if in a game. Word lists not in a game
        /// are always loaded.
        /// </summary>
        public void Unload() {
            if (!mLoaded) {
                // ignore
                return;
            }
            Clear();
            mLoaded = false;
            mIsChanged = false;
        }

        /// <summary>
        /// Saves this word list to file by compiling the words in the
        /// list in the proper Sierra AGI WORDS.TOK file format. If not in a game, 
        /// the default filename is used unless one is passed to this method.
        /// </summary>
        /// <param name="SaveFile"></param>
        public void Save(string SaveFile = "") {
            WinAGIException.ThrowIfNotLoaded(this);
            if (mInGame) {
                SaveFile = mResFile;
            }
            else {
                if (SaveFile.Length == 0) {
                    SaveFile = mResFile;
                }
                ArgumentException.ThrowIfNullOrWhiteSpace(SaveFile);
            }
            try {
                Compile(SaveFile);
            }
            catch {
                throw;
            }
            if (mInGame) {
                parent.agLastEdit = DateTime.Now;
            }
            else {
                mResFile = SaveFile;
            }
            mIsChanged = false;
            // clear errors
            Error = ResourceErrorType.NoError;
            ErrData = ["", "", "", "", "", ""];
            // check warnings
            // 32 = multiple group 1 words<br />
            // 64 = multiple group 9999 words
            Warnings = 0;
            if (this.GroupByNumber(1).WordCount > 1) {
                Warnings |= 32;
            }
            if (this.GroupByNumber(9999).WordCount > 1) {
                Warnings |= 64;
            }
        }

        /// <summary>
        /// Compiles the word list into a Sierra WORDS.TOK file.
        /// </summary>
        /// <param name="CompileFile"></param>
        void Compile(string CompileFile) {
            byte outputbyte;
            byte[] wordtext;
            byte[] previousword = [];
            int[] letterIndex = new int[26];
            System.Buffers.SearchValues<char> s_inv = System.Buffers.SearchValues.Create(",.?!();:[]{}'`-\"");

            ArgumentException.ThrowIfNullOrWhiteSpace(CompileFile);
            if (!mIsChanged && CompileFile.Equals(mResFile, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            string tempFile = Path.GetTempFileName();
            FileStream fsWords;
            try {
                fsWords = new FileStream(tempFile, FileMode.OpenOrCreate);
                // letter index placeholders
                for (int i = 0; i <= 51; i++) {
                    fsWords.WriteByte(0);
                }
                // if no words, no need to initialize offset table 
                if (mWordCol.Count > 0) {
                    byte firstletter = (byte)'a';
                    letterIndex[0] = 52;
                    foreach (AGIWord tmpWord in mWordCol.Values) {
                        // get next word
                        wordtext = Encoding.GetEncoding(CodePage).GetBytes(tmpWord.WordText);
                        // upper chase characters not allowed
                        if (wordtext.Any(ch => ch >= 65 && ch <= 90)) {
                            throw new WinAGIException(LoadResString(549).Replace(
                                ARG1, tmpWord.WordText)) {
                                HResult = WINAGI_ERR + 549,
                            };
                        }
                        if (tmpWord.WordText.AsSpan().IndexOfAny(s_inv) >= 0) {
                            throw new WinAGIException(LoadResString(550).Replace(
                                ARG1, tmpWord.WordText)) {
                                HResult = WINAGI_ERR + 550,
                            };
                        }
                        if (parent is not null || !parent.PowerPack) {
                            // extended characters not allowed unless using powerpack
                            if (wordtext.Any(ch => ch >127)) {
                                throw new WinAGIException(LoadResString(551).Replace(
                                    ARG1, tmpWord.WordText)) {
                                    HResult = WINAGI_ERR + 551,
                                };
                            }
                            // first char must be a letter unless powerpack is used
                            if (wordtext[0] < 97 || wordtext[0] > 122) {
                                throw new WinAGIException(LoadResString(552).Replace(
                                    ARG1, tmpWord.WordText)) {
                                    HResult = WINAGI_ERR + 552,
                                };
                            }
                        }
                        // if first letter is not current first letter, AND it is 'b' through 'z'
                        // (this ensures non letter words (such as numbers) are included in the 'a' section)
                        if ((wordtext[0] != firstletter) && (wordtext[0] >= 97) && (wordtext[0] <= 122)) {
                            // reset index pointer
                            firstletter = wordtext[0];
                            letterIndex[firstletter - 97] = (int)fsWords.Position;
                        }
                        // calculate number of characters that are common to previous word
                        byte prevcharcount = 0;
                        int i = 0;
                        bool match = true;
                        do {
                            // if not at end of word, AND current position is not longer than previous word,
                            if ((i < wordtext.Length - 1) && (i < previousword.Length)) {
                                if (previousword[i] == wordtext[i]) {
                                    prevcharcount++;
                                }
                                else {
                                    match = false;
                                }
                            }
                            else {
                                match = false;
                            }
                            i++;
                        }
                        while (match);
                        fsWords.WriteByte(prevcharcount);
                        previousword = wordtext;
                        // strip off characters that are same as previous word
                        wordtext = wordtext[prevcharcount..];
                        if (wordtext.Length > 1) {
                            // write all but last character
                            for (i = 0; i < wordtext.Length - 1; i++) {
                                // check for extended characters
                                if (wordtext[i] > 127) {
                                    // add marker
                                    fsWords.WriteByte(0);
                                    // adjust character down by 128
                                    outputbyte = (byte)(0x7F ^ (wordtext[i] - 128));
                                }
                                else {
                                    // encrypt character before writing it
                                    outputbyte = (byte)(0x7F ^ wordtext[i]);
                                }
                                // add the encrypted character
                                fsWords.WriteByte(outputbyte);
                            }
                        }
                        // add last char - first check for extended character
                        if (wordtext[^1] > 127) {
                            // add marker
                            fsWords.WriteByte(0);
                            // adjust character down by 128
                            outputbyte = (byte)(0x80 + (0x7F ^ (wordtext[^1] - 128)));
                        }
                        else {
                            outputbyte = (byte)(0x80 + (0x7F ^ wordtext[^1]));
                        }
                        // add the encrypted end character
                        fsWords.WriteByte(outputbyte);
                        // write group number (stored as 2-byte word; high byte first)
                        outputbyte = (byte)(tmpWord.Group / 256);
                        fsWords.WriteByte(outputbyte);
                        outputbyte = (byte)(tmpWord.Group % 256);
                        fsWords.WriteByte(outputbyte);
                    }
                    // add null character to end of file
                    fsWords.WriteByte(0);
                    // reset file pointer to start
                    fsWords.Seek(0, SeekOrigin.Begin);
                    // write index values for all 26 letters
                    for (int i = 0; i < 26; i++) {
                        // two byte word, high byte first
                        outputbyte = (byte)(letterIndex[i] / 256);
                        fsWords.WriteByte(outputbyte);
                        outputbyte = (byte)(letterIndex[i] % 256);
                        fsWords.WriteByte(outputbyte);
                    }
                }
                fsWords.Dispose();
                // copy tempfile to CompileFile
                SafeFileMove(tempFile, CompileFile, true);
            }
            catch (Exception ex) {
                WinAGIException wex = new(LoadResString(502).Replace(
                    ARG1, ex.Message).Replace(
                    ARG2, tempFile)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = ex;
                wex.Data["ID"] = "WORDS.TOK";
                throw wex;
            }
        }

        /// <summary>
        /// Compiles this wordlist into a file that is compatible with the ScummVM
        /// Extended Word List file format.
        /// </summary>
        /// <param name="CompileFile"></param>
        void CompileSVE(string CompileFile) {
            // format is:
            //   header (1st line): "Unofficial extended format to support ASCII range of 128-255"
            //   subsequent lines are words, in alphabetical order
            //   in following format:
            //        wordtext + chr(0) + groupnum.ToString() + newline(CRLF)
            string strTempFile;
            int i;

            ArgumentException.ThrowIfNullOrWhiteSpace(CompileFile);
            strTempFile = Path.GetTempFileName();
            try {
                StreamWriter swWords = new(strTempFile);
                swWords.WriteLine("Unofficial extended format to support ASCII range of 128-255");
                // add all words, in alphabetical order
                for (i = 0; i < this.WordCount; i++) {
                    swWords.WriteLine(this[i].WordText + (char)0 + this[i].Group);
                }
                swWords.Close();
                swWords.Dispose();
                SafeFileMove(strTempFile, CompileFile, true);
            }
            catch (Exception ex) {
                WinAGIException wex = new(LoadResString(502).Replace(
                    ARG1, ex.Message).Replace(
                    ARG2, strTempFile)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = ex;
                wex.Data["ID"] = "WORDS.TOK";
                throw wex;
            }
        }

        /// <summary>
        /// Exports this word list to the specified file.
        /// </summary>
        public void Export(string ExportFile) {
            WinAGIException.ThrowIfNotLoaded(this);
            try {
                Compile(ExportFile);
            }
            catch {
                throw;
            }
            if (!mInGame) {
                mIsChanged = false;
                mResFile = ExportFile;
            }
            return;
        }

        /// <summary>
        /// Exports this word list in a format compatible with SCUMM SVE.
        /// </summary>
        public void ExportSVE(string ExportFile) {
            WinAGIException.ThrowIfNotLoaded(this);
            try {
                CompileSVE(ExportFile);
            }
            catch {
                throw;
            }
        }

        /// <summary>
        /// Creates an exact copy of this WordList.
        /// </summary>
        /// <returns>The WordList that this method creates</returns>
        public WordList Clone() {
            // only loaded wordlists can be cloned
            WinAGIException.ThrowIfNotLoaded(this);

            WordList clonelist = new();
            // clear the defaults
            clonelist.mWordCol = new(new AGIWordComparer());
            clonelist.mGroupCol = [];

            int lngGrpNum;
            string strWord;
            WordGroup tmpGroup;
            AGIWord tmpWord;

            foreach (WordGroup group in mGroupCol.Values) {
                tmpGroup = new WordGroup();
                lngGrpNum = group.GroupNum;
                tmpGroup.GroupNum = lngGrpNum;
                clonelist.mGroupCol.Add(lngGrpNum, tmpGroup);
            }
            foreach (AGIWord word in mWordCol.Values) {
                strWord = word.WordText;
                lngGrpNum = word.Group;
                tmpWord.WordText = strWord;
                tmpWord.Group = lngGrpNum;
                clonelist.mWordCol.Add(strWord, tmpWord);
                clonelist.mGroupCol[lngGrpNum].AddWordToGroup(strWord);
            }
            clonelist.mLoaded = mLoaded;
            clonelist.mDescription = mDescription;
            clonelist.mResFile = mResFile;
            clonelist.mIsChanged = mIsChanged;
            clonelist.mCodePage = mCodePage;
            clonelist.Error = Error;
            clonelist.Warnings = Warnings;
            for (int i = 0; i < ErrData.Length; i++) {
                clonelist.ErrData[i] = ErrData[i];
                clonelist.WarnData[i] = WarnData[i];
            }
            return clonelist;
        }

        public void CloneFrom(WordList clonelist) {
            int lngGrpNum;
            string strWord;
            WordGroup tmpGroup;
            AGIWord tmpWord;

            WinAGIException.ThrowIfNotLoaded(this);
            WinAGIException.ThrowIfNotLoaded(clonelist);
            mGroupCol = [];
            foreach (WordGroup group in clonelist.mGroupCol.Values) {
                tmpGroup = new WordGroup();
                lngGrpNum = group.GroupNum;
                tmpGroup.GroupNum = lngGrpNum;
                mGroupCol.Add(lngGrpNum, tmpGroup);
            }
            mWordCol = new(new AGIWordComparer());
            foreach (AGIWord word in clonelist.mWordCol.Values) {
                strWord = word.WordText;
                lngGrpNum = word.Group;
                tmpWord.WordText = strWord;
                tmpWord.Group = lngGrpNum;
                mWordCol.Add(strWord, tmpWord);
                mGroupCol[lngGrpNum].AddWordToGroup(strWord);
            }
            mDescription = clonelist.Description;
            mResFile = clonelist.ResFile;
            mIsChanged = clonelist.mIsChanged;
            mCodePage = clonelist.mCodePage;
            Error = clonelist.Error;
            Warnings = clonelist.Warnings;
            for (int i = 0; i < ErrData.Length; i++) {
                ErrData[i] = clonelist.ErrData[i];
                WarnData[i] = clonelist.WarnData[i];
            }
            mLoaded = true;
        }
        
        /// <summary>
        /// Clears this word list and sets all properties to default values.
        /// </summary>
        public void Clear() {
            WinAGIException.ThrowIfNotLoaded(this);
            mGroupCol = [];
            mWordCol = new(new AGIWordComparer());
            // default words
            AddWord("a", 0);
            AddWord("i", 0);
            AddWord("anyword", 1);
            AddWord("rol", 9999);
            mDescription = "";
            mIsChanged = true;
        }

        /// <summary>
        /// Returns a group by its group number.
        /// </summary>
        /// <param name="groupNum"></param>
        /// <returns></returns>
        public WordGroup GroupByNumber(int groupNum) {
            WinAGIException.ThrowIfNotLoaded(this);
            // return this group by its number (key value)
            return mGroupCol[groupNum];
        }

        public int GroupIndexFromNumber(int groupNum) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (!mGroupCol.ContainsKey(groupNum)) {
                throw new ArgumentException("group number not found");
            }
            return mGroupCol.IndexOfKey(groupNum);
        }

        /// <summary>
        /// Returns a group by its index (NOT the same as group number).
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public WordGroup GroupByIndex(int Index) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (Index < 0 || Index > mGroupCol.Count - 1) {
                throw new IndexOutOfRangeException("invalid word group index");
            }
            // access the group by its index
            return mGroupCol.Values[Index];
        }

        /// <summary>
        /// Returns true if the specified group number exists in this word list.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public bool GroupExists(int groupnumber) {
            WinAGIException.ThrowIfNotLoaded(this);
            return mGroupCol.ContainsKey(groupnumber);
        }

        /// <summary>
        /// Adds a new word group to this word list. The new group is created 
        /// with no words - words must be added after the group is created.
        /// </summary>
        /// <param name="GroupNumber"></param>
        public void AddGroup(int GroupNumber) {
            WordGroup tmpGroup;
            WinAGIException.ThrowIfNotLoaded(this);
            ArgumentOutOfRangeException.ThrowIfNegative(GroupNumber);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(GroupNumber, MAX_GROUP_NUM);
            // see if group already exists
            if (mGroupCol.ContainsKey(GroupNumber)) {
                throw new ArgumentException("group already exists");
            }
            tmpGroup = new WordGroup {
                GroupNum = GroupNumber
            };
            mGroupCol.Add(GroupNumber, tmpGroup);
            mIsChanged = true;
        }

        /// <summary>
        /// Removes a group and all its words from this word list.
        /// </summary>
        /// <param name="GroupNumber"></param>
        public void RemoveGroup(int GroupNumber) {
            int i;
            WinAGIException.ThrowIfNotLoaded(this);
            ArgumentOutOfRangeException.ThrowIfNegative(GroupNumber, nameof(GroupNumber));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(GroupNumber, MAX_GROUP_NUM, nameof(GroupNumber));
            if (!GroupExists(GroupNumber)) {
                WinAGIException wex = new(LoadResString(515)) {
                    HResult = WINAGI_ERR + 515
                };
                throw wex;
            }
            // step through all words in main list
            // CAN'T use foreach because we need to delete 
            // some of the words
            for (i = mWordCol.Count - 1; i >= 0; i--) {
                if (mWordCol.Values[i].Group == GroupNumber) {
                    mWordCol.RemoveAt(i);
                }
            }
            mGroupCol.Remove(GroupNumber);
            mIsChanged = true;
        }

        /// <summary>
        /// Changes the number of a word group in this word list.
        /// </summary>
        /// <param name="oldgroupnum"></param>
        /// <param name="newgroupnum"></param>
        public void RenumberGroup(int oldgroupnum, int newgroupnum) {
            int i;
            WordGroup tmpGroup;

            WinAGIException.ThrowIfNotLoaded(this);
            ArgumentOutOfRangeException.ThrowIfNegative(oldgroupnum, nameof(oldgroupnum));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(oldgroupnum, MAX_GROUP_NUM, nameof(oldgroupnum));
            ArgumentOutOfRangeException.ThrowIfNegative(newgroupnum, nameof(newgroupnum));
            ArgumentOutOfRangeException.ThrowIfGreaterThan(newgroupnum, MAX_GROUP_NUM, nameof(newgroupnum));
            if (!GroupExists(oldgroupnum)) {
                throw new ArgumentException("oldgroupnum does not exist");
            }
            if (GroupExists(newgroupnum)) {
                throw new ArgumentException("newgroupnum already exists");
            }
            tmpGroup = mGroupCol[oldgroupnum];
            _ = mGroupCol.Remove(oldgroupnum);
            tmpGroup.GroupNum = newgroupnum;
            // change group number for all words in the group
            for (i = 0; i < mWordCol.Count; i++) {
                if (mWordCol.Values[i].Group == oldgroupnum) {
                    AGIWord tmpWord = mWordCol.Values[i];
                    tmpWord.Group = newgroupnum;
                    mWordCol.RemoveAt(i);
                    mWordCol.Add(tmpWord.WordText, tmpWord);

                }
            }
            // then re-add the group
            mGroupCol.Add(newgroupnum, tmpGroup);
            mIsChanged = true;
        }

        /// <summary>
        /// Returns true if the specified word exists in this word list.
        /// </summary>
        /// <param name="aWord"></param>
        /// <returns></returns>
        public bool WordExists(string aWord) {
            WinAGIException.ThrowIfNotLoaded(this);
            return mWordCol.ContainsKey(aWord);
        }

        /// <summary>
        /// Adds a new word to this word list with the specified group
        /// number, creating the group if it doesn't already exist.
        /// </summary>
        /// <param name="WordText"></param>
        /// <param name="Group"></param>
        public int AddWord(string WordText, int Group) {
            AGIWord NewWord;

            WinAGIException.ThrowIfNotLoaded(this);
            // convert input to lowercase
            WordText = WordText.LowerAGI();
            // check to see if word is already in collection,
            if (mWordCol.ContainsKey(WordText)) {
                WinAGIException wex = new(LoadResString(513)) {
                    HResult = WINAGI_ERR + 513
                };
                throw wex;
            }
            if (Group < 0 || Group > MAX_GROUP_NUM) {
                WinAGIException wex = new(LoadResString(514)) {
                    HResult = WINAGI_ERR + 514
                };
                throw wex;
            }
            if (!GroupExists(Group)) {
                AddGroup(Group);
            }
            mGroupCol[Group].AddWordToGroup(WordText);
            // add it to the main word list
            NewWord.WordText = WordText;
            NewWord.Group = Group;
            mWordCol.Add(WordText, NewWord);
            mIsChanged = true;
            return mWordCol.IndexOfKey(WordText);
        }

        /// <summary>
        /// Removes the specified word from this word list.
        /// </summary>
        /// <param name="aWord"></param>
        public void RemoveWord(string aWord) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (!mWordCol.TryGetValue(aWord, out AGIWord value)) {
                WinAGIException wex = new(LoadResString(516)) {
                    HResult = WINAGI_ERR + 516
                };
                throw wex;
            }
            // delete this word from its assigned group by Group number
            WordGroup tmpGroup = GroupByNumber(value.Group);
            tmpGroup.DeleteWordFromGroup(aWord);
            mWordCol.Remove(aWord);
            mIsChanged = true;
        }

        public int WordIndex(string aWord) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (!mWordCol.TryGetValue(aWord, out AGIWord value)) {
                throw new ArgumentException("word does not exist");
            }
            return mWordCol.IndexOfKey(aWord);
        }
        #endregion

        #region Enumeration
        public WordNumEnum GetEnumerator() {
            return new WordNumEnum(mWordCol);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<AGIWord> IEnumerable<AGIWord>.GetEnumerator() {
            return (IEnumerator<AGIWord>)GetEnumerator();
        }

        /// <summary>
        /// Implements enumeration for the WordList class.
        /// </summary>
        public class WordNumEnum : IEnumerator<AGIWord> {
            public SortedList<string, AGIWord> _words;
            int position = -1;
            public WordNumEnum(SortedList<string, AGIWord> list) {
                _words = list;
            }
            object IEnumerator.Current => Current;
            public AGIWord Current {
                get {
                    try {
                        return _words.Values[position];
                    }
                    catch (IndexOutOfRangeException) {

                        throw new InvalidOperationException();
                    }
                }
            }
            public bool MoveNext() {
                position++;
                return (position < _words.Count);
            }
            public void Reset() {
                position = -1;
            }
            public void Dispose() {
                _words = null;
            }
        }
        #endregion

        /// <summary>
        /// Custom Comparer function that prioritizes words starting
        /// with non-letters before words that start with letters
        /// ('a' - 'z'); within each sub-group, words are sorted by
        /// standard string comparison.
        /// </summary>
        public class AGIWordComparer : Comparer<string> {
            public override int Compare(string x, string y) {
                if (x is null || x.Length == 0) {
                    return -1;
                }
                if (y is null || y.Length == 0) {
                    return 1;
                }
                // words starting with non-letters or extended characters always come before
                // characters a-z
                bool xLtr = x[0] >= 'a' && x[0] <= 'z';
                bool yLtr = y[0] >= 'a' && y[0] <= 'z';
                // if both are 'letters' or both are 'extended/non-letter'
                // sort them normally
                if (xLtr == yLtr) {
                    int retval = string.CompareOrdinal(x, y);
                    return retval;
                }
                else {
                    // extended/non-letter always comes first
                    if (xLtr) {
                        return 1;
                    }
                    else {
                        return -1;
                    }
                }
            }
        }
    }
}