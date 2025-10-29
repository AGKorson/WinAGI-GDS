using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
namespace WinAGI.Engine {
    /// <summary>
    /// Represents a list of AGI defines that can be formatted as an
    /// include file.
    /// </summary>
    public class GlobalList {
        // TODO: replace global list with a 'define list' object that
        // lets users edit all defines files in the same editor, but
        // now all files must be manually included (no more auto-include
        // globals.txt)
        // create a scope property allowing files to be tagged 'global'
        // (which will automatically add them to existing/new logics)
        // or identify which logics they apply to; add a branch to
        // resource tree to list define files

        #region Local Members
        private readonly AGIGame parent;
        private string mResFile = "";
        private bool mIsChanged = false;
        private bool mInGame = false;
        private bool mLoaded = false;
        private List<TDefine> agGlobal = new();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes this defines list by assigning it to the specified AGI game.
        /// </summary>
        /// <param name="parent"></param>
        public GlobalList(AGIGame parent) {
            this.parent = parent;
            mInGame = true;
            mIsChanged = false;
            mResFile = parent.agResDir + '\\' + "globals.txt";
            if (!File.Exists(mResFile)) {
                using FileStream fs = File.Create(mResFile);
                string hdr =  "[\n";
                hdr += "[ global defines file for " + parent.GameID + "\n";
                hdr += "[\n";
                fs.Write(Encoding.Default.GetBytes(hdr));
                fs.Close();
            }
        }

        public GlobalList(string filename) {
            parent = null;
            mInGame = false;
            mIsChanged = false;
            mResFile = filename;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the define object at the specified location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TDefine this[int index] {
            get {
                return agGlobal[index];
            }
            set {
                agGlobal[index] = value;
                mIsChanged = true;
            }
        }

        /// <summary>
        /// Gets the number of defines in this defines list.
        /// </summary>
        public int Count {
            get { return agGlobal.Count; }
        }

        /// <summary>
        /// Gets the change state of this defines list. Returns true if the
        /// list has not been changed.
        /// </summary>
        public bool IsChanged {
            get {
                return mIsChanged;
            }
            internal set {
                mIsChanged = value;
            }
        }

        public bool InGame {
            get {
                return mInGame;
            }
            internal set {
                mInGame = value;
            }
        }

        public ResourceErrorType Error { get; internal set; } = ResourceErrorType.NoError;

        public string[] ErrData { get; internal set; } = ["", "", "", "", "", ""];

        public int Warnings { get; internal set; } = 0;

        public string[] WarnData { get; internal set; } = ["", "", "", "", "", ""];


        public bool Loaded {
            get => mLoaded;
        }

        #endregion

        #region Methods
        public void Load(string LoadFile = "") {
            if (mLoaded) {
                return;
            }
            if (mInGame) {
                LoadFile = mResFile;
            }
            // always set loaded flag regardless of error status
            mLoaded = true;
            mIsChanged = false;
            (Error, Warnings) = LoadGlobalDefines(mResFile);
            if (!mInGame) {
                mResFile = LoadFile;
            }
            mIsChanged = false;
        }

        public void Unload() {
            if (!mLoaded) {
                return;
            }
            agGlobal = [];
            mLoaded = false;
            mIsChanged = false;
        }

        /// <summary>
        /// Loads the specified defines file, creating the defines list.
        /// </summary>
        private (ResourceErrorType, int) LoadGlobalDefines(string definefile) {
            FileStream fsDefines = null;
            StreamReader srDefines = null;
            string strLine;
            TDefine tdNewDefine = new();
            ResourceErrorType reterr = ResourceErrorType.NoError;
            int retwarn = 0;

            agGlobal = [];
            // verify file exists
            if (!File.Exists(definefile)) {
                return (ResourceErrorType.GlobalsNoFile, 0);
            }
            // check for readonly
            if ((File.GetAttributes(definefile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                reterr = ResourceErrorType.GlobalsIsReadOnly;
            }

            try {
                fsDefines = new FileStream(definefile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (Exception ex) {
                ErrData[0] = ex.Message;
                return (ResourceErrorType.GlobalsAccessError, 0);
            }
            // if nothing to load, just return
            if (fsDefines.Length == 0) {
                return (reterr, 0);
            }
            try {
                // grab the file data
                srDefines = new StreamReader(fsDefines);
                while (true) {
                    strLine = srDefines.ReadLine();
                    if (strLine is null)
                        break;
                    strLine = strLine.Replace('\t', ' ');
                    // strip off comment
                    string s = "";
                    strLine = StripComments(strLine, ref s);
                    if (strLine.Length != 0) {
                        if (strLine.Left(8) == "#define ") {
                            strLine = strLine[8..].Trim();
                            int i = strLine.IndexOf(' ');
                            if (i != -1) {
                                tdNewDefine.Default = tdNewDefine.Name = strLine[..i].Trim();
                                tdNewDefine.Value = strLine[i..].Trim();
                                DefineNameCheck chkName = ValidateGlobalName(tdNewDefine);
                                switch (chkName) {
                                case DefineNameCheck.OK or
                                ReservedVar or
                                ReservedFlag or
                                ReservedNum or
                                ReservedObj or
                                ReservedStr or
                                ReservedMsg:
                                    DefineValueCheck chkValue = FanLogicCompiler.ValidateDefineValue(ref tdNewDefine, null);
                                    switch (chkValue) {
                                    case DefineValueCheck.OK or Reserved or DefineValueCheck.Global:
                                        agGlobal.Add(tdNewDefine);
                                        break;
                                    default:
                                        // DefineValueCheck.Empty
                                        // OutofBounds
                                        // BadArgNumber
                                        // NotAValue
                                        retwarn = 1;
                                        break;
                                    }
                                    break;
                                default:
                                    // DefineNameCheck.Empty
                                    // Numeric
                                    // ActionCommand
                                    // TestCommand
                                    // KeyWord
                                    // ArgMarker
                                    // BadChar
                                    // DefineNameCheck.Global
                                    // ReservedGameInfo
                                    // ResourceID
                                    retwarn = 1;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch {
                srDefines.Dispose();
                fsDefines.Dispose();
                return (ResourceErrorType.GlobalsAccessError, 0);
            }
            srDefines.Dispose();
            fsDefines.Dispose();
            return (reterr, retwarn);
        }

        public void SetDefines(List<TDefine> definelist) {
            agGlobal = definelist;
            mIsChanged = true;
        }

        public void Clear() {
            agGlobal = [];
            mIsChanged = true;
        }

        public DefineNameCheck ValidateGlobalName(TDefine CheckDef, int index) {
            // if already at default, just exit
            if (CheckDef.Name.Length > 0 && CheckDef.Name == CheckDef.Default) {
                return DefineNameCheck.OK;
            }
            // basic checks
            bool sierrasyntax = parent is not null && parent.SierraSyntax;
            DefineNameCheck retval = BaseNameCheck(CheckDef.Name, sierrasyntax);
            if (retval != DefineNameCheck.OK) {
                return retval;
            }
            // order: locals>globals>resids>reserved

            if (parent is not null) {
                // resourceids
                foreach (Logic logic in parent.Logics) {
                    if (CheckDef.Name == logic.ID) {
                        return DefineNameCheck.ResourceID;
                    }
                }
                foreach (Picture picture in parent.Pictures) {
                    if (CheckDef.Name == picture.ID) {
                        return DefineNameCheck.ResourceID;
                    }
                }
                foreach (Sound sound in parent.Sounds) {
                    if (CheckDef.Name == sound.ID) {
                        return DefineNameCheck.ResourceID;
                    }
                }
                foreach (Engine.View view in parent.Views) {
                    if (CheckDef.Name == view.ID) {
                        return DefineNameCheck.ResourceID;
                    }
                }
                // reserved defines
                for (int i = 0; i < 10; i++) {
                    TDefine[] tmpDefines = parent.agReservedDefines.ByGroup((ResDefGroup)i);
                    for (int j = 0; j < tmpDefines.Length; j++) {
                        if (CheckDef.Name == tmpDefines[j].Name) {
                            switch ((ResDefGroup)i) {
                            case ResDefGroup.Variable:
                                return DefineNameCheck.ReservedVar;
                            case ResDefGroup.Flag:
                                return DefineNameCheck.ReservedFlag;
                            case ResDefGroup.EdgeCode:
                            case ResDefGroup.ObjectDir:
                            case ResDefGroup.VideoMode:
                            case ResDefGroup.ComputerType:
                            case ResDefGroup.Color:
                                return DefineNameCheck.ReservedNum;
                            case ResDefGroup.Object:
                                return DefineNameCheck.ReservedObj;
                            case ResDefGroup.String:
                                return DefineNameCheck.ReservedStr;
                            case ResDefGroup.GameInfo:
                                return DefineNameCheck.ReservedGameInfo;
                            }
                        }
                    }
                }
            }
            // check globals already in this list
            if (index >= 0) {
                for (int i = 0; i < agGlobal.Count; i++) {
                    if (i == index) {
                        continue;
                    }
                    if (CheckDef.Name == agGlobal[i].Name) {
                        return DefineNameCheck.Global;
                    }
                }
            }
            // if no error conditions, it's OK
            return DefineNameCheck.OK;
        }

        public DefineNameCheck ValidateGlobalName(TDefine CheckDef) {
            return ValidateGlobalName(CheckDef, -1);
        }

        public void Save() {
            // save list of globals

            List<string> stlGlobals = BuildGlobalsFile();
            try {
                File.WriteAllLines(mResFile, stlGlobals);
                mIsChanged = false;
            }
            catch {
                throw;
            }
        }

        private List<string> BuildGlobalsFile() {
            // determine longest name length to facilitate aligning values
            int lngMaxLen = 0;
            int lngMaxV = 0;
            for (int i = 0; i < agGlobal.Count; i++) {
                if (agGlobal[i].Name.Length > lngMaxLen) {
                    lngMaxLen = agGlobal[i].Name.Length;
                }
                // right-align non-strings
                if (agGlobal[i].Value[0] != '"') {
                    if (agGlobal[i].Value.Length > lngMaxV) {
                        lngMaxV = agGlobal[i].Value.Length;
                    }
                }
            }
            List<string> tmpStrList = [];
            // add a useful header
            tmpStrList.Add("[");
            if (parent is null) {
                tmpStrList.Add("[ global defines file " + Path.GetFileName(mResFile));
            }
            else {
                tmpStrList.Add("[ global defines file for " + parent.GameID);
            }
            tmpStrList.Add("[");
            for (int i = 0; i < agGlobal.Count; i++) {
                // get name and Value
                string strName = agGlobal[i].Name.PadRight(lngMaxLen);
                string strValue = agGlobal[i].Value.PadLeft(4);
                // right align non-strings
                if (strValue[0] != '"') {
                    strValue = strValue.PadLeft(lngMaxV);
                }
                string strComment = agGlobal[i].Comment;
                if (strComment.Length > 0) {
                    strComment = " " + strComment;
                }
                tmpStrList.Add("#define " + strName + "  " + strValue + strComment);
            }
            return tmpStrList;
        }

        public void Add(string name, string value, string comment, ArgType type) {
            TDefine adddef = new() {
                Name = name,
                Default = name,
                Value = value,
                Comment = comment,
                Type = type
            };
            agGlobal.Add(adddef);
        }

        #endregion
    }
}
