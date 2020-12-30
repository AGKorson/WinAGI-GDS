using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  class AGINotes
  {
    void tmpNotes()
    {
      /*
Option Explicit

'local variable to hold collection
Private mCol As Collection

Private mParent As AGISound
Private mTParent As AGITrack

Private strErrSource As String

Public Sub Clear()

  'clear by setting collection to nothing
  Dim i As Long
  
  For i = mCol.Count To 1 Step -1
    mCol.Remove i
  Next i
  
  Set mCol = Nothing
  Set mCol = New Collection
  
  'if parent is assigned
  If Not mParent Is Nothing Then
    'notify parent
    mParent.NoteChanged
    mTParent.SetLengthDirty
  End If
End Sub

Public Function Add(ByVal FreqDivisor As Integer, ByVal Duration As Long, ByVal Attenuation As Integer, Optional ByVal InsertPos As Long = -1) As AGINote

  Dim agNewNote As AGINote

  On Error GoTo ErrHandler
  
  'NOTE: VB collections are '1' based;
  'AGI objects are zero based; InsertPos is
  'passed assuming zero-base, so it must
  'be adjusted to conform to VB 1-base
  InsertPos = InsertPos + 1
  
  'create a new object
  Set agNewNote = New AGINote
  
  'set parent property
  agNewNote.SetParent mParent, mTParent
  
  'set the properties passed into the method
  agNewNote.FreqDivisor = FreqDivisor
  agNewNote.Duration = Duration
  agNewNote.Attenuation = Attenuation
  
  'if no position passed (or position is past end)
  If InsertPos < 1 Or InsertPos > mCol.Count Then
    'add it to end
    mCol.Add agNewNote
  Else
    'add it before insert pos
    mCol.Add agNewNote, , InsertPos
  End If

  'return the object created
  Set Add = agNewNote
  Set agNewNote = Nothing
  
  'if parent is assigned
  If Not mParent Is Nothing Then
    'notify parent
    mParent.NoteChanged
    mTParent.SetLengthDirty
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function
Public Property Get Item(ByVal Index As Long) As AGINote
  
  'NOTE: need to adjust index by one
  'because VB collections are 1-based, but
  'agi objects are zero based
  
  On Error GoTo ErrHandler
  
  'validate
  If Index < 0 Then
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Property
  End If
  If Index > mCol.Count - 1 Then
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Property
  End If
  
  Set Item = mCol(Index + 1)
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property
Public Property Get Count() As Long
    
    Count = mCol.Count
End Property


Public Sub Remove(ByVal Index As Long)

  'NOTE: need to adjust index by one
  'because VB collections are 1-based, but
  'agi objects are zero based
  
  On Error GoTo ErrHandler
  
  'validate
  If Index < 0 Then
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Sub
  End If
  
  If Index > mCol.Count - 1 Then
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Sub
  End If
  
  mCol.Remove Index + 1
  
  'if parent is assigned
  If Not mParent Is Nothing Then
    'notify parent
    mParent.NoteChanged
    mTParent.SetLengthDirty
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Property Get NewEnum() As IUnknown
    
    Set NewEnum = mCol.[_NewEnum]
End Property


Friend Sub SetParent(NewParent As AGISound, NewTrackParent As AGITrack)

  Set mParent = NewParent
  Set mTParent = NewTrackParent
End Sub

Private Sub Class_Initialize()
    'creates the collection when this class is created
    Set mCol = New Collection
    
    strErrSource = "AGINotes"
End Sub


Private Sub Class_Terminate()
  
  'destroys collection when this class is terminated
  Dim i As Long
  For i = mCol.Count To 1 Step -1
    mCol.Remove i
  Next i
  Set mCol = Nothing
    
  'ensure parent is cleared
  Set mParent = Nothing
  Set mTParent = Nothing
End Sub
      */
    }
  }
}
