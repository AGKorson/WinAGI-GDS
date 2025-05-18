using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAGI.Engine;

namespace WinAGI.Editor {
    internal class ViewUndo {
        public enum ActionType {
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

        public ActionType UDAction;
        public View View;
        public Loop UndoLoop;
        public Cel UndoCel;
        public int UDLoopNo;
        public int UDCelNo;
        public string OldText;
        public int[] UndoData;
        public byte[,] CelData;
        public List<frmViewEdit.PixelInfo> PixelData;
        public frmViewEdit.SelectionInfo UDSelection;
        public ViewUndo() {
            // Initialize the arrays with a size of 0
            UndoData = [];
            CelData = new byte[0, 0];
        }
    }
}
