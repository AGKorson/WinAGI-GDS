using System;

namespace WinAGI.Engine {
    public class Track {
        Notes mNotes;
        bool mMuted;
        byte mInstrument;
        bool mVisible;
        internal Sound mParent;
        bool mLengthDirty;
        double mLength;

        /// <summary>
        /// Initializes a new Track object that is not associated with a game.
        /// </summary>
        public Track() {
            mNotes = [];
            mLengthDirty = true;
            mVisible = true;
            mInstrument = 80;
        }

        /// <summary>
        /// Internal constuctor when initializing or cloning a sound resource that is in a game. 
        /// </summary>
        /// <param name="parent"></param>
        internal Track(Sound parent) {
            mNotes = new Notes(parent, this);
            mLengthDirty = true;
            mVisible = true;
            mInstrument = 80;
            mParent = parent;
        }
        #region Properties
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
                //returns the length of this track, in seconds
                int i, lngTickCount = 0;
                //if length has changed,
                if (mLengthDirty) {
                    for (i = 0; i <= mNotes.Count - 1; i++) {
                        lngTickCount += mNotes[i].Duration;
                    }
                    //60 ticks per second
                    mLength = (double)lngTickCount / 60;
                    mLengthDirty = false;
                }
                return mLength;
            }
        }

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
        internal void SetLengthDirty() {
            mLengthDirty = true;
        }

        /// <summary>
        /// Returns a copy of this track
        /// </summary>
        /// <param name="cloneparent"></param>
        /// <returns></returns>
        public Track Clone(Sound cloneparent) {
            //
            Track CopyTrack = new(cloneparent) {
                mNotes = new Notes(cloneparent, this),
                mMuted = mMuted,
                mInstrument = mInstrument,
                mVisible = mVisible,
                mLengthDirty = mLengthDirty,
                mLength = mLength
            };
            CopyTrack.mNotes = mNotes.Clone(this);
            return CopyTrack;
        }
        #endregion
    }
}
