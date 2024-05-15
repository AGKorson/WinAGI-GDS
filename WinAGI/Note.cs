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
        internal byte[] mrawData = [0, 0, 0, 0, 0];
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
                mTrkParent?.SetLengthDirty();
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

        /// <summary>
        ///  Gets the note data for this note as an array of five bytes arranged in
        ///  the format used by AGI sound resources. If note is not part of a track,
        ///  the data are configured as if it were track 0.
        /// </summary>
        public byte[] rawData {
            get {
                // TODO: need to decide how to handle the passing of raw data
                // it depends on track number - maybe include that as a parameter?

                //// duration
                //mrawData[0] = (byte)(mDuration >> 8);
                //mrawData[1] = (byte)(mDuration & 0xff);
                //int track = mTrkParent is null ? 0 : mTrkParent.mTrack;
                //// frequency data
                //mrawData[2] = (byte)(mFreqDiv / 16);
                //mrawData[3] = (byte)((mFreqDiv % 16) + 128 + 32 * ());
                //// attenuation
                //mrawData[4] = (byte)(mAttenuation + (byte)(144 + 32 * (mTrkParent is null ? 0 : mTrkParent.mTrack)));
                return mrawData;
            }
        }
        #endregion

        #region Methods
        // None
        #endregion
    }
}
