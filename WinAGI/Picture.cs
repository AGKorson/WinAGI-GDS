using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using WinAGI.Common;
using static WinAGI.Engine.Base;
using static WinAGI.Common.Base;
using static WinAGI.Engine.PictureFunctions;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents an AGI Picture resource, with WinAGI extensions.
    /// </summary>
    public class Picture : AGIResource {
        #region Structs
        // none
        #endregion

        #region Members
        byte mPriBase;
        internal bool mPicBMPSet;
        int mDrawPos;
        bool mStepDraw;
        PenStatus mCurrentPen;
        bool mPenSet;
        byte[] mVisData;
        byte[] mPriData;
        Bitmap bmpVis;
        Bitmap bmpPri;
        private EGAColors mPalette;
        private PictureBackgroundSettings mBkgdSettings;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new AGI picture resource that is not in a game.
        /// </summary>
        public Picture() : base(AGIResType.Picture) {
            // not in a game so resource is always loaded
            mLoaded = true;
            InitPicture();
            // use a default ID
            mResID = "NewPicture";
        }

        /// <summary>
        /// Internal constructor to initialize a new or cloned picture resource being added
        /// to an AGI game.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="NewPicture"></param>
        internal Picture(AGIGame parent, byte ResNum, Picture NewPicture = null) : base(AGIResType.Picture) {
            InitPicture(NewPicture);
            base.InitInGame(parent, ResNum);
        }

        /// <summary>
        /// Internal constructor to add a new picture resource during initial game load.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="VOL"></param>
        /// <param name="Loc"></param>
        internal Picture(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.Picture) {
            InitPicture(null);
            base.InitInGame(parent, AGIResType.Picture, ResNum, VOL, Loc);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets a bitmap image of the visual screen.
        /// </summary>
        public Bitmap VisualBMP {
            get {
                if (!mLoaded) {
                    return null;
                }
                if (!mPicBMPSet) {
                    BuildBMPs();
                }
                return bmpVis;
            }
        }

        /// <summary>
        /// Gets a bitmap image of the priority screen.
        /// </summary>
        public Bitmap PriorityBMP {
            get {
                if (!mLoaded) {
                    return null;
                }
                if (!mPicBMPSet) {
                    BuildBMPs();
                }
                return bmpPri;
            }
        }

        /// <summary>
        /// Gets the byte array that represents the visual screen in a bitmap. 
        /// </summary>
        public byte[] VisData {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (!mPicBMPSet) {
                    BuildBMPs();
                }
                return mVisData;
            }
        }

        /// <summary>
        /// Gets the byte array that respresents the priority screen in a bitmap.
        /// </summary>
        public byte[] PriData {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (!mPicBMPSet) {
                    BuildBMPs();
                }
                return mPriData;
            }
        }

        /// <summary>
        /// Gets or sets the background settings used in picture editors.
        /// </summary>
        public PictureBackgroundSettings BackgroundSettings {
            get => mBkgdSettings;
            set {
                bool oldvis = mBkgdSettings.Visible;
                mBkgdSettings = value;
                PropsChanged = true;
                if (!InGame && (mBkgdSettings.Visible != oldvis || mBkgdSettings.Visible)) {
                    mPicBMPSet = false;
                }
            }
        }

        public string BkgdFileName {
            get {
                return mBkgdSettings.FileName;
            }
            set {
                mBkgdSettings.FileName = value;
                PropsChanged = true;
            }
        }
        public bool BkgdVisible {
            get {
                return mBkgdSettings.Visible;
            }
            set {
                mBkgdSettings.Visible = value;
                PropsChanged = true;
                if (!InGame) {
                    mPicBMPSet = false;
                }
            }
        }
        public bool BkgdShowVis {
            get {
                return mBkgdSettings.ShowVis;
            }
            set {
                mBkgdSettings.ShowVis = value;
                PropsChanged = true;
                if (!InGame && mBkgdSettings.Visible) {
                    mPicBMPSet = false;
                }
            }
        }
        public bool BkgdShowPri {
            get {
                return mBkgdSettings.ShowPri;
            }
            set {
                mBkgdSettings.ShowPri = value;
                PropsChanged = true;
                if (!InGame && mBkgdSettings.Visible) {
                    mPicBMPSet = false;
                }
            }
        }
        public byte BkgdTransparency {
            get {
                return mBkgdSettings.Transparency;
            }
            set {
                mBkgdSettings.Transparency = value;
                PropsChanged = true;
                if (!InGame && mBkgdSettings.Visible) {
                    mPicBMPSet = false;
                }
            }
        }
        public bool BkgdDefaultTransparency {
            get {
                return mBkgdSettings.DefaultAlwaysTransparent;
            }
            set {
                mBkgdSettings.DefaultAlwaysTransparent = value;
                PropsChanged = true;
                if (!InGame && mBkgdSettings.Visible) {
                    mPicBMPSet = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the priority base to use when testing the picture in the
        /// WinAGI Editor. Only applies if the picture is part of a game with a 
        /// version of 2.936 or higher.
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
                PropsChanged = true;
            }
        }

        /// <summary>
        /// Gets the pen color and status, and brush style/size/shape base on the current
        /// drawing position in the picture.
        /// </summary>
        public PenStatus CurrentPenStatus {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (!mPenSet) {
                    mCurrentPen = GetPenStatus(mDrawPos);
                    mPenSet = true;
                }
                return mCurrentPen;
            }
        }

        /// <summary>
        /// Gets or sets the stopping position to use when drawing the picture. Used
        /// by the WinAGI Editor to allow insertion of new drawing commands anywhere
        /// within the picture.
        /// </summary>
        public int DrawPos {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mInGame) {
                    return -1;
                }
                else {
                    return mDrawPos;
                }
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mInGame) {
                    return;
                }
                if (value == mDrawPos) {
                    return;
                }
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
                mPenSet = false;
            }
        }

        /// <summary>
        /// Gets or sets a value to indicate if the entire picture should be drawn,
        /// or if it should only process commands up to the current DrawPos location.
        /// </summary>
        public bool StepDraw {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mInGame) {
                    return false;
                }
                else {
                    return mStepDraw;
                }
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mInGame) {
                    return;
                }
                if (mStepDraw != value) {
                    mStepDraw = value;
                    mPicBMPSet = false;
                }
            }
        }

        /// <summary>
        /// Gets the current state of picture bmps. Used to determine if calling
        /// program needs to redraw bitmaps.
        /// </summary>
        public bool BMPSet {
            get => mPicBMPSet;
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
        #endregion

        #region Methods
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
                mBkgdSettings = new();
                mPicBMPSet = false;
                mStepDraw = false;
                mCurrentPen = new PenStatus();
            }
            else {
                // copy base properties
                NewPicture.CloneTo(this);
                // copy picture properties
                mDrawPos = NewPicture.mDrawPos;
                mPriBase = NewPicture.mPriBase;
                mBkgdSettings = NewPicture.mBkgdSettings;
                mStepDraw = NewPicture.mStepDraw;
                mCurrentPen = NewPicture.mCurrentPen;
                mVisData = NewPicture.mVisData;
                mPriData = NewPicture.mPriData;
                // bitmaps always need to be rebuilt
                mPicBMPSet = false;
            }
            mPalette = defaultPalette.CopyPalette();
        }

        /// <summary>
        /// Creates an exact copy of this Picture resource.
        /// </summary>
        /// <returns>The Picture resource this method creates.</returns>
        public Picture Clone() {
            // only loaded pictures can be cloned
            WinAGIException.ThrowIfNotLoaded(this);

            Picture CopyPicture = new();
            // copy base properties
            base.CloneTo(CopyPicture);
            // copy picture properties
            CopyPicture.mBkgdSettings = mBkgdSettings;
            CopyPicture.mPriBase = mPriBase;
            CopyPicture.mDrawPos = mDrawPos;
            CopyPicture.mStepDraw = mStepDraw;
            CopyPicture.mCurrentPen = mCurrentPen;
            CopyPicture.mVisData = mVisData;
            CopyPicture.mPriData = mPriData;
            if (parent != null) {
                // copy parent colors
                CopyPicture.mPalette = parent.Palette.CopyPalette();
            }
            else {
                // copy picture colors
                CopyPicture.mPalette = mPalette.CopyPalette();
            }
            CopyPicture.ErrLevel = ErrLevel;
            // bitmaps always need to be rebuilt
            CopyPicture.mPicBMPSet = false;
            return CopyPicture;
        }

        /// <summary>
        /// Copies properties from SourcePicture into this picture.
        /// </summary>
        /// <param name="SourcePicture"></param>
        public void CloneFrom(Picture SourcePicture) {
            // only loaded pictures can be cloned
            WinAGIException.ThrowIfNotLoaded(this);
            WinAGIException.ThrowIfNotLoaded(SourcePicture);

            // copy base properties
            base.CloneFrom(SourcePicture);
            // copy picture properties
            mBkgdSettings = SourcePicture.mBkgdSettings;
            mPriBase = SourcePicture.mPriBase;
            if (!mInGame) {
                mDrawPos = SourcePicture.mDrawPos;
                mStepDraw = SourcePicture.mStepDraw;
                mCurrentPen = SourcePicture.mCurrentPen;
            }
            mVisData = SourcePicture.mVisData;
            mPriData = SourcePicture.mPriData;
            if (SourcePicture.parent != null) {
                // copy parent colors
                mPalette = SourcePicture.parent.Palette.CopyPalette();
            }
            else {
                // copy picture colors
                mPalette = SourcePicture.mPalette.CopyPalette();
            }
            ErrLevel = SourcePicture.ErrLevel;
            // bitmaps always need to be rebuilt
            mPicBMPSet = false;
        }

        /// <summary>
        /// Forces the bitmaps to rebuild. Useful when the picture data are changed
        /// since the resource doesn't raise a DataChanged event.
        /// </summary>
        public void ForceRefresh() {
            mPicBMPSet = false;
        }

        /// <summary>
        /// Returns true if the specified paramters represent a cel baseline that is
        /// entirely on water.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool ObjOnWater(Point location, byte Length) {
            AGIColorIndex CurPri;
            byte EndX;

            WinAGIException.ThrowIfNotLoaded(this);
            if (location.X < 0 || location.X > 159) {
                throw new ArgumentOutOfRangeException(nameof(location.X));
            }
            if (location.Y < 0 || location.Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(location.Y));
            }
            if (!mPicBMPSet) {
                BuildBMPs();
            }
            EndX = (byte)(location.X + Length - 1);
            if (EndX > 159) {
                EndX = 159;
            }
            for (int i = location.X; i <= EndX; i++) {
                CurPri = (AGIColorIndex)mPriData[i + 160 * location.Y];
                if (CurPri != AGIColorIndex.Cyan) {
                    return false;
                }
            }
            // if not exited, must be on water
            return true;
        }

        /// <summary>
        /// Returns the actual lowest priority code for a line of pixels,
        /// including control codes.
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
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            if (X + Length > 159) {
                Length = (byte)(160 - X);
            }
            if (!mPicBMPSet) {
                BuildBMPs();
            }
            retval = AGIColorIndex.White;
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
        /// Returns the visual color of the pixel at the specified location.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public AGIColorIndex VisPixel(byte X, byte Y) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            if (!mPicBMPSet) {
                BuildBMPs();
            }
            return (AGIColorIndex)mVisData[X + 160 * Y];
        }

        /// <summary>
        /// Returns the priority color of the pixel at the specified  location.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public AGIColorIndex PriPixel(byte X, byte Y) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            if (!mPicBMPSet) {
                BuildBMPs();
            }
            return (AGIColorIndex)mPriData[X + 160 * Y];
        }

        /// <summary>
        /// Return priority color of the specified pixel, excluding control lines by
        /// searching below the pixel for first non-control value.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public AGIColorIndex PixelPri(byte X, byte Y) {
            AGIColorIndex retval;

            WinAGIException.ThrowIfNotLoaded(this);
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            if (!mPicBMPSet) {
                BuildBMPs();
            }
            // find first pixel that is NOT a control color (0-2)
            do {
                retval = (AGIColorIndex)mPriData[X + (160 * Y++)];
            }
            while (retval < AGIColorIndex.Cyan && Y < 168);
            if (retval < AGIColorIndex.Cyan) {
                // control pixels all the way down - return default
                return AGIColorIndex.White;
            }
            else {
                return retval;
            }
        }

        /// <summary>
        /// Clears the resource data, resetting pictures to a blank white screen with all
        /// pixels at priority four.
        /// </summary>
        public override void Clear() {
            WinAGIException.ThrowIfNotLoaded(this);
            base.Clear();
            mData = [0xff];
            mDrawPos = -1;
            mStepDraw = false;
            mBkgdSettings = new();
            mPriBase = 48;
            mCurrentPen = new();
            BuildBMPs();
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
            if (ErrLevel < 0) {
                // return a blank picture resource without adjusting error level
                ErrClear();
                return;
            }
            // load bkgd info, if there is such
            string picturekey = "Picture" + Number;
            mBkgdSettings.FileName = parent.agGameProps.GetSetting(picturekey, "BkgdImgFile", "");
            if (mBkgdSettings.FileName.Length != 0) {
                mBkgdSettings.Visible = parent.agGameProps.GetSetting(picturekey, "BkgdVisible", false);
                mBkgdSettings.ShowVis = parent.agGameProps.GetSetting(picturekey, "BkgdShowVis", false);
                mBkgdSettings.ShowPri = parent.agGameProps.GetSetting(picturekey, "BkgdShowPri", false);
                mBkgdSettings.DefaultAlwaysTransparent = parent.agGameProps.GetSetting(picturekey, "BkgdDefaultTrans", true);
                mBkgdSettings.Transparency = (byte)parent.agGameProps.GetSetting(picturekey, "BkgdTrans", 50);
                mBkgdSettings.SourceRegion.X = parent.agGameProps.GetSetting(picturekey, "BkgdSrcX", 0f);
                mBkgdSettings.SourceRegion.Y = parent.agGameProps.GetSetting(picturekey, "BkgdSrcY", 0f);
                mBkgdSettings.SourceRegion.Width = parent.agGameProps.GetSetting(picturekey, "BkgdSrcW", 0f);
                mBkgdSettings.SourceRegion.Height = parent.agGameProps.GetSetting(picturekey, "BkgdSrcH", 0f);
                mBkgdSettings.SourceSize.Width = parent.agGameProps.GetSetting(picturekey, "BkgdSizeW", 0);
                mBkgdSettings.SourceSize.Height = parent.agGameProps.GetSetting(picturekey, "BkgdSizeH", 0);
                mBkgdSettings.TargetPos.X = parent.agGameProps.GetSetting(picturekey, "BkgdTgtX", 0);
                mBkgdSettings.TargetPos.Y = parent.agGameProps.GetSetting(picturekey, "BkgdTgtY", 0);
            }
            BuildBMPs();
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
            BuildBMPs();
            mBkgdSettings = new();
        }

        /// <summary>
        /// Forces bitmap to reload. Use when palette changes (or any other reason
        /// that the calling program needs the cel to be refreshed).
        /// </summary>
        public void ResetPicture() {
            mPicBMPSet = false;
        }

        /// <summary>
        /// Saves properties of this picture to the game's WAG file.
        /// </summary>
        public void SaveProps() {
            if (mInGame) {
                string strPicKey = "Picture" + mResNum;
                parent.WriteGameSetting(strPicKey, "ID", mResID, "Pictures");
                parent.WriteGameSetting(strPicKey, "Description", mDescription);
                if (mPriBase != 48) {
                    parent.WriteGameSetting(strPicKey, "PriBase", mPriBase.ToString());
                }
                else {
                    parent.agGameProps.DeleteKey(strPicKey, "PriBase");
                }
                // if no bkgdfile, delete other settings
                if (mBkgdSettings.FileName.Length == 0) {
                    mBkgdSettings = new();
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdImgFile");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdVisible");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdShowVis");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdShowPri");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdTrans");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdDefaultTrans");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdSrcX");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdSrcY");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdSrcW");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdSrcH");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdSizeW");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdSizeH");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdTgtX");
                    parent.agGameProps.DeleteKey(strPicKey, "BkgdTgtY");

                }
                else {
                    parent.WriteGameSetting(strPicKey, "BkgdImgFile", mBkgdSettings.FileName);
                    parent.WriteGameSetting(strPicKey, "BkgdVisible", mBkgdSettings.Visible);
                    parent.WriteGameSetting(strPicKey, "BkgdShowVis", mBkgdSettings.ShowVis);
                    parent.WriteGameSetting(strPicKey, "BkgdShowPri", mBkgdSettings.ShowPri);
                    parent.WriteGameSetting(strPicKey, "BkgdTrans", mBkgdSettings.Transparency);
                    parent.WriteGameSetting(strPicKey, "BkgdDefaultTrans", mBkgdSettings.DefaultAlwaysTransparent);
                    parent.WriteGameSetting(strPicKey, "BkgdSrcX", mBkgdSettings.SourceRegion.X);
                    parent.WriteGameSetting(strPicKey, "BkgdSrcY", mBkgdSettings.SourceRegion.Y);
                    parent.WriteGameSetting(strPicKey, "BkgdSrcW", mBkgdSettings.SourceRegion.Width);
                    parent.WriteGameSetting(strPicKey, "BkgdSrcH", mBkgdSettings.SourceRegion.Height);
                    parent.WriteGameSetting(strPicKey, "BkgdTgtX", mBkgdSettings.TargetPos.X);
                    parent.WriteGameSetting(strPicKey, "BkgdTgtY", mBkgdSettings.TargetPos.Y);
                    parent.WriteGameSetting(strPicKey, "BkgdSizeW", mBkgdSettings.SourceSize.Width);
                    parent.WriteGameSetting(strPicKey, "BkgdSizeH", mBkgdSettings.SourceSize.Height);
                }
                PropsChanged = false;
            }
        }

        /// <summary>
        /// Saves this picture resource. If in a game, it updates the DIR and VOL files. 
        /// If not in a game the picture is saved to its resource file specified by the
        /// FileName property.
        /// </summary>
        public new void Save() {
            WinAGIException.ThrowIfNotLoaded(this);
            if (PropsChanged && mInGame) {
                SaveProps();
            }
            if (mIsChanged) {
                // No picture-specific action needed, since changes in picture are made
                // directly to resource data. Use the base save method. Any bmp errors
                // will remain until they are fixed by the user, so don't reset error
                // flag.
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
        /// Builds bitmaps for the visual and priority screens.
        /// </summary>
        internal void BuildBMPs() {
            bmpVis = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
            bmpPri = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
            // update color palette
            // setup bitmap data variables
            var BoundsRect = new Rectangle(0, 0, 160, 168);
            BitmapData bmpVisData = bmpVis.LockBits(BoundsRect, ImageLockMode.WriteOnly, bmpVis.PixelFormat);
            IntPtr ptrVis = bmpVisData.Scan0;
            BitmapData bmpPriData = bmpPri.LockBits(BoundsRect, ImageLockMode.WriteOnly, bmpPri.PixelFormat);
            IntPtr ptrPri = bmpPriData.Scan0;
            mVisData = new byte[26880];
            mPriData = new byte[26880];
            // build arrays of bitmap data, set error level
            ErrLevel = CompilePicData(ref mVisData, ref mPriData, mData, mStepDraw ? mDrawPos : -1, mDrawPos);
            // build palette, adding transparency info if not in a game
            ColorPalette ncp = bmpVis.Palette;
            for (int i = 0; i < 16; i++) {
                int a = 255;
                if (!InGame && mBkgdSettings.Visible && mBkgdSettings.ShowVis) {
                    if (mBkgdSettings.DefaultAlwaysTransparent && i == 15) {
                        a = 0;
                    }
                    else {
                        a = (byte)(255 * (100d - mBkgdSettings.Transparency) / 100d);
                    }
                }
                if (parent != null) {
                    ncp.Entries[i] = Color.FromArgb(a,
                    parent.Palette[i].R,
                    parent.Palette[i].G,
                    parent.Palette[i].B);
                }
                else {
                    ncp.Entries[i] = Color.FromArgb(a,
                    mPalette[i].R,
                    mPalette[i].G,
                    mPalette[i].B);
                }
            }
            bmpVis.Palette = ncp;
            // adjust the palette for priority screen if necessary
            if (!InGame && mBkgdSettings.Visible && (mBkgdSettings.ShowVis || mBkgdSettings.ShowPri)) {
                for (int i = 0; i < 16; i++) {
                    int a = 255;
                    if (!InGame && mBkgdSettings.Visible && mBkgdSettings.ShowPri) {
                        if (mBkgdSettings.DefaultAlwaysTransparent && i == 4) {
                            a = 0;
                        }
                        else {
                            a = (byte)(255 * (100d - mBkgdSettings.Transparency) / 100d);
                        }
                    }
                    if (parent != null) {
                        ncp.Entries[i] = Color.FromArgb(a,
                        parent.Palette[i].R,
                        parent.Palette[i].G,
                        parent.Palette[i].B);
                    }
                    else {
                        ncp.Entries[i] = Color.FromArgb(a,
                        mPalette[i].R,
                        mPalette[i].G,
                        mPalette[i].B);
                    }
                }
            }
            bmpPri.Palette = ncp;
            // copy the picture data to the bitmaps
            Marshal.Copy(mVisData, 0, ptrVis, 26880);
            bmpVis.UnlockBits(bmpVisData);
            Marshal.Copy(mPriData, 0, ptrPri, 26880);
            bmpPri.UnlockBits(bmpPriData);
            // update pen status
            mCurrentPen = SavePen;
            mPenSet = true;
            mPicBMPSet = true;
        }

        public PenStatus GetPenStatus(int statuspos) {
            int lngPos;
            short bytIn;
            PenStatus CurrentPen = new();
            byte[] picdata = mData;
            if (statuspos < 0 || statuspos > mData.Length - 1) {
                // use end pos
                statuspos = mData.Length - 1;
            }
            lngPos = 0;
            bytIn = mData[lngPos++];
            try {
                while (lngPos <= statuspos) 
                {
                    switch (bytIn) {
                    case 0xF4:
                    case 0xF5:
                    case 0xF6:
                    case 0xF7:
                    case 0xF8:
                    case 0xFA:
                        // skip to next command
                         while (mData[lngPos] < 0xF0) {
                            lngPos++;
                        }
                        bytIn = mData[lngPos++];
                        break;
                    case 0xF9:
                        // Change pen size and style
                        bytIn = mData[lngPos++];
                        CurrentPen.PlotStyle = (PlotStyle)((bytIn & 0x20) / 0x20);
                        CurrentPen.PlotShape = (PlotShape)((bytIn & 0x10) / 0x10);
                        CurrentPen.PlotSize = bytIn & 0x7;
                        bytIn = mData[lngPos++];
                        break;
                    case 0xF0:
                        // Change picture color and enable picture draw
                        bytIn = mData[lngPos++];
                        CurrentPen.VisColor = (AGIColorIndex)(bytIn & 0xF);
                        // AGI has a slight bug - if color is > 15, the
                        // upper nibble will overwrite the priority color
                        if (bytIn > 15) {
                            // pass upper nibble to priority
                            CurrentPen.PriColor |= (AGIColorIndex)(bytIn / 16);
                        }
                        bytIn = mData[lngPos++];
                        break;
                    case 0xF1:
                        // Disable visual draw
                        CurrentPen.VisColor = AGIColorIndex.None;
                        bytIn = mData[lngPos++];
                        break;
                    case 0xF2:
                        // Change priority color and enable priority draw
                        bytIn = mData[lngPos++];
                        // AGI uses ONLY priority color; if the passed value is
                        // greater than 15, the upper nibble gets ignored
                        CurrentPen.PriColor = ((AGIColorIndex)(bytIn & 0xF));
                        bytIn = mData[lngPos++];
                        break;
                    case 0xF3:
                        // Disable priority draw
                        CurrentPen.PriColor = AGIColorIndex.None;
                        bytIn = mData[lngPos];
                        lngPos++;
                        break;
                    case 0xFF:
                        // end of drawing
                        break;
                    default:
                        // if expecting a command, and byte is <240 but >250 (not 255)
                        // just ignore it
                        bytIn = mData[lngPos++];
                        break;
                    }
                    if (bytIn == 0xFF) {
                        break;
                    }
                }
            }
            catch (Exception ex) {
                // ignore errors
            }
            return CurrentPen;
        }

        /// <summary>
        /// Unloads this picture resource. Data elements are undefined and non-accessible
        /// while unloaded. Only pictures that are in a game can be unloaded.
        /// </summary>
        public override void Unload() {
            if (!mInGame) {
                return;
            }
            base.Unload();
            // cleanup picture resources
            bmpVis = null;
            bmpPri = null;
            mPicBMPSet = false;
        }
        #endregion
    }
}
