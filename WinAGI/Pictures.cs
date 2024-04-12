using System;
using System.Collections;
using System.Collections.Generic;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// 
    /// </summary>
    public class Pictures : IEnumerable<Picture> {
        readonly AGIGame parent;
        public Pictures(AGIGame parent) {
            this.parent = parent;
            Col = [];
        }
        public SortedList<byte, Picture> Col { get; private set; }
        public Picture this[int index] {
            get {
                if (index < 0 || index > 255 || !Exists((byte)index)) {
                    throw new IndexOutOfRangeException();
                }
                return Col[(byte)index];
            }
        }

        /// <summary>
        /// Returns the number of pictures in this collection.
        /// </summary>
        public byte Count {
            get {
                return (byte)Col.Count;
            }
        }
        /// <summary>
        /// Returns the highest index in use in this collection.
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
        /// Returns true if a picture with number ResNum exists in this collection.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <returns></returns>
        public bool Exists(byte ResNum) {
            return Col.ContainsKey(ResNum);
        }

        /// <summary>
        /// Clears the collection by deleting all pictures.
        /// </summary>
        public void Clear() {
            Col = [];
        }

        /// <summary>
        /// Adds a picture to the collection. If NewPicture is null a blank
        /// picture is added, otherwise the new picture is cloned from NewPicture.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="NewPicture"></param>
        /// <returns>A reference to the newly added picture.</returns>
        public Picture Add(byte ResNum, Picture NewPicture = null) {
            Picture agResource;
            int intNextNum = 0;
            string strID, strBaseID;
            //if this Picture already exists
            if (Exists(ResNum)) {
                //resource already exists
                WinAGIException wex = new(LoadResString(602)) {
                    HResult = WINAGI_ERR + 602
                };
                throw wex;
            }
            //create new ingame picture object
            agResource = new Picture(parent, ResNum, NewPicture);
            //if no object was passed
            if (NewPicture is null) {
                //proposed ID will be default
                strID = "Picture" + ResNum;
            }
            else {
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
            // save new picture
            agResource.Save();
            // return the new picture
            return agResource;
        }

        /// <summary>
        /// Removes a picture from the collection.
        /// </summary>
        /// <param name="Index"></param>
        public void Remove(byte Index) {
            if (Col.TryGetValue(Index, out Picture value)) {
                //need to clear the directory file first
                UpdateDirFile(value, true);
                Col.Remove(Index);
                // remove all properties from the wag file
                parent.agGameProps.DeleteSection("Picture" + Index);
                //remove ID from compiler list
                Compiler.blnSetIDs = false;
            }
        }

        /// <summary>
        /// Changes the number of a picture in this collection.
        /// </summary>
        /// <param name="OldPic"></param>
        /// <param name="NewPic"></param>
        public void Renumber(byte OldPic, byte NewPic) {
            Picture tmpPic;
            int intNextNum = 0;
            bool blnUnload = false;
            string strSection, strID, strBaseID;
            if (OldPic == NewPic) {
                return;
            }
            // verify old number exists
            if (!Col.ContainsKey(OldPic)) {
                throw new IndexOutOfRangeException("picture does not exist");
            }
            //verify new number is not in collection
            if (Col.ContainsKey(NewPic)) {
                WinAGIException wex = new(LoadResString(669)) {
                    HResult = WINAGI_ERR + 669
                };
                throw wex;
            }
            tmpPic = Col[OldPic];
            // remove old picture
            parent.agGameProps.DeleteSection("Picture" + OldPic);
            Col.Remove(OldPic);
            UpdateDirFile(tmpPic, true);
            // adjust id if it is default
            if (tmpPic.ID == "Picture" + OldPic) {
                strID = strBaseID = "Picture" + NewPic;
                while (NotUniqueID(strID, parent)) {
                    strID = strBaseID + "_" + intNextNum;
                    intNextNum++;
                }
            }
            // add it back with new number
            tmpPic.Number = NewPic;
            Col.Add(NewPic, tmpPic);
            UpdateDirFile(tmpPic);
            strSection = "Picture" + NewPic;
            parent.WriteGameSetting(strSection, "ID", tmpPic.ID, "Pictures");
            parent.WriteGameSetting(strSection, "Description", tmpPic.Description);
            if (tmpPic.PriBase != 48) {
                parent.WriteGameSetting(strSection, "PriBase", tmpPic.PriBase.ToString());
            }
            // always drop background; user will have to re-add it themselves
            tmpPic.PropDirty = false;
            Compiler.blnSetIDs = false;
        }

        /// <summary>
        /// Called by the load game methods for the initial loading of
        /// resources into pictures collection.
        /// </summary>
        /// <param name="bytResNum"></param>
        /// <param name="bytVol"></param>
        /// <param name="lngLoc"></param>
        internal void InitLoad(byte bytResNum, sbyte bytVol, int lngLoc) {
            Picture newResource = new(parent, bytResNum, bytVol, lngLoc);
            newResource.Load();
            Col.Add(bytResNum, newResource);
            // leave it loaded, so error level can be addressed by loader
        }

        // Collection enumerator methods
        PictureEnum GetEnumerator() {
            return new PictureEnum(Col);
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator)GetEnumerator();
        }
        IEnumerator<Picture> IEnumerable<Picture>.GetEnumerator() {
            return (IEnumerator<Picture>)GetEnumerator();
        }
    }
    internal class PictureEnum : IEnumerator<Picture> {
        public SortedList<byte, Picture> _pictures;
        int position = -1;
        public PictureEnum(SortedList<byte, Picture> list) {
            _pictures = list;
        }
        object IEnumerator.Current => Current;
        public Picture Current {
            get {
                try {
                    return _pictures.Values[position];
                }
                catch (IndexOutOfRangeException) {
                    throw new InvalidOperationException();
                }
            }
        }
        public bool MoveNext() {
            position++;
            return (position < _pictures.Count);
        }
        public void Reset() {
            position = -1;
        }
        public void Dispose() {
            _pictures = null;
        }
    }
}
