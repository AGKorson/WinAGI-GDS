using System;
using WinAGI.Engine;

namespace WinAGI.Editor {
    [Serializable]
    public class PictureClipboardData {
        private byte[] mData = [];
        public byte[] Data {
            get {
                return mData;
            }
            set {
                mData = value;
            }
        }
        public int CmdCount { get; internal set; }
        public bool IncludeVisPen { get; internal set; }
        public bool IncludePriPen { get; internal set; }
        public bool IncludePlotPen { get; internal set; }
        public bool HasPlotCmds { get; internal set; }
        public bool HasPenChange { get; internal set; }
        public PenStatus StartPen { get; internal set; }
        public PenStatus EndPen { get; internal set; }
        public PenStatus DrawCmdStartPen { get; internal set; }
        public PenStatus DrawCmdEndPen { get; internal set; }
        public int DrawCmdStart { get; internal set; }
        public int DrawCmdCount { get; internal set; }
        public int DrawByteStart { get; internal set; }
        public int DrawByteCount { get; internal set; }
    }
}
