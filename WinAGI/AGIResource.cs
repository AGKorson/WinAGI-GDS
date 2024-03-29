using System;
using System.IO;
using static WinAGI.Engine.Base;
using static WinAGI.Common.Base;

namespace WinAGI.Engine {
    //public abstract class AGIResource
    public class AGIResource {
        protected bool mLoaded = false;
        protected int mErrLevel = 0; // <0 means unreadable data; 0 means no errors; >0 means minor errors but resource is readable
        protected string mResID;
        protected sbyte mVolume = -1;
        protected int mLoc = -1;
        protected int mSize = -1; // actual size, if compressed this is different from mSizeInVol
        protected int mSizeInVol = -1;  // size as stored in VOL
        protected RData mRData = new(0);
        protected bool mInGame;
        internal AGIGame parent;
        protected bool mIsDirty;
        protected byte mResNum;
        protected string mDescription;
        protected readonly AGIResType mResType;
        protected string mResFile;
        private bool mblnEORes; //flag indicating pointer is at beginning of resource (CurPos=0)
        private int mlngCurPos;  //current position of pointer in resource data

        internal delegate void AGIResPropChangedEventHandler(object sender, AGIResPropChangedEventArgs e);
        internal event AGIResPropChangedEventHandler PropertyChanged;

        protected AGIResource(AGIResType ResType) {
            // can ONLY create new resource from within other game resources
            // when first created, a resource MUST have a type assigned
            mResType = ResType;
            // New resources start out NOT in game; so vol and loc are undefined
            mInGame = false;
            mVolume = -1;
            mLoc = -1;
            // calling resource constructor is responsible for creating default data
            // assume no compression
            V3Compressed = 0;
        }

        public class AGIResPropChangedEventArgs(string name) {
            public string Name { get; } = name;
        }
        protected void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new AGIResPropChangedEventArgs(name));
        }

        protected void InitInGame(AGIGame parent, byte ResNum) {
            // attaches a new resource to a game
            // and finds a location in vol
            this.parent = parent;
            mInGame = true;
            mResNum = ResNum;
            // ingame resources start loaded
            mLoaded = true;
            // set vol/loc
            AddToVol(this, parent.agIsVersion3);
            UpdateDirFile(this);
        }

        protected void InitInGame(AGIGame parent, byte ResNum, sbyte VOL, int Loc) {
            //attaches an existing resource to a game
            this.parent = parent;
            mInGame = true;
            mResNum = ResNum;
            mVolume = VOL;
            mLoc = Loc;
            //// ingame resources start loaded
            ////mLoaded = true;
        }

        public sbyte Volume {
            get {
                if (!mInGame) {
                    return -1;
                }
                else {
                    return mVolume;
                }
            }
            internal set { mVolume = value; }
        }

        public int Loc {
            get {
                if (mInGame) {
                    return mLoc;
                }
                else {
                    return -1;
                }
            }
            internal set {
                mLoc = value;
            }
        }

        public int Size {
            get {
                // returns the uncompressed size of the resource
                // location of resource doesn't matter; should always have a size

                if (mInGame) {
                    if (mLoaded) {
                        return mRData.Length;
                    }
                    else {
                        return mSize;
                    }
                }
                else {
                    // not in a game:
                    if (mLoaded) {
                        return mRData.Length;
                    }
                    else {
                        return 0;
                    }
                }
            }
            protected set {
                //only subclass can set size, when initializing a new game resource
                mSize = value;
            }
        }

        public virtual int SizeInVOL {
            get {
                //returns the size of the resource on the volume

                //if IN a game:
                //  - SizeInVol - read from wag during initial load;
                //                if not present, get from res Header in vol file
                //              - update in WAG file when resource is saved
                //  if NOT in a game:
                //  - SizeInVol - is always -1
                
                if (mInGame) {
                    if (mErrLevel < 0) {
                        // invalid
                        return -1;
                    }
                    else {
                        return mSizeInVol;
                    }
                }
                else {
                    return -1;
                }
            }
            internal set {
                // only AddToVOL calls this; either on intial load, or after saving a resource
                // after saving a resource to a VOL file
                mSizeInVol = value;
            }
        }

        internal int GetSizeInVOL() {
            //returns the size of this resource in its VOL file

            //if an error occurs while trying to read the size of this
            //resource, the function returns -1
            byte bytHigh, bytLow;
            int lngV3Offset;
            string strVolFile;
            //any file access errors
            //result in invalid size

            //if version 3
            if (parent.agIsVersion3) {
                //adjusts header so compressed size is retrieved
                lngV3Offset = 2;
                //set filename
                strVolFile = parent.agGameDir + parent.agGameID + "VOL." + mVolume;
            }
            else {
                lngV3Offset = 0;
                //set filename
                strVolFile = parent.agGameDir + "VOL." + mVolume;
            }
            try {
                //open the volume file
                fsVOL = new FileStream(strVolFile, FileMode.Open);
                brVOL = new BinaryReader(fsVOL);
                //verify enough room to get length of resource
                if (fsVOL.Length >= mLoc + 5 + lngV3Offset) {
                    //get size low and high bytes
                    fsVOL.Seek(mLoc, SeekOrigin.Begin);
                    bytLow = brVOL.ReadByte();
                    bytHigh = brVOL.ReadByte();
                    //verify this is a proper resource
                    if ((bytLow == 0x12) && (bytHigh == 0x34)) {
                        //now get the low and high bytes of the size
                        fsVOL.Seek(1, SeekOrigin.Current);
                        bytLow = brVOL.ReadByte();
                        bytHigh = brVOL.ReadByte();
                        fsVOL.Dispose();
                        brVOL.Dispose();
                        return (bytHigh << 8) + bytLow;
                    }
                }
            }
            catch (Exception) {
                // treat all errors the same
            }
            finally {
                //ensure file is closed
                fsVOL.Dispose();
                brVOL.Dispose();
            }
            // if size not found, return -1
            return -1;
        }

        public int V3Compressed { get; internal set; }// 0 = not compressed; 1 = picture compress; 2 = LZW compress

        public bool InGame { get { return mInGame; } internal set { mInGame = value; } }

        public byte Number {
            get { if (mInGame) return mResNum; return 0; }//TODO: number is meaningless if not in a game
            internal set { mResNum = value; }
        }

        public bool Loaded { get => mLoaded; internal set { } }

        public int ErrLevel { get => mErrLevel; internal set { } }

        string[] mErrData = ["", "", "", "", ""];
        public string[] ErrData {
            get => mErrData;
            private protected set => mErrData = value;
        }

        public string ResFile {
            get {
                if (mInGame) {
                    return mResFile;
                }
                else {
                    return "";
                }
            }
            set {
                mResFile = value;
            }
        }
        
        internal bool WritePropState { get; set; }
        
        public virtual bool IsDirty { get => mIsDirty; internal set { } }

        public AGIResType ResType { get { return mResType; } }
        
        public virtual string ID {
            get { return mResID; }
            set {
                // sets the ID for a resource;
                // resource IDs must be unique to each resource type
                // max length of ID is 64 characters
                // min of 1 character;
                // if new value is invalid, ignore it
                string NewID = value;

                if (NewID.Length == 0) {
                    // ignore
                    return;
                }
                else if (NewID.Length > 64) {
                    NewID = NewID[..64];
                }
                if (!NewID.Equals(mResID, StringComparison.OrdinalIgnoreCase)) {
                    // ingame resources must have unique IDs
                    if (InGame) {
                        // step through other resources
                        foreach (Logic tmpRes in parent.agLogs) {
                            if (tmpRes.ID.Equals(NewID) && this != tmpRes) {
                                // duplicate - ignore change request
                                return;
                            }
                        }
                        foreach (Picture tmpRes in parent.agPics) {
                            if (tmpRes.ID.Equals(NewID) && this != tmpRes) {
                                // duplicate - ignore change request
                                return;
                            }
                        }
                        foreach (Sound tmpRes in parent.agSnds) {
                            //if resource IDs are same
                            if (tmpRes.ID.Equals(NewID) && this != tmpRes) {
                                // duplicate - ignore change request
                                return;
                            }
                        }
                        foreach (View tmpRes in parent.agViews) {
                            //if resource IDs are same
                            if (tmpRes.ID.Equals(NewID) && this != tmpRes) {
                                // duplicate - ignore change request
                                return;
                            }
                        }
                    }

                    //save ID
                    mResID = NewID;
                    //reset compiler list of ids
                    Compiler.blnSetIDs = false;
                }
            }
        }
        
        public string Description {
            get { return mDescription; }
            set {
                // limit description to 1K
                string newDesc;

                if (value.Length > 1024) {
                    newDesc = value[..1024];
                }
                else {
                    newDesc = value;
                }
                if (newDesc != mDescription) {
                    mDescription = newDesc;
                    if (mInGame) {
                        parent.WriteGameSetting("Logic" + Number, "Description", mDescription, "Logics");
                    }
                }
            }
        }
        
        public bool EORes {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mlngCurPos >= mRData.Length;
            }
        }

        // to allow indexing of data property, a separate class needs to
        // be created
        public RData Data {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mRData;
            }
            internal set {
                // can only set the data object internally
                mRData = value;
            }
        }

        internal void Clone(AGIResource NewRes) {
            // copies entire resource structure from this resource into NewRes

            // ingame property is never copied; only way to change ingame status is to use
            // methods for adding/removing resources to/from game
            NewRes.parent = parent;

            // resource data are copied manually as necessary by calling method
            // EORes and CurPos are calculated; don't need to copy them
            NewRes.mResID = mResID;
            NewRes.mDescription = mDescription;
            NewRes.mResNum = mResNum;
            NewRes.mResFile = mResFile;
            // copy vol and loc info even though the copy
            // may or may not also be marked ingame
            NewRes.mVolume = mVolume;
            NewRes.mLoc = mLoc;
            NewRes.mLoaded = mLoaded;
            NewRes.Data.AllData = mRData.AllData;
            NewRes.mIsDirty = mIsDirty;
            NewRes.WritePropState = WritePropState;
            NewRes.mSize = mSize;
            NewRes.mSizeInVol = mSizeInVol;
            NewRes.mblnEORes = mblnEORes;
            NewRes.mlngCurPos = mlngCurPos;
            NewRes.ErrLevel = mErrLevel;
            NewRes.ErrData = ErrData;
        }
        
        public virtual void Load() {
            // loads the data for this resource
            // from its VOL file, if in a game
            // or from its resfile
            byte bytLow, bytHigh, bytVolNum;
            bool blnIsPicture = false;
            int intSize, lngExpandedSize = 0;
            string strLoadResFile;

            // if already loaded,
            if (mLoaded) {
                // do nothing
                return;
            }
            // always return success
            mLoaded = true;
            mIsDirty = false;
            WritePropState = false;

            //if in a game,
            if (mInGame) {
                // resource data is loaded from the AGI VOL file
                if (parent.agIsVersion3) {
                    strLoadResFile = parent.agGameDir + parent.agGameID + "VOL." + mVolume.ToString();
                }
                else {
                    strLoadResFile = parent.agGameDir + "VOL." + mVolume.ToString();
                }
            }
            else {
                // use resource filename
                strLoadResFile = mResFile;
            }
            // verify file exists
            if (!File.Exists(strLoadResFile)) {
                mErrLevel = -1;
                ErrData[0] = strLoadResFile;
                ErrData[1] = mResID;
                return;
            }
            // check for readonly
            if ((File.GetAttributes(strLoadResFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                mErrLevel = -2;
                ErrData[0] = strLoadResFile;
                return;
            }
            // open file (VOL or individual resource)
            try {
                fsVOL = new FileStream(strLoadResFile, FileMode.Open);
                brVOL = new BinaryReader(fsVOL);
            }
            catch (Exception e1) {
                fsVOL.Dispose();
                brVOL.Dispose();
                mErrLevel = -3;
                ErrData[0] = e1.Message;
                ErrData[1] = mResID;
                return;
            }
            // verify resource is within file bounds
            if (mLoc > fsVOL.Length) {
                fsVOL.Dispose();
                brVOL.Dispose();
                mErrLevel = -4;
                ErrData[0] = mLoc.ToString();
                ErrData[1] = mVolume.ToString();
                ErrData[2] = Path.GetFileName(fsVOL.Name);
                ErrData[3] = mResID;
                return;
            }
            // if loading from a VOL file (i.e. is in a game)
            if (mInGame) {
                //read header bytes
                brVOL.BaseStream.Seek(mLoc, SeekOrigin.Begin);
                bytHigh = brVOL.ReadByte();
                bytLow = brVOL.ReadByte();
                if (bytHigh != 0x12 || bytLow != 0x34) {
                    fsVOL.Dispose();
                    brVOL.Dispose();
                    mErrLevel = -5;
                    ErrData[0] = mLoc.ToString();
                    ErrData[1] = mVolume.ToString();
                    ErrData[2] = Path.GetFileName(fsVOL.Name);
                    ErrData[3] = mResID;
                    return;
                }
                // get volume where this resource is stored
                bytVolNum = brVOL.ReadByte();
                // determine if this resource is a compressed picture
                blnIsPicture = ((bytVolNum & 0x80) == 0x80);
                // get size info
                bytLow = brVOL.ReadByte();
                bytHigh = brVOL.ReadByte();
                intSize = (bytHigh << 8) + bytLow;
                // if version3,
                if (parent.agIsVersion3) {
                    // the size retreived is the expanded size
                    lngExpandedSize = intSize;
                    // now get compressed size
                    bytLow = brVOL.ReadByte();
                    bytHigh = brVOL.ReadByte();
                    intSize = (bytHigh << 8) + bytLow;
                }
            }
            else {
                // get size from total file length
                intSize = (int)fsVOL.Length;
                // version 3 files are never compressed when loaded as individual files
                lngExpandedSize = intSize;
            }
            // get resource data
            mRData.ReSize(intSize);
            mRData.AllData = brVOL.ReadBytes(intSize);
            fsVOL.Dispose();
            brVOL.Dispose();
            // if version3
            if (parent.agIsVersion3) {
                // if resource is a compressed picture
                if (blnIsPicture) {
                    // pictures use this decompression
                    V3Compressed = 1;
                    mRData.AllData = DecompressPicture(mRData.AllData);
                    // adjust size
                    intSize = lngExpandedSize;
                }
                else {
                    // if resource is LZW compressed,
                    if (mRData.Length != lngExpandedSize) {
                        // all other resources use LZW compression
                        V3Compressed = 2;
                        mRData.AllData = ExpandV3ResData(mRData.AllData, lngExpandedSize);
                        // update size
                        intSize = lngExpandedSize;
                    }
                }
            }
            // reset resource markers
            mlngCurPos = 0;
            mblnEORes = false;
            // update size property
            mSize = intSize;
            // attach events
            mRData.PropertyChanged += Raise_DataChange;
            return;
        }

        public virtual void Unload() {
            // only ingame resources can be unloaded
            if (!mInGame) {
                //just exit
                return;
            }
            // reset flag first so size doesn't get cleared for in game resources
            mLoaded = false;
            mIsDirty = false;
            // reset resource variables
            mRData.Clear();
            // don't mess with sizes though! they remain accessible even when unloaded
            mblnEORes = true;
            mlngCurPos = 0;
            //// clear error info - it might clear when reloaded
            //mErrLevel = 0;
            //mErrData = ["", "", "", "", ""];
            // detach events
            mRData.PropertyChanged -= Raise_DataChange;
        }

        internal void Save() {
            // saves a resource into a VOL file if in a game
            // saves to standalone resource file if not in a game

            WinAGIException.ThrowIfNotLoaded(this);
            // if in game,
            if (mInGame) {
                try {
                    //add resource to VOL file to save it
                    AddToVol(this, parent.agIsVersion3);
                    // update saved size
                    mSize = mRData.Length;
                    // resource is no longer compressed
                    V3Compressed = 0;
                    mSizeInVol = mSize;
                }
                catch {
                    // pass error along
                    throw;
                }
                //change date of last edit
                parent.agLastEdit = DateTime.Now;
            }
            else {
                try {
                    // export it
                    Export(mResFile);
                }
                catch {
                    // pass error along
                    throw;
                }
            }
        }

        public void Export(string ExportFile) {
            // exports resource to file
            // doesn't affect the current resource filename
            // MUST specify a valid export file
            // if export file already exists, it is overwritten
            // caller is responsible for verifying overwrite is ok or not

            bool blnUnload = false;

            //if no filename passed
            if (ExportFile.Length == 0) {
                WinAGIException wex = new(LoadResString(599)) {
                    HResult = WINAGI_ERR + 599,
                };
                throw wex;
            }
            // resource has to be loaded
            // TODO: should I change this to an error if not loaded?
            // make the calling function make sure it's loaded first?
            if (!mLoaded) {
                blnUnload = true;
                Load();
            }
            // check for errors
            if (mErrLevel < 0) {
                WinAGIException wex = new(LoadResString(601)) {
                    HResult = WINAGI_ERR + 601,
                };
                throw wex;
            }
            // get temporary file
            FileStream fsExport = null;
            try {
                // open file for output
                fsExport = new FileStream(ExportFile, FileMode.Open);
                // write data
                fsExport.Write(mRData.AllData, 0, mRData.Length);
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(582)) {
                    HResult = WINAGI_ERR + 582,
                };
                wex.Data["exception"] = e;
                throw wex;
            }
            finally {
                // close file,
                fsExport.Dispose();
                // unload if necessary
                if (blnUnload) {
                    Unload();
                }
            }
            // if NOT in a game,
            if (!mInGame) {
                //change resfile to match new export filename
                mResFile = ExportFile;
            }
        }

        public virtual void Import(string ImportFile) {
            // imports resource from a file, and loads it
            // if in a game, it also saves the new resource in a VOL file
            //
            // import does not check if the imported resource is valid or not, nor if it is
            // of the correct resource type
            //
            // the calling program is responsible for that check

            //if no filename passed
            if (ImportFile.Length == 0) {
                // error
                WinAGIException wex = new(LoadResString(604)) {
                    HResult = WINAGI_ERR + 604,
                };
                throw wex;
            }
            //if file doesn't exist
            if (!File.Exists(ImportFile)) {
                //error
                WinAGIException wex = new(LoadResString(524).Replace(ARG1, ImportFile)) {
                    HResult = WINAGI_ERR + 524,
                };
                wex.Data["missingfile"] = ImportFile;
                throw wex;
            }
            // if resource is currently loaded,
            if (mLoaded) {
                Unload();
            }
            //open file for binary
            FileStream fsImport;
            try {
                fsImport = new FileStream(ImportFile, FileMode.Open);
            }
            catch (Exception) {
                WinAGIException wex = new(LoadResString(605).Replace(ARG1, ImportFile)) {
                    HResult = WINAGI_ERR + 605,
                };
                throw wex;
            }
            // if file is empty
            if (fsImport.Length == 0) {
                WinAGIException wex = new(LoadResString(605).Replace(ARG1, ImportFile)) {
                    HResult = WINAGI_ERR + 605,
                };
                throw wex;
            }
            // load resource data from file
            fsImport.Read(mRData.AllData, 0, (int)fsImport.Length);
            // reset resource markers
            mlngCurPos = 0;
            mblnEORes = false;
            // TODO: why is loaded set to true? only the data is added
            // the actual resource isn't yet loaded...
            mLoaded = true;
            // raise change event
            OnPropertyChanged("Data");
            if (mInGame) {
                try {
                    // save resource (which finds a place for it in VOL files)
                    Save();
                }
                catch {
                    // pass along save errors
                    throw;
                }
            }
            else {
                // save the resource filename
                mResFile = ImportFile;
            }
        }

        public void WriteByte(byte InputByte, int Pos = -1) {
            bool bNoEvent = false;

            WinAGIException.ThrowIfNotLoaded(this);
            if (Pos != -1) {
                // validate location
                if (Pos < 0 || Pos > mRData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(Pos));
                }
            }
            else {
                // otherwise, default to end of resource
                Pos = mRData.Length;
            }
            if (Pos == mRData.Length) {
                //adjust to make room for new data being added
                mRData.ReSize(mRData.Length + 1);
                // this calls a change event, don't need to do another
                bNoEvent = true;
            }
            mRData[Pos] = InputByte;
            mlngCurPos = Pos + 1;
            mblnEORes = (mlngCurPos == mRData.Length);

            if (!bNoEvent) {
                OnPropertyChanged("Data");
            }
            return;
        }

        public void WriteWord(ushort InputInt, int Pos = -1, bool blnMSLS = false) {
            bool bNoEvent = false;
            byte bytHigh, bytLow;

            WinAGIException.ThrowIfNotLoaded(this);
            if (Pos != -1) {
                // validate location
                if (Pos < 0 || Pos > mRData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(Pos));
                }
            }
            else {
                // otherwise, default to end of resource
                Pos = mRData.Length;
            }
            // if at end of resource
            if (Pos == mRData.Length) {
                // adjust to make room for new data being added
                mRData.ReSize(mRData.Length + 2);
                // this calls a change event, don't need to do another
                bNoEvent = true;
            }
            //if one byte away from end of resource
            else if (Pos == mRData.Length - 1) {
                mRData.ReSize(mRData.Length + 1);
                bNoEvent = true;
            }
            // split integer portion into two bytes and add it
            bytHigh = (byte)(InputInt >> 8);
            bytLow = (byte)(InputInt % 256);
            if (blnMSLS) {
                mRData[Pos] = bytHigh;
                mRData[Pos + 1] = bytLow;
            }
            else {
                mRData[Pos] = bytLow;
                mRData[Pos + 1] = bytHigh;
            }
            mlngCurPos = Pos + 2;
            mblnEORes = (mlngCurPos == mRData.Length);
            if (!bNoEvent) {
                OnPropertyChanged("Data");
            }
        }

        public int Pos {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mlngCurPos;
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
                // validate position
                if (value < 0 || value > mRData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                mlngCurPos = value;
                mblnEORes = (mlngCurPos == mRData.Length);
                return;
            }
        }

        public ushort ReadWord(int Pos = -1, bool MSLS = false) {
            byte bytLow, bytHigh;

            WinAGIException.ThrowIfNotLoaded(this);

            // if a position is passed,
            if (Pos != -1) {
                //validate position
                if (Pos < 0 || Pos >= mRData.Length - 1) {
                    throw new ArgumentOutOfRangeException(nameof(Pos));
                }
                mlngCurPos = Pos;
            }
            if (MSLS) {
                // high first
                bytHigh = mRData[mlngCurPos];
                bytLow = mRData[mlngCurPos + 1];
            }
            else {
                // low first
                bytLow = mRData[mlngCurPos];
                bytHigh = mRData[mlngCurPos + 1];
            }
            mlngCurPos += 2;
            mblnEORes = mlngCurPos == mRData.Length;
            return (ushort)((bytHigh << 8) + bytLow);
        }

        public byte ReadByte(int Pos = -1) {

            WinAGIException.ThrowIfNotLoaded(this);

            // if a position is passed
            if (Pos != -1) {
                // validate position
                if (Pos < 0 || Pos >= mRData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(Pos));
                }
                mlngCurPos = Pos;
            }
            mlngCurPos++;
            mblnEORes = (mlngCurPos == mRData.Length);
            return mRData[mlngCurPos];
        }

        public void InsertData(dynamic NewData, int InsertPos = -1) {
            // inserts newdata into resource at insertpos, if passed,
            // or at end of resource, if not passed

            int i, lngResEnd, lngNewDatLen;
            byte[] bNewData = [0];

            WinAGIException.ThrowIfNotLoaded(this);

            if (NewData is byte[] bData) {
                // array of bytes = OK
                bNewData = bData;
            }
            else if (NewData is byte newByte) {
                // single byte = OK
                bNewData[0] = newByte;
            }
            else {
                // anything that can be converted to single byte = OK
                if (NewData is string) {
                    if (!byte.TryParse(NewData, out bNewData[0])) {
                        throw new ArgumentException("Data Type Mismatch");
                    }
                }
                else {
                    try {
                        bNewData[0] = (byte)NewData;
                    }
                    catch (Exception) {
                        throw new ArgumentException("Data Type Mismatch");
                    }
                }
            }
            // get resource end and length of data being inserted
            lngResEnd = mRData.Length;
            lngNewDatLen = bNewData.Length;

            if (InsertPos != -1) {
                // validate position
                if (Pos < 0 || Pos >= mRData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(InsertPos));
                }
                // insert pos is OK
                mRData.ReSize(mRData.Length + lngNewDatLen);
                //move current data forward to make room for inserted data
                for (i = lngResEnd + lngNewDatLen - 1; i >= InsertPos + lngNewDatLen; i--) {
                    mRData[i] = mRData[i - lngNewDatLen];
                }
                // add newdata at insertpos
                for (i = 0; i < lngNewDatLen; i++) {
                    mRData[InsertPos + i] = bNewData[i];
                }
            }
            else {
                // insert at end
                mRData.ReSize(mRData.Length + lngNewDatLen);
                for (i = 0; i < lngNewDatLen; i++) {
                    mRData[lngResEnd + i] = bNewData[i];
                }
            }
            //raise change event
            OnPropertyChanged("Data");
        }

        public void RemoveData(int RemovePos, int RemoveCount = 1) {
            //removes data from RemovePos; if a Count
            //is passed, that number of bytes removed; if no
            //Count is passed, then only one byte removed

            int i;

            WinAGIException.ThrowIfNotLoaded(this);
            // validate Count
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(RemoveCount);
            // validate removepos
            if (RemovePos < 0 || RemovePos >= mRData.Length) {
                throw new ArgumentOutOfRangeException(nameof(RemovePos));
            }
            // adjust count so it does not exceed array length
            if (RemovePos + RemoveCount > mRData.Length) {
                RemoveCount = mRData.Length - RemovePos;
            }
            for (i = RemovePos; i >= (mRData.Length - RemoveCount); i++) {
                mRData[i] = mRData[i + RemoveCount];
            }
            mRData.ReSize(mRData.Length - RemoveCount);
            OnPropertyChanged("Data");
        }

        private protected void ErrClear() {
            int errlevel = mErrLevel;
            string[] errdata = ErrData;
            // use public clear method
            Clear();
            // restore error info
            mErrLevel = errlevel;
            ErrData = errdata;
        }
        public virtual void Clear() {
            WinAGIException.ThrowIfNotLoaded(this);
            //clears the resource data
            mRData.Clear();
            mlngCurPos = 0;
            //mSizeInVol is undefined
            mSizeInVol = -1;
            mblnEORes = false;
            mIsDirty = true;
            mErrLevel = 0;
            mErrData = ["", "", "", "", ""];
        }
        public override string ToString() {
            if (mResID.Length > 0) {
                return mResID;
            }
            return "blank " + ResTypeName[(int)mResType];
        }
        internal void Raise_DataChange(object sender, RData.RDataChangedEventArgs e) {
            //pass it along
            OnPropertyChanged("Data");
        }
        internal bool IsUniqueResID(string checkID) {
            // check all resids
            foreach (AGIResource tmpRes in parent.agLogs.Col.Values) {
                if (tmpRes.ID.Equals(checkID, StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }
            }
            foreach (AGIResource tmpRes in parent.agPics.Col.Values) {
                if (tmpRes.ID.Equals(checkID, StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }
            }
            foreach (AGIResource tmpRes in parent.agSnds.Col.Values) {
                if (tmpRes.ID.Equals(checkID, StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }
            }
            foreach (AGIResource tmpRes in parent.agViews.Col.Values) {
                if (tmpRes.ID.Equals(checkID, StringComparison.OrdinalIgnoreCase)) {
                    return false;
                }
            }
            // not found; must be unique
            return true;
        }

        internal string UniqueResFile(AGIResType ResType) {
            int intNo = 0;
            string retval;
            do {
                intNo++;
                retval = parent.agResDir + "New" + ResTypeName[(int)ResType] + intNo + ".ag" + ResTypeName[(int)ResType][0];
            }
            while (File.Exists(retval));
            return retval;
        }
    }
}