﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents the list of inventory items stored in the
    /// OBJECT file of an AGI game.
    /// </summary>
    public class InventoryList : IEnumerable<InventoryItem> {
        #region Local Members
        byte mMaxScreenObjects;
        bool mEncrypted;
        bool mAmigaOBJ;
        string mResFile = "";
        string mDescription = "";
        bool mInGame;
        bool mIsChanged;
        bool mLoaded;
        int mErrLevel = 0;
        // 1 = empty file
        // 2 = invalid data (decrypt failure)
        // 3 = invalid data (datawidth failure)
        // 4 = invalid data (invalid text pointer)
        // 5 = first object is not '?'
        AGIGame parent = null;
        int mCodePage = Base.CodePage;
        #endregion

        #region Constructors
        /// <summary>
        /// <summary>
        /// Instantiates an inventory item list and attaches it to an AGI game.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="Loaded"></param>
        internal InventoryList(AGIGame parent, bool Loaded = false) {
            this.parent = parent;
            mInGame = true;
            mResFile = parent.agGameDir + "OBJECT";
            mCodePage = parent.agCodePage;
            InitInvObj();
            mLoaded = Loaded;
        }

        /// Instantiates a blank inventory item list that is not part of an AGI game.
        /// </summary>
        public InventoryList() {
            mInGame = false;
            mResFile = "";
            InitInvObj();
            mLoaded = true;
            mIsChanged = true;
            
        }

        /// Instantiates an inventory item list from a file that is not part
        /// of an AGI game. </summary>
        public InventoryList(string filename) {
            mInGame = false;
            mResFile = filename;
            InitInvObj();
            try {
                Load(filename);
            }
            catch {
                // blank the list
                mResFile = "";
                InitInvObj();
                // return the error
                throw;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the list of items in this list.
        /// </summary>
        internal List<InventoryItem> mItems { get; private set; }

        /// <summary>
        /// Gets the specified inventory item from this list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public InventoryItem this[int index] {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (index < 0 || index >= mItems.Count) {
                    throw new ArgumentOutOfRangeException();
                }
                return mItems[index];
            }
        }

        /// <summary>
        /// Gets or sets the index of highest allowable screen object that gets stored
        /// in the OBJECT file for this list.
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
                    mIsChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets the load status of this inventory items list. The items and other
        /// properties are not available if the list is not loaded. If not in a game,
        /// the list is always loaded.
        /// </summary>
        public bool Loaded {
            get {
                return mLoaded;
            }
        }

        /// <summary>
        /// Gets a a value that indicates if this list's items do not match what is 
        /// stored its assigned OBJECT file.
        /// </summary>
        public bool IsChanged {
            get {
                return mIsChanged;
            }
            internal set {
                mIsChanged = value;
            }
        }

        /// <summary>
        /// Gets or sets the OBJECT filename associated with this item list. If in a game, 
        /// the resfile name is readonly.
        /// </summary>
        public string ResFile {
            get {
                return mResFile;
            }
            set {
                if (mInGame) {
                    return;
                }
                else {
                    mResFile = value;
                }
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
                // limit description to 1K
                value = value.Left(1024);
                if (value != mDescription) {
                    mDescription = value;
                    if (mInGame) {
                        parent.WriteGameSetting("OBJECT", "Description", mDescription);
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
        /// Gets the number of inventory items in this list, including all
        /// null ('?') items.
        /// </summary>
        public int Count {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                // although first object is a null item, it still needs
                // to be counted (as do all other nulls)
                return mItems.Count;
            }
        }

        /// <summary>
        /// returns the number of non-null inventory items in this list.
        /// </summary>
        public byte InUseCount {
            get {
                byte retval = 0;
                WinAGIException.ThrowIfNotLoaded(this);
                for (int i = 0; i < mItems.Count; i++) {
                    if (mItems[i].ItemName != "?") {
                        retval++;
                    }
                }
                return retval;
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if the data for this list
        /// are encrypted when saved as an OBJECT file. You should normally
        /// let WinAGI handle encryption status as it is tied to the
        /// targeted interpreter version. Changing it could cause probems
        /// if the interpreter expects one format but finds another.
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
                    mIsChanged = true;
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
        /// Gets or sets the character code page to use when converting 
        /// characters to or from a byte stream.
        /// </summary>
        public int CodePage {
            get => parent is null ? mCodePage : parent.agCodePage;
            set {
                if (parent is null) {
                    // confirm new codepage is supported; error if it is not
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
        /// Gets a value that indicates if this inventory items list will be formatted
        /// for Amiga AGI game files instead of MSDOS AGI when the list is saved to
        /// the OBJECT file.  Amiga format uses four bytes per item entry in the offset
        /// table.
        /// </summary>
        public bool AmigaOBJ {
            get {
                return mAmigaOBJ;
            }
            set {
                // for now, only allow converting FROM Amiga TO DOS
                if (!mAmigaOBJ || value) {
                    return;
                }
                if (mAmigaOBJ != value) {
                    if (value) {
                        SetAmigaFormat();
                    }
                    else {
                        SetMSDOSFormat();
                    }
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the inventory list when it is first created.
        /// </summary>
        private void InitInvObj() {
            mItems = [];
            InventoryItem tmpItem;
            mMaxScreenObjects = defMaxSO;
            // add placeholder for item 0
            //tmpItem = new InventoryItem(this) {
            //    ItemName = "?",
            //    Room = 0
            //};
            tmpItem = new InventoryItem(this);
            tmpItem.mItemName = "?";
            tmpItem.mRoom = 0;
            mItems.Add(tmpItem);
        }

        /// <summary>
        /// Loads inventory items for the game from an OBJECT file. If not
        /// in a game, LoadFile must be specified.
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
                wex.Data["ID"] = "OBJECT";
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
                mIsChanged = false;
            }
        }

        /// <summary>
        /// Loads an inventory item list by reading from the OBJECT file.
        /// </summary>
        /// <param name="LoadFile"></param>
        /// <returns>0 = no errors or warnings<br />
        /// -21 = invalid data, unable to decrypt<br />
        /// -22 = invalid header, unable to read item data<br />
        /// -23 = file access error, unable to read file
        /// 1 = no data/empty file<br />
        /// 2 = invalid item pointer<br />
        /// 4 = first item is not '?'<br />
        /// </returns>
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
            catch {
                return -3;
            }
            mLoaded = true;

            if (fsObj.Length == 0) {
                fsObj.Dispose();
                return 1;
            }
            Array.Resize(ref bytData, (int)fsObj.Length);
            fsObj.Read(bytData);
            fsObj.Dispose();
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
                return -1;
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
                return -2;
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
                if (lngPos > bytData.Length) {
                    retval |= 2;
                    intItem++;
                    continue;
                    //return retval;
                }
                // build item name string
                sbItem = new();
                while (lngPos < bytData.Length) {
                    if (bytData[lngPos] == 0) {
                        break;
                    }
                    bytChar[0] = bytData[lngPos];
                    sbItem.Append(Encoding.GetEncoding(CodePage).GetString(bytChar));
                    lngPos++;
                }
                sItem = sbItem.ToString();
                // first item IS USUALLY a '?'  , but NOT always
                // (See MH2 for example!!!!)
                if (intItem == 0) {
                    // don't add first item if it's a '?'; it is already added
                    // but it might not be a '?'
                    if (sItem != "?") {
                        // rename first object
                        mItems[0].mItemName = sItem;
                        mItems[0].mRoom = bytRoom;
                        retval |= 4;
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
        /// Unloads this list if in a game. Inventory item lists that are not part
        /// of a game are always loaded. 
        /// </summary>
        public void Unload() {
            if (!mLoaded || !mInGame) {
                return;
            }
            Clear();
            mLoaded = false;
            mIsChanged = false;
        }

        /// <summary>
        /// Saves this inventory item list to file by compiling the items in the
        /// list in the proper Sierra AGI OBJECT file format. If not in a game, 
        /// the default filename is used unless one is passed to this method.
        /// </summary>
        /// <param name="SaveFile"></param>
        public void Save(string SaveFile = "") {
            WinAGIException.ThrowIfNotLoaded(this);
            if (mInGame) {
                Compile(mResFile);
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
                mResFile = SaveFile;
            }
            mIsChanged = false;
        }

        /// <summary>
        /// Exports this inventory item list to a standalone file. If the item list is
        /// not in a game, exporting to the current OBJECT file is the same as saving.
        /// </summary>
        /// <param name="ExportFile"></param>
        /// <exception cref="Exception"></exception>
        public void Export(string ExportFile) {
            WinAGIException.ThrowIfNotLoaded(this);
            try {
                Compile(ExportFile);
            }
            catch (Exception) {
                WinAGIException wex = new(LoadResString(582)) {
                    HResult = WINAGI_ERR + 582,
                };
                throw wex;
            }
            if (!mInGame) {
                mIsChanged = false;
                mResFile = ExportFile;
            }
        }

        /// <summary>
        /// Compiles the object list into a Sierra AGI compatible OBJECT file.
        /// </summary>
        /// <param name="CompileFile"></param>
        void Compile(string CompileFile) {
            int lngFileSize;    // size of object file
            int CurrentChar;    // current character in name
            int lngTextStart;  // start of item names
            int lngPos;         // position in current item name
            byte[] bytTemp;
            string strTempFile = "";
            int Dwidth;

            if (CompileFile.Length == 0) {
                WinAGIException wex = new(LoadResString(616)) {
                    HResult = WINAGI_ERR + 616
                };
                throw wex;
            }
            if (!mIsChanged && CompileFile.Equals(mResFile, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            // PC version (most common) has 3 bytes per item in offest table;
            // amiga version has four bytes per item
            Dwidth = mAmigaOBJ ? 4 : 3;
            // calculate base filesize
            // (offset table size plus header + null obj '?')
            lngFileSize = (mItems.Count + 1) * Dwidth + 2;
            // step through all items to determine length of each, and
            // add it to file length counter
            foreach (InventoryItem tmpItem in mItems) {
                if (tmpItem.ItemName != "?") {
                    // add size of object name to file size
                    // (include null character at end of string)
                    lngFileSize += tmpItem.ItemName.Length + 1;
                }
            }
            // initialize byte array to final size of file
            bytTemp = new byte[lngFileSize];
            // set offset to start of string data as measured from the beginning
            // of the index table, NOT including the header (which is just the
            // total length of index table: dwidth * numitems) 
            lngTextStart = mItems.Count * Dwidth;
            bytTemp[0] = (byte)(lngTextStart % 256);
            bytTemp[1] = (byte)(lngTextStart / 256);
            // add maxscreenobject value
            // (first item's index will begin at pos = 3)
            bytTemp[2] = mMaxScreenObjects;
            // write string for null item (?) at start of text data, after
            // adjusting position to account for width of header
            lngPos = lngTextStart + Dwidth;
            bytTemp[lngPos] = 63;
            bytTemp[lngPos + 1] = 0;
            lngPos += 2;
            // now add all items 
            for (int i = 0; i < mItems.Count; i++) {
                if (mItems[i].ItemName == "?") {
                    // write offset data to null item
                    bytTemp[3 + i * Dwidth] = (byte)(lngTextStart % 256);
                    bytTemp[3 + i * Dwidth + 1] = (byte)(lngTextStart / 256);
                    bytTemp[3 + i * Dwidth + 2] = mItems[i].Room;
                }
                else {
                    // write offset data for start of this word in its proper
                    // place in the index table, remember to subtract data
                    // element width because offset is from end of header,
                    // not beginning of file; lngPos IS referenced from
                    // beginning of file
                    bytTemp[3 + i * Dwidth] = (byte)((lngPos - Dwidth) % 256);
                    bytTemp[3 + i * Dwidth + 1] = (byte)((lngPos - Dwidth) / 256);
                    bytTemp[3 + i * Dwidth + 2] = mItems[i].Room;
                    // write all characters of this object
                    byte[] tmpItemBytes = Encoding.GetEncoding(CodePage).GetBytes(mItems[i].ItemName);
                    for (CurrentChar = 0; CurrentChar < tmpItemBytes.Length; CurrentChar++) {
                        bytTemp[lngPos++] = tmpItemBytes[CurrentChar];
                    }
                    // add null character to end
                    bytTemp[lngPos++] = 0;
                }
            }
            if (mEncrypted) {
                for (lngPos = 0; lngPos < bytTemp.Length; lngPos++) {
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
                File.Move(strTempFile, CompileFile, true);
            }
            catch (Exception e) {
                SafeFileDelete(strTempFile);
                WinAGIException wex = new(LoadResString(674).Replace(ARG1, e.HResult.ToString())) {
                    HResult = WINAGI_ERR + 674,
                };
                throw wex;
            }
        }

        /// <summary>
        /// Adds a new inventory item to this list, with the specified parameters.
        /// </summary>
        /// <param name="NewItem"></param>
        /// <param name="Room"></param>
        /// <returns>A reference to the added inventory item.</returns>
        public InventoryItem Add(string NewItem, byte Room) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (mItems.Count == MAX_ITEMS) {
                WinAGIException wex = new(LoadResString(569)) {
                    HResult = WINAGI_ERR + 569,
                };
                throw wex;
            }
            InventoryItem tmpItem = new InventoryItem(this);
            tmpItem.ItemName = NewItem;
            tmpItem.Room = Room;
            mItems.Add(tmpItem);
            mIsChanged = true;
            return tmpItem;
        }

        public InventoryItem Add(InventoryItem item) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (mItems.Count == MAX_ITEMS) {
                WinAGIException wex = new(LoadResString(569)) {
                    HResult = WINAGI_ERR + 569,
                };
                throw wex;
            }
            InventoryItem tmpItem = new InventoryItem(this);
            tmpItem.ItemName = item.ItemName;
            tmpItem.Room = item.Room;
            mItems.Add(tmpItem);
            mIsChanged = true;
            return tmpItem;
        }

        /// <summary>
        /// Removes an inventory item from this list.
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(int Index) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (Index < 0 || Index >= mItems.Count) {
                throw new ArgumentOutOfRangeException();
            }
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
            // if this at end of the list (but not FIRST item)
            if (Index == mItems.Count - 1 && mItems.Count > 1) {
                // remove the item
                mItems.RemoveAt(Index);
            }
            else {
                //set item to '?'
                mItems[Index].Unique = true;
                mItems[Index].ItemName = "?";
                mItems[Index].Room = 0;
            }
            mIsChanged = true;
        }

        /// <summary>
        /// Clears this list and sets all properties to default values.
        /// </summary>
        public void Clear() {
            InventoryItem tmpItem;

            WinAGIException.ThrowIfNotLoaded(this);
            mEncrypted = false;
            mMaxScreenObjects = 16;
            mAmigaOBJ = false;
            mDescription = "";
            mItems = [];
            // add the placeholder
            tmpItem = new InventoryItem(this) {
                ItemName = "?",
                Room = 0
            };
            mItems.Add(tmpItem);
            mIsChanged = true;
        }

        public InventoryList Clone() {
            // only loaded views can be cloned
            WinAGIException.ThrowIfNotLoaded(this);

            InventoryList clonelist = new();

            // copy all items and properties except ingame status and parent
            // related properties
            clonelist.mAmigaOBJ = mAmigaOBJ;
            clonelist.mCodePage = mCodePage;
            clonelist.mDescription = mDescription;
            clonelist.mEncrypted = mEncrypted;
            clonelist.mErrLevel = mErrLevel;
            clonelist.mLoaded = mLoaded;
            clonelist.mMaxScreenObjects = mMaxScreenObjects;
            clonelist.mResFile = mResFile;
            clonelist.mIsChanged = mIsChanged;
            // need to remove the default to avoid duplication
            clonelist.mItems.RemoveAt(0);
            foreach (InventoryItem itm in mItems) {
                InventoryItem tmp = new InventoryItem(clonelist);
                tmp.mItemName = itm.ItemName;
                tmp.mRoom = itm.Room;
                tmp.Unique = itm.Unique;
                clonelist.mItems.Add(tmp);
            }
            return clonelist;
        }

        public void CloneFrom(InventoryList srcList) {
            // copy all items and properties except ingame status
            // related properties
            WinAGIException.ThrowIfNotLoaded(srcList);

            mAmigaOBJ = srcList.mAmigaOBJ;
            mCodePage = srcList.mCodePage;
            mDescription = srcList.mDescription;
            mEncrypted = srcList.mEncrypted;
            mErrLevel = srcList.mErrLevel;
            mMaxScreenObjects = srcList.mMaxScreenObjects;
            mResFile = srcList.mResFile;
            mIsChanged = srcList.mIsChanged;
            mItems = [];
            foreach (InventoryItem itm in srcList.mItems) {
                InventoryItem tmp = new InventoryItem(this);
                tmp.ItemName = itm.ItemName;
                tmp.Room = itm.Room;
                mItems.Add(tmp);
            }
            mLoaded = true;
        }

        /// <summary>
        /// Currently this function is not available, and does nothing.
        /// </summary>
        public void SetAmigaFormat() {
            // for now, we only allow converting FROM Amiga TO DOS
            return;
        }

        /// <summary>
        /// Changes the format of this inventory list to MSDOS, and updates
        /// the OBJECT file. MSDOS format uses three bytes per item entry in
        /// the offset table.
        /// </summary>
        public void SetMSDOSFormat() {
            // if aready DOS, exit
            if (!mAmigaOBJ) {
                return;
            }
            // set the flag to be NON-Amiga
            mAmigaOBJ = false;
            string theDir;
            if (parent is null) {
                theDir = JustPath(mResFile);
            }
            else {
                theDir = parent.agGameDir;
            }
            SafeFileDelete(theDir + "OBJECT.amg");
            try {
                File.Move(parent.agGameDir + "OBJECT", theDir + "OBJECT.amg", true);
                // mark it as changed, and save it to create a new file in MSDOS format
                mIsChanged = true;
                Save();
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(582)) {
                    HResult = WINAGI_ERR + 582,
                };
                wex.Data["exception"] = e;
                throw wex;
            }
        }

        /// <summary>
        /// This method checks the OBJECT file data to determine if it is
        /// encrypted with the string, "Avis Durgan".
        /// </summary>
        /// <param name="bytLast"></param>
        /// <param name="lngEndPos"></param>
        /// <returns>0 if cleartext (not encrypted)<br />
        /// 1 if encrypted<br />
        /// 2 if invalid</returns>
        private static int IsEncrypted(byte bytLast, int lngEndPos) {
            // check the last byte in the OBJECT data stream. It should ALWAYS
            // be null char ('0'):
            //    if the resource is NOT encrypted,
            //      the last byte will have a Value of 0x00
            //      the function will return a Value of 0
            //
            //    if it IS encrypted,
            //      it will be a character from the "Avis Durgan"
            //      string, dependent on the offset of the last
            //      byte from a multiple of 11 (the length of "Avis Durgan")
            //      the function will return a Value of 1
            // if the last character doesn't properly decrypt to (char)0
            // the function returns an error Value (2)
            if (bytLast == 0) {
                // return unencrypted
                return 0;
            }
            else {
                // decrypt character
                bytLast = (byte)(bytLast ^ bytEncryptKey[lngEndPos % 11]);
                // now, it should be zero
                return bytLast == 0 ? 1 : 2;
            }
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
        #endregion

        #region Enumeration
        ItemEnum GetEnumerator() {
            return new ItemEnum(mItems);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<InventoryItem> IEnumerable<InventoryItem>.GetEnumerator() {
            return (IEnumerator<InventoryItem>)GetEnumerator();
        }

        /// <summary>
        /// Implements enumeration for the InventoryList class.
        /// </summary>
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
        #endregion
    }
}
