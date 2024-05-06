using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using WinAGI.Common;
using static WinAGI.Engine.PictureFunctions;

namespace WinAGI.Engine {

    public struct PicBkgdPos {
        public float srcX;
        public float srcY;
        public float tgtX;
        public float tgtY;
        public override readonly string ToString() {
            return srcX.ToString() + "|" + srcY.ToString() + "|" + tgtX.ToString() + "|" + tgtY.ToString();
        }
        public void FromString(string value) {
            string[] strings = value.Split('|');
            if (strings.Length == 4) {
                if (!float.TryParse(strings[0], out srcX)) {
                    srcX = 0;
                }
                if (!float.TryParse(strings[0], out srcY)) {
                    srcY = 0;
                }
                if (!float.TryParse(strings[0], out tgtX)) {
                    tgtX = 0;
                }
                if (!float.TryParse(strings[0], out tgtY)) {
                    tgtY = 0;
                }
            }
            else {
                srcX = 0;
                srcY = 0;
                tgtX = 0;
                tgtY = 0;
            }
        }
    }
    public struct PicBkgdSize {
        public float srcW;
        public float srcH;
        public float tgtW;
        public float tgtH;
        public override readonly string ToString() {
            return srcW.ToString() + "|" + srcH.ToString() + "|" + tgtW.ToString() + "|" + tgtH.ToString();
        }
        public void FromString(string value) {
            string[] strings = value.Split('|');
            if (strings.Length == 4) {
                if (!float.TryParse(strings[0], out srcW)) {
                    srcW = 0;
                }
                if (!float.TryParse(strings[0], out srcH)) {
                    srcH = 0;
                }
                if (!float.TryParse(strings[0], out tgtW)) {
                    tgtW = 0;
                }
                if (!float.TryParse(strings[0], out tgtH)) {
                    tgtH = 0;
                }
            }
            else {
                srcW = 0;
                srcH = 0;
                tgtW = 0;
                tgtH = 0;
            }
        }
    }

    /// <summary>
    /// A class that represents an AGI Picture resource, with WinAGI extensions.
    /// </summary>
    public class Picture : AGIResource {
        string mBkImgFile;
        bool mBkShow;
        int mBkTrans;
        PicBkgdPos mBkPos;
        PicBkgdSize mBkSize;
        byte mPriBase;
        bool mPicBMPSet;
        int mDrawPos;
        bool mStepDraw;
        PenStatus mCurrentPen;
        byte[] mVisData;
        byte[] mPriData;
        Bitmap bmpVis;
        Bitmap bmpPri;

        /// <summary>
        /// Initializes a new AGI picture resource that is not in a game.
        /// </summary>
        public Picture() : base(AGIResType.rtPicture) {
            // not in a game so resource is always loaded
            mLoaded = true;
            InitPicture();
            // use a default ID
            mResID = "NewPicture";
        }

        /// <summary>
        /// Internal constructor to initialize a new or cloned picture resource being added to an AGI game.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="NewPicture"></param>
        internal Picture(AGIGame parent, byte ResNum, Picture NewPicture = null) : base(AGIResType.rtPicture) {
            // initialize
            InitPicture(NewPicture);
            // set up base resource
            base.InitInGame(parent, ResNum);
        }

        /// <summary>
        /// Internal constructor to add a new picture resource during initial game load.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="VOL"></param>
        /// <param name="Loc"></param>
        internal Picture(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.rtPicture) {
            // adds a picture from dir/vol files, setting its resource 
            // location properties, and reads properties from the wag file

            // set up base resource
            base.InitInGame(parent, AGIResType.rtPicture, ResNum, VOL, Loc);
            // default to entire image
            mDrawPos = -1;
            // default pribase is 48
            mPriBase = 48;
        }

        /// <summary>
        /// Initializes a new picture resource when first instantiated. If NewPicture is null, 
        /// a blank picture resource is created. If NewPicture is not null, it is cloned into
        /// the new picture.
        /// </summary>
        /// <param name="NewPicture"></param>
        private void InitPicture(Picture NewPicture = null) {
            if (NewPicture is null) {
                // create default picture with no commands
                mData = new byte[1];
                mData[0] = 0xff;
                mDrawPos = -1;
                mPriBase = 48;
                mBkImgFile = "";
                mBkShow = false;
                mBkTrans = 0;
                mBkPos = new();
                mBkSize = new();
                mPicBMPSet = false;
                mStepDraw = false;
                mCurrentPen = new PenStatus();
            }
            else {
                // copy base properties
                NewPicture.CloneTo(this);
                // copy picture properties
                mBkImgFile = NewPicture.mBkImgFile;
                mBkShow = NewPicture.mBkShow;
                mBkTrans = NewPicture.mBkTrans;
                mBkPos = NewPicture.mBkPos;
                mBkSize = NewPicture.mBkSize;
                mPriBase = NewPicture.mPriBase;
                mPicBMPSet = NewPicture.mPicBMPSet;
                mDrawPos = NewPicture.mDrawPos;
                mStepDraw = NewPicture.mStepDraw;
                mCurrentPen = NewPicture.mCurrentPen;
                mVisData = NewPicture.mVisData;
                mPriData = NewPicture.mPriData;
            }
        }

        /// <summary>
        /// Creates an exact copy of this Picture resource.
        /// </summary>
        /// <returns>The Picture resource this method creates.</returns>
        internal Picture Clone() {
            Picture CopyPicture = new();
            // copy base properties
            base.CloneTo(CopyPicture);
            // copy picture properties
            CopyPicture.mBkImgFile = mBkImgFile;
            CopyPicture.mBkShow = mBkShow;
            CopyPicture.mBkTrans = mBkTrans;
            CopyPicture.mBkPos = mBkPos;
            CopyPicture.mBkSize = mBkSize;
            CopyPicture.mPriBase = mPriBase;
            CopyPicture.mPicBMPSet = mPicBMPSet;
            CopyPicture.mDrawPos = mDrawPos;
            CopyPicture.mStepDraw = mStepDraw;
            CopyPicture.mCurrentPen = mCurrentPen;
            CopyPicture.mVisData = mVisData;
            CopyPicture.mPriData = mPriData;
            CopyPicture.ErrLevel = ErrLevel;
            return CopyPicture;
        }

        /// <summary>
        /// 
        /// </summary>
        public string BkgdImgFile {
            get {
                return mBkImgFile;
            }
            set {
                mBkImgFile = value;
                PropsDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public PicBkgdPos BkgdPosition {
            get {
                return mBkPos;
            }
            set {
                mBkPos = value;
                PropsDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public PicBkgdSize BkgdSize {
            get {
                return mBkSize;
            }
            set {
                mBkSize = value;
                PropsDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int BkgdTrans {
            get {
                return mBkTrans;
            }
            set {
                mBkTrans = value;
                PropsDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool BkgdShow {
            get {
                return mBkShow;
            }
            set {
                mBkShow = value;
                PropsDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte PriBase {
            get {
                // if before v2.936, always return value of 48
                if (parent is not null && int.Parse(parent.agIntVersion) < 2.936) {
                    mPriBase = 48;
                }
                return mPriBase;
            }
            set {
                // if before v2.936, it's always 48
                if (parent is not null && int.Parse(parent.agIntVersion) < 2.936) {
                    mPriBase = 48;
                }
                //max value is 158
                else if (value > 158) {
                    mPriBase = 158;
                }
                else {
                    mPriBase = value;
                }
                PropsDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte[] VisData {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (!mPicBMPSet) {
                    BuildPictures();
                }
                return mVisData;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte[] PriData {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (!mPicBMPSet) {
                    BuildPictures();
                }
                return mPriData;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public PenStatus CurrentToolStatus {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mCurrentPen;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int DrawPos {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mDrawPos;
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
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
                mPicBMPSet = false;
            }
        }

        /// <summary>
        /// Returns true if testcel at position x,y is entirely on water.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool ObjOnWater(byte X, byte Y, byte Length) {
            byte i;
            AGIColorIndex CurPri;
            byte EndX;

            WinAGIException.ThrowIfNotLoaded(this);
            // validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
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
            // if not exited, must be on water
            return true;
        }

        /// <summary>
        /// returns the actual lowest priority code for a line,
            //including control codes
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public AGIColorIndex ControlPixel(byte X, byte Y, byte Length = 1) {
            byte i = 0;
            AGIColorIndex CurPri, retval;

            WinAGIException.ThrowIfNotLoaded(this);
            // validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            // ensure enough room for length
            if (X + Length > 159) {
                Length = (byte)(160 - X);
            }
            if (!mPicBMPSet) {
                BuildPictures();
            }
            // default to max (15 == white)
            retval = AGIColorIndex.agWhite;
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

        /// <summary>
        /// Returns pixel visual color.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public AGIColorIndex VisPixel(byte X, byte Y) {
            WinAGIException.ThrowIfNotLoaded(this);
            // validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            if (!mPicBMPSet) {
                BuildPictures();
            }
            return (AGIColorIndex)mVisData[X + 160 * Y];
        }

        /// <summary>
        /// Returns pixel priority color.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public AGIColorIndex PriPixel(byte X, byte Y) {
            WinAGIException.ThrowIfNotLoaded(this);
            // validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            if (!mPicBMPSet) {
                BuildPictures();
            }
            return (AGIColorIndex)mPriData[X + 160 * Y];
        }

        /// <summary>
        /// Return priority of pixel, exclusive of control lines.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public AGIColorIndex PixelPri(byte X, byte Y) {
            WinAGIException.ThrowIfNotLoaded(this);
            // validate x,y
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            if (!mPicBMPSet) {
                BuildPictures();
            }
            AGIColorIndex retval;
            // set default to 15
            AGIColorIndex defval = AGIColorIndex.agWhite;
            // find first pixel that is NOT a control color (0-2)
            do {
                retval = (AGIColorIndex)mPriData[X + 160 * Y];
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

        /// <summary>
        /// 
        /// </summary>
        public override void Clear() {
            WinAGIException.ThrowIfNotLoaded(this);
            base.Clear();
            mData = [0xff];
            //mRData.AllData = [0xff];
            mDrawPos = -1;
            mStepDraw = false;
            mBkImgFile = "";
            mBkPos = new();
            mBkSize = new();
            mBkTrans = 0;
            mBkShow = false;
            mPriBase = 48;
            mCurrentPen = new();
            BuildPictures();
        }

        /// <summary>
        /// Loads this picture resource by reading its data from the VOL file. Only
        /// applies to pictures in a game. Non-game pictures are always loaded.
        /// </summary>
        public override void Load() {
            if (mLoaded) {
                return;
            }
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
                mBkPos.FromString(parent.agGameProps.GetSetting("Picture" + Number, "BkgdPosn", ""));
                mBkSize.FromString(parent.agGameProps.GetSetting("Picture" + Number, "BkgdSize", ""));
            }
            BuildPictures();
        }

        /// <summary>
        /// Imports a picture resource from a file into this picture.
        /// </summary>
        /// <param name="ImportFile"></param>
        public override void Import(string ImportFile) {
            try {
                base.Import(ImportFile);
            }
            catch {
                // pass along any errors
                throw;
            }
            BuildPictures();
            mBkImgFile = "";
            mBkShow = false;
            mBkTrans = 0;
            mBkPos.FromString("");
            mBkSize.FromString("");
        }

        /// <summary>
        /// Forces bitmap to reload. Use when palette changes
        /// (or any other reason that needs the cel to be refreshed).
        /// </summary>
        public void ResetBMP() {
            mPicBMPSet = false;
        }

        /// <summary>
        /// Returns a bitmap image of the visual screen.
        /// </summary>
        public Bitmap VisualBMP {
            get {
                if (!mLoaded) {
                    return null;
                }
                if (!mPicBMPSet) {
                    BuildPictures();
                }
                return bmpVis;
            }
        }

        /// <summary>
        /// Returns a bitmap image of the priority screen.
        /// </summary>
        public Bitmap PriorityBMP {
            get {
                if (!mLoaded) {
                    return null;
                }
                if (!mPicBMPSet) {
                    BuildPictures();
                }
                return bmpPri;
            }
        }

        /// <summary>
        /// Saves properties of this picture to the game's WAG file.
        /// </summary>
        public void SaveProps() {
            if (mInGame) {
                string strPicKey = "Picture" + Number;
                parent.WriteGameSetting(strPicKey, "ID", mResID, "Pictures");
                parent.WriteGameSetting(strPicKey, "Description", mDescription);
                if (mPriBase != 48) {
                    parent.WriteGameSetting(strPicKey, "PriBase", mPriBase.ToString());
                }
                else {
                    parent.agGameProps.DeleteKey(strPicKey, "PriBase");
                }
                // if no bkgdfile, delete other settings
                if (mBkImgFile.Length == 0) {
                    mBkShow = false;
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdImg");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdShow");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdTrans");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdPosn");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdSize");
                }
                else {
                    parent.WriteGameSetting(strPicKey, "BkgdImg", mBkImgFile);
                    parent.WriteGameSetting(strPicKey, "BkgdShow", mBkShow.ToString());
                    parent.WriteGameSetting(strPicKey, "BkgdTrans", mBkTrans.ToString());
                    parent.WriteGameSetting(strPicKey, "BkgdPosn", mBkPos.ToString());
                    parent.WriteGameSetting(strPicKey, "BkgdSize", mBkSize.ToString());
                }
                PropsDirty = false;
            }
        }

        /// <summary>
        /// Saves this picture resource. If in a game, it updates the DIR and VOL files. 
        /// If not in a game the picture is saved to its resource file specified by FileName.
        /// </summary>
        public new void Save() {
            WinAGIException.ThrowIfNotLoaded(this);
            if (PropsDirty && mInGame) {
                SaveProps();
            }
            if (mIsDirty) {
                // (no picture-specific action needed, since changes in picture are made
                // directly to resource data); use the base save method
                // any bmp errors will remain until they are fixed by the
                // user, so don't reset error flag
                try {
                    base.Save();
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PicData"></param>
        public void SetPictureData(byte[] PicData) {
            // sets the picture resource data to PicData()

            WinAGIException.ThrowIfNotLoaded(this);
            mData = PicData;
            //mRData.AllData = PicData;
            if (mLoaded) {
                // rebuild pictures
                BuildPictures();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal void BuildPictures() {
            // create new visual picture bitmap
            bmpVis = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
            // create new priority picture bitmap
            bmpPri = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
            // modify color palette to match current AGI palette
            ColorPalette ncp = bmpVis.Palette;
            if (parent is not null) {
                for (int i = 0; i < 16; i++) {
                    ncp.Entries[i] = Color.FromArgb(255,
                    parent.AGIColors[i].R,
                    parent.AGIColors[i].G,
                    parent.AGIColors[i].B);
                }
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
            mErrLevel = BuildBMPs(ref mVisData, ref mPriData, mData, mStepDraw ? mDrawPos : -1, mDrawPos);
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

        /// <summary>
        /// 
        /// </summary>
        public bool StepDraw {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mStepDraw;
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mStepDraw != value) {
                    mStepDraw = value;
                    mPicBMPSet = false;
                }
            }
        }

        /// <summary>
        /// Unloads this picture resource. Data elements are undefined and non-accessible
        /// while unloaded. Only pictures that are in a game can be unloaded.
        /// </summary>
        public override void Unload() {
            // only ingame resources can be unloaded
            if (!mInGame) {
                return;
            }
            base.Unload();
            // cleanup picture resources
            bmpVis = null;
            bmpPri = null;
            mPicBMPSet = false;
        }
    }
}
