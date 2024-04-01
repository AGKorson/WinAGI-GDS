using System;
using System.Text;
using System.IO;
using static WinAGI.Engine.Base;
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using System.Windows.Forms;

namespace WinAGI.Engine {
    internal class AudioPlayer : NativeWindow, IDisposable {
        private bool disposed = false;
        internal bool blnPlaying;
        internal byte PlaySndResNum;
        internal Sound SndPlaying;
        internal byte[] mMIDIData;

        internal AudioPlayer() {
            CreateParams cpSndPlayer = new() {
                Caption = String.Empty,
            };
            CreateHandle(cpSndPlayer);
        }

        protected override void WndProc(ref Message m) {
            // Listen for messages that are sent to the sndplayer window.
            switch (m.Msg) {
            case MM_MCINOTIFY:
                // determine success status
                bool blnSuccess = (m.WParam == MCI_NOTIFY_SUCCESSFUL);
                // close the sound
                _ = mciSendString("close all", null, 0, 0);
                // raise the 'done' event
                SndPlaying.Raise_SoundCompleteEvent(blnSuccess);
                // reset the flag
                blnPlaying = false;
                // release the object
                SndPlaying = null;
                break;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Plays the wav or midi output of an AGI sound resource. MSDOS sounds are
        /// converted to MIDI; IIg sounds are natively MIDI or wav.
        /// </summary>
        /// <param name="SndRes"></param>
        internal void PlaySound(Sound SndRes) {
            string strTempFile;
            int rtn;
            string strMode;
            StringBuilder strError = new(255);
            // create MIDI sound file
            strTempFile = Path.GetTempFileName();
            FileStream fsMidi = new(strTempFile, FileMode.Open);
            fsMidi.Write(SndRes.MIDIData);
            fsMidi.Dispose();
            //if midi (format 1 or 3 converted from agi, or native IIg midi)
            if (SndRes.SndFormat == SoundFormat.sfAGI || SndRes.SndFormat == SoundFormat.sfMIDI) {
                strMode = "sequencer";
            }
            else {
                strMode = "waveaudio";
            }
            // open midi file and assign alias
            rtn = mciSendString("open " + strTempFile + " type " + strMode + " alias " + SndRes.ID, null, 0, IntPtr.Zero);
            // check for error
            if (rtn != 0) {
                _ = mciGetErrorString(rtn, strError, 255);
                WinAGIException wex = new(LoadResString(628)) {
                    HResult = WINAGI_ERR + 628,
                };
                wex.Data["error"] = strError;
                throw wex;
            }
            blnPlaying = true;
            PlaySndResNum = SndRes.Number;
            SndPlaying = SndRes;
            // play the file
            _ = mciSendString("play " + SndRes.ID + " notify", null, 0, Handle);
            // check for errors
            if (rtn != 0) {
                _ = mciGetErrorString(rtn, strError, 255);
                // reset playing flag
                blnPlaying = false;
                SndPlaying = null;
                // close sound
                _ = mciSendString("close all", null, 0, 0);
                // return the error
                WinAGIException wex = new(LoadResString(628)) {
                    HResult = WINAGI_ERR + 628,
                };
                wex.Data["error"] = strError;
                throw wex;
            }
        }

        internal void StopSound() {
            _ = mciSendString("close all", null, 0, (IntPtr)null);
            blnPlaying = false;
        }

        public void Dispose() {
            Dispose(disposing: true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // check to see if Dispose has already been called
            if (!disposed) {
                // if disposing is true, dispose all managed and unmanaged resources
                if (disposing) {
                    DestroyHandle();
                }
                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                DestroyHandle();
                // Note disposing has been done.
                disposed = true;
            }
        }

        // Use C# finalizer syntax for finalization code.
        // This finalizer will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide finalizer in types derived from this class.
        ~AudioPlayer() {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(disposing: false) is optimal in terms of
            // readability and maintainability.
            Dispose(disposing: false);
        }
    }
}
