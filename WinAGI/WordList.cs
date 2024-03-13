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

namespace WinAGI.Engine {
    public class WordList : IEnumerable<WordGroup> {
        //collection of words (agiWord objects)

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
        public WordList() {
            //initialize the collections
            mWordCol = new SortedList<string, AGIWord>();
            mGroupCol = new SortedList<int, WordGroup>();
            // add starting words, but don't use AddWord method
            // because list is not loaded yet
            AGIWord tmpWord = new AGIWord { WordText = "a", Group = 0 };
            mWordCol.Add("a", tmpWord);
            tmpWord.WordText = "anyword";
            tmpWord.Group = 1;
            mWordCol.Add("anyword", tmpWord);
            tmpWord.WordText = "rol";
            tmpWord.Group = 9999;
            mWordCol.Add("rol", tmpWord);
            WordGroup tmpGroup = new WordGroup { mGroupNum = 0 };
            tmpGroup.AddWordToGroup("a");
            mGroupCol.Add(0, tmpGroup);
            tmpGroup = new WordGroup { mGroupNum = 1 };
            tmpGroup.AddWordToGroup("any");
            mGroupCol.Add(1, tmpGroup);
            tmpGroup = new WordGroup { mGroupNum = 9999 };
            tmpGroup.AddWordToGroup("rol");
            mGroupCol.Add(9999, tmpGroup);
        }
        internal WordList(AGIGame parent, bool Loaded = false) {
            //initialize the collections
            mWordCol = new SortedList<string, AGIWord>();
            mGroupCol = new SortedList<int, WordGroup>();
            //this function is only called for a vocab word list
            //that is part of a game
            //it sets the ingame flag
            mInGame = true;
            this.parent = parent;
            //if loaded property is passed, set loaded flag as well
            mLoaded = Loaded;
            //it also sets the default name to //WORDS.TOK//
            mResFile = this.parent.agGameDir + "WORDS.TOK";
        }
        public void NewWords() {
            //marks the resource as loaded
            //this is needed so new resources can be created and edited
            //if already loaded
            if (mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(642)) {
                    HResult = WINAGI_ERR + 642
                };
                throw wex;
            }

            //cant call NewResource if already in a game;
            //clear it instead
            if (mInGame) {

                WinAGIException wex = new(LoadResString(510)) {
                    HResult = WINAGI_ERR + 510
                };
                throw wex;
            }
            //mark as loaded
            mLoaded = true;
            //clear resname and description
            mResFile = "";
            mDescription = "";
            //use clear method to ensure list is reset
            Clear();

            //add default groups
            AddWord("a", 0);
            AddWord("anyword", 1);
            AddWord("rol", 9999);
        }
        public void Export(string ExportFile, int FileType = 0, bool ResetDirty = true) {
            //exports words.tok
            // filetype = 0 means AGI WORDS.TOK file
            // filetype = 1 means SCUMM SVE format

            //if not loaded
            if (!mLoaded) {
                //error

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
            //if NOT in a game,
            if (!mInGame) {
                if (ResetDirty) {
                    //clear dirty flag
                    mIsDirty = false;
                }
                //save filename
                mResFile = ExportFile;
            }
            return;
        }
        internal AGIWord ItemByIndex(int Index) {  //used by SetWords method to retrieve words one by one
                                                   //NOTE: no error checking is done, because ONLY
                                                   //SetWords uses this function, and it ensures
                                                   //Index is valid
            return mWordCol.Values[Index];
        }
        public bool Loaded {
            get { return mLoaded; }
        }
        public int ErrLevel {
            //provides access to current error level of the word list

            //can be used by calling programs to provide feedback
            //on errors in the sound data

            //return 0 if successful, no errors/warnings
            // non-zero for error/warning:
            //   1 = abnormal index ('a' group doesn't start at position 52)
            get {
                return mErrLvl;
            }
        }
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
            try {
                //open the file
                fsWords = new FileStream(LoadFile, FileMode.Open);
            }
            catch (Exception) {
                // no good
                throw new Exception("bad WORDS.TOK file");
            }
            //if no data,
            if (fsWords.Length == 0) {
                fsWords.Dispose();
                return false;
            }
            // empty word and group columns
            mWordCol = [];
            mGroupCol = [];

            //read in entire resource
            Array.Resize(ref bytData, (int)fsWords.Length);
            fsWords.Read(bytData);
            //start at beginning of words section
            //note that words.tok file uses MSLS format for two byte-word data)
            bytHigh = bytData[0];
            bytLow = bytData[1];
            lngPos = (bytHigh << 8) + bytLow;
            if (lngPos != 52) {
                // words.tok file is corrupt or invalid
                // don't assume it's bad just yet; if the target
                // byte is zero, give it a try
                if (bytData[lngPos] == 0) {
                    // note the problem in a warning
                    mErrLvl = 1;
                }
                else {
                    mLoaded = false;
                    return false;
                    ////invalid words.tok file!
                    //throw new Exception("bad WORDS.TOK file");
                }
            }
            //for first word, there is no previous word
            strPrevWord = "";
            //read character Count for first word
            bytPrevWordCharCount = bytData[lngPos];
            lngPos++;

            //continue loading words until end of resource is reached
            while (lngPos < bytData.Length) // Until lngPos > UBound(bytData)
            {
                //initialize word
                strThisWord = "";
                //if some characters should be copied from previous word
                if (bytPrevWordCharCount > 0) {
                    //copy them
                    strThisWord = Left(strPrevWord, bytPrevWordCharCount);
                }
                //build rest of word
                do {
                    bytVal[0] = bytData[lngPos];
                    // **** check for extended character marker
                    if (bytVal[0] == 0) {
                        //nextchar is the one
                        lngPos++;
                        bytVal[0] = bytData[lngPos];
                        bytExt = 128;
                    }
                    else {
                        bytExt = 0;
                    }
                    //if bit7 is clear
                    if (bytVal[0] < 0x80) {
                        bytVal[0] = (byte)(bytVal[0] ^ 0x7F + bytExt);
                        strThisWord += parent.agCodePage.GetString(bytVal);
                    }
                    lngPos++;
                    //continue until last character (indicated by flag) or endofresource is reached
                }
                while ((bytVal[0] < 0x80) && (lngPos < bytData.Length)); // Loop Until (bytVal >= 0x80) || lngPos > UBound(bytData)
                                                                         //if end of file is reached before 0x80,
                if (lngPos >= bytData.Length) {
                    //invalid words.tok file!
                    throw new Exception("bad WORDS.TOK file");
                }
                //add last character (after stripping off flag)
                bytVal[0] ^= 0xFF;
                strThisWord += parent.agCodePage.GetString(bytVal);
                // TODO: convert any upper-case characters to lower-case (just in case)
                // do I need a custom converter? 
                strThisWord = strThisWord.ToLower();
                //get group number
                lngGrpNum = (bytData[lngPos] << 8) + bytData[lngPos + 1];
                //set pointer to next word

                lngPos += 2;
                //if this word different to previous,
                //(this ensures no duplicates are added)
                if (strThisWord != strPrevWord) {
                    //if this word already exists,
                    if (mWordCol.ContainsKey(strThisWord)) {
                        //delete the old one
                        RemoveWord(strThisWord);
                    }
                    //add word
                    AddWord(strThisWord, lngGrpNum);
                    //this word is now the previous word
                    strPrevWord = strThisWord;
                }
                bytPrevWordCharCount = bytData[lngPos];
                lngPos++;
            }
            //if no words
            if (mWordCol.Count == 0) {
                //add default words
                AddWord("a", 0);
                AddWord("anyword", 1);
                AddWord("rol", 9999);
            }
            //if reserved groups are empty, add default words
            if (!GroupExists(0)) {
                AddWord("a", 0);
            }
            if (!GroupExists(1)) {
                AddWord("anyword", 1);
            }
            if (!GroupExists(9999)) {
                AddWord("rol", 9999);
            }
            return true;
        }
        public void Clone(WordList WordListToClone) {
            //copies word list from NewWords to
            //this word list
            int i, j;
            int lngGrpNum, lngCount;
            string strWord;
            WordGroup tmpGroup;
            AGIWord tmpWord;
            //if source wordlist is not loaded
            if (!WordListToClone.Loaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //first, clear current list
            Clear();
            //then add all groups
            foreach (WordGroup oldGroup in WordListToClone) {
                //create new temp group, assign the number
                tmpGroup = new WordGroup();
                lngGrpNum = oldGroup.GroupNum;
                tmpGroup.GroupNum = lngGrpNum;
                //add group to end of collection
                mGroupCol.Add(lngGrpNum, tmpGroup);
            }

            //then add all words
            foreach (AGIWord oldWord in WordListToClone.mWordCol.Values) {
                //get word
                strWord = oldWord.WordText;
                lngGrpNum = oldWord.Group;
                //create new word item
                tmpWord.WordText = strWord;
                tmpWord.Group = lngGrpNum;
                //add word to wordcol
                mWordCol.Add(strWord, tmpWord);
                //add word to its groupcol
                //in order to access internal properties,
                //need to create a direct reference to the AGIwordgroup class
                mGroupCol[lngGrpNum].AddWordToGroup(strWord);
            } //nxt  i

            //copy description
            mDescription = WordListToClone.Description;
            //set dirty flags
            mIsDirty = WordListToClone.IsDirty;
            mWriteProps = WordListToClone.WriteProps;
            //copy filename
            mResFile = WordListToClone.ResFile;
            //set loaded flag
            mLoaded = true;
        }
        internal bool WriteProps { get { return mWriteProps; } }
        public string Description {
            get {
                //if not loaded
                if (!mLoaded) {
                    //error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                return mDescription;
            }
            set {
                //if not loaded
                if (!mLoaded) {
                    //error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                //limit description to 1K
                value = Left(value, 1024);
                //if changing
                if (value != mDescription) {
                    mDescription = value;
                    //if in a game
                    if (mInGame) {
                        parent.WriteGameSetting("WORDS.TOK", "Description", mDescription);
                        //change date of last edit
                        parent.agLastEdit = DateTime.Now;
                    }
                }
            }
        }
        internal bool InGame {
            get {
                //only used by setword method
                return mInGame;
            }
            set {
                mInGame = value;
            }
        }
        public string ResFile {
            get {
                return mResFile;
            }
            set {
                //resfile cannot be changed if resource is part of a game
                if (mInGame) {
                    //error- resfile is readonly for ingame resources

                    WinAGIException wex = new(LoadResString(680)) {
                        HResult = WINAGI_ERR + 680
                    };
                    throw wex;
                }
                else {
                    mResFile = value;
                }
            }
        }
        public void Clear() {
            //clears the word group, and sets up a blank list
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //reset group and word collections
            mGroupCol = [];
            mWordCol = [];
            mDescription = "";
            mIsDirty = true;
        }
        public WordGroup GroupN(int groupNum) {
            //returns a group by its group number
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //return this group by it's number (key value)
            return mGroupCol[groupNum];
        }
        public bool GroupExists(int GroupNumber) {
            //if this group exists, it returns true
            return mGroupCol.ContainsKey(GroupNumber);
        }
        public bool IsDirty {
            get {
                //if resource is dirty, or (prop values need writing AND in game)
                return (mIsDirty || (mWriteProps && mInGame));
            }
            set {
                mIsDirty = value;
            }
        }
        public AGIWord this[dynamic vKeyIndex] {
            get {
                //access is by word string or index number
                //if not loaded
                if (!mLoaded) {
                    //error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                //only allow string or integer number
                if ((vKeyIndex is int) || (vKeyIndex is byte) || (vKeyIndex is short)) {
                    //retrieve via index

                    //validate index
                    if (vKeyIndex < 0 || vKeyIndex > mWordCol.Count - 1) {
                        throw new IndexOutOfRangeException();
                    }
                    return mWordCol.Values[vKeyIndex];
                }
                else if (vKeyIndex is string) {
                    // a string could be a number passed as a string, OR an actual word?
                    // user has to make sure it's not a number passed as a string...
                    //retrieve via string key - actual word, which is the key
                    return mWordCol[vKeyIndex];
                }
                else {
                    throw new Exception("invalid key/index");
                }
            }
        }
        public void Save(string SaveFile = "") {
            //saves wordlist

            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //if in a game,
            if (mInGame) {
                SaveFile = mResFile;
            }
            else {
                if (SaveFile.Length == 0) {
                    SaveFile = mResFile;
                }
            }
            try {
                //compile the file
                Compile(SaveFile);
            }
            catch {
                throw;
            }
            if (mInGame) {
                //change date of last edit
                parent.agLastEdit = DateTime.Now;
            }
            else {
                //save filename
                mResFile = SaveFile;
            }
            //mark as clean
            mIsDirty = false;
        }
        public void Unload() {
            //unloads the resource; same as clear, except file marked as not dirty
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            Clear();
            mLoaded = false;
            mWriteProps = false;
            mIsDirty = false;
        }
        public int WordCount {
            get {
                //if not loaded
                if (!mLoaded) {
                    //error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                return mWordCol.Count;
            }
        }
        public bool WordExists(string aWord) {
            return mWordCol.ContainsKey(aWord);
        }
        public void AddGroup(int GroupNumber) {
            int i;
            WordGroup tmpGroup;
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //if group number is invalid
            if (GroupNumber < 0) {
                //error

                WinAGIException wex = new(LoadResString(575)) {
                    HResult = WINAGI_ERR + 575
                };
                throw wex;
            }
            if (GroupNumber > MAX_GROUP_NUM) {
                //error

                WinAGIException wex = new(LoadResString(575)) {
                    HResult = WINAGI_ERR + 575
                };
                throw wex;
            }
            //see if group already exists
            if (mGroupCol.ContainsKey(GroupNumber)) {

                WinAGIException wex = new(LoadResString(576)) {
                    HResult = WINAGI_ERR + 576
                };
                throw wex;
            }
            tmpGroup = new WordGroup {
                GroupNum = GroupNumber
            };
            //add it
            mGroupCol.Add(GroupNumber, tmpGroup);
            mIsDirty = true;
        }
        void RemoveWord(string aWord) {
            //deletes aWord
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // if word doesn't exist
            if (!mWordCol.TryGetValue(aWord, out AGIWord value)) {
                //word not found

                WinAGIException wex = new(LoadResString(584)) {
                    HResult = WINAGI_ERR + 584
                };
                throw wex;
            }
            //delete this word from its assigned group BY Group number
            WordGroup tmpGroup = GroupN(value.Group);
            tmpGroup.DeleteWordFromGroup(aWord);
            //if group is now empty, delete it too
            if (tmpGroup.WordCount == 0) {
                mGroupCol.Remove(tmpGroup.GroupNum);
            }
            //remove it from the word collection
            mWordCol.Remove(aWord);
            //set dirty flag
            mIsDirty = true;
        }
        public int GroupCount {
            get {
                //if not loaded
                if (!mLoaded) {
                    //error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }

                return mGroupCol.Count;
            }
        }
        public void RemoveGroup(int GroupNumber) {
            int i;
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //if group number is invalid
            if (GroupNumber < 0 || GroupNumber > MAX_GROUP_NUM) {
                //error

                WinAGIException wex = new(LoadResString(575)) {
                    HResult = WINAGI_ERR + 575
                };
                throw wex;
            }
            //if group doesn't exist
            if (!GroupExists(GroupNumber)) {
                //error

                WinAGIException wex = new(LoadResString(583)) {
                    HResult = WINAGI_ERR + 583
                };
                throw wex;
            }
            //step through all words in main list
            // CAN'T use foreach because we need to delete 
            // some of the objects
            for (i = mWordCol.Count - 1; i >= 0; i--) {
                if (mWordCol.Values[i].Group == GroupNumber) {
                    //delete this word from main list
                    mWordCol.RemoveAt(i);
                }
            }
            //delete the group
            mGroupCol.Remove(GroupNumber);
            //set dirty flag
            mIsDirty = true;
        }
        public void RenumberGroup(int OldGroupNumber, int NewGroupNumber) {
            int i;
            WordGroup tmpGroup;
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //if oldgroup number is invalid
            if (OldGroupNumber < 0 || OldGroupNumber > MAX_GROUP_NUM) {
                //error

                WinAGIException wex = new(LoadResString(575)) {
                    HResult = WINAGI_ERR + 575
                };
                throw wex;
            }
            //if new group number is invalid
            if (NewGroupNumber < 0 || NewGroupNumber > MAX_GROUP_NUM) {
                //error

                WinAGIException wex = new(LoadResString(575)) {
                    HResult = WINAGI_ERR + 575
                };
                throw wex;
            }
            //if old group doesn't exist
            if (!GroupExists(OldGroupNumber)) {
                //error

                WinAGIException wex = new(LoadResString(696)) {
                    HResult = WINAGI_ERR + 696
                };
                throw wex;
            }
            //if new group already exists
            if (GroupExists(NewGroupNumber)) {
                //error

                WinAGIException wex = new(LoadResString(697)) {
                    HResult = WINAGI_ERR + 697
                };
                throw wex;
            }
            //make temp copy of old group
            tmpGroup = mGroupCol[OldGroupNumber];
            //remove group
            mGroupCol.Remove(OldGroupNumber);
            // change group number
            tmpGroup.GroupNum = NewGroupNumber;
            //change group number for all words in the group
            // CAN'T use forach, because we are adding to the collection
            for (i = 0; i < mWordCol.Count; i++) {
                if (mWordCol.Values[i].Group == OldGroupNumber) {
                    AGIWord tmpWord = mWordCol.Values[i];
                    tmpWord.Group = NewGroupNumber;
                }
            }
            //then re-add the group
            mGroupCol.Add(NewGroupNumber, tmpGroup);

            //set dirty flag
            mIsDirty = true;
        }
        void Compile(string CompileFile) {
            //compiles the word list into a Sierra WORDS.TOK file
            int i;
            byte CurByte;
            string strCurWord;
            string strPrevWord = "";
            byte intPrevWordCharCount;
            bool blnWordsMatch;
            string strFirstLetter;
            int[] intLetterIndex = new int[26];
            string strTempFile;
            //if no filename passed,
            if (CompileFile.Length == 0) {
                WinAGIException wex = new(LoadResString(616)) {
                    HResult = WINAGI_ERR + 616
                };
                throw wex;
            }
            //if not dirty AND CompileFile=resfile
            if (!mIsDirty && CompileFile.Equals(mResFile, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            //if there are no word groups to add
            if (mGroupCol.Count == 0) {
                WinAGIException wex = new(LoadResString(672).Replace(ARG1, "no word groups to add")) {
                    HResult = WINAGI_ERR + 672
                };
                throw wex;
            }
            //if there are no words,
            if (mWordCol.Count == 0) {
                //error
                WinAGIException wex = new(LoadResString(672).Replace(ARG1, "no words to add")) {
                    HResult = WINAGI_ERR + 672
                };
                throw wex;
            }
            //create temp file
            strTempFile = Path.GetTempFileName();
            //open the file
            FileStream fsWords;
            try {
                fsWords = new FileStream(strTempFile, FileMode.OpenOrCreate);
                //write letter index
                for (i = 0; i <= 51; i++) {
                    fsWords.WriteByte(0);
                }
                //index pointer starts with 'a'
                strFirstLetter = "a";
                intLetterIndex[1] = 52;
                //now step through all words in list
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
                WinAGIException wex = new(LoadResString(672).Replace(ARG1, lngError.ToString()) + ": " + strError) {
                    HResult = WINAGI_ERR + 672,
                    Source = strErrSrc
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
                    throw new Exception("524, strErrSource, Replace(LoadResString(524), ARG1, LoadFile)");
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
            //WordText = UnicodeToCP(WordText, parent.agCodePage);
            //WordText = Encoding.Convert(Encoding.Unicode, parent.agCodePage, Encoding.Unicode.GetBytes(WordText)).ToString();
            //convert input to lowercase
            // TODO: do I need custom lowercase? because extended chars 
            // can be extended without causing problems
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
