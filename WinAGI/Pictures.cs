using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using System.IO;
using static WinAGI.Common.Base;

namespace WinAGI.Engine {
    public class Pictures : IEnumerable<Picture> {
        internal AGIGame parent;
        public Pictures(AGIGame parent) {
            this.parent = parent;
            // create the initial Col object
            Col = [];
        }
        public SortedList<byte, Picture> Col { get; private set; }
        public Picture this[int index] {
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
            //check for thsi picture in the collection
            return Col.ContainsKey(ResNum);
        }
        public void Clear() {
            Col = [];
        }
        public Picture Add(byte ResNum, Picture NewPicture = null) {
            //adds a new picture to a currently open game
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
                //get proposed id
                strID = agResource.ID;
            }
            // validate id
            strBaseID = strID;
            while (!agResource.IsUniqueResID(strID)) {
                intNextNum++;
                strID = strBaseID + "_" + intNextNum;
            }
            //add it
            Col.Add(ResNum, agResource);
            //force flags so save function will work
            agResource.IsDirty = true;
            agResource.WritePropState = true;
            //save new picture
            agResource.Save();
            //return the object created
            return agResource;
        }
        public void Remove(byte Index) {
            //removes a picture from the game file

            // if the resource exists
            if (Col.TryGetValue(Index, out Picture value)) {
                //need to clear the directory file first
                UpdateDirFile(value, true);
                Col.Remove(Index);
                //remove all properties from the wag file
                parent.agGameProps.DeleteSection("Picture" + Index);
                //remove ID from compiler list
                Compiler.blnSetIDs = false;
            }
        }
        public void Renumber(byte OldPic, byte NewPic) {
            //renumbers a resource
            Picture tmpPic;
            int intNextNum = 0;
            bool blnUnload = false;
            string strSection, strID, strBaseID;
            //if no change
            if (OldPic == NewPic) {
                return;
            }
            //verify new number is not in collection
            if (Col.ContainsKey(NewPic)) {
                //number already in use

                WinAGIException wex = new(LoadResString(669)) {
                    HResult = WINAGI_ERR + 669
                };
                throw wex;
            }
            //get picture being renumbered
            tmpPic = Col[OldPic];

            //if not loaded,
            if (!tmpPic.Loaded) {
                tmpPic.Load();
                blnUnload = true;
            }

            //remove old properties
            parent.agGameProps.DeleteSection("Picture" + OldPic);

            //remove from collection
            Col.Remove(OldPic);

            //delete picture from old number in dir file
            //by calling update directory file method
            UpdateDirFile(tmpPic, true);

            //if id is default
            if (tmpPic.ID.Equals("Picture" + OldPic, StringComparison.OrdinalIgnoreCase)) {
                //change default ID to new ID
                strID = strBaseID = "Picture" + NewPic;
                while (!tmpPic.IsUniqueResID(strID)) {
                    intNextNum++;
                    strID = strBaseID + "_" + intNextNum;
                }
            }
            //change number
            tmpPic.Number = NewPic;

            //add with new number
            Col.Add(NewPic, tmpPic);

            //update new picture number in dir file
            UpdateDirFile(tmpPic);

            //add properties back with new picture number
            strSection = "Picture" + NewPic;
            parent.WriteGameSetting(strSection, "ID", tmpPic.ID, "Pictures");
            parent.WriteGameSetting(strSection, "Description", tmpPic.Description);
            //
            //TODO: add rest of default property values
            //

            //force writeprop state back to false
            tmpPic.WritePropState = false;

            //unload if necessary
            if (blnUnload) {
                tmpPic.Unload();
            }
            //reset compiler list of ids
            Compiler.blnSetIDs = false;
        }

        internal void InitLoad(byte bytResNum, sbyte bytVol, int lngLoc) {
            // called by the resource loading method for the initial loading of
            // resources into logics collection

            // create new logic object
            Picture newResource = new(parent, bytResNum, bytVol, lngLoc);
            // try to load it
            try {
                newResource.Load();
            }
            catch (Exception) {
                // throw it
                throw;
            }
            finally {
                // add it
                Col.Add(bytResNum, newResource);
                // unload it
                newResource.Unload();
            }
        }

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
