using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.Base;
using static WinAGI.Common.Base;
using System.IO;
using System.Text;

namespace WinAGI.Engine {
    public class InventoryList : IEnumerable<InventoryItem> {
        byte mMaxScreenObjects;
        bool mEncrypted;
        bool mAmigaOBJ;
        string mResFile = "";
        string mDescription = "";
        bool mInGame;
        bool mIsDirty;
        bool mWriteProps;
        bool mLoaded;
        int mErrLevel = 0;
        // 1 = empty file
        // 2 = invalid data (decrypt failure)
        // 3 = invalid data (datawidth failure)
        // 4 = invalid data (invalid text pointer)
        // 5 = first object is not '?'
        AGIGame parent = null;
        Encoding mCodePage = Encoding.GetEncoding(437);
        //other
        public InventoryList() {
            mInGame = false;
            mResFile = "";
            InitInvObj();
        }
        public InventoryList(AGIGame parent, bool Loaded = false) {
            this.parent = parent;
            mInGame = true;
            //if loaded property is passed, set loaded flag as well
            mLoaded = Loaded;
            //set resourcefile to game default
            mResFile = parent.agGameDir + "OBJECT";
            InitInvObj();
        }
        private void InitInvObj() {
            // create the initial Col object
            mItems = [];
            InventoryItem tmpItem;
            mMaxScreenObjects = 16;
            //add placeholder for item 0
            tmpItem = new InventoryItem {
                ItemName = "?",
                Room = 0
            };
            tmpItem.SetParent(this);
            mItems.Add(tmpItem);
        }
        internal List<InventoryItem> mItems { get; private set; }
        public InventoryItem this[byte index] { get { return mItems[index]; } }

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
        /// 
        /// </summary>
        public byte Count {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                // although first object does not Count as an object
                // it must be returned as part of the Count
                // so everything works properly
                return (byte)mItems.Count;
            }
            private set {
            }
        }
        public bool AmigaOBJ {
            get {
                return mAmigaOBJ;
            }
            set {
                // for now, we only allow converting FROM Amiga TO DOS
                //                                         (T)     (F)
                // if trying to make it Amiga, exit
                if (value) {
                    return;
                }
                // if aready DOS, exit
                if (!mAmigaOBJ) {
                    return;
                }
                // set the flag to be NON-Amiga
                mAmigaOBJ = value;
                //save the current file as 'OBJECT.amg'
                string theDir;
                if (parent is null) {
                    theDir = JustPath(mResFile);
                }
                else {
                    theDir = parent.agGameDir;
                }
                try {
                    if (File.Exists(theDir + "OBJECT.amg")) {
                        File.Delete(theDir + "OBJECT.amg");
                    }
                    File.Move(parent.agGameDir + "OBJECT", theDir + "OBJECT.amg");

                    //now delete the current file
                    File.Delete(theDir + "OBJECT");
                    //mark it as dirty, and save it to create a new file
                    mIsDirty = true;
                    Save();
                }
                catch (Exception) {
                    // TODO: need new exception code
                    throw new Exception("error during Amiga conversion");
                }
            }
        }

        /// <summary>
        /// Gets the error level associated with this inventory item list.
        /// </summary>
        public int ErrLevel {
            get {
                return mErrLevel;
            }
        }

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
                        parent.WriteProperty("OBJECT", "Description", mDescription);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ExportFile"></param>
        /// <param name="ResetDirty"></param>
        /// <exception cref="Exception"></exception>
        public void Export(string ExportFile, bool ResetDirty = true) {
            //exports the list of inventory objects

            WinAGIException.ThrowIfNotLoaded(this);
            try {
                Compile(ExportFile);
            }
            catch (Exception) {
                //return error condition
                WinAGIException wex = new(LoadResString(582)) {
                    HResult = WINAGI_ERR + 582,
                };
                throw wex;
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
        }

        /// <summary>
        /// 
        /// </summary>
        public bool InGame {
            get {
                //only used by  setobjects method
                return mInGame;
            }
            internal set {
                mInGame = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDirty {
            get {
                //if resource is dirty, or (prop values need writing AND in game)
                return (mIsDirty || (mWriteProps && mInGame));
            }
            set {
                mIsDirty = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        internal bool WriteProps {
            get { return mWriteProps; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Loaded {
            get {
                return mLoaded;
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
        /// <param name="IsDirty"></param>
        /// <param name="WriteProps"></param>
        internal void SetFlags(ref bool IsDirty, ref bool WriteProps) {
            // used when copying a resource?
            IsDirty = mIsDirty;
            WriteProps = mWriteProps;
        }

        /// <summary>
        /// Loads an OBJECT file in original Sierra format.
        /// </summary>
        /// <param name="LoadFile"></param>
        /// <returns>true if successful, false if it fails</returns>
        private int LoadSierraFile(string LoadFile) {
            StringBuilder sbItem;
            string sItem;
            int intItem;
            byte bytRoom;
            int retval = 0;
            byte[] bytData = [], bytChar = new byte[1];
            int lngDataOffset, lngNameOffset;
            int lngPos, Dwidth;
            FileStream fsObj;

            try {
                fsObj = new(LoadFile, FileMode.Open);
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, LoadFile)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = e;
                wex.Data["badfile"] = LoadFile;
                throw wex;
            }
            // no major errors; consider it loaded
            // (even if data problems, it's still considered loaded)
            mLoaded = true;

            //if no data,
            if (fsObj.Length == 0) {
                fsObj.Dispose();
                return 1;
            }
            // read in entire resource
            Array.Resize(ref bytData, (int)fsObj.Length);
            fsObj.Read(bytData);
            fsObj.Dispose();
            // determine if file is encrypted or clear
            switch (IsEncrypted(bytData[^1], bytData.Length - 1)) {
            case 0:
                // unencrypted
                mEncrypted = false;
                break;
            case 1: 
                // encrypted - need to decrypt
                ToggleEncryption(ref bytData);
                mEncrypted = true;
                break;
            case 2: 
                // error in object file
                return 2;
            }
            // MSDOS files always have data element widths of 3 bytes;
            // Amigas have four; need to make sure correct value is used
            Dwidth = 3;
            do {
                lngDataOffset = (bytData[1] << 8) + bytData[0] + Dwidth;
                // first char of first item should be a question mark ('?')
                if (bytData[lngDataOffset] == 63) {
                    // correct file type found
                    mAmigaOBJ = (Dwidth == 4);
                    break;
                }
                else {
                    // try again, with four
                    Dwidth++;
                }
            }
            while (Dwidth <= 4);
            if (Dwidth == 5) {
                // error
                return 3;
            }
            mMaxScreenObjects = bytData[2];
            intItem = 0;
            // extract each item offset, and then its string data
            // (intItem*3) is offset address of string data;
            // stop if this goes past beginning of actual data
            do {
                lngNameOffset = (bytData[(intItem + 1) * Dwidth + 1] << 8) + bytData[(intItem + 1) * Dwidth] + Dwidth;
                bytRoom = bytData[Dwidth + intItem * Dwidth + 2];
                lngPos = lngNameOffset;
                // if past end of resource,
                if (lngPos > bytData.Length) {
                    // error
                    return 4;
                }
                // build item name string
                sbItem = new();
                while (lngPos < bytData.Length) {
                    if (bytData[lngPos] == 0) {
                        break;
                    }
                    bytChar[0] = bytData[lngPos];
                    sbItem.Append(parent.agCodePage.GetString(bytChar));
                    lngPos++;
                }
                sItem = sbItem.ToString();
                // first item IS USUALLY a '?'  , but NOT always
                // (See MH2 for example!!!!)
                if (intItem == 0) {
                    // don't add first item; it is already added
                    // but it might not be a '?'
                    if (sItem != "?") {
                        // rename first object
                        mItems[0].ItemName = sItem;
                        mItems[0].Room = bytRoom;
                        retval = 5;
                    }
                }
                else {
                    // add without key (to avoid duplicate key error)
                    Add(sItem, bytRoom);
                }
                intItem++;
            }
            while (((intItem * Dwidth) + Dwidth < lngDataOffset) && (intItem < MAX_ITEMS));
            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="NewItem"></param>
        /// <param name="Room"></param>
        /// <returns></returns>
        public InventoryItem Add(string NewItem, byte Room) {
            //adds new item to object list
            InventoryItem tmpItem;

            WinAGIException.ThrowIfNotLoaded(this);
            //if already have max number of items,
            if (mItems.Count == MAX_ITEMS) {
                //error
                WinAGIException wex = new(LoadResString(569)) {
                    HResult = WINAGI_ERR + 569,
                };
                throw wex;
            }
            //add the item
            tmpItem = new InventoryItem();
            //set parent first, so duplicate protection works
            tmpItem.SetParent(this);
            tmpItem.ItemName = NewItem;
            tmpItem.Room = Room;
            mItems.Add(tmpItem);
            //set dirty flag
            mIsDirty = true;
            return tmpItem;
        }

        /// <summary>
        /// Removes an item from the inventory list.
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(byte Index) {
            WinAGIException.ThrowIfNotLoaded(this);
            InventoryItem tmpItem = mItems[Index];

            //if this item is currently a duplicate, 
            // need to de-unique-ify this item
            if (!tmpItem.Unique) {
                // there are at least two objects with this item name;
                // this object and one or more duplicates
                // if there is only one other duplicate, it needs to have its
                // unique property reset because it will no longer be unique
                // after this object is changed
                // if there are multiple duplicates, the unique property does
                // not need to be reset
                byte dupItem = 0, dupCount = 0;
                for (int i = 0; i < mItems.Count; i++) {
                    if (mItems[(byte)i] != tmpItem) {
                        if (tmpItem.ItemName.Equals(mItems[(byte)i].ItemName, StringComparison.OrdinalIgnoreCase)) {
                            // duplicate found- is this the second?
                            if (dupCount == 1) {
                                // the other's are still non-unique
                                // so don't reset them
                                dupCount = 2;
                                break;
                            }
                            else {
                                // set duplicate count
                                dupCount = 1;
                                // save dupitem number
                                dupItem = (byte)i;
                            }
                        }
                    }
                }
                // if only one duplicate found
                if (dupCount == 1) {
                    // set unique flag for that item; it's no longer a duplicate
                    mItems[dupItem].Unique = true;
                }
            }
            //if this is the last item (but not FIRST item)
            if (Index == mItems.Count - 1 && mItems.Count > 1) {
                //remove the item
                mItems.RemoveAt(Index);
            }
            else {
                //set item to '?'
                mItems[Index].Unique = true;
                mItems[Index].ItemName = "?";
                mItems[Index].Room = 0;
            }
            mIsDirty = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Encrypted {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mEncrypted;
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mEncrypted != value) {
                    mEncrypted = value;
                    mIsDirty = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte MaxScreenObjects {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mMaxScreenObjects;
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
                if (value != mMaxScreenObjects) {
                    mMaxScreenObjects = value;
                    mIsDirty = true;
                }
            }
        }

        /// <summary>
        /// Loads inventory items for the game from an OBJECT file. If not in a game, LoadFile must be specified.
        /// </summary>
        /// <param name="LoadFile"></param>
        /// <exception cref="Exception"></exception>
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
                    mDescription = parent.agGameProps.GetSetting("OBJECT", "Description", "", true);
                }
                else {
                    mResFile = LoadFile;
                }
                mIsDirty = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytLast"></param>
        /// <param name="lngEndPos"></param>
        /// <returns></returns>
        int IsEncrypted(byte bytLast, int lngEndPos) {
            // this routine checks the resource to determine if it is
            // encrypted with the string, "Avis Durgan"
            // it does this by checking the last byte in the resource. It should ALWAYS
            // be Chr$(0):
            //    if the resource is NOT encrypted,
            //      the last byte will have a Value of 0x00
            //      the function will return a Value of 0
            //
            //    if it IS encrypted,
            //      it will be a character from the "Avis Durgan"
            //      string, dependent on the offset of the last
            //      byte from a multiple of 11 (the length of "Avis Durgan")
            //      the function will return a Value of 1
            // if the last character doesn't properly decrypt to Chr$(0)
            // the function returns an error Value (2)
            if (bytLast == 0) {
                // return unencrypted
                return 0;
            }
            else {
                // decrypt character
                bytLast = (byte)(bytLast ^ bytEncryptKey[lngEndPos % 11]);
                // now, it should be zero
                if (bytLast == 0) {
                    return 1;
                }
                else {
                    return 2;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SaveFile"></param>
        public void Save(string SaveFile = "") {
            // saves the list of inventory objects

            WinAGIException.ThrowIfNotLoaded(this);
            //if in a game,
            if (mInGame) {
                //compile the file
                Compile(mResFile);
                //change date of last edit
                parent.agLastEdit = DateTime.Now;
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
                Compile(SaveFile);
                // save filename
                mResFile = SaveFile;
            }
            //mark as clean
            mIsDirty = false;
        }

        /// <summary>
        /// Encrypts/decrypts the data by XOR'ing it with the encryption string
        /// </summary>
        /// <param name="bytData"></param>
        private void ToggleEncryption(ref byte[] bytData) {
            for (int lngPos = 0; lngPos < bytData.Length; lngPos++) {
                bytData[lngPos] = (byte)(bytData[lngPos] ^ bytEncryptKey[lngPos % 11]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Unload() {
            //unloads ther resource; same as clear, except file marked as not dirty
            if (!mLoaded) {
                return;
            }
            Clear();
            mLoaded = false;
            mWriteProps = false;
            mIsDirty = false;
        }

        /// <summary>
        /// Clears all objects and sets default values adds placeholder for item 0.
        /// </summary>
        public void Clear() {
            InventoryItem tmpItem;

            WinAGIException.ThrowIfNotLoaded(this);
            mEncrypted = false;
            mMaxScreenObjects = 16;
            mAmigaOBJ = false;
            mDescription = "";
            mItems = [];
            //add the placeholder
            tmpItem = new InventoryItem {
                ItemName = "?",
                Room = 0
            };
            mItems.Add(tmpItem);
            //but don't set parent; otherwise
            //circular object reference is created
            tmpItem.SetParent(this);
            //set dirty flag
            mIsDirty = true;
        }

        /// <summary>
        /// c\Compiles the object list into a Sierra AGI compatible OBJECT file
        /// </summary>
        /// <param name="CompileFile"></param>
        void Compile(string CompileFile) {
            int lngFileSize;    //size of object file
            int CurrentChar;    //current character in name
            int lngDataOffset;  //start of item names
            int lngPos;         //position in current item name
            byte[] bytTemp = [];
            string strTempFile = "";
            byte bytLow, bytHigh;
            int i;
            int Dwidth;

            if (CompileFile.Length == 0) {
                WinAGIException wex = new(LoadResString(616)) {
                    HResult = WINAGI_ERR + 616
                };
                throw wex;
            }
            //if not dirty AND compilefile=resfile
            if (!mIsDirty && CompileFile.Equals(mResFile, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            // PC version (most common) has 3 bytes per item in offest table; amiga version has four bytes per item
            Dwidth = mAmigaOBJ ? 4 : 3;

            // calculate min filesize
            // (offset table size plus header + null obj '?')
            lngFileSize = (mItems.Count + 1) * Dwidth + 2;
            // step through all items to determine length of each, and add it to file length counter
            foreach (InventoryItem tmpItem in mItems) {
                if (tmpItem.ItemName != "?") {
                    // add size of object name to file size
                    // (include null character at end of string)
                    lngFileSize += tmpItem.ItemName.Length + 1;
                }
            }
            // initialize byte array to final size of file
            Array.Resize(ref bytTemp, lngFileSize);
            // set offset from index to start of string data
            lngDataOffset = mItems.Count * Dwidth;
            bytHigh = (byte)(lngDataOffset / 256);
            bytLow = (byte)(lngDataOffset % 256);
            bytTemp[0] = bytLow;
            bytTemp[1] = bytHigh;
            bytTemp[2] = mMaxScreenObjects;
            // increment offset by width (to take into account file header)
            // this is also pointer to the null item
            lngDataOffset += Dwidth;
            // set counter to beginning of data
            lngPos = lngDataOffset;
            // write string for null item (?)
            bytTemp[lngPos] = 63;
            bytTemp[lngPos + 1] = 0;
            lngPos += 2;
            // now step through all items
            for (i = 1; 0 < mItems.Count - 1; i++) {
                if (mItems[i].ItemName == "?") {
                    // write offset data to null item
                    bytTemp[i * Dwidth] = (byte)(lngDataOffset % 256);
                    bytTemp[i * Dwidth + 1] = (byte)(lngDataOffset / 256);
                    // set room number for this object
                    bytTemp[i * Dwidth + 2] = mItems[i].Room;
                }
                else {
                    // write offset data for start of this word
                    // subtract data element width because offset is from end of header,
                    // not beginning of file; lngPos is referenced from position zero)
                    bytHigh = (byte)((lngPos - Dwidth) / 256);
                    bytLow = (byte)((lngPos - Dwidth) % 256);
                    bytTemp[i * Dwidth] = bytLow;
                    bytTemp[i * Dwidth + 1] = bytHigh;
                    bytTemp[i * Dwidth + 2] = mItems[i].Room;
                    // write all characters of this object
                    for (CurrentChar = 0; CurrentChar < mItems[i].ItemName.Length; CurrentChar++) {
                        // TODO: make sure extended charcters are working for all codepages
                        bytTemp[lngPos] = (byte)mItems[i].ItemName[CurrentChar];
                        lngPos++;
                    }
                    //add null character to end
                    bytTemp[lngPos] = 0;
                    lngPos++;
                }
            }
            //reduce array to actual size
            Array.Resize(ref bytTemp, lngPos);

            //if file is to be encrypted
            if (mEncrypted) {
                //step through entire file
                for (lngPos = 0; lngPos < bytTemp.Length; i++) {
                    //encrypt with 'Avis Durgan'
                    bytTemp[lngPos] ^= bytEncryptKey[lngPos % 11];
                }
            }
            try {
                // write output to a temp file
                strTempFile = Path.GetTempFileName();
                FileStream fsObj = new(strTempFile, FileMode.Open);
                fsObj.Write(bytTemp, 0, bytTemp.Length);
                fsObj.Dispose();
                if (File.Exists(CompileFile)) {
                    File.Delete(CompileFile);
                }
                File.Move(strTempFile, CompileFile);
            }
            catch (Exception e) {
                File.Delete(strTempFile);
                WinAGIException wex = new(LoadResString(674).Replace(ARG1, e.HResult.ToString())) {
                    HResult = WINAGI_ERR + 674,
                };
                throw wex;
            }
        }
        ItemEnum GetEnumerator() {
            return new ItemEnum(mItems);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<InventoryItem> IEnumerable<InventoryItem>.GetEnumerator() {
            return (IEnumerator<InventoryItem>)GetEnumerator();
        }
    }
    internal class ItemEnum : IEnumerator<InventoryItem> {
        public List<InventoryItem> _invitems;
        int position = -1;
        public ItemEnum(List<InventoryItem> list) {
            _invitems = list;
        }
        object IEnumerator.Current => Current;
        public InventoryItem Current {
            get {
                try {
                    return _invitems[position];
                }
                catch (IndexOutOfRangeException) {

                    throw new InvalidOperationException();
                }
            }
        }
        public bool MoveNext() {
            position++;
            return (position < _invitems.Count);
        }
        public void Reset() {
            position = -1;
        }
        public void Dispose() {
            _invitems = null;
        }
    }
}
