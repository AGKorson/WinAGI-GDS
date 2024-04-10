using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Engine {
    public class Track {
        //local variable(s) to hold property Value(s)
        Notes mNotes;
        bool mMuted;
        byte mInstrument;
        bool mVisible;
        internal Sound mParent;
        bool mLengthDirty;
        double mLength;
        public byte Instrument {
            get {
                return mInstrument;
            }
            set {
                //validate
                if (value >= 128) {
                    //error
                    throw new ArgumentOutOfRangeException();
                }
                if (mInstrument != value) {
                    mInstrument = value;
                    //note change
                    mParent.TrackChanged();
                }
            }
        }
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
        public Notes Notes {
            get {
                return mNotes;
            }
            internal set {
                mNotes = value;
            }
        }
        internal void SetLengthDirty() {
            //used by tracks to let parent sound know that length needs to be recalculated
            mLengthDirty = true;
        }
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
        public Track() {
            mNotes = [];
            mLengthDirty = true;
            mVisible = true;
            mInstrument = 80;
        }
        internal Track(Sound parent) {
            mNotes = new Notes(parent, this);
            mLengthDirty = true;
            mVisible = true;
            mInstrument = 80;
            mParent = parent;
        }
        public Track Clone(Sound cloneparent) {
            //returns a copy of this track
            Track CopyTrack = new(cloneparent) {
                mNotes = mNotes.Clone(this),
                mMuted = mMuted,
                mInstrument = mInstrument,
                mVisible = mVisible,
                mLengthDirty = mLengthDirty,
                mLength = mLength
            };
            return CopyTrack;
        }
    }
}
