using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Engine {
    public class InventoryItem {
        private string mItemName = "";
        private byte mRoom;
        private InventoryList mParent;
        
        /// <summary>
        /// 
        /// </summary>
        public InventoryItem() {
            // always unique until proven otherwise
            Unique = true;
        }

        /// <summary>
        /// A unique flag is used to identify objects that are unique to the list.
        /// If two or more objects have the same name, then all are flagged
        /// as NOT unique; that way the compilers and decompilers can handle
        /// the duplicate objects correctly.
        /// </summary>
        public bool Unique { get; internal set; }
        
        /// <summary>
        /// 
        /// </summary>
        public byte Room {
            get => mRoom;
            set {
                mRoom = value;
                //if there is a parent
                if (mParent is not null) {
                    mParent.IsDirty = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Parent"></param>
        internal void SetParent(InventoryList Parent) {
            //sets parent for this item; needed so we can update the item's unique property
            // when it changes
            mParent = Parent;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ItemName {
            get => mItemName;
            set {
                int i;
                // first, 'unduplicate' the current item name
                // then, assign the new item name, and then
                // re-check for duplicates of new name

                //if there is a parent
                if (mParent is not null) {
                    //if this item is currently a duplicate
                    if (!Unique) {
                        // there are at least two objects with this item name;
                        // this object and one or more duplicates
                        // if there is only one other duplicate, it needs to have its
                        // unique property reset because it will no longer be unique
                        // after this object is changed
                        // if there are multiple duplicates, the unique property does
                        // not need to be reset
                        byte dupItem = 0, dupCount = 0;
                        for (i = 0; i < mParent.Count; i++) {
                            if (mParent[(byte)i] != this) {
                                if (mItemName == mParent[(byte)i].ItemName) {
                                    //duplicate found- is this the second?
                                    if (dupCount == 1) {
                                        //the other's are still non-unique
                                        // so don't reset them
                                        dupCount = 2;
                                        break;
                                    }
                                    else {
                                        //set duplicate count
                                        dupCount = 1;
                                        //save dupitem number
                                        dupItem = (byte)i;
                                    }
                                }
                            }
                        }
                        // assume this item is now unique
                        Unique = true;
                        // if only one duplicate found
                        if (dupCount == 1) {
                            // also assume it's now unique
                            mParent[dupItem].Unique = true;
                        }
                    }
                }
                // assign name (blanks become "?"
                mItemName = value.Length == 0 ? "?" : value;
                if (mParent is not null) {
                    mParent.IsDirty = true;
                    if (mItemName != "?") {
                        // check for duplicates
                        for (i = 0; i < mParent.Count; i++) {
                            if (mParent[(byte)i] != this) {
                                if (mItemName == mParent[(byte)i].ItemName) {
                                    // mark both as NOT unique
                                    mParent[(byte)i].Unique = false;
                                    Unique = false;
                                    // no need to continue
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
