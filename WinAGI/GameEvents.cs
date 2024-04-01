namespace WinAGI.Engine {
    public class CompileGameEventArgs(ECStatus status, AGIResType restype, byte num, TWinAGIEventInfo errInfo) {
        public ECStatus CStatus { get; } = status;
        public AGIResType ResType { get; } = restype;
        public byte ResNum { get; } = num;
        public TWinAGIEventInfo ErrorInfo { get; } = errInfo;
    }

    public class LoadGameEventArgs(TWinAGIEventInfo loadinfo) {
        public TWinAGIEventInfo LoadInfo { get; } = loadinfo;
    }

    public class CompileLogicEventArgs(TWinAGIEventInfo compInfo) {
        public TWinAGIEventInfo CompInfo { get; } = compInfo;
    }

    public class DecodeLogicEventArgs(TWinAGIEventInfo decodeInfo) {
        public TWinAGIEventInfo DecodeInfo { get; } = decodeInfo;
    }

    public partial class AGIGame {
        // Declare the delegate.
        public delegate void CompileGameEventHandler(object sender, CompileGameEventArgs e);
        // Declare the event.
        public static event CompileGameEventHandler CompileGameStatus;
        // Declare access method to raise the event 
        internal static void Raise_CompileGameEvent(ECStatus cStatus, AGIResType ResType, byte ResNum, TWinAGIEventInfo CompileInfo) {
            // Raise the event in a thread-safe manner using the ?. operator.
            CompileGameStatus?.Invoke(null, new CompileGameEventArgs(cStatus, ResType, ResNum, CompileInfo));
        }

        // Declare the delegate.
        public delegate void LoadGameEventHandler(object sender, LoadGameEventArgs e);
        // Declare the event.
        public static event LoadGameEventHandler LoadGameStatus;
        // Declare access method to raise the event 
        internal static void Raise_LoadGameEvent(TWinAGIEventInfo LoadInfo) {
            // Raise the event in a thread-safe manner using the ?. operator.
            LoadGameStatus?.Invoke(null, new LoadGameEventArgs(LoadInfo));
        }

        // Declare the delegate.
        public delegate void CompileLogicEventHandler(object sender, CompileLogicEventArgs e);
        // Declare the event.
        public static event CompileLogicEventHandler CompileLogicStatus;
        // Declare access method to raise the event 
        internal static void Raise_CompileLogicEvent(TWinAGIEventInfo CompInfo) {
            // Raise the event in a thread-safe manner using the ?. operator.
            CompileLogicStatus?.Invoke(null, new CompileLogicEventArgs(CompInfo));
        }

        // Declare the delegate
        public delegate void DecodeLogicEventHandler(object sender, DecodeLogicEventArgs e);
        // Declare the event
        public static event DecodeLogicEventHandler DecodeLogicStatus;
        // Declare access method to raise the event 
        internal static void Raise_DecodeLogicEvent(TWinAGIEventInfo DecodeInfo) {
            // Raise the event in a thread-safe manner using the ?. operator
            DecodeLogicStatus?.Invoke(null, new DecodeLogicEventArgs(DecodeInfo));
        }
    }
}