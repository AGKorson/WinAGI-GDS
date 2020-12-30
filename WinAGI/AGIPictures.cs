using System;
using System.Collections.Generic;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using System.IO;

namespace WinAGI
{
  public class AGIPictures
  {
    public AGIPictures()
    {
      // create the initial Col object
      Col = new SortedList<byte, AGIPicture>();
    }
    internal SortedList<byte, AGIPicture> Col
    { get; private set; }
    public AGIPicture this[byte index]
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
      //check for thsi picture in the collection
      return Col.ContainsKey(ResNum);
    }
    public void Clear()
    {
      Col = new SortedList<byte, AGIPicture>();
    }
    public AGIPicture Add(byte ResNum, AGIPicture NewPicture = null)
    {
      //adds a new picture to a currently open game
      AGIPicture agResource;
      int intNextNum = 1;
      string strID, strBaseID;
      //if this Picture already exists
      if (Exists(ResNum))
      {
        //resource already exists
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //create new picture object
      agResource = new AGIPicture();
      //if an object was passed
      if ((NewPicture == null))
      {
        //proposed ID will be default
        strID = "Picture" + ResNum;
      }
      else
      {
        //copy entire picture
        agResource.SetPicture(NewPicture);
        //get proposed id
        strID = NewPicture.ID;
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

      //save new picture
      agResource.Save();

      //return the object created
      return agResource;
    }
    public void Remove(byte Index)
    {
      //removes a picture from the game file

      // if the resource exists
      if (Col.ContainsKey(Index))
      {
        //need to clear the directory file first
        UpdateDirFile(Col[Index], true);
        Col.Remove(Index);
        //remove all properties from the wag file
        DeleteSettingSection(agGameProps, "Picture" + Index);
      }
    }
    public void Renumber(byte OldPic, byte NewPic)
    {
      //renumbers a resource
      AGIPicture tmpPic;
      int intNextNum = 0;
      bool blnUnload = false;
      string strSection, strID, strBaseID;
      //if no change
      if (OldPic == NewPic)
      {
        return;
      }
      //verify new number is not in collection
      if (Col.Keys.Contains(NewPic))
      {
        //number already in use
        throw new Exception("669, LoadResString(669)");
      }
      //get picture being renumbered
      tmpPic = Col[OldPic];

      //if not loaded,
      if (!tmpPic.Loaded)
      {
        tmpPic.Load();
        blnUnload = true;
      }

      //remove old properties
      DeleteSettingSection(agGameProps, "Picture" + OldPic);

      //remove from collection
      Col.Remove(OldPic);

      //delete picture from old number in dir file
      //by calling update directory file method
      UpdateDirFile(tmpPic, true);

      //if id is default
      if (tmpPic.ID.Equals("Picture" + OldPic, StringComparison.OrdinalIgnoreCase))
      {
        //change default ID to new ID
        strID = strBaseID = "Picture" + NewPic;
        while (!IsUniqueResID(strID))
        {
          intNextNum++;
          strID = strBaseID + "_" + intNextNum;
        }
      }
      //change number
      tmpPic.Number = NewPic;

      //add with new number
      Col.Add(NewPic, tmpPic);

      //update new picture number in dir file
      UpdateDirFile(tmpPic);

      //add properties back with new picture number
      strSection = "Picture" + NewPic;
      WriteGameSetting(strSection, "ID", tmpPic.ID, "Pictures");
      WriteGameSetting(strSection, "Description", tmpPic.Description);
      //
      //TODO: add rest of default property values
      //

      //force writeprop state back to false
      tmpPic.WritePropState = false;

      //unload if necessary
      if (blnUnload)
      {
        tmpPic.Unload();
      }
    }
    internal void LoadPicture(byte bytResNum, sbyte bytVol, int lngLoc)
    {
      //called by the resource loading method for the initial loading of
      //resources into logics collection
      //if this Logic number is already in the game
      if (agPics.Exists(bytResNum))
      {
        throw new Exception("602, strErrSource, LoadResString(602)");
      }
      //create new logic object
      AGIPicture newResource = new AGIPicture();
      newResource.InGameInit(bytResNum, bytVol, lngLoc);
      //add it
      Col.Add(bytResNum, newResource);
    }
  }
}