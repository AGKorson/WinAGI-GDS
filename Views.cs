using System;
using System.Collections;
using System.Collections.Generic;
using WinAGI.Common;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that holds all the view resources in an AGI game.
    /// </summary>
    public class Views : IEnumerable<View> {
        #region Fields
        private readonly AGIGame parent;
        #endregion

        #region Constructors
        internal Views(AGIGame parent) {
            this.parent = parent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the list of views in this game.
        /// </summary>
        public SortedList<int, View> Col { get; private set; } = [];

        /// <summary>
        /// Gets the view with the specified index value from this list of views.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public View this[int index] {
            get {
                if (index < 0 || index > 255 || !Contains(index)) {
                    throw new IndexOutOfRangeException();
                }
                return Col[index];
            }
        }

        /// <summary>
        /// Gets the number of sounds in this AGI game.
        /// </summary>
        public byte Count {
            get {
                return (byte)Col.Count;
            }
        }

        /// <summary>
        /// Gets the highest index in use in this views collection.
        /// </summary>
        public int Max {
            get {
                int max = 0;
                if (Col.Count > 0)
                    max = Col.Keys[Col.Count - 1];
                return max;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Called by the load game methods for the initial loading of
        /// resources into views collection.
        /// </summary>
        /// <param name="resnum"></param>
        /// <param name="volnum"></param>
        /// <param name="location"></param>
        internal void InitLoad(byte resnum, sbyte volnum, int location) {
            View newResource = new(parent, resnum, volnum, location);
            newResource.Load();
            Col.Add(resnum, newResource);
            // leave it loaded, so error level can be addressed by loader
        }

        /// <summary>
        /// Returns true if a picture with the specified number exists in this game.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <returns></returns>
        public bool Contains(int ResNum) {
            return Col.ContainsKey(ResNum);
        }

        /// <summary>
        /// Adds a view to this game. If NewView is null a blank view is 
        /// added, otherwise the added view is cloned from NewView.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="NewView"></param>
        /// <returns>A reference to the newly added view.</returns>
        public View Add(byte ResNum, View NewView = null) {
            View agResource;
            int nextNum = 0;
            string id, baseid;
            if (Contains(ResNum)) {
                WinAGIException wex = new(EngineResourceByNum(520)) {
                    HResult = WINAGI_ERR + 520
                };
                throw wex;
            }
            // create new ingame view
            agResource = new View(parent, ResNum, NewView);
            if (NewView is null) {
                id = "View" + ResNum;
            }
            else {
                id = agResource.ID;
            }
            baseid = id;
            while (NotUniqueID(id, parent)) {
                nextNum++;
                id = baseid + "_" + nextNum;
            }
            Col.Add(ResNum, agResource);
            // force flags so save function will work
            agResource.mViewChanged = true;
            agResource.PropsChanged = true;
            // save new view to add it to VOL file
            agResource.Save();
            FanLogicCompiler.setIDs = false;
            return agResource;
        }

        /// <summary>
        /// Removes the specified view from this game.
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(byte Index) {
            if (Col.TryGetValue(Index, out View value)) {
                // need to clear the directory file first
                VOLManager.UpdateDirFile(value, true);
                Col.Remove(Index);
                // remove all properties from the wag file
                parent.agGameProps.DeleteSection("View" + Index);
                // remove ID from compiler list
                FanLogicCompiler.setIDs = false;
            }
        }

        /// <summary>
        /// Removes all views from this game. Does not update WAG file.
        /// </summary>
        internal void Clear() {
            Col = [];

        }

        /// <summary>
        /// Changes the index number of a view in this game.
        /// </summary>
        /// <param name="OldView"></param>
        /// <param name="NewView"></param>
        public void Renumber(byte OldView, byte NewView) {
            int nextNum = 0;
            string id, baseid;

            if (OldView == NewView) {
                return;
            }
            // verify old number exists
            if (!Col.TryGetValue(OldView, out View tmpView)) {
                throw new IndexOutOfRangeException("view does not exist");
            }
            // verify new number is not in collection
            if (Col.ContainsKey(NewView)) {
                WinAGIException wex = new(EngineResourceByNum(531)) {
                    HResult = WINAGI_ERR + 531
                };
                throw wex;
            }
            // remove old view
            parent.agGameProps.DeleteSection("View" + OldView);
            Col.Remove(OldView);
            VOLManager.UpdateDirFile(tmpView, true);
            // adjust id if it is default
            if (tmpView.ID.Equals("view" + OldView, StringComparison.OrdinalIgnoreCase)) {
                id = baseid = tmpView.ID[..4] + NewView;
                while (NotUniqueID(id, parent)) {
                    id = baseid + "_" + nextNum;
                    nextNum++;
                }
                tmpView.ID = id;
            }
            // add it back with new number
            tmpView.Number = NewView;
            Col.Add(NewView, tmpView);
            VOLManager.UpdateDirFile(tmpView);
            tmpView.SaveProps();
            FanLogicCompiler.setIDs = false;
        }
        #endregion

        #region Enumeration
        ViewEnum GetEnumerator() {
            return new ViewEnum(Col);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        IEnumerator<View> IEnumerable<View>.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Implements enumeration for the Views class
        /// </summary>
        internal class ViewEnum : IEnumerator<View> {
            public SortedList<int, View> _views;
            int position = -1;
            public ViewEnum(SortedList<int, View> list) {
                _views = list;
            }
            object IEnumerator.Current => Current;
            public View Current {
                get {
                    try {
                        return _views.Values[position];
                    }
                    catch (IndexOutOfRangeException) {

                        throw new InvalidOperationException();
                    }
                }
            }
            public bool MoveNext() {
                position++;
                return position < _views.Count;
            }
            public void Reset() {
                position = -1;
            }
            public void Dispose() {
                _views = null;
            }
        }
        #endregion
    }
}
