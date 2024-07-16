using System;
using System.Collections.Generic;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// Internal class used to track current DIR and VOL information during game compilation.
    /// </summary>
    internal class VOLManager {
        #region Structs
        private struct ResInfo {
            public int Loc;
            public int Size;
        }
        #endregion

        #region Members
        internal FileStream fsVOL, fsDIR;
        internal AGIGame parent;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the VOLManager class and attaches it to an AGI game.
        /// </summary>
        /// <param name="game"></param>
        internal VOLManager(AGIGame game) {
            // for dir files, use arrays during compiling process
            // then build the dir files at the end
            DIRData = new byte[4, 256, 3];
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 256; j++) {
                    for (int k = 0; k < 3; k++) {
                        DIRData[i, j, k] = 255;
                    }
                }
            }
            parent = game;
            Loc = 0;
            Index = 0;
            Count = 1;
            // file access members will be intialized by calling code
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the directory data which tracks the volume and 
        /// index position of each resource in this game.
        /// </summary>
        public byte[,,] DIRData { get; set; }

        /// <summary>
        /// Gets or sets the file stream associated with the current DIR file, which
        /// is the file that resources are currently being added to.
        /// </summary>
        public FileStream DIRFile {
            get => fsDIR;
            set {
                DIRWriter?.Dispose();
                fsDIR?.Dispose();
                fsDIR = value;
                DIRWriter = new BinaryWriter(fsDIR);
            }
        }

        /// <summary>
        /// Gets or sets the file stream associated with the current VOL file, which is the
        /// VOL file that resources are currently being added to.
        /// </summary>
        public FileStream VOLFile {
            get => fsVOL;
            set {
                VOLWriter?.Dispose();
                fsVOL?.Dispose();
                fsVOL = value;
                VOLWriter = new BinaryWriter(fsVOL);
            }
        }

        /// <summary>
        /// Gets or sets the binary writer that provides write access to the 
        /// current DIR file.
        /// </summary>
        public BinaryWriter DIRWriter { get; private set; }

        /// <summary>
        /// Gets or sets the binary writer that provides write access to the
        /// current VOL file.
        /// </summary>
        public BinaryWriter VOLWriter { get; private set; }

        /// <summary>
        /// Gets or sets the index (location) in the current VOL file where
        /// resources will be added.
        /// </summary>
        public int Loc { get; set; }

        /// <summary>
        /// Gets or set the index of the VOL file that is currently being written
        /// to, i.e. VOL.0, VOL.1, etc.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the largest VOL file index value currently in use for
        /// this game.
        /// </summary>
        public int Count { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Disposes the file handlers and resets parameters.
        /// </summary>
        internal void Clear() {
            VOLFile?.Dispose();
            VOLWriter?.Dispose();
            DIRFile?.Dispose();
            DIRWriter?.Dispose();
        }

        /// <summary>
        /// Initializes this VOLManager object to begin writing resources to 
        /// VOL and DIR files.
        /// </summary>
        /// <param name="gamedir"></param>
        public void InitVol(string gamedir) {
            VOLWriter?.Dispose();
            fsVOL?.Dispose();
            fsVOL = File.Create(gamedir + "NEW_VOL.0");
            VOLWriter = new BinaryWriter(fsVOL);
        }

        /// <summary>
        /// Adds a resource to the next available location in the current VOL file.
        /// Used by CompileGame only.
        /// </summary>
        /// <param name="AddRes"></param>
        /// <param name="replace"></param>
        internal void AddNextRes(AGIResource AddRes, bool IsV3) {
            byte[] ResHeader;

            // the DIR file is not updated; that is done by the
            // CompileGame method AFTER all VOL files are built
            // using the data stored in a temporary array
            DIRData[(int)AddRes.ResType, AddRes.Number, 0] = (byte)((Index << 4) + (Loc >> 16));
            DIRData[(int)AddRes.ResType, AddRes.Number, 1] = (byte)((Loc % 0x10000) >> 8);
            DIRData[(int)AddRes.ResType, AddRes.Number, 2] = (byte)(Loc % 0x100);
            // build header
            ResHeader = new byte[5];
            ResHeader[0] = 0x12;
            ResHeader[1] = 0x34;
            ResHeader[2] = (byte)Index;
            ResHeader[3] = (byte)(AddRes.Size % 256);
            ResHeader[4] = (byte)(AddRes.Size / 256);
            if (IsV3) {
                Array.Resize(ref ResHeader, 7);
                ResHeader[5] = ResHeader[3];
                ResHeader[6] = ResHeader[4];
            }
            try {
                fsVOL.Seek(Loc, SeekOrigin.Begin);
                VOLWriter.Write(ResHeader, 0, IsV3 ? 7 : 5);
                VOLWriter.Write(AddRes.Data, 0, AddRes.Data.Length);
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(638)) {
                    HResult = WINAGI_ERR + 638,
                };
                wex.Data["exception"] = e;
                wex.Data["ID"] = AddRes.ID;
                throw wex;
            }
            // increment loc pointer
            Loc += AddRes.Size + (IsV3 ? 7 : 5);
        }

        /// <summary>
        /// This method is used by the game compiler to find the next valid
        /// location for a resource to be added to the VOL files It checks
        /// the current VOL file to determine if there is room for a resource.
        /// If not, it closes current VOL file, and opens next one.
        /// </summary>
        /// <param name="resource"></param>
        internal void ValidateVolAndLoc(AGIResource resource, bool IsV3) {
            // check if resource is too long
            if (Loc + resource.Size > MAX_VOLSIZE || (Index == 0 && (Loc + resource.Size > resource.parent.agMaxVol0))) {
                //set maxvol count to 4 or 15, depending on version
                int lngMaxVol = IsV3 ? 15 : 4;

                // close current vol file
                VOLFile.Dispose();
                VOLWriter.Dispose();

                // start check with previous vol files, to see if there is room at
                // end of one of those; check up to max vol number
                for (int i = 0; i <= lngMaxVol; i++) {
                    try {
                        // open this file (if the file doesn't exist, this will create it
                        // and it will then be set as the next vol, with pos=0
                        VOLFile = new FileStream(resource.parent.agGameDir + "NEW_VOL." + i, FileMode.OpenOrCreate);
                    }
                    catch (Exception e) {
                        Clear();
                        WinAGIException wex = new(LoadResString(640)) {
                            HResult = WINAGI_ERR + 640
                        };
                        wex.Data["exception"] = e;
                        wex.Data["ID"] = resource.ID;
                        throw wex;
                    }
                    // is there room at the end of this file?
                    if ((i > 0 && (VOLFile.Length + resource.Size <= MAX_VOLSIZE)) || (VOLFile.Length + resource.Size <= resource.parent.agMaxVol0)) {
                        // if so, set pointer to end of the file, and exit
                        Index = (sbyte)i;
                        Loc = (int)VOLFile.Length;
                        if (Index >= Count) {
                            Count = Index + 1;
                        } 
                        return;
                    }
                }
                // if no volume found, we've got a problem...
                WinAGIException wex1 = new(LoadResString(593)) {
                    HResult = WINAGI_ERR + 593
                };
                throw wex1;
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Compiles all the resources of ResType that are in the tmpResCol collection object
        /// by adding them to the new VOL files.<br /><br />
        /// If RebuildOnly is passed, it won't try to compile logics; it will only add
        /// the items into the new VOL files.<br /><br />
        /// The NewIsV3 flag is used when the compiling activity is being done because
        /// the version is changing from a V2 game to a V3 game.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="tmpResCol"></param>
        /// <param name="ResType"></param>
        /// <param name="RebuildOnly"></param>
        /// <param name="IsV3"></param>
        /// <returns>true, if no resource errors encountered, otherwise false.</returns>
        internal static bool CompileResCol(AGIGame game, dynamic tmpResCol, AGIResType ResType, bool RebuildOnly, bool IsV3) {
            byte CurResNum;
            bool blnUnloadRes;

            foreach (AGIResource tmpGameRes in tmpResCol) {
                CurResNum = tmpGameRes.Number;
                // update status
                TWinAGIEventInfo tmpWarn = new() {
                    Type = EventType.etInfo,
                    InfoType = EInfoType.itResources,
                    ResType = ResType,
                    ResNum = CurResNum,
                    ID = tmpGameRes.ID,
                    Module = "",
                    Text = ""
                };
                // check for cancellation
                if (AGIGame.OnCompileGameStatus(ECStatus.csAddResource, tmpWarn)) {
                    tmpGameRes.parent.CompleteCancel();
                    return false;
                }
                // set flag to force unload, if resource not currently loaded
                blnUnloadRes = !tmpGameRes.Loaded;
                // then load resource if necessary
                if (blnUnloadRes) {
                    // load resource
                    tmpGameRes.Load();
                }
                if (tmpGameRes.ErrLevel != 0 || ((tmpGameRes.ResType == AGIResType.Logic) && ((Logic)tmpGameRes).SrcErrLevel != 0)) {
                    // check for major error (compile automatically fails)
                    if (tmpGameRes.ErrLevel < 0) {
                        // error level is less than zero - major error
                        // -1  resource: file does not exist
                        // -2  resource: file is readonly
                        // -3  resource: file access error
                        // -4  resource: invalid location value in VOL file
                        // -5  resource: invalid data (missing AGI '0x12-0x34' header)
                        // -6  resource: failure to expand v3 compressed resource
                        tmpWarn.Text = $"Unable to load {tmpGameRes.ID}";
                        tmpWarn.Line = tmpGameRes.ErrLevel.ToString();
                        _ = AGIGame.OnCompileGameStatus(ECStatus.csResError, tmpWarn);
                        // make sure unloaded
                        tmpGameRes.Unload();
                        // and stop compiling
                        tmpGameRes.parent.CompleteCancel(true);
                        return false;
                    }
                    // check for minor error (compile fails if user cancels)
                    if (tmpGameRes.ErrLevel > 0) {
                        // minor errors/warnings
                        // ask if compiling, assume ok if rebuilding
                        // Logics:
                        //  1 = error when decoding messages
                        //  2 = error when locating goto labels
                        //  4 = error when decoding if block
                        // Pictures:
                        //  1 = no EOP marker
                        //  2 = bad vis color data
                        //  4 = invalid command byte
                        //  8 = unused data at end of resource
                        //  16 = other error
                        // Sounds:
                        //  1 = invalid track offset
                        //  2 = zero length note
                        //  4 = missing track end marker
                        //  8 = no sound data
                        // 16 = invalid sound file format
                        // Views:
                        //  1 = no loops
                        //  2 = invalid loop offset
                        //  4 = invalid mirror loop pair
                        //  8 = more than two loops share mirror
                        // 16 = invalid cel offset
                        // 32 = unexpected end of cel data
                        // 64 = invalid description offset
                        tmpWarn.Text = $"Errors and/or anomalies detected in {tmpGameRes.ID}";
                        tmpWarn.Line = tmpGameRes.ErrLevel.ToString();
                        // check for cancellation
                        if (AGIGame.OnCompileGameStatus(ECStatus.csWarning, tmpWarn)) {
                            // unload if needed
                            if (blnUnloadRes && tmpGameRes is not null) tmpGameRes.Unload();
                            // then stop compiling
                            tmpGameRes.parent.CompleteCancel();
                            return false;
                        }
                    }
                    // for logics, check for source errors (compile automatically fails)
                    if (tmpGameRes.ResType == AGIResType.Logic && ((Logic)tmpGameRes).SrcErrLevel != 0) {
                        // -1  logic source: file does not exist
                        // -2  logic source: file is readonly
                        // -3  logic source: file access error
                        tmpWarn.Text = $"Unable to load source code for {tmpGameRes.ID}";
                        tmpWarn.Line = tmpGameRes.ErrLevel.ToString();
                        _ = AGIGame.OnCompileGameStatus(ECStatus.csResError, tmpWarn);
                        // make sure unloaded
                        tmpGameRes.Unload();
                        // and stop compiling
                        tmpGameRes.parent.CompleteCancel(true);
                        return false;
                    }
                }
                // if full compile (not rebuild only)
                if (!RebuildOnly) {
                    // for logics, compile the sourcetext
                    // TODO: test- will tmpLog just be a null object?
                    if (tmpGameRes is Logic tmpLog) {
                        TWinAGIEventInfo tmpInfo = LogicCompiler.CompileLogic(tmpLog);
                        if (tmpInfo.Type == EventType.etError) {
                            // logic compile error - always cancel
                            _ = AGIGame.OnCompileGameStatus(ECStatus.csLogicError, tmpInfo);
                            // unload if needed
                            if (blnUnloadRes && tmpGameRes is not null) tmpGameRes.Unload();
                            // then stop compiling
                            tmpGameRes.parent.CompleteCancel(true);
                            return false;
                        }
                    }
                }
                try {
                    // add it to VOL/DIR files
                    game.volManager.ValidateVolAndLoc(tmpGameRes, IsV3);
                    game.volManager.AddNextRes(tmpGameRes, IsV3);
                }
                catch (Exception e) {
                    tmpWarn.Type = EventType.etError;
                    tmpWarn.Text = "Unable to add resource to VOL file (" + e.Message + ")";
                    _ = AGIGame.OnCompileGameStatus(ECStatus.csResError, tmpWarn);
                    // unload resource, if applicable
                    if (blnUnloadRes && tmpGameRes is not null) tmpGameRes.Unload();
                    // then stop compiling
                    tmpGameRes.parent.CompleteCancel(true);
                    return false;
                }
                // done with resource; unload if applicable
                if (blnUnloadRes && tmpGameRes is not null) {
                    tmpGameRes.Unload();
                }
            }
            return true;
        }

        /// <summary>
        /// This method finds a place in vol files to store a resource and updates
        /// the resource's Volume and Loc properties
        /// </summary>
        /// <param name="resource"></param>
        private static void FindFreeVOLSpace(AGIResource resource) {
            // the search has to account for the header that AGI uses at the
            // beginning of each resource; 5 bytes for v2 and 7 bytes for v3
            SortedList<int, ResInfo>[] resInfo = new SortedList<int, ResInfo>[16];
            for (int i = 0; i < 16; i++) {
                resInfo[i] = [];
            }
            ResInfo addInfo = new();
            int lngHeader, lngMaxVol, j;
            int previousend, nextstart, freespace;

            // set header size, max# of VOL files depending on version
            if (resource.parent.agIsVersion3) {
                lngHeader = 7;
                lngMaxVol = 15;
            }
            else {
                lngHeader = 5;
                lngMaxVol = 4;
            }
            // build array of all resources, sorted by VOL order
            foreach (Logic tmpRes in resource.parent.agLogs.Col.Values) {
                addInfo.Loc = tmpRes.Loc;
                addInfo.Size = tmpRes.SizeInVOL + lngHeader;
                resInfo[tmpRes.Volume].Add(tmpRes.Loc, addInfo);
            }
            foreach (Picture tmpRes in resource.parent.agPics.Col.Values) {
                addInfo.Loc = tmpRes.Loc;
                addInfo.Size = tmpRes.SizeInVOL + lngHeader;
                resInfo[tmpRes.Volume].Add(tmpRes.Loc, addInfo);
            }
            foreach (Sound tmpRes in resource.parent.agSnds.Col.Values) {
                addInfo.Loc = tmpRes.Loc;
                addInfo.Size = tmpRes.SizeInVOL + lngHeader;
                resInfo[tmpRes.Volume].Add(tmpRes.Loc, addInfo);
            }
            foreach (View tmpRes in resource.parent.agViews.Col.Values) {
                addInfo.Loc = tmpRes.Loc;
                addInfo.Size = tmpRes.SizeInVOL + lngHeader;
                resInfo[tmpRes.Volume].Add(tmpRes.Loc, addInfo);
            }
            // step through volumes,
            for (int i = 0; i <= lngMaxVol; i++) {
                // start at beginning
                previousend = 0;
                // check for space between resources
                for (j = 0; j < resInfo[i].Count; j++) {
                    //if this is not the end of the list
                    if (j < resInfo[i].Count - 1) {
                        nextstart = resInfo[i][j].Loc;
                    }
                    else {
                        nextstart = i == 0 ? resource.parent.agMaxVol0 : MAX_VOLSIZE;
                    }
                    // calculate space between end of one resource and start of next
                    freespace = nextstart - previousend;
                    if (freespace >= resource.Size + lngHeader) {
                        // it fits here!
                        resource.Volume = (sbyte)i;
                        resource.Loc = previousend;
                        return;
                    }
                    // not enough room;
                    // adjust start to end of current resource
                    previousend = nextstart + resInfo[i][j].Size;
                }
            }
            // if no room in any VOL file, raise an error
            WinAGIException wex = new(LoadResString(593)) {
                HResult = WINAGI_ERR + 593
            };
            throw wex;
        }

        /// <summary>
        /// Saves a resource to a VOL file in first empty space where it fits and 
        /// updates the DIR file. Used by the resource Save method.
        /// </summary>
        /// <param name="AddRes"></param>
        /// <param name="replace"></param>
        internal static void UpdateInVol(AGIResource AddRes) {
            // It finds the first available spot for the resource, and
            // adds it there. If relpacing an existing resource it will
            // not delete the resource data from its old position (but
            // the area will be available for future use by another resource)
            // and then it updates the DIR file.
            byte[] ResHeader;
            string strID;

            try {
                // get vol number and location where there is room for this resource
                FindFreeVOLSpace(AddRes);
            }
            catch {
                // pass it along
                throw;
            }
            // build header
            ResHeader = new byte[5];
            ResHeader[0] = 0x12;
            ResHeader[1] = 0x34;
            ResHeader[2] = (byte)AddRes.Volume;
            ResHeader[3] = (byte)(AddRes.Size % 256);
            ResHeader[4] = (byte)(AddRes.Size / 256);
            if (AddRes.parent.agIsVersion3) {
                Array.Resize(ref ResHeader, 7);
                ResHeader[5] = ResHeader[3];
                ResHeader[6] = ResHeader[4];
                strID = AddRes.parent.agGameID.ToUpper();
            }
            else {
                strID = "";
            }
            FileStream fsVOL = null;
            BinaryWriter bwVOL = null;
            try {
                // save the resource into the vol file
                fsVOL = new FileStream(AddRes.parent.agGameDir + strID + "VOL." + AddRes.Volume.ToString(), FileMode.Open);
                fsVOL.Seek(AddRes.Loc, SeekOrigin.Begin);
                bwVOL = new BinaryWriter(fsVOL);
                bwVOL.Write(ResHeader, 0, AddRes.parent.agIsVersion3 ? 7 : 5);
                bwVOL.Write(AddRes.Data, 0, AddRes.Data.Length);
            }
            catch (Exception e) {
                fsVOL?.Dispose();
                bwVOL?.Dispose();
                WinAGIException wex = new(LoadResString(638)) {
                    HResult = WINAGI_ERR + 638,
                };
                wex.Data["exception"] = e;
                wex.Data["ID"] = AddRes.ID;
                throw wex;
            }
            try {
                UpdateDirFile(AddRes);
            }
            catch {
                // only error that will be returned is expandv3dir error
                // pass it along
                throw;
            }
        }
            
        /// <summary>
        /// This method updates the DIR file for this resource type with its current
        /// volume and location.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="remove"></param>
        internal static void UpdateDirFile(AGIResource resource, bool remove = false) {
            // if Remove option passed as true, the resource is marked as 'deleted'
            // and 0xFFFFFF is written in the resource's DIR file place holder
            //
            // the DIR file is then expanded or compressed as necessary

            // NOTE: directories inside a V3 dir file are in this order:
            //      LOGIC, PICTURE, VIEW, SOUND
            // the ResType enumeration is in this order:
            //      LOGIC, PICTURE, SOUND, VIEW
            string strDirFile;
            byte[] bytDIR, DirByte = new byte[2];
            int intMax = 0, intOldMax;
            int lngDirOffset = 0, lngDirEnd = 0, i, lngStart, lngStop;
            FileStream fsDIR;

            if (remove) {
                // resource marked for deletion
                DirByte[0] = 0xFF;
                DirByte[1] = 0xFF;
                DirByte[2] = 0xFF;
            }
            else {
                // calculate directory bytes
                DirByte[0] = (byte)((resource.Volume << 4) + ((uint)resource.Loc >> 16));
                DirByte[1] = (byte)((resource.Loc % 0x10000) >> 8);
                DirByte[2] = (byte)(resource.Loc % 0x100);
            }
            // what is current max for this res type?
            switch (resource.ResType) {
            case AGIResType.Logic:
                intMax = resource.parent.agLogs.Max;
                break;
            case AGIResType.Picture:
                intMax = resource.parent.agPics.Max;
                break;
            case AGIResType.Sound:
                intMax = resource.parent.agSnds.Max;
                break;
            case AGIResType.View:
                intMax = resource.parent.agViews.Max;
                break;
            }
            if (resource.parent.agIsVersion3) {
                strDirFile = resource.parent.agGameDir + resource.parent.agGameID + "DIR";
            }
            else {
                strDirFile = resource.parent.agGameDir + ResTypeAbbrv[(int)resource.ResType] + "DIR";
            }
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
                WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = e;
                wex.Data["badfile"] = strDirFile;
                throw wex;
            }
            // calculate old max and offset (for v3 files)
            if (resource.parent.agIsVersion3) {
                // calculate directory offset
                switch (resource.ResType) {
                case AGIResType.Logic:
                    lngDirOffset = 8;
                    lngDirEnd = (bytDIR[3] << 8) + bytDIR[2];
                    break;
                case AGIResType.Picture:
                    lngDirOffset = (bytDIR[3] << 8) + bytDIR[2];
                    lngDirEnd = (bytDIR[5] << 8) + bytDIR[4];
                    break;
                case AGIResType.View:
                    lngDirOffset = (bytDIR[5] << 8) + bytDIR[4];
                    lngDirEnd = (bytDIR[7] << 8) + bytDIR[6];
                    break;
                case AGIResType.Sound:
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
            if ((remove && (intMax >= intOldMax)) || (!remove && (intOldMax >= intMax))) {
                // adjust offset for resnum
                lngDirOffset += 3 * resource.Number;
                // insert the new data, and save the file
                try {
                    using (fsDIR = new FileStream(strDirFile, FileMode.Open)) {
                        _ = fsDIR.Seek(lngDirOffset, SeekOrigin.Begin);
                        fsDIR.Write(bytDIR);
                    }
                    return;
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                        HResult = WINAGI_ERR + 502,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["badfile"] = strDirFile;
                    throw wex;
                }
            }
            // dir file size needs to change
            if (remove) {
                // must be shrinking - for v2, just resize the array
                if (!resource.parent.agIsVersion3) {
                    Array.Resize(ref bytDIR, (intMax + 1) * 3);
                }
                else {
                    // if restype is sound, we also can just truncate the file
                    if (resource.ResType == AGIResType.Sound) {
                        Array.Resize(ref bytDIR, bytDIR.Length - 3 * (intOldMax - intMax));
                    }
                    else {
                        // need to move data from the current directory's max
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
                        // if resource is a view, we are done
                        if (resource.ResType != AGIResType.View) {
                            // move view offset
                            lngDirOffset = (bytDIR[5] << 8) + bytDIR[4];
                            lngDirOffset -= 3 * (intOldMax - intMax);
                            bytDIR[5] = (byte)(lngDirOffset >> 8);
                            bytDIR[4] = (byte)(lngDirOffset % 0x100);
                            // if resource is a pic, now we are done
                            if (resource.ResType != AGIResType.Picture) {
                                // move picture offset
                                lngDirOffset = (bytDIR[3] << 8) + bytDIR[2];
                                lngDirOffset -= 3 * (intOldMax - intMax);
                                bytDIR[3] = (byte)(lngDirOffset >> 8);
                                bytDIR[2] = (byte)(lngDirOffset % 0x100);
                            }
                        }
                    }
                }
                try {
                    // delete the existing file
                    File.Delete(strDirFile);
                    // now create the new file
                    using (fsDIR = new FileStream(strDirFile, FileMode.OpenOrCreate)) {
                        fsDIR.Write(bytDIR, 0, bytDIR.Length);
                    }
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                        HResult = WINAGI_ERR + 502,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["badfile"] = strDirFile;
                    throw wex;
                }
            }
            else {
                // must be expanding
                Array.Resize(ref bytDIR, bytDIR.Length + 3 * (intMax - intOldMax));
                // if v2, add 0xffs to fill gap up to the last entry
                if (!resource.parent.agIsVersion3) {
                    lngStart = lngDirEnd;
                    lngStop = lngDirEnd + 3 * (intMax - intOldMax - 1) - 1;
                    for (i = lngStart; i <= lngStop; i++) {
                        bytDIR[i] = 0xFF;
                    }
                    // add updated dir data to end
                    bytDIR[lngStop + 1] = DirByte[0];
                    bytDIR[lngStop + 2] = DirByte[1];
                    bytDIR[lngStop + 3] = DirByte[2];
                }
                else {
                    // if expanding the sound dir, just fill it in with 0xffs
                    if (resource.ResType == AGIResType.Sound) {
                        lngStart = lngDirEnd;
                        lngStop = lngDirEnd + 3 * (intMax - intOldMax - 1) - 1;
                        for (i = lngStart; i <= lngStop; i++) {
                            bytDIR[i] = 0xFF;
                        }
                        // add data to end
                        bytDIR[lngStop + 1] = DirByte[0];
                        bytDIR[lngStop + 2] = DirByte[1];
                        bytDIR[lngStop + 3] = DirByte[2];
                    }
                    else {
                        // move data to make room for inserted resource
                        lngStop = bytDIR.Length - 1;
                        lngStart = lngDirEnd + 3 * (intMax - intOldMax);
                        for (i = lngStop; i > lngStart; i--) {
                            bytDIR[i] = bytDIR[i - 3 * (intMax - intOldMax)];
                        }
                        // insert ffs, up to insert location
                        lngStop = lngStart - 4;
                        lngStart = lngStop - 3 * (intMax - intOldMax - 1);
                        for (i = lngStart; i <= lngStop; i++) {
                            bytDIR[i] = 0xFF;
                        }
                        // add updated dir data to end
                        bytDIR[lngStop + 1] = DirByte[0];
                        bytDIR[lngStop + 2] = DirByte[1];
                        bytDIR[lngStop + 3] = DirByte[2];
                        // then adjust the offsets:
                        // move snddir offset first
                        lngDirOffset = (bytDIR[7] << 8) + bytDIR[6];
                        lngDirOffset += 3 * (intMax - intOldMax);
                        bytDIR[7] = (byte)(lngDirOffset >> 8);
                        bytDIR[6] = (byte)(lngDirOffset % 0x100);
                        // if resource is a view, we are done
                        if (resource.ResType != AGIResType.View) {
                            // move view offset
                            lngDirOffset = (byte)((bytDIR[5] << 8) + bytDIR[4]);
                            lngDirOffset += 3 * (intMax - intOldMax);
                            bytDIR[5] = (byte)(lngDirOffset >> 8);
                            bytDIR[4] = (byte)(lngDirOffset % 0x100);
                            // if resource is a pic, we are done
                            if (resource.ResType != AGIResType.Picture) {
                                // move picture offset
                                lngDirOffset = (byte)((bytDIR[3] << 8) + bytDIR[2]);
                                lngDirOffset += 3 * (intMax - intOldMax);
                                bytDIR[3] = (byte)(lngDirOffset >> 8);
                                bytDIR[2] = (byte)(lngDirOffset % 0x100);
                            }
                        }
                    }
                }
                // now write the array back to the filee
                try {
                    using (fsDIR = new FileStream(strDirFile, FileMode.Open)) {
                        fsDIR.Write(bytDIR, 0, bytDIR.Length);
                    }
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, strDirFile)) {
                        HResult = WINAGI_ERR + 502,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["badfile"] = strDirFile;
                    throw wex;
                }
            }
        }
        #endregion
    }
}
