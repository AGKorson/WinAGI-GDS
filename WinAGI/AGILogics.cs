﻿using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.AGICommands;
using static WinAGI.Common.WinAGI;
using System.IO;

namespace WinAGI.Engine
{
  public class AGILogics : IEnumerable<AGILogic>
  {
    AGIGame parent;
    internal AGILogics(AGIGame parent)
    {
      this.parent = parent;
      // create the initial Col object
      Col = new SortedList<byte, AGILogic>();
    }
    internal SortedList<byte, AGILogic> Col
    { get; private set; }
    public AGILogic this[int index]
    {
      get
      {
        //validate index
        if (index < 0 || index > 255)
          throw new IndexOutOfRangeException();
        return Col[(byte)index];
      }
    }
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
      if (Exists(ResNum)) {
        //resource already exists
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //if an object was not passed
      if ((NewLogic == null)) {
        //create a new logic object
        agResource = new AGILogic();
        //proposed ID will be default
        strID = "Logic" + ResNum;
      }
      else {
        //clone the passed logic
        agResource = NewLogic.Clone();
        //get proposed id
        strID = NewLogic.ID;
      }
      // validate id
      strBaseID = strID;
      while (!NewLogic.IsUniqueResID(strID)) {
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
      if (Col.ContainsKey(Index)) {
        //need to clear the directory file first
        UpdateDirFile(Col[Index], true);
        Col.Remove(Index);
        //remove all properties from the wag file
        parent.agGameProps.DeleteSection("Logic" + Index);
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
      if (OldLogic == NewLogic) {
        return;
      }
      //verify new number is not in collection
      if (Col.Keys.Contains(NewLogic)) {
        //number already in use
        throw new Exception("669, LoadResString(669)");
      }
      //get logic being renumbered
      tmpLogic = Col[OldLogic];

      //if not loaded,
      if (!tmpLogic.Loaded) {
        tmpLogic.Load();
        blnUnload = true;
      }

      //remove old properties
      parent.agGameProps.DeleteSection("Logic" + OldLogic);

      //remove from collection
      Col.Remove(OldLogic);

      //delete logic from old number in dir file
      //by calling update directory file method
      UpdateDirFile(tmpLogic, true);

      //if id is default
      if (tmpLogic.ID.Equals("Logic" + OldLogic, StringComparison.OrdinalIgnoreCase)) {
        //change default ID to new ID
        strID = strBaseID = "Logic" + NewLogic;
        while (!tmpLogic.IsUniqueResID(strID)) {
          intNextNum++;
          strID = strBaseID + "_" + intNextNum;
        }
        try {
          //get rid of existing file with same name as new logicif needed
          File.Delete(parent.agResDir + tmpLogic.ID + agSrcExt);
          //rename sourcefile
          File.Move(parent.agResDir + "Logic" + OldLogic + agSrcExt, parent.agResDir + tmpLogic.ID + agSrcExt);
        }
        catch (Exception e) {
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
      parent.WriteGameSetting(strSection, "ID", tmpLogic.ID, "Logics");
      parent.WriteGameSetting(strSection, "Description", tmpLogic.Description);
      parent.WriteGameSetting(strSection, "CRC32", "0x" + tmpLogic.CRC.ToString("x"));
      parent.WriteGameSetting(strSection, "CompCRC32", "0x" + (tmpLogic.CompiledCRC.ToString("x")));
      parent.WriteGameSetting(strSection, "IsRoom", tmpLogic.IsRoom.ToString());

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
      if (Exists(bytResNum)) {
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //create new logic object
      AGILogic newResource = new AGILogic(parent, bytResNum, bytVol, lngLoc);
      //add it
      Col.Add(bytResNum, newResource);
    }
    public void MarkAllAsDirty()
    {
      foreach (AGILogic tmpLogic in Col.Values) {
        tmpLogic.CompiledCRC = 0;
        parent.WriteGameSetting("Logic" + tmpLogic.Number, "CompCRC32", "0x00", "Logics");
      }
      parent.agGameProps.Save();
    }
    public string ConvertArg(string ArgIn, ArgTypeEnum ArgType, bool VarOrNum = false)
    {
      //tie function to allow access to the LogCompile variable conversion function
      //if in a game
      if (parent.agGameLoaded) {
        //initialize global defines
        if (!parent.GlobalDefines.IsSet) {
          parent.GlobalDefines.GetGlobalDefines();
        }
        //if ids not set yet
        if (!Compiler.blnSetIDs) {
          Compiler.SetResourceIDs(parent);
        }
      }
      //convert argument
      Compiler.ConvertArgument(ref ArgIn, ArgType, ref VarOrNum);
      //return it
      return ArgIn.ToLower();
    }
    LogicEnum GetEnumerator()
    {
      return new LogicEnum(Col);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }
    IEnumerator<AGILogic> IEnumerable<AGILogic>.GetEnumerator()
    {
      return (IEnumerator<AGILogic>)GetEnumerator();
    }
  }
  internal class LogicEnum : IEnumerator<AGILogic>
  {
    public SortedList<byte, AGILogic> _logics;
    int position = -1;
    public LogicEnum(SortedList<byte, AGILogic> list)
    {
      _logics = list;
    }
    object IEnumerator.Current => Current;
    public AGILogic Current
    {
      get
      {
        try {
          return _logics.Values[position];
        }
        catch (IndexOutOfRangeException) {

          throw new InvalidOperationException();
        }
      }
    }
    public bool MoveNext()
    {
      position++;
      return (position < _logics.Count);
    }
    public void Reset()
    {
      position = -1;
    }
    public void Dispose()
    {
      _logics = null;
    }
  }
}