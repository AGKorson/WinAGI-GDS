namespace WinAGI.Editor {
    public class PictureUndo {
        public ActionType UDAction;
        public string UDCmd;
        public string UDText;
        public int UDPicPos;
        public int UDCmdIndex;
        public int UDCoordIndex;
        private byte[] mUDData;
        
        public PictureUndo() {
            mUDData = [];
        }

        public byte[] UDData {
            get {
                return mUDData;
            }
            set {
                mUDData = value;
            }
        }

        public enum ActionType {
            ChangeColor,
            ChangePlotPen,
            DelCmd,
            DelCoord,
            AddCmd,
            AddCoord,
            InsertCoord,
            EditCoord,
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
            EditPlotCoord,
            SetPriBase,
        }
    }
}
