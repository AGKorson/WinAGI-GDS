using System;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents an AGI Sound resource, with WinAGI extensions.
    /// </summary>
    public class Sound : AGIResource {
        #region Members
        internal Track[] mTrack = new Track[4];
        // sound change status is dependent on track and note data
        // not on the underlying resource data
        bool mSoundChanged = false;
        double mLength;
        int mKey;
        int mTPQN;
        SoundFormat mFormat;
        byte[] midiData = [];
        byte[] wavData = [];
        bool mOutputSet = false; // true if wav and midi data match resource data 
        #endregion

        #region Events
        // Sound resources include an event to notify calling programs when playback
        // of a sound is complete.

        // declare the event delegate, and event
        public delegate void SoundCompleteEventHandler(object? sender, SoundCompleteEventArgs args);
        public event SoundCompleteEventHandler SoundComplete;
        public class SoundCompleteEventArgs : EventArgs {
            public SoundCompleteEventArgs(bool noerror) {
                NoError = noerror;
            }
            public bool NoError { get; }
        }

        internal void OnSoundComplete(bool noerror) {
            // Raise the event in a thread-safe manner using the ?. operator.
            SoundComplete?.Invoke(this, new SoundCompleteEventArgs(noerror));
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor to create a new AGI sound resource that is not part of an AGI game.
        /// </summary>
        public Sound() : base(AGIResType.Sound) {
            // not in a game so resource is always loaded
            mLoaded = true;
            InitSound();
            // use a default ID
            mResID = "NewSound";
        }

        /// <summary>
        /// Internal constructor to create a new or cloned sound resource to  be added
        /// to an AGI game has already been loaded. 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="NewSound"></param>
        internal Sound(AGIGame parent, byte ResNum, Sound NewSound = null) : base(AGIResType.Sound) {
            InitSound(NewSound);
            base.InitInGame(parent, ResNum);
        }

        /// <summary>
        /// Internal constructor to add a new AGI sound resource during initial game load.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="VOL"></param>
        /// <param name="Loc"></param>
        internal Sound(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.Sound) {
            InitSound(null);
            base.InitInGame(parent, AGIResType.Sound, ResNum, VOL, Loc);
            // length is undefined until sound is built
            mLength = -1;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the track object corresponding to index. If tracks are not set,
        /// they are rebuilt first.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Track this[int index] {
            get {
                if (index < 0 || index > 3) {
                    throw new IndexOutOfRangeException("invalid track number");
                }
                return Tracks[index];
            }
        }

        /// <summary>
        /// Gets the track collection for this sound. If tracks are not set,
        /// they are rebuilt first.
        /// </summary>
        public Track[] Tracks {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mTrack;
            }
        }
        /// <summary>
        /// Returns true if the track/note data do not match the AGI Resource data.
        /// </summary>
        public override bool IsChanged {
            get {
                return mSoundChanged; 
            } 
        }
        /// <summary>
        /// Gets or sets the key signature to use when displaying notes for this sound.
        /// </summary>
        public int Key {
            get => mKey;
            set {
                if (value < -7 || value > 7) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (mKey != value) {
                    mKey = value;
                    PropsChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets the sound format for this sound resource. Undefined if sound is not loaded.
        /// </summary>
        public SoundFormat SndFormat {
            //  0 = not loaded
            //  1 = //standard// agi
            //  2 = IIgs sampled sound
            //  3 = IIgs midi
            get {
                if (mLoaded) {
                    return mFormat;
                }
                else {
                    return SoundFormat.Undefined;
                }
            }
        }

        /// <summary>
        /// Gets the length of this sound in seconds.
        /// </summary>
        public double Length {
            get {
                if (!mLoaded) {
                    return -1;
                }
                if (mLength == -1) {
                    mLength = GetSoundLength();
                }
                return mLength;
            }
        }

        /// <summary>
        /// Gets or sets the Ticks Per Quarter Note property for this sound.
        /// </summary>
        public int TPQN {
            get {
                return mTPQN;
            }
            set {
                value = (value / 4) * 4;
                if (value < 4 || value > 64) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (mTPQN != value) {
                    mTPQN = value;
                    PropsChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets the WAV data stream for this sound for playback. Only appliciable for
        /// PCjr and IIgs PCM sounds.
        /// </summary>
        public byte[] WAVData {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mFormat != SoundFormat.AGI && mFormat != SoundFormat.WAV) {
                    WinAGIException wex = new(LoadResString(705)) {
                        HResult = WINAGI_ERR + 705,
                    };
                    throw wex;
                }
                if (!mOutputSet) {
                    try {
                        BuildSoundOutput();
                    }
                    catch (Exception) {
                        // pass along errors
                        throw;
                    }
                }
                return wavData;
            }
        }

        /// <summary>
        /// Gets the MIDI data stream for this sound for playback. Only applicable for
        /// PCjr and IIgs MIDI sounds.
        /// </summary>
        public byte[] MIDIData {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mFormat != SoundFormat.AGI && mFormat != SoundFormat.MIDI) {
                    WinAGIException wex = new(LoadResString(705)) {
                        HResult = WINAGI_ERR + 705,
                    };
                    throw wex;
                }
                if (!mOutputSet) {
                    try {
                        BuildSoundOutput();
                    }
                    catch (Exception) {
                        // pass along errors
                        throw;
                    }
                }
                return midiData;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new sound resource when first instantiated. If NewSound is null, 
        /// a blank sound resource is created. If NewSound is not null, it is cloned into
        /// the new sound.
        /// </summary>
        /// <param name="NewSound"></param>
        private void InitSound(Sound NewSound = null) {
            if (NewSound is null) {
                // create default PC/PCjr sound with no notes in any tracks
                mData = [ 0x08, 0x00, 0x08, 0x00,
                                    0x08, 0x00, 0x08, 0x00,
                                    0xff, 0xff];
                // byte 0/1, 2/2, 4/5, 6/7 = offset to track data
                // byte 8/9 are end of track markers
                mFormat = SoundFormat.AGI;
                for (int i = 0; i < 4; i++) {
                    mTrack[i] = new Track(this);
                }
                mTrack[0].Instrument = 80;
                mTrack[1].Instrument = 80;
                mTrack[2].Instrument = 80;
                // default tqpn is 16
                mTPQN = 16;
                // default key is c
                mKey = 0;
            }
            else {
                // copy base properties
                NewSound.CloneTo(this);
                // copy sound properties
                mKey = NewSound.mKey;
                mTPQN = NewSound.mTPQN;
                mSoundChanged = NewSound.mSoundChanged;
                mLength = NewSound.mLength;
                mFormat = NewSound.mFormat;
                // never copy output build status; cloned sound will have to rebuild it
                mOutputSet = false;
                // clone the tracks
                for (int i = 0; i < 4; i++) {
                    mTrack[i] = NewSound.mTrack[i].Clone(this);
                }
            }
        }

        /// <summary>
        /// Creates an exact copy of this Sound resource.
        /// </summary>
        /// <returns>The Sound resource this method creates.</returns>
        public Sound Clone() {
            // only loaded sounds can be cloned
            WinAGIException.ThrowIfNotLoaded(this);

            Sound clonesound = new();
            // copy base properties
            base.CloneTo(clonesound);
            // copy sound properties
            clonesound.mKey = mKey;
            clonesound.mTPQN = mTPQN;
            clonesound.mSoundChanged = mSoundChanged;
            clonesound.mLength = mLength;
            clonesound.mFormat = mFormat;
            // never copy output build status; cloned sound will have to rebuild it
            clonesound.mOutputSet = false;
            // clone the tracks
            // TODO: cloning tracks needs to be checked- it looks wrong right now
            for (int i = 0; i < 4; i++) {
                clonesound.mTrack[i] = mTrack[i].Clone(clonesound);
            }
            return clonesound;
        }

        public void CloneFrom(Sound SourceSound) {
            // only loaded sounds can be cloned
            WinAGIException.ThrowIfNotLoaded(this);
            WinAGIException.ThrowIfNotLoaded(SourceSound);

            // copy base properties
            base.CloneFrom(SourceSound);
            // copy sound properties
            mKey = SourceSound.mKey;
            mTPQN = SourceSound.mTPQN;
            mSoundChanged = SourceSound.mSoundChanged;
            mLength = SourceSound.mLength;
            mFormat = SourceSound.mFormat;
            // never copy output build status; cloned sound will have to rebuild it
            mOutputSet = false;

            // clone the tracks
            // TODO: cloning tracks needs to be checked
            for (int i = 0; i < 4; i++) {
                mTrack[i].CloneFrom(SourceSound.mTrack[i]);
            }
        }
        
        /// <summary>
        /// This method creates midi and wav output data stream for a sound resource.
        /// </summary>
        void BuildSoundOutput() {
            try {
                switch (mFormat) {
                case SoundFormat.AGI:
                    // standard pc/pcjr is playable as both WAV and MIDI
                    midiData = BuildMIDI(this);
                    mLength = GetSoundLength();
                    wavData = wavPlayer.BuildPCjrWAV(this);
                    break;
                case SoundFormat.WAV:
                    // IIgs pcm sound
                    wavData = BuildIIgsPCM(this);
                    mLength = GetSoundLength();

                    break;
                case SoundFormat.MIDI:
                    // IIgs MIDI sound
                    midiData = BuildIIgsMIDI(this, ref mLength);
                    break;
                }
                mOutputSet = true;
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(596)) {
                    HResult = WINAGI_ERR + 596
                };
                wex.Data["exception"] = e;
                wex.Data["ID"] = mResID;
                throw wex;
            }
        }

        /// <summary>
        /// Calculates the length of a sound resource in seconds.
        /// </summary>
        /// <returns></returns>
        private double GetSoundLength() {
            // this function assumes a sound has been loaded properly
            int i;
            double retval = 0;

            switch (mFormat) {
            case SoundFormat.AGI:
                // standard pc/pcjr resource
                for (i = 0; i <= 3; i++) {
                    if (retval < mTrack[i].Length && !mTrack[i].Muted) {
                        retval = mTrack[i].Length;
                    }
                }
                break;
            case SoundFormat.WAV:
                // IIgs pcm sampling
                // since sampling is at 8000Hz, length is just data length/8000
                retval = ((double)mSize - 54) / 8000;
                break;
            case SoundFormat.MIDI:
                // IIgs midi
                // length has to be calculated during midi build
                BuildSoundOutput();
                retval = mLength;
                break;
            }
            return retval;
        }

        /// <summary>
        /// Extracts sound data from the resource and builds the sound tracks.
        /// </summary>
        /// <returns>Zero if no errors, otherwise an error code:<br />
        /// 1 = invalid track offset<br />
        /// 2 = zero length note<br />
        /// 4 = missing track end marker<br />
        /// 8 = no sound data
        /// </returns>
        internal int LoadTracks() {
            int i, lngLength = 0, lngTLength;
            int lngStart, lngDur = 0;
            short intFreq;
            byte bytAttn;

            // assume no errors
            int retval = 0;
            if (Size < 10) {
                // not enough data to be a sound
                return 8;
            }
            // extract note information for each track from resource
            for (i = 0; i <= 3; i++) {
                lngStart = ReadWord(i * 2);
                lngTLength = 0;
                // validate track data location
                if (lngStart > mSize - 1) {
                    // invalid track offset
                    retval |= 1;
                    continue;
                }
                else {
                    // step through notes in this track (5 bytes at a time)
                    Pos = lngStart;
                    while (Pos < Size - 1) {
                        lngDur = ReadWord(Pos);
                        if (lngDur == 0xFFFF) {
                            break;
                        }
                        if (i < 3) {
                            // TONE channel:
                            // duration
                            if (lngDur > 0) {
                                if (Pos + 2 < Size) {
                                    // frequency
                                    intFreq = (short)(16 * (mData[Pos] & 0x3F) + (mData[Pos + 1] & 0xF));
                                    // attenuation
                                    bytAttn = ((byte)(mData[Pos + 2] & 0xF));
                                    mTrack[i].Notes.Add(intFreq, lngDur, bytAttn);
                                    lngTLength += lngDur;
                                }
                            }
                            else {
                                // zero length note
                                retval |= 2;
                            }
                        }
                        else {
                            // NOISE channel:
                            if (lngDur > 0) {
                                if (Pos + 2 < Size) {
                                    // get freq divisor (first two bits of second freq byte)
                                    // and noise type (3rd bit) as a single number
                                    intFreq = ((short)(mData[Pos + 1] & 7));
                                    // attenuation
                                    bytAttn = (byte)(mData[Pos + 2] & 0xF);
                                    mTrack[3].Notes.Add(intFreq, lngDur, bytAttn);
                                    lngTLength += lngDur;
                                }
                            }
                        }
                        Pos += 3;
                    }
                    // update total sound length
                    if (lngTLength > lngLength) {
                        lngLength = lngTLength;
                    }
                    // duration should be 0xffff
                    if (lngDur != 0xFFFF) {
                        retval |= 4;
                    }
                }
            }
            // calculate length in seconds (original playsound DOS app used sound
            // timing of 1/64 sec per tick but correct value is 1/60 sec)
            mLength = (double)lngLength / 60;
            mSoundChanged = false;
            return retval;
        }

        /// <summary>
        /// Compiles this sound by converting track notes into correctly formatted AGI
        /// resource data stream.
        /// </summary>
        private void CompileSound() {
            int i = 0, j;
            Sound tmpRes;
            tmpRes = new Sound();

            // placeholder for header
            tmpRes.Data = [];
            tmpRes.WriteWord(8, 0);
            for (j = 0; j <= 2; j++) {
                i += (mTrack[j].Notes.Count * 5) + 2;
                tmpRes.WriteWord((ushort)(8 + i));
            }
            try {
                // add TONE tracks
                for (j = 0; j <= 2; j++) {
                    for (i = 0; i < mTrack[j].Notes.Count; i++) {
                        // duration
                        tmpRes.WriteWord((ushort)mTrack[j].Notes[i].Duration);
                        // frequency data
                        tmpRes.WriteByte((byte)(mTrack[j].Notes[i].FreqDivisor / 16));
                        tmpRes.WriteByte((byte)((mTrack[j].Notes[i].FreqDivisor % 16) + 128 + 32 * j));
                        // attenuation
                        tmpRes.WriteByte((byte)(mTrack[j].Notes[i].Attenuation + 144 + 32 * j));
                    }
                    // end of track
                    tmpRes.WriteWord(0xFFFF);
                }
                // add NOISE track
                for (i = 0; i < mTrack[3].Notes.Count; i++) {
                    // duration
                    tmpRes.WriteWord((ushort)mTrack[3].Notes[i].Duration);
                    // null byte
                    tmpRes.WriteByte(0);
                    // type and frequency
                    tmpRes.WriteByte((byte)(224 + mTrack[3].Notes[i].FreqDivisor));
                    // attenuation
                    tmpRes.WriteByte((byte)(mTrack[3].Notes[i].Attenuation + 240));
                }
                // end of track
                tmpRes.WriteWord(0xFFFF);
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(566).Replace(ARG1, e.Message)) {
                    HResult = WINAGI_ERR + 566
                };
                wex.Data["exception"] = e;
                wex.Data["ID"] = mResID;
                throw wex;
            }
            mData = tmpRes.mData;
            mSoundChanged = false;
            ErrLevel = 0;
        }

        /// <summary>
        /// This mehod is called by child notes so build and changed statuses can be updated.
        /// </summary>
        internal void NoteChanged() {
            mOutputSet = false;
            mIsChanged = true;
            mLength = -1;
        }

        /// <summary>
        /// Clears the sound resource to a default PC/PCjr sound with no notes in any track.
        /// </summary>
        public override void Clear() {
            int i;
            WinAGIException.ThrowIfNotLoaded(this);
            base.Clear();
            mData = [ 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0xff, 0xff];
            // byte 0/1, 2/2, 4/5, 6/7 = offset to track data
            // byte 8/9 are end of track markers

            // clear all tracks
            for (i = 0; i <= 3; i++) {
                mTrack[i] = new Track(this) {
                    Instrument = 80
                };
                mTrack[0].Muted = false;
                mTrack[0].Visible = true;
            }
            mSoundChanged = true;
            mOutputSet = false;
            mLength = 0;
        }
        
        /// <summary>
        /// Plays this sound asynchronsously. IIgs sounds played only as PCM wav or MIDI.
        /// PC/PCJr sounds can be played as emulated PCM wav (default) or as a MIDI if
        /// playmode is non-zero.
        /// </summary>
        /// <param name="mode"></param>
        public void PlaySound(SoundPlaybackMode playmode = SoundPlaybackMode.PCSpeaker) {
            // TODO: need to add PC single channel mode playback option
            WinAGIException.ThrowIfNotLoaded(this);
            switch (mFormat) {
            case SoundFormat.AGI:
                // use passed mode

                break;
            case SoundFormat.WAV:
                // IIgs wav file - wav output only
                playmode =  SoundPlaybackMode.WAV;
                break;
            case SoundFormat.MIDI:
                // IIgs midi file - midi output only
                playmode = SoundPlaybackMode.MIDI;
                break;
            }
            // play a wav file
            if (!mOutputSet) {
                try {
                    BuildSoundOutput();
                }
                catch (Exception) {
                    // pass along exception
                    throw;
                }
            }
            switch (playmode) {
            case SoundPlaybackMode.PCSpeaker:
                throw (new NotImplementedException());
                //break;
            case SoundPlaybackMode.WAV:
                try {
                    wavPlayer.PlayWAVSound(this);
                }
                catch (Exception) {
                    throw;
                }
                break;
            case SoundPlaybackMode.MIDI:
                try {
                    midiPlayer.PlayMIDISound(this);
                }
                catch (Exception) {
                    throw;
                }
                break;
            }
        }

        /// <summary>
        /// Exports this resource to a standalone file in the specified format.
        /// </summary>
        /// <param name="exportfile"></param>
        /// <param name="format"></param>
        public void Export(string exportfile, SoundFormat format) {
            WinAGIException.ThrowIfNotLoaded(this);
            switch (format) {
            case SoundFormat.AGI:
                ExportAGISound(exportfile);
                break;
            case SoundFormat.MIDI:
                ExportAsMIDI(exportfile);
                break;
            case SoundFormat.WAV:
                ExportAsWAV(exportfile);
                break;
            case SoundFormat.Script:
                ExportAsScript(exportfile);
                break;
            case SoundFormat.Undefined:
                // error? or use default?
                break;
            }
        }

        /// <summary>
        /// Exports this resource to a standalone file as an AGI sound resource.
        /// </summary>
        /// <param name="exportfile"></param>
        public new void Export(string exportfile) {
            Export(exportfile, SoundFormat.AGI);
        }
        
        /// <summary>
        /// Exports the sound as a native AGI sound resource.
        /// </summary>
        /// <param name="filename"></param>
        private void ExportAGISound(string filename) {
            try {
                if (mSoundChanged) {
                    CompileSound();
                }
                base.Export(filename);
            }
            catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// Exports this sound as a MIDI file. Only applicable for standard PC/PCjr
        /// sound resources or Apple IIgs MIDI resources.
        /// </summary>
        /// <param name="MIDIFile"></param>
        private void ExportAsMIDI(string MIDIFile) {
            // only pcjr and IIgs midi can be exported as midi
            if (mFormat != SoundFormat.AGI && mFormat != SoundFormat.MIDI) {
                WinAGIException wex = new(LoadResString(705)) {
                    HResult = WINAGI_ERR + 705,
                };
                throw wex;
            }
            if (mSoundChanged) {
                try {
                    CompileSound();
                }
                catch (Exception) {
                    throw;
                }
            }
            try {
                if (!mOutputSet) {
                    BuildSoundOutput();
                }
                SafeFileDelete(MIDIFile);
                FileStream fsSnd = new(MIDIFile, FileMode.OpenOrCreate);
                fsSnd.Write(midiData);
                fsSnd.Dispose();
            }
            catch (Exception) {
                // pass along error
                throw;
            }
        }

        /// <summary>
        /// Exports this sound as a PCM WAV file. Only applicable for standard PC/PCjr
        /// sounds and Apple IIgs PCM sounds.
        /// </summary>
        /// <param name="WAVFile"></param>
        private void ExportAsWAV(string WAVFile) {
            // only pcjr and IIgs pcm can be exported as wav file
            int lngSize;

            if (mFormat != SoundFormat.AGI && mFormat != SoundFormat.WAV) {
                WinAGIException wex = new(LoadResString(705)) {
                    HResult = WINAGI_ERR + 705,
                };
                throw wex;
            }
            if (mSoundChanged) {
                try {
                    CompileSound();
                }
                catch (Exception) {
                    throw;
                }
            }
            try {
                if (!mOutputSet) {
                    BuildSoundOutput();
                }
                SafeFileDelete(WAVFile);
                if (mFormat == SoundFormat.AGI) {
                    // get size from the wav stream
                    lngSize = wavData.Length;
                }
                else {
                    // size of sound data is total file size, minus the PCM header 
                    lngSize = mData.Length - 54;
                }
                byte[] bOutput = new byte[lngSize];
                bOutput = new byte[44 + lngSize];
                // WAV generated from PCjr sounds are 44100 sample rate, 16bit, two channel.
                // IIgs WAV sounds are 8000 sample rate, 8bit, one channel.

                // required format for WAV data file:
                // Position       Value           Description
                //   0 - 3        "RIFF"          Marks the file as a riff (WAV) file.
                //   4 - 7        <varies>        Size of the overall file
                //   8 -11        "WAVE"          File Type Header. (should always equals "WAVE")
                //   12-15        "fmt "          Format chunk marker. Includes trailing space
                //   16-19        16              Length of format header data as listed above
                //   20-21        1               Type of format (1 is PCM)
                //   22-23        1    or 2       Number of Channels
                //   24-27        8000 or 44100   Sample Rate
                //   28-31        8000 or 176400  (Sample Rate * BitsPerSample * Channels) / 8
                //   32-33        1    or 4       (BitsPerSample * Channels) / 8 (1 - 8 bit mono)
                //   34-35        8    or 16      Bits per sample
                //   36-39        "data"          "data" chunk header. Marks the beginning of the data section.
                //   40-43        <varies>        Size of the data section.
                //   44+          data
                bOutput[0] = 82;
                bOutput[1] = 73;
                bOutput[2] = 70;
                bOutput[3] = 70;
                bOutput[4] = (byte)((lngSize + 36) & 0xFF);
                bOutput[5] = (byte)(((lngSize + 36) >> 8) & 0xFF);
                bOutput[6] = (byte)(((lngSize + 36) >> 16) & 0xFF);
                bOutput[7] = (byte)((lngSize + 36) >> 24);
                bOutput[8] = 87;
                bOutput[9] = 65;
                bOutput[10] = 86;
                bOutput[11] = 69;
                bOutput[12] = 102;
                bOutput[13] = 109;
                bOutput[14] = 116;
                bOutput[15] = 32;
                bOutput[16] = 16;
                bOutput[17] = 0;
                bOutput[18] = 0;
                bOutput[19] = 0;
                bOutput[20] = 1;
                bOutput[21] = 0;
                if (mFormat == SoundFormat.AGI) {
                    bOutput[22] = 2; // 00 02
                    bOutput[23] = 0;
                    bOutput[24] = 0x44; // 44100 = 00 00 AC 44
                    bOutput[25] = 0xAC;
                    bOutput[26] = 0;
                    bOutput[27] = 0;
                    bOutput[28] = 0x10; // 176400 = 00 02 B1 10
                    bOutput[29] = 0xB1;
                    bOutput[30] = 2;
                    bOutput[31] = 0;
                    bOutput[32] = 4; // 00 04
                    bOutput[33] = 0;
                    bOutput[34] = 0x10; // 00 10
                    bOutput[35] = 0;
                }
                else {
                    bOutput[22] = 1;
                    bOutput[23] = 0;
                    bOutput[24] = 64;
                    bOutput[25] = 31;
                    bOutput[26] = 0;
                    bOutput[27] = 0;
                    bOutput[28] = 64;
                    bOutput[29] = 31;
                    bOutput[30] = 0;
                    bOutput[31] = 0;
                    bOutput[32] = 1;
                    bOutput[33] = 0;
                    bOutput[34] = 8;
                    bOutput[35] = 0;
                }
                bOutput[36] = 100;
                bOutput[37] = 97;
                bOutput[38] = 116;
                bOutput[39] = 97;
                // ????????????why subtract 2???????????????
                bOutput[40] = (byte)((lngSize - 2) & 0xFF);
                bOutput[41] = (byte)(((lngSize - 2) >> 8) & 0xFF);
                bOutput[42] = (byte)(((lngSize - 2) >> 16) & 0xFF);
                bOutput[43] = (byte)((lngSize - 2) >> 24);
                // add data
                int pos = 44;
                if (mFormat == SoundFormat.AGI) {
                    // copy data from wav stream
                    for (int i = 0; i < wavData.Length; i++) {
                        bOutput[pos++] = wavData[i];
                    }
                }
                else {
                    // copy data from sound resource
                    for (int i = 54; i < mData.Length; i++) {
                        bOutput[pos++] = mData[i];
                    }
                }
                FileStream fsSnd = new(WAVFile, FileMode.OpenOrCreate);
                fsSnd.Write(bOutput);
                fsSnd.Dispose();
            }
            catch (Exception) {
                // pass along any errors
                throw;
            }
        }

        /// <summary>
        /// Exports this sound as a script file in the format developed by Nick Sonneveld.
        /// Only applicable for PC/PCjr sounds.
        /// </summary>
        /// <param name="ExportFile"></param>
        private void ExportAsScript(string ExportFile) {
            int i, j;

            // scripts don't need a compiled sound resource

            // only agi format can be exported as script
            if (mFormat != SoundFormat.AGI) {
                WinAGIException wex = new(LoadResString(705)) {
                    HResult = WINAGI_ERR + 705,
                };
                throw wex;
            }
            try {
                SafeFileDelete(ExportFile);
                // creat script file
                FileStream fsSnd = new(ExportFile, FileMode.OpenOrCreate);
                StreamWriter swSnd = new(fsSnd);
                // add comment header
                swSnd.WriteLine("# agi script file");
                swSnd.WriteLine("");
                swSnd.WriteLine("##Description=" + mDescription);
                swSnd.WriteLine("##TPQN=" + mTPQN);
                swSnd.WriteLine("");
                // add sound tracks
                for (i = 0; i <= 2; i++) {
                    swSnd.WriteLine("# track " + i);
                    swSnd.WriteLine("tone");
                    for (j = 0; j < mTrack[i].Notes.Count; j++) {
                        if (j == 0) {
                            swSnd.WriteLine("a, " + mTrack[i].Notes[0].FreqDivisor + ", " + mTrack[i].Notes[0].Attenuation + ", " + mTrack[i].Notes[0].Duration);
                        }
                        else {
                            swSnd.WriteLine(mTrack[i].Notes[j].FreqDivisor + ", " + mTrack[i].Notes[j].Attenuation + ", " + mTrack[i].Notes[j].Duration);
                        }
                    }
                    swSnd.WriteLine("##instrument" + i + "=" + mTrack[i].Instrument);
                    swSnd.WriteLine("##visible" + i + "=" + mTrack[i].Visible);
                    swSnd.WriteLine("##muted" + i + "=" + mTrack[i].Muted);
                    swSnd.WriteLine("");
                }
                // add noise track
                swSnd.WriteLine("# track 3");
                swSnd.WriteLine("noise");
                for (j = 0; j < mTrack[3].Notes.Count; j++) {
                    if ((mTrack[3].Notes[j].FreqDivisor & 4) == 4) {
                        // white noise
                        swSnd.WriteLine("w," + (mTrack[3].Notes[j].FreqDivisor & 3) + ", " + mTrack[3].Notes[j].Attenuation + ", " + mTrack[3].Notes[j].Duration);
                    }
                    else {
                        swSnd.WriteLine("p," + (mTrack[3].Notes[j].FreqDivisor & 3) + ", " + mTrack[3].Notes[j].Attenuation + ", " + mTrack[3].Notes[j].Duration);
                    }
                }
                // add visible and muted properties
                swSnd.WriteLine("##visible3=" + mTrack[3].Visible);
                swSnd.WriteLine("##muted3=" + mTrack[3].Muted);
                swSnd.Dispose();
                fsSnd.Dispose();
            }
            catch (Exception) {
                // pass along any errors
                throw;
            }
        }

        /// <summary>
        /// Imports a sound resource from a file into this sound.
        /// </summary>
        /// <param name="ImportFile"></param>
        public override void Import(string ImportFile) {
            try {
                base.Import(ImportFile);
            }
            catch {
                // pass along any errors
                throw;
            }
            // set defaults
            mTPQN = 16;
            mKey = 0;
            mTrack[0].Instrument = 80;
            mTrack[1].Instrument = 80;
            mTrack[2].Instrument = 80;
            mTrack[0].Muted = false;
            mTrack[1].Muted = false;
            mTrack[2].Muted = false;
            mTrack[3].Muted = false;
            mTrack[0].Visible = true;
            mTrack[1].Visible = true;
            mTrack[0].Visible = true;
            mTrack[0].Visible = true;
            // set tracks and wav/midi data
            FinishLoad();
        }

        /// <summary>
        /// Imports a sound resource from a script file into this sound.
        /// </summary>
        /// <param name="scriptfile"></param>
        public void ImportScript(string scriptfile) {
            string strLine;
            string[] strLines, strTag;
            int i, lngTrack, lngDur;
            int lngNoteType = -1;
            //         -1 => undefined
            //   tone:  0 => agi freq index
            //          1 => raw freq
            //   noise: 0 => periodic tone
            //          4 => white noise
            bool blnError = false;
            short intFreq;
            byte bytVol, temp;
            bool bVal;

            if (scriptfile is null || scriptfile.Length == 0) {
                WinAGIException wex = new(LoadResString(604)) {
                    HResult = WINAGI_ERR + 604,
                };
                throw wex;
            }
            if (!File.Exists(scriptfile)) {
                WinAGIException wex = new(LoadResString(524).Replace(ARG1, scriptfile)) {
                    HResult = WINAGI_ERR + 524,
                };
                wex.Data["missingfile"] = scriptfile;
                throw wex;
            }
            // clear the resource before importing
            Clear();
            // default to no tracks
            lngTrack = -1;
            try {
                using FileStream fsSnd = new(scriptfile, FileMode.Open);
                using StreamReader srSnd = new(fsSnd);
                strLine = srSnd.ReadToEnd();
                fsSnd.Dispose();
                srSnd.Dispose();
            }
            catch (Exception) {
                // pass along any errors
                throw;
            }
            // standardize end of line markers
            strLine = strLine.Replace("\r\n", "\r");
            strLine = strLine.Replace("\n", "\r");
            strLines = strLine.Split("\r");
            for (i = 0; i < strLines.Length; i++) {
                strLine = strLines[i].Trim();
                if (strLine.Length == 0) {
                    continue;
                }
                if (strLine[0] == '#') {
                    // check for winagi tags
                    if (strLine.Left(2) == "##") {
                        // split the line into tag and Value
                        strTag = strLine.Split("=");
                        if (strTag.Length == 2) {
                            // tag
                            switch (strTag[0].Trim().ToLower()) {
                            case "##description":
                                mDescription = strTag[1];
                                break;
                            case "##instrument0":
                                if (byte.TryParse(strTag[1], out temp)) {
                                    mTrack[0].Instrument = temp;
                                }
                                break;
                            case "##instrument1":
                                if (byte.TryParse(strTag[1], out temp)) {
                                    mTrack[1].Instrument = temp;
                                }
                                break;
                            case "##instrument2":
                                if (byte.TryParse(strTag[1], out temp)) {
                                    mTrack[2].Instrument = temp;
                                }
                                break;
                            case "##visible0":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[0].Visible = bVal;
                                }
                                break;
                            case "##visible1":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[1].Visible = bVal;
                                }
                                break;
                            case "##visible2":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[2].Visible = bVal;
                                }
                                break;
                            case "##visible3":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[3].Visible = bVal;
                                }
                                break;
                            case "##muted0":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[0].Muted = bVal;
                                }
                                break;
                            case "##muted1":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[1].Muted = bVal;
                                }
                                break;
                            case "##muted2":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[2].Muted = bVal;
                                }
                                break;
                            case "##muted3":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[3].Muted = bVal;
                                }
                                break;
                            case "##tpqn":
                                mTPQN = (int)strTag[1].Val() / 4 * 4;
                                if (mTPQN < 4) {
                                    mTPQN = 4;
                                }
                                else if (mTPQN > 64) {
                                    mTPQN = 64;
                                }
                                break;
                            case "##key":
                                mKey = (int)strTag[1].Val();
                                if (mKey < -7) {
                                    mKey = -7;
                                }
                                else if (mKey > 7) {
                                    mKey = 7;
                                }
                                break;
                            }
                        }
                    }
                    else {
                        // any other use of '#' is a comment to be ignored
                        continue;
                    }
                }
                else if (strLine.Equals("tone", StringComparison.OrdinalIgnoreCase)) {
                    lngTrack++;
                    // reset default note type
                    lngNoteType = -1;
                }
                else if (strLine.Equals("noise", StringComparison.OrdinalIgnoreCase) && lngTrack != 3) {
                    lngTrack = 3;
                    // reset default note type
                    lngNoteType = -1;
                }
                else {
                    if (lngTrack < 0 || lngTrack > 3) {
                        blnError = true;
                        break;
                    }
                    strTag = strLine.Split(",");
                    // should only be three or four elements
                    if (strTag.Length >= 4) {
                        // check first element for new note type, depending on track
                        if (lngTrack == 3) {
                            // 'p' or 'w' only
                            if (strTag[0].Equals("p", StringComparison.OrdinalIgnoreCase)) {
                                lngNoteType = 0;
                            }
                            else if (strTag[0].Equals("w", StringComparison.OrdinalIgnoreCase)) {
                                lngNoteType = 4;
                            }
                            else {
                                blnError = true;
                                break;
                            }
                            intFreq = (short)((uint)strTag[1].Val() | (uint)lngNoteType);
                        }
                        else {
                            // for music tracks, 'a' or 'f' only
                            if (strTag[0] == "a") {
                                lngNoteType = 0;
                                // agi freq index is the Value passed
                                intFreq = (short)strTag[1].Val();
                            }
                            else if (strTag[0] == "f") {
                                lngNoteType = 1;
                                // a real freq Value was passed
                                intFreq = (short)strTag[1].Val();
                                // can't be zero
                                if (intFreq == 0) {
                                    blnError = true;
                                    break;
                                }
                                intFreq = (short)((double)intFreq / 111860);
                            }
                            else {
                                blnError = true;
                                break;
                            }
                        }
                        // get volume and duration
                        bytVol = (byte)strTag[2].Val();
                        lngDur = (int)strTag[3].Val();
                    }
                    else if (strTag.Length == 3) {
                        // note type must be set
                        if (lngNoteType == -1) {
                            blnError = true;
                            break;
                        }
                        if (lngTrack != 3) {
                            if (lngNoteType == 0) {
                                intFreq = (short)strTag[0].Val();
                            }
                            else {
                                intFreq = (short)strTag[0].Val();
                                // can't be zero
                                if (intFreq == 0) {
                                    blnError = true;
                                    break;
                                }
                                intFreq = (short)((double)intFreq / 111860);
                            }
                        }
                        else {

                            // track 3: use previous note type
                            intFreq = (short)((uint)strTag[0].Val() | (uint)lngNoteType);
                        }
                        // get volume and duration
                        bytVol = (byte)strTag[1].Val();
                        lngDur = (int)strTag[2].Val();
                    }
                    else {
                        blnError = true;
                        break;
                    }
                    // validate input
                    if (intFreq < 0 || intFreq >= 1024 || bytVol < 0 || bytVol >= 16 || lngDur < 0 || lngDur > 65535) {
                        blnError = true;
                        break;
                    }
                    // duration of zero is not an error, but it is ignored
                    if (lngDur != 0) {
                        // add the note to current track
                        mTrack[lngTrack].Notes.Add(intFreq, lngDur, bytVol);
                    }
                }


                if (blnError) {
                    Clear();
                    mResID = "";
                    mDescription = "";
                    WinAGIException wex = new(LoadResString(681)) {
                        HResult = WINAGI_ERR + 681
                    };
                    throw wex;
                }
            }
            // compile the sound so the resource matches the tracks
            try {
                CompileSound();
            }
            catch {
                // pass along any errors
                throw;
            }
            // set ID to the filename without extension
            string tmpID = Path.GetFileNameWithoutExtension(scriptfile);
            if (tmpID.Length > 64) {
                tmpID = tmpID[..64];
            }
            if (mInGame) {
                if (NotUniqueID(tmpID, parent)) {
                    i = 0;
                    string baseid = mResID;
                    do {
                        mResID = baseid + "_" + i++.ToString();
                    }
                    while (NotUniqueID(this));
                }
            }
            mResID = tmpID;
            mFormat = SoundFormat.AGI;
            ErrLevel = 0;

            // reset changed flags
            mIsChanged = false;
            PropsChanged = false;
            // set tracks and wav/midi data
            try {
                // standard pc/pcjr is playable as both WAV and MIDI
                midiData = BuildMIDI(this);
                mLength = GetSoundLength();
                wavData = wavPlayer.BuildPCjrWAV(this);
                mOutputSet = true;
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(596)) {
                    HResult = WINAGI_ERR + 596
                };
                wex.Data["exception"] = e;
                wex.Data["ID"] = mResID;
                throw wex;
            }
        }

        /// <summary>
        /// Called by sound tracks to indicate a change has occured. ResetMIDI
        /// flag allows some track changes to occur that don't affect the MIDI
        /// data (such as Visible) but still set the writeprops flag.
        /// </summary>
        /// <param name="ResetOutput"></param>
        internal void TrackChanged(bool ResetOutput = true) {
            // when track status changes, need to recalculate length
            mLength = -1;
            if (ResetOutput) {
                // output needs update
                mOutputSet = false;
            }
            PropsChanged = true;
        }

        /// <summary>
        /// Loads this sound resource by reading its data from the VOL file. Only
        /// applies to sounds in a game. Non-game sounds are always loaded.
        /// </summary>
        public override void Load() {
            if (mLoaded) {
                return;
            }
            // load base resource
            base.Load();
            if (ErrLevel < 0) {
                ErrClear();
                // clear the sound to empty set of tracks
                for (int i = 0; i <= 3; i++) {
                    mTrack[i] = new Track(this) {
                        Instrument = 80
                    };
                    mTrack[0].Muted = false;
                    mTrack[0].Visible = true;
                }
                mOutputSet = false;
                mLength = 0;
            }
            else {
                // finish loading sound
                FinishLoad();
                // get settings
                mKey = parent.agGameProps.GetSetting("Sound" + mResNum, "Key", 0);
                if (mKey < -7 || mKey > 7) {
                    mKey = 0;
                }
                mTPQN = parent.agGameProps.GetSetting("Sound" + mResNum, "TPQN", 0);
                mTPQN = (mTPQN / 4) * 4;
                if (mTPQN < 4) {
                    mTPQN = 4;
                }
                if (mTPQN > 64) {
                    mTPQN = 64;
                }
                mTrack[0].Instrument = parent.agGameProps.GetSetting("Sound" + mResNum, "Inst0", (byte)80);
                mTrack[1].Instrument = parent.agGameProps.GetSetting("Sound" + mResNum, "Inst1", (byte)80);
                mTrack[2].Instrument = parent.agGameProps.GetSetting("Sound" + mResNum, "Inst2", (byte)80);
                mTrack[0].Muted = parent.agGameProps.GetSetting("Sound" + mResNum, "Mute0", false);
                mTrack[1].Muted = parent.agGameProps.GetSetting("Sound" + mResNum, "Mute1", false);
                mTrack[2].Muted = parent.agGameProps.GetSetting("Sound" + mResNum, "Mute2", false);
                mTrack[3].Muted = parent.agGameProps.GetSetting("Sound" + mResNum, "Mute3", false);
                mTrack[0].Visible = parent.agGameProps.GetSetting("Sound" + mResNum, "Visible0", true);
                mTrack[1].Visible = parent.agGameProps.GetSetting("Sound" + mResNum, "Visible1", true);
                mTrack[2].Visible = parent.agGameProps.GetSetting("Sound" + mResNum, "Visible2", true);
                mTrack[3].Visible = parent.agGameProps.GetSetting("Sound" + mResNum, "Visible3", true);
            }
        }

        /// <summary>
        /// This method sets the sound format and loads tracks and output data.
        /// </summary>
        private void FinishLoad() {
            int i;

            // initialize tracks
            for (i = 0; i <= 3; i++) {
                //clear out tracks by assigning to nothing, then new
                mTrack[i] = new Track(this);
            }
            // check header to determine what type of sound resource;
            //    0x01 = IIgs sampled sound
            //    0x02 = IIgs midi sound
            //    0x08 = PC/PCjr 
            switch (ReadWord(0)) {
            case 1:
                mFormat = SoundFormat.WAV;
                mSoundChanged = false;
                break;
            case 2:
                mFormat = SoundFormat.MIDI;
                mSoundChanged = false;
                break;
            case 8:
                mFormat = SoundFormat.AGI;
                ErrLevel = LoadTracks();
                break;
            default:
                // bad sound
                ErrLevel = 16;
                ErrClear();
                break;
            }
            try {
                BuildSoundOutput();
            }
            catch (Exception) {
                // pass along errors
                throw;
            }
        }

        /// <summary>
        /// Saves properties of this sound to the game's WAG file.
        /// </summary>
        public void SaveProps() {
            if (mInGame) {
                string strSection = "Sound" + mResNum;
                parent.WriteGameSetting(strSection, "ID", mResID, "Sounds");
                parent.WriteGameSetting(strSection, "Description", mDescription);
                parent.WriteGameSetting(strSection, "Key", mKey, "Sounds");
                parent.WriteGameSetting(strSection, "TPQN", mTPQN);
                parent.WriteGameSetting(strSection, "Inst0", mTrack[0].Instrument);
                parent.WriteGameSetting(strSection, "Inst1", mTrack[1].Instrument);
                parent.WriteGameSetting(strSection, "Inst2", mTrack[2].Instrument);
                parent.WriteGameSetting(strSection, "Mute0", mTrack[0].Muted);
                parent.WriteGameSetting(strSection, "Mute1", mTrack[1].Muted);
                parent.WriteGameSetting(strSection, "Mute2", mTrack[2].Muted);
                parent.WriteGameSetting(strSection, "Mute3", mTrack[3].Muted);
                parent.WriteGameSetting(strSection, "Visible0", mTrack[0].Visible);
                parent.WriteGameSetting(strSection, "Visible1", mTrack[1].Visible);
                parent.WriteGameSetting(strSection, "Visible2", mTrack[2].Visible);
                parent.WriteGameSetting(strSection, "Visible3", mTrack[3].Visible);
                PropsChanged = false;
            }
        }

        /// <summary>
        /// Saves this sound resource. If in a game, it updates the DIR and VOL files. 
        /// If not in a game the sound is saved to its resource file specified by FileName.
        /// </summary>
        public new void Save() {
            WinAGIException.ThrowIfNotLoaded(this);
            if (PropsChanged && mInGame) {
                SaveProps();
            }
            if (mSoundChanged) {
                try {
                    CompileSound();
                }
                catch (Exception) {
                    throw;
                }
            }
            if (mIsChanged) {
                try {
                    base.Save();
                }
                catch (Exception) {
                    throw;
                }
            }
        }

        /// <summary>
        /// Forces sound tracks to rebuild and output to reload. Use when the calling
        /// program needs the sound to be refreshed.
        /// </summary>
        public void ResetSound() {
            mOutputSet = false;
            FinishLoad();
        }

        /// <summary>
        /// Stops the sound, if it is currently playing. Note that calling this for ANY
        /// sound will stop ALL sound.
        /// </summary>
        public void StopSound() {
            if (!mLoaded) {
                return;
            }
            // stop all modes
            StopAllSound();
        }

        /// <summary>
        /// Unloads this sound resource. Data elements are undefined and non-accessible
        /// while unloaded. Only sounds that are in a game can be unloaded.
        /// </summary>
        public override void Unload() {
            // only ingame resources can be unloaded
            if (!mInGame) {
                return;
            }
            base.Unload();
            mIsChanged = false;
            // clear midi and wav data
            midiData = [];
            wavData = [];
            mOutputSet = false;
            // reset length
            mLength = -1;
            // clear tracks
            for (int i = 0; i < 4; i++) { 
                mTrack[i] = new Track(this);
            }
            mSoundChanged = false;
        }
        #endregion
    }
}
