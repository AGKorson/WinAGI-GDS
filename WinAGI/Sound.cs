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
            //  1 = PCjr agi
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
                    WinAGIException wex = new(LoadResString(542)) {
                        HResult = WINAGI_ERR + 542,
                    };
                    throw wex;
                }
                if (!mOutputSet) {
                    try {
                        BuildSoundOutput();
                    }
                    catch {
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
                    WinAGIException wex = new(LoadResString(542)) {
                        HResult = WINAGI_ERR + 542,
                    };
                    throw wex;
                }
                if (!mOutputSet) {
                    try {
                        BuildSoundOutput();
                    }
                    catch {
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
                mData = [0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0xff, 0xff];
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
            for (int i = 0; i < 4; i++) {
                clonesound.mTrack[i] = mTrack[i].Clone(clonesound);
            }
            return clonesound;
        }

        /// <summary>
        /// Replaces the contents of this Sound resource with the contents
        /// of another Sound resource.
        /// </summary>
        /// <param name="SourceSound"></param>
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
        private void BuildSoundOutput() {
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
            catch {
                throw;
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
        /// <returns>Error and warning status:<br />
        /// Errors:<br />
        /// -9 = no data<br />
        /// -10 = no data<br />
        /// 1 = invalid track offset<br />
        /// 2 = zero length note<br />
        /// 4 = missing track end marker<br />
        /// 8 = no sound data
        /// </returns>
        internal (ResourceErrorType, int) LoadTracks() {
            int i, lngLength = 0, lngTLength;
            int lngStart, lngDur = 0;
            short intFreq;
            byte bytAttn;

            // assume no errors or warnings
            ResourceErrorType errval = ResourceErrorType.NoError;
            int warnval = 0;

            // minimum size for a sound resource is 10 bytes:
            // 8 bytes for track offsets + 2 bytes for end of track marker
            if (Size < 10) {
                // not enough data to be a sound
                return (ResourceErrorType.SoundNoData, 0);
            }
            // extract note information for each track from resource
            for (i = 0; i <= 3; i++) {
                lngStart = ReadWord(i * 2);
                lngTLength = 0;
                // validate track data location
                if (lngStart > mSize - 1) {
                    // invalid track offset
                    errval = ResourceErrorType.SoundBadTracks;
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
                                warnval |= 1;
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
                                else {
                                    // zero length note
                                    warnval |= 1;
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
                        warnval |= 2;
                    }
                }
            }
            // calculate length in seconds (original playsound DOS app used sound
            // timing of 1/64 sec per tick but correct value is 1/60 sec)
            mLength = (double)lngLength / 60;
            mSoundChanged = false;
            return (errval, warnval);
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
                WinAGIException wex = new(LoadResString(511).Replace(ARG1, e.Message)) {
                    HResult = WINAGI_ERR + 511
                };
                wex.Data["exception"] = e;
                wex.Data["ID"] = mResID;
                throw wex;
            }
            mData = tmpRes.mData;
            mSoundChanged = false;
            Error = ResourceErrorType.NoError;
            ErrData = ["", "", "", "", "", ""];
            Warnings = 0;
            WarnData = ["", "", "", "", "", ""];
        }

        /// <summary>
        /// This mehod is called by child notes so build and changed statuses can be updated.
        /// </summary>
        internal void NoteChanged() {
            mOutputSet = false;
            mSoundChanged = true;
            mLength = -1;
        }

        /// <summary>
        /// Clears the sound resource to a default PC/PCjr sound with no notes in any track.
        /// </summary>
        public override void Clear() {
            int i;
            WinAGIException.ThrowIfNotLoaded(this);
            base.Clear();
            mData = [0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0xff, 0xff];
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
                catch {
                    // pass along exception
                    throw;
                }
            }
            switch (playmode) {
            case SoundPlaybackMode.PCSpeaker:
                throw (new NotImplementedException());
            case SoundPlaybackMode.WAV:
                try {
                    wavPlayer.PlayWAVSound(this);
                }
                catch {
                    throw;
                }
                break;
            case SoundPlaybackMode.MIDI:
                try {
                    midiPlayer.PlayMIDISound(this);
                }
                catch {
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
            catch {
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
                WinAGIException wex = new(LoadResString(542)) {
                    HResult = WINAGI_ERR + 542,
                };
                throw wex;
            }
            if (mSoundChanged) {
                try {
                    CompileSound();
                }
                catch {
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
            catch {
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
                WinAGIException wex = new(LoadResString(542)) {
                    HResult = WINAGI_ERR + 542,
                };
                throw wex;
            }
            if (mSoundChanged) {
                try {
                    CompileSound();
                }
                catch {
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
            catch {
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
                WinAGIException wex = new(LoadResString(542)) {
                    HResult = WINAGI_ERR + 542,
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
            catch {
                // pass along any errors
                throw;
            }
        }

        /// <summary>
        /// Imports a sound resource from a file into this sound.
        /// </summary>
        /// <param name="ImportFile"></param>
        public void Import(string ImportFile, SoundImportFormat format = SoundImportFormat.AGI, SoundImportOptions options = null) {
            ArgumentException.ThrowIfNullOrWhiteSpace(ImportFile, nameof(ImportFile));
            if (!File.Exists(ImportFile)) {
                throw new FileNotFoundException(ImportFile);
            }
            switch (format) {
            case SoundImportFormat.AGI:
                // import as AGI sound
                base.Import(ImportFile);
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
                break;
            case SoundImportFormat.MIDI:
                try {
                    SoundImport.MIDI2AGI(ImportFile, this, options);
                }
                catch (Exception e) {
                    // reset to a default sound
                    Error = ResourceErrorType.SoundCantConvert;
                    ErrData[0] = format.ToString();
                    ErrData[1] = e.Message;
                    ErrClear();
                    ClearTracks();
                }
                // rebuild sound data
                CompileSound();
                break;
            case SoundImportFormat.MOD:
                try {
                    SoundImport.MOD2AGI(ImportFile, this, options);
                }
                catch (Exception e) {
                    // reset to a default sound
                    Error = ResourceErrorType.SoundCantConvert;
                    ErrData[0] = format.ToString();
                    ErrData[1] = e.Message;
                    ErrClear();
                    ClearTracks();
                }
                CompileSound();
                break;
            case SoundImportFormat.IT:
                try {
                    SoundImport.IT2AGI(ImportFile, this, options);
                }
                catch (Exception e) {
                    // reset to a default
                    Error = ResourceErrorType.SoundCantConvert;
                    ErrData[0] = format.ToString();
                    ErrData[1] = e.Message;
                    ErrClear();
                    ClearTracks();
                }
                CompileSound();
                break;
            case SoundImportFormat.Script:
                try {
                    SoundImport.Script2AGI(ImportFile, this);
                }
                catch (Exception e) {
                    // reset to a default
                    Error = ResourceErrorType.SoundCantConvert;
                    ErrData[0] = format.ToString();
                    ErrData[1] = e.Message;
                    ErrClear();
                    ClearTracks();
                }
                CompileSound();
                break;
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
            Load(false);
        }

        internal void Load(bool validateonly) {
            if (mLoaded) {
                return;
            }
            // load base resource
            base.Load();
            if (Error != ResourceErrorType.NoError &&
                Error != ResourceErrorType.FileIsReadonly) {
                // errors other than readonly can't be extracted
                ErrClear();
                ClearTracks();
            }
            else {
                // finish loading sound
                FinishLoad(validateonly);
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

        private void ClearTracks() {
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

        /// <summary>
        /// This method sets the sound format and loads tracks and output data.
        /// </summary>
        private void FinishLoad(bool no_output = false) {
            int i;

            // initialize tracks
            for (i = 0; i <= 3; i++) {
                // clear out tracks by assigning new
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
                (Error, Warnings) = LoadTracks();
                break;
            default:
                // bad sound
                Error = ResourceErrorType.SoundBadTracks;
                Warnings = 0;
                ErrClear();
                break;
            }
            if (!no_output) {
                try {
                    BuildSoundOutput();
                }
                catch {
                    // pass along errors
                    throw;
                }
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
                catch {
                    throw;
                }
            }
            if (mIsChanged) {
                try {
                    base.Save();
                }
                catch {
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
