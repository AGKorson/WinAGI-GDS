using System;
using System.IO;
using static WinAGI.Engine.Base;
using WinAGI.Common;
using static WinAGI.Common.Base;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace WinAGI.Engine {
    /// <summary>
    /// A base class that represents an AGI resource (logic, picture, view, sound). The base class
    /// encapsulates the methods and properties needed to extract resources from VOL/DIR files or 
    /// from stand alone files, as well as reading and writing of the raw data so the derived
    /// classes can expose the resource in the proper formats.
    /// </summary>
    public class AGIResource {
        #region Local Members
        protected bool mLoaded = false;
        /// <summary>
        /// Less than zero means unreadable data<br />
        /// Zero means no errors <br />
        /// Greater than zero means minor errors but resource is readable
        /// </summary>
        protected int mErrLevel = 0; // <0 means unreadable data; 0 means no errors; >0 means minor errors but resource is readable
        protected string mResID;
        protected sbyte mVolume = -1;
        protected int mLoc = -1;
        /// <summary>
        /// actual size; if compressed this is different from mSizeInVol
        /// </summary>
        protected int mSize = -1;
        /// <summary>
        /// size as stored in VOL file; only different from mSize if it is an unaltered
        /// v3 resource that is compressed
        /// </summary>
        protected int mSizeInVol = -1;
        /// <summary>
        /// Byte array with raw resource data as stored in the VOL files
        /// </summary>      
        protected byte[] mData = [];
        protected bool mInGame;
        internal AGIGame parent;
        internal bool mIsDirty;
        protected byte mResNum;
        protected string mDescription = "";
        protected readonly AGIResType mResType;
        protected string mResFile = "";
        /// <summary>
        /// flag indicating pointer is at beginning of resource (CurPos=0)
        /// </summary>
        private bool mblnEORes;
        /// <summary>
        /// current position of pointer in resource data
        /// </summary>
        private int mlngCurPos;
        string[] mErrData = ["", "", "", "", ""];
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new resource of specified type. The base resource can 
        /// only be created from within one of the derived types.
        /// </summary>
        /// <param name="ResType"></param>
        protected AGIResource(AGIResType ResType) {
            mResType = ResType;
            // New resources start out NOT in game; so vol and loc are undefined
            mInGame = false;
            mVolume = -1;
            mLoc = -1;
            // assume no compression
            V3Compressed = 0;
            // calling resource constructor is responsible for creating default data
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of the VOL file where this resource is located.
        /// </summary>
        /// <returns>VOL number if resource is in a game, -1 if resource is not in a game
        /// </returns>
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

        /// <summary>
        /// Gets the position within the VOL file where this resource is located.
        /// </summary>
        /// <returns>
        /// Location in VOL file if resource is in a game, -1 if resource is not in a game
        /// </returns>
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

        /// <summary>
        /// Returns the uncompressed size of the resource. Location of
        /// the resource doesn't matter. All resources have a size.
        /// </summary>
        public int Size {
            get {
                if (mInGame) {
                    if (mLoaded) {
                        return mData.Length;
                    }
                    else {
                        return mSize;
                    }
                }
                else {
                    // if not in game, resource is always loaded
                    return mData.Length;
                }
            }
            protected set {
                // only subclass can set size, when initializing a new game resource
                mSize = value;
            }
        }

        /// <summary>
        /// Returns the size of the resource in the VOL file. Only applicable to resources 
        /// that are in a game.
        /// </summary>
        public virtual int SizeInVOL {
            get {
                // if IN a game:
                //  - SizeInVol - read from wag during initial load; -1 if resource is unreadable
                //  if NOT in a game:
                //  - SizeInVol - is always -1

                if (mInGame) {
                    if (mErrLevel < 0) {
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

        /// <summary>
        /// Gets or sets the current compressed state of this resource.
        /// </summary>
        /// <returns>0 = not compressed<br />
        /// 1 = picture compression<br />
        /// 2 = LZW compression<br />
        /// </returns>
        public int V3Compressed { get; internal set; }

        /// <summary>
        /// Gets or sets the in-game status of this resource, i.e. whether it's in a game
        /// or a stand alone resource.
        /// </summary>
        public bool InGame { get { return mInGame; } internal set { mInGame = value; } }

        /// <summary>
        /// Gets the resource number (index in DIR file) of this resource. The number
        /// property is meaningless for resources that are not in a game.
        /// </summary>
        public byte Number {
            get { if (mInGame) return mResNum; return 0; }
            internal set { mResNum = value; }
        }

        /// <summary>
        /// Gets the load status of this resource. The raw resource data and derived 
        /// resource properties (pictures, view loops/cels, etc) are not accessible if 
        /// the resource is not loaded. If not in a game, resources always loaded.
        /// </summary>
        public bool Loaded { get => mLoaded; internal set { } }

        /// <summary>
        /// Gets the error level for this resource. <br />
        /// </summary>
        /// <returns>
        /// Less than zero = unreadable data<br />
        /// Zero = no errors <br />
        /// Greater than zero = minor errors but resource is readable <br />
        /// (varies for each type of derived resource)
        /// </returns>
        public int ErrLevel { get => mErrLevel; internal set { } }

        /// <summary>
        /// Gets individualized error data associated with the error level for 
        /// this resource. Varies for each type of derived resource.
        /// </summary>
        public string[] ErrData {
            get => mErrData;
            private protected set => mErrData = value;
        }

        /// <summary>
        /// Gets or sets the resource file of a resource that is not in a game. 
        /// </summary>
        /// <returns>null string, if in a game
        /// full filename if not in a game
        /// </returns>
        public string ResFile {
            get {
                if (mInGame) {
                    return "";
                }
                else {
                    return mResFile;
                }
            }
            internal set {
                // TODO: validate name?
                mResFile = value;
            }
        }

        /// <summary>
        /// When true, indicates resource properties in resource need to be updated
        /// in the game's WAG file. Meaningless if resource is not in a game.
        /// </summary>
        internal bool PropsDirty { get; set; }

        /// <summary>
        /// For resources in a game, IsDirty is true if the data in the resource does not
        /// match the data in the VOL file. For resources not in a game, IsDirty is true
        /// if the data in the resource does not match the data in the resource file.
        /// </summary>
        public virtual bool IsDirty {
            get {
                // if not loaded, resource is never dirty
                if (mLoaded) {
                    return mIsDirty;
                }
                else {
                    return false;
                }
            }
            internal set {
                // DEBUG: should only change if resource is loaded
                Debug.Assert(mLoaded);
                mIsDirty = value;
            }
        }

        /// <summary>
        /// Gets the derived resource type of this resource.
        /// </summary>
        public AGIResType ResType { get { return mResType; } }

        /// <summary>
        /// Gets or sets the cursor position in the raw data array. Used for reading and
        /// writing data to/from the array.
        /// </summary>
        public int Pos {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mlngCurPos;
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
                if (value < 0 || value > mData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                mlngCurPos = value;
                mblnEORes = (mlngCurPos == mData.Length);
                return;
            }
        }

        /// <summary>
        /// Returns true if the raw data cursor position is at the end of the resource 
        /// data. Meaningless if resource is not loaded.
        /// </summary>
        public bool EORes {
            get {
                // TODO: maybe just return false?
                WinAGIException.ThrowIfNotLoaded(this);
                return mlngCurPos >= mData.Length;
            }
        }

        /// <summary>
        /// Gets or sets the raw data (byte array) for this resource. 
        /// </summary>
        public byte[] Data {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mData;
            }
            internal set {
                // can only set the data object internally
                mData = value;
            }
        }

        /// <summary>
        /// Gets or sets the ID for this resource. IDs are used by WinAGI to identify
        /// resources in source code. When in a game, IDs must be unique across all
        /// resources of all types. When not in a games IDs do not need to be unique
        /// but have no functionality.
        /// </summary>
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
                if (INVALID_FIRST_CHARS.Any(ch => ch == value[0])) {
                    value = "_" + value[1..];
                }
                if (value.Any(INVALID_ID_CHARS.Contains)) {
                    // replace them with '_'
                    StringBuilder sb = new(value);
                    foreach (char c in INVALID_ID_CHARS) {
                        sb.Replace(c, '_');
                    }
                    value = sb.ToString();
                }
                if (value.Any(ch => ch > 127) || value.Any(ch => ch < 32)) {
                    StringBuilder sb = new(value);
                    for (int i = 0; i < sb.Length; i++) {
                        if (sb[i] < 32 || sb[i] > 127) {
                            sb[i] = '_';
                        }
                    }
                }

                if (NewID != mResID) {
                    // ingame resources must have unique IDs
                    if (mInGame) {
                        if (NotUniqueID(this)) {
                            return;
                        }
                    }
                    mResID = NewID;
                    LogicCompiler.blnSetIDs = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets a text field that can be used for any purpose. The description
        /// property is stored in the games' WinAGI Game file, but is not used in any other
        /// way. If not in a game, no use is made of the description property.
        /// </summary>
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
        #endregion

        #region Methods
        /// <summary>
        /// Attaches a new resource to a game after the game has been opened and initialized.
        /// This allows new resources to be added to the game.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        protected void InitInGame(AGIGame parent, byte ResNum) {
            this.parent = parent;
            mInGame = true;
            mResNum = ResNum;
            // ingame resources start loaded
            mLoaded = true;
        }

        /// <summary>
        /// Attaches an existing resource from a VOL file to a game. Used during initial loading
        /// of the game to add all existing resources.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="VOL"></param>
        /// <param name="Loc"></param>
        protected void InitInGame(AGIGame parent, AGIResType resType, byte resNum, sbyte VOL, int Loc) {
            this.parent = parent;
            mInGame = true;
            mResNum = resNum;
            mVolume = VOL;
            mLoc = Loc;
            // ID should be in the propertyfile
            mResID = parent.agGameProps.GetSetting(resType.ToString() + resNum, "ID", "", true);
            if (ID.Length == 0) {
                // ID not found; save default ID
                ID = resType.ToString() + resNum;
                parent.WriteGameSetting(ID, "ID", ID, resType.ToString());
            }
            mDescription = parent.agGameProps.GetSetting(resType.ToString() + resNum, "Description", "");
        }

        /// <summary>
        /// Gets the number of bytes that this resource takes up in its VOL file. 
        /// </summary>
        /// <returns>Size of this resource in its VOL file</returns>
        internal int GetSizeInVOL() {
            // if an error occurs while trying to read the size of this
            // resource, the function returns -1
            byte bytHigh, bytLow;
            string strLoadResFile;
            FileStream fsVOL = null;
            BinaryReader brVOL = null;

            if (parent.agIsVersion3) {
                strLoadResFile = parent.agGameDir + parent.agGameID + "VOL." + mVolume.ToString();
            }
            else {
                strLoadResFile = parent.agGameDir + "VOL." + mVolume.ToString();
            }
            try {
                fsVOL = new FileStream(strLoadResFile, FileMode.Open);
                brVOL = new BinaryReader(fsVOL);
                if (fsVOL.Length >= mLoc + (parent.agIsVersion3 ? 7 : 5)) {
                    fsVOL.Seek(mLoc, SeekOrigin.Begin);
                    bytLow = brVOL.ReadByte();
                    bytHigh = brVOL.ReadByte();
                    //verify this is a proper resource
                    if ((bytLow == 0x12) && (bytHigh == 0x34)) {
                        // now get the low and high bytes of the size
                        fsVOL.Seek(1, SeekOrigin.Current);
                        bytLow = brVOL.ReadByte();
                        bytHigh = brVOL.ReadByte();
                        return (bytHigh << 8) + bytLow;
                    }
                }
            }
            catch {
                // treat all errors the same
            }
            finally {
                // ensure file is closed
                brVOL.Dispose();
                fsVOL.Dispose();
            }
            // if size not found, return -1
            return -1;
        }

        /// <summary>
        /// Copies entire resource structure from this resource into NewRes.
        /// InGame property is never copied; only way to change ingame status is to use
        /// methods for adding/removing resources to/from game.
        /// </summary>
        /// <param name="NewRes"></param>
        internal void CloneTo(AGIResource NewRes) {

            NewRes.parent = parent;
            // derived resource data are copied by calling method
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
            NewRes.Data = mData;
            NewRes.mIsDirty = mIsDirty;
            NewRes.PropsDirty = PropsDirty;
            NewRes.mSize = mSize;
            NewRes.mSizeInVol = mSizeInVol;
            NewRes.mblnEORes = mblnEORes;
            NewRes.mlngCurPos = mlngCurPos;
            NewRes.ErrLevel = mErrLevel;
            NewRes.ErrData = ErrData;
        }

        /// <summary>
        /// Loads the data for this resource from its VOL file, if in a game,
        /// or from its ressource file, if not in a game.
        /// </summary>
        public virtual void Load() {
            byte bytLow, bytHigh, bytVolNum;
            bool blnIsPicture = false;
            int diskSize, fullSize = 0;
            string strLoadResFile;

            if (mLoaded) {
                return;
            }
            // always return success
            mLoaded = true;
            mIsDirty = false;
            PropsDirty = false;
            // clear error info before loading
            mErrLevel = 0;
            mErrData = ["", "", "", "", ""];

            if (mInGame) {
                if (parent.agIsVersion3) {
                    strLoadResFile = parent.agGameDir + parent.agGameID + "VOL." + mVolume.ToString();
                }
                else {
                    strLoadResFile = parent.agGameDir + "VOL." + mVolume.ToString();
                }
            }
            else {
                strLoadResFile = mResFile;
            }
            if (!File.Exists(strLoadResFile)) {
                mErrLevel = -1;
                ErrData[0] = strLoadResFile;
                ErrData[1] = mResID;
                return;
            }
            if ((File.GetAttributes(strLoadResFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                mErrLevel = -2;
                ErrData[0] = strLoadResFile;
                return;
            }
            // open file (VOL or individual resource)
            FileStream fsVOL = null;
            BinaryReader brVOL = null;
            try {
                fsVOL = new FileStream(strLoadResFile, FileMode.Open);
                brVOL = new BinaryReader(fsVOL);
            }
            catch (Exception e1) {
                fsVOL?.Dispose();
                brVOL?.Dispose();
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
            if (mInGame) {
                // check for valid header
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
                fullSize = (bytHigh << 8) + bytLow;
                // if version3,
                if (parent.agIsVersion3) {
                    // compressed size
                    bytLow = brVOL.ReadByte();
                    bytHigh = brVOL.ReadByte();
                    diskSize = (bytHigh << 8) + bytLow;
                }
                else {
                    diskSize = fullSize;
                }
            }
            else {
                // get size from total file length
                fullSize = (int)fsVOL.Length;
                // version 3 files are never compressed when loaded as individual files
                diskSize = fullSize;
            }
            // get resource data
            mData = new byte[diskSize];
            mData = brVOL.ReadBytes(diskSize);
            fsVOL.Dispose();
            brVOL.Dispose();
            if (blnIsPicture) {
                // pictures use this decompression
                V3Compressed = 1;
                mData = DecompressPicture(mData, fullSize);
            }
            else {
                if (mData.Length != fullSize) {
                    // all other resources use LZW compression
                    V3Compressed = 2;
                    mData = AGILZW.ExpandV3ResData(mData, fullSize);
                }
            }
            // reset resource markers
            mlngCurPos = 0;
            mblnEORes = false;
            // update size property
            mSize = fullSize;
            return;
        }

        /// <summary>
        /// Unloads an in-game resource. If resource is not in a game, this method does
        /// nothing.
        /// </summary>
        public virtual void Unload() {
            // reset flag first so size doesn't get cleared for in game resources
            mLoaded = false;
            mIsDirty = false;
            // reset resource variables
            mData = [];
            // don't mess with sizes though! they remain accessible even when unloaded
            mblnEORes = true;
            mlngCurPos = 0;
        }

        /// <summary>
        /// This method saves the resource - into a VOL file if in a game or to its
        /// resource file if not in a game.
        /// </summary>
        internal void Save() {
            WinAGIException.ThrowIfNotLoaded(this);
            if (mInGame) {
                try {
                    VOLManager.UpdateInVol(this);
                    // update saved size
                    mSize = mData.Length;
                    // when saving in WinAGI, resource are never compressed
                    V3Compressed = 0;
                    mSizeInVol = mSize;
                }
                catch {
                    // pass error along
                    throw;
                }
                // change date of last edit
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
            // no longer dirty
            mIsDirty = false;
        }

        /// <summary>
        /// Exports this resource to a standalone file. If the resource is not in a game,
        /// exporting to the resource's current resource file name is the same as saving.
        /// </summary>
        /// <param name="ExportFile"></param>
        public void Export(string ExportFile) {
            // MUST specify a valid export file
            // if export file already exists, it is overwritten
            // caller is responsible for verifying overwrite is ok or not

            WinAGIException.ThrowIfNotLoaded(this);
            if (ExportFile.Length == 0) {
                WinAGIException wex = new(LoadResString(599)) {
                    HResult = WINAGI_ERR + 599,
                };
                throw wex;
            }
            // resources with major errors can't be exported
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
                fsExport.Write(mData, 0, mData.Length);
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
            }
            // if NOT in a game,
            if (!mInGame) {
                // change resfile to match new export filename
                mResFile = ExportFile;
                // ID always tracks the resfile name
                mResID = Path.GetFileName(ExportFile);
                if (mResID.Length > 64) {
                    mResID = Left(mResID, 64);
                }
            }
        }

        /// <summary>
        /// Imports resource from a file, and loads it. Existing data are overwritten
        /// without warning. If in a game, it also saves the new resource in a VOL
        /// file. Import does not check if the imported resource is valid or not, nor
        /// if it is of the correct resource type. The calling program is responsible
        /// for that check.
        /// </summary>
        /// <param name="ImportFile"></param>
        public virtual void Import(string ImportFile) {
            if (ImportFile is null || ImportFile.Length == 0) {
                WinAGIException wex = new(LoadResString(604)) {
                    HResult = WINAGI_ERR + 604,
                };
                throw wex;
            }
            if (!File.Exists(ImportFile)) {
                WinAGIException wex = new(LoadResString(524).Replace(ARG1, ImportFile)) {
                    HResult = WINAGI_ERR + 524,
                };
                wex.Data["missingfile"] = ImportFile;
                throw wex;
            }
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
            mData = new byte[(int)fsImport.Length];
            fsImport.Read(mData, 0, (int)fsImport.Length);
            // reset resource markers
            mlngCurPos = 0;
            mblnEORes = false;
            mIsDirty = false;
            PropsDirty = false;
            // update size property
            mSize = (int)fsImport.Length;
            // always return success
            mLoaded = true;
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
                // update the resource filename to match
                mResFile = ImportFile;
            }
            // set ID to the filename without extension;
            // the calling function will take care or reassigning it later, if needed
            // (for example, if being added to a game)
            string tmpID = Path.GetFileNameWithoutExtension(ImportFile);
            if (tmpID.Length > 64) {
                tmpID = tmpID[..64];
            }
            if (mInGame) {
                if (NotUniqueID(tmpID, parent)) {
                    int i = 0;
                    string baseid = mResID;
                    do {
                        mResID = baseid + "_" + i++.ToString();
                    }
                    while (NotUniqueID(this));
                }
            }
            mResID = tmpID;
        }

        /// <summary>
        /// Writes a byte to the raw data at specified location. If no location is 
        /// specified (Pos = 0), the write location is current cursor position. If 
        /// at end of data, the data array is expanded. The cursor position is updated 
        /// after writing.
        /// </summary>
        /// <param name="InputByte"></param>
        /// <param name="Pos"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void WriteByte(byte InputByte, int Pos = -1) {

            WinAGIException.ThrowIfNotLoaded(this);
            if (Pos != -1) {
                // validate location
                if (Pos < 0 || Pos > mData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(Pos));
                }
            }
            else {
                // otherwise, default to end of resource
                Pos = mData.Length;
            }
            if (Pos == mData.Length) {
                //adjust to make room for new data being added
                Array.Resize(ref mData, mData.Length + 1);
            }
            mData[Pos] = InputByte;
            mlngCurPos = Pos + 1;
            mblnEORes = (mlngCurPos == mData.Length);
            return;
        }

        /// <summary>
        /// Writes a word (two-byte value) to the raw data at specified location. 
        /// If no location is specified (Pos = 0), the write location is current
        /// cursor position. If at end of data, the data array is expanded. The
        /// format can be specified as 'most significant first' if needed. The 
        /// cursor position is updated after writing.
        /// </summary>
        /// <param name="InputInt"></param>
        /// <param name="Pos"></param>
        /// <param name="blnMSLS"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void WriteWord(ushort InputInt, int Pos = -1, bool blnMSLS = false) {
            byte bytHigh, bytLow;

            WinAGIException.ThrowIfNotLoaded(this);
            if (Pos != -1) {
                if (Pos < 0 || Pos > mData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(Pos));
                }
            }
            else {
                // default to end of resource
                Pos = mData.Length;
            }
            // if necessary, adjust to make room for new data being added
            if (Pos == mData.Length) {
                Array.Resize(ref mData, mData.Length + 2);
            }
            // if one byte away from end of resource
            else if (Pos == mData.Length - 1) {
                Array.Resize(ref mData, mData.Length + 1);
            }
            // split integer portion into two bytes and add it
            bytHigh = (byte)(InputInt >> 8);
            bytLow = (byte)(InputInt % 256);
            if (blnMSLS) {
                mData[Pos] = bytHigh;
                mData[Pos + 1] = bytLow;
            }
            else {
                mData[Pos] = bytLow;
                mData[Pos + 1] = bytHigh;
            }
            mlngCurPos = Pos + 2;
            mblnEORes = (mlngCurPos == mData.Length);
        }

        /// <summary>
        /// Reads a byte from the raw data at the specified location. If no location is 
        /// specified (Pos = 0), the write location is current cursor position. The cursor
        /// position is updated after reading.
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte ReadByte(int Pos = -1) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (Pos != -1) {
                if (Pos < 0 || Pos >= mData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(Pos));
                }
                mlngCurPos = Pos;
            }
            byte retval = mData[mlngCurPos++];
            mblnEORes = (mlngCurPos == mData.Length);
            return retval;
        }

        /// <summary>
        /// Reads a word (two-byte value) from the raw data at the specified location. If
        /// no location is specified (Pos = 0), the write location is current cursor
        /// position. The format of the word can be specified as 'most significant first'
        /// if needed. The cursor position is updated after the read.
        /// </summary>
        /// <param name="Pos"></param>
        /// <param name="MSLS"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ushort ReadWord(int Pos = -1, bool MSLS = false) {
            byte bytLow, bytHigh;

            WinAGIException.ThrowIfNotLoaded(this);

            if (Pos != -1) {
                if (Pos < 0 || Pos >= mData.Length - 1) {
                    throw new ArgumentOutOfRangeException(nameof(Pos));
                }
                mlngCurPos = Pos;
            }
            if (MSLS) {
                // high first
                bytHigh = mData[mlngCurPos];
                bytLow = mData[mlngCurPos + 1];
            }
            else {
                // low first
                bytLow = mData[mlngCurPos];
                bytHigh = mData[mlngCurPos + 1];
            }
            mlngCurPos += 2;
            mblnEORes = mlngCurPos == mData.Length;
            return (ushort)((bytHigh << 8) + bytLow);
        }

        /// <summary>
        /// Inserts a variable number of byte data into resource at the specified
        /// position. If no location is specified (Pos = 0), the new data are 
        /// inserted at the end of the array. The cursor position is not
        /// affected by insertions.
        /// </summary>
        /// <param name="NewData"></param>
        /// <param name="InsertPos"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void InsertData(dynamic NewData, int InsertPos = -1) {
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
            lngResEnd = mData.Length;
            lngNewDatLen = bNewData.Length;

            if (InsertPos != -1) {
                // validate position
                if (Pos < 0 || Pos >= mData.Length) {
                    throw new ArgumentOutOfRangeException(nameof(InsertPos));
                }
                // insert pos is OK
                Array.Resize(ref mData, mData.Length + lngNewDatLen);
                // move current data forward to make room for inserted data
                for (i = lngResEnd + lngNewDatLen - 1; i >= InsertPos + lngNewDatLen; i--) {
                    mData[i] = mData[i - lngNewDatLen];
                }
                // add newdata at insertpos
                for (i = 0; i < lngNewDatLen; i++) {
                    mData[InsertPos + i] = bNewData[i];
                }
            }
            else {
                // insert at end
                Array.Resize(ref mData, mData.Length + lngNewDatLen);
                for (i = 0; i < lngNewDatLen; i++) {
                    mData[lngResEnd + i] = bNewData[i];
                }
            }
        }

        /// <summary>
        /// Removes the desired number of bytes from the data array, beginning at
        /// the specified location. Default number of bytes to remove is one. The
        /// array is compressed after removing the specified section of data.
        /// </summary>
        /// <param name="RemovePos"></param>
        /// <param name="RemoveCount"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void RemoveData(int RemovePos, int RemoveCount = 1) {
            int i;

            WinAGIException.ThrowIfNotLoaded(this);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(RemoveCount);
            if (RemovePos < 0 || RemovePos >= mData.Length) {
                throw new ArgumentOutOfRangeException(nameof(RemovePos));
            }
            // adjust count so it does not exceed array length
            if (RemovePos + RemoveCount > mData.Length) {
                RemoveCount = mData.Length - RemovePos;
            }
            for (i = RemovePos; i >= (mData.Length - RemoveCount); i++) {
                mData[i] = mData[i + RemoveCount];
            }
            Array.Resize(ref mData, mData.Length - RemoveCount);
        }

        /// <summary>
        /// This is an internal method to clear a resource without adjusting its error
        /// status.
        /// </summary>
        private protected void ErrClear() {
            // cache error info
            int errlevel = mErrLevel;
            string[] errdata = ErrData;
            // use public clear method
            Clear();
            // restore error info
            mErrLevel = errlevel;
            ErrData = errdata;
        }

        /// <summary>
        /// Clears the resource by replacing data with an empty array, and clearing
        /// error information.
        /// </summary>
        public virtual void Clear() {
            WinAGIException.ThrowIfNotLoaded(this);
            //clears the resource data
            mData = [];
            mlngCurPos = 0;
            // mSizeInVol is undefined
            mSizeInVol = -1;
            mblnEORes = false;
            mIsDirty = true;
            mErrLevel = 0;
            mErrData = ["", "", "", "", ""];
        }

        /// <summary>
        /// Displays the resource ID as the string representation of this resource.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            if (mResID.Length > 0) {
                return mResID;
            }
            return "blank " + mResType.ToString();
        }

        /// <summary>
        /// This method finds a unique file name (one not in use in the current game's
        /// resource directory) based on the specified resource type.
        /// </summary>
        /// <param name="ResType"></param>
        /// <returns></returns>
        internal string UniqueResFile(AGIResType ResType) {
            int intNo = 0;
            string retval;
            do {
                intNo++;
                retval = parent.agResDir + "New" + ResType.ToString() + intNo + ".ag" + ResType.ToString().ToLower()[0];
            }
            while (File.Exists(retval));
            return retval;
        }
        #endregion
    }
}