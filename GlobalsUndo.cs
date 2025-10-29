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
        }

        public GlobalUndoAction Action;
        private TDefine[] mDefine = [];
        private int mCount = 0;
        public int Pos = 0;
        public string Text = "";

        public int Count {
            get {
                return mCount;
            }
            set {
                mCount = value;
                mDefine = new TDefine[mCount];
            }
        }

        public TDefine[] UDDefine {
            get => mDefine;
        }
    }
}
