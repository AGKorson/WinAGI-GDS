using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
namespace WinAGI.Engine {
    /// <summary>
    /// Represents a list of AGI defines that can be formatted as an
    /// include file. Applies only to games using fan syntax
    /// </summary>
    public class DefinesList {

        #region Local Members
        private readonly AGIGame parentGame;
        private readonly DefinesList parentList = null;
        private string mResFile = "";
        private bool mIsChanged = false;
        private bool mInGame = false;
        private Dictionary<string, TDefine> agDefines = [];
        #endregion

        #region Constructors
        /// <summary>
        /// A blank constructor for creating an empty defines list.
        /// </summary>
        public DefinesList() {
            parentGame = null;
            mInGame = false;
            mIsChanged = false;
            mResFile = "";
        }

        /// <summary>
        /// Initializes this defines list by loading the specified fan defines file.
        /// The defines list is not associated with an AGI game.
        /// </summary>
        /// <param name="filename"></param>
        public DefinesList(string filename) {
            parentGame = null;
            mInGame = false;
            mIsChanged = false;
            mResFile = filename;
            (Error, Warnings) = LoadDefines(mResFile);
        }

        /// <summary>
        /// Initializes the defines list assigned to the specified AGI game.
        /// </summary>
        /// <param name="parent"></param>
        public DefinesList(string filename, AGIGame game) {
            parentGame = game;
            mInGame = true;
            mIsChanged = false;
            mResFile = filename;
            (Error, Warnings) = LoadDefines(mResFile);
        }

        /// <summary>
        /// Initializes a new defines list and assigns it to the specified AGI game.
        /// </summary>
        /// <param name="game"></param>
        public DefinesList(AGIGame game) {
            parentGame = game;
            mInGame = true;
            mIsChanged = false;
            mResFile = game.SrcResDir + "globals.txt";
            agDefines = [];
            try {
                File.WriteAllText(mResFile, "[\r\n[ global defines file for " +
                    game.GameID + "\r\n[\r\n");
            }
            catch {
                Error = ResourceErrorType.FileAccessError;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the define object at the specified location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TDefine this[string index] {
            get => agDefines[index];
        }

        public Dictionary<string, TDefine>.ValueCollection Values {
            get =>agDefines.Values;
        }

        /// <summary>
        /// Gets the number of defines in this defines list.
        /// </summary>
        public int Count {
            get { return agDefines.Count; }
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

        public string ResFile {
            get => mResFile;
            set {
                mResFile = value;
                // rename the file
                try {
                    File.Move(mResFile, value);
                }
                catch {
                    throw;
                }
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Loads the specified defines file, creating the defines list.
        /// </summary>
        private (ResourceErrorType Error, int Warnings) LoadDefines(string definefile) {
            ResourceErrorType reterr = ResourceErrorType.NoError;
            int retwarn = 0;

            agDefines = [];
            // verify file exists
            if (!File.Exists(definefile)) {
                return (ResourceErrorType.DefinesNoFile, 0);
            }
            // check for readonly
            if ((File.GetAttributes(definefile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                reterr = ResourceErrorType.DefinesReadOnly;
            }

            string[] definelist;
            try {
                definelist = File.ReadAllLines(definefile);
            }
            catch (Exception ex) {
                ErrData[0] = ex.Message;
                return (ResourceErrorType.DefinesAccessError, 0);
            }
            // if nothing to load, just return
            if (definelist.Length == 0) {
                return (reterr, 0);
            }
            retwarn = ReadFanDefines(definelist);
            return (reterr, retwarn);
        }

        private int ReadFanDefines(string[] definelist) {
            // invalid entries are ignored with a warning;
            // all valid entries are kept
            TDefine tdNewDefine = new();
            int retwarn = 0;
            for (int i = 0; i < definelist.Length; i++) {
                string linetext = definelist[i].Replace('\t', ' ').Trim();
                // strip off comment
                string s = "";
                linetext = StripComments(linetext, ref s);
                if (linetext.Length != 0) {
                    if (linetext.Left(8) == "#define ") {
                        linetext = linetext[8..].Trim();
                        int pos = linetext.IndexOf(' ');
                        if (pos != -1) {
                            tdNewDefine.Default = tdNewDefine.Name = linetext[..pos].Trim();
                            tdNewDefine.Value = linetext[pos..].Trim();
                            DefineNameCheck chkName = ValidateDefineName(tdNewDefine.Name);
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
                                    agDefines.Add(tdNewDefine.Name, tdNewDefine);
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
            return retwarn;
        }

        public void Clear() {
            agDefines = [];
            mIsChanged = true;
        }

        public bool ContainsName(string name) {
            return agDefines.ContainsKey(name);
        }

        public DefineNameCheck ValidateDefineName(string checkName, bool checklist) {
            // basic checks
            DefineNameCheck retval = BaseNameCheck(checkName, false);
            if (retval != DefineNameCheck.OK) {
                return retval;
            }
            // overrides OK in fan syntax, order: locals>globals>resids>reserved
            // no overrides in sierra syntax

            // check define already in this list
            if (checklist) {
                if (agDefines.ContainsKey(checkName)) {
                    return DefineNameCheck.Global;
                }
            }
            if (parentGame is not null) {
                if (parentGame.IncludeIDs) {
                    // resourceids
                    foreach (Logic logic in parentGame.Logics) {
                        if (checkName == logic.ID) {
                            return ResourceID;
                        }
                    }
                    foreach (Picture picture in parentGame.Pictures) {
                        if (checkName == picture.ID) {
                            return ResourceID;
                        }
                    }
                    foreach (Sound sound in parentGame.Sounds) {
                        if (checkName == sound.ID) {
                            return ResourceID;
                        }
                    }
                    foreach (Engine.View view in parentGame.Views) {
                        if (checkName == view.ID) {
                            return ResourceID;
                        }
                    }
                }
                if (parentGame.IncludeReserved) {
                    // reserved defines
                    for (int i = 0; i < 10; i++) {
                        TDefine[] tmpDefines = parentGame.agReservedDefines.ByGroup((ResDefGroup)i);
                        for (int j = 0; j < tmpDefines.Length; j++) {
                            if (checkName == tmpDefines[j].Name) {
                                switch ((ResDefGroup)i) {
                                case ResDefGroup.Variable:
                                    return ReservedVar;
                                case ResDefGroup.Flag:
                                    return ReservedFlag;
                                case ResDefGroup.EdgeCode:
                                case ResDefGroup.ObjectDir:
                                case ResDefGroup.VideoMode:
                                case ResDefGroup.ComputerType:
                                case ResDefGroup.Color:
                                    return ReservedNum;
                                case ResDefGroup.Object:
                                    return ReservedObj;
                                case ResDefGroup.String:
                                    return ReservedStr;
                                case ResDefGroup.GameInfo:
                                    return ReservedGameInfo;
                                }
                            }
                        }
                    }
                }
            }
            // if no error conditions, it's OK
            return DefineNameCheck.OK;
        }

        public DefineNameCheck ValidateDefineName(string checkName) {
            return ValidateDefineName(checkName, true);
        }

        public void Save() {
            // save list of defines

            List<string> stlDefines = BuildFanDefinesFile();
            try {
                File.WriteAllLines(mResFile, stlDefines);
                mIsChanged = false;
            }
            catch {
                throw;
            }
        }

        private List<string> BuildFanDefinesFile() {
            // determine longest name length to facilitate aligning values
            int lngMaxLen = 0;
            int lngMaxV = 0;
            foreach (var define in agDefines.Values) {
                if (define.Name.Length > lngMaxLen) {
                    lngMaxLen = define.Name.Length;
                }
                // right-align non-strings
                if (define.Value[0] != '"') {
                    if (define.Value.Length > lngMaxV) {
                        lngMaxV = define.Value.Length;
                    }
                }
            }
            List<string> tmpStrList = [];
            // add a useful header
            tmpStrList.Add("[");
            if (parentGame is null) {
                tmpStrList.Add("[ global defines file " + Path.GetFileName(mResFile));
            }
            else {
                tmpStrList.Add("[ global defines file for " + parentGame.GameID);
            }
            tmpStrList.Add("[");
            foreach (var define in agDefines.Values) {
                // get name and Value
                string strName = define.Name.PadRight(lngMaxLen);
                string strValue = define.Value.PadLeft(4);
                // right align non-strings
                if (strValue[0] != '"') {
                    strValue = strValue.PadLeft(lngMaxV);
                }
                string strComment = define.Comment;
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
            agDefines.Add(adddef.Name, adddef);
        }

        #endregion
    }
}
