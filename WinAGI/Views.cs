using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIGame;
using System.IO;

namespace WinAGI.Engine
{
  public class Views : IEnumerable<View>
  {
    AGIGame parent;
    public Views(AGIGame parent)
    {
      this.parent = parent;
      // create the initial Col object
      Col = new SortedList<byte, View>();
    }
    internal SortedList<byte, View> Col
    { get; private set; }
    public View this[int index]
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
      //check for thsi view in the collection
      return Col.ContainsKey(ResNum);
    }
    public void Clear()
    {
      Col = new SortedList<byte, View>();
    }
    public View Add(byte ResNum, View NewView = null)
    {
      //adds a new view to a currently open game
      View agResource;
      int intNextNum = 1;
      string strID, strBaseID;
      //if this view already exists
      if (Exists(ResNum)) {
        //resource already exists

        Exception e = new(LoadResString(602))
        {
          HResult = 602
        };
        throw e;
      }
      //if no object was passed
      if ((NewView == null)) {
        //create new view resource
        agResource = new View();
        //proposed ID will be default
        strID = "View" + ResNum;
      }
      else {
        //clone the passed view
        agResource = NewView.Clone();

        //get proposed id
        strID = NewView.ID;
      }
      // validate id
      strBaseID = strID;
      while (!NewView.IsUniqueResID(strID)) {
        intNextNum++;
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
      if (Col.ContainsKey(Index)) {
        //need to clear the directory file first
        UpdateDirFile(Col[Index], true);
        Col.Remove(Index);
        //remove all properties from the wag file
        parent.agGameProps.DeleteSection("View" + Index);
      }
    }
    public void Renumber(byte OldView, byte NewView)
    {
      //renumbers a resource
      View tmpView;
      int intNextNum = 0;
      bool blnUnload = false;
      string strSection, strID, strBaseID;
      //if no change
      if (OldView == NewView) {
        return;
      }
      //verify new number is not in collection
      if (Col.Keys.Contains(NewView)) {
        //number already in use

        Exception e = new(LoadResString(669))
        {
          HResult = 669
        };
        throw e;
      }
      //get view being renumbered
      tmpView = Col[OldView];

      //if not loaded,
      if (!tmpView.Loaded) {
        tmpView.Load();
        blnUnload = true;
      }

      //remove old properties
      parent.agGameProps.DeleteSection("View" + OldView);

      //remove from collection
      Col.Remove(OldView);

      //delete view from old number in dir file
      //by calling update directory file method
      UpdateDirFile(tmpView, true);

      //if id is default
      if (tmpView.ID.Equals("View" + OldView, StringComparison.OrdinalIgnoreCase)) {
        //change default ID to new ID
        strID = strBaseID = "View" + NewView;
        while (!tmpView.IsUniqueResID(strID)) {
          intNextNum++;
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
      parent.WriteGameSetting(strSection, "ID", tmpView.ID, "Views");
      parent.WriteGameSetting(strSection, "Description", tmpView.Description);
      //
      //TODO: add rest of default property values
      //

      //force writeprop state back to false
      tmpView.WritePropState = false;

      //unload if necessary
      if (blnUnload) {
        tmpView.Unload();
      }
    }
    internal void LoadView(byte bytResNum, sbyte bytVol, int lngLoc)
    {
      //called by the resource loading method for the initial loading of
      //resources into logics collection
      //if this view number is already in the game
      if (Exists(bytResNum)) {

        Exception e = new(LoadResString(602))
        {
          HResult = 0
        };
        throw e;
      }
      //create new logic object
      View newResource = new View(parent, bytResNum, bytVol, lngLoc);
      //add it
      Col.Add(bytResNum, newResource);
      // update VOL and LOC
      newResource.Volume = bytVol;
      newResource.Loc = lngLoc;
    }
    ViewEnum GetEnumerator()
    {
      return new ViewEnum(Col);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }
    IEnumerator<View> IEnumerable<View>.GetEnumerator()
    {
      return (IEnumerator<View>)GetEnumerator();
    }
  }
  internal class ViewEnum : IEnumerator<View>
  {
    public SortedList<byte, View> _views;
    int position = -1;
    public ViewEnum(SortedList<byte, View> list)
    {
      _views = list;
    }
    object IEnumerator.Current => Current;
    public View Current
    {
      get
      {
        try {
          return _views.Values[position];
        }
        catch (IndexOutOfRangeException) {

          throw new InvalidOperationException();
        }
      }
    }
    public bool MoveNext()
    {
      position++;
      return (position < _views.Count);
    }
    public void Reset()
    {
      position = -1;
    }
    public void Dispose()
    {
      _views = null;
    }
  }
}