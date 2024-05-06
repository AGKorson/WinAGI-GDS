using System;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    public class Loop {
        internal Cels mCelCol;
        int mMirrorPair;
        internal int mIndex;
        readonly View mParent;

        /// <summary>
        /// 
        /// </summary>
        public Cels Cels {
            get {
                // set mirror flag
                mCelCol.SetMirror(mMirrorPair < 0);

                //return the cels collection
                return mCelCol;
            }
            set {
                //sets the cels collection
                int i;
                mCelCol = value;
                // if mirrored
                if (mMirrorPair != 0) {
                    // find mirror pair (sum of mirror pairs is zero)
                    for (i = 0; i < mParent.mLoopCol.Count; i++) {
                        if (mParent.mLoopCol[(byte)i].MirrorPair + mMirrorPair == 0) {
                            // is the cels collection already set to this object?
                            if (mParent.mLoopCol[(byte)i].Cels == value) {
                                // nothing to do
                                return;
                            }
                            // set the mirrored loops cels
                            mParent.mLoopCol[(byte)i].Cels = value;
                            return;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Copies the source loop into this loop.
        /// </summary>
        /// <param name="SourceLoop"></param>
        public void CopyLoop(Loop SourceLoop) {
            // if this is a primary mirrored loop
            if (mMirrorPair > 0) {
                // call unmirror for the secondary loop
                // so it will get a correct copy of cels
                mParent.mLoopCol[MirrorLoop].UnMirror();
            }
            else if (mMirrorPair < 0) {
                // this is a secondary mirrored loop;
                // only need to reset mirror status
                // because copy function will create new cel collection
                mParent.mLoopCol[MirrorLoop].MirrorPair = 0;
                mMirrorPair = 0;
            }
            //now copy source loop cels
            this.mCelCol = SourceLoop.Cels;
            // parent, index and mirror status go unchanged
            //if there is a parent object
            if (mParent is not null) {
                mParent.IsDirty = true;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Cel this[int index] {
            get {
                return Cels[index];
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public int Index {
            get {
                return mIndex;
            }
            internal set {
                // validate
                if (mIndex < 0 || mIndex > MAX_LOOPS) {
                    throw new IndexOutOfRangeException("invalid index");
                }
                mIndex = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public int Mirrored {
            get {
                // if this loop is part of a mirror pair then it is mirrored
                // return sign so calling function can tell if this is
                // original loop, or the mirrored loop
                return Math.Sign(mMirrorPair);
            }
        }
        
        /// <summary>
        /// Return the index of the loop that mirrors this loop. If this loop is not 
        /// a mirror, returns -1.
        /// </summary>
        public int MirrorLoop {
            get {
                byte i;

                if (mMirrorPair == 0) {
                    // not mirrored
                    return -1;
                }
                // step through all loops in the loop collection
                for (i = 0; i < mParent.mLoopCol.Count; i++) {
                    // if mirror pair values equal zero
                    if (mParent.mLoopCol[i].MirrorPair + mMirrorPair == 0) {
                        // this is the loop
                        break;
                    }
                }
                return i;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal int MirrorPair {
            get {
                return mMirrorPair;
            }
            set {
                mMirrorPair = value;

            }
        }
        
        /// <summary>
        /// If the this loop is mirrored, the mirror is removed and both loops get
        /// assigned individual copies of cel data.
        /// </summary>
        public void UnMirror() {
            // Unmirroring is handled by the secondary loop; if the loop that calls
            // this function is the primary loop, this function passes the call to
            // the secondary loop for processing
            byte i;
            Cels tmpCels;
            if (mMirrorPair == 0) {
                return;
            }
            //if this is the primary loop
            if (mMirrorPair > 0) {
                //unmirror other loop
                mParent.mLoopCol[MirrorLoop].UnMirror();
                return;
            }
            // this is the secondary loop; need to create new cel collection
            // and copy cel data
            tmpCels = new Cels(mParent);
            for (i = 0; i < mCelCol.Count; i++) {
                tmpCels.Add(i, mCelCol[i].Width, mCelCol[i].Height, mCelCol[i].TransColor);
                // access cels through parent so mirror status is set properly
                tmpCels[i].AllCelData = mParent.mLoopCol[mIndex].Cels[i].AllCelData;
            }
            // set cel collectionto new cel col
            mCelCol = tmpCels;
            // clear mirror properties
            mParent.mLoopCol[MirrorLoop].MirrorPair = 0;
            mMirrorPair = 0;
            //if there is a parent object
            if (mParent is not null) {
                //set dirty flag
                mParent.IsDirty = true;
            }
        }
        
        /// <summary>
        /// Creates an exact copy of this Loop.
        /// </summary>
        /// <param name="cloneparent"></param>
        /// <returns>The Loop this method creates.</returns>
        public Loop Clone(View cloneparent) {
            // returns a copy of this loop
            Loop CopyLoop = new(cloneparent) {
                mMirrorPair = mMirrorPair,
                mIndex = mIndex,
                mCelCol = mCelCol.Clone(cloneparent)
            };
            return CopyLoop;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Loop() {
            mCelCol = [];
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public Loop(View parent) {
            mCelCol = new Cels(parent);
            mParent = parent;
        }
    }
}
