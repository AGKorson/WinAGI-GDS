using System;
using System.IO;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
namespace WinAGI.Engine {
    /// <summary>
    /// Represents a list of AGI defines that can be represented as an
    /// include file.
    /// </summary>
    public class GlobalList {
        // TODO: no more global list; instead provide 'define list' object that lets users 
        // edit defines in the same editor, but all files must be manually included;
        // create a scope property allowing files to be tagged 'global' (which will
        // automatically add them to existing/new logics) or identify which logics they
        // apply to; add a branch to resource tree to list define files

        #region Local Members
        TDefine[] agGlobal;
        readonly AGIGame parent;
        uint agGlobalCRC = 0xffffffff;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes this defines list by assigning it to the specified AGI game.
        /// </summary>
        /// <param name="parent"></param>
        public GlobalList(AGIGame parent) {
            this.parent = parent;
            agGlobal = [];
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the define object at the specified location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TDefine this[int index] {
            get { return agGlobal[index]; }
            set { agGlobal[index] = value; }
        }

        /// <summary>
        /// Gets the number of defines in this defines list.
        /// </summary>
        public int Count {
            get { return agGlobal.Length; }
        }

        /// <summary>
        /// Gets the change state of this defines list. Returns true if the
        /// list has not been changed.
        /// </summary>
        public bool IsChanged {
            get {
                if (File.Exists(parent.agGameDir + "globals.txt")) {
                    // true if CRC shows file hasn't changed
                    DateTime dtFileMod = File.GetLastWriteTime(parent.agGameDir + "globals.txt");
                    return CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(dtFileMod.ToString())) != agGlobalCRC;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the CRC value for this defines list.
        /// </summary>
        public uint CRC { get => agGlobalCRC; }
        #endregion

        #region Methods
        /// <summary>
        /// Loads the specified defines file, creating the defines list.
        /// </summary>
        internal void LoadGlobalDefines(string definefile) {
            FileStream fsDefines = null;
            StreamReader srDefines;
            string strLine;
            int i, gCount = 0;
            TDefine tdNewDefine = new();
            DateTime dtFileMod;
            agGlobalCRC = 0xffffffff;

            agGlobal = [];
            // file must exist
            if (!File.Exists(definefile)) {
                WinAGIException wex = new(LoadResString(524).Replace(ARG1, definefile)) {
                    HResult = WINAGI_ERR + 524,
                };
                wex.Data["missingfile"] = definefile;
                throw wex;
            }
            // file must not be readonly
            if ((File.GetAttributes(definefile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                try {
                    File.SetAttributes(definefile, FileAttributes.Normal);
                }
                catch {
                    WinAGIException wex = new(LoadResString(700).Replace(ARG1, definefile)) {
                        HResult = WINAGI_ERR + 700,
                    };
                    wex.Data["badfile"] = definefile;
                    throw wex;
                }
            }
            // open file for input
            try {
                fsDefines = new FileStream(definefile, FileMode.Open);
            }
            catch (Exception ex) {
                WinAGIException wex = new(LoadResString(502).Replace(ARG1, ex.HResult.ToString()).Replace(ARG2, definefile)) {
                    HResult = WINAGI_ERR + 502,
                };
                wex.Data["exception"] = ex;
                wex.Data["badfile"] = definefile;
                fsDefines?.Dispose();
                throw wex;
            }
            // if this is an empty file
            if (fsDefines.Length == 0) {
                WinAGIException wex = new(LoadResString(605).Replace(ARG1, definefile)) {
                    HResult = WINAGI_ERR + 605,
                };
                fsDefines.Dispose();
                throw wex;
            }
            // grab the file data
            srDefines = new StreamReader(fsDefines);
            while (true) {
                strLine = srDefines.ReadLine();
                if (strLine is null)
                    break;
                // strip off comment
                string s = "";
                strLine = StripComments(strLine, ref s);
                if (strLine.Length != 0) {
                    if (strLine[..8] == "#define ") {
                        strLine = strLine[8..].Trim();
                        i = strLine.IndexOf(' ');
                        if (i != 0) {
                            tdNewDefine.Name = strLine[..i].Trim();
                            tdNewDefine.Value = strLine[i..].Trim();
                            // validate define name
                            DefineNameCheck chkName = LogicCompiler.ValidateNameGlobal(tdNewDefine);
                            switch (chkName) {
                            case DefineNameCheck.OK or
                            ReservedVar or
                            ReservedFlag or
                            ReservedNum or
                            ReservedObj or
                            ReservedStr or
                            ReservedMsg:
                                DefineValueCheck chkValue = LogicCompiler.ValidateDefValue(ref tdNewDefine);
                                switch (chkValue) {
                                case DefineValueCheck.OK or Reserved or DefineValueCheck.Global:
                                    gCount++;
                                    Array.Resize(ref agGlobal, gCount);
                                    agGlobal[gCount - 1] = tdNewDefine;
                                    break;
                                }
                                break;
                            }
                        }
                    }
                }
            }
            // get datemodified property
            dtFileMod = File.GetLastWriteTime(parent.agGameDir + "globals.txt");
            // save crc for this file
            agGlobalCRC = CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(dtFileMod.ToString()));
            srDefines.Dispose();
            fsDefines.Dispose();
        }
        #endregion
    }
}
