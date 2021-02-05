using System;
using System.Text;
using System.IO;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Common.API;
using static WinAGI.Common.WinAGI;
using System.Windows.Forms;

namespace WinAGI.Engine
{
  internal class AudioPlayer : NativeWindow, IDisposable
  {
    IntPtr piFormHandle = IntPtr.Zero;
    internal bool blnPlaying;
    internal byte PlaySndResNum;
    internal AGISound SndPlaying;
    internal byte[] mMIDIData;
    internal AudioPlayer()
    {
      CreateParams cpSndPlayer = new CreateParams
      {
        //Style = 1
      };
      this.CreateHandle(cpSndPlayer);
      piFormHandle = this.Handle;
    }
    internal void PlaySound(AGISound SndRes)
    {
      string strTempFile, strShortFile;
      int rtn;
      string strID, strMode;
      StringBuilder strError = new StringBuilder(255);
      //no spaces allowed in id
      strID = SndRes.ID.Replace(" ", "_");
      //create MIDI sound file
      strTempFile = Path.GetTempFileName();
      FileStream fsMidi = new FileStream(strTempFile, FileMode.Open);
      fsMidi.Write(SndRes.MIDIData);
      fsMidi.Dispose();
      //convert to shortname
      strShortFile = ShortFileName(strTempFile);
      //if midi (format 1 or 3)
      if (SndRes.SndFormat == 1 || SndRes.SndFormat == 3) {
        strMode = "sequencer";
      }
      else {
        strMode = "waveaudio";
      }
      //open midi file and assign alias
      rtn = mciSendString("open " + strShortFile + " type " + strMode + " alias " + strID, null, 0, IntPtr.Zero);
      //check for error
      if (rtn != 0) {
        rtn = mciGetErrorString(rtn, strError, 255);
        //return the error
        throw new Exception("678, SndSubclass " + strError.ToString());
      }
      //set playing flag and number of sound being played
      blnPlaying = true;
      PlaySndResNum = SndRes.Number;
      SndPlaying = SndRes;
      //play the file
      rtn = mciSendString("play " + strID + " notify", null, 0, this.Handle);
      //check for errors
      if (rtn != 0) {
        rtn = mciGetErrorString(rtn, strError, 255);
        //reset playing flag
        blnPlaying = false;
        SndPlaying = null;
        //close sound
        rtn = mciSendString("close all", null, 0, (IntPtr)0);
        //return the error
        throw new Exception("628, SndSubclass " + strError.ToString());
      }
    }
    public void Dispose()
    {
      this.DestroyHandle();
    }
    protected override void WndProc(ref Message m)
    {
      bool blnSuccess;
      //check for mci msg
      switch (m.Msg) {
      case MM_MCINOTIFY:
        //determine success status
        blnSuccess = m.WParam.ToInt32() == MCI_NOTIFY_SUCCESSFUL;
        //close the sound
        _ = mciSendString("close all", null, 0, (IntPtr)null);
        //raise the 'done' event, if a sound is playing
        SndPlaying?.Raise_SoundCompleteEvent(blnSuccess);
        //reset the flag
        blnPlaying = false;
        break;
      }
      base.WndProc(ref m);
    }
  }
}
