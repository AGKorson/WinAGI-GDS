using System;
using System.IO;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Common.WinAGI;

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
          strLine = WinAGI.StripComments(strLine, ref s, false);
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
  }
}
