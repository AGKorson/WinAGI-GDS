using System;
using System.Collections;
using System.Collections.Generic;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that holds all the views in an AGI game.
    /// </summary>
    public class Views : IEnumerable<View> {
        readonly AGIGame parent;

        internal Views(AGIGame parent) {
            this.parent = parent;
        }
        public SortedList<byte, View> Col { get; private set; } = [];
        public View this[int index] {
            get {
                if (index < 0 || index > 255 || !Contains((byte)index)) {
                    throw new IndexOutOfRangeException();
                }
                return Col[(byte)index];
            }
        }

        /// <summary>
        /// Returns the number of sounds in this AGI game.
        /// </summary>
        public byte Count {
            get {
                return (byte)Col.Count;
            } 
        }

        /// <summary>
        /// Returns the highest index in use in the views collection.
        /// </summary>
        public byte Max {
            get {
                byte max = 0;
                if (Col.Count > 0)
                    max = Col.Keys[Col.Count - 1];
                return max;
            }
        }

        /// <summary>
        /// Returns true if a picture with number ResNum exists in this game.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <returns></returns>
        public bool Contains(byte ResNum) {
            return Col.ContainsKey(ResNum);
        }

        /// <summary>
        /// Removes all views from this game.
        /// </summary>
        public void Clear() {
            Col = [];
        }

        /// <summary>
        /// Adds a view to the game. If NewView is null a blank view is 
        /// added, otherwise the added view is cloned from NewView.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="NewView"></param>
        /// <returns></returns>
        public View Add(byte ResNum, View NewView = null) {
            View agResource;
            int intNextNum = 0;
            string strID, strBaseID;
            if (Contains(ResNum)) {
                WinAGIException wex = new(LoadResString(602)) {
                    HResult = WINAGI_ERR + 602
                };
                throw wex;
            }
            // create new ingame view
            agResource = new View(parent, ResNum, NewView);
            if ((NewView is null)) {
                strID = "View" + ResNum;
            }
            else {
                strID = agResource.ID;
            }
            strBaseID = strID;
            while (NotUniqueID(strID, parent)) {
                intNextNum++;
                strID = strBaseID + "_" + intNextNum;
            }
            Col.Add(ResNum, agResource);
            //force flags so save function will work
            agResource.IsDirty = true;
            agResource.PropsDirty = true;
            //save new view to add it to VOL file
            agResource.Save();
            // id list needs to be updated
            LogicCompiler.blnSetIDs = false;
            //return the new view
            return agResource;
        }

        /// <summary>
        /// Removes a view from the game.
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(byte Index) {
            if (Col.TryGetValue(Index, out View value)) {
                // need to clear the directory file first
                VOLManager.Base.UpdateDirFile(value, true);
                Col.Remove(Index);
                // remove all properties from the wag file
                parent.agGameProps.DeleteSection("View" + Index);
                // remove ID from compiler list
                LogicCompiler.blnSetIDs = false;
            }
        }

        /// <summary>
        /// Changes the number of a view in this game.
        /// </summary>
        /// <param name="OldView"></param>
        /// <param name="NewView"></param>
        public void Renumber(byte OldView, byte NewView) {
            View tmpView;
            int intNextNum = 0;
            string strID, strBaseID;

            if (OldView == NewView) {
                return;
            }
            // verify old number exists
            if (!Col.ContainsKey(OldView)) {
                throw new IndexOutOfRangeException("view does not exist");
            }
            //verify new number is not in collection
            if (Col.ContainsKey(NewView)) {
                WinAGIException wex = new(LoadResString(669)) {
                    HResult = WINAGI_ERR + 669
                };
                throw wex;
            }
            tmpView = Col[OldView];
            // remove old view
            parent.agGameProps.DeleteSection("View" + OldView);
            Col.Remove(OldView);
            VOLManager.Base.UpdateDirFile(tmpView, true);
            // adjust id if it is default
            if (tmpView.ID == "View" + OldView) {
                strID = strBaseID = "View" + NewView;
                while (NotUniqueID(strID, parent)) {
                    strID = strBaseID + "_" + intNextNum;
                    intNextNum++;
                }
            }
            // add it back with new number
            tmpView.Number = NewView;
            Col.Add(NewView, tmpView);
            VOLManager.Base.UpdateDirFile(tmpView);
            tmpView.SaveProps();
            // id list needs updating
            LogicCompiler.blnSetIDs = false;
        }

        /// <summary>
        /// Called by the load game methods for the initial loading of
        /// resources into views collection.
        /// </summary>
        /// <param name="bytResNum"></param>
        /// <param name="bytVol"></param>
        /// <param name="lngLoc"></param>
        internal void InitLoad(byte bytResNum, sbyte bytVol, int lngLoc) {
            View newResource = new(parent, bytResNum, bytVol, lngLoc);
            newResource.Load();
            Col.Add(bytResNum, newResource);
            // leave it loaded, so error level can be addressed by loader
        }

        #region Enumeration
        ViewEnum GetEnumerator() {
            return new ViewEnum(Col);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<View> IEnumerable<View>.GetEnumerator() {
            return (IEnumerator<View>)GetEnumerator();
        }

        /// <summary>
        /// Implements enumeration for the Views class
        /// </summary>
        internal class ViewEnum : IEnumerator<View> {
            public SortedList<byte, View> _views;
            int position = -1;
            public ViewEnum(SortedList<byte, View> list) {
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
                return (position < _views.Count);
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
