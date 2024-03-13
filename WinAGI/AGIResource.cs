using System;
using System.ComponentModel;
using System.IO;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Common.Base;

namespace WinAGI.Engine {
    //public abstract class AGIResource
    public class AGIResource {
        protected bool mLoaded = false;
        protected string mResID;
        protected sbyte mVolume = -1;
        protected int mLoc = -1;
        protected int mSize = -1; //actual size, if compressed this is different from mSizeInVol
        protected int mSizeInVol = -1;  //size as stored in VOL
        /*
        size- if NOT in a game:
          - SizeInVol is always -1
          - if resource loaded,
            Size = Data.Length
          - if resource NOT loaded,
            Size = 0

        size- if IN a game:
          - SizeInVol - read from wag during initial load;
                        if not present, get from res Header in vol file
                      - update in WAG file when resource is saved


        */
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
            // calling resource constructor is responsible for creating
            // default data
            // assume no compression
            V3Compressed = 0;
            //new resources start as loaded by default, and can only be unloaded when
            // in a game
            //           mLoaded = true;
        }
        public class AGIResPropChangedEventArgs {
            public AGIResPropChangedEventArgs(string name) {
                Name = name;
            }
            public string Name { get; }
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

                //
                if (mInGame) {
                    if (mLoaded) {
                        return mRData.Length;
                    }
                    else {
                        return mSize;
                    }
                }
                else {
                    //not in a game:
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
                if (mInGame) {
                    //if not established yet
                    if (mSizeInVol == -1) {
                        //get Value
                        mSizeInVol = GetSizeInVOL();
                        //if valid Value not returned,
                        if (mSizeInVol == -1) {
                            WinAGIException wex = new(LoadResString(625)) {
                                HResult = WINAGI_ERR + 625,
                            };
                            throw wex;
                        }
                    }
                    return mSizeInVol;
                }
                else {
                    return -1;
                }
            }
            set {
                //only AddToVOL calls this; either on intial load, or after saving a resource
                //after saving a resource to a VOL file
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
            // if size not found,
            //ensure file is closed, and return -1
            fsVOL.Dispose();
            brVOL.Dispose();
            return -1;
        }
        public int V3Compressed { get; internal set; }// 0 = not compressed; 1 = picture compress; 2 = LZW compress
        public bool InGame { get { return mInGame; } internal set { mInGame = value; } }
        public byte Number {
            get { if (mInGame) return mResNum; return 0; }//TODO: number is meaningless if not in a game
            internal set { mResNum = value; }
        }
        public bool Loaded { get { return mLoaded; } internal set { } }
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
        public virtual bool IsDirty { get { return mIsDirty; } internal set { } }
        public AGIResType ResType { get { return mResType; } }
        public virtual string ID {
            get { return mResID; }
            internal set {
                //sets the ID for a resource;
                //resource IDs must be unique to each resource type
                //max length of ID is 64 characters
                //min of 1 character
                string NewID = value;
                //validate length
                if (NewID.Length == 0) {
                    //error
                    WinAGIException wex = new(LoadResString(667)) {
                        HResult = WINAGI_ERR + 667
                    };
                    throw wex;
                }
                else if (NewID.Length > 64) {
                    NewID = Left(NewID, 64);
                }

                //if changing,
                if (!NewID.Equals(mResID, StringComparison.OrdinalIgnoreCase)) {
                    //if in a game,
                    if (InGame) {
                        //step through other resources
                        foreach (Logic tmpRes in parent.agLogs) {
                            //if resource IDs are same
                            if (tmpRes.ID.Equals(NewID, StringComparison.OrdinalIgnoreCase)) {
                                //if not the same resource
                                if (tmpRes.Number != Number || tmpRes.ResType != ResType) {
                                    // error
                                    WinAGIException wex = new(LoadResString(623)) {
                                        HResult = WINAGI_ERR + 623,
                                    };
                                    throw wex;
                                }
                            }
                        }
                        foreach (Picture tmpRes in parent.agPics) {
                            //if resource IDs are same
                            if (tmpRes.ID.Equals(NewID, StringComparison.OrdinalIgnoreCase)) {
                                //if not the same resource
                                if (tmpRes.Number != Number || tmpRes.ResType != ResType) {
                                    //error
                                    WinAGIException wex = new(LoadResString(623)) {
                                        HResult = WINAGI_ERR + 623,
                                    };
                                    throw wex;
                                }
                            }
                        }
                        foreach (Sound tmpRes in parent.agSnds) {
                            //if resource IDs are same
                            if (tmpRes.ID.Equals(NewID, StringComparison.OrdinalIgnoreCase)) {
                                //if not the same resource
                                if (tmpRes.Number != Number || tmpRes.ResType != ResType) {
                                    //error
                                    WinAGIException wex = new(LoadResString(623)) {
                                        HResult = WINAGI_ERR + 623,
                                    };
                                    throw wex;
                                }
                            }
                        }
                        foreach (View tmpRes in parent.agViews) {
                            //if resource IDs are same
                            if (tmpRes.ID.Equals(NewID, StringComparison.OrdinalIgnoreCase)) {
                                //if not the same resource
                                if (tmpRes.Number != Number || tmpRes.ResType != ResType) {
                                    //error
                                    WinAGIException wex = new(LoadResString(623)) {
                                        HResult = WINAGI_ERR + 623,
                                    };
                                    throw wex;
                                }
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
            internal set {
                //limit description to 1K
                string newDesc = Left(value, 1024);
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

                //if not loaded
                if (!mLoaded) {
                    //error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563,
                    };
                    throw wex;
                }
                return mlngCurPos >= mRData.Length;
            }
            private set { }
        }
        // to allow indexing of data property, a separate class needs to
        // be created
        public RData Data {
            get {
                //if not loaded
                if (!mLoaded) {
                    //error
                    throw new Exception(LoadResString(563));
                }
                return mRData;
            }
            internal set {
                // can only set the data object internally
                mRData = value;
            }
        }
        internal void Clone(AGIResource NewRes) {
            //called by setview, setlog, setpic and setsnd
            //copies entire resource structure from this resource 
            //into NewRes

            //ingame property is never copied; only way to
            //change ingame status is to do so through
            //appropriate methods for adding/removing
            //resources to/from game
            NewRes.parent = parent;

            //resource data are copied manually as necessary by calling method
            //EORes and CurPos are calculated; don't need to copy them
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
            //copy dirty flag and writeprop flag
            NewRes.mIsDirty = mIsDirty;
            NewRes.WritePropState = WritePropState;
            NewRes.mSize = mSize;
            NewRes.mSizeInVol = mSizeInVol;
            NewRes.mblnEORes = mblnEORes;
            NewRes.mlngCurPos = mlngCurPos;
        }
        public virtual void Load() {
            //loads the data for this resource
            //from its VOL file, if in a game
            //or from its resfile
            byte bytLow, bytHigh, bytVolNum;
            bool blnIsPicture = false;
            int intSize, lngExpandedSize = 0;
            string strLoadResFile;

            // if already loaded,
            if (mLoaded) {
                // do nothing
                return;
            }
            //if in a game,
            if (mInGame) {
                // resource data is loaded from the AGI VOL file
                // build filename
                if (parent.agIsVersion3) {
                    strLoadResFile = parent.agGameDir + parent.agGameID + "VOL." + mVolume.ToString();
                }
                else {
                    strLoadResFile = parent.agGameDir + "VOL." + mVolume.ToString();
                }
            }
            else {
                // if no filename
                if (mResFile.Length == 0) {
                    //error- nothing to load
                    WinAGIException wex = new(LoadResString(626)) {
                        HResult = WINAGI_ERR + 626,
                    };
                    throw wex;
                }
                else {
                    // use resource filename
                    strLoadResFile = mResFile;
                }
            }
            // verify file exists
            if (!File.Exists(strLoadResFile)) {
                WinAGIException wex = new(LoadResString(606).Replace(ARG1, Path.GetFileName(strLoadResFile))) {
                    HResult = WINAGI_ERR + 606,
                };
                wex.Data["ID"] = mResID;
                throw wex;
            }
            // open file (VOL or individual resource)
            try {
                fsVOL = new FileStream(strLoadResFile, FileMode.Open);
                brVOL = new BinaryReader(fsVOL);
            }
            catch (Exception e1) {
                fsVOL.Dispose();
                brVOL.Dispose();
                WinAGIException wex = new(LoadResString(502)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = e1;
                wex.Data["ID"] = mResID;
                throw wex;
            }
            // verify resource is within file bounds
            if (mLoc > fsVOL.Length) {
                fsVOL.Dispose();
                brVOL.Dispose();
                WinAGIException wex = new(LoadResString(505).Replace(ARG1, mLoc.ToString()).Replace(ARG2, fsVOL.Name).Replace(ARG2, mVolume.ToString())) {
                    HResult = WINAGI_ERR + 505,
                };
                wex.Data["loc"] = mLoc;
                wex.Data["vol"] = mVolume;
                wex.Data["volname"] = Path.GetFileName(fsVOL.Name);
                wex.Data["ID"] = mResID;
                throw wex;
            }
            // if loading from a VOL file (i.e. is in a game)
            if (mInGame) {
                //read header bytes
                brVOL.BaseStream.Seek(mLoc, SeekOrigin.Begin);
                bytHigh = brVOL.ReadByte();
                bytLow = brVOL.ReadByte();
                if ((!((bytHigh == 0x12) && (bytLow == 0x34)))) {
                    WinAGIException wex = new(LoadResString(506)) {
                        HResult = WINAGI_ERR + 506,
                    };
                    wex.Data["loc"] = mLoc;
                    wex.Data["vol"] = mVolume;
                    wex.Data["volname"] = Path.GetFileName(fsVOL.Name);
                    wex.Data["ID"] = mResID;
                    throw wex;
                }
                //get volume where this resource is stored
                bytVolNum = brVOL.ReadByte();
                //determine if this resource is a compressed picture
                blnIsPicture = ((bytVolNum & 0x80) == 0x80);
                //get size info
                bytLow = brVOL.ReadByte();
                bytHigh = brVOL.ReadByte();
                intSize = (bytHigh << 8) + bytLow;
                //if version3,
                if (parent.agIsVersion3) {
                    //the size retreived is the expanded size
                    lngExpandedSize = intSize;
                    //now get compressed size
                    bytLow = brVOL.ReadByte();
                    bytHigh = brVOL.ReadByte();
                    intSize = (bytHigh << 8) + bytLow;
                }
            }
            else {
                //get size from total file length
                intSize = (int)fsVOL.Length;
                // version 3 files are never compressed when
                // loaded as individual files
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
                        //all other resources use LZW compression
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
            mLoaded = true;
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
            // detach events
            mRData.PropertyChanged -= Raise_DataChange;
        }
        internal void Save() {
            // saves a resource into a VOL file if in a game
            // saves to standalone resource file if not in a game

            //if not loaded
            if (!mLoaded) {
                // error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563,
                };
                throw wex;
            }
            //if in game,
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
                catch (Exception e) {
                    // pass error along
                    WinAGIException wex = new(LoadResString(999)) {
                        HResult = WINAGI_ERR + 999,
                    };
                    wex.Data["error"] = e;
                    throw wex;
                }
                //change date of last edit
                parent.agLastEdit = DateTime.Now;
            }
            else {
                //export it
                Export(mResFile);
            }
        }

        public void Export(string ExportFile) {
            //exports resource to file
            //doesn't affect the current resource filename
            //MUST specify a valid export file
            //if export file already exists, it is overwritten
            //caller is responsible for verifying overwrite is ok or not

            bool blnUnload = false;

            //if no filename passed
            if (ExportFile.Length == 0) {
                WinAGIException wex = new(LoadResString(599)) {
                    HResult = WINAGI_ERR + 599,
                };
                throw wex;
            }
            //if not loaded
            if (!mLoaded) {
                blnUnload = true;
                try {
                    Load();
                }
                catch (Exception) {
                    WinAGIException wex = new(LoadResString(601)) {
                        HResult = WINAGI_ERR + 601,
                    };
                    throw wex;
                }
            }
            // get temporary file
            string strTempFile = Path.GetTempFileName();
            FileStream fsExport = null;
            try {
                // open file for output
                fsExport = new FileStream(strTempFile, FileMode.Open);
                // write data
                fsExport.Write(mRData.AllData, 0, mRData.Length);
            }
            catch (Exception) {
                fsExport.Dispose();
                File.Delete(strTempFile);
                if (blnUnload) {
                    Unload();
                    //return error condition
                }
                WinAGIException wex = new(LoadResString(582)) {
                    HResult = WINAGI_ERR + 582,
                };
                throw wex;
            }

            //close file,
            fsExport.Dispose();
            //unload if necessary
            if (blnUnload) {
                Unload();
            }

            //if savefile exists
            if (File.Exists(ExportFile)) {
                //delete it
                try {
                    File.Delete(ExportFile);
                }
                catch (Exception) {
                    //ignore if it can't be deleted; user will just have
                    // to deal with it
                }
            }
            try {
                //copy tempfile to savefile
                File.Move(strTempFile, ExportFile);
            }
            catch (Exception e) {
                //erase the temp file
                File.Delete(strTempFile);
                //return error condition
                WinAGIException wex = new(LoadResString(582)) {
                    HResult = WINAGI_ERR + 582,
                };
                wex.Data["error"] = e;
                throw wex;
            }

            //if NOT in a game,
            if (!mInGame) {
                //change resfile to match new export filename
                mResFile = ExportFile;
            }
        }

        public virtual void Import(string ImportFile) {
            //imports resource from a file, and loads it
            //if in a game, it also saves the new
            //resource in a VOL file
            //
            //import does not check if the imported
            //resource is valid or not, nor if it is
            //of the correct resource type
            //
            //the calling program is responsible for
            //that check

            //if no filename passed
            if (ImportFile.Length == 0) {
                //error
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
                throw wex;
            }
            //if resource is currently loaded,
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
            //load resource from file
            fsImport.Read(mRData.AllData, 0, (int)fsImport.Length);
            //if in a game
            if (mInGame) {
                //save resource
                Save();
            }
            else {
                //save the resource filename
                mResFile = ImportFile;
            }
            //reset resource markers
            mlngCurPos = 0;
            mblnEORes = false;
            mLoaded = true;
            //raise change event
            OnPropertyChanged("Data");
        }

        public void WriteByte(byte InputByte, int Pos = -1) {
            bool bNoEvent = false;
            //if not loaded
            if (!mLoaded) {
                //error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563,
                };
                throw wex;
            }
            //if a location is passed,
            if (Pos != -1) {
                int lngError = 0;
                //validate the new position
                if (Pos >= MAX_RES_SIZE) {
                    lngError = 516;
                }
                else if (Pos < 0) {
                    lngError = 514;
                }
                else if (Pos > mRData.Length) {
                    lngError = 517;
                }
                if (lngError > 0) {
                    WinAGIException wex = new(LoadResString(lngError)) {
                        HResult = WINAGI_ERR + lngError,
                    };
                    throw wex;
                }
            }
            else {
                //otherwise, default to end of resource
                Pos = mRData.Length;
            }
            //if currently pointing past end of data,
            if (Pos == mRData.Length) {
                //adjust to make room for new data being added
                mRData.ReSize(mRData.Length + 1);
                //this calls a change event, don't need to do another
                bNoEvent = true;
            }
            //save input data
            mRData[Pos] = InputByte;

            //set current position to Pos
            mlngCurPos = Pos + 1;

            //set EORes Value
            mblnEORes = (mlngCurPos == mRData.Length);

            if (!bNoEvent) {
                //raise change event
                OnPropertyChanged("Data");
            }
            return;
        }

        public void WriteWord(ushort InputInt, int Pos = -1, bool blnMSLS = false) {
            bool bNoEvent = false;

            byte bytHigh = 0, bytLow = 0;
            //if not loaded
            if (!mLoaded) {
                //error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563,
                };
                throw wex;
            }
            //if a location is passed,
            if (Pos != -1) {
                //validate the new position
                int lngError = 0;
                if (Pos >= MAX_RES_SIZE - 1) {
                    lngError = 516;
                }
                else if (Pos < 0) {
                    lngError = 514;
                }
                else if (Pos > mRData.Length) {
                    lngError = 513;
                }
                if (lngError > 0) {
                    WinAGIException wex = new(LoadResString(lngError)) {
                        HResult = WINAGI_ERR + lngError,
                    };
                    throw wex;
                }
            }
            else {
                //otherwise, default to end of resource
                Pos = mRData.Length;
            }
            //if past end of resource (by two bytes)
            if (Pos == mRData.Length) {
                //adjust to make room for new data being added
                mRData.ReSize(mRData.Length + 2);
                //this calls a change event, don't need to do another
                bNoEvent = true;
            }
            //if past end of resource (by one byte)
            else if (Pos == mRData.Length - 1) {
                //adjust to make room for new data being added
                mRData.ReSize(mRData.Length + 1);
                //this calls a change event, don't need to do another
                bNoEvent = true;
            }
            //split integer portion into two bytes
            bytHigh = (byte)(InputInt >> 8);
            bytLow = (byte)(InputInt % 256);
            //if wrting in MSLS mode,
            if (blnMSLS) {
                //save input data
                mRData[Pos] = bytHigh;
                mRData[Pos + 1] = bytLow;
            }
            else {
                mRData[Pos] = bytLow;
                mRData[Pos + 1] = bytHigh;
            }
            //set current position to Pos+2
            mlngCurPos = Pos + 2;
            //set EORes Value
            mblnEORes = (mlngCurPos == mRData.Length);
            if (!bNoEvent) {
                //raise change event
                OnPropertyChanged("Data");
            }
        }

        public int Pos {
            get {
                //if not loaded
                if (!mLoaded) {
                    //error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563,
                    };
                    throw wex;
                }
                //returns current position
                return mlngCurPos;
            }
            set {
                //if not loaded
                if (!mLoaded) {
                    //error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563,
                    };
                    throw wex;
                }
                //validate position
                int lngError = 0;
                if (value < 0) {
                    //no non-negative values
                    lngError = 514;
                }
                else if (value > mRData.Length) {
                    //cant point past eor
                    lngError = 513;
                }
                if (lngError > 0) {
                    WinAGIException wex = new(LoadResString(lngError)) {
                        HResult = WINAGI_ERR + lngError,
                    };
                    throw wex;
                }
                //new position is ok; assign it
                mlngCurPos = value;
                //set eor Value
                mblnEORes = (mlngCurPos == mRData.Length);
                //set bor Value
                return;
            }
        }
        public ushort ReadWord(int Pos = -1, bool MSLS = false) {
            byte bytLow, bytHigh;
            int lngError = 0;
            //if not loaded
            if (!mLoaded) {
                //error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563,
                };
                throw wex;
            }
            //if a position is passed,
            if (Pos != -1) {
                //validate position
                if (Pos < 0) {
                    //no non-negative values
                    lngError = 514;
                }
                else if (Pos >= mRData.Length) {
                    //cant read past end
                    lngError = 513;
                }
                if (lngError > 0) {
                    WinAGIException wex = new(LoadResString(lngError)) {
                        HResult = WINAGI_ERR + lngError,
                    };
                    throw wex;
                }
                //new position is ok; assign it
                mlngCurPos = Pos;
            }

            //if reading in MS-LS format,
            if (MSLS) {
                //read the two bytes, high first
                bytHigh = mRData[mlngCurPos];
                bytLow = mRData[mlngCurPos + 1];
            }
            else {
                //read the two bytes, low first
                bytLow = mRData[mlngCurPos];
                bytHigh = mRData[mlngCurPos + 1];
            }
            //adjust intCount
            mlngCurPos += 2;
            //check for end of resource
            mblnEORes = (mlngCurPos == mRData.Length);
            //calculate word Value
            return (ushort)((bytHigh << 8) + bytLow);
        }

        public byte ReadByte(int Pos = MAX_RES_SIZE + 1) {
            int lngError = 0;
            //if not loaded
            if (!mLoaded) {
                //error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563,
                };
                throw wex;
            }
            //if a position is passed
            if (Pos != MAX_RES_SIZE + 1) {
                //validate position
                if (Pos < 0) {
                    //no negatives allowed
                    lngError = 514;
                }
                else if (Pos >= mRData.Length) {
                    //cant read past end
                    lngError = 513;
                }
                if (lngError > 0) {
                    WinAGIException wex = new(LoadResString(lngError)) {
                        HResult = WINAGI_ERR + lngError,
                    };
                    throw wex;
                }
                //new position is ok; assign it
                mlngCurPos = Pos;
            }
            byte bRetVal = mRData[mlngCurPos];
            mlngCurPos++;

            //set end of resource Value
            mblnEORes = (mlngCurPos == mRData.Length);
            return bRetVal;
        }

        public void InsertData(dynamic NewData, int InsertPos = -1) {
            //inserts newdata into resource at insertpos, if passed,
            //or at end of resource, if not passed

            int i, lngResEnd, lngNewDatLen;
            byte[] bNewData = [0];
            //if not loaded
            if (!mLoaded) {
                //error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563,
                };
                throw wex;
            }

            if (NewData is byte[] bData) {
                //array of bytes = OK
                bNewData = bData;
            }
            else if (NewData is byte newByte) {
                //single byte = OK
                // ok
                bNewData[0] = newByte;
            }
            else {
                //anything that can be converted to single byte = OK
                if (NewData is string) {
                    if (byte.TryParse(NewData, out bNewData[0])) {
                        //ok
                    }
                    else {
                        //not ok
                        throw new ArgumentException("Data Type Mismatch");
                    }
                }
                else {
                    try {
                        bNewData[0] = (byte)NewData;
                    }
                    catch (Exception) {
                        //not ok
                        throw new ArgumentException("Data Type Mismatch");
                    }
                }
            }
            //get resource end and length of data being inserted
            lngResEnd = mRData.Length;
            lngNewDatLen = bNewData.Length;

            //if no insert pos passed,
            if (InsertPos == -1) {
                //insert at end
                mRData.ReSize(mRData.Length + lngNewDatLen);
                for (i = 0; i < lngNewDatLen; i++) {
                    mRData[lngResEnd + i] = bNewData[i];
                }
            }
            else {
                //validate insert pos
                if (InsertPos < 0) {
                    //error
                    WinAGIException wex = new(LoadResString(620)) {
                        HResult = WINAGI_ERR + 620,
                    };
                    throw wex;
                }
                if (InsertPos >= mRData.Length) {
                    //error
                    WinAGIException wex = new(LoadResString(620)) {
                        HResult = WINAGI_ERR + 620,
                    };
                    throw wex;
                }
                // insert pos is OK
                //increase array size
                mRData.ReSize(mRData.Length + lngNewDatLen);
                //move current data forward to make room for inserted data
                for (i = lngResEnd + lngNewDatLen - 1; i >= InsertPos + lngNewDatLen; i--) {
                    mRData[i] = mRData[i - lngNewDatLen];
                }
                //add newdata at insertpos
                for (i = 0; i < lngNewDatLen; i++) {
                    mRData[InsertPos + i] = bNewData[i];
                }
            }
            //raise change event
            OnPropertyChanged("Data");
        }

        public void RemoveData(int RemovePos, int RemoveCount = 1) {
            //removes data from RemovePos; if a Count
            //is passed, that number of bytes removed; if no
            //Count is passed, then only one byte removed

            int i, lngResEnd;

            //if not loaded
            if (!mLoaded) {
                //error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563,
                };
                throw wex;
            }

            lngResEnd = mRData.Length - 1;

            //validate Count
            if (RemoveCount <= 0) {
                //force back to 1
                RemoveCount = 1;
            }

            //validate removepos
            if (RemovePos < 0) {
                //error
                WinAGIException wex = new(LoadResString(620)) {
                    HResult = WINAGI_ERR + 620,
                };
                throw wex;
            }
            if (RemovePos >= mRData.Length) {
                //error
                WinAGIException wex = new(LoadResString(620)) {
                    HResult = WINAGI_ERR + 620,
                };
                throw wex;
            }
            //remove by moving data backwards
            for (i = RemovePos; i >= (mRData.Length - RemoveCount - 1); i++) {
                mRData[i] = mRData[i + RemoveCount];
            } // for i
            mRData.ReSize(mRData.Length - RemoveCount);
            //raise change event
            OnPropertyChanged("Data");
        }
        public virtual void Clear() {
            //if not loaded
            if (!mLoaded) {
                //error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563,
                };
                throw wex;
            }

            //clears the resource data
            mRData.Clear();
            mlngCurPos = 0;
            //mSizeInVol is undefined
            mSizeInVol = -1;
            mblnEORes = false;
            mIsDirty = true;
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