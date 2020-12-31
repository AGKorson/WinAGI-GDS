using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
  public class AGIInventoryItem
  {
    private string mItemName = "";
    private byte mRoom;
    //unique flag used to identify objects that are unique to the list
    //if two or more objects have the same name, then all are flagged
    //as NOT unique; that way the compilers and decompilers can handle
    //the duplicate objects correctly
    private bool mUnique;
    private AGIInventoryObjects mParent;
    public AGIInventoryItem()
    {
      //always unique until proven otherwise
      mUnique = true;
    }
    public bool Unique
    {
      get
      {
        return mUnique;
      }
      internal set
      {
        mUnique = value;
      }
    }
    public byte Room
    {
      get
      {
        return mRoom;
      }
      set
      {
        mRoom = value;

        //if there is a parent
        if (mParent != null)
        {
          mParent.IsDirty = true;
        }
      }
    }
    internal void SetParent(AGIInventoryObjects Parent)
    {
      //sets parent for this item
      mParent = Parent;
    }
    public string ItemName
    {
      get
      {
        return mItemName;
      }
      set
      {
        int i;
        // first, 'unduplicate' the current item name
        // then, assign the new item name, and then
        // re-check for duplicates of new name

        //if there is a parent
        if (mParent != null)
        {
          //if this item is currently a duplicate
          if (!mUnique)
          {
            //there are at least two objects with this item name;
            //this object and one or more duplicates
            //if there is only one other duplicate, it needs to have its
            //unique property reset because it will no longer be unique
            //after this object is changed
            //if there are multiple duplicates, the unique property does
            //not need to be reset
            int lngDupItem = 0, lngDupCount = 0;
            for (i = 0; i < mParent.Count; i++)
            {
              if (mParent[(byte)i] != this)
              {
                if (mItemName.Equals(mParent[(byte)i].ItemName, StringComparison.OrdinalIgnoreCase))
                {
                  //duplicate found- is this the second?
                  if (lngDupCount == 1)
                  {
                    //the other's are still non-unique
                    // so don't reset them
                    lngDupCount = 2;
                    break;
                  }
                  else
                  {
                    //set duplicate count
                    lngDupCount = 1;
                    //save dupitem number
                    lngDupItem = i;
                  }
                }
              }
            }
            //set the unique flag for this object
            mUnique = true;
            // if only one duplicate found
            if (lngDupCount == 1)
            {
              // set unique flag for that object too
              mParent[(byte)lngDupItem].Unique = true;
            }
          }
        }
        //assign name
        mItemName = value;
        //if blank,
        if (mItemName.Length == 0)
        {
          //set it to '?'
          mItemName = "?";
        }
        //if there is a parent
        if (mParent != null)
        {
          mParent.IsDirty = true;
          //if this item is NOT an unassigned object ('?')
          if (mItemName != "?")
          {
            //check for duplicates
            for (i = 0; i < mParent.Count; i++)
            {
              //skip this item
              if (mParent[(byte)i] != this)
              {
                if (mItemName.Equals(mParent[(byte)i].ItemName, StringComparison.OrdinalIgnoreCase))
                  {
                  //mark both as NOT unique
                  mParent[(byte)i].Unique = false;
                  mUnique = false;
                }
              }
            }
          }
        }
      }
    }
  }
}
