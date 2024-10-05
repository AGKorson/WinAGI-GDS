using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Sound;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmSoundEdit : Form {
        public int SoundNumber;
        public Sound EditSound;
        internal bool InGame;

        public frmSoundEdit() {
            InitializeComponent();
        }

        private void frmSoundEdit_Load(object sender, EventArgs e) {
            // if  a game is loaded, list all sounds by id
            if (EditGame is not null) {
                //foreach (AGILogic tmpLog in agLogs.Col.Values)
                //{
                //  listBox1.Items.Add(tmpLog.ToString());
                //}
                //foreach (AGIPicture tmpPic in agPics.Col.Values)
                //{
                //  listBox1.Items.Add(tmpPic.ToString());
                //}
                foreach (Sound tmpSound in EditGame.Sounds.Col.Values) {
                    listBox1.Items.Add(tmpSound);
                }
                //foreach (AGIView tmpView in agViews.Col.Values)
                //{
                //  listBox1.Items.Add(tmpView.ToString());
                //}
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e) {
            // let's load it

            Sound tmpSnd = (Sound)listBox1.SelectedItem;
            tmpSnd.Load();
            tmpSnd.SoundComplete += This_SoundComplete;
            tmpSnd.PlaySound(tmpSnd.SndFormat);
        }
        private void This_SoundComplete(object sender, SoundCompleteEventArgs e) {
            MessageBox.Show("all done!");
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {

        }

        public bool LoadSound(Sound ThisSound) {
            return true;
            /*
        Dim i As Long

        On Error GoTo ErrHandler

        'set ingame flag based on sound passed
        InGame = ThisSound.Resource.InGame

        'set number if this sound is in a game
        If InGame Then
          SoundNumber = ThisSound.Number
        Else
          'use a number that can never match
          'when searches for open sounds are made
          SoundNumber = 256
        End If

        'create new Sound object for the editor
        Set SoundEdit = New AGISound

        'copy the passed sound to the editor sound
        SoundEdit.SetSound ThisSound

        blnDontDraw = True

        'set caption and dirty flag
        If Not InGame And SoundEdit.ID = "NewSound" Then
          SoundCount = SoundCount + 1
          SoundEdit.ID = "NewSound" & CStr(SoundCount)
          IsDirty = True
        Else
          IsDirty = SoundEdit.IsDirty
        End If
        Caption = sSNDED & ResourceName(SoundEdit, InGame, True)
        If IsDirty Then
          MarkAsDirty
        Else
          frmMDIMain.mnuRSave.Enabled = False
          frmMDIMain.Toolbar1.Buttons("save").Enabled = False
        End If

        'load tree with actual sound data
        If Not BuildSoundTree() Then
          'error
          ErrMsgBox "This sound has corrupt or invalid data. Unable to open it for editing:", "", "Sound Resource Data Error"
          SoundEdit.Unload
          Set SoundEdit = Nothing
          Exit Function
        End If

        For i = 0 To 3
          picStaff(i).Visible = SoundEdit.Track(i).Visible
          picStaffVis(i) = SoundEdit.Track(i).Visible
        Next i

        SetKeyWidth

        blnDontDraw = False
        If OneTrack Then
          DrawStaff -2
        Else
          DrawStaff -1
        End If

        'return true
        EditSound = True
      Exit Function

      ErrHandler:
        'unload and show error
        ErrMsgBox "Error while opening sound: ", "Unable to edit this sound", "Edit Sound Error"
        SoundEdit.Unload
        Set SoundEdit = Nothing
      */
        }

        public void MenuClickDescription(int FirstProp) {
            /*
                    'change description and ID
                    Dim strID As String, strDescription As String

                    On Error GoTo ErrHandler

                    If FirstProp<> 1 And FirstProp <> 2 Then
                      FirstProp = 1
                    End If

                    strID = SoundEdit.ID
                    strDescription = SoundEdit.Description

                    If GetNewResID(AGIResType.Sound, SoundNumber, strID, strDescription, InGame, FirstProp) Then
                      'save changes
                      UpdateID strID, strDescription
                    End If
                  Exit Sub

                  ErrHandler:
                    '*'Debug.Assert False
                    Resume Next
            */
        }

        void tmpsoundform() {
            /*
      Option Explicit

        'variables for selected components

        Private UndoCol As Collection

        Public WithEvents SoundEdit As AGISound
        Public SoundNumber As Long
        Public InGame As Boolean
        Public IsDirty As Boolean
        Public PrevSEWndProc As Long

        'variables for selected components
        Private SelectedProp As Long
        Private PropRows As Long
        Private PropRowCount As Long
        Private EditPropDropdown As Boolean
        Private ListItemHeight As Long
        Private PropGotFocus As Long, PropDblClick As Boolean
        Private TreeRightButton As Boolean
        Private TreeX As Single, TreeY As Single

        'selection point variables
        Private SelTrack As Long, SelAnchor As Long
        Private SelStart As Long, SelLength As Long
        'SelAnchor is either SelStart, or SelStart+SelLength, depending
        'on whether selection is right-to-left, or left-to-right
        Private NodeX As Single, NodeY As Single

        Private picStaffVis(3) As Boolean  'need a local variable to track picStaff visibility,
                                      'because when drawing is disabled, VB thinks the
                                      'picture box is not visible, even if it really is

        Private CursorOn As Boolean, SelActive As Boolean
        Private KeyWidth As Long

        'default note properties
        Private DefLength As Long, DefAttn As Long
        Private DefOctave As Long, blnMute As Boolean
        'cursor control
        Private CursorPos As Long
        Private lngScrollDir As Long

        'variables for managing staves
        Private SOHorz As Long
        Private SOVert(3) As Long
        Public StaffScale As Long
        Private StaffMargin As Long
        Private blnDontDraw As Boolean
        Private OneTrack As Boolean

        Private MKbOffset As Long, NKbOffset As Long
        Private hMIDI As Long
        Private lngMIDInote As Long, blnNoteOn As Boolean
        Private mShift As Integer, mButton As Integer, mX As Single, mY As Single

        Private Const TICK_WIDTH = 5.625 'width of a tick, in pixels, at scale of 1 (60 ticks per second)

        'constants/variables for control/window placement
        Private Const SE_MARGIN As Long = 5 'in pixels

        Private CalcWidth As Long, CalcHeight As Long
        Private Const MIN_HEIGHT = 361
        Private Const MIN_WIDTH = 360

      Public Sub Activate()
        'bridge method to call the form's Activate event method
        Form_Activate
      End Sub

      Private Sub AddNotes(ByVal AddTrack As Long, ByVal InsertPos As Long, AddedNotes As AGINotes, SelectAll As Boolean, Optional ByVal DontUndo As Boolean = False)

        'adds notes to a track at a given position

        Dim NextUndo As SoundUndo
        Dim i As Long, lngCount As Long
        Dim tmpNode As Node

        On Error GoTo ErrHandler

        If Not DontUndo And Settings.SndUndo <> 0 Then
          'save undo information
          Set NextUndo = New SoundUndo
          NextUndo.UDAction = udsPaste
          NextUndo.UDTrack = AddTrack
          NextUndo.UDStart = InsertPos
          NextUndo.UDLength = AddedNotes.Count
          AddUndo NextUndo
        End If

        'reference track node
        Set tmpNode = tvwSound.Nodes(AddTrack + 2)

        'get number of notes currently in this track
        lngCount = SoundEdit.Track(AddTrack).Notes.Count

        'reset caption of End placeholder
        tmpNode.Child.LastSibling.Text = "Note " & lngCount

        'if currently there are no nodes
        i = 0
        Do
          'insert note at this position
          SoundEdit.Track(AddTrack).Notes.Add AddedNotes(i).FreqDivisor, AddedNotes(i).Duration, AddedNotes(i).Attenuation, InsertPos + i
          'add note placeholder to tree
          tvwSound.Nodes.Add(tmpNode.Index, tvwChild).Tag = CStr(lngCount + i + 1)
          tmpNode.Child.LastSibling.Text = "Note " & CStr(lngCount + i + 1)
          'increment counter
          i = i + 1
        Loop Until i = AddedNotes.Count

        'reset caption of End placeholder
        tmpNode.Child.LastSibling.Text = "End"

        'readjust track length, staff scroll, if necessary
        SetHScroll

        'set anchor, then change selection
        SelAnchor = SelStart
        'if selecting inserted notes
        If SelectAll Then
          ChangeSelection AddTrack, InsertPos, i, False, False, True
        Else
          'set cursor to point at end of inserted note
          ChangeSelection AddTrack, InsertPos + i, 0, False, False, True
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


       Private Function BuildSoundTree() As Boolean

        Dim i As Long, j As Long
        Dim tmpNode As Node, lngNoteCount As Long
        Dim strMsg As Long
        Dim blnWhiteNoise As Boolean

        '
        'note: when building midi freq from agifreq, that instead of adding .5 and using int(), it works the same to round
        'notes to the nearest integer
        '
        '
        On Error GoTo ErrHandler

        With tvwSound.Nodes
          'clear out tree
          .Clear

          'add root
          .Add , , "Root", SoundEdit.ID

          'add track headers
          .Add 1, tvwChild, "t0", "Track 0"
          .Add 1, tvwChild, "t1", "Track 1"
          .Add 1, tvwChild, "t2", "Track 2"
          .Add 1, tvwChild, "tn", "Noise"


          For i = 0 To 2
            'add this track's notes
            lngNoteCount = SoundEdit.Track(i).Notes.Count - 1
            For j = 0 To lngNoteCount
              Set tmpNode = .Add(i + 2, tvwChild, , "Note " & CStr(j))
              tmpNode.Tag = j
            Next j
            'add end placeholder
            Set tmpNode = .Add(i + 2, tvwChild, , "End")
            tmpNode.Tag = j
          Next i

          'add noise track data
          lngNoteCount = SoundEdit.Track(3).Notes.Count - 1
          For j = 0 To lngNoteCount
            Set tmpNode = .Add(5, tvwChild, CStr(i) & ":" & CStr(j), "Note " & CStr(j))
            tmpNode.Tag = j
          Next j
          'add end placeholder
          Set tmpNode = .Add(5, tvwChild, , "End")
          tmpNode.Tag = j
        End With

        'force update by using click method on root
        tvwSound.Nodes(1).Selected = True
        tvwSound.Nodes(1).Expanded = True
        tvwSound_NodeClick tvwSound.SelectedItem

      '  'use scale method to adjust scrollbar
      '  ZoomScale 0

        'return success
        BuildSoundTree = True
      Exit Function

      ErrHandler:
        'error- let calling method deal with it
        'by returning false
        blnDontDraw = False
      End Function
      Private Sub ChangeSelection(ByVal NewTrack As Long, NewStart As Long, NewLength As Long, Optional ByVal NoScroll As Boolean = False, Optional ByVal MoveEndPt As Boolean = False, Optional ByVal ForceRedraw As Boolean = False)

        Dim PrevTrack As Long, tmpNode As Node
        Dim i As Long
        Dim blnPrevVis As Boolean, blnSelVis As Boolean
        Dim lngStartPos As Long, lngEndPos As Long
        Dim blnScrolled As Boolean
        Dim lngTreeTrack As Long

        'if newtrack=-1, means no track selected, and redraw all VISIBLE tracks

        On Error GoTo ErrHandler

        'note previuos seltrack
        PrevTrack = SelTrack
        SelTrack = NewTrack

        'tracks may not be visible; if not don't need to deal with them
        If SelTrack <> -1 Then
          blnSelVis = picStaffVis(SelTrack)
        Else
          'if one track, and nothing selected, hide all tracks and we're done!
          If OneTrack Then
            For i = 0 To 3
              picStaff(i).Visible = False
              picStaffVis(i) = False
            Next i
            hsbStaff.Visible = False
            Exit Sub
          Else
           blnSelVis = True
          End If
        End If
        If PrevTrack <> -1 Then
          blnPrevVis = picStaffVis(PrevTrack)
        Else
          blnPrevVis = True
        End If

        'if selection requires scrolling, do that first before anything else;
        'that saves from drawing staves once in old position, then again
        'after scrolling

        'check to see if staff is supposed to be scrolled first
        If Not NoScroll And blnSelVis And NewTrack >= 0 Then
          'calculate position of the new selection start point
          lngStartPos = NotePos(NewTrack, NewStart)
          'and position of the new selection end point
          lngEndPos = NotePos(NewTrack, NewStart + NewLength)

          'if moving end point:
          If MoveEndPt Then
            'endpos to left, OR endpos to right:
            If SOHorz + lngEndPos * TICK_WIDTH * StaffScale < 0 Or SOHorz + lngEndPos * TICK_WIDTH * StaffScale + KeyWidth > picStaff(SelTrack).ScaleWidth + vsbStaff(SelTrack).Visible * vsbStaff(SelTrack).Width Then
              'set scroll so note is at right edge
              lngEndPos = lngEndPos - picStaff(SelTrack).ScaleWidth / TICK_WIDTH / StaffScale + 16

              'set scrollbars
              If lngEndPos > hsbStaff.Max Then
                lngEndPos = hsbStaff.Max
              End If
              If lngEndPos < 0 Then
                lngEndPos = 0
              End If
              blnDontDraw = True
              hsbStaff.Value = lngEndPos
              blnDontDraw = False
              blnScrolled = True
            End If

          'if moving start point (or neither point)
          Else
            'startpos to left, or startpos to right:
            If SOHorz + lngStartPos * TICK_WIDTH * StaffScale < 0 Or SOHorz + lngStartPos * TICK_WIDTH * StaffScale + KeyWidth > hsbStaff.Width Then
              'set scroll so note is at left edge (allow small margin)
              'set scrollbars

              'verify new position is within bounds of staff display
              If lngStartPos < 0 Then lngStartPos = 0
              'if pos is past right extent of scrollbar, max out
              If lngStartPos > hsbStaff.Max Then lngStartPos = hsbStaff.Max
              blnDontDraw = True
              hsbStaff.Value = lngStartPos
              blnDontDraw = False
              blnScrolled = True
            End If
          End If
        End If

        'if scrolled, draw all tracks, which clears selection/cursor
        If blnScrolled Then
          'which 'alldraw' used depends on onetrack setting
          If OneTrack Then
            DrawStaff -2
          Else
            DrawStaff -1
          End If

          'make sure cursor status is set to off and timer disabled
          CursorOn = False
          Timer1.Enabled = False
          'and selactive is set to off
          SelActive = False

        'if not scrolled, do regular checks to clear selection/cursor:

        'did seltrack change to a different track?
        ElseIf SelTrack <> PrevTrack Then
          'if previuos track was a valid one and visible, need to clear it
          If PrevTrack <> -1 And blnPrevVis Then
            'BUT only if showing all tracks?

            'redraw prevtrack (automatically clears old selection, if there was one)
            DrawStaff PrevTrack
            'make sure cursor status is set to off and timer disabled
            CursorOn = False
            Timer1.Enabled = False
            'and selactive is set to off
            SelActive = False
          End If

          'if no track selected?
          If SelTrack = -1 Then
            'if forcing a redraw, do it now
            If ForceRedraw Then
              DrawStaff -1
            End If
          Else
            'deal with new selected track
            'is new track visible?
            If picStaffVis(SelTrack) Then
              'if it is, select it by drawing selection border
              With picStaff(SelTrack)
                .DrawWidth = 3
                .FillStyle = vbFSTransparent
                'add track selection border
                picStaff(SelTrack).Line (0, 0)-(picStaff(SelTrack).ScaleWidth + (vsbStaff(SelTrack).Visible * vsbStaff(SelTrack).Width) - 2, picStaff(SelTrack).ScaleHeight - 2), vbBlue, B
                .DrawWidth = 1
                .FillStyle = vbFSSolid
              End With
            Else
              'onetrack mode?
              If OneTrack Then
                'use special draw value (-2) to force redraw of correct track
                DrawStaff -2
              End If
            End If
          End If

          'if track changed, might have to switch and redraw keyboard
          If picKeyboard.Visible Then
            If (PrevTrack = 3 Or SelTrack = 3) Then
              'set scrollbar properties
              SetKeyboardScroll
              'reset scrollbar Value
              hsbKeyboard.Value = hsbKeyboard.Min
              NKbOffset = 0
              MKbOffset = 45
              picKeyboard.Refresh
            End If
          End If

          'change keyboard instrument if track changes
          'adjust midi output to match track, if midi is enabled
          If Not Settings.NoMIDI And hMIDI <> 0 Then
            'if track is a music track
            If SelTrack >= 0 And SelTrack <= 2 Then
              'send instrument to midi
              midiOutShortMsg hMIDI, CLng(SoundEdit.Track(SelTrack).Instrument * &H100 + &HC0)
            Else
              'set instrument to 0
              midiOutShortMsg hMIDI, &H1C0&
            End If
          End If

          'if oldtrack is noise, or new track is noise
          If SelTrack = 3 Or PrevTrack = 3 Then
            'refresh keyboard so it shows correctly
            picKeyboard.Refresh
          End If
        Else
          'track hasn't changed

          '(but watchout for 'no track')
          'if forcing a redraw, do it now
          If ForceRedraw Then

            'draw new staff, IF it is visible
            If blnSelVis Then
              DrawStaff SelTrack
            End If
            'make sure timer is off (don't use hidecursor, as it
            'could cause selection to invert!)
            Timer1.Enabled = False
            CursorOn = False

          Else
            'seltrack is the same
            'is there a selection?
            If SelActive Then
              'clear it, no flicker
              ClearSelection True
            Else
              HideCursor PrevTrack
            End If
          End If
        End If

        'previous track/aselection is cleared; new seltrack is drawn, visible;
        'nothing is selected, cursor should be off

        'now make new selection
        SelStart = NewStart
        SelLength = NewLength

        'if a valid track is selected, make the selection
        If SelTrack <> -1 Then
          If SelStart >= 0 Then
            If blnSelVis Then
              'show cursor or selection
              ShowSelection
            End If
            'select starting note node, if not already selected
            If tvwSound.SelectedItem.Parent Is Nothing Then
              lngTreeTrack = -1
            Else
              lngTreeTrack = tvwSound.SelectedItem.Parent.Index - 2
            End If
            If tvwSound.SelectedItem.Tag <> SelStart Or lngTreeTrack <> SelTrack Then
              Set tmpNode = tvwSound.Nodes(SelTrack + 2).Child
              Do Until tmpNode.Tag = SelStart
                Set tmpNode = tmpNode.Next
              Loop
              tmpNode.Selected = True

              If tmpNode.Parent.Index = 5 And PropRowCount <> 4 Then
                PropRowCount = 4
                SetPropertyBoxPos
              Else
                If PropRowCount <> 6 Then
                  PropRowCount = 6
                  SetPropertyBoxPos
                End If
              End If
              'fill in property window
              PaintPropertyWindow
              'and make sure edit menu is correct
              SetEditMenu
            End If
          Else
            'select track node
            tvwSound.Nodes(SelTrack + 2).Selected = True
            'fill in property window
            PaintPropertyWindow
            'and make sure edit menu is correct
            SetEditMenu
          End If
        Else
          'select root node
          tvwSound.Nodes(1).Selected = True
          'fill in property window
          PaintPropertyWindow
          'and make sure edit menu is correct
          SetEditMenu
        End If

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub ClearSelection(ByVal NoFlicker As Boolean)

        Dim lngStartPos As Long, lngEndPos As Long

        On Error GoTo ErrHandler

        'assumes that:
        '  a track is selected (SelTrack >= 0) AND
        '  track is visible (picStaffVis(SelTrack) = True) AND
        '  one or more notes currently selected (SelStart >=0 And SelLength >=1) AND
        '  selection is actively displayed (SelActive= True)
        '
        '  if NoFlicker = True, disable updating until specifically re-enabled
        '  this allows expanding/shrinking selection without flickering

        '*'Debug.Assert SelTrack >= 0
        If SelTrack < 0 Then
          Exit Sub
        End If
        '*'Debug.Assert picStaffVis(SelTrack) = True
        '*'Debug.Assert SelActive = True

        If NoFlicker Then
          'DON'T DO THIS IF TRACK IS NOT VISIBLE
          If picStaffVis(SelTrack) Then
      '''      SendMessage picStaff(SelTrack).hWnd, WM_SETREDRAW, 0, 0
          End If
        End If

        'calculate start pos
        lngStartPos = SOHorz + KeyWidth + NotePos(SelTrack, SelStart) * TICK_WIDTH * StaffScale

        'set draw mode to invert
        picStaff(SelTrack).DrawMode = 6

        'if starting pos <keywidth,
        If lngStartPos < KeyWidth Then
          lngStartPos = KeyWidth
        End If

        'calculate end pos
        lngEndPos = SOHorz + KeyWidth + NotePos(SelTrack, SelStart + SelLength) * TICK_WIDTH * StaffScale - 2

        'if end note is past edge of staff
        If lngEndPos > picStaff(SelTrack).Width - 25 Then
          lngEndPos = picStaff(SelTrack).Width - 25
        End If

        'draw box over selection
        picStaff(SelTrack).Line (lngStartPos, 2)-(lngEndPos, picStaff(SelTrack).ScaleHeight - 4), vbBlack, BF

        'mode to copypen
        picStaff(SelTrack).DrawMode = 13

        'set selection status to off
        SelActive = False
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub DeleteNotes(ByVal DelTrack As Long, ByVal DelPos As Long, ByVal DelCount As Long, Optional ByVal DontUndo As Boolean = False)

        'deletes notes at this position

        Dim NextUndo As SoundUndo
        Dim DelNotes As AGINotes
        Dim tmpNode As Node
        Dim i As Long

        'if not skipping undo
        If Not DontUndo Then
          Set NextUndo = New SoundUndo
          NextUndo.UDAction = udsDelete
          NextUndo.UDTrack = DelTrack
          NextUndo.UDStart = DelPos
          NextUndo.UDLength = DelCount
          Set DelNotes = New AGINotes
        End If

        'delete the selected notes
        For i = 0 To DelCount - 1
          'if not undoing
          If Not DontUndo Then
            'add this note to undolist
            DelNotes.Add SoundEdit.Track(DelTrack).Notes(DelPos).FreqDivisor, SoundEdit.Track(DelTrack).Notes(DelPos).Duration, SoundEdit.Track(DelTrack).Notes(DelPos).Attenuation
          End If
          'delete the note
          SoundEdit.Track(DelTrack).Notes.Remove DelPos
          'remove note from tree
          Set tmpNode = tvwSound.Nodes(DelTrack + 2).Child.LastSibling.Previous
          tvwSound.Nodes.Remove tmpNode.Index
        Next i

        If Not DontUndo And Settings.SndUndo <> 0 Then
          Set NextUndo.UDNotes = DelNotes
          AddUndo NextUndo
        End If

        'reset End placeholder tag Value
        tvwSound.Nodes(DelTrack + 2).Child.LastSibling.Tag = SoundEdit.Track(DelTrack).Notes.Count

        ChangeSelection SelTrack, DelPos, 0, False, False, True

      End Sub
      Private Sub DrawNoiseNote(ByVal HPos As Long, ByVal Note As Long, ByVal Length As Long, ByVal Attenuation As Long)

        'draws noise note on staff;
        'HPos is horizontal position where note will be drawn
        'Note is the freq of the note to be played
        'Length is length of note is AGI ticks
        'Attenuation is amount of volume attenuation (0 means loudest; 15 means mute)

        Dim rtn As Long
        Dim sngLen As Single
        Dim i As Single, lngVPos As Long
        Dim lngColor As Long
        Dim lngTPQN As Long

        'get tpqn
        lngTPQN = SoundEdit.TPQN

        'set note color based on attenuation
        lngColor = EGAColor(Attenuation)
        picStaff(3).FillColor = lngColor

        'convert length of note to MIDI Value, using TPQN
        sngLen = Length / lngTPQN * 4 '(one MIDI unit is a sixteenth note)

        'set vertical position
        lngVPos = (11 + 6 * (Note And 3)) * StaffScale + SOVert(3) + 1

        'set fill style to diagonal hash
        picStaff(3).FillStyle = vbUpwardDiagonal

        'if white noise
        If (Note And 4) = 4 Then
          'draw line with diagonal fill
          picStaff(3).Line (HPos, lngVPos)-(HPos + StaffScale * TICK_WIDTH * (Length - 0.3), lngVPos + StaffScale * 6 - 2), lngColor, B
        Else
          'draw line with solid fill
          picStaff(3).Line (HPos, lngVPos)-(HPos + StaffScale * TICK_WIDTH * (Length - 0.3), lngVPos + StaffScale * 6 - 2), lngColor, BF
        End If

        'reset fill style
        picStaff(3).FillStyle = vbFSSolid

      End Sub

      Private Sub DrawRest(ByVal Track As Long, ByVal HPos As Long, ByVal Length As Long)

        'draws rest on staff;
        'HPos is horizontal position where note will be drawn
        'Length is length of note is AGI ticks

        Dim rtn As Long, i As Long
        Dim VPos As Long, TrackCount As Long
        Dim sngLen As Single, lngTempPos As Long
        Dim lngTPQN As Long

        'get tpqn
        lngTPQN = SoundEdit.TPQN

        'convert length of note to MIDI Value, using TPQN
        sngLen = Length / lngTPQN * 4 '(one MIDI unit is a sixteenth note)

        'set fill color to black so dots draw correctly
        picStaff(Track).FillColor = vbBlack

        'if this is noise track
        If Track = 3 Then
          'notes are drawn only once
          TrackCount = 1
          VPos = 16 * StaffScale + SOVert(Track)
        Else
          TrackCount = 2
          VPos = 101 * StaffScale + SOVert(Track)
        End If

        lngTempPos = HPos
        For i = 1 To TrackCount
          HPos = lngTempPos
          'if showing notes
          If Settings.ShowNotes Then
           'draw appropriate rest
           Select Case sngLen
           Case 1  'sixteenth rest
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 0, 264, 72, 108, SRCAND)
           Case 2  'eighth rest
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 72, 264, 72, 108, SRCAND)
           Case 3  'eighth rest dotted
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 72, 264, 72, 108, SRCAND)
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 10, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack

           Case 4  'quarter rest
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 144, 264, 72, 108, SRCAND)
           Case 5 'quater rest and sixteenth rest
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 144, 264, 72, 108, SRCAND)
             'draw connector
             rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                              NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
             'increment position
             HPos = HPos + StaffScale * TICK_WIDTH * lngTPQN
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 0, 264, 72, 108, SRCAND)
           Case 6  'quarter rest dotted
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 144, 264, 72, 108, SRCAND)
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 10, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack

           Case 7  'quarter rest double dotted
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 144, 264, 72, 108, SRCAND)
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 10, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 13, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack

           Case 8  'half rest
             picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF

           Case 9  'half rest and sixteenth
             picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
             'draw connector
             rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 2, StaffScale * 7, _
                             NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
             'increment position
             HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 0, 264, 72, 108, SRCAND)
           Case 10 'half rest and eighth
             picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
             'draw connector
             rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 2, StaffScale * 7, _
                              NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
             'increment position
             HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 72, 264, 72, 108, SRCAND)
           Case 11 'half rest, eighth dotted
             picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
             'draw connector
             rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 2, StaffScale * 7, _
                              NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
             'increment position
             HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 72, 264, 72, 108, SRCAND)
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 10, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack
           Case 12 'half rest dotted
             picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 11, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack

           Case 13 'half rest dotted and sixteenth
             picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 11, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack
             'draw connector
             rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 3, StaffScale * 7, _
                              NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
             'increment position
             HPos = HPos + StaffScale * TICK_WIDTH * 3 * lngTPQN
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 0, 264, 72, 108, SRCAND)
           Case 14 'half rest double dotted
             picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 11, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 14, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack

           Case 15 'half rest double dotted and sixteenth
             picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 11, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack
             'draw dot
             picStaff(Track).Circle (HPos + StaffScale * 14, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack
             'draw connector
             rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 3.5, StaffScale * 7, _
                              NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
             'increment position
             HPos = HPos + StaffScale * TICK_WIDTH * 3.5 * lngTPQN
             rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                              NotePictures.hDC, 0, 264, 72, 108, SRCAND)
           Case 16 'whole rest
             picStaff(Track).Line (HPos, VPos + StaffScale)-Step(7 * StaffScale, 4 * StaffScale), vbBlack, BF

           Case Is > 16
            'greater than whole note;
            'recurse to draw one whole note at a time until done
            'ONLY do this once;
            If i = 1 Then
              Do Until Length < 4 * lngTPQN
                DrawRest Track, HPos, 4 * lngTPQN
                'draw connector
                rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 4, StaffScale * 7, _
                                 NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
                If Track <> 3 Then
                  rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 77, StaffScale * lngTPQN * TICK_WIDTH * 4, StaffScale * 7, _
                                   NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
                End If
                  'decrement length
                  Length = Length - 4 * lngTPQN
                  'increment horizontal position
                  HPos = HPos + StaffScale * TICK_WIDTH * lngTPQN * 4
                Loop
                'if anything left
                If Length > 0 Then
                  'draw remaining portion of note
                  DrawRest Track, HPos, Length
                End If
              End If
            Case Else
              picStaff(Track).FillStyle = vbFSTransparent
              'not a normal note; draw a bar
              picStaff(Track).Line (HPos, VPos - 2 * StaffScale)-Step _
                                   (StaffScale * TICK_WIDTH * (Length - 0.5), StaffScale * 18), RGB(228, 228, 228), BF
              'draw black border around bar
              picStaff(Track).Line (HPos, VPos - 2 * StaffScale)-Step _
                                   (StaffScale * TICK_WIDTH * (Length - 0.5), StaffScale * 18), vbBlack, B
              picStaff(Track).FillStyle = vbFSSolid
            End Select
          Else
            'draw all rest notes as blocks
            picStaff(Track).FillStyle = vbFSTransparent
            picStaff(Track).Line (HPos, VPos - 2 * StaffScale)-Step _
                                 (StaffScale * TICK_WIDTH * (Length - 0.5), StaffScale * 18), RGB(228, 228, 228), BF
            'draw black border around bar
            picStaff(Track).Line (HPos, VPos - 2 * StaffScale)-Step _
                                 (StaffScale * TICK_WIDTH * (Length - 0.5), StaffScale * 18), vbBlack, B
            picStaff(Track).FillStyle = vbFSSolid
          End If

          'reset vpos
          VPos = 161 * StaffScale + SOVert(Track)
        Next i
      End Sub

      Public Sub DrawStaff(ByVal TrackNo As Long)

        'draws the staff for the given track

        Dim i As Long, j As Long
        Dim lngFreq As Long, lngDur As Long, lngAtt As Long
        Dim lngMIDInote As Long
        Dim tmpNode As Node, sngStaffWidth As Single
        Dim lngHPos As Long
        Dim lngTPQN As Long, lngNoteCount As Long
        Dim intMax As Integer, lngSndLength As Long
        Dim intVal As Integer
        Dim strTime As String

        On Error GoTo ErrHandler

        'if overriding draw
        If blnDontDraw Then
          Exit Sub
        End If

        'special draw functions are managed as negative values
        If TrackNo < 0 Then
          Select Case TrackNo
          Case -1
            'draw all tracks
            For i = 0 To 3
              'recurse to draw correct tracks
              DrawStaff i
            Next i
          Case -2
            'switch TO/FROM one track
            For i = 0 To 3
            '*'Debug.Assert OneTrack
                If i = SelTrack Then
                  picStaff(i).Visible = True
                  picStaffVis(i) = True
                Else
                  picStaff(i).Visible = False
                  picStaffVis(i) = False
                End If
            Next i
            'use resize to force update
            Form_Resize
          Case -3
            'force redraw due to change in settings
            'which depends on which displaymode is currently active
            If OneTrack Then
              DrawStaff -2
            Else
              DrawStaff -1
            End If
            Exit Sub
          End Select
          'exit
          Exit Sub
        End If

        'get tpqn
        lngTPQN = SoundEdit.TPQN

        'if track not visible
        If Not picStaffVis(TrackNo) Then
          Exit Sub
        End If

        'clear the staff
        picStaff(TrackNo).Cls
        picStaff(TrackNo).ForeColor = vbBlack

        'set drawmode to copypen (so lines draw correctly)
        picStaff(TrackNo).DrawMode = 13

        'cache scalewidth and sound length
        sngStaffWidth = picStaff(TrackNo).ScaleWidth
        lngSndLength = SoundEdit.Length * 15 / lngTPQN

        'if noise track
        If TrackNo = 3 Then
          For i = 0 To 4
            picStaff(3).Line (0, (11 + 6 * i) * StaffScale + SOVert(3))-(sngStaffWidth, (11 + 6 * i) * StaffScale + SOVert(3)), vbBlack
          Next i
          'draw time lines at whole note intervals, offset by two pixels
          For i = 0 To lngSndLength
            'if past right edge,
            'horizontal pos of marker
            lngHPos = i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2

            'if past right edge
            If lngHPos > sngStaffWidth Then
              Exit For
            End If
            'if greater than 0 (not to left of visible window)
            If lngHPos >= 0 Then
              picStaff(3).Line (i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2, (11 * StaffScale) + SOVert(3))-Step(0, 24 * StaffScale), vbBlack
            End If
          Next i
        Else
          For i = 0 To 4
            picStaff(TrackNo).Line (0, (96 + 6 * i) * StaffScale + SOVert(TrackNo))-(sngStaffWidth, (96 + 6 * i) * StaffScale + SOVert(TrackNo)), vbBlack
          Next i
          For i = 0 To 4
            picStaff(TrackNo).Line (0, (156 + 6 * i) * StaffScale + SOVert(TrackNo))-(sngStaffWidth, (156 + 6 * i) * StaffScale + SOVert(TrackNo)), vbBlack
          Next i
          'draw time lines at whole note intervals
          For i = 0 To lngSndLength
            'horizontal pos of marker
            lngHPos = i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2
            'if past right edge,
            If lngHPos > sngStaffWidth Then
              Exit For
            End If
            If lngHPos >= 0 Then
              picStaff(TrackNo).Line (lngHPos, (96 * StaffScale) + SOVert(TrackNo))-Step(0, 84 * StaffScale), vbBlack
            End If
          Next i
        End If

        'start at first note, offset by clef and Key signature
        lngHPos = SOHorz + KeyWidth

        'step through all notes in this track
        lngNoteCount = SoundEdit.Track(TrackNo).Notes.Count - 1
        For i = 0 To lngNoteCount
          'get duration first
          lngDur = SoundEdit.Track(TrackNo).Notes(i).Duration

          'if note is visible,
          If lngHPos + StaffScale * TICK_WIDTH * lngDur > KeyWidth * 0.75 Then
            'now get freq and attenuation
            lngFreq = SoundEdit.Track(TrackNo).Notes(i).FreqDivisor
            lngAtt = SoundEdit.Track(TrackNo).Notes(i).Attenuation

            'if music note is zero, or attenuation is zero
            If (TrackNo <> 3 And lngFreq = 0) Or lngAtt = 15 Then
              'draw a rest note
              DrawRest TrackNo, lngHPos, lngDur
            Else
              'if noise track
              If TrackNo = 3 Then
                'draw noise note
                DrawNoiseNote lngHPos, lngFreq, lngDur, lngAtt
              Else
                'convert note to MIDE and draw it
                DrawNote TrackNo, lngHPos, MIDINote(lngFreq), lngDur, lngAtt
              End If
            End If
          End If
          'calculate new position by adding this note's length
          lngHPos = lngHPos + StaffScale * TICK_WIDTH * lngDur

          'if note is past visible area
          If lngHPos > sngStaffWidth Then
            Exit For
          End If
        Next i

        'now add clef and time marks

        'if noise track
        If TrackNo = 3 Then
          'clear clef area
          picStaff(3).Line (0, 0)-(KeyWidth - 3, picStaff(3).ScaleHeight), vbWhite, BF

          'redraw the staff lines
          For i = 0 To 4
            picStaff(3).Line (0, (11 + 6 * i) * StaffScale + SOVert(3))-Step(KeyWidth, 0), vbBlack
          Next i

          'add time markers
          picStaff(3).FontSize = picStaff(3).FontSize * 2
          picStaff(3).CurrentY = 36 * StaffScale + SOVert(3)
          'draw time marks at whole note intervals
          For i = 0 To lngSndLength
            strTime = format(i / 15 * lngTPQN, "0.0#")
            'horizontal pos of marker
            lngHPos = i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2 - picStaff(TrackNo).TextWidth(strTime) / 2
            'if past right edge
            If lngHPos > sngStaffWidth Then
              Exit For
            End If
            'if greater than 0 (not to left of visible window)
            If lngHPos >= 0 Then
              picStaff(3).CurrentX = lngHPos
              picStaff(3).Print strTime;
            End If
          Next i
          picStaff(3).FontSize = picStaff(3).FontSize / 2

          picStaff(3).FontTransparent = True
          'draw noise clef
          picStaff(3).CurrentX = 3
          picStaff(3).CurrentY = 10 * StaffScale + SOVert(3) + 1
          picStaff(3).Print "   2330"
          picStaff(3).CurrentX = 3
          picStaff(3).CurrentY = 16 * StaffScale + SOVert(3) + 1
          picStaff(3).Print "   1165"
          picStaff(3).CurrentX = 3
          picStaff(3).CurrentY = 22 * StaffScale + SOVert(3) + 1
          picStaff(3).Print "    583"
          picStaff(3).CurrentX = 3
          picStaff(3).CurrentY = 28 * StaffScale + SOVert(3) + 1
          picStaff(3).Print "Track 2"
          picStaff(3).FontTransparent = False

        Else
          'clear clef area (adjust by two for offset, then one more for linewidth)
      '    picStaff(TrackNo).Line (0, 0)-(KeyWidth - 6 * StaffScale, picStaff(TrackNo).ScaleHeight), vbWhite, BF
          picStaff(TrackNo).Line (0, 0)-(KeyWidth - 3, picStaff(TrackNo).ScaleHeight), vbWhite, BF

          'redraw the staff lines
          For i = 0 To 4
      '      picStaff(TrackNo).Line (0, (96 + 6 * i) * StaffScale + SOVert(TrackNo))- _
      '                             (KeyWidth - 6 * StaffScale + 1, (96 + 6 * i) * StaffScale + SOVert(TrackNo)), vbBlack
            picStaff(TrackNo).Line (0, (96 + 6 * i) * StaffScale + SOVert(TrackNo))-Step(KeyWidth, 0), vbBlack
          Next i
          For i = 0 To 4
      '      picStaff(TrackNo).Line (0, (156 + 6 * i) * StaffScale + SOVert(TrackNo))- _
      '                             (KeyWidth - 6 * StaffScale + 1, (156 + 6 * i) * StaffScale + SOVert(TrackNo)), vbBlack
            picStaff(TrackNo).Line (0, (156 + 6 * i) * StaffScale + SOVert(TrackNo))-Step(KeyWidth, 0), vbBlack
          Next i

          'draw time markers
          picStaff(TrackNo).FontSize = picStaff(TrackNo).FontSize * 2
          For i = 0 To lngSndLength
            strTime = format(i / 15 * lngTPQN, "0.0#")
            'horizontal pos of marker
            lngHPos = i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2 - picStaff(TrackNo).TextWidth(strTime) / 2
            'if past right edge
            If lngHPos > sngStaffWidth Then
              Exit For
            End If
            'if greater than 0 (not to left of visible window)
            If lngHPos >= 0 Then
              picStaff(TrackNo).CurrentY = 130 * StaffScale + SOVert(TrackNo)
              picStaff(TrackNo).CurrentX = lngHPos
              picStaff(TrackNo).Print strTime;
            End If
          Next i
          picStaff(TrackNo).FontSize = picStaff(TrackNo).FontSize / 2

          'draw clefs
          i = StretchBlt(picStaff(TrackNo).hDC, 3, 90 * StaffScale - 2 + SOVert(TrackNo), 16 * StaffScale, 41 * StaffScale, _
                         NotePictures.hDC, 367, 0, 140, 358, SRCAND)
          i = StretchBlt(picStaff(TrackNo).hDC, 3, 156 * StaffScale + 1 + SOVert(TrackNo), 16 * StaffScale, 20 * StaffScale, _
                         NotePictures.hDC, 520, 0, 150, 174, SRCAND)

          'add Key signature
          Do While SoundEdit.Key <> 0
            If Sgn(SoundEdit.Key) > 0 Then
              'add f (-10,+2)
              i = StretchBlt(picStaff(TrackNo).hDC, 20 * StaffScale, 89 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 20 * StaffScale, 149 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              If SoundEdit.Key = 1 Then
                Exit Do
              End If
              'add c (-7, +5)
              i = StretchBlt(picStaff(TrackNo).hDC, 26 * StaffScale, 98 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 26 * StaffScale, 158 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              If SoundEdit.Key = 2 Then
                Exit Do
              End If
              'add g (-11, +1)
              i = StretchBlt(picStaff(TrackNo).hDC, 32 * StaffScale, 86 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 32 * StaffScale, 146 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              If SoundEdit.Key = 3 Then
                Exit Do
              End If
              'add d (-8,+4)
              i = StretchBlt(picStaff(TrackNo).hDC, 38 * StaffScale, 95 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 38 * StaffScale, 155 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              If SoundEdit.Key = 4 Then
                Exit Do
              End If
              'add a (-5,+7)
              i = StretchBlt(picStaff(TrackNo).hDC, 44 * StaffScale, 104 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 44 * StaffScale, 164 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              If SoundEdit.Key = 5 Then
                Exit Do
              End If
              'add e (-9,+3)
              i = StretchBlt(picStaff(TrackNo).hDC, 50 * StaffScale, 92 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 50 * StaffScale, 152 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              If SoundEdit.Key = 6 Then
                Exit Do
              End If
              'add b (-6,+6)
              i = StretchBlt(picStaff(TrackNo).hDC, 56 * StaffScale, 101 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 56 * StaffScale, 161 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            Else
              'add b (-6, +6)
              i = StretchBlt(picStaff(TrackNo).hDC, 20 * StaffScale, 98 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 20 * StaffScale, 158 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              If SoundEdit.Key = -1 Then
                Exit Do
              End If
              'add e (-9, +3)
              i = StretchBlt(picStaff(TrackNo).hDC, 26 * StaffScale, 89 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 26 * StaffScale, 149 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              If SoundEdit.Key = -2 Then
                Exit Do
              End If
              'add a (-5, +7)
              i = StretchBlt(picStaff(TrackNo).hDC, 32 * StaffScale, 101 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 32 * StaffScale, 161 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              If SoundEdit.Key = -3 Then
                Exit Do
              End If
              'add d (-8, +4)
              i = StretchBlt(picStaff(TrackNo).hDC, 38 * StaffScale, 92 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 38 * StaffScale, 152 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              If SoundEdit.Key = -4 Then
                Exit Do
              End If
              'add g (-4, +8)
              i = StretchBlt(picStaff(TrackNo).hDC, 44 * StaffScale, 104 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 44 * StaffScale, 164 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              If SoundEdit.Key = -5 Then
                Exit Do
              End If
              'add c (-7, +5)
              i = StretchBlt(picStaff(TrackNo).hDC, 50 * StaffScale, 95 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 50 * StaffScale, 155 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              If SoundEdit.Key = -6 Then
                Exit Do
              End If
              'add f (-3, +9)
              i = StretchBlt(picStaff(TrackNo).hDC, 56 * StaffScale, 107 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
              i = StretchBlt(picStaff(TrackNo).hDC, 56 * StaffScale, 167 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            End If
            'always exit
            Exit Do
          Loop
        End If

        'if this is the selected track
        If TrackNo = SelTrack Then
          picStaff(TrackNo).DrawWidth = 3
          picStaff(TrackNo).FillStyle = vbFSTransparent
          'add track selection border
          picStaff(TrackNo).Line (0, 0)-(sngStaffWidth + (vsbStaff(TrackNo).Visible * vsbStaff(TrackNo).Width) - 2, picStaff(TrackNo).ScaleHeight - 2), vbBlue, B
          picStaff(TrackNo).DrawWidth = 1
          picStaff(TrackNo).FillStyle = vbFSSolid
          'reset cursor flag
          CursorOn = False
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub DrawNote(ByVal Track As Long, ByVal HPos As Long, ByVal NoteIndex As Long, ByVal Length As Long, ByVal Attenuation As Long)

        'draws note on staff;
        'Track identifies which note to draw
        'HPos is horizontal position where note will be drawn
        'Length is duration of note in AGI ticks
        'Attenuation is volume attenuation; 0 means full sound; 15 means silence

        Dim rtn As Long, dnNote As tDisplayNote
        Dim lngNoteTop As Long
        Dim i As Single, lngNPos As Long
        Dim lngDPos As Long, lngAPos As Long
        Dim lngTPos As Long, lngBPos As Long
        Dim lngColor As Long
        Dim lngTPQN As Long

        'make local copies of sound and note parameters
        lngTPQN = SoundEdit.TPQN

        'set note color based on attenuation
        lngColor = EGAColor(Attenuation)
        picStaff(Track).FillColor = lngColor

        'get note pos and accidental based on current key
        dnNote = DisplayNote(NoteIndex, SoundEdit.Key)

        'draw extra staffline if above or below staff
        For i = -6 To dnNote.Pos / 2 Step -1
          picStaff(Track).Line (HPos - 3 * StaffScale, (96 + 6 * (i + 5)) * StaffScale + SOVert(Track))-Step(12 * StaffScale, 0), vbBlack
        Next i

        If dnNote.Pos = 0 Then
          picStaff(Track).Line (HPos - 3 * StaffScale, 126 * StaffScale + SOVert(Track))-Step(12 * StaffScale, 0), vbBlack
        End If
        'based on note position, determine if it should be drawn rightside up or upside down
        'lngNoteTop is vertical offset on the Notes bitmap to the correctly oriented note
        'lngNPos is the absolute position on picScale where the bitmap needs to be placed
        '
        'when drawing notes as blocks, drawing dots, or drawing accidentals,
        'lngNPos is adjusted by an amount that results in correct placement
        'lngDPos is used for the adjusted Value of dots;
        'lngAPos is used for the adjusted Value of accidentals, ties, blocks

        'if negative (meaning note is above middle c)
        If dnNote.Pos <= 0 Then
          'notes above middle B(vpos<-6) are drawn upsidedown
          If dnNote.Pos < -6 Then
            lngNoteTop = 132
            'draw on treble staff
            lngNPos = (123 + (3 * dnNote.Pos)) * StaffScale + SOVert(Track)
            'set position for dots, blocks, accidentals and ties
            lngDPos = lngNPos + 3 * StaffScale
            lngBPos = lngDPos
            lngAPos = lngNPos - 4 * StaffScale
            lngTPos = lngNPos - 8 * StaffScale
          Else
            lngNoteTop = 0
            'draw on treble staff
            lngNPos = (107 + (3 * dnNote.Pos)) * StaffScale + SOVert(Track)
            'set position for dots, blocks, accidentals and ties
            lngDPos = lngNPos + 19 * StaffScale
            lngBPos = lngDPos
            lngAPos = lngNPos + 12 * StaffScale
            lngTPos = lngNPos + 24 * StaffScale
          End If
        Else
          'notes above middle B of bass staff(v<=6) are drawn upside down
          If dnNote.Pos < 6 Then
            lngNoteTop = 133
            'draw on bass staff
            lngNPos = (147 + (3 * dnNote.Pos)) * StaffScale + SOVert(Track)
            'set position for dots, blocks, accidentals and ties
            lngDPos = lngNPos + 3 * StaffScale
            lngBPos = lngDPos
            lngAPos = lngNPos - 4 * StaffScale
            lngTPos = lngNPos - 8 * StaffScale
          Else
            lngNoteTop = 0
            'draw on bass staff
            lngNPos = (131 + (3 * dnNote.Pos)) * StaffScale + SOVert(Track)
            'set position for dots, blocks, accidentals and ties
            lngDPos = lngNPos + 19 * StaffScale
            lngBPos = lngDPos
            lngAPos = lngNPos + 12 * StaffScale
            lngTPos = lngNPos + 24 * StaffScale
          End If
        End If

        'if note is on a line,
        If (Int(dnNote.Pos / 2) = dnNote.Pos / 2) Then
          'dot needs to be moved off the line
          lngDPos = lngDPos - 2 * StaffScale
        End If

        'if drawing notes as bitmaps
        If Settings.ShowNotes Then
          'convert length of note to MIDI Value, using TPQN
          Select Case Length / lngTPQN * 4
          Case 1  'sixteenth note
            'draw sixteenth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

          Case 2  'eighth note
            'draw eighth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 72, lngNoteTop, 72, 133, TRANSCOPY)

          Case 3  'eighth note dotted
            'draw eighth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 72, lngNoteTop, 72, 133, TRANSCOPY)
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor

          Case 4  'quarter note
            'draw quarter
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 144, lngNoteTop, 72, 133, TRANSCOPY)

          Case 5 'quater note tied to sixteenth note
            'draw quarter
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 144, lngNoteTop, 72, 133, TRANSCOPY)
            'add accidental, if necessary
            Select Case dnNote.Tone
            Case ntSharp
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
            Case ntFlat
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
            Case ntNatural
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
            End Select
            'draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                             NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
            'increment position
            HPos = HPos + StaffScale * TICK_WIDTH * lngTPQN

            'draw sixteenth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

          Case 6  'quarter note dotted
            'draw quarter
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 144, lngNoteTop, 72, 133, TRANSCOPY)
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, picStaff(Track).FillColor

           Case 7  'quarter note double dotted
            'draw quarter
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 144, lngNoteTop, 72, 133, TRANSCOPY)
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 13, lngDPos), StaffScale * 1.125, lngColor

          Case 8  'half note
            'draw half note
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)

          Case 9  'half note tied to sixteenth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
            'add accidental, if necessary
            Select Case dnNote.Tone
            Case ntSharp
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
            Case ntFlat
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
            Case ntNatural
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
            End Select
            'draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 2 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                               NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
            'increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
            'if note is on bottom, it trails past the staff mark; need to bump it back a little
            If lngNoteTop = 0 Then
              HPos = HPos - StaffScale * TICK_WIDTH * 1.25
            End If

            'draw sixteenth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

          Case 10 'half note tied to eighth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
            'add accidental, if necessary
            Select Case dnNote.Tone
            Case ntSharp
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
            Case ntFlat
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
            Case ntNatural
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
            End Select
            'draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 2 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                             NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
            'increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
            'if note is on bottom, it trails past the staff mark; need to bump it back a little
            If lngNoteTop = 0 Then
              HPos = HPos - StaffScale * TICK_WIDTH * 1.25
            End If

            'draw eighth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 72, lngNoteTop, 72, 133, TRANSCOPY)

          Case 11 'half note tied to dotted eighth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
            'add accidental, if necessary
            Select Case dnNote.Tone
            Case ntSharp
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
            Case ntFlat
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
            Case ntNatural
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
            End Select
            'draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 2 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                               NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
            'increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
            'if note is on bottom, it trails past the staff mark; need to bump it back a little
            If lngNoteTop = 0 Then
              HPos = HPos - StaffScale * TICK_WIDTH * 1.25
            End If

            'draw eighth note
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 72, lngNoteTop, 72, 133, TRANSCOPY)
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor

          Case 12 'half note dotted
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor

          Case 13 'half note dotted tied to sixteenth
             rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor
            'add accidental, if necessary
            Select Case dnNote.Tone
            Case ntSharp
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
            Case ntFlat
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
            Case ntNatural
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
            End Select
            'draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 3 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                               NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
            'increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 3 * lngTPQN
            'if note is on bottom, it trails past the staff mark; need to bump it back a little
            If lngNoteTop = 0 Then
              HPos = HPos - StaffScale * TICK_WIDTH * 1.25
            End If

            'draw sixteenth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

          Case 14 'half note double dotted
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 13, lngDPos), StaffScale * 1.125, lngColor

          Case 15 'half note double dotted tied to sixteenth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor
            'draw dot
            picStaff(Track).Circle (HPos + StaffScale * 13, lngDPos), StaffScale * 1.125, lngColor
            'add accidental, if necessary
            Select Case dnNote.Tone
            Case ntSharp
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
            Case ntFlat
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
            Case ntNatural
              rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                               NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
            End Select
            'draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 3.5 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                               NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
            'increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 3.5 * lngTPQN
            'if note is on bottom, it trails past the staff mark; need to bump it back a little
            If lngNoteTop = 0 Then
              HPos = HPos - StaffScale * TICK_WIDTH * 1.25
            End If

            'draw sixteenth
            rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

          Case 16 'whole note
             rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                             NotePictures.hDC, 288, lngNoteTop, 72, 133, TRANSCOPY)

          Case Is > 16
            'greater than whole note;
            'recurse to draw one whole note at a time until done
            Do Until Length < 4 * lngTPQN
              DrawNote Track, HPos, NoteIndex, 4 * lngTPQN, Attenuation
              'draw connector
              rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 4 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                               NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)

              'decrement length
              Length = Length - 4 * lngTPQN
              'increment horizontal position
              HPos = HPos + StaffScale * TICK_WIDTH * lngTPQN * 4
              'special case- if EXACTLY one sixteenth note left AND on bottom
              'bump it back a little
              If CSng(Length / lngTPQN * 4) = 1 And lngNoteTop = 0 Then
                HPos = HPos - StaffScale * TICK_WIDTH * 1.25
              End If

            Loop
            'if anything left
            If Length > 0 Then
              'draw remaining portion of note
              DrawNote Track, HPos, NoteIndex, Length, Attenuation
            End If
            'exit
            Exit Sub

          Case Else
            'not a normal note; draw a bar
            'this adjustment is interfering with the accidental position; need to reset lngNPos after drawing box
            picStaff(Track).Line (HPos, lngBPos - StaffScale * 3)-Step(StaffScale * TICK_WIDTH * (Length - 0.8), StaffScale * 6), lngColor, BF
          End Select
        Else
          'draw the block for this note
          picStaff(Track).Line (HPos, lngBPos - StaffScale * 3)-Step(StaffScale * TICK_WIDTH * (Length - 0.8), StaffScale * 6), lngColor, BF
        End If

        'add accidental, if necessary
        Select Case dnNote.Tone
        Case ntSharp
          rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                           NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
        Case ntFlat
          rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                           NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
        Case ntNatural
          rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                           NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
        End Select
      End Sub


      Private Sub HideCursor(ByVal HideTrack As Long)

        Timer1.Enabled = False
        'un-highlight previous selection
        If CursorOn Then
          'draw the cursor line, using invert pen
          With picStaff(HideTrack)
            .DrawWidth = 2
            .DrawMode = 6 'invert
            picStaff(HideTrack).Line (CursorPos, 0)-(CursorPos, picStaff(HideTrack).ScaleHeight), vbBlack
            .DrawWidth = 1
            .DrawMode = 13 'copy pen
          End With
          'set cursor status to off
          CursorOn = False
        End If
      End Sub

      Public Sub InitMIDI()

        Dim rtn As Long

        On Error GoTo ErrHandler

        'if sound is disabled,
        If Settings.NoMIDI Then
          Exit Sub
        End If

        'open and reset midi device so notes can be played as
        'user presses 'keys'
        rtn = midiOutOpen(hMIDI, -1, 0, 0, 0)
        If rtn <> 0 Then
          'sound error; disable midi
          MsgBox "An error was encountered while attempting to initialize MIDI device. MIDI playback features will be disabled.", vbCritical + vbOKOnly, "MIDI Device Error"
          Settings.NoMIDI = True
          frmMDIMain.mnuRCustom1.Enabled = False
          hMIDI = 0
          'disable the preview window, if its visible
          If PreviewWin.cmdPlay.Visible Then
            PreviewWin.cmdPlay.Enabled = False
          End If
          'use kill midi to reset buttons/menus
          KillMIDI
          Exit Sub
        End If

        'reset midi
        rtn = midiOutReset(hMIDI)

        'if a track is selected,
        If SelTrack >= 0 And SelTrack <= 2 Then
          'send instrument to midi
          midiOutShortMsg hMIDI, CLng(SoundEdit.Track(SelTrack).Instrument * &H100 + &HC0)
        Else
          'set instrument to 0 (it will be set correctly when a key is pressed)
          midiOutShortMsg hMIDI, &H1C0&
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub KillMIDI()

        On Error GoTo ErrHandler

        If hMIDI <> 0 Then
          'ensure midi is closed
          midiOutClose hMIDI
        End If
        hMIDI = 0

        'if playing a sound,
        'If frmMDIMain.mnuECustom1.Caption = "Stop Sound" & vbTab & "Ctrl+Enter" Then
        If cmdStop.Enabled Then
          SoundEdit.StopSound
        End If

        'set play buttons and menu to play status
        cmdStop.Enabled = False
        cmdPlay.Enabled = Not Settings.NoMIDI
        frmMDIMain.mnuECustom1.Enabled = Not Settings.NoMIDI
        frmMDIMain.mnuECustom1.Caption = "Play Sound" & vbTab & "Ctrl+Enter"
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Public Sub MenuClickCustom1()

        Dim i As Long

        'toggle onetrack setting
        OneTrack = Not OneTrack

        'update menu and update display
        If OneTrack Then
          frmMDIMain.mnuRCustom1.Caption = "Show All Visible Tracks"
          DrawStaff -2
        Else
          frmMDIMain.mnuRCustom1.Caption = "Show Only Selected Track"
          'need to show all tracks that are visible
          For i = 0 To 3
            If SoundEdit.Track(i).Visible Then
              picStaff(i).Visible = True
              picStaffVis(i) = True
            Else
              picStaff(i).Visible = False
              picStaffVis(i) = False
            End If
          Next i
          'use form resize to redraw
          Form_Resize
        End If

      End Sub

      Public Sub MenuClickHelp()

        On Error Resume Next

        'help
        HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Sound_Editor.htm"
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Function NoteFromPos(ByVal TrackNo As Long, ByVal Pos As Long, Optional ByVal RoundUp As Boolean = False) As Long
        'converts an X position into a note's index number for a given track

        Dim i As Long, lngNoteCount As Long
        Dim lngHPos As Long, lngDur As Long

        'start at first note, offset by clef and Key signature
        lngHPos = SOHorz + (5 + Abs(SoundEdit.Key)) * StaffScale * 6
        If lngHPos < SOHorz + 36 * StaffScale Then
          lngHPos = SOHorz + 36 * StaffScale
        End If

        lngNoteCount = SoundEdit.Track(TrackNo).Notes.Count - 1
        'step through all notes in this track
        For i = 0 To lngNoteCount
          'get note dur
          lngDur = SoundEdit.Track(TrackNo).Notes(i).Duration

          'if rounding
          If RoundUp Then
            'if 1/2 of note extends past this position
            If lngHPos + (StaffScale * TICK_WIDTH * lngDur) / 2 > Pos Then
              'this is note
              Exit For
            End If

          'if not rounding,
          Else
            'if note extends past this position,
            If lngHPos + StaffScale * TICK_WIDTH * lngDur > Pos Then
              'this is the note
              Exit For
            End If
          End If

          'increment position counter
          lngHPos = lngHPos + StaffScale * TICK_WIDTH * lngDur
        Next i

        'if loop is exited normally, i will equal Notes.Count
        'which means cursor is positioned at end of track
        'and added notes will be at end

        NoteFromPos = i
      End Function

      Private Sub NoteOff(ByVal Note As Long)
        'turns off the note currently being played
        'then closed midi device

        Dim rtn As Long, NoteNum As Long
        Dim lngKeyW As Long

        On Error GoTo ErrHandler

        'if a note is playing
        If blnNoteOn Then
          If Not Settings.NoMIDI And (hMIDI <> 0) Then
            'need to recalculate noise notes
            If SelTrack = 3 Then
              'set correct instrument
              If Note <= 3 Then
                'tone noise

                'change instrument
                midiOutShortMsg hMIDI, &H6C0 'inst#6; (msg val is 122 * &H100 + &HC0)
              Else
                'white noise

                'change instrument
                midiOutShortMsg hMIDI, &H7AC0 'inst#122; (msg val is 122 * &H100 + &HC0)
              End If

              'calculate correct note value
              Note = CByte((Log10(CInt(2330.4296875 / 2 ^ (Note And 3))) / LOG10_1_12) - 64)
            End If

            'now turn off note
            rtn = midiOutShortMsg(hMIDI, &H80 + Note * &H100)
          End If

          If SelTrack <> 3 Then
            'convert midinote into key position
            NoteNum = Note Mod 12
            'reverse the calculation in picKeyboard_MouseDown
            If NoteNum > 4 Then
              rtn = NoteNum + 1
            Else
              rtn = NoteNum
            End If
            rtn = 7 * ((Note \ 12) - 3) + rtn / 2 - MKbOffset + 40

            'restore key
            picKeyboard.FontBold = ((Note \ 12) = DefOctave)
            Select Case NoteNum
            Case 0, 5 'C, F
              'draw key in magenta
              picKeyboard.Line (rtn * 24 + 1, 33)-(rtn * 24 + 23, 62), vbWhite, BF
              picKeyboard.Line (rtn * 24 + 1, 1)-(rtn * 24 + 17, 32), vbWhite, BF
              'add key labels
              picKeyboard.CurrentX = rtn * 24 + 9
              picKeyboard.CurrentY = 48
              picKeyboard.Print IIf(NoteNum, "F", "C");
              'if note is 'c'
              If NoteNum = 0 Then
                picKeyboard.CurrentX = rtn * 24 + 6
                picKeyboard.CurrentY = 34
                picKeyboard.Print Note \ 12;
              End If

            Case 4, 11 'E, B
              'draw key in magenta
              picKeyboard.Line (rtn * 24 + 1, 33)-(rtn * 24 + 23, 62), vbWhite, BF
              picKeyboard.Line (rtn * 24 + 7, 1)-(rtn * 24 + 23, 32), vbWhite, BF
              'add key labels
              picKeyboard.CurrentX = rtn * 24 + 9
              picKeyboard.CurrentY = 48
              picKeyboard.Print IIf(NoteNum = 4, "E", "B");

            Case 2, 7, 9 'D, G, A
              'draw key in magenta
              picKeyboard.Line (rtn * 24 + 1, 33)-(rtn * 24 + 23, 62), vbWhite, BF
              picKeyboard.Line (rtn * 24 + 7, 1)-(rtn * 24 + 17, 32), vbWhite, BF
              'add key labels
              picKeyboard.CurrentX = rtn * 24 + 9
              picKeyboard.CurrentY = 48
              If NoteNum = 2 Then
                picKeyboard.Print "D";
              ElseIf NoteNum = 7 Then
                picKeyboard.Print "G";
              Else
                picKeyboard.Print "A";
              End If

            Case Else 'black key
              'recalculate key
              If NoteNum < 4 Then
                rtn = NoteNum + 1
              Else
                rtn = NoteNum + 2
              End If
              rtn = 14 * ((Note \ 12) - 3) + rtn - (MKbOffset - 40) * 2
              'draw key in black
              picKeyboard.Line (rtn * 12 - 6, 0)-(rtn * 12 + 6, 32), vbBlack, BF

            End Select
          Else
            'restore the key
            lngKeyW = picKeyboard.ScaleWidth / 8
            If lngKeyW < 30 Then lngKeyW = 30
            'white out area of key being pressed
            picKeyboard.Line (Note * lngKeyW - NKbOffset + 1, 33)-((Note + 1) * lngKeyW - NKbOffset - 1, 62), vbWhite, BF
            'add right border line
            picKeyboard.Line (picKeyboard.ScaleWidth - 1, 0)-Step(0, 63), vbBlack
            'set x and Y for key text
            picKeyboard.CurrentY = 49
            Select Case Note
            Case 0, 4
              picKeyboard.CurrentX = (1 + 2 * (Note And 4)) * lngKeyW / 2 - 12 - NKbOffset
              picKeyboard.Print "2330";
            Case 1, 5
              picKeyboard.CurrentX = (3 + 2 * (Note And 4)) * lngKeyW / 2 - 12 - NKbOffset
              picKeyboard.Print "1165";
            Case 2, 6
              picKeyboard.CurrentX = (5 + 2 * (Note And 4)) * lngKeyW / 2 - 9 - NKbOffset
              picKeyboard.Print "583";
            Case 3, 7
              picKeyboard.CurrentX = (7 + 2 * (Note And 4)) * lngKeyW / 2 - 14 - NKbOffset
              picKeyboard.Print "TRK2";
            End Select
          End If

          'set flag
          blnNoteOn = False
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub NoteOn(ByVal Note As Long, ByVal InsertNote As Boolean)
        'open midi device, send current instrument code,
        'then play the note
        'toggle keyboard display

        'if InsertNote is true, note is added at SelStart in SelTrack

        Dim rtn As Long, lngKeyW As Long
        Dim NoteNum As Long, Octave As Long
        Dim AddNoteCol As AGINotes
        Dim blnReplace As Boolean
        Dim intOffset As Integer

        On Error GoTo ErrHandler

        'if a note is already playing
        If blnNoteOn Then
          Exit Sub
        End If

        blnNoteOn = True

        'if not blocking midi (and not playing a sound)
        If Not Settings.NoMIDI And (hMIDI <> 0) Then
          'send note on
          If SelTrack = 3 Then
            'set correct instrument
            If Note <= 3 Then
              'tone noise

              'change instrument
              midiOutShortMsg hMIDI, &H6C0 'inst#6; (msg val is 122 * &H100 + &HC0)
            Else
              'white noise

              'change instrument
              midiOutShortMsg hMIDI, &H7AC0 'inst#122; (msg val is 122 * &H100 + &HC0)
            End If

            'calculate correct note value
            Note = CByte((Log10(CInt(2330.4296875 / 2 ^ (Note And 3))) / LOG10_1_12) - 64)
          Else
            'change instrument
            midiOutShortMsg hMIDI, SoundEdit.Track(SelTrack).Instrument * &H100 + &HC0
          End If

          'now play the note
          rtn = midiOutShortMsg(hMIDI, Note * &H100 + &H7F0090)
        End If

        'if not noise track
        If SelTrack <> 3 Then
          'convert midinote into key position
          NoteNum = Note Mod 12
          'reverse the calculation in picKeyboard_MouseDown
          If NoteNum > 4 Then
            rtn = NoteNum + 1
          Else
            rtn = NoteNum
          End If
          rtn = 7 * ((Note \ 12) - 3) + rtn / 2 - MKbOffset + 40

          'flash key
          Select Case NoteNum
          Case 0, 5 'C and F
            'draw key in magenta
            picKeyboard.Line (rtn * 24 + 1, 33)-(rtn * 24 + 23, 62), vbMagenta, BF
            picKeyboard.Line (rtn * 24 + 1, 1)-(rtn * 24 + 17, 32), vbMagenta, BF
          Case 4, 11 'B and E
            'draw key in magenta
            picKeyboard.Line (rtn * 24 + 1, 33)-(rtn * 24 + 23, 62), vbMagenta, BF
            picKeyboard.Line (rtn * 24 + 7, 1)-(rtn * 24 + 23, 32), vbMagenta, BF
          Case 2, 7, 9 'A, D, G
            'draw key in magenta
            picKeyboard.Line (rtn * 24 + 1, 33)-(rtn * 24 + 23, 62), vbMagenta, BF
            picKeyboard.Line (rtn * 24 + 7, 1)-(rtn * 24 + 17, 32), vbMagenta, BF
          Case Else 'black key
            'recalculate key
            If NoteNum < 4 Then
              rtn = NoteNum + 1
            Else
              rtn = NoteNum + 2
            End If
            rtn = 14 * ((Note \ 12) - 3) + rtn - (MKbOffset - 40) * 2

            'draw key in magenta
            picKeyboard.Line (rtn * 12 - 6, 0)-(rtn * 12 + 6, 32), vbMagenta, BF
          End Select
        Else
          'flash noise key
          lngKeyW = picKeyboard.ScaleWidth / 8
          If lngKeyW < 30 Then lngKeyW = 30
          picKeyboard.Line (Note * lngKeyW - NKbOffset + 1, 33)-((Note + 1) * lngKeyW - NKbOffset - 1, 62), vbMagenta, BF
        End If

        'if not inserting note,
        If Not InsertNote Then
          Exit Sub
        End If

        'if a selection needs to be deleted
        If SelLength > 0 Then
          'delete selection first
          DeleteNotes SelTrack, SelStart, SelLength
          blnReplace = True
        End If

        'add new note
        Set AddNoteCol = New AGINotes

        If SelTrack <> 3 Then
          'convert note to freq divisor and add
          AddNoteCol.Add NoteToFreq(Note), SoundEdit.TPQN * DefLength / 4, IIf(blnMute, 15, DefAttn)
        Else
          'note doesn't need conversion
          AddNoteCol.Add Note, SoundEdit.TPQN * DefLength / 4, IIf(blnMute, 15, DefAttn)
        End If

        'add the note
        AddNotes SelTrack, SelStart, AddNoteCol, False

        'adjust undo so it displays 'add note'
        UndoCol(UndoCol.Count).UDAction = udsAddNote
        'if replacing,
        If blnReplace Then
          'set flag in undo item
          UndoCol(UndoCol.Count).UDText = "R"
        End If

        SetEditMenu
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Function NotePos(ByVal Track As Long, ByVal NoteNumber As Long) As Long

        'returns the timepostion of a note
        'Track and NoteNumber should be validated BEFORE calling this function

        Dim i As Long

        On Error GoTo ErrHandler

        With SoundEdit.Track(Track).Notes
          'if looking for a note past end, return the end
          If NoteNumber > .Count Then
            NoteNumber = .Count
          End If

          For i = 0 To NoteNumber - 1
            NotePos = NotePos + .Item(i).Duration
          Next i
        End With

      Exit Function

      ErrHandler:
        Resume Next
      End Function

      Private Sub SelectPropFromList()

        Dim i As Long, j As Long
        Dim lngNoteIndex As Long
        Dim NextUndo As SoundUndo
        Dim NoChange As Boolean

        On Error GoTo ErrHandler

        'create undo object
        Set NextUndo = New SoundUndo

        'get note index values
        lngNoteIndex = Val(tvwSound.SelectedItem.Tag)

        'update property that was edited
        Select Case lstProperty.Tag
        Case "KEY"
          If SoundEdit.Key <> lstProperty.ListIndex - 7 Then
            NextUndo.UDAction = udsChangeKey
            NextUndo.UDStart = SoundEdit.Key
            SoundEdit.Key = lstProperty.ListIndex - 7
            SetKeyWidth
            'use force-redraw to update display
            ChangeSelection SelTrack, SelStart, SelLength, True, False, True '
          Else
            NoChange = True
          End If

        Case "INST"
          If SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument <> lstProperty.ListIndex Then
            NextUndo.UDAction = udsChangeInstrument
            NextUndo.UDTrack = tvwSound.SelectedItem.Index - 2
            NextUndo.UDStart = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument
            SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument = lstProperty.ListIndex
            'adjust current instrument for keypresses/keyboard
            If Not Settings.NoMIDI And hMIDI <> 0 Then
              midiOutShortMsg hMIDI, lstProperty.ListIndex * &H100 + &HC0
            End If
          Else
            NoChange = True
          End If

        Case "MUTE"
          SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Muted = (lstProperty.ListIndex = 1)
          'not undoable
          NoChange = True
          picMute_Paint tvwSound.SelectedItem.Index - 2

        Case "TYPE"
          'if changed
          If ((SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor And 4) = 4 And lstProperty.ListIndex = 0) Or _
             (((SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor And 4) = 0) And (lstProperty.ListIndex = 1)) Then
            'set undo properties
            NextUndo.UDAction = udsEditNote
            NextUndo.UDTrack = 3
            NextUndo.UDStart = CLng(lngNoteIndex)
            NextUndo.UDText = SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor
            NextUndo.UDLength = 1
            'if user chose white noise
            If lstProperty.ListIndex Then
              'set bit in freq divisor
              SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor = _
                 SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor Or 4
            Else
              'clear bit in freq divisor
              SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor = _
                 SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor And 3
            End If
            '*'Debug.Assert SelTrack = 3
            'use force-redraw to update display
            ChangeSelection SelTrack, SelStart, SelLength, True, False, True '
          Else
            NoChange = True
          End If

        Case "NOISEFREQ"
          If (SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor And 3) <> lstProperty.ListIndex Then
            'set undo properties
            NextUndo.UDAction = udsEditNote
            NextUndo.UDTrack = 3
            NextUndo.UDStart = CLng(lngNoteIndex)
            NextUndo.UDText = SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor
            NextUndo.UDLength = 1
            'set noise freq
            SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor = _
            (SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor And 4) + lstProperty.ListIndex
            '*'Debug.Assert SelTrack = 3
            'use force-redraw to update display
            ChangeSelection SelTrack, SelStart, SelLength, True, False, True '
          Else
            NoChange = True
          End If

        Case "MNOTE"
          'set freq based on new midinote
          i = 11 - lstProperty.ListIndex
          'if in last octave (only eight notes available)
          If lstProperty.ListCount = 8 Then
            'adjust by 4
            i = i - 4
          End If
          If (MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor) Mod 12) <> i Then
            'get current octave
            j = MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor) \ 12
            'set undo properties
            NextUndo.UDAction = udsEditNote
            NextUndo.UDTrack = tvwSound.SelectedItem.Parent.Index - 2
            NextUndo.UDStart = CLng(lngNoteIndex)
            NextUndo.UDText = SoundEdit.Track(NextUndo.UDTrack).Notes(lngNoteIndex).FreqDivisor
            NextUndo.UDLength = 1
            'calculate new freqdivisor
            SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor = NoteToFreq(i + 12 * j)
            '*'Debug.Assert SelTrack = tvwSound.SelectedItem.Parent.Index - 2
            'use force-redraw to update display
            ChangeSelection SelTrack, SelStart, SelLength, True, False, True '
          Else
            NoChange = True
          End If

        Case "MOCT"
          'calculate new octave (middle c is defined as octave 5, since middle c = 60
          j = CLng(lstProperty.Text)
          If (MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor) \ 12) <> j Then
            'get midinote
            i = MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor) Mod 12
            'set undo properties
            NextUndo.UDAction = udsEditNote
            NextUndo.UDTrack = tvwSound.SelectedItem.Parent.Index - 2
            NextUndo.UDStart = CLng(lngNoteIndex)
            NextUndo.UDText = SoundEdit.Track(NextUndo.UDTrack).Notes(lngNoteIndex).FreqDivisor
            NextUndo.UDLength = 1
            'calculate new freqdivisor
            SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor = NoteToFreq(i + 12 * j)
            '*'Debug.Assert SelTrack = tvwSound.SelectedItem.Parent.Index - 2
            'use force-redraw to update display
            ChangeSelection SelTrack, SelStart, SelLength, True, False, True '
          Else
            NoChange = True
          End If

        Case "MLEN"
          If SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).Duration <> (lstProperty.ListIndex + 1) * SoundEdit.TPQN / 4 Then
            'set undo properties
            NextUndo.UDAction = udsEditNote
            NextUndo.UDTrack = tvwSound.SelectedItem.Parent.Index - 2
            NextUndo.UDStart = CLng(lngNoteIndex)
            NextUndo.UDText = SoundEdit.Track(NextUndo.UDTrack).Notes(lngNoteIndex).Duration
            NextUndo.UDLength = 2
            'set new duration
            SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).Duration = (lstProperty.ListIndex + 1) * SoundEdit.TPQN / 4

            'update scrollbar based on new note length
            SetHScroll
            '*'Debug.Assert SelTrack = tvwSound.SelectedItem.Parent.Index - 2
            'use force-redraw to update display
            ChangeSelection SelTrack, SelStart, SelLength, True, False, True '
          Else
            NoChange = True
          End If

        Case "VIS"
          'show/hide this track
          SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible = (lstProperty.ListIndex = 1)
          picStaff(tvwSound.SelectedItem.Index - 2).Visible = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible
          picStaffVis(tvwSound.SelectedItem.Index - 2) = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible
          'use resize to force redraw
          Form_Resize
          'not undoable
          NoChange = True
        End Select

        'if there is a change
        If Not NoChange And Settings.SndUndo <> 0 Then
          'add undo
          AddUndo NextUndo
        End If

        'hide listbox
        lstProperty.Visible = False
        'force repaint
        PaintPropertyWindow
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub SelectPropFromText()

        Dim tmpVal As Long, tmpNode As Node
        Dim NextUndo As SoundUndo
        Dim NoChange As Boolean

        On Error GoTo ErrHandler

        'create undo object
        Set NextUndo = New SoundUndo

        'save property that was edited
        Select Case txtProperty.Tag
        Case "TPQN"
          'validate
          tmpVal = CLng(txtProperty.Text)
          If tmpVal < 4 Then tmpVal = 4
          If tmpVal > 64 Then tmpVal = 64
          tmpVal = (tmpVal \ 4) * 4
          If tmpVal <> SoundEdit.TPQN Then
            'set undo properties
            NextUndo.UDAction = udsChangeTPQN
            NextUndo.UDStart = SoundEdit.TPQN
            SoundEdit.TPQN = tmpVal
            DrawStaff -1
          Else
            'no change
            NoChange = True
          End If

        Case "DUR"
        '*'Debug.Assert SelTrack = tvwSound.SelectedItem.Parent.Index - 2
        '*'Debug.Assert SelTrack <> -1 And SelStart >= 0 And SelLength <= 1

          If SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).Duration <> CLng(txtProperty.Text) Then
            'change note length for this note
            ShiftDuration CLng(txtProperty.Text)
            'since ShiftDuration method adds undo, set
            'NoChange to true so we don't get a blank undo
            'added below
            NoChange = True
          Else
            NoChange = True
          End If

        Case "ATT"
          tmpVal = CLng(Val(txtProperty.Text))
          If tmpVal > 15 Then
            tmpVal = 15
          End If
          If SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).Attenuation <> CLng(txtProperty.Text) Then
            'set undo properties
            NextUndo.UDAction = udsEditNote
            NextUndo.UDTrack = tvwSound.SelectedItem.Parent.Index - 2
            NextUndo.UDStart = CLng(tvwSound.SelectedItem.Tag)
            NextUndo.UDText = SoundEdit.Track(NextUndo.UDTrack).Notes(tvwSound.SelectedItem.Tag).Attenuation
            NextUndo.UDLength = 3
            'make change
            SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).Attenuation = tmpVal

            '*'Debug.Assert SelTrack = tvwSound.SelectedItem.Parent.Index - 2
            '*'Debug.Assert SelStart = CLng(tvwSound.SelectedItem.Tag)
            ChangeSelection SelTrack, SelStart, 1, False, False, True
          Else
            NoChange = True
          End If

        Case "FREQ"
          tmpVal = CLng(Val(txtProperty.Text))
          If tmpVal > 1023 Then
            tmpVal = 1023
          End If
          If SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor <> CLng(txtProperty.Text) Then
            'set undo properties
            NextUndo.UDAction = udsEditNote
            NextUndo.UDTrack = tvwSound.SelectedItem.Parent.Index - 2
            NextUndo.UDStart = CLng(tvwSound.SelectedItem.Tag)
            NextUndo.UDText = SoundEdit.Track(NextUndo.UDTrack).Notes(tvwSound.SelectedItem.Tag).FreqDivisor
            NextUndo.UDLength = 1
            'make change
            SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor = tmpVal

            '*'Debug.Assert SelTrack = tvwSound.SelectedItem.Parent.Index - 2
            '*'Debug.Assert SelStart = CLng(tvwSound.SelectedItem.Tag)
            ChangeSelection SelTrack, SelStart, 1, False, False, True
          Else
            NoChange = True
          End If
        End Select

        'if there is a change
        If Not NoChange And Settings.SndUndo <> 0 Then
          'add undo
          AddUndo NextUndo
        End If

        'hide
        txtProperty.Visible = False
        'force repaint
        PaintPropertyWindow
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub SetHScroll()

        Dim tmpWidth As Long
        Dim i As Long
        Dim lngKybdHt As Long

        On Error GoTo ErrHandler

        lngKybdHt = picKeyboard.Visible * -picKeyboard.Height

        'position horizontal scrollbar
        hsbStaff.Top = CalcHeight - lngKybdHt - hsbStaff.Height
        hsbStaff.Width = CalcWidth - hsbStaff.Left

        'if any vertical scroll bars
        For i = 0 To 3
          If SoundEdit.Track(i).Visible And vsbStaff(i).Visible Then
            hsbStaff.Width = hsbStaff.Width - 17
            Exit For
          End If
        Next i

        'calculate width of sound that doesn't fit in display
        '(total sound length minus what will fit in display;
        ' value is negative if the sound is small enough to fit
        ' without a scrollbar)
        tmpWidth = SoundEdit.Length * 60 + (KeyWidth + 24 - hsbStaff.Width) / (TICK_WIDTH * StaffScale)

        'show horizontal scrollbar if necessary
        hsbStaff.Visible = (tmpWidth > 0)
        If hsbStaff.Visible Then
          'if Max width is exceeded (which is approx 8 minutes or so)
          'just ignore the error; some of the sound won't be visible
          hsbStaff.Max = tmpWidth
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub SetKeyboardScroll()

        'set keyboard scrollbar properties
        If SelTrack = 3 Then
          hsbKeyboard.Min = 0
          hsbKeyboard.Enabled = (picKeyboard.ScaleWidth / 8 < 30)
          If hsbKeyboard.Enabled Then
            hsbKeyboard.Max = 240 - picKeyboard.ScaleWidth
          End If
          hsbKeyboard.LargeChange = 10
          hsbKeyboard.SmallChange = 10

        Else
          hsbKeyboard.Min = 45
          'enable scrollbar only if all keys don't fit
          hsbKeyboard.Enabled = (picKeyboard.Width \ 24) < 49
          If hsbKeyboard.Enabled Then
            hsbKeyboard.Max = 93 - picKeyboard.Width \ 24
          End If
          hsbKeyboard.SmallChange = 1
          hsbKeyboard.LargeChange = 1
        End If
      End Sub


      Private Sub SetKeyWidth()

        KeyWidth = (5 + Abs(SoundEdit.Key)) * 6 * StaffScale
        If KeyWidth < 36 * StaffScale Then
          KeyWidth = 36 * StaffScale
        End If
      End Sub

      Private Sub SetPropertyBoxPos()

        'set property box height
        picProperties.Height = (PropRowCount + 1) * PropRowHeight + 1

        'move property box and set treeview height
        picProperties.Top = CalcHeight - picProperties.Height
        tvwSound.Height = CalcHeight - tvwSound.Top - picProperties.Height
      End Sub

      Private Sub SetVScroll(ByVal Index As Long, ByVal PrevScale As Long)

        'shows/hides vertical scrollbar, and
        'adjust position so staves stay in same relative
        'position after redrawing

        Dim intNewScroll As Integer
        Dim intMax As Integer, intSH0 As Integer

        On Error GoTo ErrHandler

        With vsbStaff(Index)
          'move scrollbar to end
          .Left = picStaff(Index).ScaleWidth - .Width
          .Height = picStaff(Index).ScaleHeight

          'reset height
          .Height = picStaff(Index).ScaleHeight

          'calculate amount of staff height that
          'exceeds picstaff scaleheight
          Select Case Index
          Case 0, 1, 2  'for music tracks
            intMax = 186 * StaffScale - picStaff(Index).ScaleHeight
            intSH0 = 186 * PrevScale
            intSH0 = vsbStaff(Index).Value + picStaff(Index).ScaleHeight / PrevScale
          Case 3        'noise track
            intMax = 50 * StaffScale - picStaff(Index).ScaleHeight
            intSH0 = 50 * PrevScale
          End Select

          'reset vertical scrollbar or hide, if not needed
          .Visible = (intMax > 0)

          If .Visible Then
            'if Max has changed,
            If .Max <> intMax Then
              'disable updates
              blnDontDraw = True
              'if vsb was not previously visible,
              If .Max = -1 Then
                Select Case Index
                Case 0, 1, 2
                  'set new scroll position to show
                  'treble clef at bottom of picStaff
                  intNewScroll = (132 * StaffScale / PrevScale) - picStaff(Index).ScaleHeight
                Case 3
                  'set new scroll position to show
                  'clef at bottom of picStaff
                  intNewScroll = intMax
                End Select
              Else
                'calculate the new scroll position
                intNewScroll = intSH0
              End If

              'reset Max
              .Max = intMax
              'validate new scroll Value
              If intNewScroll < 0 Then intNewScroll = 0
              If intNewScroll > intMax Then intNewScroll = intMax
              'set Value
              .Value = intNewScroll
              'and offset
              SOVert(Index) = -intNewScroll
              'restore drawing
              blnDontDraw = False
            End If
          Else
            'reset offset
            SOVert(Index) = 0
            .Value = 0
            .Max = -1
          End If
          'update small and large change values
          .SmallChange = picStaff(Index).Height * SM_SCROLL
          .LargeChange = picStaff(Index).Height * LG_SCROLL

        End With
      Exit Sub

      ErrHandler:
        Resume Next
      End Sub

      Private Sub ShiftDuration(ByVal NewLength As Long)

        'shift (change) duration for selected note to new value
        '
        'NOTE: this only works for a single note; can't adjust
        'duration of a group of notes

        Dim NextUndo As SoundUndo

        On Error GoTo ErrHandler

        Set NextUndo = New SoundUndo

        'allow editing of a single selected note, or no selection
        '(which means cursor note is being edited)
        If SelTrack <> -1 And SelStart >= 0 And SelLength <= 1 Then
          'one note- change it
          '*'Debug.Assert SelTrack = tvwSound.SelectedItem.Parent.Index - 2
          'set undo properties
          NextUndo.UDAction = udsEditNote
          NextUndo.UDTrack = SelTrack
          NextUndo.UDStart = CLng(tvwSound.SelectedItem.Tag)
          NextUndo.UDText = SoundEdit.Track(SelTrack).Notes(tvwSound.SelectedItem.Tag).Duration
          NextUndo.UDLength = 2

          'make change
          SoundEdit.Track(SelTrack).Notes(tvwSound.SelectedItem.Tag).Duration = NewLength

          'add undo
          If Settings.SndUndo <> 0 Then
            AddUndo NextUndo
          End If

          'reset horizontal scrollbar
          SetHScroll
          'use force-redraw to update display
          ChangeSelection SelTrack, SelStart, SelLength, True, False, True
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub ShiftTone(ByVal Dir As Long)

        'shifts notes up(dir=1) or down(dir=-1)
        On Error GoTo ErrHandler

        Dim NextUndo As SoundUndo
        Dim strOldNotes As String
        Dim i As Long, lngNew As Long

        'must have an active track, and an active selection
        If SelTrack < 0 Or SelTrack > 2 Or SelStart < 0 Or SelLength < 1 Then
          Exit Sub
        End If

        'validate direction
        If Sgn(Dir) = 0 Then
          Exit Sub
        End If

        'set undo object
        Set NextUndo = New SoundUndo
        NextUndo.UDAction = udsShiftKey
        NextUndo.UDTrack = SelTrack
        NextUndo.UDStart = SelStart
        NextUndo.UDLength = SelLength

        'step through notes
        For i = 0 To SelLength - 1
          'save old note
          strOldNotes = strOldNotes & ChrW$(SoundEdit.Track(SelTrack).Notes(SelStart + i).FreqDivisor \ &H100) & _
                                      ChrW$(SoundEdit.Track(SelTrack).Notes(SelStart + i).FreqDivisor Mod &H100)
          'shift note if possible
          lngNew = MIDINote(SoundEdit.Track(SelTrack).Notes(SelStart + i).FreqDivisor) + Sgn(Dir)

          'if <limit
          If lngNew < 45 Then lngNew = 45
          'if >limit
          If lngNew > 127 Then lngNew = 127
          'special check for high notes that get skipped
          If Sgn(Dir) = 1 Then
            'going up
            If lngNew = 121 Then lngNew = 122
            If lngNew = 124 Then lngNew = 125
            If lngNew = 126 Then lngNew = 127
          Else
            'going down
            If lngNew = 126 Then lngNew = 125
            If lngNew = 124 Then lngNew = 123
            If lngNew = 121 Then lngNew = 120
          End If

          'save note
          SoundEdit.Track(SelTrack).Notes(SelStart + i).FreqDivisor = NoteToFreq(lngNew)
        Next i

        If Settings.SndUndo <> 0 Then
          'add undo
          NextUndo.UDText = strOldNotes
          AddUndo NextUndo
        End If

        'use force-redraw to update display
        ChangeSelection SelTrack, SelStart, SelLength, True, False, True '
      Exit Sub

      ErrHandler:
        Resume Next
      End Sub


      Private Sub ShiftVol(ByVal Dir As Long)

        'shifts note volume up(dir=1) or down(dir=-1)
        '
        'NOTE: for vol up, attenuation is decreased;
        'for vol down, attenuation is increased

        On Error GoTo ErrHandler

        Dim NextUndo As SoundUndo
        Dim strOldNotes As String
        Dim i As Long, lngNew As Long

        'must have an active track, and an active selection
        If SelTrack < 0 Or SelTrack > 2 Or SelStart < 0 Or SelLength < 1 Then
          Exit Sub
        End If

        'validate direction
        If Sgn(Dir) = 0 Then
          Exit Sub
        End If

        'set undo object
        Set NextUndo = New SoundUndo
        NextUndo.UDAction = udsShiftVol
        NextUndo.UDTrack = SelTrack
        NextUndo.UDStart = SelStart
        NextUndo.UDLength = SelLength

        'step through notes
        For i = 0 To SelLength - 1
          'save old note
          lngNew = SoundEdit.Track(SelTrack).Notes(SelStart + i).Attenuation
          strOldNotes = strOldNotes & ChrW$(lngNew)
          'calculate new attenuation (opposite of vol change direction!)
          lngNew = lngNew - Sgn(Dir)
          'validate
          If lngNew < 0 Then lngNew = 0
          If lngNew > 15 Then lngNew = 15

          'save note
          SoundEdit.Track(SelTrack).Notes(SelStart + i).Attenuation = lngNew
        Next i

        'add undo
        If Settings.SndUndo <> 0 Then
          NextUndo.UDText = strOldNotes
          AddUndo NextUndo
        End If

        'use force-redraw to update display
        ChangeSelection SelTrack, SelStart, SelLength, True, False, True '
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub ShowSelection()

        Dim tmpNode As Node, lngStartPos As Long, lngEndPos As Long
        Dim i As Long

        'assumes that:
        '  a track is selected (SelTrack >= 0) AND
        '  track is visible (picStaffVis(SelTrack) = True) AND
        '  zero or more notes currently selected (SelStart >=0 And SelLength >=0) AND
        '  selection is NOT actively displayed (SelActive = False)

        On Error GoTo ErrHandler

        '*'Debug.Assert picStaffVis(SelTrack) = True
        If Not picStaffVis(SelTrack) Then
          Exit Sub
        End If

        'calculate selstart pos
        lngStartPos = SOHorz + KeyWidth + NotePos(SelTrack, SelStart) * TICK_WIDTH * StaffScale

        If SelLength = 0 Then
          'cursorpos is lngstartpos; if cursorpos is less than keywidth, or if
          'cursorpos is greater than right edge, don't draw cursor

          If lngStartPos >= KeyWidth And lngStartPos <= picStaff(SelTrack).Width - 8 + vsbStaff(SelTrack).Visible * 17 Then
            CursorPos = lngStartPos
            'enable cursor
            Timer1.Enabled = True
          End If
        Else
          'set draw mode to invert
          picStaff(SelTrack).DrawMode = 6

          'calculate end pos
          lngEndPos = SOHorz + KeyWidth + NotePos(SelTrack, SelStart + SelLength) * TICK_WIDTH * StaffScale - 2

          'if not completely off left or right edge
          If lngStartPos <= picStaff(SelTrack).Width - 8 + vsbStaff(SelTrack).Visible * 17 And lngEndPos >= KeyWidth Then
            'if starting pos <keywidth,
            If lngStartPos < KeyWidth Then
              lngStartPos = KeyWidth

            End If

            'if end note is past edge of staff
            If lngEndPos > picStaff(SelTrack).Width - 8 + vsbStaff(SelTrack).Visible * 17 Then
              lngEndPos = picStaff(SelTrack).Width - 8 + vsbStaff(SelTrack).Visible * 17
            End If

            'draw box over selection
            picStaff(SelTrack).Line (lngStartPos, 2)-(lngEndPos, picStaff(SelTrack).ScaleHeight - 4), vbBlack, BF

            'mode to copypen
            picStaff(SelTrack).DrawMode = 13

            SelActive = True
          End If
        End If

        'redraw property window
        PaintPropertyWindow
        'update edit menu
        SetEditMenu
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Public Sub UpdateID(ByVal NewID As String, NewDescription As String)

        On Error GoTo ErrHandler

        If SoundEdit.Description <> NewDescription Then
          'change the SoundEdit object's description
          SoundEdit.Description = NewDescription
          'if node 1 is selected
          If tvwSound.SelectedItem.Index = 1 Then
            'force redraw
            PaintPropertyWindow
          End If
        End If

        If SoundEdit.ID <> NewID Then

          'change the SoundEdit object's ID and caption
          SoundEdit.ID = NewID

          'if soundedit is dirty
          If Asc(Caption) = 42 Then
            Caption = sDM & sSNDED & ResourceName(SoundEdit, InGame, True)
          Else
            Caption = sSNDED & ResourceName(SoundEdit, InGame, True)
          End If

          'change root node of notes list
          Me.tvwSound.Nodes(1).Text = ResourceName(SoundEdit, InGame, True)
          'if node 1 is selected
          If tvwSound.SelectedItem.Index = 1 Then
            'force redraw
            PaintPropertyWindow
          End If
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub ZoomScale(ByVal Dir As Long)

        'adjusts zoom factor for display
        'positive dir increases scale; negative decreases scale

        Dim i As Long
        Dim lngPrevScale As Long

        'save current scroll position
        lngPrevScale = StaffScale

        'increment/decrement scale
        StaffScale = StaffScale + Sgn(Dir)

        'if below minimum
        If StaffScale = 0 Then
          'reset and exit
          StaffScale = 1
          Exit Sub
        End If

        'if above maximum
        If StaffScale = 4 Then
          'reset and exit
          StaffScale = 3
          Exit Sub
        End If

        If MainStatusBar.Tag <> CStr(AGIResType.Sound) Then
          AdjustMenus AGIResType.Sound, InGame, True, IsDirty
        End If
        'update statusbar
        MainStatusBar.Panels("Scale").Text = "Scale: " & CStr(StaffScale)

        'adjust offset
        SOHorz = SOHorz / lngPrevScale * StaffScale

        For i = 0 To 3
          'set font size for track
          picStaff(i).FontSize = 5 * StaffScale
          'reset vertical scrollbars
          SetVScroll i, lngPrevScale
        Next i

        'resize horizonal scale
        SetHScroll
        'need to convert pixels into duration, since the
        'scrollbar uses units of duration
        hsbStaff.SmallChange = (picStaff(0).Width - KeyWidth) * SM_SCROLL / TICK_WIDTH / StaffScale
        hsbStaff.LargeChange = (picStaff(0).Width - KeyWidth) * LG_SCROLL / TICK_WIDTH / StaffScale

        SetKeyWidth

        'redraw staves
        DrawStaff -1

        'if a selection is active, use changeselection to draw it correctly
        If SelTrack <> -1 Then
          ChangeSelection SelTrack, SelStart, SelLength, True, False, True '
        End If
      End Sub

      Private Sub cmdPlay_Click()

        '*'Debug.Assert Not cmdStop.Enabled
        'play the sound
        MenuClickECustom1
      End Sub

      Private Sub cmdStop_Click()

        '*'Debug.Assert Not cmdPlay.Enabled
        'stop the sound
        MenuClickECustom1
      End Sub


      Private Sub Form_Deactivate()

        'release midi

        Dim rtn As Long

        On Error GoTo ErrHandler

        If Not Settings.NoMIDI Then
          'if playing
          If frmMDIMain.mnuECustom1.Caption = "Stop Sound" & vbTab & "Ctrl+Enter" Then
            'stop it
            SoundEdit.StopSound
            'reset menu caption
            frmMDIMain.mnuECustom1.Caption = "Play Sound" & vbTab & "Ctrl+Enter"
          End If

          'close midi
          KillMIDI
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub Form_KeyPress(KeyAscii As Integer)

        Dim intKey As Integer

        'don't override editboxes
        If ActiveControl.Name = "txtProperty" Or ActiveControl.Name = "lstProperty" Then
          Exit Sub
        End If

        '*'Debug.Assert frmMDIMain.ActiveMdiChild Is Me

        'local copy of key
        intKey = KeyAscii
        'clear buffer so key is not processed
        'further
        KeyAscii = 0


        Select Case intKey
        Case 45, 95 '"-", "_" vol down (attenuation up)
          picDuration_MouseDown 0, 0, 1, 1

        Case 43, 61 '"+", "=" vol up (attenuation down)
          picDuration_MouseDown 0, 0, 25, 1

        Case 32 'space toggle mute
          blnMute = Not blnMute
          ShowDefaultDuration

        Case 60, 44 '"<", "," octave down
          DefOctave = DefOctave - 1
          If DefOctave = 2 Then DefOctave = 3
          picKeyboard.Refresh

        Case 62, 46 '">", "." octave up
          DefOctave = DefOctave + 1
          If DefOctave = 11 Then DefOctave = 10
          picKeyboard.Refresh

        Case 91, 123 '"{", "[" length down
          If DefLength > 1 Then
            udDuration.Value = udDuration.Value - 1
          End If

        Case 93, 125 '"}", "]" length up
          If DefLength < 16 Then
            udDuration.Value = udDuration.Value + 1
          End If

        Case 97 To 103  'a - g /natural tones
          If SelTrack = 3 Then
            'don't do anything
            Exit Sub
          End If
          'convert to 0-7 scale
          lngMIDInote = intKey - 99
          'shift so c=0 and g=6
          If lngMIDInote < 0 Then lngMIDInote = lngMIDInote + 7
          'convert letter number to music scale
          lngMIDInote = lngMIDInote * 2
          If lngMIDInote > 4 Then lngMIDInote = lngMIDInote - 1
          'combine with octave to get midi note
          lngMIDInote = DefOctave * 12 + lngMIDInote
          'validate
          If lngMIDInote < 45 Or lngMIDInote > 127 Or lngMIDInote = 121 Or lngMIDInote = 124 Or lngMIDInote = 126 Then
            lngMIDInote = -1
            Exit Sub
          End If

          'play it
          NoteOn lngMIDInote, Not (SelTrack < 0 Or SelTrack > 3 Or SelStart = -1)

        Case 67, 68, 70, 71, 65 'C, D, F, G, A (sharps)
          If SelTrack = 3 Then
            'don't do anything
            Exit Sub
          End If
          'convert to 0-7 scale
          lngMIDInote = intKey - 67
          'shift so C=0 and G=6
          If lngMIDInote < 0 Then lngMIDInote = lngMIDInote + 7
          'convert letter number to music scale
          lngMIDInote = lngMIDInote * 2
          If lngMIDInote > 4 Then lngMIDInote = lngMIDInote - 1
          'combine with octave and sharpen
          lngMIDInote = DefOctave * 12 + lngMIDInote + 1

          'validate
          If lngMIDInote < 45 Or lngMIDInote > 127 Or lngMIDInote = 121 Or lngMIDInote = 124 Or lngMIDInote = 126 Then
            lngMIDInote = -1
            Exit Sub
          End If

          'play it
          NoteOn lngMIDInote, Not (SelTrack < 0 Or SelTrack > 3 Or SelStart = -1)
        Case 49 To 52 '1, 2, 3, 4
          'only used in noise track
          If SelTrack <> 3 Then
            Exit Sub
          End If

          'insert a noise track periodic tone note with appopriate frequency
          lngMIDInote = intKey - 49
          NoteOn intKey - 49, True

        Case 33, 64, 35, 36 '!,@,#,$
          'only used in noise track
          If SelTrack <> 3 Then
            Exit Sub
          End If
          'insert a noise track white noise note with appropriate frequency
          If intKey = 64 Then
            lngMIDInote = 5
          Else
            lngMIDInote = intKey - 29
          End If
          NoteOn lngMIDInote, True
        End Select

      End Sub

      Private Sub Form_KeyUp(KeyCode As Integer, Shift As Integer)

        'if a note is playing that was pressed on the keyboard,
        If lngMIDInote <> -1 Then
          'turn it off
          NoteOff lngMIDInote
        End If
        'reset note
        lngMIDInote = -1
      End Sub

      Private Sub hsbKeyboard_Change()

        'if not skipping draw
        If Not blnDontDraw Then
          If SelTrack = 3 Then
            'set offset
            NKbOffset = hsbKeyboard.Value
          Else
            'set offset
            MKbOffset = hsbKeyboard.Value
          End If
          'redraw keyboard
          picKeyboard_Paint
        End If
      End Sub
      Private Sub hsbKeyboard_GotFocus()

        'force focus back to keyboard window
        picKeyboard.SetFocus
      End Sub

      Private Sub hsbKeyboard_Scroll()

        hsbKeyboard_Change
      End Sub

      Private Sub hsbStaff_Change()

        'adjust offset
        SOHorz = -hsbStaff.Value * CLng(TICK_WIDTH) * StaffScale

        If Not blnDontDraw Then
          'use force-redraw to update display
          DrawStaff -1
          'reset cursor and selection flags
          CursorOn = False
          Timer1.Enabled = False
          SelActive = False

          'reselect as appropriate
          ChangeSelection SelTrack, SelStart, SelLength, True, False, False '
        End If
      End Sub

      Private Sub hsbStaff_GotFocus()

        'if there is a track
        If SelTrack >= 0 Then
          If picStaffVis(SelTrack) Then
            'set focus to staff
            picStaff(SelTrack).SetFocus
          Else
            'set focus to tree
            tvwSound.SetFocus
          End If
        Else
          'set focus to tree
          tvwSound.SetFocus
        End If
      End Sub


      Private Sub hsbStaff_Scroll()

        hsbStaff_Change
      End Sub

      Private Sub lstProperty_DblClick()

        'user has made a selection
        SelectPropFromList

        'set focus to picProperties
        picProperties.SetFocus
      End Sub
      Private Sub lstProperty_GotFocus()
        picProperties.Refresh
      End Sub

      Private Sub lstProperty_KeyPress(KeyAscii As Integer)

        'enter = accept
        'esc = cancel

        Select Case KeyAscii
        Case vbKeyEscape
          'hide the list, set focus to property box
          lstProperty.Visible = False
          picProperties.SetFocus

        Case vbKeyReturn
          'select the property, set focus to property box
          SelectPropFromList
          picProperties.SetFocus
        End Select
      End Sub


      Private Sub lstProperty_LostFocus()

        On Error GoTo ErrHandler

        'if visible, just hide it
        If lstProperty.Visible Then
          lstProperty.Visible = False
        End If

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub picDuration_DblClick()

        '*'Debug.Assert frmMDIMain.ActiveMdiChild Is Me
        'if double-clicking on volume icon
        If mX >= 12 And mX <= 22 And mY < 18 Then
          'toggle mute
          blnMute = Not blnMute
          ShowDefaultDuration

        Else
          'same as click
          picDuration_MouseDown 0, 0, mX, mY
        End If
      End Sub

      Private Sub picDuration_GotFocus()

        PaintPropertyWindow False
      End Sub


      Private Sub picDuration_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'check for change in volume control
        '*'Debug.Assert frmMDIMain.ActiveMdiChild Is Me

        'if above volume control edge
        If Y < 18 And Not blnMute Then
          Select Case X
          Case Is <= 11 'minus
            'make quieter (increase vol attenuation)
            DefAttn = DefAttn + 1
            If DefAttn > 14 Then
              DefAttn = 14
            Else
              ShowDefaultDuration
            End If

          Case Is >= 23 'plus
            'make louder (decrease vol attenuation)
            DefAttn = DefAttn - 1
            If DefAttn < 0 Then
              DefAttn = 0
            Else
              ShowDefaultDuration
            End If
          End Select
        End If

        'save x and Y
        mX = X
        mY = Y
      End Sub

      Private Sub ShowDefaultDuration()

        'draw default notelength

        Dim rtn As Long

        'disable redraw to reduce flicker
        SendMessage picDuration.hWnd, WM_SETREDRAW, 0, 0
        picDuration.Cls
        'add volume control icon
        rtn = BitBlt(picDuration.hDC, 11, 0, 13, 18, NotePictures.hDC, 766, 0, SRCCOPY)

        'add volume controls
        picDuration.ForeColor = vbBlack
        picDuration.CurrentX = 3
        picDuration.CurrentY = -1
        picDuration.Print "-";
        picDuration.CurrentX = 25
        picDuration.Print "+";

        'set color based on volume
        picDuration.FillColor = EGAColor(DefAttn)
        picDuration.ForeColor = picDuration.FillColor

        'if volume is not off AND not muting,
        If DefAttn < 15 And Not blnMute Then
         Select Case DefLength
         Case 1  'sixteenth note
           'draw sixteenth
           rtn = StretchBlt(picDuration.hDC, 4, 20, 24, 40, _
                            NotePictures.hDC, 0, 0, 73, 133, TRANSCOPY)

         Case 2  'eighth note
           'draw eighth
           rtn = StretchBlt(picDuration.hDC, 4, 20, 24, 40, _
                            NotePictures.hDC, 72, 0, 73, 133, TRANSCOPY)

         Case 3  'eighth note dotted
           'draw eighth
           rtn = StretchBlt(picDuration.hDC, 2, 20, 24, 40, _
                            NotePictures.hDC, 72, 0, 73, 133, TRANSCOPY)
           'draw dot
           picDuration.Circle (26, 53), 2

         Case 4  'quarter note
           'draw quarter
           rtn = StretchBlt(picDuration.hDC, 8, 20, 24, 40, _
                            NotePictures.hDC, 144, 0, 73, 133, TRANSCOPY)

         Case 5 'quater note tied to sixteenth note
           'draw quarter
           rtn = StretchBlt(picDuration.hDC, 2, 25, 12, 22, _
                            NotePictures.hDC, 144, 0, 73, 133, TRANSCOPY)
           'draw connector
           rtn = StretchBlt(picDuration.hDC, 3, 49, 22, 6, _
                            NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
           'draw sixteenth
           rtn = StretchBlt(picDuration.hDC, 20, 25, 12, 22, _
                            NotePictures.hDC, 0, 0, 73, 133, TRANSCOPY)

         Case 6  'quarter note dotted
           'draw quarter
           rtn = StretchBlt(picDuration.hDC, 4, 20, 24, 40, _
                            NotePictures.hDC, 144, 0, 73, 133, TRANSCOPY)
           'draw dot
           picDuration.Circle (26, 53), 2

          Case 7  'quarter note double dotted
           'draw quarter
           rtn = StretchBlt(picDuration.hDC, 3, 20, 24, 40, _
                            NotePictures.hDC, 144, 0, 73, 133, TRANSCOPY)
           'draw dot
           picDuration.Circle (23, 53), 2
           'draw dot
           picDuration.Circle (30, 53), 2

         Case 8  'half note
           'draw half note
           rtn = StretchBlt(picDuration.hDC, 8, 20, 24, 40, _
                            NotePictures.hDC, 216, 0, 73, 133, TRANSCOPY)

         Case 9  'half note tied to sixteenth
           rtn = StretchBlt(picDuration.hDC, 2, 25, 12, 22, _
                            NotePictures.hDC, 216, 0, 73, 133, TRANSCOPY)
           'draw connector
           rtn = StretchBlt(picDuration.hDC, 3, 49, 22, 6, _
                              NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
           'draw sixteenth
           rtn = StretchBlt(picDuration.hDC, 20, 25, 12, 22, _
                            NotePictures.hDC, 0, 0, 73, 133, TRANSCOPY)

         Case 10 'half note tied to eighth
           rtn = StretchBlt(picDuration.hDC, 2, 25, 12, 22, _
                            NotePictures.hDC, 216, 0, 73, 133, TRANSCOPY)
           'draw connector
           rtn = StretchBlt(picDuration.hDC, 3, 49, 22, 6, _
                            NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
           'increment position
           'draw eighth
           rtn = StretchBlt(picDuration.hDC, 20, 25, 12, 22, _
                            NotePictures.hDC, 72, 0, 73, 133, TRANSCOPY)

         Case 11 'half note tied to dotted eighth
           rtn = StretchBlt(picDuration.hDC, 2, 25, 12, 22, _
                            NotePictures.hDC, 216, 0, 73, 133, TRANSCOPY)
           'draw connector
           rtn = StretchBlt(picDuration.hDC, 3, 49, 22, 6, _
                              NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
           'draw eighth note
           rtn = StretchBlt(picDuration.hDC, 20, 25, 12, 22, _
                            NotePictures.hDC, 72, 0, 73, 133, TRANSCOPY)
           'draw dot
           picDuration.Circle (29, 43), 1

         Case 12 'half note dotted
           rtn = StretchBlt(picDuration.hDC, 4, 20, 24, 40, _
                            NotePictures.hDC, 216, 0, 73, 133, TRANSCOPY)
           'draw dot
           picDuration.Circle (26, 53), 2

         Case 13 'half note dotted tied to sixteenth
            rtn = StretchBlt(picDuration.hDC, 2, 25, 12, 22, _
                            NotePictures.hDC, 216, 0, 73, 133, TRANSCOPY)
           'draw dot
           picDuration.Circle (11, 53), 1
           'draw connector
           rtn = StretchBlt(picDuration.hDC, 3, 49, 22, 6, _
                              NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
           'draw sixteenth
           rtn = StretchBlt(picDuration.hDC, 20, 25, 12, 22, _
                            NotePictures.hDC, 0, 0, 73, 133, TRANSCOPY)

         Case 14 'half note double dotted
           rtn = StretchBlt(picDuration.hDC, 3, 20, 24, 40, _
                            NotePictures.hDC, 216, 0, 73, 133, TRANSCOPY)
           'draw dot
           picDuration.Circle (23, 53), 2
           'draw dot
           picDuration.Circle (30, 53), 2

         Case 15 'half note double dotted tied to sixteenth
           rtn = StretchBlt(picDuration.hDC, 2, 25, 12, 22, _
                            NotePictures.hDC, 216, 0, 73, 133, TRANSCOPY)
           'draw dot
           picDuration.Circle (11, 40), 1
           'draw dot
           picDuration.Circle (15, 40), 1
           'draw connector
           rtn = StretchBlt(picDuration.hDC, 3, 49, 22, 6, _
                              NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
           'draw sixteenth
           rtn = StretchBlt(picDuration.hDC, 20, 25, 12, 22, _
                            NotePictures.hDC, 0, 0, 72, 133, TRANSCOPY)

         Case 16 'whole note
            rtn = StretchBlt(picDuration.hDC, 6, 22, 24, 22, _
                            NotePictures.hDC, 288, 66, 73, 67, TRANSCOPY)
          End Select
        Else
          picDuration.FillColor = vbBlack
          picDuration.ForeColor = vbBlack
          'draw a rest
          Select Case DefLength
          Case 1  'sixteenth rest
            rtn = StretchBlt(picDuration.hDC, 13, 26, 12, 20, NotePictures.hDC, 0, 264, 72, 108, SRCAND)
          Case 2  'eighth rest
            rtn = StretchBlt(picDuration.hDC, 13, 26, 12, 20, NotePictures.hDC, 72, 264, 72, 108, SRCAND)
          Case 3  'eighth rest dotted
            rtn = StretchBlt(picDuration.hDC, 12, 26, 12, 20, NotePictures.hDC, 72, 264, 72, 108, SRCAND)
            'draw dot
            picDuration.Circle (21, 36), 1
          Case 4  'quarter rest
            rtn = StretchBlt(picDuration.hDC, 12, 26, 12, 20, NotePictures.hDC, 144, 264, 72, 108, SRCAND)
          Case 5 'quater rest and sixteenth rest
            rtn = StretchBlt(picDuration.hDC, 8, 26, 12, 20, NotePictures.hDC, 144, 264, 72, 108, SRCAND)
            rtn = StretchBlt(picDuration.hDC, 20, 26, 12, 20, NotePictures.hDC, 0, 264, 72, 108, SRCAND)
          Case 6  'quarter rest dotted
            rtn = StretchBlt(picDuration.hDC, 12, 26, 12, 20, NotePictures.hDC, 144, 264, 72, 108, SRCAND)
            'draw dot
            picDuration.Circle (21, 36), 1
          Case 7  'quarter rest double dotted
            rtn = StretchBlt(picDuration.hDC, 10, 26, 12, 20, NotePictures.hDC, 144, 264, 72, 108, SRCAND)
            'draw dot
            picDuration.Circle (19, 36), 1
            'draw dot
            picDuration.Circle (24, 36), 1
          Case 8  'half rest
            picDuration.Line (13, 35)-Step(8, 3), vbBlack, BF
            picDuration.Line (11, 38)-Step(13, 0), vbBlack
          Case 9  'half rest and sixteenth
            picDuration.Line (6, 35)-Step(8, 3), vbBlack, BF
            picDuration.Line (4, 38)-Step(13, 0), vbBlack
            rtn = StretchBlt(picDuration.hDC, 21, 26, 12, 20, NotePictures.hDC, 0, 264, 72, 108, SRCAND)
          Case 10 'half rest and eighth
            picDuration.Line (6, 35)-Step(8, 3), vbBlack, BF
            picDuration.Line (4, 38)-Step(13, 0), vbBlack
            rtn = StretchBlt(picDuration.hDC, 21, 26, 12, 20, NotePictures.hDC, 72, 264, 72, 108, SRCAND)
          Case 11 'half rest, eighth dotted
            picDuration.Line (5, 35)-Step(8, 3), vbBlack, BF
            picDuration.Line (3, 38)-Step(13, 0), vbBlack
            rtn = StretchBlt(picDuration.hDC, 20, 26, 12, 20, NotePictures.hDC, 72, 264, 72, 108, SRCAND)
            'draw dot
            picDuration.Circle (29, 36), 1
          Case 12 'half rest dotted
            picDuration.Line (10, 35)-Step(8, 3), vbBlack, BF
            picDuration.Line (8, 38)-Step(13, 0), vbBlack
            'draw dot
            picDuration.Circle (23, 36), 1
          Case 13 'half rest dotted and sixteenth
            picDuration.Line (5, 35)-Step(8, 3), vbBlack, BF
            picDuration.Line (3, 38)-Step(13, 0), vbBlack
            'draw dot
            picDuration.Circle (18, 36), 1
            rtn = StretchBlt(picDuration.hDC, 25, 26, 12, 20, NotePictures.hDC, 0, 264, 72, 108, SRCAND)
          Case 14 'half rest double dotted
            picDuration.Line (8, 35)-Step(8, 3), vbBlack, BF
            picDuration.Line (6, 38)-Step(13, 0), vbBlack
            'draw dot
            picDuration.Circle (21, 36), 1
            'draw dot
            picDuration.Circle (26, 36), 1
          Case 15 'half rest double dotted and sixteenth
            picDuration.Line (3, 35)-Step(8, 3), vbBlack, BF
            picDuration.Line (1, 38)-Step(13, 0), vbBlack
            'draw dot
            picDuration.Circle (16, 36), 1
            'draw dot
            picDuration.Circle (21, 36), 1
            rtn = StretchBlt(picDuration.hDC, 28, 26, 12, 20, NotePictures.hDC, 0, 264, 72, 108, SRCAND)
          Case 16 'whole rest
            picDuration.Line (12, 32)-Step(9, 5), vbBlack, BF
            picDuration.Line (9, 32)-Step(16, 0), vbBlack
          End Select
        End If

        'reenable updating
        SendMessage picDuration.hWnd, WM_SETREDRAW, 1, 0
        picDuration.Refresh
      End Sub

      Public Sub MouseWheel(ByVal MouseKeys As Long, ByVal Rotation As Long, ByVal xPos As Long, ByVal yPos As Long)

        ' mouse wheel changes default note length/volume if over the default note picture
        ' it scrolls staves left/right if over one of the staves

        Dim lngDur As Long, lngLen As Long
        Dim lngNewDur As Long, intAttn As Integer
        Dim lngTarget As Long, i As Long

        'determine
        'MouseKeys values:
        '    0 = no keys pressed
        '    8 = Ctrl
        '    4 = Shift
        '    NO VALUE for Alt?

        On Error GoTo ErrHandler

        ' this should never happen, but just in case
        If Not frmMDIMain.ActiveMdiChild Is Me Then
          Exit Sub
        End If

        'determine which control the cursor is currently over
        With picDuration
          If xPos > .Left And xPos < .Left + .Width And yPos > .Top And yPos < .Top + .Height Then
            lngTarget = 1
          End If
        End With

        'if over a visible staff:
        For i = 0 To 3
          With picStaff(i)
            If .Visible Then
              If xPos > .Left And xPos < .Left + .Width And yPos > .Top And yPos < .Top + .Height Then
                'found it
                lngTarget = 2
                Exit For
              End If
            End If
          End With
        Next i

        'check the scrollbar
        With hsbStaff
          If .Visible Then
            If xPos > .Left And xPos < .Left + .Width And yPos > .Top And yPos < .Top + .Height Then
              'same as being over a staff
              lngTarget = 2
            End If
          End If
        End With

        'check the keyboard
        With picKeyboard
          If .Visible Then
            If xPos > .Left And xPos < .Left + .Width And yPos > .Top And yPos < .Top + .Height Then
              lngTarget = 3
            End If
          End If
        End With

        'if not over a target
        If lngTarget = 0 Then
          'do nothing
          Exit Sub
        End If

        ' scroll based on target value
        Select Case lngTarget
        Case 1 'default note length
          'NOTE: in this context, note DURATION is the AGI duration value
          'and note LENGTH is the corresponding MIDI note length (accounting
          'for TPQN value), which may be a rounded value

          Select Case MouseKeys
          Case 0 'no shift, ctrl, or alt
            'adjust note length

            'is a single note on one (music) track selected?
            If SelTrack >= 0 And SelTrack <= 2 And SelStart >= 0 And SelLength = 1 Then
              '*'Debug.Assert SelTrack = tvwSound.SelectedItem.Parent.Index - 2
              'get current note duration
              lngDur = SoundEdit.Track(SelTrack).Notes(tvwSound.SelectedItem.Tag).Duration
              'is it a 'regular' note value? i.e. does it exactly match a standard note duration?

              'convert to note length, which will include rounding, if needed
              lngLen = lngDur * 4 / SoundEdit.TPQN
              'now convert back, to compare against original (if not an exact match)
              lngNewDur = lngLen * SoundEdit.TPQN / 4
              'if it matches original duration, then we have a 'regular' note
              If lngDur = lngNewDur Then
                'adjust the length based on wheel, using call to ShiftDuration
                If Sgn(Rotation) = 1 Then
                  If lngLen > 1 Then
                    lngNewDur = SoundEdit.TPQN * (lngLen - 1) / 4
                    ShiftDuration lngNewDur
                  End If

                ElseIf Sgn(Rotation) = -1 Then
                  lngNewDur = SoundEdit.TPQN * (lngLen + 1) / 4
                  ShiftDuration lngNewDur
                End If
              End If

            Else
              'adjust default length
              If Sgn(Rotation) = 1 Then
                If DefLength > 1 Then
                  DefLength = DefLength - 1
                  ShowDefaultDuration
                End If
              ElseIf Sgn(Rotation) = -1 Then
                If DefLength < 16 Then
                  DefLength = DefLength + 1
                  ShowDefaultDuration
                End If
              End If
            End If

          Case 8
            'adjust note volume
            'adjusting attenuation is not limited to a single note

            'is at least one note on one (music) track selected?
            If SelTrack >= 0 And SelTrack <= 2 And SelStart >= 0 And SelLength > 0 Then
              '*'Debug.Assert SelTrack = tvwSound.SelectedItem.Parent.Index - 2
              'get current note attenuation
              intAttn = SoundEdit.Track(SelTrack).Notes(tvwSound.SelectedItem.Tag).Attenuation
              'adjust the length based on wheel, using call to ShiftDuration

              If Sgn(Rotation) = 1 Then
                If intAttn > 0 Then
                  ShiftVol 1 'Sgn(Rotation)
                End If

              ElseIf Sgn(Rotation) = -1 Then
                If intAttn < 14 Then
                  ShiftVol -1 'Sgn(Rotation)
                End If
              End If

            Else
              'adjust default volume
              If Sgn(Rotation) = 1 Then
                If DefAttn > 1 Then
                  DefAttn = DefAttn - 1
                  ShowDefaultDuration
                End If
              ElseIf Sgn(Rotation) = -1 Then
                'it's 14 because attenuation of 15 equals mute, which
                'is represented on the staves differently
                If DefAttn < 14 Then
                  DefAttn = DefAttn + 1
                  ShowDefaultDuration
                End If
              End If
            End If

          End Select

        Case 2 ' staff
          'ignore unless no keys pressed
          If MouseKeys = 0 Then

            If Sgn(Rotation) = 1 Then
              With hsbStaff
                If .Value > .Min Then
                  'if there is room for a small change
                  If .Value > .Min + .SmallChange Then
                    .Value = .Value - .SmallChange
                  Else
                    hsbStaff.Value = .Min
                  End If
                End If
              End With

            ElseIf Sgn(Rotation) = -1 Then
              With hsbStaff
                If .Value < .Max Then
                  'if there is room for a small change
                  If .Value < .Max - .SmallChange Then
                    .Value = .Value + hsbStaff.SmallChange
                  Else
                    .Value = .Max
                  End If
                End If
              End With
            End If

          End If

        Case 3 'keyboard

          'ignore unless no keys pressed
          If MouseKeys = 0 Then

            If Sgn(Rotation) = 1 Then
              With hsbKeyboard
                If .Value > .Min Then
                  'if there is room for a small change
                  If .Value > .Min + .SmallChange Then
                    .Value = .Value - .SmallChange
                  Else
                    .Value = .Min
                  End If
                End If
              End With

            ElseIf Sgn(Rotation) = -1 Then
              With hsbKeyboard
                If .Value < .Max Then
                  'if there is room for a small change
                  If .Value < .Max - .SmallChange Then
                    .Value = .Value + .SmallChange
                  Else
                    .Value = .Max
                  End If
                End If
              End With
            End If

          End If
        End Select

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub picKeyboard_DblClick()
        picKeyboard_MouseDown mButton, mShift, mX, mY
      End Sub

      Private Sub picKeyboard_GotFocus()

        PaintPropertyWindow False
      End Sub


      Private Sub picKeyboard_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'play this note
        Dim NoteNum As Long, Octave As Long
        Dim blnOnBlack As Boolean

        'if no track selected, don't do anything
        If SelTrack = -1 Then
          Exit Sub

        ElseIf SelTrack <> 3 Then
          'note number is 1/2 key width
          'adjusted for offset, and one half key width
          'so keys are read correctly
          NoteNum = (X + 6) \ 12 + (MKbOffset - 40) * 2
          'new octave every 14 keys (adjusted so it changes at each C note)
          Octave = NoteNum \ 14 + 3
          'convert note to relative number
          NoteNum = (NoteNum) Mod 14

          'this is a black key if note is even and NOT (0 or 6) AND ABOVE black key edge
          blnOnBlack = (NoteNum Mod 2 = 0) And Not (NoteNum = 0 Or NoteNum = 6) And (Y <= 32)

          'if on a black key
          If blnOnBlack Then
            If NoteNum <= 4 Then
              NoteNum = NoteNum - 1
            Else
              NoteNum = NoteNum - 2
            End If
          Else
            'recalculate which key was pressed
            NoteNum = X \ 24 + MKbOffset - 40
            Octave = NoteNum \ 7 + 3
            NoteNum = NoteNum Mod 7

            'adjust for black keys
            NoteNum = NoteNum * 2
            If NoteNum > 4 Then
              NoteNum = NoteNum - 1
            End If
          End If

          'calculate note to play
          lngMIDInote = (Octave * 12) + NoteNum

          'send noteon midi cmd
          NoteOn lngMIDInote, Not (SelTrack < 0 Or SelStart = -1 Or Button = vbRightButton)
        Else
          'if not on keys (based on vertical position)
          If Y < 33 Then
            Exit Sub
          End If

          lngMIDInote = (X + NKbOffset) \ IIf(picKeyboard.ScaleWidth / 8 < 30, 30, picKeyboard.ScaleWidth / 8)
          'play this note
          NoteOn lngMIDInote, Not (SelStart = -1 Or Button = vbRightButton)
        End If
        If tmrScroll.Enabled Then
          'should never get here
          tmrScroll.Enabled = False
        End If

        'save mouse state for dbl click
        mButton = Button
        mShift = Shift
        mX = X
        mY = Y
      End Sub

      Private Sub picKeyboard_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'send the currently playing midi note to NoteOff method
        NoteOff lngMIDInote
      End Sub


      Private Sub picKeyboard_Paint()

        'paint the keyboard surface

        Dim i As Long, j As Long
        Dim lngKeyW As Long

        picKeyboard.Cls

        If SelTrack <> 3 Then
          'draw black keys
          For i = 0 To picKeyboard.Width \ 24
            'only draw key if it belongs
            If ((MKbOffset + i - 3) Mod 7) <> 2 And ((MKbOffset + i - 3) Mod 7) <> 5 Then
              picKeyboard.Line (i * 24 - 6, 0)-(i * 24 + 6, 32), vbBlack, BF
            End If
            'don't draw above limit of keys
            If MKbOffset + i - 3 >= 127 Then
              Exit For
            End If
          Next i

          For i = 0 To picKeyboard.Width \ 24
            'draw main key outline
            picKeyboard.Line (i * 24, 0)-((i + 1) * 24, 63), vbBlack, B
            'add key labels
            picKeyboard.CurrentX = i * 24 + 9
            picKeyboard.CurrentY = 48
            picKeyboard.FontBold = ((MKbOffset + i - 5) \ 7 - 2 = DefOctave)
            picKeyboard.Print ChrW$(((MKbOffset + i - 3) Mod 7) + 65);
            'if note is 'c'
            If (MKbOffset + i - 3) Mod 7 = 2 Then
              picKeyboard.CurrentX = i * 24 + 6
              picKeyboard.CurrentY = 34
              picKeyboard.Print (MKbOffset + i - 3) \ 7 - 2;
            End If
            'don't draw above limit of keys
            If MKbOffset + i - 3 >= 127 Then
              Exit For
            End If
          Next i
        Else
          'draw noise keyboard
          lngKeyW = picKeyboard.ScaleWidth / 8
          If lngKeyW < 30 Then
            'split it 8 ways
            lngKeyW = 30
          End If
          'draw white noise header with diagonal lines
          picKeyboard.FillColor = vbBlack
          picKeyboard.FillStyle = vbUpwardDiagonal
          picKeyboard.Line (lngKeyW * 4 - NKbOffset, 0)-(picKeyboard.ScaleWidth, 32), , B
          'draw periodic tone header with white space
          picKeyboard.FillStyle = vbFSTransparent
          picKeyboard.Line (0, 0)-(lngKeyW * 4 - NKbOffset, 32), vbBlack, B
          'draw the vertical lines separating keys
          For i = 0 To 7
            picKeyboard.Line (i * lngKeyW - NKbOffset, 32)-(i * lngKeyW - NKbOffset, 64), vbBlack
          Next i
          'draw white space over noise header to hold text
          picKeyboard.Line (lngKeyW * 6 - 38 - NKbOffset, 10)-Step(72, 13), vbWhite, BF
          'draw border at right edge
          picKeyboard.Line (picKeyboard.ScaleWidth - 1, 0)-Step(0, 64), vbBlack
          'draw top and bottom borders
          picKeyboard.Line (0, 0)-(picKeyboard.ScaleWidth, 0), vbBlack
          picKeyboard.Line (0, 64 - 1)-(picKeyboard.ScaleWidth, 64 - 1), vbBlack
          'set x and Y to print header text
          picKeyboard.CurrentX = lngKeyW * 2 - 42 - NKbOffset
          picKeyboard.CurrentY = 10
          picKeyboard.Print "PERIODIC TONE"
          picKeyboard.CurrentX = lngKeyW * 6 - 36 - NKbOffset
          picKeyboard.CurrentY = 10
          picKeyboard.Print "WHITE NOISE"
          'set x and Y to print key text
          picKeyboard.CurrentY = 49
          picKeyboard.CurrentX = lngKeyW / 2 - 12 - NKbOffset
          picKeyboard.Print "2330";
          picKeyboard.CurrentX = 3 * lngKeyW / 2 - 12 - NKbOffset
          picKeyboard.Print "1165";
          picKeyboard.CurrentX = 5 * lngKeyW / 2 - 9 - NKbOffset
          picKeyboard.Print "583";
          picKeyboard.CurrentX = 7 * lngKeyW / 2 - 14 - NKbOffset
          picKeyboard.Print "TRK2";
          picKeyboard.CurrentX = 9 * lngKeyW / 2 - 12 - NKbOffset
          picKeyboard.Print "2330";
          picKeyboard.CurrentX = 11 * lngKeyW / 2 - 12 - NKbOffset
          picKeyboard.Print "1165";
          picKeyboard.CurrentX = 13 * lngKeyW / 2 - 9 - NKbOffset
          picKeyboard.Print "583";
          picKeyboard.CurrentX = 15 * lngKeyW / 2 - 14 - NKbOffset
          picKeyboard.Print "TRK2";
        End If
      End Sub

      Private Sub picKeyboard_Resize()

        'force redraw
        picKeyboard_Paint
      End Sub

      Private Sub picMute_Click(Index As Integer)

        'toggle mute status for this track
        SoundEdit.Track(Index).Muted = Not SoundEdit.Track(Index).Muted

        'redraw
        picMute_Paint Index

        'if the track is selected, need to also update property window
        If tvwSound.SelectedItem.Index > 1 And tvwSound.SelectedItem.Index < 6 Then
          PaintPropertyWindow
        End If
      End Sub

      Private Sub picMute_Paint(Index As Integer)

        'draw correct picture

        If Not SoundEdit Is Nothing Then
          If SoundEdit.Track(Index).Muted Then
            picMute(Index).PaintPicture LoadResPicture("MUTEON", vbResBitmap), 0, 0
          Else
            picMute(Index).PaintPicture LoadResPicture("MUTEOFF", vbResBitmap), 0, 0
          End If
        End If
      End Sub
      Private Sub picProperties_DblClick()

        'set dblclick mode (to allow properties to be toggled)
        PropDblClick = True

        'call mouse down again
        picProperties_MouseDown 0, 0, mX, mY
      End Sub

      Private Sub picProperties_KeyDown(KeyCode As Integer, Shift As Integer)

        If Shift = 0 Then
          Select Case KeyCode
          Case vbKeyUp
            If SelectedProp <> 1 Then
              SelectedProp = SelectedProp - 1
              PaintPropertyWindow
            End If

          Case vbKeyDown
            If SelectedProp <> PropRowCount Then
              SelectedProp = SelectedProp + 1
              PaintPropertyWindow
            End If

          Case vbKeyReturn

          End Select
        End If
      End Sub

      Private Sub picProperties_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
        'set selprop variable
        Dim tmpProp As Long, i As Long
        Dim rtn As Long, blnDblClick As Boolean
        Dim NextUndo As SoundUndo
        Dim lngNoteIndex As Long

        'local copy of dblclick mode
        blnDblClick = PropDblClick
        'clear global dblclick mode
        PropDblClick = False

        'save position in case of double click
        mX = X
        mY = Y

        'if current node is 'end'
        If tvwSound.SelectedItem.Text = "End" Then
          Exit Sub
        End If

        'calculate property number
        tmpProp = Y \ PropRowHeight

        'verify not out of bounds
        If tmpProp > PropRowCount Then
          tmpProp = PropRowCount
        End If
        If tmpProp < 0 Then
          tmpProp = 0
        End If

        'if not past limit for selected item
        Select Case tvwSound.SelectedItem.Index
        Case 1  'root - 5 props
          If tmpProp > 5 Then
            Exit Sub
          End If

        Case 2 To 4 'sound tracks - 4 props
          If tmpProp > 4 Then
            Exit Sub
          End If

        Case 5  'noise track - 3 props
          If tmpProp > 3 Then
            Exit Sub
          End If

        Case Else ' a note
          If tvwSound.SelectedItem.Parent.Index = 5 Then
            'noise note:4 props
            If tmpProp > 4 Then
              Exit Sub
            End If
          Else
            'sound note: 6 props
            If tmpProp > 6 Then
              Exit Sub
            End If
          End If
        End Select

        'if selected property was clicked
        If tmpProp = SelectedProp Then
          'edit as appropriate
          Select Case tvwSound.SelectedItem.Index
          Case 1  'root
            Select Case SelectedProp
            Case 1, 2 ' "ID", "Description
              'id only enabled if in a game
              If SelectedProp = 1 And Not InGame Then
                Exit Sub
              End If

             'if button clicked or doubleclicked,
              If X > picProperties.Width - 17 Or blnDblClick Then
                'copy pressed dropdlg picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, SelectedProp * PropRowHeight, 17, 17, DropDlgDC, 18, 0, SRCCOPY)
                'call edit id/desc
                MenuClickDescription SelectedProp
                'reset dropdialog button
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, SelectedProp * PropRowHeight, 17, 17, DropDlgDC, 0, 0, SRCCOPY)
              End If

            Case 3 ' "Key"
              'if dblclicking AND on actual property
              If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                If Settings.SndUndo <> 0 Then
                  'undo
                  Set NextUndo = New SoundUndo
                  NextUndo.UDAction = udsChangeKey
                  NextUndo.UDStart = SoundEdit.Key
                  AddUndo NextUndo
                End If

                'increment key
                If SoundEdit.Key = 7 Then
                  SoundEdit.Key = -7
                Else
                  SoundEdit.Key = SoundEdit.Key + 1
                End If
                DrawStaff -1
              Else
                'if clicking on dropdown button
                If X > picProperties.Width - 17 Then
                  'copy pressed dropdown picture
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, 3 * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                  'display list box
                  DisplayPropertyListBox PropSplitLoc, 3 * PropRowHeight, picProperties.Width, ListItemHeight * 4 + 2, "KEY"
                End If
              End If

            Case 4 ' "TPQN"
              'display textbox
              DisplayPropertyEditBox PropSplitLoc + 3, 4 * PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "TPQN", 0
            Case 5 ' "Length"
              'read only
            End Select

          Case 2 To 4 'sound tracks
            Select Case SelectedProp
            Case 1 ' "Instrument"
              'if dblclicking AND on actual property
              If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                'add undo
                If Settings.SndUndo <> 0 Then
                  Set NextUndo = New SoundUndo
                  NextUndo.UDAction = udsChangeInstrument
                  NextUndo.UDTrack = tvwSound.SelectedItem.Index - 2
                  NextUndo.UDStart = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument
                  AddUndo NextUndo
                End If

                If SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument = 127 Then
                  SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument = 0
                Else
                  SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument + 1
                End If
              Else
                'if clicking on dropdown button
                If X > picProperties.Width - 17 Then
                  'copy pressed dropdown picture
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                  'display list box
                  DisplayPropertyListBox PropSplitLoc, PropRowHeight, picProperties.Width, ListItemHeight * 4 + 2, "INST"
                End If
              End If
            Case 2 ' "Muted"
              'if dblclicking AND on actual property
              If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Muted = Not SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Muted
                picMute_Paint tvwSound.SelectedItem.Index - 2
              Else
                'if clicking on dropdown button
                If X > picProperties.Width - 17 Then
                  'copy pressed dropdown picture
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, 2 * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                  'display list box
                  DisplayPropertyListBox PropSplitLoc, 2 * PropRowHeight, picProperties.Width - PropSplitLoc, ListItemHeight * 2 + 2, "MUTE"
                End If
              End If
            Case 3 ' "Note Count"
              'read only
            Case 4  'visible
              'can't adjust visible property in onetrack mode
              If Not OneTrack Then
                'if dblclicking AND on actual property
                If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                  SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible = Not SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible
                  picStaff(tvwSound.SelectedItem.Index - 2).Visible = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible
                  picStaffVis(tvwSound.SelectedItem.Index - 2) = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible

                  'use form resize to redraw
                  Form_Resize
                Else
                  'if clicking on dropdown button
                  If X > picProperties.Width - 17 Then
                    'copy pressed dropdown picture
                    rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, 2 * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                    'display list box
                    DisplayPropertyListBox PropSplitLoc, 4 * PropRowHeight, picProperties.Width - PropSplitLoc, ListItemHeight * 2 + 2, "VIS"
                  End If
                End If
              End If
            End Select

          Case 5  'noise track
            'muted and Count
            Select Case SelectedProp
            Case 1 ' "Muted"
              'if dblclicking AND on actual property
              If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                SoundEdit.Track(3).Muted = Not SoundEdit.Track(3).Muted
                picMute_Paint tvwSound.SelectedItem.Index - 2
              Else
                'if clicking on dropdown button
                If X > picProperties.Width - 17 Then
                  'copy pressed dropdown picture
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                  'display list box
                  DisplayPropertyListBox PropSplitLoc, PropRowHeight, picProperties.Width, ListItemHeight * 2 + 2, "MUTE"
                End If
              End If
            Case 2 ' "Note Count"
              'read only
            Case 3  'visible
              'can't adjust visible property in onetrack mode
              If Not OneTrack Then
                'if dblclicking AND on actual property
                If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                  SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible = Not SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible
                  picStaff(tvwSound.SelectedItem.Index - 2).Visible = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible
                  picStaffVis(tvwSound.SelectedItem.Index - 2) = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible
                  'use form resize to redraw
                  Form_Resize
                Else
                  'if clicking on dropdown button
                  If X > picProperties.Width - 17 Then
                    'copy pressed dropdown picture
                    rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, 2 * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                    'display list box
                    DisplayPropertyListBox PropSplitLoc, 4 * PropRowHeight, picProperties.Width - PropSplitLoc, ListItemHeight * 2 + 2, "VIS"
                  End If
                End If
              End If
            End Select

          Case Else ' a note
            'local copy of note number
            lngNoteIndex = CLng(tvwSound.SelectedItem.Tag)
            'if this is a noise note,
            If tvwSound.SelectedItem.Parent.Index = 5 Then
              'noise note: Type, Freq, duration, attenuation
              Select Case SelectedProp
              Case 1 ' "Type"
                'if dblclicking AND on actual property
                If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                  'add undo
                  If Settings.SndUndo <> 0 Then
                    Set NextUndo = New SoundUndo
                    NextUndo.UDAction = udsEditNote
                    NextUndo.UDTrack = 3
                    NextUndo.UDStart = CLng(lngNoteIndex)
                    NextUndo.UDText = SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor
                    NextUndo.UDLength = 1
                    AddUndo NextUndo
                  End If

                  'use rtn to adjust Type only
                  rtn = IIf((SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor And 4) = 4, 0, 4)
                  SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor = rtn + (SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor And 3)
                  DrawStaff tvwSound.SelectedItem.Parent.Index - 2

                Else
                  'if clicking on dropdown button
                  If X > picProperties.Width - 17 Then
                    'copy pressed dropdown picture
                    rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                    'display list box
                    DisplayPropertyListBox PropSplitLoc, PropRowHeight, picProperties.Width, ListItemHeight * 2 + 2, "TYPE"
                  End If
                End If
              Case 2 ' "Freq Div"
                'if dblclicking AND on actual property
                If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                  'add undo
                  If Settings.SndUndo <> 0 Then
                    Set NextUndo = New SoundUndo
                    NextUndo.UDAction = udsEditNote
                    NextUndo.UDTrack = 3
                    NextUndo.UDStart = CLng(lngNoteIndex)
                    NextUndo.UDText = SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor
                    NextUndo.UDLength = 1
                    AddUndo NextUndo
                  End If

                  'use rtn to adjust freq only
                  rtn = SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor And 3
                  rtn = rtn + 1
                  If rtn = 4 Then
                    rtn = 0
                  End If
                  SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor = rtn + (SoundEdit.Track(3).Notes(lngNoteIndex).FreqDivisor And 4)
                  DrawStaff tvwSound.SelectedItem.Parent.Index - 2
                Else
                  'if clicking on dropdown button
                  If X > picProperties.Width - 17 Then
                    'copy pressed dropdown picture
                    rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, 2 * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                    'display list box
                    DisplayPropertyListBox PropSplitLoc, 2 * PropRowHeight, picProperties.Width, ListItemHeight * 4 + 2, "NOISEFREQ"
                  End If
                End If
              Case 3 ' "Duration"
                'display textbox
                DisplayPropertyEditBox PropSplitLoc + 3, 3 * PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "DUR", 0
              Case 4 ' "Attenuation"
                 'display textbox
                DisplayPropertyEditBox PropSplitLoc + 3, 4 * PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "ATT", 0
             End Select
            Else
              'sound note: Freq Div, Duration, Attenuation, MIDI Note, MIDI Length, MIDI Octave
              Select Case SelectedProp
              Case 1 ' "Freq Div"
                 'display textbox
                DisplayPropertyEditBox PropSplitLoc + 3, PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "FREQ", 0
              Case 2 ' "Duration"
                 'display textbox
                DisplayPropertyEditBox PropSplitLoc + 3, 2 * PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "DUR", 0
              Case 3 ' "Attenuation"
                 'display textbox
                DisplayPropertyEditBox PropSplitLoc + 3, 3 * PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "ATT", 0
              Case 4 ' "MIDI Note"
                'if dblclicking AND on actual property
                If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                  'get current note
                  rtn = MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor)
                  If rtn < 127 Then
                    rtn = rtn + 1
                    If rtn = 121 Or rtn = 124 Or rtn = 126 Then
                      rtn = rtn + 1
                    End If
                  End If

                  If Settings.SndUndo <> 0 Then
                    'set undo properties
                    Set NextUndo = New SoundUndo
                    NextUndo.UDAction = udsEditNote
                    NextUndo.UDTrack = tvwSound.SelectedItem.Parent.Index - 2
                    NextUndo.UDStart = CLng(lngNoteIndex)
                    NextUndo.UDText = SoundEdit.Track(NextUndo.UDTrack).Notes(lngNoteIndex).FreqDivisor
                    NextUndo.UDLength = 1
                    AddUndo NextUndo
                  End If

                  'calculate new freqdivisor
                  SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor = NoteToFreq(rtn)
                  DrawStaff tvwSound.SelectedItem.Parent.Index - 2
                Else
                  'if clicking on dropdown button
                  If X > picProperties.Width - 17 Then
                    'copy pressed dropdown picture
                    rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, 4 * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                    'display list box
                    DisplayPropertyListBox PropSplitLoc, 4 * PropRowHeight, picProperties.Width - PropSplitLoc / 2, ListItemHeight * 5 + 2, "MNOTE"
                  End If
                End If
              Case 5 ' "MIDI Octave"
                'if dblclicking AND on actual property
                If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                  'get current note, and add one octave
                  rtn = MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor) + 12
                  If rtn > 127 Then
                    rtn = rtn - 84
                    If rtn < 45 Then rtn = rtn + 12
                  End If
                  If Settings.SndUndo <> 0 Then
                    'set undo properties
                    Set NextUndo = New SoundUndo
                    NextUndo.UDAction = udsEditNote
                    NextUndo.UDTrack = tvwSound.SelectedItem.Parent.Index - 2
                    NextUndo.UDStart = CLng(lngNoteIndex)
                    NextUndo.UDText = SoundEdit.Track(NextUndo.UDTrack).Notes(lngNoteIndex).FreqDivisor
                    NextUndo.UDLength = 1
                    AddUndo NextUndo
                  End If

                  'calculate new freqdivisor
                  SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).FreqDivisor = NoteToFreq(rtn)
                  DrawStaff tvwSound.SelectedItem.Parent.Index - 2
                Else
                  'if clicking on dropdown button
                  If X > picProperties.Width - 17 Then
                    'copy pressed dropdown picture
                    rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, 5 * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                    'display list box
                    DisplayPropertyListBox PropSplitLoc, 5 * PropRowHeight, picProperties.Width - PropSplitLoc / 2, ListItemHeight * 5 + 2, "MOCT"
                  End If
                End If
              Case 6 ' "MIDI Length"
                'if dblclicking AND on actual property
                If blnDblClick And X > PropSplitLoc And X < picProperties.Width - 17 Then
                  rtn = CLng(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).Duration / SoundEdit.TPQN * 4)
                  rtn = rtn + 1
                  If rtn > 16 Then
                    rtn = 1
                  End If
                  If Settings.SndUndo <> 0 Then
                    'set undo properties
                    Set NextUndo = New SoundUndo
                    NextUndo.UDAction = udsEditNote
                    NextUndo.UDTrack = tvwSound.SelectedItem.Parent.Index - 2
                    NextUndo.UDStart = CLng(lngNoteIndex)
                    NextUndo.UDText = SoundEdit.Track(NextUndo.UDTrack).Notes(lngNoteIndex).Duration
                    NextUndo.UDLength = 2
                    AddUndo NextUndo
                  End If
                  'set new duration
                  SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(lngNoteIndex).Duration = rtn * SoundEdit.TPQN / 4
                  DrawStaff tvwSound.SelectedItem.Parent.Index - 2
                Else
                  'if clicking on dropdown button
                  If X > picProperties.Width - 17 Then
                    'copy pressed dropdown picture
                    rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, 6 * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                    'display list box
                    DisplayPropertyListBox PropSplitLoc, 6 * PropRowHeight, picProperties.Width - PropSplitLoc / 2, ListItemHeight * 5 + 2, "MLEN"
                  End If
                End If
              End Select
            End If
          End Select

        'if not same as current prop
        Else
          'if changed
          If tmpProp <> SelectedProp And tmpProp <> 0 Then
            SelectedProp = tmpProp
            PaintPropertyWindow
          End If
        End If
      Exit Sub

      ErrHandler:
        Resume Next
      End Sub

      Private Sub DisplayPropertyEditBox(ByVal posX As Long, ByVal posY As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal strProp As String, ByVal lngBorderStyle As Long)
        'moves the edit box to appropriate position
        'preloads it with appropriate prop Value

        With txtProperty
          .Move picProperties.Left + posX, picProperties.Top + posY
          .Width = nWidth
          .Height = nHeight

          Select Case strProp
          Case "TPQN"
            .Text = SoundEdit.TPQN
          Case "DUR"
            .Text = SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).Duration
          Case "ATT"
            .Text = SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).Attenuation
          Case "FREQ"
            .Text = SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor
          End Select

          'set border
          .BorderStyle = lngBorderStyle

          'pass property to textbox tag
          .Tag = strProp

          'show text
          .ZOrder
          .Visible = True
          'select all
          txtProperty.SelStart = 0
          .SelLength = Len(.Text)
          .SetFocus
        End With
      End Sub



      Private Sub DisplayPropertyListBox(ByVal posX As Long, ByVal posY As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal strProp As String)
        'moves the list box to appropriate position
        'preloads it with appropriate prop Value

        Dim i As Long
        Dim tmpDur As Single

        lstProperty.Width = nWidth
        lstProperty.Height = nHeight

        'if there is room
        If picProperties.Top + posY < picProperties.Top + picProperties.Height - lstProperty.Height Then
          lstProperty.Move picProperties.Left + posX, picProperties.Top + posY
        Else
          lstProperty.Move picProperties.Left + posX, picProperties.Top + picProperties.Height - lstProperty.Height
        End If

        'clear the list box
        lstProperty.Clear

        Select Case strProp
        Case "KEY"
        'load key signatures
        With lstProperty
          For i = 0 To 14
            .AddItem LoadResString(KEYSIGNATURE + i)
          Next i
        End With

        'set key sig
        lstProperty.ListIndex = SoundEdit.Key + 7

        Case "INST"
          With lstProperty
            For i = 0 To 127
              .AddItem LoadResString(INSTRUMENTNAMETEXT + i)
            Next i
          End With
          lstProperty.ListIndex = SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument

        Case "MUTE"
          lstProperty.AddItem "False"
          lstProperty.AddItem "True"
          lstProperty.ListIndex = IIf(SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Muted, 1, 0)

        Case "VIS"
          lstProperty.AddItem "False"
          lstProperty.AddItem "True"
          lstProperty.ListIndex = IIf(SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible, 1, 0)

        Case "TYPE"
          'load choices for noise Type
          lstProperty.AddItem "Periodic"
          lstProperty.AddItem "White Noise"
          lstProperty.ListIndex = IIf(SoundEdit.Track(3).Notes(tvwSound.SelectedItem.Tag).FreqDivisor And 4, 1, 0)

        Case "NOISEFREQ"
          'load choices for noise freq
          lstProperty.AddItem "2330 Hz"
          lstProperty.AddItem "1165 Hz"
          lstProperty.AddItem "583 Hz"
          lstProperty.AddItem "Track 2"

          lstProperty.ListIndex = SoundEdit.Track(3).Notes(tvwSound.SelectedItem.Tag).FreqDivisor And 3

        Case "MNOTE"
          'add notes
          If MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor) \ 12 < 10 Then
            lstProperty.AddItem "B"
            lstProperty.AddItem "A#/Bb"
            lstProperty.AddItem "A"
          End If
          If MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor) \ 12 > 3 And _
             MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor) \ 12 < 10 Then
            lstProperty.AddItem "G#/Ab"
          End If
          If MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor) \ 12 > 3 Then
            lstProperty.AddItem "G"
            lstProperty.AddItem "F#/Gb"
            lstProperty.AddItem "F"
            lstProperty.AddItem "E"
            lstProperty.AddItem "D#/Eb"
            lstProperty.AddItem "D"
            lstProperty.AddItem "C#/Db"
            lstProperty.AddItem "C"
          End If

          'select current note (invert so it goes from low to high)
          i = MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor) Mod 12
          'adjust if in last octave
          If lstProperty.ListCount = 3 Then
            i = i - 9
          End If
          lstProperty.ListIndex = lstProperty.ListCount - i - 1

        Case "MOCT"
          If MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor) Mod 12 <= 7 Then
            lstProperty.AddItem "10"
          End If
          lstProperty.AddItem "9"
          lstProperty.AddItem "8"
          lstProperty.AddItem "7"
          lstProperty.AddItem "6"
          lstProperty.AddItem "5"
          lstProperty.AddItem "4"
          If MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor) Mod 12 >= 9 Then
            lstProperty.AddItem "3"
          End If

          'select current octave
          lstProperty.Text = CStr(MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).FreqDivisor) \ 12)

        Case "MLEN"
          'add note length choices to list box
          lstProperty.AddItem "1/16"
          lstProperty.AddItem "1/8"
          lstProperty.AddItem "1/8*"
          lstProperty.AddItem "1/4"
          lstProperty.AddItem "1/4 + 1/16"
          lstProperty.AddItem "1/4*"
          lstProperty.AddItem "1/4**"
          lstProperty.AddItem "1/2"
          lstProperty.AddItem "1/2 + 1/16"
          lstProperty.AddItem "1/2 + 1/8"
          lstProperty.AddItem "1/2 + 1/8*"
          lstProperty.AddItem "1/2*"
          lstProperty.AddItem "1/2* + 1/16"
          lstProperty.AddItem "1/2**"
          lstProperty.AddItem "1/2** + 1/16"
          lstProperty.AddItem "1"

          'if note length is not undefined
          tmpDur = SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(tvwSound.SelectedItem.Tag).Duration / SoundEdit.TPQN * 4
          If tmpDur = Int(tmpDur) And tmpDur <= 16 And tmpDur > 0 Then
            lstProperty.ListIndex = tmpDur - 1
          End If
        End Select

        'pass property to tag
        lstProperty.Tag = strProp

        'show list box
        lstProperty.Visible = True
        'set top index to the selected Value
        If lstProperty.ListIndex >= 0 Then
          lstProperty.TopIndex = lstProperty.ListIndex
        End If

        'set focus to listbox
        lstProperty.SetFocus
      End Sub
      Private Sub picProperties_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)
        'cache the Y Value
        mY = Y
      End Sub

      Private Sub AddUndo(NextUndo As SoundUndo)

        If Not IsDirty Then
          MarkAsDirty
        End If

        'remove old undo items until there is room for this one
        'to be added
        If Settings.SndUndo > 0 Then
          Do While UndoCol.Count > Settings.SndUndo
            UndoCol.Remove 1
          Loop
        End If

        'adds the next undo object
        UndoCol.Add NextUndo

        'set undo menu
        frmMDIMain.mnuEUndo.Enabled = True
        frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(SNDUNDOTEXT + NextUndo.UDAction) & vbTab & "Ctrl+Z"
      End Sub
      Public Sub MenuClickClear()

        'verify notes are selected

        'shift selected notes up one note on scale
        ShiftTone 1
      End Sub

      Public Sub MenuClickCopy()

        Dim i As Long

        'if an active selection
        If SelTrack <> -1 And SelStart >= 0 And SelLength > 0 Then

          'clear clipboard
          SoundClipboard.Clear

          'add selected notes
          For i = 0 To SelLength - 1
            SoundClipboard.Add SoundEdit.Track(SelTrack).Notes(SelStart + i).FreqDivisor, SoundEdit.Track(SelTrack).Notes(SelStart + i).Duration, SoundEdit.Track(SelTrack).Notes(SelStart + i).Attenuation
          Next i

          'set mode based on track
          If SelTrack = 3 Then
            'notes are noise note
            SoundCBMode = 1
          Else
            'notes are regular notes
            SoundCBMode = 0
          End If

          'update menus
          SetEditMenu
        End If
      End Sub
      Public Sub MenuClickCut()

        'copy
        MenuClickCopy

        'then delete
        MenuClickDelete

        'change last undo item to 'cut'
        UndoCol(UndoCol.Count).UDAction = udsCut

      End Sub

      Public Sub MenuClickDelete()

        'if a track is selected, and insertion point is visible
        If SelTrack <> -1 And SelStart >= 0 Then
          'delete
          If SelLength = 0 Then
            If SelStart < SoundEdit.Track(SelTrack).Notes.Count Then
              DeleteNotes SelTrack, SelStart, 1
            End If
          Else
            DeleteNotes SelTrack, SelStart, SelLength
          End If
        End If
      End Sub


      Public Sub MenuClickInsert()

        'shift selected notes down
        ShiftTone -1
      End Sub
      Public Sub MenuClickOpen()
        'implemented by frmMDIMain

      End Sub
*/
        }

        public void MenuClickSave() {
            /*
            'save this resource

        Dim rtn As VbMsgBoxResult
        Dim i As Long
        Dim blnLoaded As Boolean

        On Error GoTo ErrHandler

        'if in a game,
        If InGame Then
          'show wait cursor
          WaitCursor

          blnLoaded = Sounds(SoundNumber).Loaded
          If Not blnLoaded Then
            Sounds(SoundNumber).Load
          End If

          'copy Sound back to game resource
          Sounds(SoundNumber).SetSound SoundEdit

          'save the sound
          Sounds(SoundNumber).Save

          'copy back to edit sound
          SoundEdit.SetSound Sounds(SoundNumber)
          'setsound copies loaded status to ingame sound resource; need to unload it
          Sounds(SoundNumber).Unload

          If Not blnLoaded Then
            Sounds(SoundNumber).Unload
          End If

          'update preview and properties
          UpdateSelection AGIResType.Sound, SoundNumber, umPreview Or umProperty

          'if autoexporting,
          If Settings.AutoExport Then
            'export using default name
            SoundEdit.Export ResDir & SoundEdit.ID & ".ags"
            'reset ID
            SoundEdit.ID = Sounds(SoundEdit.Number).ID
          End If

          'restore cursor
          Screen.MousePointer = vbDefault
        Else
          'if no name yet,
          If LenB(SoundEdit.Resource.ResFile) = 0 Then
            'use export to get a name
            MenuClickExport
            Exit Sub
          Else
            'show wait cursor
            WaitCursor

            'save the Soundedit Sound
            SoundEdit.Export SoundEdit.Resource.ResFile

            'restore cursor
            Screen.MousePointer = vbDefault
          End If
        End If

        'if not midi
        If LCase$(Right$(SoundEdit.Resource.ResFile, 4)) <> ".mid" Then
          'reset dirty flag
          IsDirty = False
          'reset caption
          Caption = sSNDED & ResourceName(SoundEdit, InGame, True)
          'disable the save menu
          frmMDIMain.mnuRSave.Enabled = False
          frmMDIMain.Toolbar1.Buttons("save").Enabled = False
        End If

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
            */
        }

        void sndfrmcode() {
            /*
      Public Sub MenuClickExport()

        Dim strFileName As String

        If ExportSound(SoundEdit, InGame) Then
          If Not InGame Then
            'if not midi
            If LCase$(Right$(SoundEdit.ID, 4)) <> ".mid" Then
              'reset dirty flag
              IsDirty = False
              'reset caption
              Caption = sSNDED & ResourceName(SoundEdit, InGame, True)
            Else
              'reset id to savefile name
              SoundEdit.ID = Path.GetFileName(SoundEdit.Resource.ResFile)
            End If

            'disable menu and toolbar button
            frmMDIMain.mnuRSave.Enabled = False
            frmMDIMain.Toolbar1.Buttons("save").Enabled = False
            'update tree node 0
            tvwSound.Nodes(1).Text = SoundEdit.ID
          Else
            'for ingame resources, SoundEdit is not actually
            'the ingame resource, but only a copy that can be edited
            'because the resource ID is changed to match savefile
            'name during the export operation, the ID needs to be
            'forced back to the correct Value
            SoundEdit.ID = Sounds(SoundNumber).ID
          End If
        End If
      End Sub
      Public Sub MenuClickImport()
        Dim tmpSnd As AGISound
        Dim i As Long

        On Error GoTo ErrHandler

        'this method is only called by the Main form's Import function
        'the MainDialog object will contain the name of the file
        'being imported.

        'steps to import are to import the sound to tmp object
        'clear the existing Image, copy tmpobject to this item
        'and reset it

        Set tmpSnd = New AGISound
        On Error Resume Next
        tmpSnd.Import MainDialog.FileName
        If Err.Number <> 0 Then
          ErrMsgBox "An error occurred while importing this sound:", "", "Import Sound Error"
          Exit Sub
        End If

        'clear Sound
        SoundEdit.Clear

        'copy tmpSndture data to SoundEdit
        SoundEdit.Resource.InsertData tmpSnd.Resource.AllData, 0
        'remove the last 11 bytes (left over from the insert process)
        SoundEdit.Resource.RemoveData SoundEdit.Resource.Size - 11, 11

        'discard the temp sound
        tmpSnd.Unload
        Set tmpSnd = Nothing

        'load tree with actual sound data
        If Not BuildSoundTree() Then
          'error
          ErrMsgBox "This sound has corrupt or invalid data. Reverting to original sound.", "", "Sound Resource Data Error"
          SoundEdit.Unload
          Set SoundEdit = Nothing
          EditSound Sounds(SoundNumber)
          Exit Sub
        End If

        SetKeyWidth

        'show/hide staves as appropriate
        If OneTrack Then
          DrawStaff -2
        Else
          For i = 0 To 3
            picStaff(i).Visible = SoundEdit.Track(i).Visible
            picStaffVis(i) = SoundEdit.Track(i).Visible
          Next i
          DrawStaff -1
        End If

        ChangeSelection -1, 0, 0

        'clear the undo buffer
        If UndoCol.Count > 0 Then
          For i = UndoCol.Count To 1 Step -1
            UndoCol.Remove i
          Next i
          SetEditMenu
        End If

        'mark as dirty
        MarkAsDirty
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub MenuClickNew()
        'implemented by frmMDIMain

      End Sub

      Public Sub MenuClickInGame()
        'toggles the game state of an object

        Dim rtn As VbMsgBoxResult
        Dim tmpNode As Node
        Dim blnDontAsk As Boolean

        If InGame Then
          'ask if resource should be exported
          If Settings.AskExport Then
            rtn = MsgBoxEx("Do you want to export '" & SoundEdit.ID & "' before removing it from your game?", _
                                vbQuestion + vbYesNoCancel, "Export Sound Before Removal", , , _
                                "Don't ask this question again", blnDontAsk)

            'save the setting
            Settings.AskExport = Not blnDontAsk
            'if now hiding update settings file
            If Not Settings.AskExport Then
              WriteSetting GameSettings, sGENERAL, "AskExport", Settings.AskExport
            End If
          Else
            'dont ask; assume no
            rtn = vbNo
          End If

          Select Case rtn
          Case vbCancel
            Exit Sub

          Case vbYes
            'export it
            MenuClickExport
          Case vbNo
            'nothing to do
          End Select

          'confirm removal
          If Settings.AskRemove Then
            rtn = MsgBoxEx("Removing '" & SoundEdit.ID & "' from your game." & vbCrLf & vbCrLf & "Select OK to proceed, or Cancel to keep it in game.", _
                            vbQuestion + vbOKCancel, "Remove Sound From Game", , , _
                            "Don't ask this question again", blnDontAsk)

            'save the setting
            Settings.AskRemove = Not blnDontAsk
            'if now hiding, update settings file
            If Not Settings.AskRemove Then
              WriteSetting GameSettings, sGENERAL, "AskRemove", Settings.AskRemove
            End If
          Else
            'assume OK
            rtn = vbOK
          End If

          'if canceled,
          If rtn = vbCancel Then
            Exit Sub
          End If

          'now remove the Sound
          RemoveSound SoundNumber

          'unload this form
          Unload Me
        Else
          'add to game

          'verify a game is loaded,
          If Not GameLoaded Then
            Exit Sub
          End If

          'show add resource form
          With frmGetResourceNum
            .ResType = AGIResType.Sound
            .WindowFunction = grAddInGame
            'setup before loading so ghosts don't show up
            .FormSetup
            .Show vbModal, frmMDIMain

            'if user makes a choice
            If Not .Canceled Then
              'store number
              SoundNumber = .NewResNum
              'new id
              SoundEdit.ID = .txtID.TabIndex
              'add Sound
              AddNewSound SoundNumber, SoundEdit

              'copy the Sound back (to ensure internal variables are copied)
              SoundEdit.Clear
              SoundEdit.SetSound Sounds(SoundNumber)

              'now we can unload the newly added sound;
              Sounds(SoundNumber).Unload

              'update caption and properties
              tvwSound.Nodes(1).Text = ResourceName(SoundEdit, True, True)
              Caption = sSNDED & tvwSound.Nodes(1).Text
              PaintPropertyWindow

              'set ingame flag
              InGame = True
              'reset dirty flag
              IsDirty = False

              'change menu caption
              frmMDIMain.mnuRInGame.Caption = "Remove from Game"
              frmMDIMain.Toolbar1.Buttons("remove").Image = 10
              frmMDIMain.Toolbar1.Buttons("remove").ToolTipText = "Remove from Game"
            End If
          End With

          Unload frmGetResourceNum
        End If
      End Sub
      Public Sub MenuClickRenumber()

        'renumbers a resource

        Dim NewResNum As Byte

        On Error GoTo ErrHandler:

        'if not in a game
        If Not InGame Then
          Exit Sub
        End If

        'get new number
        NewResNum = RenumberResource(SoundNumber, AGIResType.Sound)

        'if changed,
        If NewResNum <> SoundNumber Then
          'copy renumbered Sound into Soundedit object
          SoundEdit.SetSound Sounds(NewResNum)

          'update number
          SoundNumber = NewResNum

          'update caption
          Caption = sSNDED & ResourceName(SoundEdit, InGame)
          If SoundEdit.IsDirty Then
            Caption = sDM & Caption
          End If

          'and tree
          Me.tvwSound.Nodes(1).Text = SoundEdit.ID
          'force repaint of property window
          PaintPropertyWindow
        End If
      Exit Sub

      ErrHandler:
        Resume Next
      End Sub
      Public Sub MenuClickECustom1()
        'play sound or stop sound, if already playing

        Dim rtn As Long

        On Error GoTo ErrHandler

        'should never get here if midi is disabled
        '*'Debug.Assert Not Settings.NoMIDI

        'if not playing
        'If frmMDIMain.mnuECustom1.Caption = "Play Sound" & vbTab & "Ctrl+Enter" Then
        If cmdPlay.Enabled Then
          'if something to play (at least one track being played
          'with at least one note)
          For rtn = 0 To 3
            If Not SoundEdit.Track(rtn).Muted And SoundEdit.Track(rtn).Length > 0 Then
              'sound can be played
              Exit For
            End If
          Next rtn

          'if valid sound found, then rtn WONT be equal to 4
          If rtn <> 4 Then
            'close midi so the sound resource can access midi functions
            KillMIDI

            'play sound
            SoundEdit.PlaySound(1)
            'shouldnt ever get to this
            'because errhandling is set to
            'goto ErrHandler already
            If Err.Number <> 0 Then
              GoTo ErrHandler
            End If

            'set menu caption & buttons
            frmMDIMain.mnuECustom1.Caption = "Stop Sound" & vbTab & "Ctrl+Enter"
            cmdPlay.Enabled = False
            cmdStop.Enabled = True
          End If
        Else
          'stop sound
          SoundEdit.StopSound

          'restore midi so this form can play notes
          'as user presses keys on keyboard
          InitMIDI

          'restore menu caption & buttons
          frmMDIMain.mnuECustom1.Caption = "Play Sound" & vbTab & "Ctrl+Enter"
          cmdPlay.Enabled = Not Settings.NoMIDI
          cmdStop.Enabled = False
        End If
      Exit Sub

      ErrHandler:
        ErrMsgBox "An error occurred while attempting to play this sound on your MIDI device: ", "MIDI playback will be disabled.", "MIDI Error"
        Settings.NoMIDI = True
        KillMIDI
      End Sub

      Public Sub MenuClickECustom2()

        'toggle keyboard, scrollbar and duration picker
        With picKeyboard
          .Visible = Not .Visible
          hsbKeyboard.Visible = .Visible
          picDuration.Visible = .Visible
          udDuration.Visible = .Visible
        End With

        'update menu
        frmMDIMain.mnuECustom2.Caption = IIf(picKeyboard.Visible, "Hide", "Show") & " Keyboard"

        'refresh
        Form_Resize
        'redraw
        DrawStaff -1
      End Sub

      Public Sub MenuClickPaste()

        'inserts clipboard notes into this track

        Dim blnReplace As Boolean

        'if clipboard has regular notes and seltrack is a regular track OR
        '   clipboard has noise notes and seltrack is noise AND there are notes on the clipboard
        If ((SoundCBMode = 0 And SelTrack >= 0 And SelTrack <= 2) Or _
           (SoundCBMode = 1 And SelTrack = 3)) And SoundClipboard.Count <> 0 Then
          'if selection is >=1
          If SelLength >= 1 Then
            'delete selection first
            DeleteNotes SelTrack, SelStart, SelLength
            blnReplace = True
          End If

          'insert clipboard notes
          AddNotes SelTrack, SelStart, SoundClipboard, False

          'if replacing,
          If blnReplace Then
            'set flag in undo item so the undo action
            'deletes the pasted notes and restores original notes
            UndoCol(UndoCol.Count).UDText = "R"
          End If
        End If
      End Sub
      Public Sub MenuClickSelectAll()

        'it there is a track
        '*'Debug.Assert SelTrack <> -1

        ChangeSelection SelTrack, 0, SoundEdit.Track(SelTrack).Notes.Count
      End Sub
      Public Sub MenuClickUndo()

        Dim NextUndo As SoundUndo
        Dim i As Long, j As Long
        Dim rtn As Long, PrevTrack As Long

        On Error GoTo ErrHandler

        'if there are no undo actions
        If UndoCol.Count = 0 Then
          'just exit
          Exit Sub
        End If

        'save previous track so keyboard can
        'be correctly drawn if changing to/from
        'Noise track as a result of Undo action
        PrevTrack = SelTrack

        'get next undo object
        Set NextUndo = UndoCol(UndoCol.Count)
        'remove undo object
        UndoCol.Remove UndoCol.Count
        'reset undo menu
        frmMDIMain.mnuEUndo.Enabled = (UndoCol.Count > 0)
        If frmMDIMain.mnuEUndo.Enabled Then
          frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(SNDUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
        Else
          frmMDIMain.mnuEUndo.Caption = "&Undo " & vbTab & "Ctrl+Z"
        End If

        'undo the action
        Select Case NextUndo.UDAction
        Case udsEditNote
          Select Case NextUndo.UDLength
          Case 1  'changed freq
            SoundEdit.Track(NextUndo.UDTrack).Notes(NextUndo.UDStart).FreqDivisor = CLng(NextUndo.UDText)
          Case 2  'changed dur
            SoundEdit.Track(NextUndo.UDTrack).Notes(NextUndo.UDStart).Duration = CLng(NextUndo.UDText)
          Case 3  'change vol/att
            SoundEdit.Track(NextUndo.UDTrack).Notes(NextUndo.UDStart).Attenuation = CLng(NextUndo.UDText)
          End Select

          ChangeSelection NextUndo.UDTrack, NextUndo.UDStart, 1

        Case udsAddNote, udsPaste
          'remove note
          DeleteNotes NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDLength, True
          'check for replace
          If NextUndo.UDText = "R" Then
            'recurse to reinsert what was replaced
            MenuClickUndo
          End If

        Case udsDelete, udsCut
          'add notes back
          AddNotes NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDNotes, True, True

        Case udsChangeKey
          SoundEdit.Key = NextUndo.UDStart
          'force redraw of all staves
          DrawStaff -1
          If tvwSound.Nodes(1).Selected Then
            PaintPropertyWindow
          End If
          'reset selection to no length
          ChangeSelection -1, SelStart, 0, False, False, True

        Case udsChangeTPQN
          SoundEdit.TPQN = NextUndo.UDStart
          'force redraw
          DrawStaff -1
          If tvwSound.Nodes(1).Selected Then
            PaintPropertyWindow
          End If
          'update any selection
          ChangeSelection SelTrack, SelStart, SelLength

        Case udsChangeInstrument
          SoundEdit.Track(NextUndo.UDTrack).Instrument = NextUndo.UDStart
          If tvwSound.Nodes(NextUndo.UDTrack + 2).Selected Then
            'force update of propertywindow
            PaintPropertyWindow
          End If
          'if this track is current
          If SelTrack = NextUndo.UDTrack And Not Settings.NoMIDI And hMIDI <> 0 Then
            'send instrument to midi
            midiOutShortMsg hMIDI, CLng(SoundEdit.Track(SelTrack).Instrument * &H100 + &HC0)
          End If

        Case udsShiftKey
          For i = 0 To NextUndo.UDLength - 1
            SoundEdit.Track(NextUndo.UDTrack).Notes(NextUndo.UDStart + i).FreqDivisor = Asc(Mid$(NextUndo.UDText, i * 2 + 1)) * &H100& + Asc(Mid$(NextUndo.UDText, i * 2 + 2))
          Next i
          ChangeSelection NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDLength

        Case udsShiftVol
          For i = 0 To NextUndo.UDLength - 1
            SoundEdit.Track(NextUndo.UDTrack).Notes(NextUndo.UDStart + i).Attenuation = Asc(Mid$(NextUndo.UDText, i + 1))
          Next i
          ChangeSelection NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDLength

        End Select

        Set NextUndo = Nothing
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Function AskClose() As Boolean

        Dim rtn As VbMsgBoxResult

        On Error GoTo ErrHandler

        'assume okay to close
        AskClose = True

        'if exiting due to error on form load, soundedit is set to nothing
        If SoundEdit Is Nothing Then
          Exit Function
        End If

        'if the Sound needs to be saved,
        '(number is set to -1 if closing is forced)
        If IsDirty And SoundNumber <> -1 Then
          rtn = MsgBox("Do you want to save changes to " & SoundEdit.ID & " ?", vbYesNoCancel, "Sound Editor")

          Select Case rtn
          Case vbYes
            'save, then continue closing
            MenuClickSave
          Case vbNo
            'don't save, and continue closing
          Case vbCancel
            'don't continue closing
            AskClose = False
          End Select
        End If
      Exit Function

      ErrHandler:
        Resume Next
      End Function


      Public Sub SetEditMenu()
        'sets the menu captions on the Sound Edit menu
        'and updates toolbar to match

        With frmMDIMain
          'always hide find, find again, replace and bar1
          .mnuEFind.Visible = False
          .mnuEFindAgain.Visible = False
          .mnuEReplace.Visible = False
          .mnuEBar1.Visible = False
          .mnuERedo.Visible = False
          .mnuECustom3.Enabled = False

          'always show both edit menu customs
          .mnuECustom1.Visible = True
          .mnuECustom1.Enabled = Not Settings.NoMIDI
          .mnuECustom1.Caption = "Play Sound" & vbTab & "Ctrl+Enter"
          .mnuECustom2.Visible = True
          .mnuECustom2.Enabled = True
          .mnuECustom2.Caption = IIf(picKeyboard.Visible, "Hide", "Show") & " Keyboard" & vbTab & "Ctrl+K"
          .mnuEBar2.Visible = True

          'show undo
          .mnuEUndo.Visible = True
          .mnuEUndo.Enabled = UndoCol.Count <> 0
          If UndoCol.Count <> 0 Then
            .mnuEUndo.Caption = "&Undo " & LoadResString(SNDUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
          Else
            .mnuEUndo.Caption = "Undo" & vbTab & "Ctrl+Z"
          End If
          .mnuEBar0.Visible = True

          'cut enabled if notes are selected
          .mnuECut.Visible = True
          .mnuECut.Enabled = (SelTrack <> -1 And SelStart >= 0 And SelLength > 0)
          .mnuECut.Caption = "Cu&t" & vbTab & "Ctrl+X"
          Toolbar1.Buttons("cu").Enabled = .mnuECut.Enabled

          'copy enabled if notes are selected
          .mnuECopy.Visible = True
          .mnuECopy.Enabled = (SelTrack <> -1 And SelStart >= 0 And SelLength > 0)
          .mnuECopy.Caption = "&Copy" & vbTab & "Ctrl+C"
          Toolbar1.Buttons("co").Enabled = .mnuECopy.Enabled

          'paste enabled if selected track matches clipboard mode, AND there are notes on the clipboard
          .mnuEPaste.Visible = True
          .mnuEPaste.Enabled = ((SoundCBMode = 0 And SelTrack >= 0 And SelTrack <= 2) Or (SoundCBMode = 1 And SelTrack = 3)) And SoundClipboard.Count <> 0
          .mnuEPaste.Caption = "&Paste" & vbTab & "Ctrl+V"
          Toolbar1.Buttons("pa").Enabled = .mnuEPaste.Enabled

          'delete enabled if notes are selected
          '(note that pressing delete key will still work
          'even if SelLength=0, but menu item enabled only
          'if SelLength>0)
          .mnuEDelete.Visible = True
          .mnuEDelete.Enabled = (SelTrack <> -1 And SelStart >= 0 And SelLength > 0)
          .mnuEDelete.Caption = "&Delete" & vbTab & "Del"
          Toolbar1.Buttons("de").Enabled = .mnuEDelete.Enabled

          'clear used for shift notes up
          'shift up enabled if notes on music track are selected
          '(not on noise track)
          .mnuEClear.Visible = True
          .mnuEClear.Enabled = (SelTrack >= 0 And SelTrack <= 2 And SelStart >= 0 And SelLength > 0)
          .mnuEClear.Caption = "Shift &Up" & vbTab & "Alt+U"
          Toolbar1.Buttons("su").Enabled = .mnuEClear.Enabled

          'insert used for shift notes down
          'shift down enabled if notes on music track are selected
          '(not on noise track)
          .mnuEInsert.Visible = True
          .mnuEInsert.Enabled = (SelTrack >= 0 And SelTrack <= 2 And SelStart >= 0 And SelLength > 0)
          .mnuEInsert.Caption = "Shift D&own" & vbTab & "Alt+D"
          Toolbar1.Buttons("sd").Enabled = .mnuEInsert.Enabled

          'select all enabled if a track is active
          .mnuESelectAll.Visible = True
          .mnuESelectAll.Enabled = (SelTrack <> -1)
          .mnuESelectAll.Caption = "Select &All" & vbTab & "Ctrl+A"

          'volume buttons on toolbar enabled if something selected
          Toolbar1.Buttons("vu").Enabled = .mnuEClear.Enabled
          Toolbar1.Buttons("vd").Enabled = .mnuEClear.Enabled

          'set text of custom menu based on showtrack value
          If OneTrack Then
            frmMDIMain.mnuRCustom1.Caption = "Show All Visible Tracks"
          Else
            frmMDIMain.mnuRCustom1.Caption = "Show Only Selected Track"
          End If

        End With
      End Sub


      Private Sub MarkAsDirty()

        If Not IsDirty Then
          IsDirty = True
        End If

        'enable menu and toolbar button
        frmMDIMain.mnuRSave.Enabled = True
        frmMDIMain.Toolbar1.Buttons("save").Enabled = True

        If Asc(Caption) <> 42 Then
          'mark caption
          Caption = sDM & Caption
        End If
      End Sub
      Private Sub Form_Activate()

        Dim rtn As Long

        On Error GoTo ErrHandler

        'if minimized, exit
        '(to deal with occasional glitch causing focus to lock up)
        If Me.WindowState = vbMinimized Then
          Exit Sub
        End If

        'if hiding prevwin on lost focus, hide it now
        If Settings.HidePreview Then
          '*'Debug.Assert Me.Visible
          PreviewWin.Hide
        End If

        'if visible,
        If Visible Then
          'force resize
          Form_Resize
        End If

        AdjustMenus AGIResType.Sound, InGame, True, IsDirty
        SetEditMenu

        'if findform is visible,
        If FindForm.Visible Then
          'hide it it
          FindForm.Visible = False
        End If

        MainStatusBar.Panels("Scale").Text = "Scale: " & CStr(StaffScale)

        'if midi is disabled
        If Settings.NoMIDI Then
          Exit Sub
        End If

        'if there is not a midi handle, get one
        If hMIDI = 0 Then
          InitMIDI
        End If

        'disable play button if no MIDI
        cmdPlay.Enabled = Not Settings.NoMIDI

        'paint the default duration box
        ShowDefaultDuration
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

        On Error GoTo ErrHandler

        'always check for help first
        If Shift = 0 And KeyCode = vbKeyF1 Then
          MenuClickHelp
          KeyCode = 0
          Exit Sub
        End If

        'don't override editboxes or the property box
        If ActiveControl Is txtProperty Or ActiveControl Is lstProperty Then
          Exit Sub
        End If

        'check for global shortcut keys
        CheckShortcuts KeyCode, Shift
        If KeyCode = 0 Then
          Exit Sub
        End If

        Select Case Shift
        Case vbCtrlMask
          Select Case KeyCode
          Case vbKeyA
            'select all
            If frmMDIMain.mnuESelectAll.Enabled Then
              MenuClickSelectAll
              KeyCode = 0
            End If

          Case vbKeyZ
            'undo
            If frmMDIMain.mnuEUndo.Enabled Then
              MenuClickUndo
              KeyCode = 0
            End If

          Case vbKeyX
            'cut
            If frmMDIMain.mnuECut.Enabled Then
              MenuClickCut
              KeyCode = 0
            End If

          Case vbKeyC
            'copy
            If frmMDIMain.mnuECopy.Enabled Then
              MenuClickCopy
              KeyCode = 0
            End If

          Case vbKeyV
            'paste
            If frmMDIMain.mnuEPaste.Enabled Then
              MenuClickPaste
              KeyCode = 0
            End If

          Case vbKeyK
            If frmMDIMain.mnuECustom2.Enabled Then
              MenuClickECustom2
              KeyCode = 0
            End If

          Case vbKeyReturn
            If frmMDIMain.mnuECustom1 Then
              MenuClickECustom1
              KeyCode = 0
            End If
          End Select
        Case 0
          'no shift, ctrl, alt
          Select Case KeyCode
          Case vbKeyDelete
            'If frmMDIMain.mnuEDelete.Enabled Then
              MenuClickDelete
              KeyCode = 0
            'End If

          Case vbKeyLeft, vbKeyUp
            'if there is an active selection or cursor is not at start,
            If SelTrack <> -1 And (SelStart > 0 Or SelLength > 0) Then
              'if just a cursor
              If SelLength = 0 Then
                'move left one note
                ChangeSelection SelTrack, SelStart - 1, 0
              Else
                'if anchor is to right of selection
                If SelStart <> SelAnchor Then
                  'reset start to right of selection before collapsing
                  ChangeSelection SelTrack, SelAnchor - SelLength, 0
                Else
                  'collapse to current startpos
                  ChangeSelection SelTrack, SelStart, 0
                End If
              End If
              SelAnchor = SelStart
            End If

          Case vbKeyRight, vbKeyDown
            'if there is an active selection or cursor is not at end,
            If SelTrack <> -1 And (SelStart < SoundEdit.Track(SelTrack).Notes.Count Or SelLength > 0) Then
              If SelLength = 0 Then
                'move right one note
                ChangeSelection SelTrack, SelStart + 1, 0, False, True
              Else
                If SelStart = SelAnchor Then
                  ChangeSelection SelTrack, SelAnchor + SelLength, 0 ', True
                Else
                  ChangeSelection SelTrack, SelAnchor, 0 ', True
                End If
              End If
            End If

          Case vbKeyBack
            'if selection is >0
            If SelLength > 0 Then
              MenuClickDelete
            Else
              'if not on first note
              If SelStart > 0 Then
                'move it back one, and delete
                SelStart = SelStart - 1
                MenuClickDelete
              End If
            End If
            KeyCode = 0
          End Select

        Case vbShiftMask
            'if working with a staff, adjust selection; if working with tracks, exit
            If tvwSound.SelectedItem.Parent Is Nothing Then
              Exit Sub
            ElseIf tvwSound.SelectedItem.Parent.Parent Is Nothing Then
              Exit Sub
            End If

          Select Case KeyCode
          Case vbKeyLeft, vbKeyUp

            'selection is expanded to left, or collapsed on the right,
            'depending on where current position is relative to the anchor

            'if there is an active selection of one or more notes
            'AND the startpoint is the anchor,
            If SelLength > 0 And SelAnchor = SelStart Then
              'shrink the selection by one note (move end pt)
              ChangeSelection SelTrack, SelStart, SelLength - 1, False, True
            Else
              'if starting point not yet at beginning of track,
              If SelStart > 0 Then
                'expand selection by one note (move start)
                ChangeSelection SelTrack, SelAnchor - SelLength - 1, SelLength + 1
              Else
                'cursor is already at beginning; just exit
                Exit Sub
              End If
            End If


          Case vbKeyRight, vbKeyDown
            'selection is expanded to right, or collapsed on the left,
            'depending on where current position is relative to the anchor

            'if there is an active selection of one or more notes AND the startpoint is the anchor,
            'OR there is no current selection
            If SelLength >= 0 And SelAnchor = SelStart Then
              'if not yet at end of track
              If SelStart + SelLength < SoundEdit.Track(SelTrack).Notes.Count Then
                'expand selection (move end pt)
                ChangeSelection SelTrack, SelStart, SelLength + 1, False, True
              Else
                'cursor is already at end; just exit
                Exit Sub
              End If
            Else
              'shrink selection by one note (move start pt)
              ChangeSelection SelTrack, SelAnchor - SelLength + 1, SelLength - 1
            End If
          End Select
        Case vbAltMask
          Select Case KeyCode
          Case vbKeyU
            If frmMDIMain.mnuEClear.Enabled Then
              MenuClickClear
              KeyCode = 0
              Shift = 0
            End If

          Case vbKeyD
            If frmMDIMain.mnuEInsert.Enabled Then
              MenuClickInsert
              KeyCode = 0
              Shift = 0
            End If
          End Select
        End Select
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Private Sub Form_Load()

        Dim i As Long

      #If DEBUGMODE <> 1 Then
        'subclass the form for mousewheel scrolling
        PrevSEWndProc = SetWindowLong(Me.hWnd, GWL_WNDPROC, AddressOf ScrollWndProc)
      #End If

        CalcWidth = MIN_WIDTH
        CalcHeight = MIN_HEIGHT

        'set property window so startup doesn't trip during resize events
        PropRowCount = 3
        picProperties.Height = (PropRowCount + 1) * PropRowHeight + 1

        'set defaults
        StaffScale = Settings.SndZoom
        SelTrack = -1
        DefLength = 4
        DefOctave = 5
        MKbOffset = 45
        lngMIDInote = -1
        OneTrack = Settings.OneTrack

        For i = 0 To 3
          'set default offsets
          SOVert(i) = -(52 * StaffScale)
          vsbStaff(i).SmallChange = picStaff(i).Height * SM_SCROLL
          vsbStaff(i).LargeChange = picStaff(i).Height * LG_SCROLL
          vsbStaff(i).Max = -SOVert(i) * 2
          vsbStaff(i).Value = -SOVert(i)
          picStaff(i).FontSize = 5 * StaffScale
          'local copy of picStaff visible property
          picStaffVis(i) = True
        Next i

        'position first track at top
        picStaff(0).Top = SE_MARGIN

        'get listitem height
        ListItemHeight = SendMessage(lstProperty.hWnd, LB_GETITEMHEIGHT, 0, 0)

        'set undo collection
        Set UndoCol = New Collection
        frmMDIMain.mnuEUndo.Caption = "&Undo" & vbTab & "Ctrl+Z"
        frmMDIMain.mnuEUndo.Enabled = False

        'disable play button if no MIDI
        cmdPlay.Enabled = Not Settings.NoMIDI

        'draw the default note display
        ShowDefaultDuration
      End Sub

      Private Sub Form_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        On Error GoTo ErrHandler

        'if right button
        If Button = vbRightButton Then
          'reset edit menu first
          SetEditMenu
          'make sure this form is the active form
          If Not (frmMDIMain.ActiveMdiChild Is Me) Then
            'set focus before showing the menu
            Me.SetFocus
          End If
          'need doevents so form activation occurs BEFORE popup
          'otherwise, errors will be generated because of menu
          'adjustments that are made in the form_activate event
          SafeDoEvents
          'show edit menu
          PopupMenu frmMDIMain.mnuEdit, , X, Y
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)

        'check if save is necessary (and set cancel if user cancels)
        Cancel = Not AskClose
      End Sub

      Private Sub Form_Resize()

        Dim i As Long, j As Long
        Dim lngStaffCount As Long
        Dim lngKybdHt As Long
        Dim lngBottom As Long

        On Error GoTo ErrHandler

        'use separate variables for managing minimum width/height
        If ScaleWidth < MIN_WIDTH Then
          CalcWidth = MIN_WIDTH
        Else
          CalcWidth = ScaleWidth
        End If
        If ScaleHeight < MIN_HEIGHT Then
          CalcHeight = MIN_HEIGHT
        Else
          CalcHeight = ScaleHeight
        End If

        'if the form is not visible or minimized
        If Not Visible Or Me.WindowState = vbMinimized Then
          Exit Sub
        End If

        'position propertybox
        SetPropertyBoxPos

        'if showing keyboard,
        If picKeyboard.Visible Then
          'move duration marker
          udDuration.Left = CalcWidth - udDuration.Width
          picDuration.Left = udDuration.Left - picDuration.Width
          picKeyboard.Width = picDuration.Left - picKeyboard.Left - 4

          udDuration.Top = CalcHeight - udDuration.Height
          picDuration.Top = CalcHeight - picDuration.Height
          'move keyboard and move scrollbar
          picKeyboard.Top = CalcHeight - picKeyboard.Height
          hsbKeyboard.Top = picKeyboard.Top

          SetKeyboardScroll

          'use keyboard height as an offset for drawing staves
          lngKybdHt = picKeyboard.Height
        End If

        'get number of staves that are visible
        lngStaffCount = -1 * (picStaffVis(0) + picStaffVis(1) + picStaffVis(2) + picStaffVis(3))

        'if nothing is visible
        If lngStaffCount = 0 Then
          hsbStaff.Visible = False
        Else

          'set bottom to CalcHeight - kybdht - scrollbarheight
          lngBottom = CalcHeight - lngKybdHt - hsbStaff.Height

          'reset horizontal scrollbar
          SetHScroll

          'if noise track is visible
          If picStaffVis(3) Then
            'if at least one other
            If lngStaffCount > 1 Then
              'if the staff fits when evenly split among others
              If lngBottom / lngStaffCount - SE_MARGIN > 52 * StaffScale Then
                'use minimum space for noise track
                picStaff(3).Move picStaff(3).Left, lngBottom - 52 * StaffScale - SE_MARGIN, CalcWidth - picStaff(i).Left, 54 * StaffScale
                'adjust bottom to top of noise track minus margin
                lngBottom = picStaff(3).Top - SE_MARGIN
                'adjust track Count so other staves are drawn properly
                lngStaffCount = lngStaffCount - 1
              Else
                'draw it at bottom, but evenly spaced
                picStaff(3).Move picStaff(3).Left, lngBottom - lngBottom / lngStaffCount + SE_MARGIN, CalcWidth - picStaff(i).Left, lngBottom / lngStaffCount - SE_MARGIN
              End If
            Else
              'fill entire window
                picStaff(3).Move picStaff(3).Left, 0, CalcWidth - picStaff(i).Left, lngBottom - SE_MARGIN
            End If
          End If

          'step through music staves,
          For i = 0 To 2
            'if this staff is visible,
            If picStaffVis(i) Then
              'move and resize (which forces update of
              'vertical scrollbars and redraws the staff)
              picStaff(i).Move picStaff(i).Left, j * lngBottom / lngStaffCount + SE_MARGIN, CalcWidth - picStaff(i).Left, lngBottom / lngStaffCount - SE_MARGIN
              j = j + 1
            End If
          Next i
        End If

        'draw all
        DrawStaff -1
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub Form_Unload(Cancel As Integer)

        Dim i As Long

        On Error GoTo ErrHandler

        'if unloading due to error on startup
        'soundedit will be set to nothing
        If Not SoundEdit Is Nothing Then
          'dereference Sound
          SoundEdit.Unload
          Set SoundEdit = Nothing
        End If

        'remove from Soundeditor collection
        For i = 1 To SoundEditors.Count
          If SoundEditors(i) Is Me Then
            SoundEditors.Remove i
            Exit For
          End If
        Next i

        'destroy undocol
        If UndoCol.Count > 0 Then
          For i = UndoCol.Count To 1 Step -1
            UndoCol.Remove i
          Next i
        End If
        Set UndoCol = Nothing

        'if midi is not disabled
        If Not Settings.NoMIDI Then
          KillMIDI
        End If

        'need to check if this is last form
        LastForm Me
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub picProperties_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'redraw to ensure button is correct
        PaintPropertyWindow
      End Sub

      Private Sub picStaff_DblClick(Index As Integer)

        'select the note being clicked;
        'use mouse down, then extend selection by 1 (move end pt)
        picStaff_MouseDown Index, 0, 0, mX, mY
        ChangeSelection SelTrack, SelStart, SelLength + 1, False, True, True '

      End Sub

      Private Sub picStaff_GotFocus(Index As Integer)

        PaintPropertyWindow False
      End Sub


      Private Sub picStaff_MouseDown(Index As Integer, Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim tmpPos As Long
        Dim tmpNode As Node

        On Error GoTo ErrHandler

        Select Case Button
        Case vbRightButton
          'if clicking on a different track
          If Index <> SelTrack Then
            'select track first
            picStaff_MouseDown Index, vbLeftButton, Shift, X, Y
          End If

          'make sure this form is the active form
          If Not (frmMDIMain.ActiveMdiChild Is Me) Then
            'set focus before showing the menu
            Me.SetFocus
          End If
          'need doevents so form activation occurs BEFORE popup
          'otherwise, errors will be generated because of menu
          'adjustments that are made in the form_activate event
          SafeDoEvents
          'context menu
          PopupMenu frmMDIMain.mnuEdit, 0, X + picStaff(Index).Left, Y + picStaff(Index).Top

        Case vbLeftButton

          'if clicking on clef area
          If X < KeyWidth Then
            'select the track
            tvwSound.Nodes(Index + 2).Selected = True
            tvwSound.SelectedItem.EnsureVisible
            ChangeSelection Index, 0, 0

          Else
            'if holding shift key AND same staff
            If Shift = vbShiftMask And Index = SelTrack Then
              'determine which note is being selected
              tmpPos = NoteFromPos(Index, X, True)
              If tmpPos > SelAnchor Then
                'extend/compress selection (move end pt)
                ChangeSelection SelTrack, SelAnchor, tmpPos - SelAnchor, False, True
              Else
                'extend/compress selection (move start pt)
                ChangeSelection SelTrack, tmpPos, SelAnchor - tmpPos
              End If
            Else
              'determine which note is being selected
              tmpPos = NoteFromPos(Index, X)
              ChangeSelection Index, tmpPos, 0
              'set anchor pos
              SelAnchor = SelStart
            End If
          End If

          'save mouse state for dbl click
          mButton = Button
          mShift = Shift
          mX = X
          mY = Y
        End Select
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub picStaff_MouseMove(Index As Integer, Button As Integer, Shift As Integer, X As Single, Y As Single)

        'determine is sellength needs to be changed,
        Dim tmpPos As Long, tmpSelStart As Long
        Dim tmpSelLength As Long
        Dim sngTime As Single

        'if not active form,
        If Not frmMDIMain.ActiveMdiChild Is Me Then
          Exit Sub
        End If

        'if no movement from starting position
        If Not tmrScroll.Enabled And mX = X And mY = Y Then
          'not really a mousemove
          Exit Sub
        End If


        Select Case Button
        Case vbLeftButton
          mX = X
          mY = Y

          'if no track, then exit
          If SelTrack = -1 Then
            Exit Sub
          End If

          'get note number under cursor
          tmpPos = NoteFromPos(SelTrack, X, True)

          'set sellength
          tmpSelLength = tmpPos - SelAnchor
          tmpSelStart = SelAnchor

          'if backwards
          If tmpSelLength < 0 Then
            'adjust so selstart is to left of selend
            tmpSelStart = tmpSelStart + tmpSelLength
            tmpSelLength = tmpSelLength * -1
          End If

          'if mouse position is off edge of screen,
          'enable autoscrolling
          If X < 0 Then
            tmrScroll.Enabled = True
            tmrScroll.Interval = 200 / ((-1 * X \ 10) + 1)
            lngScrollDir = 1
          ElseIf X > picStaff(Index).ScaleWidth + vsbStaff(Index).Visible * vsbStaff(Index).Width Then
            tmrScroll.Enabled = True
            tmrScroll.Interval = 200 / (((X - (picStaff(Index).ScaleWidth + vsbStaff(Index).Visible * vsbStaff(Index).Width)) \ 10) + 1)
            lngScrollDir = -1
          Else
            tmrScroll.Enabled = False
            lngScrollDir = 0
          End If

          'if NOT a change in selection
          If tmpSelLength = SelLength And tmpSelStart = SelStart Then
            Exit Sub
          Else
            'extend/compress; direction depends on anchor relation to start
            ChangeSelection SelTrack, tmpSelStart, tmpSelLength, False, (tmpSelStart = SelAnchor) '
          End If

        End Select

        'update time marker
        tmpPos = (5 + Abs(SoundEdit.Key)) * StaffScale * 6
        If tmpPos < 36 * StaffScale Then
          tmpPos = 36 * StaffScale
        End If

        sngTime = CSng((X - tmpPos - SOHorz)) / TICK_WIDTH / 60 / StaffScale
        If sngTime < 0 Then
          sngTime = 0
        End If

        If MainStatusBar.Tag <> CStr(AGIResType.Sound) Then
          AdjustMenus AGIResType.Sound, InGame, True, IsDirty
        End If
        MainStatusBar.Panels("Time").Text = "Pos: " & format$(sngTime, "0.00") & " sec"
      End Sub


      Private Sub picStaff_MouseUp(Index As Integer, Button As Integer, Shift As Integer, X As Single, Y As Single)

        'always disable autoscrolling
        tmrScroll.Enabled = False
      End Sub

      Private Sub picStaff_Resize(Index As Integer)

        Dim intMax As Integer
        Dim i As Integer

        'set vertical scrollbar
        SetVScroll Index, StaffScale

        'set horizontal scrollbar change values
        hsbStaff.SmallChange = (picStaff(0).Width - KeyWidth) * SM_SCROLL / TICK_WIDTH / StaffScale
        hsbStaff.LargeChange = (picStaff(0).Width - KeyWidth) * LG_SCROLL / TICK_WIDTH / StaffScale

        'redraw staff
        DrawStaff Index
      End Sub


      Private Sub SoundEdit_SoundComplete(NoError As Boolean)

        Dim rtn As Long

        'should never get here if midi is disabled
        '*'Debug.Assert Not Settings.NoMIDI

        'restore editor midi connection
        InitMIDI

        'restore menu caption & buttons
        cmdPlay.Enabled = Not Settings.NoMIDI
        cmdStop.Enabled = False
        frmMDIMain.mnuECustom1.Caption = "Play Sound" & vbTab & "Ctrl+Enter"
      End Sub

      Private Sub Timer1_Timer()
        'toggles the cursor in the appropriate staff window
        On Error GoTo ErrHandler

        If SelTrack = -1 Then
          Timer1.Enabled = False
          Exit Sub
        End If

        'draw the cursor line, using invert pen
        picStaff(SelTrack).DrawWidth = 2
        picStaff(SelTrack).DrawMode = 6 'invert
        picStaff(SelTrack).Line (CursorPos, 0)-(CursorPos, picStaff(SelTrack).ScaleHeight), vbBlack
        picStaff(SelTrack).DrawWidth = 1
        picStaff(SelTrack).DrawMode = 13 'copy pen

        'set cursor status
        CursorOn = Not CursorOn
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub tmrScroll_Timer()

        'autoscroll staves

        Select Case lngScrollDir
        Case 1  'scroll left
          'if already at left edge
          If SOHorz = 0 Then
            'disable autoscroll
            lngScrollDir = 0
            tmrScroll.Enabled = False
          Else
            'if there is room for a small change
            If hsbStaff.Value > hsbStaff.SmallChange Then
              hsbStaff.Value = hsbStaff.Value - hsbStaff.SmallChange
            Else
              hsbStaff.Value = 0
            End If
            'force change
            hsbStaff_Change
          End If

        Case -1 'scroll right
          'if already at right edge
          If hsbStaff.Value = hsbStaff.Max Then
            'disable autoscroll
            lngScrollDir = 0
            tmrScroll.Enabled = False
          Else
            'if there is room for a small change
            If hsbStaff.Value < hsbStaff.Max - hsbStaff.SmallChange Then
              hsbStaff.Value = hsbStaff.Value + hsbStaff.SmallChange
            Else
              hsbStaff.Value = hsbStaff.Max
            End If
          End If
        End Select

        'force mousemove event
        picStaff_MouseMove CInt(SelTrack), mButton, mShift, mX, mY
      End Sub

      Private Sub Toolbar1_ButtonClick(ByVal Button As MSComctlLib.Button)

        Select Case Button.Key
        Case "zi"  'zoom in
          ZoomScale 1
        Case "zo"  'zoom out
          ZoomScale -1

        Case "pa"
          MenuClickPaste

        Case "cu"
          MenuClickCut

        Case "co"
          MenuClickCopy

        Case "de"
          MenuClickDelete

        Case "su"
          ShiftTone 1

        Case "sd"
          ShiftTone -1

        Case "vu"
          ShiftVol 1

        Case "vd"
          ShiftVol -1
        End Select
      End Sub

      Private Sub tvwSound_DblClick()

        If tvwSound.HitTest(NodeX, NodeY) Is tvwSound.SelectedItem Then
        '*'Debug.Assert SelLength = 0
          ChangeSelection SelTrack, SelStart, 1
        End If
      End Sub

      Private Sub tvwSound_GotFocus()

        'force focus to property window
        On Error GoTo ErrHandler

        picProperties.SetFocus
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub tvwSound_KeyDown(KeyCode As Integer, Shift As Integer)

        'use form preview?
        Form_KeyDown KeyCode, Shift

      End Sub

      Private Sub tvwSound_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'save values so dbl-click can know where cursor is
        NodeX = X
        NodeY = Y

      End Sub

      Private Sub tvwSound_NodeClick(ByVal Node As MSComctlLib.Node)

        Dim tmpTrack As Long, tmpStart As Long, tmpLength As Long
        Dim i As Long

        On Error GoTo ErrHandler

        'unless a note node is clicked, selection wont change
        tmpTrack = SelTrack
        tmpStart = SelStart
        tmpLength = SelLength

        'set prop row Count
        Select Case Node.Index
        Case 1  'root
          PropRowCount = 5
          SetPropertyBoxPos

          'no track
          tmpTrack = -1

        Case 2 To 4 'sound tracks 0,1,2
          PropRowCount = 4
          SetPropertyBoxPos
          'set track
          tmpTrack = Node.Index - 2
          tmpStart = -1

        Case 5  'noise track
          PropRowCount = 3
          SetPropertyBoxPos
          'set track
          tmpTrack = 3
          tmpStart = -1

        Case Else
          If Node.Parent.Index = 5 Then
            PropRowCount = 4
            SetPropertyBoxPos
          Else
            PropRowCount = 6
            SetPropertyBoxPos
          End If

          'set track
          tmpTrack = Node.Parent.Index - 2
          'set selection start
          tmpStart = tvwSound.SelectedItem.Tag
          SelAnchor = tmpStart
          If tvwSound.SelectedItem.Text = "End" Or Not picStaffVis(tmpTrack) Then
            tmpLength = 0
          Else
            'select the note? or set cursor here? not sure which I want here
            tmpLength = 1
            tmpLength = 0
          End If
          'PaintPropertyWindow
        End Select

        'special case to check for 'end'
        If tvwSound.SelectedItem.Text = "End" Then
          PropRowCount = 0
        End If

        ChangeSelection tmpTrack, tmpStart, tmpLength

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub txtProperty_Change()

        'need to manually replace any crlf's
        If Right$(txtProperty.Text, 2) = vbNewLine Then
          txtProperty.Text = Left$(txtProperty.Text, Len(txtProperty.Text) - 2)
          txtProperty.SelStart = Len(txtProperty.Text)
        End If
      End Sub

      Private Sub txtProperty_GotFocus()

        'refresh properties
        '(why do this for gotfocus?
        picProperties.Refresh
      End Sub

      Private Sub txtProperty_KeyPress(KeyAscii As Integer)

        'trap enter key
        Select Case KeyAscii
        Case 10 'ctrl-enter key combination
          'if not description
          If txtProperty.Tag <> "DESC" Then
            'select new prop value
            KeyAscii = 0
            SelectPropFromText
            Exit Sub
          End If

        Case 13 'enter key
          'select new prop value
          KeyAscii = 0
          SelectPropFromText
          Exit Sub

        Case 27 'esc key
          'cancel - just hide the text box
          txtProperty.Visible = False
          KeyAscii = 0
          Exit Sub
        End Select

        'tqpn, duration, attenuation, freqdiv only accept numbers
        If txtProperty.Tag = "TPQN" Or txtProperty.Tag = "DUR" Or txtProperty.Tag = "ATT" Or txtProperty.Tag = "FREQ" Then
          'only accept numbers, backspace, delete

          Select Case KeyAscii
          Case 48 To 57
            'if no selection
            If txtProperty.SelLength = 0 Then
              'limit text length: TPQN and ATT - 2 digits
              '                   FREQ - 4 digits
              If ((txtProperty.Tag = "TPQN" Or txtProperty.Tag = "ATT") And Len(txtProperty.Text) >= 2) Or _
                 (txtProperty.Tag = "FREQ" And Len(txtProperty.Text) >= 4) Then
                KeyAscii = 0
              End If
            End If

          Case 8 'backspace always ok
          Case Else
            'ignore all other keys
            KeyAscii = 0
          End Select
        End If
      End Sub

      Private Sub txtProperty_LostFocus()

        On Error GoTo ErrHandler

        'if not already hidden
        If txtProperty.Visible Then
          'hide it
          txtProperty.Visible = False
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub PaintPropertyWindow(Optional ByVal Highlight As Boolean = True)

        Dim i As Long, strProp As String
        Dim rtn As Long, lngRows As Long
        Dim lngPosY As Long

        On Error GoTo ErrHandler

      '  SendMessage picProperties.hWnd, WM_SETREDRAW, 0, 0
        picProperties.Cls

        'draw property header cells
        picProperties.Line (1, 1)-(PropSplitLoc - 1, PropRowHeight - 2), RGB(236, 233, 216), BF
        picProperties.Line (PropSplitLoc + 2, 1)-(picProperties.Width - 2, PropRowHeight - 2), DKGray, BF
        picProperties.Line (1, PropRowHeight - 1)-(PropSplitLoc, PropRowHeight - 1), vbBlack
        picProperties.Line (PropSplitLoc, 0)-(PropSplitLoc, PropRowHeight - 1), vbBlack
        picProperties.Line (PropSplitLoc + 2, PropRowHeight - 1)-(picProperties.Width - 1, PropRowHeight - 1), vbBlack
        picProperties.ForeColor = vbBlack
        picProperties.CurrentX = 3
        picProperties.CurrentY = 1
        picProperties.Print "Property"
        picProperties.CurrentX = PropSplitLoc + 4
        picProperties.CurrentY = 1
        picProperties.Print "Value"

        'draw properties if something is selected
        '(use a do loop so we can exit the draw prop
        'activity if nothing needs to be drawn)
        Do While Not (tvwSound.SelectedItem Is Nothing)

          'if displaying root
          Select Case tvwSound.SelectedItem.Index
          Case 1  'root
            'ID, Description, Key, TPQN, LENGTH

            'id enabled only if sound is in a game
            If InGame Then
              DrawProp picProperties, "ID", SoundEdit.ID, 1, Highlight, SelectedProp, 0, True, bfDialog
            Else
              DrawProp picProperties, "ID", SoundEdit.ID, 1, Highlight, SelectedProp, 0, False
            End If
            DrawProp picProperties, "Description", SoundEdit.Description, 2, Highlight, SelectedProp, 0, True, bfDialog
            DrawProp picProperties, "Key", LoadResString(KEYSIGNATURE + SoundEdit.Key + 7), 3, Highlight, SelectedProp, 0, True, bfDown
            DrawProp picProperties, "TPQN", SoundEdit.TPQN, 4, Highlight, SelectedProp, 0, True
            DrawProp picProperties, "Length", format$(SoundEdit.Length, "0.00"), 5, Highlight, SelectedProp, 0, True

          Case 2 To 4 'sound tracks
            'instrument, muted, note Count
            DrawProp picProperties, "Instrument", LoadResString(INSTRUMENTNAMETEXT + SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Instrument), 1, Highlight, SelectedProp, 0, True, bfDown
            DrawProp picProperties, "Muted", CBool(SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Muted), 2, Highlight, SelectedProp, 0, True, bfDown
      '      DrawProp picProperties, "Note Count", SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Notes.Count, 3, Highlight, SelectedProp, 0, True
            DrawProp picProperties, "Note Count", SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Notes.Count, 3, False, SelectedProp, 0, False
            'visible available only if NOT in onetrack mode
            DrawProp picProperties, "Visible", SoundEdit.Track(tvwSound.SelectedItem.Index - 2).Visible, 4, Highlight And Not OneTrack, SelectedProp, 0, Not OneTrack, bfDown

          Case 5  'noise track
            'muted and Count
            DrawProp picProperties, "Muted", CBool(SoundEdit.Track(3).Muted), 1, Highlight, SelectedProp, 0, True, bfDown
            DrawProp picProperties, "Note Count", SoundEdit.Track(3).Notes.Count, 2, False, SelectedProp, 0, False
            'visible available only if NOT in onetrack mode
            DrawProp picProperties, "Visible", SoundEdit.Track(3).Visible, 3, Highlight And Not OneTrack, SelectedProp, 0, Not OneTrack, bfDown

          Case Else ' a note
            'if end,
            If tvwSound.SelectedItem.Text = "End" Then
              Exit Do
            End If

            'get note index
            i = CLng(tvwSound.SelectedItem.Tag)
            If tvwSound.SelectedItem.Parent.Index = 5 Then
              'noise note: Type, Freq, duration, attenuation
              DrawProp picProperties, "Type", IIf((SoundEdit.Track(3).Notes(i).FreqDivisor And 4) = 4, "White Noise", "Periodic"), 1, Highlight, SelectedProp, 0, True, bfDown
              Select Case SoundEdit.Track(3).Notes(i).FreqDivisor And 3
              Case 0
                strProp = "2330 Hz"
              Case 1
                strProp = "1165 Hz"
              Case 2
                strProp = "583 Hz"
              Case 3
                strProp = "Track 2"
              End Select
              DrawProp picProperties, "Freq Div", strProp, 2, Highlight, SelectedProp, 0, True, bfDown
              DrawProp picProperties, "Duration", SoundEdit.Track(3).Notes(i).Duration, 3, Highlight, SelectedProp, 0, True
              DrawProp picProperties, "Attenuation", SoundEdit.Track(3).Notes(i).Attenuation, 4, Highlight, SelectedProp, 0, True

            Else
              'sound note: Freq Div, Duration, Attenuation, MIDI Note, MIDI Octave
              DrawProp picProperties, "Freq Div", SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(i).FreqDivisor, 1, Highlight, SelectedProp, 0, True
              DrawProp picProperties, "Duration", SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(i).Duration, 2, Highlight, SelectedProp, 0, True
              DrawProp picProperties, "Attenuation", SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(i).Attenuation, 3, Highlight, SelectedProp, 0, True
              DrawProp picProperties, "MIDI Note", NoteName(MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(i).FreqDivisor), SoundEdit.Key), 4, Highlight, SelectedProp, 0, True, bfDown
              DrawProp picProperties, "MIDI Octave", MIDINote(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(i).FreqDivisor) \ 12, 5, Highlight, SelectedProp, 0, True, bfDown
              DrawProp picProperties, "MIDI Length", MIDILength(SoundEdit.Track(tvwSound.SelectedItem.Parent.Index - 2).Notes(i).Duration, SoundEdit.TPQN), 6, Highlight, SelectedProp, 0, True, bfDown
            End If
          End Select
          Exit Do
        Loop

        picProperties.Line (picProperties.Width - 1, 0)-(picProperties.Width - 1, PropRowHeight - 1), vbBlack
        'draw vertical line separating columns
        picProperties.Line (PropSplitLoc, PropRowHeight)-(PropSplitLoc, picProperties.Height - 1), LtGray
        picProperties.Line (picProperties.Width - 1, PropRowHeight)-(picProperties.Width - 1, picProperties.Height - 1), LtGray
        'draw horizontal lines separating rows
        lngRows = (picProperties.Height - 1) / PropRowHeight
        For i = 2 To lngRows
          picProperties.Line (0, i * PropRowHeight - 1)-(picProperties.Width - 1, i * PropRowHeight - 1), LtGray
        Next i

        'if nothing selected
        If (tvwSound.SelectedItem Is Nothing) Then
          'set proprowcount to zero
          PropRowCount = 0
        Else
          'if end marker is selected
          If tvwSound.SelectedItem.Text = "End" Then
            PropRowCount = 0
          End If
        End If

      '  SendMessage picProperties.hWnd, WM_SETREDRAW, 1, 0
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub udDuration_Change()

        DefLength = udDuration.Value

        ShowDefaultDuration
        picKeyboard.SetFocus
      End Sub

      Private Sub udDuration_GotFocus()

        picKeyboard.SetFocus
      End Sub


      Private Sub vsbStaff_Change(Index As Integer)

        If Not blnDontDraw Then
          'set new offset
          SOVert(Index) = -vsbStaff(Index).Value

          'if staff was selected, easiest thing to do is use changeselection
          If Index = SelTrack Then
            ChangeSelection SelTrack, SelStart, SelLength, True, False, True
          Else
            'use draw staff method
            DrawStaff Index
          End If
        End If

      End Sub


      Private Sub vsbStaff_GotFocus(Index As Integer)

        'scoll bars don't keep focus; pass back to the staff
        picStaff(Index).SetFocus
      End Sub


            */
        }
    }
}
