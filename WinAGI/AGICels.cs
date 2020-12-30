using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  class AGICels
  {
    void tmpCELS()
    {
/*
Option Explicit

'local variable to hold array of cels

Private mCelCol As Collection

Private mParent As AGIView
Private mSetMirror As Boolean

'other

Private strErrSource As String

  

Public Property Get Item(ByVal Index As Byte) As AGICel
  'NOTE: need to adjust index by one because
  'VB collections are 1-based but AGI objects
  'are 0-based
  On Error GoTo ErrHandler
  
  Set Item = mCelCol(Index + 1)
      
  'pass setmirror flag
  Item.SetMirror mSetMirror
Exit Property

ErrHandler:
  'invalid item
  On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
End Property



Public Property Get Count() As Long
    'return number of cels
    Count = mCelCol.Count
End Property



Public Function Add(Optional ByRef Pos As Byte = 255, Optional ByVal CelWidth As Byte = 1, Optional ByVal CelHeight As Byte = 1, Optional ByVal TransColor As AGIColors = agBlack) As AGICel
  Dim agNewCel As AGICel
  
  'NOTE: VB collections are 1-based; AGI collections are zero based
  'this function adjusts input values (which are zero based) to
  'the correct Value to work with VB collections
  On Error GoTo ErrHandler
  
  'if too many cels
  If mCelCol.Count = MAX_CELS Then
    'error - too many cels
    On Error GoTo 0: Err.Raise vbObjectError + 552, strErrSource, Replace(LoadResString(552), ARG1, vbNullString)
    Exit Function
  End If
  
  'set the properties passed into the method
  Set agNewCel = New AGICel
  'set parent
  agNewCel.SetParent mParent
  
  On Error Resume Next
  agNewCel.Width = CelWidth
  agNewCel.Height = CelHeight
  agNewCel.TransColor = TransColor
  On Error GoTo ErrHandler
  
  'if no position is passed,
  '(or if past end of loops),
  If Pos > mCelCol.Count Then
    'set it to end
    Pos = mCelCol.Count
  End If
  
  'if no cels yet
  If mCelCol.Count = 0 Then
    'just add it
    mCelCol.Add agNewCel
  ElseIf Pos = 0 Then
    'add new loop to front
    mCelCol.Add agNewCel, , 1
  Else
    'add it after the current loop with that number
    mCelCol.Add agNewCel, , , Pos
  End If
  
  'return the object created
  Set Add = agNewCel
  Set agNewCel = Nothing
  'if there is a parent view
  If Not (mParent Is Nothing) Then
    'tag as dirty
    mParent.IsDirty = True
  End If
  
Exit Function

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  On Error GoTo 0: Err.Raise vbObjectError + 653, strErrSrc, Replace(LoadResString(653), ARG1, CStr(lngError) & ":" & strError)
End Function





Public Sub Remove(ByVal Index As Long)
  
  Dim i As Long
  Dim tmpCel As AGICel
  
  On Error Resume Next
  
  'if this is last cel
  If mCelCol.Count = 1 Then
    'cant remove last cel
    On Error GoTo 0: Err.Raise vbObjectError + 612, strErrSource, LoadResString(612)
    Exit Sub
  End If
  
  'remove cel
  mCelCol.Remove Index + 1

  'if error
  If Err.Number <> 0 Then
    'invalid item
    On Error GoTo 0: Err.Raise 9, strErrSource, "subscript out of range"
    Exit Sub
  End If
  
  'if this was not last cel
  If Index < mCelCol.Count Then
    'ensure cels after this position have correct index
    For i = Index + 1 To mCelCol.Count
      Set tmpCel = mCelCol(i)
      tmpCel.Index = CByte(i - 1)
    Next i
    Set tmpCel = Nothing
  End If
  
  'tag as dirty
  mParent.IsDirty = True
End Sub

Friend Sub SetMirror(ByVal NewState As Boolean)
  'this method is called just before the cels collection
  'is referenced by a mirrored loop
  'it is used to force the celbmp functions to
  'flip cel bitmaps and to flip cel data
  mSetMirror = NewState
End Sub

Friend Sub SetParent(NewParent As AGIView)
  
  Set mParent = NewParent
End Sub


Private Sub Class_Initialize()
  
  Set mCelCol = New Collection
  
  strErrSource = "WINAGI.agiCels"
End Sub

Private Sub Class_Terminate()
  
  Dim i As Long
  
  For i = mCelCol.Count To 1 Step -1
    mCelCol.Remove i
  Next i
  
  Set mCelCol = Nothing
  Set mParent = Nothing
End Sub

*/
    }
  }
}
