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
    internal static int lngCurrentVol, lngCurrentLoc;
    internal static BinaryWriter bwDIR, bwVOL;
    internal static FileStream fsDIR, fsVOL;
    internal static byte[,] bytDIR = new byte[4, 768]; // (3, 767)
    internal static string strNewDir;

    //Private Sub AddResInfo(ByVal ResVOL As Byte, ByVal ResLOC As Long, ByVal ResSize As Long, VOLLoc() As Long, VOLSize() As Long)

    //  'adds TempRes to volume loc/size arrays, sorted by LOC
    //  '1st element of each VOL section is number of entries for that VOL
    //  Dim i As Long, j As Long

    //  'if this is first resource for this volume
    //  If VOLLoc(ResVOL, 0) = 0 Then
    //    'add it to first position
    //    VOLLoc(ResVOL, 1) = ResLOC
    //    VOLSize(ResVOL, 1) = ResSize
    //    'increment Count for this volume
    //    VOLLoc(ResVOL, 0) = 1
    //  Else
    //    'find the place where this vol belongs
    //    For i = 1 To VOLLoc(ResVOL, 0)
    //      'if it comes BEFORE this position,
    //      If ResLOC<VOLLoc(ResVOL, i) Then
    //        'i contains position where new volume belongs
    //        Exit For
    //      End If
    //    Next i

    //    'first, increment Count
    //    VOLLoc(ResVOL, 0) = VOLLoc(ResVOL, 0) + 1

    //    'now move all items ABOVE i up one space
    //    For j = VOLLoc(ResVOL, 0) To i + 1 Step -1
    //      VOLLoc(ResVOL, j) = VOLLoc(ResVOL, j - 1)
    //      VOLSize(ResVOL, j) = VOLSize(ResVOL, j - 1)
    //    Next j

    //    'store new data at position i
    //    VOLLoc(ResVOL, i) = ResLOC
    //    VOLSize(ResVOL, i) = ResSize
    //  End If
    //End Sub

    //Public Sub AddToVol(AddRes As AGIResource, ByVal Version3 As Boolean, Optional NewVOL As Boolean = False, Optional ByVal lngVol As Long = -1, Optional ByVal lngLoc As Long = -1)
    //  'this method will add a resource to a VOL file
    //  '
    //  'if the NewVOL flag is true, it adds the resource
    //  'to the specified vol file at the specified location
    //  'the DIR file is not updated; that is done by the
    //  'CompileGame method (which calls this method)
    //  '
    //  'if the NewVOL flag is false, it finds the first
    //  'available spot for the resource, and adds it there
    //  'the method will add the resource at a new location based
    //  'on first open position; it will not delete the resource
    //  'data from its old position (but the area will be available
    //  'for future use by another resource)
    //  'and then it updates the DIR file
    //  '
    //  'only resources that are in a game can be added to a VOL file
    //  '

    //  Dim ResHeader() As Byte
    //  Dim strID As String


    //  On Error Resume Next

    //  'should NEVER get here for a resource
    //  'that is NOT in a game
    //  '*'Debug.Assert AddRes.InGame = True

    //  'if NOT adding to a new VOL file
    //  If Not NewVOL Then
    //    'get vol number and location where there is room for this resource
    //    FindFreeVOLSpace AddRes

    //    'if error
    //    If Err.Number<> 0 Then
    //      'pass it along
    //      lngError = Err.Number
    //      strError = Err.Description
    //      strErrSrc = Err.Source


    //      On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    //      Exit Sub
    //    End If
    //    lngLoc = AddRes.Loc
    //    lngVol = AddRes.Volume
    //  Else
    //    'need to verify valid values are passed
    //    If lngLoc < 0 Or lngVol < 0 Then
    //      'error!!!
    //      '*'Debug.Assert False
    //      '*'Debug.Assert False
    //      'add appropriate error handling here
    //    End If
    //  End If

    //  'build header
    //  ReDim ResHeader(4)
    //  ResHeader(0) = &H12
    //  ResHeader(1) = &H34
    //  ResHeader(2) = lngVol
    //  ResHeader(3) = AddRes.Size Mod 256
    //  ResHeader(4) = AddRes.Size \ 256

    //  'if the resource is a version 3 resource,
    //  If Version3 Then
    //    'adjust header size
    //    ReDim Preserve ResHeader(6)
    //    ResHeader(5) = ResHeader(3)
    //    ResHeader(6) = ResHeader(4)
    //    strID = agGameID
    //  Else
    //    strID = vbNullString
    //  End If

    //  'if not adding to a new vol file
    //  If Not NewVOL Then
    //    'save the resource into the vol file
    //    'get as file number
    //    intVolFile = FreeFile()
    //    Open agGameDir & strID & "VOL." + CStr(lngVol) For Binary As intVolFile
    //  End If

    //  'add header to vol file
    //  Put intVolFile, lngLoc + 1, ResHeader

    //  'add resource data to vol file
    //  Put intVolFile, , AddRes.AllData
    //  'if error,
    //  If Err.Number<> 0 Then
    //    'save error
    //    strError = Err.Description
    //    strErrSrc = Err.Source
    //    lngError = Err.Number

    //    'if not adding to a new file, close volfile
    //    If Not NewVOL Then
    //      Close intVolFile
    //    End If
    //    On Error GoTo 0: Err.Raise vbObjectError + 638, strErrSrc, Replace(LoadResString(638), ARG1, CStr(lngError) & ":" & strError)
    //    Exit Sub
    //  End If

    //  'always update sizeinvol
    //  SetSizeInVol AddRes, AddRes.Size

    //  'if adding to a new vol file
    //  If NewVOL Then
    //    'increment loc pointer
    //    lngCurrentLoc = lngCurrentLoc + AddRes.Size + 5
    //    'if version3
    //    If Version3 Then
    //      'add two more
    //      lngCurrentLoc = lngCurrentLoc + 2
    //    End If
    //  Else
    //    'close vol file
    //    Close intVolFile
    //    'update location in dir file
    //    UpdateDirFile AddRes

    //    'check for errors
    //    If Err.Number<> 0 Then
    //      'only error that will be returned is expandv3dir error
    //      'pass it along
    //      lngError = Err.Number
    //      strError = Err.Description
    //      strErrSrc = Err.Source


    //      On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    //      Exit Sub
    //    End If
    //  End If
    //End Sub

    internal static void CompileResCol(object tmpResColl, AGIResType ResType, bool RebuildOnly, bool NewIsV3)
    {
      //Public Sub CompileResCol(ByVal tmpResCol As Object, ByVal ResType As AGIResType, ByVal RebuildOnly As Boolean, ByVal NewIsV3 As Boolean)

      //  'compiles all the resources of ResType that are in the tmpResCol collection object
      //  'by adding them to the new VOL files
      //  'if RebuildOnly is passed, it won't try to compile the logic; it will only add
      //  'the items into the new VOL files
      //  'the NewIsV3 flag is used when the compiling activity is being done because
      //  'the version is changing from a V2 game to a V3 game

      //  'if a resource error is encountered, the function returns a Value of False
      //  'if no resource errors encountered, the function returns True

      //  'if any other errors encountered, the Err object is set and the calling function
      //  'must deal with the error


      //  Dim tmpGameRes As Object, tmpRes As AGIResource
      //  Dim CurResNum As Long, strMsg As String
      //  Dim blnWarning As Boolean, lngV3Offset As Long
      //  Dim blnUnloadGRes As Boolean
      //  Dim blnUnloadRes As Boolean, strError As String


      //  On Error Resume Next

      //  'add all resources of this type
      //  For Each tmpGameRes In tmpResCol
      //    CurResNum = tmpGameRes.Number
      //    'update status
      //    agGameEvents.RaiseEvent_CompileGameStatus csAddResource, ResType, CurResNum, vbNullString
      //    'check for cancellation
      //    If Not agCompGame Then
      //      CompleteCancel
      //      GoTo DoneAdding
      //    End If

      //    'use a loop to add resources;
      //    'exit loop when an error occurs, or after
      //    'resource is successfully added to vol file
      //    Do
      //      'get the actual resource object
      //      Set tmpRes = tmpGameRes.Resource
      //      'set flag to force unload, if game res not currently loaded
      //      blnUnloadGRes = Not tmpGameRes.Loaded
      //      'set flag to force unload, if resource not currently loaded
      //      blnUnloadRes = Not tmpRes.Loaded

      //      'if rebuild only
      //      If RebuildOnly Then
      //        'if need to load resource,
      //        If blnUnloadRes Then
      //          'load resource
      //          tmpRes.Load
      //          'if error,
      //          If Err.Number<> 0 Then
      //            'note it
      //            agGameEvents.RaiseEvent_CompileGameStatus csResError, ResType, CurResNum, "Unable to load " & tmpGameRes.ID & " (" & Err.Description & ")"
      //            'check for cancellation
      //            If Not agCompGame Then
      //              CompleteCancel
      //              GoTo DoneAdding
      //            End If
      //            'if not canceled, user has already removed bad resource
      //            Err.Clear
      //            Exit Do
      //          End If
      //        End If
      //      Else
      //        'load game resource if necessary
      //        If blnUnloadGRes Then
      //          'always reset warning flag
      //          blnWarning = False

      //          'load game resource
      //          tmpGameRes.Load

      //          'picture warnings aren't in error handler anymore
      //          If ResType = rtPicture Then
      //            'check bmp error level by property, not error number
      //            Select Case tmpGameRes.BMPErrLevel
      //            Case 0
      //              'ok
      //'''              Case -1
      //'''                'bad error
      //            Case Is >= 8
      //              'unhandled error
      //              strMsg = "Unhandled error in picture data- picture may not display correctly"
      //              blnWarning = True
      //            Case Else
      //              'missing EOP marker, bad colr or bad cmd
      //              strMsg = "Data anomalies in picture data but picture should display"
      //              blnWarning = True
      //            End Select
      //          End If

      //          'if error,
      //          If Err.Number<> 0 And Err.Number<> vbObjectError + 610 Then
      //          'the 610 error is the one related to bitmap creation; it doesn't really
      //          'mean there's an error

      //            'assume a resource error until determined otherwise
      //            strMsg = "Unable to load " & tmpGameRes.ID & " (" & Err.Description & ")"
      //            'reset warning flag
      //            blnWarning = False


      //            Select Case ResType
      //            Case rtPicture
      //'''              'check bmp error level by property, not error number
      //'''              'unused errors:
      //'''              ' 527 Unknown drawing action 0x%1 found at position 0x%2.
      //'''              ' 609 Unhandled error while building picture (%1) at position 0x%2
      //'''              ' 682 Invalid picture resource file(missing end-of-resource code)
      //'''
      //''''''              'check for invalid picture data errors
      //''''''              If Err.Number = vbObjectError + 527 Or Err.Number = vbObjectError + 609 Then
      //''''''                'can add picture, but it is invalid
      //''''''                strMsg = "Invalid picture data (" & Err.Description & ")"
      //''''''                blnWarning = True
      //''''''              End If


      //            Case rtView
      //              'check for invalid view data errors
      //              Select Case Err.Number - vbObjectError
      //              Case 537, 548, 552, 553, 513, 563, 539, 540, 550, 551
      //                'can add view, but it is invalid
      //                strMsg = "Invalid view data (" & Err.Description & ")"
      //                blnWarning = True
      //              End Select


      //            Case rtSound
      //              'check for invalid sound data errors
      //              If Err.Number = vbObjectError + 598 Then
      //                'can add sound, but it is invalid
      //                strMsg = "Invalid sound data (" & Err.Description & ")"
      //                blnWarning = True
      //              End If


      //            Case rtLogic
      //              'always a resource error
      //            End Select


      //            Err.Clear

      //            'if error (warning not set)
      //            If Not blnWarning Then
      //              'note the error
      //              agGameEvents.RaiseEvent_CompileGameStatus csResError, ResType, CurResNum, strMsg
      //              'check for cancellation
      //              If Not agCompGame Then
      //                CompleteCancel
      //                GoTo DoneAdding
      //              End If
      //              'if not canceled, user has already removed bad resource from game
      //              Err.Clear
      //              Exit Do
      //            End If
      //          End If

      //          'if a warning
      //          If blnWarning Then
      //            'note the warning
      //            agGameEvents.RaiseEvent_CompileGameStatus csWarning, ResType, CurResNum, "--|" & strMsg & "|--|--"
      //            'check for cancellation
      //            If Not agCompGame Then
      //              CompleteCancel
      //              GoTo DoneAdding
      //            End If
      //          End If
      //        End If

      //        'for logics, compile the sourcetext
      //        'load actual object
      //        If ResType = rtLogic Then
      //          'compile it
      //          CompileLogic tmpGameRes

      //          'if error
      //          Select Case Err.Number
      //          Case vbObjectError + 635 'compile error
      //            'raise compile event
      //            agGameEvents.RaiseEvent_CompileGameStatus csLogicError, ResType, CurResNum, Err.Description
      //            'check for cancellation
      //            If Not agCompGame Then
      //              CompleteCancel
      //              GoTo DoneAdding
      //            End If
      //            'if user wants to continue, the logic will already have been removed; no other action needed
      //            Err.Clear
      //            Exit Do

      //          Case 0
      //            'no error
      //          Case Else
      //            'note it
      //            agGameEvents.RaiseEvent_CompileGameStatus csResError, ResType, CurResNum, "Unable to compile Logic (" & Err.Description & ")"
      //            'check for cancellation
      //            If Not agCompGame Then
      //              CompleteCancel
      //              GoTo DoneAdding
      //            End If
      //            'if user did not cancel, resource will already have been removed
      //            Err.Clear
      //            Exit Do
      //          End Select
      //        End If
      //      End If

      //      'validate vol and loc
      //      ValidateVolAndLoc tmpRes.Size

      //      'if error
      //      If Err.Number<> 0 Then
      //        'save error msg
      //        strError = Err.Description
      //        strErrSrc = Err.Source
      //        lngError = Err.Number

      //        'clean up compiler
      //        CompleteCancel True

      //        'unload resource, if applicable
      //        If blnUnloadRes Then
      //          If Not tmpRes Is Nothing Then
      //            tmpRes.Unload
      //          End If
      //        End If
      //        Set tmpRes = Nothing

      //        'unload game resource, if applicable
      //        If blnUnloadGRes Then
      //          If Not tmpGameRes Is Nothing Then
      //            tmpGameRes.Unload
      //          End If
      //        End If

      //        'raise appropriate error
      //        If lngError = vbObjectError + 593 Then 'exceed max storage
      //          On Error GoTo 0: Err.Raise vbObjectError + 593, "CompResCol", LoadResString(593)
      //        Else 'file access error
      //          On Error GoTo 0: Err.Raise vbObjectError + 638, strErrSrc, Replace(LoadResString(638), ARG1, CStr(lngError) & ":" & strError)
      //        End If
      //      End If

      //      'new strategy is to use arrays for DIR data, and then
      //      'to build the DIR files after compiling all resources
      //      bytDIR(ResType, tmpRes.Number* 3) = lngCurrentVol* &H10 + lngCurrentLoc \ &H10000
      //    bytDIR(ResType, tmpRes.Number* 3 + 1) = (lngCurrentLoc Mod &H10000) \ &H100
      //   bytDIR(ResType, tmpRes.Number* 3 + 2) = lngCurrentLoc Mod &H100

      //      'add it to vol
      //      AddToVol tmpRes, NewIsV3, True, lngCurrentVol, lngCurrentLoc
      //      'if error,
      //      If Err.Number<> 0 Then
      //        'note it
      //        agGameEvents.RaiseEvent_CompileGameStatus csResError, ResType, CurResNum, "Unable to add Logic resource to VOL file (" & Err.Description & ")"
      //        'check for cancellation
      //        If Not agCompGame Then
      //          CompleteCancel
      //          GoTo DoneAdding
      //        End If
      //        Exit Do
      //      End If

      //    'always exit loop
      //    Loop Until True

      //    'clear any errors
      //    Err.Clear

      //    If RebuildOnly Then
      //      'unload resource, if applicable
      //      If blnUnloadRes Then
      //        If Not tmpRes Is Nothing Then
      //          tmpRes.Unload
      //        End If
      //      End If
      //    Else
      //      'unload game resource, if applicable
      //      If blnUnloadGRes Then
      //        If Not tmpGameRes Is Nothing Then
      //          tmpGameRes.Unload
      //          '*'Debug.Assert Err.Number = 0
      //        End If
      //      End If
      //    End If
      //  Next

      //DoneAdding:

      //  'unload resource, if applicable
      //  If RebuildOnly Then
      //    If blnUnloadRes Then
      //      If Not tmpRes Is Nothing Then
      //        tmpRes.Unload
      //      End If
      //    End If
      //    Set tmpRes = Nothing
      //  Else
      //    'unload game resource, if applicable
      //    If blnUnloadGRes Then
      //      If Not tmpGameRes Is Nothing Then
      //        tmpGameRes.Unload
      //      End If
      //    End If
      //  End If

      //  'clear any errors
      //  Err.Clear
      //End Sub
    }

    //Public Sub CompleteCancel(Optional ByVal NoEvent As Boolean = False)
    //  'cleans up after a compile game cancel or error

    //  If Not NoEvent Then
    //    agGameEvents.RaiseEvent_CompileGameStatus csCanceled, 0, 0, vbNullString
    //  End If
    //  agCompGame = False

    //  Close intVolFile
    //End Sub


    //Public Sub FindFreeVOLSpace(NewResource As AGIResource)

    //  'this method will try and find a volume and location to store a resource
    //  'if a resource is being updated, it will have its volume set to 255

    //  'sizes are adjusted to include the header that AGI uses at the beginning of each
    //  'resource; 5 bytes for v2 and 7 bytes for v3

    //  Dim lngLoc(15, 1023) As Long
    //  Dim lngSize(15, 1023) As Long
    //  Dim tmpRes As AGIResource, tmpSize As Long
    //  Dim lngHeader As Long, lngMaxVol As Long
    //  Dim i As Integer, j As Integer
    //  Dim lngStart As Long, lngEnd As Long
    //  Dim lngFree As Long
    //  Dim strID As String
    //  Dim NewResType As AGIResType, NewResNum As Byte, NewResSize As Long


    //  On Error Resume Next

    //  'set header size, max# of VOL files and ID string depending on version
    //  If agIsVersion3 Then
    //    lngHeader = 7
    //    strID = agGameID
    //    lngMaxVol = 15
    //  Else
    //    lngHeader = 5
    //    strID = vbNullString
    //    lngMaxVol = 4
    //  End If

    //  'local copy of restype and resnum (for improved speed)
    //  NewResType = NewResource.ResType
    //  NewResNum = NewResource.Number
    //  NewResSize = NewResource.Size + lngHeader

    //  'build array of all resources, sorted by VOL order (except the one being loaded)
    //  For Each tmpRes In agLogs
    //    'if not the resource being replaced
    //    If NewResType<> rtLogic Or tmpRes.Number<> NewResNum Then
    //      tmpSize = tmpRes.SizeInVol + lngHeader
    //      'if no error
    //      If Err.Number = 0 Then
    //        AddResInfo tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc(), lngSize()
    //      Else
    //        '*'Debug.Print "err: "; Err.Description: '*'Debug.Print tmpRes.ResType; "|"; tmpRes.Number: '*'Debug.Print
    //      End If
    //      Err.Clear
    //    End If
    //  Next

    //  For Each tmpRes In agPics
    //    'if not the resource being replaced
    //    If NewResType <> rtPicture Or tmpRes.Number<> NewResNum Then
    //      tmpSize = tmpRes.SizeInVol + lngHeader
    //      'if no error
    //      If Err.Number = 0 Then
    //        AddResInfo tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc(), lngSize()
    //      Else
    //        '*'Debug.Print "err: "; Err.Description: '*'Debug.Print tmpRes.ResType; "|"; tmpRes.Number: '*'Debug.Print
    //      End If
    //      Err.Clear
    //    End If
    //  Next

    //  For Each tmpRes In agSnds
    //    'if not the resource being replaced
    //    If NewResType <> rtSound Or tmpRes.Number<> NewResNum Then
    //      tmpSize = tmpRes.SizeInVol + lngHeader
    //      'if no error
    //      If Err.Number = 0 Then
    //        AddResInfo tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc(), lngSize()
    //      Else
    //        '*'Debug.Print "err: "; Err.Description: '*'Debug.Print tmpRes.ResType; "|"; tmpRes.Number: '*'Debug.Print
    //      End If
    //      Err.Clear
    //    End If
    //  Next

    //  For Each tmpRes In agViews
    //    'if not the resource being replaced
    //    If NewResType <> rtView Or tmpRes.Number<> NewResNum Then
    //      tmpSize = tmpRes.SizeInVol + lngHeader
    //      'if no error
    //      If Err.Number = 0 Then
    //        AddResInfo tmpRes.Volume, tmpRes.Loc, tmpSize, lngLoc(), lngSize()
    //      Else
    //        '*'Debug.Print "err: "; Err.Description: '*'Debug.Print tmpRes.ResType; "|"; tmpRes.Number: '*'Debug.Print
    //      End If
    //      Err.Clear
    //    End If
    //  Next

    //  On Error GoTo ErrHandler

    //  'step through volumes,
    //  For i = 0 To lngMaxVol
    //    'start at beginning
    //    lngStart = 0
    //    For j = 1 To lngLoc(i, 0) + 1
    //      'if this is not the end of the list
    //      If j<lngLoc(i, 0) + 1 Then
    //      lngEnd = lngLoc(i, j)
    //      Else
    //        lngEnd = MAX_VOLSIZE
    //      End If
    //      'calculate space between end of one resource and start of next
    //      lngFree = lngEnd - lngStart

    //      'if enough space is found
    //      If lngFree >= NewResSize Then
    //        'set volume and location
    //        NewResource.Volume = i
    //        NewResource.Loc = lngStart
    //        Exit Sub
    //      End If
    //      'set start to end of current resource
    //      lngStart = lngLoc(i, j) + lngSize(i, j)
    //    Next j
    //  Next i

    //  'if no room in any VOL file, raise an error
    //  On Error GoTo 0: Err.Raise vbObjectError + 593, "ResourceFunctions.FindFreeVOLSpace", LoadResString(593)
    //Exit Sub

    //ErrHandler:
    //  'just put at end of first volume with enough room

    //  For i = 0 To lngMaxVol
    //    'if this vol file exists,
    //    If FileExists(agGameDir & strID & "VOL." & CStr(i)) Then
    //      'if there is enough room,
    //      If FileLen(agGameDir & strID & "VOL." & CStr(i)) + NewResource.Size<MAX_VOLSIZE Then
    //        'this is the volume where the resource will be added
    //        NewResource.Volume = i
    //        NewResource.Loc = FileLen(agGameDir & strID & "VOL." & CStr(i))
    //        Exit Sub
    //      End If
    //    Else
    //      'a new file is needed
    //      NewResource.Volume = i
    //      NewResource.Loc = 0
    //      Exit Sub
    //    End If
    //  Next i

    //  'if for/next exited normally,
    //  'that means no volume found (reached max of 16)
    //  On Error GoTo 0: Err.Raise vbObjectError + 593, "ResourceFunctions.FindFreeVOLSpace", LoadResString(593)
    //End Sub


    //Public Function GetSizeInVOL(ByVal bytVol As Byte, ByVal lngLoc As Long) As Long
    //  'returns the size of this resource in its VOL file
    //  'inputs are the volume filename and offset to beginning
    //  'of resource

    //  'if an error occurs while trying to read the size of this
    //  'resource, the function returns -1
    //  Dim intFile As Integer
    //  Dim bytHigh As Byte, bytLow As Byte
    //  Dim lngV3Offset As Long
    //  Dim strVolFile As String

    //  'any file access errors
    //  'result in invalid size
    //  On Error GoTo ErrHandler

    //  'if version 3
    //  If agIsVersion3 Then
    //    'adjusts header so compressed size is retrieved
    //    lngV3Offset = 2
    //    'set filename
    //    strVolFile = agGameDir & agGameID & "VOL." & CStr(bytVol)
    //  Else
    //    'set filename
    //    strVolFile = agGameDir & "VOL." & CStr(bytVol)
    //  End If

    //  'open the volume file
    //  intFile = FreeFile()
    //  Open strVolFile For Binary As intFile

    //  'verify enough room to get length of resource
    //  If LOF(intFile) >= lngLoc + 5 Then
    //    'get size low and high bytes
    //    '(adjust by one, since Get function is '1' based
    //    'and resources are '0' based)
    //    Get intFile, lngLoc + 1, bytLow
    //    Get intFile, , bytHigh
    //    'verify this is a proper resource
    //    If(bytLow = &H12) And(bytHigh = &H34) Then
    //      'now get the low and high bytes of the size
    //      Get intFile, lngLoc + lngV3Offset + 4, bytLow
    //      Get intFile, lngLoc + lngV3Offset + 5, bytHigh
    //      GetSizeInVOL = CLng(bytHigh) * 256 & +CLng(bytLow)
    //      Close intFile
    //      Exit Function
    //    End If
    //  End If
    //ErrHandler:
    //  'if any errors encountered,
    //  'ensure file is closed, and return -1
    //  Err.Clear
    //  Close intFile
    //  GetSizeInVOL = -1
    //End Function



    //Public Sub ShrinkDirFile(intFile As Integer, ByVal Size As Integer)
    //  'shrinking is harder; I think we have to copy the data up to Size, then
    //  'close the current file, then re-open a new file; copy the data to it
    //  'then delete original file, copy new file to old file and then,
    //  'finally, re-open the old file (which should now be smaller)

    //End Sub

    //Public Sub SetSizeInVol(ThisResource As AGIResource, ByVal NewSizeInVol As Long)

    //  'sets the size of the resource
    //  Select Case ThisResource.ResType
    //  Case rtLogic
    //    agLogs(ThisResource.Number).Resource.SizeInVol = NewSizeInVol
    //  Case rtPicture
    //    agPics(ThisResource.Number).Resource.SizeInVol = NewSizeInVol
    //  Case rtSound
    //    agSnds(ThisResource.Number).Resource.SizeInVol = NewSizeInVol
    // Case rtView
    //    agViews(ThisResource.Number).Resource.SizeInVol = NewSizeInVol
    //  End Select
    //End Sub

    //Public Sub UpdateDirFile(UpdateResource As AGIResource, Optional Remove As Boolean = False)
    //  'this method updates the DIR file with the volume and location
    //  'of a resource
    //  'if Remove option passed as true,
    //  'the resource is treated as 'deleted' and
    //  ' &HFFFFFF is written in the resource's DIR file place holder

    //  'NOTE: directories inside a V3 dir file are in this order: LOGIC, PICTURE, VIEW, SOUND
    //  'the ResType enumeration is in this order: LOGIC, PICTURE, SOUND, VIEW
    //  'because of the switch between VIEW and SOUND, can't use a formula to calculate
    //  'directory offsets


    //  Dim strDirFile As String, intFile As Integer
    //  Dim bytDIR() As Byte, intMax As Integer, intOldMax As Integer
    //  Dim DirByte(0 To 2) As Byte
    //  Dim lngDirOffset As Long, lngDirEnd As Long
    //  Dim i As Long, lngStart As Long, lngStop As Long


    //  On Error GoTo ErrHandler
    //  'strategy:
    //  'if deleting--
    //  '    is the the dir larger than the new max?
    //  '    yes: compress the dir
    //  '    no:  insert &HFFs
    //  '
    //  'if adding--
    //  '    is the dir too small?
    //  '    yes: expand the dir
    //  '    no:  insert the data


    //  If Remove Then
    //    'resource marked for deletion
    //    DirByte(0) = &HFF
    //    DirByte(1) = &HFF
    //    DirByte(2) = &HFF
    //  Else
    //    'calculate directory bytes
    //    DirByte(0) = UpdateResource.Volume* &H10 + UpdateResource.Loc \ &H10000
    //   DirByte(1) = (UpdateResource.Loc Mod &H10000) \ &H100
    //   DirByte(2) = UpdateResource.Loc Mod &H100
    // End If

    //  'what is current max for this res type?
    //  Select Case UpdateResource.ResType
    //  Case rtLogic
    //    intMax = agLogs.Max
    //  Case rtPicture
    //    intMax = agPics.Max
    //  Case rtSound
    //    intMax = agSnds.Max
    //  Case rtView
    //    intMax = agViews.Max
    //  End Select

    //  'open the correct dir file, store in a temp array
    //  intFile = FreeFile()
    //  'if version3
    //  If agIsVersion3 Then
    //    strDirFile = agGameDir & agGameID & "DIR"
    //  Else
    //    strDirFile = agGameDir & ResTypeAbbrv(UpdateResource.ResType) & "DIR"
    //  End If
    //  Open strDirFile For Binary As intFile
    //  ReDim bytDIR(LOF(intFile) - 1) ' correct! bytDIR is 0 based!
    //  Get intFile, 1, bytDIR()
    //  Close intFile

    //  'calculate old max and offset (for v3 files)
    //  If agIsVersion3 Then
    //    'calculate directory offset
    //    Select Case UpdateResource.ResType
    //    Case rtLogic
    //      lngDirOffset = 8
    //      lngDirEnd = bytDIR(3) * 256 + bytDIR(2)
    //    Case rtPicture
    //      lngDirOffset = bytDIR(3) * 256 + bytDIR(2)
    //      lngDirEnd = bytDIR(5) * 256 + bytDIR(4)
    //    Case rtView
    //      lngDirOffset = bytDIR(5) * 256 + bytDIR(4)
    //      lngDirEnd = bytDIR(7) * 256 + bytDIR(6)
    //    Case rtSound
    //      lngDirOffset = bytDIR(7) * 256 + bytDIR(6)
    //      lngDirEnd = UBound(bytDIR()) + 1
    //    End Select
    //    intOldMax = (lngDirEnd - lngDirOffset) / 3 - 1
    //  Else
    //    lngDirEnd = (UBound(bytDIR()) + 1)
    //    intOldMax = lngDirEnd / 3 - 1
    //    lngDirOffset = 0
    //  End If

    //  'if it fits (doesn't matter if inserting or deleting)
    //  If(Remove And intMax >= intOldMax) Or(Not Remove And intOldMax >= intMax) Then
    //    'adjust offset for resnum
    //    lngDirOffset = lngDirOffset + 3 * UpdateResource.Number

    //    'just insert the new data, and save the file
    //    intFile = FreeFile()
    //    Open strDirFile For Binary As intFile
    //    Put intFile, lngDirOffset + 1, DirByte()
    //    Close intFile
    //    Exit Sub
    //  End If

    //  'size has changed!
    //  If Remove Then
    //    'must be shrinking
    //    '*'Debug.Assert intOldMax > intMax
    //    'if v2, just redim the array
    //    If Not agIsVersion3 Then
    //      ReDim Preserve bytDIR((intMax + 1) * 3 - 1)
    //    Else
    //      'if restype is sound, we also can just truncate the file
    //      If UpdateResource.ResType = rtSound Then
    //        ReDim Preserve bytDIR(UBound(bytDIR) - 3 * (intOldMax - intMax))
    //      Else
    //        'we need to move data from the current directory's max
    //        'backwards to compress the directory
    //        'start with resource just after new max; then move all bytes down
    //        lngStart = lngDirOffset + intMax* 3 + 3
    //        lngStop = UBound(bytDIR) - 3 * (intOldMax - intMax)
    //        For i = lngStart To lngStop
    //          bytDIR(i) = bytDIR(i + 3 * (intOldMax - intMax))
    //        Next i
    //        'now shrink the array
    //        ReDim Preserve bytDIR(UBound(bytDIR()) - 3 * (intOldMax - intMax))
    //        'lastly, we need to update affected offsets
    //        'move snddir offset first
    //        lngDirOffset = bytDIR(7) * 256 + bytDIR(6)
    //        lngDirOffset = lngDirOffset - 3 * (intOldMax - intMax)
    //        bytDIR(7) = lngDirOffset \ 256
    //        bytDIR(6) = lngDirOffset Mod 256
    //        'if resource is a view, we are done
    //        If UpdateResource.ResType<> rtView Then
    //          'move view offset
    //          lngDirOffset = bytDIR(5) * 256 + bytDIR(4)
    //          lngDirOffset = lngDirOffset - 3 * (intOldMax - intMax)
    //          bytDIR(5) = lngDirOffset \ 256
    //          bytDIR(4) = lngDirOffset Mod 256
    //          'if resource is a pic, we are done
    //          If UpdateResource.ResType<> rtPicture Then
    //            'move picture offset
    //            lngDirOffset = bytDIR(3) * 256 + bytDIR(2)
    //            lngDirOffset = lngDirOffset - 3 * (intOldMax - intMax)
    //            bytDIR(3) = lngDirOffset \ 256
    //            bytDIR(2) = lngDirOffset Mod 256
    //          End If
    //        End If
    //      End If
    //    End If

    //    'delete the existing file
    //    Kill strDirFile
    //    'now save the file
    //    intFile = FreeFile()
    //    Open strDirFile For Binary As intFile
    //    Put intFile, 1, bytDIR()
    //    Close intFile
    //  Else
    //    'must be expanding
    //    '*'Debug.Assert intMax > intOldMax
    //    '*'Debug.Assert UpdateResource.Number = intMax

    //    ReDim Preserve bytDIR(UBound(bytDIR()) + 3 * (intMax - intOldMax))

    //    'if v2, add ffs to fill gap up to the last entry
    //    If Not agIsVersion3 Then
    //      lngStart = lngDirEnd
    //      lngStop = lngDirEnd + 3 * (intMax - intOldMax - 1) - 1
    //      For i = lngStart To lngStop
    //        bytDIR(i) = &HFF
    //      Next i
    //      'add dir data to end
    //      bytDIR(lngStop + 1) = DirByte(0)
    //      bytDIR(lngStop + 2) = DirByte(1)
    //      bytDIR(lngStop + 3) = DirByte(2)
    //    Else

    //      'if expanding the sound dir, just fill it in with FF's
    //      If UpdateResource.ResType = rtSound Then
    //        lngStart = lngDirEnd
    //        lngStop = lngDirEnd + 3 * (intMax - intOldMax - 1) - 1
    //        For i = lngStart To lngStop
    //          bytDIR(i) = &HFF
    //        Next i
    //        'add dir data to end
    //        bytDIR(lngStop + 1) = DirByte(0)
    //        bytDIR(lngStop + 2) = DirByte(1)
    //        bytDIR(lngStop + 3) = DirByte(2)
    //      Else
    //        'move data to make room for inserted resource
    //        lngStop = UBound(bytDIR())
    //        lngStart = lngDirEnd + 3 * (intMax - intOldMax)
    //        For i = lngStop To lngStart Step -1
    //          bytDIR(i) = bytDIR(i - 3 * (intMax - intOldMax))
    //        Next i
    //        'insert ffs, up to insert location
    //        lngStop = lngStart - 4
    //        lngStart = lngStop - 3 * (intMax - intOldMax - 1) + 1
    //        For i = lngStart To lngStop
    //          bytDIR(i) = &HFF
    //        Next i
    //        'add dir data to end
    //        bytDIR(lngStop + 1) = DirByte(0)
    //        bytDIR(lngStop + 2) = DirByte(1)
    //        bytDIR(lngStop + 3) = DirByte(2)

    //        'last thing is to adjust the offsets
    //        'move snddir offset first
    //        lngDirOffset = bytDIR(7) * 256 + bytDIR(6)
    //        lngDirOffset = lngDirOffset + 3 * (intMax - intOldMax)
    //        bytDIR(7) = lngDirOffset \ 256
    //        bytDIR(6) = lngDirOffset Mod 256
    //        'if resource is a view, we are done
    //        If UpdateResource.ResType<> rtView Then
    //          'move view offset
    //          lngDirOffset = bytDIR(5) * 256 + bytDIR(4)
    //          lngDirOffset = lngDirOffset + 3 * (intMax - intOldMax)
    //          bytDIR(5) = lngDirOffset \ 256
    //          bytDIR(4) = lngDirOffset Mod 256
    //          'if resource is a pic, we are done
    //          If UpdateResource.ResType<> rtPicture Then
    //            'move picture offset
    //            lngDirOffset = bytDIR(3) * 256 + bytDIR(2)
    //            lngDirOffset = lngDirOffset + 3 * (intMax - intOldMax)
    //            bytDIR(3) = lngDirOffset \ 256
    //            bytDIR(2) = lngDirOffset Mod 256
    //          End If
    //        End If
    //      End If
    //    End If

    //    'now save the file
    //    intFile = FreeFile()
    //    Open strDirFile For Binary As intFile
    //    Put intFile, 1, bytDIR()
    //    Close intFile
    //  End If


    //Exit Sub

    //ErrHandler:
    //  '*'Debug.Assert False
    //  Resume Next
    //End Sub

    //Public Sub ValidateVolAndLoc(ByVal ResSize As Long)
    //  'this method ensures current vol has room for resource with given size

    //  'if not, it closes current vol file, and opens next one

    //  'this method is only used by the game compiler when doing a
    //  'complete compile or resource rebuild

    //  Dim i As Long, lngMaxVol As Long


    //  On Error Resume Next

    //  'verify enough room here
    //  If lngCurrentLoc + ResSize > MAX_VOLSIZE Or(lngCurrentVol = 0 And (lngCurrentLoc + ResSize > agMaxVol0)) Then

    //    'set maxvol count to 4 or 15, depending on version
    //    If agIsVersion3 Then
    //      lngMaxVol = 15
    //    Else
    //      lngMaxVol = 4
    //    End If

    //    'close current vol
    //    Close intVolFile

    //    'first check previous vol files, to see if there is room at end of one of those
    //    For i = 0 To lngMaxVol
    //      'open this file (if the file doesn't exist, this will create it
    //      'and it will then be set as the next vol, with pos=0
    //      intVolFile = FreeFile()
    //      Open strNewDir & "NEW_VOL." & CStr(i) For Binary As intVolFile
    //      'check for error
    //      If Err.Number<> 0 Then
    //        'also, compiler should check for this error, as it is fatal
    //        On Error GoTo 0: Err.Raise vbObjectError + 640, "VolManager.ValidateVolAndLoc", LoadResString(640)
    //        Exit Sub
    //      End If

    //      'is there room at the end of this file?
    //      If(i > 0 And LOF(intVolFile) + ResSize <= MAX_VOLSIZE) Or((LOF(intVolFile) + ResSize <= agMaxVol0)) Then
    //        'if so, set pointer to end of the file, and exit
    //        lngCurrentVol = i
    //        lngCurrentLoc = LOF(intVolFile)
    //        Exit Sub
    //      End If
    //      'close the file, and try next
    //      Close intVolFile
    //    Next i

    //    'if no volume found, we've got a problem...
    //    'raise error!
    //    'also, compiler should check for this error, as it is fatal
    //    On Error GoTo 0: Err.Raise vbObjectError + 593, "VolManager.ValidateVolAndLoc", LoadResString(593)
    //    Exit Sub
    //  End If
    //End Sub
  }
}
