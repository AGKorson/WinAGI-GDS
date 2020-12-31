using System;
using System.ComponentModel;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;

namespace WinAGI
{
  public class AGIView : AGIResource
  {
    public AGIView() : base(AGIResType.rtView, "NewView")
    {
      //initialize
      //attach events
      base.PropertyChanged += ResPropChange;
      strErrSource = "WinAGI.View";
      //set default resource data
      Data = new RData(0);// ();
    }
    internal void InGameInit(byte ResNum, sbyte VOL, int Loc)
    {
      //this internal function adds this resource to a game, setting its resource 
      //location properties, and reads properties from the wag file

      //set up base resource
      base.InitInGame(ResNum, VOL, Loc);

      //if first time loading this game, there will be nothing in the propertyfile
      ID = ReadSettingString(agGameProps, "View" + ResNum, "ID", "");
      if (ID.Length == 0)
      {
        //no properties to load; save default ID
        ID = "View" + ResNum;
        WriteGameSetting("Logic" + ResNum, "ID", ID, "Views");
        //load resource to get size
        Load();
        WriteGameSetting("View" + ResNum, "Size", Size.ToString());
        Unload();
      }
      else
      {
        //get description, size and other properties from wag file
        mDescription = ReadSettingString(agGameProps, "View" + ResNum, "Description", "");
        Size = ReadSettingLong(agGameProps, "View" + ResNum, "Size", -1);
      }
    }
    private void ResPropChange(object sender, AGIResPropChangedEventArgs e)
    {
      ////let's do a test
      //// increment number everytime data changes
      //Number++;
    }
    internal void SetView(AGIView newView)
    {
      throw new NotImplementedException();
    }
    void tmpView()
    {
      /*
Option Explicit

Implements AGIResource

Private WithEvents agRes As AGIResource
Private mResID As String
Private mDescription As String
Private mIsDirty As Boolean
Private mWriteProps As Boolean

'flag to note loops loaded from res data
Private mViewSet As Boolean
Private mLoopCol As AGILoops
Private mViewDesc As String

Private strErrSource As String
Private Sub NewView()
  'add header
  agRes.WriteByte 1, 0
  agRes.WriteByte 1
  'add one loop
  agRes.WriteByte 1
  'no description
  agRes.WriteWord 0
  'loop starts at 7
  agRes.WriteWord 7
  'add one cel
  agRes.WriteByte 1
  'placeholder for cel position
  agRes.WriteWord 3
  'add width
  agRes.WriteByte 1
  'add height
  agRes.WriteByte 1
  'add trans color
  agRes.WriteByte 0
  'add cel data
  agRes.WriteByte 0
End Sub

Friend Sub SetFlags(ByRef IsDirty As Boolean, ByRef ViewSet As Boolean, ByRef WriteProps As Boolean)
  
  IsDirty = mIsDirty
  ViewSet = mViewSet
  WriteProps = mWriteProps
End Sub
Public Sub Clear()
  'resets the view
  'to a single loop with
  'a single view with
  'a height and witdh of 1
  'and transparent color of 0
  'and no description
  
  Dim i As Integer, j As Integer
  
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
  End If
    
  'clear the resource
  agRes.Clear
  'dereference all loops
  For i = Loops.Count - 1 To 1 Step -1
    'dereference all cels
    For j = Loops(i).Cels.Count - 1 To 1 Step -1
      Loops(i).Cels(j).Clear
      Loops(i).Cels.Remove j
    Next j
    Loops.Remove i
  Next i
  
  'clear the remaining cel in the remaining loop
  Loops(0).Cels(0).Clear
  
  'clear description
  mViewDesc = vbNullString
  
  'write data for default view
  NewView
  
  'set dirty flag
  mIsDirty = True
End Sub


Private Sub CompileView()

  Dim tmpRes As AGIResource
  Dim lngLoopLoc() As Long, lngCelLoc() As Long
  Dim i As Long, j As Long
  Dim bytTransCol As Byte
  Dim k As Long, bytCelData() As Byte
  Dim blnMirrorAdded As Boolean
  
  On Error GoTo ErrHandler
  
  Set tmpRes = New AGIResource
  
  With tmpRes
    .NewResource
    
    'write header
    .WriteByte 1, 0
    .WriteByte 1
    'write number of loops
    .WriteByte mLoopCol.Count
    'placeholder for description
    .WriteWord 0
    
    'initialize loop loc array
    ReDim lngLoopLoc(mLoopCol.Count - 1)
    'write place holders for loop positions
    For i = 0 To mLoopCol.Count - 1
      .WriteWord 0
    Next i
    
    'step through all loops to add them
    For i = 0 To mLoopCol.Count - 1
      'if loop is mirrored AND already added
      '(can tell if not added by comparing the mirror loop
      'property against current loop being added)
      If mLoopCol(i).Mirrored Then
        blnMirrorAdded = (mLoopCol(i).MirrorLoop < i)
      Else
        blnMirrorAdded = False
      End If
      
      If blnMirrorAdded Then
        'loop location is same as mirror
        lngLoopLoc(i) = lngLoopLoc(mLoopCol(i).MirrorLoop)
      Else
        'set loop location
        lngLoopLoc(i) = .GetPos
        'write number of cels
        .WriteByte mLoopCol(i).Cels.Count
        'initialize cel loc array
        ReDim lngCelLoc(mLoopCol(i).Cels.Count - 1)
        'write placeholders for cel locations
        For j = 0 To mLoopCol(i).Cels.Count - 1
          .WriteWord 0
        Next j
        'step through all cels to add them
        For j = 0 To mLoopCol(i).Cels.Count - 1
          'save cel loc
          lngCelLoc(j) = .GetPos - lngLoopLoc(i)
          'write width
          .WriteByte mLoopCol(i).Cels(j).Width
          'write height
          .WriteByte mLoopCol(i).Cels(j).Height
          'if loop is mirrored
          If mLoopCol(i).Mirrored Then
            'set bit 7 for mirror flag and include loop number
            'in bits 6-5-4 for transparent color
            bytTransCol = &H80 + i * &H10 + mLoopCol(i).Cels(j).TransColor
          Else
            'just use transparent color
            bytTransCol = mLoopCol(i).Cels(j).TransColor
          End If
          'write transcolor
          .WriteByte bytTransCol
          'get cel data
          bytCelData = CompressCelData(mLoopCol(i).Cels(j), mLoopCol(i).Mirrored)
          'write cel data
          For k = 0 To UBound(bytCelData)
            .WriteByte bytCelData(k)
          Next k
        Next j
        
        'step through cels and add cel loc
        For j = 0 To mLoopCol(i).Cels.Count - 1
          .WriteWord lngCelLoc(j), lngLoopLoc(i) + 1 + 2 * j
        Next j
        'restore pos to end of resource
        .SetPos .Size
      End If
    Next i
    
    'step through loops again to add loop loc
    For i = 0 To mLoopCol.Count - 1
      .WriteWord lngLoopLoc(i), 5 + 2 * i
    Next i
    
    'if there is a view description
    If LenB(mViewDesc) > 0 Then
      'write view description location
      .WriteWord .Size, 3
      'move pointer back to end of resource
      .SetPos .Size
      'write view description
      For i = 1 To Len(mViewDesc)
        .WriteByte Asc(Mid$(mViewDesc, i))
      Next i
      'add terminating null char
      .WriteByte 0
    End If
    
    'assign data to resource
    agRes.AllData = .AllData
  End With
  Set tmpRes = Nothing
  
  'set viewloaded flag
  mViewSet = True
Exit Sub

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  '*'Debug.Assert False
  On Error GoTo 0: Err.Raise vbObjectError + 632, strErrSrc, Replace(LoadResString(632), ARG1, CStr(lngError) & ":" & strError)
End Sub

Friend Property Let FriendID(NewID As String)

  'sets the ID for a resource internally
  'does not validate the ID or set dirty flag
  
  'save ID
  mResID = NewID
End Property

Friend Property Get FriendLoops() As AGILoops
  
  'return the loop collection
  Set FriendLoops = mLoopCol
End Property
Public Sub SetView(CopyView As AGIView)
  'copies view data from CopyView into this view
  
  Dim i As Byte, j As Byte
  Dim xPos As Byte, yPos As Byte
  Dim tmpLoop As AGILoop
  
  On Error GoTo ErrHandler
  
  'copy all items
  With CopyView
    'add WinAGI items
    mResID = .ID
    mDescription = .Description
    
    'copy rest of resource variables
    agRes.SetRes .Resource
    
    'if loaded,
    If .Loaded Then
      'copy resource data
      agRes.AllData = .Resource.AllData
      'copy view desc
      mViewDesc = .ViewDescription
    
      'clear out loop collection by assigning a new one
      Set mLoopCol = New AGILoops
      'set parent for loop collection
      mLoopCol.SetParent Me
      
      'copy any existing loop information
      For i = 1 To .Loops.Count
        'first, create copy of every loop
        Set tmpLoop = mLoopCol.Add(i - 1)
        tmpLoop.CopyLoop .Loops(i - 1)
      Next i
    
      'step through again to set mirrors
      For i = 1 To .Loops.Count
        If .Loops(i - 1).Mirrored Then
          'if this is the primary mirror
          If .Loops(i - 1).MirrorPair > 0 Then
            SetMirror .Loops(i - 1).MirrorLoop, i - 1
          End If
        End If
      Next i
    End If
    
    'copy dirty flag and writeprop flag
    .SetFlags mIsDirty, mViewSet, mWriteProps
  End With
Exit Sub

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  On Error GoTo 0: Err.Raise vbObjectError + 643, strErrSrc, Replace(LoadResString(643), ARG1, CStr(lngError) & ":" & strError)
End Sub
Private Sub ExpandCelData(ByVal StartPos As Long, TempCel As AGICel)
  'this function will expand the RLE data beginning at
  'position StartPos
  'it then passes the expanded data to the cel
  Dim bytWidth As Byte, bytHeight As Byte, bytTransColor As Byte
  Dim bytCelX As Byte, bytCelY As Byte
  Dim bytIn As Byte
  Dim bytChunkColor As Byte, bytChunkCount As Byte
  Dim tmpCelData() As AGIColors
  
  On Error GoTo ErrHandler
  
  bytWidth = TempCel.Width
  bytHeight = TempCel.Height
  bytTransColor = TempCel.TransColor
  
  'reset size of data array
  ReDim tmpCelData(bytWidth - 1, bytHeight - 1)
  
  'set resource to starting position
  agRes.SetPos StartPos
  
  ' extract pixel data
  Do
    bytCelX = 0
    Do
      'read each byte, where lower four bits are number of pixels,
      'and upper four bits are color for these pixels
      bytIn = agRes.ReadByte()
      'skip zero values
      If bytIn > 0 Then
        'extract color
        bytChunkColor = bytIn \ &H10
        bytChunkCount = bytIn Mod &H10
        'add data to bitmap data array
        'now store this color for correct number of pixels
        For bytCelX = bytCelX To bytCelX + bytChunkCount - 1
          tmpCelData(bytCelX, bytCelY) = bytChunkColor
        Next bytCelX
      End If
    Loop Until bytIn = 0
    
    'fill in rest of this line with transparent color, if necessary
    Do Until bytCelX >= bytWidth
      tmpCelData(bytCelX, bytCelY) = bytTransColor
      bytCelX = bytCelX + 1
    Loop
    bytCelY = bytCelY + 1
    
  Loop Until bytCelY >= bytHeight
  
  'pass cel data to the cel
  TempCel.AllCelData = tmpCelData
  
Exit Sub

ErrHandler:
  Err.Clear
  'raise error
  On Error GoTo 0: Err.Raise vbObjectError + 555, strErrSource, LoadResString(555)
  
End Sub

Private Function GetMirrorPair() As Byte
  'this function will generate a unique mirrorpair number
  'that is used to identify a pair of mirrored loops
  'the source loop is positive; the copy is negative
  Dim i As Byte
  
  'start with 1
  GetMirrorPair = 1
  Do
    For i = 0 To mLoopCol.Count - 1
      'if this loop is using this mirror pair
      If GetMirrorPair = Abs(mLoopCol(i).MirrorPair) Then
        'get another number
        Exit For
      End If
    Next i
    
    'if loop was exited normally
    If i = mLoopCol.Count Then
      'use this mirrorpair
      Exit Do
    End If
    'try another
    GetMirrorPair = GetMirrorPair + 1
  Loop
End Function


Friend Property Let IsDirty(ByVal NewState As Boolean)
  
  mIsDirty = NewState
End Property

Friend Property Let WritePropState(ByVal NewWritePropState As Boolean)

  mWriteProps = NewWritePropState
End Property


Friend Sub LoadLoops()
  Dim bytNumLoops As Byte, bytNumCels As Byte
  Dim lngLoopStart(MAX_LOOPS) As Long
  Dim lngCelStart As Long, lngDescLoc As Long
  
  Dim bytLoop As Byte, bytCel As Byte
  Dim tmpLoopNo As Byte, bytInput As Byte
  
  Dim bytMaxW(MAX_LOOPS) As Byte, bytMaxH(MAX_LOOPS) As Byte
  Dim bytWidth As Byte, bytHeight As Byte
  Dim bytTransCol As Byte
  
  On Error GoTo ErrHandler
  
  'clear out loop collection by assigning a new one
  Set mLoopCol = New AGILoops
  'set parent for loop collection
  mLoopCol.SetParent Me
  
  'if empty (as in creating a new view)
  If agRes.Size = 1 Then
    'set flag and exit
    mViewSet = True
    Exit Sub
  End If
  
  'get number of loops and strDescription location
  bytNumLoops = agRes.ReadByte(2)
   
  'get offset to description
  lngDescLoc = agRes.ReadWord()
  
  'if no loops
  If bytNumLoops = 0 Then
    'error - invalid data
    On Error GoTo 0: Err.Raise vbObjectError + 595, strErrSource, LoadResString(595)
    Exit Sub
  End If
  
  'get loop offset data for each loop
  For bytLoop = 0 To bytNumLoops - 1
    'get offset to start of this loop
    lngLoopStart(bytLoop) = agRes.ReadWord()
    'if loop data is past end of resource
    If (lngLoopStart(bytLoop) > agRes.Size) Then
      Unload
      On Error GoTo 0: Err.Raise vbObjectError + 548, strErrSource, LoadResString(548)
      Exit Sub
    End If
  Next bytLoop
  
  'step through all loops
  For bytLoop = 0 To bytNumLoops - 1
    'add the loop
    mLoopCol.Add CByte(bytLoop)
    'loop zero is NEVER mirrored
    If bytLoop > 0 Then
      'for all other loops, check to see if it mirrors an earlier one
      For tmpLoopNo = 0 To bytLoop - 1
        'if the loops have the same starting position,
        If lngLoopStart(bytLoop) = lngLoopStart(tmpLoopNo) Then
          'this loop is a mirror
          On Error Resume Next
          SetMirror bytLoop, tmpLoopNo
          If Err.Number <> 0 Then
            'if error is because source is already mirrored
            'continue without setting mirror; data will be
            'treated as a completely separate loop
            If Err.Number <> vbObjectError + 551 Then
              'pass along the error
              lngError = Err.Number
              strError = Err.Description
              Unload
              'error
              On Error GoTo 0: Err.Raise lngError, strErrSource, strError
              Exit Sub
            End If
          End If
          On Error GoTo ErrHandler
          Exit For
        End If
      Next tmpLoopNo
    End If
      
    'if loop not mirrored,
    If Not mLoopCol(CByte(bytLoop)).Mirrored Then
      'point to start of this loop
      agRes.SetPos CLng(lngLoopStart(bytLoop))
      'read number of cels
      bytNumCels = agRes.ReadByte()
      
      'if error
      If (bytNumCels > MAX_CELS) Then
        Unload
        On Error GoTo 0: Err.Raise vbObjectError + 552, strErrSource, Replace(LoadResString(552), ARG1, CStr(bytLoop))
        Exit Sub
      End If
        
      'step through all cels in this loop
      For bytCel = 0 To (bytNumCels - 1)
        'read starting position
        lngCelStart = agRes.ReadWord(lngLoopStart(bytLoop) + 2 * bytCel + 1) + lngLoopStart(bytLoop)
        If (lngCelStart > agRes.Size) Then
          Unload
          On Error GoTo 0: Err.Raise vbObjectError + 553, strErrSource, LoadResString(553)
          Exit Sub
        End If
          
        'get height/width
        bytWidth = agRes.ReadByte(lngCelStart)
        bytHeight = agRes.ReadByte()
        'get transparency color for this cel
        bytTransCol = agRes.ReadByte()
        bytTransCol = bytTransCol Mod &H10
        
        'add the cel
        mLoopCol(CByte(bytLoop)).Cels.Add CByte(bytCel), bytWidth, bytHeight, agColor(bytTransCol)
        'extract bitmap data from RLE data
        ExpandCelData lngCelStart + 3, mLoopCol(CByte(bytLoop)).Cels(bytCel)
      Next bytCel
    End If
  Next bytLoop
  'clear the description string
  mViewDesc = vbNullString
  
  'if there is a description for this view,
  If lngDescLoc > 0 Then
    'ensure it can be loaded
    If lngDescLoc < agRes.Size - 1 Then
      'set resource pointer to beginning of description string
      agRes.SetPos lngDescLoc
      Do
        'get character
        bytInput = agRes.ReadByte()
        'if not zero, and string not yet up to 255 characters,
        If (bytInput > 0) And (Len(mViewDesc) < 255) Then
          'add the character
          mViewDesc = mViewDesc & Chr$(bytInput)
        End If
      'stop if zero reached, end of resource reached, or 255 characters read
      Loop Until agRes.EORes Or (bytInput = 0) Or (Len(mViewDesc) >= 255)
    Else
      Unload
      'error? can't load strDescription?
      On Error GoTo 0: Err.Raise vbObjectError + 513, strErrSource, LoadResString(513)
      Exit Sub
    End If
  End If
  
  'set flag indicating view matches resource data
  mViewSet = True
  'MUST be clean, since loaded from resource data
  mIsDirty = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Property Get Number() As Byte
  Number = agRes.Number
End Property




Public Property Get Description() As String
  Description = mDescription
End Property

Public Property Let Description(ByVal NewDescription As String)

  'limit description to 1K
  NewDescription = Left$(NewDescription, 1024)
  
  'if changing
  If NewDescription <> mDescription Then
    mDescription = NewDescription
    
    If agRes.InGame Then
      WriteGameSetting "View" & CStr(agRes.Number), "Description", mDescription, "Views"
    End If
  End If
End Property


Public Property Get ID() As String
  
    ID = mResID
End Property
Public Property Let ID(NewID As String)
  'sets the ID for a resource;
  'resource IDs must be unique to each resource type
  'max length of ID is 64 characters
  'min of 1 character
  
  Dim tmpView As AGIView
  
  'why the h*** do I need to reset error here?
  Err.Clear
  On Error GoTo ErrHandler
  
  'validate length
  If LenB(NewID) = 0 Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 667, strErrSource, LoadResString(667)
    Exit Property
  ElseIf Len(NewID) > 64 Then
    NewID = Left$(NewID, 64)
  End If
  
  'if changing,
  If NewID <> mResID Then
    'if in a game,
    If agRes.InGame Then
      'step through other resources
      For Each tmpView In agViews
        'if resource IDs are same
        If tmpView.ID = NewID Then
          'if not the same view
          If tmpView.Number <> agRes.Number Then
            'error
            On Error GoTo 0: Err.Raise vbObjectError + 623, strErrSource, LoadResString(623)
            Exit Property
          End If
        End If
      Next
    End If
    
    'save ID
    mResID = NewID
    If agRes.InGame Then
      WriteGameSetting "View" & CStr(agRes.Number), "ID", mResID, "Views"
    End If
    
    'reset compiler list of ids
    blnSetIDs = False
  End If
Exit Property

ErrHandler:
  lngError = Err.Number
  strErrSrc = Err.Source
  strError = Err.Description
  
  '*'Debug.Assert False
  On Error GoTo 0: Err.Raise vbObjectError + 686, strErrSrc, Replace(Replace(LoadResString(686), ARG1, "Logic"), ARG2, strError)
End Property




Public Sub Export(ExportFile As String, Optional ByVal ResetDirty As Boolean = True)
  On Error GoTo ErrHandler
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  'if view is dirty
  If mIsDirty Then
    'need to recompile
    CompileView
  End If
    
  agRes.Export ExportFile
  
  'if not in a game,
  If Not agRes.InGame Then
    'ID always tracks the resfile name
    mResID = JustFileName(ExportFile)
    If Len(mResID) > 64 Then
      mResID = Left$(mResID, 64)
    End If
    
    If ResetDirty Then
      'clear dirty flag
      mIsDirty = False
    End If
  End If
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub


Public Sub Import(ImportFile As String)
  'imports a view resource
  
  On Error Resume Next
  
  'import the resource
  agRes.Import ImportFile
  
  'if there was an error,
  If Err.Number <> 0 Then
    'pass along error
    lngError = Err.Number
    strErrSrc = Err.Source
    strError = Err.Description
    
    Unload
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    Exit Sub
  End If
  
  'set ID
  mResID = JustFileName(ImportFile)
  If Len(mResID) > 64 Then
    mResID = Left$(mResID, 64)
  End If
  
  If Err.Number <> 0 Then
    'pass along error (by exiting without clearing it)
    lngError = Err.Number
    strErrSrc = Err.Source
    strError = Err.Description
    
    Unload
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    Exit Sub
  End If
  
  'reset dirty flag
  mIsDirty = False
  mWriteProps = False
  
  'reset loadloop
  mViewSet = False
End Sub



Public Property Get IsDirty() As Boolean
    
  'if resource is dirty, or (prop values need writing AND in game)
  IsDirty = (mIsDirty Or (mWriteProps And agRes.InGame))
End Property


Public Sub Load()
  
  On Error Resume Next
  
  'if not ingame, the resource should already be loaded
  If Not agRes.InGame Then
    '*'Debug.Assert agRes.Loaded
  End If
  
  'if resource not loaded yet,
  If Not agRes.Loaded Then
    'load resource
    agRes.Load
  End If
  
  If Err.Number <> 0 Then
    lngError = Err.Number
    strErrSrc = Err.Source
    strError = Err.Description
    
    Unload
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    Exit Sub
  End If
  
  '*'Debug.Assert Not mViewSet
  LoadLoops
  
  If Err.Number <> 0 Then
    lngError = Err.Number
    strErrSrc = Err.Source
    strError = Err.Description
    
    Unload
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    Exit Sub
  End If
  
  'clear dirty flags
  mIsDirty = False
  mWriteProps = False
End Sub


Public Sub Unload()

  'unload resource
  
  On Error GoTo ErrHandler
  
  agRes.Unload
  mIsDirty = False
  
  
  'clear out loop collection by assigning a new one
  Set mLoopCol = New AGILoops
  
  mViewSet = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Property Get Resource() As AGIResource
  Set Resource = agRes
End Property



Public Sub Save()
  'saves the view
  
  On Error Resume Next
  
  'if properties need to be written
  If mWriteProps And agRes.InGame Then
    'save ID and description to ID file
    WriteGameSetting "View" & CStr(agRes.Number), "ID", mResID, "Views"
    WriteGameSetting "View" & CStr(agRes.Number), "Description", mDescription
    mWriteProps = False
  End If
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  'if dirty,
  If mIsDirty Then
    'rebuild Resource
    CompileView
    
    'if any errors
    If Err.Number <> 0 Then
      'pass error along
      lngError = Err.Number
      strError = Err.Description
      strErrSrc = Err.Source
      
      On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
      Exit Sub
    End If
    
    'use the resource save method
    agRes.Save
    'if any errors
    If Err.Number <> 0 Then
      'pass error along
      lngError = Err.Number
      strError = Err.Description
      strErrSrc = Err.Source
      
      On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
      Exit Sub
    End If
    
    WriteGameSetting "View" & CStr(agRes.Number), "Size", agRes.Size, "Views"
    
    'reset flag
    mIsDirty = False
  End If
End Sub


Public Property Get Loops() As AGILoops
  
  On Error Resume Next
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  'if view not set,
  If Not mViewSet Then
    'load loops first
    LoadLoops
  End If
  'return any errors
  If Err.Number <> 0 Then
    'pass error along
    lngError = Err.Number
    strError = Err.Description
    strErrSrc = Err.Source
    
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    Exit Property
  End If
  
  'return the loop collection
  Set Loops = mLoopCol
End Property

Public Property Get ViewDescription() As String

  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  ViewDescription = mViewDesc
End Property




Public Property Let ViewDescription(NewDescription As String)
  
  'if changing,
  If mViewDesc <> NewDescription Then
    mViewDesc = NewDescription
    mIsDirty = True
  End If
End Property


Public Sub SetMirror(ByVal TargetLoop As Byte, ByVal SourceLoop As Byte)
  'TargetLoop is the loop that will be a mirror of
  'SourceLoop; the cels collection in TargetLoop will be lost
  'once the mirror property is set
  
  Dim i As Integer
  
  On Error GoTo ErrHandler
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  'the source loop must already exist
  '(must be less than or equal to max number of loops)
  If SourceLoop > mLoopCol.Count - 1 Then
    'error - loop must exist
    On Error GoTo 0: Err.Raise vbObjectError + 539, strErrSource, LoadResString(539)
    Exit Sub
  End If
  
  'the source loop and the target loop must be less than 8
  If SourceLoop >= 8 Or TargetLoop >= 8 Then
    'error - loop must exist
    On Error GoTo 0: Err.Raise vbObjectError + 539, strErrSource, LoadResString(539)
    Exit Sub
  End If
  
  'mirror source and target can't be the same
  If SourceLoop = TargetLoop Then
    'error - can't be a mirror of itself
    On Error GoTo 0: Err.Raise vbObjectError + 540, strErrSource, LoadResString(540)
    Exit Sub
  End If
  
  'this loop can't be already mirrored
  If mLoopCol(TargetLoop).Mirrored Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 550, strErrSource, LoadResString(550)
    Exit Sub
  End If
  
  'the mirror loop can't already have a mirror
  If mLoopCol(SourceLoop).Mirrored Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 551, strErrSource, Replace(LoadResString(551), ARG1, CStr(SourceLoop))
    Exit Sub
  End If
  
  'get a new mirror pair number
  i = GetMirrorPair()
  
  'set the mirror loop hasmirror property
  mLoopCol(SourceLoop).MirrorPair = i
  'set the mirror loop mirrorloop property
  mLoopCol(TargetLoop).MirrorPair = -i
  Set mLoopCol(TargetLoop).Cels = mLoopCol(SourceLoop).Cels
  'set dirty flag
  mIsDirty = True
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub



Private Property Get AGIResource_AllData() As Byte()
  AGIResource_AllData = agRes.AllData
End Property

Private Sub AGIResource_Clear()
  Clear
End Sub


Private Property Let AGIResource_Data(ByVal Pos As Long, ByVal NewData As Byte)
  agRes.Data(Pos) = NewData

End Property

Private Property Get AGIResource_Data(ByVal Pos As Long) As Byte
  On Error GoTo ErrHandler
  
  AGIResource_Data = agRes.Data(Pos)
Exit Property

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Property


Private Property Get AGIResource_EORes() As Boolean
  AGIResource_EORes = agRes.EORes
End Property


Private Sub AGIResource_Export(ExportFile As String)
  Export ExportFile
End Sub



Private Function AGIResource_GetPos() As Long
  AGIResource_GetPos = agRes.GetPos
End Function


Private Property Get AGIResource_InGame() As Boolean
  AGIResource_InGame = agRes.InGame
End Property

Private Sub AGIResource_InsertData(NewData As Variant, Optional ByVal InsertPos As Long = -1&)

  On Error GoTo ErrHandler
  
  agRes.InsertData NewData, InsertPos
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub


Private Property Get AGIResource_Loaded() As Boolean
  AGIResource_Loaded = agRes.Loaded
End Property



Private Property Get AGIResource_Loc() As Long
  AGIResource_Loc = agRes.Loc

End Property


Private Sub AGIResource_NewResource(Optional ByVal Reset As Boolean = False)
  On Error GoTo ErrHandler
  
  agRes.NewResource Reset
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub

Private Property Get AGIResource_Number() As Byte
  AGIResource_Number = agRes.Number
End Property



Friend Sub Init(ByVal ResNum As Byte, ByVal VOL As Long, ByVal Loc As Long)
  
  On Error GoTo ErrHandler
  
  'initialize resource object
  agRes.Init ResNum, VOL, Loc
  'set parent for the loop collection
  mLoopCol.SetParent Me
  
  'if first time loading this game, there will be nothing in the propertyfile
  mResID = ReadSettingString(agGameProps, "View" & CStr(ResNum), "ID", "")
  If Len(mResID) = 0 Then
    'no properties to load; save default ID
    mResID = "View" & CStr(ResNum)
    'load resource to get size
    agRes.Load
    'save ID and size to WAG file
    WriteGameSetting "View" & CStr(ResNum), "ID", mResID, "Views"
    WriteGameSetting "View" & CStr(ResNum), "Size", agRes.Size
    'unload when done
    agRes.Unload
  Else
    'get ID and description and other properties from wag file
    mDescription = ReadSettingString(agGameProps, "View" & CStr(ResNum), "Description", "")
    agRes.Size = ReadSettingLong(agGameProps, "View" & CStr(ResNum), "Size", -1)
  End If
Exit Sub

ErrHandler:
  
  'pass along the error
  Err.Raise Err.Number, Err.Source, Err.Description, Err.HelpFile, Err.HelpContext
End Sub


Public Property Get Loaded() As Boolean
  
  Loaded = agRes.Loaded
End Property

Private Function AGIResource_ReadByte(Optional ByVal lngPos As Long = MAX_RES_SIZE + 1) As Byte
  On Error GoTo ErrHandler
  
  AGIResource_ReadByte = agRes.ReadByte(lngPos)
Exit Function

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Function



Private Function AGIResource_ReadWord(Optional ByVal lngPos As Long = -1&, Optional ByVal blnMSLS As Boolean = False) As Long
  On Error GoTo ErrHandler
  
  AGIResource_ReadWord = agRes.ReadWord(lngPos)
Exit Function

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Function


Private Sub AGIResource_RemoveData(ByVal RemovePos As Long, Optional ByVal RemoveCount As Long = 1)

  On Error GoTo ErrHandler
  
  agRes.RemoveData RemovePos, RemoveCount
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub


Private Property Get AGIResource_ResFile() As String
  AGIResource_ResFile = agRes.ResFile
End Property



Private Property Get AGIResource_ResType() As AGIResType
  AGIResource_ResType = agRes.ResType
End Property



Private Sub AGIResource_SetData(NewData() As Byte)

  agRes.SetData NewData
End Sub

Private Sub AGIResource_SetPos(ByVal lngPos As Long)
  On Error GoTo ErrHandler
  
  agRes.SetPos lngPos
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub



Private Property Get AGIResource_Size() As Long
  AGIResource_Size = agRes.Size
End Property



Private Property Get AGIResource_SizeInVOL() As Long
  AGIResource_SizeInVOL = agRes.SizeInVol

End Property


Private Property Get AGIResource_Volume() As Long
  AGIResource_Volume = agRes.Volume
End Property



Private Sub agRes_Change()
  'set flag to indicate data has changed
  mViewSet = False
  'set dirtyflag
  mIsDirty = True
End Sub

Private Sub Class_Initialize()
  
  strErrSource = "WinAGI.agiView"
  Set agRes = New AGIResource
  agRes.SetType rtView
  
  'create default view with one pixel in one cel in one loop
  agRes.NewResource
  NewView
  
  'set loopcol (need to do this now
  '             so new views can be created)
  Set mLoopCol = New AGILoops
  
  mResID = "NewView"
End Sub

Private Sub Class_Terminate()
  
  'if loaded,
  If agRes.Loaded Then
    Unload
  End If
  Set agRes = Nothing
  
  If Not mLoopCol Is Nothing Then
    'if loops set
    If mLoopCol.Count > 0 Then
      'should never terminate without loops being cleared first
      Dim i As Long
      For i = mLoopCol.Count - 1 To 0 Step -1
        mLoopCol.Remove i
      Next i
    End If
    Set mLoopCol = Nothing
  End If
End Sub
      */
    }
  }
}