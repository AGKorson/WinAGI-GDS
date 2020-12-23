using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static WinAGI.AGIGame;
using System.IO;

namespace WinAGI
{
  public static partial class WinAGI
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
      foreach (AGIResource tmpGameRes in tmpResCol)
      {
        CurResNum = tmpGameRes.Number;
        //update status
        Raise_CompileGameEvent(ECStatus.csAddResource, ResType, CurResNum, "");
        //check for cancellation
        if (!agCompGame)
        {
          CompleteCancel();
          return;
        }

        //use a loop to add resources;
        //exit loop when an error occurs, or after
        //resource is successfully added to vol file
        do
        {
          //set flag to force unload, if resource not currently loaded
          blnUnloadRes = !tmpGameRes.Loaded;
          //then load resource if necessary
          if (blnUnloadRes)
          {
            try
            {
              //always reset warning flag
              blnWarning = false;

              //load resource
              tmpGameRes.Load();
            }
            catch (Exception e)
            {

              if (RebuildOnly)
              {
                //note it
                Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, "Unable to load " + tmpGameRes.ID + " (" + e.Message + ")");
                //check for cancellation
                if (!agCompGame)
                {
                  CompleteCancel();
                  // make sure unloaded
                  tmpGameRes.Unload();
                  // and stop compiling
                  return;
                }
              }
              else
              {
                //if error,
                if (e.HResult !=  610)
                {
                  //the 610 error is the one related to bitmap creation; it doesn't really
                  //mean there's an error

                  //assume a resource error until determined otherwise
                  strMsg = "Unable to load " + tmpGameRes.ID + " (" + e.Message + ")";
                  //reset warning flag
                  blnWarning = false;
                  switch (ResType)
                  {
                    case AGIResType.rtPicture:
                    case AGIResType.rtView:
                      //check for invalid view data errors
                      switch (e.HResult)
                      {
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
                      if (e.HResult == 598)
                      {
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
                  if (!blnWarning)
                  {
                    //note the error
                    Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, strMsg);
                    //check for cancellation
                    if (!agCompGame)
                    {
                      CompleteCancel();
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
          if (!RebuildOnly)
          {
            //game resource is verified open; check for warnings

            //picture warnings aren't in error handler anymore and 
            // need a special check here
            if (tmpGameRes is AGIPicture tmpPic)
            {
              //check bmp error level by property, not error number
              if (tmpPic.BMPErrLevel >= 0)
              {
                //case 0:
                //ok
                //unhandled error
                strMsg = "Unhandled error in picture data- picture may not display correctly";
              blnWarning = true;
              }
              else
              {
                //missing EOP marker, bad color or bad cmd
                strMsg = "Data anomalies in picture data but picture should display";
                blnWarning = true;
              }
            }

            //if a warning
            if (blnWarning)
            {
              //note the warning
              Raise_CompileGameEvent(ECStatus.csWarning, ResType, CurResNum, "--|" + strMsg + "|--|--");
              //check for cancellation
              if (!agCompGame)
              {
                CompleteCancel();
                // unload if needed
                if (blnUnloadRes && tmpGameRes != null) tmpGameRes.Unload();
                // then stop compiling
                return;
              }
            }
            //for logics, compile the sourcetext
            //load actual object
            if (tmpGameRes is AGILogic tmpLog)
            {
              try
              {
                //compile it
                CompileLogic(tmpLog);
              }
              catch (Exception e)
              {
                switch (e.HResult)
                {
                  case 635: //compile error
                                            //raise compile event
                    Raise_CompileGameEvent(ECStatus.csLogicError, ResType, CurResNum, e.Message);
                    //check for cancellation
                    if (!agCompGame)
                    {
                      CompleteCancel();
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
                    if (!agCompGame)
                    {
                      CompleteCancel();
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

          try
          {
            //validate vol and loc
            ValidateVolAndLoc(tmpGameRes.Size);
          }
          catch (Exception e)
          {
            //save error msg
            strError = e.Message;
            //strErrSrc = Err.Source;
            lngError = e.HResult;

            //clean up compiler
            CompleteCancel(true);

            //unload resource, if applicable
            if (blnUnloadRes && tmpGameRes != null) tmpGameRes.Unload();

            //raise appropriate error
            if (lngError == 593)
            { 
              //exceed max storage
              throw new Exception("593, CompResCol, LoadResString(593)");
            }
            else
            { 
              //file access error
              throw new Exception("638, strErrSrc, LoadResString(638), (lngError) + ':' + strError)");
            }
          }

          //new strategy is to use arrays for DIR data, and then
          //to build the DIR files after compiling all resources
          bytDIR[(int)ResType, tmpGameRes.Number * 3] = (byte)(lngCurrentVol * 0x10 + lngCurrentLoc / 0x10000);
          bytDIR[(int)ResType, tmpGameRes.Number * 3 + 1] = (byte)((lngCurrentLoc % 0x10000) / 0x100);
          bytDIR[(int)ResType, tmpGameRes.Number * 3 + 2] = (byte)(lngCurrentLoc % 0x100);

          try
          {
            //add it to vol
            AddToVol(tmpGameRes, NewIsV3, true, lngCurrentVol, lngCurrentLoc);
          }
          catch (Exception e)
          {
            //note it
            Raise_CompileGameEvent(ECStatus.csResError, ResType, CurResNum, "Unable to add Logic resource to VOL file (" + e.Message + ")");
            //check for cancellation
            if (!agCompGame)
            {
              CompleteCancel();
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

    internal static void AddToVol(AGIResource AddRes, bool Version3, bool NewVOL  = false, sbyte lngVol = -1, int lngLoc = -1)
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
      if (!NewVOL)
      {
        try
        {
        //get vol number and location where there is room for this resource
        FindFreeVOLSpace(AddRes);
        }
        catch (Exception e)
        {
          //pass it along
          lngError = e.HResult;
          strError = e.Message;
          //strErrSrc = Err.Source
          throw new Exception(strError);
        }
        lngLoc = AddRes.Loc;
        lngVol = AddRes.Volume;
      }
      else
      {
        //need to verify valid values are passed
        if (lngLoc < 0 || lngVol < 0)
        {
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
      if (Version3)
      {
        //adjust header size
        Array.Resize(ref ResHeader, 7);
        ResHeader[5] = ResHeader[3];
        ResHeader[6] = ResHeader[4];
        strID = agGameID;
      }
      else
      {
        strID = "";
      }

      //if not adding to a new vol file
      if (!NewVOL)
      {
        //save the resource into the vol file
        //get as file number
        fsVOL = new FileStream(agGameDir + strID + "VOL." + lngVol.ToString(), FileMode.Open);
        bwVOL = new BinaryWriter(fsVOL);
      }
      try
      {
        fsVOL.Seek(lngLoc, SeekOrigin.Begin);
        //add header to vol file
        bwVOL.Write(ResHeader, 0, agIsVersion3 ? 7 : 5);
        // add resource data to vol file
        bwVOL.Write(AddRes.Data.AllData, 0, AddRes.Data.Length);
      }
      catch (Exception e)
      {
        //save error
        strError = e.Message;
        //strErrSrc = Err.Source
        lngError = e.HResult;

        //if not adding to a new file, close volfile
        if (!NewVOL)
        {
          fsVOL.Dispose();
          bwVOL.Dispose();
        }
        throw new Exception("638 :" + strError);
      }
      //always update sizeinvol
      SetSizeInVol(AddRes, AddRes.Size);

      //if adding to a new vol file
      if (NewVOL)
      {
        //increment loc pointer
        lngCurrentLoc = lngCurrentLoc + AddRes.Size + 5;
        //if version3
        if (Version3)
        {
          //add two more
          lngCurrentLoc = lngCurrentLoc + 2;
        }
      }
      else
      {
        try
        {
          //update location in dir file
          UpdateDirFile(AddRes);
        }
        catch (Exception e)
        {
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
      if (VOLLoc[ResVOL, 0] == 0)
      {
        //add it to first position
        VOLLoc[ResVOL, 1] = ResLOC;
        VOLSize[ResVOL, 1] = ResSize;
        //increment Count for this volume
        VOLLoc[ResVOL, 0] = 1;
      }
      else
      {
        //find the place where this vol belongs
        for (i = 1; i <= VOLLoc[ResVOL, 0]; i++)
        { 
          //if it comes BEFORE this position,
          if (ResLOC < VOLLoc[ResVOL, i])
          {
            //i contains position where new volume belongs
            break;
          }
        }

        //first, increment Count
        VOLLoc[ResVOL, 0] = VOLLoc[ResVOL, 0] + 1;

        //now move all items ABOVE i up one space
        for (j = VOLLoc[ResVOL, 0]; j > i; i--)
        {
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
      if (agIsVersion3)
      {
        lngHeader = 7;
        strID = agGameID;
        lngMaxVol = 15;
      }
      else
      {
        lngHeader = 5;
        strID = "";
        lngMaxVol = 4;
      }

      //local copy of restype and resnum (for improved speed)
      NewResType = NewResource.ResType;
      NewResNum = NewResource.Number;
      NewResSize = NewResource.Size + lngHeader;
      //build array of all resources, sorted by VOL order (except the one being loaded)
      foreach (AGILogic tmpRes in agLogs.Col)
      {
        //if not the resource being replaced
        if (NewResType != AGIResType.rtLogic || tmpRes.Number != NewResNum)
        {
          tmpSize = tmpRes.SizeInVOL + lngHeader;
          AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc, lngSize);
        }
      }

      foreach (AGIPicture tmpRes in agPics.Col)
      {
        //if not the resource being replaced
        if (NewResType != AGIResType.rtPicture || tmpRes.Number != NewResNum)
        {
          tmpSize = tmpRes.SizeInVOL + lngHeader;
          AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc, lngSize);
        }
      }

      foreach (AGISound tmpRes in agSnds.Col)
      {
        //if not the resource being replaced
        if (NewResType != AGIResType.rtSound || tmpRes.Number != NewResNum)
        {
          tmpSize = tmpRes.SizeInVOL + lngHeader;
          AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc, lngSize);
        }
      }

      foreach (AGIView tmpRes in agViews.Col)
      {
        //if not the resource being replaced
        if (NewResType != AGIResType.rtView || tmpRes.Number != NewResNum)
        {
          tmpSize = tmpRes.SizeInVOL + lngHeader;
          AddResInfo(tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc, lngSize);
        }
      }

      //step through volumes,
      for (sbyte i = 0; i <= lngMaxVol; i++)
      {
        //start at beginning
        lngStart = 0;
        // check for space between resources
        for (j = 1;  j <= lngLoc[i, 0] + 1; j++)
        {
          //if this is not the end of the list
          if (j < lngLoc[i, 0] + 1)
          {
            lngEnd = lngLoc[i, j];
          }
          else
          {
            lngEnd = MAX_VOLSIZE;
          }
          //calculate space between end of one resource and start of next
          lngFree = lngEnd - lngStart;

          //if enough space is found
          if (lngFree >= NewResSize)
          {
            //it fits here!
            //set volume and location
            NewResource.Volume = i;
            NewResource.Loc = lngStart;
            return;
          }
          // not enough room; 
          // adjust start to end of current resource
          lngStart = lngLoc[i, j] + lngSize[i, j];
        } //Next j
      } //Next i

      //if no room in any VOL file, raise an error
      throw new Exception("593,ResourceFunctions.FindFreeVOLSpace, LoadResString(593)");
    }
    internal static int GetSizeInVOL(sbyte bytVol, int lngLoc)
    {
      //returns the size of this resource in its VOL file
      //inputs are the volume filename and offset to beginning
      //of resource

      //if an error occurs while trying to read the size of this
      //resource, the function returns -1
      byte bytHigh, bytLow;
      int lngV3Offset;
      string strVolFile;

  //any file access errors
  //result in invalid size

      //if version 3
      if (agIsVersion3)
      {
        //adjusts header so compressed size is retrieved
        lngV3Offset = 2;
        //set filename
        strVolFile = agGameDir + agGameID + "VOL." + bytVol;
      }
      else
      {
        lngV3Offset = 0;
        //set filename
        strVolFile = agGameDir + "VOL." + bytVol;
      }

      try
      {
        //open the volume file
        fsVOL = new FileStream(strVolFile, FileMode.Open);
        brVOL = new BinaryReader(fsVOL);
        //verify enough room to get length of resource
        if (fsVOL.Length >= lngLoc + 5)
        {
          //get size low and high bytes
          fsVOL.Seek(lngLoc, SeekOrigin.Begin);
          bytLow = brVOL.ReadByte();
          bytHigh = brVOL.ReadByte();
          //verify this is a proper resource
          if ((bytLow == 0x12) && (bytHigh == 0x34))
          {
            //now get the low and high bytes of the size
            fsVOL.Seek(1, SeekOrigin.Current);
            bytLow = brVOL.ReadByte();
            bytHigh = brVOL.ReadByte();
            fsVOL.Dispose();
            brVOL.Dispose();
            return (int)bytHigh * 256 + bytLow;
          }
        }
      }
      catch (Exception)
      {
        // treat all errors the same
      }
      // if size not found ,
      //ensure file is closed, and return -1
      fsVOL.Dispose();
      brVOL.Dispose();
      return -1;
    }
    private static void SetSizeInVol(AGIResource ThisResource, int NewSizeInVol)
    {
      //sets the size of the resource
      switch (ThisResource.ResType)
      {
        case AGIResType.rtLogic:
          agLogs[ThisResource.Number].SizeInVOL = NewSizeInVol;
          break;
        case AGIResType.rtPicture:
          agPics[ThisResource.Number].SizeInVOL = NewSizeInVol;
          break;
        case AGIResType.rtSound:
          agSnds[ThisResource.Number].SizeInVOL = NewSizeInVol;
          break;
        case AGIResType.rtView:
          agViews[ThisResource.Number].SizeInVOL = NewSizeInVol;
          break;
      }
    }
      private static void Tmp()
    {/*
public void SetSizeInVol(ThisResource As AGIResource, NewSizeInVol As Long)
  }
}

public void UpdateDirFile(UpdateResource As AGIResource, Remove As Boolean = false)
  //this method updates the DIR file with the volume and location
  //of a resource
  //if Remove option passed as true,
  //the resource is treated as //deleted// and
  // 0xFFFFFF is written in the resource//s DIR file place holder

  //NOTE: directories inside a V3 dir file are in this order: LOGIC, PICTURE, VIEW, SOUND
  //the ResType enumeration is in this order: LOGIC, PICTURE, SOUND, VIEW
  //because of the switch between VIEW and SOUND, can//t use a formula to calculate
  //directory offsets


  Dim strDirFile As String, intFile As Integer
  Dim bytDIR() As Byte, intMax As Integer, intOldMax As Integer
  Dim DirByte(0 To 2) As Byte
  Dim lngDirOffset As Long, lngDirEnd As Long
  Dim i As Long, lngStart As Long, lngStop As Long


  On Error goto ErrHandler
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
    DirByte(0) = 0xFF
    DirByte(1) = 0xFF
    DirByte(2) = 0xFF
  } else {
    //calculate directory bytes
    DirByte(0) = UpdateResource.Volume* 0x10 + UpdateResource.Loc \ 0x10000
   DirByte(1) = (UpdateResource.Loc Mod 0x10000) \ 0x100
   DirByte(2) = UpdateResource.Loc Mod 0x100
 }

  //what is current max for this res type?
  switch ( UpdateResource.ResType) {
  case rtLogic
    intMax = agLogs.Max
  case rtPicture
    intMax = agPics.Max
  case rtSound
    intMax = agSnds.Max
  case rtView
    intMax = agViews.Max
  }

  //open the correct dir file, store in a temp array
  intFile = FreeFile()
  //if version3
  if (agIsVersion3) {
    strDirFile = agGameDir + agGameID + "DIR"
  } else {
    strDirFile = agGameDir + ResTypeAbbrv(UpdateResource.ResType) + "DIR"
  }
  Open strDirFile For  As intFile
  ReDim bytDIR(LOF(intFile) - 1) // correct! bytDIR is 0 based!
  Get intFile, 1, bytDIR()
  Close intFile

  //calculate old max and offset (for v3 files)
  if (agIsVersion3) {
    //calculate directory offset
    switch ( UpdateResource.ResType) {
    case rtLogic
      lngDirOffset = 8
      lngDirEnd = bytDIR(3) * 256 + bytDIR(2)
    case rtPicture
      lngDirOffset = bytDIR(3) * 256 + bytDIR(2)
      lngDirEnd = bytDIR(5) * 256 + bytDIR(4)
    case rtView
      lngDirOffset = bytDIR(5) * 256 + bytDIR(4)
      lngDirEnd = bytDIR(7) * 256 + bytDIR(6)
    case rtSound
      lngDirOffset = bytDIR(7) * 256 + bytDIR(6)
      lngDirEnd = UBound(bytDIR()) + 1
    }
    intOldMax = (lngDirEnd - lngDirOffset) / 3 - 1
  } else {
    lngDirEnd = (UBound(bytDIR()) + 1)
    intOldMax = lngDirEnd / 3 - 1
    lngDirOffset = 0
  }

  //if it fits (doesn//t matter if inserting or deleting)
  if ((Remove && intMax >= intOldMax) Or(!Remove && intOldMax >= intMax)) {
    //adjust offset for resnum
    lngDirOffset = lngDirOffset + 3 * UpdateResource.Number

    //just insert the new data, and save the file
    intFile = FreeFile()
    Open strDirFile For  As intFile
    Put intFile, lngDirOffset + 1, DirByte()
    Close intFile
    return;
  }

  //size has changed!
  if (Remove) {
    //must be shrinking
    //////Debug.Assert intOldMax > intMax
    //if v2, just redim the array
    if (!agIsVersion3) {
      ReDim Preserve bytDIR((intMax + 1) * 3 - 1)
    } else {
      //if restype is sound, we also can just truncate the file
      if (UpdateResource.ResType = rtSound) {
        ReDim Preserve bytDIR(UBound(bytDIR) - 3 * (intOldMax - intMax))
      } else {
        //we need to move data from the current directory//s max
        //backwards to compress the directory
        //start with resource just after new max; then move all bytes down
        lngStart = lngDirOffset + intMax* 3 + 3
        lngStop = UBound(bytDIR) - 3 * (intOldMax - intMax)
        for (i = lngStart To lngStop
          bytDIR(i) = bytDIR(i + 3 * (intOldMax - intMax))
        } //Next i
        //now shrink the array
        ReDim Preserve bytDIR(UBound(bytDIR()) - 3 * (intOldMax - intMax))
        //lastly, we need to update affected offsets
        //move snddir offset first
        lngDirOffset = bytDIR(7) * 256 + bytDIR(6)
        lngDirOffset = lngDirOffset - 3 * (intOldMax - intMax)
        bytDIR(7) = lngDirOffset \ 256
        bytDIR(6) = lngDirOffset Mod 256
        //if resource is a view, we are done
        if (UpdateResource.ResType!= rtView) {
          //move view offset
          lngDirOffset = bytDIR(5) * 256 + bytDIR(4)
          lngDirOffset = lngDirOffset - 3 * (intOldMax - intMax)
          bytDIR(5) = lngDirOffset \ 256
          bytDIR(4) = lngDirOffset Mod 256
          //if resource is a pic, we are done
          if (UpdateResource.ResType!= rtPicture) {
            //move picture offset
            lngDirOffset = bytDIR(3) * 256 + bytDIR(2)
            lngDirOffset = lngDirOffset - 3 * (intOldMax - intMax)
            bytDIR(3) = lngDirOffset \ 256
            bytDIR(2) = lngDirOffset Mod 256
          }
        }
      }
    }

    //delete the existing file
    Kill strDirFile
    //now save the file
    intFile = FreeFile()
    Open strDirFile For  As intFile
    Put intFile, 1, bytDIR()
    Close intFile
  } else {
    //must be expanding
    //////Debug.Assert intMax > intOldMax
    //////Debug.Assert UpdateResource.Number = intMax

    ReDim Preserve bytDIR(UBound(bytDIR()) + 3 * (intMax - intOldMax))

    //if v2, add ffs to fill gap up to the last entry
    if (!agIsVersion3) {
      lngStart = lngDirEnd
      lngStop = lngDirEnd + 3 * (intMax - intOldMax - 1) - 1
      for (i = lngStart To lngStop
        bytDIR(i) = 0xFF
      } //Next i
      //add dir data to end
      bytDIR(lngStop + 1) = DirByte(0)
      bytDIR(lngStop + 2) = DirByte(1)
      bytDIR(lngStop + 3) = DirByte(2)
    } else {

      //if expanding the sound dir, just fill it in with FF//s
      if (UpdateResource.ResType = rtSound) {
        lngStart = lngDirEnd
        lngStop = lngDirEnd + 3 * (intMax - intOldMax - 1) - 1
        for (i = lngStart To lngStop
          bytDIR(i) = 0xFF
        } //Next i
        //add dir data to end
        bytDIR(lngStop + 1) = DirByte(0)
        bytDIR(lngStop + 2) = DirByte(1)
        bytDIR(lngStop + 3) = DirByte(2)
      } else {
        //move data to make room for inserted resource
        lngStop = UBound(bytDIR())
        lngStart = lngDirEnd + 3 * (intMax - intOldMax)
        for (i = lngStop To lngStart Step -1
          bytDIR(i) = bytDIR(i - 3 * (intMax - intOldMax))
        } //Next i
        //insert ffs, up to insert location
        lngStop = lngStart - 4
        lngStart = lngStop - 3 * (intMax - intOldMax - 1) + 1
        for (i = lngStart To lngStop
          bytDIR(i) = 0xFF
        } //Next i
        //add dir data to end
        bytDIR(lngStop + 1) = DirByte(0)
        bytDIR(lngStop + 2) = DirByte(1)
        bytDIR(lngStop + 3) = DirByte(2)

        //last thing is to adjust the offsets
        //move snddir offset first
        lngDirOffset = bytDIR(7) * 256 + bytDIR(6)
        lngDirOffset = lngDirOffset + 3 * (intMax - intOldMax)
        bytDIR(7) = lngDirOffset \ 256
        bytDIR(6) = lngDirOffset Mod 256
        //if resource is a view, we are done
        if (UpdateResource.ResType!= rtView) {
          //move view offset
          lngDirOffset = bytDIR(5) * 256 + bytDIR(4)
          lngDirOffset = lngDirOffset + 3 * (intMax - intOldMax)
          bytDIR(5) = lngDirOffset \ 256
          bytDIR(4) = lngDirOffset Mod 256
          //if resource is a pic, we are done
          if (UpdateResource.ResType!= rtPicture) {
            //move picture offset
            lngDirOffset = bytDIR(3) * 256 + bytDIR(2)
            lngDirOffset = lngDirOffset + 3 * (intMax - intOldMax)
            bytDIR(3) = lngDirOffset \ 256
            bytDIR(2) = lngDirOffset Mod 256
          }
        }
      }
    }

    //now save the file
    intFile = FreeFile()
    Open strDirFile For  As intFile
    Put intFile, 1, bytDIR()
    Close intFile
  }


return;

ErrHandler:
  //////Debug.Assert false
  Resume Next
}

public void ValidateVolAndLoc(ResSize As Long)
  //this method ensures current vol has room for resource with given size

  //if not, it closes current vol file, and opens next one

  //this method is only used by the game compiler when doing a
  //complete compile or resource rebuild

  Dim i As Long, lngMaxVol As Long


  On Error Resume Next

  //verify enough room here
  if (lngCurrentLoc + ResSize > MAX_VOLSIZE Or(lngCurrentVol = 0 && (lngCurrentLoc + ResSize > agMaxVol0))) {

    //set maxvol count to 4 or 15, depending on version
    if (agIsVersion3) {
      lngMaxVol = 15
    } else {
      lngMaxVol = 4
    }

    //close current vol
    Close intVolFile

    //first check previous vol files, to see if there is room at end of one of those
    for (i = 0 To lngMaxVol
      //open this file (if the file doesn//t exist, this will create it
      //and it will then be set as the next vol, with pos=0
      intVolFile = FreeFile()
      Open strNewDir + "NEW_VOL." + CStr(i) For  As intVolFile
      //check for error
      if (e.HResult!= 0) {
        //also, compiler should check for this error, as it is fatal
        throw new Exception("640, "VolManager.ValidateVolAndLoc", LoadResString(640)");
        return;
      }

      //is there room at the end of this file?
      if ((i > 0 && LOF(intVolFile) + ResSize <= MAX_VOLSIZE) Or((LOF(intVolFile) + ResSize <= agMaxVol0))) {
        //if so, set pointer to end of the file, and exit
        lngCurrentVol = i
        lngCurrentLoc = LOF(intVolFile)
        return;
      }
      //close the file, and try next
      Close intVolFile
    } //Next i

    //if no volume found, we//ve got a problem...
    //raise error!
    //also, compiler should check for this error, as it is fatal
    throw new Exception("593, "VolManager.ValidateVolAndLoc", LoadResString(593)");
    return;
  }
}
    */
    }
  }
}
