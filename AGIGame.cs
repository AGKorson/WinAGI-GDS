using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.FanLogicCompiler;
using static WinAGI.Engine.LogicDecoder;

namespace WinAGI.Engine {
    /// <summary>
    /// This class represents an AGI game, including all methods,
    /// classes, properties, including WinAGI specific features, that
    /// are needed to open, view, edit and save all game resources
    /// in the WinAGI Game Development System.
    /// </summary>
    public partial class AGIGame {
        #region Enums
        public enum IncludeType {
            Other,
            Reserved,
            ResourceIDs,
            Globals,
            Sysdefs,
            Gamedefs
        }
        #endregion

        #region Structs
        /// <summary>
        /// Used to pass all needed parameters to the AGIGame constructor 
        /// to create or open games that do not have an existing WAG file.
        /// </summary>
        public struct GameParams {
            public OpenGameMode Mode;
            public string GameFile;
            public string ID;
            public AGIVersion Version;
            public string GameDir;
            public string SrcResDirName;
            public string SrcExt;
            public string TemplateDir;
            public bool IncludeIDs = false;
            public bool IncludeReserved = false;
            public bool IncludeGlobals = false;
            public bool SierraSyntax = false;
            public int CodePage;
            public bool Failed;
            public Exception Error;
            public int ErrorCode = 0;
            public bool Warnings;
            public GameParams() {

            }
        }

        /// <summary>
        /// Used by both the open game and new game functions to pass parameters
        /// to the AGIGame constructor and to report results back to the worker.
        /// </summary>
        public struct LoadGameResults {
            public GameParams Parameters;
            public string Source;
            public bool Failed;
            public Exception Error;
            public int ErrorCode;
            public bool Warnings;
        }

        public struct IncludeInfo {
            public string Filename = "";
            public IncludeType Type;

            public IncludeInfo() {
            }
        }
        #endregion

        #region Local Members
        internal VOLManager volManager;
        internal Logics agLogs;
        internal Sounds agSnds;
        internal Views agViews;
        internal Pictures agPics;
        internal InventoryList agInvObj;
        internal WordList agVocabWords;
        internal List<IncludeInfo> agIncludeFiles;
        internal DefinesList agGlobals;
        internal ReservedDefineList agReservedDefines;
        internal EGAColors agEGAcolors = new();
        internal string agGameDir = "";
        internal string agSrcResDir = "";
        internal string agSrcResDirName = "";
        internal string agGameID = "";
        internal bool agLoadWarnings = false;
        internal string agDesigner = "";
        internal DateTime agLastEdit;
        internal string agDescription = "";
        internal AGIVersionInfo agIntVersion = new() {
            Index = AGIVersion.v2917,
        };
        internal string agGameAbout = "";
        internal string agGameVersion = "";
        internal string agGameFile = "";
        internal int agMaxVol0 = MAX_VOLSIZE;
        internal int agMaxVolSize = MAX_VOLSIZE; // currently this cannot be changed 
        internal PlatformType agPlatformType = PlatformType.None;
        internal string agPlatformFile = "";
        internal string agPlatformOpts = "";
        internal string agDOSExec = "";
        internal bool agUseLE = false;
        internal bool agIncludeIDs = true;
        internal bool agIncludeReserved = true;
        internal bool agIncludeGlobals = true;
        internal int agCodePage = 437;
        internal bool agPowerPack = false;
        internal string agSrcFileExt = "";
        internal bool agSierraSyntax = false;
        internal SettingsFile agGameProps;
        //internal WinAGIFileWatcher agFileWatcher;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new WinAGI game object using the passed parameters.
        /// Parameters determine if the game will create a new blank game,
        /// a new gam from a template, open an existing WinAGI game file
        /// or import an existing set of AGI game files.
        /// </summary>
        public AGIGame(GameParams argval) {
            InitGame();
            switch (argval.Mode) {
            case OpenGameMode.New:
                // pass along parameters needed to create the new game
                NewGame(argval);
                break;
            case OpenGameMode.File:
                // all parameters will be extracted from the WAG file
                WinAGIEventInfo retval = OpenGameWAG(argval.GameFile);
                if (retval.Type == EventType.GameLoadError) {
                    // error info is in the Data property; copy that info
                    // into the pass-through error object
                    Exception ex = (Exception)retval.Data;
                    ExceptionDispatchInfo.Capture(ex).Throw();
                }
                break;
            case OpenGameMode.Directory:
                // pass along parameters needed to import the game
                retval = OpenGameDIR(argval);
                if (retval.Type == EventType.GameLoadError) {
                    // error info is in the Data property; copy that info
                    // into the pass-through error object
                    Exception ex = (Exception)retval.Data;
                    ExceptionDispatchInfo.Capture(ex).Throw();
                }
                break;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of Logic resources in this game.
        /// </summary>
        public Logics Logics {
            get => agLogs;
        }

        /// <summary>
        /// Gets the collection of Picture resources in this game.
        /// </summary>
        public Pictures Pictures {
            get => agPics;
        }

        /// <summary>
        /// Gets the collection of Sound resources in this game.
        /// </summary>
        public Sounds Sounds {
            get => agSnds;
        }

        /// <summary>
        /// Gets the collection of View resources in this game.
        /// </summary>
        public Views Views {
            get => agViews;
        }

        /// <summary>
        /// Gets the list of words from this game's WORDS.TOK file.
        /// </summary>
        public WordList WordList {
            get => agVocabWords;
        }

        /// <summary>
        /// Gets the list of inventory items from this game's OBJECT file.
        /// </summary>
        public InventoryList InvObjects {
            get => agInvObj;
        }

        /// <summary>
        /// Gets the list of include files that are used for this game.
        /// Used by the logic compiler.
        /// </summary>
        public List<IncludeInfo> IncludeFiles {
            get => agIncludeFiles;
        }

        /// <summary>
        /// Gets the list of global defines for this game. Used by the logic compiler.
        /// </summary>
        public DefinesList GlobalDefines {
            get => agGlobals;
        }

        public ReservedDefineList ReservedDefines {
            get => agReservedDefines;
        }

        /// <summary>
        /// Gets the AGI color palette that is used for displaying pictures and views.
        /// </summary>
        public EGAColors Palette {
            get => agEGAcolors;
        }

        /// <summary>
        /// Gets or sets the Game ID for this game. For V3 games, this also
        /// renames DIR and VOL files.
        /// </summary>
        public string GameID {
            get {
                return agGameID;
            }
            set {
                // limit gameID to 6 characters for v2 games and 5 characters for v3 games
                string NewID = value;
                if (agIntVersion.IsV3) {
                    if (value.Length > 5) {
                        NewID = NewID.Left(5);
                    }
                }
                else {
                    if (NewID.Length > 6) {
                        NewID = NewID.Left(6);
                    }
                }
                if (agGameID == NewID) {
                    return;
                }
                if (agIntVersion.IsV3) {
                    try {
                        File.Move(Path.Combine(agGameDir, agGameID + "DIR"), Path.Combine(agGameDir, NewID.ToUpper() + "DIR"));
                        foreach (string volFile in Directory.EnumerateFiles(agGameDir, agGameID + "VOL.*")) {
                            string extension = Path.GetExtension(volFile);
                            File.Move(volFile, Path.Combine(agGameDir, NewID.ToUpper() + "VOL" + extension));
                        }
                    }
                    catch (Exception e) {
                        WinAGIException wex = new(EngineResourceByNum(503).Replace(ARG1, e.HResult.ToString())) {
                            HResult = WINAGI_ERR + 503
                        };
                        wex.Data["exception"] = e;
                        throw wex;
                    }
                }
                agGameID = NewID;
                WriteGameSetting("General", "GameID", NewID, "", true);
                agReservedDefines?.SaveList(true);
            }
        }

        /// <summary>
        /// Gets or sets the folder where this game's AGI files and WinAGI
        /// supporting files are stored.
        /// </summary>
        public string GameDir {
            get {
                return agGameDir;
            }
            set {
                // changing directory is allowed, but....
                //
                // changing gamedir directly may cause problems
                // because the game may not be able to find the
                // resource files it is looking for;
                // also, changing the gamedir directly does not
                // update the resource directory
                //
                // It is responsibility of calling function to
                // make sure that all game resources/files/
                // folders are also moved/renamed as needed to
                // support the new directory; exception is the
                // agGameFile property, which gets updated
                // in this property

                // validate gamedir
                if (Directory.Exists(value)) {
                    throw new DirectoryNotFoundException(value);
                }
                agGameDir = value;
                agGameFile = Path.Combine(agGameDir, Path.GetFileName(agGameFile));
                agSrcResDir = Path.Combine(agGameDir, agSrcResDirName);
                agLastEdit = DateTime.Now;
            }
        }

        /// <summary>
        /// Gets or sets the name of this game's WAGI game file. When changing the
        /// file name, the old file is automatically moved to the new file name.
        /// </summary>
        public string GameFile {
            get {
                return agGameFile;
            }
            set {
                // calling function has to make sure NewFile is valid!
                try {
                    File.Move(agGameFile, value);
                }
                finally {
                    // ignore errors- caller will need to deal with it
                }
                agGameFile = value;
                agLastEdit = DateTime.Now;
                agGameProps.Filename = agGameFile;
                WriteGameSetting("General", "LastEdit", agLastEdit.ToString(), "", true);
            }
        }

        /// <summary>
        /// Gets or sets a 'game about' string that can be accessed in logics.
        /// </summary>
        public string GameAbout {
            get => agGameAbout;
            set {
                if (value.Length > 4096) {
                    agGameAbout = value.Left(4096).Replace("\r", "");
                }
                else {
                    agGameAbout = value.Replace("\r", "");
                }
                WriteGameSetting("General", "About", agGameAbout);
                agReservedDefines?.SaveList(true);
            }
        }

        /// <summary>
        /// Gets or sets the GameDesigner meta-property. Used only by WinAGI editor.
        /// </summary>
        public string Designer {
            get => agDesigner;
            set {
                if (value.Length > 256) {
                    agDesigner = value.Left(256);
                }
                else {
                    agDesigner = value;
                }
                WriteGameSetting("General", "Designer", agDesigner);
            }
        }

        /// <summary>
        /// Gets or sets the GameDescription meta-property. Used only by WinAGI editor.
        /// </summary>
        public string Description {
            get => agDescription;
            set {
                agDescription = value;
                WriteGameSetting("General", "Description", agDescription);
            }
        }

        /// <summary>
        /// Gets or sets the Sierra Interpreter version that this game is designed 
        /// to be compatible with. If changed between V2 and V3, the game will
        /// automatically be recompiled to correct DIR/VOL format.
        /// </summary>
        public AGIVersionInfo InterpreterVersion {
            get {
                return agIntVersion;
            }
            set {
                // if new version and old version are not same major version:
                if (agIntVersion.IsV3 != value.IsV3) {
                    // set flag to switch version on rebuild
                    V3V2Swap = true;
                    // if new version is V3 (i.e., current version isn't)
                    if (!value.IsV3) {
                        if (GameID.Length > 5) {
                            // truncate the ID
                            GameID = GameID[..5];
                        }
                    }
                    // use compiler to rebuild new vol and dir files
                    // (it is up to calling program to deal with changed or invalid resources)
                    if (Compile(true) != CompileStatus.OK) {
                        // failed - don't make the change
                        Debug.Assert(!Compiling);
                        return;
                    }
                    Debug.Assert(!Compiling);
                }
                // OBJECT file encryption depends on version
                if (value.Index == AGIVersion.v2089 || value.Index == AGIVersion.v2272) {
                    if (agInvObj.Encrypted) {
                        agInvObj.Encrypted = false;
                        agInvObj.Save();
                    }
                }
                else {
                    if (!agInvObj.Encrypted) {
                        agInvObj.Encrypted = true;
                        agInvObj.Save();
                    }
                }
                agLastEdit = DateTime.Now;
                WriteGameSetting("General", "Interpreter", value.VersionString);
                agGameProps.Save();

                // OK to set new version
                agIntVersion = value;
                CorrectCommands(agIntVersion.Index);
            }
        }

        /// <summary>
        /// Gets the full path of this game's resource folder.
        /// </summary>
        public string SrcResDir {
            get {
                return agSrcResDir;
            }
        }

        /// <summary>
        /// Gets or sets the folder name used to store logic source code
        /// files, and exported resource files.<br /><br />
        /// Changing the folder name does NOT automatically move files
        /// from the old folder. Calling function must do that.
        /// </summary>
        public string SrcResDirName {
            get {
                return agSrcResDirName;
            }
            set {
                string tmpName = value.Trim();
                // ignore if blank
                if (tmpName.Length == 0) {
                    return;
                }
                if (Path.GetInvalidFileNameChars().Any(tmpName.Contains)) {
                    throw new ArgumentOutOfRangeException(nameof(SrcResDirName), "Invalid property Value");
                }
                agSrcResDirName = tmpName;
                agSrcResDir = Path.Combine(agGameDir, agSrcResDirName);
                WriteGameSetting("General", "ResDir", agSrcResDirName);
            }
        }

        /// <summary>
        /// Gets or sets the maximum size that WinAGI will use for VOL.0 when
        /// compiling this game. Provided for backward compatibility of floppy
        /// disk installations. 
        /// </summary>
        public int MaxVol0Size {
            get {
                return agMaxVol0;
            }
            set {
                if (value < 32768) {
                    agMaxVol0 = 32768;
                }
                else if (value >= MAX_VOLSIZE) {
                    agMaxVol0 = MAX_VOLSIZE;
                }
                else {
                    agMaxVol0 = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if resource IDs are automatically
        /// considered  defined, or if they must be manually defined.
        /// </summary>
        public bool IncludeIDs {
            get => agIncludeIDs;
            set {
                agIncludeIDs = value;
                agIncludeFiles.RemoveAll(e => e.Filename.Equals(
                    Path.Combine(agSrcResDir, "resourceids.txt"), StringComparison.OrdinalIgnoreCase));
                if (value) {
                    // add new entry for globals.txt
                    agIncludeFiles.Insert(0, new IncludeInfo {
                        Filename = Path.Combine(agSrcResDir, "resourceids.txt"),
                        Type = IncludeType.ResourceIDs
                    });
                }
                WriteGameSetting("Includes", "IncludeIDs", agIncludeIDs.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if reserved variables and flags 
        /// are automatically defined, or if they must be manually defined.
        /// </summary>
        public bool IncludeReserved {
            get => agIncludeReserved;
            set {
                agIncludeReserved = value;
                agIncludeFiles.RemoveAll(e => e.Filename.Equals(
                    Path.Combine(agSrcResDir, "reserved.txt"), StringComparison.OrdinalIgnoreCase));
                if (value) {
                    // add new entry for globals.txt
                    agIncludeFiles.Insert(0, new IncludeInfo {
                        Filename = Path.Combine(agSrcResDir, "reserved.txt"),
                        Type = IncludeType.ResourceIDs
                    });
                }
                WriteGameSetting("Includes", "IncludeReserved", agIncludeReserved.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a value that determines if global defines are 
        /// automatically included in all logics, or if they must be manually
        /// defined.
        /// </summary>
        public bool IncludeGlobals {
            get => agIncludeGlobals;
            set {
                agIncludeGlobals = value;
                agIncludeFiles.RemoveAll(e => e.Filename.Equals(
                    agGlobals.ResFile, StringComparison.OrdinalIgnoreCase));
                if (value) {
                    // add new entry for globals.txt
                    agIncludeFiles.Insert(0, new IncludeInfo {
                        Filename = agGlobals.ResFile,
                        Type = IncludeType.Globals
                    });
                }
                WriteGameSetting("Includes", "IncludeGlobals", agIncludeGlobals.ToString());
            }
        }

        /// <summary>
        /// Gets the load warnings flag - true if warnings were encountered when 
        /// game was loaded.
        /// </summary>
        public bool LoadWarnings {
            get => agLoadWarnings;
        }

        /// <summary>
        /// Gets or sets the default source extension that this game will use when 
        /// decompiling logics or creating new source code files.
        /// </summary>
        public string SourceExt {
            get {
                return agSrcFileExt;
            }
            set {
                // must be non-zero length
                if (value.Length == 0) {
                    return;
                }
                agSrcFileExt = value.ToLower();
                WriteGameSetting("General", "SourceFileExt", agSrcFileExt);
            }
        }

        /// <summary>
        /// Gets the date/time stamp of the last edit operation on this game.
        /// </summary>
        /// 
        public DateTime LastEdit {
            get {
                return agLastEdit;
            }
        }

        /// <summary>
        /// Internal flag that lets other game resources know that a game is currently 
        /// being compiled.
        /// </summary>
        internal bool Compiling { get; set; } = false;

        /// <summary>
        /// Internal flag that is used to detect cancellation of a 
        /// compiled action.
        /// </summary>
        internal bool CancelComp = false;

        /// <summary>
        /// The game compiler needs to know if compiling is happening to because of
        /// a change in version to or from v2 and v3.
        /// </summary>
        private bool V3V2Swap { get; set; } = false;

        /// <summary>
        /// Gets or sets the platform to use when testing this game from the 
        /// game editor.
        /// </summary>
        public PlatformType PlatformType {
            get => agPlatformType;
            set {
                // only 0 - 4 are valid
                if ((int)value < 0 || (int)value > 4) {
                    agPlatformType = PlatformType.None;
                }
                else {
                    agPlatformType = value;
                }
                WriteGameSetting("General", "PlatformType", ((int)agPlatformType).ToString());
            }
        }

        /// <summary>
        /// Gets or sets the platform file that is run when testing this game from 
        /// within the game editor.
        /// </summary>
        public string Platform {
            get => agPlatformFile;
            set {
                agPlatformFile = value;
                WriteGameSetting("General", "Platform", agPlatformFile);
            }
        }

        /// <summary>
        /// Gets or sets optional command line parameters that are passed to the
        /// platform program when testing this game from within the game editor.
        /// </summary>
        public string PlatformOpts {
            get => agPlatformOpts;
            set {
                agPlatformOpts = value;
                WriteGameSetting("General", "PlatformOpts", agPlatformOpts);
            }
        }

        /// <summary>
        /// Gets or sets the MSDOS executable program when using the DOSBox platform
        /// option.
        /// </summary>
        public string DOSExec {
            get => agDOSExec;
            set {
                string newExec = value;
                agDOSExec = newExec;
                WriteGameSetting("General", "DOSExec", agDOSExec);
            }
        }

        /// <summary>
        /// Gets or sets a value that determines whether or not this game will use
        /// WinAGI's Layout Editor
        /// </summary>
        public bool UseLE {
            get {
                return agUseLE;
            }
            set {
                agUseLE = value;
                WriteGameSetting("General", "UseLE", agUseLE);
            }
        }

        /// <summary>
        /// Gets or sets the character encoding used in this game. Changing from the
        /// default encoding is only needed if providing support for other languages.
        /// </summary>
        public int CodePage {
            get {
                return agCodePage;
            }
            set {
                if (validcodepages.Contains(value)) {
                    agCodePage = value;
                    WriteGameSetting("General", "CodePage", agCodePage);
                }
                else {
                    throw new ArgumentOutOfRangeException(nameof(value), "Unsupported or invalid CodePage value");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value that enables original Sierra logic code syntax.
        /// </summary>
        public bool SierraSyntax {
            get {
                return agSierraSyntax;
            }
            set {
                agSierraSyntax = value;
                WriteGameSetting("General", "SierraSyntax", agSierraSyntax);
            }
        }

        /// <summary>
        /// When enabled, WinAGI will automatically provide support for the AGI Power Pack
        /// logic scripts.
        /// </summary>
        public bool PowerPack {
            get {
                return agPowerPack;
            }
            set {
                agPowerPack = value;
                WriteGameSetting("General", "PowerPack", agPowerPack);
            }
        }

        /// <summary>
        /// Gets or sets a string value that can be accessed by logic scripts as a define
        /// name to easily manage the 'game version' message in an AGI game.
        /// </summary>
        public string GameVersion {
            get => agGameVersion;
            set {
                // limit to 256 bytes
                if (value.Length > 256) {
                    agGameVersion = value[..256].Replace("\r", "");
                }
                else {
                    agGameVersion = value.Replace("\r", "");
                }
                WriteGameSetting("General", "GameVersion", agGameVersion);
                agReservedDefines?.SaveList(true);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// This method initializes all game properties and internal game state 
        /// members for a WinAGI game.
        /// </summary>
        private void InitGame() {
            // set up volume manager
            volManager = new(this);

            // initialize all game variables
            ClearGameState();
        }

        /// <summary>
        /// Closes this game and releases all resources.
        /// </summary>
        public void CloseGame() {
            // unload and remove all resources
            agLogs.Clear();
            agPics.Clear();
            agSnds.Clear();
            agViews.Clear();
            agInvObj.Unload();
            agVocabWords.Unload();
            // write date of last edit and save wag
            WriteGameSetting("General", "LastEdit", agLastEdit.ToString(), "", true);
            // clear all game properties
            ClearGameState();
            // shutdown filewatcher
            //agFileWatcher.Dispose();
            //agFileWatcher = null;
        }

        /// <summary>
        /// Compiles the game into NewGameDir, resulting in new VOL and DIR
        /// files that eliminate all dead space.All logics are recompiled 
        /// before being added. Resources with errors will pause until the
        /// calling program provides instructions to  skip them, or add them
        /// with the errors. WORDS.TOK and OBJECT files are only recompiled if
        /// currently changed. If RebuildOnly is true, the VOL files are rebuilt
        /// without recompiling all logics and WORDS.TOK and OBJECT are not
        /// recompiled.<br />
        /// If NewGameDir is same as current directory this WILL overwrite
        /// current game files.
        /// </summary>
        /// <param name="RebuildOnly"></param>
        /// <param name="NewGameDir"></param>
        /// <returns>OK if compile completed successfully, otherwise either
        /// Canceled or Error, depending on reason for failure.</returns>
        public CompileStatus Compile(bool RebuildOnly, string NewGameDir = "") {
            bool replace, newIsV3;
            string gameid;
            int tmpMax = 0, i, j;
            WinAGIEventInfo compInfo = new();

            Compiling = true;
            CancelComp = false;
            // if no directory passed assume current dir
            if (NewGameDir.Length == 0) {
                NewGameDir = agGameDir;
            }
            if (!Directory.Exists(NewGameDir)) {
                // raise error event
                WinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = EngineResourceByNum(510).Replace(ARG1, NewGameDir),
                    Data = 510
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.ResourceError;
            }
            replace = NewGameDir.Equals(agGameDir, StringComparison.OrdinalIgnoreCase);
            // whether or not the new game files will be V3 depends on
            // if changing major interpreter version or not
            if (V3V2Swap) {
                // new version is V3 if current version is V2
                newIsV3 = !agIntVersion.IsV3;
                V3V2Swap = false;
            }
            else {
                // new version is same as current version
                newIsV3 = agIntVersion.IsV3;
            }
            if (newIsV3) {
                gameid = agGameID.ToUpper();
            }
            else {
                gameid = "";
            }
            // version2 IDs limited to 6 characters,  v3 IDs limited to 5 characters
            if (agGameID.Length > 6 || (newIsV3 && agGameID.Length > 5)) {
                // invalid ID; should never happen
                agGameID = agGameID[..(newIsV3 ? 5 : 6)];
            }
            if (!RebuildOnly) {
                // full compile - check words.tok and object files first for
                // errors, and recompile them if needed
                compInfo.Text = "";
                compInfo.ResType = AGIResType.Words;
                compInfo.InfoType = InfoType.Resources;
                if (CheckForCancel(GameCompileStatus.CompileWords, compInfo)) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                // verify no errors before compiling
                if (agVocabWords.Error != ResourceErrorType.NoError) {
                    WinAGIEventInfo tmpError = new() {
                        Type = EventType.GameCompileError,
                        ResType = AGIResType.Words,
                        Text = "WORDS.TOK Error: ",
                    };
                    switch (agVocabWords.Error) {
                    case ResourceErrorType.WordsTokNoFile:
                        tmpError.Text += EngineResources.RE19;
                        break;
                    case ResourceErrorType.WordsTokIsReadOnly:
                        tmpError.Text += EngineResources.RE20;
                        break;
                    case ResourceErrorType.WordsTokAccessError:
                        tmpError.Text += EngineResources.RE21.Replace(
                            ARG1, agVocabWords.ErrData[0]);
                        break;
                    case ResourceErrorType.WordsTokNoData:
                        tmpError.Text += EngineResources.RE22;
                        break;
                    case ResourceErrorType.WordsTokBadIndex:
                        tmpError.Text += EngineResources.RE23;
                        break;
                    }
                    OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                    CompleteCancel(true);
                    return CompileStatus.ResourceError;
                }
                if (agVocabWords.IsChanged) {
                    try {
                        agVocabWords.Save();
                    }
                    catch (Exception ex) {
                        WinAGIEventInfo tmpError = new() {
                            Type = EventType.GameCompileError,
                            ResType = AGIResType.Words,
                            Text = "Error during compilation of WORDS.TOK (" + ex.Message + ")"
                        };
                        OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                        CompleteCancel(true);
                        return CompileStatus.ResourceError;
                    }
                }
                // check it for errors
                compInfo.InfoType = InfoType.ClearWarnings;
                if (CheckForCancel(GameCompileStatus.CompileWords, compInfo)) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                if (agVocabWords.Error != ResourceErrorType.NoError) {
                    AddCompileError(AGIResType.Words, 0, agVocabWords.Error, agVocabWords.ErrData);
                }
                if (agVocabWords.Warnings != 0) {
                    if (AddCompileWarnings(AGIResType.Words, 0, agVocabWords.Warnings, agVocabWords.WarnData)) {
                        CompleteCancel();
                        return CompileStatus.Canceled;
                    }
                }
                if (!replace) {
                    // copy WORDS.TOK to new folder
                    try {
                        // then copy the current file to new location
                        File.Copy(agVocabWords.ResFile, Path.Combine(NewGameDir, "WORDS.TOK"), true);
                    }
                    catch (Exception ex) {
                        WinAGIEventInfo tmpError = new() {
                            Type = EventType.GameCompileError,
                            ResType = AGIResType.Words,
                            Text = "Error while creating WORDS.TOK file (" + ex.Message + ")"
                        };
                        OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                        CompleteCancel(true);
                        return CompileStatus.ResourceError;
                    }
                }
                // OBJECT file is next
                compInfo.Text = "";
                compInfo.ResType = AGIResType.Objects;
                compInfo.InfoType = InfoType.Resources;
                if (CheckForCancel(GameCompileStatus.CompileObjects, compInfo)) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                // verify no errors before compiling
                if (agInvObj.Error != ResourceErrorType.NoError) {
                    WinAGIEventInfo tmpError = new() {
                        Type = EventType.GameCompileError,
                        ResType = AGIResType.Objects,
                        Text = "OBJECT Error: ",
                    };
                    switch (agInvObj.Error) {
                    case ResourceErrorType.ObjectNoFile:
                        tmpError.Text += EngineResources.RE13;
                        break;
                    case ResourceErrorType.ObjectIsReadOnly:
                        tmpError.Text += EngineResources.RE14;
                        break;
                    case ResourceErrorType.ObjectAccessError:
                        tmpError.Text += EngineResources.RE15.Replace(
                            ARG1, agInvObj.ErrData[0]);
                        break;
                    case ResourceErrorType.ObjectNoData:
                        tmpError.Text += EngineResources.RE16;
                        break;
                    case ResourceErrorType.ObjectDecryptError:
                        tmpError.Text += EngineResources.RE17;
                        break;
                    case ResourceErrorType.ObjectBadHeader:
                        tmpError.Text += EngineResources.RE18;
                        break;
                    }
                    OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                    CompleteCancel(true);
                    return CompileStatus.ResourceError;
                }
                if (agInvObj.IsChanged) {
                    try {
                        agInvObj.Save();
                    }
                    catch (Exception ex) {
                        WinAGIEventInfo tmpError = new() {
                            Type = EventType.GameCompileError,
                            ResType = AGIResType.Objects,
                            Text = "Error during compilation of OBJECT (" + ex.Message + ")"
                        };
                        OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                        CompleteCancel(true);
                        return CompileStatus.ResourceError;
                    }
                }
                // check it for errors
                compInfo.InfoType = InfoType.ClearWarnings;
                if (CheckForCancel(GameCompileStatus.CompileObjects, compInfo)) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                if (agInvObj.Error != ResourceErrorType.NoError) {
                    AddCompileError(AGIResType.Objects, 0, agInvObj.Error, agInvObj.ErrData);
                }
                if (agInvObj.Warnings != 0) {
                    if (AddCompileWarnings(AGIResType.Objects, 0, agInvObj.Warnings, agInvObj.WarnData)) {
                        CompleteCancel();
                        return CompileStatus.Canceled;
                    }
                }
                if (!replace) {
                    // copy OBJECT to new folder
                    try {
                        File.Copy(agInvObj.ResFile, Path.Combine(NewGameDir, "OBJECT"), true);
                    }
                    catch (Exception ex) {
                        WinAGIEventInfo tmpError = new() {
                            Type = EventType.GameCompileError,
                            ResType = AGIResType.Objects,
                            Text = "Error while creating OBJECT file (" + ex.Message + ")"
                        };
                        OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                        CompleteCancel(true);
                        return CompileStatus.ResourceError;
                    }
                }
                // if using globals, confirm globals file doesn't have any errors
                if (IncludeGlobals) {
                    if (agGlobals.Error != ResourceErrorType.NoError) {
                        WinAGIEventInfo tmpError = new() {
                            Type = EventType.GameCompileError,
                            ResType = AGIResType.Globals,
                            Text = "globals.txt Error: ",
                        };
                        switch (agGlobals.Error) {
                        case ResourceErrorType.DefinesNoFile:
                            tmpError.Text += EngineResources.RE24;
                            break;
                        case ResourceErrorType.DefinesReadOnly:
                            tmpError.Text += EngineResources.RE25;
                            break;
                        case ResourceErrorType.DefinesAccessError:
                            tmpError.Text += EngineResources.RE26.Replace(
                                ARG1, agGlobals.ErrData[0]);
                            break;
                        }
                        OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                        CompleteCancel(true);
                        return CompileStatus.ResourceError;
                    }
                }
            }
            // reset game compiler variables
            volManager.Clear();

            // remove any temp vol files
            for (int v = 0; v < 15; v++) {
                SafeFileDelete(Path.Combine(NewGameDir, "NEW_VOL." + v.ToString()));
            }
            try {
                // open first new vol file
                volManager.InitVol(NewGameDir);
            }
            catch (Exception e) {
                // raise error event
                WinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    Module = e.Message,
                    ResType = AGIResType.Game,
                    Text = EngineResourceByNum(503).Replace(ARG1, Path.Combine(NewGameDir, "NEW_VOL.0")),
                    Data = 503
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.ResourceError;
            }
            // add all logic resources
            try {
                CompileStatus result = VOLManager.CompileResCol(this, agLogs, AGIResType.Logic, RebuildOnly, newIsV3);
                // reset added include file list
                AddedIncludes.Clear();
                if (result != CompileStatus.OK) {
                    CompleteCancel(true);
                    return result;
                }
            }
            catch (Exception e) {
                // reset added include file list
                AddedIncludes.Clear();
                // raise error event
                WinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = e.Message + ":\n\n" + e.StackTrace,
                    Data = e.HResult
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.ResourceError;
            }
            // add all picture resources
            try {
                CompileStatus result = VOLManager.CompileResCol(this, agPics, AGIResType.Picture, RebuildOnly, newIsV3);
                if (result != CompileStatus.OK) {
                    // resource error (or user canceled)
                    CompleteCancel(true);
                    return result;
                }
            }
            catch (Exception e) {
                // raise error event
                WinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = e.Message + ":\n\n" + e.StackTrace,
                    Data = e.HResult
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.ResourceError;
            }
            // add all view resources
            try {
                CompileStatus result = VOLManager.CompileResCol(this, agViews, AGIResType.View, RebuildOnly, newIsV3);
                if (result != CompileStatus.OK) {
                    // resource error (or user canceled)
                    CompleteCancel(true);
                    return result;
                }
            }
            catch (Exception e) {
                // raise error event
                WinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = e.Message + ":\n\n" + e.StackTrace,
                    Data = e.HResult
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.ResourceError;
            }
            // add all sound resources
            try {
                CompileStatus result = VOLManager.CompileResCol(this, agSnds, AGIResType.Sound, RebuildOnly, newIsV3);
                if (result != CompileStatus.OK) {
                    // resource error (or user canceled)
                    CompleteCancel(true);
                    return result;
                }
            }
            catch (Exception e) {
                // raise error event
                WinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = e.Message + ":\n\n" + e.StackTrace,
                    Data = e.HResult
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.ResourceError;
            }
            // release handle on vol file
            volManager.VOLFile.Dispose();
            volManager.VOLWriter.Dispose();
            // delete existing dir files
            if (newIsV3) {
                SafeFileDelete(Path.Combine(NewGameDir, gameid + "DIR"));
            }
            else {
                SafeFileDelete(Path.Combine(NewGameDir, "LOGDIR"));
                SafeFileDelete(Path.Combine(NewGameDir, "PICDIR"));
                SafeFileDelete(Path.Combine(NewGameDir, "VIEWDIR"));
                SafeFileDelete(Path.Combine(NewGameDir, "SNDDIR"));
            }
            // now build the new DIR files
            if (newIsV3) {
                volManager.DIRFile = File.Create(Path.Combine(NewGameDir, gameid + "DIR"));
                using (volManager.DIRWriter) {
                    // add offsets - logdir offset is always 8
                    volManager.DIRWriter.Write(Convert.ToInt16(8));
                    // pic offset is 8 + 3*logmax
                    tmpMax = agLogs.Max + 1;
                    if (tmpMax == 0) {
                        // always put at least one; even if it's all FFs
                        tmpMax = 1;
                    }
                    volManager.DIRWriter.Write((short)(8 + 3 * tmpMax));
                    i = 8 + 3 * tmpMax;
                    // view offset is pic offset + 3*picmax
                    tmpMax = agPics.Max + 1;
                    if (tmpMax == 0) {
                        tmpMax = 1;
                    }
                    volManager.DIRWriter.Write((short)(i + 3 * tmpMax));
                    i += 3 * tmpMax;
                    // sound is view offset + 3*viewmax
                    tmpMax = agViews.Max + 1;
                    if (tmpMax == 0) {
                        tmpMax = 1;
                    }
                    volManager.DIRWriter.Write((short)(i + 3 * tmpMax));
                    // now add all the dir entries (we can't use a for-next loop
                    // because sound and view dirs are swapped in v3 directory
                    // logics first
                    tmpMax = agLogs.Max;
                    for (i = 0; i <= tmpMax; i++) {
                        volManager.DIRWriter.Write(volManager.DIRData[0, i, 0]);
                        volManager.DIRWriter.Write(volManager.DIRData[0, i, 1]);
                        volManager.DIRWriter.Write(volManager.DIRData[0, i, 2]);
                    }
                    // next are pictures
                    tmpMax = agPics.Max;
                    for (i = 0; i <= tmpMax; i++) {
                        volManager.DIRWriter.Write(volManager.DIRData[1, i, 0]);
                        volManager.DIRWriter.Write(volManager.DIRData[1, i, 1]);
                        volManager.DIRWriter.Write(volManager.DIRData[1, i, 2]);
                    }
                    // then views
                    tmpMax = agViews.Max;
                    for (i = 0; i <= tmpMax; i++) {
                        volManager.DIRWriter.Write(volManager.DIRData[3, i, 0]);
                        volManager.DIRWriter.Write(volManager.DIRData[3, i, 1]);
                        volManager.DIRWriter.Write(volManager.DIRData[3, i, 2]);
                    }
                    // and finally, sounds
                    tmpMax = agSnds.Max;
                    for (i = 0; i <= tmpMax; i++) {
                        volManager.DIRWriter.Write(volManager.DIRData[2, i, 0]);
                        volManager.DIRWriter.Write(volManager.DIRData[2, i, 1]);
                        volManager.DIRWriter.Write(volManager.DIRData[2, i, 2]);
                    }
                }
            }
            else {
                // make separate dir files
                for (j = 0; j < 4; j++) {
                    switch ((AGIResType)j) {
                    case AGIResType.Logic:
                        volManager.DIRFile = File.Create(Path.Combine(NewGameDir, "LOGDIR"));
                        tmpMax = agLogs.Max;
                        break;
                    case AGIResType.Picture:
                        volManager.DIRFile = File.Create(Path.Combine(NewGameDir, "PICDIR"));
                        tmpMax = agPics.Max;
                        break;
                    case AGIResType.Sound:
                        volManager.DIRFile = File.Create(Path.Combine(NewGameDir, "SNDDIR"));
                        tmpMax = agSnds.Max;
                        break;
                    case AGIResType.View:
                        volManager.DIRFile = File.Create(Path.Combine(NewGameDir, "VIEWDIR"));
                        tmpMax = agViews.Max;
                        break;
                    }
                    // create the dir file
                    using (volManager.DIRWriter) {
                        for (i = 0; i <= tmpMax; i++) {
                            volManager.DIRWriter.Write(volManager.DIRData[j, i, 0]);
                            volManager.DIRWriter.Write(volManager.DIRData[j, i, 1]);
                            volManager.DIRWriter.Write(volManager.DIRData[j, i, 2]);
                        }
                    }
                }
            }
            // delete current vol files
            for (i = 0; i < 16; i++) {
                if (newIsV3) {
                    if (File.Exists(Path.Combine(NewGameDir, gameid + "VOL." + i.ToString()))) {
                        SafeFileDelete(Path.Combine(NewGameDir, gameid + "VOL." + i.ToString()));
                    }
                }
                else {
                    if (File.Exists(Path.Combine(NewGameDir, "VOL." + i.ToString()))) {
                        SafeFileDelete(Path.Combine(NewGameDir, "VOL." + i.ToString()));
                    }
                }
            }
            // now rename newly created VOL files
            for (i = 0; i < volManager.Count; i++) {
                File.Move(Path.Combine(NewGameDir, "NEW_VOL." + i.ToString()), Path.Combine(NewGameDir, gameid + "VOL." + i.ToString()));
            }
            // update status to indicate complete
            compInfo.Text = "";
            compInfo.ResType = AGIResType.Game;
            if (CheckForCancel(GameCompileStatus.CompileComplete, compInfo)) {
                CompleteCancel();
                return CompileStatus.Canceled;
            }
            if (replace) {
                // need to update the vol/loc info for all ingame resources;
                // this is done here instead of when the resources are compiled because
                // if there's an error, or the user cancels, we don't want the directories
                // to point to the wrong place
                foreach (Logic tmpLogic in agLogs.Col.Values) {
                    tmpLogic.Volume = (sbyte)(volManager.DIRData[0, tmpLogic.Number, 0] >> 4);
                    tmpLogic.Loc = ((volManager.DIRData[0, tmpLogic.Number, 0] & 0xF) << 16) + (volManager.DIRData[0, tmpLogic.Number, 1] << 8) + volManager.DIRData[0, tmpLogic.Number, +2];
                }
                foreach (Picture tmpPicture in agPics.Col.Values) {
                    tmpPicture.Volume = (sbyte)(volManager.DIRData[1, tmpPicture.Number, 0] >> 4);
                    tmpPicture.Loc = ((volManager.DIRData[1, tmpPicture.Number, 0] & 0xF) << 16) + (volManager.DIRData[1, tmpPicture.Number, 1] << 8) + volManager.DIRData[1, tmpPicture.Number, 2];
                }
                foreach (Sound tmpSound in agSnds.Col.Values) {
                    tmpSound.Volume = (sbyte)(volManager.DIRData[2, tmpSound.Number, 0] >> 4);
                    tmpSound.Loc = ((volManager.DIRData[2, tmpSound.Number, 0] & 0xF) << 16) + (volManager.DIRData[2, tmpSound.Number, 1] << 8) + volManager.DIRData[2, tmpSound.Number, 2];
                }
                foreach (View tmpView in agViews.Col.Values) {
                    tmpView.Volume = (sbyte)(volManager.DIRData[3, tmpView.Number, 0] >> 4);
                    tmpView.Loc = ((volManager.DIRData[3, tmpView.Number, 0] & 0xF) << 16) + (volManager.DIRData[3, tmpView.Number, 1] << 8) + volManager.DIRData[3, tmpView.Number, 2];
                }
            }
            try {
                // save the wag file
                agGameProps.Save();
            }
            catch (Exception ex) {
                // raise error event
                WinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = ex.Message + ":\n\n" + ex.StackTrace,
                    Data = ex.HResult
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.ResourceError;
            }
            // reset compiling flag
            Compiling = false;
            // clear the volManager
            volManager.Clear();
            return CompileStatus.OK;
        }

        /// <summary>
        /// This method forces WinAGI to save the property file, instead of waiting
        /// until a property is updated.
        /// </summary>
        public void SaveProperties() {
            agGameProps.Save();
        }

        /// <summary>
        /// Creates a new game in specified folder with the specified parameters.
        /// If a template folder is passed, the new game will clone the template
        /// folder.
        /// </summary>
        /// <param name="NewID"></param>
        /// <param name="NewVersion"></param>
        /// <param name="NewGameDir"></param>
        /// <param name="NewResDir"></param>
        /// <param name="TemplateDir"></param>
        /// <param name="NewExt"></param>
        /// <returns></returns>
        private void NewGame(GameParams newGame) {
            string oldExt;
            WinAGIEventInfo eventInfo = new();

            // reset game variables
            ClearGameState();
            // set up event status object
            eventInfo.InfoType = InfoType.Initialize;
            eventInfo.Text = "validating parameters";
            OnNewGameStatus(eventInfo);

            // validate target folder is acceptable
            if (!Directory.Exists(newGame.GameDir)) {
                // folder must already exist
                throw new DirectoryNotFoundException(newGame.GameDir);
            }
            if (Directory.GetFiles(newGame.GameDir, "*.wag").Length != 0) {
                // folder cannot have existing WinAGI game files
                WinAGIException wex = new(EngineResourceByNum(535)) {
                    HResult = WINAGI_ERR + 535
                };
                throw wex;
            }
            bool x = false; //
            if (IsValidGameDir(newGame.GameDir, ref x)) {
                // folder cannot have existing Sierra game files
                WinAGIException wex = new(EngineResourceByNum(536)) {
                    HResult = WINAGI_ERR + 536
                };
                throw wex;
            }

            // folder is OK
            agGameDir = newGame.GameDir;
            if (newGame.SrcResDirName.Length == 0) {
                newGame.SrcResDirName = agDefResDir;
            }
            if (newGame.SrcExt.Length == 0) {
                newGame.SrcExt = defSrcExt;
            }
            // get settings
            agSierraSyntax = newGame.SierraSyntax;
            agCodePage = newGame.CodePage;
            agSrcFileExt = newGame.SrcExt.ToLower();
            agIncludeGlobals = newGame.IncludeGlobals;
            agIncludeIDs = newGame.IncludeIDs;
            agIncludeReserved = newGame.IncludeReserved;

            if (newGame.TemplateDir.Length == 0) {
                // blank game (no template) so create the new files
                // and set game parameters
                eventInfo.InfoType = InfoType.Resources;
                eventInfo.Text = "creating new game components";
                OnNewGameStatus(eventInfo);
                agIntVersion = new() {
                    Index = newGame.Version,
                };
                // set game id (limit to 6 characters for v2, and 5 characters for v3
                if (agIntVersion.IsV3) {
                    agGameID = newGame.ID.Left(5);
                }
                else {
                    agGameID = newGame.ID.Left(6);
                }
                // set the resource directory name so it can be set up
                agSrcResDirName = newGame.SrcResDirName;
                agSrcResDir = Path.Combine(agGameDir, agSrcResDirName);
                // assign source file extension
                agSrcFileExt = newGame.SrcExt;
                // create empty property file
                agGameFile = Path.Combine(agGameDir, agGameID + ".wag");
                agGameProps = new SettingsFile(agGameFile, FileMode.Create);
                agGameProps.Lines.Add("# WinAGI Game Property File for " + agGameID);
                agGameProps.Lines.Add("#");
                agGameProps.Lines.Add("");
                agGameProps.Lines.Add("[General]");
                agGameProps.Lines.Add("   WinAGIVersion = " + WINAGI_VERSION);
                agGameProps.Lines.Add("   GameID = " + agGameID);
                agGameProps.Lines.Add("   Interpreter = " + agIntVersion.VersionString);
                agGameProps.Lines.Add("   ResDir = " + newGame.SrcResDirName);
                agGameProps.Lines.Add("   SourceFileExt = " + newGame.SrcExt);
                agGameProps.Lines.Add("   IncludeIDs = " + agIncludeIDs);
                agGameProps.Lines.Add("   IncludeReserved = " + agIncludeReserved);
                agGameProps.Lines.Add("   IncludeGlobals = " + agIncludeGlobals);
                agGameProps.Lines.Add("   SierraSyntax = " + agSierraSyntax);
                agGameProps.Lines.Add("   CodePage" + agCodePage);
                agGameProps.Lines.Add("[Palette]");
                // save palette colors
                for (int i = 0; i < 16; i++) {
                    agGameProps.Lines.Add("   Color" + i + " = " + EGAColors.ColorText(Palette[i]));
                }
                agGameProps.Lines.Add("");
                agGameProps.Lines.Add("[WORDS.TOK]");
                agGameProps.Lines.Add("");
                agGameProps.Lines.Add("[OBJECT]");
                agGameProps.Lines.Add("");
                agGameProps.Lines.Add("[::BEGIN Logics::]");
                agGameProps.Lines.Add("[::END Logics::]");
                agGameProps.Lines.Add("");
                agGameProps.Lines.Add("[::BEGIN Pictures::]");
                agGameProps.Lines.Add("[::END Pictures::]");
                agGameProps.Lines.Add("");
                agGameProps.Lines.Add("[::BEGIN Sounds::]");
                agGameProps.Lines.Add("[::END Sounds::]");
                agGameProps.Lines.Add("");
                agGameProps.Lines.Add("[::BEGIN Views::]");
                agGameProps.Lines.Add("[::END Views::]");
                agGameProps.Lines.Add("");
                agGameProps.Save();
                // create default resource DIR files
                // (default DIR files shold only have ONE 'FFFFFF' entry)
                byte[] bytDirData = [0xff, 0xff, 0xff];
                if (agIntVersion.IsV3) {
                    // for v3, header should be '08 - 00 - 0B - 00 - 0E - 00 - 11 - 00
                    byte[] bytDirHdr = [8, 0, 0x0b, 0, 0x0e, 0, 0x11, 0];
                    using FileStream fsDIR = new(Path.Combine(agGameDir, agGameID + "DIR"), FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirHdr);
                    for (int i = 0; i < 4; i++) {
                        fsDIR.Write(bytDirData);
                    }
                    fsDIR.Dispose();
                }
                else {
                    FileStream fsDIR = new(Path.Combine(agGameDir, "LOGDIR"), FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR.Dispose();
                    fsDIR = new FileStream(Path.Combine(agGameDir, "PICDIR"), FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR.Dispose();
                    fsDIR = new FileStream(Path.Combine(agGameDir, "SNDDIR"), FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR.Dispose();
                    fsDIR = new FileStream(Path.Combine(agGameDir, "VIEWDIR"), FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR.Dispose();
                }
                // default vocabulary word list;  use loaded argument to force
                // load of the new wordlist so it can be saved
                agVocabWords = new WordList(this, true);
                agVocabWords.AddWord("a", 0);
                agVocabWords.AddWord("anyword", 1);
                agVocabWords.AddWord("rol", 9999);
                agVocabWords.Save();
                // create inventory objects list
                // use loaded argument to force load of new inventory list
                agInvObj = new InventoryList(this, true);
                // adjust encryption based on version
                if (newGame.Version == AGIVersion.v2089 ||
                    newGame.Version == AGIVersion.v2272) {
                    agInvObj.Encrypted = false;
                }
                else {
                    agInvObj.Encrypted = true;
                }
                agInvObj.Save();
                // create/confirm resource dir
                if (!Directory.Exists(agSrcResDir)) {
                    try {
                        Directory.CreateDirectory(agSrcResDir);
                    }
                    catch {
                        // note the problem as a warning
                        WinAGIEventInfo dirinfo = new() {
                            ResType = AGIResType.Game,
                            Type = EventType.ResourceWarning,
                            ID = "RW03",
                            Text = EngineResources.RW03.Replace(
                                ARG1, agSrcResDir)
                        };
                        OnNewGameStatus(dirinfo);
                        // use game directory
                        agSrcResDir = agGameDir;
                    }
                }
                if (IncludeReserved) {
                    agReservedDefines = new(this);
                }
                // assign a globals object
                agGlobals = new(this);
                if (IncludeGlobals) {
                    if (!File.Exists(Path.Combine(agSrcResDir, "globals.txt"))) {
                        using FileStream fs = File.Create(Path.Combine(agSrcResDir, "globals.txt"));
                        string hdr = "[\n";
                        hdr += "[ global defines file for " + agGameID + "\n";
                        hdr += "[\n";
                        fs.Write(Encoding.Default.GetBytes(hdr));
                        fs.Close();
                    }
                    agGlobals.LoadDefines();
                }
                // set commands based on AGI version
                CorrectCommands(agIntVersion.Index);
                // add logic zero
                Logic newlogic = agLogs.Add(0);

                // add default text
                List<string> src =
                [];
                src.Add("[*********************************************************************");
                src.Add("[");
                src.Add("[ " + newlogic.ID);
                src.Add("[");
                src.Add("[*********************************************************************");
                if (!agSierraSyntax) {
                    // add standard include files
                    if (IncludeIDs) {
                        src.Add("#include \"resourceids.txt\"");
                    }
                    if (IncludeReserved) {
                        src.Add("#include \"reserved.txt\"");
                    }
                    if (IncludeGlobals) {
                        src.Add("#include \"globals.txt\"");
                    }
                }
                src.Add("");
                src.Add("return();");
                src.Add("");
                newlogic.SourceText = string.Join(NEWLINE, [.. src]);
                newlogic.Save();
                agLogs[0].Unload();
            }
            else {
                eventInfo.InfoType = InfoType.Initialize;
                eventInfo.Text = "copying template files to new location";
                OnNewGameStatus(eventInfo);
                // template should include wag file, dir files, vol files, words.tok and object;
                // also layout file and source directory with logic source files and required includes
                // should be exactly one wag file
                if (Directory.GetFiles(newGame.TemplateDir, "*.wag").Length == 0) {
                    WinAGIException wex = new(EngineResourceByNum(527).Replace(
                        ARG1, newGame.TemplateDir)) {
                        HResult = WINAGI_ERR + 527
                    };
                    throw wex;
                }
                else if (Directory.GetFiles(newGame.TemplateDir, "*.wag").Length != 1) {
                    WinAGIException wex = new(EngineResourceByNum(543)) {
                        HResult = WINAGI_ERR + 543
                    };
                    throw wex;
                }
                // template should have at least one subdirectory
                if (Directory.GetDirectories(newGame.TemplateDir).Length == 0) {
                    // no resource directory
                    WinAGIException wex = new(EngineResourceByNum(544)) {
                        HResult = WINAGI_ERR + 544
                    };
                    throw wex;
                }
                // 1. copy files from template
                try {
                    CopyDirectory(newGame.TemplateDir, agGameDir);
                    // get wag file name (it's first[and only] element)
                    agGameFile = Directory.GetFiles(agGameDir, "*.wag")[0];
                    // rename it to match new ID
                    File.Move(agGameFile, Path.Combine(agGameDir, newGame.ID + ".wag"));
                    agGameFile = Path.Combine(agGameDir, newGame.ID + ".wag");
                }
                catch (Exception ex) {
                    WinAGIException wex = new(EngineResourceByNum(533).Replace(
                        ARG1, ex.Message)) {
                        HResult = WINAGI_ERR + 533
                    };
                    wex.Data["exception"] = ex;
                    throw wex;
                }
                // 2. adjust game parameters:
                eventInfo.InfoType = InfoType.Initialize;
                eventInfo.Text = "updating game components";
                OnNewGameStatus(eventInfo);
                agGameID = newGame.ID;
                // retrieve name of the first directory as current resource dir
                agSrcResDir = Directory.GetDirectories(agGameDir)[0];
                agSrcResDirName = newGame.SrcResDirName;
                if (!agSrcResDir.Equals(Path.Combine(agGameDir, agSrcResDirName), StringComparison.OrdinalIgnoreCase)) {
                    // rename it to new 
                    try {
                        DirectoryInfo resDir = new(agSrcResDir);
                        resDir.MoveTo(Path.Combine(agGameDir, newGame.SrcResDirName));
                        agSrcResDir = Path.Combine(agGameDir, newGame.SrcResDirName);
                    }
                    catch (Exception e) {
                        WinAGIException wex = new(EngineResourceByNum(545).Replace(
                            ARG1, e.Message)) {
                            HResult = WINAGI_ERR + 545
                        };
                        wex.Data["exception"] = e;
                        throw wex;
                    }
                }
                // need to open the new wag file to update key properties
                try {
                    agGameProps = new(agGameFile, FileMode.Open);
                }
                catch (Exception ex) {
                    WinAGIException wex = new(EngineResourceByNum(547).Replace(ARG1, ex.Message)) {
                        HResult = WINAGI_ERR + 547
                    };
                    throw wex;
                }
                // check version
                string version = agGameProps.GetSetting("General", "WinAGIVersion", "");
                if (version.Left(4) == "1.2." || (version.Left(2) == "2.")) {
                    // template must be updated before it can be used
                    WinAGIException wex = new(EngineResourceByNum(549)) {
                        HResult = WINAGI_ERR + 549,
                    };
                    wex.Data["version"] = version;
                    throw wex;
                }
                if (version != WINAGI_VERSION) {
                    // invalid WinAGI version
                    WinAGIException wex = new(EngineResourceByNum(549)) {
                        HResult = WINAGI_ERR + 550
                    };
                    wex.Data["version"] = version;
                    throw wex;
                }
                // gameid
                agGameProps.WriteSetting("General", "GameID", newGame.ID);
                // resdir
                agGameProps.WriteSetting("General", "ResDir", newGame.SrcResDirName);
                // source file ext
                oldExt = agGameProps.GetSetting("General", "SourceFileExt", DefaultSrcExt).Trim();
                if (oldExt[0] == '.') {
                    oldExt = oldExt[1..];
                }
                if (!oldExt.Equals(agSrcFileExt, StringComparison.OrdinalIgnoreCase)) {
                    agGameProps.WriteSetting("General", "SourceFileExt", agSrcFileExt);
                    try {
                        // GetExtension returns the preceding '.'
                        oldExt = "." + oldExt;
                        // rename all existing logic source files
                        foreach (string file in Directory.GetFiles(agSrcResDir)) {
                            if (Path.GetExtension(file) == oldExt) {
                                File.Move(file, Path.Combine(agSrcResDir, Path.GetFileNameWithoutExtension(file) + "." + agSrcFileExt));
                            }
                        }
                    }
                    catch {
                        // ignore errors
                    }
                }
                // force update of other game parameters
                agGameProps.WriteSetting("Includes", "IncludeIDs", agIncludeIDs);
                agGameProps.WriteSetting("Includes", "IncludeReserved", agIncludeReserved);
                agGameProps.WriteSetting("Includes", "IncludeGlobals", agIncludeGlobals);
                agGameProps.WriteSetting("General", "SierraSyntax", agSierraSyntax);
                agGameProps.WriteSetting("General", "CodePage", agCodePage);
                agGameProps.Save();
                // rename wal file, if present
                foreach (string tmpWAL in Directory.GetFiles(agGameDir, "*.wal")) {
                    try {
                        File.Move(tmpWAL, Path.Combine(agGameDir, Path.GetFileNameWithoutExtension(agGameFile) + ".wal"));
                    }
                    catch {
                        // ignore errors
                    }
                    // only need to check for a single file
                    break;
                }
                // update global file header for non-Sierra syntax games
                if (!agSierraSyntax) {
                    if (File.Exists(Path.Combine(agSrcResDir, "globals.txt"))) {
                        try {
                            SettingsFile stlGlobals = new(Path.Combine(agSrcResDir, "globals.txt"), FileMode.OpenOrCreate);
                            if (stlGlobals.Lines.Count > 3) {
                                if (stlGlobals.Lines[1].Trim().Left(1) == "[") {
                                    stlGlobals.Lines[1] = "[ global defines file for " + newGame.ID;
                                }
                                // save it
                                stlGlobals.Save();
                            }
                        }
                        catch {
                            // ignore errors
                        }
                    }
                }
                // 3. open the newly created game
                eventInfo.InfoType = InfoType.Finalizing;
                eventInfo.Text = "opening the new game";
                OnNewGameStatus(eventInfo);
                try {
                    // finish by loading the game resources
                    FinishGameLoad(OpenGameMode.New);
                }
                catch (Exception ex) {
                    WinAGIException wex = new(EngineResourceByNum(534).Replace(
                        ARG1, ex.Message)) {
                        HResult = WINAGI_ERR + 534
                    };
                    wex.Data["exception"] = ex;
                    throw wex;
                }
            }
            setIDs = false;
            SetResourceIDs(this);
            // enable file watcher
            //agFileWatcher = new(agGameDir);
            //agFileWatcher.Enabled = true;
        }

        /// <summary>
        /// Creates a new WinAGI game file from an existing Sierra game directory.
        /// If successful, warning/load info is passed back. 
        /// If fails, exception is thrown.
        /// </summary>
        /// <returns></returns>
        private WinAGIEventInfo OpenGameDIR(GameParams argval) {
            // periodically report status of the load back to calling function
            WinAGIEventInfo warnInfo = new() {
                Type = EventType.ResourceWarning,
            };
            // set game directory
            agGameDir = argval.GameDir;
            warnInfo.Type = EventType.Info;
            warnInfo.InfoType = InfoType.Validating;
            warnInfo.Text = "";
            OnLoadGameStatus(warnInfo);
            bool importresources = false;

            // check for valid DIR/VOL files, (which gets gameid, and sets
            // version 3 status flag)
            bool isV3 = false;
            if (IsValidGameDir(agGameDir, ref isV3)) {
                // get version number (version3 flag already set)
                agIntVersion = GetIntVersion(agGameDir, isV3);
            }
            else {
                if (argval.SierraSyntax) {
                    // if no vol/dir files, assume importing sierra source
                    importresources = true;
                    // assume ID is the gamedir name
                    agGameID = Path.GetFileNameWithoutExtension(agGameDir[..^1]);
                    const string unwanted = "!\"&'()*+,-/:;<=>?[\\]^`{|}~";
                    agGameID = new string([.. agGameID.Replace(" ", "").ToUpper().Where(
                        c => c >= 32 && c <= 127 && !unwanted.Contains(c))]);
                    agGameID = agGameID.Left(5);
                    // always use v2.936
                    agIntVersion = new() {
                        Index = AGIVersion.v2936,
                    };
                }
                else {
                    // directory is not a valid AGI directory
                    ClearGameState();
                    WinAGIException wex = new(EngineResourceByNum(504).Replace(ARG1, argval.GameDir)) {
                        HResult = WINAGI_ERR + 504,
                    };
                    wex.Data["baddir"] = argval.GameDir;
                    throw wex;
                }
            }
            agGameFile = Path.Combine(agGameDir, agGameID + ".wag");
            // delete any existing game file
            if (File.Exists(agGameFile)) {
                SafeFileDelete(agGameFile);
            }
            // create new wag file
            try {
                agGameProps = new SettingsFile(agGameFile, FileMode.Create);
            }
            catch (Exception e) {
                ClearGameState();
                // file creation error
                WinAGIException wex = new(EngineResourceByNum(546).Replace(ARG1, e.Message)) {
                    HResult = WINAGI_ERR + 546,
                };
                wex.Data["exception"] = e;
                throw wex;
            }
            agGameProps.Lines.Add("# WinAGI Game Property File");
            agGameProps.Lines.Add("#");
            agGameProps.WriteSetting("General", "WinAGIVersion", WINAGI_VERSION);
            agGameProps.WriteSetting("General", "GameID", agGameID);
            // add interpreter version
            WriteGameSetting("General", "Interpreter", agIntVersion.VersionString);
            // update all newgame properties
            agSrcResDirName = argval.SrcResDirName;
            agSrcFileExt = argval.SrcExt;
            agIncludeIDs = argval.IncludeIDs;
            agIncludeReserved = argval.IncludeReserved;
            agIncludeGlobals = argval.IncludeGlobals;
            agSierraSyntax = argval.SierraSyntax;
            agCodePage = argval.CodePage;
            // finish the game load
            try {
                // if importing as SierraSyntax, pass along ImportResource property
                return FinishGameLoad(OpenGameMode.Directory, importresources);
            }
            catch (Exception ex) {
                // return an error
                WinAGIEventInfo retval = new() {
                    Type = EventType.GameLoadError,
                    Data = ex
                };
                return retval;
            }
        }

        /// <summary>
        /// Opens a WinAGI game file (must be passed as a full length file name).
        /// If successful, warning/load info is passed back.
        /// If fails, exception is returned.
        /// </summary>
        /// <param name="GameWAG"></param>
        /// <returns></returns>
        private WinAGIEventInfo OpenGameWAG(string GameWAG) {
            string version;

            if (!File.Exists(GameWAG)) {
                // invalid wag -
                // is the file missing?, or the directory?
                if (Directory.Exists(Path.GetDirectoryName(GameWAG))) {
                    // it's a missing file - return wagfile as error string
                    WinAGIException wex = new(EngineResourceByNum(529).Replace(
                        ARG1, GameWAG)) {
                        HResult = WINAGI_ERR + 529,
                    };
                    wex.Data["badfile"] = GameWAG;
                    // return an error
                    WinAGIEventInfo retval = new() {
                        Type = EventType.GameLoadError,
                        Data = wex
                    };
                    return retval;
                }
                else {
                    // it's an invalid or missing directory - return directory as error string
                    WinAGIException wex = new(EngineResourceByNum(504).Replace(ARG1, Path.GetDirectoryName(GameWAG))) {
                        HResult = WINAGI_ERR + 504,
                    };
                    wex.Data["baddir"] = Path.GetDirectoryName(GameWAG);
                    // return an error
                    WinAGIEventInfo retval = new() {
                        Type = EventType.GameLoadError,
                        Data = wex
                    };
                    return retval;
                }
            }
            // set game file property
            agGameFile = GameWAG;
            // set game directory
            agGameDir = Path.GetDirectoryName(GameWAG);
            // check for readonly (not allowed)
            if ((File.GetAttributes(agGameFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                WinAGIException wex = new(EngineResourceByNum(539).Replace(
                    ARG1, GameWAG)) {
                    HResult = WINAGI_ERR + 539,
                };
                wex.Data["badfile"] = agGameFile;
                // return an error
                WinAGIEventInfo retval = new() {
                    Type = EventType.GameLoadError,
                    Data = wex
                };
                return retval;
            }
            try {
                // open the WAG
                agGameProps = new SettingsFile(agGameFile, FileMode.Open);
            }
            catch (Exception e) {
                // reset game variables
                ClearGameState();
                WinAGIException wex = new(EngineResourceByNum(540).Replace(
                    ARG1, e.HResult.ToString()).Replace(
                    ARG2, e.Message)) {
                    HResult = WINAGI_ERR + 540,
                };
                wex.Data["exception"] = e;
                wex.Data["badfile"] = GameWAG;
                // return an error
                WinAGIEventInfo retval = new() {
                    Type = EventType.GameLoadError,
                    Data = wex
                };
                return retval;
            }
            // check to see if it's valid
            version = agGameProps.GetSetting("General", "WinAGIVersion", "");
            if (version != WINAGI_VERSION) {
                if (version.Left(4) == "1.2." || (version.Left(2) == "2.")) {
                    // any v1.2.x or 2.x is ok, but user will need to update
                    // let calling function know an upgrade is occurring
                    WinAGIEventInfo loadInfo = new() {
                        Type = EventType.Info,
                        Text = version,
                        InfoType = InfoType.Validating
                    };
                    OnLoadGameStatus(loadInfo);
                    // update the WinAGI version
                    WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION, "", true);
                }
                else {
                    // clear game variables
                    ClearGameState();
                    // invalid wag file, or invalid wag version
                    if (version.Length == 0) {
                        WinAGIException wex = new(EngineResourceByNum(555)) {
                            HResult = WINAGI_ERR + 555,
                        };
                        wex.Data["badfile"] = GameWAG;
                        wex.Data["badversion"] = version;
                        // return an error
                        WinAGIEventInfo retval = new() {
                            Type = EventType.GameLoadError,
                            Data = wex
                        };
                        return retval;
                    }
                    else {
                        WinAGIException wex = new(EngineResourceByNum(530).Replace(
                            ARG1, version)) {
                            HResult = WINAGI_ERR + 530,
                        };
                        wex.Data["badversion"] = version;
                        // return an error
                        WinAGIEventInfo retval = new() {
                            Type = EventType.GameLoadError,
                            Data = wex
                        };
                        return retval;
                    }
                }
            }
            // get gameID
            agGameID = agGameProps.GetSetting("General", "GameID", "");
            // if an id is found, keep going
            if (agGameID.Length > 0) {
                // trim it to 6 characters (older versions of WinAGI allowed longer v2 IDs
                if (agGameID.Length > 6) {
                    agGameID = agGameID[..6];
                    agGameProps.WriteSetting("General", "GameID", agGameID);
                }
                // got ID; now get interpreter version from propfile
                string vertext = agGameProps.GetSetting("General", "Interpreter", "");
                int verIndex = Array.IndexOf(IntVersions, vertext);
                // validate it
                if (verIndex == -1) {
                    // clear game variables
                    ClearGameState();
                    // invalid int version inside wag file
                    WinAGIException wex = new(EngineResourceByNum(538).Replace(
                        ARG1, vertext)) {
                        HResult = WINAGI_ERR + 538,
                    };
                    wex.Data["badversion"] = vertext;
                    // return an error
                    WinAGIEventInfo retval = new() {
                        Type = EventType.GameLoadError,
                        Data = wex
                    };
                    return retval;
                }
                agIntVersion = new AGIVersionInfo() {
                    Index = (AGIVersion)verIndex,
                };
            }
            else {
                // missing GameID in wag file - make user address it
                // save wagfile name as error string
                // clear game variables
                ClearGameState();
                // invalid wag file
                WinAGIException wex = new(EngineResourceByNum(537)) {
                    HResult = WINAGI_ERR + 537,
                };
                wex.Data["badfile"] = GameWAG;
                // return an error
                WinAGIEventInfo retval = new() {
                    Type = EventType.GameLoadError,
                    Data = wex
                };
                return retval;
            }
            // if a valid wag file was found, we now have agGameID, agGameFile
            // and correct interpreter version;
            try {
                // finish the game load
                return FinishGameLoad(OpenGameMode.File);
            }
            catch (Exception ex) {
                // return an error
                WinAGIEventInfo retval = new() {
                    Type = EventType.GameLoadError,
                    Data = ex
                };
                return retval;
            }
        }

        /// <summary>
        /// Finishes game load. Mode determines whether opening by wag file or
        /// extracting from Sierra game files. If successful, warning/load info
        /// is passed back. If fails, exception is thrown.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private WinAGIEventInfo FinishGameLoad(OpenGameMode mode, bool importresources = false) {
            // when mode = New, it means opening a copy of a template game
            // so assumption is all WinAGI resources and settings are known
            // just like opening a game from WAG file

            WinAGIEventInfo retval = new() {
                Type = EventType.Info,
                InfoType = InfoType.Done,
            };
            // provide feedback to calling function
            WinAGIEventInfo loadInfo = new();

            // update resource directory
            switch (mode) {
            case OpenGameMode.File:
                loadInfo.InfoType = InfoType.PropertyFile;
                OnLoadGameStatus(loadInfo);
                // get resdir before loading resources
                agSrcResDirName = agGameProps.GetSetting("General", "ResDir", "");
                break;
            case OpenGameMode.Directory:
                WriteGameSetting("General", "ResDir", agSrcResDirName);
                // if a set.game.id command is found during decompilation
                // it will be used as the suggested ID
                DecodeGameID = "";
                break;
            case OpenGameMode.New:
                // for new-from-template use new resdir
                WriteGameSetting("General", "ResDir", agSrcResDirName);
                break;
            }
            // now create full resdir from name
            agSrcResDir = Path.Combine(agGameDir, agSrcResDirName);
            bool decompall = agSierraSyntax && !Directory.Exists(agSrcResDir);
            // ensure resource directory exists
            if (!Directory.Exists(agSrcResDir)) {
                try {
                    Directory.CreateDirectory(agSrcResDir);
                }
                catch (Exception) {
                    // if can't create the resources directory
                    // note the problem as a warning
                    loadInfo.ResType = AGIResType.Game;
                    loadInfo.Type = EventType.ResourceWarning;
                    loadInfo.ID = "RW03";
                    loadInfo.Text = EngineResources.RW03.Replace(
                        ARG1, agSrcResDirName);
                    LoadEventStatus(mode, loadInfo);
                    // use game directory
                    agSrcResDir = agGameDir;
                    agSrcResDirName = "";
                    WriteGameSetting("General", "ResDir", agSrcResDirName);
                    // set warning
                    agLoadWarnings = true;
                }
            }
            switch (mode) {
            case OpenGameMode.File:
            case OpenGameMode.New:
                try {
                    // get rest of game properties
                    GetGameProperties();
                }
                catch (Exception e) {
                    // note the problem as a warning
                    loadInfo.ResType = AGIResType.Game;
                    loadInfo.Type = EventType.ResourceWarning;
                    loadInfo.ID = "RW04";
                    loadInfo.Text = EngineResources.RW04.Replace(
                        ARG1, e.HResult.ToString()).Replace(
                        ARG2, e.Message);
                    LoadEventStatus(mode, loadInfo);
                    // set warning
                    agLoadWarnings = true;
                }
                break;
            case OpenGameMode.Directory:
                // game file is new; force save with all values
                for (int i = 0; i < 16; i++) {
                    agGameProps.WriteSetting("Palette", "Color" + i.ToString(), DefaultPalette[i]);
                }
                agGameProps.WriteSetting("General", "Description", "");
                agGameProps.WriteSetting("General", "Designer", "");
                agGameProps.WriteSetting("General", "About", "");
                agGameProps.WriteSetting("General", "GameVersion", "");
                agGameProps.WriteSetting("General", "PlatformType", 0);
                agGameProps.WriteSetting("General", "Platform", "");
                agGameProps.WriteSetting("General", "DOSExec", "");
                agGameProps.WriteSetting("General", "PlatformOpts", "");
                agGameProps.WriteSetting("General", "CodePage", agCodePage);
                agGameProps.WriteSetting("Includes", "IncludeReserved", agIncludeReserved);
                agGameProps.WriteSetting("Includes", "IncludeIDs", agIncludeIDs);
                agGameProps.WriteSetting("Includes", "IncludeGlobals", agIncludeGlobals);
                agGameProps.WriteSetting("General", "UseLE", agUseLE);
                agGameProps.WriteSetting("General", "SierraSyntax", agSierraSyntax);
                agGameProps.WriteSetting("General", "SourceFileExt", agSrcFileExt);
                // default to now
                agLastEdit = DateTime.Now;
                break;
            }
            // VOL files, DIR files and Logic, Picture, Sound, View resources
            if (agSierraSyntax && importresources) {
                // create a blank game, then attempt to import resources
                // from 'LGC', 'PIC', 'SND' and 'VIEW' subdirectories
                // logic source code directory will be 'SRC'
                string filename = "";
                try {
                    filename = Path.Combine(agGameDir, "VOL.0");
                    File.Create(filename).Close();
                    filename = Path.Combine(agGameDir, "LOGDIR");
                    File.WriteAllBytes(filename, [0xff, 0xff, 0xff]);
                    filename = Path.Combine(agGameDir, "PICDIR");
                    File.WriteAllBytes(filename, [0xff, 0xff, 0xff]);
                    filename = Path.Combine(agGameDir, "SNDDIR");
                    File.WriteAllBytes(filename, [0xff, 0xff, 0xff]);
                    filename = Path.Combine(agGameDir, "VIEWDIR");
                    File.WriteAllBytes(filename, [0xff, 0xff, 0xff]);
                }
                catch (Exception ex) {
                    // note the problem as an error
                    WinAGIException wex = new(EngineResourceByNum(541).Replace(
                        ARG1, Path.GetFileName(filename)).Replace(
                        ARG2, ex.HResult.ToString()).Replace(
                        ARG3, ex.Message)) {
                        HResult = WINAGI_ERR + 541
                    };
                    wex.Data["exception"] = ex;
                    wex.Data["dirfile"] = Path.GetFileName(filename);
                    throw wex;
                }
                // import resources from Sierra's default directories
                agLoadWarnings |= ImportSierraResources();

                // check for WORDS.TOK
                if (!File.Exists(Path.Combine(agGameDir, "WORDS.TOK"))) {
                    // if there's a source file, try compiling it
                    if (File.Exists(Path.Combine(agSrcResDir, "words.txt"))) {
                        loadInfo.Type = EventType.Info;
                        loadInfo.InfoType = InfoType.Resources;
                        loadInfo.ResType = AGIResType.Words;
                        LoadEventStatus(OpenGameMode.Directory, loadInfo);
                        WordList tmp = new(Path.Combine(agSrcResDir, "words.txt"), true);
                        // check errors/warnings here, save will clear them
                        if (tmp.Error != ResourceErrorType.NoError) {
                            // note the problem as a warning
                            AddLoadError(mode, this, AGIResType.Words, 0, tmp.Error, tmp.ErrData);
                            agLoadWarnings = true;
                        }
                        if (tmp.Warnings != 0) {
                            // note the problem as a warning
                            AddLoadWarning(mode, this, AGIResType.Words, 0, tmp.Warnings, tmp.WarnData);
                            agLoadWarnings = true;
                        }
                        try {
                            tmp.Save(Path.Combine(agGameDir, "WORDS.TOK"));
                        }
                        catch (Exception ex) {
                            AddLoadError(mode, this, AGIResType.Words, 0,
                                ResourceErrorType.WordsTokAccessError, [ex.Message]);
                            // use a blank file if save fails
                            WordList blank = new();
                            blank.Save(Path.Combine(agGameDir, "WORDS.TOK"));
                        }
                        // compiling a sierra words.txt may generate warnings
                        if (tmp.Warnings != 0) {
                            // note the problem as a warning
                            AddLoadWarning(mode, this, AGIResType.Words, 0, tmp.Warnings, tmp.WarnData);
                            agLoadWarnings = true;
                        }
                    }
                    else {
                        // create a blank file
                        WordList tmp = new();
                        tmp.Save(Path.Combine(agGameDir, "WORDS.TOK"));
                    }
                }

                // check for OBJECT
                if (!File.Exists(Path.Combine(agGameDir, "OBJECT"))) {
                    // if there's a source file, try compiling it
                    if (File.Exists(Path.Combine(agSrcResDir, "object.txt"))) {
                        loadInfo.Type = EventType.Info;
                        loadInfo.InfoType = InfoType.Resources;
                        loadInfo.ResType = AGIResType.Objects;
                        LoadEventStatus(OpenGameMode.Directory, loadInfo);
                        InventoryList tmp = new(Path.Combine(agSrcResDir, "object.txt"), true);
                        // check errors/warnings here, save will clear them
                        if (tmp.Error != ResourceErrorType.NoError) {
                            AddLoadError(mode, this, AGIResType.Objects, 0, tmp.Error, tmp.ErrData);
                            agLoadWarnings = true;
                        }
                        // check for warnings
                        if (agInvObj.Warnings != 0) {
                            // note the problem as a warning
                            AddLoadWarning(mode, this, AGIResType.Objects, 0, tmp.Warnings, tmp.WarnData);
                            agLoadWarnings = true;
                        }
                        try {
                            tmp.Save(Path.Combine(agGameDir, "OBJECT"));
                        }
                        catch (Exception ex) {
                            AddLoadError(mode, this, AGIResType.Objects, 0,
                                ResourceErrorType.ObjectAccessError, [ex.Message]);
                            // use a blank file if save fails
                            InventoryList blank = [];
                            blank.Save(Path.Combine(agGameDir, "OBJECT"));
                        }
                    }
                    else {
                        // create a blank file
                        InventoryList tmp = [];
                        tmp.Save(Path.Combine(agGameDir, "OBJECT"));
                    }
                }
            }
            else {
                try {
                    agLoadWarnings |= ExtractResources(this, mode);
                }
                catch {
                    throw;
                }
            }

            // WORDS.TOK file
            loadInfo.Type = EventType.Info;
            loadInfo.InfoType = InfoType.Resources;
            loadInfo.ResType = AGIResType.Words;
            LoadEventStatus(mode, loadInfo);
            agVocabWords = new WordList(this);
            agVocabWords.Load(Path.Combine(agGameDir, "WORDS.TOK"));
            // get description, if there is one
            agVocabWords.Description = agGameProps.GetSetting("WORDS.TOK", "Description", "", true);
            if (agVocabWords.Error != ResourceErrorType.NoError) {
                // note the problem as a warning
                AddLoadError(mode, this, AGIResType.Words, 0, agVocabWords.Error, agVocabWords.ErrData);
                agLoadWarnings = true;
            }
            if (agVocabWords.Warnings != 0) {
                // note the problem as a warning
                AddLoadWarning(mode, this, AGIResType.Words, 0, agVocabWords.Warnings, agVocabWords.WarnData);
                agLoadWarnings = true;
            }

            // OBJECT file
            loadInfo.Type = EventType.Info;
            loadInfo.InfoType = InfoType.Resources;
            loadInfo.ResType = AGIResType.Objects;
            LoadEventStatus(mode, loadInfo);
            agInvObj = new InventoryList(this);
            agInvObj.Load();
            // get description, if there is one
            agInvObj.Description = agGameProps.GetSetting("OBJECT", "Description", "", true);
            // check for errors
            if (agInvObj.Error != ResourceErrorType.NoError) {
                AddLoadError(mode, this, AGIResType.Objects, 0, agInvObj.Error, agInvObj.ErrData);
                agLoadWarnings = true;
            }
            // check for warnings
            if (agInvObj.Warnings != 0) {
                // note the problem as a warning
                AddLoadWarning(mode, this, AGIResType.Objects, 0, agInvObj.Warnings, agInvObj.WarnData);
                agLoadWarnings = true;
            }

            // include files
            if (agIncludeReserved) {
                // load reserved defines
                agReservedDefines = new(this);
            }
            if (BkgdTasks.updating) {
                // if there's a globals.txt file, move it
                SafeFileMove(Path.Combine(agGameDir, "globals.txt"), Path.Combine(agSrcResDir, "globals.txt"), true);
            }

            // assign a globals object
            agGlobals = new(this);
            if (agIncludeGlobals) {
                // if file exists, load it
                if (File.Exists(agGlobals.ResFile)) {
                    // load globals file
                    agGlobals.LoadDefines();
                }
                else {
                    // save a blank list
                    agGlobals.Save();
                }
                if (agGlobals.Error != ResourceErrorType.NoError) {
                    // note the problem as a warning
                    AddLoadError(mode, this, AGIResType.Globals, 0, agGlobals.Error, agGlobals.ErrData);
                    agLoadWarnings = true;
                }
                if (agGlobals.Warnings != 0) {
                    // note the problem as a warning
                    AddLoadWarning(mode, this, AGIResType.Globals, 0, agGlobals.Warnings, agGlobals.WarnData);
                    agLoadWarnings = true;
                }
            }

            // adust commands based on AGI version
            CorrectCommands(agIntVersion.Index);

            // Logic source code files
            if (agSierraSyntax && mode == OpenGameMode.Directory && decompall) {
                // if using sierra syntax and importing from a directory, create default
                // sysdefs.h then decode all logics (gamedefs.h added after all logics
                // are decoded)
                try {
                    File.WriteAllText(Path.Combine(agSrcResDir, "sysdefs.h"), EngineResources.SYSDEFS);
                }
                catch (Exception) {
                    // for now, ignore errors
                }
                agLoadWarnings |= DecodeAllSierraLogics(this);

            }
            else {
                // check for sourcefile errors,  decompile warnings, TODO entries and validate logic CRC values
                // (this has to happen AFTER everything is loaded otherwise some resources
                // that are referenced in the logics won't exist yet, and will give a
                // false warning about missing resources)
                foreach (Logic tmpLog in agLogs) {
                    // the CRC/source check depends on mode
                    switch (mode) {
                    case OpenGameMode.File:
                    case OpenGameMode.New:
                        // opening existing wag file - check CRC
                        loadInfo.ResType = AGIResType.Logic;
                        loadInfo.ResNum = tmpLog.Number;
                        loadInfo.Type = EventType.Info;
                        loadInfo.InfoType = InfoType.CheckCRC;
                        LoadEventStatus(mode, loadInfo);
                        // if there is a source file, need to verify source CRC value
                        if (File.Exists(tmpLog.SourceFile)) {
                            // recalculate the CRC value for this sourcefile by loading the source
                            tmpLog.LoadSource();
                            if (tmpLog.SourceError != ResourceErrorType.LogicSourceAccessError) {
                                // check it for TODO items
                                List<WinAGIEventInfo> TODOs = ExtractTODO(tmpLog.Number, tmpLog.SourceText, tmpLog.ID);
                                foreach (WinAGIEventInfo tmpInfo in TODOs) {
                                    LoadEventStatus(mode, tmpInfo);
                                }
                                // and decomp warnings
                                List<WinAGIEventInfo> DecompWarnings = ExtractDecompWarn(tmpLog.Number, tmpLog.SourceText, tmpLog.ID);
                                foreach (WinAGIEventInfo tmpInfo in DecompWarnings) {
                                    LoadEventStatus(mode, tmpInfo);
                                }
                                // and compiler warnings and errors
                                List<WinAGIEventInfo> CompIssues = tmpLog.LoadWarnings();
                                foreach (WinAGIEventInfo tmpInfo in CompIssues) {
                                    LoadEventStatus(mode, tmpInfo);
                                }
                            }
                            // then unload it
                            tmpLog.Unload();
                        }
                        else {
                            // no source - decompile it now
                            // force decompile
                            tmpLog.LoadSource(true);
                            // unload the logic after decompile is done
                            tmpLog.Unload();
                        }
                        // the resource and the source file have now been checked; display
                        // source error if applicable (decode errors would have been
                        // caught during LoadSource)
                        if (tmpLog.SourceError != ResourceErrorType.NoError &&
                            tmpLog.SourceError != ResourceErrorType.LogicSourceDecompileError) {
                            AddLoadError(mode, this, AGIResType.Logic, tmpLog.Number, tmpLog.SourceError, tmpLog.ErrData);
                            agLoadWarnings = true;
                        }
                        break;
                    case OpenGameMode.Directory:
                        // if extracting from a directory, none of the logics have source
                        // code; they should all be extracted here, and marked as clean
                        // (if error occurs in decoding, the error gets caught, noted, and
                        // a blank source file is used instead)

                        // decompiling
                        loadInfo.ResType = AGIResType.Logic;
                        loadInfo.ResNum = tmpLog.Number;
                        loadInfo.Type = EventType.Info;
                        loadInfo.InfoType = InfoType.Decompiling;
                        LoadEventStatus(mode, loadInfo);
                        // force decompile unless importing sierra resources
                        tmpLog.LoadSource();
                        if (tmpLog.SourceError != ResourceErrorType.NoError &&
                            tmpLog.SourceError != ResourceErrorType.LogicSourceDecompileError) {
                            AddLoadError(mode, this, AGIResType.Logic, tmpLog.Number, tmpLog.SourceError, tmpLog.ErrData);
                            agLoadWarnings = true;
                        }
                        // unload the logic after decompile is done
                        tmpLog.Unload();
                        break;
                    }
                }
                // get module warnings and errors
                List<WinAGIEventInfo> modWarnings = Logics.LoadModWarnings();
                foreach (WinAGIEventInfo tmpInfo in modWarnings) {
                    LoadEventStatus(mode, tmpInfo);
                }

            }

            // if decompiling AND a new ID found, use it
            if (mode == OpenGameMode.Directory && DecodeGameID.Length != 0) {
                agGameID = DecodeGameID;
                DecodeGameID = "";
                SafeFileMove(agGameFile, Path.Combine(agGameDir, agGameID + ".wag"), true);
                agGameFile = Path.Combine(agGameDir, agGameID + ".wag");
                agGameProps.Filename = agGameFile;
                WriteGameSetting("General", "GameID", agGameID);
            }

            // everything loaded OK; tidy things up before exiting
            loadInfo.Type = EventType.Info;
            loadInfo.InfoType = InfoType.Finalizing;
            LoadEventStatus(mode, loadInfo);
            // force id reset
            setIDs = false;
            SetResourceIDs(this);
            if (mode == OpenGameMode.Directory) {
                // write create date
                WriteGameSetting("General", "LastEdit", agLastEdit.ToString());
            }
            agGameProps.Save();
            // TODO: add file watcher support
            //agFileWatcher = new(agGameDir);
            //agFileWatcher.Enabled = true;
            return retval;
        }

        private bool ImportSierraResources() {
            bool retval = false;

            // try to load logics
            WinAGIEventInfo loadInfo = new()
            {
                Type = EventType.Info,
                InfoType = InfoType.Resources,
                ResType = AGIResType.Logic
            };
            LoadEventStatus(OpenGameMode.Directory, loadInfo);

            for (int i = 0; i < 256; i++) {
                string path = Path.Combine(agGameDir, @"LGC\RM." + i.ToString());
                try {
                    if (File.Exists(path)) {
                        loadInfo.ResNum = i;
                        LoadEventStatus(OpenGameMode.Directory, loadInfo);
                        Logic logic = new();
                        // import it as a resource, but don't load source code yet
                        logic.ImportResource(path);
                        // adjust ID to match sierra's default sourcefile naming convention
                        logic.ID = "RM" + i.ToString();
                        agLogs.Add((byte)i, logic);
                        if (agLogs[i].Error != ResourceErrorType.NoError) {
                            AddLoadError(OpenGameMode.Directory, this, AGIResType.Picture,
                                (byte)i, agLogs[i].Error, agLogs[i].ErrData);
                            retval = true;
                        }
                        // logics don't have warnings
                    }
                    else {
                        // check for a source file
                        if (File.Exists(Path.Combine(agSrcResDir, "RM" + i.ToString() + ".CG"))) {
                            loadInfo.ResNum = i;
                            LoadEventStatus(OpenGameMode.Directory, loadInfo);
                            Logic logic = new() {
                                ID = "RM" + i.ToString()
                            };
                            agLogs.Add((byte)i, logic);
                        }
                    }
                }
                catch (Exception ex) {
                    // add error if it can't be loaded/added
                    // note the problem as an error
                    AddLoadError(OpenGameMode.Directory, this, AGIResType.Logic,
                        (byte)i, ResourceErrorType.SierraResourceError, [ex.Message]);
                }
                // verify logic0 was loaded; if not, create a new blank
                if (!Logics.Contains(0)) {
                    // force logic0
                    Logics.Add(0);
                    Logics[0].LoadSource(true);
                }
            }
            // try to load pictures
            loadInfo.ResType = AGIResType.Picture;
            for (int i = 0; i < 256; i++) {
                string path = Path.Combine(agGameDir, @"PIC\PIC." + i.ToString());
                try {
                    if (File.Exists(path)) {
                        loadInfo.ResNum = i;
                        LoadEventStatus(OpenGameMode.Directory, loadInfo);
                        Picture picture = new();
                        picture.Import(path);
                        // adjust ID to match sierra convention
                        picture.ID = "PIC" + i.ToString();
                        agPics.Add((byte)i, picture);
                        if (agPics[i].Error != ResourceErrorType.NoError) {
                            AddLoadError(OpenGameMode.Directory, this, AGIResType.Picture,
                                (byte)i, agPics[i].Error, agPics[i].ErrData);
                            retval = true;
                        }
                        if (agPics[i].Warnings != 0) {
                            AddLoadWarning(OpenGameMode.Directory, this, AGIResType.Picture,
                                (byte)i, agPics[i].Warnings, agPics[i].WarnData);
                            retval = true;
                        }
                    }
                }
                catch (Exception ex) {
                    // add error if it can't be loaded/added
                    // note the problem as an error
                    AddLoadError(OpenGameMode.Directory, this, AGIResType.Picture,
                        (byte)i, ResourceErrorType.SierraResourceError, [ex.Message]);
                }
            }
            // try to load sounds
            loadInfo.ResType = AGIResType.Sound;
            for (int i = 0; i < 256; i++) {
                string path = Path.Combine(agGameDir, @"SND\SND." + i.ToString());
                try {
                    if (File.Exists(path)) {
                        loadInfo.ResNum = i;
                        LoadEventStatus(OpenGameMode.Directory, loadInfo);
                        Sound sound = new();
                        sound.Import(path);
                        // change ID to match sierra convention
                        sound.ID = "SND" + i.ToString();
                        agSnds.Add((byte)i, sound);
                        if (agSnds[i].Error != ResourceErrorType.NoError) {
                            AddLoadError(OpenGameMode.Directory, this, AGIResType.Sound,
                                (byte)i, agSnds[i].Error, agSnds[i].ErrData);
                            retval = true;
                        }
                        if (agSnds[i].Warnings != 0) {
                            AddLoadWarning(OpenGameMode.Directory, this, AGIResType.Sound,
                                (byte)i, agSnds[i].Warnings, agSnds[i].WarnData);
                            retval = true;
                        }
                    }
                }
                catch (Exception ex) {
                    // add error if it can't be loaded/added
                    // note the problem as an error
                    AddLoadError(OpenGameMode.Directory, this, AGIResType.Sound,
                        (byte)i, ResourceErrorType.SierraResourceError, [ex.Message]);
                }
            }
            // try to load views
            loadInfo.ResType = AGIResType.View;
            for (int i = 0; i < 256; i++) {
                string path = Path.Combine(agGameDir, @"VIEW\VIEW." + i.ToString());
                try {
                    if (File.Exists(path)) {
                        loadInfo.ResNum = i;
                        LoadEventStatus(OpenGameMode.Directory, loadInfo);
                        Engine.View view = new();
                        view.Import(path);
                        // change ID to match sierra convention
                        view.ID = "VIEW" + i.ToString();
                        agViews.Add((byte)i, view);
                        if (agViews[i].Error != ResourceErrorType.NoError) {
                            AddLoadError(OpenGameMode.Directory, this, AGIResType.View,
                                (byte)i, agViews[i].Error, agViews[i].ErrData);
                            retval = true;
                        }
                        if (agViews[i].Warnings != 0) {
                            AddLoadWarning(OpenGameMode.Directory, this, AGIResType.View,
                                (byte)i, agViews[i].Warnings, agViews[i].WarnData);
                            retval = true;
                        }
                    }
                }
                catch (Exception ex) {
                    // add error if it can't be loaded/added
                    // note the problem as an error
                    AddLoadError(OpenGameMode.Directory, this, AGIResType.View,
                        (byte)i, ResourceErrorType.SierraResourceError, [ex.Message]);
                }
            }
            return retval;
        }

        internal static void LoadEventStatus(OpenGameMode mode, WinAGIEventInfo loadInfo) {
            switch (mode) {
            case OpenGameMode.New:
                OnNewGameStatus(loadInfo);
                break;
            default:
                OnLoadGameStatus(loadInfo);
                break;
            }
        }

        /// <summary>
        /// Clears basic game variables for a blank, unopened game.
        /// </summary>
        internal void ClearGameState() {
            // resource objects
            agLogs = new Logics(this);
            agPics = new Pictures(this);
            agSnds = new Sounds(this);
            agViews = new Views(this);
            agInvObj = new InventoryList(this);
            agVocabWords = new WordList(this);
            agIncludeFiles = [];
            agReservedDefines = null;
            agGlobals = null;// new GlobalList(this);
            // clear out game properties
            agGameID = "";
            agIntVersion = new() {
                Index = AGIVersion.v2917,
            };
            agGameProps = new SettingsFile();
            agLastEdit = new DateTime();
            agSierraSyntax = false;
            agPowerPack = false;
            agLoadWarnings = false;
            // reset directories
            agGameDir = "";
            agSrcResDir = "";
            agGameFile = "";
            agSrcFileExt = defSrcExt;
            // clear dev properties
            agDescription = "";
            agDesigner = "";
            agGameVersion = "";
            agGameAbout = "";
            agSrcResDirName = "";
            agPlatformType = PlatformType.None;
            agPlatformFile = "";
            agPlatformOpts = "";
            agDOSExec = "";
            // other properties
            DecodeGameID = "";
            IndentSize = 4;
        }

        /// <summary>
        /// This function will determine if the directory is a valid Sierra AGI
        /// game directory. It also sets the gameID, if one is found, and the
        /// version3 flag.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public bool IsValidGameDir(string directory, ref bool isV3) {
            string filename;
            byte[] bChunk = new byte[6];
            FileStream fsCOM;
            int dirCount;

            try {
                // search for 'DIR' files
                dirCount = Directory.EnumerateFiles(directory, "*DIR").Count();
            }
            catch (Exception) {
                // if error, assume NOT a valid directory
                return false;
            }
            if (dirCount > 0) {
                // this might be an AGI game directory-
                // if exactly four dir files
                if (dirCount == 4) {
                    // check names
                    if (!File.Exists(Path.Combine(directory, "LOGDIR"))) {
                        return false;
                    }
                    if (!File.Exists(Path.Combine(directory, "PICDIR"))) {
                        return false;
                    }
                    if (!File.Exists(Path.Combine(directory, "SNDDIR"))) {
                        return false;
                    }
                    if (!File.Exists(Path.Combine(directory, "VIEWDIR"))) {
                        return false;
                    }
                    // DIR files check out - assume it's a v2 game

                    // check for at least one VOL file
                    if (File.Exists(Path.Combine(directory, "VOL.0"))) {
                        // clear version3 flag
                        isV3 = false;
                        // clear ID
                        agGameID = "";
                        // look for loader file to find ID
                        foreach (string loader in Directory.EnumerateFiles(directory, "*.COM")) {
                            // open file and get chunk
                            string chunk = new(' ', 6);
                            try {
                                using (fsCOM = new FileStream(loader, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                                    // see if the word 'LOADER' is at position 3 of the file
                                    fsCOM.Position = 3;
                                    fsCOM.Read(bChunk, 0, 6);
                                    chunk = Encoding.UTF8.GetString(bChunk);
                                    fsCOM.Dispose();
                                    // if this is a Sierra loader
                                    if (chunk == "LOADER") {
                                        // determine ID to use based on loader filename
                                        filename = Path.GetFileName(loader);
                                        if (loader != "SIERRA.COM") {
                                            // use this filename as ID
                                            agGameID = filename.Left(filename.Length - 4).ToUpper();
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) {
                                // ignore if file is readonly
                                // (or any other access error)
                                Debug.Print(ex.Message);
                            }
                        }
                        // if no loader file found (looped through all files, no luck)
                        if (agGameID.Length == 0) {
                            // use default
                            agGameID = "AGI";
                        }
                    }
                }
                else if (dirCount == 1) {
                    // if only one, it's probably v3 game
                    filename = Path.GetFileName(Directory.GetFiles(directory, "*DIR")[0].ToUpper());
                    agGameID = filename.Left(filename.IndexOf("DIR"));
                    // check for matching VOL file;
                    if (File.Exists(Path.Combine(directory, agGameID + "VOL.0"))) {
                        // set version3 flag
                        isV3 = true;
                    }
                    else {
                        // if no vol file, assume not valid
                        return false;
                    }
                }
                // DIR/VOL files found; ID set; look for OBJECT/WORDS.TOK
                if (!File.Exists(Path.Combine(directory, "OBJECT"))) {
                    return false;
                }
                if (!File.Exists(Path.Combine(directory, "WORDS.TOK"))) {
                    return false;
                }
                // all necessary files exist; this is a valid AGI directory
                return true;
            }
            // no valid files found; not an AGI directory
            return false;
        }

        /// <summary>
        /// Writes a string value to this game's WinAGI Game File but does not save
        /// the file.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        internal void WriteSettingNoSave(string Section, string Key, string Value, string Group = "") {
            agGameProps.WriteSetting(Section, Key, Value, Group);
        }

        /// <summary>
        /// Writes a string value to this game's WinAGI Game File. Use the save
        /// parameter to specify whether the file should be saved immediately.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        internal void WriteGameSetting(string Section, string Key, string Value, string Group = "", bool save = false) {
            agGameProps.WriteSetting(Section, Key, Value, Group);
            if (!Key.Equals("lastedit", StringComparison.CurrentCultureIgnoreCase) &&
                !Key.Equals("winagiversion", StringComparison.CurrentCultureIgnoreCase) &&
                !Key.Equals("palette", StringComparison.CurrentCultureIgnoreCase)) {
                agLastEdit = DateTime.Now;
            }
            if (save) {
                agGameProps.Save();
            }
        }

        /// <summary>
        /// Writes an integer value to this game's WinAGI Game File. File is automatically
        /// saved after writing.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        internal void WriteGameSetting(string Section, string Key, int Value, string Group = "", bool save = false) {
            WriteGameSetting(Section, Key, Value.ToString(), Group, save);
        }

        /// <summary>
        /// Writes a boolean value to this game's WinAGI Game File. File is automatically 
        /// saved after writing.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        internal void WriteGameSetting(string Section, string Key, bool Value, string Group = "", bool save = false) {
            WriteGameSetting(Section, Key, Value.ToString(), Group, save);
        }

        /// <summary>
        /// Writes a float value to this game's WinAGI Game File. File is automatically 
        /// saved after writing.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        internal void WriteGameSetting(string Section, string Key, float Value, string Group = "", bool save = false) {
            WriteGameSetting(Section, Key, Value.ToString(), Group, save);
        }

        /// <summary>
        /// Provides calling programs a way to write property values to the WinAGI
        /// Game File.</summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        public void WriteProperty(string Section, string Key, string Value, string Group = "", bool save = false) {
            WriteGameSetting(Section, Key, Value, Group, save);
        }

        /// <summary>
        /// Reads the game properties from the game's WinAGI Game file as
        /// part of the initial game opening method.
        /// </summary>
        internal void GetGameProperties() {
            // what's loaded BEFORE we get here:
            //     General:
            //         GameID
            //         Interpreter
            //         ResDir

            // ASSUMES a valid game property file has been loaded
            // loads only these properties:
            //     General:
            //         codepage
            //         description
            //         designer
            //         about
            //         game version
            //         platform, platform program, platform options, dos executable
            //         sierra syntax mode
            //         include IDs
            //         include reserved
            //         include globals
            //         use layout editor
            //         last date
            //     Include files:
            //         file list
            //     Palette:
            //         all colors
            //     Decompiler:
            //         logic sourcefile extension

            agSierraSyntax = agGameProps.GetSetting("General", "SierraSyntax", defaultSierraSyntax);
            // include files:
            // (force auto-includes to false if using sierra syntax)
            agIncludeReserved = !agSierraSyntax && agGameProps.GetSetting("Includes", "IncludeReserved", true);
            agIncludeIDs = !agSierraSyntax && agGameProps.GetSetting("Includes", "IncludeIDs", true);
            agIncludeGlobals = !agSierraSyntax && agGameProps.GetSetting("Includes", "IncludeGlobals", true);
            // split list into array, trim whitespace, and add to include file list
            bool reserved = !agIncludeReserved;
            bool ids = !agIncludeIDs;
            bool globals = !agIncludeGlobals;
            var list = agGameProps.GetSetting("Includes", "FileList", "").Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
            foreach (string file in list) {
                IncludeInfo info = new();
                // if relative path, convert to filename and add to list;
                // otherwise, just add filename
                if (Path.IsPathRooted(file)) {
                    info.Filename = file;
                }
                else {
                    info.Filename = Path.GetFullPath(file, agSrcResDir);
                }
                // check for reserved, resourceids and globals files and set type accordingly
                if (file == "reserved.txt" && agIncludeReserved) {
                    info.Type = IncludeType.Reserved;
                    reserved = true;
                }
                else if (file == "resourceids.txt" && agIncludeIDs) {
                    info.Type = IncludeType.ResourceIDs;
                    ids = true;
                }
                else if (file == "globals.txt" && agIncludeGlobals) {
                    info.Type = IncludeType.Globals;
                    globals = true;
                }
                else {
                    info.Type = IncludeType.Other;
                }
                // only add if unique
                if (!agIncludeFiles.Contains(info)) {
                    agIncludeFiles.Add(info);
                }
            }
            // confirm that reserved.txt, resourceids.txt and globals.txt are in the include file list if they're supposed to be included
            if (agIncludeReserved && !reserved) {
                agIncludeFiles.Add(new() {
                    Filename = Path.Combine(agSrcResDir, "reserved.txt"),
                    Type = IncludeType.Reserved
                });
            }
            if (agIncludeIDs && !ids) {
                agIncludeFiles.Add(new() {
                    Filename = Path.Combine(agSrcResDir, "resourceids.txt"),
                    Type = IncludeType.ResourceIDs
                });
            }
            if (agIncludeGlobals && !globals) {
                agIncludeFiles.Add(new() {
                    Filename = Path.Combine(agSrcResDir, "globals.txt"),
                    Type = IncludeType.Globals
                });
            }
            // Palette:
            for (int i = 0; i < 16; i++) {
                Palette[i] = agGameProps.GetSetting("Palette", "Color" + i.ToString(), DefaultPalette[i]);
            }
            // other properties
            agDescription = agGameProps.GetSetting("General", "Description", "");
            agDesigner = agGameProps.GetSetting("General", "Designer", "");
            agGameAbout = agGameProps.GetSetting("General", "About", "").Replace("\n", "\\n");
            agGameVersion = agGameProps.GetSetting("General", "GameVersion", "").Replace("\n", "\\n");
            agPlatformType = agGameProps.GetSetting("General", "PlatformType", PlatformType.None);
            agPlatformFile = agGameProps.GetSetting("General", "Platform", "");
            agDOSExec = agGameProps.GetSetting("General", "DOSExec", "");
            agPlatformOpts = agGameProps.GetSetting("General", "PlatformOpts", "");
            agCodePage = agGameProps.GetSetting("General", "CodePage", 437);
            agPowerPack = agGameProps.GetSetting("General", "PowerPack", false, true);
            // force layout to false if using sierra syntax
            agUseLE = !agSierraSyntax && agGameProps.GetSetting("General", "UseLE", true);
            agSrcFileExt = agGameProps.GetSetting("General", "SourceFileExt", defSrcExt).ToLower().Trim();
            if (agSrcFileExt[0] == '.') {
                agSrcFileExt = agSrcFileExt[1..];
                WriteGameSetting("General", "SourceFileExt", agSrcFileExt);
            }
            if (!DateTime.TryParse(agGameProps.GetSetting("General", "LastEdit", DateTime.Now.ToString()), out agLastEdit)) {
                // default to now
                agLastEdit = DateTime.Now;
            }
        }

        /// <summary>
        /// Cleans up after a compile game cancel or error.
        /// </summary>
        /// <param name="NoEvent"></param>
        internal void CompleteCancel(bool NoEvent = false) {
            if (!NoEvent) {
                WinAGIEventInfo tmpWarn = new() {
                    Type = EventType.ResourceWarning,
                    ResType = AGIResType.Game,
                };
                OnCompileGameStatus(GameCompileStatus.Canceled, tmpWarn);
            }
            Compiling = false;
            CancelComp = false;
            volManager.Clear();
            AddedIncludes.Clear();
        }

        public CompileStatus CompileChangedLogics() {
            bool unloadRes;
            Compiling = true;
            CancelComp = false;

            // set up info object
            WinAGIEventInfo tmpWarn = new() {
                Type = EventType.Info,
                ResType = AGIResType.Logic,
            };

            foreach (Logic logres in Logics) {
                tmpWarn.ResNum = logres.Number;
                tmpWarn.Module = logres.ID;
                // set flag to force unload, if resource not currently loaded
                unloadRes = !logres.Loaded;
                // then load resource if necessary
                if (unloadRes) {
                    // load resource
                    logres.Load();
                }
                // only source file access error is relevant
                if (logres.SourceError == ResourceErrorType.LogicSourceAccessError) {
                    AddCompileError(AGIResType.Logic, logres.Number, logres.SourceError, logres.ErrData);
                    // make sure unloaded
                    logres.Unload();
                    // and stop compiling
                    CompleteCancel(true);
                    return CompileStatus.ResourceError;
                }
                // logic resources currently don't have warnings

                if (!logres.Compiled) {
                    // clear existing warnings/errors
                    tmpWarn.InfoType = InfoType.ClearWarnings;
                    // check for cancellation
                    if (CheckForCancel(GameCompileStatus.AddResource, tmpWarn)) {
                        CompleteCancel();
                        return CompileStatus.Canceled;
                    }
                    // update status to compiling
                    tmpWarn.InfoType = InfoType.Compiling;
                    // check for cancellation
                    if (CheckForCancel(GameCompileStatus.AddResource, tmpWarn)) {
                        CompleteCancel();
                        return CompileStatus.Canceled;
                    }
                    if (!Base.CompileLogic(logres)) {
                        // unload if needed
                        if (unloadRes && logres is not null) {
                            logres.Unload();
                        }
                        CompleteCancel();
                        return CompileStatus.LogicCompileError;
                    }
                    // check for cancellation
                    if (CancelComp) {
                        if (unloadRes && logres is not null) {
                            logres.Unload();
                        }
                        CompleteCancel();
                        return CompileStatus.Canceled;
                    }
                    // save the updated resource data (updates VOL/DIR files)
                    try {
                        ((AGIResource)logres).Save();
                    }
                    catch (Exception e) {
                        // force uncompiled state
                        logres.mCompiledCRC = 0xffffffff;
                        logres.parent.WriteGameSetting("Logic" + logres.Number, "CompCRC32", "0x" + logres.mCompiledCRC.ToString("x8"), "Logics");
                        // report any errors
                        tmpWarn.Text = $"Unable to save {logres.ID} to game files: " + e.Message;
                        OnCompileGameStatus(GameCompileStatus.FatalError, tmpWarn);
                        // make sure unloaded
                        logres.Unload();
                        // and stop compiling
                        CompleteCancel(true);
                        return CompileStatus.ResourceError;
                    }
                    // update status to compiled
                    tmpWarn.InfoType = InfoType.Compiled;
                    // check for cancellation
                    if (CheckForCancel(GameCompileStatus.AddResource, tmpWarn)) {
                        CompleteCancel();
                        return CompileStatus.Canceled;
                    }
                }
            }
            // reset added include file list
            AddedIncludes.Clear();
            // save game properties
            agGameProps.Save();
            // reset compiling flag
            Compiling = false;
            return CompileStatus.OK;
        }
        #endregion
    }
}