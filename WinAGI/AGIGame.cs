using System;
using System.Linq;
using System.Text;
using System.IO;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Metadata;

namespace WinAGI.Engine {
    //***************************************************
    // AGIGame Class
    //
    // this class exposes a single AGI game, including  
    // all components for reading/editing
    //
    //***************************************************
    public partial class AGIGame { //  : IDisposable {
        private bool disposed = false;

        // local variable(s) to hold property Value(s)
        // for game properties which need to be accessible
        // from all objects in the game system
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
        internal string agIntVersion = "";
        internal bool agIsVersion3 = false;
        internal string agAbout = "";
        internal string agGameVersion = "";
        internal string agGameFile = "";
        //internal bool agGameLoaded = false;
        internal int agMaxVol0 = 0;
        internal int agMaxVolSize = 0;
        internal string agCompileDir = "";
        internal int agPlatformType = 0;
        // 0 = none
        // 1 = DosBox
        // 2 = ScummVM
        // 3 = NAGI
        // 4 = Other
        internal string agPlatformFile = "";
        internal string agPlatformOpts = "";
        internal string agDOSExec = "";
        internal bool agUseLE = false;
        internal Encoding agCodePage = Encoding.GetEncoding(437);
        internal bool agPowerPack = false;
        internal string agDefSrcExt = "lgc";
        internal string agSrcFileExt = "";
        internal TDefine[] agResGameDef = new TDefine[4];
        internal SettingsList agGameProps;

        // game compile variables
        internal bool agCompGame = false;
        internal bool agCancelComp = false;
        internal bool agChangeVersion = false;

        /// <summary>
        /// 
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
                // throw return value as exception
                WinAGIException wex = new(retval.Text) {
                    HResult = WINAGI_ERR + int.Parse(retval.ID),
                };
                wex.Data["retval"] = retval;
                throw wex;
            }
        }

        /// <summary>
        /// 
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

        //public void Dispose() {
        //    Dispose(true);
        //    // This object will be cleaned up by the Dispose method.
        //    // Therefore, you should call GC.SuppressFinalize to
        //    // take this object off the finalization queue
        //    // and prevent finalization code for this object
        //    // from executing a second time.
        //    GC.SuppressFinalize(this);
        //}
        //protected virtual void Dispose(bool disposing) {
        //    // If disposing equals true, dispose all managed
        //    // and unmanaged resources.
        //    if (disposing) {
        //        // Dispose managed resources.

        //    }

        //    // Call the appropriate methods to clean up
        //    // unmanaged resources here.
        //    // If disposing is false,
        //    // only the following code is executed.
        //    compGame = null;

        //    // Note disposing has been done.
        //    disposed = true;
        //}
        //~AGIGame() {
        //    // Do not re-create Dispose clean-up code here.
        //    // Calling Dispose(disposing: false) is optimal in terms of
        //    // readability and maintainability.
        //    Dispose(false);
        //}

        /// <summary>
        /// 
        /// </summary>
        private void InitGame() {
            // set up volume manager
            volManager = new(this);

            //get default max vol sizes
            agMaxVolSize = 1023 * 1024;

            //set max vol0 size
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

            //initialize all game variables
            ClearGameState();
        }

        /// <summary>
        /// 
        /// </summary>
        public Logics Logics { get => agLogs; }

        /// <summary>
        /// 
        /// </summary>
        public Pictures Pictures { get => agPics; }

        /// <summary>
        /// 
        /// </summary>
        public Sounds Sounds { get => agSnds; }

        /// <summary>
        /// 
        /// </summary>
        public Views Views { get => agViews; }

        /// <summary>
        /// 
        /// </summary>
        public WordList WordList { get => agVocabWords; }

        /// <summary>
        /// 
        /// </summary>
        public InventoryList InvObjects { get => agInvObj; }

        /// <summary>
        /// 
        /// </summary>
        public GlobalList GlobalDefines { get => agGlobals; }

        /// <summary>
        /// 
        /// </summary>
        public EGAColors AGIColors { get => agEGAcolors; }

        /// <summary>
        /// 
        /// </summary>
        public void CancelCompile() {
            // can be called by parent program during a compile
            // action to cancel the compile

            //if compiling
            if (agCompGame) {
                //set flag indicating cancellation
                agCancelComp = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string DOSExec {
            get { return agDOSExec; }
            set {
                string newExec = value;
                // no validation required
                agDOSExec = newExec;
                WriteGameSetting("General", "DOSExec", agDOSExec);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string GameAbout {
            get { return agAbout; }
            set {
                //limit to 4096 characters
                if (value.Length > 4096) {
                    agAbout = Left(value, 4096);
                }
                else {
                    agAbout = value;
                }
                WriteGameSetting("General", "About", agAbout);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public string GameAuthor {
            get { return agAuthor; }
            set {
                //limit author to 256 bytes
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
        /// 
        /// </summary>
        public string GameDescription {
            get => agDescription;
            set {
                //comments limited to 4K
                if (value.Length > 4096)
                    agDescription = Left(value, 4096);
                else
                    agDescription = value;

                //write new property
                WriteGameSetting("General", "Description", agDescription);
            }
        }

        /// <summary>
        /// 
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
                if (Directory.Exists(CDir(value))) {
                    //return error
                    WinAGIException wex = new(LoadResString(630).Replace(ARG1, value)) {
                        HResult = WINAGI_ERR + 630
                    };
                    wex.Data[0] = value;
                    throw wex;
                }
                // change the directory
                agGameDir = CDir(value);
                // update gamefile name
                agGameFile = agGameDir + Path.GetFileName(agGameFile);
                // update resdir
                agResDir = agGameDir + agResDirName + @"\";
                // change date of last edit
                agLastEdit = DateTime.Now;
            }
        }
        /// <summary>
        /// 
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
                // errors are ignored
                //change property 
                agGameFile = value;
                //change date of last edit
                agLastEdit = DateTime.Now;
                //write date of last edit
                WriteGameSetting("General", "LastEdit", agLastEdit.ToString());
                //save the game prop file
                agGameProps.Save();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public int PlatformType {
            get => agPlatformType;
            set {
                //only 0 - 4 are valid
                if (value < 0 || value > 4) {
                    agPlatformType = 0;
                }
                else {
                    agPlatformType = value;
                }
                //write new property
                WriteGameSetting("General", "PlatformType", agPlatformType.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Platform {
            get { return agPlatformFile; }
            set {
                // no validation needed
                agPlatformFile = value;
                WriteGameSetting("General", "Platform", agPlatformFile);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string PlatformOpts {
            get { return agPlatformOpts; }
            set {
                //no validation required
                agPlatformOpts = value;
                //write new property
                WriteGameSetting("General", "PlatformOpts", agPlatformOpts);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UseLE {
            get {
                return agUseLE;
                //if a game is not loaded
            }
            set {
                // no validation required
                agUseLE = value;
                // write new property
                WriteGameSetting("General", "UseLE", agUseLE.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Encoding CodePage {
            get { return agCodePage; }
            set {
                // confirm new codepage is supported; ignore if it is not
                switch (value.CodePage) {
                case 437 or 737 or 775 or 850 or 852 or 855 or 857 or 860 or
                     861 or 862 or 863 or 865 or 866 or 869 or 858:
                    agCodePage = value;
                    // write the new property
                    WriteGameSetting("General", "CodePage", agCodePage.CodePage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("CodePage", "Unsupported or invalid CodePage value");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SierraSyntax {
            get { return agSierraSyntax; }
            set {
                agSierraSyntax = value;
                WriteGameSetting("General", "SierraSyntax", agSierraSyntax);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool PowerPack {
            get { return agPowerPack; }
            set {
                agPowerPack = value;
                WriteGameSetting("General", "PowerPack", agPowerPack);
            }
        }

        /// <summary>
        /// 
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

        /// <summary>
        /// 
        /// </summary>
        public TDefine[] ReservedGameDefines {
            get {
                //returns the reserved defines that are game-specific:
                //     gamever, gameabout, gameid, invobj Count
                TDefine[] tmpDefines = agResGameDef;

                // always refrsh the values
                tmpDefines[0].Value = "\"" + GameID + "\"";
                tmpDefines[1].Value = "\"" + agGameVersion + "\"";
                tmpDefines[2].Value = "\"" + GameAbout + "\"";
                tmpDefines[3].Value = agInvObj.Count.ToString();
                return tmpDefines;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CloseGame() {
            // unload and remove all resources
            agLogs.Clear();
            agPics.Clear();
            agSnds.Clear();
            agViews.Clear();
            agInvObj.Unload();
            agInvObj.InGame = false;
            agVocabWords.Unload();
            agVocabWords.InGame = false;
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
        /// Compiles the game into NewGameDir.
        /// If RebuildOnly is true, the VOL files are
        /// rebuilt without recompiling all logics
        /// and WORDS.TOK and OBJECT are not recompiled.
        /// WARNING: if NewGameDir is same as current directory
        /// this WILL overwrite current game files.
        /// </summary>
        /// <param name="RebuildOnly"></param>
        /// <param name="NewGameDir"></param>
        /// <returns></returns>
        public bool CompileGame(bool RebuildOnly, string NewGameDir = "") {
            bool blnReplace, NewIsV3;
            string strFileName = "";
            int tmpMax = 0, i, j;
            TWinAGIEventInfo compInfo = new() {
                Type = EventType.etInfo,
                ID = "",
                Module = "",
                Text = ""
            };

            agCompGame = true;
            agCancelComp = false;
            // if no directory passed assume current dir
            if (NewGameDir.Length == 0) {
                NewGameDir = agGameDir;
            }
            NewGameDir = CDir(NewGameDir);
            if (!Directory.Exists(NewGameDir)) {
                //this isn't a directory
                CompleteCancel(true);
                WinAGIException wex = new(LoadResString(561).Replace(ARG1, NewGameDir)) {
                    HResult = WINAGI_ERR + 561
                };
                throw wex;
            }
            // set flag if game is being compiled in its current directory
            blnReplace = NewGameDir.Equals(agGameDir, StringComparison.OrdinalIgnoreCase);
            // save compile dir so rebuild method can access it
            agCompileDir = NewGameDir;
            // set new game version
            if (agChangeVersion) {
                NewIsV3 = !agIsVersion3;
            }
            else {
                NewIsV3 = agIsVersion3;
            }
            // ensure switch flag is reset
            agChangeVersion = false;
            string strID;
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
                // save/copy words.tok and object files first
                compInfo.Text = "";
                //set status
                Raise_CompileGameEvent(ECStatus.csCompWords, AGIResType.rtWords, 0, compInfo);
                if (agCancelComp) {
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
                            Text = "Error during compilation of WORDS.TOK (" + ex.Message + ")"
                        };
                        Raise_CompileGameEvent(ECStatus.csResError, AGIResType.rtWords, 0, tmpError);
                        // cancel it
                        CompleteCancel();
                        return false;
                    }
                }
                if (!blnReplace) {
                    try {
                        // rename then delete existing file, if it exists
                        if (File.Exists(NewGameDir + "WORDS.TOK")) {
                            // delete the 'old' file if it exists
                            if (File.Exists(NewGameDir + "WORDS.OLD")) {
                                File.Delete(NewGameDir + "WORDS.OLD");
                            }
                            File.Move(NewGameDir + "WORDS.TOK", NewGameDir + "WORDS.OLD");
                        }
                        // then copy the current file to new location
                        File.Copy(agVocabWords.ResFile, NewGameDir + "WORDS.TOK");
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.etError,
                            ID = "",
                            Module = "",
                            Text = "Error while creating WORDS.TOK file (" + ex.Message + ")"
                        };
                        Raise_CompileGameEvent(ECStatus.csResError, AGIResType.rtWords, 0, tmpError);
                        CompleteCancel();
                        return false;
                    }
                }
                // OBJECT file is next
                compInfo.Text = "";
                Raise_CompileGameEvent(ECStatus.csCompObjects, AGIResType.rtObjects, 0, compInfo);
                if (agCancelComp) {
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
                            Text = "Error during compilation of OBJECT (" + ex.Message + ")"
                        };
                        Raise_CompileGameEvent(ECStatus.csResError, AGIResType.rtObjects, 0, tmpError);
                        CompleteCancel();
                        return false;
                    }
                }
                if (!blnReplace) {
                    try {
                        // rename then delete existing file, if it exists
                        if (File.Exists(NewGameDir + "OBJECT")) {
                            // first, delete the 'old' file if it exists
                            if (File.Exists(NewGameDir + "OBJECT.OLD")) {
                                File.Delete(NewGameDir + "OBJECT.OLD");
                            }
                            File.Move(NewGameDir + "OBJECT", NewGameDir + "OBJECT.OLD");
                        }
                        // then copy the current file to new location
                        File.Copy(agInvObj.ResFile, NewGameDir + "OBJECT");
                    }
                    catch (Exception ex) {
                        TWinAGIEventInfo tmpError = new() {
                            Type = EventType.etError,
                            ID = "",
                            Module = "",
                            Text = "Error while creating OBJECT file (" + ex.Message + ")"
                        };
                        Raise_CompileGameEvent(ECStatus.csResError, AGIResType.rtObjects, 0, tmpError);
                        CompleteCancel();
                        return false;
                    }
                }
            }
            // reset game compiler variables
            volManager.Clear();
            volManager.NewDir = NewGameDir;

            try {
                // ensure all temp vol files are removed
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
                volManager.VOLFile = File.Create(NewGameDir + "NEW_VOL.0");
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
                CompileResCol(this, agLogs, AGIResType.rtLogic, RebuildOnly, NewIsV3);
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            // if a resource error (or user canceled) encountered, just exit
            if (agCancelComp) {
                CompleteCancel(true);
                return false;
            }
            // add all picture resources
            try {
                CompileResCol(this, agPics, AGIResType.rtPicture, RebuildOnly, NewIsV3);
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            // if a resource error (or user canceled) encountered, just exit
            if (agCancelComp) {
                CompleteCancel(true);
                return false;
            }
            // add all view resources
            try {
                CompileResCol(this, agViews, AGIResType.rtView, RebuildOnly, NewIsV3);
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            // if a resource error (or user canceled) encountered, just exit
            if (agCancelComp) {
                CompleteCancel(true);
                return false;
            }
            // add all sound resources
            try {
                CompileResCol(this, agSnds, AGIResType.rtSound, RebuildOnly, NewIsV3);
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            // if a resource error (or user canceled) encountered, just exit
            if (agCancelComp) {
                CompleteCancel(true);
                return false;
            }
            // remove any existing old dirfiles
            if (NewIsV3) {
                if (File.Exists(NewGameDir + agGameID + "DIR.OLD")) {
                    File.Delete(NewGameDir + agGameID + "DIR.OLD");
                }
            }
            else {
                if (File.Exists(NewGameDir + "LOGDIR.OLD")) {
                    File.Delete(NewGameDir + "LOGDIR.OLD");
                }
                if (File.Exists(NewGameDir + "PICDIR.OLD")) {
                    File.Delete(NewGameDir + "PICDIR.OLD");
                }
                if (File.Exists(NewGameDir + "VIEWDIR.OLD")) {
                    File.Delete(NewGameDir + "VIEWDIR.OLD");
                }
                if (File.Exists(NewGameDir + "SNDDIR.OLD")) {
                    File.Delete(NewGameDir + "SNDDIR.OLD");
                }
            }
            // rename existing dir files as .OLD
            if (NewIsV3) {
                if (File.Exists(NewGameDir + agGameID + "DIR")) {
                    File.Move(NewGameDir + agGameID + "DIR", NewGameDir + agGameID + "DIR.OLD");
                    File.Delete(NewGameDir + agGameID + "DIR");
                }
            }
            else {
                if (File.Exists(NewGameDir + "LOGDIR")) {
                    File.Move(NewGameDir + "LOGDIR", NewGameDir + "LOGDIR.OLD");
                    File.Delete(NewGameDir + "LOGDIR");
                }
                if (File.Exists(NewGameDir + "PICDIR")) {
                    File.Move(NewGameDir + "PICDIR", NewGameDir + "PICDIR.OLD");
                    File.Delete(NewGameDir + "PICDIR");
                }
                if (File.Exists(NewGameDir + "VIEWDIR")) {
                    File.Move(NewGameDir + "VIEWDIR", NewGameDir + "VIEWDIR.OLD");
                    File.Delete(NewGameDir + "VIEWDIR");
                }
                if (File.Exists(NewGameDir + "SNDDIR")) {
                    File.Move(NewGameDir + "SNDDIR", NewGameDir + "SNDDIR.OLD");
                    File.Delete(NewGameDir + "SNDDIR");
                }
            }
            // now build the new DIR files
            if (NewIsV3) {
                strFileName = NewGameDir + agGameID + "DIR";
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
                        volManager.DIRWriter.Write(volManager.DIRData[0, 3 * i]);
                        volManager.DIRWriter.Write(volManager.DIRData[0, 3 * i + 1]);
                        volManager.DIRWriter.Write(volManager.DIRData[0, 3 * i + 2]);
                    }
                    // next are pictures
                    tmpMax = agPics.Max;
                    for (i = 0; i <= tmpMax; i++) {
                        volManager.DIRWriter.Write(volManager.DIRData[1, 3 * i]);
                        volManager.DIRWriter.Write(volManager.DIRData[1, 3 * i + 1]);
                        volManager.DIRWriter.Write(volManager.DIRData[1, 3 * i + 2]);
                    }
                    // then views
                    tmpMax = agViews.Max;
                    for (i = 0; i <= tmpMax; i++) {
                        volManager.DIRWriter.Write(volManager.DIRData[3, 3 * i]);
                        volManager.DIRWriter.Write(volManager.DIRData[3, 3 * i + 1]);
                        volManager.DIRWriter.Write(volManager.DIRData[3, 3 * i + 2]);
                    }
                    // and finally, sounds
                    tmpMax = agSnds.Max;
                    for (i = 0; i <= tmpMax; i++) {
                        volManager.DIRWriter.Write(volManager.DIRData[2, 3 * i]);
                        volManager.DIRWriter.Write(volManager.DIRData[2, 3 * i + 1]);
                        volManager.DIRWriter.Write(volManager.DIRData[2, 3 * i + 2]);
                    }
                }
            }
            else {
                // make separate dir files
                for (j = 0; j < 4; j++) {
                    switch ((AGIResType)j) {
                    case AGIResType.rtLogic:
                        strFileName = NewGameDir + "LOGDIR";
                        tmpMax = agLogs.Max;
                        break;
                    case AGIResType.rtPicture:
                        strFileName = NewGameDir + "PICDIR";
                        tmpMax = agPics.Max;
                        break;
                    case AGIResType.rtSound:
                        strFileName = NewGameDir + "SNDDIR";
                        tmpMax = agSnds.Max;
                        break;
                    case AGIResType.rtView:
                        strFileName = NewGameDir + "VIEWDIR";
                        tmpMax = agViews.Max;
                        break;
                    }
                    // create the dir file
                    using (volManager.DIRWriter) {
                        for (i = 0; i <= tmpMax; i++) {
                            volManager.DIRWriter.Write(volManager.DIRData[j, 3 * i]);
                            volManager.DIRWriter.Write(volManager.DIRData[j, 3 * i + 1]);
                            volManager.DIRWriter.Write(volManager.DIRData[j, 3 * i + 2]);
                        }
                    }
                }
            }
            // remove any existing old vol files
            for (i = 0; i < 16; i++) {
                if (NewIsV3) {
                    if (File.Exists(NewGameDir + agGameID + "VOL." + i.ToString() + ".OLD")) {
                        File.Delete(NewGameDir + agGameID + "VOL." + i.ToString() + ".OLD");
                    }
                }
                else {
                    if (File.Exists(NewGameDir + "VOL." + i.ToString() + ".OLD")) {
                        File.Delete(NewGameDir + "VOL." + i.ToString() + ".OLD");
                    }
                }
            }
            // rename current vol files
            for (i = 0; i < 16; i++) {
                if (NewIsV3) {
                    if (File.Exists(NewGameDir + agGameID + "VOL." + i.ToString())) {
                        File.Move(NewGameDir + agGameID + "VOL." + i.ToString(), NewGameDir + agGameID + "VOL." + i.ToString() + ".OLD");
                    }
                }
                else {
                    if (File.Exists(NewGameDir + "VOL." + i.ToString())) {
                        File.Move(NewGameDir + "VOL." + i.ToString(), NewGameDir + "VOL." + i.ToString() + ".OLD");
                    }
                }
            }
            // now rename VOL files
            for (i = 0; i < volManager.Count; i++) {
                strFileName = strID + "VOL." + i.ToString();
                File.Move(NewGameDir + "NEW_VOL." + i.ToString(), NewGameDir + strFileName);
            }
            // update status to indicate complete
            compInfo.Text = "";
            Raise_CompileGameEvent(ECStatus.csCompileComplete, 0, 0, compInfo);
            if (blnReplace) {
                // need to update the vol/loc info for all ingame resources;
                // this is done here instead of when the rsources are compiled because
                // if there's an error, or the user cancels, we don't want the directories
                // to point to the wrong place
                foreach (Logic tmpLogic in agLogs.Col.Values) {
                    tmpLogic.Volume = (sbyte)(volManager.DIRData[0, tmpLogic.Number * 3] >> 4);
                    tmpLogic.Loc = ((volManager.DIRData[0, tmpLogic.Number * 3] & 0xF) << 16) + (volManager.DIRData[0, tmpLogic.Number * 3 + 1] << 8) + volManager.DIRData[0, tmpLogic.Number * 3 + 2];
                }
                foreach (Picture tmpPicture in agPics.Col.Values) {
                    tmpPicture.Volume = (sbyte)(volManager.DIRData[1, tmpPicture.Number * 3] >> 4);
                    tmpPicture.Loc = ((volManager.DIRData[1, tmpPicture.Number * 3] & 0xF) << 16) + (volManager.DIRData[1, tmpPicture.Number * 3 + 1] << 8) + volManager.DIRData[1, tmpPicture.Number * 3 + 2];
                }
                foreach (Sound tmpSound in agSnds.Col.Values) {
                    tmpSound.Volume = (sbyte)(volManager.DIRData[2, tmpSound.Number * 3] >> 4);
                    tmpSound.Loc = ((volManager.DIRData[2, tmpSound.Number * 3] & 0xF) << 16) + (volManager.DIRData[2, tmpSound.Number * 3 + 1] << 8) + volManager.DIRData[2, tmpSound.Number * 3 + 2];
                }
                foreach (View tmpView in agViews.Col.Values) {
                    tmpView.Volume = (sbyte)(volManager.DIRData[3, tmpView.Number * 3] >> 4);
                    tmpView.Loc = ((volManager.DIRData[3, tmpView.Number * 3] & 0xF) << 16) + (volManager.DIRData[3, tmpView.Number * 3 + 1] << 8) + volManager.DIRData[3, tmpView.Number * 3 + 2];
                }
            }
            // save the wag file
            agGameProps.Save();
            // reset compiling flag
            agCompGame = false;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public string GameID {
            get {
                return agGameID;
            }
            set {
                // limit gameID to 6 characters for v2 games and 5 characters for v3 games
                string NewID = value;
                // version 3 games limited to 5 characters
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
                        // dir file
                        File.Move(agGameDir + agGameID + "DIR", agGameDir + NewID + "DIR");
                        //and vol files
                        foreach (string strVolFile in Directory.EnumerateFiles(agGameDir, agGameID + "VOL.*")) {
                            //if an archived (OLD) file, skip it
                            if (!strVolFile[^4..].Equals(".OLD", StringComparison.OrdinalIgnoreCase)) {
                                string[] strExtension = strVolFile.Split(".");
                                File.Move(agGameDir + strVolFile, agGameDir + NewID + "VOL." + strExtension[1]);
                            }
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
        /// 
        /// </summary>
        public bool LoadWarnings {
            get => agLoadWarnings;
        }

        /// <summary>
        /// 
        /// </summary>
        public DateTime LastEdit {
            get {
                return agLastEdit;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MaxVol0Size {
            get { return agMaxVol0; }
            set {
                //validate
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
        /// 
        /// </summary>
        public string InterpreterVersion {
            get {
                // if no version yet
                if (agIntVersion.Length == 0) {
                    // use 2.917
                    return "2.917";
                }
                return agIntVersion;
            }
            set {
                // validate new version, adjusting resources if necessary
                if (IntVersions.Contains(value)) {
                    // if new version and old version are not same major version:
                    if (agIntVersion[0] != value[0]) {
                        // set flag to switch version on rebuild
                        agChangeVersion = true;
                        // if new version is V3 (i.e., current version isn't)
                        if (!agIsVersion3) {
                            if (GameID.Length > 5) {
                                // truncate the ID
                                GameID = Left(GameID, 5);
                            }
                        }
                        // use compiler to rebuild new vol and dir files
                        // (it is up to calling program to deal with dirty and invalid resources)
                        try {
                            CompileGame(true);
                        }
                        catch {
                            agCancelComp = false;
                            throw;
                        }
                        if (agCancelComp) {
                            agCancelComp = false;
                            return;
                        }
                    }
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
                    // change date of last edit
                    agLastEdit = DateTime.Now;
                    //write new version to file
                    WriteGameSetting("General", "Interpreter", value);
                    // and save it
                    agGameProps.Save();
                    
                    // OK to set new version
                    agIntVersion = value;
                    agIsVersion3 = agIntVersion[0] == '3';
                    CorrectCommands(agIntVersion);
                }
                else {
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
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ResDir {
            get {
                return agResDir;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ResDirName {
            get { return agResDirName; }
            set {
                // validate- cant have  \/:*?<>|
                string tmpName = value.Trim();
                // if  blank use default
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
        /// This forces WinAGI to save the property file, instead of waiting until
        /// a property is updated.
        /// </summary>
        public void SaveProperties() {
            // 
            agGameProps.Save();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="NewID"></param>
        /// <param name="NewVersion"></param>
        /// <param name="NewGameDir"></param>
        /// <param name="NewResDir"></param>
        /// <param name="TemplateDir"></param>
        /// <param name="NewExt"></param>
        /// <returns></returns>
        private TWinAGIEventInfo NewGame(string NewID, string NewVersion, string NewGameDir, string NewResDir, string TemplateDir = "", string NewExt = "") {
            // creates a new game in NewGameDir
            // if a template directory is passed,
            // use the resources from that template directory
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
                WinAGIException wex = new(LoadResString(630).Replace(ARG1, NewGameDir)) {
                    HResult = WINAGI_ERR + 630
                };
                throw wex;
            }
            // if a game already exists
            if (Directory.GetFiles(NewGameDir, "*.wag").Length != 0) {
                WinAGIException wex = new(LoadResString(687)) {
                    HResult = WINAGI_ERR + 687
                };
                throw wex;
            }
            if (IsValidGameDir(CDir(NewGameDir))) {
                // game files already exist;
                WinAGIException wex = new(LoadResString(687)) {
                    HResult = WINAGI_ERR + 687
                };
                throw wex;
            }
            ClearGameState();
            agGameDir = CDir(NewGameDir);
            // ensure resdir is valid
            if (NewResDir.Length == 0) {
                NewResDir = agDefResDir;
            }
            // if using template
            if (TemplateDir.Length != 0) {
                // template should include dir files, vol files, words.tok and object;
                // also globals list and layout, and source directory with logic source files
                TemplateDir = CDir(TemplateDir);
                // should be exactly one wag file
                if (Directory.GetFiles(TemplateDir, "*.wag").Length != 1) {
                    //raise error
                    WinAGIException wex = new(LoadResString(630)) {
                        HResult = WINAGI_ERR + 630
                    };
                    throw wex;
                }
                // get file name (it's first[and only] element)
                strGameWAG = Directory.GetFiles(TemplateDir, "*.wag")[0];
                // template should have at least one subdirectory
                if (Directory.GetDirectories(TemplateDir).Length == 0) {
                    //no resource directory; we will build a default
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
                    oldExt = oldProps.GetSetting("General", "SourceFileExt", DefaultSrcExt);
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
                    // TODO: error number??? might need to rewrite this errmsg
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
                    DirectoryInfo resDir = new(agGameDir + strTmplResDir);
                    resDir.MoveTo(agGameDir + NewResDir);
                    // force all logics to update to new resdir
                    foreach (Logic tmpLog in Logics) {
                        tmpLog.ID = tmpLog.ID;
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
                //rename the wag file
                File.Move(agGameDir + strGameWAG, agGameFile, true);
                // change propfile to correct name
                agGameProps.Lines[0] = agGameFile;
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
                // validate new version
                if (IntVersions.Contains(NewVersion)) {
                    // ok; set version
                    agIntVersion = NewVersion;
                    agIsVersion3 = (Val(NewVersion) > 3);
                }
                else {
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
                // set game id (limit to 6 characters for v2, and 5 characters for v3
                // (don't use the GameID property; gameloaded flag is not set yet
                // so using GameID property will cause error)
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
                agInvObj = new InventoryList(this, true)
                {
                    { "?", 0 }
                };
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
                agLogs.Add(0);
                agLogs[0].Save();
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
                        ResType = AGIResType.rtGame,
                        ResNum = 0,
                        ID = "OW01",
                        Module = "",
                        Text = "Can't create " + agResDir,
                        Line = "--"
                    };
                    Raise_LoadGameEvent(warnInfo);
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
        /// If fails, error is thrown.
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
            agGameDir = CDir(NewGameDir);
            warnInfo.Type = EventType.etInfo;
            warnInfo.InfoType = EInfoType.itValidating;
            warnInfo.Text = "";
            Raise_LoadGameEvent(warnInfo);
            // check for valid DIR/VOL files
            // (which gets gameid, and sets version 3 status flag)
            if (!IsValidGameDir(agGameDir)) {
                ClearGameState();
                // directory is not a valid AGI directory
                WinAGIException wex = new(LoadResString(541).Replace(ARG1, CDir(NewGameDir))) {
                    HResult = WINAGI_ERR + 541,
                };
                wex.Data["baddir"] = CDir(NewGameDir);
                throw wex;
            }
            // create a new wag file name
            agGameFile = agGameDir + agGameID + ".wag";
            // rename any existing game file (in case user is re-importing)
            try {
                if (File.Exists(agGameFile)) {
                    if (File.Exists(agGameFile + ".OLD")) {
                        File.Delete(agGameFile + ".OLD");
                    }
                    File.Move(agGameFile, agGameFile + ".OLD");
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
        /// If fails, error is thrown.
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

            // TODO: add some status updates for openbywag
            if (!File.Exists(GameWAG)) {
                // invalid wag
                retval.Type = EventType.etError;
                // is the file missing?, or the directory?
                if (Directory.Exists(JustPath(GameWAG, true))) {
                    // it's a missing file - return wagfile as error string
                    WinAGIException wex = new(LoadResString(655).Replace(ARG1, GameWAG)) {
                        HResult = WINAGI_ERR + 655,
                    };
                    wex.Data["badwag"] = GameWAG;
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
                    // any v1.2.x or 2.x is ok, but update
                    // TODO: after testing, re-enable version updating
                    //WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION);
                }
                else {
                    // clear game variables
                    ClearGameState();
                    // invalid wag
                    WinAGIException wex = new(LoadResString(665).Replace(ARG1, GameWAG)) {
                        HResult = WINAGI_ERR + 665,
                    };
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
                // TODO: gameid hasn't been validated! what if it's bad?
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
        /// is passed back. If fails, error is thrown.
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
                // informs user we are checking property file to set game parameters
                loadInfo.InfoType = EInfoType.itPropertyFile;
                Raise_LoadGameEvent(loadInfo);
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
                    loadInfo.ResType = AGIResType.rtGame;
                    loadInfo.Type = EventType.etWarning;
                    loadInfo.ID = "OW01";
                    loadInfo.Text = "Can't create " + agResDir;
                    Raise_LoadGameEvent(loadInfo);
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
            // clear other game properties
            agAuthor = "";
            agDescription = "";
            agGameVersion = "";
            agAbout = "";
            agPlatformType = 0;
            agPlatformFile = "";
            agPlatformOpts = "";
            agDOSExec = "";
            try {
                // get rest of game properties
                GetGameProperties();
            }
            catch (Exception e) {
                // note the problem as a warning
                loadInfo.ResType = AGIResType.rtGame;
                loadInfo.Type = EventType.etWarning;
                loadInfo.ID = "OW02";
                loadInfo.Text = $"Error while loading WAG file; some properties not loaded. (Error {e.HResult}: {e.Message})";
                Raise_LoadGameEvent(loadInfo);
                // set warning
                blnWarnings = true;
            }
            // load vocabulary word list
            loadInfo.Type = EventType.etInfo;
            loadInfo.InfoType = EInfoType.itResources;
            loadInfo.ResType = AGIResType.rtWords;
            Raise_LoadGameEvent(loadInfo);
            try {
                agVocabWords = new WordList(this);
                agVocabWords.Load(agGameDir + "WORDS.TOK");
            }
            catch (Exception e) {
                // if there was an error,
                // note the problem as a warning
                loadInfo.ResType = AGIResType.rtWords;
                loadInfo.Type = EventType.etWarning;
                loadInfo.ID = "RW01";
                loadInfo.Text = $"An error occurred while loading WORDS.TOK (Error {e.HResult}: {e.Message}";
                loadInfo.Module = "WORDS.TOK";
                Raise_LoadGameEvent(loadInfo);
                // set warning flag
                blnWarnings = true;
            }
            if (agVocabWords.ErrLevel != 0) {
                // note the problem as a warning
                AddLoadWarning(AGIResType.rtWords, 0, agVocabWords.ErrLevel, []);
                blnWarnings = true;
            }
            // get description, if there is one
            agVocabWords.Description = agGameProps.GetSetting("WORDS.TOK", "Description", "", true);
            // load inventory objects list
            loadInfo.Type = EventType.etInfo;
            loadInfo.InfoType = EInfoType.itResources;
            loadInfo.ResType = AGIResType.rtObjects;
            Raise_LoadGameEvent(loadInfo);
            try {
                agInvObj = new InventoryList(this);
                agInvObj.Load();
            }
            catch (Exception e) {
                // if there was an error,
                // note the problem as a warning
                loadInfo.ResType = AGIResType.rtObjects;
                loadInfo.Type = EventType.etWarning;
                loadInfo.ID = "RW03";
                loadInfo.Text = $"An error occurred while loading OBJECT:(Error {e.HResult}: {e.Message}";
                loadInfo.Module = "OBJECT";
                Raise_LoadGameEvent(loadInfo);
                blnWarnings = true;
            }
            // get description, if there is one
            agInvObj.Description = agGameProps.GetSetting("OBJECT", "Description", "", true);
            // check for warnings
            if (agInvObj.ErrLevel != 0) {
                // note the problem as a warning
                AddLoadWarning(AGIResType.rtObjects, 0, agInvObj.ErrLevel, []);
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
                    loadInfo.ResType = AGIResType.rtLogic;
                    loadInfo.ResNum = tmpLog.Number;
                    loadInfo.Type = EventType.etInfo;
                    loadInfo.InfoType = EInfoType.itCheckCRC;
                    Raise_LoadGameEvent(loadInfo);
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
                                Raise_LoadGameEvent(tmpInfo);
                            }
                        }
                        // check for Decompile warnings
                        List<TWinAGIEventInfo> DecompWarnings = ExtractDecompWarn(tmpLog.Number, tmpLog.SourceText, tmpLog.ID);
                        if (DecompWarnings.Count > 0) {
                            foreach (TWinAGIEventInfo tmpInfo in DecompWarnings) {
                                Raise_LoadGameEvent(tmpInfo);
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
                    // TODO: what if resource AND source both have errors????
                    // the resource and the source file have now been checked; display error if applicable
                    if (tmpErr != tmpLog.ErrLevel && tmpLog.ErrLevel < 0) {
                        AddLoadWarning(AGIResType.rtLogic, tmpLog.Number, tmpLog.ErrLevel, tmpLog.ErrData);
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
                    loadInfo.ResType = AGIResType.rtLogic;
                    loadInfo.ResNum = tmpLog.Number;
                    loadInfo.Type = EventType.etInfo;
                    loadInfo.InfoType = EInfoType.itDecompiling;
                    Raise_LoadGameEvent(loadInfo);
                    // force decompile
                    tmpLog.LoadSource(true);
                    if (tmpLog.ErrLevel < 0) {
                        AddLoadWarning(AGIResType.rtSound, tmpLog.Number, tmpLog.ErrLevel, tmpLog.ErrData);
                        blnWarnings = true;
                    }
                    // unload the logic after decompile is done
                    tmpLog.Unload();
                    // if a new ID found, use it
                    if (DecodeGameID.Length != 0) {
                        agGameID = DecodeGameID;
                        DecodeGameID = "";
                        File.Move(agGameFile, agGameDir + agGameID + ".wag", true);
                        //// File.Move is supposed to delete the old file, but it doesn't seem to work that way...
                        //File.Delete(agGameFile);
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
            Raise_LoadGameEvent(loadInfo);
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
        /// Clears basic game variables for a blank, unopened game
        /// </summary>
        internal void ClearGameState() {
            agLogs = new Logics(this);
            agPics = new Pictures(this);
            agSnds = new Sounds(this);
            agViews = new Views(this);
            agInvObj = new InventoryList(this);
            agVocabWords = new WordList(this);
            agGlobals = new GlobalList(this);
            // clear out game properties
            agGameID = "";
            agIntVersion = "";
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
            agAbout = "";
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
            IndentSize = 2;
            // TODO: set default indent?
            // release compGame reference
            compGame = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        internal void WriteGameSetting(string Section, string Key, string Value, string Group = "") {
            agGameProps.WriteSetting(Section, Key, Value.ToString(), Group);
            if (!Key.Equals("lastedit", StringComparison.CurrentCultureIgnoreCase) && !Key.Equals("winagiversion", StringComparison.CurrentCultureIgnoreCase) && !Key.Equals("palette", StringComparison.CurrentCultureIgnoreCase)) {
                agLastEdit = DateTime.Now;
            }
            // always save settings file
            agGameProps.Save();
        }

        internal void WriteGameSetting(string Section, string Key, int Value, string Group = "") {
            WriteGameSetting(Section, Key, Value.ToString(), Group);
        }
        internal void WriteGameSetting(string Section, string Key, bool Value, string Group = "") {
            WriteGameSetting(Section, Key, Value.ToString(), Group);
        }

        /// <summary>
        /// Provides calling programs a way to write property
        /// values to the WAG file.
        /// No validation of section or newval is done, so calling function
        /// needs to be careful.
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Group"></param>
        /// <param name="ForceSave"></param>
        public void WriteProperty(string Section, string Key, string Value, string Group = "", bool ForceSave = false) {
            try {
                WriteGameSetting(Section, Key, Value, Group);
                // if forcing a save
                if (ForceSave) {
                    SaveProperties();
                }
            }
            catch (Exception) {
                // ignore if error?
            }
            return;
        }

        /// <summary>
        /// This function will determine if the strDir is a
        /// valid sierra AGI game directory.
        /// It also sets the gameID, if one is found, and the version3 flag.
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
                            // readonly, and other access errors will throw exeption
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
        /// 
        /// </summary>
        internal void GetGameProperties() {
            //what's loaded BEFORE we get here:
            // General:
            //  GameID
            //  Interpreter
            //  ResDir

            //ASSUMES a valid game property file has been loaded
            //loads only these properties:
            //
            //  Palette:
            //     all colors
            //
            //  General:
            //     codepage
            //     description
            //     author
            //     about
            //     game version
            //     last date
            //     platform, platform program, platform options, dos executable
            //     use res names property
            //     use layout editor
            //     sierra syntax mode
            //     logic sourcefile extension

            // Palette: (make sure AGI defaults set first)
            DefaultColors = new();
            for (int i = 0; i < 16; i++) {
                AGIColors[i] = agGameProps.GetSetting("Palette", "Color" + i.ToString(), DefaultColors[i]);
            }
            agCodePage = Encoding.GetEncoding(agGameProps.GetSetting("General", "CodePage", 437)); // TODO: need to validate codepage
            agDescription = agGameProps.GetSetting("General", "Description", "");
            agAuthor = agGameProps.GetSetting("General", "Author", "");
            agAbout = agGameProps.GetSetting("General", "About", "").Replace("\n", "\\n");
            agGameVersion = agGameProps.GetSetting("General", "GameVersion", "").Replace("\n", "\\n");
            if (!DateTime.TryParse(agGameProps.GetSetting("General", "LastEdit", DateTime.Now.ToString()), out agLastEdit)) {
                // default to now
                agLastEdit = DateTime.Now;
            }
            agPlatformType = agGameProps.GetSetting("General", "PlatformType", 0);
            agPlatformFile = agGameProps.GetSetting("General", "Platform", "");
            agDOSExec = agGameProps.GetSetting("General", "DOSExec", "");
            agPlatformOpts = agGameProps.GetSetting("General", "PlatformOpts", "");
            // use res names property (use current value, if one not found in property file)
            UseReservedNames = agGameProps.GetSetting("General", "UseResNames", UseReservedNames);
            agUseLE = agGameProps.GetSetting("General", "UseLE", false);
            agSierraSyntax = agGameProps.GetSetting("General", "SierraSyntax", false);
            agSrcFileExt = agGameProps.GetSetting("General", "SourceFileExt", agDefSrcExt).ToLower().Trim();
        }

        /// <summary>
        /// Cleans up after a compile game cancel or error.
        /// </summary>
        /// <param name="NoEvent"></param>
        internal void CompleteCancel(bool NoEvent = false) {
            if (!NoEvent) {
                TWinAGIEventInfo tmpWarn = new() {
                    Type = EventType.etWarning,
                    ID = "",
                    Module = "",
                    Text = ""
                };
                Raise_CompileGameEvent(ECStatus.csCanceled, 0, 0, tmpWarn);
            }
            agCompGame = false;
            volManager.Clear();
        }
    }
}