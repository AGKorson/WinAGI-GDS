using System;
using System.Linq;
using System.Text;
using System.IO;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Compiler;

namespace WinAGI.Engine
{
    //***************************************************
    // AGIGame Class
    //
    // this class exposes a single AGI game, including  
    // all components for reading/editing
    //
    //***************************************************
    public partial class AGIGame
    {
        internal SettingsList agGameProps; // = new SettingsList("");

        static readonly string strErrSource = "WinAGI.Engine.AGIGame";

        //game compile variables
        internal bool agCompGame = false;
        internal bool agCancelComp = false;
        internal bool agChangeVersion = false;

        //local variable(s) to hold property Value(s)
        //for game properties which need to be accessible
        //from all objects in the game system
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
        internal string agAuthor = "";
        internal DateTime agLastEdit;
        internal string agDescription = "";
        internal string agIntVersion = "";
        internal bool agIsVersion3 = false;
        internal string agAbout = "";
        internal string agGameVersion = "";
        internal string agGameFile = "";
        internal bool agGameLoaded = false;
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
        internal string agDefSrcExt = "";
        internal string agSrcFileExt = "";

        internal TDefine[] agResGameDef = new TDefine[4];

        public AGIGame(OpenGameMode mode, string gameSource)
        {
            InitGame();
            int retval = -1;

            switch (mode) {
            case OpenGameMode.File:
                retval = OpenGameWAG(gameSource);
                break;
            case OpenGameMode.Directory:
                retval = OpenGameDIR(gameSource);
                break;
            default:
                //bad mode - do nothing?
                break;
            }
            if (retval != 0) {
                //throw exception?
                throw new Exception("could not open");
            }
        }
        public AGIGame(string id, string version, string gamedir, string resdir, string template = "")
        {
            InitGame();
            NewGame(id, version, gamedir, resdir, template);
        }
        private void InitGame()
        {
            //// enable encoding access to codepage 437; this gives us access to the standard MSDOS extended characters
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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
        public Logics Logics
        { get => agLogs; }
        public Pictures Pictures
        { get => agPics; }
        public Sounds Sounds
        { get => agSnds; }
        public Views Views
        { get => agViews; }
        public WordList WordList
        { get => agVocabWords; }
        public InventoryList InvObjects
        { get => agInvObj; }
        public GlobalList GlobalDefines
        { get => agGlobals; }
        public EGAColors AGIColors
        { get => agEGAcolors; }
        public bool GameLoaded
        { get => agGameLoaded; }
        public void CancelCompile()
        {
            // can be called by parent program during a compile
            // action to cancel the compile

            //if compiling
            if (agCompGame) {
                //set flag indicating cancellation
                agCancelComp = true;
            }
        }
        public string DOSExec
        {
            get { return agDOSExec; }
            set
            {
                string newExec = value;

                //no validation required
                agDOSExec = newExec;

                //if a game is loaded,
                if (agGameLoaded) {
                    //write new property
                    WriteGameSetting("General", "DOSExec", agDOSExec);
                }
            }
        }
        public string GameAbout
        {
            get { return agAbout; }
            set
            {
                //limit to 4096 characters
                if (value.Length > 4096)
                    agAbout = Left(value, 4096);
                else
                    agAbout = value;


                //if a game is loaded,
                if (agGameLoaded)
                    //write new property
                    WriteGameSetting("General", "About", agAbout);
            }

        }
        public string GameAuthor
        {
            get { return agAuthor; }
            set
            {
                //limit author to 256 bytes
                if (value.Length > 256)
                    agAuthor = Left(value, 256);
                else
                    agAuthor = value;


                //if game loaded,
                if (agGameLoaded)
                    //write property
                    WriteGameSetting("General", "Author", agAuthor);
            }
        }
        public string GameDescription
        {
            get => agDescription;
            set
            {
                //comments limited to 4K
                if (value.Length > 4096)
                    agDescription = Left(value, 4096);
                else
                    agDescription = value;

                //if a game is loaded
                if (agGameLoaded)
                    //write new property
                    WriteGameSetting("General", "Description", agDescription);
            }
        }
        public string GameDir
        {
            get
            {
                //if a game is loaded,
                if (agGameLoaded) {
                    //use resdir property
                    return agGameDir;
                }
                else {
                    //use current directory
                    return CDir(Directory.GetCurrentDirectory());
                }
            }
            set
            {

                //changing directory is only allowed if a game
                //is loaded, but....
                //
                //changing gamedir directly may cause problems
                //because the game may not be able to find the
                //resource files it is looking for;
                //also, changing the gamedir directly does not
                //update the resource directory
                //
                //It is responsibility of calling function to
                //make sure that all game resources/files/
                //folders are also moved/renamed as needed to
                //support the new directory; exception is the
                //agGameFile property, which gets updated
                //in this property (but not moved or created)

                if (agGameLoaded)
                    //validate gamedir
                    if (Directory.Exists(CDir(value))) {
                        //return error
                        Exception e = new(LoadResString(630).Replace(ARG1, value))
                        {
                            HResult = WINAGI_ERR + 630
                        };
                        throw e;
                    }

                //change the directory
                agGameDir = CDir(value);

                //update gamefile name
                agGameFile = agGameDir + JustFileName(agGameFile);

                //update resdir
                agResDir = agGameDir + agResDirName + @"\";

                //change date of last edit
                agLastEdit = DateTime.Now;
            }
        }
        public string GameFile
        {
            get
            {
                //error if game not loaded
                if (!agGameLoaded) {

                    Exception e = new(LoadResString(693))
                    {
                        HResult = WINAGI_ERR + 693
                    };
                    throw e;
                }
                return agGameFile;
            }
            set
            {
                //error if game not loaded
                if (!agGameLoaded) {

                    Exception e = new(LoadResString(693))
                    {
                        HResult = WINAGI_ERR + 693
                    };
                    throw e;
                }
                //calling function has to make sure NewFile is valid!
                try {
                    File.Move(agGameFile, value);
                }
                finally {
                    // errors are ignored
                }

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
        public int PlatformType
        {
            get => agPlatformType;
            set
            {
                //only 0 - 4 are valid
                if (value < 0 || value > 4)
                    agPlatformType = 0;
                else
                    agPlatformType = value;


                //if a game is loaded,
                if (agGameLoaded)
                    //write new property
                    WriteGameSetting("General", "PlatformType", agPlatformType.ToString());
            }
        }
        public string Platform
        {
            get { return agPlatformFile; }
            set
            {
                // no validation needed
                agPlatformFile = value;

                if (agGameLoaded)
                    WriteGameSetting("General", "Platform", agPlatformFile);
            }
        }
        public string PlatformOpts
        {
            get { return agPlatformOpts; }
            set
            {
                //no validation required
                agPlatformOpts = value;

                //if a game is loaded,
                if (agGameLoaded)
                    //write new property
                    WriteGameSetting("General", "PlatformOpts", agPlatformOpts);
            }
        }
        public bool UseLE
        {
            get
            {
                //if a game is not loaded
                if (!agGameLoaded)
                    //always return false
                    return false;
                else
                    //return stored value
                    return agUseLE;
            }
            set
            {
                // no validation required
                agUseLE = value;
                System.Diagnostics.Debug.Print($"changing UseLE to {value}");
                //if a game is loaded,
                if (agGameLoaded)
                    //write new property
                    WriteGameSetting("General", "UseLE", agUseLE.ToString());
            }
        }

        public Encoding CodePage
        {
            get { return agCodePage; }
            set {
                // confirm new codepage is supported; ignore if it is not
                switch (value.CodePage) {
                case 437:
                case 737:
                case 775:
                case 850:
                case 852:
                case 855:
                case 857:
                case 860:
                case 861:
                case 862:
                case 863:
                case 865:
                case 866:
                case 869:
                case 858:
                    agCodePage = value;
                    // write the new property
                    WriteGameSetting("General", "CodePage", agCodePage.CodePage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("CodePage", "Unsupported or invalid CodePage value");
                }
            }
        }

        public bool SierraSyntax
        {
            get { return agSierraSyntax; }
            set
            {
                agSierraSyntax = value;
                // update property file if game loaded
                if (agGameLoaded) {
                    WriteGameSetting("General", "SierraSyntax", agSierraSyntax);
                }
            }
        }

        public bool PowerPack
        {
            get { return agPowerPack; }
            set
            {
                agPowerPack = value;
                // update property file if game loaded
                if (agGameLoaded) {
                    WriteGameSetting("General", "PowerPack", agPowerPack);
                }
            }
        }

        public string GameVersion
        {
            get => agGameVersion;
            set
            {
                //limit to 256 bytes
                if (value.Length > 256)
                    agGameVersion = Left(value, 256);
                else
                    agGameVersion = value;

                //if game loaded
                if (agGameLoaded)
                    //write new property
                    WriteGameSetting("General", "GameVersion", agGameVersion);
            }
        }
        public TDefine[] ReservedGameDefines
        {
            get
            {
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
        public void CloseGame()
        {
            //if no game is currently loaded
            if (!agGameLoaded)
                return;
            //unload and remove all resources
            agLogs.Clear();
            agPics.Clear();
            agSnds.Clear();
            agViews.Clear();

            if (agInvObj.Loaded)
                agInvObj.Unload();
            agInvObj.InGame = false;

            if (agVocabWords.Loaded)
                agVocabWords.Unload();
            agVocabWords.InGame = false;

            //restore default AGI colors
            ResetDefaultColors();

            //write date of last edit
            WriteGameSetting("General", "LastEdit", agLastEdit.ToString());

            //now save it
            agGameProps.Save();

            // clear all game properties
            ClearGameState();
        }
        public bool CompileGame(bool RebuildOnly, string NewGameDir = "")
        {
            //compiles the game into NewGameDir

            //if RebuildOnly is true, the VOL files are
            //rebuilt without recompiling all logics
            //and WORDS.TOK and OBJECT are not recompiled

            //WARNING: if NewGameDir is same as current directory
            //this WILL overwrite current game files

            //only loaded games can be compiled
            if (!agGameLoaded)
                return false;

            bool blnReplace, NewIsV3;
            string strFileName = "";
            int tmpMax = 0, i, j;
            TWarnInfo compInfo = new();
            
            //set compiling flag
            agCompGame = true;
            //reset cancel flag
            agCancelComp = false;

            compInfo.Type = EWarnType.ecCompWarn;

            //if no directory passed,
            if (NewGameDir.Length == 0)
                NewGameDir = agGameDir;

            //validate new directory
            NewGameDir = CDir(NewGameDir);
            if (!Directory.Exists(NewGameDir)) {
                //this isn't a directory
                CompleteCancel(true);

                Exception e = new(LoadResString(561).Replace(ARG1, NewGameDir))
                {
                    HResult = WINAGI_ERR + 561
                };
                throw e;
            }

            //set flag if game is being compiled in its current directory
            blnReplace = NewGameDir.Equals(agGameDir, StringComparison.OrdinalIgnoreCase);

            //save compile dir so rebuild method can access it
            agCompileDir = NewGameDir;

            //set new game version
            if (agChangeVersion) {
                NewIsV3 = !agIsVersion3;
            }
            else {
                NewIsV3 = agIsVersion3;
            }

            //ensure switch flag is reset
            agChangeVersion = false;
            string strID;
            //if version 3
            if (NewIsV3) {
                //version 3 ids limited to 5 characters
                if (agGameID.Length > 5) {
                    //invalid ID; calling function should know better!
                    CompleteCancel(true);

                    Exception e = new(LoadResString(694))
                    {
                        HResult = WINAGI_ERR + 694
                    };
                    throw e;
                }
                strID = agGameID;
            }
            else {
                strID = "";
            }

            //if not rebuildonly
            if (!RebuildOnly) {
                // save/copy words.tok and object files first
                compInfo.Text = "";

                //set status
                Raise_CompileGameEvent(ECStatus.csCompWords, AGIResType.rtWords, 0, compInfo);
                //check for cancellation
                if (agCancelComp) {
                    CompleteCancel();
                    return false;
                }

                //compile WORDS.TOK if dirty
                if (agVocabWords.IsDirty) {
                    try {
                        agVocabWords.Save();
                    }
                    catch (Exception ex) {
                        // note it
                        TWarnInfo tmpWarn = new()
                        {
                            Type = EWarnType.ecCompWarn,
                            LWType = ELoadWarningSource.lwResource,
                            Text = "Error during compilation of WORDS.TOK (" + ex.Message + ")"
                        };
                        Raise_CompileGameEvent(ECStatus.csResError, AGIResType.rtWords, 0, tmpWarn);
                        // cancel it
                        CompleteCancel();
                        return false;
                    }
                }

                //if compiling to a different directory
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
                        //note error
                        TWarnInfo tmpWarn = new()
                        {
                            Type = EWarnType.ecCompWarn,
                            LWType = ELoadWarningSource.lwResource,
                            Text = "Error while creating WORDS.TOK file (" + ex.Message + ")"
                        };
                        Raise_CompileGameEvent(ECStatus.csResError, AGIResType.rtObjects, 0, tmpWarn);
                        //cancel it
                        CompleteCancel();
                        return false;
                    }
                }
                // OBJECT file is next
                //refresh status
                compInfo.Text = "";
                Raise_CompileGameEvent(ECStatus.csCompObjects, AGIResType.rtObjects, 0, compInfo);
                //check for cancellation
                if (agCancelComp) {
                    CompleteCancel();
                    return false;
                }

                //compile OBJECT file if dirty
                if (agInvObj.IsDirty) {
                    try {
                        agInvObj.Save();
                    }
                    catch (Exception ex) {
                        // note it
                        TWarnInfo tmpWarn = new()
                        {
                            Type = EWarnType.ecCompWarn,
                            LWType = ELoadWarningSource.lwResource,
                            Text = "Error during compilation of OBJECT (" + ex.Message + ")"
                        };
                        Raise_CompileGameEvent(ECStatus.csResError, AGIResType.rtObjects, 0, tmpWarn);
                        // cancel it
                        CompleteCancel();
                        return false;
                    }
                }
                //if compiling to a different directory
                if (!blnReplace) {
                    try {
                        // rename then delete existing file, if it exists
                        if (File.Exists(NewGameDir + "OBJECT")) {
                            //first, delete the 'old' file if it exists
                            if (File.Exists(NewGameDir + "OBJECT.OLD")) {
                                File.Delete(NewGameDir + "OBJECT.OLD");
                            }
                            File.Move(NewGameDir + "OBJECT", NewGameDir + "OBJECT.OLD");
                        }
                        // then copy the current file to new location
                        File.Copy(agInvObj.ResFile, NewGameDir + "OBJECT");
                    }
                    catch (Exception ex) {
                        // note error
                        TWarnInfo tmpWarn = new()
                        {
                            Type = EWarnType.ecCompWarn,
                            LWType = ELoadWarningSource.lwResource,
                            Text = "Error while creating OBJECT file (" + ex.Message + ")"
                        };
                        Raise_CompileGameEvent(ECStatus.csResError, AGIResType.rtObjects, 0, tmpWarn);
                        // cancel it
                        CompleteCancel();
                        return false;
                    }
                }
            }

            //reset game compiler variables
            lngCurrentVol = 0;
            lngCurrentLoc = 0;
            strNewDir = NewGameDir;

            //new strategy with dir files; use arrays during
            //compiling process; then build the dir files at
            //the end; initialize the array
            for (i = 0; i < 4; i++) {
                for (j = 0; i < 768; i++) {
                    bytDIR[i, j] = 255;
                }
            }

            try {
                //ensure all temp vol files are removed
                for (int v = 0; v < 15; v++) {
                    if (File.Exists(NewGameDir + "NEW_VOL." + v.ToString())) {
                        File.Delete(NewGameDir + "NEW_VOL." + v.ToString());
                    }
                }
            }
            catch (Exception) {
                //ignore errors
            }

            try {
                //open first new vol file
                fsVOL = File.Create(NewGameDir + "NEW_VOL.0");
                bwVOL = new BinaryWriter(fsVOL);
            }
            catch (Exception) {
                CompleteCancel(true);
                Exception e = new(LoadResString(503).Replace(ARG1, NewGameDir + "NEW_VOL.0"))
                {
                    HResult = WINAGI_ERR + 503
                };
                throw e;
            }

            //add all logic resources
            try {
                CompileResCol(agLogs, AGIResType.rtLogic, RebuildOnly, NewIsV3);
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            //check for cancellation
            //if a resource error (or user canceled) encountered, just exit
            if (agCancelComp) {
                CompleteCancel(true);
                return false;
            }

            //add picture resources
            try {
                CompileResCol(agPics, AGIResType.rtPicture, RebuildOnly, NewIsV3);
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            //check for cancellation
            //if a resource error (or user canceled) encountered, just exit
            if (agCancelComp) {
                CompleteCancel(true);
                return false;
            }

            //add view resources
            try {
                CompileResCol(agViews, AGIResType.rtView, RebuildOnly, NewIsV3);
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            //check for cancellation
            //if a resource error (or user canceled) encountered, just exit
            if (agCancelComp) {
                CompleteCancel(true);
                return false;
            }

            //add sound resources
            try {
                CompileResCol(agSnds, AGIResType.rtSound, RebuildOnly, NewIsV3);
            }
            catch {
                CompleteCancel(true);
                throw;
            }
            //check for cancellation
            //if a resource error (or user canceled) encountered, just exit
            if (agCancelComp) {
                CompleteCancel(true);
                return false;
            }

            //remove any existing old dirfiles
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

            //rename existing dir files as .OLD
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

            //now build the new DIR files
            if (NewIsV3) {
                //one dir file
                strFileName = NewGameDir + agGameID + "DIR";
                fsDIR = File.Create(strFileName);
                bwDIR = new BinaryWriter(fsDIR);
                //add offsets - logdir offset is always 8
                bwDIR.Write(Convert.ToInt16(8));
                //pic offset is 8 + 3*logmax
                tmpMax = agLogs.Max + 1;
                if (tmpMax == 0) {
                    // always put at least one; even if it's all FFs
                    tmpMax = 1;
                }
                bwDIR.Write((short)(8 + 3 * tmpMax));
                i = 8 + 3 * tmpMax;
                //view offset is pic offset + 3*picmax
                tmpMax = agPics.Max + 1;
                if (tmpMax == 0) {
                    tmpMax = 1;
                }
                bwDIR.Write((short)(i + 3 * tmpMax));
                i += 3 * tmpMax;
                //sound is view offset + 3*viewmax
                tmpMax = agViews.Max + 1;
                if (tmpMax == 0) {
                    tmpMax = 1;
                }
                bwDIR.Write((short)(i + 3 * tmpMax));

                //now add all the dir entries
                //NOTE: we can't use a for-next loop
                //because sound and view dirs are swapped in v3 directory

                //logics first
                tmpMax = agLogs.Max;
                for (i = 0; i <= tmpMax; i++) {
                    bwDIR.Write(bytDIR[0, 3 * i]);
                    bwDIR.Write(bytDIR[0, 3 * i + 1]);
                    bwDIR.Write(bytDIR[0, 3 * i + 2]);
                }
                //next are pictures
                tmpMax = agPics.Max;
                for (i = 0; i <= tmpMax; i++) {
                    bwDIR.Write(bytDIR[1, 3 * i]);
                    bwDIR.Write(bytDIR[1, 3 * i + 1]);
                    bwDIR.Write(bytDIR[1, 3 * i + 2]);
                }
                //then views
                tmpMax = agViews.Max;
                for (i = 0; i <= tmpMax; i++) {
                    bwDIR.Write(bytDIR[3, 3 * i]);
                    bwDIR.Write(bytDIR[3, 3 * i + 1]);
                    bwDIR.Write(bytDIR[3, 3 * i + 2]);
                }
                //and finally, sounds
                tmpMax = agSnds.Max;
                for (i = 0; i <= tmpMax; i++) {
                    bwDIR.Write(bytDIR[2, 3 * i]);
                    bwDIR.Write(bytDIR[2, 3 * i + 1]);
                    bwDIR.Write(bytDIR[2, 3 * i + 2]);
                }
                //done! close the stream and file
                fsDIR.Dispose();
                bwDIR.Dispose();
            }
            else {
                //make separate dir files
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
                    //create the dir file
                    fsDIR = File.Create(strFileName);
                    bwDIR = new BinaryWriter(fsDIR);
                    for (i = 0; i <= tmpMax; i++) {
                        bwDIR.Write(bytDIR[j, 3 * i]);
                        bwDIR.Write(bytDIR[j, 3 * i + 1]);
                        bwDIR.Write(bytDIR[j, 3 * i + 2]);
                    }
                    fsDIR.Dispose();
                    bwDIR.Dispose();
                }
            }

            //remove any existing old vol files
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

            //rename current vol files
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
            for (i = 0; i <= lngCurrentVol; i++) {
                strFileName = strID + "VOL." + i.ToString();
                File.Move(NewGameDir + "NEW_VOL." + i.ToString(), NewGameDir + strFileName);
            }
            //update status to indicate complete
            compInfo.Text = "";
            Raise_CompileGameEvent(ECStatus.csCompileComplete, 0, 0, compInfo);
            //if replacing (meaning we are compiling to the current game directory)
            if (blnReplace) {
                //then we need to update the vol/loc info for all ingame resources;
                //this is done here instead of when the logics are compiled because
                //if there's an error, or the user cancels, we don't want the resources
                //to point to the wrong place
                foreach (Logic tmpLogic in agLogs.Col.Values) {
                    tmpLogic.Volume = (sbyte)(bytDIR[0, tmpLogic.Number * 3] >> 4);
                    tmpLogic.Loc = ((bytDIR[0, tmpLogic.Number * 3] & 0xF) << 16) + (bytDIR[0, tmpLogic.Number * 3 + 1] << 8) + bytDIR[0, tmpLogic.Number * 3 + 2];
                }
                foreach (Picture tmpPicture in agPics.Col.Values) {
                    tmpPicture.Volume = (sbyte)(bytDIR[1, tmpPicture.Number * 3] >> 4);
                    tmpPicture.Loc = ((bytDIR[1, tmpPicture.Number * 3] & 0xF) << 16) + (bytDIR[1, tmpPicture.Number * 3 + 1] << 8) + bytDIR[1, tmpPicture.Number * 3 + 2];
                }
                foreach (Sound tmpSound in agSnds.Col.Values) {
                    tmpSound.Volume = (sbyte)(bytDIR[2, tmpSound.Number * 3] >> 4);
                    tmpSound.Loc = ((bytDIR[2, tmpSound.Number * 3] & 0xF) << 16) + (bytDIR[2, tmpSound.Number * 3 + 1] << 8) + bytDIR[2, tmpSound.Number * 3 + 2];
                }
                foreach (View tmpView in agViews.Col.Values) {
                    tmpView.Volume = (sbyte)(bytDIR[3, tmpView.Number * 3] >> 4);
                    tmpView.Loc = ((bytDIR[3, tmpView.Number * 3] & 0xF) << 16) + (bytDIR[3, tmpView.Number * 3 + 1] << 8) + bytDIR[3, tmpView.Number * 3 + 2];
                }
            }

            //save the wag file
            agGameProps.Save();

            //reset compiling flag
            agCompGame = false;
            return false;
        }

        public string GameID
        {
            get
            {
                //id is undefined if a game is not loaded
                if (!agGameLoaded) {

                    Exception e = new(LoadResString(677))
                    {
                        HResult = WINAGI_ERR + 677
                    };
                    throw e;
                }
                return agGameID;
            }
            set
            {
                //limit gameID to 6 characters for v2 games and 5 characters for v3 games

                string NewID = value;

                //id is undefined if a game is not loaded
                if (!agGameLoaded) {

                    Exception e = new(LoadResString(677))
                    {
                        HResult = WINAGI_ERR + 677
                    };
                    throw e;
                }
                //version 3 games limited to 5 characters
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

                //if no change
                if (agGameID == NewID) {
                    return;
                }

                //if version 3
                if (agIsVersion3) {
                    //TODO: need error trap for all file ops
                    try { 
                        //need to rename the dir file
                        File.Move(agGameDir + agGameID + "DIR", agGameDir + NewID + "DIR");
                        //delete old dirfile
                        File.Delete(agGameDir + agGameID + "DIR");

                        //and vol files
                        foreach (string strVolFile in Directory.EnumerateFiles(agGameDir, agGameID + "VOL.*")) {
                            //if an archived (OLD) file, skip it
                            if (Right(strVolFile, 4).ToUpper() != ".OLD") {
                                //get extension
                                string[] strExtension = strVolFile.Split(".");
                                //rename
                                File.Move(agGameDir + strVolFile, agGameDir + NewID + "VOL." + strExtension[1]);
                                //TODO: delete the old one
                                File.Delete(agGameDir + strVolFile);
                            }
                        }
                    }
                    catch (Exception e) {
                        Exception e1 = new(LoadResString(530).Replace(ARG1,e.HResult.ToString()))
                        {
                            HResult = WINAGI_ERR + 530
                        };
                        throw e1;
                    }
                }
                //set new id
                agGameID = NewID;

                //write new property
                WriteGameSetting("General", "GameID", NewID);
            }
        }
        public DateTime LastEdit
        {
            get
            {
                //if game loaded,
                if (agGameLoaded) {
                    return agLastEdit;
                }
                else {
                    return DateTime.Now;
                }
            }
        }
        public int MaxVol0Size
        {
            get { return agMaxVol0; }
            set
            {
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
        public string InterpreterVersion
        {
            get
            {
                // if a game is loaded
                if (agGameLoaded) {
                    return agIntVersion;
                }
                // if no version yet
                if (agIntVersion.Length == 0) {
                    // use 2.917
                    return "2.917";
                }
                //otherwise return set value
                return agIntVersion;
            }
            set
            {
                //attempts to set version to new Value

                //validate new version
                if (IntVersions.Contains(value)) {
                    //if in a game, need to adjust resources if necessary
                    if (agGameLoaded) {
                        //if new version and old version are not same major version:
                        if (agIntVersion[0] != value[0]) {
                            //set flag to switch version on rebuild
                            agChangeVersion = true;

                            //if new version is V3 (i.e., current version isn't)
                            if (!agIsVersion3) {
                                if (GameID.Length > 5) {
                                    //truncate the ID
                                    GameID = Left(GameID, 5);
                                }
                            }
                            //use compiler to rebuild new vol and dir files
                            //(it is up to calling program to deal with dirty and invalid resources)
                            try {
                                CompileGame(true);
                            }
                            catch {

                                //reset cancel flag
                                agCancelComp = false;

                                // pass along the exception
                                throw;
                            }

                            //check for user cancellation
                            if (agCancelComp) {
                                //reset cancel flag
                                agCancelComp = false;
                                // and quit
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
                        //change date of last edit
                        agLastEdit = DateTime.Now;

                        //write new version to file
                        WriteGameSetting("General", "Interpreter", value);

                        //and save it
                        agGameProps.Save();
                    }

                    //OK to set new version
                    agIntVersion = value;

                    //set v3 flag
                    agIsVersion3 = agIntVersion[0] == '3';

                    //adjust commands
                    CorrectCommands(agIntVersion);
                }
                else {
                    // if not numeric
                    if (!IsNumeric(value)) {
                        Exception e = new(LoadResString(597))
                        {
                            HResult = WINAGI_ERR + 597
                        };
                        throw e;
                    }
                    else if (Double.Parse(value) < 2 || Double.Parse(value) > 3) {
                        //not a version 2 or 3 game

                        Exception e = new(LoadResString(597))
                        {
                            HResult = WINAGI_ERR + 597
                        };
                        throw e;
                    }
                    else {
                        //unsupported version 2 or 3 game

                        Exception e = new(LoadResString(543))
                        {
                            HResult = WINAGI_ERR + 543
                        };
                        throw e;
                    }
                }
            }
        }
        public string ResDir
        {
            get
            {
                // if a game is loaded
                if (agGameLoaded) {
                    return agResDir;
                }
                //otherwise use current directory
                return CDir(System.IO.Directory.GetCurrentDirectory());
            }
        }
        public string ResDirName
        {
            get { return agResDirName; }
            set
            {
                //validate- cant have  \/:*?<>|
                // (VB didn't allow spaces, but they are perfectly fine in directory names)

                string tmpName = value.Trim();

                // ignore blank
                if (tmpName.Length == 0)
                    return;

                if (@"\/:*?<>|".Any(tmpName.Contains)) {
                    throw new ArgumentOutOfRangeException("Invalid property Value");
                }

                //save new resdir name
                agResDirName = tmpName;
                //update the actual resdir
                agResDir = agGameDir + agResDirName + @"\";

                //save resdirname
                WriteGameSetting("General", "ResDir", agResDirName);

                //change date of last edit
                agLastEdit = DateTime.Now;
            }
        }
        public void SaveProperties()
        {
            //this forces WinAGI to save the property file, rather than waiting until
            //the game is unloaded

            if (agGameLoaded)
                agGameProps.Save();
        }
        public void NewGame(string NewID, string NewVersion, string NewGameDir, string NewResDir, string TemplateDir = "", string NewExt = "")
        {
            //creates a new game in NewGameDir
            //if a template directory is passed,
            //use the resources from that template directory

            string strGameWAG, strTmplResDir, strTempDir, oldExt;
            int i, lngDirCount;
            bool blnWarnings = false;
            SettingsList stlGlobals;

            //if a game is already open,
            if (agGameLoaded) {
                //can't open a game if one is already open
                Exception e = new(LoadResString(501))
                {
                    HResult = WINAGI_ERR + 501
                };
                throw e;
            }
            //if not a valid directory
            if (!Directory.Exists(NewGameDir)) {
                //raise error
                Exception e = new(LoadResString(630).Replace(ARG1, NewGameDir))
                {
                    HResult = WINAGI_ERR + 630
                };
                throw e;
            }
            //if a game already exists
            if (Directory.GetFiles(NewGameDir, "*.wag").Length != 0) {
                //wag file already exists;
                Exception e = new(LoadResString(687))
                {
                    HResult = WINAGI_ERR + 687
                };
                throw e;
            }
            if (IsValidGameDir(CDir(NewGameDir))) {
                //game files already exist;
                Exception e = new(LoadResString(687))
                {
                    HResult = WINAGI_ERR + 687
                };
                throw e;
            }

            // clear game properties
            ClearGameState();
            //set game directory
            agGameDir = CDir(NewGameDir);
            //ensure resdir is valid
            if (NewResDir.Length == 0) {
                //if blank use default
                NewResDir = agDefResDir;
            }
            //if using template
            if (TemplateDir.Length != 0) {
                //template should include dir files, vol files, words.tok and object;
                // also globals list and layout, and source directory with logic source files
                TemplateDir = CDir(TemplateDir);
                // should be exactly one wag file
                if (Directory.GetFiles(TemplateDir, "*.wag").Length != 1) {
                    //raise error
                    Exception e = new(LoadResString(630))
                    {
                        HResult = WINAGI_ERR + 630
                    };
                    throw e;
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
                    SettingsList oldProps = new SettingsList(agGameFile);
                    oldProps.Open(false);
                    oldExt = oldProps.GetSetting("General", "SourceFileExt", DefaultSrcExt);
                    oldProps = null;
                }
                catch (Exception) {
                    oldExt = DefaultSrcExt;
                }

                //copy all files from the templatedir into gamedir
                try {
                    if (!DirectoryCopy(TemplateDir, agGameDir, true)) {
                        //TODO: error number??? might need to rewrite this errmsg
                        Exception e = new(LoadResString(683).Replace(ARG1, "unknown error"))
                        {
                            HResult = WINAGI_ERR + 683
                        };
                        throw e;
                    }
                }
                catch (Exception e) {
                    Exception e1 = new(LoadResString(683).Replace(ARG1, e.Message))
                    {
                        HResult = WINAGI_ERR + 683
                    };
                    throw e1;
                }

                if (!oldExt.Equals(NewExt, StringComparison.OrdinalIgnoreCase)) {
                    //update source extension BEFORE opening wag file
                    SettingsList newProps = new(agGameDir + strGameWAG);
                    newProps.WriteSetting("General", "SourceFileExt", NewExt);
                    newProps.Save();
                    newProps = null;
                }

                // open the game in the newly created directory
                try {
                    //open game with template id
                    OpenGameWAG(agGameDir + strGameWAG);
                }
                catch (Exception e) {
                    Exception eR = new(LoadResString(684).Replace(ARG1, e.Message))
                    {
                        HResult = WINAGI_ERR + 684
                    };
                    throw eR;
                }
                //then change the resource directory property
                agResDirName = NewResDir;
                //update the actual resdir
                agResDir = agGameDir + agResDirName + @"\";

                //we need to rename the resdir
                //(have to do this AFTER load, because loading will keep the current
                //resdir that's in the WAG file)
                if (!NewResDir.Equals(strTmplResDir, StringComparison.OrdinalIgnoreCase)) {
                    //we need to change it
                    DirectoryInfo resDir = new(agGameDir + strTmplResDir);
                    resDir.MoveTo(agGameDir + NewResDir);
                    // then force all logics to update to new resdir
                    foreach (Logic tmpLog in Logics) {
                        tmpLog.ID = tmpLog.ID;
                    }
                }
                //change gameid
                try {
                    GameID = NewID;
                }
                catch (Exception e) {
                    Exception eR = new(LoadResString(685).Replace(ARG1, e.Message))
                    {
                        HResult = WINAGI_ERR + 685
                    };
                    throw eR;
                }
                //change game propfile name
                agGameFile = agGameDir + NewID + ".wag";
                //rename the wag file
                File.Move(agGameDir + strGameWAG, agGameFile, true);
                //change propfile to correct name
                agGameProps.Lines[0] = agGameFile;
                //rename wal file, if present
                if (File.Exists(agGameDir + FileNameNoExt(strGameWAG) + ".wal")) {
                    File.Move(agGameDir + FileNameNoExt(strGameWAG) + ".wal", agGameDir + NewID + ".wal");
                }
                //update global file header
                stlGlobals = new SettingsList(agGameDir + "globals.txt");
                stlGlobals.Open(false);
                if (stlGlobals.Lines.Count > 3) {
                    if (Left(stlGlobals.Lines[2].Trim(), 1) == "[") {
                        stlGlobals.Lines[2] = "[ global defines file for " + NewID;
                    }
                    //save it
                    stlGlobals.Save();
                }
            }
            else
            //if not using template,
            {
                //validate new version
                if (IntVersions.Contains(NewVersion)) {
                    //ok; set version
                    agIntVersion = NewVersion;
                    //set version3 flag
                    agIsVersion3 = (Val(NewVersion) > 3);
                }
                else {
                    if (Val(NewVersion) < 2 || Val(NewVersion) > 3) {
                        //not a version 2 or 3 game
                        Exception e = new(LoadResString(597))
                        {
                            HResult = WINAGI_ERR + 597
                        };
                        throw e;
                    }
                    else {
                        //unsupported version 2 or 3 game
                        Exception e = new(LoadResString(543))
                        {
                            HResult = WINAGI_ERR + 543
                        };
                        throw e;
                    }
                }

                //set game id (limit to 6 characters for v2, and 5 characters for v3
                //(don't use the GameID property; gameloaded flag is not set yet
                //so using GameID property will cause error)
                if (agIsVersion3) {
                    agGameID = Left(NewID, 5);
                }
                else {
                    agGameID = Left(NewID, 6);
                }

                //create empty property file
                agGameFile = agGameDir + agGameID + ".wag";
                if (File.Exists(agGameFile)) {
                    File.Delete(agGameFile);
                }
                agGameProps = new SettingsList(agGameFile);
                agGameProps.Open();
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
                //add WinAGI version
                WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION);
                //set the resource directory name so it can be set up
                ResDirName = NewResDir;
                // assign source file extension
                agSrcFileExt = "." + NewExt.ToLower();
                //create default resource directories
                //(default DIR files shold only have ONE 'FFFFFF' entry)
                byte[] bytDirData = new byte[3];
                for (i = 0; i < 2; i++) bytDirData[i] = 0xff;

                if (agIsVersion3) {
                    // for v3, header should be '08 - 00 - 0B - 00 - 0E - 00 - 11 - 00
                    byte[] bytDirHdr = new byte[8] { 8, 0, 0x0b, 0, 0x0e, 0, 0x11, 0 };
                    using FileStream fsDIR = new FileStream(agGameDir + agGameID + "DIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirHdr);
                    for (i = 0; i < 4; i++) {
                        fsDIR.Write(bytDirData);
                    }
                    fsDIR.Dispose();
                }
                else {
                    FileStream fsDIR = new FileStream(agGameDir + "LOGDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR = new FileStream(agGameDir + "PICDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR = new FileStream(agGameDir + "SNDDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR = new FileStream(agGameDir + "VIEWDIR", FileMode.OpenOrCreate);
                    fsDIR.Write(bytDirData);
                    fsDIR.Dispose();
                }
                //create default vocabulary word list;  use loaded argument to force
                //load of the new wordlist so it can be saved
                agVocabWords = new WordList(this, true);
                agVocabWords.AddWord("a", 0);
                agVocabWords.AddWord("anyword", 1);
                agVocabWords.AddWord("rol", 9999);
                agVocabWords.Save();
                //create inventory objects list
                //use loaded argument to force load of new inventory list
                agInvObj = new InventoryList(this, true)
                {
                    { "?", 0 }
                };
                //adjust encryption based on version
                if (NewVersion == "2.089" || NewVersion == "2.272") {
                    agInvObj.Encrypted = false;
                }
                else {
                    agInvObj.Encrypted = true;
                }
                agInvObj.Save();

                //commands based on AGI version
                CorrectCommands(agIntVersion);

                //add logic zero
                agLogs.Add(0);
                agLogs[0].Clear();
                agLogs[0].Save();
                agLogs[0].Unload();

                //force id reset
                Compiler.blnSetIDs = false;

                //set open flag, so properties can be updated
                agGameLoaded = true;
            }

            //set resource directory
            //ensure resource directory exists
            if (!Directory.Exists(agGameDir + agResDirName)) {
                if (Directory.CreateDirectory(agGameDir + agResDirName) == null) {
                    //note the problem as a warning
                    TWarnInfo warnInfo = new();
                    warnInfo.Type = EWarnType.ecLoadWarn;
                    warnInfo.LWType = ELoadWarningSource.lwOther;
                    warnInfo.WarningNum = 1;
                    warnInfo.Text = "Can't create " + agResDir;
                    Raise_LoadGameEvent(ELStatus.lsLoadWarning, AGIResType.rtGame, 0, warnInfo);
                    //use main directory
                    agResDir = agGameDir;
                    //set warning flag
                    blnWarnings = true;
                }
            }
            //for non-template games, save the source code for logic 0
            if (TemplateDir.Length == 0) {
                agLogs[0].SaveSource();
            }

            //save gameID, version, directory resource name to the property file;
            //rest of properties need to be set by the calling function
            WriteGameSetting("General", "GameID", agGameID);
            WriteGameSetting("General", "Interpreter", agIntVersion);
            WriteGameSetting("General", "ResDir", agResDirName);
            WriteGameSetting("General", "SourceFileExt", agSrcFileExt[1..].ToUpper());

            //save palette colors
            for (i = 0; i < 16; i++) {
                WriteGameSetting("Palette", "Color" + i, agEGAcolors.ColorText(i));
            }

            //if errors
            // TODO: convert error handler to function return value
            if (blnWarnings) {
                Exception e = new(LoadResString(637))
                {
                    HResult = WINAGI_ERR + 637
                };
                throw e;
            }
            return;
        }
        public int OpenGameDIR(string NewGameDir)
        {
            //creates a new WinAGI game file from Sierra game directory

            TWarnInfo warnInfo = new();

            //if a game is already open,
            if (agGameLoaded) {
                //can't open a game if one is already open
                return WINAGI_ERR + 501;
            }

            //set game directory
            agGameDir = CDir(NewGameDir);

            //update status
            warnInfo.Type = EWarnType.ecLoadWarn;
            warnInfo.Text = "";
            Raise_LoadGameEvent(ELStatus.lsValidating, AGIResType.rtNone, 0, warnInfo);

            //attempt to extract agGameID, agGameFile and interpreter version
            //from Sierra files (i.e. from DIR files and VOL files)

            //check for valid DIR/VOL files
            //(which gets gameid, and sets version 3 status flag)
            if (!IsValidGameDir(agGameDir)) {
                //save dir as error string
                //clear game variables
                ClearGameState();
                //invalid game directory
                return WINAGI_ERR + 541;
            }

            //create a new wag file name
            agGameFile = agGameDir + agGameID + ".wag";

            //rename any existing game file (in case user is re-importing)
            try {

                 if (File.Exists(agGameFile)) {
                    if (File.Exists(agGameFile + ".OLD")) {
                        File.Delete(agGameFile + ".OLD");
                    }
                    File.Move(agGameFile, agGameFile + ".OLD");
                }
            }
            catch {
                //clear game variables
                ClearGameState();
                // file error
                // TODO: convert return values to an enum
                return WINAGI_ERR + 999;
            }

            //create new wag file
            agGameProps = new SettingsList(agGameFile);
            agGameProps.Open(false);
            agGameProps.Lines.Add("# WinAGI Game Property File");
            agGameProps.Lines.Add("#");
            agGameProps.WriteSetting("General", "WinAGIVersion", WINAGI_VERSION);
            agGameProps.WriteSetting("General", "GameID", agGameID);

            //get version number (version3 flag already set)
            agIntVersion = Base.GetIntVersion(agGameDir, agIsVersion3);

            //if not valid
            if (agIntVersion.Length == 0) {
                //clear game variables
                ClearGameState();
                //invalid number found
                return WINAGI_ERR + 543;
            }

            //save version
            WriteGameSetting("General", "Interpreter", agIntVersion);

            //finish the game load
            return FinishGameLoad(OpenGameMode.File);
        }
        internal void ClearGameState()
        {
            //clears basic game variables for a blank, unopened game
            // no game is loaded
            agGameLoaded = false;
            agLogs = new Logics(this);
            agPics = new Pictures(this);
            agSnds = new Sounds(this);
            agViews = new Views(this);
            agInvObj = new InventoryList(this);
            agVocabWords = new WordList(this);
            agGlobals = new GlobalList(this);

            //clear out game properties
            agGameID = "";
            agIntVersion = "";
            agIsVersion3 = false;
            agGameProps = new SettingsList("");
            agLastEdit = new DateTime();// Convert.ToDateTime("");
            agSierraSyntax = false;
            agPowerPack = false;

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
            IndentLevel = 2; // TODO: set default indent?
        }
        public int OpenGameWAG(string GameWAG)
        {
            //TODO: all game manipulation functions (open, new, finish, close, etc
            // should be bool or int functions, instead of using error handler

            //opens a WinAGI game file (must be passed as a full length file name)

            string strVer;

            //if a game is already open,
            if (agGameLoaded) {
                //can't open a game if one is already open
                return WINAGI_ERR + 501;
            }

            //verify the wag file exists
            if (!File.Exists(GameWAG)) {
                //is the file missing?, or the directory?
                if (Directory.Exists(JustPath(GameWAG, true))) {
                    //it's a missing file - return wagfile as error string
                    //invalid wag
                    return WINAGI_ERR + 655;
                }
                else {
                    //it's an invalid or missing directory - return directory as error string
                    //invalid wag
                    return WINAGI_ERR + 541;
                }
            }

            //reset game variables
            ClearGameState();

            //set game file property
            agGameFile = GameWAG;

            //set game directory
            agGameDir = JustPath(GameWAG);

            //open the property file (file has to already exist)
            try {
                agGameProps = new SettingsList(agGameFile);
                agGameProps.Open(false);
            }
            catch (Exception) {
                //// reset game variables
                ClearGameState();
                return WINAGI_ERR + 655;
            }
            //check to see if it's valid
            strVer = agGameProps.GetSetting("General", "WinAGIVersion", "");
            if (strVer != WINAGI_VERSION) {
                if (Left(strVer, 4) == "1.2." || (Left(strVer, 2) == "2.")) {
                    //any v1.2.x or 2.x is ok, but update
                    // TODO: after testing, re-enable version updating
                    //            WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION);
                }
                else {
                    //clear game variables
                    ClearGameState();
                    //invalid wag
                    return WINAGI_ERR + 665;
                }
            }
            //get gameID
            agGameID = agGameProps.GetSetting("General", "GameID", "");

            //if an id is found, keep going
            if (agGameID.Length > 0) {
                //got ID; now get interpreter version from propfile
                agIntVersion = agGameProps.GetSetting("General", "Interpreter", "");

                //validate it
                if (!IntVersions.Contains(agIntVersion)) {
                    //save wagfile as error string
                    //clear game variables
                    ClearGameState();
                    //invalid int version inside wag file
                    return WINAGI_ERR + 691;
                }
            }
            else {
                //missing GameID in wag file - make user address it
                //save wagfile name as error string
                //clear game variables
                ClearGameState();
                //invalid wag file
                return WINAGI_ERR + 690;
            }

            //if a valid wag file was found, we now have agGameID, agGameFile
            //and correct interpreter version;

            //finish the game load
            return FinishGameLoad(0);
        }
        public int FinishGameLoad(OpenGameMode Mode)
        {
            //finishes game load
            //mode determines whether opening by wag file or
            //extracting from Sierra game files
            //(currently, no difference between them)

            //instead of throwing exceptions, errors get passed back as a return value

            bool blnWarnings;
            TWarnInfo loadInfo = new()
            {
                Type = EWarnType.ecLoadWarn
            };

            //set v3 flag
            agIsVersion3 = (Val(agIntVersion) >= 3);
            //if loading from a wag file
            if (Mode == OpenGameMode.File) {
                //informs user we are checking property file to set game parameters
                loadInfo.LWType = ELoadWarningSource.lwNA;
                loadInfo.WarningNum = 0;
                loadInfo.Text = "";
                loadInfo.Module = "--";
                Raise_LoadGameEvent(ELStatus.lsPropertyFile, AGIResType.rtNone, 0, loadInfo);

                //get resdir before loading resources
                agResDirName = agGameProps.GetSetting("General", "ResDir", "");
            }
            else {
                //no resdir yet for imported game
                agResDirName = "";
                //if a set.game.id command is found during decompilation
                // it will be used as the suggested ID
                DecodeGameID = "";
            }

            //if none, check for existing, or use default
            if (agResDirName.Length == 0) {
                //look for an existing directory

                if (Directory.GetDirectories(agGameDir).Length == 1) {
                    //assume it is resource directory
                    agResDirName = Directory.GetDirectories(agGameDir)[0];
                }
                else {
                    //either no subfolders, or more than one
                    //so use default
                    agResDirName = agDefResDir;
                }
                WriteGameSetting("General", "ResDir", JustFileName(agResDirName));
            }
            //now create full resdir from name
            agResDir = agGameDir + agResDirName + @"\";

            //ensure resource directory exists
            if (!Directory.Exists(agResDir)) {
                try {
                    Directory.CreateDirectory(agResDir);
                }
                catch (Exception) {
                    //if can't create the resources directory
                    //note the problem as a warning
                    loadInfo.LWType = ELoadWarningSource.lwOther;
                    loadInfo.WarningNum = 1;
                    loadInfo.Text = "Can't create " + agResDir;
                    loadInfo.Module = "--";
                    Raise_LoadGameEvent(ELStatus.lsLoadWarning, AGIResType.rtGame, 0, loadInfo);
                    //use game directory
                    agResDir = agGameDir;
                    //set warning
                    blnWarnings = true;
                }
            }

            try {
                blnWarnings = ExtractResources(this);
            }
            catch (Exception e) {
                //if there was an error
                //can't continue
                //save error information
                //strError = e.Message;
                //lngError = Err.Number

                //clear game variables
                ClearGameState();

                return WINAGI_ERR + e.HResult;
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
                //get rest of game properties
                GetGameProperties();
            }
            catch (Exception e) {
                //note the problem as a warning
                loadInfo.LWType = ELoadWarningSource.lwOther;
                loadInfo.WarningNum = 2;
                loadInfo.Text = "Error while loading WAG file; some properties not loaded. (Error number: " + e.Message + ")";
                loadInfo.Module = "--";
                Raise_LoadGameEvent(ELStatus.lsLoadWarning, AGIResType.rtGame, 0, loadInfo);
                //set warning
                blnWarnings = true;
            }

            //load vocabulary word list
            loadInfo.LWType = ELoadWarningSource.lwNA;
            loadInfo.WarningNum = 0;
            loadInfo.Text = "";
            loadInfo.Module = "--";
            Raise_LoadGameEvent(ELStatus.lsResources, AGIResType.rtWords, 0, loadInfo);
            try {
                agVocabWords = new WordList(this);
                agVocabWords.Load(agGameDir + "WORDS.TOK");
            }
            catch (Exception e) {
                //if there was an error,
                //note the problem as a warning
                loadInfo.LWType = ELoadWarningSource.lwResource;
                loadInfo.WarningNum = 1;
                loadInfo.Text = "An error occurred while loading WORDS.TOK: " + e.Message;
                loadInfo.Module = "--";
                Raise_LoadGameEvent(ELStatus.lsLoadWarning, AGIResType.rtGame, 0, loadInfo);
                //set warning flag
                blnWarnings = true;
            }

            //load inventory objects list
            loadInfo.LWType = ELoadWarningSource.lwNA;
            loadInfo.WarningNum = 0;
            loadInfo.Text = "";
            loadInfo.Module = "--";
            Raise_LoadGameEvent(ELStatus.lsResources, AGIResType.rtObjects, 0, loadInfo);
            try {
                agInvObj = new InventoryList(this);
                agInvObj.Load();
            }
            catch (Exception e) {
                //if there was an error,
                //note the problem as a warning
                loadInfo.LWType = ELoadWarningSource.lwResource;
                loadInfo.WarningNum = 1;
                loadInfo.Text = "An error occurred while loading OBJECT: " + e.Message;
                loadInfo.Module = "--";
                Raise_LoadGameEvent(ELStatus.lsLoadWarning, AGIResType.rtGame, 0, loadInfo);
                //set warning flag
                blnWarnings = true;
            }

            //we've established that the game can be opened
            //so set the loaded flag now
            agGameLoaded = true;

            // adust commands based on AGI version
            CorrectCommands(agIntVersion);

            // check for decompile warnings, TODO entries and validate logic CRC values
            //(this has to happen AFTER everything is loaded otherwise some resources
            // that are referenced in the logics won't exist yet, and will give a
            // false warning about missing resources)
            foreach (Logic tmpLog in agLogs) {
                //the CRC/source check depends on mode
                if (Mode == OpenGameMode.File) {
                    //opening existing wag file - check CRC
                    loadInfo.LWType = ELoadWarningSource.lwNA;
                    loadInfo.WarningNum = 0;
                    loadInfo.Text = "";
                    loadInfo.Module = "--";
                    Raise_LoadGameEvent(ELStatus.lsCheckCRC, AGIResType.rtLogic, tmpLog.Number, loadInfo);
                    //if there is a source file, need to verify source CRC value
                    if (File.Exists(tmpLog.SourceFile)) {
                        //recalculate the CRC value for this sourcefile by loading the source
                        tmpLog.LoadSource(false);
                        // check it for TODO items
                        //ExtractTODO(tmpLog.Number, tmpLog.SourceText);
                        // check for Decompile warnings
                        //ExtractDecompWarn(tmpLog.Number, tmpLog.SourceText);
                        // then unload it
                        tmpLog.Unload();
                    }
                    else {
                        // no source; consider logic as clean
                        tmpLog.CRC = 0;
                        tmpLog.CompiledCRC = 0;
                    }
                }
                else {
                    // extracting from game directory
                    // if extracting from a directory, none of the logics have source
                    // code; they should all be extracted here, and marked as clean
                    // (if error occurs in decoding, the error gets caught, noted, and
                    // a blank source file is used instead)

                    // decompiling
                    loadInfo.LWType = ELoadWarningSource.lwNA;
                    loadInfo.WarningNum = 0;
                    loadInfo.Text = "";
                    loadInfo.Module = "--";
                    Raise_LoadGameEvent(ELStatus.lsDecompiling, AGIResType.rtLogic, tmpLog.Number, loadInfo);
                    try {
                        tmpLog.LoadSource(true);
                    }
                    catch (Exception e) {
                        if (e.HResult == WINAGI_ERR + 688) {
                            //create a blank file for this bad logic
                            tmpLog.Clear();
                            // use export function to save it without any other updating
                            tmpLog.SaveSource("", true);
                            // finally, change the compiled CRC value to default, since
                            // it no longer matches the source CRC
                            tmpLog.CompiledCRC = 0;
                            loadInfo.LWType = ELoadWarningSource.lwResource;
                            loadInfo.WarningNum = 4;
                            loadInfo.Text = "Logic " + tmpLog.Number + " cannot be decompiled (" + e.Message + "); inserting blank source file";
                            loadInfo.Module = tmpLog.ID;
                            Raise_LoadGameEvent(ELStatus.lsLoadWarning, AGIResType.rtLogic, tmpLog.Number, loadInfo);
                        }
                    }
                    // then unload it
                    tmpLog.Unload();
                }
                // if a new ID found, use it
                if (DecodeGameID.Length != 0) {
                    agGameID = DecodeGameID;
                    DecodeGameID = "";
                    File.Move(agGameFile, agGameDir + agGameID + ".wag", true);
                    agGameFile = agGameDir + agGameID + ".wag";
                    agGameProps.Lines[0] = agGameFile;
                    WriteGameSetting("General", "GameID", agGameID);
                }
            }
            // force id reset
            blnSetIDs = false;

            // if extracting a game
            if (Mode == OpenGameMode.Directory) {
                // write create date
                WriteGameSetting("General", "LastEdit", agLastEdit);
            }

            loadInfo.LWType = ELoadWarningSource.lwNA;
            loadInfo.WarningNum = 0;
            loadInfo.Text = "";
            loadInfo.Module = "--";
            Raise_LoadGameEvent(ELStatus.lsFinalizing, AGIResType.rtNone, 0, loadInfo);

            //if errors
            if (blnWarnings) {
                return WINAGI_ERR + 636;
            }
            else {
                return 0;
            }
        }
        internal void WriteGameSetting(string Section, string Key, dynamic Value, string Group = "")
        {
            agGameProps.WriteSetting(Section, Key, Value.ToString(), Group);

            if (Key.ToLower() != "lastedit" && Key.ToLower() != "winagiversion" && Key.ToLower() != "palette")
                agLastEdit = DateTime.Now;
        }
        public void WriteProperty(string Section, string Key, string Value, string Group = "", bool ForceSave = false)
        {
            // this procedure provides calling programs a way to write property
            // values to the WAG file

            // no validation of section or newval is done, so calling function
            // needs to be careful

            try {
                WriteGameSetting(Section, Key, Value, Group);

                // if forcing a save
                if (ForceSave) {
                    SaveProperties();
                }
            }
            catch (Exception) {

                //ignore if error?
            }
            return;
        }
        public bool IsValidGameDir(string strDir)
        {
            string strFile;
            byte[] bChunk = new byte[6];
            FileStream fsCOM;

            //this function will determine if the strDir is a
            //valid sierra AGI game directory
            //it also sets the gameID, if one is found and the version3 flag
            //search for 'DIR' files
            int dirCount;
            try {
                dirCount = Directory.EnumerateFiles(strDir, "*DIR").Count();
            }
            catch (Exception) {
                // if error, assume NOT a directory
                return false;
            }

            if (dirCount > 0) {
                //this might be an AGI game directory-
                // if exactly four dir files
                if (dirCount == 4) {
                    // assume it's a v2 game

                    // check for at least one VOL file
                    if (File.Exists(strDir + "VOL.0")) {
                        //clear version3 flag
                        agIsVersion3 = false;

                        //clear ID
                        agGameID = "";

                        //look for loader file to find ID
                        foreach (string strLoader in Directory.EnumerateFiles(strDir, "*.COM")) {
                            //open file and get chunk
                            string strChunk = new string(' ', 6);
                            using (fsCOM = new FileStream(strLoader, FileMode.Open)) {
                                // see if the word 'LOADER' is at position 3 of the file
                                fsCOM.Position = 3;
                                fsCOM.Read(bChunk, 0, 6);
                                strChunk = Encoding.UTF8.GetString(bChunk);
                                fsCOM.Dispose();

                                //if this is a Sierra loader
                                if (strChunk == "LOADER") {
                                    // determine ID to use
                                    //if not SIERRA.COM
                                    strFile = JustFileName(strLoader);
                                    if (strLoader != "SIERRA.COM") {
                                        //use this filename as ID
                                        agGameID = Left(strFile, strFile.Length - 4).ToUpper();
                                        return true;
                                    }
                                }
                            }
                        }

                        //if no loader file found (looped through all files, no luck)
                        //use default
                        agGameID = "AGI";
                        return true;
                    }
                }
                else if (dirCount == 1) {
                    //if only one, it's probably v3 game
                    strFile = JustFileName(Directory.GetFiles(strDir, "*DIR")[0].ToUpper());
                    agGameID = Left(strFile, strFile.IndexOf("DIR"));

                    // check for matching VOL file;
                    if (File.Exists(strDir + agGameID + "VOL.0")) {
                        //set version3 flag
                        agIsVersion3 = true;
                        return true;
                    }

                    //if no vol file, assume not valid
                    agGameID = "";
                    return false;
                }
            }

            // no valid files/loader found; not an AGI directory
            return false;
        }
        internal void GetGameProperties()
        {
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
            //     description
            //     author
            //     about
            //     game version
            //     last date
            //     platform, platform program, platform options, dos executable
            //     use res names property
            //     use layout editor

            //Palette: (make sure AGI defaults set first)
            ResetDefaultColors();
            for (int i = 0; i < 16; i++) {
                AGIColors[i] = agGameProps.GetSetting("Palette", "Color" + i.ToString(), DefaultColors[i]);
            }
            //description
            agDescription = agGameProps.GetSetting("General", "Description", "");

            //author
            agAuthor = agGameProps.GetSetting("General", "Author", "");

            //about
            agAbout = agGameProps.GetSetting("General", "About", "");

            //game version
            agGameVersion = agGameProps.GetSetting("General", "GameVersion", "");


            if (!DateTime.TryParse(agGameProps.GetSetting("General", "LastEdit", DateTime.Now.ToString()), out agLastEdit)) {
                // default to now
                agLastEdit = DateTime.Now;
            }

            //platform
            agPlatformType = agGameProps.GetSetting("General", "PlatformType", 0);

            //platform program
            agPlatformFile = agGameProps.GetSetting("General", "Platform", "");

            //dos executable
            agDOSExec = agGameProps.GetSetting("General", "DOSExec", "");

            //platform options
            agPlatformOpts = agGameProps.GetSetting("General", "PlatformOpts", "");

            //use res names property (use current value, if one not found in property file)
            Compiler.UseReservedNames = agGameProps.GetSetting("General", "UseResNames", Compiler.UseReservedNames);

            // use layout editor property
            agUseLE = agGameProps.GetSetting("General", "UseLE", false);
        }
        internal void CompleteCancel(bool NoEvent = false)
        {
            //cleans up after a compile game cancel or error

            if (!NoEvent) {
                TWarnInfo tmpWarn = new();
                Raise_CompileGameEvent(ECStatus.csCanceled, 0, 0, tmpWarn);
            }
            agCompGame = false;
            fsDIR.Dispose();
            fsVOL.Dispose();
            bwVOL.Dispose();
            bwDIR.Dispose();
        }
    }
}
