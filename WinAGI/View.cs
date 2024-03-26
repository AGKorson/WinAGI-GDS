using System;
using System.ComponentModel;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Commands;
using static WinAGI.Common.Base;
using System.IO;
using System.Diagnostics;

namespace WinAGI.Engine {
    public class View : AGIResource {
        bool mViewSet; //flag to note loops loaded from res data
        internal Loops mLoopCol;
        string mViewDesc;
        int mErrLvl;

        private void InitView(View NewView = null) {
            //attach events
            base.PropertyChanged += ResPropChange;
            if (NewView is null) {
                // add rempty loop col
                mLoopCol = new Loops(this);
                mRData.AllData = [0x01, 0x01, 0x00, 0x00, 0x00];
                // byte0 = unknown (always 1 or 2?)
                // byte1 = unknown (always 1?)
                // byte2 = loop count
                // byte3 = high byte of viewdesc
                // byte4 = low byte of viewdesc
            }
            else {
                // clone the view
                NewView.Clone(this);
            }
        }

        public View() : base(AGIResType.rtView) {
            // new view, not in game

            // initialize
            InitView();
            // create default ID
            mResID = "NewView";
            // if not in a game, resource is always loaded
            mLoaded = true;
        }

        internal View(AGIGame parent, byte ResNum, View NewView = null) : base(AGIResType.rtView) {
            // internal method to add a new view and find a place for it in vol files

            // initialize
            InitView(NewView);
            // set up base resource
            base.InitInGame(parent, ResNum);
        }

        internal View(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.rtView) {
            //this internal function adds this resource to a game, setting its resource 
            //location properties, and reads properties from the wag file

            //attach events
            base.PropertyChanged += ResPropChange;
            //set up base resource
            base.InitInGame(parent, ResNum, VOL, Loc);

            //if importing, there will be nothing in the propertyfile
            mResID = this.parent.agGameProps.GetSetting("View" + ResNum, "ID", "", true);
            if (ID.Length == 0) {
                //no properties to load; save default ID
                ID = "View" + ResNum;
                this.parent.WriteGameSetting("View" + ResNum, "ID", ID, "Views");
            }
            else {
                //get description and other properties from wag file
                mDescription = parent.agGameProps.GetSetting("View" + ResNum, "Description", "");
            }
            // add rempty loop col
            mLoopCol = new Loops(this);
        }
        private void ResPropChange(object sender, AGIResPropChangedEventArgs e) {
            ////let//s do a test
            //// increment number everytime data changes
            //Number++;
        }

        public View Clone() {
            //copies view data from this view and returns a completely separate object reference
            View CopyView = new();
            // copy base properties
            base.Clone(CopyView);
            //add WinAGI items
            CopyView.mViewSet = mViewSet;
            CopyView.mViewDesc = mViewDesc;
            CopyView.mLoopCol = mLoopCol.Clone(this);
            CopyView.mErrLvl = mErrLvl;
            return CopyView;
        }
        public override void Clear() {
            // resets the view to a single loop with a single view with a height and witdh of 1
            // and transparent color of 0 and no description

            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //clear the resource
            base.Clear();
            //clear description
            mViewDesc = "";
            //reset loop col
            mLoopCol = new Loops(this);
            //set default resource data
            mRData.AllData = [0x01, 0x01, 0x00, 0x00, 0x00];
            // byte0 = unknown (always 1 or 2?)
            // byte1 = unknown (always 1?)
            // byte2 = loop count
            // byte3 = high byte of viewdesc
            // byte4 = low byte of viewdesc
            mIsDirty = true;
        }
        void CompileView() {
            // converts loop/cel objects into correct 
            // AGI data stream
            int[] lngLoopLoc, lngCelLoc;
            int i, j;
            byte bytTransCol;
            int k;
            byte[] bytCelData;
            bool blnMirrorAdded;
            // clear the resource data by reinitializing
            mRData.Clear();

            //write header
            WriteByte(1, 0);
            WriteByte(1);
            //write number of loops
            WriteByte((byte)mLoopCol.Count);
            //placeholder for description
            WriteWord(0);
            //initialize loop location array
            lngLoopLoc = new int[mLoopCol.Count];
            //write place holders for loop positions
            for (i = 0; i < mLoopCol.Count; i++) {
                WriteWord(0);
            }
            //step through all loops to add them
            for (i = 0; i < mLoopCol.Count; i++) {
                //if loop is mirrored AND already added
                //(can tell if not added by comparing the mirror loop
                //property against current loop being added)
                if (mLoopCol[i].Mirrored != 0) {
                    blnMirrorAdded = (mLoopCol[i].MirrorLoop < i);
                }
                else {
                    blnMirrorAdded = false;
                }
                if (blnMirrorAdded) {
                    //loop location is same as mirror
                    lngLoopLoc[i] = lngLoopLoc[mLoopCol[i].MirrorLoop];
                }
                else {
                    //set loop location
                    lngLoopLoc[i] = Pos;
                    //write number of cels
                    WriteByte((byte)mLoopCol[i].Cels.Count);
                    //initialize cel loc array
                    lngCelLoc = new int[mLoopCol[i].Cels.Count];
                    //write placeholders for cel locations
                    for (j = 0; j < mLoopCol[i].Cels.Count; j++) {
                        WriteWord(0);
                    }
                    //step through all cels to add them
                    for (j = 0; j < mLoopCol[i].Cels.Count; j++) {
                        //save cel loc
                        lngCelLoc[j] = Pos - lngLoopLoc[i];
                        //write width
                        WriteByte(mLoopCol[i].Cels[j].Width);
                        //write height
                        WriteByte(mLoopCol[i].Cels[j].Height);
                        //if loop is mirrored
                        if (mLoopCol[i].Mirrored != 0) {
                            //set bit 7 for mirror flag and include loop number
                            //in bits 6-5-4 for transparent color
                            bytTransCol = (byte)(0x80 + i * 0x10 + mLoopCol[i].Cels[j].TransColor);
                        }
                        else {
                            //just use transparent color
                            bytTransCol = (byte)mLoopCol[i].Cels[j].TransColor;
                        }
                        //write transcolor
                        WriteByte(bytTransCol);
                        //get compressed cel data
                        bytCelData = CompressedCel(mLoopCol[i].Cels[j], (mLoopCol[i].Mirrored != 0));
                        //write cel data
                        for (k = 0; k < bytCelData.Length; k++) {
                            WriteByte(bytCelData[k]);
                        }
                    }
                    //step through cels and add cel location
                    for (j = 0; j < mLoopCol[i].Cels.Count; j++) {
                        WriteWord((ushort)lngCelLoc[j], lngLoopLoc[i] + 1 + 2 * j);
                    }
                    //restore pos to end of resource
                    Pos = mRData.Length;
                }
            }
            //step through loops again to add loop loc
            for (i = 0; i < mLoopCol.Count; i++) {
                WriteWord((ushort)lngLoopLoc[i], 5 + 2 * i);
            }
            //if there is a view description
            if (mViewDesc.Length > 0) {
                //write view description location
                WriteWord((ushort)mRData.Length, 3);
                //move pointer back to end of resource
                Pos = mRData.Length;
                //write view description
                for (i = 1; i < mViewDesc.Length; i++) {
                    WriteByte((byte)mViewDesc[i]);
                }
                //add terminating null char
                WriteByte(0);
            }
            // clear viewdesc ptr error
            mErrLvl = 0;
            //set viewloaded flag
            mViewSet = true;
        }
        void ExpandCelData(int StartPos, Cel TempCel) {  //this function will expand the RLE data beginning at
                                                         //position StartPos
                                                         //it then passes the expanded data to the cel
            byte bytWidth, bytHeight, bytTransColor;
            byte bytCelX, bytCelY = 0;
            byte bytIn;
            byte bytChunkColor, bytChunkCount;
            byte[,] tmpCelData;
            bytWidth = TempCel.Width;
            bytHeight = TempCel.Height;
            bytTransColor = (byte)TempCel.TransColor;
            //reset size of data array
            tmpCelData = new byte[bytWidth, bytHeight];
            //set resource to starting position
            Pos = StartPos;
            // extract pixel data
            do {
                bytCelX = 0;
                do {
                    //read each byte, where lower four bits are number of pixels,
                    //and upper four bits are color for these pixels
                    bytIn = ReadByte();
                    //skip zero values
                    if (bytIn > 0) {
                        //extract color
                        bytChunkColor = (byte)(bytIn / 0x10);
                        bytChunkCount = (byte)(bytIn % 0x10);
                        //add data to bitmap data array
                        //now store this color for correct number of pixels
                        for (int i = 0; i < bytChunkCount; i++) {
                            tmpCelData[bytCelX, bytCelY] = bytChunkColor;
                            bytCelX++;
                        }
                    }
                } while (bytIn != 0); //Loop Until bytIn = 0
                                      //fill in rest of this line with transparent color, if necessary
                while (bytCelX < bytWidth) { // Until bytCelX >= bytWidth
                    tmpCelData[bytCelX, bytCelY] = bytTransColor;
                    bytCelX++;
                }
                bytCelY++;

            } while (bytCelY < bytHeight); // Until bytCelY >= bytHeight

            //pass cel data to the cel
            TempCel.AllCelData = tmpCelData;
        }

        private int GetMirrorPair() {
            //this function will generate a unique mirrorpair number
            //that is used to identify a pair of mirrored loops
            //the source loop is positive; the copy is negative
            byte i;
            bool goodnum;

            //start with 1
            byte retval = 1;
            do {
                // assume number is ok
                goodnum = true;
                for (i = 0; i < mLoopCol.Count; i++) {
                    //if this loop is using this mirror pair
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
                //try another
                retval++;
            } while (true);
            return retval;
        }

        internal int LoadLoops() {
            // used by load function to extract the view
            // loops and cels from the data stream
            byte bytNumLoops, bytNumCels;
            int[] lngLoopStart = new int[MAX_LOOPS];
            ushort lngCelStart, lngDescLoc;
            byte tmpLoopNo, bytLoop, bytCel;
            byte[] bytInput = new byte[1];
            byte bytWidth, bytHeight;
            byte bytTransCol;
            int result = 0; // assume OK

            //clear out loop collection by assigning a new one
            mLoopCol = new Loops(this);

            //get number of loops and strDescription location
            bytNumLoops = ReadByte(2);
            //get offset to description
            lngDescLoc = ReadWord();
            //if no loops
            if (bytNumLoops == 0) {
                //error - invalid data

                WinAGIException wex = new(LoadResString(595)) {
                    HResult = WINAGI_ERR + 595
                };
                wex.Data["ID"] = mResID;
                throw wex;
            }
            //get loop offset data for each loop
            for (bytLoop = 0; bytLoop < bytNumLoops; bytLoop++) {
                //get offset to start of this loop
                lngLoopStart[bytLoop] = ReadWord();
                //if loop data is past end of resource
                if ((lngLoopStart[bytLoop] > mSize)) {
                    Unload();

                    WinAGIException wex = new(LoadResString(548)) {
                        HResult = WINAGI_ERR + 548
                    };
                    wex.Data["ID"] = mResID;
                    wex.Data["loop"] = bytLoop;
                    throw wex;
                }
            }
            //step through all loops
            // TODO: max number of loops? should there be an error check here?
            for (bytLoop = 0; bytLoop < bytNumLoops; bytLoop++) {
                //add the loop
                mLoopCol.Add(bytLoop);
                //loop zero is NEVER mirrored
                if (bytLoop > 0) {
                    //for all other loops, check to see if it mirrors an earlier loop
                    for (tmpLoopNo = 0; tmpLoopNo < bytLoop; tmpLoopNo++) {
                        //if the loops have the same starting position,
                        if (lngLoopStart[bytLoop] == lngLoopStart[tmpLoopNo]) {
                            //this loop is a mirror
                            try {
                                //get a new mirror pair number
                                SetMirror(bytLoop, tmpLoopNo);
                            }
                            catch (Exception e) {
                                //if error is because source is already mirrored
                                //continue without setting mirror; data will be
                                //treated as a completely separate loop; otherwise
                                // error is unrecoverable
                                if (e.HResult != WINAGI_ERR + 551) {
                                    //pass along the error
                                    Unload();
                                    //error
                                    throw;
                                }
                            }
                            break;
                        }
                    }
                }
                //if loop not mirrored,
                if (mLoopCol[bytLoop].Mirrored == 0) {
                    //point to start of this loop
                    Pos = lngLoopStart[bytLoop];
                    //read number of cels
                    bytNumCels = ReadByte();
                    //step through all cels in this loop
                    for (bytCel = 0; bytCel < bytNumCels; bytCel++) {
                        //read starting position
                        lngCelStart = (ushort)(ReadWord(lngLoopStart[bytLoop] + 2 * bytCel + 1) + lngLoopStart[bytLoop]);
                        if ((lngCelStart > mSize)) {
                            Unload();

                            WinAGIException wex = new(LoadResString(553)) {
                                HResult = WINAGI_ERR + 553
                            };
                            wex.Data["ID"] = mResID;
                            wex.Data["loop"] = bytLoop;
                            wex.Data["cel"] = bytCel;
                            throw wex;
                        }
                        //get height/width
                        bytWidth = ReadByte(lngCelStart);
                        bytHeight = ReadByte();
                        //get transparency color for this cel
                        bytTransCol = ReadByte();
                        bytTransCol = (byte)(bytTransCol % 0x10);
                        //add the cel
                        mLoopCol[bytLoop].Cels.Add(bytCel, bytWidth, bytHeight, (AGIColorIndex)bytTransCol);
                        //extract bitmap data from RLE data
                        ExpandCelData(lngCelStart + 3, mLoopCol[bytLoop].Cels[bytCel]);
                    }
                }
            }
            //clear the description string
            mViewDesc = "";
            //if there is a description for this view,
            if (lngDescLoc > 0) {
                //ensure it can be loaded
                if (lngDescLoc < mSize - 1) {
                    //set resource pointer to beginning of description string
                    Pos = lngDescLoc;
                    do {
                        //get character
                        bytInput[0] = ReadByte();
                        //if not zero, and string not yet up to 255 characters,
                        if ((bytInput[0] > 0) && (mViewDesc.Length < 255)) {
                            //add the character
                            mViewDesc += parent.agCodePage.GetString(bytInput);
                        }
                        //stop if zero reached, end of resource reached, or 255 characters read
                    }
                    while (!EORes && bytInput[0] != 0 && mViewDesc.Length < 255);
                }
                else {
                    // pointer is not valid; fix it (reset to zero)
                    WriteWord(0, 3);
                    // return error level
                    result = 1;
                }
            }
            //set flag indicating view matches resource data
            mViewSet = true;
            //MUST be clean, since loaded from resource data
            mIsDirty = false;
            return result;
        }

        public void Export(string ExportFile, bool ResetDirty = true) {
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            try {
                //if view is dirty
                if (mIsDirty) {
                    //need to recompile
                    CompileView();
                }
                Export(ExportFile);
            }
            catch (Exception) { throw; }
            //if not in a game,
            if (!mInGame) {
                //ID always tracks the resfile name
                mResID = Path.GetFileName(ExportFile);
                if (mResID.Length > 64) {
                    mResID = Left(mResID, 64);
                }
                if (ResetDirty) {
                    //clear dirty flag
                    mIsDirty = false;
                }
            }
        }
        
        public override void Import(string ImportFile) {  //imports a view resource
            try {
                //import the resource
                Import(ImportFile);
            }
            catch (Exception) {
                //pass along error
                Unload();
                throw;
            }
            //if not in a game,
            if (!mInGame) {
                //set ID
                mResID = Path.GetFileName(ImportFile);
                if (mResID.Length > 64) {
                    mResID = Left(mResID, 64);
                }
            }
            //reset dirty flag
            mIsDirty = false;
            WritePropState = false;
            //loops need rebuilding
            mViewSet = false;
        }
        
        public override void Load() {
            //if not ingame, the resource should already be loaded
            if (!mInGame) {
                Debug.Assert(mLoaded);
            }
            // ignore if already loaded
            if (mLoaded) {
                return;
            }
            mIsDirty = false;
            WritePropState = false;
            // load base resource data
            base.Load();
            if (mErrLevel < 0) {
                // return empty view, with no loops
                Clear();
                return;
            }
            // extract loops/cels
                mErrLvl = LoadLoops();
            
            catch (Exception) {
                Unload();
                //pass along error
                throw;
            }
            //clear dirty flags
            mIsDirty = false;
            WritePropState = false;
        }
        
        public override void Unload() {
            //unload resource
            base.Unload();
            mIsDirty = false;
            //clear out loop collection
            mLoopCol = new Loops(this);
            mErrLvl = 0;
            mViewSet = false;
        }
        public new void Save() {
            //saves an ingame view
            //if properties need to be written
            if (WritePropState && mInGame) {
                //save ID and description to ID file
                parent.WriteGameSetting("View" + Number, "ID", mResID, "Views");
                parent.WriteGameSetting("View" + Number, "Description", mDescription);
                WritePropState = false;
            }
            //if not loaded
            if (!mLoaded) {
                //nothing to do
                return;
            }
            //if dirty,
            if (mIsDirty) {
                try {
                    //rebuild Resource
                    CompileView();
                    base.Save();
                }
                catch
               (Exception) {
                    //pass error along
                    throw;
                }
                //reset flag
                mIsDirty = false;
            }
        }
        
        public Loop this[int index] {
            get {
                try {
                    return Loops[index];
                }
                catch (Exception) {
                    // pass along any error
                    throw;
                }
            }
        }
        
        public Loops Loops {
            get {
                //if not loaded
                if (!mLoaded) {
                    //error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                //if view not set,
                if (!mViewSet) {
                    //error
                    WinAGIException wex = new(LoadResString(563) + ": loops not load????") {
                        HResult = WINAGI_ERR + 563,
                    };
                    throw wex;
                }
                //return the loop collection
                return mLoopCol;
            }
        }
        
        public string ViewDescription {
            get {
                //if not loaded
                if (!mLoaded) {
                    try {
                        Load();
                        string tmpVal = mViewDesc;
                        Unload();
                        return tmpVal;
                    }
                    catch (Exception) {
                        return "";
                    }
                }
                return mViewDesc;
            }
            set {
                //if changing,
                if (mViewDesc != value) {
                    mViewDesc = Left(value, 255);
                    mIsDirty = true;
                }
            }
        }
        
        public int ErrLevel {
            //provides access to current error level of the view

            //can be used by calling programs to provide feedback
            //on errors in the view data

            //return 0 if successful, no errors/warnings
            // non-zero for error/warning:
            //     1 = invalid viewdesc pointer
            get {
                return mErrLvl;
            }
        }

        public void SetMirror(byte TargetLoop, byte SourceLoop) {
            //TargetLoop is the loop that will be a mirror of
            //SourceLoop; the cels collection in TargetLoop will be lost
            //once the mirror property is set
            int i;
            //if not loaded
            if (!mLoaded) {
                //error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //the source loop must already exist
            //(must be less than or equal to max number of loops)
            if (SourceLoop >= mLoopCol.Count) {
                //error - loop must exist
                WinAGIException wex = new(LoadResString(539)) {
                    HResult = WINAGI_ERR + 539
                };
                wex.Data["ID"] = mResID;
                wex.Data["srcloop"] = SourceLoop;
                throw wex;
            }
            // TODO: shouldn't targetloop also be checked for being valid?

            //the source loop and the target loop must be less than 8
            if (SourceLoop >= 8 || TargetLoop >= 8) {
                //error - invalid mirror loop
                WinAGIException wex = new(LoadResString(539)) {
                    HResult = WINAGI_ERR + 539
                };
                wex.Data["ID"] = mResID;
                wex.Data["srcloop"] = SourceLoop;
                wex.Data["tgtloop"] = TargetLoop;
                throw wex;
            }
            //mirror source and target can't be the same
            if (SourceLoop == TargetLoop) {
                //ignore - can't be a mirror of itself
                return;
            }
            //the target loop can't be already mirrored
            if (mLoopCol[TargetLoop].Mirrored != 0) {
                //error
                WinAGIException wex = new(LoadResString(550)) {
                    HResult = WINAGI_ERR + 550
                };
                wex.Data["ID"] = mResID;
                wex.Data["tgtloop"] = TargetLoop;
                throw wex;
            }
            //the source loop can't already have a mirror
            if (mLoopCol[SourceLoop].Mirrored != 0) {
                //error
                WinAGIException wex = new(LoadResString(551)) {
                    HResult = WINAGI_ERR + 551
                };
                wex.Data["ID"] = mResID;
                wex.Data["srcloop"] = SourceLoop;
                throw wex;
            }
            //get a new mirror pair number
            i = GetMirrorPair();
            //set the mirror loop hasmirror property
            mLoopCol[SourceLoop].MirrorPair = i;
            //set the mirror loop mirrorloop property
            mLoopCol[TargetLoop].MirrorPair = -i;
            mLoopCol[TargetLoop].Cels = mLoopCol[SourceLoop].Cels;
            //set dirty flag
            mIsDirty = true;
        }
    }
}
