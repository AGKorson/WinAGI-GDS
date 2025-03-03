using System;
using System.Drawing;
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
        internal bool mViewChanged;
        internal Loops mLoopCol;
        internal string mViewDesc;
        internal EGAColors mPalette;
        Encoding mCodePage = Encoding.GetEncoding(Base.CodePage.CodePage);
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor to create a new AGI view resource that is not part of
        /// an AGI game.
        /// </summary>
        public View() : base(AGIResType.View) {
            InitView();
            // create default ID
            mResID = "NewView";
            // if not in a game, resource is always loaded
            mLoaded = true;
        }

        /// <summary>
        /// Internal constructor to create a new or cloned view resource to
        /// be added to an AGI game has already been loaded. 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="NewView"></param>
        internal View(AGIGame parent, byte ResNum, View NewView = null) : base(AGIResType.View) {
            InitView(NewView);
            base.InitInGame(parent, ResNum);
            mCodePage = Encoding.GetEncoding(parent.agCodePage.CodePage);
        }

        /// <summary>
        /// Internal constructor to add a new AGI view resource during initial
        /// game load.
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
        /// Returns true if the loop/cel data do not match the AGI Resource data.
        /// </summary>
        public override bool IsChanged {
            get {
                return mViewChanged;
            }
        }
        
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
                    mViewDesc = value.Left(255);
                    mViewChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the color palette that is used if the resource is not
        /// attached to a game. 
        /// </summary>
        public EGAColors Palette {
            get {
                if (mInGame) {
                    return parent.Palette;
                }
                else {
                    return mPalette;
                }
            }
            set {
                if (!mInGame) {
                    mPalette = value.CopyPalette();
                }
            }
        }

        /// <summary>
        /// Gets or sets the character code page to use when converting 
        /// characters to or from a byte stream.
        /// </summary>
        public Encoding CodePage {
            get => mCodePage;
            set {
                if (parent is null) {
                    // confirm new codepage is supported; error if it is not
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
                // single loop, single cel, one pixel
                mLoopCol = new Loops(this);
                mData = [0x01, 0x01, 0x01, 0x00, 0x00, 0x07, 0x00, 0x01,
                         0x03, 0x00, 0x01, 0x01, 0x00, 0x00];
                mViewChanged = false;
                mViewDesc = "";
            }
            else {
                // copy base properties
                NewView.CloneTo(this);
                mViewChanged = NewView.mViewChanged;
                mViewDesc = NewView.mViewDesc;
                mLoopCol = NewView.mLoopCol.Clone(this);
            }
            mPalette = defaultPalette.CopyPalette();
        }

        /// <summary>
        /// Creates an exact copy of this View resource.
        /// </summary>
        /// <returns>The View resource this method creates.</returns>
        public View Clone() {
            // only loaded views can be cloned
            WinAGIException.ThrowIfNotLoaded(this);

            View CopyView = new();
            // copy base properties
            CloneTo(CopyView);
            // copy view properties
            CopyView.mViewDesc = mViewDesc;
            CopyView.mLoopCol = mLoopCol.Clone(CopyView);
            CopyView.ErrLevel = ErrLevel;
            CopyView.mViewChanged = mViewChanged;
            CopyView.ErrData = ErrData;
            if (parent != null) {
                // copy parent colors
                CopyView.mPalette = parent.Palette.CopyPalette();
            }
            else {
                // copy view colors
                CopyView.mPalette = mPalette.CopyPalette();
            }
            CopyView.mCodePage = Encoding.GetEncoding(mCodePage.CodePage);
            return CopyView;
        }

        /// <summary>
        /// Copies view data from SourceView into this view.
        /// </summary>
        /// <param name="SourceView"></param>
        public void CloneFrom(View SourceView) {
            // only loaded views can be cloned
            WinAGIException.ThrowIfNotLoaded(this);
            WinAGIException.ThrowIfNotLoaded(SourceView);

            // copy base properties
            base.CloneFrom(SourceView);
            // copy view properties
            mViewChanged = SourceView.mViewChanged;
            mViewDesc = SourceView.mViewDesc;
            mLoopCol.CloneFrom(SourceView.mLoopCol);
            ErrLevel = SourceView.ErrLevel;
            ErrData = SourceView.ErrData;
            if (SourceView.parent != null) {
                // copy parent colors
                mPalette = SourceView.parent.Palette.CopyPalette();
            }
            else {
                // copy view colors
                mPalette = SourceView.mPalette.CopyPalette();
            }
            mCodePage = Encoding.GetEncoding(SourceView.mCodePage.CodePage);
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
            mViewChanged = true;
        }

        /// <summary>
        /// Forces view loops to rebuild. Use when the calling
        /// program needs the view to be refreshed.
        /// </summary>
        public void ResetView() {
            ErrLevel = LoadLoops();
        }

        /// <summary>
        /// Converts this view's loop/cel collection into a valid AGI byte array
        /// and stores it in this resource's data.
        /// </summary>
        private void CompileView() {
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
                desc = CodePage.GetBytes(mViewDesc);
                for (i = 0; i < desc.Length; i++) {
                    WriteByte(desc[i]);
                }
                // add terminating null char
                WriteByte(0);
            }
            mViewChanged = false;
            mIsChanged = true;
            // clear error level
            ErrLevel = 0;
            ErrData = ["", "", "", "", ""];
        }

        /// <summary>
        /// This method expands the run length encoding (RLE) data beginning at
        /// position StartPos in the view resource data and passes it to a cel.
        /// </summary>
        /// <param name="StartPos"></param>
        /// <param name="aCel"></param>
        /// <returns>true if data is expanded successfully, otherwise false.</returns>
        bool ExpandCelData(int StartPos, Cel aCel) {
            ArgumentNullException.ThrowIfNull(aCel);
            bool retval = true;
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
                            tmpCelData[bytCelX++, bytCelY] = bytChunkColor;
                        }
                    }
                    // check for missing end
                    if (EORes && bytIn != 0) {
                        retval = false;
                        break;
                    }
                } while (bytIn != 0);
                // check for unexpected end of data
                if (!retval) {
                    // no need to continue
                    break;
                }
                // fill in rest of this line with transparent color, if necessary
                while (bytCelX < bytWidth) {
                    tmpCelData[bytCelX++, bytCelY] = bytTransColor;
                }
                bytCelY++;
            } while (bytCelY < bytHeight);
            aCel.AllCelData = tmpCelData;
            return retval;
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
        /// <returns>Zero if no errors, otherwise an error code:<br />
        /// 1 = no loops<br />
        /// 2 = invalid loop offset<br />
        /// 4 = invalid mirror loop pair<br />
        /// 8 = more than two loops share mirror<br />
        /// 16 = invalid cel offset
        /// 32 = unexpected end of cel data
        /// 64 = invalid description offset
        /// </returns>
        internal int LoadLoops() {
            // 
            byte bytNumLoops, bytNumCels;
            int[] lngLoopStart = new int[MAX_LOOPS];
            ushort lngCelStart, lngDescLoc;
            byte tmpLoopNo, bytLoop, bytCel;
            byte[] bytInput = new byte[1];
            byte bytWidth, bytHeight;
            byte bytTransCol;
            int retval = 0; // assume no errors

            mLoopCol = new Loops(this);
            bytNumLoops = ReadByte(2);
            // get offset to ViewDesc
            lngDescLoc = ReadWord();
            if (bytNumLoops == 0) {
                // error - invalid data
                ErrData[0] = mResID;
                mViewChanged = false;
                return 1;
            }
            // get loop offset data for each loop
            for (bytLoop = 0; bytLoop < bytNumLoops; bytLoop++) {
                lngLoopStart[bytLoop] = ReadWord();
                if ((lngLoopStart[bytLoop] > mSize)) {
                    // invalid loop; let any that are alreay loaded stay loaded
                    ErrData[0] = mResID;
                    ErrData[1] = bytLoop.ToString();
                    mViewChanged = false;
                    retval |= 2;
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
                            // cel header will have mirror info
                            // assume loop is ok until confirmed
                            // confirm valid mirror
                            switch (SetMirror(bytLoop, tmpLoopNo)) {
                            // only valid return values when setting mirrors during
                            // LoadLoops are -3(loop >8) or -6(source already mirrored)
                            case -3:
                                // invalid mirror loop
                                retval |= 4;
                                break;
                            case -6:
                                // source loop already mirrored
                                retval |= 8;
                                break;
                            }
                        }
                    }
                }
                if (mLoopCol[bytLoop].Mirrored == 0) {
                    if (lngLoopStart[bytLoop] < mSize) {
                        Pos = lngLoopStart[bytLoop];
                        bytNumCels = ReadByte();
                        for (bytCel = 0; bytCel < bytNumCels; bytCel++) {
                            // read starting position
                            lngCelStart = (ushort)(ReadWord(lngLoopStart[bytLoop] + 2 * bytCel + 1) + lngLoopStart[bytLoop]);
                            if (lngCelStart < mSize - 3) {
                                bytWidth = ReadByte(lngCelStart);
                                bytHeight = ReadByte();
                                bytTransCol = ReadByte();
                                bytTransCol = (byte)(bytTransCol % 0x10);
                                // add the cel
                                mLoopCol[bytLoop].Cels.Add(bytCel, bytWidth, bytHeight, (AGIColorIndex)bytTransCol);
                                // extract bitmap data from RLE data
                                if (!ExpandCelData(lngCelStart + 3, mLoopCol[bytLoop].Cels[bytCel])) {
                                    retval |= 32;
                                }
                            }
                            else {
                                // keep view data already loaded
                                retval |= 16;
                            }
                        }
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
                            mViewDesc += CodePage.GetString(bytInput);
                        }
                    }
                    while (!EORes && bytInput[0] != 0 && mViewDesc.Length < 255);
                }
                else {
                    // pointer is not valid
                    ErrData[0] = mResID;
                    retval |= 64;
                }
            }
            mViewChanged = false;
            return retval;
        }

        /// <summary>
        /// Exports this resource to a standalone file.
        /// </summary>
        /// <param name="ExportFile"></param>
        public new void Export(string ExportFile) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (mViewChanged) {
                CompileView();
            }
            try {
                base.Export(ExportFile);
            }
            catch {
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
            ErrLevel = LoadLoops();
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
            if (ErrLevel < 0) {
                // return empty view, with one loop, one cel, one pixel
                ErrClear();
                mData = [0x01, 0x01, 0x01, 0x00, 0x00, 0x07, 0x00, 0x01,
                         0x01, 0x03, 0x00, 0x01, 0x01, 0x00, 0x00];
                mViewChanged = false;
                return;
            }
            // extract loops/cels
            ErrLevel = LoadLoops();
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
            mViewChanged = false;
        }

        /// <summary>
        /// Saves properties of this view to the game's WAG file.
        /// </summary>
        public void SaveProps() {
            if (mInGame) {
                parent.WriteGameSetting("View" + Number, "ID", mResID, "Views");
                parent.WriteGameSetting("View" + Number, "Description", mDescription);
                PropsChanged = false;
            }
        }
        
        /// <summary>
        /// Saves this view resource. If in a game, it updates the DIR and VOL files. 
        /// If not in a game the view is saved to its resource file specified by the
        /// FileName property.
        /// </summary>
        public new void Save() {
            WinAGIException.ThrowIfNotLoaded(this);
            if (PropsChanged && mInGame) {
                SaveProps();
            }
            if (mViewChanged) {
                CompileView();
            }
            if (mIsChanged) {
                try {
                    base.Save();
                }
                catch {
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
            mIsChanged = true;
            return 0;
        }
        #endregion
    }
}
