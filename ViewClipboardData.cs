using System;
using WinAGI.Engine;

namespace WinAGI.Editor {
    [Serializable]
    internal class ViewClipboardData {
        public ViewClipboardData(ViewClipboardMode mode) {
            Mode = mode;
            if (mode == ViewClipboardMode.Loop) {
                Loop = new Loop();
            }
            else {
                Cel = new Cel();
            }
        }
        public ViewClipboardMode Mode { get; set; }
        public Loop Loop { get; set; }
        public Cel Cel { get; set; }
    }

    public enum ViewClipboardMode {
        Loop,
        Cel
    }
}
