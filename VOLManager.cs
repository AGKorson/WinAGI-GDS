using System;
using System.Collections.Generic;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
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
        public byte[,,] DIRData {
            get; set;
        }

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
        public BinaryWriter DIRWriter {
            get; private set;
        }

        /// <summary>
        /// Gets or sets the binary writer that provides write access to the
        /// current VOL file.
        /// </summary>
        public BinaryWriter VOLWriter {
            get; private set;
        }

        /// <summary>
        /// Gets or sets the index (location) in the current VOL file where
        /// resources will be added.
        /// </summary>
        public int Loc {
            get; set;
        }

        /// <summary>
        /// Gets or set the index of the VOL file that is currently being written
        /// to, i.e. VOL.0, VOL.1, etc.
        /// </summary>
        public int Index {
            get; set;
        }

        /// <summary>
        /// Gets or sets the largest VOL file index value currently in use for
        /// this game.
        /// </summary>
        public int Count {
            get; private set;
        }
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
            Index = 0;
            Loc = 0;
        }

        /// <summary>
        /// Initializes this VOLManager object to begin writing resources to 
        /// VOL and DIR files.
        /// </summary>
        /// <param name="gamedir"></param>
        public void InitVol(string gamedir) {
            VOLWriter?.Dispose();
            fsVOL?.Dispose();
            fsVOL = File.Create(Path.Combine(gamedir, "NEW_VOL.0"));
            VOLWriter = new BinaryWriter(fsVOL);
        }

        /// <summary>
        /// Adds a resource to the next available location in the current VOL file.
        /// Used by EditGame.Compile only.
        /// </summary>
        /// <param name="AddRes"></param>
        /// <param name="replace"></param>
        internal void AddNextRes(AGIResource AddRes, bool IsV3) {
            byte[] ResHeader;

            // the DIR file is not updated; that is done by the
            // EditGame.Compile method AFTER all VOL files are built
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
            catch (Exception ex) {
                WinAGIException wex = new(EngineResourceByNum(528)) {
                    HResult = WINAGI_ERR + 528,
                };
                fsVOL?.Dispose();
                VOLWriter?.Dispose();
                wex.Data["exception"] = ex;
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
        internal void ValidateVolAndLoc(AGIResource resource, bool isV3) {
            // check if resource is too long
            if (Loc + resource.Size > parent.agMaxVolSize || (Index == 0 && (Loc + resource.Size > resource.parent.agMaxVol0))) {
                // set maxvol count to 4 or 15, depending on version
                int maxvolnum = isV3 ? 15 : 4;

                // close current vol file
                VOLFile.Dispose();
                VOLWriter.Dispose();

                // start check with previous vol files, to see if there is room at
                // end of one of those; check up to max vol number
                for (int i = 0; i <= maxvolnum; i++) {
                    try {
                        // open this file (if the file doesn't exist, this will create it
                        // and it will then be set as the next vol, with pos=0
                        VOLFile = new FileStream(Path.Combine(resource.parent.agGameDir, "NEW_VOL." + i), FileMode.OpenOrCreate);
                    }
                    catch (Exception e) {
                        Clear();
                        WinAGIException wex = new(EngineResourceByNum(528)) {
                            HResult = WINAGI_ERR + 528
                        };
                        wex.Data["exception"] = e;
                        wex.Data["ID"] = resource.ID;
                        throw wex;
                    }
                    // is there room at the end of this file?
                    if ((i > 0 && (VOLFile.Length + resource.Size <= resource.parent.agMaxVolSize)) || (VOLFile.Length + resource.Size <= resource.parent.agMaxVol0)) {
                        // if so, set pointer to end of the file, and exit
                        Index = (sbyte)i;
                        Loc = (int)VOLFile.Length;
                        if (Index >= Count) {
                            Count = Index + 1;
                        }
                        return;
                    }
                }
                // if no volume found, means all 16 are full
                WinAGIException wex1 = new(EngineResourceByNum(517)) {
                    HResult = WINAGI_ERR + 517
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
        internal static CompileStatus CompileResCol(AGIGame game, dynamic tmpResCol, AGIResType ResType, bool RebuildOnly, bool IsV3) {
            byte CurResNum;
            bool unloadRes;
            bool compError = false;
            foreach (AGIResource tmpGameRes in tmpResCol) {
                CurResNum = tmpGameRes.Number;
                // update status
                WinAGIEventInfo tmpWarn = new() {
                    Type = EventType.Info,
                    InfoType = InfoType.ClearWarnings,
                    ResType = ResType,
                    ResNum = CurResNum,
                    Module = tmpGameRes.ID,
                };
                // clear existing warnings/errors
                if (CheckForCancel(GameCompileStatus.AddResource, tmpWarn)) {
                    game.CompleteCancel();
                    return CompileStatus.Canceled;
                }
                // set info type for next action
                tmpWarn.InfoType = InfoType.Resources;
                // check for cancellation
                if (CheckForCancel(GameCompileStatus.AddResource, tmpWarn)) {
                    game.CompleteCancel();
                    return CompileStatus.Canceled;
                }
                // set flag to force unload, if resource not currently loaded
                unloadRes = !tmpGameRes.Loaded;
                // then load resource if necessary
                if (unloadRes) {
                    // load resource
                    tmpGameRes.Load();
                }
                // check for errors
                if (tmpGameRes.Error != ResourceErrorType.NoError) {
                    // add event
                    AddCompileError(tmpGameRes.ResType, tmpGameRes.Number, tmpGameRes.Error, tmpGameRes.ErrData);
                    // major error (compile automatically fails)
                    if (tmpGameRes.ResType == AGIResType.Logic) {
                        tmpWarn.Text = $"Unable to load source code for {tmpGameRes.ID}";
                    }
                    else {
                        tmpWarn.Text = $"Unable to load {tmpGameRes.ID}";
                    }
                    OnCompileGameStatus(GameCompileStatus.FatalError, tmpWarn);
                    // make sure unloaded
                    tmpGameRes.Unload();
                    // and stop compiling
                    game.CompleteCancel(true);
                    return CompileStatus.ResourceError;
                }
                // check for minor errors/warnings
                if (tmpGameRes.Warnings != 0) {
                    // add event
                    if (AddCompileWarnings(tmpGameRes.ResType, tmpGameRes.Number, tmpGameRes.Warnings, tmpGameRes.WarnData)) {
                        // unload if needed
                        if (unloadRes && tmpGameRes is not null)
                            tmpGameRes.Unload();
                        // then cancel
                        game.CompleteCancel();
                        return CompileStatus.Canceled;
                    }
                    tmpWarn.Text = $"Errors and/or anomalies detected in {tmpGameRes.ID}";
                    // check for cancellation (compile fails if user cancels)
                    if (CheckForCancel(GameCompileStatus.Warning, tmpWarn)) {
                        // unload if needed
                        if (unloadRes && tmpGameRes is not null)
                            tmpGameRes.Unload();
                        // then stop compiling
                        game.CompleteCancel();
                        return CompileStatus.Canceled;
                    }
                }
                // if full compile (not rebuild only)
                if (!RebuildOnly) {
                    // for logics, compile the sourcetext
                    if (tmpGameRes is Logic) {
                        // update status and check for cancellation
                        tmpWarn.InfoType = InfoType.Compiling;
                        if (CheckForCancel(GameCompileStatus.AddResource, tmpWarn)) {
                            // unload if needed
                            if (unloadRes && tmpGameRes is not null)
                                tmpGameRes.Unload();
                            game.CompleteCancel();
                            return CompileStatus.Canceled;
                        }
                        if (!CompileLogic((Logic)tmpGameRes)) {
                            // logic compile error- unload if needed
                            if (unloadRes && tmpGameRes is not null)
                                tmpGameRes.Unload();
                            compError = true;
                            tmpWarn.InfoType = InfoType.CompileError;
                        }
                        else {
                            // update status and check for cancellation
                            tmpWarn.InfoType = InfoType.Compiled;
                        }
                        if (CheckForCancel(GameCompileStatus.AddResource, tmpWarn)) {
                            // unload if needed
                            if (unloadRes && tmpGameRes is not null)
                                tmpGameRes.Unload();
                            game.CompleteCancel();
                            return CompileStatus.Canceled;
                        }
                    }
                }
                if (!compError) {
                    try {
                        // add it to VOL/DIR files
                        game.volManager.ValidateVolAndLoc(tmpGameRes, IsV3);
                        game.volManager.AddNextRes(tmpGameRes, IsV3);
                        // clear changed property
                        tmpGameRes.IsChanged = false;
                    }
                    catch (Exception e) {
                        tmpWarn.Type = EventType.GameCompileError;
                        tmpWarn.Text = "Unable to add resource to VOL file (" + e.Message + ")";
                        OnCompileGameStatus(GameCompileStatus.FatalError, tmpWarn);
                        // unload resource, if applicable
                        if (unloadRes && tmpGameRes is not null)
                            tmpGameRes.Unload();
                        // then stop compiling
                        game.CompleteCancel(true);
                        return CompileStatus.ResourceError;
                    }
                }
                // done with resource; unload if applicable
                if (unloadRes && tmpGameRes is not null) {
                    tmpGameRes.Unload();
                }
            }
            if (compError) {
                return CompileStatus.LogicCompileError;
            }
            else {
                return CompileStatus.OK;
            }
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
            int header, maxVol, j;
            int previousend, nextstart, freespace;

            // set header size, max# of VOL files depending on version
            if (resource.parent.agIntVersion.IsV3) {
                header = 7;
                maxVol = 15;
            }
            else {
                header = 5;
                maxVol = 4;
            }
            // build array of all resources, sorted by VOL order
            // skpping the target resource if it's not yet added
            // (i.e. if its VOL/Loc values are -1) OR if it's the 
            // target resource
            foreach (Logic tmpRes in resource.parent.agLogs.Col.Values) {
                if (tmpRes.Loc >= 0 && tmpRes != resource) {
                    addInfo.Loc = tmpRes.Loc;
                    addInfo.Size = tmpRes.SizeInVOL + header;
                    resInfo[tmpRes.Volume].Add(tmpRes.Loc, addInfo);
                }
            }
            foreach (Picture tmpRes in resource.parent.agPics.Col.Values) {
                if (tmpRes.Loc >= 0 && tmpRes != resource) {
                    addInfo.Loc = tmpRes.Loc;
                    addInfo.Size = tmpRes.SizeInVOL + header;
                    resInfo[tmpRes.Volume].Add(tmpRes.Loc, addInfo);
                }
            }
            foreach (Sound tmpRes in resource.parent.agSnds.Col.Values) {
                if (tmpRes.Loc >= 0 && tmpRes != resource) {
                    addInfo.Loc = tmpRes.Loc;
                    addInfo.Size = tmpRes.SizeInVOL + header;
                    resInfo[tmpRes.Volume].Add(tmpRes.Loc, addInfo);
                }
            }
            foreach (View tmpRes in resource.parent.agViews.Col.Values) {
                if (tmpRes.Loc >= 0 && tmpRes != resource) {
                    addInfo.Loc = tmpRes.Loc;
                    addInfo.Size = tmpRes.SizeInVOL + header;
                    resInfo[tmpRes.Volume].Add(tmpRes.Loc, addInfo);
                }
            }
            // step through volumes,
            for (int i = 0; i <= maxVol; i++) {
                // start at beginning
                previousend = 0;
                // check for space between resources
                for (j = 0; j <= resInfo[i].Count; j++) {
                    // if this is not the end of the list
                    if (j < resInfo[i].Count) {
                        nextstart = resInfo[i].Values[j].Loc;
                    }
                    else {
                        nextstart = i == 0 ? resource.parent.agMaxVol0 : resource.parent.agMaxVolSize;
                    }
                    // calculate space between end of one resource and start of next
                    freespace = nextstart - previousend;
                    if (freespace >= resource.Size + header) {
                        // it fits here!
                        resource.Volume = (sbyte)i;
                        resource.Loc = previousend;
                        return;
                    }
                    // not enough room
                    if (j < resInfo[i].Count) {
                        // adjust start to end of current resource
                        previousend = resInfo[i].Values[j].Loc + resInfo[i].Values[j].Size;
                    }
                }
            }
            // if no room in any VOL file, raise an error
            WinAGIException wex = new(EngineResourceByNum(517)) {
                HResult = WINAGI_ERR + 517
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
            // adds it there. If replacing an existing resource it will
            // not delete the resource data from its old position (but
            // the area will be available for future use by another resource)
            // and then it updates the DIR file.
            byte[] ResHeader;
            string gameid;

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
            if (AddRes.parent.agIntVersion.IsV3) {
                Array.Resize(ref ResHeader, 7);
                ResHeader[5] = ResHeader[3];
                ResHeader[6] = ResHeader[4];
                gameid = AddRes.parent.agGameID.ToUpper();
            }
            else {
                gameid = "";
            }
            FileStream fsVOL = null;
            BinaryWriter bwVOL = null;
            try {
                // save the resource into the vol file
                fsVOL = new FileStream(Path.Combine(AddRes.parent.agGameDir, gameid + "VOL." + AddRes.Volume.ToString()),
                    FileMode.OpenOrCreate);
                fsVOL.Seek(AddRes.Loc, SeekOrigin.Begin);
                bwVOL = new BinaryWriter(fsVOL);
                bwVOL.Write(ResHeader, 0, AddRes.parent.agIntVersion.IsV3 ? 7 : 5);
                bwVOL.Write(AddRes.Data, 0, AddRes.Data.Length);
                fsVOL?.Dispose();
                bwVOL?.Dispose();
            }
            catch (Exception ex) {
                WinAGIException wex = new(EngineResourceByNum(528)) {
                    HResult = WINAGI_ERR + 528,
                };
                fsVOL?.Dispose();
                bwVOL?.Dispose();
                wex.Data["exception"] = ex;
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
            string dirFile;
            byte[] dirData, dirByte = new byte[3];
            int max = 0, oldmax;
            int dirOffset = 0, dirEnd = 0, i, start, stop;
            FileStream fsDIR;

            if (remove) {
                // resource marked for deletion
                dirByte[0] = 0xFF;
                dirByte[1] = 0xFF;
                dirByte[2] = 0xFF;
            }
            else {
                // calculate directory bytes
                dirByte[0] = (byte)((resource.Volume << 4) + ((uint)resource.Loc >> 16));
                dirByte[1] = (byte)((resource.Loc % 0x10000) >> 8);
                dirByte[2] = (byte)(resource.Loc % 0x100);
            }
            // what is current max for this res type?
            switch (resource.ResType) {
            case AGIResType.Logic:
                max = resource.parent.agLogs.Max;
                break;
            case AGIResType.Picture:
                max = resource.parent.agPics.Max;
                break;
            case AGIResType.Sound:
                max = resource.parent.agSnds.Max;
                break;
            case AGIResType.View:
                max = resource.parent.agViews.Max;
                break;
            }
            if (resource.parent.agIntVersion.IsV3) {
                dirFile = Path.Combine(resource.parent.agGameDir, resource.parent.agGameID + "DIR");
            }
            else {
                dirFile = Path.Combine(resource.parent.agGameDir, ResTypeAbbrv[(int)resource.ResType] + "DIR");
            }
            if (!File.Exists(dirFile)) {
                throw new FileNotFoundException(dirFile);
            }
            // check for readonly
            if ((File.GetAttributes(dirFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                WinAGIException wex = new(EngineResourceByNum(539).Replace(ARG1, dirFile)) {
                    HResult = WINAGI_ERR + 539,
                };
                wex.Data["badfile"] = dirFile;
                throw wex;
            }
            try {
                using (fsDIR = new FileStream(dirFile, FileMode.Open)) {
                    dirData = new byte[fsDIR.Length];
                    fsDIR.Read(dirData, 0, (int)fsDIR.Length);
                    fsDIR.Dispose();
                }
            }
            catch (Exception e) {
                WinAGIException wex = new(EngineResourceByNum(541)) {
                    HResult = WINAGI_ERR + 541,
                };
                wex.Data["exception"] = e;
                wex.Data["dirfile"] = dirFile;
                throw wex;
            }
            // calculate old max and offset (for v3 files)
            if (resource.parent.agIntVersion.IsV3) {
                // calculate directory offset
                switch (resource.ResType) {
                case AGIResType.Logic:
                    dirOffset = 8;
                    dirEnd = (dirData[3] << 8) + dirData[2];
                    break;
                case AGIResType.Picture:
                    dirOffset = (dirData[3] << 8) + dirData[2];
                    dirEnd = (dirData[5] << 8) + dirData[4];
                    break;
                case AGIResType.View:
                    dirOffset = (dirData[5] << 8) + dirData[4];
                    dirEnd = (dirData[7] << 8) + dirData[6];
                    break;
                case AGIResType.Sound:
                    dirOffset = (dirData[7] << 8) + dirData[6];
                    dirEnd = dirData.Length;
                    break;
                }
                oldmax = (dirEnd - dirOffset) / 3 - 1;
            }
            else {
                dirEnd = (dirData.Length);
                oldmax = dirEnd / 3 - 1;
                dirOffset = 0;
            }

            // if it fits (doesn't matter if inserting or deleting)
            if ((remove && (max >= oldmax)) || (!remove && (oldmax >= max))) {
                // adjust offset for resnum
                dirOffset += 3 * resource.Number;
                // insert the new data, and save the file
                try {
                    using (fsDIR = new FileStream(dirFile, FileMode.Open)) {
                        _ = fsDIR.Seek(dirOffset, SeekOrigin.Begin);
                        fsDIR.Write(dirByte);
                        fsDIR.Dispose();
                    }
                    return;
                }
                catch (Exception e) {
                    WinAGIException wex = new(EngineResourceByNum(541)) {
                        HResult = WINAGI_ERR + 541,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["dirfile"] = dirFile;
                    throw wex;
                }
            }
            // dir file size needs to change
            if (remove) {
                // must be shrinking - for v2, just resize the array
                if (!resource.parent.agIntVersion.IsV3) {
                    Array.Resize(ref dirData, (max + 1) * 3);
                }
                else {
                    // if restype is sound, we also can just truncate the file
                    if (resource.ResType == AGIResType.Sound) {
                        Array.Resize(ref dirData, dirData.Length - 3 * (oldmax - max));
                    }
                    else {
                        // need to move data from the current directory's max
                        // backwards to compress the directory
                        // start with resource just after new max; then move all bytes down
                        start = dirOffset + max * 3 + 3;
                        stop = dirData.Length - 3 * (oldmax - max);
                        for (i = start; i < stop; i++) {
                            dirData[i] = dirData[i + 3 * (oldmax - max)];
                        }
                        // now shrink the array
                        Array.Resize(ref dirData, dirData.Length - 3 * (oldmax - max));
                        // then we need to update affected offsets
                        // move snddir offset first
                        dirOffset = (dirData[7] << 8) + dirData[6];
                        dirOffset -= 3 * (oldmax - max);
                        dirData[7] = (byte)(dirOffset >> 8);
                        dirData[6] = (byte)(dirOffset % 0x100);
                        // if resource is a view, we are done
                        if (resource.ResType != AGIResType.View) {
                            // move view offset
                            dirOffset = (dirData[5] << 8) + dirData[4];
                            dirOffset -= 3 * (oldmax - max);
                            dirData[5] = (byte)(dirOffset >> 8);
                            dirData[4] = (byte)(dirOffset % 0x100);
                            // if resource is a pic, now we are done
                            if (resource.ResType != AGIResType.Picture) {
                                // move picture offset
                                dirOffset = (dirData[3] << 8) + dirData[2];
                                dirOffset -= 3 * (oldmax - max);
                                dirData[3] = (byte)(dirOffset >> 8);
                                dirData[2] = (byte)(dirOffset % 0x100);
                            }
                        }
                    }
                }
                try {
                    // delete the existing file
                    SafeFileDelete(dirFile);
                    // now create the new file
                    using (fsDIR = new FileStream(dirFile, FileMode.OpenOrCreate)) {
                        fsDIR.Write(dirData, 0, dirData.Length);
                        fsDIR.Dispose();
                    }
                }
                catch (Exception e) {
                    WinAGIException wex = new(EngineResourceByNum(541)) {
                        HResult = WINAGI_ERR + 541,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["dirfile"] = dirFile;
                    throw wex;
                }
            }
            else {
                // must be expanding
                Array.Resize(ref dirData, dirData.Length + 3 * (max - oldmax));
                // if v2, add 0xffs to fill gap up to the last entry
                if (!resource.parent.agIntVersion.IsV3) {
                    start = dirEnd;
                    stop = dirEnd + 3 * (max - oldmax - 1) - 1;
                    for (i = start; i <= stop; i++) {
                        dirData[i] = 0xFF;
                    }
                    // add updated dir data to end
                    dirData[++stop] = dirByte[0];
                    dirData[++stop] = dirByte[1];
                    dirData[++stop] = dirByte[2];
                }
                else {
                    // if expanding the sound dir, just fill it in with 0xffs
                    if (resource.ResType == AGIResType.Sound) {
                        start = dirEnd;
                        stop = dirEnd + 3 * (max - oldmax - 1) - 1;
                        for (i = start; i <= stop; i++) {
                            dirData[i] = 0xFF;
                        }
                        // add data to end
                        dirData[++stop] = dirByte[0];
                        dirData[++stop] = dirByte[1];
                        dirData[++stop] = dirByte[2];
                    }
                    else {
                        // move data to make room for inserted resource
                        stop = dirData.Length - 1;
                        start = dirEnd + 3 * (max - oldmax);
                        for (i = stop; i >= start; i--) {
                            dirData[i] = dirData[i - 3 * (max - oldmax)];
                        }
                        // insert ffs, up to insert location
                        stop = start - 4;
                        start = dirEnd;
                        for (i = start; i <= stop; i++) {
                            dirData[i] = 0xFF;
                        }
                        // add updated dir data to end
                        dirData[++stop] = dirByte[0];
                        dirData[++stop] = dirByte[1];
                        dirData[++stop] = dirByte[2];
                        // then adjust the offsets:
                        // move snddir offset first
                        dirOffset = (dirData[7] << 8) + dirData[6];
                        dirOffset += 3 * (max - oldmax);
                        dirData[7] = (byte)(dirOffset >> 8);
                        dirData[6] = (byte)(dirOffset % 0x100);
                        // if resource is a view, we are done
                        if (resource.ResType != AGIResType.View) {
                            // move view offset
                            dirOffset = ((dirData[5] << 8) + dirData[4]);
                            dirOffset += 3 * (max - oldmax);
                            dirData[5] = (byte)(dirOffset >> 8);
                            dirData[4] = (byte)(dirOffset % 0x100);
                            // if resource is a pic, we are done
                            if (resource.ResType != AGIResType.Picture) {
                                // move picture offset
                                dirOffset = ((dirData[3] << 8) + dirData[2]);
                                dirOffset += 3 * (max - oldmax);
                                dirData[3] = (byte)(dirOffset >> 8);
                                dirData[2] = (byte)(dirOffset % 0x100);
                            }
                        }
                    }
                }
                // now write the array back to the file
                try {
                    using (fsDIR = new FileStream(dirFile, FileMode.Open)) {
                        fsDIR.Write(dirData, 0, dirData.Length);
                        fsDIR.Dispose();
                    }
                }
                catch (Exception e) {
                    WinAGIException wex = new(EngineResourceByNum(541)) {
                        HResult = WINAGI_ERR + 541,
                    };
                    wex.Data["exception"] = e;
                    wex.Data["dirfile"] = dirFile;
                    throw wex;
                }
            }
        }
        #endregion
    }
}
