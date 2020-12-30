using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  class AGINote
  {
    void tmpNote()
    {
      /*
Option Explicit

Private mFreqDiv As Integer
Private mDuration As Long
Private mAttenuation As Integer

Private mParent As AGISound
Private mTParent As AGITrack

Private strErrSource As String

Property Let Attenuation(ByVal NewAttenuation As Integer)

  On Error GoTo ErrHandler
  
  'validate
  If NewAttenuation < 0 Then
    'invalid frequency
    On Error GoTo 0: Err.Raise 6, strErrSource, "Overflow"
    Exit Property
  End If
  
  If NewAttenuation > 15 Then
    'invalid item
    On Error GoTo 0: Err.Raise 6, strErrSource, "Overflow"
    Exit Property
  End If
  
  mAttenuation = NewAttenuation
  
  'if parent is assigned
  If Not mParent Is Nothing Then
    'notify parent
    mParent.NoteChanged
  End If
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property

Public Property Get Attenuation() As Integer
  
  Attenuation = mAttenuation
End Property

Public Property Let Duration(ByVal NewDuration As Long)

  On Error GoTo ErrHandler
  
  'validate
  If NewDuration < 0 Then
    'invalid frequency
    On Error GoTo 0: Err.Raise 6, strErrSource, "Overflow"
    Exit Property
  End If
  
  If NewDuration > &HFFFF& Then
    'invalid item
    On Error GoTo 0: Err.Raise 6, strErrSource, "Overflow"
    Exit Property
  End If
  
  mDuration = NewDuration
  
  'if parent is assigned
  If Not mParent Is Nothing Then
    'notify parent
    mParent.NoteChanged
    mTParent.SetLengthDirty
  End If
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property

Public Property Get Duration() As Long
  
  Duration = mDuration
End Property


Public Property Let FreqDivisor(ByVal NewFreq As Integer)
  
  On Error GoTo ErrHandler
  
  'validate
  If NewFreq < 0 Then
    'invalid frequency
    On Error GoTo 0: Err.Raise 6, strErrSource, "Overflow"
    Exit Property
  End If
  
  If NewFreq > 1023 Then
    'invalid item
    On Error GoTo 0: Err.Raise 6, strErrSource, "Overflow"
    Exit Property
  End If
  
  mFreqDiv = NewFreq
  
  'if parent is assigned
  If Not mParent Is Nothing Then
    'notify parent
    mParent.NoteChanged
  End If
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property
Public Property Get FreqDivisor() As Integer
  
  FreqDivisor = mFreqDiv
End Property


Private Sub Class_Initialize()

  strErrSource = "AGINote"
End Sub


Private Sub Class_Terminate()

  'ensure reference to parent is released
  Set mParent = Nothing
  Set mTParent = Nothing
End Sub


Friend Sub SetParent(NewParent As AGISound, NewTrackParent As AGITrack)
  
  'sets parent for this item
  Set mParent = NewParent
  Set mTParent = NewTrackParent
End Sub

      */
    }
  }
}
