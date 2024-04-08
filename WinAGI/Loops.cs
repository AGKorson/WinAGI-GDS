using System;
using System.Collections;
using System.Collections.Generic;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    public class Loops : IEnumerable<Loop> {
        List<Loop> mLoopCol;
        View mParent;

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        public Loop Add(int Pos) {
            // Pos is position of this loop in the loop collection
            Loop agNewLoop;
            int i;
            // if too many loops or invalid pos
            if (mLoopCol.Count == MAX_LOOPS || Pos < 0) {
                throw new IndexOutOfRangeException();
            }
            if (Pos > mLoopCol.Count) {
                // set it to end
                Pos = mLoopCol.Count;
            }
            // if adding a loop in position 0-7
            // (which could push a mirror loop out of position
            if (Pos < 7 && mLoopCol.Count >= 7) {
                // if loop 7(index of 6) is a mirror
                if (mLoopCol[6].Mirrored != 0) {
                    // unmirror it
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
        /// 
        /// </summary>
        public int Count {
            get {
                return mLoopCol.Count;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void Remove(byte Index) {
            byte i;
            //if this is last loop
            if (mLoopCol.Count == 1) {
                //can't delete last loop
                WinAGIException wex = new(LoadResString(613)) {
                    HResult = WINAGI_ERR + 613
                };
                throw wex;
            }
            if (Index >= mLoopCol.Count) {
                throw new IndexOutOfRangeException("index out of bounds");
            }
            if (mLoopCol[Index].Mirrored != 0) {
                // first, unmirror the match for this loop
                mLoopCol[mLoopCol[Index].MirrorLoop].MirrorPair = 0;
                // if that loop was the primary loop
                if (mLoopCol[mLoopCol[Index].MirrorLoop].MirrorPair > 0) {
                    // permanently flip that loop's cel data
                    for (i = 0; i < mLoopCol[mLoopCol[Index].MirrorLoop].Cels.Count; i++) {
                        mLoopCol[mLoopCol[Index].MirrorLoop].Cels[i].FlipCel();
                    }
                }
            }
            mLoopCol.RemoveAt(Index);
            //ensure all loop indices are correct
            if (mLoopCol.Count > 0) {
                for (i = 0; i < mLoopCol.Count; i++) {
                    mLoopCol[i].Index = (byte)i;
                }
            }
            if (mParent is not null) {
                //tag as dirty
                mParent.IsDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cloneparent"></param>
        /// <returns></returns>
        public Loops Clone(View cloneparent) {
            // returns a copy of this loop collection
            Loops CopyLoops = new(cloneparent);
            foreach (Loop tmpLoop in mLoopCol) {
                CopyLoops.mLoopCol.Add(tmpLoop.Clone(cloneparent));
            }
            return CopyLoops;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Loops() {
            mLoopCol = [];
        }
        
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public Loops(View parent) {
            mLoopCol = [];
            // set parent
            mParent = parent;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        LoopEnum GetEnumerator() {
            return new LoopEnum(mLoopCol);
        }
        
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        
        IEnumerator<Loop> IEnumerable<Loop>.GetEnumerator() {
            return (IEnumerator<Loop>)GetEnumerator();
        }
    }
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
