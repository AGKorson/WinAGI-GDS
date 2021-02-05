using System;
using System.Collections.Generic;
using System.Collections;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.AGICommands;
using static WinAGI.Common.WinAGI;
using System.IO;

namespace WinAGI.Engine
{
  public class AGIInventoryObjects : IEnumerable<AGIInventoryItem>
  {
    byte mMaxScreenObjects;
    bool mEncrypted;
    bool mAmigaOBJ;
    string mResFile = "";
    string mDescription = "";
    bool mInGame;
    bool mIsDirty;
    bool mWriteProps;
    bool mLoaded;
    bool mLoading;
    AGIGame parent = null;
    //other
    string strErrSource = "";
    public AGIInventoryObjects()
    {
      mInGame = false;
      mResFile = "";
      InitInvObj();
    }
    public AGIInventoryObjects(AGIGame parent, bool Loaded = false) 
    {
      this.parent = parent;
      mInGame = true;
      //if loaded property is passed, set loaded flag as well
      mLoaded = Loaded;
      //set resourcefile to game default
      mResFile = parent.agGameDir + "OBJECT";
      InitInvObj();
    }
    private void InitInvObj()
    {
      // create the initial Col object
      mItems = new List<AGIInventoryItem>();
      AGIInventoryItem tmpItem;
      strErrSource = "WINAGI.agiObjectFile";
      mMaxScreenObjects = 16;
      //add placeholder for item 0
      tmpItem = new AGIInventoryItem();
      tmpItem.ItemName = "?";
      tmpItem.Room = 0;
      tmpItem.SetParent(this);
      mItems.Add(tmpItem);
    }
    internal List<AGIInventoryItem> mItems
    { get; private set; }
    public AGIInventoryItem this[byte index]
    { get { return mItems[index]; } }
    public byte Count
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //although first object does not Count as an object
        //it must be returned as part of the Count
        //so everything works properly
        return (byte)mItems.Count;
      }
      private set
      {
      }
    }
    public bool AmigaOBJ
    {
      get
      {
        return mAmigaOBJ;
      }
      set
      {
        //for now, we only allow converting FROM Amiga TO DOS
        //                                        (T)     (F)
        //if trying to make it Amiga, exit
        if (value)
        {
          return;
        }
        //if aready DOS, exit
        if (!mAmigaOBJ)
        {
          return;
        }
        //set the flag to be NON-Amiga
        mAmigaOBJ = value;
        //save the current file as 'OBJECT.amg'
        string theDir;
        if (parent == null) {
          theDir = JustPath(mResFile);
        }
        else {
          theDir = parent.agGameDir;
        }
        try
        {
          if (File.Exists(theDir + "OBJECT.amg")) ;
          {
            File.Delete(theDir + "OBJECT.amg");
          }
          File.Move(parent.agGameDir + "OBJECT", theDir + "OBJECT.amg");

          //now delete the current file
          File.Delete(theDir + "OBJECT");
          //mark it as dirty, and save it to create a new file
          mIsDirty = true;
          Save();
        }
        catch (Exception)
        {
          throw new Exception("error during Amiga conversion");
        }
      }
    }
    public void NewObjects()
    {
      //marks the resource as loaded; this is needed so new 
      //resources can be created and edited
      //if already loaded
      if (mLoaded)
      {
        //error
        throw new Exception("642, strErrSource, LoadResString(642)");
      }
      //can't call NewResource if already in a game;
      //clear it instead
      if (mInGame)
      {
        throw new Exception("510, strErrSource, LoadResString(510)");
      }

      //mark as loaded
      mLoaded = true;

      //clear resname and description
      mResFile = "";
      mDescription = "";
      mMaxScreenObjects = 16;
      mEncrypted = true;

      //use clear method to ensure object list is reset
      Clear();
    }
    public string Description
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mDescription;
      }
      set
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }

        //limit description to 1K
        value = Left(value, 1024);
        //if changing
        if (value != mDescription)
        {
          mDescription = value;
          //if in a game
          if (mInGame)
          {
            parent.WriteProperty("OBJECT", "Description", mDescription);
          }
        }
      }
    }
    public void Export(string ExportFile, int FileType, bool ResetDirty = true)
    {
      //exports the list of inventory objects
      //  filetype = 0 means AGI OBJECT file
      //  filetype = 1 means WinAGI object list file

      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      try
      {
        switch (FileType)
        {
          case 0:  //compile agi OBJECT file
            Compile(ExportFile);
            break;
          case 1:  //create WinAGI Object list
            CompileWinAGI(ExportFile);
            break;
        }
      }
      catch (Exception)
      {
        //return error condition
        throw new Exception("582, strErrSource, LoadResString(582)");
      }
      //if NOT in a game,
      if (!mInGame)
      {
        if (ResetDirty)
        {
          //clear dirty flag
          mIsDirty = false;
        }
        //save filename
        mResFile = ExportFile;
      }
    }
    public void CompileWinAGI(string CompileFile)
    {
      /*
  'items are saved with the description and room number
  'of each item (separated by a tab character) on a separate line
  'first line is version identifier
  'file description is saved at end of words
  
  string strTempFile
  AGIInventoryItem tmpItem
  
  On Error GoTo ErrHandler
  
  'if no name
  if (LenB(CompileFile) == 0) {
    On Error GoTo 0
    'raise error
    throw new Exception("615, strErrSource, LoadResString(615)
    return;
  }
  
  'get temporary file
  strTempFile = Path.GetTempFileName();
  
  'open file for output
  intFile = FreeFile()
  Open strTempFile output
  'print version
  Print #intFile, WINAGI_VERSION
  
  'print max screen objects
  Print #intFile, mMaxScreenObjects
  
  'print item description and room for each object
  foreach (tmpItem In Me
    Print #intFile, tmpItem.ItemName + vbTab + CStr(tmpItem.Room)
  Next
  
  'if there is a description
  if (LenB(mDescription) != 0) {
    'print eof marker
    Print #intFile, Chr$(255) + Chr$(255)
    'print description
    Print #intFile, mDescription
  }
  
  'close file
  Close intFile
  
  'if CompileFile exists
  if (File.Exists(CompileFile)) {
    'delete it
    Kill CompileFile
    Err.Clear
  }
  
  'copy tempfile to CompileFile
  FileCopy strTempFile, CompileFile
  
  'delete temp file
  Kill strTempFile
  Err.Clear
  
  'if not in a game,
  if (!mInGame) {
    'change resfile
    mResFile = CompileFile
    'mark as clean
    mIsDirty = false
  }
return;

ErrHandler:
  'close file
  Close intFile
  'erase the temp file
  Kill CompileFile
  Err.Clear
  'return error condition
  throw new Exception("582, strErrSource, LoadResString(582)
      */
    }
    public bool InGame
    {
      get
      {
        //only used by  setobjects method
        return mInGame;
      }
      internal set
      {
        mInGame = value;
      }
    }
    public bool IsDirty
    {
      get
      {
        //if resource is dirty, or (prop values need writing AND in game)
        return (mIsDirty || (mWriteProps && mInGame));
      }
      set
      {
        mIsDirty = value;
      }
    }
    internal bool WriteProps
    {
      get { return mWriteProps; }
    }
    public bool Loaded
    {
      get
      {
        return mLoaded;
      }
    }
    public string ResFile
    {
      get
      {
        return mResFile;
      }
      set
      {
        //resfile cannot be changed if resource is part of a game
        if (mInGame)
        {
          //error- resfile is readonly for ingame resources
          throw new Exception("680, strErrSource, LoadResString(680)");
        }
        else
        {
          mResFile = value;
        }
      }
    }
    internal void SetFlags(ref bool IsDirty, ref bool WriteProps)
    {
      // used when copying a resource?
      IsDirty = mIsDirty;
      WriteProps = mWriteProps;
    }
    bool LoadSierraFile(string LoadFile)
    {
      //attempts to load a sierra OBJECT file
      //rturns true if successful

      string strItem;
      int intItem;
      byte bytRoom;
      int rtn;
      byte[] bytData = Array.Empty<byte>();
      byte bytLow, bytHigh;
      int lngDataOffset, lngNameOffset;
      int lngPos, Dwidth;
      //open the file
      FileStream fsObj = new FileStream(LoadFile, FileMode.Open);
      //if no data,
      if (fsObj.Length == 0)
      {
        fsObj.Dispose();
        return false;
      }
      //set load flag
      mLoaded = true;
      //read in entire resource
      Array.Resize(ref bytData, (int)fsObj.Length);
      fsObj.Read(bytData);
      fsObj.Dispose();
      //determine if file is encrypted or clear
      rtn = IsEncrypted(bytData[bytData.Length - 1], bytData.Length - 1);
      switch (rtn)
      {
        case 0:  //unencrypted
          mEncrypted = false;
          break;
        case 1: //encrypted
                //unencrypt file
          ToggleEncryption(ref bytData);
          //set flag
          mEncrypted = true;
          break;
        case 2: //error
                //error in object file- return false
          mLoaded = false;
          return false;
      }
      //set loading flag (avoids recursing and stuff like that)
      mLoading = true;
      //PC files always have data element widths of 3 bytes;
      //Amigas have four; need to make sure correct value is used
      Dwidth = 3;
      do
      {
        //get offset to start of string data
        bytLow = bytData[0];
        bytHigh = bytData[1];
        lngDataOffset = bytHigh * 256 + bytLow + Dwidth;

        //first char of first item should be a question mark ('?')
        if (bytData[lngDataOffset] != 63)
        {
          //try again, with four
          Dwidth++;
        }
        else
        {
          //correct file type found
          mAmigaOBJ = (Dwidth == 4);
          break;
        }
      }
      while (Dwidth <= 4);
      //dwidth will be 5 if valid item data not found
      if (Dwidth == 5)
      {
        //error
        mLoading = false;
        return false;
      }
      //max scrn objects is always at position 2
      mMaxScreenObjects = bytData[2];
      //set counter to zero, and extract item info
      intItem = 0;
      //extract each item offset, and then its string data
      //(intItem*3) is offset address of string data;
      //stop if this goes past beginning of actual data
      do
      {
        //extract and build offset to string data
        bytLow = bytData[Dwidth + intItem * Dwidth];
        bytHigh = bytData[Dwidth + intItem * Dwidth + 1];
        lngNameOffset = bytHigh * 256 + bytLow + Dwidth;
        //get room number for this object
        bytRoom = bytData[Dwidth + intItem * Dwidth + 2];
        //set pointer to beginning of name string data
        lngPos = lngNameOffset;
        //if past end of resource,
        if (lngPos > bytData.Length)
        {
          //error in object file- return false
          mLoading = false;
          return false;
        }
        //build item name string
        strItem = "";
        while (lngPos < bytData.Length) 
        {
          if (bytData[lngPos] == 0)
          {
            break;
          }
          strItem += ((char)bytData[lngPos]).ToString();
          lngPos++;
        }
        //first item IS USUALLY a '?'  , but NOT always
        //(See MH2 for example!!!!)
        if (intItem == 0)
        {
          if (strItem != "?")
          {
            //rename first object
            mItems[0].ItemName = strItem;
            mItems[0].Room = bytRoom;
            //***and if game is being imported, make a note of this!
          }
          else
          {
            //skip it - '?' is already added by default
          }
          //dont add first item; it is already added
        }
        else
        {
          //add without key (to avoid duplicate key error)
          Add(strItem, bytRoom);
        }
        intItem++;
      }
      while (((intItem * Dwidth) + Dwidth < lngDataOffset) && (intItem < MAX_ITEMS)); //Until ((intItem * Dwidth) + Dwidth >= lngDataOffset) || (intItem >= MAX_ITEMS)

      //reset loading flag
      mLoading = false;
      return true;
    }
    internal bool LoadWinAGIFile(string LoadFile)
    {
      //attempts to load a winAGI file
      //if unsuccessful, returns false
      string strInput, strItem = "", strRoom;
      byte bytRoom;
      int lngPos;
      //attempt to open the WinAGI resource
      FileStream fsObj;
      StreamReader srObj;
      try
      {
        //attempt to open the WinAGI resource
        fsObj = new FileStream(LoadFile, FileMode.Open);
        srObj = new StreamReader(fsObj);
      }
      catch (Exception)
      {
        //ignore and fail
        return false;
      }
      //first line should be loader
      strInput = srObj.ReadLine();
      switch (strInput)
      {
        case WINAGI_VERSION:
        case WINAGI_VERSION_1_2:
        case WINAGI_VERSION_1_0:
        case WINAGI_VERSION_BETA:
          //ok
          break;
        default:
          //any 1.2.x is ok
          if (Left(strInput, 4) != "1.2.")
          {
            //close file
            fsObj.Dispose();
            srObj.Dispose();
            return false;
          }
          break;
      }
      //never encrypt
      mEncrypted = false;
      //get max screen objects from first line
      strInput = srObj.ReadLine();
      //if within bounds
      if (Val(strInput) > 255)
      {
        mMaxScreenObjects = 255;
      }
      else if (Val(strInput) < 1)
      {
        mMaxScreenObjects = 1;
      }
      else
      {
        mMaxScreenObjects = (byte)Val(strInput);
      }
      //set loading flag
      mLoading = true;
      //read in each item
      while (!srObj.EndOfStream)
      {
        //get input string
        strInput = srObj.ReadLine();
        //check for end of input characters
        if (strInput == ((char)255 + (char)255).ToString())
        {
          break;
        }
        //check for tab character
        lngPos = strInput.IndexOf('\t');
        //if tab character found
        if (lngPos >= 0)
        {
          //strip off room number
          strRoom = Right(strInput, strInput.Length - lngPos);
          strItem = Left(strInput, lngPos - 1);
          //convert to byte Value
          if (Val(strRoom) > 255)
          {
            bytRoom = 255;
          }
          else if (Val(strRoom) < 0)
          {
            bytRoom = 0;
          }
          else
          {
            bytRoom = (byte)Val(strRoom);
          }
        }
        else
        {
          //no room included; assume zero
          bytRoom = 0;
        }
        //add without key (to avoid duplicate key error)
        Add(strItem, bytRoom);
      } //loop

      //if any lines left
      if (!srObj.EndOfStream)
      {
        //get first remaining line
        strItem = srObj.ReadLine();
        //if there are additional lines, add them as a description
        while (!srObj.EndOfStream)
        {
          strInput = srObj.ReadLine();
          strItem += NEWLINE + strInput;
        }
        //save description
        mDescription = strItem;
      }
      //close file
      fsObj.Dispose();
      srObj.Dispose();
      //if no objects loaded
      if (mItems.Count == 0)
      {
        //add default object
        Add("?", 0);
      }
      //clear loading flag
      mLoading = false;
      //set loaded flag
      mLoaded = true;
      return true;
    }
    public AGIInventoryItem Add(string NewItem, byte Room)
    {
      //adds new item to object list
      AGIInventoryItem tmpItem;
      //if not currently loading, or not already loaded
      if (!mLoading && !mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //if already have max number of items,
      if (mItems.Count == MAX_ITEMS)
      {
        //error
        throw new Exception("569, LoadResString(569)");
      }
      //add the item
      tmpItem = new AGIInventoryItem();
      //set parent first, so duplicate protection works
      tmpItem.SetParent(this);
      tmpItem.ItemName = NewItem;
      tmpItem.Room = Room;
      mItems.Add(tmpItem);
      //set dirty flag
      mIsDirty = true;
      return tmpItem;
    }
    public void Remove(byte Index)
    {
      //removes this from the object list
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      AGIInventoryItem tmpItem = mItems[Index];

      //if this item is currently a duplicate, 
      // need to de-unique-ify this item
      if (!tmpItem.Unique)
      {
        //there are at least two objects with this item name;
        //this object and one or more duplicates
        //if there is only one other duplicate, it needs to have its
        //unique property reset because it will no longer be unique
        //after this object is changed
        //if there are multiple duplicates, the unique property does
        //not need to be reset
        byte dupItem = 0, dupCount = 0;
        for (int i = 0; i < mItems.Count; i++)
        {
          if (mItems[(byte)i] != tmpItem)
          {
            if (tmpItem.ItemName.Equals(mItems[(byte)i].ItemName, StringComparison.OrdinalIgnoreCase))
            {
              //duplicate found- is this the second?
              if (dupCount == 1)
              {
                //the other's are still non-unique
                // so don't reset them
                dupCount = 2;
                break;
              }
              else
              {
                //set duplicate count
                dupCount = 1;
                //save dupitem number
                dupItem = (byte)i;
              }
            }
          }
        }
        // if only one duplicate found
        if (dupCount == 1)
        {
          // set unique flag for that item; it's no longer a duplicate
          mItems[dupItem].Unique = true;
        }
      }
      //if this is the last item (but not FIRST item)
      if (Index == mItems.Count - 1 && mItems.Count > 1)
      {
        //remove the item
        mItems.RemoveAt(Index);
      }
      else
      {
        //set item to '?'
        mItems[Index].Unique = true;
        mItems[Index].ItemName = "?";
        mItems[Index].Room = 0;
      }
      //set dirty flag
      mIsDirty = true;
    }
    public bool Encrypted
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mEncrypted;
      }
      set
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //if change in encryption
        if (mEncrypted != value)
        {
          //set dirty flag
          mIsDirty = true;
        }
        mEncrypted = value;
      }
    }
    public byte MaxScreenObjects
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mMaxScreenObjects;
      }
      set
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //if change in max objects
        if (value != mMaxScreenObjects)
        {
          mMaxScreenObjects = value;
          mIsDirty = true;
        }
      }
    }
    public void Load(string LoadFile = "")
    {
      //this function loads inventory objects for the game
      //if loading from a Sierra game, it extracts items
      //from the OBJECT file;
      //if not in a game, LoadFile must be specified

      //if already loaded,
      if (mLoaded)
      {
        //error- resource already loaded
        throw new Exception("511, strErrSource, LoadResString(511)");
      }
      //if in a game
      if (mInGame)
      {
        //use default Sierra name
        LoadFile = parent.agGameDir + "OBJECT";
        //attempt to load
        if (!LoadSierraFile(LoadFile))
        {
          //reset objects resource using Clear method
          Clear();
          //error
          throw new Exception("692, strErrSource, LoadResString(692)");
        }
        //get description, if there is one
        mDescription = parent.agGameProps.GetSetting("OBJECT", "Description", "");
      }
      else
      {
        //if NOT in a game, file must be specified
        if (LoadFile.Length == 0)
        {
          //no file specified; return error
          throw new Exception("599, strErrSource, LoadResString(599)");
        }
        //verify file exists
        if (!File.Exists(LoadFile))
        {
          //error
          throw new Exception("524, strErrSource, Replace(LoadResString(524), ARG1, LoadFile)");
        }
        //if extension is .ago then
        if ((Right(LoadFile, 4)).ToLower() == ".ago")
        {
          //assume winagi format
          if (!LoadWinAGIFile(LoadFile))
          {
            //try sierra format
            if (!LoadSierraFile(LoadFile))
            {
              //error
              throw new Exception("692, strErrSource, LoadResString(692)");
            }
          }
        }
        else
        {
          //assume sierra format
          if (!LoadSierraFile(LoadFile))
          {
            //try winagi format
            if (!LoadWinAGIFile(LoadFile))
            {
              //error
              throw new Exception("692, strErrSource, LoadResString(692)");
            }
          }
        }
        //save filename
        mResFile = LoadFile;
      }
      //reset dirty flag
      mIsDirty = false;
    }
    int IsEncrypted(byte bytLast, int lngEndPos)
    {
      //this routine checks the resource to determine if it is
      //encrypted with the string, "Avis Durgan"
      //it does this by checking the last byte in the resource. It should ALWAYS
      //be Chr$(0):
      //   if the resource is NOT encrypted,
      //     the last byte will have a Value of 0x00
      //     the function will return a Value of 0
      //
      //   if it IS encrypted,
      //     it will be a character from the "Avis Durgan"
      //     string, dependent on the offset of the last
      //     byte from a multiple of 11 (the length of "Avis Durgan")
      //     the function will return a Value of 1
      //if the last character doesn't properly decrypt to Chr$(0)
      //the function returns an error Value (2)
      //Note: the last byte is passed by Value so it can be temporarily
      //modified during the check

      //if it's a zero,
      if (bytLast == 0)
      {
        //return unencrypted
        return 0;
      }
      else
      {
        //decrypt character
        bytLast = (byte)(bytLast ^ bytEncryptKey[lngEndPos % 11]);
        //now, it should be zero
        if (bytLast == 0)
        {
          return 1;
        }
        else
        {
          return 2;
        }
      }
    }
    public void Save(string SaveFile = "", int FileType = 0)
    {
      //saves the list of inventory objects
      //  filetype = 0 means AGI OBJECT file
      //  filetype = 1 means WinAGI object list file
      //for ingame resources, SaveFile and filetype are ignored

      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //if in a game,
      if (mInGame)
      {
        //compile the file
        Compile(mResFile);
        //change date of last edit
        parent.agLastEdit = DateTime.Now;
      }
      else
      {
        if (SaveFile.Length == 0)
        {
          SaveFile = mResFile;
        }
        //if still no file
        if (SaveFile.Length == 0)
        {
          throw new Exception("615, strErrSource, LoadResString(615)");
        }

        switch (FileType)
        {
          case 0:  //compile agi OBJECT file
            Compile(SaveFile);
            break;
          case 1:  //create WinAGI Object list
            CompileWinAGI(SaveFile);
            break;
        }

        //save filename
        mResFile = SaveFile;
      }
      //mark as clean
      mIsDirty = false;
    }
    public void SetObjects(AGIInventoryObjects NewObjects)
    {
      //copies object list from NewObjects to
      //this object list
      int i;
      //if source objectlist is not loaded
      if (!NewObjects.Loaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //first, clear current list
      Clear();
      //first item of new objects should normally be a '?'
      //add all objects EXCEPT for first //?// object
      //which is already preloaded with the clear method
      for (i = 1; i < NewObjects.Count; i++)
      {
        Add(NewObjects[(byte)i].ItemName, NewObjects[(byte)i].Room);
      }
      //RARE, but check for name/room change to item 0
      if (NewObjects[0].ItemName != "?")
      {
        this[0].ItemName = NewObjects[0].ItemName;
      }
      if (NewObjects[0].Room != 0)
      {
        this[0].Room = NewObjects[0].Room;
      }
      //set max screenobjects
      mMaxScreenObjects = NewObjects.MaxScreenObjects;
      //set encryption flag
      mEncrypted = NewObjects.Encrypted;
      //set description
      mDescription = NewObjects.Description;
      //set dirty flags
      mIsDirty = NewObjects.IsDirty;
      mWriteProps = NewObjects.WriteProps;
      //set filename
      mResFile = NewObjects.ResFile;
      //set load status
      mLoaded = true;
    }
    void ToggleEncryption(ref byte[] bytData)
    {  // this function encrypts/decrypts the data
       // by XOR'ing it with the encryption string

      for (int lngPos = 0; lngPos < bytData.Length; lngPos++)
      {
        bytData[lngPos] = (byte)(bytData[lngPos] ^ bytEncryptKey[lngPos % 11]);
      }
    }
    public void Unload()
    {  //unloads ther resource; same as clear, except file marked as not dirty

      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      Clear();
      mLoaded = false;
      mWriteProps = false;
      mIsDirty = false;
    }
    public void Clear()
    {
      //clears ALL objects and sets default values
      //adds placeholder for item 0

      AGIInventoryItem tmpItem;
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      mEncrypted = false;
      mMaxScreenObjects = 16;
      mAmigaOBJ = false;
      mDescription = "";
      mItems = new List<AGIInventoryItem>();
      //add the placeholder
      tmpItem = new AGIInventoryItem();
      tmpItem.ItemName = "?";
      tmpItem.Room = 0;
      mItems.Add(tmpItem);
      //but don't set parent; otherwise
      //circular object reference is created
      tmpItem.SetParent(this);
      //set dirty flag
      mIsDirty = true;
    }
    void Compile(string CompileFile)
    {
      //compiles the object list into a Sierra AGI compatible OBJECT file

      int lngFileSize;    //size of object file
      int CurrentChar;    //current character in name
      int lngDataOffset;  //start of item names
      int lngPos;         //position in current item name
      byte[] bytTemp = Array.Empty<byte>();
      string strTempFile = "";
      byte bytLow, bytHigh;
      int i;
      int Dwidth;
      //if no file
      if (CompileFile.Length == 0)
      {
        throw new Exception("616, strErrSource, LoadResString(616)");
      }

      //if not dirty AND compilefile=resfile
      if (!mIsDirty && CompileFile.Equals(mResFile, StringComparison.OrdinalIgnoreCase))
      {
        return;
      }

      //PC version (most common) has 3 bytes per item in offest table; amiga version has four bytes per item
      if (mAmigaOBJ)
      {
        Dwidth = 4;
      }
      else
      {
        Dwidth = 3;
      }
      //calculate min filesize
      //(offset table size plus header + null obj '?')
      lngFileSize = (mItems.Count + 1) * Dwidth + 2;
      //step through all items to determine length of each, and add it to file length counter
      foreach (AGIInventoryItem tmpItem in mItems)
      {
        //if this item is NOT "?"
        if (tmpItem.ItemName != "?")
        {
          //add size of object name to file size
          //(include null character at end of string)
          lngFileSize += tmpItem.ItemName.Length + 1;
        }
      }
      //initialize byte array to final size of file
      Array.Resize(ref bytTemp, lngFileSize);
      //set offset from index to start of string data
      lngDataOffset = mItems.Count * Dwidth;
      bytHigh = (byte)(lngDataOffset / 256);
      bytLow = (byte)(lngDataOffset % 256);
      //write offest for beginning of text data
      bytTemp[0] = bytLow;
      bytTemp[1] = bytHigh;
      //write max number of screen objects
      bytTemp[2] = mMaxScreenObjects;
      //increment offset by width (to take into account file header)
      //this is also pointer to the null item
      lngDataOffset += Dwidth;
      //set counter to beginning of data
      lngPos = lngDataOffset;
      //write string for null object (?)
      bytTemp[lngPos] = 63; //(?)
      bytTemp[lngPos + 1] = 0;
      lngPos += 2;
      //now step through all items
      for (i = 1; i < mItems.Count - 1; i++)
      {
        //if object is //?//
        if (mItems[i].ItemName == "?")
        {
          //write offset data to null item
          bytTemp[i * Dwidth] = (byte)(lngDataOffset % 256);
          bytTemp[i * Dwidth + 1] = (byte)(lngDataOffset / 256);
          //set room number for this object
          bytTemp[i * Dwidth + 2] = mItems[i].Room;
        }
        else
        {
          //write offset data for start of this word
          //subtract data element width because offset is from end of header,
          //not beginning of file; lngPos is referenced from position zero)
          bytHigh = (byte)((lngPos - Dwidth) / 256);
          bytLow = (byte)((lngPos - Dwidth) % 256);
          bytTemp[i * Dwidth] = bytLow;
          bytTemp[i * Dwidth + 1] = bytHigh;
          //write room number for this object
          bytTemp[i * Dwidth + 2] = mItems[i].Room;
          //write all characters of this object
          for (CurrentChar = 0; CurrentChar < mItems[i].ItemName.Length; CurrentChar++)
          {
            bytTemp[lngPos] = (byte)mItems[i].ItemName[CurrentChar];
            lngPos++;
          }
          //add null character to end
          bytTemp[lngPos] = 0;
          lngPos++;
        }
      }
      //reduce array to actual size
      Array.Resize(ref bytTemp, lngPos);

      //if file is to be encrypted
      if (mEncrypted)
      {
        //step through entire file
        for (lngPos = 0; lngPos < bytTemp.Length; i++)
        {
          //encrypt with //Avis Durgan//
          bytTemp[lngPos] ^= bytEncryptKey[lngPos % 11];
        }
      }
      try
      {
        //open temp file
        strTempFile = Path.GetTempFileName();
        //write the data to file
        FileStream fsObj = new FileStream(strTempFile, FileMode.Open);
        fsObj.Write(bytTemp, 0, bytTemp.Length);
        fsObj.Dispose();
        //if savefile already exists
        if (File.Exists(CompileFile))
        {
          //delete it
          File.Delete(CompileFile);
        }
        //move tempfile to savefile
        File.Move(strTempFile, CompileFile);
      }
      catch (Exception)
      {
        //delete temporary file
        File.Delete(strTempFile);
        //raise the error
        throw new Exception("674, strErrSrc, Replace(LoadResString(674)");
      }
    }
    ItemEnum GetEnumerator()
    {
      return new ItemEnum(mItems);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }
    IEnumerator<AGIInventoryItem> IEnumerable<AGIInventoryItem>.GetEnumerator()
    {
      return (IEnumerator<AGIInventoryItem>)GetEnumerator();
    }
  }
  internal class ItemEnum : IEnumerator<AGIInventoryItem>
  {
    public List<AGIInventoryItem> _invitems;
    int position = -1;
    public ItemEnum(List<AGIInventoryItem> list)
    {
      _invitems = list;
    }
    object IEnumerator.Current => Current;
    public AGIInventoryItem Current
    {
      get
      {
        try
        {
          return _invitems[position];
        }
        catch (IndexOutOfRangeException)
        {

          throw new InvalidOperationException();
        }
      }
    }
    public bool MoveNext()
    {
      position++;
      return (position < _invitems.Count);
    }
    public void Reset()
    {
      position = -1;
    }
    public void Dispose()
    {
      _invitems = null;
    }
  }
}