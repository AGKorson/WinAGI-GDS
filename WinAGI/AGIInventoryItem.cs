using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  class AGIInventoryItem
  {
    void tmpInvItem()
    {
      /*
Option Explicit


Private mItemName As String
Private mRoom As Byte

'unique flag used to identify objects that are unique to the list
'if two or more objects have the same name, then all are flagged
'as NOT unique; that way the compilers and decompilers can handle
'the duplicate objects correctly
Private mUnique As Boolean

Private mParent As AGIInventoryObjects


Public Property Let ItemName(NewName As String)
  
  Dim i As Long
  Dim lngDupItem As Long, lngDupCount As Long
  
  'if there is a parent
  If Not mParent Is Nothing Then
    'if this item is currently a duplicate
    If Not mUnique Then
      'there are at least two objects with this item name;
      'this object and one or more duplicates
      'if there is only one other duplicate, it needs to have its
      'unique property reset because it will no longer be unique
      'after this object is changed
      'if there are multiple duplicates, the unique property does
      'not need to be reset
      '*'Debug.Assert mItemName <> "?"
      
      For i = 0 To mParent.Count - 1
        If Not mParent(i) Is Me Then
          If StrComp(mItemName, mParent(i).ItemName, vbTextCompare) = 0 Then
            'duplicate found- is this the second?
            If lngDupCount = 1 Then
              'no need to set unique property
              Exit For
            Else
              'increment dupcount
              lngDupCount = 1
              'save dupitem number
              lngDupItem = i
            End If
          End If
        End If
      Next i
      
      '*'Debug.Assert lngDupCount = 1
      'set the unique flag for this object and for the dup object
      mUnique = True
      mParent(lngDupItem).Unique = True
    End If
  End If
  
  'assign name
  mItemName = NewName
  
  'if blank,
  If LenB(mItemName) = 0 Then
    'set it to '?'
    mItemName = "?"
  End If
  
  'if there is a parent
  If Not mParent Is Nothing Then
    mParent.IsDirty = True
    
    'if this item is NOT an unassigned object ('?')
    If mItemName <> "?" Then
      'check for duplicates
      For i = 0 To mParent.Count - 1
        'skip this item
        If Not mParent(i) Is Me Then
          If StrComp(mItemName, mParent(i).ItemName, vbTextCompare) = 0 Then
            'mark both as NOT unique
            mParent(i).Unique = False
            mUnique = False
          End If
        End If
      Next i
    End If
  End If
End Property


Public Property Get ItemName() As String
  ItemName = mItemName
End Property


Public Property Let Room(ByVal NewRoom As Byte)
  mRoom = NewRoom
  
  'if there is a parent
  If Not mParent Is Nothing Then
    mParent.IsDirty = True
  End If
End Property


Public Property Get Room() As Byte
  Room = mRoom
  
End Property

Friend Sub SetParent(Parent As AGIInventoryObjects)
#If DEBUGMODE = 1 Then
tmpCount = tmpCount + 1
If tmpCount > 1000000 Then
Debug.Assert False
Exit Sub
End If
#End If

  'sets parent for this item
  Set mParent = Parent
End Sub


Friend Property Let Unique(ByVal NewVal As Boolean)

  mUnique = NewVal
End Property

Public Property Get Unique() As Boolean

  Unique = mUnique
End Property

Private Sub Class_Initialize()

  'always unique until proven otherwise
  mUnique = True
End Sub

Private Sub Class_Terminate()
  'ensure reference to parent is released
  Set mParent = Nothing
End Sub


      */
    }
  }
}
