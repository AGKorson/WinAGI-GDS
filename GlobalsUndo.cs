using WinAGI.Engine;

namespace WinAGI.Editor {
    // not much error checking; assumption
    // is that calling programs know what
    // they are doing

    // MAKE SURE  to set count property
    // BEFORE saving defines or positions

    internal class GlobalsUndo {
        public enum udgActionType {
            udgAddDefine,
            udgImportDefines,
            udgPasteDefines,
            udgDeleteDefine,
            udgCutDefine,
            udgClearList,
            udgEditName,
            udgEditValue,
            udgEditComment,
        }

        public udgActionType UDAction;
        private TDefine[] mUDDefine = [];
        private int mUDCount = 0;
        public int UDPos = 0;
        public string UDText = "";

        public int UDCount {
            get {
                return mUDCount;
            }
            set {
                mUDCount = value;
                mUDDefine = new TDefine[mUDCount];
            }
        }

        public TDefine[] UDDefine {
            get => mUDDefine;
        }
    }
}
