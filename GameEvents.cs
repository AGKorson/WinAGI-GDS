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
        public class CompileGameEventArgs(GameCompileStatus status, WinAGIEventInfo errInfo) {
            public GameCompileStatus CStatus { get; } = status;
            public bool Cancel { get; set; } = false;
            public WinAGIEventInfo CompileInfo { get; } = errInfo;
        }

        /// <summary>
        /// Provides data for the NewGameStatus event.
        /// </summary>
        /// <param name="newinfo"></param>
        public class NewGameEventArgs(WinAGIEventInfo newinfo) {
            public WinAGIEventInfo NewInfo { get; } = newinfo;
        }

        /// <summary>
        /// Provides data for the LoadGameStatus event.
        /// </summary>
        /// <param name="loadinfo"></param>
        public class LoadGameEventArgs(WinAGIEventInfo loadinfo) {
            public WinAGIEventInfo LoadInfo { get; } = loadinfo;
        }

        /// <summary>
        /// Provides data for the CompileLogicStatus event.
        /// </summary>
        /// <param name="compInfo"></param>
        public class CompileLogicEventArgs(WinAGIEventInfo compInfo) {
            public WinAGIEventInfo CompInfo { get; } = compInfo;
        }

        /// <summary>
        /// Provides data for the DecodeLogicStatus event.
        /// </summary>
        /// <param name="decodeInfo"></param>
        public class DecodeLogicEventArgs(WinAGIEventInfo decodeInfo) {
            public WinAGIEventInfo DecodeInfo { get; } = decodeInfo;
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
        public static event CompileGameEventHandler CompileGameStatus;

        /// <summary>
        /// Raises the CompileGameStatus event. If user cancels in response, returns
        /// a value of true.
        /// </summary>
        /// <param name="cStatus"></param>
        /// <param name="ResType"></param>
        /// <param name="ResNum"></param>
        /// <param name="CompileInfo"></param>
        /// <returns>true means canceled</returns>
        internal static bool CheckForCancel(GameCompileStatus cStatus, WinAGIEventInfo CompileInfo) {
            CompileGameEventArgs e = new(cStatus, CompileInfo);
            if (Editor.Base.ConsoleMode) {
                // in console mode, send the info to the console handler instead of raising event
                Editor.WAGConsole.CompileGameStatus(e);
                return false;
            }
            // Raise the event in a thread-safe manner using the ?. operator.
            CompileGameStatus?.Invoke(null, e);
            // pass back cancel value
            return e.Cancel;
        }

        internal static void OnCompileGameStatus(GameCompileStatus cStatus, WinAGIEventInfo CompileInfo) {
            CompileGameEventArgs e = new(cStatus, CompileInfo);
            if (Editor.Base.ConsoleMode) {
                // in console mode, send the info to the console handler instead of raising event
                Editor.WAGConsole.CompileGameStatus(e);
                return;
            }
            // Raise the event in a thread-safe manner using the ?. operator.
            CompileGameStatus?.Invoke(null, e);
        }

        /// <summary>
        /// NewGameStatus event handler delegate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void NewGameEventHandler(object sender, NewGameEventArgs e);

        /// <summary>
        /// This event is raised at various points when a game is being created to provide
        /// feedback to the calling program.
        /// </summary>
        public static event NewGameEventHandler NewGameStatus;

        /// <summary>
        /// Raises the NewGameStatus event.
        /// </summary>
        /// <param name="NewInfo"></param>
        internal static void OnNewGameStatus(WinAGIEventInfo NewInfo) {
            if (Editor.Base.ConsoleMode) {
                // in console mode, send the info to the console handler instead of raising event
                Editor.WAGConsole.NewGameStatus(NewInfo);
                return;
            }
            // Raise the event in a thread-safe manner using the ?. operator.
            NewGameStatus?.Invoke(null, new NewGameEventArgs(NewInfo));
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
        public static event LoadGameEventHandler LoadGameStatus;

        /// <summary>
        /// Raises the LoadGameStatus event.
        /// </summary>
        /// <param name="LoadInfo"></param>
        internal static void OnLoadGameStatus(WinAGIEventInfo LoadInfo) {
            if (Editor.Base.ConsoleMode) {
                // in console mode, send the info to the console handler instead of raising event
                Editor.WAGConsole.LoadGameStatus(LoadInfo);
                return;
            }
            // Raise the event in a thread-safe manner using the ?. operator.
            LoadGameStatus?.Invoke(null, new LoadGameEventArgs(LoadInfo));
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
        public static event CompileLogicEventHandler CompileLogicStatus;

        /// <summary>
        /// Raises the CompileLogicStatus event. 
        /// </summary>
        /// <param name="CompInfo"></param>
        internal static bool OnCompileLogicStatus(AGIGame game, WinAGIEventInfo CompInfo) {
            GameCompileStatus stat;
            if (Editor.Base.ConsoleMode) {
                // in console mode, send the info to the console handler instead of raising event
                Editor.WAGConsole.CompileLogicStatus(CompInfo);
                return true;
            }
            if (CompInfo.Type == EventType.LogicCompileError) {
                stat = GameCompileStatus.LogicError;
            }
            else {
                stat = GameCompileStatus.Warning;
            }
            if (game is null || !game.Compiling) {
                // not compiling a game, must be compiling a single logic
                // Raise the event in a thread-safe manner using the ?. operator.
                CompileLogicStatus?.Invoke(null, new CompileLogicEventArgs(CompInfo));
                return true;
            }
            // if compiling a game, use that event
            else {
                // Raise the event in a thread-safe manner using the ?. operator.
                CompileGameEventArgs e = new(stat, CompInfo);
                CompileGameStatus?.Invoke(null, e);
                return e.Cancel;
            }
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
        public static event DecodeLogicEventHandler DecodeLogicStatus;

        /// <summary>
        /// Raises the DecodeLogicStatus event. 
        /// </summary>
        /// <param name="DecodeInfo"></param>
        internal static void OnDecodeLogicStatus(WinAGIEventInfo DecodeInfo) {
            if (Editor.Base.ConsoleMode) {
                // in console mode, send the info to the console handler instead of raising event
                Editor.WAGConsole.DecodeLogicStatus(DecodeInfo);
                return;
            }
            // Raise the event in a thread-safe manner using the ?. operator
            DecodeLogicStatus?.Invoke(null, new DecodeLogicEventArgs(DecodeInfo));
        }
        #endregion
    }
}
