using System;
using System.Collections.Generic;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGICommands;
using System.IO;

namespace WinAGI
{
  public class AGILogics
  {
    public AGILogics()
    {
      // create the initial Col object
      Col = new SortedList<byte, AGILogic>();
    }
    internal SortedList<byte, AGILogic> Col
    { get; private set; }
    public AGILogic this[byte index]
    { get { return Col[index]; } }
    public byte Count
    { get { return (byte)Col.Count; } private set { } }
    public byte Max
    {
      get
      {
        byte max = 0;
        if (Col.Count > 0)
          max = Col.Keys[Col.Count - 1];
        return max;
      }
    }
    public bool Exists(byte ResNum)
    {
      //check for thsi logic in the collection
      return Col.ContainsKey(ResNum);
    }
    internal void Clear()
    {
      Col = new SortedList<byte, AGILogic>();
    }
    public AGILogic Add(byte ResNum, AGILogic NewLogic = null)
    {
      //adds a new logic to a currently open game
      AGILogic agResource;
      int intNextNum = 0;
      string strID, strBaseID;
      //if this Logic already exists
      if (Exists(ResNum))
      {
        //resource already exists
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //create new logic object
      agResource = new AGILogic();
      //if an object was passed
      if ((NewLogic == null))
      {
        //proposed ID will be default
        strID = "Logic" + ResNum;
      }
      else
      {
        //copy entire logic
        agResource.SetLogic(NewLogic);
        //get proposed id
        strID = NewLogic.ID;
      }
      // validate id
      strBaseID = strID;
      while (!IsUniqueResID(strID))
      {
        intNextNum++;
        strID = strBaseID + "_" + intNextNum;
      }
      //add it
      Col.Add(ResNum, agResource);
      //force flags so save function will work
      agResource.IsDirty = true;
      agResource.WritePropState = true;

      //save new logic
      agResource.Save();

      //return the object created
      return agResource;
    }
    public void Remove(byte Index)
    {
      //removes a logic from the game file

      // if the resource exists
      if (Col.ContainsKey(Index))
      {
        //need to clear the directory file first
        UpdateDirFile(Col[Index], true);
        Col.Remove(Index);
        //remove all properties from the wag file
        DeleteSettingSection(agGameProps, "Logic" + Index);
      }
    }
    public void Renumber(byte OldLogic, byte NewLogic)
    {
      //renumbers a resource
      AGILogic tmpLogic;
      int intNextNum = 0;
      bool blnUnload = false;
      string strSection, strID, strBaseID;
      //if no change
      if (OldLogic == NewLogic) 
      {
        return;
      }
      //verify new number is not in collection
      if (Col.Keys.Contains(NewLogic))
      {
        //number already in use
        throw new Exception("669, LoadResString(669)");
      }
      //get logic being renumbered
      tmpLogic = Col[OldLogic];

      //if not loaded,
      if (!tmpLogic.Loaded)
      {
        tmpLogic.Load();
        blnUnload = true;
      }

      //remove old properties
      DeleteSettingSection(agGameProps, "Logic" + OldLogic);
  
      //remove from collection
      Col.Remove(OldLogic);
  
      //delete logic from old number in dir file
      //by calling update directory file method
      UpdateDirFile(tmpLogic, true);
  
    //if id is default
    if (tmpLogic.ID.Equals("Logic" + OldLogic, StringComparison.OrdinalIgnoreCase))
      {
        //change default ID to new ID
        strID = strBaseID = "Logic" + NewLogic;
        while (!IsUniqueResID(strID))
        {
          intNextNum++;
          strID = strBaseID + "_" + intNextNum;
        }
        try
        {
          //get rid of existing file with same name as new logicif needed
          File.Delete(agResDir + tmpLogic.ID + agSrcExt);
          //rename sourcefile
          File.Move(agResDir + "Logic" + OldLogic + agSrcExt, agResDir + tmpLogic.ID + agSrcExt);
        }
        catch (Exception e)
        {
          throw new Exception("670, LoadResString(670) " + e.Message);
        }
      }
  
      //change number
      tmpLogic.Number = NewLogic;
  
      //add with new number
      Col.Add(NewLogic, tmpLogic);
  
      //update new logic number in dir file
      UpdateDirFile(tmpLogic);
  
      //add properties back with new logic number
      strSection = "Logic" + NewLogic;
      WriteGameSetting(strSection, "ID", tmpLogic.ID, "Logics");
      WriteGameSetting(strSection, "Description", tmpLogic.Description);
      WriteGameSetting(strSection, "CRC32", "0x" + tmpLogic.CRC.ToString("x"));
      WriteGameSetting(strSection, "CompCRC32", "0x" +(tmpLogic.CompiledCRC.ToString("x")));
      WriteGameSetting(strSection, "IsRoom", tmpLogic.IsRoom.ToString());
  
      //force writeprop state back to false
      tmpLogic.WritePropState = false;
  
      //unload if necessary
      if (blnUnload) {
        tmpLogic.Unload();
      }
    }
    internal void LoadLogic(byte bytResNum, sbyte bytVol, int lngLoc)
    {
      //called by the resource loading method for the initial loading of
      //resources into logics collection
      //if this Logic number is already in the game
      if (agLogs.Exists(bytResNum))
      {
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //create new logic object
      AGILogic newResource = new AGILogic();
      newResource.InGameInit(bytResNum, bytVol, lngLoc);
      //add it
      Col.Add(bytResNum, newResource);
    }
    public void MarkAllAsDirty()
    {
      foreach (AGILogic tmpLogic in Col.Values)
      {
        tmpLogic.CompiledCRC = 0;
        WriteGameSetting("Logic" + tmpLogic.Number, "CompCRC32", "0x00", "Logics");
      }
      SaveSettingList(agGameProps);
    }
    public string ConvertArg(string ArgIn, ArgTypeEnum ArgType, bool VarOrNum = false)
    {
      throw new NotImplementedException();
      /*
   'tie function to allow access to the LogCompile variable conversion function

   DateTime dtFileMod

   On Error Resume Next
   'if in a game
   if (agMainGame.GameLoaded) {
     'initialize global defines
     'get datemodified property
     dtFileMod = FileLastMod(agGameDir + "globals.txt")
     if (CRC32(StrConv(CStr(dtFileMod), vbFromUnicode)) != agGlobalCRC) {
       GetGlobalDefines
     }

     'if ids not set yet
     if (!blnSetIDs) {
       SetResourceIDs
     }
   }

   'convert argument
   ConvertArgument ArgIn, ArgType, VarOrNum
   'return it
   ConvertArg = LCase$(ArgIn)
       */
    }
  }
}