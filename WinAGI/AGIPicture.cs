using System;
using System.ComponentModel;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGILogicSourceSettings;
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
    int lngVisDC;
    int lngPriDC;
    BitmapInfo biVis; 
    BitmapInfo biPri; 

    int hVisDIBSec;
    int hPriDIBSec;

    int lngVisAddr;
    int lngPriAddr;

    int hOldVisDIB;
    int hOldPriDIB;
   public AGIPicture() : base(AGIResType.rtPicture, "NewPicture")
    {
      //initialize
      //attach events
      base.PropertyChanged += ResPropChange;
      strErrSource = "WinAGI.Picture";
      //set default resource data
      Data = new RData(0);// ();
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
      ////let's do a test
      //// increment number everytime data changes
      //Number += 1;
    }
    internal void SetPicture(AGIPicture CopyPicture)
    {
      //copies picture data from CopyPicture into this picture
      //add resource data
      base.SetRes(CopyPicture);
      //add WinAGI items
      mResID = CopyPicture.ID;
      mDescription = CopyPicture.Description;
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
      //copy dirty flag and writeprop flag
      mIsDirty = CopyPicture.IsDirty;
      WritePropState = CopyPicture.WritePropState;
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
      while (retval < AGIColors.agCyan && Y < 168);// Until PixelPriority >= 3 Or Y = 168
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
    public void Import(string ImportFile)
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
      //set dirty flag
      mIsDirty = true;
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
      // get other properties, if it's a game resource
      if (InGame)
      {
        //load bkgd info, if there is such
        mBkImgFile = ReadSettingString(agGameProps, "Picture" + Number, "BkgdImg", "");
        if (mBkImgFile.Length != 0)
        {
          mBkShow = ReadSettingBool(agGameProps, "Picture" + Number, "BkgdShow", false);
          mBkTrans = ReadSettingLong(agGameProps, "Picture" + Number, "BkgdTrans", 0);
          mBkPos = ReadSettingString(agGameProps, "Picture" + Number, "BkgdPosn", "");
          mBkSize = ReadSettingString(agGameProps, "Picture" + Number, "BkgdSize", "");
        }
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
    public int PriorityBMP
    {
      get
      {
        int rtn;
        //if not loaded,
        if (!Loaded)
        {
          //raise error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //if pictures not built, or have changed,
        if (!mPicBMPSet)
        {
          //if an old picture is loaded,
          if (lngVisDC != 0)
          {
            //unload current picture resource
            rtn = SelectObject(lngVisDC, hOldVisDIB);
            rtn = SelectObject(lngPriDC, hOldPriDIB);
            rtn = DeleteDC(lngVisDC);
            rtn = DeleteDC(lngPriDC);
            rtn = DeleteObject(hVisDIBSec);
            rtn = DeleteObject(hPriDIBSec);
            lngVisDC = 0;
            lngPriDC = 0;
            hOldVisDIB = 0;
            hOldPriDIB = 0;
            lngVisAddr = 0;
            lngPriAddr = 0;
            hVisDIBSec = 0;
            hPriDIBSec = 0;
          }
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
        return lngPriDC;
      }
    }
    public void Save()
    {
      //saves the picture resource
      string strPicKey = "";
      //if properties need to be written
      if (WritePropState && mInGame) {
        //save ID and description to ID file
        strPicKey = "Picture" + Number;
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
      //if dirty
      if (mIsDirty)
      {
        //if not loaded
        if (!Loaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
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
      int rtn, i;

      //Bitmap bmVis = new Bitmap(160, 168);
      var bmVis = new Bitmap(160, 168, PixelFormat.Format8bppIndexed);
      ColorPalette ncp = bmVis.Palette;
      for (i = 0; i < 16; i++)
      {
        ncp.Entries[i] = Color.FromArgb(255,
        (int)((lngEGARevCol[i] & 0xFF0000) / 0x10000),
        (int)((lngEGARevCol[i] & 0xFF00) / 0x100),
        (int)(lngEGARevCol[i] % 0x100)
          );
      }
      bmVis.Palette = ncp;
      var BoundsRect = new Rectangle(0, 0, 160, 168);
      BitmapData bmpData = bmVis.LockBits(BoundsRect,
                                      ImageLockMode.WriteOnly,
                                      bmVis.PixelFormat);
      IntPtr ptr = bmpData.Scan0;
      int bytes = bmpData.Stride * bmVis.Height;
      var rgbValues = new byte[bytes];

      // fill in rgbValues, e.g. with a for loop over an input array

      Marshal.Copy(rgbValues, 0, ptr, bytes);
      bmVis.UnlockBits(bmpData);













      //if picture bmps already exist
      if (lngVisDC != 0 || lngPriDC != 0) 
      {
        //cleanup picture resources
        rtn = SelectObject(lngVisDC, hOldVisDIB);
        rtn = SelectObject(lngPriDC, hOldPriDIB);
        rtn = DeleteObject(hVisDIBSec);
        rtn = DeleteObject(hPriDIBSec);
        rtn = DeleteDC(lngVisDC);
        rtn = DeleteDC(lngPriDC);
        lngVisDC = 0;
        lngPriDC = 0;
        hOldVisDIB = 0;
        hOldPriDIB = 0;
        lngVisAddr = 0;
        lngPriAddr = 0;
        hVisDIBSec = 0;
        hPriDIBSec = 0;
      }
      //load the colors
      biVis.bmiColor = new RGBQUAD[16];
      biPri.bmiColor = new RGBQUAD[16];
      for (i = 0; i < 16; i++)
      {
        biVis.bmiColor[i].rgbBlue = (byte)(lngEGARevCol[i] % 0x100);
        biVis.bmiColor[i].rgbGreen = (byte)((lngEGARevCol[i] & 0xFF00) / 0x100);
        biVis.bmiColor[i].rgbRed = (byte)((lngEGARevCol[i] & 0xFF0000) / 0x10000);
        biPri.bmiColor[i].rgbBlue = (byte)(lngEGARevCol[i] % 0x100);
        biPri.bmiColor[i].rgbGreen = (byte)((lngEGARevCol[i] & 0xFF00) / 0x100);
        biPri.bmiColor[i].rgbRed = (byte)((lngEGARevCol[i] & 0xFF0000) / 0x10000);
      }
      //build arrays of bitmap data
      //Build function returns an error level value
      mBMPErrLvl = BuildBMPs(mVisData, mPriData, mRData.AllData, mStepDraw ? mDrawPos : -1, mDrawPos);
      //get pen status
      mCurrentPen = GetToolStatus();

      //create compatible DCs to use
      lngVisDC = CreateCompatibleDC(0);
      if (lngVisDC == 0)
      {
        throw new Exception("610, strErrSource, LoadResString(610)");
      }
      //get handles to dibsection (bitmaps)
      //Do
      hVisDIBSec = CreateDIBSection(lngVisDC, biVis, DIB_RGB_COLORS, lngVisAddr, 0, 0);
      if (hVisDIBSec == 0)
      {
        rtn = DeleteDC(lngVisDC);
        throw new Exception("610, strErrSource, LoadResString(610)");
      }

      //save old DIBsec, and select this object into the bitmaps
      hOldVisDIB = SelectObject(lngVisDC, hVisDIBSec);
      if (hOldVisDIB == 0)
      {
        rtn = DeleteDC(lngVisDC);
        rtn = DeleteObject(hVisDIBSec);
        throw new Exception("610, strErrSource, LoadResString(610)");
      }
      //create compatible DCs to use
      lngPriDC = CreateCompatibleDC(0);
      if (lngPriDC == 0)
      {
        throw new Exception("610, strErrSource, LoadResString(610)");
      }
      //get handles to dibsection (bitmaps)
      hPriDIBSec = CreateDIBSection(lngPriDC, biPri, DIB_RGB_COLORS, lngPriAddr, 0, 0);
      if (hPriDIBSec == 0)
      {
        rtn = DeleteDC(lngPriDC);
        throw new Exception("610, strErrSource, LoadResString(610)");
      }
      //save old dibsec, and select this object into the bitmaps
      hOldPriDIB = SelectObject(lngPriDC, hPriDIBSec);
      if (hOldPriDIB == 0)
      {
        rtn = DeleteDC(lngPriDC);
        rtn = DeleteObject(hPriDIBSec);
        throw new Exception("610, strErrSource, LoadResString(610)");
      }
      //set background color for visual image????
      rtn = SetBkColor(lngVisDC, 0xFFFFFF);
      //copy data into bitmaps
      //HandleRef hrVis, hrPri;
      //hrVis = new HandleRef(lngVisAddr);
      //CopyMemory(lngVisAddr, mVisData, 26880);
      //CopyMemory(lngPriAddr, mPriData, 26880);

      //set flag
      mPicBMPSet = true;
    }
    void temp()
    {
      /*
Friend Sub BuildPictures()
End Sub


public Property Let StepDraw(ByVal NewValue As Boolean)
  
  On Error GoTo ErrHandler
  
  //if not loaded,
  if (!Loaded) {
    //raise error
    throw new Exception("563, strErrSource, LoadResString(563)
    return;
  }
  
  //if a change,
  if (mStepDraw != NewValue) {
    //make the change,
    mStepDraw = NewValue
    //set flag to force redraw of picture
    mPicBMPSet = false
  }
return;

ErrHandler:
  //////Debug.Assert false
  Resume Next
}
public Property Get StepDraw() As Boolean
  
  On Error Resume Next
  
  //if not loaded,
  if (!Loaded) {
    //raise error
    throw new Exception("563, strErrSource, LoadResString(563)
    Exit Function
  }
  
  StepDraw = mStepDraw
}

public Sub Unload()

  //unload resource
  
  On Error GoTo ErrHandler
  
  Unload
  mIsDirty = false
  
  Dim rtn As Long
  
  //cleanup picture resources
  rtn = SelectObject(lngVisDC, hOldVisDIB)
  rtn = SelectObject(lngPriDC, hOldPriDIB)
  rtn = DeleteObject(hVisDIBSec)
  rtn = DeleteObject(hPriDIBSec)
  rtn = DeleteDC(lngVisDC)
  rtn = DeleteDC(lngPriDC)
  
  lngVisDC = 0
  lngPriDC = 0
  hOldVisDIB = 0
  hOldPriDIB = 0
  lngVisAddr = 0
  lngPriAddr = 0
  hVisDIBSec = 0
  hPriDIBSec = 0
  
  mPicBMPSet = false
Exit Sub

ErrHandler:
  //////Debug.Assert false
  Resume Next
End Sub


public Property Get VisualBMP() As Long
  //returns a device context to the bitmap image of the visual screenoutput
  Dim rtn As Long
  
  On Error Resume Next
  
  //if not loaded,
  if (!Loaded) {
    //raise error
    throw new Exception("563, strErrSource, LoadResString(563)
    Exit Function
  }
    
  //if pictures not built, or have changed,
  if (!mPicBMPSet) {
    //if an old picture is loaded,
    if (lngVisDC != 0) {
      //unload current picture resource
      rtn = SelectObject(lngVisDC, hOldVisDIB)
      rtn = SelectObject(lngPriDC, hOldPriDIB)
      rtn = DeleteDC(lngVisDC)
      rtn = DeleteDC(lngPriDC)
      rtn = DeleteObject(hVisDIBSec)
      rtn = DeleteObject(hPriDIBSec)
      lngVisDC = 0
      lngPriDC = 0
      hOldVisDIB = 0
      hOldPriDIB = 0
      lngVisAddr = 0
      lngPriAddr = 0
      hVisDIBSec = 0
      hPriDIBSec = 0
    }
    //load pictures to get correct pictures
    BuildPictures
    //if errors,
    if (Err.Number != 0) {
      //pass error along
      lngError = Err.Number
      strError = Err.Description
      strErrSrc = Err.Source
      
      On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
      Exit Sub
    }
  }
  
  VisualBMP = lngVisDC
}


public Property Get Number() As Byte
  Number = Number
}



Property Get AGIResource_AllData() As Byte()
  AGIResource_AllData = AllData
}
Sub AGIResource_Clear()
  Clear
End Sub


Property Let AGIResource_Data(ByVal Pos As Long, ByVal NewData As Byte)

  Data(Pos) = NewData
}

Property Get AGIResource_Data(ByVal Pos As Long) As Byte
  On Error GoTo ErrHandler
  
  AGIResource_Data = Data(Pos)
return;

ErrHandler:
  //pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
}

Property Get AGIResource_EORes() As Boolean

  AGIResource_EORes = EORes
}


Sub AGIResource_Export(ExportFile As String)
  Export ExportFile
End Sub


Function AGIResource_GetPos() As Long
  AGIResource_GetPos = GetPos
End Function



Property Get AGIResource_InGame() As Boolean
  AGIResource_InGame = InGame
}

Sub AGIResource_InsertData(NewData As Variant, Optional ByVal InsertPos As Long = -1&)

  On Error GoTo ErrHandler
  
  InsertData NewData, InsertPos
Exit Sub

ErrHandler:
  //pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub


Sub AGIResource_Load()
  Load
End Sub


Property Get AGIResource_Loaded() As Boolean
  AGIResource_Loaded = Loaded
}



Property Get AGIResource_Loc() As Long
  AGIResource_Loc = Loc
}


Sub AGIResource_NewResource(Optional ByVal Reset As Boolean = false)
  On Error GoTo ErrHandler
  
  NewResource Reset
Exit Sub

ErrHandler:
  //pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub

Property Get AGIResource_Number() As Byte
  AGIResource_Number = Number
}


Function AGIResource_ReadByte(Optional ByVal lngPos As Long = 65531) As Byte
  On Error GoTo ErrHandler
  
  AGIResource_ReadByte = ReadByte(lngPos)
Exit Function

ErrHandler:
  //pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Function



Function AGIResource_ReadWord(Optional ByVal lngPos As Long = -1&, Optional ByVal blnMSLS As Boolean = false) As Long
  On Error GoTo ErrHandler
  
  AGIResource_ReadWord = ReadWord(lngPos)
Exit Function

ErrHandler:
  //pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Function


Sub AGIResource_RemoveData(ByVal RemovePos As Long, Optional ByVal RemoveCount As Long = 1)

  On Error GoTo ErrHandler
  
  RemoveData RemovePos, RemoveCount
Exit Sub

ErrHandler:
  //pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub


Property Get AGIResource_ResFile() As String
  AGIResource_ResFile = ResFile
}


Property Get AGIResource_ResType() As AGIResType
  AGIResource_ResType = ResType
}


Sub AGIResource_SaveRes(Optional SaveFile As String)
  
End Sub



Sub AGIResource_SetData(NewData() As Byte)

  SetData NewData
End Sub

Sub AGIResource_SetPos(ByVal lngPos As Long)
  On Error GoTo ErrHandler
  
  SetPos lngPos
Exit Sub

ErrHandler:
  //pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub



Property Get AGIResource_Size() As Long
  AGIResource_Size = Size
}



Property Get AGIResource_SizeInVOL() As Long
  AGIResource_SizeInVOL = SizeInVol
}


Sub AGIResource_UnloadRes()

End Sub


Property Get AGIResource_Volume() As Long
  AGIResource_Volume = Volume
}

Sub agRes_Change()
  //set flag to indicate picture data does not match picture bmps
  mPicBMPSet = false
  //for picture only, changing resource sets dirty flag
  mIsDirty = true
End Sub


Sub Class_Initialize()
  Dim i As Integer
  
  strErrSource = "WINAGI.agiPicture"
  Set agRes = New AGIResource
  SetType rtPicture
  
  //create default picture with no commands
  NewResource
  Data(0) = 0xFF
  
  //initialize the DIBSection headers
  With biVis.bmiHeader
    .biSize = 40
    .biWidth = 160
    .biHeight = -168 //negative cuz bitmaps normally go down to up; we build pics going up to down...
    .biPlanes = 1
    .biBitCount = 8
    .biCompression = BI_RGB
    .biSizeImage = 26880
    .biClrUsed = 16 //need to define this because if set to undefined (0), files expect 2^8 entries
  End With
    
  With biPri.bmiHeader
    .biSize = 40
    .biWidth = 160
    .biHeight = -168
    .biPlanes = 1
    .biBitCount = 8
    .biCompression = BI_RGB
    .biSizeImage = 26880
    .biClrUsed = 16
  End With
  
  //default to entire image
  mDrawPos = -1
  
  mResID = "NewPicture"
  
  //default pribase is 48
  mPriBase = 48
End Sub



Sub Class_Terminate()
  //resources should be unloaded,
  //but just in case,
  //check in the termination section
  //to do the unload operation
  
  Dim rtn As Long
  
  if (Loaded) {
    //unload it
    Unload
  }
  
  Set agRes = Nothing
  
  //if picture bmps exist
  if (lngVisDC != 0 Or lngPriDC != 0) {
    //cleanup picture resources
    rtn = SelectObject(lngVisDC, hOldVisDIB)
    rtn = SelectObject(lngPriDC, hOldPriDIB)
    rtn = DeleteObject(hVisDIBSec)
    rtn = DeleteObject(hPriDIBSec)
    rtn = DeleteDC(lngVisDC)
    rtn = DeleteDC(lngPriDC)
    
    lngVisDC = 0
    lngPriDC = 0
    hOldVisDIB = 0
    hOldPriDIB = 0
    lngVisAddr = 0
    lngPriAddr = 0
    hVisDIBSec = 0
    hPriDIBSec = 0
  }
End Sub
      */
    }
  }
}