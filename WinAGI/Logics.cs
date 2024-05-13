using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.LogicCompiler;
using static WinAGI.Engine.LogicDecoder;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that holds all the logics in an AGI game.
    /// </summary>
    public class Logics : IEnumerable<Logic> {
        readonly AGIGame parent;
        internal string mSourceFileExt = "";

        internal Logics(AGIGame parent) {
            this.parent = parent;
            mSourceFileExt = DefaultSrcExt;
        }
        public SortedList<byte, Logic> Col { get; private set; } = [];
        public Logic this[int index] {
            get {
                if (index < 0 || index > 255 || !Exists((byte)index)) {
                    throw new IndexOutOfRangeException();
                }
                return Col[(byte)index];
            }
        }

        /// <summary>
        /// Returns the number of logics in this AGI game.
        /// </summary>
        public byte Count { 
            get { 
                return (byte)Col.Count;
            }
        }

        /// <summary>
        /// Returns the highest index in use in the logics collection.
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
        /// Gets or sets the default source file extension that will be used for
        /// source code files in ithis game.
        /// </summary>
        public string SourceFileExt {
            get {
                return mSourceFileExt;
            }
            set {
                // lower case, max four characters, not null
                if (value.Length == 0) {
                    // use default
                    mSourceFileExt = DefaultSrcExt;
                    return;
                }
                // no period
                if (mSourceFileExt.Contains('.')) {
                    throw new ArgumentException("unallowable characters");
                }
                // no unallowed filename characters
                if (mSourceFileExt.Any(Path.GetInvalidFileNameChars().Contains)) {
                    throw new ArgumentException("unallowable characters");
                }
                mSourceFileExt = value.ToLower();
                parent.WriteGameSetting("General", "SourceFileExt", mSourceFileExt);
            }
        }
        
        /// <summary>
        /// Returns true if a logic with number ResNum exists in this game.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <returns></returns>
        public bool Exists(byte ResNum) {
            return Col.ContainsKey(ResNum);
        }
        
        /// <summary>
        /// Removes all logics from the game.
        /// </summary>
        internal void Clear() {
            Col = [];
        }

        /// <summary>
        /// Adds a logic to the game. If NewLogic is null a blank logic is
        /// added, otherwise the added logic is cloned from NewLogic.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="NewLogic"></param>
        /// <returns>A reference to the newly added logic.</returns>
        public Logic Add(byte ResNum, Logic NewLogic = null) {
            Logic agResource;
            int intNextNum = 0;
            string strID, strBaseID;
            if (Exists(ResNum)) {
                WinAGIException wex = new(LoadResString(602)) {
                    HResult = WINAGI_ERR + 602
                };
                throw wex;
            }
            // create new ingame logic
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
            Col.Add(ResNum, agResource);
            // force flags so save function will work
            agResource.IsDirty = true;
            agResource.PropsDirty = true;
            // save new logic to add it to VOL file
            agResource.Save();
            // id list needs to be updated
            blnSetIDs = false;
            // return the new logic
            return agResource;
        }

        /// <summary>
        /// Removes a logic from the game.
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(byte Index) {
            if (Col.TryGetValue(Index, out Logic value)) {
                // need to clear the directory file first
                VOLManager.Base.UpdateDirFile(value, true);
                Col.Remove(Index);
                // remove all properties from the wag file
                parent.agGameProps.DeleteSection("Logic" + Index);
                // remove ID from compiler list
                blnSetIDs = false;
            }
        }

        /// <summary>
        /// Changes the number of a logic in this game.
        /// </summary>
        /// <param name="OldLogic"></param>
        /// <param name="NewLogic"></param>
        public void Renumber(byte OldLogic, byte NewLogic) {
            Logic tmpLogic;
            int intNextNum = 0;
            string strID, strBaseID;

            if (OldLogic == NewLogic) {
                return;
            }
            // verify old number exists
            if (!Col.ContainsKey(OldLogic)) {
                throw new IndexOutOfRangeException("logic does not exist");
            }
            //verify new number is not in collection
            if (Col.ContainsKey(NewLogic)) {
                WinAGIException wex = new(LoadResString(669)) {
                    HResult = WINAGI_ERR + 669,
                };
                throw wex;
            }
            tmpLogic = Col[OldLogic];
            // remove old logic
            parent.agGameProps.DeleteSection("Logic" + OldLogic);
            Col.Remove(OldLogic);
            VOLManager.Base.UpdateDirFile(tmpLogic, true);
            // adjust ID if it is default
            if (tmpLogic.ID == "Logic" + OldLogic) {
                strID = strBaseID = "Logic" + NewLogic;
                while (NotUniqueID(strID, parent)) {
                    strID = strBaseID + "_" + intNextNum;
                    intNextNum++;
                }
                // move source file to match new ID
                try {
                    File.Delete(parent.agResDir + tmpLogic.ID + parent.agSrcFileExt);
                    File.Move(parent.agResDir + "Logic" + OldLogic + parent.agSrcFileExt, parent.agResDir + tmpLogic.ID + parent.agSrcFileExt);
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(670)) {
                        HResult = WINAGI_ERR + 670,
                    };
                    wex.Data["exception"] = e;
                    throw wex;
                }
            }
            // add it back with new number
            tmpLogic.Number = NewLogic;
            Col.Add(NewLogic, tmpLogic);
            VOLManager.Base.UpdateDirFile(tmpLogic);
            // id list needs updating
            tmpLogic.SaveProps();
            blnSetIDs = false;
        }

        /// <summary>
        /// Called by the load game methods for the initial loading of
        /// resources into logics collection.
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
        /// Sets the CRC for all logics in the collection to indicate they need
        /// to be recompiled.
        /// </summary>
        public void MarkAllAsDirty() {
            foreach (Logic tmpLogic in Col.Values) {
                tmpLogic.CompiledCRC = 0xffffffff;
                parent.WriteGameSetting("Logic" + tmpLogic.Number, "CompCRC32", "0x00", "Logics");
            }
            parent.agGameProps.Save();
        }

        /// <summary>
        /// Sets the CRC for a logic in the collection to indicate it needs
        /// to be recompiled.
        /// </summary>
        /// <param name="ResNum"></param>
        public void MarkAsDirty(byte ResNum) {
            if (Exists(ResNum)) {
                Col[ResNum].CompiledCRC = 0xffffffff;
                parent.WriteGameSetting("Logic" + ResNum.ToString(), "CompCRC32", "0x00", "Logics");
                return;
            }
        }
        
        /// <summary>
        /// A tie function to allow access to the LogCompile variable conversion function.
        /// </summary>
        /// <param name="ArgIn"></param>
        /// <param name="ArgType"></param>
        /// <param name="VarOrNum"></param>
        /// <returns></returns>
        public string ConvertArg(string ArgIn, ArgTypeEnum ArgType, bool VarOrNum = false) {
            if (parent is not null) {
                if (!parent.GlobalDefines.IsSet) {
                    parent.GlobalDefines.LoadGlobalDefines(parent.agGameDir + "globals.txt");
                }
                if (!blnSetIDs) {
                    SetResourceIDs(parent);
                }
            }
            ConvertArgument(ref ArgIn, ArgType, ref VarOrNum);
            return ArgIn;
        }

        // Collection enumerator methods
        LogicEnum GetEnumerator() {
            return new LogicEnum(Col);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<Logic> IEnumerable<Logic>.GetEnumerator() {
            return (IEnumerator<Logic>)GetEnumerator();
        }
    }

    internal class LogicEnum : IEnumerator<Logic> {
        public SortedList<byte, Logic> _logics;
        int position = -1;
        public LogicEnum(SortedList<byte, Logic> list) {
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
}
