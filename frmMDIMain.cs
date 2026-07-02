using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Common.BkgdTasks;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.EventType;

namespace WinAGI.Editor {
    public partial class frmMDIMain : Form {
        #region Fields
        // warning grid variables
        private readonly List<int> WarnGridSortOrder = [2, 3, 4, 5, 6];
        internal InfoGridScope infoGridScope = InfoGridScope.EntireProject;
        private bool gridWarnings = true;
        private bool gridErrors = true;
        private bool gridTODOs = true;

        // navigation list variables
        private int NLOffset, NLRow, NLRowHeight;
        public string LastNodeName;

        // property box height
        private int PropPanelSplit;
        private readonly int PropPanelMaxSize;
        private bool splashDone;
        // tracks status of caps/num/ins
        private static bool CapsLock = false;
        private static bool NumLock = false;
        internal static bool Overwrite = false;
        // required to prevent MDI form from overriding help requests when
        // MessageBox help is requested
        private bool ShowingMsgBox = false;
        internal Timer statusFlashTimer = new();
        internal int StatusFlashCount = 0;
        #endregion

        #region Constructors
        public frmMDIMain() {
            InitializeComponent();
            // what is resolution?
            Debug.Print($"DeviceDPI: {DeviceDpi}");
            Debug.Print($"AutoScaleFactor: {AutoScaleFactor}");
            Debug.Print($"AutoscaleDimensions: {AutoScaleDimensions}");

            // use idle time to update caps/num/ins
            Application.Idle += new EventHandler(OnIdle);
            CapsLock = Console.CapsLock;
            spCapsLock.Text = CapsLock ? "CAP" : "\t";
            NumLock = Console.NumberLock;
            spNumLock.Text = NumLock ? "NUM" : "\t";
            Overwrite = IsKeyLocked(Keys.Insert);
            spInsLock.Text = Overwrite ? "OVR" : "INS";

            // save pointer to main form
            MDIMain = this;

            // setup info grid
            infoGridTable.Columns.Add("Type", typeof(string)); // EventType, hidden
            infoGridTable.Columns.Add("ResType", typeof(string));  // AGIResType, hidden
            infoGridTable.Columns.Add("Code", typeof(string));
            infoGridTable.Columns.Add("Description", typeof(string));
            infoGridTable.Columns.Add("ResNum", typeof(int));
            infoGridTable.Columns.Add("Line", typeof(int));
            infoGridTable.Columns.Add("Module", typeof(string));
            infoGridTable.Columns.Add("Filename", typeof(string));
            infoGridBinding.DataSource = infoGridTable;
            // configure the warning grid
            SetupInfoGrid();
            gridFilter.SelectedIndex = 0;

            btnNewRes.DefaultItem = btnNewLogic;
            btnOpenRes.DefaultItem = btnOpenLogic;
            btnImportRes.DefaultItem = btnImportLogic;
            MainStatusBar = statusStrip1;
            statusFlashTimer.Interval = 100;
            statusFlashTimer.Tick += StatusFlashTimer_Tick;
            FindingForm = new frmFind();

            // set property window split location based on longest word
            Size szText = TextRenderer.MeasureText(" Use Res Names ", propertyGrid1.Font);
            // set height based on text (assume padding of 3 pixels above/below)
            int propRowHeight = szText.Height + 6;
            splResource.Panel2MinSize = 3 * propRowHeight;
            PropPanelMaxSize = 10 * propRowHeight;
            // set grid row height
            fgWarnings.RowTemplate.Height = szText.Height + 2;
            // initialize the basic app functionality
            InitializeResMan();

            ProgramDir = Path.GetDirectoryName(Application.ExecutablePath);
            DefaultResDir = ProgramDir;
            // set browser start dir to program dir
            BrowserStartDir = ProgramDir;

            LogicEditors = [];
            ViewEditors = [];
            PictureEditors = [];
            SoundEditors = [];
        }
        #endregion

        #region Event Handlers
        #region Form Events
        private void frmMDIMain_Load(object sender, EventArgs e) {
            bool lastLoad;

            // hide resource and warning panels until needed
            pnlResources.Visible = false;
            pnlInfoGrid.Visible = false;

            // get game settings and set initial window positions
            if (!ReadSettings()) {
                // problem with settings
                MessageBox.Show("Fatal error: Unable to read program settings", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
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
            // if using snippets
            if (WinAGISettings.UseSnippets.Value) {
                // build snippet table
                BuildSnippets();
            }

            WinAGIHelp = Path.Combine(ProgramDir, "WinAGI.chm");

            // initialize resource treelist by using clear method
            ClearResourceList();

            // set navlist parameters
            // set property window split location based on longest word
            Size szText = TextRenderer.MeasureText(" Logic 1 ", new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value));
            NLRowHeight = szText.Height + 2;
            picNavList.Height = NLRowHeight * 5;
            picNavList.Top = (cmdBack.Top + cmdBack.Height / 2) - picNavList.Height / 2;

            // retrieve user's preferred AGI colors
            GetDefaultColors();
            RefreshSyntaxStyles();

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
            CheckCommandLine();

            // was a game loaded when app was last closed
            lastLoad = WinAGISettingsFile.GetSetting(sMRULIST, "LastLoad", false);

            // if nothing loaded AND autoreload is set AND something was loaded
            // last time program ended,
            if (EditGame is null && ActiveMdiChild is null && WinAGISettings.AutoOpen.Value && lastLoad) {
                // open mru0
                OpenMRUGame(0);
            }
        }

        private void frmMDIMain_KeyDown(object sender, KeyEventArgs e) {
            // check for grid keypresses first
            // (this is only way I know to catch keypresses in the property grid...)
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
                case "Number":
                case "ID":
                case "Description":
                case "ViewDesc":
                    // never allow direct editing
                    e.SuppressKeyPress = true;
                    break;
                }
                return;
            }

            // check for focus on resource tree or list
            if (lstResources.Focused || tvwResources.Focused) {
                if (e.Control) {
                    switch (e.KeyCode) {
                    case Keys.F:
                        SearchForID();
                        break;
                    }
                    e.SuppressKeyPress = true;
                }
                return;
            }
        }

        private void frmMDIMain_KeyPress(object sender, KeyPressEventArgs e) {
            // grid keypresses occur AFTER the form keypresses

            // if the active control is the property grid, 
            // we need to handle some keypresses
            if (ActiveControl == splResource && splResource.ActiveControl == propertyGrid1) {
                // for some properties, direct editing not allowed
                switch (propertyGrid1.SelectedGridItem.Label) {
                case "GameID":
                    // only alphanumeric, backspace, delete, return
                    if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '\b' && e.KeyChar != '\r' && e.KeyChar != '\x7f') {
                        e.Handled = true;
                    }
                    break;
                case "Number":
                case "ID":
                case "Designer":
                case "Description":
                case "GameVer":
                case "GameAbout":
                case "ResDir":
                case "ViewDesc":
                    // never allow direct editing
                    e.Handled = true;
                    break;
                }
                return;
            }

            // after processing key press for the main form, pass it 
            // to preview window, if it's active
            if (!e.Handled) {
                if (ActiveMdiChild == PreviewWin) {
                    PreviewWin.KeyHandler(e);
                }
            }
        }

        private void frmMDIMain_MdiChildActivate(object sender, EventArgs e) {
            // update statusbar
            ResetStatusStrip();
            if (ActiveMdiChild is not null) {
                // !!!
                // statusstrip merging SUCKS... so we do it manually
                // !!!
                MergeStatusStrip(ActiveMdiChild);
            }
            // update toolbar
            if (ActiveMdiChild is null) {
                UpdateTBResourceBtns(None, false, false, -1);
            }
            else {
                bool ingame;
                bool changed;
                AGIResType restype;
                int resnum = -1;

                if (ActiveMdiChild is frmPreview) {
                    UpdateTBResourceBtns(SelResType, true, false, SelResNum);
                }
                else if (ActiveMdiChild is frmLogicEdit logicForm) {
                    restype = logicForm.FormMode == LogicFormMode.Logic ?
                        AGIResType.Logic : Include;
                    resnum = logicForm.LogicNumber;
                    ingame = logicForm.InGame;
                    changed = logicForm.IsChanged;
                    UpdateTBResourceBtns(restype, ingame, changed, resnum);
                    // add/remove only if a filename has been assigned
                    if (logicForm.FormMode == LogicFormMode.Logic) {
                        btnAddRemove.Enabled = logicForm.EditLogic.SourceFile.Length > 0;
                    }
                    else {
                        btnAddRemove.Enabled = logicForm.TextFilename.Length > 0;
                    }
                }
                else if (ActiveMdiChild is frmPicEdit picForm) {
                    restype = AGIResType.Picture;
                    resnum = picForm.PictureNumber;
                    ingame = picForm.InGame;
                    changed = picForm.IsChanged;
                    UpdateTBResourceBtns(restype, ingame, changed, resnum);
                }
                else if (ActiveMdiChild is frmSoundEdit soundForm) {
                    restype = AGIResType.Sound;
                    resnum = soundForm.SoundNumber;
                    ingame = soundForm.InGame;
                    changed = soundForm.IsChanged;
                    UpdateTBResourceBtns(restype, ingame, changed, resnum);
                }
                else if (ActiveMdiChild is frmViewEdit viewForm) {
                    restype = AGIResType.View;
                    resnum = viewForm.ViewNumber;
                    ingame = viewForm.InGame;
                    changed = viewForm.IsChanged;
                    UpdateTBResourceBtns(restype, ingame, changed, resnum);
                }
                else if (ActiveMdiChild is frmGlobals globalsForm) {
                    restype = Globals;
                    ingame = globalsForm.InGame;
                    changed = globalsForm.IsChanged;
                    UpdateTBResourceBtns(restype, ingame, changed, resnum);
                }
                else if (ActiveMdiChild is frmLayout layoutForm) {
                    changed = layoutForm.IsChanged;
                    UpdateTBResourceBtns(AGIResType.Layout, true, changed, -1);
                }
                else if (ActiveMdiChild is frmObjectEdit objectForm) {
                    changed = objectForm.IsChanged;
                    UpdateTBResourceBtns(Objects, false, changed, -1);
                }
                else if (ActiveMdiChild is frmWordsEdit wordsForm) {
                    changed = wordsForm.IsChanged;
                    UpdateTBResourceBtns(Words, false, changed, -1);
                }
                else if (ActiveMdiChild is frmTextScreenEdit txtscreenForm) {
                    changed = txtscreenForm.IsChanged;
                    UpdateTBResourceBtns(TextScreen, false, changed, -1);
                }
                else if (ActiveMdiChild is frmMenuEdit menuForm) {
                    changed = EditGame is not null && menuForm.IsChanged && menuForm.MenuLogic != -1;
                    UpdateTBResourceBtns(Menu, false, changed, -1);
                }
                else {
                    UpdateTBResourceBtns(None, false, false, -1);
                }
            }
            // hide preview window if needed
            if (WinAGISettings.ShowPreview.Value && WinAGISettings.HidePreview.Value) {
                if (ActiveMdiChild is not null && ActiveMdiChild != PreviewWin) {
                    PreviewWin.Hide();
                }
            }
        }

        private void frmMDIMain_FormClosing(object sender, FormClosingEventArgs e) {
            bool lastLoad;

            // force cancel, in case one of the child forms wants to
            // stay open
            e.Cancel = false;
            if (EditGame is null) {
                lastLoad = false;
            }
            else {
                lastLoad = true;
            }
            if (lastLoad) {
                try {
                    e.Cancel = !CloseGame();
                }
                catch (Exception ex) {
                    ErrMsgBox(ex,
                        "An error occurred while trying to close:",
                        ex.StackTrace,
                        "Critical Error");
                }
            }
            if (e.Cancel) {
                return;
            }
            // before closing, make sure all non-resource editors are also closed,
            // canceling if the user decides not to close
            GlobalsEditor?.Close();
            if (GlobalsEditor is not null) {
                e.Cancel = true;
                return;
            }
            MenuEditor?.Close();
            if (MenuEditor is not null) {
                e.Cancel = true;
                return;
            }
            LayoutEditor?.Close();
            if (LayoutEditor is not null) {
                e.Cancel = true;
                return;
            }
            TextScreenEditor?.Close();
            if (TextScreenEditor is not null) {
                e.Cancel = true;
                return;
            }

            WinAGISettingsFile.WriteSetting(sMRULIST, "LastLoad", lastLoad);
            SaveSettings();

            // detach  AGI game events
            NewGameStatus -= MDIMain.GameEvents_NewGameStatus;
            LoadGameStatus -= MDIMain.GameEvents_LoadGameStatus;
            CompileGameStatus -= MDIMain.GameEvents_CompileGameStatus;
            CompileLogicStatus -= MDIMain.GameEvents_CompileLogicStatus;
            DecodeLogicStatus -= MDIMain.GameEvents_DecodeLogicStatus;
            // dispose all
            MDIMain = null;
            Dispose();
            PreviewWin?.Dispose();
            ProgressWin?.Dispose();
            CompStatusWin?.Dispose();
            ObjectEditor?.Dispose();
            WordEditor?.Dispose();
            LayoutEditor?.Dispose();
            GlobalsEditor?.Dispose();
            MenuEditor?.Dispose();
            FindingForm?.Dispose();
            bgwCompGame?.Dispose();
            bgwNewGame?.Dispose();
            bgwOpenGame?.Dispose();
            Application.Exit();
        }

        private void frmMDIMain_Activated(object sender, EventArgs e) {
            if (ActiveMdiChild is not null && ActiveMdiChild.Name == "frmLogicEdit") {
                // force focus back to the editor textbox
                frmLogicEdit frm = (frmLogicEdit)ActiveMdiChild;
                frm.fctb.Focus();
            }
        }

        private void frmMDIMain_HelpRequested(object sender, HelpEventArgs hlpevent) {
            if (ShowingMsgBox) {
                // message boxes handle their own help requests, but they
                // don't mark the event as handled, so it bubbles up; 
                // to keep the form's help from overriding, ignore the help 
                // request here
                hlpevent.Handled = true;
                return;
            }
            ShowHelp();
            hlpevent.Handled = true;
        }

        public void OnIdle(object sender, EventArgs e) {
            // Update the 'lock' panels when the program is idle
            // unless panel is in a window that doesn't show them
            if (MainStatusBar.Items.ContainsKey(nameof(spCapsLock))) {
                bool newCapsLock = Console.CapsLock;
                bool newNumLock = Console.NumberLock;
                bool newOverwrite = IsKeyLocked(Keys.Insert);
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
                if (newOverwrite != Overwrite) {
                    Overwrite = newOverwrite;
                    if (spInsLock is not null) {
                        spInsLock.Text = Overwrite ? "OVR" : "INS";
                    }
                    // let the textscreen editor know
                    if (TSEInUse) {
                        TextScreenEditor.ToggleINSMode();
                    }
                }
            }
            // if the active control on the main form is the property grid, 
            // disable the property grid's context menu (by assigning a blank one)
            var gridtext = propertyGrid1.ActiveControl as TextBox;
            if (gridtext is not null) {
                gridtext.ContextMenuStrip ??= new();
            }
            // if a view editor is active, check for a property grid context menu
            // and create if it doesn't exist (but only for the ViewDesc property-
            // clear it if it's not ViewDesc)
            if (ActiveMdiChild is frmViewEdit viewForm) {
                if (viewForm.propertyGrid1.SelectedGridItem is not null) {
                    var viewgridtext = viewForm.propertyGrid1.ActiveControl as TextBox;
                    if (viewgridtext is not null) {
                        if (viewForm.propertyGrid1.SelectedGridItem.Label == "ViewDesc") {
                            viewForm.EditTextBox = viewgridtext;
                            viewgridtext.ContextMenuStrip ??= viewForm.cmViewDesc;
                        }
                        else {
                            if (viewgridtext.ContextMenuStrip is null) {
                                return;
                            }
                            viewgridtext.ContextMenuStrip = null;
                        }
                    }
                }
            }
        }

        private void StatusFlashTimer_Tick(object sender, EventArgs e) {
            StatusFlashCount++;
            if (StatusFlashCount > 8) {
                // stop timer and reset status strip
                statusFlashTimer.Enabled = false;
                return;
            }
            if ((StatusFlashCount & 1) == 1) {
                spStatus.BackColor = Color.Red;
                spStatus.ForeColor = Color.White;
            }
            else {
                spStatus.BackColor = SystemColors.Control;
                spStatus.ForeColor = SystemColors.ControlText;
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
            // scroll the navlist
            if (NLRow < 0) {
                // scroll up
                if (NLOffset > 3) {
                    NLOffset--;
                    picNavList_Paint(sender, new PaintEventArgs(picNavList.CreateGraphics(), picNavList.Bounds));
                }
            }
            else {
                // scroll down
                if (NLOffset < ResQueue.Length - 3) {
                    NLOffset++;
                    picNavList_Paint(sender, new PaintEventArgs(picNavList.CreateGraphics(), picNavList.Bounds));
                }
            }
        }
        #endregion

        #region Menu Item Events
        #region Game Menu
        private void mnuGame_DropDownOpening(object sender, EventArgs e) {
            mnuGClose.Enabled = EditGame is not null;
            mnuGCompile.Enabled = EditGame is not null;
            mnuGCompileTo.Enabled = EditGame is not null;
            mnuGRun.Enabled = EditGame is not null;
            mnuGRebuild.Enabled = EditGame is not null;
            mnuGCompileChanged.Enabled = EditGame is not null;
            mnuGProperties.Enabled = EditGame is not null;
            mnuRImport.Enabled = EditGame is not null;
        }

        private void mnuGNewTemplate_Click(object sender, EventArgs e) {
            // create new game using template
            NewGame(true);
        }

        private void mnuGNewBlank_Click(object sender, EventArgs e) {
            // create new blank game
            NewGame(false);
        }

        private void mnuGImport_Click(object sender, EventArgs e) {
            // import a game by directory
            ImportGame();
        }

        private void mnuGOpen_Click(object sender, EventArgs e) {
            // open a game
            OpenWAGFile();
        }

        private void mnuGClose_Click(object sender, EventArgs e) {
            if (CloseGame()) {
                LastNodeName = "";
            }
        }

        private void mnuGCompile_Click(object sender, EventArgs e) {
            if (EditGame is null) {
                return;
            }
            CompileGame(EditGame.GameDir);
        }

        private void mnuGCompileTo_Click(object sender, EventArgs e) {
            if (EditGame is null) {
                return;
            }
            CompileGame();
        }

        private void mnuGRebuild_Click(object sender, EventArgs e) {
            if (EditGame is null) {
                return;
            }
            CompileGame(EditGame.GameDir, true);
        }

        private void mnuGCompileChanged_Click(object sender, EventArgs e) {
            if (EditGame is null) {
                return;
            }
            CompileChangedLogics();
        }

        private void mnuGRun_Click(object sender, EventArgs e) {
            if (EditGame is null) {
                return;
            }
            RunGame();
        }

        private void mnuGProperties_Click(object sender, EventArgs e) {
            if (EditGame is null) {
                return;
            }
            ShowProperties();
        }

        private void mnuGMRU_Click(object sender, EventArgs e) {
            // open the mru game assigned to this menu item
            _ = int.TryParse(((ToolStripMenuItem)sender).Tag.ToString(), out int index);
            OpenMRUGame(index);
        }

        private void mnuGExit_Click(object sender, EventArgs e) {
            // shut it all down
            Close();
        }
        #endregion

        #region Resources Menu
        private void mnuResources_DropDownOpening(object sender, EventArgs e) {
            // configure the dropdown menu before displaying it:
            // - if preview window is the active mdi form, align the
            //    menu for the currently selected resource
            // - if any other form
            //    is active, align it for the current form

            if (ActiveMdiChild is not null) {
                // configure for the current editor
                if (ActiveMdiChild is frmLogicEdit logicForm) {
                    logicForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmPicEdit picForm) {
                    picForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmSoundEdit soundForm) {
                    soundForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmViewEdit viewForm) {
                    viewForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmGlobals globalsForm) {
                    globalsForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmLayout layoutForm) {
                    layoutForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmObjectEdit objectForm) {
                    objectForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmWordsEdit wordsForm) {
                    wordsForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmTextScreenEdit txtscreenForm) {
                    txtscreenForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmMenuEdit menuForm) {
                    menuForm.SetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmPreview previewForm) {
                    previewForm.SetResourceMenu();
                }
                else {
                    Debug.Assert(false);
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
                mnuRSep2.Visible = true;
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
                mnuRSep2.Visible = true;
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
                mnuRSep2.Visible = true;
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
                if (SelResType == Game) {
                    mnuRExport.Visible = true;
                    mnuRExport.Text = "Export All Resources";
                }
                else {
                    mnuRExport.Visible = false;
                }
                mnuRSep2.Visible = true;
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
            mnuRSep2.Visible = true;
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
                err = EditGame.Logics[SelResNum].SourceError == ResourceErrorType.LogicSourceAccessError;
                mnuRSep3.Visible = !EditGame.Logics[SelResNum].Compiled;
                mnuRCompileLogic.Visible = !EditGame.Logics[SelResNum].Compiled;
                mnuRCompileLogic.Enabled = err;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                break;
            case AGIResType.Picture:
                err = EditGame.Pictures[SelResNum].Error != ResourceErrorType.NoError;
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
                //err = EditGame.Sounds[SelResNum].Error != ResourceErrorType.NoError;
                mnuRSep3.Visible = false;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                break;
            case AGIResType.View:
                err = EditGame.Views[SelResNum].Error != ResourceErrorType.NoError;
                mnuRSep3.Visible = true;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Text = "Export Loop As Animated GIF...";
                mnuRExportGIF.Visible = true;
                mnuRExportGIF.Enabled = !err && PreviewWin.Visible;
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
            if (ActiveMdiChild is not null) {
                if (ActiveMdiChild is frmLogicEdit logicForm) {
                    logicForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmPicEdit picForm) {
                    picForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmSoundEdit soundForm) {
                    soundForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmViewEdit viewForm) {
                    viewForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmGlobals globalsForm) {
                    globalsForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmLayout layoutForm) {
                    layoutForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmObjectEdit objectForm) {
                    objectForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmWordsEdit wordsForm) {
                    wordsForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmTextScreenEdit txtscreenForm) {
                    txtscreenForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmMenuEdit menuForm) {
                    menuForm.ResetResourceMenu();
                    return;
                }
                else if (ActiveMdiChild is frmPreview previewForm) {
                    previewForm.ResetResourceMenu();
                }
                else {
                    Debug.Assert(false);
                }
            }
            mnuROpenRes.Enabled = true;
            mnuRExport.Enabled = true;
            mnuRSave.Enabled = true;

            mnuRRemove.Enabled = true;
            mnuRRenumber.Enabled = true;
            mnuRProperties.Enabled = true;

            mnuRCompileLogic.Enabled = true;
            mnuRSavePicImage.Enabled = true;
            mnuRExportGIF.Enabled = true;
        }

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
                cmRExport.Enabled = EditGame.InvObjects.Error == ResourceErrorType.NoError ||
                    EditGame.InvObjects.Error == ResourceErrorType.ObjectIsReadOnly;
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
                cmRExport.Enabled = EditGame.WordList.Error == ResourceErrorType.NoError ||
                    EditGame.WordList.Error == ResourceErrorType.WordsTokIsReadOnly;
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
            // INCLUDE
            if (SelResType == Include) {
                if (SelResNum == -1) {
                    // header- no options
                    e.Cancel = true;
                    return;
                }
                cmROpenRes.Visible = true;
                switch (EditGame.IncludeFiles[SelResNum].Type) {
                case IncludeType.Reserved:
                    cmROpenRes.Text = "Open Reserved Defines Editor";
                    cmRRemove.Enabled = false;
                    break;
                case IncludeType.ResourceIDs:
                    // not allowed
                    e.Cancel = true;
                    return;
                case IncludeType.Globals:
                    cmROpenRes.Text = "Open Global Defines Editor";
                    cmRRemove.Enabled = false;
                    break;
                case IncludeType.Sysdefs:
                case IncludeType.Gamedefs:
                case IncludeType.Other:
                    cmROpenRes.Text = "Open " + Path.GetFileName(EditGame.IncludeFiles[SelResNum].Filename);
                    cmRRemove.Enabled = true;
                    break;
                }
                cmRSave.Visible = true;
                cmRSave.Text = "Save";
                cmRSave.Enabled = false;
                cmRExport.Visible = true;
                cmRExport.Text = "Export";
                cmRExport.Enabled = false;
                cmRRemove.Visible = true;
                cmRRemove.Text = "Remove from Game";
                cmRRenumber.Visible = false;
                cmRProperties.Visible = false;
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
                if (SelResType == Game) {
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
                // if error other than readonly
                err = EditGame.Pictures[SelResNum].Error != ResourceErrorType.NoError &&
                    EditGame.Pictures[SelResNum].Error != ResourceErrorType.FileIsReadonly;
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
                err = EditGame.Sounds[SelResNum].Error != ResourceErrorType.NoError &&
                    EditGame.Sounds[SelResNum].Error != ResourceErrorType.FileIsReadonly;
                cmRSep2.Visible = false;
                cmRCompileLogic.Visible = false;
                cmRSavePicImage.Visible = false;
                cmRExportGIF.Visible = false;
                break;
            case AGIResType.View:
                err = EditGame.Views[SelResNum].Error != ResourceErrorType.NoError &&
                    EditGame.Views[SelResNum].Error != ResourceErrorType.FileIsReadonly;
                cmRSep2.Visible = true;
                cmRCompileLogic.Visible = false;
                cmRSavePicImage.Visible = false;
                cmRExportGIF.Text = "Export Loop As Animated GIF...";
                cmRExportGIF.Visible = true;
                cmRExportGIF.Enabled = !err && PreviewWin.Visible;
                break;
            }
            // if resource has an error, only add/remove is enabled
            cmROpenRes.Enabled = !err;
            cmRExport.Enabled = !err;
            cmRRenumber.Enabled = !err;
            cmRProperties.Enabled = !err;
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
            if (EditGame is not null) {
                OpenGameLogic();
            }
            else {
                OpenLogic();
            }
        }

        internal void mnuROPicture_Click(object sender, EventArgs e) {
            if (EditGame is not null) {
                OpenGamePicture();
            }
            else {
                OpenPicture();
            }
        }

        internal void mnuROSound_Click(object sender, EventArgs e) {
            if (EditGame is not null) {
                OpenGameSound();
            }
            else {
                OpenSound();
            }
        }

        internal void mnuROView_Click(object sender, EventArgs e) {
            if (EditGame is null) {
                OpenView();
            }
            else {
                OpenGameView();
            }
        }

        internal void mnuROObjects_Click(object sender, EventArgs e) {
            if (EditGame is not null && !OEInUse) {
                OpenGameOBJECT();
            }
            else {
                string filename = GetOpenResourceFilename("Open ", Objects);
                if (filename.Length > 0) {
                    OpenOBJECT(filename);
                }
            }
        }

        internal void mnuROWords_Click(object sender, EventArgs e) {
            if (EditGame is not null && !WEInUse) {
                OpenGameWORDSTOK();
            }
            else {
                string filename = GetOpenResourceFilename("Open ", Words);
                if (filename.Length > 0) {
                    OpenWORDSTOK(filename);
                }
            }
        }

        internal void mnuROText_Click(object sender, EventArgs e) {
            string filename = GetOpenResourceFilename("Open ", Include);
            if (filename.Length > 0) {
                OpenTextFile(filename);
            }
        }

        internal void mnuRILogic_Click(object sender, EventArgs e) {
            string importfile = GetOpenResourceFilename("Import ", AGIResType.Logic);
            if (importfile.Length > 0) {
                if (ActiveMdiChild is not null) {
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
                SoundImportFormat format = SoundImport.GetSoundImportFormat(importfile);
                SoundImportOptions options;
                // IT, MOD, and MIDI have import options
                switch (format) {
                case SoundImportFormat.IT:
                case SoundImportFormat.MOD:
                case SoundImportFormat.MIDI:
                    // get options
                    using (var frm = new frmImportSoundOptions(format)) {
                        if (frm.ShowDialog(MDIMain) == DialogResult.OK) {
                            options = frm.Options;
                            frm.Dispose();
                        }
                        else {
                            // canceled
                            return;
                        }
                    }
                    break;
                default:
                    // no options needed
                    options = null;
                    break;
                }
                if (ActiveMdiChild.Name == "frmSoundEdit") {
                    if (MessageBox.Show(MDIMain,
                        "Do you want to replace the sound you are currently editing?",
                        "Import Sound",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes) {
                        ((frmSoundEdit)ActiveMdiChild).ImportSound(importfile, format, options);
                        return;
                    }
                }
                NewSound(importfile, format, options);
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
            string importfile = GetOpenResourceFilename("Import ", Objects);
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
            string importfile = GetOpenResourceFilename("Import ", Words);
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
            case Objects:
                OpenGameOBJECT();
                break;
            case Words:
                OpenGameWORDSTOK();
                break;
            case Include:
                OpenInclude(SelResNum);
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
            // only if  previewing a valid picture
            if (SelResType != AGIResType.Picture ||
                (EditGame.Pictures[SelResNum].Error != ResourceErrorType.NoError &&
                EditGame.Pictures[SelResNum].Error != ResourceErrorType.FileIsReadonly)) {
                return;
            }
            switch (SelResNum) {
            case -1:
                ExportAllPicImgs();
                break;
            default:
                ExportOnePicImg(EditGame.Pictures[SelResNum]);
                break;
            }
        }

        private void mnuRExportGIF_Click(object sender, EventArgs e) {
            switch (SelResType) {
            case AGIResType.Picture:
                if (EditGame.Pictures[SelResNum].Error == ResourceErrorType.NoError ||
                    EditGame.Pictures[SelResNum].Error == ResourceErrorType.FileIsReadonly) {
                    ExportPicAsGif(EditGame.Pictures[SelResNum]);
                }
                break;
            case AGIResType.View:
                if (EditGame.Views[SelResNum].Error == ResourceErrorType.NoError ||
                    EditGame.Views[SelResNum].Error == ResourceErrorType.FileIsReadonly) {
                    ExportLoopGIF(EditGame.Views[SelResNum], 0);
                }
                break;
            }
        }
        #endregion

        #region Tools Menu
        private void mnuTools_DropDownOpening(object sender, EventArgs e) {
            mnuTLayout.Enabled = EditGame is not null && EditGame.UseLE;
            mnuTWarning.Enabled = EditGame is not null;
            mnuTWarning.Text = pnlInfoGrid.Visible ? "Hide Warning List" : "Show Warning List";
        }

        private void mnuTSettings_Click(object sender, EventArgs e) {
            // starting page depends on currently active form
            int startpage = 0;
            if (ActiveMdiChild is not null) {
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
                case "frmLayout":
                    startpage = 6;
                    break;
                }
            }
            using (frmSettings frm = new(startpage)) {
                frm.ShowDialog(MDIMain);
            }
            // the settings form handles all mods and updates based on settings changes
        }

        private void mnuTLayout_Click(object sender, EventArgs e) {
            OpenLayout();
        }

        private void mnuTTextEd_Click(object sender, EventArgs e) {
            OpenTextscreenEditor();
        }

        private void mnuTMenuEditor_Click(object sender, EventArgs e) {
            if (EditGame is not null && EditGame.SierraSyntax) {
                return;
            }
            OpenMenuEditor();
        }

        private void mnuTGlobals_Click(object sender, EventArgs e) {
            if (sender == btnGlobals) {
                if (EditGame is not null) {
                    if (GEInUse) {
                        // switch to game's global editor
                        GlobalsEditor.Select();
                        return;
                    }
                }
            }
            OpenGlobals(GEInUse);
        }

        private void mnuTReserved_Click(object sender, EventArgs e) {
            OpenReservedEditor();
        }

        private void mnuTSnippets_Click(object sender, EventArgs e) {
            using (frmSnippets Snippets = new(false)) {
                Snippets.ShowDialog(this);
            }
        }

        private void mnuTPalette_Click(object sender, EventArgs e) {
            using (frmPalette frm = new(0)) {
                if (frm.ShowDialog(MDIMain) == DialogResult.OK) {
                    // refresh all picture, view and textscreen editors and the preview window (if visible)
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
                    if (TSEInUse) {
                        TextScreenEditor.RefreshScreen();
                    }
                }
            }
        }

        private void mnuTWarning_Click(object sender, EventArgs e) {
            if (pnlInfoGrid.Visible) {
                HideInfoGrid();
            }
            else {
                if (EditGame is not null) {
                    ShowInfoGrid();
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
                    ErrMsgBox(ex,
                        "Unable to open this URL.",
                        target,
                        "Custom Tool Error");
                }
            }
            else {
                // save the old path, so it can be restored once we're done
                string oldpath = Directory.GetCurrentDirectory();

                // if a game is open, assume it's the current directory;
                // otherwise, assume program directory is current directory
                if (EditGame is not null) {
                    Directory.SetCurrentDirectory(EditGame.GameDir);
                }
                else {
                    Directory.SetCurrentDirectory(ProgramDir);
                }
                // does this tool entry include a directory?
                string filename;
                if (Path.GetDirectoryName(target).Length > 0) {
                    // if a path is provided, check for program dir
                    filename = target.Replace("%PROGDIR%", ProgramDir);
                }
                else {
                    filename = target;
                }

                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = filename,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex) {
                    ErrMsgBox(ex,
                        "Sorry, an error occurred when trying to open this tool entry.",
                        "Please edit your tool information to point to a valid file/program.",
                        "Custom Tool Error");
                }
                // restore current directory
                Directory.SetCurrentDirectory(oldpath);
            }
        }

        private void mnuTCustomize_Click(object sender, EventArgs e) {
            using (var ToolsEditor = new frmTools()) {
                ToolsEditor.ShowDialog(this);
            }
        }
        #endregion

        #region Windows Menu
        private void mnuWindow_DropDownOpening(object sender, EventArgs e) {
            // disable the close item if no windows or if active window is preview
            mnuWClose.Enabled = (MdiChildren.Length != 0) && (ActiveMdiChild != PreviewWin) && (ActiveMdiChild is not null);
            foreach (ToolStripItem item in mnuWindow.DropDownItems) {
                if (item is ToolStripMenuItem) {
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
            // need to mimic HelpRequested event
            if (ActiveMdiChild is not null) {
                if (ActiveMdiChild is frmGlobals frmG) {
                    frmG.ShowHelp();
                }
                else if (ActiveMdiChild is frmLayout frmLO) {
                    frmLO.ShowHelp();
                }
                else if (ActiveMdiChild is frmLogicEdit frmLE) {
                    frmLE.ShowHelp();
                }
                else if (ActiveMdiChild is frmMenuEdit frmME) {
                    frmME.ShowHelp();
                }
                else if (ActiveMdiChild is frmObjectEdit frmOE) {
                    frmObjectEdit.ShowHelp();
                }
                else if (ActiveMdiChild is frmPicEdit frmPE) {
                    frmPE.ShowHelp();
                }
                else if (ActiveMdiChild is frmPreview frmP) {
                    frmP.ShowHelp();
                }
                else if (ActiveMdiChild is frmSoundEdit frmSE) {
                    frmSoundEdit.ShowHelp();
                }
                else if (ActiveMdiChild is frmTextScreenEdit frmTE) {
                    frmTextScreenEdit.ShowHelp();
                }
                else if (ActiveMdiChild is frmViewEdit frmVE) {
                    frmVE.ShowHelp();
                }
                else if (ActiveMdiChild is frmWordsEdit frmWE) {
                    frmWE.ShowHelp();
                }
            }
            else {
                if (EditGame is null) {
                    // show general help
                    Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\winagihelp.htm");
                }
                else {
                    if (WinAGISettings.ResListType.Value == ResListType.None) {
                        // show general help
                        Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\winagihelp.htm");
                    }
                    else {
                        // show restree help
                        Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\resourcetree.htm");
                    }
                }
            }
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
            using (var frm = new frmAbout()) {
                frm.ShowDialog(MDIMain);
            }
        }
        #endregion
        #endregion

        #region Panel Splitter Events
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

        #region Resource Tree/List Events
        private void tvwResources_MouseDown(object sender, MouseEventArgs e) {
            // force selection to change BEFORE context menu is shown
            if (e.Button == MouseButtons.Right) {
                TreeNode node = tvwResources.GetNodeAt(e.X, e.Y);
                if (node is not null) {
                    tvwResources.SelectedNode = node;
                }
            }
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
                case Include:
                    OpenInclude(SelResNum);
                    break;
                }
                break;
            }
        }

        private void tvwResources_AfterSelect(object sender, TreeViewEventArgs e) {
            // probably due to navigation - 
            // there are some operations that seem to trigger this event when
            // the tree isn't visible and no game is being edited; examples include
            //    - opening help file then closing the program,
            //    - opening new game in some scenarios,
            //    - ? others?
            if (EditGame is null) {
                return;
            }

            // if no active form
            if (MdiChildren.Length == 0) {
                // set control focus to treeview
                tvwResources.Select();
            }
            // if nothing selected
            if (e.Node is null) {
                return;
            }
            // if not changed from previous number
            if (LastNodeName == e.Node.Name) {
                if (!PreviewWin.Visible && WinAGISettings.ShowPreview.Value) {
                    PreviewWin.Show();
                    // set form focus to preview
                    PreviewWin.Activate();
                    // set control focus to tvwlist
                    tvwResources.Select();
                }
                // don't need to change anything
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
                SelectResource((AGIResType)e.Node.Parent.Index, (int)e.Node.Tag);
                // logics only - it's possible for the source to be edited
                // outside WinAGI, so always check compiled status
                if ((AGIResType)e.Node.Parent.Index == AGIResType.Logic) {
                    if (EditGame.Logics[(int)e.Node.Tag].Compiled) {
                        e.Node.ForeColor = Color.Black;
                    }
                    else {
                        e.Node.ForeColor = Color.Red;
                    }
                }
            }
            // after selection, force preview window to show and
            // move up, if those settings are active
            if (WinAGISettings.ShowPreview.Value) {
                if (!PreviewWin.Visible) {
                    PreviewWin.Show();
                    PreviewWin.Activate();
                    tvwResources.Select();
                }
                else {
                    if (ActiveMdiChild != PreviewWin && WinAGISettings.ShiftPreview.Value) {
                        // if previn hidden on lostfocus, need to show it AFTER changing displayed resource
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
                    tmpItem.Tag = (int)tmpRes.Number;
                    tmpItem.ForeColor = tmpRes.Compiled ? Color.Black : Color.Red;
                }
                selRes = AGIResType.Logic;
                break;
            case 2:
                foreach (Picture tmpRes in EditGame.Pictures) {
                    tmpItem = lstResources.Items.Add("p" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = (int)tmpRes.Number;
                }
                selRes = AGIResType.Picture;
                break;
            case 3:
                foreach (Sound tmpRes in EditGame.Sounds) {
                    tmpItem = lstResources.Items.Add("s" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = (int)tmpRes.Number;
                }
                selRes = AGIResType.Sound;
                break;
            case 4:
                foreach (Engine.View tmpRes in EditGame.Views) {
                    tmpItem = lstResources.Items.Add("v" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = (int)tmpRes.Number;
                }
                selRes = AGIResType.View;
                break;
            case 5:
                // objects
                selRes = Objects;
                break;
            case 6:
                // words
                selRes = Words;
                break;
            case 7:
                // includes
                for (int i = 0; i < EditGame.IncludeFiles.Count; i++) {
                    tmpItem = lstResources.Items.Add("i" + i, Path.GetFileName(EditGame.IncludeFiles[i].Filename), 0);
                    if (EditGame.IncludeIDs &&
                        EditGame.IncludeFiles[i].Filename.Equals(
                            Path.Combine(EditGame.SrcResDir, "resourceids.txt"), StringComparison.OrdinalIgnoreCase)) {
                        tmpItem.ForeColor = Color.Blue;
                    }
                    if (EditGame.IncludeReserved &&
                        EditGame.IncludeFiles[i].Filename.Equals(
                            Path.Combine(EditGame.SrcResDir, "reserved.txt"), StringComparison.OrdinalIgnoreCase)) {
                        tmpItem.ForeColor = Color.Blue;
                    }
                    if (EditGame.IncludeGlobals &&
                        EditGame.IncludeFiles[i].Filename.Equals(
                            Path.Combine(EditGame.SrcResDir, "globals.txt"), StringComparison.OrdinalIgnoreCase)) {
                        tmpItem.ForeColor = Color.Blue;
                    }
                    tmpItem.Tag = i;
                }
                selRes = Include;
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
            case Include:
                OpenInclude(SelResNum);
                break;
            }
        }

        private void lstResources_SelectedIndexChanged(object sender, EventArgs e) {
            AGIResType newType;
            int newNum;

            switch (cmbResType.SelectedIndex) {
            case 0:
                // game - root
                // no list items
                newType = Game;
                newNum = -1;
                break;
            case 5:
                // objects
                // no listitems
                newType = Objects;
                newNum = -1;
                break;
            case 6:
                // words
                // no listitems
                newType = Words;
                newNum = -1;
                break;
            default:
                newType = (AGIResType)(cmbResType.SelectedIndex - 1);
                // if nothing selected
                if (lstResources.SelectedItems.Count == 0) {
                    // delect root
                    newNum = -1;
                }
                else {
                    newNum = (int)lstResources.SelectedItems[0].Tag;
                }
                break;
            }
            if (!DontQueue) {
                SelectResource(newType, newNum);
            }
            // after selection, force preview window to show and
            // move up, if those settings are active
            if (WinAGISettings.ShowPreview.Value) {
                if (!PreviewWin.Visible) {
                    PreviewWin.Show();
                    PreviewWin.Activate();
                    tvwResources.Select();
                }
                else {
                    if (ActiveMdiChild != PreviewWin && WinAGISettings.ShiftPreview.Value) {
                        // if previn hidden on lostfocus, need to show it AFTER changing displayed resource
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

        #region Resource Navigation Events
        private void cmdBack_Click(object sender, EventArgs e) {
            // if resqptr is not at beginning, go back one
            // and select that resource
            if (ResQPtr > 0) {
                // back up one
                ResQPtr--;
                // select this node if still present
                SelectFromQueue();

                // adjust the buttons for availability
                cmdBack.Enabled = (ResQPtr > 0);
                cmdForward.Enabled = true;
            }

            // always set focus to the resource list
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                tvwResources.Select();
                break;
            case ResListType.ComboList:
                lstResources.Select();
                break;
            }
        }

        private void cmdBack_MouseDown(object sender, MouseEventArgs e) {
            // if right button, show list of resources on nav stack
            if (e.Button == MouseButtons.Right) {
                // set left edge of list to match this button
                picNavList.Left = cmdBack.Left;
                picNavList.Width = cmdBack.Width;
                // show it
                picNavList.Visible = true;
                // set mouse capture to the list picture
                picNavList.Capture = true;
                // offset is current queue position
                NLOffset = ResQPtr;
            }
        }

        private void cmdForward_Click(object sender, EventArgs e) {
            // if resqptr is not at end, go forward one
            // and select that resource
            if (ResQPtr < ResQueue.Length - 1) {
                // go forward one
                ResQPtr++;
                // select this node if still present
                SelectFromQueue();

                // adjust the buttons for availability
                cmdBack.Enabled = true;
                cmdForward.Enabled = ResQPtr < ResQueue.Length - 1;
            }

            // always set focus to the resource list
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                tvwResources.Select();
                break;
            case ResListType.ComboList:
                lstResources.Select();
                break;
            }
        }

        private void cmdForward_MouseDown(object sender, MouseEventArgs e) {
            // if right button, show list of resources on nav stack
            if (e.Button == MouseButtons.Right) {
                // set left edge of list to match this button
                picNavList.Left = cmdForward.Left;
                picNavList.Width = cmdForward.Width;
                picNavList.Top = cmdForward.Parent.Parent.Parent.Top + e.Y - picNavList.Height / 2;
                // show it
                picNavList.Visible = true;
                // set mouse capture to the list picture
                picNavList.Capture = true;
                // offset is current queue position
                NLOffset = ResQPtr;
            }
        }

        private void picNavList_MouseMove(object sender, MouseEventArgs e) {
            int SelRow;

            if (e.Button != MouseButtons.Right) {
                return;
            }

            // if selrow has changed, repaint

            // determine which row is under cursor
            SelRow = (int)Math.Floor((float)e.Location.Y / NLRowHeight);
            // if on a new row
            if (SelRow != NLRow) {
                // if both values still offscreen
                if (SelRow < 0 && NLRow < 0 || (SelRow > 4 && NLRow > 4)) {
                    // just update the selected row
                    NLRow = SelRow;
                    // no need to repaint
                    return;
                }

                // need to update and repaint
                NLRow = SelRow;
                picNavList_Paint(sender, new PaintEventArgs(picNavList.CreateGraphics(), picNavList.Bounds));
            }

            // if not on the list
            if (SelRow < 0 || SelRow > 4) {
                // enable autoscrolling
                tmrNavList.Enabled = true;
            }
            else {
                // no scrolling
                tmrNavList.Enabled = false;
            }
        }

        private void picNavList_Paint(object sender, PaintEventArgs e) {

            int i;
            SolidBrush hbrush = new(Color.FromArgb(0xff, 0xe0, 0xe0));//                  FFE0E0));
            SolidBrush bbrush = new(Color.Black);
            Font nlFont = new(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
            // draw list of resources on stack, according to current
            // offset; whatever is selected is also highlighted

            // start with a clean slate
            e.Graphics.Clear(picNavList.BackColor);

            PointF nlPoint = new();
            // display five lines
            for (i = 0; i < 5; i++) {
                if (i + NLOffset - 2 >= 0 && i + NLOffset - 2 <= ResQueue.Length - 1) {
                    // if this row is highlighted (under cursor and valid)
                    if (i == NLRow) {
                        e.Graphics.FillRectangle(hbrush, 0, (int)((i + 0.035) * NLRowHeight), picNavList.Width, NLRowHeight);
                    }
                    // set x and y positions
                    nlPoint.X = 1;
                    nlPoint.Y = NLRowHeight * i;

                    // add the id
                    switch (ResQueue[i + NLOffset - 2].ResType) {
                    case Game:
                        e.Graphics.DrawString(EditGame.GameID, nlFont, bbrush, nlPoint);
                        break;
                    case AGIResType.Logic:
                        if (ResQueue[i + NLOffset - 2].ResNum == 256) {
                            e.Graphics.DrawString("LOGICS", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Logics[ResQueue[i + NLOffset - 2].ResNum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case AGIResType.Picture:
                        if (ResQueue[i + NLOffset - 2].ResNum == 256) {
                            e.Graphics.DrawString("PICTURES", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Pictures[ResQueue[i + NLOffset - 2].ResNum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case AGIResType.Sound:
                        if (ResQueue[i + NLOffset - 2].ResNum == 256) {
                            e.Graphics.DrawString("SOUNDS", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Sounds[ResQueue[i + NLOffset - 2].ResNum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case AGIResType.View:
                        if (ResQueue[i + NLOffset - 2].ResNum == 256) {
                            e.Graphics.DrawString("VIEWS", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Views[ResQueue[i + NLOffset - 2].ResNum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case Objects:
                        e.Graphics.DrawString("OBJECTS", nlFont, bbrush, nlPoint);
                        break;
                    case Words:
                        e.Graphics.DrawString("WORDS", nlFont, bbrush, nlPoint);
                        break;
                    case Include:
                        if (ResQueue[i + NLOffset - 2].ResNum == 256) {
                            e.Graphics.DrawString("INCLUDES", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(Path.GetFileName(EditGame.IncludeFiles[ResQueue[i + NLOffset - 2].ResNum].Filename), nlFont, bbrush, nlPoint);
                        }
                        break;
                    }
                }
            }
        }

        private void picNavList_MouseUp(object sender, MouseEventArgs e) {
            int newPtr;
            picNavList.Visible = false;
            tmrNavList.Enabled = false;

            // get new ptr value; exit if it's invalid
            newPtr = NLOffset + NLRow - 2;
            if (newPtr < 0) {
                return;
            }
            else if (newPtr > ResQueue.Length - 1) {
                return;
            }

            // if selected row (including offset)
            // is different than current queue position
            if (newPtr != ResQPtr) {
                // move to the offset
                ResQPtr = newPtr;
                SelectFromQueue();
                // adjust the buttons for availability
                cmdBack.Enabled = ResQPtr > 0;
                cmdForward.Enabled = ResQPtr < (ResQueue.Length - 1);
            }
        }
        #endregion

        #region Property Grid Events
        private void propertyGrid1_MouseWheel(object s, MouseEventArgs e) {
            // wheeling on the IntVersion property should open the dropdownlist
            // but I don't know how to do it, so instead we ignore the mousewheel
            if (propertyGrid1.SelectedGridItem.Label == "IntVer") {
                ((HandledMouseEventArgs)e).Handled = true;
            }
        }
        #endregion

        #region WarningGrid Events
        private void btnClose_Click(object sender, EventArgs e) {
            HideInfoGrid();
        }

        private void warningToggle_Click(object sender, EventArgs e) {
            gridWarnings = !gridWarnings;
            if (gridWarnings) {
                warningToggle.BorderStyle = BorderStyle.FixedSingle;
            }
            else {
                warningToggle.BorderStyle = BorderStyle.None;
            }
            RefreshInfoGrid();
        }

        private void errorToggle_Click(object sender, EventArgs e) {
            gridErrors = !gridErrors;
            if (gridErrors) {
                errorToggle.BorderStyle = BorderStyle.FixedSingle;
            }
            else {
                errorToggle.BorderStyle = BorderStyle.None;
            }
            RefreshInfoGrid();
        }

        private void todoToggle_Click(object sender, EventArgs e) {
            gridTODOs = !gridTODOs;
            if (gridTODOs) {
                todoToggle.BorderStyle = BorderStyle.FixedSingle;
            }
            else {
                todoToggle.BorderStyle = BorderStyle.None;
            }
            RefreshInfoGrid();
        }

        private void gridFilter_SelectedIndexChanged(object sender, EventArgs e) {
            infoGridScope = (InfoGridScope)gridFilter.SelectedIndex;
            // refresh info grid
            RefreshInfoGrid();
        }

        private void fgWarnings_MouseDown(object sender, MouseEventArgs e) {
            // before displaying context menu select rows under cursor
            if (fgWarnings.SelectedRows.Count == 0) {
                return;
            }
            if (e.Button == MouseButtons.Right) {
                DataGridView.HitTestInfo hit = fgWarnings.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0 && hit.RowIndex != fgWarnings.SelectedRows[0].Index) {
                    fgWarnings.Rows[hit.RowIndex].Selected = true;
                }
            }
        }

        private void fgWarnings_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e) {
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++) {
                if (!Enum.TryParse<EventType>((string)fgWarnings.Rows[i].Cells[0].Value, out EventType evt)) {
                    continue;
                }
                // types Info, GameLoadError, GameCompileError don't get added
                switch (evt) {
                case LogicCompileError:
                case ResourceError:
                case DecompError:
                    // bold, red
                    fgWarnings.Rows[i].DefaultCellStyle.Font = new Font(fgWarnings.Font, FontStyle.Bold);
                    fgWarnings.Rows[i].DefaultCellStyle.ForeColor = Color.Red;
                    break;
                case TODO:
                    // bold, italic
                    fgWarnings.Rows[i].DefaultCellStyle.Font = new Font(fgWarnings.Font, FontStyle.Bold | FontStyle.Italic);
                    fgWarnings.Rows[i].DefaultCellStyle.ForeColor = Color.DarkGray;
                    break;
                case LogicCompileWarning:
                case ResourceWarning:
                case DecompWarning:
                    fgWarnings.Rows[i].DefaultCellStyle.Font = new Font(fgWarnings.Font, FontStyle.Regular);
                    fgWarnings.Rows[i].DefaultCellStyle.ForeColor = Color.Black;
                    break;
                default:
                    Debug.Assert(false);
                    break;
                }

            }
            // always make first row visible
            if (!fgWarnings.Rows[e.RowIndex].Displayed) {
                fgWarnings.CurrentCell = fgWarnings.Rows[e.RowIndex].Cells[2];
            }
        }

        private void fgWarnings_CellDoubleClick(object sender, DataGridViewCellEventArgs e) {
            if (fgWarnings.SelectedRows.Count != 1 || e.RowIndex == -1) {
                return;
            }
            if (e.RowIndex != fgWarnings.SelectedRows[0].Index) {
                fgWarnings.Rows[e.RowIndex].Selected = true;
            }
            // double-click does 'goto' event for logic related messages only
            if (!Enum.TryParse((string)fgWarnings.SelectedRows[0].Cells[0].Value, out EventType type)) {
                return;
            }
            switch (type) {
            case LogicCompileError:
            case LogicCompileWarning:
            case DecompError:
            case DecompWarning:
            case TODO:
            case ResourceWarning:
                // use the 'goto' menu event handler
                cmiGoWarning.PerformClick();
                break;
            case ResourceError:
                break;
            case GameLoadError:
                break;
            case GameCompileError:
                break;
            case Info:
                break;
            }
        }

        private void fgWarnings_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            try {
                int index = e.ColumnIndex;
                WarnGridSortOrder.Remove(index);
                WarnGridSortOrder.Insert(0, index);
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
            if (colindex != WarnGridSortOrder[0]) {
                WarnGridSortOrder.Remove(e.Column.Index);
                WarnGridSortOrder.Insert(0, e.Column.Index);
            }
            int retval = 0;
            for (int i = 0; i < 5; i++) {
                retval = CompareValues(fgWarnings.Rows[e.RowIndex1].Cells[WarnGridSortOrder[i]].Value.ToString(), fgWarnings.Rows[e.RowIndex2].Cells[WarnGridSortOrder[i]].Value.ToString(), (WarnGridSortOrder[i] == 4 || WarnGridSortOrder[i] == 5));
                if (retval != 0) {
                    break;
                }
            }
            e.SortResult = retval;
            e.Handled = true;

            static int CompareValues(string a, string b, bool isnum) {
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
            if (e.Value is null) {
                return;
            }
            // first determine if tooltip is needed
            DataGridViewCell cell = fgWarnings.Rows[e.RowIndex].Cells[e.ColumnIndex];

            string text = e.Value.ToString();
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
            // Declare a proposed size with dimensions set to the maximum integer value.
            Size proposedSize = new(int.MaxValue, int.MaxValue);
            // get size
            Size szText = TextRenderer.MeasureText(fgWarnings.CreateGraphics(), text, e.CellStyle.Font, proposedSize, flags);
            if (szText.Width > cell.Size.Width - 8) {
                cell.ToolTipText = text;
            }
            else {
                cell.ToolTipText = "";
            }
            // format negative resnums and lines as "--"
            if ((e.ColumnIndex == 4 || e.ColumnIndex == 5) && e.Value is int) {
                if (e.Value is int val) {
                    if (val < 0) {
                        e.Value = "--";
                        e.FormattingApplied = true;
                    }
                }
            }

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

        private void fgWarnings_DataError(object sender, DataGridViewDataErrorEventArgs e) {
            // sometimes the grid throws an error while refreshing- 
            // the error message says to implement this event...

        }

        private void cmsGrid_Opening(object sender, CancelEventArgs e) {
            Point mp = fgWarnings.PointToClient(MousePosition);
            DataGridView.HitTestInfo hit = fgWarnings.HitTest(mp.X, mp.Y);
            if (hit.RowIndex == -1) {
                e.Cancel = true;
                return;
            }

            // if no warnings, don't show menu
            if (infoGridTable.Rows.Count == 0) {
                e.Cancel = true;
                return;
            }
            // if nothing selected, don't show menu
            if (fgWarnings.SelectedRows.Count != 1) {
                e.Cancel = true;
                return;
            }
            if (!Enum.TryParse((string)fgWarnings.SelectedRows[0].Cells[0].Value, out EventType evt)) {
                e.Cancel = true;
                return;
            }
            string resname = (string)fgWarnings.SelectedRows[0].Cells[6].Value;
            switch (evt) {
            case ResourceError:
                // non-logic  error
                cmiDismiss.Visible = true;
                cmiDismiss.Text = "Dismiss this Resource Error";
                cmiDismissRes.Visible = false;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = false;
                cmiGoWarning.Text = "Goto ...";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Resource Error";
                break;
            case ResourceWarning:
                cmiDismiss.Text = "Dismiss this Resource Warning";
                cmiDismiss.Visible = true;
                cmiDismissRes.Text = "Dismiss all " + resname + " Warnings";
                cmiDismissRes.Visible = true;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = false;
                cmiGoWarning.Text = "Goto ...";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Resource Warning";
                // non-logic warning
                break;
            case LogicCompileError:
                cmiDismiss.Text = "Dissmiss this Error";
                cmiDismiss.Visible = true;
                cmiDismissRes.Visible = false;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = true;
                cmiGoWarning.Text = "Goto Compiler Error";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Compiler Error";
                break;
            case LogicCompileWarning:
                cmiDismiss.Text = "Dismiss this Warning";
                cmiDismiss.Visible = true;
                cmiDismissRes.Text = "Dismiss all " + resname + " Warnings";
                cmiDismissRes.Visible = true;
                cmiIgnoreWarning.Visible = true;
                cmiGoWarning.Enabled = true;
                cmiGoWarning.Text = "Goto Compiler Warning";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Compiler Warning";
                break;
            case DecompWarning:
                cmiDismiss.Text = "Dismiss this Warning";
                cmiDismiss.Visible = true;
                cmiDismissRes.Text = "Dismiss all " + resname + " Warnings";
                cmiDismissRes.Visible = true;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = true;
                cmiGoWarning.Text = "Goto Decompiler Warning";
                cmiHelp.Visible = true;
                cmiHelp.Text = "Help with this Decompiler Warning";
                break;
            case TODO:
                cmiDismiss.Visible = false;
                cmiDismissRes.Visible = false;
                cmiIgnoreWarning.Visible = false;
                cmiGoWarning.Enabled = true;
                cmiGoWarning.Text = "Goto TODO";
                cmiHelp.Visible = true;
                cmiHelp.Text = "TODO Help";
                break;
            }
        }

        private void cmiDismiss_Click(object sender, EventArgs e) {
            if (fgWarnings.SelectedRows.Count == 1) {
                var rowView = fgWarnings.SelectedRows[0].DataBoundItem as DataRowView;
                DataRow dataRow = rowView.Row;
                int tableIndex = infoGridTable.Rows.IndexOf(dataRow);
                DismissInfoItem(tableIndex);
            }
        }

        private void cmiDismissAll_Click(object sender, EventArgs e) {
            // delete everything except TODO entries
            DismissSourceWarnings();
        }

        private void cmiDismissRes_Click(object sender, EventArgs e) {
            // delete all warnings for the specified resource
            if (fgWarnings.SelectedRows.Count == 1) {
                var rowView = fgWarnings.SelectedRows[0].DataBoundItem as DataRowView;
                DataRow dataRow = rowView.Row;
                int tableIndex = infoGridTable.Rows.IndexOf(dataRow);
                DismissResourceWarnings(tableIndex);
            }
        }

        private void cmiIgnoreWarning_Click(object sender, EventArgs e) {
            // only if current row is a warning (>5000 and <6000)
            int warnnum = int.Parse((string)fgWarnings.SelectedRows[0].Cells[2].Value);
            if (warnnum > 5000 && warnnum < 6000) {
                IgnoreWarning(warnnum);
            }
        }

        private void cmiGoWarning_Click(object sender, EventArgs e) {
            DataGridViewRow row = fgWarnings.SelectedRows[0];
            if (!Enum.TryParse((string)row.Cells[0].Value, out EventType type)) {
                return;
            }
            if (!Enum.TryParse((string)row.Cells[1].Value, out AGIResType restype)) {
                return;
            }

            switch (type) {
            case LogicCompileError:
            case LogicCompileWarning:
            case DecompError:
            case DecompWarning:
            case TODO:
                int line = (int)row.Cells[5].Value;
                string msg = (string)row.Cells[3].Value;
                int lognum = (int)row.Cells[4].Value;
                string module = (string)row.Cells[7].Value;
                HighlightLine(line, msg, lognum, module, type);
                break;
            case ResourceError:
                // open the resource?
                break;
            case ResourceWarning:
                // open the resource
                int resnum = (int)row.Cells[4].Value;
                switch (restype) {
                case AGIResType.Logic:
                    OpenGameLogic((byte)resnum);
                    break;
                case AGIResType.Picture:
                    OpenGamePicture((byte)resnum);
                    break;
                case AGIResType.Sound:
                    OpenGameSound((byte)resnum);
                    break;
                case AGIResType.View:
                    OpenGameView((byte)resnum);
                    break;
                }
                break;
            case Info:
            case GameLoadError:
            case GameCompileError:
                // should not be possible
                break;
            default:
                // should not be possible
                break;
            }
        }

        private void cmiErrorHelp_Click(object sender, EventArgs e) {
            EventType type = Enum.Parse<EventType>((string)fgWarnings.SelectedRows[0].Cells[0].Value);
            HelpInfoItem(type, (string)fgWarnings.SelectedRows[0].Cells[2].Value);
        }
        #endregion

        #region Toolbar Button Events
        private void btnCompileTo_DropDownOpening(object sender, EventArgs e) {
            // force dropdown width to just show the buttons
            btnCompileTo.DropDown.AutoSize = false;
            btnCompileTo.DropDown.DropShadowEnabled = false;
            btnCompileTo.DropDown.Width = 0;
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
            if (EditGame is not null) {
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

        private void btnObjects_Click(object sender, EventArgs e) {
            if (EditGame is not null) {
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
            if (ActiveMdiChild is null) {
                mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmPreview) {
                mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmLogicEdit logicForm) {
                logicForm.mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmPicEdit picForm) {
                picForm.mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmSoundEdit soundForm) {
                soundForm.mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmViewEdit viewForm) {
                viewForm.mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmObjectEdit objectForm) {
                objectForm.mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmWordsEdit wordsForm) {
                wordsForm.mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmGlobals globalsForm) {
                globalsForm.mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmLayout layoutForm) {
                layoutForm.mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmMenuEdit menuForm) {
                menuForm.mnuUpdateLogic_Click(sender, e);
            }
            else if (ActiveMdiChild is frmTextScreenEdit txtscreenForm) {
                txtscreenForm.mnuRSave_Click(sender, e);
            }
            else if (ActiveMdiChild is frmFind) {
                Debug.Assert(false);
            }
            else {
                Debug.Assert(false);
            }
        }

        private void btnAddRemove_Click(object sender, EventArgs e) {
            if (ActiveMdiChild is null) {
                mnuRRemove_Click(sender, e);
            }
            else if (ActiveMdiChild is frmPreview) {
                mnuRRemove_Click(sender, e);
            }
            else if (ActiveMdiChild is frmLogicEdit logicForm) {
                logicForm.mnuRInGame_Click(sender, e);
            }
            else if (ActiveMdiChild is frmPicEdit picForm) {
                picForm.mnuRInGame_Click(sender, e);
            }
            else if (ActiveMdiChild is frmSoundEdit soundForm) {
                soundForm.mnuRInGame_Click(sender, e);
            }
            else if (ActiveMdiChild is frmViewEdit viewForm) {
                viewForm.mnuRInGame_Click(sender, e);
            }
            else if (ActiveMdiChild is frmObjectEdit) {
                Debug.Assert(false);
            }
            else if (ActiveMdiChild is frmWordsEdit) {
                Debug.Assert(false);
            }
            else if (ActiveMdiChild is frmGlobals globalsForm) {
                globalsForm.mnuRInGame_Click(sender, e);
            }
            else if (ActiveMdiChild is frmLayout) {
                Debug.Assert(false);
            }
            else if (ActiveMdiChild is frmMenuEdit) {
                Debug.Assert(false);
            }
            else if (ActiveMdiChild is frmTextScreenEdit) {
                Debug.Assert(false);
            }
            else if (ActiveMdiChild is frmFind) {
                Debug.Assert(false);
            }
            else {
                Debug.Assert(false);
            }
        }

        private void btnExportRes_Click(object sender, EventArgs e) {
            if (ActiveMdiChild is null) {
                mnuRExport_Click(sender, e);
            }
            else if (ActiveMdiChild is frmPreview) {
                mnuRExport_Click(sender, e);
            }
            else if (ActiveMdiChild is frmLogicEdit logicForm) {
                logicForm.mnuRExport_Click(sender, e);
            }
            else if (ActiveMdiChild is frmPicEdit picForm) {
                picForm.mnuRExport_Click(sender, e);
            }
            else if (ActiveMdiChild is frmSoundEdit soundForm) {
                soundForm.mnuRExport_Click(sender, e);
            }
            else if (ActiveMdiChild is frmViewEdit viewForm) {
                viewForm.mnuRExport_Click(sender, e);
            }
            else if (ActiveMdiChild is frmObjectEdit objectForm) {
                objectForm.mnuRExport_Click(sender, e);
            }
            else if (ActiveMdiChild is frmWordsEdit wordsForm) {
                wordsForm.mnuRExport_Click(sender, e);
            }
            else if (ActiveMdiChild is frmGlobals globalsForm) {
                globalsForm.mnuRSaveAs_Click(sender, e);
            }
            else if (ActiveMdiChild is frmLayout) {
                Debug.Assert(false);
            }
            else if (ActiveMdiChild is frmMenuEdit) {
                Debug.Assert(false);
            }
            else if (ActiveMdiChild is frmTextScreenEdit txtscreenForm) {
                txtscreenForm.mnuRSaveAs_Click(sender, e);
            }
            else if (ActiveMdiChild is frmFind) {
                Debug.Assert(false);
            }
            else {
                Debug.Assert(false);
            }
        }
        #endregion

        #region AGI Game Events
        internal void GameEvents_CompileGameStatus(object sender, CompileGameEventArgs e) {
            // check for a cancel
            if (CompStatusWin.CompCanceled) {
                e.Cancel = true;
                return;
            }
            // pass event to background task (can't make changes to 
            // UI from here)
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
                case InfoType.DecodingAllLogics:
                    if (e.LoadInfo.ResType == AGIResType.None) {
                        bgwOpenGame.ReportProgress(0, "Decoding all logic resources ...");
                    }
                    else {
                        bgwOpenGame.ReportProgress(0, "Decoding logic " + e.LoadInfo.ResNum.ToString());
                    }
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
            switch (e.CompInfo.Type) {
            case LogicCompileError:
                // error
                MDIMain.AddInfoItem(e.CompInfo, true);
                break;
            case LogicCompileWarning:
                // warning
                CompWarnings = true;
                MDIMain.AddInfoItem(e.CompInfo, true);
                break;
            case Info:
                // clear warnings for an include file
                MDIMain.ClearInfoGrid(e.CompInfo.Filename);
                break;
            default:
                Debug.Assert(false);
                break;
                //case Info:
                //    switch (e.CompInfo.InfoType) {
                //    case InfoType.Initialize:
                //        break;
                //    case InfoType.Validating:
                //        break;
                //    case InfoType.PropertyFile:
                //        break;
                //    case InfoType.Resources:
                //        break;
                //    case InfoType.Decompiling:
                //        break;
                //    case InfoType.CheckCRC:
                //        break;
                //    case InfoType.Finalizing:
                //        break;
                //    }
                //    break;
                //case ResourceWarning:
                //    break;
                //case DecompWarning:
                //    break;
                //case TODO:
                //    break;
            }
        }

        internal void GameEvents_DecodeLogicStatus(object sender, DecodeLogicEventArgs e) {
            if (bgwOpenGame.IsBusy) {
                if (e.DecodeInfo.InfoType == InfoType.ClearWarnings) {
                    // ignore clear code when decoding during gameload
                    return;
                }
                if (e.DecodeInfo.Type == EventType.TODO) {
                    bgwOpenGame?.ReportProgress(2, e.DecodeInfo);
                }
                else {
                    bgwOpenGame?.ReportProgress(3, e.DecodeInfo);
                }
            }
            else {
                // check for clear
                if (e.DecodeInfo.InfoType == InfoType.ClearWarnings) {
                    ClearInfoGrid(AGIResType.Logic, e.DecodeInfo.ResNum);
                }
                else {
                    // add it directly using AddWarning function
                    AddInfoItem(e.DecodeInfo, true);
                }
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Resource List Methods
        public void ShowResTree() {
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.None:
                // no tree
                // shouldn't get here, but
                return;

            case ResListType.TreeList:
                tvwResources.Visible = true;
                cmbResType.Visible = false;
                lstResources.Visible = false;
                // change font to match current preview font
                tvwResources.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
                break;
            case ResListType.ComboList:
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
            tvwResources.Nodes[0].Nodes.Add("Includes", "Includes");
            tvwResources.Nodes[0].Expand();
            // save reference to nodes for ease of acess
            RootNode = tvwResources.Nodes[0];
            HdrNode = new TreeNode[7];
            for (int i = 0; i < 7; i++) {
                HdrNode[i] = RootNode.Nodes[i];
            }
        }

        public void ClearResourceList() {
            // reset the navigation queue and don't add to queue while clearing
            ResetQueue();
            DontQueue = true;
            // list type determines clear actions
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                ResetTreeList();
                break;
            case ResListType.ComboList:
                cmbResType.Items[0] = "AGIGame";
                lstResources.Items.Clear();
                break;
            }
            SelResType = Game;
            SelResNum = -1;
            // reenable queuing
            DontQueue = false;
        }

        public void BuildResourceTree() {
            // builds the resource tree list
            // for the current open game
            TreeNode tmpNode;

            switch (WinAGISettings.ResListType.Value) {
            case ResListType.None:
                return;
            case ResListType.TreeList:
                // remove existing resources
                HdrNode[0].Nodes.Clear();
                HdrNode[1].Nodes.Clear();
                HdrNode[2].Nodes.Clear();
                HdrNode[3].Nodes.Clear();
                HdrNode[6].Nodes.Clear();
                Debug.Assert(EditGame.GameID.Length != 0);
                // update root
                tvwResources.Nodes[0].Text = EditGame.GameID;
                // add logics
                if (EditGame.Logics.Count > 0) {
                    for (int i = 0; i <= 255; i++) {
                        // if a valid resource
                        if (EditGame.Logics.Contains(i)) {
                            tmpNode = tvwResources.Nodes[0].Nodes[sLOGICS].Nodes.Add("l" + i, ResourceName(EditGame.Logics[i], true));
                            tmpNode.Tag = i;
                            // get compiled status
                            if (EditGame.Logics[i].Compiled && EditGame.Logics[i].Error == ResourceErrorType.NoError) {
                                tmpNode.ForeColor = Color.Black;
                            }
                            else {
                                tmpNode.ForeColor = Color.Red;
                            }
                        }
                    }
                }
                if (EditGame.Pictures.Count > 0) {
                    for (int i = 0; i <= 255; i++) {
                        // if a valid resource
                        if (EditGame.Pictures.Contains(i)) {
                            tmpNode = tvwResources.Nodes[0].Nodes[sPICTURES].Nodes.Add("p" + i, ResourceName(EditGame.Pictures[i], true));
                            tmpNode.Tag = i;
                            tmpNode.ForeColor = EditGame.Pictures[i].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                        }
                    }
                }
                if (EditGame.Sounds.Count > 0) {
                    for (int i = 0; i <= 255; i++) {
                        // if a valid resource
                        if (EditGame.Sounds.Contains(i)) {
                            tmpNode = tvwResources.Nodes[0].Nodes[sSOUNDS].Nodes.Add("s" + i, ResourceName(EditGame.Sounds[i], true));
                            tmpNode.Tag = i;
                            tmpNode.ForeColor = EditGame.Sounds[i].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                        }
                    }
                }
                if (EditGame.Views.Count > 0) {
                    for (int i = 0; i <= 255; i++) {
                        // if a valid resource
                        if (EditGame.Views.Contains(i)) {
                            tmpNode = tvwResources.Nodes[0].Nodes[sVIEWS].Nodes.Add("v" + i, ResourceName(EditGame.Views[i], true));
                            tmpNode.Tag = i;
                            tmpNode.ForeColor = EditGame.Views[i].Error == ResourceErrorType.NoError ? Color.Black : Color.Red;
                        }
                    }
                }
                if (EditGame.IncludeFiles.Count > 0) {
                    for (int i = 0; i < EditGame.IncludeFiles.Count; i++) {
                        tmpNode = HdrNode[6].Nodes.Add("i" + i, Path.GetFileName(EditGame.IncludeFiles[i].Filename));
                        if (EditGame.IncludeIDs &&
                            EditGame.IncludeFiles[i].Filename.Equals(
                                Path.Combine(EditGame.SrcResDir, "resourceids.txt"), StringComparison.OrdinalIgnoreCase)) {
                            tmpNode.ForeColor = Color.Blue;
                        }
                        if (EditGame.IncludeReserved &&
                            EditGame.IncludeFiles[i].Filename.Equals(
                                Path.Combine(EditGame.SrcResDir, "reserved.txt"), StringComparison.OrdinalIgnoreCase)) {
                            tmpNode.ForeColor = Color.Blue;
                        }
                        if (EditGame.IncludeGlobals &&
                            EditGame.IncludeFiles[i].Filename.Equals(
                                Path.Combine(EditGame.SrcResDir, "globals.txt"), StringComparison.OrdinalIgnoreCase)) {
                            tmpNode.ForeColor = Color.Blue;
                        }
                        tmpNode.Tag = i;
                    }
                }
                break;
            case ResListType.ComboList:
                // update root
                cmbResType.Items[0] = EditGame.GameID;
                // select root
                cmbResType.SelectedIndex = 0;
                break;
            }
            return;
        }

        public void AddResourceToList(AGIResType restype, int resnum) {
            int pos;
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                TreeNode tmpNode = HdrNode[(int)restype];
                // find place to insert this resource
                for (pos = 0; pos < HdrNode[(int)restype].Nodes.Count; pos++) {
                    if ((int)tmpNode.Nodes[pos].Tag > resnum) {
                        break;
                    }
                }
                // add to tree
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
            case ResListType.ComboList:
                // only update if logics are being listed
                if (cmbResType.SelectedIndex - 1 == (int)restype) {
                    ListViewItem tmpListItem = null;
                    // find a place to insert this resource in the box list
                    for (pos = 0; pos < lstResources.Items.Count; pos++) {
                        if ((int)lstResources.Items[pos].Tag > resnum) {
                            break;
                        }
                    }
                    switch (restype) {
                    case AGIResType.Logic:
                        tmpListItem = lstResources.Items.Insert(pos, "l" + resnum, ResourceName(EditGame.Logics[resnum], true), 0);
                        // set source compiled status
                        tmpListItem.ForeColor = EditGame.Logics[resnum].Compiled ? Color.Black : Color.Red;
                        break;
                    case AGIResType.Picture:
                        tmpListItem = lstResources.Items.Insert(pos, "p" + resnum, ResourceName(EditGame.Pictures[resnum], true), 0);
                        break;
                    case AGIResType.Sound:
                        tmpListItem = lstResources.Items.Insert(pos, "s" + resnum, ResourceName(EditGame.Sounds[resnum], true), 0);
                        break;
                    case AGIResType.View:
                        tmpListItem = lstResources.Items.Insert(pos, "v" + resnum, ResourceName(EditGame.Views[resnum], true), 0);
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
            LogicListChange();
            // last node marker is no longer accurate; reset
            MDIMain.LastNodeName = "";
        }

        /// <summary>
        /// Updates the include list in the wag file, and updates the resource list.
        /// Called when includes are added/removed.
        /// </summary>
        public void RefreshIncludeList() {
            // rebuild include file list in tree (because index numbers won't always align
            // after making changes)

            // update the wag file property
            string includelist = "";
            for (int i = 0; i < EditGame.IncludeFiles.Count; i++) {
                if (i > 0)
                    includelist += ",";
                includelist += RelativeToSrcDir(EditGame.IncludeFiles[i].Filename);
            }
            EditGame.WriteGameSetting("Includes", "FileList", includelist, "", true);

            // then update the resource list if includes are shown in it
            switch (WinAGISettings.ResListType.Value) {
            case ResListType.TreeList:
                tvwResources.BeginUpdate();
                HdrNode[6].Nodes.Clear();
                for (int i = 0; i < EditGame.IncludeFiles.Count; i++) {
                    var tmpNode = HdrNode[6].Nodes.Add("i" + i, Path.GetFileName(EditGame.IncludeFiles[i].Filename));
                    if (EditGame.IncludeIDs &&
                        EditGame.IncludeFiles[i].Filename.Equals(
                            Path.Combine(EditGame.SrcResDir, "resourceids.txt"), StringComparison.OrdinalIgnoreCase)) {
                        tmpNode.ForeColor = Color.Blue;
                    }
                    if (EditGame.IncludeReserved &&
                        EditGame.IncludeFiles[i].Filename.Equals(
                            Path.Combine(EditGame.SrcResDir, "reserved.txt"), StringComparison.OrdinalIgnoreCase)) {
                        tmpNode.ForeColor = Color.Blue;
                    }
                    if (EditGame.IncludeGlobals &&
                        EditGame.IncludeFiles[i].Filename.Equals(
                            Path.Combine(EditGame.SrcResDir, "globals.txt"), StringComparison.OrdinalIgnoreCase)) {
                        tmpNode.ForeColor = Color.Blue;
                    }
                    tmpNode.Tag = i;
                }
                tvwResources.EndUpdate();
                break;
            case ResListType.ComboList:
                if (SelResType == Include) {
                    lstResources.BeginUpdate();
                    lstResources.Items.Clear();
                    for (int i = 0; i < EditGame.IncludeFiles.Count; i++) {
                        var tmpItem = lstResources.Items.Add("i" + i, Path.GetFileName(EditGame.IncludeFiles[i].Filename), 0);
                        tmpItem.Tag = i;
                    }
                    lstResources.EndUpdate();
                }
                break;
            }
            if (SelResType == Include) {
                // always select the header because index numbers may not
                // be aligned anymore
                MDIMain.SelectResource(Include, -1, true);
            }
            // force lookup refresh
            foreach (var form in LogicEditors) {
                form.DefChanged = true;
                form.ListChanged = true;
            }
        }

        private void SelectFromQueue() {
            // selects the node/resource from the current queue position
            //
            // if the current resource is gone (deleted)
            // the function will select the appropriate header
            //
            // while selecting from queue, disable additions to the
            // queue

            string key = "";
            // make sure queue has something
            if (ResQPtr < 0) {
                return;
            }
            // disable queue addition
            DontQueue = true;

            if (ResQueue[ResQPtr].ResNum == 256) {
                // header
                switch (WinAGISettings.ResListType.Value) {
                case ResListType.TreeList:
                    // treelist
                    if (ResQueue[ResQPtr].ResType == Game) {
                        tvwResources.SelectedNode = RootNode;
                    }
                    else {
                        tvwResources.SelectedNode = HdrNode[(int)ResQueue[ResQPtr].ResType];
                    }
                    break;
                case ResListType.ComboList:
                    // listbox
                    switch (ResQueue[ResQPtr].ResType) {
                    case Game: // root
                        cmbResType.SelectedIndex = 0;
                        // then force selection change
                        SelectResource(Game, -1);
                        break;
                    default:
                        // (restype+1 matches desired listindex)
                        cmbResType.SelectedIndex = (int)(ResQueue[ResQPtr].ResType + 1);
                        // reset the listbox
                        lstResources.SelectedItems.Clear();
                        // then force selection change
                        SelectResource(ResQueue[ResQPtr].ResType, -1);
                        break;
                    }
                    break;
                }
            }
            else {
                // does the resource still exist?
                switch (ResQueue[ResQPtr].ResType) {
                case AGIResType.Logic:
                    if (EditGame.Logics.Contains(ResQueue[ResQPtr].ResNum)) {
                        key = "l" + ResQueue[ResQPtr].ResNum;
                    }
                    break;
                case AGIResType.Picture:
                    if (EditGame.Pictures.Contains(ResQueue[ResQPtr].ResNum)) {
                        key = "p" + ResQueue[ResQPtr].ResNum;
                    }
                    break;
                case AGIResType.Sound:
                    if (EditGame.Sounds.Contains(ResQueue[ResQPtr].ResNum)) {
                        key = "s" + ResQueue[ResQPtr].ResNum;
                    }
                    break;
                case AGIResType.View:
                    if (EditGame.Views.Contains(ResQueue[ResQPtr].ResNum)) {
                        key = "v" + ResQueue[ResQPtr].ResNum;
                    }
                    break;
                case Include:
                    if (ResQueue[ResQPtr].ResNum < EditGame.IncludeFiles.Count) {
                        key = "i" + ResQueue[ResQPtr].ResNum;
                    }
                    break;
                }

                // if no key
                if (key.Length == 0) {
                    // this resource doesn't exist anymore - probably
                    // deleted; select the header
                    switch (WinAGISettings.ResListType.Value) {
                    case ResListType.TreeList:
                        // treelist
                        tvwResources.SelectedNode = HdrNode[(int)ResQueue[ResQPtr].ResType];
                        break;
                    case ResListType.ComboList:
                        // (restype+1 matches desired combobox index)
                        cmbResType.SelectedIndex = (int)(ResQueue[ResQPtr].ResType + 1);
                        // reset the listbox
                        lstResources.SelectedItems.Clear();
                        // then force selection change
                        SelectResource(ResQueue[ResQPtr].ResType, -1);
                        break;
                    }
                    return;
                }
                // select this resource
                switch (WinAGISettings.ResListType.Value) {
                case ResListType.TreeList:
                    tvwResources.SelectedNode = HdrNode[(int)ResQueue[ResQPtr].ResType].Nodes[key];
                    break;
                case ResListType.ComboList:
                    // (restype+1 matches desired combobox index)
                    cmbResType.SelectedIndex = (int)(ResQueue[ResQPtr].ResType + 1);
                    // now select the resource
                    lstResources.Items[key].Selected = true;
                    break;
                }
                // force selection
                SelectResource(ResQueue[ResQPtr].ResType, ResQueue[ResQPtr].ResNum);
            }

            // restore queue addition
            DontQueue = false;
            return;
        }
        #endregion

        #region Status Strip Methods
        private void ResetStatusStrip() {
            spStatus.Text = "";
            for (int i = statusStrip1.Items.Count - 1; i >= 0; i--) {
                var item = statusStrip1.Items[i];
                if (item != spStatus && item != spCapsLock && item != spNumLock && item != spInsLock) {
                    statusStrip1.Items.Remove(item);
                }
            }
        }

        private void MergeStatusStrip(Form form) {
            switch (form) {
            case frmGlobals:
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
                // status
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
            case frmMenuEdit:
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
            case frmPreview:
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
                // status
                statusStrip1.Items.Insert(2, frmTE.spRow);
                statusStrip1.Items.Insert(3, frmTE.spCol);
                spCapsLock.Visible = true;
                spNumLock.Visible = true;
                spInsLock.Visible = true;
                break;
            case frmViewEdit frmVE:
                statusStrip1.Items.Insert(0, frmVE.spScale);
                statusStrip1.Items.Insert(1, frmVE.spTool);
                // status
                // status
                statusStrip1.Items.Insert(3, frmVE.spCurX);
                statusStrip1.Items.Insert(4, frmVE.spCurY);
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

        internal void FlashStatus() {
            StatusFlashCount = 0;
            statusFlashTimer.Enabled = true;
        }
        #endregion

        #region InfoGrid Methods
        internal void SetupInfoGrid() {
            fgWarnings.Columns.Clear();
            fgWarnings.DataSource = infoGridBinding;

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
        }

        public void AddInfoItem(WinAGIEventInfo infoItem, bool noUpdate = false) {
            // seven types of warnings/errors get added to list
            // - resource errors
            // - resource warnings
            // - logic compile errors
            // - logic compile warnings
            // - logic decompile errors
            // - logic decompile warnings
            // - TODO entries
            switch (infoItem.Type) {
            case Info:
                // ignore decompiling
                switch (infoItem.InfoType) {
                case InfoType.Decompiled:
                case InfoType.Decompiling:
                    break;
                default:
                    Debug.Assert(false);
                    break;
                }
                break;
            case GameLoadError:
            case GameCompileError:
                Debug.Assert(false);
                break;
            case ResourceError:
            case ResourceWarning:
            case LogicCompileError:
            case LogicCompileWarning:
            case DecompError:
            case DecompWarning:
            case TODO:
                // check for decompile renumber
                if (infoItem.Type == DecompWarning && infoItem.ID == "renumber") {
                    int offset = infoItem.Line;
                    RenumberInfoItems(infoItem.ResNum, offset);
                    return;
                }
                infoGridTable.Rows.Add(infoItem.Type,
                             infoItem.ResType,
                             infoItem.ID,
                             infoItem.Text,
                             infoItem.ResType <= AGIResType.View ? infoItem.ResNum : -1,
                             infoItem.Line,
                             infoItem.Module,
                             infoItem.Filename);
                if (!pnlInfoGrid.Visible) {
                    if (WinAGISettings.AutoWarn.Value) {
                        if (!bgwOpenGame.IsBusy) {
                            ShowInfoGrid();
                        }
                    }
                }
                if (!noUpdate) {
                    UpdateGridCounts();
                }
                break;
            }
        }

        public void HideInfoGrid(bool clearlist = false) {
            if (clearlist) {
                ClearInfoGrid();
            }
            pnlInfoGrid.Visible = false;
            splitInfoGrid.Visible = false;
        }

        internal void ShowInfoGrid() {
            splitInfoGrid.Visible = true;
            pnlInfoGrid.Visible = true;
        }

        internal void RefreshInfoGrid() {
            // adjust grid filter based on options
            string filter = "";
            if (!gridErrors) {
                filter = "Type <> 'ResourceError' AND Type <> 'LogicCompileError' AND Type <> 'DecompError'";
            }
            if (!gridWarnings) {
                if (filter.Length > 0) {
                    filter += " AND ";
                }
                filter += "Type <> 'ResourceWarning' AND Type <> 'LogicCompileWarning' AND Type <> 'DecompWarning'";
            }
            if (!gridTODOs) {
                if (filter.Length > 0) {
                    filter += " AND ";
                }
                filter += "Type <> 'TODO'";
            }
            switch (infoGridScope) {
            case InfoGridScope.EntireProject:
                // no additional filter
                break;
            case InfoGridScope.AllLogics:
                if (filter.Length > 0) {
                    filter += " AND ";
                }
                filter += $"ResType = '{AGIResType.Logic}' OR ResType = 'Include'";
                break;
            case InfoGridScope.SelectedResource:
                AGIResType resType;
                int resNum;
                if (filter.Length > 0) {
                    filter += " AND ";
                }
                if (MDIMain.ActiveMdiChild is frmLogicEdit) {
                    frmLogicEdit frm = MDIMain.ActiveMdiChild as frmLogicEdit;
                    if (frm.FormMode == LogicFormMode.Logic) {
                        resType = AGIResType.Logic;
                        resNum = frm.LogicNumber;
                    }
                    else {
                        // includes are filtered by filename
                        filter += $"ResType = 'Include' AND Filename = '{frm.TextFilename}'";
                        break;
                    }
                }
                else if (MDIMain.ActiveMdiChild is frmPicEdit) {
                    resType = AGIResType.Picture;
                    resNum = (MDIMain.ActiveMdiChild as frmPicEdit).PictureNumber;
                }
                else if (MDIMain.ActiveMdiChild is frmSoundEdit) {
                    resType = AGIResType.Sound;
                    resNum = (MDIMain.ActiveMdiChild as frmSoundEdit).SoundNumber;
                }
                else if (MDIMain.ActiveMdiChild is frmViewEdit) {
                    resType = AGIResType.View;
                    resNum = (MDIMain.ActiveMdiChild as frmViewEdit).ViewNumber;
                }
                else if (OEInUse) {
                    resType = Words;
                    resNum = -1;
                }
                else if (WEInUse) {
                    resType = Objects;
                    resNum = -1;
                }
                else {
                    // use selected resource in resource pane
                    if (SelResType == Include) {
                        // includes are filtered by filename
                        filter += $"ResType = 'Include'";
                        if (SelResNum >= 0) {
                            filter += $" AND Filename = '{EditGame.IncludeFiles[SelResNum].Filename}'";
                        }
                        break;
                    }
                    else {
                        resType = SelResType;
                        resNum = SelResNum;
                    }
                }
                filter += $"ResType = '{resType}' AND ResNum = {resNum}";
                break;
            case InfoGridScope.OpenResources:
                // for each open resource editor add each restype/number combo to filter
                string filterPart = "";
                foreach (var frm in MdiChildren) {
                    if (frm is frmLogicEdit logicform) {
                        if (logicform.InGame) {
                            if (logicform.FormMode == LogicFormMode.Logic) {
                                if (filterPart.Length > 0) {
                                    filterPart += " OR ";
                                }
                                filterPart += $"(ResType = '{AGIResType.Logic}' AND ResNum = {logicform.LogicNumber})";
                            }
                            else {
                                if (filterPart.Length > 0) {
                                    filterPart += " OR ";
                                }
                                // includes are filtered by filename
                                filterPart += $"ResType = 'Include' AND Filename = '{logicform.TextFilename}'";
                            }
                        }
                    }
                    else if (frm is frmPicEdit picform) {
                        if (picform.InGame) {
                            if (filterPart.Length > 0) {
                                filterPart += " OR ";
                            }
                            filterPart += $"(ResType = '{AGIResType.Picture}' AND ResNum = {picform.PictureNumber})";
                        }
                    }
                    else if (frm is frmSoundEdit soundform) {
                        if (soundform.InGame) {
                            if (filterPart.Length > 0) {
                                filterPart += " OR ";
                            }
                            filterPart += $"(ResType = '{AGIResType.Sound}' AND ResNum = {soundform.SoundNumber})";
                        }
                    }
                    else if (frm is frmViewEdit viewform) {
                        if (viewform.InGame) {
                            if (filterPart.Length > 0) {
                                filterPart += " OR ";
                            }
                            filterPart += $"(ResType = '{AGIResType.View}' AND ResNum = {viewform.ViewNumber})";
                        }
                    }
                }
                if (WEInUse) {
                    if (filterPart.Length > 0) {
                        filterPart += " OR ";
                    }
                    filterPart += $"ResType = '{Words}'";
                }
                if (OEInUse) {
                    if (filterPart.Length > 0) {
                        filterPart += " OR ";
                    }
                    filterPart += $"ResType = '{Objects}'";
                }
                if (filter.Length > 0) {
                    filter += " AND ";
                }
                if (filterPart.Length > 0) {
                    filter += "(" + filterPart + ")";
                }
                else {
                    filter += "ResType = ''";
                }
                break;
            }
            if (infoGridBinding.Filter != filter) {
                infoGridBinding.Filter = filter;
            }
            UpdateGridCounts();
        }

        internal void UpdateGridCounts() {
            // update the info grid labels with counts based on current filter
            int totalErrors = 0;
            int totalWarnings = 0;
            int totalTODOs = 0;
            int shownErrors = 0;
            int shownWarnings = 0;
            int shownTODOs = 0;
            foreach (DataRow fgRow in infoGridTable.Rows) {
                switch (fgRow.Field<string>(0)) {
                case "ResourceError":
                case "LogicCompileError":
                case "DecompError":
                    totalErrors++;
                    break;
                case "ResourceWarning":
                case "LogicCompileWarning":
                case "DecompWarning":
                    totalWarnings++;
                    break;
                case "TODO":
                    totalTODOs++;
                    break;
                }
            }
            foreach (DataRowView fgRowView in infoGridBinding) {
                var fgRow = fgRowView.Row;
                switch (fgRow.Field<string>(0)) {
                case "ResourceError":
                case "LogicCompileError":
                case "DecompError":
                    shownErrors++;
                    break;
                case "ResourceWarning":
                case "LogicCompileWarning":
                case "DecompWarning":
                    shownWarnings++;
                    break;
                case "TODO":
                    shownTODOs++;
                    break;
                }
            }
            if (shownErrors == totalErrors) {
                errorToggle.Text = $"       {totalErrors} Errors";
            }
            else {
                errorToggle.Text = $"       {shownErrors} of {totalErrors} Errors";
            }
            if (shownWarnings == totalWarnings) {
                warningToggle.Text = $"       {totalWarnings} Warnings";
            }
            else {
                warningToggle.Text = $"       {shownWarnings} of {totalWarnings} Warnings";
            }
            if (shownTODOs == totalTODOs) {
                todoToggle.Text = $"       {totalTODOs} TODOs";
            }
            else {
                todoToggle.Text = $"       {shownTODOs} of {totalTODOs} TODOs";
            }
            // reposition/resize
            Font font = errorToggle.Font;
            errorToggle.Width = TextRenderer.MeasureText(errorToggle.Text, font).Width + 8;
            warningToggle.Left = errorToggle.Left + errorToggle.Width + 15;
            warningToggle.Width = TextRenderer.MeasureText(warningToggle.Text, font).Width + 8;
            todoToggle.Left = warningToggle.Left + warningToggle.Width + 15;
            todoToggle.Width = TextRenderer.MeasureText(todoToggle.Text, font).Width + 8;
        }

        /// <summary>
        /// Deletes the infoitem at the specified row.
        /// </summary>
        /// <param name="row"></param>
        private void DismissInfoItem(int row) {
            AGIResType restype = (AGIResType)Enum.Parse(typeof(AGIResType), infoGridTable.Rows[row].Field<string>(1));
            int resnum = infoGridTable.Rows[row].Field<int>(4);
            infoGridTable.Rows.RemoveAt(row);
            UpdateGridCounts();

            switch (restype) {
            case AGIResType.Logic:
                // update the game property file.
                Debug.Assert(resnum != -1);
                RefreshLogicWarnings(resnum);
                break;
            case Include:
                RefreshModWarnings();
                // include file
                break;
            default:
                break;
            }
        }

        /// <summary>
        /// Dismisses all rows of type LogicCompileWarning and DecompWarning
        /// in the info grid. LogicCompileError, ResourceWarning, ResourceError
        /// and TODO enties are kept.
        /// </summary>
        private void DismissSourceWarnings() {
            List<int> logics = [];
            // pause warnings list updates
            infoGridBinding.RaiseListChangedEvents = false;

            for (int i = infoGridTable.Rows.Count - 1; i >= 0; i--) {
                var fgRow = infoGridTable.Rows[i];
                if (fgRow.Field<string>(0) == LogicCompileWarning.ToString() ||
                    fgRow.Field<string>(0) == DecompWarning.ToString()) {
                    byte resnum = (byte)fgRow.Field<int>(4);
                    if (!logics.Contains(resnum)) {
                        logics.Add(resnum);
                    }
                    infoGridTable.Rows.RemoveAt(i);
                }
            }
            foreach (var logic in logics) {
                if (logic < 0) {
                    RefreshModWarnings();
                }
                else {
                    RefreshLogicWarnings(logic, false);
                }
            }
            RefreshModWarnings(false);
            EditGame.SaveProperties();
            // refresh the warnings list
            infoGridBinding.RaiseListChangedEvents = true;
            infoGridBinding.ResetBindings(false);
            UpdateGridCounts();
        }

        /// <summary>
        /// Dismiss all warnings that match the resource for the specified by the row.
        /// </summary>
        /// <param name="row"></param>
        private void DismissResourceWarnings(int row) {
            AGIResType restype = (AGIResType)Enum.Parse(typeof(AGIResType), infoGridTable.Rows[row].Field<string>(1));
            int resnum = infoGridTable.Rows[row].Field<int>(4);
            switch (restype) {
            case AGIResType.Logic:
            case AGIResType.Picture:
            case AGIResType.Sound:
            case AGIResType.View:
                ClearInfoGrid(restype, resnum);
                // update the game property file.
                Debug.Assert(resnum != -1);
                RefreshLogicWarnings(resnum);
                break;
            case AGIResType.Include:
                // find the matching lines (by restype/filename)
                string resname = (string)fgWarnings.SelectedRows[0].Cells[6].Value;
                for (int i = infoGridTable.Rows.Count - 1; i >= 0; i--) {
                    var fgRow = infoGridTable.Rows[i];
                    if (fgRow.Field<string>(1) == "Text" &&
                        fgRow.Field<string>(6) == resname) {
                        infoGridTable.Rows.RemoveAt(i);
                    }
                }
                UpdateGridCounts();
                RefreshModWarnings();
                // include file
                break;
            default:
                break;
            }
        }

        /// <summary>
        /// Clears entire info grid. Only used when closing a game or opening a new game.
        /// </summary>
        private static void ClearInfoGrid() {
            infoGridTable.Clear();
        }

        /// <summary>
        /// Clears info grid entries for the resource specified by type, number and event type.
        /// </summary>
        /// <param name="resnum"></param>
        /// <param name="restype"></param>
        public void ClearInfoGrid(AGIResType restype, int resnum, EventType eventtype) {
            ClearInfoGrid(restype, resnum, [eventtype]);
        }

        /// <summary>
        /// Clears info grid entries for the resource specified by type, number that 
        /// match one of the specified event types.
        /// </summary>
        /// <param name="restype"></param>
        /// <param name="resnum"></param>
        /// <param name="eventtypes"></param>
        public void ClearInfoGrid(AGIResType restype, int resnum, EventType[] eventtypes) {
            // find the matching lines (by restype/number/eventtype)
            for (int i = infoGridTable.Rows.Count - 1; i >= 0; i--) {
                var fgRow = infoGridTable.Rows[i];
                if (fgRow.Field<string>(1) == restype.ToString() &&
                    fgRow.Field<int>(4) == resnum) {
                    foreach (var eventtype in eventtypes) {
                        if (fgRow.Field<string>(0) == eventtype.ToString()) {
                            infoGridTable.Rows.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            if (!bgwCompGame.IsBusy) {
                UpdateGridCounts();
            }
        }

        /// <summary>
        /// Clears all info grid entries for the resource specified by type and number.
        /// </summary>
        /// <param name="resnum"></param>
        /// <param name="restype"></param>
        public void ClearInfoGrid(AGIResType restype, int resnum) {
            Base.ClearInfoGrid(restype, resnum);
            if (!bgwCompGame.IsBusy) {
                UpdateGridCounts();
            }
        }

        public void ClearInfoGrid(string includefile) {
            // find the matching lines (by filename)
            for (int i = infoGridTable.Rows.Count - 1; i >= 0; i--) {
                var fgRow = infoGridTable.Rows[i];
                if (fgRow.Field<string>(1) == "Include" &&
                    fgRow.Field<string>(7) == includefile) {
                    infoGridTable.Rows.RemoveAt(i);
                }
            }
            if (!bgwCompGame.IsBusy) {
                UpdateGridCounts();
            }
        }

        private static void HelpInfoItem(EventType type, string id) {
            // show help for the warning (or error) that is selected
            string topic = "";

            switch (type) {
            case ResourceError:
            case DecompError:
            case LogicCompileError:
                topic = @"htm\winagi\errors\" + id + ".htm";
                break;
            case ResourceWarning:
            case DecompWarning:
            case LogicCompileWarning:
                topic = @"htm\winagi\warnings\" + id + ".htm";
                break;
            case TODO:
                topic = @"htm\winagi\editor_logic.htm#TODO";
                break;
            }
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
        }

        private void IgnoreWarning(int warningNumber) {
            List<int> updatelogics = [];
            // ignore changes until all done
            infoGridBinding.RaiseListChangedEvents = false;
            MDIMain.UseWaitCursor = true;
            MDIMain.Refresh();
            SetIgnoreWarning(warningNumber, true);
            for (int i = infoGridTable.Rows.Count - 1; i >= 0; i--) {
                var fgRow = infoGridTable.Rows[i];
                if (fgRow.Field<string>(2) == warningNumber.ToString()) {
                    if (!updatelogics.Contains(fgRow.Field<int>(4))) {
                        updatelogics.Add(fgRow.Field<int>(4));
                    }
                    infoGridTable.Rows.RemoveAt(i);
                }
            }
            UpdateGridCounts();
            // update any logics affected
            foreach (int lognum in updatelogics) {
                if (lognum < 0) {
                    RefreshModWarnings();
                }
                else {
                    RefreshLogicWarnings(lognum, false);
                }
            }
            // refresh the grid
            infoGridBinding.RaiseListChangedEvents = true;
            infoGridBinding.ResetBindings(false);
            MDIMain.UseWaitCursor = false;
        }

        internal void ResetInfoGrid(byte logicnum, List<WinAGIEventInfo> list) {
            ClearInfoGrid(AGIResType.Logic, logicnum, [LogicCompileError, LogicCompileWarning]);
            foreach (var warnitem in list) {
                AddInfoItem(warnitem, true);
            }
            UpdateGridCounts();
        }

        private void HighlightLine(int errorLine, string errorMsg, int logicNumber, string module, EventType eventtype) {
            // this procedure uses warning/error/TODO info to open the file
            // with the desired entry
            // it highlights the target line and, if it is a warning or error,
            // it displays the warning/error message in the status bar
            frmLogicEdit frmTemp = null;

            if (module.Length != 0) {
                if (OpenTextFile(module, true)) {
                    for (int i = 0; i < LogicEditors.Count; i++) {
                        if (LogicEditors[i].FormMode == LogicFormMode.Text && LogicEditors[i].TextFilename == module) {
                            frmTemp = LogicEditors[i];
                            break;
                        }
                    }
                }
                else {
                    if (eventtype != LogicCompileError) {
                        return;
                    }
                    errorLine = 0;
                    errorMsg += " (in INCLUDE file)";
                }
            }
            else {
                for (int i = 0; i < LogicEditors.Count; i++) {
                    if (LogicEditors[i].FormMode == LogicFormMode.Logic && LogicEditors[i].LogicNumber == logicNumber) {
                        frmTemp = LogicEditors[i];
                        if (frmTemp.WindowState == FormWindowState.Minimized) {
                            frmTemp.WindowState = FormWindowState.Normal;
                        }
                        frmTemp.BringToFront();
                        frmTemp.Select();
                        break;
                    }
                }
                if (frmTemp is null) {
                    if (OpenGameLogic((byte)logicNumber, true)) {
                        for (int i = 0; i < LogicEditors.Count; i++) {
                            if (LogicEditors[i].FormMode == LogicFormMode.Logic && LogicEditors[i].LogicNumber == logicNumber) {
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
            if (frmTemp is null) {
                string msgboxtext = "";
                string msgboxtitle = "";
                switch (eventtype) {
                case LogicCompileError:
                    // error
                    msgboxtext = "ERROR in line " + errorLine + "- " + errorMsg;
                    msgboxtitle = "Compile Logic Error";
                    break;
                case LogicCompileWarning:
                    // warning
                    msgboxtext = "WARNING in line " + errorLine + ": " + errorMsg;
                    msgboxtitle = "Compile Logic Warning";
                    break;
                case TODO:
                    msgboxtext = "In " + EditGame.Logics[logicNumber].ID + " at line " + errorLine + ":" +
                           "\n\nTODO: " + errorMsg;
                    msgboxtitle = "TODO Item";
                    break;
                case DecompWarning:
                    // decomp warning
                    msgboxtext = "Decompiling warning in " + EditGame.Logics[logicNumber].ID + " at line " + errorLine + ":\n\n" +
                           errorMsg;
                    msgboxtitle = "TODO Item";
                    break;
                }
                MessageBox.Show(MDIMain,
                    msgboxtext, msgboxtitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            if (errorLine >= frmTemp.fctb.LinesCount) {
                errorLine = frmTemp.fctb.LinesCount - 1;
            }
            frmTemp.fctb.Selection.Start = new(0, errorLine);
            frmTemp.fctb.Selection.End = frmTemp.fctb.Selection.Start;
            frmTemp.fctb.DoSelectionVisible();
            frmTemp.BringToFront();
            frmTemp.Select();
            // if not a TODO, update the status bar as well
            if (eventtype != TODO) {
                string errorType;
                if (eventtype == LogicCompileError) {
                    errorType = "ERROR ";
                }
                else {
                    errorType = "WARNING ";
                }
                MainStatusBar.Items[nameof(spStatus)].Text = errorType + "in line " + errorLine + ": " + errorMsg;
            }
        }
        #endregion

        #region Property Grid Methods
        internal void RefreshPropertyGrid() {
            RefreshPropertyGrid(SelResType, SelResNum);
        }

        internal void RefreshPropertyGrid(AGIResType restype, int resnum) {
            // re-select the current property object

            if (WinAGISettings.ResListType.Value == ResListType.None) {
                return;
            }

            propertyGrid1.SelectedObject = null;
            switch (restype) {
            case None:
                return;
            case Game:
                // show gameid, gamedesigner, description,etc
                GameProperties pGame = new();
                propertyGrid1.SelectedObject = pGame;
                // layout is readonly in Sierra syntax mode
                Attribute readOnlyG = TypeDescriptor.GetProperties(pGame.GetType())["LayoutEditor"].Attributes[typeof(ReadOnlyAttribute)];
                FieldInfo fiG = readOnlyG.GetType().GetRuntimeFields().ToArray()[0];
                fiG?.SetValue(readOnlyG, EditGame.SierraSyntax);

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
                    // picture header
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
                    // view header
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
            case Include:
                if (resnum == -1) {
                    // include header
                    IncludeHdrProperties pIncHdr = new();
                    // set read-only status of some properties based on Sierra syntax
                    SetReadOnly(pIncHdr, "IncludeIDs");
                    SetReadOnly(pIncHdr, "IncludeReserved");
                    SetReadOnly(pIncHdr, "IncludeGlobals");
                    propertyGrid1.SelectedObject = pIncHdr;
                }
                else {
                    propertyGrid1.SelectedObject = null;
                }
                break;
            }

            static void SetReadOnly(IncludeHdrProperties hdr, string propname) {
                Attribute readOnly = TypeDescriptor.GetProperties(hdr.GetType())[propname].Attributes[typeof(ReadOnlyAttribute)];
                FieldInfo fi = readOnly.GetType().GetRuntimeFields().ToArray()[0];
                fi?.SetValue(readOnly, EditGame.SierraSyntax);
            }
        }
        #endregion

        #region Resource Selection Methods
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
            // if update is requested
            if (WinAGISettings.ShowPreview.Value && UpdatePreview) {
                // load the preview item
                PreviewWin.LoadPreview(NewResType, NewResNum);
            }

            // update selection properties
            SelResType = NewResType;
            SelResNum = NewResNum;
            if (WinAGISettings.ResListType.Value != ResListType.None) {
                // add selected resource to navigation queue
                AddToQueue(SelResType, SelResNum < 0 ? 256 : SelResNum);
                // always disable forward button
                cmdForward.Enabled = false;
                // enable back button if at least two in queue
                cmdBack.Enabled = ResQPtr > 0;
                // if a logic is selected, and layout editor is active form
                if (SelResType == AGIResType.Logic) {
                    // if syncing the layout editor and the treeview list
                    if (WinAGISettings.LESync.Value) {
                        if (ActiveMdiChild is not null) {
                            if (ActiveMdiChild is frmLayout) {
                                if (EditGame.Logics[SelResNum].IsRoom) {
                                    // if option to sync is set
                                    LayoutEditor.SelectRoom(SelResNum);
                                }
                            }
                        }
                    }
                }
            }
            // if preview win is active, OR no windows
            if (MDIMain.ActiveMdiChild == PreviewWin ||
                MDIMain.ActiveMdiChild is null) {
                UpdateTBResourceBtns(SelResType, true, false, NewResNum);
            }
            if (infoGridScope == InfoGridScope.SelectedResource) {
                RefreshInfoGrid();
            }
        }

        private static void RenumberSelectedResource() {
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
            case Include:
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.LogicNumber == SelResNum) {
                        frm.ToggleInGame();
                        return;
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
                // words, objects, game or none
                // InGame does not apply
                return;
            }
            // no open editor; remove it using default process
            DialogResult rtn;
            bool dontAsk = false, haserror = false;
            string resID = "";
            switch (SelResType) {
            case AGIResType.Logic:
                resID = EditGame.Logics[SelResNum].ID;
                haserror = EditGame.Logics[SelResNum].Error != ResourceErrorType.NoError &&
                    EditGame.Logics[SelResNum].Error != ResourceErrorType.FileIsReadonly;
                break;
            case AGIResType.Picture:
                resID = EditGame.Pictures[SelResNum].ID;
                haserror = EditGame.Pictures[SelResNum].Error != ResourceErrorType.NoError &&
                    EditGame.Pictures[SelResNum].Error != ResourceErrorType.FileIsReadonly;
                break;
            case AGIResType.Sound:
                resID = EditGame.Sounds[SelResNum].ID;
                haserror = EditGame.Sounds[SelResNum].Error != ResourceErrorType.NoError &&
                    EditGame.Sounds[SelResNum].Error != ResourceErrorType.FileIsReadonly;
                break;
            case AGIResType.View:
                resID = EditGame.Views[SelResNum].ID;
                haserror = EditGame.Views[SelResNum].Error != ResourceErrorType.NoError &&
                    EditGame.Views[SelResNum].Error != ResourceErrorType.FileIsReadonly;
                break;
            case Include:
                // remove it without checking for export, since includes can't be exported
                Debug.Assert(EditGame.IncludeFiles[SelResNum].Type != IncludeType.ResourceIDs);
                Debug.Assert(EditGame.IncludeFiles[SelResNum].Type != IncludeType.Reserved);
                Debug.Assert(EditGame.IncludeFiles[SelResNum].Type != IncludeType.Globals);
                IncludeDefines.Remove(EditGame.IncludeFiles[SelResNum].Filename);
                EditGame.IncludeFiles.RemoveAt(SelResNum);
                RefreshIncludeList();
                return;
            default:
                Debug.Assert(false);
                return;
            }
            if (WinAGISettings.AskExport.Value && !haserror) {
                rtn = MsgBoxEx.Show(MDIMain,
                    "Do you want to export '" + resID + "' before removing it from your game?",
                    "Export " + SelResType.ToString() + " Before Removal",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    "Don't ask this question again", ref dontAsk);
                WinAGISettings.AskExport.Value = !dontAsk;
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
                dontAsk = false;
                rtn = MsgBoxEx.Show(MDIMain,
                    "Removing '" + resID + "' from your game.\n\nSelect OK to proceed, or Cancel to keep it in game.",
                    "Remove " + SelResType.ToString() + " From Game",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    "Don't ask this question again", ref dontAsk);
                WinAGISettings.AskRemove.Value = !dontAsk;
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

        public static void EditSelectedItemProperties(int FirstProp) {
            // only use for resources that are NOT being edited;
            // if the resource is being edited, the editor for that
            // resource handles description, ID and number changes
            string resID = "", description = "";

            switch (SelResType) {
            case AGIResType.Logic:
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.FormMode == LogicFormMode.Logic && frm.LogicNumber == SelResNum) {
                        frm.EditLogicProperties(1);
                        return;
                    }
                }
                resID = EditGame.Logics[SelResNum].ID;
                description = EditGame.Logics[SelResNum].Description;
                break;
            case AGIResType.Picture:
                foreach (frmPicEdit frm in PictureEditors) {
                    if (frm.PictureNumber == SelResNum) {
                        frm.EditPictureProperties(1);
                        return;
                    }
                }
                resID = EditGame.Pictures[SelResNum].ID;
                description = EditGame.Pictures[SelResNum].Description;
                break;
            case AGIResType.Sound:
                foreach (frmSoundEdit frm in SoundEditors) {
                    if (frm.SoundNumber == SelResNum) {
                        frm.EditSoundProperties(1);
                        return;
                    }
                }
                resID = EditGame.Sounds[SelResNum].ID;
                description = EditGame.Sounds[SelResNum].Description;
                break;
            case AGIResType.View:
                foreach (frmViewEdit frm in ViewEditors) {
                    if (frm.ViewNumber == SelResNum) {
                        frm.EditViewProperties(1);
                        return;
                    }
                }
                resID = EditGame.Views[SelResNum].ID;
                description = EditGame.Views[SelResNum].Description;
                break;
            case AGIResType.Objects:
                description = EditGame.InvObjects.Description;
                break;
            case AGIResType.Words:
                description = EditGame.WordList.Description;
                break;
            }
            _ = GetNewResID(SelResType, SelResNum, ref resID, ref description, true, FirstProp);
            return;
        }

        public static void SearchForID() {
            switch (SelResType) {
            case AGIResType.Logic:
                GFindText = EditGame.Logics[SelResNum].ID;
                break;
            case AGIResType.Picture:
                GFindText = EditGame.Pictures[SelResNum].ID;
                break;
            case AGIResType.Sound:
                GFindText = EditGame.Sounds[SelResNum].ID;
                break;
            case AGIResType.View:
                GFindText = EditGame.Views[SelResNum].ID;
                break;
            }
            GFindDir = FindDirection.All;
            GMatchWord = true;
            GMatchCase = true;
            GLogFindLoc = FindLocation.All;
            GFindSynonym = false;

            // reset search flags
            frmFind.ResetSearch();

            // display find form
            FindingForm.SetForm(FindFormFunction.FindLogic, true);
            FindingForm.Visible = true;
            FindingForm.Select();
        }
        #endregion

        #region General Methods
        internal void UpdateTBGameBtns() {
            // enable/disable buttons based on current game/editor state
            btnCloseGame.Enabled = EditGame is not null;
            btnCompile.Enabled = EditGame is not null;
            btnRun.Enabled = EditGame is not null;
            btnImportRes.Enabled = EditGame is not null;
            mnuTLayout.Enabled = btnLayoutEd.Enabled = EditGame is not null && EditGame.UseLE;
            if (EditGame is null || !EditGame.SierraSyntax) {
                mnuTMenuEditor.Enabled = true;
                btnMenuEd.Enabled = true;
                mnuTGlobals.Text = "Global Defines ...";
                btnGlobals.ToolTipText = "Global Defines";
                mnuTReserved.Text = "Reserved Defines ...";
            }
            else {
                mnuTMenuEditor.Enabled = false;
                btnMenuEd.Enabled = false;
                mnuTGlobals.Text = "'gamedefs.h' Editor";
                btnGlobals.ToolTipText = "gamedefs.h";
                mnuTReserved.Text = "'sysdefs.h' Editor";
            }
        }

        internal void UpdateTBResourceBtns(AGIResType restype, bool ingame, bool changed, int resnum) {
            btnSaveResource.Enabled = changed;
            switch (restype) {
            case Game:
                btnSaveResource.Text = "Save Resource";
                btnExportRes.Enabled = true;
                btnExportRes.Text = "Export All Resources";
                btnAddRemove.Enabled = false;
                btnAddRemove.Image = EditorResources.tbAdd;
                btnAddRemove.Text = "Add/Remove Resource";
                break;
            case AGIResType.Logic:
            case AGIResType.Picture:
            case AGIResType.Sound:
            case AGIResType.View:
                if (resnum == -1) {
                    // header - no export, save, or add/remove
                    btnSaveResource.Text = "Save";
                    btnSaveResource.Enabled = false;
                    btnAddRemove.Enabled = false;
                    btnAddRemove.Image = EditorResources.tbRemove;
                    btnAddRemove.Text = "Remove";
                    btnExportRes.Text = "Export";
                    btnExportRes.Enabled = false;
                }
                else {
                    btnSaveResource.Text = "Save " + restype.ToString();
                    btnExportRes.Enabled = true;
                    btnExportRes.Text = "Export " + restype.ToString();
                    btnAddRemove.Enabled = true;
                    if (ingame) {
                        btnAddRemove.Image = EditorResources.tbRemove;
                        btnAddRemove.Text = "Remove " + restype.ToString();
                    }
                    else {
                        btnAddRemove.Image = EditorResources.tbAdd;
                        btnAddRemove.Text = "Add " + restype.ToString();
                    }
                }
                break;
            case Objects:
            case Words:
                btnSaveResource.Text = "Save " + restype.ToString();
                btnExportRes.Enabled = true;
                btnExportRes.Text = "Export " + restype.ToString();
                btnAddRemove.Enabled = false;
                btnAddRemove.Image = EditorResources.tbAdd;
                btnAddRemove.Text = "Add/Remove Resource";
                break;
            case Include:
                btnSaveResource.Text = "Save";
                btnExportRes.Enabled = false;
                btnExportRes.Text = "Export";
                btnAddRemove.Enabled = true;
                if (ingame) {
                    btnAddRemove.Image = EditorResources.tbRemove;
                    btnAddRemove.Text = "Remove Include";
                }
                else {
                    btnAddRemove.Image = EditorResources.tbAdd;
                    btnAddRemove.Text = "Add Include";
                }
                break;

            case Globals:
                btnSaveResource.Text = "Save " + restype.ToString();
                btnExportRes.Enabled = true;
                btnExportRes.Text = "Export " + restype.ToString();
                btnAddRemove.Enabled = false;
                btnAddRemove.Image = EditorResources.tbAdd;
                btnAddRemove.Text = "Add/Remove Resource";
                break;
            case Menu:
                btnSaveResource.Text = "Update Source Logic";
                btnExportRes.Enabled = false;
                btnExportRes.Text = "Export Resource";
                btnAddRemove.Enabled = false;
                btnAddRemove.Image = EditorResources.tbAdd;
                btnAddRemove.Text = "Add/Remove Resource";
                break;
            default:
                btnSaveResource.Text = "Save Resource";
                btnExportRes.Enabled = false;
                btnExportRes.Text = "Export Resource";
                btnAddRemove.Enabled = false;
                btnAddRemove.Image = EditorResources.tbAdd;
                btnAddRemove.Text = "Add/Remove Resource";
                break;
            }
        }

        private void ShowProperties() {
            ShowProperties(false, "General", "");
        }

        public void ShowProperties(bool EnableOK, string StartTab = "", string StartProp = "") {
            // show properties form
            using (frmGameProperties propForm = new(GameSettingFunction.Edit, StartTab, StartProp)) {
                propForm.btnOK.Enabled = EnableOK;
                if (propForm.ShowDialog(MDIMain) == DialogResult.Cancel) {
                    // exit withoutsaving anything
                    propForm.Dispose();
                    return;
                }
                EditGame.Designer = propForm.txtDesigner.Text;
                EditGame.Description = propForm.txtGameDescription.Text;
                EditGame.GameAbout = propForm.txtGameAbout.Text;
                // if no directory, force platform to nothing
                if (propForm.NewPlatformFile.Length == 0) {
                    EditGame.PlatformType = PlatformType.None;
                }
                else {
                    // platform
                    if (propForm.optDosBox.Checked) {
                        EditGame.PlatformType = PlatformType.DosBox;
                    }
                    else if (propForm.optScummVM.Checked) {
                        EditGame.PlatformType = PlatformType.ScummVM;
                    }
                    else if (propForm.optNAGI.Checked) {
                        EditGame.PlatformType = PlatformType.NAGI;
                    }
                    else if (propForm.optAGILE.Checked) {
                        EditGame.PlatformType = PlatformType.AGILE;
                    }
                    else if (propForm.optOther.Checked) {
                        EditGame.PlatformType = PlatformType.Other;
                    }
                }

                if (EditGame.PlatformType != PlatformType.None) {
                    EditGame.Platform = propForm.NewPlatformFile;
                    // platform options OK if dosbox or scummvm
                    if (EditGame.PlatformType == PlatformType.DosBox ||
                        EditGame.PlatformType == PlatformType.ScummVM ||
                        EditGame.PlatformType == PlatformType.AGILE ||
                        EditGame.PlatformType == PlatformType.Other) {
                        EditGame.PlatformOpts = propForm.txtOptions.Text;
                    }
                    else {
                        EditGame.PlatformOpts = "";
                    }
                    // dos executable only used if dosbox
                    if (EditGame.PlatformType == PlatformType.DosBox) {
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
                // interpreter version (if changed)
                if ((int)EditGame.InterpreterVersion.Index != propForm.cmbVersion.SelectedIndex) {
                    ChangeIntVersion(propForm.cmbVersion.Text);
                }
                if (EditGame.GameID != propForm.txtGameID.Text) {
                    Debug.Assert(propForm.txtGameID.Text != "");
                    ChangeGameID(propForm.txtGameID.Text);
                }
                if (!EditGame.SrcResDirName.Equals(propForm.txtResDir.Text, StringComparison.CurrentCultureIgnoreCase)) {
                    ChangeResDir(propForm.txtResDir.Text);
                }
                if (EditGame.SourceExt != propForm.txtSrcExt.Text) {
                    EditGame.SourceExt = propForm.txtSrcExt.Text.ToLower();
                    foreach (Logic aLogic in EditGame.Logics) {
                        SafeFileMove(aLogic.SourceFile, Path.Combine(EditGame.SrcResDir, Path.GetFileNameWithoutExtension(aLogic.SourceFile) + "." + EditGame.SourceExt), true);
                    }
                }
                bool useglobal = EditGame.IncludeGlobals;
                EditGame.SierraSyntax = propForm.chkSierraSyntax.Checked;
                if (EditGame.SierraSyntax) {
                    EditGame.IncludeIDs = false;
                    EditGame.IncludeReserved = false;
                    EditGame.IncludeGlobals = false;
                }
                else {
                    EditGame.IncludeIDs = propForm.chkResourceIDs.Checked;
                    EditGame.IncludeReserved = propForm.chkResDefs.Checked;
                    EditGame.IncludeGlobals = propForm.chkGlobals.Checked;
                }
                // update global list if it changed
                if (useglobal != EditGame.IncludeGlobals) {
                    if (EditGame.IncludeGlobals) {
                        EditGame.GlobalDefines.LoadDefines();
                    }
                    else {
                        EditGame.GlobalDefines.Clear();
                        GEInUse = false;
                    }
                    LogicListChange();
                }
                // refresh include list in case they changed
                RefreshIncludeList();
                EditGame.UseLE = propForm.chkUseLE.Checked;
                // update menu/toolbar, and hide LE if not in use anymore
                UpdateLEStatus();
                EditGame.CodePage = propForm.NewCodePage;
                // save changes to file
                EditGame.SaveProperties();
                RefreshPropertyGrid();
            }
        }

        private void RunGame() {
            string parameters = "";

            string errorTitle = "", errorType = "";
            bool failed;

            // first check for missing platform
            if (EditGame.PlatformType == PlatformType.None) {
                // notify user and show property dialog
                MDIMain.MsgBoxWithHelp(
                    "You need to select a platform on which to run your game first.",
                    "No Platform Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    "htm\\winagi\\prop_platform.htm#platform");
                ShowProperties(false, "Platform");
                // if still no platform
                if (EditGame.PlatformType == PlatformType.None) {
                    // just exit
                    return;
                }
            }
            switch (EditGame.PlatformType) {
            case PlatformType.DosBox:
                // DosBox - verify target exists
                if (!File.Exists(Path.Combine(EditGame.GameDir, EditGame.DOSExec))) {
                    MDIMain.MsgBoxWithHelp(
                        "The DOS executable file '" + EditGame.DOSExec + "' is missing from the " +
                        "game directory. Aborting DosBox session.",
                        "Missing DOS Executable File",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        "htm\\winagi\\prop_platform.htm#platform");
                    return;
                }
                // dosbox parameters: gamedir+agi.exe -noautoexec
                //  (need -noautoexec option as mandatory setting to avoid virtual C-drive assignment issues)
                parameters = '"' + Path.Combine(EditGame.GameDir, EditGame.DOSExec) + '"' + " -noautoexec " + EditGame.PlatformOpts;
                break;
            case PlatformType.ScummVM:
                // scummvm parameters: --p gamedir --auto-detect
                parameters = "-p \"" + Path.GetDirectoryName(EditGame.GameDir) + "\" --auto-detect " + EditGame.PlatformOpts;
                break;
            case PlatformType.AGILE:
                // agile parameters: gamedir
                parameters = '"' + Path.Combine(EditGame.GameDir, EditGame.GameID) + '"' + " " + EditGame.PlatformOpts;
                break;
            case PlatformType.NAGI:
                // no parameters for nagi; just run the program
                break;
            case PlatformType.Other:
                // run with whatever is in Platform and PlatformOpts
                parameters = EditGame.PlatformOpts;
                break;
            }
            if (CheckLogics()) {
                // run the program if check is OK
                Process runagi = new();
                ProcessStartInfo runparams = new() {
                    FileName = EditGame.Platform,
                    Arguments = parameters,
                    ErrorDialog = true,
                    ErrorDialogParentHandle = Handle,
                    //runparams.UseShellExecute = true;
                    WindowStyle = ProcessWindowStyle.Normal,
                    WorkingDirectory = EditGame.GameDir
                };
                runagi.StartInfo = runparams;
                try {
                    failed = !runagi.Start();
                    runagi.Dispose();
                    if (failed) {
                        errorType = "(process failed to run)";
                    }
                }
                catch (Exception ex) {
                    failed = true;
                    errorType = "(" + ex.Message + ")";
                }

                if (failed) {
                    string errorMsg = "";
                    switch (EditGame.PlatformType) {
                    case Engine.PlatformType.DosBox:
                        errorTitle = "DosBox Error";
                        errorMsg = "DosBox ";
                        break;
                    case Engine.PlatformType.ScummVM:
                        errorTitle = "ScummVM Error";
                        errorMsg = "ScummVM ";
                        break;
                    case Engine.PlatformType.NAGI:
                        errorTitle = "NAGI Error";
                        errorMsg = "NAGI ";
                        break;
                    case Engine.PlatformType.Other:
                        errorTitle = "Run AGI Game Error";
                        errorMsg = "this program ";
                        break;
                    }
                    errorMsg = "Unable to run " + errorMsg + errorType + ". Make sure you " +
                                "have selected the correct executable file, and that any parameters " +
                                "you included are correct.";
                    MDIMain.MsgBoxWithHelp(
                        errorMsg,
                        errorTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation,
                        "htm\\winagi\\prop_platform.htm#platform");
                }
            }
        }

        private static void ShowHelp() {
            // called by HelpRequest event when the main form has focus
            // such as when no other child form is visible

            if (EditGame is null) {
                // show general help
                Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\winagihelp.htm");
            }
            else {
                if (WinAGISettings.ResListType.Value == ResListType.None) {
                    // show general help
                    Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\winagihelp.htm");
                }
                else {
                    // show restree help
                    Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\resourcetree.htm");
                }
            }
        }

        private bool ReadSettings() {
            double value;
            double top, left;
            double width, height;
            int i, noCompVal;
            bool max;
            // first check if the user config file exists; if not, copy the default config file to the user config location
            string userConfig = UserConfigPath;
            string defaultConfig = Path.Combine(ProgramDir, "winagi.config");
            if (!File.Exists(userConfig) && File.Exists(defaultConfig)) {
                File.Copy(defaultConfig, userConfig);
            }
            // check other default files; if missing, try copying them from the program directory
            var filechecks = new List<Action>() {
                () => {
                    if (!File.Exists(Path.Combine(AppDataDir, "snippets.txt")) && File.Exists(Path.Combine(ProgramDir, "snippets.txt"))) {
                        File.Copy(Path.Combine(ProgramDir, "snippets.txt"), Path.Combine(AppDataDir, "snippets.txt"));
                    }
                },
                () => {
                    if (!File.Exists(Path.Combine(AppDataDir, "deflog.txt")) && File.Exists(Path.Combine(ProgramDir, "deflog.txt"))) {
                        File.Copy(Path.Combine(ProgramDir, "deflog.txt"), Path.Combine(AppDataDir, "deflog.txt"));
                    }
                },
                () => {
                    if (!File.Exists(Path.Combine(AppDataDir, "default_menu.txt")) && File.Exists(Path.Combine(ProgramDir, "default_menu.txt"))) {
                        File.Copy(Path.Combine(ProgramDir, "default_menu.txt"), Path.Combine(AppDataDir, "default_menu.txt"));
                    }
                },
                () => {
                    // verify template directory exists; if not, create it
                    string appDataTemplates = Path.Combine(AppDataDir, "Templates");
                    string programTemplates = Path.Combine(ProgramDir, "Templates");
                    if (!Directory.Exists(appDataTemplates) && Directory.Exists(programTemplates)) {
                        CopyDirectory(programTemplates, appDataTemplates);
                    }
                }
            };
            foreach (var check in filechecks) {
                try {
                    check();
                }
                catch {
                    // if any error occurs during the file copy, just ignore it and continue;
                    // the program can run without these files, and we'll try again next time
                }
            }
            // open the program settings  file
            try {
                WinAGISettingsFile = new SettingsFile(userConfig, FileMode.OpenOrCreate);
            }
            catch (WinAGIException wex) {
                if (wex.HResult == WINAGI_ERR + 539) {
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
                        ErrMsgBox(wex, "Unable to open Configuration file (winagi.config) due to file access error",
                            "Attempting to create a new file.", "Configuration File Error");
                        File.Move(userConfig, userConfig + "_OLD");
                        WinAGISettingsFile = new SettingsFile(userConfig, FileMode.Create);
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
            WinAGISettings.ErrorLevel.ReadSetting(WinAGISettingsFile);
            ErrorLevel = WinAGISettings.ErrorLevel.Value;


            // WARNINGS
            WinAGISettings.RenameWAG.ReadSetting(WinAGISettingsFile);
            WinAGISettings.OpenNew.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AskExport.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AskRemove.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AutoUpdateDefines.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AutoUpdateResDefs.ReadSetting(WinAGISettingsFile);
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
            WinAGISettings.NotifyCompWarn.ReadSetting(WinAGISettingsFile);
            WinAGISettings.NotifyCompFail.ReadSetting(WinAGISettingsFile);
            WinAGISettings.NotifyLoadWarn.ReadSetting(WinAGISettingsFile);
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
            WinAGISettings.Warn2089Mirror.ReadSetting(WinAGISettingsFile);
            // GENERAL
            WinAGISettings.ShowSplashScreen.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowPreview.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShiftPreview.ReadSetting(WinAGISettingsFile);
            WinAGISettings.HidePreview.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ResListType.ReadSetting(WinAGISettingsFile);
            WinAGISettings.AutoExport.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowInfoGrid.ReadSetting(WinAGISettingsFile);
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
            // decoder uses default extension
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
            WinAGISettings.PlaybackMode.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowKeyboard.ReadSetting(WinAGISettingsFile);
            WinAGISettings.NoKeyboardSound.ReadSetting(WinAGISettingsFile);
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
            WinAGISettings.ShowGrid.ReadSetting(WinAGISettingsFile);
            WinAGISettings.ShowVEPreview.ReadSetting(WinAGISettingsFile);
            WinAGISettings.DefPrevPlay.ReadSetting(WinAGISettingsFile);
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
            else if (WinAGISettings.ViewScaleEdit.Value > 10) {
                WinAGISettings.ViewScaleEdit.Value = 10;
            }
            else {
                WinAGISettings.ViewScaleEdit.Value = (int)WinAGISettings.ViewScaleEdit.Value;
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
            WinAGISettings.LEUseGrid.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEGridMinor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEGridMajor.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEShowGrid.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEShowPics.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEShowHidden.ReadSetting(WinAGISettingsFile);
            // minor restricted to 0.05 - 1.0 in 0.05 increments
            if (WinAGISettings.LEGridMinor.Value > 1) {
                WinAGISettings.LEGridMinor.Value = 1;
            }
            else if (WinAGISettings.LEGridMinor.Value < 0.05f) {
                WinAGISettings.LEGridMinor.Value = 0.05f;
            }
            else {
                WinAGISettings.LEGridMinor.Value = (float)(Math.Round(WinAGISettings.LEGridMinor.Value * 20) * 0.05);
            }
            // major grid must be an increment of minor
            WinAGISettings.LEGridMajor.Value = (float)(Math.Round(WinAGISettings.LEGridMajor.Value / WinAGISettings.LEGridMinor.Value) * WinAGISettings.LEGridMinor.Value);
            WinAGISettings.LESync.ReadSetting(WinAGISettingsFile);
            WinAGISettings.LEScale.ReadSetting(WinAGISettingsFile);
            if (WinAGISettings.LEScale.Value < 1) {
                WinAGISettings.LEScale.Value = 1;
            }
            else if (WinAGISettings.LEScale.Value > 9) {
                WinAGISettings.LEScale.Value = 9;
            }
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

            // GLOBAL EDITOR
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
            max = WinAGISettingsFile.GetSetting(sPOSITION, "WindowMax", false);
            left = WinAGISettingsFile.GetSetting(sPOSITION, "Left", Screen.PrimaryScreen.Bounds.Width * 0.15);
            top = WinAGISettingsFile.GetSetting(sPOSITION, "Top", Screen.PrimaryScreen.Bounds.Height * 0.15);
            width = WinAGISettingsFile.GetSetting(sPOSITION, "Width", Screen.PrimaryScreen.Bounds.Width * 0.7);
            height = WinAGISettingsFile.GetSetting(sPOSITION, "Height", Screen.PrimaryScreen.Bounds.Height * 0.7);
            if (width < 360) {
                width = 360;
            }
            if (width > SystemInformation.VirtualScreen.Width) {
                width = SystemInformation.VirtualScreen.Width;
            }
            if (height < 361) {
                height = 361;
            }
            if (height > SystemInformation.VirtualScreen.Height) {
                height = SystemInformation.VirtualScreen.Height;
            }
            if (left < 0) {
                left = 0;
            }
            if (left > SystemInformation.VirtualScreen.Width * 0.85) {
                left = SystemInformation.VirtualScreen.Width * 0.85;
            }
            if (top < 0) {
                top = 0;
            }
            if (top > SystemInformation.VirtualScreen.Height * 0.85) {
                top = SystemInformation.VirtualScreen.Height * 0.85;
            }
            MDIMain.Bounds = new Rectangle((int)left, (int)top, (int)width, (int)height);
            if (max) {
                WindowState = FormWindowState.Maximized;
            }

            // RESOURCE PANE AND PROPERTY WINDOW SPLIT
            value = WinAGISettingsFile.GetSetting(sPOSITION, "ResourceWidth", 125 * 1.5);
            if (value < 125) {
                value = 125;
            }
            else if (value > Bounds.Width - 125) {
                value = Bounds.Width - 125;
            }
            pnlResources.Width = (int)value;
            PropPanelSplit = WinAGISettingsFile.GetSetting(sPOSITION, "PropPanelSplit", 4);
            if (PropPanelSplit < splResource.Panel2MinSize) {
                PropPanelSplit = splResource.Panel2MinSize;
            }
            if (PropPanelSplit > PropPanelMaxSize) {
                PropPanelSplit = PropPanelMaxSize;
            }
            // set initial position of property panel
            splResource.SplitterDistance = splResource.Height - splResource.Margin.Top - splResource.Margin.Bottom - splResource.SplitterWidth - PropPanelSplit;
            // 
            pnlInfoGrid.Height = WinAGISettingsFile.GetSetting(sPOSITION, "InfoGridSplit", 86);

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
            bool tools = false;
            for (i = 1; i <= 6; i++) {
                string caption = WinAGISettingsFile.GetSetting(sTOOLS, "Caption" + i, "");
                string tool = WinAGISettingsFile.GetSetting(sTOOLS, "Source" + i, "");
                if (caption.Length > 0 && tool.Length > 0) {
                    mnuTools.DropDownItems["mnuTCustom" + i].Visible = true;
                    mnuTools.DropDownItems["mnuTCustom" + i].Text = caption;
                    mnuTools.DropDownItems["mnuTCustom" + i].Tag = tool;
                    tools = true;
                }
            }
            mnuTSep2.Visible = tools;

            // DEFAULT RESERVED DEFINES
            DefaultReservedDefines = new(WinAGISettingsFile);

            // IGNORED COMPILER WARNINGS
            noCompVal = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn0", 0);
            // 5001 - 5030
            for (i = 0; i <= 29; i++) {
                SetIgnoreWarning(5001 + i, (noCompVal & (1 << i)) == (1 << i));
            }
            noCompVal = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn1", 0);
            // 5031 - 5060
            for (i = 30; i <= 59; i++) {
                SetIgnoreWarning(5001 + i, (noCompVal & (1 << (i - 30))) == 1 << (i - 30));
            }
            noCompVal = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn2", 0);
            // 5061 - 5090
            for (i = 60; i <= 89; i++) {
                SetIgnoreWarning(5001 + i, (noCompVal & (1 << (i - 60))) == 1 << (i - 60));
            }
            noCompVal = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn3", 0);
            // 5091 - 5121 (current max of 5121)
            for (i = 90; i <= 120; i++) {
                SetIgnoreWarning(5001 + i, (noCompVal & (1 << (i - 90))) == 1 << (i - 90));
            }
            noCompVal = WinAGISettingsFile.GetSetting(sLOGICS, "NoCompWarn4", 0);
            // 7001 - 7020 (current max of 7020)
            for (i = 121; i < WARNCOUNT; i++) {
                SetIgnoreWarning(7001 + i - 121, (noCompVal & (1 << (i - 121))) == 1 << (i - 121));
            }
            return true;
        }

        private void SaveSettings() {
            // update the non-settings items to the settings list,
            // then save it to file

            // WINDOW POSITION
            if (MDIMain.WindowState == FormWindowState.Maximized) {
                // save Max Value only
                WinAGISettingsFile.WriteSetting(sPOSITION, "WindowMax", true);
            }
            else {
                // save all window settings
                WinAGISettingsFile.WriteSetting(sPOSITION, "Top", Top.ToString());
                WinAGISettingsFile.WriteSetting(sPOSITION, "Left", Left.ToString());
                WinAGISettingsFile.WriteSetting(sPOSITION, "Width", Width.ToString());
                WinAGISettingsFile.WriteSetting(sPOSITION, "Height", Height.ToString());
                WinAGISettingsFile.WriteSetting(sPOSITION, "WindowMax", false.ToString());
            }

            // RESOURCE PANE AND PROPERTY WINDOW SPLIT
            WinAGISettingsFile.WriteSetting(sPOSITION, "ResourceWidth", pnlResources.Width);
            PropPanelSplit = splResource.Height - splResource.Margin.Top - splResource.Margin.Bottom - splResource.SplitterWidth - splResource.SplitterDistance;
            WinAGISettingsFile.WriteSetting(sPOSITION, "PropPanelSplit", PropPanelSplit);
            // WARNING GRID SPLIT
            WinAGISettingsFile.WriteSetting(sPOSITION, "InfoGridSplit", pnlInfoGrid.Height);

            // MRU ENTRIES
            for (int i = 0; i < 4; i++) {
                WinAGISettingsFile.WriteSetting(sMRULIST, "MRUGame" + (i + 1), strMRU[i]);
            }

            // TOOLS SETTINGS
            // updated WHEN changes made by the edit tools form

            // RESERVED DEFINES OVERRIDES
            // updated when changes made by the edit reserved defines form
            // 

            // IGNORED COMPILER WARNINGS
            int warndata = 0;
            for (int i = 0; i < 30; i++) {
                warndata |= Engine.Base.IgnoreWarning(5001 + i) ? 1 << i : 0;
            }
            WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn0", warndata);
            warndata = 0;
            for (int i = 0; i < 30; i++) {
                warndata |= Engine.Base.IgnoreWarning(5031 + i) ? 1 << i : 0;
            }
            WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn1", warndata);
            warndata = 0;
            for (int i = 0; i < 30; i++) {
                warndata |= Engine.Base.IgnoreWarning(5061 + i) ? 1 << i : 0;
            }
            WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn2", warndata);
            warndata = 0;
            for (int i = 0; i < 30; i++) {
                warndata |= Engine.Base.IgnoreWarning(5091 + i) ? 1 << i : 0;
            }
            WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn3", warndata);
            warndata = 0;
            for (int i = 1; i < (WARNCOUNT % 30); i++) {
                warndata |= Engine.Base.IgnoreWarning(7001 + i) ? 1 << i : 0;
            }
            WinAGISettingsFile.WriteSetting(sLOGICS, "NoCompWarn4", warndata);

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

        private static void CheckCommandLine() {
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
            else if (args[1].Right(4).Equals("." + WinAGISettings.DefaultExt.Value, StringComparison.CurrentCultureIgnoreCase)) {
                // open a logic source text file or logic resource
                NewLogic(args[1]);
            }
            else {
                // check for a file
                // (first check for logic source (it is variable so can't be
                // used in a switch statement)
                if (args[1].Right(4).Equals("." + WinAGISettings.DefaultExt.Value, StringComparison.CurrentCultureIgnoreCase)) {
                    // open a logic source text file
                    NewLogic(args[1]);
                    return;
                }
                switch (args[1].Right(4).ToLower()) {
                case ".wag":
                    // open a game
                    OpenWAGFile(args[1]);
                    break;
                case ".wal":
                    // layout files can't be opened by command line anymore
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
                    NewSound(args[1], SoundImportFormat.AGI, null);
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
                        MessageBox.Show(MDIMain,
                            $"'{Path.GetFileName(args[1])}' is not a valid WinAGI resource filename.",
                            "Invalid command line.",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    break;
                }
            }
        }

        public DialogResult MsgBoxWithHelp(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string helpTopic) {
            ShowingMsgBox = true;
            DialogResult result;
            // Show MessageBox with Help button and topic
            result = MessageBox.Show(this, text, caption, buttons, icon, 0, 0, WinAGIHelp, helpTopic);
            ShowingMsgBox = false;
            return result;
        }

        public static string RelativeToSrcDir(string filename) {
            string src = EditGame.SrcResDir;

            // Normalize both paths
            string fullSrc = Path.GetFullPath(src);
            string fullFile = Path.GetFullPath(filename);

            // Case-insensitive compare on Windows
            if (fullFile.StartsWith(fullSrc, StringComparison.OrdinalIgnoreCase)) {
                string relative = fullFile.Substring(fullSrc.Length)
                                          .TrimStart(Path.DirectorySeparatorChar);
                return relative;
            }

            // Different drive → cannot be relative
            if (!string.Equals(Path.GetPathRoot(fullSrc), Path.GetPathRoot(fullFile),
                               StringComparison.OrdinalIgnoreCase)) {
                return filename;
            }

            // Compute relative path
            string rel = Path.GetRelativePath(fullSrc, fullFile);

            // If it goes up 3 or more levels, keep absolute
            if (rel.StartsWith(".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "..")) {
                return filename;
            }

            return rel;
        }
        #endregion
        #endregion
    }

    public class WarningGridInfo {
        public EventType Type {
            get; set;
        }     // column 0, not visible
        public AGIResType ResType {
            get; set;
        } // column 1, not visible
        public string Code {
            get; set;
        }        // column 2, visible
        public string Description {
            get; set;
        } // column 3, visible
        public int ResNum {
            get; set;
        }         // column 4, visible
        public int Line {
            get; set;
        }           // column 5, visible
        public string Module {
            get; set;
        }      // column 6, visible
        public string Filename {
            get; set;
        }    // column 7, not visible
    }
}
