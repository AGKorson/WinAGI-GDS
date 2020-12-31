using System;
using System.ComponentModel;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;

namespace WinAGI
{
  public class AGISound : AGIResource
  {
    public AGISound() : base(AGIResType.rtSound, "NewSound")
    {
      //initialize
      //attach events
      base.PropertyChanged += ResPropChange;
      strErrSource = "WinAGI.Sound";
      //set default resource data
      Data = new RData(0);// ();
    }
    internal void InGameInit(byte ResNum, sbyte VOL, int Loc)
    {
      //this internal function adds this resource to a game, setting its resource 
      //location properties, and reads properties from the wag file

      //set up base resource
      base.InitInGame(ResNum, VOL, Loc);

      //if first time loading this game, there will be nothing in the propertyfile
      ID = ReadSettingString(agGameProps, "Sound" + ResNum, "ID", "");
      if (ID.Length == 0)
      {
        //no properties to load; save default ID
        ID = "Sound" + ResNum;
        WriteGameSetting("Logic" + ResNum, "ID", ID, "Sounds");
        //load resource to get size
        Load();
        WriteGameSetting("Sound" + ResNum, "Size", Size.ToString());
        Unload();
      }
      else
      {
        //get description, size and other properties from wag file
        mDescription = ReadSettingString(agGameProps, "Sound" + ResNum, "Description", "");
        Size = ReadSettingLong(agGameProps, "Sound" + ResNum, "Size", -1);
      }
    }
    private void ResPropChange(object sender, AGIResPropChangedEventArgs e)
    {
      ////let's do a test
      //// increment number everytime data changes
      //Number++;
    }
    internal void SetSound(AGISound newSound)
    {
      throw new NotImplementedException();
    }

    void tmpSound()
    {
      /*
Option Explicit
Option Compare Text

Implements AGIResource

Private WithEvents agRes As AGIResource
Private mResID As String
Private mDescription As String
Private mIsDirty As Boolean
Private mWriteProps As Boolean

Private mTrack(3) As AGITrack
Private mTracksSet As Boolean
Private mLength As Single
Private mKey As Long
Private mTPQN As Long
Private mFormat As Long

'flag to note tracks match res data

'variables to support MIDI file creation
Private mMIDISet As Boolean
Private mMIDIData() As Byte

'other
Private strErrSource As String

Public Event SoundComplete(NoError As Boolean)


Private Sub BuildSoundOutput()

  'creates midi/wav output data stream for this sound resource
  
  On Error GoTo ErrHandler
  
  Select Case mFormat
  Case 1 'standard pc/pcjr sound
    'build the midi first
    mMIDIData = BuildMIDI(Me)
    'get length
    mLength = GetSoundLength()
  
  Case 2  'IIgs pcm sound
    mMIDIData = BuildIIgsPCM(Me)
    'get length
    mLength = GetSoundLength()
    
  Case 3 'IIgs MIDI sound
    'build the midi data and get length info
    mMIDIData = BuildIIgsMIDI(Me, mLength)
    
  End Select
  
  'set flag
  mMIDISet = True
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Err.Clear
  On Error GoTo 0: Err.Raise vbObjectError + 596, strErrSource, LoadResString(596)
End Sub

Private Function GetSoundLength() As Single
  
  Dim i As Long
  
  'this function assumes a sound has been loaded properly
  
  'get length
  Select Case mFormat
  Case 1 'standard pc/pcjr resource
    For i = 0 To 3
      If GetSoundLength < mTrack(i).Length And Not mTrack(i).Muted Then
        GetSoundLength = mTrack(i).Length
      End If
    Next i
  
  Case 2  'pcm sampling
    'since sampling is at 8000Hz, size is just data length/8000
    GetSoundLength = (agRes.Size - 54) / 8000
    
  Case 3 'iigs midi
    'length has to be calculated during midi build
    '*'Debug.Assert Not mMIDISet
    BuildSoundOutput
    GetSoundLength = mLength
    
  End Select
  'does 0 work?
End Function

Public Property Let Key(ByVal NewKey As Long)
  
  'validate
  If NewKey < -7 Or NewKey > 7 Then
    'raise error
    On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
    Exit Property
  End If
  
  If mKey <> NewKey Then
    'assign it
    mKey = NewKey
    'set props flag
    mWriteProps = True
  End If
End Property
Public Property Get Key() As Long
  
  Key = mKey
End Property

Public Property Get SndFormat() As Long
  
  '  0 = not loaded
  '  1 = 'standard' agi
  '  2 = IIgs sampled sound
  '  3 = IIgs midi
  
  If agRes.Loaded Then
    SndFormat = mFormat
  Else
    SndFormat = 0
  End If
End Property

Friend Sub LoadTracks()

  On Error GoTo ErrHandler
  
  Dim i As Long, lngLength As Long, lngTLength As Long
  Dim lngTrackStart As Long, lngTrackEnd As Long
  Dim lngStart As Long, lngEnd As Long, lngResPos As Long
  Dim intFreq As Integer, lngDur As Long, intVol As Integer
  
'''  For i = 0 To 3
'''    'clear out tracks by assigning to nothing, then new
'''    Set mTrack(i) = Nothing
'''    Set mTrack(i) = New AGITrack
'''    'set parent
'''    mTrack(i).SetParent Me
'''  Next i
  
  'if in a game,
  If agRes.InGame Then
    'get track properties from the .WAG file
    mTrack(0).Instrument = ReadSettingByte(agGameProps, "Sound" & CStr(agRes.Number), "Inst0", 80)
    mTrack(1).Instrument = ReadSettingByte(agGameProps, "Sound" & CStr(agRes.Number), "Inst1", 80)
    mTrack(2).Instrument = ReadSettingByte(agGameProps, "Sound" & CStr(agRes.Number), "Inst2", 80)
    mTrack(0).Muted = ReadSettingBool(agGameProps, "Sound" & CStr(agRes.Number), "Mute0", False)
    mTrack(1).Muted = ReadSettingBool(agGameProps, "Sound" & CStr(agRes.Number), "Mute1", False)
    mTrack(2).Muted = ReadSettingBool(agGameProps, "Sound" & CStr(agRes.Number), "Mute2", False)
    mTrack(3).Muted = ReadSettingBool(agGameProps, "Sound" & CStr(agRes.Number), "Mute3", False)
    mTrack(0).Visible = ReadSettingBool(agGameProps, "Sound" & CStr(agRes.Number), "Visible0", True)
    mTrack(1).Visible = ReadSettingBool(agGameProps, "Sound" & CStr(agRes.Number), "Visible1", True)
    mTrack(2).Visible = ReadSettingBool(agGameProps, "Sound" & CStr(agRes.Number), "Visible2", True)
    mTrack(3).Visible = ReadSettingBool(agGameProps, "Sound" & CStr(agRes.Number), "Visible3", True)
  Else
    For i = 0 To 3
      mTrack(i).Visible = True
    Next i
  End If
  
 'extract note information for each track from resource

  'write the sound tracks
  For i = 0 To 2
    'reset length for this track
    lngTLength = 0
    'get start and end of this track (stored at beginning of resource
    'in LSMS format)
    '  track 0 start is byte 0-1, track 1 start is byte 2-3
    '  track 2 start is byte 4-5, noise start is byte 6-7
    lngStart = agRes.Data(i * 2 + 0) + 256& * agRes.Data(i * 2 + 1)
    'end is start of next track -5 (5 bytes per note in each track) -2 (trailing &HFFFF)
    lngEnd = agRes.Data(i * 2 + 2) + 256& * agRes.Data(i * 2 + 3) - 7
    'validate
    If lngStart < 0 Or lngEnd < 0 Or lngStart > agRes.Size Or lngEnd > agRes.Size Then
      'raise error
      On Error Resume Next
      On Error GoTo 0: Err.Raise vbObjectError + 598, strErrSource, LoadResString(598)
      Exit Sub
    End If
        
    'step through notes in this track (5 bytes at a time)
    For lngResPos = lngStart To lngEnd Step 5
      'get duration
      lngDur = (agRes.Data(lngResPos) + 256& * agRes.Data(lngResPos + 1))
      
      'get frequency
      intFreq = 16 * (agRes.Data(lngResPos + 2) And &H3F) + (agRes.Data(lngResPos + 3) And &HF)
      'attenuation information in byte5
      intVol = agRes.Data(lngResPos + 4) And &HF
      'add the note
      mTrack(i).Notes.Add intFreq, lngDur, intVol
      'add length
      lngTLength = lngTLength + lngDur
    Next lngResPos
    
    'if this is longest length
    If lngTLength > lngLength Then
      lngLength = lngTLength
    End If
  Next i
  
  lngTLength = 0
  'getstart and end of noise track
  lngStart = agRes.Data(6) + 256& * agRes.Data(7)
  lngEnd = agRes.Size - 7
  For lngResPos = lngStart To lngEnd Step 5
    'First and second byte: Note duration
    lngDur = (agRes.Data(lngResPos) + 256& * agRes.Data(lngResPos + 1))
    'get freq divisor (first two bits of fourth byte)
    'and noise type (3rd bit) as a single number
    intFreq = (agRes.Data(lngResPos + 3) And 7)
    'Fifth byte: volume attenuation
    intVol = agRes.Data(lngResPos + 4) And &HF
    
    'if duration>0
    If lngDur > 0 Then
      'add the note
      mTrack(3).Notes.Add intFreq, lngDur, intVol
      'add to length
      lngTLength = lngTLength + lngDur
    End If
  Next lngResPos
  
  'if this is longest length
  If lngTLength > lngLength Then
    lngLength = lngTLength
  End If
  
  'save length
  'based on original playsound, agi sound tick is 1/64 sec
  mLength = lngLength / 64

  'based on NAGI, and by listening to sounds played on interpreter
  mLength = lngLength / 60
  
  'set flag to indicate tracks loaded
  mTracksSet = True
  'MUST be clean, since loaded from resource data
  mIsDirty = False
Exit Sub

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  '*'Debug.Assert False
  
  On Error GoTo 0: Err.Raise vbObjectError + 565, strErrSrc, Replace(LoadResString(565), ARG1, CStr(lngError) & ":" & strError)
End Sub
Private Sub CompileSound()
  'compiles this sound by converting notes into
  'an AGI resource datastream
  
  Dim i As Long, j As Long
  Dim tmpRes As AGIResource
  
  On Error GoTo ErrHandler
  
  Set tmpRes = New AGIResource
  
  With tmpRes
    .NewResource
    
    'build header
    .WriteWord 8, 0
    i = 0
    For j = 0 To 2
      i = i + mTrack(j).Notes.Count * 5 + 2
      .WriteWord 8 + i
    Next j
    
    'add regular tracks
    For j = 0 To 2
      For i = 0 To mTrack(j).Notes.Count - 1
        'write duration
        .WriteWord mTrack(j).Notes(i).Duration
        'add frequency bytes
        .WriteByte mTrack(j).Notes(i).FreqDivisor \ 16
        .WriteByte (mTrack(j).Notes(i).FreqDivisor Mod 16) + 128 + 32 * j
        'add attenuation
        .WriteByte mTrack(j).Notes(i).Attenuation + 144 + 32 * j
      Next i
      'add end of track
      .WriteByte &HFF
      .WriteByte &HFF
    Next j
    
    'write noise track
    For i = 0 To mTrack(3).Notes.Count - 1
      'write duration
      .WriteWord mTrack(3).Notes(i).Duration
      'write placeholder
      .WriteByte 0
      'write type and freq
      .WriteByte 224 + mTrack(3).Notes(i).FreqDivisor
      'add attenuation
      .WriteByte mTrack(3).Notes(i).Attenuation + 240
    Next i
    'add end of track
    .WriteByte &HFF
    .WriteByte &HFF
    
    'assign data to resource
    agRes.AllData = .AllData
  End With
  Set tmpRes = Nothing
  
  'set tracksloaded flag
  mTracksSet = True
Exit Sub

ErrHandler:
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  On Error GoTo 0: Err.Raise vbObjectError + 566, strErrSrc, Replace(LoadResString(566), ARG1, CStr(lngError) & ":" & strError)
End Sub

Friend Sub NoteChanged()
  'called by child notes to indicate a change
  'has occured
  
  'change in note forces midiset to false
  mMIDISet = False
  
  'change in note sets dirty flag to true
  mIsDirty = True
  
'  'change in note sets compiled flag to false
'  mTracksSet = False
  
  'and resets sound length
  mLength = -1
End Sub
Friend Sub SetFlags(ByRef IsDirty As Boolean, ByRef TracksSet As Boolean, ByRef WriteProps As Boolean)
  
  IsDirty = mIsDirty
  TracksSet = mTracksSet
  WriteProps = mWriteProps
End Sub
Public Sub Clear()
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  If Not agRes.Loaded Then
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  'clear all tracks
  For i = 0 To 3
    'clear out tracks by assigning to nothing, then new
    Set mTrack(i) = Nothing
    Set mTrack(i) = New AGITrack
    'set parent
    mTrack(i).SetParent Me
    'set track defaults
    mTrack(i).Instrument = 0
    mTrack(0).Muted = False
    mTrack(0).Visible = True
  Next i
  
  
  'set dirty flag
  mIsDirty = True
  
  'reset length
  mLength = -1
  
  'clear the resource
  agRes.Clear
  'set all track pointers to position 8
  agRes.WriteWord 8, 0
  agRes.WriteWord 8
  agRes.WriteWord 8
  agRes.WriteWord 8
  'set no data at position 8 (two &HFF bytes)
  agRes.WriteByte &HFF
  agRes.WriteByte &HFF
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Property Get Length() As Single
  'returns length of sound in seconds
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'if not loaded,
  If Not agRes.Loaded Then
    Length = -1
    Exit Property
  End If
  
  'if length is changed
  If mLength = -1 Then
    mLength = GetSoundLength()
  End If
  
  'length is max length of tracks
  Length = mLength
Exit Property

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Property

Public Sub PlaySound()
  'plays sound asynchronously by generating a MIDI stream
  'that is fed to a MIDI output
  
  'in order to raise event at end of sound,
  'need to subclass a form
  
  On Error Resume Next
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  'if sound is already open
  If blnPlaying Then
    On Error GoTo 0: Err.Raise vbObjectError + 629, strErrSource, LoadResString(629)
    Exit Sub
  End If

  'dont need to worry if tracks are properly loaded because
  'changing track data causes mMIDISet to be reset; this forces
  'midi rebuild, which references the Tracks throught the Track
  'property, which forces rebuild of track data when the
  'BUILDMIDI method first access the track property

  'if sound data not set to play
  If Not mMIDISet Then
    BuildSoundOutput
    'if error,
    If Err.Number <> 0 Then
      'pass error along
      lngError = Err.Number
      strError = Err.Description
      strErrSrc = Err.Source
      On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    End If
  End If
  
'  'if nothing to play
'  If mLength = 0 Then
'    'exit
'    Exit Sub
'  End If
  
  'play the sound
  frmSndSubclass.PlaySound Me
  'if error,
  If Err.Number <> 0 Then
    'pass error along
    lngError = Err.Number
    strError = Err.Description
    strErrSrc = Err.Source
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
  End If
End Sub

Friend Sub RaiseSoundComplete(ByVal NoError As Boolean)

  'this is a tie over method to raise the soundcomplete event
  RaiseEvent SoundComplete(NoError)
End Sub

Public Sub SetSound(CopySound As AGISound)
  'copies sound data from CopySound into this sound
  
  Dim i As Long, j As Long
  
  On Error GoTo ErrHandler
  
  With CopySound
    'add WinAGI items
    mResID = .ID
    mDescription = .Description
    mKey = .Key
    mTPQN = .TPQN
    
    'add resource data
    agRes.SetRes .Resource
    
    'if loaded
    If .Loaded Then
      agRes.AllData = .Resource.AllData
    End If
    
    'build regular tracks
    For i = 0 To 2
      'set track properties
      mTrack(i).SetParent Me
      mTrack(i).Muted = .Track(i).Muted
      mTrack(i).Visible = .Track(i).Visible
      'clear notes
      mTrack(i).Notes.Clear
      'add new notes
      For j = 0 To .Track(i).Notes.Count - 1
        mTrack(i).Notes.Add .Track(i).Notes(j).FreqDivisor, .Track(i).Notes(j).Duration, .Track(i).Notes(j).Attenuation
      Next j
      mTrack(i).Instrument = .Track(i).Instrument
    Next i
    
    'copy noise track
    mTrack(3).SetParent Me
    mTrack(3).Muted = .Track(3).Muted
    mTrack(3).Visible = .Track(3).Visible
    
    'clear notes
    mTrack(3).Notes.Clear
    'add new notes
    For j = 0 To .Track(3).Notes.Count - 1
      mTrack(3).Notes.Add .Track(3).Notes(j).FreqDivisor, .Track(3).Notes(j).Duration, .Track(3).Notes(j).Attenuation
    Next j
    
    'never build midi;
    'first reference to MIDI will force rebuild
    'at that time
    mMIDISet = False
    'set track loaded property to true
    mTracksSet = True
    'reset length
    mLength = -1
    
    'copy flags
    .SetFlags mIsDirty, mTracksSet, mWriteProps
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  
  strError = Err.Description
  strErrSrc = Err.Source
  lngError = Err.Number
  
  On Error GoTo 0: Err.Raise vbObjectError + 567, strErrSrc, Replace(LoadResString(567), ARG1, CStr(lngError) & ":" & strError)
End Sub


Public Property Get Description() As String
  Description = mDescription
End Property


Public Property Let Description(ByVal NewDescription As String)

  'limit description to 1K
  NewDescription = Left$(NewDescription, 1024)
  
  'if changing
  If NewDescription <> mDescription Then
    mDescription = NewDescription
    If agRes.InGame Then
      WriteGameSetting "Sound" & CStr(agRes.Number), "Description", mDescription, "Sounds"
    End If
  End If
End Property

Public Sub Export(ExportFile As String, Optional ByVal FileFormat As SoundFormat = sfAGI, Optional ByVal ResetDirty As Boolean = True)
  
  Dim intFile As Integer
  Dim i As Long, j As Long
  Dim lngType As Long, lngFreq As Long
  
  On Error GoTo ErrHandler
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  'if format is not predefined
  If FileFormat <= sfUndefined Or FileFormat > 3 Then
    'need to determine type from filename
    If StrComp(Right$(ExportFile, 4), ".ass", vbTextCompare) = 0 Then
      'script
      FileFormat = sfScript
    ElseIf StrComp(Right$(ExportFile, 4), ".mid", vbTextCompare) = 0 Then
      'midi
      FileFormat = sfMIDI
    ElseIf StrComp(Right$(ExportFile, 4), ".wav", vbTextCompare) = 0 Then
      'wav
      FileFormat = sfWAV
    Else
      'default to agi
      FileFormat = sfAGI
    End If
  End If
  
  Select Case FileFormat
  Case sfAGI 'all data formats OK
    'if dirty
    If mIsDirty Then
      'need to recompile
      CompileSound
    End If
    
    'export agi resource
    agRes.Export ExportFile
    
    If Not agRes.InGame Then
      If ResetDirty Then
        'clear dirty flag
        mIsDirty = False
      End If
    End If
    
  Case sfMIDI 'pc and IIgs midi
    On Error Resume Next
    'if wrong format
    If mFormat = 2 Then
      On Error GoTo 0: Err.Raise vbObjectError + 596, strErrSource, "Can't export PCM formatted resource as MIDI file"
      Exit Sub
    End If
    
    'if midi not set
    If Not mMIDISet Then
      'build the midi first
      BuildSoundOutput
    End If
    
    'delete any existing file
    Kill ExportFile
    Err.Clear
    
    'create midi file
    intFile = FreeFile()
    Open ExportFile For Binary As intFile
    Put intFile, 1, mMIDIData
    Close intFile
      
  Case sfScript 'pc only
    'if wrong format
    If mFormat <> 1 Then
      On Error GoTo 0: Err.Raise vbObjectError + 596, strErrSource, "Only PC/PCjr sound resources can be exported as script files"
      Exit Sub
    End If
       
    'if dirty
    If mIsDirty Then
      'need to recompile
      CompileSound
    End If
    
    On Error Resume Next
    'delete any existing file
    Kill ExportFile
    Err.Clear
    
    'creat script file
    intFile = FreeFile()
    Open ExportFile For Output As intFile
    
    'if sound not set,
    If Not mTracksSet Then
      'load tracks first
      LoadTracks
    End If
    
    'add comment header
    Print #intFile, "# agi script file"
    Print #intFile,
    Print #intFile, "##Description=" & mDescription
    Print #intFile, "##TPQN=" & mTPQN
    Print #intFile,
    
    'add sound tracks
    For i = 0 To 2
      Print #intFile, "# track " & CStr(i)
      Print #intFile, "tone"
      For j = 0 To mTrack(i).Notes.Count - 1
        'if first note,
        If j = 0 Then
          'include tone type
          Print #1, "a, " & mTrack(i).Notes(0).FreqDivisor & ", " & mTrack(i).Notes(0).Attenuation & ", " & mTrack(i).Notes(0).Duration
        Else
          'don't need tone type
          Print #1, mTrack(i).Notes(j).FreqDivisor & ", " & mTrack(i).Notes(j).Attenuation & ", " & mTrack(i).Notes(j).Duration
        End If
      Next j
      'add instrument, visible and muted properties
      Print #intFile, "##instrument" & CStr(i) & "=" & CStr(mTrack(i).Instrument)
      Print #intFile, "##visible" & CStr(i) & "=" & CStr(Track(i).Visible)
      Print #intFile, "##muted" & CStr(i) & "=" & CStr(mTrack(i).Muted)
      Print #intFile,
    Next i
    
    'add noise track
    Print #intFile, "# track 3"
    Print #intFile, "noise"
    For j = 0 To mTrack(3).Notes.Count - 1
      'if note is white noise(bit 2 of freq is 1)
      If mTrack(3).Notes(j).FreqDivisor And 4 Then
        'white noise
        Print #intFile, "w," & CStr(mTrack(3).Notes(j).FreqDivisor And 3) & ", " & mTrack(3).Notes(j).Attenuation & ", " & mTrack(3).Notes(j).Duration
      Else
        Print #intFile, "p," & CStr(mTrack(3).Notes(j).FreqDivisor And 3) & ", " & mTrack(3).Notes(j).Attenuation & ", " & mTrack(3).Notes(j).Duration
      End If
    Next j
    'add visible and muted properties
    Print #intFile, "##visible3=" & CStr(Track(3).Visible)
    Print #intFile, "##muted3=" & CStr(mTrack(3).Muted)
    
    'close the file
    Close #intFile
    
    If ResetDirty Then
      'clear dirty flag
      mIsDirty = False
    End If
    
  Case sfWAV 'IIgs pcm only
    On Error Resume Next
    'if wrong format
    If mFormat <> 2 Then
      On Error GoTo 0: Err.Raise vbObjectError + 596, strErrSource, "Can't export MIDI formatted sound resource as .WAV file"
      Exit Sub
    End If
     
    'if data not set
    If Not mMIDISet Then
      'build the wav first
      'build the midi first
      BuildSoundOutput
    End If
    
    'delete any existing file
    Kill ExportFile
    Err.Clear
    
    'create midi file
    intFile = FreeFile()
    Open ExportFile For Binary As intFile
    Put intFile, 1, mMIDIData
    Close intFile
  
  End Select
  
  'if not in a game,
  If Not agRes.InGame Then
    'ID always tracks the resfile name
    mResID = JustFileName(ExportFile)
    If Len(mResID) > 64 Then
      mResID = Left$(mResID, 64)
    End If
  End If
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub


Public Property Get ID() As String
  
  ID = mResID
End Property


Public Property Let ID(NewID As String)
  'sets the ID for a resource;
  'resource IDs must be unique to each resource type
  'max length of ID is 64 characters
  'min of 1 character
  
  Dim tmpSnd As AGISound
  
  Err.Clear
  On Error GoTo ErrHandler
  
  'validate length
  If LenB(NewID) = 0 Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 667, strErrSource, LoadResString(667)
    Exit Property
  ElseIf Len(NewID) > 64 Then
    NewID = Left$(NewID, 64)
  End If
  
  'if changing,
  If StrComp(NewID, mResID, vbBinaryCompare) <> 0 Then
    'if in a game
    If agRes.InGame Then
      'step through other resources
      For Each tmpSnd In agSnds
        'if resource IDs are same
        If tmpSnd.ID = NewID Then
          'if not same sound,
          If tmpSnd.Number <> agRes.Number Then
            'error
            On Error GoTo 0: Err.Raise vbObjectError + 623, strErrSource, LoadResString(623)
            Exit Property
          End If
        End If
      Next
    End If
    
    'save ID
    mResID = NewID
    If agRes.InGame Then
      WriteGameSetting "Sound" & CStr(agRes.Number), "ID", mResID, "Sounds"
    End If
    
    'reset compiler list of ids
    blnSetIDs = False
  End If
Exit Property

ErrHandler:
  lngError = Err.Number
  strErrSrc = Err.Source
  strError = Err.Description
  
  '*'Debug.Assert False
  On Error GoTo 0: Err.Raise vbObjectError + 686, strErrSrc, Replace(Replace(LoadResString(686), ARG1, "Logic"), ARG2, strError)
End Property


Friend Property Let FriendID(NewID As String)

  'sets the ID for a resource internally
  'does not validate the ID or set dirty flag
  
  'save ID
  mResID = NewID
End Property


Public Sub Import(ImportFile As String)
  
  'imports a sound resource
  
  Dim intFile As Integer, intData As Integer
  Dim bytData As Byte, strLine As String
  Dim strLines() As String, i As Long
  Dim strTag() As String, lngTrack As Long
  Dim lngNoteType As Long, blnError As Boolean
  Dim intFreq As Integer, lngDur As Long, intVol As Integer
  
  On Error Resume Next
  
  'determine file format by checking for '8'-'0' start to file
  '(that is how all sound resources will begin)
  intFile = FreeFile()
  Open ImportFile For Binary As intFile
  'verify long enough
  If LOF(intFile) <= 2 Then
    'error
    Close intFile
    On Error GoTo 0: Err.Raise vbObjectError + 681, strErrSource, LoadResString(681)
    Exit Sub
  End If
  
  'set key and tpqn defaults
  mTPQN = 16
  mKey = 0
  
  On Error GoTo ErrHandler
  
  'set ID
  mResID = JustFileName(ImportFile)
  If Len(mResID) > 64 Then
    mResID = Left$(mResID, 64)
  End If
  
  'get integer Value at beginning of file
  Get intFile, 1, intData
  Close intFile
  
  'if sound resource, intFile =8
  If intData = 8 Then
    'import the resource
    agRes.Import ImportFile
    
    'if there was an error,
    If Err.Number <> 0 Then
      'pass along error (by exiting without clearing)
      Exit Sub
    End If
    
    'load the notes into the tracks
    LoadTracks
  Else
    'must be a script
    
    'clear the resource
    Clear
    lngTrack = -1
    
    'import a script file
    Open ImportFile For Binary As intFile
    
    'get data from file
    strLine = Space$(LOF(intFile))
    Get intFile, 1, strLine
    Close intFile
    
    'replace crlf with cr only
    strLine = Replace(strLine, vbCrLf, vbCr)
    'replace lf with cr
    strLine = Replace(strLine, vbLf, vbCr)
    'split based on cr
    strLines = Split(strLine, vbCr)
    i = 0
    
    Do Until i > UBound(strLines)
    
      'get next line
      strLine = Trim$(strLines(i))
      
      'check for winagi tags
      If Left$(strLine, 2) = "##" Then
        'split the line into tag and Value
        strTag = Split(strLine, "=")
        'should only be two
        If UBound(strTag) = 1 Then
          'what is the tag?
          Select Case Trim$(strTag(0))
          Case "##Description"
            'use this description
            mDescription = strTag(1)
            
          Case "##instrument0"
            'set instrument
            mTrack(0).Instrument = CByte(strTag(1))
            
          Case "##instrument1"
            'set instrument
            mTrack(1).Instrument = CByte(strTag(1))
            
          Case "##instrument2"
            'set instrument
            mTrack(2).Instrument = CByte(strTag(1))
            
          Case "##visible0"
            mTrack(0).Visible = CBool(strTag(1))
            
          Case "##visible1"
            mTrack(1).Visible = CBool(strTag(1))
            
          Case "##visible2"
            mTrack(2).Visible = CBool(strTag(1))
            
          Case "##visible3"
            mTrack(3).Visible = CBool(strTag(1))
            
          Case "##muted0"
            mTrack(0).Muted = CBool(strTag(1))
            
          Case "##muted1"
            mTrack(1).Muted = CBool(strTag(1))
            
          Case "##muted2"
            mTrack(2).Muted = CBool(strTag(1))
            
          Case "##muted3"
            mTrack(3).Muted = CBool(strTag(1))
          Case "##tpqn"
            mTPQN = (CLng(strTag(1)) \ 4) * 4
            If mTPQN < 4 Then
              mTPQN = 4
            ElseIf mTPQN > 64 Then
              mTPQN = 64
            End If
            
          Case "##key"
            mKey = CLng(strTag(1))
            If mKey < -7 Then
              mKey = -7
            ElseIf mKey > 7 Then
              mKey = 7
            End If
            
          End Select
        End If
      Else
        'ignore blank lines, and commented lines
        Do
          If LenB(strLine) = 0 Then
            Exit Do
          End If
          If AscW(strLine) = 35 Then
            Exit Do
          End If

          'check for new track
          If strLine = "tone" Then
            lngTrack = lngTrack + 1
            'default note type is agi (0)
            lngNoteType = 0
            'show track
            mTrack(lngTrack).Visible = True
            
          ElseIf strLine = "noise" And lngTrack <> 3 Then
            lngTrack = 3
            'no default note type for track 3
            lngNoteType = -1
            'show track
            mTrack(3).Visible = True
            
          Else
            'verify there is a valid track
            If lngTrack < 0 Or lngTrack > 3 Then
              'invalid sound resource;
              blnError = True
              Exit Do
            End If

            'split line using commas
            strTag = Split(strLine, ",")

            'should only be three or four elements
            Select Case UBound(strTag())
            Case Is >= 3 'if four elements (or more; extras are ignored)
              'check first element for new note type
              If lngTrack = 3 Then
                'p' or 'w' only
                If strTag(0) = "p" Then
                  lngNoteType = 0
                ElseIf strTag(0) = "w" Then
                  lngNoteType = 4
                Else
                  'error
                  blnError = True
                  Exit Do
                End If

                'calculate freq Value
                intFreq = CInt(Val(strTag(1))) Or lngNoteType

              Else
                'a' or 'f' only
                If strTag(0) = "a" Then
                  'agi freq index is the Value passed
                  intFreq = CInt(Val(strTag(1)))

                ElseIf strTag(0) = "f" Then
                  'a real freq Value was passed
                  intFreq = CInt(Val(strTag(1)))
                  'can't be zero
                  If intFreq = 0 Then
                    blnError = True
                    Exit Do
                  End If
                  'convert
                  intFreq = CInt(CSng(intFreq) / 111860!)

                Else
                  'error
                  blnError = True
                  Exit Do
                End If
              End If
              
              'calculate volume and duration
              intVol = CInt(Val(strTag(2)))
              lngDur = CLng(Val(strTag(3)))

            Case 2 'if three elements
              '0, 1, 2 - assume note type 'a'
              '3 - use previous note type
              If lngTrack <> 3 Then
                intFreq = CInt(Val(strTag(0)))
              
              Else
                'track three
                'if no type yet,
                If lngNoteType = -1 Then
                  blnError = True
                  Exit Do
                End If

                'calculate freq Value
                intFreq = CInt(Val(strTag(0))) Or lngNoteType
              End If

              'calculate volume and duration
              intVol = CInt(Val(strTag(1)))
              lngDur = CLng(Val(strTag(2)))

            Case Else
              'error
              blnError = True
              Exit Do
            End Select

            'validate input
            If intFreq < 0 Or intFreq >= 1024 Or intVol < 0 Or intVol >= 16 Or lngDur < 0 Or lngDur > 65535 Then
              'invalid data
              blnError = True
              Exit Do
            End If
            
            'duration of zero is not an error, but it is ignored
            If lngDur <> 0 Then
              'add the note to current track
              mTrack(lngTrack).Notes.Add intFreq, lngDur, intVol
            End If
          End If
        Loop Until True
        
        'if error,
        If blnError Then
          Clear
          'reset no id
          mResID = vbNullString
          mDescription = vbNullString
          'raise the error
          On Error GoTo 0: Err.Raise vbObjectError + 681, strErrSource, LoadResString(681)
          Exit Sub
        End If
      End If
      
      'increment line counter
      i = i + 1
    Loop
    
    'if there was an error,
    If Err.Number <> 0 Then
      'pass along error (by exiting without clearing)
      Exit Sub
    End If
    
    'compile the sound so the resource matches the tracks
    CompileSound
  End If

  If Err.Number <> 0 Then
    'pass along error (by exiting without clearing it)
    Exit Sub
  End If
  
  'reset dirty flags
  mIsDirty = False
  mWriteProps = False
  
  'reset track flag
  mTracksSet = True
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Friend Property Let IsDirty(ByVal NewState As Boolean)

  mIsDirty = NewState
End Property

Public Property Let TPQN(NewTPQN As Long)

  'validate it
  mTPQN = (mTPQN \ 4) * 4
  If mTPQN < 4 Or mTPQN > 64 Then
    'raise error
    On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
    Exit Property
  End If
  
  If mTPQN <> NewTPQN Then
    mTPQN = NewTPQN
    mWriteProps = True
  End If
End Property

Public Property Get TPQN() As Long

  TPQN = mTPQN
End Property

Public Property Get Track(ByVal Index As Long) As AGITrack
  
  On Error Resume Next
  
  'validate index
  If Index < 0 Or Index > 3 Then
    On Error GoTo 0: Err.Raise 9, strErrSource, "Index out of bounds"
    Exit Property
  End If
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Property
  End If
  
  'if sound not set,
  If Not mTracksSet Then
    'load tracks first
    LoadTracks
  End If
  
  'return any errors
  If Err.Number <> 0 Then
    'pass error along
    lngError = Err.Number
    strError = Err.Description
    strErrSrc = Err.Source
    
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    Exit Property
  End If
  
  'return the loop collection
  Set Track = mTrack(Index)
End Property

Friend Sub TrackChanged(Optional ByVal ResetMIDI As Boolean = True)

  'called by sound tracks to indicate a change
  'has occured; ResetMIDI flag allows some track changes to
  'occur that don't affect the MIDI data (such as Visible)
  'but still set the writeprops flag
  
  'when track status changes, need to recalculate length
  mLength = -1
  
  If ResetMIDI Then
    'change in track forces midiset to false
    mMIDISet = False
  End If
  
  'change in track sets writeprop to true
  mWriteProps = True
End Sub

Public Property Get IsDirty() As Boolean
    
  'if resource is dirty, or (prop values need writing AND in game)
  'IsDirty = (mIsDirty Or (mWriteProps And agRes.InGame))
  IsDirty = (mIsDirty Or mWriteProps)
End Property
Public Sub Load()
  
  Dim lngFormat As Long, i As Long
  
  On Error Resume Next
  
  'if resource data not loaded yet,
  If Not agRes.Loaded Then
    'load resource
    agRes.Load
  End If
  
  If Err.Number <> 0 Then
    'pass along the error
    lngError = Err.Number
    strErrSrc = Err.Source
    strError = Err.Description
    
    Unload
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
  End If
  
  'if there is a song key signature, get it
  mKey = ReadSettingLong(agGameProps, "Sound" & CStr(agRes.Number), "Key", 0)
  'validate it
  If mKey < -7 Or mKey > 7 Then
    mKey = 0
  End If
  
  'get ticks per quarternote
  mTPQN = ReadSettingLong(agGameProps, "Sound" & CStr(agRes.Number), "TPQN", 0)
  'validate it
  mTPQN = (mTPQN \ 4) * 4
  If mTPQN < 4 Then
    mTPQN = 4
  End If
  If mTPQN > 64 Then
    mTPQN = 64
  End If
  
  For i = 0 To 3
    'clear out tracks by assigning to nothing, then new
    Set mTrack(i) = Nothing
    Set mTrack(i) = New AGITrack
    'set parent
    mTrack(i).SetParent Me
  Next i
  
  'check header to determine what type of sound resource;
  '   0x01 = IIgs sampled sound
  '   0x02 = IIgs midi sound
  '   0x08 = PC/PCjr 'standard'
  
  Select Case agRes.ReadWord(0)
  Case 1 'IIgs sampled sound
    mFormat = 2
    'tracks are not applicable, so just set flag to true
    mTracksSet = True
    'load wav data (use midi data array)
    
  Case 2 'IIgs midi
    mFormat = 3
    'tracks are not applicable, so just set flag to true
    mTracksSet = True
    'load midi data
    
  Case 8 'standard PC/PCjr
    mFormat = 1
    'load notes
    LoadTracks
    
  Case Else
    'bad sound
    Unload
    On Error GoTo 0: Err.Raise vbObjectError + 598, strErrSrc, LoadResString(598)
  End Select
  
  'if error
  If Err.Number <> 0 Then
    lngError = Err.Number
    strErrSrc = Err.Source
    strError = Err.Description
    
    Unload
    On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
    Exit Sub
  End If
  
  'clear dirty flag
  mIsDirty = False
  mWriteProps = False
End Sub


Public Property Get MIDIData() As Byte()
  'returns the MIDI data stream or WAV strem for this sound resource
  
  On Error Resume Next
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  'if resource changed,
  If Not mMIDISet Then
    'build the midi first
    BuildSoundOutput
    If Err.Number <> 0 Then
      lngError = Err.Number
      strErrSrc = Err.Source
      strError = Err.Description
      On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
      Exit Property
    End If
  End If
  
  MIDIData = mMIDIData
End Property

Public Sub Save(Optional SaveFile As String)
  'saves the sound
  
  Dim strSection As String
  
  On Error GoTo ErrHandler
  
  'if properties need to be written
  If mWriteProps And agRes.InGame Then
    strSection = "Sound" & CStr(agRes.Number)
    'save ID and description to ID file
    WriteGameSetting strSection, "ID", mResID, "Sounds"
    WriteGameSetting strSection, "Description", mDescription
    'write song key signature, tqpn, and track instruments
    WriteGameSetting strSection, "Key", mKey, "Sounds"
    WriteGameSetting strSection, "TPQN", mTPQN
    WriteGameSetting strSection, "Inst0", mTrack(0).Instrument
    WriteGameSetting strSection, "Inst1", mTrack(1).Instrument
    WriteGameSetting strSection, "Inst2", mTrack(2).Instrument
    WriteGameSetting strSection, "Mute0", mTrack(0).Muted
    WriteGameSetting strSection, "Mute1", mTrack(1).Muted
    WriteGameSetting strSection, "Mute2", mTrack(2).Muted
    WriteGameSetting strSection, "Mute3", mTrack(3).Muted
    WriteGameSetting strSection, "Visible0", mTrack(0).Visible
    WriteGameSetting strSection, "Visible1", mTrack(1).Visible
    WriteGameSetting strSection, "Visible2", mTrack(2).Visible
    WriteGameSetting strSection, "Visible3", mTrack(3).Visible
    
    mWriteProps = False
    
  End If
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  'if dirty
  If mIsDirty Then
    'compile first
    CompileSound
    
    'if type is sound script
    If StrComp(Right$(SaveFile, 4), ".ass", vbTextCompare) = 0 Then
      'use export for script
      Export agRes.ResFile, sfScript
      
    ElseIf StrComp(Right$(SaveFile, 4), ".mid", vbTextCompare) = 0 Then
      'use export for MIDI
      Export agRes.ResFile, sfMIDI
    Else
      'use the resource save method
      agRes.Save SaveFile
      'if any errors
      If Err.Number <> 0 Then
        'pass error along
        lngError = Err.Number
        strError = Err.Description
        strErrSrc = Err.Source
        
        On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
        Exit Sub
      End If
      
      WriteGameSetting "Sound" & CStr(agRes.Number), "Size", agRes.Size, "Sounds"
      
      'mark as clean
      mIsDirty = False
    End If
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Property Get Resource() As AGIResource
  Set Resource = agRes
End Property



Public Sub StopSound()
  'stops the sound, if it is playing
  'calling this for ANY sound will stop ALL sound
  
  Dim rtn As Long
  
  On Error Resume Next
  
  'if not loaded
  If Not agRes.Loaded Then
    'error
    On Error GoTo 0: Err.Raise vbObjectError + 563, strErrSource, LoadResString(563)
    Exit Sub
  End If
  
  'if playing
  If blnPlaying Then
    rtn = mciSendString("close all", 0&, 0&, 0&)
    Set frmSndSubclass.agSndToPlay = Nothing
    blnPlaying = False
  End If
End Sub

Public Sub Unload()

  'unload resource
  
  On Error GoTo ErrHandler
  
  agRes.Unload
  mIsDirty = False
    
  'clear midi data
  ReDim mMIDIData(0)
  mMIDISet = False
  'reset length
  mLength = -1
  'clear notes collection by assigning to nothing
  Set mTrack(0) = Nothing
  Set mTrack(1) = Nothing
  Set mTrack(2) = Nothing
  Set mTrack(3) = Nothing
  
  mTracksSet = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub



Friend Property Let WritePropState(ByVal NewWritePropState As Boolean)

  mWriteProps = NewWritePropState
End Property

Private Property Get AGIResource_AllData() As Byte()
  AGIResource_AllData = agRes.AllData
End Property


Private Sub AGIResource_Clear()
  Clear
End Sub


Private Property Let AGIResource_Data(ByVal Pos As Long, ByVal NewData As Byte)
  agRes.Data(Pos) = NewData

End Property

Private Property Get AGIResource_Data(ByVal Pos As Long) As Byte
  On Error GoTo ErrHandler
  
  AGIResource_Data = agRes.Data(Pos)
Exit Property

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Property


Private Property Get AGIResource_EORes() As Boolean
  AGIResource_EORes = agRes.EORes
End Property


Private Sub AGIResource_Export(ExportFile As String)
  Export ExportFile
End Sub



Private Function AGIResource_GetPos() As Long
  AGIResource_GetPos = agRes.GetPos
End Function



Public Property Get Number() As Byte
  Number = agRes.Number
End Property


Private Property Get AGIResource_InGame() As Boolean
  AGIResource_InGame = agRes.InGame
End Property

Private Sub AGIResource_InsertData(NewData As Variant, Optional ByVal InsertPos As Long = -1&)

  On Error GoTo ErrHandler
  
  agRes.InsertData NewData, InsertPos
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub


Private Property Get AGIResource_Loaded() As Boolean
  AGIResource_Loaded = agRes.Loaded
End Property



Private Property Get AGIResource_Loc() As Long
  AGIResource_Loc = agRes.Loc

End Property


Private Sub AGIResource_NewResource(Optional ByVal Reset As Boolean = False)
  On Error GoTo ErrHandler
  
  agRes.NewResource Reset
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub

Private Property Get AGIResource_Number() As Byte
  AGIResource_Number = agRes.Number
End Property



Friend Sub Init(ByVal ResNum As Byte, ByVal VOL As Long, ByVal Loc As Long)
  
  On Error GoTo ErrHandler
  
  'initialize resource object
  agRes.Init ResNum, VOL, Loc
  
  'if first time loading this game, there will be nothing in the propertyfile
  mResID = ReadSettingString(agGameProps, "Sound" & CStr(ResNum), "ID", "")
  If Len(mResID) = 0 Then
    'no properties to load; save default ID
    mResID = "Sound" & CStr(ResNum)
    'load resource to get size
    agRes.Load
    'save ID and size to WAG file
    WriteGameSetting "Sound" & CStr(ResNum), "ID", mResID, "Sounds"
    WriteGameSetting "Sound" & CStr(ResNum), "Size", agRes.Size
    'unload when done
    agRes.Unload
  Else
    'get ID and description and other properties from wag file
    mDescription = ReadSettingString(agGameProps, "Sound" & CStr(ResNum), "Description", "")
    agRes.Size = ReadSettingLong(agGameProps, "Sound" & CStr(ResNum), "Size", -1)
    '*'Debug.Assert agRes.Size <> -1
  End If
Exit Sub

ErrHandler:
  
  'pass along the error
  Err.Raise Err.Number, Err.Source, Err.Description, Err.HelpFile, Err.HelpContext
  Resume Next
End Sub


Public Property Get Loaded() As Boolean
  
  Loaded = agRes.Loaded
End Property



Private Function AGIResource_ReadByte(Optional ByVal lngPos As Long = MAX_RES_SIZE + 1) As Byte
  On Error GoTo ErrHandler
  
  AGIResource_ReadByte = agRes.ReadByte(lngPos)
Exit Function

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Function





Private Function AGIResource_ReadWord(Optional ByVal lngPos As Long = -1&, Optional ByVal blnMSLS As Boolean = False) As Long
  On Error GoTo ErrHandler
  
  AGIResource_ReadWord = agRes.ReadWord(lngPos)
Exit Function

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Function


Private Sub AGIResource_RemoveData(ByVal RemovePos As Long, Optional ByVal RemoveCount As Long = 1)

  On Error GoTo ErrHandler
  
  agRes.RemoveData RemovePos, RemoveCount
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub


Private Property Get AGIResource_ResFile() As String
  AGIResource_ResFile = agRes.ResFile
End Property



Private Property Get AGIResource_ResType() As AGIResType
  AGIResource_ResType = agRes.ResType
End Property


Private Sub AGIResource_SetData(NewData() As Byte)

  agRes.SetData NewData
End Sub

Private Sub AGIResource_SetPos(ByVal lngPos As Long)
  On Error GoTo ErrHandler
  
  agRes.SetPos lngPos
Exit Sub

ErrHandler:
  'pass error along
  lngError = Err.Number
  strError = Err.Description
  strErrSrc = Err.Source
  
  On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
End Sub



Private Property Get AGIResource_Size() As Long
  AGIResource_Size = agRes.Size
End Property



Private Property Get AGIResource_SizeInVOL() As Long
  AGIResource_SizeInVOL = agRes.SizeInVol

End Property


Private Property Get AGIResource_Volume() As Long
  AGIResource_Volume = agRes.Volume
End Property



Private Sub agRes_Change()

  'sound data has changed- tracks don't match
  mTracksSet = False
  mIsDirty = True
End Sub

Private Sub Class_Initialize()

  'set play track to true
  mIsDirty = False
  strErrSource = "WINAGI.agiSound"
  Set agRes = New AGIResource
  agRes.SetType rtSound
  
  'create default PC/PCjr sound with no notes in any tracks
  mFormat = 1
  agRes.NewResource
  agRes.WriteByte &H8, 0
  agRes.WriteByte &H0
  agRes.WriteByte &H8
  agRes.WriteByte &H0
  agRes.WriteByte &H8
  agRes.WriteByte &H0
  agRes.WriteByte &H8
  agRes.WriteByte &H0
  agRes.WriteByte &HFF
  agRes.WriteByte &HFF
  
  
  Set mTrack(0) = New AGITrack
  Set mTrack(1) = New AGITrack
  Set mTrack(2) = New AGITrack
  Set mTrack(3) = New AGITrack
  mTrack(0).Instrument = 80
  mTrack(1).Instrument = 80
  mTrack(2).Instrument = 80
  
  
  'default tqpn is 16
  mTPQN = 16
  
  'length is undefined
  mLength = -1
  
  mResID = "NewSound"
End Sub



Private Sub Class_Terminate()
  
  Set agRes = Nothing
  Set mTrack(0) = New AGITrack
  Set mTrack(1) = New AGITrack
  Set mTrack(2) = New AGITrack
  Set mTrack(3) = New AGITrack
End Sub


      */
    }
  }
}