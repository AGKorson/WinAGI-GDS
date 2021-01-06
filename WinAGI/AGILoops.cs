using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;

namespace WinAGI
{
  public class AGILoops
  {
    List<AGILoop> mLoopCol;
    AGIView mParent;
    string strErrSource;
    public AGILoop Add(int Pos)
    {
      //Pos is position of this loop in the loop collection
      AGILoop agNewLoop;
      int i;
      //if too many loops or invalid pos
      if (mLoopCol.Count == MAX_LOOPS || Pos < 0)
      {
        //error - too many loops
        throw new Exception("537, strErrSource, LoadResString(537)");
      }
      //if no position is past end
      if (Pos >= mLoopCol.Count)
      {
        //set it to end
        Pos = (byte)mLoopCol.Count;
      }
      //if adding a loop in position 0-7
      //(which could push a mirror loop out of position
      if (Pos < 7 && mLoopCol.Count >= 7)
      {
        //if loop 7(index of 6) is a mirror
        if (mLoopCol[6].Mirrored)
        {
          //unmirror it
          mLoopCol[6].UnMirror();
        }
      }
      //create new loop object
      agNewLoop = new AGILoop(mParent)
      {
        //set index
        Index = Pos
      };
      //if no loops yet
      if (mLoopCol.Count == 0)
      {
        //just add it
        mLoopCol.Add(agNewLoop);
        //} else if ( Pos == 0) {
        //  //add new loop to front
        //  mLoopCol.Insert(0, agNewLoop);
      }
      else
      {
        //add it after the current loop with that number
        mLoopCol.Insert(Pos, agNewLoop);
      }
      //update index of all loops
      for (i = 0; i < mLoopCol.Count; i++)
      {
        mLoopCol[i].Index = (byte)i;
      }
      //set dirty flag
      mParent.IsDirty = true;
      //return the object created
      return agNewLoop;
    }
    public AGILoop this[int index]
    {
      get
      {
        //validate
        if (index < 0)
        {
          throw new Exception("index out of bounds");
        }
        if (index >= mLoopCol.Count)
        {
          throw new Exception("index out of bounds");
        }
        return mLoopCol[index];
      }
    }
    public int Count
    {
      get
      {
        return mLoopCol.Count;
      }
    }
    public void Remove(byte Index)
    {
      byte i;
      //if this is last loop
      if (mLoopCol.Count == 1)
      {
        //can't delete last loop
        throw new Exception("613, strErrSource, LoadResString(613)");
      }
      // if past the end
      if (Index >= mLoopCol.Count)
      {
        //invalid item
        throw new Exception("index out of bounds");
      }
      //if this loop is a mirrored loop
      if (mLoopCol[Index].Mirrored)
      {
        //clear mirrorpair for the mirror
        mLoopCol[mLoopCol[Index].MirrorLoop].MirrorPair = 0;
        //if the mirror is the primary loop
        if (mLoopCol[mLoopCol[Index].MirrorLoop].MirrorPair > 0)
        {
          //need to permanently flip that loop's cel data
          for (i = 0; i < mLoopCol[mLoopCol[Index].MirrorLoop].Cels.Count; i++)
          {
            mLoopCol[mLoopCol[Index].MirrorLoop].Cels[i].FlipCel();
          }
        }
      }
      mLoopCol.RemoveAt(Index);
      //ensure all loop indices are correct
      if (mLoopCol.Count > 0)
      {
        for (i = 0; i < mLoopCol.Count; i++)
        {
          mLoopCol[i].Index = (byte)i;
        }
      }
      if (mParent != null)
      {
        //tag as dirty
        mParent.IsDirty = true;
      }
    }
    public AGILoops()
    {
      //creates the collection when this class is created
      mLoopCol = new List<AGILoop>();
    }
    internal AGILoops(AGIView parent)
    {
      //create the collection when this class is created
      mLoopCol = new List<AGILoop>();
      // set parent
      mParent = parent;
    }
  }
}
