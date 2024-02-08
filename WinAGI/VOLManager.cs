using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinAGI.Engine.AGIGame;
using System.IO;

namespace WinAGI.Engine
{
    public static partial class Base
    {
        internal static int lngCurrentLoc;
        internal static sbyte lngCurrentVol;
        internal static BinaryWriter bwDIR, bwVOL;
        internal static BinaryReader brDIR, brVOL;
        internal static FileStream fsDIR, fsVOL;
        internal static byte[,] bytDIR = new byte[4, 768]; // (3, 767)
        internal static string strNewDir;

        internal static void CompileResCol(dynamic tmpResCol, AGIResType ResType, bool RebuildOnly, bool NewIsV3)
        {

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
            byte CurResNum = 0;
            int lngV3Offset;
            string strMsg = "", strError;
            bool blnWarning = false, blnUnloadRes = false;

            //add all resources of this type
            foreach (AGIResource tmpGameRes in tmpResCol) {
                CurResNum = tmpGameRes.Number;
                //update status
                Raise_CompileGameEvent(ECStatus.csAddResource, ResType, CurResNum, "");
                //check for cancellation
                if (!tmpGameRes.parent.agCompGame) {
                    tmpGameRes.parent.CompleteCancel();
                    return;
                }

                //use a loop to add resources;
                //exit loop when an error occurs, or after
                //resource is successfully added to vol file
                do {
                    //set flag to force unload, if resource not currently loaded
                    blnUnloadRes = !tmpGameRes.Loaded;
                    //then load resource if necessary
                    if (blnUnloadRes) {
                        try {
                            //always reset warning flag
                            blnWarning = false;

                            //load resource
                            tmpGameRes.Load();
                        }
                        catch (Exception e) {

                            if (RebuildOnly) {
                                //note it
                                Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, "Unable to load " + tmpGameRes.ID + " (" + e.Message + ")");
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
                                //if error,
                                if (e.HResult != 610) {
                                    //the 610 error is the one related to bitmap creation; it doesn't really
                                    //mean there's an error

                                    //assume a resource error until determined otherwise
                                    strMsg = "Unable to load " + tmpGameRes.ID + " (" + e.Message + ")";
                                    //reset warning flag
                                    blnWarning = false;
                                    switch (ResType) {
                                    case AGIResType.rtPicture:
                                    case AGIResType.rtView:
                                        //check for invalid view data errors
                                        switch (e.HResult) {
                                        case 537:
                                        case 548:
                                        case 552:
                                        case 553:
                                        case 513:
                                        case 563:
                                        case 539:
                                        case 540:
                                        case 550:
                                        case 551:
                                            //can add view, but it is invalid
                                            strMsg = "Invalid view data (" + e.Message + ")";
                                            blnWarning = true;
                                            break;
                                        }
                                        break;
                                    case AGIResType.rtSound:
                                        //check for invalid sound data errors
                                        if (e.HResult == 598) {
                                            //can add sound, but it is invalid
                                            strMsg = "Invalid sound data (" + e.Message + ")";
                                            blnWarning = true;
                                        }
                                        break;
                                    case AGIResType.rtLogic:
                                        //always a resource error
                                        break;
                                    }
                                    //if error (warning not set)
                                    if (!blnWarning) {
                                        //note the error
                                        Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, strMsg);
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
                                }
                            }
                            //if not canceled, user has already removed bad resource
                            break;
                        }
                    }

                    //if full compile (not rebuild only)
                    if (!RebuildOnly) {
                        //game resource is verified open; check for warnings

                        //picture warnings aren't in error handler anymore and 
                        // need a special check here
                        if (tmpGameRes is Picture tmpPic) {
                            //check bmp error level by property, not error number
                            if (tmpPic.BMPErrLevel >= 0) {
                                //case 0:
                                //ok
                                //unhandled error
                                strMsg = "Unhandled error in picture data- picture may not display correctly";
                                blnWarning = true;
                            }
                            else {
                                //missing EOP marker, bad color or bad cmd
                                strMsg = "Data anomalies in picture data but picture should display";
                                blnWarning = true;
                            }
                        }

                        //if a warning
                        if (blnWarning) {
                            //note the warning
                            Raise_CompileGameEvent(ECStatus.csWarning, ResType, CurResNum, "--|" + strMsg + "|--|--");
                            //check for cancellation
                            if (!tmpGameRes.parent.agCompGame) {
                                tmpGameRes.parent.CompleteCancel();
                                // unload if needed
                                if (blnUnloadRes && tmpGameRes != null) tmpGameRes.Unload();
                                // then stop compiling
                                return;
                            }
                        }
                        //for logics, compile the sourcetext
                        //load actual object
                        if (tmpGameRes is Logic tmpLog) {
                            try {
                                //compile it
                                Compiler.CompileLogic(tmpLog);
                            }
                            catch (Exception e) {
                                switch (e.HResult) {
                                case 635: //compile error
                                          //raise compile event
                                    Raise_CompileGameEvent(ECStatus.csLogicError, ResType, CurResNum, e.Message);
                                    //check for cancellation
                                    if (!tmpGameRes.parent.agCompGame) {
                                        tmpGameRes.parent.CompleteCancel();
                                        // unload if needed
                                        if (blnUnloadRes && tmpGameRes != null) tmpGameRes.Unload();
                                        // then stop compiling
                                        return;
                                    }
                                    //if user wants to continue, the logic will already have been removed; no other action needed
                                    break;
                                default:
                                    //any other error; note it
                                    Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, "Unable to compile Logic (" + e.Message + ")");
                                    //check for cancellation
                                    if (!tmpGameRes.parent.agCompGame) {
                                        tmpGameRes.parent.CompleteCancel();
                                        // unload if needed
                                        if (blnUnloadRes && tmpGameRes != null) tmpGameRes.Unload();
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
                        ValidateVolAndLoc(tmpGameRes);
                    }
                    catch (Exception e) {
                        //save error msg
                        strError = e.Message;
                        //strErrSrc = Err.Source;
                        lngError = e.HResult;

                        //clean up compiler
                        tmpGameRes.parent.CompleteCancel(true);

                        //unload resource, if applicable
                        if (blnUnloadRes && tmpGameRes != null) tmpGameRes.Unload();

                        //raise appropriate error
                        if (lngError == 593) {
                            //exceed max storage
                            throw;
                        }
                        else {
                            //file access error

                            Exception eR = new(LoadResString(638).Replace(Common.Base.ARG1, e.Message))
                            {
                                HResult = 638
                            };
                            throw eR;
                        }
                    }

                    //new strategy is to use arrays for DIR data, and then
                    //to build the DIR files after compiling all resources
                    bytDIR[(int)ResType, tmpGameRes.Number * 3] = (byte)((lngCurrentVol << 4) + (lngCurrentLoc >> 16));
                    bytDIR[(int)ResType, tmpGameRes.Number * 3 + 1] = (byte)((lngCurrentLoc % 0x10000) >> 8);
                    bytDIR[(int)ResType, tmpGameRes.Number * 3 + 2] = (byte)(lngCurrentLoc % 0x100);

                    try {
                        //add it to vol
                        AddToVol(tmpGameRes, NewIsV3, true, lngCurrentVol, lngCurrentLoc);
                    }
                    catch (Exception e) {
                        //note it
                        Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, "Unable to add Logic resource to VOL file (" + e.Message + ")");
                        //check for cancellation
                        if (!tmpGameRes.parent.agCompGame) {
                            tmpGameRes.parent.CompleteCancel();
                            //unload resource, if applicable
                            if (blnUnloadRes && tmpGameRes != null) tmpGameRes.Unload();
                            // then stop compiling
                            return;
                        }
                        // if not canceled, user deleted bad resource
                        // so exit loop to move to next resource
                        break;
                    }
                    //always exit loop
                }
                while (false);

                //done with resource; unload if applicable
                if (blnUnloadRes && tmpGameRes != null) tmpGameRes.Unload();
            }
        }
        internal static void AddToVol(AGIResource AddRes, bool Version3, bool NewVOL = false, sbyte lngVol = -1, int lngLoc = -1)
        {
            //this method will add a resource to a VOL file
            //
            //if the NewVOL flag is true, it adds the resource
            //to the specified vol file at the specified location
            //the DIR file is not updated; that is done by the
            //CompileGame method (which calls this method)
            //
            //if the NewVOL flag is false, it finds the first
            //available spot for the resource, and adds it there
            //the method will add the resource at a new location based
            //on first open position; it will not delete the resource
            //data from its old position (but the area will be available
            //for future use by another resource)
            //and then it updates the DIR file
            //
            //only resources that are in a game can be added to a VOL file
            //

            byte[] ResHeader = Array.Empty<byte>();
            string strID;
            //should NEVER get here for a resource
            //that is NOT in a game

            //if NOT adding to a new VOL file
            if (!NewVOL) {
                try {
                    //get vol number and location where there is room for this resource
                    FindFreeVOLSpace(AddRes);
                }
                catch (Exception e) {
                    //pass it along
                    lngError = e.HResult;
                    strError = e.Message;
                    //strErrSrc = Err.Source
                    throw new Exception(strError);
                }
                lngLoc = AddRes.Loc;
                lngVol = AddRes.Volume;
            }
            else {
                //need to verify valid values are passed
                if (lngLoc < 0 || lngVol < 0) {
                    //error!!!
                    //TODO: add appropriate error handling here
                }
            }

            //build header
            ResHeader = new byte[5];
            ResHeader[0] = 0x12;
            ResHeader[1] = 0x34;
            ResHeader[2] = (byte)lngVol;
            ResHeader[3] = (byte)(AddRes.Size % 256);
            ResHeader[4] = (byte)(AddRes.Size / 256);

            //if the resource is a version 3 resource,
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
            if (!NewVOL) {
                //save the resource into the vol file
                //get as file number
                fsVOL = new FileStream(AddRes.parent.agGameDir + strID + "VOL." + lngVol.ToString(), FileMode.Open);
                bwVOL = new BinaryWriter(fsVOL);
            }
            try {
                fsVOL.Seek(lngLoc, SeekOrigin.Begin);
                //add header to vol file
                bwVOL.Write(ResHeader, 0, AddRes.parent.agIsVersion3 ? 7 : 5);
                // add resource data to vol file
                bwVOL.Write(AddRes.Data.AllData, 0, AddRes.Data.Length);
            }
            catch (Exception e) {
                //save error
                strError = e.Message;
                //strErrSrc = Err.Source
                lngError = e.HResult;

                //if not adding to a new file, close volfile
                if (!NewVOL) {
                    fsVOL.Dispose();
                    bwVOL.Dispose();
                }
                throw new Exception("638 :" + strError);
            }
            ////always update sizeinvol
            //SetSizeInVol(AddRes, AddRes.Size);

            //if adding to a new vol file
            if (NewVOL) {
                //increment loc pointer
                lngCurrentLoc = lngCurrentLoc + AddRes.Size + 5;
                //if version3
                if (Version3) {
                    //add two more
                    lngCurrentLoc += 2;
                }
            }
            else {
                try {
                    //update location in dir file
                    UpdateDirFile(AddRes);
                }
                catch (Exception e) {
                    //only error that will be returned is expandv3dir error
                    //pass it along
                    lngError = e.HResult;
                    strError = e.Message;
                    //strErrSrc = Err.Source
                    throw new Exception("lngError" + strError);
                }
            }
        }
        private static void AddResInfo(sbyte ResVOL, int ResLOC, int ResSize, int[,] VOLLoc, int[,] VOLSize)
        {
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
                VOLLoc[ResVOL, 0] = VOLLoc[ResVOL, 0] + 1;

                //now move all items ABOVE i up one space
                for (j = VOLLoc[ResVOL, 0]; j > i; i--) {
                    VOLLoc[ResVOL, j] = VOLLoc[ResVOL, j - 1];
                    VOLSize[ResVOL, j] = VOLSize[ResVOL, j - 1];
                }

                //store new data at position i
                VOLLoc[ResVOL, i] = ResLOC;
                VOLSize[ResVOL, i] = ResSize;
            }
        }
        private static void FindFreeVOLSpace(AGIResource NewResource)
        {
            //this method will try to find a volume and location to store a resource
            //if a resource is being updated, it will have its volume set to 255

            //sizes are adjusted to include the header that AGI uses at the beginning of each
            //resource; 5 bytes for v2 and 7 bytes for v3

            int[,] lngLoc = new int[15, 1023];
            int[,] lngSize = new int[15, 1023];
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
                    AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc, lngSize);
                }
            }
            foreach (Picture tmpRes in NewResource.parent.agPics.Col.Values) {
                //if not the resource being replaced
                if (NewResType != AGIResType.rtPicture || tmpRes.Number != NewResNum) {
                    tmpSize = tmpRes.SizeInVOL + lngHeader;
                    AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc, lngSize);
                }
            }
            foreach (Sound tmpRes in NewResource.parent.agSnds.Col.Values) {
                //if not the resource being replaced
                if (NewResType != AGIResType.rtSound || tmpRes.Number != NewResNum) {
                    tmpSize = tmpRes.SizeInVOL + lngHeader;
                    AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc, lngSize);
                }
            }
            foreach (View tmpRes in NewResource.parent.agViews.Col.Values) {
                //if not the resource being replaced
                if (NewResType != AGIResType.rtView || tmpRes.Number != NewResNum) {
                    tmpSize = tmpRes.SizeInVOL + lngHeader;
                    AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc, lngSize);
                }
            }
            //step through volumes,
            for (sbyte i = 0; i <= lngMaxVol; i++) {
                //start at beginning
                lngStart = 0;
                // check for space between resources
                for (j = 1; j <= lngLoc[i, 0] + 1; j++) {
                    //if this is not the end of the list
                    if (j < lngLoc[i, 0] + 1) {
                        lngEnd = lngLoc[i, j];
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
                    lngStart = lngLoc[i, j] + lngSize[i, j];
                }
            }
            //if no room in any VOL file, raise an error

            Exception e = new(LoadResString(593))
            {
                HResult = 593
            };
            throw e;
        }
        internal static void UpdateDirFile(AGIResource UpdateResource, bool Remove = false)
        {
            //this method updates the DIR file with the volume and location
            //of a resource
            //if Remove option passed as true,
            //the resource is treated as //deleted// and
            // 0xFFFFFF is written in the resource//s DIR file place holder

            //NOTE: directories inside a V3 dir file are in this order: LOGIC, PICTURE, VIEW, SOUND
            //the ResType enumeration is in this order: LOGIC, PICTURE, SOUND, VIEW
            //because of the switch between VIEW and SOUND, can't use a formula to calculate
            //directory offsets
            string strDirFile;
            byte[] bytDIR, DirByte = new byte[2];
            int intMax = 0, intOldMax;
            int lngDirOffset = 0, lngDirEnd = 0, i, lngStart, lngStop;
            //strategy:
            //if deleting--
            //    is the the dir larger than the new max?
            //    yes: compress the dir
            //    no:  insert 0xFFs
            //
            //if adding--
            //    is the dir too small?
            //    yes: expand the dir
            //    no:  insert the data
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
            //if version3
            if (UpdateResource.parent.agIsVersion3) {
                strDirFile = UpdateResource.parent.agGameDir + UpdateResource.parent.agGameID + "DIR";
            }
            else {
                strDirFile = UpdateResource.parent.agGameDir + ResTypeAbbrv[(int)UpdateResource.ResType] + "DIR";
            }
            try {
                fsDIR = new FileStream(strDirFile, FileMode.Open);
                bytDIR = new byte[fsDIR.Length];
                fsDIR.Read(bytDIR, 0, (int)fsDIR.Length);
            }
            catch (Exception) {
                //error? what to do???
                fsDIR.Dispose();
                throw new Exception("can't open DIR for updating");
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

            //if it fits (doesn't matter if inserting or deleting)
            if ((Remove && (intMax >= intOldMax)) || (!Remove && (intOldMax >= intMax))) {
                //adjust offset for resnum
                lngDirOffset += 3 * UpdateResource.Number;
                //just insert the new data, and save the file
                try {
                    fsDIR.Seek(lngDirOffset, SeekOrigin.Begin);
                    fsDIR.Write(bytDIR, 0, 3);
                }
                catch (Exception) {
                    throw new Exception("unable to update the DIR file...");
                }
                return;
            }

            //size has changed!
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
                        //we need to move data from the current directory's max
                        //backwards to compress the directory
                        //start with resource just after new max; then move all bytes down
                        lngStart = lngDirOffset + intMax * 3 + 3;
                        lngStop = bytDIR.Length - 3 * (intOldMax - intMax);
                        for (i = lngStart; i < lngStop; i++) {
                            bytDIR[i] = bytDIR[i + 3 * (intOldMax - intMax)];
                        } //Next i
                          //now shrink the array
                        Array.Resize(ref bytDIR, bytDIR.Length - 3 * (intOldMax - intMax));
                        //then we need to update affected offsets
                        //move snddir offset first
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

                //delete the existing file
                fsDIR.Dispose();
                File.Delete(strDirFile);
                //now save the file
                fsDIR = new FileStream(strDirFile, FileMode.OpenOrCreate);
                fsDIR.Write(bytDIR, 0, bytDIR.Length);
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

                //now save the file
                fsDIR.Dispose();
                fsDIR = new FileStream(strDirFile, FileMode.Open);
                fsDIR.Write(bytDIR, 0, bytDIR.Length);
            }
        }
        private static void ValidateVolAndLoc(AGIResource resource)
        {
            //this method ensures current vol has room for resource with given size
            //if not, it closes current vol file, and opens next one
            //this method is only used by the game compiler when doing a
            //complete compile or resource rebuild

            int i = 0, lngMaxVol = 0;
            //this ressource doesn't fit (goes past end, OR if it's vol 0, and it exceeds vol0 size)
            if (lngCurrentLoc + resource.Size > MAX_VOLSIZE || (lngCurrentVol == 0 && (lngCurrentLoc + resource.Size > resource.parent.agMaxVol0))) {
                //set maxvol count to 4 or 15, depending on version
                if (resource.parent.agIsVersion3) {
                    lngMaxVol = 15;
                }
                else {
                    lngMaxVol = 4;
                }

                //close current vol
                fsVOL.Dispose();
                bwVOL.Dispose();

                //first check previous vol files, to see if there is room at end of one of those
                for (i = 0; i <= lngMaxVol; i++) {
                    try {
                        //open this file (if the file doesn't exist, this will create it
                        //and it will then be set as the next vol, with pos=0
                        fsVOL = new FileStream(strNewDir + "NEW_VOL." + i, FileMode.OpenOrCreate);
                        bwVOL = new BinaryWriter(fsVOL);
                    }
                    catch (Exception) {
                        fsVOL.Dispose();
                        bwVOL.Dispose();
                        //also, compiler should check for this error, as it is fatal

                        Exception eR = new(LoadResString(640))
                        {
                            HResult = 640
                        };
                        throw eR;
                    }
                    //is there room at the end of this file?
                    if ((i > 0 && (fsVOL.Length + resource.Size <= MAX_VOLSIZE)) || (fsVOL.Length + resource.Size <= resource.parent.agMaxVol0)) {
                        //if so, set pointer to end of the file, and exit
                        lngCurrentVol = (sbyte)i;
                        lngCurrentLoc = (int)fsVOL.Length;
                        return;
                    }
                } //Next i

                //if no volume found, we//ve got a problem...
                //raise error!
                //also, compiler should check for this error, as it is fatal

                Exception e = new(LoadResString(593))
                {
                    HResult = 0
                };
                throw e;
            }
        }
    }
}
