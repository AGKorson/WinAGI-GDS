using System;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Common.Base;

namespace WinAGI.Common {

    public class BkgdTasks {
        internal static BackgroundWorker bgwCompGame = null;
        internal static BackgroundWorker bgwNewGame = null;
        internal static BackgroundWorker bgwOpenGame = null;
        private static bool updating;
        private static CompileMode mode = CompileMode.Full;
        private static bool replace = true;

        public static void NewGameDoWork(object sender, DoWorkEventArgs e) {
            string strError = "";
            bool blnWarnings = false;
            int lngErr;
            NewGameResults argval = (NewGameResults)e.Argument;
            bool Loaded;

            updating = false;
            try {
                // create new game  (newID, version, directory resouredirname and template info)
                EditGame = new AGIGame(argval.NewID, argval.Version, argval.GameDir, argval.ResDir, argval.SrcExt, argval.TemplateDir);
                // no errors, means game was successfully created
                Loaded = true;
                // blnWarnings = true;
            }
            catch (Exception ex) {
                // error always causes failure
                EditGame = null;
                Loaded = false;
                lngErr = ex.HResult;
                strError = "Error encountered, game not created: " + ex.Message;
                bgwNewGame.ReportProgress(0, "Error encountered, game not created");
            }
            if (Loaded) {
                argval.Failed = false;
                bgwNewGame.ReportProgress(55, "Game created successfully, setting up editors");
                argval.Warnings = blnWarnings;
            }
            else {
                argval.Failed = true;
                argval.ErrorMsg = strError;
            }
            e.Result = argval;
        }

        public static void NewGameProgressChanged(object sender, ProgressChangedEventArgs e) {
            // progress percentage used to identify event types
            switch (e.ProgressPercentage) {
            case 1:
                // load warning/error
                MDIMain.AddWarning((TWinAGIEventInfo)e.UserState);
                break;
            case 2:
                // TODOs
                MDIMain.AddWarning((TWinAGIEventInfo)e.UserState);
                break;
            case 3:
                // Decode warning
                MDIMain.AddWarning((TWinAGIEventInfo)e.UserState);
                break;
            case 51:
                // initializing
                ProgressWin.lblProgress.Text = e.UserState.ToString();
                break;
            case 52:
                // adding/copying resource files
                ProgressWin.lblProgress.Text = e.UserState.ToString();
                break;
            case 53:
                // opening new game (after copying from template)
                ProgressWin.lblProgress.Text = "opening new game files";
                ProgressWin.pgbStatus.Visible = true;
                break;
            case 54:
                // updating WinAGI version
                updating = true;
                break;
            case 55:
                // game finished loading
                MDIMain.Text = "WinAGI GDS - " + EditGame.GameID;
                // build resource list
                BuildResourceTree();
                // show it, if needed
                if (WinAGISettings.ResListType.Value != EResListType.None) {
                    // show resource tree pane
                    MDIMain.ShowResTree();
                }
                switch (WinAGISettings.ResListType.Value) {
                case EResListType.TreeList:
                    // select root
                    MDIMain.tvwResources.SelectedNode = MDIMain.tvwResources.Nodes[0];
                    break;
                case EResListType.ComboList:
                    // select root
                    MDIMain.cmbResType.SelectedIndex = 0;
                    // update selected resource
                    MDIMain.SelectResource(Game, -1);
                    break;
                }
                if (updating) {
                    DialogResult rtn = MessageBox.Show(MDIMain,
                        "The template game is from an older version of WinAGI. " +
                        "If your game uses extended characters, you will need to update " +
                        "the logic source files.\n\nDo you want your source files " +
                        "updated automatically?\n\n(Choose NO if your game does NOT use " +
                        "extended characters.)",
                        "Update WAG File to New Version",
                        MessageBoxButtons.YesNo);
                    if (rtn == DialogResult.Yes) {
                        foreach (Logic logic in EditGame.Logics) {
                            if (File.Exists(logic.SourceFile)) {
                                try {
                                    byte[] strdat = File.ReadAllBytes(logic.SourceFile);
                                    string srcText = EditGame.CodePage.GetString(strdat);
                                    File.WriteAllText(logic.SourceFile, srcText);
                                }
                                catch {
                                    // ignore exceptions
                                }
                            }
                        }
                        SafeFileMove(EditGame.GameDir + "globals.txt", EditGame.ResDir + "globals.txt", true);
                    }
                }
                // show selection in preview, if needed
                if (WinAGISettings.ShowPreview.Value) {
                    PreviewWin.Show();
                }
                break;
            default:
                // all other progress events just update the label text
                ProgressWin.lblProgress.Text = e.UserState.ToString();
                break;
            }
        }

        public static void NewGameWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // new game action completed
            ProgressWin.Close();
            ProgressWin.Dispose();
            MDIMain.Refresh();
            NewResults = (NewGameResults)e.Result;
            if (NewResults.Failed) {
                // remove everything from target directory
                foreach (string file in Directory.GetFiles(EditGame.GameDir)) {
                    try {
                        File.Delete(file);
                    }
                    catch {
                        // ignore errors
                    }
                }
                foreach (string file in Directory.GetFiles(EditGame.ResDir)) {
                    try {
                        File.Delete(file);
                    }
                    catch {
                        // ignore errors
                    }
                }
                try {
                    Directory.Delete(EditGame.ResDir);
                }
                catch {
                    // ignore errors
                }

                MessageBox.Show(MDIMain,
                    "Unable to create new game due to an error:\n\n" + NewResults.ErrorMsg,
                    "New AGI Game Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else {
                if (NewResults.Warnings) {
                    MessageBox.Show(
                        MDIMain,
                        "Some errors and/or anomalies in resource data were encountered in the template.",
                        "Template Game Anomalies",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public static void OpenGameDoWork(object sender, DoWorkEventArgs e) {
            string strError = "";
            bool blnWarnings = false;
            int lngErr;
            LoadGameResults argval = (LoadGameResults)e.Argument;
            bool blnLoaded;

            updating = false;
            try {
                // if game can't be loaded, constructor ALWAYS returns error
                if (argval.Mode == 0) {
                    EditGame = new AGIGame(OpenGameMode.File, argval.Source);
                }
                else {
                    EditGame = new AGIGame(OpenGameMode.Directory, argval.Source);
                }
                // no errors, means game loaded OK
                blnLoaded = true;
                blnWarnings = EditGame.LoadWarnings;
            }
            catch (Exception ex) {
                // errors always causes failure to load
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                blnLoaded = false;
                lngErr = ex.HResult;
                if ((lngErr & WINAGI_ERR) == WINAGI_ERR) {
                    switch (lngErr - WINAGI_ERR) {
                    case 501:
                        strError = "A game is already loaded. Close it before opening another game.";
                        break;
                    case 502:
                        // DIR file access error
                        // ["exception"] Exception = ex
                        // ["dirfile"] = string dirfile
                        strError = $"A file access error occurred while trying to open {Path.GetFileName((string)ex.Data["dirfile"])}:{Environment.NewLine}{Environment.NewLine}{((WinAGIException)ex.Data["exception"]).HResult}: {((WinAGIException)ex.Data["exception"]).Message}";
                        break;
                    case 524:
                        // missing v2 DIR file
                        // ["missingfile"] = string dirfile
                        strError = $"A critical game file ({Path.GetFileName((string)ex.Data["missingfile"])}) is missing.";
                        break;
                    case 541:
                        // Not a valid AGI directory
                        //["baddir"] = string gamedir
                        strError = $"Invalid or missing directory file '{Path.GetFileName((string)ex.Data["baddir"])}'";
                        break;
                    case 542:
                        // invalid DIR file
                        //["baddir"] = string DIRfile
                        strError = $"'{ex.Data["baddir"]}' is an invalid directory file.";
                        break;
                    case 543:
                        //invalid interpreter version - couldn't find correct version from the AGI
                        //files
                        strError = ex.Message;
                        break;
                    case 655:
                        strError = $"Missing game property file ({ex.Data["badfile"]}).";
                        break;
                    case 665:
                        if (ex.Data["badversion"].ToString().Length > 0) {
                            strError = $"Invalid WINAGI version ({ex.Data["badversion"]}).";
                        }
                        else {
                            strError = $"Invalid WINAGI property file ({ex.Data["badfile"]}).";
                        }
                        break;
                    case 690:
                        //missing gameID in wag file
                        strError = "Game property file does not contain a valid GameID.";
                        break;
                    case 691:
                        //invalid intVersion in wag file
                        strError = $"{ex.Data["badversion"]} is not a valid Interpreter Version.";
                        break;
                    case 699:
                        // Unable to backup existing wag file
                        // ["exception"] = Exception ex
                        strError = $"Unable to backup existing WAG file - {ex.HResult}: {ex.Message}";
                        break;
                    case 700:
                        // game file is readonly
                        strError = $"Game file ({Path.GetFileName((string)ex.Data["badfile"])}) is readonly";
                        break;
                    case 701:
                        //file error accessing WAG
                        // ["exception"] = Exception ex
                        strError = $"File access error when reading WAG file - {ex.HResult}: {ex.Message}";
                        break;
                    case 703:
                        // file access error in DIR file
                        // ["exception"] Exception = ex
                        // ["dirfile"] = string dirfile
                        strError = $"File access error when reading {Path.GetFileName(ex.Data["dirfile"].ToString())} file - {ex.HResult}: {ex.Message}";
                        break;
                    default:
                        // unknown error
                        Debug.Assert(false);
                        strError = "UNKNOWN: " + lngErr.ToString() + " - " + ex.Source + " - " + ex.Message;
                        break;
                    }
                }
                else {
                    // unknown error
                    strError = "UNKNOWN: " + lngErr.ToString() + " - " + ex.Source + " - " + strError;
                }
            }
            if (blnLoaded) {
                bgwOpenGame.ReportProgress(50, "Game " + (argval.Mode == 0 ? "loaded" : "imported") + " successfully, setting up editors");
                argval.Warnings = blnWarnings;
            }
            else {
                argval.Failed = true;
                argval.ErrorMsg = strError;
            }
            e.Result = argval;
        }

        public static void OpenGameProgressChanged(object sender, ProgressChangedEventArgs e) {
            // progress percentage used to identify different types of events

            switch (e.ProgressPercentage) {
            case 1:
                // resource error/warning
                MDIMain.AddWarning((TWinAGIEventInfo)e.UserState);
                break;
            case 2:
                // TODOs
                MDIMain.AddWarning((TWinAGIEventInfo)e.UserState);
                break;
            case 3:
                // Decode warning
                MDIMain.AddWarning((TWinAGIEventInfo)e.UserState);
                break;
            case 4:
                // updating WinAGI version
                updating = true;
                break;
            case 50:
                // game finished loading
                MDIMain.Text = "WinAGI GDS - " + EditGame.GameID;
                BuildResourceTree();
                if (WinAGISettings.ResListType.Value != EResListType.None) {
                    MDIMain.ShowResTree();
                }
                // setting reslist type will also show PreviewWin if needed
                switch (WinAGISettings.ResListType.Value) {
                case EResListType.TreeList:
                    MDIMain.tvwResources.SelectedNode = MDIMain.tvwResources.Nodes[0];
                    break;
                case EResListType.ComboList:
                    MDIMain.cmbResType.SelectedIndex = 0;
                    MDIMain.SelectResource(Game, -1);
                    break;
                }
                if (updating) {
                    _ = MessageBox.Show(MDIMain,
                        "This game was last opened with an older version of WinAGI. " +
                        "Logic source files need to be updated.\n\n" +
                        "Your original source files will be moved to a backup folder " +
                        "in your game directory.",
                        "Updating WAG File to New Version",
                        MessageBoxButtons.OK);
                    try {
                        Directory.CreateDirectory(EditGame.ResDir[..^1] + "_BACKUP");
                    }
                    catch {
                        // ignore exceptions
                    }
                    foreach (string file in Directory.GetFiles(EditGame.ResDir, "*.lgc")) {
                        try {
                            File.Copy(file, EditGame.ResDir[..^1] + "_BACKUP\\" + Path.GetFileName(file), true);
                        }
                        catch {
                        }
                    }
                    foreach (Logic logic in EditGame.Logics) {
                        if (File.Exists(logic.SourceFile)) {
                            try {
                                byte[] strdat = File.ReadAllBytes(logic.SourceFile);
                                bool _ = false;
                                string srcText = CheckIncludes(EditGame.CodePage.GetString(strdat),EditGame, ref _);
                                File.WriteAllText(logic.SourceFile, srcText);
                            }
                            catch {
                                // ignore exceptions
                            }
                        }
                    }
                    // move globals file
                    SafeFileMove(EditGame.GameDir + "globals.txt", EditGame.ResDir + "globals.txt", true);
                }
                break;
            default:
                // all other progress events just update the label text
                ProgressWin.lblProgress.Text = e.UserState.ToString();
                break;
            }
        }

        public static void OpenGameWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // load is over
            ProgressWin.Close();
            ProgressWin.Dispose();
            // refresh results
            LoadResults = (LoadGameResults)e.Result;
            if (LoadResults.Failed) {
                MessageBox.Show(
                    MDIMain,
                    LoadResults.ErrorMsg,
                    "Unable to " + (LoadResults.Mode == 0 ? "Open" : "Import") + " Game",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else {
                if (LoadResults.Warnings) {
                    MessageBox.Show(
                        MDIMain,
                        "Some errors and/or anomalies in resource data were encountered.",
                        "Anomalies Detected During " + (LoadResults.Mode == 0 ? "Load" : "Import"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public static void CompileGameDoWork(object sender, DoWorkEventArgs e) {
            CompileGameResults argval = (CompileGameResults)e.Argument;
            CompileStatus results = CompileStatus.OK;
            mode = argval.Mode;
            try {
                switch (argval.Mode) {
                case CompileMode.Full:
                    // full compile
                    replace = argval.parm.Length == 0 || EditGame.GameDir.Equals(argval.parm, StringComparison.OrdinalIgnoreCase);
                    results = EditGame.Compile(false, argval.parm);
                    break;
                case CompileMode.RebuildOnly:
                    // normal rebuild
                    replace = true;
                    results = EditGame.Compile(true);
                    break;
                case CompileMode.ChangedLogics:
                    replace = true;
                    results = EditGame.CompileChangedLogics();
                    break;
                case CompileMode.ChangeVersion:
                    // change version rebuild
                    replace = true;
                    EditGame.InterpreterVersion = argval.parm;
                    break;
                }
                // pass back results status
                argval.Status = results;
                e.Result = argval;
            }
            catch (Exception ex) {
                argval.Status = CompileStatus.Error;
                argval.CompExc = ex;
                e.Result = argval;
            }
        }

        public static void CompileGameProgressChanged(object sender, ProgressChangedEventArgs e) {
            TWinAGIEventInfo compInfo = (TWinAGIEventInfo)e.UserState;
            
            switch ((GameCompileStatus)e.ProgressPercentage) {
            case GameCompileStatus.CompileWords:
                switch (compInfo.InfoType) {
                case InfoType.Resources:
                    CompStatusWin.lblStatus.Text = "Compiling WORDS.TOK";
                    CompStatusWin.pgbStatus.Value++;
                    break;
                case InfoType.ClearWarnings:
                    MDIMain.ClearWarnings(compInfo.ResType, compInfo.ResNum);
                    break;
                }
                break;
            case GameCompileStatus.CompileObjects:
                CompStatusWin.lblStatus.Text = "Compiling OBJECT";
                CompStatusWin.pgbStatus.Value++;
                break;
            case GameCompileStatus.AddResource:
                switch (compInfo.InfoType) {
                case InfoType.ClearWarnings:
                    // clear warnings and errors for this resource
                    MDIMain.ClearWarnings(compInfo.ResType, compInfo.ResNum, [EventType.LogicCompileError, EventType.LogicCompileWarning, EventType.ResourceError, EventType.ResourceWarning]);
                    break;
                case InfoType.CheckLogic:
                    CompStatusWin.lblStatus.Text = "Checking logic: " + compInfo.ID;
                    CompStatusWin.pgbStatus.Value++;
                    break;
                case InfoType.Compiling:
                    // compiling a logic a resource
                    CompStatusWin.lblStatus.Text = "Compiling logic: " + compInfo.ID;
                    break;
                case InfoType.Compiled:
                    RefreshTree(compInfo.ResType, compInfo.ResNum);
                    break;
                case InfoType.Resources:
                    // adding a resource
                    CompStatusWin.lblStatus.Text = "Adding resource: " + compInfo.ID;
                    CompStatusWin.pgbStatus.Value++;
                    break;
                }
                break;
            case GameCompileStatus.DoneAdding:
                break;
            case GameCompileStatus.CompileComplete:
                CompStatusWin.lblStatus.Text = "Creating DIR file entries";
                break;
            case GameCompileStatus.Warning:
                // warning generated
                int warnings = CompStatusWin.Warnings;
                warnings++;
                warnings.ToString();
                CompStatusWin.lblWarnings.Text = warnings.ToString();
                CompStatusWin.Warnings = warnings;
                MDIMain.AddWarning(compInfo);
                break;
            case GameCompileStatus.ResError:
            case GameCompileStatus.LogicError:
                // error encountered
                int errors = CompStatusWin.Errors;
                errors++;
                errors.ToString();
                CompStatusWin.lblWarnings.Text = errors.ToString();
                CompStatusWin.Warnings = errors;
                MDIMain.AddWarning(compInfo);
                break;
            case GameCompileStatus.FatalError:
                // need to tell user!
                CompStatusWin.FatalError = compInfo;
                CompStatusWin.lblStatus.Text = "FATAL ERROR...";
                CompStatusWin.pgbStatus.Value = CompStatusWin.pgbStatus.Maximum;
                break;
            case GameCompileStatus.Canceled:
                CompStatusWin.lblStatus.Text = "CANCELING COMPILE...";
                CompStatusWin.pgbStatus.Value = CompStatusWin.pgbStatus.Maximum;
                break;
            }
            CompStatusWin.Refresh();
        }

        /// <summary>
        /// This method handles the worker completed event for the background EditGame.Compile method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CompileGameWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            TWinAGIEventInfo errmsg = CompStatusWin.FatalError;
            CompStatusWin.Close();
            CompStatusWin?.Dispose();

            // refresh results
            CompGameResults = (CompileGameResults)e.Result;
            switch (CompGameResults.Mode) {
            case CompileMode.Full:
            case CompileMode.RebuildOnly:
                switch (CompGameResults.Status) {
                case CompileStatus.OK:
                    //everything is ok
                    MDIMain.UseWaitCursor = false;
                    if (CompGameResults.Warnings) {
                        if (!MDIMain.pnlWarnings.Visible) {
                            MDIMain.pnlWarnings.Visible = true;
                        }
                        //msgbox to user
                        MessageBox.Show(MDIMain,
                            "Warnings were generated during game" + (CompGameResults.Mode == CompileMode.RebuildOnly ? "rebuild." : "compile."),
                            CompGameResults.Mode == CompileMode.RebuildOnly ? "Rebuild VOL Files" : "Compile Game",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else {
                        MDIMain.UseWaitCursor = false;
                        MessageBox.Show(MDIMain,
                            (CompGameResults.Mode == CompileMode.RebuildOnly ? "Rebuild" : "Compile") + " completed successfully.",
                            CompGameResults.Mode == CompileMode.RebuildOnly ? "Rebuild VOL Files" : "Compile Game",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    break;
                case CompileStatus.Error:
                case CompileStatus.Canceled:
                    // need to only restore words/object if
                    // compile was to another directory
                    if (CompGameResults.parm != EditGame.GameDir) {
                        // file ops in try blocks
                        // delete any new files
                        SafeFileDelete(CompGameResults.parm + "WORDS.TOK");
                        SafeFileDelete(CompGameResults.parm + "OBJECT");
                        // restore old files if they exist
                        if (File.Exists(CompGameResults.parm + "WORDS_OLD.TOK")) {
                            SafeFileMove(CompGameResults.parm + "WORDS_OLD.TOK", CompGameResults.parm + "WORDS.TOK", true);

                        }
                        if (File.Exists(CompGameResults.parm + "OBJECT_OLD")) {
                            SafeFileMove(CompGameResults.parm + "OBJECT_OLD", CompGameResults.parm + "OBJECT", true);
                        }
                    }
                    // clean up any leftover vol/dir files
                    if (EditGame.InterpreterVersion[0] == '3') {
                        SafeFileDelete(CompGameResults.parm + EditGame.GameID + "DIR.NEW");
                    }
                    else {
                        SafeFileDelete(CompGameResults.parm + "LOGDIR.NEW");
                        SafeFileDelete(CompGameResults.parm + "PICDIR.NEW");
                        SafeFileDelete(CompGameResults.parm + "SNDDIR.NEW");
                        SafeFileDelete(CompGameResults.parm + "VIEWDIR.NEW");
                    }
                    foreach (string file in Directory.GetFiles(CompGameResults.parm, "NEW_VOL.*")) {
                        SafeFileDelete(file);
                    }
                    MDIMain.UseWaitCursor = false;
                    string strTemp = "";
                    if (CompGameResults.Status == CompileStatus.Error) {
                        // rebuild resource list
                        BuildResourceTree();
                        strTemp = "An error occurred while building game files:\n\n" + 
                            errmsg.Text +
                            "\n\nOriginal files have been restored, but you should " + 
                            "check all files to make sure nothing " +
                            "was lost or corrupted.";
                    }
                    else {
                        // cancelled
                        strTemp = (CompGameResults.Mode == CompileMode.RebuildOnly ? "Rebuild" : "Compile") + " canceled. No changes made to game files.";
                    }
                    MessageBox.Show(MDIMain,
                        strTemp,
                        CompGameResults.Mode == CompileMode.RebuildOnly ? "Rebuild VOL Files" : "Compile Game",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                }
                break;
            case CompileMode.ChangedLogics:
                switch (CompGameResults.Status) {
                case CompileStatus.OK:
                    //everything is ok
                    MDIMain.UseWaitCursor = false;
                    MessageBox.Show(MDIMain,
                        "All logics compiled successfully.",
                        "Compile Logics",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                case CompileStatus.Error:
                    MessageBox.Show(MDIMain,
                        "One or more errors occurred while compiling logics. Not all logics have " +
                        "been compiled.",
                        "Compile Logics",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                case CompileStatus.Canceled:
                        // cancelled
                    MessageBox.Show(MDIMain,
                        "Logic compile action canceled. Not all logics were compiled.",
                        "Compile Logics",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                }
                break;
            case CompileMode.ChangeVersion:
                // change version rebuild
                if (CompGameResults.Status != CompileStatus.OK) {
                    ErrMsgBox(CompGameResults.CompExc, "Error during version change: ", "Original version has been restored.", "Change Interpreter Version");
                }
                else {
                    // TODO: need better way to check for errors and warnings
                    // (add count values to the results object?)
                    // check for errors and warnings
                    if (int.Parse(CompStatusWin.lblErrors.Text) + int.Parse(CompStatusWin.lblWarnings.Text) > 0) {
                        MessageBox.Show(MDIMain,
                            "Errors and/or warnings were generated during game rebuild.",
                            "Version Change Rebuild",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        if (int.Parse(CompStatusWin.lblErrors.Text) > 0) {
                            // reuild resource list if there were errors
                            BuildResourceTree();
                        }
                        if (int.Parse(CompStatusWin.lblWarnings.Text) > 0) {
                            if (!MDIMain.pnlWarnings.Visible) {
                                MDIMain.pnlWarnings.Visible = true;
                            }
                        }
                    }
                    else {
                        // everything is ok
                        MessageBox.Show("Version change and rebuild completed successfully.",
                        "Change Interpreter Version", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    }
                }
                break;
            }
        }
    }
}
