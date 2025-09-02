using System;
using System.Collections.Generic;
using WinAGI.Engine;

namespace WinAGI.Editor {
    [Serializable]
    internal class SoundClipboardData {
        public SoundClipboardData(SoundCBTrackType type) {
            Type = type;
        }
        public SoundCBTrackType Type { get; set; }
        public List<CBNote> Notes { get; set; } = [];
        [Serializable]
        public struct CBNote {
            public CBNote(int freqdivisor, int duration, byte attenuation) {
                FreqDivisor = freqdivisor;
                Duration = duration;
                Attenuation = attenuation;
            }
            public CBNote(Note note) {
                FreqDivisor = note.FreqDivisor;
                Duration = note.Duration;
                Attenuation = note.Attenuation;
            }
            public int FreqDivisor { get; }
            public int Duration { get; }
            public byte Attenuation { get; }
        }
    }
    public enum  SoundCBTrackType {
        Tone,
        Noise
    }
}
