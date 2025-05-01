using FastColoredTextBoxNS;
using NAudio.Wave;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using WinAGI.Common;
using static WinAGI.Common.Base;
using static WinAGI.Common.BkgdTasks;
using static WinAGI.Editor.Base;
using WinAGI.Engine;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.EventType;
using System.Text.RegularExpressions;

namespace WinAGI.Editor {
    public partial class frmMDIMain : Form {
        List<WarningGridInfo> WarningList = [];
        List<int> warncol = [2, 3, 4, 5, 6];
        int NLOffset, NLRow, NLRowHeight;
        public string LastNodeName;

        //property box height
        int PropPanelSplit;
        private bool splashDone;
        //tracks status of caps/num/ins
        static bool CapsLock = false;
        static bool NumLock = false;
        static bool InsertLock = false;

        public frmMDIMain() {
            InitializeComponent();
            //what is resolution?
            Debug.Print($"DeviceDPI: {this.DeviceDpi}");
            Debug.Print($"AutoScaleFactor: {this.AutoScaleFactor}");
            Debug.Print($"AutoscaleDimensions: {this.AutoScaleDimensions}");

            //use idle time to update caps/num/ins
            Application.Idle += new System.EventHandler(OnIdle);

            // save pointer to main form
            MDIMain = this;

            // controls setup
            fgWarnings.Columns[0].Visible = false;
            fgWarnings.Columns[1].Visible = false;

            fgWarnings.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            fgWarnings.Columns[2].FillWeight = 20F;
            fgWarnings.Columns[2].HeaderText = "Code";
            fgWarnings.Columns[2].MinimumWidth = 10;
            fgWarnings.Columns[2].ReadOnly = true;
            fgWarnings.Columns[2].Tag = false;

            fgWarnings.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            fgWarnings.Columns[3].FillWeight = 50F;
            fgWarnings.Columns[3].HeaderText = "Description";
            fgWarnings.Columns[3].MinimumWidth = 10;
            fgWarnings.Columns[3].ReadOnly = true;
            fgWarnings.Columns[3].Tag = false;

            fgWarnings.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            fgWarnings.Columns[4].FillWeight = 10F;
            fgWarnings.Columns[4].HeaderText = "Res#";
            fgWarnings.Columns[4].MinimumWidth = 10;
            fgWarnings.Columns[4].ReadOnly = true;
            fgWarnings.Columns[4].Tag = false;

            fgWarnings.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            fgWarnings.Columns[5].FillWeight = 10F;
            fgWarnings.Columns[5].HeaderText = "Line#";
            fgWarnings.Columns[5].MinimumWidth = 10;
            fgWarnings.Columns[5].ReadOnly = true;
            fgWarnings.Columns[5].Tag = false;

            fgWarnings.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            fgWarnings.Columns[6].FillWeight = 20F;
            fgWarnings.Columns[6].HeaderText = "Module";
            fgWarnings.Columns[6].MinimumWidth = 10;
            fgWarnings.Columns[6].ReadOnly = true;
            fgWarnings.Columns[6].Tag = false;

            fgWarnings.Columns[7].Visible = false;

            btnNewRes.DefaultItem = btnNewLogic;
            btnOpenRes.DefaultItem = btnOpenLogic;
            btnImportRes.DefaultItem = btnImportLogic;
            MainStatusBar = statusStrip1;
            FindingForm = new frmFind();

            //set property window split location based on longest word
            Size szText = TextRenderer.MeasureText(" Use Res Names ", propertyGrid1.Font);
            // set height based on text (assume padding of 3 pixels above/below)
            PropRowHeight = szText.Height + 6;
            splResource.Panel2MinSize = 3 * PropRowHeight;
            PropPanelMaxSize = 10 * PropRowHeight;
            // set grid row height
            fgWarnings.RowTemplate.Height = szText.Height + 2;
            // initialize the basic app functionality
            InitializeResMan();

            ProgramDir = FullDir(JustPath(Application.ExecutablePath));
            DefaultResDir = ProgramDir;
            //set browser start dir to program dir
            BrowserStartDir = ProgramDir;

            LogicEditors = [];
            ViewEditors = [];
            PictureEditors = [];
            SoundEditors = [];
        }

        #region Event Handlers
        #region Form Event Handlers
        private void frmMDIMain_Load(object sender, EventArgs e) {
            bool blnLastLoad;

            // hide resource and warning panels until needed
            pnlResources.Visible = false;
            pnlWarnings.Visible = false;

            // get game settings and set initial window positions
            // this happens after all the stuff above because
            // Readsettings affects things that need to be initialized first
            if (!ReadSettings()) {
                //problem with settings
                MessageBox.Show("Fatal error: Unable to read program settings", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            frmSplash splash = null;
            // show splash screen, if applicable
            if (WinAGISettings.ShowSplashScreen.Value) {
                Visible = true;
                Refresh();
                splash = new frmSplash();
                // the 'CenterParent' option for StartPosition property does NOT work...
                splash.Location = new Point(Left + (Width - splash.Width) / 2, Top + (Height - splash.Height) / 2);
                splash.Show(this);
                splash.Refresh();
            }
            // enable timer; it starts out as splash screen timer
            tmrNavList.Interval = 1750;
            tmrNavList.Enabled = true;

            // this creates a new form, adding it to MDIChildren collection
            // have to do this AFTER reading settings because they're used
            // by the form to set up defaults
            PreviewWin = new();
            ResetTreeList();
            //if using snippets
            if (WinAGISettings.UseSnippets.Value) {
                // build snippet table
                BuildSnippets();
            }

            WinAGIHelp = ProgramDir + "WinAGI.chm";

            // initialize resource treelist by using clear method
            ClearResourceList();

            // set navlist parameters
            //set property window split location based on longest word
            Size szText = TextRenderer.MeasureText(" Logic 1 ", new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value));
            NLRowHeight = szText.Height + 2;
            picNavList.Height = NLRowHeight * 5;
            picNavList.Top = (cmdBack.Top + cmdBack.Height / 2) - picNavList.Height / 2;

            // retrieve user's preferred AGI colors
            GetDefaultColors();

            // let the system catch up
            Refresh();

            if (WinAGISettings.ShowSplashScreen.Value) {
                // dont close unless ~1.75 seconds passed
                while (!splashDone) {
                    Application.DoEvents();
                }
                splash.Close();
                splash.Dispose();
            }

            // attach  AGI game events
            NewGameStatus += MDIMain.GameEvents_NewGameStatus;
            LoadGameStatus += MDIMain.GameEvents_LoadGameStatus;
            CompileGameStatus += MDIMain.GameEvents_CompileGameStatus;
            CompileLogicStatus += MDIMain.GameEvents_CompileLogicStatus;
            DecodeLogicStatus += MDIMain.GameEvents_DecodeLogicStatus;

            // check for command string
            CheckCmd();

            // was a game loaded when app was last closed
            blnLastLoad = WinAGISettingsFile.GetSetting(sMRULIST, "LastLoad", false);

            //if nothing loaded AND autoreload is set AND something was loaded last time program ended,
            if (EditGame is null && this.ActiveMdiChild is null && WinAGISettings.AutoOpen.Value && blnLastLoad) {
                // open mru0
                OpenMRUGame(0);
            }
        }

        private void frmMDIMain_KeyPress(object sender, KeyPressEventArgs e) {
            // after processing key press for the main form, pass it 
            // to preview window, if it's active
            if (!e.Handled) {
                if (ActiveMdiChild == PreviewWin) {
                    PreviewWin.KeyHandler(e);
                }
            }
        }

        private void frmMDIMain_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            Debug.Print($"Main - PreviewKeyDown: {e.KeyCode}; KeyData: {e.KeyData}; KeyModifiers: {e.Modifiers}");
        }

        private void frmMDIMain_KeyDown(object sender, KeyEventArgs e) {
            // this is only way I know to catch keypresses in the property grid...
            if (ActiveControl == splResource && splResource.ActiveControl == propertyGrid1) {
                // for some properties, direct editing not allowed
                switch (propertyGrid1.SelectedGridItem.Label) {
                case "GameID":
                    // only alphanumeric, backspace, delete, return
                    switch (e.KeyValue) {
                    case (>= 65 and <= 90) or (>= 97 and <= 122):
                    case >= 48 and <= 57:
                    case 8:
                    case 13:
                    case 46:
                        break;
                    default:
                        e.SuppressKeyPress = true;
                        break;
                    }
                    break;
                case "GlobalDef":
                case "Number":
                case "ID":
                case "Description":
                case "ViewDesc":
                    // never allow direct editing
                    e.SuppressKeyPress = true;
                    break;
                }
            }

            // TODO: need to supress ctrl+ keycodes that aren't valid menu calls to stop the ding
            // but... only on forms where those keys aren't required for that form 
            if (ActiveMdiChild == null) {
                if (e.KeyCode == Keys.Enter) {
                    e.SuppressKeyPress = true;
                }
                if (e.Control) {
                    switch (e.KeyCode) {
                    case Keys.F:
                        SearchForID();
                        break;
                    }
                    e.SuppressKeyPress = true;
                }
            }
            else {
                switch (ActiveMdiChild.Name) {
                case "frmLogicEdit":
                    // no keypresses are suppressed
                    break;
                }
            }
        }

        private void frmMDIMain_MdiChildActivate(object sender, EventArgs e) {
            // update statusbar
            ResetStatusStrip();
            if (this.ActiveMdiChild != null) {
                // !!!
                // statusstrip merging SUCKS... so we do it manually
                // !!!
                MergeStatusStrip(statusStrip1, this.ActiveMdiChild);
            }
            // update toolbar
            if (MDIMain.ActiveMdiChild == null) {
                UpdateTBResourceBtns(AGIResType.None, false, false);
            }
            else {
                dynamic form = MDIMain.ActiveMdiChild;
                bool ingame = false;
                bool changed = false;
                AGIResType restype = AGIResType.None;

                switch (MDIMain.ActiveMdiChild.Name) {
                case "frmPreview":
                    UpdateTBResourceBtns(SelResType, true, false);
                    break;
                case "frmLogicEdit":
                case "frmPicEdit":
                case "frmSoundEdit":
                case "frmViewEdit":
                case "frmGlobals":
                    switch (MDIMain.ActiveMdiChild.Name) {
                    case "frmLogicEdit":
                        restype = AGIResType.Logic;
                        break;
                    case "frmPicEdit":
                        restype = AGIResType.Picture;
                        break;
                    case "frmSoundEdit":
                        restype = AGIResType.Sound;
                        break;
                    case "frmViewEdit":
                        restype = AGIResType.View;
                        break;
                    case "frmGlobals":
                        restype = AGIResType.Globals;
                        break;
                    }
                    ingame = form.InGame;
                    changed = form.IsChanged;
                    if (MDIMain.ActiveMdiChild.Name == "frmLogicEdit") {
                        if (((frmLogicEdit)MDIMain.ActiveMdiChild).FormMode == LogicFormMode.Text) {
                            restype = AGIResType.Text;
                        }
                    }
                    UpdateTBResourceBtns(restype, ingame, changed);
                    break;
                case "frmLayout":
                    changed = form.IsChanged;
                    UpdateTBResourceBtns(AGIResType.Layout, true, changed);
                    break;
                case "frmObjectEdit":
                    changed = form.IsChanged;
                    UpdateTBResourceBtns(AGIResType.Objects, false, changed);
                    break;
                case "frmWordsEdit":
                    changed = form.IsChanged;
                    UpdateTBResourceBtns(AGIResType.Words, false, changed);
                    break;
                case "frmTextScreenEdit":
                    changed = form.IsChanged;
                    UpdateTBResourceBtns(AGIResType.TextScreen, false, changed);
                    break;
                default:
                    UpdateTBResourceBtns(AGIResType.None, false, false);
                    break;
                }
            }
            // hide preview window if needed
            if (WinAGISettings.ShowPreview.Value && WinAGISettings.HidePreview.Value) {
                if (MDIMain.ActiveMdiChild != null && MDIMain.ActiveMdiChild != PreviewWin) {
                    PreviewWin.Hide();
                }
            }
        }

        private void frmMDIMain_FormClosing(object sender, FormClosingEventArgs e) {
            bool blnLastLoad;

            // !!!! for MDI forms, the Cancel property seems to always be set to true
            // by default - set it to false explicitly, THEN do closegame check...
            e.Cancel = false;
            if (EditGame is null) {
                blnLastLoad = false;
            }
            else {
                blnLastLoad = true;
            }
            if (blnLastLoad) {
                try {
                    e.Cancel = !CloseThisGame();
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "An error occurred while trying to close:", "", "Critical Error");
                }
            }
            if (e.Cancel) {
                return;
            }
            WinAGISettingsFile.WriteSetting(sMRULIST, "LastLoad", blnLastLoad);
            SaveSettings();

            // detach  AGI game events
            NewGameStatus -= MDIMain.GameEvents_NewGameStatus;
            LoadGameStatus -= MDIMain.GameEvents_LoadGameStatus;
            CompileGameStatus -= MDIMain.GameEvents_CompileGameStatus;
            CompileLogicStatus -= MDIMain.GameEvents_CompileLogicStatus;
            DecodeLogicStatus -= MDIMain.GameEvents_DecodeLogicStatus;
            // dispose all
            MDIMain = null;
            this.Dispose();
            PreviewWin?.Dispose();
            ProgressWin?.Dispose();
            CompStatusWin?.Dispose();
            ObjectEditor?.Dispose();
            WordEditor?.Dispose();
            LayoutEditor?.Dispose();
            GlobalsEditor?.Dispose();
            MenuEditor?.Dispose();
            SnippetForm?.Dispose();
            FindingForm?.Dispose();
            bgwCompGame?.Dispose();
            bgwNewGame?.Dispose();
            bgwOpenGame?.Dispose();
            Application.Exit();
        }

        public void OnIdle(object sender, EventArgs e) {
            //string msg = MDIMain.ActiveMdiChild == null ? "none" : MDIMain.ActiveMdiChild.Name;
            //Debug.Print($"activemdichild: {msg}");

            //bool f = false;
            //for (int i = 0; i < MDIMain.OwnedForms.Length; i++) {
            //    if (MDIMain.OwnedForms[i].Focused) {
            //        msg = MDIMain.OwnedForms[i].Name;
            //        f = true;
            //        break;
            //    }
            //}
            //if (!f) {
            //    msg = "none";
            //}
            //Debug.Print($"owned focus: {msg}");

            //msg = Form.ActiveForm == null ? "none" : Form.ActiveForm.Name;
            // Debug.Print($"active form: {msg}");

            //bool g = false;
            //for (int i = 0; i < MDIMain.MdiChildren.Length; i++) {
            //    if (MDIMain.MdiChildren[i].Focused) {
            //        msg = MDIMain.MdiChildren[i].Name;
            //        g = true;
            //        break;
            //    }
            //}
            //if (!g) {
            //    msg = "none";
            //}
            //Debug.Print($"mdichild focus : {msg}");
            //Debug.Print("loged has focus: " + MDIMain.MdiChildren[1].Focused);


            // Update the 'lock' panels when the program is idle
            // unless panel is in a window that doesn't show them
            if (MainStatusBar.Items.ContainsKey(nameof(spCapsLock))) {
                bool newCapsLock = Console.CapsLock;
                bool newNumLock = Console.NumberLock;
                bool newInsertLock = IsKeyLocked(Keys.Insert);
                if (newCapsLock != CapsLock) {
                    CapsLock = newCapsLock;
                    if (spCapsLock is not null) {
                        spCapsLock.Text = CapsLock ? "CAP" : "\t";
                    }
                }
                if (newNumLock != NumLock) {
                    NumLock = newNumLock;
                    if (spNumLock is not null) {
                        spNumLock.Text = NumLock ? "NUM" : "\t";
                    }
                }
                if (newInsertLock != InsertLock) {
                    InsertLock = newInsertLock;
                    if (spInsLock is not null) {
                        spInsLock.Text = InsertLock ? "INS" : "\t";
                    }
                }
            }

            // horrible hack to prevent context menu strip from showing in
            // propertygrid text boxes
            var gridtext = MDIMain.propertyGrid1.ActiveControl as TextBox;
            if (gridtext != null) {
                if (gridtext.ContextMenuStrip == null) {
                    Debug.Print("Resetting context menu");
                    // TODO: create new menu strip that properly displays 
                    // cut/copy/paste/select all based on which property is selected
                    gridtext.ContextMenuStrip = new ContextMenuStrip();
                }
            }

        }
        #endregion

        #region Menu Item Event Handlers
        #region Game Menu
        private void mnuGame_DropDownOpening(object sender, EventArgs e) {
            mnuGClose.Enabled = EditGame != null;
            mnuGCompile.Enabled = EditGame != null;
            mnuGCompileTo.Enabled = EditGame != null;
            mnuGRun.Enabled = EditGame != null;
            mnuGRebuild.Enabled = EditGame != null;
            mnuGCompileChanged.Enabled = EditGame != null;
            mnuGProperties.Enabled = EditGame != null;
            mnuRImport.Enabled = EditGame != null;
        }

        private void mnuGNewTemplate_Click(object sender, EventArgs e) {
            // create new game using template
            NewAGIGame(true);
        }

        private void mnuGNewBlank_Click(object sender, EventArgs e) {
            // create new blank game
            NewAGIGame(false);
        }

        private void mnuGImport_Click(object sender, EventArgs e) {
            // import a game by directory
            OpenDIR();
        }

        private void mnuGOpen_Click(object sender, EventArgs e) {
            // open a game
            OpenWAGFile();
        }

        private void mnuGClose_Click(object sender, EventArgs e) {
            if (CloseThisGame()) {
                LastNodeName = "";
            }
        }

        private void mnuGCompile_Click(object sender, EventArgs e) {
            CompileGame(EditGame.GameDir);
        }

        private void mnuGCompileTo_Click(object sender, EventArgs e) {
            CompileGame();
        }

        private void mnuGRebuild_Click(object sender, EventArgs e) {
            CompileGame(EditGame.GameDir, true);
        }

        private void mnuGCompileChanged_Click(object sender, EventArgs e) {
            CompileChangedLogics();
        }

        private void mnuGRun_Click(object sender, EventArgs e) {
            RunGame();
        }

        private void mnuGProperties_Click(object sender, EventArgs e) {
            ShowProperties();
        }

        private void mnuGMRU_Click(object sender, EventArgs e) {
            // open the mru game assigned to this menu item
            _ = int.TryParse(((ToolStripMenuItem)sender).Tag.ToString(), out int index);
            OpenMRUGame(index);
        }

        private void mnuGExit_Click(object sender, EventArgs e) {
            // shut it all down
            this.Close();
        }

        #endregion

        #region Resources Menu
        private void mnuResources_DropDownOpening(object sender, EventArgs e) {
            // configure the dropdown menu before displaying it:
            // - if preview window is the active mdi form, align the
            //    menu for the currently selected resource
            // - if any other form
            //    is active, align it for the current form

            // TODO: need to account for resource errors

            if (ActiveMdiChild != null) {
                // configure for the current editor
                dynamic form = ActiveMdiChild;
                form.SetResourceMenu();
                // preview only- use the main form menu setup
                if (ActiveMdiChild != PreviewWin) {
                    return;
                }
            }
            // configure for the tree/list selected item
            if (EditGame is null) {
                // no game
                mnuRImport.Enabled = false;
                mnuROpenRes.Visible = false;
                mnuRSave.Visible = true;
                mnuRSave.Text = "Save Resource";
                mnuRSave.Enabled = false;
                mnuRExport.Visible = false;
                mnuRRemove.Visible = true;
                mnuRRemove.Text = "Add to Game";
                mnuRRemove.Enabled = false;
                mnuRRenumber.Visible = true;
                mnuRRenumber.Text = "Renumber Resource";
                mnuRRenumber.Enabled = false;
                mnuRProperties.Visible = false;
                mnuRSep3.Visible = false;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                return;
            }
            // if a game is loaded, import is also always available
            mnuRImport.Enabled = true;
            // OBJECT
            if (SelResType == Objects) {
                mnuROpenRes.Visible = true;
                mnuROpenRes.Text = "Open OBJECT";
                mnuRSave.Visible = true;
                mnuRSave.Text = "Save OBJECT";
                mnuRSave.Enabled = false;
                mnuRExport.Visible = true;
                mnuRExport.Text = "Export OBJECT";
                mnuRExport.Enabled = true;
                mnuRRemove.Visible = true;
                mnuRRemove.Text = "Add to Game";
                mnuRRemove.Enabled = false;
                mnuRRenumber.Visible = false;
                mnuRProperties.Visible = true;
                mnuRProperties.Text = "Description...";
                mnuRProperties.Enabled = true;
                mnuRSep3.Visible = false;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                return;
            }
            // WORDS.TOK
            if (SelResType == Words) {
                mnuROpenRes.Visible = true;
                mnuROpenRes.Text = "Open WORDS.TOK";
                mnuRSave.Visible = true;
                mnuRSave.Text = "Save WORDS.TOK";
                mnuRSave.Enabled = false;
                mnuRExport.Visible = true;
                mnuRExport.Text = "Export WORDS.TOK";
                mnuRExport.Enabled = true;
                mnuRRemove.Visible = true;
                mnuRRemove.Text = "Add to Game";
                mnuRRemove.Enabled = false;
                mnuRRenumber.Visible = false;
                mnuRProperties.Visible = true;
                mnuRProperties.Text = "Description...";
                mnuRProperties.Enabled = true;
                mnuRSep3.Visible = false;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                return;
            }

            // resource header or game
            if (SelResNum == -1) {
                mnuROpenRes.Visible = false;
                mnuRSave.Visible = true;
                mnuRSave.Text = "Save Resource";
                mnuRSave.Enabled = false;
                if (SelResType == AGIResType.Game) {
                    mnuRExport.Visible = true;
                    mnuRExport.Text = "Export All Resources";
                }
                else {
                    mnuRExport.Visible = false;
                }
                mnuRRemove.Visible = true;
                mnuRRemove.Text = "Add to Game";
                mnuRRemove.Enabled = false;
                mnuRRenumber.Visible = true;
                mnuRRenumber.Text = "Renumber Resource";
                mnuRRenumber.Enabled = false;
                mnuRProperties.Visible = false;
                mnuRCompileLogic.Visible = false;
                if (SelResType == AGIResType.Picture) {
                    mnuRSep3.Visible = true;
                    mnuRSavePicImage.Visible = true;
                    mnuRSavePicImage.Text = "Export All Picture Images...";
                }
                else {
                    mnuRSep3.Visible = false;
                    mnuRSavePicImage.Visible = false;
                }
                mnuRExportGIF.Visible = false;
                return;
            }
            // must be a logic/picture/sound/view resource
            mnuROpenRes.Visible = true;
            mnuROpenRes.Text = "Open " + SelResType.ToString();
            mnuRSave.Visible = true;
            mnuRSave.Text = "Save " + SelResType.ToString();
            mnuRSave.Enabled = false;
            mnuRExport.Visible = true;
            mnuRExport.Text = "Export " + SelResType.ToString();
            mnuRRemove.Visible = true;
            mnuRRemove.Text = "Remove from Game";
            mnuRRemove.Enabled = true;
            mnuRRenumber.Visible = true;
            mnuRRenumber.Text = "Renumber " + SelResType.ToString();
            mnuRProperties.Visible = true;
            mnuRProperties.Text = "ID/Description...";
            bool err = false;
            switch (SelResType) {
            case AGIResType.Logic:
                // error level doesn't affect logics
                // err = EditGame.Logics[SelResNum].ErrLevel < 0;
                mnuRSep3.Visible = !EditGame.Logics[SelResNum].Compiled;
                mnuRCompileLogic.Visible = !EditGame.Logics[SelResNum].Compiled;
                mnuRCompileLogic.Enabled = true;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                break;
            case AGIResType.Picture:
                err = EditGame.Pictures[SelResNum].ErrLevel < 0;
                mnuRSep3.Visible = true;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = true;
                mnuRSavePicImage.Enabled = !err;
                mnuRSavePicImage.Text = "Save Picture Image As...";
                mnuRExportGIF.Text = "Export Picture As Animated GIF...";
                mnuRExportGIF.Visible = true;
                mnuRExportGIF.Enabled = !err;
                break;
            case AGIResType.Sound:
                err = EditGame.Sounds[SelResNum].ErrLevel < 0;
                mnuRSep3.Visible = false;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                break;
            case AGIResType.View:
                err = EditGame.Views[SelResNum].ErrLevel < 0;
                mnuRSep3.Visible = true;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Text = "Export Loop As Animated GIF...";
                mnuRExportGIF.Visible = true;
                mnuRExportGIF.Enabled = !err;
                break;
            }
            // if resource has an error, only add/remove is enabled
            mnuROpenRes.Enabled = !err;
            mnuRExport.Enabled = !err;
            mnuRRenumber.Enabled = !err;
            mnuRProperties.Enabled = !err;
        }

        private void mnuResources_DropDownClosed(object sender, EventArgs e) {
            // re-enable so shortcut keys will work
            // (make sure all menu items that use shortcuts have appropriate checks 
            // to make sure they're supposed to be enabled)
            if (ActiveMdiChild != null) {
                // configure for the current editor
                dynamic form = ActiveMdiChild;
                form.ResetResourceMenu();
                // preview only- use the main form menu setup
                if (ActiveMdiChild != PreviewWin) {
                    return;
                }
            }

            mnuROpenRes.Enabled = true;    //ctrl+alt+s
            mnuRExport.Enabled = true;  //ctrl+e
            mnuRSave.Enabled = true;    //ctrl+s

            mnuRRemove.Enabled = true;     //ctrl+shift+a
            mnuRRenumber.Enabled = true;   //alt+n
            mnuRProperties.Enabled = true; //ctrl+d

            mnuRCompileLogic.Enabled = true;
            mnuRSavePicImage.Enabled = true; //ctrl+alt+s
            mnuRExportGIF.Enabled = true; //ctrl+alt+g
        }

        internal void mnuRNLogic_Click(object sender, EventArgs e) {
            NewLogic();
        }

        internal void mnuRNPicture_Click(object sender, EventArgs e) {
            NewPicture();
        }

        internal void mnuRNSound_Click(object sender, EventArgs e) {
            NewSound();
        }

        internal void mnuRNView_Click(object sender, EventArgs e) {
            NewView();
        }

        internal void mnuRNObjects_Click(object sender, EventArgs e) {
            NewInvObjList();
        }

        internal void mnuRNWords_Click(object sender, EventArgs e) {
            NewWordList();
        }

        internal void mnuRNText_Click(object sender, EventArgs e) {
            NewTextFile();
        }

        internal void mnuROLogic_Click(object sender, EventArgs e) {
            if (EditGame != null) {
                OpenGameLogic();
            }
            else {
                OpenLogic();
            }
        }

        internal void mnuROPicture_Click(object sender, EventArgs e) {
            if (EditGame != null) {
                OpenGamePicture();
            }
            else {
                OpenPicture();
            }
        }

        internal void mnuROSound_Click(object sender, EventArgs e) {
            if (EditGame != null) {
                OpenGameSound();
            }
            else {
                OpenSound();
            }
        }

        internal void mnuROView_Click(object sender, EventArgs e) {
            if (EditGame == null) {
                OpenView();
            }
            else {
                OpenGameView();
            }
        }

        internal void mnuROObjects_Click(object sender, EventArgs e) {
            if (EditGame != null && !OEInUse) {
                OpenGameOBJECT();
            }
            else {
                string filename = GetOpenResourceFilename("Open ", AGIResType.Objects);
                if (filename.Length > 0) {
                    OpenOBJECT(filename);
                }
            }
        }

        internal void mnuROWords_Click(object sender, EventArgs e) {
            if (EditGame != null && !WEInUse) {
                OpenGameWORDSTOK();
            }
            else {
                string filename = GetOpenResourceFilename("Open ", AGIResType.Words);
                if (filename.Length > 0) {
                    OpenWORDSTOK(filename);
                }
            }
        }

        internal void mnuROText_Click(object sender, EventArgs e) {
            string filename = GetOpenResourceFilename("Open ", AGIResType.Text);
            if (filename.Length > 0) {
                OpenTextFile(filename);
            }
        }

        internal void mnuRILogic_Click(object sender, EventArgs e) {
            string importfile = GetOpenResourceFilename("Import ", AGIResType.Logic);
            if (importfile.Length > 0) {
                if (ActiveMdiChild != null) {
                    if (ActiveMdiChild.Name == "frmLogicEdit") {
                        if (((frmLogicEdit)ActiveMdiChild).FormMode == LogicFormMode.Logic) {
                            if (MessageBox.Show(MDIMain,
                                "Do you want to replace the logic you are currently editing?",
                                "Import Logic",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes) {
                                ((frmLogicEdit)ActiveMdiChild).ImportLogic(importfile);
                                return;
                            }
                        }
                    }
                }
                NewLogic(importfile);
            }
        }

        internal void mnuRIPicture_Click(object sender, EventArgs e) {
            string importfile = GetOpenResourceFilename("Import ", AGIResType.Picture);
            if (importfile.Length > 0) {
                if (ActiveMdiChild.Name == "frmPicEdit") {
                    if (MessageBox.Show(MDIMain,
                        "Do you want to replace the picture you are currently editing?",
                        "Import Picture",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes) {
                        ((frmPicEdit)ActiveMdiChild).ImportPicture(importfile);
                        return;
                    }
                }
                NewPicture(importfile);
            }
        }

        internal void mnuRISound_Click(object sender, EventArgs e) {
            string importfile = GetOpenResourceFilename("Import ", AGIResType.Sound);
            if (importfile.Length > 0) {
                if (ActiveMdiChild.Name == "frmSoundEdit") {
                    if (MessageBox.Show(MDIMain,
                        "Do you want to replace the sound you are currently editing?",
                        "Import Sound",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes) {
                        ((frmSoundEdit)ActiveMdiChild).ImportSound(importfile);
                        return;
                    }
                }
                NewSound(importfile);
            }
        }

        internal void mnuRIView_Click(object sender, EventArgs e) {
            string importfile = GetOpenResourceFilename("Import ", AGIResType.View);
            if (importfile.Length > 0) {
                if (ActiveMdiChild.Name == "frmViewEdit") {
                    if (MessageBox.Show(MDIMain,
                        "Do you want to replace the view you are currently editing?",
                        "Import View",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes) {
                        ((frmViewEdit)ActiveMdiChild).ImportView(importfile);
                        return;
                    }
                }
                NewView(importfile);
            }
        }

        internal void mnuRIObjects_Click(object sender, EventArgs e) {
            bool skipcheck = false;
            string importfile = GetOpenResourceFilename("Import ", AGIResType.Objects);
            if (importfile.Length > 0) {
                if (ActiveMdiChild.Name == "frmObjectEdit") {
                    if (MessageBox.Show(MDIMain,
                        "Do you want to replace the OBJECT file you are currently editing? " +
                        "(This CANNOT be undone.)",
                        "Import OBJECT",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes) {
                        ((frmObjectEdit)ActiveMdiChild).ImportObjects(importfile);
                        return;
                    }
                    skipcheck = true;
                }
                NewInvObjList(importfile, skipcheck);
            }
        }

        internal void mnuRIWords_Click(object sender, EventArgs e) {
            bool skipcheck = false;
            string importfile = GetOpenResourceFilename("Import ", AGIResType.Words);
            if (importfile.Length > 0) {
                if (ActiveMdiChild.Name == "frmWordsEdit") {
                    if (MessageBox.Show(MDIMain,
                        "Do you want to replace the WORDS.TOK file you are currently editing? " +
                        "(This CANNOT be undone.)",
                        "Import WORDS.TOK",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes) {
                        ((frmWordsEdit)ActiveMdiChild).ImportWords(importfile);
                        return;
                    }
                    skipcheck = true;
                }
                NewWordList(importfile, skipcheck);
            }
        }

        private void mnuROpenRes_Click(object sender, EventArgs e) {
            switch (SelResType) {
            case AGIResType.Logic:
                OpenGameLogic((byte)SelResNum);
                break;
            case AGIResType.Picture:
                OpenGamePicture((byte)SelResNum);
                break;
            case AGIResType.Sound:
                OpenGameSound((byte)SelResNum);
                break;
            case AGIResType.View:
                OpenGameView((byte)SelResNum);
                break;
            case AGIResType.Objects:
                OpenGameOBJECT();
                break;
            case AGIResType.Words:
                OpenGameWORDSTOK();
                break;
            }
        }

        private void mnuRSave_Click(object sender, EventArgs e) {
            // place holder, not active when preview/tree/list selected
            // replaced by active window's menu item
        }

        private void mnuRExport_Click(object sender, EventArgs e) {
            ExportGameResource(SelResType, SelResNum);
        }

        internal void mnuRRenumber_Click(object sender, EventArgs e) {
            RenumberSelectedResource();
        }

        private void mnuRRemove_Click(object sender, EventArgs e) {
            RemoveSelectedResource();
        }

        private void mnuRProperties_Click(object sender, EventArgs e) {
            EditSelectedItemProperties(1);
        }

        private void mnuRCompileLogic_Click(object sender, EventArgs e) {
            CompileLogic(null, (byte)SelResNum);
        }

        private void mnuRSavePicImage_Click(object sender, EventArgs e) {
            switch (SelResNum) {
            case -1:
                ExportAllPicImgs();
                break;
            default:
                Picture agPic = EditGame.Pictures[SelResNum];
                if (agPic.ErrLevel >= 0) {
                    ExportOnePicImg(agPic);
                }
                break;
            }
        }

        private void mnuRExportGIF_Click(object sender, EventArgs e) {
            switch (SelResType) {
            case AGIResType.Picture:
                if (EditGame.Pictures[SelResNum].ErrLevel >= 0) {
                    ExportPicAsGif(EditGame.Pictures[SelResNum]);
                }
                break;
            case AGIResType.View:
                if (EditGame.Views[SelResNum].ErrLevel >= 0) {
                    ExportLoopGIF(EditGame.Views[SelResNum], 0);
                }
                break;
            }
        }

        #endregion

        #region Tools Menu
        private void mnuTools_DropDownOpening(object sender, EventArgs e) {
            mnuTLayout.Enabled = EditGame != null && EditGame.UseLE;
            mnuTWarning.Enabled = EditGame != null;
            mnuTWarning.Text = pnlWarnings.Visible ? "Hide Warning List" : "Show Warning List";
        }

        private void mnuTSettings_Click(object sender, EventArgs e) {
            // starting page depends on currently active form
            int startpage = 0;
            if (ActiveMdiChild != null) {
                switch (ActiveMdiChild.Name) {
                case "frmLogicEdit":
                    startpage = 1;
                    break;
                case "frmGlobals":
                    startpage = 2;
                    break;
                case "frmPicEdit":
                    startpage = 3;
                    break;
                case "frmSoundEdit":
                    startpage = 4;
                    break;
                case "frmViewEdit":
                    startpage = 5;
                    break;
                }
            }
            frmSettings frm = new frmSettings(startpage);
            frm.ShowDialog(MDIMain);
            frm.Dispose();
            // the settings form handles all mods and updates based on settings changes
        }

        private void mnuTLayout_Click(object sender, EventArgs e) {
            MessageBox.Show(MDIMain, "TODO: layout editor");
            if (MDIMain.ActiveMdiChild.Name == "frmLogicEdit") {
                ((frmLogicEdit)MDIMain.ActiveMdiChild).RestoreFocusHack();
            }
        }

        private void mnuTMenuEditor_Click(object sender, EventArgs e) {
            MessageBox.Show(MDIMain, "TODO: menu editor");
            if (MDIMain.ActiveMdiChild.Name == "frmLogicEdit") {
                ((frmLogicEdit)MDIMain.ActiveMdiChild).RestoreFocusHack();
            }
        }

        private void mnuTGlobals_Click(object sender, EventArgs e) {
            OpenGlobals(GEInUse);
        }

        private void mnuTReserved_Click(object sender, EventArgs e) {
            frmReserved frm = new();
            frm.ShowDialog(MDIMain);
            frm.Dispose();


            if (MDIMain.ActiveMdiChild.Name == "frmLogicEdit") {
                ((frmLogicEdit)MDIMain.ActiveMdiChild).RestoreFocusHack();
            }
        }

        private void mnuTSnippets_Click(object sender, EventArgs e) {
            frmSnippets Snippets = new(false);
            _ = Snippets.ShowDialog(this);
            Snippets.Dispose();
        }

        private void mnuTPalette_Click(object sender, EventArgs e) {

            frmPalette frm = new(0);
            if (frm.ShowDialog(MDIMain) == DialogResult.OK) {
                // refresh all picture, view  editors and the preview window (if visible)
                foreach (frmPicEdit editfrm in PictureEditors) {
                    editfrm.RefreshPic();
                }
                foreach (frmViewEdit editfrm in ViewEditors) {
                    editfrm.RefreshCel();
                }
                if (PreviewWin.Visible) {
                    switch (SelResType) {
                    case AGIResType.Picture:
                        PreviewWin.RefreshPic();
                        break;
                    case AGIResType.View:
                        PreviewWin.RefreshView();
                        break;
                    }
                }
            }
            frm.Dispose();
        }

        private void mnuTWarning_Click(object sender, EventArgs e) {
            if (pnlWarnings.Visible) {
                HideWarningList();
            }
            else {
                if (EditGame != null) {
                    ShowWarningList();
                }
            }
        }

        private void mnuTCustom_Click(object sender, EventArgs e) {
            ToolStripMenuItem thisTool = (ToolStripMenuItem)sender;
            string target = thisTool.Tag.ToString();

            if ((target.Left(4) == "http") || (target.Left(4) == "www.")) {
                // open as a url, not a file
                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = target,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "Unable to open this URL.", "Unhandled system error encountered.", "Custom Tool Error");
                }
            }
            else {
                // save the old path, so it can be restored once we're done
                string oldpath = Directory.GetCurrentDirectory();

                // if a game is open, assume it's the current directory;
                // otherwise, assume program directory is current directory
                if (EditGame != null) {
                    Directory.SetCurrentDirectory(EditGame.GameDir);
                }
                else {
                    Directory.SetCurrentDirectory(ProgramDir);
                }
                // does this tool entry include a directory?
                string strFile;
                if (JustPath(target, false).Length > 0) {
                    // if a path is provided, check for program dir
                    strFile = target.Replace("%PROGDIR%", ProgramDir[..^1]);
                }
                else {
                    strFile = target;
                }

                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = strFile,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "Sorry, an error occurred when trying to open this tool entry.", "Please edit your tool information to point to a valid file/program.", "Custom Tool Error");
                }
                // restore current directory
                Directory.SetCurrentDirectory(oldpath);
            }
        }

        private void mnuTCustomize_Click(object sender, EventArgs e) {
            frmTools ToolsEditor = new() { };
            _ = ToolsEditor.ShowDialog(this);
        }

        #endregion

        #region Windows Menu
        private void mnuWindow_DropDownOpening(object sender, EventArgs e) {
            // disable the close item if no windows or if active window is preview
            mnuWClose.Enabled = (MdiChildren.Length != 0) && (ActiveMdiChild != PreviewWin) && (ActiveMdiChild != null);
            // image scaling messes up the checkbox that indicates the 
            // currently sctive window ...
            foreach (ToolStripItem item in mnuWindow.DropDownItems) {
                if (item.GetType() == typeof(ToolStripMenuItem)) {
                    if (((ToolStripMenuItem)item).Checked) {
                        item.ImageScaling = ToolStripItemImageScaling.None;
                        break;
                    }
                }
            }

        }

        private void mnuWCascade_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void mnuWTileV_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void mnuWTileH_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void mnuWArrange_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void mnuWMinimize_Click(object sender, EventArgs e) {
            foreach (Form childForm in MdiChildren) {
                childForm.WindowState = FormWindowState.Minimized;
            }
        }

        private void mnuWClose_Click(object sender, EventArgs e) {
            // closes the active mdi child window
            if (ActiveMdiChild != PreviewWin) {
                Form form = ActiveMdiChild;
                form.Close();
                if (!form.Visible) {
                    form.Dispose();
                }
            }
        }
        #endregion

        #region Help Menu
        private void mnuHContents_Click(object sender, EventArgs e) {
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.TableOfContents);
        }

        private void mnuHIndex_Click(object sender, EventArgs e) {
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Index);
        }

        private void mnuHCommands_Click(object sender, EventArgs e) {
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.TopicId, "1001");
        }

        private void mnuHReference_Click(object sender, EventArgs e) {
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.TopicId, "1002");
        }

        private void mnuHAbout_Click(object sender, EventArgs e) {
            // TODO: update About screen
            MessageBox.Show(this, $"This is WinAGI.\n\n{Application.ProductVersion}", "About WinAGI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: for controls, use the value 'Topic' for 'HelpNavigator' and
            // set the 'HelpKeyword' field to the topic name (i.e. "htm\winagi\restree.htm#propwindow")
            // DON'T use the 'HelpString' field
        }

        #endregion

        private void cmsResource_Opening(object sender, CancelEventArgs e) {
            // configure the menu before opening
            // OBJECT
            if (SelResType == Objects) {
                cmROpenRes.Visible = true;
                cmROpenRes.Text = "Open OBJECT";
                cmRSave.Visible = true;
                cmRSave.Text = "Save OBJECT";
                cmRSave.Enabled = false;
                cmRExport.Visible = true;
                cmRExport.Text = "Export OBJECT";
                cmRExport.Enabled = true;
                cmRRemove.Visible = true;
                cmRRemove.Text = "Add to Game";
                cmRRemove.Enabled = false;
                cmRRenumber.Visible = false;
                cmRProperties.Visible = true;
                cmRProperties.Text = "Description...";
                cmRProperties.Enabled = true;
                cmRSep2.Visible = false;
                cmRCompileLogic.Visible = false;
                cmRSavePicImage.Visible = false;
                cmRExportGIF.Visible = false;
                return;
            }
            // WORDS.TOK
            if (SelResType == Words) {
                cmROpenRes.Visible = true;
                cmROpenRes.Text = "Open WORDS.TOK";
                cmRSave.Visible = true;
                cmRSave.Text = "Save WORDS.TOK";
                cmRSave.Enabled = false;
                cmRExport.Visible = true;
                cmRExport.Text = "Export WORDS.TOK";
                cmRExport.Enabled = true;
                cmRRemove.Visible = true;
                cmRRemove.Text = "Add to Game";
                cmRRemove.Enabled = false;
                cmRRenumber.Visible = false;
                cmRProperties.Visible = true;
                cmRProperties.Text = "Description...";
                cmRProperties.Enabled = true;
                cmRSep2.Visible = false;
                cmRCompileLogic.Visible = false;
                cmRSavePicImage.Visible = false;
                cmRExportGIF.Visible = false;
                return;
            }
            // game or resource header 
            if (SelResNum == -1) {
                cmROpenRes.Visible = false;
                cmRSave.Visible = true;
                cmRSave.Text = "Save Resource";
                cmRSave.Enabled = false;
                if (SelResType == AGIResType.Game) {
                    cmRExport.Visible = true;
                    cmRExport.Text = "Export All Resources";
                }
                else {
                    cmRExport.Visible = false;
                }
                cmRRemove.Visible = true;
                cmRRemove.Text = "Add to Game";
                cmRRemove.Enabled = false;
                cmRRenumber.Visible = true;
                cmRRenumber.Text = "Renumber Resource";
                cmRRenumber.Enabled = false;
                cmRProperties.Visible = false;
                cmRCompileLogic.Visible = false;
                if (SelResType == AGIResType.Picture) {
                    cmRSep2.Visible = true;
                    cmRSavePicImage.Visible = true;
                    cmRSavePicImage.Text = "Export All Picture Images...";
                }
                else {
                    cmRSep2.Visible = false;
                    cmRSavePicImage.Visible = false;
                }
                cmRExportGIF.Visible = false;
                return;
            }
            // must be a logic/picture/sound/view resource
            cmROpenRes.Visible = true;
            cmROpenRes.Text = "Open " + SelResType.ToString();
            cmRSave.Visible = true;
            cmRSave.Text = "Save " + SelResType.ToString();
            cmRSave.Enabled = false;
            cmRExport.Visible = true;
            cmRExport.Text = "Export " + SelResType.ToString();
            cmRRemove.Visible = true;
            cmRRemove.Text = "Remove from Game";
            cmRRemove.Enabled = true;
            cmRRenumber.Visible = true;
            cmRRenumber.Text = "Renumber " + SelResType.ToString();
            cmRProperties.Visible = true;
            cmRProperties.Text = "ID/Description...";
            bool err = false;
            switch (SelResType) {
            case AGIResType.Logic:
                // error level doesn't affect logics
                // err = EditGame.Logics[SelResNum].ErrLevel < 0;
                cmRSep2.Visible = !EditGame.Logics[SelResNum].Compiled;
                cmRCompileLogic.Visible = !EditGame.Logics[SelResNum].Compiled;
                cmRCompileLogic.Enabled = true;
                cmRSavePicImage.Visible = false;
                cmRExportGIF.Visible = false;
                break;
            case AGIResType.Picture:
                err = EditGame.Pictures[SelResNum].ErrLevel < 0;
                cmRSep2.Visible = true;
                cmRCompileLogic.Visible = false;
                cmRSavePicImage.Visible = true;
                cmRSavePicImage.Enabled = !err;
                cmRSavePicImage.Text = "Save Picture Image As...";
                cmRExportGIF.Text = "Export Picture As Animated GIF...";
                cmRExportGIF.Visible = true;
                cmRExportGIF.Enabled = !err;
                break;
            case AGIResType.Sound:
                err = EditGame.Sounds[SelResNum].ErrLevel < 0;
                cmRSep2.Visible = false;
                cmRCompileLogic.Visible = false;
                cmRSavePicImage.Visible = false;
                cmRExportGIF.Visible = false;
                break;
            case AGIResType.View:
                err = EditGame.Views[SelResNum].ErrLevel < 0;
                cmRSep2.Visible = true;
                cmRCompileLogic.Visible = false;
                cmRSavePicImage.Visible = false;
                cmRExportGIF.Text = "Export Loop As Animated GIF...";
                cmRExportGIF.Visible = true;
                cmRExportGIF.Enabled = !err;
                break;
            }
            // if resource has an error, only add/remove is enabled
            cmROpenRes.Enabled = !err;
            cmRExport.Enabled = !err;
            cmRRenumber.Enabled = !err;
            cmRProperties.Enabled = !err;
        }
        #endregion

        #region Panel Splitter Event Handlers
        private void splResource_SplitterMoved(object sender, SplitterEventArgs e) {
            if (splResource.Visible) {
                if (splResource.Panel2.Height > PropPanelMaxSize) {
                    splResource.SplitterDistance = splResource.Height - PropPanelMaxSize;
                }
            }
        }

        private void splResource_SizeChanged(object sender, EventArgs e) {
            if (splResource.Panel2.Height > PropPanelMaxSize) {
                splResource.SplitterDistance = splResource.Height - PropPanelMaxSize;
            }
        }

        private void splResource_Panel1_Resize(object sender, EventArgs e) {
            // resize the navigation buttons
            cmdBack.Width = splResource.Width / 2;
            cmdForward.Width = splResource.Width / 2;
            cmdForward.Left = splResource.Width / 2;
        }
        #endregion

        #region Resource Tree/List Event Handlers
        private void tvwResources_MouseDown(object sender, MouseEventArgs e) {
            // force selection to change BEFORE context menu is shown
            if (e.Button == MouseButtons.Right) {
                TreeNode node = tvwResources.GetNodeAt(e.X, e.Y);
                if (node != null) {
                    tvwResources.SelectedNode = node;
                }
            }
        }

        private void tvwResources_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
            // select it first
            //          tvwResources.SelectedNode = e.Node;
        }

        private void tvwResources_AfterCollapse(object sender, TreeViewEventArgs e) {
            //when collapsing, select the collapsed node
            tvwResources_NodeMouseClick(sender, new TreeNodeMouseClickEventArgs(e.Node, MouseButtons.None, 0, 0, 0));
        }

        private void tvwResources_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) {
            if (e.Node != tvwResources.SelectedNode) {
                // this seems to only happen if the tree is collapsing or expanding;
                // best thing to do is just ignore the dblclick in that case
                return;
            }
            switch (e.Node.Level) {
            case 0:
                ShowProperties();
                break;
            case 1:
                switch (SelResType) {
                case AGIResType.Objects:
                    OpenGameOBJECT();
                    break;
                case AGIResType.Words:
                    OpenGameWORDSTOK();
                    break;
                }
                break;
            case 2:
                switch (SelResType) {
                case AGIResType.Logic:
                    OpenGameLogic((byte)SelResNum);
                    break;
                case AGIResType.Picture:
                    OpenGamePicture((byte)SelResNum);
                    break;
                case AGIResType.Sound:
                    OpenGameSound((byte)SelResNum);
                    break;
                case AGIResType.View:
                    OpenGameView((byte)SelResNum);
                    break;
                }
                break;
            }
        }

        private void tvwResources_AfterSelect(object sender, TreeViewEventArgs e) {
            // probably due to navigation - 
            Debug.Assert(EditGame != null);
            // ???? there are some operations that seem to trigger this event when
            // the tree isn't visible and no game is being edited; examples include
            //    - opening help file then closing the program,
            //    - opening new game in some scenarios,
            //    - ? others?
            if (EditGame == null) {
                return;
            }

            //if no active form
            if (MdiChildren.Length == 0) {
                //set control focus to treeview
                tvwResources.Select();
            }
            //if nothing selected
            if (e.Node is null) {
                return;
            }
            //if not changed from previous number
            if (LastNodeName == e.Node.Name) {
                if (!PreviewWin.Visible && WinAGISettings.ShowPreview.Value) {
                    PreviewWin.Show();
                    //set form focus to preview
                    PreviewWin.Activate();
                    //set control focus to tvwlist
                    tvwResources.Select();
                }
                //don't need to change anything
                return;
            }
            // save current index as last index
            LastNodeName = e.Node.Name;
            // now select it
            if (e.Node.Level == 0) {
                // it's the game node
                SelectResource(Game, -1);
            }
            else if (e.Node.Level == 1) {
                // it's a resource header
                SelectResource((AGIResType)e.Node.Index, -1);
            }
            else {
                // it's a resource
                SelectResource((AGIResType)e.Node.Parent.Index, (byte)e.Node.Tag);
                // logics only - it's possible for the source to be edited
                // outside WinAGI, so always check compiled status
                if ((AGIResType)e.Node.Parent.Index == AGIResType.Logic) {
                    if (EditGame.Logics[(byte)e.Node.Tag].Compiled) {
                        e.Node.ForeColor = Color.Black;
                    }
                    else {
                        e.Node.ForeColor = Color.Red;
                    }
                }
            }
            //after selection, force preview window to show and
            // move up, if those settings are active
            if (WinAGISettings.ShowPreview.Value) {
                if (!PreviewWin.Visible) {
                    PreviewWin.Show();
                    PreviewWin.Activate();
                    tvwResources.Select();
                }
                else {
                    if (ActiveMdiChild != PreviewWin && WinAGISettings.ShiftPreview.Value) {
                        //if previn hidden on lostfocus, need to show it AFTER changing displayed resource
                        PreviewWin.Activate();
                        tvwResources.Select();
                    }
                }
            }
        }

        private void cmbResType_SelectedIndexChanged(object sender, EventArgs e) {
            // fill list box with resources for selected type
            AGIResType selRes;

            Debug.Assert(EditGame is not null);
            // clear current list
            lstResources.Items.Clear();

            selRes = (AGIResType)cmbResType.SelectedIndex;
            ListViewItem tmpItem;
            switch (cmbResType.SelectedIndex) {
            case 0:
                selRes = Game;
                break;
            case 1:
                foreach (Logic tmpRes in EditGame.Logics) {
                    tmpItem = lstResources.Items.Add("l" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = tmpRes.Number;
                    tmpItem.ForeColor = tmpRes.Compiled ? Color.Black : Color.Red;
                }
                selRes = AGIResType.Logic;
                break;
            case 2:
                foreach (Picture tmpRes in EditGame.Pictures) {
                    tmpItem = lstResources.Items.Add("p" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = tmpRes.Number;
                }
                selRes = AGIResType.Picture;
                break;
            case 3:
                foreach (Sound tmpRes in EditGame.Sounds) {
                    tmpItem = lstResources.Items.Add("s" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = tmpRes.Number;
                }
                selRes = AGIResType.Sound;
                break;
            case 4:
                foreach (Engine.View tmpRes in EditGame.Views) {
                    tmpItem = lstResources.Items.Add("v" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = tmpRes.Number;
                }
                selRes = AGIResType.View;
                break;
            case 5:
                //objects
                selRes = Objects;
                break;
            case 6:
                //words
                selRes = Words;
                break;
            }
            SelectResource(selRes, -1, true);
        }

        private void lstResources_DoubleClick(object sender, EventArgs e) {
            switch (SelResType) {
            case AGIResType.Logic:
                OpenGameLogic((byte)SelResNum);
                break;
            case AGIResType.Picture:
                OpenGamePicture((byte)SelResNum);
                break;
            case AGIResType.Sound:
                OpenGameSound((byte)SelResNum);
                break;
            case AGIResType.View:
                OpenGameView((byte)SelResNum);
                break;
            }
        }

        private void lstResources_SelectedIndexChanged(object sender, EventArgs e) {
            AGIResType NewType = Game;
            int NewNum = 0;

            switch (cmbResType.SelectedIndex) {
            case 0:
                // game - root
                NewType = Game;
                NewNum = -1;
                // currently no list items
                break;
            case 1:
                // logics
                // if nothing to select
                if (lstResources.SelectedItems.Count == 0) {
                    // just exit
                    return;
                }
                NewType = AGIResType.Logic;
                NewNum = (byte)lstResources.SelectedItems[0].Tag;
                //// show id, number, description, compiled status, isroom
                // don't need to adjust context menu; preview window will do that
                break;
            case 2:
                // pictures
                // if nothing to select
                if (lstResources.SelectedItems.Count == 0) {
                    // just exit
                    return;
                }
                NewType = AGIResType.Picture;
                NewNum = (byte)lstResources.SelectedItems[0].Tag;
                //// show id, number, description
                // don't need to adjust context menu; preview window will do that
                break;
            case 3:
                // sounds
                // if nothing to select
                if (lstResources.SelectedItems.Count == 0) {
                    // just exit
                    return;
                }
                NewType = AGIResType.Sound;
                NewNum = (byte)lstResources.SelectedItems[0].Tag;
                //// show id, number, description
                // don't need to adjust context menu; preview window will do that
                break;
            case 4:
                // views
                // if nothing to select
                if (lstResources.SelectedItems.Count == 0) {
                    // just exit
                    return;
                }
                NewType = AGIResType.View;
                NewNum = (byte)lstResources.SelectedItems[0].Tag;
                //// show id, number, description
                // don't need to adjust context menu; preview window will do that
                break;
            case 5:
                // objects
                // no listitems
                NewType = Objects;
                NewNum = -1;
                break;
            case 6:
                // words
                // no listitems
                NewType = Words;
                NewNum = -1;
                break;
            }
            if (!DontQueue) {
                SelectResource(NewType, NewNum);
            }
            //after selection, force preview window to show and
            // move up, if those settings are active
            if (WinAGISettings.ShowPreview.Value) {
                if (!PreviewWin.Visible) {
                    PreviewWin.Show();
                    PreviewWin.Activate();
                    tvwResources.Select();
                }
                else {
                    if (ActiveMdiChild != PreviewWin && WinAGISettings.ShiftPreview.Value) {
                        //if previn hidden on lostfocus, need to show it AFTER changing displayed resource
                        PreviewWin.Activate();
                        tvwResources.Select();
                    }
                }
            }
        }

        private void lstResources_SizeChanged(object sender, EventArgs e) {
            // always adjust column to fill entire listbox
            lstResources.Columns[0].Width = lstResources.Width - 4;
        }
        #endregion

        #region Resource Navigation Event Handlers
        private void cmdBack_Click(object sender, EventArgs e) {
            // if resqptr is not at beginning, go back one
            // and select that resource
            if (ResQPtr > 0) {
                //back up one
                ResQPtr--;
                //select this node if still present
                SelectFromQueue();

                //adjust the buttons for availability
                cmdBack.Enabled = (ResQPtr > 0);
                cmdForward.Enabled = true;
            }

            //always set focus to the resource list
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                tvwResources.Select();
                break;
            case EResListType.ComboList:
                lstResources.Select();
                break;
            }
        }

        private void cmdBack_MouseDown(object sender, MouseEventArgs e) {
            //if right button, show list of resources on nav stack
            if (e.Button == MouseButtons.Right) {
                //set left edge of list to match this button
                picNavList.Left = cmdBack.Left;
                picNavList.Width = cmdBack.Width;
                //show it
                picNavList.Visible = true;
                //set mouse capture to the list picture
                picNavList.Capture = true;
                //picNavList.Parent = MDIMain;
                //offset is current queue position
                NLOffset = ResQPtr;
            }
        }

        private void cmdForward_Click(object sender, EventArgs e) {
            // if resqptr is not at end, go forward one
            // and select that resource
            if (ResQPtr < ResQueue.Length - 1) {
                //go forward one
                ResQPtr++;
                //select this node if still present
                SelectFromQueue();

                //adjust the buttons for availability
                cmdBack.Enabled = true;
                cmdForward.Enabled = ResQPtr < ResQueue.Length - 1;
            }

            //always set focus to the resource list
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                tvwResources.Select();
                break;
            case EResListType.ComboList:
                lstResources.Select();
                break;
            }
        }

        private void cmdForward_MouseDown(object sender, MouseEventArgs e) {
            //if right button, show list of resources on nav stack
            if (e.Button == MouseButtons.Right) {
                //set left edge of list to match this button
                picNavList.Left = cmdForward.Left;
                picNavList.Width = cmdForward.Width;
                picNavList.Top = cmdForward.Parent.Parent.Parent.Top + e.Y - picNavList.Height / 2;
                //show it
                picNavList.Visible = true;
                //set mouse capture to the list picture
                picNavList.Capture = true;
                //offset is current queue position
                NLOffset = ResQPtr;
            }
        }

        private void picNavList_MouseMove(object sender, MouseEventArgs e) {
            //POINTAPI mPos;
            //int rtn;
            int SelRow;

            if (e.Button != MouseButtons.Right) {
                return;
            }

            // if selrow has changed, repaint

            // determine which row is under cursor
            SelRow = (int)Math.Floor((float)e.Location.Y / NLRowHeight);
            //if on a new row
            if (SelRow != NLRow) {
                //if both values still offscreen
                if (SelRow < 0 && NLRow < 0 || (SelRow > 4 && NLRow > 4)) {
                    //just update the selected row
                    NLRow = SelRow;
                    //no need to repaint
                    return;
                }

                //need to update and repaint
                NLRow = SelRow;
                picNavList_Paint(sender, new PaintEventArgs(picNavList.CreateGraphics(), picNavList.Bounds));
            }

            //if not on the list
            if (SelRow < 0 || SelRow > 4) {
                //enable autoscrolling
                tmrNavList.Enabled = true;
            }
            else {
                //no scrolling
                tmrNavList.Enabled = false;
            }
        }

        private void picNavList_Paint(object sender, PaintEventArgs e) {

            int i;
            SolidBrush hbrush = new(Color.FromArgb(0xff, 0xe0, 0xe0));//                  FFE0E0));
            SolidBrush bbrush = new(Color.Black);
            Font nlFont = new(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
            //draw list of resources on stack, according to current
            // offset; whatever is selected is also highlighted

            //start with a clean slate
            e.Graphics.Clear(picNavList.BackColor);

            PointF nlPoint = new();
            // display five lines
            for (i = 0; i < 5; i++) {
                if (i + NLOffset - 2 >= 0 && i + NLOffset - 2 <= ResQueue.Length - 1) {
                    //if this row is highlighted (under cursor and valid)
                    if (i == NLRow) {
                        e.Graphics.FillRectangle(hbrush, 0, (int)((i + 0.035) * NLRowHeight), picNavList.Width, NLRowHeight);
                    }
                    ////set x and y positions
                    nlPoint.X = 1;
                    nlPoint.Y = NLRowHeight * i;

                    // add the id
                    AGIResType restype = (AGIResType)(ResQueue[i + NLOffset - 2] >> 16);
                    int resnum = ResQueue[i + NLOffset - 2] & 0xFFFF;
                    switch (restype) {
                    case Game:
                        e.Graphics.DrawString(EditGame.GameID, nlFont, bbrush, nlPoint);
                        break;
                    case AGIResType.Logic:
                        if (resnum == 256) {
                            e.Graphics.DrawString("LOGICS", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Logics[resnum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case AGIResType.Picture:
                        if (resnum == 256) {
                            e.Graphics.DrawString("PICTURES", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Pictures[resnum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case AGIResType.Sound:
                        if (resnum == 256) {
                            e.Graphics.DrawString("SOUNDS", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Sounds[resnum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case AGIResType.View:
                        if (resnum == 256) {
                            e.Graphics.DrawString("VIEWS", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Views[resnum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case Objects:
                        e.Graphics.DrawString("OBJECTS", nlFont, bbrush, nlPoint);
                        break;
                    case Words:
                        e.Graphics.DrawString("WORDS", nlFont, bbrush, nlPoint);
                        break;
                    }
                }
            }
        }

        private void picNavList_MouseUp(object sender, MouseEventArgs e) {
            int newPtr;
            picNavList.Visible = false;
            tmrNavList.Enabled = false;

            //get new ptr value; exit if it's invalid
            newPtr = NLOffset + NLRow - 2;
            Debug.Print($" new resQ: {newPtr}");
            //     return;

            if (newPtr < 0) {
                return;
            }
            else if (newPtr > ResQueue.Length - 1) {
                return;
            }

            //if selected row (including offset)
            //is different than current queue position
            if (newPtr != ResQPtr) {
                // move to the offset
                ResQPtr = newPtr;
                SelectFromQueue();
                //adjust the buttons for availability
                cmdBack.Enabled = ResQPtr > 0;
                cmdForward.Enabled = ResQPtr < (ResQueue.Length - 1);
            }
        }
        #endregion

        #region Property Grid Event Handlers
        private void propertyGrid1_MouseWheel(object s, MouseEventArgs e) {
            // wheeling on the IntVersion property should open the dropdownlist
            // but I don't know how to do it, so instead we ignore the mousewheel
            if (propertyGrid1.SelectedGridItem.Label == "IntVer") {
                ((HandledMouseEventArgs)e).Handled = true;
            }
        }
        #endregion

        #region WarningGrid Event Handlers
        private void fgWarnings_MouseDown(object sender, MouseEventArgs e) {
            // before displaying context menu select rows under cursor
            if (e.Button == MouseButtons.Right) {
                DataGridView.HitTestInfo hit = fgWarnings.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0 && hit.RowIndex != fgWarnings.SelectedRows[0].Index) {
                    fgWarnings.Rows[hit.RowIndex].Selected = true;
                }
            }
        }

        private void fgWarnings_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) {
            int tmpRow = e.RowIndex;

            switch ((string)fgWarnings.Rows[tmpRow].Cells[0].Value) {
            case nameof(LogicCompileError):
            case nameof(ResourceError):
                // bold, red
                fgWarnings.Rows[tmpRow].DefaultCellStyle.Font = new Font(fgWarnings.Font, FontStyle.Bold);
                fgWarnings.Rows[tmpRow].DefaultCellStyle.ForeColor = Color.Red;
                break;
            case nameof(TODO):
                // bold, italic
                fgWarnings.Rows[tmpRow].DefaultCellStyle.Font = new Font(fgWarnings.Font, FontStyle.Bold | FontStyle.Italic);
                break;
            case nameof(LogicCompileWarning):
            case nameof(ResourceWarning):
            case nameof(DecompWarning):
                break;
            }
            // always make it visible
            if (!fgWarnings.Rows[tmpRow].Displayed) {
                fgWarnings.CurrentCell = fgWarnings.Rows[tmpRow].Cells[2];
            }
        }

        private void fgWarnings_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (fgWarnings.SelectedRows.Count != 1 || e.RowIndex == -1) {
                return;
            }
            if (e.RowIndex != fgWarnings.SelectedRows[0].Index) {
                fgWarnings.Rows[e.RowIndex].Selected = true;
            }
            switch ((string)fgWarnings.SelectedRows[0].Cells[0].Value) {
            case nameof(LogicCompileError):
            case nameof(DecompWarning):
            case nameof(LogicCompileWarning):
            case nameof(TODO):
                // use the 'goto' menu event handler
                cmiGoTODO_Click(sender, e);
                break;
            }
        }

        private void fgWarnings_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            try {
                int index = e.ColumnIndex;
                warncol.Remove(index);
                warncol.Insert(0, index);
                bool dir = !(bool)fgWarnings.Columns[index].Tag;
                fgWarnings.Columns[index].Tag = dir;
                fgWarnings.Sort(fgWarnings.Columns[index], dir ? ListSortDirection.Ascending : ListSortDirection.Descending);
            }
            catch {
                Debug.Assert(false);
            }
        }

        private void fgWarnings_SortCompare(object sender, DataGridViewSortCompareEventArgs e) {
            int colindex = e.Column.Index;
            if (colindex != warncol[0]) {
                Debug.Assert(false);
                warncol.Remove(e.Column.Index);
                warncol.Insert(0, e.Column.Index);
            }
            int retval = 0;
            for (int i = 0; i < 5; i++) {
                retval = CompareValues(fgWarnings.Rows[e.RowIndex1].Cells[warncol[i]].Value.ToString(), fgWarnings.Rows[e.RowIndex2].Cells[warncol[i]].Value.ToString(), (warncol[i] == 4 || warncol[i] == 5));
                if (retval != 0) {
                    break;
                }
            }
            e.SortResult = retval;
            e.Handled = true;

            int CompareValues(string a, string b, bool isnum) {
                if (isnum) {
                    if (!int.TryParse(a.ToString(), out int val1)) {
                        val1 = -1;
                    }
                    if (!int.TryParse(b.ToString(), out int val2)) {
                        val2 = -1;
                    }
                    if (val1 > val2) {
                        return 1;
                    }
                    else if (val1 < val2) {
                        return -1;
                    }
                    else {
                        return 0;
                    }
                }
                return string.Compare(a, b);
            }
        }

        private void fgWarnings_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.Value == null) {
                return;
            }
            // first determine if tooltip is needed
            DataGridViewCell cell = fgWarnings.Rows[e.RowIndex].Cells[e.ColumnIndex];
            string text = (string)e.Value;
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
            // Declare a proposed size with dimensions set to the maximum integer value.
            Size proposedSize = new Size(int.MaxValue, int.MaxValue);
            // get size
            Size szText = TextRenderer.MeasureText(fgWarnings.CreateGraphics(), text, e.CellStyle.Font, proposedSize, flags);
            if (szText.Width > cell.Size.Width - 8) {
                cell.ToolTipText = text;
            }
            else {
                cell.ToolTipText = "";
            }

            // then apply formatting based on warning type
            // TODO: right now, this is done when rows are added, and it works
            // fine; maybe I don't need to move it to this formatting event
        }

        private void fgWarnings_CellMouseEnter(object sender, DataGridViewCellEventArgs e) {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) {
                return;
            }
            DataGridViewCell cell = fgWarnings.Rows[e.RowIndex].Cells[e.ColumnIndex];
            if (cell.ToolTipText.Length > 0) {
                fgWarnings.ShowCellToolTips = true;
            }
        }

        private void fgWarnings_CellMouseLeave(object sender, DataGridViewCellEventArgs e) {
            fgWarnings.ShowCellToolTips = false;
        }

        private void cmsGrid_Opening(object sender, CancelEventArgs e) {
            Point mp = fgWarnings.PointToClient(MousePosition);
            DataGridView.HitTestInfo hit = fgWarnings.HitTest(mp.X, mp.Y);
            if (hit.RowIndex == -1) {
                e.Cancel = true;
                return;
            }

            // if no warnings, don't show menu
            if (WarningList.Count == 0) {
                e.Cancel = true;
                return;
            }
            if (fgWarnings.SelectedRows.Count != 1) {
                e.Cancel = true;
                return;
            }
            switch ((string)fgWarnings.SelectedRows[0].Cells[0].Value) {
            case nameof(LogicCompileError):
                cmiDismiss.Visible = false;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = true;
                cmiGoWarning.Text = "Goto Compiler Error";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Compiler Error";
                break;
            case nameof(ResourceError):
                // non-logic  error
                cmiDismiss.Visible = false;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = false;
                cmiGoWarning.Text = "Goto ...";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Resource Error";
                break;
            case nameof(ResourceWarning):
                cmiDismiss.Visible = false;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = false;
                cmiGoWarning.Text = "Goto ...";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Resource Warning";
                // non-logic warning
                break;
            case nameof(DecompWarning):
                cmiDismiss.Visible = true;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = true;
                cmiGoWarning.Text = "Goto Decompiler Warning";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Decompiler Warning";
                break;
            case nameof(LogicCompileWarning):
                cmiDismiss.Visible = true;
                cmiIgnoreWarning.Visible = true;
                cmiGoWarning.Enabled = true;
                cmiGoWarning.Text = "Goto Compiler Warning";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Compiler Warning";
                break;
            case nameof(TODO):
                cmiDismiss.Visible = false;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = true;
                cmiGoWarning.Text = "Goto TODO";
                cmiHelp.Visible = true;
                cmiHelp.Text = "TODO Help";
                break;
            }
        }

        private void cmiDismiss_Click(object sender, EventArgs e) {
            DismissWarning(fgWarnings.SelectedRows[0].Index);
        }

        private void cmiDismissAll_Click(object sender, EventArgs e) {
            //delete everything except TODO entries
            DismissWarnings();
        }

        private void cmiIgnoreError_Click(object sender, EventArgs e) {
            // only if current row is a warning (>5000 and <6000)
            int warnnum = int.Parse((string)fgWarnings.SelectedRows[0].Cells[0].Value);
            if (warnnum > 5000 && warnnum < 6000) {
                IgnoreWarning(warnnum);
            }
        }

        private void cmiGoTODO_Click(object sender, EventArgs e) {
            EventType gotoevent = new();

            switch ((string)fgWarnings.SelectedRows[0].Cells[0].Value) {
            case nameof(LogicCompileError):
                gotoevent = LogicCompileError;
                break;
            case nameof(DecompWarning):
                gotoevent = DecompWarning;
                break;
            case nameof(LogicCompileWarning):
                gotoevent = LogicCompileWarning;
                break;
            case nameof(TODO):
                gotoevent = TODO;
                break;
            }
            int line = int.Parse((string)fgWarnings.SelectedRows[0].Cells[5].Value);
            string msg = (string)fgWarnings.SelectedRows[0].Cells[3].Value;
            int lognum = int.Parse((string)fgWarnings.SelectedRows[0].Cells[4].Value);
            string strModule = (string)fgWarnings.SelectedRows[0].Cells[7].Value;
            HighlightLine(line, msg, lognum, strModule, gotoevent);
        }

        private void cmiErrorHelp_Click(object sender, EventArgs e) {
            HelpWarning((string)fgWarnings.SelectedRows[0].Cells[0].Value, (string)fgWarnings.SelectedRows[0].Cells[2].Value);
        }
        #endregion

        #region Toolbar Button Event Handlers
        private void btnOpenGame_Click(object sender, EventArgs e) {
            OpenWAGFile();
        }

        private void btnCloseGame_Click(object sender, EventArgs e) {
            if (CloseThisGame()) {
                LastNodeName = "";
            }
        }

        private void btnRun_Click(object sender, EventArgs e) {
            RunGame();
        }

        private void btnNewLogic_Click(object sender, EventArgs e) {
            btnNewRes.DefaultItem = btnNewLogic;
            btnNewRes.Image = btnNewLogic.Image;
            NewLogic();
        }

        private void btnNewPicture_Click(object sender, EventArgs e) {
            btnNewRes.DefaultItem = btnNewPicture;
            btnNewRes.Image = btnNewPicture.Image;
            NewPicture();
        }

        private void btnNewSound_Click(object sender, EventArgs e) {
            btnNewRes.DefaultItem = btnNewSound;
            btnNewRes.Image = btnNewSound.Image;
            //create new logic and enter edit mode
            NewSound();
        }

        private void btnNewView_Click(object sender, EventArgs e) {
            btnNewRes.DefaultItem = btnNewView;
            btnNewRes.Image = btnNewView.Image;
            // create new view and enter edit mode
            NewView();
        }

        private void btnOpenLogic_Click(object sender, EventArgs e) {
            btnOpenRes.DefaultItem = btnOpenLogic;
            btnOpenRes.Image = btnOpenLogic.Image;
            mnuROLogic_Click(sender, e);
        }

        private void btnOpenPicture_Click(object sender, EventArgs e) {
            btnOpenRes.DefaultItem = btnOpenPicture;
            btnOpenRes.Image = btnOpenPicture.Image;
            mnuROPicture_Click(sender, e);
        }

        private void btnOpenSound_Click(object sender, EventArgs e) {
            btnOpenRes.DefaultItem = btnOpenSound;
            btnOpenRes.Image = btnOpenSound.Image;
            mnuROSound_Click(sender, e);
        }

        private void btnOpenView_Click(object sender, EventArgs e) {
            btnOpenRes.DefaultItem = btnOpenView;
            btnOpenRes.Image = btnOpenView.Image;
            mnuROView_Click(sender, e);
        }

        private void btnImportLogic_Click(object sender, EventArgs e) {
            btnImportRes.DefaultItem = btnImportLogic;
            btnImportRes.Image = btnImportLogic.Image;
            mnuRILogic_Click(sender, e);
        }

        private void btnImportPicture_Click(object sender, EventArgs e) {
            btnImportRes.DefaultItem = btnImportPicture;
            btnImportRes.Image = btnImportPicture.Image;
            mnuRIPicture_Click(sender, e);
        }

        private void btnImportSound_Click(object sender, EventArgs e) {
            btnImportRes.DefaultItem = btnImportSound;
            btnImportRes.Image = btnImportSound.Image;
            mnuRISound_Click(sender, e);
        }

        private void btnImportView_Click(object sender, EventArgs e) {
            btnImportRes.DefaultItem = btnImportView;
            btnImportRes.Image = btnImportView.Image;
            mnuRIView_Click(sender, e);
        }

        private void btnWords_Click(object sender, EventArgs e) {
            if (EditGame != null) {
                if (WEInUse) {
                    WordEditor.Select();
                }
                else {
                    OpenGameWORDSTOK();
                }
            }
            else {
                mnuROWords_Click(sender, e);
            }
        }

        private void btnOjects_Click(object sender, EventArgs e) {
            if (EditGame != null) {
                if (OEInUse) {
                    ObjectEditor.Select();
                }
                else {
                    OpenGameOBJECT();
                }
            }
            else {
                mnuROObjects_Click(sender, e);
            }
        }

        private void btnSaveResource_Click(object sender, EventArgs e) {
            if (ActiveMdiChild == null || ActiveMdiChild == PreviewWin) {
                mnuRSave_Click(sender, e);
            }
            else {
                dynamic form = ActiveMdiChild;
                form.mnuRSave_Click(sender, e);
            }
        }

        private void btnAddRemove_Click(object sender, EventArgs e) {
            if (ActiveMdiChild == null || ActiveMdiChild == PreviewWin) {
                mnuRRemove_Click(sender, e);
            }
            else {
                dynamic form = ActiveMdiChild;
                form.mnuRInGame_Click(sender, e);
            }
        }

        private void btnExportRes_Click(object sender, EventArgs e) {
            if (ActiveMdiChild == null || ActiveMdiChild == PreviewWin) {
                mnuRExport_Click(sender, e);
            }
            else {
                dynamic form = ActiveMdiChild;
                form.mnuRExport_Click(sender, e);
            }
        }

        private void btnLayoutEd_Click(object sender, EventArgs e) {
            MessageBox.Show("TODO: Layout Editor");
        }

        private void btnMenuEd_Click(object sender, EventArgs e) {
            MessageBox.Show("TODO: Menu Editor");
        }

        private void btnTextEd_Click(object sender, EventArgs e) {
            MessageBox.Show("TODO: Text Screen Editor");
        }

        private void btnGlobals_Click(object sender, EventArgs e) {
            MessageBox.Show("TODO: Globals Editor");
        }

        private void btnHelp_Click(object sender, EventArgs e) {
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Index);
        }
        #endregion

        #region AGI Game Event Handlers
        internal void GameEvents_CompileGameStatus(object sender, CompileGameEventArgs e) {
            // check for a cancel
            if (CompStatusWin.CompCanceled) {
                CompStatusWin.CompCanceled = false;
                e.Cancel = true;
                return;
            }
            // pass event to background task
            bgwCompGame.ReportProgress((int)e.CStatus, e.CompileInfo);
        }

        internal void GameEvents_NewGameStatus(object sender, NewGameEventArgs e) {
            // data field used to determine if event is a newgame vent or a load event
            // passed when template game is loaded
            switch (e.NewInfo.Type) {
            case EventType.Info:
                switch (e.NewInfo.InfoType) {
                case InfoType.Initialize:
                    bgwNewGame.ReportProgress(51, e.NewInfo.Text);
                    break;
                case InfoType.Resources:
                    bgwNewGame.ReportProgress(52, e.NewInfo.Text);
                    break;
                case InfoType.Finalizing:
                    bgwNewGame.ReportProgress(53, e.NewInfo.Text);
                    break;
                case InfoType.PropertyFile:
                    bgwNewGame.ReportProgress(54, "");
                    break;
                }
                break;
            case LogicCompileError:
                bgwNewGame.ReportProgress(1, e.NewInfo);
                break;
            case ResourceWarning:
                bgwNewGame.ReportProgress(1, e.NewInfo);
                break;
            case DecompWarning:
                bgwNewGame.ReportProgress(3, e.NewInfo);
                break;
            case TODO:
                // add to warning list
                bgwNewGame.ReportProgress(2, e.NewInfo);
                break;
            }
        }

        internal void GameEvents_LoadGameStatus(object sender, LoadGameEventArgs e) {
            switch (e.LoadInfo.Type) {
            case Info:
                switch (e.LoadInfo.InfoType) {
                case InfoType.Initialize:
                    break;
                case InfoType.Validating:
                    // check for WinAGI version update
                    if (e.LoadInfo.Text.Length > 0) {
                        bgwOpenGame.ReportProgress(4, "");
                    }
                    bgwOpenGame.ReportProgress(0, "Validating AGI game files ...");
                    break;
                case InfoType.PropertyFile:
                    if (e.LoadInfo.ResNum == 0) {
                        bgwOpenGame.ReportProgress(0, "Creating game property file ...");
                    }
                    else {
                        bgwOpenGame.ReportProgress(0, "Loading game property file ...");
                    }
                    break;
                case InfoType.Resources:
                    switch (e.LoadInfo.ResType) {
                    case AGIResType.Logic:
                    case AGIResType.Picture:
                    case AGIResType.View:
                    case AGIResType.Sound:
                        bgwOpenGame.ReportProgress(0, "Validating Resources: " + e.LoadInfo.ResType + " " + e.LoadInfo.ResNum);
                        break;
                    case Words:
                        bgwOpenGame.ReportProgress(0, "Validating WORDS.TOK file ...");
                        break;
                    case Objects:
                        bgwOpenGame.ReportProgress(0, "Validating OBJECT file ...");
                        break;
                    }
                    break;
                case InfoType.Decompiling:
                    bgwOpenGame.ReportProgress(0, "Validating AGI game files ...");
                    break;
                case InfoType.CheckCRC:
                    break;
                case InfoType.Finalizing:
                    bgwOpenGame.ReportProgress(0, "Configuring WinAGI");
                    break;
                }
                break;
            case LogicCompileError:
                bgwOpenGame.ReportProgress(0, $"Load Error: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to error list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
            case ResourceError:
                bgwOpenGame.ReportProgress(0, $"Load Error: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to error list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
            case ResourceWarning:
                bgwOpenGame.ReportProgress(0, $"Load Warning: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
            case LogicCompileWarning:
                bgwOpenGame.ReportProgress(0, $"Load Warning: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
            case DecompWarning:
                bgwOpenGame.ReportProgress(0, $"Load Warning: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
            case TODO:
                bgwOpenGame.ReportProgress(0, $"{e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(2, e.LoadInfo);
                break;
            }
        }

        internal void GameEvents_CompileLogicStatus(object sender, CompileLogicEventArgs e) {
            // TODO: event handling needs a complete overhaul- 
            // need custom arg objects for each event, split logic comp from game comp, 
            switch (e.CompInfo.Type) {
            case Info:
                switch (e.CompInfo.InfoType) {
                case InfoType.Initialize:

                    break;
                case InfoType.Validating:

                    break;
                case InfoType.PropertyFile:

                    break;
                case InfoType.Resources:

                    break;
                case InfoType.Decompiling:

                    break;
                case InfoType.CheckCRC:

                    break;
                case InfoType.Finalizing:

                    break;
                }
                break;
            case LogicCompileError:
                // error
                MDIMain.AddWarning(e.CompInfo);
                break;
            case ResourceWarning:

                break;
            case LogicCompileWarning:
                // warning
                MDIMain.AddWarning(e.CompInfo);
                break;
            case DecompWarning:

                break;
            case TODO:

                break;
            }
        }

        internal void GameEvents_DecodeLogicStatus(object sender, DecodeLogicEventArgs e) {
            // TODO: if not opening  game, find different way to report decode errors...

            Debug.Print($"decode it: {e.DecodeInfo.Text}");
            if (bgwOpenGame.IsBusy) {
                if (e.DecodeInfo.Type == EventType.TODO) {
                    bgwOpenGame?.ReportProgress(2, e.DecodeInfo);
                }
                else {
                    bgwOpenGame?.ReportProgress(3, e.DecodeInfo);
                }
            }
        }
        #endregion
        #endregion

        private void ResetStatusStrip() {
            spStatus.Text = "";
            for (int i = statusStrip1.Items.Count - 1; i >= 0; i--) {
                var item = statusStrip1.Items[i];
                if (item != spStatus && item != spCapsLock && item != spNumLock && item != spInsLock) {
                    statusStrip1.Items.Remove(item);
                }
            }
        }

        private void MergeStatusStrip(StatusStrip statusstrip, Form form) {
            switch (form) {
            case frmGlobals frmGE:
                // status
                spCapsLock.Visible = true;
                spNumLock.Visible = true;
                spInsLock.Visible = true;
                break;
            case frmLayout frmLO:
                statusStrip1.Items.Insert(0, frmLO.spScale);
                statusStrip1.Items.Insert(1, frmLO.spTool);
                statusStrip1.Items.Insert(2, frmLO.spID);
                statusStrip1.Items.Insert(3, frmLO.spType);
                statusStrip1.Items.Insert(4, frmLO.spRoom1);
                statusStrip1.Items.Insert(5, frmLO.spRoom2);
                //status
                statusStrip1.Items.Insert(7, frmLO.spCurX);
                statusStrip1.Items.Insert(8, frmLO.spCurY);
                spCapsLock.Visible = false;
                spNumLock.Visible = false;
                spInsLock.Visible = false;
                break;
            case frmLogicEdit frmLE:
                // status
                statusStrip1.Items.Insert(1, frmLE.spLine);
                statusStrip1.Items.Insert(2, frmLE.spColumn);
                spCapsLock.Visible = true;
                spNumLock.Visible = true;
                spInsLock.Visible = true;
                break;
            case frmMenuEdit frmME:
                // status
                spCapsLock.Visible = true;
                spNumLock.Visible = true;
                spInsLock.Visible = true;
                break;
            case frmObjectEdit frmOE:
                statusStrip1.Items.Insert(0, frmOE.spCount);
                statusStrip1.Items.Insert(1, frmOE.spEncrypt);
                // status
                spCapsLock.Visible = true;
                spNumLock.Visible = true;
                spInsLock.Visible = true;
                break;
            case frmPicEdit frmPE:
                statusStrip1.Items.Insert(0, frmPE.spScale);
                statusStrip1.Items.Insert(1, frmPE.spMode);
                statusStrip1.Items.Insert(2, frmPE.spTool);
                statusStrip1.Items.Insert(3, frmPE.spAnchor);
                statusStrip1.Items.Insert(4, frmPE.spBlock);
                // status
                statusStrip1.Items.Insert(6, frmPE.spCurX);
                statusStrip1.Items.Insert(7, frmPE.spCurY);
                statusStrip1.Items.Insert(8, frmPE.spPriBand);
                spCapsLock.Visible = false;
                spNumLock.Visible = false;
                spInsLock.Visible = false;
                break;
            case frmPreview frmPR:
                // status
                spCapsLock.Visible = true;
                spNumLock.Visible = true;
                spInsLock.Visible = true;
                break;
            case frmSoundEdit frmSE:
                statusStrip1.Items.Insert(0, frmSE.spScale);
                statusStrip1.Items.Insert(1, frmSE.spTime);
                // status
                spCapsLock.Visible = false;
                spNumLock.Visible = false;
                spInsLock.Visible = false;
                break;
            case frmTextScreenEdit frmTE:
                statusStrip1.Items.Insert(0, frmTE.spScale);
                statusStrip1.Items.Insert(1, frmTE.spMode);
                statusStrip1.Items.Insert(2, frmTE.spTool);
                // status
                spCapsLock.Visible = true;
                spNumLock.Visible = true;
                spInsLock.Visible = true;
                break;
            case frmViewEdit frmVE:
                statusStrip1.Items.Insert(0, frmVE.spScale);
                statusStrip1.Items.Insert(1, frmVE.spTool);
                // status
                spCapsLock.Visible = false;
                spNumLock.Visible = false;
                spInsLock.Visible = false;
                break;
            case frmWordsEdit frmWE:
                statusStrip1.Items.Insert(0, frmWE.spGroupCount);
                statusStrip1.Items.Insert(1, frmWE.spWordCount);
                // status
                spCapsLock.Visible = true;
                spNumLock.Visible = true;
                spInsLock.Visible = true;
                break;
            }
        }
        private void SplashTimer(object sender, EventArgs e) {
            // used by splash screen
            splashDone = true;
            tmrNavList.Enabled = false;
            // re-assign timer to navlist
            tmrNavList.Tick -= new System.EventHandler(SplashTimer);
            tmrNavList.Tick += new System.EventHandler(NavListTimer);
            tmrNavList.Interval = 200;
        }

        private void NavListTimer(object sender, EventArgs e) {
            //scroll the navlist
            if (NLRow < 0) {
                //scroll up
                if (NLOffset > 3) {
                    NLOffset--;
                    picNavList_Paint(sender, new PaintEventArgs(picNavList.CreateGraphics(), picNavList.Bounds));
                }
            }
            else {
                //scroll down
                if (NLOffset < ResQueue.Length - 3) {
                    NLOffset++;
                    picNavList_Paint(sender, new PaintEventArgs(picNavList.CreateGraphics(), picNavList.Bounds));
                }
            }
        }

        public void ShowResTree() {
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.None:
                // no tree
                // shouldn't get here, but
                return;

            case EResListType.TreeList:
                tvwResources.Visible = true;
                cmbResType.Visible = false;
                lstResources.Visible = false;
                // change font to match current preview font
                tvwResources.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
                break;
            case EResListType.ComboList:
                // combo/list boxes
                tvwResources.Visible = false;
                // set combo and listbox height, and set fonts
                cmbResType.Visible = true;
                cmbResType.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
                lstResources.Top = cmbResType.Top + cmbResType.Height + 2;
                lstResources.Visible = true;
                lstResources.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
                break;
            }
            // show and position the resource list panels
            pnlResources.Visible = true;
            splitResource.Visible = true;
        }

        public void HideResTree() {
            pnlResources.Visible = false;
            splitResource.Visible = false;
        }

        private void ResetTreeList() {
            if (tvwResources.Nodes.Count > 0) {
                // always collapse first
                tvwResources.Nodes[0].Collapse();
                // clear the treelist
                tvwResources.Nodes.Clear();
            }
            // add the base nodes
            tvwResources.Nodes.Add("root", "AGIGame");
            tvwResources.Nodes[0].Nodes.Add(sLOGICS, sLOGICS);
            tvwResources.Nodes[0].Nodes.Add(sPICTURES, sPICTURES);
            tvwResources.Nodes[0].Nodes.Add(sSOUNDS, sSOUNDS);
            tvwResources.Nodes[0].Nodes.Add(sVIEWS, sVIEWS);
            tvwResources.Nodes[0].Nodes.Add("Objects", "Objects");
            tvwResources.Nodes[0].Nodes.Add("Words", "Words");
            tvwResources.Nodes[0].Expand();
            // save reference to nodes for ease of acess
            RootNode = tvwResources.Nodes[0];
            HdrNode = new TreeNode[6];
            for (int i = 0; i < 6; i++) {
                HdrNode[i] = RootNode.Nodes[i];
            }
        }

        public void ClearResourceList() {
            // reset the navigation queue and don't add to queue while clearing
            ResetQueue();
            DontQueue = true;
            // list type determines clear actions
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                ResetTreeList();
                break;
            case EResListType.ComboList:
                cmbResType.Items[0] = "AGIGame";
                lstResources.Items.Clear();
                break;
            }
            SelResType = AGIResType.Game;
            SelResNum = -1;
            // reenable queuing
            DontQueue = false;
        }

        public void SelectResource(AGIResType NewResType, int NewResNum, bool UpdatePreview = true) {
            // selects a resource for previewing
            // (always synched with the resource list)

            // always unload the current resource, if it's logic/pic/sound/view
            if (SelResNum != -1) {
                switch (SelResType) {
                case AGIResType.Logic:
                    EditGame.Logics[SelResNum].Unload();
                    break;
                case AGIResType.Picture:
                    EditGame.Pictures[SelResNum].Unload();
                    break;
                case AGIResType.Sound:
                    EditGame.Sounds[SelResNum].Unload();
                    break;
                case AGIResType.View:
                    EditGame.Views[SelResNum].Unload();
                    break;
                }
            }
            RefreshPropertyGrid(NewResType, NewResNum);
            if (WinAGISettings.ShowPreview.Value) {
                // if update is requested
                if (WinAGISettings.ShowPreview.Value && UpdatePreview) {
                    // load the preview item
                    PreviewWin.LoadPreview(NewResType, NewResNum);
                }
            }

            //update selection properties
            SelResType = NewResType;
            SelResNum = NewResNum;
            if (WinAGISettings.ResListType.Value != EResListType.None) {
                //add selected resource to navigation queue
                AddToQueue(SelResType, SelResNum < 0 ? 256 : SelResNum);
                // always disable forward button
                cmdForward.Enabled = false;
                // enable back button if at least two in queue
                cmdBack.Enabled = ResQPtr > 0;
                //if a logic is selected, and layout editor is active form
                if (SelResType == AGIResType.Logic) {
                    //if syncing the layout editor and the treeview list
                    if (WinAGISettings.LESync.Value) {
                        if (ActiveMdiChild is not null) {
                            if (ActiveMdiChild is frmLayout) {
                                if (EditGame.Logics[(byte)SelResNum].IsRoom) {
                                    //if option to sync is set
                                    LayoutEditor.SelectRoom(SelResNum);
                                }
                            }
                        }
                    }
                }
            }
            UpdateTBResourceBtns(SelResType, true, false);

        }

        void SelectFromQueue() {
            //selects the node/resource from the current queue position
            //
            //if the current resource is gone (deleted)
            //the function will select the appropriate header
            //
            // while selecting from queue, disable additions to the
            // queue

            AGIResType ResType;
            int ResNum;
            string strKey = "";
            // make sure queue has something
            if (ResQPtr < 0) {
                return;
            }
            //disable queue addition
            DontQueue = true;
            //extract restype and number from the resqueue
            ResType = (AGIResType)(ResQueue[ResQPtr] >> 16);
            ResNum = ResQueue[ResQPtr] & 0xFFFF;

            if (ResNum == 256) {
                // header
                switch (WinAGISettings.ResListType.Value) {
                case EResListType.TreeList:
                    // treelist
                    if (ResType == Game) {
                        tvwResources.SelectedNode = RootNode;
                    }
                    else {
                        tvwResources.SelectedNode = HdrNode[(int)ResType];
                    }
                    // call the node click to finish selection
                    tvwResources_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(tvwResources.SelectedNode, MouseButtons.None, 0, 0, 0));
                    break;
                case EResListType.ComboList:
                    // listbox
                    switch (ResType) {
                    case Game: //root
                        cmbResType.SelectedIndex = 0;
                        //then force selection change
                        SelectResource(Game, -1);
                        break;
                    default:
                        //(resnum+1 matches desired listindex)
                        cmbResType.SelectedIndex = (int)(ResType + 1);
                        //reset the listbox
                        lstResources.SelectedItems.Clear();
                        //then force selection change
                        SelectResource(ResType, -1);
                        break;
                    }
                    break;
                }
            }
            else {
                //does the resource still exist?
                switch (ResType) {
                case AGIResType.Logic:
                    if (EditGame.Logics.Contains((byte)ResNum)) {
                        strKey = "l" + ResNum;
                    }
                    break;
                case AGIResType.Picture:
                    if (EditGame.Pictures.Contains((byte)ResNum)) {
                        strKey = "p" + ResNum;
                    }
                    break;
                case AGIResType.Sound:
                    if (EditGame.Sounds.Contains((byte)ResNum)) {
                        strKey = "s" + ResNum;
                    }
                    break;
                case AGIResType.View:
                    if (EditGame.Views.Contains((byte)ResNum)) {
                        strKey = "v" + ResNum;
                    }
                    break;
                }

                //if no key
                if (strKey.Length == 0) {
                    //this resource doesn't exist anymore - probably
                    //deleted; select the header
                    switch (WinAGISettings.ResListType.Value) {
                    case EResListType.TreeList:
                        // treelist
                        tvwResources.SelectedNode = HdrNode[(int)ResType];
                        tvwResources_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(tvwResources.SelectedNode, MouseButtons.None, 0, 0, 0));
                        break;
                    case EResListType.ComboList:
                        //(restype+1 matches desired combobox index)
                        cmbResType.SelectedIndex = (int)(ResType + 1);
                        //reset the listbox
                        lstResources.SelectedItems.Clear();
                        //then force selection change
                        SelectResource(ResType, -1);
                        break;
                    }
                    return;
                }
                //select this resource
                switch (WinAGISettings.ResListType.Value) {
                case EResListType.TreeList:
                    tvwResources.SelectedNode = HdrNode[(int)ResType].Nodes[strKey];
                    break;
                case EResListType.ComboList:
                    // (restype+1 matches desired combobox index)
                    cmbResType.SelectedIndex = (int)(ResType + 1);
                    // now select the resource
                    lstResources.Items[strKey].Selected = true;
                    break;
                }
                //force selection
                SelectResource(ResType, ResNum);
            }

            //restore queue addition
            DontQueue = false;
            return;
        }

        public void AddResourceToList(AGIResType restype, byte resnum) {
            int pos;
            switch (WinAGISettings.ResListType.Value) {
            case EResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)restype];
                //find place to insert this resource
                for (pos = 0; pos < HdrNode[(int)restype].Nodes.Count; pos++) {
                    if ((byte)tmpNode.Nodes[pos].Tag > resnum) {
                        break;
                    }
                }
                //add to tree
                switch (restype) {
                case AGIResType.Logic:
                    tmpNode = HdrNode[0].Nodes.Insert(pos, "l" + resnum, ResourceName(EditGame.Logics[resnum], true));
                    // set source compiled status
                    tmpNode.ForeColor = EditGame.Logics[resnum].Compiled ? Color.Black : Color.Red;
                    break;
                case AGIResType.Picture:
                    tmpNode = HdrNode[1].Nodes.Insert(pos, "p" + resnum, ResourceName(EditGame.Pictures[resnum], true));
                    break;
                case AGIResType.Sound:
                    tmpNode = HdrNode[2].Nodes.Insert(pos, "s" + resnum, ResourceName(EditGame.Sounds[resnum], true));
                    break;
                case AGIResType.View:
                    tmpNode = HdrNode[3].Nodes.Insert(pos, "v" + resnum, ResourceName(EditGame.Views[resnum], true));
                    break;
                }
                tmpNode.Tag = resnum;
                break;
            case EResListType.ComboList:
                //only update if logics are being listed
                if (MDIMain.cmbResType.SelectedIndex - 1 == (int)restype) {
                    ListViewItem tmpListItem = null;
                    //find a place to insert this resource in the box list
                    for (pos = 0; pos < MDIMain.lstResources.Items.Count; pos++) {
                        if ((byte)MDIMain.lstResources.Items[pos].Tag > resnum) {
                            break;
                        }
                    }
                    switch (restype) {
                    case AGIResType.Logic:
                        tmpListItem = MDIMain.lstResources.Items.Insert(pos, "l" + resnum, ResourceName(EditGame.Logics[resnum], true), 0);
                        // set source compiled status
                        tmpListItem.ForeColor = EditGame.Logics[resnum].Compiled ? Color.Black : Color.Red;
                        break;
                    case AGIResType.Picture:
                        tmpListItem = MDIMain.lstResources.Items.Insert(pos, "p" + resnum, ResourceName(EditGame.Pictures[resnum], true), 0);
                        break;
                    case AGIResType.Sound:
                        tmpListItem = MDIMain.lstResources.Items.Insert(pos, "s" + resnum, ResourceName(EditGame.Sounds[resnum], true), 0);
                        break;
                    case AGIResType.View:
                        tmpListItem = MDIMain.lstResources.Items.Insert(pos, "v" + resnum, ResourceName(EditGame.Views[resnum], true), 0);
                        break;
                    }
                    tmpListItem.Tag = resnum;
                }
                break;
            }
            // update the logic tooltip lookup table
            switch (restype) {
            case AGIResType.Logic:
                IDefLookup[(int)restype, resnum].Name = EditGame.Logics[resnum].ID;
                break;
            case AGIResType.Picture:
                IDefLookup[(int)restype, resnum].Name = EditGame.Pictures[resnum].ID;
                break;
            case AGIResType.Sound:
                IDefLookup[(int)restype, resnum].Name = EditGame.Sounds[resnum].ID;
                break;
            case AGIResType.View:
                IDefLookup[(int)restype, resnum].Name = EditGame.Views[resnum].ID;
                break;
            }
            IDefLookup[(int)restype, resnum].Type = ArgType.Num;
            // then let open logic editors know
            foreach (frmLogicEdit frm in LogicEditors) {
                frm.ListChanged = true;
            }
            // last node marker is no longer accurate; reset
            MDIMain.LastNodeName = "";
        }

        public void RenumberSelectedResource() {
            // renumbers the preview resource -
            // depending on selected type, look for an open editor first
            // if found, use that editor's function; if not found use the
            // default function

            switch (SelResType) {
            case AGIResType.Logic:
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.FormMode == LogicFormMode.Logic) {
                        if (frm.LogicNumber == SelResNum) {
                            frm.RenumberLogic();
                            return;
                        }
                    }
                }
                break;
            case AGIResType.Picture:
                foreach (frmPicEdit frm in PictureEditors) {
                    if (frm.PictureNumber == SelResNum) {
                        frm.RenumberPicture();
                        return;
                    }
                }
                break;
            case AGIResType.Sound:
                foreach (frmSoundEdit frm in SoundEditors) {
                    if (frm.SoundNumber == SelResNum) {
                        frm.RenumberSound();
                        return;
                    }
                }
                break;
            case AGIResType.View:
                foreach (frmViewEdit frm in ViewEditors) {
                    if (frm.ViewNumber == SelResNum) {
                        frm.RenumberView();
                        return;
                    }
                }
                break;
            default:
                // words, objects, game or none
                // Renumber does not apply
                return;
            }
            // no open editor; renumber using default process
            GetNewNumber(SelResType, (byte)SelResNum);
        }

        public void RemoveSelectedResource() {
            // removing a preview resource -
            // depending on selected type, look for an open editor first
            // if found, use that editor's function; if not found use the
            // default function

            switch (SelResType) {
            case AGIResType.Logic:
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.FormMode == LogicFormMode.Logic) {
                        if (frm.LogicNumber == SelResNum) {
                            frm.ToggleInGame();
                            return;
                        }
                    }
                }
                break;
            case AGIResType.Picture:
                foreach (frmPicEdit frm in PictureEditors) {
                    if (frm.PictureNumber == SelResNum) {
                        frm.ToggleInGame();
                        return;
                    }
                }
                break;
            case AGIResType.Sound:
                foreach (frmSoundEdit frm in SoundEditors) {
                    if (frm.SoundNumber == SelResNum) {
                        frm.ToggleInGame();
                        return;
                    }
                }
                break;
            case AGIResType.View:
                foreach (frmViewEdit frm in ViewEditors) {
                    if (frm.ViewNumber == SelResNum) {
                        frm.ToggleInGame();
                        return;
                    }
                }
                break;
            default:
                //words, objects, game or none
                //InGame does not apply
                return;
            }
            // no open editor; remove it using default process
            DialogResult rtn;
            bool blnDontAsk = false, haserror = false;
            string strID = "";
            switch (SelResType) {
            case AGIResType.Logic:
                strID = EditGame.Logics[SelResNum].ID;
                haserror = EditGame.Logics[SelResNum].ErrLevel < 0;
                break;
            case AGIResType.Picture:
                strID = EditGame.Pictures[SelResNum].ID;
                haserror = EditGame.Pictures[SelResNum].ErrLevel < 0;
                break;
            case AGIResType.Sound:
                strID = EditGame.Sounds[SelResNum].ID;
                haserror = EditGame.Sounds[SelResNum].ErrLevel < 0;
                break;
            case AGIResType.View:
                strID = EditGame.Views[SelResNum].ID;
                haserror = EditGame.Views[SelResNum].ErrLevel < 0;
                break;
            default:
                Debug.Assert(false);
                return;
            }
            if (WinAGISettings.AskExport.Value && !haserror) {
                rtn = MsgBoxEx.Show(MDIMain,
                    "Do you want to export '" + strID + "' before removing it from your game?",
                    "Export " + SelResType.ToString() + " Before Removal",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    "Don't ask this question again", ref blnDontAsk);
                WinAGISettings.AskExport.Value = !blnDontAsk;
                if (!WinAGISettings.AskExport.Value) {
                    WinAGISettings.AskExport.WriteSetting(WinAGISettingsFile);
                }
            }
            else {
                rtn = DialogResult.No;
            }
            switch (rtn) {
            case DialogResult.Cancel:
                return;
            case DialogResult.Yes:
                switch (SelResType) {
                case AGIResType.Logic:
                    _ = ExportLogic(EditGame.Logics[SelResNum], true);
                    break;
                case AGIResType.Picture:
                    _ = ExportPicture(EditGame.Pictures[SelResNum], true);
                    break;
                case AGIResType.Sound:
                    ExportSound(EditGame.Sounds[SelResNum], true);
                    break;
                case AGIResType.View:
                    ExportView(EditGame.Views[SelResNum], true);
                    break;
                }
                break;
            case DialogResult.No:
                break;
            }
            if (WinAGISettings.AskRemove.Value) {
                blnDontAsk = false;
                rtn = MsgBoxEx.Show(MDIMain,
                    "Removing '" + strID + "' from your game.\n\nSelect OK to proceed, or Cancel to keep it in game.",
                    "Remove " + SelResType.ToString() + " From Game",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    "Don't ask this question again", ref blnDontAsk);
                WinAGISettings.AskRemove.Value = !blnDontAsk;
                if (!WinAGISettings.AskRemove.Value) {
                    WinAGISettings.AskRemove.WriteSetting(WinAGISettingsFile);
                }
            }
            else {
                rtn = DialogResult.OK;
            }
            if (rtn == DialogResult.Cancel) {
                return;
            }
            // clear warnings for this resource
            ClearWarnings(SelResType, (byte)SelResNum);
            // now remove it
            switch (SelResType) {
            case AGIResType.Logic:
                RemoveLogic((byte)SelResNum);
                break;
            case AGIResType.Picture:
                RemovePicture((byte)SelResNum);
                break;
            case AGIResType.Sound:
                RemoveSound((byte)SelResNum);
                break;
            case AGIResType.View:
                RemoveView((byte)SelResNum);
                break;
            }
        }

        public void EditSelectedItemProperties(int FirstProp) {
            // only use for resources that are NOT being edited;
            // if the resource is being edited, the editor for that
            // resource handles description, ID and number changes
            string strID = "", strDesc = "";

            switch (SelResType) {
            case AGIResType.Logic:
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.LogicNumber == SelResNum) {
                        frm.EditLogicProperties(1);
                        return;
                    }
                }
                strID = EditGame.Logics[SelResNum].ID;
                strDesc = EditGame.Logics[SelResNum].Description;
                break;
            case AGIResType.Picture:
                foreach (frmPicEdit frm in PictureEditors) {
                    if (frm.PictureNumber == SelResNum) {
                        frm.EditPictureProperties(1);
                        return;
                    }
                }
                strID = EditGame.Pictures[SelResNum].ID;
                strDesc = EditGame.Pictures[SelResNum].Description;
                break;
            case AGIResType.Sound:
                foreach (frmSoundEdit frm in SoundEditors) {
                    if (frm.SoundNumber == SelResNum) {
                        frm.EditSoundProperties(1);
                        return;
                    }
                }
                strID = EditGame.Sounds[SelResNum].ID;
                strDesc = EditGame.Sounds[SelResNum].Description;
                break;
            case AGIResType.View:
                foreach (frmViewEdit frm in ViewEditors) {
                    if (frm.ViewNumber == SelResNum) {
                        frm.EditViewProperties(1);
                        return;
                    }
                }
                strID = EditGame.Views[SelResNum].ID;
                strDesc = EditGame.Views[SelResNum].Description;
                break;
            case AGIResType.Objects:
                strDesc = EditGame.InvObjects.Description;
                break;
            case AGIResType.Words:
                strDesc = EditGame.WordList.Description;
                break;
            }
            _ = GetNewResID(SelResType, SelResNum, ref strID, ref strDesc, true, FirstProp);
            return;
        }

        bool ReadSettings() {
            double sngProp;
            double sngTop, sngLeft;
            double sngWidth, sngHeight;
            int i, lngNoCompVal;
            bool blnMax;

            // open the program settings  file
            try {
                WinAGISettingsFile = new SettingsFile(ProgramDir + "winagi.config", FileMode.OpenOrCreate);
            }
            catch (WinAGIException wex) {
                if (wex.HResult == WINAGI_ERR + 700) {
                    // readonly - unable to change it
                    MessageBox.Show("Configuration file (winagi.config) is marked 'readonly'. It must be 'read/write' " +
                                    "for WinAGI to load. Change the file's property then try again.",
                                    "Configuration File Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                }
                else {
                    // file access error; try renaming it and creating a default list
                    try {
                        // bad file; 
                        File.Move(ProgramDir + "winagi.config", ProgramDir + "winagi_OLD.config");
                        WinAGISettingsFile = new SettingsFile(ProgramDir + "winagi.config", FileMode.Create);
                    }
                    catch {
                        // unrecoverable error
                        MessageBox.Show("Unable to read configuration file (winagi.config). It may be corrupt. Try " +
                                        "deleting it then restart WinAGI to restore default settings.",
                                        "Configuration File Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Close();
                    }
                }
            }
            // WARNINGS
            WinAGISettings.RenameWAG.ReadSetting(WinAGISettingsFile);
            WinAGISettings.OpenNew.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AskExport.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AskRemove.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AutoUpdateDefines.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AutoUpdateResDefs.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AutoUpdateWords.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AutoUpdateObjects.ReadSetting(WinAGISettingsFile);
            WinAGISettings.WarnDupGName.ReadSetting(WinAGISettingsFile);
            WinAGISettings.WarnDupGVal.ReadSetting(WinAGISettingsFile);
            WinAGISettings.WarnInvalidStrVal.ReadSetting(WinAGISettingsFile);
            WinAGISettings.WarnInvalidCtlVal.ReadSetting(WinAGISettingsFile);
            WinAGISettings.WarnResOverride.ReadSetting(WinAGISettingsFile);
            WinAGISettings.WarnDupObj.ReadSetting(WinAGISettingsFile);
            WinAGISettings.WarnCompile.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DelBlankG.ReadSetting(WinAGISettingsFile);
            WinAGISettings.NotifyCompSuccess.ReadSetting(WinAGISettingsFile);
            //WinAGISettings.NotifyCompWarn.ReadSetting(WinAGISettingsFile);
            WinAGISettings.NotifyCompFail.ReadSetting(WinAGISettingsFile);
            WinAGISettings.WarnItem0.ReadSetting(WinAGISettingsFile);
            WinAGISettings.SaveOnCompile.ReadSetting(WinAGISettingsFile);
            WinAGISettings.CompileOnRun.ReadSetting(WinAGISettingsFile);
            WinAGISettings.WarnMsgs.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.WarnMsgs.Value < 0) {
                WinAGISettings.WarnMsgs.Value = 0;
            }
            if (WinAGISettings.WarnMsgs.Value > 2) {
                WinAGISettings.WarnMsgs.Value = 2;
            }
            WinAGISettings.WarnEncrypt.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEDelPicToo.ReadSetting(WinAGISettingsFile);
            //WinAGISettings.WarnPlotPaste.ReadSetting(WinAGISettingsFile);
            // GENERAL
            WinAGISettings.ShowSplashScreen.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowPreview.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShiftPreview.ReadSetting(WinAGISettingsFile);
            WinAGISettings.HidePreview.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ResListType.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AutoExport.ReadSetting(WinAGISettingsFile);
            WinAGISettings.BackupResFile.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefMaxSO.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.DefMaxSO.Value < 1) {
                WinAGISettings.DefMaxSO.Value = 1;
            }
            Engine.Base.DefMaxSO = WinAGISettings.DefMaxSO.Value;
            WinAGISettings.DefMaxVol0.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.DefMaxVol0.Value < 32768) {
                WinAGISettings.DefMaxVol0.Value = 32768;
            }
            if (WinAGISettings.DefMaxVol0.Value > 1047552) {
                WinAGISettings.DefMaxVol0.Value = 1047552;
            }
            Engine.Base.DefMaxVol0Size = WinAGISettings.DefMaxVol0.Value;
            WinAGISettings.DefCP.ReadSetting(WinAGISettingsFile);
            switch (WinAGISettings.DefCP.Value) {
            case 437 or 850 or 852 or 855 or 857 or 858 or 860 or 861 or 863 or 869:
                break;
            default:
                WinAGISettings.DefCP.Reset();
                break;
            }
            CodePage = Encoding.GetEncoding(WinAGISettings.DefCP.Value);
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
            Engine.Base.DefResDir = WinAGISettings.DefResDir.Value;

            // MRU MENU LIST
            WinAGISettings.AutoOpen.ReadSetting(WinAGISettingsFile);

            // RESFORMAT
            WinAGISettings.ShowResNum.ReadSetting(WinAGISettingsFile);
            WinAGISettings.IncludeResNum.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ResFormatNameCase.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.ResFormatNameCase.Value < 0 || WinAGISettings.ResFormatNameCase.Value > 2) {
                WinAGISettings.ResFormatNameCase.Value = 2;
            }
            WinAGISettings.ResFormatSeparator.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.ResFormatSeparator.Value.Length > 1) {
                WinAGISettings.ResFormatSeparator.Value = WinAGISettings.ResFormatSeparator.Value[0..0];
            }
            WinAGISettings.ResFormatNumFormat.ReadSetting(WinAGISettingsFile);
            WinAGISettings.OpenOnErr.ReadSetting(WinAGISettingsFile);

            // LOGICS
            WinAGISettings.AutoWarn.ReadSetting(WinAGISettingsFile);
            WinAGISettings.HighlightLogic.ReadSetting(WinAGISettingsFile);
            WinAGISettings.HighlightText.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LogicTabWidth.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.LogicTabWidth.Value < 1) {
                WinAGISettings.LogicTabWidth.Value = 1;
            }
            if (WinAGISettings.LogicTabWidth.Value > 32) {
                WinAGISettings.LogicTabWidth.Value = 32;
            }
            WinAGISettings.MaximizeLogics.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AutoQuickInfo.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowDefTips.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowDocMap.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowLineNumbers.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefaultExt.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefaultExt.Value = WinAGISettings.DefaultExt.Value.ToLower().Trim();
            if (WinAGISettings.DefaultExt.Value[0] == '.') {
                WinAGISettings.DefaultExt.Value = WinAGISettings.DefaultExt.Value[1..];
            }
            // deoder uses default extension
            LogicDecoder.DefaultSrcExt = WinAGISettings.DefaultExt.Value;
            WinAGISettings.EditorFontName.ReadSetting(WinAGISettingsFile);
            if (!IsFontInstalled(WinAGISettings.EditorFontName.Value)) {
                WinAGISettings.EditorFontName.Reset();
            }
            WinAGISettings.EditorFontSize.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.EditorFontSize.Value < 8) {
                WinAGISettings.EditorFontSize.Value = 8;
            }
            if (WinAGISettings.EditorFontSize.Value > 24) {
                WinAGISettings.EditorFontSize.Value = 24;
            }
            WinAGISettings.PreviewFontName.ReadSetting(WinAGISettingsFile);
            if (!IsFontInstalled(WinAGISettings.PreviewFontName.Value)) {
                WinAGISettings.PreviewFontName.Reset();
            }
            WinAGISettings.PreviewFontSize.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.PreviewFontSize.Value < 6) {
                WinAGISettings.PreviewFontSize.Value = 6;
            }
            if (WinAGISettings.PreviewFontSize.Value > 24) {
                WinAGISettings.PreviewFontSize.Value = 24;
            }
            WinAGISettings.ErrorLevel.ReadSetting(WinAGISettingsFile);
            LogicCompiler.ErrorLevel = WinAGISettings.ErrorLevel.Value;
            WinAGISettings.DefIncludeIDs.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefIncludeReserved.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefIncludeGlobals.ReadSetting(WinAGISettingsFile);
            WinAGISettings.UseSnippets.ReadSetting(WinAGISettingsFile);

            // SYNTAXHIGHLIGHTFORMAT
            WinAGISettings.EditorBackColor.ReadSetting(WinAGISettingsFile);
            for (int j = 0; j < 10; j++) {
                WinAGISettings.SyntaxStyle[j].Color.ReadSetting(WinAGISettingsFile);
                WinAGISettings.SyntaxStyle[j].FontStyle.ReadSetting(WinAGISettingsFile);
                if (WinAGISettings.SyntaxStyle[j].FontStyle.Value < 0) {
                    WinAGISettings.SyntaxStyle[j].FontStyle.Value = 0;
                }
                if ((int)WinAGISettings.SyntaxStyle[j].FontStyle.Value > 3) {
                    WinAGISettings.SyntaxStyle[j].FontStyle.Value = (FontStyle)3;
                }
            }

            // LOGIC DECOMPILER
            LogicDecoder.MsgsByNumber = WinAGISettings.MsgsByNumber.ReadSetting(WinAGISettingsFile);
            LogicDecoder.IObjsByNumber = WinAGISettings.IObjsByNumber.ReadSetting(WinAGISettingsFile);
            LogicDecoder.WordsByNumber = WinAGISettings.WordsByNumber.ReadSetting(WinAGISettingsFile);
            LogicDecoder.ShowAllMessages = WinAGISettings.ShowAllMessages.ReadSetting(WinAGISettingsFile);
            LogicDecoder.SpecialSyntax = WinAGISettings.SpecialSyntax.ReadSetting(WinAGISettingsFile);
            LogicDecoder.ReservedAsText = WinAGISettings.ReservedAsText.ReadSetting(WinAGISettingsFile);
            LogicDecoder.CodeStyle = WinAGISettings.CodeStyle.ReadSetting(WinAGISettingsFile);

            // PICTURES
            WinAGISettings.ShowBands.ReadSetting(WinAGISettingsFile);
            WinAGISettings.SplitWindow.ReadSetting(WinAGISettingsFile);
            WinAGISettings.PicScalePreview.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.PicScalePreview.Value < 1) {
                WinAGISettings.PicScalePreview.Value = 1;
            }
            if (WinAGISettings.PicScalePreview.Value > 4) {
                WinAGISettings.PicScalePreview.Value = 4;
            }
            WinAGISettings.PicScaleEdit.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.PicScaleEdit.Value < 1) {
                WinAGISettings.PicScaleEdit.Value = 1;
            }
            if (WinAGISettings.PicScaleEdit.Value > 4) {
                WinAGISettings.PicScaleEdit.Value = 4;
            }
            WinAGISettings.CursorMode.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.CursorMode.Value < 0) {
                WinAGISettings.CursorMode.Value = 0;
            }
            if (WinAGISettings.CursorMode.Value > 1) {
                WinAGISettings.CursorMode.Value = 1;
            }

            // PICTEST
            PicEditTestSettings.ObjSpeed.ReadSetting(WinAGISettingsFile);
            PicEditTestSettings.ObjPriority.ReadSetting(WinAGISettingsFile);
            PicEditTestSettings.ObjRestriction.ReadSetting(WinAGISettingsFile);
            PicEditTestSettings.Horizon.ReadSetting(WinAGISettingsFile);
            PicEditTestSettings.IgnoreHorizon.ReadSetting(WinAGISettingsFile);
            PicEditTestSettings.IgnoreBlocks.ReadSetting(WinAGISettingsFile);
            PicEditTestSettings.CycleAtRest.ReadSetting(WinAGISettingsFile);

            // SOUNDS
            WinAGISettings.ShowKeyboard.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowNotes.ReadSetting(WinAGISettingsFile);
            WinAGISettings.OneTrack.ReadSetting(WinAGISettingsFile);
            for (i = 0; i < 3; i++) {
                WinAGISettings.DefInst[i].ReadSetting(WinAGISettingsFile);
            }
            for (i = 0; i < 4; i++) {
                WinAGISettings.DefMute[i].ReadSetting(WinAGISettingsFile);
            }
            WinAGISettings.SndZoom.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.SndZoom.Value < 1) {
                WinAGISettings.SndZoom.Value = 1;
            }
            if (WinAGISettings.SndZoom.Value > 3) {
                WinAGISettings.SndZoom.Value = 3;
            }

            // VIEWS
            WinAGISettings.ShowVEPreview.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefPrevPlay.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowGrid.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ViewAlignH.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.ViewAlignH.Value < 0) {
                WinAGISettings.ViewAlignH.Value = 0;
            }
            if (WinAGISettings.ViewAlignH.Value > 2) {
                WinAGISettings.ViewAlignH.Value = 2;
            }
            WinAGISettings.ViewAlignV.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.ViewAlignV.Value < 0) {
                WinAGISettings.ViewAlignV.Value = 0;
            }
            if (WinAGISettings.ViewAlignV.Value > 2) {
                WinAGISettings.ViewAlignV.Value = 2;
            }
            WinAGISettings.DefCelH.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.DefCelH.Value < 1) {
                WinAGISettings.DefCelH.Value = 1;
            }
            if (WinAGISettings.DefCelH.Value > 167) {
                WinAGISettings.DefCelH.Value = 167;
            }
            WinAGISettings.DefCelW.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.DefCelW.Value < 1) {
                WinAGISettings.DefCelW.Value = 1;
            }
            if (WinAGISettings.DefCelW.Value > 160) {
                WinAGISettings.DefCelW.Value = 160;
            }
            WinAGISettings.ViewScalePreview.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.ViewScalePreview.Value < 1) {
                WinAGISettings.ViewScalePreview.Value = 1;
            }
            if (WinAGISettings.ViewScalePreview.Value > 10) {
                WinAGISettings.ViewScalePreview.Value = 10;
            }
            WinAGISettings.ViewScaleEdit.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.ViewScaleEdit.Value < 1) {
                WinAGISettings.ViewScaleEdit.Value = 1;
            }
            if (WinAGISettings.ViewScaleEdit.Value > 10) {
                WinAGISettings.ViewScaleEdit.Value = 10;
            }
            WinAGISettings.DefVColor1.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.DefVColor1.Value < 0) {
                WinAGISettings.DefVColor1.Value = 0;
            }
            if (WinAGISettings.DefVColor1.Value > 15) {
                WinAGISettings.DefVColor1.Value = 15;
            }
            WinAGISettings.DefVColor2.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.DefVColor2.Value < 0) {
                WinAGISettings.DefVColor2.Value = 0;
            }
            if (WinAGISettings.DefVColor2.Value > 15) {
                WinAGISettings.DefVColor2.Value = 15;
            }

            // OBJECT
            // none

            // WORDS.TOK
            // none

            // LAYOUT EDITOR
            WinAGISettings.DefUseLE.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEPages.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEShowPics.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEUseGrid.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEGrid.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEGrid.Value = Math.Round(WinAGISettings.LEGrid.Value, 2);
            if (WinAGISettings.LEGrid.Value > 1) {
                WinAGISettings.LEGrid.Value = 1;
            }
            if (WinAGISettings.LEGrid.Value < 0.05) {
                WinAGISettings.LEGrid.Value = 0.05;
            }
            WinAGISettings.LESync.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEZoom.ReadSetting(WinAGISettingsFile);
            WinAGISettings.RoomEdgeColor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.RoomFillColor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.TransPtEdgeColor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.TransPtFillColor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.CmtEdgeColor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.CmtFillColor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ErrPtEdgeColor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ErrPtFillColor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ExitEdgeColor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ExitOtherColor.ReadSetting(WinAGISettingsFile);

            //GLOBAL EDITOR
            WinAGISettings.GEShowComment.ReadSetting(WinAGISettingsFile);
            WinAGISettings.GENameFrac.ReadSetting(WinAGISettingsFile);
            WinAGISettings.GEValFrac.ReadSetting(WinAGISettingsFile);

            // MENU EDITOR
            WinAGISettings.AutoAlignHotKey.ReadSetting(WinAGISettingsFile);

            // PLATFORM DEFAULTS
            WinAGISettings.AutoFill.ReadSetting(WinAGISettingsFile);
            WinAGISettings.PlatformType.ReadSetting(WinAGISettingsFile);
            WinAGISettings.PlatformFile.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DOSExec.ReadSetting(WinAGISettingsFile);
            WinAGISettings.PlatformOpts.ReadSetting(WinAGISettingsFile);

            // non-setting properties:

            // WINDOW POSITION
            blnMax = WinAGISettingsFile.GetSetting(sPOSITION, "WindowMax", false);
            sngLeft = WinAGISettingsFile.GetSetting(sPOSITION, "Left", Screen.PrimaryScreen.Bounds.Width * 0.15);
            sngTop = WinAGISettingsFile.GetSetting(sPOSITION, "Top", Screen.PrimaryScreen.Bounds.Height * 0.15);
            sngWidth = WinAGISettingsFile.GetSetting(sPOSITION, "Width", Screen.PrimaryScreen.Bounds.Width * 0.7);
            sngHeight = WinAGISettingsFile.GetSetting(sPOSITION, "Height", Screen.PrimaryScreen.Bounds.Height * 0.7);
            if (sngWidth < 360) {
                sngWidth = 360;
            }
            if (sngWidth > SystemInformation.VirtualScreen.Width) {
                sngWidth = SystemInformation.VirtualScreen.Width;
            }
            if (sngHeight < 361) {
                sngHeight = 361;
            }
            if (sngHeight > SystemInformation.VirtualScreen.Height) {
                sngHeight = SystemInformation.VirtualScreen.Height;
            }
            if (sngLeft < 0) {
                sngLeft = 0;
            }
            if (sngLeft > SystemInformation.VirtualScreen.Width * 0.85) {
                sngLeft = SystemInformation.VirtualScreen.Width * 0.85;
            }
            if (sngTop < 0) {
                sngTop = 0;
            }
            if (sngTop > SystemInformation.VirtualScreen.Height * 0.85) {
                sngTop = SystemInformation.VirtualScreen.Height * 0.85;
            }
            MDIMain.Bounds = new Rectangle((int)sngLeft, (int)sngTop, (int)sngWidth, (int)sngHeight);
            if (blnMax) {
                WindowState = FormWindowState.Maximized;
            }

            // RESOURCE PANE AND PROPERTY WINDOW SPLIT
            sngProp = WinAGISettingsFile.GetSetting(sPOSITION, "ResourceWidth", 125 * 1.5);
            if (sngProp < 125) {
                sngProp = 125;
            }
            else if (sngProp > MDIMain.Bounds.Width - 125) {
                sngProp = MDIMain.Bounds.Width - 125;
            }
            pnlResources.Width = (int)sngProp;
            PropPanelSplit = WinAGISettingsFile.GetSetting(sPOSITION, "PropPanelSplit", 4);
            if (PropPanelSplit < splResource.Panel2MinSize) {
                PropPanelSplit = splResource.Panel2MinSize;
            }
            if (PropPanelSplit > PropPanelMaxSize) {
                PropPanelSplit = PropPanelMaxSize;
            }
            // set initial position of property panel
            splResource.SplitterDistance = splResource.Height - splResource.Margin.Top - splResource.Margin.Bottom - splResource.SplitterWidth - PropPanelSplit;

            // MRU ENTRIES
            for (i = 0; i < 4; i++) {
                strMRU[i] = WinAGISettingsFile.GetSetting(sMRULIST, "MRUGame" + (i + 1), "");
                if (strMRU[i].Length != 0) {
                    mnuGame.DropDownItems["mnuGMRU" + i].Text = CompactPath(strMRU[i], 60);
                    mnuGame.DropDownItems["mnuGMRU" + i].Visible = true;
                    mnuGMRUBar.Visible = true;
                }
                else {
                    // stop loading list at first blank
                    break;
                }
            }

            // TOOLS SETTINGS
            bool blnTools = false;
            for (i = 1; i <= 6; i++) {
                string strCaption = WinAGISettingsFile.GetSetting(sTOOLS, "Caption" + i, "");
                string strTool = WinAGISettingsFile.GetSetting(sTOOLS, "Source" + i, "");
                if (strCaption.Length > 0 && strTool.Length > 0) {
                    mnuTools.DropDownItems["mnuTCustom" + i].Visible = true;
                    mnuTools.DropDownItems["mnuTCustom" + i].Text = strCaption;
                    mnuTools.DropDownItems["mnuTCustom" + i].Tag = strTool;
                    blnTools = true;
                }
            }
            mnuTSep2.Visible = blnTools;

            // DEFAULT RESERVED DEFINES
            DefaultReservedDefines = new(WinAGISettingsFile);

            // IGNORED COMPILER WARNINGS
            lngNoCompVal = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn0", 0);
            for (i = 1; i <= 30; i++) {
                LogicCompiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << i)) == (1 << i));
            }
            lngNoCompVal = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn1", 0);
            for (i = 31; i <= 60; i++) {
                LogicCompiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << (i - 30))) == 1 << (i - 30));
            }
            lngNoCompVal = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn2", 0);
            for (i = 61; i <= 90; i++) {
                LogicCompiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << (i - 60))) == 1 << (i - 60));
            }
            lngNoCompVal = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn3", 0);
            for (i = 91; i < LogicCompiler.WARNCOUNT; i++) {
                LogicCompiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << (i - 90))) == 1 << (i - 90));
            }
            return true;
        }

        public void SaveSettings() {
            // update the non-settings items to the settings list,
            // then save it to file
            int i, lngCompVal;

            // WINDOW POSITION
            if (MDIMain.WindowState == FormWindowState.Maximized) {
                // save Max Value only
                WinAGISettingsFile.WriteSetting(sPOSITION, "WindowMax", true);
            }
            else {
                // save all window settings
                WinAGISettingsFile.WriteSetting(sPOSITION, "Top", MDIMain.Top.ToString());
                WinAGISettingsFile.WriteSetting(sPOSITION, "Left", MDIMain.Left.ToString());
                WinAGISettingsFile.WriteSetting(sPOSITION, "Width", MDIMain.Width.ToString());
                WinAGISettingsFile.WriteSetting(sPOSITION, "Height", MDIMain.Height.ToString());
                WinAGISettingsFile.WriteSetting(sPOSITION, "WindowMax", false.ToString());
            }
            WinAGISettingsFile.WriteSetting(sPOSITION, "PropPanelSplit", PropPanelSplit);

            // RESOURCE PANE AND PROPERTY WINDOW SPLIT
            WinAGISettingsFile.WriteSetting(sPOSITION, "ResourceWidth", pnlResources.Width);
            PropPanelSplit = splResource.Height - splResource.Margin.Top - splResource.Margin.Bottom - splResource.SplitterWidth - splResource.SplitterDistance;
            WinAGISettingsFile.WriteSetting(sPOSITION, "PropPanelSplit", PropPanelSplit);

            // MRU ENTRIES
            for (i = 0; i < 4; i++) {
                WinAGISettingsFile.WriteSetting(sMRULIST, "MRUGame" + (i + 1), strMRU[i]);
            }

            // TOOLS SETTINGS
            // updated WHEN changes made by the edit tools form

            // RESERVED DEFINES OVERRIDES
            // updated when changes made by the edit reserved defines form
            // 

            // IGNORED COMPILER WARNINGS
            lngCompVal = 0;
            for (i = 1; i <= 30; i++) {
                lngCompVal |= (LogicCompiler.IgnoreWarning(5000 + i) ? 1 << i : 0);
            }
            WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn0", lngCompVal);
            lngCompVal = 0;
            for (i = 1; i <= 30; i++) {
                lngCompVal |= (LogicCompiler.IgnoreWarning(5030 + i) ? 1 << i : 0);
            }
            WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn1", lngCompVal);
            lngCompVal = 0;
            for (i = 1; i <= 30; i++) {
                lngCompVal |= (LogicCompiler.IgnoreWarning(5060 + i) ? 1 << i : 0);
            }
            WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn2", lngCompVal);
            lngCompVal = 0;
            for (i = 1; i < (LogicCompiler.WARNCOUNT % 30); i++) {
                lngCompVal |= (LogicCompiler.IgnoreWarning(5090 + i) ? 1 << i : 0);
            }
            WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn3", lngCompVal);

            // make sure header is present
            if (WinAGISettingsFile.Lines[1] != "# WinAGI GDS Configuration File") {
                // delete any comment lines at beginning, then re-add the correct header
                do {
                    if (WinAGISettingsFile.Lines[0].Trim().Length == 0 || WinAGISettingsFile.Lines[0].Trim()[0] == '#') {
                        WinAGISettingsFile.Lines.RemoveAt(0);
                    }
                    else {
                        break;
                    }
                } while (true);
                WinAGISettingsFile.Lines.InsertRange(0, [
                    "#",
                    "# WinAGI GDS Configuration File",
                    "#",
                    "# These settings should normally be adjusted from within WinAGI",
                    "#"
                ]);
            }
            // now save all settings to file
            WinAGISettingsFile.Save();
        }

        void CheckCmd() {
            string[] args = Environment.GetCommandLineArgs();
            // arg[0] is the program name
            // arg[1] is the target file

            // only first arg is used; extras are ignored
            if (args.Length != 2) {
                return;
            }

            // ensure no quotes
            if (args[1][0] == '"') {
                args[1] = args[1].Mid(2, args[1].Length - 2);
            }
            // check for OBJECT or WORDS.TOK file:
            if (Path.GetFileName(args[1]).Equals("OBJECT", StringComparison.OrdinalIgnoreCase)) {
                // open a object resource
                NewInvObjList(args[1]);
            }
            else if (Path.GetFileName(args[1]).Equals("WORDS.TOK", StringComparison.OrdinalIgnoreCase)) {
                // open a word resource
                NewWordList(args[1]);
            }
            else if (args[1].Right(4).ToLower() == "." + WinAGISettings.DefaultExt.Value) {
                // open a logic source text file or logic resource
                NewLogic(args[1]);
            }
            else {
                // check for a file
                // (first check for logic source (it is variable so can't be
                // used in a switch statement)
                if (args[1].Right(4).ToLower() == "." + WinAGISettings.DefaultExt.Value) {
                    // open a logic source text file
                    NewLogic(args[1]);
                    return;
                }
                switch (args[1].Right(4).ToLower()) {
                case ".wag":
                    //open a game
                    OpenWAGFile(args[1]);
                    break;
                case ".wal":
                    //layout files can't be opened by command line anymore
                    //////      //open a game; then open layout editor
                    break;
                case ".agl":
                    // open a logic resource
                    NewLogic(args[1]);
                    break;
                case ".agp":
                    // open a picture resource
                    NewPicture(args[1]);
                    break;
                case ".ags":
                    // open a sound resource
                    NewSound(args[1]);
                    break;
                case ".agv":
                    // open a view resource
                    NewView(args[1]);
                    break;
                default:
                    // check for 'OBJECT' and 'WORDS.TOK'
                    if (Path.GetFileName(args[1]).Equals("OBJECT", StringComparison.CurrentCultureIgnoreCase)) {
                        NewInvObjList(args[1]);
                    }
                    else if (Path.GetFileName(args[1]).Equals("WORDS.TOK", StringComparison.CurrentCultureIgnoreCase)) {
                        NewWordList(args[1]);
                    }
                    else {
                        // ignore anything else
                        MessageBox.Show($"'{Path.GetFileName(args[1])}' is not a valid WinAGI resource filename.");
                    }
                    break;
                }
            }
        }

        public void SearchForID() {
            switch (SelResType) {
            case AGIResType.Logic:
                GFindText = EditGame.Logics[(byte)SelResNum].ID;
                break;
            case AGIResType.Picture:
                GFindText = EditGame.Pictures[(byte)SelResNum].ID;
                break;
            case AGIResType.Sound:
                GFindText = EditGame.Sounds[(byte)SelResNum].ID;
                break;
            case AGIResType.View:
                GFindText = EditGame.Views[(byte)SelResNum].ID;
                break;
            }
            GFindDir = FindDirection.All;
            GMatchWord = true;
            GMatchCase = true;
            GLogFindLoc = FindLocation.All;
            GFindSynonym = false;

            //reset search flags
            FindingForm.ResetSearch();

            //display find form
            FindingForm.SetForm(FindFormFunction.FindLogic, true);
            FindingForm.Visible = true;

            // decided to stick with just showing the form, instead of
            // automatically starting a search

            //////  FindInLogic GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc
        }

        public void AddWarning(TWinAGIEventInfo warnInfo) {
            // six types of warnings/errors get added to list
            // - resource errors: ErrLevel > 0
            // - resource warnings: ErrLevel < 0
            // - logic compile errors
            // - logic compile warnings
            // - logic decompile warnings
            // - TODO entries

            Debug.Assert(warnInfo.Type != Info);
            // convert event info to grid info
            WarningGridInfo gridinfo = new WarningGridInfo() {
                Type = warnInfo.Type.ToString(),
                ResType = warnInfo.ResType.ToString(),
                Code = warnInfo.ID,
                Description = warnInfo.Text,
                ResNum = (int)warnInfo.ResType < 4 ? warnInfo.ResNum.ToString() : "--",
                Line = warnInfo.Line,
                Module = warnInfo.Module,
                Filename = warnInfo.Filename
            };
            WarningList.Add(gridinfo);
            AddWarningToGrid(gridinfo);
            if (!MDIMain.pnlWarnings.Visible) {
                if (WinAGISettings.AutoWarn.Value) {
                    ShowWarningList();
                }
            }
            // check for gameid truncation
        }

        public void AddWarningToGrid(WarningGridInfo warnInfo, bool showit = true) {
            int tmpRow = fgWarnings.Rows.Add(warnInfo.Type,
                         warnInfo.ResType,
                         warnInfo.Code,
                         warnInfo.Description,
                         warnInfo.ResNum,
                         warnInfo.Line,
                         warnInfo.Module,
                         warnInfo.Filename);
            //switch (warnInfo.Type) {
            //case nameof(LogicCompileError):
            //case nameof(ResourceError):
            //    // bold, red
            //    fgWarnings.Rows[tmpRow].DefaultCellStyle.Font = new Font(fgWarnings.Font, FontStyle.Bold);
            //    fgWarnings.Rows[tmpRow].DefaultCellStyle.ForeColor = Color.Red;
            //    break;
            //case nameof(TODO):
            //    // bold, italic
            //    fgWarnings.Rows[tmpRow].DefaultCellStyle.Font = new Font(fgWarnings.Font, FontStyle.Bold | FontStyle.Italic);
            //    break;
            //case nameof(LogicCompileWarning):
            //case nameof(ResourceWarning):
            //case nameof(DecompWarning):
            //    break;
            //}
            //// always make it visible
            //if (showit && !fgWarnings.Rows[tmpRow].Displayed) {
            //    fgWarnings.CurrentCell = fgWarnings.Rows[tmpRow].Cells[0];
            //}
        }
        public void HideWarningList(bool clearlist = false) {
            if (clearlist) {
                ClearWarnings();
            }
            MDIMain.pnlWarnings.Visible = false;
            MDIMain.splitWarning.Visible = false;
        }

        public void ShowWarningList() {
            splitWarning.Visible = true;
            pnlWarnings.Visible = true;
        }

        /// <summary>
        /// Clears entire warning/error panel.
        /// </summary>
        public void ClearWarnings() {
            WarningList.Clear();
            fgWarnings.Rows.Clear();
        }

        public void DismissWarning(int row) {
            fgWarnings.Rows.RemoveAt(row);
            for (int i = 0; i < WarningList.Count; i++) {
                if ((string)fgWarnings.Rows[row].Cells[0].Value == WarningList[i].Type) {
                    if ((string)fgWarnings.Rows[row].Cells[0].Value == WarningList[i].ResType) {
                        if ((string)fgWarnings.Rows[row].Cells[2].Value == WarningList[i].Code) {
                            if ((string)fgWarnings.Rows[row].Cells[3].Value == WarningList[i].Description) {
                                if ((string)fgWarnings.Rows[row].Cells[4].Value == WarningList[i].ResNum) {
                                    if ((string)fgWarnings.Rows[row].Cells[5].Value == WarningList[i].Line) {
                                        if ((string)fgWarnings.Rows[row].Cells[6].Value == WarningList[i].Module) {
                                            WarningList.RemoveAt(i);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void DismissWarnings() {
            // dismiss all  LogicCompileWarning and DecompWarning
            // keep LogicCompileError, ResourceWarning, ResourceError, TODO

            for (int i = WarningList.Count - 1; i >= 0; i--) {
                if (WarningList[i].Type == nameof(LogicCompileWarning) || WarningList[i].Type == nameof(DecompWarning)) {
                    WarningList.RemoveAt(i);
                }
                string evtype = (string)fgWarnings.Rows[i].Cells[0].Value;
                if (evtype == nameof(LogicCompileWarning) || evtype == nameof(DecompWarning)) {
                    fgWarnings.Rows.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Clears warning/error entries for the resource specified by type, number and event type.
        /// </summary>
        /// <param name="resnum"></param>
        /// <param name="restype"></param>
        public void ClearWarnings(AGIResType restype, byte resnum, EventType eventtype) {
            ClearWarnings(restype, resnum, [eventtype]);
        }

        /// <summary>
        /// Clears warning/error entries for the resource specified by type, number that 
        /// match one of the specified event types.
        /// </summary>
        /// <param name="restype"></param>
        /// <param name="resnum"></param>
        /// <param name="eventtypes"></param>
        public void ClearWarnings(AGIResType restype, byte resnum, EventType[] eventtypes) {
            string s_resnum;
            if (restype <= AGIResType.View) {
                s_resnum = resnum.ToString();
            }
            else {
                s_resnum = "--";
            }
            //find the matching lines (by restype/number/eventtype)
            for (int i = WarningList.Count - 1; i >= 0; i--) {
                for (int j = 0; j < eventtypes.Length; j++) {
                    if (i < WarningList.Count && WarningList[i].Type == eventtypes[j].ToString() && WarningList[i].ResType == restype.ToString() && WarningList[i].ResNum == s_resnum) {
                        WarningList.RemoveAt(i);
                    }
                    if (i < fgWarnings.Rows.Count && (string)fgWarnings.Rows[i].Cells[0].Value == eventtypes[j].ToString() && (string)fgWarnings.Rows[i].Cells[1].Value == restype.ToString() && (string)fgWarnings.Rows[i].Cells[4].Value == s_resnum) {
                        fgWarnings.Rows.RemoveAt(i);
                    }
                }
            }
            Debug.Assert(WarningList.Count == fgWarnings.Rows.Count);
        }

        /// <summary>
        /// Clears all warning/error entries for the resource specified by type and number.
        /// </summary>
        /// <param name="resnum"></param>
        /// <param name="restype"></param>
        public void ClearWarnings(AGIResType restype, byte resnum) {
            // find the matching lines (by restype/number)
            string s_resnum;
            if (restype <= AGIResType.View) {
                s_resnum = resnum.ToString();
            }
            else {
                s_resnum = "--";
            }
            for (int i = WarningList.Count - 1; i >= 0; i--) {
                if (WarningList[i].ResType == restype.ToString() && WarningList[i].ResNum == s_resnum) {
                    WarningList.RemoveAt(i);
                }
                if ((string)fgWarnings.Rows[i].Cells[1].Value == restype.ToString() && (string)fgWarnings.Rows[i].Cells[4].Value == s_resnum) {
                    fgWarnings.Rows.RemoveAt(i);
                }
            }
            Debug.Assert(WarningList.Count == fgWarnings.Rows.Count);
        }

        public void HelpWarning(string type, string id) {
            // show help for the warning (or error) that is selected
            string strTopic = "";

            switch (type) {
            case nameof(LogicCompileError):
                strTopic = @"htm\winagi\compilererrors.htm#" + id;
                break;
            case nameof(ResourceError):
            case nameof(ResourceWarning):
                strTopic = @"htm\winagi\gamewarnings.htm#" + id;
                break;
            case nameof(LogicCompileWarning):
                strTopic = @"htm\winagi\compilerwarnings.htm#" + id;
                break;
            case nameof(DecompWarning):
                strTopic = @"htm\winagi\decompwarnings.htm#" + id;
                break;
            case nameof(TODO):
                strTopic = @"htm\winagi\Logic_Editor.htm#TODO";
                break;
            }
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, strTopic);
        }

        private void IgnoreWarning(int WarningNumber) {
            LogicCompiler.SetIgnoreWarning(WarningNumber, true);
            for (int i = WarningList.Count - 1; i >= 0; i--) {
                if (WarningList[i].Code == WarningNumber.ToString()) {
                    WarningList.RemoveAt(i);
                }
                if ((string)fgWarnings.Rows[i].Cells[2].Value == WarningNumber.ToString()) {
                    fgWarnings.Rows.RemoveAt(i);
                }
            }
            Debug.Assert(WarningList.Count == fgWarnings.Rows.Count);
        }

        public void HighlightLine(int lngErrLine, string strErrMsg, int LogicNumber, string strModule, EventType eventtype) {
            // this procedure uses warning/error/TODO info to open the file
            // with the desired entry
            // it highlights the target line and, if it is a warning or error,
            // it displays the warning/error message in the status bar
            //
            // lngType: 0 = Error, 1 = Warning, 2 = TODO, 3 = decomp
            frmLogicEdit frmTemp = null;

            if (strModule.Length != 0) {
                if (OpenTextFile(strModule, true)) {
                    for (int i = 0; i < LogicEditors.Count; i++) {
                        if (LogicEditors[i].FormMode == LogicFormMode.Text && LogicEditors[i].TextFilename == strModule) {
                            frmTemp = LogicEditors[i];
                            break;
                        }
                    }
                }
                else {
                    if (eventtype != EventType.LogicCompileError) {
                        return;
                    }
                    lngErrLine = 0;
                    strErrMsg += " (in INCLUDE file)";
                }
            }
            else {
                for (int i = 0; i < LogicEditors.Count; i++) {
                    if (LogicEditors[i].FormMode == LogicFormMode.Logic && LogicEditors[i].LogicNumber == LogicNumber) {
                        frmTemp = LogicEditors[i];
                        if (frmTemp.WindowState == FormWindowState.Minimized) {
                            frmTemp.WindowState = FormWindowState.Normal;
                        }
                        frmTemp.BringToFront();
                        frmTemp.Select();
                        break;
                    }
                }
                if (frmTemp == null) {
                    if (OpenGameLogic((byte)LogicNumber, true)) {
                        for (int i = 0; i < LogicEditors.Count; i++) {
                            if (LogicEditors[i].FormMode == LogicFormMode.Logic && LogicEditors[i].LogicNumber == LogicNumber) {
                                frmTemp = LogicEditors[i];
                                if (frmTemp.WindowState == FormWindowState.Minimized) {
                                    frmTemp.WindowState = FormWindowState.Normal;
                                }
                                frmTemp.BringToFront();
                                frmTemp.Select();
                                break;
                            }
                        }
                    }
                }
            }
            // if not opened, just show an error message
            if (frmTemp == null) {
                string msgboxtext = "";
                string msgboxtitle = "";
                switch (eventtype) {
                case EventType.LogicCompileError:
                    // error
                    msgboxtext = "ERROR in line " + lngErrLine + "- " + strErrMsg;
                    msgboxtitle = "Compile Logic Error";
                    break;
                case EventType.LogicCompileWarning:
                    // warning
                    msgboxtext = "WARNING in line " + lngErrLine + ": " + strErrMsg;
                    msgboxtitle = "Compile Logic Warning";
                    break;
                case EventType.TODO:
                    // TODO
                    msgboxtext = "In " + EditGame.Logics[LogicNumber].ID + " at line " + lngErrLine + ":" +
                           "\n\nTODO: " + strErrMsg;
                    msgboxtitle = "TODO Item";
                    break;
                case EventType.DecompWarning:
                    // decomp warning
                    msgboxtext = "Decompiling warning in " + EditGame.Logics[LogicNumber].ID + " at line " + lngErrLine + ":\n\n" +
                           strErrMsg;
                    msgboxtitle = "TODO Item";
                    break;
                }
                MessageBox.Show(MDIMain,
                    msgboxtext, msgboxtitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            if (lngErrLine >= frmTemp.fctb.LinesCount) {
                lngErrLine = frmTemp.fctb.LinesCount - 1;
            }
            frmTemp.fctb.Selection.Start = new(0, lngErrLine);
            frmTemp.fctb.Selection.End = frmTemp.fctb.Selection.Start; // new(frmTemp.fctb.GetLineLength(lngErrLine), lngErrLine);
            frmTemp.fctb.DoSelectionVisible();
            frmTemp.BringToFront();
            frmTemp.Select();
            // if not a TODO, update the status bar as well
            if (eventtype != TODO) {
                string strErrType;
                if (eventtype == EventType.LogicCompileError) {
                    strErrType = "ERROR ";
                }
                else {
                    strErrType = "WARNING ";
                }
                MainStatusBar.Items[nameof(spStatus)].Text = strErrType + "in line " + lngErrLine + ": " + strErrMsg;
            }
        }

        public void ShowProperties() {
            ShowProperties(false, "General", "");
        }

        public void ShowProperties(bool EnableOK, string StartTab = "", string StartProp = "") {
            // show properties form
            frmGameProperties propForm = new(GameSettingFunction.Edit, StartTab, StartProp);
            propForm.btnOK.Enabled = EnableOK;
            if (propForm.ShowDialog(MDIMain) == DialogResult.Cancel) {
                // exit withoutsaving anything
                propForm.Dispose();
                return;
            }
            EditGame.GameAuthor = propForm.txtGameAuthor.Text;
            EditGame.GameDescription = propForm.txtGameDescription.Text;
            EditGame.GameAbout = propForm.txtGameAbout.Text;
            // if no directory, force platform to nothing
            if (propForm.NewPlatformFile.Length == 0) {
                EditGame.PlatformType = Engine.PlatformType.None;
            }
            else {
                // platform
                if (propForm.optDosBox.Checked) {
                    EditGame.PlatformType = Engine.PlatformType.DosBox;
                }
                else if (propForm.optScummVM.Checked) {
                    EditGame.PlatformType = Engine.PlatformType.ScummVM;
                }
                else if (propForm.optNAGI.Checked) {
                    EditGame.PlatformType = Engine.PlatformType.NAGI;
                }
                else if (propForm.optOther.Checked) {
                    EditGame.PlatformType = Engine.PlatformType.Other;
                }
            }

            // platformdir OK as long as not nuthin
            if (EditGame.PlatformType > 0) {
                EditGame.Platform = propForm.NewPlatformFile;
                // platform options OK if dosbox or scummvm
                if (EditGame.PlatformType == Engine.PlatformType.DosBox ||
                              EditGame.PlatformType == Engine.PlatformType.ScummVM ||
                              EditGame.PlatformType == Engine.PlatformType.Other) {
                    EditGame.PlatformOpts = propForm.txtOptions.Text;
                }
                else {
                    EditGame.PlatformOpts = "";
                }
                // dos executable only used if dosbox
                if (EditGame.PlatformType == Engine.PlatformType.DosBox) {
                    EditGame.DOSExec = propForm.txtExec.Text;
                }
                else {
                    EditGame.DOSExec = "";
                }
            }
            else {
                EditGame.Platform = "";
                EditGame.PlatformOpts = "";
                EditGame.DOSExec = "";
            }
            EditGame.GameVersion = propForm.txtGameVersion.Text;
            // int version (if changed)
            if (EditGame.InterpreterVersion != propForm.cmbVersion.Text) {
                ChangeIntVersion(propForm.cmbVersion.Text);
            }
            if (EditGame.GameID != propForm.txtGameID.Text) {
                Debug.Assert(propForm.txtGameID.Text != "");
                ChangeGameID(propForm.txtGameID.Text);
            }
            if (!EditGame.ResDirName.Equals(propForm.txtResDir.Text, StringComparison.CurrentCultureIgnoreCase)) {
                ChangeResDir(propForm.txtResDir.Text);
            }
            if (EditGame.SourceExt != propForm.txtSrcExt.Text) {
                EditGame.SourceExt = propForm.txtSrcExt.Text.ToLower();
                foreach (Logic aLogic in EditGame.Logics) {
                    SafeFileMove(aLogic.SourceFile, EditGame.ResDir + Path.GetFileNameWithoutExtension(aLogic.SourceFile) + "." + EditGame.SourceExt, true);
                }
            }
            EditGame.IncludeIDs = propForm.chkResourceIDs.Checked;
            EditGame.IncludeReserved = propForm.chkResDefs.Checked;
            EditGame.IncludeGlobals = propForm.chkGlobals.Checked;
            EditGame.UseLE = propForm.chkUseLE.Checked;
            // update menu/toolbar, and hide LE if not in use anymore
            UpdateLEStatus();
            EditGame.CodePage = Encoding.GetEncoding(propForm.NewCodePage);
            WinAGISettingsFile.Save();
            propForm.Dispose();
        }

        private void RunGame() {
            string strParams = "";
            string strErrTitle = "", strErrMsg = "", strErrType = "";
            bool failed = false;

            // first check for missing platform
            if (EditGame.PlatformType == 0) {
                // notify user and show property dialog
                MessageBox.Show(MDIMain,
                    "You need to select a platform on which to run your game first.",
                    "No Platform Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    0, 0,
                    WinAGIHelp,
                    "htm\\winagi\\Properties.htm#platform");
                ShowProperties(false, "Platform");
                // if still no platform
                if (EditGame.PlatformType == 0) {
                    // just exit
                    return;
                }
            }
            switch (EditGame.PlatformType) {
            case Engine.PlatformType.DosBox:
                // DosBox - verify target exists
                if (!File.Exists(EditGame.GameDir + EditGame.DOSExec)) {
                    MessageBox.Show(MDIMain,
                        "The DOS executable file '" + EditGame.DOSExec + "' is missing from the " +
                        "game directory. Aborting DosBox session.",
                        "Missing DOS Executable File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        0, 0,
                        WinAGIHelp,
                        "htm\\winagi\\Properties.htm#platform");
                    return;
                }
                // dosbox parameters: gamedir+agi.exe -noautoexec
                //  (need -noautoexec option as mandatory setting to avoid virtual C-drive assignment issues)
                strParams = '"' + EditGame.GameDir + EditGame.DOSExec + '"' + " -noautoexec " + EditGame.PlatformOpts;
                break;
            case Engine.PlatformType.ScummVM:
                // scummvm parameters: --p gamedir --auto-detect
                strParams = "-p \"" + Path.GetDirectoryName(EditGame.GameDir) + "\" --auto-detect " + EditGame.PlatformOpts;
                break;
            case Engine.PlatformType.NAGI:
                // no parameters for nagi; just run the program
                break;
            case Engine.PlatformType.Other:
                // run with whatever is in Platform and PlatformOpts
                strParams = EditGame.PlatformOpts;
                break;
            }
            if (CheckLogics()) {
                // run the program if check is OK
                Process runagi = new Process();
                ProcessStartInfo runparams = new();
                runparams.FileName = EditGame.Platform;
                runparams.Arguments = strParams;
                runparams.ErrorDialog = true;
                runparams.ErrorDialogParentHandle = this.Handle;
                //runparams.UseShellExecute = true;
                runparams.WindowStyle = ProcessWindowStyle.Normal;
                runparams.WorkingDirectory = EditGame.GameDir;
                runagi.StartInfo = runparams;
                try {
                    failed = !runagi.Start();
                    runagi.Dispose();
                    if (failed) {
                        strErrType = "(process failed to run)";
                    }
                }
                catch (Exception ex) {
                    failed = true;
                    strErrType = "(" + ex.Message + ")";
                }

                if (failed) {
                    switch (EditGame.PlatformType) {
                    case Engine.PlatformType.DosBox:
                        strErrTitle = "DosBox Error";
                        strErrMsg = "DosBox ";
                        break;
                    case Engine.PlatformType.ScummVM:
                        strErrTitle = "ScummVM Error";
                        strErrMsg = "ScummVM ";
                        break;
                    case Engine.PlatformType.NAGI:
                        strErrTitle = "NAGI Error";
                        strErrMsg = "NAGI ";
                        break;
                    case Engine.PlatformType.Other:
                        strErrTitle = "Run AGI Game Error";
                        strErrMsg = "this program ";
                        break;
                    }
                    strErrMsg = "Unable to run " + strErrTitle + strErrType + ". Make sure you " +
                                "have selected the correct executable file, and that any parameters " +
                                "you included are correct.";
                    MessageBox.Show(MDIMain,
                        strErrMsg,
                        strErrTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        0, 0,
                        WinAGIHelp,
                        "htm\\winagi\\Properties.htm#platform");
                }
            }
        }

        internal void RefreshPropertyGrid() {
            RefreshPropertyGrid(SelResType, SelResNum);
        }

        internal void RefreshPropertyGrid(AGIResType restype, int resnum) {
            // re-select the current property object

            if (WinAGISettings.ResListType.Value == EResListType.None) {
                return;
            }

            //// this does not work as I'd hoped
            //foreach (Control c in propertyGrid1.Controls) {
            //    c.MouseDoubleClick -= propertyGrid1_MouseDoubleClick;
            //}

            propertyGrid1.SelectedObject = null;
            switch (restype) {
            case None:
                return;
            case Game:
                //// show gameid, gameauthor, description,etc
                GameProperties pGame = new();
                propertyGrid1.SelectedObject = pGame;

                // TODO: this works for dblclicks on the property name but not
                // on the value; can't find any way to make it work...
                //// try catching doubleclick events
                //foreach (Control c in propertyGrid1.Controls) {
                //    c.MouseDoubleClick += propertyGrid1_MouseDoubleClick;
                //    //c.Controls[1].MouseDoubleClick += propertyGrid1_MouseDoubleClick;
                //}

                break;
            case AGIResType.Logic:
                if (resnum == -1) {
                    // logic header
                    LogicHdrProperties pLgcHdr = new();
                    propertyGrid1.SelectedObject = pLgcHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Logics[resnum].Load();
                    LogicProperties pLog = new(EditGame.Logics[resnum]);
                    // get reference to value field of the ReadOnly attribute for the IsRoom property 
                    Attribute readOnly = TypeDescriptor.GetProperties(pLog.GetType())["IsRoom"].Attributes[typeof(ReadOnlyAttribute)];
                    FieldInfo fi = readOnly.GetType().GetRuntimeFields().ToArray()[0];
                    // IsRoom is ReadOnly for logic0, ReadWrite for all others
                    if (resnum == 0) {
                        fi?.SetValue(readOnly, true);
                    }
                    else {
                        fi?.SetValue(readOnly, false);
                    }
                    propertyGrid1.SelectedObject = pLog;
                }
                break;
            case AGIResType.Picture:
                if (resnum == -1) {
                    //picture header
                    PictureHdrProperties pPicHdr = new();
                    propertyGrid1.SelectedObject = pPicHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Pictures[resnum].Load();
                    PictureProperties pPicture = new(EditGame.Pictures[resnum]);
                    propertyGrid1.SelectedObject = pPicture;
                }
                break;
            case AGIResType.Sound:
                if (resnum == -1) {
                    // sound header
                    SoundHdrProperties pSndHdr = new();
                    propertyGrid1.SelectedObject = pSndHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Sounds[resnum].Load();
                    SoundProperties pSound = new(EditGame.Sounds[resnum]);
                    propertyGrid1.SelectedObject = pSound;
                }
                break;
            case AGIResType.View:
                if (resnum == -1) {
                    //view header
                    ViewHdrProperties pViewHdr = new();
                    propertyGrid1.SelectedObject = pViewHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Views[resnum].Load();
                    ViewProperties pView = new(EditGame.Views[resnum]);
                    propertyGrid1.SelectedObject = pView;
                }
                break;
            case Objects:
                // OBJECT file is always loaded
                InvObjProperties pInvObj = new(EditGame.InvObjects);
                propertyGrid1.SelectedObject = pInvObj;
                break;
            case Words:
                // WORDS.TOK is always loaded
                WordListProperties pWordList = new(EditGame.WordList);
                propertyGrid1.SelectedObject = pWordList;
                break;
            }
        }
    }

    public class WarningGridInfo {
        public string Type { get; set; } //0
        public string ResType { get; set; } //1
        public string Code { get; set; } //2
        public string Description { get; set; } //3
        public string ResNum { get; set; } //4
        public string Line { get; set; } //5
        public string Module { get; set; } //6
        public string Filename { get; set; } //7
    }

    //public class SortableBindingList<T> : BindingList<T> {
    //    List<T> originalList;
    //    ListSortDirection sortDirection;
    //    PropertyDescriptor sortProperty;

    //    // function that refereshes the contents of the base classes collection of elements
    //    Action<SortableBindingList<T>, List<T>> populateBaseList = (a, b) => a.ResetItems(b);

    //    static Dictionary<string, Func<List<T>, IEnumerable<T>>> cachedOrderByExpressions = new Dictionary<string, Func<List<T>, IEnumerable<T>>>();

    //    public SortableBindingList() {
    //        originalList = new List<T>();
    //    }

    //    public SortableBindingList(IEnumerable<T> enumerable) {
    //        originalList = enumerable.ToList();
    //        populateBaseList(this, originalList);
    //    }

    //    public SortableBindingList(List<T> list) {
    //        originalList = list;
    //        populateBaseList(this, originalList);
    //    }

    //    protected override void ApplySortCore(PropertyDescriptor prop,
    //                            ListSortDirection direction) {
    //        /*
    //         Look for an appropriate sort method in the cache if not found .
    //         Call CreateOrderByMethod to create one. 
    //         Apply it to the original list.
    //         Notify any bound controls that the sort has been applied.
    //         */
    //        sortProperty = prop;

    //        var orderByMethodName = sortDirection ==
    //            ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending";
    //        var cacheKey = typeof(T).GUID + prop.Name + orderByMethodName;

    //        if (!cachedOrderByExpressions.ContainsKey(cacheKey)) {
    //            CreateOrderByMethod(prop, orderByMethodName, cacheKey);
    //        }

    //        ResetItems(cachedOrderByExpressions[cacheKey](originalList).ToList());
    //        ResetBindings();
    //        sortDirection = sortDirection == ListSortDirection.Ascending ?
    //                        ListSortDirection.Descending : ListSortDirection.Ascending;
    //    }

    //    private void CreateOrderByMethod(PropertyDescriptor prop,
    //                 string orderByMethodName, string cacheKey) {
    //        /*
    //         Create a generic method implementation for IEnumerable<T>.
    //         Cache it.
    //        */

    //        var sourceParameter = Expression.Parameter(typeof(List<T>), "source");
    //        var lambdaParameter = Expression.Parameter(typeof(T), "lambdaParameter");
    //        var accesedMember = typeof(T).GetProperty(prop.Name);
    //        var propertySelectorLambda =
    //            Expression.Lambda(Expression.MakeMemberAccess(lambdaParameter,
    //                              accesedMember), lambdaParameter);
    //        var orderByMethod = typeof(Enumerable).GetMethods()
    //                                      .Where(a => a.Name == orderByMethodName &&
    //                                                   a.GetParameters().Length == 2)
    //                                      .Single()
    //                                      .MakeGenericMethod(typeof(T), prop.PropertyType);

    //        var orderByExpression = Expression.Lambda<Func<List<T>, IEnumerable<T>>>(
    //                                    Expression.Call(orderByMethod,
    //                                            new Expression[] { sourceParameter,
    //                                                           propertySelectorLambda }),
    //                                            sourceParameter);

    //        cachedOrderByExpressions.Add(cacheKey, orderByExpression.Compile());
    //    }

    //    protected override void RemoveSortCore() {
    //        ResetItems(originalList);
    //    }

    //    private void ResetItems(List<T> items) {
    //        base.ClearItems();
    //        for (int i = 0; i < items.Count; i++) {
    //            base.InsertItem(i, items[i]);
    //        }
    //    }

    //    protected override bool SupportsSortingCore {
    //        get {
    //            return true;
    //        }
    //    }

    //    protected override ListSortDirection SortDirectionCore {
    //        get {
    //            return sortDirection;
    //        }
    //    }

    //    protected override PropertyDescriptor SortPropertyCore {
    //        get {
    //            return sortProperty;
    //        }
    //    }

    //    protected override void OnListChanged(ListChangedEventArgs e) {
    //        originalList = base.Items.ToList();
    //    }
    //}
}
