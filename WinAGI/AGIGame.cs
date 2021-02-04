﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Engine.AGILogicSourceSettings;
using static WinAGI.Engine.AGICommands;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Common.WinAGI;
using System.Drawing;

namespace WinAGI.Engine
{
  //***************************************************
  // AGIGame Class
  //
  // this class exposes a single AGI game, including  
  // all components for reading/editing
  //
  //***************************************************
  public partial class AGIGame
  {
    //AGIGame agMainGame;

    internal List<string> agGameProps = new List<string> { };

    static readonly string strErrSource = "WinAGI.Engine.Engine";

    //game compile variables
    internal bool agCompGame = false;
    internal bool agCancelComp = false;
    internal bool agChangeVersion = false;

    //local variable(s) to hold property Value(s)
    //for game properties which need to be accessible
    //from all objects in the game system
    internal AGILogics agLogs = new AGILogics();
    internal AGISounds agSnds = new AGISounds();
    internal AGIViews agViews = new AGIViews();
    internal AGIPictures agPics = new AGIPictures();
    internal AGIInventoryObjects agInvObj = new AGIInventoryObjects();
    internal AGIWordList agVocabWords = new AGIWordList();
    internal string agGameDir = "";
    internal string agResDir = "";
    internal string agResDirName = "";
    internal string agDefResDir = "";
    internal string agGameID = "";
    internal string agAuthor = "";
    internal DateTime agLastEdit; //As Date
    internal string agDescription = "";
    internal string agIntVersion = "";
    internal bool agIsVersion3 = false;
    internal string agAbout = "";
    internal string agGameVersion = "";
    internal string agGameFile = "";
    //status of game load
    internal bool agGameLoaded = false;
    internal int agMaxVol0 = 0;
    internal int agMaxVolSize = 0;
    internal string agCompileDir = "";
    internal int agPlatformType = 0;
    // 0 = none
    // 1 = DosBox
    // 2 = ScummVM
    // 3 = NAGI
    // 4 = Other
    internal string agPlatformFile = "";
    internal string agPlatformOpts = "";
    internal string agDOSExec = "";
    internal bool agUseLE = false;

    public AGILogics Logics
    { get => agLogs; set { } }
    public AGIPictures Pictures
    { get => agPics; set { } }
    public AGISounds Sounds
    { get => agSnds; set { } }
    public AGIViews Views
    { get => agViews; set { } }
    public AGIWordList WordList
    { get => agVocabWords; set { } }
    public AGIInventoryObjects InvObjects
    { get => agInvObj; set { } }
    public bool GameLoaded
    { get => agGameLoaded; set { } }
    public void CancelCompile()
    {
      // can be called by parent program during a compile
      // action to cancel the compile

      //if compiling
      if (agCompGame) {
        //reset to force cancel
        agCompGame = false;
        //set flag indicating cancellation
        agCancelComp = true;
      }
    }
    public string DefResDir
    {
      get { return agDefResDir; }
      set
      {

        string NewDir = value;

        NewDir = NewDir.Trim();

        if (NewDir.Length == 0) {
          //throw new Exception("380, strErrSource, "Invalid property Value"
          throw new Exception("Invalid property Value");
        }
        if (NewDir.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
          //throw new Exception("380, strErrSource, "Invalid property Value"
          throw new Exception("Invalid property Value");
        }
        //save new resdir name
        agDefResDir = NewDir;

        //change date of last edit
        agLastEdit = DateTime.Now;
      }
    }
    public string DOSExec
    {
      get { return agDOSExec; }
      set
      {
        string newExec = value;

        //no validation required
        agDOSExec = newExec;

        //if a game is loaded,
        if (agGameLoaded) {
          //write new property
          //              WriteGameSetting("General", "DOSExec", agDOSExec
        }
      }
    }
    public EGAColors EGAColor
    {
      get { return colorEGA; }
      //set { colorEGA = value; }
    }
    public string GameAbout
    {
      get { return agAbout; }
      set
      {
        //limit to 4096 characters
        if (value.Length > 4096)
          agAbout = Left(value, 4096);
        else
          agAbout = value;


        //if a game is loaded,
        if (agGameLoaded)
          //write new property
          WriteGameSetting("General", "About", agAbout);
      }

    }
    public string GameAuthor
    {
      get { return agAuthor; }
      set
      {
        //limit author to 256 bytes
        if (value.Length > 256)
          agAuthor = Left(value, 256);
        else
          agAuthor = value;


        //if game loaded,
        if (agGameLoaded)
          //write property
          WriteGameSetting("General", "Author", agAuthor);
      }
    }
    public string GameDescription
    {
      get => agDescription;
      set
      {
        //comments limited to 4K
        if (value.Length > 4096)
          agDescription = Left(value, 4096);
        else
          agDescription = value;

        //if a game is loaded
        if (agGameLoaded)
          //write new property
          WriteGameSetting("General", "Description", agDescription);
      }
    }
    public string GameDir
    {
      get
      {
        //if a game is loaded,
        if (agGameLoaded) {
          //use resdir property
          return agGameDir;
        }
        else {
          //use current directory
          return CDir(Directory.GetCurrentDirectory());
        }
      }
      set
      {

        //changing directory is only allowed if a game
        //is loaded, but....
        //
        //changing gamedir directly may cause problems
        //because the game may not be able to find the
        //resource files it is looking for;
        //also, changing the gamedir directly does not
        //update the resource directory
        //
        //It is responsibility of calling function to
        //make sure that all game resources/files/
        //folders are also moved/renamed as needed to
        //support the new directory; exception is the
        //agGameFile property, which gets updated
        //in this property (but not moved or created)

        if (agGameLoaded)
          //validate gamedir
          if (Directory.Exists(CDir(value))) //, vbDirectory) 
                                             //return error
                                             //throw new Exception("630, strErrSource, Replace(LoadResString(630), ARG1, NewDir)
                                             //return;
            throw new System.NotImplementedException();

        //change the directory
        agGameDir = CDir(value);

        //update gamefile name
        agGameFile = agGameDir + JustFileName(agGameFile);

        //update resdir
        agResDir = agGameDir + agResDirName + @"\";

        //change date of last edit
        agLastEdit = DateTime.Now;
      }
    }
    public string GameFile
    {
      get
      {
        //error if game not loaded
        if (!agGameLoaded)
          throw new Exception("LoadResString(693)");

        return agGameFile;
      }
      set
      {
        //error if game not loaded
        if (!agGameLoaded)
          throw new Exception("LoadResString(693)");

        //calling function has to make sure NewFile is valid!
        try {
          File.Move(agGameFile, value);
        }
        finally {
          // errors are ignored
        }

        //change property 
        agGameFile = value;

        //change date of last edit
        agLastEdit = DateTime.Now;

        //write date of last edit
        WriteGameSetting("General", "LastEdit", agLastEdit.ToString());

        //save the game prop file
        agGameProps[0] = agGameFile;
        SaveSettingList(agGameProps);
      }
    }
    public int PlatformType
    {
      get => agPlatformType;
      set
      {
        //only 0 - 4 are valid
        if (value < 0 || value > 4)
          agPlatformType = 0;
        else
          agPlatformType = value;


        //if a game is loaded,
        if (agGameLoaded)
          //write new property
          WriteGameSetting("General", "PlatformType", agPlatformType.ToString());
      }
    }
    public string Platform
    {
      get { return agPlatformFile; }
      set
      {
        // no validation needed
        agPlatformFile = value;

        if (agGameLoaded)
          WriteGameSetting("General", "Platform", agPlatformFile);
      }
    }
    public string PlatformOpts
    {
      get { return agPlatformOpts; }
      set
      {
        //no validation required
        agPlatformOpts = value;

        //if a game is loaded,
        if (agGameLoaded)
          //write new property
          WriteGameSetting("General", "PlatformOpts", agPlatformOpts);
      }
    }
    public bool UseLE
    {
      get
      {
        //if a game is not loaded
        if (!agGameLoaded)
          //always return false
          return false;
        else
          //return stored value
          return agUseLE;
      }
      set
      {
        // no validation required
        agUseLE = value;

        //if a game is loaded,
        if (agGameLoaded)
          //write new property
          WriteGameSetting("General", "UseLE", agUseLE.ToString());
      }
    }
    public string GameVersion
    {
      get => agGameVersion;
      set
      {
        //limit to 256 bytes
        if (value.Length > 256)
          agGameVersion = Left(value, 256);
        else
          agGameVersion = value;

        //if game loaded
        if (agGameLoaded)
          //write new property
          WriteGameSetting("General", "GameVersion", agGameVersion);
      }
    }
    public TDefine[] GlobalDefines
    {
      get
      {
        DateTime dtFileMod = File.GetLastWriteTime(agGameDir + "globals.txt");
        if (CRC32(System.Text.Encoding.GetEncoding(437).GetBytes(dtFileMod.ToString())) != agGlobalCRC)
          GetGlobalDefines();
        return agGlobal;
      }
    }
    public void CloseGame()
    {
      //if no game is currently loaded
      if (!agGameLoaded)
        return;
      //unload and remove all resources
      agLogs.Clear();
      agPics.Clear();
      agSnds.Clear();
      agViews.Clear();

      if (agInvObj.Loaded)
        agInvObj.Unload();

      agInvObj.InGame = false;
      if (agVocabWords.Loaded)
        agVocabWords.Unload();

      agVocabWords.InGame = false;


      //restore default AGI colors
      RestoreDefaultColors();

      //write date of last edit
      WriteGameSetting("General", "LastEdit", agLastEdit.ToString());

      //now save it
      SaveSettingList(agGameProps);

      // clear all game properties
      ClearGameState();
    }
    public void CompileGame(bool RebuildOnly, string NewGameDir = "")
    {
      //compiles the game into NewGameDir

      //if RebuildOnly is true, the VOL files are
      //rebuilt without recompiling all logics
      //and WORDS.TOK and OBJECT are not recompiled

      //WARNING: if NewGameDir is same as current directory
      //this WILL overwrite current game files

      //only loaded games can be compiled
      if (!agGameLoaded)
        return;

      //AGILogic tmpLogic = new AGILogic { };
      //AGIPicture tmpPicture = new AGIPicture();
      //AGISound tmpSound = new AGISound { };
      //AGIView tmpView = new AGIView { };
      bool blnReplace, NewIsV3;
      string strFileName = "";
      int tmpMax = 0, i, j;

      //set compiling flag
      agCompGame = true;
      //reset cancel flag
      agCancelComp = false;

      //if no directory passed,
      if (NewGameDir.Length == 0)
        NewGameDir = agGameDir;

      //validate new directory
      NewGameDir = CDir(NewGameDir);
      if (!Directory.Exists(NewGameDir)) {
        //this isn't a directory
        CompleteCancel(true);
        throw new Exception("Replace(LoadResString(561), ARG1, NewGameDir)");
      }

      //set flag if game is being compiled in its current directory
      blnReplace = NewGameDir.Equals(agGameDir, StringComparison.OrdinalIgnoreCase);

      //save compile dir so rebuild method can access it
      agCompileDir = NewGameDir;

      //set new game version
      if (agChangeVersion) {
        NewIsV3 = !agIsVersion3;
      }
      else {
        NewIsV3 = agIsVersion3;
      }

      //ensure switch flag is reset
      agChangeVersion = false;
      string strID;
      //if version 3
      if (NewIsV3) {
        //version 3 ids limited to 5 characters
        if (agGameID.Length > 5) {
          //invalid ID; calling function should know better!
          CompleteCancel(true);
          throw new Exception("LoadResString(694)");
        }
        strID = agGameID;
      }
      else {
        strID = "";
      }

      //if not rebuildonly
      if (!RebuildOnly) {
        // save/copy words.tok and object files first

        //set status
        Raise_CompileGameEvent(ECStatus.csCompWords, 0, 0, "");
        //check for cancellation
        if (!agCompGame) {
          CompleteCancel();
          return;
        }

        //compile WORDS.TOK if dirty
        if (agVocabWords.IsDirty) {
          try {
            agVocabWords.Save();
          }
          catch (Exception ex) {
            //note it
            Raise_CompileGameEvent(ECStatus.csResError, 0, 0, "Error during compilation of WORDS.TOK (" + ex.ToString() + ")");
            //check for cancellation
            if (!agCompGame) {
              CompleteCancel();
              return;
            }
          }
        }

        //if compiling to a different directory
        if (!blnReplace) {
          try {
            // rename then delete existing file, if it exists
            if (File.Exists(NewGameDir + "WORDS.TOK")) {
              // delete the 'old' file if it exists
              if (File.Exists(NewGameDir + "WORDS.OLD")) {
                File.Delete(NewGameDir + "WORDS.OLD");
              }
              File.Move(NewGameDir + "WORDS.TOK", NewGameDir + "WORDS.OLD");
            }
            // then copy the current file to new location
            File.Copy(agVocabWords.ResFile, NewGameDir + "WORDS.TOK");
          }
          catch (Exception) {
            //note error
            Raise_CompileGameEvent(ECStatus.csResError, 0, 0, "Error while creating WORDS.TOK file (Err.Description)");
            //check for cancellation
            if (!agCompGame) {
              CompleteCancel();
              return;
            }
          }
        }
        // OBJECT file is next
        //set status
        Raise_CompileGameEvent(ECStatus.csCompObjects, 0, 0, "");
        //check for cancellation
        if (!agCompGame) {
          CompleteCancel();
          return;
        }

        //compile OBJECT file if dirty
        if (agInvObj.IsDirty) {
          try {
            agInvObj.Save();
          }
          catch (Exception ex) {
            //note it
            Raise_CompileGameEvent(ECStatus.csResError, 0, 0, "Error during compilation of OBJECT (" + ex.ToString() + ")");
            //check for cancellation
            if (!agCompGame) {
              CompleteCancel();
              return;
            }
          }
        }
        //if compiling to a different directory
        if (!blnReplace) {
          try {
            // rename then delete existing file, if it exists
            if (File.Exists(NewGameDir + "OBJECT")) {
              //first, delete the 'old' file if it exists
              if (File.Exists(NewGameDir + "OBJECT.OLD")) {
                File.Delete(NewGameDir + "OBJECT.OLD");
              }
              File.Move(NewGameDir + "OBJECT", NewGameDir + "OBJECT.OLD");
            }
            // then copy the current file to new location
            File.Copy(agInvObj.ResFile, NewGameDir + "OBJECT");
          }
          catch (Exception ex) {
            //note error
            Raise_CompileGameEvent(ECStatus.csResError, 0, 0, "Error while creating OBJECT file (" + ex.ToString() + ")");
            //check for cancellation
            if (!agCompGame) {
              CompleteCancel();
              return;
            }
          }
        }
      }

      //reset game compiler variables
      lngCurrentVol = 0;
      lngCurrentLoc = 0;
      strNewDir = NewGameDir;

      //new strategy with dir files; use arrays during
      //compiling process; then build the dir files at
      //the end; initialize the array
      for (i = 0; i < 4; i++) {
        for (j = 0; i < 768; i++) {
          bytDIR[i, j] = 255;
        }
      }

      try {
        //ensure all temp vol files are removed
        for (int v = 0; v < 15; v++) {
          if (File.Exists(NewGameDir + "NEW_VOL." + v.ToString())) {
            File.Delete(NewGameDir + "NEW_VOL." + v.ToString());
          }
        }
      }
      catch (Exception) {
        //ignore errors
      }

      try {
        //open first new vol file
        fsVOL = File.Create(NewGameDir + "NEW_VOL.0");
        bwVOL = new BinaryWriter(fsVOL);
      }
      catch (Exception) {
        CompleteCancel(true);
        throw new Exception("Replace(LoadResString(503), ARG1, Exception");
      }

      //add all logic resources
      try {
        CompileResCol(agLogs, AGIResType.rtLogic, RebuildOnly, NewIsV3);
      }
      catch (Exception ex) {
        CompleteCancel(true);
        throw new Exception("err: " + ex.ToString());
      }
      //check for cancellation
      //if a resource error (or user canceled) encountered, just exit
      if (!agCompGame) {
        return;
      }

      //add picture resources
      try {
        CompileResCol(agPics, AGIResType.rtPicture, RebuildOnly, NewIsV3);
      }
      catch (Exception ex) {
        CompleteCancel(true);
        throw new Exception("err: " + ex.ToString());
      }
      //check for cancellation
      //if a resource error (or user canceled) encountered, just exit
      if (!agCompGame) {
        return;
      }

      //add view resources
      try {
        CompileResCol(agViews, AGIResType.rtView, RebuildOnly, NewIsV3);
      }
      catch (Exception ex) {
        CompleteCancel(true);
        throw new Exception("err " + ex.ToString());
      }
      //check for cancellation
      //if a resource error (or user canceled) encountered, just exit
      if (!agCompGame) {
        return;
      }

      //add sound resources
      try {
        CompileResCol(agSnds, AGIResType.rtSound, RebuildOnly, NewIsV3);
      }
      catch (Exception ex) {
        CompleteCancel(true);
        throw new Exception("err: " + ex.ToString());
      }
      //check for cancellation
      //if a resource error (or user canceled) encountered, just exit
      if (!agCompGame) {
        return;
      }

      //remove any existing old dirfiles
      if (NewIsV3) {
        if (File.Exists(NewGameDir + agGameID + "DIR.OLD")) {
          File.Delete(NewGameDir + agGameID + "DIR.OLD");
        }
      }
      else {
        if (File.Exists(NewGameDir + "LOGDIR.OLD")) {
          File.Delete(NewGameDir + "LOGDIR.OLD");
        }
        if (File.Exists(NewGameDir + "PICDIR.OLD")) {
          File.Delete(NewGameDir + "PICDIR.OLD");
        }
        if (File.Exists(NewGameDir + "VIEWDIR.OLD")) {
          File.Delete(NewGameDir + "VIEWDIR.OLD");
        }
        if (File.Exists(NewGameDir + "SNDDIR.OLD")) {
          File.Delete(NewGameDir + "SNDDIR.OLD");
        }
      }

      //rename existing dir files as .OLD
      if (NewIsV3) {
        if (File.Exists(NewGameDir + agGameID + "DIR")) {
          File.Move(NewGameDir + agGameID + "DIR", NewGameDir + agGameID + "DIR.OLD");
          File.Delete(NewGameDir + agGameID + "DIR");
        }
      }
      else {
        if (File.Exists(NewGameDir + "LOGDIR")) {
          File.Move(NewGameDir + "LOGDIR", NewGameDir + "LOGDIR.OLD");
          File.Delete(NewGameDir + "LOGDIR");
        }
        if (File.Exists(NewGameDir + "PICDIR")) {
          File.Move(NewGameDir + "PICDIR", NewGameDir + "PICDIR.OLD");
          File.Delete(NewGameDir + "PICDIR");
        }
        if (File.Exists(NewGameDir + "VIEWDIR")) {
          File.Move(NewGameDir + "VIEWDIR", NewGameDir + "VIEWDIR.OLD");
          File.Delete(NewGameDir + "VIEWDIR");
        }
        if (File.Exists(NewGameDir + "SNDDIR")) {
          File.Move(NewGameDir + "SNDDIR", NewGameDir + "SNDDIR.OLD");
          File.Delete(NewGameDir + "SNDDIR");
        }
      }

      //now build the new DIR files
      if (NewIsV3) {
        //one dir file
        strFileName = NewGameDir + agGameID + "DIR";
        fsDIR = File.Create(strFileName);
        bwDIR = new BinaryWriter(fsDIR);
        //add offsets - logdir offset is always 8
        bwDIR.Write(Convert.ToInt16(8));
        //pic offset is 8 + 3*logmax
        tmpMax = agLogs.Max + 1;
        if (tmpMax == 0) {
          // always put at least one; even if it's all FFs
          tmpMax = 1;
        }
        bwDIR.Write((short)(8 + 3 * tmpMax));
        i = 8 + 3 * tmpMax;
        //view offset is pic offset + 3*picmax
        tmpMax = agPics.Max + 1;
        if (tmpMax == 0) {
          tmpMax = 1;
        }
        bwDIR.Write((short)(i + 3 * tmpMax));
        i += 3 * tmpMax;
        //sound is view offset + 3*viewmax
        tmpMax = agViews.Max + 1;
        if (tmpMax == 0) {
          tmpMax = 1;
        }
        bwDIR.Write((short)(i + 3 * tmpMax));

        //now add all the dir entries
        //NOTE: we can't use a for-next loop
        //because sound and view dirs are swapped in v3 directory

        //logics first
        tmpMax = agLogs.Max;
        for (i = 0; i <= tmpMax; i++) {
          bwDIR.Write(bytDIR[0, 3 * i]);
          bwDIR.Write(bytDIR[0, 3 * i + 1]);
          bwDIR.Write(bytDIR[0, 3 * i + 2]);
        }
        //next are pictures
        tmpMax = agPics.Max;
        for (i = 0; i <= tmpMax; i++) {
          bwDIR.Write(bytDIR[1, 3 * i]);
          bwDIR.Write(bytDIR[1, 3 * i + 1]);
          bwDIR.Write(bytDIR[1, 3 * i + 2]);
        }
        //then views
        tmpMax = agViews.Max;
        for (i = 0; i <= tmpMax; i++) {
          bwDIR.Write(bytDIR[3, 3 * i]);
          bwDIR.Write(bytDIR[3, 3 * i + 1]);
          bwDIR.Write(bytDIR[3, 3 * i + 2]);
        }
        //and finally, sounds
        tmpMax = agSnds.Max;
        for (i = 0; i <= tmpMax; i++) {
          bwDIR.Write(bytDIR[2, 3 * i]);
          bwDIR.Write(bytDIR[2, 3 * i + 1]);
          bwDIR.Write(bytDIR[2, 3 * i + 2]);
        }
        //done! close the stream and file
        fsDIR.Dispose();
        bwDIR.Dispose();
      }
      else {
        //make separate dir files
        for (j = 0; j < 4; j++) {
          switch ((AGIResType)j) {
          case AGIResType.rtLogic:
            strFileName = NewGameDir + "LOGDIR";
            tmpMax = agLogs.Max;
            break;
          case AGIResType.rtPicture:
            strFileName = NewGameDir + "PICDIR";
            tmpMax = agPics.Max;
            break;
          case AGIResType.rtSound:
            strFileName = NewGameDir + "SNDDIR";
            tmpMax = agSnds.Max;
            break;
          case AGIResType.rtView:
            strFileName = NewGameDir + "VIEWDIR";
            tmpMax = agViews.Max;
            break;
          }
          //create the dir file
          fsDIR = File.Create(strFileName);
          bwDIR = new BinaryWriter(fsDIR);
          for (i = 0; i <= tmpMax; i++) {
            bwDIR.Write(bytDIR[j, 3 * i]);
            bwDIR.Write(bytDIR[j, 3 * i + 1]);
            bwDIR.Write(bytDIR[j, 3 * i + 2]);
          }
          fsDIR.Dispose();
          bwDIR.Dispose();
        }
      }

      //remove any existing old vol files
      for (i = 0; i < 16; i++) {
        if (NewIsV3) {
          if (File.Exists(NewGameDir + agGameID + "VOL." + i.ToString() + ".OLD")) {
            File.Delete(NewGameDir + agGameID + "VOL." + i.ToString() + ".OLD");
          }
        }
        else {
          if (File.Exists(NewGameDir + "VOL." + i.ToString() + ".OLD")) {
            File.Delete(NewGameDir + "VOL." + i.ToString() + ".OLD");
          }
        }
      }

      //rename current vol files
      for (i = 0; i < 16; i++) {
        if (NewIsV3) {
          if (File.Exists(NewGameDir + agGameID + "VOL." + i.ToString())) {
            File.Move(NewGameDir + agGameID + "VOL." + i.ToString(), NewGameDir + agGameID + "VOL." + i.ToString() + ".OLD");
          }
        }
        else {
          if (File.Exists(NewGameDir + "VOL." + i.ToString())) {
            File.Move(NewGameDir + "VOL." + i.ToString(), NewGameDir + "VOL." + i.ToString() + ".OLD");
          }
        }
      }
      // now rename VOL files
      for (i = 0; i <= lngCurrentVol; i++) {
        strFileName = strID + "VOL." + i.ToString();
        File.Move(NewGameDir + "NEW_VOL." + i.ToString(), NewGameDir + strFileName);
      }
      //update status to indicate complete
      Raise_CompileGameEvent(ECStatus.csCompileComplete, 0, 0, "");
      //if replacing (meaning we are compiling to the current game directory)
      if (blnReplace) {
        //then we need to update the vol/loc info for all ingame resources;
        //this is done here instead of when the logics are compiled because
        //if there's an error, or the user cancels, we don't want the resources
        //to point to the wrong place
        foreach (AGILogic tmpLogic in agLogs.Col.Values) {
          tmpLogic.Volume = (sbyte)(bytDIR[0, tmpLogic.Number * 3] >> 4);
          tmpLogic.Loc = (bytDIR[0, tmpLogic.Number * 3] + 0xF) * 0x10000 + bytDIR[0, tmpLogic.Number * 3 + 1] * 0x100 + bytDIR[0, tmpLogic.Number * 3 + 2];
        }
        foreach (AGIPicture tmpPicture in agPics.Col.Values) {
          tmpPicture.Volume = (sbyte)(bytDIR[1, tmpPicture.Number * 3] >> 4);
          tmpPicture.Loc = (bytDIR[1, tmpPicture.Number * 3] + 0xF) * 0x10000 + bytDIR[1, tmpPicture.Number * 3 + 1] * 0x100 + bytDIR[1, tmpPicture.Number * 3 + 2];
        }
        foreach (AGISound tmpSound in agSnds.Col.Values) {
          tmpSound.Volume = (sbyte)(bytDIR[2, tmpSound.Number * 3] >> 4);
          tmpSound.Loc = (bytDIR[2, tmpSound.Number * 3] + 0xF) * 0x10000 + bytDIR[2, tmpSound.Number * 3 + 1] * 0x100 + bytDIR[2, tmpSound.Number * 3 + 2];
        }
        foreach (AGIView tmpView in agViews.Col.Values) {
          tmpView.Volume = (sbyte)(bytDIR[3, tmpView.Number * 3] >> 4);
          tmpView.Loc = (bytDIR[3, tmpView.Number * 3] + 0xF) * 0x10000 + bytDIR[3, tmpView.Number * 3 + 1] * 0x100 + bytDIR[3, tmpView.Number * 3 + 2];
        }
      }

      //save the wag file
      SaveSettingList(agGameProps);

      //reset compiling flag
      agCompGame = false;
      return;

      //ErrHandler:
      ////file error when creating new DIR and VOL files
      ////pass along any errors
      //lngError = Err.Number
      //Select case lngError
      //case 58 //file already exists
      //  //this means the previous version of the file we are trying to create
      //  //wasn't able to be renamed; most likely due to being open
      //  strError = "Unable to delete existing " + strFileName + ". New " + strFileName + " file was not created."
      //case  else 
      //  //some other error - who knows what's going on
      //  strError = Err.Description
      //}


      //CompleteCancel(true);
      //throw new Exception("lngError, strErrSource, strError
    }
    public string GameID
    {
      get
      {
        //id is undefined if a game is not loaded
        if (!agGameLoaded) {
          throw new Exception("LoadResString(677)");
        }
        return agGameID;
      }
      set
      {
        //limit gameID to 6 characters for v2 games and 5 characters for v3 games

        string NewID = value;

        //id is undefined if a game is not loaded
        if (!agGameLoaded) {
          throw new Exception("LoadResString(677)");
        }
        //version 3 games limited to 5 characters
        if (agIsVersion3) {
          if (value.Length > 5) {
            NewID = Left(NewID, 5);
          }
        }
        else {
          if (NewID.Length > 6) {
            NewID = Left(NewID, 6);
          }
        }

        //if no change
        if (agGameID == NewID) {
          return;
        }

        //if version 3
        if (agIsVersion3) {
          //need to rename the dir file
          File.Move(agGameDir + agGameID + "DIR", agGameDir + NewID + "DIR");
          //delete old dirfile
          File.Delete(agGameDir + agGameID + "DIR");

          //and vol files
          foreach (string strVolFile in Directory.EnumerateFiles(agGameDir, agGameID + "VOL.*")) {
            //if an archived (OLD) file, skip it
            if (Right(strVolFile, 4).ToUpper() != ".OLD") {
              //get extension
              string[] strExtension = strVolFile.Split(".");
              //rename
              File.Move(agGameDir + strVolFile, agGameDir + NewID + "VOL." + strExtension[1]);
              //TODO: delete the old one
              File.Delete(agGameDir + strVolFile);
            }
          }
        }
        //if property file is currently linked to game ID
        if (agGameFile.Equals(agGameDir + agGameID + ".wag", StringComparison.OrdinalIgnoreCase)) {
          //update gamefile
          GameFile = agGameDir + NewID + ".wag";
        }

        //set new id
        agGameID = NewID.ToUpper();

        //write new property
        WriteGameSetting("General", "GameID", NewID);
        //ErrHandler:
        //  //file renaming or property writing are only sources of error
        //  strError = Err.Description
        //  strErrSrc = Err.Source
        //  lngError = Err.Number

        //  throw new Exception("530, strErrSrc, Replace(LoadResString(530), ARG1, CStr(lngError) + ":" + strError)
      }
    }
    public DateTime LastEdit
    {
      get
      {
        //if game loaded,
        if (agGameLoaded) {
          return agLastEdit;
        }
        else {
          return DateTime.Now;
        }
      }
    }
    public AGILogicSourceSettings LogicSourceSettings
    {
      get
      {
        return agMainLogSettings;
      }
      set
      {
        agMainLogSettings = value;

        //change date of last edit
        agLastEdit = DateTime.Now;
      }
    }
    public int MaxVol0Size
    {
      get { return agMaxVol0; }
      set
      {
        //validate
        if (value < 32768) {
          agMaxVol0 = 32768;
        }
        else if (value != MAX_VOLSIZE) {
          agMaxVol0 = MAX_VOLSIZE;
        }
        else {
          agMaxVol0 = value;
        }
      }
    }
    public string InterpreterVersion
    {
      get
      {
        // if a game is loaded
        if (agGameLoaded) {
          return agIntVersion;
        }
        // if no version yet
        if (agIntVersion.Length == 0) {
          // use 2.917
          return "2.917";
        }
        //otherwise return set value
        return agIntVersion;
      }
      set
      {
        //attempts to set version to new Value

        //validate new version
        // if (ValidateVersion(value))
        if (IntVersions.Contains(value)) {
          //if in a game, need to adjust resources if necessary
          if (agGameLoaded) {
            //if new version and old version are not same major version:
            if (agIntVersion[0] != value[0]) {
              //set flag to switch version on rebuild
              agChangeVersion = true;

              //if new version is V3 (i.e., current version isn't)
              if (!agIsVersion3) {
                if (GameID.Length > 5) {
                  //truncate the ID
                  GameID = Left(GameID, 5);
                }
              }
              //use compiler to rebuild new vol and dir files
              //(it is up to calling program to deal with dirty and invalid resources)
              try {
                CompileGame(true);
              }
              catch (Exception ex) {

                //reset cancel flag
                agCancelComp = false;

                // pass along the exception
                throw new Exception(ex.Message);
              }

              //check for user cancellation
              if (agCancelComp) {
                //reset cancel flag
                agCancelComp = false;
                // and quit
                return;
              }
            }

            //change date of last edit
            agLastEdit = DateTime.Now;

            //write new version to file
            WriteGameSetting("General", "Interpreter", value);

            //and save it
            SaveSettingList(agGameProps);
          }

          //OK to set new version
          agIntVersion = value;

          //set v3 flag
          agIsVersion3 = agIntVersion[0] == '3';

          //adjust commands
          CorrectCommands(agIntVersion);
        }
        else {
          // if not numeric
          if (!IsNumeric(value))
            throw new Exception("LoadResString(597)");
          else if (Double.Parse(value) < 2 || Double.Parse(value) > 3)
            //not a version 2 or 3 game
            throw new Exception("LoadResString(597)");
          else
            //unsupported version 2 or 3 game
            throw new Exception("LoadResString(543)");
        }
      }
    }
    public string ResDir
    {
      get
      {
        // if a game is loaded
        if (agGameLoaded) {
          return agResDir;
        }
        //otherwise use current directory
        return CDir(System.IO.Directory.GetCurrentDirectory());
      }
      set
      {

      }
    }
    public string ResDirName
    {
      get { return agResDirName; }
      set
      {
        //validate- cant have  \/:*?<>|
        // (VB didn't allow spaces, but they are perfectly fine in directory names)

        string tmpName = value.Trim();

        // ignore blank
        if (tmpName.Length == 0)
          return;

        if (@"\/:*?<>|".Any(tmpName.Contains)) {
          throw new Exception("380, Invalid property Value");
        }

        //save new resdir name
        agResDirName = tmpName;
        //update the actual resdir
        agResDir = agGameDir + agResDirName + @"\";

        //save resdirname
        WriteGameSetting("General", "ResDir", agResDirName);

        //change date of last edit
        agLastEdit = DateTime.Now;
      }
    }
    public void SaveProperties()
    {
      //this forces WinAGI to save the property file, rather than waiting until
      //the game is unloaded

      if (agGameLoaded)
        SaveSettingList(agGameProps);
    }
    public int OpenGameDIR(string NewGameDir)
    {
      //creates a new WinAGI game file from Sierra game directory

      //if a game is already open,
      if (agGameLoaded) {
        //can't open a game if one is already open
        return WINAGI_ERR + 501;
        //throw new Exception("LoadResString(501)");
      }

      //set game directory
      agGameDir = CDir(NewGameDir);

      //attempt to extract agGameID, agGameFile and interpreter version
      //from Sierra files (i.e. from DIR files and VOL files)

      //set status to decompiling game dir files
      Raise_LoadGameEvent(ELStatus.lsDecompiling, AGIResType.rtNone, 0, "");

      //check for valid DIR/VOL files
      //(which gets gameid, and sets version 3 status flag)
      if (!IsValidGameDir(agGameDir)) {
        //save dir as error string
        //clear game variables
        ClearGameState();
        //invalid game directory
        return WINAGI_ERR + 541;
        //throw new Exception("LoadResString(541), ARG1, agGameDir");
      }

      //create a new wag file name
      agGameFile = agGameDir + agGameID + ".wag";

      //rename any existing game file (in case user is re-importing)
      if (File.Exists(agGameFile)) {
        if (File.Exists(agGameFile + ".OLD")) {
          File.Delete(agGameFile + ".OLD");
        }
        File.Move(agGameFile, agGameFile + ".OLD");
      }

      //create new wag file, but don't save it yet
      agGameProps = OpenSettingList(agGameFile, false);
      agGameProps.Add("# WinAGI Game Property File");
      agGameProps.Add("#");
      WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION);
      WriteGameSetting("General", "GameID", agGameID);

      //get version number (version3 flag already set)
      agIntVersion = GetIntVersion();

      //if not valid
      if (agIntVersion.Length == 0) {
        //clear game variables
        ClearGameState();
        //invalid number found
        return WINAGI_ERR + 543;
        //throw new Exception("LoadResString(543)");
      }

      //save version
      WriteGameSetting("General", "Interpreter", agIntVersion);

      //finish the game load
      return FinishGameLoad(1);
    }
    internal void ClearGameState()
    {
      //clears basic game variables when a game is
      //not loaded

      //since this happen a lot, use a function to make it easier

      // no game is loaded
      agGameLoaded = false;

      //clear out game properties
      agGameID = "";
      agIntVersion = "";
      agIsVersion3 = false;
      agGameProps = new List<string> { };
      agLastEdit = new DateTime();// Convert.ToDateTime("");

      // reset directories
      agGameDir = "";
      agResDir = "";
      agGameFile = "";

      //reset global CRC
      agGlobalCRC = 0;

      // clear dev properties
      agDescription = "";
      agAuthor = "";
      agGameVersion = "";
      agAbout = "";
      agResDirName = "";
      agPlatformType = 0;
      agPlatformFile = "";
      agPlatformOpts = "";
      agDOSExec = "";
    }
    public int OpenGameWAG(string GameWAG)
    {
      //TODO: all game manipulation functions (open, new, finish, close, etc
      // should be bool or int functions, instead of using error handler

      //opens a WinAGI game file (must be passed as a full length file name)

      string strVer;

      //if a game is already open,
      if (agGameLoaded) {
        //can't open a game if one is already open
        return WINAGI_ERR + 501;
      }

      //verify the wag file exists
      if (!File.Exists(GameWAG)) {
        //clear game variables
        ClearGameState();

        //is the file missing?, or the directory?
        if (Directory.Exists(JustPath(GameWAG, true))) {
          //it's a missing file - return wagfile as error string
          //invalid wag
          return WINAGI_ERR + 655;
          //throw new Exception("655, JustFileName(GameWAG)");
        }
        else {
          //it's an invalid or missing directory - return directory as error string
          //invalid wag
          return WINAGI_ERR + 541;
          //throw new Exception("541, strErrSource, JustPath(GameWAG, true)");
        }
      }

      //set game file property
      agGameFile = GameWAG;

      //set game directory
      agGameDir = JustPath(GameWAG);

      //open the property file (file has to already exist)
      try {
        agGameProps = OpenSettingList(agGameFile, false);
      }
      catch (Exception) {
        //// reset game variables
        //ClearGameState();
      }
      //check to see if it's valid
      strVer = ReadSettingString(agGameProps, "General", "WinAGIVersion", "");
      if (strVer.Length == 0) {
        //not a valid file; maybe it's an old binary version
        ConvertWag();
      }
      else {
        if (strVer != WINAGI_VERSION) {
          if (Left(strVer, 4) == "1.2." || (Left(strVer, 4) == "2.1.")) {
            //any v1.2.x or 2.1.x is ok, but update
            // TODO: after testing, re-enable version updating
            //            WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION);
          }
          else {
            //if another winagi version is ever released,
            //yeah, right; just consider this file invalid

            //clear game variables
            ClearGameState();
            //invalid wag
            return WINAGI_ERR + 665;
            //throw new Exception("665,JustFileName(agGameFile) ");
          }
        }
      }
      //get gameID
      agGameID = ReadSettingString(agGameProps, "General", "GameID", "");

      //if an id is found, keep going
      if (agGameID.Length > 0) {
        //got ID; now get interpreter version from propfile
        agIntVersion = ReadSettingString(agGameProps, "General", "Interpreter", "");

        //validate it
        if (!IntVersions.Contains(agIntVersion)) {
          //save wagfile as error string
          //clear game variables
          ClearGameState();
          //invalid int version inside wag file
          return WINAGI_ERR + 691;
          //throw new Exception("691, JustFileName(agGameFile)");
        }
      }
      else {
        //missing GameID in wag file - make user address it
        //save wagfile name as error string
        //clear game variables
        ClearGameState();
        //invalid wag file
        return WINAGI_ERR + 690;
        //throw new Exception("690, JustFileName(agGameFile)");
      }

      //if a valid wag file was found, we now have agGameID, agGameFile
      //and correct interpreter version;

      //finish the game load
      return FinishGameLoad(0);
    }
    public int FinishGameLoad(int Mode = 0)
    {
      //finishes game load
      //mode determines whether opening by wag file (0) or
      //extracting from Sierra game files (1)
      //(currently, no difference between them)

      //instead of throwing exceptions, errors get psse back as a return value

      bool blnWarnings;
      string strError;

      //set v3 flag
      agIsVersion3 = (Val(agIntVersion) >= 3);
      //if loading from a wag file
      if (Mode == 0) {
        //informs user we are checking property file to set game parameters
        Raise_LoadGameEvent(ELStatus.lsPropertyFile, AGIResType.rtNone, 0, "");

        //get resdir before loading resources
        agResDirName = ReadSettingString(agGameProps, "General", "ResDir", "");
      }
      else {
        //no resdir yet for imported game
        agResDirName = "";
      }

      //if none, check for existing, or use default
      if (agResDirName.Length == 0) {
        //look for an existing directory

        if (Directory.GetDirectories(agGameDir).Length == 1) {
          //assume it is resource directory
          agResDirName = Directory.GetDirectories(agGameDir)[0];
        }
        else {
          //either no subfolders, or more than one
          //so use default
          agResDirName = agDefResDir;
        }
        WriteGameSetting("General", "ResDir", JustFileName(agResDirName));
      }
      //now create full resdir from name
      agResDir = agGameDir + agResDirName + @"\";

      //ensure resource directory exists
      if (!Directory.Exists(agResDir)) {
        try {
          Directory.CreateDirectory(agResDir);
        }
        catch (Exception) {
          //if can't create the resources directory
          //note the problem in the error log as a warning
          RecordLogEvent(LogEventType.leWarning, "Can't create " + agResDir);
          //use game directory
          agResDir = agGameDir;
          //set warning
          blnWarnings = true;
        }
      }

      try {
        //  //if loading from a WAG file
        //
        //  if (Mode == 0) {
        //    //only resources in the WAG file need to be checked on initial load
        //    blnWarnings = LoadResources()
        //  } else {
        //    //if opening by directory, extract resources from the VOL
        //    //files load resources
        blnWarnings = ExtractResources();
        //  }
      }
      catch (Exception e) {
        //if there was an error
        //can't continue
        //save error information
        strError = e.Message;
        //lngError = Err.Number

        //clear game variables
        ClearGameState();

        //reset collections + objects
        agLogs = new AGILogics();
        agViews = new AGIViews();
        agPics = new AGIPictures();
        agSnds = new AGISounds();
        agInvObj = new AGIInventoryObjects();
        agVocabWords = new AGIWordList();

        agGameProps = new List<string> { };

        return WINAGI_ERR + e.HResult;
        ////raise error
        //throw new Exception("lngError, strErrSource, strError");
      }
      //load vocabulary word list
      Raise_LoadGameEvent(ELStatus.lsResources, AGIResType.rtWords, 0, "");
      try {
        agVocabWords = new AGIWordList();
        agVocabWords.Init();
        agVocabWords.Load(agGameDir + "WORDS.TOK");
      }
      catch (Exception e) {
        //if there was an error,
        //note the problem in the error log as a warning
        RecordLogEvent(LogEventType.leError, "An error occurred while loading WORDS.TOK: " + e.Message);
        //reset warning flag
        blnWarnings = true;
      }

      //load inventory objects list
      Raise_LoadGameEvent(ELStatus.lsResources, AGIResType.rtObjects, 0, "");
      try {
        agInvObj = new AGIInventoryObjects();
        agInvObj.Init();
        agInvObj.Load(agGameDir + "OBJECT");
      }
      catch (Exception e) {
        //if there was an error,
        //note the problem in the error log as a warning
        RecordLogEvent(LogEventType.leError, "An error occurred while loading OBJECT: " + e.Message);
        //reset warning flag
        blnWarnings = true;
      }
      Raise_LoadGameEvent(ELStatus.lsFinalizing, AGIResType.rtNone, 0, "");
      // adust commands based on AGI version
      CorrectCommands(agIntVersion);
      //clear other game properties
      agAuthor = "";
      agDescription = "";
      agGameVersion = "";
      agAbout = "";
      agPlatformType = 0;
      agPlatformFile = "";
      agPlatformOpts = "";
      agDOSExec = "";
      agLastEdit = new DateTime();
      try {
        //get rest of game properties
        GetGameProperties();
      }
      catch (Exception e) {
        //record event
        RecordLogEvent(LogEventType.leWarning, "Error while loading WAG file; some properties not loaded. (Error number: " + e.Message + ")");
        blnWarnings = true;
      }
      //force id reset
      blnSetIDs = false;
      //we've established that the game can be opened
      //so set the loaded flag now
      agGameLoaded = true;
      //write create date
      WriteGameSetting("General", "LastEdit", agLastEdit.ToString());
      //and save the wag file
      SaveSettingList(agGameProps);
      //if errors
      if (blnWarnings) {
        return WINAGI_ERR + 636;
      }
      else {
        return 0;
      }
    }
    public AGIGame()
    {
      // enable encoding access to codepage 437; this gives us access to the standard MSDOS extended characters
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
  }
  public class EGAColors
  {
    Color[] colorEGA = new Color[16];
    public Color this[int index]
    {
      get
      {
        if (index < 0 || index > 15) {
          throw new IndexOutOfRangeException("bad color");
        }
        return colorEGA[index];
      }
      set
      {
        if (index < 0 || index > 15) {
          throw new IndexOutOfRangeException("bad color");
        }
        colorEGA[index] = value;
        // if color for a game is changed, how to let the game object know???
        if (agGameLoaded) {
          WriteGameSetting("Palette", "Color" + index, ColorText(index));
        }
      }
    }
  }
}
