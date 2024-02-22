using System;
using System.ComponentModel;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Commands;
using WinAGI.Common;
using static WinAGI.Common.Base;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace WinAGI.Engine
{
    public class Sound : AGIResource
    {
        Track[] mTrack = new Track[4];
        bool mTracksSet;
        double mLength;
        int mKey;
        int mTPQN;
        int mFormat;
        int mErrLvl;
        //variables to support MIDI file creation
        bool mMIDISet;

        //other
        // Declare the event delegate, and event
        public delegate void SoundCompleteEventHandler(object sender, SoundCompleteEventArgs e);
        public event SoundCompleteEventHandler SoundComplete;
        public class SoundCompleteEventArgs
        {
            public SoundCompleteEventArgs(bool noerror)
            {
                NoError = noerror;
            }
            public bool NoError { get; }
        }
        internal void Raise_SoundCompleteEvent(bool noerror)
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            this.SoundComplete?.Invoke(null, new SoundCompleteEventArgs(noerror));
            //TODO: need to attach events when object instantiated?

            //for example - 
            // new AGISound newSound.CompileGameStatus += myForm.mySoundEventHandler;
        }
        public Sound() : base(AGIResType.rtSound)
        {
            //initialize
            mResID = "NewSound";
            //attach events
            base.PropertyChanged += ResPropChange;
            strErrSource = "WinAGI.Sound";
            //create default PC/PCjr sound with no notes in any tracks
            mRData.AllData = new byte[] { 0x08, 0x00, 0x08, 0x00,
                                    0x08, 0x00, 0x08, 0x00,
                                    0xff, 0xff};
            // byte 0/1, 2/2, 4/5, 6/7 = offset to track data
            // byte 8/9 are end of track markers
            mFormat = 1;
            mTrack[0] = new Track(this);
            mTrack[1] = new Track(this);
            mTrack[2] = new Track(this);
            mTrack[3] = new Track(this);
            mTrack[0].Instrument = 80;
            mTrack[1].Instrument = 80;
            mTrack[2].Instrument = 80;
            //default tqpn is 16
            mTPQN = 16;
            //length is undefined
            mLength = -1;
            //default key is c
            mKey = 0;
        }
        public Sound(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.rtSound)
        {
            //this internal function adds this resource to a game, setting its resource 
            //location properties, and reads properties from the wag file
            //initialize
            //attach events
            base.PropertyChanged += ResPropChange;
            strErrSource = "WinAGI.Sound";

            //set up base resource
            base.InitInGame(parent, ResNum, VOL, Loc);

            //if importing, there will be nothing in the propertyfile
            ID = parent.agGameProps.GetSetting("Sound" + ResNum, "ID", "", true);
            if (ID.Length == 0) {
                //no properties to load; save default ID
                ID = "Sound" + ResNum;
                parent.WriteGameSetting("Sound" + ResNum, "ID", ID, "Sounds");
            }
            else {
                //get description and other properties from wag file
                mDescription = parent.agGameProps.GetSetting("Sound" + ResNum, "Description", "");
                //length is undefined until sound is built
                mLength = -1;
            }
        }
        private void ResPropChange(object sender, AGIResPropChangedEventArgs e)
        {
            //sound data has changed- tracks no longer match
            mTracksSet = false;
            mIsDirty = true;
        }
        void BuildSoundOutput()
        {
            //creates midi/wav output data stream for this sound resource
            try {
                switch (mFormat) {
                case 1: //standard pc/pcjr sound
                        //build the midi first
                    SndPlayer.mMIDIData = BuildMIDI(this);
                    //get length
                    mLength = GetSoundLength();
                    break;
                case 2:  //IIgs pcm sound
                    SndPlayer.mMIDIData = BuildIIgsPCM(this);
                    //get length
                    mLength = GetSoundLength();

                    break;
                case 3: //IIgs MIDI sound
                        //build the midi data and get length info
                    SndPlayer.mMIDIData = BuildIIgsMIDI(this, ref mLength);
                    break;
                } //switch
                  //set flag
                mMIDISet = true;
            }
            catch (Exception) {

                Exception e = new(LoadResString(596))
                {
                    HResult = WINAGI_ERR + 596
                };
                throw e;
            }
        }
        double GetSoundLength()
        {
            int i;
            double retval = 0;
            //this function assumes a sound has been loaded properly
            //get length
            switch (mFormat) {
            case 1: //standard pc/pcjr resource
                for (i = 0; i <= 3; i++) {
                    if (retval < mTrack[i].Length && !mTrack[i].Muted) {
                        retval = mTrack[i].Length;
                    }
                }
                break;
            case 2:  //pcm sampling
                     //since sampling is at 8000Hz, size is just data length/8000
                retval = ((double)mSize - 54) / 8000;
                break;
            case 3: //iigs midi
                    //length has to be calculated during midi build
                BuildSoundOutput();
                retval = mLength;
                break;
            } //switch
              //does 0 work?
            return retval;
        }
        public Track this[int index]
        {
            get
            {
                return Track(index);
            }
        }
        public int Key
        {
            get => mKey;
            set
            {
                //validate
                if (value < -7 || value > 7) {
                    //raise error
                    throw new Exception("380, strErrSource, Invalid property Value");
                }

                if (mKey != value) {
                    //assign it
                    mKey = value;
                    //set props flag
                    WritePropState = true;
                }
            }
        }
        public int SndFormat
        {
            //  0 = not loaded
            //  1 = //standard// agi
            //  2 = IIgs sampled sound
            //  3 = IIgs midi
            get
            {
                if (mLoaded) {
                    return mFormat;
                }
                else {
                    return 0;
                }
            }
        }
        public int ErrLevel
        {

            //provides access to current error level of the sound tracks

            //can be used by calling programs to provide feedback
            //on errors in the sound data

            //return 0 if successful, no errors/warnings
            // non-zero for error/warning:
            //  -1 = error- can't build sound tracks
            //   1 = bad track0 offset
            //   2 = bad track1 offset
            //   4 = bad track2 offset
            //   8 = bad track3 offset
            //  16 = bad track0 data
            //  32 = bad track1 data
            //  64 = bad track2 data
            // 128 = bad track3 data
            // 256 = missing track data
            get
            {
                return mErrLvl;
            }
        }

        internal void LoadTracks()
        {
            int i, lngLength = 0, lngTLength;
            int lngTrackStart, lngTrackEnd;
            int lngStart, lngEnd, lngResPos, lngDur;
            short intFreq;
            byte bytAttn;
            //if in a game,
            if (mInGame) {
                //get track properties from the .WAG file
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
                for (i = 0; i <= 3; i++) {
                    mTrack[i].Visible = true;
                } //next i
            }
            try {
                //extract note information for each track from resource
                //write the sound tracks
                for (i = 0; i <= 2; i++) {
                    //reset length for this track
                    lngTLength = 0;
                    //get start and end of this track (stored at beginning of resource
                    //in LSMS format)
                    //  track 0 start is byte 0-1, track 1 start is byte 2-3
                    //  track 2 start is byte 4-5, noise start is byte 6-7
                    lngStart = mRData[i * 2 + 0] + 256 * mRData[i * 2 + 1];
                    //end is start of next track -5 (5 bytes per note in each track) -2 (trailing 0xFFFF)
                    lngEnd = mRData[i * 2 + 2] + 256 * mRData[i * 2 + 3] - 7;
                    //validate
                    if (lngStart < 0 || lngEnd < 0 || lngStart > mSize || lngEnd > mSize) {
                        //raise error

                        Exception e = new(LoadResString(598))
                        {
                            HResult = WINAGI_ERR + 598
                        };
                        throw e;
                    }

                    //step through notes in this track (5 bytes at a time)
                    for (lngResPos = lngStart; lngResPos <= lngEnd; lngResPos += 5) {
                        //get duration
                        lngDur = (mRData[lngResPos] + 256 * mRData[lngResPos + 1]);

                        //get frequency
                        intFreq = (short)(16 * (mRData[lngResPos + 2] & 0x3F) + (mRData[lngResPos + 3] & 0xF));
                        //attenuation information in byte5
                        bytAttn = ((byte)(mRData[lngResPos + 4] & 0xF));
                        //add the note
                        mTrack[i].Notes.Add(intFreq, lngDur, bytAttn);
                        //add length
                        lngTLength += lngDur;
                    } //next lngResPos

                    //if this is longest length
                    if (lngTLength > lngLength) {
                        lngLength = lngTLength;
                    }
                } //next i
                lngTLength = 0;
                //getstart and end of noise track
                lngStart = mRData[6] + 256 * mRData[7];
                lngEnd = mSize - 7;
                for (lngResPos = lngStart; lngResPos <= lngEnd; lngResPos += 5) {
                    //First and second byte: Note duration
                    lngDur = (mRData[lngResPos] + 256 * mRData[lngResPos + 1]);
                    //get freq divisor (first two bits of fourth byte)
                    //and noise type (3rd bit) as a single number
                    intFreq = ((short)(mRData[lngResPos + 3] & 7));
                    //Fifth byte: volume attenuation
                    bytAttn = (byte)(mRData[lngResPos + 4] & 0xF);

                    //if duration>0
                    if (lngDur > 0) {
                        //add the note
                        mTrack[3].Notes.Add(intFreq, lngDur, bytAttn);
                        //add to length
                        lngTLength += lngDur;
                    }
                } //next lngResPos
                  //if this is longest length
                if (lngTLength > lngLength) {
                    lngLength = lngTLength;
                }
            }
            catch (Exception e) {

                Exception eR = new(LoadResString(565).Replace(ARG1, e.Message))
                {
                    HResult = WINAGI_ERR + 565
                };
                throw eR;
            }
            //save length
            // original playsound dos app used, sound tick of 1/64 sec
            // but correct value is 1/60 sec
            mLength = (double)lngLength / 60;

            //set flag to indicate tracks loaded
            mTracksSet = true;
            //MUST be clean, since loaded from resource data
            mIsDirty = false;
        }
        void CompileSound()
        {
            //compiles this sound by converting notes into an AGI resource datastream
            int i, j;
            Sound tmpRes;
            tmpRes = new Sound();
            //build header
            tmpRes.WriteWord(8, 0);
            i = 0;
            for (j = 0; j <= 2; j++) {
                i = i + (mTrack[j].Notes.Count * 5) + 2;
                tmpRes.WriteWord((ushort)(8 + i));
            } //next j
            try {
                //add regular tracks
                for (j = 0; j <= 2; j++) {
                    for (i = 0; i < mTrack[j].Notes.Count; i++) {
                        //write duration
                        tmpRes.WriteWord((ushort)mTrack[j].Notes[i].Duration);
                        //add frequency bytes
                        tmpRes.WriteByte((byte)(mTrack[j].Notes[i].FreqDivisor / 16));
                        tmpRes.WriteByte((byte)((mTrack[j].Notes[i].FreqDivisor % 16) + 128 + 32 * j));
                        //add attenuation
                        tmpRes.WriteByte((byte)(mTrack[j].Notes[i].Attenuation + 144 + 32 * j));
                    } //next i
                      //add end of track
                    tmpRes.WriteByte(0xFF);
                    tmpRes.WriteByte(0xFF);
                } //next j
                  //write noise track
                for (i = 0; i < mTrack[3].Notes.Count; i++) {
                    //write duration
                    tmpRes.WriteWord((ushort)mTrack[3].Notes[i].Duration);
                    //write placeholder
                    tmpRes.WriteByte(0);
                    //write type and freq
                    tmpRes.WriteByte((byte)(224 + mTrack[3].Notes[i].FreqDivisor));
                    //add attenuation
                    tmpRes.WriteByte((byte)(mTrack[3].Notes[i].Attenuation + 240));
                } //next i
                  //add end of track
                tmpRes.WriteByte(0xFF);
                tmpRes.WriteByte(0xFF);
            }
            catch (Exception e) {
                Exception eR = new(LoadResString(566).Replace(ARG1, e.Message))
                {
                    HResult = WINAGI_ERR + 566
                };
                throw eR;
            }
            //assign data to resource
            mRData.AllData = tmpRes.mRData.AllData;
            //set tracksloaded flag
            mTracksSet = true;
        }
        internal void NoteChanged()
        {
            //called by child notes to indicate a change
            //has occured
            //change in note forces midiset to false
            mMIDISet = false;
            //change in note sets dirty flag to true
            mIsDirty = true;
            //and resets sound length
            mLength = -1;
        }
        public override void Clear()
        {
            int i;
            if (!mLoaded) {

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            base.Clear();
            //write data for default view
            //set default resource data
            //create default PC/PCjr sound with no notes in any tracks
            //set default resource data
            mRData.AllData = new byte[] { 0x08, 0x00, 0x08, 0x00,
                                    0x08, 0x00, 0x08, 0x00,
                                    0xff, 0xff};
            // byte 0/1, 2/2, 4/5, 6/7 = offset to track data
            // byte 8/9 are end of track markers

            //clear all tracks
            for (i = 0; i <= 3; i++) {
                //clear out tracks by assigning to nothing, then new
                mTrack[i] = new Track(this)
                {
                    //set track defaults
                    Instrument = 0
                };
                mTrack[0].Muted = false;
                mTrack[0].Visible = true;
            } //next i
              //reset length
            mLength = -1;
        }
        public double Length
        {
            get
            {
                //returns length of sound in seconds
                int i;
                //if not loaded,
                if (!mLoaded) {
                    return -1;
                }
                //if length is changed
                if (mLength == -1) {
                    mLength = (double)GetSoundLength();
                    System.Diagnostics.Debug.Print($"length: {mLength}");
                }
                return mLength;
            }
        }
        public void PlaySound()
        {  //plays sound asynchronously by generating a MIDI stream
           //that is fed to a MIDI output

            //if not loaded
            if (!mLoaded) {
                //error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }

            //if sound is already open
            if (SndPlayer.blnPlaying) {
                // TODO: change this to just stop the sound instead of error; also
                // need tomake sure all sounds get stopped- should this be a static function?
                // YES...

                Exception e = new(LoadResString(629))
                {
                    HResult = WINAGI_ERR + 629
                };
                throw e;
            }
            //dont need to worry if tracks are properly loaded because
            //changing track data causes mMIDISet to be reset; this forces
            //midi rebuild, which references the Tracks throught the Track
            //property, which forces rebuild of track data when the
            //BUILDMIDI method first access the track property

            //if sound data not set to play
            if (!mMIDISet) {
                try {
                    BuildSoundOutput();
                }
                catch (Exception) {
                    throw new Exception("lngError, strErrSrc, strError");
                }
            }
            try {
                //play the sound
                SndPlayer.PlaySound(this);
            }
            catch (Exception) {
                throw new Exception("lngError, strErrSrc, strError");
            }
        }
        public Sound Clone()
        {
            //copies sound data from this sound and returns a completely separate object reference
            Sound CopySound = new Sound();
            // copy base properties
            base.Clone(CopySound);
            //add WinAGI items
            CopySound.mKey = mKey;
            CopySound.mTPQN = mTPQN;
            CopySound.mTracksSet = mTracksSet;
            CopySound.mLength = mLength;
            CopySound.mFormat = mFormat;

            //never copy midiset; cloned sound will have to build it own?
            CopySound.mMIDISet = false; // mMIDISet; ;
                                        //clone the tracks
            for (int i = 0; i < 4; i++) {
                CopySound.mTrack[i] = mTrack[i].Clone(this);
            }
            return CopySound;
        }
        public void Export(string ExportFile, SoundFormat FileFormat = SoundFormat.sfAGI, bool ResetDirty = true)
        {
            int i, j;
            int lngType, lngFreq;
            //if not loaded
            if (!mLoaded) {
                //error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            //if format is not predefined
            if ((FileFormat <= SoundFormat.sfUndefined) || ((int)FileFormat > 3)) {
                //need to determine type from filename
                if (Right(ExportFile, 4).Equals(".ass", StringComparison.OrdinalIgnoreCase)) {
                    //script
                    FileFormat = SoundFormat.sfScript;
                }
                else if (Right(ExportFile, 4).Equals(".mid", StringComparison.OrdinalIgnoreCase)) {
                    //midi
                    FileFormat = SoundFormat.sfMIDI;
                }
                else if (Right(ExportFile, 4).Equals(".wav", StringComparison.OrdinalIgnoreCase)) {
                    //wav
                    FileFormat = SoundFormat.sfWAV;
                }
                else {
                    //default to agi
                    FileFormat = SoundFormat.sfAGI;
                }
            }
            if (mIsDirty) {
                try {
                    //need to recompile
                    CompileSound();
                }
                catch (Exception) {
                    throw;
                }
            }
            //export accordind to desred format
            switch (FileFormat) {
            case SoundFormat.sfAGI: //all data formats OK
                                    //export agi resource
                base.Export(ExportFile);
                if (!mInGame) {
                    if (ResetDirty) {
                        //clear dirty flag
                        mIsDirty = false;
                    }
                }
                break;
            case SoundFormat.sfMIDI: //pc and IIgs midi
                                     //if wrong format
                if (mFormat == 2) {
                    throw new Exception("596, strErrSource, Can't export PCM formatted resource as MIDI file");
                }
                try {
                    //if midi not set
                    if (!mMIDISet) {
                        //build the midi first
                        BuildSoundOutput();
                    }
                    //delete any existing file
                    if (File.Exists(ExportFile)) {
                        File.Delete(ExportFile);
                    }
                    //create midi file
                    FileStream fsSnd = new FileStream(ExportFile, FileMode.Open);
                    fsSnd.Write(SndPlayer.mMIDIData);
                    fsSnd.Dispose();
                }
                catch (Exception) {
                    // pass along error
                    throw;
                }
                break;
            case SoundFormat.sfScript: //pc only
                                       //if wrong format
                if (mFormat != 1) {
                    throw new Exception("596, strErrSource, Only PC/PCjr sound resources can be exported as script files");
                }
                try {
                    //delete any existing file
                    if (File.Exists(ExportFile)) {
                        File.Delete(ExportFile);
                    }
                    //creat script file
                    FileStream fsSnd = new FileStream(ExportFile, FileMode.Open);
                    StreamWriter swSnd = new StreamWriter(fsSnd);
                    //if sound not set,
                    if (!mTracksSet) {
                        //load tracks first
                        LoadTracks();
                    }
                    //add comment header
                    swSnd.WriteLine("# agi script file");
                    swSnd.WriteLine("");
                    swSnd.WriteLine("##Description=" + mDescription);
                    swSnd.WriteLine("##TPQN=" + mTPQN);
                    swSnd.WriteLine("");
                    //add sound tracks
                    for (i = 0; i <= 2; i++) {
                        swSnd.WriteLine("# track " + i);
                        swSnd.WriteLine("tone");
                        for (j = 0; j < mTrack[i].Notes.Count; j++) {
                            //if first note,
                            if (j == 0) {
                                //include tone type
                                swSnd.WriteLine("a, " + mTrack[i].Notes[0].FreqDivisor + ", " + mTrack[i].Notes[0].Attenuation + ", " + mTrack[i].Notes[0].Duration);
                            }
                            else {
                                //don't need tone type
                                swSnd.WriteLine(mTrack[i].Notes[j].FreqDivisor + ", " + mTrack[i].Notes[j].Attenuation + ", " + mTrack[i].Notes[j].Duration);
                            }
                        } //next j
                          //add instrument, visible and muted properties
                        swSnd.WriteLine("##instrument" + i + "=" + mTrack[i].Instrument);
                        swSnd.WriteLine("##visible" + i + "=" + mTrack[i].Visible);
                        swSnd.WriteLine("##muted" + i + "=" + mTrack[i].Muted);
                        swSnd.WriteLine("");
                    } //next i
                      //add noise track
                    swSnd.WriteLine("# track 3");
                    swSnd.WriteLine("noise");
                    for (j = 0; j < mTrack[3].Notes.Count; j++) {
                        //if note is white noise(bit 2 of freq is 1)
                        if ((mTrack[3].Notes[j].FreqDivisor & 4) == 4) {
                            //white noise
                            swSnd.WriteLine("w," + (mTrack[3].Notes[j].FreqDivisor & 3) + ", " + mTrack[3].Notes[j].Attenuation + ", " + mTrack[3].Notes[j].Duration);
                        }
                        else {
                            swSnd.WriteLine("p," + (mTrack[3].Notes[j].FreqDivisor & 3) + ", " + mTrack[3].Notes[j].Attenuation + ", " + mTrack[3].Notes[j].Duration);
                        }
                    } //next j
                      //add visible and muted properties
                    swSnd.WriteLine("##visible3=" + mTrack[3].Visible);
                    swSnd.WriteLine("##muted3=" + mTrack[3].Muted);
                    //close the file
                    fsSnd.Dispose();
                    swSnd.Dispose();
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
                if (ResetDirty) {
                    //clear dirty flag
                    mIsDirty = false;
                }
                break;
            case SoundFormat.sfWAV: //IIgs pcm only
                                    //if wrong format
                if (mFormat != 2) {
                    throw new Exception("596, strErrSource, Can't export MIDI formatted sound resource as .WAV file");
                }
                try {
                    //if data not set
                    if (!mMIDISet) {
                        //build the wav first
                        //build the midi first
                        BuildSoundOutput();
                    }
                    //delete any existing file
                    if (File.Exists(ExportFile)) {
                        File.Delete(ExportFile);
                    }
                    //create midi file
                    FileStream fsSnd = new FileStream(ExportFile, FileMode.Open);
                    fsSnd.Write(SndPlayer.mMIDIData);
                    fsSnd.Dispose();
                }
                catch (Exception) {
                    // pass along any errors
                    throw;
                }
                break;
            } //switch
              //if not in a game,
            if (!mInGame) {
                //ID always tracks the resfile name
                mResID = Path.GetFileName(ExportFile);
                if (mResID.Length > 64) {
                    mResID = Left(mResID, 64);
                }
            }
        }
        public override void Import(string ImportFile)
        {
            //imports a sound resource
            short intData;
            string strLine;
            string[] strLines, strTag;
            int i, lngTrack, lngDur;
            int lngNoteType = 0;
            bool blnError = false;
            short intFreq;
            byte bytVol;
            //determine file format by checking for '8'-'0' start to file
            //(that is how all sound resources will begin)
            FileStream fsSnd = new FileStream(ImportFile, FileMode.Open);
            BinaryReader brSnd = new BinaryReader(fsSnd);
            //verify long enough
            if (fsSnd.Length <= 2) {
                //error
                fsSnd.Dispose();
                brSnd.Dispose();

                Exception e = new(LoadResString(681))
                {
                    HResult = WINAGI_ERR + 681
                };
                throw e;
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
                    //import the resource
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
                    StreamReader srSnd = new StreamReader(fsSnd);
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
                bool bVal = false;
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
                            } //switch
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
                            if (strLine.ToLower() == "tone") {
                                lngTrack++;
                                //default note type is agi (0)
                                lngNoteType = 0;
                                //default to show track (property change should happend
                                // AFTER track data)
                                mTrack[lngTrack].Visible = true;
                            }
                            else if (strLine.ToLower() == "noise" && lngTrack != 3) {
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
                                        if (strTag[0].ToLower() == "p") {
                                            lngNoteType = 0;
                                        }
                                        else if (strTag[0].ToLower() == "w") {
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

                            Exception e = new(LoadResString(681))
                            {
                                HResult = WINAGI_ERR + 681
                            };
                            throw e;
                        }
                    }
                    //increment line counter
                    i++;
                } //while
                  //compile the sound so the resource matches the tracks
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
            WritePropState = false;
            //reset track flag
            mTracksSet = true;
        }
        public int TPQN
        {
            get
            {
                return mTPQN;
            }
            set
            {
                //validate it
                value = (value / 4) * 4;
                if (value < 4 || value > 64) {
                    //raise error
                    throw new Exception("380, strErrSource, Invalid property Value");
                }
                if (mTPQN != value) {
                    mTPQN = value;
                    WritePropState = true;
                }
            }
        }
        public Track Track(int Index)
        {
            //validate index
            if (Index < 0 || Index > 3) {
                throw new IndexOutOfRangeException("Index out of bounds");
            }
            //if not loaded
            if (!mLoaded) {
                //error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
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
        internal void TrackChanged(bool ResetMIDI = true)
        {
            //called by sound tracks to indicate a change
            //has occured; ResetMIDI flag allows some track changes to
            //occur that don't affect the MIDI data (such as Visible)
            //but still set the writeprops flag

            //when track status changes, need to recalculate length
            mLength = -1;
            if (ResetMIDI) {
                //change in track forces midiset to false
                mMIDISet = false;
            }
            //change in track sets writeprop to true
            WritePropState = true;
        }
        public override void Load()
        {
            //load data into the sound tracks
            int i;
            //if already loaded
            if (Loaded) {
                return;
            }
            //if not ingame, the resource is already loaded
            if (!InGame) {//TODO- not true! nongame resources certainly can be unloaded and loaded;
                          // they just need a valid resfile to get data from
                throw new Exception("non-game sound should already be loaded");
            }
            try {
                //load base resource
                base.Load();
            }
            catch (Exception) {
                // pass along any error
                throw;
            }
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
            } //next i

            //check header to determine what type of sound resource;
            //   0x01 = IIgs sampled sound
            //   0x02 = IIgs midi sound
            //   0x08 = PC/PCjr //standard//
            switch (ReadWord(0)) {
            case 1: //IIgs sampled sound
                mFormat = 2;
                //tracks are not applicable, so just set flag to true
                mTracksSet = true;
                break;
            case 2: //IIgs midi
                mFormat = 3;
                //tracks are not applicable, so just set flag to true
                mTracksSet = true;
                break;
            case 8: //standard PC/PCjr
                mFormat = 1;
                try {
                    //load notes
                    LoadTracks();
                }
                catch (Exception) {
                    // pass along error
                    throw;
                }
                break;
            default:
                //bad sound
                Unload();

                Exception e = new(LoadResString(598))
                {
                    HResult = WINAGI_ERR + 598
                };
                throw e;
            } //switch
              //clear dirty flag
            mIsDirty = false;
            WritePropState = false;
        }
        public byte[] MIDIData
        {
            get
            {
                //returns the MIDI data stream or WAV strem for this sound resource
                //if not loaded
                if (!mLoaded) {
                    //error

                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
                //if resource changed,
                if (!mMIDISet) {
                    try {
                        //build the midi first
                        BuildSoundOutput();
                    }
                    catch (Exception) {
                        // pass along errors
                        throw;
                    }
                }
                return SndPlayer.mMIDIData;
            }
        }
        public new void Save(string SaveFile = "")
        {
            //saves the sound
            string strSection;
            //if properties need to be written
            if (WritePropState && mInGame) {
                strSection = "Sound" + mResNum;
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
                WritePropState = false;
            }
            //if not loaded
            if (!mLoaded) {
                //nothing to do
                return;
            }
            //if dirty
            if (mIsDirty) {
                //compile first
                try {
                    CompileSound();
                    //if type is sound script
                    if (Right(SaveFile, 4).Equals(".ass", StringComparison.OrdinalIgnoreCase)) {
                        //use export for script
                        Export(mResFile, SoundFormat.sfScript);
                    }
                    else if (Right(SaveFile, 4).Equals(".mid", StringComparison.OrdinalIgnoreCase)) {
                        //use export for MIDI
                        Export(mResFile, SoundFormat.sfMIDI);
                    }
                    else {
                        //use the resource save method
                        base.Save(SaveFile);
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
        public void StopSound()
        {
            //stops the sound, if it is playing
            //calling this for ANY sound will stop ALL sound
            int rtn;
            //if not loaded
            if (!mLoaded) {
                //do nothing
                //        return;
            }
            //if playing
            if (SndPlayer.blnPlaying) {
                rtn = API.mciSendString("close all", null, 0, (IntPtr)null);
                SndPlayer.blnPlaying = false;
            }
        }
        public override void Unload()
        {
            //unload resource
            base.Unload();
            mIsDirty = false;
            //clear midi data
            SndPlayer.mMIDIData = Array.Empty<byte>();
            mMIDISet = false;
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
