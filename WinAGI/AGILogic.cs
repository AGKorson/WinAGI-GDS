using System;
using System.ComponentModel;
using static WinAGI.WinAGI;
using static WinAGI.AGIGame;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;
using System.IO;
using System.Linq;
using System.Text;

namespace WinAGI
{
  public class AGILogic : AGIResource
  {
    //source code properties
    string mSourceFile;
    string mSourceText;
    uint mCompiledCRC;
    uint mCRC;
    bool mSourceDirty;
    bool mIsRoom;
   public override void Load()
    {
      //if not ingame, the resource should already be loaded
      if (!mInGame)
      {
        if (!mLoaded)
        {
          throw new Exception("non-game resources should always be loaded");
        }
      }
      // if already loaded, just exit
      if (mLoaded)
      {
        return;
      }
      try
      {
        // load the base resource data
        base.Load();
      }
      catch (Exception)
      {
        // pass it along
        throw;
      }
      try
      {
        //load the sourcetext
        LoadSource();
      }
      catch (Exception)
      {
        // pass it along
        //throw;
        //>???? how to break, but still keep going after fixing error????
      }

      //compiledCRC Value should already be set,
      //and source crc gets calculated when source is loaded

      //clear dirty flag
      IsDirty = false;
      mSourceDirty = false;
    }
    public override void Unload()
    {
      base.Unload();
      mSourceDirty = false;
      mSourceText = "";
    }
    public AGILogic() : base(AGIResType.rtLogic, "NewLogic")
    {
      //initialize
      //attach events
      base.PropertyChanged += ResPropChange;
      strErrSource = "WinAGI.Logic";
      //set default resource data
      Data = new RData(6)
      {
        AllData = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x02, 0x00 }
      };
      // set default source
      mSourceText = agCmds[0].Name + "();" + NEWLINE + NEWLINE + "[ messages";
      //to avoid having compile property read true if both values are 0, set compiled to -1 on initialization
      CompiledCRC = 0xffffff;
      CRC = 0;
    }
    internal void InGameInit(byte ResNum, sbyte VOL, int Loc)
    {
      //this internal function adds this resource to a game, setting its resource 
      //location properties, and reads properties from the wag file

      //set up base resource
      base.InitInGame(ResNum, VOL, Loc);

      //if first time loading this game, there will be nothing in the propertyfile
      ID = ReadSettingString(agGameProps, "Logic" + ResNum, "ID", "");
      if (ID.Length == 0)
      {
        //no properties to load; save default ID
        ID = "Logic" + ResNum;
        WriteGameSetting("Logic" + ResNum, "ID", ID, "Logics");
        //load resource to get size
        base.Load();
        WriteGameSetting("Logic" + ResNum, "Size", Size.ToString());
        base.Unload();
        //save CRC and CompCRC values as defaults; they//ll be adjusted first time logic is accessed
        WriteGameSetting("Logic" + ResNum, "CRC32", "0x00000000", "Logics");
        WriteGameSetting("Logic" + ResNum, "CompCRC32", "0xffffffff");
      }
      else
      {
        //get description, size and other properties from wag file
        mDescription = ReadSettingString(agGameProps, "Logic" + ResNum, "Description", "");
        Size = ReadSettingLong(agGameProps, "Logic" + ResNum, "Size", -1);

        mCRC = ReadSettingUint32(agGameProps, "Logic" + ResNum, "CRC32");
        mCompiledCRC = ReadSettingUint32(agGameProps, "Logic" + ResNum, "CompCRC32");
        mIsRoom = ReadSettingBool(agGameProps, "Logic" + ResNum, "IsRoom");
      }

      //if res is zero
      if (ResNum == 0)
      {
        //make sure isroom flag is false
        mIsRoom = false;
      }
    }
    internal void SetLogic(AGILogic CopyLogic)
    {
      //copies logic data from CopyLogic into this logic
      base.SetRes(CopyLogic);
      //add WinAGI items
      mResID = CopyLogic.ID;
      mDescription = CopyLogic.Description;
      mIsRoom = CopyLogic.IsRoom;

      //if NOT loaded, need to load if ingame to get source text
      bool blnLoaded = Loaded;
      if (!blnLoaded)
      {
        CopyLogic.Load();
      }
      //crc data
      mCompiledCRC = CopyLogic.CompiledCRC;
      mCRC = CopyLogic.CRC;
      mSourceText = CopyLogic.SourceText;
      mSourceDirty = CopyLogic.SourceDirty;
      mSourceFile = CopyLogic.SourceFile;
      //copy dirty flag and writeprop flag
      mIsDirty = CopyLogic.IsDirty;
      WritePropState = CopyLogic.WritePropState;

      //unload if wasn't loaded earlier
      if (!blnLoaded)
      {
        CopyLogic.Unload();
      }
    }
    public uint CompiledCRC { 
      get 
      {
        //if not in a game
        if (!InGame)
        {
          return 0;
        }
        else
        {
          return mCompiledCRC;
        }
      }

      internal set { }
    }
    public bool IsRoom {
      get { return mIsRoom; }
      internal set 
      {
        //if in a game, and logic 0
        if (mInGame && (Number == 0))
        {
          throw new Exception("450, Can't assign to read-only property");
        }

        //if changing
        if (mIsRoom != value)
        {
          mIsRoom = value;
          WriteGameSetting("Logic" + Number, "IsRoom", mIsRoom.ToString(), "Logics");
        }
     }
    }
    public bool SourceDirty
    {
      get { return mSourceDirty; }
    }
    public string SourceFile
    {
      get
      {
        //if in a game,
        if (mInGame)
        {
          //sourcefile is predefined
          return agResDir + mResID + agSrcExt;
        }
        else
        {
          return mSourceFile;
        }
      }
      set 
      {
        //if in a game,
        if (mInGame)
        {
          //sourcefile is predefined; raise error
          throw new Exception("450, Can't assign to read-only property");
        }
        else
        {
          mSourceFile = value;
        }
      }
    }
    public uint CRC { get; internal set; }
    private void ResPropChange(object sender, AGIResPropChangedEventArgs e)
    {
      ////let's do a test
      //// increment number everytime data changes
      //Number++;
    }
    private void SaveProps()
    {
      string strSection;
      //save properties
      strSection = "Logic" + Number;
      WriteGameSetting(strSection, "ID", ID, "Logics");
      WriteGameSetting(strSection, "Description", Description);
      WriteGameSetting(strSection, "CRC32", "0x" + mCRC.ToString("x8"));
      WriteGameSetting(strSection, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
      WriteGameSetting(strSection, "IsRoom", mIsRoom.ToString());

      //set flag indicating all props are uptodate
      WritePropState = false;
    }
    public bool Compiled
    { get
      {
        // return true if source code CRC is same as stored compiled crc
        // if not in a game, compiled is normally undefined, but in case
        // of a Logic copied from an InGame resource, this property will work

        //if crc is 0, means crc not loaded from a file, or calculated yet;  OR ERROR!!!
        if (mCRC == 0)
        {
          // load the sourcetext to get the crc (also sets compiledcrc if
          // no sourcefile exists)
          if (!Loaded)
          {
            Load();
            try
            {
              LoadSource();
            }
            catch (Exception)
            {
              //presumably due to decompiling error;
              //return value of false?
              ////Debug.Assert False
              Unload();
              return false;
            }
            Unload();
            //done with sourcetext
            mSourceText = "";
          }
        }
        return (mCRC == mCompiledCRC);
      }
      private set { }
    }
    public override void Clear()
    {
      //clears out source code text
      //and clears the resource data
      if (InGame)
      {
        if (!Loaded)
        {
          //nothing to clear
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
      }
      //clear resource
      base.Clear();
      //set default resource data
      Data = new RData(6)
      {
        AllData = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x02, 0x00 }
      };

      //clear the source code by setting it to //return// command
      mSourceText = agCmds[0].Name + "();" + NEWLINE + NEWLINE + "[ messages";
      if (InGame)
      {
        //reset crcs
        mCRC = CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(mSourceText));
        mCompiledCRC = mCRC;
      }

      //note change by marking resource as dirty
      IsDirty = true;
      mSourceDirty = true;
    }
    public void Export(string ExportFile, bool ResetDirty)
    {
      //exports a compiled resource; ingame only
      //(since only ingame logics can be compiled)

      //if not in a game,
      if (!InGame)
      {
        //not allowed; nothing to export
        throw new Exception("668, strErrSource, LoadResString(668)");
      }
      //if not loaded
      if (!Loaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //export a logic resource file
      base.Export(ExportFile);
    }
    public void Import(string ImportFile, bool AsSource)
    {
      //imports a logic resource
      //i.e., opens from a standalone file
      //doesn't matter if ingame or not
      //if importing a logic resource, it will overwrite current source text with decompiled source

      //clear existing resource
      Clear();
      //if importing source code
      if (AsSource)
      {
        //load the source code
        mSourceFile = ImportFile;
        LoadSource(false);
      }
      else
      {
        try
        {
          //import the compiled resource
          Import(ImportFile);
          //load the source code by decompiling
          LoadSource(true);
          //force filename to null
          mSourceFile = "";
        }
        catch (Exception e)
        {
          Unload();
          throw new Exception(e.Message);
        }
      }
      //set ID to the filename without extension;
      //the calling function will take care or reassigning it later, if needed
      //(for example, if the new logic will be added to a game)
      ID = FileNameNoExt(ImportFile);
      //reset dirty flags
      IsDirty = false;
      mSourceDirty = false;
    }
    public override string ID 
    { 
      get => base.ID;
      internal set 
      { 
        base.ID = value;
        if (mInGame)
        {
          //source file always tracks ID for ingame resources
          mSourceFile = agResDir + base.ID + agSrcExt;
          WriteGameSetting("Logic" + Number, "ID", mResID, "Logics");
        }
      }
    }
    private void LoadSource(bool Decompile = false)
    {
      //loads LoadFile as the source code
      //for not-ingame, calling code must first set sourcefile name
      string strInput, LoadFile;

      //if in a game,
      if (mInGame)
      {
        //load file is predefined
        LoadFile = agResDir + mResID + agSrcExt;
        mSourceFile = LoadFile;

        //if it does not exist,
        if (!File.Exists(LoadFile))
        {
          //check for AGI Studio file format
          if (File.Exists(agResDir + "logic" + Number + ".txt") && (agSrcExt == ".lgc"))
          {
            //rename it to correct format
            File.Move(agResDir + "logic" + Number + ".txt", LoadFile);
          } 
          else if (File.Exists(agResDir + mResID + ".txt") && (agSrcExt == ".lgc"))
          {
            //rename it to correct format
            File.Move(agResDir + mResID + ".txt", LoadFile);
          } 
          else
          {
            //default file does not exist; AGI Studio file does not exist;
            //set to null string to force decompile
            LoadFile = "";
          }
        }
      }
      else
      {
        LoadFile = mSourceFile;
      }

      //if no name, (OR forcing decompile)
      if (LoadFile.Length == 0 || Decompile) 
      {
        //get source code by decoding the resource
        strInput = string.Join(NEWLINE, DecodeLogic(mRData.AllData).ToArray());
        //make sure decompile flag is set (so crc can be saved)
        Decompile = true;
        //if in a game, always save this newly created source
      } 
      else
      {
        FileStream fsLogic;
        try
        {
          //load sourcecode from file
          fsLogic = new FileStream(LoadFile, FileMode.Open);
          StreamReader srLogic = new StreamReader(fsLogic, Encoding.GetEncoding(437));
          strInput = srLogic.ReadToEnd();
          srLogic.Dispose();
          fsLogic.Dispose();
        }
        catch (Exception e)
        {
          throw new Exception($"502, LoadResString(502), LoadFile, {e.Message}");
        }
      }
      //send text in source code string list
      mSourceText = strInput;
      //calculate source crc
      //mCRC = CRC32(strInput.ToCharArray());
      mCRC = CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(strInput));
      if (mInGame)
      {
        WriteGameSetting("Logic" + Number, "CRC32", "0x" + mCRC.ToString("x8"), "Logics");
        //if decompiling, also save the source and the compiled crc
        //(it's the same as the source crc!)
        if (Decompile)
        {
          SaveSource();
          mCompiledCRC = mCRC;
          WriteGameSetting("Logic" + Number, "CompCRC32", "0x" + mCompiledCRC.ToString("x8"));
        }
      }
      //set loaded flag and dirty status
      mIsDirty = false;
      mSourceDirty = false;
    }
    public string SourceText
    { 
      get 
      { 
        if (!mLoaded)
        {
          throw new Exception("563, strErrSource, LoadResString(563)");
        }
        return mSourceText;
      }
      set
      {
        int i = 0;
        mSourceText = value;

        //strip off any CRs at the end (unless it//s the only character)
        int lngLen = mSourceText.Length;
        if (lngLen > 1)
        {
          for (i = 1; i < lngLen; i++)
          {
            if (Right(mSourceText, i)[0] != '\n')
            {
              break;
            }
          }
        }
        //remove the extras
        mSourceText = Left(mSourceText, lngLen - i + 1);

        //if in a game, save the crc value
        if (mInGame)
        {
          //calculate new crc value
          mCRC = CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(mSourceText));
          mSourceDirty = true;
        }
      }
    }
    public void SaveSource(string SaveFile = "", bool Exporting = false)
    {
      //saves the source code for this logic to file
      string strTempFile;
      byte[] bytText;
      bool blnUnload;
      //no filename needed for ingame logics,
      //but must have filename if NOT ingame

      //if Exporting flag is true, we are just saving the file; don't do any other
      //maintenance on the logic resource

      //if not loaded, then load it
      blnUnload = !mLoaded;
      if (blnUnload)
      {
        Load();
      }

      if (SaveFile.Length == 0)
      {
        //if in a game
        if (mInGame)
        {
          //filename is predfined
          SaveFile = mSourceFile;
        }
        else
        {
          //raise an error
          if (blnUnload)
          {
            Unload();
          }
          throw new Exception("599, strErrSource, LoadResString(599)");
        }
      }
      try
      {
        //get temporary file
        strTempFile = Path.GetTempFileName();
        //open file for output
        FileStream fsOut = new FileStream(strTempFile, FileMode.Open);
        ////convert any extended characters to correct byte value
        // bytText() = ExtCharToByte(mSourceText)
        //write text to file
        bytText = System.Text.Encoding.GetEncoding(437).GetBytes(mSourceText);
        fsOut.Write(bytText, 0,bytText.Length);
        //close file,
        fsOut.Dispose();
      }
      catch (Exception)
      {
        if (blnUnload)
        {
          Unload();
        }
        throw new Exception("582, strErrSource, LoadResString(582)");
      }
      try
      {
        //if savefile exists
        if (File.Exists(SaveFile))
        {
          //delete it
          File.Delete(SaveFile);
        }
        //copy tempfile to savefile
        File.Move(strTempFile, SaveFile);
      }
      catch (Exception)
      {
        throw new Exception("663, LoadResString(663)");
      }
      //if exporting, nothing left to do
      if (Exporting)
      {
        if (blnUnload)
        {
          Unload();
        }
      }
      //reset source dirty flag
      mSourceDirty = false;
      //if in a game, update the source crc
      if (InGame)
      {
        //save the crc value
        WriteGameSetting("Logic" + Number, "CRC32", "0x" + mCRC.ToString("x8"), "Logics");
        //change date of last edit
        agLastEdit = DateTime.Now;
      }
      else
      {
        //update id to match savefile name
        mResID = JustFileName(SaveFile);
      }
      //if unloaded when call to this function was made
      if (blnUnload)
      {
        Unload();
      }
    }
    public void Compile()
    {
      //compiles the source code for this logic
      //and saves the resource

      //if not in a game
      if (!mInGame)
      {
        throw new Exception("618, strErrSource, LoadResString(618)");
      }
      //if not loaded, raise error
      if (!mLoaded)
      {
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //if no data in sourcecode
      if (mSourceText.Length == 0)
      {
        throw new Exception("546, strErrSource, LoadResString(546)");
      }
      try
      {
        //compile the logic
        CompileLogic(this);
        //set dirty flag (forces save to work correctly)
        IsDirty = true;
        //save logic to vol file
        Save();
      }
      catch (Exception)
      {
        // pass it along
        throw;
      }
      //if no error
    }
    public new void Save(string SaveFile = "")
    {
      //this file saves the logic resource to next available VOL file

      //NOTE: this saves the compiled resource NOT the
      //text based source code; 
      //SaveSource saves the source code
      //if Save method is called for a resource NOT in a game,
      //it calls the SaveSource method automatically

      int i;
      string tmpVal;

      //if properties need to be written
      if (WritePropState && mInGame) 
      {
        SaveProps();
      }
      //if not loaded
      if (!mLoaded)
      {
        //error
        throw new Exception("563, strErrSource, LoadResString(563)");
      }
      //if in a game
      if (mInGame)
      {
        //if dirty
        if (mIsDirty)
        {
          try
          {
            //use the base resource save method
            base.Save(SaveFile);
          }
          catch (Exception)
          {
            throw;
          }
        }
        WriteGameSetting("Logic" + Number, "Size", Size.ToString(), "Logics");
     }
      else
      {
        //if source is dirty
        if (mSourceDirty)
        {
          //same as savesource
          SaveSource(SaveFile);
        }
      }
      //reset dirty flag
      IsDirty = false;
    }
    public override bool IsDirty 
    {
      get 
      {
        //if in a game,
        if (InGame)
        {
          //if resource is dirty, or prop values need writing,
          return (mIsDirty || WritePropState);
        }
        else
        {
          return mSourceDirty;
        }
      }
    }
  }
}