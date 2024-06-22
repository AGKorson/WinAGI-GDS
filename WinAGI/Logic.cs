using System;
using System.IO;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.LogicCompiler;

namespace WinAGI.Engine {
    /// <summary>
    /// A class that represents an AGI Logic resource, with WinAGI extensions.
    /// </summary>
    public class Logic : AGIResource {
        #region Members
        internal string mSourceFile = "";
        string mSourceText = "";
        int mSrcErrLevel = 0;
        uint mCompiledCRC;
        uint mCRC;
        int mCodeSize;
        bool mSourceDirty;
        bool mIsRoom;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new AGI logic resource that is not in a game.
        /// </summary>
        public Logic() : base(AGIResType.Logic) {
            // not in a game so resource is always loaded
            mLoaded = true;
            InitLogic();
            // use a default ID
            mResID = "NewLogic";
        }

        /// <summary>
        /// Internal constructor to initialize a new or cloned logic resource being added to an AGI game.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="NewLogic"></param>
        internal Logic(AGIGame parent, byte ResNum, Logic NewLogic = null) : base(AGIResType.Logic) {
            InitLogic(NewLogic);
            base.InitInGame(parent, ResNum);
            if (ResNum == 0) {
                // make sure isroom flag is false
                mIsRoom = false;
            }
        }

        /// <summary>
        /// Internal constructor to add a new logic resource during initial game load.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="ResNum"></param>
        /// <param name="VOL"></param>
        /// <param name="Loc"></param>
        internal Logic(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.Logic) {
            // adds a logic from dir/vol files, setting its resource 
            // location properties, and reads properties from the wag file

            // set up base resource
            base.InitInGame(parent, AGIResType.Logic, ResNum, VOL, Loc);
            // get rest of properties
            mCRC = parent.agGameProps.GetSetting("Logic" + ResNum, "CRC32", (uint)0);
            mCompiledCRC = parent.agGameProps.GetSetting("Logic" + ResNum, "CompCRC32", (uint)0xffffffff);
            mSourceFile = parent.agResDir + mResID + parent.agSrcFileExt;
            if (ResNum == 0) {
                // logic0 can never be a room
                mIsRoom = false;
            }
            else {
                mIsRoom = parent.agGameProps.GetSetting("Logic" + ResNum, "IsRoom", false);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the resource ID for this logic. The source code file for
        /// in-game resources is automatically renamed to match the new ID.
        /// </summary>
        public override string ID {
            get => base.ID;
            set {
                base.ID = value;
                if (mInGame) {
                    try {
                        File.Move(mSourceFile, parent.agResDir + base.ID + parent.agSrcFileExt);
                    }
                    finally {
                        mSourceFile = parent.agResDir + base.ID + parent.agSrcFileExt;
                        parent.WriteGameSetting("Logic" + Number, "ID", mResID, "Logics");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the compiled CRC value for this logic's source code. The compiled
        /// CRC is set when a logic is compiled, indicating the compiled logic byte code
        /// is a true representation of the source code.
        /// </summary>
        public uint CompiledCRC {
            get => mCompiledCRC;
            internal set => mCompiledCRC = value;
        }

        /// <summary>
        /// Gets or sets a flag that is used by the WinAGI layout editor to determine which
        /// logics are treated as 'rooms' and are displayed in the layout.
        /// </summary>
        public bool IsRoom {
            get {
                return mIsRoom;
            }
            set {
                // ignore if in a game and logic 0
                if (mInGame && (Number == 0)) {
                    return;
                }
                if (mIsRoom != value) {
                    mIsRoom = value;
                    parent.WriteGameSetting("Logic" + Number, "IsRoom", mIsRoom, "Logics");
                }
            }
        }
        
        /// <summary>
        /// Gets a flag that indicates if changes in the source code for this logic have
        /// not been saved to file.
        /// </summary>
        public bool SourceDirty {
            get {
                if (mLoaded) {
                    return mSourceDirty;
                }
                else {
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the name of the source code file for this logic.
        /// </summary>
        public string SourceFile {
            get {
                return mSourceFile;
            }
            set {
                // ignore if in a game
                if (!mInGame) {
                    if (!IsValidFilename(value)) {
                        throw new ArgumentException("invalid file name", nameof(value));
                    }
                    mSourceFile = value;
                }
            }
        }

        /// <summary>
        /// Gets the error level for the source code file.
        /// </summary>
        public int SrcErrLevel {
            get {
                return mSrcErrLevel;
            }
        }
        
        /// <summary>
        /// Gets or sets the CRC value for this logic's source code. The CRC value is 
        /// used to track the compiled status of the logic.
        /// </summary>
        public uint CRC {
            get => mCRC;
            internal set {
                if (mLoaded) {
                    mCRC = value;
                }
            }
        }

        /// <summary>
        /// Gets the size of the logic byte code minus the message data.
        /// </summary>
        public int CodeSize {
            get {
                WinAGIException.ThrowIfNotLoaded(this);
                return mCodeSize;
            }
            internal set {
                mCodeSize = value;
            }
        }
        
        /// <summary>
        /// Gets the compiled status for this logic. A logic is considered compiled
        /// if the CRC for the current source code matches the CRC for the last
        /// compiled source code. Logics that are not in a game are never considered
        /// compiled.
        /// </summary>
        public bool Compiled {
            get {
                return mInGame && mCRC == mCompiledCRC;
            }
        }

        /// <summary>
        /// Gets or sets the source code text for this logic. The source code
        /// text can be compiled to convert it into AGI byte code data.
        /// </summary>
        public string SourceText {
            get {
                return mSourceText;
            }
            set {
                mSourceText = string.Join(NEWLINE, value.Split(separator, StringSplitOptions.None));
                // remove excess blank lines, leaving just one
                int lngLen = mSourceText.Length;
                if (lngLen > 1) {
                    int i;
                    for (i = 1; i < lngLen; i++) {
                        if (mSourceText[^i..][0] != '\n') {
                            break;
                        }
                        // i=1 means no cr
                        // i=2 means one cr
                        // i=3 means two cr
                        // etc.
                        // if more than one found (meaning counter got
                        // to 3 before finding non-CR)
                        if (i > 2) {
                            // remove the extras
                            mSourceText = mSourceText[(lngLen - i + 2)..];
                        }
                    }
                }
                if (mInGame) {
                    mCRC = CRC32(parent.agCodePage.GetBytes(mSourceText));
                    mSourceDirty = true;
                }
            }
        }

        private static readonly string[] separator = new[] {"\r\n", "\r", "\n" };
        #endregion

        #region Methods
        /// <summary>
        /// This method is used by the ExtractResources function to do the initial load of
        /// logic resource data without loading the source code text file.
        /// </summary>
        internal void LoadNoSource() {
            // load the base resource data
            base.Load();
            if (ErrLevel < 0) {
                // return a blank logic resource
                ErrClear();
            }
            // code size is message offset value plus two
            mCodeSize = ReadWord(0) + 2;
            mIsDirty = false;
            mSourceDirty = false;
        }

        /// <summary>
        /// Loads this logic resource by reading its data from the VOL file. Only
        /// applies to logics in a game. Non-game logics loaded when they are instantiated
        /// and remain loaded until disposed.
        /// </summary>
        public override void Load() {
            if (mLoaded) {
                return;
            }
            base.Load();
            if (ErrLevel < 0) {
                ErrClear();
            }
            mSourceDirty = false;
            // get code size (add 2 to msgstart offset)
            mCodeSize = ReadWord(0) + 2;
            // load the sourcetext
            LoadSource();
            // force uncompilied if error
            if (ErrLevel < 0) {
                //
            }
        }

        /// <summary>
        /// Unloads this logic resource. Data elements are undefined and non-accessible
        /// while unloaded. Only logics that are in a game can be unloaded.
        /// </summary>
        public override void Unload() {
            if (!mInGame) {
                return;
            }
            base.Unload();
            mSourceDirty = false;
            mSourceText = "";
        }

        /// <summary>
        /// Initializes a new logic resource when first instantiated. If NewLogic is null, 
        /// a blank logic resource is created. If NewLogic is not null, it is cloned into
        /// the new picture.
        /// </summary>
        /// <param name="NewLogic"></param>
        private void InitLogic(Logic NewLogic = null) {
            if (NewLogic is null) {
                // set default resource data by clearing
                Clear();
                // set default source
                mSourceText = "return();" + NEWLINE;
                // to avoid having compile property read true if both values are 0,
                // set compiledCRC to -1 on initialization
                CompiledCRC = 0xffffffff;
                CRC = 0;
            }
            else {
                // copy base properties
                NewLogic.CloneTo(this);
                // copy logic properties
                mIsRoom = NewLogic.mIsRoom;
                mLoaded = NewLogic.mLoaded;
                mCompiledCRC = NewLogic.mCompiledCRC;
                mCRC = NewLogic.mCRC;
                mSourceText = NewLogic.mSourceText;
                mSourceDirty = NewLogic.mSourceDirty;
                mSourceFile = NewLogic.mSourceFile;
            }
        }

        /// <summary>
        /// Creates an exact copy of this Logic resource.
        /// </summary>
        /// <returns>The Logic resource this method creates.</returns>
        public Logic Clone() {
            Logic CopyLogic = new();
            // copy base properties
            base.CloneTo(CopyLogic);
            // add WinAGI items
            CopyLogic.mIsRoom = mIsRoom;
            CopyLogic.mLoaded = mLoaded;
            CopyLogic.mCompiledCRC = mCompiledCRC;
            CopyLogic.mCRC = mCRC;
            CopyLogic.mSourceText = mSourceText;
            CopyLogic.mSourceDirty = mSourceDirty;
            CopyLogic.mSourceFile = mSourceFile;
            return CopyLogic;
        }

        /// <summary>
        /// Clears out source code text and clears the resource data.
        /// </summary>
        public override void Clear() {
            WinAGIException.ThrowIfNotLoaded(this);
            base.Clear();
            // set default resource data
            mData = [0x01, 0x00, 0x00, 0x00, 0x02, 0x00];
            // byte0 = low byte of msg section offset (relative to byte 2)
            // byte1 = high byte of msg section offset
            // byte2 = first byte of code data (a single return)
            // byte3 = first byte of msg section = # of messages
            // byte4 = high byte of msg end offset
            // byte5 = low byte of msg end offset

            // clear the source code by setting it to 'return' command
            mSourceText = "return();" + NEWLINE;
            if (mInGame) {
                // reset crcs
                mCRC = CRC32(Encoding.GetEncoding(437).GetBytes(mSourceText));
                mCompiledCRC = mCRC;
            }
            mSourceDirty = true;
            mCodeSize = 3;
        }

        /// <summary>
        /// Exports a compiled resource; ingame only (since only ingame logics can be compiled)
        /// </summary>
        /// <param name="ExportFile"></param>
        public void ExportSource(string ExportFile) {
            WinAGIException.ThrowIfNotLoaded(this);
            try {
                File.WriteAllText(ExportFile, mSourceText);
            }
            catch (Exception) {
                WinAGIException wex = new(LoadResString(582)) {
                    HResult = WINAGI_ERR + 582
                };
                throw wex;
            }
        }

        /// <summary>
        /// Imports a logic from a file into this resource. This will overwrite current source
        /// text with decompiled resource. Use ImportSource to import source code directly.
        /// </summary>
        /// <param name="ImportFile"></param>
        /// <param name="AsSource"></param>
        public override void Import(string ImportFile) {
            try {
                base.Import(ImportFile);
                // load the source code by decompiling
                LoadSource(true);
            }
            catch (Exception) {
                throw;
            }
            mSourceDirty = false;
        }

        /// <summary>
        /// Imports a an existing source code file into this logic resource. It does not 
        /// compile the logic so the resource data remains unchanged.
        /// </summary>
        /// <param name="ImportFile"></param>
        public void ImportSource(string ImportFile) {
            if (ImportFile.Length == 0) {
                WinAGIException wex = new(LoadResString(615)) {
                    HResult = WINAGI_ERR + 615
                };
                throw wex;
            }
            if (!mInGame) {
                // change file name to match the import file
                mSourceFile = ImportFile;
            }
            LoadSource();
            // set ID to the filename without extension;
            // the calling function will take care or reassigning it later, if needed
            // (for example, if the new logic will be added to a game)
            mResID = Path.GetFileNameWithoutExtension(ImportFile);
            if (mResID.Length > 64) {
                mResID = Left(mResID, 64);
            }
            if (NotUniqueID(this)) {
                // create one
                int i = 0;
                string baseid = mResID;
                do {
                    mResID = baseid + "_" + i++.ToString();
                }
                while (NotUniqueID(this));
            }
            // reset dirty flag
            mSourceDirty = false;
        }

        /// <summary>
        /// Loads the source code text file for this logic. If logic is not
        /// in a game, calling code must make sure filename is set before 
        /// calling this method.
        /// </summary>
        /// <param name="Decompile"></param>
        internal void LoadSource(bool Decompile = false) {
            uint tmpCRC;

            if (mInGame) {
                // check that file exists; if not, look for alternate filename
                // formats, and move/rename as needed
                if (!File.Exists(mSourceFile)) {
                    if (File.Exists(parent.agResDir + "logic" + Number + ".txt")) {
                        File.Move(parent.agResDir + "logic" + Number + ".txt", mSourceFile);
                    }
                    else if (File.Exists(parent.agResDir + mResID + ".txt")) {
                        File.Move(parent.agResDir + mResID + ".txt", mSourceFile);
                    }
                    else {
                        if (agSierraSyntax) {
                            if (File.Exists(parent.agResDir + "RM" + Number + ".cg")) {
                                File.Move(parent.agResDir + "RM" + Number + ".cg", mSourceFile);
                            }
                            else {
                                // default file does not exist
                                // AGI Studio file does not exist
                                // CG file does not exist
                                // force decompile
                                Decompile = true;
                            }
                        }
                        else {
                            // default file does not exist
                            // AGI Studio file does not exist
                            // force decompile
                            Decompile = true;
                        }
                    }
                }
            }
            // if forcing decompile
            if (Decompile) {
                if (ErrLevel == 0) {
                    // get source code by decoding the resource raw data
                    // (this also set error level)
                    mSourceText = LogicDecoder.DecodeLogic(this);
                    if (ErrLevel != 0) {
                        // unable to decompile; force uncompiled state
                        mCRC = 0;
                        mCompiledCRC = 0xffffffff;
                        return;
                    }
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
                if (!File.Exists(mSourceFile)) {
                    mSrcErrLevel = -1;
                    ErrData[0] = mSourceText;
                    ErrData[1] = mResID;
                    mSourceText = "return();" + NEWLINE;
                    // force uncompile state
                    mCRC = 0;
                    mCompiledCRC = 0xffffffff;
                    return;
                }
                // check for readonly
                if ((File.GetAttributes(mSourceFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                    mSrcErrLevel = -2;
                    ErrData[0] = mSourceText;
                    ErrData[1] = mResID;
                    mSourceText = "return();" + NEWLINE;
                    // force uncompile state
                    mCRC = 0;
                    mCompiledCRC = 0xffffffff;
                    return;
                }
                try {
                    mSourceText = File.ReadAllText(mSourceFile);
                }
                catch (Exception e) {
                    mSrcErrLevel = -3;
                    ErrData[0] = e.Message;
                    ErrData[1] = mResID;
                    mSourceText = "return();" + NEWLINE;
                    // force uncompile state
                    mCRC = 0;
                    mCompiledCRC = 0xffffffff;
                    return;
                }
            }
            // replace tabs with indent spaces
            if (mSourceText.Contains('\t')) {
                mSourceText = mSourceText.Replace("\t", LogicDecoder.INDENT);
            }
            // calculate source crc
            if (mInGame) {
                tmpCRC = CRC32(parent.agCodePage.GetBytes(mSourceText));
                if (mCRC != tmpCRC) {
                    // update crc
                    mCRC = tmpCRC;
                    parent.WriteGameSetting("Logic" + Number, "CRC32", "0x" + mCRC.ToString("x8"), "Logics");
                }
                // if decompiling, also save the source and the compiled crc
                if (Decompile) {
                    SaveSource();
                    mCompiledCRC = tmpCRC;
                    parent.WriteGameSetting("Logic" + Number, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
                }
            }
            else {
                // update crc
                mCRC = CRC32(Encoding.GetEncoding(437).GetBytes(mSourceText));
            }
            mSourceDirty = false;
        }

        /// <summary>
        /// Saves the source code text for this logic to a file. If in a game,
        /// target file is predefined as the logic's sourcefile. If not in a game
        /// the target file can be specified as a parameter. If blank, the
        /// current sourcefile name is assumed.
        /// </summary>
        /// <param name="SaveFile"></param>
        public void SaveSource(string SaveFile = "") {
            if (SaveFile.Length == 0) {
                //if in a game
                if (mInGame) {
                    //filename is predfined
                    SaveFile = SourceFile;
                }
                else {
                    SaveFile = mSourceFile;
                    if (SaveFile.Length == 0) {
                        WinAGIException wex = new(LoadResString(599)) {
                            HResult = WINAGI_ERR + 599
                        };
                        throw wex;
                    }
                }
            }
            try {
                File.WriteAllText(SaveFile, mSourceText);
            }
            catch (Exception) {
                WinAGIException wex = new(LoadResString(582)) {
                    HResult = WINAGI_ERR + 582
                };
                throw wex;
            }
            mSourceDirty = false;
            if (InGame) {
                parent.WriteGameSetting("Logic" + Number, "CRC32", "0x" + mCRC.ToString("x8"), "Logics");
                parent.agLastEdit = DateTime.Now;
            }
            else {
                mResID = Path.GetFileName(SaveFile);
            }
        }
        
        /// <summary>
        /// Compiles the source code for this logic and saves the resource. Only logics
        /// in a game can be compiled.
        /// </summary>
        public void Compile() {
            // TODO: currently nongame logics can't be compiled; need to refactor to allow it
            TWinAGIEventInfo tmpInfo;

            WinAGIException.ThrowIfNotLoaded(this);
            if (!mInGame) {
                WinAGIException wex = new(LoadResString(618)) {
                    HResult = WINAGI_ERR + 618
                };
                throw wex;
            }
            if (mSourceText.Length == 0) {
                WinAGIException wex = new(LoadResString(546)) {
                    HResult = WINAGI_ERR + 546
                };
                throw wex;
            }
            try {
                tmpInfo = CompileLogic(this);
                // set dirty flag (forces save to work correctly)
                mIsDirty = true;
                base.Save();
            }
            catch (Exception) {
                // force uncompiled state
                mCompiledCRC = 0xffffffff;
                compGame.WriteGameSetting("Logic" + Number, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
                // pass it along
                throw;
            }
            // if check for compiler error (ignore warnings)
            if (tmpInfo.Type == EventType.etError) {
                // force uncompiled state
                mCompiledCRC = 0xffffffff;
                compGame.WriteGameSetting("Logic" + Number, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
            }
        }

        /// <summary>
        /// Saves properties of this logic to the game's WAG file.
        /// </summary>
        public void SaveProps() {
            if (mInGame) {
                string strSection = "Logic" + Number;
                parent.WriteGameSetting(strSection, "ID", ID, "Logics");
                parent.WriteGameSetting(strSection, "Description", Description);
                parent.WriteGameSetting(strSection, "CRC32", "0x" + mCRC.ToString("x8"));
                parent.WriteGameSetting(strSection, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
                parent.WriteGameSetting(strSection, "IsRoom", mIsRoom.ToString());
                PropsDirty = false;
            }
        }

        /// <summary>
        /// If INGAME: <br/>
        /// Same as SaveSource. To update an igame logic resource, use the 
        /// Compile method.<br />
        /// If NOT INGAME:<br />
        /// Saves this logic resource to its resource file specified by FileName.
        /// This does NOT save the source file; use SaveSource to do that.
        /// </summary>
        /// <param name="SaveFile"></param>
        public new void Save() {
            WinAGIException.ThrowIfNotLoaded(this);
            if (PropsDirty && mInGame) {
                SaveProps();
            }
            if (mInGame) {
                SaveSource();
            }
            else {
                if (mIsDirty) {
                    try {
                        base.Save();
                    }
                    catch {
                        throw;
                    }
                }
            }
        }
        #endregion
    }
}