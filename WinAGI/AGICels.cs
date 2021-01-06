using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;

namespace WinAGI
{
  class AGICels
  {
    //local variable to hold array of cels
    internal readonly List<AGICel> mCelCol;
    AGIView mParent;
    bool mSetMirror;
    //other
    string strErrSource;
    public AGICel this[int index]
    {
      get
      {
        //validate
        if (index < 0)
        {
          throw new Exception("index out of bounds");
        }
        if (index >= mCelCol.Count)
        {
          throw new Exception("index out of bounds");
        }
        // mirror status is already set; just
        // pass the desired cel
        return mCelCol[index];
      }
    }
    public int Count
    {
      get
      {
        //return number of cels
        return mCelCol.Count;
      }
    }
    public AGICel Add(int Pos, byte CelWidth = 1, byte CelHeight = 1, AGIColors TransColor = AGIColors.agBlack)
    {
      AGICel agNewCel;
      //if too many cels, or invalid pos
      if (mCelCol.Count == MAX_CELS || Pos < 0)
      {
        //error - too many cels
        throw new Exception("552, strErrSource, Replace(LoadResString(552), ARG1,");
        }
      //set the properties passed into the method
      agNewCel = new AGICel(mParent)
      {
        mWidth = CelWidth,
        mHeight = CelHeight,
        mTransColor = TransColor
      };
      // set data array
      agNewCel.mCelData = new byte[agNewCel.mWidth, agNewCel.mHeight];
      //set mirror state
      agNewCel.SetMirror(mSetMirror);
      //if no position is passed,
      //(or if past end of loops),
      if (Pos > mCelCol.Count)
      {
        //set it to end
        Pos = (byte)mCelCol.Count;
      }
      //if no cels yet
      if (mCelCol.Count == 0)
      {
        //just add it
        mCelCol.Add(agNewCel);
        //} else if (Pos == 0) {
        //  //add new loop to front
        //  mCelCol.Insert(Pos, agNewCel);
        }
      else
      {
        //add it after the current loop with that number
        mCelCol.Insert(Pos, agNewCel);
      }
      //if there is a parent view
      if (mParent != null)
      {
        //tag as dirty
        mParent.IsDirty = true;
      }
      //return the object created
      return agNewCel;
    }
    public void Remove(int index)
    {
      int i;
      //if this is last cel
      if (mCelCol.Count == 1)
      {
        //cant remove last cel
        throw new Exception("612, strErrSource, LoadResString(612)");
      }
      //if past end
      if (index >= mCelCol.Count)
      {
        //invalid item
        throw new Exception("9, strErrSource, subscript out of range");
      }
      //remove cel
      mCelCol.RemoveAt(index);
      //if this was not last cel
      if (index < mCelCol.Count)
      {
        //ensure cels after this position have correct index
        for (i = index; i < mCelCol.Count; i++)
        {
          mCelCol[index].Index = (byte)i;
        }
      }
      if (mParent != null)
      {
        //tag as dirty
        mParent.IsDirty = true;
      }
    }
    internal void SetMirror(bool NewState)
    {
      //this method is called just before the cels collection
      //is referenced by a mirrored loop
      //it is used to force the celbmp functions to
      //flip cel bitmaps and to flip cel data
      mSetMirror = NewState;
      foreach (AGICel tmpCel in mCelCol)
      {
        tmpCel.SetMirror(NewState);
      }
    }
    internal void SetParent(AGIView NewParent)
    {
      mParent = NewParent;
    }
    public AGICels()
    {
      mCelCol = new List<AGICel>();
      strErrSource = "WINAGI.agiCels";
    }
    internal AGICels(AGIView parent)
    {
      mCelCol = new List<AGICel>();
      strErrSource = "WINAGI.agiCels";
      mParent = parent;
    }
  }
}
