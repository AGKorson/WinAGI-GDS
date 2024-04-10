﻿using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using static WinAGI.Engine.Base;
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using System.Windows.Forms;

namespace WinAGI.Engine {

    public static partial class Base {
        //sound subclassing variables, constants, declarations
        internal static MIDIPlayer midiPlayer = new();
        internal static PCjrPlayer pcjrPlayer = new();
        internal static WAVPlayer WAVPlayer = new();
        internal static bool bPlayingMIDI = false;
        internal static bool bPlayingPCjr = false;
        internal static bool bPlayingWAV = false;
        internal static Sound soundPlaying;

        internal static void StopAllSound() {
            _ = mciSendString("close all", null, 0, (IntPtr)null);
            bPlayingWAV = false;
            bPlayingMIDI = false;
            // TODO: how to stop a PCjr sound?
            soundPlaying = null;
        }
    }

    internal class MIDIPlayer : NativeWindow, IDisposable {
        private bool disposed = false;
        internal byte[] mMIDIData;

        internal MIDIPlayer() {
            CreateParams cpMIDIPlayer = new() {
                Caption = "",
            };
            CreateHandle(cpMIDIPlayer);
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
                soundPlaying.Raise_SoundCompleteEvent(blnSuccess);
                // reset the flag
                bPlayingMIDI = false;
                // release the object
                soundPlaying = null;
                break;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Plays a MIDI sound, either an MSDOS sound converted to MIDI, or a native IIg NIDI sound.
        /// </summary>
        /// <param name="SndRes"></param>
        internal void PlayMIDISound(Sound SndRes) {
            StringBuilder strError = new(255);

            // create MIDI sound file
            string strTempFile = Path.GetTempFileName();
            FileStream fsMidi = new(strTempFile, FileMode.Open);
            fsMidi.Write(SndRes.MIDIData);
            fsMidi.Dispose();
            // if midi (format 1 or 3 converted from agi, or native IIg midi)
            if (SndRes.SndFormat != SoundFormat.sfAGI && SndRes.SndFormat != SoundFormat.sfMIDI) {
                // exception
                WinAGIException wex = new WinAGIException(LoadResString(705)) {
                    HResult = 705,
                };
                throw wex;
            }
            // open midi file and assign alias
            int rtn = mciSendString("open " + strTempFile + " type sequencer alias " + SndRes.ID, null, 0, IntPtr.Zero);
            // check for error
            if (rtn != 0) {
                _ = mciGetErrorString(rtn, strError, 255);
                WinAGIException wex = new(LoadResString(628)) {
                    HResult = WINAGI_ERR + 628,
                };
                wex.Data["error"] = strError;
                throw wex;
            }
            bPlayingMIDI = true;
            soundPlaying = SndRes;
            // play the file
            rtn = mciSendString("play " + SndRes.ID + " notify", null, 0, Handle);
            // check for errors
            if (rtn != 0) {
                _ = mciGetErrorString(rtn, strError, 255);
                // reset playing flag
                bPlayingMIDI = false;
                soundPlaying = null;
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
        ~MIDIPlayer() {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(disposing: false) is optimal in terms of
            // readability and maintainability.
            Dispose(disposing: false);
        }
    }

    internal class WAVPlayer : NativeWindow, IDisposable {
        private bool disposed = false;
        internal byte[] mMIDIData;

        internal WAVPlayer() {
            CreateParams cpWAVPlayer = new() {
                Caption = "",
            };
            CreateHandle(cpWAVPlayer);
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
                soundPlaying.Raise_SoundCompleteEvent(blnSuccess);
                // reset the flag
                bPlayingMIDI = false;
                // release the object
                soundPlaying = null;
                break;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Plays a IIg WAV sound.
        /// </summary>
        /// <param name="SndRes"></param>
        internal void PlayWAVSound(Sound SndRes) {
            StringBuilder strError = new(255);

            // create WAV sound file
            string strTempFile = Path.GetTempFileName();
            FileStream fsWAV = new(strTempFile, FileMode.Open);
            // MIDIData stream is used for both MIDI and WAV, depending on sound format
            fsWAV.Write(SndRes.MIDIData);
            fsWAV.Dispose();
            //if not native IIg wav 
            if (SndRes.SndFormat!= SoundFormat.sfWAV) {
                // exception
                WinAGIException wex = new WinAGIException(LoadResString(705)) {
                    HResult = 705,
                };
                throw wex;
            }
            // open wav file and assign alias
            int rtn = mciSendString("open " + strTempFile + " type waveaudio alias " + SndRes.ID, null, 0, IntPtr.Zero);
            // check for error
            if (rtn != 0) {
                _ = mciGetErrorString(rtn, strError, 255);
                WinAGIException wex = new(LoadResString(628)) {
                    HResult = WINAGI_ERR + 628,
                };
                wex.Data["error"] = strError;
                throw wex;
            }
            bPlayingWAV = true;
            soundPlaying = SndRes;
            // play the file
            rtn = mciSendString("play " + SndRes.ID + " notify", null, 0, Handle);
            // check for errors
            if (rtn != 0) {
                _ = mciGetErrorString(rtn, strError, 255);
                // reset playing flag
                bPlayingWAV = false;
                soundPlaying = null;
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
        ~WAVPlayer() {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(disposing: false) is optimal in terms of
            // readability and maintainability.
            Dispose(disposing: false);
        }
    }

    /// <summary>
    /// A class for playing AGI Sounds using an emulated PCjr sound chip.
    /// </summary>
    internal class PCjrPlayer {
        private const int SAMPLE_RATE = 44100;
        // PCjrPlayer code based on prior work by Lance Ewing in 
        // AGILE. Thank you Lance

        /// <summary>
        /// The Thread that is waiting for the sound to finish.
        /// </summary>
        private Thread playerThread;

        /// <summary>
        /// A cache of the generated WAVE data for loaded sounds.
        /// </summary>
        public Dictionary<string, byte[]> SoundCache { get; }

        /// <summary>
        /// NAudio output device that we play the generated WAVE data with.
        /// </summary>
        private readonly WaveOutEvent outputDevice;

        /// <summary>
        /// NAudio ISampleProvider that mixes multiple sounds together.
        /// </summary>
        private readonly MixingSampleProvider mixer;

        private readonly short[] dissolveDataV2 =
        [
              -2,   -3,   -2,   -1, 0x00, 0x00, 0x01, 0x01,
            0x01, 0x01, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03,
            0x03, 0x04, 0x04, 0x04, 0x04, 0x05, 0x05, 0x05,
            0x05, 0x06, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07,
            0x07, 0x07, 0x08, 0x08, 0x08, 0x08, 0x09, 0x09,
            0x09, 0x09, 0x0A, 0x0A, 0x0A, 0x0A, 0x0B, 0x0B,
            0x0B, 0x0B, 0x0B, 0x0B, 0x0C, 0x0C, 0x0C, 0x0C,
            0x0C, 0x0C, 0x0D, -100
        ];

        private readonly short[] dissolveDataV3 =
        [
              -2,   -3,   -2,   -1, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x02,
            0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02,
            0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03,
            0x03, 0x04, 0x04, 0x04, 0x04, 0x04, 0x05, 0x05,
            0x05, 0x05, 0x05, 0x06, 0x06, 0x06, 0x06, 0x06,
            0x07, 0x07, 0x07, 0x07, 0x08, 0x08, 0x08, 0x08,
            0x09, 0x09, 0x09, 0x09, 0x0A, 0x0A, 0x0A, 0x0A,
            0x0B, 0x0B, 0x0B, 0x0B, 0x0B, 0x0B, 0x0C, 0x0C,
            0x0C, 0x0C, 0x0C, 0x0C, 0x0D, -100
        ];

        private short[] dissolveData;

        /// <summary>
        /// Constructor for PCjrPlayer.
        /// </summary>
        public PCjrPlayer() {
            SoundCache = [];
            bPlayingPCjr = false;

            // Set up the NAudio mixer. Using a single WaveOutEvent instance, and associated mixer eliminates
            // delays caused by creation of a WaveOutEvent per sound.
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SAMPLE_RATE, 2)) {
                ReadFully = true
            };
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        /// <summary>
        /// Loads and generates an AGI Sound, caching it in a ready to play state.
        /// </summary>
        /// <param name="sound">The AGI Sound to load.</param>
        public void LoadSoundCache(Sound sound) {
            Note[] voiceCurrentNote = new Note[4];
            bool[] voicePlaying = [true, true, true, true];
            int[] voiceSampleCount = new int[4];
            int[] voiceNoteNum = new int[4];
            int[] voiceDissolveCount = new int[4];
            int durationUnitCount = 0;

            if (sound.parent == null) {
                dissolveData = dissolveDataV2;
            }
            else {
                dissolveData = sound.parent.agIsVersion3 ? dissolveDataV3 : dissolveDataV2;
            }

            // A single note duration unit is 1/60th of a second
            int samplesPerDurationUnit = SAMPLE_RATE / 60;
            MemoryStream sampleStream = new();

            // Create a new PSG for each sound, to guarantee a clean state.
            SN76496 psg = new();

            // Start by converting the Notes into samples.
            while (voicePlaying[0] || voicePlaying[1] || voicePlaying[2] || voicePlaying[3]) {
                for (int voiceNum = 0; voiceNum < 4; voiceNum++) {
                    if (voicePlaying[voiceNum]) {
                        if (voiceSampleCount[voiceNum]-- <= 0) {
                            if (voiceNoteNum[voiceNum] < sound.Track(voiceNum).Notes.Count) {
                                voiceCurrentNote[voiceNum] = sound.Track(voiceNum).Notes[voiceNoteNum[voiceNum]++];
                                byte[] psgBytes = voiceCurrentNote[voiceNum].rawData;
                                psg.Write(psgBytes[3]);
                                psg.Write(psgBytes[2]);
                                psg.Write(psgBytes[4]);
                                voiceSampleCount[voiceNum] = voiceCurrentNote[voiceNum].Duration * samplesPerDurationUnit;
                                voiceDissolveCount[voiceNum] = 0;
                            }
                            else {
                                voicePlaying[voiceNum] = false;
                                psg.SetVolByNumber(voiceNum, 0x0F);
                            }
                        }
                        if ((durationUnitCount == 0) && (voicePlaying[voiceNum])) {
                            voiceDissolveCount[voiceNum] = UpdateVolume(psg, voiceCurrentNote[voiceNum].Attenuation, voiceNum, voiceDissolveCount[voiceNum]);
                        }
                    }
                }
                // This count hits zero 60 times a second. It counts samples from 0 to 734 (i.e. (44100 / 60) - 1).
                durationUnitCount = (durationUnitCount + 1) % samplesPerDurationUnit;

                // Use the SN76496 PSG emulation to generate the sample data.
                short sample = (short)psg.Render();
                sampleStream.WriteByte((byte)(sample & 0xFF));
                sampleStream.WriteByte((byte)((sample >> 8) & 0xFF));
                sampleStream.WriteByte((byte)(sample & 0xFF));
                sampleStream.WriteByte((byte)((sample >> 8) & 0xFF));
            }

            // Cache for use when the sound is played. This reduces overhead of generating WAV on every play.
            SoundCache.Add(sound.ID, sampleStream.ToArray());
        }

        /// <summary>
        /// Updates the volume of the given channel, by applying the dissolve data and master volume to the 
        /// given base volume and then sets that in the SN76496 PSG. The noise channel does not apply the
        /// dissolve data, so skips that bit.
        /// </summary>
        /// <param name="psg">The SN76496 PSG to set the calculated volume in.</param>
        /// <param name="baseVolume">The base volume to apply the dissolve data and master volume to.</param>
        /// <param name="channel">The channel to update the volume for.</param>
        /// <param name="dissolveCount">The current dissolve count value for the note being played by the given channel.</param>
        /// <returns>The new dissolve count value for the channel.</returns>
        private int UpdateVolume(SN76496 psg, int baseVolume, int channel, int dissolveCount) {
            int volume = baseVolume;

            if (volume != 0x0F) {
                int dissolveValue = (dissolveData[dissolveCount] == -100 ? dissolveData[dissolveCount - 1] : dissolveData[dissolveCount++]);

                // Add dissolve value to current channel volume. Noise channel doesn't dissolve.
                if (channel < 3) volume += dissolveValue;
                if (volume < 0) volume = 0;
                if (volume > 0x0F) volume = 0x0F;
                if (volume < 8) volume += 2;
                // Apply calculated volume to PSG channel.
                psg.SetVolByNumber(channel, volume);
            }
            return dissolveCount;
        }

        /// <summary>
        /// Plays the given AGI Sound using emulated PCjr sound chip.
        /// </summary>
        /// <param name="sound">The AGI Sound to play.</param>
        public void PlayPCjrSound(Sound sound) {
            // Stop any currently playing sound.
            StopAllSound();

            soundPlaying = sound;
            // load the sound cache first
            LoadSoundCache(sound);

            // Get WAV data from the cache.
            byte[] waveData = null;
            if (SoundCache.TryGetValue(sound.ID, out waveData)) {
                // Now play the Wave file.
                MemoryStream memoryStream = new(waveData);
                playerThread = new Thread(() => PlayWaveStreamAndWait(memoryStream));
                playerThread.Start();
            }
        }

        /// <summary>
        /// Plays the Wave file data from the given MemoryStream.
        /// </summary>
        /// <param name="waveStream">The MemoryStream containing the Wave file data to play.</param>
        private void PlayWaveStreamAndWait(MemoryStream waveStream) {
            bPlayingPCjr = true;
            PlayWithNAudioMix(waveStream);
            // The above call blocks until the sound has finished playing. If sound is not on, then it happens immediately.
            bPlayingPCjr = false;
            // raise the 'done' event
            soundPlaying.Raise_SoundCompleteEvent(true);
        }

        /// <summary>
        /// Plays the WAVE file data contained in the given MemoryStream using the NAudio library.
        /// </summary>
        /// <param name="memoryStream">The MemoryStream containing the WAVE file data.</param>
        public void PlayWithNAudioMix(MemoryStream memoryStream) {
            // Add the new sound as an input to the NAudio mixer.
            RawSourceWaveStream rs = new(memoryStream, new WaveFormat(44100, 16, 2));
            ISampleProvider soundMixerInput = rs.ToSampleProvider();
            mixer.AddMixerInput(soundMixerInput);

            // Register a handler for when this specific sound ends.
            bool playbackEnded = false;
            void handlePlaybackEnded(object sender, SampleProviderEventArgs args) {
                // It is possible that we get sound overlaps, so we check that this is the same sound.
                if (ReferenceEquals(args.SampleProvider, soundMixerInput)) {
                    mixer.MixerInputEnded -= handlePlaybackEnded;
                    playbackEnded = true;
                }
            };
            mixer.MixerInputEnded += handlePlaybackEnded;

            // Wait until either the sound has ended, or we have been told to stop.
            while (!playbackEnded && bPlayingPCjr) {
                Thread.Sleep(10);
            }

            // If we didn't stop due to the playback ending, then tell it to stop playing.
            if (!playbackEnded) {
                mixer.RemoveMixerInput(soundMixerInput);
            }
        }

        /// <summary>
        /// Resets the internal state of the SoundPlayer.
        /// </summary>
        public void Reset() {
            StopSound();
            SoundCache.Clear();
            mixer.RemoveAllMixerInputs();
        }

        /// <summary>
        /// Fully shuts down the SoundPlayer. Only intended for when AGILE is closing down.
        /// </summary>
        public void Shutdown() {
            Reset();
            outputDevice.Stop();
            outputDevice.Dispose();
        }

        /// <summary>
        /// Stops the currently playing sound
        /// </summary>
        /// <param name="wait">true to wait for the player thread to stop; otherwise false to not wait.</param>
        public void StopSound(bool wait = true) {
            if (bPlayingPCjr) {
                // This tells the thread to stop.
                bPlayingPCjr = false;
                if (playerThread != null) {
                    // We wait for the thread to stop only if instructed to do so.
                    if (wait) {
                        while (playerThread.ThreadState != ThreadState.Stopped) {
                            bPlayingPCjr = false;
                            Thread.Sleep(10);
                        }
                    }
                    playerThread = null;
                }
            }
        }

        /// <summary>
        /// SN76496 is the audio chip used in the IBM PC JR and therefore what the original AGI sound format was designed for.
        /// </summary>
        public sealed class SN76496 {
            private const float IBM_PCJR_CLOCK = 3579545f;

            private static float[] volumeTable = new float[] {
                8191.5f,
                6506.73973474395f,
                5168.4870873095f,
                4105.4752242578f,
                3261.09488758897f,
                2590.37974532693f,
                2057.61177037107f,
                1634.41912530676f,
                1298.26525860452f,
                1031.24875107119f,
                819.15f,
                650.673973474395f,
                516.84870873095f,
                410.54752242578f,
                326.109488758897f,
                0.0f
            };

            private int[] channelVolume = new int[4] { 15, 15, 15, 15 };
            private int[] channelCounterReload = new int[4];
            private int[] channelCounter = new int[4];
            private int[] channelOutput = new int[4];
            private uint lfsr;
            private uint latchedChannel;
            private bool updateVolume;
            private float ticksPerSample;
            private float ticksCount;

            public SN76496() {
                ticksPerSample = IBM_PCJR_CLOCK / 16 / SAMPLE_RATE;
                ticksCount = ticksPerSample;
                latchedChannel = 0;
                updateVolume = false;
                lfsr = 0x4000;
            }

            public void SetVolByNumber(int channel, int volume) {
                channelVolume[channel] = (int)(volume & 0x0F);
            }

            public int GetVolByNumber(int channel) {
                return (channelVolume[channel] & 0x0F);
            }

            public void Write(int data) {
                /*
                 * A tone is produced on a voice by passing the sound chip a 3-bit register address 
                 * and then a 10-bit frequency divisor. The register address specifies which voice 
                 * the tone will be produced on. 
                 * 
                 * The actual frequency produced is the 10-bit frequency divisor given by F0 to F9
                 * divided into 1/32 of the system clock frequency (3.579 MHz) which turns out to be 
                 * 111,860 Hz. Keeping all this in mind, the following is the formula for calculating
                 * the frequency:
                 * 
                 *  f = 111860 / (((Byte2 & 0x3F) << 4) + (Byte1 & 0x0F))
                 */
                int counterReloadValue;

                if ((data & 0x80) != 0) {
                    // First Byte
                    // 7  6  5  4  3  2  1  0
                    // 1  .  .  .  .  .  .  .      Identifies first byte (command byte)
                    // .  R0 R1 .  .  .  .  .      Voice number (i.e. channel)
                    // .  .  .  R2 .  .  .  .      1 = Update attenuation, 0 = Frequency count
                    // .  .  .  .  A0 A1 A2 A3     4-bit attenuation value.
                    // .  .  .  .  F6 F7 F8 F9     4 of 10 - bits in frequency count.
                    latchedChannel = (uint)(data >> 5) & 0x03;
                    counterReloadValue = (int)(((uint)channelCounterReload[latchedChannel] & 0xfff0) | ((uint)data & 0x0F));
                    // Third Byte is volume/attenuation 
                    updateVolume = ((data & 0x10) != 0) ? true : false;
                }
                else {
                    // Second Byte - Frequency count only
                    // 7  6  5  4  3  2  1  0
                    // 0  .  .  .  .  .  .  .      Identifies second byte (completing byte for frequency count)
                    // .  X  .  .  .  .  .  .      Unused, ignored.
                    // .  .  F0 F1 F2 F3 F4 F5     6 of 10 - bits in frequency count.
                    counterReloadValue = (int)(((uint)channelCounterReload[latchedChannel] & 0x000F) | (((uint)data & 0x3F) << 4));
                }

                if (updateVolume) {
                    // Volume latched. Update attenuation for latched channel.
                    channelVolume[latchedChannel] = (data & 0x0F);
                }
                else {
                    // Data latched. Update counter reload register for channel.
                    channelCounterReload[latchedChannel] = counterReloadValue;

                    // If it is for the noise control register, then set LFSR back to starting value.
                    if (latchedChannel == 3) lfsr = 0x4000;
                }
            }

            private void UpdateToneChannel(int channel) {
                // If the tone counter reload register is 0, then skip update.
                if (channelCounterReload[channel] == 0) return;

                // Note: For some reason SQ2 intro, in docking scene, is quite sensitive to how this is decremented and tested.

                // Decrement channel counter. If zero, then toggle output and reload from
                // the tone counter reload register.
                if (--channelCounter[channel] <= 0) {
                    channelCounter[channel] = channelCounterReload[channel];
                    channelOutput[channel] ^= 1;
                }
            }

            public float Render() {
                while (ticksCount > 0) {
                    UpdateToneChannel(0);
                    UpdateToneChannel(1);
                    UpdateToneChannel(2);

                    channelCounter[3] -= 1;
                    if (channelCounter[3] < 0) {
                        // Reload noise counter.
                        if ((channelCounterReload[3] & 0x03) < 3) {
                            channelCounter[3] = (0x20 << (channelCounterReload[3] & 3));
                        }
                        else {
                            // In this mode, the counter reload value comes from tone register 2.
                            channelCounter[3] = channelCounterReload[2];
                        }

                        uint feedback = ((channelCounterReload[3] & 0x04) == 0x04) ?
                            // White noise. Taps bit 0 and bit 1 of the LFSR as feedback, with XOR.
                            ((lfsr & 0x0001) ^ ((lfsr & 0x0002) >> 1)) :
                            // Periodic. Taps bit 0 for the feedback.
                            (lfsr & 0x0001);

                        // LFSR is shifted every time the counter times out. SR is 15-bit. Feedback added to top bit.
                        lfsr = (lfsr >> 1) | (feedback << 14);
                        channelOutput[3] = (int)(lfsr & 1);
                    }

                    ticksCount -= 1;
                }

                ticksCount += ticksPerSample;

                return (float)((volumeTable[channelVolume[0] & 0x0F] * ((channelOutput[0] - 0.5) * 2)) +
                               (volumeTable[channelVolume[1] & 0x0F] * ((channelOutput[1] - 0.5) * 2)) +
                               (volumeTable[channelVolume[2] & 0x0F] * ((channelOutput[2] - 0.5) * 2)) +
                               (volumeTable[channelVolume[3] & 0x0F] * ((channelOutput[3] - 0.5) * 2)));
            }
        }
    }

}
