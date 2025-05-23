﻿using System;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIColorIndex;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data.Common;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents a single cel in an AGI View resource.
    /// </summary>
    [Serializable]
    public class Cel {
        #region Local Members
        internal byte mWidth;
        internal byte mHeight;
        internal AGIColorIndex mTransColor;
        internal byte[,] mCelData;
        internal int mIndex;
        /// <summary>
        /// When blnCelBMPSet is false, it means cel bitmap needs to be rebuilt.
        /// </summary>
        internal bool blnCelBMPSet;
        //internal bool mTransparency;
        /// <summary>
        /// When mCelChanged is true, it means cel data has changed.
        /// </summary>
        internal bool mCelChanged;
        internal Bitmap mCelBMP;
        internal Bitmap mTransBMP;
        /// <summary>
        /// mSetMirror is true if cel is supposed to show the mirror.
        /// </summary>
        internal bool mSetMirror;
        /// <summary>
        /// mMirrored is true if the cel IS showing the mirror.
        /// </summary>
        internal bool mMirrored;
        [NonSerialized]
        internal readonly View parentview;
        internal EGAColors mPalette;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new one-pixel cel (that is not part of a view resource)
        /// with color set to default transparent color (black).
        /// </summary>
        public Cel() {
            mCelData = new byte[1, 1];
            mTransColor = Black;
            mCelData[0,0] = (byte)Black;
            mWidth = 1;
            mHeight = 1;
            mPalette = defaultPalette.Clone();
        }

        /// <summary>
        /// Creates a new one-pixel cel for a cel that is part of a view  resource 
        /// with color set to default transparent color (black).
        /// </summary>
        /// <param name="parent"></param>
        internal Cel(View parent) {
            mCelData = new byte[1, 1];
            mTransColor = Black;
            mCelData[0, 0] = (byte)(Black);
            mWidth = 1;
            mHeight = 1;
            parentview = parent;
            // assign default palette (for cels not attached to
            // a view)
            mPalette = defaultPalette.Clone();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the pixel color for this cel at the specified coordinates.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte this[int xPos, int yPos] {
            get {
                if (xPos < 0 || xPos > mWidth - 1) {
                    throw new ArgumentOutOfRangeException(nameof(xPos));
                }
                if (yPos < 0 || yPos > mHeight - 1) {
                    throw new ArgumentOutOfRangeException(nameof(yPos));
                }
                if (mSetMirror) {
                    // reverse x direction
                    return mCelData[mWidth - 1 - xPos, yPos];
                }
                else {
                    return mCelData[xPos, yPos];
                }
            }
            set {
                if (xPos >= mWidth) {
                    throw new ArgumentOutOfRangeException(nameof(xPos));
                }
                if (yPos >= mHeight) {
                    throw new ArgumentOutOfRangeException(nameof(yPos));
                }
                if (value > 15) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                // check mirror state
                if (mSetMirror) {
                    // if this is the mirrored cel, reverse x direction
                    mCelData[mWidth - 1 - xPos, yPos] = value;
                }
                else {
                    mCelData[xPos, yPos] = value;
                }
                mCelChanged = true;
                // if loop is mirrored, let other loop know about the change
                if (parentview is not null) {
                    parentview.mViewChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the pixel color for this cel at the specified location.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte this[Point pos] {
            get => this[pos.X, pos.Y];
            set => this[pos.X, pos.Y] = value;
        }

        /// <summary>
        /// Gets the index (position) of this cel within its loop. Only applicable
        /// to loops that are part of a loop.
        /// </summary>
        public int Index {
            get {
                return mIndex;
            }
            internal set {
                // index can only be set by the parent view/loop when
                // adding or deleting cels
                mIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets the entire array of pixel data for this cel, flipping
        /// it if the cel is a mirror not in the primary loop.
        /// </summary>
        public byte[,] AllCelData {
            get {
                byte[,] tmpData;
                int i, j;
                if (mSetMirror) {
                    // need to flip the data
                    tmpData = new byte[mWidth, mHeight];
                    for (i = 0; i < mWidth; i++) {
                        for (j = 0; j < mHeight; j++) {
                            // copy backwards
                            tmpData[mWidth - 1 - i, j] = mCelData[i, j];
                        }
                    }
                    return tmpData;
                }
                else {
                    return mCelData;
                }
            }
            set {
                if (value.GetUpperBound(0) != mWidth - 1) {
                    WinAGIException wex = new(LoadResString(614)) {
                        HResult = WINAGI_ERR + 614,
                    };
                    throw wex;
                }
                if (value.GetUpperBound(1) != mHeight - 1) {
                    WinAGIException wex = new(LoadResString(614)) {
                        HResult = WINAGI_ERR + 614,
                    };
                    throw wex;
                }
                mCelData = value;
                if (parentview is not null) {
                    // mark it as uncompiled
                    parentview.mViewChanged = true;
                }
                mCelChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the width of this cel.
        /// </summary>
        public byte Width {
            get { return mWidth; }
            set {
                int i, j;
                byte[,] tmpData;
                if (value == 0 || value > MAX_CEL_WIDTH) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (mWidth != value) {
                    // can't easily resize multidimensional arrays, so 
                    // make a new array, copy desired data over
                    tmpData = new byte[value, mHeight];
                    for (i = 0; i < value; i++) {
                        for (j = 0; j < mHeight; j++) {
                            if (i >= mWidth) {
                                tmpData[i, j] = (byte)mTransColor;
                            }
                            else {
                                tmpData[i, j] = mCelData[i, j];
                            }
                        }
                    }
                    mCelData = tmpData;
                    mWidth = value;
                    if (parentview is not null) {
                        // mark it as uncompiled
                        parentview.mViewChanged = true;
                    }
                    mCelChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the height of this cel.
        /// </summary>
        public byte Height {
            get { return mHeight; }
            set {
                int i, j;
                byte[,] tmpData;

                if (value == 0 || value > MAX_CEL_HEIGHT) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (mHeight != value) {
                    // can't easily resize multidimensional arrays, so 
                    // make a new array, copy desired data over
                    tmpData = new byte[mWidth, value];
                    // copy data to temp array
                    for (i = 0; i < mWidth; i++) {
                        for (j = 0; j < value; j++) {
                            // if array grew, add transparent pixels
                            if (j >= mHeight) {
                                tmpData[i, j] = (byte)mTransColor;
                            }
                            else {
                                tmpData[i, j] = mCelData[i, j];
                            }
                        }
                    }
                    mCelData = tmpData;
                    mHeight = value;
                    if (parentview is not null) {
                        // mark it as uncompiled
                        parentview.mViewChanged = true;
                    }
                    mCelChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the transparent color for this cel.
        /// </summary>
        public AGIColorIndex TransColor {
            get { return mTransColor; }
            set {
                if (value < 0 || (byte)value > 15) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (value != mTransColor) {
                    mTransColor = value;
                    if (parentview is not null) {
                        // mark it as uncompiled
                        parentview.mViewChanged = true;
                    }
                    mCelChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets the bitmap representation of this cel, with the 
        /// transparent color showing as its normal color.
        /// </summary>
        public Bitmap CelImage {
            get {
                if (blnCelBMPSet) {
                    if ((mSetMirror == mMirrored) && (!mCelChanged)) {
                        return mCelBMP;
                    }
                }
                BuildBMPs();
                return mCelBMP;
            }
        }

        /// <summary>
        /// Gets the bitmap representation of this cel, with the
        /// transparent color showing as transparent.
        /// </summary>
        public Bitmap TransImage {
            get {
                if (blnCelBMPSet) {
                    if ((mSetMirror == mMirrored) && (!mCelChanged)) {
                        return mTransBMP;
                    }
                }
                BuildBMPs();
                return mTransBMP;
            }
        }

        ///// <summary>
        ///// Gets or sets the transparency value for this cel. Transparency determines
        ///// how the transparent color in the cel will be displayed when the bitmap is
        ///// generated. When true, the transparent pixels are made transparent in the
        ///// bitmap. When false, the transparent pixels show as the transparent color.
        ///// </summary>
        //public bool Transparency {
        //    get { return mTransparency; }
        //    set {
        //        if (mTransparency != value) {
        //            mTransparency = value;
        //            mCelChanged = true;
        //            if (parentview is not null) {
        //                parentview.PropsChanged = true;
        //            }
        //        }
        //    }
        //}

        public EGAColors Palette {
            get {
                if (parentview != null) {
                    return parentview.Palette;
                }
                else {
                    return mPalette;
                }
            }
            set {
                if (parentview == null) {
                    mPalette = value.Clone();
                }
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clears the bitmap for this cel to a null value.
        /// </summary>
        private void ClearBMP() {
            mCelBMP = null;
            blnCelBMPSet = false;
        }

        /// <summary>
        /// Creates an exact copy of this Cel with the specified
        /// view parent. Use null for parent if the cloned cel
        /// will not be part of a view.
        /// </summary>
        /// <param name="cloneparent"></param>
        /// <returns>The Cel this method creates.</returns>
        public Cel Clone(View cloneparent = null) {
            Cel CopyCel = new(cloneparent) {
                mWidth = mWidth,
                mHeight = mHeight,
                mTransColor = mTransColor,
                mCelData = CloneCelData(mCelData),
                mIndex = mIndex,
                mSetMirror = mSetMirror,
                mMirrored = mMirrored,
                blnCelBMPSet = blnCelBMPSet,
                //mTransparency = mTransparency,
                mCelChanged = mCelChanged,
            };
            if (parentview != null) {
                // copy parent colors
                CopyCel.mPalette = parentview.Palette.Clone();
            }
            else {
                // copy view colors
                CopyCel.mPalette = mPalette.Clone();
            }
            if (mCelBMP is null) {
                CopyCel.mCelBMP = null;
            }
            else {
                CopyCel.mCelBMP = (Bitmap)mCelBMP.Clone();
            }
            return CopyCel;
        }

        /// <summary>
        /// Copies all data from SourceCel into this cel.
        /// </summary>
        /// <param name="SourceCel"></param>
        public void CloneFrom(Cel SourceCel) {
            mWidth = SourceCel.mWidth;
            mHeight = SourceCel.mHeight;
            mTransColor = SourceCel.mTransColor;
            mCelData = SourceCel.mCelData;
            mIndex = SourceCel.mIndex;
            mSetMirror = SourceCel.mSetMirror;
            mMirrored = SourceCel.mMirrored;
            blnCelBMPSet = SourceCel.blnCelBMPSet;
            //mTransparency = SourceCel.mTransparency;
            mCelChanged = SourceCel.mCelChanged;
            mPalette = SourceCel.mPalette.Clone();
            if (SourceCel.mCelBMP is null) {
                mCelBMP = null;
            }
            else {
            mCelBMP = (Bitmap)SourceCel.mCelBMP.Clone();
            }
        }

        /// <summary>
        /// This method will force bitmaps to refresh. (Used when palette changes
        /// (or any other reason that needs the cel to be refreshed.)
        /// </summary>
        public void ResetBMP() {
            blnCelBMPSet = false;
        }

        private void BuildBMPs() {
            int i, j;
            byte[] mCelBData;
            byte[] mCelTData;

            // create new visual picture bitmap
            mCelBMP = new Bitmap(mWidth, mHeight, PixelFormat.Format8bppIndexed);
            mTransBMP = new Bitmap(mWidth, mHeight, PixelFormat.Format8bppIndexed);
            // set color palettes to match current AGI palette
            ColorPalette ncp = mCelBMP.Palette;
            ColorPalette ncpT = mTransBMP.Palette;
            for (i = 0; i < 16; i++) {
                if (parentview == null) {
                    ncp.Entries[i] = Color.FromArgb(
                        255,
                        mPalette[i].R,
                        mPalette[i].G,
                        mPalette[i].B
                    );
                    ncpT.Entries[i] = Color.FromArgb(
                        (i == (int)mTransColor) ? 0 : 255,
                        mPalette[i].R,
                        mPalette[i].G,
                        mPalette[i].B
                    );
                }
                else {
                    ncp.Entries[i] = Color.FromArgb(
                        255,
                        parentview.Palette[i].R,
                        parentview.Palette[i].G,
                        parentview.Palette[i].B
                    );
                    ncpT.Entries[i] = Color.FromArgb(
                        (i == (int)mTransColor) ? 0 : 255,
                        parentview.Palette[i].R,
                        parentview.Palette[i].G,
                        parentview.Palette[i].B
                    );
                }
            }
            mCelBMP.Palette = ncp;
            mTransBMP.Palette = ncpT;
            var BoundsRect = new Rectangle(0, 0, mWidth, mHeight);
            // create access point for bitmap datas
            BitmapData bmpCelData = mCelBMP.LockBits(BoundsRect, ImageLockMode.WriteOnly, mCelBMP.PixelFormat);
            BitmapData bmpCelTData = mTransBMP.LockBits(BoundsRect, ImageLockMode.WriteOnly, mTransBMP.PixelFormat);
            IntPtr ptrVis = bmpCelData.Scan0;
            IntPtr ptrVisT = bmpCelTData.Scan0;
            // now we can create the custom data arrays
            // array size is determined by stride (bytes per row) and height
            mCelBData = new byte[bmpCelData.Stride * mHeight];
            mCelTData = new byte[bmpCelTData.Stride * mHeight];
            // set cel mirror state to desired Value (determined by mSetMirror)
            mMirrored = mSetMirror;
            for (i = 0; i < mWidth; i++) {
                for (j = 0; j < mHeight; j++) {
                    if (mMirrored) {
                        // set cel data backwards
                        mCelBData[mWidth - i - 1 + j * bmpCelData.Stride] = mCelData[i, j];
                        mCelTData[mWidth - i - 1 + j * bmpCelTData.Stride] = mCelData[i, j];
                    }
                    else {
                        // set cel data forwards
                        mCelBData[i + j * bmpCelData.Stride] = mCelData[i, j];
                        mCelTData[i + j * bmpCelTData.Stride] = mCelData[i, j];
                    }
                }
            }
            // copy the picture data to the bitmaps
            Marshal.Copy(mCelBData, 0, ptrVis, bmpCelData.Stride * mHeight);
            Marshal.Copy(mCelTData, 0, ptrVisT, bmpCelTData.Stride * mHeight);
            mCelBMP.UnlockBits(bmpCelData);
            mTransBMP.UnlockBits(bmpCelTData);
            blnCelBMPSet = true;
        }

        /// <summary>
        /// This method flips the cel data in this cel horizontally.
        /// </summary>
        internal void FlipCel() {
            // this method is needed specifically to support loop changes
            // when a mirrored pair has its secondary (the loop with the
            // negative mirror pair) either deleted, unmirrored, or set to
            // another mirror, the cels original configuration stays correct;
            // BUT if the primary loop is deleted, unmirrored, or set to
            // another mirror, then the cels need to be flipped so the
            // remaining secondary cel will get the data in the correct
            // orientation
            int i, j;
            byte[,] tmpCelData = new byte[mWidth, mHeight];
            for (i = 0; i < mWidth; i++) {
                for (j = 0; j < mHeight; j++) {
                    tmpCelData[mWidth - 1 - i, j] = mCelData[i, j];
                }
            }
            mCelChanged = true;
            if (parentview is not null) {
                // mark it as uncompiled
                parentview.mViewChanged = true;
            }
            mCelData = tmpCelData;
        }

        /// <summary>
        /// This method resets this cel to a single pixel, with color set to
        /// default transparent color (black).
        /// </summary>
        public void Clear() {
            mHeight = 1;
            mWidth = 1;
            mTransColor = Black;
            mCelData = new byte[1, 1];
            mCelData[0, 0] = (byte)Black;
            if (blnCelBMPSet) {
                ClearBMP();
            }
            if (parentview is not null) {
                // mark it as uncompiled
                parentview.mViewChanged = true;
            }
            mCelChanged = true;
        }

        /// <summary>
        /// Sets the mirror state of this cel. Used by the parent Cels collection
        /// to notify the cel that the mirrored loop is being shown.
        /// </summary>
        /// <param name="blnNew"></param>
        internal void SetMirror(bool blnNew) {
            mSetMirror = blnNew;
        }
        private byte[,] CloneCelData(byte[,] source) {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            byte[,] clone = new byte[width, height];

            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    clone[i, j] = source[i, j];
                }
            }

            return clone;
        }
        #endregion
    }
}