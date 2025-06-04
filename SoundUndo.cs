using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAGI.Engine;

namespace WinAGI.Editor {
    internal class SoundUndo {

        public enum SoundUndoType {
            ChangeKey,
            ChangeTPQN,
            ChangeInstrument,
            EditNoteFreqDiv,
            EditNoteDuration,
            EditNoteAttenuation,
            Cut,
            Paste,
            AddNote,
            Delete,
            ShiftTone,
            ShiftVol,
            ClearTrack,
            ClearSound,
        }

        public SoundUndoType UDAction;
        public int UDTrack;
        public int UDStart;
        public int UDLength;
        public string UDText;
        public int UDData;
        private Sound mUDSound;
        private Notes mUDNotes;

        public Notes UDNotes {
            get => mUDNotes;
            set => mUDNotes = value.Clone();
        }
        public Sound UDSound {
            get => mUDSound;
            set => mUDSound = value.Clone();
        }
    }
}
