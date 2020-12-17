using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static WinAGI.WinAGI;
using static WinAGI.AGILogicSourceSettings;
using static WinAGI.AGICommands;

namespace WinAGI
{
  public static partial class AGIGame
  {
    //AGIGame agMainGame;

    // exposed game properties, methods, objects
    public static AGILogicSourceSettings agMainLogSettings = new AGILogicSourceSettings();

    internal static List<string> agGameProps = new List<string> { };// As StringList

    static string strErrSource = "";

    //game compile variables
    static bool agCompGame = false;
    static bool agCancelComp = false;
    static bool agChangeVersion = false;

    //local variable(s) to hold property Value(s)
    //for game properties which need to be accessible
    //from all objects in the game system
    internal static AGILogics agLogs = new AGILogics();
    internal static AGISounds agSnds = new AGISounds();
    internal static AGIViews agViews = new AGIViews();
    internal static AGIPictures agPics = new AGIPictures();
    internal static AGIInventoryObjects agInvObjList = new AGIInventoryObjects();
    static AGIWordList agVocabWords = new AGIWordList();
    public static AGILogics Logics
    { get => agLogs; set { } }
    public static AGIPictures Pictures
    { get => agPics; set { } }
    public static AGISounds Sounds
    { get => agSnds; set { } }
    public static AGIViews Views
    { get => agViews; set { } }
    public static AGIWordList WordList
    { get => agVocabWords; set { } }
    public static AGIInventoryObjects InvObjects
    { get => agInvObjList; set { } }

    //status of game load
    static bool agGameLoaded = false;
    public static bool GameLoaded
    { get => agGameLoaded; set { } }

    internal static string agGameDir = "";
    internal static string agResDir = "";
    internal static string agResDirName = "";
    internal static string agDefResDir = "";
    internal static string agTemplateDir = "";
    internal static string agGameID = "";
    internal static string agAuthor = "";
    internal static DateTime agLastEdit; //As Date
    internal static string agDescription = "";
    internal static string agIntVersion = "";
    internal static bool agIsVersion3 = false;
    internal static string agAbout = "";
    internal static string agGameVersion = "";
    internal static string agGameFile = "";

    internal static int agMaxVol0 = 0;
    internal static string agCompileDir = "";
    internal static int agPlatformType = 0;
    // 0 = none
    // 1 = DosBox
    // 2 = ScummVM
    // 3 = NAGI
    // 4 = Other
    internal static string agPlatformFile = "";
    internal static string agPlatformOpts = "";
    internal static string agDOSExec = "";
    internal static bool agUseLE = false;

    internal static string agSrcExt = "";

    //error number and string to return error values
    //from various functions/subroutines
    static int lngError = 0;
    static string strError = "";
    static string strErrSrc = "";

    //temp file location
    internal static string TempFileDir = "";
    public static void CancelCompile()
    {
      // can be called by parent program during a compile
      // action to cancel the compile

      //if compiling
      if (agCompGame)
      {
        //reset to force cancel
        agCompGame = false;
        //set flag indicating cancellation
        agCancelComp = true;
      }
    }

    public static string DefResDir
    {
      get { return agDefResDir; }
      set
      {

        string NewDir = value;

        //validate- cant have  \/:*?<>|

        NewDir = NewDir.Trim();

        if (NewDir.Length == 0)
        {
          //throw new Exception("380, strErrSource, "Invalid property Value"
          throw new Exception("Invalid property Value");
        }
        if ("\\/:*?<>|".Any(NewDir.Contains))
          //throw new Exception("380, strErrSource, "Invalid property Value"
          throw new Exception("Invalid property Value");

        // ? space should be OK in a directory!
        //if (NewDir.IndexOf(" ") != 0) {
        //  //throw new Exception("380, strErrSource, "Invalid property Value"
        //  throw new Exception("Invalid property Value");
        //}

        //save new resdir name
        agDefResDir = NewDir;

        //change date of last edit
        agLastEdit = DateTime.Now;
      }
    }
    public static string DOSExec
    {
      get { return agDOSExec; }
      set
      {
        string newExec = value;

        //no validation required
        agDOSExec = newExec;

        //if a game is loaded,
        if (agGameLoaded)
        {
          //write new property
          //              WriteGameSetting("General", "DOSExec", agDOSExec
        }
      }
    }

    public static int EGAColor(int index)
    {
      //in VB (and other languages?) colors are four byte:
      //         xxbbggrr,
      // this is the format used by EGAColor

      //in API calls, the red and blue values are reversed:
      //         xxrrggbb
      //(perhaps because of way CopyMemory works?)
      //so API calls need to use EGARevColor instead

      //validate index
      if (index < 0 || index > 15)
        //throw new Exception("9, strErrSource, "Subscript out of range"
        throw new Exception("Subscript out of range");
      return lngEGACol[index];
    }
    public static void EGAColorSet(int index, int newcolor)
    {
      //in VB (and other languages?) colors are four byte:
      //         xxbbggrr,
      // this is the format used by EGAColor

      //in API calls, the red and blue values are reversed:
      //         xxrrggbb
      //(perhaps because of way CopyMemory works?)
      //so API calls need to use EGARevColor instead

      //validate index
      if (index < 0 || index > 15)
        //throw new Exception("9, strErrSource, "Subscript out of range"
        throw new Exception("Subscript out of range");

      //store the new color
      lngEGACol[index] = newcolor;

      //now invert red and blue components for revcolor
      lngEGARevCol[index] = (int)(newcolor & 0xFF000000) + (newcolor & 0xFF) * 0x10000 + (newcolor & 0xFF00) + (newcolor & 0xFF0000) / 0x10000;

      //if in a game, save the color in the game's WAG file
      if (agGameLoaded)
        WriteGameSetting("Palette", "Color" + index, "&H" + newcolor.ToString("X"));
    }

    public static int[] EGARevColor
    { get { return lngEGARevCol; } set { } }

    public static string GameAbout
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

    public static string GameAuthor
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

    public static string GameDescription
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

    public static string GameDir
    {
      get
      {
        //if a game is loaded,
        if (agGameLoaded)
        {
          //use resdir property
          return agGameDir;
        }
        else
        {
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
          if (Directory.Exists(WinAGI.CDir(value))) //, vbDirectory) 
                                                    //return error
                                                    //throw new Exception("vbObjectError + 630, strErrSource, Replace(LoadResString(630), ARG1, NewDir)
                                                    //Exit Property
            throw new System.NotImplementedException();

        //change the directory
        agGameDir = WinAGI.CDir(value);

        //update gamefile name
        agGameFile = agGameDir + WinAGI.JustFileName(agGameFile);

        //update resdir
        agResDir = agGameDir + agResDirName + @"\";

        //change date of last edit
        agLastEdit = DateTime.Now;
      }
    }

    public static string GameFile
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
        try
        {
          File.Move(agGameFile, value);
        }
        finally
        {
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

    public static int PlatformType
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
    public static string Platform
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

    public static string PlatformOpts
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

    public static bool UseLE
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

    public static string GameVersion
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

    public static TDefine[] GlobalDefines
    {
      get
      {
        DateTime dtFileMod = File.GetLastWriteTime(agGameDir + "globals.txt");
        if (CRC32(dtFileMod.ToString().ToCharArray()) != agGlobalCRC)
          GetGlobalDefines();

        return agGlobal;
      }
    }

    public static void CloseGame()
    {

      //if no game is currently loaded
      if (!agGameLoaded)
        return;
      //unload and remove all resources
      agLogs.Clear();
      agPics.Clear();
      agSnds.Clear();
      agViews.Clear();

      if (agInvObjList.Loaded)
        agInvObjList.Unload();

      agInvObjList.InGame = false;
      if (agVocabWords.Loaded)
        agVocabWords.Unload();

      agVocabWords.InGame = false;


      //restore default AGI colors
      RestoreDefaultColors();

      //write date of last edit
      WriteGameSetting("General", "LastEdit", agLastEdit.ToString());

      //now save it
      SaveSettingList(agGameProps);
      agGameProps = new List<string> { "" };

      //then clear properties
      agLastEdit = Convert.ToDateTime(0);

      //clear out other properties
      agIntVersion = "";
      agIsVersion3 = false;
      agDescription = "";
      agGameID = "";
      agAuthor = "";
      agGameVersion = "";
      agAbout = "";
      agResDirName = "";
      agPlatformType = 0;
      agPlatformFile = "";
      agPlatformOpts = "";
      agDOSExec = "";

      agGameFile = "";

      //clear flag
      agGameLoaded = false;

      //reset global CRC
      agGlobalCRC = 0;

      //finally, reset directories
      agGameDir = "";
      agResDir = "";

      //throw new Exception("vbObjectError + 644, strErrSrc, Replace(LoadResString(644), ARG1, CStr(lngError) & ":" & strError)
    }
    internal static void CompleteCancel(bool NoEvent = false)
    {
      //cleans up after a compile game cancel or error

      if (!NoEvent)
      {
        Raise_CompileGameEvent(ECStatus.csCanceled, 0, 0, "");
      }
      agCompGame = false;
      fsDIR.Dispose();
      fsVOL.Dispose();
      bwVOL.Dispose();
      bwDIR.Dispose();
    }

    public static void CompileGame(bool RebuildOnly, string NewGameDir = "")
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
      if (!Directory.Exists(NewGameDir))
      {
        //this isn't a directory
        CompleteCancel(true);
        throw new Exception("Replace(LoadResString(561), ARG1, NewGameDir)");
      }

      //set flag if game is being compiled in its current directory
      blnReplace = (NewGameDir.ToLower() == agGameDir.ToLower());

      //save compile dir so rebuild method can access it
      agCompileDir = NewGameDir;

      //set new game version
      if (agChangeVersion)
      {
        NewIsV3 = !agIsVersion3;
      }
      else
      {
        NewIsV3 = agIsVersion3;
      }

      //ensure switch flag is reset
      agChangeVersion = false;
      string strID;
      //if version 3
      if (NewIsV3)
      {
        //version 3 ids limited to 5 characters
        if (agGameID.Length > 5)
        {
          //invalid ID; calling function should know better!
          CompleteCancel(true);
          throw new Exception("LoadResString(694)");
        }
        strID = agGameID;
      }
      else
      {
        strID = "";
      }

      //if not rebuildonly
      if (!RebuildOnly)
      {
        // save/copy words.tok and object files first

        //set status
        Raise_CompileGameEvent(ECStatus.csCompWords, 0, 0, "");
        //check for cancellation
        if (!agCompGame)
        {
          CompleteCancel();
          return;
        }

        //compile WORDS.TOK if dirty
        if (agVocabWords.IsDirty)
        {
          try
          {
            agVocabWords.Save();
          }
          catch (Exception ex)
          {
            //note it
            Raise_CompileGameEvent(ECStatus.csResError, 0, 0, "Error during compilation of WORDS.TOK (" + ex.ToString() + ")");
            //check for cancellation
            if (!agCompGame)
            {
              CompleteCancel();
              return;
            }
          }
        }

        //if compiling to a different directory
        if (!blnReplace)
        {
          try
          {
            // rename then delete existing file, if it exists
            if (File.Exists(NewGameDir + "WORDS.TOK"))
            {
              // delete the 'old' file if it exists
              if (File.Exists(NewGameDir + "WORDS.OLD"))
              {
                File.Delete(NewGameDir + "WORDS.OLD");
              }
              File.Move(NewGameDir + "WORDS.TOK", NewGameDir + "WORDS.OLD");
            }
            // then copy the current file to new location
            File.Copy(agVocabWords.ResFile, NewGameDir + "WORDS.TOK");
          }
          catch (Exception)
          {
            //note error
            Raise_CompileGameEvent(ECStatus.csResError, 0, 0, "Error while creating WORDS.TOK file (Err.Description)");
            //check for cancellation
            if (!agCompGame)
            {
              CompleteCancel();
              return;
            }
          }
        }
        // OBJECT file is next
        //set status
        Raise_CompileGameEvent(ECStatus.csCompObjects, 0, 0, "");
        //check for cancellation
        if (!agCompGame)
        {
          CompleteCancel();
          return;
        }

        //compile OBJECT file if dirty
        if (agInvObjList.IsDirty)
        {
          try
          {
            agInvObjList.Save();
          }
          catch (Exception ex)
          {
            //note it
            Raise_CompileGameEvent(ECStatus.csResError, 0, 0, "Error during compilation of OBJECT (" + ex.ToString() + ")");
            //check for cancellation
            if (!agCompGame)
            {
              CompleteCancel();
              return;
            }
          }
        }
        //if compiling to a different directory
        if (!blnReplace)
        {
          try
          {
            // rename then delete existing file, if it exists
            if (File.Exists(NewGameDir + "OBJECT"))
            {
              //first, delete the 'old' file if it exists
              if (File.Exists(NewGameDir + "OBJECT.OLD"))
              {
                File.Delete(NewGameDir + "OBJECT.OLD");
              }
              File.Move(NewGameDir + "OBJECT", NewGameDir + "OBJECT.OLD");
            }
            // then copy the current file to new location
            File.Copy(agInvObjList.ResFile, NewGameDir + "OBJECT");
          }
          catch (Exception ex)
          {
            //note error
            Raise_CompileGameEvent(ECStatus.csResError, 0, 0, "Error while creating OBJECT file (" + ex.ToString() + ")");
            //check for cancellation
            if (!agCompGame)
            {
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
      for (i = 0; i < 4; i++)
      {
        for (j = 0; i < 768; i++)
        {
          bytDIR[i, j] = 255;
        }
      }

      try
      {
        //ensure all temp vol files are removed
        for (int v = 0; v < 15; v++)
        {
          if (File.Exists(NewGameDir + "NEW_VOL." + v.ToString()))
          {
            File.Delete(NewGameDir + "NEW_VOL." + v.ToString());
          }
        }
      }
      catch (Exception)
      {
        //ignore errors
      }

      try
      {
        //open first new vol file
        fsVOL = File.Create(NewGameDir + "NEW_VOL.0");
        bwVOL = new BinaryWriter(fsVOL);
      }
      catch (Exception)
      {
        CompleteCancel(true);
        throw new Exception("Replace(LoadResString(503), ARG1, Exception");
      }

      //add all logic resources
      try
      {
        CompileResCol(agLogs, AGIResType.rtLogic, RebuildOnly, NewIsV3);
      }
      catch (Exception ex)
      {
        CompleteCancel(true);
        throw new Exception("err: " + ex.ToString());
      }
      //check for cancellation
      //if a resource error (or user canceled) encountered, just exit
      if (!agCompGame)
      {
        return;
      }

      //add picture resources
      try
      {
        CompileResCol(agPics, AGIResType.rtPicture, RebuildOnly, NewIsV3);
      }
      catch (Exception ex)
      {
        CompleteCancel(true);
        throw new Exception("err: " + ex.ToString());
      }
      //check for cancellation
      //if a resource error (or user canceled) encountered, just exit
      if (!agCompGame)
      {
        return;
      }

      //add view resources
      try
      {
        CompileResCol(agViews, AGIResType.rtView, RebuildOnly, NewIsV3);
      }
      catch (Exception ex)
      {
        CompleteCancel(true);
        throw new Exception("err " + ex.ToString());
      }
      //check for cancellation
      //if a resource error (or user canceled) encountered, just exit
      if (!agCompGame)
      {
        return;
      }

      //add sound resources
      try
      {
        CompileResCol(agSnds, AGIResType.rtSound, RebuildOnly, NewIsV3);
      }
      catch (Exception ex)
      {
        CompleteCancel(true);
        throw new Exception("err: " + ex.ToString());
      }
      //check for cancellation
      //if a resource error (or user canceled) encountered, just exit
      if (!agCompGame)
      {
        return;
      }

      //remove any existing old dirfiles
      if (NewIsV3)
      {
        if (File.Exists(NewGameDir + agGameID + "DIR.OLD"))
        {
          File.Delete(NewGameDir + agGameID + "DIR.OLD");
        }
      }
      else
      {
        if (File.Exists(NewGameDir + "LOGDIR.OLD"))
        {
          File.Delete(NewGameDir + "LOGDIR.OLD");
        }
        if (File.Exists(NewGameDir + "PICDIR.OLD"))
        {
          File.Delete(NewGameDir + "PICDIR.OLD");
        }
        if (File.Exists(NewGameDir + "VIEWDIR.OLD"))
        {
          File.Delete(NewGameDir + "VIEWDIR.OLD");
        }
        if (File.Exists(NewGameDir + "SNDDIR.OLD"))
        {
          File.Delete(NewGameDir + "SNDDIR.OLD");
        }
      }

      //rename existing dir files as .OLD
      if (NewIsV3)
      {
        if (File.Exists(NewGameDir + agGameID + "DIR"))
        {
          File.Move(NewGameDir + agGameID + "DIR", NewGameDir + agGameID + "DIR.OLD");
          File.Delete(NewGameDir + agGameID + "DIR");
        }
      }
      else
      {
        if (File.Exists(NewGameDir + "LOGDIR"))
        {
          File.Move(NewGameDir + "LOGDIR", NewGameDir + "LOGDIR.OLD");
          File.Delete(NewGameDir + "LOGDIR");
        }
        if (File.Exists(NewGameDir + "PICDIR"))
        {
          File.Move(NewGameDir + "PICDIR", NewGameDir + "PICDIR.OLD");
          File.Delete(NewGameDir + "PICDIR");
        }
        if (File.Exists(NewGameDir + "VIEWDIR"))
        {
          File.Move(NewGameDir + "VIEWDIR", NewGameDir + "VIEWDIR.OLD");
          File.Delete(NewGameDir + "VIEWDIR");
        }
        if (File.Exists(NewGameDir + "SNDDIR"))
        {
          File.Move(NewGameDir + "SNDDIR", NewGameDir + "SNDDIR.OLD");
          File.Delete(NewGameDir + "SNDDIR");
        }
      }

      //now build the new DIR files
      if (NewIsV3)
      {
        //one dir file
        strFileName = NewGameDir + agGameID + "DIR";
        fsDIR = File.Create(strFileName);
        bwDIR = new BinaryWriter(fsDIR);
        //add offsets - logdir offset is always 8
        bwDIR.Write(Convert.ToInt16(8));
        //pic offset is 8 + 3*logmax
        tmpMax = agLogs.Max + 1;
        if (tmpMax == 0) tmpMax = 1; // always put at least one; even if it//s all FFs
        bwDIR.Write((short)(8 + 3 * tmpMax));
        i = 8 + 3 * tmpMax;
        //view offset is pic offset +3*picmax
        tmpMax = agPics.Max + 1;
        if (tmpMax == 0) tmpMax = 1;
        bwDIR.Write((short)(i + 3 * tmpMax));
        i += 3 * tmpMax;
        //sound is view offset + 3*viewmax
        tmpMax = agViews.Max + 1;
        if (tmpMax == 0) tmpMax = 1;
        bwDIR.Write((short)(i + 3 * tmpMax));

        //now add all the dir entries
        //NOTE: we can't use a for-next loop
        //because sound and view dirs are swapped in v3 directory

        //logics first
        tmpMax = agLogs.Max;
        for (i = 0; i <= tmpMax; i++)
        {
          bwDIR.Write(bytDIR[0, 3 * i]);
          bwDIR.Write(bytDIR[0, 3 * i + 1]);
          bwDIR.Write(bytDIR[0, 3 * i + 2]);
        }
        //next are pictures
        tmpMax = agPics.Max;
        for (i = 0; i <= tmpMax; i++)
        {
          bwDIR.Write(bytDIR[1, 3 * i]);
          bwDIR.Write(bytDIR[1, 3 * i + 1]);
          bwDIR.Write(bytDIR[1, 3 * i + 2]);
        }
        //then views
        tmpMax = agViews.Max;
        for (i = 0; i <= tmpMax; i++)
        {
          bwDIR.Write(bytDIR[3, 3 * i]);
          bwDIR.Write(bytDIR[3, 3 * i + 1]);
          bwDIR.Write(bytDIR[3, 3 * i + 2]);
        }
        //and finally, sounds
        tmpMax = agSnds.Max;
        for (i = 0; i <= tmpMax; i++)
        {
          bwDIR.Write(bytDIR[2, 3 * i]);
          bwDIR.Write(bytDIR[2, 3 * i + 1]);
          bwDIR.Write(bytDIR[2, 3 * i + 2]);
        }
        //done! close the stream and file
        fsDIR.Dispose();
        bwDIR.Dispose();
      }
      else
      {
        //make separate dir files
        for (j = 0; j < 4; j++)
        {
          switch ((AGIResType)j)
          {
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
          for (i = 0; i <= tmpMax; i++)
          {
            bwDIR.Write(bytDIR[j, 3 * i]);
            bwDIR.Write(bytDIR[j, 3 * i + 1]);
            bwDIR.Write(bytDIR[j, 3 * i + 2]);
          }
          fsDIR.Dispose();
          bwDIR.Dispose();
        }
      }

      //remove any existing old vol files
      for (i = 0; i < 16; i++)
      {
        if (NewIsV3)
        {
          if (File.Exists(NewGameDir + agGameID + "VOL." + i.ToString() + ".OLD"))
          {
            File.Delete(NewGameDir + agGameID + "VOL." + i.ToString() + ".OLD");
          }
        }
        else
        {
          if (File.Exists(NewGameDir + "VOL." + i.ToString() + ".OLD"))
          {
            File.Delete(NewGameDir + "VOL." + i.ToString() + ".OLD");
          }
        }
      }

      //rename current vol files
      for (i = 0; i < 16; i++)
      {
        if (NewIsV3)
        {
          if (File.Exists(NewGameDir + agGameID + "VOL." + i.ToString()))
          {
            File.Move(NewGameDir + agGameID + "VOL." + i.ToString(), NewGameDir + agGameID + "VOL." + i.ToString() + ".OLD");
          }
        }
        else
        {
          if (File.Exists(NewGameDir + "VOL." + i.ToString()))
          {
            File.Move(NewGameDir + "VOL." + i.ToString(), NewGameDir + "VOL." + i.ToString() + ".OLD");
          }
        }
      }

      // now rename VOL files
      for (i = 0; i <= lngCurrentVol; i++)
      {
        strFileName = strID + "VOL." + i.ToString();
        File.Move(NewGameDir + "NEW_VOL." + i.ToString(), NewGameDir + strFileName);
      }


      //update status to indicate complete
      Raise_CompileGameEvent(ECStatus.csCompileComplete, 0, 0, "");

      //if replacing (meaning we are compiling to the current game directory)
      if (blnReplace)
      {
        //then we need to update the vol/loc info for all ingame resources;
        //this is done here instead of when the logics are compiled because
        //if there's an error, or the user cancels, we don't want the resources
        //to point to the wrong place
        foreach (AGILogic tmpLogic in agLogs.Col)
        {
          tmpLogic.Volume = bytDIR[0, tmpLogic.Number * 3] / 0x10;
          tmpLogic.Loc = (bytDIR[0, tmpLogic.Number * 3] & 0xF) * 0x10000 + bytDIR[0, tmpLogic.Number * 3 + 1] * 0x100 + bytDIR[0, tmpLogic.Number * 3 + 2];
        }
        foreach (AGIPicture tmpPicture in agPics.Col)
        {
          tmpPicture.Volume = bytDIR[1, tmpPicture.Number * 3] / 0x10;
          tmpPicture.Loc = (bytDIR[1, tmpPicture.Number * 3] & 0xF) * 0x10000 + bytDIR[1, tmpPicture.Number * 3 + 1] * 0x100 + bytDIR[1, tmpPicture.Number * 3 + 2];
        }
        foreach (AGISound tmpSound in agSnds.Col)
        {
          tmpSound.Volume = bytDIR[2, tmpSound.Number * 3] / 0x10;
          tmpSound.Loc = (bytDIR[2, tmpSound.Number * 3] & 0xF) * 0x10000 + bytDIR[2, tmpSound.Number * 3 + 1] * 0x100 + bytDIR[2, tmpSound.Number * 3 + 2];
        }
        foreach (AGIView tmpView in agViews.Col)
        {
          tmpView.Volume = bytDIR[3, tmpView.Number * 3] / 0x10;
          tmpView.Loc = (bytDIR[3, tmpView.Number * 3] & 0xF) * 0x10000 + bytDIR[3, tmpView.Number * 3 + 1] * 0x100 + bytDIR[3, tmpView.Number * 3 + 2];
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
      //  strError = "Unable to delete existing " & strFileName & ". New " & strFileName & " file was not created."
      //case  else 
      //  //some other error - who knows what's going on
      //  strError = Err.Description
      //End Select


      //CompleteCancel(true);
      //throw new Exception("lngError, strErrSource, strError
    }

    public static string GameID
    {
      get
      {
        //id is undefined if a game is not loaded
        if (!agGameLoaded)
        {
          throw new Exception("LoadResString(677)");
        }
        return agGameID;
      }
      set
      {
        //limit gameID to 6 characters for v2 games and 5 characters for v3 games

        string NewID = value;
        string[] strExtension = new string[0];

        //id is undefined if a game is not loaded
        if (!agGameLoaded)
        {
          throw new Exception("LoadResString(677)");
        }
        //version 3 games limited to 5 characters
        if (agIsVersion3)
        {
          if (value.Length > 5)
          {
            NewID = Left(NewID, 5);
          }
        }
        else
        {
          if (NewID.Length > 6)
          {
            NewID = Left(NewID, 6);
          }
        }

        //if no change
        if (agGameID == NewID)
        {
          return;
        }

        //if version 3
        if (agIsVersion3)
        {
          //need to rename the dir file
          File.Move(agGameDir + agGameID + "DIR", agGameDir + NewID + "DIR");
          //delete old dirfile
          File.Delete(agGameDir + agGameID + "DIR");

          //and vol files
          foreach (string strVolFile in Directory.EnumerateFiles(agGameDir, agGameID + "VOL.*"))
          {
            //if an archived (OLD) file, skip it
            if (Right(strVolFile, 4).ToUpper() != ".OLD")
            {
              //get extension
              strExtension = strVolFile.Split(".");
              //rename
              File.Move(agGameDir + strVolFile, agGameDir + NewID + "VOL." + strExtension[1]);
              //TODO: delete the old one
              File.Delete(agGameDir + strVolFile);
            }
          }
        }
        //if property file is currently linked to game ID
        if (agGameFile.ToUpper() == (agGameDir + agGameID + ".wag").ToUpper())
        {
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

        //  throw new Exception("vbObjectError + 530, strErrSrc, Replace(LoadResString(530), ARG1, CStr(lngError) & ":" & strError)
      }
    }

    public static DateTime LastEdit
    {
      get
      {
        //if game loaded,
        if (agGameLoaded)
        {
          return agLastEdit;
        }
        else
        {
          return DateTime.Now;
        }
      }
    }
    public static AGILogicSourceSettings LogicSourceSettings
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

    public static int MaxVol0Size
    {
      get { return agMaxVol0; }
      set
      {
        //validate
        if (value < 32768)
        {
          agMaxVol0 = 32768;
        }
        else if (value != MAX_VOLSIZE)
        {
          agMaxVol0 = MAX_VOLSIZE;
        }
        else
        {
          agMaxVol0 = value;
        }
      }
    }

    public static string InterpreterVersion
    {
      get
      {
        // if a game is loaded
        if (agGameLoaded)
        {
          return agIntVersion;
        }
        // if no version yet
        if (agIntVersion.Length == 0)
        {
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
        if (IntVersions.Contains(value))
        {
          //if in a game, need to adjust resources if necessary
          if (agGameLoaded)
          {
            //if new version and old version are not same major version:
            if (agIntVersion[0] != value[0])
            {
              //set flag to switch version on rebuild
              agChangeVersion = true;

              //if new version is V3 (i.e., current version isn't)
              if (!agIsVersion3)
              {
                if (GameID.Length > 5)
                {
                  //truncate the ID
                  GameID = Left(GameID, 5);
                }
              }
              //use compiler to rebuild new vol and dir files
              //(it is up to calling program to deal with dirty and invalid resources)
              try
              {
                CompileGame(true);
              }
              catch (Exception ex)
              {

                //reset cancel flag
                agCancelComp = false;

                // pass along the exception
                throw new Exception(ex.Message);
              }

              //check for user cancellation
              if (agCancelComp)
              {
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
        else
        {
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

    public static string ResDir
    {
      get
      {
        // if a game is loaded
        if (agGameLoaded)
        {
          return agResDir;
        }
        //otherwise use current directory
        return CDir(System.IO.Directory.GetCurrentDirectory());
      }
      set
      {

      }
    }

    public static string ResDirName
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

        if ("\\/:*?<>|".Any(tmpName.Contains))
        {
          throw new Exception("380, Invalid property Value");
        }

        //save new resdir name
        agResDirName = tmpName;
        //update the actual resdir
        agResDir = agGameDir + agResDirName + "\\";

        //save resdirname
        WriteGameSetting("General", "ResDir", agResDirName);

        //change date of last edit
        agLastEdit = DateTime.Now;
      }
    }

    public static void SaveProperties()
    {

      //this forces WinAGI to save the property file, rather than waiting until
      //the game is unloaded

      if (agGameLoaded)
        SaveSettingList(agGameProps);
    }

    public static string TemplateDir
    {
      get { return agTemplateDir; }
      set
      {
        // directory has to exist
        if (!Directory.Exists(value))
          throw new Exception("Replace(LoadResString(630), ARG1, NewDir)");

        agTemplateDir = CDir(value);
      }
    }

    public static void OpenGameDIR(string NewGameDir)
    {
      //creates a new WinAGI game file from Sierra game directory

      //if a game is already open,
      if (agGameLoaded)
      {
        //can't open a game if one is already open
        throw new Exception("LoadResString(501)");
      }

      //set game directory
      agGameDir = CDir(NewGameDir);

      //attempt to extract agGameID, agGameFile and interpreter version
      //from Sierra files (i.e. from DIR files and VOL files)

      //set status to decompiling game dir files
      Raise_LoadGameEvent(ELStatus.lsDecompiling, AGIResType.rtNone, 0, "");

      //check for valid DIR/VOL files
      //(which gets gameid, and sets version 3 status flag)
      if (!IsValidGameDir(agGameDir))
      {
        //save dir as error string
        strError = agGameDir;
        //clear game variables
        agGameDir = "";
        agGameFile = "";
        agGameID = "";
        agIntVersion = "";
        agIsVersion3 = false;
        //invalid game directory (
        throw new Exception("LoadResString(541), ARG1, strError");
      }

      //create a new wag file name
      agGameFile = agGameDir + agGameID + ".wag";

      //rename any existing game file (in case user is re-importing)
      if (File.Exists(agGameFile))
      {
        if (File.Exists(agGameFile + ".OLD"))
        {
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
      if (agIntVersion.Length == 0)
      {
        //clear game variables
        agGameDir = "";
        agGameFile = "";
        agGameID = "";
        agIsVersion3 = false;
        agGameProps = new List<string> { };
        //invalid number found
        throw new Exception("LoadResString(543)");
      }

      //save version
      WriteGameSetting("General", "Interpreter", agIntVersion);

      try
      {
        //finish the game load
        FinishGameLoad(1);
      }
      catch (Exception ex)
      {

        throw new Exception(ex.Message);
      }
    }

    public static void OpenGameWAG(string GameWAG)
    {
      //Public Sub OpenGameWAG(GameWAG As String)
      //opens a WinAGI game file (must be passed as a full length file name)

      string strVer = "";

      //if a game is already open,
      if (agGameLoaded)
      { //can't open a game if one is already open
        throw new Exception("LoadResString(501)");
      }

      //Debug.Assert agGameID = ""

      //verify the wag file exists
      if (!File.Exists(GameWAG))
      {  //clear game variables
        agGameDir = "";
        agGameFile = "";
        agGameID = "";
        agIntVersion = "";
        agIsVersion3 = false;
        //agGameProps = null;

        //error - is it the file, or the directory?
        if (Directory.Exists(JustPath(GameWAG)))
        {
          //it's a missing file - return wagfile as error string
          strError = JustFileName(GameWAG);
          //invalid wag
          throw new Exception("vbObjectError + 655");
        }
        else
        {
          //it's an invalid or missing directory - return directory as error string
          //strError = JustPath(GameWAG, true)
          //invalid wag
          throw new Exception("vbObjectError + 541");
        }
      }

      //set game file property
      agGameFile = GameWAG;

      //set game directory
      agGameDir = JustPath(GameWAG);

      //open the property file (file has to already exist)
      agGameProps = OpenSettingList(agGameFile, false);

      //check to see if it's valid
      strVer = ReadSettingString(agGameProps, "General", "WinAGIVersion", "");
      if (strVer.Length == 0)
      {
        //not a valid file; maybe it's an old binary version
        //TODO: make this a function that returns true if successful
        ConvertWag();
      }
      else
      {

        switch (strVer)
        {
          case WINAGI_VERSION:
            //this is the current version
            break;
          case "1.2.2":
          case "1.2.3":
          case "1.2.4":
          case "1.2.5":
          case "1.2.6":
          case "1.2.7":
          case "1.2.8": // need to convert it
            WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION);
            break;

          default:
            //any v1.2.x is ok
            if (Left(strVer, 4) == "1.2.")
            {
              //ok, but update
              WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION);
            }
            else
            {
              //if another winagi version is ever released,
              //yeah, right; just consider this file invalid

              //save wagfile as error string
              strError = JustFileName(agGameFile);
              //clear game variables
              agGameDir = "";
              agGameFile = "";
              agGameID = "";
              agIntVersion = "";
              agIsVersion3 = false;
              // agGameProps = null;
              //invalid wag
              throw new Exception("vbObjectError + 665");
            }
            break;
        }
      }

      //get gameID
      agGameID = ReadSettingString(agGameProps, "General", "GameID", "");

      //if an id is found, keep going
      if (agGameID.Length > 0)
      {
        //got ID; now get interpreter version from propfile
        agIntVersion = ReadSettingString(agGameProps, "General", "Interpreter", "");

        //validate it
        if (IntVersions.Contains(agIntVersion))
        {
          if (!IntVersions.Contains(agIntVersion))
          {
            //save wagfile as error string
            strError = JustFileName(agGameFile);
            //clear propfile
            // agGameProps = null;
            //clear game variables
            agGameDir = "";
            agGameFile = "";
            agGameID = "";
            agIntVersion = "";
            agIsVersion3 = false;
            //invalid int version inside wag file
            throw new Exception("vbObjectError + 691");
          }
        }
        else
        {
          //missing GameID in wag file - make user address it
          //by raising an error

          //save wagfile name as error string
          strError = JustFileName(agGameFile);
          //clear propfile
          // agGameProps = null;
          //clear game variables
          agGameDir = "";
          agGameFile = "";
          agGameID = "";
          agIntVersion = "";
          agIsVersion3 = false;
          //invalid wag file
          throw new Exception("vbObjectError + 690");
        }

        //if a valid wag file was found, we now have agGameID, agGameFile
        //and correct interpreter version;

        //must have a valid gameID, gamefile and intversion
        //Debug.Assert LenB(agGameID) > 0
        //Debug.Assert LenB(agIntVersion) > 0
        //Debug.Assert LenB(agGameFile) > 0
        //Debug.Assert Not(agGameProps Is Nothing)

        //normally, gameID will be the same as the game property
        //file name, but it doesn't have to be.So no check is
        //made to see if they match

        try
        {
          //finish the game load
          FinishGameLoad(0);
        }
        catch (Exception e)
        {

          throw new Exception(e.Message);
        }
      }
    }

      public static void FinishGameLoad(int Mode = 0)
      {
        /*                    //finishes game load
                            //mode determines whether opening by wag file (0) or
                            //extracting from Sierra game files (1)
                            //(currently, no difference between them)

                            Dim blnWarnings As Boolean, lngError As Long
                            Dim strTempDir As String, lngDirCount As Long
                            Dim strTempFile As String


                            On Error Resume Next

                            //set v3 flag
                            agIsVersion3 = (Val(agIntVersion) >= 3)

                            //if loading from a wag file
                            if (Mode = 0)
                              //informs user we are checking property file to set game parameters
                              Raise_LoadGameEvent lsPropertyFile, rtNone, 0, ""

                              //get resdir before loading resources
                              agResDirName = ReadSettingString(agGameProps, "General", "ResDir", "")
                            } else {
                              //no resdir yet for imported game
                              agResDirName = ""
                            }

                            //if none, use default
                            if (LenB(agResDirName) = 0)
                              //look for an existing directory
                              strTempDir = Dir(agGameDir, vbDirectory)
                              Do Until Len(strTempDir) = 0
                                if (strTempDir<> "." And strTempDir != "..")
                                  if ((GetAttr(agGameDir & strTempDir) And vbDirectory) = vbDirectory)
                                    lngDirCount = lngDirCount + 1
                                    if (lngDirCount > 1)
                                      Exit Do
                                    }
                                    strTempFile = strTempDir
                                  }
                                }
                                strTempDir = Dir(, vbDirectory)
                              Loop

                              //is there only one subfolder?
                              if (lngDirCount = 1)
                                //assume it is resource directory
                                  agResDirName = strTempFile
                              } else {
                                //either no subfolders, or more than one
                                //so use default
                                agResDirName = agDefResDir
                              }

                              WriteGameSetting("General", "ResDir", agResDirName
                            }

                            //now create full resdir from name
                            agResDir = agGameDir & agResDirName & "\"

                            //ensure resource directory exists
                            if (!FileExists(agGameDir & agResDirName, vbDirectory))
                              MkDir agGameDir & agResDirName
                              //if can't create the resources directory
                              if (Err.Number<> 0)
                                //note the problem in the error log as a warning
                                RecordLogEvent leWarning, "Can't create " & agResDir
                                //use game directory
                                agResDir = agGameDir
                                //set warning
                                blnWarnings = true
                              }
                            }

                          //  //if loading from a WAG file
                          //
                          //  if (Mode = 0)
                          //    //only resources in the WAG file need to be checked on initial load
                          //    blnWarnings = LoadResources()
                          //  } else {
                          //    //if opening bi directory, extract resources from the VOL
                          //    //files load resources
                                 blnWarnings = ExtractResources()
                          //  }

                            //if there was an error
                            if (Err.Number<> 0)
                              //can't continue
                              //save error information
                              strError = Err.Description
                              lngError = Err.Number

                              //clear game variables
                              agGameDir = ""
                              agGameFile = ""
                              agGameID = ""
                              agIntVersion = ""
                              agIsVersion3 = false

                              //reset collections & objects
                              Set agLogs = Nothing
                              Set agViews = Nothing
                              Set agPics = Nothing
                              Set agSnds = Nothing
                              Set agInvObjList = Nothing
                              Set agVocabWords = Nothing
                              Set agLogs = New AGILogics
                              Set agViews = New AGIViews
                              Set agPics = New AGIPictures
                              Set agSnds = New AGISounds
                              Set agInvObjList = New AGIInventoryObjects
                              Set agVocabWords = New AGIWordList

                              Set agGameProps = Nothing

                              //raise error
                              throw new Exception("lngError, strErrSource, strError
                            }

                            //load vocabulary word list
                            Raise_LoadGameEvent lsResources, rtWords, 0, ""

                            Set agVocabWords = New AGIWordList
                            agVocabWords.Init
                            agVocabWords.Load agGameDir & "WORDS.TOK"
                            //if there was an error,
                            if (Err.Number<> 0)
                              //note the problem in the error log as a warning
                              RecordLogEvent leError, "An error occurred while loading WORDS.TOK: " & Err.Description
                              //reset warning flag
                              blnWarnings = true
                            }

                            //load inventory objects list
                            Raise_LoadGameEvent lsResources, rtObjects, 0, ""

                            Set agInvObjList = New AGIInventoryObjects
                            agInvObjList.Init
                            agInvObjList.Load agGameDir & "OBJECT"
                            //if there was an error,
                            if (Err.Number<> 0)
                              //note the problem in the error log as a warning
                              RecordLogEvent leError, "An error occurred while loading OBJECT: " & Err.Description
                              //reset warning flag
                              blnWarnings = true
                            }


                            Raise_LoadGameEvent lsFinalizing, rtNone, 0, ""

                            //assign commands
                            AssignCommands

                            //and adust commands based on AGI version
                            CorrectCommands agIntVersion

                            //clear other game properties
                            agLastEdit = 0
                            agAuthor = ""
                            agDescription = ""
                            agGameVersion = ""
                            agAbout = ""
                            agPlatformType = 0
                            agPlatformFile = ""
                            agPlatformOpts = ""
                            agDOSExec = ""

                            //get rest of game properties
                            //Debug.Assert Not (agGameProps Is Nothing)
                            GetGameProperties

                            //if errors
                            if (Err.Number<> 0)
                              //record event
                              RecordLogEvent leWarning, "Error while loading WAG file; some properties not loaded. (Error number: " & CStr(Err.Number) & ")"
                              Err.Clear
                              blnWarnings = true
                            }

                            //force id reset
                            blnSetIDs = false

                            //we've established that the game can be opened
                            //so set the loaded flag now
                            agGameLoaded = true

                            //write create date
                            WriteGameSetting("General", "LastEdit", agLastEdit

                            //and save the wag file
                            SaveSettingList agGameProps

                            //if errors
                            if (blnWarnings)
                              throw new Exception("vbObjectError + 636, strErrSource, LoadResString(636)
                            }
                          End Sub
       /*


                          Public Sub NewGame(NewID As String, ByVal NewVersion As String, NewGameDir As String, NewResDir As String, Optional ByVal TemplateDir As String = "")

                            //creates a new game in NewGameDir
                            //if a template directory is passed,
                            //use the resources from that template directory

                            Dim strDirData As String, intFile As Integer, strGameWAG As String
                            Dim i As Long, strTempDir As String, lngDirCount As Long
                            Dim blnWarnings As Boolean, strTmplResDir As String
                            Dim stlGlobals As StringList


                            On Error Resume Next

                            //if a game is already open,
                            if (agGameLoaded)
                              //can't open a game if one is already open
                              throw new Exception("vbObjectError + 501, strErrSource, LoadResString(501)
                              return;
                            }

                            //if not a valid directory
                            if (!FileExists(NewGameDir, vbDirectory))
                              //raise error
                              throw new Exception("vbObjectError + 630, strErrSource, Replace(LoadResString(630), ARG1, NewGameDir)
                              return;
                            }

                            //if a game already exists
                            if (LenB(Dir(CDir(NewGameDir) &"*.wag")) != 0)
                             //game file exists;
                             throw new Exception("vbObjectError + 687, strErrSource, LoadResString(687)
                              return;


                            } else {if (IsValidGameDir(CDir(NewGameDir)))
                              //game files exist;
                              agGameDir = ""
                              agIsVersion3 = false
                              throw new Exception("vbObjectError + 687, strErrSource, LoadResString(687)
                              return;
                            }

                            //set game directory
                            agGameDir = CDir(NewGameDir)

                            //ensure resdir is valid
                            if (LenB(NewResDir) = 0)
                              //if blank use default
                              NewResDir = agDefResDir
                            }

                            //if using template
                            if (LenB(TemplateDir) != 0)
                              //template should include dir files, vol files, logic
                              //source, words and objects lists;
                              //globals lists and layouts

                              //(first, make sure it's formatted as a directory
                              TemplateDir = CDir(TemplateDir)
                              //we need to know name of the resource subdir so we can
                              //rename it later, if needed

                              // get wag file
                              strGameWAG = Dir(TemplateDir & "*.wag")
                              //if not a valid directory
                              if (Len(strGameWAG) = 0)
                                //raise error
                                throw new Exception("vbObjectError + 630, strErrSource, LoadResString(508)
                                return;
                              }

                              //find first subdir, use that name
                              strTmplResDir = Dir(TemplateDir, vbDirectory)
                              Do Until Len(strTmplResDir) = 0
                                if (strTmplResDir<> "." And strTmplResDir != "..")
                                  if ((GetAttr(TemplateDir & strTmplResDir) And vbDirectory) = vbDirectory)
                                    //this is it
                                    lngDirCount = 1
                                    Exit Do
                                  }
                                }
                                strTmplResDir = Dir()
                              Loop

                              //if no resdir found, reset template resdir name so it gets handled correctly
                              if (lngDirCount<> 1)
                                strTmplResDir = ""
                              }

                              //copy all files from the templatedir into gamedir
                              if (!CopyFolder(TemplateDir, agGameDir, false))
                                throw new Exception("vbObjectError + 683, strErrSource, Replace(LoadResString(683), ARG1, Err.Description)
                                return;
                              }

                              //open game with template id
                              OpenGameWAG agGameDir & strGameWAG

                              if (Err.Number<> 0)
                               throw new Exception("vbObjectError + 684, strErrSource, Replace(LoadResString(684), ARG1, Err.Description)
                                return;
                              }

                              //we need to rename the resdir
                              //(have to do this AFTER load, because loading will keep the current
                              //resdir that's in the WAG file)
                              if (NewResDir<> strTmplResDir)
                                //we need to change it
                                  Name agGameDir & strTmplResDir As agGameDir & NewResDir
                              }
                              //then change the resource directory property
                              agResDirName = NewResDir
                              //update the actual resdir
                              agResDir = agGameDir & agResDirName & "\"

                              //change gameid
                              GameID = NewID
                              if (Err.Number<> 0)
                                throw new Exception("vbObjectError + 685, strErrSource, Replace(LoadResString(685), ARG1, Err.Description)
                                return;
                              }

                              //update global file header
                              Set stlGlobals = OpenSettingList(agGameDir & "globals.txt", false)
                              if (!stlGlobals Is Nothing)
                                if (stlGlobals.Count > 3)
                                  if (Left(Trim(stlGlobals.StringLine(2)), 1) = "[")
                                    stlGlobals.StringLine(2) = "[ global defines file for " & NewID
                                  }
                                  //save it
                                  SaveSettingList stlGlobals
                                }
                                //toss it
                                Set stlGlobals = Nothing
                              }

                            //if not using template,
                            } else {
                              //validate new version
                               if (IntVersions.Contains(value))
                              //if (ValidateVersion(NewVersion))
                                //ok; set version
                                agIntVersion = NewVersion
                                //set version3 flag
                                agIsVersion3 = (Val(NewVersion) > 3)
                              } else {
                                if (Val(NewVersion) < 2 Or Val(NewVersion) > 3)
                                  //not a version 2 or 3 game
                                  throw new Exception("vbObjectError + 597, strErrSource, LoadResString(597)
                                } else {
                                  //unsupported version 2 or 3 game
                                  throw new Exception("vbObjectError + 543, strErrSource, LoadResString(543)
                                }
                                return;
                              }

                              //set game id (limit to 6 characters for v2, and 5 characters for v3
                              //(don't use the GameID property; gameloaded flag is not set yet
                              //so using GameID property will cause error)
                              if (agIsVersion3)
                                agGameID = UCase$(Left$(NewID, 5))
                              } else {
                                agGameID = UCase$(Left$(NewID, 6))
                              }

                              //create empty property file
                              agGameFile = agGameDir & agGameID & ".wag"
                              Kill agGameFile
                              Err.Clear
                              Set agGameProps = OpenSettingList(agGameFile)
                              With agGameProps
                                .Add "#"
                                .Add "# WinAGI Game Property File for " & agGameID
                                .Add "#"
                                .Add "[General]"
                                .Add ""
                                .Add "[Palette]"
                                .Add ""
                                .Add "[WORDS.TOK]"
                                .Add ""
                                .Add "[OBJECT]"
                                .Add ""
                                .Add "[::BEGIN Logics::]"
                                .Add "[::END Logics::]"
                                .Add ""
                                .Add "[::BEGIN Pictures::]"
                                .Add "[::END Pictures::]"
                                .Add ""
                                .Add "[::BEGIN Sounds::]"
                                .Add "[::END Sounds::]"
                                .Add ""
                                .Add "[::BEGIN Views::]"
                                .Add "[::END Views::]"
                                .Add ""
                              End With
                              //add WinAGI version
                              WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION


                              //set the resource directory name so it can be set up
                              ResDirName = NewResDir

                              //errors after this cause failure
                              On Error GoTo ErrHandler

                              //create default resource directories
                              strDirData = String$(768, 0xFF)
                              if (agIsVersion3)
                                intFile = FreeFile()
                                Open agGameDir & agGameID & "DIR" For Binary As intFile
                                Put intFile, 1, Chr$(8) &Chr$(0) & Chr$(8) & Chr$(3) & Chr$(8) & Chr$(6) & Chr$(8) & Chr$(9)
                                Put intFile, , strDirData
                                Put intFile, , strDirData
                                Put intFile, , strDirData
                                Put intFile, , strDirData
                                Close intFile
                              } else {
                                intFile = FreeFile()
                                Open agGameDir & "LOGDIR" For Binary As intFile
                                Put intFile, 1, strDirData
                                Close intFile


                                intFile = FreeFile()
                                Open agGameDir & "PICDIR" For Binary As intFile
                                Put intFile, 1, strDirData
                                Close intFile


                                intFile = FreeFile()
                                Open agGameDir & "SNDDIR" For Binary As intFile
                                Put intFile, 1, strDirData
                                Close intFile


                                intFile = FreeFile()
                                Open agGameDir & "VIEWDIR" For Binary As intFile
                                Put intFile, 1, strDirData
                                Close intFile
                              }

                              //create default vocabulary word list
                              Set agVocabWords = New AGIWordList
                              //use loaded argument to force load of the new wordlist
                              agVocabWords.Init true
                              agVocabWords.AddWord "a", 0
                              agVocabWords.AddWord "anyword", 1
                              agVocabWords.AddWord "rol", 9999
                              agVocabWords.Save

                              //create inventory objects list
                              Set agInvObjList = New AGIInventoryObjects
                              //use loaded argument to force load of new inventory list
                              agInvObjList.Init true
                              agInvObjList.Add "?", 0
                              agInvObjList.Save

                              //assign and adust commands based on AGI version
                              AssignCommands
                              CorrectCommands agIntVersion

                              //add logic zero
                              agLogs.Add 0
                              agLogs(0).Clear
                              agLogs(0).Save
                              agLogs(0).Unload

                              //force id reset
                              blnSetIDs = false

                              //set open flag, so properties can be updated
                              agGameLoaded = true
                            }

                            //set resource directory
                            //Debug.Assert LenB(agResDirName) != 0
                            //Debug.Assert agResDir = agGameDir & agResDirName & "\"


                            On Error Resume Next
                            //ensure resource directory exists
                            if (!FileExists(agGameDir & agResDirName, vbDirectory))
                              MkDir agGameDir & agResDirName
                              //if can't create the resources directory
                              if (Err.Number<> 0)
                                //note the problem in the error log as a warning
                                RecordLogEvent leWarning, "Can't create " & agResDir
                                //use main directory
                                agResDir = agGameDir
                                //set warning flag
                                blnWarnings = true
                              }
                            }

                            //for non-template games, save the source code for logic 0
                            if (LenB(TemplateDir) = 0)
                              agLogs(0).SaveSource
                            }

                            //save gameID, version, directory resource name to the property file;
                            //rest of properties need to be set by the calling function
                            WriteGameSetting("General", "GameID", agGameID
                            WriteGameSetting("General", "Interpreter", agIntVersion
                            WriteGameSetting("General", "ResDir", agResDirName

                            //save palette colors
                            For i = 0 To 15
                              WriteGameSetting("Palette", "Color" & CStr(i), "&H" & PadHex(lngEGACol(i), 8)
                            Next i

                            //if errors
                            if (blnWarnings)
                              throw new Exception("vbObjectError + 637, strErrSource, LoadResString(637)
                            }

                            //Debug.Print "logic0: ", Logics(0).Loaded
                          return;

                          ErrHandler:
                            strError = Err.Description
                            strErrSrc = Err.Source
                            lngError = Err.Number

                            //reset collections & objects
                  Set agLogs = Nothing
                            Set agViews = Nothing
                            Set agPics = Nothing
                            Set agSnds = Nothing
                            Set agInvObjList = Nothing
                            Set agVocabWords = Nothing
                            Set agLogs = New AGILogics
                            Set agViews = New AGIViews
                            Set agPics = New AGIPictures
                            Set agSnds = New AGISounds
                            Set agInvObjList = New AGIInventoryObjects
                            Set agVocabWords = New AGIWordList


                            throw new Exception("vbObjectError + 631, strErrSrc, Replace(LoadResString(631), ARG1, CStr(lngError) & ":" & strError)
                          End Sub

                          Public Property Get VocabularyWords() As AGIWordList


                            Set VocabularyWords = agVocabWords
                          End Property
                          Public Sub WriteProperty(ByVal Section As String, ByVal Key As String, ByVal value As String, Optional Group As String = "", Optional ForceSave As Boolean = false)

                            // this procedure provides calling programs a way to write property
                            // values to the WAG file

                            // no validation of section or newval is done, so calling function
                            // needs to be careful

                            On Error GoTo ErrHandler


                            WriteGameSetting(Section, Key, value, Group

                            // if forcing a save
                            if (ForceSave)
                              SaveProperties
                            }
                          return;

                          ErrHandler:
                            //Debug.Assert false
                            Resume Next
                          End Sub
       /*
                          Private Sub Class_Initialize()
                            Dim rtn As Long

                          End Sub



                          Private Sub Class_Terminate()

                            //release objects
                            Set agLogs = Nothing
                            Set agPics = Nothing
                            Set agSnds = Nothing
                            Set agViews = Nothing


                            Set agInvObjList = Nothing
                            Set agVocabWords = Nothing

                            Set agMainGame = Nothing
                          */
      }
    }
  }
