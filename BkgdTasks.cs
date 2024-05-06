using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Common.Base;
using WinAGI.Common;
using static WinAGI.Engine.AGIResType;
using System.IO;
using System.Diagnostics;

namespace WinAGI.Editor {
    class BkgdTasks {
        internal static BackgroundWorker bgwOpenGame = null;
        public static void OpenGameBkgd(object sender, DoWorkEventArgs e) {
            bool blnLoaded = false;
            string strError = "";
            bool blnWarnings = false;
            int lngErr;
            LoadGameResults argval = (LoadGameResults)e.Argument;
            try {
                // load the game/dir
                // TODO: need to re-do game management; games should ALWAYS be loaded; 
                // if constructor successfully loads game, it returns
                // if game can't be loaded, constructor ALWAYS returns error
                if (argval.Mode == 0) {
                    EditGame = new AGIGame(OpenGameMode.File, argval.Source);
                }
                else {
                    EditGame = new AGIGame(OpenGameMode.Directory, argval.Source);
                }
                // if no errors, means game loaded OK
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
                    bgwOpenGame.ReportProgress(0, "Error encountered, game not loaded");
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
                        strError = $"Missing game property file ({ex.Data["badwag"]}).";
                        break;
                    case 665:
                        strError = $"Invalid WINAGE property file ({ex.Data["badversion"]}).";
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
                        // file / dir(WAG) is readonly
                        strError = "Game directory and/or WAG File is readonly";
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
                        break;
                    }
                }
                else {
                    // unknown error
                    strError = "UNKNOWN: " + lngErr.ToString() + " - " + ex.Source + " - " + strError;
                }
            }
            // if loaded OK, 
            if (blnLoaded) {
                //ProgressWin.lblProgress.Text = "Game " + (mode == 0 ? "loaded" : "imported") + " successfully, setting up editors";
                bgwOpenGame.ReportProgress(50, "Game " + (argval.Mode == 0 ? "loaded" : "imported") + " successfully, setting up editors");
                //set default directory
                BrowserStartDir = EditGame.GameDir;
                //store game file name
                CurGameFile = EditGame.GameFile;
                //set default text file directory to game source file directory
                DefaultResDir = EditGame.GameDir + EditGame.ResDirName + "\\";
                //build the lookup tables for logic tooltips
                BuildIDefLookup();
                BuildGDefLookup();
                //update the reserved lookup values
                RDefLookup[90].Value = QUOTECHAR + EditGame.GameVersion + QUOTECHAR;
                RDefLookup[91].Value = QUOTECHAR + EditGame.GameAbout + QUOTECHAR;
                RDefLookup[92].Value = QUOTECHAR + EditGame.GameID + QUOTECHAR;
                RDefLookup[93].Value = (EditGame.InvObjects.Count - 1).ToString();
                argval.Warnings = blnWarnings;
            }
            else {
                argval.Failed = true;
                argval.ErrorMsg = strError;
            }
            e.Result = argval;
        }
        public static void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            // progress percentage used to identify different types of events
            switch (e.ProgressPercentage) {
            case 1:
                // load warning
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

            default:
                ProgressWin.lblProgress.Text = e.UserState.ToString();
                if (e.ProgressPercentage == 50) {
                    MDIMain.Text = "WinAGI GDS - " + EditGame.GameID;
                    //build resource list
                    BuildResourceTree();
                    // show it, if needed
                    if (Settings.ResListType != 0) {
                        // show resource tree pane
                        MDIMain.ShowResTree();
                        //ok up to here
                    }
                    switch (Settings.ResListType) {
                    case 1: //tree
                            //select root
                        MDIMain.tvwResources.SelectedNode = MDIMain.tvwResources.Nodes[0];
                        //update selected resource
                        MDIMain.SelectResource(rtGame, -1);
                        //set LastNodeName property
                        MDIMain.LastNodeName = RootNode.Name;
                        break;
                    case 2:
                        //select root
                        MDIMain.cmbResType.SelectedIndex = 0;
                        //update selected resource
                        MDIMain.SelectResource(rtGame, -1);
                        break;
                    }
                    // show selection in preview, if needed
                    if (Settings.ShowPreview) {
                        PreviewWin.Show();
                    }
                }
                break;
            }

        }
        public static void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            //load is over
            ProgressWin.Close();

            // refresh results
            LoadResults = (LoadGameResults)e.Result;
            if (LoadResults.Failed) {
                //show error message
                MessageBox.Show(LoadResults.ErrorMsg, "Unable to " + (LoadResults.Mode == 0 ? "Open" : "Import") + " Game", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else {
                //if warnings
                if (LoadResults.Warnings) {
                    //warn about errors
                    MessageBox.Show("Some errors in resource data were encountered.", "Errors During " + (LoadResults.Mode == 0 ? "Load" : "Import"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        internal static BackgroundWorker bgwLoadLogic = null;
        // TODO: write event handlers to manage logic TODOs and warnings 
        // when 
    }
}
