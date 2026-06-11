using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FastColoredTextBoxNS;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Editor.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Base;

namespace WinAGI.Editor {
    public partial class frmLogicEdit : Form {

        #region Structs
        struct MessageData {
            public string Text;
            public bool Declared;
            public bool ByNumber;
            public bool ByRef;
            public int Type; // 0=string, 1=marker, 2=define
            public int Line;
            public int Concat;
        }

        struct ReadMsgResult {
            public int Error;   // 0 = ok, no error
                                // 1 = invalid msg number
                                // 2 = duplicate msg number
                                // 3 = not a string
                                // 4 = stuff not allowed after msg declaration
                                // 5 = not valid string <1 char, or no closing quote

            public int Line;
            public int Pos;
            public int MsgNum;
        }
        #endregion

        #region Fields
        public int LogicNumber;
        public int IncludeIndex;
        public Logic EditLogic;
        public bool Compiled = false;
        internal readonly LogicFormMode FormMode;
        public bool InGame = false;
        public bool IsChanged = false;
        public string TextFilename = "";
        private bool loading;
        private bool saving = false;

        public WinAGIFCTB fctb;
        private bool SelectEntireLine = false;
        List<string> Includes = [];
        List<Define> LDefLookup = [];
        public bool DefChanged = true;
        // DefChanged means text has changed, so the lookup list needs
        // to be rebuilt;
        public bool ListChanged = true;
        // ListChanged means the ShowDefinesList needs to be rebuilt
        ArgListType ListType;
        // tracks what is currently being included in the list

        // to manage the defines list feature
        Place DefStartPos;
        Place DefEndPos;
        string PrevText;
        string DefText;

        // tool tip variables
        AGIToken TipCmdToken;
        int TipCurArg = 0;
        int SnipIndent = 0;

        // StatusStrip Items
        internal ToolStripStatusLabel spLine;
        internal ToolStripStatusLabel spColumn;
        internal ToolStripStatusLabel spStatus;
        internal ToolStripStatusLabel spCapsLock;
        internal ToolStripStatusLabel spNumLock;
        internal ToolStripStatusLabel spInsLock;

        // warning/error list
        internal List<WinAGIEventInfo> WarningList = [];
        private int chgStart = -1;
        private int chgLength = -1;
        private int chgCount = 0;
        #endregion

        #region Constructors
        public frmLogicEdit(LogicFormMode mode) {
            InitializeComponent();
            // set service colors for collapse/expand markers
            // (needed here because designer uses BinaryFormatter, which is bad)
            // same BinaryFormatter issue with imgage list items...
            imageList1.Images.Clear();
            imageList1.Images.Add("def_lnum", ByteArrayToImage(EditorResources.def_lnum));
            imageList1.Images.Add("def_lvar", ByteArrayToImage(EditorResources.def_lvar));
            imageList1.Images.Add("def_lflag", ByteArrayToImage(EditorResources.def_lflag));
            imageList1.Images.Add("def_lmsg", ByteArrayToImage(EditorResources.def_lmsg));
            imageList1.Images.Add("def_lsobj", ByteArrayToImage(EditorResources.def_lsobj));
            imageList1.Images.Add("def_liobj", ByteArrayToImage(EditorResources.def_liobj));
            imageList1.Images.Add("def_lstr", ByteArrayToImage(EditorResources.def_lstr));
            imageList1.Images.Add("def_lword", ByteArrayToImage(EditorResources.def_lword));
            imageList1.Images.Add("def_lctrl", ByteArrayToImage(EditorResources.def_lctrl));
            imageList1.Images.Add("def_ldefstr", ByteArrayToImage(EditorResources.def_ldefstr));
            imageList1.Images.Add("def_lvocwrd", ByteArrayToImage(EditorResources.def_lword));
            imageList1.Images.Add("def_gnum", ByteArrayToImage(EditorResources.def_gnum));
            imageList1.Images.Add("def_gvar", ByteArrayToImage(EditorResources.def_gvar));
            imageList1.Images.Add("def_gflag", ByteArrayToImage(EditorResources.def_gflag));
            imageList1.Images.Add("def_gmsg", ByteArrayToImage(EditorResources.def_gmsg));
            imageList1.Images.Add("def_gsobj", ByteArrayToImage(EditorResources.def_gsobj));
            imageList1.Images.Add("def_giobj", ByteArrayToImage(EditorResources.def_giobj));
            imageList1.Images.Add("def_gstr", ByteArrayToImage(EditorResources.def_gstr));
            imageList1.Images.Add("def_gword", ByteArrayToImage(EditorResources.def_gword));
            imageList1.Images.Add("def_gctrl", ByteArrayToImage(EditorResources.def_gctrl));
            imageList1.Images.Add("def_gdefstr", ByteArrayToImage(EditorResources.def_gdefstr));
            imageList1.Images.Add("def_gvocwrd", ByteArrayToImage(EditorResources.def_gvocwrd));
            imageList1.Images.Add("def_glogic", ByteArrayToImage(EditorResources.def_glogic));
            imageList1.Images.Add("def_gpicture", ByteArrayToImage(EditorResources.def_gpicture));
            imageList1.Images.Add("def_gsound", ByteArrayToImage(EditorResources.def_gsound));
            imageList1.Images.Add("def_gview", ByteArrayToImage(EditorResources.def_gview));
            imageList1.Images.Add("def_rnum", ByteArrayToImage(EditorResources.def_rnum));
            imageList1.Images.Add("def_rvar", ByteArrayToImage(EditorResources.def_rvar));
            imageList1.Images.Add("def_rflag", ByteArrayToImage(EditorResources.def_rflag));
            imageList1.Images.Add("def_rmsg", ByteArrayToImage(EditorResources.def_rmsg));
            imageList1.Images.Add("def_rsobj", ByteArrayToImage(EditorResources.def_rsobj));
            imageList1.Images.Add("def_riobj", ByteArrayToImage(EditorResources.def_riobj));
            imageList1.Images.Add("def_rstr", ByteArrayToImage(EditorResources.def_rstr));
            imageList1.Images.Add("def_rword", ByteArrayToImage(EditorResources.def_rword));
            imageList1.Images.Add("def_rctrl", ByteArrayToImage(EditorResources.def_rctrl));
            imageList1.Images.Add("def_rdefstr", ByteArrayToImage(EditorResources.def_rdefstr));
            imageList1.Images.Add("def_rvocwrd", ByteArrayToImage(EditorResources.def_rvocwrd));
            static Image ByteArrayToImage(byte[] bytes) {
                using MemoryStream ms = new(bytes);
                return Image.FromStream(ms);
            }
            InitStatusStrip();
            FormMode = mode;
            rtfLogic1.Select();
            fctb = rtfLogic1;
            splitLogic.Panel1.TabIndex = 1;
            splitLogic.Panel2.TabIndex = 0;
            rtfLogic1.Controls.Add(picTip);
            splitLogic.SplitterDistance = 0;
            MdiParent = MDIMain;
            rtfLogic1.LeftBracket = '(';
            rtfLogic1.RightBracket = ')';
            rtfLogic1.LeftBracket2 = '{';
            rtfLogic1.RightBracket2 = '}';
            rtfLogic2.LeftBracket = '(';
            rtfLogic2.RightBracket = ')';
            rtfLogic2.LeftBracket2 = '{';
            rtfLogic2.RightBracket2 = '}';
            InitFonts();
            splitContainer1.SplitterDistance = splitContainer1.Width - 80;
            if (!WinAGISettings.ShowDocMap.Value) {
                splitContainer1.Panel2Collapsed = true;
            }
            rtfLogic1.ShowLineNumbers = WinAGISettings.ShowLineNumbers.Value;
            rtfLogic2.ShowLineNumbers = WinAGISettings.ShowLineNumbers.Value;
            rtfLogic1.IsReplaceMode = IsKeyLocked(Keys.Insert);
            rtfLogic2.IsReplaceMode = IsKeyLocked(Keys.Insert);

            loading = true;
            switch (mode) {
            case LogicFormMode.Logic:
                EditLogic = new();
                break;
            case LogicFormMode.Text:
                // no compiling or msg cleanup
                btnCompile.Enabled = false;
                btnMsgClean.Enabled = false;
                break;
            }
            // list boxes don't actually apply the sort until they have been 
            // displayed at least one time, so we move it off screen and show
            // at at the start; when needed, it will be moved to correct
            // location
            lstDefines.Left = -9999;
            lstDefines.Visible = true;
        }
        #endregion

        #region Event Handlers
        #region Form Events
        private void frmLogicEdit_Activated(object sender, EventArgs e) {
            if (FindingForm.Visible &&
                FindingForm.FormFunction != FindFormFunction.FindWordsLogic &&
                FindingForm.FormFunction != FindFormFunction.FindObjsLogic) {
                if (FindingForm.rtfReplace.Visible) {
                    FindingForm.SetForm(FindFormFunction.ReplaceLogic, InGame);
                }
                else {
                    FindingForm.SetForm(FindFormFunction.FindLogic, InGame);
                }
            }
            if (MDIMain.infoGridScope == InfoGridScope.SelectedResource) {
                MDIMain.RefreshInfoGrid();
            }
            // track INS state
            rtfLogic1.IsReplaceMode = rtfLogic2.IsReplaceMode = frmMDIMain.Overwrite;
        }

        private void frmLogicEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            e.Cancel = !AskClose();
        }

        private void frmLogicEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // dereference object
            EditLogic?.Unload();
            EditLogic = null;
            if (FormMode == LogicFormMode.Logic && InGame) {
                if (EditGame.Logics[LogicNumber].Loaded) {
                    EditGame.Logics[LogicNumber].Unload();
                }
            }
            if (InGame) {
                // form stays in MDIChild collection until AFTER
                // FormClosed is complete; to avoid problems with 
                // warnings filter, need to set InGame to false BEFORE 
                // refreshing filters
                InGame = false;
                if (MDIMain.infoGridScope == InfoGridScope.OpenResources) {
                    MDIMain.RefreshInfoGrid();
                }
            }
            // remove from logic editor collection
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm == this) {
                    LogicEditors.Remove(frm);
                    frmFind.ResetSearch();
                    break;
                }
            }
        }

        private void frmLogicEdit_HelpRequested(object sender, HelpEventArgs hlpevent) {
            ShowHelp();
            hlpevent.Handled = true;
        }
        #endregion

        #region Resource Menu Events
        /// <summary>
        /// Configures the resource menu prior to displaying it.
        /// </summary>
        internal void SetResourceMenu() {
            mnuRSave.Enabled = fctb.IsChanged;
            if (EditGame is null) {
                // no game is open
                MDIMain.mnuRImport.Enabled = false;
                mnuRSave.Text = FormMode == LogicFormMode.Logic ? "Save Logic" : "Save Text File";
                mnuRExport.Text = "Save As ...";
                if (FormMode == LogicFormMode.Logic) {
                    mnuRInGame.Visible = true;
                    mnuRInGame.Enabled = false;
                    mnuRInGame.Text = "Add Logic to Game";
                    mnuRRenumber.Visible = true;
                    mnuRRenumber.Enabled = false;
                    mnuRRenumber.Text = "Renumber Logic";
                    mnuRProperties.Visible = true;
                    MDIMain.mnuRSep2.Visible = true;
                    MDIMain.mnuRSep3.Visible = true;
                    mnuRCompile.Visible = true;
                    mnuRCompile.Enabled = false;
                    mnuRCompile.Text = "Compile This Logic";
                    mnuRMsgCleanup.Visible = true;
                    mnuRIsRoom.Visible = true;
                    mnuRIsRoom.Enabled = false;
                    mnuRIsRoom.Checked = false;
                }
                else {
                    mnuRInGame.Visible = false;
                    mnuRRenumber.Visible = false;
                    mnuRProperties.Visible = false;
                    MDIMain.mnuRSep2.Visible = false;
                    MDIMain.mnuRSep3.Visible = false;
                    mnuRCompile.Visible = false;
                    mnuRMsgCleanup.Visible = false;
                    mnuRIsRoom.Visible = false;
                }
            }
            else {
                // if a game is loaded, base import is also always available
                MDIMain.mnuRImport.Enabled = true;
                mnuRSave.Text = FormMode == LogicFormMode.Logic ? "Save Logic" : "Save Text File";
                if (FormMode == LogicFormMode.Logic && InGame) {
                    mnuRExport.Text = "Export Logic";
                }
                else {
                    mnuRExport.Text = "Save As ...";
                }
                if (FormMode == LogicFormMode.Logic) {
                    mnuRInGame.Visible = true;
                    mnuRInGame.Enabled = EditGame is not null && EditLogic.SourceFile.Length > 0;
                    mnuRInGame.Text = InGame ? "Remove from Game" : "Add to Game";
                    mnuRRenumber.Visible = true;
                    mnuRRenumber.Enabled = InGame;
                    mnuRRenumber.Text = "Renumber Logic";
                    mnuRProperties.Visible = true;
                    MDIMain.mnuRSep2.Visible = true;
                    MDIMain.mnuRSep3.Visible = true;
                    mnuRCompile.Visible = true;
                    mnuRCompile.Enabled = InGame && !EditLogic.Compiled;
                    mnuRCompile.Text = "Compile This Logic";
                    mnuRMsgCleanup.Visible = true;
                    mnuRIsRoom.Visible = true;
                    mnuRIsRoom.Enabled = LogicNumber != 0 && InGame;
                    mnuRIsRoom.Checked = EditLogic.IsRoom;
                }
                else {
                    mnuRInGame.Visible = true;
                    mnuRInGame.Enabled = TextFilename.Length > 0;
                    mnuRInGame.Text = InGame ? "Remove from Game" : "Add to Game";
                    mnuRRenumber.Visible = false;
                    mnuRProperties.Visible = false;
                    MDIMain.mnuRSep2.Visible = false;
                    MDIMain.mnuRSep3.Visible = false;
                    mnuRCompile.Visible = false;
                    mnuRMsgCleanup.Visible = false;
                    mnuRIsRoom.Visible = false;
                }
            }
        }

        /// <summary>
        /// Resets all resource menu items so shortcut keys can work correctly.
        /// </summary>
        internal void ResetResourceMenu() {
            mnuRSave.Enabled = true;
            mnuRExport.Enabled = true;
            mnuRInGame.Enabled = true;
            mnuRRenumber.Enabled = true;
            mnuRProperties.Enabled = true;
            mnuRCompile.Enabled = true;
            mnuRMsgCleanup.Enabled = true;
            mnuRIsRoom.Enabled = true;
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            if (fctb.IsChanged) {
                if (FormMode == LogicFormMode.Logic) {
                    SaveLogicSource();
                }
                else {
                    SaveTextFile(TextFilename);
                }
            }
        }

        internal void mnuRExport_Click(object sender, EventArgs e) {
            if (FormMode == LogicFormMode.Logic) {
                ExportLogic();
            }
            else {
                SaveTextFile();
            }
        }

        internal void mnuRInGame_Click(object sender, EventArgs e) {
            if (EditGame is not null) {
                ToggleInGame();
            }
        }

        private void mnuRRenumber_Click(object sender, EventArgs e) {
            if (FormMode == LogicFormMode.Logic && InGame) {
                RenumberLogic();
            }
        }

        private void mnuRProperties_Click(object sender, EventArgs e) {
            if (FormMode == LogicFormMode.Logic && InGame) {
                EditLogicProperties(1);
            }
        }

        private void mnuRCompile_Click(object sender, EventArgs e) {
            if (FormMode == LogicFormMode.Logic && InGame && !EditLogic.Compiled) {
                Compiled = CompileLogic(this, (byte)LogicNumber);
            }
        }

        private void mnuRSaveAll_Click(object sender, EventArgs e) {
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm.FormMode == LogicFormMode.Logic) {
                    if (frm.IsChanged) {
                        frm.SaveLogicSource();
                    }
                }
            }
        }

        private void mnuRMsgCleanup_Click(object sender, EventArgs e) {
            if (FormMode == LogicFormMode.Logic) {
                MessageCleanup();
            }
        }

        private void mnuRIsRoom_Click(object sender, EventArgs e) {
            if (FormMode == LogicFormMode.Logic && InGame && LogicNumber != 0) {
                EditLogic.IsRoom = !EditLogic.IsRoom;
                MDIMain.RefreshPropertyGrid(AGIResType.Logic, LogicNumber);
                MarkAsChanged();
                UpdateExitInfo(EditLogic.IsRoom ? UpdateReason.ShowRoom : UpdateReason.HideRoom, LogicNumber, EditLogic);
            }
        }
        #endregion

        #region Edit Menu Events
        private void SetEditMenu() {
            // Undo (Ctrl+Z) V; E:if able (is type available?)
            // Redo (Ctrl+Y) V; E:if able (is type available?)
            // ----------- V
            // Cut (Ctrl+X) V; E
            // Copy (Ctrl+C) V; E
            // Paste (Ctrl+V) V; E
            // Delete (Del) V; E
            // Select All (Ctrl+A) V; E
            // ----------- V
            // Find (Ctrl+F)-> V; E
            // Find Next (F3) V; E:search in progress
            // Replace (Ctrl+H) V; E
            // ----------- V
            // Insert Snippet (Ctrl+Shift+T) V:UseSnippets && sellength==0; E
            // Create Snippet (Ctrl+Shift+T) V:UseSnippets && sellength>0; E
            // List Defines (Ctrl+J) V:not sierrasyntax; E
            // List Commands (Shift+Ctrl+J) V:not sierrasyntax; E
            // Block Comment (Alt+B) V; E
            // Unblock Comment (Alt+U) V; E
            // ----------- V
            // Open xx for Editing (Shift+Ctrl+E) V:ingame && on resource token; E
            // View Synonyms for xxx () V:on vocab word; E
            // Toggle Document Map (Ctrl+Shift+M) V; E
            // Character Map (Ctrl+Ins) V; E;
            mnuEUndo.Enabled = fctb.UndoEnabled;
            mnuERedo.Enabled = fctb.RedoEnabled;
            mnuECut.Enabled = mnuECopy.Enabled = mnuEDelete.Enabled = fctb.SelectionLength > 0;
            mnuEPaste.Enabled = Clipboard.ContainsText();
            mnuESelectAll.Enabled = fctb.TextLength > 0;
            mnuESnippet.Visible = WinAGISettings.UseSnippets.Value;
            mnuESnippet.Text = fctb.Selection.Length > 0 ? "Create Code Snippet..." : "Insert Code Snippet";
            mnuEFindAgain.Enabled = GFindText.Length > 0;
            mnuEListCommands.Visible = InGame && !EditGame.SierraSyntax;
            mnuEListDefines.Visible = InGame;
            // default to not visible
            mnuESep2a.Visible = mnuEOpenRes.Visible = false;
            mnuEViewSynonym.Visible = false;
            if (InGame) {
                AGIToken seltoken = fctb.TokenFromPos();
                switch (seltoken.Type) {
                case AGITokenType.Identifier:
                    // look for resourceid or define
                    for (int restype = 0; restype < 4; restype++) {
                        for (int num = 0; num < 256; num++) {
                            if (IDefLookup[restype, num].Type != ArgType.None) {
                                if (seltoken.Text == IDefLookup[restype, num].Name) {
                                    mnuEOpenRes.Text = "Open " + seltoken.Text + " for Editing";
                                    Point taginfo = new() {
                                        X = restype,
                                        Y = num
                                    };
                                    mnuEOpenRes.Tag = taginfo;
                                    mnuESep2a.Visible = mnuEOpenRes.Visible = true;
                                    mnuEOpenRes.Enabled = true;
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case AGITokenType.String:
                    if (fctb.PreviousToken(seltoken).Text == "#include" ||
                        (EditGame is not null && EditGame.SierraSyntax &&
                        fctb.PreviousToken(seltoken).Text == "%include")) {
                        if (seltoken.Text[^1] != '"') {
                            seltoken.Text += '"';
                        }
                        mnuEOpenRes.Text = "Open " + seltoken.Text + " for Editing";
                        Point taginfo = new() {
                            X = 4,
                            Y = 0
                        };
                        mnuEOpenRes.Tag = taginfo;
                        mnuESep2a.Visible = mnuEOpenRes.Visible = true;
                        // resoureids.txt is not editable
                        mnuEOpenRes.Enabled = (!seltoken.Text.Equals("\"resourceids.txt\"", StringComparison.OrdinalIgnoreCase));
                        break;
                    }
                    // not an include, check for 'said' word
                    if (EditGame is not null && !EditGame.SierraSyntax) {
                        int ac = 0;
                        AGIToken cmdtoken = FindPrevCmd(fctb, seltoken, ref ac);
                        if (cmdtoken.Type == AGITokenType.Identifier && cmdtoken.Text == "said") {
                            string wordtext = seltoken.Text[1..^1];
                            if (EditGame.WordList.WordExists(wordtext)) {
                                mnuEViewSynonym.Visible = true;
                                mnuEViewSynonym.Text = "View Synonyms for " + seltoken.Text;
                                int group = EditGame.WordList[wordtext].Group;
                                mnuEViewSynonym.Tag = group;
                                mnuEViewSynonym.Enabled = EditGame.WordList.GroupByNumber(group).WordCount > 1;
                            }
                        }
                    }
                    break;
                }
            }
            mnuEDocumentMap.Text = (splitContainer1.Panel2Collapsed ? "Show" : "Hide") + " Document Map";
            mnuELineNumbers.Text = (rtfLogic1.ShowLineNumbers ? "Hide" : "Show") + " Line Numbers";
        }

        private void ResetEditMenu() {
            // always reenable all items so shortcuts work
            foreach (ToolStripItem itm in cmEdit.Items) {
                itm.Enabled = true;
            }
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            // move menu items to edit menu
            mnuEUndo.Owner = mnuEdit.DropDown;
            mnuERedo.Owner = mnuEdit.DropDown;
            mnuESep0.Owner = mnuEdit.DropDown;
            mnuECut.Owner = mnuEdit.DropDown;
            mnuEDelete.Owner = mnuEdit.DropDown;
            mnuECopy.Owner = mnuEdit.DropDown;
            mnuEPaste.Owner = mnuEdit.DropDown;
            mnuESelectAll.Owner = mnuEdit.DropDown;
            mnuESep1.Owner = mnuEdit.DropDown;
            mnuEBack.Owner = mnuEdit.DropDown;
            mnuEForward.Owner = mnuEdit.DropDown;
            mnuESep1a.Owner = mnuEdit.DropDown;
            mnuEFind.Owner = mnuEdit.DropDown;
            mnuEFindAgain.Owner = mnuEdit.DropDown;
            mnuEReplace.Owner = mnuEdit.DropDown;
            mnuESep2.Owner = mnuEdit.DropDown;
            mnuESnippet.Owner = mnuEdit.DropDown;
            mnuEListDefines.Owner = mnuEdit.DropDown;
            mnuEListCommands.Owner = mnuEdit.DropDown;
            mnuEViewSynonym.Owner = mnuEdit.DropDown;
            mnuEBlockCmt.Owner = mnuEdit.DropDown;
            mnuEUnblockCmt.Owner = mnuEdit.DropDown;
            mnuESep2a.Owner = mnuEdit.DropDown;
            mnuEOpenRes.Owner = mnuEdit.DropDown;
            mnuESep3.Owner = mnuEdit.DropDown;
            mnuEDocumentMap.Owner = mnuEdit.DropDown;
            mnuELineNumbers.Owner = mnuEdit.DropDown;
            mnuECharMap.Owner = mnuEdit.DropDown;
            SetEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            // return menu items to context menu
            mnuEUndo.Owner = cmEdit;
            mnuERedo.Owner = cmEdit;
            mnuESep0.Owner = cmEdit;
            mnuECut.Owner = cmEdit;
            mnuEDelete.Owner = cmEdit;
            mnuECopy.Owner = cmEdit;
            mnuEPaste.Owner = cmEdit;
            mnuESelectAll.Owner = cmEdit;
            mnuESep1.Owner = cmEdit;
            mnuEBack.Owner = cmEdit;
            mnuEForward.Owner = cmEdit;
            mnuESep1a.Owner = cmEdit;
            mnuEFind.Owner = cmEdit;
            mnuEFindAgain.Owner = cmEdit;
            mnuEReplace.Owner = cmEdit;
            mnuESep2.Owner = cmEdit;
            mnuESnippet.Owner = cmEdit;
            mnuEListDefines.Owner = cmEdit;
            mnuEListCommands.Owner = cmEdit;
            mnuEViewSynonym.Owner = cmEdit;
            mnuEBlockCmt.Owner = cmEdit;
            mnuEUnblockCmt.Owner = cmEdit;
            mnuESep2a.Owner = cmEdit;
            mnuEOpenRes.Owner = cmEdit;
            mnuESep3.Owner = cmEdit;
            mnuEDocumentMap.Owner = cmEdit;
            mnuELineNumbers.Owner = cmEdit;
            mnuECharMap.Owner = cmEdit;
            ResetEditMenu();
        }

        private void cmEdit_Opening(object sender, CancelEventArgs e) {
            SetEditMenu();
        }

        private void cmEdit_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void mnuEUndo_Click(object sender, EventArgs e) {
            fctb.Undo();
            MarkAsChanged();
        }

        private void mnuERedo_Click(object sender, EventArgs e) {
            fctb.Redo();
            MarkAsChanged();
        }

        private void mnuECut_Click(object sender, EventArgs e) {
            if (fctb.SelectionLength > 0) {
                fctb.Cut();
                MarkAsChanged();
            }
        }

        private void mnuEDelete_Click(object sender, EventArgs e) {
            if (fctb.Selection.Length == 0) {
                // select next char
                fctb.Selection.GoRight(true);
            }
            if (fctb.Selection.Length > 0) {
                fctb.ClearSelected();
                MarkAsChanged();
            }
        }

        private void mnuECopy_Click(object sender, EventArgs e) {
            if (fctb.SelectionLength > 0) {
                fctb.Copy();
            }
        }

        private void mnuEPaste_Click(object sender, EventArgs e) {
            if (Clipboard.ContainsText()) {
                fctb.Paste();
                MarkAsChanged();
            }
        }

        private void mnuESelectAll_Click(object sender, EventArgs e) {
            fctb.SelectAll();
        }

        private void mnuEBack_Click(object sender, EventArgs e) {
            fctb.NavigateBackward();
        }

        private void mnuEForward_Click(object sender, EventArgs e) {
            fctb.NavigateForward();
        }

        private void mnuEFind_Click(object sender, EventArgs e) {
            FindingForm.SetForm(FormMode == LogicFormMode.Logic ? FindFormFunction.FindLogic : FindFormFunction.FindText, InGame);
            if (fctb.SelectionLength > 0) {
                FindingForm.txtFind.Text = fctb.SelectedText;
            }
            if (!FindingForm.Visible) {
                FindingForm.Visible = true;
            }
            FindingForm.Select();
            FindingForm.txtFind.Select();
        }

        private void mnuEFindAgain_Click(object sender, EventArgs e) {
            if (GFindText.Length > 0) {
                FindInLogic(this, GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc);
            }
        }

        private void mnuEReplace_Click(object sender, EventArgs e) {
            FindingForm.SetForm(FormMode == LogicFormMode.Logic ? FindFormFunction.ReplaceLogic : FindFormFunction.ReplaceText, InGame);
            if (fctb.SelectionLength > 0) {
                FindingForm.txtFind.Text = fctb.SelectedText;
            }
            if (!FindingForm.Visible) {
                FindingForm.Visible = true;
            }
            FindingForm.Select();
            FindingForm.rtfReplace.Select();
        }

        private void mnuESnippet_Click(object sender, EventArgs e) {
            if (fctb.Selection.Length == 0) {
                ShowSnippetList();
            }
            else {
                using (frmSnippets Snippets = new(true, fctb.SelectedText)) {
                    Snippets.ShowDialog(this);
                }
            }
        }

        private void mnuEListDefines_Click(object sender, EventArgs e) {
            if (InGame) {
                ShowDefineList();
            }
        }

        private void mnuEListCommands_Click(object sender, EventArgs e) {
            if (InGame && !EditGame.SierraSyntax) {
                ShowCommandList();
            }
        }

        private void mnuEBlockCmt_Click(object sender, EventArgs e) {
            // if line ends with newline, InsertLinePrefix will put the prefix on the next line,
            // so backup one line first
            fctb.Selection.Normalize();
            if (fctb.Selection.End.iChar == 0 && fctb.Selection.Length > 0) {
                fctb.Selection.End = new Place(fctb.Selection.End.iChar, fctb.Selection.End.iLine - 1);
            }
            fctb.InsertLinePrefix(fctb.CommentPrefix);
        }

        private void mnuEUnblockCmt_Click(object sender, EventArgs e) {
            // if line ends with newline, InsertLinePrefix will put the prefix on the next line,
            // so backup one line first
            fctb.Selection.Normalize();
            if (fctb.Selection.End.iChar == 0 && fctb.Selection.Length > 0) {
                fctb.Selection.End = new Place(fctb.Selection.End.iChar, fctb.Selection.End.iLine - 1);
            }
            fctb.RemoveLinePrefix(fctb.CommentPrefix);
        }

        private void mnuEOpenRes_Click(object sender, EventArgs e) {
            // only if logic/include is InGame
            if (!InGame) {
                return;
            }

            Point taginfo = (Point)mnuEOpenRes.Tag;
            switch (taginfo.X) {
            case 0:
                OpenGameLogic((byte)taginfo.Y);
                break;
            case 1:
                OpenGamePicture((byte)taginfo.Y);
                break;
            case 2:
                OpenGameSound((byte)taginfo.Y);
                break;
            case 3:
                OpenGameView((byte)taginfo.Y);
                break;
            default:
                // include file
                string filename = mnuEOpenRes.Text[6..(^13)];
                if (InGame && !EditGame.SierraSyntax) {
                    // globals.txt and reserved.txt use special editors
                    if (filename.Equals("globals.txt", StringComparison.OrdinalIgnoreCase)) {
                        OpenGlobals();
                        return;
                    }
                    else if (filename.Equals("reserved.txt", StringComparison.OrdinalIgnoreCase)) {
                        OpenReservedEditor();
                        return;
                    }
                    // relative to the source dir
                    filename = Path.GetFullPath(Path.Combine(EditGame.SrcResDir, filename));
                }
                else {
                    // relative to current dir of this logic
                    if (FormMode == LogicFormMode.Logic) {
                        if (EditLogic.SourceFile.Length > 0) {
                            filename = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(EditLogic.SourceFile), filename));
                        }
                        else {
                            // try current dir
                            filename = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), filename));
                        }
                    }
                    else {
                        if (TextFilename.Length > 0) {
                            filename = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(TextFilename), filename));
                        }
                        else {
                            // try current dir
                            filename = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), filename));
                        }
                    }
                }
                if (!File.Exists(filename)) {
                    MDIMain.MsgBoxWithHelp(
                        "Unable to find '" + mnuEOpenRes.Text[6..(^13)] + "' relative to the current directory.",
                        "File Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        @"htm\commands\syntax_fan.htm#include");
                    return;
                }
                OpenTextFile(filename);
                break;
            }
        }

        private void mnuEViewSynonym_Click(object sender, EventArgs e) {
            if (EditGame is null || !EditGame.SierraSyntax) {
                AGIToken token = fctb.TokenFromPos();
                Place place = new(token.StartPos, token.Line);
                fctb.Selection.Start = place;
                place.iChar = token.EndPos;
                fctb.Selection.End = place;
                ShowSynonymList(token.Text);
            }
        }

        private void mnuEDocumentMap_Click(object sender, EventArgs e) {
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
        }

        private void mnuELineNumbers_Click(object sender, EventArgs e) {
            rtfLogic1.ShowLineNumbers = !rtfLogic1.ShowLineNumbers;
            rtfLogic2.ShowLineNumbers = !rtfLogic2.ShowLineNumbers;
        }

        private void mnuECharMap_Click(object sender, EventArgs e) {
            using (frmCharPicker CharPicker = new(CodePage)) {
                CharPicker.ShowDialog(MDIMain);
                if (CharPicker.DialogResult == DialogResult.OK) {
                    if (CharPicker.InsertString.Length > 0) {
                        fctb.InsertText(CharPicker.InsertString, true);
                    }
                }
            }
        }
        #endregion

        #region Control Events
        private void btnIndent_Click(object sender, EventArgs e) {
            fctb.IncreaseIndent();
        }

        private void btnOutdent_Click(object sender, EventArgs e) {
            fctb.DecreaseIndent();
        }

        private void fctb_Enter(object sender, EventArgs e) {
            // !!! when form gets focus, it always sets focus to the control in
            // the Panel with TabIndex 0; to keep the current WinAGIFCTB in 
            // focus, the panel TabIndex values must be reset each time the 
            // focused control changes

            fctb = (sender as WinAGIFCTB);
            if (fctb == rtfLogic1) {
                splitLogic.Panel1.TabIndex = 1;
                splitLogic.Panel2.TabIndex = 0;
            }
            else {
                splitLogic.Panel1.TabIndex = 0;
                splitLogic.Panel2.TabIndex = 1;
            }
            documentMap1.Target = fctb;
            picTip.Visible = false;
            fctb.Controls.Add(picTip);
            picTip.BringToFront();
        }

        private void fctb_KeyPressed(object sender, KeyPressEventArgs e) {
            string linetext;

            switch ((int)e.KeyChar) {
            case 9:
                // TAB
                // tabs need to be ignored; they are converted to spaces automatically
                // by the control
                e.KeyChar = (char)0;
                e.Handled = true;
                break;
            case 10 or 13:
                // ENTER
                break;
            case 8:
                // BACKSPACE
                if (picTip.Visible) {
                    Place thispos = new Place(TipCmdToken.EndPos + 1, TipCmdToken.Line);
                    if (fctb.Selection.Start <= thispos) {
                        // cursor has backed over start of command needing the tip - hide it
                        picTip.Visible = false;
                    }
                    else {
                        UpdateTip(fctb.Selection.Start);
                    }
                }
                break;
            case 40:
                // open parenthesis
                if (WinAGISettings.AutoQuickInfo.Value) {
                    if (!picTip.Visible) {
                        if (NeedCommandTip(fctb, fctb.Selection.Start)) {
                            RepositionTip(TipCmdToken);
                        }
                    }
                }
                break;
            case 32 or 44:
                // space, comma
                if (WinAGISettings.AutoQuickInfo.Value) {
                    if (!picTip.Visible) {
                        if (NeedCommandTip(fctb, fctb.Selection.Start)) {
                            // if in a string, don't respond to space, comma
                            if (fctb.TokenFromPos(fctb.Selection.Start).Type != AGITokenType.String) {
                                if (e.KeyChar == ',') {
                                    TipCurArg++;
                                }
                                RepositionTip(TipCmdToken);
                            }
                        }
                    }
                    else {
                        AGITokenType cursortype = fctb.TokenFromPos().Type;
                        if (cursortype != AGITokenType.Comment && cursortype != AGITokenType.String) {
                            if (e.KeyChar == ',') {
                                TipCurArg++;
                            }
                            RepositionTip(TipCmdToken);
                            picTip.Refresh();
                        }
                    }
                }
                break;
            case 41:
                // close parenthesis
                if (picTip.Visible) {
                    // if this range of text is not in a quote or comment
                    AGITokenType cursortype = fctb.TokenFromPos().Type;
                    if (cursortype != AGITokenType.Comment && cursortype != AGITokenType.String) {
                        picTip.Visible = false;
                    }
                }
                break;
            case 35:
                // #
                if (WinAGISettings.UseSnippets.Value) {
                    Place check = fctb.Selection.Start;
                    check.iChar--;
                    linetext = fctb.Lines[check.iLine];
                    AGIToken snippetname = fctb.TokenFromPos(check);
                    if (snippetname.Text == "#") {
                        AGIToken starttoken = WinAGIFCTB.PreviousToken(linetext, snippetname);
                        if (starttoken.Text == ")") {
                            // snippet has args - backup to find the full snippet text
                            do {
                                starttoken = WinAGIFCTB.PreviousToken(linetext, starttoken);
                                if (starttoken.Text == "(") {
                                    break;
                                }
                            } while (starttoken.Type != AGITokenType.None);
                            if (starttoken.Type != AGITokenType.None) {
                                starttoken = WinAGIFCTB.PreviousToken(linetext, starttoken);
                                if (starttoken.Text[0] == '#') {
                                    // adjust snippetname
                                    snippetname.StartPos = starttoken.StartPos;
                                    snippetname.Text = linetext[snippetname.StartPos..snippetname.EndPos];
                                }
                            }
                        }
                    }
                    if (snippetname.Type != AGITokenType.Identifier || snippetname.Text.Length <= 2 || snippetname.Text[0] != '#') {
                        return;
                    }
                    // check for indentation; if leading hashtag is preceded ONLY by
                    // white space, save the indent value in case the replaced text
                    // is multi-line
                    if (snippetname.StartPos > 0 && linetext[..snippetname.StartPos].Trim().Length == 0) {
                        SnipIndent = snippetname.StartPos;
                    }
                    Snippet snippetvalue = new();
                    snippetvalue = CheckSnippet(snippetname.Text[1..^1]);
                    if (snippetvalue.Name.Length == 0) {
                        return;
                    }
                    snippetvalue.Value = snippetvalue.Value.Replace("\r\n", "\r\n" + "".PadRight(SnipIndent));
                    Place start = new(snippetname.StartPos, snippetname.Line);
                    Place end = fctb.Selection.Start;
                    fctb.Selection.Start = start;
                    fctb.Selection.End = end;
                    fctb.InsertText(snippetvalue.Value, true);
                    e.KeyChar = (char)0;
                    e.Handled = true;
                }
                break;
            }
        }

        private void fctb_KeyUp(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
            case Keys.Up or Keys.Down:
                if (picTip.Visible) {
                    picTip.Visible = false;
                }
                break;
            case Keys.Left or Keys.Right:
                if (picTip.Visible) {
                    if (NeedCommandTip(fctb, fctb.Selection.Start, true)) {
                        UpdateTip(fctb.Selection.Start);
                    }
                    else {
                        picTip.Visible = false;
                    }
                }
                break;
            case Keys.Escape:
                // ESC
                if (picTip.Visible) {
                    picTip.Visible = false;
                }
                if (fctb.ToolTip.Active) {
                    fctb.ToolTip.Hide(this);
                }
                break;
            }
        }

        private void fctb_MouseDown(object sender, MouseEventArgs e) {
            if (lstDefines.Visible) {
                lstDefines.Visible = false;
                fctb.CaretVisible = true;
            }
            // ! FCTB doesn't return correct place/position when mouse-down occurs on
            // a folded line, it returns the line number immediately below the fold.
            // This makes it VERY difficult to place the cursor on a right-click.
            // What seems to work is to convert the Location(Point) to a Place using
            // PointToPlace(), then convert it back to a Point using PlaceToPoint().
            // Because the converted Point Y value is top pixel of the row, it needs
            // to be corrected by the offet (e.Y % charHeight). If they are different,
            // then new mousepos must be on a fold

            int scrolloffset = 0;
            if (fctb.VerticalScroll.Enabled) {
                scrolloffset = fctb.VerticalScroll.Value % fctb.CharHeight;
            }
            bool onfold = (e.Y - (e.Y + scrolloffset) % fctb.CharHeight) != fctb.PlaceToPoint(fctb.PointToPlace(e.Location)).Y;

            if (e.Button == MouseButtons.Right) {
                // move cursor if not on selection
                if (fctb.Selection.Length == 0) {
                    fctb.Selection.Start = fctb.PointToPlace(e.Location);
                    fctb.SelectionLength = 0;
                }
                else {
                    int spos = fctb.PlaceToPosition(fctb.Selection.Start);
                    int epos = fctb.PlaceToPosition(fctb.Selection.End);
                    Place place = fctb.PointToPlace(e.Location);
                    if (onfold) {
                        //adjust line up 1
                        place.iLine--;
                    }
                    int pos = fctb.PlaceToPosition(place);
                    if (fctb.Selection.Start > fctb.Selection.End) {
                        int swap = spos;
                        spos = epos;
                        epos = swap;
                    }
                    if (pos < spos || pos > epos) {
                        fctb.Selection.Start = fctb.PointToPlace(e.Location);
                        fctb.SelectionLength = 0;
                    }
                }
            }
            // override FCTB line selection  (it does not include the
            // linefeed) by checking for left-click on line number
            // area
            //
            if (fctb.Cursor != Cursors.Hand && (e.Button == MouseButtons.Left) && (e.Location.X < (fctb.LeftIndent - 1))) {
                SelectEntireLine = true;
            }
        }

        private void fctb_MouseUp(object sender, MouseEventArgs e) {
            if (picTip.Visible) {
                if (fctb.Selection.Start.iLine != TipCmdToken.Line) {
                    picTip.Visible = false;
                }
                else {
                    AGIToken seltoken = fctb.TokenFromPos();
                    int argcount = 0;
                    AGIToken cmdtoken = FindPrevCmd(fctb, seltoken, ref argcount);
                    if (cmdtoken.StartPos == TipCmdToken.StartPos && cmdtoken.EndPos == TipCmdToken.EndPos) {
                        if (argcount != TipCurArg) {
                            TipCurArg = argcount;
                            picTip.Refresh();
                        }
                    }
                    else {
                        picTip.Visible = false;
                    }
                }
            }
            if (SelectEntireLine) {
                SelectEntireLine = false;
                if (e.Button == MouseButtons.Left && e.Location.X < (fctb.LeftIndent - 1)) {
                    // need to account for window being offset from exact line spacing, as well as
                    // mouse not being on exact start of a new line
                    int scrolloffset = 0;
                    if (fctb.VerticalScroll.Enabled) {
                        scrolloffset = fctb.VerticalScroll.Value % fctb.CharHeight;
                    }
                    bool onfold = (e.Y - (e.Y + scrolloffset) % fctb.CharHeight) != fctb.PlaceToPoint(fctb.PointToPlace(e.Location)).Y;

                    if (onfold) {
                        // start is always where fold is
                        var start = fctb.Selection.Start;
                        var end = fctb.Selection.End;
                        // if only the fold is selected (lines are the same)
                        if (start.iLine == end.iLine) {
                            // start is ok
                            // change end to end of fold
                            if (fctb.EndFoldingLine == -1) {
                                // (watch out for invalid folding value!)
                                return;
                            }
                            end = new(0, fctb.EndFoldingLine + 1);
                        }
                        else {
                            if (start < end) {
                                // dragging up; change start so it points to start of fold
                                if (fctb.StartFoldingLine == -1) {
                                    // (watch out for invalid folding value!)
                                    return;
                                }
                                start = new(0, fctb.StartFoldingLine + 1);
                                // change end so it points to start of next line
                                if (end.iLine + 1 < fctb.LinesCount) {
                                    end = new(0, end.iLine + 1);
                                }
                            }
                            else {
                                // drag down; change start so it points to end of fold
                                // start and end are already correct?
                            }
                        }
                        fctb.Selection.Start = start;
                        fctb.Selection.End = end;
                    }
                    else {
                        if (fctb.Selection.Start <= fctb.Selection.End) {
                            if (fctb.Selection.End.iLine + 1 < fctb.LinesCount) {
                                fctb.Selection.End = new(0, fctb.Selection.End.iLine + 1);
                            }
                        }
                        else {
                            if (fctb.Selection.Start.iLine + 1 < fctb.LinesCount) {
                                Place tmp = fctb.Selection.End;
                                fctb.Selection.Start = new(0, fctb.Selection.Start.iLine + 1);
                                fctb.Selection.End = tmp;
                            }
                        }
                    }
                    fctb.Invalidate();
                }
            }
        }

        private void fctb_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (ModifierKeys.HasFlag(Keys.Control)) {
                AGIToken token = fctb.TokenFromPos();
                switch (token.Type) {
                case AGITokenType.Identifier:
                    // look for resourceid
                    for (int restype = 0; restype < 4; restype++) {
                        for (int num = 0; num < 256; num++) {
                            if (IDefLookup[restype, num].Type != ArgType.None) {
                                if (token.Text == IDefLookup[restype, num].Name) {
                                    switch (restype) {
                                    case 0:
                                        OpenGameLogic((byte)num);
                                        break;
                                    case 1:
                                        OpenGamePicture((byte)num);
                                        break;
                                    case 2:
                                        OpenGameSound((byte)num);
                                        break;
                                    case 3:
                                        OpenGameView((byte)num);
                                        break;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case AGITokenType.String:
                    if (fctb.PreviousToken(token).Text == "#include" ||
                        (EditGame is not null && EditGame.SierraSyntax &&
                        fctb.PreviousToken(token).Text == "%include")) {
                        if (token.Text[^1] != '"') {
                            token.Text += '"';
                        }
                        // resoureids.txt is not editable
                        if (!token.Text.Equals("\"resourceids.txt\"", StringComparison.OrdinalIgnoreCase)) {
                            // include file
                            string filename = token.Text.Trim('\"');
                            if (InGame && !EditGame.SierraSyntax) {
                                // globals.txt and reserved.txt use special editors
                                if (filename.Equals("globals.txt", StringComparison.OrdinalIgnoreCase)) {
                                    OpenGlobals();
                                    return;
                                }
                                else if (filename.Equals("reserved.txt", StringComparison.OrdinalIgnoreCase)) {
                                    OpenReservedEditor();
                                    return;
                                }
                                // relative to the source dir
                                filename = Path.GetFullPath(Path.Combine(EditGame.SrcResDir, filename));
                            }
                            else {
                                // relative to current dir of this logic
                                if (FormMode == LogicFormMode.Logic) {
                                    if (EditLogic.SourceFile.Length > 0) {
                                        filename = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(EditLogic.SourceFile), filename));
                                    }
                                    else {
                                        // try current dir
                                        filename = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), filename));
                                    }
                                }
                                else {
                                    if (TextFilename.Length > 0) {
                                        filename = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(TextFilename), filename));
                                    }
                                    else {
                                        // try current dir
                                        filename = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), filename));
                                    }
                                }
                            }
                            if (!File.Exists(filename)) {
                                MDIMain.MsgBoxWithHelp(
                                    "Unable to find '" + mnuEOpenRes.Text[6..(^13)] + "' relative to the current directory.",
                                    "File Not Found",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information,
                                    @"htm\commands\syntax_fan.htm#include");
                                return;
                            }
                            OpenTextFile(filename);
                            break;
                        }
                        break;
                    }
                    break;
                }
            }
        }

        private void fctb_SelectionChanged(object sender, EventArgs e) {
            if (fctb is null) {
                return;
            }
            spLine.Text = "Line: " + fctb.Selection.End.iLine;
            spColumn.Text = "Col: " + fctb.Selection.End.iChar;
        }

        private void fctb_TextChanging(object sender, TextChangingEventArgs e) {
            if (loading || !InGame) {
                return;
            }
            // to manage line changes for updating error/warning
            // list, save current line count and selection start line
            // before the change occurs
            chgStart = Math.Min(fctb.Selection.Start.iLine, fctb.Selection.End.iLine);
            chgCount = fctb.LinesCount;
            // also save selection length for single character changes
            chgLength = fctb.Selection.Length;
        }

        private void fctb_TextChanged(object sender, TextChangedEventArgs e) {
            if (loading || !Visible) {
                return;
            }

            // if an ingame logic, check for line changes
            // UNLESS saving; no change made in that case)
            if (FormMode == LogicFormMode.Logic && InGame && !saving) {
                // lines are either added or deleted; if a
                // selection is pasted over multiple lines,
                // first a delete change occurs, then an add
                // change
                int added = fctb.Selection.FromLine - chgStart;
                int deleted = chgCount - fctb.LinesCount;
                if (added != 0 || deleted != 0) {
                    if (added > 0) {
                        UpdateInfoGrid(LogicNumber, chgStart, added, 1);
                    }
                    else if (deleted > 0) {
                        if (deleted == 1 && chgLength <= 1) {
                            UpdateInfoGrid(LogicNumber, chgStart, 1, 3);
                        }
                        else {
                            UpdateInfoGrid(LogicNumber, chgStart, deleted, 2);
                        }
                    }
                    // refresh counts
                    MDIMain.UpdateGridCounts();
                }
            }

            DefChanged = true;
            MarkAsChanged();
            spStatus.Text = "";
            spLine.Text = "Line: " + fctb.Selection.End.iLine;
            spColumn.Text = "Col: " + fctb.Selection.End.iChar;
        }

        private void fctb_TextChangedDelayed(object sender, TextChangedEventArgs e) {
            string text = e.ChangedRange.Text ?? string.Empty;
            bool bigChange =
                text.Length > 1 ||          // more than a single char
                text.Contains('\n') ||  // multi-line
                text.Contains('\r');    // in case of CRLF
            if (bigChange) {
                // highlight entire changed block
                AGISyntaxHighlight(e.ChangedRange);
            }
            else {
                // highlight only changed line ±1
                int line = e.ChangedRange.Start.iLine;

                int startLine = Math.Max(0, line - 1);
                int endLine = Math.Min(fctb.LinesCount - 1, line + 1);

                var smallRange = new FastColoredTextBoxNS.Range(
                    fctb,
                    0, startLine,
                    fctb.Lines[endLine].Length, endLine
                );

                AGISyntaxHighlight(smallRange);
            }
        }

        private void fctb_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) {
            if (!WinAGISettings.ShowDefTips.Value || picTip.Visible) {
                return;
            }
            AGIToken seltoken = fctb.TokenFromPos(e.Place);
            if (seltoken.Type != AGITokenType.Identifier) {
                return;
            }
            string definetext = seltoken.Text;
            if (definetext.Length == 0) {
                return;
            }
            // check locals first
            if (DefChanged) {
                BuildLDefLookup();
            }
            foreach (var def in LDefLookup) {
                if (definetext.Equals(def.Name)) {
                    definetext += " = " + def.Value;
                    e.ToolTipText = definetext;
                    return;
                }
            }
            if (EditGame is not null) {
                if (!EditGame.SierraSyntax) {
                    // next check globals
                    if (EditGame.IncludeGlobals) {
                        if (EditGame.GlobalDefines.ContainsName(definetext)) {
                            definetext += " = " + EditGame.GlobalDefines[definetext].Value;
                            e.ToolTipText = definetext;
                            return;
                        }
                    }
                    // then resIDs
                    if (EditGame.IncludeIDs) {
                        // then ids; we will test logics, then views, then sounds, then pics
                        // as that's the order that defines are most likely to be used
                        for (int i = 0; i <= EditGame.Logics.Max; i++) {
                            if (IDefLookup[(int)AGIResType.Logic, i].Type != ArgType.None) {
                                if (definetext.Equals(IDefLookup[(int)AGIResType.Logic, i].Name)) {
                                    definetext += " = " + IDefLookup[(int)AGIResType.Logic, i].Value;
                                    e.ToolTipText = definetext;
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i <= EditGame.Views.Max; i++) {
                            if (IDefLookup[(int)AGIResType.View, i].Type != ArgType.None) {
                                if (definetext.Equals(IDefLookup[(int)AGIResType.View, i].Name)) {
                                    definetext += " = " + IDefLookup[(int)AGIResType.View, i].Value;
                                    e.ToolTipText = definetext;
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i <= EditGame.Sounds.Max; i++) {
                            if (IDefLookup[(int)AGIResType.Sound, i].Type != ArgType.None) {
                                if (definetext.Equals(IDefLookup[(int)AGIResType.Sound, i].Name)) {
                                    definetext += " = " + IDefLookup[(int)AGIResType.Sound, i].Value;
                                    e.ToolTipText = definetext;
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i <= EditGame.Pictures.Max; i++) {
                            if (IDefLookup[(int)AGIResType.Picture, i].Type != ArgType.None) {
                                if (definetext.Equals(IDefLookup[(int)AGIResType.Picture, i].Name)) {
                                    definetext += " = " + IDefLookup[(int)AGIResType.Picture, i].Value;
                                    e.ToolTipText = definetext;
                                    return;
                                }
                            }
                        }
                    }
                    // then reserved
                    if (EditGame.IncludeReserved) {
                        // still no match, check reserved define
                        Define[] tmpDefines = EditGame.ReservedDefines.All();
                        for (int i = 0; i < tmpDefines.Length; i++) {
                            if (definetext.Equals(tmpDefines[i].Name)) {
                                definetext += " = " + tmpDefines[i].Value;
                                e.ToolTipText = definetext;
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void picTip_Paint(object sender, PaintEventArgs e) {
            RedrawTipWindow(e.Graphics);
        }

        private void lstDefines_MouseDoubleClick(object sender, MouseEventArgs e) {
            SelectDefine();
        }

        private void lstDefines_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) {
            // tab key needs to be handled as normal input
            if (e.KeyCode == Keys.Tab) {
                e.IsInputKey = true;
            }
        }

        private void lstDefines_KeyDown(object sender, KeyEventArgs e) {
            // tab is same as enter
            if (e.KeyCode == Keys.Tab) {
                // ENTER selects the highlighted define
                SelectDefine();
                lstDefines.Visible = false;
                fctb.CaretVisible = true;
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void lstDefines_KeyPress(object sender, KeyPressEventArgs e) {
            switch (e.KeyChar) {
            case '\r' or '\n':
                // ENTER selects the highlighted define
                SelectDefine();
                lstDefines.Visible = false;
                fctb.CaretVisible = true;
                break;
            case (char)27:
                // ESCAPE cancels, and restores selection
                fctb.Selection.Start = DefStartPos;
                fctb.Selection.End = DefEndPos;
                fctb.SelectedText = DefText;
                fctb.Selection.Start = fctb.Selection.End;
                lstDefines.Visible = false;
                fctb.CaretVisible = true;
                break;
            default:
                // only defines build a preview text
                if ((string)lstDefines.Tag != "defines") {
                    return;
                }
                if (e.KeyChar == (char)8) {
                    // BACKSPACE deletes character preceding cursor
                    if (PrevText.Length > 0) {
                        PrevText = PrevText[..^1];
                        DefEndPos.iChar--;
                    }
                    else {
                        break;
                    }
                }
                else {
                    // add newly typed character
                    PrevText += e.KeyChar;
                    DefEndPos.iChar++;
                }
                if ((string)lstDefines.Tag == "defines") {
                    // the SelectText method causes focus to shift to the
                    // textbox without firing the Enter event. Trial and
                    // error testing found this hack to keep focus in the
                    // listbox - force focus to fctb, change the text,
                    // then force it back to listbox. So far, it appears
                    // to be working
                    fctb.Select();
                    fctb.SelectedText = PrevText;
                    lstDefines.Select();
                    fctb.Selection.Start = DefStartPos;
                    fctb.Selection.End = DefEndPos;
                }
                // find closest match, if there's something typed
                // (ignore case)
                if (PrevText.Length > 0) {
                    foreach (ListViewItem item in lstDefines.Items) {
                        if (item.Text.Left(PrevText.Length).Equals(PrevText, StringComparison.OrdinalIgnoreCase)) {
                            if (lstDefines.SelectedItems.Count > 0) {
                                lstDefines.SelectedItems[0].Selected = false;
                            }
                            item.Selected = true;
                            item.EnsureVisible();
                            break;
                        }
                    }
                }
                break;
            }
            e.KeyChar = '\0';
            e.Handled = true;
        }

        private void lstDefines_VisibleChanged(object sender, EventArgs e) {
            fctb.NoMouse = lstDefines.Visible;
        }
        #endregion
        #endregion

        #region Methods
        private void InitStatusStrip() {
            spLine = new ToolStripStatusLabel();
            spColumn = new ToolStripStatusLabel();
            spStatus = MDIMain.spStatus;
            spCapsLock = MDIMain.spCapsLock;
            spNumLock = MDIMain.spNumLock;
            spInsLock = MDIMain.spInsLock;
            // 
            // spLine
            // 
            spLine.AutoSize = false;
            spLine.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spLine.BorderStyle = Border3DStyle.SunkenInner;
            spLine.Name = "spLine";
            spLine.Size = new Size(70, 18);
            spLine.Text = "Line: --";
            // 
            // spColumn
            // 
            spColumn.AutoSize = false;
            spColumn.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spColumn.BorderStyle = Border3DStyle.SunkenInner;
            spColumn.Name = "spColumn";
            spColumn.Size = new Size(70, 18);
            spColumn.Text = "Col: --";
        }

        private ReadMsgResult ReadMsgs(ref MessageData[] Messages) {
            // all valid message declarations in strText are
            // put into the Messages() array; the MsgUsed array
            // is used to mark each declared message added
            //
            // if an error in the logic is detected that would
            // make it impossible to accurately update the message
            // section, the function returns an error code, and includes
            // the line where the error was found, and any
            // additional information regarding the error
            // error codes:
            //     1 = invalid msg number
            //     2 = duplicate msg number
            //     3 = not a string
            //     4 = stuff not allowed after msg declaration
            //     5 = invalid string (missing closing quote)
            int msgNum;
            ReadMsgResult retval = new() {
                Error = 0
            };

            // get first message marker position
            AGIToken token = fctb.TokenFromPos(new Place(0, 0));
            while (token.Type != AGITokenType.None) {
                if (token.Type == AGITokenType.Identifier && token.Text == "#message") {
                    // next cmd should be msg number
                    token = fctb.NextToken(token);
                    if (token.Type != AGITokenType.Number || (msgNum = int.Parse(token.Text)) < 1 || msgNum > 255) {
                        // invalid msg number
                        retval.Error = 1;
                        retval.Pos = token.StartPos;
                        retval.Line = token.Line;
                        return retval;
                    }
                    if (Messages[msgNum].Declared) {
                        // duplicate msg number
                        // user needs to fix message section first;
                        // return false, and use the Message structure to indicate
                        // what the problem is, and on which line it occurred
                        retval.Error = 2;
                        retval.Pos = token.StartPos;
                        retval.Line = token.Line;
                        retval.MsgNum = msgNum;
                        return retval;
                    }
                    // next cmd should be a string
                    token = fctb.NextToken(token);
                    switch (token.Type) {
                    case AGITokenType.String:
                        // valid string is >1 char, ends with '"' and doesn't end with '\"'
                        if (token.Text.Length == 1 || token.Text[^1] != '\"' || token.Text[^2] == '\\') {
                            retval.Error = 5;
                            retval.Pos = token.StartPos;
                            retval.Line = token.Line;
                            retval.MsgNum = msgNum;
                            return retval;
                        }
                        Messages[msgNum].Text = token.Text;
                        Messages[msgNum].Line = token.Line;
                        Messages[msgNum].Type = 0;
                        // check for concatenation
                        AGIToken concattoken;
                        do {
                            // check for end of line
                            Place concatstart = new(token.StartPos, token.Line);
                            token = fctb.NextToken(token, false);
                            if (token.Type != AGITokenType.None && token.Type != AGITokenType.Comment) {
                                // stuff not allowed on line after msg declaration
                                retval.Error = 4;
                                retval.Pos = concatstart.iChar;
                                retval.Line = concatstart.iLine;
                                retval.MsgNum = msgNum;
                                return retval;
                            }
                            concattoken = fctb.NextToken(token, true);
                            while (concattoken.Type == AGITokenType.String) {
                                if (concattoken.Text[^1] == '\"') {
                                    // valid string is >1 char, ends with '"' and doesn't end with '\"'
                                    if (concattoken.Text.Length == 1 || concattoken.Text[^2] == '\\') {
                                        retval.Error = 5;
                                        retval.Pos = concattoken.StartPos;
                                        retval.Line = concattoken.Line;
                                        retval.MsgNum = msgNum;
                                        return retval;
                                    }
                                }
                                // add it to current 
                                Messages[msgNum].Text = Messages[msgNum].Text[..^1] + concattoken.Text[1..];
                                Messages[msgNum].Concat++;
                                token = concattoken;
                                concattoken = fctb.NextToken(token, true);
                            }
                        } while (concattoken.Type == AGITokenType.String);
                        // set flag to show message is declared
                        Messages[msgNum].Declared = true;
                        break;
                    case AGITokenType.Identifier:
                        // try replacing with define (locals, then globals, then reserved
                        bool defFound = false;
                        foreach (var def in LDefLookup) {
                            if (def.Type == ArgType.DefStr) {
                                if (def.Name == token.Text) {
                                    token.Text = def.Value;
                                    defFound = true;
                                    break;
                                }
                            }
                        }
                        if (!defFound) {
                            if (EditGame.GlobalDefines.ContainsName(token.Text)) {
                                if (EditGame.GlobalDefines[token.Text].Type == ArgType.DefStr) {
                                    defFound = true;
                                    break;
                                }
                            }
                        }
                        if (!defFound) {
                            for (int i = 0; i < EditGame.ReservedDefines.ByArgType(ArgType.DefStr).Length; i++) {
                                if (EditGame.ReservedDefines.ByArgType(ArgType.DefStr)[i].Name == token.Text) {
                                    defFound = true;
                                    break;
                                }
                            }
                        }
                        // if it was replaced, accept whatever was used as
                        // the define value; if not replaced, it's error
                        if (defFound) {
                            Messages[msgNum].Text = token.Text;
                            Messages[msgNum].Line = token.Line;
                            Messages[msgNum].Type = 2;
                            Messages[msgNum].Declared = true;
                            break;
                        }
                        else {
                            // not a string or defined string
                            retval.Error = 3;
                            retval.Pos = token.StartPos;
                            retval.Line = token.Line;
                            retval.MsgNum = msgNum;
                            return retval;
                        }
                    default:
                        // not a string or identifer
                        retval.Error = 3;
                        retval.Pos = token.StartPos;
                        retval.Line = token.Line + 1;
                        retval.MsgNum = msgNum;
                        return retval;
                    }
                }
                token = fctb.NextToken(token, true);
            }
            return retval;
        }

        private AGIToken NextMsg(AGIToken token, string[] StrDefs) {
            // starting at position pos, step through cmds until a match is
            // found for a cmd that has a msg argument:
            //     log(m#)
            //     print(m#)
            //     display(#,#,m#)
            //     get.num(m#, v#)
            //     print.at(m#, #, #, #)
            //     set.menu(m#)
            //     set.string(s#, m#)
            //     get.string(s#, m#, #, #, #)
            //     set.game.id(m#)
            //     set.menu.item(m#, c#)
            //     set.cursor.char(m#)
            // also need to check for s#=m#; do this by building custom array
            // of matching elements; all s## tokens, plus any defines that
            // are stringtype
            //
            // return values
            //    - if OK, token pointing to the message value (type = String)
            //      or to the message marker (type = Identifier)
            //      
            //    - if error token number set to error value:
            //         -1 = missing '(' after a command
            //         -2 = missing end quote
            //         -3 = not a string, define or message marker(m##)
            int argnum;

            AGIToken msgtoken = fctb.NextToken(token, true);
            while (msgtoken.Type != AGITokenType.None) {
                if (msgtoken.Type == AGITokenType.Identifier) {
                    argnum = -1;
                    // check against list of cmds with msg arguments:
                    switch (msgtoken.Text.Length) {
                    case 3:
                        if (msgtoken.Text == "log") {
                            argnum = 0;
                        }
                        break;
                    case 5:
                        if (msgtoken.Text == "print") {
                            argnum = 0;
                        }
                        break;
                    case 7:
                        if (msgtoken.Text == "display") {
                            argnum = 2;
                        }
                        else if (msgtoken.Text == "get.num") {
                            argnum = 0;
                        }
                        break;
                    case 8:
                        if (msgtoken.Text == "print.at" || msgtoken.Text == "set.menu") {
                            argnum = 0;
                        }
                        break;
                    case 10:
                        if (msgtoken.Text == "set.string" || msgtoken.Text == "get.string") {
                            argnum = 1;
                        }
                        break;
                    case 11:
                        if (msgtoken.Text == "set.game.id") {
                            argnum = 0;
                        }
                        break;
                    case 13:
                        if (msgtoken.Text == "set.menu.item") {
                            argnum = 0;
                        }
                        break;
                    case 15:
                        if (msgtoken.Text == "set.cursor.char") {
                            argnum = 0;
                        }
                        break;
                    }
                    if (argnum >= 0) {
                        msgtoken = fctb.NextToken(msgtoken);
                        if (msgtoken.Text != "(") {
                            // not a valid cmd; return error
                            msgtoken.Number = -1;
                            return msgtoken;
                        }
                        // arg0
                        msgtoken = fctb.NextToken(msgtoken);
                        if (msgtoken.Type == AGITokenType.None) {
                            // end of input, arg missing; return error
                            msgtoken.Number = -5;
                            return msgtoken;
                        }
                        if (argnum != 0) {
                            do {
                                // next cmd is a comma
                                msgtoken = fctb.NextToken(msgtoken);
                                if (msgtoken.Text != ",") {
                                    // missing comma; return error
                                    msgtoken.Number = -6;
                                    return msgtoken;
                                }
                                // now get next arg
                                msgtoken = fctb.NextToken(msgtoken);
                                argnum--;
                            } while (argnum != 0);
                        }
                    }
                    else {
                        // check for string assignment
                        // s##="text"; or strdefine="text";
                        for (int i = 0; i < StrDefs.Length; i++) {
                            if (msgtoken.Text == StrDefs[i]) {
                                // possible string assignment
                                argnum = 0;
                                break;
                            }
                        }
                        // if not found as a define, check for a string marker(s##)
                        if (argnum == -1 && msgtoken.Text.Length > 0 && msgtoken.Text[0] == 's') {
                            // strip off the 's'
                            string valuetext = msgtoken.Text[1..];
                            if (int.TryParse(valuetext, out int num)) {
                                // possible string assignment
                                // do we care what number? yes- must be 0-23
                                // in the off chance the user is working with
                                // a version that has a limit of 12 strings
                                // we will let the compiler worry about it
                                if (num >= 0 && num <= 23) {
                                    argnum = 0;
                                }
                                else {
                                    // invalid string marker; error
                                    msgtoken.Number = -7;
                                    return msgtoken;
                                }
                            }
                        }
                        if (argnum == 0) {
                            // next cmd should be an equal sign
                            msgtoken = fctb.NextToken(msgtoken, true);
                            if (msgtoken.Text == "=") {
                                // ok, now we know the very next thing is the assigned string!
                                msgtoken = fctb.NextToken(msgtoken);
                                if (msgtoken.Type == AGITokenType.None) {
                                    // end of input; arg missing; error
                                    msgtoken.Number = -5;
                                    return msgtoken;
                                }
                            }
                            else {
                                // not an assignment- probably a string (s#)
                                // argument (e.g in word.to.string
                                argnum = -1;
                            }
                        }
                    }
                    // if a message token was found (argnum >=0)
                    if (argnum >= 0) {
                        // it might be a message marker ('m##') or it might be
                        // a string; or it might be a local, global or reserved
                        // define
                        switch (msgtoken.Type) {
                        case AGITokenType.String:
                            if (msgtoken.Text.Length == 1 || msgtoken.Text[^1] != '\"' || msgtoken.Text[^2] == '\\') {
                                msgtoken.Number = -2;
                                return msgtoken;
                            }
                            // check for concatenation
                            AGIToken concattoken = fctb.NextToken(msgtoken, true);
                            while (concattoken.Type == AGITokenType.String) {
                                if (concattoken.Text[^1] == '\"') {
                                    // valid string is >1 char, ends with '"' and doesn't end with '\"'
                                    if (concattoken.Text.Length == 1 || concattoken.Text[^1] != '\"' || concattoken.Text[^2] == '\\') {
                                        msgtoken.Number = -2;
                                        return msgtoken;
                                    }
                                }

                                // add it to current 
                                msgtoken.Text = msgtoken.Text[..^1] + concattoken.Text[1..];
                                msgtoken.EndPos = concattoken.EndPos;
                                msgtoken.Line = concattoken.Line;
                                concattoken = fctb.NextToken(concattoken, true);
                            }
                            msgtoken.Number = 0;
                            return msgtoken;
                        case AGITokenType.Identifier:
                            // check for message marker first
                            if (msgtoken.Text[0] == 'm') {
                                int num;
                                if (int.TryParse(msgtoken.Text[1..], out num)) {
                                    if (num > 0 && num < 256) {
                                        msgtoken.Number = 0;
                                        return msgtoken;
                                    }
                                    else {
                                        msgtoken.Number = -8;
                                        return msgtoken;
                                    }
                                }
                            }
                            // not a msg marker; try replacing with define -
                            // check local defines first
                            foreach (var def in LDefLookup) {
                                if (msgtoken.Text == def.Name) {
                                    switch (def.Type) {
                                    case ArgType.MsgNum:
                                        msgtoken.Text = def.Value;
                                        msgtoken.Number = 0;
                                        return msgtoken;

                                    case ArgType.DefStr:
                                        msgtoken.Text = def.Value;
                                        msgtoken.Number = 0;
                                        return msgtoken;
                                    default:
                                        // invalid argtype
                                        msgtoken.Number = -3;
                                        return msgtoken;
                                    }
                                }
                            }
                            // try globals next
                            if (EditGame.GlobalDefines.ContainsName(msgtoken.Text)) {
                                var define = EditGame.GlobalDefines[msgtoken.Text];
                                switch (define.Type) {
                                case ArgType.MsgNum:
                                    msgtoken.Number = 0;
                                    return msgtoken;
                                case ArgType.DefStr:
                                    msgtoken.Number = 0;
                                    return msgtoken;
                                default:
                                    // invalid argtype
                                    msgtoken.Number = -3;
                                    return msgtoken;
                                }
                            }
                            // lastly check reserved defines

                            for (int i = 0; i <= 2; i++) {
                                if (msgtoken.Text == EditGame.ReservedDefines.GameInfo[i].Name) {
                                    msgtoken.Number = 0;
                                    return msgtoken;
                                }
                            }
                            // no define found; token is invalid
                            msgtoken.Number = -3;
                            return msgtoken;
                        default:
                            // anything else is an error
                            msgtoken.Number = -3;
                            return msgtoken;
                        }
                    }
                }
                msgtoken = fctb.NextToken(msgtoken, true);
            }
            // end of input
            msgtoken.Number = 0;
            return msgtoken;
        }

        private void MessageCleanup() {
            MessageData[] Messages = new MessageData[256];
            string[] NewMsgs = [];
            bool keepUnused, repeatAction = false;
            string[] stringDefList = [];
            int msgCount = 0;
            string msgtext;
            Place start;
            Place end;

            if (WinAGISettings.WarnMsgs.Value == 0) {
                repeatAction = false;
                DialogResult rtn = MsgBoxEx.Show(MDIMain,
                    "If messages in the message section are not not used anywhere " +
                   "in the logic text, do you still want to keep them?",
                    "Message Cleanup Tool",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    "Always take this action when updating messages.", ref repeatAction,
                   WinAGIHelp, "htm\\winagi\\editor_logic.htm#msgcleanup");
                // if canceled
                if (rtn == DialogResult.Cancel) {
                    return;
                }
                keepUnused = rtn == DialogResult.Yes;
            }
            else {
                keepUnused = WinAGISettings.WarnMsgs.Value == 1;
            }
            if (repeatAction) {
                if (keepUnused) {
                    WinAGISettings.WarnMsgs.Value = 1;
                }
                else {
                    WinAGISettings.WarnMsgs.Value = 2;
                }
                WinAGISettings.WarnMsgs.WriteSetting(WinAGISettingsFile);
            }
            if (DefChanged) {
                BuildLDefLookup();
            }
            // check locals, globals, and reserved for string defines
            int count = 0;
            foreach (var def in LDefLookup) {
                if (def.Type == ArgType.Str) {
                    Array.Resize(ref stringDefList, ++count);
                    stringDefList[^1] = def.Name;
                }
            }
            if (EditGame is not null) {
                Debug.Assert(!EditGame.SierraSyntax);
                foreach (var define in EditGame.GlobalDefines.Values) {
                    if (define.Type == ArgType.Str) {
                        Array.Resize(ref stringDefList, ++count);
                        stringDefList[^1] = define.Name;
                    }
                }
                // add the only resdef that's a string
                Array.Resize(ref stringDefList, ++count);
                stringDefList[^1] = EditGame.ReservedDefines.ReservedStrings[0].Name;
            }
            else {
                // add the only resdef that's a string
                Array.Resize(ref stringDefList, ++count);
                stringDefList[^1] = DefaultReservedDefines.ReservedStrings[0].Name;
            }

            // next, get all messages that are predefined
            ReadMsgResult result = ReadMsgs(ref Messages);
            if (result.Error != 0) {
                // a problem was found that needs to be fixed before
                // messages can be cleaned up
                Place errorlocation = new(result.Pos, result.Line);
                msgtext = "Syntax error in line " + result.Line + " must be corrected\n" +
                         "before message cleanup can continue:\n\n";
                switch (result.Error) {
                case 1:
                    // invalid msg number
                    MDIMain.MsgBoxWithHelp(
                        msgtext + "Invalid message index number.",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\commands\\syntax_fan.htm#messages");
                    break;
                case 2:
                    // duplicate msg number
                    MDIMain.MsgBoxWithHelp(
                        msgtext + "Message index " + result.MsgNum + " is already in use.",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\commands\\syntax_fan.htm#messages");
                    break;
                case 3:
                    // msg val should be a string
                    MDIMain.MsgBoxWithHelp(
                    msgtext + "Expected string value",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\commands\\syntax_fan.htm#messages");
                    break;
                case 4:
                    // stuff not allowed on line after msg declaration
                    MDIMain.MsgBoxWithHelp(
                    msgtext + "Expected end of line",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\commands\\syntax_fan.htm#messages");
                    break;
                case 5:
                    // missing end quote
                    MDIMain.MsgBoxWithHelp(
                        msgtext + "String is missing end quote mark",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        "htm\\commands\\syntax_fan.htm#messages");
                    break;
                }
                fctb.Selection.Start = errorlocation;
                fctb.Selection.End = new(fctb.GetLineLength(errorlocation.iLine), errorlocation.iLine);
                return;
            }
            if (keepUnused) {
                for (int i = 1; i < 256; i++) {
                    if (Messages[i].Declared) {
                        msgCount++;
                    }
                }
            }
            AGIToken token = new();
            token = NextMsg(token, stringDefList);
            while (token.Type != AGITokenType.None || token.Number < 0) {
                // check for error
                if (token.Number < 0) {
                    fctb.SelectLine(token.Line);
                    string msg = "Syntax error in line " + (token.Line + 1).ToString() + " must be corrected " +
                            "before message cleanup can continue:\n\n";

                    switch (token.Number) {
                    case -1:
                        // missing '(' after command
                        MDIMain.MsgBoxWithHelp(
                            msg + "Expected '(' after command text.",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\commands\\syntax_fan.htm#messages");
                        break;
                    case -2:
                        // missing end quote
                        MDIMain.MsgBoxWithHelp(
                            msg + "Missing quote mark (\") at end of string.",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\commands\\syntax_fan.htm#messages");
                        break;
                    case -3:
                        // arg not a string
                        MDIMain.MsgBoxWithHelp(
                            msg + "Argument is not a string or msg marker ('m##').",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\commands\\syntax_fan.htm#messages");
                        break;
                    case -4:
                        // stuff not allowed after message string
                        MDIMain.MsgBoxWithHelp(
                            msg + "Line break expected after message declaration.",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\commands\\syntax_fan.htm#messages");
                        break;
                    case -5:
                        // missing arg value
                        MDIMain.MsgBoxWithHelp(
                            msg + "Argument value is missing",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\commands\\syntax_fan.htm#messages");
                        break;
                    case -6:
                        // missing comma after arg
                        MDIMain.MsgBoxWithHelp(
                            msg + "Expected ',' after command arguments.",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\commands\\syntax_fan.htm#messages");
                        break;
                    case -7:
                        // invalid string marker
                        MDIMain.MsgBoxWithHelp(
                            msg + "Invalid string marker (must be s0 - s23).",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\commands\\syntax_fan.htm#messages");
                        break;
                    case -8:
                        // invalid message marker
                        MDIMain.MsgBoxWithHelp(
                            msg + "Invalid message marker (must be m1 - m255).",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information,
                            "htm\\commands\\syntax_fan.htm#messages");
                        break;
                    }
                    return;
                }
                else {
                    // return values are string("text"), marker (m1) or define (astring)
                    int j = 0;
                    if (token.Text[0] == 'm') {
                        // it might be a msg marker
                        if (int.TryParse(token.Text[1..], out j)) {
                            if (j == 0 || j > 255) {
                                // error! invalid message marker
                                fctb.SelectLine(token.Line);
                                msgtext = "Syntax error in line " + token.Line + " must be corrected " +
                                         "before message cleanup can continue:\n\n";
                                MDIMain.MsgBoxWithHelp(
                                    msgtext + "Invalid message marker value (must be 'm1' - 'm255')",
                                    "Syntax Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information,
                                    "htm\\commands\\syntax_fan.htm#messages");
                                return;
                            }
                            if (Messages[j].Declared) {
                                Messages[j].ByNumber = true;
                            }
                            else {
                                // error! this msg isn't defined
                                fctb.SelectLine(token.Line);
                                msgtext = "Syntax error in line " + token.Line + " must be corrected " +
                                         "before message cleanup can continue:\n\n";
                                MDIMain.MsgBoxWithHelp(
                                    msgtext + "'" + token.Text + "' is not a declared message.",
                                    "Syntax Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information,
                                    "htm\\commands\\syntax_fan.htm#messages");
                                return;
                            }
                        }
                    }
                    if (j == 0) {
                        // check this string/define against list of declared messages
                        for (j = 1; j < 256; j++) {
                            if (Messages[j].Declared) {
                                if (Messages[j].Text == token.Text) {
                                    // if not yet marked as inuse by ref
                                    if (!Messages[j].ByRef) {
                                        // mark as used by reference
                                        Messages[j].ByRef = true;
                                        // if not keeping all declared, need to increment the count
                                        if (!keepUnused) {
                                            // increment msgcount
                                            msgCount++;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        if (j == 256) {
                            // check this msg against new message collection
                            for (j = 0; j < NewMsgs.Length; j++) {
                                if (NewMsgs[j] == token.Text) {
                                    break;
                                }
                            }
                            // if still not found (unique to both)
                            if (j == NewMsgs.Length) {
                                // add to new msg list
                                Array.Resize(ref NewMsgs, NewMsgs.Length + 1);
                                NewMsgs[^1] = token.Text;
                                // increment Count
                                msgCount++;
                            }
                        }
                        if (msgCount >= 256) {
                            MDIMain.MsgBoxWithHelp(
                                "There are too many messages being used by this logic. AGI only " +
                                "supports 255 messages per logic. Edit the logic to reduce the " +
                                "number of messages to 255 or less.",
                                "Too Many Messages",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation,
                                "htm\\agi\\logics.htm#messages");
                            return;
                        }
                    }
                }
                token = NextMsg(token, stringDefList);
            }

            // Now add all newfound messages to the message array
            int newmsgnum = 1;
            for (int i = 0; i < NewMsgs.Length; i++) {
                // if message is not in use (byref or bynum), we can overwrite it
                do
                    if (keepUnused && Messages[newmsgnum].Declared) {
                        // if keeping declared, skip if this msg is already declared
                        newmsgnum++;
                    }
                    else if (Messages[newmsgnum].ByNumber || Messages[newmsgnum].ByRef) {
                        // skip if this msg is in use (byref or bynum)
                        newmsgnum++;
                    }
                    else {
                        // this number can be used for a new msg
                        break;
                    }
                while (newmsgnum < 256);
                Messages[newmsgnum].Text = NewMsgs[i];
                // mark it as in use by ref
                Messages[newmsgnum].ByRef = true;
                newmsgnum++;
            }
            // now build the message section using all messages that are marked as in use
            // add right after last 'return() command
            int lastline = fctb.LinesCount - 1;
            while (fctb.GetLineLength(lastline) == 0) {
                lastline--;
                if (lastline == 0) {
                    break;
                }
            }
            token = fctb.TokenFromPos(new(fctb.GetLineLength(lastline) - 1, lastline));
            do {
                if (token.Type == AGITokenType.Identifier) {
                    if (token.Text == "return") {
                        break;
                    }
                }
                token = fctb.PreviousToken(token, true);
            } while (token.Type != AGITokenType.None);
            int insertline = token.Line + 1;
            if (token.Type == AGITokenType.None) {
                // just add to end
                insertline = fctb.LinesCount;
            }
            // remove any defines that come BEFORE the insert line
            int removed = 0;
            for (int i = 255; i > 0; i--) {
                if (Messages[i].Declared && Messages[i].Line < insertline) {
                    fctb.RemoveLine(Messages[i].Line);
                    removed++;
                }
            }
            insertline -= removed;
            // and add a comment header
            msgtext = "\n[ **************************************\n[ DECLARED MESSAGES\n[ **************************************\n";
            for (int i = 1; i < 256; i++) {
                // if used by ref or num, OR if keeping all and it's declared,add this msg
                if (Messages[i].ByRef || Messages[i].ByNumber || (Messages[i].Declared && keepUnused)) {
                    msgtext += "#message " + i + " " + Messages[i].Text + "\n";
                }
            }
            // delete everything from here to end of text
            //  and replace with the messages
            start = new(0, insertline);
            end = new(fctb.Lines[fctb.LinesCount - 1].Length, fctb.LinesCount - 1);
            fctb.Selection.Start = start;
            fctb.Selection.End = end;
            fctb.SelectedText = msgtext;
        }

        private void SelectDefine() {
            int i;
            bool cancel = false;
            if (lstDefines.SelectedItems.Count == 0) {
                return;
            }
            if ((string)lstDefines.Tag == "snippets") {
                string snippetvalue = "";
                for (i = 0; i < CodeSnippets.Length; i++) {
                    if (CodeSnippets[i].Name == lstDefines.SelectedItems[0].Text) {
                        snippetvalue = CodeSnippets[i].Value;
                        break;
                    }
                }
                string[] arglist = CodeSnippets[i].ArgTips.Split(',');
                i = 1;
                while (snippetvalue.Contains("%" + i.ToString())) {
                    string argvalue = "";
                    string argprompt = "Enter text for argument #" + i.ToString() + ":";
                    string arginfo;
                    if (arglist.Length >= i) {
                        arginfo = arglist[i - 1];
                    }
                    else {
                        arginfo = "";
                    }
                    cancel = ShowInputDialog(MDIMain, argprompt, arginfo, ref argvalue) != DialogResult.OK;
                    if (!cancel) {
                        snippetvalue = snippetvalue.Replace("%" + i.ToString(), argvalue);
                    }
                    else {
                        break;
                    }
                    i++;
                }
                if (!cancel) {
                    fctb.SelectedText = snippetvalue;
                }
            }
            else {
                fctb.SelectedText = lstDefines.SelectedItems[0].Text;
            }
            fctb.Selection.Start = fctb.Selection.End;
            lstDefines.ShowItemToolTips = false;
            lstDefines.Visible = false;
            fctb.CaretVisible = true;
        }

        /// <summary>
        /// Initializes or updates the displayed fonts used by the editor.
        /// </summary>
        internal void InitFonts() {
            rtfLogic1.ForeColor = WinAGISettings.SyntaxStyle[0].Color.Value;
            rtfLogic1.BackColor = WinAGISettings.EditorBackColor.Value;
            int red = 255 - WinAGISettings.EditorBackColor.Value.R;
            int green = 255 - WinAGISettings.EditorBackColor.Value.G;
            int blue = 255 - WinAGISettings.EditorBackColor.Value.B;
            rtfLogic1.SelectionColor = Color.FromArgb(128, red, green, blue);
            rtfLogic1.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            rtfLogic1.DefaultStyle = new TextStyle(rtfLogic1.DefaultStyle.ForeBrush, rtfLogic1.DefaultStyle.BackgroundBrush, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            // always re-register styles for the text boxes
            RegisterStyles(rtfLogic1);
            rtfLogic1.ClearStylesBuffer();
            if ((FormMode == LogicFormMode.Logic && WinAGISettings.HighlightLogic.Value) ||
                (FormMode == LogicFormMode.Text && WinAGISettings.HighlightText.Value)) {
                AGISyntaxHighlight(rtfLogic1.Range);
            }
            rtfLogic1.TabLength = WinAGISettings.LogicTabWidth.Value;
            rtfLogic2.ForeColor = WinAGISettings.SyntaxStyle[0].Color.Value;
            rtfLogic2.BackColor = WinAGISettings.EditorBackColor.Value;
            rtfLogic2.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            rtfLogic2.DefaultStyle = new TextStyle(rtfLogic2.DefaultStyle.ForeBrush, rtfLogic2.DefaultStyle.BackgroundBrush, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            // always re-register styles for the text boxes
            RegisterStyles(rtfLogic2);
            if ((FormMode == LogicFormMode.Logic && WinAGISettings.HighlightLogic.Value) ||
                (FormMode == LogicFormMode.Text && WinAGISettings.HighlightText.Value)) {
                AGISyntaxHighlight(rtfLogic2.Range);
            }
            rtfLogic2.TabLength = WinAGISettings.LogicTabWidth.Value;
            documentMap1.BackColor = WinAGISettings.EditorBackColor.Value;
            lstDefines.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            lstDefines.Height = 6 * fctb.CharHeight;
            lstDefines.Width = 20 * fctb.CharWidth;
            lstDefines.Columns[0].Width = lstDefines.Width;
        }

        private void AGISyntaxHighlight(FastColoredTextBoxNS.Range changedRange) {
            bool allowMultilineStrings = EditGame != null && EditGame.SierraSyntax;
            bool inMultilineString = false;
            int mlStringStartLine = -1;
            int mlStringStartChar = -1;

            // 0. Early exits
            if ((FormMode != LogicFormMode.Logic || !WinAGISettings.HighlightLogic.Value) &&
                (FormMode != LogicFormMode.Text || !WinAGISettings.HighlightText.Value))
                return;

            if (changedRange.Length == 0)
                return;

            var tb = changedRange.tb;

            // 1. Clear only our styles, not everything
            changedRange.ClearStyle(
                CommentStyle,
                StringStyle,
                KeyWordStyle,
                TestCmdStyle,
                ActionCmdStyle,
                InvalidCmdStyle,
                NumberStyle,
                ArgIdentifierStyle,
                SymbolStyle,
                DefIdentifierStyle
            );

            // 2. Collect protected regions (comments + strings)
            var protectedRanges = new List<FastColoredTextBoxNS.Range>();

            // simple overlap helper (single-line ranges, which is true for our comments/strings)
            bool Overlaps(FastColoredTextBoxNS.Range a, FastColoredTextBoxNS.Range b) {
                if (a.Start.iLine != b.Start.iLine)
                    return false;

                return !(a.End.iChar <= b.Start.iChar || a.Start.iChar >= b.End.iChar);
            }

            for (int line = changedRange.Start.iLine; line <= changedRange.End.iLine && line < tb.LinesCount; line++) {
                string text = tb.Lines[line];
                if (string.IsNullOrEmpty(text)) {
                    // If we're inside a multiline string, the entire empty line is part of it
                    if (inMultilineString) {
                        var r = new FastColoredTextBoxNS.Range(tb, 0, line, 0, line);
                        r.SetStyle(StringStyle);
                        protectedRanges.Add(r);
                    }
                    continue;
                }

                int length = text.Length;
                int commentStart = -1;

                bool inString = inMultilineString;   // carry state across lines
                bool escaped = false;
                int iStringStart = inString ? mlStringStartChar : -1;

                int i = 0;
                while (i < length) {
                    char c = text[i];

                    if (inString) {
                        if (!escaped && c == '"') {
                            // END of string
                            int stringEnd = i + 1;

                            var r = new FastColoredTextBoxNS.Range(
                                tb,
                                iStringStart,
                                mlStringStartLine,
                                stringEnd,
                                line
                            );
                            r.SetStyle(StringStyle);
                            protectedRanges.Add(r);

                            inString = false;
                            inMultilineString = false;
                            escaped = false;
                            i++;
                            continue;
                        }

                        if (!escaped && c == '\\')
                            escaped = true;
                        else
                            escaped = false;

                        i++;
                        continue;
                    }

                    // not in string
                    if (c == '"') {
                        inString = true;
                        escaped = false;
                        iStringStart = i;
                        mlStringStartLine = line;
                        mlStringStartChar = i;
                        i++;
                        continue;
                    }

                    // comment start (only when not in string)
                    if (c == '[') {
                        commentStart = i;
                        break;
                    }

                    if (c == '/' && i + 1 < length && text[i + 1] == '/') {
                        commentStart = i;
                        break;
                    }

                    i++;
                }

                // comment range (if any)
                if (commentStart >= 0) {
                    var r = new FastColoredTextBoxNS.Range(tb, commentStart, line, length, line);
                    r.SetStyle(CommentStyle);
                    protectedRanges.Add(r);
                }

                // If we ended the line still inside a string
                if (inString) {
                    if (allowMultilineStrings) {
                        // mark entire remainder of line as string
                        var r = new FastColoredTextBoxNS.Range(tb, iStringStart, line, length, line);
                        r.SetStyle(StringStyle);
                        protectedRanges.Add(r);

                        // continue string on next line
                        inMultilineString = true;
                        mlStringStartChar = 0; // next line starts at char 0
                    }
                    else {
                        // single-line mode: string ends at EOL
                        var r = new FastColoredTextBoxNS.Range(tb, iStringStart, line, length, line);
                        r.SetStyle(StringStyle);
                        protectedRanges.Add(r);
                    }
                }
            }

            bool IsInsideProtected(FastColoredTextBoxNS.Range r) {
                foreach (var p in protectedRanges) {
                    if (Overlaps(r, p))
                        return true;
                }
                return false;
            }

            // 3. Apply other token styles, skipping protected regions

            if (EditGame is not null && EditGame.SierraSyntax) {
                foreach (var r in changedRange.GetRanges(SierraKeyWordStyleRegEx))
                    if (!IsInsideProtected(r))
                        r.SetStyle(KeyWordStyle);
            }
            else {
                foreach (var r in changedRange.GetRanges(FanKeyWordStyleRegEx))
                    if (!IsInsideProtected(r))
                        r.SetStyle(KeyWordStyle);

                foreach (var r in changedRange.GetRanges(TestCmdStyleRegEx))
                    if (!IsInsideProtected(r))
                        r.SetStyle(TestCmdStyle);

                foreach (var r in changedRange.GetRanges(ActionCmdStyleRegEx))
                    if (!IsInsideProtected(r))
                        r.SetStyle(ActionCmdStyle);

                foreach (var r in changedRange.GetRanges(InvalidCmdStyleRegEx))
                    if (!IsInsideProtected(r))
                        r.SetStyle(InvalidCmdStyle);
            }

            foreach (var r in changedRange.GetRanges(NumberStyleRegEx))
                if (!IsInsideProtected(r))
                    r.SetStyle(NumberStyle);

            foreach (var r in changedRange.GetRanges(ArgIdentifierStyleRegEx))
                if (!IsInsideProtected(r))
                    r.SetStyle(ArgIdentifierStyle);

            foreach (var r in changedRange.GetRanges(SymbolStyleRegEx))
                if (!IsInsideProtected(r))
                    r.SetStyle(SymbolStyle);

            foreach (var r in changedRange.GetRanges(DefIdentifierStyleRegEx))
                if (!IsInsideProtected(r))
                    r.SetStyle(DefIdentifierStyle);

            // 4. Folding markers
            changedRange.ClearFoldingMarkers();
            changedRange.SetFoldingMarkers("{", "}");
        }

        private static void RegisterStyles(FastColoredTextBox tb) {
            // Clear any previously registered styles (safe because we re-add them immediately)
            tb.ClearStylesBuffer();

            // Register styles in the exact order we want them layered.
            // Lower index = weaker, higher index = stronger.

            tb.AddStyle(StringStyle);
            tb.AddStyle(CommentStyle);
            tb.AddStyle(KeyWordStyle);
            tb.AddStyle(ActionCmdStyle);
            tb.AddStyle(TestCmdStyle);
            tb.AddStyle(InvalidCmdStyle);
            tb.AddStyle(SymbolStyle);
            tb.AddStyle(NumberStyle);
            tb.AddStyle(ArgIdentifierStyle);
            tb.AddStyle(DefIdentifierStyle);
        }

        internal bool LoadLogic(Logic loadlogic, bool quiet = false) {
            InGame = loadlogic.InGame;
            int codepage;
            if (InGame) {
                LogicNumber = loadlogic.Number;
                codepage = EditLogic.CodePage;
            }
            else {
                // use a number that can never match
                // when searches for open logics are made
                LogicNumber = 256;
                // and WinAGI default codepage
                codepage = CodePage;
            }
            try {
                loadlogic.Load();
            }
            catch (Exception ex) {
                // unhandled error
                if (!quiet) {
                    string resid = InGame ? "Logic " + LogicNumber : loadlogic.ID;
                    ErrMsgBox(ex,
                        "Something went wrong. Unable to load " + resid,
                        ex.StackTrace,
                        "Load Logic Failed");
                }
                return false;
            }
            if (loadlogic.SourceError != ResourceErrorType.NoError) {
                if (!quiet) {
                    // messages are same for ingame/standalone logic sources
                    switch (loadlogic.SourceError) {
                    case ResourceErrorType.LogicSourceIsReadonly:
                        MessageBox.Show(MDIMain,
                            "Source file for this logic is tagged as readonly. WinAGI " +
                            "needs full access to edit source files.",
                            "Logic Source Is Readonly",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return false;
                    case ResourceErrorType.LogicSourceAccessError:
                        MessageBox.Show(MDIMain,
                            "Unable to open and read the source file for this logic.",
                            "Logic Source File Access Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return false;
                    case ResourceErrorType.LogicSourceDecompileError:
                        MessageBox.Show(MDIMain,
                            "Errors were encountered when decompiling this logic. " +
                            "The logic may be corrupt. Check the output carefully and make" +
                            "any corrections as needed.",
                            "Logic Decompilation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        break;
                    }
                }
            }
            Compiled = loadlogic.Compiled;
            EditLogic = loadlogic.Clone();
            // to ensure any non-supported characters for the current codepage
            // are converted to '?', we need to convert the text to bytes, then 
            // back to text
            rtfLogic1.Text = Encoding.GetEncoding(codepage).GetString(Encoding.GetEncoding(codepage).GetBytes(EditLogic.SourceText));
            rtfLogic1.ClearUndo();
            if (EditLogic.SourceFile.Length != 0) {
                rtfLogic1.IsChanged = EditLogic.SourceChanged;
            }
            else {
                LogCount++;
                EditLogic.ID = "NewLogic" + LogCount;
                rtfLogic1.IsChanged = true;
            }
            // use ID for ingame; filename for not (logics don't
            // assign file name to ID like the other resources)
            if (InGame) {
                Text = LOGIC_EDITOR + ResourceName(EditLogic, true, true);
                // msg cleanup not available if in SierraSyntax
                btnMsgClean.Enabled = mnuRMsgCleanup.Enabled = !EditGame.SierraSyntax;
            }
            else {
                // get file name from SourceFile (if this is a source file)
                // or from ResFile (if this is a compiled logic file)
                // or from ID (if a new logic)
                Text = LOGIC_EDITOR;
                if (EditLogic.SourceFile.Length > 0) {
                    Text += CompactPath(EditLogic.SourceFile, 75);
                }
                else if (EditLogic.ResFile.Length > 0) {
                    string newname = Path.Combine(Path.GetDirectoryName(EditLogic.ResFile), Path.GetFileNameWithoutExtension(EditLogic.ResFile));
                    Text += CompactPath(newname, 75);
                }
                else {
                    Text += EditLogic.ID;
                }
                // no compiling if not ingame
                btnCompile.Enabled = mnuRCompile.Enabled = false;
            }
            // set IsChanged base on rtfLogic change state
            IsChanged = rtfLogic1.IsChanged;
            if (IsChanged) {
                Text = CHG_MARKER + Text;
            }
            mnuRSave.Enabled = !IsChanged;
            MDIMain.btnSaveResource.Enabled = !IsChanged;
            if (WinAGISettings.MaximizeLogics.Value) {
                WindowState = FormWindowState.Maximized;
            }
            // cache warning/error info
            if (InGame) {
                WarningList = EditGame.Logics[LogicNumber].LoadWarnings();
                if (MDIMain.infoGridScope == InfoGridScope.OpenResources) {
                    MDIMain.RefreshInfoGrid();
                }
            }
            // force highlight
            AGISyntaxHighlight(fctb.Range);
            loading = false;
            return true;
        }

        internal bool LoadText(string filename, bool quiet = false) {
            LogicNumber = 256;
            if (filename.Length > 0) {
                if (File.Exists(filename)) {
                    try {
                        rtfLogic1.OpenFile(filename);
                        TextFilename = filename;
                    }
                    catch (Exception ex) {
                        if (!quiet) {
                            ErrMsgBox(ex,
                                "Something went wrong. Unable to load " + filename,
                                ex.StackTrace,
                                "Load Text File Failed");
                        }
                        return false;
                    }
                }
                else {
                    if (!quiet) {
                        MessageBox.Show("Unable to load " + filename + ". File not found.",
                            "File Not Found",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    return false;
                }
                // check if this is an include file (only 'other' files; if user opens a reserved
                // file, it will be treated as a normal text file)
                if (EditGame is null) {
                    InGame = false;
                    IncludeIndex = -1;
                }
                else {
                    bool notfound = true;
                    for (int i = 0; i < EditGame.IncludeFiles.Count; i++) {
                        if (EditGame.IncludeFiles[i].Type == IncludeType.Other &&
                            EditGame.IncludeFiles[i].Filename.Equals(filename, StringComparison.OrdinalIgnoreCase)) {
                            InGame = true;
                            IncludeIndex = i;
                            notfound = false;
                            break;
                        }
                    }
                    if (notfound) {
                        InGame = false;
                        IncludeIndex = -1;
                    }
                }
                Text = "Text Editor - " + CompactPath(filename, 75);
            }
            else {
                TextCount++;
                Text = "Text Editor - " + "NewTextFile" + TextCount.ToString();
                InGame = false;
                IncludeIndex = -1;
            }
            rtfLogic1.ClearUndo();
            IsChanged = rtfLogic1.IsChanged = filename.Length == 0;
            if (IsChanged) {
                Text = CHG_MARKER + Text;
            }
            mnuRSave.Enabled = !IsChanged;
            MDIMain.btnSaveResource.Enabled = !IsChanged;
            MDIMain.btnAddRemove.Enabled = TextFilename.Length > 0;
            // maximize, if that's the current setting
            if (WinAGISettings.MaximizeLogics.Value) {
                WindowState = FormWindowState.Maximized;
            }
            if (InGame) {
                if (MDIMain.infoGridScope == InfoGridScope.OpenResources) {
                    MDIMain.RefreshInfoGrid();
                }
            }
            // force highlight
            AGISyntaxHighlight(fctb.Range);
            loading = false;
            return true;
        }

        private static string NewTextFileName(string filename = "") {
            DialogResult rtn;

            if (filename.Length != 0) {
                MDIMain.SaveDlg.Title = "Save Text File";
                MDIMain.SaveDlg.FileName = Path.GetFileName(filename);
            }
            else {
                MDIMain.SaveDlg.Title = "Save Text File As";
                MDIMain.SaveDlg.FileName = "";
            }
            MDIMain.SaveDlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            MDIMain.SaveDlg.FilterIndex = 1;
            MDIMain.SaveDlg.DefaultExt = "txt";
            MDIMain.SaveDlg.CheckPathExists = true;
            MDIMain.SaveDlg.ExpandedMode = true;
            MDIMain.SaveDlg.ShowHiddenFiles = false;
            MDIMain.SaveDlg.OverwritePrompt = true;
            MDIMain.SaveDlg.OkRequiresInteraction = true;
            MDIMain.SaveDlg.InitialDirectory = DefaultResDir;
            rtn = MDIMain.SaveDlg.ShowDialog(MDIMain);
            if (rtn == DialogResult.Cancel) {
                // nothing selected
                return "";
            }
            DefaultResDir = Path.GetDirectoryName(MDIMain.SaveDlg.FileName);
            return MDIMain.SaveDlg.FileName;
        }

        internal void ImportLogic(string importfile) {
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(importfile));
            if (!File.Exists(importfile)) {
                throw new FileNotFoundException("Import file not found", importfile);
            }
            string filetext = "";

            MDIMain.UseWaitCursor = true;
            Logic tmpLogic = new();
            // open file to see if it is sourcecode or compiled logic
            try {
                filetext = File.ReadAllText(importfile);
            }
            catch (Exception) {
                // ignore errors
            }
            // check if logic is a compiled logic
            // (check for existence of char '0')
            bool isSource = !filetext.Contains('\0');
            // import the logic (and check for error)
            if (isSource) {
                tmpLogic.ImportSource(importfile);
                if (tmpLogic.SourceError != ResourceErrorType.NoError) {
                    MDIMain.UseWaitCursor = false;
                    switch (tmpLogic.SourceError) {
                    case ResourceErrorType.LogicSourceIsReadonly:
                        MessageBox.Show(MDIMain,
                            "This logic source file is marked 'readonly'. WinAGI requires write-access to edit source files.",
                            "Read only Files not Allowed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    case ResourceErrorType.LogicSourceAccessError:
                        MessageBox.Show(MDIMain,
                            "A file access error has occurred. Unable to read this file.",
                            "File Access Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                    return;
                }
            }
            else {
                try {
                    tmpLogic.Import(importfile);
                    if (tmpLogic.SourceError != ResourceErrorType.NoError) {
                        MDIMain.UseWaitCursor = false;
                        MessageBox.Show(MDIMain,
                            "Errors were encountered when decompiling this logic. " +
                            "The logic may be corrupt. Check the output carefully and make" +
                            "any corrections as needed.",
                            "Logic Decompilation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex) {
                    MDIMain.UseWaitCursor = false;
                    ErrMsgBox(ex,
                        "Unable to load this logic resource. It can't be decompiled, " +
                        "and does not appear to be a text file.",
                        ex.StackTrace,
                        "Invalid Logic Resource");
                    return;
                }
            }
            fctb.Text = tmpLogic.SourceText;
            UpdateAutoIncludes();
            EditLogic.SourceText = fctb.Text;
            MDIMain.UseWaitCursor = false;
        }

        internal void SaveLogicSource() {
            // saves the source code to file; to save the 
            // AGI resource, use the Compile method
            MDIMain.UseWaitCursor = true;
            saving = true;
            if (InGame) {
                //   - copy editor text to GAME LOGIC source
                //   if a room and using layout editor
                //      - update source for room references
                //      - copy updated source back to editor
                //   -save source

                // unlike other resources, the ingame logic is referenced directly
                // when being edited; so, it's possible that the logic might get closed
                // such as when changing which logic is being previewed;
                // SO, we need to make sure the logic is loaded BEFORE saving
                bool loaded = EditGame.Logics[LogicNumber].Loaded;
                if (!loaded) {
                    EditGame.Logics[LogicNumber].Load();
                }
                // before saving, update the auto-include files with any changes that
                // might have been made to the editor text
                UpdateAutoIncludes();
                EditGame.Logics[LogicNumber].SourceText = fctb.Text;

                if (EditGame.UseLE && EditLogic.IsRoom) {
                    // need to update the layout editor and the layout data file with new exit info
                    // (this also updates the logic source by adding/updating layout tags)
                    UpdateExitInfo(UpdateReason.UpdateRoom, LogicNumber, EditGame.Logics[LogicNumber]);
                }
                if (!Compiling) {
                    // update and save warning/error info for this logic
                    WarningList = RefreshLogicWarnings((byte)LogicNumber);
                    RefreshModWarnings(false);
                }
                // use the ingame logic to save the SOURCE
                EditGame.Logics[LogicNumber].SaveSource();
                if (EditLogic.IsRoom && EditGame.Logics[LogicNumber].SourceText != fctb.Text) {
                    // update the editor to match
                    var lines = EditGame.Logics[LogicNumber].SourceText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
                    Place start = fctb.Selection.Start;
                    Place end = fctb.Selection.End;
                    fctb.BeginAutoUndo();
                    for (int i = lines.Length - 1; i >= 0; i--) {
                        if (lines[i] != fctb.Lines[i]) {
                            var oldLine = fctb.Lines[i];
                            fctb.Selection.Start = new Place(0, i);
                            fctb.Selection.End = new Place(oldLine.Length, i);
                            fctb.InsertText(lines[i]);
                        }
                    }
                    fctb.EndAutoUndo();
                    fctb.Selection.Start = start;
                    fctb.Selection.End = end;
                }
                if (!loaded) {
                    EditGame.Logics[LogicNumber].Unload();
                }
                // remove existing TODO items and decompiler warnings for this logic
                MDIMain.ClearInfoGrid(AGIResType.Logic, (byte)LogicNumber, [EventType.TODO, EventType.DecompWarning]);

                // update TODO list and decomp warnings for this logic
                List<WinAGIEventInfo> TODOs = ExtractTODO((byte)LogicNumber, rtfLogic1.Text, EditLogic.ID);
                foreach (WinAGIEventInfo tmpInfo in TODOs) {
                    MDIMain.AddInfoItem(tmpInfo);
                }
                // check for Decompile warnings
                List<WinAGIEventInfo> DecompWarnings = ExtractDecompWarn((byte)LogicNumber, rtfLogic1.Text, EditLogic.ID);
                foreach (WinAGIEventInfo tmpInfo in DecompWarnings) {
                    MDIMain.AddInfoItem(tmpInfo);
                }
                MDIMain.UpdateGridCounts();
            }
            else if (EditLogic.SourceFile.Length != 0) {
                // preserve all extended characters by using default codepage
                rtfLogic1.SaveToFile(EditLogic.SourceFile, Encoding.Default);
            }
            else {
                // not in a game, and no sourcefile assigned yet -
                MDIMain.UseWaitCursor = false;
                ExportLogic();
                return;
            }
            UpdateStatusBar();
            MarkAsSaved();
            if (InGame && !Compiling) {
                RefreshTree(AGIResType.Logic, LogicNumber);
            }
            if (InGame) {
                EditGame.agGameProps.Save();
            }
            MDIMain.UseWaitCursor = false;
            saving = false;
        }

        internal void SaveTextFile(string filename = "") {
            if (filename.Length == 0) {
                if (TextFilename.Length == 0) {
                    filename = Path.Combine(DefaultResDir, "NewTextFile" + TextCount.ToString() + ".txt");
                }
                else {
                    filename = TextFilename;
                }
                filename = NewTextFileName(filename);
                if (filename.Length == 0) {
                    return;
                }
            }
            // preserve all extended characters by using default codepage
            rtfLogic1.SaveToFile(filename, Encoding.Default);
            TextFilename = filename;
            // adding now allowed
            MDIMain.btnAddRemove.Enabled = true;
            // if ingame, force lookup refresh
            if (InGame) {
                foreach (var form in LogicEditors) {
                    form.DefChanged = true;
                    form.ListChanged = true;
                }
                Debug.Assert(IncludeDefines.ContainsKey(TextFilename));
                IncludeDefines[TextFilename].IsChanged = true;
            }
            MarkAsSaved();
            if (InGame) {
                // get index of this file in the IncludeFiles list
                int index = -1;
                for (int i = 0; i < EditGame.IncludeFiles.Count; i++) {
                    if (EditGame.IncludeFiles[i].Filename.Equals(
                        TextFilename, StringComparison.OrdinalIgnoreCase)) {
                        index = i;
                        break;
                    }
                }
                // if being previewed, refresh it
                if (SelResType == AGIResType.Include && index == SelResNum) {
                    // redraw the preview
                    PreviewWin.LoadPreview(SelResType, SelResNum);
                }

            }
        }

        internal void ExportLogic() {
            // export logic or source

            // logics that are NOT in a game can't export the actual logic
            // resource; they only export the source code (which is functionally
            // equivalent to 'save as'

            // update sourcecode with current logic text
            EditLogic.SourceText = rtfLogic1.Text;
            if (InGame) {
                // MUST make sure logic sourcefile is set to correct Value
                // BEFORE calling exporting; this is because EditLogic is NOT
                // in a game; it only mimics the ingame resource
                EditLogic.SourceFile = EditGame.Logics[LogicNumber].SourceFile;
                if (EditGame.Logics[LogicNumber].CompiledCRC != EditLogic.CRC) {
                    if (MessageBox.Show(MDIMain,
                        "Source code has changed. Do you want to compile before exporting this logic?",
                        "Export Logic",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes) {
                        if (!CompileLogic(this, (byte)LogicNumber)) {
                            MessageBox.Show(MDIMain,
                                "Compile error; Unable to export the logic resource.",
                                "Compiler error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
                if (Base.ExportLogic(EditLogic, true, false) == 1) {
                    // because EditLogic is not the actual ingame logic its
                    // ID needs to be reset back to the ingame value
                    EditLogic.ID = EditGame.Logics[LogicNumber].ID;
                }
                UpdateStatusBar();
            }
            else {
                // not in game; save-as is only operation allowed
                string exportName = NewSourceName(EditLogic, InGame);
                if (exportName.Length != 0) {
                    EditLogic.SourceFile = exportName;
                    // preserve all extended characters by using default codepage
                    rtfLogic1.SaveToFile(EditLogic.SourceFile, Encoding.Default);
                    EditLogic.ID = Path.GetFileName(exportName);
                    MarkAsSaved();
                }
            }
        }

        internal void ToggleInGame() {
            // toggles the ingame state of a logic or include file

            DialogResult rtn;
            string exportName;
            bool dontAsk = false;

            switch (FormMode) {
            case LogicFormMode.Logic:
                if (EditLogic.SourceFile.Length == 0) {
                    break;
                }
                if (InGame) {
                    if (WinAGISettings.AskExport.Value) {
                        rtn = MsgBoxEx.Show(MDIMain,
                            "Do you want to export '" + EditLogic.ID + "' source before removing it from your game?",
                            "Don't ask this question again",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            "Export Logic Before Removal", ref dontAsk);
                        WinAGISettings.AskExport.Value = !dontAsk;
                        if (!WinAGISettings.AskExport.Value) {
                            WinAGISettings.AskExport.WriteSetting(WinAGISettingsFile);
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
                        // export source - 
                        // MUST make sure logic sourcefile is set to correct Value
                        // BEFORE calling exporting; this is because EditLogic is NOT
                        // in a game; it only mimics the ingame resource
                        EditLogic.SourceFile = Path.Combine(EditGame.SrcResDir, EditLogic.ID + "." + EditGame.SourceExt);
                        exportName = NewSourceName(EditLogic, InGame);
                        if (exportName.Length > 0) {
                            // preserve all extended characters by using default codepage
                            rtfLogic1.SaveToFile(exportName, Encoding.Default);
                            UpdateStatusBar();
                        }
                        break;
                    case DialogResult.No:
                        // nothing to do
                        break;
                    }
                    if (WinAGISettings.AskRemove.Value) {
                        rtn = MsgBoxEx.Show(MDIMain,
                            "Removing '" + EditLogic.ID + "' from your game.\n\nSelect OK to proceed, or Cancel to keep it in game.",
                            "Remove Logic From Game",
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
                    // remove the logic (force-closes this editor)
                    RemoveLogic((byte)LogicNumber);
                    return;
                }
                else {
                    // add to game
                    string id;
                    if (EditLogic.SourceFile.Length > 0) {
                        id = Path.GetFileNameWithoutExtension(EditLogic.SourceFile);
                    }
                    else {
                        id = "";
                    }
                    using (frmGetResourceNum frmGetNum = new(GetRes.AddInGame, AGIResType.Logic, id)) {
                        if (frmGetNum.ShowDialog(MDIMain) != DialogResult.Cancel) {
                            LogicNumber = frmGetNum.NewResNum;
                            // change id before adding to game
                            EditLogic.ID = frmGetNum.txtID.Text;
                            UpdateAutoIncludes();
                            // copy text back into sourcecode
                            EditLogic.SourceText = fctb.Text;
                            // always import logics as non-room;
                            // user can always change it later via the
                            // InRoom property
                            // add Logic (which saves the source file ot resdir)
                            AddNewLogic((byte)LogicNumber, EditLogic);
                            EditGame.Logics[LogicNumber].Load();
                            // copy the Logic back (to ensure internal variables are copied)
                            EditLogic = EditGame.Logics[LogicNumber].Clone();
                            // now we can unload the newly added logic;
                            EditGame.Logics[LogicNumber].Unload();
                            InGame = true;
                            MarkAsSaved();
                            MDIMain.btnAddRemove.Image = EditorResources.tbRemove;
                            MDIMain.btnAddRemove.Text = "Remove Logic";
                            UpdateExitInfo(UpdateReason.ShowRoom, LogicNumber, EditLogic);
                        }
                    }
                }
                btnCompile.Enabled = InGame;
                btnMsgClean.Enabled = InGame;
                break;
            case LogicFormMode.Text:
                if (TextFilename.Length == 0) {
                    break;
                }
                if (InGame) {
                    // remove it
                    // TODO: confirm first?
                    var includeinfo = EditGame.IncludeFiles.Find(m =>
                        m.Filename.Equals(TextFilename, StringComparison.OrdinalIgnoreCase));
                    Debug.Assert(includeinfo.Filename != "");
                    Debug.Assert(includeinfo.Type != IncludeType.ResourceIDs);
                    Debug.Assert(includeinfo.Type != IncludeType.Reserved);
                    Debug.Assert(includeinfo.Type != IncludeType.Globals);
                    IncludeDefines.Remove(TextFilename);
                    EditGame.IncludeFiles.Remove(includeinfo);
                    MDIMain.btnAddRemove.Image = EditorResources.tbAdd;
                    MDIMain.btnAddRemove.Text = "Add Include";
                    // rebuild include file list in tree (because numbers won't always align
                    // after deleting a file)
                    MDIMain.RefreshIncludeList();
                    InGame = false;
                }
                else {
                    Debug.Assert(TextFilename.Length > 0);
                    if (TextFilename.Length == 0) {
                        // file not saved yet
                        MessageBox.Show(MDIMain,
                            "No filename has been assigned to this text file, so it can't " +
                            "be added to a game. Save it, then try again.",
                            "No Filename",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                    // add it as an 'other' include file
                    IncludeDefines.Add(TextFilename, new DefineList() { IsChanged = true });
                    EditGame.IncludeFiles.Add(new IncludeInfo() {
                        Type = IncludeType.Other,
                        Filename = TextFilename,
                    });
                    // mark all includes as changed
                    foreach (var include in IncludeDefines) {
                        include.Value.IsChanged = true;
                    }
                    foreach (var form in LogicEditors) {
                        form.DefChanged = true;
                        form.ListChanged = true;
                    }
                    MDIMain.btnAddRemove.Image = EditorResources.tbRemove;
                    MDIMain.btnAddRemove.Text = "Remove Include";
                    MDIMain.RefreshIncludeList();
                    InGame = true;

                }
                break;
            }
        }

        internal void RenumberLogic() {
            string oldid = EditLogic.ID;
            byte NewResNum = GetNewNumber(AGIResType.Logic, (byte)LogicNumber);
            if (NewResNum != LogicNumber) {
                // update ID (it may have changed if using default ID)
                EditLogic.ID = EditGame.Logics[NewResNum].ID;
                LogicNumber = NewResNum;
                Text = LOGIC_EDITOR + ResourceName(EditLogic, InGame, true);
                if (IsChanged) {
                    Text = CHG_MARKER + Text;
                }
                if (EditLogic.ID != oldid) {
                    if (File.Exists(Path.Combine(EditGame.SrcResDir, oldid + "." + EditGame.SourceExt))) {
                        SafeFileMove(Path.Combine(EditGame.SrcResDir, oldid + "." + EditGame.SourceExt), Path.Combine(EditGame.SrcResDir, EditGame.Logics[NewResNum].ID + "." + EditGame.SourceExt), true);
                    }
                }
            }
        }

        /// <summary>
        /// For FAN syntax ingame logics only, this verifies the correct include lines are
        /// at the start of the source code. They should be the first three lines of the
        /// file, following any comment header.<br /><br />
        /// If the lines are not there and they should be, add them. If there
        /// and not needed, remove them.
        /// </summary>
        internal void UpdateAutoIncludes() {
            if (EditGame.SierraSyntax) {
                return;
            }

            int line, insertpos = -1, idpos = -1, reservedpos = -1, globalspos = -1;
            int badidpos = -1, badreservedpos = -1, badglobalspos = -1;
            string includefile;
            Regex goodinclude;
            Regex badinclude;

            for (line = 0; line < fctb.Lines.Count; line++) {
                if (insertpos < 0 && fctb.Lines[line].Trim().Left(1) != "[" && fctb.Lines[line].Trim().Left(2) != "//") {
                    insertpos = line;
                }
                if (fctb.Lines[line].Trim().StartsWith("#include")) {
                    // resourceids.txt
                    includefile = @"resourceids\.txt";
                    goodinclude = new Regex(@"#include\s*(?i)""" + includefile + @"""");
                    badinclude = new Regex(@"(#include|\binclude)\s*(?i)(""" + includefile + @"(?!"")\b|" + includefile + @"""|" + includefile + @"\b)");
                    if (idpos == -1 && goodinclude.Match(fctb.Lines[line]).Success) {
                        idpos = line;
                    }
                    else if (badidpos == -1 && badinclude.Match(fctb.Lines[line]).Success) {
                        badidpos = line;
                    }
                    // reserved.txt
                    includefile = @"reserved\.txt";
                    goodinclude = new Regex(@"#include\s*(?i)""" + includefile + @"""");
                    badinclude = new Regex(@"(#include|\binclude)\s*(?i)(""" + includefile + @"(?!"")\b|" + includefile + @"""|" + includefile + @"\b)");
                    if (reservedpos == -1 && goodinclude.Match(fctb.Lines[line]).Success) {
                        reservedpos = line;
                    }
                    else if (badreservedpos == -1 && badinclude.Match(fctb.Lines[line]).Success) {
                        badreservedpos = line;
                    }
                    // globals.txt
                    includefile = @"globals\.txt";
                    goodinclude = new Regex(@"#include\s*(?i)""" + includefile + @"""");
                    badinclude = new Regex(@"(#include|\binclude)\s*(?i)(""" + includefile + @"(?!"")\b|" + includefile + @"""|" + includefile + @"\b)");
                    if (globalspos == -1 && goodinclude.Match(fctb.Lines[line]).Success) {
                        globalspos = line;
                    }
                    else if (badglobalspos == -1 && badinclude.Match(fctb.Lines[line]).Success) {
                        badglobalspos = line;
                    }

                }
                if (idpos >= 0 && reservedpos >= 0 && globalspos >= 0) {
                    break;
                }
            }
            if (EditGame.IncludeIDs) {
                if (idpos >= 0) {
                    if (idpos != insertpos) {
                        // move it
                        if (reservedpos >= 0 && reservedpos < idpos)
                            reservedpos++;
                        if (badreservedpos >= 0 && badreservedpos < idpos)
                            badreservedpos++;
                        if (globalspos >= 0 && globalspos < idpos)
                            globalspos++;
                        if (badglobalspos >= 0 && badglobalspos < idpos)
                            badglobalspos++;
                        fctb.InsertLine(insertpos, fctb.Lines[idpos]);
                        fctb.RemoveLine(++idpos);
                    }
                }
                else if (badidpos >= 0) {
                    // move it, but update to correct text
                    if (reservedpos >= 0 && reservedpos < idpos)
                        reservedpos++;
                    if (badreservedpos >= 0 && badreservedpos < idpos)
                        badreservedpos++;
                    if (globalspos >= 0 && globalspos < idpos)
                        globalspos++;
                    if (badglobalspos >= 0 && badglobalspos < idpos)
                        badglobalspos++;
                    fctb.InsertLine(insertpos, "#include \"resourceids.txt\"");
                    fctb.RemoveLine(++badidpos);
                }
                else {
                    // insert it
                    if (reservedpos >= 0)
                        reservedpos++;
                    if (badreservedpos >= 0)
                        reservedpos++;
                    if (globalspos >= 0)
                        globalspos++;
                    if (badglobalspos >= 0)
                        globalspos++;
                    fctb.InsertLine(insertpos, "#include \"resourceids.txt\"");
                }
                insertpos++;
            }
            else {
                if (idpos >= 0) {
                    if (reservedpos > idpos)
                        reservedpos--;
                    if (badreservedpos > idpos)
                        reservedpos--;
                    if (globalspos > idpos)
                        globalspos--;
                    if (badglobalspos > idpos)
                        globalspos--;
                    fctb.RemoveLine(idpos);
                }
                else if (badidpos >= 0) {
                    if (reservedpos > badidpos)
                        reservedpos--;
                    if (badreservedpos > badidpos)
                        reservedpos--;
                    if (globalspos > badidpos)
                        globalspos--;
                    if (badglobalspos > badidpos)
                        globalspos--;
                    fctb.RemoveLine(badidpos);
                }
            }
            if (EditGame.IncludeReserved) {
                if (reservedpos >= 0) {
                    if (reservedpos != insertpos) {
                        // move it
                        if (globalspos >= 0 && globalspos < reservedpos)
                            globalspos++;
                        if (badglobalspos >= 0 && badglobalspos < reservedpos)
                            badglobalspos++;
                        fctb.InsertLine(insertpos, fctb.Lines[reservedpos]);
                        fctb.RemoveLine(++reservedpos);
                    }
                }
                else if (badreservedpos >= 0) {
                    // move it, but update to correct text
                    if (globalspos >= 0 && globalspos < badreservedpos)
                        globalspos++;
                    if (badglobalspos >= 0 && badglobalspos < badreservedpos)
                        badglobalspos++;
                    fctb.InsertLine(insertpos, "#include \"reserved.txt\"");
                    fctb.RemoveLine(++badreservedpos);
                }
                else {
                    // insert it
                    if (globalspos >= 0)
                        globalspos++;
                    if (badglobalspos >= 0)
                        globalspos++;
                    fctb.InsertLine(insertpos, "#include \"reserved.txt\"");
                }
                insertpos++;
            }
            else {
                if (reservedpos >= 0) {
                    if (globalspos > reservedpos)
                        globalspos--;
                    if (badglobalspos > reservedpos)
                        badglobalspos--;
                    fctb.RemoveLine(reservedpos);
                }
                else if (badreservedpos >= 0) {
                    if (globalspos > badreservedpos)
                        globalspos--;
                    if (badglobalspos > badreservedpos)
                        badglobalspos--;
                    fctb.RemoveLine(badreservedpos);
                }
            }
            if (EditGame.IncludeGlobals) {
                if (globalspos >= 0) {
                    if (globalspos != insertpos) {
                        // move it
                        fctb.InsertLine(insertpos, fctb.Lines[globalspos]);
                        fctb.RemoveLine(++globalspos);
                    }
                }
                else if (badglobalspos >= 0) {
                    // move it, but update to correct text
                    fctb.InsertLine(insertpos, "#include \"globals.txt\"");
                    fctb.RemoveLine(++badglobalspos);
                }
                else {
                    // insert it
                    fctb.InsertLine(insertpos, "#include \"globals.txt\"");
                }
            }
            else {
                if (globalspos >= 0) {
                    fctb.RemoveLine(globalspos);
                }
                else if (badglobalspos >= 0) {
                    fctb.RemoveLine(badglobalspos);
                }
            }
        }

        internal void EditLogicProperties(int FirstProp) {
            string id = EditLogic.ID;
            string description = EditLogic.Description;
            if (GetNewResID(AGIResType.Logic, LogicNumber, ref id, ref description, InGame, 1)) {
                UpdateID(id, description);
            }
        }

        internal void UpdateID(string id, string description) {
            if (EditLogic.Description != description) {
                EditLogic.Description = description;
            }
            if (EditLogic.ID != id) {
                EditLogic.ID = id;
                Text = LOGIC_EDITOR + ResourceName(EditLogic, InGame, true);
                if (IsChanged) {
                    Text = CHG_MARKER + Text;
                }
            }
            MDIMain.RefreshPropertyGrid(AGIResType.Logic, LogicNumber);
        }

        /// <summary>
        /// Builds the local list of defines for use by the tooltip 
        /// and showlist functions.
        /// </summary>
        private void BuildLDefLookup() {
            Define tmpDefine = new();
            bool waiting;

            // if cursor is already the wait cursor, we need to
            // NOT restore it after completion; calling function
            // will do that
            waiting = MDIMain.UseWaitCursor;
            if (!waiting) {
                MDIMain.UseWaitCursor = true;
            }
            Includes = [];
            LDefLookup = [];
            // add local defines
            AGIToken token = fctb.NextToken(new(), true);
            while (token.Type != AGITokenType.None) {
                if (EditGame is null || !EditGame.SierraSyntax) {
                    // Fansyntax: only #define or #include
                    switch (token.Type) {
                    case AGITokenType.Identifier:
                        switch (token.Text) {
                        case "#define":
                            // next token is name
                            token = fctb.NextToken(token, false);
                            // valid identifier name required
                            if (token.Type == AGITokenType.Identifier) {
                                tmpDefine.Name = token.Text;
                                // next token is value
                                token = fctb.NextToken(token, false);
                                switch (token.Type) {
                                case AGITokenType.String:
                                    tmpDefine.Value = token.Text;
                                    tmpDefine.Type = ArgType.DefStr;
                                    LDefLookup.Add(tmpDefine);
                                    break;
                                case AGITokenType.Number:
                                    tmpDefine.Value = token.Text;
                                    tmpDefine.Type = ArgType.Num;
                                    LDefLookup.Add(tmpDefine);
                                    break;
                                case AGITokenType.Identifier:
                                    // don't bother validating; just use it as is
                                    tmpDefine.Value = token.Text;
                                    tmpDefine.Type = DefTypeFromValue(tmpDefine.Value);
                                    LDefLookup.Add(tmpDefine);
                                    break;
                                default:
                                    // ignore all other types
                                    break;
                                }
                                if (token.Type == AGITokenType.Identifier) {
                                    // don't bother validating; just use it as is
                                    tmpDefine.Value = token.Text;
                                    tmpDefine.Type = DefTypeFromValue(tmpDefine.Value);
                                    LDefLookup.Add(tmpDefine);
                                }
                            }
                            break;
                        case "#include":
                            // next token is filename
                            token = fctb.NextToken(token, false);
                            // convert to absolute path (starting from resource dir)
                            if (token.Type == AGITokenType.String) {
                                string file = token.Text.Trim('\"');
                                if (!Path.IsPathRooted(file)) {
                                    file = Path.GetFullPath(file, EditGame.SrcResDir);
                                }
                                if ((!EditGame.IncludeGlobals ||
                                    !file.Equals(EditGame.GlobalDefines.ResFile, StringComparison.OrdinalIgnoreCase)) &&
                                    (!EditGame.IncludeIDs ||
                                    !file.Equals(Path.Combine(EditGame.SrcResDir, "resourceids.txt"), StringComparison.OrdinalIgnoreCase)) &&
                                    (!EditGame.IncludeReserved ||
                                    !file.Equals(Path.Combine(EditGame.SrcResDir, "reserved.txt"), StringComparison.OrdinalIgnoreCase))) {
                                    Includes.Add(file);
                                }
                            }
                            break;
                        }
                        break;
                    }
                }
                else {
                    // Sierra syntax: #define/%define, #var/%var, #flag/%flag,
                    //                #object/%object, #view/%view, #include/%include,
                    //                #action/%action, #test/%test
                    if (token.Type == AGITokenType.Identifier) {
                        switch (token.Text) {
                        case "#define":
                        case "%define":
                        case "#var":
                        case "%var":
                        case "#flag":
                        case "%flag":
                        case "#object":
                        case "%object":
                        case "#view":
                        case "%view":
                            // next token is name
                            token = fctb.NextToken(token, false);
                            // valid identifier name required
                            if (token.Type == AGITokenType.Identifier) {
                                tmpDefine.Name = token.Text;
                                // next token is value
                                token = fctb.NextToken(token, false);
                                // values can be Identifier, Number, LiteralString
                                switch (token.Type) {
                                case AGITokenType.Identifier:
                                case AGITokenType.Number:
                                case AGITokenType.String:
                                    // don't bother validating; just use it as is
                                    tmpDefine.Value = token.Text;
                                    // define type is based on token
                                    if (token.Type == AGITokenType.String) {
                                        tmpDefine.Type = ArgType.DefStr;
                                    }
                                    else {
                                        tmpDefine.Type = ArgType.Num;
                                    }
                                    LDefLookup.Add(tmpDefine);
                                    break;
                                }
                            }
                            break;
                        case "#include":
                        case "%include":
                            // next token if filename
                            token = fctb.NextToken(token, false);
                            if (token.Type == AGITokenType.String) {
                                string file = token.Text.Trim('\"');
                                if (!Path.IsPathRooted(file)) {
                                    file = Path.GetFullPath(file, EditGame.SrcResDir);
                                }
                                Includes.Add(file);
                            }
                            break;
                        default:
                            break;
                        }
                    }
                }
                token = fctb.NextToken(token, true);
            }
            // first check for nesting
            CheckNesting(Includes);
            DefChanged = false;
            if (!waiting) {
                MDIMain.UseWaitCursor = false;
            }
        }

        private void CheckNesting(List<string> includes) {
            // check for includes
            foreach (string includefile in includes) {
                // only add includes if in-game
                if (IncludeDefines.TryGetValue(includefile, out DefineList value)) {
                    if (value.IsChanged) {
                        // rebuild the list
                        value = ReadIncludeDefines(includefile);
                        IncludeDefines[includefile] = value;
                    }
                    // add these defines to the local list
                    LDefLookup.AddRange(value);
                }
                if (IncludeDefines.ContainsKey(includefile)) {
                    // check each nested include for further nesting
                    CheckNesting(IncludeDefines[includefile].NestedIncludes);
                }
            }
        }

        private void UpdateTip(Place tiplocation) {
            AGIToken seltoken = fctb.TokenFromPos(tiplocation);
            if (seltoken.Type == AGITokenType.Comment) {
                picTip.Visible = false;
                return;
            }
            int oldarg = TipCurArg;
            TipCurArg = EditArgNumber(fctb, tiplocation);
            if (oldarg != TipCurArg) {
                RepositionTip(TipCmdToken);
                picTip.Refresh();
            }
        }

        private static int EditArgNumber(WinAGIFCTB fctb, Place place) {
            // count commas going backward until '(' found
            bool next = false, failed = false;
            int argcount = 0;
            AGIToken token = fctb.TokenFromPos(place);
            token = fctb.PreviousToken(token, true);
            while (token.Type != AGITokenType.None) {
                if (next) {
                    break;
                }
                if (token.Type == AGITokenType.Symbol) {
                    switch (token.Text) {
                    case ",":
                        argcount++;
                        break;
                    case "(":
                        next = true;
                        break;
                    default:
                        failed = true;
                        break;
                    }
                    if (failed) {
                        break;
                    }
                }
                token = fctb.PreviousToken(token, true);
            }
            if (failed) {
                return 0;
            }
            else {
                return argcount;
            }
        }

        private void RepositionTip(AGIToken token) {
            Font tipfont = new("Arial", 9, FontStyle.Regular);
            Font tipbold = new("Arial", 9, FontStyle.Bold);
            Point startPoint = new(0, 0);
            // Set TextFormatFlags to no padding so strings are drawn together.
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
            // Declare a proposed size with dimensions set to the maximum integer value.
            Size proposedSize = new Size(int.MaxValue, int.MaxValue);
            using Graphics graphics = picTip.CreateGraphics();
            Size szText = TextRenderer.MeasureText(graphics, " ()  ", tipfont, proposedSize, flags);
            startPoint.X += szText.Width;
            for (int i = 0; i < TipCmdToken.ArgList.Length; i++) {
                if (TipCurArg == i) {
                    szText = TextRenderer.MeasureText(graphics, TipCmdToken.ArgList[i], tipbold, proposedSize, flags);
                }
                else {
                    szText = TextRenderer.MeasureText(graphics, TipCmdToken.ArgList[i], tipfont, proposedSize, flags);
                }
                startPoint.X += szText.Width;
                if (i < TipCmdToken.ArgList.Length - 1) {
                    szText = TextRenderer.MeasureText(graphics, ", ", tipfont, proposedSize, flags);
                    startPoint.X += szText.Width;
                }
            }
            picTip.Width = startPoint.X;
            picTip.Height = (int)(szText.Height * 1.2);
            Place tp = new(token.EndPos + 1, token.Line);
            Point pPos;
            pPos = fctb.PlaceToPoint(tp);
            picTip.Top = pPos.Y + fctb.CharHeight;
            picTip.Left = pPos.X;
            picTip.Visible = true;
        }

        /// <summary>This function will examine current row, and determine
        /// if a command is being edited it sets value of the module 
        /// variables TipCmdPos, TipCurArg, TipCmdNum<br/>
        /// if matchonly is true, it only returns true if the identified command matches
        /// the command currently being tipped.</summary>
        /// <returns>TRUE if a valid AGI command is found, otherwise it
        /// returns FALSE</returns>
        private bool NeedCommandTip(WinAGIFCTB fctb, Place place, bool matchonly = false) {
            if (EditGame is not null && EditGame.SierraSyntax) {
                return false;
            }
            AGIToken seltoken = fctb.TokenFromPos(place);
            if (seltoken.Type == AGITokenType.Comment) {
                return false;
            }
            // get previous command from current pos
            int argnum = 0;
            AGIToken cmdtoken = FindPrevCmd(fctb, seltoken, ref argnum);

            if (cmdtoken.SubType != TokenSubtype.ActionCmd && cmdtoken.SubType != TokenSubtype.TestCmd && cmdtoken.SubType != TokenSubtype.Snippet) {
                TipCmdToken = new AGIToken();
                return false;
            }
            if (matchonly && cmdtoken.StartPos == TipCmdToken.StartPos && cmdtoken.Line == TipCmdToken.Line) {
                return true;
            }
            // only commands that take arguments are of interest
            if (CmdHasArgs(ref cmdtoken)) {
                TipCmdToken = cmdtoken;
                return true;
            }
            else {
                TipCmdToken = new AGIToken();
                return false;
            }
        }

        private static bool CmdHasArgs(ref AGIToken cmdtoken) {
            // returns true if the command has one or more arguments
            // argument list string is stored in a module variable
            int i = 0, j = 0;
            // index based on first letter
            switch (cmdtoken.Text[0]) {
            case 'a':
                i = 0;
                j = 7;
                break;
            case 'b':
                i = 8;
                j = 8;
                break;
            case 'c':
                i = 9;
                j = 20;
                break;
            case 'd':
                i = 21;
                j = 34;
                break;
            case 'e':
                i = 35;
                j = 39;
                break;
            case 'f':
                i = 40;
                j = 43;
                break;
            case 'g':
                i = 44;
                j = 53;
                break;
            case 'h':
                i = 54;
                j = 54;
                break;
            case 'i':
                i = 55;
                j = 60;
                break;
            case 'l':
                i = 61;
                j = 72;
                break;
            case 'm':
                i = 73;
                j = 77;
                break;
            case 'n':
                i = 78;
                j = 82;
                break;
            case 'o':
                i = 83;
                j = 92;
                break;
            case 'p':
                i = 93;
                j = 102;
                break;
            case 'q':
                i = 103;
                j = 103;
                break;
            case 'r':
                i = 104;
                j = 115;
                break;
            case 's':
                i = 116;
                j = 153;
                break;
            case 't':
                i = 154;
                j = 156;
                break;
            case 'w':
                i = 157;
                j = 158;
                break;
            case '#':
                // snippet args already set
                if (cmdtoken.ArgList.Length > 0) {
                    return true;
                }
                break;
            default:
                return false;
            }
            for (int k = i; k <= j; k++) {
                if (cmdtoken.Text == Editor.Base.EditorResourceByNum(ALPHACMDTEXT + k)) {
                    cmdtoken.ArgList = Editor.Base.EditorResourceByNum(5000 + k).Split(", ");
                    cmdtoken.ArgIndex = k;
                    return true;
                }
            }
            // not a command with arguments
            return false;
        }

        private void RedrawTipWindow(Graphics graphics) {
            Font tipfont = new("Arial", 9, FontStyle.Regular);
            Font tipbold = new("Arial", 9, FontStyle.Bold);
            Point startPoint = new(0, 0);
            // Set TextFormatFlags to no padding so strings are drawn together.
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.NoClipping;
            // Declare a proposed size with dimensions set to the maximum integer value.
            Size proposedSize = new Size(int.MaxValue, int.MaxValue);
            Size szText = TextRenderer.MeasureText(graphics, " (", tipfont, proposedSize, flags);
            TextRenderer.DrawText(graphics, " (", tipfont, startPoint, Color.Black, flags);
            startPoint.X += szText.Width;
            for (int i = 0; i < TipCmdToken.ArgList.Length; i++) {
                if (TipCurArg == i) {
                    szText = TextRenderer.MeasureText(graphics, TipCmdToken.ArgList[i], tipbold, proposedSize, flags);
                    TextRenderer.DrawText(graphics, TipCmdToken.ArgList[i], tipbold, startPoint, Color.Black, flags);
                }
                else {
                    szText = TextRenderer.MeasureText(graphics, TipCmdToken.ArgList[i], tipfont, proposedSize, flags);
                    TextRenderer.DrawText(graphics, TipCmdToken.ArgList[i], tipfont, startPoint, Color.Black, flags);
                }
                startPoint.X += szText.Width;
                if (i < TipCmdToken.ArgList.Length - 1) {
                    szText = TextRenderer.MeasureText(graphics, ", ", tipfont, proposedSize, flags);
                    TextRenderer.DrawText(graphics, ", ", tipfont, startPoint, Color.Black, flags);
                    startPoint.X += szText.Width;
                }
            }
            TextRenderer.DrawText(graphics, ")", tipfont, startPoint, Color.Black, flags);
        }

        private void AddIfUnique(string name, int icon, string value) {
            if (lstDefines.Items.IndexOfKey(name) != -1) {
                return;
            }
            // OK to add it
            ListViewItem item = lstDefines.Items.Add(name, name, icon);
            item.ToolTipText = value;
            return;
        }

        /// <summary>
        /// Adds defines to the listview, which is then presented to user
        /// the list defaults to all defines; but when a specific argument
        /// type is needed, it will adjust to show only the defines
        /// of that particular type.
        /// </summary>
        /// <param name="ArgType"></param>
        private void BuildDefineList(ArgListType ArgType = ArgListType.All) {

            // if no game loaded, skip globals and resIDs (locals and reserved only)

            bool add;
            string linetext;

            if (!ListChanged && !DefChanged && ArgType == ListType) {
                return;
            }
            lstDefines.Items.Clear();
            lstDefines.Tag = "defines";

            // locals not needed if looking for only a ResID
            if (ArgType < ArgListType.Logic) {
                if (DefChanged) {
                    BuildLDefLookup();
                }
                // add local defines (if duplicate defines, only first one will be used)
                foreach (var def in LDefLookup) {
                    // add these local defines IF
                    //     types match OR
                    //     argtype is ALL OR
                    //     argtype is (msg OR invobj) AND deftype is defined string OR
                    //     argtype matches a special type
                    add = false;
                    if ((int)def.Type == (int)ArgType) {
                        add = true;
                    }
                    else {
                        switch (ArgType) {
                        case ArgListType.All:
                            add = true;
                            break;
                        case ArgListType.IfArg:
                            // variables and flags
                            add = def.Type == Engine.ArgType.Var || def.Type == Engine.ArgType.Flag;
                            break;
                        case ArgListType.OthArg:
                            // variables and strings
                            add = def.Type == Engine.ArgType.Var || def.Type == Engine.ArgType.Str;
                            break;
                        case ArgListType.Values:
                            // variables and numbers
                            add = def.Type == Engine.ArgType.Var || def.Type == Engine.ArgType.Num;
                            break;
                        case ArgListType.Msg or ArgListType.IObj:
                            add = def.Type == Engine.ArgType.DefStr;
                            break;
                        }
                    }
                    if (add) {
                        AddIfUnique(def.Name, (int)def.Type, def.Value);
                    }
                }
            }
            else if (ArgType == ArgListType.ActionCmds) {
                // add all action commands
                for (int i = 0; i < Commands.ActionCommands.Length; i++) {
                    linetext = Commands.ActionCommands[i].FanName;
                    lstDefines.Items.Add(linetext, linetext, 26).ToolTipText = linetext;
                }
                ListChanged = false;
                ListType = ArgType;
                return;
            }
            else if (ArgType == ArgListType.TestCmds) {
                // add all action commands
                for (int i = 0; i < Commands.TestCommands.Length; i++) {
                    linetext = Commands.TestCommands[i].FanName;
                    lstDefines.Items.Add(linetext, linetext, 26).ToolTipText = linetext;
                }
                ListChanged = false;
                ListType = ArgType;
                return;
            }
            else {
                if (EditGame is null) {
                    // no resIDs if no game is loaded
                    return;
                }
            }
            // global defines next (but only if not looking for just a ResID AND game is loaded)
            if (EditGame is not null && ArgType < ArgListType.Logic) {
                foreach (var define in EditGame.GlobalDefines.Values) {
                    // add these global defines IF
                    //     types match OR
                    //     argtype is ALL OR
                    //     argtype is (msg OR invobj) AND deftype is defined string
                    //     argtype matches a special type
                    add = false;
                    if ((int)define.Type == (int)ArgType) {
                        add = true;
                    }
                    else {
                        switch (ArgType) {
                        case ArgListType.All:
                            add = true;
                            break;
                        case ArgListType.IfArg:
                            // variables and flags
                            add = define.Type == Engine.ArgType.Var || define.Type == Engine.ArgType.Flag;
                            break;
                        case ArgListType.OthArg:
                            // variables and strings
                            add = define.Type == Engine.ArgType.Var || define.Type == Engine.ArgType.Str;
                            break;
                        case ArgListType.Values:
                            // variables and numbers
                            add = define.Type == Engine.ArgType.Var || define.Type == Engine.ArgType.Num;
                            break;
                        case ArgListType.Msg or ArgListType.IObj:
                            add = define.Type == Engine.ArgType.DefStr;
                            break;
                        }
                    }
                    if (add) {
                        // don't add if already defined (image offset is 11)
                        AddIfUnique(define.Name, 11 + (int)define.Type, define.Value);
                    }
                }
            }
            // resource IDs
            if (EditGame is not null) {
                // check for logics, views, sounds, iobjs, voc words AND pics
                switch (ArgType) {
                case ArgListType.All:
                case ArgListType.Byte:
                case ArgListType.Values:
                case ArgListType.Logic:
                case ArgListType.Picture:
                case ArgListType.Sound:
                case ArgListType.View:
                    for (int j = 0; j < 4; j++) {
                        if (ArgType == ArgListType.All ||
                            ArgType == ArgListType.Byte ||
                            ArgType == ArgListType.Values ||
                            ArgType - ArgListType.Logic == j) {
                            for (int i = 0; i < 256; i++) {
                                // if valid resource, type is atNum
                                if (IDefLookup[j, i].Type == Engine.ArgType.Num) {
                                    AddIfUnique(IDefLookup[j, i].Name, 22 + j, IDefLookup[j, i].Value);
                                }
                            }
                        }
                    }
                    break;
                case ArgListType.IObj:
                    // add inv items (ok if matches an existing define)
                    if (!EditGame.InvObjects.Loaded) {
                        EditGame.InvObjects.Load();
                    }
                    // skip any items that are just a question mark
                    for (int i = 0; i < EditGame.InvObjects.Count; i++) {
                        linetext = EditGame.InvObjects[i].ItemName;
                        if (linetext != "?") {
                            if (linetext.Contains(QUOTECHAR)) {
                                linetext = linetext.Replace("\"", "\\" + QUOTECHAR);
                            }
                            lstDefines.Items.Add("\"" + linetext + "\"", "\"" + linetext + "\"", 17).ToolTipText = "i" + i;
                        }
                    }
                    break;
                case ArgListType.VocWrd:
                    // add vocab words items (ok if matches an existing define)
                    if (!EditGame.WordList.Loaded) {
                        EditGame.WordList.Load();
                    }
                    // skip group 0
                    for (int i = 1; i < EditGame.WordList.GroupCount; i++) {
                        // skip groups with no name (only possible if group 1 or 9999
                        // have no words added)
                        if (EditGame.WordList.GroupByIndex(i).WordCount > 0) {
                            linetext = "\"" + EditGame.WordList.GroupByIndex(i).Words[0] + "\"";
                            lstDefines.Items.Add(linetext, linetext, 22).ToolTipText = EditGame.WordList.GroupByIndex(i).GroupNum.ToString();
                        }
                    }
                    break;
                }
            }
            // lastly, check for reserved defines option (if not looking for a resourceID)
            Define[] tmpDefines = null;
            if (EditGame is null && WinAGISettings.DefIncludeReserved.Value && ArgType < ArgListType.Logic) {
                tmpDefines = DefaultReservedDefines.All();
            }
            if (EditGame is not null && EditGame.IncludeReserved && ArgType < ArgListType.Logic) {
                tmpDefines = EditGame.ReservedDefines.All();
            }
            if (tmpDefines is not null) {
                for (int i = 0; i < tmpDefines.Length; i++) {
                    // add these reserved defines IF
                    //     types match OR
                    //     argtype is ALL OR
                    //     argtype is (msg OR invobj) AND deftype is defined string
                    //     argtype matches a special type
                    add = false;
                    if ((int)tmpDefines[i].Type == (int)ArgType) {
                        add = true;
                    }
                    else {
                        switch (ArgType) {
                        case ArgListType.All:
                            add = true;
                            break;
                        case ArgListType.IfArg:
                            // variables and flags
                            add = tmpDefines[i].Type == Engine.ArgType.Var || tmpDefines[i].Type == Engine.ArgType.Flag;
                            break;
                        case ArgListType.OthArg:
                            // variables and strings
                            add = tmpDefines[i].Type == Engine.ArgType.Var || tmpDefines[i].Type == Engine.ArgType.Str;
                            break;
                        case ArgListType.Values:
                            // variables and numbers
                            add = tmpDefines[i].Type == Engine.ArgType.Var || tmpDefines[i].Type == Engine.ArgType.Num;
                            break;
                        case ArgListType.Msg or ArgListType.IObj:
                            add = tmpDefines[i].Type == Engine.ArgType.DefStr;
                            break;
                        }
                    }
                    if (add) {
                        // don't add if already defined
                        AddIfUnique(tmpDefines[i].Name, 26 + (int)tmpDefines[i].Type, tmpDefines[i].Value);
                    }
                }
            }
            ListChanged = false;
            ListType = ArgType;
        }

        private bool InIfBlock(AGIToken token) {
            // returns true if the token is inside an IF block
            int ifpos = WinAGIFCTB.FindTokenPosRev(fctb.Text, "if", fctb.PlaceToPosition(token.Start));
            if (ifpos == -1) {
                return false;
            }
            int blockstart = WinAGIFCTB.FindTokenPosRev(fctb.Text, "{", fctb.PlaceToPosition(token.Start));
            return blockstart < ifpos;
        }

        private void ShowSynonymList(string aWord) {
            // displays a list of synonyms in a list box
            ListChanged = true;
            int GrpNum = EditGame.WordList[aWord[1..^1]].Group;
            int GrpCount = EditGame.WordList.GroupByNumber(GrpNum).WordCount;
            lstDefines.Items.Clear();
            lstDefines.Tag = "words";
            for (byte i = 0; i < GrpCount; i++) {
                lstDefines.Items.Add("\"" + EditGame.WordList.GroupByNumber(GrpNum)[i] + "\"", 21);
            }
            // save pos and text
            DefStartPos = fctb.Selection.Start;
            DefEndPos = fctb.Selection.End;
            PrevText = fctb.Selection.Text;
            DefText = PrevText;
            // select the word
            foreach (ListViewItem tmpItem in lstDefines.Items) {
                if (aWord == tmpItem.Text) {
                    tmpItem.Selected = true;
                    tmpItem.EnsureVisible();
                    break;
                }
            }
            // not found; select first item in list
            lstDefines.Items[0].Selected = true;
            lstDefines.Items[0].EnsureVisible();
            // position it
            PositionListBox();
            lstDefines.Visible = true;
            fctb.CaretVisible = false;
            lstDefines.Select();
            return;
        }

        private void PositionListBox() {
            Place tp = fctb.Selection.Start;
            Point pPos;
            pPos = fctb.PlaceToPoint(tp);
            // convert to form coordinates
            pPos = fctb.PointToScreen(pPos);
            pPos = PointToClient(pPos);
            lstDefines.Top = pPos.Y + fctb.CharHeight;
            lstDefines.Left = pPos.X;
        }

        private void ShowSnippetList() {
            // displays snippets in a list box
            if (CodeSnippets.Length == 0) {
                return;
            }
            if (fctb.Lines[fctb.Selection.Start.iLine][..fctb.Selection.Start.iChar].Length == 0) {
                SnipIndent = fctb.Selection.Start.iChar;
            }
            else {
                SnipIndent = 0;
            }
            ListChanged = true;
            lstDefines.Items.Clear();
            lstDefines.Tag = "snippets";
            for (int i = 0; i < CodeSnippets.Length; i++) {
                lstDefines.Items.Add(CodeSnippets[i].Name).ToolTipText = CodeSnippets[i].Value;
                lstDefines.Items[i].Tag = CodeSnippets[i].ArgTips;
            }
            lstDefines.Columns[0].Width = lstDefines.Width;
            lstDefines.Items[0].Selected = true;
            lstDefines.Items[0].EnsureVisible();
            // save pos and text
            DefStartPos = fctb.Selection.Start;
            DefEndPos = fctb.Selection.End;
            PrevText = "";
            DefText = "";
            // position it
            PositionListBox();

            lstDefines.ShowItemToolTips = true;
            lstDefines.Visible = true;
            fctb.CaretVisible = false;
            lstDefines.Select();
        }

        /// <summary>
        /// Displays defines in a list box
        /// that user can select from to replace current word (if cursor
        /// is in a word) or insert at current position (if cursor is
        /// in between words).
        /// </summary>
        private void ShowDefineList() {
            AGIToken token;
            picTip.Visible = false;
            if (fctb.SelectionLength == 0) {
                int linelength = fctb.GetLineLength(fctb.Selection.Start.iLine);
                if (linelength == 0) {
                    token = fctb.TokenFromPos();
                }
                // '|ab ' or ' |ab' or  'a|b'  or 'ab| ' or ' ab|'
                // should all return 'ab'
                // TokenFromPos() will get first three
                else if (fctb.Selection.Start.iChar > 0 && fctb.Selection.Start.iChar == linelength || fctb.Lines[fctb.Selection.Start.iLine][fctb.Selection.Start.iChar] == ' ') {
                    Place next = fctb.Selection.Start;
                    next.iChar--;
                    token = fctb.TokenFromPos(next);
                }
                else {
                    token = fctb.TokenFromPos();
                }
            }
            else {
                token = fctb.TokenFromPos();
            }
            ArgListType tmpType = GetArgType(token);
            BuildDefineList(tmpType);
            if (lstDefines.Items.Count == 0) {
                return;
            }
            // expand selection if necessary
            Place tokenstart = new(token.StartPos, token.Line);
            Place tokenend = new(token.EndPos, token.Line);
            if (fctb.SelectionLength == 0) {
                if (token.Text.Length > 0 && (token.Type == AGITokenType.Identifier || token.Type == AGITokenType.String || token.Type == AGITokenType.Number)) {
                    fctb.Selection.Start = tokenstart;
                    fctb.Selection.End = tokenend;
                }
            }
            else {
                // expand only if selection is inside the token
                if (token.Text.Length > 0) {
                    if (fctb.Selection.Start >= tokenstart && fctb.Selection.End <= tokenend) {
                        fctb.Selection.Start = tokenstart;
                        fctb.Selection.End = tokenend;
                    }
                }
            }
            // save pos and text
            DefStartPos = fctb.Selection.Start;
            DefEndPos = fctb.Selection.End;
            PrevText = fctb.Selection.Text;
            DefText = PrevText;
            fctb.CaretVisible = false;
            // check for match or partial in the list
            if (fctb.SelectionLength > 0) {
                string seltext = fctb.SelectedText;
                bool selected = false;
                // check for value match first
                if (Regex.IsMatch(seltext, @"[vfscimo]\d{1,3}\b")) {
                    foreach (ListViewItem tmpItem in lstDefines.Items) {
                        if (seltext == tmpItem.ToolTipText) {
                            tmpItem.Selected = true;
                            tmpItem.EnsureVisible();
                            selected = true;
                            break;
                        }
                    }
                }
                if (!selected) {
                    bool found = false;
                    foreach (ListViewItem tmpItem in lstDefines.Items) {
                        switch (string.Compare(seltext, tmpItem.Text, StringComparison.OrdinalIgnoreCase)) {
                        case 0:
                            // select this one
                            tmpItem.Selected = true;
                            lstDefines.TopItem = tmpItem;
                            tmpItem.EnsureVisible();
                            found = true;
                            break;
                        case < 0:
                            // strText < tmpItem.Text
                            // stop here; don't select it, but scroll to it
                            lstDefines.TopItem = tmpItem;
                            tmpItem.EnsureVisible();
                            found = true;
                            break;
                        }
                        if (found) {
                            break;
                        }
                    }
                }
            }
            lstDefines.Columns[0].Width = lstDefines.Width;
            // position it
            PositionListBox();
            lstDefines.ShowItemToolTips = true;
            lstDefines.Visible = true;
            fctb.CaretVisible = false;
            lstDefines.Select();
        }

        /// <summary>
        /// Displays available commands in a list box
        /// that user can select from to replace current word (if cursor
        /// is in a word) or insert at current position (if cursor is
        /// in between words).
        /// </summary>
        private void ShowCommandList() {
            AGIToken token;
            picTip.Visible = false;
            if (fctb.SelectionLength == 0) {
                int linelength = fctb.GetLineLength(fctb.Selection.Start.iLine);
                if (linelength == 0) {
                    token = fctb.TokenFromPos();
                }
                // '|ab ' or ' |ab' or  'a|b'  or 'ab| ' or ' ab|'
                // should all return 'ab'
                // TokenFromPos() will get first three
                else if (fctb.Selection.Start.iChar > 0 && fctb.Selection.Start.iChar == linelength || fctb.Lines[fctb.Selection.Start.iLine][fctb.Selection.Start.iChar] == ' ') {
                    Place next = fctb.Selection.Start;
                    next.iChar--;
                    token = fctb.TokenFromPos(next);
                }
                else {
                    token = fctb.TokenFromPos();
                }
            }
            else {
                token = fctb.TokenFromPos();
            }
            ArgListType tmpType = InIfBlock(token) ? ArgListType.TestCmds : ArgListType.ActionCmds;
            BuildDefineList(tmpType);
            // expand selection if necessary
            Place tokenstart = new(token.StartPos, token.Line);
            Place tokenend = new(token.EndPos, token.Line);
            if (fctb.SelectionLength == 0) {
                if (token.Text.Length > 0 && (token.Type == AGITokenType.Identifier || token.Type == AGITokenType.String || token.Type == AGITokenType.Number)) {
                    fctb.Selection.Start = tokenstart;
                    fctb.Selection.End = tokenend;
                }
            }
            else {
                // expand only if selection is inside the token
                if (token.Text.Length > 0) {
                    if (fctb.Selection.Start >= tokenstart && fctb.Selection.End <= tokenend) {
                        fctb.Selection.Start = tokenstart;
                        fctb.Selection.End = tokenend;
                    }
                }
            }
            // save pos and text
            DefStartPos = fctb.Selection.Start;
            DefEndPos = fctb.Selection.End;
            PrevText = fctb.Selection.Text;
            DefText = PrevText;
            fctb.CaretVisible = false;
            // check for match or partial in the list
            if (fctb.SelectionLength > 0) {
                string seltext = fctb.SelectedText;
                bool selected = false;
                // check for value match first
                if (!selected) {
                    bool found = false;
                    foreach (ListViewItem tmpItem in lstDefines.Items) {
                        switch (string.Compare(seltext, tmpItem.Text, StringComparison.OrdinalIgnoreCase)) {
                        case 0:
                            // select this one
                            tmpItem.Selected = true;
                            lstDefines.TopItem = tmpItem;
                            tmpItem.EnsureVisible();
                            found = true;
                            break;
                        case < 0:
                            // strText < tmpItem.Text
                            // stop here; don't select it, but scroll to it
                            lstDefines.TopItem = tmpItem;
                            tmpItem.EnsureVisible();
                            found = true;
                            break;
                        }
                        if (found) {
                            break;
                        }
                    }
                }
            }
            lstDefines.Columns[0].Width = lstDefines.Width;
            // position it
            PositionListBox();
            lstDefines.ShowItemToolTips = true;
            lstDefines.Visible = true;
            fctb.CaretVisible = false;
            lstDefines.Select();
        }

        internal void ShowHelp() {
            string topic = "htm\\";
            switch (FormMode) {
            case LogicFormMode.Logic:
                if (InGame && !EditGame.SierraSyntax) {
                    // if current token is a command show help for that command
                    AGIToken token = fctb.TokenFromPos();
                    bool isCmd = false;
                    // check for command
                    if (token.Type == AGITokenType.Identifier) {
                        // check this command against list
                        isCmd = CommandNum(token.Text) >= 0;
                    }
                    if (isCmd) {
                        topic += "commands\\cmd_" + token.Text.ToLower().Replace(".", "") + ".htm";
                    }
                    else {
                        topic += "winagi\\editor_logic.htm";
                    }
                }
                else {
                    topic += "winagi\\editor_logic.htm";
                }
                break;
            case LogicFormMode.Text:
                topic += "winagi\\editor_text.htm";
                break;
            }
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
        }

        private ArgListType GetArgType(AGIToken token) {
            int argpos = 0;
            AGIToken cmdtoken = FindPrevCmd(fctb, token, ref argpos);
            if (cmdtoken.SubType == TokenSubtype.ActionCmd || cmdtoken.SubType == TokenSubtype.TestCmd) {
                // only commands with args need to be checked
                if (!CmdHasArgs(ref cmdtoken)) {
                    // args not valid- let user choose anything
                    return ArgListType.All;
                }
                if (cmdtoken.ArgIndex == 116) {
                    // 'said' command
                    return ArgListType.VocWrd;
                }
                else {
                    if (argpos >= cmdtoken.ArgList.Length) {
                        // too many args - let user choose anything
                        return ArgListType.All;
                    }
                    // use arglist to determine arg type
                    switch (cmdtoken.ArgList[argpos][0]) {
                    case 'b':
                        // byte or number
                        ArgListType retval = ArgListType.Byte;
                        // check for special cases where resourceIDs are also valid
                        switch (cmdtoken.ArgIndex) {
                        case 0:
                            // add.to.pic(view, byt, byt, byt, byt, byt, byt)
                            if (argpos == 0) {
                                retval = ArgListType.View;
                            }
                            break;
                        case 9:
                            // call(logic)
                            retval = ArgListType.Logic;
                            break;
                        case 24:
                            // discard.sound(sound)
                            retval = ArgListType.Sound;
                            break;
                        case 25:
                            // discard.view(view)
                            retval = ArgListType.View;
                            break;
                        case 66:
                            // load.logics(logic)
                            retval = ArgListType.Logic;
                            break;
                        case 69:
                            // load.sound(sound)
                            retval = ArgListType.Sound;
                            break;
                        case 70:
                            // load.view(view)
                            retval = ArgListType.View;
                            break;
                        case 78:
                            // new.room(logic)
                            retval = ArgListType.Logic;
                            break;
                        case 138:
                            // set.view(obj,view)
                            retval = ArgListType.View;
                            break;
                        case 141:
                            // show.obj(view)
                            retval = ArgListType.View;
                            break;
                        case 143:
                            // sound(sound,flg)
                            retval = ArgListType.Sound;
                            break;
                        case 156:
                            // trace.info(logic,byt,byt)
                            if (argpos == 0) {
                                retval = ArgListType.Logic;
                            }
                            break;
                        }
                        return retval;
                    case 'v':
                        // variable
                        return ArgListType.Var;
                    case 'f':
                        // flag
                        return ArgListType.Flag;
                    case 'm':
                        // message
                        return ArgListType.Msg;
                    case 'o':
                        // screen obj
                        return ArgListType.SObj;
                    case 'i':
                        // inv obj
                        return ArgListType.IObj;
                    case 's':
                        // string
                        return ArgListType.Str;
                    case 'w':
                        // word
                        return ArgListType.Word;
                    case 'c':
                        // controller
                        return ArgListType.Ctl;
                    default:
                        // not possible but compiler needs a return command
                        return ArgListType.All;
                    }
                }
            }
            // not editing a command; use previous token to inform 
            // choice (skipping '(' tokens)
            cmdtoken = fctb.PreviousToken(token);
            while (cmdtoken.Type != AGITokenType.None) {
                if (cmdtoken.Text != "(") {
                    break;
                }
                cmdtoken = fctb.PreviousToken(cmdtoken);
            }
            if (cmdtoken.Type == AGITokenType.None) {
                // not clear what is being entered
                return ArgListType.All;
            }
            switch (cmdtoken.Text) {
            case "==" or "!=" or ">" or ">" or "<=" or ">=" or "=<" or "=>" or "||" or "&&":
                // var or number
                return ArgListType.Values;
            case "++" or "--":
                return ArgListType.Var;
            case "=":
                // var, number, string
                return ArgListType.Values;
            case "if":
                // var, flag
                return ArgListType.IfArg;
            }
            // still not clear what is being entered
            return ArgListType.All;
        }

        private bool AskClose() {
            if (!Visible) {
                // if exiting due to error on form load
                return true;
            }
            if (LogicNumber == -1) {
                // force shutdown
                return true;
            }
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to " + (FormMode == LogicFormMode.Logic ? "source code" : "text file"),
                    "Save " + (FormMode == LogicFormMode.Logic ? "Logic Source" : "Text File"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    switch (FormMode) {
                    case LogicFormMode.Logic:
                        SaveLogicSource();
                        break;
                    case LogicFormMode.Text:
                        SaveTextFile(TextFilename);
                        break;
                    }
                    if (rtfLogic1.IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "File not saved. Continue closing anyway?",
                            "Save " + (FormMode == LogicFormMode.Logic ? "Logic Source" : "Text File"),
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        return rtn == DialogResult.Yes;
                    }
                    break;
                case DialogResult.Cancel:
                    return false;
                case DialogResult.No:
                    if (InGame) {
                        if (FormMode == LogicFormMode.Logic) {
                            // restore warning/error list to last save state
                            // update and save warning/error info for this logic
                            MDIMain.ResetInfoGrid((byte)LogicNumber, WarningList);
                        }
                        else {
                            //
                        }
                    }
                    return true;
                }
            }
            if (FormMode == LogicFormMode.Logic && InGame && !EditGame.Logics[LogicNumber].Compiled) {
                // only logics can be in game, so no need to check for text file
                bool dontAsk = false;
                DialogResult rtn = DialogResult.Yes;
                switch (WinAGISettings.WarnCompile.Value) {
                case AskOption.Ask:
                    rtn = MsgBoxEx.Show(MDIMain,
                       "Do you want to compile this logic before closing it?",
                       "Save Logic Source", // Text,
                                   MessageBoxButtons.YesNoCancel,
                       MessageBoxIcon.Question,
                                   "Always take this action when closing a logic source file.", ref dontAsk);
                    if (dontAsk) {
                        if (rtn == DialogResult.Yes) {
                            WinAGISettings.WarnCompile.Value = AskOption.Yes;
                        }
                        else if (rtn == DialogResult.No) {
                            WinAGISettings.WarnCompile.Value = AskOption.No;
                        }
                        WinAGISettings.WarnCompile.WriteSetting(WinAGISettingsFile);
                    }
                    break;
                case AskOption.No:
                    rtn = DialogResult.No;
                    break;
                case AskOption.Yes:
                    rtn = DialogResult.Yes;
                    break;
                }
                switch (rtn) {
                case DialogResult.Yes:
                    if (!CompileLogic(this, (byte)LogicNumber)) {
                        // error
                        return false;
                    }
                    RefreshTree(AGIResType.Logic, LogicNumber);
                    return true;
                case DialogResult.No:
                    return true;
                case DialogResult.Cancel:
                    return false;
                }
            }
            return true;
        }

        internal void UpdateStatusBar() {
            if (!Visible) {
                return;
            }
            spLine.Text = "Line: " + fctb.Selection.End.iLine;
            spColumn.Text = "Col: " + fctb.Selection.End.iChar;
            spStatus.Text = "";
        }

        void MarkAsChanged() {
            // ignore when loading (not visible yet)
            if (!Visible) {
                return;
            }
            if (!IsChanged && (rtfLogic1.IsChanged || rtfLogic2.IsChanged)) {
                IsChanged = true;
                mnuRSave.Enabled = true;
                MDIMain.btnSaveResource.Enabled = true;
                Text = CHG_MARKER + Text;
            }
            btnUndo.Enabled = rtfLogic1.UndoEnabled;
            btnRedo.Enabled = rtfLogic1.RedoEnabled;
            frmFind.ResetSearch();
        }

        private void MarkAsSaved() {
            if (FormMode == LogicFormMode.Logic) {
                Text = LOGIC_EDITOR;
                if (InGame) {
                    Text += ResourceName(EditLogic, InGame, true);
                }
                else {
                    Text += CompactPath(EditLogic.SourceFile, 75);
                }
            }
            else {
                Text = "Text Editor - " + CompactPath(TextFilename, 75);
            }
            IsChanged = false;
            rtfLogic1.IsChanged = false;
            mnuRSave.Enabled = false;
            MDIMain.btnSaveResource.Enabled = false;
        }
        #endregion
    }
}
