using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.Base;
using static WinAGI.Common.Base;
using System.IO;
using System.Linq;
using System.Text;

namespace WinAGI.Engine {
    /// <summary>
    /// Collection of words (agiWord objects).
    /// </summary>
    public class WordList : IEnumerable<WordGroup> {
        /// <summary>
        /// 
        /// </summary>
        class AGIWordComparer : Comparer<string> {
            public override int Compare(string x, string y) {
                // words starting with numbers or extended characters always come before
                // characters a-z
                bool x1 = x[0] > 127 || (x[0] >= 48 && x[0] <= 57);
                bool y1 = y[0] > 127 || (y[0] >= 48 && y[0] <= 57);
                // if both are 'regular' or both are 'extended/num'
                // sort them normally
                if (x1 == y1) {
                    int retval = string.CompareOrdinal(x, y);
                    return retval;
                }
                else {
                    // the 'ext/num' always comes first
                    if (x1) {
                        return -1;
                    }
                    else {
                        {
                            return 1;
                        }
                    }
                }
            }
        }

        SortedList<string, AGIWord> mWordCol;
        SortedList<int, WordGroup> mGroupCol;
        string mResFile = "";
        string mDescription = "";
        bool mInGame;
        AGIGame parent;
        Encoding mCodePage = Encoding.GetEncoding(437);
        bool mIsDirty;
        bool mWriteProps;
        bool mLoaded;
        int mErrLevel = 0;
        // 0 = no errors or warnings
        // 1 = nonstandard letter index
        // 2 = invalid eof marker
        // 4 = upper case detected
        // 8 = empty word file
        // 16 = corrupt index file

        readonly string strErrSource = "WinAGI.AGIWordList";

        /// <summary>
        /// Initialize a new WordList, not in a game
        /// </summary>
        public WordList() {
            // initialize the word collection with a custom sort order
            mWordCol = new(new AGIWordComparer());
            mGroupCol = [];
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
        /// This constructor is only called for a WordList
        /// that is part of a game.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Loaded"></param>
        internal WordList(AGIGame parent, bool Loaded = false) {
            mWordCol = [];
            mGroupCol = [];

            mInGame = true;
            this.parent = parent;
            // if loaded property is passed, set loaded flag as well
            mLoaded = Loaded;
            // also sets the default name to 'WORDS.TOK'
            mResFile = this.parent.agGameDir + "WORDS.TOK";
        }

        public Encoding CodePage {
            get => parent is null ? mCodePage : parent.agCodePage;
            set {
                if (parent is null) {
                    // confirm new codepage is supported; ignore if it is not
                    switch (value.CodePage) {
                    case 437 or 737 or 775 or 850 or 852 or 855 or 857 or 860 or
                         861 or 862 or 863 or 865 or 866 or 869 or 858:
                        mCodePage = value;
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
        /// FileType = 0 means AGI WORDS.TOK file, 1 means SCUMM SVE format.
        /// </summary>
        public void Export(string ExportFile, int FileType = 0, bool ResetDirty = true) {
            WinAGIException.ThrowIfNotLoaded(this);
            try {
                if (FileType == 0) {
                    Compile(ExportFile);
                }
                else {
                    CompileSVE(ExportFile);
                }
            }
            catch {
                throw;
            }
            if (!mInGame) {
                if (ResetDirty) {
                    mIsDirty = false;
                }
                mResFile = ExportFile;
            }
            return;
        }

        /// <summary>
        /// Used by SetWords method to retrieve words one by one.
        /// NOTE: no error checking is done, because ONLY
        /// SetWords uses this function, and it ensures
        /// Index is valid.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        internal AGIWord ItemByIndex(int Index) {
            return mWordCol.Values[Index];
        }

        public bool Loaded {
            get { return mLoaded; }
        }

        /// <summary>
        /// Provides access to current error level of the WordList to
        /// get feedback on errors in the sound data.
        /// </summary>
        public int ErrLevel => mErrLevel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LoadFile"></param>
        /// <returns></returns>
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

            try {
                fsWords = new FileStream(LoadFile, FileMode.Open);
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, LoadFile)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = e;
                wex.Data["badfile"] = LoadFile;
                throw wex;
            }
            // reset word and group columns
            mWordCol = [];
            mGroupCol = [];
            // if no data,
            if (fsWords.Length == 0) {
                // TODO: why does this need to be disposed? other resources don't do this
                fsWords.Dispose();
                return 8;
            }
            // read in entire resource
            Array.Resize(ref bytData, (int)fsWords.Length);
            fsWords.Read(bytData);
            // start at beginning of words section
            // (words.tok file uses MSLS format for two byte-word data)
            bytHigh = bytData[0];
            bytLow = bytData[1];
            lngPos = (bytHigh << 8) + bytLow;
            if (lngPos != 52) {
                // words.tok file is corrupt or invalid;
                // don't assume it's bad just yet - if the target
                // byte is zero, give it a try
                if (bytData[lngPos] == 0) {
                    // note the problem in a warning
                    retval = 1;
                }
                else {
                    return 16;
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
                        sbThisWord.Append(parent.agCodePage.GetString(bytVal));
                    }
                    lngPos++;
                    // continue until last character (indicated by flag) or
                    // endofresource is reached
                }
                while ((bytVal[0] < 0x80) && (lngPos < bytData.Length));
                // if end of file is reached before 0x80,
                if (lngPos >= bytData.Length) {
                    // invalid words.tok file ending
                    mErrLevel |= 2;
                    break;
                }
                // add last character (after stripping off flag)
                bytVal[0] ^= 0xFF;
                sbThisWord.Append(parent.agCodePage.GetString(bytVal));
                sThisWord = sbThisWord.ToString();
                // check for ascii upper case (allowed, but not useful)
                if (sThisWord.Any(ch => (ch >= 65 && ch <= 90))) {
                    mErrLevel |= 4;
                }
                sThisWord = sThisWord.ToLower();
                lngGrpNum = (bytData[lngPos] << 8) + bytData[lngPos + 1];
                // set pointer to next word
                lngPos += 2;
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
                bytPrevWordCharCount = bytData[lngPos];
                lngPos++;
            }
            return retval;
        }

        /// <summary>
        /// Copies word list from NewWords to this word list.
        /// </summary>
        /// <param name="wordlist"></param>
        public void Clone(WordList wordlist) {
            //
            int lngGrpNum;
            string strWord;
            WordGroup tmpGroup;
            AGIWord tmpWord;

            WinAGIException.ThrowIfNotLoaded(this);
            Clear();
            foreach (WordGroup oldGroup in wordlist) {
                tmpGroup = new WordGroup();
                lngGrpNum = oldGroup.GroupNum;
                tmpGroup.GroupNum = lngGrpNum;
                mGroupCol.Add(lngGrpNum, tmpGroup);
            }
            foreach (AGIWord oldWord in wordlist.mWordCol.Values) {
                strWord = oldWord.WordText;
                lngGrpNum = oldWord.Group;
                tmpWord.WordText = strWord;
                tmpWord.Group = lngGrpNum;
                mWordCol.Add(strWord, tmpWord);
                mGroupCol[lngGrpNum].AddWordToGroup(strWord);
            }
            mDescription = wordlist.Description;
            mIsDirty = wordlist.IsDirty;
            mWriteProps = wordlist.WriteProps;
            mResFile = wordlist.ResFile;
            mLoaded = true;
        }

        /// <summary>
        /// 
        /// </summary>
        internal bool WriteProps { get { return mWriteProps; } }

        /// <summary>
        /// 
        /// </summary>
        public string Description {
            get {
                return mDescription;
            }
            set {
                // limit description to 1K
                value = Left(value, 1024);
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
        /// Only used by SetWord method.
        /// </summary>
        internal bool InGame {
            get {
                return mInGame;
            }
            set {
                mInGame = value;
            }
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public void Clear() {
            //clears the word group, and sets up a blank list
            if (!mLoaded) {
                // ignore
                return;
            }
            mGroupCol = [];
            mWordCol = [];
            mDescription = "";
            mIsDirty = true;
        }

        /// <summary>
        /// Returns a group by its group number.
        /// </summary>
        /// <param name="groupNum"></param>
        /// <returns></returns>
        public WordGroup GroupN(int groupNum) {
            WinAGIException.ThrowIfNotLoaded(this);
            // return this group by it's number (key value)
            return mGroupCol[groupNum];
        }

        /// <summary>
        /// Returns true if groupnum exists in this wordlist.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public bool GroupExists(int groupnumber) {
            WinAGIException.ThrowIfNotLoaded(this);
            // if this group exists, it returns true
            return mGroupCol.ContainsKey(groupnumber);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDirty {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return (mIsDirty || (mWriteProps && mInGame));
            }
        }

        /// <summary>
        /// Accesses a word by string or index number.
        /// </summary>
        /// <param name="vKeyIndex"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public AGIWord this[dynamic vKeyIndex] {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                // only allow string or integer number
                if ((vKeyIndex is int) || (vKeyIndex is byte) || (vKeyIndex is short)) {
                    if (vKeyIndex < 0 || vKeyIndex > mWordCol.Count - 1) {
                        throw new IndexOutOfRangeException();
                    }
                    return mWordCol.Values[vKeyIndex];
                }
                else if (vKeyIndex is string) {
                    // a string could be a number passed as a string, OR an actual word?
                    // user has to make sure it's not a number passed as a string...
                    //retrieve via string key - actual word, which is the key
                    if (mWordCol.ContainsKey(vKeyIndex)) {
                        return mWordCol[vKeyIndex];
                    }
                    else {
                        throw new IndexOutOfRangeException();
                    }
                }
                else {
                    throw new ArgumentException("invalid key/index");
                }
            }
        }

        /// <summary>
        /// 
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
            mIsDirty = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Unload() {
            if (!mLoaded) {
                // ignore
                return;
            }
            Clear();
            mLoaded = false;
            mWriteProps = false;
            mIsDirty = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public int WordCount {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mWordCol.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aWord"></param>
        /// <returns></returns>
        public bool WordExists(string aWord) {
            WinAGIException.ThrowIfNotLoaded(this);
            return mWordCol.ContainsKey(aWord);
        }

        /// <summary>
        /// 
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
            mIsDirty = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aWord"></param>
        /// 
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
            mIsDirty = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public int GroupCount {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mGroupCol.Count;
            }
        }

        /// <summary>
        /// 
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
            // some of the objects
            for (i = mWordCol.Count - 1; i >= 0; i--) {
                if (mWordCol.Values[i].Group == GroupNumber) {
                    mWordCol.RemoveAt(i);
                }
            }
            mGroupCol.Remove(GroupNumber);
            mIsDirty = true;
        }

        /// <summary>
        /// 
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
            // CAN'T use forach, because we are adding to the collection
            for (i = 0; i < mWordCol.Count; i++) {
                if (mWordCol.Values[i].Group == oldgroupnum) {
                    AGIWord tmpWord = mWordCol.Values[i];
                    tmpWord.Group = newgroupnum;
                }
            }
            // then re-add the group
            mGroupCol.Add(newgroupnum, tmpGroup);
            mIsDirty = true;
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
            if (!mIsDirty && CompileFile.Equals(mResFile, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            if (mGroupCol.Count == 0) {
                // TODO: why can't a completely empty list work?
                WinAGIException wex = new(LoadResString(672).Replace(ARG1, "no word groups to add")) {
                    HResult = WINAGI_ERR + 672
                };
                throw wex;
            }
            strTempFile = Path.GetTempFileName();
            FileStream fsWords;
            try {
                fsWords = new FileStream(strTempFile, FileMode.OpenOrCreate);
                // write letter index
                for (i = 0; i <= 51; i++) {
                    fsWords.WriteByte(0);
                }
                strFirstLetter = (byte)'a';
                intLetterIndex[0] = 52;
                foreach (AGIWord tmpWord in mWordCol.Values) {
                    // get next word
                    if (parent is null) {
                        strCurWord = mCodePage.GetBytes(tmpWord.WordText);
                    }
                    else {
                        strCurWord = parent.agCodePage.GetBytes(tmpWord.WordText);
                    }
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
                File.Move(strTempFile, CompileFile, true);
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
        /// 
        /// </summary>
        /// <param name="CompileFile"></param>
        void CompileSVE(string CompileFile) {
            // ScummVM Extended Word List File format
            //
            // header (1st line): Unofficial extended format to support ASCII range of 128-255
            // wordtext & chr(0) & Cstr(groupnum) & newline(CRLF)

            string strTempFile;
            int i;

            if (CompileFile.Length == 0) {
                WinAGIException wex = new(LoadResString(616)) {
                    HResult = WINAGI_ERR + 616
                };
                throw wex;
            }
            if (mWordCol.Count == 0) {
                //error
                WinAGIException wex = new(LoadResString(672).Replace(ARG1, "no words to add")) {
                    HResult = WINAGI_ERR + 672
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
                File.Move(strTempFile, CompileFile, true);
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
        /// Loads words for the game. If loading from a Sierra game, it extracts words
        /// from the WORDS.TOK file. If not in a game, LoadFile must be specified
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
            // verify file exists
            if (!File.Exists(LoadFile)) {
                WinAGIException wex = new(LoadResString(606).Replace(ARG1, LoadFile)) {
                    HResult = WINAGI_ERR + 606,
                };
                wex.Data["missingfile"] = LoadFile;
                wex.Data["ID"] = "WORDS.TOK";
                throw wex;
            }
            // check for readonly
            if ((File.GetAttributes(LoadFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                WinAGIException wex = new(LoadResString(700).Replace(ARG1, LoadFile)) {
                    HResult = WINAGI_ERR + 700,
                };
                wex.Data["badfile"] = LoadFile;
                throw wex;
            }
            try {
                mErrLevel = LoadSierraFile(LoadFile);
            }
            catch {
                throw;
            }
            finally {
                // always set loaded flag regardless of error status
                mLoaded = true;
                if (mInGame) {
                    mDescription = parent.agGameProps.GetSetting("WORDS.TOK", "Description", "", true);
                }
                else {
                    mResFile = LoadFile;
                }
                mIsDirty = false;
            }
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
            //access the group by its index
            return mGroupCol.Values[Index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="WordText"></param>
        /// <param name="Group"></param>
        public void AddWord(string WordText, int Group) {
            AGIWord NewWord;

            WinAGIException.ThrowIfNotLoaded(this);
            // convert input to lowercase
            WordText = LowerAGI(WordText);
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
            mIsDirty = true;
        }


        public WGrpEnum GetEnumerator() {
            return new WGrpEnum(mGroupCol);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<WordGroup> IEnumerable<WordGroup>.GetEnumerator() {
            return (IEnumerator<WordGroup>)GetEnumerator();
        }
    }

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
}