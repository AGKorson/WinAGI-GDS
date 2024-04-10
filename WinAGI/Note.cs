using System;

namespace WinAGI.Engine {
    public class Note {
        internal int mFreqDiv;
        internal int mDuration;
        internal byte mAttenuation;
        internal byte[] mrawData = [0, 0, 0, 0, 0];
        internal Sound mSndParent;
        internal Track mTrkParent;

        /// <summary>
        /// 
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
        /// 
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
        /// 
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

        public byte[] rawData {
            get {
                return mrawData;
            }
            internal set {
                mrawData = value;
            }
        }
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tparent"></param>
        internal Note(Sound parent, Track tparent) {
            mSndParent = parent;
            mTrkParent = tparent;
        }
    }
}
