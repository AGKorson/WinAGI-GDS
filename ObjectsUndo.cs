namespace WinAGI.Editor {
    public class ObjectsUndo {
        public ActionType UDAction;
        public byte UDObjectRoom; // also used for Max objects & encryption
        public int UDObjectNo;
        public string UDObjectText = "";
        
        public enum ActionType {
            AddItem,      // store object number that was added
            DeleteItem,   // store object number, text, and room that was deleted
            ModifyItem,   // store old object number, text
            ModifyRoom,   // store old object number, room
            ChangeMaxObj,   // store old maxobjects
            TglEncrypt,     // store old encryption Value
            Clear,          // store old Objects object
            Replace,    // store old object number, text
            ReplaceAll, // store all old numbers and text
        }

        public ObjectsUndo() {

        }
    }
}
