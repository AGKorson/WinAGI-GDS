using System;
using System.Collections;
using System.Collections.Generic;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents a collection of notes, usually as part 
    /// of an AGI Sound resource track.
    /// </summary>
    public class Notes : IEnumerable<Note> {
        #region Members
        List<Note> mCol = [];
        Sound mParent;
        Track mTParent;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new notes collection that is not part of an AGI Sound
        /// resource.
        /// </summary>
        public Notes() {
            mCol = [];
        }

        /// <summary>
        /// Creates a new notes collection that is being added to a newly
        /// created AGI Sound resource.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tparent"></param>
        internal Notes(Sound parent, Track tparent) {
            mCol = [];
            mParent = parent;
            mTParent = tparent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the note from this collection at the specified location.
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
        /// Gets the number of notes in this notes collection.
        /// </summary>
        public int Count {
            get {
                return mCol.Count;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Deletes all notes from this collection.
        /// </summary>
        public void Clear() {
            mCol = [];
            if (mParent is not null) {
                mParent.NoteChanged();
                mTParent.SetLengthDirty();
            }
        }

        /// <summary>
        /// Adds a new note to this colelction with the specified parameters
        /// at the specified location.
        /// </summary>
        /// <param name="FreqDivisor"></param>
        /// <param name="Duration"></param>
        /// <param name="Attenuation"></param>
        /// <param name="InsertPos"></param>
        /// <returns>A reference to the newly added note.</returns>
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
            if (InsertPos < 0 || InsertPos > mCol.Count - 1) {
                // add it to end
                mCol.Add(agNewNote);
            }
            else {
                // add it before insert pos
                mCol.Insert(InsertPos, agNewNote);
            }
            if (mParent is not null) {
                mParent.NoteChanged();
                mTParent.SetLengthDirty();
            }
            return agNewNote;
        }

        /// <summary>
        /// Removes the note at the specified location from this collection.
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
        /// Creates an exact copy of this Notes object.
        /// </summary>
        /// <param name="cloneTparent"></param>
        /// <returns>The Notes object this method creates.</returns>
        internal Notes Clone(Track cloneTparent) {
            Notes CopyNotes = new(mParent, cloneTparent);
            foreach (Note tmpNote in mCol) {
                CopyNotes.mCol.Add(new Note(tmpNote.mFreqDiv, tmpNote.mDuration, tmpNote.mAttenuation, mParent, mTParent) {
                });
            }
            return CopyNotes;
        }
        #endregion

        #region Enumeration
        NoteEnum GetEnumerator() {
            return new NoteEnum(mCol);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }

        IEnumerator<Note> IEnumerable<Note>.GetEnumerator() {
            return (IEnumerator<Note>)GetEnumerator();
        }
        /// <summary>
        /// Implements enumeration for the Notes class.
        /// </summary>
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
        #endregion
    }
}
