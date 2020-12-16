using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static WinAGI.WinAGI;

namespace WinAGI
{
  static class AGIGame
  {
    //AGIGame agMainGame;

    // exposed game properties, methods, objects
    public static AGILogicSourceSettings agMainLogSettings = new AGILogicSourceSettings();
    //static public AGICommands agCmdCol = new AGICommands();
    //static public AGITestCommands agTestCmdCol = new AGITestCommands();
    public static AGIGameEvents agGameEvents = new AGIGameEvents();

    internal static List<string> agGameProps = new List<string> { "" };// As StringList


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
    internal static int agMaxVolSize = 0;
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
          //On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
          throw new Exception("Invalid property Value");
        }
        if ("\\/:*?<>|".Any(NewDir.Contains))
          //On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
          throw new Exception("Invalid property Value");

        // ? space should be OK in a directory!
        //if (NewDir.IndexOf(" ") <> 0) {
        //  //On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
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
        //On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
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
        //On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
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
      get => agGameDir;
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
          if (Directory.Exists(WinAGI.cDir(value))) //, vbDirectory) 
                                                    //return error
                                                    //On Error GoTo 0: Err.Raise vbObjectError + 630, strErrSource, Replace(LoadResString(630), ARG1, NewDir)
                                                    //Exit Property
            throw new System.NotImplementedException();

        //change the directory
        agGameDir = WinAGI.cDir(value);

        //update gamefile name
        agGameFile = agGameDir + WinAGI.JustFileName(agGameFile);

        //update resdir
        agResDir = agGameDir + agResDirName + @"\";

        //change date of last edit
        agLastEdit = DateTime.Now;
      }
    }

    public static AGIGameEvents GameEvents => agGameEvents;

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

    public static string InterpreterVersion
    {
      get
      {
        //if a game is loaded
        if (agGameLoaded)
          return agIntVersion;
        else
        {
          //if no version yet
          if (agIntVersion.Length == 0)
            //      //use 2.917
            return "2.917";
          else
            return agIntVersion;
        }
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

      //On Error GoTo 0: Err.Raise vbObjectError + 644, strErrSrc, Replace(LoadResString(644), ARG1, CStr(lngError) & ":" & strError)
    }
    /*
               Public Sub CloseGame()


               End Sub

               Public Sub CompileGame(ByVal RebuildOnly As Boolean, Optional ByVal NewGameDir As String)
                 //compiles the game into NewGameDir
                 //
                 //if RebuildOnly is true, the VOL files are
                 //rebuilt without recompiling all logics
                 //and WORDS.TOK and OBJECT are not recompiled
                 //
                 //WARNING: if NewGameDir is same as current directory
                 //this WILL overwrite current game files
                 //

                 Dim tmpLogic As AGILogic, tmpPicture As AGIPicture
                 Dim tmpView As AGIView, tmpSound As AGISound
                 Dim CurResType As AGIResType, CurResNum As Byte
                 Dim tmpLoaded As Boolean, tmpSourceLoaded As Boolean
                 Dim strID As String, tmpMax As Integer
                 Dim i As Long, j As Long, blnReplace As Boolean
                 Dim intTmpDir As Integer, lngOffset As Long
                 Dim NewIsV3 As Boolean, strFileName As String

                 On Error Resume Next

                 //only loaded games can be compiled
                 If Not agGameLoaded Then
                   Exit Sub
                 }

                 //set compiling flag
                 agCompGame = True
                 //reset cancel flag
                 agCancelComp = False

                 //if no directory passed,
                 If LenB(NewGameDir) = 0 Then
                   NewGameDir = agGameDir
                 }

                 //validate new directory
                 NewGameDir = cDir(NewGameDir)
                 If LenB(Dir(NewGameDir, vbDirectory)) = 0 Then
                   //this isn't a directory
                   CompleteCancel True
                   On Error GoTo 0: Err.Raise vbObjectError + 561, Replace(LoadResString(561), ARG1, NewGameDir)
                   Exit Sub
                 }

                 //set flag if game is being compiled in its current directory
                 blnReplace = (LCase$(NewGameDir) = LCase$(agGameDir))

                 //save compile dir so rebuild method can access it
                 agCompileDir = NewGameDir

                 //set new game version
       If agChangeVersion Then
                   NewIsV3 = Not agIsVersion3
                 Else
                   NewIsV3 = agIsVersion3
                 }

                 //ensure switch flag is reset
                 agChangeVersion = False

                 //if version 3
                 If NewIsV3 Then
                   //version 3 ids limited to 5 characters
                   If Len(agGameID) > 5 Then
                     //invalid ID; calling function should know better!
                     CompleteCancel True
                     On Error GoTo 0: Err.Raise vbObjectError + 694, LoadResString(694)
                     Exit Sub
                   }
                   strID = agGameID
                 Else
                   strID = ""
                 }

                 //if not rebuildonly
                 If Not RebuildOnly Then
                   //set status
                   agGameEvents.RaiseEvent_CompileGameStatus csCompWords, 0, 0, ""
                   //check for cancellation
                   If Not agCompGame Then
                     CompleteCancel
                     Exit Sub
                   }

                   //compile WORDS.TOK if dirty
                   If agVocabWords.IsDirty Then
                     agVocabWords.Save
                   }
                   If Err.Number<> 0 Then
                     //note it
                     agGameEvents.RaiseEvent_CompileGameStatus csResError, 0, 0, "Error during compilation of WORDS.TOK (" & Err.Description & ")"
                     Err.Clear
                     //check for cancellation
                     If Not agCompGame Then
                       CompleteCancel
                       Exit Sub
                     }
                   }

                   //if compiling to a different directory
                   If Not blnReplace Then
                     Kill NewGameDir & "WORDS.OLD"
                     Err.Clear
                     Name NewGameDir & "WORDS.TOK" As NewGameDir & "WORDS.OLD"
                     Err.Clear
                     //copy into new directory
                     FileCopy agVocabWords.ResFile, NewGameDir & "WORDS.TOK"
                     If Err.Number<> 0 Then
                       //note it
                       agGameEvents.RaiseEvent_CompileGameStatus csResError, 0, 0, "Error while creating WORDS.TOK file (" & Err.Description & ")"
                       Err.Clear
                       //check for cancellation
                       If Not agCompGame Then
                         CompleteCancel
                         Exit Sub
                       }
                     }
                   }

                   //clear errors
                   Err.Clear

                   //set status
                   agGameEvents.RaiseEvent_CompileGameStatus csCompObjects, 0, 0, ""
                   //check for cancellation
                   If Not agCompGame Then
                     CompleteCancel
                     Exit Sub
                   }

                   //compile OBJECT file if dirty
                   If agInvObjList.IsDirty Then
                     agInvObjList.Save
                   }
                   If Err.Number<> 0 Then
                     //note it
                     agGameEvents.RaiseEvent_CompileGameStatus csResError, 0, 0, "Error during compilation of OBJECT (" & Err.Description & ")"
                     //check for cancellation
                     If Not agCompGame Then
                       CompleteCancel
                       Exit Sub
                     }
                     Err.Clear
                   }

                   //if compiling to a different directory
                   If Not blnReplace Then
                     Kill NewGameDir & "OBJECT.OLD"
                     Err.Clear
                     Name NewGameDir & "OBJECT" As NewGameDir & "OBJECT.OLD"
                     Err.Clear
                     //copy into new directory
                     FileCopy agInvObjList.ResFile, NewGameDir & "OBJECT"
                     If Err.Number<> 0 Then
                       //note it
                       agGameEvents.RaiseEvent_CompileGameStatus csResError, 0, 0, "Error while creating OBJECT file (" & Err.Description & ")"
                       Err.Clear
                       //check for cancellation
                       If Not agCompGame Then
                         CompleteCancel
                         Exit Sub
                       }
                     }
                   }
                 }

                 //ensure error object is cleared
                 Err.Clear

                 //reset game compiler variables
                 lngCurrentVol = 0
                 lngCurrentLoc = 0
                 strNewDir = NewGameDir

                 //new strategy with dir files; use arrays during
                 //compiling process; then build the dir files at
                 //the end
                 For i = 0 To 3
                   For j = 0 To 767
                     bytDIR(i, j) = 255
                   Next j
                 Next i

                 //ensure temp vol files are removed
                 Kill NewGameDir & "NEW_VOL.*"
                 Err.Clear

                 //open first new vol file
                 intVolFile = FreeFile()
                 Open NewGameDir & "NEW_VOL.0" For Binary As intVolFile
                 If Err.Number<> 0 Then
                   Close intVolFile
                   strError = Err.Number
                   strError = Err.Description
                   strErrSrc = Err.Source
                   lngError = Err.Number
                   CompleteCancel True


                   On Error GoTo 0: Err.Raise vbObjectError + 503, strErrSrc, Replace(LoadResString(503), ARG1, CStr(lngError) & ":" & strError)
                   Exit Sub
                 }

                 //add all logic resources
                 CompileResCol agLogs, rtLogic, RebuildOnly, NewIsV3

                 //pass along any errors
                 If Err.Number<> 0 Then
                   lngError = Err.Number
                   strError = Err.Description
                   CompleteCancel True
                   On Error GoTo 0: Err.Raise lngError, strErrSource, strError
                   Exit Sub
                 }
                 //check for cancellation
                 //if a resource error (or user canceled) encountered, just exit
                 If Not agCompGame Then
                   Exit Sub
                 }

                 //add picture resources
                 CompileResCol agPics, rtPicture, RebuildOnly, NewIsV3
                 //pass along any errors
                 If Err.Number<> 0 Then
                   lngError = Err.Number
                   strError = Err.Description
                   CompleteCancel True
                   On Error GoTo 0: Err.Raise lngError, strErrSource, strError
                   Exit Sub
                 }
                 //check for cancellation
                 //if a resource error (or user canceled) encountered, just exit
                 If Not agCompGame Then
                   Exit Sub
                 }

                 //add view resources
                 CompileResCol agViews, rtView, RebuildOnly, NewIsV3
                 //pass along any errors
                 If Err.Number<> 0 Then
                   lngError = Err.Number
                   strError = Err.Description
                   CompleteCancel True
                   On Error GoTo 0: Err.Raise lngError, strErrSource, strError
                   Exit Sub
                 }
                 //check for cancellation
                 //if a resource error (or user canceled) encountered, just exit
                 If Not agCompGame Then
                   Exit Sub
                 }

                 //add sound resources
                 CompileResCol agSnds, rtSound, RebuildOnly, NewIsV3
                 //pass along any errors
                 If Err.Number<> 0 Then
                   lngError = Err.Number
                   strError = Err.Description
                   CompleteCancel True
                   On Error GoTo 0: Err.Raise lngError, strErrSource, strError
                   Exit Sub
                 }
                 //check for cancellation
                 //if a resource error (or user canceled) encountered, just exit
                 If Not agCompGame Then
                   Exit Sub
                 }

                 //close the vol file; we are done adding resources
                 Close intVolFile

                 //update status
                 agGameEvents.RaiseEvent_CompileGameStatus csDoneAdding, 0, 0, ""
                 //check for cancellation
                 If Not agCompGame Then
                   CompleteCancel
                   Exit Sub
                 }

                 //remove any existing old dirfiles
                 If NewIsV3 Then
                   Kill NewGameDir & agGameID & "DIR.OLD"
                   Err.Clear
                 Else
                   Kill NewGameDir & "LOGDIR.OLD"
                   Err.Clear
                   Kill NewGameDir & "PICDIR.OLD"
                   Err.Clear
                   Kill NewGameDir & "SNDDIR.OLD"
                   Err.Clear
                   Kill NewGameDir & "VIEWDIR.OLD"
                   Err.Clear
                 }

                 //remove any existing old vol files
                 For i = 0 To 15
                   If NewIsV3 Then
                     Kill NewGameDir & agGameID & "VOL." & CStr(i) &".OLD"
                     Err.Clear
                   Else
                     Kill NewGameDir & "VOL." & CStr(i) &".OLD"
                     Err.Clear
                   }
                 Next i

                 //rename existing dir and vol files as .OLD
                 If NewIsV3 Then
                   Name NewGameDir & agGameID & "DIR" As NewGameDir & agGameID & "DIR.OLD"
                 Else
                   Name NewGameDir & "LOGDIR" As NewGameDir & "LOGDIR.OLD"
                   Name NewGameDir & "PICDIR" As NewGameDir & "PICDIR.OLD"
                   Name NewGameDir & "VIEWDIR" As NewGameDir & "VIEWDIR.OLD"
                   Name NewGameDir & "SNDDIR" As NewGameDir & "SNDDIR.OLD"
                 }
                 For i = 0 To 15
                   //rename current vol files
                   If NewIsV3 Then
                     Name NewGameDir & agGameID & "VOL." & CStr(i) As NewGameDir & agGameID & "VOL." & CStr(i) &".OLD"
                   Else
                     Name NewGameDir & "VOL." & CStr(i) As NewGameDir & "VOL." & CStr(i) &".OLD"
                   }
                 Next i
                 Err.Clear

                 //use errhandler to identify any issues with creating new files
                 On Error GoTo ErrHandler

                 //now build the new DIR files
                 If NewIsV3 Then
                   //one dir file
                   strFileName = NewGameDir & agGameID & "DIR"
                   intDirFile = FreeFile()
                   //create the dir file
                   Open strFileName For Binary As intDirFile
                   //add offsets - logdir offset is always 8
                   Put intDirFile, 1, CInt(8)
                   //pic offset is 8 + 3*logmax
                   tmpMax = agLogs.Max + 1
                   If tmpMax = 0 Then tmpMax = 1 // always put at least one; even if it//s all FFs
                   Put intDirFile, , CInt(8 + 3 * tmpMax)
                   i = 8 + 3 * tmpMax
                   //view offset is pic offset +3*picmax
                   tmpMax = agPics.Max + 1
                   If tmpMax = 0 Then tmpMax = 1
                   Put intDirFile, , CInt(i + 3 * tmpMax)
                   i = i + 3 * tmpMax
                   //sound is view offset + 3*viewmax
                   tmpMax = agViews.Max + 1
                   If tmpMax = 0 Then tmpMax = 1
                   Put intDirFile, , CInt(i + 3 * tmpMax)

                   //now add all the dir entries
                   //NOTE: we can't use a for-next loop
                   //because sound and view dirs are swapped in v3 directory

                   //logics first
                   tmpMax = agLogs.Max
                   For i = 0 To tmpMax
                     Put intDirFile, , bytDIR(0, 3 * i)
                     Put intDirFile, , bytDIR(0, 3 * i + 1)
                     Put intDirFile, , bytDIR(0, 3 * i + 2)
                   Next i
                   //next are pictures
                   tmpMax = agPics.Max
                   For i = 0 To tmpMax
                     Put intDirFile, , bytDIR(1, 3 * i)
                     Put intDirFile, , bytDIR(1, 3 * i + 1)
                     Put intDirFile, , bytDIR(1, 3 * i + 2)
                   Next i
                   //then views
                   tmpMax = agViews.Max
                   For i = 0 To tmpMax
                     Put intDirFile, , bytDIR(3, 3 * i)
                     Put intDirFile, , bytDIR(3, 3 * i + 1)
                     Put intDirFile, , bytDIR(3, 3 * i + 2)
                   Next i
                   //and finally, sounds
                   tmpMax = agSnds.Max
                   For i = 0 To tmpMax
                     Put intDirFile, , bytDIR(2, 3 * i)
                     Put intDirFile, , bytDIR(2, 3 * i + 1)
                     Put intDirFile, , bytDIR(2, 3 * i + 2)
                   Next i
                   //done! close the file
                   Close intDirFile
                 Else
                   //make separate dir files
                   For j = 0 To 3
                     Select Case j
                     Case rtLogic
                       strFileName = NewGameDir & "LOGDIR"
                       tmpMax = agLogs.Max
                     Case rtPicture
                       strFileName = NewGameDir & "PICDIR"
                       tmpMax = agPics.Max
                     Case rtSound
                       strFileName = NewGameDir & "SNDDIR"
                       tmpMax = agSnds.Max
                     Case rtView
                       strFileName = NewGameDir & "VIEWDIR"
                       tmpMax = agViews.Max
                     End Select

                     intDirFile = FreeFile()
                     //create the dir file
                     Open strFileName For Binary As intDirFile
                     For i = 0 To tmpMax
                       Put intDirFile, , bytDIR(j, 3 * i)
                       Put intDirFile, , bytDIR(j, 3 * i + 1)
                       Put intDirFile, , bytDIR(j, 3 * i + 2)
                     Next i
                     Close intDirFile
                   Next j
                 }

                 Err.Clear
                 For i = 0 To lngCurrentVol
                   strFileName = strID & "VOL." & CStr(i)
                   Name NewGameDir & "NEW_VOL." & CStr(i) As NewGameDir & strFileName
                 Next i

                 On Error Resume Next

                 //update status to indicate complete
                 agGameEvents.RaiseEvent_CompileGameStatus csCompileComplete, 0, 0, ""

                 //ensure errors cleared
                 Err.Clear

                 //if replacing (meaning we are compiling to the current game directory)
                 If blnReplace Then
                   //then we need to update the vol/loc info for all ingame resources;
                   //this is done here instead of when the logics are compiled because
                   //if there's an error, or the user cancels, we don't want the resources
                   //to point to the wrong place
                   For Each tmpLogic In agLogs
                     tmpLogic.Resource.Volume = bytDIR(0, tmpLogic.Number* 3) \ 0x10
                    tmpLogic.Resource.Loc = (bytDIR(0, tmpLogic.Number * 3) And 0xF) *0x10000 + bytDIR(0, tmpLogic.Number * 3 + 1) * 0x100 & +bytDIR(0, tmpLogic.Number * 3 + 2)
                   Next
                   For Each tmpPicture In agPics
                     tmpPicture.Resource.Volume = bytDIR(1, tmpPicture.Number* 3) \ 0x10
                    tmpPicture.Resource.Loc = (bytDIR(1, tmpPicture.Number * 3) And 0xF) *0x10000 + bytDIR(1, tmpPicture.Number * 3 + 1) * 0x100 & +bytDIR(1, tmpPicture.Number * 3 + 2)
                   Next
                   For Each tmpSound In agSnds
                     tmpSound.Resource.Volume = bytDIR(2, tmpSound.Number* 3) \ 0x10
                    tmpSound.Resource.Loc = (bytDIR(2, tmpSound.Number * 3) And 0xF) *0x10000 + bytDIR(2, tmpSound.Number * 3 + 1) * 0x100 & +bytDIR(2, tmpSound.Number * 3 + 2)
                   Next
                   For Each tmpView In agViews
                     tmpView.Resource.Volume = bytDIR(3, tmpView.Number* 3) \ 0x10
                    tmpView.Resource.Loc = (bytDIR(3, tmpView.Number * 3) And 0xF) *0x10000 + bytDIR(3, tmpView.Number * 3 + 1) * 0x100 & +bytDIR(3, tmpView.Number * 3 + 2)
                   Next
                 }

                 //ensure error object is cleared
                 Err.Clear

                 //save the wag file
                 SaveSettingList agGameProps

                 //reset compiling flag
                 agCompGame = False
               Exit Sub

               ErrHandler:
                 //file error when creating new DIR and VOL files
                 //pass along any errors
                 lngError = Err.Number
                 Select Case lngError
                 Case 58 //file already exists
                   //this means the previous version of the file we are trying to create
                   //wasn't able to be renamed; most likely due to being open
                   strError = "Unable to delete existing " & strFileName & ". New " & strFileName & " file was not created."
                 Case Else
                   //some other error - who knows what's going on
                   strError = Err.Description
                 End Select


                 CompleteCancel True
                 On Error GoTo 0: Err.Raise lngError, strErrSource, strError
               End Sub


               Public Property Let GameID(NewID As String)
                 //limit gameID to 6 characters for v2 games and 5 characters for v3 games

                 Dim strVolFile As String
                 Dim strExtension() As String


                 On Error GoTo ErrHandler

                 //id is undefined if a game is not loaded
                 If Not agGameLoaded Then
                   On Error GoTo 0: Err.Raise vbObjectError + 677, strErrSource, LoadResString(677)
                   Exit Property
                 }

                 //version 3 games limited to 5 characters
                 If agIsVersion3 Then
                   If Len(NewID) > 5 Then
                     NewID = Left$(NewID, 5)
                   }
                 Else
                   If Len(NewID) > 6 Then
                     NewID = Left$(NewID, 6)
                   }
                 }

                 //if no change
                 If agGameID = NewID Then
                   Exit Property
                 }

                 //if version 3
                 If agIsVersion3 Then
                   //need to rename the dir file
                   Name agGameDir & agGameID & "DIR" As agGameDir & NewID & "DIR"
                   //and vol files
                   strVolFile = Dir(agGameDir & agGameID & "VOL.*")


                   Do Until LenB(strVolFile) = 0
                     //if an archived (OLD) file, skip it
                     If UCase(Right(strVolFile, 4)) <> ".OLD" Then
                       //get extension
                       strExtension = Split(strVolFile, ".")
                       //rename
                       Name agGameDir & strVolFile As agGameDir & NewID & "VOL." & strExtension(1)
                     }

                     //get next
                     strVolFile = Dir()
                   Loop
                 }

                 //if property file is currently linked to game ID
                 If StrComp(agGameFile, agGameDir & agGameID & ".wag", vbTextCompare) = 0 Then
                   //update gamefile
                   GameFile = agGameDir & NewID & ".wag"
                 }

                 //set new id
                 agGameID = UCase$(NewID)

                 //write new property
                 WriteGameSetting("General", "GameID", NewID


               Exit Property

               ErrHandler:
                 //file renaming or property writing are only sources of error
                 strError = Err.Description
                 strErrSrc = Err.Source
                 lngError = Err.Number

                 On Error GoTo 0: Err.Raise vbObjectError + 530, strErrSrc, Replace(LoadResString(530), ARG1, CStr(lngError) & ":" & strError)
               End Property
               Public Property Get GameID() As String

                 //id is undefined if a game is not loaded
                 If Not agGameLoaded Then
                   On Error GoTo 0: Err.Raise vbObjectError + 677, strErrSource, LoadResString(677)
                   Exit Property
                 }


                 GameID = agGameID
               End Property


               Public Property Get GameLoaded() As Boolean

                 //returns state of game load
                 GameLoaded = agGameLoaded
               End Property


               Public Property Get LastEdit() As Date

                 //if game loaded,
                 if (agGameLoaded)
                   LastEdit = agLastEdit
                 Else
                   LastEdit = Now()
                 }
               End Property


               Public Property Set LogicSourceSettings(NewLogSettings As AGILogicSourceSettings)

                 Set agMainLogSettings = NewLogSettings

                 //change date of last edit
                 agLastEdit = Now()
               End Property

               Public Property Get LogicSourceSettings() As AGILogicSourceSettings


                 Set LogicSourceSettings = agMainLogSettings
               End Property


               Public Property Get MaxVol0Size() As Long


                 MaxVol0Size = agMaxVol0
               End Property

               Public Property Let MaxVol0Size(ByVal NewSize As Long)

                 //validate
                 If NewSize< 32768 Then
                   NewSize = 32768
                 ElseIf NewSize > agMaxVolSize Then
                   NewSize = agMaxVolSize
                 }


                 agMaxVol0 = NewSize
               End Property

               Public Property Get Pictures() As AGIPictures

                 Set Pictures = agPics
               End Property

               Public Property Let InterpreterVersion(NewVersion As String)

                 //attempts to set version to new Value

       Dim i As Long
                 Dim OldVersion As String


                 On Error Resume Next

                 //validate new version
                 If ValidateVersion(NewVersion) Then
                   //if in a game, need to adjust resources if necessary
                   if (agGameLoaded)
                     //if new version and old version are not same major version:
                     If Asc(agIntVersion) <> Asc(NewVersion) Then
                       //set flag to switch version on rebuild
                       agChangeVersion = True

                       //if new version is V3 (i.e., current version isn't)
                       If Not agIsVersion3 Then
                         If Len(GameID) > 5 Then
                           //truncate the ID
                           GameID = Left(GameID, 5)
                         }
                       }

                       //use compiler to rebuild new vol and dir files
                       //(it is up to calling program to deal with dirty and invalid resources)
                       CompileGame True

                       //check for errors, or cancellation
                       If Err.Number<> 0 Then
                         strError = Err.Description
                         strErrSrc = Err.Source
                         lngError = Err.Number

                         //reset cancel flag
                         agCancelComp = False
                         //pass along the error
                         On Error GoTo 0: Err.Raise vbObjectError + 641, strErrSrc, Replace(LoadResString(641), ARG1, CStr(lngError) & ":" & strError)
                         Exit Property

                       //if canceled,
                       ElseIf agCancelComp Then
                         //reset cancel flag
                         agCancelComp = False
                         Exit Property
                       }
                     }
                     Err.Clear

                     //change date of last edit
                     agLastEdit = Now()

                     //write new version to file
                     WriteGameSetting("General", "Interpreter", NewVersion

                     //and save it
                     SaveSettingList agGameProps


                   }

                   //set new version
                   agIntVersion = NewVersion

                   //set v3 flag
                   agIsVersion3 = (Val(agIntVersion) > 3)

                   //adjust commands
                   CorrectCommands agIntVersion
                 Else
                   If Val(NewVersion) < 2 Or Val(NewVersion) > 3 Then
                     //not a version 2 or 3 game
                     On Error GoTo 0: Err.Raise vbObjectError + 597, strErrSource, LoadResString(597)
                   Else
                     //unsupported version 2 or 3 game
                     On Error GoTo 0: Err.Raise vbObjectError + 543, strErrSource, LoadResString(543)
                   }
                 }


               End Property
               Public Property Get ResDir() As String

                 //if a game is loaded,
                 if (agGameLoaded)
                   //use resdir property
                   ResDir = agResDir
                 Else
                   //use current directory
                   ResDir = cDir(CurDir$())
                 }
               End Property


               Public Property Get ResDirName() As String

                 ResDirName = agResDirName
               End Property
               Public Property Let ResDirName(NewResDirName As String)

                 //validate- cant have  \/:*?<>|

                 NewResDirName = Trim$(NewResDirName)

                 If LenB(NewResDirName) = 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }
                 If InStr(1, NewResDirName, "\") <> 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }
                 If InStr(1, NewResDirName, "/") <> 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }
                 If InStr(1, NewResDirName, ":") <> 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }
                 If InStr(1, NewResDirName, "*") <> 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }
                 If InStr(1, NewResDirName, "?") <> 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }
                 If InStr(1, NewResDirName, "<") <> 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }
                 If InStr(1, NewResDirName, ">") <> 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }
                 If InStr(1, NewResDirName, "|") <> 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }
                 If InStr(1, NewResDirName, " ") <> 0 Then
                   On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                   Exit Property
                 }

                 //save new resdir name
                 agResDirName = NewResDirName
                 //update the actual resdir
                 agResDir = agGameDir & agResDirName & "\"

                 //save resdirname
                 WriteGameSetting("General", "ResDir", NewResDirName

                 //change date of last edit
                 agLastEdit = Now()
               End Property


               Public Property Get ResTypeName(ByVal ResType As AGIResType)

                 //returns the resource name
                 ResTypeName = agResTypeName(ResType)
               End Property

               Public Sub SaveProperties()

                 //this forces WinAGI to save the property file, rather than waiting until
                 //the game is unloaded

                 //Debug.Assert Not(agGameProps Is Nothing)
                 If Not(agGameProps Is Nothing) Then
                  SaveSettingList agGameProps
                }
               End Sub

               Public Property Let TemplateDir(NewDir As String)

                 //validate gamedir
                 If Not FileExists(cDir(NewDir), vbDirectory) Then
                   //return error
                   On Error GoTo 0: Err.Raise vbObjectError + 630, strErrSource, Replace(LoadResString(630), ARG1, NewDir)
                   Exit Property
                 }


                 agTemplateDir = cDir(NewDir)
               End Property

               Public Property Get TemplateDir() As String


                 TemplateDir = agTemplateDir
               End Property


               Public Property Get TestCommands() As AGITestCommands


                 Set TestCommands = agTestCmdCol
               End Property


               Public Property Get Views() As AGIViews


                 Set Views = agViews
               End Property




               Public Property Get Sounds() As AGISounds


                 Set Sounds = agSnds
               End Property

               Public Property Get Logics() As AGILogics


                 Set Logics = agLogs
               End Property

               Public Property Get GameDir() As String

                 //if a game is loaded,
                 if (agGameLoaded)
                   //use resdir property
                   GameDir = agGameDir
                 Else
                   //use current directory
                   GameDir = cDir(CurDir$())
                 }
               End Property
               Public Property Get InventoryObjects() As AGIInventoryObjects
                 //if nothing,
                 If agInvObjList Is Nothing Then
                   Set InventoryObjects = Nothing
                 Else
                   Set InventoryObjects = agInvObjList
                 }
               End Property

               Public Sub OpenGameDIR(NewGameDir As String)
                 //creates a new WinAGI game file from Sierra game directory

                 On Error Resume Next

                 //if a game is already open,
                 if (agGameLoaded)
                   //can't open a game if one is already open
                   On Error GoTo 0: Err.Raise vbObjectError + 501, strErrSource, LoadResString(501)
                   Exit Sub
                 }

                 //Debug.Assert agGameID = ""

                 //set game directory
                 agGameDir = cDir(NewGameDir)

                 //attempt to extract agGameID, agGameFile and interpreter version
                 //from Sierra files (i.e. from DIR files and VOL files)

                 //Debug.Assert LenB(agGameID) = 0
                 //Debug.Assert LenB(agGameFile) = 0
                 //Debug.Assert LenB(agIntVersion) = 0
                 //Debug.Assert agGameProps Is Nothing

                 //set status to decompiling game dir files
                 agGameEvents.RaiseEvent_LoadStatus lsDecompiling, rtNone, 0, ""

                 //check for valid DIR/VOL files
                 //(which gets gameid, and sets version 3 status flag)
                 If Not IsValidGameDir(agGameDir) Then
                   //save dir as error string
                   strError = agGameDir
                   //clear game variables
                   agGameDir = ""
                   agGameFile = ""
                   agGameID = ""
                   agIntVersion = ""
                   agIsVersion3 = False
                   //invalid game directory
                   On Error GoTo 0: Err.Raise vbObjectError + 541, strErrSource, Replace(LoadResString(541), ARG1, strError)
                   Exit Sub
                 }

                 //Debug.Assert LenB(agGameID) > 0

                 //create a new wag file
                 agGameFile = agGameDir & agGameID & ".wag"

                 //rename any existing game file (in case user is re-importing)
                 Kill agGameFile & ".OLD"
                 Err.Clear
                 Name agGameFile As agGameFile & ".OLD"
                 Err.Clear

                 //create new wag file, but don't save it yet
                 Set agGameProps = OpenSettingList(agGameFile, False)
                 agGameProps.Add "# WinAGI Game Property File"
                 agGameProps.Add "#"
                 WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION
                 WriteGameSetting("General", "GameID", agGameID

                 //get version number (version3 flag already set)
                 agIntVersion = GetIntVersion()

                 //if not valid
                 If Len(agIntVersion) = 0 Then
                   //clear game variables
                   agGameDir = ""
                   agGameFile = ""
                   agGameID = ""
                   agIsVersion3 = False
                   Set agGameProps = Nothing
                   //invalid number found
                   On Error GoTo 0: Err.Raise vbObjectError + 543, strErrSource, LoadResString(543)
                   Exit Sub
                 }

                 //save version
                 WriteGameSetting("General", "Interpreter", agIntVersion

                 //finish the game load
                 FinishGameLoad 1

                 //if error, pass it along
                 If Err.Number<> 0 Then
                   Err.Raise Err.Number, Err.Source, Err.Description
                 }
               End Sub


               Public Sub OpenGameWAG(GameWAG As String)
                 //opens a WinAGI game file (must be passed as a full length file name)

                 Dim strVer As String


                 On Error Resume Next

                 //if a game is already open,
                 if (agGameLoaded)
                   //can't open a game if one is already open
                   On Error GoTo 0: Err.Raise vbObjectError + 501, strErrSource, LoadResString(501)
                   Exit Sub
                 }

                 //Debug.Assert agGameID = ""

                 //verify the wag file exists
                 If Not FileExists(GameWAG) Then
                   //clear game variables
                   agGameDir = ""
                   agGameFile = ""
                   agGameID = ""
                   agIntVersion = ""
                   agIsVersion3 = False
                   Set agGameProps = Nothing

                   //error - is it the file, or the directory?
                   If FileExists(JustPath(GameWAG, True), vbDirectory) Then
                     //it's a missing file - return wagfile as error string
                     strError = JustFileName(GameWAG)
                     //invalid wag
                     On Error GoTo 0: Err.Raise vbObjectError + 655, strErrSource, strError
                   Else
                     //it's an invalid or missing directory - return directory as error string
                     strError = JustPath(GameWAG, True)
                     //invalid wag
                     On Error GoTo 0: Err.Raise vbObjectError + 541, strErrSource, JustPath(GameWAG, True)
                   }


                   Exit Sub
                 }

                 //set game file property
                 agGameFile = GameWAG

                 //set game directory
                 agGameDir = JustPath(GameWAG)

                 //open the property file (file has to already exist)
                 Set agGameProps = OpenSettingList(agGameFile, False)

                 //check to see if it's valid
                 strVer = ReadSettingString(agGameProps, "General", "WinAGIVersion", "")
                 If Len(strVer) = 0 Then
                   //not a valid file; maybe it's an old binary version
                   ConvertWag
                 Else
                   Select Case strVer
                   Case WINAGI_VERSION
                     //this is the current version

               //    Case "1.2.2", "1.2.3", "1.2.4", "1.2.5", "1.2.6", "1.2.7", "1.2.8" // need to convert it
               //      WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION


                   Case Else
                     //any v1.2.x is ok
                     If Left(strVer, 4) = "1.2." Then
                       //ok, but update
                       WriteGameSetting("General", "WinAGIVersion", WINAGI_VERSION
                     Else
                       //if another winagi version is ever released,
                       //yeah, right; just consider this file invalid

                       //save wagfile as error string
                       strError = JustFileName(agGameFile)
                       //clear game variables
                       agGameDir = ""
                       agGameFile = ""
                       agGameID = ""
                       agIntVersion = ""
                       agIsVersion3 = False
                       Set agGameProps = Nothing
                       //invalid wag
                       On Error GoTo 0: Err.Raise vbObjectError + 665, strErrSource, strError
                       Exit Sub
                     }
                   End Select
                 }

                 //get gameID
                 agGameID = ReadSettingString(agGameProps, "General", "GameID", "")

                 //if an id is found, keep going
                 If Len(agGameID) > 0 Then
                   //got ID; now get interpreter version from propfile
                   agIntVersion = ReadSettingString(agGameProps, "General", "Interpreter", "")

                   //validate it
                   If Not ValidateVersion(agIntVersion) Then
                     //save wagfile as error string
                     strError = JustFileName(agGameFile)
                     //clear propfile
                     Set agGameProps = Nothing
                     //clear game variables
                     agGameDir = ""
                     agGameFile = ""
                     agGameID = ""
                     agIntVersion = ""
                     agIsVersion3 = False
                     //invalid int version inside wag file
                     On Error GoTo 0: Err.Raise vbObjectError + 691, strErrSource, strError
                     Exit Sub
                   }


                 Else
                   //missing GameID in wag file - make user address it
                   //by raising an error

                   //save wagfile name as error string
                   strError = JustFileName(agGameFile)
                   //clear propfile
                   Set agGameProps = Nothing
                   //clear game variables
                   agGameDir = ""
                   agGameFile = ""
                   agGameID = ""
                   agIntVersion = ""
                   agIsVersion3 = False
                   //invalid wag file
                   On Error GoTo 0: Err.Raise vbObjectError + 690, strErrSource, strError
                   Exit Sub
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

                 //finish the game load
                 FinishGameLoad 0

                 //if error, pass it along
                 If Err.Number<> 0 Then
                   Err.Raise Err.Number, Err.Source, Err.Description
                 }
               End Sub

               Public Sub FinishGameLoad(Optional ByVal Mode As Long = 0)
                 //finishes game load
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
                 If Mode = 0 Then
                   //informs user we are checking property file to set game parameters
                   agGameEvents.RaiseEvent_LoadStatus lsPropertyFile, rtNone, 0, ""

                   //get resdir before loading resources
                   agResDirName = ReadSettingString(agGameProps, "General", "ResDir", "")
                 Else
                   //no resdir yet for imported game
                   agResDirName = ""
                 }

                 //if none, use default
                 If LenB(agResDirName) = 0 Then
                   //look for an existing directory
                   strTempDir = Dir(agGameDir, vbDirectory)
                   Do Until Len(strTempDir) = 0
                     If strTempDir<> "." And strTempDir <> ".." Then
                       If (GetAttr(agGameDir & strTempDir) And vbDirectory) = vbDirectory Then
                         lngDirCount = lngDirCount + 1
                         If lngDirCount > 1 Then
                           Exit Do
                         }
                         strTempFile = strTempDir
                       }
                     }
                     strTempDir = Dir(, vbDirectory)
                   Loop

                   //is there only one subfolder?
                   If lngDirCount = 1 Then
                     //assume it is resource directory
                       agResDirName = strTempFile
                   Else
                     //either no subfolders, or more than one
                     //so use default
                     agResDirName = agDefResDir
                   }

                   WriteGameSetting("General", "ResDir", agResDirName
                 }

                 //now create full resdir from name
                 agResDir = agGameDir & agResDirName & "\"

                 //ensure resource directory exists
                 If Not FileExists(agGameDir & agResDirName, vbDirectory) Then
                   MkDir agGameDir & agResDirName
                   //if can't create the resources directory
                   If Err.Number<> 0 Then
                     //note the problem in the error log as a warning
                     RecordLogEvent leWarning, "Can't create " & agResDir
                     //use game directory
                     agResDir = agGameDir
                     //set warning
                     blnWarnings = True
                   }
                 }

               //  //if loading from a WAG file
               //
               //  If Mode = 0 Then
               //    //only resources in the WAG file need to be checked on initial load
               //    blnWarnings = LoadResources()
               //  Else
               //    //if opening bi directory, extract resources from the VOL
               //    //files load resources
                      blnWarnings = ExtractResources()
               //  }

                 //if there was an error
                 If Err.Number<> 0 Then
                   //can't continue
                   //save error information
                   strError = Err.Description
                   lngError = Err.Number

                   //clear game variables
                   agGameDir = ""
                   agGameFile = ""
                   agGameID = ""
                   agIntVersion = ""
                   agIsVersion3 = False

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
                   On Error GoTo 0: Err.Raise lngError, strErrSource, strError
                 }

                 //load vocabulary word list
                 agGameEvents.RaiseEvent_LoadStatus lsResources, rtWords, 0, ""

                 Set agVocabWords = New AGIWordList
                 agVocabWords.Init
                 agVocabWords.Load agGameDir & "WORDS.TOK"
                 //if there was an error,
                 If Err.Number<> 0 Then
                   //note the problem in the error log as a warning
                   RecordLogEvent leError, "An error occurred while loading WORDS.TOK: " & Err.Description
                   //reset warning flag
                   blnWarnings = True
                 }

                 //load inventory objects list
                 agGameEvents.RaiseEvent_LoadStatus lsResources, rtObjects, 0, ""

                 Set agInvObjList = New AGIInventoryObjects
                 agInvObjList.Init
                 agInvObjList.Load agGameDir & "OBJECT"
                 //if there was an error,
                 If Err.Number<> 0 Then
                   //note the problem in the error log as a warning
                   RecordLogEvent leError, "An error occurred while loading OBJECT: " & Err.Description
                   //reset warning flag
                   blnWarnings = True
                 }


                 agGameEvents.RaiseEvent_LoadStatus lsFinalizing, rtNone, 0, ""

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
                 If Err.Number<> 0 Then
                   //record event
                   RecordLogEvent leWarning, "Error while loading WAG file; some properties not loaded. (Error number: " & CStr(Err.Number) & ")"
                   Err.Clear
                   blnWarnings = True
                 }

                 //force id reset
                 blnSetIDs = False

                 //we've established that the game can be opened
                 //so set the loaded flag now
                 agGameLoaded = True

                 //write create date
                 WriteGameSetting("General", "LastEdit", agLastEdit

                 //and save the wag file
                 SaveSettingList agGameProps

                 //if errors
                 If blnWarnings Then
                   On Error GoTo 0: Err.Raise vbObjectError + 636, strErrSource, LoadResString(636)
                 }
               End Sub



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
                   On Error GoTo 0: Err.Raise vbObjectError + 501, strErrSource, LoadResString(501)
                   Exit Sub
                 }

                 //if not a valid directory
                 If Not FileExists(NewGameDir, vbDirectory) Then
                   //raise error
                   On Error GoTo 0: Err.Raise vbObjectError + 630, strErrSource, Replace(LoadResString(630), ARG1, NewGameDir)
                   Exit Sub
                 }

                 //if a game already exists
                 If LenB(Dir(cDir(NewGameDir) &"*.wag")) <> 0 Then
                  //game file exists;
                  On Error GoTo 0: Err.Raise vbObjectError + 687, strErrSource, LoadResString(687)
                   Exit Sub


                 ElseIf IsValidGameDir(cDir(NewGameDir)) Then
                   //game files exist;
                   agGameDir = ""
                   agIsVersion3 = False
                   On Error GoTo 0: Err.Raise vbObjectError + 687, strErrSource, LoadResString(687)
                   Exit Sub
                 }

                 //set game directory
                 agGameDir = cDir(NewGameDir)

                 //ensure resdir is valid
                 If LenB(NewResDir) = 0 Then
                   //if blank use default
                   NewResDir = agDefResDir
                 }

                 //if using template
                 If LenB(TemplateDir) <> 0 Then
                   //template should include dir files, vol files, logic
                   //source, words and objects lists;
                   //globals lists and layouts

                   //(first, make sure it's formatted as a directory
                   TemplateDir = cDir(TemplateDir)
                   //we need to know name of the resource subdir so we can
                   //rename it later, if needed

                   // get wag file
                   strGameWAG = Dir(TemplateDir & "*.wag")
                   //if not a valid directory
                   If Len(strGameWAG) = 0 Then
                     //raise error
                     On Error GoTo 0: Err.Raise vbObjectError + 630, strErrSource, LoadResString(508)
                     Exit Sub
                   }

                   //find first subdir, use that name
                   strTmplResDir = Dir(TemplateDir, vbDirectory)
                   Do Until Len(strTmplResDir) = 0
                     If strTmplResDir<> "." And strTmplResDir <> ".." Then
                       If (GetAttr(TemplateDir & strTmplResDir) And vbDirectory) = vbDirectory Then
                         //this is it
                         lngDirCount = 1
                         Exit Do
                       }
                     }
                     strTmplResDir = Dir()
                   Loop

                   //if no resdir found, reset template resdir name so it gets handled correctly
                   If lngDirCount<> 1 Then
                     strTmplResDir = ""
                   }

                   //copy all files from the templatedir into gamedir
                   If Not CopyFolder(TemplateDir, agGameDir, False) Then
                     On Error GoTo 0: Err.Raise vbObjectError + 683, strErrSource, Replace(LoadResString(683), ARG1, Err.Description)
                     Exit Sub
                   }

                   //open game with template id
                   OpenGameWAG agGameDir & strGameWAG

                   If Err.Number<> 0 Then
                    On Error GoTo 0: Err.Raise vbObjectError + 684, strErrSource, Replace(LoadResString(684), ARG1, Err.Description)
                     Exit Sub
                   }

                   //we need to rename the resdir
                   //(have to do this AFTER load, because loading will keep the current
                   //resdir that's in the WAG file)
                   If NewResDir<> strTmplResDir Then
                     //we need to change it
                       Name agGameDir & strTmplResDir As agGameDir & NewResDir
                   }
                   //then change the resource directory property
                   agResDirName = NewResDir
                   //update the actual resdir
                   agResDir = agGameDir & agResDirName & "\"

                   //change gameid
                   GameID = NewID
                   If Err.Number<> 0 Then
                     On Error GoTo 0: Err.Raise vbObjectError + 685, strErrSource, Replace(LoadResString(685), ARG1, Err.Description)
                     Exit Sub
                   }

                   //update global file header
                   Set stlGlobals = OpenSettingList(agGameDir & "globals.txt", False)
                   If Not stlGlobals Is Nothing Then
                     If stlGlobals.Count > 3 Then
                       If Left(Trim(stlGlobals.StringLine(2)), 1) = "[" Then
                         stlGlobals.StringLine(2) = "[ global defines file for " & NewID
                       }
                       //save it
                       SaveSettingList stlGlobals
                     }
                     //toss it
                     Set stlGlobals = Nothing
                   }

                 //if not using template,
                 Else
                   //validate new version
                   If ValidateVersion(NewVersion) Then
                     //ok; set version
                     agIntVersion = NewVersion
                     //set version3 flag
                     agIsVersion3 = (Val(NewVersion) > 3)
                   Else
                     If Val(NewVersion) < 2 Or Val(NewVersion) > 3 Then
                       //not a version 2 or 3 game
                       On Error GoTo 0: Err.Raise vbObjectError + 597, strErrSource, LoadResString(597)
                     Else
                       //unsupported version 2 or 3 game
                       On Error GoTo 0: Err.Raise vbObjectError + 543, strErrSource, LoadResString(543)
                     }
                     Exit Sub
                   }

                   //set game id (limit to 6 characters for v2, and 5 characters for v3
                   //(don't use the GameID property; gameloaded flag is not set yet
                   //so using GameID property will cause error)
                   If agIsVersion3 Then
                     agGameID = UCase$(Left$(NewID, 5))
                   Else
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
                   If agIsVersion3 Then
                     intFile = FreeFile()
                     Open agGameDir & agGameID & "DIR" For Binary As intFile
                     Put intFile, 1, Chr$(8) &Chr$(0) & Chr$(8) & Chr$(3) & Chr$(8) & Chr$(6) & Chr$(8) & Chr$(9)
                     Put intFile, , strDirData
                     Put intFile, , strDirData
                     Put intFile, , strDirData
                     Put intFile, , strDirData
                     Close intFile
                   Else
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
                   agVocabWords.Init True
                   agVocabWords.AddWord "a", 0
                   agVocabWords.AddWord "anyword", 1
                   agVocabWords.AddWord "rol", 9999
                   agVocabWords.Save

                   //create inventory objects list
                   Set agInvObjList = New AGIInventoryObjects
                   //use loaded argument to force load of new inventory list
                   agInvObjList.Init True
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
                   blnSetIDs = False

                   //set open flag, so properties can be updated
                   agGameLoaded = True
                 }

                 //set resource directory
                 //Debug.Assert LenB(agResDirName) <> 0
                 //Debug.Assert agResDir = agGameDir & agResDirName & "\"


                 On Error Resume Next
                 //ensure resource directory exists
                 If Not FileExists(agGameDir & agResDirName, vbDirectory) Then
                   MkDir agGameDir & agResDirName
                   //if can't create the resources directory
                   If Err.Number<> 0 Then
                     //note the problem in the error log as a warning
                     RecordLogEvent leWarning, "Can't create " & agResDir
                     //use main directory
                     agResDir = agGameDir
                     //set warning flag
                     blnWarnings = True
                   }
                 }

                 //for non-template games, save the source code for logic 0
                 If LenB(TemplateDir) = 0 Then
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
                 If blnWarnings Then
                   On Error GoTo 0: Err.Raise vbObjectError + 637, strErrSource, LoadResString(637)
                 }

                 //Debug.Print "logic0: ", Logics(0).Loaded
               Exit Sub

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


                 On Error GoTo 0: Err.Raise vbObjectError + 631, strErrSrc, Replace(LoadResString(631), ARG1, CStr(lngError) & ":" & strError)
               End Sub

               Public Property Get VocabularyWords() As AGIWordList


                 Set VocabularyWords = agVocabWords
               End Property
               Public Sub WriteProperty(ByVal Section As String, ByVal Key As String, ByVal value As String, Optional Group As String = "", Optional ForceSave As Boolean = False)

                 // this procedure provides calling programs a way to write property
                 // values to the WAG file

                 // no validation of section or newval is done, so calling function
                 // needs to be careful

                 On Error GoTo ErrHandler


                 WriteGameSetting(Section, Key, value, Group

                 // if forcing a save
                 If ForceSave Then
                   SaveProperties
                 }
               Exit Sub

               ErrHandler:
                 //Debug.Assert False
                 Resume Next
               End Sub

               Private Sub Class_Initialize()
                 Dim rtn As Long


                 On Error GoTo ErrHandler

                 //save copy of this object to allow internal access
                 Set agMainGame = Me

                 //set events object
                 Set agGameEvents = New AGIGameEvents

                 //initialize arrays that serve as constants
                 InitializeAGI

                 //set collections
                 Set agLogs = New AGILogics
                 Set agPics = New AGIPictures
                 Set agSnds = New AGISounds
                 Set agViews = New AGIViews
                 Set agInvObjList = New AGIInventoryObjects
                 Set agVocabWords = New AGIWordList

                 //initialize logic source settings object and command info
                 Set agMainLogSettings = New AGILogicSourceSettings
                 Set agCmdCol = New AGICommands
                 Set agTestCmdCol = New AGITestCommands
                 //assign commands
                 AssignCommands

                 //assign default reserved defines
                 AssignReservedDefines

                 strErrSource = "agiGame"
               Exit Sub

               ErrHandler:
                 //Debug.Assert False
                 Resume Next
               End Sub



               Private Sub Class_Terminate()

                 //release objects
                 Set agLogs = Nothing
                 Set agPics = Nothing
                 Set agSnds = Nothing
                 Set agViews = Nothing


                 Set agInvObjList = Nothing
                 Set agVocabWords = Nothing

                 Set agGameEvents = Nothing
                 Set agMainGame = Nothing
               End Sub
               */
  }
}
