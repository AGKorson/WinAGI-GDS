using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Engine
{
    public class Note
    {
        internal int mFreqDiv;
        internal int mDuration;
        internal byte mAttenuation;
        internal Sound mSndParent;
        internal Track mTrkParent;
        string strErrSource;
        public byte Attenuation
        {
            get
            {
                return mAttenuation;
            }
            set
            {
                //validate
                if (value > 15) {
                    //invalid item
                    throw new Exception("6, strErrSource, Overflow");
                }
                mAttenuation = value;
                //if parent is assigned
                //notify parent
                mSndParent?.NoteChanged();
            }
        }
        public int Duration
        {
            get
            {
                return mDuration;
            }
            set
            {
                //validate
                if (value < 0 || value > 0xFFFF) {
                    //invalid frequency
                    throw new Exception("6, strErrSource, Overflow");
                }
                mDuration = value;
                //notify parents, if applicable
                mSndParent?.NoteChanged();
                mTrkParent?.SetLengthDirty();
            }
        }
        public int FreqDivisor
        {
            get
            {
                return mFreqDiv;
            }
            set
            {
                //validate
                // ****TODO: is zero allowed? it will cause divbyzero error when trying to 
                // convert it to MIDI
                if (value < 0 || value > 1023) {
                    //invalid frequency
                    throw new Exception("6, strErrSource, Overflow");
                }
                mFreqDiv = value;
                //if parent is assigned
                //notify parent
                mSndParent?.NoteChanged();
            }
        }
        public Note()
        {
            strErrSource = "AGINote";
        }
        public Note(int freqdiv, int duration, byte attenuation)
        {
            strErrSource = "AGINote";
            //validate freqdiv
            if (freqdiv < 0 || freqdiv > 1023) {
                //invalid frequency
                throw new Exception("6, strErrSource, Overflow");
            }
            mFreqDiv = freqdiv;
            //validate duration
            if (duration < 0 || duration > 0xFFFF) {
                //invalid frequency
                throw new Exception("6, strErrSource, Overflow");
            }
            mDuration = duration;
            //validate attenuation
            if (attenuation > 15) {
                //invalid item
                throw new Exception("6, strErrSource, Overflow");
            }
            mAttenuation = attenuation;
        }
        internal Note(Sound parent, Track tparent)
        {
            mSndParent = parent;
            mTrkParent = tparent;
            strErrSource = "AGINote";
        }
    }
}
