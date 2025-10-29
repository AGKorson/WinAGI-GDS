using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.Linq;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents an AGI View resource, with WinAGI extensions.
    /// </summary>
    [Serializable]
    public class View : AGIResource {
        #region Members
        internal Loops mLoopCol;
        internal string mViewDesc;
        internal EGAColors mPalette;
        internal int mCodePage = 437; // default code page
        /// <summary>
        /// True if the view resource data does not match the current view
        /// loop/cel/description objects.
        /// </summary>
        internal bool mViewChanged;
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
            mCodePage = parent.agCodePage;
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
                    mPalette = value.Clone();
                }
            }
        }

        /// <summary>
        /// Gets or sets the character code page to use when converting 
        /// characters to or from a byte stream.
        /// </summary>
        public int CodePage {
            get => mCodePage;
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
                mData = [0x01, 0x01, 0x01, 0x00, 0x00, 0x07, 0x00, 0x01, 0x03, 0x00, 0x01, 0x01, 0x00, 0x00];
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
            mPalette = defaultPalette.Clone();
            WarnData = ["", "", "", "", "", "", ""];
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
            CopyView.Error = Error;
            CopyView.Warnings = Warnings;
            for (int i = 0; i < ErrData.Length; i++) {
                CopyView.ErrData[i] = ErrData[i];
                CopyView.WarnData[i] = WarnData[i];
            }
            CopyView.mViewChanged = mViewChanged;
            CopyView.ErrData = ErrData;
            if (parent is not null) {
                // copy parent colors
                CopyView.mPalette = parent.Palette.Clone();
            }
            else {
                // copy view colors
                CopyView.mPalette = mPalette.Clone();
            }
            CopyView.mCodePage = mCodePage;
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
            Error = SourceView.Error;
            Warnings = SourceView.Warnings;
            for (int i = 0; i < ErrData.Length; i++) {
                ErrData[i] = SourceView.ErrData[i];
                WarnData[i] = SourceView.WarnData[i];
            }
            if (SourceView.parent is not null) {
                // copy parent colors
                mPalette = SourceView.parent.Palette.Clone();
            }
            else {
                // copy view colors
                mPalette = SourceView.mPalette.Clone();
            }
            mCodePage = SourceView.mCodePage;
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
            (Error, Warnings) = LoadLoops();
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
                if (mLoopCol[i].Mirrored) {
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
                        WriteByte(mLoopCol[i][j].Width);
                        WriteByte(mLoopCol[i][j].Height);
                        if (mLoopCol[i].Mirrored) {
                            // set bit 7 for mirror flag and include loop number
                            // in bits 6-5-4 for transparent color
                            bytTransCol = (byte)(0x80 + i * 0x10 + mLoopCol[i][j].TransColor);
                        }
                        else {
                            // just use transparent color
                            bytTransCol = (byte)mLoopCol[i][j].TransColor;
                        }
                        WriteByte(bytTransCol);
                        // cel data
                        bytCelData = CompressedCel(mLoopCol[i][j], mLoopCol[i].Mirrored);
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
                desc = Encoding.GetEncoding(CodePage).GetBytes(mViewDesc);
                for (i = 0; i < desc.Length; i++) {
                    WriteByte(desc[i]);
                }
                // add terminating null char
                WriteByte(0);
            }
            mViewChanged = false;
            mIsChanged = true;
            // clear errors and warnings
            Error = ResourceErrorType.NoError;
            ErrData = ["", "", "", "", "", ""];
            Warnings = 0;
            WarnData = ["", "", "", "", "", "", ""];
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
                // if number is ok
                if (goodnum) {
                    // use this mirrorpair
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
        /// 1 = invalid loop offset<br />
        /// 2 = invalid mirror loop pair<br />
        /// 4 = more than two loops share mirror<br />
        /// 8 = invalid cel offset
        /// 16 = unexpected end of cel data
        /// 32 = invalid description offset
        /// </returns>
        internal (ResourceErrorType, int) LoadLoops() {
            int[] loopoffset = new int[MAX_LOOPS];
            int[] mirrorpair = new int[MAX_LOOPS];
            Array.Fill(mirrorpair, -1);
            byte width, height;

            // assume no errors or warnings
            ResourceErrorType errval = ResourceErrorType.NoError;
            int warnval = 0;

            if (this.Size < 14) {
                // too small to be a view
                // reset to an empty view
                mData = [0x01, 0x01, 0x01, 0x00, 0x00, 0x07, 0x00,
                         0x01, 0x03, 0x00, 0x01, 0x01, 0x00, 0x00];
                errval = ResourceErrorType.ViewNoData;
            }
            try {
                mLoopCol = new Loops(this);
                byte loopcount = ReadByte(2);
                // get offset to ViewDesc
                ushort descriptionoffset = ReadWord();
                if (loopcount == 0) {
                    // error - invalid data
                    ErrData[0] = mResID;
                    mViewChanged = false;
                    return (ResourceErrorType.ViewNoLoops, 0);
                }
                // get loop offset data for each loop
                for (int loop = 0; loop < loopcount; loop++) {
                    loopoffset[loop] = ReadWord();
                    if (loopoffset[loop] > mSize) {
                        // invalid loop pointer
                        WarnData[1] += "|" + loop;
                        mViewChanged = false;
                        warnval |= 1;
                    }
                }
                // step through each loop to extract cels
                for (int loop = 0; loop < loopcount; loop++) {
                    bool badmirror = false;
                    bool badshare = false;
                    mLoopCol.Add(loop);
                    if (mirrorpair[loop] >= 0) {
                        int check = SetMirror(loop, mirrorpair[loop]);
                        Debug.Assert(check == 0);
                        continue;
                    }
                    // what defines a MIRROR loop?
                    //  x valid loop pointer AND
                    //  x one or more valid cels AND
                    //  x each cel in the loop has the 'mirror' bit set AND
                    //  x loop number is less than eight AND
                    //  x loop shares data with exactly one another loop AND
                    //  x each cel has mirror loopnum that equals one of the loops AND
                    //  x view is NOT from 2.089
                    if (loopoffset[loop] < mSize) {
                        // valid loop pointer
                        Pos = loopoffset[loop];
                        byte celcount = ReadByte();
                        if (celcount > 0) {
                            // one or more cels
                            int mirrorcount = 0;
                            int mirrorLoop = -1;
                            // check for loop with shared data
                            int shared = 0;
                            for (int i = 0; i < loopcount; i++) {
                                if (loop != i && loopoffset[i] == loopoffset[loop]) {
                                    // sharing data
                                    shared++;
                                }
                            }

                            for (int cel = 0; cel < celcount; cel++) {
                                // read starting position
                                int celoffset = ReadWord(loopoffset[loop] + 2 * cel + 1) + loopoffset[loop];
                                if (celoffset < mSize - 3) {
                                    width = ReadByte(celoffset);
                                    height = ReadByte();
                                    byte transcolor = ReadByte();
                                    if (mirrorLoop < 0) {
                                        // currently not a mirror loop
                                        if ((transcolor & 0x80) == 0x80) {
                                            // mirror bit is set
                                            if (loop < 8) {
                                                // only loops 7 or less can be mirrors
                                                // check for a matching loop
                                                int pair = -1;
                                                for (int i = 0; i < 8 && i < loopcount; i++) {
                                                    if (loop != i && loopoffset[i] == loopoffset[loop]) {
                                                        if (pair < 0) {
                                                            pair = i;
                                                        }
                                                        else {
                                                            // invalid- more than one loop
                                                            // is using these loop data
                                                            badmirror = true;
                                                            pair = -2;
                                                            break;
                                                        }
                                                    }
                                                }
                                                if (pair >= 0) {
                                                    mirrorLoop = (transcolor & 0x70) / 0x10;
                                                    if (mirrorLoop == loop) {
                                                        if (mInGame && parent.InterpreterVersion == "2.089") {
                                                            // v2089 does allow mirrors
                                                            badmirror = true;
                                                        }
                                                        else {
                                                            // it is a mirror
                                                            mirrorcount = 1;
                                                            mirrorpair[loop] = pair;
                                                            mirrorpair[pair] = loop;
                                                        }
                                                    }
                                                    else {
                                                        // invalid mirror loop number
                                                        badmirror = true;
                                                    }
                                                }
                                                else if (pair == -1) {
                                                    // no match found
                                                    badmirror = true;
                                                }
                                            }
                                            else {
                                                // not a valid mirror
                                                badmirror = true;
                                            }
                                        }
                                        else {
                                            // should NOT share cel data
                                            if (shared > 0) {
                                                badshare = true;
                                            }
                                        }
                                    }
                                    else {
                                        // confirm mirror info matches
                                        if (transcolor < 0x80) {
                                            // missing mirror bit
                                            badmirror = true;
                                        }
                                        else if ((transcolor & 0x70) / 0x10 != mirrorLoop) {
                                            badmirror = true;
                                        }
                                        mirrorcount++;
                                    }
                                    // transparent color is lower nibble
                                    transcolor = (byte)(transcolor & 0x0f);
                                    // add the cel
                                    mLoopCol[loop].Cels.Add(cel, width, height, (AGIColorIndex)transcolor);
                                    // extract bitmap data from RLE data
                                    if (!ExpandCelData(celoffset + 3, mLoopCol[loop][cel])) {
                                        warnval |= 32;
                                        WarnData[6] += "|" + loop + "|" + cel;
                                    }
                                }
                                else {
                                    // invalid cell pointer
                                    warnval |= 16;
                                    WarnData[5] += "|" + loop + "|" + cel;
                                    // use a single pixel placeholder
                                }
                            }
                            if (mirrorLoop != -1) {
                                // confirm all cels were looped
                                if (celcount != mirrorcount) {
                                    badmirror = true;
                                }
                            }

                        }
                        else {
                            // no cels
                            warnval |= 8;
                            WarnData[4] += "|" + loop;
                            // add a default cel
                            mLoopCol[loop].Cels.Add(0, 1, 1, AGIColorIndex.Black);
                        }
                    }
                    else {
                        // invalid loop- a default cel as a place holder
                        mLoopCol[loop].Cels.Add(0, 1, 1, AGIColorIndex.Black);
                    }
                    // if bad mirror, add warning
                    if (badmirror) {
                        warnval |= 2;
                        WarnData[2] += "|" + loop;
                    }
                    if (badshare) {
                        warnval |= 4;
                        WarnData[3] += "|" + loop;
                    }
                }
                mViewDesc = "";
                if (descriptionoffset > 0) {
                    if (descriptionoffset < mSize - 1) {
                        // set resource pointer to beginning of description string
                        Pos = descriptionoffset;
                        byte nextchar;
                        do {
                            nextchar = ReadByte();
                            // if not zero, and string not yet up to 255 characters,
                            if ((nextchar > 0) && (mViewDesc.Length < 255)) {
                                mViewDesc += Encoding.GetEncoding(CodePage).GetString(new byte[nextchar]);
                            }
                        }
                        while (!EORes && nextchar != 0 && mViewDesc.Length < 255);
                    }
                    else {
                        // pointer is not valid
                        warnval |= 64;
                    }
                }
                if (warnval != 0) {
                    WarnData[0] = mResID;
                }
            }
            catch (IndexOutOfRangeException) {
                // unexpected end of data
                //errval = ResourceErrorType.ViewDataError;
            }
            catch {
                // general error
                //errval = ResourceErrorType.ViewDataError;
            }
            mViewChanged = false;
            return (errval, warnval);
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
            catch {
                // pass along error
                throw;
            }
            (Error, Warnings) = LoadLoops();
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
            if (Error != ResourceErrorType.NoError) {
                // return empty view, with one loop, one cel, one pixel
                ErrClear();
                mData = [0x01, 0x01, 0x01, 0x00, 0x00, 0x07, 0x00,
                         0x01, 0x03, 0x00, 0x01, 0x01, 0x00, 0x00];
                mViewChanged = false;
                return;
            }
            // extract loops/cels
            (Error, Warnings) = LoadLoops();
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
        public int SetMirror(int TargetLoop, int SourceLoop) {
            // source loop must already exist
            if (SourceLoop < 0 || SourceLoop >= mLoopCol.Count) {
                return -1;
            }
            // target loop must already exist
            if (TargetLoop < 0 || TargetLoop >= mLoopCol.Count) {
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
            if (mLoopCol[TargetLoop].Mirrored) {
                return -5;
            }
            // the source loop can't already have a mirror
            if (mLoopCol[SourceLoop].Mirrored) {
                return -6;
            }
            // if ingame, can't be v2.089
            if (mInGame && parent.InterpreterVersion == "2.089") {
                return -7;
            }
            // get a new mirror pair number
            int mp = GetMirrorPair();
            // set the mirror loop hasmirror property
            mLoopCol[SourceLoop].MirrorPair = mp;
            // set the mirror loop mirrorloop property
            mLoopCol[TargetLoop].MirrorPair = -mp;
            mLoopCol[TargetLoop].mCelCol = mLoopCol[SourceLoop].mCelCol;
            mViewChanged = true;
            mIsChanged = true;
            return 0;
        }
        #endregion
    }
}
