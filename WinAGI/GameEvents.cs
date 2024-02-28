using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace WinAGI.Engine
{
    public class CompileGameEventArgs
    {
        public CompileGameEventArgs(ECStatus status, AGIResType restype, byte num, TWinAGIEventInfo errInfo)
        {
            CStatus = status;
            ResType = restype;
            ResNum = num;
            ErrorInfo = errInfo;
        }
        public ECStatus CStatus { get; }
        public AGIResType ResType { get; }
        public byte ResNum { get; }
        public TWinAGIEventInfo ErrorInfo { get; }
    }

    public class LoadGameEventArgs
    {
        public LoadGameEventArgs(TWinAGIEventInfo loadinfo)
        {
            LoadInfo = loadinfo;
        }
        public TWinAGIEventInfo LoadInfo { get; }
    }

    public class CompileLogicEventArgs
    {
        public CompileLogicEventArgs(TWinAGIEventInfo compInfo)
        {
            Info = compInfo;
        }
        public TWinAGIEventInfo Info { get; }
    }

    // 
    public partial class AGIGame
    {
        // Declare the delegate.
        internal delegate void CompileGameEventHandler(object sender, CompileGameEventArgs e);
        // Declare the event.
        internal static event CompileGameEventHandler CompileGameStatus;

        // Declare the delegate.
        internal delegate void LoadGameEventHandler(object sender, LoadGameEventArgs e);
        // Declare the event.
        internal static event LoadGameEventHandler LoadGameStatus;

        // Declare the delegate.
        internal delegate void CompileLogicEventHandler(object sender, CompileLogicEventArgs e);
        // Declare the event.
        internal static event CompileLogicEventHandler CompileLogicStatus;
        internal static void Raise_CompileGameEvent(ECStatus cStatus, AGIResType ResType, byte ResNum, TWinAGIEventInfo CompileInfo)
        {
            //// if error, assume cancel, but it can be overridden by event handler for Logic errors
            //if (cStatus == ECStatus.csLogicError || cStatus == ECStatus.csResError) {
            //    AGIGame.agCancelComp = true;
            //}
           // Raise the event in a thread-safe manner using the ?. operator.
            CompileGameStatus?.Invoke(null, new CompileGameEventArgs(cStatus, ResType, ResNum, CompileInfo));
        }
        internal static void Raise_LoadGameEvent(TWinAGIEventInfo LoadInfo)
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            LoadGameStatus?.Invoke(null, new LoadGameEventArgs(LoadInfo));   
        }
        internal static void Raise_CompileLogicEvent(TWinAGIEventInfo CompInfo)
        {
            // Raise the event in a thread-safe manner using the ?. operator.
            CompileLogicStatus?.Invoke(null, new CompileLogicEventArgs(CompInfo));
        }
    }
}