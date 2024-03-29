using System;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Commands;
using static WinAGI.Common.Base;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Configuration;

namespace WinAGI.Engine {
    public class Picture : AGIResource {
        string mBkImgFile;
        bool mBkShow;
        int mBkTrans;
        string mBkPos;
        string mBkSize;
        byte mPriBase;
        bool mPicBMPSet;
        int mDrawPos;
        bool mStepDraw;
        PenStatus mCurrentPen;
        byte[] mVisData;
        byte[] mPriData;
        Bitmap bmpVis;
        Bitmap bmpPri;

        private void InitPicture(Picture NewPicture = null) {
            // attach events
            PropertyChanged += ResPropChange;
            if (NewPicture is null) {
                // create default picture with no commands
                WriteByte(0xff);
                // default to entire image
                mDrawPos = -1;
                // default pribase is 48
                mPriBase = 48;
            }
            else {
                // clone this picture
                NewPicture.Clone(this);
            }
        }

        public Picture() : base(AGIResType.rtPicture) {
            // new picture, not in game

            // initialize
            InitPicture();
            // create a default ID
            mResID = "NewPicture";
            // if not in a game, resource is always loaded
            mLoaded = true;
        }

        internal Picture(AGIGame parent, byte ResNum, Picture NewPicture = null) : base(AGIResType.rtPicture) {
            // internal method to a new picture and find place for it in vol files

            // initialize
            InitPicture(NewPicture);
            // set up base resource
            base.InitInGame(parent, ResNum);
        }

        public Picture(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.rtPicture) {
            //this internal function adds this resource to a game, setting its resource 
            //location properties, and reads properties from the wag file

            //attach events
            base.PropertyChanged += ResPropChange;
            //set up base resource
            base.InitInGame(parent, ResNum, VOL, Loc);

            //if importing, there will be nothing in the propertyfile
            mResID = parent.agGameProps.GetSetting("Picture" + ResNum, "ID", "", true);
            if (mResID.Length == 0) {
                //no properties to load; save default ID
                mResID = "Picture" + ResNum;
                parent.WriteGameSetting("Picture" + ResNum, "ID", ID, "Pictures");
            }
            else {
                //get description and other properties from wag file
                mDescription = parent.agGameProps.GetSetting("Picture" + ResNum, "Description", "");
            }
            //default to entire image
            mDrawPos = -1;
            //default pribase is 48
            mPriBase = 48;
        }
        void ResPropChange(object sender, AGIResPropChangedEventArgs e) {
            //set flag to indicate picture data does not match picture bmps
            mPicBMPSet = false;
            //for picture only, changing resource sets dirty flag
            mIsDirty = true;
        }
        internal Picture Clone() {
            //copies picture data from this picture and returns a completely separate object reference
            Picture CopyPicture = new();
            // copy base properties
            base.Clone(CopyPicture);
            //add WinAGI items
            CopyPicture.mBkImgFile = mBkImgFile;
            CopyPicture.mBkShow = mBkShow;
            CopyPicture.mBkTrans = mBkTrans;
            CopyPicture.mBkPos = mBkPos;
            CopyPicture.mBkSize = mBkSize;
            CopyPicture.mPriBase = mPriBase;
            CopyPicture.mPicBMPSet = mPicBMPSet;
            CopyPicture.DrawPos = DrawPos;
            CopyPicture.mStepDraw = mStepDraw;
            CopyPicture.mCurrentPen = mCurrentPen;
            CopyPicture.mVisData = mVisData;
            CopyPicture.mPriData = mPriData;
            CopyPicture.ErrLevel = ErrLevel;
            return CopyPicture;
        }
        public string BkgdImgFile {
            get {
                return mBkImgFile;
            }
            set {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                mBkImgFile = value;
                WritePropState = true;
            }
        }
        public string BkgdPosition {
            get {
                return mBkPos;
            }
            set {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                mBkPos = value;
                WritePropState = true;
            }
        }
        public string BkgdSize {
            get {
                return mBkSize;
            }
            set {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                mBkSize = value;
                WritePropState = true;
            }
        }
        public int BkgdTrans {
            get {
                return mBkTrans;
            }
            set {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                mBkTrans = value;
                WritePropState = true;
            }
        }
        public bool BkgdShow {
            get {
                return mBkShow;
            }
            set {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                mBkShow = value;
                WritePropState = true;
            }
        }

        public byte PriBase {
            get {
                //if before v2.936, always return value of 48
                if (int.Parse(parent.agIntVersion) < 2.936) {
                    mPriBase = 48;
                }
                return mPriBase;
            }
            set {
                //if not loaded,
                if (!mLoaded) {
                    //raise error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                //max value is 158
                if (value > 158) {
                    mPriBase = 158;
                }
                else {
                    mPriBase = value;
                }
                WritePropState = true;
            }
        }

        public byte[] VisData {
            get {
                //if not loaded
                if (!Loaded) {
                    //error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                //if picture data changed,
                if (!mPicBMPSet) {
                    //load pictures
                    BuildPictures();
                }
                return mVisData;
            }
        }

        public byte[] PriData {
            get {
                //if not loaded
                if (!Loaded) {
                    //error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                //if picture data changed,
                if (!mPicBMPSet) {
                    //load pictures
                    BuildPictures();
                }
                return mPriData;
            }
        }

        public PenStatus CurrentToolStatus {
            get {
                //if not loaded
                if (!Loaded) {
                    //error

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                return mCurrentPen;
            }
        }

        public int DrawPos {
            get {
                //if not loaded
                if (!Loaded) {
                    //error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                return mDrawPos;
            }
            set {
                //if not loaded,
                if (!Loaded) {
                    //raise error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                //if not changed
                if (value == mDrawPos) {
                    return;
                }
                //validate input
                if (value < 0) {
                    mDrawPos = -1;
                }
                else if (value >= Size) {
                    mDrawPos = Size - 1;
                }
                else {
                    mDrawPos = value;
                }
                //set flag to indicate picture data does not match picture bmps
                mPicBMPSet = false;
            }
        }

        public bool ObjOnWater(byte X, byte Y, byte Length) {
            //returns true if testcel at position x,y is entirely on water
            byte i;
            AGIColorIndex CurPri;
            byte EndX;

            // if not loaded
            if (!Loaded) {
                // error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            // if picture data changed,
            if (!mPicBMPSet) {
                BuildPictures();
            }
            // ensure enough room for length
            if (X + Length > 159) {
                Length = (byte)(160 - X);
            }
            // step through all pixels on this line
            EndX = (byte)(X + Length - 1);
            for (i = X; i <= EndX; i++) {
                CurPri = (AGIColorIndex)mPriData[i + 160 * Y];
                if (CurPri != AGIColorIndex.agCyan) {
                    // not on water- return false
                    return false;
                }
            }
            //if not exited, must be on water
            return true;
        }

        public AGIColorIndex PixelControl(byte X, byte Y, byte Length = 1) {
            //returns the actual lowest priority code for a line,
            //including control codes
            byte i = 0;
            AGIColorIndex CurPri, retval;
            //if not loaded,
            if (!mLoaded) {
                //raise error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            // if picture data changed,
            if (!mPicBMPSet) {
                BuildPictures();
            }
            // default to max (15 == white)
            retval = AGIColorIndex.agWhite;
            // ensure enough room for length
            if (X + Length > 159) {
                Length = (byte)(160 - X);
            }
            // get lowest pixel priority on the line
            do {
                CurPri = (AGIColorIndex)mPriData[X + i + 160 * Y];
                if (CurPri < retval) {
                    retval = CurPri;
                }
                i++;
            }
            while (i < Length);
            return retval;
        }

        public AGIColorIndex VisPixel(byte X, byte Y) {
            //returns visual pixel color
            //if not loaded,
            if (!Loaded) {
                //raise error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            // if picture data changed,
            if (!mPicBMPSet) {
                BuildPictures();
            }
            return (AGIColorIndex)mVisData[X + 160 * Y];
        }

        public AGIColorIndex PriPixel(byte X, byte Y) {
            //if not loaded,
            if (!Loaded) {
                //raise error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            // if picture data changed,
            if (!mPicBMPSet) {
                BuildPictures();
            }
            return (AGIColorIndex)mPriData[X + 160 * Y];
        }

        public AGIColorIndex PixelPriority(byte X, byte Y) {
            //if not loaded,
            if (!Loaded) {
                //raise error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            //if picture data changed,
            if (!mPicBMPSet) {
                BuildPictures();
            }
            AGIColorIndex retval;
            //set default to 15
            AGIColorIndex defval = AGIColorIndex.agWhite;
            // need to loop to find first pixel that is NOT a control color (0-2)
            do {
                //get pixel priority
                retval = (AGIColorIndex)mPriData[X + 160 * Y];
                //move down to next row
                Y++;
            }
            while (retval < AGIColorIndex.agCyan && Y < 168);
            // if not valid
            if (retval < AGIColorIndex.agCyan) {
                return defval;
            }
            else {
                return retval;
            }
        }

        public void Export(string ExportFile, bool ResetDirty = true) {
            //if not loaded
            if (!Loaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // use base function
            base.Export(ExportFile);
            //if not in a game,
            if (!InGame) {
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

        public override void Import(string ImportFile) {
            // imports a picture resource
            // TODO: importing also has to load the resource and set error level

            try {
                // use base function
                base.Import(ImportFile);
            }
            catch (Exception) {
                // reset to empty picture
                Clear();
                // pass along any errors
                throw;
            }
            finally {
                // set ID to the filename without extension;
                // the calling function will take care or reassigning it later, if needed
                // (for example, if the new logic will be added to a game)
                mResID = Path.GetFileName(ImportFile);
                if (mResID.Length > 64) {
                    mResID = Left(mResID, 64);
                }
                mIsDirty = false;
            }
        }

        public override void Clear() {
            if (InGame) {
                if (!Loaded) {
                    //nothing to clear
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
            }
            base.Clear();
            mRData.AllData = [0xff];
            // reset position pointer
            mDrawPos = 1;
            // load pictures
            BuildPictures();
        }

        public override void Load() {
            // if not ingame, the resource is already loaded
            if (!mInGame) {
                Debug.Assert(mLoaded);
            }
            // load data into the bitmap data area for this set of bitmaps
            if (mLoaded) {
                return;
            }
            // load base resource
            base.Load();
            if (mErrLevel < 0) {
                // return a blank picture resource without adjusting error level
                ErrClear();
                // ignore background image stats
                return;
            }
            // load bkgd info, if there is such
            mBkImgFile = parent.agGameProps.GetSetting("Picture" + Number, "BkgdImg", "");
            if (mBkImgFile.Length != 0) {
                mBkShow = parent.agGameProps.GetSetting("Picture" + Number, "BkgdShow", false);
                mBkTrans = parent.agGameProps.GetSetting("Picture" + Number, "BkgdTrans", 0);
                mBkPos = parent.agGameProps.GetSetting("Picture" + Number, "BkgdPosn", "");
                mBkSize = parent.agGameProps.GetSetting("Picture" + Number, "BkgdSize", "");
            }
            BuildPictures();
        }

        /// <summary>
        /// Forces bitmap to reload. Use when palette changes
        /// (or any other reason that needs the cel to be refreshed)
        /// </summary>
        public void ResetBMP() {
            mPicBMPSet = false;
        }

        public Bitmap VisualBMP {
            get {
                //returns a device context to the bitmap image of the visual screenoutput
                //if not loaded,
                if (!mLoaded) {
                    return null;
                    ////raise error
                    //WinAGIException wex = new(LoadResString(563)) {
                    //    HResult = WINAGI_ERR + 563
                    //};
                    //throw wex;
                }
                //if pictures not built, or have changed,
                if (!mPicBMPSet) {
                    //load pictures to get correct pictures
                    BuildPictures();
                }
                return bmpVis;
            }
        }

        public Bitmap PriorityBMP {
            get {
                //if not loaded,
                if (!mLoaded) {
                    return null;
                    ////raise error
                    //WinAGIException wex = new(LoadResString(563)) {
                    //    HResult = WINAGI_ERR + 563
                    //};
                    //throw wex;
                }
                //if pictures not built, or have changed,
                if (!mPicBMPSet) {
                    //load pictures to get correct pictures
                    BuildPictures();
                }
                return bmpPri;
            }
        }

        public new void Save() {
            if (!mLoaded) {
                // do nothing? error?
                return;
            }

            // if properties need to be written
            if (WritePropState && mInGame) {
                //saves the picture resource
                //save ID and description to ID file
                string strPicKey = "Picture" + Number;
                parent.WriteGameSetting(strPicKey, "ID", mResID, "Pictures");
                parent.WriteGameSetting(strPicKey, "Description", mDescription);
                if (mPriBase != 48) {
                    parent.WriteGameSetting(strPicKey, "PriBase", mPriBase.ToString());
                }
                else {
                    parent.agGameProps.DeleteKey(strPicKey, "PriBase");
                }
                //if no bkgdfile, delete other settings
                if (mBkImgFile.Length == 0) {
                    mBkShow = false;
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdImg");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdShow");
                    //mBkTrans = 0
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdTrans");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdPosn");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdSize");
                }
                else {
                    parent.WriteGameSetting(strPicKey, "BkgdImg", mBkImgFile);
                    parent.WriteGameSetting(strPicKey, "BkgdShow", mBkShow.ToString());
                    parent.WriteGameSetting(strPicKey, "BkgdTrans", mBkTrans.ToString());
                    parent.WriteGameSetting(strPicKey, "BkgdPosn", mBkPos);
                    parent.WriteGameSetting(strPicKey, "BkgdSize", mBkSize);
                }
                WritePropState = false;
            }
            if (mIsDirty) {
                // (no picture-specific action needed, since changes in picture are
                // made directly to resource data)
                // use the base save method
                // any bmp errors will remain until they are fixed by the
                // user, so don't reset error flag
                try {
                    base.Save();
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
                // check for errors

                // reset flag
                mIsDirty = false;
            }
        }
        
        public void SetPictureData(byte[] PicData) {
            // sets the picture resource data to PicData()
            //
            // if the data are invalid, the resource will
            // be identified as corrupted
            // (clear the picture, replace it with valid data
            // or unload it without saving to recover from
            // invalid input data)
            // copy the picture data
            if (!mLoaded) {
                //raise error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            try {
                mRData.AllData = PicData;
                if (mLoaded) {
                    //rebuild pictures
                    BuildPictures();
                }
            }
            catch (Exception) {
                //ignore?
            }
        }

        internal void BuildPictures() {
            // create new visual picture bitmap
            bmpVis = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
            // create new priority picture bitmap
            bmpPri = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
            // modify color palette to match current AGI palette
            ColorPalette ncp = bmpVis.Palette;
            for (int i = 0; i < 16; i++) {
                ncp.Entries[i] = Color.FromArgb(255,
                parent.AGIColors[i].R,
                parent.AGIColors[i].G,
                parent.AGIColors[i].B);
            }
            // both bitmaps use same palette
            bmpVis.Palette = ncp;
            bmpPri.Palette = ncp;
            // set boundary rectangles
            var BoundsRect = new Rectangle(0, 0, 160, 168);
            // create access points for bitmap data
            BitmapData bmpVisData = bmpVis.LockBits(BoundsRect, ImageLockMode.WriteOnly, bmpVis.PixelFormat);
            IntPtr ptrVis = bmpVisData.Scan0;
            BitmapData bmpPriData = bmpPri.LockBits(BoundsRect, ImageLockMode.WriteOnly, bmpPri.PixelFormat);
            IntPtr ptrPri = bmpPriData.Scan0;
            // now we can create our custom data arrays
            // array size is determined by stride (bytes per row) and height
            mVisData = new byte[26880];
            mPriData = new byte[26880];
            // build arrays of bitmap data
            // Build function returns an error level value
            mErrLevel = BuildBMPs(ref mVisData, ref mPriData, mRData.AllData, mStepDraw ? mDrawPos : -1, mDrawPos);
            if (mErrLevel != 0) {
                ErrData[0] = mResID;
            }
            // copy the picture data to the bitmaps
            Marshal.Copy(mVisData, 0, ptrVis, 26880);
            bmpVis.UnlockBits(bmpVisData);
            Marshal.Copy(mPriData, 0, ptrPri, 26880);
            bmpPri.UnlockBits(bmpPriData);
            // get pen status
            mCurrentPen = GetToolStatus();
            // set flag
            mPicBMPSet = true;
        }

        public bool StepDraw {
            get {
                if (!mLoaded) {
                    // TODO: return false? or exception?

                    //raise error
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                return mStepDraw;
            }
            set {
                if (!mLoaded) {

                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
                // if a change
                if (mStepDraw != value) {
                    mStepDraw = value;
                    //set flag to force redraw
                    mPicBMPSet = false;
                }
            }
        }

        public override void Unload() {
            //unload resource
            base.Unload();
            //cleanup picture resources
            bmpVis = null;
            bmpPri = null;
            mPicBMPSet = false;
        }
    }
}
