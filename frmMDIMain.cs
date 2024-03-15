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
using static WinAGI.Editor.BkgdTasks;
using System.IO;

namespace WinAGI.Editor
{
    public partial class frmMDIMain : Form {
        //constants for control/window placement
        int CalcWidth, CalcHeight;
        const int MIN_HEIGHT = 361;
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
        bool KeepFocus;
        int FlashCount;

        //property box variables
        bool PropDblClick;
        int SelectedProp;
        bool EditPropDropdown;
        int PropGotFocus;
        int PropRows;
        int PropRowCount;
        int PropScroll;
        int ListItemHeight;
        bool NoPaint;
        bool AllowSelect;
        private bool splashDone;
        //tracks status of caps/num/ins
        static bool CapsLock = false;
        static bool NumLock = false;
        static bool InsertLock = false;

        public void OnIdle(object sender, EventArgs e) {
            // Update the panels when the program is idle.
            bool newCapsLock = Console.CapsLock;
            bool newNumLock = Console.NumberLock;
            bool newInsertLock = Control.IsKeyLocked(Keys.Insert);
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
        }
        public frmMDIMain() {
            InitializeComponent();

            //attach  AGI game events
            LoadGameStatus += GameEvents_LoadGameStatus;
            CompileGameStatus += GameEvents_CompileGameStatus;
            CompileLogicStatus += GameEvents_CompileLogicStatus;
            DecodeLogicStatus += GameEvents_DecodeLogicStatus;

            //initialize the WinAGI engine
            InitWinAGI();

            //use idle time to update caps/num/ins
            Application.Idle += new System.EventHandler(OnIdle);

            // save pointer to main form
            MDIMain = this;

            // set resource list and property controls to default location
            //tvwResources.Width = splResource.Panel1.Width;
            //tvwResources.Height = splResource.Panel1.Height - cmdBack.Height;
            cmbResType.Width = splResource.Panel1.Width;
            lstResources.Width = splResource.Panel1.Width;
            lstResources.Height = splResource.Panel1.Height - cmdBack.Height - cmbResType.Height;
            //picProperties.Width = splResource.Panel2.Width;
            //picProperties.Height = splResource.Panel2.Height;
            //fsbProperty.Height = splResource.Panel2.Height;
        }
        private void GameEvents_CompileGameStatus(object sender, CompileGameEventArgs e) {

        }
        private void GameEvents_LoadGameStatus(object sender, LoadGameEventArgs e) {
            switch (e.LoadInfo.Type) {
            case etInfo:
                switch (e.LoadInfo.InfoType) {
                case EInfoType.itInitialize:
                    break;
                case EInfoType.itValidating:
                    bgwOpenGame.ReportProgress(0, "Validating AGI game files ...");
                    break;
                case EInfoType.itPropertyFile:
                    if (e.LoadInfo.ResNum == 0) {
                        //ProgressWin.lblProgress.Text = "Creating game property file ...";
                        bgwOpenGame.ReportProgress(0, "Creating game property file ...");
                    }
                    else {
                        //ProgressWin.lblProgress.Text = "Loading game property file ...";
                        bgwOpenGame.ReportProgress(0, "Loading game property file ...");
                    }
                    break;
                case EInfoType.itResources:
                    switch (e.LoadInfo.ResType) {
                    case rtLogic:
                    case rtPicture:
                    case rtView:
                    case rtSound:
                        //ProgressWin.lblProgress.Text = "Validating Resources: " + ResTypeName[(int)e.ResType] + " " + e.ResNum;
                        bgwOpenGame.ReportProgress(0, "Validating Resources: " + ResTypeName[(int)e.LoadInfo.ResType] + " " + e.LoadInfo.ResNum);
                        break;
                    case rtWords:
                        //ProgressWin.lblProgress.Text = "Validating WORDS.TOK file";
                        bgwOpenGame.ReportProgress(0, "Validating WORDS.TOK file ...");
                        break;
                    case rtObjects:
                        //ProgressWin.lblProgress.Text = "Validating OBJECT file";
                        bgwOpenGame.ReportProgress(0, "Validating OBJECT file ...");
                        break;
                    }
                    break;
                case EInfoType.itDecompiling:
                    //ProgressWin.lblProgress.Text = "Validating AGI game files ...";
                    bgwOpenGame.ReportProgress(0, "Validating AGI game files ...");
                    break;
                case EInfoType.itCheckCRC:
                    break;
                case EInfoType.itFinalizing:
                    //ProgressWin.lblProgress.Text = "Configuring WinAGI";
                    bgwOpenGame.ReportProgress(0, "Configuring WinAGI");
                    break;
                }
                break;
            case etError:
                break;
            case etWarning:
                bgwOpenGame.ReportProgress(0, $"Load Warning: {e.LoadInfo.ID}: {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(1, e.LoadInfo);
                break;
                //MDIMain.AddWarning(e.LoadInfo);
                //break;
            case etTODO:
                bgwOpenGame.ReportProgress(0, $"{e.LoadInfo.ID} TODO: : {e.LoadInfo.Text}");
                // add to warning list
                bgwOpenGame.ReportProgress(2, e.LoadInfo);
                break;
                //MDIMain.AddWarning(e.LoadInfo);
                //break;
            }
        }
        private void GameEvents_CompileLogicStatus(object sender, CompileLogicEventArgs e) {

        }
        private void GameEvents_DecodeLogicStatus(object sender, DecodeLogicEventArgs e) {
            Debug.Print($"decode it: {e.DecodeInfo.Text}");
            bgwOpenGame?.ReportProgress(2, e.DecodeInfo);
            //MDIMain.AddWarning(e.DecodeInfo);
        }
        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {

        }
        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e) {

        }
        private void btnOpenGame_Click(object sender, EventArgs e) {
            //open a game!
            OpenDlg.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            OpenDlg.DefaultExt = "wag";
            OpenDlg.Filter = "WinAGI Game Files (*.wag)|*.wag|All files|*.*";
            DialogResult result = OpenDlg.ShowDialog(this);
            if (result == DialogResult.OK) {
                //let's open it
                //        this.UseWaitCursor = true;
                Refresh();
                OpenWAGFile(OpenDlg.FileName);
                //        this.UseWaitCursor = false;
            }
            else {
                return;
            }
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
            //only close if window is close-able
            this.ActiveMdiChild.Close();
        }
        private void mnuWindow_DropDownOpening(object sender, EventArgs e) {
            // disable the close item if no windows
            mnuWClose.Enabled = (this.MdiChildren.Length != 0);
        }
        private void frmMDIMain_Load(object sender, EventArgs e) {
            bool blnLastLoad;

            // STRATEGY FOR READING/WRITING DATA TO/FROM
            // AGI TEXT RESOURCES:
            //
            // TO READ:
            // read in as a byte array; then convert the
            // byte array to current codepage:
            //   strIn = agCodePage.GetString(bIn);
            //
            // TO WRITE:
            // convert string to byte array, then write
            // the byte array
            //   bOut = agCodePage.GetBytes(strOut);
            //
            // means can't easily convert to different codepage
            // on the fly; need to close and then restart the
            // game
            //int i;
            //byte[] charval = new byte[1];
            //string sTest = "";
            //for (i = 160; i < 176; i++) {
            //    charval[0] = (byte)i;
            //    sTest += Encoding.GetEncoding(850).GetString(charval);
            //}
            //byte tmp = (byte)sTest[0];
            //Debug.Print(sTest);

            //what is resolution?
            Debug.Print($"DeviceDPI: {this.DeviceDpi}");
            Debug.Print($"AutoScaleFactor: {this.AutoScaleFactor}");
            Debug.Print($"AutoscaleDimensions: {this.AutoScaleDimensions}");

            CalcWidth = MIN_WIDTH;
            CalcHeight = MIN_HEIGHT;

            // toolbar stuff;
            btnNewRes.DefaultItem = btnNewLogic;
            btnOpenRes.DefaultItem = btnOpenLogic;
            btnImportRes.DefaultItem = btnImportLogic;

            //set preview window, status bar and other dialog objects
            PreviewWin = new frmPreview
            {
                MdiParent = MDIMain
            };
            MainStatusBar = statusStrip1;
            //      ViewClipboard = picViewCB;
            SoundClipboard = [];
            //      NotePictures = picNotes;
            //        WordsClipboard = new WordsUndo();
            FindingForm = new frmFind();

            //hide rsource and warning panels until needed
            pnlResources.Visible = false;
            pnlWarnings.Visible = false;
            //set property window split location based on longest word
            Size szText = TextRenderer.MeasureText(" Use Res Names ", propertyGrid1.Font);
            PropSplitLoc = szText.Width;
            // set height based on text (assume padding of four? pixels above/below)
            PropRowHeight = szText.Height + 7;
            MAX_PROPGRID_HEIGHT = 11 * PropRowHeight;
            ////set initial position of property panel
            splResource.SplitterIncrement = PropRowHeight;
            splResource.Panel2MinSize = 3 * PropRowHeight;
            splResource.SplitterDistance = splResource.Height - splResource.Margin.Top - splResource.Margin.Bottom - splResource.SplitterWidth - 7 * PropRowHeight;
            // set grid row height
            fgWarnings.RowTemplate.Height = szText.Height +2;
            //background color for previewing views is set to default
            PrevWinBColor = SystemColors.Control;
            //set selected prop
            SelectedProp = 1;

            // initialize the basic app functionality
            InitializeResMan();

            ProgramDir = CDir(JustPath(Application.ExecutablePath));
            DefaultResDir = ProgramDir;
            //set browser start dir to program dir
            BrowserStartDir = ProgramDir;

            //get game settings and set initial window positions
            //this happens after all the stuff above because
            //Readsettings affects things that need to be initialized first
            if (!ReadSettings()) {
                //problem with settings
                MessageBox.Show("Fatal error: Unable to read program settings", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }
            frmSplash splash = null;
            //show splash screen, if applicable
            if (Settings.ShowSplashScreen) {
                Visible = true;
                Refresh();
                splash = new frmSplash();
                splash.Show(this);
                splash.Refresh();
            }
            //enable timer; it starts out as splash screen timer
            tmrNavList.Interval = 1750;
            tmrNavList.Enabled = true;

            LogicEditors = [];
            ViewEditors = [];
            PictureEditors = [];
            SoundEditors = [];

            //build the lookup table for reserved defines
            BuildRDefLookup();
            //default to an empty globals list
            GDefLookup = [];
            //if using snippets
            if (Settings.Snippets) {
                //build snippet table
                BuildSnippets();
            }
            //get reference to temporary directory
            //TempFileDir = GetTempFileDir()
            WinAGIHelp = ProgramDir + "WinAGI.chm";
            //      mnuHContents.Text = "Contents" + Keys.Tab + "F1";

            //initialize resource treelist by using clear method
            ClearResourceList();

            // set navlist parameters
            //set property window split location based on longest word
            szText = TextRenderer.MeasureText(" Logic 1 ", new Font(Settings.PFontName, Settings.PFontSize));
            NLRowHeight = szText.Height + 2;
            picNavList.Height = NLRowHeight * 5;
            picNavList.Top = (cmdBack.Top + cmdBack.Height / 2) - picNavList.Height / 2;
            //retrieve user's preferred AGI colors
            GetDefaultColors();

            //let the system catch up
            Refresh();

            //      //establish printer margins (don't need to show error msg if printer isn't available)
            //      UpdatePrinterCaps(Printer.Orientation, true);

            if (Settings.ShowSplashScreen) {
                //dont close unless ~1.75 seconds passed
                while (!splashDone) {
                    Application.DoEvents();
                }
                splash.Close();
            }

            //check for command string
            CheckCmd();

            //was a game loaded when app was last closed
            blnLastLoad = GameSettings.GetSetting(sMRULIST, "LastLoad", false);


            //if nothing loaded AND autoreload is set AND something was loaded last time program ended,
            if (EditGame is null && this.ActiveMdiChild is null && Settings.AutoOpen && blnLastLoad) {
                //open mru1
                OpenMRUGame(0);
            }
            //show the form
            //if no printers,
            if (NoPrinter && !Settings.SkipPrintWarning) {
                //        MsgBoxEx("There are no printers available, so printing functions will be disabled.", vbInformation + vbOKOnly, "WinAGI GDS", , , "Don//t show this warning again.", Settings.SkipPrintWarning
            }
            if (Settings.SkipPrintWarning) {
                GameSettings.WriteSetting(sGENERAL, "SkipPrintWarning", Settings.SkipPrintWarning);
            }
            //      UseWaitCursor = false;
        }
        private void btnNewLogic_Click(object sender, EventArgs e) {
            MessageBox.Show("new logic...");
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
            //let's test object list
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
            //fill list box with resources for selected type
            if (EditGame.GameLoaded) {
                AGIResType selRes;

                // clear current list
                lstResources.Items.Clear();

                selRes = (AGIResType)cmbResType.SelectedIndex;
                ListViewItem tmpItem;
                switch (cmbResType.SelectedIndex) {
                case 0: // game
                    selRes = rtGame;
                    break;
                case 1: //logics
                    foreach (Logic tmpRes in EditGame.Logics) {
                        tmpItem = lstResources.Items.Add("l" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                        tmpItem.Tag = tmpRes;
                        //set color based on compiled status;
                        if (tmpRes.Compiled) {
                            tmpItem.ForeColor = Color.Black;
                        }
                        else {
                            tmpItem.ForeColor = Color.Red;
                        }
                    }
                    selRes = rtLogic;
                    break;
                case 2://pictures
                    foreach (Picture tmpRes in EditGame.Pictures) {
                        tmpItem = lstResources.Items.Add("p" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                        tmpItem.Tag = tmpRes;
                    }
                    selRes = rtPicture;
                    break;
                case 3: //sounds
                    foreach (Sound tmpRes in EditGame.Sounds) {
                        tmpItem = lstResources.Items.Add("s" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                        tmpItem.Tag = tmpRes;
                    }
                    selRes = rtSound;
                    break;
                case 4: //views
                    foreach (Engine.View tmpRes in EditGame.Views) {
                        tmpItem = lstResources.Items.Add("v" + tmpRes.Number, ResourceName(tmpRes, true), 0);
                        tmpItem.Tag = tmpRes;
                    }
                    selRes = rtView;
                    break;
                case 5: //objects
                    selRes = rtObjects;
                    break;
                case 6: //words
                    selRes = rtWords;
                    break;
                }
                SelectResource(selRes, -1, true);
            }
        }
        private void lstResources_SelectedIndexChanged(object sender, EventArgs e) {
            if (EditGame.GameLoaded) {

                AGIResType NewType = rtGame; int NewNum = 0;
                switch (cmbResType.SelectedIndex) {
                case 0: //game
                        //root
                    NewType = rtGame;
                    NewNum = -1;
                    // currently no list items
                    break;
                case 1: //logics
                        //if nothing to select
                    if (lstResources.SelectedItems.Count == 0) {
                        //just exit
                        return;
                    }
                    NewType = rtLogic;
                    AGIResource tmp = (AGIResource)(lstResources.SelectedItems[0].Tag);
                    NewNum = tmp.Number;
                    //show id, number, description, compiled status, isroom
                    PropRows = 8;
                    //don't need to adjust context menu; preview window will do that
                    break;
                case 2: //pictures
                        //if nothing to select
                    if (lstResources.SelectedItems.Count == 0) {
                        //just exit
                        return;
                    }
                    NewType = rtPicture;
                    tmp = (AGIResource)(lstResources.SelectedItems[0].Tag);
                    NewNum = tmp.Number;
                    //show id, number, description
                    PropRows = 6;
                    //don't need to adjust context menu; preview window will do that
                    break;
                case 3: //sounds
                        //if nothing to select
                    if (lstResources.SelectedItems.Count == 0) {
                        //just exit
                        return;
                    }
                    NewType = rtSound;
                    tmp = (AGIResource)(lstResources.SelectedItems[0].Tag);
                    NewNum = tmp.Number;
                    //show id, number, description
                    PropRows = 6;
                    //don't need to adjust context menu; preview window will do that
                    break;
                case 4: //views
                        //if nothing to select
                    if (lstResources.SelectedItems.Count == 0) {
                        //just exit
                        return;
                    }
                    NewType = rtView;
                    tmp = (AGIResource)(lstResources.SelectedItems[0].Tag);
                    NewNum = tmp.Number;
                    //show id, number, description
                    PropRows = 7;
                    //don't need to adjust context menu; preview window will do that
                    break;
                case 5: //objects
                        //no listitems
                    NewType = rtObjects;
                    NewNum = -1;
                    break;

                case 6: //words
                        //no listitems
                    NewType = rtWords;
                    NewNum = -1;
                    break;
                }
                if (!DontQueue) {
                    SelectResource(NewType, NewNum);
                }
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
            //selects a resource for previewing
            //(always synched with the resource list)

            // always unload the current resource, if it's logic/pic/sound/view
            if (SelResNum != -1) {
                switch (SelResType) {
                case rtLogic:
                    EditGame.Logics[SelResNum].Unload();
                    break;
                case rtPicture:
                    EditGame.Pictures[SelResNum].Unload();
                    break;
                case rtSound:
                    EditGame.Sounds[SelResNum].Unload();
                    break;
                case rtView:
                    EditGame.Views[SelResNum].Unload();
                    break;
                }
            }
            //reset selprop
            SelectedProp = 0;
            propertyGrid1.SelectedObject = null;
            //get number of rows to display based on new selection
            switch (NewResType) {
            case rtNone:
                //nothing to show
                PropRows = 0;
                return;
            case rtGame:
                //show gameid, gameauthor, description,etc
                PropRows = 10;
                GameProperties pGame = new(EditGame);
                propertyGrid1.SelectedObject = pGame;
                break;
            case rtLogic:
                if (NewResNum == -1) {
                    //logic header
                    PropRows = 3;
                    LogicHdrProperties pLgcHdr = new(EditGame.Logics.Count, Compiler.UseReservedNames);
                    propertyGrid1.SelectedObject = pLgcHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Logics[NewResNum].Load();
                    //show logic properties
                    PropRows = 8;
                    //if compiled state doesn't match correct tree color, fix it now
                    LogicProperties pLog = new(EditGame.Logics[NewResNum]);
                    propertyGrid1.SelectedObject = pLog;
                }
                break;
            case rtPicture:
                if (NewResNum == -1) {
                    //picture header
                    PictureHdrProperties pPicHdr = new(EditGame.Pictures.Count);
                    propertyGrid1.SelectedObject = pPicHdr;
                    PropRows = 1;
                }
                else {
                    // always load before selecting
                    EditGame.Pictures[NewResNum].Load();
                    //show picture properties
                    PropRows = 6;
                    PictureProperties pPicture = new(EditGame.Pictures[NewResNum]);
                    propertyGrid1.SelectedObject = pPicture;
                }
                break;
            case rtSound:
                if (NewResNum == -1) {
                    //sound header
                    PropRows = 1;
                    SoundHdrProperties pSndHdr = new(EditGame.Sounds.Count);
                    propertyGrid1.SelectedObject = pSndHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Sounds[NewResNum].Load();
                    //show sound properties
                    PropRows = 6;
                    SoundProperties pSound = new(EditGame.Sounds[NewResNum]);
                    propertyGrid1.SelectedObject = pSound;
                }
                break;
            case rtView:
                if (NewResNum == -1) {
                    //view header
                    VieweHdrProperties pViewHdr = new(EditGame.Views.Count);
                    PropRows = 1;
                    propertyGrid1.SelectedObject = pViewHdr;
                }
                else {
                    // always load before selecting
                    EditGame.Sounds[NewResNum].Load();
                    //show view properties
                    PropRows = 7;
                    ViewProperties pView = new(EditGame.Views[NewResNum]);
                    propertyGrid1.SelectedObject = pView;
                }
                break;
            case rtObjects:
                // OBJECT file is always loaded
                //show object Count, description, encryption, and Max screen objects
                PropRows = 4;
                InvObjProperties pInvObj = new(EditGame.InvObjects);
                propertyGrid1.SelectedObject = pInvObj;
                break;
            case rtWords:
                // WORDS.TOK is always loaded
                //show group Count and word Count and description
                PropRows = 3;
                WordListProperties pWordList = new(EditGame.WordList);
                propertyGrid1.SelectedObject = pWordList;
                break;
            }

            //if previewing
            if (Settings.ShowPreview) {
                //if update is requested
                if (Settings.ShowPreview && UpdatePreview) {
                    // load the preview item
                    NoPaint = true;
                    PreviewWin.LoadPreview(NewResType, NewResNum);
                }
            }

            //update selection properties
            SelResType = NewResType;
            SelResNum = NewResNum;

            //if resource list is visible,
            if (Settings.ResListType >= 0) {
                //add selected resource to navigation queue
                //force update property window
                if (SelResNum < 0) {
                    //add headers and non-regular resources by type
                    AddToQueue(SelResType, 256);
                }
                else {
                    // add regular resourses by type/number
                    AddToQueue(SelResType, SelResNum);
                }
                //always disable forward button
                cmdForward.Enabled = false;
                //enable back button if at least two in queue
                cmdBack.Enabled = ResQPtr > 0;
                //if a logic is selected, and layout editor is active form
                if (SelResType == rtLogic) {
                    //if syncing the layout editor and the treeview list
                    if (Settings.LESync) {
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

            //create new view and enter edit mode
            NewView();

            if (!EditGame.GameLoaded) return;
            // show editor form
            frmViewEdit frmNew = new()
            {
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
                switch (Settings.ResListType) {
                case 1:
                    // treelist
                    if (ResType == rtGame) {
                        tvwResources.SelectedNode = RootNode;
                    }
                    else {
                        tvwResources.SelectedNode = HdrNode[(int)ResType];
                    }
                    // call the node click to finish selection
                    tvwResources_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(tvwResources.SelectedNode, MouseButtons.None, 0, 0, 0));
                    break;
                case 2:
                    // listbox
                    switch (ResType) {
                    case rtGame: //root
                        cmbResType.SelectedIndex = 0;
                        //then force selection change
                        SelectResource(rtGame, -1);
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
                case rtLogic:
                    if (EditGame.Logics.Exists((byte)ResNum)) {
                        strKey = "l" + ResNum;
                    }
                    break;
                case rtPicture:
                    if (EditGame.Pictures.Exists((byte)ResNum)) {
                        strKey = "p" + ResNum;
                    }
                    break;
                case rtSound:
                    if (EditGame.Sounds.Exists((byte)ResNum)) {
                        strKey = "s" + ResNum;
                    }
                    break;
                case rtView:
                    if (EditGame.Views.Exists((byte)ResNum)) {
                        strKey = "v" + ResNum;
                    }
                    break;
                }

                //if no key
                if (strKey.Length == 0) {
                    //this resource doesn't exist anymore - probably
                    //deleted; select the header
                    switch (Settings.ResListType) {
                    case 1:
                        // treelist
                        tvwResources.SelectedNode = HdrNode[(int)ResType];
                        tvwResources_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(tvwResources.SelectedNode, MouseButtons.None, 0, 0, 0));
                        break;
                    case 2:
                        //listbox
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
                switch (Settings.ResListType) {
                case 1:
                    tvwResources.SelectedNode = HdrNode[(int)ResType].Nodes[strKey];
                    break;
                case 2:
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
            string strCaption, strTool;
            bool blnErrors, blnMax;

            //open the program settings  file
            GameSettings = new SettingsList(ProgramDir + "winagi.config");
            GameSettings.Open();
            //don't have to worry about missing file; defaults will be added automatically
            //GENERAL settings
            Settings.ShowSplashScreen = GameSettings.GetSetting(sGENERAL, "ShowSplashScreen", DEFAULT_SHOWSPLASHSCREEN);
            Settings.SkipPrintWarning = GameSettings.GetSetting(sGENERAL, "SkipPrintWarning", DEFAULT_SKIPPRINTWARNING);
            Settings.WarnCompile = GameSettings.GetSetting(sGENERAL, "WarnCompile", DEFAULT_WARNCOMPILE);
            Settings.NotifyCompSuccess = GameSettings.GetSetting(sGENERAL, "NotifyCompSuccess", DEFAULT_NOTIFYCOMPSUCCESS);
            Settings.NotifyCompWarn = GameSettings.GetSetting(sGENERAL, "NotifyCompWarn", DEFAULT_NOTIFYCOMPWARN);
            Settings.NotifyCompFail = GameSettings.GetSetting(sGENERAL, "NotifyCompFail", DEFAULT_NOTIFYCOMPFAIL);
            Settings.WarnDupGName = GameSettings.GetSetting(sGENERAL, "WarnDupGName", DEFAULT_WARNDUPGNAME);
            Settings.WarnDupGVal = GameSettings.GetSetting(sGENERAL, "WarnDupGVal", DEFAULT_WARNDUPGVAL);
            Settings.WarnInvalidStrVal = GameSettings.GetSetting(sGENERAL, "WarnInvalidStrVal", DEFAULT_WARNSTRVAL);
            Settings.WarnInvalidCtlVal = GameSettings.GetSetting(sGENERAL, "WarnInvalidCtlVal", DEFAULT_WARNCTLVAL);
            Settings.WarnResOvrd = GameSettings.GetSetting(sGENERAL, "WarnResOvrd", DEFAULT_WARNRESOVRD);
            Settings.WarnDupObj = GameSettings.GetSetting(sGENERAL, "WarnDupObj", DEFAULT_WARNDUPOBJ);
            Settings.WarnItem0 = GameSettings.GetSetting(sGENERAL, "WarnItem0", DEFAULT_WARNITEM0);
            Settings.DelBlankG = GameSettings.GetSetting(sGENERAL, "DelBlankG", DEFAULT_DELBLANKG);
            Settings.ShowPreview = GameSettings.GetSetting(sGENERAL, "ShowPreview", DEFAULT_SHOWPREVIEW);
            Settings.ShiftPreview = GameSettings.GetSetting(sGENERAL, "ShiftPreview", DEFAULT_SHIFTPREVIEW);
            Settings.HidePreview = GameSettings.GetSetting(sGENERAL, "HidePreview", DEFAULT_HIDEPREVIEW);
            Settings.ResListType = GameSettings.GetSetting(sGENERAL, "ResListType", DEFAULT_RESLISTTYPE);
            //validate treetype
            if (Settings.ResListType < 0 || Settings.ResListType > 2) {
                //use default
                Settings.ResListType = DEFAULT_RESLISTTYPE;
                GameSettings.WriteSetting(sGENERAL, "ResListType", Settings.ResListType.ToString(), "");
            }
            Settings.AutoExport = GameSettings.GetSetting(sGENERAL, "AutoExport", DEFAULT_AUTOEXPORT);
            Settings.AutoUpdateDefines = GameSettings.GetSetting(sLOGICS, "AutoUpdateDefines", DEFAULT_AUTOUPDATEDEFINES);
            Settings.AutoUpdateResDefs = GameSettings.GetSetting(sLOGICS, "AutoUpdateResDefs", DEFAULT_AUTOUPDATERESDEFS);
            Settings.AskExport = GameSettings.GetSetting(sGENERAL, "AskExport", DEFAULT_ASKEXPORT);
            Settings.AskRemove = GameSettings.GetSetting(sGENERAL, "AskRemove", DEFAULT_ASKREMOVE);
            Settings.OpenNew = GameSettings.GetSetting(sGENERAL, "OpenNew", DEFAULT_OPENNEW);
            Settings.RenameDelRes = GameSettings.GetSetting(sGENERAL, "RenameDelRes", DEFAULT_RENAMEDELRES);
            Settings.AutoWarn = GameSettings.GetSetting(sLOGICS, "AutoWarn", DEFAULT_AUTOWARN);
            //(DefResDir is not an element of settings; it's a WinAGI property)
            DefResDir = GameSettings.GetSetting(sGENERAL, "DefResDir", "src").Trim();
            //validate directory
            if (DefResDir == "") {
                DefResDir = "src";
            }
            else if ((CTRL_CHARS + " \\/:*?\"<>|").Any(DefResDir.Contains)) {
                //invalid character; reset to default
                DefResDir = "src";
            }
            Settings.MaxSO = GameSettings.GetSetting(sGENERAL, "MaxSO", DEFAULT_MAXSO);
            if (Settings.MaxSO > 255)
                Settings.MaxSO = 255;
            if (Settings.MaxSO < 1)
                Settings.MaxSO = 1;
            //(maxvolsize is an AGIGame property, not a WinAGI setting)  ? but this is the default value if one is
            //not provided in a game, right?
            Settings.MaxVol0Size = GameSettings.GetSetting(sGENERAL, "MaxVol0", 1047552);
            if (Settings.MaxVol0Size < 32768)
                Settings.MaxVol0Size = 32768;
            if (Settings.MaxVol0Size > 1047552)
                Settings.MaxVol0Size = 1047552;
            DefMaxVol0Size = Settings.MaxVol0Size;
            //get help window parent
            if (!GameSettings.GetSetting(sGENERAL, "DockHelpWindow", true)) {
                //HelpParent = GetDesktopWindow();
                //if (HelpParent = 0)
                //HelpParent = this.hWnd;
            }
            //RESFORMAT settings
            Settings.ShowResNum = GameSettings.GetSetting("ResFormat", "ShowResNum", DEFAULT_SHOWRESNUM);
            Settings.IncludeResNum = GameSettings.GetSetting("ResFormat", "IncludeResNum", DEFAULT_INCLUDERESNUM);
            Settings.ResFormat.NameCase = GameSettings.GetSetting("ResFormat", "NameCase", (int)DEFAULT_NAMECASE);
            if ((int)Settings.ResFormat.NameCase < 0 || (int)Settings.ResFormat.NameCase > 2) {
                Settings.ResFormat.NameCase = DEFAULT_NAMECASE;
            }
            Settings.ResFormat.Separator = Left(GameSettings.GetSetting("ResFormat", "Separator", DEFAULT_SEPARATOR), 1);
            Settings.ResFormat.NumFormat = GameSettings.GetSetting("ResFormat", "NumFormat", DEFAULT_NUMFORMAT);
            //GLOBAL EDITOR
            Settings.GlobalUndo = GameSettings.GetSetting("Globals", "GlobalUndo", DEFAULT_GLBUNDO);
            Settings.GEShowComment = GameSettings.GetSetting("Globals", "ShowCommentColumn", DEFAULT_GESHOWCMT);
            Settings.GENameFrac = GameSettings.GetSetting("Globals", "GENameFrac", 0);
            Settings.GEValFrac = GameSettings.GetSetting("Globals", "GEValFrac", 0);

            //get overrides of reserved defines, if there are any
            GetResDefOverrides();

            //LAYOUT
            Settings.DefUseLE = GameSettings.GetSetting(sLAYOUT, "DefUseLE", DEFAULT_DEFUSELE);
            Settings.LEPages = GameSettings.GetSetting(sLAYOUT, "PageBoundaries", DEFAULT_LEPAGES);
            Settings.LEDelPicToo = GameSettings.GetSetting(sLAYOUT, "DelPicToo", DEFAULT_LEWARNDELETE);
            Settings.LEShowPics = GameSettings.GetSetting(sLAYOUT, "ShowPics", DEFAULT_LESHOWPICS);
            Settings.LESync = GameSettings.GetSetting(sLAYOUT, "Sync", DEFAULT_LESYNC);
            Settings.LEUseGrid = GameSettings.GetSetting(sLAYOUT, "UseGrid", DEFAULT_LEUSEGRID);
            Settings.LEGrid = GameSettings.GetSetting(sLAYOUT, "GridSize", DEFAULT_LEGRID);
            Settings.LEGrid = Math.Round(Settings.LEGrid, 2);
            if (Settings.LEGrid > 1)
                Settings.LEGrid = 1;
            if (Settings.LEGrid < 0.05)
                Settings.LEGrid = 0.05;
            Settings.LEZoom = GameSettings.GetSetting(sLAYOUT, "Zoom", DEFAULT_LEZOOM);
            //get editor colors
            Settings.LEColors.Room.Edge = GameSettings.GetSetting(sLAYOUT, "RoomEdgeColor", DEFAULT_LEROOM_EDGE);
            Settings.LEColors.Room.Fill = GameSettings.GetSetting(sLAYOUT, "RoomFillColor", DEFAULT_LEROOM_FILL);
            Settings.LEColors.TransPt.Edge = GameSettings.GetSetting(sLAYOUT, "TransEdgeColor", DEFAULT_LETRANSPT_EDGE);
            Settings.LEColors.TransPt.Fill = GameSettings.GetSetting(sLAYOUT, "TransFillColor", DEFAULT_LETRANSPT_FILL);
            Settings.LEColors.ErrPt.Edge = GameSettings.GetSetting(sLAYOUT, "ErrEdgeColor", DEFAULT_LEERR_EDGE);
            Settings.LEColors.ErrPt.Fill = GameSettings.GetSetting(sLAYOUT, "ErrFillColor", DEFAULT_LEERR_FILL);
            Settings.LEColors.Cmt.Edge = GameSettings.GetSetting(sLAYOUT, "CmtEdgeColor", DEFAULT_LECMT_EDGE);
            Settings.LEColors.Cmt.Fill = GameSettings.GetSetting(sLAYOUT, "CmtFillColor", DEFAULT_LECMT_FILL);
            Settings.LEColors.Edge = GameSettings.GetSetting(sLAYOUT, "ExitEdgeColor", DEFAULT_LEEXIT_EDGE);
            Settings.LEColors.Other = GameSettings.GetSetting(sLAYOUT, "ExitOtherColor", DEFAULT_LEEXIT_OTHERS);
            //LOGICS
            Settings.HighlightLogic = GameSettings.GetSetting(sLOGICS, "HighlightLogic", DEFAULT_HILITELOG);
            Settings.HighlightText = GameSettings.GetSetting(sLOGICS, "HighlightText", DEFAULT_HILITETEXT);
            Settings.LogicTabWidth = GameSettings.GetSetting(sLOGICS, "TabWidth", DEFAULT_LOGICTABWIDTH);
            if (Settings.LogicTabWidth < 1)
                Settings.LogicTabWidth = 1;
            if (Settings.LogicTabWidth > 32)
                Settings.LogicTabWidth = 32;
            Settings.MaximizeLogics = GameSettings.GetSetting(sLOGICS, "MaximizeLogics", DEFAULT_MAXIMIZELOGICS);
            Settings.AutoQuickInfo = GameSettings.GetSetting(sLOGICS, "AutoQuickInfo", DEFAULT_AUTOQUICKINFO);
            Settings.ShowDefTips = GameSettings.GetSetting(sLOGICS, "ShowDefTips", DEFAULT_SHOWDEFTIPS);
            Settings.UseTxt = GameSettings.GetSetting(sLOGICS, "UseTxt", DEFAULT_USETXT);
            Settings.EFontName = GameSettings.GetSetting(sLOGICS, "EditorFontName", DEFAULT_EFONTNAME);
            i = 0;
            foreach (FontFamily font in System.Drawing.FontFamily.Families) {
                if (font.Name.Equals(Settings.EFontName, StringComparison.OrdinalIgnoreCase)) {
                    //found
                    i = 1;
                    break;
                }
            }
            // not found?
            if (i == 1)
                Settings.EFontName = DEFAULT_EFONTNAME;
            Settings.EFontSize = GameSettings.GetSetting(sLOGICS, "EditorFontSize", DEFAULT_EFONTSIZE);
            if (Settings.EFontSize < 8)
                Settings.EFontSize = 8;
            if (Settings.EFontSize > 24)
                Settings.EFontSize = 24;
            Settings.PFontName = GameSettings.GetSetting(sLOGICS, "PreviewFontName", DEFAULT_PFONTNAME);
            i = 0;
            foreach (FontFamily font in System.Drawing.FontFamily.Families) {
                if (font.Name.Equals(Settings.PFontName, StringComparison.OrdinalIgnoreCase)) {
                    //found
                    i = 1;
                    break;
                }
            }
            if (i == 1)
                Settings.PFontName = DEFAULT_PFONTNAME;
            Settings.PFontSize = GameSettings.GetSetting(sLOGICS, "PreviewFontSize", DEFAULT_PFONTSIZE);
            if (Settings.PFontSize < 6)
                Settings.PFontSize = 6;
            if (Settings.PFontSize > 36)
                Settings.PFontSize = 36;
            Settings.OpenOnErr = GameSettings.GetSetting(sLOGICS, "OpenOnErr", DEFAULT_OPENONERR);
            if (Settings.OpenOnErr < 0)
                Settings.OpenOnErr = 0;
            if (Settings.OpenOnErr > 2)
                Settings.OpenOnErr = 2;
            Settings.SaveOnCompile = GameSettings.GetSetting(sLOGICS, "SaveOnComp", DEFAULT_SAVEONCOMP);
            if (Settings.SaveOnCompile < 0)
                Settings.SaveOnCompile = 0;
            if (Settings.SaveOnCompile > 2)
                Settings.SaveOnCompile = 2;
            Settings.CompileOnRun = GameSettings.GetSetting(sLOGICS, "CompOnRun", DEFAULT_COMPONRUN);
            if (Settings.CompileOnRun < 0)
                Settings.CompileOnRun = 0;
            if (Settings.CompileOnRun > 2)
                Settings.CompileOnRun = 2;
            Settings.LogicUndo = GameSettings.GetSetting(sLOGICS, "LogicUndo", DEFAULT_LOGICUNDO);
            if (Settings.LogicUndo < -1)
                Settings.LogicUndo = -1;
            Settings.WarnMsgs = GameSettings.GetSetting(sLOGICS, "WarnMsgs", DEFAULT_WARNMSGS);
            if (Settings.WarnMsgs < 0)
                Settings.WarnMsgs = 0;
            if (Settings.WarnMsgs > 2)
                Settings.WarnMsgs = 2;
            Compiler.ErrorLevel = (LogicErrorLevel)GameSettings.GetSetting(sLOGICS, "ErrorLevel", (int)DEFAULT_ERRORLEVEL);
            if (Compiler.ErrorLevel < 0)
                Compiler.ErrorLevel = LogicErrorLevel.leLow;
            if ((int)Compiler.ErrorLevel > 2)
                Compiler.ErrorLevel = LogicErrorLevel.leHigh;
            Settings.DefUseResDef = GameSettings.GetSetting(sLOGICS, "DefUseResDef", DEFAULT_DEFUSERESDEF);
            Settings.Snippets = GameSettings.GetSetting(sLOGICS, "Snippets", DEFAULT_SNIPPETS);
            //SYNTAXHIGHLIGHTFORMAT
            Settings.HColor[0] = GameSettings.GetSetting(sSHFORMAT, "NormalColor", DEFAULT_HNRMCOLOR);
            Settings.HColor[1] = GameSettings.GetSetting(sSHFORMAT, "KeywordColor", DEFAULT_HKEYCOLOR);
            Settings.HColor[2] = GameSettings.GetSetting(sSHFORMAT, "IdentifierColor", DEFAULT_HIDTCOLOR);
            Settings.HColor[3] = GameSettings.GetSetting(sSHFORMAT, "StringColor", DEFAULT_HSTRCOLOR);
            Settings.HColor[4] = GameSettings.GetSetting(sSHFORMAT, "CommentColor", DEFAULT_HCMTCOLOR);
            Settings.HColor[5] = GameSettings.GetSetting(sSHFORMAT, "BackColor", DEFAULT_HBKGCOLOR);
            Settings.HBold[0] = GameSettings.GetSetting(sSHFORMAT, "NormalBold", DEFAULT_HNRMBOLD);
            Settings.HBold[1] = GameSettings.GetSetting(sSHFORMAT, "KeywordBold", DEFAULT_HKEYBOLD);
            Settings.HBold[2] = GameSettings.GetSetting(sSHFORMAT, "IdentifierBold", DEFAULT_HIDTBOLD);
            Settings.HBold[3] = GameSettings.GetSetting(sSHFORMAT, "StringBold", DEFAULT_HSTRBOLD);
            Settings.HBold[4] = GameSettings.GetSetting(sSHFORMAT, "CommentBold", DEFAULT_HCMTBOLD);
            Settings.HItalic[0] = GameSettings.GetSetting(sSHFORMAT, "NormalItalic", DEFAULT_HNRMITALIC);
            Settings.HItalic[1] = GameSettings.GetSetting(sSHFORMAT, "KeywordItalic", DEFAULT_HKEYITALIC);
            Settings.HItalic[2] = GameSettings.GetSetting(sSHFORMAT, "IdentifierItalic", DEFAULT_HIDTITALIC);
            Settings.HItalic[3] = GameSettings.GetSetting(sSHFORMAT, "StringItalic", DEFAULT_HSTRITALIC);
            Settings.HItalic[4] = GameSettings.GetSetting(sSHFORMAT, "CommentItalic", DEFAULT_HCMTITALIC);

            //PICTURES
            Settings.PicScale.Edit = GameSettings.GetSetting(sPICTURES, "EditorScale", DEFAULT_PICSCALE_EDIT);
            if (Settings.PicScale.Edit < 1)
                Settings.PicScale.Edit = 1;
            if (Settings.PicScale.Edit > 4)
                Settings.PicScale.Edit = 4;
            Settings.PicScale.Preview = GameSettings.GetSetting(sPICTURES, "PreviewScale", DEFAULT_PICSCALE_PREVIEW);
            if (Settings.PicScale.Preview < 1)
                Settings.PicScale.Preview = 1;
            if (Settings.PicScale.Preview > 4)
                Settings.PicScale.Preview = 4;
            Settings.PicUndo = GameSettings.GetSetting(sPICTURES, "PicUndo", DEFAULT_PICUNDO);
            if (Settings.PicUndo < -1)
                Settings.PicUndo = -1;
            Settings.ShowBands = GameSettings.GetSetting(sPICTURES, "ShowBands", DEFAULT_SHOWBANDS);
            Settings.SplitWindow = GameSettings.GetSetting(sPICTURES, "SplitWindow", DEFAULT_SPLITWINDOW);
            Settings.CursorMode = (EPicCursorMode)GameSettings.GetSetting(sPICTURES, "CursorMode", DEFAULT_CURSORMODE);

            //PICTEST
            //these settings get loaded with each picedit form (and the logic template, which uses
            //the Horizion setting) that gets loaded; no need to
            //retrieve them here

            //SOUNDS
            Settings.ShowKybd = GameSettings.GetSetting(sSOUNDS, "ShowKeyboard", DEFAULT_SHOWKYBD);
            Settings.ShowNotes = GameSettings.GetSetting(sSOUNDS, "ShowNotes", DEFAULT_SHOWNOTES);
            Settings.OneTrack = GameSettings.GetSetting(sSOUNDS, "OneTrack", DEFAULT_ONETRACK);
            Settings.SndUndo = GameSettings.GetSetting(sSOUNDS, "SndUndo", DEFAULT_SNDUNDO);
            if (Settings.SndUndo < -1)
                Settings.SndUndo = -1;
            Settings.SndZoom = GameSettings.GetSetting(sSOUNDS, "Zoom", DEFAULT_SNDZOOM);
            if (Settings.SndZoom < 1)
                Settings.SndZoom = 1;
            if (Settings.SndZoom > 3)
                Settings.SndZoom = 3;
            Settings.NoMIDI = GameSettings.GetSetting(sSOUNDS, "NoMIDI", DEFAULT_NOMIDI);
            i = GameSettings.GetSetting(sSOUNDS, "Instrument0", DEFAULT_DEFINST);
            if (i > 255)
                i = 255;
            if (i < 0)
                i = 0;
            Settings.DefInst0 = (byte)i;
            i = GameSettings.GetSetting(sSOUNDS, "Instrument1", DEFAULT_DEFINST);
            if (i > 255)
                i = 255;
            if (i < 0)
                i = 0;
            Settings.DefInst1 = (byte)i;
            i = GameSettings.GetSetting(sSOUNDS, "Instrument2", DEFAULT_DEFINST);
            if (i > 255)
                i = 255;
            if (i < 0)
                i = 0;
            Settings.DefInst2 = (byte)i;
            Settings.DefMute0 = GameSettings.GetSetting(sSOUNDS, "Mute0", DEFAULT_DEFMUTE);
            Settings.DefMute1 = GameSettings.GetSetting(sSOUNDS, "Mute1", DEFAULT_DEFMUTE);
            Settings.DefMute2 = GameSettings.GetSetting(sSOUNDS, "Mute2", DEFAULT_DEFMUTE);
            Settings.DefMute3 = GameSettings.GetSetting(sSOUNDS, "Mute3", DEFAULT_DEFMUTE);

            //VIEWS
            Settings.ViewScale.Edit = GameSettings.GetSetting(sVIEWS, "EditorScale", DEFAULT_VIEWSCALE_EDIT);
            if (Settings.ViewScale.Edit < 1)
                Settings.ViewScale.Edit = 1;
            if (Settings.ViewScale.Edit > 10)
                Settings.ViewScale.Edit = 10;
            Settings.ViewScale.Preview = GameSettings.GetSetting(sVIEWS, "PreviewScale", DEFAULT_VIEWSCALE_PREVIEW);
            if (Settings.ViewScale.Preview < 1)
                Settings.ViewScale.Preview = 1;
            if (Settings.ViewScale.Preview > 10)
                Settings.ViewScale.Preview = 10;
            Settings.ViewAlignH = GameSettings.GetSetting(sVIEWS, "AlignH", DEFAULT_VIEWALIGNH);
            if (Settings.ViewAlignH < 0)
                Settings.ViewAlignH = 0;
            if (Settings.ViewAlignH > 2)
                Settings.ViewAlignH = 2;
            Settings.ViewAlignV = GameSettings.GetSetting(sVIEWS, "AlignV", DEFAULT_VIEWALIGNV);
            if (Settings.ViewAlignV < 0)
                Settings.ViewAlignV = 0;
            if (Settings.ViewAlignV > 2)
                Settings.ViewAlignV = 2;
            Settings.ViewUndo = GameSettings.GetSetting(sVIEWS, "ViewUndo", DEFAULT_VIEWUNDO);
            if (Settings.ViewUndo < -1)
                Settings.ViewUndo = -1;
            i = GameSettings.GetSetting(sVIEWS, "DefaultCelHeight", DEFAULT_DEFCELH);
            if (i < 1)
                i = 1;
            if (i > 167)
                i = 167;
            Settings.DefCelH = (byte)i;
            i = GameSettings.GetSetting(sVIEWS, "DefaultCelWidth", DEFAULT_DEFCELW);
            if (i < 1) i = 1;
            if (i > 167)
                i = 160;
            Settings.DefCelW = (byte)i;
            Settings.DefVColor1 = GameSettings.GetSetting(sVIEWS, "Color1", (int)DEFAULT_DEFVCOLOR1);
            if (Settings.DefVColor1 < 0)
                Settings.DefVColor1 = 0;
            if (Settings.DefVColor1 > 15)
                Settings.DefVColor1 = 15;
            Settings.DefVColor2 = GameSettings.GetSetting(sVIEWS, "Color2", (int)DEFAULT_DEFVCOLOR2);
            if (Settings.DefVColor2 < 0)
                Settings.DefVColor2 = 0;
            if (Settings.DefVColor2 > 15)
                Settings.DefVColor2 = 15;
            Settings.ShowVEPrev = GameSettings.GetSetting(sVIEWS, "ShowVEPreview", DEFAULT_SHOWVEPREV);
            Settings.ShowGrid = GameSettings.GetSetting(sVIEWS, "ShowEditGrid", DEFAULT_SHOWGRID);

            //DECOMPILER
            // TODO: decompiler settings are global; eventually I should add game properties that
            // allow per-game setting of these properties...
            Compiler.ShowAllMessages = GameSettings.GetSetting(sDECOMPILER, "ShowAllMessages", DEFAULT_SHOWALLMSGS);
            Compiler.MsgsByNumber = GameSettings.GetSetting(sDECOMPILER, "MsgsByNum", DEFAULT_MSGSBYNUM);
            Compiler.ElseAsGoto = GameSettings.GetSetting(sDECOMPILER, "ElseAsGoto", DEFAULT_ELSEASGOTO);
            Compiler.SpecialSyntax = GameSettings.GetSetting(sDECOMPILER, "SpecialSyntax", DEFAULT_SPECIALSYNTAX);
            Compiler.ReservedAsText = GameSettings.GetSetting(sDECOMPILER, "ReservedAsText", DEFAULT_SHOWRESVARS);
            Compiler.UseReservedNames = Settings.DefUseResDef;
            if (Settings.UseTxt) {
                Compiler.SourceExt = ".txt";
            }
            else {
                Compiler.SourceExt = ".lgc";
            }

            //get property window height
            PropRowCount = GameSettings.GetSetting(sPOSITION, "PropRowCount", 4);
            if (PropRowCount < 3)
                PropRowCount = MIN_SPLIT_RES;
            if (PropRowCount > 10)
                PropRowCount = MAX_SPLIT_RES;

            //get main window state
            blnMax = GameSettings.GetSetting(sPOSITION, "WindowMax", false);
            //get main window position
            sngLeft = GameSettings.GetSetting(sPOSITION, "Left", Screen.PrimaryScreen.Bounds.Width * 0.15);
            sngTop = GameSettings.GetSetting(sPOSITION, "Top", Screen.PrimaryScreen.Bounds.Height * 0.15);
            sngWidth = GameSettings.GetSetting(sPOSITION, "Width", Screen.PrimaryScreen.Bounds.Width * 0.7);
            sngHeight = GameSettings.GetSetting(sPOSITION, "Height", Screen.PrimaryScreen.Bounds.Height * 0.7);
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
            //if (sngWidth <= Screen.PrimaryScreen.Bounds.Width * 0.2) {
            //    sngWidth = Screen.PrimaryScreen.Bounds.Width * 0.2;
            //}
            //if (sngWidth > Screen.PrimaryScreen.Bounds.Width) {
            //    sngWidth = Screen.PrimaryScreen.Bounds.Width;
            //}
            //if (sngHeight <= Screen.PrimaryScreen.Bounds.Height * 0.2) {
            //    sngHeight = Screen.PrimaryScreen.Bounds.Height * 0.2;
            //}
            //else if (sngHeight > Screen.PrimaryScreen.Bounds.Height) {
            //    sngHeight = Screen.PrimaryScreen.Bounds.Height;
            //}
            //if (sngLeft < 0) {
            //    sngLeft = 0;
            //}
            //else if (sngLeft > Screen.PrimaryScreen.Bounds.Width * 0.85) {
            //    sngLeft = Screen.PrimaryScreen.Bounds.Width * 0.85;
            //}
            //if (sngTop < 0) {
            //    sngTop = 0;
            //}
            //else if (sngTop > Screen.PrimaryScreen.Bounds.Height * 0.85) {
            //    sngTop = Screen.PrimaryScreen.Bounds.Height * 0.85;
            //}
            //now move the form
            MDIMain.Bounds = new Rectangle((int)sngLeft, (int)sngTop, (int)sngWidth, (int)sngHeight);
            //if maximized
            if (blnMax) {
                //maximize window
                WindowState = FormWindowState.Maximized;
            }
            //get resource window width
            sngProp = GameSettings.GetSetting(sPOSITION, "ResourceWidth", MIN_SPLIT_V * 1.5);
            if (sngProp < MIN_SPLIT_V) {
                sngProp = MIN_SPLIT_V;
            }
            else if (sngProp > MDIMain.Bounds.Width - MIN_SPLIT_V) {
                sngProp = MDIMain.Bounds.Width - MIN_SPLIT_V;
            }
            //set width
            pnlResources.Width = (int)sngProp;

            //get mru settings
            Settings.AutoOpen = GameSettings.GetSetting(sMRULIST, "AutoOpen", DEFAULT_AUTOOPEN);
            for (i = 0; i < 4; i++) {
                strMRU[i] = GameSettings.GetSetting(sMRULIST, "MRUGame" + (i + 1), "");
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

            ////get tools info
            //blnTools = false;
            //for (i = 1; i <= 6; i++) {
            //  strCaption = GameSettings.GetSetting(sTOOLS, "Caption" + CStr(i), "");
            //  strTool = GameSettings.GetSetting(sTOOLS, "Source" + CStr(i), "");
            //  //update tools menu
            //  With MDIMain.mnuTCustom(i)
            //    if (strCaption.Length > 0 && strTool.Length > 0) {
            //      MDIMain.mnuTCustom[i].Visible = true;
            //      MDIMain.mnuTCustom(i).Caption = strCaption
            //      MDIMain.mnuTCustom(i).Tag = strTool
            //      blnTools = true;
            //    } else {
            //      Settings.Visible = false
            //    }
            //}
            ////if no tools, hide separator
            //MDIMain.mnuTBar1.Visible = blnTools

            //error warning settings
            lngNoCompVal = GameSettings.GetSetting(sLOGICS, "NoCompWarn0", 0);
            for (i = 1; i <= 30; i++) {
                Compiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << i)) == (1 << i));
            }
            lngNoCompVal = GameSettings.GetSetting(sLOGICS, "NoCompWarn1", 0);
            for (i = 31; i <= 60; i++) {
                Compiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << (i - 30))) == 1 << (i - 30));
            }
            lngNoCompVal = GameSettings.GetSetting(sLOGICS, "NoCompWarn2", 0);
            for (i = 61; i <= 90; i++) {
                Compiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << (i - 60))) == 1 << (i - 60));
            }
            lngNoCompVal = GameSettings.GetSetting(sLOGICS, "NoCompWarn3", 0);
            for (i = 91; i < Compiler.WARNCOUNT; i++) {
                Compiler.SetIgnoreWarning(5000 + i, (lngNoCompVal & (1 << (i - 90))) == 1 << (i - 90));
            }
            return true;
        }
        public void SaveSettings() {  //saves game settings to config file
            int i, lngCompVal;
            //if main form is maximized
            if (MDIMain.WindowState == FormWindowState.Maximized) {
                //save Max Value only
                GameSettings.WriteSetting(sPOSITION, "WindowMax", true);
            }
            else {
                //save all window settings
                GameSettings.WriteSetting(sPOSITION, "Top", MDIMain.Top.ToString());
                GameSettings.WriteSetting(sPOSITION, "Left", MDIMain.Left.ToString());
                GameSettings.WriteSetting(sPOSITION, "Width", MDIMain.Width.ToString());
                GameSettings.WriteSetting(sPOSITION, "Height", MDIMain.Height.ToString());
                GameSettings.WriteSetting(sPOSITION, "WindowMax", false.ToString());
            }
            //save other position settings
            GameSettings.WriteSetting(sPOSITION, "PropRowCount", PropRowCount);
            //save mru settings
            for (i = 0; i < 4; i++) {
                GameSettings.WriteSetting(sMRULIST, "MRUGame" + (i + 1), strMRU[i]);
            }
            //save resource pane width
            GameSettings.WriteSetting(sPOSITION, "ResourceWidth", pnlResources.Width);
            //save other general settings
            GameSettings.WriteSetting(sGENERAL, "DockHelpWindow", HelpParent == this.Handle);
            // for warnings, create a bitfield to mark which are being ignored
            lngCompVal = 0;
            for (i = 1; i <= 30; i++) {
                lngCompVal |= (Compiler.IgnoreWarning(5000 + i) ? 1 << i : 0);
            }
            GameSettings.WriteSetting(sLOGICS, "NoCompWarn0", lngCompVal);
            lngCompVal = 0;
            for (i = 1; i <= 30; i++) {
                lngCompVal |= (Compiler.IgnoreWarning(5030 + i) ? 1 << i : 0);
            }
            GameSettings.WriteSetting(sLOGICS, "NoCompWarn1", lngCompVal);
            lngCompVal = 0;
            for (i = 1; i <= 30; i++) {
                lngCompVal |= (Compiler.IgnoreWarning(5060 + i) ? 1 << i : 0);
            }
            GameSettings.WriteSetting(sLOGICS, "NoCompWarn2", lngCompVal);
            lngCompVal = 0;
            for (i = 1; i < (Compiler.WARNCOUNT % 30); i++) {
                lngCompVal |= (Compiler.IgnoreWarning(5090 + i) ? 1 << i : 0);
            }
            GameSettings.WriteSetting(sLOGICS, "NoCompWarn3", lngCompVal);
            //save to file
            GameSettings.Save();
        }
        private void tvwResources_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) {
            // select it first
            tvwResources.SelectedNode = e.Node;

            // show the preview for this node
            //if previewing
            if (Settings.ShowPreview) {
                if (ActiveMdiChild != PreviewWin && (Settings.ShiftPreview && ForcePreview)) {
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
                if (!PreviewWin.Visible && Settings.ShowPreview) {
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
            if (e.Node == RootNode) {
                //it's the game node
                SelectResource(rtGame, -1);
            }
            else if (e.Node.Parent == RootNode) {
                //it's a resource header
                SelectResource((AGIResType)e.Node.Index, -1);
            }
            else {
                //it's a resource
                SelectResource((AGIResType)e.Node.Parent.Index, (int)e.Node.Tag);
            }
            //after selection, force preview window to show and
            //move up, if those settings are active
            if (!PreviewWin.Visible && Settings.ShowPreview) {
                PreviewWin.Show();
                //set form focus to preview
                PreviewWin.Activate();
                //set control focus to tvwlist
                tvwResources.Focus();
            }
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
            //first hide the resource panel and associated elements
            pnlResources.Visible = false;
            splitResource.Visible = false;
            ////if the warnings list is still visible, adjust it so it fits correctly
            //if (picWarnings.Visible) {
            //  picWarnings.Left = picBottom.Left
            //  picWarnings.Width = picBottom.Width
            //  picSplitH.Left = picBottom.Left + 30
            //  picSplitH.Width = picBottom.Width - 60
            //}
        }
        private void picProperties_Paint(object sender, PaintEventArgs e) {
            //
            /*      int i;
                  byte bSelResNum = (byte)SelResNum;
                  // always expand the clipping region so the entire surface
                  // gets repainted
                  Graphics gProp = e.Graphics;
                  gProp.ResetClip();

                  //if no game loaded, or nothing selected
                  if ((!EditGame.GameLoaded)) {
                    //set proprows to zero
                    PropRows = 0;
                  }
                  //
                  PropRowCount = (int)(gProp.ClipBounds.Height / PropRowHeight) - 1;
                  if (PropRows > PropRowCount) {
                    //show scrollbar
                    picProperties.Width = splResource.Width - fsbProperty.Width;
                    // note that in .NET, the actual highest value attainable in a
                    // scrollbar is NOT the Maximum value; it's Maximum - LargeChange + 1!!
                    // that seems really dumb, but it's what happens... SO, 
                    //Max(propertysetting) = Max(desired) + LargeChange - 1
                    fsbProperty.Maximum = PropRows - PropRowCount + fsbProperty.LargeChange - 1;
                    fsbProperty.Visible = true;
                    //if current scroll position is too high
                    if (PropScroll > PropRows - PropRowCount) {
                      //reset propscroll
                      PropScroll = 0;
                      fsbProperty.Value = 0;
                    }
                  }
                  else {
                    //reset to top and hide scrollbar
                    PropScroll = 0;
                    fsbProperty.Value = 0;
                    fsbProperty.Visible = false;
                    picProperties.Width = splResource.Width;
                  }
                  // create brushes and pens for drawing the various elements
                  SolidBrush brushProp = new SolidBrush(PropGray);// Color.FromArgb(236, 233, 216));//  SystemColors.ButtonFace);
                  Pen penProp = new Pen(Color.Black, 1);
                  Font fontProp = new Font("MS Sans Serif", 8);

                  //draw property header cell
                  gProp.FillRectangle(brushProp, 1, 1, PropSplitLoc - 1, PropRowHeight - 2);
                  brushProp.Color = Color.Black;
                  gProp.DrawString("Property", fontProp, brushProp, 3, 1);
                  gProp.DrawLine(penProp, 1, PropRowHeight - 1, PropSplitLoc, PropRowHeight - 1);
                  //reset canselect flag
                  AllowSelect = false;
                  //if a property is selected //and picProperty has focus,
                  if (SelectedProp > 0) {
                    //if there is an active control
                    if (MDIMain.ActiveControl == picProperties) {
                      //allow selection
                      AllowSelect = true;
                    }
                  }
                  //if something selected
                  if (PropRows > 0) {
                    switch (SelResType) {
                    case rtGame: //  1  //root
                      DrawProp(gProp, "GameID", EditGame.GameID, 1, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded);
                      DrawProp(gProp, "Author", EditGame.GameAuthor, 2, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded);
                      DrawProp(gProp, "GameDir", EditGame.GameDir, 3, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfDialog);
                      DrawProp(gProp, "ResDir", EditGame.ResDirName, 4, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded);
                      DrawProp(gProp, "IntVer", EditGame.InterpreterVersion, 5, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDown);
                      DrawProp(gProp, "Description", EditGame.GameDescription, 6, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfOver);
                      DrawProp(gProp, "GameVer", EditGame.GameVersion, 7, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfOver);
                      DrawProp(gProp, "GameAbout", EditGame.GameAbout, 8, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfOver);
                      DrawProp(gProp, "LayoutEditor", EditGame.UseLE.ToString(), 9, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDown);
                      DrawProp(gProp, "LastEdit", EditGame.LastEdit.ToString("G"), 10, AllowSelect, SelectedProp, PropScroll, false);
                      break;
                    case rtLogic: // 2 //logic resource header
                      if (SelResNum == -1) {
                        DrawProp(gProp, "Count", EditGame.Logics.Count.ToString(), 1, AllowSelect, SelectedProp, PropScroll, false);
                        DrawProp(gProp, "GlobalDef", "(List)", 2, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "UseResNames", Compiler.UseReservedNames.ToString(), 3, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDown);
                      }
                      else {
                        DrawProp(gProp, "Number", EditGame.Logics[bSelResNum].Number.ToString(), 1, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "ID", EditGame.Logics[bSelResNum].ID, 2, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "Description", EditGame.Logics[bSelResNum].Description, 3, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        if (EditGame.Logics[bSelResNum].Number == 0) {
                          DrawProp(gProp, "IsRoom", EditGame.Logics[bSelResNum].IsRoom.ToString(), 4, AllowSelect, SelectedProp, PropScroll, false);
                        }
                        else {
                          DrawProp(gProp, "IsRoom", EditGame.Logics[bSelResNum].IsRoom.ToString(), 4, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDown);
                        }
                        DrawProp(gProp, "Compiled", EditGame.Logics[bSelResNum].Compiled.ToString(), 5, AllowSelect, SelectedProp, PropScroll, false);
                        //if compiled state doesn't match correct tree color, fix it now
                        if (Settings.ResListType == 1) {
                          tvwResources.SelectedNode.ForeColor = EditGame.Logics[bSelResNum].Compiled ? Color.Black : Color.Red;
                        }
                        else {
                          lstResources.SelectedItems[0].ForeColor = EditGame.Logics[bSelResNum].Compiled ? Color.Black : Color.Red;
                        }
                        DrawProp(gProp, "Volume", EditGame.Logics[bSelResNum].Volume >= 0 ? EditGame.Logics[bSelResNum].Volume.ToString() : "Error", 6, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                        DrawProp(gProp, "LOC", EditGame.Logics[bSelResNum].Loc >= 0 ? EditGame.Logics[bSelResNum].Loc.ToString() : "Error", 7, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                        DrawProp(gProp, "Size", EditGame.Logics[bSelResNum].Size > 0 ? EditGame.Logics[bSelResNum].Size.ToString() : "Error", 8, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                      }
                      break;
                    case rtPicture: //3 //picture resource header
                      if (SelResNum == -1) {
                        DrawProp(gProp, "Count", EditGame.Pictures.Count.ToString(), 1, AllowSelect, SelectedProp, PropScroll, false);
                      }
                      else {
                        DrawProp(gProp, "Number", EditGame.Pictures[bSelResNum].Number.ToString(), 1, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "ID", EditGame.Pictures[bSelResNum].ID, 2, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "Description", EditGame.Pictures[bSelResNum].Description, 3, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "Volume", EditGame.Pictures[bSelResNum].Volume >= 0 ? EditGame.Pictures[bSelResNum].Volume.ToString() : "Error", 4, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                        DrawProp(gProp, "LOC", EditGame.Pictures[bSelResNum].Loc >= 0 ? EditGame.Pictures[bSelResNum].Loc.ToString() : "Error", 5, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                        DrawProp(gProp, "Size", EditGame.Pictures[bSelResNum].Size > 0 ? EditGame.Pictures[bSelResNum].Size.ToString() : "Error", 6, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                      }
                      break;
                    case rtSound: //4 //sound resource header
                      if (SelResNum == -1) {
                        DrawProp(gProp, "Count", EditGame.Sounds.Count.ToString(), 1, AllowSelect, SelectedProp, PropScroll, false);
                      }
                      else {
                        DrawProp(gProp, "Number", EditGame.Sounds[bSelResNum].Number.ToString(), 1, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "ID", EditGame.Sounds[bSelResNum].ID, 2, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "Description", EditGame.Sounds[bSelResNum].Description, 3, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "Volume", EditGame.Sounds[bSelResNum].Volume >= 0 ? EditGame.Sounds[bSelResNum].Volume.ToString() : "Error", 4, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                        DrawProp(gProp, "LOC", EditGame.Sounds[bSelResNum].Loc >= 0 ? EditGame.Sounds[bSelResNum].Loc.ToString() : "Error", 5, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                        DrawProp(gProp, "Size", EditGame.Sounds[bSelResNum].Size > 0 ? EditGame.Sounds[bSelResNum].Size.ToString() : "Error", 6, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                      }
                      break;
                    case rtView: //5 //view resource header
                      if (SelResNum == -1) {
                        DrawProp(gProp, "Count", EditGame.Views.Count.ToString(), 1, AllowSelect, SelectedProp, PropScroll, false);
                      }
                      else {
                        DrawProp(gProp, "Number", EditGame.Views[bSelResNum].Number.ToString(), 1, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "ID", EditGame.Views[bSelResNum].ID, 2, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        DrawProp(gProp, "Description", EditGame.Views[bSelResNum].Description, 3, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDialog);
                        //viewdescription only accessible if view is loaded
                        bool bLoaded = EditGame.Views[bSelResNum].Loaded;
                        if (!bLoaded) {
                          EditGame.Views[bSelResNum].Load();
                        }
                        DrawProp(gProp, "View Desc", EditGame.Views[bSelResNum].ViewDescription, 4, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfOver);
                        if (!bLoaded) {
                          EditGame.Views[bSelResNum].Unload();
                        }
                        DrawProp(gProp, "Volume", EditGame.Views[bSelResNum].Volume >= 0 ? EditGame.Views[bSelResNum].Volume.ToString() : "Error", 5, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                        DrawProp(gProp, "LOC", EditGame.Views[bSelResNum].Loc >= 0 ? EditGame.Views[bSelResNum].Loc.ToString() : "Error", 6, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                        DrawProp(gProp, "Size", EditGame.Views[bSelResNum].Size > 0 ? EditGame.Views[bSelResNum].Size.ToString() : "Error", 7, AllowSelect, SelectedProp, PropScroll, false, EButtonFace.bfNone);
                      }
                      break;
                    case rtObjects: // 6  //objects
                      DrawProp(gProp, "Obj Count", (EditGame.InvObjects.Count - 1).ToString(), 1, AllowSelect, SelectedProp, PropScroll, false);
                      DrawProp(gProp, "Description", EditGame.InvObjects.Description, 2, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfOver);
                      DrawProp(gProp, "Encrypted", EditGame.InvObjects.Encrypted.ToString(), 3, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfDown);
                      DrawProp(gProp, "Max Obj", EditGame.InvObjects.MaxScreenObjects.ToString(), 4, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded);
                      break;
                    case rtWords: //7  //words
                      DrawProp(gProp, "Group Count", EditGame.WordList.GroupCount.ToString(), 1, AllowSelect, SelectedProp, PropScroll, false);
                      DrawProp(gProp, "Word Count", EditGame.WordList.WordCount.ToString(), 2, AllowSelect, SelectedProp, PropScroll, false);
                      DrawProp(gProp, "Description", EditGame.WordList.Description, 3, AllowSelect, SelectedProp, PropScroll, EditGame.GameLoaded, EButtonFace.bfOver);
                      break;
                    }
                  }
                  //if property edit box or list box is visible,
                  if (lstProperty.Visible) {
                    int lngPosY;
                    switch (lstProperty.Tag) {
                    case "INTVER":
                      lngPosY = (4 - PropScroll) * PropRowHeight;
                      break;
                    case "OBJENCRYPT":
                      lngPosY = (3 - PropScroll) * PropRowHeight;
                      break;
                    case "ISROOM":
                      lngPosY = (6 - PropScroll) * PropRowHeight;
                      break;
                    case "USERESNAMES":
                      lngPosY = (3 - PropScroll) * PropRowHeight;
                      break;
                    }
                    ////move it to correct position
                    //if (lngPosY < picProperties.Height - lstProperty.Height) {
                    //  lstProperty.Location = new Point(picProperties.Left + PropSplitLoc, picProperties.Top + lngPosY);
                    //}
                    //else {
                    //  lstProperty.Location = new Point(picProperties.Left + PropSplitLoc, picProperties.Top + picProperties.Height - lstProperty.Height);
                    //}
                  }
                  // draw the grid lines
                  gProp.DrawLine(penProp, PropSplitLoc, 0, PropSplitLoc, PropRowHeight - 1);
                  //draw Value header cell
                  brushProp.Color = PropGray;
                  gProp.FillRectangle(brushProp, PropSplitLoc + 2, 1, picProperties.Width - 2, PropRowHeight - 2);
                  brushProp.Color = Color.Black;
                  gProp.DrawString("Value", fontProp, brushProp, PropSplitLoc + 4, 1);
                  gProp.DrawLine(penProp, PropSplitLoc + 2, PropRowHeight - 1, picProperties.Width - 1, PropRowHeight - 1);
                  gProp.DrawLine(penProp, picProperties.Width - 1, 0, picProperties.Width - 1, PropRowHeight - 1);
                  //draw vertical line separating columns
                  penProp.Color = LtGray;
                  gProp.DrawLine(penProp, PropSplitLoc, PropRowHeight, PropSplitLoc, picProperties.Height - 1);
                  gProp.DrawLine(penProp, picProperties.Width - 1, PropRowHeight, picProperties.Width - 1, picProperties.Height - 1);
                  //draw horizontal lines separating rows
                  for (i = 2; i <= PropRowCount + 1; i++) {
                    gProp.DrawLine(penProp, 0, i * PropRowHeight - 1, picProperties.Width - 1, i * PropRowHeight - 1);
                  }
            */
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
            EditGame.Logics[SelResNum].SaveSource();
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

            //if not changed from previous number
            if (LastNodeName != e.Node.Name) {
                // force it to select by using node-click
                tvwResources_NodeMouseClick(sender, new TreeNodeMouseClickEventArgs(e.Node, MouseButtons.None, 0, 0, 0));
            }
        }
        private void frmMDIMain_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            Debug.Print($"Main - PreviewKeyDown: {e.KeyCode}; KeyData: {e.KeyData}; KeyModifiers: {e.Modifiers}");
        }
        private void frmMDIMain_KeyDown(object sender, KeyEventArgs e) {
            //      Debug.Print($"Main - KeyDown: {e.KeyCode}; KeyData: {e.KeyData}; KeyModifiers: {e.Modifiers}");
            if (this.splResource.ActiveControl == this.propertyGrid1 && this.ActiveControl == this.splResource) {
                //? cancel it, unless in editing mode
                // e.SuppressKeyPress = true;
            }
        }
        private void propertyGrid1_KeyDown(object sender, KeyEventArgs e) {
            Debug.Print($"propgrid - KeyDown: {e.KeyCode}; KeyData: {e.KeyData}; KeyModifiers: {e.Modifiers}");
            //? cancel it, unless in editing mode
            e.SuppressKeyPress = true;
        }


        public void RemoveSelectedRes() {

            /*
        //removes a resource from the game

        Dim rtn As VbMsgBoxResult
        Dim blnDontAsk As Boolean
        Dim strID As String

        switch (SelResType
        case rtLogic
          strID = Logics(SelResNum).ID

        case rtPicture
          strID = Pictures(SelResNum).ID

        case rtSound
          strID = Sounds(SelResNum).ID

        case rtView
          strID = Views(SelResNum).ID

        default:
          //Debug.Assert false
          return;
        }

        //ask if resource should be exported first
        if (Settings.AskExport) {
          rtn = MsgBoxEx("Do you want to export //" + strID + "// before" + vbNewLine + "removing it from your game?", _
                              vbQuestion + vbYesNoCancel, "Export " + ResTypeName(SelResType) + " Before Removal", , , _
                              "Don//t ask this question again", blnDontAsk)

          //save the setting
          Settings.AskExport = !blnDontAsk
          //if now hiding, update settings file
          if (!Settings.AskExport) {
            GameSettings.WriteSetting(sGENERAL, "AskExport", Settings.AskExport
          }
        } else {
          //dont ask; assume no
          rtn = vbNo
        }

        //if canceled,
        switch (rtn
        case vbCancel
          return;
        case vbYes
          //export it
          SelectedItemExport
        case vbNo
          //nothing to do
        }

        //confirm removal
        if (Settings.AskRemove) {
          rtn = MsgBoxEx("Removing //" + strID + "// from your game." + vbCrLf + vbCrLf + "Select OK to proceed, or Cancel to keep it in game.", _
                          vbQuestion + vbOKCancel, "Remove " + ResTypeName(SelResType) + " From Game", , , _
                          "Don//t ask this question again", blnDontAsk)

          //save the setting
          Settings.AskRemove = !blnDontAsk
          //if now hiding, update settings file
          if (!Settings.AskRemove) {
            GameSettings.WriteSetting(sGENERAL, "AskRemove", Settings.AskRemove
          }
        } else {
          //assume OK
          rtn = vbOK
        }

        //if canceled,
        if (rtn = vbCancel) {
          return;
        }

        //now remove the resource
        switch (SelResType
        case rtView
          //remove the view
          RemoveView SelResNum

        case rtLogic
          //remove the logic
          RemoveLogic SelResNum

        case rtPicture
          //remove the picture
          RemovePicture SelResNum

        case rtSound
          //remove the sound
          RemoveSound SelResNum
        */
        }
        public void SearchForID(FindFormFunction ffValue = FindFormFunction.ffFindLogic) {
            //set search form defaults
            switch (SelResType) {
            case rtLogic:
                GFindText = EditGame.Logics[(byte)SelResNum].ID;
                break;
            case rtPicture:
                GFindText = EditGame.Pictures[(byte)SelResNum].ID;
                break;
            case rtSound:
                GFindText = EditGame.Sounds[(byte)SelResNum].ID;
                break;
            case rtView:
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
            switch (warnInfo.Type) {
            case etInfo:
                WarningList.Add(warnInfo);
                break;
            case etError:
                break;
            case etWarning:
                WarningList.Add(warnInfo);
                break;
            case etTODO:
                WarningList.Add(warnInfo);
                break;
            }
            // if grid is visible, add the last item
            if (MDIMain.pnlWarnings.Visible) {
                AddWarningToGrid(warnInfo);
            }
            // if not visible, show if autowarn is true
            else if (Settings.AutoWarn) {
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
            int tmpRow = fgWarnings.Rows.Add(warnInfo.ID,
                         warnInfo.Text,
                         (int)warnInfo.ResType < 4 ? warnInfo.ResNum.ToString() : "--",
                         warnInfo.ResType == rtLogic ? warnInfo.Line.ToString() : "--",
                        // To avoid runtime errors during sort, all items in a column must be same 
                        // object type, that's why resnum and line must be strings
                         warnInfo.Module.Length > 0 ? Path.GetFileName(warnInfo.Module) : "--");
            //save restype in row data tag
            fgWarnings.Rows[tmpRow].Tag = rtLogic.ToString();
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
            case etWarning:
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
            switch (Settings.ResListType) {
            case 1:
                tvwResources.Focus();
                break;
            case 2:
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
            switch (Settings.ResListType) {
            case 1:
                tvwResources.Focus();
                break;
            case 2:
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
            Font nlFont = new(Settings.PFontName, Settings.PFontSize);
            //draw list of resources on stack, according to current
            // offset; whatever is selected is also highlighted

            //start with a clean slate
            e.Graphics.Clear(picNavList.BackColor);

            PointF nlPoint = new();
            //print five lines
            for (i = 0; i < 5; i++) {
                if (i + NLOffset - 2 >= 0 && i + NLOffset - 2 <= ResQueue.Length - 1) {
                    //if this row is highlighted (under cursor and valid)
                    if (i == NLRow) {
                        e.Graphics.FillRectangle(hbrush, 0, (int)((i + 0.035) * NLRowHeight), picNavList.Width, NLRowHeight);
                    }
                    ////set x and y positions for the printed id
                    nlPoint.X = 1;
                    nlPoint.Y = NLRowHeight * i;

                    //print the id
                    AGIResType restype = (AGIResType)(ResQueue[i + NLOffset - 2] >> 16);
                    int resnum = ResQueue[i + NLOffset - 2] & 0xFFFF;
                    switch (restype) {
                    case rtGame:
                        e.Graphics.DrawString(EditGame.GameID, nlFont, bbrush, nlPoint);
                        break;
                    case rtLogic:
                        if (resnum == 256) {
                            e.Graphics.DrawString("LOGICS", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Logics[resnum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case rtPicture:
                        if (resnum == 256) {
                            e.Graphics.DrawString("PICTURES", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Pictures[resnum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case rtSound:
                        if (resnum == 256) {
                            e.Graphics.DrawString("SOUNDS", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Sounds[resnum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case rtView:
                        if (resnum == 256) {
                            e.Graphics.DrawString("VIEWS", nlFont, bbrush, nlPoint);
                        }
                        else {
                            e.Graphics.DrawString(EditGame.Views[resnum].ID, nlFont, bbrush, nlPoint);
                        }
                        break;
                    case rtObjects:
                        e.Graphics.DrawString("OBJECTS", nlFont, bbrush, nlPoint);
                        break;
                    case rtWords:
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
            //import a game by directory
            OpenDIR();
        }
        private void mnuGOpen_Click(object sender, EventArgs e) {
            //open a game - user will get chance to select wag file in OpenWAGFile()
            OpenWAGFile();
        }
        public void ClearWarnings() {
            // clear entire list
            WarningList.Clear();
            // then clear the grid
            fgWarnings.Rows.Clear();
        }
        public void ClearWarnings(byte ResNum, AGIResType ResType) {
            //find the matching lines (by type/number)
            foreach (TWinAGIEventInfo item in WarningList) {
                if (item.ResNum == ResNum && item.ResType == ResType) {
                    WarningList.Remove(item);
                }
            };
            for (int i = fgWarnings.Rows.Count - 1; i >= 0; i--) {
                if ((byte)fgWarnings.Rows[i].Cells[4].Value == ResNum && (AGIResType)fgWarnings.Rows[i].Tag == ResType) {
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

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
            // what to do if changed?
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

            //show warning help
            _ = API.HtmlHelpS(HelpParent, WinAGIHelp, API.HH_DISPLAY_TOPIC, strTopic);
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




      void SelectPropFromList()

        Dim i As Long
        Dim Reason As EUReason

        On Error GoTo ErrHandler

        //verify property pic box is visible
        if (!picProperties.Visible) {
          return;
        }

        //restore previously selected prop
        switch (lstProperty.Tag
        case "INTVER"
          SelectedProp = 4
        case "OBJENCRYPT"
          SelectedProp = 3
        case "ISROOM"
          SelectedProp = 6
        case "USERESNAMES"
          SelectedProp = 4
        case "USELE"
          SelectedProp = 9
        }

        //save property that was edited
        switch (lstProperty.Tag
        case "INTVER"
          //determine if a change was made:
          //if there is a change to make
          if (InterpreterVersion != lstProperty.Text) {
            //make change to version
            ChangeIntVersion lstProperty.Text
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

        case "USERESNAMES"
          //determine if a change was made
          if (CBool(lstProperty.SelectedIndex) != LogicSourceSettings.UseReservedNames) {
            LogicSourceSettings.UseReservedNames = CBool(lstProperty.SelectedIndex)
          }

        case "USELE"
          //determine if a change was made
          if (CBool(lstProperty.SelectedIndex) != UseLE) {
            UseLE = CBool(lstProperty.SelectedIndex)
            // update menu, toolbar and close LE if necessary
            UpdateLEStatus
          }

        case "OBJENCRYPT"
          //determine if a change was made
          if (CBool(lstProperty.SelectedIndex) != InvObjects.Encrypted) {
            InvObjects.Encrypted = CBool(lstProperty.SelectedIndex)
            InvObjects.Save
          }

        case "ISROOM"
          //determine if a change was made
          if (CBool(lstProperty.SelectedIndex) != Logics(SelResNum).IsRoom) {
            WaitCursor

            Logics(SelResNum).IsRoom = CBool(lstProperty.SelectedIndex)
            Logics(SelResNum).Save
            if (UseLE) {
              if (Logics(SelResNum).IsRoom) {
                Reason = euShowRoom
              } else {
                Reason = euRemoveRoom
              }
              //update layout editor to show new room status
              UpdateExitInfo Reason, SelResNum, Logics(SelResNum)
            }

            //update any open editor
            For i = 1 To LogicEditors.Count
              if (LogicEditors(i).LogicNumber = SelResNum) {
                LogicEditors(i).LogicEdit.IsRoom = Logics(SelResNum).IsRoom
                Exit For
              }
            Next i

            Screen.MousePointer = vbDefault
          }
        }

        //hide listbox
        lstProperty.Visible = false

        //set focus to property window
        picProperties.Focus()

        //force repaint
        PaintPropertyWindow
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void SelectPropFromText()

        On Error GoTo ErrHandler

        //restore selected property
        switch (txtProperty.Tag
        case "GAMEID"
          SelectedProp = 1
        case "GAMEAUTHOR"
          SelectedProp = 2
        case "RESDIR"
          SelectedProp = 4
        case "GAMEDESC"
          SelectedProp = 6
        case "GAMEVER"
          SelectedProp = 7
        case "GAMEABOUT"
          SelectedProp = 8
        case "WORDSDESC"
          SelectedProp = 3
        case "OBJDESC"
          SelectedProp = 2
        case "VIEWDESC"
          SelectedProp = 4
        case "MAXOBJ"
          SelectedProp = 4
        }

        //save property that was edited
        switch (txtProperty.Tag
        case "GAMEID"
          //new game id

          //if new id is valid (not zero-length) AND changed
          if (LenB(txtProperty.Text) != 0 && GameID != txtProperty.Text) {
            ChangeGameID txtProperty.Text
          }

        case "RESDIR"
          //if changed
          if (LCase(txtProperty.Text) != LCase(ResDirName)) {
            ChangeResDir txtProperty.Text
          }

        case "GAMEAUTHOR"
          GameAuthor = txtProperty.Text

        case "GAMEDESC"
          GameDescription = txtProperty.Text

        case "GAMEVER"
          GameVersion = txtProperty.Text

        case "GAMEABOUT"
          GameAbout = txtProperty.Text

        case "WORDSDESC"
          WordList.Description = txtProperty.Text
          WordList.Save

        case "OBJDESC"
          InvObjects.Description = txtProperty.Text
          InvObjects.Save

        case "VIEWDESC"
          //viewdescription only available if view is loaded
          With Views(SelResNum)
            if (!.Loaded) {
              .Load
              .ViewDescription = Replace(txtProperty.Text, vbNewLine, vbLf)
              .Save
              .Unload
            } else {
              .ViewDescription = Replace(txtProperty.Text, vbNewLine, vbLf)
              .Save
            }
          End With
          //check for an editor with this number
          Dim tmpFrm As Form
          foreach (tmpFrm In Forms
            if (tmpFrm.Name = "frmViewEdit") {
              if (tmpFrm.ViewNumber = SelResNum && tmpFrm.InGame) {
                tmpFrm.ViewEdit.ViewDescription = Views(SelResNum).ViewDescription
                tmpFrm.UpdateViewDesc
              }
            }
          Next

        case "MAXOBJ"
          //validate Max val
          if (Val(txtProperty) > 255) {
            txtProperty.Text = 255
          } else if ( Val(txtProperty) <= 0) {
            txtProperty.Text = 1
          }
          InvObjects.MaxScreenObjects = (byte)txtProperty.Text)
          InvObjects.Save
        }

        //hide textbox
        txtProperty.Visible = false

        //set focus to property window
        picProperties.Focus()

        //force repaint
        PaintPropertyWindow
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      public void WLMouseWheel(ByVal MouseKeys As Long, ByVal Rotation As Long, ByVal xPos As Long, ByVal yPos As Long)
        Dim NewValue As Long
        Dim Lstep As Single

        On Error Resume Next

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

        case rtLogic
          //resource header has no action
          if (ResNum >= 0) {
            OpenLogic ResNum
          }

        case rtPicture
           //resource header has no action
          if (ResNum >= 0) {
            OpenPicture ResNum
          }

        case rtSound
          //resource header has no action
          if (ResNum >= 0) {
            OpenSound ResNum
          }

        case rtView
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

      public void PrintResources()
      {
        Dim i As Long

        On Error GoTo ErrHandler

        //check for an open editor that matches resource being previewed
        switch (SelResType
        case rtLogic
          //if any logic editor matches this resource
          For i = 1 To LogicEditors.Count
            if (LogicEditors(i).FormMode = fmLogic) {
              if (LogicEditors(i).LogicNumber = SelResNum) {
                //use this form//s method
                LogicEditors(i).MenuClickPrint
                return;
              }
            }
          Next i

        case rtPicture
          //if any Picture editor matches this resource
          For i = 1 To PictureEditors.Count
            if (PictureEditors(i).PicNumber = SelResNum) {
              //use this form//s method
              PictureEditors(i).MenuClickPrint
              return;
            }
          Next i

        case rtSound
          //if any Sound editor matches this resource
          For i = 1 To SoundEditors.Count
            if (SoundEditors(i).SoundNumber = SelResNum) {
              //use this form//s method
              SoundEditors(i).MenuClickPrint
              return;
            }
          Next i

        case rtView
          //if any View editor matches this resource
          For i = 1 To ViewEditors.Count
            if (ViewEditors(i).ViewNumber = SelResNum) {
              //use this form//s method
              ViewEditors(i).MenuClickPrint
              return;
            }
          Next i

        case rtWords
          //if editing words
          if (WEInUse) {
            //use word editor
            WordEditor.MenuClickPrint
            return;
          }

        case rtObjects
          //if editing objects,
          if (OEInUse) {
            //use Objects Editor
            ObjectEditor.MenuClickPrint
            return;
          }

        default: //text, game or none
          //print does not apply

        }

        //if no open editor is found, use the selected item method
        SelectedItemPrint
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      public void RDescription()
        //tie function
        mnuRDescription_Click
      }

      void SelectedItemDescription(ByVal FirstProp As Long)
      {
        Dim strID As String, strTempD As String
        Dim frm As Form

        //only use for resources that are NOT being edited;
        //if the resource is being edited, the editor for that
        //resource handles description, ID and number changes

        On Error GoTo ErrHandler

        //is there an open editor for this resource?
        //if so, use that editor//s renumber function
        switch (SelResType
        case rtLogic
          //step through logiceditors
          foreach (frm In LogicEditors
            if (frm.LogicNumber = SelResNum) {
              frm.MenuClickDescription 1
              return;
            }
          Next
        case rtPicture
          //step through pictureeditors
          foreach (frm In PictureEditors
            if (frm.PicNumber = SelResNum) {
              frm.MenuClickDescription 1
              return;
            }
          Next
        case rtSound
          //step through soundeditors
          foreach (frm In SoundEditors
            if (frm.SoundNumber = SelResNum) {
              frm.MenuClickDescription 1
              return;
            }
          Next
        case rtView
          //step through Vieweditors
          foreach (frm In ViewEditors
            if (frm.ViewNumber = SelResNum) {
              frm.MenuClickDescription 1
              return;
            }
          Next
        }

        //Debug.Assert FirstProp > 0 && FirstProp < 3
        //set current values
        switch (SelResType
        case rtObjects
          strTempD = InvObjects.Description

        case rtWords
          strTempD = WordList.Description

        case rtLogic
          strID = Logics(SelResNum).ID
          strTempD = Logics(SelResNum).Description

        case rtPicture
          strID = Pictures(SelResNum).ID
          strTempD = Pictures(SelResNum).Description

        case rtSound
          strID = Sounds(SelResNum).ID
          strTempD = Sounds(SelResNum).Description

        case rtView
          strID = Views(SelResNum).ID
          strTempD = Views(SelResNum).Description
        }

        GetNewResID SelResType, SelResNum, strID, strTempD, true, FirstProp
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
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

        case rtLogic
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

        case rtPicture
          ExportPicture Pictures(SelResNum), true

        case rtSound
          ExportSound Sounds(SelResNum), true

        case rtView
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
        case rtLogic
          //step through logiceditors
          foreach (frm In LogicEditors
            if (frm.LogicNumber = SelResNum) {
              frm.MenuClickRenumber
              return;
            }
          Next
        case rtPicture
          //step through pictureeditors
          foreach (frm In PictureEditors
            if (frm.PicNumber = SelResNum) {
              frm.MenuClickRenumber
              return;
            }
          Next
        case rtSound
          //step through soundeditors
          foreach (frm In SoundEditors
            if (frm.SoundNumber = SelResNum) {
              frm.MenuClickRenumber
              return;
            }
          Next
        case rtView
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
        if (SelResType = rtLogic) {
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
        if (SelResType = rtLogic) {
          //if ID changed because of renumbering
          if (Logics(SelResNum).ID != strOldID) {
            //if old default file exists
            if (File.Exists(ResDir + strOldID + LogicSourceSettings.SourceExt)) {
              On Error Resume Next
              //rename it
              Name ResDir + strOldID + LogicSourceSettings.SourceExt As ResDir + Logics(SelResNum).ID + LogicSourceSettings.SourceExt
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
      public void SelectedItemPrint()

        Dim blnLoaded As Boolean

        On Error GoTo ErrHandler

        switch (SelResType
        case rtLogic
          //load logic if necessary
          blnLoaded = Logics(SelResNum).Loaded
          if (!blnLoaded) {
            Logics(SelResNum).Load
          }
          //show logic printing form
          Load frmPrint
          frmPrint.SetMode rtLogic, Logics(SelResNum), , true
          KeepFocus = true
          frmPrint.Show vbModal, Me
          KeepFocus = false

          //unload logic if necessary
          if (!blnLoaded) {
            Logics(SelResNum).Unload
          }

        case rtPicture
          //load picture if necessary
          blnLoaded = Pictures(SelResNum).Loaded
          if (!blnLoaded) {
            Pictures(SelResNum).Load
          }
          Load frmPrint
          frmPrint.SetMode rtPicture, Pictures(SelResNum), , true
          KeepFocus = true
          frmPrint.Show vbModal, Me
          KeepFocus = false

          //unload picture if necessary
          if (!blnLoaded) {
            Pictures(SelResNum).Unload
          }

        case rtSound
          //load sound if necessary
          blnLoaded = Sounds(SelResNum).Loaded
          if (!blnLoaded) {
            Sounds(SelResNum).Load
          }
          Load frmPrint
          frmPrint.SetMode rtSound, Sounds(SelResNum), , true
          KeepFocus = true
          frmPrint.Show vbModal, Me
          KeepFocus = false

          //unload sound if necessary
          if (!blnLoaded) {
            Sounds(SelResNum).Unload
          }

        case rtView
          //load view if necessary
          blnLoaded = Views(SelResNum).Loaded
          if (!blnLoaded) {
            Views(SelResNum).Load
          }
          Load frmPrint
          frmPrint.SetMode rtView, Views(SelResNum), , true
          KeepFocus = true
          frmPrint.Show vbModal, Me
          KeepFocus = false

          //unload logic if necessary
          if (!blnLoaded) {
            Views(SelResNum).Unload
          }

        case rtObjects
          Load frmPrint
          frmPrint.SetMode rtObjects, InvObjects, , true
          KeepFocus = true
          frmPrint.Show vbModal, Me
          KeepFocus = false

        case rtWords
          //if nothing to print,
          if (WordList.WordCount = 0) {
            MessageBox.Show( "There are no words in this word list to print.", vbInformation + vbOKOnly, "Print Word List"
            return;
          }

          Load frmPrint
          frmPrint.SetMode rtWords, WordList, , true
          KeepFocus = true
          frmPrint.Show vbModal, Me
          KeepFocus = false

        }
      return;

      ErrHandler:
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
            case rtLogic
              .lblStatus.Caption = "Adding " + ResourceName(Logics(ResNum), true, true)
              SetLogicCompiledStatus ResNum, true

            case rtPicture
              .lblStatus.Caption = "Adding " + ResourceName(Pictures(ResNum), true, true)
            case rtSound
              .lblStatus.Caption = "Adding " + ResourceName(Sounds(ResNum), true, true)
            case rtView
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
            if (ResType = rtLogic) {
              //need to parse out the warnings
              strWarnings = Split(ErrString, "|")
              For i = 0 To UBound(strWarnings)
                AddWarning strWarnings(i), rtLogic, ResNum
              Next i
            } else {
              AddWarning ErrString, ResType, ResNum
            }

          case csResError
            .lblStatus.Caption = "Compiler error"
            //need to increment error counter, and store this error
            .lblErrors.Caption = .lblErrors.Caption + 1
            switch (ResType
            case rtLogic
              strID = ResourceName(Logics(ResNum), true, true)
            case rtPicture
              strID = ResourceName(Pictures(ResNum), true, true)
            case rtSound
              strID = ResourceName(Sounds(ResNum), true, true)
            case rtView
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
            case rtLogic, rtPicture, rtView, rtSound
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
        AddWarning Warning, rtLogic, CLng(LogNum)

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

            NewType = rtLogic
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

            NewType = rtPicture
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

            NewType = rtSound
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

            NewType = rtView
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
          if (ActiveForm = null) {
            switch (SelResType
            case rtGame, rtNone
              AdjustMenus rtGame, true, false, false

            case rtLogic, rtPicture, rtSound, rtView
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
              if (!ActiveForm = null) {
                //reset menus for active form
                ActiveForm.Activate
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
          if (rtRes = rtLogic && rtRes = rtText) {
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
            if (rtRes = rtLogic && rtRes = rtText) {
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
          if (ActiveForm = null) {
            switch (SelResType
            case rtGame, rtNone
              AdjustMenus rtGame, true, false, false

            case rtLogic, rtPicture, rtSound, rtView
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
            case rtLogic, rtPicture, rtSound, rtView
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
            if (SelResType = rtPicture && KeyCode = Keys.S) {
              //save Image as ...
              frmPreview.MenuClickCustom1
            }
          }
        } else if ( Shift = vbCtrlMask) {
          switch (KeyCode
          case Keys.F //Ctrl+F (Find)
            switch (SelResType
            case rtLogic, rtPicture, rtSound, rtView
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
              if (!ActiveForm = null) {
                //reset menus for active form
                ActiveForm.Activate
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

      void mnuCWPrint_Click()

        //print the list
        Load frmPrint
        frmPrint.SetMode rtWarnings, null
        frmPrint.Show vbModal, Me

      }

      void mnuECustom1_Click()

        On Error Resume Next

        ActiveForm.MenuClickECustom1
      }
      void mnuECustom2_Click()

        On Error Resume Next

        ActiveForm.MenuClickECustom2
      }


      void mnuECustom3_Click()

        On Error Resume Next

        ActiveForm.MenuClickECustom3
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
      //////  this.ActiveForm.SetEditMenu
      //////return;
      //////
      //////ErrHandler:
      //////  //Debug.Assert false
      //////  Resume Next
      }

      void mnuERedo_Click()

        On Error Resume Next

        ActiveForm.MenuClickRedo
      }

      void mnuEReplace_Click()

        On Error Resume Next

        ActiveForm.MenuClickReplace
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
          LogicSourceSettings.UseReservedNames = (.chkUseReserved.Value = vbChecked)

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

        if (ActiveForm Is PreviewWin) {
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


      void mnuTCustom_Click(Index As Integer)

        Dim rtn As Long
        Dim strTemp As String
        Dim intFile As Integer
        Dim OldPath As String
        Dim strFile As String, strNewPath As String

        //if a url
        if (Left$(mnuTCustom(Index).Tag, 4) = "http" && Left$(mnuTCustom(Index).Tag, 4) = "www.") {
          //open as a url, not a file

          //create a temporary file that is the url
          strTemp = Path.GetTempFileName()

          //open it
          intFile = FreeFile()
          Open strTemp For Output As intFile
          Print #intFile, "[InternetShortcut]"
          Print #intFile, "URL=" + mnuTCustom(Index).Tag
          Close #intFile

          On Error Resume Next
          Name strTemp As strTemp + ".url"
          rtn = ShellExecute(this.hWnd, "open", strTemp + ".url", "", "", SW_SHOWNORMAL)
          Kill strTemp + ".url"
        } else {
          //execute the command stored in the tag property
          On Error Resume Next

          //save the old path, so it can be restored once we//re done
          OldPath = CurDir$()

          //if a game is open, assume it's the current directory;
          //otherwise, assume program directory is current directory
          if (GameLoaded) {
            ChDrive GameDir
            ChDir GameDir
          } else {
            ChDrive ProgramDir
            ChDir ProgramDir
          }

          //does this tool entry include a directory?
          strNewPath = JustPath(mnuTCustom(Index).Tag, false)

          if (Len(strNewPath) > 0) {
          //if a path is provided, check for program dir
            strNewPath = Replace(strNewPath, "%PROGDIR%", ProgramDir)
            strFile = strNewPath + Path.GetFileName(mnuTCustom(Index).Tag)
          } else {
            strFile = mnuTCustom(Index).Tag
          }

          rtn = ShellExecute(this.hWnd, "open", strFile, "", "", SW_SHOWNORMAL)

          if (rtn <= 32) {
            //error - display a msg to user
            switch (rtn
            //regular WinExec() codes
            case 2 //#define SE_ERR_FNF              2       // file not found
              MessageBox.Show( "Sorry, the file was not found." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "File !Found"
            case 3 //#define SE_ERR_PNF              3       // path not found
              MessageBox.Show( "Sorry, the path in this file name was not found." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "Path !Found"
            case 5 //#define SE_ERR_ACCESSDENIED     5       // access denied
              MessageBox.Show( "Sorry, this file could not be accessed." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "Access Denied"
            case 8 //#define SE_ERR_OOM              8       // out of memory
              MessageBox.Show( "Holy CRAP! You are out of memory!" + vbNewLine + "You might want to free up some memory before trying to open this file/program.", vbInformation + vbOKOnly, "Out of Memory"
            case 32 //#define SE_ERR_DLLNOTFOUND              32
              MessageBox.Show( "Sorry, a supporting DLL for this file/program was not found." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "DLL !Found"
            //
            //error values for ShellExecute() beyond the regular WinExec() codes
            case 26 //#define SE_ERR_SHARE                    26
              MessageBox.Show( "Sorry, a share violation occurred." + vbNewLine + "Please address the file share issue, then try again.", vbInformation + vbOKOnly, "Share Violation"
            case 27 //#define SE_ERR_ASSOCINCOMPLETE          27
              MessageBox.Show( "Sorry, unable to determine the correct association for this file." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "Incomplete Association"
            case 28 //#define SE_ERR_DDETIMEOUT               28
              MessageBox.Show( "Sorry, a DDE timeout error occurred." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "DDE Timeout Error"
            case 29 //#define SE_ERR_DDEFAIL                  29
              MessageBox.Show( "Sorry, DDE failed; unable to open this file/program." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "DDE Failure"
            case 30 //#define SE_ERR_DDEBUSY                  30
              MessageBox.Show( "Sorry, DDE was not able to open this file/program." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "DDE Busy Error"
            case 31 //#define SE_ERR_NOASSOC                  31
              MessageBox.Show( "Sorry, an association was not found." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "Association !Found"
            default:
              MessageBox.Show( "Sorry, an error occurred when trying to open this tool entry." + vbNewLine + "Please edit your tool information to point to a valid file/program.", vbInformation + vbOKOnly, "Unknown Error"
            }
          }

          //restore current directory
          ChDrive OldPath
          ChDir OldPath
        }
      }

      void mnuTCustomize_Click()

        KeepFocus = true
        frmTools.Show vbModal, Me
        KeepFocus = false
      }

      void mnuTGlobals_Click()
        On Error GoTo ErrHandler

        //open the game//s globals

        //if already have game global window open, menu forces loading external list
        OpenGlobals GEInUse
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
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
        if (MDIMain.ActiveForm.Name = "frmLogicEdit" && MDIMain.ActiveForm.Name = "frmTextEdit") {
          //force focus to edit control
          MDIMain.ActiveForm.rtfLogic.Focus();
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

      void mnuWTileH_Click()

        //tile horizontal
        MDIMain.Arrange vbTileHorizontal
      }

      void mnuWTileV_Click()

        //tile vertically
        MDIMain.Arrange vbTileVertical
      }

      void picProperties_GotFocus()

        //if nothing selected, exit
        if (SelResType = rtNone) {
          return;
        }

        //if clicked or tabbed to get here,
        //and nothing is selected yet,
        //select the first property
        if (SelectedProp = 0) {
          //Debug.Assert PropRowCount > 0
          SelectedProp = 1
        }
        PaintPropertyWindow

        //if not using preview window then focus
        //events must be tracked
        if (!Settings.ShowPreview) {
          //if mdi form already has focus
          //don't do anything else
          if (MDIHasFocus) {
            return;
          }
          //force focus marker to true
          MDIHasFocus = true

          //set menus based on currently selected item

          switch (SelResType
          case rtGame  //root
            AdjustMenus rtGame, true, false, false

          case 2 To 5 //resource header
            AdjustMenus rtNone, true, false, false

          case rtLogic, rtPicture, rtSound, rtView  //logic, picture, sound, view
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

      void picProperties_KeyPress(KeyAscii As Integer)

        On Error GoTo ErrHandler

        if (PreviewWin.Visible) {
          PreviewWin.KeyHandler KeyAscii
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void picProperties_LostFocus()

        //if not using the preview window, then
        //focus events must be tracked

        if (!Settings.ShowPreview) {
          MDIHasFocus = false

          //make sure something is active
          if (ActiveControl = null) {
            return;
          }

          //if active control is still picProperties
          //then an open form must have been clicked
          if (ActiveControl.Name = "picProperties") {
            if (!ActiveForm = null) {
              //reset menus for active form
              ActiveForm.Activate
            }
          } else {
            //if focus is staying on the form
            if (ActiveControl.Name = "tvwResources" && _
               ActiveControl.Name = "picSplitRes" && _
               ActiveControl.Name = "picSplitV" && _
               ActiveControl.Name = "cmbResType" && _
               ActiveControl.Name = "lstResources") {
              //force focus marker back to true
              MDIHasFocus = true
            }
          }


        }

        //if not overriding repaint
        if (!NoPaint) {
          PaintPropertyWindow
        }
        NoPaint = false
      }

      void picSplitH_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        //begin split operation
        picSplitHIcon.Width = picSplitH.Width
        picSplitHIcon.Move picSplitH.Left, picSplitH.Top
        picSplitHIcon.Visible = true

        //save offset
        SplitHOffset = picSplitH.Top - Y
      }


      void picSplitH_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim Pos As Single

        //if splitting
        if (picSplitHIcon.Visible) {
          Pos = Y + SplitHOffset
          //limit movement- split pos determines size of panel BELOW split
          //need to do some math...
          if (picBottom.Top + picBottom.Height - Pos < MIN_SPLIT_H) {
            Pos = picBottom.Top + picBottom.Height - MIN_SPLIT_H
          }

          if (picBottom.Top + picBottom.Height - Pos > MAX_SPLIT_H) {
            Pos = picBottom.Top + picBottom.Height - MAX_SPLIT_H
          }

          //also need to limit movement upwards so there is a minimum amount of space available
          if (Pos < 3000) {
            Pos = 3000
          }

          //move splitter icon and splitter form
          picSplitHIcon.Top = Pos
        }
      }


      void picSplitH_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim Pos As Single
        //if splitting
        if (picSplitHIcon.Visible) {
          //stop splitting
          picSplitHIcon.Visible = false

          Pos = Y + SplitHOffset
          //limit movement- split pos determines size of panel BELOW split
          //need to do some math...
          if (picBottom.Top + picBottom.Height - Pos < MIN_SPLIT_H) {
            Pos = picBottom.Top + picBottom.Height - MIN_SPLIT_H
          }

          if (picBottom.Top + picBottom.Height - Pos > MAX_SPLIT_H) {
            Pos = picBottom.Top + picBottom.Height - MAX_SPLIT_H
          }

          //also need to limit movement upwards so there is a minimum amount of space available
          if (Pos < 3000) {
            Pos = 3000
          }

          //redraw!
          UpdateSplitH Pos

          //if focus came from MDI
          if (MDIHasFocus) {
            //force focus back to mdi (use resource list)
            switch (Settings.ResListType
            case 1
              tvwResources.Focus();
            case 2
              lstResources.Focus();
            }
          } else {
            //force focus back to active form, if there is one
            if (!ActiveForm = null) {
              ActiveForm.Focus();
            }
          }
        }
      }


      void picSplitH_Paint()

        //draw a line across bottom row of pixels to act as a border for the grid

        //sometimes the splitter isn//t exactly on the right spot after resizing; can
        //the paint method find and fix that?

      }

      void picSplitRes_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        //begin split operation
        picSplitResIcon.Width = picSplitRes.Width
        picSplitResIcon.Move picSplitRes.Left, picSplitRes.Top
        picSplitResIcon.Visible = true

        //save offset
        SplitResOffset = picSplitRes.Top - Y
      }


      void picSplitRes_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim tmpHeight As Long

        //if splitting
        if (picSplitResIcon.Visible) {
          tmpHeight = (picResources.ScaleHeight - (Y + SplitResOffset) - SPLIT_HEIGHT) / PropRowHeight - 1
          //limit movement
          if (tmpHeight < MIN_SPLIT_RES) {
            tmpHeight = MIN_SPLIT_RES
          } else if ( tmpHeight > MAX_SPLIT_RES) {
            tmpHeight = MAX_SPLIT_RES
          }

          //move splitter icon and splitter form (adjust by one to account for header)
          picSplitResIcon.Top = picResources.ScaleHeight - ((tmpHeight + 1) * PropRowHeight) - SPLIT_HEIGHT
        }
      }


      void picSplitRes_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim tmpHeight As Long

        //if splitting
        if (picSplitResIcon.Visible) {
          //stop splitting
          picSplitResIcon.Visible = false

          tmpHeight = (picResources.ScaleHeight - (Y + SplitResOffset) - SPLIT_HEIGHT) / PropRowHeight - 1
          //limit movement
          if (tmpHeight < MIN_SPLIT_RES) {
            tmpHeight = MIN_SPLIT_RES
          } else if ( tmpHeight > MAX_SPLIT_RES) {
            tmpHeight = MAX_SPLIT_RES
          }

          //save rowcount
          PropRowCount = tmpHeight

          //redraw (adjust by one to account for header)
          UpdateSplitRes picResources.ScaleHeight - ((tmpHeight + 1) * PropRowHeight) - SPLIT_HEIGHT
          PaintPropertyWindow

          //if focus came from MDI
          if (MDIHasFocus) {
            //force focus back to mdi (use resource list)
            switch (Settings.ResListType
            case 1
              tvwResources.Focus();
            case 2
              lstResources.Focus();
            }
          } else {
            //force focus back to active form, if there is one
            if (!ActiveForm = null) {
              ActiveForm.Focus();
            }
          }
        }
      }


      void picSplitV_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        //begin split operation
        picSplitVIcon.Height = picSplitV.Height
        picSplitVIcon.Move picSplitV.Left, picSplitV.Top
        picSplitVIcon.Visible = true

        //save offset
        SplitVOffset = picSplitV.Left - X
      }


      void picSplitV_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim Pos As Single

        //if splitting
        if (picSplitVIcon.Visible) {
          Pos = X + SplitVOffset
          //limit movement
          if (Pos < MIN_SPLIT_V) {
            Pos = MIN_SPLIT_V
          } else if ( Pos > MAX_SPLIT_V) {
            Pos = MAX_SPLIT_V
          }

          //move splitter icon and splitter form
          picSplitVIcon.Left = Pos
        }
      }


      void picSplitV_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim Pos As Single

        //if splitting
        if (picSplitVIcon.Visible) {

          //stop splitting
          picSplitVIcon.Visible = false

          Pos = X + SplitVOffset
          //limit movement
          if (Pos < MIN_SPLIT_V) {
            Pos = MIN_SPLIT_V
          } else if ( Pos > MAX_SPLIT_V) {
            Pos = MAX_SPLIT_V
          }

          //redraw! (don't adjust for splitpos; it's builtin to the splitting process)
          UpdateSplitV Pos

          PaintPropertyWindow

          //if focus came from MDI
          if (MDIHasFocus) {
            //force focus back to mdi (use resource list)
            switch (Settings.ResListType
            case 1
              tvwResources.Focus();
            case 2
              lstResources.Focus();
            }
          } else {
            //force focus back to active form, if there is one
            if (!ActiveForm = null) {
              ActiveForm.Focus();
            }
          }
        }
      }

      void picWarnings_Paint()

        //draw that darn line?
        picWarnings.Line (0, 30)-Step(picWarnings.Width, 0), Color.Black

      }

      void picWarnings_Resize()

        On Error GoTo ErrHandler

        Dim lngVarW As Long

        if (!MDIMain.Visible) {
          return;
        }

        With fgWarnings
          .Width = picWarnings.ScaleWidth
          .Height = picWarnings.ScaleHeight - picSplitH.Height
          .ColWidth(0) = 0  //path for module, if there is one
          .ColWidth(1) = 360 //left border column, fixed
          .ColWidth(2) = 1080 //warning/error number
          lngVarW = .Width - (4560 + 360 + 270)
          if (lngVarW < 360) {
            lngVarW = 360
          }
          .ColWidth(3) = lngVarW // description
          .ColWidth(4) = 600 //res num
          .ColWidth(5) = 720 //line #
          .ColWidth(6) = 2160 //module
        End With
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
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
        if (ActiveForm = null) {
          return;
        }

        switch (ActiveForm.Name
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
              if (ActiveForm.ScaleFactor > 1) {
                ActiveForm.ScaleFactor = ActiveForm.ScaleFactor - 1
              }
            } else {
              //if not at Max
              if (ActiveForm.ScaleFactor < 4) {
                ActiveForm.ScaleFactor = ActiveForm.ScaleFactor + 1
              }
            }
            //redraw pictures
            ActiveForm.ResizePictures
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
              ActiveForm.ZoomScale -1
            } else {
              ActiveForm.ZoomScale 1
            }
          }

        case "frmViewEdit"
          switch (Panel.Key
          case "Scale"
            if (StatusMouseBtn = vbRightButton) {
              //zoom in
              ActiveForm.ZoomCel 0
            } else {
              //zoom out
              ActiveForm.ZoomCel 1
            }
          }

        case "frmTextEdit"
        case "frmObjectEdit"
          switch (Panel.Key
          case "Encrypt"
            //toggle encryption
            ActiveForm.MenuClickCustom3
          }

        case "frmWordsEdit"
        case "frmLayout"
          switch (Panel.Key
          case "Scale"
            if (StatusMouseBtn = vbRightButton) {
              ActiveForm.ChangeScale 1
            } else {
              ActiveForm.ChangeScale -1
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
          if (ActiveForm = null) {
            //if no node selected, select first node
            if (tvwResources.SelectedItem = null) {
              tvwResources.SelectedItem = tvwResources.Nodes[1)
            }

            switch (SelResType
            case rtGame
              AdjustMenus rtGame, true, false, false

            case rtLogic, rtPicture, rtSound, rtView
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
            case rtLogic, rtPicture, rtSound, rtView
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
            if (SelResType = rtPicture && KeyCode = Keys.S) {
              //save Image as ...
              frmPreview.MenuClickCustom1
            }
          }
        } else if ( Shift = vbCtrlMask) {
          switch (KeyCode
          case Keys.F //Ctrl+F (Find)
            switch (SelResType
            case rtLogic, rtPicture, rtSound, rtView
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
              if (!ActiveForm = null) {
                //reset menus for active form
                ActiveForm.Activate
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

      void picProperties_DblClick()

        //set dblclick mode (to allow properties to be toggled)
        PropDblClick = true

        //call mouse down again
        picProperties_MouseDown 0, 0, mPX, mPY
      }

      void picProperties_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim tmpProp As Long
        Dim rtn As Long, blnDblClick As Boolean
        Dim strNewDir As String
        Dim Reason As EUReason
        Dim i As Long

        On Error GoTo ErrHandler

        //local copy of dblclick mode
        blnDblClick = PropDblClick
        //clear global dblclick mode
        PropDblClick = false

        //save position in case of double click
        mPX = X
        mPY = Y

        //if not in a game
        if (!GameLoaded) {
          return;
        }

        //override repaint
        NoPaint = true

        if (Settings.ShowPreview) {
          if (!ActiveForm Is PreviewWin && Settings.ShiftPreview) {
            //now set form focus to preview
            if (!PreviewWin.Visible) {
              PreviewWin.Show
            }
            PreviewWin.Focus();
            //set control focus to picproperties
            picProperties.Focus();
          }
        }

        //force control focus to picProperties
        picProperties.Focus();

        tmpProp = Y \ PropRowHeight

        //verify not out of bounds
        if (tmpProp > PropRowCount) {
          tmpProp = PropRowCount
        }
        if (tmpProp < 0) {
          tmpProp = 0
        }

        //if not clicking on header
        if (tmpProp > 0) {
          tmpProp = tmpProp + PropScroll
        }

        //if past limit for selected item
        switch (SelResType
        case rtGame //root - 10 props
          if (tmpProp > 10) {
            return;
          }

        case rtLogic
          if (SelResNum = -1) {
            //logic header 4 props
            if (tmpProp > 4) {
              return;
            }
          } else {
            //logic resource
            //4 editable properties; 4 read-only
            if (tmpProp > 8) {
              return;
            }
          }

        case rtPicture, rtSound
          if (SelResNum = -1) {
            //other resource headers - 1 property
            if (tmpProp > 1) {
              return;
            }
          } else {
            //picture and sound
            //3 editable properties, 3 read-only
            if (tmpProp > 6) {
              return;
            }
          }

        case rtView
          if (SelResNum = -1) {
            //other resource headers - 1 property
            if (tmpProp > 1) {
              return;
            }
          } else {
            //view
            //4 editable properties, 3 read-only
            if (tmpProp > 7) {
              return;
            }
          }

        case rtObjects
          //OBJECT - 4 props
          if (tmpProp > 4) {
            return;
          }

        case rtWords
          //WORDS.TOK - 3 props;
          if (tmpProp > 3) {
            return;
          }
        }

        //if property selected was clicked
        if (tmpProp = SelectedProp) {
          //check which property
          switch (SelResType
          case rtGame //root
            switch (SelectedProp
            case 1  //gameid
              //display textbox
              DisplayPropertyEditBox PropSplitLoc + 3, PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "GAMEID", 0
            case 2  //gameauthor
              //display textbox
              DisplayPropertyEditBox PropSplitLoc + 3, (2 - PropScroll) * PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "GAMEAUTHOR", 0

            case 3  //gamedir
              //game dir is now read only

            case 4  //resdir
              //display textbox
              DisplayPropertyEditBox PropSplitLoc + 3, (4 - PropScroll) * PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "RESDIR", 0

            case 5  //int version
              if ((X > picProperties.Width - 17) && blnDblClick) {
                //copy pressed dropdown picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (5 - PropScroll) * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                //display list box
                DisplayPropertyListBox PropSplitLoc, (5 - PropScroll) * PropRowHeight, picProperties.Width - PropSplitLoc - 17, ListItemHeight * 4, "INTVER"
              }

            case 6  //Description
              //if button clicked
              if (((X > picProperties.Width - 17) && blnDblClick) && !txtProperty.Visible) {
                //copy pressed dropover picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (6 - PropScroll) * PropRowHeight, 17, 17, DropOverDC, 18, 0, SRCCOPY)
                //display edit box
                DisplayPropertyEditBox picProperties.Width, 0, 2 * picProperties.Width, picProperties.Height, "GAMEDESC", 1
                rtn = BringWindowToTop(txtProperty.hWnd)
              }

            case 7  //game version
              //if button clicked
              if (((X > picProperties.Width - 17) && blnDblClick) && !txtProperty.Visible) {
                //copy pressed dropover picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (7 - PropScroll) * PropRowHeight, 17, 17, DropOverDC, 18, 0, SRCCOPY)
                //display edit box
                DisplayPropertyEditBox picProperties.Width, 0, 2 * picProperties.Width, picProperties.Height, "GAMEVER", 1
                rtn = BringWindowToTop(txtProperty.hWnd)
              }

            case 8  //game about
              //if button clicked
              if (((X > picProperties.Width - 17) && blnDblClick) && !txtProperty.Visible) {
                //copy pressed dropover picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (8 - PropScroll) * PropRowHeight, 17, 17, DropOverDC, 18, 0, SRCCOPY)
                //display edit box
                DisplayPropertyEditBox picProperties.Width, 0, 2 * picProperties.Width, picProperties.Height, "GAMEABOUT", 1
                rtn = BringWindowToTop(txtProperty.hWnd)
              }

            case 9  //use Layout Editor
              //if dblclicking AND on actual property
              if (blnDblClick && X > PropSplitLoc && X < picProperties.Width - 17) {
                //toggle useresdef
                UseLE = !UseLE
                PaintPropertyWindow
                //update toolbar buttons and close LE if needed
                UpdateLEStatus

              //if button is clicked
              } else if ( X > picProperties.Width - 17) {
                //copy pressed dropdown picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (9 - PropScroll) * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                //show list box offering choice of true or false
                DisplayPropertyListBox PropSplitLoc, (9 - PropScroll) * PropRowHeight, picProperties.Width - PropSplitLoc - 17, ListItemHeight * 2, "USELE"
              }

            case 10  //last edit date
              //read only

            }

          case rtLogic
            if (SelResNum = -1) {
              // logic header
              switch (SelectedProp
              case 2  //globals
                //if button is clicked
                if ((X > picProperties.Width - 17) && blnDblClick) {
                  //copy pressed dropdialog picture
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (2 - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 18, 0, SRCCOPY)
                  //display edit globals dialog
                  //by invoking menu bar item
                  mnuTGlobals_Click
                }

              case 3  //use reserved defines
                //if dblclicking AND on actual property
                if (blnDblClick && X > PropSplitLoc && X < picProperties.Width - 17) {
                  //toggle useresdef
                  LogicSourceSettings.UseReservedNames = !LogicSourceSettings.UseReservedNames
                  PaintPropertyWindow
                //if button is clicked
                } else if ( X > picProperties.Width - 17) {
                  //copy pressed dropdown picture
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (3 - PropScroll) * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                  //show list box offering choice of true or false
                  DisplayPropertyListBox PropSplitLoc, (3 - PropScroll) * PropRowHeight, picProperties.Width - PropSplitLoc - 17, ListItemHeight * 2, "USERESNAMES"
                }

              }
            } else {
              //logic resource
              //if selected prop is 1,2,3
              switch (SelectedProp
              case 1, 2, 3
                //if button is clicked or doubleclicking
                if (X > picProperties.Width - 17 && blnDblClick) {
                  //copy pressed dropdialog picture
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (SelectedProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 18, 0, SRCCOPY)
                  if (SelectedProp = 1) {
                    //use selected item method
                    SelectedItemRenumber
                  } else {
                    //use selected item method (remember to adjust selprop by 1)
                    SelectedItemDescription SelectedProp - 1
                  }
                  //reset dropdialog button
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (SelectedProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 0, 0, SRCCOPY)
                }

              case 4  //isroom (used to be 6)
                //if dblclicking AND on actual property
                if (blnDblClick && X > PropSplitLoc && X < picProperties.Width - 17) {
                  //toggle isroom
                  if (SelResNum != 0) {
                    Logics(SelResNum).IsRoom = !Logics(SelResNum).IsRoom
                    if (UseLE) {
                      if (Logics(SelResNum).IsRoom) {
                        Reason = euShowRoom
                      } else {
                        Reason = euRemoveRoom
                      }
                      //update layout editor and data file to show new room status
                      UpdateExitInfo Reason, SelResNum, Logics(SelResNum)
                    }
                    PaintPropertyWindow

                    //update the logic editor if it's open
                    For i = 1 To LogicEditors.Count
                      if (LogicEditors(i).LogicNumber = SelResNum) {
                        LogicEditors(i).LogicEdit.IsRoom = Logics(SelResNum).IsRoom
                        Exit For
                      }
                    Next i
                  }

                //if button is clicked
                } else if ( X > picProperties.Width - 17) {
                  if (SelResNum != 0) {
                    //copy pressed dropdown picture
                    rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (4 - PropScroll) * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                    //display list box
                    DisplayPropertyListBox PropSplitLoc, (4 - PropScroll) * PropRowHeight, picProperties.Width - PropSplitLoc - 17, ListItemHeight * 2, "ISROOM"
                  }
                }
              }
            }

          case rtPicture
            //header is readonly
            if (SelResNum >= 0) {
              //pictures, sounds
              //if button is clicked or doubleclicking
              if (X > picProperties.Width - 17 && blnDblClick) {
                //copy pressed dropdialog picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (SelectedProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 18, 0, SRCCOPY)

                //if number
                if (SelectedProp = 1) {
                  //use selected item method
                  SelectedItemRenumber
                } else {
                  //use selected item method (remember to adjust selprop by 1)
                  SelectedItemDescription SelectedProp - 1
                }
                //reset dropdialog button
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (tmpProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 0, 0, SRCCOPY)
              }
            }

          case rtSound
            //header is readonly
            if (SelResNum >= 0) {
              //pictures, sounds
              //if button is clicked or doubleclicking
              if (X > picProperties.Width - 17 && blnDblClick) {
                //copy pressed dropdialog picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (SelectedProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 18, 0, SRCCOPY)

                //if number
                if (SelectedProp = 1) {
                  //use selected item method
                  SelectedItemRenumber
                } else {
                  //use selected item method (remember to adjust selprop by 1)
                  SelectedItemDescription SelectedProp - 1
                }
                //reset dropdialog button
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (tmpProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 0, 0, SRCCOPY)
              }
            }

          case rtView
            //header is readonly
            if (SelResNum >= 0) {
              //views
              //if button is clicked or doubleclicking
              if (X > picProperties.Width - 17 && blnDblClick) {
                switch (SelectedProp
                case 1  //number
                  //copy pressed dropdialog picture
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (SelectedProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 18, 0, SRCCOPY)
                  //use selected item method
                  SelectedItemRenumber
                  //reset dropdialog button
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (tmpProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 0, 0, SRCCOPY)

                case 2, 3 //id/desc
                  //copy pressed dropdialog picture
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (SelectedProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 18, 0, SRCCOPY)
                  //use selected item method (remember to adjust selprop by 1)
                  SelectedItemDescription SelectedProp - 1
                 //reset dropdialog button
                  rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (tmpProp - PropScroll) * PropRowHeight, 17, 17, DropDlgDC, 0, 0, SRCCOPY)

                case 4  //view desc
                  //if text box is visible, don't do anything
                  if (!txtProperty.Visible) {
                    //copy pressed dropdover picture
                    rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (SelectedProp - PropScroll) * PropRowHeight, 17, 17, DropOverDC, 18, 0, SRCCOPY)
                    //display edit box
                    DisplayPropertyEditBox picProperties.Width, 0, 2 * picProperties.Width, picProperties.Height, "VIEWDESC", 1
                  }
                }
              }
            }

          case rtObjects
            switch (SelectedProp
            case 2 // description
              //if button clicked
              if (((X > picProperties.Width - 17) && blnDblClick) && !txtProperty.Visible) {
                //copy pressed dropover picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, SelectedProp * PropRowHeight, 17, 17, DropOverDC, 18, 0, SRCCOPY)
                //display edit box
                DisplayPropertyEditBox picProperties.Width, 0, 2 * picProperties.Width, picProperties.Height, "OBJDESC", 1
              }
            case 3  //encryption
              //if dblclicking AND on actual property
              if (blnDblClick && X > PropSplitLoc && X < picProperties.Width - 17) {
                //toggle encryption
                InvObjects.Encrypted = !InvObjects.Encrypted
                InvObjects.Save
                PaintPropertyWindow
                //if the editor is open,
                if (!ObjectEditor = null) {
                  //set its encryption status to match
                  ObjectEditor.ObjectsEdit.Encrypted = InvObjects.Encrypted
                }

              //if button is clicked
              } else if ( X > picProperties.Width - 17) {
                //copy pressed dropdown picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (3 - PropScroll) * PropRowHeight, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                //display list box
                DisplayPropertyListBox PropSplitLoc, (3 - PropScroll) * PropRowHeight, picProperties.Width - PropSplitLoc - 17, ListItemHeight * 2, "OBJENCRYPT"
              }

            case 4  //Max screen objects
              DisplayPropertyEditBox PropSplitLoc + 3, (4 - PropScroll) * PropRowHeight + 1, picProperties.Width - PropSplitLoc - 4, PropRowHeight - 2, "MAXOBJ", 0
            }

          case rtWords
            switch (SelectedProp
            case 3 //description
              //if button clicked
              if (((X > picProperties.Width - 17) && blnDblClick) && !txtProperty.Visible) {
                //copy pressed dropover picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, (2 - PropScroll) * PropRowHeight, 17, 17, DropOverDC, 18, 0, SRCCOPY)
                //display edit box
                DisplayPropertyEditBox picProperties.Width, 0, 2 * picProperties.Width, picProperties.Height, "WORDSDESC", 1
              }
            }
          }

        //if not the same as current prop
        } else {
          //if changed
          if (tmpProp != SelectedProp && tmpProp != 0) {
            SelectedProp = tmpProp
            PaintPropertyWindow
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void DisplayPropertyEditBox(ByVal posX As Long, ByVal posY As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal strProp As String, ByVal lngBorderStyle As Long)
        //moves the edit box to appropriate position
        //preloads it with appropriate prop Value

        //convert pixels to twips, since mdiform scale is always twips

        //for style, 0 = no border, 1 = border

        //set border
        txtProperty.BorderStyle = lngBorderStyle

        txtProperty.Move picResources.Left + (picProperties.Left + posX) * ScreenTWIPSX, picResources.Top + (picProperties.Top + posY) * ScreenTWIPSY

        txtProperty.Width = nWidth * ScreenTWIPSX
        txtProperty.Height = nHeight * ScreenTWIPSY

        switch (strProp
        case "GAMEID"
          txtProperty.Text = GameID
        case "GAMEDESC"
          txtProperty.Text = GameDescription
        case "GAMEAUTHOR"
          txtProperty.Text = GameAuthor
        case "WORDSDESC"
          txtProperty.Text = WordList.Description
        case "OBJDESC"
          txtProperty.Text = InvObjects.Description
        case "GAMEVER"
          txtProperty.Text = GameVersion
        case "GAMEABOUT"
          txtProperty.Text = GameAbout
        case "RESDIR"
          txtProperty.Text = ResDirName
        case "VIEWDESC"
          if (!Views(SelResNum).Loaded) {
            Views(SelResNum).Load
            txtProperty.Text = Replace(Views(SelResNum).ViewDescription, vbLf, vbNewLine)
            Views(SelResNum).Unload
          } else {
            txtProperty.Text = Replace(Views(SelResNum).ViewDescription, vbLf, vbNewLine)
          }

        case "MAXOBJ"
          txtProperty.Text = InvObjects.MaxScreenObjects
        }

        //pass property to textbox tag
        txtProperty.Tag = strProp

        //show text
        txtProperty.ZOrder
        txtProperty.Visible = true
        //select it
        txtProperty.Focus();
        //move cursor to end
        txtProperty.SelStart = Len(txtProperty.Text)
      }


      void DisplayPropertyListBox(ByVal posX As Long, ByVal posY As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal strProp As String)
        //moves the list box to appropriate position
        //preloads it with appropriate prop Value

        Dim strVersions(15) As String
        Dim i As Long

        On Error GoTo ErrHandler

        //set height/width
        lstProperty.Width = nWidth
        lstProperty.Height = nHeight + 2 //+2 to account for border

        //if there is room
        if (posY < picProperties.Height - lstProperty.Height) {
          lstProperty.Move picProperties.Left + posX, picProperties.Top + posY
        } else {
          lstProperty.Move picProperties.Left + posX, picProperties.Top + picProperties.Height - lstProperty.Height
        }

        //clear the list box
        lstProperty.Clear

        switch (strProp
        case "INTVER"
          //load versions
          strVersions(0) = "2.089"
          strVersions(1) = "2.272"
          strVersions(2) = "2.411"
          strVersions(3) = "2.425"
          strVersions(4) = "2.426"
          strVersions(5) = "2.435"
          strVersions(6) = "2.439"
          strVersions(7) = "2.440"
          strVersions(8) = "2.915"
          strVersions(9) = "2.917"
          strVersions(10) = "2.936"
          strVersions(11) = "3.002086"
          strVersions(12) = "3.002098"
          strVersions(13) = "3.002102"
          strVersions(14) = "3.002107"
          strVersions(15) = "3.002149"
          With lstProperty
            For i = 0 To UBound(strVersions)
              .AddItem strVersions(i)
            Next i
          End With

          //check versions
          For i = 0 To lstProperty.ListCount - 1
            if (lstProperty.List(i) = InterpreterVersion) {
              lstProperty.SelectedIndex = i
              Exit For
            }
          Next i

        case "OBJENCRYPT"
          //load choices
          lstProperty.AddItem "false"
          lstProperty.AddItem "true"

          //select entry matching current Value
          if (InvObjects.Encrypted) {
            lstProperty.SelectedIndex = 1
          } else {
            lstProperty.SelectedIndex = 0
          }

        case "ISROOM"
          //load choices
          lstProperty.AddItem "false"
          lstProperty.AddItem "true"

          //select entry matching current Value
          if (Logics(SelResNum).IsRoom) {
            lstProperty.SelectedIndex = 1
          } else {
            lstProperty.SelectedIndex = 0
          }

        case "USERESNAMES"
          //load choices
          lstProperty.AddItem "false"
          lstProperty.AddItem "true"

          //select entry matching current Value
          if (LogicSourceSettings.UseReservedNames) {
            lstProperty.SelectedIndex = 1
          } else {
            lstProperty.SelectedIndex = 0
          }

        case "USELE"
          //load choices
          lstProperty.AddItem "false"
          lstProperty.AddItem "true"

          //select entry matching current value
          if (UseLE) {
            lstProperty.SelectedIndex = 1
          } else {
            lstProperty.SelectedIndex = 0
          }

        }

        //pass property to tag
        lstProperty.Tag = strProp

        //show list box
        lstProperty.Visible = true
        //set top index to the selected Value
        lstProperty.TopIndex = lstProperty.SelectedIndex
        //select it
        lstProperty.Focus();
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void picProperties_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)
        //cache the Y Value
        mPY = Y
      }

      void picProperties_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

      //  PaintPropertyWindow
      }


      void MDIForm_Resize()

        On Error GoTo ErrHandler

        //resize resource and warning windows to match left/bottom panels

        //use separate variables for managing minimum width/height
        if (ScaleWidth < MIN_WIDTH) {
          CalcWidth = MIN_WIDTH
        } else {
          CalcWidth = ScaleWidth
        }
        if (ScaleHeight < MIN_HEIGHT) {
          CalcHeight = MIN_HEIGHT
        } else {
          CalcHeight = ScaleHeight
        }

        //don't resize unless form is NOT minimized, and not resized too small
        if (this.WindowState != vbMinimized && this.Visible) {
          //resize horizontally
          if (picWarnings.Visible && ScaleWidth > MIN_WIDTH) {
            picWarnings.Width = this.Width - WLOffsetW - picResources.Width + 60
            picSplitH.Width = picWarnings.Width - 60
          }
          //resize vertically
          if (picResources.Visible) {
            if (this.Height - WLOffsetH + 60 + picWarnings.Height > 100) {
              picResources.Height = this.Height - WLOffsetH + 60 + picWarnings.Height
            }
            picSplitV.Height = picResources.Height
          }
          if (picWarnings.Visible) {
            picWarnings.Top = this.Height - WLOffsetH + this.Toolbar1.Height + 60
            picSplitH.Top = picWarnings.Top
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }


      void MDIForm_Unload(Cancel As Integer)

        Dim frm As Form
        Dim rtn As Long

        On Error GoTo ErrHandler

        if (GameLoaded) {
          CloseGame
        }

        //ensure all forms are unloaded
        foreach (frm In Forms
          if (frm.Name != "MDIMain") {
            Unload frm
          }
        Next

        //ensure all global objects are set to nothing
        ClipViewLoop = null
        ClipViewCel = null
        LogicEditors = null
        ViewEditors = null
        PictureEditors = null
        SoundEditors = null
        LayoutEditor = null
        ObjectEditor = null
        WordEditor = null
        GlobalsEditor = null
        PreviewWin = null
        NotePictures = null
        SoundClipboard = null
        MainStatusBar = null
        OpenDlg = null
        SaveDlg = null
        ViewClipboard = null
        WordsClipboard = null
        FindingForm = null
        SearchForm = null

        //reset parent for controls that were moved to the form
        rtn = SetParent(picResources.hWnd, picLeft.hWnd)
        rtn = SetParent(picWarnings.hWnd, picBottom.hWnd)
        rtn = SetParent(txtProperty.hWnd, picResources.hWnd)
        rtn = SetParent(picSplitVIcon.hWnd, picResources.hWnd)
        rtn = SetParent(picSplitV.hWnd, picResources.hWnd)
        rtn = SetParent(picSplitH.hWnd, picWarnings.hWnd)
        rtn = SetParent(picSplitHIcon.hWnd, picWarnings.hWnd)



        //release gdi plus object
        EndGDIPlus

        On Error Resume Next
      #if (DEBUGMODE != 1) {
        //release subclass hook to propwindow
        SetWindowLong picProperties.hWnd, GWL_WNDPROC, PrevPropWndProc
        //release subclass hook to flexgrid
        SetWindowLong fgWarnings.hWnd, GWL_WNDPROC, PrevFGWndProc
      #}
        //ensure temporary files are deleted
        Kill TempFileDir + "AGI*.tmp"
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuEClear_Click()

        On Error Resume Next

        ActiveForm.MenuClickClear
      }

      void mnuECopy_Click()

        On Error Resume Next

        ActiveForm.MenuClickCopy
      }

      void mnuECut_Click()

        On Error Resume Next

        ActiveForm.MenuClickCut
      }


      void mnuEDelete_Click()

        On Error Resume Next

        ActiveForm.MenuClickDelete
      }

      void mnuEFind_Click()

        On Error Resume Next

        ActiveForm.MenuClickFind
      }

      void mnuEFindAgain_Click()

        On Error Resume Next

        ActiveForm.MenuClickFindAgain
      }

      void mnuEInsert_Click()

        On Error Resume Next

        ActiveForm.MenuClickInsert
      }

      void mnuEPaste_Click()

        On Error Resume Next

        ActiveForm.MenuClickPaste
      }

      void mnuESelectAll_Click()

        On Error Resume Next

        ActiveForm.MenuClickSelectAll
      }

      void mnuEUndo_Click()
        //undo an action

        On Error Resume Next

        ActiveForm.MenuClickUndo
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

        Dim i As Long

        On Error GoTo ErrHandler

        //if no form is active
        if (ActiveForm = null) {
          //can only mean that Settings.ShowPreview is false,
          //AND Settings.ResListType is non-zero, AND no editor window are open
          //use selected item method (remember to adjust selprop by 1)
          i = SelectedProp - 1
          // validate; (if nothing selected, default to first property)
          if (i < 1 && i > 3) {
            i = 1
          }
          SelectedItemDescription i
        } else {
          //if active form is NOT the preview form
          //if any form other than preview is active
          if (ActiveForm.Name != "frmPreview") {
            //use the active form method (always default to editing ID when selected via menu)
            ActiveForm.MenuClickDescription 1
          } else {

            //check for an open editor that matches resource being previewed
            switch (SelResType
            case rtLogic
              //if any logic editor matches this resource
              For i = 1 To LogicEditors.Count
                if (LogicEditors(i).FormMode = fmLogic) {
                  if (LogicEditors(i).LogicNumber = SelResNum) {
                    //use this form//s method
                    LogicEditors(i).MenuClickDescription SelectedProp
                    return;
                  }
                }
              Next i

            case rtPicture
              //if any Picture editor matches this resource
              For i = 1 To PictureEditors.Count
                if (PictureEditors(i).PicNumber = SelResNum) {
                  //use this form//s method
                  PictureEditors(i).MenuClickDescription SelectedProp
                  return;
                }
              Next i

            case rtSound
              //if any Sound editor matches this resource
              For i = 1 To SoundEditors.Count
                if (SoundEditors(i).SoundNumber = SelResNum) {
                  //use this form//s method
                  SoundEditors(i).MenuClickDescription SelectedProp
                  return;
                }
              Next i

            case rtView
              //if any View editor matches this resource
              For i = 1 To ViewEditors.Count
                if (ViewEditors(i).ViewNumber = SelResNum) {
                  //use this form//s method
                  ViewEditors(i).MenuClickDescription SelectedProp
                  return;
                }
              Next i

            case rtWords
              //if using editor
              if (WEInUse) {
                //use word editor
                WordEditor.MenuClickDescription 2 //only desc can be edited for words.tok
                return;
              }

            case rtObjects
              //if using editor
              if (OEInUse) {
                //use Objects Editor
                ObjectEditor.MenuClickDescription 2 //only desc can be edited for words.tok
                return;
              }

            default: //text, game or none
              //export does not apply

            }

            //if no open editor is found, use the selected item method
            switch (SelResType
            case rtObjects, rtWords
              //always select description
              SelectedItemDescription 2
            case rtLogic, rtPicture, rtSound, rtView
              //if id or desc selected, use it, otherwise default to id
              if (SelectedProp = 2 && SelectedProp = 3) {
                //(remember to adjust selprop by 1)
                SelectedItemDescription SelectedProp - 1
              } else {
                SelectedItemDescription 1
              }
            default:
              //shouldnt get here?
              //Debug.Assert false
            }
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuGRebuild_Click()

        //rebuild volfiles only
        CompileAGIGame GameDir, true
      }

      void mnuGRun_Click()

        Dim rtn As Long, strParams As String
        Dim strErrTitle As String, strErrMsg As String, strErrType As String

        On Error GoTo ErrHandler

        //first check for missing platform
        if (PlatformType = 0) {
          //notify user and show property dialog
          MsgBoxEx "You need to select a platform on which to run your game first.", vbOKOnly + vbInformation + vbMsgBoxHelpButton, "No Platform Selected", WinAGIHelp, "htm\winagi\Properties.htm#platform"
          mnuGProperties_Click
          //if still no platform
          if (PlatformType = 0) {
            //just exit
            return;
          }
        }

        //load selected platform
        switch (PlatformType
        case 1 //DosBox
          //verify target exists?
          if (Len(Dir(GameDir + DosExec)) = 0) {
            MsgBoxEx "The DOS executable file //" + DosExec + "// is missing from the" + vbNewLine + "game directory. Aborting DosBox session.", vbCritical + vbOKOnly + vbMsgBoxHelpButton, "Missing DOS Executable File", WinAGIHelp, "htm\winagi\Properties.htm#platform"
            return;
          }
          //dosbox parameters: dosbox gamedir+agi.exe -noautoexec
          // (need -noautoexec option as mandatory setting to avoid virtual C-drive assignment issues)
          strParams = Chr$(34) + GameDir + DosExec + Chr$(34) + " -noautoexec " + PlatformOpts

        case 2 //ScummVM
          //scummvm parameters: --path=gamedir agi
          strParams = "--path=""" + JustPath(GameDir, true) + """ agi " + PlatformOpts

        case 3 //NAGI
          //no parameters for nagi; just run the program
          strParams = ""

        case 4 //other
          //run with whatever is in Platform and PlatformOpts
          strParams = PlatformOpts

        }

        //check if any logics are dirty;
        if (CheckLogics()) {
          //run the program if check is OK
          rtn = ShellExecute(this.hWnd, "open", Platform, strParams, JustPath(Platform, true), SW_SHOWNORMAL)
          if (rtn <= 32) {

            switch (rtn
            //regular WinExec() codes
            case 2 //#define SE_ERR_FNF              2       // file not found
              strErrType = "(File !Found)"
            case 3 //#define SE_ERR_PNF              3       // path not found
              strErrType = "(Path !Found)"
            case 5 //#define SE_ERR_ACCESSDENIED     5       // access denied
              strErrType = "(Access Denied)"
            case 8 //#define SE_ERR_OOM              8       // out of memory
              strErrType = "(Out of Memory)"
            case 32 //#define SE_ERR_DLLNOTFOUND              32
              strErrType = "(DLL !Found)"
            //
            //error values for ShellExecute() beyond the regular WinExec() codes
            case 26 //#define SE_ERR_SHARE                    26
              strErrType = "(Share Violation)"
            case 27 //#define SE_ERR_ASSOCINCOMPLETE          27
              strErrType = "(Incomplete Association)"
            case 28 //#define SE_ERR_DDETIMEOUT               28
              strErrType = "(DDE Timeout Error)"
            case 29 //#define SE_ERR_DDEFAIL                  29
              strErrType = "(DDE Failure)"
            case 30 //#define SE_ERR_DDEBUSY                  30
              strErrType = "(DDE Busy Error)"
            case 31 //#define SE_ERR_NOASSOC                  31
              strErrType = "(Association !Found)"
            default:
              strErrType = "(Unknown Error)"
            }

            //message also depends on platform type
            switch (PlatformType
            case 1 //DosBox
              strErrTitle = "DosBox Error"
              strErrMsg = "DosBox"

            case 2 //ScummVM
              strErrTitle = "ScummVM Error"
              strErrMsg = "ScummVM"

            case 3 //NAGI
              strErrTitle = "NAGI Error"
              strErrMsg = "NAGI"

            case 4 //other
              strErrTitle = "Run AGI Game Error"
              strErrMsg = "this program"
            }

            strErrMsg = "Unable to run " + strErrMsg + " " + strErrType + ". Make sure you" + vbNewLine + _
                        "have selected the correct executable file, and that any parameters" + vbNewLine + _
                        "you included are correct."

            MsgBoxEx strErrMsg, vbOKOnly + vbCritical + vbMsgBoxHelpButton, strErrTitle, WinAGIHelp, "htm\winagi\Properties.htm#platform"
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      void mnuRCustom1_Click()

        On Error Resume Next

        ActiveForm.MenuClickCustom1
      }

      void mnuRCustom2_Click()

        On Error Resume Next

        ActiveForm.MenuClickCustom2
      }


      void mnuRCustom3_Click()

        On Error Resume Next

        ActiveForm.MenuClickCustom3
      }


      void mnuRExport_Click()

        Dim i As Long

        On Error GoTo ErrHandler

        //if no form is active
        if (ActiveForm = null) {
          //can only mean that Settings.ShowPreview is false,
          //AND Settings.ResListType is non-zero, AND no editor window are open
          //use selected item method
          SelectedItemExport
        } else {
          //if active form is preview window OR the mdi form is active
          if (ActiveForm.Name != "frmPreview") {
            //use the active form method
            ActiveForm.MenuClickExport
          } else {
            //check for an open editor that matches resource being previewed
            switch (SelResType
            case rtLogic
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

            case rtPicture
              //if any Picture editor matches this resource
              For i = 1 To PictureEditors.Count
                if (PictureEditors(i).PicNumber = SelResNum) {
                  //use this form//s method
                  PictureEditors(i).MenuClickExport
                  return;
                }
              Next i

            case rtSound
              //if any Sound editor matches this resource
              For i = 1 To SoundEditors.Count
                if (SoundEditors(i).SoundNumber = SelResNum) {
                  //use this form//s method
                  SoundEditors(i).MenuClickExport
                  return;
                }
              Next i

            case rtView
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

        if (!ActiveForm = null) {
          //if a logic editor is currently open AND it is in a game
          if (ActiveForm.Name = "frmLogicEdit") {
            if (ActiveForm.InGame) {
              //ask user if this is supposed to replace the existing logic
              if (MessageBox.Show(("Do you want to replace the logic you are currently editing?", vbYesNo, "Import Logic") = vbYes) {
                //use the active form//s import function
                ActiveForm.MenuClickImport
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
              ObjectEditor = ActiveForm

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

        if (!ActiveForm = null) {
          //if a picture editor is currently open AND it is in a game
          if (ActiveForm.Name = "frmPictureEdit") {
            if (ActiveForm.InGame) {
              //ask user if this is supposed to replace existing picture
              if (MessageBox.Show(("Do you want to replace the picture you are currently editing?", vbYesNo, "Import Picture") = vbYes) {
                //use the active form//s import function
                ActiveForm.MenuClickImport
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

        if (!ActiveForm = null) {
          //if a sound editor is currently open AND it is in a game
          if (ActiveForm.Name = "frmSoundEdit") {
            if (ActiveForm.InGame) {
              //ask user if this is supposed to replace existing sound
              if (MessageBox.Show(("Do you want to replace the sound you are currently editing?", vbYesNo, "Import Sound") = vbYes) {
                //use the active form//s import function
                ActiveForm.MenuClickImport
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

        if (!ActiveForm = null) {
          //if a view editor is currently open AND it is in a game
          if (ActiveForm.Name = "frmViewEdit") {
            if (ActiveForm.InGame) {
              //ask user if this is supposed to replace existing view
              if (MessageBox.Show(("Do you want to replace the view you are currently editing?", vbYesNo, "Import View") = vbYes) {
                //use the active form//s import function
                ActiveForm.MenuClickImport
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
              WordEditor = ActiveForm

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
            ObjectEditor = ActiveForm

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
            WordEditor = ActiveForm

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
            .ResType = rtLogic
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
            .ResType = rtPicture
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
            .ResType = rtSound
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
            .ResType = rtView
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

      void mnuRPrint_Click()

        Dim i As Long

        On Error GoTo ErrHandler

        //if no form is active
        if (ActiveForm = null) {
          //can only mean that Settings.ShowPreview is false,
          //AND Settings.ResListType is non-zero, AND no editor window are open
          //use selected item method
          SelectedItemPrint
        } else {
          //if active form is NOT the preview form
          //if any form other than preview is active
          if (ActiveForm.Name != "frmPreview") {
            //use the active form method
            ActiveForm.MenuClickPrint
          } else {
            //use generic print method
            //to check if resource is open
            PrintResources
          }
        }
      return;

      ErrHandler:
        //Debug.Assert false
        Resume Next
      }

      public void mnuRRenumber_Click()

        Dim i As Long

        On Error GoTo ErrHandler

        //if no form is active
        if (ActiveForm = null) {
          //can only mean that Settings.ShowPreview is false,
          //AND Settings.ResListType is non-zero, AND no editor window are open
          //use selected item method
          SelectedItemRenumber
        } else {
          //if active form is NOT the preview form
          //if any form other than preview is active
          if (ActiveForm.Name != "frmPreview") {
            //use the active form method
            ActiveForm.MenuClickRenumber
          } else {
        //Debug.Assert SelResNum >= 0 && SelResNum <= 255
            //check for an open editor that matches resource being previewed
            switch (SelResType
            case rtLogic
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

            case rtPicture
              //if any Picture editor matches this resource
              For i = 1 To PictureEditors.Count
                if (PictureEditors(i).PicNumber = SelResNum && PictureEditors(i).InGame) {
                  //use this form//s method
                  PictureEditors(i).MenuClickRenumber
                  return;
                }
              Next i

            case rtSound
              //if any Sound editor matches this resource
              For i = 1 To SoundEditors.Count
                if (SoundEditors(i).SoundNumber = SelResNum && SoundEditors(i).InGame) {
                  //use this form//s method
                  SoundEditors(i).MenuClickRenumber
                  return;
                }
              Next i

            case rtView
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

        ActiveForm.MenuClickSave
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

      void mnuGNewBlank_Click()

        //create new blank game
        NewAGIGame false
      }

      void mnuGNTemplate_Click()

        //create new game using template
        NewAGIGame true
      }







      void mnuWiClose_Click()

        //make sure this is not preview window
        if (MDIMain.ActiveForm.Name != "frmPreview") {
          Unload ActiveForm
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

        case "print"
          mnuRPrint_Click

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
          if (!ActiveForm = null) {
            On Error Resume Next
            ActiveForm.MenuClickHelp
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
      public void PropMouseWheel(ByVal MouseKeys As Long, ByVal Rotation As Long, ByVal xPos As Long, ByVal yPos As Long)

        On Error Resume Next

        //validate cursor is over window
        if (xPos < 0 && yPos < 0 && xPos >= picProperties.Width && yPos >= picProperties.Height) {
          return;
        }

        //if property window doesn't have focus,
        //make it so
        if (!MDIMain.ActiveControl Is picProperties) {
          picProperties.Focus();
        }

        //if nothing selected
        if (SelectedProp = 0) {
          //select first item
          SelectedProp = 1
          PaintPropertyWindow
          return;
        }

        //can the prop window scroll?
        if (Rotation > 0) {
          //same as up arrow
          //decrement selected prop
          if (SelectedProp > 1) {
            SelectedProp = SelectedProp - 1
            //adjust scroll if necessary
            if (SelectedProp - PropScroll = 0) {
              //scroll up
              fsbProperty.Value = fsbProperty.Value - 1
            }
            //repaint property window
            PaintPropertyWindow
          }

        } else {
          //same as down arrow
          //increment selected prop
          if (SelectedProp < PropRows) {
            SelectedProp = SelectedProp + 1
            //adjust scroll if necessary
            if (SelectedProp - PropScroll = PropRowCount + 1) {
              //scroll up
              fsbProperty.Value = fsbProperty.Value + 1
            }
            //repaint property window
            PaintPropertyWindow
          }
        }
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
            //Debug.Print xPos, .Left + .Width
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
            //reset the navigation queue
            ResetQueue();
            //don't add to queue while clearing
            DontQueue = true;
            //list type determines clear actions
            switch (Settings.ResListType) {
            case 0: //none
                break;
            case 1: //tree
                if (tvwResources.Nodes.Count > 0) {
                    //always collapse first
                    tvwResources.Nodes[0].Collapse();
                    //clear the treelist
                    tvwResources.Nodes.Clear();
                }
                //add the base nodes
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
                ////select resource root
                ////deselect property
                //SelectedProp = 1;
                //PaintPropertyWindow();
                break;
            case 2: //combo/list box
                if (EditGame.GameLoaded) {
                    cmbResType.Items[0] = EditGame.GameID;
                }
                else {
                    cmbResType.Items[0] = "AGIGame";
                }
                //select top item (game level)
                cmbResType.SelectedIndex = 0;
                ////select resource root
                ////deselect property
                //SelectedProp = 1;
                //PaintPropertyWindow();
                break;
            }
            //allow queuing
            DontQueue = false;
        }
        public void ShowResTree() {

            // need to make sure resource list (tree or listbox)
            // is set to proper height based on current number of
            // rows in the properties window (including 
            //pnlProp.Height = PropRowHeight * (PropRowCount + 1) + 2;
            //pnlProp.Top = pnlResources.Height - pnlProp.Height;
            //int lngSplitLoc = pnlResources.Height - pnlProp.Height;
            //if (lngSplitLoc < tvwResources.Top) {
            //  lngSplitLoc = tvwResources.Top + 1;
            //}

            switch (Settings.ResListType) {
            case 0: //no tree
                    //shouldn't get here, but
                return;

            case 1: //treeview list
                tvwResources.Visible = true;
                cmbResType.Visible = false;
                lstResources.Visible = false;
                //set tree height
                //tvwResources.Height = lngSplitLoc - tvwResources.Top;
                //change font to match current preview font
                tvwResources.Font = new Font(Settings.PFontName, Settings.PFontSize);
                break;
            case 2: //combo/list boxes
                tvwResources.Visible = false;
                //set combo and listbox height, and set fonts
                cmbResType.Visible = true;
                cmbResType.Font = new Font(Settings.EFontName, Settings.EFontSize);
                lstResources.Top = cmbResType.Top + cmbResType.Height + 2;
                //lstResources.Height = lngSplitLoc - lstResources.Top;
                lstResources.Visible = true;
                lstResources.Font = new Font(Settings.PFontName, Settings.PFontSize);
                break;
            }
            //show and position the resource list panels
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
                blnLastLoad = EditGame.GameLoaded;
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
            GameSettings.WriteSetting(sMRULIST, "LastLoad", blnLastLoad);
            //save settings to register
            SaveSettings();

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
            Settings.ResListType++;
            if (Settings.ResListType == 3) {
                Settings.ResListType = 0;
            };
            // force refresh of type
            switch (Settings.ResListType) {
            case 0:
                // no list
                if (splResource.Visible) {
                    HideResTree();
                }
                break;
            case 1 or 2:
                // treelist
                if (!splResource.Visible) {
                    ShowResTree();
                }
                else {
                    //if (Settings.ResListType != oldResListType) {
                    // reset to root, then switch
                    SelResType = rtGame;
                    SelResNum = -1;
                    BuildResourceTree();
                    ShowResTree();
                    //}
                }
                break;
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
    }
}
