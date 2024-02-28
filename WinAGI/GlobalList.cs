using System;
using System.IO;
using static WinAGI.Engine.Base;
using static WinAGI.Common.Base;
using System.Linq;
using static WinAGI.Engine.DefineNameCheck;
using static WinAGI.Engine.DefineValueCheck;
using Microsoft.VisualStudio.OLE.Interop;
namespace WinAGI.Engine
{
    public class GlobalList
    {
        //user defined global defines
        TDefine[] agGlobal;
        AGIGame parent;
        uint agGlobalCRC = 0xffffffff;
        public GlobalList(AGIGame parent)
        {
            this.parent = parent;
            agGlobal = Array.Empty<TDefine>();
        }
        public TDefine this[int index]
        {
            get { return agGlobal[index]; }
            set { agGlobal[index] = value; }
        }
        public int Count
        {
            get { return agGlobal.Length; }
        }
        public bool IsSet
        {
            get
            {
                // true if CRC shows file hasn't changed
                DateTime dtFileMod = File.GetLastWriteTime(parent.agGameDir + "globals.txt");
                return (CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(dtFileMod.ToString())) != agGlobalCRC);
            }
        }
        public uint CRC
        { get => agGlobalCRC; }
        internal void GetGlobalDefines()
        {
            string strLine, strTmp;
            string[] strSplitLine;
            int i, gCount = 0;
            TDefine tdNewDefine = new TDefine();
            DateTime dtFileMod;
            agGlobalCRC = 0xffffffff;

            //clear global list
            agGlobal = Array.Empty<TDefine>();
            //look for global file
            if (!File.Exists(parent.agGameDir + "globals.txt")) {
                return;
            }
            //open file for input
            using var glbSR = new StringReader(parent.agGameDir + "globals.txt");
            {
                //read in globals
                while (true) {
                    //get next line
                    strLine = glbSR.ReadLine();
                    if (strLine == null)
                        break;
                    //strip off comment (need a throwaway string for comment)
                    string s = "";
                    strLine = Compiler.StripComments(strLine, ref s, false);
                    //ignore blanks '''and comments(// or [)
                    if (strLine.Length != 0) {
                        //splitline into name and Value
                        strSplitLine = strLine.Split("\t");//     vbTab
                                                           //if not exactly two elements
                        if (strSplitLine.Length != 1) {
                            //not a valid global.txt; check for defines.txt
                            strTmp = strLine.Substring(0, 8).ToLower();
                            if (strTmp == "#define ")
                                //set up splitline array
                                Array.Resize(ref strSplitLine, 2);
                            //strip off the define statement
                            strLine = strLine.Substring(8, strLine.Length - 8);
                            //extract define name
                            i = strLine.IndexOf(" ");
                            if (i != 0) {
                                strSplitLine[0] = strLine.Substring(0, i - 1);
                                strLine = strLine.Substring(i, strLine.Length - i);
                                strSplitLine[1] = strLine.Trim();
                            }
                            else {
                                //invalid; reset splitline so this line
                                //gets ignored
                                Array.Resize(ref strSplitLine, 0);
                            }
                        }
                        //if exactly two elements
                        if (strSplitLine.Length == 2) {
                            //strSplitLine(0] has name
                            tdNewDefine.Name = strSplitLine[0].Trim();
                            //strSplitLine(1] has Value
                            tdNewDefine.Value = strSplitLine[1].Trim();
                            //validate define name
                            DefineNameCheck chkName = ValidateDefName(tdNewDefine);
                            switch (chkName) {
                            case ncOK or
                            ncReservedVar or
                            ncReservedFlag or
                            ncReservedNum or
                            ncReservedObj or
                            ncReservedStr or
                            ncReservedMsg:
                                DefineValueCheck chkValue = ValidateDefValue(tdNewDefine);
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
                //save crc for this file
                //get datemodified property
                dtFileMod = File.GetLastWriteTime(parent.agGameDir + "globals.txt");
                agGlobalCRC = CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(dtFileMod.ToString()));
            }
        }
        internal DefineNameCheck ValidateDefName(TDefine CheckDef)
        {
            // validates that DefValue is a valid define Value
            int i;
            DefineNameCheck tmpResult;

            // use standard compiler to check everything except other globals
            tmpResult = Compiler.ValidateName(CheckDef);
            if (tmpResult != DefineNameCheck.ncOK) {
                // pass along error value
                return tmpResult;
            }


            // check against current globals
            for (i = 0; i < agGlobal.Length; i++) {
                if (CheckDef.Name == agGlobal[i].Name)
                    return ncGlobal;
            }
            // check against ingame reserved defines:
            if (Compiler.UseReservedNames) {
                for (i = 0; i < parent.agResGameDef.Length; i++) {
                    if (CheckDef.Name == parent.agResGameDef[i].Name)
                        //invobj count is number; rest are msgstrings
                        return i == 3 ? ncReservedNum : ncReservedMsg;
                }
            }
            // Linq feature makes it easy to check for invalid characters
            // Any applies the test inside to each element of the source
            // so testList.Any(checkItem.Op) returns true if checkItem.Op is true
            // for any element in testList!
            if ((INVALID_DEFNAME_CHARS).Any(CheckDef.Name.Contains)) {
                // bad
                return ncBadChar;
            }
            // if no error conditions, it's OK
            return ncOK;
        }

        internal DefineNameCheck ValidateDefName(string CheckName)
        {
            TDefine CheckDef = new()
            {
                Name = CheckName
            };
            return ValidateDefName(CheckDef);
        }
        internal DefineValueCheck ValidateDefValue(TDefine TestDefine)
        {
            //validates that TestDefine.Value is a valid define Value
            string strVal;
            int intVal;

            if (TestDefine.Value.Length == 0)
                return vcEmpty;
            //values must be a variable/flag/etc, string, or a number
            if (!IsNumeric(TestDefine.Value)) {
                //if Value is an argument marker
                // use LINQ - if the name starts with any of these letters
                if ("vfmoiswc".Any(TestDefine.Value.ToLower().StartsWith)) {
                    //if rest of Value is numeric,
                    strVal = TestDefine.Value.Substring(0, TestDefine.Value.Length - 1);
                    if (IsNumeric(strVal)) {
                        //if Value is not between 0-255
                        intVal = int.Parse(strVal);
                        if (intVal < 0 || intVal > 255)
                            return vcBadArgNumber;
                        //check defined globals
                        for (int i = 0; i <= agGlobal.Length - 1; i++) {
                            //if this define has same Value
                            if (agGlobal[i].Value == TestDefine.Value)
                                return vcGlobal;
                        }
                        //verify that the Value is not already assigned
                        switch ((int)TestDefine.Value.ToLower().ToCharArray()[0]) {
                        case 102: //flag
                            TestDefine.Type = ArgTypeEnum.atFlag;
                            if (Compiler.UseReservedNames)
                                //if already defined as a reserved flag
                                if (intVal <= 15)
                                    return vcReserved;
                            // if version 3.002.102 and flag 20
                            if (Val(parent.agIntVersion) >= 3.002102 && intVal == 20) {
                                return vcReserved;
                            }
                            break;
                        case 118: //variable
                            TestDefine.Type = ArgTypeEnum.atVar;
                            if (Compiler.UseReservedNames)
                                //if already defined as a reserved variable
                                if (intVal <= 26)
                                    return vcReserved;
                            break;
                        case 109: //message
                            TestDefine.Type = ArgTypeEnum.atMsg;
                            break;
                        case 111: //screen object
                            TestDefine.Type = ArgTypeEnum.atSObj;
                            if (Compiler.UseReservedNames)
                                //can't be ego
                                if (TestDefine.Value == "o0")
                                    return vcReserved;
                            break;
                        case 105: //inv object
                            TestDefine.Type = ArgTypeEnum.atInvItem;
                            break;
                        case 115: //string
                            TestDefine.Type = ArgTypeEnum.atStr;
                            if (intVal > 23 || (intVal > 11 &&
                              (parent.agIntVersion == "2.089" ||
                              parent.agIntVersion == "2.272" ||
                              parent.agIntVersion == "3.002149")))
                                return vcBadArgNumber;
                            break;
                        case 119: //word
                            TestDefine.Type = ArgTypeEnum.atWord;
                            break;
                        case 99: //controller
                                 //controllers limited to 0-49
                            if (intVal > 49)
                                return vcBadArgNumber;
                            TestDefine.Type = ArgTypeEnum.atCtrl;
                            break;
                        }
                        //Value is ok
                        return vcOK;
                    }
                }
                //non-numeric, non-marker and most likely a string
                TestDefine.Type = ArgTypeEnum.atDefStr;
                //check Value for string delimiters in Value
                //TODO: need a 'IsAStrng' function; just becuase it ends in a quote 
                // doesn't mean it's a string; need to check for embedded strings
                if (TestDefine.Value.Substring(0, 1) != "\"" || TestDefine.Value.Substring(TestDefine.Value.Length - 1, 1) != "\"")
                    return vcNotAValue;
                else
                    return vcOK;
            }
            else {
                // numeric
                TestDefine.Type = ArgTypeEnum.atNum;
                //TODO: should validate number? are negatives OK? values > 255?
                // shouldn't they at least throw a warning?
                return vcOK;
            }
        }
    }
}
