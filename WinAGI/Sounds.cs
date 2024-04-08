using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using System.IO;
using static WinAGI.Common.Base;

namespace WinAGI.Engine {
    public class Sounds(AGIGame parent) : IEnumerable<Sound> {
        AGIGame parent = parent;

        public SortedList<byte, Sound> Col { get; private set; } = [];
        public Sound this[int index] {
            get {
                //validate index
                if (index < 0 || index > 255)
                    throw new IndexOutOfRangeException();
                return Col[(byte)index];
            }
        }
        public byte Count { get { return (byte)Col.Count; } private set { } }
        public byte Max {
            get {
                byte max = 0;
                if (Col.Count > 0)
                    max = Col.Keys[Col.Count - 1];
                return max;
            }
        }
        public bool Exists(byte ResNum) {
            //check for thsi sound in the collection
            return Col.ContainsKey(ResNum);
        }
        public void Clear() {
            Col = [];
        }
        public Sound Add(byte ResNum, Sound NewSound = null) {
            //adds a new sound to a currently open game
            Sound agResource;
            int intNextNum = 0;
            string strID, strBaseID;
            //if this sound already exists
            if (Exists(ResNum)) {
                //resource already exists
                WinAGIException wex = new(LoadResString(602)) {
                    HResult = WINAGI_ERR + 602
                };
                throw wex;
            }
            //create new ingame sound resource
            agResource = new Sound(parent, ResNum, NewSound);
            //if an object was not passed
            if ((NewSound is null)) {
                //proposed ID will be default
                strID = "Sound" + ResNum;
            }
            else {
                //get proposed id
                strID = agResource.ID;
            }
            // validate id
            strBaseID = strID;
            while (NotUniqueID(strID, parent)) {
                intNextNum++;
                strID = strBaseID + "_" + intNextNum;
            }
            //add it
            Col.Add(ResNum, agResource);
            //force flags so save function will work
            agResource.IsDirty = true;
            agResource.PropDirty = true;
            //save new sound
            agResource.Save();
            //return the object created
            return agResource;
        }
        public void Remove(byte Index) {
            //removes a sound from the game file

            // if the resource exists
            if (Col.TryGetValue(Index, out Sound value)) {
                //need to clear the directory file first
                UpdateDirFile(value, true);
                Col.Remove(Index);
                //remove all properties from the wag file
                parent.agGameProps.DeleteSection("Sound" + Index);
                //remove ID from compiler list
                Compiler.blnSetIDs = false;
            }
        }
        public void Renumber(byte OldSound, byte NewSound) {
            //renumbers a resource
            Sound tmpSound;
            int intNextNum = 0;
            bool blnUnload = false;
            string strSection, strID, strBaseID;
            //if no change
            if (OldSound == NewSound) {
                return;
            }
            //verify new number is not in collection
            if (Col.ContainsKey(NewSound)) {
                //number already in use

                WinAGIException wex = new(LoadResString(669)) {
                    HResult = WINAGI_ERR + 669
                };
                throw wex;
            }
            //get sound being renumbered
            tmpSound = Col[OldSound];

            //if not loaded,
            if (!tmpSound.Loaded) {
                // TODO: ignore errors when renumbering?
                tmpSound.Load();
                blnUnload = true;
            }

            //remove old properties
            parent.agGameProps.DeleteSection("Sound" + OldSound);

            //remove from collection
            Col.Remove(OldSound);

            //delete sound from old number in dir file
            //by calling update directory file method
            UpdateDirFile(tmpSound, true);

            //if id is default
            if (tmpSound.ID.Equals("Sound" + OldSound, StringComparison.OrdinalIgnoreCase)) {
                //change default ID to new ID
                strID = strBaseID = "Sound" + NewSound;
                while (NotUniqueID(strID, parent)) {
                    intNextNum++;
                    strID = strBaseID + "_" + intNextNum;
                }
            }
            //change number
            tmpSound.Number = NewSound;

            //add with new number
            Col.Add(NewSound, tmpSound);

            //update new sound number in dir file
            UpdateDirFile(tmpSound);

            //add properties back with new sound number
            strSection = "Sound" + NewSound;
            parent.WriteGameSetting(strSection, "ID", tmpSound.ID, "Sounds");
            parent.WriteGameSetting(strSection, "Description", tmpSound.Description);
            //
            //TODO: add rest of default property values
            //

            //force writeprop state back to false
            tmpSound.PropDirty = false;

            //unload if necessary
            if (blnUnload) {
                tmpSound.Unload();
            }
            //reset compiler list of ids
            Compiler.blnSetIDs = false;
        }

        internal void InitLoad(byte bytResNum, sbyte bytVol, int lngLoc) {
            // called by the resource loading method for the initial loading of
            // resources into logics collection

            // create new sound object
            Sound newResource = new(parent, bytResNum, bytVol, lngLoc);
            // load it
            newResource.Load();
            // add it
            Col.Add(bytResNum, newResource);
            // leave it loaded, so error level can be addressed by loader
        }

        SoundEnum GetEnumerator() {
            return new SoundEnum(Col);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<Sound> IEnumerable<Sound>.GetEnumerator() {
            return (IEnumerator<Sound>)GetEnumerator();
        }
    }
    internal class SoundEnum : IEnumerator<Sound> {
        public SortedList<byte, Sound> _sounds;
        int position = -1;
        public SoundEnum(SortedList<byte, Sound> list) {
            _sounds = list;
        }
        object IEnumerator.Current => Current;
        public Sound Current {
            get {
                try {
                    return _sounds.Values[position];
                }
                catch (IndexOutOfRangeException) {

                    throw new InvalidOperationException();
                }
            }
        }
        public bool MoveNext() {
            position++;
            return (position < _sounds.Count);
        }
        public void Reset() {
            position = -1;
        }
        public void Dispose() {
            _sounds = null;
        }
    }
}
