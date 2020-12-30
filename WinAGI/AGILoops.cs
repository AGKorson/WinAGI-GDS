using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  class AGILoops
  {
    void tmpLoops()
    {
      /*
Option Explicit

'local variable to hold collection

Private mLoopCol As Collection
Private mParent As AGIView

Private strErrSource As String
Public Function Add(Optional ByRef Pos As Byte = 255) As AGILoop
  'Pos is position of this loop in the loop collection
  Dim agNewLoop As AGILoop
  Dim i As Integer
  Dim tmpCel As AGICel
  
  'NOTE: VB collections are 1-based; AGI collections are zero based
  'this function adjusts input values (which are zero based) to
  'the correct Value to work with VB collections
  '*'Debug.Assert Not mParent Is Nothing
  
  'if too many loops
  If mLoopCol.Count = MAX_LOOPS Then
    'error - too many loops
    On Error GoTo 0: Err.Raise vbObjectError + 537, strErrSource, LoadResString(537)
    Exit Function
  End If
  
  'if no position is passed,
  '(or if past end of loops),
  If Pos > mLoopCol.Count Then
    'set it to end
    Pos = mLoopCol.Count
  End If
  
  'if adding a loop in position 0-7
  '(which could push a mirror loop out of position
  If Pos <= 7 And mLoopCol.Count > 7 Then
    'if loop 7 is a mirror
    If mLoopCol(7).Mirrored Then
      'unmirror it
      mLoopCol(7).UnMirror
    End If
  End If
  
  'create new loop object
  Set agNewLoop = New AGILoop
  'set index
  agNewLoop.Index = Pos
  'set parent
  agNewLoop.SetParent mParent
  
  'if no loops yet
  If mLoopCol.Count = 0 Then
    'just add it
    mLoopCol.Add agNewLoop
  ElseIf Pos = 0 Then
    'add new loop to front
    mLoopCol.Add agNewLoop, , 1
  Else
    'add it after the current loop with that number
    mLoopCol.Add agNewLoop, , , Pos
  End If
  
'  'add a cel to the loop?
'  agNewLoop.Cels.Add

  'return the object created
  Set Add = agNewLoop
  
  'update index of all loops
  For i = 1 To mLoopCol.Count
    'need to copy object in order to access friend properties
    Set agNewLoop = mLoopCol(i)
    agNewLoop.Index = CByte(i - 1)
  Next i
  
  Set agNewLoop = Nothing
  'set dirty flag
  mParent.IsDirty = True
Exit Function

ErrHandler:

End Function

Public Property Get Item(ByVal Index As Byte) As AGILoop
  
  'NOTE: need to adjust index by one
  'because VB collections are 1-based, but
  'agi objects are zero based
  
  On Error Resume Next
  
  'validate
  If Index < 0 Then
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Property
  End If
  If Index > mLoopCol.Count - 1 Then
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Property
  End If
    
  Set Item = mLoopCol(Index + 1)
End Property




Public Property Get Count() As Long
    
    Count = mLoopCol.Count
End Property



Public Sub Remove(ByVal Index As Byte)
  Dim i As Byte, j As Byte
  Dim xPos As Byte, yPos As Byte
  Dim bH As Byte, bW As Byte
  Dim tmpLoop As AGILoop, tmpCel As AGICel
  Dim tmpCelData() As AGIColors
  
  On Error GoTo ErrHandler
  
  'if this is last loop
  If mLoopCol.Count = 1 Then
    'can't delete last loop
    On Error Resume Next
    On Error GoTo 0: Err.Raise vbObjectError + 613, strErrSource, LoadResString(613)
    Exit Sub
  End If

  On Error Resume Next
  'if this loop is  a mirrored loop
  If mLoopCol(Index + 1).Mirrored Then
    'first attempt to access the object will validate the index Value)
    'check for error
    If Err.Number <> 0 Then
      'invalid item
      On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
      Exit Sub
    End If
    
    'use errhandler
    On Error GoTo ErrHandler
    
    'need to make local copy of mirror loop
    'to access friend properties
    Set tmpLoop = mLoopCol(mLoopCol(Index + 1).MirrorLoop + 1)
    'clear mirrorpair for the mirror
    tmpLoop.MirrorPair = 0
    
    'need to make local copy of this loop
    'to access friend properties
    Set tmpLoop = mLoopCol(Index + 1)
    
    'if this is the primary loop
    If tmpLoop.MirrorPair > 0 Then
      'need to permanently flip cel data
      For i = 0 To tmpLoop.Cels.Count - 1
        Set tmpCel = tmpLoop.Cels(i)
        tmpCel.FlipCel
      Next i
      Set tmpCel = Nothing
    End If
    Set tmpLoop = Nothing
  End If
  
  'ensure errhandling
  On Error GoTo ErrHandler
  
  mLoopCol.Remove Index + 1
   
  'ensure all loop indices are correct
  If mLoopCol.Count > 0 Then
    For i = 0 To mLoopCol.Count - 1
      Set tmpLoop = mLoopCol(i + 1)
      tmpLoop.Index = i
    Next i
    Set tmpLoop = Nothing
  End If
  
  'if this was not last loop
  If Index < mLoopCol.Count Then
    'ensure loops after this position have correct index
    For i = Index + 1 To mLoopCol.Count
      Set tmpLoop = mLoopCol(i)
      tmpLoop.Index = CByte(i - 1)
    Next i
    Set tmpLoop = Nothing
  End If
  
  'tag as dirty
  mParent.IsDirty = True
Exit Sub
  
ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  On Error GoTo 0: Err.Raise vbObjectError + 661, strErrSrc, Replace(LoadResString(661), ARG1, CStr(lngError) & ":" & strError)
End Sub


Public Property Get NewEnum() As IUnknown
    
    Set NewEnum = mLoopCol.[_NewEnum]
End Property


Friend Sub SetParent(NewParent As AGIView)
  
  Set mParent = NewParent
End Sub


Private Sub Class_Initialize()

    'creates the collection when this class is created
    Set mLoopCol = New Collection
End Sub
Private Sub Class_Terminate()

  'destroys collection when this class is terminated
  Dim i As Long
  For i = mLoopCol.Count To 1 Step -1
    mLoopCol.Remove i
  Next i
  
  Set mLoopCol = Nothing
  Set mParent = Nothing
End Sub
      */
    }
  }
}
