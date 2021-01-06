using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  public class AGINotes
  {
    //local variable to hold collection
    List<AGINote> mCol = new List<AGINote>();
    AGISound mParent;
    AGITrack mTParent;
    string strErrSource;
    public AGINote this[int index]
    { 
      get 
      { // validate
        //validate
        if (index < 0 || index > mCol.Count - 1)
        {
          throw new Exception("index out of bounds");
        }
        return mCol[index]; 
      } 
    }
    public void Clear()
    {
      //clear by setting collection to nothing
      mCol = new List<AGINote>();

      //if parent is assigned
      if (mParent != null)
      {
        //notify parent
        mParent.NoteChanged();
        mTParent.SetLengthDirty();
      }
    }
    public AGINote Add(int FreqDivisor, int Duration, byte Attenuation, int InsertPos = -1)
    {
      AGINote agNewNote = new AGINote
      {
        FreqDivisor = FreqDivisor,
        Duration = Duration,
        Attenuation = Attenuation,
        // copy parent objects, even if null
        mSndParent = this.mParent,
        mTrkParent = this.mTParent
      };
      //if no position passed (or position is past end)
      if (InsertPos < 0 || InsertPos > mCol.Count - 1)
      {
        //add it to end
        mCol.Add(agNewNote);
      }
      else
      {
        //add it before insert pos
        mCol.Insert(InsertPos, agNewNote);
      }
      //if parent is assigned
      if (mParent != null)
      {
        //notify parent
        mParent.NoteChanged();
        mTParent.SetLengthDirty();
      }
      //return the object created
      return agNewNote;
    }
    public int Count
    {
      get
      {
        return mCol.Count;
      }
    }
    public void Remove(int Index)
    {
      //validate
      if (Index < 0 || Index > mCol.Count - 1)
      {
        throw new Exception("index out of bounds");
      }
      mCol.RemoveAt(Index);
      //if parent is assigned
      if (mParent != null)
      {
        //notify parent
        mParent.NoteChanged();
        mTParent.SetLengthDirty();
      }
    }
    public AGINotes()
    {    //creates the collection when this class is created
      mCol = new List<AGINote>();
      strErrSource = "AGINotes";
    }
    internal AGINotes(AGISound parent, AGITrack tparent)
    {    //creates the collection when this class is created
      mCol = new List<AGINote>();
      mParent = parent;
      mTParent = tparent; 
      strErrSource = "AGINotes";
    }
  }
}
