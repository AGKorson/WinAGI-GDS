using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  class AGILoop
  {
    void tmpLoop()
    {
      /*
Option Explicit

'local variable(s) to hold property Value(s)
'Private mCels As AGICels

Private mCelCol As AGICels

Private mMirrorPair As Integer


Private mIndex As Byte
Private mParent As AGIView

'other

Dim strErrSource As String

  

Friend Property Set Cels(NewCelCol As AGICels)
  'sets the cels collection
  Dim i As Integer
  
  On Error GoTo ErrHandler
  
  Set mCelCol = NewCelCol
  
  'if mirrored
  If mMirrorPair <> 0 Then
    'find mirror pair
    For i = 0 To mParent.FriendLoops.Count - 1
      'if sum of mirror pairs is zero
      If mParent.FriendLoops(CByte(i)).MirrorPair + mMirrorPair == 0 Then
        'is the cels collection already set to this object?
        If mParent.FriendLoops(CByte(i)).Cels Is NewCelCol Then
          'need to exit to avoid recursion
          Exit Property
        End If
        'set the mirrored loops cels
        Set mParent.FriendLoops(CByte(i)).Cels = NewCelCol
        Exit Property
      End If
    Next i
  End If
Exit Property

ErrHandler:
  '*'Debug.Assert False
  'should never get an error
  Resume Next
End Property

Public Sub CopyLoop(SourceLoop As AGILoop)
  'copies the source loop into this loop
  
  Dim i As Byte, bCelCount As Byte
  Dim tmpCels As AGICels
  
  'if this is mirrored, and the primary loop
  If Sgn(mMirrorPair) > 0 Then
    'call unmirror for the secondary loop
    'so it will get a correct copy of cels
    mParent.FriendLoops(MirrorLoop).UnMirror
  ElseIf Sgn(mMirrorPair) < 0 Then
    'this is a secondary loop;
    'only need to reset mirror status
    'because copy function will create new cel collection
    mParent.FriendLoops(MirrorLoop).MirrorPair = 0
    mMirrorPair = 0
  End If
  
  'now copy source loop cels
  'create temporary collection of cels
  Set tmpCels = New AGICels
  tmpCels.SetParent mParent
  
  'copy cels from current cel collection
  bCelCount = SourceLoop.Cels.Count
  For i = 0 To bCelCount - 1
    tmpCels.Add i, SourceLoop.Cels(i).Width, SourceLoop.Cels(i).Height, SourceLoop.Cels(i).TransColor
    'add data
    tmpCels(i).AllCelData = SourceLoop.Cels(i).AllCelData
  Next i
  
  'dereference cel collection
  Set mCelCol = Nothing
  'set it to new cel col
  Set mCelCol = tmpCels
  'if there is a parent object
  If Not (mParent Is Nothing) Then
    'set dirty flag
    mParent.IsDirty = True
  End If
Exit Sub

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  On Error GoTo 0: Err.Raise vbObjectError + 564, strErrSrc, Replace(LoadResString(564), ARG1, CStr(lngError) & ":" & strError)
End Sub


Friend Property Let Index(NewIndex As Byte)
  mIndex = NewIndex
End Property


Public Property Get Index() As Byte
  'return the loop number of this loop
  Index = mIndex
End Property



Public Property Get Mirrored() As Boolean
  'if this loop is part of a mirror pair
  'then it is mirrored
  Mirrored = (mMirrorPair <> 0)
End Property


Public Property Get Cels() As AGICels
  
  'set mirror flag
  mCelCol.SetMirror (Sgn(mMirrorPair) = -1)
  
  'return the cels collection
  Set Cels = mCelCol
End Property



Public Property Get MirrorLoop() As Byte
  'return the mirror loop
  'by finding the other loop that has this mirror pair Value
  Dim i As Byte
  
  On Error GoTo ErrHandler
  
  'if not mirrored
  If mMirrorPair == 0 Then
    'raise error
    On Error GoTo 0: Err.Raise vbObjectError + 611, strErrSource, LoadResString(611)
    Exit Property
  End If
  
  'step through all loops in the loop collection
  For i = 0 To mParent.FriendLoops.Count - 1
    'if mirror pair values equal zero
    If mParent.FriendLoops(i).MirrorPair + mMirrorPair == 0 Then
      'this is the loop
      MirrorLoop = i
      Exit Property
    End If
  Next i
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property


Friend Property Let MirrorPair(ByVal NewMirrorPair As Integer)
  mMirrorPair = NewMirrorPair
  
End Property

Friend Property Get MirrorPair() As Integer
  MirrorPair = mMirrorPair
End Property

Friend Sub SetParent(NewParent As AGIView)
  
  Set mParent = NewParent
  mCelCol.SetParent NewParent
  
End Sub

Public Sub UnMirror()
  'if the loop is mirrored,
  'this method clears it
  'NOTE: unmirroring is handled by the
  'secondary loop; if the loop that
  'calls this function is the primary loop,
  'this function passes the call to the
  'secondary loop for processing
  
  Dim i As Byte, tmpCels As AGICels
  Dim bCelCount As Byte
  
  On Error GoTo ErrHandler
  
  If mMirrorPair == 0 Then
    Exit Sub
  End If
  
  'if this is the primary loop
  If Sgn(mMirrorPair) > 0 Then
    'unmirror other loop
    mParent.FriendLoops(MirrorLoop).UnMirror
    'exit
    Exit Sub
  End If
    
  'this is the secondary loop;
  'need to create new cel collection
  'and copy cel data
  
  'create temporary collection of cels
  Set tmpCels = New AGICels
  tmpCels.SetParent mParent
  
  'copy cels from current cel collection
  bCelCount = mCelCol.Count
  For i = 0 To bCelCount - 1
    tmpCels.Add i, mCelCol(i).Width, mCelCol(i).Height, mCelCol(i).TransColor
    'access cels through parent so mirror status is set properly
    tmpCels(i).AllCelData = mParent.FriendLoops(mIndex).Cels(i).AllCelData
  Next i
  
  'dereference cel collection
  Set mCelCol = Nothing
  'set it to new cel col
  Set mCelCol = tmpCels
  
  'clear mirror properties
  mParent.FriendLoops(MirrorLoop).MirrorPair = 0
  mMirrorPair = 0
  'if there is a parent object
  If Not (mParent Is Nothing) Then
    'set dirty flag
    mParent.IsDirty = True
  End If
Exit Sub

ErrHandler:
  'should never get an error
  Resume Next
End Sub


Private Sub Class_Initialize()
  
  
  'initialize cel collection object
  Set mCelCol = New AGICels
  
  strErrSource = "WINAGI.AGILoop"
End Sub
Private Sub Class_Terminate()
  
  Set mCelCol = Nothing
  Set mParent = Nothing
End Sub
      */
    }
  }
}
