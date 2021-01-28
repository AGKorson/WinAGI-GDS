using System;
using System.ComponentModel;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.AGILogicSourceSettings;
using static WinAGI.Engine.AGICommands;
using static WinAGI.Common.WinAGI;

namespace WinAGI.Engine
{
  public class AGIView : AGIResource
  {
    bool mViewSet; //flag to note loops loaded from res data
    internal AGILoops mLoopCol;
    string mViewDesc;
    public AGIView() : base(AGIResType.rtView, "NewView")
    {
      //initialize
      //attach events
      base.PropertyChanged += ResPropChange;
      strErrSource = "WinAGI.View";
      // add rempty loop col
      mLoopCol = new AGILoops(this);
      //set default resource data
      mRData = new RData(0);
      mRData.AllData = new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00 };
      // byte0 = unknown (always 1 or 2?)
      // byte1 = unknown (always 1?)
      // byte2 = loop count
      // byte3 = high byte of viewdesc
      // byte4 = low byte of viewdesc
    }
    internal void InGameInit(byte ResNum, sbyte VOL, int Loc)
    {
      //this internal function adds this resource to a game, setting its resource 
      //location properties, and reads properties from the wag file

      //set up base resource
      base.InitInGame(ResNum, VOL, Loc);

      //if first time loading this game, there will be nothing in the propertyfile
      ID = ReadSettingString(agGameProps, "View" + ResNum, "ID", "");
      if (ID.Length == 0)
      {
        //no properties to load; save default ID
        ID = "View" + ResNum;
        WriteGameSetting("Logic" + ResNum, "ID", ID, "Views");
        //load resource to get size
        Load();
        WriteGameSetting("View" + ResNum, "Size", Size.ToString());
        Unload();
      }
      else
      {
        //get description, size and other properties from wag file
        mDescription = ReadSettingString(agGameProps, "View" + ResNum, "Description", "");
        Size = ReadSettingLong(agGameProps, "View" + ResNum, "Size", -1);
      }
    }
    private void ResPropChange(object sender, AGIResPropChangedEventArgs e)
    {
      ////let//s do a test
      //// increment number everytime data changes
      //Number++;
    }
    internal AGIView Clone()
    {
      //copies view data from this view and returns a completely separate object reference
      AGIView CopyView = new AGIView();
      // copy base properties
      base.SetRes(CopyView);
      //add WinAGI items
      CopyView.mViewSet = mViewSet; 
      CopyView.mViewDesc = mViewDesc;
      CopyView.mLoopCol = mLoopCol.Clone(this);
      return CopyView;
    }
    public override void Clear()
    {
      //resets the view
      //to a single loop with
      //a single view with
      //a height and witdh of 1
      //and transparent color of 0
      //and no description
      int i, j;
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //clear the resource
      base.Clear();
      //clear description
      mViewDesc = "";
      //reset loop col
      mLoopCol = new AGILoops(this);
      //set default resource data
      mRData.AllData = new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00 };
      // byte0 = unknown (always 1 or 2?)
      // byte1 = unknown (always 1?)
      // byte2 = loop count
      // byte3 = high byte of viewdesc
      // byte4 = low byte of viewdesc
      //set dirty flag
      mIsDirty = true;
    }
    void CompileView()
    {
      // converts loop/cel objects into correct 
      // AGI data stream
      int[] lngLoopLoc, lngCelLoc;
      int i, j;
      byte bytTransCol;
      int k;
      byte[] bytCelData;
      bool blnMirrorAdded;
      // clear the resource data by reinitializing
      mRData.Clear();

      //write header
      WriteByte(1, 0);
      WriteByte(1);
      //write number of loops
      WriteByte((byte)mLoopCol.Count);
      //placeholder for description
      WriteWord(0);
      //initialize loop location array
      lngLoopLoc = new int[mLoopCol.Count];
      //write place holders for loop positions
      for (i = 0; i < mLoopCol.Count; i++)
      {
        WriteWord(0);
      }
      //step through all loops to add them
      for (i = 0; i < mLoopCol.Count; i++)
      {
        //if loop is mirrored AND already added
        //(can tell if not added by comparing the mirror loop
        //property against current loop being added)
        if (mLoopCol[i].Mirrored)
        {
          blnMirrorAdded = (mLoopCol[i].MirrorLoop < i);
        }
        else
        {
          blnMirrorAdded = false;
        }
        if (blnMirrorAdded)
        {
          //loop location is same as mirror
          lngLoopLoc[i] = lngLoopLoc[mLoopCol[i].MirrorLoop];
        }
        else
        {
          //set loop location
          lngLoopLoc[i] = Pos;
          //write number of cels
          WriteByte((byte)mLoopCol[i].Cels.Count);
          //initialize cel loc array
          lngCelLoc = new int[mLoopCol[i].Cels.Count];
          //write placeholders for cel locations
          for (j = 0; j < mLoopCol[i].Cels.Count; j++)
          {
            WriteWord(0);
          }
          //step through all cels to add them
          for (j = 0; j < mLoopCol[i].Cels.Count; j++)
          {
            //save cel loc
            lngCelLoc[j] = Pos - lngLoopLoc[i];
            //write width
            WriteByte(mLoopCol[i].Cels[j].Width);
            //write height
            WriteByte(mLoopCol[i].Cels[j].Height);
            //if loop is mirrored
            if (mLoopCol[i].Mirrored)
            {
              //set bit 7 for mirror flag and include loop number
              //in bits 6-5-4 for transparent color
              bytTransCol = (byte)(0x80 + i * 0x10 + mLoopCol[i].Cels[j].TransColor);
            }
            else
            {
              //just use transparent color
              bytTransCol = (byte)mLoopCol[i].Cels[j].TransColor;
            }
            //write transcolor
            WriteByte(bytTransCol);
            //get compressed cel data
            bytCelData = CompressedCel(mLoopCol[i].Cels[j], mLoopCol[i].Mirrored);
            //write cel data
            for (k = 0; k < bytCelData.Length; k++)
            {
              WriteByte(bytCelData[k]);
            }
          }
          //step through cels and add cel location
          for (j = 0; j < mLoopCol[i].Cels.Count; j++)
          {
            WriteWord((ushort)lngCelLoc[j], lngLoopLoc[i] + 1 + 2 * j);
          }
          //restore pos to end of resource
          Pos = mRData.Length;
        }
      }
      //step through loops again to add loop loc
      for (i = 0; i < mLoopCol.Count; i++)
      {
        WriteWord((ushort)lngLoopLoc[i], 5 + 2 * i);
      }
      //if there is a view description
      if (mViewDesc.Length > 0)
      {
        //write view description location
        WriteWord((ushort)mRData.Length, 3);
        //move pointer back to end of resource
        Pos = mRData.Length;
        //write view description
        for (i = 1; i < mViewDesc.Length; i++)
        {
          WriteByte((byte)mViewDesc[i]);
        }
        //add terminating null char
        WriteByte(0);
      }
      //set viewloaded flag
      mViewSet = true;
    }
    void ExpandCelData(int StartPos, AGICel TempCel)
    {  //this function will expand the RLE data beginning at
       //position StartPos
       //it then passes the expanded data to the cel
      byte bytWidth, bytHeight, bytTransColor;
      byte bytCelX, bytCelY = 0;
      byte bytIn;
      byte bytChunkColor, bytChunkCount;
      byte[,] tmpCelData;
      bytWidth = TempCel.Width;
      bytHeight = TempCel.Height;
      bytTransColor = (byte)TempCel.TransColor;
      //reset size of data array
      tmpCelData = new byte[bytWidth, bytHeight];
      //set resource to starting position
      Pos = StartPos;
      // extract pixel data
      do
      {
        bytCelX = 0;
        do
        {
          //read each byte, where lower four bits are number of pixels,
          //and upper four bits are color for these pixels
          bytIn = ReadByte();
          //skip zero values
          if (bytIn > 0)
          {
            //extract color
            bytChunkColor = (byte)(bytIn / 0x10);
            bytChunkCount = (byte)(bytIn % 0x10);
            //add data to bitmap data array
            //now store this color for correct number of pixels
            for (int i = 0; i < bytChunkCount; i++)
            {
              tmpCelData[bytCelX, bytCelY] = bytChunkColor;
              bytCelX++;
            }
          }
        } while (bytIn != 0); //Loop Until bytIn = 0
                              //fill in rest of this line with transparent color, if necessary
        while (bytCelX < bytWidth)
        { // Until bytCelX >= bytWidth
          tmpCelData[bytCelX, bytCelY] = bytTransColor;
          bytCelX++;
        }
        bytCelY++;

      } while (bytCelY < bytHeight); // Until bytCelY >= bytHeight

      //pass cel data to the cel
      TempCel.AllCelData = tmpCelData;
    }
    byte GetMirrorPair()
    {  //this function will generate a unique mirrorpair number
       //that is used to identify a pair of mirrored loops
       //the source loop is positive; the copy is negative
      byte i;
      bool goodnum;
      //start with 1
      byte retval = 1;
      do
      {
        // assume number is ok
        goodnum = true;
        for (i = 0; i < mLoopCol.Count; i++)
        {
          //if this loop is using this mirror pair
          if (retval == Math.Abs(mLoopCol[i].MirrorPair))
          {
            //try another number
            goodnum = false;
            break;
          }
        }
        //if number is ok
        if (goodnum)
        {
          //use this mirrorpair
          break;
        }
        //try another
        retval++;
      } while (true);
      return retval;
    }
    internal void LoadLoops()
    {
      // used by load function to extract the view
      // loops and cels from the data stream
      byte bytNumLoops, bytNumCels;
      int[] lngLoopStart = new int[MAX_LOOPS];
      ushort lngCelStart, lngDescLoc;
      int tmpLoopNo, bytLoop, bytCel;
      byte bytInput;
      byte[] bytMaxW = new byte[MAX_LOOPS], bytMaxH = new byte[MAX_LOOPS];
      byte bytWidth, bytHeight;
      byte bytTransCol;
      //clear out loop collection by assigning a new one
      mLoopCol = new AGILoops(this);
      ////if empty (as in creating a new view)
      //if (mSize == 1) {
      //  //set flag and exit
      //  mViewSet = true;
      //  return;
      //}
      
      //get number of loops and strDescription location
      bytNumLoops = ReadByte(2);
      //get offset to description
      lngDescLoc = ReadWord();
      //if no loops
      if (bytNumLoops == 0)
      {
        //error - invalid data
        throw new Exception("595, strErrSource, LoadResString(595)");
      }
      //get loop offset data for each loop
      for (bytLoop = 0; bytLoop < bytNumLoops; bytLoop++)
      {
        //get offset to start of this loop
        lngLoopStart[bytLoop] = ReadWord();
        //if loop data is past end of resource
        if ((lngLoopStart[bytLoop] > mSize))
        {
          Unload();
          throw new Exception("548, strErrSource, LoadResString(548)");
        }
      }
      //step through all loops
      for (bytLoop = 0; bytLoop < bytNumLoops; bytLoop++)
      {
        //add the loop
        mLoopCol.Add(bytLoop);
        //loop zero is NEVER mirrored
        if (bytLoop > 0)
        {
          //for all other loops, check to see if it mirrors an earlier loop
          for (tmpLoopNo = 0; tmpLoopNo < bytLoop; tmpLoopNo++)
          {
            //if the loops have the same starting position,
            if (lngLoopStart[bytLoop] == lngLoopStart[tmpLoopNo])
            {
              //this loop is a mirror
              try
              {
                //get a new mirror pair number
                byte i = GetMirrorPair();
                //set the mirror loop hasmirror property
                mLoopCol[tmpLoopNo].MirrorPair = i;
                //set the mirror loop mirrorloop property
                mLoopCol[bytLoop].MirrorPair = -i;
                //link to the cel collection
                mLoopCol[bytLoop].Cels = mLoopCol[tmpLoopNo].Cels;
              }
              catch (Exception e)
              {
                //if error is because source is already mirrored
                //continue without setting mirror; data will be
                //treated as a completely separate loop; otherwise
                // error is unrecoverable
                if (e.HResult != WINAGI_ERR + 551)
                {
                  //pass along the error
                  Unload();
                  //error
                  throw;
                }
              }
              break;
            }
          }
        }
        //if loop not mirrored,
        if (!mLoopCol[bytLoop].Mirrored)
        {
          //point to start of this loop
          Pos = lngLoopStart[bytLoop];
          //read number of cels
          bytNumCels = ReadByte();
          //step through all cels in this loop
          for (bytCel = 0; bytCel < bytNumCels; bytCel++)
          {
            //read starting position
            lngCelStart = (ushort)(ReadWord(lngLoopStart[bytLoop] + 2 * bytCel + 1) + lngLoopStart[bytLoop]);
            if ((lngCelStart > mSize))
            {
              Unload();
              throw new Exception("553, strErrSource, LoadResString(553)");
            }
            //get height/width
            bytWidth = ReadByte(lngCelStart);
            bytHeight = ReadByte();
            //get transparency color for this cel
            bytTransCol = ReadByte();
            bytTransCol = (byte)(bytTransCol % 0x10);
            //add the cel
            mLoopCol[bytLoop].Cels.Add(bytCel, bytWidth, bytHeight, (AGIColors)bytTransCol);
            //extract bitmap data from RLE data
            ExpandCelData(lngCelStart + 3, mLoopCol[bytLoop].Cels[bytCel]);
          }
        }
      }
      //clear the description string
      mViewDesc = "";
      //if there is a description for this view,
      if (lngDescLoc > 0)
      {
        //ensure it can be loaded
        if (lngDescLoc < mSize - 1)
        {
          //set resource pointer to beginning of description string
          Pos = lngDescLoc;
          do
          {
            //get character
            bytInput = ReadByte();
            //if not zero, and string not yet up to 255 characters,
            if ((bytInput > 0) && (mViewDesc.Length < 255))
            {
              //add the character
              mViewDesc += (char)bytInput;
            }
            //stop if zero reached, end of resource reached, or 255 characters read
          }
          while (!EORes && bytInput != 0 && mViewDesc.Length < 255); // Until EORes || (bytInput == 0) || mViewDesc.Length >= 255)
        } 
        else
        {
          Unload();
          //error? can't load this description
          throw new Exception("513, strErrSource, LoadResString(513)");
        }
      }
      //set flag indicating view matches resource data
      mViewSet = true;
      //MUST be clean, since loaded from resource data
      mIsDirty = false;
    }
    public void Export(string ExportFile, bool ResetDirty = true)
    {
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      try
      {
        //if view is dirty
        if (mIsDirty)
        {
          //need to recompile
          CompileView();
        }
        Export(ExportFile);
      }
      catch (Exception) { throw; }
      //if not in a game,
      if (!mInGame)
      {
        //ID always tracks the resfile name
        mResID = JustFileName(ExportFile);
        if (mResID.Length > 64)
        {
          mResID = Left(mResID, 64);
        }
        if (ResetDirty)
        {
          //clear dirty flag
          mIsDirty = false;
        }
      }
    }
    public override void Import(string ImportFile)
    {  //imports a view resource
      try
      {
        //import the resource
        Import(ImportFile);
      }
      catch (Exception)
      {
        //pass along error
        Unload();
        throw;
      }
      //if not in a game,
      if (!mInGame)
      {
        //set ID
        mResID = JustFileName(ImportFile);
        if (mResID.Length > 64)
        {
          mResID = Left(mResID, 64);
        }
      }
      //reset dirty flag
      mIsDirty = false;
      WritePropState = false;
      //loops need rebuilding
      mViewSet = false;
    }
    public override void Load()
    {
      // ignore if already loaded
      if (Loaded)
      {
        return;
      }
      //if not ingame, the resource should already be loaded
      if (!mInGame)
      {
        throw new Exception("non-game view should already be loaded");
      }
      try
      {
        //load base resource data
        base.Load();
        LoadLoops();
      }
      catch (Exception)
      {
        Unload();
        //pass along error
        throw;
      }
      //clear dirty flags
      mIsDirty = false;
      WritePropState = false;
    }
    public override void Unload()
    {
      //unload resource
      base.Unload();
      mIsDirty = false;
      //clear out loop collection
      mLoopCol = new AGILoops(this);
      mViewSet = false;
    }
    public void Save()
    {
      //saves an ingame view
      //if properties need to be written
      if (WritePropState && mInGame)
      {
        //save ID and description to ID file
        WriteGameSetting("View" + Number, "ID", mResID, "Views");
        WriteGameSetting("View" + Number, "Description", mDescription);
        WritePropState = false;
      }
      //if not loaded
      if (!mLoaded)
      {
        //nothing to do
        return;
      }
      //if dirty,
      if (mIsDirty)
      {
        try
        {
          //rebuild Resource
          CompileView();
          base.Save();
        }
        catch
       (Exception)
        {
          //pass error along
          throw;
        }
        WriteGameSetting("View" + Number, "Size", mSize, "Views");
        //reset flag
        mIsDirty = false;
      }
    }
    public AGILoop this [int index]
    {
      get
      {
        try
        {
          return Loops[index];
        }
        catch (Exception)
        {
          // pass along any error
          throw;
        }
      }
    }
    public AGILoops Loops
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        //if view not set,
        if (!mViewSet)
        {
          try
          {
            //load loops first
            LoadLoops();
          }
          catch (Exception)
          {
            //pass error along
            throw;
          }
        }
        //return the loop collection
        return mLoopCol;
      }
    }
    public string ViewDescription
    {
      get
      {
        //if not loaded
        if (!mLoaded)
        {
          //error
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mViewDesc;
      }
      set
      {
        //if changing,
        if (mViewDesc != value)
        {
          mViewDesc = Left(value, 255);
          mIsDirty = true;
        }
      }
    }
    public void SetMirror(byte TargetLoop, byte SourceLoop)
    {
      //TargetLoop is the loop that will be a mirror of
      //SourceLoop; the cels collection in TargetLoop will be lost
      //once the mirror property is set
      int i;
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //the source loop must already exist
      //(must be less than or equal to max number of loops)
      if (SourceLoop >= mLoopCol.Count)
      {
        //error - loop must exist
        throw new Exception("539, strErrSource, LoadResString(539)");
      }
      //the source loop and the target loop must be less than 8
      if (SourceLoop >= 8 || TargetLoop >= 8)
      {
        //error - loop must exist
        throw new Exception("539, strErrSource, LoadResString(539)");
      }
      //mirror source and target can't be the same
      if (SourceLoop == TargetLoop)
      {
        //ignore - can't be a mirror of itself
        return;
      }
      //this loop can't be already mirrored
      if (mLoopCol[TargetLoop].Mirrored)
      {
        //error
        throw new Exception("550, strErrSource, LoadResString(550)");
      }
      //the mirror loop can't already have a mirror
      if (mLoopCol[SourceLoop].Mirrored)
      {
        //error
        throw new Exception("551, strErrSource, LoadResString(551), SourceLoop");
      }
      //get a new mirror pair number
      i = GetMirrorPair();
      //set the mirror loop hasmirror property
      mLoopCol[SourceLoop].MirrorPair = i;
      //set the mirror loop mirrorloop property
      mLoopCol[TargetLoop].MirrorPair = -i;
      mLoopCol[TargetLoop].Cels = mLoopCol[SourceLoop].Cels;
      //set dirty flag
      mIsDirty = true;
    }
  }
}