using System.Collections.Generic;
using WinAGI.Engine;

namespace WinAGI.Editor {
    // not much error checking; assumption
    // is that calling programs know what
    // they are doing

    // MAKE SURE  to set count property
    // BEFORE saving defines or positions

    internal class GlobalsUndo {
        public enum GlobalUndoAction {
            AddDefine,
            ImportDefines,
            PasteDefines,
            DeleteDefine,
            CutDefine,
            ClearList,
            EditName,
            EditValue,
            EditComment,
            MoveRows,
            SortList,
        }

        public GlobalUndoAction Action;
        private Define[] mDefine = [];
        private int mCount = 0;
        public int Pos = 0;
        public int Start = 0;
        public int End = 0;
        public string Text = "";
        public List<int> SortOrder = null;

        public int Count {
            get {
                return mCount;
            }
            set {
                mCount = value;
                mDefine = new Define[mCount];
            }
        }

        public Define[] UDDefine {
            get => mDefine;
        }
    }
}
