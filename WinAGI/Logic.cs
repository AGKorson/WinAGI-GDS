using System;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.Compiler;
using static WinAGI.Common.Base;
using System.IO;
using System.Diagnostics;

namespace WinAGI.Engine {
    public class Logic : AGIResource {
        //source code properties
        string mSourceFile = "";
        string mSourceText = "";
        uint mCompiledCRC;
        uint mCRC;
        bool mSourceDirty;
        bool mIsRoom;
        //int mCodeSize;

        internal void LoadNoSource() {
            // used by extractresources function to load
            // logic data without loading the sourcecode

            // load the base resource data
            base.Load();
            if (mErrLevel < 0) {
                // return a blank logic resource
                ErrClear();
            }
            // set code size (add 2 to msgstart offset)
            CodeSize = ReadWord(0) + 2;
            // clear dirty flag
            mIsDirty = false;
            mSourceDirty = false;
        }

        public override void Load() {
            // if not ingame, the resource should already be loaded
            if (!mInGame) {
                Debug.Assert(mLoaded);
            }
            // if already loaded, just exit
            if (mLoaded) {
                return;
            }
            mSourceDirty = false;
            // compiledCRC Value should already be set,
            // and source crc gets calculated when source is loaded

            // load the base resource data
            base.Load();
            if (mErrLevel < 0) {
                ErrClear();
            }
            // create empty logic with blank source code
            // set code size (add 2 to msgstart offset)
            CodeSize = ReadWord(0) + 2;
            //  load the sourcetext
            LoadSource();
        }

        public override void Unload() {
            base.Unload();
            mSourceDirty = false;
            mSourceText = "";
        }

        private void InitLogic(Logic NewLogic = null) {
            // attach events
            base.PropertyChanged += ResPropChange;
            if (NewLogic is null) {
                // set default resource data by clearing
                Clear();
                // set default source
                mSourceText = ActionCommands[0].Name + "();" + NEWLINE + NEWLINE + "[ messages" + NEWLINE;
                // to avoid having compile property read true if both values are 0, set compiled to -1 on initialization
                CompiledCRC = 0xffffffff;
                CRC = 0;
            }
            else {
                // clone this logic
                NewLogic.Clone(this);
            }
        }
        
        public Logic() : base(AGIResType.rtLogic) {
            // new logic, not in game

            // initialize
            InitLogic();
            // create a default ID
            mResID = "NewLogic";
            // if not in a game, resource is always loaded
            mLoaded = true;
        }

        internal Logic(AGIGame parent, byte ResNum, Logic NewLogic = null) : base(AGIResType.rtLogic) {
            // internal method to add a new logic and find place for it in vol files
            // TODO: why doesn't this constructor assign events?
            // initialize
            InitLogic(NewLogic);
            // set up base resource
            base.InitInGame(parent, ResNum);
            //if res is zero
            if (ResNum == 0) {
                //make sure isroom flag is false
                mIsRoom = false;
            }
        }
        
        internal Logic(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.rtLogic) {
            // adds a logic from dir/vol files, setting its resource 
            // location properties, and reads properties from the wag file

            // attach events
            base.PropertyChanged += ResPropChange;
            // set up base resource
            base.InitInGame(parent, ResNum, VOL, Loc);

            // if importing, there will be nothing in the propertyfile
            mResID = parent.agGameProps.GetSetting("Logic" + ResNum, "ID", "", true);
            if (mResID.Length == 0) {
                //no properties to load; save default ID
                mResID = "Logic" + ResNum;
                parent.WriteGameSetting("Logic" + ResNum, "ID", ID, "Logics");
                //save CRC and CompCRC values as defaults; they'll be adjusted first time logic is accessed
                parent.WriteGameSetting("Logic" + ResNum, "CRC32", "0x00000000", "Logics");
                parent.WriteGameSetting("Logic" + ResNum, "CompCRC32", "0xffffffff");
            }
            else {
                //get description, size and other properties from wag file
                mDescription = parent.agGameProps.GetSetting("Logic" + ResNum, "Description", "");
                mCRC = parent.agGameProps.GetSetting("Logic" + ResNum, "CRC32", (uint)0);
                mCompiledCRC = parent.agGameProps.GetSetting("Logic" + ResNum, "CompCRC32", (uint)0xffffffff);
                mIsRoom = parent.agGameProps.GetSetting("Logic" + ResNum, "IsRoom", false);
            }
            if (ResNum == 0) {
                //make sure isroom flag is false
                mIsRoom = false;
            }
        }
        
        public Logic Clone() {
            //copies logic data from this logic and returns a completely separate object reference
            Logic CopyLogic = new();
            // copy base properties
            base.Clone(CopyLogic);
            //add WinAGI items
            CopyLogic.mIsRoom = IsRoom;
            CopyLogic.mLoaded = mLoaded;
            //crc data
            CopyLogic.mCompiledCRC = mCompiledCRC;
            CopyLogic.mCRC = mCRC;
            CopyLogic.mSourceText = mSourceText;
            CopyLogic.mSourceDirty = mSourceDirty;
            // TODO: figure out how to handle source filename when cloning
            CopyLogic.mSourceFile = mSourceFile;
            return CopyLogic;
        }
        
        public uint CompiledCRC {
            get {
                //if not in a game
                if (!InGame) {
                    return 0;
                }
                else {
                    return mCompiledCRC;
                }
            }

            internal set { }
        }
        
        public bool IsRoom {
            get { return mIsRoom; }
            set {
                //if in a game, and logic 0
                if (mInGame && (Number == 0)) {
                    WinAGIException wex = new(LoadResString(450)) {
                        HResult = WINAGI_ERR + 450,
                    };
                    throw wex;
                }

                //if changing
                if (mIsRoom != value) {
                    mIsRoom = value;
                    parent.WriteGameSetting("Logic" + Number, "IsRoom", mIsRoom.ToString(), "Logics");
                }
            }
        }
        
        public bool SourceDirty {
            get { return mSourceDirty; }
        }
        
        public string SourceFile {
            get {
                //if in a game,
                if (mInGame) {
                    //sourcefile is predefined
                    // force re-sync of name
                    //    Debug.Assert(mSourceFile == parent.agResDir + mResID + agSrcFileExt);
                    mSourceFile = parent.agResDir + mResID + agSrcFileExt;
                }
                return mSourceFile;
            }
            set {
                //if in a game,
                if (mInGame) {
                    //sourcefile is predefined; raise error
                    WinAGIException wex = new(LoadResString(450)) {
                        HResult = WINAGI_ERR + 450,
                    };
                    throw wex;
                }
                else {
                    mSourceFile = value;
                }
            }
        }
        
        public uint CRC { get; internal set; }
        
        public int CodeSize { get; internal set; }
        
        private void ResPropChange(object sender, AGIResPropChangedEventArgs e) {
            ////let's do a test
            //// increment number everytime data changes
            //Number++;
        }

        private void SaveProps() {
            string strSection;
            //save properties
            strSection = "Logic" + Number;
            parent.WriteGameSetting(strSection, "ID", ID, "Logics");
            parent.WriteGameSetting(strSection, "Description", Description);
            parent.WriteGameSetting(strSection, "CRC32", "0x" + mCRC.ToString("x8"));
            parent.WriteGameSetting(strSection, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
            parent.WriteGameSetting(strSection, "IsRoom", mIsRoom.ToString());

            //set flag indicating all props are uptodate
            WritePropState = false;
        }
        
        public bool Compiled {
            get {
                // ingame:
                //   return true if source code CRC is same as stored compiled crc
                // not in a game:
                //   always false

                if (mInGame) {
                    return mCRC == mCompiledCRC;
                }
                else {
                    return false;
                }
            }
        }

        public override void Clear() {
            // clears out source code text
            // and clears the resource data
            if (mInGame) {
                if (!mLoaded) {
                    //nothing to clear
                    WinAGIException wex = new(LoadResString(563)) {
                        HResult = WINAGI_ERR + 563
                    };
                    throw wex;
                }
            }
            // clear resource
            base.Clear();
            //set default resource data
            // TODO: confirm correct empty logic data block
            mRData.AllData = [0x01, 0x00, 0x00, 0x00, 0x02, 0x00];
            // byte0 = low byte of msg section offset (relative to byte 2)
            // byte1 = high byte of msg section offset
            // byte2 = first byte of code data (a single return)
            // byte3 = first byte of msg section = # of messages
            // byte4 = high byte of msg end offset
            // byte5 = low byte of msg end offset

            // clear the source code by setting it to 'return' command
            mSourceText = ActionCommands[0].Name + "();" + NEWLINE + NEWLINE + "[ messages" + NEWLINE;
            if (mInGame) {
                // reset crcs
                mCRC = CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(mSourceText));
                mCompiledCRC = mCRC;
            }
            // note change by marking source as dirty
            mSourceDirty = true;
            // TODO: for a completely empty logic, is it two? or three? or 4?
            CodeSize = 4;
        }

        public void Export(string ExportFile, bool ResetDirty) {
            // exports a compiled resource; ingame only
            // (since only ingame logics can be compiled)

            if (!mInGame) {
                //not allowed; nothing to export
                WinAGIException wex = new(LoadResString(668)) {
                    HResult = WINAGI_ERR + 668
                };
                throw wex;
            }
            if (!mLoaded) {
                //error
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            // export a logic resource file
            base.Export(ExportFile);
        }

        public override void Import(string ImportFile) {
            // use import, assuming as logic source
            Import(ImportFile, true);
        }

        public void Import(string ImportFile, bool AsSource) {
            // imports a logic resource from a standalone file
            // doesn't matter if ingame or not

            // TODO: importing also has to load the resource and set error level

            // if importing a logic resource, it will overwrite current source text with decompiled source
            // if importing a source file, ther resource data is left alone, and compiled status is adjusted

            // clear existing resource
            Clear();
            // if importing source code
            if (AsSource) {
                // load the source code
                if (!mInGame) {
                    mSourceFile = ImportFile;
                }
                LoadSource();
                // set ID to the filename without extension;
                // the calling function will take care or reassigning it later, if needed
                // (for example, if the new logic will be added to a game)
                mResID = Path.GetFileNameWithoutExtension(ImportFile);
                // TODO: all resources need to validate ID when importing
                if (mResID.Length > 64) {
                    mResID = Left(mResID, 64);
                }
                //reset dirty flags
                mIsDirty = false;
                mSourceDirty = false;
            }
            else {
                try {
                    // import the compiled resource
                    base.Import(ImportFile);
                    //load the source code by decompiling
                    LoadSource(true);
                    if (!mInGame) {
                        //force filename to null
                        mSourceFile = "";
                    }
                }
                catch (Exception) {
                    Unload();
                    throw;
                }
                finally {
                    // set ID to the filename without extension;
                    // the calling function will take care or reassigning it later, if needed
                    // (for example, if the new logic will be added to a game)
                    mResID = Path.GetFileNameWithoutExtension(ImportFile);
                    // TODO: all resources need to validate ID when importing
                    if (mResID.Length > 64) {
                        mResID = Left(mResID, 64);
                    }
                    //reset dirty flags
                    mIsDirty = false;
                    mSourceDirty = false;
                }
            }
        }
        
        public override string ID {
            get => base.ID;
            set {
                base.ID = value;
                if (mInGame) {
                    //source file always tracks ID for ingame resources
                    mSourceFile = parent.agResDir + base.ID + agSrcFileExt;
                    parent.WriteGameSetting("Logic" + Number, "ID", mResID, "Logics");
                }
            }
        }
        
        internal void LoadSource(bool Decompile = false) {
            // loads LoadFile as the source code
            // for not-ingame, calling code must first set sourcefile name
            string LoadFile;
            uint tmpCRC;

            //if in a game,
            if (mInGame) {
                // load file is predefined
                LoadFile = parent.agResDir + mResID + agSrcFileExt;
                //    Debug.Assert(mSourceFile == LoadFile);

                //if it does not exist,
                if (!File.Exists(LoadFile)) {
                    //check for AGI Studio file format
                    if (File.Exists(parent.agResDir + "logic" + Number + ".txt")) {
                        //rename it to correct format
                        File.Move(parent.agResDir + "logic" + Number + ".txt", LoadFile);
                    }
                    else if (File.Exists(parent.agResDir + mResID + ".txt")) {
                        //rename it to correct format
                        File.Move(parent.agResDir + mResID + ".txt", LoadFile);
                    }
                    else {
                        //check for cg files
                        if (agSierraSyntax) {
                            if (File.Exists(parent.agResDir + "RM" + Number + ".cg")) {
                                //rename it to correct format
                                File.Move(parent.agResDir + "RM" + Number + ".cg", LoadFile);
                            }
                            else {
                                //default file does not exist
                                //AGI Studio file does not exist
                                //CG file does not exist
                                //force decompile
                                Decompile = true;
                            }
                        }
                        else {
                            //default file does not exist
                            //AGI Studio file does not exist
                            //force decompile
                            Decompile = true;
                        }
                    }
                }
            }
            else {
                LoadFile = SourceFile;
            }

            // if forcing decompile
            if (Decompile) {
                if (mErrLevel == 0) {
                    // get source code by decoding the resource, decrypting messages if not v3compressed
                    mSourceText = DecodeLogic(this, mInGame ? Number : -1);
                    if (mErrLevel < 0) {
                        // unable to decompile; force uncompiled state
                        mCRC = 0;
                        mCompiledCRC = 0xffffffff;
                        return;
                    }
                    // make sure decompile flag is set (so crc can be saved)
                    Decompile = true;
                }
                else {
                    // if base failed to load, there is nothing to decompile
                    mSourceText = "return();" + NEWLINE;
                    // force uncompile state
                    mCRC = 0;
                    mCompiledCRC = 0xffffffff;
                    return;
                }
            }
            else {
                // verify file exists
                if (!File.Exists(LoadFile)) {
                    mErrLevel = -6;
                    ErrData[0] = LoadFile;
                    ErrData[1] = mResID;
                    mSourceText = "return();" + NEWLINE;
                    // force uncompile state
                    mCRC = 0;
                    mCompiledCRC = 0xffffffff;
                    return;
                    //WinAGIException wex = new(LoadResString(704).Replace(ARG1, LoadFile)) {
                    //    HResult = WINAGI_ERR + 704
                    //};
                    //wex.Data["missingfile"] = LoadFile;
                    //throw wex;
                }
                // check for readonly
                if ((File.GetAttributes(LoadFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    mErrLevel = -7;
                    ErrData[0] = LoadFile;
                    ErrData[1] = mResID;
                    //mSourceText = "return();" + NEWLINE;
                    //// force uncompile state
                    //mCRC = 0;
                    //mCompiledCRC = 0xffffffff;
                    //return;
                    //WinAGIException wex = new(LoadResString(700).Replace(ARG1, LoadFile)) {
                    //    HResult = WINAGI_ERR + 700,
                    //};
                    //wex.Data["badfile"] = LoadFile;
                    //throw wex;
                }
                try {
                    // load sourcecode from file
                    mSourceText = File.ReadAllText(LoadFile, parent.agCodePage);
                }
                catch (Exception e) {
                    mErrLevel = -8;
                    ErrData[0] = e.Message;
                    ErrData[1] = mResID;
                    mSourceText = "return();" + NEWLINE;
                    // force uncompile state
                    mCRC = 0;
                    mCompiledCRC = 0xffffffff;
                    return;
                    //WinAGIException wex = new(LoadResString(502).Replace(ARG1, e.HResult.ToString()).Replace(ARG2, LoadFile)) {
                    //    HResult = WINAGI_ERR + 502
                    //};
                    //wex.Data["exception"] = e;
                    //wex.Data["badfile"] = LoadFile;
                    //throw wex;
                }
            }
            // replace tabs with indent spaces
            if (mSourceText.Contains('\t')) {
                mSourceText = mSourceText.Replace("\t", INDENT);
            }
            // calculate source crc
            tmpCRC = CRC32(parent.agCodePage.GetBytes(mSourceText));
            if (mInGame) {
                if (mCRC != tmpCRC) {
                    // update crc
                    mCRC = tmpCRC;
                    parent.WriteGameSetting("Logic" + Number, "CRC32", "0x" + mCRC.ToString("x8"), "Logics");
                }
                // if decompiling, also save the source and the compiled crc
                // (it's the same as the source crc!)
                if (Decompile) {
                    SaveSource();
                    mCompiledCRC = tmpCRC;
                    parent.WriteGameSetting("Logic" + Number, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
                }
            }
            else {
                // update crc
                mCRC = tmpCRC;
            }
            // set dirty status
            mSourceDirty = false;
        }

        public string SourceText {
            get {
                // ********** SourceText is UNICODE, not CP byte code
                // it gets converted to CP byte code when it gets compiled
                return mSourceText;
            }
            set {
                mSourceText = value;

                //strip off any CRs at the end (unless it's the only character)
                int lngLen = mSourceText.Length;
                if (lngLen > 1) {
                    int i;
                    for (i = 1; i < lngLen; i++) {
                        if (Right(mSourceText, i)[0] != '\n') {
                            break;
                        }
                        //i=1 means no cr
                        //i=2 means one cr
                        //i=3 means two cr
                        // etc.
                        //if more than one found (meaning counter
                        // got to 3 before finding non-CR)
                        if (i > 2) {
                            //remove the extras
                            mSourceText = Left(mSourceText, lngLen - i + 2);
                        }
                    }
                    //remove the extras
                    mSourceText = Left(mSourceText, lngLen - i + 1);
                }

                //if in a game, save the crc value
                if (mInGame) {
                    //calculate new crc value
                    mCRC = CRC32(parent.agCodePage.GetBytes(mSourceText));
                    mSourceDirty = true;
                }
            }
        }
        
        public void SaveSource(string SaveFile = "", bool Exporting = false) {
            //saves the source code for this logic to file

            //no filename needed for ingame logics,
            //but must have filename if NOT ingame

            //if Exporting flag is true, we are just saving the file; don't do any other
            //maintenance on the logic resource

            if (SaveFile.Length == 0) {
                //if in a game
                if (mInGame) {
                    //filename is predfined
                    SaveFile = SourceFile;
                }
                else {
                    // filename missing
                    WinAGIException wex = new(LoadResString(599)) {
                        HResult = WINAGI_ERR + 599
                    };
                    throw wex;
                }
            }
            try {
                // need to replace single CR with CRLF (but first
                // check if the source already has the correct end-of-line marks

                // TODO: need strategy to deal with end-of-line; in files
                // allow whatver is local environment setting; when doing
                // CRC or compiling, only \r

                //if (!mSourceText.Contains("\r\n")) {
                //    mSourceText = mSourceText.Replace("\r", "\r\n");
                //}

                File.WriteAllBytes(SaveFile, parent.agCodePage.GetBytes(mSourceText));
                // TODO: replace all text file handling to use simpler
                // functions that incorporate codepage
            }
            catch (Exception) {
                // file error
                WinAGIException wex = new(LoadResString(582)) {
                    HResult = WINAGI_ERR + 582
                };
                throw wex;
            }
            //if exporting, nothing left to do
            if (Exporting) {
                return;
            }
            // reset source dirty flag
            mSourceDirty = false;
            // if in a game, update the source crc
            if (InGame) {
                //save the crc value
                parent.WriteGameSetting("Logic" + Number, "CRC32", "0x" + mCRC.ToString("x8"), "Logics");
                //change date of last edit
                parent.agLastEdit = DateTime.Now;
            }
            else {
                //update id to match savefile name
                mResID = Path.GetFileName(SaveFile);
            }
        }
        
        public void Compile() {
            //compiles the source code for this logic
            //and saves the resource
            TWinAGIEventInfo tmpInfo = new() {
                ID = "",
                Module = "",
                Text = ""
            };


            //if not in a game
            if (!mInGame) {
                WinAGIException wex = new(LoadResString(618)) {
                    HResult = WINAGI_ERR + 618
                };
                throw wex;
            }
            //if not loaded, raise error
            if (!mLoaded) {
                WinAGIException wex = new(LoadResString(563)) {
                    HResult = WINAGI_ERR + 563
                };
                throw wex;
            }
            //if no data in sourcecode
            if (mSourceText.Length == 0) {
                WinAGIException wex = new(LoadResString(546)) {
                    HResult = WINAGI_ERR + 546
                };
                throw wex;
            }
            try {
                //compile the logic
                tmpInfo = CompileLogic(this);
                //set dirty flag (forces save to work correctly)
                IsDirty = true;
                //save logic to vol file
                Save();
            }
            catch (Exception) {
                //force uncompiled state
                mCompiledCRC = 0;
                compGame.WriteGameSetting("Logic" + Number, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
                // pass it along
                throw;
            }
            // TODO: should Compile be a function, that returns value of true if successful
            // and false if not?

            //if no error, check result
            if (tmpInfo.Type == EventType.etError) {
                //force uncompiled state
                mCompiledCRC = 0;
                compGame.WriteGameSetting("Logic" + Number, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
            }
            else {
                //set dirty flag (forces save to work correctly)
                mIsDirty = true;
                //save logic to vol file
                Save();
            }
        }
        
        public void Save(string SaveFile = "") {
            // this file saves the logic resource to next available VOL file

            //NOTE: this saves the compiled resource NOT the
            //text based source code; 
            //SaveSource saves the source code
            //if Save method is called for a resource NOT in a game,
            //it calls the SaveSource method automatically

            //if properties need to be written
            if (WritePropState && mInGame) {
                SaveProps();
            }
            //if not loaded
            if (!mLoaded) {
                //nothing to do
                return;
            }
            //if in a game
            if (mInGame) {
                //if dirty
                if (mIsDirty) {
                    try {
                        //use the base resource save method
                        base.Save();
                    }
                    catch (Exception) {
                        throw;
                    }
                }
            }
            else {
                //if source is dirty
                if (mSourceDirty) {
                    //same as savesource
                    SaveSource(SaveFile);
                }
            }
            //reset dirty flag
            mIsDirty = false;
        }
        
        public override bool IsDirty {
            get {
                //if in a game,
                if (InGame) {
                    //if resource is dirty, or prop values need writing,
                    return (mIsDirty || WritePropState);
                }
                else {
                    return mSourceDirty;
                }
            }
        }
    }
}