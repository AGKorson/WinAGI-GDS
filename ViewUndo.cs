using System.Collections.Generic;
using WinAGI.Engine;

namespace WinAGI.Editor {
    internal class ViewUndo {
        public enum ViewUndoAction {
            AddLoop,
            AddCel,
            DelLoop,
            DelCel,
            ChangeVDesc,
            IncHeight,
            IncWidth,
            DecHeight,
            DecWidth,
            FlipLoopH,
            FlipLoopV,
            FlipCelH,
            FlipCelV,
            FlipSelectionH,
            FlipSelectionV,
            Line,
            Box,
            BoxFill,
            Draw,
            Erase,
            PaintFill,
            Mirror,
            ChangeTransCol,
            PasteLoop,
            PasteCel,
            CutSelection,
            DelSelection,
            PasteSelection,
            MoveSelection,
            ClearView,
            ClearLoop,
            ClearCel,
            CutLoop,
            CutCel,
        }

        public ViewUndoAction Action;
        public View View;
        public Loop UndoLoop;
        public Cel UndoCel;
        public int LoopNumber;
        public int CelNumber;
        public string OldText;
        public int[] UndoData;
        public byte[,] CelData;
        public List<frmViewEdit.PixelInfo> PixelData;
        public frmViewEdit.SelectionInfo SelectionInfo;
        public ViewUndo() {
            // Initialize the arrays with a size of 0
            UndoData = [];
            CelData = new byte[0, 0];
        }
    }
}
