using System;
using System.IO;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Common.WinAGI;
using System.Linq;

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
    { get
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
      int i, lngTest, gCount = 0;
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
              lngTest = ValidateDefName(tdNewDefine.Name);
              if (lngTest == 0 || (lngTest >= 8 && lngTest <= 12)) {
                lngTest = ValidateDefName(tdNewDefine.Name);
                lngTest = ValidateDefValue(tdNewDefine);
                if (lngTest == 0 || lngTest == 5 || lngTest == 6) {
                  //increment Count
                  gCount++;
                  //add it
                  Array.Resize(ref agGlobal, gCount);
                  agGlobal[gCount - 1] = tdNewDefine;
                }
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
    internal int ValidateDefName(string DefName)
    {
      // validates that DefValue is a valid define Value
      // 
      // returns 0 if successful
      // 
      // returns an error code on failure:
      // 1 = no name
      // 2 = name is numeric
      // 3 = name is command
      // 4 = name is test command
      // 5 = name is a compiler keyword
      // 6 = name is an argument marker
      // 7 = name is already globally defined
      // 8 = name is reserved variable name
      // 9 = name is reserved flag name
      // 10 = name is reserved number constant
      // 11 = name is reserved object constant
      // 12 = name is reserved message constant
      // 13 = name contains improper character
      int i;

      // if no name
      if (DefName.Length == 0)
        return 1;
      // name cant be numeric
      if (IsNumeric(DefName))
        return 2;
      // check against regular commands
      for (i = 0; i <= AGICommands.agCmds.Length; i++) {
        if (DefName == AGICommands.agCmds[i].Name)
          return 3;
      }
      // check against test commands
      for (i = 0; i <= AGITestCommands.agTestCmds.Length; i++) {
        if (DefName == AGITestCommands.agTestCmds[i].Name)
          return 4;
      }
      // check against keywords
      if ((DefName.ToLower() == "if") || (DefName.ToLower() == "else") ||
         (DefName.ToLower() == "goto"))
        return 5;
      // if the name starts with any of these letters
      if ("vfmoiswc".Any(DefName.ToLower().StartsWith))
        //if rest of string is numeric
        if (IsNumeric(Right(DefName, DefName.Length - 1)))
          // can't have a name that's a valid marker
          return 6;
      // check against current globals
      for (i = 0; i < agGlobal.Length; i++) {
        if (DefName == agGlobal[i].Name)
          return 7;
      }
      // check against reserved defines:
      if (Compiler.UseReservedNames) {
        // check against reserved variables
        for (i = 0; i <= 26; i++) {
          if (DefName == Compiler.ReservedDefines(ArgTypeEnum.atVar)[i].Name)
            return 8;
        }
        // check against reserved flags
        for (i = 0; i <= 17; i++) {
          if (DefName == Compiler.ReservedDefines(ArgTypeEnum.atFlag)[i].Name)
            return 9;
        }
        // check against other reserved constants
        for (i = 0; i <= 4; i++) {
          if (DefName == Compiler.ResDefByGrp(3)[i].Name)
            return 10;
        }
        for (i = 0; i <= 8; i++) {
          if (DefName == Compiler.ResDefByGrp(4)[i].Name)
            return 10;
        }
        for (i = 0; i <= 4; i++) {
          if (DefName == Compiler.ResDefByGrp(5)[i].Name)
            return 10;
        }
        // check against other reserved defines
        if (DefName == Compiler.ResDefByGrp(8)[0].Name)
          return 11;
        //TODO: need to rewrite checks for ingame defines
        //for (i = 1; i <= 3; i++) {
        //  if (DefName == agResDef[i].Name)
        //    return 12;
        //if (DefName == agResDef[4].Name)
        //  return 10;
        //if (DefName == agResDef[5].Name)
        //  return 11;
        //}
      }
      // Linq feature makes it easy to check for invalid characters
      // Any applies the test inside to each element of the source
      // so testList.Any(checkItem.Op) returns true if checkItem.Op is true
      // for any element in testList!
      if ((INVALID_DEFNAME_CHARS).Any(DefName.Contains)) {
        // bad
        return 13;
      }
      // if no error conditions, it's OK
      return 0;
    }
    internal int ValidateDefValue(TDefine TestDefine)
    {
      //validates that TestDefine.Value is a valid define Value
      //
      //returns 0 if successful
      //
      //returns an error code on failure:
      //1 = no Value
      //2 = Value is an invalid argument marker (not used anymore]
      //3 = Value contains an invalid argument Value
      //4 = Value is not a string, number or argument marker
      //5 = Value is already defined by a reserved name
      //6 = Value is already defined by a global name

      string strVal;
      int intVal;

      if (TestDefine.Value.Length == 0)
        return 1;
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
              return 3;
            //check defined globals
            for (int i = 0; i <= agGlobal.Length - 1; i++) {
              //if this define has same Value
              if (agGlobal[i].Value == TestDefine.Value)
                return 6;
            }
            //verify that the Value is not already assigned
            switch ((int)TestDefine.Value.ToLower().ToCharArray()[0]) {
            case 102: //flag
              TestDefine.Type = ArgTypeEnum.atFlag;
              if (Compiler.UseReservedNames)
                //if already defined as a reserved flag
                if (intVal <= 15)
                  return 5;
              break;
            case 118: //variable
              TestDefine.Type = ArgTypeEnum.atVar;
              if (Compiler.UseReservedNames)
                //if already defined as a reserved variable
                if (intVal <= 26)
                  return 5;
              break;
            case 109: //message
              TestDefine.Type = ArgTypeEnum.atMsg;
              break;
            case 111: //screen object
              TestDefine.Type = ArgTypeEnum.atSObj;
              if (Compiler.UseReservedNames)
                //can't be ego
                if (TestDefine.Value == "o0")
                  return 5;
              break;
            case 105: //inv object
              TestDefine.Type = ArgTypeEnum.atIObj;
              break;
            case 115: //string
              TestDefine.Type = ArgTypeEnum.atStr;
              break;
            case 119: //word
              TestDefine.Type = ArgTypeEnum.atWord;
              break;
            case 99: //controller
                     //controllers limited to 0-49
              if (intVal > 49)
                return 3;
              TestDefine.Type = ArgTypeEnum.atCtrl;
              break;
            }
            //Value is ok
            return 0;
          }
        }
        //non-numeric, non-marker and most likely a string
        TestDefine.Type = ArgTypeEnum.atDefStr;
        //check Value for string delimiters in Value
        if (TestDefine.Value.Substring(0, 1) != "\"" || TestDefine.Value.Substring(TestDefine.Value.Length - 1, 1) != "\"")
          return 4;
        else
          return 0;
      }
      else {
        // numeric
        TestDefine.Type = ArgTypeEnum.atNum;
        return 0;
      }
    }
  }
}
