using System;
using System.IO;
using static WinAGI.Common.Base;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
namespace WinAGI.Engine {
    public class GlobalList {
        // user defined global defines
        // TODO: no more global list; instead provide 'define list' object that lets users 
        // edit defines in the same editor, but all files must be manually included;
        // create a scope property allowing files to be tagged 'global' (which will
        // automatically add them to existing/new logics) or identify which logics they
        // apply to; add a branch to resource tree to list define files
        TDefine[] agGlobal;
        AGIGame parent;
        uint agGlobalCRC = 0xffffffff;

        public GlobalList(AGIGame parent) {
            this.parent = parent;
            agGlobal = [];
        }

        public TDefine this[int index] {
            get { return agGlobal[index]; }
            set { agGlobal[index] = value; }
        }

        public int Count {
            get { return agGlobal.Length; }
        }

        public bool IsSet {
            get {
                // true if CRC shows file hasn't changed
                DateTime dtFileMod = File.GetLastWriteTime(parent.agGameDir + "globals.txt");
                return (CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(dtFileMod.ToString())) != agGlobalCRC);
            }
        }

        public uint CRC { get => agGlobalCRC; }

        internal void LoadGlobalDefines() {
            string strLine, strTmp;
            string[] strSplitLine;
            int i, gCount = 0;
            TDefine tdNewDefine = new();
            DateTime dtFileMod;
            agGlobalCRC = 0xffffffff;

            // clear global list
            agGlobal = [];
            if (!File.Exists(parent.agGameDir + "globals.txt")) {
                return;
            }
            // open file for input
            using var glbSR = new StringReader(parent.agGameDir + "globals.txt");
            {
                while (true) {
                    strLine = glbSR.ReadLine();
                    if (strLine is null)
                        break;
                    // strip off comment
                    string s = "";
                    strLine = Compiler.StripComments(strLine, ref s, false);
                    // ignore blanks '''and comments(// or [)
                    if (strLine.Length != 0) {
                        // splitline into name and Value
                        strSplitLine = strLine.Split("\t");
                        // if not exactly two elements
                        if (strSplitLine.Length != 1) {
                            // not a valid global.txt; check for defines.txt
                            strTmp = strLine[..8].ToLower();
                            if (strTmp == "#define ")
                                // set up splitline array
                                Array.Resize(ref strSplitLine, 2);
                            // strip off the define statement
                            strLine = strLine[8..];
                            //extract define name
                            i = strLine.IndexOf(' ');
                            if (i != 0) {
                                strSplitLine[0] = strLine[..(i - 1)];
                                strLine = strLine[i..];
                                strSplitLine[1] = strLine.Trim();
                            }
                            else {
                                // invalid; reset splitline so this line gets ignored
                                Array.Resize(ref strSplitLine, 0);
                            }
                        }
                        // if exactly two elements
                        if (strSplitLine.Length == 2) {
                            //strSplitLine(0] has name
                            tdNewDefine.Name = strSplitLine[0].Trim();
                            //strSplitLine(1] has Value
                            tdNewDefine.Value = strSplitLine[1].Trim();
                            //validate define name
                            DefineNameCheck chkName = Compiler.ValidateDefName(tdNewDefine);
                            switch (chkName) {
                            case ncOK or
                            ncReservedVar or
                            ncReservedFlag or
                            ncReservedNum or
                            ncReservedObj or
                            ncReservedStr or
                            ncReservedMsg:
                                DefineValueCheck chkValue = Compiler.ValidateDefValue(tdNewDefine);
                                switch (chkValue) {
                                case vcOK or vcReserved or vcGlobal:
                                    //increment Count
                                    gCount++;
                                    //add it
                                    Array.Resize(ref agGlobal, gCount);
                                    agGlobal[gCount - 1] = tdNewDefine;
                                    break;
                                }
                                break;
                            }
                        }
                    }
                }
                // get datemodified property
                dtFileMod = File.GetLastWriteTime(parent.agGameDir + "globals.txt");
                // save crc for this file
                agGlobalCRC = CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(dtFileMod.ToString()));
            }
        }
    }
}
