﻿using System;
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
        /// <summary>
        /// Encapsulates the position information for a background picture that
        /// can be overlaid on an AGI Picture resource while it is being edited.
        /// </summary>
        public struct PicBkgdPos () {
            public float srcX;
            public float srcY;
            public float tgtX;
            public float tgtY;

            /// <summary>
            /// Returns a string representation of the background position data
            /// in a single string format that can be stored in a settings file.
            /// </summary>
            /// <returns></returns>
            public override readonly string ToString() {
                return srcX.ToString() + "|" + srcY.ToString() + "|" + tgtX.ToString() + "|" + tgtY.ToString();
            }

            /// <summary>
            /// Converts a string representation of background position data
            /// information into its individual values.
            /// </summary>
            /// <param name="value"></param>
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

        /// <summary>
        /// Encapsulates the size information for a background picture that can
        /// be overlaid on an AGI Picture resource while it is being edited.
        /// </summary>
        public struct PicBkgdSize {
            public float srcW;
            public float srcH;
            public float tgtW;
            public float tgtH;

            /// <summary>
            /// Returns a string representation of background size data in a
            /// single string format that can be stored in a settings file.
            /// </summary>
            /// <returns></returns>
            public override readonly string ToString() {
                return srcW.ToString() + "|" + srcH.ToString() + "|" + tgtW.ToString() + "|" + tgtH.ToString();
            }

            /// <summary>
            /// Converts a string representation of background size data information
            /// into its individual values.
            /// </summary>
            /// <param name="value"></param>
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
        #endregion

        #region Members
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
        private EGAColors mPalette;
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
        /// Gets or sets the filename of a background image to use when editing the
        /// picture in the WinAGI Editor.
        /// </summary>
        public string BkgdImgFile {
            get {
                return mBkImgFile;
            }
            set {
                mBkImgFile = value;
                PropsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the position information for a background image to use when
        /// editing the picture in the WinAGI Editor.
        /// </summary>
        public PicBkgdPos BkgdPosition {
            get {
                return mBkPos;
            }
            set {
                mBkPos = value;
                PropsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the background size information for a background image to
        /// use when editing the picture in the WinAGI Editor.
        /// </summary>
        public PicBkgdSize BkgdSize {
            get {
                return mBkSize;
            }
            set {
                mBkSize = value;
                PropsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets transparency setting for a background image when editing
        /// the picture in the WinAGI Editor.
        /// </summary>
        public int BkgdTrans {
            get {
                return mBkTrans;
            }
            set {
                mBkTrans = value;
                PropsChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets a value to indicate if the background image should be visible when
        /// editing the picture in the WinAGI Editor.
        /// </summary>
        public bool BkgdShow {
            get {
                return mBkShow;
            }
            set {
                mBkShow = value;
                PropsChanged = true;
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
        public PenStatus CurrentToolStatus {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
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
                return mDrawPos;
            }
            set {
                WinAGIException.ThrowIfNotLoaded(this);
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
            }
        }

        /// <summary>
        /// Gets or sets a value to indicate if the entire picture should be drawn,
        /// or if it should only process commands up to the current DrawPos location.
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
                mDrawPos = NewPicture.mDrawPos;
                mPriBase = NewPicture.mPriBase;
                mBkImgFile = NewPicture.mBkImgFile;
                mBkShow = NewPicture.mBkShow;
                mBkTrans = NewPicture.mBkTrans;
                mBkPos = NewPicture.mBkPos;
                mBkSize = NewPicture.mBkSize;
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
            CopyPicture.mBkImgFile = mBkImgFile;
            CopyPicture.mBkShow = mBkShow;
            CopyPicture.mBkTrans = mBkTrans;
            CopyPicture.mBkPos = mBkPos;
            CopyPicture.mBkSize = mBkSize;
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
            mBkImgFile = SourcePicture.mBkImgFile;
            mBkShow = SourcePicture.mBkShow;
            mBkTrans = SourcePicture.mBkTrans;
            mBkPos = SourcePicture.mBkPos;
            mBkSize = SourcePicture.mBkSize;
            mPriBase = SourcePicture.mPriBase;
            mDrawPos = SourcePicture.mDrawPos;
            mStepDraw = SourcePicture.mStepDraw;
            mCurrentPen = SourcePicture.mCurrentPen;
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
        /// Returns true if the specified paramters represent a cel baseline that is
        /// entirely on water.
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
            if (X > 159) {
                throw new ArgumentOutOfRangeException(nameof(X));
            }
            if (Y > 167) {
                throw new ArgumentOutOfRangeException(nameof(Y));
            }
            if (!mPicBMPSet) {
                BuildBMPs();
            }
            if (X + Length > 159) {
                Length = (byte)(160 - X);
            }
            EndX = (byte)(X + Length - 1);
            for (i = X; i <= EndX; i++) {
                CurPri = (AGIColorIndex)mPriData[i + 160 * Y];
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
            mBkImgFile = "";
            mBkPos = new();
            mBkSize = new();
            mBkTrans = 0;
            mBkShow = false;
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
            mBkImgFile = parent.agGameProps.GetSetting("Picture" + Number, "BkgdImg", "");
            if (mBkImgFile.Length != 0) {
                mBkShow = parent.agGameProps.GetSetting("Picture" + Number, "BkgdShow", false);
                mBkTrans = parent.agGameProps.GetSetting("Picture" + Number, "BkgdTrans", 0);
                mBkPos.FromString(parent.agGameProps.GetSetting("Picture" + Number, "BkgdPosn", ""));
                mBkSize.FromString(parent.agGameProps.GetSetting("Picture" + Number, "BkgdSize", ""));
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
            mBkImgFile = "";
            mBkShow = false;
            mBkTrans = 0;
            mBkPos.FromString("");
            mBkSize.FromString("");
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
            ColorPalette ncp = bmpVis.Palette;
            for (int i = 0; i < 16; i++) {
                if (parent != null) {
                    ncp.Entries[i] = Color.FromArgb(255,
                    parent.Palette[i].R,
                    parent.Palette[i].G,
                    parent.Palette[i].B);
                }
                else {
                    ncp.Entries[i] = Color.FromArgb(255,
                    mPalette[i].R,
                    mPalette[i].G,
                    mPalette[i].B);
                }
            }
            // both bitmaps use same palette
            bmpVis.Palette = ncp;
            bmpPri.Palette = ncp;
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
            // copy the picture data to the bitmaps
            Marshal.Copy(mVisData, 0, ptrVis, 26880);
            bmpVis.UnlockBits(bmpVisData);
            Marshal.Copy(mPriData, 0, ptrPri, 26880);
            bmpPri.UnlockBits(bmpPriData);
            // update pen status
            mCurrentPen = SavePen;
            mPicBMPSet = true;
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
