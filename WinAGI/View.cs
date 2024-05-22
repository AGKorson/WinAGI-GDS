using System;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents an AGI View resource, with WinAGI extensions.
    /// </summary>
    public class View : AGIResource {
        #region Members
        /// <summary>
        /// True if the view resource data does not match the current view
        /// loop/cel/description objects.
        /// </summary>
        bool mViewSet;
        internal Loops mLoopCol;
        string mViewDesc;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor to create a new AGI view resource that is not part of an AGI game.
        /// </summary>
        public View() : base(AGIResType.View) {
            InitView();
            // create default ID
            mResID = "NewView";
            // if not in a game, resource is always loaded
            mLoaded = true;
        }

        /// <summary>
        /// Internal constructor to create a new or cloned view resource to  be added
        /// to an AGI game has already been loaded. 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="NewView"></param>
        internal View(AGIGame parent, byte ResNum, View NewView = null) : base(AGIResType.View) {
            InitView(NewView);
            base.InitInGame(parent, ResNum);
        }

         /// <summary>
        /// Internal constructor to add a new AGI view resource during initial game load.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="VOL"></param>
        /// <param name="Loc"></param>
        internal View(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.View) {
            InitView(null);
            base.InitInGame(parent, AGIResType.View, ResNum, VOL, Loc);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the specified loop from this view.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Loop this[int index] {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (index < 0 || index >= mLoopCol.Count) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return Loops[index];
            }
        }

        /// <summary>
        /// Gets the loop collection for this view.
        /// </summary>
        public Loops Loops {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mLoopCol;
            }
        }

        /// <summary>
        /// Gets or sets the AGI View Description property.
        /// </summary>
        public string ViewDescription {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mViewDesc;
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mViewDesc != value) {
                    mViewDesc = Left(value, 255);
                    mViewSet = false;
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new view resource when first instantiated. If NewView is null, 
        /// a blank view resource is created. If NewView is not null, it is cloned into
        /// the new view.
        /// </summary>
        /// <param name="NewView"></param>
        private void InitView(View NewView = null) {
            if (NewView is null) {
                // TODO: should this add a one-loop/one-cel/one-pixel view instead?
                // add empty loop col
                mLoopCol = new Loops(this);
                mData = [0x01, 0x01, 0x00, 0x00, 0x00];
                // byte0 = unknown (always 1 or 2?)
                // byte1 = unknown (always 1?)
                // byte2 = loop count
                // byte3 = high byte of viewdesc
                // byte4 = low byte of viewdesc
                mViewSet = true;
                mViewDesc = "";
            }
            else {
                // copy base properties
                NewView.CloneTo(this);
                mViewSet = NewView.mViewSet;
                mViewDesc = NewView.mViewDesc;
                mLoopCol = NewView.mLoopCol.Clone(this);
            }
        }

        /// <summary>
        /// Creates an exact copy of this View resource.
        /// </summary>
        /// <returns>The View resource this method creates.</returns>
        public View Clone() {
            View CopyView = new();
            // copy base properties
            CloneTo(CopyView);
            // copy view properties
            CopyView.mViewSet = mViewSet;
            CopyView.mViewDesc = mViewDesc;
            CopyView.mLoopCol = mLoopCol.Clone(this);
            CopyView.ErrLevel = mErrLevel;
            return CopyView;
        }

        /// <summary>
        /// Resets the view to a single loop with a single cel of height and width of 1
        /// and transparent color of 0 and no description
        /// </summary>
        public override void Clear() {
            WinAGIException.ThrowIfNotLoaded(this);
            base.Clear();
            mViewDesc = "";
            mLoopCol = new Loops(this);
            mLoopCol.Add(0);
            mLoopCol[0].Cels.Add(0);
            mViewSet = false;
            mIsDirty = true;
        }

        /// <summary>
        /// Converts this view's loop/cel collection into a valid AGI byte array
        /// and stores it in this resource's data.
        /// </summary>
        void CompileView() {
            int[] lngLoopOffset, lngCelOffset;
            int i, j;
            byte bytTransCol;
            int k;
            byte[] bytCelData;
            bool blnMirrorAdded;
            mData = [];

            // header
            WriteByte(1, 0);
            WriteByte(1);
            // number of loops
            WriteByte((byte)mLoopCol.Count);
            // placeholder for description
            WriteWord(0);
            // initialize loop offset array
            lngLoopOffset = new int[mLoopCol.Count];
            // place holders for loop positions
            for (i = 0; i < mLoopCol.Count; i++) {
                WriteWord(0);
            }
            // step through all loops to add them
            for (i = 0; i < mLoopCol.Count; i++) {
                // if loop is mirrored AND already added
                // (can tell if not added by comparing the mirror loop
                // property against current loop being added)
                if (mLoopCol[i].Mirrored != 0) {
                    blnMirrorAdded = (mLoopCol[i].MirrorLoop < i);
                }
                else {
                    blnMirrorAdded = false;
                }
                if (blnMirrorAdded) {
                    // loop offset is same as mirror
                    lngLoopOffset[i] = lngLoopOffset[mLoopCol[i].MirrorLoop];
                }
                else {
                    lngLoopOffset[i] = Pos;
                    WriteByte((byte)mLoopCol[i].Cels.Count);
                    lngCelOffset = new int[mLoopCol[i].Cels.Count];
                    // placeholders for cel offsets
                    for (j = 0; j < mLoopCol[i].Cels.Count; j++) {
                        WriteWord(0);
                    }
                    // step through all cels to add them
                    for (j = 0; j < mLoopCol[i].Cels.Count; j++) {
                        lngCelOffset[j] = Pos - lngLoopOffset[i];
                        WriteByte(mLoopCol[i].Cels[j].Width);
                        WriteByte(mLoopCol[i].Cels[j].Height);
                        if (mLoopCol[i].Mirrored != 0) {
                            // set bit 7 for mirror flag and include loop number
                            // in bits 6-5-4 for transparent color
                            bytTransCol = (byte)(0x80 + i * 0x10 + mLoopCol[i].Cels[j].TransColor);
                        }
                        else {
                            // just use transparent color
                            bytTransCol = (byte)mLoopCol[i].Cels[j].TransColor;
                        }
                        WriteByte(bytTransCol);
                        // cel data
                        bytCelData = CompressedCel(mLoopCol[i].Cels[j], (mLoopCol[i].Mirrored != 0));
                        for (k = 0; k < bytCelData.Length; k++) {
                            WriteByte(bytCelData[k]);
                        }
                    }
                    // add cel offsets
                    for (j = 0; j < mLoopCol[i].Cels.Count; j++) {
                        WriteWord((ushort)lngCelOffset[j], lngLoopOffset[i] + 1 + 2 * j);
                    }
                    // restore pointer to end of resource
                    Pos = mData.Length;
                }
            }
            // step through loops again to add loop offsets
            for (i = 0; i < mLoopCol.Count; i++) {
                WriteWord((ushort)lngLoopOffset[i], 5 + 2 * i);
            }
            if (mViewDesc.Length > 0) {
                // view description offset
                WriteWord((ushort)mData.Length, 3);
                // move pointer back to end of resource
                Pos = mData.Length;
                // add view description
                byte[] desc;
                if (parent is null) {
                    desc = Encoding.GetEncoding(437).GetBytes(mViewDesc);
                }
                else {
                    desc = parent.agCodePage.GetBytes(mViewDesc);
                }
                for (i = 0; i < desc.Length; i++) {
                    WriteByte(desc[i]);
                }
                // add terminating null char
                WriteByte(0);
            }
            mViewSet = true;
            // clear error level
            mErrLevel = 0;
            ErrData = ["", "", "", "", ""];
        }

        /// <summary>
        /// This method expands the run length encoding (RLE) data beginning at
        /// position StartPos in the view resource data and passes it to a cel.
        /// </summary>
        /// <param name="StartPos"></param>
        /// <param name="aCel"></param>
        void ExpandCelData(int StartPos, Cel aCel) {
            ArgumentNullException.ThrowIfNull(aCel);
            byte bytCelY = 0;
            byte bytIn;
            byte bytChunkColor, bytChunkCount;
            byte bytWidth = aCel.Width, bytHeight = aCel.Height;
            byte bytTransColor = (byte)aCel.TransColor;
            byte[,] tmpCelData = new byte[bytWidth, bytHeight];

            // set resource to starting position
            Pos = StartPos;
            // extract pixel data
            do {
                byte bytCelX = 0;
                do {
                    // read each byte, where lower four bits are number of pixels,
                    // and upper four bits are color for these pixels
                    bytIn = ReadByte();
                    // skip zero values
                    if (bytIn > 0) {
                        // extract color
                        bytChunkColor = (byte)(bytIn / 0x10);
                        bytChunkCount = (byte)(bytIn % 0x10);
                        // add this color for correct number of pixels
                        for (int i = 0; i < bytChunkCount; i++) {
                            tmpCelData[bytCelX, bytCelY] = bytChunkColor;
                            bytCelX++;
                        }
                    }
                } while (bytIn != 0);
                // fill in rest of this line with transparent color, if necessary
                while (bytCelX < bytWidth) {
                    tmpCelData[bytCelX, bytCelY] = bytTransColor;
                    bytCelX++;
                }
                bytCelY++;
            } while (bytCelY < bytHeight);
            aCel.AllCelData = tmpCelData;
        }

        /// <summary>
        /// Generates a unique mirrorpair number that is used to identify a
        /// pair of mirrored loops. The source loop is positive, the mirror
        /// is negative.
        private int GetMirrorPair() {
            byte i;
            bool goodnum;

            byte retval = 1;
            do {
                // assume number is ok
                goodnum = true;
                for (i = 0; i < mLoopCol.Count; i++) {
                    // if this loop is using this mirror pair
                    if (retval == Math.Abs(mLoopCol[i].MirrorPair)) {
                        // try another number
                        goodnum = false;
                        break;
                    }
                }
                //if number is ok
                if (goodnum) {
                    //use this mirrorpair
                    break;
                }
                // try another
                retval++;
            } while (!goodnum);
            return retval;
        }

        /// <summary>
        /// This method is used by load function to extract the view loops
        /// and cels from the data stream.
        /// </summary>
        /// <returns>Zero if no errors, otherwise an error code</returns>
        internal int LoadLoops() {
            // 
            byte bytNumLoops, bytNumCels;
            int[] lngLoopStart = new int[MAX_LOOPS];
            ushort lngCelStart, lngDescLoc;
            byte tmpLoopNo, bytLoop, bytCel;
            byte[] bytInput = new byte[1];
            byte bytWidth, bytHeight;
            byte bytTransCol;
            int result = 0; // assume no errors

            mLoopCol = new Loops(this);
            bytNumLoops = ReadByte(2);
            // get offset to ViewDesc
            lngDescLoc = ReadWord();
            if (bytNumLoops == 0) {
                // error - invalid data
                ErrData[0] = mResID;
                mViewSet = true;
                mIsDirty = false;
                return -15;
            }
            // get loop offset data for each loop
            for (bytLoop = 0; bytLoop < bytNumLoops; bytLoop++) {
                lngLoopStart[bytLoop] = ReadWord();
                if ((lngLoopStart[bytLoop] > mSize)) {
                    // invalid loop; let any that are alreay loaded stay loaded
                    ErrData[0] = mResID;
                    ErrData[1] = bytLoop.ToString();
                    mViewSet = true;
                    mIsDirty = false;
                    return -16;
                }
            }
            for (bytLoop = 0; bytLoop < bytNumLoops; bytLoop++) {
                mLoopCol.Add(bytLoop);
                //  loop zero NEVER mirrors another loop (but others can 
                // mirror it)
                if (bytLoop > 0) {
                    // for all other loops, check to see if it mirrors an earlier loop
                    for (tmpLoopNo = 0; tmpLoopNo < bytLoop; tmpLoopNo++) {
                        if (lngLoopStart[bytLoop] == lngLoopStart[tmpLoopNo]) {
                            // confirm valid mirror
                            switch (SetMirror(bytLoop, tmpLoopNo)) {
                            // only valid return values when setting mirrors during
                            // LoadLoops are -3(loop >8) or -6(source already mirrored)
                            case -3:
                                // invalid mirror loop
                                return -17;
                            case -6:
                                // source loop already mirrored
                                return -18;
                            }
                        }
                    }
                }
                if (mLoopCol[bytLoop].Mirrored == 0) {
                    Pos = lngLoopStart[bytLoop];
                    bytNumCels = ReadByte();
                    for (bytCel = 0; bytCel < bytNumCels; bytCel++) {
                        // read starting position
                        lngCelStart = (ushort)(ReadWord(lngLoopStart[bytLoop] + 2 * bytCel + 1) + lngLoopStart[bytLoop]);
                        if ((lngCelStart > mSize)) {
                            // keep view data already loaded
                            ErrData[0] = mResID;
                            ErrData[1] = bytLoop.ToString();
                            ErrData[2] = bytCel.ToString();
                            mViewSet = true;
                            mIsDirty = false;
                            return -20;
                        }
                        bytWidth = ReadByte(lngCelStart);
                        bytHeight = ReadByte();
                        bytTransCol = ReadByte();
                        bytTransCol = (byte)(bytTransCol % 0x10);
                        // add the cel
                        mLoopCol[bytLoop].Cels.Add(bytCel, bytWidth, bytHeight, (AGIColorIndex)bytTransCol);
                        // extract bitmap data from RLE data
                        ExpandCelData(lngCelStart + 3, mLoopCol[bytLoop].Cels[bytCel]);
                    }
                }
            }
            mViewDesc = "";
            if (lngDescLoc > 0) {
                if (lngDescLoc < mSize - 1) {
                    // set resource pointer to beginning of description string
                    Pos = lngDescLoc;
                    do {
                        bytInput[0] = ReadByte();
                        //if not zero, and string not yet up to 255 characters,
                        if ((bytInput[0] > 0) && (mViewDesc.Length < 255)) {
                            if (parent is null) {
                                mViewDesc += Encoding.GetEncoding(437).GetString(bytInput);
                            }
                            else {
                                mViewDesc += parent.agCodePage.GetString(bytInput);
                            }
                        }
                    }
                    while (!EORes && bytInput[0] != 0 && mViewDesc.Length < 255);
                }
                else {
                    // pointer is not valid
                    ErrData[0] = mResID;
                    result = 1;
                }
            }
            mViewSet = true;
            mIsDirty = false;
            return result;
        }

        /// <summary>
        /// Exports this resource to a standalone file.
        /// </summary>
        /// <param name="ExportFile"></param>
        public new void Export(string ExportFile) {
            WinAGIException.ThrowIfNotLoaded(this);
            try {
                if (!mViewSet) {
                    // need to recompile
                    CompileView();
                }
                Export(ExportFile);
            }
            catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Imports a view resource from a file into this view.
        /// </summary>
        /// <param name="ImportFile"></param>
        public override void Import(string ImportFile) {
            try {
                base.Import(ImportFile);
            }
            catch (Exception) {
                // pass along error
                throw;
            }
            mErrLevel = LoadLoops();
        }

        /// <summary>
        /// Loads this view resource by reading its data from the VOL file. Only
        /// applies to views in a game. Non-game views are always loaded.
        /// </summary>
        public override void Load() {
            if (mLoaded) {
                return;
            }
            // load base resource data
            base.Load();
            if (mErrLevel < 0) {
                // return empty view, with one loop, one cel, one pixel
                ErrClear();
                mData = [1, 1, 1, 0, 0, 7, 0, 1, 3, 0, 1, 1, 0, 0];
                mViewSet = true;
                return;
            }
            // extract loops/cels
            mErrLevel = LoadLoops();
        }

        /// <summary>
        /// Unloads this view resource. Data elements are undefined and non-accessible
        /// while unloaded. Only views that are in a game can be unloaded.
        /// </summary>
        public override void Unload() {
            if (!mInGame) {
                return;
            }
            base.Unload();
            // reset loop collection
            mLoopCol = new Loops(this);
            mViewSet = false;
        }

        /// <summary>
        /// Saves properties of this view to the game's WAG file.
        /// </summary>
        public void SaveProps() {
            if (mInGame) {
                parent.WriteGameSetting("View" + Number, "ID", mResID, "Views");
                parent.WriteGameSetting("View" + Number, "Description", mDescription);
                PropsDirty = false;
            }
        }
        
        /// <summary>
        /// Saves this view resource. If in a game, it updates the DIR and VOL files. 
        /// If not in a game the view is saved to its resource file specified by the
        /// FileName property.
        /// </summary>
        public new void Save() {
            WinAGIException.ThrowIfNotLoaded(this);
            if (PropsDirty && mInGame) {
                SaveProps();
            }
            if (mIsDirty) {
                try {
                    if (!mViewSet) {
                        CompileView();
                    }
                    base.Save();
                }
                catch {
                    // pass error along
                    throw;
                }
            }
        }

        /// <summary>
        /// Sets up a mirror pair between the two specified loops.
        /// </summary>
        /// <param name="TargetLoop"></param>
        /// <param name="SourceLoop"></param>
        /// <returns>0 = loops successfully mirrored<br />
        /// -1 = invalid source loop<br />
        /// -2 = invalid target loop<br />
        /// -3 = both loops must be less than eight <br />
        /// -4 = target and source are the same<br />
        /// -5 = target is already mirrored<br />
        /// -6 = source is already mirrored
        /// </returns>
        public int SetMirror(byte TargetLoop, byte SourceLoop) {
            // source loop must already exist
            if (SourceLoop >= mLoopCol.Count) {
                return -1;
            }
            // target loop must already exist
            if (SourceLoop >= mLoopCol.Count) {
                return -2;
            }
            // mirror loops must be less than eight
            if (SourceLoop >= 8 || TargetLoop >= 8) {
                return -3;
            }
            // mirror source and target can't be the same
            if (SourceLoop == TargetLoop) {
                return -4;
            }
            // the target loop can't be already mirrored
            if (mLoopCol[TargetLoop].Mirrored != 0) {
                return -5;
            }
            // the source loop can't already have a mirror
            if (mLoopCol[SourceLoop].Mirrored != 0) {
                return -6;
            }
            // get a new mirror pair number
            int mp = GetMirrorPair();
            // set the mirror loop hasmirror property
            mLoopCol[SourceLoop].MirrorPair = mp;
            // set the mirror loop mirrorloop property
            mLoopCol[TargetLoop].MirrorPair = -mp;
            mLoopCol[TargetLoop].Cels = mLoopCol[SourceLoop].Cels;
            mIsDirty = true;
            return 0;
        }
        #endregion
    }
}
