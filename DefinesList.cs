using System;
using System.Collections.Generic;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
namespace WinAGI.Engine {
    /// <summary>
    /// Represents a list of AGI defines that can be formatted as an
    /// include file. Applies only to games using fan syntax
    /// </summary>
    public class DefinesList {

        #region Fields
        private readonly AGIGame parentGame;
        private string mResFile = "";
        private bool mInGame = false;
        private Dictionary<string, Define> agDefines = [];
        #endregion

        #region Constructors
        /// <summary>
        /// A blank constructor for creating an empty defines list.
        /// </summary>
        public DefinesList() {
            parentGame = null;
            mInGame = false;
            mResFile = "";
        }

        /// <summary>
        /// Initializes a new defines list and assigns it to the specified AGI game.
        /// </summary>
        /// <param name="game"></param>
        public DefinesList(AGIGame game) {
            parentGame = game;
            mInGame = true;
            mResFile = Path.Combine(parentGame.SrcResDir, "globals.txt");
            agDefines = [];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the define object at the specified location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Define this[string index] {
            get => agDefines[index];
        }

        public Dictionary<string, Define>.ValueCollection Values {
            get => agDefines.Values;
        }

        /// <summary>
        /// Gets the number of defines in this defines list.
        /// </summary>
        public int Count {
            get {
                return agDefines.Count;
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

        public string ResFile {
            get => mResFile;
            set {
                mResFile = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads resource defines the list's associated file.
        /// </summary>
        public void LoadDefines() {
            if (mResFile.Length == 0) {
                Error = ResourceErrorType.DefinesNoFile;
                Warnings = 0;
                return;
            }

            // reset error and warnings
            Error = ResourceErrorType.NoError;
            Warnings = 0;

            agDefines = [];
            // verify file exists
            if (!File.Exists(mResFile)) {
                Error = ResourceErrorType.DefinesNoFile;
                return;
            }
            // check for readonly
            if ((File.GetAttributes(mResFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                Error = ResourceErrorType.DefinesReadOnly;
            }

            string[] definelist = [];
            try {
                definelist = File.ReadAllLines(mResFile);
            }
            catch (Exception ex) {
                ErrData[0] = ex.Message;
                Error = ResourceErrorType.DefinesAccessError;
                Warnings = 0;
            }
            // if nothing to load, just return
            if (definelist.Length == 0) {
                return;
            }
            Warnings = ReadFanDefines(definelist);
        }

        private int ReadFanDefines(string[] definelist) {
            // invalid entries are ignored with a warning;
            // all valid entries are kept
            Define tdNewDefine = new();
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
                            tdNewDefine.DefaultName = tdNewDefine.Name = linetext[..pos].Trim();
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
                        Define[] tmpDefines = parentGame.agReservedDefines.ByGroup((ResDefGroup)i);
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
            }
            catch {
                throw;
            }
        }

        private List<string> BuildFanDefinesFile() {
            // determine longest name length to facilitate aligning values
            int maxLen = 0;
            int maxV = 0;
            foreach (var define in agDefines.Values) {
                if (define.Name.Length > maxLen) {
                    maxLen = define.Name.Length;
                }
                // right-align non-strings
                if (define.Value[0] != '"') {
                    if (define.Value.Length > maxV) {
                        maxV = define.Value.Length;
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
                string name = define.Name.PadRight(maxLen);
                string value = define.Value.PadLeft(4);
                // right align non-strings
                if (value[0] != '"') {
                    value = value.PadLeft(maxV);
                }
                string comment = define.Comment;
                if (comment.Length > 0) {
                    comment = " " + comment;
                }
                tmpStrList.Add("#define " + name + "  " + value + comment);
            }
            return tmpStrList;
        }

        public void Add(string name, string value, string comment, ArgType type) {
            Define adddef = new() {
                Name = name,
                DefaultName = name,
                Value = value,
                DefaultValue = value,
                Comment = comment,
                Type = type
            };
            agDefines.Add(adddef.Name, adddef);
        }

        internal void Clone(DefinesList newlist) {
            // repace he current list with the defines in newlist
            Clear();
            foreach (Define newdef in newlist.Values) {
                Add(newdef.Name, newdef.Value, newdef.Comment, newdef.Type);
            }
        }

        #endregion
    }
}
