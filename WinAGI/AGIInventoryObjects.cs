using System;

namespace WinAGI
{
  public class AGIInventoryObjects
  {
    private bool mLoaded = false;
    private bool mInGame = false;
    public bool Loaded
    { get => mLoaded; }

    public bool InGame
    {
      get => mInGame;
      internal set { mInGame = value; }
    }

    public bool IsDirty { get; internal set; }
    public string ResFile { get; internal set; }

    public void Unload()
    {
      // add code here
    }

    internal void Save()
    {
      throw new NotImplementedException();
    }

    internal void Load(string v)
    {
      //throw new NotImplementedException();
    }
    public int Count
      {get; set;}

    void tmpInvObjects()
    {
      /*
Option Explicit

Private mItems As Collection

Private mMaxScreenObjects As Byte
Private mEncrypted As Boolean
Private mAmigaOBJ As Boolean

Private mResFile As String
Private mDescription As String

Private mInGame As Boolean
Private mIsDirty As Boolean
Private mWriteProps As Boolean
Private mLoaded As Boolean
Private mLoading As Boolean

'other
Private strErrSource As String

Public Property Let AmigaOBJ(ByVal NewVal As Boolean)

  'for now, we only allow converting FROM Amiga TO DOS
  '                                        (T)     (F)
  
  On Error GoTo ErrHandler
  
  'if trying to make it Amiga, exit
  If NewVal = True Then
    Exit Property
  End If
  
  'if aready DOS, exit
  If Not mAmigaOBJ Then
    Exit Property
  End If
  
  'set the flag to be NON-Amiga
  mAmigaOBJ = NewVal
  
  'save the current file as 'OBJECT.amg'
  On Error Resume Next
  Kill agGameDir & "OBJECT.amg"
  FileCopy agGameDir & "OBJECT", agGameDir & "OBJECT.amg"
  
  'now delete the current file
  Kill agGameDir & "OBJECT"
  'mark it as dirty, and save it to create a new file
  mIsDirty = True
  Save
  
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property

Public Property Get AmigaOBJ() As Boolean

  AmigaOBJ = mAmigaOBJ
End Property


Public Sub NewObjects()
  'marks the resource as loaded
  'this is needed so new resources can be created and edited
  
  On Error GoTo ErrHandler
  
  'if already loaded
  If mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 642, strErrSource, LoadResString(642)
    Exit Sub
  End If
  
  'cant call NewResource if already in a game;
  'clear it instead
  If mInGame Then
    On Error GoTo 0: Err.Raise vbObjectError + 510, strErrSource, LoadResString(510)
    Exit Sub
  End If
  
  'mark as loaded
  mLoaded = True
  
  'clear resname and description
  mResFile = vbNullString
  mDescription = vbNullString
  mMaxScreenObjects = 16
  mEncrypted = True
  
  'use clear method to ensure object list is reset
  Clear
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub CompileWinAGI(ByRef CompileFile As String)
  
  'items are saved with the description and room number
  'of each item (separated by a tab character) on a separate line
  'first line is version identifier
  'file description is saved at end of words
  
  Dim strTempFile As String
  Dim intFile As Integer
  Dim tmpItem As AGIInventoryItem
  
  On Error GoTo ErrHandler
  
  'if no name
  If LenB(CompileFile) = 0 Then
    On Error GoTo 0
    'raise error
    On Error GoTo 0: Err.Raise vbObjectError + 615, strErrSource, LoadResString(615)
    Exit Sub
  End If
  
  'get temporary file
  strTempFile = TempFileName()
  
  'open file for output
  intFile = FreeFile()
  Open strTempFile For Output As intFile
  'print version
  Print #intFile, WINAGI_VERSION
  
  'print max screen objects
  Print #intFile, mMaxScreenObjects
  
  'print item description and room for each object
  For Each tmpItem In Me
    Print #intFile, tmpItem.ItemName & vbTab & CStr(tmpItem.Room)
  Next
  
  'if there is a description
  If LenB(mDescription) <> 0 Then
    'print eof marker
    Print #intFile, Chr$(255) & Chr$(255)
    'print description
    Print #intFile, mDescription
  End If
  
  'close file
  Close intFile
  
  'if CompileFile exists
  If FileExists(CompileFile) Then
    'delete it
    Kill CompileFile
    Err.Clear
  End If
  
  'copy tempfile to CompileFile
  FileCopy strTempFile, CompileFile
  
  'delete temp file
  Kill strTempFile
  Err.Clear
  
  'if not in a game,
  If Not mInGame Then
    'change resfile
    mResFile = CompileFile
    'mark as clean
    mIsDirty = False
  End If
Exit Sub

ErrHandler:
  'close file
  Close intFile
  'erase the temp file
  Kill CompileFile
  Err.Clear
  'return error condition
  On Error GoTo 0: Err.Raise vbObjectError + 582, strErrSource, LoadResString(582)
End Sub

Public Property Get Description() As String

  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  Description = mDescription
End Property


Public Property Let Description(ByVal NewDescription As String)

  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  'limit description to 1K
  NewDescription = Left$(NewDescription, 1024)
  
  'if changing
  If NewDescription <> mDescription Then
    mDescription = NewDescription
  
    'if in a game
    If mInGame Then
      WriteGameSetting "OBJECT", "Description", mDescription
    End If
  End If
End Property
Public Sub Export(ExportFile As String, ByVal FileType As Long, Optional ByVal ResetDirty As Boolean = True)
  
  'exports the list of inventory objects
  '  filetype = 0 means AGI OBJECT file
  '  filetype = 1 means WinAGI object list file
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  On Error GoTo ErrHandler
  
  Select Case FileType
  Case 0  'compile agi OBJECT file
    Compile ExportFile
    
  Case 1  'create WinAGI Object list
    CompileWinAGI ExportFile
    
  End Select
  
  'if NOT in a game,
  If Not mInGame Then
    If ResetDirty Then
      'clear dirty flag
      mIsDirty = False
    End If
    
    'save filename
    mResFile = ExportFile
  End If
Exit Sub
  
ErrHandler:
  'return error condition
  On Error GoTo 0: Err.Raise vbObjectError + 582, strErrSource, LoadResString(582)
End Sub

Friend Property Let InGame(NewValue As Boolean)

  mInGame = NewValue
End Property

Friend Property Get InGame() As Boolean
  
  'only used by  setobjects method
  InGame = mInGame
End Property

Friend Sub Init(Optional ByVal Loaded As Boolean = False)
  
  'this function is only called for the object list
  'that is part of a game
  'it sets the ingame flag
  mInGame = True
  
  'if loaded property is passed, set loaded flag as well
  mLoaded = Loaded
  
  'set resourcefile to game default
  mResFile = agGameDir & "OBJECT"
End Sub

Public Property Get Loaded() As Boolean

  Loaded = mLoaded
End Property

Private Function LoadSierraFile(LoadFile As String) As Boolean
  'attempts to load a sierra OBJECT file
  'rturns true if successful
  
  Dim strItem As String
  Dim intItem As Integer, bytRoom As Byte
  Dim rtn As Integer
  Dim intFile As Integer
  Dim bytData() As Byte
  Dim bytLow As Byte, bytHigh As Byte
  Dim lngDataOffset As Long
  Dim lngNameOffset As Long
  Dim lngPos As Long
  Dim Dwidth As Long
  
  On Error GoTo ErrHandler
  
  'open the file
  intFile = FreeFile()
  Open LoadFile For Binary As intFile
  'if no data,
  If LOF(intFile) = 0 Then
    Close intFile
    'return false
    Exit Function
  End If
  
  'set load flag
  mLoaded = True
  
  'read in entire resource
  ReDim bytData(LOF(intFile) - 1)
  Get intFile, 1, bytData
  Close intFile
  
  'determine if file is encrypted or clear
  rtn = IsEncrypted(bytData(UBound(bytData)), UBound(bytData))
  Select Case rtn
  Case 0  'unencrypted
    mEncrypted = False
  Case 1 'encrypted
    'unencrypt file
    ToggleEncryption bytData()
    'set flag
    mEncrypted = True
  Case 2 'error
    'error in object file- return false
    mLoaded = False
    Exit Function
  End Select
  
  'set loading flag (avoids recursing and stuff like that)
  mLoading = True
  
  
  'PC files always have data element widths of 3 bytes;
  'Amigas have four; need to make sure it's correct
  Dwidth = 3
  Do Until Dwidth > 4
    'get offset to start of string data
    bytLow = bytData(0)
    bytHigh = bytData(1)
    lngDataOffset = bytHigh * 256 + bytLow + Dwidth
    
    'first char of first item should be a question mark ('?')
    If bytData(lngDataOffset) <> 63 Then
      'try again, with four
      Dwidth = Dwidth + 1
    Else
      'correct file type found
      mAmigaOBJ = (Dwidth = 4)
      Exit Do
    End If
  Loop
  
  'dwidth will be 5 if valid item data not found
  If Dwidth = 5 Then
    'error
    mLoading = False
    Exit Function
  End If
    
  'max scrn objects is always at position 2
  mMaxScreenObjects = bytData(2)
  
  'set counter to zero, and extract item info
  intItem = 0
  
  'extract each item offset, and then its string data
  '(intItem*3) is offset address of string data;
  'stop if this goes past beginning of actual data
  Do Until ((intItem * Dwidth) + Dwidth >= lngDataOffset) Or (intItem >= MAX_ITEMS)
    'extract and build offset to string data
    bytLow = bytData(Dwidth + intItem * Dwidth)
    bytHigh = bytData(Dwidth + intItem * Dwidth + 1)
    lngNameOffset = bytHigh * 256 + bytLow + Dwidth
    
    'get room number for this object
    bytRoom = bytData(Dwidth + intItem * Dwidth + 2)
    
    'set pointer to beginning of name string data
    lngPos = lngNameOffset
    'if past end of resource,
    If lngPos > UBound(bytData()) Then
      'error in object file- return false
      mLoading = False
      Exit Function
    End If
    
    'build item name string
    strItem = vbNullString
    Do Until (lngPos > UBound(bytData()))
      If bytData(lngPos) = 0 Then
        Exit Do
      End If
      strItem = strItem + Chr$(bytData(lngPos))
      lngPos = lngPos + 1
    Loop
    
    'first item IS USUALLY a '?'  , but NOT always
    '(See MH2 for example!!!!
    If intItem = 0 Then
      If strItem <> "?" Then
        'rename first object
        Item(0).ItemName = strItem
        Item(0).Room = bytRoom
        '***and if game is being imported, make a note of this!
      Else
'''        'error-
'''        mLoading = False
'''        Exit Function
        'skip it - it's added by default
      End If
      'dont add first item; it is already added
    Else
      'add without key (to avoid duplicate key error)
      Add strItem, bytRoom
    End If
    
    intItem = intItem + 1
  Loop
  
  'return true
  LoadSierraFile = True
  'reset loading flag
  mLoading = False
Exit Function

ErrHandler:
  'error-
  'reset loading flag, and return false
  mLoading = False
  mLoaded = False
  
  'ensure file is closed
  Close intFile
End Function

Private Function LoadWinAGIFile(LoadFile As String) As Boolean
  'attempts to load a winAGI file
  'if unsuccessful, returns false
  
  'use error handling to trap errors
  On Error GoTo ErrHandler
  
  Dim strInput As String
  Dim strItem As String, strRoom As String
  Dim bytRoom As Byte
  Dim intFile As Integer
  Dim lngPos As Long
  
  'set loaded flag
  mLoaded = True
  
  'attempt to open the WinAGI resource
  intFile = FreeFile()
  Open LoadFile For Input As intFile
  
  'first line should be loader
  Line Input #intFile, strInput
  Select Case strInput
  Case WINAGI_VERSION, WINAGI_VERSION_1_2, WINAGI_VERSION_1_0, WINAGI_VERSION_BETA
    'ok
  Case Else
    'any 1.2.x is ok
    If Left(strInput, 4) <> "1.2." Then
      'close file
      Close intFile
      'return false
      Exit Function
    End If
  End Select
  
  'never encrypt
  mEncrypted = False
  
  'get max screen objects from first line
  Line Input #intFile, strInput
  'if within bounds
  If Val(strInput) > 255 Then
    mMaxScreenObjects = 255
  ElseIf Val(strInput) < 1 Then
    mMaxScreenObjects = 1
  Else
    mMaxScreenObjects = CByte(Val(strInput))
  End If
  
  'set loading flag
  mLoading = True
  
  'read in each item
  Do While Not EOF(intFile)
    'get input string
    Line Input #intFile, strInput
    'check for end of input characters
    If strInput = Chr$(255) & Chr$(255) Then
      Exit Do
    End If
    
    'check for tab character
    lngPos = InStr(1, strInput, vbTab)
    'if tab character found
    If lngPos > 0 Then
      'strip off room number
      strRoom = Right$(strInput, Len(strInput) - lngPos)
      strItem = Left$(strInput, lngPos - 1)
      'convert to byte Value
      If Val(strRoom) > 255 Then
        bytRoom = 255
      ElseIf Val(strRoom) < 0 Then
        bytRoom = 0
      Else
        bytRoom = CByte(Val(strRoom))
      End If
    Else
      'no room included; assume zero
      bytRoom = 0
    End If
    'add without key (to avoid duplicate key error)
    Add strItem, bytRoom
  Loop
  
  'if any lines left
  If Not EOF(intFile) Then
    'get first remaining line
    Line Input #intFile, strItem
    'if there are additional lines, add them as a description
    Do Until EOF(intFile)
      Line Input #intFile, strInput
      strItem = strItem & vbCr & strInput
    Loop
    'save description
    mDescription = strItem
  End If
  
  'close file
  Close intFile
  
  'should already have one object?????
  '*'Debug.Assert mItems.Count = 1
  'if no objects loaded
  If mItems.Count = 0 Then
    'add default object
    mItems.Add "?", 0
  End If
  
  'clear loading flag
  mLoading = False
  'return true
  LoadWinAGIFile = True
Exit Function

ErrHandler:
  'error- ensure file is closed
  Close intFile
  'return false
  mLoaded = False
End Function

Public Property Get ResFile() As String
    
  ResFile = mResFile
End Property



Public Property Let ResFile(NewSourceFile As String)
  
  'resfile cannot be changed if resource is part of a game
  
  'if in a game
  If mInGame Then
    'error- resfile is readonly for ingame resources
    On Error GoTo 0: Err.Raise vbObjectError + 680, strErrSource, LoadResString(680)
  Else
    mResFile = NewSourceFile
  End If
End Property

Public Property Get Count() As Integer

  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  'although first object does not Count as an object
  'it must be returned as part of the Count
  'so everything works properly
  Count = mItems.Count
End Property


Public Property Let IsDirty(ByVal DirtyState As Boolean)
    
    mIsDirty = DirtyState
End Property


Public Property Get NewEnum() As IUnknown
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  Set NewEnum = mItems.[_NewEnum]
End Property



Public Property Get Item(ByVal Index As Long) As AGIInventoryItem
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  'collections are 1 based; this object should be zero based
  'valid index number is 0 to Count-1
  If Index < 0 Or Index > mItems.Count - 1 Then
    'error- invalid index
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Property
  End If
    
  'retrieve via index
  Set Item = mItems(Index + 1)
  
End Property




Public Property Get IsDirty() As Boolean
  
  'if resource is dirty, or (prop values need writing AND in game)
  IsDirty = (mIsDirty Or (mWriteProps And mInGame))
End Property

Friend Sub SetFlags(ByRef IsDirty As Boolean, ByRef WriteProps As Boolean)
  
  IsDirty = mIsDirty
  WriteProps = mWriteProps
End Sub


Public Function Add(NewItem As String, ByVal Room As Byte) As AGIInventoryItem
  'adds new item to object list
  On Error GoTo ErrHandler
  
  Dim i As Long
  Dim tmpItem As AGIInventoryItem
  
  'if not currently loading, or already loaded
  If Not mLoading And Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Function
  End If
  
  'if already have max number of items,
  If mItems.Count = MAX_ITEMS Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 569, LoadResString(569)
    Exit Function
  End If
  
  'add the item
  Set tmpItem = New AGIInventoryItem
  tmpItem.SetParent Me
  tmpItem.ItemName = NewItem
  tmpItem.Room = Room
  
  mItems.Add tmpItem
  
  Set Add = tmpItem
  
  Set tmpItem = Nothing
  
  'set dirty flag
  mIsDirty = True
Exit Function

ErrHandler:
  'unknown error during add?
  On Error GoTo 0: Err.Raise vbObjectError + 571, strErrSource, Replace(LoadResString(571), ARG1, CStr(Err.Number))
End Function



Public Sub Remove(ByVal Index As Long)
  'removes strObject from the object list
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
    
  'collections are 1 based; this object should be zero based
  'valid range of index is 0 to Count-1
  
  If Index < 0 Or Index >= mItems.Count Then
    'error- invalid index
    On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
    Exit Sub
  End If
  
  'if this is the last item (but not FIRST item)
  If Index + 1 = mItems.Count And mItems.Count > 1 Then
    'remove the item
    mItems.Remove Index + 1
  Else
    'set item to '?'
    mItems(Index + 1).ItemName = "?"
    mItems(Index + 1).Room = 0
  End If
  
  'set dirty flag
  mIsDirty = True
End Sub


Public Property Let Encrypted(ByVal Encrypt As Boolean)
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  'if change in encryption
  If mEncrypted <> Encrypt Then
    'set dirty flag
    mIsDirty = True
  End If
  mEncrypted = Encrypt
End Property



Public Property Get Encrypted() As Boolean
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  Encrypted = mEncrypted
End Property




Public Property Get MaxScreenObjects() As Byte

  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  MaxScreenObjects = mMaxScreenObjects
End Property



Public Property Let MaxScreenObjects(ByVal NewMaxVal As Byte)
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  'if change in max objects
  If NewMaxVal <> mMaxScreenObjects Then
    mMaxScreenObjects = NewMaxVal
    mIsDirty = True
  End If
End Property





Public Sub Load(Optional LoadFile As String)
  
  'this function loads inventory objects for the game
  'if loading from a Sierra game, it extracts items
  'from the OBJECT file;
  'if not in a game, LoadFile must be specified
  
  Dim blnSuccess As Boolean
  
  On Error GoTo ErrHandler
  
  'if already loaded,
  If mLoaded Then
    'error- resource already loaded
    On Error GoTo 0: Err.Raise vbObjectError + 511, strErrSource, LoadResString(511)
    Exit Sub
  End If
  
  'should always start with a default list
  '*'Debug.Assert mItems.Count = 1
  
  'if in a game
  If mInGame Then
    'use default Sierra name
    LoadFile = agGameDir & "OBJECT"
    'attempt to load
    If Not LoadSierraFile(LoadFile) Then
      'reset objects resource using 'clear' method
      '*'Debug.Assert mLoaded = True
      Me.Clear
      
      'error
      On Error GoTo 0: Err.Raise vbObjectError + 692, strErrSource, LoadResString(692)
    End If
    
    'get description, if there is one
    '*'Debug.Assert Not (agGameProps Is Nothing)
    ReadSettingString agGameProps, "OBJECT", "Description", vbNullString
    
  Else
    'if NOT in a game, file must be specified
  
    'if optional filename not passed,
    If LenB(LoadFile) = 0 Then
      'no file specified; return error
      On Error GoTo 0
      On Error GoTo 0: Err.Raise vbObjectError + 599, strErrSource, LoadResString(599)
      Exit Sub
    End If
    
    'verify file exists
    If Not FileExists(LoadFile) Then
      On Error GoTo 0
      'error
      On Error GoTo 0: Err.Raise vbObjectError + 524, strErrSource, Replace(LoadResString(524), ARG1, LoadFile)
      Exit Sub
    End If
    
    'if extension is .ago then
    If LCase$(Right$(LoadFile, 4)) = ".ago" Then
      'assume winagi format
      If Not LoadWinAGIFile(LoadFile) Then
        'try sierra format
        If Not LoadSierraFile(LoadFile) Then
          On Error GoTo 0
          'error
          On Error GoTo 0: Err.Raise vbObjectError + 692, strErrSource, LoadResString(692)
          Exit Sub
        End If
      End If
    Else
      'assume sierra format
      If Not LoadSierraFile(LoadFile) Then
        'try winagi format
        If Not LoadWinAGIFile(LoadFile) Then
          On Error GoTo 0
          'error
          On Error GoTo 0: Err.Raise vbObjectError + 692, strErrSource, LoadResString(692)
          Exit Sub
        End If
      End If
    End If
  
    'save filename
    mResFile = LoadFile
  End If
  
  'reset dirty flag
  mIsDirty = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Function IsEncrypted(ByVal bytLast As Byte, lngEndPos As Long) As Integer
  'this routine checks the resource to determine if it is
  'encrypted with the string, "Avis Durgan"
  
  'it does this by checking the last byte in the resource. It should ALWAYS
  'be Chr$(0):
  '   if the resource is NOT encrypted,
  '     the last byte will have a Value of &H00
  '     the function will return a Value of 0
  '
  '   if it IS encrypted,
  '     it will be a character from the "Avis Durgan"
  '     string, dependent on the offset of the last
  '     byte from a multiple of 11 (the length of "Avis Durgan")
  '     the function will return a Value of 1
  
  'if the last character doesn't properly decrypt to Chr$(0)
  'the function returns an error Value (2)
  'Note: the last byte is passed by Value so it can be temporarily
  'modified during the check
  
  'if it's a zero,
  If bytLast = 0 Then
    'return unencrypted
    IsEncrypted = 0
  Else
    'decrypt character
    bytLast = bytLast Xor bytEncryptKey(lngEndPos Mod 11)
    
    'now, it should be zero
    If bytLast = 0 Then
      IsEncrypted = 1
    Else
      IsEncrypted = 2
    End If
  End If
End Function

Public Sub Save(Optional SaveFile As String, Optional ByVal FileType As Long)
  
  'saves the list of inventory objects
  '  filetype = 0 means AGI OBJECT file
  '  filetype = 1 means WinAGI object list file
  'for ingame resources, SaveFile and filetype are ignored
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  On Error GoTo ErrHandler
  
  'if in a game,
  If mInGame Then
    'compile the file
    Compile mResFile
    
    'change date of last edit
    agLastEdit = Now()
  
  Else
    If LenB(SaveFile) = 0 Then
      SaveFile = mResFile
    End If
    
    'if still no file
    If LenB(SaveFile) = 0 Then
      On Error GoTo 0: Err.Raise 615, strErrSource, LoadResString(615)
      Exit Sub
    End If
    
    Select Case FileType
    Case 0  'compile agi OBJECT file
      Compile SaveFile
      
    Case 1  'create WinAGI Object list
      CompileWinAGI SaveFile
      
    End Select
  
    'save filename
    mResFile = SaveFile
  End If
  
  'mark as clean
  mIsDirty = False
  
Exit Sub
  
ErrHandler:
  'return error condition
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub
Public Sub SetObjects(NewObjects As AGIInventoryObjects)
  'copies object list from NewObjects to
  'this object list
  
  Dim i As Long
  
  'if source objectlist is not loaded
  If Not NewObjects.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  On Error GoTo ErrHandler
  
  'first, clear current list
  Clear
  
  'first item of new objects should be a '?'
  '*'Debug.Assert NewObjects(0).ItemName = "?"
  
  With NewObjects
    'add all objects EXCEPT for first '?' object
    'which is already preloaded with the clear method
    For i = 1 To .Count - 1
      Add .Item(i).ItemName, .Item(i).Room
    Next i
    
    'RARE, but check for name/room change to item 0
    If .Item(0).ItemName <> "?" Then
      Me.Item(0).ItemName = .Item(0).ItemName
    End If
    If .Item(0).Room <> 0 Then
      Me.Item(0).Room = .Item(0).Room
    End If
    
    'set max screenobjects
    mMaxScreenObjects = .MaxScreenObjects
    'set encryption flag
    mEncrypted = .Encrypted
    
    'set description
    mDescription = .Description
    
    'set dirty flag
    .SetFlags mIsDirty, mWriteProps
    
    'set filename
    mResFile = .ResFile
    
    'set load status
    mLoaded = True
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub ToggleEncryption(ByRef bytData() As Byte)
  'this function encrypts/decrypts the data
  'by XOR'ing it with the encryption string
  
  Dim lngPos As Long
  
  For lngPos = 0 To UBound(bytData())
    bytData(lngPos) = bytData(lngPos) Xor bytEncryptKey(lngPos Mod 11)
  Next lngPos
End Sub



Public Sub Unload()
  'unloads ther resource; same as clear, except file marked as not dirty
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  Clear
  mLoaded = False
  
  mWriteProps = False
  mIsDirty = False
End Sub

Private Sub Class_Initialize()

  Dim tmpItem As AGIInventoryItem
  
  On Error GoTo ErrHandler
  
  strErrSource = "WINAGI.agiObjectFile"
  'verify encryption key is loaded
  If bytEncryptKey(0) <> 65 Then
    'call initialization
    InitializeAGI
  End If

  mMaxScreenObjects = 16
  
  Set mItems = New Collection
  
  'add placeholder for item 0
  Set tmpItem = New AGIInventoryItem
  'but don't set parent; otherwise
  'circular object reference is created
  tmpItem.ItemName = "?"
  tmpItem.Room = 0
  mItems.Add tmpItem
  Set tmpItem = Nothing
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Public Sub Clear()

  'clears ALL objects and sets default values
  'adds placeholder for item 0
  
  Dim tmpItem As AGIInventoryItem
  
  'if not loaded
  If Not mLoaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  mEncrypted = False
  mMaxScreenObjects = 16
  mAmigaOBJ = False
  mDescription = vbNullString
  Set mItems = Nothing
  Set mItems = New Collection
  
  'add the placeholder
  Set tmpItem = New AGIInventoryItem
  tmpItem.ItemName = "?"
  tmpItem.Room = 0
  mItems.Add tmpItem
  'but don't set parent; otherwise
  'circular object reference is created
tmpItem.SetParent Me
  Set tmpItem = Nothing
    
  'set dirty flag
  mIsDirty = True
End Sub


 
 Private Sub Compile(CompileFile As String)
  'compiles the object list into a Sierra AGI compatible OBJECT file
  
  Dim lngFileSize  As Long   'size of object file
  Dim CurrentChar As Integer 'current character in name
  Dim lngDataOffset As Long  'start of item names
  Dim lngPos As Long         'position in current item name
  Dim bytTemp() As Byte
  Dim strTempFile As String
  Dim bytLow As Byte, bytHigh As Byte
  Dim intFile As Integer
  Dim i As Integer
  Dim Dwidth As Long
  
  'use errorhandler
  On Error GoTo ErrHandler
  
  'if no file
  If LenB(CompileFile) = 0 Then
    On Error GoTo 0: Err.Raise vbObjectError + 616, strErrSource, LoadResString(616)
    Exit Sub
  End If
  
  'if not dirty AND compilefile=resfile
  If Not mIsDirty And UCase$(CompileFile) = UCase$(mResFile) Then
    Exit Sub
  End If
  
  'PC version (most common) has 3 bytes per item in offest table; amiga version has four bytes per item
  If mAmigaOBJ Then
    Dwidth = 4
  Else
    Dwidth = 3
  End If
  
  'calculate min filesize
  '(offset table size plus header + null obj '?')
  lngFileSize = (mItems.Count + 1) * Dwidth + 2
  
  '******remember that item collection is '1' based
  
  'step through all items to determine length of each, and add it to file length counter
  For i = 1 To mItems.Count
    'if this item is NOT "?"
    If mItems(i).ItemName <> "?" Then
      'add number size of object name to file size
      '(include null character at end of string)
      lngFileSize = lngFileSize + Len(mItems(i).ItemName) + 1
    End If
  Next i

  'initialize byte array to final size of file
  ReDim bytTemp(lngFileSize - 1)
  
  'set offset from index to start of string data
  lngDataOffset = mItems.Count * Dwidth
  bytHigh = lngDataOffset \ 256
  bytLow = lngDataOffset Mod 256
  
  'write offest for beginning of text data
  bytTemp(0) = bytLow
  bytTemp(1) = bytHigh
  'write max number of screen objects
  bytTemp(2) = mMaxScreenObjects
  
  'increment offset by width (to take into account file header)
  'this is also pointer to the null item
  lngDataOffset = lngDataOffset + Dwidth
  'set counter to beginning of data
  lngPos = lngDataOffset
  
  'write string for null object ('?')
  bytTemp(lngPos) = 63 '(?)
  bytTemp(lngPos + 1) = 0
  lngPos = lngPos + 2
  
  'now step through all words
  For i = 1 To mItems.Count
    'if object is '?'
    If mItems(i).ItemName = "?" Then
      'write offset data to null item
      bytTemp(i * Dwidth) = lngDataOffset Mod 256
      bytTemp(i * Dwidth + 1) = lngDataOffset \ 256
      'set room number for this object
      bytTemp(i * Dwidth + 2) = mItems(i).Room
    Else
      'write offset data for start of this word
      'subtract data element width because offset is from end of header,
      'not beginning of file; lngPos is referenced from position zero)
      bytHigh = (lngPos - Dwidth) \ 256
      bytLow = (lngPos - Dwidth) Mod 256
      bytTemp(i * Dwidth) = bytLow
      bytTemp(i * Dwidth + 1) = bytHigh
      'write room number for this object
      bytTemp(i * Dwidth + 2) = mItems(i).Room
      'write all characters of this object
      For CurrentChar = 1 To Len(mItems(i).ItemName)
        bytTemp(lngPos) = AscW(Mid$(mItems(i).ItemName, CurrentChar))
        lngPos = lngPos + 1
      Next CurrentChar
      'add null character to end
      bytTemp(lngPos) = 0
      lngPos = lngPos + 1
    End If
  Next i
  
  'reduce array to actual size
  ReDim Preserve bytTemp(lngPos - 1)
  
  'if file is to be encrypted
  If mEncrypted Then
    'step through entire file
    For lngPos = 0 To UBound(bytTemp)
      'encrypt with 'Avis Durgan'
      bytTemp(lngPos) = bytTemp(lngPos) Xor bytEncryptKey(lngPos Mod 11)
    Next lngPos
  End If

  'make temp file
  strTempFile = TempFileName()
  
  'write the data to file
  intFile = FreeFile()
  Open strTempFile For Binary As intFile
  Put intFile, 1, bytTemp
  Close intFile
  
  'if savefile already exists
  If FileExists(CompileFile) Then
    'delete it
    Kill CompileFile
  End If
  
  'copy tempfile to savefile
  FileCopy strTempFile, CompileFile
  'delete temp file
  Kill strTempFile
Exit Sub

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  
  'delete temporary file
  Kill strTempFile
  'raise the error
  On Error GoTo 0: Err.Raise vbObjectError + 674, strErrSrc, Replace(LoadResString(674), ARG1, CStr(lngError) & ":" & strError)
End Sub
Private Sub Class_Terminate()
  
  Set mItems = Nothing
End Sub


      */
    }
  }

}