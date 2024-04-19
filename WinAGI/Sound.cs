using System;
using System.Diagnostics;
using System.IO;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents an AGI Sound resource, with WinAGI extensions.
    /// </summary>
    public class Sound : AGIResource {
        Track[] mTrack = new Track[4];
        bool mTracksSet;
        double mLength;
        int mKey;
        int mTPQN;
        SoundFormat mFormat;
        byte[] midiData = [];
        byte[] wavData = [];
        bool mOutputSet;

        // Sound resources include an event to notify calling programs when playback
        // of a sound is complete.

        // declare the event delegate, and event
        public delegate void SoundCompleteEventHandler(object sender, SoundCompleteEventArgs e);
        public event SoundCompleteEventHandler SoundComplete;
        public class SoundCompleteEventArgs {
            public SoundCompleteEventArgs(bool noerror) {
                NoError = noerror;
            }
            public bool NoError { get; }
        }

        internal void Raise_SoundCompleteEvent(bool noerror) {
            // Raise the event in a thread-safe manner using the ?. operator.
            SoundComplete?.Invoke(null, new SoundCompleteEventArgs(noerror));
        }

        /// <summary>
        /// Initializes a new AGI sound resource that is not in a game.
        /// </summary>
        public Sound() : base(AGIResType.rtSound) {
            // new sound, not in game

            //initialize
            InitSound();
            // create a default ID
            mResID = "NewSound";
            // if not in a game, resource is always loaded
            mLoaded = true;
        }

        /// <summary>
        /// Internal constructor to initialize a new or cloned sound resource being added to an AGI game.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="NewSound"></param>
        internal Sound(AGIGame parent, byte ResNum, Sound NewSound = null) : base(AGIResType.rtSound) {
            // initialize
            InitSound(NewSound);
            //set up base resource
            base.InitInGame(parent, ResNum);
        }

        /// <summary>
        /// Internal constructor to add a new sound resource during initial game load.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="VOL"></param>
        /// <param name="Loc"></param>
        internal Sound(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.rtSound) {
            // adds this resource to a game, setting its resource 
            // location properties, and reads properties from the wag file

            // attach events
            base.PropertyChanged += ResPropChange;
            // set up base resource
            base.InitInGame(parent, AGIResType.rtSound, ResNum, VOL, Loc);
            //length is undefined until sound is built
            mLength = -1;
        }

        /// <summary>
        /// Initializes a new sound resource when first instantiated. If NewSound is null, 
        /// a blank sound resource is created. If NewSound is not null, it is cloned into
        /// the new sound.
        /// </summary>
        /// <param name="NewSound"></param>
        private void InitSound(Sound NewSound = null) {
            // attach events
            base.PropertyChanged += ResPropChange;
            if (NewSound is null) {
                // create default PC/PCjr sound with no notes in any tracks
                mRData.AllData = [ 0x08, 0x00, 0x08, 0x00,
                                    0x08, 0x00, 0x08, 0x00,
                                    0xff, 0xff];
                // byte 0/1, 2/2, 4/5, 6/7 = offset to track data
                // byte 8/9 are end of track markers
                mFormat = SoundFormat.sfAGI;
                mTrack[0] = new Track(this);
                mTrack[1] = new Track(this);
                mTrack[2] = new Track(this);
                mTrack[3] = new Track(this);
                mTrack[0].Instrument = 80;
                mTrack[1].Instrument = 80;
                mTrack[2].Instrument = 80;
                // default tqpn is 16
                mTPQN = 16;
                // default key is c
                mKey = 0;
            }
            else {
                // clone the new sound
                NewSound.Clone(this);
            }
        }

        /// <summary>
        /// Event handler for changes in base resource data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResPropChange(object sender, AGIResPropChangedEventArgs e) {
            // sound data has changed- tracks no longer match
            mTracksSet = false;
            mIsDirty = true;
        }

        /// <summary>
        /// This method creates midi/wav output data stream for a sound resource.
        /// </summary>
        void BuildSoundOutput() {
            try {
                switch (mFormat) {
                case SoundFormat.sfAGI:
                    // standard pc/pcjr is playable as both WAV and MIDI
                    midiData = BuildMIDI(this);
                    mLength = GetSoundLength();
                    wavData = wavPlayer.BuildPCjrWAV(this);
                    break;
                case SoundFormat.sfWAV:
                    // IIgs pcm sound
                    wavData = BuildIIgsPCM(this);
                    mLength = GetSoundLength();

                    break;
                case SoundFormat.sfMIDI:
                    // IIgs MIDI sound
                    // build the midi data and get length info
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
        double GetSoundLength() {
            // this function assumes a sound has been loaded properly
            int i;
            double retval = 0;

            switch (mFormat) {
            case SoundFormat.sfAGI:
                // standard pc/pcjr resource
                for (i = 0; i <= 3; i++) {
                    if (retval < mTrack[i].Length && !mTrack[i].Muted) {
                        retval = mTrack[i].Length;
                    }
                }
                break;
            case SoundFormat.sfWAV:
                // IIgs pcm sampling
                // since sampling is at 8000Hz, length is just data length/8000
                retval = ((double)mSize - 54) / 8000;
                break;
            case SoundFormat.sfMIDI:
                // IIgs midi
                // length has to be calculated during midi build
                BuildSoundOutput();
                retval = mLength;
                break;
            }
            return retval;
        }

        /// <summary>
        /// Gets the track for this sound corresponding to index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Track this[int index] {
            get {
                if (index < 0 || index > 3) {
                    throw new IndexOutOfRangeException("invalid track number");
                }
                return Track(index);
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
                    PropDirty = true;
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
                    return SoundFormat.sfUndefined;
                }
            }
        }

        /// <summary>
        /// Extracts sound data from the resource and builds the sound tracks.
        /// </summary>
        internal void LoadTracks() {
            int i, lngLength = 0, lngTLength;
            int lngStart, lngEnd, lngResPos, lngDur;
            short intFreq;
            byte bytAttn;

            if (mInGame) {
                // get track properties from the WAG file
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
            else {
                // default visible status is true
                mTrack[0].Visible = true;
                mTrack[1].Visible = true;
                mTrack[0].Visible = true;
                mTrack[0].Visible = true;
            }
            try {
                // extract note information for each track from resource
                for (i = 0; i <= 3; i++) {
                    lngTLength = 0;
                    // get start and end of this track (stored at beginning of
                    // resource in LSMS format)
                    //   track 0 start is byte 0-1, track 1 start is byte 2-3
                    //   track 2 start is byte 4-5, noise start is byte 6-7
                    lngStart = mRData[i * 2 + 0] + 256 * mRData[i * 2 + 1];
                    if (i < 3) {
                        // end (last note) is start of next track -7 (5 bytes per
                        // note in each track, -2 for end of track marker [0xFFFF])
                        lngEnd = mRData[i * 2 + 2] + 256 * mRData[i * 2 + 3] - 7;
                    }
                    else {
                        // last track end
                        lngEnd = mSize - 7;
                    }
                    // validate track data location
                    if (lngStart < 0 || lngEnd < 0 || lngStart > mSize || lngEnd > mSize) {
                        mErrLevel = -13;
                        ErrData[0] = mResID;
                        break;
                    }
                    // step through notes in this track (5 bytes at a time)
                    for (lngResPos = lngStart; lngResPos <= lngEnd; lngResPos += 5) {
                        if (i < 3) {
                            // TONE channel:
                            // duration
                            lngDur = (mRData[lngResPos] + 256 * mRData[lngResPos + 1]);
                            // frequency
                            intFreq = (short)(16 * (mRData[lngResPos + 2] & 0x3F) + (mRData[lngResPos + 3] & 0xF));
                            // attenuation
                            bytAttn = ((byte)(mRData[lngResPos + 4] & 0xF));
                            mTrack[i].Notes.Add(intFreq, lngDur, bytAttn).mrawData = 
                                [mRData[lngResPos],
                                mRData[lngResPos + 1],
                                mRData[lngResPos + 2],
                                mRData[lngResPos + 3],
                                mRData[lngResPos + 4]];
                            lngTLength += lngDur;
                        }
                        else {
                            // NOISE channel:
                            // duration
                            lngDur = (mRData[lngResPos] + 256 * mRData[lngResPos + 1]);
                            // get freq divisor (first two bits of fourth byte)
                            // and noise type (3rd bit) as a single number
                            intFreq = ((short)(mRData[lngResPos + 3] & 7));
                            // attenuation
                            bytAttn = (byte)(mRData[lngResPos + 4] & 0xF);
                            // if duration>0
                            if (lngDur > 0) {
                                mTrack[3].Notes.Add(intFreq, lngDur, bytAttn).mrawData =
                                [mRData[lngResPos],
                                mRData[lngResPos + 1],
                                mRData[lngResPos + 2],
                                mRData[lngResPos + 3],
                                mRData[lngResPos + 4]];
                                // add to length
                                lngTLength += lngDur;
                            }
                        }
                    }
                    // update total sound length
                    if (lngTLength > lngLength) {
                        lngLength = lngTLength;
                    }
                }
            }
            catch (Exception e) {
                // bad sound data
                mErrLevel = -14;
                ErrData[0] = mResID;
                ErrData[1] = e.Message;
            }
            // calculate length in seconds (original playsound DOS app used sound
            // timing of 1/64 sec per tick but correct value is 1/60 sec)
            mLength = (double)lngLength / 60;

            // done
            mTracksSet = true;
            mIsDirty = false;
        }

        /// <summary>
        /// Compiles this sound by converting track notes into correctly formatted AGI
        /// resource data stream.
        /// </summary>
        void CompileSound() {
            int i = 0, j;
            Sound tmpRes;
            tmpRes = new Sound();

            // placeholder for header
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
                tmpRes.WriteByte(0xFF);
                tmpRes.WriteByte(0xFF);
            }
            catch (Exception e) {
                WinAGIException wex = new(LoadResString(566).Replace(ARG1, e.Message)) {
                    HResult = WINAGI_ERR + 566
                };
                wex.Data["exception"] = e;
                wex.Data["ID"] = mResID;
                throw wex;
            }
            mRData.AllData = tmpRes.mRData.AllData;
            mTracksSet = true;
        }

        /// <summary>
        /// This mehod is called by child notes so build and dirty statuses can be updated.
        /// </summary>
        internal void NoteChanged() {
            mOutputSet = false;
            mIsDirty = true;
            mLength = -1;
        }

        /// <summary>
        /// Clears the sound resource to a default PC/PCjr sound with no notes in any track.
        /// </summary>
        public override void Clear() {
            int i;
            WinAGIException.ThrowIfNotLoaded(this);
            base.Clear();
            mRData.AllData = [ 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0x08, 0x00, 0xff, 0xff];
            // byte 0/1, 2/2, 4/5, 6/7 = offset to track data
            // byte 8/9 are end of track markers

            // clear all tracks
            for (i = 0; i <= 3; i++) {
                mTrack[i] = new Track(this) {
                    Instrument = 0
                };
                mTrack[0].Muted = false;
                mTrack[0].Visible = true;
            }
            mOutputSet = false;
            mLength = 0;
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
                    mLength = (double)GetSoundLength();
                }
                return mLength;
            }
        }

        /// <summary>
        /// Plays this sound asynchronsously. The output is determined by mode:<br />
        /// 0 = emulated PCjr soundchip<br />
        /// 1 = MIDI (converted from PCjr sound, or native Apple IIgs MIDI)<br />
        /// 2 = wav (from a native Apple IIgs PCM wav file)
        /// </summary>
        /// <param name="mode"></param>
        /// <exception cref="Exception"></exception>
        public void PlaySound(SoundFormat mode) {
            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            switch (mode) {
            case SoundFormat.sfAGI:
            case SoundFormat.sfWAV:
                // default for PCjr is WAV
                // IIgs PCM can only be played as WAV
                if (mFormat != SoundFormat.sfAGI && mFormat != SoundFormat.sfWAV) {
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
                        // pass along exception
                        throw;
                    }
                }
                try {
                    // play the sound
                    wavPlayer.PlayWAVSound(this);
                }
                catch (Exception) {
                    // pass along exception
                    throw;
                }
                break;
            case SoundFormat.sfMIDI:
                // midi (format 1 or 3 converted from agi, or native IIg midi)
                if (mFormat != SoundFormat.sfAGI && mFormat != SoundFormat.sfMIDI) {
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
                        // pass along exception
                        throw;
                    }
                }
                try {
                    //play the sound
                    midiPlayer.PlayMIDISound(this);
                }
                catch (Exception) {
                    // pass along exception
                    throw;
                }
                break;
            }
        }

        /// <summary>
        /// Copies sound data from this sound and returns a completely separate object
        /// reference.
        /// </summary>
        /// <returns>a clone of this sound</returns>
        public Sound Clone() {
            //copies sound data from this sound and returns a completely separate object reference
            Sound CopySound = new();
            // copy base properties
            base.Clone(CopySound);
            // copy WinAGI items
            CopySound.mKey = mKey;
            CopySound.mTPQN = mTPQN;
            CopySound.mTracksSet = mTracksSet;
            CopySound.mLength = mLength;
            CopySound.mFormat = mFormat;
            //never copy output build status; cloned sound will have to rebuild it
            CopySound.mOutputSet = false;
            // clone the tracks
            for (int i = 0; i < 4; i++) {
                CopySound.mTrack[i] = mTrack[i].Clone(this);
            }
            return CopySound;
        }

        public new void Export(string ExportFile) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (!mTracksSet) {
                try {
                    CompileSound();
                }
                catch (Exception) {
                    throw;
                }
            }
            base.Export(ExportFile);
            if (!mInGame) {
                // ID always tracks the resfile name
                mResID = Path.GetFileName(ExportFile);
                if (mResID.Length > 64) {
                    mResID = Left(mResID, 64);
                }
            }
        }

        public void ExportAsMIDI(string MIDIFile) {
            WinAGIException.ThrowIfNotLoaded(this);

            if (!mTracksSet) {
                try {
                    CompileSound();
                }
                catch (Exception) {
                    throw;
                }
            }
            // pcjr and IIgs midi can be exported as midi
            if (mFormat != SoundFormat.sfAGI && mFormat != SoundFormat.sfMIDI) {
                WinAGIException wex = new(LoadResString(596)) {
                    HResult = WINAGI_ERR + 596,
                };
                throw wex;
            }
            try {
                if (!mOutputSet) {
                    BuildSoundOutput();
                }
                if (File.Exists(MIDIFile)) {
                    File.Delete(MIDIFile);
                }
                FileStream fsSnd = new(MIDIFile, FileMode.Open);
                fsSnd.Write(midiPlayer.mMIDIData);
                fsSnd.Dispose();
            }
            catch (Exception) {
                // pass along error
                throw;
            }
        }

        public void ExportAsWAV(string WAVFile) {
            WinAGIException.ThrowIfNotLoaded(this);
            if (!mTracksSet) {
                try {
                    CompileSound();
                }
                catch (Exception) {
                    throw;
                }
            }
            // TODO: need to add support for exporting pcjr (wav data builder 
            // already in pcjrPlayer)
            // only pcjr and IIgs pcm can be exported as wav file
            //if wrong format
            if (mFormat != SoundFormat.sfAGI && mFormat != SoundFormat.sfWAV) {
                WinAGIException wex = new(LoadResString(596)) {
                    HResult = WINAGI_ERR + 596,
                };
                throw wex;
            }
            try {
                //if data not set
                if (!mOutputSet) {
                    BuildSoundOutput();
                }
                if (File.Exists(WAVFile)) {
                    File.Delete(WAVFile);
                }
                //create WAV file
                // TODO: need to include header data here,not in Build function
                FileStream fsSnd = new(WAVFile, FileMode.Open);
                fsSnd.Write(wavData);
                fsSnd.Dispose();
            }
            catch (Exception) {
                // pass along any errors
                throw;
            }
        }

        public void ExportAsScript(string ExportFile) {
            int i, j;
            WinAGIException.ThrowIfNotLoaded(this);

            if (!mTracksSet) {
                try {
                    CompileSound();
                }
                catch (Exception) {
                    throw;
                }
            }
            // only agi format can be exported as script
            if (mFormat != SoundFormat.sfAGI) {
                WinAGIException wex = new(LoadResString(596)) {
                    HResult = WINAGI_ERR + 596,
                };
                throw wex;
            }
            try {
                if (File.Exists(ExportFile)) {
                    File.Delete(ExportFile);
                }
                // creat script file
                FileStream fsSnd = new(ExportFile, FileMode.Open);
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
                fsSnd.Dispose();
                swSnd.Dispose();
            }
            catch (Exception) {
                // pass along any errors
                throw;
            }
        }

        public override void Import(string ImportFile) {
            // imports a sound resource
            // TODO: importing also has to load the resource and set error level
            // TODO: create individual overload functions for different types of importing
            // (instead of trying to analyze the data)

            short intData;
            string strLine;
            string[] strLines, strTag;
            int i, lngTrack, lngDur;
            int lngNoteType = 0;
            bool blnError = false;
            short intFreq;
            byte bytVol;
            // determine file format by checking for '8'-'0' start to file
            // (that is how all sound resources will begin)
            // TODO: do I need a 'using' here? also error handler in case file is 
            // missing or invalid or readonly?
            FileStream fsSnd = new(ImportFile, FileMode.Open);
            BinaryReader brSnd = new(fsSnd);

            //verify long enough
            if (fsSnd.Length <= 2) {
                //error
                fsSnd.Dispose();
                brSnd.Dispose();

                WinAGIException wex = new(LoadResString(681)) {
                    HResult = WINAGI_ERR + 681
                };
                throw wex;
            }
            //set key and tpqn defaults
            mTPQN = 16;
            mKey = 0;
            //set ID
            mResID = Path.GetFileName(ImportFile);
            if (mResID.Length > 64) {
                mResID = Left(mResID, 64);
            }
            //get integer Value at beginning of file
            intData = brSnd.ReadInt16();
            fsSnd.Dispose();
            brSnd.Dispose();
            //if sound resource, intData = 8
            if (intData == 8) {
                try {
                    // import the resource
                    base.Import(ImportFile);
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
                //load the notes into the tracks
                LoadTracks();
            }
            else {
                //must be a script
                //clear the resource
                Clear();
                lngTrack = -1;
                //import a script file
                try {
                    fsSnd = new FileStream(ImportFile, FileMode.Open);
                    StreamReader srSnd = new(fsSnd);
                    //get data from file
                    strLine = srSnd.ReadToEnd();
                    fsSnd.Dispose();
                    srSnd.Dispose();
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
                //replace crlf with cr only
                strLine = strLine.Replace("\n\r", "\r");
                //replace lf with cr
                strLine = strLine.Replace("\n", "\r");
                //split based on cr
                strLines = strLine.Split("\r");
                i = 0;
                bool bVal;
                while (i < strLines.Length) // Until i > UBound(strLines)
                {
                    //get next line
                    strLine = strLines[i].Trim();
                    //check for winagi tags
                    if (Left(strLine, 2) == "##") {
                        //split the line into tag and Value
                        strTag = strLine.Split("=");
                        //should only be two
                        if (strTag.Length == 2) {
                            //what is the tag?
                            switch (strTag[0].Trim().ToLower()) {
                            case "##description":
                                //use this description
                                mDescription = strTag[1];
                                break;
                            case "##instrument0":
                                //set instrument
                                mTrack[0].Instrument = (byte)Val(strTag[1]);
                                break;
                            case "##instrument1":
                                //set instrument
                                mTrack[1].Instrument = (byte)Val(strTag[1]);
                                break;
                            case "##instrument2":
                                //set instrument
                                mTrack[2].Instrument = (byte)Val(strTag[1]);
                                break;
                            case "##visible0":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[0].Visible = bVal;
                                }
                                else {
                                    mTrack[0].Visible = false;
                                }
                                break;
                            case "##visible1":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[1].Visible = bVal;
                                }
                                else {
                                    mTrack[1].Visible = false;
                                }
                                break;
                            case "##visible2":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[2].Visible = bVal;
                                }
                                else {
                                    mTrack[2].Visible = false;
                                }
                                break;
                            case "##visible3":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[3].Visible = bVal;
                                }
                                else {
                                    mTrack[3].Visible = false;
                                }
                                break;
                            case "##muted0":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[0].Muted = bVal;
                                }
                                else {
                                    mTrack[0].Muted = false;
                                }
                                break;
                            case "##muted1":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[1].Muted = bVal;
                                }
                                else {
                                    mTrack[1].Muted = false;
                                }
                                break;
                            case "##muted2":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[2].Muted = bVal;
                                }
                                else {
                                    mTrack[2].Muted = false;
                                }
                                break;
                            case "##muted3":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    mTrack[3].Muted = bVal;
                                }
                                else {
                                    mTrack[3].Muted = false;
                                }
                                break;
                            case "##tpqn":
                                mTPQN = ((int)Val(strTag[1]) / 4) * 4;
                                if (mTPQN < 4) {
                                    mTPQN = 4;
                                }
                                else if (mTPQN > 64) {
                                    mTPQN = 64;
                                }
                                break;
                            case "##key":
                                mKey = (int)Val(strTag[1]);
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
                        //ignore blank lines, and commented lines
                        do {
                            if (strLine.Length == 0) {
                                break;
                            }
                            if (strLine[0] == '#') {
                                break;
                            }
                            //check for new track
                            if (strLine.Equals("tone", StringComparison.OrdinalIgnoreCase)) {
                                lngTrack++;
                                //default note type is agi (0)
                                lngNoteType = 0;
                                //default to show track (property change should happend
                                // AFTER track data)
                                mTrack[lngTrack].Visible = true;
                            }
                            else if (strLine.Equals("noise", StringComparison.OrdinalIgnoreCase) && lngTrack != 3) {
                                lngTrack = 3;
                                //no default note type for track 3
                                lngNoteType = -1;
                                //show track
                                mTrack[3].Visible = true;
                            }
                            else {
                                //verify there is a valid track
                                if (lngTrack < 0 || lngTrack > 3) {
                                    //invalid sound resource;
                                    blnError = true;
                                    break;
                                }
                                //split line using commas
                                strTag = strLine.Split(",");
                                //should only be three or four elements
                                if (strTag.Length >= 4) //if four elements (or more; extras are ignored)
                                {
                                    //check first element for new note type, depending on track
                                    if (lngTrack == 3) {
                                        //p// or //w// only
                                        if (strTag[0].Equals("p", StringComparison.OrdinalIgnoreCase)) {
                                            lngNoteType = 0;
                                        }
                                        else if (strTag[0].Equals("w", StringComparison.OrdinalIgnoreCase)) {
                                            lngNoteType = 4;
                                        }
                                        else {
                                            //error
                                            blnError = true;
                                            break;
                                        }
                                        //calculate freq Value
                                        intFreq = (short)((uint)Val(strTag[1]) | (uint)lngNoteType);
                                    }
                                    else {
                                        // for music tracks, 'a' or 'f' only
                                        if (strTag[0] == "a") {
                                            //agi freq index is the Value passed
                                            intFreq = (short)Val(strTag[1]);
                                        }
                                        else if (strTag[0] == "f") {
                                            //a real freq Value was passed
                                            intFreq = (short)Val(strTag[1]);
                                            //can//t be zero
                                            if (intFreq == 0) {
                                                blnError = true;
                                                break;
                                            }
                                            //convert
                                            intFreq = (short)((double)intFreq / 111860);
                                        }
                                        else {
                                            //error
                                            blnError = true;
                                            break;
                                        }
                                    }
                                    //calculate volume and duration
                                    bytVol = (byte)Val(strTag[2]);
                                    lngDur = (int)Val(strTag[3]);
                                }
                                else if (strTag.Length == 3) {
                                    //if three elements
                                    //0, 1, 2 - assume note type //a//
                                    //3 - use previous note type
                                    if (lngTrack != 3) {
                                        intFreq = (short)Val(strTag[0]);
                                    }
                                    else {
                                        //track three
                                        //if no type yet,
                                        if (lngNoteType == -1) {
                                            blnError = true;
                                            break;
                                        }
                                        //calculate freq Value
                                        intFreq = (short)((uint)Val(strTag[0]) | (uint)lngNoteType);
                                    }
                                    //calculate volume and duration
                                    bytVol = (byte)Val(strTag[1]);
                                    lngDur = (int)Val(strTag[2]);
                                }
                                else {
                                    //error
                                    blnError = true;
                                    break;
                                }
                                //validate input
                                if (intFreq < 0 || intFreq >= 1024 || bytVol < 0 || bytVol >= 16 || lngDur < 0 || lngDur > 65535) {
                                    //invalid data
                                    blnError = true;
                                    break;
                                }
                                //duration of zero is not an error, but it is ignored
                                if (lngDur != 0) {
                                    //add the note to current track
                                    mTrack[lngTrack].Notes.Add(intFreq, lngDur, bytVol);
                                }
                            }
                        } while (false);
                        //if error,
                        if (blnError) {
                            Clear();
                            //reset no id
                            mResID = "";
                            mDescription = "";
                            //raise the error

                            WinAGIException wex = new(LoadResString(681)) {
                                HResult = WINAGI_ERR + 681
                            };
                            throw wex;
                        }
                    }
                    // increment line counter
                    i++;
                }
                // compile the sound so the resource matches the tracks
                try {
                    CompileSound();
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
            }
            //reset dirty flags
            mIsDirty = false;
            PropDirty = false;
            //reset track flag
            mTracksSet = true;
        }

        public int TPQN {
            get {
                return mTPQN;
            }
            set {
                //validate it
                value = (value / 4) * 4;
                if (value < 4 || value > 64) {
                    throw new ArgumentOutOfRangeException();
                }
                if (mTPQN != value) {
                    mTPQN = value;
                    PropDirty = true;
                }
            }
        }
        public Track Track(int Index) {
            //validate index
            if (Index < 0 || Index > 3) {
                throw new IndexOutOfRangeException("Index out of bounds");
            }
            //if not loaded
            if (!mLoaded) {
                //error

                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //if sound not set,
            if (!mTracksSet) {
                try {
                    //load tracks first
                    LoadTracks();
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
            }
            //return the desired track
            return mTrack[Index];
        }
        internal void TrackChanged(bool ResetMIDI = true) {
            //called by sound tracks to indicate a change
            //has occured; ResetMIDI flag allows some track changes to
            //occur that don't affect the MIDI data (such as Visible)
            //but still set the writeprops flag

            //when track status changes, need to recalculate length
            mLength = -1;
            if (ResetMIDI) {
                //change in track forces midiset to false
                mOutputSet = false;
            }
            //change in track sets writeprop to true
            PropDirty = true;
        }
        public override void Load() {
            // load data into the sound tracks
            int i;
            // if already loaded
            if (mLoaded) {
                return;
            }
            // if not ingame, the resource is already loaded
            if (!mInGame) {
                // TODO- not true?? can nongame resources be unloaded and loaded?
                // they just need a valid resfile to get data from
                Debug.Assert(mLoaded);
            }
            // load base resource
            base.Load();
            if (mErrLevel < 0) {
                // clear the sound to empty set of tracks
                ErrClear();
            }
            else {
                // finish loading sound
                mKey = parent.agGameProps.GetSetting("Sound" + mResNum, "Key", 0);
                //validate it
                if (mKey < -7 || mKey > 7) {
                    mKey = 0;
                }
                mTPQN = parent.agGameProps.GetSetting("Sound" + mResNum, "TPQN", 0);
                //validate it
                mTPQN = (mTPQN / 4) * 4;
                if (mTPQN < 4) {
                    mTPQN = 4;
                }
                if (mTPQN > 64) {
                    mTPQN = 64;
                }
                // initialize tracks
                for (i = 0; i <= 3; i++) {
                    //clear out tracks by assigning to nothing, then new
                    mTrack[i] = new Track(this);
                }

                //check header to determine what type of sound resource;
                //   0x01 = IIgs sampled sound
                //   0x02 = IIgs midi sound
                //   0x08 = PC/PCjr //standard//
                switch (ReadWord(0)) {
                case 1:
                    mFormat = SoundFormat.sfWAV;
                    //tracks are not applicable, so just set flag to true
                    mTracksSet = true;
                    break;
                case 2:
                    mFormat = SoundFormat.sfMIDI;
                    //tracks are not applicable, so just set flag to true
                    mTracksSet = true;
                    break;
                case 8: //standard PC/PCjr
                    mFormat = SoundFormat.sfAGI;
                    // load notes
                    LoadTracks();
                    break;
                default:
                    // bad sound
                    mErrLevel = -13;
                    ErrData[0] = mResID;
                    // clear to set blank tracks
                    // TODO: I don't think this resets the tracks correctly???
                    ErrClear();
                    break;
                }
                // midi/wav data not set
                mOutputSet = false;
            }
        }
        /// <summary>
        /// Gets the WAV data stream for this sound for playback. Only appliciable for
        /// PCjr and IIgs PCM sounds.
        /// </summary>
        public byte[] WAVData {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                if (mFormat != SoundFormat.sfAGI && mFormat != SoundFormat.sfWAV) {
                    WinAGIException wex = new(LoadResString(596)) {
                        HResult = WINAGI_ERR + 596,
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
                if (mFormat != SoundFormat.sfAGI && mFormat != SoundFormat.sfMIDI) {
                    WinAGIException wex = new(LoadResString(596)) {
                        HResult = WINAGI_ERR + 596,
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

        /// <summary>
        /// 
        /// </summary>
        public void SaveProps() {
            if (PropDirty && mInGame) {
                string strSection = "Sound" + mResNum;
                //save ID and description to ID file
                parent.WriteGameSetting(strSection, "ID", mResID, "Sounds");
                parent.WriteGameSetting(strSection, "Description", mDescription);
                //write song key signature, tqpn, and track instruments
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
                PropDirty = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Save() {
            //saves the sound
            //if not loaded
            if (!mLoaded) {
                //nothing to do
                return;
            }
            // if properties need to be written
            if (PropDirty && mInGame) {
                SaveProps();
            }
            //if dirty
            if (mIsDirty) {
                //compile first
                try {
                    CompileSound();
                    //if type is sound script
                    if (Right(mResFile, 4).Equals(".ass", StringComparison.OrdinalIgnoreCase)) {
                        ExportAsScript(mResFile);
                    }
                    else if (Right(mResFile, 4).Equals(".mid", StringComparison.OrdinalIgnoreCase)) {
                        ExportAsMIDI(mResFile);
                    }
                    else if (Right(mResFile, 4).Equals(".wav", StringComparison.OrdinalIgnoreCase)) {
                        ExportAsWAV(mResFile);
                    }
                    else {
                        //use the resource save method
                        base.Save();
                    }
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
                //mark as clean
                mIsDirty = false;
            }
        }

        public void StopSound() {
            //stops the sound, if it is playing
            //calling this for ANY sound will stop ALL sound

            if (!mLoaded) {
                return;
            }
            // stop all modes
            StopAllSound();
        }
        public override void Unload() {
            //unload resource
            base.Unload();
            mIsDirty = false;
            //clear midi data
            midiPlayer.mMIDIData = [];
            mOutputSet = false;
            //reset length
            mLength = -1;
            //clear notes collection by assigning to nothing
            mTrack[0] = new Track(this);
            mTrack[1] = new Track(this);
            mTrack[2] = new Track(this);
            mTrack[3] = new Track(this);
            mTracksSet = false;
        }
    }
}
