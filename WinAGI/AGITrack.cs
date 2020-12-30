using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  class AGITrack
  {
    void tmpTrack()
    {
      /*
Option Explicit

'local variable(s) to hold property Value(s)
Private mNotes As AGINotes
Private mMuted As Boolean
Private mInstrument As Byte
Private mVisible As Boolean

Private mParent As AGISound
Private mLengthDirty As Boolean
Private mLength As Single

Private strErrSource As String

Public Property Let Instrument(ByVal NewInstrument As Byte)
  
  'validate
  If NewInstrument >= 128 Then
    'error
    On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
    Exit Property
  End If
  
  If mInstrument <> NewInstrument Then
    mInstrument = NewInstrument
    'note change
    mParent.TrackChanged
  End If
End Property
Public Property Get Instrument() As Byte
    
  Instrument = mInstrument
End Property



Public Property Let Muted(ByVal NewState As Boolean)
    
  If mMuted <> NewState Then
    mMuted = NewState
    mParent.TrackChanged
  End If
End Property

Public Property Get Muted() As Boolean
    
  Muted = mMuted
End Property



Public Property Get Length() As Single
  'returns the length of this track, in seconds
  
  Dim i As Long, lngTickCount As Long
  
  On Error GoTo ErrHandler
  
  'if length has changed,
  If mLengthDirty Then
    For i = 0 To mNotes.Count - 1
      lngTickCount = lngTickCount + mNotes(i).Duration
    Next i

    '60 ticks per second
    mLength = lngTickCount / 60
    mLengthDirty = False
  End If
  
  Length = mLength
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property

Public Property Get Notes() As AGINotes
    
  Set Notes = mNotes
End Property

Friend Sub SetLengthDirty()
  
  'used by tracks to let parent sound know that length needs to be recalculated
  mLengthDirty = True
End Sub

Friend Sub SetParent(NewParent As AGISound)

  Set mParent = NewParent
  mNotes.SetParent NewParent, Me
End Sub
Public Property Let Visible(ByVal NewVal As Boolean)

  If mVisible <> NewVal Then
    mVisible = NewVal
    mParent.TrackChanged False
  End If
End Property

Public Property Get Visible() As Boolean
  
  Visible = mVisible
End Property

Private Sub Class_Initialize()

  Set mNotes = New AGINotes
  mLengthDirty = True
  mVisible = True
  mInstrument = 80
End Sub


Private Sub Class_Terminate()

  Set mNotes = Nothing
  Set mParent = Nothing
End Sub


      */
    }
  }
}
