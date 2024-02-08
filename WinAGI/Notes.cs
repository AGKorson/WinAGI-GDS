using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Engine
{
    public class Notes : IEnumerable<Note>
    {
        //local variable to hold collection
        List<Note> mCol = new List<Note>();
        Sound mParent;
        Track mTParent;
        string strErrSource;
        public Note this[int index]
        {
            get
            { // validate
              //validate
                if (index < 0 || index > mCol.Count - 1) {
                    throw new Exception("index out of bounds");
                }
                return mCol[index];
            }
        }
        public void Clear()
        {
            //clear by setting collection to nothing
            mCol = new List<Note>();

            //if parent is assigned
            if (mParent != null) {
                //notify parent
                mParent.NoteChanged();
                mTParent.SetLengthDirty();
            }
        }
        public Note Add(int FreqDivisor, int Duration, byte Attenuation, int InsertPos = -1)
        {
            Note agNewNote = new Note
            {
                FreqDivisor = FreqDivisor,
                Duration = Duration,
                Attenuation = Attenuation,
                // copy parent objects, even if null
                mSndParent = this.mParent,
                mTrkParent = this.mTParent
            };
            //if no position passed (or position is past end)
            if (InsertPos < 0 || InsertPos > mCol.Count - 1) {
                //add it to end
                mCol.Add(agNewNote);
            }
            else {
                //add it before insert pos
                mCol.Insert(InsertPos, agNewNote);
            }
            //if parent is assigned
            if (mParent != null) {
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
            if (Index < 0 || Index > mCol.Count - 1) {
                throw new Exception("index out of bounds");
            }
            mCol.RemoveAt(Index);
            //if parent is assigned
            if (mParent != null) {
                //notify parent
                mParent.NoteChanged();
                mTParent.SetLengthDirty();
            }
        }
        public Notes()
        {    //creates the collection when this class is created
            mCol = new List<Note>();
            strErrSource = "AGINotes";
        }
        internal Notes(Sound parent, Track tparent)
        {    //creates the collection when this class is created
            mCol = new List<Note>();
            mParent = parent;
            mTParent = tparent;
            strErrSource = "AGINotes";
        }
        internal Notes Clone(Track cloneTparent)
        {
            // return a copy of this notes collection
            Notes CopyNotes = new Notes(mParent, cloneTparent);
            foreach (Note tmpNote in mCol) {
                CopyNotes.mCol.Add(new Note(mParent, mTParent)
                {
                    mAttenuation = tmpNote.mAttenuation,
                    mDuration = tmpNote.mDuration,
                    mFreqDiv = tmpNote.mFreqDiv
                });
                ;
            }
            return CopyNotes;
        }
        NoteEnum GetEnumerator()
        {
            return new NoteEnum(mCol);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        IEnumerator<Note> IEnumerable<Note>.GetEnumerator()
        {
            return (IEnumerator<Note>)GetEnumerator();
        }
    }
    internal class NoteEnum : IEnumerator<Note>
    {
        public List<Note> _notes;
        int position = -1;
        public NoteEnum(List<Note> list)
        {
            _notes = list;
        }
        object IEnumerator.Current => Current;
        public Note Current
        {
            get
            {
                try {
                    return _notes[position];
                }
                catch (IndexOutOfRangeException) {

                    throw new InvalidOperationException();
                }
            }
        }
        public bool MoveNext()
        {
            position++;
            return (position < _notes.Count);
        }
        public void Reset()
        {
            position = -1;
        }
        public void Dispose()
        {
            _notes = null;
        }
    }
}
