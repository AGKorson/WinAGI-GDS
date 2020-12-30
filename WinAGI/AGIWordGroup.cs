using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  class AGIWordGroup
  {
    void tmpWordGrp()
    {
      /*
Option Explicit

'collection of words (string only)

Private mWords As Collection

Private mGroupNum As Long


Private strErrSource As String


Friend Sub AddWordToGroup(aWord As String)
  'add word to collection of strings
  
  'the fact that this word DOES NOT yet exist in this
  'group has been validated BEFORE this property is called
  
  Dim i As Integer
  
  On Error GoTo ErrHandler
  
  'if this is the first word,
  If mWords.Count = 0 Then
    'add it, using itself as key
    mWords.Add aWord, aWord
  Else
    'find correct position for this word
    If aWord < mWords(1) Then
      'this word should be first
      mWords.Add aWord, aWord, 1
    Else
      'step through rest of objects
      For i = mWords.Count To 1 Step -1
        'if new word is greater than current word
        If aWord > mWords(i) Then
          'this is where word goes
          Exit For
        End If
      Next i
      'add it, using the word itself as the key
      mWords.Add aWord, aWord, , i
    End If
  End If
Exit Sub

ErrHandler:
  'unknown error
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  On Error GoTo 0: Err.Raise vbObjectError + 573, strErrSrc, Replace(LoadResString(573), ARG1, CStr(lngError) & ":" & strError)
End Sub


Friend Sub DeleteWordFromGroup(aWord As String)
  'delete word from group
  On Error GoTo ErrHandler
  
  'the fact that this word exists in this group is
  'tested BEFORE this function is called
  
  'remove the word
  mWords.Remove aWord
Exit Sub

ErrHandler:
  'unknown error
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  On Error GoTo 0: Err.Raise vbObjectError + 572, strErrSrc, Replace(LoadResString(572), ARG1, CStr(lngError) & ":" & strError)
End Sub



Public Property Get GroupName() As String
  'return first word in group
  If mWords.Count = 0 Then
    'return empty string
    GroupName = vbNullString
  Else
    GroupName = mWords(1)
  End If
End Property


Friend Property Let GroupNum(ByVal NewGroupNumber As Long)
  'friend property needed to set group number
  'of new groups
  mGroupNum = NewGroupNumber
End Property



Public Property Get GroupNum() As Long

    GroupNum = mGroupNum
End Property



Public Property Get Word(ByVal Index As Long) As String
  'access to word list is by index only
  
  'if invalid index
  If Index < 0 Or Index > mWords.Count - 1 Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 574, strErrSource, LoadResString(574)
    Exit Property
  End If
  
  'return this word
  Word = mWords(Index + 1)
  
End Property


Public Property Get WordCount() As Integer
  WordCount = mWords.Count
  
End Property



Private Sub Class_Initialize()
  Set mWords = New Collection
  strErrSource = "WINAgi.agiWordGroup"
  
End Sub


Private Sub Class_Terminate()
  Set mWords = Nothing
End Sub

      */
    }
  }
}
