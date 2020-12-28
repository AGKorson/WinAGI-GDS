﻿using System;
using System.Collections.Generic;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using System.IO;

namespace WinAGI
{
  public class AGISounds
  {
    public AGISounds()
    {
      // create the initial Col object
      Col = new SortedList<byte, AGISound>();
    }
    internal SortedList<byte, AGISound> Col
    { get; private set; }
    public AGISound this[byte index]
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
      //check for thsi sound in the collection
      return Col.ContainsKey(ResNum);
    }
    public void Clear()
    {
      Col = new SortedList<byte, AGISound>();
    }
    public AGISound Add(byte ResNum, AGISound NewSound = null)
    {
      //adds a new sound to a currently open game
      AGISound agResource;
      int intNextNum = 1;
      string strID, strBaseID;
      //if this sound already exists
      if (Exists(ResNum))
      {
        //resource already exists
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //create new sound resource
      agResource = new AGISound();
      //if an object was passed
      if ((NewSound == null))
      {
        //proposed ID will be default
        strID = "Sound" + ResNum;
      }
      else
      {
        //copy entire sound
        agResource.SetSound(NewSound);
        //get proposed id
        strID = NewSound.ID;
      }
      // validate id
      strBaseID = strID;
      while (!IsUniqueResID(strID))
      {
        intNextNum += 1;
        strID = strBaseID + "_" + intNextNum;
      }
      //add it
      Col.Add(ResNum, agResource);
      //force flags so save function will work
      agResource.IsDirty = true;
      agResource.WritePropState = true;

      //save new sound
      agResource.Save();

      //return the object created
      return agResource;
    }
    public void Remove(byte Index)
    {
      //removes a sound from the game file

      // if the resource exists
      if (Col.ContainsKey(Index))
      {
        //need to clear the directory file first
        UpdateDirFile(Col[Index], true);
        Col.Remove(Index);
        //remove all properties from the wag file
        DeleteSettingSection(agGameProps, "Sound" + Index);
      }
    }
    public void Renumber(byte OldSound, byte NewSound)
    {
      //renumbers a resource
      AGISound tmpSound;
      int intNextNum = 0;
      bool blnUnload = false;
      string strSection, strID, strBaseID;
      //if no change
      if (OldSound == NewSound)
      {
        return;
      }
      //verify new number is not in collection
      if (Col.Keys.Contains(NewSound))
      {
        //number already in use
        throw new Exception("669, LoadResString(669)");
      }
      //get sound being renumbered
      tmpSound = Col[OldSound];

      //if not loaded,
      if (!tmpSound.Loaded)
      {
        tmpSound.Load();
        blnUnload = true;
      }

      //remove old properties
      DeleteSettingSection(agGameProps, "Sound" + OldSound);

      //remove from collection
      Col.Remove(OldSound);

      //delete sound from old number in dir file
      //by calling update directory file method
      UpdateDirFile(tmpSound, true);

      //if id is default
      if (tmpSound.ID.Equals("Sound" + OldSound, StringComparison.OrdinalIgnoreCase))
      {
        //change default ID to new ID
        strID = strBaseID = "Sound" + NewSound;
        while (!IsUniqueResID(strID))
        {
          intNextNum += 1;
          strID = strBaseID + "_" + intNextNum;
        }
      }
      //change number
      tmpSound.Number = NewSound;

      //add with new number
      Col.Add(NewSound, tmpSound);

      //update new sound number in dir file
      UpdateDirFile(tmpSound);

      //add properties back with new sound number
      strSection = "Sound" + NewSound;
      WriteGameSetting(strSection, "ID", tmpSound.ID, "Sounds");
      WriteGameSetting(strSection, "Description", tmpSound.Description);
      //
      //TODO: add rest of default property values
      //

      //force writeprop state back to false
      tmpSound.WritePropState = false;

      //unload if necessary
      if (blnUnload)
      {
        tmpSound.Unload();
      }
    }
    internal void LoadSound(byte bytResNum, sbyte bytVol, int lngLoc)
    {
      //called by the resource loading method for the initial loading of
      //resources into logics collection
      //if this Logic number is already in the game
      if (agSnds.Exists(bytResNum))
      {
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //create new logic object
      AGISound newResource = new AGISound();
      newResource.InGameInit(bytResNum, bytVol, lngLoc);
      //add it
      Col.Add(bytResNum, newResource);
    }
  }
}