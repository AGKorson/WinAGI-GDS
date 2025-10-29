using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static WinAGI.Common.Base;
using static WinAGI.Engine.LogicDecoder;
using static WinAGI.Engine.ArgType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
using static WinAGI.Engine.LogicErrorLevel;
using System.Diagnostics;

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
        public class CompileGameEventArgs(GameCompileStatus status, TWinAGIEventInfo errInfo) {
            public GameCompileStatus CStatus { get; } = status;
            public bool Cancel { get; set; } = false;
            public TWinAGIEventInfo CompileInfo { get; } = errInfo;
        }

        /// <summary>
        /// Provides data for the NewGameStatus event.
        /// </summary>
        /// <param name="newinfo"></param>
        public class NewGameEventArgs(TWinAGIEventInfo newinfo) {
            public TWinAGIEventInfo NewInfo { get; } = newinfo;
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
        public static event CompileGameEventHandler CompileGameStatus;

        /// <summary>
        /// Raises the CompileGameStatus event. If user cancels in response, returns
        /// a value of true.
        /// </summary>
        /// <param name="cStatus"></param>
        /// <param name="ResType"></param>
        /// <param name="ResNum"></param>
        /// <param name="CompileInfo"></param>
        /// <returns></returns>
        internal static void OnCompileGameStatus(GameCompileStatus cStatus, TWinAGIEventInfo CompileInfo, ref bool Cancel) {
            // Raise the event in a thread-safe manner using the ?. operator.
            CompileGameEventArgs e = new(cStatus, CompileInfo);
            CompileGameStatus?.Invoke(null, e);
            // check for cancel
            if (e.Cancel) {
                Cancel = true;
            }
        }

        internal static void OnCompileGameStatus(GameCompileStatus cStatus, TWinAGIEventInfo CompileInfo) {
            // use a throwaway for cancel check
            bool _ = false;
            OnCompileGameStatus(cStatus, CompileInfo, ref _);
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
        internal static void OnNewGameStatus(TWinAGIEventInfo NewInfo) {
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
        internal static void OnLoadGameStatus(TWinAGIEventInfo LoadInfo) {
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
        internal static bool OnCompileLogicStatus(AGIGame game, TWinAGIEventInfo CompInfo) {
            GameCompileStatus stat;
            if (CompInfo.Type == EventType.LogicCompileError) {
                stat = GameCompileStatus.LogicError;
            }
            else {
                stat = GameCompileStatus.Warning;
            }
            if (game is null || !game.Compiling) {
                // not compiling a game, must be compiling a single logic
                // Raise the event in a thread-safe manner using the ?. operator.
                // TODO: do I need a status parameter here (like gamecompile)?
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
        internal static void OnDecodeLogicStatus(TWinAGIEventInfo DecodeInfo) {
            // Raise the event in a thread-safe manner using the ?. operator
            DecodeLogicStatus?.Invoke(null, new DecodeLogicEventArgs(DecodeInfo));
        }
        #endregion
    }
}
