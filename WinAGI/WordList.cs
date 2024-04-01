using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Commands;
using static WinAGI.Common.Base;
using System.IO;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Drawing;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace WinAGI.Engine {
    /// <summary>
    /// Collection of words (agiWord objects).
    /// </summary>
    public class WordList : IEnumerable<WordGroup> {
        SortedList<string, AGIWord> mWordCol;
        SortedList<int, WordGroup> mGroupCol;
        string mResFile = "";
        string mDescription = "";
        bool mInGame;
        AGIGame parent;
        bool mIsDirty;
        bool mWriteProps;
        bool mLoaded;
        int mErrLvl = 0;
        readonly string strErrSource = "WinAGI.AGIWordList";

        /// <summary>
        /// 
        /// </summary>
        public WordList() {
            // initialize the collections
            mWordCol = [];
            mGroupCol = [];
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

        /// <summary>
        /// FileType = 0 means AGI WORDS.TOK file, 1 means SCUMM SVE format.
        /// </summary>
        public void Export(string ExportFile, int FileType = 0, bool ResetDirty = true) {
            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
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
        public int ErrLevel => mErrLvl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="LoadFile"></param>
        /// <returns></returns>
        bool LoadSierraFile(string LoadFile) {
            byte bytHigh, bytLow, bytExt;
            int lngPos;
            byte[] bytData = [];
            string strThisWord;
            string strPrevWord;
            byte[] bytVal = new byte[1];
            int lngGrpNum;
            byte bytPrevWordCharCount;
            FileStream fsWords;

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
                // also, why not an error if empty? or should it return an empty resource?
                fsWords.Dispose();
                return false;
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
                    mErrLvl = 1;
                }
                else {
                    mLoaded = false;
                    // invalid words.tok file!
                    WinAGIException wex = new(LoadResString(529)) {
                        HResult = WINAGI_ERR + 529,
                    };
                    throw wex;
                }
            }
            // for first word, there is no previous word
            strPrevWord = "";
            // read character Count for first word
            bytPrevWordCharCount = bytData[lngPos];
            lngPos++;
            // continue loading words until end of resource is reached
            while (lngPos < bytData.Length) {
                strThisWord = "";
                if (bytPrevWordCharCount > 0) {
                    strThisWord = Left(strPrevWord, bytPrevWordCharCount);
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
                        strThisWord += parent.agCodePage.GetString(bytVal);
                    }
                    lngPos++;
                    // continue until last character (indicated by flag) or
                    // endofresource is reached
                }
                while ((bytVal[0] < 0x80) && (lngPos < bytData.Length));
                // if end of file is reached before 0x80,
                if (lngPos >= bytData.Length) {
                    // invalid words.tok file ending
                    // TODO: add support for another warning level
                    mErrLvl |= 2;
                    break;
                }
                // add last character (after stripping off flag)
                bytVal[0] ^= 0xFF;
                strThisWord += parent.agCodePage.GetString(bytVal);
                // check for upper case (allowed, but not useful)
                if (strThisWord.Any(char.IsUpper)) {
                    mErrLvl |= 4;
                }
                strThisWord = strThisWord.ToLower();
                lngGrpNum = (bytData[lngPos] << 8) + bytData[lngPos + 1];
                // set pointer to next word
                lngPos += 2;
                // skip if same as previous word (unlikely, but 
                // it would cause an exception, if it did happen)
                if (strThisWord != strPrevWord) {
                    if (mWordCol.ContainsKey(strThisWord)) {
                        // remove the old one so a duplicate isn't
                        // added, which would cause an exception
                        RemoveWord(strThisWord);
                    }
                    AddWord(strThisWord, lngGrpNum);
                    // this word is now the previous word
                    strPrevWord = strThisWord;
                }
                bytPrevWordCharCount = bytData[lngPos];
                lngPos++;
            }
            // TODO: if default words/groups not included, should they be added?
            // or leave them out? (I think leaving them out is correct approach)
            //// if no words
            //if (mWordCol.Count == 0) {
            //    //add default words
            //    AddWord("a", 0);
            //    AddWord("anyword", 1);
            //    AddWord("rol", 9999);
            //}
            ////if reserved groups are empty, add default words
            //if (!GroupExists(0)) {
            //    AddWord("a", 0);
            //}
            //if (!GroupExists(1)) {
            //    AddWord("anyword", 1);
            //}
            //if (!GroupExists(9999)) {
            //    AddWord("rol", 9999);
            //}
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wordlist"></param>
        public void Clone(WordList wordlist) {
            //copies word list from NewWords to
            //this word list
            int lngGrpNum;
            string strWord;
            WordGroup tmpGroup;
            AGIWord tmpWord;
            //if source wordlist is not loaded
            if (!wordlist.Loaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
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
            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // return this group by it's number (key value)
            return mGroupCol[groupNum];
        }

        /// <summary>
        /// Returns true if groupnum exists in this wordlist.
        /// </summary>
        /// <param name="groupnumber"></param>
        /// <returns></returns>
        public bool GroupExists(int groupnumber) {
            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // if this group exists, it returns true
            return mGroupCol.ContainsKey(groupnumber);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDirty {
            get {
                if (!mLoaded) {
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
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
                if (!mLoaded) {
                    //error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
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
            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
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
                if (!mLoaded) {
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                return mWordCol.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aWord"></param>
        /// <returns></returns>
        public bool WordExists(string aWord) {
            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            return mWordCol.ContainsKey(aWord);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="GroupNumber"></param>
        public void AddGroup(int GroupNumber) {
            WordGroup tmpGroup;

            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
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
            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
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
                if (!mLoaded) {
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                return mGroupCol.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="GroupNumber"></param>
        public void RemoveGroup(int GroupNumber) {
            int i;
            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
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

            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
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
            string strCurWord;
            string strPrevWord = "";
            byte intPrevWordCharCount;
            bool blnWordsMatch;
            string strFirstLetter;
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
                strFirstLetter = "a";
                intLetterIndex[1] = 52;
                foreach (AGIWord tmpWord in mWordCol.Values) {
                    //get next word
                    strCurWord = tmpWord.WordText;
                    //if first letter is not current first letter, AND it is 'b' through 'z'
                    //(this ensures non letter words (such as numbers) are included in the //a// section)
                    if ((strCurWord[0] != strFirstLetter[0]) && (strCurWord[0] >= 97) && (strCurWord[0] <= 122)) {
                        //reset index pointer
                        strFirstLetter = Left(strCurWord, 1);
                        intLetterIndex[(byte)strFirstLetter[0] - 96] = (int)fsWords.Position;
                    }
                    //calculate number of characters that are common to previous word
                    intPrevWordCharCount = 0;
                    i = 1;
                    blnWordsMatch = true;
                    do {
                        //if not at end of word, AND current position is not longer than previous word,
                        if ((i <= strCurWord.Length) && (strPrevWord.Length >= i)) {
                            //if characters at this position match,
                            if (strPrevWord[i] == strCurWord[i]) {
                                //increment Count
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
                    while (!blnWordsMatch);
                    //write number of characters from previous word
                    fsWords.WriteByte(intPrevWordCharCount);
                    //set previous word Value to this word
                    strPrevWord = strCurWord;
                    //strip off characters that are same as previous word
                    strCurWord = Right(strCurWord, strCurWord.Length - intPrevWordCharCount);
                    //if there are two or more characters to write,
                    if (strCurWord.Length > 1) {
                        //write all but last character
                        for (i = 0; i < strCurWord.Length - 1; i++) {
                            //check for extended characters
                            if ((byte)strCurWord[i] > 127) {
                                //add marker
                                fsWords.WriteByte((byte)0);
                                //adjust character down by 128
                                CurByte = (byte)(0x7F ^ ((byte)strCurWord[i] - (byte)128));
                            }
                            else {
                                //encrypt character before writing it
                                CurByte = (byte)(0x7F ^ (byte)strCurWord[i]);
                            }
                            // add the encrypted character
                            fsWords.WriteByte(CurByte);
                        }
                    }

                    //check for extended characters
                    if ((byte)strCurWord[^1] > 127) {
                        //add marker
                        fsWords.WriteByte((byte)0);
                        //adjust character down by 128
                        CurByte = (byte)(0x80 + (0x7F ^ ((byte)strCurWord[^1] - (byte)128)));
                    }
                    else {
                        //encrypt character, and set flag 0x80 for last char
                        CurByte = (byte)(0x80 + (0x7F ^ (byte)strCurWord[^1]));
                    }
                    // add the encrypted end character
                    fsWords.WriteByte(CurByte);
                    //write group number (stored as 2-byte word; high byte first)
                    CurByte = (byte)(tmpWord.Group / 256);
                    fsWords.WriteByte(CurByte);
                    CurByte = (byte)(tmpWord.Group % 256);
                    fsWords.WriteByte(CurByte);
                }
                //add null character to end of file
                fsWords.WriteByte(0);
                //reset file pointer to start
                fsWords.Seek(0, SeekOrigin.Begin);
                //write index values for all 26 letters
                for (i = 1; i <= 26; i++) {
                    //(two byte word, high byte first
                    CurByte = (byte)(intLetterIndex[i] / 256);
                    fsWords.WriteByte(CurByte);
                    CurByte = (byte)(intLetterIndex[i] % 256);
                    fsWords.WriteByte(CurByte);
                }
                //close file,
                fsWords.Dispose();
                //copy tempfile to CompileFile
                File.Move(strTempFile, CompileFile, true);
            }
            catch (Exception e) {
                //raise the error
                WinAGIException wex = new(LoadResString(672).Replace(ARG1, e.HResult.ToString()) + ": " + e.Message) {
                    HResult = WINAGI_ERR + 672,
                };
                wex.Data["exception"] = e;
                wex.Data["ID"] = "WORDS.TOK";
                throw wex;
            }
        }
        void CompileSVE(string CompileFile) {
            // ScummVM Extended Word List File format
            //
            // header (1st line): Unofficial extended format to support ASCII range of 128-255
            // wordtext & chr(0) & Cstr(groupnum) & newline(CRLF)

            string strTempFile;
            int i;

            //if no filename passed,
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
            //create temp file
            strTempFile = Path.GetTempFileName();
            try {
                //open the file
                StreamWriter swWords = new(strTempFile);
                //print header
                swWords.WriteLine("Unofficial extended format to support ASCII range of 128-255");

                //add all words, in alphabetical order
                for (i = 0; i < this.WordCount; i++) {
                    swWords.WriteLine(this[i].WordText + (char)0 + this[i].Group);
                }
                swWords.Close();
                //copy tempfile to CompileFile
                File.Move(strTempFile, CompileFile, true);
            }
            catch (Exception) {
                throw;
            }
        }
        public void Load(string LoadFile = "") {
            //this function loads words for the game
            //if loading from a Sierra game, it extracts words
            //from the WORDS.TOK file;
            //if not in a game, LoadFile must be specified

            //if already loaded,
            if (mLoaded) {
                //do nothing
                return;
            }
            if (mInGame) {
                // always set loaded flag for ingame resource regardless of error status
                mLoaded = true;
                //use default filename
                LoadFile = parent.agGameDir + "WORDS.TOK";
                //attempt to load
                if (!LoadSierraFile(LoadFile)) {
                    //error

                    WinAGIException wex = new(LoadResString(529)) {
                        HResult = WINAGI_ERR + 529
                    };
                    throw wex;
                }
                //get description, if there is one
                mDescription = parent.agGameProps.GetSetting("WORDS.TOK", "Description", "");
            }
            else {
                //if not in a game, must have a valid filename
                if (LoadFile.Length == 0) {
                    //not in game; return error

                    WinAGIException wex = new(LoadResString(599)) {
                        HResult = WINAGI_ERR + 599
                    };
                    throw wex;
                }
                //verify file exists
                if (!File.Exists(LoadFile)) {
                    //error
                    WinAGIException wex = new(LoadResString(524).Replace(ARG1, LoadFile)) {
                        HResult = WINAGI_ERR + 524,
                    };
                    wex.Data["missingfile"] = LoadFile;
                    throw wex;
                }
                //try sierra format
                if (!LoadSierraFile(LoadFile)) {
                    //error
                    WinAGIException wex = new(LoadResString(529)) {
                        HResult = WINAGI_ERR + 529
                    };
                    throw wex;
                }
                // set loaded flag
                mLoaded = true;
                //save filename
                mResFile = LoadFile;
            }
            //reset dirty flag
            mIsDirty = false;
            return;
        }
        public WordGroup Group(int Index) {
            //returns a group by its index (NOT the same as group number)
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //if invalid index
            if (Index < 0 || Index > mGroupCol.Count - 1) {
                //error

                WinAGIException wex = new(LoadResString(588)) {
                    HResult = WINAGI_ERR + 588
                };
                throw wex;
            }
            //access the group by its index
            return mGroupCol.Values[Index];
        }
        public void AddWord(string WordText, int Group) {
            AGIWord NewWord;
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // convert to byte code in correct codepage
            // WordText = UnicodeToCP(WordText, parent.agCodePage);
            // WordText = Encoding.Convert(Encoding.Unicode, parent.agCodePage, Encoding.Unicode.GetBytes(WordText)).ToString();
            // convert input to lowercase
            // TODO: do I need custom lowercase? because extended chars 
            // can be uppercase without causing problems
            WordText = WordText.ToLower();
            //check to see if word is already in collection,
            if (mWordCol.ContainsKey(WordText)) {

                WinAGIException wex = new(LoadResString(579)) {
                    HResult = WINAGI_ERR + 579
                };
                throw wex;
            }
            //if group number is invalid (negative, or > max)
            if (Group < 0 || Group > MAX_GROUP_NUM) {
                //error

                WinAGIException wex = new(LoadResString(581)) {
                    HResult = WINAGI_ERR + 581
                };
                throw wex;
            }
            //if this group does not yet exist,
            if (!GroupExists(Group)) {
                //add this group
                AddGroup(Group);
            }
            //add word to the group
            mGroupCol[Group].AddWordToGroup(WordText);
            //now add it to the main word collection
            NewWord.WordText = WordText;
            NewWord.Group = Group;
            //add it, using the word itself as the key
            mWordCol.Add(WordText, NewWord);
            //set dirty flag
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