using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.LogicCompiler;
using static WinAGI.Engine.LogicDecoder;

namespace WinAGI.Engine {
    /// <summary>
    /// This class represents an AGI game, including all methods,
    /// classes, properties, including WinAGI specific features, that
    /// are needed to open, view, edit and save all game resources
    /// in the WinAGI Game Development System.
    /// </summary>
    public partial class AGIGame {
        #region Local Members
        internal VOLManager volManager;
        internal Logics agLogs;
        internal Sounds agSnds;
        internal Views agViews;
        internal Pictures agPics;
        internal InventoryList agInvObj;
        internal WordList agVocabWords;
        internal GlobalList agGlobals;
        internal ReservedDefineList agReservedDefines;
        internal EGAColors agEGAcolors = new();
        internal string agGameDir = "";
        internal string agResDir = "";
        internal string agResDirName = "";
        internal string agGameID = "";
        internal bool agLoadWarnings = false;
        internal string agAuthor = "";
        internal DateTime agLastEdit;
        internal string agDescription = "";
        internal string agIntVersion = "2.917";
        internal bool agIsVersion3 = false;
        internal string agGameAbout = "";
        internal string agGameVersion = "";
        internal string agGameFile = "";
        internal int agMaxVol0 = MAX_VOLSIZE;
        internal int agMaxVolSize = MAX_VOLSIZE; // currently this cannot be changed 
        internal string agCompileDir = "";
        internal PlatformType agPlatformType = PlatformType.None;
        internal string agPlatformFile = "";
        internal string agPlatformOpts = "";
        internal string agDOSExec = "";
        internal bool agUseLE = false;
        internal bool agIncludeIDs = true;
        internal bool agIncludeReserved = true;
        internal bool agIncludeGlobals = true;
        internal Encoding agCodePage = Encoding.GetEncoding(437);
        internal bool agPowerPack = false;
        internal string agSrcFileExt = "";
        internal bool agSierraSyntax = false;
        internal SettingsFile agGameProps;
        internal WinAGIFileWatcher agFileWatcher;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor used to open an existing game from a WinAGI Game
        /// File (WAG) or a directory (which creates a new WAG file.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="gameSource"></param>
        public AGIGame(OpenGameMode mode, string gameSource) {
            InitGame();
            TWinAGIEventInfo retval = new();
            switch (mode) {
            case OpenGameMode.File:
                retval = OpenGameWAG(gameSource);
                break;
            case OpenGameMode.Directory:
                retval = OpenGameDIR(gameSource);
                break;
            }
            if (retval.Type == EventType.GameLoadError) {
                // release compGame
                compGame = null;
                WinAGIException wex = new(retval.Text) {
                    HResult = WINAGI_ERR + int.Parse(retval.ID),
                };
                wex.Data["retval"] = retval;

                throw wex;
            }
        }

        /// <summary>
        /// Constructor used to create a new blank game with no resources.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="gamedir"></param>
        /// <param name="resdir"></param>
        /// <param name="template"></param>
        public AGIGame(string id, string version, string gamedir, string resdir, string srcext) {
            InitGame();
            // give compiler access
            compGame = this;
            // create a new, blank game
            NewGame(id, version, gamedir, resdir, srcext, "");
        }

        /// <summary>
        /// Constructor used to create a new game from a template.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="gamedir"></param>
        /// <param name="resdir"></param>
        /// <param name="template"></param>
        public AGIGame(string id, string version, string gamedir, string resdir, string srcext, string template) {
            InitGame();
            // give compiler access
            compGame = this;
            // create a new, blank game
            NewGame(id, version, gamedir, resdir, srcext, template);
        }



        #endregion

        #region Properties
        /// <summary>
        /// Gets the collection of Logic resources in this game.
        /// </summary>
        public Logics Logics { get => agLogs; }

        /// <summary>
        /// Gets the collection of Picture resources in this game.
        /// </summary>
        public Pictures Pictures { get => agPics; }

        /// <summary>
        /// Gets the collection of Sound resources in this game.
        /// </summary>
        public Sounds Sounds { get => agSnds; }

        /// <summary>
        /// Gets the collection of View resources in this game.
        /// </summary>
        public Views Views { get => agViews; }

        /// <summary>
        /// Gets the list of words from this game's WORDS.TOK file.
        /// </summary>
        public WordList WordList { get => agVocabWords; }

        /// <summary>
        /// Gets the list of inventory items from this game's OBJECT file.
        /// </summary>
        public InventoryList InvObjects { get => agInvObj; }

        /// <summary>
        /// Gets the list of global defines for this game. Used by the logic compiler.
        /// </summary>
        public GlobalList GlobalDefines { get => agGlobals; }

        public ReservedDefineList ReservedDefines { get => agReservedDefines;  }

        /// <summary>
        /// Gets the AGI color palette that is used for displaying pictures and views.
        /// </summary>
        public EGAColors Palette { get => agEGAcolors; }

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
                if (agIsVersion3) {
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
                if (agIsVersion3) {
                    try {
                        File.Move(agGameDir + agGameID + "DIR", agGameDir + NewID.ToUpper() + "DIR");
                        foreach (string strVolFile in Directory.EnumerateFiles(agGameDir, agGameID + "VOL.*")) {
                            string strExtension = Path.GetExtension(strVolFile);
                            File.Move(strVolFile, agGameDir + NewID.ToUpper() + "VOL" + strExtension);
                        }
                    }
                    catch (Exception e) {
                        WinAGIException wex = new(LoadResString(530).Replace(ARG1, e.HResult.ToString())) {
                            HResult = WINAGI_ERR + 530
                        };
                        wex.Data["exception"] = e;
                        throw wex;
                    }
                }
                agGameID = NewID;
                WriteGameSetting("General", "GameID", NewID);
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
                if (Directory.Exists(FullDir(value))) {
                    WinAGIException wex = new(LoadResString(630).Replace(ARG1, value)) {
                        HResult = WINAGI_ERR + 630
                    };
                    wex.Data[0] = value;
                    throw wex;
                }
                agGameDir = FullDir(value);
                agGameFile = agGameDir + Path.GetFileName(agGameFile);
                agResDir = agGameDir + agResDirName + @"\";
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
                WriteGameSetting("General", "LastEdit", agLastEdit.ToString());
            }
        }

        /// <summary>
        /// Gets or sets a 'game about' string that can be accessed in logics.
        /// </summary>
        public string GameAbout {
            get => agGameAbout;
            set {
                if (value.Length > 4096) {
                    agGameAbout = value.Left(4096);
                }
                else {
                    agGameAbout = value;
                }
                WriteGameSetting("General", "About", agGameAbout);
            }
        }

        /// <summary>
        /// Gets or sets the GameAuthor meta-property. Used only by WinAGI editor.
        /// </summary>
        public string GameAuthor {
            get => agAuthor;
            set {
                if (value.Length > 256) {
                    agAuthor = value.Left(256);
                }
                else {
                    agAuthor = value;
                }
                WriteGameSetting("General", "Author", agAuthor);
            }
        }

        /// <summary>
        /// Gets or sets the GameDescription meta-property. Used only by WinAGI editor.
        /// </summary>
        public string GameDescription {
            get => agDescription;
            set {
                if (value.Length > 4096) {
                    agDescription = value.Left(4096);
                }
                else {
                    agDescription = value;
                }
                WriteGameSetting("General", "Description", agDescription);
            }
        }

        /// <summary>
        /// Gets or sets the Sierra Interpreter version that this game is designed 
        /// to be compatible with. If changed between V2 and V3, the game will
        /// automatically be recompiled to correct DIR/VOL format.
        /// </summary>
        public string InterpreterVersion {
            get {
                return agIntVersion;
            }
            set {
                if (!IntVersions.Contains(value)) {
                    if (!value.IsNumeric()) {
                        WinAGIException wex = new(LoadResString(597)) {
                            HResult = WINAGI_ERR + 597
                        };
                        throw wex;
                    }
                    else if (Double.Parse(value) < 2 || Double.Parse(value) > 3) {
                        //not a version 2 or 3 game
                        WinAGIException wex = new(LoadResString(597)) {
                            HResult = WINAGI_ERR + 597
                        };
                        throw wex;
                    }
                    else {
                        //unsupported version 2 or 3 game
                        WinAGIException wex = new(LoadResString(543)) {
                            HResult = WINAGI_ERR + 543
                        };
                        throw wex;
                    }
                }
                // if new version and old version are not same major version:
                if (agIntVersion[0] != value[0]) {
                    // set flag to switch version on rebuild
                    V3V2Swap = true;
                    // if new version is V3 (i.e., current version isn't)
                    if (!agIsVersion3) {
                        if (GameID.Length > 5) {
                            // truncate the ID
                            GameID = GameID[..5];
                        }
                    }
                    // use compiler to rebuild new vol and dir files
                    // (it is up to calling program to deal with changed or invalid resources)
                    if (Compile(true) != CompileStatus.OK) {
                        // failed - don't make the change
                        return;
                    }
                }
                // OBJECT file encryption depends on version
                if (value == "2.089" || value == "2.272") {
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
                WriteGameSetting("General", "Interpreter", value);
                agGameProps.Save();

                // OK to set new version
                agIntVersion = value;
                agIsVersion3 = agIntVersion[0] == '3';
                CorrectCommands(agIntVersion);
            }
        }

        /// <summary>
        /// Gets the full path of this game's resource folder.
        /// </summary>
        public string ResDir {
            get {
                return agResDir;
            }
        }

        /// <summary>
        /// Gets or sets the folder name used to store logic source code
        /// files, and exported resource files.<br /><br />
        /// Changing the folder name does NOT automatically move files
        /// from the old folder.
        /// </summary>
        public string ResDirName {
            get { return agResDirName; }
            set {
                string tmpName = value.Trim();
                // ignore if blank
                if (tmpName.Length == 0) {
                    return;
                }
                if (Path.GetInvalidFileNameChars().Any(tmpName.Contains)) {
                    throw new ArgumentOutOfRangeException(nameof(ResDirName), "Invalid property Value");
                }
                agResDirName = tmpName;
                agResDir = agGameDir + agResDirName + @"\";
                WriteGameSetting("General", "ResDir", agResDirName);
                agLastEdit = DateTime.Now;
            }
        }

        /// <summary>
        /// Gets or sets the maximum size that WinAGI will use for VOL.0 when
        /// compiling this game. Provided for backward compatibility of floppy
        /// disk installations. 
        /// </summary>
        public int MaxVol0Size {
            get { return agMaxVol0; }
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
                WriteGameSetting("General", "IncludeIDs", agIncludeIDs.ToString());
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
                WriteGameSetting("General", "IncludeReserved", agIncludeReserved.ToString());
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
                WriteGameSetting("General", "IncludeGlobals", agIncludeGlobals.ToString());
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
                if (value < 0 || value > (PlatformType)4) {
                    agPlatformType = 0;
                }
                else {
                    agPlatformType = value;
                }
                // TODO: adjust settings file to read strings that can be parsed directly to enum
                // i.e. platform = Enum.Parse(typeof(PlatformTypeEnum), "DosBox")
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
                WriteGameSetting("General", "UseLE", agUseLE.ToString());
            }
        }

        /// <summary>
        /// Gets or sets the character encoding used in this game. Changing from the
        /// default encoding is only needed if providing support for other languages.
        /// </summary>
        public Encoding CodePage {
            get { return agCodePage; }
            set {
                if (validcodepages.Contains(value.CodePage)) {
                    agCodePage = Encoding.GetEncoding(value.CodePage);
                    WriteGameSetting("General", "CodePage", agCodePage.CodePage);
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
            get { return agSierraSyntax; }
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
            get { return agPowerPack; }
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
                    agGameVersion = value[..256];
                }
                else {
                    agGameVersion = value;
                }
                WriteGameSetting("General", "GameVersion", agGameVersion);
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
            //write date of last edit
            WriteGameSetting("General", "LastEdit", agLastEdit.ToString());
            //now save it
            agGameProps.Save();
            // clear all game properties
            ClearGameState();
            // TODO: shutdown filewatcher
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
            bool blnReplace, NewIsV3;
            string strID;
            int tmpMax = 0, i, j;
            TWinAGIEventInfo compInfo = new();

            Compiling = true;
            CancelComp = false;
            // if no directory passed assume current dir
            if (NewGameDir.Length == 0) {
                NewGameDir = agGameDir;
            }
            NewGameDir = FullDir(NewGameDir);
            if (!Directory.Exists(NewGameDir)) {
                // raise error event
                TWinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = LoadResString(561).Replace(ARG1, NewGameDir),
                    Data = 561
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.Error;
            }
            blnReplace = NewGameDir.Equals(agGameDir, StringComparison.OrdinalIgnoreCase);
            // save compile dir now so rebuild method can access it
            agCompileDir = NewGameDir;
            // whether or not the new game files will be V3 depends on
            // if changing major interpreter version or not
            if (V3V2Swap) {
                // new version is V3 if current version is V2
                NewIsV3 = !agIsVersion3;
                V3V2Swap = false;
            }
            else {
                // new version is same as current version
                NewIsV3 = agIsVersion3;
            }
            if (NewIsV3) {
                strID = agGameID.ToUpper();
            }
            else {
                strID = "";
            }
            // version2 IDs limited to 6 characters,  v3 IDs limited to 5 characters
            if (agGameID.Length > 6 || (NewIsV3 && agGameID.Length > 5)) {
                // invalid ID; calling function should know better!
                // warn user and truncate it
                TWinAGIEventInfo warning = new() {
                    Type = EventType.ResourceWarning,
                    ResType = AGIResType.Game,
                    ID = "RE27",
                    Text = LoadResString(694),
                    ResNum = 0,
                    Line = "--",
                    Module = "--"
                };
                OnCompileGameStatus(GameCompileStatus.Warning, warning, ref CancelComp);
                if (CancelComp) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                agGameID = agGameID[..(NewIsV3 ? 5 : 6)];
            }
            if (!RebuildOnly) {
                // full compile - save/copy words.tok and object files first
                compInfo.Text = "";
                compInfo.ResType = AGIResType.Words;
                compInfo.InfoType = InfoType.Resources;
                OnCompileGameStatus(GameCompileStatus.CompileWords, compInfo, ref CancelComp);
                if (CancelComp) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                if (agVocabWords.IsChanged) {
                    try {
                        agVocabWords.Save();
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.GameCompileError,
                            ResType = AGIResType.Words,
                            Text = "Error during compilation of WORDS.TOK (" + ex.Message + ")"
                        };
                        OnCompileGameStatus(GameCompileStatus.ResError, tmpError);
                        CompleteCancel(true);
                        return CompileStatus.Error;
                    }
                }
                // check it for errors
                compInfo.InfoType = InfoType.ClearWarnings;
                OnCompileGameStatus(GameCompileStatus.CompileWords, compInfo, ref CancelComp);
                if (CancelComp) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                if (agVocabWords.ErrLevel != 0) {
                    AddCompileWarning(AGIResType.Words, 0, agVocabWords.ErrLevel, []);
                }
                if (!blnReplace) {
                    // copy WORDS.TOK to new folder
                    try {
                        // then copy the current file to new location
                        File.Copy(agVocabWords.ResFile, NewGameDir + "WORDS.TOK", true);
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.GameCompileError,
                            ResType = AGIResType.Words,
                            Text = "Error while creating WORDS.TOK file (" + ex.Message + ")"
                        };
                        OnCompileGameStatus(GameCompileStatus.ResError, tmpError);
                        CompleteCancel(true);
                        return CompileStatus.Error;
                    }
                }
                // OBJECT file is next
                compInfo.Text = "";
                compInfo.ResType = AGIResType.Objects;
                OnCompileGameStatus(GameCompileStatus.CompileObjects, compInfo, ref CancelComp);
                if (CancelComp) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                if (agInvObj.IsChanged) {
                    try {
                        agInvObj.Save();
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.GameCompileError,
                            ResType = AGIResType.Objects,
                            Text = "Error during compilation of OBJECT (" + ex.Message + ")"
                        };
                        OnCompileGameStatus(GameCompileStatus.ResError, tmpError);
                        CompleteCancel(true);
                        return CompileStatus.Error;
                    }
                }
                // check it for errors
                compInfo.InfoType = InfoType.ClearWarnings;
                OnCompileGameStatus(GameCompileStatus.CompileObjects, compInfo, ref CancelComp);
                if (agInvObj.ErrLevel != 0) {
                    AddCompileWarning(AGIResType.Objects, 0, agInvObj.ErrLevel, []);
                }
                if (CancelComp) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                if (!blnReplace) {
                    // copy OBJECT to new folder
                    try {
                        File.Copy(agInvObj.ResFile, NewGameDir + "OBJECT", true);
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.GameCompileError,
                            ResType = AGIResType.Objects,
                            Text = "Error while creating OBJECT file (" + ex.Message + ")"
                        };
                        OnCompileGameStatus(GameCompileStatus.ResError, tmpError);
                        CompleteCancel(true);
                        return CompileStatus.Error;
                    }
                }
            }
            // reset game compiler variables
            volManager.Clear();

            // remove any temp vol files
            for (int v = 0; v < 15; v++) {
                SafeFileDelete(NewGameDir + "NEW_VOL." + v.ToString());
            }
            try {
                // open first new vol file
                volManager.InitVol(NewGameDir);
            }
            catch (Exception e) {
                // raise error event
                TWinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    Module = e.Message,
                    ResType = AGIResType.Game,
                    Text = LoadResString(503).Replace(ARG1, NewGameDir + "NEW_VOL.0"),
                    Data = 503
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.Error;
            }
            // add all logic resources
            try {
                CompileStatus result = VOLManager.CompileResCol(this, agLogs, AGIResType.Logic, RebuildOnly, NewIsV3);
                if (result != CompileStatus.OK) {
                    // resource error (or user canceled)
                    return result;
                }
            }
            catch (Exception e) {
                // raise error event
                TWinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = e.Message + ":\n\n" + e.StackTrace.SplitLines()[0],
                    Data = e.HResult - WINAGI_ERR
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.Error;
            }
            // add all picture resources
            try {
                CompileStatus result = VOLManager.CompileResCol(this, agPics, AGIResType.Picture, RebuildOnly, NewIsV3);
                if (result != CompileStatus.OK) {
                    // resource error (or user canceled)
                    return result;
                }
            }
            catch (Exception e) {
                // raise error event
                TWinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = e.Message,
                    Data = e.HResult - WINAGI_ERR
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.Error;
            }
            // add all view resources
            try {
                CompileStatus result = VOLManager.CompileResCol(this, agViews, AGIResType.View, RebuildOnly, NewIsV3);
                if (result != CompileStatus.OK) {
                    // resource error (or user canceled)
                    return result;
                }
            }
            catch (Exception e) {
                // raise error event
                TWinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = e.Message,
                    Data = e.HResult - WINAGI_ERR
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.Error;
            }
            // add all sound resources
            try {
                CompileStatus result = VOLManager.CompileResCol(this, agSnds, AGIResType.Sound, RebuildOnly, NewIsV3);
                if (result != CompileStatus.OK) {
                    // resource error (or user canceled)
                    return result;
                }
            }
            catch (Exception e) {
                // raise error event
                TWinAGIEventInfo tmpError = new() {
                    Type = EventType.GameCompileError,
                    ResType = AGIResType.Game,
                    Text = e.Message,
                    Data = e.HResult - WINAGI_ERR
                };
                OnCompileGameStatus(GameCompileStatus.FatalError, tmpError);
                CompleteCancel(true);
                return CompileStatus.Error;
            }
            // release handle on vol file
            volManager.VOLFile.Dispose();
            volManager.VOLWriter.Dispose();
            // rename existing dir files as _OLD
            if (NewIsV3) {
                SafeFileMove(NewGameDir + strID + "DIR", NewGameDir + strID + "DIR_OLD", true);
            }
            else {
                SafeFileMove(NewGameDir + "LOGDIR", NewGameDir + "LOGDIR_OLD", true);
                SafeFileMove(NewGameDir + "PICDIR", NewGameDir + "PICDIR_OLD", true);
                SafeFileMove(NewGameDir + "VIEWDIR", NewGameDir + "VIEWDIR_OLD", true);
                SafeFileMove(NewGameDir + "SNDDIR", NewGameDir + "SNDDIR_OLD", true);
            }
            // now build the new DIR files
            if (NewIsV3) {
                volManager.DIRFile = File.Create(NewGameDir + strID + "DIR");
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
                        volManager.DIRFile = File.Create(NewGameDir + "LOGDIR");
                        tmpMax = agLogs.Max;
                        break;
                    case AGIResType.Picture:
                        volManager.DIRFile = File.Create(NewGameDir + "PICDIR");
                        tmpMax = agPics.Max;
                        break;
                    case AGIResType.Sound:
                        volManager.DIRFile = File.Create(NewGameDir + "SNDDIR");
                        tmpMax = agSnds.Max;
                        break;
                    case AGIResType.View:
                        volManager.DIRFile = File.Create(NewGameDir + "VIEWDIR");
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
            // rename current vol files
            for (i = 0; i < 16; i++) {
                if (NewIsV3) {
                    if (File.Exists(NewGameDir + strID + "VOL." + i.ToString())) {
                        SafeFileMove(
                                  NewGameDir + strID + "VOL." + i.ToString(),
                                  NewGameDir + strID + "VOL_OLD." + i.ToString(),
                                  true
                        );
                    }
                }
                else {
                    if (File.Exists(NewGameDir + "VOL." + i.ToString())) {
                        SafeFileMove(
                                  NewGameDir + "VOL." + i.ToString(),
                                  NewGameDir + "VOL_OLD." + i.ToString(),
                                  true
                        );
                    }
                }
            }
            // now rename VOL files
            for (i = 0; i < volManager.Count; i++) {
                File.Move(NewGameDir + "NEW_VOL." + i.ToString(), NewGameDir + strID + "VOL." + i.ToString());
            }
            // update status to indicate complete
            compInfo.Text = "";
            compInfo.ResType = AGIResType.Game;
            OnCompileGameStatus(GameCompileStatus.CompileComplete, compInfo, ref CancelComp);
            if (CancelComp) {
                CompleteCancel();
                return CompileStatus.Canceled;
            }
            if (blnReplace) {
                // need to update the vol/loc info for all ingame resources;
                // this is done here instead of when the resources are compiled because
                // if there's an error, or the user cancels, we don't want the directories
                // to point to the wrong place
                foreach (Logic tmpLogic in agLogs.Col.Values) {
                    tmpLogic.Volume = (sbyte)(volManager.DIRData[0, tmpLogic.Number, 0] >> 4);
                    tmpLogic.Loc = ((volManager.DIRData[0, tmpLogic.Number, 0] & 0xF) << 16) + (volManager.DIRData[0, tmpLogic.Number, 1] << 8) + volManager.DIRData[0, tmpLogic.Number, + 2];
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
            // save the wag file
            agGameProps.Save();
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
        private void NewGame(string NewID, string NewVersion, string NewGameDir, string NewResDir, string NewExt, string TemplateDir = "") {
            string oldExt;
            SettingsFile stlGlobals;
            TWinAGIEventInfo eventInfo = new();

            // reset game variables
            ClearGameState();
            // set up event status object
            eventInfo.InfoType = InfoType.Initialize; 
            eventInfo.Text = "validating parameters";
            OnNewGameStatus(eventInfo);
            if (!Directory.Exists(NewGameDir)) {
                // folder must already exist
                WinAGIException wex = new(LoadResString(630).Replace(ARG1, NewGameDir)) {
                    HResult = WINAGI_ERR + 630
                };
                throw wex;
            }
            if (Directory.GetFiles(NewGameDir, "*.wag").Length != 0) {
                // folder cannot have existing WinAGI game files
                WinAGIException wex = new(LoadResString(687)) {
                    HResult = WINAGI_ERR + 687
                };
                throw wex;
            }
            if (IsValidGameDir(FullDir(NewGameDir))) {
                // folder cannot have existing Sierra game files
                WinAGIException wex = new(LoadResString(687)) {
                    HResult = WINAGI_ERR + 687
                };
                throw wex;
            }
            agGameDir = FullDir(NewGameDir);
            if (NewResDir.Length == 0) {
                NewResDir = agDefResDir;
            }
            if (NewExt.Length == 0) {
                NewExt = agDefSrcExt;
            }
            NewExt = NewExt.ToLower();
            if (TemplateDir.Length == 0) {
                // if blank game (no template),
                // just create the new files and set game parameters
                eventInfo.InfoType = InfoType.Resources;
                eventInfo.Text = "creating new game components";
                OnNewGameStatus(eventInfo);
                if (!IntVersions.Contains(NewVersion)) {
                    if (NewVersion.Val() < 2 || NewVersion.Val() >= 4) {
                        // not a version 2 or 3 game
                        WinAGIException wex = new(LoadResString(597)) {
                            HResult = WINAGI_ERR + 597
                        };
                        throw wex;
                    }
                    else {
                        // unsupported version 2 or 3 game
                        WinAGIException wex = new(LoadResString(543)) {
                            HResult = WINAGI_ERR + 543
                        };
                        throw wex;
                    }
                }
                agIntVersion = NewVersion;
                agIsVersion3 = agIntVersion[0] == '3';
                // set game id (limit to 6 characters for v2, and 5 characters for v3
                if (agIsVersion3) {
                    agGameID = NewID.Left(5);
                }
                else {
                    agGameID = NewID.Left(6);
                }
                // create empty property file
                agGameFile = agGameDir + agGameID + ".wag";
                agGameProps = new SettingsFile(agGameFile, FileMode.Create);
                agGameProps.Lines.Add("# WinAGI Game Property File for " + agGameID);
                agGameProps.Lines.Add("#");
                agGameProps.Lines.Add("");
                agGameProps.Lines.Add("[General]");
                // save gameID, version, directory resource name, srcext and palette to the property file;
                // rest of properties need to be set by the calling function
                agGameProps.Lines.Add("   WinAGIVersion = " + WINAGI_VERSION);
                agGameProps.Lines.Add("   GameID = " + agGameID);
                agGameProps.Lines.Add("   Interpreter = " + agIntVersion);
                agGameProps.Lines.Add("   ResDir = " + NewResDir);
                agGameProps.Lines.Add("   SourceFileExt = " + NewExt);
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
                // set the resource directory name so it can be set up
                agResDirName = NewResDir;
                agResDir = agGameDir + agResDirName + @"\";
                // assign source file extension
                agSrcFileExt = NewExt;
                // create default resource DIR files
                // (default DIR files shold only have ONE 'FFFFFF' entry)
                byte[] bytDirData = [0xff, 0xff, 0xff];
                if (agIsVersion3) {
                    // for v3, header should be '08 - 00 - 0B - 00 - 0E - 00 - 11 - 00
                    byte[] bytDirHdr = [8, 0, 0x0b, 0, 0x0e, 0, 0x11, 0];
                    using FileStream fsDIR = new(agGameDir + agGameID + "DIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirHdr);
                    for (int i = 0; i < 4; i++) {
                        fsDIR.Write(bytDirData);
                    }
                    fsDIR.Dispose();
                }
                else {
                    FileStream fsDIR = new(agGameDir + "LOGDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR.Dispose();
                    fsDIR = new FileStream(agGameDir + "PICDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR.Dispose();
                    fsDIR = new FileStream(agGameDir + "SNDDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR.Dispose();
                    fsDIR = new FileStream(agGameDir + "VIEWDIR", FileMode.OpenOrCreate);
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
                if (NewVersion == "2.089" || NewVersion == "2.272") {
                    agInvObj.Encrypted = false;
                }
                else {
                    agInvObj.Encrypted = true;
                }
                agInvObj.Save();
                // create/confirm resource dir
                if (!Directory.Exists(agResDir)) {
                    try {
                        Directory.CreateDirectory(agResDir);
                    }
                    catch {
                        throw;
                    }
                }
                agReservedDefines = new(this);
                agGlobals = new(this);

                // set commands based on AGI version
                CorrectCommands(agIntVersion);
                // add logic zero
                Logic newlogic = agLogs.Add(0);

                //add default text
                StringList src =
                [];
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
                src.Add("");
                src.Add("return();");
                src.Add("");
                newlogic.SourceText = string.Join(NEWLINE, [.. src]);
                newlogic.Save();
                agLogs[0].Unload();
                if (IncludeGlobals) {

                    StringList resIDlist = [
                        "[",
                        "[ global defines file for " + agGameID,
                        "[",
                        ""];
                    try {
                        using FileStream fsList = new FileStream(agResDir + "globals.txt", FileMode.Create);
                        using StreamWriter swList = new StreamWriter(fsList);
                        foreach (string line in resIDlist) {
                            swList.WriteLine(line);
                        }
                    }
                    catch {
                        // ignore errors for now
                    }
                }
            }
            else {
                eventInfo.InfoType = InfoType.Initialize;
                eventInfo.Text = "copying template files to new location";
                OnNewGameStatus(eventInfo);
                // if from a template:
                // template should include dir files, vol files, words.tok and object;
                // also globals list and layout, and source directory with logic source files
                TemplateDir = FullDir(TemplateDir);
                // should be exactly one wag file
                if (Directory.GetFiles(TemplateDir, "*.wag").Length != 1) {
                    WinAGIException wex = new(LoadResString(630)) {
                        HResult = WINAGI_ERR + 630
                    };
                    throw wex;
                }
                // template should have at least one subdirectory
                if (Directory.GetDirectories(TemplateDir).Length == 0) {
                    // no resource directory
                    // TODO: need new err num
                    WinAGIException wex = new(LoadResString(630)) {
                        HResult = WINAGI_ERR + 630
                    };
                    throw wex;
                }
                // 1. copy files from template
                try {
                    CopyDirectory(TemplateDir, agGameDir, true);
                    // get wag file name (it's first[and only] element)
                    agGameFile = Directory.GetFiles(agGameDir, "*.wag")[0];
                    // rename it to match new ID
                    File.Move(agGameFile, agGameDir + NewID + ".wag");
                    agGameFile = agGameDir + NewID + ".wag";
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(683).Replace(ARG1, e.Message)) {
                        HResult = WINAGI_ERR + 683
                    };
                    wex.Data["exception"] = e;
                    throw wex;
                }
                // 2. adjust game parameters:
                eventInfo.InfoType = InfoType.Initialize;
                eventInfo.Text = "updating game components";
                OnNewGameStatus(eventInfo);
                agGameID = NewID;
                // retrieve name of the first directory as current resource dir
                agResDir = Directory.GetDirectories(agGameDir)[0];
                agResDirName = NewResDir;
                if (!agResDir.Equals(agGameDir + NewResDir, StringComparison.OrdinalIgnoreCase)) {
                    // rename it to new 
                    try {
                        DirectoryInfo resDir = new(agResDir);
                        resDir.MoveTo(agGameDir + NewResDir);
                        agResDir = agGameDir + NewResDir + @"\";
                    }
                    catch (Exception e) {
                        WinAGIException wex = new(LoadResString(683).Replace(ARG1, e.Message)) {
                            HResult = WINAGI_ERR + 683
                        };
                        wex.Data["exception"] = e;
                        throw wex;
                    }
                }
                // need to open the new wag file to update key properties
                try {
                    agGameProps = new(agGameFile, FileMode.Open);
                }
                catch (Exception) {
                    WinAGIException wex = new(LoadResString(630)) {
                        HResult = WINAGI_ERR + 630
                    };
                    throw wex;
                }
                // check version
                string strValue = agGameProps.GetSetting("General", "WinAGIVersion", "");
                if (strValue.Left(4) == "1.2." || (strValue.Left(2) == "2.")) {
                    // any v1.2.x or 2.x is ok, but need to update
                    agGameProps.WriteSetting("General", "WinAGIVersion", WINAGI_VERSION);
                    eventInfo.InfoType = InfoType.PropertyFile;
                    eventInfo.Text = "";
                    OnNewGameStatus(eventInfo);
                }
                // gameid
                agGameProps.WriteSetting("General", "GameID", NewID);
                // resdir
                agGameProps.WriteSetting("General", "ResDir", NewResDir);
                // src ext
                agSrcFileExt = NewExt;
                oldExt = agGameProps.GetSetting("Decompiler", "SourceFileExt", DefaultSrcExt);
                if (!oldExt.Equals(agSrcFileExt, StringComparison.OrdinalIgnoreCase)) {
                    agGameProps.WriteSetting("Decompiler", "SourceFileExt", agSrcFileExt);
                    try {
                        // rename all existing logic source file
                        foreach (string file in Directory.GetFiles(agResDir)) {
                            File.Move(file, agResDir + Path.GetFileNameWithoutExtension(file) + "." + agSrcFileExt);
                        }
                    }
                    catch {
                        // ignore errors?
                        // TODO: create newgame warning for this situation
                    }
                }
                agGameProps.Save();
                // rename wal file, if present
                foreach (string tmpWAL in Directory.GetFiles(agGameDir, "*.wal")) {
                    try {
                        File.Move(tmpWAL, agGameDir + Path.GetFileNameWithoutExtension(agGameFile) + ".wal");
                    }
                    catch {
                        // ignore errors?
                        // TODO: create newgame warning for this situation
                    }
                    // only need to check for a single file
                    break;
                }
                // update global file header
                if (File.Exists(agResDir + "globals.txt")) {
                    try {
                        stlGlobals = new SettingsFile(agResDir + "globals.txt", FileMode.OpenOrCreate);
                        if (stlGlobals.Lines.Count > 3) {
                            if (stlGlobals.Lines[1].Trim().Left(1) == "[") {
                                stlGlobals.Lines[1] = "[ global defines file for " + NewID;
                            }
                            // save it
                            stlGlobals.Save();
                        }
                    }
                    catch {
                        // ignore errors?
                        // TODO: create newgame warning for this situation
                    }
                }
                // 3. open the newly created game
                eventInfo.InfoType = InfoType.Finalizing;
                eventInfo.Text = "opening the new game";
                OnNewGameStatus(eventInfo);
                try {
                    // finish by loading the game resources
                    TWinAGIEventInfo retval = FinishGameLoad(OpenGameMode.New);
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(684).Replace(ARG1, e.Message)) {
                        HResult = WINAGI_ERR + 684
                    };
                    wex.Data["exception"] = e;
                    throw wex;
                }
                //check retval
            }
            blnSetIDs = false;
            SetResourceIDs(this);
            // give access to compiler
            compGame = this;
            // enable file watcher
            agFileWatcher = new(agGameDir);
            agFileWatcher.Enabled = true;

            // done
            return;
        }

        /// <summary>
        /// Creates a new WinAGI game file from an existing Sierra game directory.
        /// If successful, warning/load info is passed back. 
        /// If fails, exception is thrown.
        /// </summary>
        /// <param name="NewGameDir"></param>
        /// <returns></returns>
        private TWinAGIEventInfo OpenGameDIR(string NewGameDir) {
            // periodically report status of the load back to calling function
            TWinAGIEventInfo warnInfo = new() {
                Type = EventType.ResourceWarning,
            };
            // set game directory
            agGameDir = FullDir(NewGameDir);
            warnInfo.Type = EventType.Info;
            warnInfo.InfoType = InfoType.Validating;
            warnInfo.Text = "";
            OnLoadGameStatus(warnInfo);
            // check for valid DIR/VOL files
            // (which gets gameid, and sets version 3 status flag)
            if (!IsValidGameDir(agGameDir)) {
                ClearGameState();
                // directory is not a valid AGI directory
                WinAGIException wex = new(LoadResString(541).Replace(ARG1, FullDir(NewGameDir))) {
                    HResult = WINAGI_ERR + 541,
                };
                wex.Data["baddir"] = FullDir(NewGameDir);
                throw wex;
            }
            agGameFile = agGameDir + agGameID + ".wag";
            string oldFile = agGameDir + agGameID + "_OLD.wag";
            // rename any existing game file (in case user is re-importing)
            if (File.Exists(agGameFile)) {
                SafeFileMove(agGameFile, oldFile, true);
            }
            // create new wag file
            try {
                agGameProps = new SettingsFile(agGameFile, FileMode.Create);
            }
            catch (Exception e) {
                ClearGameState();
                // file error
                // TODO: wrong error message here
                WinAGIException wex = new(LoadResString(699)) {
                    HResult = WINAGI_ERR + 699,
                };
                wex.Data["exception"] = e;
                throw wex;
            }
            agGameProps.Lines.Add("# WinAGI Game Property File");
            agGameProps.Lines.Add("#");
            agGameProps.WriteSetting("General", "WinAGIVersion", WINAGI_VERSION);
            agGameProps.WriteSetting("General", "GameID", agGameID);
            // get version number (version3 flag already set)
            agIntVersion = GetIntVersion(agGameDir, agIsVersion3);
            // save version
            WriteGameSetting("General", "Interpreter", agIntVersion);
            // give access to compiler
            compGame = this;
            // finish the game load
            try {
                return FinishGameLoad(OpenGameMode.Directory);
            }
            catch (Exception ex) {
                // return an error
                TWinAGIEventInfo retval = new() {
                    Type = EventType.GameLoadError,
                    ID = "648",
                    Module = ex.StackTrace,
                    Text = ex.Message
                };
                return retval;
            }
        }

        /// <summary>
        /// Opens a WinAGI game file (must be passed as a full length file name).
        /// If successful, warning/load info is passed back.
        /// If fails, exception is thrown.
        /// </summary>
        /// <param name="GameWAG"></param>
        /// <returns></returns>
        private TWinAGIEventInfo OpenGameWAG(string GameWAG) {
            string strVer;

            if (!File.Exists(GameWAG)) {
                // invalid wag -
                // is the file missing?, or the directory?
                if (Directory.Exists(JustPath(GameWAG, true))) {
                    // it's a missing file - return wagfile as error string
                    WinAGIException wex = new(LoadResString(655).Replace(ARG1, GameWAG)) {
                        HResult = WINAGI_ERR + 655,
                    };
                    wex.Data["badfile"] = GameWAG;
                    throw wex;
                }
                else {
                    // it's an invalid or missing directory - return directory as error string
                    WinAGIException wex = new(LoadResString(541).Replace(ARG1, JustPath(GameWAG, true))) {
                        HResult = WINAGI_ERR + 541,
                    };
                    wex.Data["baddir"] = JustPath(GameWAG, true);
                    throw wex;
                }
            }
            // reset game variables
            ClearGameState();
            // set game file property
            agGameFile = GameWAG;
            // set game directory
            agGameDir = JustPath(GameWAG);
            // check for readonly (not allowed)
            if ((File.GetAttributes(agGameFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                WinAGIException wex = new(LoadResString(700).Replace(ARG1, GameWAG)) {
                    HResult = WINAGI_ERR + 700,
                };
                wex.Data["badfile"] = agGameFile;
                throw wex;
            }
            try {
                // open the WAG
                agGameProps = new SettingsFile(agGameFile, FileMode.Open);
            }
            catch (Exception e) {
                // reset game variables
                ClearGameState();
                WinAGIException wex = new(LoadResString(701)) {
                    HResult = WINAGI_ERR + 701,
                };
                wex.Data["exception"] = e;
                wex.Data["badfile"] = GameWAG;
                throw wex;
            }
            // check to see if it's valid
            strVer = agGameProps.GetSetting("General", "WinAGIVersion", "");
            if (strVer != WINAGI_VERSION) {
                if (strVer.Left(4) == "1.2." || (strVer.Left(2) == "2.")) {
                    // any v1.2.x or 2.x is ok, but user will need to update
                    // let calling function know an upgrade is occurring
                    TWinAGIEventInfo loadInfo = new() {
                        Type = EventType.Info,
                        Text = strVer,
                        InfoType = InfoType.Validating
                    };
                    OnLoadGameStatus(loadInfo);
                    // update the WinAGI version
                    WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION);
                }
                else {
                    // clear game variables
                    ClearGameState();
                    // invalid wag file, or invalid wag version
                    WinAGIException wex = new(LoadResString(665).Replace(ARG1, GameWAG)) {
                        HResult = WINAGI_ERR + 665,
                    };
                    wex.Data["badfile"] = GameWAG;
                    wex.Data["badversion"] = strVer;
                    throw wex;
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
                agIntVersion = agGameProps.GetSetting("General", "Interpreter", "");
                // validate it
                if (!IntVersions.Contains(agIntVersion)) {
                    string badver = agIntVersion;
                    // clear game variables
                    ClearGameState();
                    // invalid int version inside wag file
                    WinAGIException wex = new(LoadResString(691)) {
                        HResult = WINAGI_ERR + 691,
                    };
                    wex.Data["badversion"] = badver;
                    throw wex;
                }
            }
            else {
                // missing GameID in wag file - make user address it
                // save wagfile name as error string
                // clear game variables
                ClearGameState();
                // invalid wag file
                WinAGIException wex = new(LoadResString(690)) {
                    HResult = WINAGI_ERR + 690,
                    
                };
                wex.Data["badfile"] = GameWAG;
                throw wex;
            }
            // if a valid wag file was found, we now have agGameID, agGameFile
            // and correct interpreter version;
            // give access to compiler
            compGame = this;
            try {
                // finish the game load
                return FinishGameLoad(OpenGameMode.File);
            }
            catch (Exception ex) {
                // return an error
                TWinAGIEventInfo retval = new() {
                    Type = EventType.GameLoadError,
                    ID = "650",
                    Module = ex.StackTrace,
                    Text = ex.Message
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
        public TWinAGIEventInfo FinishGameLoad(OpenGameMode mode) {
            TWinAGIEventInfo retval = new() {
                Type = EventType.Info,
                InfoType = InfoType.Done,
            };
            // provide feedback to calling function
            bool blnWarnings = false;
            TWinAGIEventInfo loadInfo = new();

            agIsVersion3 = agIntVersion[0] == '3';
            switch (mode) {
            case OpenGameMode.File:
                loadInfo.InfoType = InfoType.PropertyFile;
                OnLoadGameStatus(loadInfo);
                // get resdir before loading resources
                agResDirName = agGameProps.GetSetting("General", "ResDir", "");
                break;
            case OpenGameMode.Directory:
                // no resdir yet for imported game: check for existing, or use default
                if (Directory.GetDirectories(agGameDir).Length == 1) {
                    // assume it is resource directory
                    DirectoryInfo tmp = new(Directory.GetDirectories(agGameDir)[0]);
                    agResDirName = tmp.Name;
                }
                else {
                    // either no subfolders, or more than one
                    // so use default
                    agResDirName = agDefResDir;
                }
                WriteGameSetting("General", "ResDir", Path.GetFileName(agResDirName));
                // if a set.game.id command is found during decompilation
                // it will be used as the suggested ID
                DecodeGameID = "";
                break;
            case OpenGameMode.New:
                // resdirname already known
                break;
            }
            // now create full resdir from name
            agResDir = agGameDir + agResDirName + @"\";
            // ensure resource directory exists
            if (!Directory.Exists(agResDir)) {
                try {
                    Directory.CreateDirectory(agResDir);
                }
                catch (Exception) {
                    // if can't create the resources directory
                    // note the problem as a warning
                    loadInfo.ResType = AGIResType.Game;
                    loadInfo.Type = EventType.ResourceWarning;
                    loadInfo.ID = "OW01";
                    loadInfo.Text = "Can't create " + agResDir;
                    LoadEventStatus(mode, loadInfo);
                    // use game directory
                    agResDir = agGameDir;
                    // set warning
                    blnWarnings = true;
                }
            }
            try {
                blnWarnings |= ExtractResources(this, mode);
            }
            catch (Exception) {
                // pass it along
                throw;
            }
            try {
                // get rest of game properties
                GetGameProperties();
            }
            catch (Exception e) {
                // note the problem as a warning
                loadInfo.ResType = AGIResType.Game;
                loadInfo.Type = EventType.ResourceWarning;
                loadInfo.ID = "OW02";
                loadInfo.Text = $"Error while loading WAG file; some properties not loaded. (Error {e.HResult}: {e.Message})";
                LoadEventStatus(mode, loadInfo);
                // set warning
                blnWarnings = true;
            }
            // load vocabulary word list
            loadInfo.Type = EventType.Info;
            loadInfo.InfoType = InfoType.Resources;
            loadInfo.ResType = AGIResType.Words;
            LoadEventStatus(mode, loadInfo);
            try {
                agVocabWords = new WordList(this);
                agVocabWords.Load(agGameDir + "WORDS.TOK");
            }
            catch (Exception e) {
                // if there was an error,
                // note the problem as a warning
                loadInfo.ResType = AGIResType.Words;
                loadInfo.Type = EventType.ResourceWarning;
                loadInfo.ID = "RE19";
                loadInfo.Text = $"An error occurred while loading WORDS.TOK (Error {e.HResult}: {e.Message}";
                loadInfo.Module = "WORDS.TOK";
                LoadEventStatus(mode, loadInfo);
                // set warning flag
                blnWarnings = true;
            }
            if (agVocabWords.ErrLevel != 0) {
                // note the problem as a warning
                AddLoadWarning(mode, this, AGIResType.Words, 0, agVocabWords.ErrLevel, []);
                blnWarnings = true;
            }
            // get description, if there is one
            agVocabWords.Description = agGameProps.GetSetting("WORDS.TOK", "Description", "", true);
            // load inventory objects list
            loadInfo.Type = EventType.Info;
            loadInfo.InfoType = InfoType.Resources;
            loadInfo.ResType = AGIResType.Objects;
            LoadEventStatus(mode, loadInfo);
            try {
                agInvObj = new InventoryList(this);
                agInvObj.Load();
            }
            catch (Exception e) {
                // if there was an error,
                // note the problem as a warning
                loadInfo.ResType = AGIResType.Objects;
                loadInfo.Type = EventType.ResourceWarning;
                loadInfo.ID = "RE21";
                loadInfo.Text = $"An error occurred while loading OBJECT:(Error {e.HResult}: {e.Message}";
                loadInfo.Module = "OBJECT";
                LoadEventStatus(mode, loadInfo);
                blnWarnings = true;
            }
            // get description, if there is one
            agInvObj.Description = agGameProps.GetSetting("OBJECT", "Description", "", true);
            // check for warnings
            if (agInvObj.ErrLevel != 0) {
                // note the problem as a warning
                AddLoadWarning(mode, this, AGIResType.Objects, 0, agInvObj.ErrLevel, []);
                blnWarnings = true;
            }
            // reserved defines
            agReservedDefines = new(this);
            // load globals file
            agGlobals = new GlobalList(this);
            agGlobals.Load();
            if (agGlobals.ErrLevel != 0) {
                // note the problem as a warning
                AddLoadWarning(mode, this, AGIResType.Globals, 0, agGlobals.ErrLevel, []);
                blnWarnings = true;
            }
            // adust commands based on AGI version
            CorrectCommands(agIntVersion);
            // check for decompile warnings, TODO entries and validate logic CRC values
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
                    // cache error level 
                    int tmpErr = tmpLog.ErrLevel;
                    // if there is a source file, need to verify source CRC value
                    if (File.Exists(tmpLog.SourceFile)) {
                        // recalculate the CRC value for this sourcefile by loading the source
                        tmpLog.LoadSource();
                        // check it for TODO items
                        List<TWinAGIEventInfo> TODOs = ExtractTODO(tmpLog.Number, tmpLog.SourceText, tmpLog.ID);
                        foreach (TWinAGIEventInfo tmpInfo in TODOs) {
                            LoadEventStatus(mode, tmpInfo);
                        }
                        List<TWinAGIEventInfo> DecompWarnings = ExtractDecompWarn(tmpLog.Number, tmpLog.SourceText, tmpLog.ID);
                        foreach (TWinAGIEventInfo tmpInfo in DecompWarnings) {
                            LoadEventStatus(mode, tmpInfo);
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
                    // source error if applicable
                    if (tmpErr != tmpLog.ErrLevel && tmpLog.ErrLevel < 0) {
                        AddLoadWarning(mode, this, AGIResType.Logic, tmpLog.Number, tmpLog.ErrLevel, tmpLog.ErrData);
                        blnWarnings = true;
                    }
                    break;
                case OpenGameMode.Directory:
                    // extracting from game directory
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
                    // force decompile
                    tmpLog.LoadSource(true);
                    if (tmpLog.ErrLevel < 0) {
                        AddLoadWarning(mode, this, AGIResType.Logic, tmpLog.Number, tmpLog.ErrLevel, tmpLog.ErrData);
                        blnWarnings = true;
                    }
                    // unload the logic after decompile is done
                    tmpLog.Unload();
                    // if a new ID found, use it
                    if (DecodeGameID.Length != 0) {
                        agGameID = DecodeGameID;
                        DecodeGameID = "";
                        SafeFileMove(agGameFile, agGameDir + agGameID + ".wag", true);
                        agGameFile = agGameDir + agGameID + ".wag";
                        agGameProps.Filename = agGameFile;
                        WriteGameSetting("General", "GameID", agGameID);
                    }
                    break;
                }
            }
            // everything loaded OK; tidy things up before exiting
            loadInfo.Type = EventType.Info;
            loadInfo.InfoType = InfoType.Finalizing;
            LoadEventStatus(mode, loadInfo);
            // force id reset
            blnSetIDs = false;
            SetResourceIDs(this);
            if (mode == OpenGameMode.Directory) {
                // write create date
                WriteGameSetting("General", "LastEdit", agLastEdit.ToString());
            }
            // if warnings or errors
            if (blnWarnings) {
                // TODO: warning info not used by calling function
                // so maybe get rid of it?
                retval.Type = EventType.ResourceWarning;
                retval.ID = 636.ToString();
                retval.Text = "WARNINGS";
            }
            // TODO: enable file watcher
            //agFileWatcher = new(agGameDir);
            //agFileWatcher.Enabled = true;
            return retval;
        }

        internal void LoadEventStatus(OpenGameMode mode, TWinAGIEventInfo loadInfo) {
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
            agReservedDefines = null;
            agGlobals = null;// new GlobalList(this);
            // clear out game properties
            agGameID = "";
            agIntVersion = "2.917";
            agIsVersion3 = false;
            agGameProps = new SettingsFile();
            agLastEdit = new DateTime();
            agSierraSyntax = false;
            agPowerPack = false;
            agLoadWarnings = false;
            // reset directories
            agGameDir = "";
            agResDir = "";
            agGameFile = "";
            agSrcFileExt = agDefSrcExt;
            // clear dev properties
            agDescription = "";
            agAuthor = "";
            agGameVersion = "";
            agGameAbout = "";
            agResDirName = "";
            agPlatformType = PlatformType.None;
            agPlatformFile = "";
            agPlatformOpts = "";
            agDOSExec = "";
            // other properties
            DecodeGameID = "";
            IndentSize = 4;
            // release compGame reference
            compGame = null;
        }

        /// <summary>
        /// This function will determine if the strDir is a valid Sierra AGI
        /// game directory. It also sets the gameID, if one is found, and the
        /// version3 flag.
        /// </summary>
        /// <param name="strDir"></param>
        /// <returns></returns>
        public bool IsValidGameDir(string strDir) {
            string strFile;
            byte[] bChunk = new byte[6];
            FileStream fsCOM;
            int dirCount;

            try {
                // search for 'DIR' files
                dirCount = Directory.EnumerateFiles(strDir, "*DIR").Count();
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
                    if (!File.Exists(strDir + "LOGDIR")) {
                        return false;
                    }
                    if (!File.Exists(strDir + "PICDIR")) {
                        return false;
                    }
                    if (!File.Exists(strDir + "SNDDIR")) {
                        return false;
                    }
                    if (!File.Exists(strDir + "VIEWDIR")) {
                        return false;
                    }
                    // DIR files check out - assume it's a v2 game

                    // check for at least one VOL file
                    if (File.Exists(strDir + "VOL.0")) {
                        // clear version3 flag
                        agIsVersion3 = false;
                        // clear ID
                        agGameID = "";
                        // look for loader file to find ID
                        foreach (string strLoader in Directory.EnumerateFiles(strDir, "*.COM")) {
                            // open file and get chunk
                            string strChunk = new(' ', 6);
                            try {
                                using (fsCOM = new FileStream(strLoader, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                                    // see if the word 'LOADER' is at position 3 of the file
                                    fsCOM.Position = 3;
                                    fsCOM.Read(bChunk, 0, 6);
                                    strChunk = Encoding.UTF8.GetString(bChunk);
                                    fsCOM.Dispose();
                                    //if this is a Sierra loader
                                    if (strChunk == "LOADER") {
                                        // determine ID to use based on loader filename
                                        strFile = Path.GetFileName(strLoader);
                                        if (strLoader != "SIERRA.COM") {
                                            // use this filename as ID
                                            agGameID = strFile.Left(strFile.Length - 4).ToUpper();
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
                    strFile = Path.GetFileName(Directory.GetFiles(strDir, "*DIR")[0].ToUpper());
                    agGameID = strFile.Left(strFile.IndexOf("DIR"));
                    // check for matching VOL file;
                    if (File.Exists(strDir + agGameID + "VOL.0")) {
                        // set version3 flag
                        agIsVersion3 = true;
                    }
                    else {
                        // if no vol file, assume not valid
                        return false;
                    }
                }
                // DIR/VOL files found; ID set; look for OBJECT/WORDS.TOK
                if (!File.Exists(strDir + "OBJECT")) {
                    return false;
                }
                if (!File.Exists(strDir + "WORDS.TOK")) {
                    return false;
                }
                // all necessary files exist; this is a valid AGI directory
                return true;
            }
            // no valid files found; not an AGI directory
            return false;
        }

        /// <summary>
        /// Writes a string value to this game's WinAGI Game File. File is automatically
        /// saved after writing.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        internal void WriteGameSetting(string Section, string Key, string Value, string Group = "") {
            agGameProps.WriteSetting(Section, Key, Value, Group);
            if (!Key.Equals("lastedit", StringComparison.CurrentCultureIgnoreCase) && !Key.Equals("winagiversion", StringComparison.CurrentCultureIgnoreCase) && !Key.Equals("palette", StringComparison.CurrentCultureIgnoreCase)) {
                agLastEdit = DateTime.Now;
            }
            // always save settings file
            agGameProps.Save();
        }

        /// <summary>
        /// Writes an integer value to this game's WinAGI Game File. File is automatically
        /// saved after writing.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        internal void WriteGameSetting(string Section, string Key, int Value, string Group = "") {
            WriteGameSetting(Section, Key, Value.ToString(), Group);
        }

        /// <summary>
        /// Writes a boolean value to this game's WinAGI Game File. File is automatically 
        /// saved after writing.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        internal void WriteGameSetting(string Section, string Key, bool Value, string Group = "") {
            WriteGameSetting(Section, Key, Value.ToString(), Group);
        }

        /// <summary>
        /// Provides calling programs a way to write property values to the WinAGI
        /// Game File.</summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        public void WriteProperty(string Section, string Key, string Value, string Group = "") {
            WriteGameSetting(Section, Key, Value, Group);
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
            //     Decompiler:
            //         SrcExt

            // ASSUMES a valid game property file has been loaded
            // loads only these properties:
            //     General:
            //         codepage
            //         description
            //         author
            //         about
            //         game version
            //         last date
            //         platform, platform program, platform options, dos executable
            //         use res names property
            //         use layout editor
            //         sierra syntax mode
            //     Palette:
            //         all colors
            //     Decompiler:
            //         logic sourcefile extension

            // Palette:
            for (int i = 0; i < 16; i++) {
                Palette[i] = agGameProps.GetSetting("Palette", "Color" + i.ToString(), DefaultPalette[i]);
            }
            agCodePage = Encoding.GetEncoding(agGameProps.GetSetting("General", "CodePage", 437));
            agDescription = agGameProps.GetSetting("General", "Description", "");
            agAuthor = agGameProps.GetSetting("General", "Author", "");
            agGameAbout = agGameProps.GetSetting("General", "About", "").Replace("\n", "\\n");
            agGameVersion = agGameProps.GetSetting("General", "GameVersion", "").Replace("\n", "\\n");
            if (!DateTime.TryParse(agGameProps.GetSetting("General", "LastEdit", DateTime.Now.ToString()), out agLastEdit)) {
                // default to now
                agLastEdit = DateTime.Now;
            }
            agPlatformType = (PlatformType)agGameProps.GetSetting("General", "PlatformType", 0, typeof(PlatformType));
            agPlatformFile = agGameProps.GetSetting("General", "Platform", "");
            agDOSExec = agGameProps.GetSetting("General", "DOSExec", "");
            agPlatformOpts = agGameProps.GetSetting("General", "PlatformOpts", "");
            agIncludeReserved = agGameProps.GetSetting("General", "IncludeReserved", true);
            agIncludeIDs = agGameProps.GetSetting("General", "IncludeIDs", true);
            agIncludeGlobals = agGameProps.GetSetting("General", "IncludeGlobals", true);
            agUseLE = agGameProps.GetSetting("General", "UseLE", false);
            agSierraSyntax = agGameProps.GetSetting("General", "SierraSyntax", defaultSierraSyntax);
            agSrcFileExt = agGameProps.GetSetting("Decompiler", "SourceFileExt", agDefSrcExt).ToLower().Trim();
            if (agSrcFileExt[0] == '.') {
                agSrcFileExt = agSrcFileExt[1..];
            }
        }

        /// <summary>
        /// Cleans up after a compile game cancel or error.
        /// </summary>
        /// <param name="NoEvent"></param>
        internal void CompleteCancel(bool NoEvent = false) {
            if (!NoEvent) {
                TWinAGIEventInfo tmpWarn = new() {
                    Type = EventType.ResourceWarning,
                    ResType = AGIResType.Game,
                };
                OnCompileGameStatus(GameCompileStatus.Canceled, tmpWarn);
            }
            Compiling = false;
            CancelComp = false;
            volManager.Clear();
        }

        public CompileStatus CompileChangedLogics() {
            bool blnUnloadRes;
            TWinAGIEventInfo compInfo = new();

            Compiling = true;
            CancelComp = false;

            foreach (Logic logres in Logics) {
                // update status
                TWinAGIEventInfo tmpWarn = new() {
                    Type = EventType.Info,
                    InfoType = InfoType.ClearWarnings,
                    ResType = AGIResType.Logic,
                    ResNum = logres.Number,
                    ID = logres.ID,
                };
                // clear existing warnings/errors
                AGIGame.OnCompileGameStatus(GameCompileStatus.AddResource, tmpWarn, ref CancelComp);
                if (CancelComp) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                // update status and check for cancellation
                tmpWarn.InfoType = InfoType.CheckLogic;
                AGIGame.OnCompileGameStatus(GameCompileStatus.AddResource, tmpWarn, ref CancelComp);
                if (CancelComp) {
                    CompleteCancel();
                    return CompileStatus.Canceled;
                }
                // set flag to force unload, if resource not currently loaded
                blnUnloadRes = !logres.Loaded;
                // then load resource if necessary
                if (blnUnloadRes) {
                    // load resource
                    logres.Load();
                }
                if (logres.SrcErrLevel != 0) {
                    // add events
                    AddCompileWarning(AGIResType.Logic, logres.Number, logres.ErrLevel, []);
                    // for logics, check for source errors (compile automatically fails)
                    // -1  logic source: file does not exist
                    // -2  logic source: file is readonly
                    // -3  logic source: file access error
                    tmpWarn.Text = $"Unable to load source code for {logres.ID}";
                    tmpWarn.Line = logres.ErrLevel.ToString();
                    AGIGame.OnCompileGameStatus(GameCompileStatus.ResError, tmpWarn);
                    // make sure unloaded
                    logres.Unload();
                    // and stop compiling
                    CompleteCancel(true);
                    return CompileStatus.Error;
                }
                if (!logres.Compiled) {
                    // update status and check for cancellation
                    tmpWarn.InfoType = InfoType.Compiling;
                    AGIGame.OnCompileGameStatus(GameCompileStatus.AddResource, tmpWarn, ref CancelComp);
                    if (CancelComp) {
                        CompleteCancel();
                        return CompileStatus.Canceled;
                    }
                    if (!CompileLogic(logres)) {
                        // logic compile critical error - always cancel
                        // unload if needed
                        if (blnUnloadRes && logres is not null) logres.Unload();
                        // then stop compiling
                        CompleteCancel(true);
                        return CompileStatus.Error;
                    }
                    // save the updated resource data (updates VOL/DIR files)
                    try {
                        ((AGIResource)logres).Save();
                    }
                    catch (Exception e) {
                        // report any errors
                        tmpWarn.Text = $"Unable to save {logres.ID} to game files.";
                        tmpWarn.Line = e.Message;
                        AGIGame.OnCompileGameStatus(GameCompileStatus.ResError, tmpWarn);
                        // make sure unloaded
                        logres.Unload();
                        // and stop compiling
                        CompleteCancel(true);
                        return CompileStatus.Error;
                    }
                    // update status and check for cancellation
                    tmpWarn.InfoType = InfoType.Compiled;
                    AGIGame.OnCompileGameStatus(GameCompileStatus.AddResource, tmpWarn, ref CancelComp);
                }
            }
            return CompileStatus.OK;
        }
        #endregion
    }
}