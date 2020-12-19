using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static WinAGI.WinAGI;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;
using static WinAGI.AGIGame;

namespace WinAGI
{
  public static partial class WinAGI
  {
    /*

    Option Explicit

    'constants used in extracting compressed resources
    Const TABLE_SIZE = 18041
    Const START_BITS = 9

    'variables used in extracting compressed resources
    Private lngMaxCode As Long
    Private intPrefix() As Integer
    Private bytAppend() As Byte
    Private lngBitsInBuffer As Long
    Private lngBitBuffer As Long
    Private lngOriginalSize As Long

    'sound subclassing variables, constants, declarations
    Public PrevSndWndProc As Long
    Public blnPlaying As Boolean
    Public PlaySndResNum As Byte
    Public Declare Function CallWindowProc Lib "user32" Alias "CallWindowProcA" (ByVal lpPrevWndFunc As Long, ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long
    Public Declare Function SetWindowLong Lib "user32" Alias "SetWindowLongA" (ByVal hWnd As Long, ByVal nIndex As Long, ByVal dwNewLong As Long) As Long
    Public Declare Function mciGetErrorString Lib "winmm.dll" Alias "mciGetErrorStringA" (ByVal dwError As Long, ByVal lpstrBuffer As String, ByVal uLength As Long) As Long
    Public Declare Function mciSendString Lib "winmm.dll" Alias "mciSendStringA" (ByVal lpstrCommand As String, ByVal lpstrReturnString As String, ByVal uReturnLength As Long, ByVal hwndCallback As Long) As Long
    Public Const GWL_WNDPROC = (-4)
    Public Const MM_MCINOTIFY = &H3B9
    Public Const MCI_NOTIFY_SUCCESSFUL = &H1

    'others
    Private lngPos As Long
    Private mMIDIData() As Byte

    Public Function CompressCelData(Cel As AGICel, blnMirror As Boolean) As Byte()
      'this method compresses cel data
      'into run-length-encoded data that
      'can be written to an AGI View resource

      'blnMirror is used to ensure mirrored cels include enough room
      'for the flipped cel

      Dim bytTempRLE() As Byte
      Dim mHeight As Byte, mWidth As Byte
      Dim mCelData() As AGIColors, mTransColor As Byte
      Dim lngByteCount As Long
      Dim bytChunkColor As AGIColors, bytChunkLen As Byte
      Dim bytNextColor As AGIColors
      Dim bytOut As Byte
      Dim blnFirstChunk As Boolean
      Dim lngMirrorCount As Long
      Dim blnErr As Boolean

      On Error GoTo ErrHandler

      Dim i As Integer, j As Integer

      'copy cel data locally
      mHeight = Cel.Height
      mWidth = Cel.Width
      mTransColor = Cel.TransColor
      mCelData = Cel.AllCelData


      'assume one byte per pixel to start with
      '(include one byte for ending zero)
      ReDim bytTempRLE(CLng(mHeight) * (CLng(mWidth) + 1))
      'step through each row
      For j = 0 To mHeight - 1
        'get first pixel color
        bytChunkColor = mCelData(0, j)
        bytChunkLen = 1
        blnFirstChunk = True

        'step through rest of pixels in this row
        For i = 1 To mWidth - 1
          'get next pixel color
          bytNextColor = mCelData(i, j)
          'if different from current chunk
          If bytNextColor <> bytChunkColor Or bytChunkLen = &HF Then
            'write chunk
            bytTempRLE(lngByteCount) = bytChunkColor * &H10 + bytChunkLen
            'increment Count
            lngByteCount = lngByteCount + 1

            'if this is NOT first chunk or NOT transparent)
            If Not blnFirstChunk Or (bytChunkColor <> mTransColor) Then
              'increment lngMirorCount for any chunks
              'after the first, and also for the first
              'if it is NOT transparent color
              lngMirrorCount = lngMirrorCount + 1
            End If

            blnFirstChunk = False
            'set chunk to new color
            bytChunkColor = bytNextColor
            'reset length
            bytChunkLen = 1
          Else
            'increment chunk length
            bytChunkLen = bytChunkLen + 1
          End If
        Next i
        'if last chunk is NOT transparent
        If bytChunkColor <> mTransColor Then
          'add last chunk
          bytTempRLE(lngByteCount) = bytChunkColor * &H10 + bytChunkLen
          'increment Count
          lngByteCount = lngByteCount + 1
        End If
        'always Count last chunk for mirror
        lngMirrorCount = lngMirrorCount + 1
        'add zero to indicate end of row
        bytTempRLE(lngByteCount) = 0
        lngByteCount = lngByteCount + 1
        lngMirrorCount = lngMirrorCount + 1
      Next j

      'if mirroring
      If blnMirror Then
        'add zeros to make room
        Do Until lngByteCount >= lngMirrorCount
          'add a zero
          bytTempRLE(lngByteCount) = 0
          lngByteCount = lngByteCount + 1
        Loop
      End If

      'reset size of array
      ReDim Preserve bytTempRLE(lngByteCount - 1)

      'return the compressed data
      CompressCelData = bytTempRLE
    Exit Function

    ErrHandler:
      strError = Err.Description
      strErrSrc = Err.Source
      lngError = Err.Number

      'if error was due to insufficient space (subscript out of range)
      'redimension the array and try again
      If Err.Number = 9 Then
        ReDim Preserve bytTempRLE(UBound(bytTempRLE()) + 20)
        Resume
      End If

      '*'Debug.Assert False
      On Error GoTo 0: Err.Raise vbObjectError + 654, strErrSrc, Replace(LoadResString(654), ARG1, CStr(lngError) & ":" & strError)
    End Function




    Public Sub ExpandV3ResData(ByRef bytOriginalData() As Byte, lngExpandedSize As Long)

      Dim intPosIn As Integer, intPosOut As Integer
      Dim intNextCode As Integer, intNewCode As Integer, intOldCode As Integer
      Dim intCount  As Integer, i  As Integer
      Dim strDat As String
      Dim strChar As String * 1
      Dim intCodeSize As Integer
      Dim bytTempData() As Byte

      On Error GoTo ErrHandler

      'initialize variables
      ReDim intPrefix(TABLE_SIZE) 'remember to correct index by 257
      ReDim bytAppend(TABLE_SIZE) 'remember to correct index by 257

      'set temporary data field
      ReDim bytTempData(lngExpandedSize - 1)
      'original size is determined by array bounds
      lngOriginalSize = UBound(bytOriginalData) + 1

      'reset variables used in expansion
      intPosIn = 0
      intPosOut = 0

      lngBitBuffer = 0
      lngBitsInBuffer = 0

      'Set initial Value for code size
      intCodeSize = NewCodeSize(START_BITS)
      'first code is 257
      intNextCode = 257
      'this seems wrong! first code should be 258, right?
      'isn't 257 the 'end' code?

      'Read in the first code.
      intOldCode = InputCode(bytOriginalData, intCodeSize, intPosIn)

      '!!!!why is this set???
      strChar = Chr$(0)

      'first code for  SIERRA resouces should always be 256
      'if first code is NOT 256
      If intOldCode <> 256 Then
        'error!
        GoTo ErrHandler
      End If

      'now begin decompressing actual data
      intNewCode = InputCode(bytOriginalData, intCodeSize, intPosIn)

      'continue extracting data, until all bytes are read (or end code is reached)
      Do While (intPosIn <= lngOriginalSize) And (intNewCode <> &H101)
        'if new code is &H100,
        If (intNewCode = &H100) Then
          'Restart LZW process (should tables be flushed?)
          intNextCode = 258
          intCodeSize = NewCodeSize(START_BITS)
          'Read in the first code.
          intOldCode = InputCode(bytOriginalData, intCodeSize, intPosIn)
          'the character Value is same as code for beginning
          strChar = Chr$(intOldCode)
          'write out the first character
          bytTempData(intPosOut) = intOldCode
          intPosOut = intPosOut + 1
          intCount = intCount + 1
          'now get next code
          intNewCode = InputCode(bytOriginalData, intCodeSize, intPosIn)
        Else
          ' This code checks for the special STRING+character+STRING+character+STRING
          ' case which generates an undefined code.  It handles it by decoding
          ' the last code, and adding a single Charactor to the end of the decode string.
          ' (new_code will ONLY return a next_code Value if the condition exists;
          ' it should otherwise return a known code, or a ascii Value)
          If (intNewCode >= intNextCode) Then
            'decode the string using old code
            strDat = DecodeString(intOldCode)
            'append the character code
            strDat = strDat & strChar
          Else
            'decode the string using new code
            strDat = DecodeString(intNewCode)
          End If
          'retreive the character Value
          strChar = Left$(strDat, 1)
          'now send out decoded data (it's backwards in the string, so
          'start at end and work back to beginning)
          For i = 1 To Len(strDat)
            bytTempData(intPosOut) = Asc(Mid$(strDat, i))
            intPosOut = intPosOut + 1
          Next i
          'if no more room in the current bit-code table,
          If (intNextCode > lngMaxCode) Then
            'get new code size (in number of bits per code)
            intCodeSize = NewCodeSize(intCodeSize + 1)
            intCount = intCount + 1
          End If

          'store code in prefix table
          intPrefix(intNextCode - 257) = intOldCode
          'store append character in table
          bytAppend(intNextCode - 257) = Asc(strChar)
          'increment next code pointer
          intNextCode = intNextCode + 1
          intOldCode = intNewCode
          'clear the decoded data string
          strDat = vbNullString
          'get the next code
          intNewCode = InputCode(bytOriginalData, intCodeSize, intPosIn)
        End If
      Loop

      'copy array
      ReDim bytOriginalData(lngExpandedSize - 1)
      bytOriginalData() = bytTempData()
      'free arrays
      ReDim intPrefix(0)
      ReDim bytAppend(0)
    Exit Sub

    ErrHandler:
      'all errors invalidate resource
      ReDim intPrefix(0)
      ReDim bytAppend(0)
      On Error GoTo 0: Err.Raise 559, Replace(LoadResString(559), ARG1, CStr(Err.Number))
    End Sub
*/

    internal static bool ExtractResources()
    {
      return false;

      //gets the resources from VOL files, and adds them to the game

      //returns true if resources loaded with warnings
      //returns false if one or more errors occur during load

      byte bytResNum;
      AGIResType bytResType;
      string strDirFile;
      byte[] bytBuffer = Array.Empty<byte>();
      bool blnDirtyDIR;
      byte byte1, byte2, byte3;
      int lngDirOffset;  // offset of this resource's directory in Dir file (for v3)
      int lngDirSize, intResCount, i;
      byte bytVol;
      int lngLoc;
      string strVolFile, strID;
      string strResID;
      bool blnWarnings;

      RaiseEvent_LoadStatus(ELStatus.lsResources, AGIResType.rtNone, 0, "");

      //if version 3
      if (AGIGame.agIsVersion3)
      {
        //get ID
        strID = agGameID;
        //get combined dir Volume
        strDirFile = agGameDir + agGameID + "DIR";
        //verify it exists
        if (!File.Exists(strDirFile))
        {
          throw new Exception("524, LoadResString(524), ARG1, strDirFile)");
          return false;
        }

        try
        {
          //open the file, load it into buffer, and close it
          fsDIR = new FileStream(strDirFile, FileMode.Open);
          bytBuffer = new byte[fsDIR.Length];
          fsDIR.Read(bytBuffer);
          fsDIR.Dispose();
        }
        catch (Exception)
        {
          throw new Exception("502LoadResString(502)");
          return false;
        }

        //if not enough bytes to hold at least the 4 dir pointers & 1 resource
        if (bytBuffer.Length < 11) // 11 + bytes
        {
          throw new Exception("542 LoadResString(542), ARG1, strDirFile");
          return false;
        }

      }
      else
      {
        //no id
        strID = "";
      }

      //step through all four resource types
      for (bytResType = 0; bytResType =< AGIResType.rtView; bytResType++)
      {
        //if version 3
        if (agIsVersion3)
        {
          //calculate offset and size of each dir component
          switch (bytResType)
          {
            case AGIResType.rtLogic:
              lngDirOffset = bytBuffer[0] + bytBuffer[1] * 256;
              lngDirSize = bytBuffer[2] + bytBuffer[3] * 256 - lngDirOffset;
              break;
            case AGIResType.rtPicture:
              lngDirOffset = bytBuffer[2] + bytBuffer[3] * 256;
              lngDirSize = bytBuffer[4] + bytBuffer[5] * 256 - lngDirOffset;
              break;
            case AGIResType.rtView:
              lngDirOffset = bytBuffer[4] + bytBuffer[5] * 256;
              lngDirSize = bytBuffer[6] + bytBuffer[7] * 256 - lngDirOffset;
              break;
            case AGIResType.rtSound:
              lngDirOffset = bytBuffer[6] + bytBuffer[7] * 256;
              lngDirSize = bytBuffer.Length - lngDirOffset;
            case default:
              break;
          }
        }
        else
        {
          //no offset for version 2
          lngDirOffset = 0;
          //get name of resource dir file
          strDirFile = agGameDir + ResTypeAbbrv(bytResType) + "DIR";
          //verify it exists
          if (!File.Exists(strDirFile))
          {
            throw new Exception("524, LoadResString(524), ARG1, strDirFile");
          }

          try
          {
            //open the file, load it into buffer, and close it
            fsDIR = new FileStream(strDirFile, FileMode.Open);
            bytBuffer = new byte[fsDIR.Length];
            fsDIR.Read(bytBuffer);
            fsDIR.Dispose();
          }
          catch (Exception)
          {
            throw new Exception("502LoadResString(502)");
          }

          //get size
          lngDirSize = bytBuffer.Length;
        }

        //if invalid dir information, return false
        if ((lngDirOffset < 0) || (lngDirSize < 0))
        {
          throw new Exception(" 542, LoadResString(542), ARG1, strDirFile");
          return false;
        }

        //if at least one resource,
        if (lngDirSize >= 3)
        {
          if (lngDirOffset + lngDirSize > bytBuffer.Length)
          {
            throw new Exception(" 542, LoadResString(542), ARG1, strDirFile");
          }
        }

        //max size of useable directory is 768 (256*3)
        if (lngDirSize > 768)
        {
          //warning- file might be invalid
          blnWarnings = true;
          if (agIsVersion3)
          {
            RecordLogEvent(LogEventTypeleWarning, ResTypeAbbrv(bytResType) + " portion of DIR file is larger than expected; it may be corrupted");
          }
          else
          {
            RecordLogEvent(LogEventType.leWarning, ResTypeAbbrv(bytResType) + "DIR file is larger than expected; it may be corrupted"
          }
          //assume the max for now
          intResCount = 256;
        }
        else
        {
          intResCount = lngDirSize / 3;
        }

        //if this resource type has entries,
        if (intResCount > 0)
        {
          //use error handler to check for bad resources
          for (i = 0; i < intResCount; i++)
          {

            RaiseEvent_LoadStatus(ELStatus.lsResources, bytResType, i, "");

            bytResNum = i;
            //get location data for this resource
            byte1 = bytBuffer[lngDirOffset + bytResNum * 3];
            byte2 = bytBuffer[lngDirOffset + bytResNum * 3 + 1];
            byte3 = bytBuffer[lngDirOffset + bytResNum * 3 + 2];
            //ignore any 0xFFFFFF sequences,
            if (byte1 != 0xff)
            {
              //extract volume and location
              bytVol = (byte)(byte1 >> 4);
              strVolFile = agGameDir + strID + "VOL." + bytVol.ToString();
              lngLoc = ((byte1 % 16) << 16) + (byte2 << 8) + byte3;
              strResID = agResTypeName(bytResType) + bytResNum.ToString();
              //add a resource of this res type
              switch (bytResType)
              {
                case AGIResType.rtLogic:  //logic
                  agLogs.LoadLogic(bytResNum, bytVol, lngLoc);
                  //make sure it was added before attempting to set property state
                  if (agLogs.Exists(bytResNum))
                  {
                    //when new resources are added, status is set to dirty; for initial load,
                    //need to reset them to false
                    agLogs[bytResNum].WritePropState = false;
                    agLogs[bytResNum].IsDirty = false;
                  }
                  else
                  {
                    //set it//s DIR file values to FFs
                    bytBuffer[lngDirOffset + bytResNum * 3] = 0xFF;
                    bytBuffer[lngDirOffset + bytResNum * 3 + 1] = 0xFF;
                    bytBuffer[lngDirOffset + bytResNum * 3 + 2] = 0xFF;
                    blnDirtyDIR = true;
                  }
                  break;
                case AGIResType.rtPicture:  //picture
                  agPics.LoadPicture(bytResNum, bytVol, lngLoc);
                  //make sure it was added before attempting to set property state
                  if (agPics.Exists(bytResNum))
                  {
                    //when new resources are added, status is set to dirty; for initial load,
                    //need to reset them to false
                    agPics[bytResNum].WritePropState = false;
                    agPics[bytResNum].IsDirty = false;
                    break;
                  }
                  else
                  {
                    //set it//s DIR file values to FFs
                    bytBuffer[lngDirOffset + bytResNum * 3] = 0xFF;
                    bytBuffer[lngDirOffset + bytResNum * 3 + 1] = 0xFF;
                    bytBuffer[lngDirOffset + bytResNum * 3 + 2] = 0xFF;
                    blnDirtyDIR = true;
                  }

                case AGIResType.rtSound:  //sound
                  agSnds.LoadSound(bytResNum, bytVol, lngLoc);
                  //make sure it was added before attempting to set property state
                  if (agSnds.Exists(bytResNum))
                  {
                    //when new resources are added, status is set to dirty; for initial load,
                    //need to reset them to false
                    agSnds[bytResNum].WritePropState = false;
                    agSnds[bytResNum].IsDirty = false;
                  }
                  else
                  {
                    //set it//s DIR file values to FFs
                    bytBuffer[lngDirOffset + bytResNum * 3] = 0xFF;
                    bytBuffer[lngDirOffset + bytResNum * 3 + 1] = 0xFF;
                    bytBuffer[lngDirOffset + bytResNum * 3 + 2] = 0xFF;
                    blnDirtyDIR = true;
                  }
                  break;

                case AGIResType.rtView:  //view
                  agViews.LoadView(bytResNum, bytVol, lngLoc);
                //make sure it was added before attempting to set property state
                if (agViews.Exists(bytResNum))
                  {
                    //when new resources are added, status is set to dirty; for initial load,
                    //need to reset them to false
                    agViews(bytResNum).WritePropState = false
                  agViews(bytResNum).IsDirty = false
                }
                  else
                  {
                    //set it//s DIR file values to FFs
                    bytBuffer[lngDirOffset + bytResNum * 3] = 0xFF;
                              bytBuffer[lngDirOffset + bytResNum * 3 + 1] = 0xFF;
                              bytBuffer[lngDirOffset + bytResNum * 3 + 2] = 0xFF;
                    blnDirtyDIR = true;
                            }
                case default: break;
              }
            }
          }

          //if a v2 DIR was modified, save it
          if (!agIsVersion3 && blnDirtyDIR)
          {
            On Error Resume Next
            //save the new DIR file
            Kill strDirFile +".OLD"
            Err.Clear
            Name strDirFile As strDirFile + ".OLD"
            Err.Clear
            intFile = FreeFile()
            Open strDirFile For Binary As intFile
            Put intFile, 1, bytBuffer()
            Close intFile

            //reset the dirty flag
  blnDirtyDIR = False
            On Error GoTo ErrHandler
}
        }
      }

      //if a V3 DIR file was modified, save it
      if (agIsVersion3 && blnDirtyDIR)
      {
        On Error Resume Next
        //save the new DIR file
        Kill strDirFile +".OLD"
        Err.Clear
        Name strDirFile As strDirFile + ".OLD"
        Err.Clear
        intFile = FreeFile()
        Open strDirFile For Binary As intFile
        Put intFile, 1, bytBuffer()
        Close intFile

        //reset the dirty flag
  blnDirtyDIR = False
        On Error GoTo ErrHandler
      }

      //return any warning codes
      return blnWarnings

    //ErrHandler:

      //if error was invalid resource data, invalid LOC value, or missing VOL file
      switch (Err.Number - vbObjectError)
      {
        case 502: //Error %1 occurred while trying to access %2.
          strError = Err.Description
        Err.Clear
        blnWarnings = True
        RecordLogEvent leWarning, strResID +" was skipped due to file access error (" + strError + ")"
        strError = ""
        Resume Next

      case 505: //Invalid resource location (%1) in %2.
          Err.Clear
          blnWarnings = True
        RecordLogEvent leWarning, strResID +" was skipped because it//s location (" + CStr(lngLoc) + ") in the VOL file(" + JustFileName(strVolFile) + ") is invalid."
        Resume Next

      case 506: //Invalid resource data at %1 in %2.
          Err.Clear
          blnWarnings = True
        RecordLogEvent leWarning, strResID +" was skipped because it does not have a valid resource header"
        Resume Next

      case 507: //Error %1 while reading resource in %2.
          strError = Err.Description
        Err.Clear
        blnWarnings = True
        RecordLogEvent leWarning, strResID +" was skipped due to resource data error (" + strError + ")"
        strError = ""
        Resume Next

      case 511: //Resource already loaded
                //this should never be possible;
                //included only for completeness
          Err.Clear
          blnWarnings = True
        RecordLogEvent leWarning, strResID +" was skipped because it has already been loaded"
        Resume Next

      case 606: //Can//t load resource: file not found (%1)
          Err.Clear
          blnWarnings = True
        RecordLogEvent leWarning, strResID +" was skipped because it//s VOL file (" + JustFileName(strVolFile) + ") is missing."
        Resume Next

      case 646:
        case 648:
        case 650:
        case 652: //unhandled error in LoadLog|Pic|Snd|View
          strError = Err.Description
        Err.Clear
        blnWarnings = True
        RecordLogEvent leWarning, strResID +" was skipped due to an error while loading (" + strError + ")"
        strError = ""
        Resume Next

      case default: break;
      }

      //otherwise, stop loading resources
      strError = Err.Description
      strErrSrc = Err.Source
      lngError = Err.Number

      On Error GoTo 0: Err.Raise vbObjectError +545, strErrSrc, Replace(LoadResString(545), ARG1, CStr(lngError) + ":" + strError)

    }

    /*
        Public Function BuildMIDI(SoundIn As AGISound) As Byte()

          Dim lngWriteTrack As Long
          Dim i As Long, j As Long
          Dim bytNote As Byte, intFreq As Integer, bytVol As Byte
          Dim lngTrackCount As Long, lngTickCount As Long
          Dim lngStart As Long, lngEnd As Long

          On Error Resume Next

          '*'Debug.Assert SoundIn.Resource.Loaded

          'calculate track Count
          If Not SoundIn.Track(0).Muted And SoundIn.Track(0).Notes.Count > 0 Then
            lngTrackCount = 1
          End If
          If Not SoundIn.Track(1).Muted And SoundIn.Track(1).Notes.Count > 0 Then
            lngTrackCount = lngTrackCount + 1
          End If
          If Not SoundIn.Track(2).Muted And SoundIn.Track(2).Notes.Count > 0 Then
            lngTrackCount = lngTrackCount + 1
          End If
          If Not SoundIn.Track(3).Muted And SoundIn.Track(3).Notes.Count > 0 Then
            'add two tracks if noise is not muted
            'because the white noise and periodic noise
            'are written as two separate tracks
            lngTrackCount = lngTrackCount + 2
          End If

          'set intial size of midi data array
          ReDim mMIDIData(70 + 256)

          ' write header
          mMIDIData(0) = 77 '"M"
          mMIDIData(1) = 84 '"T"
          mMIDIData(2) = 104 '"h"
          mMIDIData(3) = 100 '"d"
          lngPos = 4


          WriteSndLong 6& 'remaining length of header (3 integers = 6 bytes)
          'write mode, trackcount and ppqn as bigendian integers
          WriteSndWord 1   'mode 0 = single track
                           'mode 1 = multiple tracks, all start at zero
                           'mode 2 = multiple tracks, independent start times
          'if no tracks,
          If lngTrackCount = 0 Then
            'need to build a 'null' set of data!!!

            ReDim Preserve mMIDIData(37)

            'one track, with one note of smallest length, and no sound
            WriteSndWord 1  'one track
            WriteSndWord 30 'pulses per quarter note
            'add the track info
            WriteSndByte 77 '"M"
            WriteSndByte 84 '"T"
            WriteSndByte 114 '"r"
            WriteSndByte 107 '"k"
            WriteSndLong 15 'track length
            'write the track number
            WriteSndDelta CLng(0)
            'write the set instrument status byte
            WriteSndByte &HC0
            'write the instrument number
            WriteSndByte 0
            'write a slight delay note with no volume to end
            WriteSndDelta 0
            WriteSndByte &H90
            WriteSndByte 60
            WriteSndByte 0
            WriteSndDelta 16
            WriteSndByte &H80
            WriteSndByte 60
            WriteSndByte 0
            'add end of track info
            WriteSndDelta 0
            WriteSndByte &HFF
            WriteSndByte &H2F
            WriteSndByte &H0
            'return
            BuildMIDI = mMIDIData()
            Exit Function
          End If

          'add track count
          WriteSndWord lngTrackCount


          'write pulses per quarter note
          '(agi sound tick is 1/60 sec; each tick of an AGI note
          'is 1/60 of a second; by default, MIDI defines a whole
          'note as 2 seconds; therefore, a quarter note is 1/2
          'second, or 30 ticks
          WriteSndWord 30

          'write the sound tracks
          For i = 0 To 2
            'if adding this instrument,
            If Not SoundIn.Track(i).Muted And SoundIn.Track(i).Notes.Count > 0 Then
              WriteSndByte 77 '"M"
              WriteSndByte 84 '"T"
              WriteSndByte 114 '"r"
              WriteSndByte 107 '"k"

              'store starting position for this track's data
              lngStart = lngPos
              'place holder for data size
              WriteSndLong 0

              'write the track number
              '*********** i think this should be zero for all tracks
              'it's the delta sound value for the instrument setting
              WriteSndDelta 0 'CLng(lngWriteTrack)

              'write the set instrument status byte
              WriteSndByte &HC0 + lngWriteTrack
              'write the instrument number
              WriteSndByte SoundIn.Track(i).Instrument

              'write a slight delay note with no volume to start
              WriteSndDelta 0
              WriteSndByte &H90 + lngWriteTrack
              WriteSndByte 60
              WriteSndByte 0
              WriteSndDelta 4 '16
              WriteSndByte &H80 + lngWriteTrack
              WriteSndByte 60
              WriteSndByte 0

              'step through notes in this track (5 bytes at a time)
              For j = 0 To SoundIn.Track(i).Notes.Count - 1
                'calculate note to play
                If SoundIn.Track(i).Notes(j).FreqDivisor > 0 Then
                  'middle C is 261.6 HZ; from midi specs,
                  'middle C is a note with a Value of 60
                  'this requires a shift in freq of approx. 36.376
                  'however, this offset results in crappy sounding music;
                  'empirically, 36.5 seems to work best
                  bytNote = CLng((Log10(111860# / CDbl(SoundIn.Track(i).Notes(j).FreqDivisor)) / LOG10_1_12) - 36.5)

                  '
                  'f = 111860 / (((Byte2 & 0x3F) << 4) + (Byte3 & 0x0F))
                  '
                  '(bytNote can never be <44 or >164)
                  'in case note is too high,
                  If bytNote > 127 Then
                    bytNote = 127
                  End If
                  bytVol = 127 * (15 - SoundIn.Track(i).Notes(j).Attenuation) / 15
                Else
                  bytNote = 0
                  bytVol = 0
                End If

                'write NOTE ON data
                WriteSndDelta 0
                WriteSndByte &H90 + lngWriteTrack
                WriteSndByte bytNote
                WriteSndByte bytVol
                'write NOTE OFF data
                WriteSndDelta CLng(SoundIn.Track(i).Notes(j).Duration)
                WriteSndByte &H80 + lngWriteTrack
                WriteSndByte bytNote
                WriteSndByte 0
              Next j

              'write a slight delay note with no volume to end
              WriteSndDelta 0
              WriteSndByte &H90 + lngWriteTrack
              WriteSndByte 60
              WriteSndByte 0
              WriteSndDelta 4 '16
              WriteSndByte &H80 + lngWriteTrack
              WriteSndByte 60
              WriteSndByte 0

              'add end of track info
              WriteSndDelta 0
              WriteSndByte &HFF
              WriteSndByte &H2F
              WriteSndByte &H0
              'save track end position
              lngEnd = lngPos
              'set cursor to start of track
              lngPos = lngStart
              'write the track length
              WriteSndLong (lngEnd - lngStart) - 4
              lngPos = lngEnd
            End If
            'increment track counter
            lngWriteTrack = lngWriteTrack + 1
          Next i

        'seashore does a good job of imitating the white noise, with frequency adjusted empirically
        'harpsichord does a good job of imitating the tone noise, with frequency adjusted empirically

          'if adding noise track,
          If Not SoundIn.Track(3).Muted And SoundIn.Track(3).Notes.Count > 0 Then
            'because there are two types of noise, must use two channels
            'one uses seashore (white noise)
            'other uses harpsichord (tone noise)

            For i = 0 To 1
              '0 means add tone
              '1 means add white noise

              WriteSndByte 77 '"M"
              WriteSndByte 84 '"T"
              WriteSndByte 114 '"r"
              WriteSndByte 107 '"k"

              'store starting position for this track's data
              lngStart = lngPos
              WriteSndLong 0       'place holder for chunklength

              'write track number
              WriteSndDelta CLng(lngWriteTrack)
              'write the set instrument byte
              WriteSndByte &HC0 + lngWriteTrack
              'write the instrument number
              Select Case i
              Case 0  'tone
                WriteSndByte 6 'harpsichord seems to be good simulation for tone
              Case 1  'white noise
                WriteSndByte 122 'seashore seems to be good simulation for white noise
                'crank up the volume
                WriteSndByte 0
                WriteSndByte &HB0 + lngWriteTrack
                WriteSndByte 7
                WriteSndByte 127
                'set legato
                WriteSndByte 0
                WriteSndByte &HB0 + lngWriteTrack
                WriteSndByte 68
                WriteSndByte 127
              End Select

              'write a slight delay note with no volume to start
              WriteSndDelta 0
              WriteSndByte &H90 + lngWriteTrack
              WriteSndByte 60
              WriteSndByte 0
              WriteSndDelta 4 '16
              WriteSndByte &H80 + lngWriteTrack
              WriteSndByte 60
              WriteSndByte 0

              'reset tick counter (used in case of need to borrow track 3 freq)
              lngTickCount = 0

              '0 - add periodic
              '1 - add white noise
              For j = 0 To SoundIn.Track(3).Notes.Count - 1
                'add duration to tickcount
                lngTickCount = lngTickCount + SoundIn.Track(3).Notes(j).Duration
                'Fourth byte: noise freq and type
                '    In the case of the noise voice,
                '    7  6  5  4  3  2  1  0
                '
                '    1  .  .  .  .  .  .  .      Always 1.
                '    .  1  1  0  .  .  .  .      Register number in T1 chip (6)
                '    .  .  .  .  X  .  .  .      Unused, ignored; can be set to 0 or 1
                '    .  .  .  .  .  FB .  .      1 for white noise, 0 for periodic
                '    .  .  .  .  .  . NF0 NF1    2 noise frequency control bits
                '
                '    NF0  NF1       Noise Frequency
                '
                '     0    0         1,193,180 / 512 = 2330
                '     0    1         1,193,180 / 1024 = 1165
                '     1    0         1,193,180 / 2048 = 583
                '     1    1         Borrow freq from channel 3
                '
                'AGINote contains bits 2-1-0 only
                '
                'if this note matches desired type
                If (SoundIn.Track(3).Notes(j).FreqDivisor And 4) = 4 * i Then
                  'if using borrow function:
                  If (SoundIn.Track(3).Notes(j).FreqDivisor And 3) = 3 Then
                    'get frequency from channel 3
                    intFreq = GetTrack3Freq(SoundIn.Track(2), lngTickCount)
                  Else
                    'get frequency from bits 0 and 1
                    intFreq = CInt(2330.4296875 / 2 ^ (SoundIn.Track(3).Notes(j).FreqDivisor And 3))
                  End If

                  'convert to midi note
                  If (SoundIn.Track(3).Notes(j).FreqDivisor And 4) = 4 Then
                    'for white noise, 96 is my best guess to imitate noise
                    'BUT... 96 causes some notes to come out negative;
                    '80 is max Value that ensures all AGI freq values convert
                    'to positive MIDI note values
                    bytNote = CByte((Log10(intFreq) / LOG10_1_12) - 80)
                  Else
                    'for periodic noise, 64 is my best guess to imitate noise
                    bytNote = CByte((Log10(intFreq) / LOG10_1_12) - 64)
                  End If
                  'get volume
                  bytVol = 127 * (15 - SoundIn.Track(3).Notes(j).Attenuation) / 15
                Else
                  'write a blank
                  bytNote = 0
                  bytVol = 0
                End If

                'write NOTE ON data
                'no delta time
                WriteSndDelta 0
                'note on this track
                WriteSndByte &H90 + lngWriteTrack
                WriteSndByte CByte(bytNote)
                WriteSndByte CByte(bytVol)

                'write NOTE OFF data
                WriteSndDelta CLng(SoundIn.Track(3).Notes(j).Duration)
                WriteSndByte &H80 + lngWriteTrack
                WriteSndByte CByte(bytNote)
                WriteSndByte 0
              Next j

              'write a slight delay note with no volume to end
              WriteSndDelta 0
              WriteSndByte &H90 + lngWriteTrack
              WriteSndByte 60
              WriteSndByte 0
              WriteSndDelta 4 '16
              WriteSndByte &H80 + lngWriteTrack
              WriteSndByte 60
              WriteSndByte 0

              'write end of track data
              WriteSndDelta 0
              WriteSndByte &HFF
              WriteSndByte &H2F
              WriteSndByte &H0
              'save ending position
              lngEnd = lngPos
              'go back to start of track, and write track length
              lngPos = lngStart
              WriteSndLong (lngEnd - lngStart) - 4
              lngPos = lngEnd
              'increment track counter
              lngWriteTrack = lngWriteTrack + 1
            Next i
          End If

          'remove any extra padding from the data array (-1?)
        '''  ReDim Preserve mMIDIData(lngPos)
          ReDim Preserve mMIDIData(lngPos - 1)
          'return
          BuildMIDI = mMIDIData()
          'clear any errors
          Err.Clear
        End Function

        Public Function BuildIIgsMIDI(SoundIn As AGISound, ByRef Length As Single) As Byte()

          Dim i As Long, midiIn() As Byte, lngInPos As Long
          Dim midiOut() As Byte, lngOutPos As Long
          Dim lngTicks As Long
          Dim bytIn As Byte, lngTime As Long, bytCmd As Byte, bytChannel As Byte


          On Error GoTo ErrHandler

          'it also counts max ticks, returning that so sound length
          'can be calculated

          'length value gets calculated by counting total number of ticks
          'assumption is 60 ticks per second; nothing to indicate that is
          'not correct; all sounds 'sound' right, so we go with it

          'builds a midi chunk based on apple IIgs sound format -
          'the raw midi data embedded in the sound seems to play OK
          'when using 'high-end' players (WinAmp as an example) but not
          'Windows Media Player, or the midi API functions; even in
          'WinAmp, the total sound length (ticks and seconds) doesn't
          'calculate correctly, even though it plays

          'this seems to be due to the prescence of the &HFC commands
          'it looks like every sound resource has them; sometimes
          'one &HFC ends the file; othertimes there are a series of
          'them that are followed by a set of &HDx and &HBx commands
          'that appear to reset all 16 channels

          'eliminating the &HFC command and everything that follows
          'plays the sound correctly (I think)

          'a common 'null' file is one with just four &HFC codes, and
          'nothing else

          'local copy of data -easier to manipulate
          midiIn() = SoundIn.Resource.AllData

          'start with size of input data, assuming it all gets used,
          'plus space for headers and track end command
          '   need 22 bytes for header
          '   size of sound data, minus the two byte header
          '   need to also add 'end of track' event, which takes up 4 bytes
          '   (adjust total by -1 because of arrays are 0-based)
          ReDim midiOut(23 + UBound(midiIn()))

          ' write header
          midiOut(0) = 77 '"M"
          midiOut(1) = 84 '"T"
          midiOut(2) = 104 '"h"
          midiOut(3) = 100 '"d"
          'size of header, as a long
          midiOut(4) = 0
          midiOut(5) = 0
          midiOut(6) = 0
          midiOut(7) = 6
          'mode, as an integer
          midiOut(8) = 0
          midiOut(9) = 1
          'track count as integer
          midiOut(10) = 0
          midiOut(11) = 1
          'write pulses per quarter note as integer
          midiOut(12) = 0
          midiOut(13) = 30
          'track header
          midiOut(14) = 77  '"M"
          midiOut(15) = 84  '"T"
          midiOut(16) = 114  '"r"
          midiOut(17) = 107  '"k"
          'size of track data (placeholder)
          midiOut(18) = 0
          midiOut(19) = 0
          midiOut(20) = 0
          midiOut(21) = 0

          'null sounds will start with &HFC in first four bytes
          ' (and nothing else), so ANY file starting with &HFC
          ' is considered empty
          If midiIn(2) = &HFC Then
            'assume no sound
            ReDim Preserve midiOut(25)
            midiOut(21) = 4
            'add end of track data
            midiOut(22) = 0
            midiOut(23) = &HFF
            midiOut(24) = &H2F
            midiOut(25) = 0
            Length = 0
            BuildIIgsMIDI = midiOut()
            Exit Function
          End If

          'starting output pos
          lngOutPos = 22

          'move the data over, one byte at a time
          lngInPos = 2
          Do
            'get next byte of input data
            bytIn = midiIn(lngInPos)
            'add it to output
            midiOut(lngOutPos) = bytIn
            lngOutPos = lngOutPos + 1

            'time is always first input; it is supposed to be a delta value
            'but it appears that agi used it as an absolute value; so if time
            'is greater than &H7F, it will cause a hiccup in modern midi
            'players

            lngTime = bytIn
            If bytIn And &H80 Then
              'treat &HFC as an end mark, even if found in time position
              '&HF8 also appears to cause an end
              If bytIn = &HFC Or bytIn = &HF8 Then
               'backup one to cancel this byte
               lngOutPos = lngOutPos - 1
                Exit Do
              End If

              'convert into two-byte time value
              ReDim Preserve midiOut(UBound(midiOut()) + 1)
              midiOut(lngOutPos - 1) = 129
              midiOut(lngOutPos) = bytIn And &H7F
              lngOutPos = lngOutPos + 1

              'ignore 'normal' delta time calculations
        '''      lngTime = (lngTime And &H7F)
        '''      Do
        '''        lngTime = lngTime * 128
        '''        lngInPos = lngInPos + 1
        '''        'err check
        '''        If lngInPos > UBound(midiIn()) Then Exit Do
        '''        bytIn = midiIn(lngInPos)
        '''
        '''        'add it to output
        '''        midiOut(lngOutPos) = bytIn
        '''        lngOutPos = lngOutPos + 1
        '''
        '''        lngTime = lngTime + (bytIn And &H7F)
        '''      Loop While (bytIn And &H80) = &H80
            End If

            lngInPos = lngInPos + 1
            'err check
            If lngInPos > UBound(midiIn()) Then Exit Do
            bytIn = midiIn(lngInPos)

            'next byte is a controller (>=&H80) OR a running status (<&H80)
            If bytIn >= &H80 Then
              'it's a command
              bytCmd = bytIn \ 16
              bytChannel = bytIn And &HF
              'commands:
              '    &H8 = note off
              '    &H9 = note on
              '    &HA = polyphonic key pressure
              '    &HB = control change (volume, pan, etc)
              '    &HC = set patch (instrument)
              '    &HD = channel pressure
              '    &HE = pitch wheel change
              '    &HF = system command
              '
              ' all agi sounds appear to start with &HC commands, then
              ' optionally &HB commands, followed by &H8/&H9s; VERY
              ' rarely &HD command will show up
              '
              ' &HFC command seems to be the terminating code for agi
              ' sounds; so if encountered, immediately stop processing
              ' sometimes extra &HD and &HB commands follow a &HFC,
              ' but they cause hiccups in modern midi programs
              If bytIn = &HFC Then
                'back up so last time value gets overwritten
                lngOutPos = lngOutPos - 1
                Exit Do
              End If

              'assume any other &HF command is OK;
              'add it to output
              midiOut(lngOutPos) = bytIn
              lngOutPos = lngOutPos + 1

            Else
              'it's a running status -
              'back up one so next byte is event data
              lngInPos = lngInPos - 1
            End If

            'increment tick count
            lngTicks = lngTicks + lngTime

            'next comes event data; number of data points depends on command
            Select Case bytCmd
            Case 8, 9, &HA, &HB, &HE
              'these all take two bytes of data
              'get next byte
              lngInPos = lngInPos + 1
              'err check
              If lngInPos > UBound(midiIn()) Then Exit Do
              bytIn = midiIn(lngInPos)

              'add it to output
              midiOut(lngOutPos) = bytIn
              lngOutPos = lngOutPos + 1

              'get next byte
              lngInPos = lngInPos + 1
              'err check
              If lngInPos > UBound(midiIn()) Then Exit Do
              bytIn = midiIn(lngInPos)

              'add it to output
              midiOut(lngOutPos) = bytIn
              lngOutPos = lngOutPos + 1

            Case &HC, &HD
              'only one byte for program change, channel pressure
              'get next byte
              lngInPos = lngInPos + 1
              'err check
              If lngInPos > UBound(midiIn()) Then Exit Do
              bytIn = midiIn(lngInPos)

              'add it to output
              midiOut(lngOutPos) = bytIn
              lngOutPos = lngOutPos + 1

            Case &HF 'system messages
              'depends on submsg (channel value) - only expected value is &HC
              Select Case bytChannel
              Case 0
                '*'Debug.Assert False
                'variable; go until &HF7 found
                Do
                  'get next byte
                  lngInPos = lngInPos + 1
                  'err check
                  If lngInPos > UBound(midiIn()) Then Exit Do
                  bytIn = midiIn(lngInPos)

                  'add it to output
                  midiOut(lngOutPos) = bytIn
                  lngOutPos = lngOutPos + 1
                Loop Until bytIn = &HF7

              Case 1, 4, 5, 9, &HD
                'all undefined- indicates an error
                'back up so last time value gets overwritten
                lngOutPos = lngOutPos - 1
                Exit Do

              Case 2 'song position
                'this uses two bytes
                'get next byte
                lngInPos = lngInPos + 1
                'err check
                If lngInPos > UBound(midiIn()) Then Exit Do
                bytIn = midiIn(lngInPos)

                'add it to output
                midiOut(lngOutPos) = bytIn
                lngOutPos = lngOutPos + 1

                'get next byte
                lngInPos = lngInPos + 1
                'err check
                If lngInPos > UBound(midiIn()) Then Exit Do
                bytIn = midiIn(lngInPos)

                'add it to output
                midiOut(lngOutPos) = bytIn
                lngOutPos = lngOutPos + 1

              Case 3 'song select
                'this uses one byte
                'get next byte
                lngInPos = lngInPos + 1
                'err check
                If lngInPos > UBound(midiIn()) Then Exit Do
                bytIn = midiIn(lngInPos)

                'add it to output
                midiOut(lngOutPos) = bytIn
                lngOutPos = lngOutPos + 1

              Case 6, 7, 8, &HA, &HB, &HE, &HF
                'these all have no bytes of data
                ' but only &HFC is expected; it gets
                ' checked above, though, so it doesn't
                ' get checked here
              End Select
            End Select

            'move to next byte (which should be another time value)
            lngInPos = lngInPos + 1
          Loop Until lngInPos > UBound(midiIn())

          'resize output array to remove correct size
          If lngOutPos + 3 <> UBound(midiOut()) Then
            ReDim Preserve midiOut(lngOutPos + 3)
          End If

          'add end of track data
          midiOut(lngOutPos) = 0
          midiOut(lngOutPos + 1) = &HFF
          midiOut(lngOutPos + 2) = &H2F
          midiOut(lngOutPos + 3) = 0
          lngOutPos = lngOutPos + 4

          'update size of track data (total length - 22)
          midiOut(18) = (lngOutPos - 22) \ &H1000000 And &HFF
          midiOut(19) = (lngOutPos - 22) \ &H10000 And &HFF
          midiOut(20) = (lngOutPos - 22) \ &H100& And &HFF
          midiOut(21) = (lngOutPos - 22) And &HFF

          '(convert ticks seconds)
          Length = lngTicks / 60

          BuildIIgsMIDI = midiOut()
        Exit Function

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Function

        Public Function BuildIIgsPCM(SoundIn As AGISound) As Byte()

          Dim i As Long, bData() As Byte
          Dim lngSize As Long

          On Error GoTo ErrHandler


          'builds a wav file stream from an apple IIgs PCM sound resource

        'Positions Sample Value  Description
        '0 - 3 = "RIFF"  Marks the file as a riff file.
        '4 - 7 = var File size (integer) Size of the overall file
        '8 -11 = "WAVE"  File Type Header. For our purposes, it always equals "WAVE".
        '12-15 = "fmt "  Format chunk marker. Includes trailing space
        '16-19 = 16  Length of format data as listed above
        '20-21 = 1 Type of format (1 is PCM) - 2 byte integer
        '22-23 = 1 Number of Channels - 2 byte integer
        '24-27 = 8000 Sample Rate - 32 byte integer.
        '28-31 = 8000  (Sample Rate * BitsPerSample * Channels) / 8.
        '32-33 = 1 (BitsPerSample * Channels) / 8. 1 - 8 bit mono; 2 - 8 bit stereo/16 bit mono; 4 - 16 bit stereo
        '34-35 = 8  Bits per sample
        '36-39 "data"  "data" chunk header. Marks the beginning of the data section.
        '40-43 = var  Size of the data section.
        '44+    data

          'local copy of data -easier to manipulate
          bData() = SoundIn.Resource.AllData

          'header is 54 bytes for pcm, but its purpose is still mostly
          'unknown; the total size is at pos 8-9; the rest of the
          'header appears identical across resources, with exception of
          'position 2- it seems to vary from low thirties to upper 60s,
          'maybe it's a volume thing?

          'all resources appear to end with a byte value of 0; not sure
          'if it's necessary for wav files, but we keep it anyway

          'size of sound data is total file size, minus header (add one
          'to account for zero based arrays)
          lngSize = UBound(bData()) + 1 - 54

          'expand midi data array to hold the sound resource data plus
          'the WAV file header
          ReDim mMIDIData(43 + lngSize)

          mMIDIData(0) = 82
          mMIDIData(1) = 73
          mMIDIData(2) = 70
          mMIDIData(3) = 70
          mMIDIData(4) = (lngSize + 36) And &HFF
          mMIDIData(5) = (lngSize + 36) \ &H100& And &HFF
          mMIDIData(6) = (lngSize + 36) \ &H10000 And &HFF
          mMIDIData(7) = (lngSize + 36) \ &H1000000 And &HFF
          mMIDIData(8) = 87
          mMIDIData(9) = 65
          mMIDIData(10) = 86
          mMIDIData(11) = 69
          mMIDIData(12) = 102
          mMIDIData(13) = 109
          mMIDIData(14) = 116
          mMIDIData(15) = 32
          mMIDIData(16) = 16
          mMIDIData(17) = 0
          mMIDIData(18) = 0
          mMIDIData(19) = 0
          mMIDIData(20) = 1
          mMIDIData(21) = 0
          mMIDIData(22) = 1
          mMIDIData(23) = 0
          mMIDIData(24) = 64
          mMIDIData(25) = 31
          mMIDIData(26) = 0
          mMIDIData(27) = 0
          mMIDIData(28) = 64
          mMIDIData(29) = 31
          mMIDIData(30) = 0
          mMIDIData(31) = 0
          mMIDIData(32) = 1
          mMIDIData(33) = 0
          mMIDIData(34) = 8
          mMIDIData(35) = 0
          mMIDIData(36) = 100
          mMIDIData(37) = 97
          mMIDIData(38) = 116
          mMIDIData(39) = 97
          mMIDIData(40) = (lngSize - 2) And &HFF
          mMIDIData(41) = (lngSize - 2) \ &H100& And &HFF
          mMIDIData(42) = (lngSize - 2) \ &H10000 And &HFF
          mMIDIData(43) = (lngSize - 2) \ &H1000000 And &HFF
          lngPos = 44
          'copy data from sound resource, beginning at pos 2
          For i = 54 To UBound(bData())
            'copy this one over
            mMIDIData(lngPos) = bData(i)
            lngPos = lngPos + 1
          Next i

          'return
          BuildIIgsPCM = mMIDIData()
        Exit Function

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Function



        Public Function LoadResources() As Boolean
          'loads resources from a WAG file;
          'it WILL verify that the resource exists at a valid location
          'in the VOL files by loading them and then unloading them

          Dim bytResType As Byte

          On Error Resume Next

          agGameEvents.RaiseEvent_LoadStatus lsResources, rtNone, 0, vbNullString

          For bytResType = 0 To 3

          Next bytResType

        End Function

        Private Sub WriteSndDelta(ByVal LongIn As Long)
          'writes variable delta times!!

          Dim i As Long
          i = LngSHR(LongIn, 21)
          If (i > 0) Then
            WriteSndByte (i And 127) Or 128
          End If

          i = LngSHR(LongIn, 14)
          If (i > 0) Then
            WriteSndByte (i And 127) Or 128
          End If

          i = LngSHR(LongIn, 7)
          If (i > 0) Then
            WriteSndByte (i And 127) Or 128
          End If

          WriteSndByte LongIn And 127
        End Sub

        Private Sub WriteSndWord(ByVal IntegerIn As Integer)
          WriteSndByte IntegerIn \ 256
          WriteSndByte IntegerIn And &HFF
        End Sub

        Private Sub WriteSndLong(ByVal LongIn As Long)

          WriteSndByte LongIn \ &H1000000
          WriteSndByte (LongIn \ &H10000) And &HFF
          WriteSndByte (LongIn \ &H100) And &HFF
          WriteSndByte (LongIn) And &HFF
        End Sub

        Private Function GetTrack3Freq(Track3 As AGITrack, ByVal lngTarget As Long) As Long
          'if noise channel needs the frequency of track 3,
          'must step through track three until the same point in time is found
          'then use that frequency for noise channel

          Dim i As Long, lngTickCount As Long

          'step through notes in this track (5 bytes at a time)
          For i = 0 To Track3.Notes.Count - 1
            'add duration
            lngTickCount = lngTickCount + Track3.Notes(i).Duration
            'if equal to or past current tick Count
            If lngTickCount >= lngTarget Then
              'this is the frequency we want
              GetTrack3Freq = Track3.Notes(i).FreqDivisor
              Exit Function
            End If
          Next i

          'if nothing found, return 0

        End Function


        Private Sub WriteSndByte(ByVal ByteIn As Byte)
          'Put intFile, , ByteIn
          mMIDIData(lngPos) = ByteIn
          lngPos = lngPos + 1
          'if at end
          If lngPos > UBound(mMIDIData) Then
            'jack it up
            ReDim Preserve mMIDIData(lngPos + 256)
          End If
        End Sub


        Private Function DecodeString(ByVal intCode As Integer) As String
        'this function converts a code Value into its original string Value

        Dim i  As Integer

          'initialize counter
          i = 0

          Do While (intCode > 255)
            'if code Value exceeds table size,
            If intCode > TABLE_SIZE Then
              '*'Debug.Print "FATAL ERROR as  Invalid code (" & CStr(intCode) & ") in DecodeString."
              Exit Function
            Else
              'build string
              DecodeString = Chr$(bytAppend(intCode - 257)) & DecodeString
              intCode = intPrefix(intCode - 257)
            End If
          Loop
          DecodeString = Chr$(intCode) & DecodeString
        End Function





        Private Function InputCode(ByRef bytData() As Byte, intCodeSize As Integer, ByRef intPosIn As Integer) As Integer

          Dim lngWord As Long, lngRet As Long

          'this routine extracts the next code Value off the input stream
          'since the number of bits per code can vary between 9 and 12,
          'can't read in directly from the stream

          'unlike normal LZW, though, the bytes are actually written in so the code boundaries
          'work from right to left, NOT left to right. For example,an input stream that needs
          'to be split on a 9 bit boundary will use eight bits of first byte, plus LOWEST
          'bit of byte 2. The second code is then the upper seven bits of byte 2 and the lower
          '2 bits of byte 3 etc:
          '                          byte boundaries (8 bits per byte)
          '          byte4           byte3           byte2           byte1           byte0
          ' ...|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0
          ' ... x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x x
          ' ... 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0|8 7 6 5 4 3 2 1 0
          '                   code3             code2             code1             code0
          '                          code boundaries (9 bits per code)
          '
          'the data stream is read into a bit buffer 8 bits at a time (i.e. a single byte)
          'once the buffer is full of data, the input code is pulled out, and the buffer
          'is shifted.
          'the input data from the stream must be shifted to ensure it lines up with data
          'currently in the buffer.

          'read until the buffer is greater than 24-
          'this ensures that the eight bits read from the input stream
          'will fit in the buffer (which is a long integer==4 bytes==32 bits)
          'also stop reading data if end of data stream is reached)
          Do While (lngBitsInBuffer <= 24) And (intPosIn < lngOriginalSize)
            'get next byte
            lngWord = CLng(bytData(intPosIn))
            intPosIn = intPosIn + 1

            'shift the data to the left by enough bits so the byte being added will not
            'overwrite the bits currently in the buffer, and add the bits to the buffer
            lngBitBuffer = lngBitBuffer Or LngSHL(lngWord, lngBitsInBuffer)

            'increment Count of how many bits are currently in the buffer
            lngBitsInBuffer = lngBitsInBuffer + 8
          Loop

          'the input code starts at the lowest bit in the buffer
          'since the buffer has 32 bits total, need to clear out all bits above the desired
          'number of bits to define the code (i.e. if 9 bits, AND with &H1FF; 10 bits,
          'AND with &H3FF, etc.)
          lngRet = lngBitBuffer And (LngSHL(1, intCodeSize) - 1)
          'why not just use 2^intCodeSize -1 instead of the shift-left function?

          'now need to shift the buffer to the RIGHT by the number of bits per code
          lngBitBuffer = LngSHR(lngBitBuffer, intCodeSize)

          'adjust number of bits currently loaded in buffer
          lngBitsInBuffer = lngBitsInBuffer - intCodeSize

          'and return code Value
          InputCode = lngRet
        End Function



        Private Function NewCodeSize(intVal As Integer) As Integer
        'this function supports the expansion of compressed resources
        'it sets the size of codes which the LZW routine uses. The size
        'of the code first starts at 9 bits, then increases as all code
        'tables are filled. the max code size is 12; if an attempt is
        'made to set a code size above that, the function does nothing
        'the function also recalculates the maximum number of codes
        'available to the expand subroutine. This number is used to
        'determine when to call this function again.

          'max code size is 12 bits
          Const MAXBITS = 12

          If (intVal = MAXBITS) Then
            NewCodeSize = 11
            'this makes no sense!!!!
            'as written, it means max code size is really 11, not
            '12; an attempt to set it to 12 keeps it at 11???

          Else
            NewCodeSize = intVal
            lngMaxCode = LngSHL(1, NewCodeSize) - 2
           End If
        End Function




        Public Sub DecompressPicture(ByRef bytOriginalData() As Byte)

        Dim intPosIn As Integer, intPosOut As Integer
        Dim bytCurComp As Byte
        Dim bytBuffer As Byte
        Dim blnOffset As Boolean
        Dim bytCurUncomp As Byte
        Dim lngTempSize As Long
        Dim bytExpandedData() As Byte
        Dim lngTempCurPos As Long

          On Error GoTo ErrHandler

          'temporarily set size to max
          lngTempSize = MAX_RES_SIZE
          ReDim bytExpandedData(MAX_RES_SIZE)

          'reset variables
          intPosIn = 0
          intPosOut = 0
          blnOffset = False
          bytBuffer = 0
          lngTempCurPos = 0

          'decompress the picture
          Do
            'get current compressed byte
            bytCurComp = bytOriginalData(intPosIn)
            intPosIn = intPosIn + 1

            'if currently offset,
            If blnOffset Then
              'adjust buffer byte
              bytBuffer = bytBuffer + BytSHR(bytCurComp, 4)
              'extract uncompressed byte
              bytCurUncomp = bytBuffer
              'shift buffer back
              bytBuffer = BytSHL(bytCurComp, 4)
            Else
              'byte is not compressed
              bytCurUncomp = bytCurComp
            End If

            'save byte to temp resource
            bytExpandedData(lngTempCurPos) = bytCurUncomp
            lngTempCurPos = lngTempCurPos + 1

            'check if byte sets or restores offset
            If ((bytCurUncomp = &HF0) Or (bytCurUncomp = &HF2)) And (intPosIn < UBound(bytOriginalData) - 1) Then
              'if currently offset
              If blnOffset Then
                'write rest of buffer byte
                bytExpandedData(lngTempCurPos) = BytSHR(bytBuffer, 4)
                lngTempCurPos = lngTempCurPos + 1
                'restore offset
                blnOffset = False
              Else
                'get next byte
                bytCurComp = bytOriginalData(intPosIn)
                intPosIn = intPosIn + 1
                'save the byte, after shifting
                bytExpandedData(lngTempCurPos) = BytSHR(bytCurComp, 4)
                lngTempCurPos = lngTempCurPos + 1
                'fill buffer
                bytBuffer = BytSHL(bytCurComp, 4)
                blnOffset = True
              End If
            End If
          'continue until all original data has been read
          Loop Until intPosIn >= UBound(bytOriginalData) + 1

          'redim to arrays to actual size
          ReDim Preserve bytExpandedData(lngTempCurPos - 1)
          ReDim bytOriginalData(lngTempCurPos - 1)

          'copy arrays
          bytOriginalData = bytExpandedData

        Exit Sub

        ErrHandler:
          'all errors invalidate resource
          On Error GoTo 0: Err.Raise vbObjectError + 504, "ResourceFunctions.DecompressPicture", LoadResString(504)

        End Sub



        Public Function SndSubclassProc(ByVal hWnd As Long, ByVal uMsg As Long, ByVal wParam As Long, ByVal lParam As Long) As Long

          Dim blnSuccess As Boolean
          Dim rtn As Long

          On Error Resume Next

          If Not blnPlaying And Not frmSndSubclass.agSndToPlay Is Nothing Then
            'set it to nothing
            '*'Debug.Print "shouldn't be!"
          End If

          'check for mci msg
          Select Case uMsg
          Case MM_MCINOTIFY
            'determine success status
            blnSuccess = (wParam = MCI_NOTIFY_SUCCESSFUL)

            'close the sound
            rtn = mciSendString("close all", 0&, 0&, 0&)

            'raise the 'done' event
            frmSndSubclass.agSndToPlay.RaiseSoundComplete blnSuccess
            'agSnds(PlaySndResNum).RaiseSoundComplete blnSuccess

            'reset the flag
            blnPlaying = False

            'release the object
            Set frmSndSubclass.agSndToPlay = Nothing
          End Select

          'pass back to previous function
          SndSubclassProc = CallWindowProc(PrevSndWndProc, hWnd, uMsg, wParam, lParam)
        End Function

        Public Function UniqueResFile(ResType As AGIResType) As String

          Dim intNo As Integer

          On Error GoTo ErrHandler

          Do
            intNo = intNo + 1
            UniqueResFile = agResDir & "New" & agResTypeName(ResType) & CStr(intNo) & ".ag" & LCase$(Left$(agResTypeName(ResType), 1))
          Loop Until Dir(UniqueResFile) = vbNullString
        Exit Function

        ErrHandler:
          'on off chance that user has too many files
          Err.Clear
          On Error GoTo 0: Err.Raise vbObjectError, "ResourceFunctions.UniqueResFile", "Can't save- max number of files exceeded in directory. NO WAY YOU HAVE THAT MANY FILES!!!!!"
        End Function


         */
  }
}
