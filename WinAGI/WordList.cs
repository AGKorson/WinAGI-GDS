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
    public class WordList : IEnumerable<WordGroup> {
        #region Members
        SortedList<string, AGIWord> mWordCol;
        SortedList<int, WordGroup> mGroupCol;
        string mResFile = "";
        string mDescription = "";
        bool mInGame;
        AGIGame parent;
        Encoding mCodePage = Encoding.GetEncoding(Base.CodePage.CodePage);
        bool mIsChanged;
        bool mLoaded;
        int mErrLevel = 0;
        // 0 = no errors or warnings
        // 1 = nonstandard letter index
        // 2 = invalid eof marker
        // 4 = upper case detected
        // 8 = empty word file
        // 16 = corrupt index file
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
            mCodePage = Encoding.GetEncoding(parent.agCodePage.CodePage);
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
            try {
                Load(filename);
            }
            catch {
                // blank the list
                mResFile = "";
                mWordCol = new(new AGIWordComparer());
                mGroupCol = [];
                mLoaded = true;
                // return the error
                throw;
            }
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
        public Encoding CodePage {
            get => parent is null ? mCodePage : parent.agCodePage;
            set {
                if (parent is null) {
                    // confirm new codepage is supported
                    switch (value.CodePage) {
                    case 437 or 737 or 775 or 850 or 852 or 855 or 857 or 860 or
                         861 or 862 or 863 or 865 or 866 or 869 or 858:
                        mCodePage = Encoding.GetEncoding(value.CodePage);
                        break;
                    default:
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
        /// Gets the current error level of the WordList.
        /// </summary>
        public int ErrLevel {
            get {
                return mErrLevel;
            }
        }

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
                WinAGIException.ThrowIfNotLoaded(this);
                return mIsChanged;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads words into this list from a WORDS.TOK file. If not in a game,
        /// LoadFile must be specified.
        /// </summary>
        /// <param name="LoadFile"></param>
        public void Load(string LoadFile = "") {
            if (mLoaded) {
                return;
            }
            if (mInGame) {
                LoadFile = mResFile;
            }
            if (LoadFile.Length == 0) {
                WinAGIException wex = new(LoadResString(599)) {
                    HResult = WINAGI_ERR + 599
                };
                throw wex;
            }
            if (!File.Exists(LoadFile)) {
                WinAGIException wex = new(LoadResString(606).Replace(ARG1, LoadFile)) {
                    HResult = WINAGI_ERR + 606,
                };
                wex.Data["missingfile"] = LoadFile;
                wex.Data["ID"] = "WORDS.TOK";
                throw wex;
            }
            if ((File.GetAttributes(LoadFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                WinAGIException wex = new(LoadResString(700).Replace(ARG1, LoadFile)) {
                    HResult = WINAGI_ERR + 700,
                };
                wex.Data["badfile"] = LoadFile;
                throw wex;
            }
            mErrLevel = LoadSierraFile(LoadFile);
            if (mInGame) {
                mDescription = parent.agGameProps.GetSetting("WORDS.TOK", "Description", "", true);
            }
            else {
                mResFile = LoadFile;
            }
            mIsChanged = false;
        }

        /// <summary>
        /// Extracts the words and group information from this word list's file.
        /// Returns an error code specifying whether any warnings or errors were
        /// encountered when the file is loaded.
        /// </summary>
        /// <param name="LoadFile"></param>
        /// <returns>0 = no errors or warnings<br />
        /// 1 = abnormal index<br />
        /// 2 = unexpected end of file<br />
        /// 4 = upper case character in word<br />
        /// 8 = no data/empty file<br />
        /// -24 = invalid index - can't read file<br />
        /// -25 = file access error, unable to read file
        /// </returns>
        private int LoadSierraFile(string LoadFile) {
            byte bytHigh, bytLow, bytExt;
            int lngPos, retval = 0;
            byte[] bytData = [];
            StringBuilder sbThisWord;
            string sThisWord;
            string strPrevWord;
            byte[] bytVal = new byte[1];
            int lngGrpNum;
            byte bytPrevWordCharCount;
            FileStream fsWords;

            mLoaded = true;
            mWordCol = new(new AGIWordComparer());
            mGroupCol = [];
            try {
                fsWords = new(LoadFile, FileMode.Open);
            }
            catch {
                // file access error
                return -25;
            }
            // if no data,
            if (fsWords.Length == 0) {
                // be sure to release the stream
                fsWords.Dispose();
                return 8;
            }
            // read in entire resource
            Array.Resize(ref bytData, (int)fsWords.Length);
            fsWords.Read(bytData);
            // done with the stream
            fsWords.Dispose();
            // start at beginning of words section
            // (words.tok file uses MSLS format for two byte-word data)
            bytHigh = bytData[0];
            bytLow = bytData[1];
            lngPos = (bytHigh << 8) + bytLow;
            if (lngPos != 52) {
                // index is corrupt or invalid;
                // don't assume it's bad just yet - if the target
                // byte is zero, give it a try
                if (bytData[lngPos] == 0) {
                    // note the problem in a warning
                    retval = 1;
                }
                else {
                    // unable to read this file
                    return -24;
                }
            }
            // for first word, there is no previous word
            strPrevWord = "";
            // read character Count for first word
            bytPrevWordCharCount = bytData[lngPos];
            lngPos++;
            // continue loading words until end of resource is reached
            while (lngPos < bytData.Length) {
                if (bytPrevWordCharCount > 0) {
                    sbThisWord = new(strPrevWord[..bytPrevWordCharCount]);
                }
                else {
                    sbThisWord = new();
                }
                do {
                    bytVal[0] = bytData[lngPos];
                    // EXTENDED CHARACTER FORMAT CHECK:
                    if (bytVal[0] == 0) {
                        // the next char is the actual character,
                        // and is extended
                        lngPos++;
                        bytVal[0] = bytData[lngPos];
                        bytExt = 128;
                    }
                    else {
                        bytExt = 0;
                    }
                    if (bytVal[0] < 0x80) {
                        bytVal[0] = (byte)(bytVal[0] ^ 0x7F + bytExt);
                        sbThisWord.Append(CodePage.GetString(bytVal));
                    }
                    lngPos++;
                    // continue until last character (indicated by flag) or
                    // endofresource is reached
                }
                while ((bytVal[0] < 0x80) && (lngPos < bytData.Length));
                // check for end of file
                if (lngPos >= bytData.Length) {
                    // invalid words.tok file ending
                    retval |= 2;
                    break;
                }
                // add last character (after stripping off flag)
                bytVal[0] ^= 0xFF;
                sbThisWord.Append(CodePage.GetString(bytVal));
                sThisWord = sbThisWord.ToString();
                // check for ascii upper case (allowed, but not useful)
                if (sThisWord.Any(ch => (ch >= 65 && ch <= 90))) {
                    retval |= 4;
                }
                sThisWord = sThisWord.LowerAGI();
                lngGrpNum = (bytData[lngPos++] << 8) + bytData[lngPos++];
                // skip if same as previous word (unlikely, but 
                // it would cause an exception, if it did happen)
                if (sThisWord != strPrevWord) {
                    if (mWordCol.ContainsKey(sThisWord)) {
                        // remove the old one so a duplicate isn't
                        // added, which would cause an exception
                        RemoveWord(sThisWord);
                    }
                    AddWord(sThisWord, lngGrpNum);
                    // this word is now the previous word
                    strPrevWord = sThisWord;
                }
                bytPrevWordCharCount = bytData[lngPos++];
            }
            return retval;
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
                // if still no file
                if (SaveFile.Length == 0) {
                    WinAGIException wex = new(LoadResString(615)) {
                        HResult = WINAGI_ERR + 615
                    };
                    throw wex;
                }
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
        }

        /// <summary>
        /// Compiles the word list into a Sierra WORDS.TOK file.
        /// </summary>
        /// <param name="CompileFile"></param>
        void Compile(string CompileFile) {
            int i;
            byte CurByte;
            byte[] strCurWord;
            byte[] strPrevWord = [];
            byte intPrevWordCharCount;
            bool blnWordsMatch;
            byte strFirstLetter;
            int[] intLetterIndex = new int[26];
            string strTempFile;

            if (CompileFile.Length == 0) {
                WinAGIException wex = new(LoadResString(616)) {
                    HResult = WINAGI_ERR + 616
                };
                throw wex;
            }
            if (!mIsChanged && CompileFile.Equals(mResFile, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            strTempFile = Path.GetTempFileName();
            FileStream fsWords;
            try {
                fsWords = new FileStream(strTempFile, FileMode.OpenOrCreate);
                // letter index placeholders
                for (i = 0; i <= 51; i++) {
                    fsWords.WriteByte(0);
                }
                strFirstLetter = (byte)'a';
                intLetterIndex[0] = 52;
                foreach (AGIWord tmpWord in mWordCol.Values) {
                    // get next word
                    strCurWord = CodePage.GetBytes(tmpWord.WordText);
                    // if first letter is not current first letter, AND it is 'b' through 'z'
                    // (this ensures non letter words (such as numbers) are included in the 'a' section)
                    if ((strCurWord[0] != strFirstLetter) && (strCurWord[0] >= 97) && (strCurWord[0] <= 122)) {
                        // reset index pointer
                        strFirstLetter = strCurWord[0];
                        intLetterIndex[strFirstLetter - 97] = (int)fsWords.Position;
                    }
                    // calculate number of characters that are common to previous word
                    intPrevWordCharCount = 0;
                    i = 0;
                    blnWordsMatch = true;
                    do {
                        // if not at end of word, AND current position is not longer than previous word,
                        if ((i < strCurWord.Length - 1) && (i < strPrevWord.Length)) {
                            if (strPrevWord[i] == strCurWord[i]) {
                                intPrevWordCharCount++;
                            }
                            else {
                                blnWordsMatch = false;
                            }
                        }
                        else {
                            blnWordsMatch = false;
                        }
                        i++;
                    }
                    while (blnWordsMatch);
                    fsWords.WriteByte(intPrevWordCharCount);
                    strPrevWord = strCurWord;
                    // strip off characters that are same as previous word
                    strCurWord = strCurWord[intPrevWordCharCount..];
                    if (strCurWord.Length > 1) {
                        // write all but last character
                        for (i = 0; i < strCurWord.Length - 1; i++) {
                            // check for extended characters
                            if (strCurWord[i] > 127) {
                                // add marker
                                fsWords.WriteByte(0);
                                //adjust character down by 128
                                CurByte = (byte)(0x7F ^ (strCurWord[i] - 128));
                            }
                            else {
                                // encrypt character before writing it
                                CurByte = (byte)(0x7F ^ strCurWord[i]);
                            }
                            // add the encrypted character
                            fsWords.WriteByte(CurByte);
                        }
                    }
                    // add last char - first check for extended character
                    if (strCurWord[^1] > 127) {
                        // add marker
                        fsWords.WriteByte(0);
                        //adjust character down by 128
                        CurByte = (byte)(0x80 + (0x7F ^ (strCurWord[^1] - 128)));
                    }
                    else {
                        CurByte = (byte)(0x80 + (0x7F ^ strCurWord[^1]));
                    }
                    // add the encrypted end character
                    fsWords.WriteByte(CurByte);
                    // write group number (stored as 2-byte word; high byte first)
                    CurByte = (byte)(tmpWord.Group / 256);
                    fsWords.WriteByte(CurByte);
                    CurByte = (byte)(tmpWord.Group % 256);
                    fsWords.WriteByte(CurByte);
                }
                // add null character to end of file
                fsWords.WriteByte(0);
                // reset file pointer to start
                fsWords.Seek(0, SeekOrigin.Begin);
                // write index values for all 26 letters
                for (i = 0; i < 26; i++) {
                    // two byte word, high byte first
                    CurByte = (byte)(intLetterIndex[i] / 256);
                    fsWords.WriteByte(CurByte);
                    CurByte = (byte)(intLetterIndex[i] % 256);
                    fsWords.WriteByte(CurByte);
                }
                fsWords.Dispose();
                // copy tempfile to CompileFile
                SafeFileMove(strTempFile, CompileFile, true);
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(672).Replace(ARG1, e.HResult.ToString()) + ": " + e.Message) {
                    HResult = WINAGI_ERR + 672,
                };
                wex.Data["exception"] = e;
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
            //format is:
            // header (1st line): "Unofficial extended format to support ASCII range of 128-255"
            // subsequent lines are words, in alphabetical order
            // in following format:
            //      wordtext + chr(0) + groupnum.ToString() + newline(CRLF)
            string strTempFile;
            int i;

            if (CompileFile.Length == 0) {
                WinAGIException wex = new(LoadResString(616)) {
                    HResult = WINAGI_ERR + 616
                };
                throw wex;
            }
            strTempFile = Path.GetTempFileName();
            try {
                StreamWriter swWords = new(strTempFile);
                swWords.WriteLine("Unofficial extended format to support ASCII range of 128-255");
                //add all words, in alphabetical order
                for (i = 0; i < this.WordCount; i++) {
                    swWords.WriteLine(this[i].WordText + (char)0 + this[i].Group);
                }
                swWords.Close();
                swWords.Dispose();
                SafeFileMove(strTempFile, CompileFile, true);
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(672).Replace(ARG1, e.HResult.ToString()) + ": " + e.Message) {
                    HResult = WINAGI_ERR + 672,
                };
                wex.Data["exception"] = e;
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
            // only loaded views can be cloned
            WinAGIException.ThrowIfNotLoaded(this);

            WordList clonelist = new();
            // clear the defaults
            clonelist.mWordCol = new(new AGIWordComparer());
            clonelist.mGroupCol = [];

            int lngGrpNum;
            string strWord;
            WordGroup tmpGroup;
            AGIWord tmpWord;

            foreach (WordGroup group in this) {
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
            clonelist.mCodePage = Encoding.GetEncoding(mCodePage.CodePage);
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
            foreach (WordGroup group in clonelist) {
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
            mCodePage = Encoding.GetEncoding(clonelist.mCodePage.CodePage);
            mLoaded = true;
        }
        
        /// <summary>
        /// Clears this word list and sets all properties to default values.
        /// </summary>
        public void Clear() {
            WinAGIException.ThrowIfNotLoaded(this);
            mGroupCol = [];
            mWordCol = new(new AGIWordComparer());
            mDescription = "";
            mIsChanged = true;
        }

        /// <summary>
        /// Returns a group by its group number.
        /// </summary>
        /// <param name="groupNum"></param>
        /// <returns></returns>
        public WordGroup GroupN(int groupNum) {
            WinAGIException.ThrowIfNotLoaded(this);
            // return this group by its number (key value)
            return mGroupCol[groupNum];
        }

        /// <summary>
        /// Returns a group by its index (NOT the same as group number).
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public WordGroup Group(int Index) {
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
                WinAGIException wex = new(LoadResString(583)) {
                    HResult = WINAGI_ERR + 583
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
        public void AddWord(string WordText, int Group) {
            AGIWord NewWord;

            WinAGIException.ThrowIfNotLoaded(this);
            // convert input to lowercase
            WordText = WordText.LowerAGI();
            // check to see if word is already in collection,
            if (mWordCol.ContainsKey(WordText)) {
                WinAGIException wex = new(LoadResString(579)) {
                    HResult = WINAGI_ERR + 579
                };
                throw wex;
            }
            if (Group < 0 || Group > MAX_GROUP_NUM) {
                WinAGIException wex = new(LoadResString(581)) {
                    HResult = WINAGI_ERR + 581
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
        }

        /// <summary>
        /// Removes the specified word from this word list.
        /// </summary>
        /// <param name="aWord"></param>
        void RemoveWord(string aWord) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (!mWordCol.TryGetValue(aWord, out AGIWord value)) {
                throw new ArgumentException("word does not exist");
            }
            // delete this word from its assigned group by Group number
            WordGroup tmpGroup = GroupN(value.Group);
            tmpGroup.DeleteWordFromGroup(aWord);
            if (tmpGroup.WordCount == 0) {
                mGroupCol.Remove(tmpGroup.GroupNum);
            }
            mWordCol.Remove(aWord);
            mIsChanged = true;
        }
        #endregion

        #region Enumeration
        public WGrpEnum GetEnumerator() {
            return new WGrpEnum(mGroupCol);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<WordGroup> IEnumerable<WordGroup>.GetEnumerator() {
            return (IEnumerator<WordGroup>)GetEnumerator();
        }

        /// <summary>
        /// Implements enumeration for the WordList class.
        /// </summary>
        public class WGrpEnum : IEnumerator<WordGroup> {
            public SortedList<int, WordGroup> _groups;
            int position = -1;
            public WGrpEnum(SortedList<int, WordGroup> list) {
                _groups = list;
            }
            object IEnumerator.Current => Current;
            public WordGroup Current {
                get {
                    try {
                        return _groups.Values[position];
                    }
                    catch (IndexOutOfRangeException) {

                        throw new InvalidOperationException();
                    }
                }
            }
            public bool MoveNext() {
                position++;
                return (position < _groups.Count);
            }
            public void Reset() {
                position = -1;
            }
            public void Dispose() {
                _groups = null;
            }
        }
        #endregion

        /// <summary>
        /// Custom Comparer function that prioritizes words starting
        /// with non-letters ('a' - 'z') before words that start with
        /// lettters; within each sub-group, words are sorted by
        /// standard string comparison.
        /// </summary>
        class AGIWordComparer : Comparer<string> {
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