using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using System.Collections;

namespace WinAGI.Engine
{
    class Cels : IEnumerable<Cel>
    {
        //local variable to hold array of cels
        internal readonly List<Cel> mCelCol;
        View mParent;
        bool mSetMirror;
        //other
        string strErrSource;
        public Cel this[int index]
        {
            get
            {
                //validate
                if (index < 0) {
                    throw new Exception("index out of bounds");
                }
                if (index >= mCelCol.Count) {
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
        public Cel Add(int Pos, byte CelWidth = 1, byte CelHeight = 1, AGIColorIndex TransColor = AGIColorIndex.agBlack)
        {
            Cel agNewCel;
            int i;
            //if too many cels, or invalid pos
            if (mCelCol.Count == MAX_CELS || Pos < 0) {
                //error - too many cels
                throw new Exception("552, strErrSource, Replace(LoadResString(552), ARG1,");
            }
            //if no position is passed,
            //(or if past end of loops),
            if (Pos > mCelCol.Count) {
                //set it to end
                Pos = mCelCol.Count;
            }
            //create new cel object
            agNewCel = new Cel(mParent)
            {
                mWidth = CelWidth,
                mHeight = CelHeight,
                mTransColor = TransColor,
                mIndex = Pos
            };
            // set data array
            agNewCel.mCelData = new byte[agNewCel.mWidth, agNewCel.mHeight];
            //set mirror state
            agNewCel.SetMirror(mSetMirror);
            //if no cels yet
            if (mCelCol.Count == 0) {
                //just add it
                mCelCol.Add(agNewCel);
                //} else if (Pos == 0) {
                //  //add new loop to front
                //  mCelCol.Insert(Pos, agNewCel);
            }
            else {
                //add it after the current loop with that number
                mCelCol.Insert(Pos, agNewCel);
            }
            //if there is a parent view
            if (mParent is not null) {
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
            if (mCelCol.Count == 1) {
                //cant remove last cel
                throw new Exception(LoadResString(612));
            }
            //if past end
            if (index >= mCelCol.Count) {
                //invalid item
                throw new IndexOutOfRangeException("subscript out of range");
            }
            //remove cel
            mCelCol.RemoveAt(index);
            //if this was not last cel
            if (index < mCelCol.Count) {
                //ensure cels after this position have correct index
                for (i = index; i < mCelCol.Count; i++) {
                    mCelCol[index].Index = (byte)i;
                }
            }
            if (mParent is not null) {
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
            foreach (Cel tmpCel in mCelCol) {
                tmpCel.SetMirror(NewState);
            }
        }
        public View Parent
        { get { return mParent; } internal set { mParent = value; } }
        public Cels()
        {
            mCelCol = [];
            strErrSource = "WINAGI.agiCels";
        }
        internal Cels(View parent)
        {
            mCelCol = [];
            strErrSource = "WINAGI.agiCels";
            mParent = parent;
        }
        internal Cels Clone(View cloneparent)
        {
            Cels CopyCels = new(cloneparent);
            foreach (Cel tmpCel in mCelCol) {
                CopyCels.mCelCol.Add(tmpCel.Clone(cloneparent));
            }
            CopyCels.mSetMirror = mSetMirror;
            return CopyCels;
        }
        CelEnum GetEnumerator()
        {
            return new CelEnum(mCelCol);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<Cel> IEnumerable<Cel>.GetEnumerator()
        {
            return (IEnumerator<Cel>)GetEnumerator();
        }
    }
    internal class CelEnum : IEnumerator<Cel>
    {
        public List<Cel> _cels;
        int position = -1;
        public CelEnum(List<Cel> list)
        {
            _cels = list;
        }
        object IEnumerator.Current => Current;
        public Cel Current
        {
            get
            {
                try {
                    return _cels[position];
                }
                catch (IndexOutOfRangeException) {

                    throw new InvalidOperationException();
                }
            }
        }
        public bool MoveNext()
        {
            position++;
            return (position < _cels.Count);
        }
        public void Reset()
        {
            position = -1;
        }
        public void Dispose()
        {
            _cels = null;
        }
    }
}
