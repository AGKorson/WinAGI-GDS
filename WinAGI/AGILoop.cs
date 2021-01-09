using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAGI;
using static WinAGI.WinAGI;

namespace WinAGI
{
  public class AGILoop
  {
    AGICels mCelCol;
    int mMirrorPair;
    internal int mIndex;
    AGIView mParent;
    //other
    string strErrSource;
    internal AGICels Cels
    {
      get
      {
        //set mirror flag
        mCelCol.SetMirror(mMirrorPair < 0);

        //return the cels collection
        return mCelCol;
      }
      set
      {
        //sets the cels collection
        int i;
        mCelCol = value;
        //if mirrored
        if (mMirrorPair != 0)
        {
          //find mirror pair
          for (i = 0; i < mParent.mLoopCol.Count; i++)
          {  //if sum of mirror pairs is zero
            if (mParent.mLoopCol[(byte)i].MirrorPair + mMirrorPair == 0)
            {
              //is the cels collection already set to this object?
              if (mParent.mLoopCol[(byte)i].Cels == value)
              {
                //need to exit to avoid recursion
                return;
              }
              //set the mirrored loops cels
              mParent.mLoopCol[(byte)i].Cels = value;
              return;
            }
          }
        }
      }
    }
    public void CopyLoop(AGILoop SourceLoop)
    {
      //copies the source loop into this loop
      //if this is mirrored, and the primary loop
      if (mMirrorPair > 0)
      {
        //call unmirror for the secondary loop
        //so it will get a correct copy of cels
        mParent.mLoopCol[MirrorLoop].UnMirror();
      }
      else if (mMirrorPair < 0)
      {
        //this is a secondary loop;
        //only need to reset mirror status
        //because copy function will create new cel collection
        mParent.mLoopCol[MirrorLoop].MirrorPair = 0;
        mMirrorPair = 0;
      }
      //now copy source loop cels
      this.mCelCol = SourceLoop.Cels;
      // parent, index and mirror status go unchanged
      //if there is a parent object
      if (mParent != null)
      {
        //set dirty flag
        mParent.IsDirty = true;
      }
    }
    public AGICel this [int index]
    {
      get
      {
        return Cels[index];
      }
    }
    public int Index
    {
      get
      {
        return mIndex;
      }
      internal set
      {
        // validate
        if (mIndex < 0 || mIndex > MAX_LOOPS)
        {
          throw new Exception("index out of bounds");
        }
        mIndex = value;
      }
    }
    public bool Mirrored
    {
      get
      {
        //if this loop is part of a mirror pair
        //then it is mirrored
        return (mMirrorPair != 0);
      }
    }
    public byte MirrorLoop
    {
      get
      {
        //return the mirror loop
        //by finding the other loop that has this mirror pair Value
        byte i;
        //if not mirrored
        if (mMirrorPair == 0)
        {
          //raise error
          throw new Exception("611, strErrSource, LoadResString(611)");
        }

        //step through all loops in the loop collection
        for (i = 0; i < mParent.mLoopCol.Count; i++) {
          //if mirror pair values equal zero
          if (mParent.mLoopCol[i].MirrorPair + mMirrorPair == 0)
          {
            //this is the loop
            return i;
          }
        }
        //should never get here
        throw new Exception("611, strErrSource, LoadResString(611)");
      }
    }
    internal int MirrorPair
    {
      get
      {
        return mMirrorPair;
      }
      set
      {
        mMirrorPair = value;

      }
    }
    public void UnMirror()
    {
      //if the loop is mirrored,
      //this method clears it
      //NOTE: unmirroring is handled by the
      //secondary loop; if the loop that
      //calls this function is the primary loop,
      //this function passes the call to the
      //secondary loop for processing
      byte i;
      AGICels tmpCels;
      if (mMirrorPair == 0)
      {
        return;
      }
      //if this is the primary loop
      if (mMirrorPair > 0)
      {
        //unmirror other loop
        mParent.mLoopCol[MirrorLoop].UnMirror();
        //exit
        return;
      }
      //this is the secondary loop;
      //need to create new cel collection
      //and copy cel data

      //create temporary collection of cels
      tmpCels = new AGICels(mParent);
      //copy cels from current cel collection
      for (i = 0; i < mCelCol.Count; i++)
      {
        tmpCels.Add(i, mCelCol[i].Width, mCelCol[i].Height, mCelCol[i].TransColor);
        //access cels through parent so mirror status is set properly
        tmpCels[i].AllCelData = mParent.mLoopCol[mIndex].Cels[i].AllCelData;
      }
      //set cel collectionto new cel col
      mCelCol = tmpCels;
      //clear mirror properties
      mParent.mLoopCol[MirrorLoop].MirrorPair = 0;
      mMirrorPair = 0;
      //if there is a parent object
      if (mParent != null)
      {
        //set dirty flag
        mParent.IsDirty = true;
      }
    }
    public AGILoop()
    {
      //initialize cel collection object
      mCelCol = new AGICels();
      strErrSource = "WINAGI.AGILoop";
    }
    internal AGILoop(AGIView parent)
    {
      //initialize cel collection object
      mCelCol = new AGICels(parent);
      strErrSource = "WINAGI.AGILoop";
      mParent = parent;
    }
  }
}
