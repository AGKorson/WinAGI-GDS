using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using WinAGI.Engine;
using WinAGI.Common;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Engine.EventType;
using static WinAGI.Common.Base;
using System.Diagnostics;
using static WinAGI.Editor.Base;
using static WinAGI.Common.BkgdTasks;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;
using System.Security.Authentication.ExtendedProtection;
using System.Runtime.CompilerServices;

namespace WinAGI.Editor
{
    public partial class frmMDIMain : Form {
        const int MIN_WIDTH = 360;

        const int SPLIT_WIDTH = 60; //in twips
        const int SPLIT_HEIGHT = 5;  //in pixels
        const int MIN_SPLIT_V = 125; //pixels? 1875 in twips
        const int MAX_SPLIT_V = 500; //pixels? 7500 in twips
        const int MIN_SPLIT_H = 125;
        const int MAX_SPLIT_H = 500;
        const int MAX_SPLIT_RES = 10; //in rows
        const int MIN_SPLIT_RES = 3; //in rows

        int WLOffsetH;
        int WLOffsetW;

        double SplitVOffset;
        double SplitHOffset;
        double SplitResOffset;

        //compiler error list variables
        bool mLoading;
        double mEX, mEY;
        List<TWinAGIEventInfo> WarningList = [];

        int lngDefIndex; //default filterindex for opening text files

        //MouseButtonConstants StatusMouseBtn; 
        double sbX, sbY;
        double mPX, mPY;
        double mTX, mTY;
        int NLOffset, NLRow, NLRowHeight;

        public string LastNodeName;
        bool MDIHasFocus;
        bool ForcePreview;
        int FlashCount;

        //property box height
        int PropRowCount;
        private bool splashDone;
        //tracks status of caps/num/ins
        static bool CapsLock = false;
        static bool NumLock = false;
        static bool InsertLock = false;

        public void OnIdle(object sender, EventArgs e) {
            // Update the panels when the program is idle.
            bool newCapsLock = Console.CapsLock;
            bool newNumLock = Console.NumberLock;
            bool newInsertLock = IsKeyLocked(Keys.Insert);
            if (newCapsLock != CapsLock) {
                CapsLock = newCapsLock;
                if (CapsLockLabel is not null) {
                    CapsLockLabel.Text = CapsLock ? "CAP" : "";
                }
            }
            if (newNumLock != NumLock) {
                NumLock = newNumLock;
                if (NumLockLabel is not null) {
                    NumLockLabel.Text = NumLock ? "NUM" : "";
                }
            }
            if (newInsertLock != InsertLock) {
                InsertLock = newInsertLock;
                if (InsertLockLabel is not null) {
                    InsertLockLabel.Text = InsertLock ? "INS" : "";
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

                    //// disable context menu for non-text properties
                    //if (MDIMain.propertyGrid1.SelectedGridItem == null) {
                    //    return;
                    //}
                    //if (MDIMain.propertyGrid1.ActiveControl.GetType() != typeof(TextBox)) {
                    //    return;
                    //}
                    //switch (MDIMain.propertyGrid1.SelectedGridItem.Label) {
                    //case "GlobalDef":
                    //case "ID":
                    //case "Number":
                    //case "Description":
                    //    if (((TextBox)propertyGrid1.ActiveControl).ContextMenuStrip == null) {
                    //        ((TextBox)propertyGrid1.ActiveControl).AllowDrop = false;
                    //        ((TextBox)propertyGrid1.ActiveControl).ContextMenuStrip = new ContextMenuStrip();
                    //    }
                    //    break;
                    //}

                }
            }
        }

        public frmMDIMain() {
            InitializeComponent();

            //use idle time to update caps/num/ins
            Application.Idle += new System.EventHandler(OnIdle);

            // save pointer to main form
            MDIMain = this;

        }

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
            case EventType.etInfo:
                switch (e.NewInfo.InfoType) {
                case EInfoType.itInitialize:
                    bgwNewGame.ReportProgress(51, e.NewInfo.Text);
                    break;
                case EInfoType.itResources:
                    bgwNewGame.ReportProgress(52, e.NewInfo.Text);
                    break;
                case EInfoType.itFinalizing:
                    bgwNewGame.ReportProgress(53, e.NewInfo.Text);
                    break;
                case EInfoType.itPropertyFile:
                    bgwNewGame.ReportProgress(54, "");
                    break;
                }
                break;
            case etError:
                bgwNewGame.ReportProgress(1, e.NewInfo);
                break;
            case etResWarning:
                bgwNewGame.ReportProgress(1, e.NewInfo);
                break;
            case etDecompWarning:
                bgwNewGame.ReportProgress(3, e.NewInfo);
                break;
            case etTODO:
                // add to warning list
                bgwNewGame.ReportProgress(2, e.NewInfo);
                break;
            }
        }

        internal void GameEvents_LoadGameStatus(object sender, LoadGameEventArgs e) {
            switch (e.LoadInfo.Type) {
            case etInfo:
                switch (e.LoadInfo.InfoType) {
                case EInfoType.itInitialize:
                    break;
                case EInfoType.itValidating:
                    // check for WinAGI version update
                    if (e.LoadInfo.Text.Length > 0) {
                        bgwOpenGame.ReportProgress(4, "");
                    }
                    bgwOpenGame.ReportProgress(0, "Validating AGI game files ...");
                    break;
                case EInfoType.itPropertyFile:
                    if (e.LoadInfo.ResNum == 0) {
                        bgwOpenGame.ReportProgress(0, "Creating game property file ...");
                    }
                    else {
                        bgwOpenGame.ReportProgress(0, "Loading game property file ...");
                    }
                    break;
                case EInfoType.itResources:
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
                case EInfoType.itDecompiling:
                    bgwOpenGame.ReportProgress(0, "Validating AGI game files ...");
                    break;
                case EInfoType.itCheckCRC:
                    break;
                case EInfoType.itFinalizing:
                    bgwOpenGame.ReportProgress(0, "Configuring WinAGI");
                    break;
                }
                break;
            case etError:
                bgwOpenGame.ReportProgress(0, $"Load Error: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to error list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
            case etResWarning:
                bgwOpenGame.ReportProgress(0, $"Load Warning: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
            case etCompWarning:
                bgwOpenGame.ReportProgress(0, $"Load Warning: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
            case etDecompWarning:
                bgwOpenGame.ReportProgress(0, $"Load Warning: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
            case etTODO:
                bgwOpenGame.ReportProgress(0, $"{e.LoadInfo.ID} TODO: : {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(2, e.LoadInfo);
                break;
            }
        }

        internal void GameEvents_CompileLogicStatus(object sender, CompileLogicEventArgs e) {

        }

        internal void GameEvents_DecodeLogicStatus(object sender, DecodeLogicEventArgs e) {
            Debug.Print($"decode it: {e.DecodeInfo.Text}");
            bgwOpenGame?.ReportProgress(2, e.DecodeInfo);
        }

        private void btnOpenGame_Click(object sender, EventArgs e) {
            // open a game!
            OpenWAGFile();
        }

        private void mnuWCascade_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void mnuWArrange_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void mnuWTileH_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void mnuWTileV_Click(object sender, EventArgs e) {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void mnuWMinimize_Click(object sender, EventArgs e) {
            foreach (Form childForm in MdiChildren) {
                childForm.WindowState = FormWindowState.Minimized;
            }
        }

        private void mnuWClose_Click(object sender, EventArgs e) {
            // only close if window is close-able
            if (ActiveMdiChild != PreviewWin) {
                ActiveMdiChild.Close();
            }
        }

        private void mnuWindow_DropDownOpening(object sender, EventArgs e) {
            // disable the close item if no windows or if active window is preview
            mnuWClose.Enabled = (MdiChildren.Length != 0) && (ActiveMdiChild != PreviewWin) && (ActiveMdiChild != null);

        }

        private void frmMDIMain_Load(object sender, EventArgs e) {
            bool blnLastLoad;

            // STRATEGY FOR READING/WRITING DATA TO/FROM
            // AGI TEXT RESOURCES:
            //
            // TO READ from AGI RESOURCE:
            // read in as a byte array; then convert the
            // byte array to current codepage:
            //   strIn = agCodePage.GetString(bIn);
            //
            // TO WRITE to AGI RESOURCE:
            // convert string to byte array, then write
            // the byte array
            //   bOut = agCodePage.GetBytes(strOut);
            //

            //what is resolution?
            Debug.Print($"DeviceDPI: {this.DeviceDpi}");
            Debug.Print($"AutoScaleFactor: {this.AutoScaleFactor}");
            Debug.Print($"AutoscaleDimensions: {this.AutoScaleDimensions}");

            // toolbar stuff;
            btnNewRes.DefaultItem = btnNewLogic;
            btnOpenRes.DefaultItem = btnOpenLogic;
            btnImportRes.DefaultItem = btnImportLogic;

            // set up context menus for the resource tree/listbox
            tvwResources.ContextMenuStrip = new();
            lstResources.ContextMenuStrip = new();
            tvwResources.ContextMenuStrip.Opening += mnuResources_DropDownOpening;
            lstResources.ContextMenuStrip.Opening += mnuResources_DropDownOpening;

            //set preview window, status bar and other dialog objects
            PreviewWin = new frmPreview {
                MdiParent = MDIMain
            };
            MainStatusBar = statusStrip1;
            //      ViewClipboard = picViewCB;
            SoundClipboard = [];
            //      NotePictures = picNotes;
            //        WordsClipboard = new WordsUndo();
            FindingForm = new frmFind();

            // hide resource and warning panels until needed
            pnlResources.Visible = false;
            pnlWarnings.Visible = false;
            ////set property window split location based on longest word
            Size szText = TextRenderer.MeasureText(" Use Res Names ", propertyGrid1.Font);
            // set height based on text (assume padding of four? pixels above/below)
            PropRowHeight = szText.Height + 7;
            MAX_PROPGRID_HEIGHT = 11 * PropRowHeight;
            // set initial position of property panel
            splResource.Panel2MinSize = 3 * PropRowHeight;
            splResource.SplitterDistance = splResource.Height - splResource.Margin.Top - splResource.Margin.Bottom - splResource.SplitterWidth - 7 * PropRowHeight;
            // set grid row height
            fgWarnings.RowTemplate.Height = szText.Height + 2;
            // background color for previewing views is set to default
            PrevWinBColor = SystemColors.Control;

            // initialize the basic app functionality
            InitializeResMan();

            ProgramDir = FullDir(JustPath(Application.ExecutablePath));
            DefaultResDir = ProgramDir;
            //set browser start dir to program dir
            BrowserStartDir = ProgramDir;

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
            if (WinAGISettings.ShowSplashScreen) {
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

            // move resource menu items to appropriate context menu
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                tvwResources.ContextMenuStrip.Items.AddRange([mnuRNew, mnuROpen, mnuRImport, mnuRSeparator1, mnuROpenRes, mnuRSave, mnuRExport, mnuRSeparator2, mnuRAddRemove, mnuRRenumber, mnuRIDDesc, mnuRSeparator3, mnuRCompileLogic, mnuRSavePicImage, mnuRExportGIF]);
                break;
            case agiSettings.EResListType.ComboList:
                lstResources.ContextMenuStrip.Items.AddRange([mnuRNew, mnuROpen, mnuRImport, mnuRSeparator1, mnuROpenRes, mnuRSave, mnuRExport, mnuRSeparator2, mnuRAddRemove, mnuRRenumber, mnuRIDDesc, mnuRSeparator3, mnuRCompileLogic, mnuRSavePicImage, mnuRExportGIF]);
                break;
            }

            LogicEditors = [];
            ViewEditors = [];
            PictureEditors = [];
            SoundEditors = [];

            // build the lookup table for reserved defines
            BuildRDefLookup();
            // default to an empty globals list
            GDefLookup = [];
            //if using snippets
            if (WinAGISettings.Snippets) {
                // build snippet table
                BuildSnippets();
            }

            WinAGIHelp = ProgramDir + "WinAGI.chm";
            //      mnuHContents.Text = "Contents" + Keys.Tab + "F1";

            // initialize resource treelist by using clear method
            ClearResourceList();

            // set navlist parameters
            //set property window split location based on longest word
            szText = TextRenderer.MeasureText(" Logic 1 ", new Font(WinAGISettings.PFontName, WinAGISettings.PFontSize));
            NLRowHeight = szText.Height + 2;
            picNavList.Height = NLRowHeight * 5;
            picNavList.Top = (cmdBack.Top + cmdBack.Height / 2) - picNavList.Height / 2;
            // retrieve user's preferred AGI colors
            GetDefaultColors();

            // let the system catch up
            Refresh();

            if (WinAGISettings.ShowSplashScreen) {
                // dont close unless ~1.75 seconds passed
                while (!splashDone) {
                    Application.DoEvents();
                }
                splash.Close();
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
            blnLastLoad = WinAGISettingsList.GetSetting(sMRULIST, "LastLoad", false);

            //if nothing loaded AND autoreload is set AND something was loaded last time program ended,
            if (EditGame is null && this.ActiveMdiChild is null && WinAGISettings.AutoOpen && blnLastLoad) {
                // open mru0
                OpenMRUGame(0);
            }

        }
        private void btnNewLogic_Click(object sender, EventArgs e) {
            btnNewRes.DefaultItem = btnNewLogic;
            btnNewRes.Image = btnNewLogic.Image;

            //create new logic and enter edit mode
            NewLogic();

            //if (!GameLoaded)
            //  return;
            //frmLogicEdit frmNew = new frmLogicEdit { MdiParent = this };
            //frmNew.Show();
        }
        private void btnNewPicture_Click(object sender, EventArgs e) {
            MessageBox.Show("new picture...");
            btnNewRes.DefaultItem = btnNewPicture;
            btnNewRes.Image = btnNewPicture.Image;
            //create new picture and enter edit mode
            NewPicture();
        }
        private void btnOjects_Click(object sender, EventArgs e) {
            // let's test object list
        }
        private void btnWords_Click(object sender, EventArgs e) {
            // now let's test words
            //for (int i = 0; i <WordList.GroupCount; i++)
            //{
            //  Debug.Print($"Group {WordList.GroupN(i).GroupNum}: {WordList.GroupN(i).GroupName} ({WordList.GroupN(i).WordCount} words)");
            //}
            int i = 0, j = 0;
            for (i = 0; i < EditGame.WordList.WordCount; i++) {
                //
            }
            foreach (WordGroup tmpGrp in (IEnumerable<WordGroup>)EditGame.WordList) {
                j++;
            }
            Debug.Print($"There are {i} words in {j} groups in this list.");
        }
        private void btnNewSound_Click(object sender, EventArgs e) {
            //update default button image
            btnNewRes.DefaultItem = btnNewSound;
            btnNewRes.Image = btnNewSound.Image;

            //create new logic and enter edit mode
            NewSound();

            //if (!GameLoaded) return;
            //// show editor form
            //frmSoundEdit frmNew = new frmSoundEdit
            //{
            //  MdiParent = this
            //};
            //frmNew.Show();
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
            case 0: // game
                selRes = Game;
                break;
            case 1:
                // logics
                foreach (Logic tmpRes in EditGame.Logics) {
                    tmpItem = lstResources.Items.Add("l" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = tmpRes;
                    //set color based on compiled status;
                    tmpItem.ForeColor = tmpRes.Compiled ? Color.Black : Color.Red;
                }
                selRes = AGIResType.Logic;
                break;
            case 2:
                //pictures
                foreach (Picture tmpRes in EditGame.Pictures) {
                    tmpItem = lstResources.Items.Add("p" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = tmpRes;
                }
                selRes = AGIResType.Picture;
                break;
            case 3:
                //sounds
                foreach (Sound tmpRes in EditGame.Sounds) {
                    tmpItem = lstResources.Items.Add("s" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = tmpRes;
                }
                selRes = AGIResType.Sound;
                break;
            case 4:
                //views
                foreach (Engine.View tmpRes in EditGame.Views) {
                    tmpItem = lstResources.Items.Add("v" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                    tmpItem.Tag = tmpRes;
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
                AGIResource tmp = (AGIResource)(lstResources.SelectedItems[0].Tag);
                NewNum = tmp.Number;
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
                tmp = (AGIResource)(lstResources.SelectedItems[0].Tag);
                NewNum = tmp.Number;
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
                tmp = (AGIResource)(lstResources.SelectedItems[0].Tag);
                NewNum = tmp.Number;
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
                tmp = (AGIResource)(lstResources.SelectedItems[0].Tag);
                NewNum = tmp.Number;
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
        private void lstResources_DoubleClick(object sender, EventArgs e) {
            // make sure item is selected,

            // then edit it
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
                    EditGame.Sounds[SelResNum]?.Unload();
                    break;
                case AGIResType.View:
                    EditGame.Views[SelResNum].Unload();
                    break;
                }
            }


            //// this does not work as I'd hoped
            //foreach (Control c in propertyGrid1.Controls) {
            //    c.MouseDoubleClick -= propertyGrid1_MouseDoubleClick;
            //}


            propertyGrid1.SelectedObject = null;
            switch (NewResType) {
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
                if (NewResNum == -1) {
                    // logic header
                    LogicHdrProperties pLgcHdr = new();
                    propertyGrid1.SelectedObject = pLgcHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Logics[NewResNum].Load();
                    LogicProperties pLog = new(EditGame.Logics[NewResNum]);
                    // get reference to value field of the ReadOnly attribute for the IsRoom property 
                    Attribute readOnly = TypeDescriptor.GetProperties(pLog.GetType())["IsRoom"].Attributes[typeof(ReadOnlyAttribute)];
                    FieldInfo fi = readOnly.GetType().GetRuntimeFields().ToArray()[0];
                    // IsRoom is ReadOnly for logic0, ReadWrite for all others
                    if (NewResNum == 0) {
                        fi?.SetValue(readOnly, true);
                    }
                    else {
                        fi?.SetValue(readOnly, false);
                    }
                    propertyGrid1.SelectedObject = pLog;
                }
                break;
            case AGIResType.Picture:
                if (NewResNum == -1) {
                    //picture header
                    PictureHdrProperties pPicHdr = new();
                    propertyGrid1.SelectedObject = pPicHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Pictures[NewResNum].Load();
                    PictureProperties pPicture = new(EditGame.Pictures[NewResNum]);
                    propertyGrid1.SelectedObject = pPicture;
                }
                break;
            case AGIResType.Sound:
                if (NewResNum == -1) {
                    // sound header
                    SoundHdrProperties pSndHdr = new();
                    propertyGrid1.SelectedObject = pSndHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Sounds[NewResNum].Load();
                    SoundProperties pSound = new(EditGame.Sounds[NewResNum]);
                    propertyGrid1.SelectedObject = pSound;
                }
                break;
            case AGIResType.View:
                if (NewResNum == -1) {
                    //view header
                    ViewHdrProperties pViewHdr = new();
                    propertyGrid1.SelectedObject = pViewHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Views[NewResNum].Load();
                    ViewProperties pView = new(EditGame.Views[NewResNum]);
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
            if (WinAGISettings.ShowPreview) {
                // if update is requested
                if (WinAGISettings.ShowPreview && UpdatePreview) {
                    // load the preview item
                    PreviewWin.LoadPreview(NewResType, NewResNum);
                }
            }

            //update selection properties
            SelResType = NewResType;
            SelResNum = NewResNum;

            //if resource list is visible,
            if (WinAGISettings.ResListType != agiSettings.EResListType.None) {
                //add selected resource to navigation queue
                AddToQueue(SelResType, SelResNum < 0 ? 256 : SelResNum);
                // always disable forward button
                cmdForward.Enabled = false;
                // enable back button if at least two in queue
                cmdBack.Enabled = ResQPtr > 0;
                //if a logic is selected, and layout editor is active form
                if (SelResType == AGIResType.Logic) {
                    //if syncing the layout editor and the treeview list
                    if (WinAGISettings.LESync) {
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
        }
        private void btnNewView_Click(object sender, EventArgs e) {
            btnNewRes.DefaultItem = btnNewView;
            // update default button function
            btnNewRes.Image = btnNewView.Image;

            // create new view and enter edit mode
            NewView();

            // show editor form
            frmViewEdit frmNew = new() {
                MdiParent = this
            };
            frmNew.Show();
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
                switch (WinAGISettings.ResListType) {
                case agiSettings.EResListType.TreeList:
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
                case agiSettings.EResListType.ComboList:
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
                    switch (WinAGISettings.ResListType) {
                    case agiSettings.EResListType.TreeList:
                        // treelist
                        tvwResources.SelectedNode = HdrNode[(int)ResType];
                        tvwResources_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(tvwResources.SelectedNode, MouseButtons.None, 0, 0, 0));
                        break;
                    case agiSettings.EResListType.ComboList:
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
                switch (WinAGISettings.ResListType) {
                case agiSettings.EResListType.TreeList:
                    tvwResources.SelectedNode = HdrNode[(int)ResType].Nodes[strKey];
                    break;
                case agiSettings.EResListType.ComboList:
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
        bool ReadSettings() {
            double sngProp;
            double sngTop, sngLeft;
            double sngWidth, sngHeight;
            int i, lngNoCompVal;
            bool blnMax;

            //open the program settings  file
            try {
                WinAGISettingsList = new SettingsList(ProgramDir + "winagi.config", FileMode.OpenOrCreate);
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
                        WinAGISettingsList = new SettingsList(ProgramDir + "winagi.config", FileMode.Create);
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
            // GENERAL settings
            WinAGISettings.ShowSplashScreen = WinAGISettingsList.GetSetting(sGENERAL, "ShowSplashScreen", DEFAULT_SHOWSPLASHSCREEN);
            WinAGISettings.WarnCompile = WinAGISettingsList.GetSetting(sGENERAL, "WarnCompile", DEFAULT_WARNCOMPILE);
            WinAGISettings.NotifyCompSuccess = WinAGISettingsList.GetSetting(sGENERAL, "NotifyCompSuccess", DEFAULT_NOTIFYCOMPSUCCESS);
            WinAGISettings.NotifyCompWarn = WinAGISettingsList.GetSetting(sGENERAL, "NotifyCompWarn", DEFAULT_NOTIFYCOMPWARN);
            WinAGISettings.NotifyCompFail = WinAGISettingsList.GetSetting(sGENERAL, "NotifyCompFail", DEFAULT_NOTIFYCOMPFAIL);
            WinAGISettings.WarnDupGName = WinAGISettingsList.GetSetting(sGENERAL, "WarnDupGName", DEFAULT_WARNDUPGNAME);
            WinAGISettings.WarnDupGVal = WinAGISettingsList.GetSetting(sGENERAL, "WarnDupGVal", DEFAULT_WARNDUPGVAL);
            WinAGISettings.WarnInvalidStrVal = WinAGISettingsList.GetSetting(sGENERAL, "WarnInvalidStrVal", DEFAULT_WARNSTRVAL);
            WinAGISettings.WarnInvalidCtlVal = WinAGISettingsList.GetSetting(sGENERAL, "WarnInvalidCtlVal", DEFAULT_WARNCTLVAL);
            WinAGISettings.WarnResOvrd = WinAGISettingsList.GetSetting(sGENERAL, "WarnResOvrd", DEFAULT_WARNRESOVRD);
            WinAGISettings.WarnDupObj = WinAGISettingsList.GetSetting(sGENERAL, "WarnDupObj", DEFAULT_WARNDUPOBJ);
            WinAGISettings.WarnItem0 = WinAGISettingsList.GetSetting(sGENERAL, "WarnItem0", DEFAULT_WARNITEM0);
            WinAGISettings.DelBlankG = WinAGISettingsList.GetSetting(sGENERAL, "DelBlankG", DEFAULT_DELBLANKG);
            WinAGISettings.ShowPreview = WinAGISettingsList.GetSetting(sGENERAL, "ShowPreview", DEFAULT_SHOWPREVIEW);
            WinAGISettings.ShiftPreview = WinAGISettingsList.GetSetting(sGENERAL, "ShiftPreview", DEFAULT_SHIFTPREVIEW);
            WinAGISettings.HidePreview = WinAGISettingsList.GetSetting(sGENERAL, "HidePreview", DEFAULT_HIDEPREVIEW);
            WinAGISettings.ResListType = (agiSettings.EResListType)WinAGISettingsList.GetSetting(sGENERAL, "ResListType", (int)DEFAULT_RESLISTTYPE, typeof(agiSettings.EResListType));
            //validate treetype
            if (WinAGISettings.ResListType < 0 || (int)WinAGISettings.ResListType > 2) {
                //use default
                WinAGISettings.ResListType = DEFAULT_RESLISTTYPE;
                WinAGISettingsList.WriteSetting(sGENERAL, "ResListType", WinAGISettings.ResListType.ToString(), "");
            }
            WinAGISettings.AutoExport = WinAGISettingsList.GetSetting(sGENERAL, "AutoExport", DEFAULT_AUTOEXPORT);
            WinAGISettings.AutoUpdateDefines = WinAGISettingsList.GetSetting(sLOGICS, "AutoUpdateDefines", DEFAULT_AUTOUPDATEDEFINES);
            WinAGISettings.AutoUpdateResDefs = WinAGISettingsList.GetSetting(sLOGICS, "AutoUpdateResDefs", DEFAULT_AUTOUPDATERESDEFS);
            WinAGISettings.AskExport = WinAGISettingsList.GetSetting(sGENERAL, "AskExport", DEFAULT_ASKEXPORT);
            WinAGISettings.AskRemove = WinAGISettingsList.GetSetting(sGENERAL, "AskRemove", DEFAULT_ASKREMOVE);
            WinAGISettings.OpenNew = WinAGISettingsList.GetSetting(sGENERAL, "OpenNew", DEFAULT_OPENNEW);
            WinAGISettings.RenameDelRes = WinAGISettingsList.GetSetting(sGENERAL, "RenameDelRes", DEFAULT_RENAMEDELRES);
            WinAGISettings.AutoWarn = WinAGISettingsList.GetSetting(sLOGICS, "AutoWarn", DEFAULT_AUTOWARN);
            //(DefResDir is not an element of settings; it's a WinAGI property)
            DefResDir = WinAGISettingsList.GetSetting(sGENERAL, "DefResDir", "src").Trim();
            //validate directory
            if (DefResDir == "") {
                DefResDir = "src";
            }
            else if ((" \\/:*?\"<>|").Any(DefResDir.Contains)) {
                DefResDir = "src";
            }
            else if (DefResDir.Any(ch => ch > 127 || ch < 32)) {
                DefResDir = "src";
            }
            WinAGISettings.MaxSO = WinAGISettingsList.GetSetting(sGENERAL, "MaxSO", DEFAULT_MAXSO);
            if (WinAGISettings.MaxSO > 255)
                WinAGISettings.MaxSO = 255;
            if (WinAGISettings.MaxSO < 1)
                WinAGISettings.MaxSO = 1;
            //(maxvolsize is an AGIGame property, not a WinAGI setting)  ? but this is the default value if one is
            //not provided in a game, right?
            WinAGISettings.MaxVol0Size = WinAGISettingsList.GetSetting(sGENERAL, "MaxVol0", 1047552);
            if (WinAGISettings.MaxVol0Size < 32768)
                WinAGISettings.MaxVol0Size = 32768;
            if (WinAGISettings.MaxVol0Size > 1047552)
                WinAGISettings.MaxVol0Size = 1047552;
            DefMaxVol0Size = WinAGISettings.MaxVol0Size;
            //get help window parent
            if (WinAGISettingsList.GetSetting(sGENERAL, "DockHelpWindow", true)) {
                HelpParent = this;
            }
            // default codepage for extended characters
            WinAGISettings.DefCP = WinAGISettingsList.GetSetting(sGENERAL, "DefCP", 437);
            switch (WinAGISettings.DefCP) {
            case 437 or 850 or 852 or 855 or 857 or 858 or 860 or 861 or 863 or 869:
                // OK
                break;
            default:
                // force to default
                WinAGISettings.DefCP = 437;
                break;
            }
            // use the default as current game codepage
            SessionCodePage = Encoding.GetEncoding(WinAGISettings.DefCP);
            //RESFORMAT settings
            WinAGISettings.ShowResNum = WinAGISettingsList.GetSetting("ResFormat", "ShowResNum", DEFAULT_SHOWRESNUM);
            WinAGISettings.IncludeResNum = WinAGISettingsList.GetSetting("ResFormat", "IncludeResNum", DEFAULT_INCLUDERESNUM);
            WinAGISettings.ResFormat.NameCase = WinAGISettingsList.GetSetting("ResFormat", "NameCase", (int)DEFAULT_NAMECASE);
            if ((int)WinAGISettings.ResFormat.NameCase < 0 || (int)WinAGISettings.ResFormat.NameCase > 2) {
                WinAGISettings.ResFormat.NameCase = DEFAULT_NAMECASE;
            }
            WinAGISettings.ResFormat.Separator = Left(WinAGISettingsList.GetSetting("ResFormat", "Separator", DEFAULT_SEPARATOR), 1);
            WinAGISettings.ResFormat.NumFormat = WinAGISettingsList.GetSetting("ResFormat", "NumFormat", DEFAULT_NUMFORMAT);
            //GLOBAL EDITOR
            WinAGISettings.GlobalUndo = WinAGISettingsList.GetSetting("Globals", "GlobalUndo", DEFAULT_GLBUNDO);
            WinAGISettings.GEShowComment = WinAGISettingsList.GetSetting("Globals", "ShowCommentColumn", DEFAULT_GESHOWCMT);
            WinAGISettings.GENameFrac = WinAGISettingsList.GetSetting("Globals", "GENameFrac", (double)0);
            WinAGISettings.GEValFrac = WinAGISettingsList.GetSetting("Globals", "GEValFrac", (double)0);

            //get overrides of reserved defines, if there are any
            GetResDefOverrides();

            //LAYOUT
            WinAGISettings.DefUseLE = WinAGISettingsList.GetSetting(sLAYOUT, "DefUseLE", DEFAULT_DEFUSELE);
            WinAGISettings.LEPages = WinAGISettingsList.GetSetting(sLAYOUT, "PageBoundaries", DEFAULT_LEPAGES);
            WinAGISettings.LEDelPicToo = WinAGISettingsList.GetSetting(sLAYOUT, "DelPicToo", DEFAULT_LEWARNDELETE);
            WinAGISettings.LEShowPics = WinAGISettingsList.GetSetting(sLAYOUT, "ShowPics", DEFAULT_LESHOWPICS);
            WinAGISettings.LESync = WinAGISettingsList.GetSetting(sLAYOUT, "Sync", DEFAULT_LESYNC);
            WinAGISettings.LEUseGrid = WinAGISettingsList.GetSetting(sLAYOUT, "UseGrid", DEFAULT_LEUSEGRID);
            WinAGISettings.LEGrid = WinAGISettingsList.GetSetting(sLAYOUT, "GridSize", DEFAULT_LEGRID);
            WinAGISettings.LEGrid = Math.Round(WinAGISettings.LEGrid, 2);
            if (WinAGISettings.LEGrid > 1)
                WinAGISettings.LEGrid = 1;
            if (WinAGISettings.LEGrid < 0.05)
                WinAGISettings.LEGrid = 0.05;
            WinAGISettings.LEZoom = WinAGISettingsList.GetSetting(sLAYOUT, "Zoom", DEFAULT_LEZOOM);
            //get editor colors
            WinAGISettings.LEColors.Room.Edge = WinAGISettingsList.GetSetting(sLAYOUT, "RoomEdgeColor", DEFAULT_LEROOM_EDGE);
            WinAGISettings.LEColors.Room.Fill = WinAGISettingsList.GetSetting(sLAYOUT, "RoomFillColor", DEFAULT_LEROOM_FILL);
            WinAGISettings.LEColors.TransPt.Edge = WinAGISettingsList.GetSetting(sLAYOUT, "TransEdgeColor", DEFAULT_LETRANSPT_EDGE);
            WinAGISettings.LEColors.TransPt.Fill = WinAGISettingsList.GetSetting(sLAYOUT, "TransFillColor", DEFAULT_LETRANSPT_FILL);
            WinAGISettings.LEColors.ErrPt.Edge = WinAGISettingsList.GetSetting(sLAYOUT, "ErrEdgeColor", DEFAULT_LEERR_EDGE);
            WinAGISettings.LEColors.ErrPt.Fill = WinAGISettingsList.GetSetting(sLAYOUT, "ErrFillColor", DEFAULT_LEERR_FILL);
            WinAGISettings.LEColors.Cmt.Edge = WinAGISettingsList.GetSetting(sLAYOUT, "CmtEdgeColor", DEFAULT_LECMT_EDGE);
            WinAGISettings.LEColors.Cmt.Fill = WinAGISettingsList.GetSetting(sLAYOUT, "CmtFillColor", DEFAULT_LECMT_FILL);
            WinAGISettings.LEColors.Edge = WinAGISettingsList.GetSetting(sLAYOUT, "ExitEdgeColor", DEFAULT_LEEXIT_EDGE);
            WinAGISettings.LEColors.Other = WinAGISettingsList.GetSetting(sLAYOUT, "ExitOtherColor", DEFAULT_LEEXIT_OTHERS);
            //LOGICS
            WinAGISettings.HighlightLogic = WinAGISettingsList.GetSetting(sLOGICS, "HighlightLogic", DEFAULT_HILITELOG);
            WinAGISettings.HighlightText = WinAGISettingsList.GetSetting(sLOGICS, "HighlightText", DEFAULT_HILITETEXT);
            WinAGISettings.LogicTabWidth = WinAGISettingsList.GetSetting(sLOGICS, "TabWidth", DEFAULT_LOGICTABWIDTH);
            if (WinAGISettings.LogicTabWidth < 1)
                WinAGISettings.LogicTabWidth = 1;
            if (WinAGISettings.LogicTabWidth > 32)
                WinAGISettings.LogicTabWidth = 32;
            WinAGISettings.MaximizeLogics = WinAGISettingsList.GetSetting(sLOGICS, "MaximizeLogics", DEFAULT_MAXIMIZELOGICS);
            WinAGISettings.AutoQuickInfo = WinAGISettingsList.GetSetting(sLOGICS, "AutoQuickInfo", DEFAULT_AUTOQUICKINFO);
            WinAGISettings.ShowDefTips = WinAGISettingsList.GetSetting(sLOGICS, "ShowDefTips", DEFAULT_SHOWDEFTIPS);
            WinAGISettings.EFontName = WinAGISettingsList.GetSetting(sLOGICS, "EditorFontName", DEFAULT_EFONTNAME);
            i = 0;
            foreach (FontFamily font in System.Drawing.FontFamily.Families) {
                if (font.Name.Equals(WinAGISettings.EFontName, StringComparison.OrdinalIgnoreCase)) {
                    //found
                    i = 1;
                    break;
                }
            }
            // not found?
            if (i == 1)
                WinAGISettings.EFontName = DEFAULT_EFONTNAME;
            WinAGISettings.EFontSize = WinAGISettingsList.GetSetting(sLOGICS, "EditorFontSize", DEFAULT_EFONTSIZE);
            if (WinAGISettings.EFontSize < 8)
                WinAGISettings.EFontSize = 8;
            if (WinAGISettings.EFontSize > 24)
                WinAGISettings.EFontSize = 24;
            WinAGISettings.PFontName = WinAGISettingsList.GetSetting(sLOGICS, "PreviewFontName", DEFAULT_PFONTNAME);
            i = 0;
            foreach (FontFamily font in System.Drawing.FontFamily.Families) {
                if (font.Name.Equals(WinAGISettings.PFontName, StringComparison.OrdinalIgnoreCase)) {
                    //found
                    i = 1;
                    break;
                }
            }
            if (i == 1)
                WinAGISettings.PFontName = DEFAULT_PFONTNAME;
            WinAGISettings.PFontSize = WinAGISettingsList.GetSetting(sLOGICS, "PreviewFontSize", DEFAULT_PFONTSIZE);
            if (WinAGISettings.PFontSize < 6)
                WinAGISettings.PFontSize = 6;
            if (WinAGISettings.PFontSize > 36)
                WinAGISettings.PFontSize = 36;
            WinAGISettings.OpenOnErr = WinAGISettingsList.GetSetting(sLOGICS, "OpenOnErr", DEFAULT_OPENONERR);
            if (WinAGISettings.OpenOnErr < 0)
                WinAGISettings.OpenOnErr = 0;
            if (WinAGISettings.OpenOnErr > 2)
                WinAGISettings.OpenOnErr = 2;
            WinAGISettings.SaveOnCompile = WinAGISettingsList.GetSetting(sLOGICS, "SaveOnComp", DEFAULT_SAVEONCOMP);
            if (WinAGISettings.SaveOnCompile < 0)
                WinAGISettings.SaveOnCompile = 0;
            if (WinAGISettings.SaveOnCompile > 2)
                WinAGISettings.SaveOnCompile = 2;
            WinAGISettings.CompileOnRun = WinAGISettingsList.GetSetting(sLOGICS, "CompOnRun", DEFAULT_COMPONRUN);
            if (WinAGISettings.CompileOnRun < 0)
                WinAGISettings.CompileOnRun = 0;
            if (WinAGISettings.CompileOnRun > 2)
                WinAGISettings.CompileOnRun = 2;
            WinAGISettings.LogicUndo = WinAGISettingsList.GetSetting(sLOGICS, "LogicUndo", DEFAULT_LOGICUNDO);
            if (WinAGISettings.LogicUndo < -1)
                WinAGISettings.LogicUndo = -1;
            WinAGISettings.WarnMsgs = WinAGISettingsList.GetSetting(sLOGICS, "WarnMsgs", DEFAULT_WARNMSGS);
            if (WinAGISettings.WarnMsgs < 0)
                WinAGISettings.WarnMsgs = 0;
            if (WinAGISettings.WarnMsgs > 2)
                WinAGISettings.WarnMsgs = 2;
            LogicCompiler.ErrorLevel = (LogicErrorLevel)WinAGISettingsList.GetSetting(sLOGICS, "ErrorLevel", (int)DEFAULT_ERRORLEVEL, typeof(LogicErrorLevel));
            if (LogicCompiler.ErrorLevel < 0)
                LogicCompiler.ErrorLevel = LogicErrorLevel.Low;
            if ((int)LogicCompiler.ErrorLevel > 2)
                LogicCompiler.ErrorLevel = LogicErrorLevel.High;
            WinAGISettings.DefUseResDef = WinAGISettingsList.GetSetting(sLOGICS, "DefUseResDef", DEFAULT_DEFUSERESDEF);
            WinAGISettings.Snippets = WinAGISettingsList.GetSetting(sLOGICS, "Snippets", DEFAULT_SNIPPETS);
            //SYNTAXHIGHLIGHTFORMAT
            WinAGISettings.HColor[0] = WinAGISettingsList.GetSetting(sSHFORMAT, "NormalColor", DEFAULT_HNRMCOLOR);
            WinAGISettings.HColor[1] = WinAGISettingsList.GetSetting(sSHFORMAT, "KeywordColor", DEFAULT_HKEYCOLOR);
            WinAGISettings.HColor[2] = WinAGISettingsList.GetSetting(sSHFORMAT, "IdentifierColor", DEFAULT_HIDTCOLOR);
            WinAGISettings.HColor[3] = WinAGISettingsList.GetSetting(sSHFORMAT, "StringColor", DEFAULT_HSTRCOLOR);
            WinAGISettings.HColor[4] = WinAGISettingsList.GetSetting(sSHFORMAT, "CommentColor", DEFAULT_HCMTCOLOR);
            WinAGISettings.HColor[5] = WinAGISettingsList.GetSetting(sSHFORMAT, "BackColor", DEFAULT_HBKGCOLOR);
            WinAGISettings.HBold[0] = WinAGISettingsList.GetSetting(sSHFORMAT, "NormalBold", DEFAULT_HNRMBOLD);
            WinAGISettings.HBold[1] = WinAGISettingsList.GetSetting(sSHFORMAT, "KeywordBold", DEFAULT_HKEYBOLD);
            WinAGISettings.HBold[2] = WinAGISettingsList.GetSetting(sSHFORMAT, "IdentifierBold", DEFAULT_HIDTBOLD);
            WinAGISettings.HBold[3] = WinAGISettingsList.GetSetting(sSHFORMAT, "StringBold", DEFAULT_HSTRBOLD);
            WinAGISettings.HBold[4] = WinAGISettingsList.GetSetting(sSHFORMAT, "CommentBold", DEFAULT_HCMTBOLD);
            WinAGISettings.HItalic[0] = WinAGISettingsList.GetSetting(sSHFORMAT, "NormalItalic", DEFAULT_HNRMITALIC);
            WinAGISettings.HItalic[1] = WinAGISettingsList.GetSetting(sSHFORMAT, "KeywordItalic", DEFAULT_HKEYITALIC);
            WinAGISettings.HItalic[2] = WinAGISettingsList.GetSetting(sSHFORMAT, "IdentifierItalic", DEFAULT_HIDTITALIC);
            WinAGISettings.HItalic[3] = WinAGISettingsList.GetSetting(sSHFORMAT, "StringItalic", DEFAULT_HSTRITALIC);
            WinAGISettings.HItalic[4] = WinAGISettingsList.GetSetting(sSHFORMAT, "CommentItalic", DEFAULT_HCMTITALIC);

            //PICTURES
            WinAGISettings.PicScale.Edit = WinAGISettingsList.GetSetting(sPICTURES, "EditorScale", DEFAULT_PICSCALE_EDIT);
            if (WinAGISettings.PicScale.Edit < 1)
                WinAGISettings.PicScale.Edit = 1;
            if (WinAGISettings.PicScale.Edit > 4)
                WinAGISettings.PicScale.Edit = 4;
            WinAGISettings.PicScale.Preview = WinAGISettingsList.GetSetting(sPICTURES, "PreviewScale", DEFAULT_PICSCALE_PREVIEW);
            if (WinAGISettings.PicScale.Preview < 1)
                WinAGISettings.PicScale.Preview = 1;
            if (WinAGISettings.PicScale.Preview > 4)
                WinAGISettings.PicScale.Preview = 4;
            WinAGISettings.PicUndo = WinAGISettingsList.GetSetting(sPICTURES, "PicUndo", DEFAULT_PICUNDO);
            if (WinAGISettings.PicUndo < -1)
                WinAGISettings.PicUndo = -1;
            WinAGISettings.ShowBands = WinAGISettingsList.GetSetting(sPICTURES, "ShowBands", DEFAULT_SHOWBANDS);
            WinAGISettings.SplitWindow = WinAGISettingsList.GetSetting(sPICTURES, "SplitWindow", DEFAULT_SPLITWINDOW);
            WinAGISettings.CursorMode = (EPicCursorMode)WinAGISettingsList.GetSetting(sPICTURES, "CursorMode", DEFAULT_CURSORMODE, typeof(EPicCursorMode));

            //PICTEST
            //these settings get loaded with each picedit form (and the logic template, which uses
            //the Horizion setting) that gets loaded; no need to
            //retrieve them here

            //SOUNDS
            WinAGISettings.ShowKybd = WinAGISettingsList.GetSetting(sSOUNDS, "ShowKeyboard", DEFAULT_SHOWKYBD);
            WinAGISettings.ShowNotes = WinAGISettingsList.GetSetting(sSOUNDS, "ShowNotes", DEFAULT_SHOWNOTES);
            WinAGISettings.OneTrack = WinAGISettingsList.GetSetting(sSOUNDS, "OneTrack", DEFAULT_ONETRACK);
            WinAGISettings.SndUndo = WinAGISettingsList.GetSetting(sSOUNDS, "SndUndo", DEFAULT_SNDUNDO);
            if (WinAGISettings.SndUndo < -1)
                WinAGISettings.SndUndo = -1;
            WinAGISettings.SndZoom = WinAGISettingsList.GetSetting(sSOUNDS, "Zoom", DEFAULT_SNDZOOM);
            if (WinAGISettings.SndZoom < 1)
                WinAGISettings.SndZoom = 1;
            if (WinAGISettings.SndZoom > 3)
                WinAGISettings.SndZoom = 3;
            WinAGISettings.NoMIDI = WinAGISettingsList.GetSetting(sSOUNDS, "NoMIDI", DEFAULT_NOMIDI);
            i = WinAGISettingsList.GetSetting(sSOUNDS, "Instrument0", DEFAULT_DEFINST);
            if (i > 255)
                i = 255;
            if (i < 0)
                i = 0;
            WinAGISettings.DefInst0 = (byte)i;
            i = WinAGISettingsList.GetSetting(sSOUNDS, "Instrument1", DEFAULT_DEFINST);
            if (i > 255)
                i = 255;
            if (i < 0)
                i = 0;
            WinAGISettings.DefInst1 = (byte)i;
            i = WinAGISettingsList.GetSetting(sSOUNDS, "Instrument2", DEFAULT_DEFINST);
            if (i > 255)
                i = 255;
            if (i < 0)
                i = 0;
            WinAGISettings.DefInst2 = (byte)i;
            WinAGISettings.DefMute0 = WinAGISettingsList.GetSetting(sSOUNDS, "Mute0", DEFAULT_DEFMUTE);
            WinAGISettings.DefMute1 = WinAGISettingsList.GetSetting(sSOUNDS, "Mute1", DEFAULT_DEFMUTE);
            WinAGISettings.DefMute2 = WinAGISettingsList.GetSetting(sSOUNDS, "Mute2", DEFAULT_DEFMUTE);
            WinAGISettings.DefMute3 = WinAGISettingsList.GetSetting(sSOUNDS, "Mute3", DEFAULT_DEFMUTE);

            //VIEWS
            WinAGISettings.ViewScale.Edit = WinAGISettingsList.GetSetting(sVIEWS, "EditorScale", DEFAULT_VIEWSCALE_EDIT);
            if (WinAGISettings.ViewScale.Edit < 1)
                WinAGISettings.ViewScale.Edit = 1;
            if (WinAGISettings.ViewScale.Edit > 10)
                WinAGISettings.ViewScale.Edit = 10;
            WinAGISettings.ViewScale.Preview = WinAGISettingsList.GetSetting(sVIEWS, "PreviewScale", DEFAULT_VIEWSCALE_PREVIEW);
            if (WinAGISettings.ViewScale.Preview < 1)
                WinAGISettings.ViewScale.Preview = 1;
            if (WinAGISettings.ViewScale.Preview > 10)
                WinAGISettings.ViewScale.Preview = 10;
            WinAGISettings.ViewAlignH = WinAGISettingsList.GetSetting(sVIEWS, "AlignH", DEFAULT_VIEWALIGNH);
            if (WinAGISettings.ViewAlignH < 0)
                WinAGISettings.ViewAlignH = 0;
            if (WinAGISettings.ViewAlignH > 2)
                WinAGISettings.ViewAlignH = 2;
            WinAGISettings.ViewAlignV = WinAGISettingsList.GetSetting(sVIEWS, "AlignV", DEFAULT_VIEWALIGNV);
            if (WinAGISettings.ViewAlignV < 0)
                WinAGISettings.ViewAlignV = 0;
            if (WinAGISettings.ViewAlignV > 2)
                WinAGISettings.ViewAlignV = 2;
            WinAGISettings.ViewUndo = WinAGISettingsList.GetSetting(sVIEWS, "ViewUndo", DEFAULT_VIEWUNDO);
            if (WinAGISettings.ViewUndo < -1)
                WinAGISettings.ViewUndo = -1;
            i = WinAGISettingsList.GetSetting(sVIEWS, "DefaultCelHeight", DEFAULT_DEFCELH);
            if (i < 1)
                i = 1;
            if (i > 167)
                i = 167;
            WinAGISettings.DefCelH = (byte)i;
            i = WinAGISettingsList.GetSetting(sVIEWS, "DefaultCelWidth", DEFAULT_DEFCELW);
            if (i < 1) i = 1;
            if (i > 167)
                i = 160;
            WinAGISettings.DefCelW = (byte)i;
            WinAGISettings.DefVColor1 = WinAGISettingsList.GetSetting(sVIEWS, "Color1", (int)DEFAULT_DEFVCOLOR1);
            if (WinAGISettings.DefVColor1 < 0)
                WinAGISettings.DefVColor1 = 0;
            if (WinAGISettings.DefVColor1 > 15)
                WinAGISettings.DefVColor1 = 15;
            WinAGISettings.DefVColor2 = WinAGISettingsList.GetSetting(sVIEWS, "Color2", (int)DEFAULT_DEFVCOLOR2);
            if (WinAGISettings.DefVColor2 < 0)
                WinAGISettings.DefVColor2 = 0;
            if (WinAGISettings.DefVColor2 > 15)
                WinAGISettings.DefVColor2 = 15;
            WinAGISettings.ShowVEPrev = WinAGISettingsList.GetSetting(sVIEWS, "ShowVEPreview", DEFAULT_SHOWVEPREV);
            WinAGISettings.ShowGrid = WinAGISettingsList.GetSetting(sVIEWS, "ShowEditGrid", DEFAULT_SHOWGRID);

            // DECOMPILER
            LogicDecoder.ShowAllMessages = WinAGISettingsList.GetSetting(sDECOMPILER, "ShowAllMessages", DEFAULT_SHOWALLMSGS);
            LogicDecoder.MsgsByNumber = WinAGISettingsList.GetSetting(sDECOMPILER, "MsgsByNum", DEFAULT_MSGSBYNUM);
            LogicDecoder.SpecialSyntax = WinAGISettingsList.GetSetting(sDECOMPILER, "SpecialSyntax", DEFAULT_SPECIALSYNTAX);
            LogicDecoder.ReservedAsText = WinAGISettingsList.GetSetting(sDECOMPILER, "ReservedAsText", DEFAULT_SHOWRESVARS);
            LogicDecoder.DefaultSrcExt = WinAGISettingsList.GetSetting(sDECOMPILER, "DefaultSrcExt", DEFAULT_DEFSRCEXT);

            // COMPILER
            LogicCompiler.UseReservedNames = WinAGISettings.DefUseResDef;

            //get property window height
            // TODO: add code to adjust size of property control
            PropRowCount = WinAGISettingsList.GetSetting(sPOSITION, "PropRowCount", 4);
            if (PropRowCount < 3)
                PropRowCount = MIN_SPLIT_RES;
            if (PropRowCount > 10)
                PropRowCount = MAX_SPLIT_RES;

            //get main window state
            blnMax = WinAGISettingsList.GetSetting(sPOSITION, "WindowMax", false);
            //get main window position
            sngLeft = WinAGISettingsList.GetSetting(sPOSITION, "Left", Screen.PrimaryScreen.Bounds.Width * 0.15);
            sngTop = WinAGISettingsList.GetSetting(sPOSITION, "Top", Screen.PrimaryScreen.Bounds.Height * 0.15);
            sngWidth = WinAGISettingsList.GetSetting(sPOSITION, "Width", Screen.PrimaryScreen.Bounds.Width * 0.7);
            sngHeight = WinAGISettingsList.GetSetting(sPOSITION, "Height", Screen.PrimaryScreen.Bounds.Height * 0.7);
            // min width
            if (sngWidth < 360) {
                sngWidth = 360;
            }
            // max width
            if (sngWidth > SystemInformation.VirtualScreen.Width) {
                sngWidth = SystemInformation.VirtualScreen.Width;
            }
            // min height
            if (sngHeight < 361) {
                sngHeight = 361;
            }
            // max height
            if (sngHeight > SystemInformation.VirtualScreen.Height) {
                sngHeight = SystemInformation.VirtualScreen.Height;
            }
            // min left pos
            if (sngLeft < 0) {
                sngLeft = 0;
            }
            // max left pos
            if (sngLeft > SystemInformation.VirtualScreen.Width * 0.85) {
                sngLeft = SystemInformation.VirtualScreen.Width * 0.85;
            }
            // min top pos
            if (sngTop < 0) {
                sngTop = 0;
            }
            // max top pos
            if (sngTop > SystemInformation.VirtualScreen.Height * 0.85) {
                sngTop = SystemInformation.VirtualScreen.Height * 0.85;
            }
            //now move the form
            MDIMain.Bounds = new Rectangle((int)sngLeft, (int)sngTop, (int)sngWidth, (int)sngHeight);
            //if maximized
            if (blnMax) {
                //maximize window
                WindowState = FormWindowState.Maximized;
            }
            //get resource window width
            sngProp = WinAGISettingsList.GetSetting(sPOSITION, "ResourceWidth", MIN_SPLIT_V * 1.5);
            if (sngProp < MIN_SPLIT_V) {
                sngProp = MIN_SPLIT_V;
            }
            else if (sngProp > MDIMain.Bounds.Width - MIN_SPLIT_V) {
                sngProp = MDIMain.Bounds.Width - MIN_SPLIT_V;
            }
            //set width
            pnlResources.Width = (int)sngProp;

            // get mru settings
            WinAGISettings.AutoOpen = WinAGISettingsList.GetSetting(sMRULIST, "AutoOpen", DEFAULT_AUTOOPEN);
            for (i = 0; i < 4; i++) {
                strMRU[i] = WinAGISettingsList.GetSetting(sMRULIST, "MRUGame" + (i + 1), "");
                //if one exists
                if (strMRU[i].Length != 0) {
                    //add it to menu
                    mnuGame.DropDownItems["mnuGMRU" + i].Text = CompactPath(strMRU[i], 60);
                    mnuGame.DropDownItems["mnuGMRU" + i].Visible = true;
                    mnuGMRUBar.Visible = true;
                }
                else {
                    //stop loading list at first blank
                    break;
                }
            }

            // get tools info
            bool blnTools = false;
            for (i = 1; i <= 6; i++) {
                string strCaption = WinAGISettingsList.GetSetting(sTOOLS, "Caption" + i, "");
                string strTool = WinAGISettingsList.GetSetting(sTOOLS, "Source" + i, "");
                // update tools menu
                if (strCaption.Length > 0 && strTool.Length > 0) {
                    mnuTools.DropDownItems["mnuTCustom" + i].Visible = true;
                    mnuTools.DropDownItems["mnuTCustom" + i].Text = strCaption;
                    mnuTools.DropDownItems["mnuTCustom" + i].Tag = strTool;
                    blnTools = true;
                }
                mnuTSep2.Visible = blnTools;
            }

            //error warning settings
            lngNoCompVal = WinAGISettingsList.GetSetting(sLOGICS, "NoCompWarn0", 0);
            for (i = 1; i <= 30; i++) {
                LogicCompiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << i)) == (1 << i));
            }
            lngNoCompVal = WinAGISettingsList.GetSetting(sLOGICS, "NoCompWarn1", 0);
            for (i = 31; i <= 60; i++) {
                LogicCompiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << (i - 30))) == 1 << (i - 30));
            }
            lngNoCompVal = WinAGISettingsList.GetSetting(sLOGICS, "NoCompWarn2", 0);
            for (i = 61; i <= 90; i++) {
                LogicCompiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << (i - 60))) == 1 << (i - 60));
            }
            lngNoCompVal = WinAGISettingsList.GetSetting(sLOGICS, "NoCompWarn3", 0);
            for (i = 91; i < LogicCompiler.WARNCOUNT; i++) {
                LogicCompiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << (i - 90))) == 1 << (i - 90));
            }
            return true;
        }
        public void SaveSettings() {  //saves game settings to config file
            int i, lngCompVal;
            //if main form is maximized
            if (MDIMain.WindowState == FormWindowState.Maximized) {
                //save Max Value only
                WinAGISettingsList.WriteSetting(sPOSITION, "WindowMax", true);
            }
            else {
                //save all window settings
                WinAGISettingsList.WriteSetting(sPOSITION, "Top", MDIMain.Top.ToString());
                WinAGISettingsList.WriteSetting(sPOSITION, "Left", MDIMain.Left.ToString());
                WinAGISettingsList.WriteSetting(sPOSITION, "Width", MDIMain.Width.ToString());
                WinAGISettingsList.WriteSetting(sPOSITION, "Height", MDIMain.Height.ToString());
                WinAGISettingsList.WriteSetting(sPOSITION, "WindowMax", false.ToString());
            }
            //save other position settings
            WinAGISettingsList.WriteSetting(sPOSITION, "PropRowCount", PropRowCount);
            //save mru settings
            for (i = 0; i < 4; i++) {
                WinAGISettingsList.WriteSetting(sMRULIST, "MRUGame" + (i + 1), strMRU[i]);
            }
            //save resource pane width
            WinAGISettingsList.WriteSetting(sPOSITION, "ResourceWidth", pnlResources.Width);
            //save other general settings
            //            WinAGISettingsList.WriteSetting(sGENERAL, "DockHelpWindow", HelpParent == this.Handle);
            // for warnings, create a bitfield to mark which are being ignored
            lngCompVal = 0;
            for (i = 1; i <= 30; i++) {
                lngCompVal |= (LogicCompiler.IgnoreWarning(5000 + i) ? 1 << i : 0);
            }
            WinAGISettingsList.WriteSetting(sLOGICS, "NoCompWarn0", lngCompVal);
            lngCompVal = 0;
            for (i = 1; i <= 30; i++) {
                lngCompVal |= (LogicCompiler.IgnoreWarning(5030 + i) ? 1 << i : 0);
            }
            WinAGISettingsList.WriteSetting(sLOGICS, "NoCompWarn1", lngCompVal);
            lngCompVal = 0;
            for (i = 1; i <= 30; i++) {
                lngCompVal |= (LogicCompiler.IgnoreWarning(5060 + i) ? 1 << i : 0);
            }
            WinAGISettingsList.WriteSetting(sLOGICS, "NoCompWarn2", lngCompVal);
            lngCompVal = 0;
            for (i = 1; i < (LogicCompiler.WARNCOUNT % 30); i++) {
                lngCompVal |= (LogicCompiler.IgnoreWarning(5090 + i) ? 1 << i : 0);
            }
            WinAGISettingsList.WriteSetting(sLOGICS, "NoCompWarn3", lngCompVal);
            //save to file
            WinAGISettingsList.Save();
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
            //if not the same as the current item
            if (e.Node != tvwResources.SelectedNode) {
                //select it
                tvwResources_NodeMouseClick(null, e);
            }
            //edit the selected item
            //*//    EditResource(SelResType, SelResNum);
        }
        public void HideResTree() {
            pnlResources.Visible = false;
            splitResource.Visible = false;
        }

        void CheckCmd() {
            return;
            /*
            string[] args = Environment.GetCommandLineArgs();
            frmObjectEdit frmNewO;
            frmWordsEdit frmNewW;

            if (args.Length == 0) {
              return;
            }
            //only first arg is used; extras are ignored

            //ensure no quotes
            if (args[0][0] == '"') {
              args[0] = Mid(args[0], 2, args[0].Length - 2);
            }

            //check for OBJECT or WORDS.TOK file:
            if (Path.GetFileName(args[0]).Equals("OBJECT", StringComparison.OrdinalIgnoreCase)) {
              //open a object resource
              frmNewO = new frmObjectEdit();
              try {
                frmNewO.LoadObjects(args[0]);
                frmNewO.Show();
              }
              catch (Exception) {
                frmNewO.Close();
              }

            } else if (Path.GetFileName(args[0]).Equals("WORDS.TOK", StringComparison.OrdinalIgnoreCase)) {
              //open a word resource
              frmNewW = new frmWordsEdit();
              try {
                frmNewW.LoadWords(args[0]);
                frmNewW.Show();
              }
              catch (Exception) {
                frmNewW.Close();
              }
            } else {
              //check for a file
              switch (Right(args[0], 4).ToLower()) {
              case ".wag":
                //open a game
                OpenWAGFile(args[0]);
                break;
              case ".wal":
                //layout files can't be opened by command line anymore
                //////      //open a game; then open layout editor
                break;
              case ".lgc":
              case ".agl":
                //open a logic source text file or logic resource
                try {
                  NewLogic(args[0]);
                  NewLogic(args[0]);
                }
                catch (Exception) {
                  //ignore any error
                }
                break;
              case ".ago":
                //open a object resource
                frmNewO = new frmObjectEdit();
                try {
                  frmNewO.LoadObjects(args[0]);
                  frmNewO.Show();
                }
                catch (Exception) {
                  frmNewO.Close();
                }
                break;
              case ".agp":
                //open a picture resource
                try {
                  NewPicture(args[0]);
                }
                catch (Exception) {
                  //ignore errors
                }
                break;
              case ".ags":
                //open a sound resource
                try {
                  NewSound(args[0]);
                }
                catch (Exception) {
                  //ignore errors
                }
                break;
              case ".agv":
                //open a view resource
                try {
                  NewView(args[0]);
                }
                catch (Exception) {
                  //ignore errors
                }
                break;
              case ".agw":
                //open a word resource
                frmNewW = new frmWordsEdit();
                try {
                  frmNewW.LoadWords(args[0]);
                  frmNewW.Show();
                }
                catch (Exception) {
                  frmNewW.Close();
                }
                break;

              default:
                //ignore anything else
                break;
              }
            }
            */
        }
        internal void mnuGClose_Click(object sender, EventArgs e) {
            //if closed ok,
            if (CloseThisGame()) {
                //reset last item in  holder
                LastNodeName = "";
            }
        }

        internal void mnuRRenumber_Click(object sender, EventArgs e) {
            GetNewNumber(SelResType, (byte)SelResNum);
        }

        internal void mnuRILogic_Click(object sender, EventArgs e) {

        }
        internal void mnuRIObjects_Click(object sender, EventArgs e) {

        }
        internal void mnuRIPicture_Click(object sender, EventArgs e) {

        }
        internal void mnuRISound_Click(object sender, EventArgs e) {

        }
        internal void mnuRIView_Click(object sender, EventArgs e) {

        }
        internal void mnuRIWords_Click(object sender, EventArgs e) {

        }
        internal void mnuRNLogic_Click(object sender, EventArgs e) {
            //create new logic and enter edit mode
            NewLogic();
        }
        internal void mnuRNObjects_Click(object sender, EventArgs e) {

        }
        internal void mnuRNPicture_Click(object sender, EventArgs e) {
            //create new picture and enter edit mode
            NewPicture();
        }
        internal void mnuRNSound_Click(object sender, EventArgs e) {
            //create new logic and enter edit mode
            NewSound();
        }
        internal void mnuRNText_Click(object sender, EventArgs e) {

        }
        internal void mnuRNView_Click(object sender, EventArgs e) {
            //create new view and enter edit mode
            NewView();
        }
        internal void mnuRNWords_Click(object sender, EventArgs e) {

        }
        internal void mnuROLogic_Click(object sender, EventArgs e) {
            // ;
        }
        internal void mnuROObjects_Click(object sender, EventArgs e) {

        }
        internal void mnuROPicture_Click(object sender, EventArgs e) {

        }
        internal void mnuROSound_Click(object sender, EventArgs e) {

        }
        internal void mnuROText_Click(object sender, EventArgs e) {

        }
        internal void mnuROView_Click(object sender, EventArgs e) {

        }
        internal void mnuROWords_Click(object sender, EventArgs e) {

        }
        private void frmMDIMain_KeyPress(object sender, KeyPressEventArgs e) {
            // after processing key press for the main form, pass it 
            // to preview window, if it's active

            //*//
            //Debug.Print($"Form Keypress: {e.KeyChar}");

            if (!e.Handled) {
                //
                if (ActiveMdiChild == PreviewWin) {
                    PreviewWin.KeyHandler(e);
                }
            }
        }
        private void tvwResources_AfterSelect(object sender, TreeViewEventArgs e) {
            // probably due to navigation - 
            Debug.Assert(EditGame != null);
            // WTF? opening help file causes the form to call this method
            // when it tries to close-- gdia
            if (EditGame == null) {
                return;
            }

            // show the preview for this node
            //if previewing
            if (WinAGISettings.ShowPreview) {
                if (ActiveMdiChild != PreviewWin && (WinAGISettings.ShiftPreview && ForcePreview)) {
                    //if previn hidden on lostfocus, need to show it AFTER changing displayed resource
                    if (PreviewWin.Visible) {
                        //set form focus to preview
                        PreviewWin.Activate();
                        //set control focus to tvwlist
                        tvwResources.Focus();
                    }
                }
            }
            //if no active form
            if (MdiChildren.Length == 0) {
                //set control focus to treeview
                tvwResources.Focus();
            }
            //if nothing selected
            if (e.Node is null) {
                return;
            }
            //if not changed from previous number
            if (LastNodeName == e.Node.Name) {
                if (!PreviewWin.Visible && WinAGISettings.ShowPreview) {
                    PreviewWin.Show();
                    //set form focus to preview
                    PreviewWin.Activate();
                    //set control focus to tvwlist
                    tvwResources.Focus();
                }
                //don't need to change anything
                return;
            }
            //save current index as last index
            LastNodeName = e.Node.Name;
            //now select it
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
            //move up, if those settings are active
            if (!PreviewWin.Visible && WinAGISettings.ShowPreview) {
                PreviewWin.Show();
                //set form focus to preview
                PreviewWin.Activate();
                //set control focus to tvwlist
                tvwResources.Focus();
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
                    // never allow direct editing
                    e.SuppressKeyPress = true;
                    break;

                }
            }
        }

        public void RemoveSelectedRes() {
            // removing a preview resource -
            // depending on selected type, look for an open editor first
            // if found, use that editor's function; if not found use the
            // default function

            //editor that matches resource being previewed
            switch (SelResType) {
            case AGIResType.Logic:
                //if any logic editor matches this resource
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.FormMode == ELogicFormMode.fmLogic) {
                        if (frm.LogicNumber == SelResNum) {
                            //use this form's method
                            frm.MenuClickInGame();
                            return;
                        }
                    }
                }
                break;
            case AGIResType.Picture:
                //if any Picture editor matches this resource
                foreach (frmPicEdit frm in PictureEditors) {
                    if (frm.PicNumber == SelResNum) {
                        //use this form's method
                        // TODO: frm.MenuClickInGame();
                        return;
                    }
                }
                break;
            case AGIResType.Sound:
                //if any Sound editor matches this resource
                foreach (frmSoundEdit frm in SoundEditors) {
                    if (frm.SoundNumber == SelResNum) {
                        //use this form's method
                        // TODO: frm.MenuClickInGame();
                        return;
                    }
                }
                break;
            case AGIResType.View:
                //if any View editor matches this resource
                foreach (frmViewEdit frm in ViewEditors) {
                    if (frm.ViewNumber == SelResNum) {
                        //use this form's method
                        // TODO: frm.MenuClickInGame();
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
            // ask if resource should be exported first
            // (only if resource has no critical errors)
            if (WinAGISettings.AskExport && !haserror) {
                rtn = MsgBoxEx.Show(MDIMain,
                    "Do you want to export '" + strID + "' before removing it from your game?",
                    "Export " + SelResType.ToString() + " Before Removal",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    "Don't ask this question again", ref blnDontAsk);
                WinAGISettings.AskExport = !blnDontAsk;
                if (!WinAGISettings.AskExport) {
                    // if now hiding, update settings file
                    WinAGISettingsList.WriteSetting(sGENERAL, "AskExport", WinAGISettings.AskExport);
                }
            }
            else {
                // dont ask; assume no
                rtn = DialogResult.No;
            }
            switch (rtn) {
            case DialogResult.Cancel:
                return;
            case DialogResult.Yes:
                // export it
                // TODO: SelectedItemExport();
                break;
            case DialogResult.No:
                //nothing to do
                break;
            }
            // confirm removal
            if (WinAGISettings.AskRemove) {
                blnDontAsk = false;
                rtn = MsgBoxEx.Show(MDIMain,
                    "Removing '" + strID + "' from your game.\n\nSelect OK to proceed, or Cancel to keep it in game.",
                    "Remove " + SelResType.ToString() + " From Game",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question,
                    "Don't ask this question again", ref blnDontAsk);
                WinAGISettings.AskRemove = !blnDontAsk;
                // if now hiding, update settings file
                if (!WinAGISettings.AskRemove) {
                    WinAGISettingsList.WriteSetting(sGENERAL, "AskRemove", WinAGISettings.AskRemove);
                }
            }
            else {
                //assume OK
                rtn = DialogResult.OK;
            }
            if (rtn == DialogResult.Cancel) {
                return;
            }
            // clear warnings for this resource
            ClearWarnings(0, (byte)SelResNum, SelResType);
            // now remove it
            switch (SelResType) {
            case AGIResType.Logic:
                RemoveLogic((byte)SelResNum);
                break;
            case AGIResType.Picture:
                RemovePicture((byte)SelResNum);
                break;
            case AGIResType.View:
                RemoveView((byte)SelResNum);
                break;
            case AGIResType.Sound:
                RemoveSound((byte)SelResNum);
                break;
            }
        }

        public void SearchForID(FindFormFunction ffValue = FindFormFunction.ffFindLogic) {
            //set search form defaults
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

            GFindDir = FindDirection.fdAll;
            GMatchWord = true;
            GMatchCase = true;
            GLogFindLoc = FindLocation.flAll;
            GFindSynonym = false;

            //reset search flags
            FindingForm.ResetSearch();

            SearchForm = MDIMain;

            //display find form
            FindingForm.SetForm(FindFormFunction.ffFindLogic, true);
            FindingForm.Show(MDIMain);

            // decided to stick with just showing the form, instead of
            // automatically starting a search

            //////  FindInLogic GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc
        }

        public void AddWarning(TWinAGIEventInfo warnInfo) {
            // four types of warnings/errors
            // - resource errors: ErrLevel > 0
            // - resource warnings: ErrLevel < 0
            // - logic source errors/warning:  
            // - TODO entries

            Debug.Assert(warnInfo.Type != etInfo);

            WarningList.Add(warnInfo);
            // if grid is visible, add the item
            if (MDIMain.pnlWarnings.Visible) {
                AddWarningToGrid(warnInfo);
            }
            // if not visible, show if autowarn is true
            else if (WinAGISettings.AutoWarn) {
                ShowWarningList();
            }
        }

        public void HideWarningList(bool clearlist = false) {
            if (clearlist) {
                ClearWarnings();
            }
            MDIMain.pnlWarnings.Visible = false;
            MDIMain.splitWarning.Visible = false;
        }

        public void ShowWarningList() {
            LoadWarnGrid();
            splitWarning.Visible = true;
            pnlWarnings.Visible = true;
            //mnuTWarnList.Caption = "Hide Warning List\tShift+Ctrl+W";
        }
        private void LoadWarnGrid() {
            fgWarnings.Rows.Clear();
            if (WarningList.Count == 0) {
                return;
            }
            foreach (TWinAGIEventInfo warnInfo in WarningList) {
                AddWarningToGrid(warnInfo, false);
            }
        }
        public void AddWarningToGrid(TWinAGIEventInfo warnInfo, bool showit = true) {
            // adds a warning/error/TODO item to the warning grid
            if (warnInfo.ID == "4001") {
                //ignore?
                return;
            }
            int tmpRow = fgWarnings.Rows.Add(warnInfo.ID,
                         warnInfo.Text,
                         // To avoid runtime errors during sort, all items in a column must be same 
                         // object type, that's why resnum and line must be strings
                         (int)warnInfo.ResType < 4 ? warnInfo.ResNum.ToString() : "--",
                         warnInfo.Line,
                         warnInfo.Module.Length > 0 ? Path.GetFileName(warnInfo.Module) : "--");
            // save restype in row data tag
            fgWarnings.Rows[tmpRow].Tag = warnInfo.ResType;
            // save type in cell[0] tag
            fgWarnings.Rows[tmpRow].Cells[0].Tag = warnInfo.Type;
            fgWarnings.Rows[tmpRow].Cells[0].ToolTipText = "";
            fgWarnings.Rows[tmpRow].Cells[1].ToolTipText = "";
            fgWarnings.Rows[tmpRow].Cells[2].ToolTipText = "";
            fgWarnings.Rows[tmpRow].Cells[3].ToolTipText = "";
            fgWarnings.Rows[tmpRow].Cells[4].ToolTipText = "";

            switch (warnInfo.Type) {
            case etError:
                // bold, red
                fgWarnings.Rows[tmpRow].DefaultCellStyle.Font = new Font(fgWarnings.Font, FontStyle.Bold);
                fgWarnings.Rows[tmpRow].DefaultCellStyle.ForeColor = Color.Red;
                break;
            case etTODO:
                // bold, italic
                fgWarnings.Rows[tmpRow].DefaultCellStyle.Font = new Font(fgWarnings.Font, FontStyle.Bold | FontStyle.Italic);
                break;
            case etResWarning:
            case etCompWarning:
            case etDecompWarning:
                break;
            }
            // always make it visible
            if (showit && !fgWarnings.Rows[tmpRow].Displayed) {
                fgWarnings.CurrentCell = fgWarnings.Rows[tmpRow].Cells[0];
            }
        }
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
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                tvwResources.Focus();
                break;
            case agiSettings.EResListType.ComboList:
                lstResources.Focus();
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
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                tvwResources.Focus();
                break;
            case agiSettings.EResListType.ComboList:
                lstResources.Focus();
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
            Font nlFont = new(WinAGISettings.PFontName, WinAGISettings.PFontSize);
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
        private void mnuGMRU_Click(object sender, EventArgs e) {
            // open the mru game assigned to this menu item
            _ = int.TryParse(((ToolStripMenuItem)sender).Tag.ToString(), out int index);
            OpenMRUGame(index);
        }
        private void mnuGExit_Click(object sender, EventArgs e) {
            // shut it all down
            this.Close();
        }
        private void btnImportLogic_Click(object sender, EventArgs e) {
            MessageBox.Show("Import logic...");
            btnImportRes.DefaultItem = btnImportLogic;
            btnImportRes.Image = btnImportLogic.Image;
        }
        private void btnImportPicture_Click(object sender, EventArgs e) {
            MessageBox.Show("Import picture...");
            btnImportRes.DefaultItem = btnImportPicture;
            btnImportRes.Image = btnImportPicture.Image;
        }
        private void btnImportSound_Click(object sender, EventArgs e) {
            MessageBox.Show("Import sound...");
            btnImportRes.DefaultItem = btnImportSound;
            btnImportRes.Image = btnImportSound.Image;
        }
        private void btnImportView_Click(object sender, EventArgs e) {
            MessageBox.Show("Import view...");
            btnImportRes.DefaultItem = btnImportView;
            btnImportRes.Image = btnImportView.Image;
        }
        private void btnOpenLogic_Click(object sender, EventArgs e) {
            MessageBox.Show("Open logic...");
            btnOpenRes.DefaultItem = btnOpenLogic;
            btnOpenRes.Image = btnOpenLogic.Image;
        }
        private void btnOpenPicture_Click(object sender, EventArgs e) {
            MessageBox.Show("Open picture...");
            btnOpenRes.DefaultItem = btnOpenPicture;
            btnOpenRes.Image = btnOpenPicture.Image;
        }
        private void btnOpenSound_Click(object sender, EventArgs e) {
            MessageBox.Show("Open sound...");
            btnOpenRes.DefaultItem = btnOpenSound;
            btnOpenRes.Image = btnOpenSound.Image;
        }
        private void btnOpenView_Click(object sender, EventArgs e) {
            MessageBox.Show("Open view...");
            btnOpenRes.DefaultItem = btnOpenView;
            btnOpenRes.Image = btnOpenView.Image;
        }
        private void mnuGImport_Click(object sender, EventArgs e) {
            // import a game by directory
            OpenDIR();
        }
        private void mnuGOpen_Click(object sender, EventArgs e) {
            // open a game
            OpenWAGFile();
        }

        /// <summary>
        /// Clears entire warning/error panel. If mode is 1, TODOs and
        /// decomp warnings are left in place.
        /// </summary>
        public void ClearWarnings() {
            WarningList.Clear();
            // then clear the grid
            fgWarnings.Rows.Clear();
        }

        /// <summary>
        /// Clears all warnings and errors for the resource specified
        /// by number and type. If mode is 1, TODOs and decomp warnings
        /// are left in place.
        /// </summary>
        /// <param name="ResNum"></param>
        /// <param name="ResType"></param>
        public void ClearWarnings(int mode, byte ResNum, AGIResType ResType) {
            // TODO: need to redo warninglist to use its own datatype

            //find the matching lines (by type/number)
            for (int i = WarningList.Count - 1; i >= 0; i--) {
                if (WarningList[i].ResNum == ResNum && WarningList[i].ResType == ResType) {
                    if (mode == 1) {
                        if (ResType == AGIResType.Logic &&
                           (WarningList[i].Type == EventType.etTODO || WarningList[i].Type == etDecompWarning)) {
                            continue;
                        }
                    }
                    WarningList.RemoveAt(i);
                }
            }

            for (int i = fgWarnings.Rows.Count - 1; i >= 0; i--) {
                string resnum;
                if (ResType <= AGIResType.View) {
                    resnum = ResNum.ToString();
                }
                else {
                    resnum = "--";
                }
                if (fgWarnings.Rows[i].Cells[2].Value == resnum && (AGIResType)fgWarnings.Rows[i].Tag == ResType) {
                    if (mode == 1) {
                        if (ResType == AGIResType.Logic && ((EventType)fgWarnings.Rows[i].Cells[0].Tag == etTODO || (EventType)fgWarnings.Rows[i].Cells[0].Tag == etDecompWarning)) {
                            continue;
                        }
                    }
                    fgWarnings.Rows.RemoveAt(i);
                }
            }
        }

        private void splResource_Panel1_Resize(object sender, EventArgs e) {
            // resize the navigation buttons
            cmdBack.Width = splResource.Width / 2;
            cmdForward.Width = splResource.Width / 2;
            cmdForward.Left = splResource.Width / 2;
        }

        private void propertyGrid1_MouseWheel(object s, MouseEventArgs e) {
            // wheeling on the IntVersion property should open the dropdownlist
            // but I don't know how to do it, so instead we ignore the mousewheel
            if (propertyGrid1.SelectedGridItem.Label == "IntVer") {
                ((HandledMouseEventArgs)e).Handled = true;
            }
        }
        public void DismissWarning(int row) {
            //remove a row to dismiss the warning
            fgWarnings.Rows.RemoveAt(row);
        }
        public void HelpWarning() {
            string strTopic, strNum;
            //show help for the warning (or error) that is selected

            strNum = (string)fgWarnings.SelectedRows[0].Cells[2].Value;
            if (Val(strNum) > 5000) {
                strTopic = @"htm\winagi\compilerwarnings.htm";
            }
            else {
                strTopic = @"htm\winagi\compilererrors.htm";
            }
            strTopic = strTopic + "#" + strNum;
            // show warning help
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, strTopic);
        }

        public void SelectedItemDescription(int FirstProp) {
            // only use for resources that are NOT being edited;
            // if the resource is being edited, the editor for that
            // resource handles description, ID and number changes
            string strID = "", strDesc = "";

            // is there an open editor for this resource?
            // if so, use that editor's renumber function
            switch (SelResType) {
            case AGIResType.Logic:
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.LogicNumber == SelResNum) {
                        frm.MenuClickDescription(1);
                        return;
                    }
                }
                // no editor found
                strID = EditGame.Logics[SelResNum].ID;
                strDesc = EditGame.Logics[SelResNum].Description;
                break;
            case AGIResType.Picture:
                foreach (frmPicEdit frm in PictureEditors) {
                    if (frm.PicNumber == SelResNum) {
                        frm.MenuClickDescription(1);
                        return;
                    }
                }
                // no editor found
                strID = EditGame.Pictures[SelResNum].ID;
                strDesc = EditGame.Pictures[SelResNum].Description;
                break;
            case AGIResType.Sound:
                foreach (frmSoundEdit frm in SoundEditors) {
                    if (frm.SoundNumber == SelResNum) {
                        frm.MenuClickDescription(1);
                        return;
                    }
                }
                // no editor found
                strID = EditGame.Sounds[SelResNum].ID;
                strDesc = EditGame.Sounds[SelResNum].Description;
                break;
            case AGIResType.View:
                foreach (frmViewEdit frm in ViewEditors) {
                    if (frm.ViewNumber == SelResNum) {
                        frm.MenuClickDescription(1);
                        return;
                    }
                }
                // no editor found
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
            // TODO: does GetNewResID need to return a value, and pass args byref?
            // I don't think so but...
            GetNewResID(SelResType, SelResNum, ref strID, ref strDesc, true, FirstProp);
            return;
        }




        void tmpFormMain() {
            /*
      //    WinAGI Game Development System
      //    Copyright (C) 2005-2020 Andrew Korson
      //
      //    This program is free software; you can redistribute it and/or modify
      //    it under the terms of the GNU General public License as published by
      //    the Free Software Foundation; either version 2 of the License, or
      //    (at your option) any later version.
      //
      //    This program is distributed in the hope that it will be useful,
      //    but WITHOUT ANY WARRANTY; without even the implied warranty of
      //    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
      //    GNU General public License for more details.
      //
      //    You should have received a copy of the GNU General public License
      //    along with this program; if not, write to the Free Software
      //    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA


      public void BeginFind()

        //used by SearchForID function

        On Error GoTo ErrHandler

        //each form has slightly different search parameters
        //and procedure; so each form will get what it needs
        //from the form, and update the global search parameters
        //as needed
        //
        //that//s why each search form checks for changes, and
        //sets the global values, instead of doing it once inside
        //the FindingForm code

        //always reset the synonym search
        GFindSynonym = false

        //ensure this form is the search form
        //Debug.Assert SearchForm Is Me

        switch (FindingForm.FormAction
        case faFind
          FindInLogic GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc

        case faReplace
          FindInLogic GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc, true, GReplaceText

        case faReplaceAll
          ReplaceAll GFindText, GReplaceText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      public void IgnoreWarning()

        //Debug.Assert fgWarnings.Row > 0

        Dim lngWarning As Long, i As Long

        On Error GoTo ErrHandler

        //get warning number
        lngWarning = fgWarnings.TextMatrix(fgWarnings.Row, 2)

        //set ignore flag
        LogicSourceSettings.IgnoreWarning(lngWarning) = true

        With fgWarnings
          //delete all rows with this warning
          For i = .Rows - 1 To 1 Step -1
            if (CLng(Val(.TextMatrix(i, 1))) = lngWarning) {
              if (.Rows = 2) {
                .Clear
              } else {
                .RemoveItem i
              }
            }
          Next i

          //always restore column headers
          .TextMatrix(0, 2) = "Warning"
          .TextMatrix(0, 3) = "Description"
          .TextMatrix(0, 4) = "Logic#"
          .TextMatrix(0, 5) = "Line#"
          .TextMatrix(0, 6) = "Module"
        End With
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }



      void EditResource(ByVal ResType As AGIResType, ByVal ResNum As Long)

        //edits a resource; called by the resource lists

        switch (ResType
        case rtGame  //root
          //same as clicking on game properties
          mnuGProperties_Click

        case AGIResType.Logic
          //resource header has no action
          if (ResNum >= 0) {
            OpenLogic ResNum
          }

        case AGIResType.Picture
           //resource header has no action
          if (ResNum >= 0) {
            OpenPicture ResNum
          }

        case AGIResType.Sound
          //resource header has no action
          if (ResNum >= 0) {
            OpenSound ResNum
          }

        case AGIResType.View
          //resource header has no action
          if (ResNum >= 0) {
            OpenView ResNum
          }

        case rtObjects
          //same as open
          mnuROObjects_Click

        case rtWords
          //same as open
          mnuROWords_Click

        }
      }

      public void GRun()
        //tie function
        mnuGRun_Click
      }

      public void RDescription()
        //tie function
        mnuRDescription_Click
      }

      public void SelectedItemExport()
       { 
        //exports the resource currently being previewed

        Dim strExportName As String

        On Error GoTo ErrHandler

        //only game resources can be selected; game resources never have a resource file
        //default filename is always resource ID and restype extension

        switch (SelResType
        case rtGame
          //export all?
          ExportAll

        case AGIResType.Logic
          //first, do source
          if (MessageBox.Show(("Do you want to export the source code for this logic?", vbQuestion + vbYesNo, "Export Logic") = vbYes) {
            //get a filename for the export
            strExportName = NewSourceName(Logics(SelResNum), true)

            //if a filename WAS chosen then we can continue
            if (strExportName != "") {
              //
              Logics(SelResNum).SaveSource strExportName, true
            }
          }

          //then do resource
          if (MessageBox.Show(("Do you want to export the compiled logic resource?", vbQuestion + vbYesNo, "Export Logic") = vbYes) {
            ExportLogic Logics(SelResNum).Number
          }

        case AGIResType.Picture
          ExportPicture Pictures(SelResNum), true

        case AGIResType.Sound
          ExportSound Sounds(SelResNum), true

        case AGIResType.View
          ExportView Views(SelResNum), true

        case rtObjects
          ExportObjects InvObjects, true

        case rtWords
          ExportWords WordList, true

        default:
          //should never get here for other resource types

        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }
      public void SelectedItemRenumber()
      {
            renumber the selected resource

        Dim NewResNum As Byte, OldResNum As Byte
        Dim strOldID As String
        Dim i As Long, frm As Form

        //only use for resources that are NOT being edited;
        //if the resource is being edited, the editor for that
        //resource handles description, ID and number changes

        On Error GoTo ErrHandler

        //is there an open editor for this resource?
        //if so, use that editor//s renumber function
        switch (SelResType
        case AGIResType.Logic
          //step through logiceditors
          foreach (frm In LogicEditors
            if (frm.LogicNumber = SelResNum) {
              frm.MenuClickRenumber
              return;
            }
          Next
        case AGIResType.Picture
          //step through pictureeditors
          foreach (frm In PictureEditors
            if (frm.PicNumber = SelResNum) {
              frm.MenuClickRenumber
              return;
            }
          Next
        case AGIResType.Sound
          //step through soundeditors
          foreach (frm In SoundEditors
            if (frm.SoundNumber = SelResNum) {
              frm.MenuClickRenumber
              return;
            }
          Next
        case AGIResType.View
          //step through Vieweditors
          foreach (frm In ViewEditors
            if (frm.ViewNumber = SelResNum) {
              frm.MenuClickRenumber
              return;
            }
          Next
        }

        //Debug.Assert SelResNum >= 0 && SelResNum <= 255

        //if logic,
        if (SelResType = AGIResType.Logic) {
          //save id
          strOldID = Logics(SelResNum).ID
        }

        //old res num
        OldResNum = SelResNum

        //get new number
        NewResNum = RenumberResource(SelResNum, SelResType)

        //if canceled, or no change
        if (OldResNum = NewResNum) {
          return;
        }

        //remember to set LastNode Value to "";
        //since indices change, it is possible the
        //the index of the newly selected resource
        //could match an existing resource index
        LastNodeName = "";

        //if a logic was renumbered
        if (SelResType = AGIResType.Logic) {
          //if ID changed because of renumbering
          if (Logics(SelResNum).ID != strOldID) {
            //if old default file exists
            if (File.Exists(ResDir + strOldID + EditGame.SourceExt)) {
              On Error Resume Next
              //rename it
              Name ResDir + strOldID + EditGame.SourceExt As ResDir + Logics(SelResNum).ID + EditGame.SourceExt
            }
          }

          //if using layout editor
          if (UseLE) {
            //update layout
            UpdateExitInfo euRenumberRoom, OldResNum, null, NewResNum
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      public void TMenu()
        //tie function
        mnuTMenuEditor_Click
      }


      public void UpdateSplitH(ByVal SplitHLoc As Single, Optional ByVal Force As Boolean = false)

        Dim frm As Form
        Dim OldHeight As Single
        Dim rtn As Long

        On Error GoTo ErrHandler

        //note that form and screen are in twips; picResources scalemode is pixels

        //if minimized,
        if ((MDIMain.WindowState = vbMinimized)) {
          //just exit
          return;
        }

        //if warnings window not visible, just exit
        if (!picWarnings.Visible) {
          return;
        }

        //resize spliticons to match panel width
        picSplitH.Width = picWarnings.Width - 60
        picSplitHIcon.Width = picWarnings.Width

        //get current split pos
        OldHeight = picWarnings.Top

        //if no change in width, AND not forcing
        if (OldHeight = SplitHLoc && !Force) {
          //nothing needs changing
          return;
        }
        Dim lngBtm As Long
        lngBtm = picWarnings.Top + picWarnings.Height

        picWarnings.Top = SplitHLoc
        picSplitH.Top = SplitHLoc
        picWarnings.Height = picBottom.Top + picBottom.Height - picWarnings.Top

        //set the main bottom panel to match warnings panel (effing status bar moves when we do this)
        picBottom.Height = picWarnings.Height
        this.StatusBar1.Top = picBottom.Top + picBottom.Height
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      public void UpdateSplitV(ByVal SplitVLoc As Single, Optional ByVal Force As Boolean = false)

        Dim frm As Form
        Dim OldWidth As Single
        Dim rtn As Long

        On Error GoTo ErrHandler

        //note that form and screen are in twips; picResources scalemode is pixels

        //if minimized,
        if ((MDIMain.WindowState = vbMinimized)) {
          //just exit
          return;
        }

        //if resources window not visible, just exit
        if (!picResources.Visible) {
          return;
        }

        //resize spliticons to match form height
        picSplitV.Height = MainStatusBar.Top - Toolbar1.Height
        picSplitVIcon.Height = MainStatusBar.Top - Toolbar1.Height

        //get current split pos
        //(width of resource list minus the split width
        OldWidth = picResources.Width - SPLIT_WIDTH

        //if no change in width, AND not forcing
        if (OldWidth = SplitVLoc && !Force) {
          //nothing needs changing
          return;
        }

        //set rescource list width
        picResources.Width = SplitVLoc + SPLIT_WIDTH

        switch (Settings.ResListType
        case 0
        case 1 //tree
          //set resource tree width
          tvwResources.Width = picResources.ScaleWidth - (SPLIT_WIDTH / ScreenTWIPSX)
        case 2 //box
          //set combo and listbox width
          cmbResType.Width = picResources.ScaleWidth - (SPLIT_WIDTH / ScreenTWIPSX)
          lstResources.Width = cmbResType.Width
        }

        //set propertywindow width
        picSplitRes.Width = picResources.ScaleWidth - (SPLIT_WIDTH / ScreenTWIPSX)
        picProperties.Width = picSplitRes.Width - 2

        //position splitter
        picSplitV.Left = SplitVLoc

        //set the main left panel to match resource panel
        picLeft.Width = picResources.Width

        //if also showing the warning list, need to adjust it
        if (picWarnings.Visible) {
          picWarnings.Left = picResources.Width
          picWarnings.Width = picBottom.Width - picWarnings.Left
          picSplitH.Left = picWarnings.Left + 30
          picSplitH.Width = picWarnings.Width - 60
        }

      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void agGameEvents_CompileGameStatus(cStatus As WinAGI.ECStatus, ResType As WinAGI.AGIResType, ResNum As Byte, ErrString As String)

        Dim rtn As VbMsgBoxResult
        Dim strID As String, blnDontAsk As Boolean
        Dim strWarnings() As String, strErrInfo() As String, i As Long

        On Error GoTo ErrHandler

        //if compiling form not loaded,
        if (CompStatusWin = null) {
          //should never get here?
          return;
        }

        With CompStatusWin
          switch (cStatus
          case csCompWords
            .lblStatus.Caption = "Compiling WORDS.TOK..."

          case csCompObjects
            .lblStatus.Caption = "Compiling OBJECT"

          case csAddResource
            switch (ResType
            case AGIResType.Logic
              .lblStatus.Caption = "Adding " + ResourceName(Logics(ResNum), true, true)
              SetLogicCompiledStatus ResNum, true

            case AGIResType.Picture
              .lblStatus.Caption = "Adding " + ResourceName(Pictures(ResNum), true, true)
            case AGIResType.Sound
              .lblStatus.Caption = "Adding " + ResourceName(Sounds(ResNum), true, true)
            case AGIResType.View
              .lblStatus.Caption = "Adding " + ResourceName(Views(ResNum), true, true)
            }

          case csDoneAdding
            .lblStatus.Caption = "Copying VOL files"

          case csCompileComplete
            switch (CompStatusWin.WindowFunction
            case 0        //if full compile
              .lblStatus.Caption = "Compile complete"
            case 1
              .lblStatus.Caption = "Rebuild complete"
            case 2
              .lblStatus.Caption = "All logics compiled"
            }

            //set progressbar to full Value
            .pgbStatus.Value = .pgbStatus.Max

          case csWarning
            //the warning will contain four elements separated by a "\":
            //type~number\warningtext\line\module~
            //for nonlogics, number, line and module are a double dash

            .lblStatus.Caption = "Resource warning"
            //need to increment warning counter
            .lblWarnings.Caption = .lblWarnings.Caption + 1
            if (ResType = AGIResType.Logic) {
              //need to parse out the warnings
              strWarnings = Split(ErrString, "|")
              For i = 0 To UBound(strWarnings)
                AddWarning strWarnings(i), AGIResType.Logic, ResNum
              Next i
            } else {
              AddWarning ErrString, ResType, ResNum
            }

          case csResError
            .lblStatus.Caption = "Compiler error"
            //need to increment error counter, and store this error
            .lblErrors.Caption = .lblErrors.Caption + 1
            switch (ResType
            case AGIResType.Logic
              strID = ResourceName(Logics(ResNum), true, true)
            case AGIResType.Picture
              strID = ResourceName(Pictures(ResNum), true, true)
            case AGIResType.Sound
              strID = ResourceName(Sounds(ResNum), true, true)
            case AGIResType.View
              strID = ResourceName(Views(ResNum), true, true)
            }

            //always cancel compile on error
            .CompCanceled = true
            CancelCompile

            //restore cursor before showing msgbox
            Screen.MousePointer = vbDefault
            rtn = MessageBox.Show(("An error occurred while attempting to add " + strID + ":" + vbNewLine + vbNewLine _
                         + ErrString, vbOKOnly + vbInformation, "Resource Error")
            //show wait cursor again
            WaitCursor
            return;

          case csLogicError
            //always cancel compile on error
            .CompCanceled = true
            CancelCompile

            //set node to //uncompiled
            SetLogicCompiledStatus ResNum, false

            //extract error info
            strErrInfo = Split(ErrString, "|")

            //add it to the warning list
            With MDIMain
              .AddError strErrInfo(0), Val(Left(strErrInfo(2), 4)), Right(strErrInfo(2), Len(strErrInfo(2)) - 6), ResNum, strErrInfo(1)
              if (!.picWarnings.Visible) {
                .pnlWarnings.Visible = true;
              }
            End With

            strID = ResourceName(Logics(ResNum), true, true)

            //extract error info
            strErrInfo() = Split(ErrString, "|")

            //determine user response to the error
            switch (Settings.OpenOnErr
            case 0 //ask
              //restore cursor before showing msgbox
              Screen.MousePointer = vbDefault
              //get user//s response
              rtn = MsgBoxEx("An error in your code has been detected in logic //" + strID + "//. Do you " + vbNewLine + _
                             "want to open the logic at the location of the error?", vbQuestion + vbYesNo, "Update Logics?", , , "Always take this action when a compile error occurs.", blnDontAsk)
              //show wait cursor again
              WaitCursor
              if (blnDontAsk) {
                if (rtn = vbYes) {
                  Settings.OpenOnErr = 1
                } else {
                  Settings.OpenOnErr = 2
                }
                //update settings list
                GameSettings.WriteSetting(sLOGICS, "OpenOnErr", Settings.OpenOnErr

              }

            case 1 //always yes
              rtn = vbYes

            case 2 //always no
              rtn = vbNo
              //restore cursor before showing msgbox
              Screen.MousePointer = vbDefault
              MsgBoxEx "An error in your code has been detected in logic //" + strID + "//:" + vbNewLine + vbNewLine _
                         + "Line " + strErrInfo(0) + ", Error# " + strErrInfo(1), vbOKOnly + vbInformation + vbMsgBoxHelpButton, "Logic Compiler Error", WinAGIHelp, "htm\winagi\compilererrors.htm#" + strErrInfo(1)
              //show wait cursor again
              WaitCursor

            }

            //if yes,
            if (rtn = vbYes) {
              //set error info (open the file)
              SetError CLng(strErrInfo(0)), strErrInfo(2), ResNum, strErrInfo(1)
              Err.Clear
              //sound a tone
              Beep
            }

            return;

          case csCanceled
            //user canceled...
            .CompCanceled = true
            CancelCompile
            return;
          }

          //if not complete, error, or warning
          if (cStatus != csCompileComplete && cStatus != csResError && cStatus != csWarning) {
            //increment progressbar
            .pgbStatus.Value = .pgbStatus.Value + 1
            //allow user chance to cancel (can//t use safe version, or user won//t be able to cancel)
            DoEvents //do NOT use SafeDoEvents!
          }
        End With
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void agGameEvents_LoadStatus(lStatus As WinAGI.ELStatus, ResType As WinAGI.AGIResType, ResNum As Byte, ErrString As String)

        //update the load ProgressWin form

        Dim strWarnings() As String, strErrInfo() As String, i As Long
        Dim blnNoWAG As Boolean

        On Error GoTo ErrHandler

        With ProgressWin
          switch (lStatus
          case lsDecompiling
            .lblProgress.Caption = "Validating AGI game files ..."
             blnNoWAG = true

          case lsPropertyFile
            if (blnNoWAG) {
              .lblProgress.Caption = "Creating game property file ..."
            } else {
              .lblProgress.Caption = "Loading game property file ..."
            }

          case lsResources
            switch (ResType
            case AGIResType.Logic, AGIResType.Picture, AGIResType.View, AGIResType.Sound
              .lblProgress.Caption = "Validating Resources: " + ResTypeName(ResType) + " " + ResNum

            case rtWords
              .lblProgress.Caption = "Validating WORDS.TOK file"

            case rtObjects
              .lblProgress.Caption = "Validating OBJECT file"
            }

          case lsFinalizing
            .lblProgress.Caption = "Configuring WinAGI"

          }

          SafeDoEvents

        End With
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void agGameEvents_LogCompWarning(Warning As String, LogNum As Byte)

        //should only happen when a logic is being compiled;
        //add this warning to the list

        //(it's up to compiling function to manage clearing the list
        //prior to compiling a new logic
        AddWarning Warning, AGIResType.Logic, CLng(LogNum)

        //if part of a full game compile, the status form is visible

        //if compiling form not loaded,
        if (CompStatusWin = null) {
          //nothing to do
          return;
        }

        With CompStatusWin
          .lblStatus.Caption = "Resource warning"
          //need to increment warning counter
          .lblWarnings.Caption = .lblWarnings.Caption + 1
        End With

      }

      void cmbResType_Click()

        Dim i As Long, tmpItem As ListItem
        Dim NewType As AGIResType, NewNum As Long
        Dim lngWidth As Long

        On Error GoTo ErrHandler

        With lstResources.ListItems
          //always clear the list
          .Clear

          switch (cmbResType.SelectedIndex
          case 0 //root
            NewType = rtGame
            NewNum = -1
            //no listitems

          case 1 //logics
            //add logics
            if (Logics.Count > 0) {
              For i = 0 To 255
                //if a valid resource
                if (Logics.Exists(i)) {
                  tmpItem = .Add(, "l" + CStr(i), ResourceName(Logics(i), true))
                  tmpItem.Tag = i
                  //load source to set compiled status
                  if (Logics(i).Compiled) {
                    tmpItem.ForeColor = Color.Black
                  } else {
                    tmpItem.ForeColor = Color.Red
                  }
                  // check for max width
                  if (picResources.TextWidth(tmpItem.Text) > lngWidth) {
                    lngWidth = picResources.TextWidth(tmpItem.Text)
                  }
                }
              Next i
            }
            //set column width
            lstResources.ColumnHeaders(1).Width = 1.2 * lngWidth

            NewType = AGIResType.Logic
            NewNum = -1

          case 2 //pictures
            if (Pictures.Count > 0) {
              For i = 0 To 255
                //if a valid resource
                if (Pictures.Exists(i)) {
                  tmpItem = .Add(, "p" + CStr(i), ResourceName(Pictures(i), true))
                  tmpItem.Tag = i
                  // check for max width
                  if (picResources.TextWidth(tmpItem.Text) > lngWidth) {
                    lngWidth = picResources.TextWidth(tmpItem.Text)
                  }
                }
              Next i
            }
            //set column width
            lstResources.ColumnHeaders(1).Width = 1.2 * lngWidth

            NewType = AGIResType.Picture
            NewNum = -1

          case 3 //sounds
            if (Sounds.Count > 0) {
              For i = 0 To 255
                //if a valid resource
                if (Sounds.Exists(i)) {
                  tmpItem = .Add(, "s" + CStr(i), ResourceName(Sounds(i), true))
                  tmpItem.Tag = i
                  // check for max width
                  if (picResources.TextWidth(tmpItem.Text) > lngWidth) {
                    lngWidth = picResources.TextWidth(tmpItem.Text)
                  }
                }
              Next i
            }
            //set column width
            lstResources.ColumnHeaders(1).Width = 1.2 * lngWidth

            NewType = AGIResType.Sound
            NewNum = -1

          case 4 //views
            if (Views.Count > 0) {
              For i = 0 To 255
                //if a valid resource
                if (Views.Exists(i)) {
                  tmpItem = .Add(, "v" + CStr(i), ResourceName(Views(i), true))
                  tmpItem.Tag = i
                  // check for max width
                  if (picResources.TextWidth(tmpItem.Text) > lngWidth) {
                    lngWidth = picResources.TextWidth(tmpItem.Text)
                  }
                }
              Next i
            }
            //set column width
            lstResources.ColumnHeaders(1).Width = 1.2 * lngWidth

            NewType = AGIResType.View
            NewNum = -1

          case 5 //objects
            NewType = rtObjects
            NewNum = -1
            //no listitems

          case 6 //words
            NewType = rtWords
            NewNum = -1
            //no listitems

          }
        End With

        //if not adding to queue
        if (!DontQueue) {
          SelectResource NewType, NewNum
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void cmbResType_GotFocus()

        //if not using preview window then focus
        //events must be tracked

        On Error GoTo ErrHandler

        if (!Settings.ShowPreview) {
          //if mdi form already has focus
          //don't do anything else
          if (MDIHasFocus) {
            return;
          }
          //force focus marker to true
          MDIHasFocus = true

          //only update menu if no editor is open; if there
          //is an editor open, it will own menus
          if (ActiveMdiChild = null) {
            switch (SelResType
            case rtGame, rtNone
              AdjustMenus rtGame, true, false, false

            case AGIResType.Logic, AGIResType.Picture, AGIResType.Sound, AGIResType.View
              if (SelResNum = -1) {
                //resource header
                AdjustMenus rtNone, true, false, false
              } else {
                //resource
                AdjustMenus SelResType, true, false, false
              }

            case rtObjects
              AdjustMenus rtObjects, true, false, false

            case rtWords
              AdjustMenus rtWords, true, false, false

            }
          }
        }
      return;

      ErrHandler:

      //Debug.Assert false
      Resume Next
      }


      void cmbResType_KeyDown(KeyCode As Integer, Shift As Integer)

        Dim strTopic As String

        On Error GoTo ErrHandler

        // if main form is disabled for a doevents
        if (!MDIMain.Enabled) {
          // don't process the keystroke
          KeyCode = 0
          Shift = 0
          return;
        }

        //check for global shortcut keys
        CheckShortcuts KeyCode, Shift
        if (KeyCode = 0) {
          return;
        }

        if (Shift = 0) {
          //no shift, ctrl, alt
          switch (KeyCode
          case Keys.F1
            //if using preview window
            if (Settings.ShowPreview) {
              frmPreview.MenuClickHelp
            } else {
              //show treeview help
              HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\restree.htm"
            }
            KeyCode = 0

          case 9  //tab
            lstResources.Focus()
            KeyCode = 0
          }
        }

      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void cmbResType_LostFocus()

        //if not using the preview window, then
        //focus events must be tracked

        On Error GoTo ErrHandler

        if (!Settings.ShowPreview) {
          MDIHasFocus = false
          if (!ActiveControl = null) {
            //if active control is still resource list
            //then an open form may have been clicked
            if (ActiveControl.Name = "lstResources" && !KeepFocus) {
              if (!ActiveMdiChild = null) {
                //reset menus for active form
                ActiveMdiChild.Activate
              }
            } else {
              //if focus is staying on the form
              if (ActiveControl.Name = "picProperties" && ActiveControl.Name = "picSplitRes" &&  ActiveControl.Name = "picSplitV" &&  ActiveControl.Name = "cmbResType") {
                //force focus marker back to true
                MDIHasFocus = true;
              }
            }
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void cmdBack_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        //always set focus to resource list
        switch (Settings.ResListType
        case 1
          tvwResources.Focus();
        case 2
          lstResources.Focus();
        }
      }

      void cmdForward_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        //always set focus to resource list
        switch (Settings.ResListType
        case 1
          tvwResources.Focus();
        case 2
          lstResources.Focus();
        }
      }

      void cmdWLClose_Click()

        //hide the warning list window
        pnlWarnings.Visible = false;
      }

      void fgWarnings_DblClick()

        Dim lngRow As Long, lngErrLine As Long, lngLogicNo As Long
        Dim strErrMsg As String, strModule As String, rtRes As AGIResType
        Dim blnWarn As Boolean

        On Error GoTo ErrHandler

        //MouseRow is the row actually clicked on
        //usually it is same as Row, but when header is
        //clicked, Row defaults to first row of data
        //(which is row 1)

        With fgWarnings
          //ignore if nothing to click on
          //(easy check is if first row is blank)
          if (Len(.TextMatrix(1, 2)) = 0) {
            return;
          }

          lngRow = .Row
          if (lngRow != .MouseRow) {
            return;
          }
          //hmm, looks like header CAN be selected? for now, ignore it if it is
          if (lngRow = 0) {
            return;
          }

          //get resource Type for selected row
          rtRes = .RowData(lngRow)

          //if a logic or text,
          if (rtRes = AGIResType.Logic && rtRes = rtText) {
            strErrMsg = .TextMatrix(.Row, 3)
            lngLogicNo = CLng(.TextMatrix(.Row, 4))
            lngErrLine = CLng(.TextMatrix(.Row, 5))
            if (.TextMatrix(.Row, 6) = Logics(lngLogicNo).ID) {
              strModule = ""
            } else {
              strModule = .TextMatrix(.Row, 0) + "\" + .TextMatrix(.Row, 6)
            }

            //warning?
            blnWarn = Val(.TextMatrix(.Row, 2)) > 5000
            //open the logic, and highlight error/warning
            SetError lngErrLine, strErrMsg, lngLogicNo, strModule, blnWarn
          }
        End With
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void fgWarnings_KeyDown(KeyCode As Integer, Shift As Integer)

        //allow select all and copy
        Dim i As Long, j As Long, strText As String

        if (Shift = vbCtrlMask) {
          switch (KeyCode
          case Keys.C
            //add everything collected to clipboard
            //  with tabs separating items on a row
            //  and carriage returns separating rows
            With fgWarnings
              //if only one cell selected, add just the one cell
              if (.Row = .RowSel && .Col = .ColSel) {
                strText = .TextMatrix(.Row, .Col)
              } else {
                //multiple rows selected
                For i = .Row To .RowSel
                  For j = .Col To .ColSel
                    strText = strText + .TextMatrix(i, j) + "\t"
                  Next j
                  //strip off last tab and add carriage return
                  strText = Left$(strText, Len(strText) - 1) + vbCr
                Next i
                //strip off last carriage return
                strText = Left$(strText, Len(strText) - 1)
              }

              //now add it to the clipboard
              Clipboard.Clear
              Clipboard.SetText strText, vbCFText
            End With

          case Keys.A
            With fgWarnings
              .Row = 1
              .Col = 2
              .RowSel = .Rows - 1
              .ColSel = .Cols - 1
            End With
          }
        }
      }

      void fgWarnings_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim lngRow As Long, rtRes As AGIResType

        On Error GoTo ErrHandler

        mEX = X
        mEY = Y

        //ignore if any shift keys are pressed at same time
        if (Shift != 0) {
          return;
        }

        With fgWarnings
          //if right button, show menu to ignore!
          if (Button = vbRightButton) {
            //ignore if clicking on header row
            if (.MouseRow = 0) {
              return;
            }

            //make sure grid is the active control
            if (this.ActiveControl = null) {
              .Focus();
            } else if ( this.ActiveControl.Name != "fgWarnings") {
              .Focus();
            }
            //Debug.Assert this.ActiveControl Is fgWarnings

            //if no warnings, don't show menu
            //(easy check is if first row is blank)
            if (Len(.TextMatrix(1, 2)) = 0) {
              return;
            }

            //make sure this row is selected
            if (.Row != .MouseRow) {
              .Row = .MouseRow
              .RowSel = .MouseRow
              .Col = 2
              .ColSel = 6
            }

            //get resource Type for selected row
            rtRes = .RowData(lngRow)

            //can only ignore/dismiss compiler warnings (restype has to be a logic/text)
            if (rtRes = AGIResType.Logic && rtRes = rtText) {
              if (Val(.TextMatrix(.Row, 2)) < 5000) {
                mnuCWIgnore.Visible = false
                mnuCWHelp.Caption = "Help with this Error"
              } else {
                mnuCWIgnore.Visible = true
                mnuCWHelp.Caption = "Help with this Warning"
              }

              PopupMenu mnuCWPopup, , mEX + picResources.Width, mEY + picWarnings.Top
            }
          }
        End With
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void fgWarnings_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

        //set tooltip if current warning doesnt fit

        On Error GoTo ErrHandler

        switch (Button
        case vbLeftButton
          //always reset rowsel to row; so only one row at a time
          With fgWarnings
            if (.Row != .MouseRow && .MouseRow != 0) {
              .Row = .MouseRow
              .RowSel = .Row
            }
          End With
        }

      //  //is mouse over an item?
      //  tmpItem = lvWarnings.HitTest(x, y)
      //  if (tmpItem = null) {
      //    return;
      //  }
      //
      //  //subitem(1) is text of the warning; but the column index is 2
      //  if (this.TextWidth(tmpItem.SubItems(1)) > lvWarnings.ColumnHeaders(2).Width) {
      //    tmpItem.ListSubItems(1).ToolTipText = tmpItem.ListSubItems(1).Text
      //  } else {
      //    tmpItem.ListSubItems(1).ToolTipText = ""
      //  }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void fgWarnings_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        if (Button = vbLeftButton) {
          With fgWarnings
            if (.Row != .MouseRow && .MouseRow != 0) {
              .Row = .MouseRow
              .RowSel = .Row
            }
          End With
        }

        mEX = X
        mEY = Y

      }


      void lstProperty_DblClick()

        On Error GoTo ErrHandler

        //save the property
        SelectPropFromList
        //hide the listbox
        lstProperty.Visible = false
        PaintPropertyWindow

        picProperties.Focus();
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void lstProperty_GotFocus()

        picProperties.Refresh
      }

      void lstProperty_KeyPress(KeyAscii As Integer)

        //trap enter key
        switch (KeyAscii
        case 10, 13 //enter key or ctrl-enter key combination
          //save choice
          KeyAscii = 0
          SelectPropFromList
          lstProperty.Visible = false
          picProperties.Focus();
          PaintPropertyWindow
          return;

        case 27 //esc key
          //cancel
          KeyAscii = 0
          lstProperty.Visible = false
          picProperties.Focus();
          PaintPropertyWindow
          return;
        }
      }

      void lstProperty_LostFocus()

        On Error GoTo ErrHandler

        //make sure list is not visible
        if (lstProperty.Visible) {
          lstProperty.Visible = false
          PaintPropertyWindow
        }

      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void lstResources_DblClick()

        On Error Resume Next

        //if there is a node under the double-clicked location
        if (!lstResources.HitTest(mTX, mTY) = null) {
          //if not the same as the current item
          if (lstResources.HitTest(mTX, mTY).Index != lstResources.SelectedItem.Index) {
            //select it
            SelectResource SelResType, lstResources.HitTest(mTX, mTY).Tag
          }

          //edit the selected item
          EditResource SelResType, SelResNum
        }
      }

      void lstResources_GotFocus()

        //if not using preview window then focus
        //events must be tracked

        On Error GoTo ErrHandler

        if (!Settings.ShowPreview) {
          //if mdi form already has focus
          //don't do anything else
          if (MDIHasFocus) {
            return;
          }
          //force focus marker to true
          MDIHasFocus = true

          //only update menu if no editor is open; if there
          //is an editor open, it will own menus
          if (ActiveMdiChild = null) {
            switch (SelResType
            case rtGame, rtNone
              AdjustMenus rtGame, true, false, false

            case AGIResType.Logic, AGIResType.Picture, AGIResType.Sound, AGIResType.View
              if (SelResNum = -1) {
                //resource header
                AdjustMenus rtNone, true, false, false
              } else {
                //resource
                AdjustMenus SelResType, true, false, false
              }

            case rtObjects
              AdjustMenus rtObjects, true, false, false

            case rtWords
              AdjustMenus rtWords, true, false, false

            }
          }
        }
      return;

      ErrHandler:

      //Debug.Assert false
      Resume Next
      }


      void lstResources_KeyDown(KeyCode As Integer, Shift As Integer)

        Dim strTopic As String

        On Error GoTo ErrHandler

        // if main form is disabled for a doevents
        if (!MDIMain.Enabled) {
          // don't process the keystroke
          KeyCode = 0
          Shift = 0
          return;
        }

        //check for global shortcut keys
        CheckShortcuts KeyCode, Shift
        if (KeyCode = 0) {
          return;
        }

        if (Shift = 0) {
          //no shift, ctrl, alt
          switch (KeyCode
          case Keys.Delete
            //if a resource is selected
            switch (SelResType
            case AGIResType.Logic, AGIResType.Picture, AGIResType.Sound, AGIResType.View
              //call remove from game method
              RemoveSelectedRes
              KeyCode = 0
            }

          case Keys.F1
            //if using preview window
            if (Settings.ShowPreview) {
              frmPreview.MenuClickHelp
            } else {
              //show treeview help
              HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\restree.htm"
            }
            KeyCode = 0

          case 9  //tab
            picProperties.Focus();
            KeyCode = 0
          }
        } else if ( Shift = vbShiftMask + vbCtrlMask) {
          //Shift+Ctrl//
          if (Settings.ShowPreview) {
            if (SelResType = AGIResType.Picture && KeyCode = Keys.S) {
              //save Image as ...
              frmPreview.MenuClickCustom1
            }
          }
        } else if ( Shift = vbCtrlMask) {
          switch (KeyCode
          case Keys.F //Ctrl+F (Find)
            switch (SelResType
            case AGIResType.Logic, AGIResType.Picture, AGIResType.Sound, AGIResType.View
              //find this resid
              SearchForID
            }
          }
        }

      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void lstResources_KeyPress(KeyAscii As Integer)

        On Error GoTo ErrHandler

        //enter key is same as double-click
        switch (KeyAscii
        case 13 //return key
          //edit the selected resource
          EditResource SelResType, SelResNum
          KeyAscii = 0
        default:

        }

        //if preview window is visible,
        if (PreviewWin.Visible) {
          PreviewWin.KeyHandler KeyAscii
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void lstResources_LostFocus()

        //if not using the preview window, then
        //focus events must be tracked

        On Error GoTo ErrHandler

        if (!Settings.ShowPreview) {
          MDIHasFocus = false
          if (!ActiveControl = null) {
            //if active control is still resource list
            //then an open form may have been clicked
            if (ActiveControl.Name = "lstResources" && !KeepFocus) {
              if (!ActiveMdiChild = null) {
                //reset menus for active form
                ActiveMdiChild.Activate
              }
            } else {
              //if focus is staying on the form
              if (ActiveControl.Name = "picProperties" && _
                 ActiveControl.Name = "picSplitRes" && _
                 ActiveControl.Name = "picSplitV" && _
                 ActiveControl.Name = "cmbResType") {
                //force focus marker back to true
                MDIHasFocus = true
              }
            }
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void lstResources_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim tmpItem As ListItem
        Dim NewNum As Long

        On Error GoTo ErrHandler

        //store x and Y values for checking in double click
        mTX = X
        mTY = Y

        //if right clicked
        if (Button = vbRightButton) {
          //res type determines action
          switch (cmbResType.SelectedIndex
          case 0, 5, 6 //root, objects, words
            //need doevents so form activation occurs BEFORE popup
            //otherwise, errors will be generated because of menu
            //adjustments that are made in the form_activate event
            SafeDoEvents
            PopupMenu mnuResource, , X + lstResources.Left, Y + picResources.Top

          case 1 To 4 //lpsv
            // get the item under the cursor
            tmpItem = lstResources.HitTest(X, Y)

            //if nothing, just exit
            if (tmpItem = null) {
              return;
            }

            //if this item is not the currently selected item
            NewNum = CLng(tmpItem.Tag)
            if (NewNum != SelResNum) {
              //select it
              SelectResource SelResType, NewNum
              tmpItem.Selected = true
            }

            //adjust the resource menu
            mnuRNew.Visible = false
            mnuROpen.Visible = false
            mnuRImport.Visible = false
            //use bar0 for open command
            mnuRBar0.Caption = "Open " + ResTypeName(SelResType)

            //need doevents so form activation occurs BEFORE popup
            //otherwise, errors will be generated because of menu
            //adjustments that are made in the form_activate event
            SafeDoEvents
            PopupMenu mnuResource, , X + lstResources.Left, Y + picResources.Top

            //restore menu settings
            mnuRNew.Visible = true
            mnuROpen.Visible = true
            mnuRImport.Visible = true
            mnuRBar0.Caption = "-"
            mnuRBar2.Visible = true
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void MDIForm_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        On Error GoTo ErrHandler

        //if property list box or edit box is visible, just hide them
        if (txtProperty.Visible) {
          //hide
          txtProperty.Visible = false
          PaintPropertyWindow
        }
        if (txtProperty.Visible) {
          //hide
          txtProperty.Visible = false
          PaintPropertyWindow
        }
        picProperties.Focus();
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }



      void mnuCWAll_Click()

        On Error Resume Next

        //Debug.Assert this.ActiveControl Is this.fgWarnings
        if (this.ActiveControl Is this.fgWarnings) {
          With fgWarnings
            //dismiss all warnings (i.e. clear the grid)
            .Clear

            //delete all but one row
            .Rows = 2

            //always restore column headers
            .TextMatrix(0, 2) = "Warning"
            .TextMatrix(0, 3) = "Description"
            .TextMatrix(0, 4) = "Logic#"
            .TextMatrix(0, 5) = "Line#"
            .TextMatrix(0, 6) = "Module"
          End With
        }
      }

      void mnuCWDismiss_Click()

        On Error Resume Next

        //Debug.Assert this.ActiveControl Is this.fgWarnings
        if (this.ActiveControl Is this.fgWarnings) {
          DismissWarning(fgWarnings.SelectedRows[0].Index);
        }
      }

      void mnuECustom1_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickECustom1
      }
      void mnuECustom2_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickECustom2
      }


      void mnuECustom3_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickECustom3
      }

      void mnuEdit_Click()


      // this doesn't work; VB can//t change visible status
      // of menu items from inside the click event of the
      // parent menu. This really sucks, because it would
      // be MUCH SIMPLER to manage the edit menu from here
      // instead of having to update it everytime a change
      // is made in an editor form

      //////  On Error GoTo ErrHandler
      //////
      //////  this.ActiveMdiChild.SetEditMenu
      //////return;
      //////
      //////ErrHandler:
      //////  //Debug.Assert false
      //////  Resume Next
      }

      void mnuERedo_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickRedo
      }

      void mnuEReplace_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickReplace
      }


      void mnuGCompileDirty_Click()

        //compile dirty
        CompileDirtyLogics
      }

      void mnuGCompileTo_Click()

        //compile game to directory of user//s choice
        CompileAGIGame
      }

      void mnuGMRU_Click()

        OpenMRUGame 0
      }

      void mnuGMRU1_Click()

        OpenMRUGame 1
      }


      void mnuGMRU2_Click()

        OpenMRUGame 2
      }


      void mnuGMRU3_Click()

        OpenMRUGame 3
      }


      void mnuGProperties_Click()

        Dim rtn As VbMsgBoxResult

        On Error GoTo ErrHandler

        //show properties form
        Load frmGameProperties
        With frmGameProperties
          .WindowFunction = gsEdit
          .SetForm

          KeepFocus = true
          .Show vbModal, Me
          KeepFocus = false

          //if canceled
          if (.Canceled) {
            // unload the form and exit without
            // saving anything
            Unload frmGameProperties
            return;
          }

          //GameDir is now read-only
      //////    //directory
      //////    if (GameDir != .DisplayDir && LenB(.DisplayDir) != 0) {
      //////      ChangeGameDir .DisplayDir, true
      //////    }

          //author
          GameAuthor = .txtGameAuthor.Text

          //description
          GameDescription = .txtGameDescription.Text

          //game about
          GameAbout = .txtGameAbout.Text

          //if no directory, force platform to nothing
          if (Len(.NewPlatformFile) = 0) {
            PlatformType = 0
          } else {
            //platform
            if (.optDosBox.Value) {
              PlatformType = 1
            } else if ( .optScummVM.Value) {
              PlatformType = 2
            } else if ( .optNAGI.Value) {
              PlatformType = 3
            } else if ( .optOther.Value) {
              PlatformType = 4
            }
          }

          //platformdir OK as long as not nuthin
          if (PlatformType > 0) {
            Platform = Trim$(.NewPlatformFile)

            //platform options OK if dosbox or scummvm
            if (PlatformType = 1 && PlatformType = 2) {
              PlatformOpts = .txtOptions.Text
            } else {
              PlatformOpts = ""
            }
            //dos executable only used if dosbox
            if (PlatformType = 1) {
              DosExec = .txtExec.Text
            } else {
              DosExec = ""
            }

          } else {
            Platform = ""
            PlatformOpts = ""
            DosExec = ""
          }

          //game version
          GameVersion = .txtGameVersion.Text

          //version (if changed)
          if (InterpreterVersion != .cmbVersion.Text) {
            ChangeIntVersion .cmbVersion.Text
            //check if change in version affected ID
            switch (Settings.ResListType
            case 1
              if (tvwResources.Nodes[1).Text != GameID) {
                tvwResources.Nodes[1).Text = GameID
              }
            case 2
              if (cmbResType.List(0) != GameID) {
                cmbResType.List(0) = GameID
              }
            }
          }

          //id
          if (GameID != .txtGameID.Text) {
            //Debug.Assert .txtGameID.Text != ""
            ChangeGameID .txtGameID.Text
          }

          //resource dir
          if (StrComp(.txtResDir.Text, ResDirName, vbTextCompare) != 0) {
            ChangeResDir .txtResDir.Text
          }

          //resdefine names
          LogicCompiler.UseReservedNames = (.chkUseReserved.Value = vbChecked)

          //layout editor
          UseLE = (.chkUseLE.Value = vbChecked)
          //update menu/toolbar, and hide LE if not in use anymore
          UpdateLEStatus

          //force update to WAG file
          SaveProperties

          //update the reserved lookup values
          RDefLookup(90).Value = QUOTECHAR + GameVersion + QUOTECHAR
          RDefLookup(91).Value = QUOTECHAR + GameAbout + QUOTECHAR
          RDefLookup(92).Value = QUOTECHAR + GameID + QUOTECHAR
        End With

        Unload frmGameProperties

        //if current restype is game
        if (SelResType = rtGame) {
          //update property window
          PaintPropertyWindow
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuCWHelp_Click()

        On Error Resume Next

        //Debug.Assert this.ActiveControl Is this.fgWarnings
        if (this.ActiveControl Is this.fgWarnings) {
          HelpWarning
        }
      }

      void mnuHIndex_Click()

        //open the help file, with index displayed

         // The hWnd is the Parent Windows handle.
         Dim hWndHelp As Long

         //select index pane
         hWndHelp = HtmlHelp(HelpParent, WinAGIHelp, HH_DISPLAY_INDEX, 0)
      }

      void mnuHLogic_Click()

        //select commands start page
        HtmlHelp HelpParent, WinAGIHelp, HH_HELP_CONTEXT, 1001
      }


      void mnuHReference_Click()

        //select reference start page
        HtmlHelp HelpParent, WinAGIHelp, HH_HELP_CONTEXT, 1002
      }

      void mnuCWIgnore_Click()

        On Error Resume Next

        //Debug.Assert this.ActiveControl Is this.fgWarnings
        if (this.ActiveControl Is this.fgWarnings) {
          IgnoreWarning
        }
      }

      void mnuLPCopy_Click()

        On Error Resume Next

        if (ActiveMdiChild Is PreviewWin) {
          PreviewWin.rtfLogPrev.Selection.Range.Copy
          ReDim GlobalsClipboard(0)
        } else {
          Clipboard.Clear
          if (mnuLPCopy.Tag = "A") {
            Clipboard.SetText Right(MainStatusBar.Panels("Anchor").Text, Len(MainStatusBar.Panels("Anchor").Text) - 8)
          } else {
            Clipboard.SetText Right(MainStatusBar.Panels("Block").Text, Len(MainStatusBar.Panels("Block").Text) - 7)
          }
        }
      }

      void mnuLPSelectAll_Click()

        //selects all the text in the preview logic window

        On Error Resume Next

        PreviewWin.rtfLogPrev.Range.SelectRange

      }

      void mnuRBar0_Click()

        //used in context menus only;
        //purpose is to open the selected
        //resource for editing

        On Error GoTo ErrHandler

        //edit the selected item
        EditResource SelResType, SelResNum
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuTBCopy_Click()

        // select the //copy// command
        TBCmd = 2
      }

      void mnuTBCut_Click()

        // select the //cut// command
        TBCmd = 1
      }


      void mnuTBPaste_Click()

        // select the //paste// command
        TBCmd = 3
      }


      void mnuTBSelectAll_Click()

        // select the //selectall// command
        TBCmd = 4
      }

      void mnuTLayout_Click()

        //open the layout editor
        OpenLayout
      }

      void mnuTMenuEditor_Click()

        On Error GoTo ErrHandler

        //show the menu editor form

        if (MEInUse) {
          MenuEditor.Focus();
          // if minimized,
          if (MenuEditor.WindowState = vbMinimized) {
            //restore it
            MenuEditor.WindowState = vbNormal
          }
        } else {
          // create a new instance of menu editor
          MenuEditor = New frmMenuEdit
          Load MenuEditor

          //if canceled, just exit
          if (MenuEditor.Canceled) {
            Unload MenuEditor
          } else {
            MenuEditor.Show
            MenuEditor.Focus();
          }
        }

      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuTPalette_Click()

        //show the palette form
        Load frmPalette
        frmPalette.SetForm 0
        frmPalette.Show vbModal, MDIMain
      }

      void mnuTResDef_Click()

        //edit reserved define values

        //displays the reserved names form for editing
        //it shows as a modal form, so user has to
        //finish with it before they can continue on
        //with anything else

        Dim i As Long, rtn As Long
        Dim OldCol As Long, OldRow As Long

        On Error GoTo ErrHandler

        //edit reserved define names
        frmReserved.Show vbModal, MDIMain

        //if NOT canceled, gotta validate all defines again
        if (!frmReserved.Canceled) {
          //if the global editor is open, update it

          //step through all rows, and format correctly based on
          //whether or not any define overrides a reserved define
          if (GEInUse) {
            With GlobalsEditor.fgGlobals
              //save cursor position
              OldCol = .Col
              OldRow = .Row

              For i = 1 To .Rows - 1
                .Row = i
                .Col = 1 //ctName
                rtn = GlobalsEditor.ValidateName(.TextMatrix(i, 1))
                if (rtn >= 8 && rtn <= 13) {
                  //override
                  .CellFontBold = true
                  .CellForeColor = Color.Red
                } else {
                  //normal
                  .CellFontBold = false
                  .CellForeColor = Color.Black
                }
              Next i

              //restore cursor
              .Col = OldCol
              .Row = OldRow
            End With
          }
        }

        //always unload the reserved form
        Unload frmReserved
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuTSettings_Click()
        //show wait cursor
        WaitCursor

        //show settings form
        KeepFocus = true
        frmSettings.Show vbModal, Me
        KeepFocus = false

        Screen.MousePointer = vbDefault
      }

      void mnuTSnippets_Click()

        On Error GoTo ErrHandler

        //open snippet manager
        SnipMode = 1
        frmSnippets.Show vbModal, MDIMain
        //if active form is an editor
        if (MDIMain.ActiveMdiChild.Name = "frmLogicEdit" && MDIMain.ActiveMdiChild.Name = "frmTextEdit") {
          //force focus to edit control
          MDIMain.ActiveMdiChild.rtfLogic.Focus();
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuWArrange_Click()

        //arrange icons
        MDIMain.Arrange vbArrangeIcons
      }

      void mnuWCascade_Click()

        //cascade windows
        MDIMain.Arrange vbCascade
      }

      void mnuWMinimize_Click()

        Dim frm As Form

        //minimizes all open editors
        foreach (frm In Forms
          //if not main, or preview
          if (frm.Name != "MDIMain" && frm.Name != "frmPreview") {
            frm.WindowState = vbMinimized
          }
        Next

        //now arrange all
        MDIMain.Arrange vbArrangeIcons

      }

      void StatusBar1_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        StatusMouseBtn = Button
        sbX = X
        sbY = Y
      }

      void StatusBar1_PanelClick(ByVal Panel As MSComctlLib.Panel)

        Dim lngErrNum As Long

        On Error GoTo ErrHandler

        //if no active form
        if (ActiveMdiChild = null) {
          return;
        }

        switch (ActiveMdiChild.Name
        case "frmPreview"
        case "frmLogicEdit"
          if (Panel.Key = "Status") {
            if (LenB(Panel.Text) != 0) {
              if (Asc(Panel.Text) = 69) {
                //get error number
                lngErrNum = Val(Mid$(Panel.Text, InStr(1, Panel.Text, ":") + 2))
                MsgBoxEx Panel.Text, vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Logic Compile Error", WinAGIHelp, "htm\winagi\compilererrors.htm#" + CStr(lngErrNum)
              } else {
                //no error info, so don't show help
                MessageBox.Show( Panel.Text, vbInformation + vbOKOnly, "Logic Editor Status"
              }
            }
          }

        case "frmPictureEdit"
          switch (Panel.Key
          case "Scale"
            if (StatusMouseBtn = vbRightButton) {
              //if not at min
              if (ActiveMdiChild.ScaleFactor > 1) {
                ActiveMdiChild.ScaleFactor = ActiveMdiChild.ScaleFactor - 1
              }
            } else {
              //if not at Max
              if (ActiveMdiChild.ScaleFactor < 4) {
                ActiveMdiChild.ScaleFactor = ActiveMdiChild.ScaleFactor + 1
              }
            }
            //redraw pictures
            ActiveMdiChild.ResizePictures
          case "Anchor"
            With MDIMain
              .mnuLPCopy.Enabled = true
              .mnuLPCopy.Tag = "A"
              .mnuLPSelectAll.Visible = false
              PopupMenu .mnuLPPopup, 0, .StatusBar1.Left + sbX, .StatusBar1.Top + sbY
            End With
          case "Block"
            With MDIMain
              .mnuLPCopy.Enabled = true
              .mnuLPCopy.Tag = "B"
              .mnuLPSelectAll.Visible = false
              PopupMenu .mnuLPPopup, 0, .StatusBar1.Left + sbX, .StatusBar1.Top + sbY
            End With

          }

        case "frmSoundEdit"
          switch (Panel.Key
          case "Scale"
            if (StatusMouseBtn = vbRightButton) {
              ActiveMdiChild.ZoomScale -1
            } else {
              ActiveMdiChild.ZoomScale 1
            }
          }

        case "frmViewEdit"
          switch (Panel.Key
          case "Scale"
            if (StatusMouseBtn = vbRightButton) {
              //zoom in
              ActiveMdiChild.ZoomCel 0
            } else {
              //zoom out
              ActiveMdiChild.ZoomCel 1
            }
          }

        case "frmTextEdit"
        case "frmObjectEdit"
          switch (Panel.Key
          case "Encrypt"
            //toggle encryption
            ActiveMdiChild.MenuClickCustom3
          }

        case "frmWordsEdit"
        case "frmLayout"
          switch (Panel.Key
          case "Scale"
            if (StatusMouseBtn = vbRightButton) {
              ActiveMdiChild.ChangeScale 1
            } else {
              ActiveMdiChild.ChangeScale -1
            }
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void StatusBar1_PanelDblClick(ByVal Panel As MSComctlLib.Panel)
        //same as click
        StatusBar1_PanelClick Panel

      }


      void tmrFlash_Timer()

        //flash the main status bar, panel 1 three times;

        FlashCount = FlashCount + 1

        With MainStatusBar
          if ((FlashCount && 1) = 1) {
            //flash it
            .Panels(1).Tag = .Panels(1).Text
            .Panels(1).Text = ""
          } else {
            //restore it
            .Panels(1).Text = .Panels(1).Tag
          }
        End With

        //at six (three flashes), turn off the timer
        if (FlashCount = 8) {
          tmrFlash.Enabled = false
          FlashCount = 0
        }

      }

      void Toolbar1_ButtonMenuClick(ByVal ButtonMenu As MSComctlLib.ButtonMenu)

        //new, open, import default resource has been changed

        Dim i As Long

        //change parent button tag to match button index-1
        ButtonMenu.Parent.Tag = ButtonMenu.Index - 1

        switch (ButtonMenu.Parent.Key
        case "new_r"
          i = 18
        case "open_r"
          i = 22
        case "import_r"
          i = 14
        }

        //update picture (caption)
        ButtonMenu.Parent.Image = i + ButtonMenu.Index

        //now //click// the button
        Toolbar1_ButtonClick ButtonMenu.Parent

      }

      void tvwResources_Click()

        On Error GoTo ErrHandler

        //if the node is double-clicked, the single click event is also raised
        //if the dbl click was on root node, prop window is shown, so trying
        //to show the preview win will raise an error; just ignore it ...

        //if prevwin hidden on lostfocus, need to show it
        if (Settings.ShowPreview) {
          //is it not already visible?
          if (!PreviewWin.Visible) {
            PreviewWin.Show
          }
        }
      return;

      ErrHandler:
        if (Err.Number != 401) {
        //Debug.Assert false
        }
        Resume Next
      }


      void tvwResources_GotFocus()

        //if not using preview window then focus
        //events must be tracked

        On Error GoTo ErrHandler

        if (!Settings.ShowPreview) {
          //if mdi form already has focus
          //don't do anything else
          if (MDIHasFocus) {
            return;
          }
          //force focus marker to true
          MDIHasFocus = true

          //only update menu if no editor is open; if there
          //is an editor open, it will own menus
          if (ActiveMdiChild = null) {
            //if no node selected, select first node
            if (tvwResources.SelectedItem = null) {
              tvwResources.SelectedItem = tvwResources.Nodes[1)
            }

            switch (SelResType
            case rtGame
              AdjustMenus rtGame, true, false, false

            case AGIResType.Logic, AGIResType.Picture, AGIResType.Sound, AGIResType.View
              if (SelResNum = -1) {
                AdjustMenus rtNone, true, false, false
              } else {
                AdjustMenus SelResType, true, false, false
              }

            case rtObjects  //objects
              AdjustMenus rtObjects, true, false, false

            case rtWords  //words
              AdjustMenus rtWords, true, false, false

            }
          }
        }
      return;

      ErrHandler:

      //Debug.Assert false
      Resume Next
      }

      void tvwResources_KeyDown(KeyCode As Integer, Shift As Integer)

        Dim strTopic As String

        On Error GoTo ErrHandler

        // if main form is disabled for a doevents
        if (!MDIMain.Enabled) {
          // don't process the keystroke
          KeyCode = 0
          Shift = 0
          return;
        }

        //check for global shortcut keys
        CheckShortcuts KeyCode, Shift
        if (KeyCode = 0) {
          return;
        }

        if (Shift = 0) {
          //no shift, ctrl, alt
          switch (KeyCode
          case Keys.Delete
            //if a resource is selected
            switch (SelResType
            case AGIResType.Logic, AGIResType.Picture, AGIResType.Sound, AGIResType.View
              //call remove from game method
              RemoveSelectedRes
              KeyCode = 0
            }

          case Keys.F1
            //if using preview window
            if (Settings.ShowPreview) {
              frmPreview.MenuClickHelp
            } else {
              //show treeview help
              HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\restree.htm"
            }
            KeyCode = 0

          case 9  //tab
            picProperties.Focus();
            KeyCode = 0
          }
        } else if ( Shift = vbShiftMask + vbCtrlMask) {
          //Shift+Ctrl//
          if (Settings.ShowPreview) {
            if (SelResType = AGIResType.Picture && KeyCode = Keys.S) {
              //save Image as ...
              frmPreview.MenuClickCustom1
            }
          }
        } else if ( Shift = vbCtrlMask) {
          switch (KeyCode
          case Keys.F //Ctrl+F (Find)
            switch (SelResType
            case AGIResType.Logic, AGIResType.Picture, AGIResType.Sound, AGIResType.View
              //find this resid
              SearchForID
            }
          }
        }

      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void tvwResources_KeyPress(KeyAscii As Integer)

        On Error GoTo ErrHandler

        //enter key is same as double-click
        switch (KeyAscii
        case 13 //return key
          //if a node is active
          //Debug.Assert !tvwResources.SelectedItem = null
          //edit the selected resource
          EditResource SelResType, SelResNum
          KeyAscii = 0
        default:

        }

        //if preview window is visible,
        if (PreviewWin.Visible) {
          PreviewWin.KeyHandler KeyAscii
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void tvwResources_LostFocus()

        //if not using the preview window, then
        //focus events must be tracked

        On Error GoTo ErrHandler

        if (!Settings.ShowPreview) {
          MDIHasFocus = false
          if (!ActiveControl = null) {
            //if active control is still tvwResources
            //then an open form may have been clicked
            if (ActiveControl.Name = "tvwResources" && !KeepFocus) {
              if (!ActiveMdiChild = null) {
                //reset menus for active form
                ActiveMdiChild.Activate
              }
            } else {
              //if focus is staying on the form
              if (ActiveControl.Name = "picProperties" && ActiveControl.Name = "picSplitRes" && ActiveControl.Name = "picSplitV") {
                //force focus marker back to true
                MDIHasFocus = true
              }
            }
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void txtProperty_Change()

        Dim tmpSS As Long

        On Error GoTo ErrHandler

        //if editing ID
        if (txtProperty.Tag = "GAMEID") {
          //version2 limited to 6 chars
          //version3 limited to 5 chars
          if (Asc(InterpreterVersion) = 50) {
            //limit to 6 characters
            if (Len(txtProperty.Text) > 6) {
              tmpSS = txtProperty.SelStart
              txtProperty.Text = Left$(txtProperty.Text, 6)
              txtProperty.SelStart = tmpSS
            }
          } else {
            //limit to 5 characters
            if (Len(txtProperty.Text) > 5) {
              tmpSS = txtProperty.SelStart
              txtProperty.Text = Left$(txtProperty.Text, 5)
              txtProperty.SelStart = tmpSS
            }
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void txtProperty_KeyPress(KeyAscii As Integer)

        //trap enter key
        switch (KeyAscii
        case 10 //ctrl-enter key combination
          //if not description or about
          if (txtProperty.Tag != "GAMEDESC" && txtProperty.Tag != "GAMEABOUT") {
            //save the property value
            KeyAscii = 0
            SelectPropFromText
            return;
          }

        case 13 //enter key
          //save the prop value
          KeyAscii = 0
          SelectPropFromText
          return;

        case 27 //esc key
          //cancel
          KeyAscii = 0
          txtProperty.Visible = false
          PaintPropertyWindow
          return;

        default:
          switch (txtProperty.Tag
          case "MAXOBJ"
            //limit to numbers only
            if (KeyAscii < 48 && KeyAscii > 57) {
              KeyAscii = 0
            }

          case "GAMEID"
            //validate- cant have  "\/:*?<>| or extended chars
            switch (KeyAscii
            case 32, 34, 42, 47, 58, 60, 62, 63, 92, 124, Is > 127
              KeyAscii = 0

            //force upper case
            case 97 To 122 // if (KeyAscii >= 97 && KeyAscii <= 122) {
              KeyAscii = KeyAscii - 32
            }

          case "RESDIR"
            //validate- cant have  "\/:*?<>|
            switch (KeyAscii
            case 32, 34, 42, 47, 58, 60, 62, 63, 92, 124
              KeyAscii = 0
            }

          }
        }
      }

      void txtProperty_LostFocus()

        On Error GoTo ErrHandler

        //make sure it is not visible
        if (txtProperty.Visible) {
          txtProperty.Visible = false
          PaintPropertyWindow
        }

      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuEClear_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickClear
      }

      void mnuECopy_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickCopy
      }

      void mnuECut_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickCut
      }


      void mnuEDelete_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickDelete
      }

      void mnuEFind_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickFind
      }

      void mnuEFindAgain_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickFindAgain
      }

      void mnuEInsert_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickInsert
      }

      void mnuEPaste_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickPaste
      }

      void mnuESelectAll_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickSelectAll
      }

      void mnuEUndo_Click()
        //undo an action

        On Error Resume Next

        ActiveMdiChild.MenuClickUndo
      }

      void mnuGCompile_Click()

        //compile game in default directory
        CompileAGIGame GameDir
      }

      void mnuGExit_Click()

        //unload
        Unload Me
      }

      void mnuRDescription_Click()

      void mnuGRebuild_Click()

        //rebuild volfiles only
        CompileAGIGame GameDir, true
      }

      void mnuRCustom1_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickCustom1
      }

      void mnuRCustom2_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickCustom2
      }


      void mnuRCustom3_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickCustom3
      }


      void mnuRExport_Click()

        Dim i As Long

        On Error GoTo ErrHandler

        //if no form is active
        if (ActiveMdiChild = null) {
          //can only mean that Settings.ShowPreview is false,
          //AND Settings.ResListType is non-zero, AND no editor window are open
          //use selected item method
          SelectedItemExport
        } else {
          //if active form is preview window OR the mdi form is active
          if (ActiveMdiChild.Name != "frmPreview") {
            //use the active form method
            ActiveMdiChild.MenuClickExport
          } else {
            //check for an open editor that matches resource being previewed
            switch (SelResType
            case AGIResType.Logic
              //if any logic editor matches this resource
              For i = 1 To LogicEditors.Count
                if (LogicEditors(i).FormMode = fmLogic) {
                  if (LogicEditors(i).LogicNumber = SelResNum) {
                    //use this form//s method
                    LogicEditors(i).MenuClickExport
                    return;
                  }
                }
              Next i

            case AGIResType.Picture
              //if any Picture editor matches this resource
              For i = 1 To PictureEditors.Count
                if (PictureEditors(i).PicNumber = SelResNum) {
                  //use this form//s method
                  PictureEditors(i).MenuClickExport
                  return;
                }
              Next i

            case AGIResType.Sound
              //if any Sound editor matches this resource
              For i = 1 To SoundEditors.Count
                if (SoundEditors(i).SoundNumber = SelResNum) {
                  //use this form//s method
                  SoundEditors(i).MenuClickExport
                  return;
                }
              Next i

            case AGIResType.View
              //if any View editor matches this resource
              For i = 1 To ViewEditors.Count
                if (ViewEditors(i).ViewNumber = SelResNum) {
                  //use this form//s method
                  ViewEditors(i).MenuClickExport
                  return;
                }
              Next i

            case rtWords
              //if using editor
              if (WEInUse) {
                //use word editor
                WordEditor.MenuClickExport
                return;
              }

            case rtObjects
              //if using editor
              if (OEInUse) {
                //use Objects Editor
                ObjectEditor.MenuClickExport
                return;
              }

            case rtGame
              ExportAll
              return;

            default: //text, game or none
              //export does not apply

            }
            //if no open editor is found, use the selected item method
            SelectedItemExport
          }
        }
      return;

      ErrHandler:
        Resume Next
      }
      void mnuRILogic_Click()

        On Error GoTo ErrHandler

        //get import file name
        With OpenDlg
          .Flags = cdlOFNHideReadOnly
          .DialogTitle = "Import Logic"
          .DefaultExt = ""
          .Filter = "AGI Logic Resource (*.agl)|*.agl|Logic Source Files (*.lgc)|*.lgc|Text files(*.txt)|*.txt|All files (*.*)|*.*"
          .FilterIndex = GameSettings.GetSetting(sLOGICS, sOPENFILTER, 2)
          .FileName = ""
          .InitDir = DefaultResDir

          .ShowOpen
          //save default filter index
          GameSettings.WriteSetting(sLOGICS, sOPENFILTER, .FilterIndex
          DefaultResDir = JustPath(.FileName)
        End With

        if (!ActiveMdiChild = null) {
          //if a logic editor is currently open AND it is in a game
          if (ActiveMdiChild.Name = "frmLogicEdit") {
            if (ActiveMdiChild.InGame) {
              //ask user if this is supposed to replace the existing logic
              if (MessageBox.Show(("Do you want to replace the logic you are currently editing?", vbYesNo, "Import Logic") = vbYes) {
                //use the active form//s import function
                ActiveMdiChild.MenuClickImport
                return;
              }
            }
          }
        }

        //import new logic
        NewLogic OpenDlg.FileName
      return;

      ErrHandler:
        //if user canceled the dialogbox,
        if (Err.Number = cdlCancel) {
          return;
        }

        //Debug.Assert false
        Resume Next
      }
      void mnuRAddRemove_Click()

        AddOrRemoveRes
      }

      void mnuRIObjects_Click()

        //imports an object file
        //if a game is loaded, ask if it should
        //overwrite the existing object file
        //if no, or if no game loaded, open it for editing

        Dim rtn As VbMsgBoxResult, blnObjOpen As Boolean

        On Error GoTo ErrHandler

        //show wait cursor
        WaitCursor

        Do
          //first, open an object file, but DON//T get current object file
          blnObjOpen = OpenObjects(false)

          if (!blnObjOpen) {
            Exit Do
          }

          //a game is loaded; find out if user wants this OBJECT file to replace existing
          if (GameLoaded) {
            if (MessageBox.Show(("Do you want to replace the existing OBJECT file with this one?", vbQuestion + vbYesNo, "Replace OBJECT File") = vbYes) {
              //if existing object file is being edited
              if (OEInUse) {
                //if it is dirty,
                if (ObjectEditor.IsDirty) {
                  //warn user
                  rtn = MessageBox.Show(("Do you want to save your changes and export the existing object file before you replace it?", vbQuestion + vbYesNoCancel, "Replace OBJECT File")
                } else {
                  rtn = MessageBox.Show(("Do you want to export the existing object file before you replace it?", vbQuestion + vbYesNoCancel, "Replace OBJECT File")
                }

                //if cancel
                if (rtn = vbCancel) {
                  Exit Do
                }

                //if yes
                if (rtn = vbYes) {
                  //if existing object file is dirty
                  if (ObjectEditor.IsDirty) {
                    //save it first
                    ObjectEditor.MenuClickSave
                    //if a problem, dirty will still be set
                    if (ObjectEditor.IsDirty) {
                      Exit Do
                    }
                  }
                  //then export it(ignoring errors)
                  ObjectEditor.MenuClickExport
                }

                //close the objecteditor object
                Unload ObjectEditor
              }

              //active form is now the Objects Editor
              OEInUse = true
              ObjectEditor = ActiveMdiChild

              //set ingame status for this object
              ObjectEditor.InGame = true
              //set resfile to ingame resfile
              ObjectEditor.ObjectsEdit.ResFile = GameDir + "OBJECT"

              //reset caption
              ObjectEditor.Caption = "Objects Editor - " + GameID
              ObjectEditor.IsDirty = false

              //copy this object list into game object and save
              InvObjects.SetObjects ObjectEditor.ObjectsEdit
              InvObjects.IsDirty = true
              InvObjects.Save

              //if previewing the objects
              if (SelResType = rtObjects) {
                //repaint properties
                PaintPropertyWindow
              }

              //adjust menus accordingly
              AdjustMenus rtWords, true, true, false
            } else {
              //not replacing; adjust menus accordingly
              AdjustMenus rtWords, false, true, false
            }
          }
        Loop Until true

      Screen.MousePointer = vbDefault

      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuRIPicture_Click()

        On Error GoTo ErrHandler

        //get import file name
        With OpenDlg
          .Flags = cdlOFNHideReadOnly
          .DialogTitle = "Import Picture"
          .DefaultExt = ""
          .Filter = "AGI Picture Resource (*.agp)|*.agp|All files (*.*)|*.*"
          .FilterIndex = GameSettings.GetSetting(sPICTURES, sOPENFILTER, 1)
          .FileName = ""
          .InitDir = DefaultResDir

          .ShowOpen
          //save default filter index
          GameSettings.WriteSetting(sPICTURES, sOPENFILTER, .FilterIndex
          DefaultResDir = JustPath(.FileName)
        End With

        if (!ActiveMdiChild = null) {
          //if a picture editor is currently open AND it is in a game
          if (ActiveMdiChild.Name = "frmPictureEdit") {
            if (ActiveMdiChild.InGame) {
              //ask user if this is supposed to replace existing picture
              if (MessageBox.Show(("Do you want to replace the picture you are currently editing?", vbYesNo, "Import Picture") = vbYes) {
                //use the active form//s import function
                ActiveMdiChild.MenuClickImport
                return;
              }
            }
          }
        }

        //import the new picture,and open it for editing
        NewPicture OpenDlg.FileName
      return;

      ErrHandler:
        //if user canceled the dialogbox,
        if (Err.Number = cdlCancel) {
          return;
        }

        //Debug.Assert false
        Resume Next
      }

      void mnuRISound_Click()

        On Error GoTo ErrHandler

        With OpenDlg
          .Flags = cdlOFNHideReadOnly
          .DialogTitle = "Import Sound"
          .DefaultExt = ""
          .Filter = "AGI Sound Resource (*.ags)|*.ags|Sound Script Files (*.ass)|*.ass|All files (*.*)|*.*"
          .FilterIndex = GameSettings.GetSetting(sSOUNDS, sOPENFILTER, 1)
          .FileName = ""
           .InitDir = DefaultResDir

          .ShowOpen
          //save default filter index
          GameSettings.WriteSetting(sSOUNDS, sOPENFILTER, .FilterIndex
          DefaultResDir = JustPath(.FileName)
        End With

        if (!ActiveMdiChild = null) {
          //if a sound editor is currently open AND it is in a game
          if (ActiveMdiChild.Name = "frmSoundEdit") {
            if (ActiveMdiChild.InGame) {
              //ask user if this is supposed to replace existing sound
              if (MessageBox.Show(("Do you want to replace the sound you are currently editing?", vbYesNo, "Import Sound") = vbYes) {
                //use the active form//s import function
                ActiveMdiChild.MenuClickImport
                return;
              }
            }
          }
        }

        //import a new sound
        NewSound OpenDlg.FileName
      return;

      ErrHandler:
        //if user canceled the dialogbox,
        if (Err.Number = cdlCancel) {
          return;
        }

        //Debug.Assert false
        Resume Next
      }
      void mnuRIView_Click()

        On Error GoTo ErrHandler

        With OpenDlg
          .Flags = cdlOFNHideReadOnly
          .DialogTitle = "Import View"
          .DefaultExt = ""
          .Filter = "AGI View Resource (*.agv)|*.agv|All files (*.*)|*.*"
          .FilterIndex = GameSettings.GetSetting(sVIEWS, sOPENFILTER, 1)
          .FileName = ""
          .InitDir = DefaultResDir

          .ShowOpen
          //save default filter index
          GameSettings.WriteSetting(sVIEWS, sOPENFILTER, .FilterIndex
          DefaultResDir = JustPath(.FileName)
        End With

        if (!ActiveMdiChild = null) {
          //if a view editor is currently open AND it is in a game
          if (ActiveMdiChild.Name = "frmViewEdit") {
            if (ActiveMdiChild.InGame) {
              //ask user if this is supposed to replace existing view
              if (MessageBox.Show(("Do you want to replace the view you are currently editing?", vbYesNo, "Import View") = vbYes) {
                //use the active form//s import function
                ActiveMdiChild.MenuClickImport
                return;
              }
            }
          }
        }

        //import new view
        NewView OpenDlg.FileName
      return;

      ErrHandler:
        //if user canceled the dialogbox,
        if (Err.Number = cdlCancel) {
          return;
        }

        //Debug.Assert false
        Resume Next
      }
      void mnuRIWords_Click()

        //open a word file
        //if a game is loaded, ask if it should
        //overwrite the existing word file
        //if no, or if no game loaded, open it for editing

        Dim rtn As VbMsgBoxResult, blnOpenWords As Boolean

        On Error GoTo ErrHandler

        //show wait cursor
        WaitCursor

        //first, open a words file, but
        //DONT get current word file
        Do
          blnOpenWords = OpenWords(false)
          if (!blnOpenWords) {
            Exit Do
          }

          //a game is loaded; find out if user wants this words file to replace existing
          if (GameLoaded) {
            if (MessageBox.Show(("Do you want to replace the existing WORDS.TOK file with this one?", vbQuestion + vbYesNo, "Replace WORDS.TOK File") = vbYes) {
              //if existing words file is being edited
              if (WEInUse) {
                //if it is dirty,
                if (WordEditor.IsDirty) {
                  //warn user
                  rtn = MessageBox.Show(("Do you want to save your changes and export the existing WORDS.TOK file before you replace it?", vbQuestion + vbYesNoCancel, "Replace WORDS.TOK File")
                } else {
                  rtn = MessageBox.Show(("Do you want to export the existing WORDS.TOK file before you replace it?", vbQuestion + vbYesNoCancel, "Replace WORDS.TOK File")
                }

                //if cancel
                if (rtn = vbCancel) {
                  Exit Do
                }

                //if yes
                if (rtn = vbYes) {
                  //if existing word file is dirty
                  if (WordEditor.IsDirty) {
                    //save it first
                    WordEditor.MenuClickSave
                    //if a problem, dirty will still be set
                    if (WordEditor.IsDirty) {
                      Exit Do
                    }
                  }
                  //then export it(ignoring errors)
                  WordEditor.MenuClickExport
                }

                //close the wordeditor word
                Unload WordEditor
              }

              //active form is now the word editor
              WEInUse = true
              WordEditor = ActiveMdiChild

              //set ingame status for this word
              WordEditor.InGame = true
              //set resfile to ingame resfile
              WordEditor.WordsEdit.ResFile = GameDir + "WORDS.TOK"

              //reset caption
              WordEditor.Caption = "Words Editor - " + GameID

              //force dirty status by clearing
              WordList.Clear
              //copy this word list into game word and save
              WordList.SetWords WordEditor.WordsEdit
              WordList.Save

              //if previewing the words
              if (SelResType = rtWords) {
                //repaint properties
                PaintPropertyWindow
              }

              //adjust menus accordingly
              AdjustMenus rtWords, true, true, false
            } else {
              //not replacing; adjust menus accordingly
              AdjustMenus rtWords, false, true, false
            }
          }
        Loop Until true
        Screen.MousePointer = vbDefault

      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuRNObjects_Click()

        //create a new object file
        //if a game is loaded, ask if it should
        //overwrite the existing object file
        //if no, or if no game loaded, open it for editing

        Dim rtn As VbMsgBoxResult

        On Error GoTo ErrHandler

        //first, create a new object file
        NewObjects

        //a game is loaded; find out if user wants this object file to replace existing
        if (GameLoaded) {
          if (MessageBox.Show(("Do you want to replace the existing OBJECT file with this one?", vbQuestion + vbYesNo, "Replace OBJECT File") = vbYes) {
            //if existing object file is being edited
            if (OEInUse) {
              //if it is dirty,
              if (ObjectEditor.IsDirty) {
                //warn user
                rtn = MessageBox.Show(("Do you want to save your changes and export the existing OBJECT file before you replace it?", vbQuestion + vbYesNoCancel, "Replace OBJECT File")
              } else {
                rtn = MessageBox.Show(("Do you want to export the existing OBJECT file before you replace it?", vbQuestion + vbYesNoCancel, "Replace OBJECT File")
              }

              //if cancel
              if (rtn = vbCancel) {
                return;
              }

              //if yes
              if (rtn = vbYes) {
                //if existing word file is dirty
                if (ObjectEditor.IsDirty) {
                  //save it first
                  ObjectEditor.MenuClickSave
                  //if a problem, dirty will still be set
                  if (ObjectEditor.IsDirty) {
                    return;
                  }
                }
                //then export it(ignoring errors)
                ObjectEditor.MenuClickExport
              }

              //close the ObjectEditor word
              Unload ObjectEditor
            }

            //active form is now the word editor
            OEInUse = true
            ObjectEditor = ActiveMdiChild

            //set ingame status for this word
            ObjectEditor.InGame = true
            //set resfile to ingame resfile
            ObjectEditor.ObjectsEdit.ResFile = GameDir + "OBJECT"

            //reset caption
            ObjectEditor.Caption = "Object Editor - " + GameID
            ObjectEditor.IsDirty = false

            //copy this word list into game word and save
            InvObjects.SetObjects ObjectEditor.ObjectsEdit
            InvObjects.IsDirty = true
            InvObjects.Save

            //if previewing the object
            if (SelResType = rtObjects) {
              //repaint properties
              PaintPropertyWindow
            }
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuRNText_Click()
        //create new text file
        NewTextFile

      }

      void mnuRNWords_Click()

        //create a new word list file
        //if a game is loaded, ask if it should
        //overwrite the existing word file
        //if no, or if no game loaded, open it for editing

        Dim rtn As VbMsgBoxResult

        On Error GoTo ErrHandler

        //first, create a new words file
        NewWords

        //a game is loaded; find out if user wants this words file to replace existing
        if (GameLoaded) {
          if (MessageBox.Show(("Do you want to replace the existing WORDS.TOK file with this one?", vbQuestion + vbYesNo, "Replace WORDS.TOK File") = vbYes) {
            //if existing words file is being edited
            if (WEInUse) {
              //if it is dirty,
              if (WordEditor.IsDirty) {
                //warn user
                rtn = MessageBox.Show(("Do you want to save your changes and export the existing WORDS.TOK file before you replace it?", vbQuestion + vbYesNoCancel, "Replace WORDS.TOK File")
              } else {
                rtn = MessageBox.Show(("Do you want to export the existing WORDS.TOK file before you replace it?", vbQuestion + vbYesNoCancel, "Replace WORDS.TOK File")
              }

              //if cancel
              if (rtn = vbCancel) {
                return;
              }

              //if yes
              if (rtn = vbYes) {
                //if existing word file is dirty
                if (WordEditor.IsDirty) {
                  //save it first
                  WordEditor.MenuClickSave
                  //if a problem, dirty will still be set
                  if (WordEditor.IsDirty) {
                    return;
                  }
                }
                //then export it(ignoring errors)
                WordEditor.MenuClickExport
              }

              //close the wordeditor word
              Unload WordEditor
            }

            //active form is now the word editor
            WEInUse = true
            WordEditor = ActiveMdiChild

            //set ingame status for this word
            WordEditor.InGame = true
            //set resfile to ingame resfile
            WordEditor.WordsEdit.ResFile = GameDir + "WORDS.TOK"

            //reset caption
            WordEditor.Caption = "Words Editor - " + GameID

            //force dirty status by clearing
            WordList.Clear
            //copy this word list into game word and save
            WordList.SetWords WordEditor.WordsEdit
            WordList.Save

            //if previewing the words
            if (SelResType = rtWords) {
              //repaint properties
              PaintPropertyWindow
            }
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuROLogic_Click()
        //open a logic for editing

        //if a game is loaded
        if (GameLoaded) {
          With frmGetResourceNum
            .WindowFunction = grOpen
            .ResType = AGIResType.Logic
            //setup before loading so ghosts don't show up
            .FormSetup
            //show the form
            KeepFocus = true
            .Show vbModal, Me
            KeepFocus = false

            //if not canceled
            if (!.Canceled) {
              //open the selected resource
              OpenLogic Val(.lstResNum.Text)
            }
          End With

          Unload frmGetResourceNum
        } else {
          //use import method
          mnuRILogic_Click
        }
      }

      void mnuROObjects_Click()

        //open the current object list for editing

        //if a game is loaded
        if (GameLoaded) {
          OpenObjects
        } else {
          //use import method
          mnuRIObjects_Click
        }
      }

      void mnuROPicture_Click()
        //open a picture for editing

        //if a game is loaded
        if (GameLoaded) {
          With frmGetResourceNum
            .WindowFunction = grOpen
            .ResType = AGIResType.Picture
            //setup before loading so ghosts don't show up
            .FormSetup
            //show the form
            KeepFocus = true
            .Show vbModal, Me
            KeepFocus = false

            //if not canceled
            if (!.Canceled) {
              //open the selected resource
              OpenPicture Val(.lstResNum.Text)
            }
          End With

          Unload frmGetResourceNum
        } else {
          //use import method
          mnuRIPicture_Click
        }
      }

      void mnuROSound_Click()
        //open a sound for editing

        //if a game is loaded
        if (GameLoaded) {
          With frmGetResourceNum
            .WindowFunction = grOpen
            .ResType = AGIResType.Sound
            //setup before loading so ghosts don't show up
            .FormSetup
            //show the form
            KeepFocus = true
            .Show vbModal, Me
            KeepFocus = false

            //if not canceled
            if (!.Canceled) {
              //open the selected resource
              OpenSound Val(.lstResNum.Text)
            }
          End With

          Unload frmGetResourceNum
        } else {
          //use import method
          mnuRISound_Click
        }
      }


      void mnuROText_Click()

        On Error GoTo ErrHandler

        //if no index yet, use default
        if (lngDefIndex = 0) {
          lngDefIndex = 1
        }

        //show dialog
        With OpenDlg
          .Flags = cdlOFNHideReadOnly
          .DialogTitle = "Open Text File"
          .DefaultExt = "txt"
          .Filter = "Text files (*.txt)|*.txt|Logic Source Files (*.lgc)|*.lgc|All files (*.*)|*.*"
          .FilterIndex = lngDefIndex
          .FileName = ""
          .InitDir = DefaultResDir
          .ShowOpen

          //open file
          OpenTextFile .FileName

          //save default directory
          DefaultResDir = JustPath(.FileName)

          //save default filter index
          lngDefIndex = .FilterIndex
        End With
      return;

      ErrHandler:
        //if user canceled the dialogbox,
        if (Err.Number = cdlCancel) {
          return;
        }

        //Debug.Assert false
        Resume Next
      }

      void mnuROView_Click()
        //open a view for editing

        //if a game is loaded
        if (GameLoaded) {
          With frmGetResourceNum
            .WindowFunction = grOpen
            .ResType = AGIResType.View
            //setup before loading so ghosts don't show up
            .FormSetup
            //show the form
            KeepFocus = true
            .Show vbModal, Me
            KeepFocus = false

            //if not canceled
            if (!.Canceled) {
              //open the selected resource
              OpenView Val(.lstResNum.Text)
            }
          End With

          Unload frmGetResourceNum
        } else {
          //use import method
          mnuRIView_Click
        }
      }

      void mnuROWords_Click()

        //opens the current word list for editing

        //if a game is loaded
        if (GameLoaded) {
          OpenWords
        } else {
          //use import method
          mnuRIWords_Click
        }
      }

      public void mnuRRenumber_Click()

        Dim i As Long

        On Error GoTo ErrHandler

        //if no form is active
        if (ActiveMdiChild = null) {
          //can only mean that Settings.ShowPreview is false,
          //AND Settings.ResListType is non-zero, AND no editor window are open
          //use selected item method
          SelectedItemRenumber
        } else {
          //if active form is NOT the preview form
          //if any form other than preview is active
          if (ActiveMdiChild.Name != "frmPreview") {
            //use the active form method
            ActiveMdiChild.MenuClickRenumber
          } else {
        //Debug.Assert SelResNum >= 0 && SelResNum <= 255
            //check for an open editor that matches resource being previewed
            switch (SelResType
            case AGIResType.Logic
              //if any logic editor matches this resource
              For i = 1 To LogicEditors.Count
                //verify the editor is a logic editor, not a text editor
                if (LogicEditors(i).FormMode = fmLogic) {
                  if (LogicEditors(i).LogicNumber = SelResNum && LogicEditors(i).InGame) {
                    //use this form//s method
                    LogicEditors(i).MenuClickRenumber
                    return;
                  }
                }
              Next i

            case AGIResType.Picture
              //if any Picture editor matches this resource
              For i = 1 To PictureEditors.Count
                if (PictureEditors(i).PicNumber = SelResNum && PictureEditors(i).InGame) {
                  //use this form//s method
                  PictureEditors(i).MenuClickRenumber
                  return;
                }
              Next i

            case AGIResType.Sound
              //if any Sound editor matches this resource
              For i = 1 To SoundEditors.Count
                if (SoundEditors(i).SoundNumber = SelResNum && SoundEditors(i).InGame) {
                  //use this form//s method
                  SoundEditors(i).MenuClickRenumber
                  return;
                }
              Next i

            case AGIResType.View
              //if any View editor matches this resource
              For i = 1 To ViewEditors.Count
                if (ViewEditors(i).ViewNumber = SelResNum && ViewEditors(i).InGame) {
                  //use this form//s method
                  ViewEditors(i).MenuClickRenumber
                  return;
                }
              Next i

            default: //words, objects, text, game or none
              //renumber doesn't apply

            }

            //if no open editor is found, use the selected item method
            SelectedItemRenumber
          }
        }
      return;

      ErrHandler:
        Resume Next
      }

      void mnuRSave_Click()

        On Error Resume Next

        ActiveMdiChild.MenuClickSave
      }

      void mnuHAbout_Click()


        frmAbout.Left = MDIMain.Left + (MDIMain.Width - frmAbout.Width) / 2
        frmAbout.Top = MDIMain.Top + (MDIMain.Height - frmAbout.Height) / 2
        KeepFocus = true
        frmAbout.Show vbModal, Me
        KeepFocus = false
      }

      public void mnuHContents_Click()

         Dim hWndHelp As Long
         //select table of contents
         hWndHelp = HtmlHelp(HelpParent, WinAGIHelp, HH_DISPLAY_TOC, 0)
      }

      void mnuWiClose_Click()

        //make sure this is not preview window
        if (MDIMain.ActiveMdiChild.Name != "frmPreview") {
          Unload ActiveMdiChild
        }
      }

      void Toolbar1_ButtonClick(ByVal Button As MSComctlLib.Button)

        On Error GoTo ErrHandler

        //determine which button is pressed:
        switch (Button.Key
        case "open"
          mnuGOpen_Click

        case "close"
            mnuGClose_Click

        case "run"
          mnuGRun_Click

        case "open_r"
          switch (Val(Button.Tag)
          case 0
            mnuROLogic_Click
          case 1
            mnuROPicture_Click
          case 2
            mnuROSound_Click
          case 3
            mnuROView_Click
          }

        case "import_r"
          switch (Val(Button.Tag)
          case 0
            mnuRILogic_Click
          case 1
            mnuRIPicture_Click
          case 2
            mnuRISound_Click
          case 3
            mnuRIView_Click
          }

        case "worded"
          mnuROWords_Click

        case "objed"
          mnuROObjects_Click

        case "save"
            mnuRSave_Click

        case "remove"
            mnuRAddRemove_Click

        case "export"
            mnuRExport_Click

        case "layout"
          mnuTLayout_Click

        case "menu"
          mnuTMenuEditor_Click

        case "globals"
          OpenGlobals

        case "help"
          //if a form is active
          if (!ActiveMdiChild = null) {
            On Error Resume Next
            ActiveMdiChild.MenuClickHelp
            if (Err.Number != 0) {
            //Debug.Assert false
              mnuHContents_Click
            }
          } else {
            //no form; use main menu click
            mnuHContents_Click
          }

        }
      return;

      ErrHandler:
        //Debug.Assert false
        Err.Clear
        Resume Next
      }
      public void MouseWheel(ByVal MouseKeys As Long, ByVal Rotation As Long, ByVal xPos As Long, ByVal yPos As Long)

        Dim NewValue As Long, lngTarget As Long
        Dim Lstep As Single

        On Error Resume Next

        //if any mousekeys
        if (MouseKeys != 0) {
          //just exit
          return;
        }

        //convert pos values
        xPos = xPos * ScreenTWIPSX
        yPos = yPos * ScreenTWIPSY

        //detrrmine control being wheeled
        With fgWarnings
          if (.Visible) {
            if (xPos > .Left && xPos < .Left + .Width && yPos > .Top && yPos < .Top + .Height) {
              lngTarget = 1
            }
          }
        End With

        switch (lngTarget
        case 1 // warnings list

          With fgWarnings
            Lstep = 4

            if (Rotation > 0) {
              NewValue = .TopRow - Lstep
              if (NewValue < 1) {
                NewValue = 1
              }
            } else {
              NewValue = .TopRow + Lstep
              if (NewValue > .Rows - 1) {
                NewValue = .Rows - 1
              }
            }
            .TopRow = NewValue
          End With

        }

      }



      void tvwResources_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim tmpNode As Node

        On Error GoTo ErrHandler

        //store x and Y values for checking in double click
        mTX = X
        mTY = Y

        //if right clicked
        if (Button = vbRightButton) {
          //get the node being clicked
          tmpNode = tvwResources.HitTest(X, Y)
          //if nothing
          if (tmpNode = null) {
            //exit
            return;
          }

          //if root or heading
          switch (tmpNode.Index
          case 1 //root
            return;

          case 2 To 7 //resource header, word, obj
            //force preview on right mouse click
            ForcePreview = true
            //select and display the selected item
            tmpNode.Selected = true
            tvwResources_NodeClick tvwResources.SelectedItem
            //reset forcepreview
            ForcePreview = false

            //if error
            if (Err.Number != 0) {
              //exit
              return;
            }
            //need doevents so form activation occurs BEFORE popup
            //otherwise, errors will be generated because of menu
            //adjustments that are made in the form_activate event
            SafeDoEvents
            PopupMenu mnuResource, , X + tvwResources.Left, Y + picResources.Top

          default:
            //a resource

            //force preview on right mouse click
            ForcePreview = true
            //select and display the selected item
            tmpNode.Selected = true
            tvwResources_NodeClick tvwResources.SelectedItem
            //reset forcepreview
            ForcePreview = false

            //if error
            if (Err.Number != 0) {
              //exit
              return;
            }

            //adjust the resource menu
            mnuRNew.Visible = false
            mnuROpen.Visible = false
            mnuRImport.Visible = false
            //use bar0 for open command
            mnuRBar0.Caption = "Open " + ResTypeName(SelResType)

            //need doevents so form activation occurs BEFORE popup
            //otherwise, errors will be generated because of menu
            //adjustments that are made in the form_activate event
            SafeDoEvents
            PopupMenu mnuResource, , X + tvwResources.Left, Y + picResources.Top

            //restore menu settings
            mnuRNew.Visible = true
            mnuROpen.Visible = true
            mnuRImport.Visible = true
            mnuRBar0.Caption = "-"
            mnuRBar2.Visible = true
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }
            */
        }
        public void ClearResourceList() {
            // reset the navigation queue
            ResetQueue();
            // don't add to queue while clearing
            DontQueue = true;
            // list type determines clear actions
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
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
                break;
            case agiSettings.EResListType.ComboList:
                cmbResType.Items[0] = EditGame.GameID;
                //select top item (game level)
                cmbResType.SelectedIndex = 0;
                break;
            }
            //allow queuing
            DontQueue = false;
        }
        public void ShowResTree() {
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.None:
                // no tree
                // shouldn't get here, but
                return;

            case agiSettings.EResListType.TreeList:
                tvwResources.Visible = true;
                cmbResType.Visible = false;
                lstResources.Visible = false;
                // change font to match current preview font
                tvwResources.Font = new Font(WinAGISettings.PFontName, WinAGISettings.PFontSize);
                break;
            case agiSettings.EResListType.ComboList:
                // combo/list boxes
                tvwResources.Visible = false;
                // set combo and listbox height, and set fonts
                cmbResType.Visible = true;
                cmbResType.Font = new Font(WinAGISettings.EFontName, WinAGISettings.EFontSize);
                lstResources.Top = cmbResType.Top + cmbResType.Height + 2;
                lstResources.Visible = true;
                lstResources.Font = new Font(WinAGISettings.PFontName, WinAGISettings.PFontSize);
                break;
            }
            // show and position the resource list panels
            pnlResources.Visible = true;
            splitResource.Visible = true;
        }
        private void frmMDIMain_FormClosing(object sender, FormClosingEventArgs e) {
            bool blnLastLoad;

            //is a game loaded?
            if (EditGame is null) {
                blnLastLoad = false;
            }
            else {
                blnLastLoad = EditGame is not null;
            }
            if (blnLastLoad) {
                //close open game (get cancel flag, in case user cancels)
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
            //unload any remaining open forms
            foreach (Form frm in this.MdiChildren) {
                //store current form Count
                int i = MdiChildren.Length;
                //unload the form
                frm.Close();
                //if Count did not decrease
                if (i == MdiChildren.Length) {
                    //user canceled
                    e.Cancel = true;
                    return;
                }
            }
            //write lastload status
            WinAGISettingsList.WriteSetting(sMRULIST, "LastLoad", blnLastLoad);
            //save settings to register
            SaveSettings();

            // detach  AGI game events
            NewGameStatus -= MDIMain.GameEvents_NewGameStatus;
            LoadGameStatus -= MDIMain.GameEvents_LoadGameStatus;
            CompileGameStatus -= MDIMain.GameEvents_CompileGameStatus;
            CompileLogicStatus -= MDIMain.GameEvents_CompileLogicStatus;
            DecodeLogicStatus -= MDIMain.GameEvents_DecodeLogicStatus;

            tvwResources.ContextMenuStrip.Opening -= mnuResources_DropDownOpening;
            lstResources.ContextMenuStrip.Opening -= mnuResources_DropDownOpening;

            // drop the global reference
            MDIMain = null;
        }

        private void splResource_SplitterMoved(object sender, SplitterEventArgs e) {
            if (splResource.Visible) {
                if (splResource.Panel2.Height > MAX_PROPGRID_HEIGHT) {
                    splResource.SplitterDistance = splResource.Height - MAX_PROPGRID_HEIGHT;
                }
            }
        }

        private void splResource_SizeChanged(object sender, EventArgs e) {
            if (splResource.Panel2.Height > MAX_PROPGRID_HEIGHT) {
                splResource.SplitterDistance = splResource.Height - MAX_PROPGRID_HEIGHT;
            }
        }

        private void mnuTSettings_Click(object sender, EventArgs e) {
            // temp code for dev purposes


            WinAGISettings.ResListType++;
            // move resource menu items to appropriate context menu
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                tvwResources.ContextMenuStrip.Items.AddRange([mnuRNew, mnuROpen, mnuRImport, mnuRSeparator1, mnuROpenRes, mnuRSave, mnuRExport, mnuRSeparator2, mnuRAddRemove, mnuRRenumber, mnuRIDDesc, mnuRSeparator3, mnuRCompileLogic, mnuRSavePicImage, mnuRExportGIF]);
                break;
            case agiSettings.EResListType.ComboList:
                lstResources.ContextMenuStrip.Items.AddRange([mnuRNew, mnuROpen, mnuRImport, mnuRSeparator1, mnuROpenRes, mnuRSave, mnuRExport, mnuRSeparator2, mnuRAddRemove, mnuRRenumber, mnuRIDDesc, mnuRSeparator3, mnuRCompileLogic, mnuRSavePicImage, mnuRExportGIF]);
                break;
            }

            if (WinAGISettings.ResListType == (agiSettings.EResListType)3) {
                WinAGISettings.ResListType = agiSettings.EResListType.None;
            };
            if (EditGame != null) {
                // force refresh of type
                switch (WinAGISettings.ResListType) {
                case agiSettings.EResListType.None:
                    if (splResource.Visible) {
                        MDIMain.HideResTree();
                    }
                    if (WinAGISettings.ShowPreview) {
                        PreviewWin.Visible = false;
                    }
                    break;
                default:
                    if (splResource.Visible) {
                        // reset to root, then switch
                        SelResType = Game;
                        SelResNum = -1;
                        BuildResourceTree();
                    }
                    ShowResTree();
                    if (WinAGISettings.ShowPreview) {
                        PreviewWin.Visible = true;
                    }
                    break;
                }
            }
        }

        private void lstResources_SizeChanged(object sender, EventArgs e) {
            // always adjust column to fill entire listbox
            lstResources.Columns[0].Width = lstResources.Width - 4;
        }

        private void fgWarnings_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e) {

        }

        private void fgWarnings_SortCompare(object sender, DataGridViewSortCompareEventArgs e) {
            // force numbers as text to sort as numbers, above text
            if (e.Column.Index == 2 || e.Column.Index == 3) {
                //
                if (!int.TryParse(e.CellValue1.ToString(), out int val1)) {
                    val1 = -1;
                }
                if (!int.TryParse(e.CellValue2.ToString(), out int val2)) {
                    val2 = -1;
                }
                if (val1 > val2) {
                    e.SortResult = 1;
                }
                else if (val1 < val2) {
                    e.SortResult = -1;
                }
                else {
                    e.SortResult = 0;
                }
                e.Handled = true;
            }
        }

        private void fgWarnings_Sorted(object sender, EventArgs e) {


        }

        /*
        private void cmsResources_Opening(object sender, CancelEventArgs e) {
            // TODO: need to account for resource errors
            string resType;
            byte resNum;
            // set up context menu
            if (EditGame == null) {
                cmiImport.Enabled = false;
                cmiOpenRes.Visible = false;
                cmiSave.Visible = true;
                cmiSave.Text = "Save Resource";
                cmiSave.Enabled = false;
                cmiExport.Visible = false;
                cmiAddRemove.Visible = true;
                cmiAddRemove.Text = "Add to Game";
                cmiAddRemove.Enabled = false;
                cmiRenumber.Visible = true;
                cmiRenumber.Text = "Renumber Resource";
                cmiRenumber.Enabled = false;
                cmiIDDesc.Visible = false;
                cmiSeparator3.Visible = false;
                cmiCompileLogic.Visible = false;
                cmiSavePicImage.Visible = false;
                cmiExportGIF.Visible = false;
                return;
            }

            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                switch (tvwResources.SelectedNode.Level) {
                case 0:
                    // root - gameID
                    cmiNew.Enabled = true;
                    cmiOpen.Enabled = true;
                    cmiImport.Enabled = true;
                    cmiSeparator1.Visible = true;
                    cmiOpenRes.Visible = false;
                    cmiSave.Visible = true;
                    cmiSave.Text = "Save Resource";
                    cmiSave.Enabled = false;
                    cmiExport.Visible = false;
                    cmiAddRemove.Visible = true;
                    cmiAddRemove.Text = "Add to Game";
                    cmiAddRemove.Enabled = false;
                    cmiRenumber.Visible = true;
                    cmiRenumber.Enabled = false;
                    cmiSeparator3.Visible = false;
                    cmiCompileLogic.Visible = false;
                    cmiSavePicImage.Visible = false;
                    cmiExportGIF.Visible = false;
                    break;
                case 1:
                    // resource header/object/words.tok
                    switch (tvwResources.SelectedNode.Index) {
                    case 0 or 1 or 2 or 3:
                        cmiNew.Visible = true;
                        cmiNew.Enabled = true;
                        cmiOpen.Visible = true;
                        cmiOpen.Enabled = true;
                        cmiImport.Visible = true;
                        cmiImport.Enabled = true;
                        cmiSeparator1.Visible = true;
                        cmiOpenRes.Visible = false;
                        cmiSave.Visible = true;
                        cmiSave.Text = "Save Resource";
                        cmiSave.Enabled = false;
                        cmiExport.Visible = false;
                        cmiAddRemove.Visible = true;
                        cmiAddRemove.Text = "Add to Game";
                        cmiAddRemove.Enabled = false;
                        cmiRenumber.Visible = true;
                        cmiRenumber.Text = "Renumber Resource";
                        cmiRenumber.Enabled = false;
                        cmiIDDesc.Visible = false;
                        cmiCompileLogic.Visible = false;
                        if (tvwResources.SelectedNode.Index == (int)AGIResType.Picture) {
                            cmiSeparator3.Visible = true;
                            cmiSavePicImage.Visible = true;
                            cmiSavePicImage.Text = "Export All Picture Images...";
                            cmiSavePicImage.Enabled = true;
                        }
                        else {
                            cmiSeparator3.Visible = false;
                            cmiSavePicImage.Visible = false;
                        }
                        cmiExportGIF.Visible = false;
                        break;
                    case 4:
                        // OBJECT
                        cmiNew.Visible = false;
                        cmiOpen.Visible = false;
                        cmiImport.Visible = false;
                        cmiSeparator1.Visible = false;
                        cmiOpenRes.Text = "Open OBJECT file";
                        cmiOpenRes.Visible = true;
                        cmiSave.Visible = true;
                        cmiSave.Text = "Save OBJECT file";
                        cmiSave.Enabled = false;
                        cmiExport.Visible = true;
                        cmiExport.Text = "Export OBJECT file";
                        cmiExport.Enabled = true;
                        cmiAddRemove.Visible = false;
                        cmiRenumber.Visible = false;
                        cmiIDDesc.Visible = true;
                        cmiIDDesc.Text = "Description...";
                        cmiIDDesc.Enabled = true;
                        cmiSeparator3.Visible = false;
                        cmiCompileLogic.Visible = false;
                        cmiSavePicImage.Visible = false;
                        cmiExportGIF.Visible = false;
                        break;
                    case 5:
                        // WORDS.TOK
                        cmiNew.Visible = false;
                        cmiOpen.Visible = false;
                        cmiImport.Visible = false;
                        cmiSeparator1.Visible = false;
                        cmiOpenRes.Text = "Open WORDS.TOK file";
                        cmiOpenRes.Visible = true;
                        cmiSave.Visible = true;
                        cmiSave.Text = "Save WORDS.TOK file";
                        cmiSave.Enabled = false;
                        cmiExport.Visible = true;
                        cmiExport.Text = "Export WORDS.TOK file";
                        cmiExport.Enabled = true;
                        cmiAddRemove.Visible = false;
                        cmiRenumber.Visible = false;
                        cmiIDDesc.Visible = true;
                        cmiIDDesc.Text = "Description...";
                        cmiIDDesc.Enabled = true;
                        cmiSeparator3.Visible = false;
                        cmiCompileLogic.Visible = false;
                        cmiSavePicImage.Visible = false;
                        cmiExportGIF.Visible = false;
                        break;
                    }
                    break;
                case 2:
                    // resource
                    resType = ((AGIResType)tvwResources.SelectedNode.Parent.Index).ToString();
                    resNum = (byte)tvwResources.SelectedNode.Tag;
                    cmiNew.Visible = false;
                    cmiOpen.Visible = false;
                    cmiImport.Visible = false;
                    cmiSeparator1.Visible = false;
                    cmiOpenRes.Visible = true;
                    cmiOpenRes.Text = "Open " + resType;
                    cmiOpenRes.Enabled = true;
                    cmiSave.Visible = true;
                    cmiSave.Text = "Save " + resType;
                    cmiSave.Enabled = false;
                    cmiExport.Visible = true;
                    cmiExport.Text = "Export " + resType;
                    cmiExport.Enabled = true;
                    cmiAddRemove.Visible = true;
                    cmiAddRemove.Text = "Remove from Game";
                    cmiAddRemove.Enabled = true;
                    cmiRenumber.Visible = true;
                    cmiRenumber.Text = "Renumber " + resType;
                    cmiRenumber.Enabled = true;
                    cmiIDDesc.Visible = true;
                    cmiIDDesc.Text = "ID/Description...";
                    cmiIDDesc.Enabled = true;
                    cmiSavePicImage.Visible = false;
                    cmiExportGIF.Visible = false;
                    switch (tvwResources.SelectedNode.Parent.Index) {
                    case 0:
                        // Logic
                        if (EditGame.Logics[resNum].Compiled) {
                            cmiSeparator3.Visible = false;
                            cmiCompileLogic.Visible = false;
                        }
                        else {
                            cmiSeparator3.Visible = true;
                            cmiCompileLogic.Visible = true;
                        }
                        cmiSavePicImage.Visible = false;
                        cmiExportGIF.Visible = false;
                        //// if resource is invalid, override settings
                        //if (EditGame.Logics[resNum].ErrLevel < 0) {
                        //    cmiOpenResource.Enabled = false;
                        //    cmiExportResource.Enabled = false;
                        //    cmiRenumber.Enabled = false;
                        //    cmiID.Enabled = false;
                        //    cmiCustom1.Enabled = false;
                        //}
                        break;
                    case 1:
                        // Picture
                        cmiSeparator3.Visible = true;
                        cmiCompileLogic.Visible = false;
                        cmiSavePicImage.Visible = true;
                        cmiSavePicImage.Text = "Save Picture Image As...";
                        cmiSavePicImage.Enabled = true;
                        // if resource is invalid, override settings
                        if (EditGame.Pictures[resNum].ErrLevel < 0) {
                            cmiOpenRes.Enabled = false;
                            cmiExport.Enabled = false;
                            cmiRenumber.Enabled = false;
                            cmiIDDesc.Enabled = false;
                            cmiSavePicImage.Enabled = false;
                        }
                        cmiExportGIF.Visible = false;
                        break;
                    case 2:
                        // Sound
                        cmiSeparator3.Visible = false;
                        cmiCompileLogic.Visible = false;
                        cmiSavePicImage.Enabled = false;
                        cmiExportGIF.Visible = false;
                        // if resource is invalid, override settings
                        if (EditGame.Sounds[resNum].ErrLevel < 0) {
                            cmiOpenRes.Enabled = false;
                            cmiExport.Enabled = false;
                            cmiRenumber.Enabled = false;
                            cmiIDDesc.Enabled = false;
                        }
                        break;
                    case 3:
                        // View
                        cmiSeparator3.Visible = true;
                        cmiCompileLogic.Visible = false;
                        cmiSavePicImage.Enabled = false;
                        cmiExportGIF.Visible = true;
                        cmiExportGIF.Text = "Export Loop As GIF";
                        cmiExportGIF.Enabled = true;
                        // if resource is invalid, override settings
                        if (EditGame.Views[resNum].ErrLevel < 0) {
                            cmiOpenRes.Enabled = false;
                            cmiExport.Enabled = false;
                            cmiRenumber.Enabled = false;
                            cmiIDDesc.Enabled = false;
                            cmiExportGIF.Enabled = false;
                        }
                        break;
                    }
                    break;
                }
                break;
            case agiSettings.EResListType.ComboList:
                // display resource context menu, depending on current resource type
                switch (cmbResType.SelectedIndex) {
                case 0:
                    // nothing to do
                    e.Cancel = true;
                    return;
                case 5:
                    // OBJECT
                    cmiNew.Visible = false;
                    cmiOpen.Visible = false;
                    cmiImport.Visible = false;
                    cmiSeparator1.Visible = false;
                    cmiOpenRes.Text = "Open OBJECT file";
                    cmiOpenRes.Visible = true;
                    cmiSave.Visible = true;
                    cmiSave.Text = "Save OBJECT file";
                    cmiSave.Enabled = false;
                    cmiExport.Visible = true;
                    cmiExport.Text = "Export OBJECT file";
                    cmiExport.Enabled = true;
                    cmiAddRemove.Visible = false;
                    cmiRenumber.Visible = false;
                    cmiIDDesc.Visible = true;
                    cmiIDDesc.Text = "Description...";
                    cmiIDDesc.Enabled = true;
                    cmiSeparator3.Visible = false;
                    cmiCompileLogic.Visible = false;
                    cmiSavePicImage.Visible = false;
                    cmiExportGIF.Visible = false;
                    break;
                case 6:
                    // WORDS.TOK
                    cmiNew.Visible = false;
                    cmiOpen.Visible = false;
                    cmiImport.Visible = false;
                    cmiSeparator1.Visible = false;
                    cmiOpenRes.Text = "Open WORDS.TOK file";
                    cmiOpenRes.Visible = true;
                    cmiSave.Visible = true;
                    cmiSave.Text = "Save WORDS.TOK file";
                    cmiSave.Enabled = false;
                    cmiExport.Visible = true;
                    cmiExport.Text = "Export WORDS.TOK file";
                    cmiExport.Enabled = true;
                    cmiAddRemove.Visible = false;
                    cmiRenumber.Visible = false;
                    cmiIDDesc.Visible = true;
                    cmiIDDesc.Text = "Description...";
                    cmiIDDesc.Enabled = true;
                    cmiSeparator3.Visible = false;
                    cmiCompileLogic.Visible = false;
                    cmiSavePicImage.Visible = false;
                    cmiExportGIF.Visible = false;
                    break;
                default:
                    // logic/picture/sound/view resource
                    if (lstResources.SelectedItems.Count != 1) {
                        e.Cancel = true;
                        return;
                    }
                    resType = ((AGIResType)cmbResType.SelectedIndex - 1).ToString();
                    resNum = ((AGIResource)lstResources.SelectedItems[0].Tag).Number;
                    cmiNew.Visible = false;
                    cmiOpen.Visible = false;
                    cmiImport.Visible = false;
                    cmiSeparator1.Visible = false;
                    cmiOpenRes.Text = "Open " + resType;
                    cmiOpenRes.Visible = true;
                    cmiSave.Visible = true;
                    cmiSave.Text = "Save " + resType;
                    cmiSave.Enabled = false;
                    cmiExport.Visible = true;
                    cmiExport.Text = "Export " + resType;
                    cmiExport.Enabled = true;
                    cmiAddRemove.Visible = true;
                    cmiAddRemove.Text = "Remove from Game";
                    cmiAddRemove.Enabled = true;
                    cmiRenumber.Visible = true;
                    cmiRenumber.Text = "Renumber " + resType;
                    cmiRenumber.Enabled = true;
                    cmiIDDesc.Visible = true;
                    cmiIDDesc.Text = "ID/Description...";
                    cmiIDDesc.Enabled = true;
                    switch (cmbResType.SelectedIndex - 1) {
                    case 0:
                        // Logic
                        if (EditGame.Logics[resNum].Compiled) {
                            cmiSeparator3.Visible = false;
                            cmiCompileLogic.Visible = false;
                        }
                        else {
                            cmiSeparator3.Visible = true;
                            cmiCompileLogic.Visible = true;
                        }
                        cmiSavePicImage.Visible = false;
                        cmiExportGIF.Visible = false;
                        //// if resource is invalid, override settings
                        //if (EditGame.Logics[resNum].ErrLevel < 0) {
                        //    cmiOpenResource.Enabled = false;
                        //    cmiExportResource.Enabled = false;
                        //    cmiRenumber.Enabled = false;
                        //    cmiID.Enabled = false;
                        //    cmiCompileLogic.Enabled = false;
                        //}
                        break;
                    case 1:
                        // Picture
                        cmiSeparator3.Visible = true;
                        cmiCompileLogic.Visible = false;
                        cmiSavePicImage.Visible = true;
                        cmiSavePicImage.Text = "Save Picture Image As...";
                        cmiSavePicImage.Enabled = true;
                        cmiExportGIF.Visible = false;
                        // if resource is invalid, override settings
                        if (EditGame.Pictures[resNum].ErrLevel < 0) {
                            cmiOpenRes.Enabled = false;
                            cmiExport.Enabled = false;
                            cmiRenumber.Enabled = false;
                            cmiIDDesc.Enabled = false;
                            cmiSavePicImage.Enabled = false;
                        }
                        break;
                    case 2:
                        // Sound
                        cmiSeparator3.Visible = false;
                        cmiCompileLogic.Visible = false;
                        cmiSavePicImage.Enabled = false;
                        cmiExportGIF.Visible = false;
                        // if resource is invalid, override settings
                        if (EditGame.Sounds[resNum].ErrLevel < 0) {
                            cmiOpenRes.Enabled = false;
                            cmiExport.Enabled = false;
                            cmiRenumber.Enabled = false;
                            cmiIDDesc.Enabled = false;
                        }
                        break;
                    case 3:
                        // View
                        cmiSeparator3.Visible = true;
                        cmiCompileLogic.Visible = false;
                        cmiSavePicImage.Enabled = false;
                        cmiExportGIF.Visible = true;
                        cmiExportGIF.Text = "Export Loop As GIF";
                        cmiExportGIF.Enabled = true;
                        // if resource is invalid, override settings
                        if (EditGame.Views[resNum].ErrLevel < 0) {
                            cmiOpenRes.Enabled = false;
                            cmiExport.Enabled = false;
                            cmiRenumber.Enabled = false;
                            cmiIDDesc.Enabled = false;
                            cmiExportGIF.Enabled = false;
                        }
                        break;
                    }
                    break;
                }
                break;
            }
        }
        */

        private void mnuResources_DropDownOpening(object sender, EventArgs e) {
            // open
            // save
            // export
            // ----
            // add/remove
            // renumber
            // id/desc
            // ----
            // compile logic
            // export pic image
            // export loop gif
            if (sender.ToString() == "&Resources") {
                mnuResources.DropDownItems.AddRange([mnuRNew, mnuROpen, mnuRImport, mnuRSeparator1, mnuROpenRes, mnuRSave, mnuRExport, mnuRSeparator2, mnuRAddRemove, mnuRRenumber, mnuRIDDesc, mnuRSeparator3, mnuRCompileLogic, mnuRSavePicImage, mnuRExportGIF]);
            }
            // TODO: need to account for resource errors

            // adjust the menu depending on currently selected resource and game state
            if (EditGame is null) {
                // no game
                mnuRImport.Enabled = false;
                mnuROpenRes.Visible = false;
                mnuRSave.Visible = true;
                mnuRSave.Text = "Save Resource";
                mnuRSave.Enabled = false;
                mnuRExport.Visible = false;
                mnuRAddRemove.Visible = true;
                mnuRAddRemove.Text = "Add to Game";
                mnuRAddRemove.Enabled = false;
                mnuRRenumber.Visible = true;
                mnuRRenumber.Text = "Renumber Resource";
                mnuRRenumber.Enabled = false;
                mnuRIDDesc.Visible = false;
                mnuRSeparator3.Visible = false;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                return;
            }
            // if a game is loaded, import is also always available
            mnuRImport.Enabled = true;
            // OBJECT
            if (SelResType == Objects) {
                mnuROpenRes.Visible = false;
                mnuRSave.Visible = true;
                mnuRSave.Text = "Save OBJECT";
                mnuRSave.Enabled = false;
                mnuRExport.Visible = true;
                mnuRExport.Text = "Export OBJECT";
                mnuRExport.Enabled = true;
                mnuRAddRemove.Visible = true;
                mnuRAddRemove.Text = "Add to Game";
                mnuRAddRemove.Enabled = false;
                mnuRRenumber.Visible = false;
                mnuRIDDesc.Visible = true;
                mnuRIDDesc.Text = "Description...";
                mnuRIDDesc.Enabled = true;
                mnuRSeparator3.Visible = false;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                return;
            }
            // WORDS.TOK
            if (SelResType == Words) {
                mnuROpenRes.Visible = false;
                mnuRSave.Visible = true;
                mnuRSave.Text = "Save WORDS.TOK";
                mnuRSave.Enabled = false;
                mnuRExport.Visible = true;
                mnuRExport.Text = "Export WORDS.TOK";
                mnuRExport.Enabled = true;
                mnuRAddRemove.Visible = true;
                mnuRAddRemove.Text = "Add to Game";
                mnuRAddRemove.Enabled = false;
                mnuRRenumber.Visible = false;
                mnuRIDDesc.Visible = true;
                mnuRIDDesc.Text = "Description...";
                mnuRIDDesc.Enabled = true;
                mnuRSeparator3.Visible = false;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                return;
            }
            // check for header 
            if (SelResNum == -1) {
                mnuROpenRes.Visible = false;
                mnuRSave.Visible = true;
                mnuRSave.Text = "Save Resource";
                mnuRSave.Enabled = false;
                mnuRExport.Visible = false;
                mnuRAddRemove.Visible = true;
                mnuRAddRemove.Text = "Add to Game";
                mnuRAddRemove.Enabled = false;
                mnuRRenumber.Visible = true;
                mnuRRenumber.Text = "Renumber Resource";
                mnuRRenumber.Enabled = false;
                mnuRIDDesc.Visible = false;
                mnuRCompileLogic.Visible = false;
                if (SelResType == AGIResType.Picture) {
                    mnuRSeparator3.Visible = true;
                    mnuRSavePicImage.Visible = true;
                    mnuRSavePicImage.Text = "Export All Picture Images...";
                }
                else {
                    mnuRSeparator3.Visible = false;
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
            mnuRAddRemove.Visible = true;
            mnuRAddRemove.Text = "Remove from Game";
            mnuRAddRemove.Enabled = true;
            mnuRRenumber.Visible = true;
            mnuRRenumber.Text = "Renumber " + SelResType.ToString();
            mnuRIDDesc.Visible = true;
            mnuRIDDesc.Text = "ID/Description...";
            bool err = false;
            switch (SelResType) {
            case AGIResType.Logic:
                // error level doesn't affect logics
                // err = EditGame.Logics[SelResNum].ErrLevel < 0;
                mnuRSeparator3.Visible = !EditGame.Logics[SelResNum].Compiled;
                mnuRCompileLogic.Visible = !EditGame.Logics[SelResNum].Compiled;
                mnuRCompileLogic.Enabled = true;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                break;
            case AGIResType.Picture:
                err = EditGame.Pictures[SelResNum].ErrLevel < 0;
                mnuRSeparator3.Visible = true;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = true;
                mnuRSavePicImage.Enabled = !err;
                mnuRSavePicImage.Text = "Save Picture Image As...";
                mnuRExportGIF.Visible = false;
                break;
            case AGIResType.Sound:
                err = EditGame.Sounds[SelResNum].ErrLevel < 0;
                mnuRSeparator3.Visible = false;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = false;
                break;
            case AGIResType.View:
                err = EditGame.Views[SelResNum].ErrLevel < 0;
                mnuRSeparator3.Visible = true;
                mnuRCompileLogic.Visible = false;
                mnuRSavePicImage.Visible = false;
                mnuRExportGIF.Visible = true;
                mnuRExportGIF.Enabled = !err;
                break;
            }
            // if resource has an error, only add/remove is enabled
            mnuROpenRes.Enabled = !err;
            mnuRExport.Enabled = !err;
            mnuRRenumber.Enabled = !err;
            mnuRIDDesc.Enabled = !err;
        }

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
            MessageBox.Show(this, "This is WinAGI.", "About WinAGI", MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: for controls, use the value 'Topic' for 'HelpNavigator' and
            // set the 'HelpKeyword' field to the topic name (i.e. "htm\winagi\restree.htm#propwindow")
            // DON'T use the 'HelpString' field
        }

        private void mnuTCustom_Click(object sender, EventArgs e) {
            ToolStripMenuItem thisTool = (ToolStripMenuItem)sender;
            string target = thisTool.Tag.ToString();

            // check for a url
            if ((Left(target, 4) == "http") || (Left(target, 4) == "www.")) {
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
                        FileName = target,
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

        private void mnuGame_DropDownOpening(object sender, EventArgs e) {
            mnuGClose.Enabled = EditGame != null;
            mnuGCompile.Enabled = EditGame != null;
            mnuGCompileTo.Enabled = EditGame != null;
            mnuGRun.Enabled = EditGame != null;
            mnuGRebuild.Enabled = EditGame != null;
            mnuGCompileDirty.Enabled = EditGame != null;
            mnuGProperties.Enabled = EditGame != null;
            mnuRImport.Enabled = EditGame != null;
        }

        private void mnuTools_DropDownOpening(object sender, EventArgs e) {
            mnuTLayout.Enabled = EditGame != null && EditGame.UseLE;
        }

        private void mnuTGlobals_Click(object sender, EventArgs e) {
            OpenGlobals(GEInUse);
        }

        private void mnuGProperties_Click(object sender, EventArgs e) {
            ShowProperties();
        }

        public void ShowProperties() {
            ShowProperties(false, "General", "");
        }

        public void ShowProperties(bool EnableOK, string StartTab = "", string StartProp = "") {
            // show properties form
            frmGameProperties propForm = new(GameSettingFunction.gsEdit);
            propForm.btnOK.Enabled = EnableOK;
            // check for valid starting tab
            if (StartTab.Length > 0) {
                propForm.StartTab = StartTab;
            }
            // check for starting control
            if (StartProp.Length > 0) {
                propForm.StartProp = StartProp;
            }
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
                EditGame.PlatformType = PlatformTypeEnum.None;
            }
            else {
                // platform
                if (propForm.optDosBox.Checked) {
                    EditGame.PlatformType = PlatformTypeEnum.DosBox;
                }
                else if (propForm.optScummVM.Checked) {
                    EditGame.PlatformType = PlatformTypeEnum.ScummVM;
                }
                else if (propForm.optNAGI.Checked) {
                    EditGame.PlatformType = PlatformTypeEnum.NAGI;
                }
                else if (propForm.optOther.Checked) {
                    EditGame.PlatformType = PlatformTypeEnum.Other;
                }
            }

            // platformdir OK as long as not nuthin
            if (EditGame.PlatformType > 0) {
                EditGame.Platform = propForm.NewPlatformFile;
                // platform options OK if dosbox or scummvm
                if (EditGame.PlatformType == PlatformTypeEnum.DosBox ||
                              EditGame.PlatformType == PlatformTypeEnum.ScummVM ||
                              EditGame.PlatformType == PlatformTypeEnum.Other) {
                    EditGame.PlatformOpts = propForm.txtOptions.Text;
                }
                else {
                    EditGame.PlatformOpts = "";
                }
                // dos executable only used if dosbox
                if (EditGame.PlatformType == PlatformTypeEnum.DosBox) {
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
            if (EditGame.ResDirName != propForm.txtResDir.Text.ToLower()) {
                ChangeResDir(propForm.txtResDir.Text);
            }
            if (EditGame.Logics.SourceFileExt != "." + propForm.txtSrcExt.Text) {
                try {
                    // rename any existing files
                    foreach (Logic aLogic in EditGame.Logics) {
                        File.Move(aLogic.SourceFile, EditGame.ResDir + Path.GetFileNameWithoutExtension(aLogic.SourceFile) + "." + propForm.txtSrcExt.Text.ToLower());
                    }
                }
                catch {
                    // ignore errors
                }
                // then update the extension
                EditGame.Logics.SourceFileExt = '.' + propForm.txtSrcExt.Text;
            }
            LogicCompiler.UseReservedNames = (propForm.chkUseReserved.Checked);
            EditGame.UseLE = (propForm.chkUseLE.Checked);
            // update menu/toolbar, and hide LE if not in use anymore
            UpdateLEStatus();
            // codepage
            SessionCodePage = propForm.NewCodePage;
            EditGame.CodePage = propForm.NewCodePage;
            // force update to WAG file
            WinAGISettingsList.Save();

            // update the reserved lookup values
            RDefLookup[90].Value = '\"' + EditGame.GameVersion + '\"';
            RDefLookup[91].Value = '\"' + EditGame.GameAbout + '\"';
            RDefLookup[92].Value = '\"' + EditGame.GameID + '\"';

            propForm.Dispose();
        }

        private void mnuGNewTemplate_Click(object sender, EventArgs e) {
            // create new game using template
            NewAGIGame(true);
        }

        private void mnuGNewBlank_Click(object sender, EventArgs e) {
            // create new blank game
            NewAGIGame(false);
        }

        private void mnuGCompile_Click(object sender, EventArgs e) {
            CompileAGIGame(EditGame.GameDir);
        }

        private void mnuGCompileTo_Click(object sender, EventArgs e) {
            CompileAGIGame();
        }

        private void mnuGCompileDirty_Click(object sender, EventArgs e) {
            CompileDirtyLogics();
        }

        private void mnuGRebuild_Click(object sender, EventArgs e) {
            CompileAGIGame(EditGame.GameDir, true);
        }

        private void mnuRAddRemove_Click(object sender, EventArgs e) {
            RemoveSelectedRes();
        }

        private void mnuRIDDesc_Click(object sender, EventArgs e) {
            SelectedItemDescription(1);
        }

        private void mnuResources_DropDownClosed(object sender, EventArgs e) {
            // move the menu items to appropriate context menu
            switch (WinAGISettings.ResListType) {
            case agiSettings.EResListType.TreeList:
                tvwResources.ContextMenuStrip.Items.AddRange([mnuRNew, mnuROpen, mnuRImport, mnuRSeparator1, mnuROpenRes, mnuRSave, mnuRExport, mnuRSeparator2, mnuRAddRemove, mnuRRenumber, mnuRIDDesc, mnuRSeparator3, mnuRCompileLogic, mnuRSavePicImage, mnuRExportGIF]);
                break;
            case agiSettings.EResListType.ComboList:
                lstResources.ContextMenuStrip.Items.AddRange([mnuRNew, mnuROpen, mnuRImport, mnuRSeparator1, mnuROpenRes, mnuRSave, mnuRExport, mnuRSeparator2, mnuRAddRemove, mnuRRenumber, mnuRIDDesc, mnuRSeparator3, mnuRCompileLogic, mnuRSavePicImage, mnuRExportGIF]);
                break;
            }
        }

        private void tvwResources_MouseDown(object sender, MouseEventArgs e) {
            // force selection to change BEFORE context menu is shown
            if (e.Button == MouseButtons.Right) {
                TreeNode node = tvwResources.GetNodeAt(e.X, e.Y);
                if (node != null) {
                    tvwResources.SelectedNode = node;
                }
            }
        }

        private void mnuGRun_Click(object sender, EventArgs e) {
            int rtn;
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
            case PlatformTypeEnum.DosBox:
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
            case PlatformTypeEnum.ScummVM:
                // scummvm parameters: --p gamedir --auto-detect
                strParams = "-p \"" + Path.GetDirectoryName(EditGame.GameDir) + "\" --auto-detect " + EditGame.PlatformOpts;
                break;
            case PlatformTypeEnum.NAGI:
                // no parameters for nagi; just run the program
                break;
            case PlatformTypeEnum.Other:
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
                    case PlatformTypeEnum.DosBox:
                        strErrTitle = "DosBox Error";
                        strErrMsg = "DosBox ";
                        break;
                    case PlatformTypeEnum.ScummVM:
                        strErrTitle = "ScummVM Error";
                        strErrMsg = "ScummVM ";
                        break;
                    case PlatformTypeEnum.NAGI:
                        strErrTitle = "NAGI Error";
                        strErrMsg = "NAGI ";
                        break;
                    case PlatformTypeEnum.Other:
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

        private void btnSaveResource_Click(object sender, EventArgs e) {

        }
    }
}
