using System;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents an individual AGI sound track note.
    /// </summary>
    public class Note {
        #region Members
        internal int mFreqDiv;
        internal int mDuration;
        internal byte mAttenuation;
        internal Sound mSndParent;
        internal Track mTrkParent;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new note that is not part of a sound track with
        /// the specified paramters.
        /// </summary>
        /// <param name="freqdiv"></param>
        /// <param name="duration"></param>
        /// <param name="attenuation"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Note(int freqdiv, int duration, byte attenuation) {
            if (freqdiv < 0 || freqdiv > 1023) {
                throw new ArgumentOutOfRangeException(nameof(freqdiv));
            }
            mFreqDiv = freqdiv;
            if (duration < 0 || duration > 0xFFFF) {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }
            mDuration = duration;
            if (attenuation > 15) {
                throw new ArgumentOutOfRangeException(nameof(attenuation));
            }
            mAttenuation = attenuation;
        }

        /// <summary>
        /// Creates an new note with the specified parameters that is being added to a 
        /// track that is part of a sound resource. 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tparent"></param>
        internal Note(int freqdivisor, int duration, byte attenuation, Sound parent, Track tparent) {
            if (freqdivisor < 0 || freqdivisor > 1023) {
                throw new ArgumentOutOfRangeException(nameof(freqdivisor));
            }
            if (duration < 0 || duration > 0xFFFF) {
                throw new ArgumentOutOfRangeException(nameof(duration));
            }
            if (attenuation > 15) {
                throw new ArgumentOutOfRangeException(nameof(attenuation));
            }
            mAttenuation = attenuation;
            mDuration = duration;
            mFreqDiv = freqdivisor;
            mSndParent = parent;
            mTrkParent = tparent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the attenuation (amount of quietness applied to volume)
        /// value for this note.
        /// </summary>
        public byte Attenuation {
            get {
                return mAttenuation;
            }
            set {
                ArgumentOutOfRangeException.ThrowIfGreaterThan(15, value);
                mAttenuation = value;
                mSndParent?.NoteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the duration (length) value for this note. Units are
        /// in 60ths of a second.
        /// </summary>
        public int Duration {
            get {
                return mDuration;
            }
            set {
                if (value < 0 || value > 0xFFFF) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                mDuration = value;
                mSndParent?.NoteChanged();
                mTrkParent?.SetLengthChanged();
            }
        }

        /// <summary>
        /// Gets or sets the frequency divisor value (which was used by the original
        /// PCjr sound chip to set the note's frequency) for this note.
        /// </summary>
        public int FreqDivisor {
            get {
                return mFreqDiv;
            }
            set {
                if (value < 0 || value > 1023) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                mFreqDiv = value;
                mSndParent?.NoteChanged();
            }
        }

        #endregion

        #region Methods
        // None
        #endregion
    }
}
