using System;
using System.Collections;
using System.Collections.Generic;

namespace WinAGI.Engine {
    public class Notes : IEnumerable<Note> {
        //local variable to hold collection
        List<Note> mCol = [];
        Sound mParent;
        Track mTParent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Note this[int index] {
            get {
                if (index < 0 || index > mCol.Count - 1) {
                    throw new IndexOutOfRangeException();
                }
                return mCol[index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear() {
            mCol = [];
            if (mParent is not null) {
                mParent.NoteChanged();
                mTParent.SetLengthDirty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FreqDivisor"></param>
        /// <param name="Duration"></param>
        /// <param name="Attenuation"></param>
        /// <param name="InsertPos"></param>
        /// <returns></returns>
        public Note Add(int freqdiv, int duration, byte attenuation, int InsertPos = -1) {
            if (freqdiv < 0 || freqdiv > 1023) {
                throw new ArgumentOutOfRangeException(nameof(freqdiv));
            }
            if (duration < 0 || duration > 0xFFFF) {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }
            if (attenuation > 15) {
                throw new ArgumentOutOfRangeException(nameof(attenuation));
            }
            Note agNewNote = new(freqdiv, duration, attenuation) {
                // copy parent objects, even if null
                mSndParent = mParent,
                mTrkParent = mTParent
            };
            // if no position passed (or position is past end)
            if (InsertPos < 0 || InsertPos > mCol.Count - 1) {
                //add it to end
                mCol.Add(agNewNote);
            }
            else {
                //add it before insert pos
                mCol.Insert(InsertPos, agNewNote);
            }
            if (mParent is not null) {
                mParent.NoteChanged();
                mTParent.SetLengthDirty();
            }
            return agNewNote;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count {
            get {
                return mCol.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Remove(int Index) {
            if (Index < 0 || Index > mCol.Count - 1) {
                throw new IndexOutOfRangeException();
            }
            mCol.RemoveAt(Index);
            if (mParent is not null) {
                mParent.NoteChanged();
                mTParent.SetLengthDirty();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Notes() {
            mCol = [];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tparent"></param>
        internal Notes(Sound parent, Track tparent) {
            mCol = [];
            mParent = parent;
            mTParent = tparent;
        }

        /// <summary>
        /// Returns a copy of this notes collection.
        /// </summary>
        /// <param name="cloneTparent"></param>
        /// <returns></returns>
        internal Notes Clone(Track cloneTparent) {
            Notes CopyNotes = new(mParent, cloneTparent);
            foreach (Note tmpNote in mCol) {
                CopyNotes.mCol.Add(new Note(mParent, mTParent) {
                    mAttenuation = tmpNote.mAttenuation,
                    mDuration = tmpNote.mDuration,
                    mFreqDiv = tmpNote.mFreqDiv
                });
            }
            return CopyNotes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        NoteEnum GetEnumerator() {
            return new NoteEnum(mCol);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }

        IEnumerator<Note> IEnumerable<Note>.GetEnumerator() {
            return (IEnumerator<Note>)GetEnumerator();
        }
    }
    internal class NoteEnum : IEnumerator<Note> {
        public List<Note> _notes;
        int position = -1;
        public NoteEnum(List<Note> list) {
            _notes = list;
        }
        object IEnumerator.Current => Current;
        public Note Current {
            get {
                try {
                    return _notes[position];
                }
                catch (IndexOutOfRangeException) {
                    throw new InvalidOperationException();
                }
            }
        }
        public bool MoveNext() {
            position++;
            return (position < _notes.Count);
        }
        public void Reset() {
            position = -1;
        }
        public void Dispose() {
            _notes = null;
        }
    }
}
