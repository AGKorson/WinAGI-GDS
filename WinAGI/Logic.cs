using System;
using System.ComponentModel;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.Compiler;
using static WinAGI.Common.Base;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;
using Microsoft.VisualBasic.Logging;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.VisualStudio.TextManager.Interop;

namespace WinAGI.Engine
{
    public class Logic : AGIResource
    {
        //source code properties
        string mSourceFile;
        string mSourceText;
        uint mCompiledCRC;
        uint mCRC;
        bool mSourceDirty;
        bool mIsRoom;
        //int mCodeSize;

        internal void LoadNoSource() {
            // used by extractresources function to load
            // logic data without loading the sourcecode
            // can skip the error checks

            // load the base resource data
            base.Load();
            //set code size (add 2 to msgstart offset)
            CodeSize = ReadWord(0) + 2;
            //clear dirty flag
            IsDirty = false;
            mSourceDirty = false;
        }

        public override void Load()
        {
            //if not ingame, the resource should already be loaded
            if (!mInGame) {
                if (!mLoaded) {
                    throw new Exception("non-game resources should always be loaded");
                }
            }
            // if already loaded, just exit
            if (mLoaded) {
                return;
            }
            try {
                // load the base resource data
                base.Load();
            }
            catch (Exception) {
                // pass it along
                throw;
            }
            //set code size (add 2 to msgstart offset)
            CodeSize = ReadWord(0) + 2;

            try {
                //load the sourcetext
                LoadSource();
            }
            catch (Exception) {
                // pass it along
                throw;
            }

            //compiledCRC Value should already be set,
            //and source crc gets calculated when source is loaded

            //clear dirty flag
            IsDirty = false;
            mSourceDirty = false;
        }
        public override void Unload()
        {
            base.Unload();
            mSourceDirty = false;
            mSourceText = "";
        }
        public Logic() : base(AGIResType.rtLogic)
        {
            //initialize a nongame logic
            mResID = "NewLogic";
            //attach events
            base.PropertyChanged += ResPropChange;
            strErrSource = "WinAGI.Logic";
            //set default resource data
            // TODO: confirm correct empty logic data block; fix Clear to match
            mRData.AllData = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x02, 0x00 };
            // byte0 = low byte of msg section offset (relative to byte 2)
            // byte1 = high byte of msg section offset
            // byte2 = first byte of code data (a single return)
            // byte3 = first byte of msg section = # of messages
            // byte4 = high byte of msg end offset
            // byte5 = low byte of msg end offset

            // set default source
            mSourceText = ActionCommands[0].Name + "();" + NEWLINE + NEWLINE + "[ messages" + NEWLINE;
            //to avoid having compile property read true if both values are 0, set compiled to -1 on initialization
            CompiledCRC = 0xffffffff;
            CRC = 0;
        }
        public Logic(AGIGame parent, byte ResNum, sbyte VOL, int Loc) : base(AGIResType.rtLogic)
        {
            //adds this resource to a game, setting its resource 
            //location properties, and reads properties from the wag file

            //attach events
            base.PropertyChanged += ResPropChange;
            strErrSource = "WinAGI.Logic";

            //set up base resource
            base.InitInGame(parent, ResNum, VOL, Loc);

            //if importing, there will be nothing in the propertyfile
            mResID = parent.agGameProps.GetSetting("Logic" + ResNum, "ID", "", true);
            if (mResID.Length == 0) {
                //no properties to load; save default ID
                mResID = "Logic" + ResNum;
                parent.WriteGameSetting("Logic" + ResNum, "ID", ID, "Logics");
                //save CRC and CompCRC values as defaults; they//ll be adjusted first time logic is accessed
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

            //if res is zero
            if (ResNum == 0) {
                //make sure isroom flag is false
                mIsRoom = false;
            }
        }
        public Logic Clone()
        {
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
            if (!mInGame) {
                CopyLogic.mSourceFile = mSourceFile;
            }
            return CopyLogic;
        }
        public uint CompiledCRC
        {
            get
            {
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
        public bool IsRoom
        {
            get { return mIsRoom; }
            internal set
            {
                //if in a game, and logic 0
                if (mInGame && (Number == 0)) {
                    throw new Exception("450, Can't assign to read-only property");
                }

                //if changing
                if (mIsRoom != value) {
                    mIsRoom = value;
                    parent.WriteGameSetting("Logic" + Number, "IsRoom", mIsRoom.ToString(), "Logics");
                }
            }
        }
        public bool SourceDirty
        {
            get { return mSourceDirty; }
        }
        public string SourceFile
        {
            get
            {
                //if in a game,
                if (mInGame) {
                    //sourcefile is predefined
                    return parent.agResDir + mResID + agSrcFileExt;
                }
                else {
                    return mSourceFile;
                }
            }
            set
            {
                //if in a game,
                if (mInGame) {
                    //sourcefile is predefined; raise error
                    throw new Exception("450, Can't assign to read-only property");
                }
                else {
                    mSourceFile = value;
                }
            }
        }
        public uint CRC { get; internal set; }
        public int CodeSize { get; internal set; }
        private void ResPropChange(object sender, AGIResPropChangedEventArgs e)
        {
            ////let's do a test
            //// increment number everytime data changes
            //Number++;
        }
        private void SaveProps()
        {
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
        public bool Compiled
        {
            get
            {
                // return true if source code CRC is same as stored compiled crc
                // if not in a game, compiled is normally undefined, but in case
                // of a Logic copied from an InGame resource, this property will work

                //if crc is 0, means crc not loaded from a file, or calculated yet;  OR ERROR!!!
                if (mCRC == 0) {
                    // load the sourcetext to get the crc (also sets compiledcrc if
                    // no sourcefile exists)
                    if (!Loaded) {
                        Load();
                        try {
                            LoadSource();
                        }
                        catch (Exception) {
                            //presumably due to decompiling error;
                            //return value of false?
                            ////Debug.Throw exception
                            Unload();
                            return false;
                        }
                        Unload();
                        //done with sourcetext
                        mSourceText = "";
                    }
                }
                return (mCRC == mCompiledCRC);
            }
            private set { }
        }
        public override void Clear()
        {
            //clears out source code text
            //and clears the resource data
            if (InGame) {
                if (!Loaded) {
                    //nothing to clear
                    Exception e = new(LoadResString(563))
                    {
                        HResult = WINAGI_ERR + 563
                    };
                    throw e;
                }
            }
            //clear resource
            base.Clear();
            //set default resource data
            Data = new RData(6)
            {
                AllData = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x02, 0x00 }
            };

            //clear the source code by setting it to //return// command
            mSourceText = ActionCommands[0].Name + "();" + NEWLINE + NEWLINE + "[ messages" + NEWLINE;
            if (InGame) {
                //reset crcs
                mCRC = CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(mSourceText));
                mCompiledCRC = mCRC;
            }
            //note change by marking source as dirty
            mSourceDirty = true;
            //TODO: for a completely empty logic, is it two? or three? 
            CodeSize = 3;
        }
        public void Export(string ExportFile, bool ResetDirty)
        {
            //exports a compiled resource; ingame only
            //(since only ingame logics can be compiled)

            //if not in a game,
            if (!InGame) {
                //not allowed; nothing to export

                Exception e = new(LoadResString(668))
                {
                    HResult = WINAGI_ERR + 668
                };
                throw e;
            }
            //if not loaded
            if (!Loaded) {
                //error

                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            //export a logic resource file
            base.Export(ExportFile);
        }
        public void Import(string ImportFile, bool AsSource)
        {
            //imports a logic resource
            //i.e., opens from a standalone file
            //doesn't matter if ingame or not
            //if importing a logic resource, it will overwrite current source text with decompiled source

            //clear existing resource
            Clear();
            //if importing source code
            if (AsSource) {
                //load the source code
                if (!mInGame) {
                    mSourceFile = ImportFile;
                }
                LoadSource(false);
            }
            else {
                try {
                    //import the compiled resource
                    Import(ImportFile);
                    //load the source code by decompiling
                    LoadSource(true);
                    if (!mInGame) {
                        //force filename to null
                        mSourceFile = "";
                    }
                }
                catch (Exception e) {
                    Unload();
                    throw new Exception(e.Message);
                }
            }
            //set ID to the filename without extension;
            //the calling function will take care or reassigning it later, if needed
            //(for example, if the new logic will be added to a game)
            ID = FileNameNoExt(ImportFile);
            //reset dirty flags
            IsDirty = false;
            mSourceDirty = false;
        }
        public override string ID
        {
            get => base.ID;
            internal set
            {
                base.ID = value;
                if (mInGame) {
                    //source file always tracks ID for ingame resources
                    mSourceFile = parent.agResDir + base.ID + agSrcFileExt;
                    parent.WriteGameSetting("Logic" + Number, "ID", mResID, "Logics");
                }
            }
        }
        internal void LoadSource(bool Decompile = false)
        {
            //loads LoadFile as the source code
            //for not-ingame, calling code must first set sourcefile name
            string strInput, LoadFile;
            uint tmpCRC;

            //if in a game,
            if (mInGame) {
                //load file is predefined
                LoadFile = parent.agResDir + mResID + agSrcFileExt;
               // mSourceFile = LoadFile;

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
                LoadFile = mSourceFile;
            }

            //if forcing decompile
            if (Decompile) {
                //get source code by decoding the resource, decrypting messages if not v3compressed
                strInput = DecodeLogic(this, mInGame ? Number : -1);

                //make sure decompile flag is set (so crc can be saved)
                Decompile = true;
                //if in a game, always save this newly created source
            }
            else {
                //FileStream fsLogic;
                try {
                    //load sourcecode from file
                    strInput = File.ReadAllText(LoadFile, parent.agCodePage);
                    //fsLogic = new FileStream(LoadFile, FileMode.Open);
                    //StreamReader srLogic = new(fsLogic, Encoding.GetEncoding(437));
                    //strInput = srLogic.ReadToEnd();
                    //srLogic.Dispose();
                    //fsLogic.Dispose();
                }
                catch (Exception e) {

                    Exception eR = new(LoadResString(502).Replace(ARG1, LoadFile).Replace(ARG2, e.Message))
                    {
                        HResult = WINAGI_ERR + 502
                    };
                    throw eR;
                }
            }
            //send text in source code string list
            
            mSourceText = strInput;
            // replace tabs with indent spaces
            if (mSourceText.Contains('\t')) {

                mSourceText = mSourceText.Replace("\t", INDENT);
            }
            //calculate source crc
            tmpCRC = CRC32(parent.agCodePage.GetBytes(strInput));
            if (mInGame) {
                if (mCRC != tmpCRC) {
                    parent.WriteGameSetting("Logic" + Number, "CRC32", "0x" + tmpCRC.ToString("x8"), "Logics");
                }
                //if decompiling, also save the source and the compiled crc
                //(it's the same as the source crc!)
                if (Decompile) {
                    SaveSource();
                    mCompiledCRC = tmpCRC;
                    parent.WriteGameSetting("Logic" + Number, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
                }
            }
            // update crc
            mCRC = tmpCRC;
            //set loaded flag and dirty status
            mIsDirty = false;
            mSourceDirty = false;
        }
        public string SourceText
        {
            get
            {
                // ********** SourceText is UNICODE, not CP byte code
                // it gets converted to CP byte code when it gets compiled
                return mSourceText;
            }
            set
            {
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
        public void SaveSource(string SaveFile = "", bool Exporting = false)
        {
            //saves the source code for this logic to file
            string strTempFile;
            bool blnUnload;
            //no filename needed for ingame logics,
            //but must have filename if NOT ingame

            //if Exporting flag is true, we are just saving the file; don't do any other
            //maintenance on the logic resource

            //if not loaded, then load it
            blnUnload = !mLoaded;
            if (blnUnload) {
                Load();
            }

            if (SaveFile.Length == 0) {
                //if in a game
                if (mInGame) {
                    //filename is predfined
                    SaveFile = mSourceFile;
                }
                else {
                    //raise an error
                    if (blnUnload) {
                        Unload();
                    }
                    Exception e = new(LoadResString(599))
                    {
                        HResult = WINAGI_ERR + 599
                    };
                    throw e;
                }
            }
            try {
                //get temporary file
                strTempFile = Path.GetTempFileName();
                // need to replace single CR with CRLF (but first
                // check if the control already has the correct end-of-line marks
                //mSourceText;

                // TODO: need strategy to deal with end-of-line; in files
                // allow whatver is local environment setting; when doing
                // CRC or compiling, only \r

                //if (!mSourceText.Contains("\r\n")) {
                //    mSourceText = mSourceText.Replace("\r", "\r\n");
                //}

                File.WriteAllBytes(strTempFile, parent.agCodePage.GetBytes(mSourceText));
                // TODO: replace all text file handling to use simpler
                // functions that incorporate codepage

                ////open file for output
                //FileStream fsOut = new FileStream(strTempFile, FileMode.Open);
                ////write text to file (conveting characters to cp437 values)
                //bytText = parent.agCodePage.GetBytes(mSourceText);
                //fsOut.Write(bytText, 0, bytText.Length);
                ////close file,
                //fsOut.Dispose();
            }
            catch (Exception) {
                if (blnUnload) {
                    Unload();
                }
                Exception e = new(LoadResString(582))
                {
                    HResult = WINAGI_ERR + 582
                };
                throw e;
            }
            try {
                //if savefile exists
                if (File.Exists(SaveFile)) {
                    //delete it
                    File.Delete(SaveFile);
                }
                //copy tempfile to savefile
                File.Move(strTempFile, SaveFile);
            }
            catch (Exception) {
                Exception e = new(LoadResString(663))
                {
                    HResult = WINAGI_ERR + 663
                };
                throw e;
            }
            //if exporting, nothing left to do
            if (Exporting) {
                if (blnUnload) {
                    Unload();
                }
                return;
            }
            //reset source dirty flag
            mSourceDirty = false;
            //if in a game, update the source crc
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
            //if unloaded when call to this function was made
            if (blnUnload) {
                Unload();
            }
        }
        public void Compile()
        {
            //compiles the source code for this logic
            //and saves the resource
            TWinAGIEventInfo tmpInfo = new()
            {
                ID = "",
                Module = "",
                Text = ""
            };


            //if not in a game
            if (!mInGame) {
                Exception e = new(LoadResString(618))
                {
                    HResult = WINAGI_ERR + 618
                };
                throw e;
            }
            //if not loaded, raise error
            if (!mLoaded) {
                Exception e = new(LoadResString(563))
                {
                    HResult = WINAGI_ERR + 563
                };
                throw e;
            }
            //if no data in sourcecode
            if (mSourceText.Length == 0) {
                Exception e = new(LoadResString(546))
                {
                    HResult = WINAGI_ERR + 546
                };
                throw e;
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
            if (tmpInfo.Type != EventType.ecCompOK) {
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
        public new void Save(string SaveFile = "")
        {
            //this file saves the logic resource to next available VOL file

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
                        base.Save(SaveFile);
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
            IsDirty = false;
        }
        public override bool IsDirty
        {
            get
            {
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