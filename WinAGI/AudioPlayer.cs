using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using WinAGI.Common;
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {

    public static partial class Base {
        #region Local Members
        // sound subclassing variables, constants, declarations
        internal static MIDIPlayer midiPlayer = new();
        internal static WAVPlayer wavPlayer = new();
        internal static bool bPlayingMIDI = false;
        internal static bool bPlayingWAV = false;
        internal static Sound soundPlaying;
        #endregion

        #region Constructors
        // none
        #endregion

        #region Properties
        // none
        #endregion

        #region Methods
        /// <summary>
        /// If a sound is playing, this method stops it, regardless of mode. No effect
        /// if no sound is playing.
        /// </summary>
        internal static void StopAllSound() {
            // stop MIDI:
            _ = mciSendString("close all", null, 0, (IntPtr)null);
            // stop WAV:
            wavPlayer.Reset();
            bPlayingWAV = false;
            bPlayingMIDI = false;
            soundPlaying = null;
        }
    #endregion
    }

    #region Classes
    /// <summary>
    /// A class for writing data stream to be played as a MIDI sound.
    /// </summary>
    internal class SoundData() {
        #region Local Members
        public byte[] Data = [];
        public int Pos = 0;
        #endregion

        #region Properties
        // none
        #endregion

        #region Methods
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
        #endregion
    }

    /// <summary>
    /// A class for playing AGI Sounds as a MIDI stream.
    /// </summary>
    public class MIDIPlayer : NativeWindow, IDisposable {
        #region Local Members
        private bool disposed = false;
        internal nint hMidi = IntPtr.Zero; // handle to the MIDI output device
        internal byte[] mMIDIData;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a MIDIplayer to play AGI sounds.
        /// </summary>
        public MIDIPlayer() {
            CreateParams cpMIDIPlayer = new() {
                Caption = "",
            };
            CreateHandle(cpMIDIPlayer);
        }
        #endregion

        #region Properties
        // none
        #endregion

        #region Methods
        /// <summary>
        /// Override WndProc method that processes midi messages.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m) {
            // Listen for messages that are sent to the sndplayer window.
            switch (m.Msg) {
            case MM_MCINOTIFY:
                // determine success status
                bool blnSuccess = (m.WParam == MCI_NOTIFY_SUCCESSFUL);
                // close the sound
                _ = mciSendString("close all", null, 0, 0);
                // raise the 'done' event
                soundPlaying?.OnSoundComplete(blnSuccess);
                // reset the flag
                bPlayingMIDI = false;
                // release the object
                soundPlaying = null;
                break;
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// Plays a MIDI sound, either an MSDOS sound converted to MIDI, 
        /// or a native IIg MIDI sound.
        /// </summary>
        /// <param name="SndRes"></param>
        public void PlayMIDISound(Sound SndRes) {
            StringBuilder strError = new(255);

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
                WinAGIException wex = new(EngineResourceByNum(526).Replace(
                    ARG1, strError.ToString())) {
                    HResult = WINAGI_ERR + 526,
                };
                wex.Data["error"] = strError.ToString();
                throw wex;
            }
            bPlayingMIDI = true;
            soundPlaying = SndRes;
            // play the file
            rtn = mciSendString("play " + SndRes.ID + " notify", null, 0, Handle);
            // check for errors
            if (rtn != 0) {
                _ = mciGetErrorString(rtn, strError, 255);
                bPlayingMIDI = false;
                soundPlaying = null;
                // close sound
                _ = mciSendString("close all", null, 0, 0);
                WinAGIException wex = new(EngineResourceByNum(526).Replace(
                    ARG1, strError.ToString())) {
                    HResult = WINAGI_ERR + 526,
                };
                wex.Data["error"] = strError.ToString();
                throw wex;
            }
        }

        /// <summary>
        /// Disposes the MIDIPlayer when it is no longer needed.
        /// </summary>
        public void Dispose() {
            Dispose(disposing: true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the MIDIPlayer when it is no longer needed.
        /// </summary>
        /// <param name="disposing"></param>
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

        /// <summary>
        /// Use C# finalizer syntax for finalization code.
        /// This finalizer will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide finalizer in types derived from this class.
        /// </summary>
        ~MIDIPlayer() {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(disposing: false) is optimal in terms of
            // readability and maintainability.
            Dispose(disposing: false);
        }
        #endregion
    }

    /// <summary>
    /// A class for playing AGI sounds as WAV data streams.
    /// </summary>
    public class WAVPlayer {
        // WAVPlayer code based on prior work by Lance Ewing in 
        // AGILE. Thank you Lance.

        #region Local Members
        private const int SAMPLE_RATE = 44100;
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
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a WAVPlayer to play AGI sounds.
        /// </summary>
        public WAVPlayer() {
            // Set up the NAudio mixer. Using a single WaveOutEvent instance, and
            // associated mixer eliminates delays caused by creation of a WaveOutEvent
            // per sound.

            // default is AGI pcjr sound format
            soundFormat = SoundFormat.AGI;
            bPlayingWAV = false;
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SAMPLE_RATE, 2)) {
                ReadFully = true
            };
            outputDevice = new WaveOutEvent();
            outputDevice.Init(mixer);
            outputDevice.Play();
        }
        #endregion

        #region Properties
        // none
        #endregion

        #region Methods
        /// <summary>
        /// This method creates a WAV audio data stream from a PCjr formatted sound resource
        /// for playback.
        /// WAV generated from PCjr sounds are 44100 sample rate, 16bit, two channel.
        /// </summary>
        /// <param name="sound"></param>
        /// <returns></returns>
        internal byte[] BuildPCjrWAV(Sound sound) {
            Note[] voiceCurrentNote = new Note[4];
            bool[] voicePlaying = [!sound[0].Muted, !sound[1].Muted, !sound[2].Muted, !sound[3].Muted];
            int[] voiceSampleCount = new int[4];
            int[] voiceNoteNum = new int[4];
            int[] voiceDissolveCount = new int[4];
            int durationUnitCount = 0;

            // TODO: dissolve should depend on a sound property, not on ingame status
            if (sound.parent is null) {
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
                        if (--voiceSampleCount[voiceNum] <= 0) {
                            if (voiceNoteNum[voiceNum] < sound.Tracks[voiceNum].Notes.Count) {
                                voiceCurrentNote[voiceNum] = sound.Tracks[voiceNum].Notes[voiceNoteNum[voiceNum]++];
                                psg.Write((byte)((voiceCurrentNote[voiceNum].FreqDivisor % 16) + 128 + 32 * voiceNum));
                                psg.Write((byte)(voiceCurrentNote[voiceNum].FreqDivisor / 16));
                                psg.Write((byte)(voiceCurrentNote[voiceNum].Attenuation + (byte)(144 + 32 * voiceNum)));
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
        /// Updates the volume of the given channel, by applying the dissolve data 
        /// and master volume to the given base volume and then sets that in the 
        /// SN76496 PSG. The noise channel does not apply the dissolve data, so skips
        /// that bit.
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
        public void PlayWAVSound(Sound sound) {
            StopAllSound();
            soundPlaying = sound;
            soundFormat = sound.SndFormat;

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
            // The above call does not return until the sound has finished playing.
            bPlayingWAV = false;
            soundPlaying?.OnSoundComplete(true);
        }

        /// <summary>
        /// Plays the WAV data contained in the given MemoryStream using
        /// a mixer from the NAudio library.
        /// </summary>
        /// <param name="memoryStream">The MemoryStream containing the WAVE file data.</param>
        private void PlayWithNAudioMix(MemoryStream memoryStream) {
            // Add the new sound as an input to the NAudio mixer.
            RawSourceWaveStream rs;
            ISampleProvider soundMixerInput;

            if (soundFormat == SoundFormat.AGI) {
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

        /// <summary>
        /// Plays the WAV data contained in the given MemoryStream using
        /// the WaveOutEvent method from the NAudio library
        /// </summary>
        /// <param name="memoryStream"></param>
        private void PlayWithWaveOut(MemoryStream memoryStream) {
            WaveOutEvent wo = new();
            RawSourceWaveStream rs;
            if (soundFormat == SoundFormat.AGI) {
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
        /// <param name="wait">true to wait for the player thread to stop; otherwise
        /// false to not wait.</param>
        internal void StopPCjrSound(bool wait = true) {
            if (bPlayingWAV) {
                // This tells the thread to stop.
                bPlayingWAV = false;
                if (playerThread is not null) {
                    // We wait for the thread to stop only if instructed to do so.
                    if (wait) {
                        while (playerThread.ThreadState != System.Threading.ThreadState.Stopped) {
                            bPlayingWAV = false;
                            Thread.Sleep(10);
                        }
                    }
                    playerThread = null;
                }
            }
        }
        #endregion

        #region Classes
        /// <summary>
        /// A class to emulate the SN76496 audio chip. The SN76496 is the audio
        /// chip used in the IBM PC JR and therefore what the original AGI sound 
        /// format was designed for.
        /// </summary>
        public sealed class SN76496 {
            #region Local Members
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
            #endregion

            #region Constructors
            /// <summary>
            /// Initializes the SN76496 audio chip emulator.
            /// </summary>
            public SN76496() {
                ticksPerSample = IBM_PCJR_CLOCK / 16 / SAMPLE_RATE;
                ticksCount = ticksPerSample;
                latchedChannel = 0;
                lfsr = 0x4000;
            }
            #endregion

            #region Properties
            // none
            #endregion

            #region Methods
            /// <summary>
            /// Sets volume for specified channel.
            /// </summary>
            /// <param name="channel"></param>
            /// <param name="volume"></param>
            public void SetVolByNumber(int channel, int volume) {
                channelVolume[channel] = volume & 0x0F;
            }

            /// <summary>
            /// Gets volume for specified channel
            /// </summary>
            /// <param name="channel"></param>
            /// <returns></returns>
            public int GetVolByNumber(int channel) {
                return (channelVolume[channel] & 0x0F);
            }

            /// <summary>
            /// Converts a Sierra/IBM PCjr tone data byte into the corresponding
            /// SN76496 data output to reproduce the correct sound.
            /// </summary>
            /// <param name="data"></param>
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

            /// <summary>
            /// Updates the tone channel.
            /// </summary>
            /// <param name="channel"></param>
            private void UpdateToneChannel(int channel) {
                // If the tone counter reload register is 0, then skip update.
                if (channelCounterReload[channel] == 0) return;

                // Note: For some reason SQ2 intro, in docking scene, is quite sensitive
                // to how this is decremented and tested.

                // Decrement channel counter. If zero, then toggle output and reload from
                // the tone counter reload register.
                if (--channelCounter[channel] <= 0) {
                    channelCounter[channel] = channelCounterReload[channel];
                    channelOutput[channel] ^= 1;
                }
            }

            /// <summary>
            /// Creates the WAV samples from the audio chip based on currently
            /// playing tone channels.
            /// </summary>
            /// <returns></returns>
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
            #endregion
        }
        #endregion
    }

    /// <summary>
    /// A class for playing MIDI notes using the Windows MIDI API.
    /// </summary>
    public class MidiNotePlayer : IDisposable {
        private nint hMidi = IntPtr.Zero;
        private bool isPlaying = false;
        private bool disposed = false;
        private int playnote = 0;

        public MidiNotePlayer() {
            // don't initialize MIDI here
        }

        public void InitMidi() {
            // if already initialized, do nothing
            if (hMidi == IntPtr.Zero) {
                int rtn = midiOutOpen(ref hMidi, -1, 0, 0, 0);
                if (rtn == 0) {
                    _ = midiOutReset(hMidi);
                }
            }
        }

        public void KillMidi() {
            if (hMidi != IntPtr.Zero) {
                _ = midiOutClose(hMidi);
                hMidi = IntPtr.Zero;
            }
        }

        public void PlayMIDINote(int instrument, int note) {
            if (isPlaying) {
                // If a note is already playing, stop it first.
                StopMIDINote();
            }
            // instrument message:
            // tone noise: 0x6C0
            // white noise: 0x7AC0
            // music note: 0x##C0 (where ## is the instrument number)

            // for noise note, calculate note value as follows:
            //   midinote = Math.Log10(2330.4296875 / Math.Pow(2, note & 3)) / LOG10_1_12 - 64;
            
            // if not enabled, do nothing
            if (hMidi == IntPtr.Zero) {
                return;
            }

            // change instrument
            midiOutShortMsg(hMidi, instrument);
            // send note on
            playnote = note; // 0x7F is the velocity, 0x90 is the note on message
            midiOutShortMsg(hMidi, note * 0x100 + 0x7F0090);
            isPlaying = true;
        }

        public void StopMIDINote() {
            if (hMidi != 0) {
                // send note off
                midiOutShortMsg(hMidi, playnote * 0x100 + 0x80);
                // reset so other midi actions can be performed
                midiOutReset(hMidi);

            }
            isPlaying = false;
        }

        /// <summary>
        /// Disposes the MidiNotePlayer when it is no longer needed.
        /// </summary>
        public void Dispose() {
            Dispose(disposing: true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the MidiNotePlayer when it is no longer needed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            // check to see if Dispose has already been called
            if (!disposed) {
                // if disposing is true, dispose all managed and unmanaged resources
                if (disposing) {
                    // Close the MIDI output device handle
                    if (hMidi != IntPtr.Zero) {
                        int rtn = midiOutClose(hMidi);
                        if (rtn != 0) {
                            StringBuilder strError = new(255);
                            _ = mciGetErrorString(rtn, strError, 255);
                            WinAGIException wex = new(EngineResourceByNum(526).Replace(
                                ARG1, strError.ToString())) {
                                HResult = WINAGI_ERR + 526,
                            };
                            wex.Data["error"] = strError.ToString();
                            throw wex;
                        }
                        hMidi = IntPtr.Zero;
                    }
                }
                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;
            }
        }

        /// <summary>
        /// Use C# finalizer syntax for finalization code.
        /// This finalizer will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide finalizer in types derived from this class.
        /// </summary>
        ~MidiNotePlayer() {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(disposing: false) is optimal in terms of
            // readability and maintainability.
            Dispose(disposing: false);
        }
    }

    /// <summary>
    /// A class for playing AGI notes using the PCjr chip emulator.
    /// </summary>
    public class WavNotePlayer : IDisposable {
        private WaveOutEvent outputDevice;
        private BufferedWaveProvider waveProvider;
        private Thread playThread;
        private bool isPlaying = false;
        private int currentFreqDivisor = -1;
        private bool disposed = false;

        private const int SAMPLE_RATE = 44100;

        public WavNotePlayer() {
            outputDevice = new WaveOutEvent();
            waveProvider = new BufferedWaveProvider(new WaveFormat(SAMPLE_RATE, 16, 2)) {
                BufferLength = SAMPLE_RATE * 4, // 2 seconds buffer
                DiscardOnBufferOverflow = true
            };
            outputDevice.Init(waveProvider);
            outputDevice.Play();
        }

        /// <summary>
        /// Starts playing a note with the given FreqDivisor.
        /// </summary>
        public void PlayWavNote(int freqDivisor, int channel = 0, int attenuation = 0) {
            StopWavNote();
            isPlaying = true;
            currentFreqDivisor = freqDivisor;

            playThread = new Thread(() => PlaySN76496Tone(freqDivisor, channel, attenuation));
            playThread.IsBackground = true;
            playThread.Start();

            outputDevice.Play();
        }

        /// <summary>
        /// Stops the currently playing note.
        /// </summary>
        public void StopWavNote() {
            isPlaying = false;
            if (playThread is not null && playThread.IsAlive) {
                playThread.Join();
                playThread = null;
            }
            waveProvider.ClearBuffer();
            outputDevice.Stop();
        }

        /// <summary>
        /// Generates and streams a tone using the SN76496 emulation.
        /// </summary>
        private void PlaySN76496Tone(int freqDivisor, int channel = 0, int attenuation = 0) {
            // Set up SN76496 for a single note on channel 0, attenuation 0 (loudest)
            var psg = new WAVPlayer.SN76496();

            if (channel < 3) {
                // First byte: 1RR0FFFF (R=channel, F=low 4 bits of freqDivisor)
                int firstByte = 0x80 | ((0 & 0x03) << 5) | (0 << 4) | (freqDivisor & 0x0F);
                // Second byte: 0XXXXXXX (X=high 6 bits of freqDivisor)
                int secondByte = (freqDivisor >> 4) & 0x3F;
                // Write attenuation for channel 0 (0=loudest)
                int attnByte = 0x90 | (0 & 0x03) << 5 | (attenuation & 0x0F);
                psg.Write(firstByte);
                psg.Write(secondByte);
                psg.Write(attnByte);
            }
            else {
                // Noise channel (3)
                // freqDivisor: bits 0-1 = frequency, bit 2 = noise type (0=periodic, 1=white)
                // SN76496 expects: 1 1 0 0 N T F F (N=noise type, T/F=frequency bits)
                // Command byte: 0xE0 | (freqDivisor & 0x07)
                int noiseByte = 0xE0 | (freqDivisor & 0x07);
                int attnByte = 0xF0 | (attenuation & 0x0F);

                psg.Write(noiseByte);
                psg.Write(attnByte);
            }

            int samplesPerBuffer = SAMPLE_RATE / 10; // 0.1s buffer
            short[] buffer = new short[samplesPerBuffer * 2]; // stereo

            while (isPlaying) {
                for (int i = 0; i < samplesPerBuffer; i++) {
                    // Render returns a float; scale to 16-bit PCM
                    float sampleF = psg.Render();
                    short sample = (short)Math.Clamp(sampleF, short.MinValue, short.MaxValue);
                    buffer[i * 2] = sample;     // Left
                    buffer[i * 2 + 1] = sample; // Right
                }
                byte[] byteBuffer = new byte[buffer.Length * 2];
                Buffer.BlockCopy(buffer, 0, byteBuffer, 0, byteBuffer.Length);
                waveProvider.AddSamples(byteBuffer, 0, byteBuffer.Length);
                Thread.Sleep(100); // match buffer duration
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    StopWavNote();
                    outputDevice.Dispose();
                }
                disposed = true;
            }
        }

        ~WavNotePlayer() {
            Dispose(false);
        }
    }
    #endregion

}
