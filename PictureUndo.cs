using System;
using System.Drawing;
using WinAGI.Engine;
using static WinAGI.Editor.frmPicEdit;

namespace WinAGI.Editor {
    [Serializable]
    public class PictureUndo {
        public ActionType Action = (ActionType)(-1);
        public DrawFunction DrawCommand = (DrawFunction)(-1);
        public string Text = "";
        public int PicPos = -1;
        public int CmdIndex = -1;
        public int CoordIndex = -1;
        public int CmdCount = -1;
        public int ByteCount = -1;
        public PlotStyle PenStyle = PlotStyle.Solid;
        private byte[] mUDData = [];
        
        public PictureUndo() {

        }

        public byte[] Data {
            get {
                return mUDData;
            }
            set {
                mUDData = value;
            }
        }

        public Point Coord { get; internal set; }

        public enum ActionType {
            ChangeColor,
            ChangePlotPen,
            DelCmd,
            DelCoord,
            AddCmd,
            AddCoord,
            InsertCoord,
            MoveCoord,
            Rectangle,
            Trapezoid,
            Ellipse,
            JoinCmds,
            SplitCmd,
            CutCmds,
            PasteCmds,
            MoveCmds,
            FlipH,
            FlipV,
            AddPlotPattern,
            DelPlotPattern,
            EditCoord,
            SetPriBase,
            Clear,
        }
    }
}
