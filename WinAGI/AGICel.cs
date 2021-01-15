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
  public class AGICel
  {
    internal byte mWidth;
    internal byte mHeight;
    internal AGIColors mTransColor;
    internal byte[,] mCelData;
    internal int mIndex;
    internal bool blnCelBMPSet;  //means cel bitmap needs to be rebuilt
    internal bool mTransparency;
    internal bool mCelChanged;   // means cel data has change? who cares?
    internal Bitmap mCelBMP;
    //mSetMirror is true if cel is supposed to show the mirror
    bool mSetMirror;
    //mMirror is true if the cel IS showing the mirror
    bool mMirrored;
    string strErrSource;
    AGIView mParent;
    public byte[,] AllCelData
    {
      get
      {
        //returns the entire array of cel data
        //flips the data if the cel is mirrored, and
        //not the primary loop
        byte[,] tmpData;
        int i, j;
        if (mSetMirror)
        {
          //need to flip the data
          tmpData = new byte[mWidth, mHeight];
          for (i = 0; i < mWidth; i++)
          {
            for (j = 0; j < mHeight; j++)
            {
              //copy backwards
              tmpData[mWidth - 1 - i, j] = mCelData[i, j];
            }
          }
          //return temp data
          return tmpData;
        }
        else
        {
          //fine to return as is
          return mCelData;
        }
      }
      set
      {
        //this method allows the entire cel data
        //to be set as an array
        //validate dimensions match height/width
        if (value.GetUpperBound(0) != mWidth - 1)
        {
          //invalid data
          throw new Exception("614, strErrSource, LoadResString(614)");
        }
        if (value.GetUpperBound(1) != mHeight - 1)
        {
          //invalid data
          throw new Exception("614, strErrSource, LoadResString(614)");
        }
        //set the celdata
        mCelData = value;
        //if there is a parent object
        if (mParent != null)
        {
          //set dirty flag
          mParent.IsDirty = true;
        }
        //note change
        mCelChanged = true;
      }
    }
    public byte this[byte xPos, byte yPos] //CelData
    {
      get
      {
        //returns the cel data for the pixel at xPos, yPos
        //verify within bounds
        if (xPos > mWidth - 1)
        {
          throw new Exception("index out of bounds");
        }
        if (yPos > mHeight - 1)
        {
          throw new Exception("index out of bounds");
        }
        //if cel is in mirror state
        if (mSetMirror)
        {
          //reverse x direction
          return mCelData[mWidth - 1 - xPos, yPos];
        }
        else
        {
          //get pixel Value
          return mCelData[xPos, yPos];
        }
      }
      set
      {
        //set the cel data for this position
        //verify within bounds
        if (xPos >= mWidth)
        {
          throw new Exception("index out of bounds");
        }

        if (yPos >= mHeight)
        {
          throw new Exception("index out of bounds");
        }
        //if cel is in mirror state
        if (mSetMirror)
        {
          //reverse x direction
          mCelData[mWidth - 1 - xPos, yPos] = value;
        }
        else
        {
          //write pixel Value
          mCelData[xPos, yPos] = value;
        }
        //note change
        mCelChanged = true;
        //if there is a parent object
        if (mParent != null)
        {
          //set dirty flag
          mParent.IsDirty = true;
        }
      }
    }
    void ClearBMP()
    {
      mCelBMP = null;
      //set flag
      blnCelBMPSet = false;
    }
    public bool Transparency
    { get { return mTransparency; }
      set
      {
        if (mTransparency != value)
        {
          mTransparency = value;
          mCelChanged = true;
          if (mParent != null)
          {
            //set dirty flag
            mParent.IsDirty = true;
          }
        }
      }
    }
    internal AGICel Clone(AGIView cloneparent)
    {
      AGICel CopyCel = new AGICel(cloneparent);
      CopyCel.mWidth = mWidth;
      CopyCel.mHeight = mHeight;
      CopyCel.mTransColor = mTransColor;
      CopyCel.mCelData = mCelData;
      CopyCel.mIndex = mIndex;
      CopyCel.mSetMirror = mSetMirror;
      CopyCel.mMirrored = mMirrored;
      CopyCel.blnCelBMPSet = blnCelBMPSet;
      CopyCel.mTransparency = mTransparency;
      CopyCel.mCelChanged = mCelChanged;   // means cel data has change? who cares?

      if (mCelBMP == null) {
        CopyCel.mCelBMP = null;
      } else {
        // make a new bitmap 
        CopyCel.mCelBMP = (Bitmap)mCelBMP.Clone();
      }
      return CopyCel;
    }
    public Bitmap CelBMP
      {
        get
        {
          int i, j;
          byte[] mCelBData;

          //if cel bitmap is already assigned
          if (blnCelBMPSet)
          {
            //if cel bitmap is in correct state AND not changed
            if ((mSetMirror == mMirrored) && (!mCelChanged))
            {
              //exit; cel bitmap is correct
              return mCelBMP;
            }
            //rebuild the bitmap; first clear it 
            ClearBMP();
          }
          //create new visual picture bitmap
          mCelBMP = new Bitmap(mWidth, mHeight, PixelFormat.Format8bppIndexed);
          //modify color palette to match current AGI palette
          ColorPalette ncp = mCelBMP.Palette;
          for (i = 0; i < 16; i++)
          {
            ncp.Entries[i] = Color.FromArgb((mTransparency && i == (int)mTransColor) ? 0 : 255,
            EGAColor[i].R,
            EGAColor[i].G,
            EGAColor[i].B);
          }
          // set the new palette
          mCelBMP.Palette = ncp;
          // set boundary rectangle
          var BoundsRect = new Rectangle(0, 0, mWidth, mHeight);
          // create access point for bitmap data
          BitmapData bmpCelData = mCelBMP.LockBits(BoundsRect, ImageLockMode.WriteOnly, mCelBMP.PixelFormat);
          IntPtr ptrVis = bmpCelData.Scan0;
          //now we can create our custom data arrays
          // array size is determined by stride (bytes per row) and height
          mCelBData = new byte[bmpCelData.Stride * mHeight];
          //set cel mirror state to desired Value (determined by mSetMirror)
          mMirrored = mSetMirror;
          for (i = 0; i < mWidth; i++)
          {
            for (j = 0; j < mHeight; j++)
            { //if showing mirrored cel
              if (mMirrored)
              {
                //set cel data backwards
                mCelBData[mWidth - i - 1 + j * bmpCelData.Stride] = (byte)mCelData[i, j];
              }
              else
              {
                //set cel data forwards
                mCelBData[i + j * bmpCelData.Stride] = (byte)mCelData[i, j];
              }
            }
          }
          // copy the picture data to the bitmaps
          Marshal.Copy(mCelBData, 0, ptrVis, bmpCelData.Stride * mHeight);
          mCelBMP.UnlockBits(bmpCelData);
          //set flag
          blnCelBMPSet = true;
          return mCelBMP;
        }
      }
    public void CopyCel(AGICel SourceCel)
    {
      //copies the source cel into this cel
      int i, j;
      AGIColors tmpColor;

      mWidth = SourceCel.Width;
      mHeight = SourceCel.Height;
      mTransColor = SourceCel.TransColor;
      AllCelData = SourceCel.AllCelData;
      //if this cel is supposed to be mirrored
      if (mSetMirror)
      {
        //need to transpose data
        for (i = 0; i < mWidth / 2; i++)
        {
          for (j = 0; j < mHeight; j++)
          {    //swap horizontally
            tmpColor = (AGIColors)mCelData[mWidth - 1 - i, j];
            mCelData[mWidth - 1 - i, j] = mCelData[i, j];
            mCelData[i, j] = (byte)tmpColor;
          }
        }
      }
      //if there is a parent object
      if (mParent != null)
      {
        mParent.IsDirty = true;
      }
      //note change
      mCelChanged = true;
    }
    internal void FlipCel()
    {
      //this is called to flip cel data
      //to support loop changes
      //when a mirrored pair has its secondary (the
      //loop with the negative mirror pair) either
      //deleted, unmirrored, or set to another mirror
      //the cels original configuration stays correct

      //if the primary loop is deleted, unmirrored, or set
      //to another mirror, then the cels need to be flipped
      //so the remaining secondary cel will get the data
      //in the correct format
      int i, j;
      byte[,] tmpCelData;
      tmpCelData = new byte[mWidth, mHeight];
      for (i = 0; i < mWidth; i++)
      {
        for (j = 0; j < mHeight; j++)
        {
          tmpCelData[mWidth - 1 - i, j] = mCelData[i, j];
        }
      }
      // save the updated data
      mCelData = tmpCelData;
      //note change
      mCelChanged = true;
      //if there is a parent object
      if (mParent != null)
      {
        //set dirty flag
        mParent.IsDirty = true;
      }
    }
    public byte Height
    {
      get { return mHeight; }
      set
      {
        //adjusts height of cel
        int i, j;
        byte[,] tmpData;
        //must be non-zero
        if (value == 0)
        {
          throw new Exception("532, strErrSource, LoadResString(532)");
        }
        if (value > MAX_CEL_HEIGHT)
        {
          throw new Exception("532, strErrSource, LoadResString(532)");
        }
        //if changed
        if (mHeight != value)
        {
          //can't easily resize multidimensional arrays, so 
          // make a new array, copy desired data over
          tmpData = new byte[mWidth, value];
          // copy data to temp array
          for (i = 0; i < mWidth; i++)
          {
            for (j = 0; j < value; j++)
            {
              // if array grew, add transparent pixels
              if (j >= mHeight)
              {
                mCelData[i, j] = (byte)mTransColor;
              }
              else
              {
                tmpData[i, j] = mCelData[i, j];
              }
            }
          }
          //if adding to height
          if (value > mHeight)
          {
            //set new rows to transparent color
            for (i = 0; i < mWidth; i++)
            {
              for (j = mHeight; j < value; j++)
              {
                mCelData[i, j] = (byte)mTransColor;
              }
            }
          }
          // save new data
          mCelData = tmpData;
          //set new height
          mHeight = value;
          //if there is a parent object
          if (mParent != null)
          {
            //set dirty flag
            mParent.IsDirty = true;
          }
          //note change
          mCelChanged = true;
        }
      }
    }
    public void Clear()
    {
      //this resets the cel to a one pixel cel,
      //with no data, and black as transcolor
      mHeight = 1;
      mWidth = 1;
      mTransColor = AGIColors.agBlack;
      mCelData = new byte[1, 1];
      //if the cel has a bitmap set,
      if (blnCelBMPSet)
      {
        ClearBMP();
        //set flag indicating no bitmap
        blnCelBMPSet = false;
      }

      //if there is a parent object
      if (mParent != null)
      {
        mParent.IsDirty = true;
      }
      mCelChanged = true;
    }
    public int Index
    {
      get
      {
        return mIndex;
      }
      internal set
      {
        //sets the index number for this cel
        mIndex = value;
      }
    }
    public byte Width
    {
      get { return mWidth; }
      set
      {
        //adjusts width of cel
        int i, j;
        byte[,] tmpData;
        //width must be non zero
        if (value == 0)
        {
          throw new Exception("533, strErrSource, LoadResString(533)");
        }
        //width must not exceed max Value
        if (value > MAX_CEL_WIDTH)
        {
          throw new Exception("533, strErrSource, LoadResString(533)");
        }
        //if changed,
        if (mWidth != value)
        {
          //can't easily resize multidimensional arrays, so 
          // make a new array, copy desired data over
          tmpData = new byte[value, mHeight];
          for (i = 0; i < mWidth; i++)
          {
            for (j = 0; j < mHeight; j++)
            {
              //if past oldwidth
              if (i >= mWidth)
              {
                //add a transparent color pixel
                tmpData[i, j] = (byte)mTransColor;
              }
              else
              {
                //add pixel from celdata
                tmpData[i, j] = mCelData[i, j];
              }
            }
          }
          // save new data
          mCelData = tmpData;
          //set new width
          mWidth = value;
          //if there is a parent object
          if (mParent != null)
          {
            //set dirty flag
            mParent.IsDirty = true;
          }
          //note change
          mCelChanged = true;
        }
      }
    }
    internal void SetMirror(bool blnNew)
    {
      mSetMirror = blnNew;
    }
    public AGIColors TransColor
    {
      get { return mTransColor; }
      set
      {
        //ensure a valid range is passed,
        if (value < 0 || (byte)value > 15)
        {
          //error
          throw new Exception("556, strErrSource, LoadResString(556)");
        }
        //if changed,
        if (value != mTransColor)
        {
          //change it
          mTransColor = value;
          mCelChanged = true;
          if (mParent != null)
          {
            //set dirty flag
            mParent.IsDirty = true;
          }
        }
      }
    }
    public AGICel()
    {
      strErrSource = "WINAGI.AGICel";
      mCelData = new byte[1, 1];
      mWidth = 1;
      mHeight = 1;
    }
    internal AGICel(AGIView parent)
    {
      strErrSource = "WINAGI.AGICel";
      mCelData = new byte[1, 1];
      mWidth = 1;
      mHeight = 1;
      mParent = parent;
    }
  }
}