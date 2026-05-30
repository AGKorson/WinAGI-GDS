using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.EventType;
namespace WinAGI.Editor {

    internal class WAGConsole {
        private enum ConsoleMode {
            None,
            Import,
            CompileGame,
            Rebuild,
            CompileChanged,
            CompileLogic,
            ExportAll,
            ExportResource,
            AddResource,
        }

        public static bool Verbose { get; set; } = true;

        private static ConsoleMode mode;
        private static int ErrCount = 0, WarnCount = 0, CompCount = 0;
        private static int logErr = 0, logWarn = 0;
        public static void InitConsole() {
            if (Debugger.IsAttached) {
                Debug.WriteLine("");
                Debug.WriteLine("DEBUGGING CONSOLE MODE");
                Debug.WriteLine("");
            }
            else {
                [System.Runtime.InteropServices.DllImport("kernel32.dll")]
                static extern bool AllocConsole();
                AllocConsole();
            }
        }

        public static void RunConsoleMode(string[] args) {
            // runs WinAGI in console mode (no GUI)
            InitConsole();

            //  Usage: --console <dir|gamefile> actionswitches [/q]
            //    <dir>       directory containing existing VOL and DIR files
            //                only valid with /i switch; can be blank
            //    <gamefile>  winagi game property file (*.wag); gamefile must be
            //                in its game directory
            //
            //   valid action switches:
            //     /i  imports existing game VOL and DIR files in <dir>
            //         and creates new wag file with default GameID
            //         optional parameters:
            //         /C:# set code page (default is 437)
            //         /E:<ext> set source resource file extension
            //         /R:<resdir> set source resource directory
            //         /S use sierra syntax (default is Fan syntax)
            //         /nG don't include global variables in logic source files
            //         /nR don't include reserved words in logic source files
            //         /nI don't include resource IDs in logic source files
            //
            //     /c  does a full game compile of <gamefile> in its directory
            //         replacing existing VOL and DIR files
            //
            //     /r  rebuilds VOL files, without recompiling logics
            //
            //     /d  compiles all changed logics, but doesn't rebuild VOL files
            //         rebuilt logics are put in first available VOL space
            //
            //     /e  exports all resources to individual files; resources will
            //         be exported to the game's source directory
            //
            //     /e:L# | /e:P# | /e:V# | /e:S#
            //        exports a single resource of the specified type and number
            //        to the game's resource directory
            //
            //     /a:L# | /a:C# | /a:P# | /a:V# | /a:S# <resfile>
            //        adds the resource in <resfile> to the the game as the
            //        specified type and number (use L for a compiled logic
            //        resource; use C for a source code file)
            //
            //     /quiet or /q   suppresses all non-essential status messages
            //                    must be last action switch in the command
            //     /help or /?    displays help text explaining how to use the
            //                    console mode

            //  let game engine know we're in console mode
            Base.ConsoleMode = true;

            if (args.Length < 1 ||
                args[0].Equals("/help", StringComparison.OrdinalIgnoreCase) ||
                args[0] == "/?") {
                // blank command line is same as asking for help
                ShowHelp();
            }
            else {
                // some game settings needed for console mode - for sure
                //  error levels, ignored warnings (should these be switch settings?)
                if (!ReadConsoleSettings()) {
                    // problem with settings
                    return;
                }
                // setup info grid to manage error/warning list
                infoGridTable.Columns.Add("Type", typeof(string)); // EventType, hidden
                infoGridTable.Columns.Add("ResType", typeof(string));  // AGIResType, hidden
                infoGridTable.Columns.Add("Code", typeof(string));
                infoGridTable.Columns.Add("Description", typeof(string));
                infoGridTable.Columns.Add("ResNum", typeof(int));
                infoGridTable.Columns.Add("Line", typeof(int));
                infoGridTable.Columns.Add("Module", typeof(string));
                infoGridTable.Columns.Add("Filename", typeof(string));

                // if last arg is quiet
                if (args.Length > 1 &&
                    args[^1].Equals("/q", StringComparison.OrdinalIgnoreCase) ||
                    args[^1].Equals("/quiet", StringComparison.OrdinalIgnoreCase)) {
                    Verbose = false;
                    Array.Resize(ref args, args.Length - 1);
                }

                // there should be at least two args left
                // unless importing from current dir
                if (args.Length < 2) {
                    if (args[0] == "/i") {
                        Array.Resize(ref args, 2);
                        args[0] = Directory.GetCurrentDirectory();
                        args[1] = "/i";
                    }
                    else {
                        WriteLine("invalid command line arguments");
                        WriteLine();
                        ShowHelp();
                        return;
                    }
                }

                // arg0 should be a directory or gamefile
                // strip quotes if necessary
                if (args[0].Length > 1 && args[0][0] == '"' && args[0][^1] == '"') {
                    args[0] = args[0][1..^1].Trim();
                }
                if (!Path.IsPathFullyQualified(args[0])) {
                    args[0] = Path.GetFullPath(args[0]);
                }
                // if next arg is /i, then import a game
                if (args[1] == "/i") {
                    mode = ConsoleMode.Import;
                    if (!Directory.Exists(args[0])) {
                        WriteLine("The specified directory '" + args[0] + "' does not exist.");
                        return;
                    }
                    // check for optional parameters
                    int codepage = 437; // default code page
                    string resdir = WinAGISettings.DefResDir.Value;
                    string ext = WinAGISettings.DefaultExt.Value;
                    bool sierraSyntax = false;
                    bool useG = true; // WinAGISettings.DefIncludeGlobals.Value;
                    bool useR = true; // WinAGISettings.DefIncludeReserved.Value;
                    bool useI = true; // WinAGISettings.DefIncludeIDs.Value;
                    for (int i = 2; i < args.Length; i++) {
                        if (args[i].StartsWith("/C:", StringComparison.OrdinalIgnoreCase)) {
                            // code page
                            if (int.TryParse(args[i][3..], out int cp)) {
                                codepage = cp;
                            }
                        }
                        else if (args[i].StartsWith("/E:", StringComparison.OrdinalIgnoreCase)) {
                            // source resource file extension
                            ext = args[i][3..];
                        }
                        else if (args[i].StartsWith("/R:", StringComparison.OrdinalIgnoreCase)) {
                            // source resource directory
                            resdir = args[i][3..];
                        }
                        else if (args[i] == "/S") {
                            // sierra syntax
                            sierraSyntax = true;
                        }
                        else if (args[i] == "/nG") {
                            // don't include global variables in logic source files
                            useG = false;
                        }
                        else if (args[i] == "/nR") {
                            // don't include reserved words in logic source files
                            useR = false;
                        }
                        else if (args[i] == "/nI") {
                            // don't include resource IDs in logic source files
                            useI = false;
                        }
                    }
                    // check for existing resource directory, so output message can be customized
                    bool createDir = !Directory.Exists(args[0] + resdir);

                    // pass game info and template info
                    GameParams gameparams = new() {
                        Mode = OpenGameMode.Directory,
                        GameDir = args[0],
                        SrcResDirName = resdir,
                        SrcExt = ext,
                        TemplateDir = "",
                        SierraSyntax = sierraSyntax,
                        CodePage = codepage,
                        Failed = false,
                        Error = null,
                        Warnings = false
                    };
                    if (gameparams.SierraSyntax) {
                        gameparams.IncludeGlobals = false;
                        gameparams.IncludeIDs = false;
                        gameparams.IncludeReserved = false;
                    }
                    else {
                        gameparams.IncludeGlobals = useG;
                        gameparams.IncludeIDs = useI;
                        gameparams.IncludeReserved = useR;
                    }
                    // IMPORT the game in this directory
                    if (!ConsoleImportGame(gameparams, createDir)) {
                        return;
                    }
                    // add error list file
                    WriteErrorFile();
                    EditGame.CloseGame();
                }
                else {
                    // expand to full directory name
                    if (!File.Exists(args[0])) {
                        WriteLine("The specified game file '" + args[0] + "' does not exist.");
                        return;
                    }
                    if (!ConsoleOpenGame(args[0])) {
                        return;
                    }
                    // now process the switches
                    for (int i = 1; i < args.Length; i++) {
                        if (args[i] == "/c") {
                            // full compile
                            ConsoleCompileAll();
                        }
                        else if (args[i] == "/r") {
                            ConsoleRebuild();
                        }
                        else if (args[i] == "/d") {
                            ConsoleCompileChanged();
                        }
                        else if (args[i] == "/e") {
                            ConsoleExportAll();
                        }
                        else if (args[i].Left(2) == "/e") {
                            ConsoleExportResource(args[i][2..]);
                        }
                        else if (args[i].Left(2) == "/a") {
                            ConsoleAddResource(args[i][2..], args, ref i);
                        }
                        // ignore unrecognized switches
                    }
                    // all done
                    EditGame.CloseGame();
                }
            }
        }

        private static void WriteErrorFile() {
            if (infoGridTable.Rows.Count > 0) {
                string output = "InfoType" + '\t' + "ResType" + '\t' +
                    "Code" + '\t' + "Description" + '\t' + "ResNum" + '\t' +
                    "Line" + '\t' + "Module" + '\t' + "Filename" + "\r\n";
                foreach (DataRow row in infoGridTable.Rows) {
                    output += row[0].ToString() + '\t' + row[1].ToString() + '\t';
                    output += row[2].ToString() + '\t' + row[3].ToString() + '\t';
                    output += row[4].ToString() + '\t' + row[5].ToString() + '\t';
                    output += row[6].ToString() + '\t' + row[7].ToString() + "\r\n";
                }
                // write to file
                try {
                    File.WriteAllText(Path.Combine(EditGame.GameDir, "errlog.txt"), output);
                }
                catch {
                    // ignore errors writing error log
                }
            }
        }

        private static bool ConsoleImportGame(GameParams gameparams, bool createResDir) {
            string msgtext;
            WriteLineVerbose("importing " + gameparams.GameDir);

            try {
                EditGame = new AGIGame(gameparams);
                // step through all logics and update warning/error list
                foreach (Logic logic in EditGame.Logics) {
                    RefreshLogicWarnings(logic.Number, false);
                }
                RefreshModWarnings(false);
                // now save the save game file
                EditGame.SaveProperties();
                // done
                if (Verbose) {
                    WriteLine("game file '" + EditGame.GameID + ".wag' has been created.");
                    if (EditGame.SrcResDirName == gameparams.SrcResDirName && createResDir) {
                        msgtext = "the subdirectory '" + EditGame.SrcResDirName + "' has been created ";
                    }
                    else if (EditGame.SrcResDirName.Length > 0) {
                        msgtext = "the existing subdirectory '" + EditGame.SrcResDirName + "' will be used ";
                    }
                    else {
                        msgtext = "the game directory will be used ";
                    }
                    msgtext += "to store logic source files and exported resources.";
                    // warn user that resource dir set to default
                    WriteLine(msgtext);
                    // does the game have an Amiga OBJECT file?
                    // very rare, but we check for it anyway
                    if (EditGame.InvObjects.AmigaOBJ) {
                        WriteLine("the OBJECT file for this game is formatted for the Amiga");
                    }
                    if (EditGame.LoadWarnings) {
                        WriteLine("Some errors and/or anomalies in resource data were encountered.");
                        WriteLine("See errlog.txt in the game directory for details.");
                    }
                }
                WriteLine("SUCCESS");
            }
            catch (WinAGIException wex) {
                // errors always causes failure to load
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                switch (wex.HResult & 0xffff) {
                case 504: // invalid game directory
                case 505: // invalid DIR file
                case 539: // game file '%1' is readonly
                case 541: // File access error when reading file - number: message
                case 546: // Unable to create wag file (%1)
                    WriteLine("Error " + (wex.HResult & 0xffff) + ": " + wex.Message);
                    break;
                default:
                    // shouldn't ever happen...
                    Debug.Assert(false);
                    WriteLine("uError " + (wex.HResult & 0xffff) + ": " + wex.Message);
                    break;
                }
                WriteLine("FAIL");
                return false;
            }
            catch (FileNotFoundException fex) {
                // errors always causes failure to load
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                // file not found:
                WriteLine("Error 508: " + EngineResourceByNum(508).Replace(
                    ARG1, Path.GetFileName((string)fex.Data["missingfile"])));
                WriteLine("FAIL");
                return false;
            }
            catch (Exception ex) {
                // errors always causes failure to load
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                // unknown error
                if (Verbose) {
                    WriteLine("Error 507: " + EngineResourceByNum(507).Replace(
                        ARG1, ex.HResult.ToString("x8")).Replace(
                        ARG2, ex.Message).Replace(
                        ARG3, ex.StackTrace));
                }
                else {
                    WriteLine("Error 507: UNHANDLED ERROR " +
                        ex.HResult.ToString("x8") + " " + ex.Message);
                }
                WriteLine("FAIL");
                return false;
            }
            return true;
        }

        private static bool ConsoleOpenGame(string gamefile) {

            if (Verbose) {
                WriteLine("opening " + gamefile);
            }
            try {
                GameParams gameparams = new() {
                    Mode = OpenGameMode.File,
                    GameFile = gamefile,
                };
                EditGame = new AGIGame(gameparams);
            }
            catch (WinAGIException wex) {
                // errors always causes failure to load
                WriteLine("FAIL");
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                switch (wex.HResult & 0xffff) {
                case 504: // invalid game directory
                case 505: // invalid DIR file
                case 529: // invalid game file
                case 530: // invalid WAG file version
                case 537: // missing GameID in WAG file
                case 538: // invalid IntVersion in WAG file
                case 539: // game file '%1' is readonly
                case 540: // file access error while opening WAG file
                case 541: // File access error when reading file - number: message
                case 555: // invalid data in WAG file
                    WriteLine("Error " + (wex.HResult & 0xffff) + ": " + wex.Message);
                    break;
                default:
                    // shouldn't ever happen...
                    WriteLine("uError " + (wex.HResult & 0xffff) + ": " + wex.Message);
                    Debug.Assert(false);
                    break;
                }
                return false;
            }
            catch (FileNotFoundException fex) {
                // errors always causes failure to load
                WriteLine("FAIL");
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                // file not found:
                WriteLine("Error 508: " + EngineResourceByNum(508).Replace(
                    ARG1, Path.GetFileName((string)fex.Data["missingfile"])));
                return false;
            }
            catch (Exception ex) {
                // errors always causes failure to load
                WriteLine("FAIL");
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                // unknown error
                if (Verbose) {
                    WriteLine("Error 507: " + EngineResourceByNum(507).Replace(
                        ARG1, ex.HResult.ToString("x8")).Replace(
                        ARG2, ex.Message).Replace(
                        ARG3, ex.StackTrace));
                }
                else {
                    WriteLine("Error 507: UNHANDLED ERROR " +
                        ex.HResult.ToString("x8") + " " + ex.Message);
                }
                return false;
            }
            WriteLineVerbose("game '" + EditGame.GameID + "' opened successfully.");
            return true;
        }

        private static void ConsoleCompileAll() {
            mode = ConsoleMode.CompileGame;
            WriteLineVerbose("Compiling '" + EditGame.GameID + "'");

            CompileStatus results = EditGame.Compile(false);
            Debug.Assert(!Compiling);
            WriteErrorFile();

            if (WarnCount > 0) {
                WriteLineVerbose(WarnCount + " warnings were generated during compilation.");
            }
            if (ErrCount > 0) {
                WriteLineVerbose(ErrCount + " errors were generated during compilation.");
            }
            if (WarnCount > 0 || ErrCount > 0) {
                WriteLineVerbose("See errlog.txt in the game directory for details.");
            }
            if (results == CompileStatus.OK) {
                WriteLineVerbose(EditGame.GameID + " compilation complete.");
                WriteLine("SUCCESS");
            }
            else {
                // error messages already written in event handler
                WriteLine("FAIL");
            }
        }

        private static void ConsoleRebuild() {
            mode = ConsoleMode.Rebuild;
            WriteLineVerbose("Rebuilding '" + EditGame.GameID + "'");

            CompileStatus results = EditGame.Compile(true);
            Debug.Assert(!Compiling);
            WriteErrorFile();

            if (WarnCount > 0) {
                WriteLineVerbose(WarnCount + " warnings were generated during rebuild.");
            }
            if (ErrCount > 0) {
                WriteLineVerbose(ErrCount + " errors were generated during rebuild.");
            }
            if (WarnCount > 0 || ErrCount > 0) {
                WriteLineVerbose("See errlog.txt in the game directory for details.");
            }
            if (results == CompileStatus.OK) {
                WriteLineVerbose(EditGame.GameID + " rebuild complete.");
                WriteLine("SUCCESS");
            }
            else {
                // error messages already written in event handler
                WriteLine("FAIL");
            }
        }

        private static void ConsoleCompileChanged() {
            mode = ConsoleMode.CompileChanged;
            WriteLineVerbose("Compiling changed logics in '" + EditGame.GameID + "'");

            // check for no changed files; nothing to do in that case
            bool nochange = true;
            foreach (Logic logres in EditGame.Logics) {
                if (!logres.Compiled) {
                    nochange = false;
                    break;
                }
            }
            if (nochange) {
                WriteLineVerbose("No uncompiled logics. No changes made to " + EditGame.GameID);
                return;
            }
            CompileStatus results = EditGame.CompileChangedLogics();

            if (results == CompileStatus.OK) {
                WriteLineVerbose(CompCount + " logics compiled.");

                if (WarnCount > 0) {
                    WriteLineVerbose(WarnCount + " warnings were generated during compilation.");
                }
                if (ErrCount > 0) {
                    WriteLineVerbose(ErrCount + " errors were generated during compilation.");
                }
                WriteLine("SUCCESS");
            }
            else {
                // error messages already written in event handler
                WriteLine("FAIL");
            }
        }

        private static void ConsoleExportAll() {
            mode = ConsoleMode.ExportAll;
            WriteLineVerbose("Exporting all resources from '" + EditGame.GameID + "'");

            string exportdir = EditGame.SrcResDir;
            bool errors = false;

            foreach (Logic logic in EditGame.Logics) {
                logic.Load();
                // source code (if not resourcedir)
                if (!exportdir.Equals(EditGame.SrcResDir)) {
                    WriteVerbose("Exporting logic " + logic.ID + " source ... ");
                    if (logic.Error == ResourceErrorType.NoError) {
                        try {
                            logic.ExportSource(Path.Combine(exportdir, logic.ID + "." + EditGame.SourceExt));
                            WriteLineVerbose("OK");
                        }
                        catch (Exception ex) {
                            errors = true;
                            WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                        }
                    }
                    else {
                        errors = true;
                        WriteLineVerbose("ERROR " + logic.Error.ToString());
                    }
                }
                // compiled logic
                WriteVerbose("Exporting logic " + logic.ID + " ... ");
                if (logic.Error == ResourceErrorType.NoError) {
                    try {
                        logic.Export(Path.Combine(exportdir, logic.ID + ".agl"));
                        WriteLineVerbose("OK");
                    }
                    catch (Exception ex) {
                        errors = true;
                        WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                    }
                }
                else {
                    errors = true;
                    WriteLineVerbose("ERROR " + logic.Error.ToString());
                }
                logic.Unload();
            }
            foreach (Picture tmpPic in EditGame.Pictures) {
                WriteVerbose("Exporting picture " + tmpPic.ID + "... ");
                tmpPic.Load();
                if (tmpPic.Error == ResourceErrorType.NoError) {
                    try {
                        tmpPic.Export(Path.Combine(exportdir, tmpPic.ID + ".agp"));
                        WriteLineVerbose("OK");
                    }
                    catch (Exception ex) {
                        errors = true;
                        WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                    }
                }
                else {
                    errors = true;
                    WriteLineVerbose("ERROR " + tmpPic.Error.ToString());
                }
                tmpPic.Unload();
            }
            foreach (Sound tmpSnd in EditGame.Sounds) {
                WriteVerbose("Exporting sound " + tmpSnd.ID + "... ");
                tmpSnd.Load();
                if (tmpSnd.Error == ResourceErrorType.NoError) {
                    try {
                        tmpSnd.Export(Path.Combine(exportdir, tmpSnd.ID + ".ags"));
                        WriteLineVerbose("OK");
                    }
                    catch (Exception ex) {
                        errors = true;
                        WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                    }
                }
                else {
                    errors = true;
                    WriteLineVerbose("ERROR " + tmpSnd.Error.ToString());
                }
                tmpSnd.Unload();
            }
            foreach (Engine.View tmpView in EditGame.Views) {
                WriteVerbose("Exporting view " + tmpView.ID + "... ");
                tmpView.Load();
                if (tmpView.Error == ResourceErrorType.NoError) {
                    try {
                        tmpView.Export(Path.Combine(exportdir, tmpView.ID + ".agv"));
                        WriteLineVerbose("OK");
                    }
                    catch (Exception ex) {
                        errors = true;
                        WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                    }
                }
                else {
                    errors = true;
                    WriteLineVerbose("ERROR " + tmpView.Error.ToString());
                }
                tmpView.Unload();
            }
            if (errors) {
                WriteLine("FAIL");
            }
            else {
                WriteLine("SUCCESS");
            }
        }

        private static void ConsoleExportResource(string argval) {
            mode = ConsoleMode.ExportResource;

            // individual export/import

            // format of argval is :L#, (or P,or S, or V)
            if (argval[0] != ':') {
                WriteLine("invalid parameter in export switch");
                WriteLine("FAIL");
                return;
            }
            argval = argval[1..]; // remove leading ':'
            AGIResType restype;

            switch (argval[0]) {
            case 'L' or 'l':
                restype = AGIResType.Logic;
                break;
            case 'P' or 'p':
                restype = AGIResType.Picture;
                break;
            case 'S' or 's':
                restype = AGIResType.Sound;
                break;
            case 'V' or 'v':
                restype = AGIResType.View;
                break;
            default:
                // error
                WriteLine("invalid parameter in export switch");
                WriteLine("FAIL");
                return;
            }
            if (!int.TryParse(argval[1..], out int resnum) || resnum < 0 || resnum > 255) {
                // error
                WriteLine("invalid parameter in export switch");
                WriteLine("FAIL");
                return;
            }
            switch (restype) {
            case AGIResType.Logic:
                if (EditGame.Logics.Contains(resnum)) {
                    Logic logic = EditGame.Logics[resnum];
                    WriteVerbose("Exporting logic " + resnum + "... ");
                    logic.Load();
                    if (logic.Error == ResourceErrorType.NoError) {
                        try {
                            logic.Export(Path.Combine(EditGame.GameDir, logic.ID + ".agl"));
                            WriteLineVerbose("OK");
                            WriteLine("SUCCESS");
                        }
                        catch (Exception ex) {
                            WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                            WriteLine("FAIL");
                        }
                    }
                    else {
                        WriteLineVerbose("ERROR " + logic.Error.ToString());
                        WriteLine("FAIL");
                    }
                    logic.Unload();
                }
                else {
                    WriteLineVerbose("Logic " + resnum + " does not exist in this game");
                    WriteLine("FAIL");
                }
                break;
            case AGIResType.Picture:
                if (EditGame.Pictures.Contains(resnum)) {
                    Picture pic = EditGame.Pictures[resnum];
                    WriteVerbose("Exporting picture " + resnum + "... ");
                    pic.Load();
                    if (pic.Error == ResourceErrorType.NoError) {
                        try {
                            pic.Export(Path.Combine(EditGame.GameDir, pic.ID + ".agp"));
                            WriteLineVerbose("OK");
                            WriteLine("SUCCESS");
                        }
                        catch (Exception ex) {
                            WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                            WriteLine("FAIL");
                        }
                    }
                    else {
                        WriteLineVerbose("ERROR " + pic.Error.ToString());
                        WriteLine("FAIL");
                    }
                    pic.Unload();
                }
                else {
                    WriteLine("Picture " + resnum + " does not exist in this game");
                    WriteLine("FAIL");
                }
                break;
            case AGIResType.Sound:
                if (EditGame.Sounds.Contains(resnum)) {
                    Sound snd = EditGame.Sounds[resnum];
                    WriteVerbose("Exporting sound " + resnum + "... ");
                    snd.Load();
                    if (snd.Error == ResourceErrorType.NoError) {
                        try {
                            snd.Export(Path.Combine(EditGame.GameDir, snd.ID + ".ags"));
                            WriteLineVerbose("OK");
                            WriteLine("SUCCESS");
                        }
                        catch (Exception ex) {
                            WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                            WriteLine("FAIL");
                        }
                    }
                    else {
                        WriteLineVerbose("ERROR " + snd.Error.ToString());
                        WriteLine("FAIL");
                    }
                    snd.Unload();
                }
                else {
                    WriteLineVerbose("Sound " + resnum + " does not exist in this game");
                    WriteLine("FAIL");
                }
                break;
            case AGIResType.View:
                if (EditGame.Views.Contains(resnum)) {
                    Engine.View view = EditGame.Views[resnum];
                    WriteLineVerbose("Exporting view " + resnum + "... ");
                    view.Load();
                    if (view.Error == ResourceErrorType.NoError) {
                        try {
                            view.Export(Path.Combine(EditGame.GameDir, view.ID + ".agv"));
                            WriteLineVerbose("OK");
                            WriteLine("SUCCESS");
                        }
                        catch (Exception ex) {
                            WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                            WriteLine("FAIL");
                        }
                    }
                    else {
                        WriteLineVerbose("ERROR " + view.Error.ToString());
                        WriteLine("FAIL");
                    }
                    view.Unload();
                }
                else {
                    WriteLine("View " + resnum + " does not exist in this game");
                    WriteLine("FAIL");
                }
                break;
            }
        }

        private static void ConsoleAddResource(string argval, string[] args, ref int i) {
            mode = ConsoleMode.AddResource;
            // import a resource

            if (argval[0] != ':') {
                WriteLine("invalid parameter in add switch");
                WriteLine("FAIL");
                return;
            }
            argval = argval[1..]; // remove leading ':'
            AGIResType restype;
            bool source = false;

            switch (argval[0]) {
            case 'L' or 'l':
                restype = AGIResType.Logic;
                break;
            case 'C' or 'c':
                restype = AGIResType.Logic;
                source = true;
                break;
            case 'P' or 'p':
                restype = AGIResType.Picture;
                break;
            case 'S' or 's':
                restype = AGIResType.Sound;
                break;
            case 'V' or 'v':
                restype = AGIResType.View;
                break;
            default:
                // error
                WriteLine("invalid parameter in export switch");
                WriteLine("FAIL");
                return;
            }
            if (!int.TryParse(argval[1..], out int resnum) || resnum < 0 || resnum > 255) {
                // error
                WriteLine("invalid parameter in export switch");
                WriteLine("FAIL");
                return;
            }

            // next arg must be filename
            i++;
            if (i >= args.Length) {
                // missing filename
                WriteLine("unable to import: no import file provided");
                WriteLine("FAIL");
                return;
            }
            // remove any quotes from import filename
            string importfile = args[i];
            if (!Path.IsPathFullyQualified(importfile)) {
                importfile = Path.GetFullPath(importfile);
            }
            if (importfile.Length > 1 && importfile[0] == '"' && importfile[^1] == '"') {
                importfile = importfile[1..^1];
            }
            switch (restype) {
            case AGIResType.Logic:
                // verify logic number not already in use
                if (EditGame.Logics.Contains(resnum)) {
                    WriteLineVerbose("unable to import logic " + resnum + ": it already exists in this game");
                    WriteLine("FAIL");
                    return;
                }
                // verify logic file exists
                if (!File.Exists(importfile)) {
                    WriteLineVerbose("unable to import logic " + resnum + ": '" + importfile + "' not found");
                    WriteLine("FAIL");
                    return;
                }
                try {
                    WriteVerbose("Importing logic " + resnum + "... ");
                    Logic logic = EditGame.Logics.Add((byte)resnum);
                    if (source) {
                        logic.ImportSource(importfile);
                    }
                    else {
                        logic.Import(importfile);
                    }
                    if (logic.Error == ResourceErrorType.NoError) {
                        logic.ID = "Logic" + resnum;
                        logic.SaveSource();
                        WriteLineVerbose("OK");
                    }
                    else {
                        WriteLineVerbose("ERROR " + logic.Error.ToString());
                        WriteLine("FAIL");
                    }
                }
                catch (Exception ex) {
                    WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                    WriteLine("FAIL");
                }
                break;
            case AGIResType.Picture:
                if (EditGame.Pictures.Contains(resnum)) {
                    WriteLineVerbose("unable to import picture " + resnum + ": it already exists in this game");
                    WriteLine("FAIL");
                    return;
                }
                if (!File.Exists(importfile)) {
                    WriteLineVerbose("unable to import picture " + resnum + ": '" + importfile + "' not found");
                    WriteLine("FAIL");
                    return;
                }
                try {
                    WriteVerbose("Importing picture " + resnum + "... ");
                    Picture picture = EditGame.Pictures.Add((byte)resnum);
                    picture.Import(importfile);
                    if (picture.Error == ResourceErrorType.NoError) {
                        WriteLineVerbose("OK");
                    }
                    else {
                        WriteLineVerbose("ERROR " + picture.Error.ToString());
                        WriteLine("FAIL");
                    }
                }
                catch (Exception ex) {
                    WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                    WriteLine("FAIL");
                }
                break;
            case AGIResType.Sound:
                if (EditGame.Sounds.Contains(resnum)) {
                    WriteLineVerbose("unable to import sound " + resnum + ": it already exists in this game");
                    WriteLine("FAIL");
                    return;
                }
                if (!File.Exists(importfile)) {
                    WriteLineVerbose("unable to import sound " + resnum + ": '" + importfile + "' not found");
                    WriteLine("FAIL");
                    return;
                }
                try {
                    WriteVerbose("Importing sound " + resnum + "... ");
                    Sound sound = EditGame.Sounds.Add((byte)resnum);
                    sound.Import(importfile);
                    if (sound.Error == ResourceErrorType.NoError) {
                        WriteLineVerbose("OK");
                    }
                    else {
                        WriteLineVerbose("ERROR " + sound.Error.ToString());
                        WriteLine("FAIL");
                    }
                }
                catch (Exception ex) {
                    WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                    WriteLine("FAIL");
                }
                break;
            case AGIResType.View:
                if (EditGame.Views.Contains(resnum)) {
                    WriteLineVerbose("unable to import view " + resnum + ": it already exists in this game");
                    WriteLine("FAIL");
                    return;
                }
                if (!File.Exists(importfile)) {
                    WriteLineVerbose("unable to import view " + resnum + ": '" + importfile + "' not found");
                    WriteLine("FAIL");
                    return;
                }
                try {
                    WriteVerbose("Importing view " + resnum + "... ");
                    Engine.View view = EditGame.Views.Add((byte)resnum);
                    view.Import(importfile);
                    if (view.Error == ResourceErrorType.NoError) {
                        WriteLineVerbose("OK");
                    }
                    else {
                        WriteLineVerbose("ERROR " + view.Error.ToString());
                        WriteLine("FAIL");
                    }
                }
                catch (Exception ex) {
                    WriteLineVerbose("ERROR 0x" + ex.HResult.ToString("x8"));
                    WriteLine("FAIL");
                }
                break;
            }
        }

        private static bool ReadConsoleSettings() {
            // open the program settings  file
            string userConfig = UserConfigPath;
            string defaultConfig = Path.Combine(ProgramDir, "winagi.config");

            if (!File.Exists(userConfig) && File.Exists(defaultConfig)) {
                File.Copy(defaultConfig, userConfig);
            }
            try {
                WinAGISettingsFile = new SettingsFile(userConfig, FileMode.OpenOrCreate);
            }
            catch (WinAGIException wex) {
                if (wex.HResult == WINAGI_ERR + 539) {
                    // readonly - unable to change it
                    WriteLine("Fatal error: program settings file is marked readonly");
                    return false;
                }
                else {
                    // file access error; try renaming it and creating a default list
                    WriteLine("Unable to read program configuration file (winagi.config)");
                    try {
                        // bad file; 
                        File.Move(userConfig, userConfig + "OLD");
                        WinAGISettingsFile = new SettingsFile(userConfig, FileMode.Create);
                        WriteLine("Restoring default configuration file");
                    }
                    catch {
                        // unrecoverable error
                        WriteLine("It may be corrupt. Try deleting it, then restart WinAGI");
                        WriteLine("to restore default settings.");
                        return false;
                    }
                }
            }

            // GENERAL
            WinAGISettings.BackupResFile.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefMaxSO.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.DefMaxSO.Value < 1) {
                WinAGISettings.DefMaxSO.Value = 1;
            }
            DefMaxSO = WinAGISettings.DefMaxSO.Value;
            WinAGISettings.DefMaxVol0.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.DefMaxVol0.Value < 32768) {
                WinAGISettings.DefMaxVol0.Value = 32768;
            }
            if (WinAGISettings.DefMaxVol0.Value > 1047552) {
                WinAGISettings.DefMaxVol0.Value = 1047552;
            }
            DefMaxVol0Size = WinAGISettings.DefMaxVol0.Value;
            WinAGISettings.DefCP.ReadSetting(WinAGISettingsFile);
            switch (WinAGISettings.DefCP.Value) {
            case 437 or 850 or 852 or 855 or 857 or 858 or 860 or 861 or 863 or 869:
                break;
            default:
                WinAGISettings.DefCP.Reset();
                break;
            }
            CodePage = WinAGISettings.DefCP.Value;
            WinAGISettings.DefResDir.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefResDir.Value = WinAGISettings.DefResDir.Value.Trim();
            if (WinAGISettings.DefResDir.Value.Length == 0) {
                WinAGISettings.DefResDir.Reset();
            }
            else if ((" \\/:*?\"<>|").Any(WinAGISettings.DefResDir.Value.Contains)) {
                WinAGISettings.DefResDir.Reset();
            }
            else if (WinAGISettings.DefResDir.Value.Any(ch => ch > 127 || ch < 32)) {
                WinAGISettings.DefResDir.Reset();
            }
            DefResDir = WinAGISettings.DefResDir.Value;

            // LOGICS
            WinAGISettings.LogicTabWidth.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.LogicTabWidth.Value < 1) {
                WinAGISettings.LogicTabWidth.Value = 1;
            }
            if (WinAGISettings.LogicTabWidth.Value > 32) {
                WinAGISettings.LogicTabWidth.Value = 32;
            }
            WinAGISettings.DefaultExt.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefaultExt.Value = WinAGISettings.DefaultExt.Value.ToLower().Trim();
            if (WinAGISettings.DefaultExt.Value[0] == '.') {
                WinAGISettings.DefaultExt.Value = WinAGISettings.DefaultExt.Value[1..];
            }
            // decoder uses default extension
            LogicDecoder.DefaultSrcExt = WinAGISettings.DefaultExt.Value;
            WinAGISettings.ErrorLevel.ReadSetting(WinAGISettingsFile);
            ErrorLevel = WinAGISettings.ErrorLevel.Value;
            WinAGISettings.DefIncludeIDs.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefIncludeReserved.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefIncludeGlobals.ReadSetting(WinAGISettingsFile);

            // LOGIC DECOMPILER
            LogicDecoder.MsgsByNumber = WinAGISettings.MsgsByNumber.ReadSetting(WinAGISettingsFile);
            LogicDecoder.IObjsByNumber = WinAGISettings.IObjsByNumber.ReadSetting(WinAGISettingsFile);
            LogicDecoder.WordsByNumber = WinAGISettings.WordsByNumber.ReadSetting(WinAGISettingsFile);
            LogicDecoder.ShowAllMessages = WinAGISettings.ShowAllMessages.ReadSetting(WinAGISettingsFile);
            LogicDecoder.SpecialSyntax = WinAGISettings.SpecialSyntax.ReadSetting(WinAGISettingsFile);
            LogicDecoder.ReservedAsText = WinAGISettings.ReservedAsText.ReadSetting(WinAGISettingsFile);
            LogicDecoder.CodeStyle = WinAGISettings.CodeStyle.ReadSetting(WinAGISettingsFile);

            // PLATFORM DEFAULTS
            WinAGISettings.AutoFill.ReadSetting(WinAGISettingsFile);
            WinAGISettings.PlatformType.ReadSetting(WinAGISettingsFile);
            WinAGISettings.PlatformFile.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DOSExec.ReadSetting(WinAGISettingsFile);
            WinAGISettings.PlatformOpts.ReadSetting(WinAGISettingsFile);

            // DEFAULT RESERVED DEFINES
            DefaultReservedDefines = new(WinAGISettingsFile);

            // IGNORED COMPILER WARNINGS
            int warndata = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn0", 0);
            for (int i = 0; i <= 29; i++) {
                SetIgnoreWarning(5001 + i, (warndata & (1 << i)) == (1 << i));
            }
            warndata = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn1", 0);
            for (int i = 30; i <= 59; i++) {
                SetIgnoreWarning(5001 + i, (warndata & (1 << (i - 30))) == 1 << (i - 30));
            }
            warndata = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn2", 0);
            for (int i = 60; i <= 89; i++) {
                SetIgnoreWarning(5001 + i, (warndata & (1 << (i - 60))) == 1 << (i - 60));
            }
            warndata = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn3", 0);
            for (int i = 90; i <= 119; i++) {
                SetIgnoreWarning(5001 + i, (warndata & (1 << (i - 90))) == 1 << (i - 90));
            }
            warndata = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn4", 0);
            for (int i = 120; i < WARNCOUNT; i++) {
                SetIgnoreWarning(7001 + i - 120, (warndata & (1 << (i - 120))) == 1 << (i - 120));
            }
            return true;
        }

        private static void AddToInfoGrid(WinAGIEventInfo loadinfo) {
            infoGridTable.Rows.Add(loadinfo.Type,
                         loadinfo.ResType,
                         loadinfo.ID,
                         loadinfo.Text,
                         loadinfo.ResType <= AGIResType.View ? loadinfo.ResNum : -1,
                         loadinfo.Line,
                         loadinfo.Module,
                         loadinfo.Filename);
        }

        public static void NewGameStatus(WinAGIEventInfo e) {
            // currently not used
        }

        public static void LoadGameStatus(WinAGIEventInfo loadinfo) {
            // only when importing 
            if (mode != ConsoleMode.Import) {
                return;
            }
            if (!Verbose) {
                return;
            }

            string msg = "";
            switch (loadinfo.Type) {
            case ResourceError:
            case ResourceWarning:
                msg = loadinfo.Type == ResourceError ? "Error " : "Warning: ";
                msg += loadinfo.ID + ": ";
                switch (loadinfo.ResType) {
                case AGIResType.Logic:
                case AGIResType.Picture:
                case AGIResType.Sound:
                case AGIResType.View:
                    msg += loadinfo.ResType.ToString() + " " + loadinfo.ResNum;
                    AddToInfoGrid(loadinfo);
                    break;
                case AGIResType.Words:
                    msg += "WORDS.TOK";
                    AddToInfoGrid(loadinfo);
                    break;
                case AGIResType.Objects:
                    msg += "OBJECT";
                    AddToInfoGrid(loadinfo);
                    break;
                case AGIResType.Globals:
                    msg += "globals.txt";
                    break;
                }
                break;
            case DecompWarning:
                // add to error list
                // check for decompile renumber
                if (loadinfo.ID == "renumber") {
                    int offset = loadinfo.Line;
                    RenumberWarnings(loadinfo.ResNum, offset);
                    return;
                }
                msg = "Decompile Warning: " + loadinfo.ID + ": Logic " +
                    loadinfo.ResNum.ToString() + " at line " + loadinfo.Line.ToString();
                break;
            case Info: // ignore
                // check for decompiling?
                switch (loadinfo.InfoType) {
                case InfoType.DecodingAllLogics:
                    // sierra syntax only; now we know which logic is being 
                    // decompiled
                    msg = "Decompiling logic " + loadinfo.ResNum;
                    break;
                case InfoType.Finalizing:
                case InfoType.Validating:
                case InfoType.Resources:
                case InfoType.Decompiling:
                case InfoType.PropertyFile:
                case InfoType.CheckCRC:
                    // ignore
                    return;
                case InfoType.Initialize:
                case InfoType.ClearWarnings:
                case InfoType.Compiling:
                case InfoType.Compiled:
                case InfoType.Decompiled:
                case InfoType.Done:
                    // check
                    Debug.Assert(false);
                    return;
                }
                break;
            case TODO: // ignore
                return;
            case LogicCompileError:
            case LogicCompileWarning:
            default:
                // should not happen during load
                return;
            }
            WriteLineVerbose(msg);
        }

        public static void CompileGameStatus(CompileGameEventArgs e) {
            WinAGIEventInfo compInfo = e.CompileInfo;
            switch (e.CStatus) {
            case GameCompileStatus.CompileWords:
                switch (compInfo.InfoType) {
                case InfoType.Resources:
                    if (Verbose) {
                        WriteLineVerbose("Compiling WORDS.TOK");
                    }
                    break;
                }
                break;
            case GameCompileStatus.CompileObjects:
                switch (compInfo.InfoType) {
                case InfoType.Resources:
                    if (Verbose) {
                        WriteLineVerbose("Compiling OBJECT");
                    }
                    break;
                }
                break;
            case GameCompileStatus.AddResource:
                string msg;
                switch (compInfo.InfoType) {
                case InfoType.ClearWarnings:
                    // reset error/warning count
                    logErr = 0;
                    logWarn = 0;
                    break;
                case InfoType.Compiling:
                    // compiling a logic a resource
                    WriteVerbose("Compiling " + compInfo.Module + " ... ");
                    break;
                case InfoType.Compiled:
                    Debug.Assert(compInfo.ResType == AGIResType.Logic);
                    if (compInfo.ResType == AGIResType.Logic) {
                        RefreshLogicWarnings(compInfo.ResNum, false);
                        msg = "";
                        if (logErr > 0) {
                            msg += logErr.ToString() + " error";
                            if (logErr > 1)
                                msg += "s";
                        }
                        if (logWarn > 0) {
                            if (msg.Length > 0) {
                                msg += "; ";
                            }
                            msg += logWarn.ToString() + " warning";
                            if (logWarn > 1)
                                msg += "s";
                        }
                        msg += " OK";
                        WriteLineVerbose(msg);
                        CompCount++;
                    }
                    break;
                case InfoType.CompileError:
                    msg = "";
                    if (logErr > 0) {
                        msg += logErr.ToString() + " error";
                        if (logErr > 1)
                            msg += "s";
                    }
                    if (logWarn > 0) {
                        if (msg.Length > 0) {
                            msg += "; ";
                        }
                        msg += logWarn.ToString() + " warning";
                        if (logWarn > 1)
                            msg += "s";
                    }
                    msg += " ERROR";
                    WriteLineVerbose(msg);
                    break;
                case InfoType.Resources:
                    // adding a resource
                    WriteLineVerbose("Adding resource: " + compInfo.Module);
                    break;
                }
                break;
            case GameCompileStatus.DoneAdding:
                break;
            case GameCompileStatus.CompileComplete:
                RefreshModWarnings(false);
                EditGame.SaveProperties();
                WriteLineVerbose("Creating DIR file entries");
                break;
            case GameCompileStatus.Warning:
                // warning generated
                if (compInfo.Type != Info) {
                    WarnCount++;
                    WriteLineVerbose("    " + compInfo.Text);
                    AddToInfoGrid(compInfo);
                }
                break;
            case GameCompileStatus.ResError:
            case GameCompileStatus.LogicError:
                // error encountered
                ErrCount++;
                if (compInfo.Type != Info) {
                    AddToInfoGrid(compInfo);
                }
                break;
            case GameCompileStatus.FatalError:
                WriteLine("FATAL ERROR: " + compInfo.Text);
                break;
            }
        }

        public static void CompileLogicStatus(WinAGIEventInfo e) {
            switch (e.Type) {
            case LogicCompileError:
                // error
                ErrCount++;
                logErr++;
                AddToInfoGrid(e);
                break;
            case LogicCompileWarning:
                // warning
                WarnCount++;
                logWarn++;
                AddToInfoGrid(e);
                break;
            case Info:
                switch (e.InfoType) {
                case InfoType.Initialize:
                case InfoType.ClearWarnings:
                case InfoType.Validating:
                case InfoType.PropertyFile:
                case InfoType.Resources:
                case InfoType.Decompiling:
                case InfoType.Decompiled:
                case InfoType.CheckCRC:
                case InfoType.Finalizing:
                default:
                    Debug.Assert(false);
                    break;
                }
                break;
            default:
                Debug.Assert(false);
                break;
                //case ResourceWarning:
                //    break;
                //case DecompWarning:
                //    break;
                //case TODO:
                //    break;
            }
        }

        public static void DecodeLogicStatus(WinAGIEventInfo e) {
            switch (e.Type) {
            case DecompWarning:
                if (e.ID == "renumber") {
                    return;
                }
                // warning
                WarnCount++;
                AddToInfoGrid(e);
                break;
            case DecompError:
                // error
                ErrCount++;
                AddToInfoGrid(e);
                break;
            case Info:
                switch (e.InfoType) {
                case InfoType.ClearWarnings:
                    // ignore if importing
                    if (mode != ConsoleMode.Import) {
                        ClearInfogrid(e.ResType, e.ResNum);
                    }
                    break;
                case InfoType.Decompiled:
                    // if not importing, save the update and save the logic's
                    // warnings (game has already been loaded)
                    if (mode != ConsoleMode.Import) {
                        RefreshLogicWarnings(e.ResNum, true);
                    }
                    break;
                case InfoType.Initialize:
                case InfoType.Validating:
                case InfoType.PropertyFile:
                case InfoType.Resources:
                case InfoType.Decompiling:
                case InfoType.CheckCRC:
                case InfoType.Finalizing:
                default:
                    Debug.Assert(false);
                    break;
                }
                break;
            default:
                Debug.Assert(false);
                break;
                //case LogicCompileError:
                //    break;
                //case LogicCompileWarning:
                //    break;
                //case ResourceWarning:
                //    break;
                //case TODO:
                //    break;
            }
        }

        private static void ShowHelp() {
            WriteLine("Usage: conWAGI <dir|gamefile> actionswitch [/q]");
            WriteLine("  <dir>       directory containing existing VOL and DIR files");
            WriteLine("              only valid with /i switch");
            WriteLine("  <gamefile>  winagi game property file (*.wag); gamefile must be");
            WriteLine("              in its game directory");
            WriteLine();
            WriteLine(" valid action switches:");
            WriteLine("   /i  imports existing game VOL and DIR files in <dir>");
            WriteLine("       and creates new wag file with default GameID");
            WriteLine("       if <dir> is blank or null, current directory is assumed");
            WriteLine("       optional parameters:");
            WriteLine("           /C:#   set code page (default is 437)");
            WriteLine("           /E:<ext>   set source resource file extension");
            WriteLine("           /R:<resdir>   set source resource directory");
            WriteLine("           /S   use sierra syntax (default is Fan syntax)");
            WriteLine("           /nG   don't include global variables in logic source files");
            WriteLine("           /nR   don't include reserved words in logic source files");
            WriteLine("           /nI   don't include resource IDs in logic source files");
            WriteLine();
            WriteLine("   /c  does a full game compile of <gamefile> in its directory");
            WriteLine("       replacing existing VOL and DIR files");
            WriteLine();
            WriteLine("   /r  rebuilds VOL files, without recompiling logics");
            WriteLine();
            WriteLine("   /d  compiles all changed logics, but doesn't rebuild VOL files");
            WriteLine("       rebuilt logics are put in first available VOL space");
            WriteLine();
            WriteLine("   /e  exports all resources to individual files; resources will");
            WriteLine("       be exported to the game's source directory");
            WriteLine();
            WriteLine("   /e:L# | /e:P# | /e:V# | /e:S# ");
            WriteLine("      exports a single resource of the specified type and number");
            WriteLine("      to the game's resource directory");
            WriteLine();
            WriteLine("   /a:L# | /a:C# | /a:P# | /a:V# | /a:S# <resfile>");
            WriteLine("      adds the resource in <resfile> to the the game as the ");
            WriteLine("      specified type and number (use L to import a compiled logic");
            WriteLine("      resource, C to import a source code file");
            WriteLine();
            WriteLine("   /quiet or /q   suppresses all non-essential status messages");
            WriteLine("                  must be last action switch in the command");
            WriteLine("   /help or /?    displays help text explaining how to use the");
            WriteLine("                  console mode");
        }

        private static void Write(string message) {
            if (Debugger.IsAttached) {
                Debug.Write(message);
            }
            else {
                Console.Write(message);
            }
        }

        private static void WriteVerbose(string message) {
            if (Verbose) {
                Write(message);
            }
        }

        private static void WriteLine(string message) {
            if (Debugger.IsAttached) {
                Debug.WriteLine(message);
            }
            else {
                Console.WriteLine(message);
            }
        }

        public static void WriteLine() {
            if (Debugger.IsAttached) {
                Debug.WriteLine("");
            }
            else {
                Console.WriteLine();
            }
        }

        private static void WriteLineVerbose(string message) {
            if (Verbose) {
                WriteLine(message);
            }
        }
    }
}
