namespace WinAGI
{
  internal class AGICel
  {
    public byte Height { get; internal set; }
    public byte TransColor { get; internal set; }
    public byte[,] AllCelData { get; internal set; }
    public byte Width { get; internal set; }

   void tmpCEL()
    {
      /*
Option Explicit
'local variable(s) to hold property Value(s)

Private mWidth As Byte

Private mHeight As Byte

Private mTransColor As AGIColors
Private mCelData() As AGIColors


Private mIndex As Byte


Private blnCelDataSet As Boolean
Private mCelChanged As Boolean

'variables used for low level graphics handling


Private lngCelDIBSec As Long

Private lngOldCelDC As Long

Private lngCelDC As Long
'mSetMirror is true if cel is supposed to show the mirror
Private mSetMirror As Boolean
'mMirror is true if the cel IS showing the mirror
Private mMirrored As Boolean

'other

Private strErrSource As String
Private mParent As AGIView

Public Property Get AllCelData() As AGIColors()
  'returns the entire array of cel data
  'flips the data if the cel is mirrored, and
  'not the primary loop
  
  Dim tmpData() As AGIColors
  Dim i As Long, j As Long
  
  If mSetMirror Then
    'need to flip the data
    ReDim tmpData(UBound(mCelData, 1), UBound(mCelData, 2))
    For i = 0 To mWidth - 1
      For j = 0 To mHeight - 1
        'copy backwards
        tmpData(mWidth - 1 - i, j) = mCelData(i, j)
      Next j
    Next i
    
    'return temp data
    AllCelData = tmpData
  Else
    'fine to return as is
    AllCelData = mCelData
  End If
End Property

Public Property Let CelData(ByVal xPos As Byte, ByVal yPos As Byte, ByVal NewPixel As AGIColors)
  'set the cel data for this position
  
  On Error GoTo ErrHandler
  
  'verify within bounds
  If xPos > mWidth - 1 Then
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Property
  End If
  
  If yPos > mHeight - 1 Then
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Property
  End If
  
  'if cel is in mirror state
  If mSetMirror Then
    'reverse x direction
    mCelData(mWidth - 1 - xPos, yPos) = NewPixel
  Else
    'write pixel Value
    mCelData(xPos, yPos) = NewPixel
  End If
  
  'note change
  mCelChanged = True
  
  'if there is a parent object
  If Not (mParent Is Nothing) Then
    'set dirty flag
    mParent.IsDirty = True
  End If
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property

Public Property Get CelData(ByVal xPos As Byte, ByVal yPos As Byte) As AGIColors
  'returns the cel data for the pixel at xPos, yPos
  
  On Error GoTo ErrHandler
  
  'verify within bounds
  If xPos > mWidth - 1 Then
    On Error GoTo 0
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Property
  End If
  
  If yPos > mHeight - 1 Then
    On Error GoTo 0
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Property
  End If
  
  'if cel is in mirror state
  If mSetMirror Then
    'reverse x direction
    CelData = mCelData(mWidth - 1 - xPos, yPos)
  Else
    'get pixel Value
    CelData = mCelData(xPos, yPos)
  End If
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property


Private Sub ClearBMP()

  On Error GoTo ErrHandler
  
  Dim rtn As Long

  'need to unload/reset the bitmap objects
  rtn = SelectObject(lngCelDC, lngOldCelDC)
  rtn = DeleteDC(lngCelDC)
  rtn = DeleteObject(lngCelDIBSec)
  
  'set flag
  blnCelDataSet = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Property Get CelBMP() As Long
  'returns a device context to the cel bitmap
  'only called externally
  Dim biCel As BitmapInfo
  Dim lngCelAddr As Long
  Dim rtn As Long
  Dim i As Long, j As Long
  Dim lngCelData() As Long
  
  On Error GoTo ErrHandler
  
  'if cel bitmap is already assigned
  If blnCelDataSet Then
    'if cel bitmap is in correct state AND not changed
    If (mSetMirror == mMirrored) And (Not mCelChanged) Then
      'exit; cel bitmap is correct
      CelBMP = lngCelDC
      Exit Property
    End If
    
    'clear bitmap
    ClearBMP
  End If
  
  'set cel mirror state to desired Value (as passed by mSetMirror)
  mMirrored = mSetMirror
'  'clear flip flag
'  mSetMirror = False
  
  'build array of long color data from AGI color array
  ReDim lngCelData(mWidth - 1, mHeight - 1)
  
  For i = 0 To mWidth - 1
    For j = 0 To mHeight - 1
      'if showing mirrored cel
      If mMirrored Then
        'set cel data backwards
        lngCelData(mWidth - i - 1, j) = lngEGARevCol(mCelData(i, j))
      Else
        'set cel data forwards
        lngCelData(i, j) = lngEGARevCol(mCelData(i, j))
      End If
    Next j
  Next i
  
  'initialize the DIBSection header
  With biCel.bmiHeader
    .biSize = 40
    .biWidth = CLng(mWidth)
    .biHeight = CLng(-mHeight)
    .biPlanes = 1
    .biBitCount = 32
    .biCompression = BI_RGB
    .biSizeImage = CLng(mWidth) * CLng(mHeight)
  End With

  'create compatible DC
  lngCelDC = CreateCompatibleDC(0)
  If lngCelDC == 0 Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 607, strErrSource, LoadResString(607)
    Exit Sub
  End If
  
  'get handle to dibsection bitmap
  lngCelDIBSec = CreateDIBSection(lngCelDC, biCel, DIB_RGB_COLORS, lngCelAddr, 0, 0)
  If lngCelDIBSec == 0 Then
    'error
    rtn = DeleteDC(lngCelDC)
    On Error GoTo 0: Err.Raise vbObjectError + 607, strErrSource, LoadResString(607)
    Exit Sub
  End If
  
  'save old device context, and select this object into the dibsection
  lngOldCelDC = SelectObject(lngCelDC, lngCelDIBSec)
  If lngOldCelDC == 0 Then
    'error
    rtn = DeleteDC(lngCelDC)
    rtn = DeleteObject(lngCelDIBSec)
    On Error GoTo 0: Err.Raise vbObjectError + 607, strErrSource, LoadResString(607)
    Exit Sub
  End If
  
  'copy bitmap data into dibsection bitmap
  CopyMemory ByVal (lngCelAddr), lngCelData(0, 0), biCel.bmiHeader.biSizeImage * 4&
  
  'return the cel dc
  CelBMP = lngCelDC
  'set flag
  blnCelDataSet = True
  'clear change flag
  mCelChanged = False
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property


Public Sub CopyCel(SourceCel As AGICel)
  'copies the source cel into this cel
  Dim i As Long, j As Long
  Dim tmpColor As AGIColors
  
  mWidth = SourceCel.Width
  mHeight = SourceCel.Height
  mTransColor = SourceCel.TransColor

  AllCelData = SourceCel.AllCelData
  
  'if this cel is supposed to be mirrored
  If mSetMirror Then
    'need to transpose data
    For i = 0 To mWidth \ 2 - 1
      For j = 0 To mHeight - 1
        'swap horizontally
        tmpColor = mCelData(mWidth - 1 - i, j)
        mCelData(mWidth - 1 - i, j) = mCelData(i, j)
        mCelData(i, j) = tmpColor
      Next j
    Next i
  End If
  
  'if there is a parent object
  If Not (mParent Is Nothing) Then
    mParent.IsDirty = True
  End If
  'note change
  mCelChanged = True
End Sub

Friend Sub FlipCel()
  'this is called to flip cel data
  'to support loop changes
  'when a mirrored pair has its secondary (the
  'loop with the negative mirror pair) either
  'deleted, unmirrored, or set to another mirror
  'the cels original configuration stays correct
  
  'if the primary loop is deleted, unmirrored, or set
  'to another mirror, then the cels need to be flipped
  'so the remaining secondary cel will get the data
  'in the correct format
  
  Dim i As Long, j As Long
  Dim tmpCelData() As AGIColors
  
  ReDim tmpCelData(mWidth - 1, mHeight - 1)
  
  For i = 0 To mWidth - 1
    For j = 0 To mHeight - 1
      tmpCelData(mWidth - 1 - i, j) = mCelData(i, j)
    Next j
  Next i
  
  mCelData = tmpCelData
  
  'note change
  mCelChanged = True
  'if there is a parent object
  If Not (mParent Is Nothing) Then
    'set dirty flag
    mParent.IsDirty = True
  End If
End Sub


Public Property Let Height(ByVal NewHeight As Byte)
  'adjusts height of cel
  
  Dim i As Long, j As Long
  
  On Error GoTo ErrHandler
  
  'must be non-zero
  If NewHeight == 0 Then
    On Error GoTo 0: Err.Raise vbObjectError + 532, strErrSource, LoadResString(532)
    Exit Property
  End If

  If NewHeight > MAX_CEL_HEIGHT Then
    On Error GoTo 0: Err.Raise vbObjectError + 532, strErrSource, LoadResString(532)
    Exit Property
  End If

  'if changed
  If mHeight <> NewHeight Then
    'adjust array size
    '(since last dimension -height- is
    'being changed, 'ReDim Preserve' works here)
    ReDim Preserve mCelData(mWidth - 1, NewHeight - 1)
    'if adding to height
    If NewHeight > mHeight Then
      'set new rows to transparent color
      For i = 0 To mWidth - 1
        For j = mHeight To NewHeight - 1
          mCelData(i, j) = mTransColor
        Next j
      Next i
    End If
    'set new height
    mHeight = NewHeight
    'if there is a parent object
    If Not (mParent Is Nothing) Then
      'set dirty flag
      mParent.IsDirty = True
    End If
    'note change
    mCelChanged = True
  End If
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property


Public Sub Clear()
  'this resets the cel to a one pixel cel,
  'with no data, and black as transcolor
  
  mHeight = 1
  mWidth = 1
  mTransColor = agBlack
  ReDim mCelData(0, 0)
  
  'if the cel has a bitmap set,
  If blnCelDataSet Then
    ClearBMP
    'set flag indicating no bitmap
    blnCelDataSet = False
  End If
  
  'if there is a parent object
  If Not (mParent Is Nothing) Then
    mParent.IsDirty = True
  End If
  mCelChanged = True
End Sub



Public Property Get Index() As Byte
  Index = mIndex
End Property

Friend Property Let Index(ByVal bytNew As Byte)
  'sets the index number for this cel
  mIndex = bytNew
  
End Property

Public Property Let AllCelData(NewCelData() As AGIColors)
  'this method allows the entire cel data
  'to be set as an array
  
  Dim i As Long, j As Long
  
  On Error Resume Next
  
  'first, validate array does not have more than two dimensions
  i = UBound(NewCelData, 3)
  If Err.Number == 0 Then
    'invalid data
    On Error GoTo 0: Err.Raise vbObjectError + 614, strErrSource, LoadResString(614)
    Exit Property
  End If
  
  On Error GoTo ErrHandler
  Err.Clear
  
  'validate dimensions match height/width
  If UBound(NewCelData, 1) <> mWidth - 1 Then
    'invalid data
    On Error GoTo 0: Err.Raise vbObjectError + 614, strErrSource, LoadResString(614)
    Exit Property
  End If
  If UBound(NewCelData, 2) <> mHeight - 1 Then
    'invalid data
    On Error GoTo 0: Err.Raise vbObjectError + 614, strErrSource, LoadResString(614)
    Exit Property
  End If
  
  'set the celdata
  mCelData() = NewCelData()
  
  'if there is a parent object
  If Not (mParent Is Nothing) Then
    'set dirty flag
    mParent.IsDirty = True
  End If
  'note change
  
  mCelChanged = True
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property


Public Property Let Width(ByVal NewWidth As Byte)
  'adjusts width of cel
  
  Dim i As Long, j As Long
  Dim tmpData() As Byte
  
  On Error GoTo ErrHandler
  
  'width must be non zero
  If NewWidth == 0 Then
    On Error GoTo 0: Err.Raise vbObjectError + 533, strErrSource, LoadResString(533)
    Exit Property
  End If

  'width must not exceed max Value
  If NewWidth > MAX_CEL_WIDTH Then
    On Error GoTo 0: Err.Raise vbObjectError + 533, strErrSource, LoadResString(533)
    Exit Property
  End If

  'if changed,
  If mWidth <> NewWidth Then
    'since first dimension -width- is being adjusted,
    'can't use 'ReDim Preserve'- need to use temporary
    'array to facilitate copying
    ReDim tmpData(mWidth - 1, mHeight - 1)
    For i = 0 To mWidth - 1
      For j = 0 To mHeight - 1
        tmpData(i, j) = mCelData(i, j)
      Next j
    Next i
    
    'adjust array size
    ReDim mCelData(NewWidth - 1, mHeight - 1)
    'copy data from temp array
    For i = 0 To NewWidth - 1
      For j = 0 To mHeight - 1
        'if past oldwidth
        If i >= mWidth Then
          'add a transparent color pixel
          mCelData(i, j) = mTransColor
        Else
          'add pixel from temp array
          mCelData(i, j) = tmpData(i, j)
        End If
      Next j
    Next i
    'set new width
    mWidth = NewWidth
    'if there is a parent object
    If Not (mParent Is Nothing) Then
      'set dirty flag
      mParent.IsDirty = True
    End If
    'note change
    mCelChanged = True
  End If
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property



Public Property Get Height() As Byte
  'return height
  Height = mHeight
End Property

Friend Sub SetMirror(ByVal blnNew As Boolean)
  mSetMirror = blnNew
End Sub


Friend Sub SetParent(NewParent As AGIView)
  'sets the parent view for this cel
  Set mParent = NewParent
End Sub
Public Property Get Width() As Byte
  'return height
  Width = mWidth
End Property
Public Property Let TransColor(ByVal bytCol As AGIColors)
  'ensure a valid range is passed,
  If bytCol < 0 Or bytCol >= 16 Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 556, strErrSource, LoadResString(556)
    Exit Property
  End If
  
  'if changed,
  If bytCol <> mTransColor Then
    'change it
    mTransColor = bytCol
    '(don't need to set celchanged flag
    'since changing transcolor does not change bitmap image)
    
    'if there is a parent object
    If Not (mParent Is Nothing) Then
      'set dirty flag
      mParent.IsDirty = True
    End If
  End If
  
End Property




Public Property Get TransColor() As AGIColors
    TransColor = mTransColor
End Property



Private Sub Class_Initialize()
  
  strErrSource = "WINAGI.AGICel"
  ReDim mCelData(0, 0)
  mWidth = 1
  mHeight = 1
End Sub
Private Sub Class_Terminate()

  'if the cel has a bitmap set,
  If blnCelDataSet Then
    ClearBMP
  End If
  Set mParent = Nothing
End Sub


      */
    }
  }
}