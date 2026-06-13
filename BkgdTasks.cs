using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WinAGI.Editor;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.AGIResType;

namespace WinAGI.Common {

    public class BkgdTasks {
        #region Fields
        internal static BackgroundWorker bgwCompGame = null;
        internal static BackgroundWorker bgwNewGame = null;
        internal static BackgroundWorker bgwOpenGame = null;
        internal static bool updating;

        internal static BackgroundWorker bgwMakePicGif = null;
        #endregion

        #region NewGame
        public static void NewGameDoWork(object sender, DoWorkEventArgs e) {
            bool warnings = false;
            GameParams argval = (GameParams)e.Argument;
            bool Loaded;

            updating = false;
            try {
                // create new game
                EditGame = new(argval);
                // no errors, means game was successfully created
                Loaded = true;
            }
            catch (WinAGIException wex) {
                // error always causes failure
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                Loaded = false;
                switch (wex.HResult & 0xffff) {
                // these are the only NewGame errors
                case 508: // critical gamefile missing
                case 527: // no wag file in template directory
                case 533: // Unable to copy template files to game directory due to error: %1
                case 534: // Unable to open the newly created game due to error: % 1
                case 535: // Can't create new game in a directory that already has a wag file
                case 536: // Can't create new game in a directory that already has AGI game files
                case 543: // multiple WAG files found
                case 544: // template is missing resource directory
                case 545: // Unable to move resource directory(% 1)
                case 547: // Error accessing wag file (%1)
                case 548: // Unsupported WinAGI version in template
                case 549: // Invalid WinAGI version in template
                    break;
                default:
                    // shouldn't ever happen...
                    Debug.Assert(false);
                    break;
                }
                argval.ErrorCode = wex.HResult & 0xffff;
                bgwNewGame.ReportProgress(0, "Error encountered, game not created");
                argval.Error = wex;
            }
            catch (Exception ex) {
                // error always causes failure
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                Loaded = false;
                // unknown error
                argval.ErrorCode = 507;
                argval.Error = new WinAGIException(Engine.Base.EngineResourceByNum(507).Replace(
                    ARG1, ex.HResult.ToString("x8")).Replace(
                    ARG2, ex.Message).Replace(
                    ARG3, ex.StackTrace));
                bgwNewGame.ReportProgress(0, "Error encountered, game not created");
            }
            if (Loaded) {
                argval.Failed = false;
                bgwNewGame.ReportProgress(55, "Game created successfully, setting up editors");
                argval.Warnings = warnings;
            }
            else {
                argval.Failed = true;
            }
            e.Result = argval;
        }

        public static void NewGameProgressChanged(object sender, ProgressChangedEventArgs e) {
            // progress percentage used to identify event types
            switch (e.ProgressPercentage) {
            case 1:
                // load warning/error
                MDIMain.AddInfoItem((WinAGIEventInfo)e.UserState, true);
                break;
            case 2:
                // TODOs
                MDIMain.AddInfoItem((WinAGIEventInfo)e.UserState, true);
                break;
            case 3:
                // Decode warning
                MDIMain.AddInfoItem((WinAGIEventInfo)e.UserState, true);
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
                MDIMain.BuildResourceTree();
                // show it, if needed
                if (WinAGISettings.ResListType.Value != ResListType.None) {
                    // show resource tree pane
                    MDIMain.ShowResTree();
                }
                switch (WinAGISettings.ResListType.Value) {
                case ResListType.TreeList:
                    // select root
                    MDIMain.tvwResources.SelectedNode = MDIMain.tvwResources.Nodes[0];
                    break;
                case ResListType.ComboList:
                    // select root
                    MDIMain.cmbResType.SelectedIndex = 0;
                    // update selected resource
                    MDIMain.SelectResource(Game, -1);
                    break;
                }
                if (updating) {
                    // should not be possible
                    Debug.Assert(!updating);
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
            ProgressWin.Invalidate();
        }

        public static void NewGameWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // new game action completed
            ProgressWin.Close();
            ProgressWin.Dispose();
            MDIMain.Refresh();
            GameParams NewGameArgs = (GameParams)e.Result;
            if (NewGameArgs.Failed) {
                // use error info to determine messaging and actions
                switch (NewGameArgs.ErrorCode) {
                case 508: // critical gamefile missing
                case 527: // no wag file in template directory
                case 533: // Unable to copy template files to game directory due to error: %1
                case 534: // Unable to open the newly created game due to error: % 1
                case 535: // Can't create new game in a directory that already has a wag file
                case 536: // Can't create new game in a directory that already has AGI game files
                case 543: // multiple WAG files found
                case 544: // template is missing resource directory
                case 545: // Unable to move resource directory(% 1)
                case 547: // Error accessing wag file (%1)
                case 548: // Unsupported WinAGI version in template
                case 549: // Invalid WinAGI version in template
                    ErrMsgBox(NewGameArgs.Error,
                        "Unable to create new game.",
                        "",
                        "New AGI Game Error",
                        "htm\\winagi\\loadgameerrors.htm#err" + NewGameArgs.ErrorCode.ToString());
                    break;
                default:
                    // shouldn't ever happen...
                    Debug.Assert(false);
                    ErrMsgBox(NewGameArgs.Error,
                        "Unable to create new game. UNHANDLED ERROR:",
                        NewGameArgs.Error.StackTrace,
                        "New AGI Game Error",
                        "htm\\winagi\\loadgameerrors.htm");
                    break;
                }
            }
            else {
                if (NewGameArgs.Warnings) {
                    MessageBox.Show(MDIMain,
                        "Some errors and/or anomalies in resource data were encountered in the template.",
                        "Template Game Anomalies",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
        #endregion

        #region OpenGame
        public static void OpenGameDoWork(object sender, DoWorkEventArgs e) {
            bool warnings = false;
            LoadGameResults argval = (LoadGameResults)e.Argument;
            bool loaded;

            updating = false;
            try {
                // if game can't be loaded, constructor ALWAYS returns error
                EditGame = new AGIGame(argval.Parameters);
                // no errors, means game loaded OK
                loaded = true;
                warnings = EditGame.LoadWarnings;
            }
            catch (WinAGIException wex) {
                // errors always causes failure to load
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                loaded = false;
                argval.ErrorCode = wex.HResult & 0xffff;
                argval.Error = wex;
                switch (wex.HResult & 0xffff) {
                case 504: // invalid game directory
                case 505: // invalid DIR file
                case 529: // missing game property file
                case 530: // invalid game property file version
                case 537: // invalid gameID in game property file
                case 538: // unsupported AGI version in game property file
                case 539: // game file '%1' is readonly
                case 540: // file error accessing WAG
                case 541: // DIR file access error
                case 546: // Unable to create wag file (%1)
                case 555: // invalid data in game property file
                    break;
                default:
                    // shouldn't ever happen...
                    Debug.Assert(false);
                    break;
                }
            }
            catch (FileNotFoundException fex) {
                // errors always causes failure to load
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                loaded = false;
                // file not found:
                argval.ErrorCode = 508;
                argval.Error = new WinAGIException(Engine.Base.EngineResourceByNum(508).Replace(
                    ARG1, Path.GetFileName(fex.Message))) {
                    HResult = 508
                };
            }
            catch (Exception ex) {
                // errors always causes failure to load
                // ALWAYS release the game object if it fails to load
                EditGame = null;
                loaded = false;
                // unknown error
                argval.ErrorCode = 507;
                argval.Error = new WinAGIException(Engine.Base.EngineResourceByNum(507).Replace(
                    ARG1, ex.HResult.ToString("x8")).Replace(
                    ARG2, ex.Message).Replace(
                    ARG3, ex.StackTrace));
            }
            if (loaded) {
                argval.Failed = false;
                bgwOpenGame.ReportProgress(50, "Game " + (argval.Parameters.Mode == OpenGameMode.File ? "loaded" : "imported") + " successfully, setting up editors");
                argval.Warnings = warnings;
            }
            else {
                argval.Failed = true;
            }
            e.Result = argval;
        }

        public static void OpenGameProgressChanged(object sender, ProgressChangedEventArgs e) {
            // progress percentage used to identify different types of events

            switch (e.ProgressPercentage) {
            case 1:
                // resource error/warning
                MDIMain.AddInfoItem((WinAGIEventInfo)e.UserState, true);
                break;
            case 2:
                // TODOs
                MDIMain.AddInfoItem((WinAGIEventInfo)e.UserState, true);
                break;
            case 3:
                // Decode errors and warnings
                MDIMain.AddInfoItem((WinAGIEventInfo)e.UserState, true);
                break;
            case 4:
                // updating WinAGI version
                updating = true;
                break;
            case 50:
                // game finished loading
                MDIMain.Text = "WinAGI GDS - " + EditGame.GameID;
                MDIMain.BuildResourceTree();
                foreach (var include in EditGame.IncludeFiles) {
                    IncludeDefines.Add(include.Filename, new() {
                        IsChanged = true
                    });
                }
                if (WinAGISettings.ResListType.Value != ResListType.None) {
                    MDIMain.ShowResTree();
                }
                // setting reslist type will also show PreviewWin if needed
                switch (WinAGISettings.ResListType.Value) {
                case ResListType.TreeList:
                    MDIMain.tvwResources.SelectedNode = MDIMain.tvwResources.Nodes[0];
                    break;
                case ResListType.ComboList:
                    MDIMain.cmbResType.SelectedIndex = 0;
                    MDIMain.SelectResource(Game, -1);
                    break;
                }
                if (updating) {
                    MDIMain.MsgBoxWithHelp(
                        "This game was last opened with an older version of WinAGI. " +
                        "Logic source files need to be updated.\n\n" +
                        "Your original source files will be moved to a backup folder " +
                        "in your source directory.",
                        "Updating WAG File to New Version",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\winagi\\opengame.htm#upgrade");
                    try {
                        Directory.CreateDirectory(Path.Combine(EditGame.SrcResDir, "BACKUP"));
                    }
                    catch {
                        // ignore exceptions
                    }
                    foreach (Logic logic in EditGame.Logics) {
                        if (File.Exists(logic.SourceFile)) {
                            try {
                                File.Copy(logic.SourceFile, Path.Combine(EditGame.SrcResDir, "BACKUP", Path.GetFileName(logic.SourceFile)), true);
                                byte[] strdat = File.ReadAllBytes(logic.SourceFile);
                                bool _ = false;
                                string srcText = RefreshAutoIncludes(Encoding.GetEncoding(EditGame.CodePage).GetString(strdat), EditGame, ref _);
                                File.WriteAllText(logic.SourceFile, srcText);
                            }
                            catch {
                                // ignore exceptions
                            }
                        }
                    }
                    // author changed to designer
                    string author = EditGame.agGameProps.GetSetting("General", "Author", "", true);
                    if (author.Length > 0) {
                        EditGame.agDesigner = author;
                        EditGame.agGameProps.WriteSetting("General", "Designer", author);
                    }
                    EditGame.agGameProps.DeleteKey("General", "Author");
                    // UseResNames no longer used
                    EditGame.agGameProps.DeleteKey("General", "UseResNames");
                    EditGame.agGameProps.Save();
                }
                break;
            default:
                // all other progress events just update the label text
                ProgressWin.lblProgress.Text = e.UserState.ToString();
                break;
            }
            ProgressWin.Invalidate();
        }

        public static void OpenGameWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            // load is over
            ProgressWin.Close();
            ProgressWin.Dispose();
            // refresh results
            LoadGameResults LoadResults = (LoadGameResults)e.Result;
            if (LoadResults.Failed) {
                MDIMain.MsgBoxWithHelp(
                    "ERROR " + LoadResults.ErrorCode.ToString() + ":\n\n" +
                    LoadResults.Error.Message,
                    "Unable to " + (LoadResults.Parameters.Mode == OpenGameMode.File ? "Open" : "Import") + " Game",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    "htm\\winagi\\loadgameerrors.htm#err" + LoadResults.ErrorCode.ToString());
            }
            else {
                // if importing, update all logics that have decompile warnings or errors
                if (LoadResults.Parameters.Mode == OpenGameMode.Directory) {
                    // step through all logics and update warning/error list
                    foreach (Logic logic in EditGame.Logics) {
                        RefreshLogicWarnings(logic.Number, false);
                    }
                    RefreshModWarnings(false);
                    // now save the save game file
                    EditGame.SaveProperties();
                }
                if (LoadResults.Warnings && WinAGISettings.NotifyLoadWarn.Value) {
                    bool dontNotify = false;
                    MsgBoxEx.Show(MDIMain,
                        "Some errors and/or anomalies in resource data were encountered.",
                        "Anomalies Detected During " + (LoadResults.Parameters.Mode == OpenGameMode.File ? "Load" : "Import"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "Don't show this message again", ref dontNotify,
                        WinAGIHelp, "");
                    WinAGISettings.NotifyLoadWarn.Value = !dontNotify;
                    if (!WinAGISettings.NotifyLoadWarn.Value) {
                        WinAGISettings.NotifyLoadWarn.WriteSetting(WinAGISettingsFile);
                    }
                }
            }
        }
        #endregion

        #region CompileGame
        private static bool warnings = false;

        public static void CompileGameDoWork(object sender, DoWorkEventArgs e) {
            CompileGameResults argval = (CompileGameResults)e.Argument;
            CompileStatus results = CompileStatus.OK;
            warnings = false;
            try {
                switch (argval.Mode) {
                case CompileMode.Full:
                    // full compile
                    results = EditGame.Compile(false, argval.parm);
                    Debug.Assert(!Compiling);
                    break;
                case CompileMode.RebuildOnly:
                    // normal rebuild
                    results = EditGame.Compile(true);
                    Debug.Assert(!Compiling);
                    break;
                case CompileMode.ChangedLogics:
                    results = EditGame.CompileChangedLogics();
                    break;
                case CompileMode.ChangeVersion:
                    // change version rebuild
                    EditGame.InterpreterVersion = new AGIVersionInfo() {
                        Index = (AGIVersion)Array.IndexOf(Engine.Base.IntVersions, argval.parm),
                    };
                    break;
                }
                // pass back results status
                argval.Status = results;
                argval.Warnings = warnings;
                e.Result = argval;
            }
            catch (Exception ex) {
                argval.Status = CompileStatus.ResourceError;
                argval.CompExc = ex;
                e.Result = argval;
            }
        }

        public static void CompileGameProgressChanged(object sender, ProgressChangedEventArgs e) {
            WinAGIEventInfo compInfo = (WinAGIEventInfo)e.UserState;

            switch ((GameCompileStatus)e.ProgressPercentage) {
            case GameCompileStatus.CompileWords:
                switch (compInfo.InfoType) {
                case InfoType.Resources:
                    CompStatusWin.lblStatus.Text = "Compiling WORDS.TOK";
                    CompStatusWin.pgbStatus.Value++;
                    break;
                case InfoType.ClearWarnings:
                    MDIMain.ClearInfoGrid(compInfo.ResType, compInfo.ResNum);
                    break;
                }
                break;
            case GameCompileStatus.CompileObjects:
                switch (compInfo.InfoType) {
                case InfoType.Resources:
                    CompStatusWin.lblStatus.Text = "Compiling OBJECT";
                    CompStatusWin.pgbStatus.Value++;
                    break;
                case InfoType.ClearWarnings:
                    MDIMain.ClearInfoGrid(compInfo.ResType, compInfo.ResNum);
                    break;
                }
                break;
            case GameCompileStatus.AddResource:
                switch (compInfo.InfoType) {
                case InfoType.ClearWarnings:
                    // clear warnings and errors for this logic
                    MDIMain.ClearInfoGrid(compInfo.ResType, compInfo.ResNum, [EventType.LogicCompileError, EventType.LogicCompileWarning, EventType.ResourceError, EventType.ResourceWarning]);
                    break;
                case InfoType.Compiling:
                    // compiling a logic a resource
                    CompStatusWin.lblStatus.Text = "Compiling logic: " + compInfo.Text;
                    break;
                case InfoType.Compiled:
                    RefreshTree(compInfo.ResType, compInfo.ResNum);
                    if (compInfo.ResType == AGIResType.Logic) {
                        RefreshLogicWarnings(compInfo.ResNum, false);
                    }
                    MDIMain.Invalidate();
                    break;
                case InfoType.Resources:
                    // adding a resource
                    CompStatusWin.lblStatus.Text = "Adding resource: " + compInfo.Module;
                    CompStatusWin.pgbStatus.Value++;
                    break;
                }
                break;
            case GameCompileStatus.DoneAdding:
                break;
            case GameCompileStatus.CompileComplete:
                RefreshModWarnings(false);
                CompStatusWin.lblStatus.Text = "Creating DIR file entries";
                break;
            case GameCompileStatus.Warning:
                if (compInfo.Type == EventType.Info) {
                    // clear includefile warnings
                    MDIMain.ClearInfoGrid(compInfo.Filename);
                }
                else {
                    // warning generated
                    warnings = true;
                    int warncount = CompStatusWin.Warnings;
                    warncount++;
                    CompStatusWin.lblWarnings.Text = warncount.ToString();
                    CompStatusWin.Warnings = warncount;
                    CompStatusWin.lblStatus.Text = compInfo.Text;
                    MDIMain.AddInfoItem(compInfo, true);
                }
                break;
            case GameCompileStatus.ResError:
            case GameCompileStatus.LogicError:
                // error encountered
                // (need to copy errors value to avoid
                // marshalling errors see CS1960)
                int errors = CompStatusWin.Errors;
                errors++;
                CompStatusWin.lblErrors.Text = errors.ToString();
                CompStatusWin.Errors = errors;
                if (compInfo.Type != EventType.Info) {
                    MDIMain.AddInfoItem(compInfo, true);
                }
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
            // using Refresh() causes the UI to freeze
            CompStatusWin.Invalidate();
        }

        public static void CompileGameWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            WinAGIEventInfo errmsg = CompStatusWin.FatalError;
            string message = "";
            CompStatusWin.Close();
            CompStatusWin?.Dispose();

            // refresh results
            CompGameResults = (CompileGameResults)e.Result;
            switch (CompGameResults.Mode) {
            case CompileMode.Full:
            case CompileMode.RebuildOnly:
                if (CompGameResults.Status == CompileStatus.OK) {
                    // everything is ok
                    MDIMain.UseWaitCursor = false;
                    if (CompGameResults.Warnings) {
                        if (!MDIMain.pnlInfoGrid.Visible) {
                            MDIMain.pnlInfoGrid.Visible = true;
                        }
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
                }
                else {
                    // clean up any leftover vol/dir files
                    if (EditGame.InterpreterVersion.IsV3) {
                        SafeFileDelete(Path.Combine(CompGameResults.parm, EditGame.GameID + "DIR.NEW"));
                    }
                    else {
                        SafeFileDelete(Path.Combine(CompGameResults.parm, "LOGDIR.NEW"));
                        SafeFileDelete(Path.Combine(CompGameResults.parm, "PICDIR.NEW"));
                        SafeFileDelete(Path.Combine(CompGameResults.parm, "SNDDIR.NEW"));
                        SafeFileDelete(Path.Combine(CompGameResults.parm, "VIEWDIR.NEW"));
                    }
                    foreach (string file in Directory.GetFiles(CompGameResults.parm, "NEW_VOL.*")) {
                        SafeFileDelete(file);
                    }
                    MDIMain.UseWaitCursor = false;
                    switch (CompGameResults.Status) {
                    case CompileStatus.LogicCompileError:
                        // logic compile error
                        MDIMain.BuildResourceTree();
                        message = "One or more errors occurred while compiling logics. ";
                        break;
                    case CompileStatus.ResourceError:
                        // rebuild resource list
                        MDIMain.BuildResourceTree();
                        message = "An error occurred while building game files:\n\n" +
                            errmsg.Text +
                            "\n\nOriginal files have been restored, but you should " +
                            "check all files to make sure nothing " +
                            "was lost or corrupted.";
                        break;
                    case CompileStatus.Canceled:
                        // cancelled
                        message = (CompGameResults.Mode == CompileMode.RebuildOnly ? "Rebuild" : "Compile") + " canceled. No changes made to game files.";
                        break;
                    }
                    MessageBox.Show(MDIMain,
                        message,
                        CompGameResults.Mode == CompileMode.RebuildOnly ? "Rebuild VOL Files" : "Compile Game",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                break;
            case CompileMode.ChangedLogics:
                if (CompGameResults.Status == CompileStatus.OK) {
                    // check for warnings
                    if (CompStatusWin.Warnings > 0) {
                        message = "One or more warnings were generated while compiling changed logics.";
                    }
                    else {
                        // everything is ok
                        message = "All changed logics compiled successfully.";
                    }
                }
                else {
                    switch (CompGameResults.Status) {
                    case CompileStatus.LogicCompileError:
                        // logic compile error
                        MDIMain.BuildResourceTree();
                        message = "One or more errors occurred while compiling changed logics. ";
                        break;
                    case CompileStatus.ResourceError:
                        message = "One or more errors occurred while compiling changed logics. Not all logics have " +
                            "been compiled.\n\n" + errmsg.Text;
                        break;
                    case CompileStatus.Canceled:
                        // cancelled
                        message = "Logic compile action canceled. Not all logics were compiled.";
                        break;
                    }
                }
                MDIMain.UseWaitCursor = false;
                MessageBox.Show(MDIMain,
                    message,
                    "Compile Changed Logics",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                break;
            case CompileMode.ChangeVersion:
                // change version rebuild
                if (CompGameResults.Status != CompileStatus.OK) {
                    ErrMsgBox(CompGameResults.CompExc,
                        "Error during version change: ",
                        CompGameResults.CompExc.StackTrace + "\n\nOriginal version has been restored.",
                        "Change Interpreter Version");
                }
                else {
                    // check for errors and warnings
                    if (CompStatusWin.Errors + CompStatusWin.Warnings > 0) {
                        MessageBox.Show(MDIMain,
                            "Errors and/or warnings were generated during game rebuild.",
                            "Version Change Rebuild",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        if (CompStatusWin.Errors > 0) {
                            // reuild resource list if there were errors
                            MDIMain.BuildResourceTree();
                        }
                        if (CompStatusWin.Warnings > 0) {
                            if (!MDIMain.pnlInfoGrid.Visible) {
                                MDIMain.pnlInfoGrid.Visible = true;
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
        #endregion

        #region MakeGif
        public static void BuildPicGifDoWork(object sender, DoWorkEventArgs e) {
            MakeGifParams args = (MakeGifParams)e.Argument;
            if (args.Mode == 0) {
                if (MakePicGif(args.Picture, args.GifOptions, args.Filename)) {
                    e.Result = 1;
                    return;
                }
            }
            else {
                if (MakeLoopGif(args.Loop, args.GifOptions, args.Filename)) {
                    e.Result = 2;
                    return;
                }
            }
            e.Result = 0;
        }

        public static void BuildPicGifProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (e.ProgressPercentage >= 0) {
                ProgressWin.pgbStatus.Value = e.ProgressPercentage;
                ProgressWin.Refresh();
            }
            else {
                ErrMsgBox((Exception)e.UserState,
                    "Unable to create gif file.",
                    ((Exception)e.UserState).StackTrace,
                    $"Make {((int)e.ProgressPercentage == -1 ? "Picture" : "Loop")} Gif Failure");
            }
        }

        public static void BuildPicGifWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            ProgressWin.Hide();
            ProgressWin.Dispose();
            MDIMain.UseWaitCursor = false;
            if ((int)e.Result > 0) {
                MessageBox.Show(MDIMain,
                    "Success!",
                    $"Export {((int)e.Result == 1 ? "Picture" : "Loop")} as GIF",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        #endregion
    }
}
