using System;
using System.ComponentModel;
using System.IO;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;

namespace WinAGI
{
  public abstract class AGIResource
  {
    private string strErrSource = "WINAGI.agiResource";
    private bool mLoaded = false;
    private sbyte mVolume = -1;
    private int mLoc = -1;
    private int mSizeInVol = -1;
    private RData mRData;
    private bool mInGame;
    private byte mResNum;
    private AGIResType mResType;
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

    public sbyte Volume { get { return mVolume; } internal set { mVolume = value; } }
    public int Loc { get; internal set; }
    public int Size
    {
      get
      {
        //returns the uncompressed size of the resource
        //location of resource doesn't matter; should always have a size

        //if not established yet TODO- how to handle initialization...
        if (mRData.Length == -1)
        {
          //can't be true if resource is loaded
          //*'Debug.Assert mLoaded == false
          //load it to get size
          Load();
          //then unload
          Unload();
        }

        //if still -1? not possible unless error encountered in load method
        if (mRData.Length == -1)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //return the size
        return mRData.Length;
      }
      set
      {
      }
    }
    public bool InGame
      { get{ return mInGame; } internal set{ mInGame = value; } }
    public byte Number
    { get { return mResNum; }
      set { mResNum = value; }
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
      set
      {
        // not allowed
      }
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

      //NOTE: because GET is //1// based, need to correct all //GET// statements
      //by adding one to position

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
            ExpandV3ResData(mRData.AllData, lngExpandedSize);
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

        //reset resource markers
        mlngCurPos = 0;
        mblnEORes = false;
        mLoaded = true;
        return;

        //ErrHandler:
        //if an AGI error, error number and string already set
        //so only need error info if some other error occurred
        ////raise the error
        //throw new Exception("LoadResString(507),JustFileName(strLoadResFile))");
        //reset resource markers
        mlngCurPos = 0;
        mblnEORes = false;
        mLoaded = false;
        //attach events
        mRData.PropertyChanged += Raise_DataChange;
      }
    }
    public virtual void Unload()
    {
      mLoaded = false;
      //throw new NotImplementedException();
    }
    internal void Init(byte ResNum, sbyte VOL = -1, int Loc = -1)
    {
      //Init method only called when a resource is added to a game
      //via the Add method of the resource collections (Views, Logics, Pictures, Sounds)
      //or via the Load<restype> method

      mResNum = ResNum;
      mVolume = VOL;
      mLoc = Loc;
      mInGame = true;
    }
    public void NewResource(bool Reset = false)
    {
      //clears any data for the resource,and marks it as loaded
      //this is needed so new resources can be created and edited
      // when not in a game
      if (mInGame)
      {
        //don't call NewResource if already in a game;
        //clear it instead
        throw new Exception(" 510, strErrSource, LoadResString(510)");
      }
      //if need to reset
      if (Reset)
      {
        //use unload to force reset
        Unload();
      }
      //mark as loaded
      mLoaded = true;

      //set loc and vol to undefined
      mVolume = -1;
      mLoc = -1;
    }
    internal bool WritePropState { get; set; }
    public bool IsDirty { get; internal set; }
    public int EORes {
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
      private set { } }
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
    private void temp()
    {
      /*
Public Sub RemoveData(ByVal RemovePos As Long, Optional ByVal RemoveCount As Long = 1)
  //removes data from RemovePos; if a Count
  //is passed, that number of bytes removed; if no
  //Count is passed, then only one byte removed
  
  Dim i As Long
  Dim lngResEnd As Long
  
  //if not loaded
  if (!mLoaded) {
    //error
    throw new Exception(" 563, strErrSource, LoadResString(563)
    return;
  }
  
  lngResEnd = mRData.Length - 1
  
  //validate Count
  if (RemoveCount <= 0) {
    //force back to 1
    RemoveCount = 1
  }
  
  //validate removepos
  if (RemovePos < 0) {
    //error
    throw new Exception(" 620, strErrSource, LoadResString(620)
    return;
  }
  if (RemovePos >= mRData.Length) {
    //error
    throw new Exception(" 620, strErrSource, LoadResString(620)
    return;
  }
  
  //remove by moving data backwards
  for (i = RemovePos To mRData.Length - RemoveCount - 1
    mRData[i) = mRData[i + RemoveCount)
  } // for i
  
  mRData.Length = mRData.Length - RemoveCount
  ReDim Preserve mRData[mRData.Length - 1)
  
  //raise change event
  OnPropertyChanged("Data");
End Sub


internal Property Let ResFile(ByVal NewFile As String)

  //called by parent resource to set a non-ingame resource file before loading
  mResFile = NewFile
End Property

Public Property Get ResType() As AGIResType
  ResType = mResType

End Property


internal Sub Save(Optional SaveFile As String)
  //saves a resource into a VOL file if in a game
  //exports the resource if not in a game
  
  On Error GoTo ErrHandler
  
  //if not loaded
  if (!mLoaded) {
    //error
    throw new Exception(" 563, strErrSource, LoadResString(563)
    return;
  }
  
  //if in game,
  if (mInGame) {
    //add resource to VOL file to save it
    AddToVol Me, agIsVersion3
  
    //change date of last edit
    agLastEdit = Now()
    
    //if error,
    if (Err.Number <> 0) {
      //pass error along
      lngError = Err.Number
      strError = Err.Description
      strErrSrc = Err.Source
      
      On Error GoTo 0: Err.Raise lngError, strErrSrc, strError
      return;
    }
  } else {
    //export it
    Export SaveFile
  }
return;

ErrHandler:
  //*'Debug.Assert false
  Resume Next
End Sub

internal Sub SetRes(NewRes As AGIResource)
  //called by setview, setlog, setpic and setsnd
  //copies entire resource structure
  
  With NewRes
    mResNum = .Number
    mResType = .ResType
    mResFile = .ResFile
    //if resource being copied is in a game,
    if (NewRes.InGame) {
      //copy vol and loc info
      mVolume = .Volume
      mLoc = .Loc
    }
    mLoaded = .Loaded
    mRData.Length = .Size
    
    //ingame property is never copied; only way to
    //change ingame status is to do so through
    //appropriate methods for adding/removing
    //resources to/from game
    
    //resresource data is copied manually as necessary by calling method
    //EORes and CurPos are calculated; don//t need to copy them
  End With
End Sub


internal Sub SetType(ByVal ResType As AGIResType)
  //should only be called when a new resource is initialized

  mResType = ResType
End Sub


internal Property Let Size(NewSize As Long)

  //only called during game load
  mRData.Length = NewSize
End Property

Public Property Get SizeInVol() As Long
  //returns the size of the resource on the volume
  
  //if in a game
  if (mInGame) {
    //if not established yet
    if (mSizeInVol = -1) {
      //get Value
      mSizeInVol = GetSizeInVOL(mVolume, mLoc)
      //if valid Value not returned,
      if (mSizeInVol = -1) {
        throw new Exception(" 625, strErrSource, LoadResString(625)
        return;
      }
    }
  } else {
    mSizeInVol = -1
  }
  
  //return Value
  SizeInVol = mSizeInVol
End Property
internal Property Let SizeInVol(ByVal NewSize As Long)
  //only AddToVOL calls this
  //after saving a resource to a VOL file
  mSizeInVol = NewSize
  
End Property


Public Property Get Volume() As Long
   
  //if not in a game
  if (!mInGame) {
    Volume = -1
  } else {
    Volume = mVolume
  }
End Property
internal Property Let Volume(ByVal NewVOL As Long)
  
  mVolume = NewVOL
End Property


Public Property Get ResFile() As String
  //only applies for resources that are not in a game
  
  if (!mInGame) {
    ResFile = mResFile
  } else {
    //return nothing
    ResFile = vbNullString
  }
End Property

Public Property Get Loaded() As Boolean
  Loaded = mLoaded
  
End Property



Public Property Get Loc() As Long
  
  if (!mInGame) {
    Loc = -1
  } else {
    Loc = mLoc
  }
End Property
internal Property Let Loc(ByVal NewLoc As Long)
  
  mLoc = NewLoc
End Property



internal Sub Unload()
  mLoaded = false
  //reset resource variables
  ReDim mRData[0)
  //don//t mess with sizes though! they remain accessible even when unloaded
  mblnEORes = true
  mlngCurPos = 0
End Sub

internal Property Let AllData(NewData() As Byte)

  //only called internally when copying a resource for editing
  //or for creating new compiled resources
  
  //whenever data is set this way, load flag is automatically set
  //to true
  
  On Error GoTo ErrHandler
  
  //assign data
  mbytData = NewData
  mRData.Length = UBound(mbytData) + 1
  
  //reset position
  mlngCurPos = 0
  //reset end of resource
  mblnEORes = false
  //set status as loaded
  mLoaded = true
  
  //raise change event
  OnPropertyChanged("Data");
return;

ErrHandler:
  //*'Debug.Assert false
  Resume Next
End Property

Public Sub SetData(NewData() As Byte)

  //allows the entire resource byte array to be replaced
  
  On Error GoTo ErrHandler
  
  //if not loaded
  if (!mLoaded) {
    //error
    throw new Exception(" 563, strErrSource, LoadResString(563)
    return;
  }
  
  //assign data
  mbytData = NewData
  mRData.Length = UBound(mbytData) + 1
  
  //reset position
  mlngCurPos = 0
  //reset end of resource
  mblnEORes = false
  
  //raise change event
  OnPropertyChanged("Data");
return;

ErrHandler:
  //*'Debug.Assert false
  Resume Next
End Sub

Public Property Get AllData() As Byte()
  //if not loaded
  if (!mLoaded) {
    //error
    throw new Exception(" 563, strErrSource, LoadResString(563)
    return;
  }
  
  //return entire array of data
  AllData = mbytData
End Property


Public Sub Export(ExportFile As String)
  //exports resource to file
  //doesn//t affect the current resource filename
  //MUST specify a valid export file
  //if export file already exists, it is overwritten
  //caller is responsible for verifying overwrite is ok or not
  
  Dim strTempFile As String
  Dim intFile As Integer, blnUnload As Boolean
  
  //if no filename passed
  if (LenB(ExportFile) = 0) {
    throw new Exception(" 599, strErrSource, LoadResString(599)
    return;
  }

  On Error Resume Next
  
  //if not loaded
  if (!mLoaded) {
    blnUnload = true
    Load
    //check for error
    if (Err.Number != 0) {
      throw new Exception(" 601, strErrSource, LoadResString(601)
      return;
    }
  }
  
  //get temporary file
  strTempFile = TempFileName()
  
  //open file for output
  intFile = FreeFile()
  Open strTempFile for (Binary As intFile
  //write data
  Put intFile, , mbytData
  
  //if error,
  if (Err.Number != 0) {
    //close file
    Close intFile
    //erase the temp file
    Kill strTempFile
    //unload if necessary
    if (blnUnload) {
      Unload
    }
    //return error condition
    throw new Exception(" 582, strErrSource, LoadResString(582)
    return;
  }
  
  //close file,
  Close intFile
  //unload if necessary
  if (blnUnload) {
    Unload
  }
  
  //if savefile exists
  if (FileExists(ExportFile)) {
    //delete it
    Kill ExportFile
    Err.Clear
  }
  
  //copy tempfile to savefile
  FileCopy strTempFile, ExportFile
  
  //if error,
  if (Err.Number != 0) {
    //close file
    Close intFile
    //erase the temp file
    Kill strTempFile
    //return error condition
    throw new Exception(" 582, strErrSource, LoadResString(582)
    return;
  }
    
  //delete temp file
  Kill strTempFile
  Err.Clear
  
  //if NOT in a game,
  if (!mInGame) {
    //change resfile to match new export file
    mResFile = ExportFile
  }
End Sub


internal Sub Import(ImportFile As String)
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
  
  Dim intFile As Integer
  
  On Error GoTo ErrHandler
  
  //if no filename passed
  if (LenB(ImportFile) = 0) {
    //error
    throw new Exception(" 604, strErrSource, LoadResString(604)
    return;
  }
  
  //if file doesn//t exist
  if (!FileExists(ImportFile)) {
    //error
    throw new Exception(" 524, strErrSource, Replace(LoadResString(524), ARG1, ImportFile)
    return;
  }
  
  //if resource loaded,
  if (mLoaded) {
    Unload
  }
  
  On Error Resume Next
  //open file for binary
  intFile = FreeFile()
  Open ImportFile for (Binary As intFile
  //check for file access error
  if (Err.Number != 0) {
    Err.Clear
    throw new Exception(" 605, strErrSource, Replace(LoadResString(605), ARG1, ImportFile)
    return;
  }
  
  if (LOF(intFile) = 0) {
    throw new Exception(" 605, strErrSource, Replace(LoadResString(605), ARG1, ImportFile)
    return;
  }
  
  On Error GoTo ErrHandler
  //load resource from file
  mRData.Length = LOF(intFile)
  ReDim mRData[mRData.Length - 1)
  Get intFile, 1, mbytData
  
  //close file
  Close intFile
  
  //if in a game
  if (mInGame) {
    //save resource
    Save
  } else {
    //save the resource filename
    mResFile = ImportFile
  }
  
  //reset resource markers
  mlngCurPos = 0
  mblnEORes = false
  mLoaded = true
  
  //raise change event
  OnPropertyChanged("Data");
return;

ErrHandler:
  //*'Debug.Assert false
  Resume Next
End Sub

Public Function ReadByte(Optional ByVal Pos As Long = MAX_RES_SIZE + 1) As Byte

End Function
    */
    }
    public AGIResource()
    {
      //// new data?
      //mRData = new RData(0);
    }
    internal void Raise_DataChange(object sender, RData.RDataChangedEventArgs e)
    {
      //pass it along
      OnPropertyChanged("Data");
    }
  }
}