using System;
using System.Collections;
using System.Collections.Generic;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents a collection of cels, usually as part of
    /// a loop object.
    /// </summary>
    [Serializable]
    public class Cels : IEnumerable<Cel> {
        #region Local Members
        internal readonly List<Cel> mCelCol;
        [NonSerialized]
        View mParent;
        bool mSetMirror;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new, empty Cels collection that is not part of a View or Loop.
        /// </summary>
        public Cels() {
            mCelCol = [];
        }

        /// <summary>
        /// Creates a new, empty Cels collection when a new loop is created.
        /// </summary>
        /// <param name="parent"></param>
        internal Cels(View parent) {
            mCelCol = [];
            mParent = parent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the cel at the specified location from this collection.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The specified cel.</returns>
        public Cel this[int index] {
            get {
                if (mCelCol.Count == 0) {
                    // no cels is abnormal
                    return null;
                }
                ArgumentOutOfRangeException.ThrowIfNegative(index);
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, mCelCol.Count);
                // mirror status is already set; just pass the desired cel
                return mCelCol[index];
            }
        }

        /// <summary>
        /// Gets the number of cels in this collection.
        /// </summary>
        public int Count {
            get {
                return mCelCol.Count;
            }
        }

        /// <summary>
        /// Gets or sets the parent view for this Cels collection.
        /// </summary>
        public View Parent { get { return mParent; } internal set { mParent = value; } }

        #endregion

        #region Methods
        /// <summary>
        /// Adds a new cel to this collection at the specified location with the 
        /// specified parameters.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="transcolor"></param>
        /// <returns>The cel that was added.</returns>
        public Cel Add(int pos, byte width = 1, byte height = 1, AGIColorIndex transcolor = AGIColorIndex.Black) {
            Cel agNewCel;

            if (mCelCol.Count == MAX_CELS || pos < 0) {
                WinAGIException wex = new(EngineResourceByNum(553).Replace("ARG1", "")) {
                    HResult = WINAGI_ERR + 553,
                };
                throw wex;
            }
            if (pos > mCelCol.Count) {
                pos = mCelCol.Count;
            }
            agNewCel = new Cel(mParent) {
                mWidth = width,
                mHeight = height,
                mTransColor = transcolor,
                mIndex = pos
            };
            agNewCel.mCelData = new byte[agNewCel.mWidth, agNewCel.mHeight];
            for (int i = 0; i < agNewCel.mWidth; i++) {
                for (int j = 0; j < agNewCel.mHeight; j++) {
                    agNewCel.mCelData[i, j] = (byte)transcolor;
                }
            }
            agNewCel.SetMirror(mSetMirror);
            if (mCelCol.Count == 0) {
                mCelCol.Add(agNewCel);
            }
            else {
                // add it at the insert position
                mCelCol.Insert(pos, agNewCel);
            }
            if (mParent is not null) {
                mParent.IsChanged = true;
            }
            return agNewCel;
        }

        /// <summary>
        /// Removes the specified cel from this collection. Not valid if only one
        /// cel. All Cels objects must include at least one cel.
        /// </summary>
        /// <param name="index"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void Remove(int index) {
            int i;
            if (mCelCol.Count == 1) {
                WinAGIException wex = new(EngineResourceByNum(522)) {
                    HResult = WINAGI_ERR + 522,
                };
                throw wex;
            }
            if (index < 0 || index >= mCelCol.Count) {
                throw new IndexOutOfRangeException();
            }
            mCelCol.RemoveAt(index);
            if (index < mCelCol.Count) {
                // adjust indices
                for (i = index; i < mCelCol.Count; i++) {
                    mCelCol[index].Index = (byte)i;
                }
            }
            if (mParent is not null) {
                mParent.IsChanged = true;
            }
        }

        /// <summary>
        /// This method is used by a Loop object when getting its Cels property. It
        /// ensures the cels are in the proper configuration before being accessed.
        /// </summary>
        /// <param name="NewState"></param>
        internal void SetMirror(bool NewState) {
            mSetMirror = NewState;
            foreach (Cel cel in mCelCol) {
                cel.SetMirror(NewState);
            }
        }

        /// <summary>
        /// Creates an exact copy of this Cels object.
        /// </summary>
        /// <param name="cloneparent"></param>
        /// <returns>The Cels object this method creates.</returns>
        public Cels Clone(View cloneparent) {
            Cels clonecels = new(cloneparent);
            foreach (Cel tmpCel in mCelCol) {
                clonecels.mCelCol.Add(tmpCel.Clone(cloneparent));
            }
            clonecels.mSetMirror = mSetMirror;
            return clonecels;
        }

        public void CloneFrom(Cels SourceCels) {
            for (int i = 0; i < SourceCels.Count; i++) {
                mCelCol.Add(new Cel(mParent));
                mCelCol[i].CloneFrom(SourceCels[i]);
            }
        }
        #endregion

        #region Enumeration
        CelEnum GetEnumerator() {
            return new CelEnum(mCelCol);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }

        IEnumerator<Cel> IEnumerable<Cel>.GetEnumerator() {
            return (IEnumerator<Cel>)GetEnumerator();
        }

        /// <summary>
        /// Implements enumeration for the Cels class.
        /// </summary>
        internal class CelEnum : IEnumerator<Cel> {
            public List<Cel> _cels;
            int position = -1;
            public CelEnum(List<Cel> list) {
                _cels = list;
            }
            object IEnumerator.Current => Current;
            public Cel Current {
                get {
                    try {
                        return _cels[position];
                    }
                    catch (IndexOutOfRangeException) {
                        throw new InvalidOperationException();
                    }
                }
            }
            public bool MoveNext() {
                position++;
                return (position < _cels.Count);
            }
            public void Reset() {
                position = -1;
            }
            public void Dispose() {
                _cels = null;
            }
        }
        #endregion
    }
}
