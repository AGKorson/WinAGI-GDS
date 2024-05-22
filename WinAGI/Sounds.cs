using System;
using System.Collections;
using System.Collections.Generic;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that holds all the sounds in an AGI game.
    /// </summary>
    public class Sounds : IEnumerable<Sound> {
        readonly AGIGame parent;

        internal Sounds(AGIGame parent) {
            this.parent = parent;
        }
        public SortedList<byte, Sound> Col { get; private set; } = [];
        public Sound this[int index] {
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
        /// Returns the highest index in use in the sounds collection. 
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
        /// Returns true if a sound with number ResNum exists in this game.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <returns></returns>
        public bool Contains(byte ResNum) {
            return Col.ContainsKey(ResNum);
        }

        /// <summary>
        /// Removes all sounds from the game.
        /// </summary>
        public void Clear() {
            Col = [];
        }

        /// <summary>
        /// Adds a sound to the game. If NewSound is null a blank sound is 
        /// added, otherwise the added sound is cloned from NewSound.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="NewSound"></param>
        /// <returns></returns>
        public Sound Add(byte ResNum, Sound NewSound = null) {
            Sound agResource;
            int intNextNum = 0;
            string strID, strBaseID;
            if (Contains(ResNum)) {
                WinAGIException wex = new(LoadResString(602)) {
                    HResult = WINAGI_ERR + 602
                };
                throw wex;
            }
            //create new ingame sound
            agResource = new Sound(parent, ResNum, NewSound);
            if ((NewSound is null)) {
                strID = "Sound" + ResNum;
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
            // save new sound to add it to VOL file
            agResource.Save();
            // id list needs to be updated
            LogicCompiler.blnSetIDs = false;
            // return the new sound
            return agResource;
        }

        /// <summary>
        /// Removes a sound from the game.
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(byte Index) {
            if (Col.TryGetValue(Index, out Sound value)) {
                // need to clear the directory file first
                VOLManager.Base.UpdateDirFile(value, true);
                Col.Remove(Index);
                // remove all properties from the wag file
                parent.agGameProps.DeleteSection("Sound" + Index);
                // remove ID from compiler list
                LogicCompiler.blnSetIDs = false;
            }
        }

        /// <summary>
        /// Changes the number of a sound in this game.
        /// </summary>
        /// <param name="OldSound"></param>
        /// <param name="NewSound"></param>
        public void Renumber(byte OldSound, byte NewSound) {
            Sound tmpSound;
            int intNextNum = 0;
            string strID, strBaseID;

            if (OldSound == NewSound) {
                return;
            }
            // verify old number exists
            if (!Col.ContainsKey(OldSound)) {
                throw new IndexOutOfRangeException("sound does not exist");
            }
            //verify new number is not in collection
            if (Col.ContainsKey(NewSound)) {
                WinAGIException wex = new(LoadResString(669)) {
                    HResult = WINAGI_ERR + 669
                };
                throw wex;
            }
            tmpSound = Col[OldSound];
            // remove old sound
            parent.agGameProps.DeleteSection("Sound" + OldSound);
            Col.Remove(OldSound);
            VOLManager.Base.UpdateDirFile(tmpSound, true);
            // adjust id if it is default
            if (tmpSound.ID =="Sound" + OldSound) {
                strID = strBaseID = "Sound" + NewSound;
                while (NotUniqueID(strID, parent)) {
                    strID = strBaseID + "_" + intNextNum;
                    intNextNum++;
                }
            }
            // add it back with new number
            tmpSound.Number = NewSound;
            Col.Add(NewSound, tmpSound);
            VOLManager.Base.UpdateDirFile(tmpSound);
            tmpSound.SaveProps();
            // id list needs updating
            LogicCompiler.blnSetIDs = false;
        }

        /// <summary>
        /// Called by the load game methods for the initial loading of
        /// resources into sounds collection.
        /// </summary>
        /// <param name="bytResNum"></param>
        /// <param name="bytVol"></param>
        /// <param name="lngLoc"></param>
        internal void InitLoad(byte bytResNum, sbyte bytVol, int lngLoc) {
            Sound newResource = new(parent, bytResNum, bytVol, lngLoc);
            newResource.Load();
            Col.Add(bytResNum, newResource);
            // leave it loaded, so error level can be addressed by loader
        }

        #region Enumeration
        SoundEnum GetEnumerator() {
            return new SoundEnum(Col);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<Sound> IEnumerable<Sound>.GetEnumerator() {
            return (IEnumerator<Sound>)GetEnumerator();
        }

        /// <summary>
        /// Implements enumeration for the Sounds class.
        /// </summary>
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
        #endregion
    }
}
