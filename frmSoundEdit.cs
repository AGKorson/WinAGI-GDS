using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Globalization;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Sound;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.SoundUndo.SoundUndoType;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Text;
using WinAGI.Common;

namespace WinAGI.Editor {
    public partial class frmSoundEdit : ClipboardMonitor {
        #region Enums
        public enum SelectionModeType {
            Sound = 0,
            MusicTrack = 1,
            NoiseTrack = 2,
            MusicNote = 3,
            NoiseNote = 4,
            EndNote = 5,
        }
        #endregion

        #region Constants
        private const float TICK_WIDTH = 5.625F; // width of a tick, in pixels, at scale of 1 (60 ticks per second)
        private const int SE_MARGIN = 5; // in pixels
        private const int MinNoiseKeyWidth = 40;
        #endregion

        #region Members
        public int SoundNumber;
        public Sound EditSound;
        internal bool InGame;
        internal bool IsChanged;
        private SelectablePictureBox[] picStaff = new SelectablePictureBox[4];
        private VScrollBar[] vsbStaff = new VScrollBar[4];
        private Button[] btnMute = new Button[4];
        private ToolTip defaultNoteTip = new();
        private Font staffFont;
        internal MidiNotePlayer midiNotePlayer = new();
        internal WavNotePlayer wavNotePlayer = new();

        private int KeyWidth;
        private int StaffScale;
        private int MKbOffset = 45, NKbOffset = 0;

        private SoundPlaybackMode PlaybackMode;
        internal EGAColors EditPalette = DefaultPalette.Clone();
        private Stack<SoundUndo> UndoCol = [];

        // variables for managing staves
        private int StaffCount; // number of staves that are visible
        private int SOHorz;
        private int[] SOVert = new int[4];
        private int[] oldstaffH = new int[4];
        private bool OneTrack, KeyboardVisible, KeyboardSound;
        private bool ShowNotes;
        // default note properties
        private int DefLength, DefAttn, DefOctave;
        private bool DefMute;

        // selection variables
        private SelectionModeType SelectionMode;
        public int SelectedTrack, SelStart;
        private int SelAnchor, SelLength;
        // SelAnchor is either SelStart, or SelStart+SelLength, depending
        // on whether selection is right-to-left, or left-to-right
        // cursor control
        private bool CursorOn = false;
        int mX = 0, mY = 0;
        int StaffScrollDir = 0;
        private bool PlayingSound = false;
        private bool blnNoteOn = false;
        private int PlayNote = 0;

        // StatusStrip Items
        internal ToolStripStatusLabel spScale;
        internal ToolStripStatusLabel spTime;
        internal ToolStripStatusLabel spStatus;
        #endregion

        public frmSoundEdit() {
            InitializeComponent();
            InitStatusStrip();
            MdiParent = MDIMain;
            // set up staff picture boxes
            picStaff[0] = picStaff0;
            picStaff[1] = picStaff1;
            picStaff[2] = picStaff2;
            picStaff[3] = picStaff3;
            vsbStaff[0] = vsbStaff0;
            vsbStaff[1] = vsbStaff1;
            vsbStaff[2] = vsbStaff2;
            vsbStaff[3] = vsbStaff3;
            btnMute[0] = btnMute0;
            btnMute[1] = btnMute1;
            btnMute[2] = btnMute2;
            btnMute[3] = btnMute3;
            for (int i = 0; i < 4; i++) {
                picStaff[i].MouseWheel += picStaff_MouseWheel;
                picStaff[i].KeyDown += picStaff_KeyDown;
                picStaff[i].KeyPress += picStaff_KeyPress;
                picStaff[i].KeyUp += picStaff_KeyUp;
                picStaff[i].Tag = i;
                vsbStaff[i].Tag = i;
                btnMute[i].Tag = i;
                picStaff[i].Controls.Add(btnMute[i]);
                picStaff[i].Controls.Add(vsbStaff[i]);
                vsbStaff[i].Dock = DockStyle.Right;
            }
            picKeyboard.MouseWheel += picKeyboard_MouseWheel;
            picDuration.MouseWheel += picDuration_MouseWheel;
            defaultNoteTip.AutoPopDelay = 2500;
            defaultNoteTip.InitialDelay = 500;
            defaultNoteTip.ReshowDelay = 100;
            defaultNoteTip.ShowAlways = true;
            defaultNoteTip.UseAnimation = true;
            defaultNoteTip.UseFading = true;
            defaultNoteTip.SetToolTip(picDuration, "1/4, Attn: 0");
            // initialize midi keyboard player
            midiNotePlayer.InitMidi();
        }

        #region Form Event Handlers
        protected override void OnClipboardChanged() {
            base.OnClipboardChanged();
            tsbPaste.Enabled = CanPaste(SelectedTrack);
        }

        private void frmSoundEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            bool closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmSoundEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // dereference Sound
            EditSound?.Unload();
            EditSound = null;
            // remove from SoundEditor collection
            foreach (frmSoundEdit frm in SoundEditors) {
                if (frm == this) {
                    SoundEditors.Remove(frm);
                    break;
                }
            }
            picKeyboard.MouseWheel -= picKeyboard_MouseWheel;
            picDuration.MouseWheel -= picDuration_MouseWheel;
            for (int i = 0; i < 4; i++) {
                picStaff[i].MouseWheel -= picStaff_MouseWheel;
                picStaff[i].KeyDown -= picStaff_KeyDown;
                picStaff[i].KeyPress -= picStaff_KeyPress;
                picStaff[i].KeyUp -= picStaff_KeyUp;
                picStaff[i].Controls.Remove(vsbStaff[i]);
                picStaff[i].Controls.Remove(btnMute[i]);
            }
            // done with midi keyboard player
            midiNotePlayer.KillMidi();
            midiNotePlayer.Dispose();
            wavNotePlayer.Dispose();
        }

        private void frmSoundEdit_Leave(object sender, EventArgs e) {
            // when losing focus, disable midi keyboard playback
            midiNotePlayer.KillMidi();
            // also stop sound playback if needed
            if (PlayingSound) {
                StopSound();
            }
        }

        private void frmSoundEdit_Enter(object sender, EventArgs e) {
            // when gaining focus, enable midi keyboard playback
            midiNotePlayer.InitMidi();
        }

        private void This_SoundComplete(object sender, SoundCompleteEventArgs e) {
            StopSound();
        }
        #endregion

        #region Menu Event Handlers
        internal void SetResourceMenu() {

            mnuRSave.Enabled = IsChanged;
            MDIMain.mnuRSep2.Visible = true;
            MDIMain.mnuRSep3.Visible = true;
            if (EditGame is null) {
                // no game is open
                MDIMain.mnuRImport.Enabled = false;
                mnuRExport.Text = "Save As ...";
                mnuRInGame.Enabled = false;
                mnuRInGame.Text = "Add Sound to Game";
                mnuRRenumber.Enabled = false;
                // mnuRProperties no change
            }
            else {
                // if a game is loaded, base import is also always available
                MDIMain.mnuRImport.Enabled = true;
                mnuRExport.Text = InGame ? "Export Sound" : "Save As ...";
                mnuRInGame.Enabled = true;
                mnuRInGame.Text = InGame ? "Remove from Game" : "Add to Game";
                mnuRRenumber.Enabled = InGame;
                // mnuRProperties no change
                mnuRPlaySound.Text = btnPlay.Enabled ? "Play Sound" : "Stop Sound";
                mnuPCSpeaker.Checked = false;
                mnuPCjr.Checked = false;
                mnuMIDI.Checked = false;
                switch (PlaybackMode) {
                case SoundPlaybackMode.PCSpeaker:
                    mnuPCSpeaker.Checked = true;
                    break;
                case SoundPlaybackMode.WAV:
                    mnuPCjr.Checked = true;
                    break;
                case SoundPlaybackMode.MIDI:
                    mnuMIDI.Checked = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Resets all resource menu items so shortcut keys can work correctly.
        /// </summary>
        internal void ResetResourceMenu() {
            mnuRSave.Enabled = true;
            mnuRExport.Enabled = true;
            mnuRInGame.Enabled = true;
            mnuRRenumber.Enabled = true;
            mnuRProperties.Enabled = true;
            mnuRPlaySound.Enabled = true;
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            if (IsChanged) {
                SaveSound();
            }
        }

        internal void mnuRExport_Click(object sender, EventArgs e) {
            ExportSound();
        }

        internal void mnuRInGame_Click(object sender, EventArgs e) {
            if (EditGame is not null) {
                ToggleInGame();
            }
        }

        private void mnuRRenumber_Click(object sender, EventArgs e) {
            if (InGame) {
                RenumberSound();
            }
        }

        private void mnuRProperties_Click(object sender, EventArgs e) {
            EditSoundProperties(1);
        }

        private void mnuRPlaySound_Click(object sender, EventArgs e) {
            if (btnPlay.Enabled && CanPlay()) {
                // play sound
                PlaySound();
            }
            else {
                // stop sound
                StopSound();
            }
        }

        private void mnuPCSpeaker_Click(object sender, EventArgs e) {
            MessageBox.Show("PC Speaker playback is not yet implemented.", "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void mnuPCjr_Click(object sender, EventArgs e) {
            if (PlaybackMode != SoundPlaybackMode.WAV) {
                PlaybackMode = SoundPlaybackMode.WAV;
            }
        }

        private void mnuMIDI_Click(object sender, EventArgs e) {
            if (PlaybackMode != SoundPlaybackMode.MIDI) {
                PlaybackMode = SoundPlaybackMode.MIDI;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            SetEditMenu();
        }

        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            mnuEdit.DropDownItems.AddRange([mnuUndo, toolStripSeparator1, mnuCut,
                mnuCopy, mnuPaste, mnuDelete, mnuSelectAll, toolStripSeparator2,
                mnuToneUp, mnuToneDown, mnuVolumeUp, mnuVolumeDown,
                toolStripSeparator3, mnuKeyboard, mnuNoKybdSound, mnuOneTrack]);
            SetEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            contextMenuStrip1.Items.AddRange([mnuUndo, toolStripSeparator1, mnuCut,
                mnuCopy, mnuPaste, mnuDelete, mnuSelectAll, toolStripSeparator2,
                mnuToneUp, mnuToneDown, mnuVolumeUp, mnuVolumeDown,
                toolStripSeparator3, mnuKeyboard, mnuNoKybdSound, mnuOneTrack]);
            ResetEditMenu();
        }

        private void SetEditMenu() {
            // set edit menu items based on current selection
            mnuUndo.Enabled = UndoCol.Count > 0;
            if (mnuUndo.Enabled) {
                mnuUndo.Text = "Undo " + Editor.Base.EditorResourceByNum(SNDUNDOTEXT + (int)UndoCol.Peek().UDAction);
            }
            else {
                mnuUndo.Text = "Undo";
            }
            mnuCut.Enabled = SelLength > 0;
            mnuCopy.Enabled = SelLength > 0;
            mnuPaste.Enabled = CanPaste(SelectedTrack);
            mnuDelete.Enabled = SelectedTrack != -1 && SelStart != EditSound[SelectedTrack].Notes.Count - 1;
            switch (SelectionMode) {
            case SelectionModeType.Sound:
                mnuClear.Visible = true;
                mnuClear.Text = "Clear Sound";
                mnuClear.Enabled = true;
                break;
            case SelectionModeType.MusicTrack:
            case SelectionModeType.NoiseTrack:
                mnuClear.Visible = true;
                mnuClear.Text = "Clear Track";
                mnuClear.Enabled = SelectedTrack != -1 && EditSound[SelectedTrack].Notes.Count > 0;
                break;
            default:
                mnuClear.Visible = false;
                break;
            }
            mnuSelectAll.Enabled = SelectedTrack != -1 && EditSound[SelectedTrack].Notes.Count > 0;
            // for note tone and volume shift, allow if music track is selected,
            // or if a music note is selected and length > 0
            if (SelectionMode == SelectionModeType.MusicTrack) {
                mnuToneUp.Enabled = true;
                mnuToneDown.Enabled = true;
                mnuVolumeUp.Enabled = true;
                mnuVolumeDown.Enabled = true;
            }
            else if (SelectionMode == SelectionModeType.MusicNote && SelLength > 0) {
                mnuToneUp.Enabled = true;
                mnuToneDown.Enabled = true;
                mnuVolumeUp.Enabled = true;
                mnuVolumeDown.Enabled = true;
            }
            else {
                mnuToneUp.Enabled = false;
                mnuToneDown.Enabled = false;
                mnuVolumeUp.Enabled = false;
                mnuVolumeDown.Enabled = false;
            }
            mnuToneUp.Enabled = SelectedTrack != -1 && SelectedTrack != 3 &&
                (EditSound[SelectedTrack].Notes.Count > 0 || SelectionMode == SelectionModeType.MusicTrack);
            mnuToneDown.Enabled = mnuToneUp.Enabled;
            mnuVolumeUp.Enabled = mnuToneUp.Enabled;
            mnuVolumeDown.Enabled = mnuToneUp.Enabled;
            mnuKeyboard.Text = KeyboardVisible ? "Hide Keyboard" : "Show Keyboard";
            mnuNoKybdSound.Enabled = KeyboardVisible;
            mnuNoKybdSound.Checked = KeyboardSound;
            mnuOneTrack.Text = OneTrack ? "Show All Visible Tracks" : "Show Only Selected Track";
        }

        private void ResetEditMenu() {
            mnuUndo.Enabled = true;
            mnuCut.Enabled = true;
            mnuCopy.Enabled = true;
            mnuPaste.Enabled = true;
            mnuDelete.Enabled = true;
            mnuClear.Enabled = true;
            mnuSelectAll.Enabled = true;
            mnuToneUp.Enabled = true;
            mnuToneDown.Enabled = true;
            mnuVolumeUp.Enabled = true;
            mnuVolumeDown.Enabled = true;
            mnuKeyboard.Enabled = true;
            mnuNoKybdSound.Enabled = true;
            mnuOneTrack.Enabled = true;
        }

        private void mnuUndo_Click(object sender, EventArgs e) {
            if (UndoCol.Count == 0) {
                return;
            }

            // save previous track so keyboard can
            // be correctly drawn if changing to/from
            // Noise track as a result of Undo action
            int PrevTrack = SelectedTrack;

            SoundUndo NextUndo = UndoCol.Pop();
            // undo the action
            switch (NextUndo.UDAction) {
            case EditNoteFreqDiv:
                SetNoteFreqDivisor(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDData, true);
                break;
            case EditNoteDuration:
                SetNoteDuration(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDData, true);
                break;
            case EditNoteAttenuation:
                SetNoteAttenuation(NextUndo.UDTrack, NextUndo.UDStart, (byte)NextUndo.UDData, true);
                break;
            case AddNote:
            case Paste:
                // remove note
                DeleteNotes(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDLength, true);
                // check for replace
                if (NextUndo.UDText == "R") {
                    NextUndo = UndoCol.Pop();
                    AddNotes(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDNotes, true, true);
                    SelectNote(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDStart, NextUndo.UDNotes.Count);
                }
                else {
                    SelectNote(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDStart, 0);
                }
                break;
            case Delete:
            case Cut:
                // add notes back
                AddNotes(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDNotes, true, true);
                SelectNote(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDStart, NextUndo.UDNotes.Count);
                break;
            case ChangeKey:
                SetSoundKey(NextUndo.UDData, true);
                break;
            case ChangeTPQN:
                SetSoundTPQN(NextUndo.UDData, true);
                break;
            case ChangeInstrument:
                SetTrackInstrument(NextUndo.UDTrack, (byte)NextUndo.UDData, true);
                break;
            case ShiftTone:
                for (int i = 0; i < NextUndo.UDLength; i++) {
                    EditSound[NextUndo.UDTrack][NextUndo.UDStart + i].FreqDivisor = NextUndo.UDNotes[i].FreqDivisor;
                }
                // update selection
                SelectNote(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDStart, NextUndo.UDLength);
                break;
            case ShiftVol:
                for (int i = 0; i < NextUndo.UDLength; i++) {
                    EditSound[NextUndo.UDTrack][NextUndo.UDStart + i].Attenuation = NextUndo.UDNotes[i].Attenuation;
                }
                // update selection
                propertyGrid1.Refresh();
                SelectNote(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDStart, NextUndo.UDLength);
                break;
            case ClearTrack:
                for (int i = 0; i < NextUndo.UDNotes.Count; i++) {
                    EditSound[NextUndo.UDTrack].Notes.Add(NextUndo.UDNotes[i].FreqDivisor, NextUndo.UDNotes[i].Duration, NextUndo.UDNotes[i].Attenuation);
                    tvwSound.Nodes[0].Nodes[NextUndo.UDTrack].Nodes.Insert(i, "Note " + i);
                }
                SelectTrack(NextUndo.UDTrack);
                break;
            case ClearSound:
                EditSound = NextUndo.UDSound.Clone();
                for (int i = 0; i < 4; i++) {
                    tvwSound.Nodes[0].Nodes[i].Nodes.Clear();
                    tvwSound.Nodes[0].Nodes[i].Nodes.Add("End");
                    for (int j = 0; j < EditSound[i].Notes.Count; j++) {
                        tvwSound.Nodes[0].Nodes[i].Nodes.Insert(j, "Note " + j);
                    }
                }
                SelectSound();
                // redraw all staves
                for (int i = 0; i < 4; i++) {
                    if (picStaff[i].Visible) {
                        picStaff[i].Invalidate();
                    }
                }
                break;
            }
        }

        private void mnuCut_Click(object sender, EventArgs e) {
            if (SelectedTrack >= 0 && SelStart >= 0 && SelLength > 0) {
                // copy notes to clipboard
                CopySelection();
                // then delete them
                DeleteSelection();
                // update clipboard action
                UndoCol.Peek().UDAction = Cut;
            }
        }

        private void mnuCopy_Click(object sender, EventArgs e) {
            if (SelectedTrack >= 0 && SelStart >= 0 && SelLength > 0) {
                // copy notes to clipboard
                CopySelection();
            }
        }

        private void mnuPaste_Click(object sender, EventArgs e) {
            if (CanPaste(SelectedTrack)) {
                bool blnReplace = false;

                // if a selection needs to be deleted
                if (SelLength > 0) {
                    // delete selection first
                    DeleteNotes(SelectedTrack, SelStart, SelLength);
                    blnReplace = true;
                }

                // add notes from clipboard
                SoundClipboardData soundCBData = Clipboard.GetData(SOUND_CB_FMT) as SoundClipboardData;
                Notes AddNoteCol = new Notes();
                for (int i = 0; i < soundCBData.Notes.Count; i++) {
                    AddNoteCol.Add(soundCBData.Notes[i].FreqDivisor, soundCBData.Notes[i].Duration, soundCBData.Notes[i].Attenuation);
                }
                AddNotes(SelectedTrack, SelStart, AddNoteCol, false);

                // adjust undo so it displays 'add note'
                UndoCol.Peek().UDAction = Paste;
                // if replacing,
                if (blnReplace) {
                    // set flag in undo item
                    UndoCol.Peek().UDText = "R";
                }
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e) {
            // if a track is selected, and insertion point is visible
            if (SelectedTrack != -1 && SelStart >= 0) {
                // delete
                DeleteSelection();
            }
        }

        private void mnuClear_Click(object sender, EventArgs e) {
            SoundUndo NextUndo;
            switch (SelectionMode) {
            case SelectionModeType.Sound:
                // clear sound
                NextUndo = new SoundUndo {
                    UDAction = ClearSound,
                    UDTrack = -1, // no track
                    UDSound = EditSound
                };
                AddUndo(NextUndo);
                EditSound.Clear();
                for (int i = 0; i < 4; i++) {
                    tvwSound.Nodes[0].Nodes[i].Nodes.Clear();
                    tvwSound.Nodes[0].Nodes[i].Nodes.Add("End");
                }
                SelectSound();
                // redraw all staves
                for (int i = 0; i < 4; i++) {
                    if (picStaff[i].Visible) {
                        picStaff[i].Invalidate();
                    }
                }
                break;
            case SelectionModeType.MusicTrack:
                // clear all notes form selected track
                if (SelectedTrack >= 0 && EditSound[SelectedTrack].Notes.Count > 0) {
                    // add undo item
                    NextUndo = new SoundUndo();
                    NextUndo.UDAction = ClearTrack;
                    NextUndo.UDTrack = SelectedTrack;
                    NextUndo.UDNotes = EditSound[SelectedTrack].Notes;
                    AddUndo(NextUndo);
                    // clear track
                    EditSound[SelectedTrack].Notes.Clear();
                    tvwSound.Nodes[0].Nodes[SelectedTrack].Nodes.Clear();
                    tvwSound.Nodes[0].Nodes[SelectedTrack].Nodes.Add("End");
                    SelectTrack(SelectedTrack);
                    // redraw staff
                    if (picStaff[SelectedTrack].Visible) {
                        picStaff[SelectedTrack].Invalidate();
                    }
                }
                break;
            }
        }

        private void mnuSelectAll_Click(object sender, EventArgs e) {
            if (SelectedTrack >= 0 && EditSound[SelectedTrack].Notes.Count > 0) {
                tvwSound.SelectedNodes = tvwSound.Nodes[0].Nodes[SelectedTrack].Nodes.Cast<TreeNode>().ToList().GetRange(0, tvwSound.Nodes[0].Nodes[SelectedTrack].Nodes.Count - 1);
                SelectNote(SelectedTrack, 0, 0, tvwSound.Nodes[0].Nodes[SelectedTrack].Nodes.Count - 1);
                if (picStaff[SelectedTrack].Visible) {
                    picStaff[SelectedTrack].Invalidate();
                }
            }
        }

        private void mnuToneUp_Click(object sender, EventArgs e) {
            if (SelectionMode == SelectionModeType.MusicTrack) {
                // shift all notes in track up one note
                ShiftNoteTones(1, SelectedTrack, 0, EditSound[SelectedTrack].Notes.Count);
            }
            else if (SelectionMode == SelectionModeType.MusicNote && SelLength > 0) {
                // shift selected notes up one note
                ShiftNoteTones(1, SelectedTrack, SelStart, SelLength);
            }
        }

        private void mnuToneDown_Click(object sender, EventArgs e) {
            if (SelectionMode == SelectionModeType.MusicTrack) {
                // shift all notes in track down one note
                ShiftNoteTones(-1, SelectedTrack, 0, EditSound[SelectedTrack].Notes.Count);
            }
            else if (SelectionMode == SelectionModeType.MusicNote && SelLength > 0) {
                // shift selected notes down one note
                ShiftNoteTones(-1, SelectedTrack, SelStart, SelLength);
            }
        }

        private void mnuVolumeUp_Click(object sender, EventArgs e) {
            if (SelectionMode == SelectionModeType.MusicTrack) {
                // shift all notes in track up one volume
                ShiftNoteVol(1, SelectedTrack, 0, EditSound[SelectedTrack].Notes.Count);
            }
            else if (SelectionMode == SelectionModeType.MusicNote && SelLength > 0) {
                // shift selected notes up one volume
                ShiftNoteVol(1, SelectedTrack, SelStart, SelLength);
            }
        }

        private void mnuVolumeDown_Click(object sender, EventArgs e) {
            if (SelectionMode == SelectionModeType.MusicTrack) {
                // shift all notes in track down one volume
                ShiftNoteVol(-1, SelectedTrack, 0, EditSound[SelectedTrack].Notes.Count);
            }
            else if (SelectionMode == SelectionModeType.MusicNote && SelLength > 0) {
                // shift selected notes down one volume
                ShiftNoteVol(-1, SelectedTrack, SelStart, SelLength);
            }
        }

        private void mnuKeyboard_Click(object sender, EventArgs e) {
            KeyboardVisible = !KeyboardVisible;
            splitContainer2.Panel2Collapsed = !splitContainer2.Panel2Collapsed;
        }

        private void mnuNoKybdSound_Click(object sender, EventArgs e) {
            if (KeyboardVisible) {
                KeyboardSound = !KeyboardSound;
            }
        }

        private void mnuOneTrack_Click(object sender, EventArgs e) {
            OneTrack = !OneTrack;
            UpdateVisibleStaves(StaffScale);
        }
        #endregion

        #region ToolStrip Event Handlers
        public void ConfigureToolbar() {
            // cut, copy enabled if notes are selected
            tsbCut.Enabled = (SelectedTrack != -1 && SelStart >= 0 && SelLength > 0);
            tsbCopy.Enabled = tsbCut.Enabled;
            // paste handled by clipboard monitor, but refresh it in case selected track
            // has changed
            tsbPaste.Enabled = CanPaste(SelectedTrack);
            // delete enabled if a track is selected, and not at end of track
            tsbDelete.Enabled = SelectedTrack != -1 && SelStart != EditSound[SelectedTrack].Notes.Count - 1;

            // for tone and volume shifts, allow if track is selected,
            // or if a note is selected and length > 0
            if (SelectionMode == SelectionModeType.MusicTrack) {
                tsbToneUp.Enabled = true;
                tsbToneDown.Enabled = true;
                tsbVolumeUp.Enabled = true;
                tsbVolumeDown.Enabled = true;
            }
            else if (SelectionMode == SelectionModeType.MusicNote && SelLength > 0) {
                tsbToneUp.Enabled = true;
                tsbToneDown.Enabled = true;
                tsbVolumeUp.Enabled = true;
                tsbVolumeDown.Enabled = true;
            }
            else {
                tsbToneUp.Enabled = false;
                tsbToneDown.Enabled = false;
                tsbVolumeUp.Enabled = false;
                tsbVolumeDown.Enabled = false;
            }
        }


        private void tsbZoomIn_Click(object sender, EventArgs e) {
            ZoomScale(1);
        }

        private void tsbZoomOut_Click(object sender, EventArgs e) {
            ZoomScale(-1);
        }
        #endregion

        #region Control Event Handlers
        private void splitContainer1_MouseUp(object sender, MouseEventArgs e) {
            tvwSound.Select();
        }

        private void splitContainer2_MouseUp(object sender, MouseEventArgs e) {
            tvwSound.Select();
        }

        private void splitContainer3_MouseUp(object sender, MouseEventArgs e) {
            tvwSound.Select();
        }

        private void splitContainer2_Panel1_Resize(object sender, EventArgs e) {
            if (Visible) {
                UpdateVisibleStaves(StaffScale);
            }
        }

        private void tvwSound_KeyDown(object sender, KeyEventArgs e) {

            // backspace is treated as 'delete previous' instead of 
            // moving the selection
            if (e.KeyValue == (int)Keys.Back) {
                if (SelLength > 0) {
                    DeleteNotes(SelectedTrack, SelStart, SelLength);
                }
                else {
                    // if not on first note
                    if (SelStart > 0) {
                        // delete previous note
                        DeleteNotes(SelectedTrack, SelStart - 1, 1);
                    }
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void tvwSound_KeyPress(object sender, KeyPressEventArgs e) {

            // use staff keypress handler handle all keypresses
            picStaff_KeyPress(picStaff[SelectedTrack], e);
            // ignore all keypresses
            e.Handled = true;
        }

        private void tvwSound_KeyUp(object sender, KeyEventArgs e) {
            if (blnNoteOn) {
                NoteOff();
            }
            // when using keyboard to move the selection, the 
            // displayed track/note also needs to be updated
            switch (e.KeyCode) {
            case Keys.Down:
            case Keys.Up:
            case Keys.Right:
            case Keys.Left:
            case Keys.PageDown:
            case Keys.PageUp:
            case Keys.Home:
            case Keys.End:
                switch (tvwSound.SelectedNode.Level) {
                case 0:
                    // root
                    SelectSound(false);
                    break;
                case 1:
                    // loop
                    SelectTrack(tvwSound.SelectedNode.Index, false);
                    break;
                case 2:
                    // cel
                    int length;
                    if (tvwSound.NoSelection) {
                        length = 0;
                    }
                    else {
                        length = tvwSound.SelectedNodes.Count;
                    }
                    SelectNote(tvwSound.SelectedNode.Parent.Index, tvwSound.SelectedNodes[0].Index, tvwSound.SelectedNode.Index, length, false);
                    break;
                }
                break;
            }
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void tvwSound_MouseUp(object sender, MouseEventArgs e) {

            if (tvwSound.SelectedNodes.Count > 1) {
                // update selection
                SelectNote(tvwSound.SelectedNodes[0].Parent.Index, tvwSound.SelectedNodes[0].Index, tvwSound.SelectedNode.Index, tvwSound.SelectedNodes.Count, false);
                // clear property grid when there is a multiple-node selection
                propertyGrid1.SelectedObject = null;
                picStaff[SelectedTrack].Invalidate();
            }
        }

        private void tvwSound_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
            switch (e.Node.Level) {
            case 0:
                // root
                SelectSound();
                break;
            case 1:
                // track
                SelectTrack(e.Node.Index);
                break;
            case 2:
                // note
                SelectNote(e.Node.Parent.Index, e.Node.Index, e.Node.Index, tvwSound.NoSelection ? 0 : 1, e.Button == MouseButtons.Left);
                break;
            }
        }

        private void tvwSound_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
            // if double-clicking on a note, select it
            if (e.Node.Level == 2 && e.Node.Index != e.Node.Parent.Nodes.Count - 1) {
                tvwSound.NoSelection = false;
                SelectNote(e.Node.Parent.Index, e.Node.Index, e.Node.Index, 1, false);
            }
        }

        private void tvwSound_After(object sender, TreeViewEventArgs e) {
            if (!Visible) {
                return;
            }
            // stinkin glitch in expand/collapse raises  node-click event
            // sometimes, but not always, so have to check again after...
            if (e.Node is not null) {
                switch (e.Node.Level) {
                case 0:
                    if (SelectionMode != SelectionModeType.Sound) {
                        SelectSound();
                    }
                    break;
                case 1:
                    switch (SelectionMode) {
                    case SelectionModeType.MusicTrack:
                    case SelectionModeType.NoiseTrack:
                        if (SelectedTrack != e.Node.Index) {
                            SelectTrack(e.Node.Index);
                        }
                        break;
                    default:
                        SelectTrack(e.Node.Index);
                        break;
                    }
                    break;
                }
            }
        }

        private void picStaff_KeyDown(object sender, KeyEventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;

            switch (e.Modifiers) {
            case Keys.None:
                // no shift, ctrl, alt
                switch (e.KeyCode) {
                case Keys.Left:// or Keys.Up:
                    // if there is an active selection or cursor is not at start,
                    if (SelectedTrack != -1 && (SelStart > 0 || SelLength > 0)) {
                        // if just a cursor
                        if (SelLength == 0) {
                            // move left one note
                            SelectNote(SelectedTrack, SelStart - 1, SelStart - 1, 0);
                        }
                        else {
                            // if anchor is to right of selection
                            // collapse to current startpos
                            SelectNote(SelectedTrack, SelStart, SelStart, 0);
                        }
                    }
                    else if (SelectedTrack != -1 && SelStart == 0) {
                        // at beginning, move up to track
                        SelectTrack(SelectedTrack);
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
                case Keys.Right:// or Keys.Down:
                    // if there is an active selection or cursor is not at end,
                    if (SelectedTrack != -1 && (SelStart < EditSound[SelectedTrack].Notes.Count || SelLength > 0)) {
                        if (SelLength == 0) {
                            // move right one note
                            SelectNote(SelectedTrack, SelStart + 1, SelStart + 1, 0);
                        }
                        else {
                            // collapse selection to current end position
                            SelectNote(SelectedTrack, SelStart + SelLength, SelStart + SelLength, 0);
                        }
                    }
                    else if (SelectedTrack != -1 && SelStart == -1) {
                        // at track level, move to first note
                        SelectNote(SelectedTrack, SelStart, SelStart, 0);
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
                case Keys.Back:
                    if (SelLength > 0) {
                        DeleteNotes(SelectedTrack, SelStart, SelLength);
                    }
                    else {
                        // if not on first note
                        if (SelStart > 0) {
                            // delete previous note
                            DeleteNotes(SelectedTrack, SelStart - 1, 1);
                        }
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
                }
                break;
            case Keys.Shift:
                // if working with a staff, adjust selection; if working with tracks, exit
                if (SelectionMode == SelectionModeType.Sound ||
                    SelectionMode == SelectionModeType.MusicTrack ||
                    SelectionMode == SelectionModeType.NoiseTrack) {
                    return;
                }
                switch (e.KeyCode) {
                case Keys.Left:// or Keys.Up:
                    // selection is expanded to left, or collapsed on the right,
                    // depending on where current position is relative to the anchor

                    // if there is an active selection of one or more notes
                    // AND the startpoint is the anchor,
                    if (SelLength > 0 && SelAnchor == SelStart) {
                        // shrink the selection by one note (move end pt)
                        SelectNote(SelectedTrack, SelStart, SelStart, SelLength - 1);
                    }
                    else {
                        // if starting point not yet at beginning of track,
                        if (SelStart > 0) {
                            // expand selection by one note (move start)
                            SelectNote(SelectedTrack, SelStart - 1, SelAnchor, SelLength + 1);
                        }
                        else {
                            // cursor is already at beginning; just exit
                            return;
                        }
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
                case Keys.Right:// or Keys.Down:
                    // selection is expanded to right, or collapsed on the left,
                    // depending on where current position is relative to the anchor
                    if (SelAnchor == SelStart) {
                        // if not yet at end of track
                        if (SelStart + SelLength < EditSound[SelectedTrack].Notes.Count) {
                            // expand selection (move end pt)
                            SelectNote(SelectedTrack, SelStart, SelStart, SelLength + 1, true, 2);
                        }
                        else {
                            // cursor is already at end; just exit
                            return;
                        }
                    }
                    else {
                        // shrink selection by one note (move start pt)
                        SelectNote(SelectedTrack, SelStart + 1, SelAnchor, SelLength - 1);
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    break;
                }
                break;
            }
        }

        private void picStaff_KeyPress(object sender, KeyPressEventArgs e) {
            // no need to check which staff, since the active staff (SelectedTrack)
            // is always the one with the keyboard focus

            int keyval = e.KeyChar;

            switch (keyval) {
            case 8: // Keys.Back
                e.Handled = true;
                break;
            case 45:
                // "-" vol down (attenuation up)
                if (DefAttn < 14) {
                    DefAttn++;
                    DrawDefaultNote();
                }
                break;
            case 43:
                // "+" vol up (attenuation down)
                if (DefAttn > 0) {
                    DefAttn--;
                    DrawDefaultNote();
                }
                break;
            case 32:
                DefMute = !DefMute;
                DrawDefaultNote();
                break;
            case 60 or 44:
                // "<", "," octave down
                if (SelectedTrack != 3 && DefOctave > 3) {
                    DefOctave--;
                    picKeyboard.Invalidate();
                }
                break;
            case 62 or 46:
                // ">", "." octave up
                if (SelectedTrack != 3 && DefOctave < 10) {
                    DefOctave++;
                    picKeyboard.Invalidate();
                }
                break;
            case 91 or 123:
                // "{", "[" default length down
                if (DefLength > 1) {
                    DefLength--;
                    DrawDefaultNote();
                }
                break;
            case 93 or 125:
                // "}", "]" default length up
                if (DefLength < 16) {
                    DefLength++;
                    DrawDefaultNote();
                }
                break;
            }
            if (SelectedTrack == -1) {
                return;
            }
            switch (keyval) {
            case >= 97 and <= 103:
                // a - g /natural tones
                if (SelectedTrack == 3) {
                    // don't do anything
                    return;
                }
                // convert to 0-7 scale
                int lngMIDInote = keyval - 99;
                // shift so c=0 and g=6
                if (lngMIDInote < 0) {
                    lngMIDInote += 7;
                }
                // convert letter number to music scale
                lngMIDInote *= 2;
                if (lngMIDInote > 4) {
                    lngMIDInote--;
                }
                // combine with octave to get midi note
                lngMIDInote = DefOctave * 12 + lngMIDInote;
                // validate
                if (lngMIDInote < 45 || lngMIDInote > 127 ||
                    lngMIDInote == 121 || lngMIDInote == 124 || lngMIDInote == 126) {
                    return;
                }
                // play it
                NoteOn(lngMIDInote);
                break;
            case 67 or 68 or 70 or 71 or 65:
                // C, D, F, G, A (sharps)
                if (SelectedTrack == 3) {
                    // don't do anything
                    return;
                }
                // convert to 0-7 scale
                lngMIDInote = keyval - 67;
                // shift so C=0 and G=6
                if (lngMIDInote < 0) {
                    lngMIDInote -= 7;
                }
                // convert letter number to music scale
                lngMIDInote *= 2;
                if (lngMIDInote > 4) {
                    lngMIDInote--;
                }
                // combine with octave and sharpen
                lngMIDInote = DefOctave * 12 + lngMIDInote + 1;

                // validate
                if (lngMIDInote < 45 || lngMIDInote > 127 ||
                    lngMIDInote == 121 || lngMIDInote == 124 || lngMIDInote == 126) {
                    return;
                }
                // play it
                NoteOn(lngMIDInote);
                break;
            case >= 49 and <= 52:
                // 1, 2, 3, 4
                // only used in noise track
                if (SelectedTrack != 3) {
                    return;
                }
                // insert a noise track periodic tone note with appopriate frequency
                NoteOn(keyval - 49);
                break;
            case 33 or 64 or 35 or 36:
                // !,@,#,$
                // only used in noise track
                if (SelectedTrack != 3) {
                    return;
                }
                // insert a noise track white noise note with appropriate frequency
                if (keyval == 64) {
                    lngMIDInote = 5;
                }
                else {
                    lngMIDInote = keyval - 29;
                }
                NoteOn(lngMIDInote);
                break;
            }
        }

        private void picStaff_KeyUp(object sender, KeyEventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;
            if (blnNoteOn) {
                NoteOff();
            }
        }

        private void picStaff_MouseDown(object sender, MouseEventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;

            mX = e.X;
            mY = e.Y;

            switch (e.Button) {
            case MouseButtons.Right:
                // if clicking on a different track
                if (index != SelectedTrack) {
                    // select track first
                    SelectTrack(index);
                }
                // context menu
                return;
            case MouseButtons.Left:
                picStaff[index].Capture = true;
                // if clicking on clef area
                if (e.X < KeyWidth) {
                    // select first note in track
                    SelectNote(index, 0, 0, 0);
                }
                else {
                    // determine which note is being selected
                    // if holding shift key AND same staff
                    int tmpPos;
                    if (ModifierKeys == Keys.Shift && index == SelectedTrack) {
                        tmpPos = NoteFromPos(index, e.X, true);
                        if (tmpPos >= SelAnchor) {
                            // extend/compress selection (move end pt)
                            SelectNote(index, SelAnchor, SelAnchor, tmpPos - SelAnchor);
                        }
                        else {
                            // extend/compress selection (move start pt)
                            SelectNote(index, tmpPos, SelAnchor, SelAnchor - tmpPos);
                        }
                    }
                    else {
                        // determine which note is being selected
                        tmpPos = NoteFromPos(index, e.X, false);
                        SelectNote(index, tmpPos, tmpPos, 0);
                    }
                }
                break;
            }
        }

        private void picStaff_MouseMove(object sender, MouseEventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;

            // if track is not selected, no need to process mouse messages
            if (index != SelectedTrack) {
                return;
            }
            // if no movement from starting position
            if (mX == e.X && mY == e.Y) {
                // not really a mousemove
                return;
            }

            // update time marker
            double sngTime = (e.X - KeyWidth - SOHorz - 2) / TICK_WIDTH / 60 / StaffScale;
            UpdateStatusBarTime(sngTime);

            if (SelectedTrack == -1) {
                return;
            }
            switch (e.Button) {
            case MouseButtons.Left:
                // cache current mouse position
                mX = e.X;
                mY = e.Y;
                // if mouse position is off edge of screen, enable autoscrolling
                if (e.X < 0 && SOHorz != 0) {
                    tmrStaffScroll.Enabled = true;
                    tmrStaffScroll.Interval = 200 / ((-e.X / 5) + 1);
                    StaffScrollDir = 1;
                }
                else if (e.X > picStaff[index].ClientSize.Width - vsbStaff[index].Width &&
                         -SOHorz != hsbStaff.Maximum - hsbStaff.LargeChange + 1) {
                    tmrStaffScroll.Enabled = true;
                    tmrStaffScroll.Interval = 200 / (((e.X - (picStaff[index].ClientSize.Width - vsbStaff[index].Width)) / 5) + 1);
                    StaffScrollDir = -1;
                }
                else {
                    tmrStaffScroll.Enabled = false;
                    StaffScrollDir = 0;
                }
                // update selection based on mouse position
                SelectUnderMouse(e.X);
                break;
            }
        }

        private void picStaff_MouseUp(object sender, MouseEventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;
            picStaff[index].Capture = false;
            tmrStaffScroll.Enabled = false;
        }

        private void picStaff_MouseDoubleClick(object sender, MouseEventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;

            int tmpPos = NoteFromPos(SelectedTrack, e.X, false);

            // if note is already selected, do nothing
            if (SelLength > 0 && tmpPos >= SelStart && tmpPos <= SelStart + SelLength) {
                return;
            }
            // select the note being clicked;
            SelectNote(index, tmpPos, tmpPos, 1);
        }

        private void picStaff_MouseLeave(object sender, EventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;
            spTime.Text = "Pos: --"; // reset time marker
        }

        private void picStaff_MouseWheel(object sender, MouseEventArgs e) {
            // default action is to scroll the staff horizontally
            // if scroll button is pressed scroll vertically

            // if over a selectionh, SHIFT will adjust attenuation, CTRL
            // will adjust length (but only if a single note is selected)

            int index = (int)((SelectablePictureBox)sender).Tag;

            if (index == SelectedTrack && SelLength > 0) {
                int tmpPos = NoteFromPos(SelectedTrack, e.X, false);
                if (tmpPos >= SelStart && tmpPos < SelStart + SelLength) {
                    switch (ModifierKeys) {
                    case Keys.Shift:
                        // attenuation
                        ShiftNoteVol(Math.Sign(e.Delta), SelectedTrack, SelStart, SelLength);
                        return;
                    case Keys.Control:
                        if (SelLength == 1) {
                            // length
                            ShiftDuration(Math.Sign(e.Delta), SelectedTrack, SelStart);
                            return;
                        }
                        break;
                    }
                }
            }

            // not over a selection; scroll the window
            int newval;
            if (MouseButtons == MouseButtons.Middle) {
                if (vsbStaff[index].Enabled) {
                    newval = vsbStaff[index].Value;
                    if (Math.Sign(e.Delta) == -1) {
                        newval += vsbStaff[index].SmallChange;
                        if (newval > vsbStaff[index].Maximum - vsbStaff[index].LargeChange + 1) {
                            newval = vsbStaff[index].Maximum - vsbStaff[index].LargeChange + 1;
                        }
                    }
                    else if (Math.Sign(e.Delta) == 1) {
                        newval -= vsbStaff[index].SmallChange;
                        if (newval < 0) {
                            newval = 0;
                        }
                    }
                    vsbStaff[index].Value = newval;
                }
            }
            else {
                if (hsbStaff.Enabled) {
                    newval = hsbStaff.Value;
                    if (Math.Sign(e.Delta) == -1) {
                        newval -= hsbStaff.SmallChange;
                        if (newval < 0) {
                            newval = 0;
                        }
                    }
                    else if (Math.Sign(e.Delta) == 1) {
                        newval += hsbStaff.SmallChange;
                        if (newval > hsbStaff.Maximum - hsbStaff.LargeChange + 1) {
                            newval = hsbStaff.Maximum - hsbStaff.LargeChange + 1;
                        }
                    }
                    hsbStaff.Value = newval;
                }
            }
        }

        private void picStaff_Paint(object sender, PaintEventArgs e) {
            if (Disposing) return;
            int index = (int)((SelectablePictureBox)sender).Tag;

            // to support inverting, need to use backbuffering
            using (Bitmap backBuffer = new Bitmap(picStaff[index].Width, picStaff[index].Height)) {
                using (Graphics bbg = Graphics.FromImage(backBuffer)) {
                    bbg.Clear(Color.White);
                    if (index == 3) {
                        DrawNoiseStaff(bbg);
                    }
                    else {
                        DrawMusicStaff(bbg, index);
                    }

                    // highlight selection by inverting it
                    if (SelectedTrack == index && SelStart >= 0) {

                        if (SelLength > 0) {
                            HighlightSelection(backBuffer, bbg);
                        }
                        else if (CursorOn) {
                            ShowCursor(backBuffer, bbg);
                        }
                    }
                    e.Graphics.DrawImageUnscaled(backBuffer, 0, 0);
                }
            }
        }

        private void tmrCursor_Tick(object sender, EventArgs e) {
            // toggle cursor state
            CursorOn = !CursorOn;
            Debug.Assert(SelectedTrack != -1);
            Debug.Assert(SelLength == 0);

            picStaff[SelectedTrack].Invalidate();
        }

        private void vsbStaff_ValueChanged(object sender, EventArgs e) {
            int index = (int)((VScrollBar)sender).Tag;
            if (SOVert[index] == -vsbStaff[index].Value) {
                return;
            }
            SOVert[index] = -vsbStaff[index].Value;
            picStaff[index].Invalidate();
        }

        private void hsbStaff_ValueChanged(object sender, EventArgs e) {
            if (SOHorz == -hsbStaff.Value) {
                return;
            }
            SOHorz = -hsbStaff.Value;
            for (int i = 0; i < 4; i++) {
                if (picStaff[i].Visible) {
                    picStaff[i].Invalidate();
                }
            }
        }

        private void tmrStaffScroll_Tick(object sender, EventArgs e) {
            // autoscroll staves

            switch (StaffScrollDir) {
            case 1:
                // scroll left
                // if already at left edge
                if (SOHorz == 0) {
                    // disable autoscroll
                    StaffScrollDir = 0;
                    tmrStaffScroll.Enabled = false;
                    return;
                }
                else {
                    hsbStaff.Value = Math.Max(hsbStaff.Minimum, hsbStaff.Value - 10);
                    // if there is room for a small change
                    if (hsbStaff.Value > hsbStaff.SmallChange) {
                        hsbStaff.Value -= hsbStaff.SmallChange;
                    }
                    else {
                        hsbStaff.Value = 0;
                    }
                }
                break;
            case -1:
                // scroll right
                // if already at right edge
                if (hsbStaff.Value == hsbStaff.Maximum - hsbStaff.LargeChange + 1) {
                    // disable autoscroll
                    StaffScrollDir = 0;
                    tmrStaffScroll.Enabled = false;
                    return;
                }
                else {
                    hsbStaff.Value = Math.Min(hsbStaff.Maximum - hsbStaff.LargeChange + 1, hsbStaff.Value + 10);
                }
                break;
            }
            // adjust mouse position? or use cached value?
            double sngTime = (mX - KeyWidth - SOHorz - 2) / TICK_WIDTH / 60 / StaffScale;
            UpdateStatusBarTime(sngTime);
            // update selection using new mouse position
            SelectUnderMouse(mX);
        }

        private void picKeyboard_MouseDown(object sender, MouseEventArgs e) {
            if (tmrStaffScroll.Enabled) {
                // should never get here
                tmrStaffScroll.Enabled = false;
            }

            switch (SelectedTrack) {
            case -1:
                // nothing selected; do nothing
                return;
            case 3:
                // noise track
                // if not on keys (based on vertical position)
                if (e.Y < 33) {
                    return;
                }
                int lngMIDInote = (e.X + NKbOffset) / (picKeyboard.Width / 8 < MinNoiseKeyWidth ? MinNoiseKeyWidth : picKeyboard.Width / 8);
                // play this note
                NoteOn(lngMIDInote);
                break;
            default:
                // tone track
                // note number is 1/2 key width
                // adjusted for offset, and one half key width
                // so keys are read correctly
                int NoteNum = (e.X + 6) / 12 + (MKbOffset - 40) * 2;
                // new octave every 14 keys (adjusted so it changes at each C note)
                int Octave = NoteNum / 14 + 3;
                // convert note to relative number
                NoteNum %= 14;

                // this is a black key if note is even and NOT (0 or 6) AND ABOVE black key edge
                bool blnOnBlack = (NoteNum % 2 == 0) && !(NoteNum == 0 || NoteNum == 6) && (e.Y <= 32);

                // if on a black key
                if (blnOnBlack) {
                    if (NoteNum <= 4) {
                        NoteNum--;
                    }
                    else {
                        NoteNum -= 2;
                    }
                }
                else {
                    // recalculate which key was pressed
                    NoteNum = e.X / 24 + MKbOffset - 40;
                    Octave = NoteNum / 7 + 3;
                    NoteNum %= 7;
                    // adjust for black keys
                    NoteNum *= 2;
                    if (NoteNum > 4) {
                        NoteNum--;
                    }
                }
                // calculate note to play
                lngMIDInote = (Octave * 12) + NoteNum;
                // play the note
                NoteOn(lngMIDInote);
                break;
            }
        }

        private void picKeyboard_MouseUp(object sender, MouseEventArgs e) {
            // stop playing note
            NoteOff();
        }

        private void picKeyboard_Resize(object sender, EventArgs e) {
            picKeyboard.Invalidate();
        }

        private void picKeyboard_Paint(object sender, PaintEventArgs e) {
            if (Visible) {
                if (!picKeyboard.Visible) {
                    return;
                }
                SetKeyboardScroll(SelectedTrack == 3);
                DrawKeyboard(e.Graphics);
            }
        }

        private void picKeyboard_MouseWheel(object sender, MouseEventArgs e) {
            if (Math.Sign(e.Delta) == -1) {
                ScrollKeyboardRight();
            }
            else if (Math.Sign(e.Delta) == 1) {
                ScrollKeyboardLeft();
            }
        }

        private void picDuration_MouseWheel(object sender, MouseEventArgs e) {
            switch (ModifierKeys) {
            case Keys.Shift:
                if (Math.Sign(e.Delta) == -1) {
                    // increase default attenuation
                    // (decrease volume)
                    if (DefAttn < 14) {
                        DefAttn += 1;
                        DrawDefaultNote();
                    }
                }
                else if (Math.Sign(e.Delta) == 1) {
                    // decrease default attenuation
                    // (increase volume)
                    if (DefAttn > 1) {
                        DefAttn -= 1;
                        DrawDefaultNote();
                    }
                }

                break;
            case Keys.None:
                if (Math.Sign(e.Delta) == -1) {
                    // decrease default length
                    if (DefLength > 1) {
                        DefLength -= 1;
                        DrawDefaultNote();
                    }
                }
                else if (Math.Sign(e.Delta) == 1) {
                    // increase default length
                    if (DefLength < 16) {
                        DefLength += 1;
                        DrawDefaultNote();
                    }
                }
                break;
            }
        }

        private void picDuration_MouseClick(object sender, MouseEventArgs e) {

            if (e.Button == MouseButtons.Left) {
                // check for click on attenuation bar
                if (e.Y < 16) {
                    if (e.X < picDuration.Width / 2 - 4) {
                        // increase default attenuation
                        if (DefAttn < 15) {
                            DefAttn += 1;
                            DrawDefaultNote();
                        }
                    }
                    else if (e.X > picDuration.Width / 2 + 4) {
                        // decrease default attenuation
                        if (DefAttn > 0) {
                            DefAttn -= 1;
                            DrawDefaultNote();
                        }
                    }
                }
            }
        }

        private void picDuration_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (e.X > 12 && e.X < 26 && e.Y < 18) {
                DefMute = !DefMute;
                DrawDefaultNote();
            }
        }

        private void btnKybdRight_MouseDown(object sender, MouseEventArgs e) {
            ScrollKeyboardRight();
            tmrKeyboardScroll.Tag = 0;
            tmrKeyboardScroll.Interval = 800; // initial delay
            tmrKeyboardScroll.Enabled = true;
        }

        private void ScrollKeyboardRight() {
            if (SelectedTrack == 3) {
                // enable if calculated keywidth is less than MinNoiseKeyWidth
                if (picKeyboard.Width / 8 < MinNoiseKeyWidth) {
                    NKbOffset += 10;
                    if (NKbOffset > 8 * MinNoiseKeyWidth - picKeyboard.Width) {
                        NKbOffset = 8 * MinNoiseKeyWidth - picKeyboard.Width;
                    }
                }
                else {
                    return;
                }
            }
            else {
                // enable scrollbar only if all keys don't fit
                // (max white-key offset is 94)
                if ((picKeyboard.Width / 24) < 49) {
                    MKbOffset += 1;
                    if (MKbOffset > 94 - picKeyboard.Width / 24) {
                        MKbOffset = 94 - picKeyboard.Width / 24;
                    }
                }
                else {
                    return;
                }
            }
            // redraw keyboard
            picKeyboard.Invalidate();
        }

        private void btnKybdLeft_MouseDown(object sender, MouseEventArgs e) {
            tmrKeyboardScroll.Tag = 1;
            tmrKeyboardScroll.Interval = 800; // initial delay
            tmrKeyboardScroll.Enabled = true;
            ScrollKeyboardLeft();
        }

        private void ScrollKeyboardLeft() {
            if (SelectedTrack == 3) {
                NKbOffset -= 10;
                if (NKbOffset < 0) {
                    NKbOffset = 0;
                }
            }
            else {
                MKbOffset -= 1;
                if (MKbOffset < 45) {
                    MKbOffset = 45;
                }
            }
            // redraw keyboard
            picKeyboard.Invalidate();
        }

        private void btnKybdLeft_MouseUp(object sender, MouseEventArgs e) {
            tmrKeyboardScroll.Enabled = false;
            SetFocus();
        }

        private void btnKybdRight_MouseUp(object sender, MouseEventArgs e) {
            tmrKeyboardScroll.Enabled = false;
            SetFocus();
        }

        private void tmrKeyboardScroll_Tick(object sender, EventArgs e) {
            if (tmrKeyboardScroll.Tag is int direction) {
                // scroll left or right
                if (direction == 0) {
                    ScrollKeyboardRight();
                }
                else if (direction == 1) {
                    ScrollKeyboardLeft();
                }
            }
            if (tmrKeyboardScroll.Interval == 800) {
                // speed up after first tick
                tmrKeyboardScroll.Interval = 200;
            }
        }

        private void btnPlay_Click(object sender, EventArgs e) {
            // play the sound
            // not playing now - begin
            PlaySound();
            // go back
            SetFocus();
        }

        private void btnStop_Click(object sender, EventArgs e) {
            // stop the sound
            StopSound();
            SetFocus();
        }

        private void btnMute_Click(object sender, EventArgs e) {
            int index = (int)((Button)sender).Tag;
            // toggle mute for this track
            SetTrackMute(index, !EditSound[index].Muted);
            SetFocus();
        }

        private void btnDurationUp_Click(object sender, EventArgs e) {
            if (DefLength < 16) {
                DefLength += 1;
                DrawDefaultNote();
            }
            SetFocus();
        }

        private void btnDurationDown_Click(object sender, EventArgs e) {
            if (DefLength > 1) {
                DefLength -= 1;
                DrawDefaultNote();
            }
            SetFocus();
        }
        #endregion

        #region Methods
        private void SetFocus() {
            // if a track is selected, and visible, set focus to it
            if (SelectedTrack != -1 && picStaff[SelectedTrack].Visible) {
                picStaff[SelectedTrack].Select();
            }
            else {
                tvwSound.Select();
            }
        }

        private void InitStatusStrip() {
            spScale = new ToolStripStatusLabel();
            spTime = new ToolStripStatusLabel();
            spStatus = MDIMain.spStatus;
            // 
            // spScale
            // 
            spScale.AutoSize = false;
            spScale.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spScale.BorderStyle = Border3DStyle.SunkenInner;
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(70, 18);
            spScale.Text = "";
            // 
            // spTime
            // 
            spTime.AutoSize = false;
            spTime.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spTime.BorderStyle = Border3DStyle.SunkenInner;
            spTime.Name = "spTime";
            spTime.Size = new System.Drawing.Size(140, 18);
            spTime.Text = "Pos: --";
        }

        public bool LoadSound(Sound loadsound, bool quiet = false) {
            InGame = loadsound.InGame;
            if (InGame) {
                SoundNumber = loadsound.Number;
            }
            else {
                // use a number that can never match
                // when searches for open sounds are made
                SoundNumber = 256;
            }
            // create new Sound object for the editor
            try {
                loadsound.Load();
            }
            catch (Exception ex) {
                // unhandled error
                if (!quiet) {
                    string resid = InGame ? "Sound " + SoundNumber : loadsound.ID;
                    ErrMsgBox(ex,
                        "Something went wrong. Unable to load " + resid,
                        ex.StackTrace,
                        "Load Sound Failed");
                }
                return false;
            }
            if (loadsound.Error != ResourceErrorType.NoError) {
                if (!quiet) {
                    if (InGame) {
                        switch (loadsound.Error) {
                        case ResourceErrorType.FileNotFound:
                            // should not be possible unless volfile deleted after
                            // the game was loaded
                            MessageBox.Show(MDIMain,
                                $"The VOL file with Sound {loadsound.Number} is missing.",
                                "Missing VOL File",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning, 0, 0,
                                WinAGIHelp, "htm\\winagi\\errors\\re01.htm");
                            break;
                        case ResourceErrorType.FileIsReadonly:
                            // should not be possible unless volfile properties were
                            // changed after the game was loaded
                            MessageBox.Show(MDIMain,
                                $"Sound {loadsound.Number} is in a VOL file tagged as readonly. " +
                                "It cannot be edited unless full access is allowed.",
                                "Readonly VOL File",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, 0, 0,
                                WinAGIHelp, "htm\\winagi\\errors\\re02.htm");
                            break;
                        case ResourceErrorType.FileAccessError:
                            MessageBox.Show(MDIMain,
                                $"A file access error in the VOL file with Picture {loadsound.Number} " +
                                "is preventing the picture from being edited. ",
                                "VOL File Access Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, 0, 0,
                                WinAGIHelp, "htm\\winagi\\errors\\re03.htm");
                            break;
                        //case ResourceErrorType.InvalidLocation:
                        //case ResourceErrorType.InvalidHeader:
                        //case ResourceErrorType.DecompressionError:
                        default:
                            // should not be possible
                            Debug.Assert(false);
                            MessageBox.Show(MDIMain,
                            "Something went wrong. Unable to load Sound " + SoundNumber,
                            "Load Sound Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                            break;
                        }
                    }
                    else {
                        // show a generic message
                        MessageBox.Show(MDIMain,
                            "Unable to open Sound " + SoundNumber + ":\n\n LoadError " +
                            loadsound.Error.ToString(),
                            "Sound Resource Load Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                return false;
            }
            // only pc sounds can be edited
            if (loadsound.SndFormat != SoundFormat.AGI) {
                Debug.Assert(false);
                MessageBox.Show(MDIMain,
                    "Only PCjr sound resources can be edited in WinAGI.",
                    "Unsupported Sound Format",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            EditSound = loadsound.Clone();
            if (InGame) {
                EditPalette = EditGame.Palette.Clone();
            }
            // set caption and changed flag
            if (!InGame && EditSound.ID == "NewSound") {
                SoundCount++;
                EditSound.ID = "NewSound" + SoundCount;
                IsChanged = true;
            }
            else {
                // TODO: why would IsChanged ever be true?
                IsChanged = EditSound.IsChanged || EditSound.Warnings != 0;
            }
            Text = sSNDED + ResourceName(EditSound, InGame, true);
            if (IsChanged) {
                Text = CHG_MARKER + Text;
            }
            mnuRSave.Enabled = !IsChanged;
            MDIMain.btnSaveResource.Enabled = !IsChanged;

            if (!BuildSoundTree()) {
                // error
                MessageBox.Show(MDIMain,
                    "This sound has corrupt or invalid data. Unable to open it for editing:",
                    "Sound Resource Data Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                EditSound.Unload();
                EditSound = null;
                return false;
            }
            tvwSound.Nodes[0].ExpandAll();
            SelectSound();

            // set defaults
            StaffScale = WinAGISettings.SndZoom.Value;
            DefLength = 4;
            DefOctave = 5;
            NKbOffset = 0;
            MKbOffset = 52;
            switch (WinAGISettings.PlaybackMode.Value) {
            case 0:
                PlaybackMode = SoundPlaybackMode.PCSpeaker;
                break;
            case 2:
                PlaybackMode = SoundPlaybackMode.MIDI;
                break;
            default:
                PlaybackMode = SoundPlaybackMode.WAV;
                break;
            }
            ShowNotes = WinAGISettings.ShowNotes.Value;
            OneTrack = WinAGISettings.OneTrack.Value;
            KeyboardVisible = WinAGISettings.ShowKeyboard.Value;
            splitContainer2.Panel2Collapsed = !KeyboardVisible;
            KeyboardSound = !WinAGISettings.NoKeyboardSound.Value;
            staffFont = new("Courier New", 8 * StaffScale);
            for (int i = 0; i < 4; i++) {
                if (EditSound[i].Muted) {
                    btnMute[i].Image = EditorResources.esi_muteon;
                }
                else {
                    btnMute[i].Image = EditorResources.esi_muteoff;
                }
            }
            spScale.Text = "Scale: " + StaffScale;
            // draw the default note display
            DrawDefaultNote();
            SetKeyWidth();
            // UpdateVisibleStaves will position and display the staves
            // when the form resizs after loadng (all picstaff picture
            // boxes should be marked not Visible before loading)
            return true;
        }

        /// <summary>
        /// Re-distribute the staff panels to match the current visibility of
        /// the tracks.
        /// </summary>
        public void UpdateVisibleStaves(int oldscale) {
            // track previous vertical scrollbar values (default is
            // minus 1 indicating panel was previously not visible)
            int[] oldvalue = [-1, -1, -1, -1];
            if (OneTrack) {
                // display only the selected track
                for (int i = 0; i <= 3; i++) {
                    if (i == SelectedTrack) {
                        if (picStaff[i].Visible == true) {
                            oldvalue[i] = vsbStaff[i].Value;
                        }
                        picStaff[i].Visible = true;
                        picStaff[i].Top = 0;
                        oldstaffH[i] = picStaff[i].ClientSize.Height;
                        picStaff[i].Height = splitContainer2.Panel1.Height - hsbStaff.Height;
                        picStaff[i].Width = splitContainer2.Panel1.Width;
                    }
                    else {
                        picStaff[i].Visible = false;
                    }
                }
            }
            else {
                // first determine which staves need to be displayed
                StaffCount = 0;
                for (int i = 0; i <= 3; i++) {
                    if (EditSound[i].Visible) {
                        if (picStaff[i].Visible) {
                            oldvalue[i] = vsbStaff[i].Value;
                        }
                        StaffCount++;
                    }
                    picStaff[i].Visible = EditSound[i].Visible;
                }
                // then set position and size of visible staves
                // (staff 3 height is never more than needed to show entire staff)
                int maxNoiseHeight = 21 + 36 * StaffScale;
                int onestaffheight = 1;
                if (StaffCount > 1) {
                    onestaffheight = (splitContainer2.Panel1.Height - hsbStaff.Height) / StaffCount;
                    if (picStaff[3].Visible && onestaffheight > maxNoiseHeight) {
                        // readjust heights so noise is at its max
                        oldstaffH[3] = picStaff[3].ClientSize.Height;
                        picStaff3.Height = maxNoiseHeight;
                        onestaffheight = (splitContainer2.Panel1.Height - hsbStaff.Height - maxNoiseHeight) / (StaffCount - 1);
                        for (int i = 0; i < 3; i++) {
                            oldstaffH[i] = picStaff[i].ClientSize.Height;
                            picStaff[i].Height = onestaffheight;
                        }
                    }
                    else {
                        for (int i = 0; i < 4; i++) {
                            oldstaffH[i] = picStaff[i].ClientSize.Height;
                            picStaff[i].Height = onestaffheight;
                        }
                    }
                }
                switch (StaffCount) {
                case 0:
                    // no staves visible
                    hsbStaff.Visible = false;
                    break;
                case 1:
                    // one staff visible
                    hsbStaff.Visible = true;
                    for (int i = 0; i <= 3; i++) {
                        if (picStaff[i].Visible) {
                            picStaff[i].Top = 0;
                            oldstaffH[i] = picStaff[i].ClientSize.Height;
                            picStaff[i].Height = splitContainer2.Panel1.Height - hsbStaff.Height;
                            picStaff[i].Width = splitContainer2.Panel1.Width;
                            break;
                        }
                    }
                    break;
                default:
                    // two or more staves visible   
                    hsbStaff.Visible = true;
                    int ctr = 0;
                    for (int i = 0; i < 4; i++) {
                        if (picStaff[i].Visible) {
                            picStaff[i].Top = ctr * onestaffheight;
                            picStaff[i].Width = splitContainer2.Panel1.Width;
                            ctr++;
                        }
                    }
                    break;
                }
            }
            // now step through all visible staves, update vertical scroll
            // bars and invalidate them to force re-paint
            for (int i = 0; i < 4; i++) {
                if (picStaff[i].Visible) {
                    SetVScroll(i, oldscale, oldvalue[i]);
                    picStaff[i].Invalidate();
                }
            }
            // adjust key width FIRST
            SetKeyWidth();
            // THEN resize horizontal scale
            SetHScroll(oldscale);
        }

        private bool BuildSoundTree() {
            for (int i = 0; i < 4; i++) {
                // add this track's notes
                for (int j = 0; j < EditSound[i].Notes.Count; j++) {
                    TreeNode tmpNode = tvwSound.Nodes[0].Nodes[i].Nodes.Insert(j, "Note " + j);
                }
            }
            tvwSound.Nodes[0].Text = EditSound.ID;
            // return success
            return true;
        }

        private void SetKeyWidth() {
            KeyWidth = (5 + Math.Abs(EditSound.Key)) * 6 * StaffScale;
            if (KeyWidth < 36 * StaffScale) {
                KeyWidth = 36 * StaffScale;
            }
        }

        private void SetKeyboardScroll(bool NoiseKybd) {
            if (KeyboardVisible) {
                // set keyboard scrollbar properties
                if (NoiseKybd) {
                    // enable if min keywidth is less than MinNoiseKeyWidth
                    btnKybdLeft.Enabled = btnKybdRight.Enabled = picKeyboard.Width / 8 < MinNoiseKeyWidth;
                }
                else {
                    // enable scrollbar only if all keys don't fit
                    btnKybdLeft.Enabled = btnKybdRight.Enabled = (picKeyboard.Width / 24) < 49;
                }
            }
        }

        private void PlaySound() {
            if (PlaybackMode == SoundPlaybackMode.MIDI) {
                // release midi that is being used by sound editor to
                // play notes on keyboard
                midiNotePlayer.KillMidi();
            }
            // set up sound resource to play the sound
            EditSound.SoundComplete += This_SoundComplete;
            EditSound.PlaySound(PlaybackMode);
            btnPlay.Enabled = false;
            btnStop.Enabled = true;
            PlayingSound = true;
        }

        private void StopSound() {
            PlayingSound = false;
            EditSound.SoundComplete -= This_SoundComplete;
            EditSound.StopSound();
            // need to check if invoke is required- accessing the UI elements
            // is done differently if that's the case
            if (btnPlay.InvokeRequired) {
                btnPlay.Invoke(new Action(() => { btnPlay.Enabled = CanPlay(); }));
                btnStop.Invoke(new Action(() => { btnStop.Enabled = false; }));
            }
            else {
                btnPlay.Enabled = CanPlay();
                btnStop.Enabled = false;
            }
            // if midi, restore the midi player to play keyboard sounds
            if (PlaybackMode == SoundPlaybackMode.MIDI) {
                midiNotePlayer.InitMidi();
            }
        }

        private bool CanPlay() {
            // check if sound is playable
            if (EditSound.Error != ResourceErrorType.NoError && EditSound.Error != ResourceErrorType.FileIsReadonly) {
                // (any error except readonly means not playable)
                return false;
            }
            if (EditSound.Length == 0) {
                return false;
            }
            return true;
        }

        internal void SelectSound(bool refreshtree = true) {
            int oldtrack = SelectedTrack;

            if (refreshtree) {
                if (tvwSound.SelectedNode != tvwSound.Nodes[0]) {
                    tvwSound.SelectedNode = tvwSound.Nodes[0];

                }
                tvwSound.SelectedNode.EnsureVisible();
            }
            SelectionMode = SelectionModeType.Sound;
            SelectedTrack = -1;
            SelStart = -1;
            tmrCursor.Enabled = false;
            propertyGrid1.SelectedObject = new SoundEditSound(this);
            if (oldtrack != SelectedTrack) {
                picStaff[oldtrack].Invalidate();
            }
            if (tvwSound.SelectedNode != tvwSound.Nodes[0]) {
                tvwSound.SelectedNode = tvwSound.Nodes[0];
                tvwSound.SelectedNode.EnsureVisible();
            }
            if (oldtrack == 3) {
                picKeyboard.Invalidate();
            }
            ConfigureToolbar();
        }

        internal void SelectTrack(int track, bool refreshtree = true) {
            int oldtrack = SelectedTrack;

            if (refreshtree) {
                if (tvwSound.SelectedNode != tvwSound.Nodes[0].Nodes[track]) {
                    tvwSound.SelectedNode = tvwSound.Nodes[0].Nodes[track];
                }
            }

            if (track == 3) {
                // noise track
                SelectionMode = SelectionModeType.NoiseTrack;
                SelectedTrack = 3;
                SelStart = -1;
                tmrCursor.Enabled = false;
                SelAnchor = -1;
                SelLength = 0;
                propertyGrid1.SelectedObject = new SoundEditNTrack(this);
            }
            else {
                // sound track
                SelectionMode = SelectionModeType.MusicTrack;
                SelectedTrack = track;
                SelStart = -1;
                tmrCursor.Enabled = false;
                SelAnchor = -1;
                SelLength = 0;
                propertyGrid1.SelectedObject = new SoundEditMTrack(this, track, SelectedTrack);
            }
            if (OneTrack) {
                UpdateVisibleStaves(StaffScale);
            }
            else {
                if (oldtrack != -1 && oldtrack != SelectedTrack) {
                    picStaff[oldtrack].Invalidate();
                }
            }
            picStaff[SelectedTrack].Invalidate();
            if (tvwSound.SelectedNode != tvwSound.Nodes[0].Nodes[SelectedTrack]) {
                tvwSound.SelectedNode = tvwSound.Nodes[0].Nodes[SelectedTrack];
                tvwSound.SelectedNode.EnsureVisible();
            }
            if (SelectedTrack == 3 && oldtrack != 3 || SelectedTrack != 3 && oldtrack == 3) {
                picKeyboard.Invalidate();
            }
            ConfigureToolbar();
        }

        internal void SelectNote(int track, int startnote, int anchor, int length, bool refreshtree = true, int showselection = 1) {
            int oldtrack = SelectedTrack;

            if (refreshtree) {
                // refresh the tree node selection
                if (length <= 1) {
                    tvwSound.SelectedNodes = [tvwSound.Nodes[0].Nodes[track].Nodes[startnote]];
                    tvwSound.SelectedNode = tvwSound.Nodes[0].Nodes[track].Nodes[startnote];
                    tvwSound.NoSelection = length == 0;
                    tvwSound.SelectedNode.EnsureVisible();
                }
                else {
                    if (tvwSound.SelectedNodes[0].Index != startnote || tvwSound.SelectedNodes.Count != length) {
                        tvwSound.BeginUpdate();
                        tvwSound.SelectedNode = tvwSound.Nodes[0].Nodes[track].Nodes[anchor];
                        tvwSound.SelectedNodes.Clear();
                        tvwSound.SelectedNodes = tvwSound.Nodes[0].Nodes[track].Nodes.Cast<TreeNode>().Skip(startnote).Take(length).ToList();
                        tvwSound.EndUpdate();
                    }
                    tvwSound.SelectedNode.EnsureVisible();
                }
            }
            if (startnote == EditSound[track].Notes.Count) {
                // end only sets insertion
                SelectionMode = SelectionModeType.EndNote;
                SelectedTrack = track;
                SelStart = startnote;
                SelAnchor = startnote;
                SelLength = 0;
                tmrCursor.Enabled = true;
                propertyGrid1.SelectedObject = null;
            }
            else if (track == 3) {
                // noise track
                SelectionMode = SelectionModeType.NoiseNote;
                SelectedTrack = 3;
                SelStart = startnote;
                SelAnchor = anchor;
                SelLength = length;
                tmrCursor.Enabled = length == 0;
                if (tvwSound.SelectedNodes.Count > 1) {
                    propertyGrid1.SelectedObject = null;
                }
                else {
                    propertyGrid1.SelectedObject = new SoundEditNNote(this, startnote);
                }
            }
            else {
                // sound track
                SelectionMode = SelectionModeType.MusicNote;
                SelectedTrack = track;
                SelStart = startnote;
                SelAnchor = anchor;
                SelLength = length;
                tmrCursor.Enabled = length == 0;
                if (tvwSound.SelectedNodes.Count > 1) {
                    propertyGrid1.SelectedObject = null;
                }
                else {
                    propertyGrid1.SelectedObject = new SoundEditMNote(this, track, startnote);
                }
            }

            if (SelectedTrack == 3 && oldtrack != 3 || SelectedTrack != 3 && oldtrack == 3) {
                picKeyboard.Invalidate();
            }
            // update displayed staves
            if (oldtrack != SelectedTrack && OneTrack) {
                picStaff[oldtrack].Visible = false;
                picStaff[SelectedTrack].Top = 0;
                picStaff[SelectedTrack].Size = picStaff[oldtrack].Size;
                SetVScroll(SelectedTrack, StaffScale, SOVert[SelectedTrack]);
                picStaff[SelectedTrack].Visible = true;
            }
            if (picStaff[SelectedTrack].Visible) {
                switch (showselection) {
                case 1:
                    // scroll startpos into view
                    int startPos = (int)(EditSound[SelectedTrack].TimePos(SelStart) * TICK_WIDTH * StaffScale + SOHorz + KeyWidth);
                    // determine if selection is in view
                    if (startPos < KeyWidth || startPos > picStaff[SelectedTrack].ClientSize.Width - 3 - vsbStaff[SelectedTrack].Width) {
                        // choose an offset that puts selection at 5% of window, past keywidth
                        SOHorz = (int)(0.00 * (picStaff[SelectedTrack].ClientSize.Width - 3 - vsbStaff[SelectedTrack].Width) - EditSound[SelectedTrack].TimePos(SelStart) * TICK_WIDTH * StaffScale);
                        if (SOHorz < -hsbStaff.Maximum + hsbStaff.LargeChange - 1) {
                            SOHorz = -hsbStaff.Maximum + hsbStaff.LargeChange - 1;
                        }
                        hsbStaff.Value = -SOHorz;
                    }
                    break;
                case 2:
                    // scroll endpos into view
                    int endPos = (int)(EditSound[SelectedTrack].TimePos(SelStart + SelLength) * TICK_WIDTH * StaffScale + SOHorz + KeyWidth);
                    if (endPos < KeyWidth || endPos > picStaff[SelectedTrack].ClientSize.Width - 3 - vsbStaff[SelectedTrack].Width) {
                        // choose an offset that puts selection at 95% of window
                        SOHorz = (int)(0.95 * (picStaff[SelectedTrack].ClientSize.Width - 3 - vsbStaff[SelectedTrack].Width) - KeyWidth - EditSound[SelectedTrack].TimePos(SelStart + SelLength) * TICK_WIDTH * StaffScale);
                        hsbStaff.Value = -SOHorz;
                    }
                    break;
                }
            }
            for (int i = 0; i < 4; i++) {
                if (picStaff[i].Visible) {
                    picStaff[i].Invalidate();
                }
            }
            ConfigureToolbar();
        }

        internal void DrawMusicStaff(Graphics gs, int track) {
            SolidBrush brush = new(Color.Black);
            Pen pen = new(Color.Black);
            int x1, x2, y1, y2;
            int lngTPQN = EditSound.TPQN;
            int lngHPos;

            gs.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gs.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // sound length in whole note units
            int lngSndLength = (int)(EditSound.Length * 15 / lngTPQN);
            // draw staff lines
            x1 = 0;
            x2 = picStaff[track].Width;
            for (int i = 0; i < 5; i++) {
                y1 = y2 = (0 + 96 + 6 * i) * StaffScale + SOVert[track];
                gs.DrawLine(pen, x1, y1, x2, y2);
                y1 = y2 = (0 + 156 + 6 * i) * StaffScale + SOVert[track];
                gs.DrawLine(pen, x1, y1, x2, y2);
            }
            // draw time lines at whole note intervals, offset by two pixels
            for (int i = 0; i <= lngSndLength; i++) {
                // horizontal pos of marker
                lngHPos = (int)(i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2);
                // if past right edge
                if (lngHPos > picStaff[track].Width) {
                    break;
                }
                // if greater than 0 (not to left of visible window)
                if (lngHPos >= 0) {
                    y1 = (96 * StaffScale) + SOVert[track];
                    y2 = y1 + 36 * StaffScale;
                    gs.DrawLine(pen, lngHPos, y1, lngHPos, y2);
                    y1 = (144 * StaffScale) + SOVert[track];
                    y2 = y1 + 36 * StaffScale;
                    gs.DrawLine(pen, lngHPos, y1, lngHPos, y2);
                }
            }

            // add notes
            // first note position
            lngHPos = SOHorz + KeyWidth;
            // step through all notes in this track
            for (int i = 0; i < EditSound[track].Notes.Count; i++) {
                // get duration first
                int lngDur = EditSound[track][i].Duration;
                // if note is visible,
                if (lngHPos + StaffScale * TICK_WIDTH * lngDur > KeyWidth * 0.75) {
                    // now get freq and attenuation
                    int lngFreq = EditSound[track][i].FreqDivisor;
                    int lngAtt = EditSound[track][i].Attenuation;
                    // if music note is zero, or attenuation is zero
                    if (lngFreq == 0 || lngAtt == 15) {
                        // draw a rest note
                        DrawRest(gs, track, lngHPos, lngDur);
                    }
                    else {
                        // convert note to MIDI and draw it
                        DrawMusicNote(gs, track, lngHPos, FreqDivToMidiNote(lngFreq), lngDur, lngAtt);
                    }
                }
                // calculate position of next note
                lngHPos = (int)(SOHorz + KeyWidth + NotePos(track, i + 1) * TICK_WIDTH * StaffScale);

                // if note is past visible area
                if (lngHPos > picStaff[track].Width) {
                    break;
                }
            }

            // clear clef area (adjust by two for offset, then one more for linewidth)
            brush = new(Color.White);
            gs.FillRectangle(brush, 0, 0, KeyWidth - 3, picStaff[track].Height);
            // redraw the staff lines
            x1 = 0;
            x2 = KeyWidth;
            for (int i = 0; i < 5; i++) {
                y1 = y2 = (96 + 6 * i) * StaffScale + SOVert[track];
                gs.DrawLine(pen, x1, y1, x2, y2);
                y1 = y2 = (156 + 6 * i) * StaffScale + SOVert[track];
                gs.DrawLine(pen, x1, y1, x2, y2);
            }

            // draw time marks at whole note intervals
            Font cleffont = new(staffFont.FontFamily, 5f * StaffScale, FontStyle.Regular);
            brush = new(Color.Black);
            y1 = 132 * StaffScale + SOVert[track];
            for (int i = 0; i <= lngSndLength; i++) {
                string strTime = ((float)i / 15 * lngTPQN).ToString("F2");
                // horizontal pos of marker
                float textwidth = gs.MeasureString(strTime, staffFont).Width;
                x1 = (int)(i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2 - textwidth / 2);
                // if past right edge
                if (x1 > picStaff[3].Width) {
                    break;
                }
                // if greater than 0 (not to left of visible window)
                if (x1 >= 0) {
                    gs.DrawString(strTime, staffFont, brush, x1, y1);
                }
            }

            // draw clefs
            gs.DrawImage(EditorResources.trebleclef, 3, 90 * StaffScale - 2 + SOVert[track], 16 * StaffScale, 41 * StaffScale);
            gs.DrawImage(EditorResources.bassclef, 3, 156 * StaffScale + 1 + SOVert[track], 16 * StaffScale, 20 * StaffScale);
            // add Key signature
            while (EditSound.Key != 0) {
                if (EditSound.Key > 0) {
                    // add f (-10,+2)
                    gs.DrawImage(EditorResources.noteflat, 20 * StaffScale, 89 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.noteflat, 20 * StaffScale, 149 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == 1) {
                        break;
                    }
                    // add c (-7, +5)
                    gs.DrawImage(EditorResources.noteflat, 26 * StaffScale, 98 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.noteflat, 26 * StaffScale, 158 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == 2) {
                        break;
                    }
                    // add g (-11, +1)
                    gs.DrawImage(EditorResources.noteflat, 32 * StaffScale, 86 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.noteflat, 32 * StaffScale, 146 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == 3) {
                        break;
                    }
                    // add d (-8,+4)
                    gs.DrawImage(EditorResources.noteflat, 38 * StaffScale, 95 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.noteflat, 38 * StaffScale, 155 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == 4) {
                        break;
                    }
                    // add a (-5,+7)
                    gs.DrawImage(EditorResources.noteflat, 44 * StaffScale, 104 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.noteflat, 44 * StaffScale, 164 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == 5) {
                        break;
                    }
                    // add e (-9,+3)
                    gs.DrawImage(EditorResources.noteflat, 50 * StaffScale, 92 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.noteflat, 50 * StaffScale, 152 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == 6) {
                        break;
                    }
                    // add b (-6,+6)
                    gs.DrawImage(EditorResources.noteflat, 56 * StaffScale, 101 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.noteflat, 56 * StaffScale, 161 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                }
                else {
                    // add b (-6, +6)
                    gs.DrawImage(EditorResources.notesharp, 20 * StaffScale, 98 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.notesharp, 20 * StaffScale, 158 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == -1) {
                        break;
                    }
                    // add e (-9, +3)
                    gs.DrawImage(EditorResources.notesharp, 26 * StaffScale, 89 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.notesharp, 26 * StaffScale, 149 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == -2) {
                        break;
                    }
                    // add a (-5, +7)
                    gs.DrawImage(EditorResources.notesharp, 32 * StaffScale, 101 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.notesharp, 32 * StaffScale, 161 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == -3) {
                        break;
                    }
                    // add d (-8, +4)
                    gs.DrawImage(EditorResources.notesharp, 38 * StaffScale, 92 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.notesharp, 38 * StaffScale, 152 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == -4) {
                        break;
                    }
                    // add g (-4, +8)
                    gs.DrawImage(EditorResources.notesharp, 44 * StaffScale, 104 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.notesharp, 44 * StaffScale, 164 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == -5) {
                        break;
                    }
                    // add c (-7, +5)
                    gs.DrawImage(EditorResources.notesharp, 50 * StaffScale, 95 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.notesharp, 50 * StaffScale, 155 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    if (EditSound.Key == -6) {
                        break;
                    }
                    // add f (-3, +9)
                    gs.DrawImage(EditorResources.notesharp, 56 * StaffScale, 107 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                    gs.DrawImage(EditorResources.notesharp, 56 * StaffScale, 167 * StaffScale + SOVert[track], StaffScale * 6, StaffScale * 14);
                }
                // always exit
                break;
            }

            if (SelectedTrack == track) {
                Pen borderpen = new(Color.Blue);
                borderpen.Width = 3;
                gs.DrawRectangle(borderpen, 1, 1, picStaff[track].ClientSize.Width - vsbStaff[track].Width - 3, picStaff[track].ClientSize.Height - 3);
            }
        }

        public void DrawNoiseStaff(Graphics gs) {
            SolidBrush brush = new(Color.Black);
            Pen pen = new(Color.Black);
            int x1, x2, y1, y2;
            int lngTPQN = EditSound.TPQN;
            int lngHPos;

            // sound length in whole note units
            int lngSndLength = (int)(EditSound.Length * 15 / lngTPQN);

            x1 = 0;
            x2 = picStaff[3].Width;
            for (int i = 0; i < 5; i++) {
                y1 = 11 + 6 * i * StaffScale + SOVert[3];
                gs.DrawLine(pen, x1, y1, x2, y1);
            }
            // draw time lines at whole note intervals, offset by two pixels
            y1 = 11 + SOVert[3];
            y2 = y1 + 27 * StaffScale;
            for (int i = 0; i <= lngSndLength; i++) {
                // horizontal pos of marker
                lngHPos = (int)(i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2);
                // if past right edge
                if (lngHPos > picStaff3.Width) {
                    break;
                }
                // if greater than 0 (not to left of visible window)
                if (lngHPos >= 0) {
                    gs.DrawLine(pen, lngHPos, y1, lngHPos, y2);
                }
            }
            // add notes
            // first note position
            lngHPos = SOHorz + KeyWidth;

            // step through all notes in this track
            for (int i = 0; i < EditSound[3].Notes.Count; i++) {
                // get duration first
                int lngDur = EditSound[3][i].Duration;
                // if note is visible,
                if (lngHPos + StaffScale * TICK_WIDTH * lngDur > KeyWidth * 0.75) {
                    // now get freq and attenuation
                    int lngFreq = EditSound[3][i].FreqDivisor;
                    int lngAtt = EditSound[3][i].Attenuation;
                    // draw noise note
                    DrawNoiseNote(gs, lngHPos, lngFreq, lngDur, lngAtt);
                }
                // calculate position of next note
                lngHPos = (int)(SOHorz + KeyWidth + NotePos(3, i + 1) * TICK_WIDTH * StaffScale);

                // if note is past visible area
                if (lngHPos > picStaff[3].Width) {
                    break;
                }
            }

            // clear clef area
            brush = new(Color.White);
            gs.FillRectangle(brush, new(0, 0, KeyWidth - 3, picStaff[3].Height));
            // redraw the staff lines
            brush = new(Color.Black);
            x1 = 0;
            x2 = KeyWidth;
            for (int i = 0; i < 5; i++) {
                y1 = 11 + 6 * i * StaffScale + SOVert[3];
                gs.DrawLine(pen, x1, y1, x2, y1);
            }
            // draw time marks at whole note intervals
            Font cleffont = new(staffFont.FontFamily, 5f * StaffScale, FontStyle.Regular);
            y1 = 11 + 27 * StaffScale + SOVert[3];
            for (int i = 0; i <= lngSndLength; i++) {
                string strTime = ((float)i / 15 * lngTPQN).ToString("F2");
                // horizontal pos of marker
                float textwidth = gs.MeasureString(strTime, staffFont).Width;
                x1 = (int)(i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2 - textwidth / 2);
                // if past right edge
                if (x1 > picStaff[3].Width) {
                    break;
                }
                // if greater than 0 (not to left of visible window)
                if (x1 >= 0) {
                    gs.DrawString(strTime, staffFont, brush, x1, y1);
                }
            }

            // draw noise clef
            x1 = 2 + 3 * StaffScale;
            y1 = 10 + SOVert[3];
            gs.DrawString("   2330", cleffont, brush, x1, y1);
            y1 = 10 + 6 * StaffScale + SOVert[3];
            gs.DrawString("   1165", cleffont, brush, x1, y1);
            y1 = 10 + 12 * StaffScale + SOVert[3];
            gs.DrawString("    583", cleffont, brush, x1, y1);
            y1 = 10 + 18 * StaffScale + SOVert[3];
            gs.DrawString("Track 2", cleffont, brush, x1, y1);

            if (SelectedTrack == 3) {
                Pen borderpen = new(Color.Blue);
                borderpen.Width = 3;
                gs.DrawRectangle(borderpen, 1, 1, picStaff[3].ClientSize.Width - vsbStaff[3].Width - 3, picStaff[3].ClientSize.Height - 3);
            }
        }

        private void DrawMusicNote(Graphics gs, int track, int HPos, int noteIndex, int length, int attenuation) {
            // draws note on staff;
            // track identifies which note to draw
            // HPos is horizontal position where note will be drawn
            // length is duration of note in AGI ticks
            // attenuation is volume attenuation; 0 means full sound; 15 means silence

            bool flipnote = false;
            int lngNoteTop, lngNPos;
            int lngDPos, lngAPos;
            int lngTPos, lngBPos;

            int lngTPQN = EditSound.TPQN;
            float x1, x2, y1, y2;
            Pen linepen = new(Color.Black);
            // set note color based on attenuation
            SolidBrush notebrush = new(EditPalette[attenuation]);
            Pen dotpen = new(EditPalette[attenuation]);

            // get note pos and accidental based on current key
            DisplayNote dnNote = DisplayNote(noteIndex, EditSound.Key);

            // draw extra staffline if above or below staff
            x1 = HPos - 3 * StaffScale;
            x2 = x1 + 12 * StaffScale;
            for (int i = -6; i >= dnNote.Pos / 2; i--) {
                y1 = y2 = (96 + 6 * (i + 5)) * StaffScale + SOVert[track];
                gs.DrawLine(linepen, x1, y1, x2, y2);
            }
            if (dnNote.Pos == 0) {
                x1 = HPos - 3 * StaffScale;
                x2 = x1 + 15 * StaffScale;
                y1 = y2 = 126 * StaffScale + SOVert[track];
                gs.DrawLine(linepen, x1, y1, x2, y2);
            }
            // based on note position, determine if it should be drawn rightside up or upside down
            // lngNoteTop is vertical offset on the Notes bitmap to the correctly oriented note
            // lngNPos is the absolute position on picScale where the bitmap needs to be placed
            // 
            // when drawing notes as blocks, drawing dots, or drawing accidentals,
            // lngNPos is adjusted by an amount that results in correct placement
            // lngDPos is used for the adjusted Value of dots;
            // lngAPos is used for the adjusted Value of accidentals, ties, blocks

            // if negative (meaning note is above middle c)
            if (dnNote.Pos <= 0) {
                // notes above middle B(vpos<-6) are drawn upsidedown
                if (dnNote.Pos < -6) {
                    flipnote = true;
                    lngNoteTop = 132;
                    // draw on treble staff
                    lngNPos = (123 + (3 * dnNote.Pos)) * StaffScale + SOVert[track];
                    // set position for dots, blocks, accidentals and ties
                    lngDPos = lngNPos + 2 * StaffScale;
                    lngBPos = lngDPos;
                    lngAPos = lngNPos - 4 * StaffScale;
                    lngTPos = lngNPos - 8 * StaffScale;
                }
                else {
                    flipnote = false;
                    lngNoteTop = 0;
                    // draw on treble staff
                    lngNPos = (107 + (3 * dnNote.Pos)) * StaffScale + SOVert[track];
                    // set position for dots, blocks, accidentals and ties
                    lngDPos = lngNPos + 18 * StaffScale;
                    lngBPos = lngDPos;
                    lngAPos = lngNPos + 12 * StaffScale;
                    lngTPos = lngNPos + 24 * StaffScale;
                }
            }
            else {
                // notes above middle B of bass staff(v<=6) are drawn upside down
                if (dnNote.Pos < 6) {
                    flipnote = true;
                    lngNoteTop = 133;
                    // draw on bass staff
                    lngNPos = (147 + (3 * dnNote.Pos)) * StaffScale + SOVert[track];
                    // set position for dots, blocks, accidentals and ties
                    lngDPos = lngNPos + 2 * StaffScale;
                    lngBPos = lngDPos;
                    lngAPos = lngNPos - 4 * StaffScale;
                    lngTPos = lngNPos - 8 * StaffScale;
                }
                else {
                    flipnote = false;
                    lngNoteTop = 0;
                    // draw on bass staff
                    lngNPos = (131 + (3 * dnNote.Pos)) * StaffScale + SOVert[track];
                    // set position for dots, blocks, accidentals and ties
                    lngDPos = lngNPos + 18 * StaffScale;
                    lngBPos = lngDPos;
                    lngAPos = lngNPos + 12 * StaffScale;
                    lngTPos = lngNPos + 24 * StaffScale;
                }
            }
            // if note is on a line,
            if (dnNote.Pos.IsEven()) {
                // dot needs to be moved off the line
                lngDPos -= 2 * StaffScale;
            }
            // if drawing notes as bitmaps
            if (ShowNotes) {
                // convert length of note to MIDI Value, using TPQN
                switch ((float)length / lngTPQN * 4) {
                case 1:
                    // sixteenth note
                    DrawNoteImage(gs, flipnote ? EditorResources.note16down : EditorResources.note16up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    break;
                case 2:
                    // eighth note
                    DrawNoteImage(gs, flipnote ? EditorResources.note8down : EditorResources.note8up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    break;
                case 3:
                    // eighth note dotted
                    DrawNoteImage(gs, flipnote ? EditorResources.note8down : EditorResources.note8up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 10, lngDPos, (float)(StaffScale * 1.25), (float)(StaffScale * 1.25));
                    break;
                case 4:
                    // quarter note
                    DrawNoteImage(gs, flipnote ? EditorResources.note4down : EditorResources.note4up,
                                 new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                 EditPalette[attenuation]);
                    break;
                case 5:
                    // quater note tied to sixteenth note
                    DrawNoteImage(gs, flipnote ? EditorResources.note4down : EditorResources.note4up,
                                 new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22), EditPalette[attenuation]);
                    // add accidental, if necessary
                    switch (dnNote.Tone) {
                    case NoteTone.Sharp:
                        DrawNoteImage(gs, EditorResources.notesharp,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Flat:
                        DrawNoteImage(gs, EditorResources.noteflat,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Natural:
                        DrawNoteImage(gs, EditorResources.notenatural,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    }
                    // draw connector
                    DrawNoteImage(gs, flipnote ? EditorResources.connectordown : EditorResources.connectorup,
                                  new Rectangle(HPos + StaffScale * 4, lngTPos, (int)(StaffScale * lngTPQN * TICK_WIDTH), StaffScale * 7),
                                  EditPalette[attenuation]);
                    // increment position
                    HPos += (int)(StaffScale * TICK_WIDTH * lngTPQN);
                    // draw sixteenth
                    DrawNoteImage(gs, flipnote ? EditorResources.note16down : EditorResources.note16up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    break;
                case 6:
                    // quarter note dotted
                    // draw quarter
                    DrawNoteImage(gs, flipnote ? EditorResources.note4down : EditorResources.note4up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 10, lngDPos, (float)(StaffScale * 2.5), (float)(StaffScale * 2.5));
                    break;
                case 7:
                    // quarter note double dotted
                    // draw quarter
                    DrawNoteImage(gs, flipnote ? EditorResources.note4down : EditorResources.note4up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    gs.DrawEllipse(dotpen, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 13, lngDPos, StaffScale * 2, StaffScale * 2);
                    gs.DrawEllipse(dotpen, HPos + StaffScale * 13, lngDPos, StaffScale * 2, StaffScale * 2);
                    break;
                case 8:
                    // half note
                    // draw half note
                    DrawNoteImage(gs, flipnote ? EditorResources.note2down : EditorResources.note2up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    break;
                case 9:
                    // half note tied to sixteenth
                    DrawNoteImage(gs, flipnote ? EditorResources.note2down : EditorResources.note2up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 2),
                                  EditPalette[attenuation]);
                    // add accidental, if necessary
                    switch (dnNote.Tone) {
                    case NoteTone.Sharp:
                        DrawNoteImage(gs, EditorResources.notesharp,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Flat:
                        DrawNoteImage(gs, EditorResources.noteflat,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Natural:
                        DrawNoteImage(gs, EditorResources.notenatural,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    }
                    // draw connector
                    DrawNoteImage(gs, flipnote ? EditorResources.connectordown : EditorResources.connectorup,
                                  new Rectangle(HPos + StaffScale * 4, lngTPos, (int)(StaffScale * 2 * lngTPQN * TICK_WIDTH), StaffScale * 7),
                                  EditPalette[attenuation]);
                    // increment position
                    HPos = (int)(StaffScale * TICK_WIDTH * 2 * lngTPQN);
                    // if note is on bottom, it trails past the staff mark; need to bump it back a little
                    if (lngNoteTop == 0) {
                        HPos -= (int)(StaffScale * TICK_WIDTH * 1.25);
                    }
                    // draw sixteenth
                    DrawNoteImage(gs, flipnote ? EditorResources.note16down : EditorResources.note16up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    break;
                case 10:
                    // half note tied to eighth
                    DrawNoteImage(gs, flipnote ? EditorResources.note2down : EditorResources.note2up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // add accidental, if necessary
                    switch (dnNote.Tone) {
                    case NoteTone.Sharp:
                        DrawNoteImage(gs, EditorResources.notesharp,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Flat:
                        DrawNoteImage(gs, EditorResources.noteflat,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Natural:
                        DrawNoteImage(gs, EditorResources.notenatural,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    }
                    // draw connector
                    DrawNoteImage(gs, flipnote ? EditorResources.connectordown : EditorResources.connectorup,
                                  new Rectangle(HPos + StaffScale * 4, lngTPos, (int)(StaffScale * 2 * lngTPQN * TICK_WIDTH), StaffScale * 7),
                                  EditPalette[attenuation]);
                    // increment position
                    HPos += (int)(StaffScale * TICK_WIDTH * 2 * lngTPQN);
                    // if note is on bottom, it trails past the staff mark; need to bump it back a little
                    if (lngNoteTop == 0) {
                        HPos -= (int)(StaffScale * TICK_WIDTH * 1.25);
                    }
                    // draw eighth
                    DrawNoteImage(gs, flipnote ? EditorResources.note8down : EditorResources.note8up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    break;
                case 11:
                    // half note tied to dotted eighth
                    DrawNoteImage(gs, flipnote ? EditorResources.note2down : EditorResources.note2up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // add accidental, if necessary
                    switch (dnNote.Tone) {
                    case NoteTone.Sharp:
                        DrawNoteImage(gs, EditorResources.notesharp,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Flat:
                        DrawNoteImage(gs, EditorResources.noteflat,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Natural:
                        DrawNoteImage(gs, EditorResources.notenatural,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    }
                    // draw connector
                    DrawNoteImage(gs, flipnote ? EditorResources.connectordown : EditorResources.connectorup,
                                  new Rectangle(HPos + StaffScale * 4, lngTPos, (int)(StaffScale * 2 * lngTPQN * TICK_WIDTH), StaffScale * 7),
                                  EditPalette[attenuation]);
                    // increment position
                    HPos += (int)(StaffScale * TICK_WIDTH * 2 * lngTPQN);
                    // if note is on bottom, it trails past the staff mark; need to bump it back a little
                    if (lngNoteTop == 0) {
                        HPos -= (int)(StaffScale * TICK_WIDTH * 1.25);
                    }
                    // draw eighth note
                    DrawNoteImage(gs, flipnote ? EditorResources.note8down : EditorResources.note8up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    gs.DrawEllipse(dotpen, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    break;
                case 12:
                    // half note dotted
                    DrawNoteImage(gs, flipnote ? EditorResources.note2down : EditorResources.note2up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    gs.DrawEllipse(dotpen, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    break;
                case 13:
                    // half note dotted tied to sixteenth
                    DrawNoteImage(gs, flipnote ? EditorResources.note2down : EditorResources.note2up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    gs.DrawEllipse(dotpen, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    // add accidental, if necessary
                    switch (dnNote.Tone) {
                    case NoteTone.Sharp:
                        DrawNoteImage(gs, EditorResources.notesharp,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Flat:
                        DrawNoteImage(gs, EditorResources.noteflat,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Natural:
                        DrawNoteImage(gs, EditorResources.notenatural,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    }
                    // draw connector
                    DrawNoteImage(gs, flipnote ? EditorResources.connectordown : EditorResources.connectorup,
                                  new Rectangle(HPos + StaffScale * 4, lngTPos, (int)(StaffScale * 3 * lngTPQN * TICK_WIDTH), StaffScale * 7),
                                  EditPalette[attenuation]);
                    // increment position
                    HPos += (int)(StaffScale * TICK_WIDTH * 3 * lngTPQN);
                    // if note is on bottom, it trails past the staff mark; need to bump it back a little
                    if (lngNoteTop == 0) {
                        HPos -= (int)(StaffScale * TICK_WIDTH * 1.25);
                    }
                    // draw sixteenth
                    DrawNoteImage(gs, flipnote ? EditorResources.note16down : EditorResources.note16up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    break;
                case 14:
                    // half note double dotted
                    DrawNoteImage(gs, flipnote ? EditorResources.note2down : EditorResources.note2up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    gs.DrawEllipse(dotpen, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 13, lngDPos, StaffScale * 2, StaffScale * 2);
                    gs.DrawEllipse(dotpen, HPos + StaffScale * 13, lngDPos, StaffScale * 2, StaffScale * 2);
                    break;
                case 15:
                    // half note double dotted tied to sixteenth
                    DrawNoteImage(gs, flipnote ? EditorResources.note2down : EditorResources.note2up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    gs.DrawEllipse(dotpen, HPos + StaffScale * 10, lngDPos, StaffScale * 2, StaffScale * 2);
                    // draw dot
                    gs.FillEllipse(notebrush, HPos + StaffScale * 13, lngDPos, StaffScale * 2, StaffScale * 2);
                    gs.DrawEllipse(dotpen, HPos + StaffScale * 13, lngDPos, StaffScale * 2, StaffScale * 2);
                    // add accidental, if necessary
                    switch (dnNote.Tone) {
                    case NoteTone.Sharp:
                        DrawNoteImage(gs, EditorResources.notesharp,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Flat:
                        DrawNoteImage(gs, EditorResources.noteflat,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    case NoteTone.Natural:
                        DrawNoteImage(gs, EditorResources.notenatural,
                                      new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                                      EditPalette[attenuation]);
                        break;
                    }
                    // draw connector
                    DrawNoteImage(gs, flipnote ? EditorResources.connectordown : EditorResources.connectorup,
                                  new Rectangle(HPos + StaffScale * 4, lngTPos, (int)(StaffScale * 3.5 * lngTPQN * TICK_WIDTH), StaffScale * 7),
                                  EditPalette[attenuation]);
                    // increment position
                    HPos += (int)(StaffScale * TICK_WIDTH * 3.5 * lngTPQN);
                    // if note is on bottom, it trails past the staff mark; need to bump it back a little
                    if (lngNoteTop == 0) {
                        HPos -= (int)(StaffScale * TICK_WIDTH * 1.25);
                    }
                    // draw sixteenth
                    DrawNoteImage(gs, flipnote ? EditorResources.note16down : EditorResources.note16up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    break;
                case 16:
                    // whole note
                    DrawNoteImage(gs, flipnote ? EditorResources.note1down : EditorResources.note1up,
                                  new Rectangle(HPos, lngNPos, StaffScale * 12, StaffScale * 22),
                                  EditPalette[attenuation]);
                    break;
                case > 16:
                    // greater than whole note;
                    // recurse to draw one whole note at a time until done
                    while (length >= 4 * lngTPQN) {
                        DrawMusicNote(gs, track, HPos, noteIndex, 4 * lngTPQN, attenuation);
                        // decrement length
                        length -= 4 * lngTPQN;
                        // draw connector if note continues
                        if (length > 0) {
                            DrawNoteImage(gs, flipnote ? EditorResources.connectordown : EditorResources.connectorup,
                                          new Rectangle(HPos + StaffScale * 4, lngTPos, (int)(StaffScale * 4 * lngTPQN * TICK_WIDTH), StaffScale * 7),
                                          EditPalette[attenuation]);
                        }
                        // increment horizontal position
                        HPos += (int)(StaffScale * TICK_WIDTH * lngTPQN * 4);
                        // special case- if EXACTLY one sixteenth note left AND on bottom
                        // bump it back a little
                        if ((length / lngTPQN * 4) == 1 && lngNoteTop == 0) {
                            HPos -= (int)(StaffScale * TICK_WIDTH * 1.25);
                        }
                    }
                    // if anything left
                    if (length > 0) {
                        // draw remaining portion of note
                        DrawMusicNote(gs, track, HPos, noteIndex, length, attenuation);
                    }
                    // exit
                    return;
                default:
                    // not a normal note; draw a bar
                    // this adjustment is interfering with the accidental position; need to reset lngNPos after drawing box
                    gs.FillRectangle(notebrush, HPos, lngBPos - 2 * StaffScale, (float)(StaffScale * TICK_WIDTH * (length - 0.8)), StaffScale * 6);
                    break;
                }
            }
            else {
                // draw the block for this note
                gs.FillRectangle(notebrush, HPos, lngBPos - StaffScale * 3, (float)(StaffScale * TICK_WIDTH * (length - 0.8)), StaffScale * 6);
            }
            // add accidental, if necessary
            switch (dnNote.Tone) {
            case NoteTone.Sharp:
                DrawNoteImage(gs, EditorResources.notesharp,
                              new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                              EditPalette[attenuation]);
                break;
            case NoteTone.Flat:
                DrawNoteImage(gs, EditorResources.noteflat,
                        new Rectangle(HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14),
                        EditPalette[attenuation]);
                break;
            case NoteTone.Natural:
                DrawNoteImage(gs, EditorResources.notenatural,
                              new Rectangle(HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14),
                              EditPalette[attenuation]);
                break;
            }
        }

        private void DrawNoiseNote(Graphics gs, int HPos, int note, int length, int attenuation) {
            // draws noise note on staff;
            // HPos is horizontal position where note will be drawn
            // Note is the freq of the note to be played
            // length is length of note is AGI ticks
            // attenuation is amount of volume attenuation (0 means loudest; 15 means mute)

            Brush brush;
            Pen pen;

            // set vertical position
            if (attenuation == 15) {
                int lngVPos = 11 + SOVert[3] + 3;
                // draw a box over entire staff
                brush = new SolidBrush(Color.LightGray);
                gs.FillRectangle(brush, HPos, lngVPos, (float)(StaffScale * TICK_WIDTH * (length - 0.3)), StaffScale * 24 - 6);
                pen = new(Color.Black);
                gs.DrawRectangle(pen, HPos, lngVPos, (float)(StaffScale * TICK_WIDTH * (length - 0.3)), StaffScale * 24 - 6);
            }
            else {
                int lngVPos = 11 + 6 * (note & 3) * StaffScale + SOVert[3] + 1;
                // if white noise
                if ((note & 4) == 4) {
                    // draw box with diagonal fill
                    brush = new HatchBrush(HatchStyle.LightUpwardDiagonal, EditPalette[attenuation], Color.White);
                    gs.FillRectangle(brush, HPos, lngVPos, (float)(StaffScale * TICK_WIDTH * (length - 0.3)), StaffScale * 6 - 2);
                }
                else {
                    // draw box with solid fill
                    brush = new SolidBrush(EditPalette[attenuation]);
                    gs.FillRectangle(brush, HPos, lngVPos, (float)(StaffScale * TICK_WIDTH * (length - 0.3)), StaffScale * 6 - 2);
                }
                pen = new(EditPalette[attenuation]);
                gs.DrawRectangle(pen, HPos, lngVPos, (float)(StaffScale * TICK_WIDTH * (length - 0.3)), StaffScale * 6 - 2);
            }
        }

        private void DrawRest(Graphics gs, int track, int HPos, int length) {
            // draws rest on staff;
            // HPos is horizontal position where note will be drawn
            // length is length of note is AGI ticks

            SolidBrush brush = new(Color.Black);

            int VPos, TrackCount;

            // get tpqn
            int lngTPQN = EditSound.TPQN;

            // convert length of note to MIDI Value, using TPQN
            // (one MIDI unit is a sixteenth note)
            float sngLen = length / lngTPQN * 4;

            // if this is noise track
            if (track == 3) {
                // notes are drawn only once
                TrackCount = 1;
                VPos = 16 * StaffScale + SOVert[track];
            }
            else {
                TrackCount = 2;
                VPos = 101 * StaffScale + SOVert[track];
            }
            int lngTempPos = HPos;
            for (int i = 1; i <= TrackCount; i++) {
                HPos = lngTempPos;
                // if showing notes
                if (ShowNotes) {
                    // draw appropriate rest
                    switch (sngLen) {
                    case 1:
                        // sixteenth rest
                        gs.DrawImage(EditorResources.rest16, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        break;
                    case 2:
                        // eighth rest
                        gs.DrawImage(EditorResources.rest8, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        break;
                    case 3:
                        // eighth rest dotted
                        gs.DrawImage(EditorResources.rest8, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 10, VPos + 10 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 10, VPos + 10 * StaffScale, StaffScale * 2, StaffScale * 2);
                        break;
                    case 4:
                        // quarter rest
                        gs.DrawImage(EditorResources.rest4, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        break;
                    case 5:
                        // quater rest and sixteenth rest
                        gs.DrawImage(EditorResources.rest4, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        // draw connector
                        gs.DrawImage(EditorResources.connectorup, new Rectangle(HPos + StaffScale * 3, VPos + StaffScale * 17, (int)(StaffScale * lngTPQN * TICK_WIDTH), StaffScale * 7));
                        // increment position
                        HPos += (int)(StaffScale * TICK_WIDTH * lngTPQN);
                        gs.DrawImage(EditorResources.rest16, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        break;
                    case 6:
                        // quarter rest dotted
                        gs.DrawImage(EditorResources.rest4, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 10, VPos + 10 * StaffScale, StaffScale * 2, StaffScale * 2);
                        break;
                    case 7:
                        // quarter rest double dotted
                        gs.DrawImage(EditorResources.rest4, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 10, VPos + 10 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 10, VPos + 10 * StaffScale, StaffScale * 2, StaffScale * 2);
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 13, VPos + 10 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 13, VPos + 10 * StaffScale, StaffScale * 2, StaffScale * 2);
                        break;
                    case 8:
                        // half rest
                        gs.FillRectangle(brush, HPos, VPos + 7 * StaffScale, 8 * StaffScale, -3 * StaffScale);
                        break;
                    case 9:
                        // half rest and sixteenth
                        gs.FillRectangle(brush, HPos, VPos + 7 * StaffScale, 8 * StaffScale, -3 * StaffScale);
                        // draw connector
                        gs.DrawImage(EditorResources.connectorup, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 2, StaffScale * 7);
                        // increment position
                        HPos += (int)(StaffScale * TICK_WIDTH * 2 * lngTPQN);
                        gs.DrawImage(EditorResources.rest16, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        break;
                    case 10:
                        // half rest and eighth
                        gs.FillRectangle(brush, HPos, VPos + 7 * StaffScale, 8 * StaffScale, -3 * StaffScale);
                        // draw connector
                        gs.DrawImage(EditorResources.connectorup, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 2, StaffScale * 7);
                        // increment position
                        HPos += (int)(StaffScale * TICK_WIDTH * 2 * lngTPQN);
                        gs.DrawImage(EditorResources.rest8, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        break;
                    case 11:
                        // half rest, eighth dotted
                        gs.FillRectangle(brush, HPos, VPos + 7 * StaffScale, 8 * StaffScale, -3 * StaffScale);
                        // draw connector
                        gs.DrawImage(EditorResources.connectorup, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 2, StaffScale * 7);
                        // increment position
                        HPos += (int)(StaffScale * TICK_WIDTH * 2 * lngTPQN);
                        gs.DrawImage(EditorResources.rest8, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 10, VPos + 10 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 10, VPos + 10 * StaffScale, StaffScale * 2, StaffScale * 2);
                        break;
                    case 12:
                        // half rest dotted
                        gs.FillRectangle(brush, HPos, VPos + 7 * StaffScale, 8 * StaffScale, -3 * StaffScale);
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 11, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 11, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        break;
                    case 13:
                        // half rest dotted and sixteenth
                        gs.FillRectangle(brush, HPos, VPos + 7 * StaffScale, 8 * StaffScale, -3 * StaffScale);
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 11, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 11, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        // draw connector
                        gs.DrawImage(EditorResources.connectorup, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 3, StaffScale * 7);
                        // increment position
                        HPos += (int)(StaffScale * TICK_WIDTH * 3 * lngTPQN);
                        gs.DrawImage(EditorResources.rest16, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        break;
                    case 14:
                        // half rest double dotted
                        gs.FillRectangle(brush, HPos, VPos + 7 * StaffScale, 8 * StaffScale, -3 * StaffScale);
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 11, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 11, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 14, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 14, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        break;
                    case 15:
                        // half rest double dotted and sixteenth
                        gs.FillRectangle(brush, HPos, VPos + 7 * StaffScale, 8 * StaffScale, -3 * StaffScale);
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 11, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 11, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        // draw dot
                        gs.FillEllipse(brush, HPos + StaffScale * 14, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        gs.DrawEllipse(new(Color.Black), HPos + StaffScale * 14, VPos + 4 * StaffScale, StaffScale * 2, StaffScale * 2);
                        // draw connector
                        gs.DrawImage(EditorResources.connectorup, HPos + StaffScale * 3, VPos + StaffScale * 17, (float)(StaffScale * lngTPQN * TICK_WIDTH * 3.5), StaffScale * 7);
                        // increment position
                        HPos += (int)(StaffScale * TICK_WIDTH * 3.5 * lngTPQN);
                        gs.DrawImage(EditorResources.rest16, new Rectangle(HPos, VPos, StaffScale * 12, StaffScale * 18));
                        break;
                    case 16:
                        // whole rest
                        gs.FillRectangle(brush, HPos, VPos + StaffScale, 7 * StaffScale, 4 * StaffScale);
                        break;
                    case > 16:
                        // greater than whole note;
                        // recurse to draw one whole note at a time until done
                        // ONLY do this once;
                        if (i == 1) {
                            while (!(length < 4 * lngTPQN)) {
                                DrawRest(gs, track, HPos, 4 * lngTPQN);
                                // decrement length
                                length -= 4 * lngTPQN;
                                if (length > 0) {
                                    // draw connectors
                                    gs.DrawImage(EditorResources.connectorup, HPos + StaffScale * 3, VPos + StaffScale * 17, (float)(StaffScale * lngTPQN * TICK_WIDTH * 4), StaffScale * 7);
                                    gs.DrawImage(EditorResources.connectorup, HPos + StaffScale * 3, VPos + StaffScale * 77, (float)(StaffScale * lngTPQN * TICK_WIDTH * 4), StaffScale * 7);
                                }
                                // increment horizontal position
                                HPos += (int)(StaffScale * TICK_WIDTH * lngTPQN * 4);
                            }
                            // if anything left
                            if (length > 0) {
                                // draw remaining portion of note
                                DrawRest(gs, track, HPos, length);
                            }
                        }
                        break;
                    default:
                        // not a normal note; draw a bar
                        brush = new SolidBrush(Color.LightGray);
                        gs.FillRectangle(brush, HPos, VPos - 2 * StaffScale, (float)(StaffScale * TICK_WIDTH * (length - 0.5)), StaffScale * 18);
                        // draw black border around bar
                        Pen pen = new(Color.Black);
                        gs.DrawRectangle(pen, HPos, VPos - 2 * StaffScale, (float)(StaffScale * TICK_WIDTH * (length - 0.5)), StaffScale * 18);
                        break;
                    }
                }
                else {
                    // draw all rest notes as blocks
                    brush = new SolidBrush(Color.LightGray);
                    gs.FillRectangle(brush, HPos, VPos - 2 * StaffScale, (float)(StaffScale * TICK_WIDTH * (length - 0.5)), StaffScale * 18);
                    // draw black border around bar
                    Pen pen = new(Color.Black);
                    gs.DrawRectangle(pen, HPos, VPos - 2 * StaffScale, (float)(HPos + StaffScale * TICK_WIDTH * (length - 0.5)), VPos - 2 * StaffScale + StaffScale * 18);
                }

                // reset vpos
                VPos = 161 * StaffScale + SOVert[track];
            }
        }

        private void HighlightSelection(Bitmap backBuffer, Graphics gs) {
            // only called if track is visible, and there are one
            // or more selected notes

            // get start pos and end pos in AGI ticks
            int startPos = (int)(EditSound[SelectedTrack].TimePos(SelStart) * TICK_WIDTH * StaffScale + SOHorz + KeyWidth);
            int endPos = (int)(EditSound[SelectedTrack].TimePos(SelStart + SelLength) * TICK_WIDTH * StaffScale + SOHorz + KeyWidth);

            // determine if selection is in view
            if (startPos >= picStaff[SelectedTrack].ClientSize.Width - 3 - vsbStaff[SelectedTrack].Width || endPos <= KeyWidth) {
                return;
            }
            // restrict to visible window
            if (startPos < KeyWidth) {
                startPos = KeyWidth;
            }
            // account for border (3 pixels) and scrollbar
            if (endPos > picStaff[SelectedTrack].ClientSize.Width - 3 - vsbStaff[SelectedTrack].Width) {
                endPos = picStaff[SelectedTrack].ClientSize.Width - 3 - vsbStaff[SelectedTrack].Width;
            }
            // invert the selection
            int x = startPos;
            int y = 3;
            int width = endPos - startPos;
            int height = picStaff[SelectedTrack].ClientSize.Height - 6;
            using (Bitmap selectionBmp = backBuffer.Clone(new Rectangle(x, y, width, height), backBuffer.PixelFormat)) {
                // Create a color matrix that inverts colors
                var invertMatrix = new ColorMatrix(
                [ [-1,  0,  0,  0, 0],
                  [0, -1,  0,  0, 0],
                  [0,  0, -1,  0, 0],
                  [0,  0,  0,  1, 0],
                  [1,  1,  1,  0, 1] ]);
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(invertMatrix);
                // Draw the inverted bitmap back onto the graphics surface
                gs.DrawImage(
                    selectionBmp,
                    new Rectangle(x, y, width, height),
                    0, 0, width, height,
                    GraphicsUnit.Pixel,
                    attributes
                );
            }
        }

        private void ShowCursor(Bitmap backBuffer, Graphics gs) {
            // when cursor is needed, it is drawn as a 2 pixel-wide bar that alternates
            // between black white?
            // get cursor pos in AGI ticks
            int startPos = (int)(EditSound[SelectedTrack].TimePos(SelStart) * TICK_WIDTH * StaffScale + SOHorz + KeyWidth);
            // determine if selection is in view
            if (startPos > picStaff[SelectedTrack].ClientSize.Width - 3 - vsbStaff[SelectedTrack].Width || startPos < KeyWidth) {
                return;
            }
            // invert the cursor location
            int x = startPos;
            int y = 3;
            int width = 2;
            int height = picStaff[SelectedTrack].ClientSize.Height - 6;
            using (Bitmap selectionBmp = backBuffer.Clone(new Rectangle(x, y, width, height), backBuffer.PixelFormat)) {
                // Create a color matrix that inverts colors
                var invertMatrix = new ColorMatrix(
                [ [-1,  0,  0,  0, 0],
                  [0, -1,  0,  0, 0],
                  [0,  0, -1,  0, 0],
                  [0,  0,  0,  1, 0],
                  [1,  1,  1,  0, 1] ]);
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(invertMatrix);
                // Draw the inverted bitmap back onto the graphics surface
                gs.DrawImage(
                    selectionBmp,
                    new Rectangle(x, y, width, height),
                    0, 0, width, height,
                    GraphicsUnit.Pixel,
                    attributes
                );
            }
        }

        private int NoteFromPos(int track, int pos, bool roundup = false) {
            // converts an X position into a note's index number for a given track

            // start at first note, offset by key width
            int lngHPos = SOHorz + KeyWidth;

            int lngNoteCount = EditSound[track].Notes.Count;
            // step through all notes in this track
            for (int i = 0; i < lngNoteCount; i++) {
                // get note dur
                int lngDur = EditSound[track][i].Duration;
                if (roundup) {
                    // if 1/2 of note extends past this position
                    if (lngHPos + (StaffScale * TICK_WIDTH * lngDur) / 2 > pos) {
                        // this is the note
                        return i;
                    }

                    // if not rounding,
                }
                else {
                    // if note extends past this position,
                    if (lngHPos + StaffScale * TICK_WIDTH * lngDur > pos) {
                        // this is the note
                        return i;
                    }
                }

                // calculate position of next note
                lngHPos += (int)(StaffScale * TICK_WIDTH * lngDur);
            }

            // if loop is exited normally, cursor is positioned at end of track
            return lngNoteCount;
        }

        private void SelectUnderMouse(int pos) {
            // get note number under cursor
            int tmpPos = NoteFromPos(SelectedTrack, pos, true);
            if (tmpPos >= SelAnchor) {
                if (tmpPos - SelAnchor == SelLength) {
                    // no change in selection
                    return;
                }
                // extend/compress selection (move end pt)
                SelectNote(SelectedTrack, SelAnchor, SelAnchor, tmpPos - SelAnchor, true, 0);
            }
            else {
                if (SelAnchor - tmpPos == SelLength) {
                    // no change in selection
                    return;
                }
                // extend/compress selection (move start pt)
                SelectNote(SelectedTrack, tmpPos, SelAnchor, SelAnchor - tmpPos, true, 0);
            }
        }

        private void UpdateStatusBarTime(double time) {
            if (time < 0) {
                time = 0;
            }
            spTime.Text = "Pos: " + time.ToString("F2") + " sec";
        }

        private DisplayNote DisplayNote(int MIDINote, int Key) {
            // returns a note position, relative to middle c
            // as either a positive or negative Value (negative meaning higher tone)
            // 
            // the Value is the offset needed to draw the note correctly
            // on the staff; one unit is a half the distance between staff lines
            // 
            // the returned Value is adjusted based on the key

            DisplayNote retval = new();

            if (MIDINote < 0 || MIDINote > 127) {
                throw new ArgumentOutOfRangeException();
            }

            // get octave and note Value
            int lngOctave = MIDINote / 12 - 1;
            int lngNote = MIDINote - ((lngOctave + 1) * 12);
            switch (lngNote) {
            case 0:
                //  "-C"
                if (Key == 7) {
                    retval.Pos = 1;
                    retval.Tone = NoteTone.None;
                }
                else {
                    retval.Pos = 0;
                    if (Key >= -5 && Key <= 1) {
                        retval.Tone = NoteTone.None;
                    }
                    else {
                        retval.Tone = NoteTone.Natural;
                    }
                }
                break;
            case 1:
                //  "-C#"
                if (Key >= 0) {
                    retval.Pos = 0;
                }
                else {
                    retval.Pos = -1;
                }
                switch (Key) {
                case 0 or 1:
                    retval.Tone = NoteTone.Sharp;
                    break;
                case -1 or -2 or -3:
                    retval.Tone = NoteTone.Flat;
                    break;
                default:
                    retval.Tone = NoteTone.None;
                    break;
                }
                break;
            case 2:
                //  "-D"
                retval.Pos = -1;
                switch (Key) {
                case > 3 or < -3:
                    retval.Tone = NoteTone.Natural;
                    break;
                default:
                    retval.Tone = NoteTone.None;
                    break;
                }
                break;
            case 3:
                //  "-D#"
                if (Key >= 0) {
                    retval.Pos = -1;
                }
                else {
                    retval.Pos = -2;
                }
                switch (Key) {
                case >= 0 and <= 3:
                    retval.Tone = NoteTone.Sharp;
                    break;
                case -1:
                    retval.Tone = NoteTone.Flat;
                    break;
                default:
                    retval.Tone = NoteTone.None;
                    break;
                }
                break;
            case 4:
                //  "-E"
                if (Key == -7) {
                    retval.Pos = -3;
                    retval.Tone = NoteTone.None;
                }
                else {
                    retval.Pos = -2;
                    if (Key >= -1 && Key <= 5) {
                        retval.Tone = NoteTone.None;
                    }
                    else {
                        retval.Tone = NoteTone.Natural;
                    }
                }
                break;
            case 5:
                //  "-F"
                if (Key >= 6) {
                    retval.Pos = -2;
                    retval.Tone = NoteTone.None;
                }
                else {
                    retval.Pos = -3;
                    if (Key <= 0 && Key >= -6) {
                        retval.Tone = NoteTone.None;
                    }
                    else {
                        retval.Tone = NoteTone.Natural;
                    }
                }
                break;
            case 6:
                //  "-F#"
                if (Key >= 0) {
                    retval.Pos = -3;
                }
                else {
                    retval.Pos = -4;
                }
                switch (Key) {
                case 0:
                    retval.Tone = NoteTone.Sharp;
                    break;
                case >= -4 and <= -1:
                    retval.Tone = NoteTone.Flat;
                    break;
                default:
                    retval.Tone = NoteTone.None;
                    break;
                }
                break;
            case 7:
                //  "-G"
                retval.Pos = -4;
                if (Key >= 3 || Key <= -5) {
                    retval.Tone = NoteTone.Natural;
                }
                else {
                    retval.Tone = NoteTone.None;
                }
                break;
            case 8:
                //  "-G#"
                if (Key >= 0) {
                    retval.Pos = -4;
                }
                else {
                    retval.Pos = -5;
                }
                switch (Key) {
                case 0 or 1 or 2:
                    retval.Tone = NoteTone.Sharp;
                    break;
                case -1 or -2:
                    retval.Tone = NoteTone.Flat;
                    break;
                default:
                    retval.Tone = NoteTone.None;
                    break;
                }
                break;
            case 9:
                //  "-A"
                retval.Pos = -5;
                if (Key >= 5 || Key <= -3) {
                    retval.Tone = NoteTone.Natural;
                }
                else {
                    retval.Tone = NoteTone.None;
                }
                break;
            case 10:
                //  "-A#"
                if (Key >= 0) {
                    retval.Pos = -5;
                }
                else {
                    retval.Pos = -6;
                }
                switch (Key) {
                case >= 0 and <= 4:
                    retval.Tone = NoteTone.Sharp;
                    break;
                default:
                    retval.Tone = NoteTone.None;
                    break;
                }
                break;
            case 11:
                //  "-B"
                if (Key <= -6) {
                    retval.Pos = -7;
                    retval.Tone = NoteTone.None;
                }
                else {
                    retval.Pos = -6;
                    if (Key <= -1 || Key == 7) {
                        retval.Tone = NoteTone.Natural;
                    }
                    else {
                        retval.Tone = NoteTone.None;
                    }
                }
                break;
            }

            // adust for octave
            retval.Pos = retval.Pos + (4 - lngOctave) * 7;
            return retval;
        }

        private void SetHScroll(int oldscale) {
            // Scrollbar math:
            // ACT_SZ = size of the area being scrolled; usually the image size + margins
            // WIN_SZ = size of the window area; the container's client size
            // SV_MAX = maximum value that scrollbar can have; this puts the scroll bar
            //          and scrolled image at farthest position
            // LG_CHG = LargeChange property of the scrollbar
            // SB_MAX = actual Maximum property of the scrollbar, to avoid out-of-bounds errors
            //
            //      SV_MAX = ACT_SZ - WIN_SZ 
            //      SB_MAX = SV_MAX + LG_CHG + 1

            int staffWidth = (int)(EditSound.Length * 60 * TICK_WIDTH * StaffScale) + KeyWidth + 24;

            // enable horizontal scrollbar if staff doesn't fit
            hsbStaff.Enabled = staffWidth > picStaff[0].ClientSize.Width - vsbStaff[0].Width;
            if (hsbStaff.Enabled) {
                // set change values
                // (LargeChange value can't exceed Max value, so set Max to high enough
                // value so it can be calculated correctly later)
                hsbStaff.Maximum = staffWidth;
                hsbStaff.LargeChange = (int)(picStaff[0].ClientSize.Width * LG_SCROLL);
                hsbStaff.SmallChange = (int)(picStaff[0].ClientSize.Width * SM_SCROLL);

                // calculate actual max (when staff is fully scrolled to right)
                int SV_MAX = staffWidth - (picStaff[0].ClientSize.Width - vsbStaff[0].Width);
                // control MAX value equals actual Max + LargeChange - 1
                hsbStaff.Maximum = SV_MAX + hsbStaff.LargeChange - 1;

                // adjust scrollbar value so left edge remains anchored
                // the correct algebra to make this work is:
                //         SB1 = SB0 + (SB0 + WAN - MGN) * (SF1 / SF0 - 1)
                // SB = scrollbar value
                // WAN = panel client window anchor point (get from cursor pos)
                // MGN is the left/top margin
                // SF = scale factor (as calculated above)
                // -0 = previous values
                // -1 = new (desired) values
                // WAN = 0 to keep anchor at left edge, MGN = 0;
                int newscroll = hsbStaff.Value + (hsbStaff.Value) * (StaffScale / oldscale - 1);
                if (newscroll < 0) {
                    newscroll = 0;
                }
                else if (newscroll > SV_MAX) {
                    newscroll = SV_MAX;
                }
                hsbStaff.Value = newscroll;
                SOHorz = -newscroll;
            }
            else {
                SOHorz = 0;
                hsbStaff.Maximum = 0;
                hsbStaff.Value = 0;
            }
        }

        private void SetVScroll(int index, int oldscale, int oldvalue) {
            // Scrollbar math:
            // ACT_SZ = size of the area being scrolled; usually the image size + margins
            // WIN_SZ = size of the window area; the container's client size
            // SV_MAX = maximum value that scrollbar can have; this puts the scroll bar
            //          and scrolled image at farthest position
            // LG_CHG = LargeChange property of the scrollbar
            // SB_MAX = actual Maximum property of the scrollbar, to avoid out-of-bounds errors
            //
            //      SV_MAX = ACT_SZ - WIN_SZ 
            //      SB_MAX = SV_MAX + LG_CHG + 1

            int staffHeight;
            if (index == 3) {
                staffHeight = 21 + 36 * StaffScale;
            }
            else {
                staffHeight = 10 + 186 * StaffScale;
            }
            vsbStaff[index].Enabled = staffHeight > picStaff[index].Height;
            if (vsbStaff[index].Enabled) {
                // set change values
                // (LargeChange value can't exceed Max value, so set Max to high enough
                // value so it can be calculated correctly later)
                vsbStaff[index].Maximum = staffHeight;
                vsbStaff[index].LargeChange = (int)(picStaff[index].ClientSize.Height * LG_SCROLL);
                vsbStaff[index].SmallChange = (int)(picStaff[index].ClientSize.Height * SM_SCROLL);

                // calculate actual max (when staff is fully scrolled up)
                int SV_MAX = staffHeight - picStaff[index].ClientSize.Height;
                // control MAX value equals actual Max + LargeChange - 1
                vsbStaff[index].Maximum = SV_MAX + vsbStaff[index].LargeChange - 1;
                // adjust scrollbar value so bottom edge remains anchored
                // the correct algebra to make this work is:
                //      SB1 = (SB0 + PW0) * (SF1 / SF0) - PW1
                // SB = scrollbar value
                // PW = panel client window width
                // SF = scale factor (as calculated above)
                // -0 = previous values
                // -1 = new (desired) values
                // if not previously visible (SB0 == -1), then
                // use the default starting position (time markers
                // at bottom of window)
                int newscroll;
                if (oldvalue == -1) {
                    if (index == 3) {
                        newscroll = SV_MAX;
                    }
                    else {
                        newscroll = SV_MAX - 42 * StaffScale;
                    }
                }
                else {
                    newscroll = (int)((oldvalue + oldstaffH[index]) * (float)StaffScale / oldscale - picStaff[index].ClientSize.Height);
                }
                if (newscroll < 0) {
                    newscroll = 0;
                }
                else if (newscroll > SV_MAX) {
                    newscroll = SV_MAX;
                }
                vsbStaff[index].Value = newscroll;
                SOVert[index] = -newscroll;
            }
            else {
                // use offset that puts staff at bottom
                SOVert[index] = picStaff[index].ClientSize.Height - staffHeight;
            }
        }

        private int NotePos(int track, int noteNumber) {

            // returns the timepostion of a note
            // track and NoteNumber should be validated BEFORE calling this function
            int retval = 0;
            // if looking for a note past end, return the end
            if (noteNumber > EditSound[track].Notes.Count) {
                noteNumber = EditSound[track].Notes.Count;
            }
            for (int i = 0; i < noteNumber; i++) {
                retval += EditSound[track][i].Duration;
            }
            return retval;
        }

        private void DrawKeyboard(Graphics g) {
            if (KeyboardVisible) {
                // draw keyboard keys, starting with A-3 (midi keyval 45)
                // and going up to G-10 (midi keyval 127)
                // to indicate a key is currently pressed, it is drawn in magenta
                // since the main keys (the white keys determine
                // the spacing, step through the visible white keys
                // and then add the black keys as needed
                // to calculate the midi value of each key, and 
                // determine where to draw the black keys, we need to
                // convert the white key offsets to a midi value.
                // keeping in mind that midi value of 60 is middle-C
                // (C-4), and that octaves change at the C note,
                // the calculation below results in the correct
                // midi value for each key.

                Pen pen = new(Color.Black);
                SolidBrush brush = new(Color.Black);
                Font font = this.Font; // new Font(this.Font.FontFamily, 8, FontStyle.Regular);
                Font fontBold = new Font(this.Font.FontFamily, 9, FontStyle.Bold | FontStyle.Italic);
                Font keyFont;
                int octave, noteindex, midivalue;

                if (SelectedTrack != 3) {
                    // draw white key outlines
                    for (int i = 0; i <= picKeyboard.Width / 24; i++) {
                        noteindex = (MKbOffset + i - 3) % 7;
                        octave = (MKbOffset + i - 5) / 7 - 2;
                        midivalue = 12 * octave + noteindex + 9;
                        // adjust to account for flats/sharps
                        switch (noteindex) {
                        case 1:
                            midivalue++;
                            break;
                        case 2:
                            midivalue -= 11;
                            break;
                        case 3:
                            midivalue -= 10;
                            break;
                        case 4 or 5:
                            midivalue -= 9;
                            break;
                        case 6:
                            midivalue -= 8;
                            break;
                        }
                        if (blnNoteOn && PlayNote == midivalue) {
                            brush = new(Color.Magenta);
                            g.FillRectangle(brush, i * 24, 0, 24, 65);
                        }
                        else {
                        }
                        // draw main key outline
                        g.DrawRectangle(pen, i * 24, 0, 24, 65);
                        // add key labels
                        string keyLabel = ((char)(noteindex + 65)).ToString();
                        if (octave == DefOctave) {
                            keyFont = fontBold;
                        }
                        else {
                            keyFont = font;
                        }

                        g.DrawString(keyLabel.ToString(), keyFont, Brushes.Black, i * 24 + 7, 48);
                        // if note is 'c'
                        if ((MKbOffset + i - 3) % 7 == 2) {
                            keyLabel = ((MKbOffset + i - 3) / 7 - 2).ToString();
                            g.DrawString(keyLabel, keyFont, Brushes.Black, i * 24 + 7, 34);
                        }
                        // don't draw above limit of keys
                        if (MKbOffset + i > 94) {
                            break;
                        }

                        // draw black keys if white key is A,B,,D,E,,G (skip C[2], F[5] 
                        if (((MKbOffset + i - 3) % 7) != 2 && ((MKbOffset + i - 3) % 7) != 5) {
                            if (blnNoteOn && PlayNote == midivalue - 1) {
                                brush = new(Color.Magenta);
                            }
                            else {
                                if (brush.Color != Color.Black) {
                                    brush = new(Color.Black);
                                }
                            }
                            // draw black key in front of this white key
                            g.FillRectangle(brush, i * 24 - 6, 0, 12, 32);
                        }
                        // don't draw above limit of keys
                        if (MKbOffset + i > 94) {
                            break;
                        }
                    }
                }
                else {
                    // draw noise keyboard
                    float lngKeyW = picKeyboard.Width / 8;
                    if (lngKeyW < MinNoiseKeyWidth) {
                        lngKeyW = MinNoiseKeyWidth;
                        // confirm offset is set correctly
                        if (NKbOffset > 8 * MinNoiseKeyWidth - picKeyboard.Width) {
                            NKbOffset = 8 * MinNoiseKeyWidth - picKeyboard.Width;
                        }
                    }
                    else {
                        // reset offset
                        NKbOffset = 0;
                    }
                    // draw white noise header with diagonal lines
                    HatchBrush hbrush = new HatchBrush(HatchStyle.LightUpwardDiagonal, Color.Black, Color.White);
                    SolidBrush wbrush = new(Color.White);
                    if (blnNoteOn) {
                        brush = new(Color.Magenta);
                        if (PlayNote == 7) {
                            g.FillRectangle(brush, PlayNote * lngKeyW - NKbOffset + 1, 33, picKeyboard.Width - (PlayNote * lngKeyW - NKbOffset + 1), 62);
                        }
                        else {
                            g.FillRectangle(brush, PlayNote * lngKeyW - NKbOffset + 1, 33, lngKeyW, 62);
                        }
                    }
                    brush = new(Color.Black);
                    g.FillRectangle(hbrush, lngKeyW * 4 - NKbOffset, 0, picKeyboard.Width - (lngKeyW * 4 - NKbOffset), 32);
                    // draw periodic tone header with white space
                    g.DrawRectangle(pen, 0, 0, picKeyboard.Width, 32);
                    g.DrawLine(pen, lngKeyW * 4 - NKbOffset, 0, lngKeyW * 4 - NKbOffset, 32);
                    // draw the vertical lines separating keys
                    for (int i = 0; i < 8; i++) {
                        g.DrawLine(pen, lngKeyW * i - NKbOffset, 32, lngKeyW * i - NKbOffset, 65);
                    }
                    // draw white space over noise header to hold text
                    g.FillRectangle(wbrush, lngKeyW * 6 - 38 - NKbOffset, 10, 80, 16);
                    // draw border at right edge
                    g.DrawLine(pen, picKeyboard.Width - 1, 0, picKeyboard.Width - 1, 65);
                    // draw top and bottom borders
                    g.DrawLine(pen, 0, 0, picKeyboard.Width, 0);
                    g.DrawLine(pen, 0, 65, picKeyboard.Width, 65);
                    // set x and Y to print header text
                    g.DrawString("PERIODIC TONE", font, brush, lngKeyW * 2 - 42 - NKbOffset, 10);
                    g.DrawString("WHITE NOISE", font, brush, lngKeyW * 6 - 36 - NKbOffset, 10);
                    // set x and Y to print key text
                    g.DrawString("2330", font, brush, lngKeyW / 2 - 16 - NKbOffset, 49);
                    g.DrawString("1165", font, brush, 3 * lngKeyW / 2 - 16 - NKbOffset, 49);
                    g.DrawString("583", font, brush, 5 * lngKeyW / 2 - 12 - NKbOffset, 49);
                    g.DrawString("TRK2", font, brush, 7 * lngKeyW / 2 - 15 - NKbOffset, 49);
                    g.DrawString("2330", font, brush, 9 * lngKeyW / 2 - 16 - NKbOffset, 49);
                    g.DrawString("1165", font, brush, 11 * lngKeyW / 2 - 16 - NKbOffset, 49);
                    g.DrawString("583", font, brush, 13 * lngKeyW / 2 - 12 - NKbOffset, 49);
                    g.DrawString("TRK2", font, brush, 15 * lngKeyW / 2 - 15 - NKbOffset, 49);
                }
            }
        }

        private void DrawDefaultNote() {
            // draw default notelength
            Font font = new(this.Font.FontFamily, 12f, FontStyle.Regular);
            SolidBrush brush = new(Color.Black);
            Pen pen = new(Color.Black);

            picDuration.Image = new Bitmap(picDuration.Width, picDuration.Height);
            Graphics g = Graphics.FromImage(picDuration.Image);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // add volume control icon
            g.DrawImage(EditorResources.esi_muteoff, 11, 0, 13, 18);
            // add volume controls
            g.DrawString("-", font, brush, 0, -3);
            g.DrawString("+", font, brush, 20, -3);

            // set color based on volume
            brush = new(EditPalette[DefAttn]);
            pen = new(EditPalette[DefAttn]);

            // if volume is not off AND not muting,
            if (DefAttn < 15 && !DefMute) {
                switch (DefLength) {
                case 1:
                    // sixteenth note
                    DrawNoteImage(g, EditorResources.note16up, new(4, 20, 24, 40), EditPalette[DefAttn]);
                    break;
                case 2:
                    // eighth note
                    DrawNoteImage(g, EditorResources.note8up, new(4, 20, 24, 40), EditPalette[DefAttn]);
                    break;
                case 3:
                    // eighth note dotted
                    DrawNoteImage(g, EditorResources.note8up, new(2, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 26, 53, 5, 5);
                    break;
                case 4:
                    // quarter note
                    DrawNoteImage(g, EditorResources.note4up, new(8, 20, 24, 40), EditPalette[DefAttn]);
                    break;
                case 5:
                    // quater note tied to sixteenth note
                    // draw quarter
                    DrawNoteImage(g, EditorResources.note4up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNoteImage(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw sixteenth
                    DrawNoteImage(g, EditorResources.note16up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    break;
                case 6:
                    // quarter note dotted
                    // draw quarter
                    DrawNoteImage(g, EditorResources.note4up, new(4, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 21, 51, 5, 5);
                    break;
                case 7:
                    // quarter note double dotted
                    // draw quarter
                    DrawNoteImage(g, EditorResources.note4up, new(3, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 19, 51, 5, 5);
                    // draw dot
                    g.FillEllipse(brush, 26, 51, 5, 5);
                    break;
                case 8:
                    // half note
                    DrawNoteImage(g, EditorResources.note2up, new(8, 20, 24, 40), EditPalette[DefAttn]);
                    break;
                case 9:
                    // half note tied to sixteenth
                    DrawNoteImage(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNoteImage(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw sixteenth
                    DrawNoteImage(g, EditorResources.note16up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    break;
                case 10:
                    // half note tied to eighth
                    DrawNoteImage(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNoteImage(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw eighth
                    DrawNoteImage(g, EditorResources.note8up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    break;
                case 11:
                    // half note tied to dotted eighth
                    DrawNoteImage(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNoteImage(g, EditorResources.connectorup, new(3, 49, 21, 6), EditPalette[DefAttn]);
                    // draw eighth note
                    DrawNoteImage(g, EditorResources.note8up, new(19, 25, 12, 22), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 29, 43, 3, 3);
                    break;
                case 12:
                    // half note dotted
                    DrawNoteImage(g, EditorResources.note2up, new(4, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 21, 51, 5, 5);
                    break;
                case 13:
                    // half note dotted tied to sixteenth
                    DrawNoteImage(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNoteImage(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw sixteenth
                    DrawNoteImage(g, EditorResources.note16up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 11, 53, 3, 3);
                    break;
                case 14:
                    // half note double dotted
                    DrawNoteImage(g, EditorResources.note2up, new(3, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 19, 51, 5, 5);
                    // draw dot
                    g.FillEllipse(brush, 26, 51, 5, 5);
                    break;
                case 15:
                    // half note double dotted tied to sixteenth
                    DrawNoteImage(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNoteImage(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw sixteenth
                    DrawNoteImage(g, EditorResources.note16up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 11, 40, 3, 3);
                    // draw dot
                    g.FillEllipse(brush, 15, 40, 3, 3);
                    break;
                case 16:
                    // whole note
                    DrawNoteImage(g, EditorResources.note1up, new(6, 22, 24, 22), EditPalette[DefAttn]);
                    break;
                }
            }
            else {
                // draw a rest
                switch (DefLength) {
                case 1:
                    // sixteenth rest
                    DrawNoteImage(g, EditorResources.rest16, new(12, 22, 18, 30), EditPalette[DefAttn]);
                    break;
                case 2:
                    // eighth rest
                    DrawNoteImage(g, EditorResources.rest8, new(12, 22, 18, 30), EditPalette[DefAttn]);
                    break;
                case 3:
                    // eighth rest dotted
                    DrawNoteImage(g, EditorResources.rest8, new(9, 22, 18, 30), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 21, 36, 5, 5);
                    break;
                case 4:
                    // quarter rest
                    DrawNoteImage(g, EditorResources.rest4, new(12, 22, 18, 30), EditPalette[DefAttn]);
                    break;
                case 5:
                    // quater rest and sixteenth rest
                    DrawNoteImage(g, EditorResources.rest4, new(8, 26, 12, 20), EditPalette[DefAttn]);
                    DrawNoteImage(g, EditorResources.rest16, new(20, 26, 12, 20), EditPalette[DefAttn]);
                    break;
                case 6:
                    // quarter rest dotted
                    DrawNoteImage(g, EditorResources.rest4, new(9, 22, 18, 30), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 21, 36, 5, 5);
                    break;
                case 7:
                    // quarter rest double dotted
                    DrawNoteImage(g, EditorResources.rest4, new(7, 22, 18, 30), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 17, 36, 5, 5);
                    // draw dot
                    g.FillEllipse(brush, 24, 36, 5, 5);
                    break;
                case 8:
                    // half rest
                    g.FillRectangle(brush, 8, 32, 17, 6);
                    g.DrawLine(pen, 5, 38, 27, 38);
                    break;
                case 9:
                    // half rest and sixteenth
                    g.FillRectangle(brush, 4, 32, 14, 5);
                    g.DrawLine(pen, 1, 37, 21, 37);
                    DrawNoteImage(g, EditorResources.rest16, new(24, 24, 12, 20), EditPalette[DefAttn]);
                    break;
                case 10:
                    // half rest and eighth
                    g.FillRectangle(brush, 4, 32, 14, 5);
                    g.DrawLine(pen, 1, 37, 21, 37);
                    DrawNoteImage(g, EditorResources.rest8, new(24, 24, 12, 20), EditPalette[DefAttn]);
                    break;
                case 11:
                    // half rest, eighth dotted
                    g.FillRectangle(brush, 4, 32, 14, 5);
                    g.DrawLine(pen, 1, 37, 21, 37);
                    DrawNoteImage(g, EditorResources.rest8, new(22, 24, 12, 20), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 30, 33, 4, 4);
                    break;
                case 12:
                    // half rest dotted
                    g.FillRectangle(brush, 6, 32, 17, 6);
                    g.DrawLine(pen, 3, 38, 25, 38);
                    // draw dot
                    g.FillEllipse(brush, 27, 32, 5, 5);
                    break;
                case 13:
                    // half rest dotted and sixteenth
                    g.FillRectangle(brush, 4, 32, 14, 5);
                    g.DrawLine(pen, 1, 37, 21, 37);
                    // draw dot
                    g.FillEllipse(brush, 21, 32, 4, 4);
                    DrawNoteImage(g, EditorResources.rest16, new(27, 24, 12, 20), EditPalette[DefAttn]);
                    break;
                case 14:
                    // half rest double dotted
                    g.FillRectangle(brush, 4, 32, 17, 6);
                    g.DrawLine(pen, 1, 38, 22, 38);
                    // draw dot
                    g.FillEllipse(brush, 24, 32, 4, 4);
                    // draw dot
                    g.FillEllipse(brush, 30, 32, 4, 4);
                    break;
                case 15:
                    // half rest double dotted and sixteenth
                    g.FillRectangle(brush, 3, 32, 14, 5);
                    g.DrawLine(pen, 0, 37, 19, 37);
                    // draw dot
                    g.FillEllipse(brush, 19, 33, 3, 3);
                    // draw dot
                    g.FillEllipse(brush, 24, 33, 3, 3);
                    DrawNoteImage(g, EditorResources.rest16, new(28, 26, 12, 20), EditPalette[DefAttn]);
                    break;
                case 16:
                    // whole rest
                    g.FillRectangle(brush, 10, 32, 15, 8);
                    g.DrawLine(pen, 6, 32, 28, 32);
                    break;
                }
            }

            // update tooltip
            defaultNoteTip.SetToolTip(picDuration,
                SoundEditMNote.MidiLengthConverter.CustomNames[DefLength - 1] +
                (DefMute ? ", Muted" : ", Attn: " + DefAttn));
        }

        private static void DrawNoteImage(Graphics g, Bitmap noteimage, Rectangle position, Color forecolor) {
            // Clone the bitmap to avoid modifying the original palette
            using (Bitmap tempImage = (Bitmap)noteimage.Clone()) {
                // Get the palette
                ColorPalette palette = tempImage.Palette;
                // Find the palette index for black (usually 0, but let's check)
                int blackIndex = -1;
                for (int i = 0; i < palette.Entries.Length; i++) {
                    if (palette.Entries[i].ToArgb() == Color.Black.ToArgb()) {
                        blackIndex = i;
                        break;
                    }
                }
                if (blackIndex >= 0) {
                    palette.Entries[blackIndex] = forecolor;
                    tempImage.Palette = palette;
                }
                // Draw the image with the updated palette
                g.DrawImage(tempImage, position);
            }
        }

        public void SetSoundKey(int newkey, bool DontUndo = false) {
            if (!DontUndo) {
                SoundUndo NextUndo = new();
                NextUndo.UDAction = ChangeKey;
                NextUndo.UDData = EditSound.Key;
                AddUndo(NextUndo);
            }
            EditSound.Key = newkey;
            SetKeyWidth();
            // update displayed staves
            for (int i = 0; i < 3; i++) {
                if (picStaff[i].Visible) {
                    picStaff[i].Invalidate();
                }
            }
            if (SelectionMode == SelectionModeType.Sound) {
                propertyGrid1.Refresh();
            }
        }

        public void SetSoundTPQN(int newtpqn, bool DontUndo = false) {
            if (!DontUndo) {
                SoundUndo NextUndo = new();
                NextUndo.UDAction = ChangeTPQN;
                NextUndo.UDData = EditSound.TPQN;
                AddUndo(NextUndo);
            }
            EditSound.TPQN = newtpqn;
            // force redraw of all tracks
            for (int i = 0; i < 4; i++) {
                if (picStaff[i].Visible) {
                    picStaff[i].Invalidate();
                }
            }
            if (SelectionMode == SelectionModeType.Sound) {
                propertyGrid1.Refresh();
            }
            // update displayed staves
            for (int i = 0; i < 3; i++) {
                if (picStaff[i].Visible) {
                    picStaff[i].Invalidate();
                }
            }
        }

        public void SetTrackVisibility(int track, bool value) {
            EditSound[track].Visible = value;
            UpdateVisibleStaves(StaffScale);
        }

        public void SetTrackMute(int track, bool value) {
            EditSound[track].Muted = value;
            if (value) {
                btnMute[track].Image = EditorResources.esi_muteon;
            }
            else {
                btnMute[track].Image = EditorResources.esi_muteoff;
            }
        }

        public void SetTrackInstrument(int track, byte newinst, bool DontUndo = false) {
            if (!DontUndo) {
                SoundUndo NextUndo = new();
                NextUndo.UDAction = ChangeInstrument;
                NextUndo.UDTrack = track;
                NextUndo.UDData = EditSound[track].Instrument;
                AddUndo(NextUndo);
            }
            EditSound[track].Instrument = newinst;

        }

        public void SetNoteFreqDivisor(int track, int note, int newfreqdiv, bool DontUndo = false) {
            if (!DontUndo) {
                SoundUndo NextUndo = new();
                NextUndo.UDAction = EditNoteFreqDiv;
                NextUndo.UDTrack = track;
                NextUndo.UDStart = note;
                NextUndo.UDData = EditSound[track].Notes[note].FreqDivisor;
                AddUndo(NextUndo);
            }
            EditSound[track].Notes[note].FreqDivisor = newfreqdiv;
            if (SelectionMode == SelectionModeType.MusicNote &&
                SelectedTrack == track &&
                SelStart == note) {
                propertyGrid1.Refresh();
            }
            if (picStaff[track].Visible) {
                picStaff[track].Invalidate();
            }
        }

        public void SetNoteDuration(int track, int notepos, int newdur, bool DontUndo = false) {
            if (!DontUndo) {
                SoundUndo NextUndo = new();
                NextUndo.UDAction = EditNoteDuration;
                NextUndo.UDTrack = track;
                NextUndo.UDStart = notepos;
                NextUndo.UDData = EditSound[track].Notes[notepos].Duration;
                AddUndo(NextUndo);
            }
            EditSound[track].Notes[notepos].Duration = newdur;
            if (SelectionMode == SelectionModeType.MusicNote &&
                SelectedTrack == track &&
                SelStart == notepos) {
                propertyGrid1.Refresh();
            }
            if (picStaff[track].Visible) {
                picStaff[track].Invalidate();
            }
        }

        public void SetNoteAttenuation(int track, int note, byte newattn, bool DontUndo = false) {
            if (!DontUndo) {
                SoundUndo NextUndo = new();
                NextUndo.UDAction = EditNoteAttenuation;
                NextUndo.UDTrack = track;
                NextUndo.UDStart = note;
                NextUndo.UDData = EditSound[track].Notes[note].Attenuation;
                AddUndo(NextUndo);
            }
            EditSound[track].Notes[note].Attenuation = newattn;
            if (SelectionMode == SelectionModeType.MusicNote &&
                SelectedTrack == track &&
                SelStart == note) {
                propertyGrid1.Refresh();
            }
            if (picStaff[track].Visible) {
                picStaff[track].Invalidate();
            }
        }

        private void NoteOff() {
            // turns off the note currently being played
            if (blnNoteOn) {
                // stop playing note
                blnNoteOn = false;
                switch (PlaybackMode) {
                case SoundPlaybackMode.PCSpeaker:
                    throw (new NotImplementedException());
                case SoundPlaybackMode.WAV:
                    try {
                        wavNotePlayer.StopWavNote();
                    }
                    catch {
                        throw;
                    }
                    break;
                case SoundPlaybackMode.MIDI:
                    try {
                        midiNotePlayer.StopMIDINote();
                    }
                    catch {
                        throw;
                    }
                    break;
                }
                // update keyboard
                if (KeyboardVisible) {
                    picKeyboard.Invalidate();
                }
                if (SelectedTrack >= 0 && SelStart != -1) {
                    InsertNote(PlayNote);
                }
            }
        }

        private void NoteOn(int Note) {
            // if a note is already playing
            if (blnNoteOn) {
                return;
            }
            blnNoteOn = true;
            PlayNote = Note;

            if (KeyboardVisible) {
                picKeyboard.Invalidate();
            }

            if (KeyboardSound && !PlayingSound) {
                // now play the note
                switch (PlaybackMode) {
                case SoundPlaybackMode.PCSpeaker:
                    throw new NotImplementedException();
                case SoundPlaybackMode.WAV:
                    try {
                        if (SelectedTrack == 3) {
                            wavNotePlayer.PlayWavNote(Note, SelectedTrack);
                        }
                        else {
                            wavNotePlayer.PlayWavNote(MidiNoteToFreqDiv(Note), SelectedTrack);
                        }
                    }
                    catch (Exception ex) {
                        MessageBox.Show("wavNotePlayer error: " + ex.Message);
                    }
                    break;
                case SoundPlaybackMode.MIDI:
                    try {
                        // instrument:
                        // tone noise: 0x6C0
                        // white noise: 0x7AC0
                        // music note: 0x##C0 (where ## is the instrument number)
                        int instrument;
                        if (SelectedTrack == 3) {
                            if (Note <= 3) {
                                // tone noise
                                instrument = 0x6C0;
                            }
                            else {
                                // white noise
                                instrument = 0x7AC0;
                            }
                        }
                        else {
                            // use selected track's instrument
                            instrument = EditSound[SelectedTrack].Instrument * 0x100 + 0xC0;
                        }
                        midiNotePlayer.PlayMIDINote(instrument, Note);
                    }
                    catch {
                        throw;
                    }
                    break;
                }
            }
        }

        private void InsertNote(int Note) {
            bool blnReplace = false;

            // if a selection needs to be deleted
            if (SelLength > 0) {
                // delete selection first
                DeleteNotes(SelectedTrack, SelStart, SelLength);
                blnReplace = true;
            }

            // add new note
            Notes AddNoteCol = new Notes();
            if (SelectedTrack != 3) {
                // convert note to freq divisor and add
                AddNoteCol.Add(MidiNoteToFreqDiv(Note), EditSound.TPQN * DefLength / 4, (byte)(DefMute ? 15 : DefAttn));
            }
            else {
                // note doesn't need conversion
                AddNoteCol.Add(Note, EditSound.TPQN * DefLength / 4, (byte)(DefMute ? 15 : DefAttn));
            }
            // add the note
            AddNotes(SelectedTrack, SelStart, AddNoteCol, false);

            // adjust undo so it displays 'add note'
            UndoCol.Peek().UDAction = AddNote;
            // if replacing,
            if (blnReplace) {
                // set flag in undo item
                UndoCol.Peek().UDText = "R";
            }
            picStaff[SelectedTrack].Invalidate();
        }

        private void AddNotes(int AddTrack, int InsertPos, Notes AddedNotes, bool SelectAll, bool DontUndo = false) {
            // adds notes to a track at a given position

            SoundUndo NextUndo = null;

            if (!DontUndo) {
                // save undo information
                NextUndo = new();
                NextUndo.UDAction = Paste;
                NextUndo.UDTrack = AddTrack;
                NextUndo.UDStart = InsertPos;
                NextUndo.UDLength = AddedNotes.Count;
                AddUndo(NextUndo);
            }

            // reference track node
            TreeNodeCollection tmpNodes = tvwSound.Nodes[0].Nodes[AddTrack].Nodes;

            // get number of notes currently in this track
            int lngCount = EditSound[AddTrack].Notes.Count;

            for (int i = 0; i < AddedNotes.Count; i++) {
                // insert note at this position
                EditSound[AddTrack].Notes.Add(AddedNotes[i].FreqDivisor, AddedNotes[i].Duration, AddedNotes[i].Attenuation, InsertPos + i);
                // add note placeholder to tree
                tmpNodes.Insert(InsertPos + i, "Note " + (InsertPos + i).ToString());
            }
            // update the text of the nodes after the inserted notes
            // (this causes significant slowdown if many notes are inserted, but
            // is necessary to keep the tree in sync with the sound data; so we
            // use update method to avoid redrawing the tree until all notes are added)
            tvwSound.BeginUpdate();
            for (int i = InsertPos + AddedNotes.Count; i < tmpNodes.Count - 1; i++) {
                tmpNodes[i].Text = "Note " + i;
            }
            tmpNodes[^1].Text = "End";
            tvwSound.EndUpdate();

            // readjust track length, staff scroll, if necessary
            SetHScroll(StaffScale);

            // set anchor, then change selection
            SelAnchor = SelStart;
            // if selecting inserted notes
            if (SelectAll) {
                SelectNote(AddTrack, InsertPos, InsertPos, AddedNotes.Count);
            }
            else {
                SelectNote(AddTrack, InsertPos + AddedNotes.Count, InsertPos + AddedNotes.Count, 0);
            }
        }

        private void DeleteSelection() {
            if (SelLength == 0) {
                if (SelStart < EditSound[SelectedTrack].Notes.Count) {
                    DeleteNotes(SelectedTrack, SelStart, 1);
                }
            }
            else {
                DeleteNotes(SelectedTrack, SelStart, SelLength);
            }
            SelectNote(SelectedTrack, SelStart, SelStart, 0);
        }

        private void DeleteNotes(int DelTrack, int DelPos, int DelCount, bool DontUndo = false) {
            // deletes notes at this position

            SoundUndo NextUndo = null;
            Notes DelNotes = null;
            // if not skipping undo
            if (!DontUndo) {
                NextUndo = new();
                NextUndo.UDAction = Delete;
                NextUndo.UDTrack = DelTrack;
                NextUndo.UDStart = DelPos;
                NextUndo.UDLength = DelCount;
                DelNotes = new();
            }

            // delete the selected notes
            for (int i = 0; i < DelCount; i++) {
                if (!DontUndo) {
                    // add this note to undolist
                    DelNotes.Add(EditSound[DelTrack][DelPos].FreqDivisor, EditSound[DelTrack][DelPos].Duration, EditSound[DelTrack][DelPos].Attenuation);
                }
                // delete the note
                EditSound[DelTrack].Notes.Remove(DelPos);
                // remove note from tree
                tvwSound.Nodes[0].Nodes[DelTrack].Nodes[DelPos].Remove();
            }

            if (!DontUndo) {
                NextUndo.UDNotes = DelNotes;
                AddUndo(NextUndo);
            }

            // renumber notes affected by deletion
            tvwSound.BeginUpdate();
            for (int i = DelPos; i < tvwSound.Nodes[0].Nodes[DelTrack].Nodes.Count - 1; i++) {
                tvwSound.Nodes[0].Nodes[DelTrack].Nodes[i].Text = "Note " + i.ToString();
            }
            tvwSound.Nodes[0].Nodes[DelTrack].Nodes[^1].Text = "End";
            tvwSound.EndUpdate();
            // update length
            SetHScroll(StaffScale);
            // adjust selection
            SelectNote(SelectedTrack, DelPos, DelPos, 0);
        }

        private void CopySelection() {
            // copy the selected notes to the clipboard
            // copy loop
            SoundClipboardData soundCB = new(SelectedTrack == 3 ? SoundCBTrackType.Noise : SoundCBTrackType.Tone);
            for (int i = 0; i < SelLength; i++) {
                // copy each note
                soundCB.Notes.Add(new(EditSound[SelectedTrack][SelStart + i]));
            }
            DataObject cbData = new(SOUND_CB_FMT, soundCB);
            Clipboard.SetDataObject(cbData, true);
        }

        private bool CanPaste(int pastetrack) {
            if (pastetrack != -1 && Clipboard.ContainsData(SOUND_CB_FMT)) {
                SoundClipboardData soundCBData = Clipboard.GetData(SOUND_CB_FMT) as SoundClipboardData;
                if (soundCBData is null) {
                    return false;
                }
                if (pastetrack == 3) {
                    // noise track can only paste noise data
                    return soundCBData.Type == SoundCBTrackType.Noise;
                }
                else if (pastetrack < 3) {
                    return soundCBData.Type == SoundCBTrackType.Tone;
                }
            }
            return false;
        }

        private void ShiftNoteTones(int Dir, int track, int startnote, int length = 0) {
            // shifts note tones up(dir=1) or down(dir=-1)

            // must have an active track, and an active selection
            if (track < 0 || track > 2 || startnote < 0 || length < 1) {
                return;
            }
            // set undo object
            SoundUndo NextUndo = new();
            NextUndo.UDAction = ShiftTone;
            NextUndo.UDTrack = track;
            NextUndo.UDStart = startnote;
            NextUndo.UDLength = length;
            NextUndo.UDNotes = [];
            // step through notes
            for (int i = 0; i < length; i++) {
                // save old note (only freq divisor is needed)
                int lngNew = EditSound[track][startnote + i].FreqDivisor;
                NextUndo.UDNotes.Add(lngNew, 0, 0);
                // shift note if possible
                lngNew = FreqDivToMidiNote(lngNew) + Math.Sign(Dir);
                // validate new note
                if (lngNew < 45) {
                    lngNew = 45;
                }
                if (lngNew > 127) {
                    lngNew = 127;
                }
                // special check for high notes that get skipped
                if (Math.Sign(Dir) == 1) {
                    // going up
                    if (lngNew == 121) {
                        lngNew = 122;
                    }
                    else if (lngNew == 124) {
                        lngNew = 125;
                    }
                    else if (lngNew == 126) {
                        lngNew = 127;
                    }
                }
                else {
                    // going down
                    if (lngNew == 126) {
                        lngNew = 125;
                    }
                    else if (lngNew == 124) {
                        lngNew = 123;
                    }
                    else if (lngNew == 121) {
                        lngNew = 120;
                    }
                }
                // update note
                EditSound[SelectedTrack][startnote + i].FreqDivisor = MidiNoteToFreqDiv(lngNew);
            }
            // add undo
            AddUndo(NextUndo);

            // update property grid for selected note
            if (length == 1 && (SelectionMode == SelectionModeType.MusicNote ||
                SelectionMode == SelectionModeType.NoiseNote) &&
                SelectedTrack == track && SelStart == startnote) {
                propertyGrid1.Refresh();
            }
            // refresh staff
            if (picStaff[SelectedTrack].Visible) {
                picStaff[SelectedTrack].Invalidate();
            }
        }

        private void ShiftNoteVol(int Dir, int track, int startnote, int length = 0) {
            // shifts note volume up(dir=1) or down(dir=-1)
            // NOTE: for vol up, attenuation is decreased; for vol down, attenuation
            // is increased. Also, track 3 is eligible for volume shifts

            // must have an active track, and an active selection
            if (track == -1 || startnote < 0 || length < 1) {
                return;
            }

            // set undo object
            SoundUndo NextUndo = new();
            NextUndo.UDAction = ShiftVol;
            NextUndo.UDTrack = track;
            NextUndo.UDStart = startnote;
            NextUndo.UDLength = length;
            NextUndo.UDNotes = [];

            // step through notes
            for (int i = 0; i < length; i++) {
                // save old note (only attenuation is needed
                int lngNew = EditSound[track][startnote + i].Attenuation;
                NextUndo.UDNotes.Add(0, 0, (byte)lngNew);
                lngNew -= Math.Sign(Dir);
                // validate
                if (lngNew < 0) {
                    lngNew = 0;
                }
                if (lngNew > 15) {
                    lngNew = 15;
                }
                // save note
                EditSound[track][startnote + i].Attenuation = (byte)lngNew;
            }
            // add undo
            AddUndo(NextUndo);

            // update property grid for selected note
            if (length == 1 && (SelectionMode == SelectionModeType.MusicNote ||
                SelectionMode == SelectionModeType.NoiseNote) &&
                SelectedTrack == track && SelStart == startnote) {
                propertyGrid1.Refresh();
            }
            // refresh staff
            if (picStaff[SelectedTrack].Visible) {
                picStaff[SelectedTrack].Invalidate();
            }
        }

        private void ShiftDuration(int Dir, int track, int notepos) {
            // shift (change) duration for selected note to new value
            // 
            // NOTE: this only works for a single note; can't adjust
            // duration of a group of notes

            // if at minimum and shortening, do nothing
            int midiLen = EditSound[track][notepos].Duration * 4 / EditSound.TPQN;
            if (midiLen <= 1 && Dir == -1) {
                return;
            }
            SoundUndo NextUndo = new();
            NextUndo.UDAction = EditNoteDuration;
            NextUndo.UDTrack = track;
            NextUndo.UDStart = notepos;
            NextUndo.UDData = EditSound[track][notepos].Duration;
            // make change
            EditSound[SelectedTrack][SelStart].Duration = (midiLen + Dir) * EditSound.TPQN / 4;
            // add undo
            AddUndo(NextUndo);
            // update property grid for selected note
            if ((SelectionMode == SelectionModeType.MusicNote ||
                SelectionMode == SelectionModeType.NoiseNote) &&
                SelectedTrack == track && SelStart == notepos) {
                propertyGrid1.Refresh();
            }
            // reset horizontal scrollbar
            SetHScroll(StaffScale);
            // refresh staff
            if (picStaff[SelectedTrack].Visible) {
                picStaff[SelectedTrack].Invalidate();
            }
        }

        public void ZoomScale(int Dir) {
            // adjusts zoom factor for display
            // positive dir increases scale; negative decreases scale
            int oldscale = StaffScale;

            // increment/decrement scale
            StaffScale += Math.Sign(Dir);
            // validate
            if (StaffScale == 0) {
                // reset and exit
                StaffScale = 1;
                return;
            }
            if (StaffScale == 4) {
                // reset and exit
                StaffScale = 3;
                return;
            }
            // update statusbar
            spScale.Text = "Scale: " + StaffScale;
            // resize font
            staffFont = new("Courier New", 8 * StaffScale);
            UpdateVisibleStaves(oldscale);
        }

        public void ImportSound(string importfile, SoundImportFormat format, SoundImportOptions options) {
            MDIMain.UseWaitCursor = true;
            Sound tmpSound = new();
            if (!Base.ImportSound(importfile, tmpSound, format, options)) {
                return;
            }
            if (tmpSound.SndFormat != SoundFormat.AGI) {
                MessageBox.Show(MDIMain,
                    "This is an Apple IIgs formatted sound.\n\nOnly PC/PCjr sounds can be edited in WinAGI.",
                    "Unsupported Sound Format",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            // copy resource data and basic sound parameters
            EditSound.ReplaceData(tmpSound.Data);
            EditSound.TPQN = tmpSound.TPQN;
            EditSound.Key = tmpSound.Key;
            // copy track data
            for (int i = 0; i < 4; i++) {
                EditSound[i].CloneFrom(tmpSound[i]);
            }
            MarkAsChanged();
            // reset tree then rebuild it
            for (int i = 0; i < 4; i++) {
                tvwSound.Nodes[0].Nodes[i].Nodes.Clear();
                tvwSound.Nodes[0].Nodes[i].Nodes.Add("End");
            }
            if (!BuildSoundTree()) {
                // error
                MessageBox.Show(MDIMain,
                    "This sound has corrupt or invalid data. No data loaded.",
                    "Sound Resource Data Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            for (int i = 0; i < 4; i++) {
                picStaff[i].Visible = EditSound[i].Visible;
                if (picStaff[i].Visible) {
                    picStaff[i].Invalidate();
                }
            }
            tvwSound.Nodes[0].ExpandAll();
            SelectSound();
            SetKeyWidth();
            MDIMain.UseWaitCursor = false;
        }

        public void SaveSound() {
            if (InGame) {
                MDIMain.UseWaitCursor = true;
                bool blnLoaded = EditGame.Sounds[SoundNumber].Loaded;
                if (!blnLoaded) {
                    EditGame.Sounds[SoundNumber].Load();
                }
                EditGame.Sounds[SoundNumber].CloneFrom(EditSound);
                EditGame.Sounds[SoundNumber].Save();
                if (!blnLoaded) {
                    EditGame.Sounds[SoundNumber].Unload();
                }
                RefreshTree(AGIResType.Sound, SoundNumber);
                if (WinAGISettings.AutoExport.Value) {
                    EditSound.Export(EditGame.SrcResDir + EditSound.ID + ".ags");
                    // reset ID (non-game id gets changed by export...)
                    EditSound.ID = EditGame.Sounds[SoundNumber].ID;
                }
                MarkAsSaved();
                MDIMain.UseWaitCursor = false;
            }
            else {
                if (EditSound.ResFile.Length == 0) {
                    ExportSound();
                    return;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    EditSound.Save();
                    MarkAsSaved();
                    MDIMain.UseWaitCursor = false;
                }
            }
        }

        private void ExportSound() {
            int retval = Base.ExportSound(EditSound, InGame);
            if (InGame) {
                // because EditSound is not the actual ingame sound its
                // ID needs to be reset back to the ingame value
                EditSound.ID = EditGame.Sounds[SoundNumber].ID;
            }
            else {
                if (retval == 1) {
                    MarkAsSaved();
                }
            }
        }

        public void ToggleInGame() {
            DialogResult rtn;
            string strExportName;
            bool blnDontAsk = false;

            if (InGame) {
                if (WinAGISettings.AskExport.Value) {
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Do you want to export '" + EditSound.ID + "// before removing it from your game?",
                        "Don't ask this question again",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        "Export Sound Before Removal", ref blnDontAsk);
                    WinAGISettings.AskExport.Value = !blnDontAsk;
                    if (!WinAGISettings.AskExport.Value) {
                        WinAGISettings.AskExport.WriteSetting(WinAGISettingsFile);
                    }
                }
                else {
                    // dont ask; assume no
                    rtn = DialogResult.No;
                }
                switch (rtn) {
                case DialogResult.Cancel:
                    return;
                case DialogResult.Yes:
                    // get a filename for the export
                    strExportName = NewResourceName(EditSound, InGame);
                    if (strExportName.Length > 0) {
                        EditSound.Export(strExportName);
                    }
                    break;
                case DialogResult.No:
                    // nothing to do
                    break;
                }
                // confirm removal
                if (WinAGISettings.AskRemove.Value) {
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Removing '" + EditSound.ID + "// from your game.\n\nSelect OK to proceed, or Cancel to keep it in game.",
                        "Remove Sound From Game",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        "Don't ask this question again", ref blnDontAsk);
                    WinAGISettings.AskRemove.Value = !blnDontAsk;
                    if (!WinAGISettings.AskRemove.Value) {
                        WinAGISettings.AskRemove.WriteSetting(WinAGISettingsFile);
                    }
                }
                else {
                    rtn = DialogResult.OK;
                }
                if (rtn == DialogResult.Cancel) {
                    return;
                }
                // remove the sound (force-closes this editor)
                RemoveSound((byte)SoundNumber);
            }
            else {
                // add to game 
                if (EditGame is null) {
                    return;
                }
                using frmGetResourceNum frmGetNum = new(GetRes.AddInGame, AGIResType.Sound, 0);
                if (frmGetNum.ShowDialog(MDIMain) != DialogResult.Cancel) {
                    SoundNumber = frmGetNum.NewResNum;
                    // change id before adding to game
                    EditSound.ID = frmGetNum.txtID.Text;
                    AddNewSound((byte)SoundNumber, EditSound);
                    EditGame.Sounds[SoundNumber].Load();
                    // copy the sound back (to ensure internal variables are copied)
                    EditSound.CloneFrom(EditGame.Sounds[SoundNumber]);
                    // now we can unload the newly added sound;
                    EditGame.Sounds[SoundNumber].Unload();
                    MarkAsSaved();
                    InGame = true;
                    MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[20];
                    MDIMain.btnAddRemove.Text = "Remove Sound";
                }
            }
        }

        public void RenumberSound() {
            if (!InGame) {
                return;
            }
            string oldid = EditSound.ID;
            int oldnum = SoundNumber;
            byte NewResNum = GetNewNumber(AGIResType.Sound, (byte)SoundNumber);
            if (NewResNum != SoundNumber) {
                // update ID (it may have changed if using default ID)
                EditSound.ID = EditGame.Sounds[NewResNum].ID;
                SoundNumber = NewResNum;
                Text = sPICED + ResourceName(EditSound, InGame, true);
                if (IsChanged) {
                    Text = CHG_MARKER + Text;
                }
                if (EditSound.ID != oldid) {
                    if (File.Exists(EditGame.SrcResDir + oldid + ".agp")) {
                        SafeFileMove(EditGame.SrcResDir + oldid + ".agp", EditGame.SrcResDir + EditGame.Sounds[NewResNum].ID + ".agp", true);
                    }
                }
            }
        }

        public void EditSoundProperties(int FirstProp) {
            string id = EditSound.ID;
            string description = EditSound.Description;
            if (GetNewResID(AGIResType.Sound, SoundNumber, ref id, ref description, InGame, FirstProp)) {
                if (EditSound.Description != description) {
                    EditSound.Description = description;
                }
                if (EditSound.ID != id) {
                    EditSound.ID = id;
                    Text = sSNDED + ResourceName(EditSound, InGame, true);
                    if (IsChanged) {
                        Text = CHG_MARKER + Text;
                    }
                }
            }
        }
        
        internal void ShowHelp() {
            string topic = "htm\\winagi\\editor.htm";

            // TODO: add context-sensitive help
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
        }

        private bool AskClose() {
            if (EditSound.Error != ResourceErrorType.NoError) {
                // if exiting due to error on form load
                return true;
            }
            if (SoundNumber == -1) {
                // force shutdown
                return true;
            }
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to this sound resource?",
                    "Save Sound Resource",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SaveSound();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "Resource not saved. Continue closing anyway?",
                            "Save Sound Resource",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        return rtn == DialogResult.Yes;
                    }
                    break;
                case DialogResult.Cancel:
                    return false;
                case DialogResult.No:
                    break;
                }
            }
            return true;
        }

        private void AddUndo(SoundUndo NextUndo) {
            if (!IsChanged) {
                MarkAsChanged();
            }
            // adds the next undo object
            UndoCol.Push(NextUndo);
        }

        private void MarkAsChanged() {
            // ignore when loading (not visible yet)
            if (!Visible) {
                return;
            }
            if (!IsChanged) {
                IsChanged = true;
                mnuRSave.Enabled = true;
                MDIMain.btnSaveResource.Enabled = true;
                Text = CHG_MARKER + Text;
            }
        }

        private void MarkAsSaved() {
            IsChanged = false;
            Text = sSNDED + ResourceName(EditSound, InGame, true);
            mnuRSave.Enabled = false;
            MDIMain.btnSaveResource.Enabled = false;
        }
        #endregion
    }

    public class SoundEditSound {
        private frmSoundEdit parent;
        private Sound sound;

        public SoundEditSound(frmSoundEdit parent) {
            this.parent = parent;
            this.sound = parent.EditSound;
        }
        public string ID {
            get { return sound.ID; }
        }
        public string Description {
            get { return sound.Description; }
        }
        [TypeConverter(typeof(KeySignatureConverter))]
        public KeySignature Key {
            get { return (KeySignature)sound.Key + 7; }
            set {
                if ((int)value != sound.Key + 7) {
                    parent.SetSoundKey((int)value - 7);
                }
            }
        }
        public int TPQN {
            get { return sound.TPQN; }
            set {
                // validate
                if (value < 4) {
                    value = 4;
                }
                else if (value > 64) {
                    value = 64;
                }
                value = value / 4 * 4;
                if (value != sound.TPQN) {
                    parent.SetSoundTPQN(value);
                }
            }
        }
        public double Length {
            get { return Math.Round(sound.Length, 2); }
        }
        public enum KeySignature {
            CFlatMajor,
            GFlatMajor,
            DFlatMajor,
            AFlatMajor,
            EFlatMajor,
            BFlatMajor,
            FMajor,
            CMajor,
            GMajor,
            DMajor,
            AMajor,
            EMajor,
            BMajor,
            FSharpMajor,
            CSharpMajor
        }
        public class KeySignatureConverter : EnumConverter {
            private static readonly string[] CustomNames = new[]
            {
                "Cb Major", "Gb Major", "Db Major", "Ab Major",
                "Eb Major", "Bb Major", "F Major", "C Major",
                "G Major", "D Major", "A Major", "E Major",
                "B Major", "F# Major", "C# Major"
            };

            public KeySignatureConverter() : base(typeof(KeySignature)) { }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == typeof(string) && value is KeySignature note) {
                    int index = (int)note;
                    if (index >= 0 && index < CustomNames.Length)
                        return CustomNames[index];
                    return note.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                if (value is string s) {
                    int idx = Array.IndexOf(CustomNames, s);
                    if (idx >= 0)
                        return (KeySignature)idx;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                return new StandardValuesCollection(Enum.GetValues(typeof(KeySignature)));
            }
        }
    }

    public class SoundEditMTrack {
        frmSoundEdit parent;
        Track track;
        int index;
        public SoundEditMTrack(frmSoundEdit parent, int track, int index) {
            this.parent = parent;
            this.track = parent.EditSound[track];
            this.index = index;
        }
        [TypeConverter(typeof(InstrumentsConverter))]
        public Instruments Instrument {
            get { return (Instruments)track.Instrument; }
            set {
                if ((int)value < 0) {
                    value = (Instruments)0;
                }
                if ((int)value > 127) {
                    value = (Instruments)127;
                }
                parent.SetTrackInstrument(index, (byte)value);
            }
        }
        public bool Muted {
            get { return track.Muted; }
            set {
                parent.SetTrackMute(index, value);
            }
        }
        public bool Visible {
            get { return track.Visible; }
            set {
                parent.SetTrackVisibility(index, value); 
            }
        }
        public int NoteCount {
            get { return track.Notes.Count; }
        }
        public enum Instruments {
            _0, _1, _2, _3, _4, _5, _6, _7, _8, _9,
            _10, _11, _12, _13, _14, _15, _16, _17, _18, _19,
            _20, _21, _22, _23, _24, _25, _26, _27, _28, _29,
            _30, _31, _32, _33, _34, _35, _36, _37, _38, _39,
            _40, _41, _42, _43, _44, _45, _46, _47, _48, _49,
            _50, _51, _52, _53, _54, _55, _56, _57, _58, _59,
            _60, _61, _62, _63, _64, _65, _66, _67, _68, _69,
            _70, _71, _72, _73, _74, _75, _76, _77, _78, _79,
            _80, _81, _82, _83, _84, _85, _86, _87, _88, _89,
            _90, _91, _92, _93, _94, _95, _96, _97, _98, _99,
            _100, _101, _102, _103, _104, _105, _106, _107, _108, _109,
            _110, _111, _112, _113, _114, _115, _116, _117, _118, _119,
            _120, _121, _122, _123, _124, _125, _126, _127,
        }
        public class InstrumentsConverter : EnumConverter {
            public static readonly string[] CustomNames = new[]
            {
                "Acoustic grand piano",
                "Bright acoustic piano",
                "Electric grand piano",
                "Honky-tonk piano",
                "Rhodes piano",
                "Chorused piano",
                "Harpsichord",
                "Clavinet",
                "Celesta",
                "Glockenspiel",
                "Music box",
                "Vibraphone",
                "Marimba",
                "Xylophone",
                "Tubular bells",
                "Dulcimer",
                "Hammond organ",
                "Percussive organ",
                "Rock organ",
                "Church organ",
                "Reed organ",
                "Accordion",
                "Harmonica",
                "Tango accordion",
                "Acoustic guitar (nylon)",
                "Acoustic guitar (steel)",
                "Acoustic guitar (jazz)",
                "Acoustic guitar (clean)",
                "Acoustic guitar (muted)",
                "Overdriven guitar",
                "Distortion guitar",
                "Guitar harmonics",
                "Acoustic bass",
                "Electric bass (finger)",
                "Electric bass (pick)",
                "Fretless bass",
                "Slap bass 1",
                "Slap bass 2",
                "Synth bass 1",
                "Synth bass 2",
                "Violin",
                "Viola",
                "Cello",
                "Contrabass",
                "Tremolo strings",
                "Pizzicato strings",
                "Orchestral harp",
                "Timpani",
                "String ensemble 1",
                "String ensemble 2",
                "Synth. strings 1",
                "Synth. strings 2",
                "Choir Aahs",
                "Voice Oohs",
                "Synth voice",
                "Orchestra hit",
                "Trumpet",
                "Trombone",
                "Tuba",
                "Muted trumpet",
                "French horn",
                "Brass section",
                "Synth. brass 1",
                "Synth. brass 2",
                "Soprano sax",
                "Alto sax",
                "Tenor sax",
                "Baritone sax",
                "Oboe",
                "English horn",
                "Bassoon",
                "Clarinet",
                "Piccolo",
                "Flute",
                "Recorder",
                "Pan flute",
                "Bottle blow",
                "Shakuhachi",
                "Whistle",
                "Ocarina",
                "Lead 1 (square)",
                "Lead 2 (sawtooth)",
                "Lead 3 (calliope lead)",
                "Lead 4 (chiff lead)",
                "Lead 5 (charang)",
                "Lead 6 (voice)",
                "Lead 7 (fifths)",
                "Lead 8 (brass + lead)",
                "Pad 1 (new age)",
                "Pad 2 (warm)",
                "Pad 3 (polysynth)",
                "Pad 4 (choir)",
                "Pad 5 (bowed)",
                "Pad 6 (metallic)",
                "Pad 7 (halo)",
                "Pad 8 (sweep)",
                "FX 1 (rain)",
                "FX 2 (soundtrack)",
                "FX 3 (crystal)",
                "FX 4 (atmosphere)",
                "FX 5 (brightness)",
                "FX 6 (goblins)",
                "FX 7 (echoes, drops)",
                "FX 8 (sci-fi, star theme)",
                "Sitar",
                "Banjo",
                "Shamisen",
                "Koto",
                "Kalimba",
                "Bag pipe",
                "Fiddle",
                "Shanai",
                "Tinkle Bell",
                "Agogo",
                "Steel Drums",
                "Woodblock",
                "Taiko Drum",
                "Melodic Tom",
                "Synth Drum",
                "Reverse Cymbal",
                "Guitar fret noise",
                "Breath noise",
                "Seashore",
                "Bird tweet",
                "Telphone ring",
                "Helicopter",
                "Applause",
                "Gunshot",
            };

            public InstrumentsConverter() : base(typeof(Instruments)) { }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == typeof(string) && value is Instruments inst) {
                    int index = (int)inst;
                    if (index >= 0 && index < CustomNames.Length)
                        return CustomNames[index];
                    return inst.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                if (value is string s) {
                    int idx = Array.IndexOf(CustomNames, s);
                    if (idx >= 0)
                        return (Instruments)idx;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                return new StandardValuesCollection(Enum.GetValues(typeof(Instruments)));
            }
        }
    }

    public class SoundEditNTrack {
        frmSoundEdit parent;
        Track track;
        public SoundEditNTrack(frmSoundEdit parent) {
            this.parent = parent;
            this.track = parent.EditSound[3];
        }
        public bool Muted {
            get { return track.Muted; }
            set {
                parent.SetTrackMute(3, value);
            }
        }
        public bool Visible {
            get { return track.Visible; }
            set {
                parent.SetTrackVisibility(3, value);
            }
        }
        public int NoteCount {
            get { return track.Notes.Count; }
        }
    }

    public class SoundEditMNote {
        frmSoundEdit parent;
        int track;
        int noteindex;
        Note note;
        public SoundEditMNote(frmSoundEdit parent, int track, int note) {
            this.parent = parent;
            this.track = track;
            noteindex = note;
            this.note = parent.EditSound[track].Notes[note];
        }
        public int FreqDiv {
            get { return note.FreqDivisor; }
            set {
                if (value < 1) {
                    value = 1;
                }
                else if (value > 1023) {
                    value = 1023;
                }
                if (value != note.FreqDivisor) {
                    parent.SetNoteFreqDivisor(track, noteindex, value);
                }
            }
        }
        public int Duration {
            get { return note.Duration; }
            set {
                if (value < 1) {
                    value = 1;
                }
                else if (value > short.MaxValue) {
                    value = short.MaxValue;
                }
                if (value != note.Duration) {
                    parent.SetNoteDuration(track, noteindex, value);
                }
            }
        }

        public byte Attenuation {
            get { return note.Attenuation; }
            set {
                if (value < 0) {
                    value = 0;
                }
                else if (value > 15) {
                    value = 15;
                }
                if (value != note.Attenuation) {
                    parent.SetNoteAttenuation(track, noteindex, value);
                }
            }
        }

        [TypeConverter(typeof(MidiNoteConverter))]
        public EMidiNote MidiNote {
            get { return (EMidiNote)(FreqDivToMidiNote(note.FreqDivisor) % 12); }
            set {
                if (value != (EMidiNote)(FreqDivToMidiNote(note.FreqDivisor) % 12)) {
                    // convert midi note to AGI freqdivisor
                    int octave = FreqDivToMidiNote(note.FreqDivisor) / 12;
                    parent.SetNoteFreqDivisor(track, noteindex, MidiNoteToFreqDiv(octave * 12 + (int)value));
                }
            }
        }
        [TypeConverter(typeof(MidiOctaveConverter))]
        public EMidiOctave MidiOctave {
            get { return (EMidiOctave)((FreqDivToMidiNote(note.FreqDivisor) / 12) - 3); }
            set {
                if ((int)value != FreqDivToMidiNote(note.FreqDivisor) / 12 - 3) {
                    int mnote = FreqDivToMidiNote(note.FreqDivisor) % 12;
                    parent.SetNoteFreqDivisor(track, noteindex, MidiNoteToFreqDiv(((int)value + 3) * 12 + mnote));
                }
            }
        }
        [TypeConverter(typeof(MidiLengthConverter))]
        public EMidiLength MidiLength {
            get {
                double tmpDur = (double)note.Duration / parent.EditSound.TPQN * 4;
                if (tmpDur == (int)tmpDur && tmpDur <= 16 && tmpDur > 0) {
                    return (EMidiLength)(tmpDur - 1);
                }
                return EMidiLength.undefined;
            }
            set {
                if ((double)value != (double)note.Duration / parent.EditSound.TPQN * 4 - 1) {
                    parent.SetNoteDuration(track, noteindex, ((int)value + 1) * parent.EditSound.TPQN / 4);
                }
            }
        }
        public enum EMidiNote {
            C,
            CsDf,
            D,
            DsEf,
            E,
            F,
            FsGf,
            G,
            GsAf,
            A,
            AsBf,
            B,
        }
        public class MidiNoteConverter : EnumConverter {
            private static readonly string[] CustomNames = new[]
            {
                "C", "C#/Db", "D", "D#/Eb",
                "E", "F", "F#/Gb", "G",
                "G#/Ab", "A", "A#/Bb", "B"
            };

            public MidiNoteConverter() : base(typeof(EMidiNote)) { }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == typeof(string) && value is EMidiNote note) {
                    int index = (int)note;
                    if (index >= 0 && index < CustomNames.Length)
                        return CustomNames[index];
                    return note.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                if (value is string s) {
                    int idx = Array.IndexOf(CustomNames, s);
                    if (idx >= 0)
                        return (EMidiNote)idx;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                // add notes based on current octave
                frmSoundEdit frm = (MDIMain.ActiveMdiChild as frmSoundEdit);
                Sound sound = frm.EditSound;
                int track = frm.SelectedTrack;
                int noteindex = frm.SelStart;
                int octave = FreqDivToMidiNote(sound[track][noteindex].FreqDivisor) / 12;
                switch (octave) {
                case 3:
                    // B, A#, A
                    EMidiNote[] values = [EMidiNote.A, EMidiNote.AsBf, EMidiNote.B];
                    return new StandardValuesCollection(values);
                case 10:
                    // G, F#, F, E, D#, D, C#, C
                    values = [EMidiNote.C, EMidiNote.CsDf, EMidiNote.D,
                        EMidiNote.DsEf, EMidiNote.E, EMidiNote.F,
                        EMidiNote.FsGf, EMidiNote.G];
                    return new StandardValuesCollection(values);
                default:
                    // all twelve
                    return new StandardValuesCollection(Enum.GetValues(typeof(EMidiNote)));
                }
            }
        }
        public enum EMidiOctave {
            _3,
            _4,
            _5,
            _6,
            _7,
            _8,
            _9,
            _10,
        }
        public class MidiOctaveConverter : EnumConverter {
            private static readonly string[] CustomNames =
            [
                "3", "4", "5", "6",
                "7", "8", "9", "10",
            ];
            public MidiOctaveConverter() : base(typeof(EMidiOctave)) { }
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == typeof(string) && value is EMidiOctave octave) {
                    int index = (int)octave;
                    if (index >= 0 && index < CustomNames.Length)
                        return CustomNames[index];
                    return octave.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                if (value is string s) {
                    int idx = Array.IndexOf(CustomNames, s);
                    if (idx >= 0)
                        return (EMidiOctave)idx;
                }
                return base.ConvertFrom(context, culture, value);
            }
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                // add octaves based on current note
                frmSoundEdit frm = (MDIMain.ActiveMdiChild as frmSoundEdit);
                Sound sound = frm.EditSound;
                int track = frm.SelectedTrack;
                int noteindex = frm.SelStart;
                int notenum = FreqDivToMidiNote(sound[track][noteindex].FreqDivisor) % 12;
                switch (notenum) {
                case >= 0 and <= 7:
                    // 0-7 == 4 - 10    // C, C#, D, D#, E, F, F#, G
                    return new StandardValuesCollection(new[] { EMidiOctave._4, EMidiOctave._5, EMidiOctave._6, EMidiOctave._7, EMidiOctave._8, EMidiOctave._9, EMidiOctave._10 });
                case 8:
                    // 8 == 4 - 9     // G#
                    return new StandardValuesCollection(new[] { EMidiOctave._4, EMidiOctave._5, EMidiOctave._6, EMidiOctave._7, EMidiOctave._8, EMidiOctave._9 });
                case >= 9 and <= 11:
                    // 9,10,11 == 3 - 9     // A,  A#, B
                    return new StandardValuesCollection(new[] { EMidiOctave._3, EMidiOctave._4, EMidiOctave._5, EMidiOctave._6, EMidiOctave._7, EMidiOctave._8, EMidiOctave._9 });
                }
                return new StandardValuesCollection(Enum.GetValues(typeof(EMidiOctave)));
            }
        }
        public enum EMidiLength {
            _1,
            _2,
            _3,
            _4,
            _5,
            _6,
            _7,
            _8,
            _9,
            _10,
            _11,
            _12,
            _13,
            _14,
            _15,
            _16,
            undefined,
        }
        public class MidiLengthConverter : EnumConverter {
            public static readonly string[] CustomNames =
            [
                "1/16", "1/8", "1/8*", "1/4",
                "1/4 + 1/16", "1/4*", "1/4**", "1/2",
                "1/2 + 1/16", "1/2 + 1/8", "1/2 + 1/8*", "1/2*",
                "1/2* + 1/16", "1/2**", "1/2** + 1/16", "1"
            ];

            public MidiLengthConverter() : base(typeof(EMidiLength)) { }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == typeof(string) && value is EMidiLength length) {
                    int index = (int)length;
                    if (index >= 0 && index < CustomNames.Length)
                        return CustomNames[index];
                    return length.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                if (value is string s) {
                    int idx = Array.IndexOf(CustomNames, s);
                    if (idx >= 0)
                        return (EMidiLength)idx;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                // exclude 'undefined'
                var values = new EMidiLength[16];
                for (int i = 0; i < 16; i++) {
                    values[i] = (EMidiLength)i;
                }
                return new StandardValuesCollection(values);
            }
        }
    }

    public class SoundEditNNote {
        frmSoundEdit parent;
        int noteindex;
        Note note;
        public SoundEditNNote(frmSoundEdit parent, int note) {
            this.parent = parent;
            this.note = parent.EditSound[3].Notes[note];
            this.noteindex = note;
        }
        public NoiseType Type {
            get { return (note.FreqDivisor & 4) == 4 ? NoiseType.WhiteNoise : NoiseType.Periodic; }
            set {
                int newfreq = note.FreqDivisor;
                if (value == NoiseType.Periodic) {
                    newfreq &= 3;
                }
                else {
                    newfreq |= 4;
                }
                if (newfreq != note.FreqDivisor) {
                    parent.SetNoteFreqDivisor(3, noteindex, newfreq);
                }
            }
        }
        public NoiseFrequency Frequency {
            get { return (NoiseFrequency)(note.FreqDivisor & 3); }
            set {
                int newfreq = note.FreqDivisor &= 4;
                newfreq |= (int)value;
                if (newfreq != note.FreqDivisor) {
                    parent.SetNoteFreqDivisor(3, noteindex, newfreq);
                }
            }
        }
        public int Duration {
            get { return note.Duration; }
            set {
                if (value < 1) {
                    value = 1;
                }
                else if (value > short.MaxValue) {
                    value = short.MaxValue;
                }
                if (value != note.Duration) {
                    parent.SetNoteDuration(3, noteindex, value);
                }
            }
        }
        public byte Attenuation {
            get { return note.Attenuation; }
            set {
                if (value < 0) {
                    value = 0;
                }
                else if (value > 15) {
                    value = 15;
                }
                if (value != note.Attenuation) {
                    parent.SetNoteAttenuation(3, noteindex, value);
                }
            }
        }
        public enum NoiseType {
            Periodic,
            WhiteNoise,
        }
        [TypeConverter(typeof(NoiseFrequencyConverter))]
        public enum NoiseFrequency {
            _2330Hz,
            _1165Hz,
            _583Hz,
            Track2,
        }
        public class NoiseFrequencyConverter : EnumConverter {
            private static readonly string[] CustomNames =
            ["2330 Hz", "1165 Hz", "583 Hz", "Track 2"];

            public NoiseFrequencyConverter() : base(typeof(NoiseFrequency)) { }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == typeof(string) && value is NoiseFrequency freq) {
                    int index = (int)freq;
                    if (index >= 0 && index < CustomNames.Length)
                        return CustomNames[index];
                    return freq.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                if (value is string s) {
                    int idx = Array.IndexOf(CustomNames, s);
                    if (idx >= 0)
                        return (NoiseFrequency)idx;
                }
                return base.ConvertFrom(context, culture, value);
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
                return new StandardValuesCollection(Enum.GetValues(typeof(NoiseFrequency)));
            }
        }
    }
}
