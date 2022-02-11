using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;

namespace WinAGI.Engine
{
  public class Loops : IEnumerable<Loop>
  {
    List<Loop> mLoopCol;
    View mParent;
    string strErrSource;
    public Loop this[int index]
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
    public Loop Add(int Pos)
    {
      //Pos is position of this loop in the loop collection
      Loop agNewLoop;
      int i;
      //if too many loops or invalid pos
      if (mLoopCol.Count == MAX_LOOPS || Pos < 0)
      {
        //error - too many loops

        Exception e = new(LoadResString(537))
        {
          HResult = 537
        };
        throw e;
      }
      //if no position is past end
      if (Pos > mLoopCol.Count)
      {
        //set it to end
        Pos = mLoopCol.Count;
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
      agNewLoop = new Loop(mParent)
      {
        //set index
        mIndex = Pos
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
      //if there is a parent view
      if (mParent != null)
      {
        //set dirty flag
        mParent.IsDirty = true;
      }
      //return the object created
      return agNewLoop;
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

        Exception e = new(LoadResString(613))
        {
          HResult = 613
        };
        throw e;
      }
      // if past the end
      if (Index >= mLoopCol.Count)
      {
        //invalid item
        throw new IndexOutOfRangeException("index out of bounds");
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

    internal Loops Clone(View cloneparent)
    {
      // returns a copy of this loop collection
      Loops CopyLoops = new Loops(cloneparent);
      foreach (Loop tmpLoop in mLoopCol) {
        CopyLoops.mLoopCol.Add(tmpLoop.Clone(cloneparent));
      }
      return CopyLoops;
    }
    public Loops()
    {
      //creates the collection when this class is created
      mLoopCol = new List<Loop>();
    }
    public View Parent
    { get { return mParent; } internal set { mParent = value; } }
    internal Loops(View parent)
    {
      //create the collection when this class is created
      mLoopCol = new List<Loop>();
      // set parent
      mParent = parent;
    }
    LoopEnum GetEnumerator()
    {
      return new LoopEnum(mLoopCol);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }
    IEnumerator<Loop> IEnumerable<Loop>.GetEnumerator()
    {
      return (IEnumerator<Loop>)GetEnumerator();
    }
  }
  internal class LoopEnum : IEnumerator<Loop>
  {
    public List<Loop> _loops;
    int position = -1;
    public LoopEnum(List<Loop> list)
    {
      _loops = list;
    }
    object IEnumerator.Current => Current;
    public Loop Current
    { get
      {
        try
        {
          return _loops[position];
        }
        catch (IndexOutOfRangeException)
        {

          throw new InvalidOperationException();
        }
      }
    }
    public bool MoveNext()
    {
      position++;
      return (position < _loops.Count);
    }
    public void Reset()
    {
      position = -1;
    }
    public void Dispose()
    {
      _loops = null;
    }
  }
}
