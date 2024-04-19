using NAudio.Wave;
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
        // sound subclassing variables, constants, declarations
        internal static MIDIPlayer midiPlayer = new();
        internal static WAVPlayer wavPlayer = new();
        internal static bool bPlayingMIDI = false;
        internal static bool bPlayingWAV = false;
        internal static Sound soundPlaying;

        /// <summary>
        /// If a sound is playing, this method stops it, regardless of mode. No effect
        /// if no sound is playing.
        /// </summary>
        internal static void StopAllSound() {
            // this will send a msg to WndProc which will raise the 'sound complete'
            // event and release the sound object
            _ = mciSendString("close all", null, 0, (IntPtr)null);
            bPlayingWAV = false;
            bPlayingMIDI = false;
            wavPlayer.Reset();
        }
    }

    /// <summary>
    /// A class for writing data stream to be played as a MIDI  or WAV sound.
    /// </summary>
    internal class SoundData() {
        public byte[] Data = [];
        public int Pos = 0;

        /// <summary>
        /// Writes a four byte long value to midi array data.
        /// </summary>
        /// <param name="ByteIn"></param>
        internal void WriteSndByte(byte ByteIn) {
            Data[Pos++] = ByteIn;
            if (Pos >= Data.Length) {
                // bump up size to hold more data
                Array.Resize(ref Data, Pos + 256);
            }
        }

        /// <summary>
        /// Writes a two byte integer value to midi array data.
        /// </summary>
        /// <param name="IntegerIn"></param>
        internal void WriteSndWord(int IntegerIn) {
            WriteSndByte((byte)(IntegerIn / 256));
            WriteSndByte((byte)(IntegerIn & 0xFF));
        }

        /// <summary>
        /// Writes a four byte long value to midi array data.
        /// </summary>
        /// <param name="LongIn"></param>
        internal void WriteSndLong(int LongIn) {
            WriteSndByte((byte)(LongIn >> 24));
            WriteSndByte((byte)((LongIn >> 16) & 0xFF));
            WriteSndByte((byte)((LongIn >> 8) & 0xFF));
            WriteSndByte((byte)(LongIn & 0xFF));
        }

        /// <summary>
        /// Writes variable delta times to midi data array.
        /// </summary>
        /// <param name="LongIn"></param>
        internal void WriteSndDelta(int LongIn) {
            int i = LongIn >> 21;
            if ((i > 0)) {
                WriteSndByte((byte)((i & 127) | 128));
            }
            i = LongIn >> 14;
            if (i > 0) {
                WriteSndByte((byte)((i & 127) | 128));
            }
            i = LongIn >> 7;
            if ((i > 0)) {
                WriteSndByte((byte)((i & 127) | 128));
            }
            WriteSndByte((byte)(LongIn & 127));
        }
    }

    /// <summary>
    /// A class for playing AGI Sounds as a MIDI stream.
    /// </summary>
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
        /// Plays a MIDI sound, either an MSDOS sound converted to MIDI, or a native IIg MIDI sound.
        /// </summary>
        /// <param name="SndRes"></param>
        internal void PlayMIDISound(Sound SndRes) {
            StringBuilder strError = new(255);

            // Stop any currently playing sound.
            StopAllSound();

            // create MIDI sound file
            string strTempFile = Path.GetTempFileName();
            FileStream fsMidi = new(strTempFile, FileMode.Open);
            fsMidi.Write(SndRes.MIDIData);
            fsMidi.Dispose();
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

    /// <summary>
    /// A class for playing WAV data streams.
    /// </summary>
    internal class WAVPlayer {
        private const int SAMPLE_RATE = 44100;
        // WAVPlayer code based on prior work by Lance Ewing in 
        // AGILE. Thank you Lance

        /// <summary>
        /// The Thread that is waiting for the sound to finish.
        /// </summary>
        private Thread playerThread;

        /// <summary>
        /// NAudio output device that will play the generated WAVE data.
        /// </summary>
        private IWavePlayer outputDevice;

        /// <summary>
        /// NAudio ISampleProvider that mixes multiple sounds together.
        /// </summary>
        private MixingSampleProvider mixer;

        private SoundFormat soundFormat;

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
        /// Constructor for WAVPlayer.
        /// </summary>
        public WAVPlayer() {
            // Set up the NAudio mixer. Using a single WaveOutEvent instance, and associated mixer eliminates
            // delays caused by creation of a WaveOutEvent per sound.

            // default is AGI pcjr sound format
            soundFormat = SoundFormat.sfAGI;
            bPlayingWAV = false;
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SAMPLE_RATE, 2)) {
                ReadFully = true
            };
            outputDevice = new WaveOutEvent();
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        /// <summary>
        /// This method creates a WAV audio data stream from a PCjr formatted sound resource
        /// for playback.
        /// </summary>
        /// <param name="sound"></param>
        /// <returns></returns>
        internal byte[] BuildPCjrWAV(Sound sound) {
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

            // store it for use when the sound is played
            return sampleStream.ToArray();
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
        internal void PlayWAVSound(Sound sound) {
            soundPlaying = sound;
            soundFormat = sound.SndFormat;

            StopAllSound();
            // Now play the Wave file.
            MemoryStream memoryStream = new(sound.WAVData);
            playerThread = new Thread(() => PlayWaveStreamAndWait(memoryStream));
            playerThread.Start();
        }

        /// <summary>
        /// Plays the Wave file data from the given MemoryStream.
        /// </summary>
        /// <param name="waveStream">The MemoryStream containing the Wave file data to play.</param>
        private void PlayWaveStreamAndWait(MemoryStream waveStream) {
            bPlayingWAV = true;
            PlayWithNAudioMix(waveStream);
            //PlayWithWaveOut(waveStream);
            // The above call blocks until the sound has finished playing. If sound is not on, then it happens immediately.
            bPlayingWAV = false;
            // raise the 'done' event
            soundPlaying?.Raise_SoundCompleteEvent(true);
        }

        /// <summary>
        /// Plays the WAVE file data contained in the given MemoryStream using the NAudio library.
        /// </summary>
        /// <param name="memoryStream">The MemoryStream containing the WAVE file data.</param>
        private void PlayWithNAudioMix(MemoryStream memoryStream) {
            // Add the new sound as an input to the NAudio mixer.
            RawSourceWaveStream rs;
            ISampleProvider soundMixerInput;

            if (soundFormat == SoundFormat.sfAGI) {
                // WAV generated from PCjr sounds are 44100 sample rate, 16bit, two channel
                rs = new(memoryStream, new WaveFormat(44100, 16, 2));
                soundMixerInput = rs.ToSampleProvider();
            }
            else {
                // WAV generated from IIgs sounds are 8000 sample rate, 8bit, one channel
                rs = new(memoryStream, new WaveFormat(8000, 8, 1));
                // to add to mixer, need to convert to IEEE float, 44100, 16bit, 2 channel
                var resampler = new MediaFoundationResampler(rs, new WaveFormat(44100, 16, 2));
                soundMixerInput = resampler.ToSampleProvider();
            }
            mixer.AddMixerInput(soundMixerInput);
            // Register a handler for when this specific sound ends.
            bool playbackEnded = false;
            void handlePlaybackEnded(object sender, SampleProviderEventArgs args) {
                mixer.MixerInputEnded -= handlePlaybackEnded;
                playbackEnded = true;
            };
            mixer.MixerInputEnded += handlePlaybackEnded;
            // Wait until either the sound has ended, or we have been told to stop.
            while (!playbackEnded && bPlayingWAV) {
                Thread.Sleep(10);
            }
            // If we didn't stop due to the playback ending, then tell it to stop playing.
            if (!playbackEnded) {
                mixer.RemoveMixerInput(soundMixerInput);
            }
        }

        private void PlayWithWaveOut(MemoryStream memoryStream) {
            // Add the new sound as an input to the NAudio mixer.
            WaveOutEvent wo = new();
            RawSourceWaveStream rs;
            if (soundFormat == SoundFormat.sfAGI) {
                // WAV generated from PCjr sounds are 44100 sample rate, 16bit, two channel
                rs = new(memoryStream, new WaveFormat(44100, 16, 2));
            }
            else {
                // WAV extracted from IIgs sounds are 8000 sample rate, 8bit, one channel
                rs = new(memoryStream, new WaveFormat(8000, 8, 1));
            }
            wo.Init(rs);
            wo.PlaybackStopped += handlePlaybackEnded;
            wo.Play();
            // Register a handler for when this specific sound ends.
            bool playbackEnded = false;
            void handlePlaybackEnded(object sender, StoppedEventArgs args) {
                playbackEnded = true;
            };

            // Wait until either the sound has ended, or we have been told to stop.
            while (!playbackEnded && bPlayingWAV) {
                Thread.Sleep(10);
            }

            // If we didn't stop due to the playback ending, then tell it to stop playing.
            if (!playbackEnded) {
                wo.Stop();
            }
        }

        /// <summary>
        /// Resets the internal state of the SoundPlayer.
        /// </summary>
        internal void Reset() {
            StopPCjrSound(false);
            mixer.RemoveAllMixerInputs();
        }

        /// <summary>
        /// Fully shuts down the SoundPlayer.
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
        internal void StopPCjrSound(bool wait = true) {
            if (bPlayingWAV) {
                // This tells the thread to stop.
                bPlayingWAV = false;
                if (playerThread != null) {
                    // We wait for the thread to stop only if instructed to do so.
                    if (wait) {
                        while (playerThread.ThreadState != ThreadState.Stopped) {
                            bPlayingWAV = false;
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

            private static float[] volumeTable = [
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
            ];

            private int[] channelVolume = [15, 15, 15, 15];
            private int[] channelCounterReload = new int[4];
            private int[] channelCounter = new int[4];
            private int[] channelOutput = new int[4];
            private uint lfsr;
            private uint latchedChannel;
            private float ticksPerSample;
            private float ticksCount;

            public SN76496() {
                ticksPerSample = IBM_PCJR_CLOCK / 16 / SAMPLE_RATE;
                ticksCount = ticksPerSample;
                latchedChannel = 0;
                lfsr = 0x4000;
            }

            public void SetVolByNumber(int channel, int volume) {
                channelVolume[channel] = volume & 0x0F;
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
                // First Byte of TONE frequency info:
                // 7  6  5  4  3  2  1  0
                // 1  .  .  .  .  .  .  .      Identifies first byte (command byte)
                // .  R0 R1 .  .  .  .  .      Voice number (i.e. channel)
                // .  .  .  0  .  .  .  .      0 = Frequency count
                // .  .  .  .  F6 F7 F8 F9     4 of 10 - bits in frequency count.
                //
                // Second Byte of TONE frequency info:
                // 7  6  5  4  3  2  1  0
                // 0  .  .  .  .  .  .  .      Identifies second byte (completing byte for frequency count)
                // .  X  .  .  .  .  .  .      Unused, ignored.
                // .  .  F0 F1 F2 F3 F4 F5     6 of 10 - bits in frequency count.
                //
                // First Byte of attenuation info:
                // 7  6  5  4  3  2  1  0
                // 1  .  .  .  .  .  .  .      Identifies first byte (command byte)
                // .  R0 R1 .  .  .  .  .      Voice number (i.e. channel)
                // .  .  .  1  .  .  .  .      1 = Update attenuation
                // .  .  .  .  A0 A1 A2 A3     4-bit attenuation value.
                //
                // First Byte of NOISE frequency info:
                // 7  6  5  4  3  2  1  0
                // 1  .  .  .  .  .  .  .      Identifies first byte (command byte)
                // .  1  1  .  .  .  .  .      Voice number (3 for noise channel)
                // .  .  .  0  .  .  .  .      0 = Frequency count
                // .  .  .  .  X  .  .  .      unused; can be ignored
                // .  .  .  .  .  FB .  .      1 for white noise, 0 for periodic
                // .  .  .  .  .  .  F0 F1     noise frequency control bits
                //                             0 0 = 2330 Hz
                //                             0 1 = 1165 Hz
                //                             1 0 = 583 Hz
                //                             1 1 = Borrow from track 3
                //
                int counterReloadValue;
                bool updateVolume = false;

                // check for command byte (first byte of freq info, or attenuation)
                if ((data & 0x80) != 0) {
                    latchedChannel = (uint)(data >> 5) & 0x03;
                    counterReloadValue = data & 0x0F;
                    updateVolume = (data & 0x10) != 0;
                }
                else {
                    counterReloadValue = (int)(((uint)channelCounterReload[latchedChannel]) | (((uint)data & 0x3F) << 4));
                }

                if (updateVolume) {
                    // Update attenuation for latched channel.
                    channelVolume[latchedChannel] = data & 0x0F;
                }
                else {
                    // Data latched. Update counter reload register for channel.
                    channelCounterReload[latchedChannel] = counterReloadValue;

                    // If it is for the noise control register, then set LFSR back to starting value.
                    if (latchedChannel == 3) {
                        lfsr = 0x4000;
                    }
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
                            channelCounter[3] = 0x20 << (channelCounterReload[3] & 3);
                            // 00=>32(2330Hz), 01=>64(1165Hz), 10=>128(583Hz)
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
