using System;
using System.ComponentModel;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Commands;
using static WinAGI.Common.Base;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace WinAGI.Engine
{
    public class Picture : AGIResource
    {
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
        int mBMPErrLvl; //holds current error level of the BMP build
                        //gets updated everytime bitmaps get built
                        //variables used for low level graphics handling
        Bitmap bmpVis;
        Bitmap bmpPri;
        public Picture() : base(AGIResType.rtPicture)
        {
            //initialize
            mResID = "NewPicture";
            //attach events
            base.PropertyChanged += ResPropChange;
            strErrSource = "WinAGI.Picture";

            //create default picture with no commands
            //mRData = new RData(1);
            //mRData[0] = 0xff;
            base.WriteByte(0xff);
            //default to entire image
            mDrawPos = -1;
            //default pribase is 48
            mPriBase = 48;
        }
        public Picture(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.rtPicture)
        {
            //this internal function adds this resource to a game, setting its resource 
            //location properties, and reads properties from the wag file

            //initialize
            //attach events
            base.PropertyChanged += ResPropChange;
            strErrSource = "WinAGI.Picture";
            //default to entire image
            mDrawPos = -1;
            //default pribase is 48
            mPriBase = 48;

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
        }
        void ResPropChange(object sender, AGIResPropChangedEventArgs e)
        {
            //set flag to indicate picture data does not match picture bmps
            mPicBMPSet = false;
            //for picture only, changing resource sets dirty flag
            mIsDirty = true;
        }
        internal Picture Clone()
        {
            //copies picture data from this picture and returns a completely separate object reference
            Picture CopyPicture = new Picture();
            // copy base properties
            base.SetRes(CopyPicture);
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
            CopyPicture.mBMPErrLvl = mBMPErrLvl;
            return CopyPicture;
        }
        public string BkgdImgFile
        {
            get
            {
                return mBkImgFile;
            }
            set
            {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                mBkImgFile = value;
                WritePropState = true;
            }
        }
        public string BkgdPosition
        {
            get
            {
                return mBkPos;
            }
            set
            {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                mBkPos = value;
                WritePropState = true;
            }
        }
        public string BkgdSize
        {
            get
            {
                return mBkSize;
            }
            set
            {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                mBkSize = value;
                WritePropState = true;
            }
        }
        public int BkgdTrans
        {
            get
            {
                return mBkTrans;
            }
            set
            {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                mBkTrans = value;
                WritePropState = true;
            }
        }
        public bool BkgdShow
        {
            get
            {
                return mBkShow;
            }
            set
            {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                mBkShow = value;
                WritePropState = true;
            }
        }
        public long BMPErrLevel
        {
            get
            {
                //provides access to current error level of the BMP build
                //
                //can be used by calling programs to provide feedback
                //on errors in the picture data
                // it's most useful when entire picture is built; not as
                // useful for partial builds

                //return 0 if successful, no errors/warnings
                // non-zero for error/warning:
                //  -1 = error- can't build the bitmap
                //  1 = no EOP marker
                //  2 = bad vis color data
                //  4 = invalid command byte
                //  8 = other error
                return mBMPErrLvl;
            }
        }
        public byte PriBase
        {
            get
            {
                //if not in a game, or if before v2.936, always return value of 48
                if (!parent.agGameLoaded || Val(parent.agIntVersion) < 2.936) {
                    mPriBase = 48;
                }
                return mPriBase;
            }
            set
            {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
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
        public byte[] VisData
        {
            get
            {
                //if not loaded
                if (!Loaded) {
                    //error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                //if picture data changed,
                if (!mPicBMPSet) {
                    //load pictures
                    BuildPictures();
                }
                return mVisData;
            }
        }
        public byte[] PriData
        {
            get
            {
                //if not loaded
                if (!Loaded) {
                    //error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                //if picture data changed,
                if (!mPicBMPSet) {
                    //load pictures
                    BuildPictures();
                }
                return mPriData;
            }
        }
        public PenStatus CurrentToolStatus
        {
            get
            {
                //if not loaded
                if (!Loaded) {
                    //error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                return mCurrentPen;
            }
        }
        public int DrawPos
        {
            get
            {
                //if not loaded
                if (!Loaded) {
                    //error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                return mDrawPos;
            }
            set
            {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
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
        public bool ObjOnWater(byte X, byte Y, byte Length)
        {
            //returns true if testcel at position x,Y is entirely on water
            byte i;
            AGIColorIndex CurPri;
            byte EndX;
            //if not loaded
            if (!Loaded) {
                //error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            //validate x,Y
            if (X > 159 || Y > 167) {

                Exception e = new(LoadResString(621))
                {
                    HResult = WINAGI_ERR + 621
                };
                throw e;
            }
            //if picture data changed,
            if (!mPicBMPSet) {
                try {
                    //load pictures
                    BuildPictures();
                }
                catch (Exception) {
                    //pass along error
                    throw;
                }
            }
            //ensure enough room for length
            if (X + Length > 159) {
                Length = (byte)(160 - X);
            }

            //step through all pixels on this line
            EndX = (byte)(X + Length - 1);
            for (i = X; i <= EndX; i++) {
                CurPri = (AGIColorIndex)mPriData[i + 160 * Y];
                if (CurPri != AGIColorIndex.agCyan) {
                    //not on water- return false
                    return false;
                }
            }
            //if not exited, must be on water
            return true;
        }
        public AGIColorIndex PixelControl(byte X, byte Y, byte Length = 1)
        {
            //returns the actual lowest priority code for a line,
            //including control codes
            byte i = 0;
            AGIColorIndex CurPri, retval;
            //if not loaded,
            if (!Loaded) {
                //raise error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            //validate x,Y
            if (X > 159 || Y > 167) {

                Exception e = new(LoadResString(621))
                {
                    HResult = WINAGI_ERR + 621
                };
                throw e;
            }
            //if picture data changed,
            if (!mPicBMPSet) {
                try {
                    //load pictures
                    BuildPictures();
                }
                catch (Exception) {
                    // pass along the error
                    throw;
                }
            }
            //default to max Value
            retval = AGIColorIndex.agWhite; //15
                                            //ensure enough room for length
            if (X + Length > 159) {
                Length = (byte)(160 - X);
            }
            //get lowest pixel priority on the line
            do {
                CurPri = (AGIColorIndex)mPriData[X + i + 160 * Y];
                if (CurPri < retval) {
                    retval = CurPri;
                }
                i++;
            }
            while (i < Length); //Until i = Length
            return retval;
        }
        public AGIColorIndex VisPixel(byte X, byte Y)
        {
            //returns visual pixel color
            //if not loaded,
            if (!Loaded) {
                //raise error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            //validate x,Y
            if (X > 159 || Y > 167) {

                Exception e = new(LoadResString(621))
                {
                    HResult = WINAGI_ERR + 621
                };
                throw e;
            }
            //if picture data changed,
            if (!mPicBMPSet) {
                try {
                    //load pictures
                    BuildPictures();
                }
                catch (Exception) {
                    // pass along the error
                    throw;
                }
            }
            return (AGIColorIndex)mVisData[X + 160 * Y];
        }
        public AGIColorIndex PriPixel(byte X, byte Y)
        {
            //if not loaded,
            if (!Loaded) {
                //raise error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            //validate x,y
            if (X > 159 || Y > 167) {

                Exception e = new(LoadResString(621))
                {
                    HResult = WINAGI_ERR + 621
                };
                throw e;
            }
            //if picture data changed,
            if (!mPicBMPSet) {
                try {
                    //load pictures
                    BuildPictures();
                }
                catch (Exception) {
                    // pass along the error
                    throw;
                }
            }
            return (AGIColorIndex)mPriData[X + 160 * Y];
        }
        public AGIColorIndex PixelPriority(byte X, byte Y)
        {
            //if not loaded,
            if (!Loaded) {
                //raise error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            //validate x,Y
            if (X > 159 || Y > 167) {

                Exception e = new(LoadResString(621))
                {
                    HResult = WINAGI_ERR + 621
                };
                throw e;
            }
            //if picture data changed,
            if (!mPicBMPSet) {
                try {
                    //load pictures
                    BuildPictures();
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
            }
            AGIColorIndex retval;
            //set default to 15
            AGIColorIndex defval = AGIColorIndex.agWhite;// 15;

            //need to loop to find first pixel that is NOT a control color (0-2)
            do {
                //get pixel priority
                retval = (AGIColorIndex)mPriData[X + 160 * Y];
                //move down to next row
                Y++;
            }
            while (retval < AGIColorIndex.agCyan && Y < 168);// Until PixelPriority >= 3 || Y = 168
                                                             // if not valid
            if (retval < AGIColorIndex.agCyan) {
                return defval;
            }
            else {
                return retval;
            }
        }
        public void Export(string ExportFile, bool ResetDirty = true)
        {
            //if not loaded
            if (!Loaded) {
                //error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            // use base function
            base.Export(ExportFile);
            //if not in a game,
            if (!InGame) {
                //ID always tracks the resfile name
                mResID = JustFileName(ExportFile);
                if (mResID.Length > 64) {
                    mResID = Left(mResID, 64);
                }
                if (ResetDirty) {
                    //clear dirty flag
                    mIsDirty = false;
                }
            }
        }
        public override void Import(string ImportFile)
        {
            //imports a picture resource
            try {
                //use base function
                Import(ImportFile);
            }
            catch (Exception) {
                // pass along any errors
                throw;
            }
            //set ID
            mResID = JustFileName(ImportFile);
            if (mResID.Length > 64) {
                mResID = Left(mResID, 64);
            }
            //reset dirty flag
            mIsDirty = false;
        }
        public override void Clear()
        {
            if (InGame) {
                if (!Loaded) {
                    //nothing to clear

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
            }
            base.Clear();
            //after clearing, resource has NO data; so we need
            //to ADD a byte; not try to change byte0
            WriteByte(0xFF);
            //reset position pointer
            mDrawPos = 1;
            //load pictures
            BuildPictures();
        }
        public override void Load()
        {
            //load data into the bitmap data area for
            //this set of bitmaps
            if (Loaded) {
                return;
            }
            //if not ingame, the resource is already loaded
            if (!InGame) {
                throw new Exception("non-game pic should already be loaded");
            }
            try {
                //load base resource
                base.Load();
            }
            catch (Exception) {
                // pass along any error
                throw;
            }
            //load bkgd info, if there is such
            mBkImgFile = parent.agGameProps.GetSetting("Picture" + Number, "BkgdImg", "");
            if (mBkImgFile.Length != 0) {
                mBkShow = parent.agGameProps.GetSetting("Picture" + Number, "BkgdShow", false);
                mBkTrans = parent.agGameProps.GetSetting("Picture" + Number, "BkgdTrans", 0);
                mBkPos = parent.agGameProps.GetSetting("Picture" + Number, "BkgdPosn", "");
                mBkSize = parent.agGameProps.GetSetting("Picture" + Number, "BkgdSize", "");
            }
            try {
                //load picture bmps
                BuildPictures();
            }
            catch (Exception) {
                // pass along any errors
                throw;
            }
            //clear dirty flag
            mIsDirty = false;
        }
        // ResetBMP used to force reset when palette changes
        // (or any other reason that needs the cel to be refreshed)
        public void ResetBMP()
        {
            mPicBMPSet = false;
        }
        public Bitmap VisualBMP
        {
            get
            {
                //returns a device context to the bitmap image of the visual screenoutput
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                //if pictures not built, or have changed,
                if (!mPicBMPSet) {
                    try {
                        //load pictures to get correct pictures
                        BuildPictures();
                    }
                    catch (Exception) {
                        // pass error along
                        throw;
                    }
                }
                return bmpVis;
            }
        }
        public Bitmap PriorityBMP
        {
            get
            {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                //if pictures not built, or have changed,
                if (!mPicBMPSet) {
                    try {
                        //load pictures to get correct pictures
                        BuildPictures();
                    }
                    catch (Exception) {
                        // pass along any errors
                        throw;
                    }
                    //if errors,
                }
                return bmpPri;
            }
        }
        public void Save()
        {
            //if properties need to be written
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
            //if not loaded
            if (!Loaded) {
                //nothing to do
                return;
            }
            //if dirty
            if (mIsDirty) {
                //(no picture-specific action needed, since changes in picture are
                //made directly to resource data)
                //use the base save method
                try {
                    base.Save();
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
                //check for errors
            }
            //reset flag
            mIsDirty = false;
        }
        public void SetPictureData(byte[] PicData)
        {
            //sets the picture resource data to PicData()
            //
            //if the data is invalid, the resource will
            //be identified as corrupted
            //(clear the picture, replace it with valid data
            //or unload it without saving to recover from
            //invalid input data)
            //copy the picture data
            try {
                mRData.AllData = PicData;
                if (Loaded) {
                    //rebuild pictures
                    BuildPictures();
                }
            }
            catch (Exception) {
                //ignore?
            }
        }
        internal void BuildPictures()
        {
            int i;
            //create new visual picture bitmap
            bmpVis = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
            //create new priority picture bitmap
            bmpPri = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
            //modify color palette to match current AGI palette
            ColorPalette ncp = bmpVis.Palette;
            for (i = 0; i < 16; i++) {
                ncp.Entries[i] = Color.FromArgb(255,
                parent.AGIColors[i].R,
                parent.AGIColors[i].G,
                parent.AGIColors[i].B);
                // ncp.Entries[i] = EGAColor[i];
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

            //now we can create our custom data arrays
            // array size is determined by stride (bytes per row) and height
            mVisData = new byte[26880];
            mPriData = new byte[26880];
            //build arrays of bitmap data
            //Build function returns an error level value
            mBMPErrLvl = BuildBMPs(ref mVisData, ref mPriData, mRData.AllData, mStepDraw ? mDrawPos : -1, mDrawPos);

            // copy the picture data to the bitmaps
            Marshal.Copy(mVisData, 0, ptrVis, 26880);
            bmpVis.UnlockBits(bmpVisData);
            Marshal.Copy(mPriData, 0, ptrPri, 26880);
            bmpPri.UnlockBits(bmpPriData);

            //get pen status
            mCurrentPen = GetToolStatus();
            //set flag
            mPicBMPSet = true;
        }
        public bool StepDraw
        {
            get
            {
                //if not loaded,
                if (!Loaded) {
                    //raise error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                return mStepDraw;
            }
            set
            {
                if (!mLoaded) {

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                // if a change
                if (mStepDraw != value) {
                    mStepDraw = value;
                    //set flag to force redraw
                    mPicBMPSet = false;
                }
            }
        }
        public override void Unload()
        {
            //unload resource
            base.Unload();
            //cleanup picture resources
            bmpVis = null;
            bmpPri = null;
            mPicBMPSet = false;
        }
    }
}
