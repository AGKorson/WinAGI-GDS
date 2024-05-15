﻿using System;
using System.Collections;
using System.Collections.Generic;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents a collection of loops, usually as part
    /// of an AGI View resource.
    /// </summary>
    public class Loops : IEnumerable<Loop> {
        #region Members
        List<Loop> mLoopCol;
        View mParent;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new empty Loops collection that is not attached to a View resource.
        /// </summary>
        public Loops() {
            mLoopCol = [];
        }
        
        /// <summary>
        /// Creates a new empty Loops collection when an AGI View resource is created.
        /// </summary>
        /// <param name="parent"></param>
        internal Loops(View parent) {
            mLoopCol = [];
            mParent = parent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the parent View resource for this Loops collection.
        /// </summary>
        public View Parent { 
            get {
                return mParent;
            }
            internal set {
                mParent = value;
            }
        }

        /// <summary>
        /// Gets a loop from this collection specified by its index value.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Loop this[int index] {
            get {
                if (index < 0 || index >= mLoopCol.Count) {
                    throw new IndexOutOfRangeException();
                }
                return mLoopCol[index];
            }
        }

        /// <summary>
        /// Gets the number of loops in this Loops collection.
        /// </summary>
        public int Count {
            get {
                return mLoopCol.Count;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds a new loop to this Loops collection at the specified position.
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public Loop Add(int Pos) {
            Loop agNewLoop;
            int i;
            if (mLoopCol.Count == MAX_LOOPS || Pos < 0) {
            // invalid operation
                throw new IndexOutOfRangeException();
            }
            if (Pos > mLoopCol.Count) {
                // set it to end
                Pos = mLoopCol.Count;
            }
            // if adding a loop in position 0-7
            // (which could push a mirror loop out of position
            if (Pos < 7 && mLoopCol.Count >= 7) {
                if (mLoopCol[6].Mirrored != 0) {
                    // unmirror loops that get pushed out of position
                    mLoopCol[6].UnMirror();
                }
            }
            agNewLoop = new Loop(mParent) {
                mIndex = Pos
            };
            if (mLoopCol.Count == 0) {
                mLoopCol.Add(agNewLoop);
            }
            else {
                mLoopCol.Insert(Pos, agNewLoop);
            }
            for (i = 0; i < mLoopCol.Count; i++) {
                mLoopCol[i].Index = (byte)i;
            }
            if (mParent is not null) {
                mParent.IsDirty = true;
            }
            return agNewLoop;
        }
        
        /// <summary>
        /// Removes the specified loop from this Loops collection. The last loop
        /// in the collection cannot be removed.
        /// </summary>
        /// <param name="Index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void Remove(byte Index) {

            if (mLoopCol.Count == 1) {
                // can't delete last loop
                WinAGIException wex = new(LoadResString(613)) {
                    HResult = WINAGI_ERR + 613
                };
                throw wex;
            }
            if (Index >= mLoopCol.Count) {
                throw new IndexOutOfRangeException("index out of bounds");
            }
            if (mLoopCol[Index].Mirrored != 0) {
                // unmirror the match for this loop
                mLoopCol[mLoopCol[Index].MirrorLoop].MirrorPair = 0;
                // if that loop was the primary loop
                if (mLoopCol[mLoopCol[Index].MirrorLoop].MirrorPair > 0) {
                    // permanently flip that loop's cel data
                    for (int i = 0; i < mLoopCol[mLoopCol[Index].MirrorLoop].Cels.Count; i++) {
                        mLoopCol[mLoopCol[Index].MirrorLoop].Cels[i].FlipCel();
                    }
                }
            }
            mLoopCol.RemoveAt(Index);
            // update all loop indices
            if (mLoopCol.Count > 0) {
                for (int i = 0; i < mLoopCol.Count; i++) {
                    mLoopCol[i].Index = (byte)i;
                }
            }
            if (mParent is not null) {
                mParent.IsDirty = true;
            }
        }

        /// <summary>
        /// Creates an exact copy of this Loops object.
        /// </summary>
        /// <param name="cloneparent"></param>
        /// <returns>The Loops collection this method creates.</returns>
        public Loops Clone(View cloneparent) {
            Loops CopyLoops = new(cloneparent);
            foreach (Loop tmpLoop in mLoopCol) {
                CopyLoops.mLoopCol.Add(tmpLoop.Clone(cloneparent));
            }
            return CopyLoops;
            // TODO: need to confirm loops and cels clone correctly
        }
        #endregion

        #region Enumeration
        LoopEnum GetEnumerator() {
            return new LoopEnum(mLoopCol);
        }
        
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        
        IEnumerator<Loop> IEnumerable<Loop>.GetEnumerator() {
            return (IEnumerator<Loop>)GetEnumerator();
        }
        #endregion
    }

    /// <summary>
    /// Implements enumeration for the Loops class.
    /// </summary>
    internal class LoopEnum : IEnumerator<Loop> {
        public List<Loop> _loops;
        int position = -1;
        public LoopEnum(List<Loop> list) {
            _loops = list;
        }
        object IEnumerator.Current => Current;
        public Loop Current {
            get {
                try {
                    return _loops[position];
                }
                catch (IndexOutOfRangeException) {
                    throw new InvalidOperationException();
                }
            }
        }
        public bool MoveNext() {
            position++;
            return (position < _loops.Count);
        }
        public void Reset() {
            position = -1;
        }
        public void Dispose() {
            _loops = null;
        }
    }
}
