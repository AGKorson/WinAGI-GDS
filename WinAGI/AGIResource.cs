using System;
using System.ComponentModel;
using System.IO;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;

namespace WinAGI
{
  //public abstract class AGIResource
  public class AGIResource
  {
    protected string strErrSource = "WINAGI.agiResource";
    protected bool mLoaded = false;
    protected string mResID;
    protected sbyte mVolume = -1;
    protected int mLoc = -1;
    protected int mSize = -1; //actual size, if compressed this is different from mSizeInVol
    protected int mSizeInVol = -1;  //size as stored in VOL
    /*
    size- if NOT in a game:
      - SizeInVol is always -1
      - if resource loaded,
        Size = Data.Length
      - if resource NOT loaded,
        Size = 0

    size- if IN a game:
      - SizeInVol - read from wag during initial load;
                    if not present, get from res Header in vol file
                  - update in WAG file when resource is saved


    */
    protected RData mRData;
    protected bool mInGame;
    protected bool mIsDirty;
    protected byte mResNum;
    protected string mDescription;
    private readonly AGIResType mResType;
    private string mResFile;
    private bool mblnEORes; //flag indicating pointer is at beginning of resource (CurPos=0)
    private int mlngCurPos;  //current position of pointer in resource data

    internal delegate void AGIResPropChangedEventHandler(object sender, AGIResPropChangedEventArgs e);
    internal event AGIResPropChangedEventHandler PropertyChanged;
    public class AGIResPropChangedEventArgs
    {
      public AGIResPropChangedEventArgs(string name)
      {
        Name = name;
      }
      public string Name { get; }
    }
    protected void OnPropertyChanged(string name)
    {
      PropertyChanged?.Invoke(this, new AGIResPropChangedEventArgs(name));
    }
    public sbyte Volume 
    {
      get 
      {
      if (!mInGame)
      {
        return -1;
      }
      else
      {
        return mVolume;
      }
      } 
      internal set { mVolume = value; }
    }
    public int Loc 
    { 
      get 
      {
        if (mInGame)
        {
          return mLoc;
        }
        else
        {
          return -1;
        }
      }
      internal set
      { 
        mLoc = value; 
      } 
    }
    public int Size
    {
      get
      {
        // returns the uncompressed size of the resource
        // location of resource doesn't matter; should always have a size

        //
        if (mInGame)
        {
          if (mLoaded)
          {
            return mRData.Length;
          }
          else
          {
            return mSize;
          }
        }
        else
        {
          //not in a game:
          if (mLoaded)
          {
            return mRData.Length;
          }
          else
          {
            return 0;
          }
        }
      }
      protected set
      {
        //only subclass can set size, when initializing a new game resource
        mSize = value;
      }
    }
    public virtual int SizeInVOL
    {
      get
      {
        //returns the size of the resource on the volume
        if (mInGame)
        {
          // is established during game load, so just return it
          return mSizeInVol;
          ////if not established yet
          //if (mSizeInVol == -1)
          //{
          //  //get Value
          //  mSizeInVol = GetSizeInVOL(mVolume, mLoc);
          //  //if valid Value not returned,
          //  if (mSizeInVol == -1)
          //  {
          //    throw new Exception(" 625, strErrSource, LoadResString(625)");
          //  }
          //}
        }
        else
        {
          return -1;
        }
      }
      set
      {
        //only AddToVOL calls this; either on intial load, or after saving a resource
        //after saving a resource to a VOL file
        mSizeInVol = value;
     }
    }
    public int V3Compressed
    { get; internal set; }// 0 = not compressed; 1 = picture compress; 2 = LZW compress
    public bool InGame
      { get{ return mInGame; } internal set{ mInGame = value; } }
    public byte Number
    { get { if (mInGame) return mResNum; return 0; }//TODO: number is meaningless if not in a game
      internal set { mResNum = value; }
    }
    public bool Loaded
    { get { return mLoaded; } internal set { } }
    public string ResFile
    { 
      get 
      {
        if (mInGame)
        {
          return mResFile;
        }
        else
        {
          return "";
        }
      } 
      set 
      { 
        mResFile = value;
      }
    }
    internal bool WritePropState { get; set; }
    public virtual bool IsDirty { get { return mIsDirty; } internal set { } }
    public AGIResType ResType
    { get { return mResType; } }
    public virtual string ID
    {
      get { return mResID; }
      internal set
      {
        //sets the ID for a resource;
        //resource IDs must be unique to each resource type
        //max length of ID is 64 characters
        //min of 1 character
        string NewID = value;
        //validate length
        if (NewID.Length == 0)
        {
          //error
          throw new Exception("667, strErrSource, LoadResString(667)");
        }
        else if (NewID.Length > 64)
        {
          NewID = Left(NewID, 64);
        }

        //if changing,
        if (!NewID.Equals(mResID,StringComparison.OrdinalIgnoreCase))
        {
          //if in a game,
          if (InGame)
          {
            //step through other resources
            foreach (AGILogic tmpRes in agLogs.Col.Values)
            {
              //if resource IDs are same
              if (tmpRes.ID == NewID)
              {
                //if not the same resource
                if (tmpRes.Number != Number || tmpRes.ResType != ResType)
                {
                  //error
                  throw new Exception("623, strErrSource, LoadResString(623)");
                }
              }
            }
            foreach (AGIPicture tmpRes in agPics.Col.Values)
            {
              //if resource IDs are same
              if (tmpRes.ID == NewID)
              {
                //if not the same resource
                if (tmpRes.Number != Number || tmpRes.ResType != ResType)
                {
                  //error
                  throw new Exception("623, strErrSource, LoadResString(623)");
                }
              }
            }
            foreach (AGISound tmpRes in agSnds.Col.Values)
            {
              //if resource IDs are same
              if (tmpRes.ID == NewID)
              {
                //if not the same resource
                if (tmpRes.Number != Number || tmpRes.ResType != ResType)
                {
                  //error
                  throw new Exception("623, strErrSource, LoadResString(623)");
                }
              }
            }
            foreach (AGIView tmpRes in agViews.Col.Values)
            {
              //if resource IDs are same
              if (tmpRes.ID == NewID)
              {
                //if not the same resource
                if (tmpRes.Number != Number || tmpRes.ResType != ResType)
                {
                  //error
                  throw new Exception("623, strErrSource, LoadResString(623)");
                }
              }
            }
          }

          //save ID
          mResID = NewID;
          //reset compiler list of ids
          blnSetIDs = false;
        }
      }
    }
    public string Description 
    { 
      get { return mDescription; } 
      internal set 
      {
        //limit description to 1K
        string newDesc = Left(value, 1024);
        if (newDesc !=  mDescription)
        {
          mDescription = newDesc;

          if (mInGame)
          {
            WriteGameSetting("Logic" + Number, "Description", mDescription, "Logics");
          }
        }
      }
    }
    public int EORes
    {
      get
      {

        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mRData.Length;
      }
      private set { }
    }
    // to allow indexing of data property, a separate class needs to
    // be created
    public class RData
    {
      internal delegate void RDataChangedEventHandler(object sender, RDataChangedEventArgs e);
      internal event RDataChangedEventHandler PropertyChanged;
      byte[] mbytData = Array.Empty<byte>();
      public class RDataChangedEventArgs
      {
        public RDataChangedEventArgs(int size)
        {
          //
        }
        public int Size { get; }
      }
      protected void OnPropertyChanged(int size)
      {
        PropertyChanged?.Invoke(this, new RDataChangedEventArgs(size));
      }
      public byte this[int index]
      {
        get
        {
          return mbytData[index];
        }
        set
        {
          mbytData[index] = value;
          OnPropertyChanged(mbytData.Length);
        }
      }
      public byte[] AllData
      { get 
        {
          return mbytData;
        }
      set
      {
        mbytData = value;
        OnPropertyChanged(mbytData.Length);
      }
    }
      public int Length { get { return mbytData.Length; } private set { } }
      public void ReSize(int newSize)
      {
        if (newSize < 0)
        {
          //error 
          throw new IndexOutOfRangeException();
        }
        if (newSize != mbytData.Length)
        {
          Array.Resize(ref mbytData, newSize);
          OnPropertyChanged(mbytData.Length);
        }
      }
      public void Clear()
      {
        //reset to an empty array
        mbytData = Array.Empty<byte>();
        OnPropertyChanged(mbytData.Length);
      }
      public RData(int Size)
      {
        mbytData = new byte[Size];
        // raise change event
        OnPropertyChanged(Size);
      }
    }
    public RData Data
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mRData;
      }
      internal set
      {
        // can only set the data object internally
        mRData = value;
      }
    }
    internal void SetRes(AGIResource NewRes)
    { 
      //called by setview, setlog, setpic and setsnd
      //copies entire resource structure

      //ingame property is never copied; only way to
      //change ingame status is to do so through
      //appropriate methods for adding/removing
      //resources to/from game

      //resource data are copied manually as necessary by calling method
      //EORes and CurPos are calculated; don't need to copy them
      mResNum = NewRes.Number;
      mResFile = NewRes.ResFile;
      //if resource being copied is in a game,
      if (NewRes.InGame)
      {
        //copy vol and loc info
        mVolume = NewRes.Volume;
        mLoc = NewRes.Loc;
      }
      mLoaded = NewRes.Loaded;
      mRData.AllData = NewRes.Data.AllData;
    }
    protected void InitInGame(byte ResNum, sbyte VOL, int Loc)
    {
      //attaches resource to a game
      mInGame = true;
      mResNum = ResNum;
      mVolume = VOL;
      mLoc = Loc;
      // ingame resources start unloaded
      mLoaded = false;
    }
    public virtual void Load()
    {
      //loads the data for this resource
      //from its VOL file, if in a game
      //or from its resfile

      byte bytLow, bytHigh, bytVolNum;
      bool blnIsPicture = false;
      int intSize, lngExpandedSize = 0;
      string strLoadResFile;

      //if already loaded,
      if (mLoaded)
      {
        //error- resource already loaded
        //throw new Exception(" 511, strErrSource, LoadResString(511)
        return;
      }

      //if in a game,
      if (mInGame)
      {
        //resource data is loaded from the AGI VOL file
        //build filename
        if (agIsVersion3)
        {
          strLoadResFile = agGameDir + agGameID + "VOL." + mVolume.ToString();
        }
        else
        {
          strLoadResFile = agGameDir + "VOL." + mVolume.ToString();
        }
      }
      else
      {
        //if no filename
        if (mResFile.Length == 0)
        {
          //error- nothing to load
          throw new Exception(" 626, strErrSource, LoadResString(626)");
        }
        else
        {
          //use resource filename
          strLoadResFile = mResFile;
        }
      }
      //verify file exists
      if (!File.Exists(strLoadResFile))
      {
        throw new Exception(" 606, RLoadResString(606), JustFileName(strLoadResFile))");
      }
      //open file (VOL or individual resource)
      try
      {
        fsVOL = new FileStream(strLoadResFile, FileMode.Open);
        brVOL = new BinaryReader(fsVOL);
      }
      catch (Exception)
      {
        fsVOL.Dispose();
        brVOL.Dispose();
        throw new Exception("LoadResString(502))");
      }
      //verify resource is within file bounds
      if (mLoc > fsVOL.Length)
      {
        throw new Exception("LoadResString(505), ARG1, CStr(mLoc))");
      }
      //if loading from a VOL file (i.e. is in a game)
      if (mInGame)
      {
        //read header bytes
        brVOL.BaseStream.Seek(mLoc, SeekOrigin.Begin);
        bytHigh = brVOL.ReadByte();
        bytLow = brVOL.ReadByte();
        if ((!((bytHigh == 0x12) && (bytLow == 0x34))))
        {
          throw new Exception("LoadResString(506)(mLoc))(strLoadResFile)");
        }
        //get volume where this resource is stored
        bytVolNum = brVOL.ReadByte();

        //determine if this resource is a compressed picture
        blnIsPicture = ((bytVolNum & 0x80) == 0x80);

        //get size info
        bytLow = brVOL.ReadByte();
        bytHigh = brVOL.ReadByte();
        intSize = bytHigh * 256 + bytLow;

        //if version3,
        if (agIsVersion3)
        {
          //the size retreived is the expanded size
          lngExpandedSize = intSize;
          //now get compressed size
          bytLow = brVOL.ReadByte();
          bytHigh = brVOL.ReadByte();
          intSize = bytHigh * 256 + bytLow;
        }
      }
      else
      {
        //get size from total file length
        intSize = (int)fsVOL.Length;
        //version 3 files are never compressed when
        //loaded as individual files
        lngExpandedSize = intSize;
      }

      //get resource data
      mRData.ReSize(intSize);
      mRData.AllData = brVOL.ReadBytes(intSize);
      fsVOL.Dispose();
      brVOL.Dispose();

      //if version3
      if (agIsVersion3)
      {
        //if resource is a compressed picture
        if (blnIsPicture)
        {
          //pictures use this decompression
          mRData.AllData = DecompressPicture(mRData.AllData);

          //if resource is LZW compressed,
        }
        else
        {
          if (mRData.Length != lngExpandedSize)
          {
            //all other resources use LZW compression
            mRData.AllData = ExpandV3ResData(mRData.AllData, lngExpandedSize);
            //if expanding a logic
            if (mResType == AGIResType.rtLogic)
            {
              int lngMsgStart;
              byte NumMessages;
              int lngPos, lngMsgEnd, EncryptionStart, i;
              //need to toggle encryption of message section
              //(since compressed version three logics don//t
              //have their message sections encrypted)
              //
              //get start of msg section
              lngMsgStart = mRData[1] * 256 + mRData[0] + 2;

              //read in number of messages
              NumMessages = mRData[lngMsgStart];
              if (NumMessages > 0)
              {
                //retrieve and adjust end of message section
                lngMsgEnd = lngMsgStart + 256 * mRData[lngMsgStart + 2] + mRData[lngMsgStart + 1];
                //set start of encryption (at end of msg section table)
                EncryptionStart = lngMsgStart + 3 + NumMessages * 2;
                //mark start of encrypted data (to align encryption string)
                //step through all bytes and toggle encryption
                for (lngPos = EncryptionStart; lngPos <= lngMsgEnd; lngPos++)
                {
                  mRData[lngPos] = (byte)(mRData[lngPos] ^ bytEncryptKey[(lngPos - EncryptionStart) % 11]);
                }
              }
            }
          }
        }
      }
      //reset resource markers
      mlngCurPos = 0;
      mblnEORes = false;
      mLoaded = true;
      //update size property
      mSize = intSize;
      //attach events
      mRData.PropertyChanged += Raise_DataChange;
      return;
        ////ErrHandler:
        ////if an AGI error, error number and string already set
        ////so only need error info if some other error occurred
        //////raise the error
        ////throw new Exception("LoadResString(507),JustFileName(strLoadResFile))");
        ////reset resource markers
        //mlngCurPos = 0;
        //mblnEORes = false;
        //mLoaded = false;
    }
    public virtual void Unload()
    {
      // only ingame resources can be unloaded
      if (!mInGame)
      {
        //just exit
        return;
      }
      // reset flag first so size doesn't get cleared for in game resources
      mLoaded = false;
      mIsDirty = false;
      //reset resource variables
      mRData.Clear();
      //don't mess with sizes though! they remain accessible even when unloaded
      mblnEORes = true;
      mlngCurPos = 0;
      //detach events
      mRData.PropertyChanged -= Raise_DataChange;
    }
    internal virtual void Save(string SaveFile = "")
    {
      //saves a resource into a VOL file if in a game
      //exports the resource if not in a game

      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception(" 563, strErrSource, LoadResString(563)");
      }

      //if in game,
      if (mInGame)
      {
        try
        {
          //add resource to VOL file to save it
          AddToVol(this, agIsVersion3);
          // update saved size
          mSize = mRData.Length;
          // resource is no longer compressed
          V3Compressed = 0;
          mSizeInVol = mSize;
        }
        catch (Exception e)
        {
          //pass error along
          throw new Exception($"error: {e.HResult}, {e.Message}");
        }

        //change date of last edit
        agLastEdit = DateTime.Now;
      }
      else
      {
        //export it
        Export(SaveFile);
      }
    }
    public void Export(string ExportFile)
    {
      //exports resource to file
      //doesn't affect the current resource filename
      //MUST specify a valid export file
      //if export file already exists, it is overwritten
      //caller is responsible for verifying overwrite is ok or not

      bool blnUnload = false;

      //if no filename passed
      if (ExportFile.Length == 0)
      {
        throw new Exception(" 599, strErrSource, LoadResString(599)");
      }

      //if not loaded
      if (!mLoaded)
      {
        blnUnload = true;
        try
        {
          Load();
        }
        catch (Exception)
        {
          throw new Exception(" 601, strErrSource, LoadResString(601)");
        }
      }
      //get temporary file
      string strTempFile = Path.GetTempFileName();

      FileStream fsExport = null;
      try
      {
        //open file for output
        fsExport = new FileStream(strTempFile, FileMode.Open);
        //write data
        fsExport.Write(mRData.AllData, 0, mRData.Length);
      }
      catch (Exception)
      {
        fsExport.Dispose();
        File.Delete(strTempFile);
        if (blnUnload)
        {
          Unload();
          //return error condition
        }
        throw new Exception(" 582, strErrSource, LoadResString(582)");
      }

      //close file,
      fsExport.Dispose();
      //unload if necessary
      if (blnUnload)
      {
        Unload();
      }

      //if savefile exists
      if (File.Exists(ExportFile))
      {
        //delete it
        try
        {
          File.Delete(ExportFile);
        }
        catch (Exception)
        {
          //ignore if it can't be deleted; user will just have
          // to deal with it
        }
      }
      try
      {
        //copy tempfile to savefile
        File.Move(strTempFile, ExportFile);
      }
      catch (Exception)
      {
        //erase the temp file
        File.Delete(strTempFile);
        //return error condition
        throw new Exception(" 582, strErrSource, LoadResString(582)");
      }

      //if NOT in a game,
      if (!mInGame)
      {
        //change resfile to match new export filename
        mResFile = ExportFile;
      }
    }
    public virtual void Import(string ImportFile)
    {
      //imports resource from a file, and loads it
      //if in a game, it also saves the new
      //resource in a VOL file
      //
      //import does not check if the imported
      //resource is valid or not, nor if it is
      //of the correct resource type
      //
      //the calling program is responsible for
      //that check

      //if no filename passed
      if (ImportFile.Length == 0)
      {
        //error
        throw new Exception(" 604, strErrSource, LoadResString(604)");
      }

      //if file doesn//t exist
      if (!File.Exists(ImportFile))
      {
        //error
        throw new Exception("LoadResString(524), ImportFile)");
      }

      //if resource is currently loaded,
      if (mLoaded)
      {
        Unload();
      }

      //open file for binary
      FileStream fsImport = null;
      try
      {
        fsImport = new FileStream(ImportFile, FileMode.Open);
      }
      catch (Exception)
      {
        throw new Exception("LoadResString(605), ImportFile");
      }
      // if file is empty
      if (fsImport.Length == 0)
      {
        throw new Exception("LoadResString(605), ImportFile");
      }
      //load resource from file
      fsImport.Read(mRData.AllData, 0,  (int)fsImport.Length);
      //if in a game
      if (mInGame)
      {
        //save resource
        Save();
      }
      else
      {
        //save the resource filename
        mResFile = ImportFile;
      }
      //reset resource markers
      mlngCurPos = 0;
      mblnEORes = false;
      mLoaded = true;

      //raise change event
      OnPropertyChanged("Data");
    }
    public void WriteByte(byte InputByte, int Pos = -1)
    {
      bool bNoEvent = false;
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception(" 563, strErrSource, LoadResString(563)");
      }
      //if a location is passed,
      if (Pos != -1)
      {
        int lngError = 0;
        //validate the new position
        if (Pos >= MAX_RES_SIZE)
        {
          lngError = 516;
        }
        else if (Pos < 0)
        {
          lngError = 514;
        }
        else if (Pos > mRData.Length)
        {
          lngError = 517;
        }
        if (lngError > 0)
        {
          throw new Exception($"error: {lngError}");
        }
      }
      else
      {
        //otherwise, default to end of resource
        Pos = mRData.Length;
      }

      //if currently pointing past end of data,
      if (Pos == mRData.Length)
      {
        //adjust to make room for new data being added
        mRData.ReSize(mRData.Length + 1);
        //this calls a change event, don't need to do another
        bNoEvent = true;
      }

      //save input data
      mRData[Pos] = InputByte;

      //set current position to Pos
      mlngCurPos = Pos + 1;

      //set EORes Value
      mblnEORes = (mlngCurPos == mRData.Length);

      if (!bNoEvent)
      {
        //raise change event
        OnPropertyChanged("Data");
      }
      return;
    }
    public void WriteWord(ushort InputInt, int Pos = -1, bool blnMSLS = false)
    {
      bool bNoEvent = false;

      byte bytHigh = 0, bytLow = 0;
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //if a location is passed,
      if (Pos != -1)
      {
        //validate the new position
        int lngError = 0;
        if (Pos >= MAX_RES_SIZE - 1)
        {
          lngError = 516;
        }
        else if (Pos < 0)
        {
          lngError = 514;
        }
        else if (Pos > mRData.Length)
        {
          lngError = 513;
        }
        if (lngError > 0)
        {
          throw new Exception($"error: {lngError}");
        }
      }
      else
      {
        //otherwise, default to end of resource
        Pos = mRData.Length;
      }

      //if past end of resource (by two bytes)
      if (Pos == mRData.Length)
      {
        //adjust to make room for new data being added
        mRData.ReSize(mRData.Length + 2);
        //this calls a change event, don't need to do another
        bNoEvent = true;
      }
      //if past end of resource (by one byte)
      else if (Pos == mRData.Length - 1)
      {
        //adjust to make room for new data being added
        mRData.ReSize(mRData.Length + 1);
        //this calls a change event, don't need to do another
        bNoEvent = true;
      }

      //split integer portion into two bytes
      bytHigh = (byte)(InputInt >> 8);
      bytLow = (byte)(InputInt % 256);
      //if wrting in MSLS mode,
      if (blnMSLS)
      {
        //save input data
        mRData[Pos] = bytHigh;
        mRData[Pos + 1] = bytLow;
      }
      else
      {
        mRData[Pos] = bytLow;
        mRData[Pos + 1] = bytHigh;
      }

      //set current position to Pos+2
      mlngCurPos = Pos + 2;

      //set EORes Value
      mblnEORes = (mlngCurPos == mRData.Length);

      if (!bNoEvent)
      {
        //raise change event
        OnPropertyChanged("Data");
      }
    }
    public int Pos
    {  
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception(" 563, strErrSource, LoadResString(563)");
        }
        //returns current position
        return mlngCurPos;
      }
      set
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception(" 563, strErrSource, LoadResString(563)");
        }
        //validate position
        int lngError = 0;
        if (value < 0)
        {
          //no non-negative values
          lngError = 514;
        }
        else if (value > mRData.Length)
        {
          //cant point past eor
          lngError = 513;
        }
        if (lngError > 0)
        {
          throw new Exception($"error: {lngError}");
        }
        //new position is ok; assign it
        mlngCurPos = Pos;
        //set eor Value
        mblnEORes = (mlngCurPos == mRData.Length);
        //set bor Value
        //  mblnBORes = (mlngCurPos = 0) //////???? why is this commented out?
        return;
      }
    }
    public ushort ReadWord(int Pos = -1, bool MSLS = false)
    {
      byte bytLow, bytHigh;
      int lngError = 0;
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception(" 563, strErrSource, LoadResString(563)");
      }
      //if a position is passed,
      if (Pos != -1)
      {
        //validate position
        if (Pos < 0)
        {
          //no non-negative values
          lngError = 514;
        }
        else if (Pos >= mRData.Length)
        {
          //cant read past end
          lngError = 513;
        }
        if (lngError > 0)
        {
          throw new Exception($"error: {lngError}");
        }
        //new position is ok; assign it
        mlngCurPos = Pos;
      }

      //if reading in MS-LS format,
      if (MSLS)
      {
        //read the two bytes, high first
        bytHigh = mRData[mlngCurPos];
        bytLow = mRData[mlngCurPos + 1];
      }
      else
      {
        //read the two bytes, low first
        bytLow = mRData[mlngCurPos];
        bytHigh = mRData[mlngCurPos + 1];
      }
      //adjust intCount
      mlngCurPos = mlngCurPos + 2;
      //check for end of resource
      mblnEORes = (mlngCurPos == mRData.Length);
      //calculate word Value
      return (ushort)(bytHigh * 256 + bytLow);
    }
    public byte ReadByte(int Pos = MAX_RES_SIZE + 1)
    {
      int lngError = 0;
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception(" 563, strErrSource, LoadResString(563)");
      }
      //if a position is passed
      if (Pos != MAX_RES_SIZE + 1)
      {
        //validate position
        if (Pos < 0)
        {
          //no negatives allowed
          lngError = 514;
        } else if (Pos >= mRData.Length)
        {
          //cant read past end
          lngError = 513;
        }
        if (lngError > 0)
        {
          throw new Exception($"error: {lngError}");
        }
        //new position is ok; assign it
        mlngCurPos = Pos;
      }
      byte bRetVal = mRData[mlngCurPos];
      mlngCurPos = mlngCurPos + 1;

      //set end of resource Value
      mblnEORes = (mlngCurPos == mRData.Length);
      return bRetVal;
    }
    public void InsertData(dynamic NewData, int InsertPos = -1)
    {
      //inserts newdata into resource at insertpos, if passed,
      //or at end of resource, if not passed

      int i, lngResEnd, lngNewDatLen;
      byte[] bNewData = new byte[] { 0 };
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception(" 563, strErrSource, LoadResString(563))");
      }

      //array of bytes = OK
      //single byte = OK
      //anything that can be converted to single byte = OK
      if (NewData is byte[])
      {
        //ok
        bNewData = (byte[])NewData;
      }
      else if (NewData is byte)
      {
        // ok
        bNewData[0] = (byte)NewData;
      }

      else if (NewData is string)
      {
        if (byte.TryParse( NewData, out bNewData[0]))
        {
          //ok
        }
        else
        {
          //not ok
          throw new Exception("Type Mismatch");
        }
      }
      else 
      {
        try
        {
          bNewData[0] = (byte)NewData;
        }
        catch (Exception)
        {
          //not ok
          throw new Exception("Type Mismatch");
        }
      }
      //get resource end and length of data being inserted
      lngResEnd = mRData.Length;
      lngNewDatLen = bNewData.Length;

      //if no insert pos passed,
      if (InsertPos == -1)
      {
        //insert at end
        mRData.ReSize(mRData.Length + lngNewDatLen);
        for (i = 0; i < lngNewDatLen; i++)
        {
          mRData[lngResEnd + i] = bNewData[i];
        }
      } 
      else
      {
        //validate insert pos
        if (InsertPos < 0)
        {
          //error
          throw new Exception(" 620, strErrSource, LoadResString(620)");
        }
        if (InsertPos >= mRData.Length) 
        {
          //error
          throw new Exception(" 620, strErrSource, LoadResString(620)");
        }
        // insert pos is OK
        //increase array size
        mRData.ReSize(mRData.Length + lngNewDatLen);
        //move current data forward to make room for inserted data
        for (i = lngResEnd + lngNewDatLen - 1; i >= InsertPos + lngNewDatLen; i--)
        {
          mRData[i] = mRData[i - lngNewDatLen];
        } 
        //add newdata at insertpos
        for (i = 0; i < lngNewDatLen; i++)
        {
          mRData[InsertPos + i] = bNewData[i];
        }
      }
      //raise change event
      OnPropertyChanged("Data");
    }
    public void RemoveData(int RemovePos, int RemoveCount = 1)
    {
      //removes data from RemovePos; if a Count
      //is passed, that number of bytes removed; if no
      //Count is passed, then only one byte removed

      int i, lngResEnd;

      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception(" 563, strErrSource, LoadResString(563)");
      }

      lngResEnd = mRData.Length - 1;

      //validate Count
      if (RemoveCount <= 0)
      {
        //force back to 1
        RemoveCount = 1;
      }

      //validate removepos
      if (RemovePos < 0)
      {
        //error
        throw new Exception(" 620, strErrSource, LoadResString(620)");
      }
      if (RemovePos >= mRData.Length)
      {
        //error
        throw new Exception(" 620, strErrSource, LoadResString(620)");
      }

      //remove by moving data backwards
      for (i = RemovePos; i >= (mRData.Length - RemoveCount - 1); i++)
      {
        mRData[i] = mRData[i + RemoveCount];
      } // for i

      mRData.ReSize(mRData.Length - RemoveCount);
  
      //raise change event
      OnPropertyChanged("Data");

  }
  public virtual void Clear()
    {
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception(" 563, strErrSource, LoadResString(563)");
      }

      //clears the resource data
      mRData.Clear();
      mlngCurPos = 0;
      //mSizeInVol is undefined
      mSizeInVol = -1;
      mblnEORes = false;
    }
    protected AGIResource(AGIResType ResType, string ID)
    {
      // can ONLY create new resource from within other game logics

      // when first created, a resource MUST have a type assigned
      mResType = ResType;
      // New resources start out NOT in game; so vol and loc are undefined
      mInGame = false;
      mVolume = -1;
      mLoc = -1;
      // and is set with a default ID
      this.ID = ID;
      // calling resource constructor is responsible for creating
      // default data

      //new resources start as loaded by default, and can only be unloaded when
      // in a game!
      mLoaded = true;
    }
    internal void Raise_DataChange(object sender, RData.RDataChangedEventArgs e)
    {
      //pass it along
      OnPropertyChanged("Data");
    }
  }
}