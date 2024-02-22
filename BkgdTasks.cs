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
using static WinAGI.Engine.AGIResType;
using System.IO;

namespace WinAGI.Editor
{
    class BkgdTasks
    {
        internal static BackgroundWorker bgwOpenGame = null;
        public static void OpenGameBkgd(object sender, DoWorkEventArgs e)
        {
            bool blnLoaded = false;
            string strError = "";
            bool blnWarnings = false;
            int lngErr;
            LoadGameResults argval = (LoadGameResults)e.Argument;
            try {
                //and load the game/dir
                // TODO: need to re-do game management; games should ALWAYS be loaded; 
                // when created, they are either NEW, opened from WAG file, or imported from
                // DIR; use overloads in the initializer function

                if (argval.Mode == 0) {
                    EditGame = new AGIGame(OpenGameMode.File, argval.Source);
                }
                else {
                    EditGame = new AGIGame(OpenGameMode.Directory, argval.Source);
                }
                blnLoaded = true;
            }
            catch (Exception ex) {
                //catch any errors/warnings that were returned
                lngErr = ex.HResult;
                strError = ex.Message;
                switch (lngErr) {
                case WINAGI_ERR + 636: //warnings only- loaded ok;
                    blnWarnings = true;
                    blnLoaded = true;
                    break;
                default:
                    //ProgressWin.lblProgress.Text = "Error encountered, game not loaded";
                    bgwOpenGame.ReportProgress(0, "Error encountered, game not loaded");
                    //error
                    switch (lngErr - WINAGI_ERR) {
                    case 501:
                        strError = "A game is already loaded. Close it before opening another game.";
                        break;
                    case 502:
                        strError = "A file access error occurred while trying to open this game: " + Environment.NewLine + Environment.NewLine + strError;
                        break;
                    case 524:
                        strError = "A critical game file (" + Path.GetFileName(strError) + " is missing.";
                        break;
                    case 541:
                        strError = "Missing or invalid directory '" + strError + "'";
                        break;
                    case 542:
                        strError = "'" + Left(strError, strError.Length - 31) + "' is an invalid directory file.";
                        break;
                    case 543:
                        //invalid interpreter version - couldn't find correct version from the AGI
                        //files; current error string is sufficient
                        break;
                    case 545:
                        //resource loading error; current error string is sufficient
                        break;
                    case 597:
                        strError = "WinAGI GDS only supports version 2 and 3 of AGI.";
                        break;
                    case 655:
                        strError = "Missing game property file (" + strError + ").";
                        break;
                    case 665:
                        strError = "Invalid or corrupt game property file (" + strError + ").";
                        break;
                    case 690:
                        //invalid gameID in wag file
                        strError = "Game property file does not contain a valid GameID.";
                        break;
                    case 691:
                        //invalid intVersion in wag file
                        strError = "Game property file does not contain a valid Interpreter Version.";
                        break;
                    }
                    break;
                }
            }
            // if loaded OK, 
            if (blnLoaded) {
                //ProgressWin.lblProgress.Text = "Game " + (mode == 0 ? "loaded" : "imported") + " successfully, setting up editors";
                bgwOpenGame.ReportProgress(50, "Game " + (argval.Mode == 0 ? "loaded" : "imported") + " successfully, setting up editors");
                //MDIMain.Text = "WinAGI GDS - " + EditGame.GameID;
                ////build resource list
                //BuildResourceTree();
                //// show it, if needed
                //if (Settings.ResListType != 0) {
                //  // show resource tree pane
                //  MDIMain.ShowResTree();
                //  //ok up to here
                //}
                //switch (Settings.ResListType) {
                //case 1: //tree
                //  //select root
                //  MDIMain.tvwResources.SelectedNode = MDIMain.tvwResources.Nodes[0];
                //  //update selected resource
                //  MDIMain.SelectResource(rtGame, -1);
                //  //set LastNodeName property
                //  MDIMain.LastNodeName = RootNode.Name;
                //  break;
                //case 2:
                //  //select root
                //  MDIMain.cmbResType.SelectedIndex = 0;
                //  //update selected resource
                //  MDIMain.SelectResource(rtGame, -1);
                //  break;
                //}
                //// show selection in preview, if needed
                //if (Settings.ShowPreview) {
                //  PreviewWin.Show();
                //}
                //set default directory
                BrowserStartDir = EditGame.GameDir;
                //store game file name
                CurGameFile = EditGame.GameFile;
                //set default text file directory to game source file directory
                DefaultResDir = EditGame.GameDir + EditGame.ResDirName + "\\";
                // after a game loads, colors may be different
                //done with ProgressWin form
                //ProgressWin.Close();
                ////if warnings
                //if (blnWarnings) {
                //  //warn about errors
                //  MessageBox.Show("Some errors in resource data were encountered. See errlog.txt in the game directory for details.", "Errors During " + (mode == 0 ? "Load" : "Import"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
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
                //done with ProgressWin form
                //ProgressWin.Close();
                ////show error message
                //MessageBox.Show(strError, "Unable to " + (mode == 0 ? "Open" : "Import") + " Game", MessageBoxButtons.OK, MessageBoxIcon.Error);
                argval.Failed = true;
                argval.ErrorMsg = strError;
            }
            e.Result = argval;
        }
        public static void bgw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
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
        }
        public static void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LoadGameResults results = (LoadGameResults)e.Result;
            //load is over
            ProgressWin.Close();

            if (results.Failed) {
                //show error message
                MessageBox.Show(results.ErrorMsg, "Unable to " + (results.Mode == 0 ? "Open" : "Import") + " Game", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else {
                //if warnings
                if (results.Warnings) {
                    //warn about errors
                    MessageBox.Show("Some errors in resource data were encountered. See errlog.txt in the game directory for details.", "Errors During " + (results.Mode == 0 ? "Load" : "Import"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
