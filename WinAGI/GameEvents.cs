namespace WinAGI.Engine {
    public partial class AGIGame {
        #region Classes
        /// <summary>
        /// Provides data for the CompileGameSatus event.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="restype"></param>
        /// <param name="num"></param>
        /// <param name="errInfo"></param>
        public class CompileGameEventArgs(ECStatus status, TWinAGIEventInfo errInfo) {
            public ECStatus CStatus { get; } = status;
            public bool Cancel { get; set; } = false;
            public TWinAGIEventInfo ErrorInfo { get; } = errInfo;
        }

        /// <summary>
        /// Provides data for the LoadGameStatus event.
        /// </summary>
        /// <param name="loadinfo"></param>
        public class LoadGameEventArgs(TWinAGIEventInfo loadinfo) {
            public TWinAGIEventInfo LoadInfo { get; } = loadinfo;
        }

        /// <summary>
        /// Provides data for the CompileLogicStatus event.
        /// </summary>
        /// <param name="compInfo"></param>
        public class CompileLogicEventArgs(TWinAGIEventInfo compInfo) {
            public TWinAGIEventInfo CompInfo { get; } = compInfo;
        }

        /// <summary>
        /// Provides data for the DecodeLogicStatus event.
        /// </summary>
        /// <param name="decodeInfo"></param>
        public class DecodeLogicEventArgs(TWinAGIEventInfo decodeInfo) {
            public TWinAGIEventInfo DecodeInfo { get; } = decodeInfo;
        }
        #endregion

        #region Events
        /// <summary>
        /// CompileGameStatus event handler delegate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CompileGameEventHandler(object sender, CompileGameEventArgs e);

        /// <summary>
        /// This event is raised at various points when a game is compiled to provide
        /// feedback to the calling program. It also provides the caller an opportunity
        /// to cancel the compile operation.
        /// </summary>
        public event CompileGameEventHandler CompileGameStatus;

        /// <summary>
        /// Raises the CompileGameStatus event. If user cancels in response, returns
        /// a value of true.
        /// </summary>
        /// <param name="cStatus"></param>
        /// <param name="ResType"></param>
        /// <param name="ResNum"></param>
        /// <param name="CompileInfo"></param>
        /// <returns></returns>
        internal bool OnCompileGameStatus(ECStatus cStatus, TWinAGIEventInfo CompileInfo) {
            // Raise the event in a thread-safe manner using the ?. operator.
            CompileGameEventArgs e = new(cStatus, CompileInfo);
            this.CompileGameStatus?.Invoke(null, e);
            return e.Cancel;
        }

        /// <summary>
        /// LoadGameStatus event handler delegate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void LoadGameEventHandler(object sender, LoadGameEventArgs e);

        /// <summary>
        /// This event is raised at various points when a game is loaded to provide
        /// feedback to the calling program.
        /// </summary>
        public event LoadGameEventHandler LoadGameStatus;

        /// <summary>
        /// Raises the LoadGameStatus event.
        /// </summary>
        /// <param name="LoadInfo"></param>
        internal void OnLoadGameStatus(TWinAGIEventInfo LoadInfo) {
            // Raise the event in a thread-safe manner using the ?. operator.
            this.LoadGameStatus?.Invoke(null, new LoadGameEventArgs(LoadInfo));
        }

        /// <summary>
        /// CompileLogicStatus event handler delegate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void CompileLogicEventHandler(object sender, CompileLogicEventArgs e);

        /// <summary>
        /// This event is raised at various points when a logic resource is 
        /// compiled to provide feedback to the calling program.
        /// </summary>
        public event CompileLogicEventHandler CompileLogicStatus;

        /// <summary>
        /// Raises the CompileLogicStatus event. 
        /// </summary>
        /// <param name="CompInfo"></param>
        internal void OnCompileLogicStatus(TWinAGIEventInfo CompInfo) {
            // Raise the event in a thread-safe manner using the ?. operator.
            CompileLogicStatus?.Invoke(null, new CompileLogicEventArgs(CompInfo));
        }

        /// <summary>
        /// DecodeLogicStatus event handler delegate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void DecodeLogicEventHandler(object sender, DecodeLogicEventArgs e);

        /// <summary>
        /// This event is raised at various points when a logic resource is 
        /// decoded to provide feedback to the calling program.
        /// </summary>
        public event DecodeLogicEventHandler DecodeLogicStatus;

        /// <summary>
        /// Raises the DecodeLogicStatus event. 
        /// </summary>
        /// <param name="DecodeInfo"></param>
        internal void OnDecodeLogicStatus(TWinAGIEventInfo DecodeInfo) {
            // Raise the event in a thread-safe manner using the ?. operator
            DecodeLogicStatus?.Invoke(null, new DecodeLogicEventArgs(DecodeInfo));
        }
        #endregion
    }
}
