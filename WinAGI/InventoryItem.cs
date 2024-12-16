using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI.Engine {
    /// <summary>
    /// 
    /// </summary>
    public class InventoryItem {
        #region Local Members
        internal string mItemName = "";
        internal byte mRoom;
        private InventoryList mParent;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new inventory item that is not part of an inventory list.
        /// </summary>
        public InventoryItem() {
            mItemName = "?";
            mRoom = 0;
            Unique = true;
        }

        /// <summary>
        /// Initializes a new inventory item that is part of an inventory list.
        /// </summary>
        public InventoryItem(InventoryList parentlist) {
            mItemName = "?";
            mRoom = 0;
            Unique = true;
            mParent = parentlist;
        }
        #endregion

        #region Properties
        /// <summary>
        /// A unique flag is used to identify objects that are unique to the list.
        /// If two or more objects have the same name, then all are flagged
        /// as NOT unique; that way the compilers and decompilers can handle
        /// the duplicate objects correctly.
        /// </summary>
        public bool Unique { get; internal set; }
        
        /// <summary>
        /// Gets or sets the Room value associated with this inventory item. This corresponds
        /// to the 'room' field in the OBJECT file item entry for this item.
        /// </summary>
        public byte Room {
            get => mRoom;
            set {
                mRoom = value;
                if (mParent is not null) {
                    mParent.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Item Name value associated with this inventory item. This
        /// corresonds to the 'text' field in the OBJECT file for this inventory item.
        /// </summary>
        public string ItemName {
            get => mItemName;
            set {
                int i;
                // first, 'unduplicate' the current item name
                // then, assign the new item name, and then
                // re-check for duplicates of new name

                if (mParent is not null) {
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
                                    // duplicate found- is this the second?
                                    if (dupCount == 1) {
                                        // yes - the other's are still non-unique
                                        // so don't reset them
                                        dupCount = 2;
                                        break;
                                    }
                                    else {
                                        dupCount = 1;
                                        dupItem = (byte)i;
                                    }
                                }
                            }
                        }
                        // assume this item is now unique
                        Unique = true;
                        // if only one duplicate found
                        if (dupCount == 1) {
                            // also assume the duplicate is unique
                            mParent[dupItem].Unique = true;
                        }
                    }
                }
                // assign name (blanks become "?")
                mItemName = value.Length == 0 ? "?" : value;
                if (mParent is not null) {
                    mParent.IsChanged = true;
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
        #endregion

        #region Methods
        // none
        #endregion
    }
}
