using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using WinAGI.Common;
using System.Diagnostics;
using System.Linq;

namespace WinAGI.Engine {
    public enum SoundImportFormat {
        AGI,
        IT,
        MOD,
        MIDI,
        Script,
    }

    public static class SoundImport {
        // Import functions for Impulse Tracker files and Protracker MOD files
        // are based on the it2agi.pl perl script, version 0.2.7, originally
        // written by Nat Budin (with some code provided by Lance Ewing), and
        // updated by Adam 'Sinus' Skawinski.

        public const double AGI_TICK = 16.66667;

        public static SoundImportFormat GetSoundInputFormat(string filename) {
            // Determine the file extension and return the corresponding format
            string ext = Path.GetExtension(filename).ToLowerInvariant();
            SoundImportFormat retval;
            switch (ext) {
            case    ".it":
                retval = SoundImportFormat.IT;
                break;
            case ".mod":
                retval = SoundImportFormat.MOD;
                break;
            case ".mid":
            case ".midi":
                retval = SoundImportFormat.MIDI;
                break;
            case ".ass":
                retval = SoundImportFormat.Script;
                break;
            case ".ags":
                retval = SoundImportFormat.AGI;
                break;
            default:
                // default to AGI for other extensions
                retval = SoundImportFormat.AGI;
                break;
            }
            return retval;
            // TODO: check file content for more robust detection
        }

        /// <summary>
        /// Imports a sound resource from a script file into this sound.
        /// </summary>
        /// <param name="scriptfile"></param>
        public static void Script2AGI(string scriptfile, Sound sound) {
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
                                sound.Description = strTag[1];
                                break;
                            case "##instrument0":
                                if (byte.TryParse(strTag[1], out temp)) {
                                    sound[0].Instrument = temp;
                                }
                                break;
                            case "##instrument1":
                                if (byte.TryParse(strTag[1], out temp)) {
                                    sound[1].Instrument = temp;
                                }
                                break;
                            case "##instrument2":
                                if (byte.TryParse(strTag[1], out temp)) {
                                    sound[2].Instrument = temp;
                                }
                                break;
                            case "##visible0":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    sound[0].Visible = bVal;
                                }
                                break;
                            case "##visible1":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    sound[1].Visible = bVal;
                                }
                                break;
                            case "##visible2":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    sound[2].Visible = bVal;
                                }
                                break;
                            case "##visible3":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    sound[3].Visible = bVal;
                                }
                                break;
                            case "##muted0":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    sound[0].Muted = bVal;
                                }
                                break;
                            case "##muted1":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    sound[1].Muted = bVal;
                                }
                                break;
                            case "##muted2":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    sound[2].Muted = bVal;
                                }
                                break;
                            case "##muted3":
                                if (bool.TryParse(strTag[1], out bVal)) {
                                    sound[3].Muted = bVal;
                                }
                                break;
                            case "##tpqn":
                                int mTPQN = strTag[1].IntVal() / 4 * 4;
                                if (mTPQN < 4) {
                                    mTPQN = 4;
                                }
                                else if (mTPQN > 64) {
                                    mTPQN = 64;
                                }
                                sound.TPQN = mTPQN;
                                break;
                            case "##key":
                                int mKey = strTag[1].IntVal();
                                if (mKey < -7) {
                                    mKey = -7;
                                }
                                else if (mKey > 7) {
                                    mKey = 7;
                                }
                                sound.Key = mKey;
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
                        lngDur = strTag[3].IntVal();
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
                        lngDur = strTag[2].IntVal();
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
                        sound[lngTrack].Notes.Add(intFreq, lngDur, bytVol);
                    }
                }


                if (blnError) {
                    sound.Clear();
                    sound.Description = "";
                    WinAGIException wex = new(LoadResString(681)) {
                        HResult = WINAGI_ERR + 681
                    };
                    throw wex;
                }
            }
        }


        // --- IT2AGI ---
        public static void IT2AGI(
            string filename, Sound sound,
            int[] channels = null,
            bool tempoExact = false,
            int autoDrumOffs = 0,
            Dictionary<int, int> instrNote = null,
            Dictionary<int, int> instrShift = null) {
            if (channels == null) channels = new int[] { 1, 2, 3, 4 };
            int NUMCH = channels.Length;
            if (NUMCH > 4) throw new ArgumentException("IT2AGI only supports up to 4 channels.");

            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            // IT Header
            var header = br.ReadBytes(4);
            if (Encoding.ASCII.GetString(header) != "IMPM")
                throw new InvalidDataException("Invalid IT header in file!");

            br.ReadBytes(26); // song name
            br.ReadBytes(2);
            var buf15 = br.ReadBytes(15);
            ushort ordnum = BitConverter.ToUInt16(buf15, 0);
            ushort insnum = BitConverter.ToUInt16(buf15, 2);
            ushort smpnum = BitConverter.ToUInt16(buf15, 4);
            ushort patnum = BitConverter.ToUInt16(buf15, 6);

            br.ReadByte(); // special
            var buf16 = br.ReadBytes(16);
            byte ispeed = buf16[2];
            byte itempo = buf16[3];

            br.ReadBytes(64); // chnlpan
            br.ReadBytes(64); // chnlvol
            var orders = br.ReadBytes(ordnum);
            br.ReadBytes(insnum * 4 + smpnum * 4);
            var patOffsets = new int[patnum];
            for (int i = 0; i < patnum; i++)
                patOffsets[i] = br.ReadInt32();

            // --- Pass 1: Read pattern data ---
            var pattern = new List<Dictionary<int, NoteEvent>>();
            byte[] pmvar = new byte[64];
            NoteEvent[] lastval = new NoteEvent[64];
            int totalRows = 0;
            for (int i = 0; i < 64; i++) {
                lastval[i] = new NoteEvent();
            }
            for (int order = 0; order < ordnum - 1; order++) {
                int patIdx = orders[order];
                if (patIdx >= patnum) continue;
                int offset = patOffsets[patIdx];
                if (offset == 0) continue;
                fs.Seek(offset, SeekOrigin.Begin);
                ushort patlen = br.ReadUInt16();
                ushort rows = br.ReadUInt16();

                br.ReadUInt32(); // skip 4 bytes

                for (int row = 0; row < rows; row++) {
                    var rowEvents = new Dictionary<int, NoteEvent>();
                    while (true) {
                        byte cvar = br.ReadByte();
                        if (cvar == 0) break;
                        int channel = (cvar - 1) & 63;
                        byte mvar;
                        if ((cvar & 128) != 0) {
                            mvar = br.ReadByte();
                            pmvar[channel] = mvar;
                        }
                        else {
                            mvar = pmvar[channel];
                        }
                        var noteEvent = new NoteEvent();
                        if ((mvar & 16) == 16) {
                            noteEvent.Note = lastval[channel].Note;
                        }
                        if ((mvar & 32) == 32) {
                            noteEvent.Instrument = lastval[channel].Instrument;
                        }
                        if ((mvar & 64) == 64) {
                            noteEvent.VolPan = lastval[channel].VolPan;
                        }
                        if ((mvar & 128) == 128) {
                            noteEvent.Command = lastval[channel].Command;
                            noteEvent.Param = lastval[channel].Param;
                        }

                        if ((mvar & 1) != 0) {
                            lastval[channel].Note = noteEvent.Note = br.ReadByte();
                        }
                        if ((mvar & 2) != 0) {
                            lastval[channel].Instrument = noteEvent.Instrument = br.ReadByte();
                        }
                        if ((mvar & 4) != 0) {
                            lastval[channel].VolPan = noteEvent.VolPan = br.ReadByte();
                        }
                        if ((mvar & 8) != 0) {
                            lastval[channel].Command = noteEvent.Command = br.ReadByte();
                            lastval[channel].Param = noteEvent.Param = br.ReadByte();
                        }
                        rowEvents[channel] = noteEvent;
                    }
                    pattern.Add(rowEvents);
                }
            }
            // remove any empty patterns from the end
            for (int i = pattern.Count - 1; i >= 0; i--) {
                if (pattern[i].Count == 0) {
                    pattern.RemoveAt(i);
                }
                else {
                    break;
                }
            }
            totalRows = pattern.Count;

            // --- Pass 2: Find note lengths and timing ---
            double rowdur_ms = (2500.0 / itempo) * ispeed;
            double rowdur_agi;
            int mul;
            if (!tempoExact) {
                // tempo is even
                rowdur_agi = 1000.0 / 60.0;
                mul = (int)Math.Round(rowdur_ms / rowdur_agi);
                if (mul < 1) mul = 1;
                rowdur_ms = mul * rowdur_agi; 
            }
            var tunedata = new List<TuneNote>[NUMCH];
            for (int outchan = 0; outchan < NUMCH; outchan++) {
                tunedata[outchan] = new List<TuneNote>();
                int channel = channels[outchan] - 1;
                for (int row = 0; row < totalRows; row++) {
                    if (pattern[row].TryGetValue(channel, out var evt) && evt.Note > 0 && evt.Note < 120) {
                        int notelen = totalRows - row;
                        for (int srchrow = row + 1; srchrow < totalRows; srchrow++) {
                            //if (pattern[srchrow].TryGetValue(channel, out var nextEvt) && nextEvt.Note > 0 && nextEvt.Note < 120) {
                            if (pattern[srchrow].TryGetValue(channel, out var nextEvt) && nextEvt.Note > 0) {
                            notelen = srchrow - row;
                                break;
                            }
                        }
                        int note = evt.Note;
                        int vol = evt.VolPan;
                        int instr = evt.Instrument;
                        if (instrShift != null && instrShift.TryGetValue(instr, out int shift)) {
                            note += shift;
                        }
                        if (instrNote != null && instrNote.TryGetValue(instr, out int forcedNote)) {
                            note = forcedNote;
                        }
                        double start = row * rowdur_ms;
                        double length = notelen * rowdur_ms;
                        double drumticks = autoDrumOffs * AGI_TICK;
                        if (outchan == 3 && autoDrumOffs > 0 && length > drumticks + 1) {
                            tunedata[outchan].Add(new TuneNote { Note = note, Vol = vol, Start = start, Length = drumticks });
                            tunedata[outchan].Add(new TuneNote { Note = 0, Vol = 0, Start = start + drumticks, Length = length - drumticks });
                        }
                        else {
                            tunedata[outchan].Add(new TuneNote { Note = note, Vol = vol, Start = start, Length = length });
                        }
                    }
                }
            }
            AGIFromTuneData(sound, tunedata, NUMCH, "IT");
        }

        // --- MOD2AGI ---
        public static void MOD2AGI(
            string filename, Sound sound,
            int[] channels = null,
            bool tempoExact = false,
            int autoDrumOffs = 0,
            Dictionary<int, int> instrNote = null,
            Dictionary<int, int> instrShift = null) {
            if (channels == null) channels = new int[] { 1, 2, 3, 4 };
            int NUMCH = channels.Length;
            if (NUMCH > 4) throw new ArgumentException("MOD2AGI only supports up to 4 channels.");

            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            // MOD Header
            fs.Seek(0x438, SeekOrigin.Begin);
            var mk = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (mk != "M.K.") throw new InvalidDataException("Not a standard 4-channel MOD file.");
            fs.Seek(0, SeekOrigin.Begin);
            br.ReadBytes(20); // song title

            // Skip sample info
            for (int sn = 0; sn < 31; sn++) br.ReadBytes(30);
            int songlen = br.ReadByte();
            br.ReadByte(); // unused
            var songpats = br.ReadBytes(128);
            br.ReadBytes(4); // "M.K."

            int patdataoffset = 1084;
            int patdatalen = 1024;
            int modpatlen = 64;
            int modchannels = 4;
            int arow = 0;

            int[] periods = { 999, 856, 808, 762, 720, 678, 640, 604, 570, 538, 508, 480, 453, 428, 404, 381, 360, 339, 320, 302, 285, 269, 254, 240, 226, 214, 202, 190, 180, 170, 160, 151, 143, 135, 127, 120, 113 };
            var periodtonote = new Dictionary<int, int>();
            for (int i = 0; i < periods.Length; i++) periodtonote[periods[i]] = 48 + i;

            var pattern = new List<Dictionary<int, NoteEvent>>();
            for (int pos = 0; pos < songlen; pos++) {
                int pat = songpats[pos];
                fs.Seek(patdataoffset + patdatalen * pat, SeekOrigin.Begin);
                for (int row = 0; row < modpatlen; row++) {
                    var rowEvents = new Dictionary<int, NoteEvent>();
                    for (int chan = 0; chan < modchannels; chan++) {
                        var b = br.ReadBytes(4);
                        int samplenum = (b[0] & 0xF0) | (b[2] >> 4);
                        int period = ((b[0] & 0x0F) << 8) | b[1];
                        int command = b[2] & 0x0F;
                        int args = b[3];
                        int note = periodtonote.TryGetValue(period, out int n) ? n : -1;
                        if (note > 0 && instrShift != null && instrShift.TryGetValue(samplenum, out int shift)) note += shift;
                        if (note > 0 && instrNote != null && instrNote.TryGetValue(samplenum, out int forcedNote)) note = forcedNote;
                        var noteEvent = new NoteEvent { Note = note };
                        if (command == 0x0C) noteEvent.VolPan = args;
                        rowEvents[chan] = noteEvent;
                    }
                    pattern.Add(rowEvents);
                }
                arow += modpatlen;
            }
            int totalRows = pattern.Count;

            // MOD timing: fixed, typically 6 ticks per row at 125 BPM
            double it = 120, ispeed = 6;
            double rowdur_ms = (2500.0 / it) * ispeed;
            double rowdur_agi = 1000.0 / 60.0;
            int mul = (int)Math.Round(rowdur_ms / rowdur_agi);
            if (mul < 1) mul = 1;
            if (!tempoExact) rowdur_ms = mul * rowdur_agi;

            var tunedata = new List<TuneNote>[NUMCH];
            for (int outchan = 0; outchan < NUMCH; outchan++) {
                tunedata[outchan] = new List<TuneNote>();
                int channel = channels[outchan] - 1;
                for (int row = 0; row < totalRows; row++) {
                    if (pattern[row].TryGetValue(channel, out var evt) && evt.Note > 0 && evt.Note < 120) {
                        int notelen = totalRows - row;
                        for (int srchrow = row + 1; srchrow < totalRows; srchrow++) {
                            if (pattern[srchrow].TryGetValue(channel, out var nextEvt) && nextEvt.Note > 0 && nextEvt.Note < 120) {
                                notelen = srchrow - row;
                                break;
                            }
                        }
                        int note = evt.Note;
                        int vol = evt.VolPan;
                        double start = row * rowdur_ms;
                        double length = notelen * rowdur_ms;
                        double drumticks = autoDrumOffs * AGI_TICK;
                        if (outchan == 3 && autoDrumOffs > 0 && length > drumticks + 1) {
                            tunedata[outchan].Add(new TuneNote { Note = note, Vol = vol, Start = start, Length = drumticks });
                            tunedata[outchan].Add(new TuneNote { Note = 0, Vol = 0, Start = start + drumticks, Length = length - drumticks });
                        }
                        else {
                            tunedata[outchan].Add(new TuneNote { Note = note, Vol = vol, Start = start, Length = length });
                        }
                    }
                }
            }
            AGIFromTuneData(sound, tunedata, NUMCH, "MOD");
        }

        // --- MIDI2AGI ---
        public static void MIDI2AGI(
            string filename, Sound sound,
            int[] channels = null,
            bool tempoExact = true,
            int autoDrumOffs = 0,
            Dictionary<int, int> instrNote = null,
            Dictionary<int, int> instrShift = null) {
            if (channels == null) channels = new int[] { 0, 1, 2, 9 };
            int NUMCH = channels.Length;
            if (NUMCH > 4) throw new ArgumentException("MIDI2AGI only supports up to 4 channels.");

            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);

            // MIDI Header
            string header = Encoding.ASCII.GetString(br.ReadBytes(4));
            if (header != "MThd") throw new InvalidDataException("Not a valid MIDI file.");
            int headerLen = ReadBigEndianInt32(br);
            int format = ReadBigEndianInt16(br);
            int ntrks = ReadBigEndianInt16(br);
            int division = ReadBigEndianInt16(br);

            // Default tempo and ticks per beat
            double miditempo = 500000; // microseconds per quarter note
            double miditicksbeat = division;
            var mididata = new List<TuneNote>[16];
            for (int i = 0; i < 16; i++) mididata[i] = new List<TuneNote>();

            for (int trk = 0; trk < ntrks; trk++) {
                var chunkType = Encoding.ASCII.GetString(br.ReadBytes(4));
                int length = ReadBigEndianInt32(br);
                var chunk = br.ReadBytes(length);
                int pos = 0;
                double totalTicks = 0, totalMs = 0;
                int lastStatus = 0;
                var lastNote = new Dictionary<int, TuneNote>();
                while (pos < chunk.Length) {
                    double delta = ReadVLQ(chunk, ref pos);
                    totalTicks += delta;
                    double deltaMs = delta / miditicksbeat * miditempo / 1000.0;
                    totalMs += deltaMs;
                    int status = chunk[pos++];
                    if ((status & 0x80) == 0) { status = lastStatus; pos--; }
                    int type = (status & 0xF0) >> 4;
                    int chan = status & 0x0F;
                    lastStatus = status;
                    if (type == 0x9) // Note on
                    {
                        int k = chunk[pos++];
                        int v = chunk[pos++];
                        if (v == 0) type = 0x8; // Note off
                        else {
                            if (lastNote.TryGetValue(chan, out var prev) && prev.Note != k) {
                                prev.Length = totalMs - prev.Start;
                                if (prev.Length > 0) mididata[chan].Add(prev);
                            }
                            lastNote[chan] = new TuneNote { Note = k, Vol = v >> 1, Start = totalMs };
                        }
                    }
                    if (type == 0x8) // Note off
                    {
                        int k = chunk[pos++];
                        int v = chunk[pos++];
                        if (lastNote.TryGetValue(chan, out var prev) && prev.Note == k) {
                            prev.Length = totalMs - prev.Start;
                            mididata[chan].Add(prev);
                            lastNote.Remove(chan);
                        }
                    }
                    if (type == 0xC) pos++; // Program change
                    if (type == 0xB || type == 0xA) pos += 2; // Control change, aftertouch
                    if (type == 0xE) pos += 2; // Pitch wheel
                    if (status == 0xFF) // Meta
                    {
                        int meta = chunk[pos++];
                        int len = (int)ReadVLQ(chunk, ref pos);
                        if (meta == 0x51 && len == 3) {
                            miditempo = (chunk[pos] << 16) | (chunk[pos + 1] << 8) | chunk[pos + 2];
                        }
                        pos += len;
                    }
                }
            }

            // Channel mapping and rest insertion
            var tunedata = new List<TuneNote>[NUMCH];
            for (int outchan = 0; outchan < NUMCH; outchan++) {
                int inchan = channels[outchan];
                var notes = new List<TuneNote>(mididata[inchan]);
                notes.Sort((a, b) => a.Start.CompareTo(b.Start));
                // Prevent overlaps
                for (int nn = 1; nn < notes.Count; nn++) {
                    if (notes[nn - 1].Start + notes[nn - 1].Length > notes[nn].Start)
                        notes[nn - 1].Length = notes[nn].Start - notes[nn - 1].Start;
                }
                tunedata[outchan] = notes;
            }

            AGIFromTuneData(sound, tunedata, NUMCH, "MIDI");
        }

        // --- AGI Conversion Core ---
        private static void AGIFromTuneData(Sound sound, List<TuneNote>[] tunedata, int NUMCH, string format) {
            // Insert rests
            var notedata = new List<TuneNote>[NUMCH];
            for (int ch = 0; ch < NUMCH; ch++) {
                notedata[ch] = new List<TuneNote>();
                var tdata = tunedata[ch];
                for (int nn = 0; nn < tdata.Count; nn++) {
                    if (nn == 0) {
                        var first = tdata[0];
                        if (first.Start == 0)
                            notedata[ch].Add(first);
                        else {
                            notedata[ch].Add(new TuneNote { Note = -1, Length = first.Start });
                            notedata[ch].Add(first);
                        }
                    }
                    else {
                        var curr = tdata[nn];
                        var prev = tdata[nn - 1];
                        double gap = curr.Start - (prev.Start + prev.Length);
                        if (gap < AGI_TICK / 2)
                            notedata[ch].Add(curr);
                        else {
                            notedata[ch].Add(new TuneNote { Note = -1, Length = gap });
                            notedata[ch].Add(curr);
                        }
                    }
                }
            }

            // AGI Sound conversion
            // always clear the sound first
            sound.Clear();
            for (int ch = 0; ch < NUMCH; ch++) {
                var track = sound.Tracks[ch];
                double prevDurFrac = 0;
                foreach (var nd in notedata[ch]) {
                    int note = nd.Note;
                    double length = nd.Length;
                    int vol = nd.Vol;
                    if (vol < 0 || vol > 63) vol = 63;
                    double duration_f = length / AGI_TICK;
                    int out_duration = (int)(duration_f + prevDurFrac + 0.5);
                    if (out_duration < 1) out_duration = 1;
                    prevDurFrac = duration_f - out_duration;
                    int freqdiv = 0, att = 0;
                    if (ch <= 2) {
                        int n = note;
                        while (n >= 0 && n < 45) n += 12;
                        double freq = (note == -1) ? 0 : 440.0 * Math.Exp((n - 69) * Math.Log(2.0) / 12.0);
                        freqdiv = (freq == 0) ? 0 : (int)(111860.0 / freq);
                    }
                    else {
                        // Noise channel, drum mapping for MIDI
                        int n = note;
                        if (format == "MIDI") {
                            var DRUMNOTES = new Dictionary<int, int> { [35] = 16, [36] = 16, [37] = 14, [38] = 15, [39] = 15, [40] = 15, [41] = 14, [42] = 14, [999] = 16 };
                            if (DRUMNOTES.TryGetValue(n, out int drumNote)) n = drumNote; else n = DRUMNOTES[999];
                        }
                        int out_noisetype = (n == -1) ? 0 : (n / 12) % 2;
                        int out_noisefreq = (n == -1) ? 0 : n % 4;
                        freqdiv = out_noisefreq + out_noisetype << 1;
                    }
                    if (note == -1) {
                        att = 15;
                    }
                    else {
                        att = 15 - (vol >> 2);
                    }

                    track.Notes.Add(freqdiv, out_duration, (byte)att);
                }
            }
        }

        // --- Helper classes and methods ---
        private class NoteEvent {
            public int Note = 0;
            public int Instrument = 0;
            public int VolPan = 63;
            public int Command = 0;
            public int Param = 0;
        }
        private class TuneNote {
            public int Note = -1;
            public int Vol = 63;
            public double Start = 0;
            public double Length = 0;
        }
        private static int ReadBigEndianInt16(BinaryReader br) {
            var bytes = br.ReadBytes(2);
            return (bytes[0] << 8) | bytes[1];
        }
        private static int ReadBigEndianInt32(BinaryReader br) {
            var bytes = br.ReadBytes(4);
            return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        }
        private static long ReadVLQ(byte[] data, ref int pos) {
            long value = 0;
            byte b;
            do {
                b = data[pos++];
                value = (value << 7) | (b & 0x7F);
            } while ((b & 0x80) != 0);
            return value;
        }
    }
}