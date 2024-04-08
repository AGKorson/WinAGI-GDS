using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// 
    /// </summary>
    public class Logics : IEnumerable<Logic> {
        readonly AGIGame parent;
        internal string mSourceFileExt = "";
        internal Logics(AGIGame parent) {
            this.parent = parent;
            // create the initial Col object
            Col = [];
            mSourceFileExt = Compiler.DefaultSrcExt;
        }
        public SortedList<byte, Logic> Col { get; private set; }
        public Logic this[int index] {
            get {
                //validate index
                if (index < 0 || index > 255)
                    throw new IndexOutOfRangeException();
                return Col[(byte)index];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public byte Count { 
            get { 
                return (byte)Col.Count;
            }
        }
        
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public string SourceFileExt {
            get {
                return mSourceFileExt;
            }
            set {
                // lower case, max four characters, not null
                if (value.Length == 0) {
                    // use default
                    mSourceFileExt = Compiler.DefaultSrcExt;
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
                mSourceFileExt = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ResNum"></param>
        /// <returns></returns>
        public bool Exists(byte ResNum) {
            // check for this logic in the collection
            return Col.ContainsKey(ResNum);
        }
        
        /// <summary>
        /// 
        /// </summary>
        internal void Clear() {
            Col = [];
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="NewLogic"></param>
        /// <returns></returns>
        public Logic Add(byte ResNum, Logic NewLogic = null) {
            //adds a new logic to a currently open game
            Logic agResource;
            int intNextNum = 0;
            string strID, strBaseID;
            // if this Logic already exists
            if (Exists(ResNum)) {
                // resource already exists
                WinAGIException wex = new(LoadResString(602)) {
                    HResult = WINAGI_ERR + 602
                };
                throw wex;
            }
            // create new ingame logic
            agResource = new Logic(parent, ResNum, NewLogic);
            // if an object was not passed
            if (NewLogic is null) {
                // proposed ID will be default
                strID = "Logic" + ResNum;
            }
            else {
                // get proposed id
                strID = agResource.ID;
            }
            // validate id
            strBaseID = strID;
            while (NotUniqueID(strID, parent)) {
                intNextNum++;
                strID = strBaseID + "_" + intNextNum;
            }
            Col.Add(ResNum, agResource);
            // force flags so save function will work
            agResource.IsDirty = true;
            agResource.PropDirty = true;
            agResource.Save();
            // return the object created
            return agResource;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(byte Index) {
            //removes a logic from the game file

            // if the resource exists
            if (Col.TryGetValue(Index, out Logic value)) {
                // need to clear the directory file first
                UpdateDirFile(value, true);
                Col.Remove(Index);
                //remove all properties from the wag file
                parent.agGameProps.DeleteSection("Logic" + Index);
                //remove ID from compiler list
                Compiler.blnSetIDs = false;
            }
        }

        /// <summary>
        /// Changes the number of a logic.
        /// </summary>
        /// <param name="OldLogic"></param>
        /// <param name="NewLogic"></param>
        public void Renumber(byte OldLogic, byte NewLogic) {
            // TODO: why is renumber a collection method? should be a resource method
            Logic tmpLogic;
            int intNextNum = 0;
            bool blnUnload = false;
            string strSection, strID, strBaseID;
            if (OldLogic == NewLogic) {
                return;
            }
            // verify old number exists
            if (!Col.ContainsKey(OldLogic)) {
                throw new IndexOutOfRangeException("logic does not exist");
            }
            //verify new number is not in collection
            if (Col.ContainsKey(NewLogic)) {
                //number already in use
                WinAGIException wex = new(LoadResString(669)) {
                    HResult = WINAGI_ERR + 669,
                };
                throw wex;
            }
            // get logic being renumbered
            tmpLogic = Col[OldLogic];
            // remove old properties
            parent.agGameProps.DeleteSection("Logic" + OldLogic);
            // remove from collection
            Col.Remove(OldLogic);
            UpdateDirFile(tmpLogic, true);
            if (tmpLogic.ID == "Logic" + OldLogic) {
                // change default ID to new ID
                strID = strBaseID = "Logic" + NewLogic;
                while (NotUniqueID(strID, parent)) {
                    strID = strBaseID + "_" + intNextNum;
                    intNextNum++;
                }
                try {
                    // get rid of existing file with same name as new logic
                    File.Delete(parent.agResDir + tmpLogic.ID + agSrcFileExt);
                    // rename sourcefile
                    File.Move(parent.agResDir + "Logic" + OldLogic + agSrcFileExt, parent.agResDir + tmpLogic.ID + agSrcFileExt);
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(670)) {
                        HResult = WINAGI_ERR + 670,
                    };
                    wex.Data["exception"] = e;
                    throw wex;
                }
            }
            // change number
            tmpLogic.Number = NewLogic;
            // add with new number
            Col.Add(NewLogic, tmpLogic);
            // update new logic number in dir file
            UpdateDirFile(tmpLogic);
            // add properties back with new logic number
            strSection = "Logic" + NewLogic;
            parent.WriteGameSetting(strSection, "ID", tmpLogic.ID, "Logics");
            parent.WriteGameSetting(strSection, "Description", tmpLogic.Description);
            parent.WriteGameSetting(strSection, "CRC32", "0x" + tmpLogic.CRC.ToString("x8"));
            parent.WriteGameSetting(strSection, "CompCRC32", "0x" + (tmpLogic.CompiledCRC.ToString("x8")));
            parent.WriteGameSetting(strSection, "IsRoom", tmpLogic.IsRoom.ToString());
            // force writeprop state back to false
            tmpLogic.PropDirty = false;
            // reset compiler list of ids
            Compiler.blnSetIDs = false;
        }

        internal void InitLoad(byte bytResNum, sbyte bytVol, int lngLoc) {
            // called by the resource loading method for the initial loading of
            // resources into logics collection

            // create new logic object
            Logic newResource = new(parent, bytResNum, bytVol, lngLoc);
            newResource.LoadNoSource();
            // add it
            Col.Add(bytResNum, newResource);
            // leave it loaded, so error level can be addressed by loader
        }
        
        public void MarkAllAsDirty() {
            foreach (Logic tmpLogic in Col.Values) {
                tmpLogic.CompiledCRC = 0xffffffff;
                parent.WriteGameSetting("Logic" + tmpLogic.Number, "CompCRC32", "0x00", "Logics");
            }
            parent.agGameProps.Save();
        }
        
        public void MarkAsDirty(byte ResNum) {
            //mark this logic as dirty by setting its compiledCRC value to zero
            //(ignore if resource is not valid)
            if (Exists(ResNum)) {
                Col[ResNum].CompiledCRC = 0xffffffff;
                parent.WriteGameSetting("Logic" + ResNum.ToString(), "CompCRC32", "0x00", "Logics");
                return;
            }
        }
        
        public string ConvertArg(string ArgIn, ArgTypeEnum ArgType, bool VarOrNum = false) {
            //tie function to allow access to the LogCompile variable conversion function
            //if in a game
            if (parent is not null) {
                //initialize global defines
                if (!parent.GlobalDefines.IsSet) {
                    parent.GlobalDefines.LoadGlobalDefines();
                }
                //if ids not set yet
                if (!Compiler.blnSetIDs) {
                    Compiler.SetResourceIDs(parent);
                }
            }
            //convert argument
            Compiler.ConvertArgument(ref ArgIn, ArgType, ref VarOrNum);
            //return it
            return ArgIn;
        }

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
