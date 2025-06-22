using System;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents an AGI sound track, containing all notes and track properties.
    /// </summary>
    public class Track {
        #region Members
        Notes mNotes;
        bool mMuted;
        byte mInstrument;
        bool mVisible;
        internal Sound mParent;
        bool mLengthChanged;
        double mLength;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new Track object that is not associated with a game.
        /// </summary>
        public Track() {
            mNotes = [];
            mLengthChanged = true;
            mVisible = true;
            mInstrument = 80;
        }

        /// <summary>
        /// Internal constuctor when initializing or cloning a sound resource that is in a game. 
        /// </summary>
        /// <param name="parent"></param>
        internal Track(Sound parent) {
            mNotes = new Notes(parent, this);
            mLengthChanged = true;
            mVisible = true;
            mInstrument = 80;
            mParent = parent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the collection of notes in this track.
        /// </summary>
        public Notes Notes {
            get {
                return mNotes;
            }
            internal set {
                mNotes = value;
            }
        }

        /// <summary>
        /// Get the specified note from this track. The index is zero-based.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Note this[int index] {
            get {
                if (index < 0 || index > mNotes.Count - 1) {
                    throw new IndexOutOfRangeException();
                }
                return mNotes[index];
            }
        }

        /// <summary>
        /// Gets or sets the MIDI instrument value to use when the sound is played as
        /// a MIDI stream. This property is only applicable to PC/PCjr sounds.
        /// </summary>
        public byte Instrument {
            get {
                return mInstrument;
            }
            set {
                if (value >= 128) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (mInstrument != value) {
                    mInstrument = value;
                    mParent.TrackChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the muted state for this track. When muted, the track will not
        /// be included when the sound is played as a MIDI stream.  This property is
        /// only applicable to PC/PCjr sounds.
        /// </summary>
        public bool Muted {
            get {
                return mMuted;
            }
            set {
                if (mMuted != value) {
                    mMuted = value;
                    mParent.TrackChanged();
                }
            }
        }

        /// <summary>
        /// Gets the length of this track, in seconds.
        /// </summary>
        public double Length {
            get {
                int i, lngTickCount = 0;
                if (mLengthChanged) {
                    for (i = 0; i <= mNotes.Count - 1; i++) {
                        lngTickCount += mNotes[i].Duration;
                    }
                    mLength = (double)lngTickCount / 60;
                    mLengthChanged = false;
                }
                return mLength;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets or sets the visible property of this track. This property is only 
        /// applicable to PC/PCjr sounds, and is only used by WinAGI when editing
        /// sounds.
        /// </summary>
        public bool Visible {
            get {
                return mVisible;
            }
            set {
                if (mVisible != value) {
                    mVisible = value;
                    mParent.TrackChanged(false);
                }
            }
        }

        /// <summary>
        /// This method is used by parent sound to let this track know that sound
        /// length needs to be recalculated.
        /// </summary>
        internal void SetLengthChanged() {
            mLengthChanged = true;
        }

        /// <summary>
        /// Creates an exact copy of this Tracks object.
        /// </summary>
        /// <param name="cloneparent"></param>
        /// <returns>The Tracks object this method creates.</returns>
        public Track Clone(Sound cloneparent) {
            Track CopyTrack = new(cloneparent) {
                mMuted = mMuted,
                mInstrument = mInstrument,
                mVisible = mVisible,
                mLengthChanged = mLengthChanged,
                mLength = mLength
            };
            CopyTrack.mNotes = mNotes.Clone(cloneparent, CopyTrack);
            return CopyTrack;
        }

        /// <summary>
        /// Copies track data from SourceTrack into this track.
        /// </summary>
        public void CloneFrom(Track SourceTrack) {
            mMuted = SourceTrack.mMuted;
            mInstrument = SourceTrack.mInstrument;
            mVisible = SourceTrack.mVisible;
            mLengthChanged = SourceTrack.mLengthChanged;
            mLength = SourceTrack.mLength;
            mNotes.CloneFrom(SourceTrack.mNotes);
        }

        /// <summary>
        /// Return the time position (in AGI Ticks) of a note in this track.
        /// </summary>
        public int TimePos(int note) {
            int pos = 0;
            for (int i = 0; i < mNotes.Count; i++) {
                if (i == note) {
                    break;
                }
                pos += mNotes[i].Duration;
            }
            return pos;
        }
        #endregion
    }
}
