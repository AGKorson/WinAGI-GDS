using System;
using System.Collections.Generic;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;

namespace WinAGI
{
  public class AGILogics
  {
    public AGILogics()
    {
      // create the initial Col object
      Col = new List<AGILogic>();
    }

    public AGILogic this[byte index]
    { get { return Col[index]; } }

    internal List<AGILogic> Col
    { get; private set; }
    public byte Count
    { get { return (byte)Col.Count; } private set { } }
    public byte Max
    { get
      {
        byte max = 0;
        foreach (AGILogic tmpLogic in Col)
        {
          if (tmpLogic.Number > max)
            max = tmpLogic.Number;
        }
        return max;
      }
    }

    public bool Exists(byte ResNum)
    {
      //check for thsi logic in the collection
      foreach (AGILogic tmpLog in Col)
      {
        if (tmpLog.Number == ResNum)
          return true;
      }
      return false;
    }
    internal void Clear()
    {
      Col = new List<AGILogic>();
    }
    public AGILogic Add( byte ResNum, AGILogic NewLogic = null)
    {
      throw new NotImplementedException();
      /*
   'adds a new logic to a currently open game

   Dim agNewLogic As AGILogic
   Dim bytNextNum As Byte
   Dim strID As String

   On Error Resume Next

   'attempt to access this Logic
   If mCol("l" & CStr(ResNum)).Loaded Then
   End If

   'if no error,
   If Err.Number = 0 Then
     'resource already exists
     Err.Clear
     On Error GoTo 0: Err.Raise vbObjectError + 602, strErrSource, LoadResString(602)
     Exit Function
   End If

   'clear error and continue
   Err.Clear
   On Error GoTo ErrHandler

   'create new logic object
   Set agNewLogic = New AGILogic

   'if an object was passed
   If Not (NewLogic Is Nothing) Then
     'copy entire logic
     agNewLogic.SetLogic NewLogic
     'get proposed id
     strID = NewLogic.ID
   Else
     'proposed ID will be default
     strID = "Logic" & CStr(ResNum)
   End If

   '*'Debug.Assert agNewLogic.Loaded

   'initialize the base resource (use placeholders for VOL and Loc - they will be updated by SAVE function)
   agNewLogic.Resource.Init ResNum, -1, -1

   'add it
   mCol.Add agNewLogic, "l" & CStr(ResNum)

   'set id to blank, so
   'error checking in ID property
   'can properly identify if
   'proposed ID is in fact unique
   agNewLogic.FriendID = vbNullString

   On Error Resume Next
   'assign proposed ID
   agNewLogic.ID = strID
   'validate it
   Do Until Err.Number <> vbObjectError + 623
     bytNextNum = bytNextNum + 1
     agNewLogic.ID = strID & "_" & CStr(bytNextNum)
   Loop
   On Error GoTo ErrHandler

   'force flags so save function will work
   agNewLogic.IsDirty = True
   agNewLogic.WritePropState = True

   'save new logic
   agNewLogic.Save

   'return the object created
   Set Add = agNewLogic
   Set agNewLogic = Nothing
 Exit Function

 ErrHandler:
   strError = Err.Description
   strErrSrc = Err.Source
   lngError = Err.Number

   On Error GoTo 0: Err.Raise vbObjectError + 645, strErrSrc, Replace(LoadResString(645), ARG1, CStr(lngError) & ":" & strError)       
        */
    }
    public void Remove(byte Index)
    {
      throw new NotImplementedException();
      /*
   'removes a logic from the game file

   Dim tmpLog As AGILogic
   Dim strKey As String

   On Error GoTo ErrHandler

   'get temp copy of resource
   strKey = "l" & CStr(Index)
   Set tmpLog = mCol.Item(strKey)

   'always unload first
   mCol.Item(strKey).Unload
   'then remove it
   mCol.Remove strKey

   'need to clear the directory file
   UpdateDirFile tmpLog, True

   'remove all properties from the wag file
   On Error Resume Next
   DeleteSettingSection agGameProps, "Logic" & CStr(Index)

 Exit Sub

 ErrHandler:
   'invalid index
   On Error GoTo 0: Err.Raise vbObjectError + 617, strErrSource, LoadResString(617)
       */
    }
    internal void LoadLogic(byte bytResNum, byte bytVol, int lngLoc)
    {
      throw new NotImplementedException();
      /*
        'called by the resource loading method for the initial loading of
        'resources into logics collection

        Dim agNewLogic As AGILogic
        Dim strText As String

        On Error Resume Next

        'attempt to access this Logic
        If mCol("l" & CStr(ResNum)).Loaded Then
        End If

        'if no error,
        If Err.Number = 0 Then
          'resource already exists
          Err.Clear
          On Error GoTo 0: Err.Raise vbObjectError + 602, strErrSource, LoadResString(602)
          Exit Sub
        End If

        'clear error and continue
        Err.Clear
        On Error GoTo ErrHandler

        'create new logic object
        Set agNewLogic = New AGILogic

        'unload it- the resource is already located in the
        'vol file so no need to keep newly created blank logic
        agNewLogic.Resource.Unload

        'initialize it
        agNewLogic.Init ResNum, VOL, Loc

        'add it
        mCol.Add agNewLogic, "l" & CStr(ResNum)
      Exit Sub

      ErrHandler:
        strError = Err.Description
        strErrSrc = Err.Source
        lngError = Err.Number

        Select Case lngError - vbObjectError
        'pass basic resource errors back
        Case 606, 502, 505, 506, 507, 511
          'pass it along
          On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
        Case Else
          'something else; re-frame it
          On Error GoTo 0: Err.Raise vbObjectError + 646, strErrSrc, Replace(LoadResString(646), ARG1, CStr(lngError) & ":" & strError)
        End Select
            */
    }
    public void MarkAllAsDirty()
    {
      foreach (AGILogic tmpLogic in Col)
      {
        tmpLogic.CompiledCRC = 0;
        WriteGameSetting("Logic" + tmpLogic.Number, "CompCRC32", "0x00", "Logics");
      }
      SaveSettingList(agGameProps);
    }
    public void Renumber(byte OldLogic, byte NewLogic)
    {
      throw new NotImplementedException();
      /*
  'renumbers a resource

  Dim tmpLogic As AGILogic
  Dim bytNextNum As Byte
  Dim blnUnload As Boolean
  Dim strSection As String
  
  'if no change
  If OldLogic = NewLogic Then
    Exit Sub
  End If
  
  'verify new number is not in collection
  On Error Resume Next
  If mCol("l" & CStr(NewLogic)).Loaded Then
  End If
  If Err.Number = 0 Then
    'number already in use??
    On Error GoTo 0: Err.Raise vbObjectError + 669, strErrSource, LoadResString(669)
    Exit Sub
  End If
  On Error GoTo ErrHandler
  
  'get logic being renumbered
  Set tmpLogic = mCol("l" & CStr(OldLogic))
  
  'if not loaded,
  If Not tmpLogic.Loaded Then
    tmpLogic.Load
    blnUnload = True
  End If

  'remove old properties
  DeleteSettingSection agGameProps, "Logic" & CStr(OldLogic)
  
  'remove from collection
  mCol.Remove "l" & CStr(OldLogic)
  
  'delete logic from old number in dir file
  'by calling update directory file method
  UpdateDirFile tmpLogic, True
  
  'if id is default
  If tmpLogic.ID = "Logic" & CStr(OldLogic) Then
    'change default ID to new ID
    On Error Resume Next
    tmpLogic.ID = "Logic" & CStr(NewLogic)
    Do Until Err.Number = 0
      bytNextNum = bytNextNum + 1
      tmpLogic.ID = "Logic" & CStr(NewLogic) & "_" & CStr(bytNextNum)
    Loop
    'get rid of existing file with same name as new logic
    Kill agResDir & tmpLogic.ID & agSrcExt
    'rename sourcefile
    Name agResDir & "Logic" & CStr(OldLogic) & agSrcExt As agResDir & tmpLogic.ID & agSrcExt
    On Error GoTo ErrHandler
  End If
  
  'change number
  tmpLogic.Resource.Number = NewLogic
  
  'add with new number
  mCol.Add tmpLogic, "l" & CStr(NewLogic)
  
  'update new logic number in dir file
  UpdateDirFile tmpLogic
  
  'add properties back with new logic number
  strSection = "Logic" & CStr(NewLogic)
  WriteGameSetting strSection, "ID", tmpLogic.ID, "Logics"
  WriteGameSetting strSection, "Description", tmpLogic.Description
  WriteGameSetting strSection, "CRC32", "&H" & Hex$(tmpLogic.CRC)
  WriteGameSetting strSection, "CompCRC32", "&H" & Hex$(tmpLogic.CompiledCRC)
  WriteGameSetting strSection, "IsRoom", CStr(tmpLogic.IsRoom)
  
  'force writeprop state back to false
  tmpLogic.WritePropState = False
  
  'unload if necessary
  If blnUnload Then
    tmpLogic.Unload
  End If
Exit Sub

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  On Error GoTo 0: Err.Raise vbObjectError + 670, strErrSrc, Replace(LoadResString(670), ARG1, CStr(lngError) & ":" & strError)
      */
    }
    public string ConvertArg(string ArgIn, ArgTypeEnum ArgType, bool VarOrNum = false)
    {
      throw new NotImplementedException();
      /*
   'tie function to allow access to the LogCompile variable conversion function

   Dim dtFileMod As Date

   On Error Resume Next
   'if in a game
   If agMainGame.GameLoaded Then
     'initialize global defines
     'get datemodified property
     dtFileMod = FileLastMod(agGameDir & "globals.txt")
     If CRC32(StrConv(CStr(dtFileMod), vbFromUnicode)) <> agGlobalCRC Then
       GetGlobalDefines
     End If

     'if ids not set yet
     If Not blnSetIDs Then
       SetResourceIDs
     End If
   End If

   'convert argument
   ConvertArgument ArgIn, ArgType, VarOrNum
   'return it
   ConvertArg = LCase$(ArgIn)
       */
    }
  }
}