using System;
using System.Collections;
using System.Collections.Generic;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that holds all the sound resources in an AGI game.
    /// </summary>
    public class Sounds : IEnumerable<Sound> {
        #region Members
        readonly AGIGame parent;
        #endregion

        #region Constructors
        internal Sounds(AGIGame parent) {
            this.parent = parent;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the list of sounds in this game.
        /// </summary>
        public SortedList<int, Sound> Col { get; private set; } = [];

        /// <summary>
        /// Gets the sound with the specified index value from this list of sounds.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public Sound this[int index] {
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
        public int Count {
            get {
                return Col.Count;
            }
        }

        /// <summary>
        /// Gets the highest index in use in this sounds collection. 
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
        /// resources into sounds collection.
        /// </summary>
        /// <param name="bytResNum"></param>
        /// <param name="bytVol"></param>
        /// <param name="lngLoc"></param>
        internal void InitLoad(byte bytResNum, sbyte bytVol, int lngLoc) {
            Sound newResource = new(parent, bytResNum, bytVol, lngLoc);
            // for initial load, skip the output build
            newResource.Load(true);
            Col.Add(bytResNum, newResource);
            // leave it loaded, so error level can be addressed by loader
        }

        /// <summary>
        /// Returns true if a sound with number the specified number exists in this game.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <returns></returns>
        public bool Contains(int ResNum) {
            return Col.ContainsKey(ResNum);
        }

        /// <summary>
        /// Adds a sound to the game. If NewSound is null a blank sound is 
        /// added, otherwise the added sound is cloned from NewSound.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="NewSound"></param>
        /// <returns>A reference to the newly added sound.</returns>
        public Sound Add(byte ResNum, Sound NewSound = null) {
            Sound agResource;
            int intNextNum = 0;
            string strID, strBaseID;
            if (Contains(ResNum)) {
                WinAGIException wex = new(EngineResourceByNum(520)) {
                    HResult = WINAGI_ERR + 520
                };
                throw wex;
            }
            // create new ingame sound
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
            // force flags so save function will work
            agResource.IsChanged = true;
            agResource.PropsChanged = true;
            // save new sound to add it to VOL file
            agResource.Save();
            FanLogicCompiler.setIDs = false;
            return agResource;
        }

        /// <summary>
        /// Removes the specified sound from this game.
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(byte Index) {
            if (Col.TryGetValue(Index, out Sound value)) {
                // need to clear the directory file first
                VOLManager.UpdateDirFile(value, true);
                Col.Remove(Index);
                // remove all properties from the wag file
                parent.agGameProps.DeleteSection("Sound" + Index);
                // remove ID from compiler list
                FanLogicCompiler.setIDs = false;
            }
        }

        /// <summary>
        /// Removes all sounds from the game. Does not update WAG file.
        /// </summary>
        internal void Clear() {
            Col = [];
        }

        /// <summary>
        /// Changes the index number of a sound in this game.
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
            if (!Col.TryGetValue(OldSound, out tmpSound)) {
                throw new IndexOutOfRangeException("sound does not exist");
            }
            // verify new number is not in collection
            if (Col.ContainsKey(NewSound)) {
                WinAGIException wex = new(EngineResourceByNum(531)) {
                    HResult = WINAGI_ERR + 531
                };
                throw wex;
            }
            // remove old sound
            parent.agGameProps.DeleteSection("Sound" + OldSound);
            Col.Remove(OldSound);
            VOLManager.UpdateDirFile(tmpSound, true);
            // adjust id if it is default
            if (tmpSound.ID.ToLower() == "sound" + OldSound) {
                strID = strBaseID = tmpSound.ID[..5] + NewSound;
                while (NotUniqueID(strID, parent)) {
                    strID = strBaseID + "_" + intNextNum;
                    intNextNum++;
                }
                tmpSound.ID = strID;
            }
            // add it back with new number
            tmpSound.Number = NewSound;
            Col.Add(NewSound, tmpSound);
            VOLManager.UpdateDirFile(tmpSound);
            tmpSound.SaveProps();
            FanLogicCompiler.setIDs = false;
        }
        #endregion

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
            public SortedList<int, Sound> _sounds;
            int position = -1;
            public SoundEnum(SortedList<int, Sound> list) {
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
