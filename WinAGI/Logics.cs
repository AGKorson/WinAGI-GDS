using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.FanLogicCompiler;
using static WinAGI.Engine.LogicDecoder;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that holds all the logic resources in an AGI game.
    /// </summary>
    public class Logics : IEnumerable<Logic> {
        #region Members
        readonly AGIGame parent;
//        internal string mSourceFileExt = "";
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the logics collection for the specified game.
        /// </summary>
        /// <param name="parent"></param>
        internal Logics(AGIGame parent) {
            this.parent = parent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the list of logics in this game.
        /// </summary>
        public SortedList<int, Logic> Col { get; private set; } = [];

        /// <summary>
        /// Gets the logic with the specified index value from this list of logics.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Logic this[int index] {
            get {
                if (index < 0 || index > 255 || !Contains(index)) {
                    throw new IndexOutOfRangeException();
                }
                return Col[index];
            }
        }

        /// <summary>
        /// Gets the number of logics in this AGI game.
        /// </summary>
        public int Count { 
            get { 
                return Col.Count;
            }
        }

        /// <summary>
        /// Gets the highest index in use in this logics collection.
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
        /// resources into thislogics collection.
        /// </summary>
        /// <param name="bytResNum"></param>
        /// <param name="bytVol"></param>
        /// <param name="lngLoc"></param>
        internal void InitLoad(byte bytResNum, sbyte bytVol, int lngLoc) {
            Logic newResource = new(parent, bytResNum, bytVol, lngLoc);
            newResource.LoadNoSource();
            Col.Add(bytResNum, newResource);
            // leave it loaded, so error level can be addressed by loader
        }

        /// <summary>
        /// Returns true if a logic with the specified number exists in this game.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <returns></returns>
        public bool Contains(int ResNum) {
            return Col.ContainsKey(ResNum);
        }
        
        /// <summary>
        /// Adds a logic to this game. If NewLogic is null a blank logic is
        /// added, otherwise the added logic is cloned from NewLogic.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="NewLogic"></param>
        /// <returns>A reference to the newly added logic.</returns>
        public Logic Add(byte ResNum, Logic NewLogic = null) {
            Logic agResource;
            int intNextNum = 0;
            string strID, strBaseID;
            if (Contains(ResNum)) {
                WinAGIException wex = new(EngineResourceByNum(520)) {
                    HResult = WINAGI_ERR + 520
                };
                throw wex;
            }
            agResource = new Logic(parent, ResNum, NewLogic);
            if (NewLogic is null) {
                strID = "Logic" + ResNum;
            }
            else {
                strID = agResource.ID;
            }
            strBaseID = strID;
            while (NotUniqueID(strID, parent)) {
                intNextNum++;
                strID = strBaseID + "_" + intNextNum;
            }
            agResource.ID = strID;
            Col.Add(ResNum, agResource);
            // force flags so save function will work
            agResource.IsChanged = true;
            agResource.PropsChanged = true;
            // add it to VOL file using base save method
            // (because Logic.Save only saves the source for ingame resource)
            ((AGIResource)agResource).Save();
            setIDs = false;
            return agResource;
        }

        /// <summary>
        /// Removes the specified logic from this game.
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(byte Index) {
            if (Col.TryGetValue(Index, out Logic value)) {
                // need to clear the directory file first
                VOLManager.UpdateDirFile(value, true);
                Col.Remove(Index);
                // remove all properties from the wag file
                parent.agGameProps.DeleteSection("Logic" + Index);
                // remove ID from compiler list
                setIDs = false;
            }
        }

        /// <summary>
        /// Removes all logics from this game. Does not update WAG file.
        /// </summary>
        internal void Clear() {
            Col = [];
        }

        /// <summary>
        /// Changes the index number of a logic in this game.
        /// </summary>
        /// <param name="OldLogic"></param>
        /// <param name="NewLogic"></param>
        public void Renumber(byte OldLogic, byte NewLogic) {
            int intNextNum = 0;
            string strID, strBaseID;

            if (OldLogic == NewLogic) {
                return;
            }
            // verify old number exists
            if (!Col.TryGetValue(OldLogic, out Logic tmpLogic)) {
                throw new IndexOutOfRangeException("logic does not exist");
            }
            // verify new number is not in collection
            if (Col.ContainsKey(NewLogic)) {
                WinAGIException wex = new(EngineResourceByNum(531)) {
                    HResult = WINAGI_ERR + 531
                };
                throw wex;
            }
            // remove old logic
            parent.agGameProps.DeleteSection("Logic" + OldLogic);
            Col.Remove(OldLogic);
            VOLManager.UpdateDirFile(tmpLogic, true);
            // adjust ID if it is default (will automatically rename source file)
            if (tmpLogic.ID.ToLower() == "logic" + OldLogic) {
                strID = strBaseID = tmpLogic.ID[..5] + NewLogic;
                while (NotUniqueID(strID, parent)) {
                    strID = strBaseID + "_" + intNextNum;
                    intNextNum++;
                }
                tmpLogic.ID = strID;
            }
            // add it back with new number
            tmpLogic.Number = NewLogic;
            Col.Add(NewLogic, tmpLogic);
            VOLManager.UpdateDirFile(tmpLogic);
            tmpLogic.SaveProps();
            setIDs = false;
        }

        /// <summary>
        /// Sets the CRC for the specified logic in this game to indicate it needs
        /// to be recompiled.
        /// </summary>
        /// <param name="ResNum"></param>
        public void MarkAsChanged(byte ResNum) {
            if (Contains(ResNum)) {
                Col[ResNum].CompiledCRC = 0xffffffff;
                parent.WriteGameSetting("Logic" + ResNum.ToString(), "CompCRC32", "0xffffffff", "Logics");
                return;
            }
        }

        /// <summary>
        /// Sets the CRC for all logics in the collection to indicate they need
        /// to be recompiled.
        /// </summary>
        public void MarkAllAsChanged() {
            foreach (Logic tmpLogic in Col.Values) {
                tmpLogic.CompiledCRC = 0xffffffff;
                parent.WriteGameSetting("Logic" + tmpLogic.Number, "CompCRC32", "0xffffffff", "Logics");
            }
            parent.agGameProps.Save();
        }
        #endregion

        #region Enumeration
        LogicEnum GetEnumerator() {
            return new LogicEnum(Col);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<Logic> IEnumerable<Logic>.GetEnumerator() {
            return (IEnumerator<Logic>)GetEnumerator();
        }

        /// <summary>
        /// Implements enumeration for the Logics class.
        /// </summary>
        internal class LogicEnum : IEnumerator<Logic> {
            public SortedList<int, Logic> _logics;
            int position = -1;
            public LogicEnum(SortedList<int, Logic> list) {
                _logics = list;
            }
            object IEnumerator.Current => Current;
            public Logic Current {
                get {
                    try {
                        return _logics.Values[position];
                    }
                    catch (IndexOutOfRangeException) {
                        throw new InvalidOperationException();
                    }
                }
            }
            public bool MoveNext() {
                position++;
                return (position < _logics.Count);
            }
            public void Reset() {
                position = -1;
            }
            public void Dispose() {
                _logics = null;
            }
        }
        #endregion
    }
}
