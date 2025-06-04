using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using EnvDTE;
using System.Drawing.Drawing2D;
using System.Globalization;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Common.API;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Sound;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.SoundUndo.SoundUndoType;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing.Text;

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
        private bool OneTrack, KeyboardVisible, KeyboardSound;

        // default note properties
        private int DefLength, DefAttn, DefOctave;
        private bool DefMute;

        // selection variables
        private SelectionModeType SelectionMode;
        public int SelectedTrack, SelStart;
        private int SelAnchor, SelLength;
        // SelAnchor is either SelStart, or SelStart+SelLength, depending
        // on whether selection is right-to-left, or left-to-right

        private bool PlayingSound = false;
        private bool blnNoteOn = false;
        private int PlayNote = 0;
        /*
  // variables for selected components
  // cursor control
  private int CursorPos;
  private int lngScrollDir;
        */

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
            picStaff[1].Tag = 1;
            picStaff[2].Tag = 2;
            picStaff[3].Tag = 3;
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
                picStaff[i].Tag = i;
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
                mnuRExport.Text = "Export Sound";
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
        /// Dynamic function to reset the resource menu.
        /// </summary>
        public void ResetResourceMenu() {
            mnuRSave.Enabled = true;
            mnuRExport.Enabled = true;
            mnuRInGame.Enabled = true;
            mnuRRenumber.Enabled = true;
            mnuRProperties.Enabled = true;
            mnuRPlaySound.Enabled = true;
        }

        public void mnuRSave_Click(object sender, EventArgs e) {
            if (IsChanged) {
                SaveSound();
            }
        }

        public void mnuRExport_Click(object sender, EventArgs e) {
            ExportSound();
        }

        public void mnuRInGame_Click(object sender, EventArgs e) {
            if (EditGame != null) {
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
                mnuUndo.Text = "Undo " + Editor.Base.LoadResString(SNDUNDOTEXT + (int)UndoCol.Peek().UDAction);
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
                //ChangeSelection(NextUndo.UDTrack, NextUndo.UDStart, 1);
                break;
            case EditNoteDuration:
                SetNoteDuration(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDData, true);
                //ChangeSelection(NextUndo.UDTrack, NextUndo.UDStart, 1);
                break;
            case EditNoteAttenuation:
                SetNoteAttenuation(NextUndo.UDTrack, NextUndo.UDStart, (byte)NextUndo.UDData, true);
                //ChangeSelection(NextUndo.UDTrack, NextUndo.UDStart, 1);
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
                propertyGrid1.Refresh();
                ChangeSelection(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDLength);
                break;
            case ShiftVol:
                for (int i = 0; i < NextUndo.UDLength; i++) {
                    EditSound[NextUndo.UDTrack][NextUndo.UDStart + i].Attenuation = NextUndo.UDNotes[i].Attenuation;
                }
                // update selection
                propertyGrid1.Refresh();
                ChangeSelection(NextUndo.UDTrack, NextUndo.UDStart, NextUndo.UDLength);
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
                // set insertion to end of inserted note
                SelStart += AddNoteCol.Count;
                SelAnchor = SelStart;
                SelLength = 0;
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
            UpdateVisibleStaves();
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

        }

        private void tsbZoomOut_Click(object sender, EventArgs e) {

        }
        #endregion

        #region Control Event Handlers

        private void splitContainer1_MouseUp(object sender, MouseEventArgs e) {
            tvwSound.Focus();
        }

        private void splitContainer2_MouseUp(object sender, MouseEventArgs e) {
            tvwSound.Focus();
        }

        private void splitContainer3_MouseUp(object sender, MouseEventArgs e) {
            tvwSound.Focus();
        }

        private void splitContainer2_Panel1_Resize(object sender, EventArgs e) {
            if (Visible) {
                UpdateVisibleStaves();
            }
        }

        private void tvwSound_KeyPress(object sender, KeyPressEventArgs e) {
            // ignore all keypresses
            e.Handled = true;
        }

        private void tvwSound_KeyUp(object sender, KeyEventArgs e) {
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
                    SelectSound();
                    break;
                case 1:
                    // loop
                    SelectTrack(tvwSound.SelectedNode.Index);
                    break;
                case 2:
                    // cel
                    SelectNote(tvwSound.SelectedNode.Parent.Index, tvwSound.SelectedNodes[0].Index, tvwSound.SelectedNode.Index, tvwSound.SelectedNodes.Count);
                    break;
                }
                break;
            }
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void tvwSound_MouseDown(object sender, MouseEventArgs e) {
            TreeNode node = tvwSound.GetNodeAt(e.X, e.Y);
            // check for right-click within the bounds of the selection
            if (e.Button == MouseButtons.Right) {
                if (tvwSound.SelectedNodes.Count > 1) {
                    if (tvwSound.SelectedNodes.Contains(node)) {
                        //return;
                    }
                }
            }
        }

        private void tvwSound_MouseUp(object sender, MouseEventArgs e) {

            if (tvwSound.SelectedNodes.Count > 1) {
                // update selection
                SelectNote(tvwSound.SelectedNodes[0].Parent.Index, tvwSound.SelectedNodes[0].Index, tvwSound.SelectedNode.Index, tvwSound.SelectedNodes.Count);
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
                SelectNote(e.Node.Parent.Index, e.Node.Index, e.Node.Index, tvwSound.NoSelection ? 0 : 1);
                break;
            }
        }

        private void tvwSound_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
            // if double-clicking on a note, select it
            if (e.Node.Level == 2 && e.Node.Index != e.Node.Parent.Nodes.Count - 1) {
                tvwSound.NoSelection = false;
                SelectNote(e.Node.Parent.Index, e.Node.Index, e.Node.Index, 1);
            }
        }

        private void tvwSound_After(object sender, TreeViewEventArgs e) {
            if (!Visible) {
                return;
            }
            // stinkin glitch in expand/collapse raises  node-click event
            // sometimes, but not always, so have to check again after...
            if (e.Node != null) {
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

        private void picStaff_MouseDown(object sender, MouseEventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;

            if (index != SelectedTrack) {
                // if not the selected track, select it
                SelectTrack(index);
            }
        }

        private void picStaff_MouseWheel(object sender, MouseEventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;
            if (Math.Sign(e.Delta) == -1) {
                //vsbStaff[index].Value += vsbStaff[index].SmallChange;
            }
            else if (Math.Sign(e.Delta) == 1) {
                //vsbStaff[index].Value -= vsbStaff[index].SmallChange;
            }
        }

        private void picStaff_Paint(object sender, PaintEventArgs e) {
            int index = (int)((SelectablePictureBox)sender).Tag;

            if (index == 3) {
                DrawNoiseStaff(e.Graphics);
            }
            else {
                DrawMusicStaff(e.Graphics, index);
            }
        }

        private void tmrStaffScroll_Tick(object sender, EventArgs e) {

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
            if (SelectedTrack >= 0 && SelStart != -1 && e.Button != MouseButtons.Right) {
                InsertNote(PlayNote);
            }
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
        }

        private void btnKybdRight_MouseUp(object sender, MouseEventArgs e) {
            tmrKeyboardScroll.Enabled = false;
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
            RestoreFocus();

        }

        private void btnStop_Click(object sender, EventArgs e) {
            // stop the sound
            StopSound();
            // go back
            RestoreFocus();
        }

        private void btnMute_Click(object sender, EventArgs e) {
            int index = (int)((Button)sender).Tag;
            // toggle mute for this track
            SetTrackMute(index, !EditSound[index].Muted);
        }

        private void btnDurationUp_Click(object sender, EventArgs e) {
            if (DefLength < 16) {
                DefLength += 1;
                DrawDefaultNote();
            }
        }

        private void btnDurationDown_Click(object sender, EventArgs e) {
            if (DefLength > 1) {
                DefLength -= 1;
                DrawDefaultNote();
            }
        }
        #endregion

        #region temp code
        void tmpsoundform() {
            /*
    private void ClearSelection()

      Dim lngStartPos As Long, lngEndPos As Long

      On Error GoTo ErrHandler

      // assumes that:
      //  a track is selected (SelectedTrack >= 0) AND
      //  track is visible (picStaffVis(SelectedTrack) = true) AND
      //  one or more notes currently selected (SelStart >=0 And SelLength >=1) AND
      //  selection is actively displayed (SelActive= true)
      // 
      if (SelectedTrack < 0) {
        return;
      }

      // calculate start pos
      lngStartPos = SOHorz + KeyWidth + NotePos(SelectedTrack, SelStart) * TICK_WIDTH * StaffScale

      // set draw mode to invert
      picStaff(SelectedTrack).DrawMode = 6

      // if starting pos <keywidth,
      if (lngStartPos < KeyWidth) {
        lngStartPos = KeyWidth
      }

      // calculate end pos
      lngEndPos = SOHorz + KeyWidth + NotePos(SelectedTrack, SelStart + SelLength) * TICK_WIDTH * StaffScale - 2

      // if end note is past edge of staff
      if (lngEndPos > picStaff(SelectedTrack).Width - 25) {
        lngEndPos = picStaff(SelectedTrack).Width - 25
      }

      // draw box over selection
      picStaff(SelectedTrack).Line (lngStartPos, 2)-(lngEndPos, picStaff(SelectedTrack).ScaleHeight - 4), vbBlack, BF

      // mode to copypen
      picStaff(SelectedTrack).DrawMode = 13

      // set selection status to off
      SelActive = false
    }

    private void DrawNoiseNote(ByVal HPos As Long, ByVal Note As Long, ByVal Length As Long, ByVal Attenuation As Long)

      // draws noise note on staff;
      // HPos is horizontal position where note will be drawn
      // Note is the freq of the note to be played
      // Length is length of note is AGI ticks
      // Attenuation is amount of volume attenuation (0 means loudest; 15 means mute)

      Dim sngLen As Single
      Dim lngVPos As Long
      Dim lngColor As Long
      Dim lngTPQN As Long

      // get tpqn
      lngTPQN = EditSound.TPQN

      // set note color based on attenuation
      lngColor = EGAColor(Attenuation)
      picStaff(3).FillColor = lngColor

      // convert length of note to MIDI Value, using TPQN
      sngLen = Length / lngTPQN * 4 // (one MIDI unit is a sixteenth note)

      // set vertical position
      lngVPos = (11 + 6 * (Note And 3)) * StaffScale + SOVert(3) + 1

      // set fill style to diagonal hash
      picStaff(3).FillStyle = vbUpwardDiagonal

      // if white noise
      if ((Note And 4) = 4) {
        // draw line with diagonal fill
        picStaff(3).Line (HPos, lngVPos)-(HPos + StaffScale * TICK_WIDTH * (Length - 0.3), lngVPos + StaffScale * 6 - 2), lngColor, B
      } else {
        // draw line with solid fill
        picStaff(3).Line (HPos, lngVPos)-(HPos + StaffScale * TICK_WIDTH * (Length - 0.3), lngVPos + StaffScale * 6 - 2), lngColor, BF
      }

      // reset fill style
      picStaff(3).FillStyle = vbFSSolid
    }

    private void DrawRest(ByVal Track As Long, ByVal HPos As Long, ByVal Length As Long)

      // draws rest on staff;
      // HPos is horizontal position where note will be drawn
      // Length is length of note is AGI ticks

      Dim rtn As Long, i As Long
      Dim VPos As Long, TrackCount As Long
      Dim sngLen As Single, lngTempPos As Long
      Dim lngTPQN As Long

      // get tpqn
      lngTPQN = EditSound.TPQN

      // convert length of note to MIDI Value, using TPQN
      sngLen = Length / lngTPQN * 4 // (one MIDI unit is a sixteenth note)

      // set fill color to black so dots draw correctly
      picStaff(Track).FillColor = vbBlack

      // if this is noise track
      if (Track = 3) {
        // notes are drawn only once
        TrackCount = 1
        VPos = 16 * StaffScale + SOVert(Track)
      } else {
        TrackCount = 2
        VPos = 101 * StaffScale + SOVert(Track)
      }

      lngTempPos = HPos
      For i = 1 To TrackCount
        HPos = lngTempPos
        // if showing notes
        if (Settings.ShowNotes) {
          // draw appropriate rest
          switch (sngLen
          case 1  // sixteenth rest
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 0, 264, 72, 108, SRCAND)

          case 2  // eighth rest
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 72, 264, 72, 108, SRCAND)

          case 3  // eighth rest dotted
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 72, 264, 72, 108, SRCAND)
           // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack

          case 4  // quarter rest
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                            NotePictures.hDC, 144, 264, 72, 108, SRCAND)

          case 5 // quater rest and sixteenth rest
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 144, 264, 72, 108, SRCAND)
            // draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                             NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
            // increment position
            HPos = HPos + StaffScale * TICK_WIDTH * lngTPQN
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 0, 264, 72, 108, SRCAND)

          case 6  // quarter rest dotted
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 144, 264, 72, 108, SRCAND)
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack

          case 7  // quarter rest double dotted
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 144, 264, 72, 108, SRCAND)
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 13, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack

          case 8  // half rest
            picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF

          case 9  // half rest and sixteenth
            picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
            // draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 2, StaffScale * 7, _
                            NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
            // increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 0, 264, 72, 108, SRCAND)

          case 10 // half rest and eighth
            picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
            // draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 2, StaffScale * 7, _
                             NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
            // increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 72, 264, 72, 108, SRCAND)

          case 11 // half rest, eighth dotted
            picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
            // draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 2, StaffScale * 7, _
                             NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
            // increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 72, 264, 72, 108, SRCAND)
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 10, VPos + 10 * StaffScale), StaffScale * 1.125, vbBlack

          case 12 // half rest dotted
            picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 11, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack

          case 13 // half rest dotted and sixteenth
            picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 11, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack
            // draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 3, StaffScale * 7, _
                             NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
            // increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 3 * lngTPQN
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 0, 264, 72, 108, SRCAND)

          case 14 // half rest double dotted
            picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 11, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 14, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack

          case 15 // half rest double dotted and sixteenth
            picStaff(Track).Line (HPos, VPos + 7 * StaffScale)-Step(8 * StaffScale, -3 * StaffScale), vbBlack, BF
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 11, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack
            // draw dot
            picStaff(Track).Circle (HPos + StaffScale * 14, VPos + 4 * StaffScale), StaffScale * 1.125, vbBlack
            // draw connector
            rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 3.5, StaffScale * 7, _
                            NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
            // increment position
            HPos = HPos + StaffScale * TICK_WIDTH * 3.5 * lngTPQN
            rtn = StretchBlt(picStaff(Track).hDC, HPos, VPos, StaffScale * 12, StaffScale * 18, _
                             NotePictures.hDC, 0, 264, 72, 108, SRCAND)

          case 16 // whole rest
            picStaff(Track).Line (HPos, VPos + StaffScale)-Step(7 * StaffScale, 4 * StaffScale), vbBlack, BF

          case Is > 16
            // greater than whole note;
            // recurse to draw one whole note at a time until done
            // ONLY do this once;
            if (i = 1) {
              Do Until Length < 4 * lngTPQN
                DrawRest Track, HPos, 4 * lngTPQN
                // decrement length
                Length = Length - 4 * lngTPQN
                if (Length > 0) {
                  // draw connectors
                  rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 17, StaffScale * lngTPQN * TICK_WIDTH * 4, StaffScale * 7, _
                                   NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
                  if (Track != 3) {
                    rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 3, VPos + StaffScale * 77, StaffScale * lngTPQN * TICK_WIDTH * 4, StaffScale * 7, _
                                     NotePictures.hDC, 0, 476, 849, 99, TRANSCOPY)
                  }
                }
                // increment horizontal position
                HPos = HPos + StaffScale * TICK_WIDTH * lngTPQN * 4
              Loop
              // if anything left
              if (Length > 0) {
                // draw remaining portion of note
                DrawRest Track, HPos, Length
              }
            }
          default:
            picStaff(Track).FillStyle = vbFSTransparent
            // not a normal note; draw a bar
            picStaff(Track).Line (HPos, VPos - 2 * StaffScale)-Step _
                                 (StaffScale * TICK_WIDTH * (Length - 0.5), StaffScale * 18), RGB(228, 228, 228), BF
            // draw black border around bar
            picStaff(Track).Line (HPos, VPos - 2 * StaffScale)-Step _
                                 (StaffScale * TICK_WIDTH * (Length - 0.5), StaffScale * 18), vbBlack, B
            picStaff(Track).FillStyle = vbFSSolid
          }
        } else {
          // draw all rest notes as blocks
          picStaff(Track).FillStyle = vbFSTransparent
          picStaff(Track).Line (HPos, VPos - 2 * StaffScale)-Step _
                               (StaffScale * TICK_WIDTH * (Length - 0.5), StaffScale * 18), RGB(228, 228, 228), BF
          // draw black border around bar
          picStaff(Track).Line (HPos, VPos - 2 * StaffScale)-Step _
                               (StaffScale * TICK_WIDTH * (Length - 0.5), StaffScale * 18), vbBlack, B
          picStaff(Track).FillStyle = vbFSSolid
        }

        // reset vpos
        VPos = 161 * StaffScale + SOVert(Track)
      Next i
    }

    public void DrawStaff(ByVal TrackNo As Long)

      // draws the staff for the given track

      Dim i As Long
      Dim lngFreq As Long, lngDur As Long, lngAtt As Long
      Dim sngStaffWidth As Single
      Dim lngHPos As Long
      Dim lngTPQN As Long, lngNoteCount As Long
      Dim lngSndLength As Long
      Dim strTime As String

      On Error GoTo ErrHandler

      // if overriding draw
      if (DontDraw) {
        return;
      }

      // special draw functions are managed as negative values
      if (TrackNo < 0) {
        switch (TrackNo
        case -1
          // draw all tracks
          For i = 0 To 3
            // recurse to draw correct tracks
            DrawStaff i
          Next i
        case -2
          // switch TO/FROM one track
          For i = 0 To 3
          // *'Debug.Assert OneTrack
              if (i = SelectedTrack) {
                picStaff(i).Visible = true
                picStaffVis(i) = true
              } else {
                picStaff(i).Visible = false
                picStaffVis(i) = false
              }
          Next i
          // use resize to force update
          Form_Resize
        case -3
          // force redraw due to change in settings
          // which depends on which displaymode is currently active
          if (OneTrack) {
            DrawStaff -2
          } else {
            DrawStaff -1
          }
          return;
        }
        // exit
        return;
      }

      // get tpqn
      lngTPQN = EditSound.TPQN

      // if track not visible
      if (Not picStaffVis(TrackNo)) {
        return;
      }

      // clear the staff
      picStaff(TrackNo).Cls
      picStaff(TrackNo).ForeColor = vbBlack

      // set drawmode to copypen (so lines draw correctly)
      picStaff(TrackNo).DrawMode = 13

      // cache scalewidth and sound length
      sngStaffWidth = picStaff(TrackNo).ScaleWidth
      lngSndLength = EditSound.Length * 15 / lngTPQN

      // if noise track
      if (TrackNo = 3) {
        For i = 0 To 4
          picStaff(3).Line (0, (11 + 6 * i) * StaffScale + SOVert(3))-(sngStaffWidth, (11 + 6 * i) * StaffScale + SOVert(3)), vbBlack
        Next i
        // draw time lines at whole note intervals, offset by two pixels
        For i = 0 To lngSndLength
          // if past right edge,
          // horizontal pos of marker
          lngHPos = i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2

          // if past right edge
          if (lngHPos > sngStaffWidth) {
            Exit For
          }
          // if greater than 0 (not to left of visible window)
          if (lngHPos >= 0) {
            picStaff(3).Line (i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2, (11 * StaffScale) + SOVert(3))-Step(0, 24 * StaffScale), vbBlack
          }
        Next i
      } else {
        For i = 0 To 4
          picStaff(TrackNo).Line (0, (96 + 6 * i) * StaffScale + SOVert(TrackNo))-(sngStaffWidth, (96 + 6 * i) * StaffScale + SOVert(TrackNo)), vbBlack
        Next i
        For i = 0 To 4
          picStaff(TrackNo).Line (0, (156 + 6 * i) * StaffScale + SOVert(TrackNo))-(sngStaffWidth, (156 + 6 * i) * StaffScale + SOVert(TrackNo)), vbBlack
        Next i
        // draw time lines at whole note intervals, offset by two pixels
        For i = 0 To lngSndLength
          // horizontal pos of marker
          lngHPos = i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2
          // if past right edge,
          if (lngHPos > sngStaffWidth) {
            Exit For
          }
          if (lngHPos >= 0) {
            picStaff(TrackNo).Line (lngHPos, (96 * StaffScale) + SOVert(TrackNo))-Step(0, 84 * StaffScale), vbBlack
          }
        Next i
      }

      // first note position
      lngHPos = CLng(SOHorz + KeyWidth)

      // step through all notes in this track
      lngNoteCount = EditSound[TrackNo).Notes.Count - 1
      For i = 0 To lngNoteCount
        // get duration first
        lngDur = EditSound[TrackNo).Notes(i).Duration

        // if note is visible,
        if (lngHPos + StaffScale * TICK_WIDTH * lngDur > KeyWidth * 0.75) {
          // now get freq and attenuation
          lngFreq = EditSound[TrackNo).Notes(i).FreqDivisor
          lngAtt = EditSound[TrackNo).Notes(i).Attenuation

          // if music note is zero, or attenuation is zero
          if ((TrackNo != 3 And lngFreq = 0) Or lngAtt = 15) {
            // draw a rest note
            DrawRest TrackNo, lngHPos, lngDur
          } else {
            // if noise track
            if (TrackNo = 3) {
              // draw noise note
              DrawNoiseNote lngHPos, lngFreq, lngDur, lngAtt
            } else {
              // convert note to MIDI and draw it
              DrawNote TrackNo, lngHPos, MIDINote(lngFreq), lngDur, lngAtt
            }
          }
        }
        // calculate position of next note
        lngHPos = CInt(SOHorz + KeyWidth + NotePos(TrackNo, i + 1) * TICK_WIDTH * StaffScale)

        // if note is past visible area
        if (lngHPos > sngStaffWidth) {
          Exit For
        }
      Next i

      // now add clef and time marks

      // if noise track
      if (TrackNo = 3) {
        // clear clef area
        picStaff(3).Line (0, 0)-(KeyWidth - 3, picStaff(3).ScaleHeight), vbWhite, BF

        // redraw the staff lines
        For i = 0 To 4
          picStaff(3).Line (0, (11 + 6 * i) * StaffScale + SOVert(3))-Step(KeyWidth, 0), vbBlack
        Next i

        // add time markers
        picStaff(3).FontSize = picStaff(3).FontSize * 2
        picStaff(3).CurrentY = 36 * StaffScale + SOVert(3)
        // draw time marks at whole note intervals
        For i = 0 To lngSndLength
          strTime = format$(i / 15 * lngTPQN, "0.0#")
          // horizontal pos of marker
          lngHPos = i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2 - picStaff(TrackNo).TextWidth(strTime) / 2
          // if past right edge
          if (lngHPos > sngStaffWidth) {
            Exit For
          }
          // if greater than 0 (not to left of visible window)
          if (lngHPos >= 0) {
            picStaff(3).CurrentX = lngHPos
            picStaff(3).Print strTime;
          }
        Next i
        picStaff(3).FontSize = picStaff(3).FontSize / 2

        picStaff(3).FontTransparent = true
        // draw noise clef
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
        picStaff(3).FontTransparent = false

      } else {
        // clear clef area (adjust by two for offset, then one more for linewidth)
    //    picStaff(TrackNo).Line (0, 0)-(KeyWidth - 6 * StaffScale, picStaff(TrackNo).ScaleHeight), vbWhite, BF
        picStaff(TrackNo).Line (0, 0)-(KeyWidth - 3, picStaff(TrackNo).ScaleHeight), vbWhite, BF

        // redraw the staff lines
        For i = 0 To 4
    //      picStaff(TrackNo).Line (0, (96 + 6 * i) * StaffScale + SOVert(TrackNo))- _
    //                             (KeyWidth - 6 * StaffScale + 1, (96 + 6 * i) * StaffScale + SOVert(TrackNo)), vbBlack
          picStaff(TrackNo).Line (0, (96 + 6 * i) * StaffScale + SOVert(TrackNo))-Step(KeyWidth, 0), vbBlack
        Next i
        For i = 0 To 4
    //      picStaff(TrackNo).Line (0, (156 + 6 * i) * StaffScale + SOVert(TrackNo))- _
    //                             (KeyWidth - 6 * StaffScale + 1, (156 + 6 * i) * StaffScale + SOVert(TrackNo)), vbBlack
          picStaff(TrackNo).Line (0, (156 + 6 * i) * StaffScale + SOVert(TrackNo))-Step(KeyWidth, 0), vbBlack
        Next i

        // draw time markers
        picStaff(TrackNo).FontSize = picStaff(TrackNo).FontSize * 2
        For i = 0 To lngSndLength
          strTime = format$(i / 15 * lngTPQN, "0.0#")
          // horizontal pos of marker
          lngHPos = i * TICK_WIDTH * 4 * lngTPQN * StaffScale + KeyWidth + SOHorz - 2 - picStaff(TrackNo).TextWidth(strTime) / 2
          // if past right edge
          if (lngHPos > sngStaffWidth) {
            Exit For
          }
          // if greater than 0 (not to left of visible window)
          if (lngHPos >= 0) {
            picStaff(TrackNo).CurrentY = 130 * StaffScale + SOVert(TrackNo)
            picStaff(TrackNo).CurrentX = lngHPos
            picStaff(TrackNo).Print strTime;
          }
        Next i
        picStaff(TrackNo).FontSize = picStaff(TrackNo).FontSize / 2

        // draw clefs
        i = StretchBlt(picStaff(TrackNo).hDC, 3, 90 * StaffScale - 2 + SOVert(TrackNo), 16 * StaffScale, 41 * StaffScale, _
                       NotePictures.hDC, 367, 0, 140, 358, SRCAND)
        i = StretchBlt(picStaff(TrackNo).hDC, 3, 156 * StaffScale + 1 + SOVert(TrackNo), 16 * StaffScale, 20 * StaffScale, _
                       NotePictures.hDC, 520, 0, 150, 174, SRCAND)

        // add Key signature
        Do While EditSound.Key != 0
          if (Sgn(EditSound.Key) > 0) {
            // add f (-10,+2)
            i = StretchBlt(picStaff(TrackNo).hDC, 20 * StaffScale, 89 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 20 * StaffScale, 149 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            if (EditSound.Key = 1) {
              Exit Do
            }
            // add c (-7, +5)
            i = StretchBlt(picStaff(TrackNo).hDC, 26 * StaffScale, 98 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 26 * StaffScale, 158 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            if (EditSound.Key = 2) {
              Exit Do
            }
            // add g (-11, +1)
            i = StretchBlt(picStaff(TrackNo).hDC, 32 * StaffScale, 86 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 32 * StaffScale, 146 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            if (EditSound.Key = 3) {
              Exit Do
            }
            // add d (-8,+4)
            i = StretchBlt(picStaff(TrackNo).hDC, 38 * StaffScale, 95 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 38 * StaffScale, 155 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            if (EditSound.Key = 4) {
              Exit Do
            }
            // add a (-5,+7)
            i = StretchBlt(picStaff(TrackNo).hDC, 44 * StaffScale, 104 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 44 * StaffScale, 164 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            if (EditSound.Key = 5) {
              Exit Do
            }
            // add e (-9,+3)
            i = StretchBlt(picStaff(TrackNo).hDC, 50 * StaffScale, 92 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 50 * StaffScale, 152 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            if (EditSound.Key = 6) {
              Exit Do
            }
            // add b (-6,+6)
            i = StretchBlt(picStaff(TrackNo).hDC, 56 * StaffScale, 101 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 56 * StaffScale, 161 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, SRCAND)
          } else {
            // add b (-6, +6)
            i = StretchBlt(picStaff(TrackNo).hDC, 20 * StaffScale, 98 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 20 * StaffScale, 158 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            if (EditSound.Key = -1) {
              Exit Do
            }
            // add e (-9, +3)
            i = StretchBlt(picStaff(TrackNo).hDC, 26 * StaffScale, 89 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 26 * StaffScale, 149 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            if (EditSound.Key = -2) {
              Exit Do
            }
            // add a (-5, +7)
            i = StretchBlt(picStaff(TrackNo).hDC, 32 * StaffScale, 101 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 32 * StaffScale, 161 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            if (EditSound.Key = -3) {
              Exit Do
            }
            // add d (-8, +4)
            i = StretchBlt(picStaff(TrackNo).hDC, 38 * StaffScale, 92 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 38 * StaffScale, 152 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            if (EditSound.Key = -4) {
              Exit Do
            }
            // add g (-4, +8)
            i = StretchBlt(picStaff(TrackNo).hDC, 44 * StaffScale, 104 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 44 * StaffScale, 164 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            if (EditSound.Key = -5) {
              Exit Do
            }
            // add c (-7, +5)
            i = StretchBlt(picStaff(TrackNo).hDC, 50 * StaffScale, 95 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 50 * StaffScale, 155 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            if (EditSound.Key = -6) {
              Exit Do
            }
            // add f (-3, +9)
            i = StretchBlt(picStaff(TrackNo).hDC, 56 * StaffScale, 107 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
            i = StretchBlt(picStaff(TrackNo).hDC, 56 * StaffScale, 167 * StaffScale + SOVert(TrackNo), StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, SRCAND)
          }
          // always exit
          Exit Do
        Loop
      }

      // if this is the selected track
      if (TrackNo = SelectedTrack) {
        picStaff(TrackNo).DrawWidth = 3
        picStaff(TrackNo).FillStyle = vbFSTransparent
        // add track selection border
        picStaff(TrackNo).Line (0, 0)-(sngStaffWidth + (vsbStaff(TrackNo).Visible * vsbStaff(TrackNo).Width) - 2, picStaff(TrackNo).ScaleHeight - 2), vbBlue, B
        picStaff(TrackNo).DrawWidth = 1
        picStaff(TrackNo).FillStyle = vbFSSolid
        // reset cursor flag
        CursorOn = false
      }
    }

    private void DrawNote(ByVal Track As Long, ByVal HPos As Long, ByVal NoteIndex As Long, ByVal Length As Long, ByVal Attenuation As Long)

      // draws note on staff;
      // Track identifies which note to draw
      // HPos is horizontal position where note will be drawn
      // Length is duration of note in AGI ticks
      // Attenuation is volume attenuation; 0 means full sound; 15 means silence

      Dim rtn As Long, dnNote As tDisplayNote
      Dim lngNoteTop As Long
      Dim i As Single, lngNPos As Long
      Dim lngDPos As Long, lngAPos As Long
      Dim lngTPos As Long, lngBPos As Long
      Dim lngColor As Long
      Dim lngTPQN As Long

      // make local copies of sound and note parameters
      lngTPQN = EditSound.TPQN

      // set note color based on attenuation
      lngColor = EGAColor(Attenuation)
      picStaff(Track).FillColor = lngColor

      // get note pos and accidental based on current key
      dnNote = DisplayNote(NoteIndex, EditSound.Key)

      // draw extra staffline if above or below staff
      For i = -6 To dnNote.Pos / 2 Step -1
        picStaff(Track).Line (HPos - 3 * StaffScale, (96 + 6 * (i + 5)) * StaffScale + SOVert(Track))-Step(12 * StaffScale, 0), vbBlack
      Next i

      if (dnNote.Pos = 0) {
        picStaff(Track).Line (HPos - 3 * StaffScale, 126 * StaffScale + SOVert(Track))-Step(12 * StaffScale, 0), vbBlack
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
          lngNoteTop = 132
          // draw on treble staff
          lngNPos = (123 + (3 * dnNote.Pos)) * StaffScale + SOVert(Track)
          // set position for dots, blocks, accidentals and ties
          lngDPos = lngNPos + 3 * StaffScale
          lngBPos = lngDPos
          lngAPos = lngNPos - 4 * StaffScale
          lngTPos = lngNPos - 8 * StaffScale
        } else {
          lngNoteTop = 0
          // draw on treble staff
          lngNPos = (107 + (3 * dnNote.Pos)) * StaffScale + SOVert(Track)
          // set position for dots, blocks, accidentals and ties
          lngDPos = lngNPos + 19 * StaffScale
          lngBPos = lngDPos
          lngAPos = lngNPos + 12 * StaffScale
          lngTPos = lngNPos + 24 * StaffScale
        }
      } else {
        // notes above middle B of bass staff(v<=6) are drawn upside down
        if (dnNote.Pos < 6) {
          lngNoteTop = 133
          // draw on bass staff
          lngNPos = (147 + (3 * dnNote.Pos)) * StaffScale + SOVert(Track)
          // set position for dots, blocks, accidentals and ties
          lngDPos = lngNPos + 3 * StaffScale
          lngBPos = lngDPos
          lngAPos = lngNPos - 4 * StaffScale
          lngTPos = lngNPos - 8 * StaffScale
        } else {
          lngNoteTop = 0
          // draw on bass staff
          lngNPos = (131 + (3 * dnNote.Pos)) * StaffScale + SOVert(Track)
          // set position for dots, blocks, accidentals and ties
          lngDPos = lngNPos + 19 * StaffScale
          lngBPos = lngDPos
          lngAPos = lngNPos + 12 * StaffScale
          lngTPos = lngNPos + 24 * StaffScale
        }
      }

      // if note is on a line,
      if ((Int(dnNote.Pos / 2) = dnNote.Pos / 2)) {
        // dot needs to be moved off the line
        lngDPos = lngDPos - 2 * StaffScale
      }

      // if drawing notes as bitmaps
      if (Settings.ShowNotes) {
        // convert length of note to MIDI Value, using TPQN
        switch (Length / lngTPQN * 4
        case 1  // sixteenth note
          // draw sixteenth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

        case 2  // eighth note
          // draw eighth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 72, lngNoteTop, 72, 133, TRANSCOPY)

        case 3  // eighth note dotted
          // draw eighth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 72, lngNoteTop, 72, 133, TRANSCOPY)
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor

        case 4  // quarter note
          // draw quarter
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 144, lngNoteTop, 72, 133, TRANSCOPY)

        case 5 // quater note tied to sixteenth note
          // draw quarter
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 144, lngNoteTop, 72, 133, TRANSCOPY)
          // add accidental, if necessary
          switch (dnNote.Tone
          case ntSharp
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
          case ntFlat
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
          case ntNatural
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
          }
          // draw connector
          rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                           NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
          // increment position
          HPos = HPos + StaffScale * TICK_WIDTH * lngTPQN

          // draw sixteenth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

        case 6  // quarter note dotted
          // draw quarter
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 144, lngNoteTop, 72, 133, TRANSCOPY)
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, picStaff(Track).FillColor

         case 7  // quarter note double dotted
          // draw quarter
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 144, lngNoteTop, 72, 133, TRANSCOPY)
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 13, lngDPos), StaffScale * 1.125, lngColor

        case 8  // half note
          // draw half note
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)

        case 9  // half note tied to sixteenth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
          // add accidental, if necessary
          switch (dnNote.Tone
          case ntSharp
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
          case ntFlat
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
          case ntNatural
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
          }
          // draw connector
          rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 2 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                             NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
          // increment position
          HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
          // if note is on bottom, it trails past the staff mark; need to bump it back a little
          if (lngNoteTop = 0) {
            HPos = HPos - StaffScale * TICK_WIDTH * 1.25
          }

          // draw sixteenth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

        case 10 // half note tied to eighth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
          // add accidental, if necessary
          switch (dnNote.Tone
          case ntSharp
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
          case ntFlat
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
          case ntNatural
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
          }
          // draw connector
          rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 2 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                           NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
          // increment position
          HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
          // if note is on bottom, it trails past the staff mark; need to bump it back a little
          if (lngNoteTop = 0) {
            HPos = HPos - StaffScale * TICK_WIDTH * 1.25
          }

          // draw eighth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 72, lngNoteTop, 72, 133, TRANSCOPY)

        case 11 // half note tied to dotted eighth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
          // add accidental, if necessary
          switch (dnNote.Tone
          case ntSharp
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
          case ntFlat
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
          case ntNatural
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
          }
          // draw connector
          rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 2 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                             NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
          // increment position
          HPos = HPos + StaffScale * TICK_WIDTH * 2 * lngTPQN
          // if note is on bottom, it trails past the staff mark; need to bump it back a little
          if (lngNoteTop = 0) {
            HPos = HPos - StaffScale * TICK_WIDTH * 1.25
          }

          // draw eighth note
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 72, lngNoteTop, 72, 133, TRANSCOPY)
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor

        case 12 // half note dotted
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor

        case 13 // half note dotted tied to sixteenth
           rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor
          // add accidental, if necessary
          switch (dnNote.Tone
          case ntSharp
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
          case ntFlat
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
          case ntNatural
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
          }
          // draw connector
          rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 3 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                             NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
          // increment position
          HPos = HPos + StaffScale * TICK_WIDTH * 3 * lngTPQN
          // if note is on bottom, it trails past the staff mark; need to bump it back a little
          if (lngNoteTop = 0) {
            HPos = HPos - StaffScale * TICK_WIDTH * 1.25
          }

          // draw sixteenth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

        case 14 // half note double dotted
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 13, lngDPos), StaffScale * 1.125, lngColor

        case 15 // half note double dotted tied to sixteenth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 216, lngNoteTop, 72, 133, TRANSCOPY)
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 10, lngDPos), StaffScale * 1.125, lngColor
          // draw dot
          picStaff(Track).Circle (HPos + StaffScale * 13, lngDPos), StaffScale * 1.125, lngColor
          // add accidental, if necessary
          switch (dnNote.Tone
          case ntSharp
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
          case ntFlat
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
          case ntNatural
            rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                             NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
          }
          // draw connector
          rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 3.5 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                             NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
          // increment position
          HPos = HPos + StaffScale * TICK_WIDTH * 3.5 * lngTPQN
          // if note is on bottom, it trails past the staff mark; need to bump it back a little
          if (lngNoteTop = 0) {
            HPos = HPos - StaffScale * TICK_WIDTH * 1.25
          }

          // draw sixteenth
          rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 0, lngNoteTop, 72, 133, TRANSCOPY)

        case 16 // whole note
           rtn = StretchBlt(picStaff(Track).hDC, HPos, lngNPos, StaffScale * 12, StaffScale * 22, _
                           NotePictures.hDC, 288, lngNoteTop, 72, 133, TRANSCOPY)

        case Is > 16
          // greater than whole note;
          // recurse to draw one whole note at a time until done
          Do Until Length < 4 * lngTPQN
            DrawNote Track, HPos, NoteIndex, 4 * lngTPQN, Attenuation
            // decrement length
            Length = Length - 4 * lngTPQN
            // draw connector if note continues
            if (Length > 0) {
              rtn = StretchBlt(picStaff(Track).hDC, HPos + StaffScale * 4, lngTPos, StaffScale * 4 * lngTPQN * TICK_WIDTH, StaffScale * 7, _
                               NotePictures.hDC, 0, 376 - 100 * (lngNoteTop = 0), 849, 99, TRANSCOPY)
            }
            // increment horizontal position
            HPos = HPos + StaffScale * TICK_WIDTH * lngTPQN * 4
            // special case- if EXACTLY one sixteenth note left AND on bottom
            // bump it back a little
            if (CSng(Length / lngTPQN * 4) = 1 And lngNoteTop = 0) {
              HPos = HPos - StaffScale * TICK_WIDTH * 1.25
            }

          Loop
          // if anything left
          if (Length > 0) {
            // draw remaining portion of note
            DrawNote Track, HPos, NoteIndex, Length, Attenuation
          }
          // exit
          return;

        default:
          // not a normal note; draw a bar
          // this adjustment is interfering with the accidental position; need to reset lngNPos after drawing box
          picStaff(Track).Line (HPos, lngBPos - StaffScale * 3)-Step(StaffScale * TICK_WIDTH * (Length - 0.8), StaffScale * 6), lngColor, BF
        }
      } else {
        // draw the block for this note
        picStaff(Track).Line (HPos, lngBPos - StaffScale * 3)-Step(StaffScale * TICK_WIDTH * (Length - 0.8), StaffScale * 6), lngColor, BF
      }

      // add accidental, if necessary
      switch (dnNote.Tone
      case ntSharp
        rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                         NotePictures.hDC, 588, 195, 36, 84, TRANSCOPY)
      case ntFlat
        rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos - 3 * StaffScale, StaffScale * 6, StaffScale * 14, _
                         NotePictures.hDC, 552, 195, 36, 84, TRANSCOPY)
      case ntNatural
        rtn = StretchBlt(picStaff(Track).hDC, HPos - StaffScale * 7, lngAPos, StaffScale * 6, StaffScale * 14, _
                         NotePictures.hDC, 624, 195, 36, 84, TRANSCOPY)
      }
    }

    private void HideCursor(ByVal HideTrack As Long)

      On Error GoTo ErrHandler

      Timer1.Enabled = false
      // un-highlight previous selection
      if (CursorOn) {
        // draw the cursor line, using invert pen
          picStaff(HideTrack).DrawWidth = 2
          picStaff(HideTrack).DrawMode = 6 // invert
          picStaff(HideTrack).Line (CursorPos, 0)-(CursorPos, picStaff(HideTrack).ScaleHeight), vbBlack
          picStaff(HideTrack).DrawWidth = 1
          picStaff(HideTrack).DrawMode = 13 // copy pen
        // set cursor status to off
        CursorOn = false
      }
    }

    public void MenuClickHelp() {
      // help
      HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Sound_Editor.htm"
    }

    private Function NoteFromPos(ByVal TrackNo As Long, ByVal Pos As Long, Optional ByVal RoundUp As Boolean = false) As Long
      // converts an X position into a note's index number for a given track

      Dim i As Long, lngNoteCount As Long
      Dim lngHPos As Long, lngDur As Long

      // start at first note, offset by clef and Key signature
      lngHPos = SOHorz + KeyWidth

      lngNoteCount = EditSound[TrackNo).Notes.Count - 1
      // step through all notes in this track
      For i = 0 To lngNoteCount
        // get note dur
        lngDur = EditSound[TrackNo).Notes(i).Duration

        // if rounding
        if (RoundUp) {
          // if 1/2 of note extends past this position
          if (lngHPos + (StaffScale * TICK_WIDTH * lngDur) / 2 > Pos) {
            // this is note
            Exit For
          }

        // if not rounding,
        } else {
          // if note extends past this position,
          if (lngHPos + StaffScale * TICK_WIDTH * lngDur > Pos) {
            // this is the note
            Exit For
          }
        }

        // calculate position of next note
        lngHPos = CLng(SOHorz + KeyWidth + NotePos(TrackNo, i + 1) * TICK_WIDTH * StaffScale)
      Next i

      // if loop is exited normally, i will equal Notes.Count
      // which means cursor is positioned at end of track
      // and added notes will be at end

      NoteFromPos = i
    }

    private Function NotePos(ByVal Track As Long, ByVal NoteNumber As Long) As Long

      // returns the timepostion of a note
      // Track and NoteNumber should be validated BEFORE calling this function

      Dim i As Long

        // if looking for a note past end, return the end
        if (NoteNumber > EditSound[Track).Notes.Count) {
          NoteNumber = EditSound[Track).Notes.Count
        }

        For i = 0 To NoteNumber - 1
          NotePos = NotePos + EditSound[Track).Notes.Item(i).Duration
        Next i
    }


    private void SetVScroll(ByVal Index As Long, ByVal PrevScale As Long)

      // shows/hides vertical scrollbar, and
      // adjust position so staves stay in same relative
      // position after redrawing

      Dim intNewScroll As Integer
      Dim intMax As Integer, intSH0 As Integer

      On Error GoTo ErrHandler

        // move scrollbar to end
        vsbStaff(Index).Left = picStaff(Index).ScaleWidth - vsbStaff(Index).Width
        vsbStaff(Index).Height = picStaff(Index).ScaleHeight

        // reset height
        vsbStaff(Index).Height = picStaff(Index).ScaleHeight

        // calculate amount of staff height that
        // exceeds picstaff scaleheight
        switch (Index
        case 0, 1, 2  // for music tracks
          intMax = 186 * StaffScale - picStaff(Index).ScaleHeight
          intSH0 = 186 * PrevScale
          intSH0 = vsbStaff(Index).Value + picStaff(Index).ScaleHeight / PrevScale
        case 3        // noise track
          intMax = 50 * StaffScale - picStaff(Index).ScaleHeight
          intSH0 = 50 * PrevScale
        }

        // reset vertical scrollbar or hide, if not needed
        vsbStaff(Index).Visible = (intMax > 0)

        if (vsbStaff(Index).Visible) {
          // if Max has changed,
          if (vsbStaff(Index).Max != intMax) {
            // disable updates
            DontDraw = true
            // if vsb was not previously visible,
            if (vsbStaff(Index).Max = -1) {
              switch (Index
              case 0, 1, 2
                // set new scroll position to show
                // treble clef at bottom of picStaff
                intNewScroll = (132 * StaffScale / PrevScale) - picStaff(Index).ScaleHeight
              case 3
                // set new scroll position to show
                // clef at bottom of picStaff
                intNewScroll = intMax
              }
            } else {
              // calculate the new scroll position
              intNewScroll = intSH0
            }

            // reset Max
            vsbStaff(Index).Max = intMax
            // validate new scroll Value
            if (intNewScroll < 0) { intNewScroll = 0
            if (intNewScroll > intMax) { intNewScroll = intMax
            // set Value
            vsbStaff(Index).Value = intNewScroll
            // and offset
            SOVert(Index) = -intNewScroll
            // restore drawing
            DontDraw = false
          }
        } else {
          // reset offset
          SOVert(Index) = 0
          vsbStaff(Index).Value = 0
          vsbStaff(Index).Max = -1
        }
        // update small and large change values
        vsbStaff(Index).SmallChange = picStaff(Index).Height * SM_SCROLL
        vsbStaff(Index).LargeChange = picStaff(Index).Height * LG_SCROLL
    return;

    ErrHandler:
      Resume Next
    }

    private void ShiftDuration(ByVal NewLength As Long)

      // shift (change) duration for selected note to new value
      // 
      // NOTE: this only works for a single note; can't adjust
      // duration of a group of notes

      Dim NextUndo As SoundUndo

      On Error GoTo ErrHandler

      Set NextUndo = New SoundUndo

      // allow editing of a single selected note, or no selection
      // (which means cursor note is being edited)
      if (SelectedTrack != -1 And SelStart >= 0 And SelLength <= 1) {
        // one note- change it
        // *'Debug.Assert SelectedTrack == tvwSound.SelectedItem.Parent.Index - 2
        // set undo properties
        NextUndo.UDAction = EditNote
        NextUndo.UDTrack = SelectedTrack
        NextUndo.UDStart = CLng(tvwSound.SelectedItem.Tag)
        NextUndo.UDText = EditSound[SelectedTrack).Notes(tvwSound.SelectedItem.Tag).Duration
        NextUndo.UDLength = 2

        // make change
        EditSound[SelectedTrack).Notes(tvwSound.SelectedItem.Tag).Duration = NewLength

        // add undo
        if (Settings.SndUndo != 0) {
          AddUndo NextUndo
        }

        // reset horizontal scrollbar
        SetHScroll
        // use force-redraw to update display
        ChangeSelection SelectedTrack, SelStart, SelLength, true, false, true
      }
    }


    private void ShowSelection()

      Dim lngStartPos As Long, lngEndPos As Long

      // assumes that:
      //  a track is selected (SelectedTrack >= 0) AND
      //  track is visible (picStaffVis(SelectedTrack) = true) AND
      //  zero or more notes currently selected (SelStart >=0 And SelLength >=0) AND
      //  selection is NOT actively displayed (SelActive = false)

      On Error GoTo ErrHandler

      // *'Debug.Assert picStaffVis(SelectedTrack) = true
      if (Not picStaffVis(SelectedTrack)) {
        return;
      }

      // calculate selstart pos
      lngStartPos = SOHorz + KeyWidth + NotePos(SelectedTrack, SelStart) * TICK_WIDTH * StaffScale

      if (SelLength = 0) {
        // cursorpos is lngstartpos; if cursorpos is less than keywidth, or if
        // cursorpos is greater than right edge, don't draw cursor

        if (lngStartPos >= KeyWidth And lngStartPos <= picStaff(SelectedTrack).Width - 8 + vsbStaff(SelectedTrack).Visible * 17) {
          CursorPos = lngStartPos
          // enable cursor
          Timer1.Enabled = true
        }
      } else {
        // set draw mode to invert
        picStaff(SelectedTrack).DrawMode = 6

        // calculate end pos
        lngEndPos = SOHorz + KeyWidth + NotePos(SelectedTrack, SelStart + SelLength) * TICK_WIDTH * StaffScale - 2

        // if not completely off left or right edge
        if (lngStartPos <= picStaff(SelectedTrack).Width - 8 + vsbStaff(SelectedTrack).Visible * 17 And lngEndPos >= KeyWidth) {
          // if starting pos <keywidth,
          if (lngStartPos < KeyWidth) {
            lngStartPos = KeyWidth
          }

          // if end note is past edge of staff
          if (lngEndPos > picStaff(SelectedTrack).Width - 8 + vsbStaff(SelectedTrack).Visible * 17) {
            lngEndPos = picStaff(SelectedTrack).Width - 8 + vsbStaff(SelectedTrack).Visible * 17
          }

          // draw box over selection
          picStaff(SelectedTrack).Line (lngStartPos, 2)-(lngEndPos, picStaff(SelectedTrack).ScaleHeight - 4), vbBlack, BF

          // mode to copypen
          picStaff(SelectedTrack).DrawMode = 13

          SelActive = true
        }
      }

      // update edit menu
      SetEditMenu
    }

    public void UpdateID(ByVal NewID As String, NewDescription As String)

      Dim blnRedraw As Boolean

      On Error GoTo ErrHandler

      if (EditSound.Description != NewDescription) {
        // change the EditSound object's description
        EditSound.Description = NewDescription
        // if node 1 is selected
        if (tvwSound.SelectedItem.Index = 1) {
          blnRedraw = true
        }
      }

      if (EditSound.ID != NewID) {

        // change the EditSound object's ID and caption
        EditSound.ID = NewID

        // if soundedit is dirty
        if (Asc(Caption) = 42) {
          Caption = sDM & sSNDED & ResourceName(EditSound, InGame, true)
        } else {
          Caption = sSNDED & ResourceName(EditSound, InGame, true)
        }

        // change root node of notes list
        Me.tvwSound.Nodes(1).Text = ResourceName(EditSound, InGame, true)
        // if node 1 is selected
        if (tvwSound.SelectedItem.Index = 1) {
          blnRedraw = true
        }
      }

      if (blnRedraw) {
        // force redraw
        PaintPropertyWindow
      }
    }

    public void ZoomScale(ByVal Dir As Long)

      // adjusts zoom factor for display
      // positive dir increases scale; negative decreases scale

      Dim i As Long
      Dim lngPrevScale As Long

      // save current scroll position
      lngPrevScale = StaffScale

      // increment/decrement scale
      StaffScale = StaffScale + Sgn(Dir)

      // if below minimum
      if (StaffScale = 0) {
        // reset and exit
        StaffScale = 1
        return;
      }

      // if above maximum
      if (StaffScale = 4) {
        // reset and exit
        StaffScale = 3
        return;
      }

      if (MainStatusBar.Tag != CStr(rtSound)) {
     // *'Debug.Print "AdjustMenus 1"
        AdjustMenus rtSound, InGame, true, IsChanged
      }
      // update statusbar
      MainStatusBar.Panels("Scale").Text = "Scale: " & CStr(StaffScale)

      // adjust offset
      SOHorz = SOHorz / lngPrevScale * StaffScale

      For i = 0 To 3
        // set font size for track
        picStaff(i).FontSize = 5 * StaffScale
        // reset vertical scrollbars
        SetVScroll i, lngPrevScale
      Next i

      // adjust key width FIRST
      SetKeyWidth();

      // THEN resize horizontal scale
      SetHScroll
      // need to convert pixels into duration, since the
      // scrollbar uses units of duration
      hsbStaff.SmallChange = (picStaff(0).Width - KeyWidth) * SM_SCROLL / TICK_WIDTH / StaffScale
      hsbStaff.LargeChange = (picStaff(0).Width - KeyWidth) * LG_SCROLL / TICK_WIDTH / StaffScale

      // redraw staves
      DrawStaff -1

      // if a selection is active, use changeselection to draw it correctly
      if (SelectedTrack != -1) {
        ChangeSelection SelectedTrack, SelStart, SelLength, true, false, true // 
      }
    }

    private void Form_Deactivate()

      // release midi

      On Error GoTo ErrHandler

      if (Not Settings.NoMIDI) {
        // if playing
        if (frmMDIMain.mnuECustom1.Caption = "Stop Sound" & vbTab & "Ctrl+Enter") {
          // stop it
          EditSound.StopSound
          // reset menu caption
          frmMDIMain.mnuECustom1.Caption = "Play Sound" & vbTab & "Ctrl+Enter"
        }

        // close midi
        KillMIDI
      }
    }

    private void Form_KeyPress(KeyAscii As Integer)

      Dim intKey As Integer

      // don't override editboxes
      if (ActiveControl.Name = "txtProperty" Or ActiveControl.Name = "lstProperty") {
        return;
      }

      // *'Debug.Assert frmMDIMain.ActiveForm Is Me

      // local copy of key
      intKey = KeyAscii
      // clear buffer so key is not processed
      // further
      KeyAscii = 0


      switch (intKey
      case 45, 95 // "-", "_" vol down (attenuation up)
        picDuration_MouseDown 0, 0, 1, 1

      case 43, 61 // "+", "=" vol up (attenuation down)
        picDuration_MouseDown 0, 0, 25, 1

      case 32 // space toggle mute
        DefMute = Not DefMute
        ShowDefaultDuration

      case 60, 44 // "<", "," octave down
        DefOctave = DefOctave - 1
        if (DefOctave = 2) { DefOctave = 3
        picKeyboard.Refresh

      case 62, 46 // ">", "." octave up
        DefOctave = DefOctave + 1
        if (DefOctave = 11) { DefOctave = 10
        picKeyboard.Refresh

      case 91, 123 // "{", "[" length down
        if (DefLength > 1) {
          udDuration.Value = udDuration.Value - 1
        }

      case 93, 125 // "}", "]" length up
        if (DefLength < 16) {
          udDuration.Value = udDuration.Value + 1
        }

      case 97 To 103  // a - g /natural tones
        if (SelectedTrack == 3) {
          // don't do anything
          return;
        }
        // convert to 0-7 scale
        lngMIDInote = intKey - 99
        // shift so c=0 and g=6
        if (lngMIDInote < 0) { lngMIDInote = lngMIDInote + 7
        // convert letter number to music scale
        lngMIDInote = lngMIDInote * 2
        if (lngMIDInote > 4) { lngMIDInote = lngMIDInote - 1
        // combine with octave to get midi note
        lngMIDInote = DefOctave * 12 + lngMIDInote
        // validate
        if (lngMIDInote < 45 Or lngMIDInote > 127 Or lngMIDInote = 121 Or lngMIDInote = 124 Or lngMIDInote = 126) {
          lngMIDInote = -1
          return;
        }

        // play it
        NoteOn lngMIDInote, Not (SelectedTrack < 0 Or SelectedTrack > 3 Or SelStart = -1)

      case 67, 68, 70, 71, 65 // C, D, F, G, A (sharps)
        if (SelectedTrack == 3) {
          // don't do anything
          return;
        }
        // convert to 0-7 scale
        lngMIDInote = intKey - 67
        // shift so C=0 and G=6
        if (lngMIDInote < 0) { lngMIDInote = lngMIDInote + 7
        // convert letter number to music scale
        lngMIDInote = lngMIDInote * 2
        if (lngMIDInote > 4) { lngMIDInote = lngMIDInote - 1
        // combine with octave and sharpen
        lngMIDInote = DefOctave * 12 + lngMIDInote + 1

        // validate
        if (lngMIDInote < 45 Or lngMIDInote > 127 Or lngMIDInote = 121 Or lngMIDInote = 124 Or lngMIDInote = 126) {
          lngMIDInote = -1
          return;
        }

        // play it
        NoteOn lngMIDInote, Not (SelectedTrack < 0 Or SelectedTrack > 3 Or SelStart = -1)
      case 49 To 52 // 1, 2, 3, 4
        // only used in noise track
        if (SelectedTrack != 3) {
          return;
        }

        // insert a noise track periodic tone note with appopriate frequency
        lngMIDInote = intKey - 49
        NoteOn intKey - 49, true

      case 33, 64, 35, 36 // !,@,#,$
        // only used in noise track
        if (SelectedTrack != 3) {
          return;
        }
        // insert a noise track white noise note with appropriate frequency
        if (intKey = 64) {
          lngMIDInote = 5
        } else {
          lngMIDInote = intKey - 29
        }
        NoteOn lngMIDInote, true
      }

    }

    private void Form_KeyUp(KeyCode As Integer, Shift As Integer)

      // if a note is playing that was pressed on the keyboard,
      if (lngMIDInote != -1) {
        // turn it off
        NoteOff lngMIDInote
      }
      // reset note
      lngMIDInote = -1
    }

    private void hsbKeyboard_GotFocus()

      // go back
      RestoreFocus
    }

    private void hsbKeyboard_Scroll()

      hsbKeyboard_Change
    }

    private void hsbStaff_Change()

      // hsbStaff.Value is measured in trackmode (ticks)- it needs
      // conversion to scalemode to calculate SOHorz

      // adjust offset
      SOHorz = CLng(-hsbStaff.Value * TICK_WIDTH * StaffScale)

      if (Not DontDraw) {
        // use force-redraw to update display
        DrawStaff -1
        // reset cursor and selection flags
        CursorOn = false
        Timer1.Enabled = false
        SelActive = false

        // reselect as appropriate
        ChangeSelection SelectedTrack, SelStart, SelLength, true, false, false // 
      }
    }

    private void hsbStaff_GotFocus()

      On Error GoTo ErrHandler

      // if there is a track
      if (SelectedTrack >= 0) {
        if (picStaffVis(SelectedTrack)) {
          // set focus to staff
          picStaff(SelectedTrack).SetFocus
        } else {
          // set focus to tree
          tvwSound.SetFocus
          FocusCtrl = fcTree
        }
      } else {
        // set focus to tree
        tvwSound.SetFocus
        FocusCtrl = fcTree
      }
    }

    private void hsbStaff_Scroll()

      hsbStaff_Change
    }

    private void lstProperty_DblClick()

      // user has made a selection
      SelectPropFromList

      // set focus to picProperties
      picProperties.SetFocus
      FocusCtrl = fcProperty
    }

    private void lstProperty_KeyPress(KeyAscii As Integer)

      // enter = accept
      // esc = cancel

      switch (KeyAscii
      case vbKeyEscape
        // hide the list, set focus to property box
        lstProperty.Visible = false
        picProperties.SetFocus
        FocusCtrl = fcProperty

      case vbKeyReturn
        // select the property, set focus to property box
        SelectPropFromList
        picProperties.SetFocus
        FocusCtrl = fcProperty
      }
    }

    private void lstProperty_LostFocus()

      On Error GoTo ErrHandler

      // if visible, just hide it
      if (lstProperty.Visible) {
        lstProperty.Visible = false
      }

      // reset the focus to original control (should always be propertybox?)
      RestoreFocus
    }

    private void picDuration_DblClick()

      // *'Debug.Assert frmMDIMain.ActiveForm Is Me
      // if double-clicking on volume icon
      if (mX >= 12 And mX <= 22 And mY < 18) {
        // toggle mute
        DefMute = Not DefMute
        ShowDefaultDuration

      } else {
        // same as click
        picDuration_MouseDown 0, 0, mX, mY
      }
    }

    private void picDuration_GotFocus()

      // go back
      RestoreFocus
    }

    private void picDuration_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

      // check for change in volume control
      // *'Debug.Assert frmMDIMain.ActiveForm Is Me

      // if above volume control edge
      if (Y < 18 And Not DefMute) {
        switch (X
        case Is <= 11 // minus
          // make quieter (increase vol attenuation)
          DefAttn = DefAttn + 1
          if (DefAttn > 14) {
            DefAttn = 14
          } else {
            ShowDefaultDuration
          }

        case Is >= 23 // plus
          // make louder (decrease vol attenuation)
          DefAttn = DefAttn - 1
          if (DefAttn < 0) {
            DefAttn = 0
          } else {
            ShowDefaultDuration
          }
        }
      }

      // save x and Y
      mX = X
      mY = Y
    }

    public void MouseWheel(ByVal MouseKeys As Long, ByVal Rotation As Long, ByVal xPos As Long, ByVal yPos As Long)

      // mouse wheel changes default note length/volume if over the default note picture
      // it scrolls staves left/right if over one of the staves

      Dim lngDur As Long, lngLen As Long
      Dim lngNewDur As Long, intAttn As Integer
      Dim lngTarget As Long, i As Long

      // determine
      // MouseKeys values:
      //    0 = no keys pressed
      //    8 = Ctrl
      //    4 = Shift
      //    NO VALUE for Alt?

      On Error GoTo ErrHandler

      // this should never happen, but just in case
      if (Not frmMDIMain.ActiveForm Is Me) {
        return;
      }

      // determine which control the cursor is currently over
        if (xPos > picDuration.Left And xPos < picDuration.Left + picDuration.Width And yPos > picDuration.Top And yPos < picDuration.Top + picDuration.Height) {
          lngTarget = 1
        }

      // if over a visible staff:
      For i = 0 To 3
          if (picStaff(i).Visible) {
            if (xPos > picStaff(i).Left And xPos < picStaff(i).Left + picStaff(i).Width And yPos > picStaff(i).Top And yPos < picStaff(i).Top + picStaff(i).Height) {
              // found it
              lngTarget = 2
              Exit For
            }
          }
      Next i

      // check the scrollbar
        if (hsbStaff.Visible) {
          if (xPos > hsbStaff.Left And xPos < hsbStaff.Left + hsbStaff.Width And yPos > hsbStaff.Top And yPos < hsbStaff.Top + hsbStaff.Height) {
            // same as being over a staff
            lngTarget = 2
          }
        }

      // check the keyboard
        if (picKeyboard.Visible) {
          if (xPos > picKeyboard.Left And xPos < picKeyboard.Left + picKeyboard.Width And yPos > picKeyboard.Top And yPos < picKeyboard.Top + picKeyboard.Height) {
            lngTarget = 3
          }
        }

      // if not over a target
      if (lngTarget = 0) {
        // do nothing
        return;
      }

      // scroll based on target value
      switch (lngTarget
      case 1 // default note length
        // NOTE: in this context, note DURATION is the AGI duration value
        // and note LENGTH is the corresponding MIDI note length (accounting
        // for TPQN value), which may be a rounded value

        switch (MouseKeys
        case 0 // no shift, ctrl, or alt
          // adjust note length

          // is a single note on one (music) track selected?
          if (SelectedTrack >= 0 And SelectedTrack <= 2 And SelStart >= 0 And SelLength = 1) {
            // *'Debug.Assert SelectedTrack == tvwSound.SelectedItem.Parent.Index - 2
            // get current note duration
            lngDur = EditSound[SelectedTrack).Notes(tvwSound.SelectedItem.Tag).Duration
            // is it a 'regular' note value? i.e. does it exactly match a standard note duration?

            // convert to note length, which will include rounding, if needed
            lngLen = lngDur * 4 / EditSound.TPQN
            // now convert back, to compare against original (if not an exact match)
            lngNewDur = lngLen * EditSound.TPQN / 4
            // if it matches original duration, then we have a 'regular' note
            if (lngDur = lngNewDur) {
              // adjust the length based on wheel, using call to ShiftDuration
              if (Sgn(Rotation) = 1) {
                if (lngLen > 1) {
                  lngNewDur = EditSound.TPQN * (lngLen - 1) / 4
                  ShiftDuration lngNewDur
                }

              } else if ( Sgn(Rotation) = -1) {
                lngNewDur = EditSound.TPQN * (lngLen + 1) / 4
                ShiftDuration lngNewDur
              }
            }

          } else {
            // adjust default length
            if (Sgn(Rotation) = 1) {
              if (DefLength > 1) {
                DefLength = DefLength - 1
                ShowDefaultDuration
              }
            } else if ( Sgn(Rotation) = -1) {
              if (DefLength < 16) {
                DefLength = DefLength + 1
                ShowDefaultDuration
              }
            }
          }

        case 8
          // adjust note volume
          // adjusting attenuation is not limited to a single note

          // is at least one note on one (music) track selected?
          if (SelectedTrack >= 0 And SelectedTrack <= 2 And SelStart >= 0 And SelLength > 0) {
            // *'Debug.Assert SelectedTrack == tvwSound.SelectedItem.Parent.Index - 2
            // get current note attenuation
            intAttn = EditSound[SelectedTrack).Notes(tvwSound.SelectedItem.Tag).Attenuation
            // adjust the length based on wheel, using call to ShiftDuration

            if (Sgn(Rotation) = 1) {
              if (intAttn > 0) {
                ShiftVol 1 // Sgn(Rotation)
              }

            } else if ( Sgn(Rotation) = -1) {
              if (intAttn < 14) {
                ShiftVol -1 // Sgn(Rotation)
              }
            }

          } else {
            // adjust default volume
            if (Sgn(Rotation) = 1) {
              if (DefAttn > 1) {
                DefAttn = DefAttn - 1
                ShowDefaultDuration
              }
            } else if ( Sgn(Rotation) = -1) {
              // it's 14 because attenuation of 15 equals mute, which
              // is represented on the staves differently
              if (DefAttn < 14) {
                DefAttn = DefAttn + 1
                ShowDefaultDuration
              }
            }
          }
        }

      case 2 // staff
        // ignore unless no keys pressed
        if (MouseKeys = 0) {
          if (Sgn(Rotation) = 1) {
              if (hsbStaff.Value > hsbStaff.Min) {
                // if there is room for a small change
                if (hsbStaff.Value > hsbStaff.Min + hsbStaff.SmallChange) {
                  hsbStaff.Value = hsbStaff.Value - hsbStaff.SmallChange
                } else {
                  hsbStaff.Value = hsbStaff.Min
                }
              }

          } else if ( Sgn(Rotation) = -1) {
              if (hsbStaff.Value < hsbStaff.Max) {
                // if there is room for a small change
                if (hsbStaff.Value < hsbStaff.Max - hsbStaff.SmallChange) {
                  hsbStaff.Value = hsbStaff.Value + hsbStaff.SmallChange
                } else {
                  hsbStaff.Value = hsbStaff.Max
                }
              }
          }
        }

      case 3 // keyboard

        // ignore unless no keys pressed
        if (MouseKeys == 0 && btnKybdLeft.Enabled) {
            if (SelectedTrack == 3 ) {
                if (Math.Sign(Rotation) == 1) {
                    NkbOffset += 10;
                    if (NKbOffset > 8 * MinNoiseKeyWidth - picKeyboard.Width) {
                        NKbOffset = 8 * MinNoiseKeyWidth - picKeyboard.Width;
                    }
                }
                else if (Math.Sign(Rotation) == -1) {
                    NkbOffset -= 10;
                    if (NKbOffset < 0) {
                        NKbOffset = 0;
                    }
                }
            }
            else {
                if (Math.Sign(Rotation) == 1) {
                    MkbOffset += 1;
                    if (MKbOffset > 93 - picKeyboard.Width / 24) {
                        MKbOffset = 93 - picKeyboard.Width / 24;
                    }
                }
                else if (Math.Sign(Rotation) == -1) {
                    MkbOffset -= 1;
                    if (MKbOffset < 45) {
                        MKbOffset = 45;
                    }
                }
            }
        }
      }
    }

    private void picKeyboard_DblClick()
      picKeyboard_MouseDown mButton, mShift, mX, mY
    }

    public void MenuClickClear()

      // verify notes are selected

      // shift selected notes up one note on scale
      ShiftTone 1
    }

    public void MenuClickCopy()

      Dim i As Long

      // if an active selection
      if (SelectedTrack != -1 And SelStart >= 0 And SelLength > 0) {

        // clear clipboard
        SoundClipboard.Clear

        // add selected notes
        For i = 0 To SelLength - 1
          SoundClipboard.Add EditSound[SelectedTrack).Notes(SelStart + i).FreqDivisor, EditSound[SelectedTrack).Notes(SelStart + i).Duration, EditSound[SelectedTrack).Notes(SelStart + i).Attenuation
        Next i

        // set mode based on track
        if (SelectedTrack == 3) {
          // notes are noise note
          SoundCBMode = 1
        } else {
          // notes are regular notes
          SoundCBMode = 0
        }

        // update menus
        SetEditMenu
      }
    }

    public void MenuClickCut()

      // copy
      MenuClickCopy

      // then delete
      MenuClickDelete

      // change last undo item to 'cut'
      UndoCol(UndoCol.Count).UDAction = Cut

    }

    public void MenuClickInsert()

      // shift selected notes down
      ShiftTone -1
    }

    public void MenuClickPaste()

      // inserts clipboard notes into this track

      Dim blnReplace As Boolean

      // if clipboard has regular notes and seltrack is a regular track OR
      //   clipboard has noise notes and seltrack is noise AND there are notes on the clipboard
      if (((SoundCBMode = 0 And SelectedTrack >= 0 And SelectedTrack <= 2) Or _
         (SoundCBMode = 1 And SelectedTrack == 3)) And SoundClipboard.Count != 0) {
        // if selection is >=1
        if (SelLength >= 1) {
          // delete selection first
          DeleteNotes SelectedTrack, SelStart, SelLength
          blnReplace = true
        }

        // insert clipboard notes
        AddNotes SelectedTrack, SelStart, SoundClipboard, false

        // if replacing,
        if (blnReplace) {
          // set flag in undo item so the undo action
          // deletes the pasted notes and restores original notes
          UndoCol(UndoCol.Count).UDText = "R"
        }
      }
    }

    private void Form_KeyDown(KeyCode As Integer, Shift As Integer)

      On Error GoTo ErrHandler

      Dim tmpMax As Long

      // always check for help first
      if (Shift = 0 And KeyCode = vbKeyF1) {
        MenuClickHelp
        KeyCode = 0
        return;
      }

      // don't override editboxes or the property box
      if (ActiveControl Is txtProperty Or ActiveControl Is lstProperty) {
        return;
      }

      // check for global shortcut keys
      CheckShortcuts KeyCode, Shift
      if (KeyCode = 0) {
        return;
      }

      switch (Shift
      case vbCtrlMask
        switch (KeyCode
        case vbKeyA
          // select all
          if (frmMDIMain.mnuESelectAll.Enabled) {
            MenuClickSelectAll
            KeyCode = 0
          }

        case vbKeyZ
          // undo
          if (frmMDIMain.mnuEUndo.Enabled) {
            MenuClickUndo
            KeyCode = 0
          }

        case vbKeyX
          // cut
          if (frmMDIMain.mnuECut.Enabled) {
            MenuClickCut
            KeyCode = 0
          }

        case vbKeyC
          // copy
          if (frmMDIMain.mnuECopy.Enabled) {
            MenuClickCopy
            KeyCode = 0
          }

        case vbKeyV
          // paste
          if (frmMDIMain.mnuEPaste.Enabled) {
            MenuClickPaste
            KeyCode = 0
          }

        case vbKeyK
          if (frmMDIMain.mnuECustom2.Enabled) {
            MenuClickECustom2
            KeyCode = 0
          }

        case vbKeyReturn
          if (frmMDIMain.mnuECustom1) {
            MenuClickECustom1
            KeyCode = 0
          }
        }
      case 0
        // no shift, ctrl, alt
        switch (KeyCode
        case vbKeyDelete
          // if (frmMDIMain.mnuEDelete.Enabled) {
            MenuClickDelete
            KeyCode = 0
          // }

        case vbKeyLeft, vbKeyUp
          switch (FocusCtrl
          case fcStaff, fcTree
            // if there is an active selection or cursor is not at start,
            if (SelectedTrack != -1 And (SelStart > 0 Or SelLength > 0)) {
              // if just a cursor
              if (SelLength = 0) {
                // move left one note
                ChangeSelection SelectedTrack, SelStart - 1, 0
              } else {
                // if anchor is to right of selection
                if (SelStart != SelAnchor) {
                  // reset start to right of selection before collapsing
                  ChangeSelection SelectedTrack, SelAnchor - SelLength, 0
                } else {
                  // collapse to current startpos
                  ChangeSelection SelectedTrack, SelStart, 0
                }
              }
              SelAnchor = SelStart

            // if at beginning, move up to track
            } else if ( SelectedTrack != -1 And SelStart = 0) {
              SelStart = -1
              ChangeSelection SelectedTrack, SelStart, 0
            }
          }
          KeyCode = 0
          Shift = 0

        case vbKeyRight, vbKeyDown
          switch (FocusCtrl
          case fcStaff, fcTree
            // if there is an active selection or cursor is not at end,
            if (SelectedTrack != -1 And (SelStart < EditSound[SelectedTrack).Notes.Count Or SelLength > 0)) {
              if (SelLength = 0) {
                // move right one note
                ChangeSelection SelectedTrack, SelStart + 1, 0, false, true
              } else {
                if (SelStart = SelAnchor) {
                  ChangeSelection SelectedTrack, SelAnchor + SelLength, 0 // , true
                } else {
                  ChangeSelection SelectedTrack, SelAnchor, 0 // , true
                }
              }
            // if at track level, move to first note
            } else if ( SelectedTrack != -1 And SelStart = -1) {
              SelStart = 0
              ChangeSelection SelectedTrack, SelStart, 0
            }
          }
          KeyCode = 0
          Shift = 0

        case vbKeyBack
          switch (FocusCtrl
          case fcProperty
          case fcStaff
            // if selection is >0
            if (SelLength > 0) {
              MenuClickDelete
            } else {
              // if not on first note
              if (SelStart > 0) {
                // move it back one, and delete
                SelStart = SelStart - 1
                MenuClickDelete
              }
            }
            KeyCode = 0
          case fcTree
          }
        }

      case vbShiftMask
          // if working with a staff, adjust selection; if working with tracks, exit
          if (tvwSound.SelectedItem.Parent Is Nothing) {
            return;
          } else if ( tvwSound.SelectedItem.Parent.Parent Is Nothing) {
            return;
          }

        switch (KeyCode
        case vbKeyLeft, vbKeyUp
          switch (FocusCtrl
          case fcStaff
            // selection is expanded to left, or collapsed on the right,
            // depending on where current position is relative to the anchor

            // if there is an active selection of one or more notes
            // AND the startpoint is the anchor,
            if (SelLength > 0 And SelAnchor = SelStart) {
              // shrink the selection by one note (move end pt)
              ChangeSelection SelectedTrack, SelStart, SelLength - 1, false, true
            } else {
              // if starting point not yet at beginning of track,
              if (SelStart > 0) {
                // expand selection by one note (move start)
                ChangeSelection SelectedTrack, SelAnchor - SelLength - 1, SelLength + 1
              } else {
                // cursor is already at beginning; just exit
                return;
              }
            }
          case fcTree
          }
          KeyCode = 0
          Shift = 0

        case vbKeyRight, vbKeyDown
          switch (FocusCtrl
          case fcStaff
            // selection is expanded to right, or collapsed on the left,
            // depending on where current position is relative to the anchor

            // if there is an active selection of one or more notes AND the startpoint is the anchor,
            // OR there is no current selection
            if (SelLength >= 0 And SelAnchor = SelStart) {
              // if not yet at end of track
              if (SelStart + SelLength < EditSound[SelectedTrack).Notes.Count) {
                // expand selection (move end pt)
                ChangeSelection SelectedTrack, SelStart, SelLength + 1, false, true
              } else {
                // cursor is already at end; just exit
                return;
              }
            } else {
              // shrink selection by one note (move start pt)
              ChangeSelection SelectedTrack, SelAnchor - SelLength + 1, SelLength - 1
            }
          case fcTree
          }
          KeyCode = 0
          Shift = 0
        }

      case vbAltMask
        switch (KeyCode
        case vbKeyU
          if (frmMDIMain.mnuEClear.Enabled) {
            MenuClickClear
            KeyCode = 0
            Shift = 0
          }

        case vbKeyD
          if (frmMDIMain.mnuEInsert.Enabled) {
            MenuClickInsert
            KeyCode = 0
            Shift = 0
          }
        }
      }
    }

    private void picStaff_DblClick(Index As Integer)

      // select the note being clicked;
      // use mouse down, then extend selection by 1 (move end pt)
      picStaff_MouseDown Index, 0, 0, mX, mY
      ChangeSelection SelectedTrack, SelStart, SelLength + 1, false, true, true // 
    }

    private void picStaff_MouseDown(Index As Integer, Button As Integer, Shift As Integer, X As Single, Y As Single)

      Dim tmpPos As Long

      On Error GoTo ErrHandler


      switch (Button
      case vbRightButton
        // if clicking on a different track
        if (Index != SelectedTrack) {
          // select track first
          picStaff_MouseDown Index, vbLeftButton, Shift, X, Y
        }

        // make sure this form is the active form
        if (Not (frmMDIMain.ActiveForm Is Me)) {
          // set focus before showing the menu
          Me.SetFocus
        }
        // need doevents so form activation occurs BEFORE popup
        // otherwise, errors will be generated because of menu
        // adjustments that are made in the form_activate event
        SafeDoEvents
        // context menu
        PopupMenu frmMDIMain.mnuEdit, 0, X + picStaff(Index).Left, Y + picStaff(Index).Top

      case vbLeftButton
        // if clicking on clef area
        if (X < KeyWidth) {
          // move cursor to start
          ChangeSelection Index, 0, 0
          // reset anchor pos
          SelAnchor = SelStart
        } else {
          // if holding shift key AND same staff
          if (Shift = vbShiftMask And Index = SelectedTrack) {
            // determine which note is being selected
            tmpPos = NoteFromPos(Index, X, true)
            if (tmpPos > SelAnchor) {
              // extend/compress selection (move end pt)
              ChangeSelection SelectedTrack, SelAnchor, tmpPos - SelAnchor, false, true
            } else {
              // extend/compress selection (move start pt)
              ChangeSelection SelectedTrack, tmpPos, SelAnchor - tmpPos
            }
          } else {
            // determine which note is being selected
            tmpPos = NoteFromPos(Index, X)
            ChangeSelection Index, tmpPos, 0
            // set anchor pos
            SelAnchor = SelStart
          }
        }

        // save mouse state for dbl click
        mButton = Button
        mShift = Shift
        mX = X
        mY = Y
      }
    }

    private void picStaff_MouseMove(Index As Integer, Button As Integer, Shift As Integer, X As Single, Y As Single)

      // determine is sellength needs to be changed,
      Dim tmpPos As Long, tmpSelStart As Long
      Dim tmpSelLength As Long
      Dim sngTime As Single

      // if no movement from starting position
      if (Not tmrScroll.Enabled And mX = X And mY = Y) {
        // not really a mousemove
        return;
      }

      switch (Button
      case vbLeftButton
        mX = X
        mY = Y

        // if no track, then exit
        if (SelectedTrack == -1) {
          return;
        }

        // get note number under cursor
        tmpPos = NoteFromPos(SelectedTrack, X, true)

        // set sellength
        tmpSelLength = tmpPos - SelAnchor
        tmpSelStart = SelAnchor

        // if backwards
        if (tmpSelLength < 0) {
          // adjust so selstart is to left of selend
          tmpSelStart = tmpSelStart + tmpSelLength
          tmpSelLength = tmpSelLength * -1
        }

        // if mouse position is off edge of screen,
        // enable autoscrolling
        if (X < 0) {
          tmrScroll.Enabled = true
          tmrScroll.Interval = 200 / ((-1 * X \ 10) + 1)
          lngScrollDir = 1
        } else if ( X > picStaff(Index).ScaleWidth + vsbStaff(Index).Visible * vsbStaff(Index).Width) {
          tmrScroll.Enabled = true
          tmrScroll.Interval = 200 / (((X - (picStaff(Index).ScaleWidth + vsbStaff(Index).Visible * vsbStaff(Index).Width)) \ 10) + 1)
          lngScrollDir = -1
        } else {
          tmrScroll.Enabled = false
          lngScrollDir = 0
        }

        // if NOT a change in selection
        if (tmpSelLength = SelLength And tmpSelStart = SelStart) {
          return;
        } else {
          // extend/compress; direction depends on anchor relation to start
          ChangeSelection SelectedTrack, tmpSelStart, tmpSelLength, false, (tmpSelStart = SelAnchor) // 
        }
      }

      // update time marker
      tmpPos = (5 + Abs(EditSound.Key)) * StaffScale * 6
      if (tmpPos < 36 * StaffScale) {
        tmpPos = 36 * StaffScale
      }

      sngTime = CSng((X - tmpPos - SOHorz)) / TICK_WIDTH / 60 / StaffScale
      if (sngTime < 0) {
        sngTime = 0
      }

      if (MainStatusBar.Tag != CStr(rtSound)) {
     // *'Debug.Print "AdjustMenus 3"
        AdjustMenus rtSound, InGame, true, IsChanged
      }
      MainStatusBar.Panels("Time").Text = "Pos: " & format$(sngTime, "0.00") & " sec"
    }

    private void Timer1_Timer()

      // toggles the cursor in the appropriate staff window

      On Error GoTo ErrHandler

      if (SelectedTrack == -1) {
        Timer1.Enabled = false
        return;
      }

      // draw the cursor line, using invert pen
      picStaff(SelectedTrack).DrawWidth = 2
      picStaff(SelectedTrack).DrawMode = 6 // invert
      picStaff(SelectedTrack).Line (CursorPos, 0)-(CursorPos, picStaff(SelectedTrack).ScaleHeight), vbBlack
      picStaff(SelectedTrack).DrawWidth = 1
      picStaff(SelectedTrack).DrawMode = 13 // copy pen

      // set cursor status
      CursorOn = Not CursorOn
    }

    private void tmrScroll_Timer()

      // autoscroll staves

      switch (lngScrollDir
      case 1  // scroll left
        // if already at left edge
        if (SOHorz = 0) {
          // disable autoscroll
          lngScrollDir = 0
          tmrScroll.Enabled = false
        } else {
          // if there is room for a small change
          if (hsbStaff.Value > hsbStaff.SmallChange) {
            hsbStaff.Value = hsbStaff.Value - hsbStaff.SmallChange
          } else {
            hsbStaff.Value = 0
          }
          // force change
          hsbStaff_Change
        }

      case -1 // scroll right
        // if already at right edge
        if (hsbStaff.Value = hsbStaff.Max) {
          // disable autoscroll
          lngScrollDir = 0
          tmrScroll.Enabled = false
        } else {
          // if there is room for a small change
          if (hsbStaff.Value < hsbStaff.Max - hsbStaff.SmallChange) {
            hsbStaff.Value = hsbStaff.Value + hsbStaff.SmallChange
          } else {
            hsbStaff.Value = hsbStaff.Max
          }
        }
      }

      // force mousemove event
      picStaff_MouseMove CInt(SelectedTrack), mButton, mShift, mX, mY
    }

    private void vsbStaff_Change(Index As Integer)

      if (Not DontDraw) {
        // set new offset
        SOVert(Index) = -vsbStaff(Index).Value

        // if staff was selected, easiest thing to do is use changeselection
        if (Index = SelectedTrack) {
          ChangeSelection SelectedTrack, SelStart, SelLength, true, false, true
        } else {
          // use draw staff method
          DrawStaff Index
        }
      }

    }
            */
        }
        #endregion

        #region Methods
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
            spScale.Text = "soundscale";
            // 
            // spTime
            // 
            spTime.AutoSize = false;
            spTime.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spTime.BorderStyle = Border3DStyle.SunkenInner;
            spTime.Name = "spTime";
            spTime.Size = new System.Drawing.Size(70, 18);
            spTime.Text = "soundtime";
        }

        public bool LoadSound(Sound loadsound) {
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
            catch {
                return false;
            }
            if (loadsound.ErrLevel < 0) {
                return false;
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
                IsChanged = (EditSound.IsChanged || EditSound.ErrLevel != 0);
            }
            Text = sSNDED + ResourceName(EditSound, InGame, true);
            if (IsChanged) {
                Text = sDM + Text;
            }
            mnuRSave.Enabled = !IsChanged;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = !IsChanged;

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
            for (int i = 0; i < 4; i++) {
                picStaff[i].Visible = EditSound[i].Visible;
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

            OneTrack = WinAGISettings.OneTrack.Value;
            KeyboardVisible = WinAGISettings.ShowKeyboard.Value;
            splitContainer2.Panel2Collapsed = !KeyboardVisible;
            KeyboardSound = !WinAGISettings.NoKeyboardSound.Value;

            for (int i = 0; i < 4; i++) {
                // set default offsets
                SOVert[i] = -(52 * StaffScale);
                vsbStaff[i].SmallChange = (int)(picStaff[i].Height * SM_SCROLL);
                vsbStaff[i].LargeChange = (int)(picStaff[i].Height * LG_SCROLL);
                vsbStaff[i].Maximum = -SOVert[i] * 2;
                vsbStaff[i].Value = -SOVert[i];
                staffFont = new("Courier New", 5 * StaffScale);
                if (EditSound[i].Muted) {
                    btnMute[i].Image = EditorResources.esi_muteon;
                }
                else {
                    btnMute[i].Image = EditorResources.esi_muteoff;
                }
            }

            // draw the default note display
            DrawDefaultNote();
            SetKeyWidth();
            return true;
        }

        /// <summary>
        /// Re-distribute the staff panels to match the current visibility of
        /// the tracks.
        /// </summary>
        public void UpdateVisibleStaves() {

            if (OneTrack) {
                for (int i = 0; i <= 3; i++) {
                    if (i == SelectedTrack) {
                        picStaff[i].Visible = true;
                        picStaff[i].Top = 0;
                        picStaff[i].Height = splitContainer2.Panel1.Height - hsbStaff.Height;
                        picStaff[i].Invalidate();
                    }
                    else {
                        picStaff[i].Visible = false;
                    }
                }
            }
            else {
                StaffCount = 0;
                for (int i = 0; i <= 3; i++) {
                    picStaff[i].Visible = EditSound[i].Visible;
                    if (EditSound[i].Visible) {
                        StaffCount++;
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
                            picStaff[i].Height = splitContainer2.Panel1.Height - hsbStaff.Height;
                            break;
                        }
                    }
                    break;
                case 2:
                    // two staves visible   
                    hsbStaff.Visible = true;
                    // if one of the staves is the noise track, then
                    // it has a static height
                    if (picStaff[3].Visible) {
                        for (int i = 0; i < 3; i++) {
                            if (picStaff[i].Visible) {
                                picStaff[i].Top = 0;
                                picStaff[i].Height = splitContainer2.Panel1.Height - 55 - hsbStaff.Height;
                                break;
                            }
                        }
                    }
                    else {
                        // distrubute the other two staves evenly
                        bool first = true;
                        for (int i = 0; i < 3; i++) {
                            if (picStaff[i].Visible) {
                                picStaff[i].Height = (splitContainer2.Panel1.Height - hsbStaff.Height) / 2;
                                if (first) {
                                    picStaff[i].Top = 0;
                                    first = false;
                                }
                                else {
                                    picStaff[i].Top = (splitContainer2.Panel1.Height - hsbStaff.Height) / 2;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case 3:
                    // three staves visible
                    hsbStaff.Visible = true;
                    // if one of the staves is the noise track, then
                    // it has a static height
                    if (picStaff[3].Visible) {
                        // distrubute the other two staves evenly
                        bool first = true;
                        for (int i = 0; i < 3; i++) {
                            if (picStaff[i].Visible) {
                                if (first) {
                                    picStaff[i].Height = (splitContainer2.Panel1.Height - 55 - hsbStaff.Height) / 2;
                                    picStaff[i].Top = 0;
                                    first = false;
                                }
                                else {
                                    picStaff[i].Height = (splitContainer2.Panel1.Height - 55 - hsbStaff.Height) / 2;
                                    picStaff[i].Top = (splitContainer2.Panel1.Height - 55 - hsbStaff.Height) / 2;
                                }
                            }
                        }
                    }
                    else {
                        // distrubute the other three staves evenly
                        picStaff[0].Height = (splitContainer2.Panel1.Height - hsbStaff.Height) / 3;
                        picStaff[1].Height = (splitContainer2.Panel1.Height - hsbStaff.Height) / 3;
                        picStaff[1].Top = picStaff[0].Bottom;
                        picStaff[2].Height = (splitContainer2.Panel1.Height - hsbStaff.Height) / 3;
                        picStaff[2].Top = picStaff[1].Bottom;
                    }
                    break;
                case 4:
                    // all staves visible
                    picStaff[0].Height = (splitContainer2.Panel1.Height - 55 - hsbStaff.Height) / 3;
                    picStaff[1].Height = (splitContainer2.Panel1.Height - 55 - hsbStaff.Height) / 3;
                    picStaff[1].Top = picStaff[0].Bottom;
                    picStaff[2].Height = (splitContainer2.Panel1.Height - 55 - hsbStaff.Height) / 3;
                    picStaff[2].Top = picStaff[1].Bottom;
                    picStaff[3].Top = picStaff[2].Bottom;
                    break;
                }
                picStaff0.Invalidate();
                picStaff1.Invalidate();
                picStaff2.Invalidate();
                picStaff3.Invalidate();
            }
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
            if (EditSound.ErrLevel != 0) {
                return false;
            }
            if (EditSound.Length == 0) {
                return false;
            }
            return true;
        }

        private void RestoreFocus() {
            //switch (FocusCtrl) {
            //case fcTree:
            //    tvwSound.Focus();
            //    break;
            //case fcProperty:
            //    picProperties.Focus();
            //    break;
            //case fcStaff:
            //    // make sure it's visible
            //    if (picStaff[SelectedTrack].Visible) {
            //        picStaff[SelectedTrack].Focus();
            //    }
            //    else {
            //        // go back to tree
            //        tvwSound.Focus();
            //    }
            //    break;
            //}
        }

        internal void SelectSound() {
            int oldtrack = SelectedTrack;

            SelectionMode = SelectionModeType.Sound;
            SelectedTrack = -1;
            SelStart = -1;
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

        internal void SelectTrack(int track) {
            int oldtrack = SelectedTrack;

            if (track == 3) {
                // noise track
                SelectionMode = SelectionModeType.NoiseTrack;
                SelectedTrack = 3;
                SelStart = -1;
                SelAnchor = -1;
                SelLength = 0;
                propertyGrid1.SelectedObject = new SoundEditNTrack(this);
            }
            else {
                // sound track
                SelectionMode = SelectionModeType.MusicTrack;
                SelectedTrack = track;
                SelStart = -1;
                SelAnchor = -1;
                SelLength = 0;
                propertyGrid1.SelectedObject = new SoundEditMTrack(this, track, SelectedTrack);
            }
            if (OneTrack) {
                UpdateVisibleStaves();
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

        internal void SelectNote(int track, int startnote, int anchor, int length) {
            int oldtrack = SelectedTrack;

            if (startnote == EditSound[track].Notes.Count) {
                // end only sets insertion
                SelectionMode = SelectionModeType.EndNote;
                SelectedTrack = track;
                SelStart = startnote;
                SelAnchor = startnote;
                SelLength = 0;
                propertyGrid1.SelectedObject = null;
            }
            else if (track == 3) {
                // noise track
                SelectionMode = SelectionModeType.NoiseNote;
                SelectedTrack = 3;
                SelStart = startnote;
                SelAnchor = anchor;
                SelLength = length;
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
                if (tvwSound.SelectedNodes.Count > 1) {
                    propertyGrid1.SelectedObject = null;
                }
                else {
                    propertyGrid1.SelectedObject = new SoundEditMNote(this, track, startnote);
                }
            }
            if (oldtrack != -1 && oldtrack != SelectedTrack) {
                picStaff[oldtrack].Invalidate();
            }
            picStaff[SelectedTrack].Invalidate();

            if (SelectedTrack == 3 && oldtrack != 3 || SelectedTrack != 3 && oldtrack == 3) {
                picKeyboard.Invalidate();
            }
            ConfigureToolbar();
        }

        private void ChangeSelection(int NewTrack, int NewStart, int NewLength, bool NoScroll = false, bool MoveEndPt = false, bool ForceRedraw = false) {
            /*
            Dim PrevTrack As Long, PrevStart As Long, PrevLength As Long, tmpNode As Node
            Dim i As Long
            Dim blnPrevVis As Boolean, blnSelVis As Boolean
            Dim lngStartPos As Long, lngEndPos As Long
            Dim blnScrolled As Boolean
            Dim lngTreeTrack As Long

            // if newtrack=-1, means no track selected, and redraw all VISIBLE tracks

            On Error GoTo ErrHandler

            // note previuos selection
            PrevTrack = SelectedTrack
            PrevStart = SelStart
            PrevLength = SelLength

            // first determine if selected track is being actively displayed - it depends
            // on which track is selected, whether OneTrack propery is enabled, and
            // visibility of selected track

            // if a specific track is selected
            if (NewTrack != -1) {
              if (OneTrack) {
                // if one track, always show selection
                blnSelVis = true
              } else {
                // if all tracks, don't need to show selection if new track is not visible
                blnSelVis = picStaffVis(NewTrack)
              }
            } else {
              // all tracks selected -
              if (OneTrack) {
                // if one track, (with all-update) hide all tracks
                For i = 0 To 3
                  picStaff(i).Visible = false
                  picStaffVis(i) = false
                Next i
                hsbStaff.Visible = false
                blnSelVis = false
              } else {
                blnSelVis = true
              }
            }
            // need to know if previous track was visible or not
            if (PrevTrack != -1) {
              blnPrevVis = picStaffVis(PrevTrack)
            } else {
              blnPrevVis = true
            }

            // if selection requires scrolling, do that first before anything else;
            // that saves from drawing staves once in old position, then again
            // after scrolling

            // check to see if staff is supposed to be scrolled first
            if (Not NoScroll And blnSelVis And NewTrack >= 0) {
              // calculate position of the new selection start point
              lngStartPos = NotePos(NewTrack, NewStart)
              // and position of the new selection end point
              lngEndPos = NotePos(NewTrack, NewStart + NewLength)

              // if moving end point:
              if (MoveEndPt) {
                // endpos to left, OR endpos to right:
                if (SOHorz + lngEndPos * TICK_WIDTH * StaffScale < 0 Or SOHorz + lngEndPos * TICK_WIDTH * StaffScale + KeyWidth > picStaff(NewTrack).ScaleWidth + vsbStaff(NewTrack).Visible * vsbStaff(NewTrack).Width) {
                  // set scroll so note is at right edge
                  lngEndPos = lngEndPos - picStaff(NewTrack).ScaleWidth / TICK_WIDTH / StaffScale + 16

                  // set scrollbars
                  if (lngEndPos > hsbStaff.Max) {
                    lngEndPos = hsbStaff.Max
                  }
                  if (lngEndPos < 0) {
                    lngEndPos = 0
                  }
                  DontDraw = true
                  hsbStaff.Value = lngEndPos
                  DontDraw = false
                  blnScrolled = true
                }

              // if moving start point (or neither point)
              } else {
                // startpos to left, or startpos to right:
                if (SOHorz + lngStartPos * TICK_WIDTH * StaffScale < 0 Or SOHorz + lngStartPos * TICK_WIDTH * StaffScale + KeyWidth > hsbStaff.Width) {
                  // set scroll so note is at left edge (allow small margin)

                  lngStartPos = lngStartPos - 3
                  // verify new position is within bounds of staff display
                  if (lngStartPos < 0) {
                    lngStartPos = 0
                  }
                  // if pos is past right extent of scrollbar, max out
                  if (lngStartPos > hsbStaff.Max) {
                    lngStartPos = hsbStaff.Max
                  }
                  DontDraw = true
                  hsbStaff.Value = lngStartPos
                  DontDraw = false
                  blnScrolled = true
                }
              }
            }

            // if scrolled, draw all tracks, which clears selection/cursor
            if (blnScrolled) {
              // update track now; it's needed to correctly redraw tracks
              SelectedTrack = NewTrack

              // which 'alldraw' used depends on onetrack setting
              if (OneTrack) {
                DrawStaff -2
              } else {
                DrawStaff -1
              }

              // make sure cursor status is set to off and timer disabled
              CursorOn = false
              Timer1.Enabled = false
              // and selactive is set to off
              SelActive = false

            // if not scrolled, do regular checks to clear selection/cursor:

            // did new track change to a different track?
            } else if ( NewTrack != PrevTrack) {
              // if previous track was a valid one and visible AND has changed, need to clear it
              if (PrevTrack != -1 And blnPrevVis And PrevTrack != NewTrack) {
                if (SelLength > 0) {
                  ClearSelection
                } else {
                  HideCursor PrevTrack
                }
                // and selactive is set to off
                SelActive = false
              }
              // if showing multiple tracks
              if (Not OneTrack) {
                // erase current selection border by redrawing
                SelectedTrack = NewTrack
                DrawStaff PrevTrack
                SelectedTrack = PrevTrack
              }

              // if no track selected?
              if (NewTrack = -1) {
                // if forcing a redraw, do it now
                if (ForceRedraw) {
                  // update track now; it's needed to correctly redraw
                  SelectedTrack = NewTrack
                  DrawStaff -1
                }
              } else {
                // if new track is visible, and has changed
                if (picStaffVis(NewTrack) And PrevTrack != NewTrack) {
                  // if it is, select it by drawing selection border
                    picStaff(NewTrack).DrawWidth = 3
                    picStaff(NewTrack).FillStyle = vbFSTransparent
                    // add track selection border
                    picStaff(NewTrack).Line (0, 0)-(.ScaleWidth + (vsbStaff(NewTrack).Visible * vsbStaff(NewTrack).Width) - 2, .ScaleHeight - 2), vbBlue, B
                    picStaff(NewTrack).DrawWidth = 1
                    picStaff(NewTrack).FillStyle = vbFSSolid
                } else {
                  // onetrack mode?
                  if (OneTrack) {
                    // update track and note now; it's needed to correctly redraw
                    SelectedTrack = NewTrack
                    SelStart = NewStart
                    SelLength = NewLength
                    // use special draw value (-2) to force redraw of correct track
                    DrawStaff -2
                  }
                }
              }

              // change keyboard instrument if track changes
              // adjust midi output to match track, if midi is enabled
              if (Not Settings.NoMIDI And hMIDI != 0) {
                // if track is a music track
                if (SelectedTrack >= 0 And SelectedTrack <= 2) {
                  // send instrument to midi
                  midiOutShortMsg hMIDI, CLng(EditSound[SelectedTrack).Instrument * &H100 + &HC0)
                } else {
                  // set instrument to 0
                  midiOutShortMsg hMIDI, &H1C0&
                }
              }

              // if oldtrack is noise, or new track is noise
              if (NewTrack = 3 Or PrevTrack = 3) {
                // refresh keyboard so it shows correctly
                picKeyboard.Refresh
              }
            } else {
              // track hasn't changed

              // if forcing a redraw, do it now
              if (ForceRedraw) {

                // draw new staff, IF it is visible
                if (blnSelVis) {
                  // update seltrack now; it's needed to correctly redraw
                  SelectedTrack = NewTrack
                  DrawStaff SelectedTrack
                }
                // make sure timer is off (don// t use hidecursor, as it
                // could cause selection to invert!)
                Timer1.Enabled = false
                CursorOn = false

              } else {
                // seltrack is the same
                // is there a selection?
                if (SelActive) {
                  // clear it
                  ClearSelection
                } else {
                  HideCursor PrevTrack
                }
              }
            }

            // previous track/selection is cleared; new seltrack is drawn, visible;
            // nothing is selected, cursor should be off

            // confirm correct track/note selection
            SelectedTrack = NewTrack
            SelStart = NewStart
            SelLength = NewLength

            // if track changed to/from track3, need to switch and redraw keyboard
            if (picKeyboard.Visible) {
              if ((PrevTrack = 3 Or NewTrack = 3) And PrevTrack != NewTrack) {
                // set scrollbar properties
                SetKeyboardScroll NewTrack = 3
                NKbOffset = 0;
                MKbOffset = 45;
                DrawKeyboard();
              }
            }

            // if a valid track is selected, make the selection
            if (SelectedTrack != -1) {
              if (SelStart >= 0) {
                // the tree node may not match selected note, if
                // change is made by clicking on the staff

                // deterimine which track was previously selected
                if (tvwSound.SelectedItem.Parent Is Nothing) {
                  lngTreeTrack = -1
                } else {
                  lngTreeTrack = tvwSound.SelectedItem.Parent.Index - 2
                }

                // is selection supposed to be shown?
                if (blnSelVis) {
                  // show cursor or selection
                  ShowSelection
                }

                // if note and/or track has changed from previous selection...
                if (tvwSound.SelectedItem.Tag != SelStart Or lngTreeTrack != SelectedTrack) {
                  Set tmpNode = tvwSound.Nodes(SelectedTrack + 2).Child
                  Do Until tmpNode.Tag = SelStart
                    Set tmpNode = tmpNode.Next
                  Loop
                  tmpNode.Selected = true

                  if (tmpNode.Parent.Index = 5 And PropRowCount != 4) {
                    PropRowCount = 4
                    SetPropertyBoxPos
                  } else {
                    if (PropRowCount != 6) {
                      PropRowCount = 6
                      SetPropertyBoxPos
                    }
                  }
                  // and make sure edit menu is correct
                  SetEditMenu
                }
              } else {
                // select track node
                tvwSound.Nodes(SelectedTrack + 2).Selected = true
                // and make sure edit menu is correct
                SetEditMenu
              }
            } else {
              // select root node
              tvwSound.Nodes(1).Selected = true
              // and make sure edit menu is correct
              SetEditMenu
            }

            // update property window if it doesn't match current property display
            if (PropTrack != SelectedTrack Or PropNote != SelStart) {
              PaintPropertyWindow
            }
            */
        }

        internal void DrawMusicStaff(Graphics gs, int track) {
            //Pen pen = new(Color.Red);
            //pen.Width = 2;
            SolidBrush brush = new(Color.Black);
            //gs.DrawLine(pen, 0, 0, picStaff[track].ClientSize.Width - vsbStaff[track].Width, picStaff[track].ClientSize.Height);
            //gs.DrawLine(pen, 0, picStaff[track].ClientSize.Height, picStaff[track].ClientSize.Width - vsbStaff[track].Width, 0);
            gs.DrawString("staff " + track + ": " + EditSound[track].Notes.Count + " notes", this.Font, brush, 25, 4);

            if (SelectedTrack == track) {
                gs.DrawString($"anchor: {SelAnchor}  start: {SelStart}  length: {SelLength}", this.Font, brush, 25, 16);
                Pen borderpen = new(Color.Blue);
                borderpen.Width = 3;
                gs.DrawRectangle(borderpen, 1, 1, picStaff[track].ClientSize.Width - vsbStaff[track].Width - 3, picStaff[track].ClientSize.Height - 3);
            }
        }

        public void DrawNoiseStaff(Graphics gs) {
            //Pen pen = new(Color.Red);
            //pen.Width = 2;
            SolidBrush brush = new(Color.Black);
            //gs.DrawLine(pen, 0, 0, picStaff[3].ClientSize.Width - vsbStaff[3].Width, picStaff[3].ClientSize.Height);
            //gs.DrawLine(pen, 0, picStaff[3].ClientSize.Height, picStaff[3].ClientSize.Width - vsbStaff[3].Width, 0);
            gs.DrawString("staff " + 3 + ": " + EditSound[3].Notes.Count + " notes", this.Font, brush, 25, 4);

            if (SelectedTrack == 3) {
                gs.DrawString($"anchor: {SelAnchor}  start: {SelStart}  length: {SelLength}", this.Font, brush, 25, 16);
                Pen borderpen = new(Color.Blue);
                borderpen.Width = 3;
                gs.DrawRectangle(borderpen, 1, 1, picStaff[3].ClientSize.Width - vsbStaff[3].Width - 3, picStaff[3].ClientSize.Height - 3);
            }
        }

        private void SetHScroll() {

            // calculate width of sound that doesn't fit in display
            // (total sound length minus what will fit in display;
            // value is negative if the sound is small enough to fit
            // without a scrollbar)
            int tmpWidth = (int)(EditSound.Length * 60 + (KeyWidth + 24 - hsbStaff.Width) / (TICK_WIDTH * StaffScale));

            // show horizontal scrollbar if necessary
            hsbStaff.Enabled = (tmpWidth > 0);
            if (hsbStaff.Enabled) {
                hsbStaff.Maximum = tmpWidth;
            }
            else {
                // if not needed, reset Max to 0
                hsbStaff.Maximum = 0;
                hsbStaff.Value = 0;
            }
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
                    DrawNote(g, EditorResources.note16up, new(4, 20, 24, 40), EditPalette[DefAttn]);
                    break;
                case 2:
                    // eighth note
                    DrawNote(g, EditorResources.note8up, new(4, 20, 24, 40), EditPalette[DefAttn]);
                    break;
                case 3:
                    // eighth note dotted
                    DrawNote(g, EditorResources.note8up, new(2, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 26, 53, 5, 5);
                    break;
                case 4:
                    // quarter note
                    DrawNote(g, EditorResources.note4up, new(8, 20, 24, 40), EditPalette[DefAttn]);
                    break;
                case 5:
                    // quater note tied to sixteenth note
                    // draw quarter
                    DrawNote(g, EditorResources.note4up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNote(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw sixteenth
                    DrawNote(g, EditorResources.note16up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    break;
                case 6:
                    // quarter note dotted
                    // draw quarter
                    DrawNote(g, EditorResources.note4up, new(4, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 21, 51, 5, 5);
                    break;
                case 7:
                    // quarter note double dotted
                    // draw quarter
                    DrawNote(g, EditorResources.note4up, new(3, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 19, 51, 5, 5);
                    // draw dot
                    g.FillEllipse(brush, 26, 51, 5, 5);
                    break;
                case 8:
                    // half note
                    DrawNote(g, EditorResources.note2up, new(8, 20, 24, 40), EditPalette[DefAttn]);
                    break;
                case 9:
                    // half note tied to sixteenth
                    DrawNote(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNote(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw sixteenth
                    DrawNote(g, EditorResources.note16up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    break;
                case 10:
                    // half note tied to eighth
                    DrawNote(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNote(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw eighth
                    DrawNote(g, EditorResources.note8up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    break;
                case 11:
                    // half note tied to dotted eighth
                    DrawNote(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNote(g, EditorResources.connectorup, new(3, 49, 21, 6), EditPalette[DefAttn]);
                    // draw eighth note
                    DrawNote(g, EditorResources.note8up, new(19, 25, 12, 22), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 29, 43, 3, 3);
                    break;
                case 12:
                    // half note dotted
                    DrawNote(g, EditorResources.note2up, new(4, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 21, 51, 5, 5);
                    break;
                case 13:
                    // half note dotted tied to sixteenth
                    DrawNote(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNote(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw sixteenth
                    DrawNote(g, EditorResources.note16up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 11, 53, 3, 3);
                    break;
                case 14:
                    // half note double dotted
                    DrawNote(g, EditorResources.note2up, new(3, 20, 24, 40), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 19, 51, 5, 5);
                    // draw dot
                    g.FillEllipse(brush, 26, 51, 5, 5);
                    break;
                case 15:
                    // half note double dotted tied to sixteenth
                    DrawNote(g, EditorResources.note2up, new(2, 25, 12, 22), EditPalette[DefAttn]);
                    // draw connector
                    DrawNote(g, EditorResources.connectorup, new(3, 49, 22, 6), EditPalette[DefAttn]);
                    // draw sixteenth
                    DrawNote(g, EditorResources.note16up, new(20, 25, 12, 22), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 11, 40, 3, 3);
                    // draw dot
                    g.FillEllipse(brush, 15, 40, 3, 3);
                    break;
                case 16:
                    // whole note
                    DrawNote(g, EditorResources.note1up, new(6, 22, 24, 22), EditPalette[DefAttn]);
                    break;
                }
            }
            else {
                // draw a rest
                switch (DefLength) {
                case 1:
                    // sixteenth rest
                    DrawNote(g, EditorResources.rest16, new(12, 22, 18, 30), EditPalette[DefAttn]);
                    break;
                case 2:
                    // eighth rest
                    DrawNote(g, EditorResources.rest8, new(12, 22, 18, 30), EditPalette[DefAttn]);
                    break;
                case 3:
                    // eighth rest dotted
                    DrawNote(g, EditorResources.rest8, new(9, 22, 18, 30), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 21, 36, 5, 5);
                    break;
                case 4:
                    // quarter rest
                    DrawNote(g, EditorResources.rest4, new(12, 22, 18, 30), EditPalette[DefAttn]);
                    break;
                case 5:
                    // quater rest and sixteenth rest
                    DrawNote(g, EditorResources.rest4, new(8, 26, 12, 20), EditPalette[DefAttn]);
                    DrawNote(g, EditorResources.rest16, new(20, 26, 12, 20), EditPalette[DefAttn]);
                    break;
                case 6:
                    // quarter rest dotted
                    DrawNote(g, EditorResources.rest4, new(9, 22, 18, 30), EditPalette[DefAttn]);
                    // draw dot
                    g.FillEllipse(brush, 21, 36, 5, 5);
                    break;
                case 7:
                    // quarter rest double dotted
                    DrawNote(g, EditorResources.rest4, new(7, 22, 18, 30), EditPalette[DefAttn]);
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
                    DrawNote(g, EditorResources.rest16, new(24, 24, 12, 20), EditPalette[DefAttn]);
                    break;
                case 10:
                    // half rest and eighth
                    g.FillRectangle(brush, 4, 32, 14, 5);
                    g.DrawLine(pen, 1, 37, 21, 37);
                    DrawNote(g, EditorResources.rest8, new(24, 24, 12, 20), EditPalette[DefAttn]);
                    break;
                case 11:
                    // half rest, eighth dotted
                    g.FillRectangle(brush, 4, 32, 14, 5);
                    g.DrawLine(pen, 1, 37, 21, 37);
                    DrawNote(g, EditorResources.rest8, new(22, 24, 12, 20), EditPalette[DefAttn]);
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
                    DrawNote(g, EditorResources.rest16, new(27, 24, 12, 20), EditPalette[DefAttn]);
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
                    DrawNote(g, EditorResources.rest16, new(28, 26, 12, 20), EditPalette[DefAttn]);
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

        private static void DrawNote(Graphics g, Bitmap noteimage, Rectangle position, Color forecolor) {
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
            // use force-redraw to update display
            ChangeSelection(SelectedTrack, SelStart, SelLength, true, false, true);
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
            // update any selection
            ChangeSelection(SelectedTrack, SelStart, SelLength);
        }

        public void SetTrackVisibility(int track, bool value) {
            EditSound[track].Visible = value;
            UpdateVisibleStaves();
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

        public void SetNoteDuration(int track, int note, int newdur, bool DontUndo = false) {
            if (!DontUndo) {
                SoundUndo NextUndo = new();
                NextUndo.UDAction = EditNoteDuration;
                NextUndo.UDTrack = track;
                NextUndo.UDStart = note;
                NextUndo.UDData = EditSound[track].Notes[note].Duration;
                AddUndo(NextUndo);
            }
            EditSound[track].Notes[note].Duration = newdur;
            if (SelectionMode == SelectionModeType.MusicNote &&
                SelectedTrack == track &&
                SelStart == note) {
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
                //break;
                case SoundPlaybackMode.WAV:
                    try {
                        wavNotePlayer.StopWavNote();
                    }
                    catch (Exception) {
                        throw;
                    }
                    break;
                case SoundPlaybackMode.MIDI:
                    try {
                        midiNotePlayer.StopMIDINote();
                    }
                    catch (Exception) {
                        throw;
                    }
                    break;
                }
                // update keyboard
                if (KeyboardVisible) {
                    picKeyboard.Invalidate();
                }
            }
        }

        private void NoteOn(int Note) {
            // play the note
            // toggle keyboard display

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
                    catch (Exception) {
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
            // set insertion to end of inserted note
            SelStart += AddNoteCol.Count;
            SelAnchor = SelStart;
            SelLength = 0;
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
            // use SET_REDRAW to avoid redrawing the tree until all notes are added)
            tvwSound.BeginUpdate();
            for (int i = InsertPos + AddedNotes.Count; i < tmpNodes.Count - 1; i++) {
                tmpNodes[i].Text = "Note " + i;
            }
            tmpNodes[^1].Text = "End";
            tvwSound.EndUpdate();

            // readjust track length, staff scroll, if necessary
            SetHScroll();

            // set anchor, then change selection
            SelAnchor = SelStart;
            // if selecting inserted notes
            if (SelectAll) {
                ChangeSelection(AddTrack, InsertPos, AddedNotes.Count, false, false, true);
            }
            else {
                // set cursor to point at end of inserted note
                ChangeSelection(AddTrack, InsertPos + AddedNotes.Count, 0, false, false, true);
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

            //renumber notes affected by deletion
            tvwSound.BeginUpdate();
            for (int i = DelPos; i < tvwSound.Nodes[0].Nodes[DelTrack].Nodes.Count - 1; i++) {
                tvwSound.Nodes[0].Nodes[DelTrack].Nodes[i].Text = "Note " + i.ToString();
            }
            tvwSound.Nodes[0].Nodes[DelTrack].Nodes[^1].Text = "End";
            tvwSound.EndUpdate();

            // adjust selection
            ChangeSelection(SelectedTrack, DelPos, 0, false, false, true);
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
                if (soundCBData == null) {
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
        }

        public void ImportSound(string importfile) {
            MDIMain.UseWaitCursor = true;
            Sound tmpSound = new();

            //string filedata = "";
            //try {
            //    using FileStream fsSnd = new(importfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //    using StreamReader srSnd = new(fsSnd);
            //    filedata = srSnd.ReadToEnd();
            //    fsSnd.Dispose();
            //    srSnd.Dispose();
            //}
            //catch (Exception e) {
            //    // something wrong
            //    ErrMsgBox(e, "Error occurred while importing sound:", "Unable to load this sound resource", "Import Sound Error");
            //    MDIMain.UseWaitCursor = false;
            //    return;
            //}

            // for now, base import type on file extension
            SoundImportFormat format = SoundImport.GetSoundInputFormat(importfile);
            // import the sound and (and check for error)
            try {
                tmpSound.Import(importfile, format);
            }
            catch (Exception e) {
                // something wrong
                ErrMsgBox(e, "Error occurred while importing sound:", "Unable to load this sound resource", "Import Sound Error");
                MDIMain.UseWaitCursor = false;
                return;
            }
            tmpSound.Load();
            if (tmpSound.ErrLevel < 0) {
                MDIMain.UseWaitCursor = false;
                ErrMsgBox(tmpSound.ErrLevel, "Error reading sound data:", "This is not a valid sound resource.", "Invalid Sound Resource");
                return;
            }
            if (tmpSound.SndFormat != SoundFormat.AGI) {
                MessageBox.Show(MDIMain,
                    "This is an Apple IIgs formatted sound. Only PC/PCjr sounds can be edited in WinAGI.",
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
                //EditSound.CloneFrom(EditGame.Sounds[SoundNumber]);// WHY?????
                if (!blnLoaded) {
                    EditGame.Sounds[SoundNumber].Unload();
                }
                RefreshTree(AGIResType.Sound, SoundNumber);
                if (WinAGISettings.AutoExport.Value) {
                    EditSound.Export(EditGame.ResDir + EditSound.ID + ".ags");
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
                        //UpdateStatusBar();
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
                    MDIMain.toolStrip1.Items["btnAddRemove"].Image = MDIMain.imageList1.Images[20];
                    MDIMain.toolStrip1.Items["btnAddRemove"].Text = "Remove Sound";
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
                    Text = sDM + Text;
                }
                if (EditSound.ID != oldid) {
                    if (File.Exists(EditGame.ResDir + oldid + ".agp")) {
                        SafeFileMove(EditGame.ResDir + oldid + ".agp", EditGame.ResDir + EditGame.Sounds[NewResNum].ID + ".agp", true);
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
                        Text = sDM + Text;
                    }
                }
            }
        }

        private bool AskClose() {
            if (EditSound.ErrLevel < 0) {
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
                MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = true;
                Text = sDM + Text;
            }
        }

        private void MarkAsSaved() {
            IsChanged = false;
            Text = sSNDED + ResourceName(EditSound, InGame, true);
            mnuRSave.Enabled = false;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = false;
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
            get { return sound.Length; }
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
            this.noteindex = note;
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
                    //3: B, A#, A
                    EMidiNote[] values = [EMidiNote.A, EMidiNote.AsBf, EMidiNote.B];
                    return new StandardValuesCollection(values);
                case 10:
                    //10: G, F#, F, E, D#, D, C#, C
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
        int track;
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
