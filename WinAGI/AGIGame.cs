using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        internal int agMaxVol0 = 0;
        internal int agMaxVolSize = 0;
        internal string agCompileDir = "";
        internal PlatformTypeEnum agPlatformType = PlatformTypeEnum.None;
        internal string agPlatformFile = "";
        internal string agPlatformOpts = "";
        internal string agDOSExec = "";
        internal bool agUseLE = false;
        internal Encoding agCodePage = Encoding.GetEncoding(437);
        internal bool agPowerPack = false;
        internal string agSrcFileExt = "";
        internal TDefine[] agResGameDef = new TDefine[4];
        internal SettingsList agGameProps;
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
            TWinAGIEventInfo retval = new() {
                Type = EventType.etInfo,
                ID = "",
                Module = "",
                Text = ""
            };
            switch (mode) {
            case OpenGameMode.File:
                retval = OpenGameWAG(gameSource);
                break;
            case OpenGameMode.Directory:
                retval = OpenGameDIR(gameSource);
                break;
            }
            if (retval.Type == EventType.etError) {
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
        /// Constructor used to create a new game from a template or as an empty
        /// game with no resources.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="gamedir"></param>
        /// <param name="resdir"></param>
        /// <param name="template"></param>
        public AGIGame(string id, string version, string gamedir, string resdir, string template = "") {
            InitGame();
            // give compiler access
            compGame = this;
            // create a new, blank game
            NewGame(id, version, gamedir, resdir, template);
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

        /// <summary>
        /// Gets the AGI color palette that is used for displaying pictures and views.
        /// </summary>
        public EGAColors AGIColors { get => agEGAcolors; }

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
                        NewID = Left(NewID, 5);
                    }
                }
                else {
                    if (NewID.Length > 6) {
                        NewID = Left(NewID, 6);
                    }
                }
                if (agGameID == NewID) {
                    return;
                }
                if (agIsVersion3) {
                    try {
                        File.Move(agGameDir + agGameID + "DIR", agGameDir + NewID + "DIR");
                        foreach (string strVolFile in Directory.EnumerateFiles(agGameDir, agGameID + "VOL.*")) {
                            string[] strExtension = strVolFile.Split(".");
                            File.Move(agGameDir + strVolFile, agGameDir + NewID + "VOL." + strExtension[1]);
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
                // in this property (but not moved or created)

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
                    // ignore errors
                }
                agGameFile = value;
                agLastEdit = DateTime.Now;
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
                    agGameAbout = Left(value, 4096);
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
                    agAuthor = Left(value, 256);
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
                    agDescription = Left(value, 4096);
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
                    if (!IsNumeric(value)) {
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
                            GameID = Left(GameID, 5);
                        }
                    }
                    // TODO: test- confirm v3 files get decompressed before compiling
                    // use compiler to rebuild new vol and dir files
                    // (it is up to calling program to deal with dirty and invalid resources)
                    try {
                        if (!CompileGame(true)) {
                            return;
                        }
                    }
                    catch {
                        throw;
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
                if (Path.GetInvalidPathChars().Any(tmpName.Contains)) {
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
        /// Gets the reserved defines that are game-specific(GameVersion, GameAbout,
        /// GameID, InvItem Count).
        /// </summary>
        public TDefine[] ReservedGameDefines {
            get {
                // refresh before returning
                agResGameDef[0].Value = "\"" + agGameID + "\"";
                agResGameDef[1].Value = "\"" + agGameVersion + "\"";
                agResGameDef[2].Value = "\"" + agGameAbout + "\"";
                agResGameDef[3].Value = agInvObj.Count.ToString();
                return agResGameDef;
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
        /// Gets or sets the source extension that this game will use when decompiling logics
        /// or creating new source code files.
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
                // must start with a period
                if (value[0] != '.') {
                    agSrcFileExt = "." + value;
                }
                else {
                    agSrcFileExt = value;
                }
                WriteGameSetting("General", "PlatformType", agPlatformType.ToString());
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
        /// Internal flag that lets othervgame resources know that a game is currently 
        /// being compiled.
        /// </summary>
        internal bool Compiling { get; set; } = false;

        /// <summary>
        /// The game compiler needs to know if compiling is happening to because of
        /// a change in version to or from v2 and v3.
        /// </summary>
        private bool V3V2Swap { get; set; } = false;

        /// <summary>
        /// Gets or sets the platform to use when testing this game from the 
        /// game editor.
        /// </summary>
        public PlatformTypeEnum PlatformType {
            get => agPlatformType;
            set {
                // only 0 - 4 are valid
                if (value < 0 || value > (PlatformTypeEnum)4) {
                    agPlatformType = 0;
                }
                else {
                    agPlatformType = value;
                }
                WriteGameSetting("General", "PlatformType", agPlatformType.ToString());
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
                switch (value.CodePage) {
                case 437 or 737 or 775 or 850 or 852 or 855 or 857 or 860 or
                     861 or 862 or 863 or 865 or 866 or 869 or 858:
                    agCodePage = value;
                    WriteGameSetting("General", "CodePage", agCodePage.CodePage);
                    break;
                default:
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
                    agGameVersion = Left(value, 256);
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
            // get default max vol sizes
            agMaxVolSize = 1023 * 1024;
            // set max vol0 size
            agMaxVol0 = agMaxVolSize;

            // reserved game defines
            agResGameDef[0].Name = "gameID";
            agResGameDef[0].Default = "gameID";
            agResGameDef[0].Type = ArgTypeEnum.atDefStr;
            agResGameDef[1].Name = "gameVersion";
            agResGameDef[1].Default = "gameVersion";
            agResGameDef[1].Type = ArgTypeEnum.atDefStr;
            agResGameDef[2].Name = "gameAbout";
            agResGameDef[2].Default = "gameAbout";
            agResGameDef[2].Type = ArgTypeEnum.atDefStr;
            agResGameDef[3].Name = "numberOfItems";
            agResGameDef[3].Default = "numberOfItems";
            agResGameDef[3].Type = ArgTypeEnum.atNum;

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
            //restore default AGI colors
            DefaultColors = new();
            //write date of last edit
            WriteGameSetting("General", "LastEdit", agLastEdit.ToString());
            //now save it
            agGameProps.Save();
            // clear all game properties
            ClearGameState();
            // release compiler access
            compGame = null;
        }

        /// <summary>
        /// Compiles the game into NewGameDir, resulting in new VOL and DIR
        /// files that eliminate all dead space.All logics are recompiled 
        /// before being added. Resources with errors will pause until the
        /// calling program provides instructions to  skip them, or add them
        /// with the errors. WORDS.TOK and OBJECT filesare only recompiled if
        /// currently dirty. If RebuildOnly is true, the VOL files are rebuilt
        /// without recompiling all logics and WORDS.TOK and OBJECT are not
        /// recompiled.<br />
        /// If NewGameDir is same as current directory this WILL overwrite
        /// current game files.
        /// </summary>
        /// <param name="RebuildOnly"></param>
        /// <param name="NewGameDir"></param>
        /// <returns></returns>
        public bool CompileGame(bool RebuildOnly, string NewGameDir = "") {
            bool blnReplace, NewIsV3;
            string strID;
            int tmpMax = 0, i, j;
            TWinAGIEventInfo compInfo = new() {
                Type = EventType.etInfo,
                ID = "",
                Module = "",
                Text = ""
            };

            Compiling = true;
            // if no directory passed assume current dir
            if (NewGameDir.Length == 0) {
                NewGameDir = agGameDir;
            }
            NewGameDir = FullDir(NewGameDir);
            if (!Directory.Exists(NewGameDir)) {
                CompleteCancel(true);
                WinAGIException wex = new(LoadResString(561).Replace(ARG1, NewGameDir)) {
                    HResult = WINAGI_ERR + 561
                };
                throw wex;
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
                // version 3 ids limited to 5 characters
                if (agGameID.Length > 5) {
                    // invalid ID; calling function should know better!
                    CompleteCancel(true);
                    WinAGIException wex = new(LoadResString(694)) {
                        HResult = WINAGI_ERR + 694
                    };
                    throw wex;
                }
                strID = agGameID;
            }
            else {
                strID = "";
            }
            if (!RebuildOnly) {
                // full compile - save/copy words.tok and object files first
                compInfo.Text = "";
                compInfo.ResType = AGIResType.Words;
                if (OnCompileGameStatus(ECStatus.csCompWords, compInfo)) {
                    CompleteCancel();
                    return false;
                }
                if (agVocabWords.IsDirty) {
                    try {
                        agVocabWords.Save();
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.etError,
                            ID = "",
                            Module = "",
                            ResType = AGIResType.Words,
                            Text = "Error during compilation of WORDS.TOK (" + ex.Message + ")"
                        };
                        _ = OnCompileGameStatus(ECStatus.csResError, tmpError);
                        CompleteCancel();
                        return false;
                    }
                }
                if (!blnReplace) {
                    // copy WORDS.TOK to new folder
                    try {
                        // rename then delete existing file, if it exists
                        if (File.Exists(NewGameDir + "WORDS.TOK")) {
                            // delete the 'old' file if it exists
                            if (File.Exists(NewGameDir + "WORDS_OLD.TOK")) {
                                File.Delete(NewGameDir + "WORDS_OLD.TOK");
                            }
                            File.Move(NewGameDir + "WORDS.TOK", NewGameDir + "WORDS_OLD.TOK");
                        }
                        // then copy the current file to new location
                        File.Copy(agVocabWords.ResFile, NewGameDir + "WORDS.TOK");
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.etError,
                            ID = "",
                            Module = "",
                            ResType = AGIResType.Words,
                            Text = "Error while creating WORDS.TOK file (" + ex.Message + ")"
                        };
                        _ = OnCompileGameStatus(ECStatus.csResError, tmpError);
                        CompleteCancel();
                        return false;
                    }
                }
                // OBJECT file is next
                compInfo.Text = "";
                compInfo.ResType = AGIResType.Objects;
                if (OnCompileGameStatus(ECStatus.csCompObjects, compInfo)) {
                    CompleteCancel();
                    return false;
                }
                if (agInvObj.IsDirty) {
                    try {
                        agInvObj.Save();
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.etError,
                            ID = "",
                            Module = "",
                            ResType = AGIResType.Objects,
                            Text = "Error during compilation of OBJECT (" + ex.Message + ")"
                        };
                        _ = OnCompileGameStatus(ECStatus.csResError, tmpError);
                        CompleteCancel();
                        return false;
                    }
                }
                if (!blnReplace) {
                    // copy OBJECT to new folder
                    try {
                        // rename then delete existing file, if it exists
                        if (File.Exists(NewGameDir + "OBJECT")) {
                            // first, delete the 'old' file if it exists
                            if (File.Exists(NewGameDir + "OBJECT_OLD")) {
                                File.Delete(NewGameDir + "OBJECT_OLD");
                            }
                            File.Move(NewGameDir + "OBJECT", NewGameDir + "OBJECT_OLD");
                        }
                        // then copy the current file to new location
                        File.Copy(agInvObj.ResFile, NewGameDir + "OBJECT");
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.etError,
                            ID = "",
                            Module = "",
                            ResType = AGIResType.Objects,
                            Text = "Error while creating OBJECT file (" + ex.Message + ")"
                        };
                        _ = OnCompileGameStatus(ECStatus.csResError, tmpError);
                        CompleteCancel();
                        return false;
                    }
                }
            }
            // reset game compiler variables
            volManager.Clear();

            // remove all temp vol files
            try {
                for (int v = 0; v < 15; v++) {
                    if (File.Exists(NewGameDir + "NEW_VOL." + v.ToString())) {
                        File.Delete(NewGameDir + "NEW_VOL." + v.ToString());
                    }
                }
            }
            catch (Exception) {
                // ignore errors
            }
            try {
                // open first new vol file
                volManager.InitVol(NewGameDir);
            }
            catch (Exception e) {
                CompleteCancel(true);
                WinAGIException wex = new(LoadResString(503).Replace(ARG1, NewGameDir + "NEW_VOL.0")) {
                    HResult = WINAGI_ERR + 503
                };
                wex.Data["exception"] = e;
                throw wex;
            }
            // add all logic resources
            try {
                if (!VOLManager.CompileResCol(this, agLogs, AGIResType.Logic, RebuildOnly)) {
                    // resource error (or user canceled)
                    CompleteCancel(true);
                    return false;
                }
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            // add all picture resources
            try {
                if (!VOLManager.CompileResCol(this, agPics, AGIResType.Picture, RebuildOnly)) {
                    // if a resource error (or user canceled) encountered, just exit
                    CompleteCancel(true);
                    return false;
                }
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            // add all view resources
            try {
                if (!VOLManager.CompileResCol(this, agViews, AGIResType.View, RebuildOnly)) {
                    // if a resource error (or user canceled) encountered, just exit
                    CompleteCancel(true);
                    return false;
                }
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            // add all sound resources
            try {
                if (!VOLManager.CompileResCol(this, agSnds, AGIResType.Sound, RebuildOnly)) {
            // if a resource error (or user canceled) encountered, just exit
                CompleteCancel(true);
                return false;
            }
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            // remove any existing old dirfiles
            if (NewIsV3) {
                if (File.Exists(NewGameDir + agGameID + "DIR_OLD")) {
                    File.Delete(NewGameDir + agGameID + "DIR_OLD");
                }
            }
            else {
                if (File.Exists(NewGameDir + "LOGDIR_OLD")) {
                    File.Delete(NewGameDir + "LOGDIR_OLD");
                }
                if (File.Exists(NewGameDir + "PICDIR_OLD")) {
                    File.Delete(NewGameDir + "PICDIR_OLD");
                }
                if (File.Exists(NewGameDir + "VIEWDIR_OLD")) {
                    File.Delete(NewGameDir + "VIEWDIR_OLD");
                }
                if (File.Exists(NewGameDir + "SNDDIR_OLD")) {
                    File.Delete(NewGameDir + "SNDDIR_OLD");
                }
            }
            // rename existing dir files as _OLD
            if (NewIsV3) {
                if (File.Exists(NewGameDir + agGameID + "DIR")) {
                    File.Move(NewGameDir + agGameID + "DIR", NewGameDir + agGameID + "DIR_OLD");
                    File.Delete(NewGameDir + agGameID + "DIR");
                }
            }
            else {
                if (File.Exists(NewGameDir + "LOGDIR")) {
                    File.Move(NewGameDir + "LOGDIR", NewGameDir + "LOGDIR_OLD");
                    File.Delete(NewGameDir + "LOGDIR");
                }
                if (File.Exists(NewGameDir + "PICDIR")) {
                    File.Move(NewGameDir + "PICDIR", NewGameDir + "PICDIR_OLD");
                    File.Delete(NewGameDir + "PICDIR");
                }
                if (File.Exists(NewGameDir + "VIEWDIR")) {
                    File.Move(NewGameDir + "VIEWDIR", NewGameDir + "VIEWDIR_OLD");
                    File.Delete(NewGameDir + "VIEWDIR");
                }
                if (File.Exists(NewGameDir + "SNDDIR")) {
                    File.Move(NewGameDir + "SNDDIR", NewGameDir + "SNDDIR_OLD");
                    File.Delete(NewGameDir + "SNDDIR");
                }
            }
            // now build the new DIR files
            if (NewIsV3) {
                volManager.DIRFile = File.Create(NewGameDir + agGameID + "DIR");
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
            // remove any existing old vol files
            for (i = 0; i < 16; i++) {
                if (NewIsV3) {
                    if (File.Exists(NewGameDir + agGameID + "VOL_OLD." + i.ToString())) {
                        File.Delete(NewGameDir + agGameID + "VOL_OLD." + i.ToString());
                    }
                }
                else {
                    if (File.Exists(NewGameDir + "VOL_OLD." + i.ToString())) {
                        File.Delete(NewGameDir + "VOL_OLD." + i.ToString());
                    }
                }
            }
            // rename current vol files
            for (i = 0; i < 16; i++) {
                if (NewIsV3) {
                    if (File.Exists(NewGameDir + agGameID + "VOL." + i.ToString())) {
                        File.Move(NewGameDir + agGameID + "VOL_OLD." + i.ToString(), NewGameDir + agGameID + "VOL." + i.ToString());
                    }
                }
                else {
                    if (File.Exists(NewGameDir + "VOL." + i.ToString())) {
                        File.Move(NewGameDir + "VOL_OLD." + i.ToString(), NewGameDir + "VOL." + i.ToString());
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
            _ = OnCompileGameStatus(ECStatus.csCompileComplete, compInfo);
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
            return false;
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
        private TWinAGIEventInfo NewGame(string NewID, string NewVersion, string NewGameDir, string NewResDir, string TemplateDir = "", string NewExt = "") {
            string strGameWAG, strTmplResDir, oldExt;
            int i;
            bool blnWarnings = false;
            SettingsList stlGlobals;
            // assume OK result
            TWinAGIEventInfo retval = new() {
                Type = EventType.etInfo,
                ID = "",
                Module = "",
                Text = ""
            };

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
            // start fresh
            ClearGameState();
            agGameDir = FullDir(NewGameDir);
            if (NewResDir.Length == 0) {
                NewResDir = agDefResDir;
            }
            if (NewExt.Length == 0) {
                NewExt = agDefSrcExt;
            }
            if (TemplateDir.Length != 0) {
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
                // get file name (it's first[and only] element)
                strGameWAG = Directory.GetFiles(TemplateDir, "*.wag")[0];
                // template should have at least one subdirectory
                if (Directory.GetDirectories(TemplateDir).Length == 0) {
                    // no resource directory, use main directory
                    strTmplResDir = "";
                }
                else {
                    // retrieve name of the first directory as resource dir
                    strTmplResDir = Directory.GetDirectories(TemplateDir)[0];
                }
                // need to open current wag file to get original extension
                // open the property file (file has to already exist)
                try {
                    SettingsList oldProps = new(agGameFile, FileMode.Open);
                    oldExt = oldProps.GetSetting("Decompiler", "SourceFileExt", DefaultSrcExt);
                    oldProps = null;
                }
                catch (Exception) {
                    oldExt = DefaultSrcExt;
                }
                // copy all files from the templatedir into gamedir
                try {
                    CopyDirectory(TemplateDir, agGameDir, true);
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(683).Replace(ARG1, e.Message)) {
                        HResult = WINAGI_ERR + 683
                    };
                    wex.Data["exception"] = e;
                    throw wex;
                }
                if (!oldExt.Equals(NewExt, StringComparison.OrdinalIgnoreCase)) {
                    // update source extension BEFORE opening wag file
                    SettingsList newProps = new(agGameDir + strGameWAG, FileMode.Open);
                    newProps.WriteSetting("General", "SourceFileExt", NewExt);
                    newProps.Save();
                }
                // open the game in the newly created directory
                try {
                    // open game with template id
                    OpenGameWAG(agGameDir + strGameWAG);
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(684).Replace(ARG1, e.Message)) {
                        HResult = WINAGI_ERR + 684
                    };
                    wex.Data["exception"] = e;
                    throw wex;
                }
                // then change the resource directory property
                agResDirName = NewResDir;
                // update the actual resdir
                agResDir = agGameDir + agResDirName + @"\";
                // rename the resdir (have to do this AFTER load, because loading 
                // will keep the current resdir that's in the WAG file)
                if (!NewResDir.Equals(strTmplResDir, StringComparison.OrdinalIgnoreCase)) {
                    // only move if there is an actual directory
                    if (strTmplResDir.Length > 0) {
                        DirectoryInfo resDir = new(agGameDir + strTmplResDir);
                        resDir.MoveTo(agGameDir + NewResDir);
                    }
                    // update all logics to new resdir
                    foreach (Logic tmpLog in Logics) {
                        tmpLog.mSourceFile = agResDir + tmpLog.ID + agSrcFileExt;
                        WriteGameSetting("Logic" + tmpLog.Number, "ID", tmpLog.ID, "Logics");
                    }
                }
                // change gameid
                try {
                    GameID = NewID;
                }
                catch (Exception e) {
                    WinAGIException wex = new(LoadResString(685).Replace(ARG1, e.Message)) {
                        HResult = WINAGI_ERR + 685
                    };
                    wex.Data["exception"] = e;
                    throw wex;
                }
                // change game propfile name
                agGameFile = agGameDir + NewID + ".wag";
                File.Move(agGameDir + strGameWAG, agGameFile, true);
                // rename wal file, if present
                if (File.Exists(agGameDir + Path.GetFileNameWithoutExtension(strGameWAG) + ".wal")) {
                    File.Move(agGameDir + Path.GetFileNameWithoutExtension(strGameWAG) + ".wal", agGameDir + NewID + ".wal");
                }
                // update global file header
                stlGlobals = new SettingsList(agGameDir + "globals.txt", FileMode.OpenOrCreate);
                if (stlGlobals.Lines.Count > 3) {
                    if (Left(stlGlobals.Lines[2].Trim(), 1) == "[") {
                        stlGlobals.Lines[2] = "[ global defines file for " + NewID;
                    }
                    // save it
                    stlGlobals.Save();
                }
            }
            // if not using template,
            else {
                if (!IntVersions.Contains(NewVersion)) {
                    if (Val(NewVersion) < 2 || Val(NewVersion) > 3) {
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
                agIsVersion3 = (Val(NewVersion) > 3);
                // set game id (limit to 6 characters for v2, and 5 characters for v3
                if (agIsVersion3) {
                    agGameID = Left(NewID, 5);
                }
                else {
                    agGameID = Left(NewID, 6);
                }
                // create empty property file
                agGameFile = agGameDir + agGameID + ".wag";
                agGameProps = new SettingsList(agGameFile, FileMode.Create);
                agGameProps.Lines.Add("#");
                agGameProps.Lines.Add("# WinAGI Game Property File for " + agGameID);
                agGameProps.Lines.Add("#");
                agGameProps.Lines.Add("[General]");
                agGameProps.Lines.Add("");
                agGameProps.Lines.Add("[Palette]");
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
                // add WinAGI version
                WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION);
                // set the resource directory name so it can be set up
                ResDirName = NewResDir;
                // assign source file extension
                agSrcFileExt = "." + NewExt.ToLower();
                // create default resource DIR files
                // (default DIR files shold only have ONE 'FFFFFF' entry)
                byte[] bytDirData = [0xff, 0xff, 0xff];
                if (agIsVersion3) {
                    // for v3, header should be '08 - 00 - 0B - 00 - 0E - 00 - 11 - 00
                    byte[] bytDirHdr = [8, 0, 0x0b, 0, 0x0e, 0, 0x11, 0];
                    using FileStream fsDIR = new(agGameDir + agGameID + "DIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirHdr);
                    for (i = 0; i < 4; i++) {
                        fsDIR.Write(bytDirData);
                    }
                    fsDIR.Dispose();
                }
                else {
                    FileStream fsDIR = new(agGameDir + "LOGDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR = new FileStream(agGameDir + "PICDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR = new FileStream(agGameDir + "SNDDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
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
                // set commands based on AGI version
                CorrectCommands(agIntVersion);
                // add logic zero
                agLogs.Add(0).Save();
                agLogs[0].Unload();
                // force id reset
                blnSetIDs = false;
            }
            // ensure resource directory exists
            if (!Directory.Exists(agGameDir + agResDirName)) {
                if (Directory.CreateDirectory(agGameDir + agResDirName) is null) {
                    // note the problem as a warning
                    TWinAGIEventInfo warnInfo = new() {
                        Type = EventType.etWarning,
                        ResType = AGIResType.Game,
                        ResNum = 0,
                        ID = "OW01",
                        Module = "",
                        Text = "Can't create " + agResDir,
                        Line = "--"
                    };
                    OnLoadGameStatus(warnInfo);
                    // use main directory
                    agResDir = agGameDir;
                    blnWarnings = true;
                }
            }
            // for non-template games, save the source code for logic 0
            if (TemplateDir.Length == 0) {
                agLogs[0].SaveSource();
            }
            // save gameID, version, directory resource name to the property file;
            // rest of properties need to be set by the calling function
            WriteGameSetting("General", "GameID", agGameID);
            WriteGameSetting("General", "Interpreter", agIntVersion);
            WriteGameSetting("General", "ResDir", agResDirName);
            WriteGameSetting("General", "SourceFileExt", agSrcFileExt[1..].ToUpper());
            // save palette colors
            for (i = 0; i < 16; i++) {
                WriteGameSetting("Palette", "Color" + i, agEGAcolors.ColorText(i));
            }
            // done
            retval.InfoType = EInfoType.itDone;
            if (blnWarnings) {
                retval.Type = EventType.etWarning;
                retval.Text = LoadResString(637);
            }
            else {
                retval.Type = EventType.etInfo;
            }
            return retval;
        }

        /// <summary>
        /// Creates a new WinAGI game file from Sierra game directory.
        /// If successful, warning/load info is passed back. 
        /// If fails, exception is thrown.
        /// </summary>
        /// <param name="NewGameDir"></param>
        /// <returns></returns>
        private TWinAGIEventInfo OpenGameDIR(string NewGameDir) {
            // assume OK result
            TWinAGIEventInfo retval = new() {
                Type = EventType.etInfo,
                ID = "",
                Module = "",
                Text = ""
            };
            // periodically report status of the load back to calling function
            TWinAGIEventInfo warnInfo = new() {
                Type = EventType.etWarning,
                ID = "",
                Module = "",
                Text = ""
            };
            // set game directory
            agGameDir = FullDir(NewGameDir);
            warnInfo.Type = EventType.etInfo;
            warnInfo.InfoType = EInfoType.itValidating;
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
            try {
                if (File.Exists(agGameFile)) {
                    if (File.Exists(oldFile)) {
                        File.Delete(oldFile);
                    }
                    File.Move(agGameFile, oldFile);
                }
            }
            catch (Exception e) {
                ClearGameState();
                // file error
                WinAGIException wex = new(LoadResString(699)) {
                    HResult = WINAGI_ERR + 699,
                };
                wex.Data["exception"] = e;
                throw wex;
            }
            // create new wag file
            agGameProps = new SettingsList(agGameFile, FileMode.Create);
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
            catch (Exception) {
                // pass it along
                throw;
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
            TWinAGIEventInfo retval = new() {
                Type = EventType.etInfo,
                ID = "",
                Module = "",
                Text = ""
            };
            // periodically report status of the load back to calling function
            TWinAGIEventInfo warnInfo = new() {
                Type = EventType.etWarning,
                ID = "",
                Module = "",
                Text = ""
            };
            string strVer;

            if (!File.Exists(GameWAG)) {
                // invalid wag
                retval.Type = EventType.etError;
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
                agGameProps = new SettingsList(agGameFile, FileMode.Open);
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
                if (Left(strVer, 4) == "1.2." || (Left(strVer, 2) == "2.")) {
                    // any v1.2.x or 2.x is ok, but user will need to update

                    // let calling function know an upgrade is occurring
                    TWinAGIEventInfo loadInfo = new() {
                        Type = EventType.etInfo,
                        ID = "",
                        Module = "",
                        Text = strVer,
                        InfoType = EInfoType.itValidating
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
            catch {
                // pass exceptions
                throw;
            }
        }

        /// <summary>
        /// Finishes game load. Mode determines whether opening by wag file or
        /// extracting from Sierra game files. If successful, warning/load info
        /// is passed back. If fails, exception is thrown.
        /// </summary>
        /// <param name="Mode"></param>
        /// <returns></returns>
        public TWinAGIEventInfo FinishGameLoad(OpenGameMode Mode) {

            TWinAGIEventInfo retval = new() {
                Type = EventType.etInfo,
                InfoType = EInfoType.itDone,
                ID = "",
                Text = "",
                Module = ""
            };
            // provide feedback to calling function
            bool blnWarnings = false;
            TWinAGIEventInfo loadInfo = new() {
                Type = EventType.etInfo,
                ID = "",
                Module = "",
                Text = ""
            };
            agIsVersion3 = (Val(agIntVersion) >= 3);
            // if loading from a wag file
            if (Mode == OpenGameMode.File) {
                loadInfo.InfoType = EInfoType.itPropertyFile;
                OnLoadGameStatus(loadInfo);
                // get resdir before loading resources
                agResDirName = agGameProps.GetSetting("General", "ResDir", "");
            }
            else {
                // no resdir yet for imported game
                agResDirName = "";
                // if a set.game.id command is found during decompilation
                // it will be used as the suggested ID
                DecodeGameID = "";
            }
            // if none, check for existing, or use default
            if (agResDirName.Length == 0) {
                // look for an existing directory
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
                    loadInfo.Type = EventType.etWarning;
                    loadInfo.ID = "OW01";
                    loadInfo.Text = "Can't create " + agResDir;
                    OnLoadGameStatus(loadInfo);
                    // use game directory
                    agResDir = agGameDir;
                    // set warning
                    blnWarnings = true;
                }
            }
            try {
                blnWarnings |= ExtractResources(this);
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
                loadInfo.Type = EventType.etWarning;
                loadInfo.ID = "OW02";
                loadInfo.Text = $"Error while loading WAG file; some properties not loaded. (Error {e.HResult}: {e.Message})";
                OnLoadGameStatus(loadInfo);
                // set warning
                blnWarnings = true;
            }
            // load vocabulary word list
            loadInfo.Type = EventType.etInfo;
            loadInfo.InfoType = EInfoType.itResources;
            loadInfo.ResType = AGIResType.Words;
            OnLoadGameStatus(loadInfo);
            try {
                agVocabWords = new WordList(this);
                agVocabWords.Load(agGameDir + "WORDS.TOK");
            }
            catch (Exception e) {
                // if there was an error,
                // note the problem as a warning
                loadInfo.ResType = AGIResType.Words;
                loadInfo.Type = EventType.etWarning;
                loadInfo.ID = "RW01";
                loadInfo.Text = $"An error occurred while loading WORDS.TOK (Error {e.HResult}: {e.Message}";
                loadInfo.Module = "WORDS.TOK";
                OnLoadGameStatus(loadInfo);
                // set warning flag
                blnWarnings = true;
            }
            if (agVocabWords.ErrLevel != 0) {
                // note the problem as a warning
                AddLoadWarning(this, AGIResType.Words, 0, agVocabWords.ErrLevel, []);
                blnWarnings = true;
            }
            // get description, if there is one
            agVocabWords.Description = agGameProps.GetSetting("WORDS.TOK", "Description", "", true);
            // load inventory objects list
            loadInfo.Type = EventType.etInfo;
            loadInfo.InfoType = EInfoType.itResources;
            loadInfo.ResType = AGIResType.Objects;
            OnLoadGameStatus(loadInfo);
            try {
                agInvObj = new InventoryList(this);
                agInvObj.Load();
            }
            catch (Exception e) {
                // if there was an error,
                // note the problem as a warning
                loadInfo.ResType = AGIResType.Objects;
                loadInfo.Type = EventType.etWarning;
                loadInfo.ID = "RW03";
                loadInfo.Text = $"An error occurred while loading OBJECT:(Error {e.HResult}: {e.Message}";
                loadInfo.Module = "OBJECT";
                OnLoadGameStatus(loadInfo);
                blnWarnings = true;
            }
            // get description, if there is one
            agInvObj.Description = agGameProps.GetSetting("OBJECT", "Description", "", true);
            // check for warnings
            if (agInvObj.ErrLevel != 0) {
                // note the problem as a warning
                AddLoadWarning(this, AGIResType.Objects, 0, agInvObj.ErrLevel, []);
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
                switch (Mode) {
                case OpenGameMode.File:
                    // opening existing wag file - check CRC
                    loadInfo.ResType = AGIResType.Logic;
                    loadInfo.ResNum = tmpLog.Number;
                    loadInfo.Type = EventType.etInfo;
                    loadInfo.InfoType = EInfoType.itCheckCRC;
                    OnLoadGameStatus(loadInfo);
                    // cache error level 
                    int tmpErr = tmpLog.ErrLevel;
                    // if there is a source file, need to verify source CRC value
                    if (File.Exists(tmpLog.SourceFile)) {
                        // recalculate the CRC value for this sourcefile by loading the source
                        tmpLog.LoadSource();
                        // check it for TODO items
                        List<TWinAGIEventInfo> TODOs = ExtractTODO(tmpLog.Number, tmpLog.SourceText, tmpLog.ID);
                        if (TODOs.Count > 0) {
                            foreach (TWinAGIEventInfo tmpInfo in TODOs) {
                                OnLoadGameStatus(tmpInfo);
                            }
                        }
                        // check for Decompile warnings
                        List<TWinAGIEventInfo> DecompWarnings = ExtractDecompWarn(tmpLog.Number, tmpLog.SourceText, tmpLog.ID);
                        if (DecompWarnings.Count > 0) {
                            foreach (TWinAGIEventInfo tmpInfo in DecompWarnings) {
                                OnLoadGameStatus(tmpInfo);
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
                    // source error if applicable
                    if (tmpErr != tmpLog.ErrLevel && tmpLog.ErrLevel < 0) {
                        AddLoadWarning(this, AGIResType.Logic, tmpLog.Number, tmpLog.ErrLevel, tmpLog.ErrData);
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
                    loadInfo.Type = EventType.etInfo;
                    loadInfo.InfoType = EInfoType.itDecompiling;
                    OnLoadGameStatus(loadInfo);
                    // force decompile
                    tmpLog.LoadSource(true);
                    if (tmpLog.ErrLevel < 0) {
                        AddLoadWarning(this, AGIResType.Logic, tmpLog.Number, tmpLog.ErrLevel, tmpLog.ErrData);
                        blnWarnings = true;
                    }
                    // unload the logic after decompile is done
                    tmpLog.Unload();
                    // if a new ID found, use it
                    if (DecodeGameID.Length != 0) {
                        agGameID = DecodeGameID;
                        DecodeGameID = "";
                        File.Move(agGameFile, agGameDir + agGameID + ".wag", true);
                        agGameFile = agGameDir + agGameID + ".wag";
                        agGameProps.Filename = agGameFile;
                        WriteGameSetting("General", "GameID", agGameID);
                    }
                    break;
                }
            }
            // everything loaded OK; tidy things up before exiting
            loadInfo.Type = EventType.etInfo;
            loadInfo.InfoType = EInfoType.itFinalizing;
            OnLoadGameStatus(loadInfo);
            // force id reset
            blnSetIDs = false;
            // if extracting a game
            if (Mode == OpenGameMode.Directory) {
                // write create date
                WriteGameSetting("General", "LastEdit", agLastEdit.ToString());
            }
            // if warnings or errors
            if (blnWarnings) {
                retval.ID = 636.ToString();
                retval.Type = EventType.etWarning;
                retval.Text = "WARNINGS";
            }
            return retval;
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
            agGlobals = new GlobalList(this);
            // clear out game properties
            agGameID = "";
            agIntVersion = "2.917";
            agIsVersion3 = false;
            agGameProps = new SettingsList();
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
            agPlatformType = 0;
            agPlatformFile = "";
            agPlatformOpts = "";
            agDOSExec = "";
            // colors
            for (int i = 0; i < 16; i++) {
                agEGAcolors[i] = DefaultColors[i];
            }
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
                                using (fsCOM = new FileStream(strLoader, FileMode.Open)) {
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
                                            agGameID = Left(strFile, strFile.Length - 4).ToUpper();
                                            break;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) {
                                // ignore if file is readonly
                                // (or any other access error)
                                System.Diagnostics.Debug.Print(ex.Message);
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
                    agGameID = Left(strFile, strFile.IndexOf("DIR"));
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

            // ASSUMES a valid game property file has been loaded
            // loads only these properties:
            //     Palette:
            //         all colors
            //
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
            //         logic sourcefile extension

            // Palette: (make sure AGI defaults set first)
            DefaultColors = new();
            for (int i = 0; i < 16; i++) {
                AGIColors[i] = agGameProps.GetSetting("Palette", "Color" + i.ToString(), DefaultColors[i]);
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
            agPlatformType = (PlatformTypeEnum)agGameProps.GetSetting("General", "PlatformType", 0);
            agPlatformFile = agGameProps.GetSetting("General", "Platform", "");
            agDOSExec = agGameProps.GetSetting("General", "DOSExec", "");
            agPlatformOpts = agGameProps.GetSetting("General", "PlatformOpts", "");
            UseReservedNames = agGameProps.GetSetting("General", "UseResNames", UseReservedNames);
            agUseLE = agGameProps.GetSetting("General", "UseLE", false);
            agSierraSyntax = agGameProps.GetSetting("General", "SierraSyntax", false);
            agSrcFileExt = agGameProps.GetSetting("Decompiler", "SourceFileExt", agDefSrcExt).ToLower().Trim();
        }

        /// <summary>
        /// Cleans up after a compile game cancel or error.
        /// </summary>
        /// <param name="NoEvent"></param>
        internal void CompleteCancel(bool NoEvent = false) {
            if (!NoEvent) {
                TWinAGIEventInfo tmpWarn = new() {
                    Type = EventType.etWarning,
                    ResType = AGIResType.Game,
                    ID = "",
                    Module = "",
                    Text = ""
                };
                _ = OnCompileGameStatus(ECStatus.csCanceled, tmpWarn);
            }
            Compiling = false;
            volManager.Clear();
        }
        #endregion
    }
}