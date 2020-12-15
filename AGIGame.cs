using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinAGI
{
    static class AGIGame
    {
        //AGIGame agMainGame;

        // exposed game properties, methods, objects
        static public AGILogicSourceSettings agMainLogSettings = new AGILogicSourceSettings();
        //static public AGICommands agCmdCol = new AGICommands();
        //static public AGITestCommands agTestCmdCol = new AGITestCommands();
        static public AGIGameEvents agGameEvents = new AGIGameEvents();

        static string[] agGameProps;// As StringList


        static string strErrSource = "";

        //status of game load
        static bool agGameLoaded = false;

        //game compile variables
        static bool agCompGame = false;
        static bool agCancelComp = false;
        static bool agChangeVersion = false;

        //local variable(s) to hold property Value(s)
        //for game properties which need to be accessible
        //from all objects in the game system
        static AGILogics agLogs = new AGILogics();
        static AGISounds agSnds = new AGISounds();
        static AGIViews agViews = new AGIViews();
        static AGIPictures agPics = new AGIPictures();
        static AGIInventoryObjects agInvObj = new AGIInventoryObjects();
        static AGIWordList agVocabWords = new AGIWordList();

        static internal string agGameDir = "";
        static string agResDir = "";
        static string agResDirName = "";
        static string agDefResDir = "";
        static string agTemplateDir = "";
        static string agGameID = "";
        static string agAuthor = "";
        static DateTime agLastEdit; //As Date
        static string agDescription = "";
        static string agIntVersion = "";
        static bool agIsVersion3 = false;
        static string agAbout = "";
        static string agGameVersion = "";
        static string agGameFile = "";

        //settings strategy: open the file, store in a stringlist; access the stringlist
        //as needed to make changes; include a ForceWrite option so results can be written periodically
        //when game (or app) closes, final save of the file
        static string[] agGameSettings; // As StringList

        static int agMaxVol0 = 0;
        static int agMaxVolSize = 0;
        static string agCompileDir = "";
        static int agPlatformType = 0;
        // 0 = none
        // 1 = DosBox
        // 2 = ScummVM
        // 3 = NAGI
        // 4 = Other
        static string agPlatformFile = "";
        static string agPlatformOpts = "";
        static string agDOSExec = "";
        static bool agUseLE = false;

        static string agSrcExt = "";

        //error number and string to return error values
        //from various functions/subroutines
        static int lngError = 0;
        static string strError = "";
        static string strErrSrc = "";

        //arrays which will be treated as constants
        //rev colors have red and blue components switched
        //so api functions using colors work correctly
        static int[] lngEGARevCol = new int[16]; //15
        static int[] lngEGACol = new int[16]; //15;
        static AGIColors[] agColor = new AGIColors[16]; //15;
        static byte[] bytEncryptKey = { (byte)'A', (byte)'v', (byte)'i',
                             (byte)'s', (byte)' ', (byte)'D',
                             (byte)'u', (byte)'r', (byte)'g',
                             (byte)'a', (byte)'n' }; //10; //' = "Avis Durgan"
        static string[] ResTypeAbbrv = { "LOG", "PIC", "VIEW", "SND" };
        static string[] agResTypeName = { "Logic", "Picture", "View", "Sound" };
        static string[] agVersion = new string[16]; //15; // (2.917, etc)

        //game constants
        internal const int MAX_RES_SIZE = 65530;
        internal const int MAX_LOOPS = 255;
        internal const int MAX_CELS = 255;
        internal const int MAX_ITEMS = 256;
        internal const int MAX_VOL_FILES = 65;
        internal const int MAX_CEL_WIDTH = 160;
        internal const int MAX_CEL_HEIGHT = 168;
        internal const int MAX_GROUP_NUM = 65535;
        internal const int MAX_WORD_GROUPS = 65535;
        //Public Const MAX_VOLSIZE = 1047552  '= 1024 * 1023
        internal const string WORD_SEPARATOR = " | ";

        //temp file location
        static string TempFileDir = "";
        static internal uint[] CRC32Table = new uint[256];
        static internal bool CRC32Loaded;

        static public void CancelCompile()
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

        /*         Public Property Get Commands() As AGICommands

                   Set Commands = agCmdCol
                 End Property

                 Public Property Let DefResDir(NewDir As String)

                   //validate- cant have  \/:*?<>|

                   NewDir = Trim$(NewDir)

                   If LenB(NewDir) = 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }
                   If InStr(1, NewDir, "\") <> 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }
                   If InStr(1, NewDir, "/") <> 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }
                   If InStr(1, NewDir, ":") <> 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }
                   If InStr(1, NewDir, "*") <> 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }
                   If InStr(1, NewDir, "?") <> 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }
                   If InStr(1, NewDir, "<") <> 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }
                   If InStr(1, NewDir, ">") <> 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }
                   If InStr(1, NewDir, "|") <> 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }
                   If InStr(1, NewDir, " ") <> 0 Then
                     On Error GoTo 0: Err.Raise 380, strErrSource, "Invalid property Value"
                     Exit Property
                   }

                   //save new resdir name
                   agDefResDir = NewDir

                   //change date of last edit
                   agLastEdit = Now()
                 End Property

                 Public Property Get DefResDir() As String
                   DefResDir = agDefResDir
                 End Property

                 Public Property Let DOSExec(NewVal As String)

                   //no validation required
                   agDOSExec = NewVal

                   //if a game is loaded,
         If agGameLoaded Then
                     //write new property
                     WriteGameSetting "General", "DOSExec", agDOSExec
                   }
                 End Property

                 Public Property Get DOSExec() As String

                   //return stored value
                   DOSExec = agDOSExec

                 End Property

                 Public Property Let EGAColor(ByVal Index As Integer, ByVal NewColor As Long)

                   //in VB (and other languages?) colors are four byte:
                   //         xxbbggrr,
                   // this is the format used by EGAColor

                   //in API calls, the red and blue values are reversed:
                   //         xxrrggbb
                   //(perhaps because of way CopyMemory works?)
                   //so API calls need to use EGARevColor instead

                   //validate index
                   If Index< 0 Or Index > 15 Then
                     On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
                     Exit Property
                   }


                   On Error Resume Next

                   //store the new color
                   lngEGACol(Index) = NewColor

                   //now invert red and blue components for revcolor
         lngEGARevCol(Index) = (NewColor And 0xFF000000) +(NewColor And 0xFF) *0x10000 + (NewColor And 0xFF00 &) +(NewColor And 0xFF0000) / 0x10000

                   //if in a game, save the color in the game's WAG file
         If agGameLoaded Then
                     WriteGameSetting "Palette", "Color" & CStr(Index), "&H" & PadHex(NewColor, 8)
                   }


                 End Property

                 Public Property Get EGAColor(ByVal Index As Integer) As Long

                   //in VB (and other languages?) colors are four byte:
                   //         xxbbggrr,
                   // this is the format used by EGAColor

                   //in API calls, the red and blue values are reversed:
                   //         xxrrggbb
                   //(perhaps because of way CopyMemory works?)
                   //so API calls need to use EGARevColor instead

                   //validate index
                   If Index< 0 Or Index > 15 Then
                     On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
                     Exit Property
                   }


                   EGAColor = lngEGACol(Index)
                 End Property

                 Public Property Get EGARevColor(ByVal Index As Integer) As Long

                   //in VB (and other languages?) colors are four byte:
                   //         xxbbggrr,
                   // this is the format used by EGAColor

                   //in API calls, the red and blue values are reversed:
                   //         xxrrggbb
                   //(perhaps because of way CopyMemory works?)
                   //so API calls need to use EGARevColor instead

                   //validate index
                   If Index< 0 Or Index > 15 Then
                     On Error GoTo 0: Err.Raise 9, strErrSource, "Subscript out of range"
                     Exit Property
                   }


                   EGARevColor = lngEGARevCol(Index)
                 End Property


                 Public Property Let GameAbout(NewAbout As String)

                   //limit to 4096 characters
                   If Len(NewAbout) > 4096 Then
                     agAbout = Left$(NewAbout, 4096)
                   Else
                     agAbout = NewAbout
                   }

                   //if a game is loaded,
                   If agGameLoaded Then
                     //write new property
                     WriteGameSetting "General", "About", agAbout
                   }
                 End Property

                 Public Property Get GameAbout() As String

                   //game about doesn't depend on game load status
                   //just return what it is...
                   GameAbout = agAbout
                 End Property

                 Public Property Let GameAuthor(NewAuthor As String)

                   //limit author to 256 bytes
                   If Len(NewAuthor) > 256 Then
                     agAuthor = Left$(NewAuthor, 256)
                   Else
                     agAuthor = NewAuthor
                   }

                   //if game loaded,
                   If agGameLoaded Then
                     //write property
                     WriteGameSetting "General", "Author", agAuthor
                   }
                 End Property


                 Public Property Get GameAuthor() As String

                   //game about doesn't depend on game load status
                   //just return what it is...
                   GameAuthor = agAuthor
                 End Property

                 Public Property Let GameDescription(NewDescription As String)

                   //comments limited to 4K
                   If Len(NewDescription) > 4096 Then
                     agDescription = Left$(NewDescription, 4096)
                   Else
                     agDescription = NewDescription
                   }

                   //if a game is loaded
                   If agGameLoaded Then
                     //write new property
                     WriteGameSetting "General", "Description", agDescription
                   }
                 End Property


                 Public Property Get GameDescription() As String

                   //game about doesn't depend on game load status
                   //just return what it is...
                   GameDescription = agDescription
                 End Property
        */
        static public string GameDir
        {
            get { return agGameDir; }
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
                    if (Directory.Exists(cDir(value))) //, vbDirectory) 
                                                       //return error
                                                       //On Error GoTo 0: Err.Raise vbObjectError + 630, strErrSource, Replace(LoadResString(630), ARG1, NewDir)
                                                       //Exit Property
                        throw new System.NotImplementedException();

                //change the directory
                agGameDir = cDir(value);

                //update gamefile name
                agGameFile = agGameDir + JustFileName(agGameFile);

                //update resdir
                agResDir = agGameDir + agResDirName + @"\";

                //change date of last edit
                agLastEdit = DateTime.Now;
            }
        }

        /*
                   Public Property Get GameEvents() As AGIGameEvents


                     Set GameEvents = agGameEvents
                   End Property

                   Public Property Get GameFile() As String

                     //gamefile is undefined if a game is not loaded
                     If Not agGameLoaded Then
                       On Error GoTo 0: Err.Raise vbObjectError + 693, strErrSource, LoadResString(693)
                       Exit Property
                     }

                     GameFile = agGameFile
                   End Property

                   Public Property Let GameFile(ByVal NewFile As String)

                     //gamefile is undefined if a game is not loaded
                     If Not agGameLoaded Then
                       On Error GoTo 0: Err.Raise vbObjectError + 693, strErrSource, LoadResString(693)
                       Exit Property
                     }

                     //rename existing game property file
                     On Error Resume Next
                     Name agGameFile As NewFile
                     Err.Clear

                     //calling function has to make sure NewFile is valid!
                     agGameFile = NewFile

                     //change date of last edit
                     agLastEdit = Now()
                     //write date of last edit
                     WriteGameSetting "General", "LastEdit", agLastEdit

                     //save the game prop file
                     agGameProps.StringLine(0) = agGameFile
                     SaveSettingList agGameProps
                   End Property

                   Public Property Get PlatformType() As Long


                     PlatformType = agPlatformType
                   End Property

                   Public Property Let PlatformType(NewVal As Long)

                     //only 0 - 4 are valid
                     If NewVal< 0 Or NewVal > 4 Then
                       agPlatformType = 0
                     Else
                       agPlatformType = NewVal
                     }

                     //if a game is loaded,
                     If agGameLoaded Then
                       //write new property
                       WriteGameSetting "General", "PlatformType", agPlatformType
                     }
                   End Property

                   Public Property Let Platform(NewVal As String)

                     //no validation required
           agPlatformFile = NewVal

                     //if a game is loaded,
           If agGameLoaded Then
                       //write new property
                       WriteGameSetting "General", "Platform", agPlatformFile
                     }
                   End Property

                   Public Property Let PlatformOpts(NewVal As String)

                     //no validation required
                     agPlatformOpts = NewVal

                     //if a game is loaded,
           If agGameLoaded Then
                       //write new property
                       WriteGameSetting "General", "PlatformOpts", agPlatformOpts
                     }
                   End Property
                   Public Property Let UseLE(NewVal As Boolean)

                     // no validation required
                     agUseLE = NewVal

                     //if a game is loaded,
           If agGameLoaded Then
                       //write new property
                       WriteGameSetting "General", "UseLE", agUseLE
                     }
                   End Property


                   Public Property Get Platform() As String

                     //return stored Value
                     Platform = agPlatformFile
                   End Property
                   Public Property Get UseLE() As Boolean

                     //if a game is not loaded
                     If Not agGameLoaded Then
                       //always return false
                       UseLE = False
                     Else
                       //return stored value
                       UseLE = agUseLE
                     }
                   End Property

                   Public Property Get PlatformOpts() As String

                     //return stored Value
                     PlatformOpts = agPlatformOpts
                   End Property

                   Public Property Get GameVersion() As String

                     //game version doesn't depend on current load status;
                     //just return what it is
                     GameVersion = agGameVersion
                   End Property

                   Public Property Let GameVersion(NewVersion As String)

                     //limit to 256 bytes
                     If Len(NewVersion) > 256 Then
                       agGameVersion = Left$(NewVersion, 256)
                     Else
                       agGameVersion = NewVersion
                     }

                     //if game loaded
                     If agGameLoaded Then
                       //write new property
                       WriteGameSetting "General", "GameVersion", agGameVersion
                     }
                   End Property

                   Public Property Get GlobalDefines() As TDefine()

                     Dim dtFileMod As Date


                     On Error Resume Next
                     //initialize global defines


                     dtFileMod = FileLastMod(agGameDir & "globals.txt")
                     If CRC32(StrConv(CStr(dtFileMod), vbFromUnicode)) <> agGlobalCRC Then
                       GetGlobalDefines
                     }

                     GlobalDefines = agGlobal()
                   End Property

                   Public Property Get InterpreterVersion() As String

                     //if a game is loaded
                     If agGameLoaded Then
                       InterpreterVersion = agIntVersion
                     Else
                       //if no version yet
                       If Len(agIntVersion) = 0 Then
                         //      //use 2.917
                         InterpreterVersion = "2.917"
                       }
                     }
                   End Property
                   Public Sub CloseGame()


                     On Error GoTo ErrHandler

                     //if no game is currently loaded
                     If Not agGameLoaded Then
                       //don't do anything
                       Exit Sub
                     }

                     //unload and remove all resources
                     agViews.Clear
                     agLogs.Clear
                     agPics.Clear
                     agSnds.Clear
                     If agInvObj.Loaded Then
                       agInvObj.Unload
                     }
                     agInvObj.InGame = False
                     If agVocabWords.Loaded Then
                       agVocabWords.Unload
                     }
                     agVocabWords.InGame = False


                     On Error Resume Next

                     //restore default AGI colors
                     RestoreDefaultColors

                     //write date of last edit
                     WriteGameSetting "General", "LastEdit", agLastEdit

                     //now save it
                     SaveSettingList agGameProps
                     Set agGameProps = Nothing

                     //then clear it
                     agLastEdit = 0

                     //clear out other properties
                     agIntVersion = vbNullString
                     agIsVersion3 = False
                     agDescription = vbNullString
                     agGameID = vbNullString
                     agAuthor = vbNullString
                     agGameVersion = vbNullString
                     agAbout = vbNullString
                     agResDirName = vbNullString
                     agPlatformType = 0
                     agPlatformFile = vbNullString
                     agPlatformOpts = vbNullString
                     agDOSExec = vbNullString


                     agGameFile = vbNullString

                     //clear flag
                     agGameLoaded = False

                     //reset global CRC
                     agGlobalCRC = 0

                     //finally, reset directories
                     agGameDir = vbNullString
                     agResDir = vbNullString
                   Exit Sub

                   ErrHandler:
                     strError = Err.Description
                     strErrSrc = Err.Source
                     lngError = Err.Number


                     On Error GoTo 0: Err.Raise vbObjectError + 644, strErrSrc, Replace(LoadResString(644), ARG1, CStr(lngError) & ":" & strError)
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
                       strID = vbNullString
                     }

                     //if not rebuildonly
                     If Not RebuildOnly Then
                       //set status
                       agGameEvents.RaiseEvent_CompileGameStatus csCompWords, 0, 0, vbNullString
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
                       agGameEvents.RaiseEvent_CompileGameStatus csCompObjects, 0, 0, vbNullString
                       //check for cancellation
                       If Not agCompGame Then
                         CompleteCancel
                         Exit Sub
                       }

                       //compile OBJECT file if dirty
                       If agInvObj.IsDirty Then
                         agInvObj.Save
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
                         FileCopy agInvObj.ResFile, NewGameDir & "OBJECT"
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
                     agGameEvents.RaiseEvent_CompileGameStatus csDoneAdding, 0, 0, vbNullString
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
                     agGameEvents.RaiseEvent_CompileGameStatus csCompileComplete, 0, 0, vbNullString

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
                     WriteGameSetting "General", "GameID", NewID


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
                     If agGameLoaded Then
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
                       If agGameLoaded Then
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
                         WriteGameSetting "General", "Interpreter", NewVersion

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
                     If agGameLoaded Then
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
                     WriteGameSetting "General", "ResDir", NewResDirName

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
                     If agGameLoaded Then
                       //use resdir property
                       GameDir = agGameDir
                     Else
                       //use current directory
                       GameDir = cDir(CurDir$())
                     }
                   End Property
                   Public Property Get InventoryObjects() As AGIInventoryObjects
                     //if nothing,
                     If agInvObj Is Nothing Then
                       Set InventoryObjects = Nothing
                     Else
                       Set InventoryObjects = agInvObj
                     }
                   End Property

                   Public Sub OpenGameDIR(NewGameDir As String)
                     //creates a new WinAGI game file from Sierra game directory

                     On Error Resume Next

                     //if a game is already open,
                     If agGameLoaded Then
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
                     agGameEvents.RaiseEvent_LoadStatus lsDecompiling, rtNone, 0, vbNullString

                     //check for valid DIR/VOL files
                     //(which gets gameid, and sets version 3 status flag)
                     If Not IsValidGameDir(agGameDir) Then
                       //save dir as error string
                       strError = agGameDir
                       //clear game variables
                       agGameDir = vbNullString
                       agGameFile = vbNullString
                       agGameID = vbNullString
                       agIntVersion = vbNullString
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
                     WriteGameSetting "General", "WinAGIVersion", WINAGI_VERSION
                     WriteGameSetting "General", "GameID", agGameID

                     //get version number (version3 flag already set)
                     agIntVersion = GetIntVersion()

                     //if not valid
                     If Len(agIntVersion) = 0 Then
                       //clear game variables
                       agGameDir = vbNullString
                       agGameFile = vbNullString
                       agGameID = vbNullString
                       agIsVersion3 = False
                       Set agGameProps = Nothing
                       //invalid number found
                       On Error GoTo 0: Err.Raise vbObjectError + 543, strErrSource, LoadResString(543)
                       Exit Sub
                     }

                     //save version
                     WriteGameSetting "General", "Interpreter", agIntVersion

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
                     If agGameLoaded Then
                       //can't open a game if one is already open
                       On Error GoTo 0: Err.Raise vbObjectError + 501, strErrSource, LoadResString(501)
                       Exit Sub
                     }

                     //Debug.Assert agGameID = ""

                     //verify the wag file exists
                     If Not FileExists(GameWAG) Then
                       //clear game variables
                       agGameDir = vbNullString
                       agGameFile = vbNullString
                       agGameID = vbNullString
                       agIntVersion = vbNullString
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
                   //      WriteGameSetting "General", "WinAGIVersion", WINAGI_VERSION


                       Case Else
                         //any v1.2.x is ok
                         If Left(strVer, 4) = "1.2." Then
                           //ok, but update
                           WriteGameSetting "General", "WinAGIVersion", WINAGI_VERSION
                         Else
                           //if another winagi version is ever released,
                           //yeah, right; just consider this file invalid

                           //save wagfile as error string
                           strError = JustFileName(agGameFile)
                           //clear game variables
                           agGameDir = vbNullString
                           agGameFile = vbNullString
                           agGameID = vbNullString
                           agIntVersion = vbNullString
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
                         agGameDir = vbNullString
                         agGameFile = vbNullString
                         agGameID = vbNullString
                         agIntVersion = vbNullString
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
                       agGameDir = vbNullString
                       agGameFile = vbNullString
                       agGameID = vbNullString
                       agIntVersion = vbNullString
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
                       agGameEvents.RaiseEvent_LoadStatus lsPropertyFile, rtNone, 0, vbNullString

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

                       WriteGameSetting "General", "ResDir", agResDirName
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
                       agGameDir = vbNullString
                       agGameFile = vbNullString
                       agGameID = vbNullString
                       agIntVersion = vbNullString
                       agIsVersion3 = False

                       //reset collections & objects
                       Set agLogs = Nothing
                       Set agViews = Nothing
                       Set agPics = Nothing
                       Set agSnds = Nothing
                       Set agInvObj = Nothing
                       Set agVocabWords = Nothing
                       Set agLogs = New AGILogics
                       Set agViews = New AGIViews
                       Set agPics = New AGIPictures
                       Set agSnds = New AGISounds
                       Set agInvObj = New AGIInventoryObjects
                       Set agVocabWords = New AGIWordList

                       Set agGameProps = Nothing

                       //raise error
                       On Error GoTo 0: Err.Raise lngError, strErrSource, strError
                     }

                     //load vocabulary word list
                     agGameEvents.RaiseEvent_LoadStatus lsResources, rtWords, 0, vbNullString

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
                     agGameEvents.RaiseEvent_LoadStatus lsResources, rtObjects, 0, vbNullString

                     Set agInvObj = New AGIInventoryObjects
                     agInvObj.Init
                     agInvObj.Load agGameDir & "OBJECT"
                     //if there was an error,
                     If Err.Number<> 0 Then
                       //note the problem in the error log as a warning
                       RecordLogEvent leError, "An error occurred while loading OBJECT: " & Err.Description
                       //reset warning flag
                       blnWarnings = True
                     }


                     agGameEvents.RaiseEvent_LoadStatus lsFinalizing, rtNone, 0, vbNullString

                     //assign commands
                     AssignCommands

                     //and adust commands based on AGI version
                     CorrectCommands agIntVersion

                     //clear other game properties
                     agLastEdit = 0
                     agAuthor = vbNullString
                     agDescription = vbNullString
                     agGameVersion = vbNullString
                     agAbout = vbNullString
                     agPlatformType = 0
                     agPlatformFile = vbNullString
                     agPlatformOpts = vbNullString
                     agDOSExec = vbNullString

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
                     WriteGameSetting "General", "LastEdit", agLastEdit

                     //and save the wag file
                     SaveSettingList agGameProps

                     //if errors
                     If blnWarnings Then
                       On Error GoTo 0: Err.Raise vbObjectError + 636, strErrSource, LoadResString(636)
                     }
                   End Sub



                   Public Sub NewGame(NewID As String, ByVal NewVersion As String, NewGameDir As String, NewResDir As String, Optional ByVal TemplateDir As String = vbNullString)

                     //creates a new game in NewGameDir
                     //if a template directory is passed,
                     //use the resources from that template directory

                     Dim strDirData As String, intFile As Integer, strGameWAG As String
                     Dim i As Long, strTempDir As String, lngDirCount As Long
                     Dim blnWarnings As Boolean, strTmplResDir As String
                     Dim stlGlobals As StringList


                     On Error Resume Next

                     //if a game is already open,
                     If agGameLoaded Then
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
                       agGameDir = vbNullString
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
                         strTmplResDir = vbNullString
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
                       WriteGameSetting "General", "WinAGIVersion", WINAGI_VERSION


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
                       Set agInvObj = New AGIInventoryObjects
                       //use loaded argument to force load of new inventory list
                       agInvObj.Init True
                       agInvObj.Add "?", 0
                       agInvObj.Save

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
                     WriteGameSetting "General", "GameID", agGameID
                     WriteGameSetting "General", "Interpreter", agIntVersion
                     WriteGameSetting "General", "ResDir", agResDirName

                     //save palette colors
                     For i = 0 To 15
                       WriteGameSetting "Palette", "Color" & CStr(i), "&H" & PadHex(lngEGACol(i), 8)
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
                     Set agInvObj = Nothing
                     Set agVocabWords = Nothing
                     Set agLogs = New AGILogics
                     Set agViews = New AGIViews
                     Set agPics = New AGIPictures
                     Set agSnds = New AGISounds
                     Set agInvObj = New AGIInventoryObjects
                     Set agVocabWords = New AGIWordList


                     On Error GoTo 0: Err.Raise vbObjectError + 631, strErrSrc, Replace(LoadResString(631), ARG1, CStr(lngError) & ":" & strError)
                   End Sub

                   Public Property Get VocabularyWords() As AGIWordList


                     Set VocabularyWords = agVocabWords
                   End Property
                   Public Sub WriteProperty(ByVal Section As String, ByVal Key As String, ByVal NewVal As String, Optional Group As String = vbNullString, Optional ForceSave As Boolean = False)

                     // this procedure provides calling programs a way to write property
                     // values to the WAG file

                     // no validation of section or newval is done, so calling function
                     // needs to be careful

                     On Error GoTo ErrHandler


                     WriteGameSetting Section, Key, NewVal, Group

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
                     Set agInvObj = New AGIInventoryObjects
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


                     Set agInvObj = Nothing
                     Set agVocabWords = Nothing

                     Set agGameEvents = Nothing
                     Set agMainGame = Nothing
                   End Sub
                   */

        static internal string cDir(string strDirIn)
        {
            //this function ensures a trailing "\" is included on strDirIn
            if (strDirIn.Length != 0)
                if (strDirIn.Substring(strDirIn.Length - 1) != @"\")
                    return strDirIn + @"\";
                else
                    return strDirIn;
            else
                return strDirIn;
        }

        static internal string JustFileName(string strFullPathName)
        {
            //will extract just the file name by removing the path info
            string[] strSplitName;

            //On Error Resume Next

            strSplitName = strFullPathName.Split(@"\");
            if (strSplitName.Length == 1)
                return strFullPathName;
            else
                return strSplitName[strSplitName.Length - 1];
        }

        static internal uint CRC32(char[] DataIn)
        //static internal uint CRC32(byte[] DataIn)
        {
            //Public Function CRC32(DataIn() As Byte) As Long
            //calculates the CRC32 for an input array of bytes
            //a special table is necessary; the table is loaded
            //at program startup

            //the CRC is calculated according to the following equation:
            //
            //  CRC[i] = LSHR8(CRC[i-1]) Xor CRC32Table[(CRC[i-1] And &HFF) Xor DataIn[i])
            //
            //initial Value of CRC is &HFFFFFFFF; iterate the equation
            //for each byte of data; then end by XORing final result with &HFFFFFFFF


            int i;
            //initial Value
            uint result = 0xffffffff;

            //if table not loaded
            if (!CRC32Loaded)
                CRC32Setup();

            //iterate CRC equation
            for (i = 0; i < DataIn.Length; i++)
                result = (result >> 8) ^ CRC32Table[(result & 0xFF) ^ DataIn[i]];

            //xor to create final answer
            return result ^ 0xFFFFFFFF;
        }

        static internal void CRC32Setup()
        {
            //build the CRC table

            CRC32Table[0] = 0x0;
            CRC32Table[1] = 0x77073096;
            CRC32Table[2] = 0xEE0E612C;
            CRC32Table[3] = 0x990951BA;
            CRC32Table[4] = 0x76DC419;
            CRC32Table[5] = 0x706AF48F;
            CRC32Table[6] = 0xE963A535;
            CRC32Table[7] = 0x9E6495A3;
            CRC32Table[8] = 0xEDB8832;
            CRC32Table[9] = 0x79DCB8A4;
            CRC32Table[10] = 0xE0D5E91E;
            CRC32Table[11] = 0x97D2D988;
            CRC32Table[12] = 0x9B64C2B;
            CRC32Table[13] = 0x7EB17CBD;
            CRC32Table[14] = 0xE7B82D07;
            CRC32Table[15] = 0x90BF1D91;
            CRC32Table[16] = 0x1DB71064;
            CRC32Table[17] = 0x6AB020F2;
            CRC32Table[18] = 0xF3B97148;
            CRC32Table[19] = 0x84BE41DE;
            CRC32Table[20] = 0x1ADAD47D;
            CRC32Table[21] = 0x6DDDE4EB;
            CRC32Table[22] = 0xF4D4B551;
            CRC32Table[23] = 0x83D385C7;
            CRC32Table[24] = 0x136C9856;
            CRC32Table[25] = 0x646BA8C0;
            CRC32Table[26] = 0xFD62F97A;
            CRC32Table[27] = 0x8A65C9EC;
            CRC32Table[28] = 0x14015C4F;
            CRC32Table[29] = 0x63066CD9;
            CRC32Table[30] = 0xFA0F3D63;
            CRC32Table[31] = 0x8D080DF5;
            CRC32Table[32] = 0x3B6E20C8;
            CRC32Table[33] = 0x4C69105E;
            CRC32Table[34] = 0xD56041E4;
            CRC32Table[35] = 0xA2677172;
            CRC32Table[36] = 0x3C03E4D1;
            CRC32Table[37] = 0x4B04D447;
            CRC32Table[38] = 0xD20D85FD;
            CRC32Table[39] = 0xA50AB56B;
            CRC32Table[40] = 0x35B5A8FA;
            CRC32Table[41] = 0x42B2986C;
            CRC32Table[42] = 0xDBBBC9D6;
            CRC32Table[43] = 0xACBCF940;
            CRC32Table[44] = 0x32D86CE3;
            CRC32Table[45] = 0x45DF5C75;
            CRC32Table[46] = 0xDCD60DCF;
            CRC32Table[47] = 0xABD13D59;
            CRC32Table[48] = 0x26D930AC;
            CRC32Table[49] = 0x51DE003A;
            CRC32Table[50] = 0xC8D75180;
            CRC32Table[51] = 0xBFD06116;
            CRC32Table[52] = 0x21B4F4B5;
            CRC32Table[53] = 0x56B3C423;
            CRC32Table[54] = 0xCFBA9599;
            CRC32Table[55] = 0xB8BDA50F;
            CRC32Table[56] = 0x2802B89E;
            CRC32Table[57] = 0x5F058808;
            CRC32Table[58] = 0xC60CD9B2;
            CRC32Table[59] = 0xB10BE924;
            CRC32Table[60] = 0x2F6F7C87;
            CRC32Table[61] = 0x58684C11;
            CRC32Table[62] = 0xC1611DAB;
            CRC32Table[63] = 0xB6662D3D;
            CRC32Table[64] = 0x76DC4190;
            CRC32Table[65] = 0x1DB7106;
            CRC32Table[66] = 0x98D220BC;
            CRC32Table[67] = 0xEFD5102A;
            CRC32Table[68] = 0x71B18589;
            CRC32Table[69] = 0x6B6B51F;
            CRC32Table[70] = 0x9FBFE4A5;
            CRC32Table[71] = 0xE8B8D433;
            CRC32Table[72] = 0x7807C9A2;
            CRC32Table[73] = 0xF00F934;
            CRC32Table[74] = 0x9609A88E;
            CRC32Table[75] = 0xE10E9818;
            CRC32Table[76] = 0x7F6A0DBB;
            CRC32Table[77] = 0x86D3D2D;
            CRC32Table[78] = 0x91646C97;
            CRC32Table[79] = 0xE6635C01;
            CRC32Table[80] = 0x6B6B51F4;
            CRC32Table[81] = 0x1C6C6162;
            CRC32Table[82] = 0x856530D8;
            CRC32Table[83] = 0xF262004E;
            CRC32Table[84] = 0x6C0695ED;
            CRC32Table[85] = 0x1B01A57B;
            CRC32Table[86] = 0x8208F4C1;
            CRC32Table[87] = 0xF50FC457;
            CRC32Table[88] = 0x65B0D9C6;
            CRC32Table[89] = 0x12B7E950;
            CRC32Table[90] = 0x8BBEB8EA;
            CRC32Table[91] = 0xFCB9887C;
            CRC32Table[92] = 0x62DD1DDF;
            CRC32Table[93] = 0x15DA2D49;
            CRC32Table[94] = 0x8CD37CF3;
            CRC32Table[95] = 0xFBD44C65;
            CRC32Table[96] = 0x4DB26158;
            CRC32Table[97] = 0x3AB551CE;
            CRC32Table[98] = 0xA3BC0074;
            CRC32Table[99] = 0xD4BB30E2;
            CRC32Table[100] = 0x4ADFA541;
            CRC32Table[101] = 0x3DD895D7;
            CRC32Table[102] = 0xA4D1C46D;
            CRC32Table[103] = 0xD3D6F4FB;
            CRC32Table[104] = 0x4369E96A;
            CRC32Table[105] = 0x346ED9FC;
            CRC32Table[106] = 0xAD678846;
            CRC32Table[107] = 0xDA60B8D0;
            CRC32Table[108] = 0x44042D73;
            CRC32Table[109] = 0x33031DE5;
            CRC32Table[110] = 0xAA0A4C5F;
            CRC32Table[111] = 0xDD0D7CC9;
            CRC32Table[112] = 0x5005713C;
            CRC32Table[113] = 0x270241AA;
            CRC32Table[114] = 0xBE0B1010;
            CRC32Table[115] = 0xC90C2086;
            CRC32Table[116] = 0x5768B525;
            CRC32Table[117] = 0x206F85B3;
            CRC32Table[118] = 0xB966D409;
            CRC32Table[119] = 0xCE61E49F;
            CRC32Table[120] = 0x5EDEF90E;
            CRC32Table[121] = 0x29D9C998;
            CRC32Table[122] = 0xB0D09822;
            CRC32Table[123] = 0xC7D7A8B4;
            CRC32Table[124] = 0x59B33D17;
            CRC32Table[125] = 0x2EB40D81;
            CRC32Table[126] = 0xB7BD5C3B;
            CRC32Table[127] = 0xC0BA6CAD;
            CRC32Table[128] = 0xEDB88320;
            CRC32Table[129] = 0x9ABFB3B6;
            CRC32Table[130] = 0x3B6E20C;
            CRC32Table[131] = 0x74B1D29A;
            CRC32Table[132] = 0xEAD54739;
            CRC32Table[133] = 0x9DD277AF;
            CRC32Table[134] = 0x4DB2615;
            CRC32Table[135] = 0x73DC1683;
            CRC32Table[136] = 0xE3630B12;
            CRC32Table[137] = 0x94643B84;
            CRC32Table[138] = 0xD6D6A3E;
            CRC32Table[139] = 0x7A6A5AA8;
            CRC32Table[140] = 0xE40ECF0B;
            CRC32Table[141] = 0x9309FF9D;
            CRC32Table[142] = 0xA00AE27;
            CRC32Table[143] = 0x7D079EB1;
            CRC32Table[144] = 0xF00F9344;
            CRC32Table[145] = 0x8708A3D2;
            CRC32Table[146] = 0x1E01F268;
            CRC32Table[147] = 0x6906C2FE;
            CRC32Table[148] = 0xF762575D;
            CRC32Table[149] = 0x806567CB;
            CRC32Table[150] = 0x196C3671;
            CRC32Table[151] = 0x6E6B06E7;
            CRC32Table[152] = 0xFED41B76;
            CRC32Table[153] = 0x89D32BE0;
            CRC32Table[154] = 0x10DA7A5A;
            CRC32Table[155] = 0x67DD4ACC;
            CRC32Table[156] = 0xF9B9DF6F;
            CRC32Table[157] = 0x8EBEEFF9;
            CRC32Table[158] = 0x17B7BE43;
            CRC32Table[159] = 0x60B08ED5;
            CRC32Table[160] = 0xD6D6A3E8;
            CRC32Table[161] = 0xA1D1937E;
            CRC32Table[162] = 0x38D8C2C4;
            CRC32Table[163] = 0x4FDFF252;
            CRC32Table[164] = 0xD1BB67F1;
            CRC32Table[165] = 0xA6BC5767;
            CRC32Table[166] = 0x3FB506DD;
            CRC32Table[167] = 0x48B2364B;
            CRC32Table[168] = 0xD80D2BDA;
            CRC32Table[169] = 0xAF0A1B4C;
            CRC32Table[170] = 0x36034AF6;
            CRC32Table[171] = 0x41047A60;
            CRC32Table[172] = 0xDF60EFC3;
            CRC32Table[173] = 0xA867DF55;
            CRC32Table[174] = 0x316E8EEF;
            CRC32Table[175] = 0x4669BE79;
            CRC32Table[176] = 0xCB61B38C;
            CRC32Table[177] = 0xBC66831A;
            CRC32Table[178] = 0x256FD2A0;
            CRC32Table[179] = 0x5268E236;
            CRC32Table[180] = 0xCC0C7795;
            CRC32Table[181] = 0xBB0B4703;
            CRC32Table[182] = 0x220216B9;
            CRC32Table[183] = 0x5505262F;
            CRC32Table[184] = 0xC5BA3BBE;
            CRC32Table[185] = 0xB2BD0B28;
            CRC32Table[186] = 0x2BB45A92;
            CRC32Table[187] = 0x5CB36A04;
            CRC32Table[188] = 0xC2D7FFA7;
            CRC32Table[189] = 0xB5D0CF31;
            CRC32Table[190] = 0x2CD99E8B;
            CRC32Table[191] = 0x5BDEAE1D;
            CRC32Table[192] = 0x9B64C2B0;
            CRC32Table[193] = 0xEC63F226;
            CRC32Table[194] = 0x756AA39C;
            CRC32Table[195] = 0x26D930A;
            CRC32Table[196] = 0x9C0906A9;
            CRC32Table[197] = 0xEB0E363F;
            CRC32Table[198] = 0x72076785;
            CRC32Table[199] = 0x5005713;
            CRC32Table[200] = 0x95BF4A82;
            CRC32Table[201] = 0xE2B87A14;
            CRC32Table[202] = 0x7BB12BAE;
            CRC32Table[203] = 0xCB61B38;
            CRC32Table[204] = 0x92D28E9B;
            CRC32Table[205] = 0xE5D5BE0D;
            CRC32Table[206] = 0x7CDCEFB7;
            CRC32Table[207] = 0xBDBDF21;
            CRC32Table[208] = 0x86D3D2D4;
            CRC32Table[209] = 0xF1D4E242;
            CRC32Table[210] = 0x68DDB3F8;
            CRC32Table[211] = 0x1FDA836E;
            CRC32Table[212] = 0x81BE16CD;
            CRC32Table[213] = 0xF6B9265B;
            CRC32Table[214] = 0x6FB077E1;
            CRC32Table[215] = 0x18B74777;
            CRC32Table[216] = 0x88085AE6;
            CRC32Table[217] = 0xFF0F6A70;
            CRC32Table[218] = 0x66063BCA;
            CRC32Table[219] = 0x11010B5C;
            CRC32Table[220] = 0x8F659EFF;
            CRC32Table[221] = 0xF862AE69;
            CRC32Table[222] = 0x616BFFD3;
            CRC32Table[223] = 0x166CCF45;
            CRC32Table[224] = 0xA00AE278;
            CRC32Table[225] = 0xD70DD2EE;
            CRC32Table[226] = 0x4E048354;
            CRC32Table[227] = 0x3903B3C2;
            CRC32Table[228] = 0xA7672661;
            CRC32Table[229] = 0xD06016F7;
            CRC32Table[230] = 0x4969474D;
            CRC32Table[231] = 0x3E6E77DB;
            CRC32Table[232] = 0xAED16A4A;
            CRC32Table[233] = 0xD9D65ADC;
            CRC32Table[234] = 0x40DF0B66;
            CRC32Table[235] = 0x37D83BF0;
            CRC32Table[236] = 0xA9BCAE53;
            CRC32Table[237] = 0xDEBB9EC5;
            CRC32Table[238] = 0x47B2CF7F;
            CRC32Table[239] = 0x30B5FFE9;
            CRC32Table[240] = 0xBDBDF21C;
            CRC32Table[241] = 0xCABAC28A;
            CRC32Table[242] = 0x53B39330;
            CRC32Table[243] = 0x24B4A3A6;
            CRC32Table[244] = 0xBAD03605;
            CRC32Table[245] = 0xCDD70693;
            CRC32Table[246] = 0x54DE5729;
            CRC32Table[247] = 0x23D967BF;
            CRC32Table[248] = 0xB3667A2E;
            CRC32Table[249] = 0xC4614AB8;
            CRC32Table[250] = 0x5D681B02;
            CRC32Table[251] = 0x2A6F2B94;
            CRC32Table[252] = 0xB40BBE37;
            CRC32Table[253] = 0xC30C8EA1;
            CRC32Table[254] = 0x5A05DF1B;
            CRC32Table[255] = 0x2D02EF8D;

            //set flag
            CRC32Loaded = true;
        }

        static internal string StripComments(string strLine, string strComment, bool NoTrim)
        {
            throw new NotImplementedException();
            return "";
            //Public Function StripComments(ByVal strLine As String, ByRef strComment As String, Optional ByVal NoTrim As Boolean = False) As String

            //  'strips off any comments on the line
            //  'if NoTrim is false, the string is also
            //  'stripped of any blank space

            //  'if there is a comment, it is passed back in the strComment argument


            //  Dim lngPos As Long, intROLIgnore As Integer, blnDblSlash As Boolean
            //  Dim blnInQuotes As Boolean, blnSlash As Boolean


            //  On Error GoTo ErrHandler

            //  'reset rol ignore
            //  intROLIgnore = 0

            //  'reset comment start & char ptr, and inquotes
            //  lngPos = 0
            //  blnInQuotes = False

            //  'assume no comment
            //  strComment = ""

            //  'if this line is not empty,
            //  If LenB(strLine) <> 0 Then
            //    Do Until lngPos >= Len(strLine)
            //      'get next character from string
            //      lngPos = lngPos + 1
            //      'if NOT inside a quotation,
            //      If Not blnInQuotes Then
            //        'check for comment characters at this position
            //        If(Mid$(strLine, lngPos, 2) = "//") Then
            //         intROLIgnore = lngPos + 1
            //          blnDblSlash = True
            //          Exit Do
            //        ElseIf(Mid$(strLine, lngPos, 1) = "[") Then
            //         intROLIgnore = lngPos
            //          Exit Do
            //        End If
            //        ' slash codes never occur outside quotes
            //        blnSlash = False
            //        'if this character is a quote mark, it starts a string
            //        blnInQuotes = (AscW(Mid$(strLine, lngPos)) = 34)
            //      Else
            //        'if last character was a slash, ignore this character
            //        'because it's part of a slash code
            //        If blnSlash Then
            //          'always reset  the slash
            //          blnSlash = False
            //        Else
            //          'check for slash or quote mark
            //          Select Case AscW(Mid$(strLine, lngPos))
            //          Case 34 'quote mark
            //            'a quote marks end of string
            //            blnInQuotes = False
            //          Case 92 'slash
            //            blnSlash = True
            //          End Select
            //        End If
            //      End If
            //    Loop
            //    'if any part of line should be ignored,
            //    If intROLIgnore > 0 Then
            //      'save the comment
            //      strComment = Trim(Right(strLine, Len(strLine) - intROLIgnore))
            //      'strip off comment
            //      If blnDblSlash Then
            //        strLine = Left$(strLine, intROLIgnore - 2)
            //      Else
            //        strLine = Left$(strLine, intROLIgnore - 1)
            //      End If
            //    End If
            //  End If


            //  If Not NoTrim Then
            //    'return the line, trimmed
            //    StripComments = Trim$(strLine)
            //  Else
            //    'return the string with just the comment removed
            //    StripComments = strLine
            //  End If
            //Exit Function

            //ErrHandler:
            //  '*'Debug.Assert False
            //  Resume Next
            //End Function
        }

        /// <summary>
        /// Extension method that works out if a string is numeric or not
        /// </summary>
        /// <param name="str">string that may be a number</param>
        /// <returns>true if numeric, false if not</returns>
        static internal bool IsNumeric(this String str)
        {
            double myNum = 0;
            if (Double.TryParse(str, out myNum))
            {
                return true;
            }
            return false;
        }

        static internal int ValidateDefName(string DefName)
        {
            throw new NotImplementedException();
            return 0;
            //    Public Function ValidateDefName(DefName As String) As Long
            //  'validates that DefValue is a valid define Value
            //  '
            //  'returns 0 if successful
            //  '
            //  'returns an error code on failure:
            //  '1 = no name
            //  '2 = name is numeric
            //  '3 = name is command
            //  '4 = name is test command
            //  '5 = name is a compiler keyword
            //  '6 = name is an argument marker
            //  '7 = name is already globally defined
            //  '8 = name is reserved variable name
            //  '9 = name is reserved flag name
            //  '10 = name is reserved number constant
            //  '11 = name is reserved object constant
            //  '12 = name is reserved message constant
            //  '13 = name contains improper character

            //  Dim i As Long, j As Long


            //  On Error GoTo ErrHandler

            //  'if no name, or no Value,
            //  If LenB(DefName) = 0 Then
            //    ValidateDefName = 1
            //    Exit Function
            //  End If

            //  'name cant be numeric
            //  If IsNumeric(DefName) Then
            //    ValidateDefName = 2
            //    Exit Function
            //  End If

            //  'check against regular commands
            //  For i = 0 To UBound(agCmds)
            //    If DefName = agCmds(i).Name Then
            //      ValidateDefName = 3
            //      Exit Function
            //    End If
            //  Next i

            //  'check against test commands
            //  For i = 0 To UBound(agTestCmds)
            //    If DefName = agTestCmds(i).Name Then
            //      ValidateDefName = 4
            //      Exit Function
            //    End If
            //  Next i

            //  'check against keywords
            //  If(DefName = "if") Or(DefName = "else") Or(DefName = "goto") Or(DefName = "then") Then
            //ValidateDefName = 5
            //    Exit Function
            //  End If

            //  'check against variable/flag/controller/string/message names
            //  Select Case AscW(LCase$(DefName))
            //  '     v    f    m    o    i    s    w    c
            //  Case 118, 102, 109, 111, 105, 115, 119, 99
            //    If IsNumeric(Right$(DefName, Len(DefName) - 1)) Then
            //      ValidateDefName = 6
            //      Exit Function
            //    End If
            //  End Select

            //  'check against current globals
            //  For i = 0 To agGlobalCount - 1
            //    If DefName = agGlobal(i).Name Then
            //      ValidateDefName = 7
            //      'dont exit so Value can be validated
            //    End If
            //  Next i


            //  If agUseRes Then
            //    'check against reserved defines:
            //    'check against reserved variables
            //    For i = 0 To 26
            //      If DefName = agResVar(i).Name Then
            //        ValidateDefName = 8
            //        Exit Function
            //      End If
            //    Next i
            //    'check against reserved flags
            //    For i = 0 To 17
            //      If DefName = agResFlag(i).Name Then
            //        ValidateDefName = 9
            //        Exit Function
            //      End If
            //    Next i
            //    'check against other reserved constants
            //    For i = 0 To 4
            //      If DefName = agEdgeCodes(i).Name Then
            //        ValidateDefName = 10
            //        Exit Function
            //      End If
            //    Next i
            //    For i = 0 To 8
            //      If DefName = agEgoDir(i).Name Then
            //        ValidateDefName = 10
            //        Exit Function
            //      End If
            //    Next i
            //    For i = 0 To 4
            //      If DefName = agVideoMode(i).Name Then
            //        ValidateDefName = 10
            //        Exit Function
            //      End If
            //    Next i

            //    'check against other reserved defines
            //    If DefName = agResDef(4).Name Then
            //      ValidateDefName = 10
            //      Exit Function
            //    End If


            //    If DefName = agResDef(0).Name Then
            //      ValidateDefName = 11
            //      Exit Function
            //    End If
            //    If DefName = agResDef(5).Name Then
            //      ValidateDefName = 11
            //      Exit Function
            //    End If


            //    For i = 1 To 3
            //      If DefName = agResDef(i).Name Then
            //        ValidateDefName = 12
            //        Exit Function
            //      End If
            //    Next i
            //  End If

            //  'check name against improper character list
            //  For i = 1 To Len(DefName)
            //    Select Case AscW(Mid$(DefName, i))
            //    'control characters (1-31), space, and these characters: !"#$%&'() *+,-/:;<=>?@[\]^`{|}~
            //Case 1 To 45, 47, 58 To 64, 91 To 94, 96, Is >= 123
            //      ValidateDefName = 13
            //      Exit Function
            //    End Select
            //  Next i
            //Exit Function

            //ErrHandler:
            //  'should never get an error
            //  '*'Debug.Assert False
            //End Function
        }
        static internal int ValidateDefValue(TDefine TestDefine)
        {
            //validates that TestDefine.Value is a valid define Value
            //
            //returns 0 if successful
            //
            //returns an error code on failure:
            //1 = no Value
            //2 = Value is an invalid argument marker (not used anymore]
            //3 = Value contains an invalid argument Value
            //4 = Value is not a string, number or argument marker
            //5 = Value is already defined by a reserved name
            //6 = Value is already defined by a global name

            string strVal;
            int intVal;

            //On Error GoTo ErrHandler

            if (TestDefine.Value.Length == 0)
                return 1;

            //values must be a variable/flag/etc, string, or a number
            if (!IsNumeric(TestDefine.Value))
            {
                //if Value is an argument marker
                switch ((int)TestDefine.Value.ToLower().ToCharArray()[0])
                {
                    //     v    f    m    o    i    s    w    c
                    //Case 118, 102, 109, 111, 105, 115, 119, 99
                    case 118:
                    case 102:
                    case 109:
                    case 111:
                    case 105:
                    case 115:
                    case 119:
                    case 99:
                        //if rest of Value is numeric,
                        strVal = TestDefine.Value.Substring(0, TestDefine.Value.Length - 1);
                        if (IsNumeric(strVal))
                        {
                            //if Value is not between 0-255
                            intVal = int.Parse(strVal);
                            if (intVal < 0 || intVal > 255)
                                return 3;

                            //check defined globals
                            for (int i = 0; i <= AGICommands.agGlobalCount - 1; i++)
                            {
                                //if this define has same Value
                                if (AGICommands.agGlobal[i].Value == TestDefine.Value)
                                    return 6;
                            }

                            //verify that the Value is not already assigned
                            switch ((int)TestDefine.Value.ToLower().ToCharArray()[0])
                            {
                                case 102: //flag
                                    TestDefine.Type = ArgTypeEnum.atFlag;
                                    if (AGICommands.agUseRes)
                                        //if already defined as a reserved flag
                                        if (intVal <= 15)
                                            return 5;
                                    break;

                                case 118: //variable
                                    TestDefine.Type = ArgTypeEnum.atVar;
                                    if (AGICommands.agUseRes)
                                        //if already defined as a reserved variable
                                        if (intVal <= 26)
                                            return 5;
                                    break;

                                case 109: //message
                                    TestDefine.Type = ArgTypeEnum.atMsg;
                                    break;

                                case 111: //screen object
                                    TestDefine.Type = ArgTypeEnum.atSObj;
                                    if (AGICommands.agUseRes)
                                        //can't be ego
                                        if (TestDefine.Value == "o0")
                                            return 5;
                                    break;

                                case 105: //inv object
                                    TestDefine.Type = ArgTypeEnum.atIObj;
                                    break;

                                case 115: //string
                                    TestDefine.Type = ArgTypeEnum.atStr;
                                    break;

                                case 119: //word
                                    TestDefine.Type = ArgTypeEnum.atWord;
                                    break;

                                case 99: //controller
                                         //controllers limited to 0-49
                                    if (intVal < 0 || intVal > 255)
                                        return 3;
                                    TestDefine.Type = ArgTypeEnum.atCtrl;
                                    break;
                                    //Value is ok
                                    return 0;
                                default:
                                    break;
                            }
                        }
                        break;
                }
                //non-numeric, non-marker and most likely a string
                TestDefine.Type = ArgTypeEnum.atDefStr;

                //check Value for string delimiters in Value
                if (TestDefine.Value.Substring(0, 1) != "\"" || TestDefine.Value.Substring(TestDefine.Value.Length - 1, 1) != "\"")
                    return 4;
                else
                    return 0;
            }
            else
            {
                // numeric
                TestDefine.Type = ArgTypeEnum.atNum;
                return 0;
            }

            //ErrHandler:
            //          strError = Err.Description
            //strErrSrc = Err.Source
            //lngError = Err.Number

            //On Error GoTo 0: Err.Raise vbObjectError +660, strErrSrc, Replace(LoadResString(660), ARG1, CStr(lngError) & ":" & strError)
        }
    }
}
