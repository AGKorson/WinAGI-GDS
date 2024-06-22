using System;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents a single loop in an AGI View resource.
    /// </summary>
    public class Loop {
        #region Members
        internal Cels mCelCol;
        int mMirrorPair;
        internal int mIndex;
        readonly View mParent;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new loop for a View that is not in a game, with no cels.
        /// </summary>
        public Loop() {
            mCelCol = [];
        }

        /// <summary>
        /// Creates a new loop for an in-game View, with no cels.
        /// </summary>
        /// <param name="parent"></param>
        internal Loop(View parent) {
            mCelCol = new Cels(parent);
            mParent = parent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of cels in this loop, including flipping the cels
        /// as necessary if the loop is a mirror.
        /// </summary>
        public Cels Cels {
            get {
                // set mirror status
                mCelCol.SetMirror(mMirrorPair < 0);
                return mCelCol;
            }
            set {
                int i;
                mCelCol = value;
                if (mMirrorPair != 0) {
                    // make sure mirror pair references the same cels
                    for (i = 0; i < mParent.mLoopCol.Count; i++) {
                        if (mParent.mLoopCol[(byte)i].MirrorPair + mMirrorPair == 0) {
                            // is the cels collection already set to this object?
                            if (mParent.mLoopCol[(byte)i].Cels == value) {
                                return;
                            }
                            mParent.mLoopCol[(byte)i].Cels = value;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the specified cel from this loop.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Cel this[int index] {
            get {
                return Cels[index];
            }
        }

        /// <summary>
        /// Gets or sets the index number for this loop.
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
        /// Gets a value indicating whether or not this loop is part of a
        /// mirror pair.
        /// </summary>
        /// <returns>Positive mirror pair number if this is the original
        /// loop, negative mirror pair number if this is the mirrored loop
        /// or zero if loop is not mirrored.</returns>
        public int Mirrored {
            get {
                // if this loop is part of a mirror pair then it is mirrored
                // return sign so calling function can tell if this is
                // original loop, or the mirrored loop
                return Math.Sign(mMirrorPair);
            }
        }

        /// <summary>
        /// Gets the index of the loop that mirrors this loop.
        /// </summary>
        /// <returns>Index of the matching mirror loop, or -1 if this loop is not a mirror.</returns>
        public int MirrorLoop {
            get {
                byte i;

                if (mMirrorPair == 0) {
                    return -1;
                }
                for (i = 0; i < mParent.mLoopCol.Count; i++) {
                    if (mParent.mLoopCol[i].MirrorPair + mMirrorPair == 0) {
                        // this is the loop
                        break;
                    }
                }
                return i;
            }
        }

        /// <summary>
        /// Gets or sets the mirror pair falue for this loop. Positive numbers indicate
        /// the original loop. Negative numbers indicate the mirrored loop. Zero indicates
        /// a non-mirrored loop.
        /// </summary>
        internal int MirrorPair {
            get {
                return mMirrorPair;
            }
            set {
                mMirrorPair = value;

            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// This loop is set to a copy of the specified loop by copying all its data
        /// elements.
        /// </summary>
        /// <param name="SourceLoop"></param>
        public void SetLoop(Loop SourceLoop) {
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
            // copy source loop cels
            this.mCelCol = SourceLoop.Cels.Clone(mParent);
            if (mParent is not null) {
                mParent.IsDirty = true;
            }
        }

        /// <summary>
        /// Removes mirror property from this loop. This loop and its mirrored loop
        /// are assigned separate copies of the loop's cel data. If loop is not mirrored
        /// no effect.
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
            if (mMirrorPair > 0) {
                // unmirror other loop
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
            mCelCol = tmpCels;
            mParent.mLoopCol[MirrorLoop].MirrorPair = 0;
            mMirrorPair = 0;
            if (mParent is not null) {
                mParent.IsDirty = true;
            }
        }
        
        /// <summary>
        /// Creates an exact copy of this Loop.
        /// </summary>
        /// <param name="cloneparent"></param>
        /// <returns>The Loop this method creates.</returns>
        internal Loop Clone(View cloneparent) {
            Loop CopyLoop = new(cloneparent) {
                mMirrorPair = mMirrorPair,
                mIndex = mIndex,
                mCelCol = mCelCol.Clone(cloneparent)
            };
            return CopyLoop;
        }
        #endregion
    }
}
