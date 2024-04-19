using NAudio.Wave;
using System;
using System.IO;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// Internal class used to track current DIR and VOL information during game compilation.
    /// </summary>
    internal class VOLManager {
        internal FileStream fsVOL, fsDIR;
        internal AGIGame parent;

        internal VOLManager (AGIGame game) {
            // for dir files, use arrays during compiling process
            // then build the dir files at the end
            DIRData = new byte[4, 768];
            for (int i = 0; i < 4; i++) {
                for (int j = 0; i < 768; i++) {
                    DIRData[i, j] = 255;
                }
            }
            parent = game;
            NewDir = "";
            Loc = 0;
            Index = 0;
            Count = 1;
            // file access members will be intialized by calling code
        }
        public byte[,] DIRData { get; set; }
        public FileStream DIRFile {
            get => fsDIR;
            set {
                fsDIR = value;
                DIRWriter = new BinaryWriter(fsDIR);
            }
        }
        public FileStream VOLFile { get => fsVOL;
            set {
                fsVOL = value;
                VOLWriter = new BinaryWriter(fsVOL);
                VOLReader = new BinaryReader(fsVOL);
            }
        }
        public BinaryWriter DIRWriter { get; private set; }
        // TODO: need way to determine which DIR is active!
        public BinaryWriter VOLWriter { get; private set; }
        public BinaryReader VOLReader { get; private set; }
        public string NewDir { get; set; }
        public int Loc { get; set; }
        public int Index { get; set; }
        public int Count { get; private set; }
        public bool IsV3 { get; set; }

        internal void Clear() {
            VOLFile?.Dispose();
            VOLWriter?.Dispose();
            VOLReader?.Dispose();
            DIRFile?.Dispose();
            DIRWriter?.Dispose();
        }
        internal void AddToVol(AGIResource AddRes, bool Version3, bool compileResCol = false, sbyte lngVol = -1, int lngLoc = -1) {
            // this method adds a resource to a VOL file
            //
            // if the compileResCol flag is true, it adds the resource
            // to the specified vol file at the specified location
            // the DIR file is not updated; that is done by the
            // CompileGame method (which calls this method)
            //
            // if the compileResCol flag is false, it finds the first
            // available spot for the resource, and adds it there
            // the method will add the resource at a new location based
            // on first open position; it will not delete the resource
            // data from its old position (but the area will be available
            // for future use by another resource) and then it updates the DIR file
            //
            // only resources that are in a game can be added to a VOL file
            byte[] ResHeader;
            string strID;

            if (!compileResCol) {
                try {
                    // get vol number and location where there is room for this resource
                    FindFreeVOLSpace(AddRes);
                }
                catch (Exception) {
                    //pass it along
                    throw;
                }
                lngLoc = AddRes.Loc;
                lngVol = AddRes.Volume;
            }
            // build header
            ResHeader = new byte[5];
            ResHeader[0] = 0x12;
            ResHeader[1] = 0x34;
            ResHeader[2] = (byte)lngVol;
            ResHeader[3] = (byte)(AddRes.Size % 256);
            ResHeader[4] = (byte)(AddRes.Size / 256);
            // if the resource is a version 3 resource,
            if (Version3) {
                //adjust header size
                Array.Resize(ref ResHeader, 7);
                ResHeader[5] = ResHeader[3];
                ResHeader[6] = ResHeader[4];
                strID = AddRes.parent.agGameID;
            }
            else {
                strID = "";
            }

            //if not adding to a new vol file
            if (!compileResCol) {
                //save the resource into the vol file
                fsVOL = new FileStream(AddRes.parent.agGameDir + strID + "VOL." + lngVol.ToString(), FileMode.Open);
            }
            try {
                fsVOL.Seek(lngLoc, SeekOrigin.Begin);
                // add header to vol file
                VOLWriter.Write(ResHeader, 0, AddRes.parent.agIsVersion3 ? 7 : 5);
                // add resource data to vol file
                VOLWriter.Write(AddRes.Data.AllData, 0, AddRes.Data.Length);
            }
            catch (Exception e) {
                // if not compiling, close volfile
                if (!compileResCol) {
                    fsVOL.Dispose();
                    VOLWriter.Dispose();
                }
                WinAGIException wex = new(LoadResString(638)) {
                    HResult = WINAGI_ERR + 638,
                };
                wex.Data["exception"] = e;
                wex.Data["ID"] = AddRes.ID;
                throw wex;
            }
            //if adding to a new vol file
            if (compileResCol) {
                //increment loc pointer
                lngLoc = lngLoc + AddRes.Size + (Version3 ? 7 : 5);
            }
            else {
                try {
                    //update location in dir file
                    UpdateDirFile(AddRes);
                }
                catch (Exception) {
                    //only error that will be returned is expandv3dir error
                    //pass it along
                    throw;
                }
            }
        }

        private void AddResInfo(sbyte ResVOL, int ResLOC, int ResSize, int[,] VOLLoc, int[,] VOLSize) {
            // TODO: replace this mess with Sorted Lists!
            //adds TempRes to volume loc/size arrays, sorted by LOC
            //1st element of each VOL section is number of entries for that VOL
            int i = 0, j = 0;
            //if this is first resource for this volume
            if (VOLLoc[ResVOL, 0] == 0) {
                //add it to first position
                VOLLoc[ResVOL, 1] = ResLOC;
                VOLSize[ResVOL, 1] = ResSize;
                //increment Count for this volume
                VOLLoc[ResVOL, 0] = 1;
            }
            else {
                //find the place where this vol belongs
                for (i = 1; i <= VOLLoc[ResVOL, 0]; i++) {
                    //if it comes BEFORE this position,
                    if (ResLOC < VOLLoc[ResVOL, i]) {
                        //i contains position where new volume belongs
                        break;
                    }
                }

                //first, increment Count
                VOLLoc[ResVOL, 0]++; // = VOLLoc[ResVOL, 0] + 1;

                //now move all items ABOVE i up one space
                for (j = VOLLoc[ResVOL, 0]; j > i; j--) {
                    VOLLoc[ResVOL, j] = VOLLoc[ResVOL, j - 1];
                    VOLSize[ResVOL, j] = VOLSize[ResVOL, j - 1];
                }

                //store new data at position i
                VOLLoc[ResVOL, i] = ResLOC;
                VOLSize[ResVOL, i] = ResSize;
            }
        }

        internal void UpdateDirFile(AGIResource UpdateResource, bool Remove = false) {
            //this method updates the DIR file with the volume and location
            //of a resource
            //if Remove option passed as true,
            //the resource is treated as 'deleted' and
            // 0xFFFFFF is written in the resource's DIR file place holder

            //strategy:
            //if deleting--
            //    is the the dir larger than the new max?
            //    yes: compress the dir
            //    no:  overwrite with 0xFFs
            //
            //if adding--
            //    is the dir too small?
            //    yes: expand the dir
            //    no:  overwrite with the data

            //NOTE: directories inside a V3 dir file are in this order: LOGIC, PICTURE, VIEW, SOUND
            //the ResType enumeration is in this order: LOGIC, PICTURE, SOUND, VIEW
            //because of the switch between VIEW and SOUND, can't use a formula to calculate
            //directory offsets
            string strDirFile;
            byte[] bytDIR, DirByte = new byte[2];
            int intMax = 0, intOldMax;
            int lngDirOffset = 0, lngDirEnd = 0, i, lngStart, lngStop;

            if (Remove) {
                //resource marked for deletion
                DirByte[0] = 0xFF;
                DirByte[1] = 0xFF;
                DirByte[2] = 0xFF;
            }
            else {
                //calculate directory bytes
                DirByte[0] = (byte)((UpdateResource.Volume << 4) + ((uint)UpdateResource.Loc >> 16));
                DirByte[1] = (byte)((UpdateResource.Loc % 0x10000) >> 8);
                DirByte[2] = (byte)(UpdateResource.Loc % 0x100);
            }

            //what is current max for this res type?
            switch (UpdateResource.ResType) {
            case AGIResType.rtLogic:
                intMax = UpdateResource.parent.agLogs.Max;
                break;
            case AGIResType.rtPicture:
                intMax = UpdateResource.parent.agPics.Max;
                break;
            case AGIResType.rtSound:
                intMax = UpdateResource.parent.agSnds.Max;
                break;
            case AGIResType.rtView:
                intMax = UpdateResource.parent.agViews.Max;
                break;
            }
            //open the correct dir file, store in a temp array
            if (UpdateResource.parent.agIsVersion3) {
                strDirFile = UpdateResource.parent.agGameDir + UpdateResource.parent.agGameID + "DIR";
            }
            else {
                strDirFile = UpdateResource.parent.agGameDir + ResTypeAbbrv[(int)UpdateResource.ResType] + "DIR";
            }
            // verify file exists
            if (!File.Exists(strDirFile)) {
                WinAGIException wex = new(LoadResString(524).Replace(ARG1, strDirFile)) {
                    HResult = WINAGI_ERR + 524
                };
                wex.Data["missingfile"] = strDirFile;
                throw wex;
            }
            // check for readonly
            if ((File.GetAttributes(strDirFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                WinAGIException wex = new(LoadResString(700).Replace(ARG1, strDirFile)) {
                    HResult = WINAGI_ERR + 700,
                };
                wex.Data["badfile"] = strDirFile;
                throw wex;
            }
            try {
                using (fsDIR = new FileStream(strDirFile, FileMode.Open)) {
                    bytDIR = new byte[fsDIR.Length];
                    fsDIR.Read(bytDIR, 0, (int)fsDIR.Length);
                }
            }
            catch (Exception e) {
                // "can't open DIR for updating"
                WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = e;
                wex.Data["badfile"] = strDirFile;
                throw wex;
            }
            //calculate old max and offset (for v3 files)
            if (UpdateResource.parent.agIsVersion3) {
                //calculate directory offset
                switch (UpdateResource.ResType) {
                case AGIResType.rtLogic:
                    lngDirOffset = 8;
                    lngDirEnd = (bytDIR[3] << 8) + bytDIR[2];
                    break;
                case AGIResType.rtPicture:
                    lngDirOffset = (bytDIR[3] << 8) + bytDIR[2];
                    lngDirEnd = (bytDIR[5] << 8) + bytDIR[4];
                    break;
                case AGIResType.rtView:
                    lngDirOffset = (bytDIR[5] << 8) + bytDIR[4];
                    lngDirEnd = (bytDIR[7] << 8) + bytDIR[6];
                    break;
                case AGIResType.rtSound:
                    lngDirOffset = (bytDIR[7] << 8) + bytDIR[6];
                    lngDirEnd = bytDIR.Length;
                    break;
                }
                intOldMax = (lngDirEnd - lngDirOffset) / 3 - 1;
            }
            else {
                lngDirEnd = (bytDIR.Length);
                intOldMax = lngDirEnd / 3 - 1;
                lngDirOffset = 0;
            }

            // if it fits (doesn't matter if inserting or deleting)
            if ((Remove && (intMax >= intOldMax)) || (!Remove && (intOldMax >= intMax))) {
                //adjust offset for resnum
                lngDirOffset += 3 * UpdateResource.Number;
                //just insert the new data, and save the file
                try {
                    using (fsDIR = new FileStream(strDirFile, FileMode.Open)) {
                        _ = fsDIR.Seek(lngDirOffset, SeekOrigin.Begin);
                        fsDIR.Write(bytDIR);
                    }
                    return;
                }
                catch (Exception e) {
                    //throw new Exception("unable to update the DIR file...");
                    WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                        HResult = WINAGI_ERR + 502,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["badfile"] = strDirFile;
                    throw wex;
                }
            }
            // size has changed!
            if (Remove) {
                //must be shrinking
                //if v2, just redim the array
                if (!UpdateResource.parent.agIsVersion3) {
                    Array.Resize(ref bytDIR, (intMax + 1) * 3);
                }
                else {
                    //if restype is sound, we also can just truncate the file
                    if (UpdateResource.ResType == AGIResType.rtSound) {
                        Array.Resize(ref bytDIR, bytDIR.Length - 3 * (intOldMax - intMax));
                    }
                    else {
                        // we need to move data from the current directory's max
                        // backwards to compress the directory
                        // start with resource just after new max; then move all bytes down
                        lngStart = lngDirOffset + intMax * 3 + 3;
                        lngStop = bytDIR.Length - 3 * (intOldMax - intMax);
                        for (i = lngStart; i < lngStop; i++) {
                            bytDIR[i] = bytDIR[i + 3 * (intOldMax - intMax)];
                        }
                        // now shrink the array
                        Array.Resize(ref bytDIR, bytDIR.Length - 3 * (intOldMax - intMax));
                        // then we need to update affected offsets
                        // move snddir offset first
                        lngDirOffset = (bytDIR[7] << 8) + bytDIR[6];
                        lngDirOffset -= 3 * (intOldMax - intMax);
                        bytDIR[7] = (byte)(lngDirOffset >> 8);
                        bytDIR[6] = (byte)(lngDirOffset % 0x100);
                        //if resource is a view, we are done
                        if (UpdateResource.ResType != AGIResType.rtView) {
                            //move view offset
                            lngDirOffset = (bytDIR[5] << 8) + bytDIR[4];
                            lngDirOffset -= 3 * (intOldMax - intMax);
                            bytDIR[5] = (byte)(lngDirOffset >> 8);
                            bytDIR[4] = (byte)(lngDirOffset % 0x100);
                            //if resource is a pic, we are done
                            if (UpdateResource.ResType != AGIResType.rtPicture) {
                                //move picture offset
                                lngDirOffset = (bytDIR[3] << 8) + bytDIR[2];
                                lngDirOffset -= 3 * (intOldMax - intMax);
                                bytDIR[3] = (byte)(lngDirOffset >> 8);
                                bytDIR[2] = (byte)(lngDirOffset % 0x100);
                            }
                        }
                    }
                }
                try {
                    //delete the existing file
                    File.Delete(strDirFile);
                    //now save the file
                    using (fsDIR = new FileStream(strDirFile, FileMode.OpenOrCreate)) {
                        fsDIR.Write(bytDIR, 0, bytDIR.Length);
                    }
                }
                catch (Exception e) {
                    // throw new Exception("unable to update the DIR file...");
                    WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                        HResult = WINAGI_ERR + 502,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["badfile"] = strDirFile;
                    throw wex;
                }
            }
            else {
                //must be expanding
                Array.Resize(ref bytDIR, bytDIR.Length + 3 * (intMax - intOldMax));
                //if v2, add ffs to fill gap up to the last entry
                if (!UpdateResource.parent.agIsVersion3) {
                    lngStart = lngDirEnd;
                    lngStop = lngDirEnd + 3 * (intMax - intOldMax - 1) - 1;
                    for (i = lngStart; i <= lngStop; i++) {
                        bytDIR[i] = 0xFF;
                    }
                    //add dir data to end
                    bytDIR[lngStop + 1] = DirByte[0];
                    bytDIR[lngStop + 2] = DirByte[1];
                    bytDIR[lngStop + 3] = DirByte[2];
                }
                else {
                    //if expanding the sound dir, just fill it in with FF//s
                    if (UpdateResource.ResType == AGIResType.rtSound) {
                        lngStart = lngDirEnd;
                        lngStop = lngDirEnd + 3 * (intMax - intOldMax - 1) - 1;
                        for (i = lngStart; i <= lngStop; i++) {
                            bytDIR[i] = 0xFF;
                        }
                        //add dir data to end
                        bytDIR[lngStop + 1] = DirByte[0];
                        bytDIR[lngStop + 2] = DirByte[1];
                        bytDIR[lngStop + 3] = DirByte[2];
                    }
                    else {
                        //move data to make room for inserted resource
                        lngStop = bytDIR.Length - 1;
                        lngStart = lngDirEnd + 3 * (intMax - intOldMax);
                        for (i = lngStop; i > lngStart; i--) {
                            bytDIR[i] = bytDIR[i - 3 * (intMax - intOldMax)];
                        }
                        //insert ffs, up to insert location
                        lngStop = lngStart - 4;
                        lngStart = lngStop - 3 * (intMax - intOldMax - 1);
                        for (i = lngStart; i <= lngStop; i++) {
                            bytDIR[i] = 0xFF;
                        }
                        //add dir data to end
                        bytDIR[lngStop + 1] = DirByte[0];
                        bytDIR[lngStop + 2] = DirByte[1];
                        bytDIR[lngStop + 3] = DirByte[2];

                        //then adjust the offsets
                        //move snddir offset first
                        lngDirOffset = (bytDIR[7] << 8) + bytDIR[6];
                        lngDirOffset += 3 * (intMax - intOldMax);
                        bytDIR[7] = (byte)(lngDirOffset >> 8);
                        bytDIR[6] = (byte)(lngDirOffset % 0x100);
                        //if resource is a view, we are done
                        if (UpdateResource.ResType != AGIResType.rtView) {
                            //move view offset
                            lngDirOffset = (byte)((bytDIR[5] << 8) + bytDIR[4]);
                            lngDirOffset += 3 * (intMax - intOldMax);
                            bytDIR[5] = (byte)(lngDirOffset >> 8);
                            bytDIR[4] = (byte)(lngDirOffset % 0x100);
                            //if resource is a pic, we are done
                            if (UpdateResource.ResType != AGIResType.rtPicture) {
                                //move picture offset
                                lngDirOffset = (byte)((bytDIR[3] << 8) + bytDIR[2]);
                                lngDirOffset += 3 * (intMax - intOldMax);
                                bytDIR[3] = (byte)(lngDirOffset >> 8);
                                bytDIR[2] = (byte)(lngDirOffset % 0x100);
                            }
                        }
                    }
                }
                // now save the file
                try {
                    using (fsDIR = new FileStream(strDirFile, FileMode.Open)) {
                        fsDIR.Write(bytDIR, 0, bytDIR.Length);
                    }
                }
                catch (Exception e) {
                    // throw new Exception("unable to update the DIR file...");
                    WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                        HResult = WINAGI_ERR + 502,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["badfile"] = strDirFile;
                    throw wex;
                }
            }
        }

        /// <summary>
        /// This method is used by the game compiler to find the next valid location for a resource
        /// to be added to the VOL files It checks the current VOL file to determine if there is
        /// room for a resource. If not, it closes current VOL file, and opens next one.
        /// </summary>
        /// <param name="resource"></param>
        internal void ValidateVolAndLoc(AGIResource resource, string gamedir) {
            int i = 0, lngMaxVol = 0;
            // check if resource is too long
            if (Loc + resource.Size > MAX_VOLSIZE || (Index == 0 && (Loc + resource.Size > resource.parent.agMaxVol0))) {
                //set maxvol count to 4 or 15, depending on version
                lngMaxVol = resource.parent.agIsVersion3 ? 15 : 4;

                //close current vol
                VOLFile.Dispose();
                VOLWriter.Dispose();

                //first check previous vol files, to see if there is room at end of one of those
                for (i = 0; i <= lngMaxVol; i++) {
                    try {
                        //open this file (if the file doesn't exist, this will create it
                        //and it will then be set as the next vol, with pos=0
                        VOLFile = new FileStream(gamedir + "NEW_VOL." + i, FileMode.OpenOrCreate);
                    }
                    catch (Exception e) {
                        Clear();
                        //also, compiler should check for this error, as it is fatal

                        WinAGIException wex = new(LoadResString(640)) {
                            HResult = WINAGI_ERR + 640
                        };
                        wex.Data["exception"] = e;
                        wex.Data["ID"] = resource.ID;
                        throw wex;
                    }
                    //is there room at the end of this file?
                    if ((i > 0 && (VOLFile.Length + resource.Size <= MAX_VOLSIZE)) || (VOLFile.Length + resource.Size <= resource.parent.agMaxVol0)) {
                        //if so, set pointer to end of the file, and exit
                        Index = (sbyte)i;
                        Loc = (int)VOLFile.Length;
                        return;
                    }
                } //Next i

                //if no volume found, we//ve got a problem...
                //raise error!
                //also, compiler should check for this error, as it is fatal

                WinAGIException wex1 = new(LoadResString(593)) {
                    HResult = WINAGI_ERR + 593
                };
                throw wex1;
            }
        }

        private void FindFreeVOLSpace(AGIResource NewResource) {
            //this method will try to find a volume and location to store a resource
            //if a resource is being updated, it will have its volume set to 255

            //sizes are adjusted to include the header that AGI uses at the beginning of each
            //resource; 5 bytes for v2 and 7 bytes for v3

            int[,] resLoc = new int[15, 1023];
            int[,] resSize = new int[15, 1023];
            //AGIResource tmpRes;
            int tmpSize, lngHeader, lngMaxVol, j;
            int lngStart, lngEnd, lngFree, NewResSize;
            string strID;
            AGIResType NewResType;
            byte NewResNum;
            //set header size, max# of VOL files and ID string depending on version
            if (NewResource.parent.agIsVersion3) {
                lngHeader = 7;
                strID = NewResource.parent.agGameID;
                lngMaxVol = 15;
            }
            else {
                lngHeader = 5;
                strID = "";
                lngMaxVol = 4;
            }
            //local copy of restype and resnum (for improved speed)
            NewResType = NewResource.ResType;
            NewResNum = NewResource.Number;
            NewResSize = NewResource.Size + lngHeader;
            //build array of all resources, sorted by VOL order (except the one being loaded)
            foreach (Logic tmpRes in NewResource.parent.agLogs.Col.Values) {
                //if not the resource being replaced
                if (NewResType != AGIResType.rtLogic || tmpRes.Number != NewResNum) {
                    tmpSize = tmpRes.SizeInVOL + lngHeader;
                    AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, resLoc, resSize);
                }
            }
            foreach (Picture tmpRes in NewResource.parent.agPics.Col.Values) {
                //if not the resource being replaced
                if (NewResType != AGIResType.rtPicture || tmpRes.Number != NewResNum) {
                    tmpSize = tmpRes.SizeInVOL + lngHeader;
                    AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, resLoc, resSize);
                }
            }
            foreach (Sound tmpRes in NewResource.parent.agSnds.Col.Values) {
                //if not the resource being replaced
                if (NewResType != AGIResType.rtSound || tmpRes.Number != NewResNum) {
                    tmpSize = tmpRes.SizeInVOL + lngHeader;
                    AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, resLoc, resSize);
                }
            }
            foreach (View tmpRes in NewResource.parent.agViews.Col.Values) {
                //if not the resource being replaced
                if (NewResType != AGIResType.rtView || tmpRes.Number != NewResNum) {
                    tmpSize = tmpRes.SizeInVOL + lngHeader;
                    AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, resLoc, resSize);
                }
            }
            //step through volumes,
            for (sbyte i = 0; i <= lngMaxVol; i++) {
                //start at beginning
                lngStart = 0;
                // check for space between resources
                for (j = 1; j <= resLoc[i, 0] + 1; j++) {
                    //if this is not the end of the list
                    if (j < resLoc[i, 0] + 1) {
                        lngEnd = resLoc[i, j];
                    }
                    else {
                        lngEnd = MAX_VOLSIZE;
                    }
                    //calculate space between end of one resource and start of next
                    lngFree = lngEnd - lngStart;

                    //if enough space is found
                    if (lngFree >= NewResSize) {
                        //it fits here!
                        //set volume and location
                        NewResource.Volume = i;
                        NewResource.Loc = lngStart;
                        return;
                    }
                    // not enough room; 
                    // adjust start to end of current resource
                    lngStart = resLoc[i, j] + resSize[i, j];
                }
            }
            //if no room in any VOL file, raise an error

            WinAGIException wex = new(LoadResString(593)) {
                HResult = WINAGI_ERR + 593
            };
            throw wex;
        }

        internal int SizeInVOL(AGIResource resource) {

            //if an error occurs while trying to read the size of this
            //resource, the function returns -1
            byte bytHigh, bytLow;
            int lngV3Offset;
            string strVolFile;
            //any file access errors
            //result in invalid size

            //if version 3
            if (IsV3) {
                //adjusts header so compressed size is retrieved
                lngV3Offset = 2;
                //set filename
                strVolFile = parent.agGameDir + parent.agGameID + "VOL." + resource.Volume;
            }
            else {
                lngV3Offset = 0;
                //set filename
                strVolFile = parent.agGameDir + "VOL." + resource.Volume;
            }
            try {
                // open the volume file
                VOLFile = new FileStream(strVolFile, FileMode.Open);
                // verify enough room to get length of resource
                if (VOLFile.Length >= resource.Loc + 5 + lngV3Offset) {
                    //get size low and high bytes
                    VOLFile.Seek(resource.Loc, SeekOrigin.Begin);
                    bytLow = VOLReader.ReadByte();
                    bytHigh = VOLReader.ReadByte();
                    //verify this is a proper resource
                    if ((bytLow == 0x12) && (bytHigh == 0x34)) {
                        //now get the low and high bytes of the size
                        VOLFile.Seek(1, SeekOrigin.Current);
                        bytLow = VOLReader.ReadByte();
                        bytHigh = VOLReader.ReadByte();
                        Clear();
                        return (bytHigh << 8) + bytLow;
                    }
                }
            }
            catch (Exception) {
                // treat all errors the same
            }
            finally {
                // ensure file is closed
                Clear();
            }
            // if size not found, return -1
            return -1;
        }
    }

    public static partial class Base {

        internal static void CompileResCol(AGIGame game, dynamic tmpResCol, AGIResType ResType, bool RebuildOnly, bool NewIsV3) {

            //compiles all the resources of ResType that are in the tmpResCol collection object
            //by adding them to the new VOL files
            //if RebuildOnly is passed, it won//t try to compile the logic; it will only add
            //the items into the new VOL files
            //the NewIsV3 flag is used when the compiling activity is being done because
            //the version is changing from a V2 game to a V3 game

            //if a resource error is encountered, the function returns a Value of false
            //if no resource errors encountered, the function returns true

            //if any other errors encountered, the Err object is set and the calling function
            //must deal with the error
            byte CurResNum;
            int lngError;
            string strMsg = "", strError;
            bool blnWarning = false, blnUnloadRes;

            //add all resources of this type
            foreach (AGIResource tmpGameRes in tmpResCol) {
                CurResNum = tmpGameRes.Number;
                //update status
                TWinAGIEventInfo tmpWarn = new() {
                    Type = EventType.etInfo,
                    InfoType = EInfoType.itResources,
                    ResType = ResType,
                    ResNum = CurResNum,
                    ID = "",
                    Module = "",
                    Text = ""
                };
                Raise_CompileGameEvent(ECStatus.csAddResource, ResType, CurResNum, tmpWarn);
                //check for cancellation
                if (!tmpGameRes.parent.agCompGame) {
                    tmpGameRes.parent.CompleteCancel();
                    return;
                }

                // use a loop to add resources;
                // exit loop when an error occurs, or after
                // resource is successfully added to vol file
                do {
                    // set flag to force unload, if resource not currently loaded
                    blnUnloadRes = !tmpGameRes.Loaded;
                    // then load resource if necessary
                    if (blnUnloadRes) {
                        // load resource
                        tmpGameRes.Load();
                        if (tmpGameRes.ErrLevel >= 0) {
                            // always reset warning flag
                            blnWarning = false;
                        }
                        else  {
                            if (RebuildOnly) {
                                //note it
                                tmpWarn.Text = $"Unable to load {tmpGameRes.ID} ({LoadResString(tmpGameRes.ErrLevel)})";
                                Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, tmpWarn);
                                //check for cancellation
                                if (!tmpGameRes.parent.agCompGame) {
                                    tmpGameRes.parent.CompleteCancel();
                                    // make sure unloaded
                                    tmpGameRes.Unload();
                                    // and stop compiling
                                    return;
                                }
                            }
                            else {
                                // deal with the error
                                switch (tmpGameRes.ErrLevel) {
                                case 610:
                                    // assume a resource error until determined otherwise
                                    //strMsg = "Unable to load " + tmpGameRes.ID + " (" + e.Message + ")";
                                    ////reset warning flag
                                    //blnWarning = false;
                                    //switch (ResType) {
                                    //case AGIResType.rtPicture:
                                    //case AGIResType.rtView:
                                    //    //check for invalid view data errors
                                    //    switch (e.HResult - WINAGI_ERR) {
                                    //    case 537:
                                    //    case 548:
                                    //    case 552:
                                    //    case 553:
                                    //    case 513:
                                    //    case 563:
                                    //    case 539:
                                    //    case 540:
                                    //    case 550:
                                    //    case 551:
                                    //        //can add view, but it is invalid
                                    //        strMsg = "Invalid view data (" + e.Message + ")";
                                    //        blnWarning = true;
                                    //        break;
                                    //    }
                                    //    break;
                                    //case AGIResType.rtSound:
                                    //    //check for invalid sound data errors
                                    //    if (e.HResult == 598) {
                                    //        //can add sound, but it is invalid
                                    //        strMsg = "Invalid sound data (" + e.Message + ")";
                                    //        blnWarning = true;
                                    //    }
                                    //    break;
                                    //case AGIResType.rtLogic:
                                    //    //always a resource error
                                    //    break;
                                    //}
                                    //if error (warning not set)
                                    if (!blnWarning) {
                                        //note the error
                                        tmpWarn.Type = EventType.etWarning;
                                        tmpWarn.Text = strMsg;
                                        Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, tmpWarn);
                                        //check for cancellation
                                        if (!tmpGameRes.parent.agCompGame) {
                                            tmpGameRes.parent.CompleteCancel();
                                            // make sure unloaded
                                            tmpGameRes.Unload();
                                            //and stop compiling
                                            return;
                                        }
                                        //if not canceled, user has already removed bad resource from game
                                        break;
                                    }
                                    break;
                                }
                            }
                            // if not canceled, user has already removed bad resource
                            break;
                        }
                    }

                    //if full compile (not rebuild only)
                    if (!RebuildOnly) {
                        //game resource is verified open; check for warnings

                        ////picture warnings aren't in error handler anymore and 
                        //// need a special check here
                        //if (tmpGameRes is Picture tmpPic) {
                        //    //check bmp error level by property, not error number
                        //    if (tmpPic.ErrLevel > 0) {
                        //        //case 0:
                        //        //ok
                        //        //unhandled error
                        //        strMsg = "Unhandled error in picture data- picture may not display correctly";
                        //        blnWarning = true;
                        //    }
                        //    else {
                        //        // missing EOP marker, bad color or bad cmd
                        //        strMsg = "Data anomalies in picture data but picture should display";
                        //        blnWarning = true;
                        //    }
                        //}

                        //if a warning
                        if (blnWarning) {
                            //note the warning
                            tmpWarn.Type = EventType.etWarning;
                            tmpWarn.Text = strMsg;
                            Raise_CompileGameEvent(ECStatus.csWarning, ResType, CurResNum, tmpWarn);
                            //check for cancellation
                            if (!tmpGameRes.parent.agCompGame) {
                                tmpGameRes.parent.CompleteCancel();
                                // unload if needed
                                if (blnUnloadRes && tmpGameRes is not null) tmpGameRes.Unload();
                                // then stop compiling
                                return;
                            }
                        }
                        // for logics, compile the sourcetext
                        //load actual object
                        if (tmpGameRes is Logic tmpLog) {
                            try {
                                //compile it
                                Compiler.CompileLogic(tmpLog);
                            }
                            catch (Exception e) {
                                switch (e.HResult - WINAGI_ERR) {
                                case 635: //compile error
                                          //raise compile event
                                    tmpWarn.Type = EventType.etWarning;
                                    tmpWarn.Text = e.Message;
                                    Raise_CompileGameEvent(ECStatus.csLogicError, ResType, CurResNum, tmpWarn);
                                    //check for cancellation
                                    if (!tmpGameRes.parent.agCompGame) {
                                        tmpGameRes.parent.CompleteCancel();
                                        // unload if needed
                                        if (blnUnloadRes && tmpGameRes is not null) tmpGameRes.Unload();
                                        // then stop compiling
                                        return;
                                    }
                                    // if user wants to continue, the logic will already have been removed; no other action needed
                                    break;
                                default:
                                    //any other error; note it
                                    tmpWarn.Type = EventType.etWarning;
                                    tmpWarn.Text = "Unable to compile Logic (" + e.Message + ")";
                                    Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, tmpWarn);
                                    //check for cancellation
                                    if (!tmpGameRes.parent.agCompGame) {
                                        tmpGameRes.parent.CompleteCancel();
                                        // unload if needed
                                        if (blnUnloadRes && tmpGameRes is not null) tmpGameRes.Unload();
                                        // then stop compiling
                                        return;
                                    }
                                    //if user did not cancel, resource will already have been removed
                                    break;
                                }
                            }
                        }
                    }

                    try {
                        //validate vol and loc
                        game.volManager.ValidateVolAndLoc(tmpGameRes, game.ResDir);
                    }
                    catch (Exception e) {
                        //save error msg
                        strError = e.Message;
                        //strErrSrc = Err.Source;
                        lngError = e.HResult;

                        //clean up compiler
                        tmpGameRes.parent.CompleteCancel(true);

                        //unload resource, if applicable
                        if (blnUnloadRes && tmpGameRes is not null) tmpGameRes.Unload();

                        // raise appropriate error
                        if (lngError == WINAGI_ERR + 593) {
                            //exceed max storage- pass it along
                            throw;
                        }
                        else {
                            // file access error
                            WinAGIException wex = new(LoadResString(638).Replace(Common.Base.ARG1, e.Message)) {
                                HResult = WINAGI_ERR + 638
                            };
                            wex.Data["exception"] = e;
                            wex.Data["ID"] = tmpGameRes.ID;
                            throw wex;
                        }
                    }

                    // use arrays for DIR data, and then build the DIR files later, after
                    // compiling all resources is complete
                    game.volManager.DIRData[(int)ResType, tmpGameRes.Number * 3] = (byte)((game.volManager.Index << 4) + (game.volManager.Loc >> 16));
                    game.volManager.DIRData[(int)ResType, tmpGameRes.Number * 3 + 1] = (byte)((game.volManager.Loc % 0x10000) >> 8);
                    game.volManager.DIRData[(int)ResType, tmpGameRes.Number * 3 + 2] = (byte)(game.volManager.Loc % 0x100);

                    try {
                        //add it to vol
                        game.volManager.AddToVol(tmpGameRes, NewIsV3, true); //TODO: , lngCurrentVol, lngCurrentLoc);
                    }
                    catch (Exception e) {
                        //note it
                        tmpWarn.Type = EventType.etWarning;
                        tmpWarn.Text = "Unable to add Logic resource to VOL file (" + e.Message + ")";
                        Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, tmpWarn);
                        //check for cancellation
                        if (!tmpGameRes.parent.agCompGame) {
                            tmpGameRes.parent.CompleteCancel();
                            //unload resource, if applicable
                            if (blnUnloadRes && tmpGameRes is not null) tmpGameRes.Unload();
                            // then stop compiling
                            return;
                        }
                        // if not canceled, user deleted bad resource
                        // so exit loop to move to next resource
                        break;
                    }
                    // always exit loop
                }
                while (false);

                // done with resource; unload if applicable
                if (blnUnloadRes && tmpGameRes is not null) tmpGameRes.Unload();
            }
        }
    }
}
