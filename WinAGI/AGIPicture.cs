using System;
using System.ComponentModel;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGICommands;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace WinAGI
{
  public class AGIPicture : AGIResource
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
    public AGIPicture() : base(AGIResType.rtPicture, "NewPicture")
    {
      //initialize
      //attach events
      base.PropertyChanged += ResPropChange;
      strErrSource = "WinAGI.Picture";
      //set default resource data
      mRData = new RData(1);
      //create default picture with no commands
      mRData[0] = 0xff;
      //initialize the DIBSection headers
      //default to entire image
      mDrawPos = -1;
      //default pribase is 48
      mPriBase = 48;
   }
    internal void InGameInit(byte ResNum, sbyte VOL, int Loc)
    {
      //this internal function adds this resource to a game, setting its resource 
      //location properties, and reads properties from the wag file

      //set up base resource
      base.InitInGame(ResNum, VOL, Loc);

      //if first time loading this game, there will be nothing in the propertyfile
      ID = ReadSettingString(agGameProps, "Picture" + ResNum, "ID", "");
      if (ID.Length == 0)
      {
        //no properties to load; save default ID
        ID = "Picture" + ResNum;
        WriteGameSetting("Logic" + ResNum, "ID", ID, "Pictures");
        //load resource to get size
        Load();
        WriteGameSetting("Picture" + ResNum, "Size", Size.ToString());
        Unload();
      }
      else
      {
        //get description, size and other properties from wag file
        mDescription = ReadSettingString(agGameProps, "Picture" + ResNum, "Description", "");
        Size = ReadSettingLong(agGameProps, "Picture" + ResNum, "Size", -1);
      }
    }
    void ResPropChange(object sender, AGIResPropChangedEventArgs e)
    {
      //set flag to indicate picture data does not match picture bmps
      mPicBMPSet = false;
      //for picture only, changing resource sets dirty flag
      mIsDirty = true;
    }
    internal void SetPicture(AGIPicture CopyPicture)
    {
      //copies picture data from CopyPicture into this picture
      //add resource data
      base.SetRes(CopyPicture);
      //add WinAGI items
      mBkImgFile = CopyPicture.BkgdImgFile;
      mBkShow = CopyPicture.BkgdShow;
      mBkTrans = CopyPicture.BkgdTrans;
      mBkPos = CopyPicture.BkgdPosition;
      mBkSize = CopyPicture.BkgdSize;
      mPriBase = CopyPicture.PriBase;
      //if loaded,
      if (CopyPicture.Loaded)
      {
      try
        {
          //load pictures
          BuildPictures();
        }
        catch (Exception)
        {
          // ignore errors
        }
      }
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
        if (!Loaded)
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
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
        if (!Loaded) 
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
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
        if (!Loaded)
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
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
        if (!Loaded)
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
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
        if (!Loaded)
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
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
        if (!agGameLoaded || Val(agIntVersion) < 2.936) {
          mPriBase = 48;
        }
        return mPriBase;
      }
      set
      {
        //if not loaded,
        if (!Loaded)
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //max value is 158
        if (value > 158)
        {
          mPriBase = 158;
        }
        else
        {
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
        if (!Loaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //if picture data changed,
        if (!mPicBMPSet)
        {
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
        if (!Loaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //if picture data changed,
        if (!mPicBMPSet)
        {
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
        if (!Loaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mCurrentPen;
      }
    }
    public int DrawPos
    {
      get
      {
        //if not loaded
        if (!Loaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mDrawPos;
      }
      set
      {
        //if not loaded,
        if (!Loaded)
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //if not changed
        if (value == mDrawPos)
        {
          return;
        }
        //validate input
        if (value < 0)
        {
          mDrawPos = -1;
        }
        else if (value >= Size)
        {
          mDrawPos = Size - 1;
        }
        else
        {
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
      AGIColors CurPri;
      byte EndX;
      //if not loaded
      if (!Loaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //validate x,Y
      if (X > 159 || Y > 167)
      {
        throw new Exception("621, strErrSource, LoadResString(621)");
      }
      //if picture data changed,
      if (!mPicBMPSet)
      {
        try
        {
          //load pictures
          BuildPictures();
        }
        catch (Exception)
        {
          //pass along error
          throw;
        }
      }
      //ensure enough room for length
      if (X + Length > 159)
      {
        Length = (byte)(160 - X);
      }

      //step through all pixels on this line
      EndX = (byte)(X + Length - 1);
      for (i = X; i <= EndX; i++)
      {
        CurPri = (AGIColors)mPriData[i + 160 * Y];
        if (CurPri != AGIColors.agCyan)
        {
          //not on water- return false
          return false;
        }
      }
      //if not exited, must be on water
      return true;
    }
    public AGIColors PixelControl(byte X, byte Y, byte Length = 1)
    {
      //returns the actual lowest priority code for a line,
      //including control codes
      byte i = 0;
      AGIColors CurPri, retval;
      //if not loaded,
      if (!Loaded)
      {
        //raise error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //validate x,Y
      if (X > 159 || Y > 167) {
        throw new Exception("621, strErrSource, LoadResString(621)");
      }
      //if picture data changed,
      if (!mPicBMPSet)
      {
        try
        {
          //load pictures
          BuildPictures();
        }
        catch (Exception)
        {
          // pass along the error
          throw;
        }
      }
      //default to max Value
      retval = AGIColors.agWhite; //15
      //ensure enough room for length
      if (X + Length > 159)
      {
        Length = (byte)(160 - X);
      }
      //get lowest pixel priority on the line
      do
      {
        CurPri = (AGIColors)mPriData[X + i + 160 * Y];
        if (CurPri < retval)
        {
          retval = CurPri;
        }
        i++;
      }
      while (i < Length); //Until i = Length
      return retval;
    }
    public AGIColors VisPixel(byte X, byte Y)
    {
      //returns visual pixel color
      //if not loaded,
      if (!Loaded)
      {
        //raise error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //validate x,Y
      if (X > 159 || Y > 167) {
        throw new Exception("621, strErrSource, LoadResString(621)");
      }
      //if picture data changed,
      if (!mPicBMPSet)
      {
        try
        {
          //load pictures
          BuildPictures();
        }
        catch (Exception)
        {
          // pass along the error
          throw;
        }
      }
      return (AGIColors)mVisData[X + 160 * Y];
    }
    public AGIColors PriPixel(byte X, byte Y)
    {
      //if not loaded,
      if (!Loaded)
      {
        //raise error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //validate x,y
      if (X > 159 || Y > 167) 
      {
        throw new Exception("621, strErrSource, LoadResString(621)");
      }
      //if picture data changed,
      if (!mPicBMPSet)
      {
        try
        {
          //load pictures
          BuildPictures();
        }
        catch (Exception)
        {
          // pass along the error
          throw;
        }
      }
      return (AGIColors)mPriData[X + 160 * Y];
    }
    public AGIColors PixelPriority(byte X, byte Y)
    {
      //if not loaded,
      if (!Loaded)
      {
        //raise error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //validate x,Y
      if (X > 159 || Y > 167) 
      {
        throw new Exception("621, strErrSource, LoadResString(621)");
      }
      //if picture data changed,
      if (!mPicBMPSet)
      {
        try
        {
          //load pictures
          BuildPictures();
        }
        catch (Exception)
        {
          // pass along any errors
          throw;
        }
      }
      AGIColors retval;
      //set default to 15
      AGIColors defval = AGIColors.agWhite;// 15;

      //need to loop to find first pixel that is NOT a control color (0-2)
      do
      {
        //get pixel priority
        retval = (AGIColors)mPriData[X + 160 * Y];
        //move down to next row
        Y++;
      }
      while (retval < AGIColors.agCyan && Y < 168);// Until PixelPriority >= 3 || Y = 168
      // if not valid
      if (retval < AGIColors.agCyan)
      {
        return defval;
      }
      else
      {
        return retval;
      }
    }
    public void Export(string ExportFile, bool ResetDirty = true)
    {
      //if not loaded
      if (!Loaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      // use base function
      base.Export(ExportFile);
      //if not in a game,
      if (!InGame)
      {
        //ID always tracks the resfile name
        mResID = JustFileName(ExportFile);
        if (mResID.Length > 64)
        {
          mResID = Left(mResID, 64);
        }
        if (ResetDirty)
        {
          //clear dirty flag
          mIsDirty = false;
        }
      }
    }
    public override void Import(string ImportFile)
    {
      //imports a picture resource
      try
      {
        //use base function
        Import(ImportFile);
      }
      catch (Exception)
      {
        // pass along any errors
        throw;
      }
      //set ID
      mResID = JustFileName(ImportFile);
      if (mResID.Length > 64)
      {
        mResID = Left(mResID, 64);
      }
      //reset dirty flag
      mIsDirty = false;
    }
    public override void Clear()
    {
      if (InGame)
      {
        if (!Loaded)
        {
          //nothing to clear
          throw new Exception("563, strErrSource, LoadResString(563)");
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
      if (Loaded)
      {
        return;
      }
      //if not ingame, the resource is already loaded
      if (!InGame)
      {
        throw new Exception("non-game pic should already be loaded");
      }
      try
      {
        //load base resource
        base.Load();
      }
      catch (Exception)
      {
        // pass along any error
        throw;
      }
      //load bkgd info, if there is such
      mBkImgFile = ReadSettingString(agGameProps, "Picture" + Number, "BkgdImg", "");
      if (mBkImgFile.Length != 0)
      {
        mBkShow = ReadSettingBool(agGameProps, "Picture" + Number, "BkgdShow", false);
        mBkTrans = ReadSettingLong(agGameProps, "Picture" + Number, "BkgdTrans", 0);
        mBkPos = ReadSettingString(agGameProps, "Picture" + Number, "BkgdPosn", "");
        mBkSize = ReadSettingString(agGameProps, "Picture" + Number, "BkgdSize", "");
      }
      try
      {
        //load picture bmps
        BuildPictures();
      }
      catch (Exception)
      {
        // pass along any errors
        throw;
      }
      //clear dirty flag
      mIsDirty = false;
    }
    public Bitmap VisualBMP
    {
      get
      {
        //returns a device context to the bitmap image of the visual screenoutput
        //if not loaded,
        if (!Loaded)
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //if pictures not built, or have changed,
        if (!mPicBMPSet)
        {
          try
          {
            //load pictures to get correct pictures
            BuildPictures();
          }
          catch (Exception)
          {
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
        if (!Loaded)
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //if pictures not built, or have changed,
        if (!mPicBMPSet)
        {
          try
          {
            //load pictures to get correct pictures
            BuildPictures();
          }
          catch (Exception)
          {
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
      if (WritePropState && mInGame)
      {
        //saves the picture resource
        //save ID and description to ID file
        string strPicKey = "Picture" + Number;
        WriteGameSetting(strPicKey, "ID", mResID, "Pictures");
        WriteGameSetting(strPicKey, "Description", mDescription);
        if (mPriBase != 48)
        {
          WriteGameSetting(strPicKey, "PriBase", mPriBase.ToString());
        }
        else
        {
          DeleteSettingKey(agGameProps, strPicKey, "PriBase");
        }
        //if no bkgdfile, delete other settings
        if (mBkImgFile.Length == 0)
        {
          mBkShow = false;
          DeleteSettingKey(agGameProps, strPicKey, "BkgdImg");
          DeleteSettingKey(agGameProps, strPicKey, "BkgdShow");
          //mBkTrans = 0
          DeleteSettingKey(agGameProps, strPicKey, "BkgdTrans");
          DeleteSettingKey(agGameProps, strPicKey, "BkgdPosn");
          DeleteSettingKey(agGameProps, strPicKey, "BkgdSize");
        }
        else
        {
          WriteGameSetting(strPicKey, "BkgdImg", mBkImgFile);
          WriteGameSetting(strPicKey, "BkgdShow", mBkShow.ToString());
          WriteGameSetting(strPicKey, "BkgdTrans", mBkTrans.ToString());
          WriteGameSetting(strPicKey, "BkgdPosn", mBkPos);
          WriteGameSetting(strPicKey, "BkgdSize", mBkSize);
        }
        WritePropState = false;
      }
      //if not loaded
      if (!Loaded)
      {
        //nothing to do
        return;
      }
      //if dirty
      if (mIsDirty)
      {
        //(no picture-specific action needed, since changes in picture are
        //made directly to resource data)
        //use the base save method
        try
        {
          base.Save();
        }
        catch (Exception)
        {
          // pass along any errors
          throw;
        }
        //check for errors
      }

      WriteGameSetting("Picture" + Number, "Size", Size.ToString(), "Pictures");

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
      try
      {
        mRData.AllData = PicData;
        if (Loaded)
        {
          //rebuild pictures
          BuildPictures();
        }
      }
      catch (Exception)
      {
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
      for (i = 0; i < 16; i++)
      {
        ncp.Entries[i] = Color.FromArgb(255,
        (int)((lngEGARevCol[i] & 0xFF0000) / 0x10000),
        (int)((lngEGARevCol[i] & 0xFF00) / 0x100),
        (int)(lngEGARevCol[i] % 0x100)
          );
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
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mStepDraw;
      }
      set
      {
        if (!mLoaded)
        {
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        // if a change
        if (mStepDraw != value)
        {
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