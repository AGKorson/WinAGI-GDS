﻿using System;
using System.Collections.Generic;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using System.IO;

namespace WinAGI
{
  public class AGIViews
  {
    public AGIViews()
    {
      // create the initial Col object
      Col = new SortedList<byte, AGIView>();
    }
    internal SortedList<byte, AGIView> Col
    { get; private set; }
    public AGIView this[byte index]
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
      //check for thsi view in the collection
      return Col.ContainsKey(ResNum);
    }
    public void Clear()
    {
      Col = new SortedList<byte, AGIView>();
    }
    public AGIView Add(byte ResNum, AGIView NewView = null)
    {
      //adds a new view to a currently open game
      AGIView agResource;
      int intNextNum = 1;
      string strID, strBaseID;
      //if this view already exists
      if (Exists(ResNum))
      {
        //resource already exists
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //create new view resource
      agResource = new AGIView();
      //if an object was passed
      if ((NewView == null))
      {
        //proposed ID will be default
        strID = "View" + ResNum;
      }
      else
      {
        //copy entire view
        agResource.SetView(NewView);
        //get proposed id
        strID = NewView.ID;
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

      //save new view
      agResource.Save();

      //return the object created
      return agResource;
    }
    public void Remove(byte Index)
    {
      //removes a view from the game file

      // if the resource exists
      if (Col.ContainsKey(Index))
      {
        //need to clear the directory file first
        UpdateDirFile(Col[Index], true);
        Col.Remove(Index);
        //remove all properties from the wag file
        DeleteSettingSection(agGameProps, "View" + Index);
      }
    }
    public void Renumber(byte OldView, byte NewView)
    {
      //renumbers a resource
      AGIView tmpView;
      int intNextNum = 0;
      bool blnUnload = false;
      string strSection, strID, strBaseID;
      //if no change
      if (OldView == NewView)
      {
        return;
      }
      //verify new number is not in collection
      if (Col.Keys.Contains(NewView))
      {
        //number already in use
        throw new Exception("669, LoadResString(669)");
      }
      //get view being renumbered
      tmpView = Col[OldView];

      //if not loaded,
      if (!tmpView.Loaded)
      {
        tmpView.Load();
        blnUnload = true;
      }

      //remove old properties
      DeleteSettingSection(agGameProps, "View" + OldView);

      //remove from collection
      Col.Remove(OldView);

      //delete view from old number in dir file
      //by calling update directory file method
      UpdateDirFile(tmpView, true);

      //if id is default
      if (tmpView.ID.Equals("View" + OldView, StringComparison.OrdinalIgnoreCase))
      {
        //change default ID to new ID
        strID = strBaseID = "View" + NewView;
        while (!IsUniqueResID(strID))
        {
          intNextNum += 1;
          strID = strBaseID + "_" + intNextNum;
        }
      }
      //change number
      tmpView.Number = NewView;

      //add with new number
      Col.Add(NewView, tmpView);

      //update new view number in dir file
      UpdateDirFile(tmpView);

      //add properties back with new view number
      strSection = "View" + NewView;
      WriteGameSetting(strSection, "ID", tmpView.ID, "Views");
      WriteGameSetting(strSection, "Description", tmpView.Description);
      //
      //TODO: add rest of default property values
      //

      //force writeprop state back to false
      tmpView.WritePropState = false;

      //unload if necessary
      if (blnUnload)
      {
        tmpView.Unload();
      }
    }
    internal void LoadView(byte bytResNum, sbyte bytVol, int lngLoc)
    {
      //called by the resource loading method for the initial loading of
      //resources into logics collection
      //if this Logic number is already in the game
      if (agViews.Exists(bytResNum))
      {
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //create new logic object
      AGIView newResource = new AGIView();
      newResource.InGameInit(bytResNum, bytVol, lngLoc);
      //add it
      Col.Add(bytResNum, newResource);
      // update VOL and LOC
      newResource.Volume = bytVol;
      newResource.Loc = lngLoc;
    }
  }
}