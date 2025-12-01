using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Engine.Base;
using static WinAGI.Editor.Base;
using System.IO;
using System.Linq;
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;
using WinAGI.Common;
using static WinAGI.Common.API;

namespace WinAGI.Editor {
    public partial class frmLogicEdit : Form {
        struct MessageData {
            public string Text;
            public bool Declared;
            public bool ByNumber;
            public bool ByRef;
            public int Type; // 0=string, 1=marker, 2=define
            public int Line;
            public int Concat;
        }

        #region Logic Editor Members
        public int LogicNumber;
        public Logic EditLogic = new() { };
        public bool Compiled = false;
        internal readonly LogicFormMode FormMode;
        public bool ListChanged = false;
        public bool InGame = false;
        public bool IsChanged = false;
        public string TextFilename = "";
        private bool closing = false;
        // editor syntax styles
        public TextStyle CommentStyle;
        public TextStyle StringStyle;
        public TextStyle KeyWordStyle;
        public TextStyle TestCmdStyle;
        public TextStyle ActionCmdStyle;
        public TextStyle InvalidCmdStyle;
        public TextStyle NumberStyle;
        public TextStyle ArgIdentifierStyle;
        public TextStyle DefIdentifierStyle;
        public WinAGIFCTB fctb;
        private bool SelectEntireLine = false;
        TDefine[] LDefLookup = [];
        private bool loading;
        bool DefDirty = true;
        // DefDirty means text has changed, so the lookup list needs
        // to be rebuilt;
        public bool ListDirty = true;
        // ListDirty means the ShowDefinesList needs to be rebuilt
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
        #endregion

        public frmLogicEdit(LogicFormMode mode) {
            InitializeComponent();
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
            CommentStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[1].Color.Value), null, WinAGISettings.SyntaxStyle[1].FontStyle.Value);
            StringStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[2].Color.Value), null, WinAGISettings.SyntaxStyle[2].FontStyle.Value);
            KeyWordStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[3].Color.Value), null, WinAGISettings.SyntaxStyle[3].FontStyle.Value);
            TestCmdStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[4].Color.Value), null, WinAGISettings.SyntaxStyle[4].FontStyle.Value);
            ActionCmdStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[5].Color.Value), null, WinAGISettings.SyntaxStyle[5].FontStyle.Value);
            InvalidCmdStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[6].Color.Value), null, WinAGISettings.SyntaxStyle[6].FontStyle.Value);
            NumberStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[7].Color.Value), null, WinAGISettings.SyntaxStyle[7].FontStyle.Value);
            ArgIdentifierStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[8].Color.Value), null, WinAGISettings.SyntaxStyle[8].FontStyle.Value);
            DefIdentifierStyle = new TextStyle(new SolidBrush(WinAGISettings.SyntaxStyle[9].Color.Value), null, WinAGISettings.SyntaxStyle[9].FontStyle.Value);
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
                break;
            case LogicFormMode.Text:
                // no compiling or msg cleanup
                btnCompile.Enabled = false;
                btnMsgClean.Enabled = false;
                break;
            }
            // another hack needed...
            // list boxes don't actually apply the sort until they have been 
            // displayed at least one time, so we move it off screen and show
            // at at the start; when needed, it will be moved to correct
            // location
            lstDefines.Left = -9999;
            lstDefines.Visible = true;
        }

        #region Form Event Handlers
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
        }

        private void frmLogicEdit_KeyDown(object sender, KeyEventArgs e) {
            // CTRL+SHIFT+S = Save All Open Logics
            if (e.Control && e.Shift && e.KeyCode == Keys.S) {
                foreach (frmLogicEdit frm in LogicEditors) {
                    if (frm.FormMode == LogicFormMode.Logic) {
                        if (frm.IsChanged) {
                            frm.SaveLogicSource();
                        }
                    }
                }
            }
        }

        private void frmLogicEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmLogicEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // dereference object
            EditLogic?.Unload();
            EditLogic = null;
            if (InGame) {
                if (EditGame.Logics[LogicNumber].Loaded) {
                    EditGame.Logics[LogicNumber].Unload();
                }
            }
            // remove from logic editor collection
            foreach (frmLogicEdit frm in LogicEditors) {
                if (frm == this) {
                    LogicEditors.Remove(frm);
                    FindingForm.ResetSearch();
                    break;
                }
            }
        }
        #endregion

        #region Resource Menu Event Handlers
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
                    mnuRInGame.Enabled = EditGame is not null;
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
            if (InGame) {
                RenumberLogic();
            }
        }

        private void mnuRProperties_Click(object sender, EventArgs e) {
            if (FormMode == LogicFormMode.Logic) {
                EditLogicProperties(1);
            }
        }

        private void mnuRCompile_Click(object sender, EventArgs e) {
            // TODO: non-game logics can't be compiled (yet...?)
            if (InGame && !EditLogic.Compiled) {
                Compiled = CompileLogic(this, (byte)LogicNumber);
            }
        }

        private void mnuRMsgCleanup_Click(object sender, EventArgs e) {
            if (FormMode == LogicFormMode.Logic) {
                MessageCleanup();
            }
        }

        private void mnuRIsRoom_Click(object sender, EventArgs e) {
            if (InGame && LogicNumber != 0) {
                EditLogic.IsRoom = !EditLogic.IsRoom;
                MDIMain.RefreshPropertyGrid(AGIResType.Logic, LogicNumber);
                MarkAsChanged();
                UpdateExitInfo(EditLogic.IsRoom ? UpdateReason.ShowRoom : UpdateReason.HideRoom, LogicNumber, EditLogic);
            }
        }
        #endregion

        #region Edit Menu Event Handlers
        private void RefreshEditMenu() {
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
            // Open xx for Editing (Shift+Ctrl+E) V:on resource token; E
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
            mnuEListCommands.Visible = EditGame is null || !EditGame.SierraSyntax;
            mnuEListDefines.Visible = EditGame is null || !EditGame.SierraSyntax;
            // default to not visible
            mnuEOpenRes.Visible = false;
            mnuEViewSynonym.Visible = false;
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
                                mnuEOpenRes.Visible = true;
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
                    mnuEOpenRes.Visible = true;
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
            mnuEDocumentMap.Text = (splitContainer1.Panel2Collapsed ? "Show" : "Hide") + " Document Map";
            mnuELineNumbers.Text = (rtfLogic1.ShowLineNumbers ? "Hide" : "Show") + " Line Numbers";
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
            mnuEOpenRes.Owner = mnuEdit.DropDown;
            mnuESep3.Owner = mnuEdit.DropDown;
            mnuEDocumentMap.Owner = mnuEdit.DropDown;
            mnuELineNumbers.Owner = mnuEdit.DropDown;
            mnuECharMap.Owner = mnuEdit.DropDown;
            RefreshEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            // return menu items to context menu
            mnuEUndo.Owner = contextMenuStrip1;
            mnuERedo.Owner = contextMenuStrip1;
            mnuESep0.Owner = contextMenuStrip1;
            mnuECut.Owner = contextMenuStrip1;
            mnuEDelete.Owner = contextMenuStrip1;
            mnuECopy.Owner = contextMenuStrip1;
            mnuEPaste.Owner = contextMenuStrip1;
            mnuESelectAll.Owner = contextMenuStrip1;
            mnuESep1.Owner = contextMenuStrip1;
            mnuEFind.Owner = contextMenuStrip1;
            mnuEFindAgain.Owner = contextMenuStrip1;
            mnuEReplace.Owner = contextMenuStrip1;
            mnuESep2.Owner = contextMenuStrip1;
            mnuESnippet.Owner = contextMenuStrip1;
            mnuEListDefines.Owner = contextMenuStrip1;
            mnuEListCommands.Owner = contextMenuStrip1;
            mnuEViewSynonym.Owner = contextMenuStrip1;
            mnuEBlockCmt.Owner = contextMenuStrip1;
            mnuEUnblockCmt.Owner = contextMenuStrip1;
            mnuEOpenRes.Owner = contextMenuStrip1;
            mnuESep3.Owner = contextMenuStrip1;
            mnuEDocumentMap.Owner = contextMenuStrip1;
            mnuELineNumbers.Owner = contextMenuStrip1;
            mnuECharMap.Owner = contextMenuStrip1;
            // TODO: re-enable all menu items so shortcut keys will work

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            RefreshEditMenu();
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
            fctb.Paste();
            MarkAsChanged();
        }

        private void mnuESelectAll_Click(object sender, EventArgs e) {
            fctb.SelectAll();
        }

        private void mnuEFind_Click(object sender, EventArgs e) {
            FindingForm.SetForm(FormMode == LogicFormMode.Logic ? FindFormFunction.FindLogic : FindFormFunction.FindText, InGame);
            if (fctb.SelectionLength > 0) {
                FindingForm.cmbFind.Text = fctb.SelectedText;
            }
            if (!FindingForm.Visible) {
                FindingForm.Visible = true;
            }
            FindingForm.Select();
            FindingForm.cmbFind.Select();
        }

        private void mnuEFindAgain_Click(object sender, EventArgs e) {
            if (GFindText.Length > 0) {
                FindInLogic(this, GFindText, GFindDir, GMatchWord, GMatchCase, GLogFindLoc);
            }
        }

        private void mnuEReplace_Click(object sender, EventArgs e) {
            FindingForm.SetForm(FormMode == LogicFormMode.Logic ? FindFormFunction.ReplaceLogic : FindFormFunction.ReplaceText, InGame);
            FindingForm.Visible = true;
        }

        private void mnuESnippet_Click(object sender, EventArgs e) {
            if (mnuESnippet.Text[0] == 'I') {
                ShowSnippetList();
            }
            else {
                frmSnippets Snippets = new(true, fctb.SelectedText);
                _ = Snippets.ShowDialog(this);
                Snippets.Dispose();
            }
        }

        private void mnuEListDefines_Click(object sender, EventArgs e) {
            if (EditGame is null || !EditGame.SierraSyntax) {
                ShowDefineList();
            }
        }

        private void mnuEListCommands_Click(object sender, EventArgs e) {
            if (EditGame is null || !EditGame.SierraSyntax) {
                ShowCommandList();
            }
        }

        private void mnuEBlockCmt_Click(object sender, EventArgs e) {
            fctb.InsertLinePrefix(fctb.CommentPrefix);
        }

        private void mnuEUnblockCmt_Click(object sender, EventArgs e) {
            fctb.RemoveLinePrefix(fctb.CommentPrefix);
        }

        private void mnuEOpenRes_Click(object sender, EventArgs e) {
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
                    filename = Path.GetFullPath(EditGame.SrcResDir + filename);
                }
                else {
                    // relative to current dir of this logic
                    if (FormMode == LogicFormMode.Logic) {
                        if (EditLogic.SourceFile.Length > 0) {
                            filename = Path.GetFullPath(Path.GetDirectoryName(EditLogic.SourceFile) + "\\" + filename);
                        }
                        else {
                            // try current dir
                            filename = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\" + filename);
                        }
                    }
                    else {
                        if (TextFilename.Length > 0) {
                            filename = Path.GetFullPath(Path.GetDirectoryName(TextFilename) + "\\" + filename);
                        }
                        else {
                            // try current dir
                            filename = Path.GetFullPath(Directory.GetCurrentDirectory() + "\\" + filename);
                        }
                    }
                }
                if (!File.Exists(filename)) {
                    MessageBox.Show(MDIMain,
                        "Unable to find '" + mnuEOpenRes.Text[6..(^13)] + "' relative to the current directory.",
                        "File Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        @"htm\commands\syntax_fan.htm#include",
                        WinAGIHelp);
                    RestoreFocusHack();
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
            frmCharPicker CharPicker = new(CodePage);
            CharPicker.ShowDialog(MDIMain);
            if (!CharPicker.Cancel) {
                if (CharPicker.InsertString.Length > 0) {
                    fctb.InsertText(CharPicker.InsertString, true);
                }
            }
            CharPicker.Close();
            CharPicker.Dispose();
        }

        public void RestoreFocusHack() {
            // TODO: is this still needed?
            // something about this form (probably the fctb? it's the only 
            // 'non-standard' control on the form) causes focus to jump to
            // the previous form/control when a messagebox or other external
            // dialog window is called from within the form. This hack seems
            // to fix it. Ugh.

            //_ = SendMessage(Handle, WM_SETREDRAW, false, 0);
            //_ = SendMessage(MDIMain.Handle, WM_SETREDRAW, false, 0);
            //Point pos = this.Location;
            //Hide();
            //Show();
            //Location = pos;
            //_ = SendMessage(Handle, WM_SETREDRAW, true, 0);
            //_ = SendMessage(MDIMain.Handle, WM_SETREDRAW, true, 0);
        }
        #endregion

        #region Control Event Handlers
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
            string strLine;
            /*
            // ALWAYS reset search flags
            FindingForm.ResetSearch();
            */
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
                            if (e.KeyChar == ',') {
                                TipCurArg++;
                            }
                            RepositionTip(TipCmdToken);
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
                    strLine = fctb.Lines[check.iLine];
                    AGIToken snippetname = fctb.TokenFromPos(check);
                    if (snippetname.Text == "#") {
                        AGIToken starttoken = WinAGIFCTB.PreviousToken(strLine, snippetname);
                        if (starttoken.Text == ")") {
                            // snippet has args - backup to find the full snippet text
                            do {
                                starttoken = WinAGIFCTB.PreviousToken(strLine, starttoken);
                                if (starttoken.Text == "(") {
                                    break;
                                }
                            } while (starttoken.Type != AGITokenType.None);
                            if (starttoken.Type != AGITokenType.None) {
                                starttoken = WinAGIFCTB.PreviousToken(strLine, starttoken);
                                if (starttoken.Text[0] == '#') {
                                    // adjust snippetname
                                    snippetname.StartPos = starttoken.StartPos;
                                    snippetname.Text = strLine[snippetname.StartPos..snippetname.EndPos];
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
                    if (snippetname.StartPos > 0 && strLine[..snippetname.StartPos].Trim().Length == 0) {
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
            }
        }

        private void fctb_MouseDown(object sender, MouseEventArgs e) {
            if (lstDefines.Visible) {
                lstDefines.Visible = false;
                fctb.CaretVisible = true;
            }
            if (e.Button == MouseButtons.Right) {
                // move cursor if not on selection
                if (fctb.Selection.Length == 0) {
                    fctb.Selection.Start = fctb.PointToPlace(e.Location);
                    fctb.SelectionLength = 0;
                }
                else {
                    int spos = fctb.PlaceToPosition(fctb.Selection.Start);
                    int epos = fctb.PlaceToPosition(fctb.Selection.End);
                    int pos = fctb.PointToPosition(e.Location);
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
            if (e.Button == MouseButtons.Left && e.Location.X < (fctb.LeftIndent - 1)) {
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
                // use same calculation that FCTB uses to determine if 
                // the line number area is clicked:
                //    internal const int minLeftIndent = 8;
                //    isLineSelect = (e.Location.X < LeftIndentLine);
                //    private int LeftIndentLine
                //    {
                //        get { return LeftIndent - minLeftIndent / 2 - 3; }
                //    }
                SelectEntireLine = false;
                if (e.Button == MouseButtons.Left && e.Location.X < (fctb.LeftIndent - 1)) {
                    if (fctb.Selection.Start <= fctb.Selection.End) {
                        if (fctb.LinesCount > fctb.Selection.End.iLine + 1) {
                            fctb.Selection.End = new(0, fctb.Selection.End.iLine + 1);
                        }
                    }
                    else {
                        //fctb.Selection.Normalize();
                        //fctb.Selection.End = new(0, fctb.Selection.End.iLine + 1);
                        if (fctb.LinesCount > fctb.Selection.Start.iLine + 1) {
                            Place tmp = fctb.Selection.End;
                            fctb.Selection.Start = new(0, fctb.Selection.Start.iLine + 1);
                            fctb.Selection.End = tmp;
                        }
                    }
                    fctb.Invalidate();
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

        private void fctb_TextChanged(object sender, TextChangedEventArgs e) {
            AGISyntaxHighlight(e.ChangedRange);
            if (loading || !Visible) {
                return;
            }
            DefDirty = true;
            MarkAsChanged();
            spStatus.Text = "";
            spLine.Text = "Line: " + fctb.Selection.End.iLine;
            spColumn.Text = "Col: " + fctb.Selection.End.iChar;
        }

        private void fctb_ToolTipNeeded(object sender, ToolTipNeededEventArgs e) {
            if (!WinAGISettings.ShowDefTips.Value || picTip.Visible) {
                return;
            }
            AGIToken seltoken = fctb.TokenFromPos(e.Place);
            if (seltoken.Type != AGITokenType.Identifier) {
                return;
            }
            string strDefine = seltoken.Text;
            if (strDefine.Length == 0) {
                return;
            }
            // check locals first
            if (DefDirty) {
                BuildLDefLookup();
            }
            for (int i = 0; i < LDefLookup.Length; i++) {
                if (strDefine.Equals(LDefLookup[i].Name)) {
                    strDefine += " = " + LDefLookup[i].Value;
                    e.ToolTipText = strDefine;
                    return;
                }
            }
            if (EditGame is not null) {
                if (!EditGame.SierraSyntax) {
                    // next check globals
                    if (EditGame.IncludeGlobals) {
                        if (EditGame.GlobalDefines.ContainsName(strDefine)) {
                            strDefine += " = " + EditGame.GlobalDefines[strDefine].Value;
                            e.ToolTipText = strDefine;
                            return;
                        }
                    }
                    if (EditGame.IncludeIDs) {
                        // then ids; we will test logics, then views, then sounds, then pics
                        // as that's the order that defines are most likely to be used
                        for (int i = 0; i <= EditGame.Logics.Max; i++) {
                            if (IDefLookup[(int)AGIResType.Logic, i].Type != ArgType.None) {
                                if (strDefine.Equals(IDefLookup[(int)AGIResType.Logic, i].Name)) {
                                    strDefine += " = " + IDefLookup[(int)AGIResType.Logic, i].Value;
                                    e.ToolTipText = strDefine;
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i <= EditGame.Views.Max; i++) {
                            if (IDefLookup[(int)AGIResType.View, i].Type != ArgType.None) {
                                if (strDefine.Equals(IDefLookup[(int)AGIResType.View, i].Name)) {
                                    strDefine += " = " + IDefLookup[(int)AGIResType.View, i].Value;
                                    e.ToolTipText = strDefine;
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i <= EditGame.Sounds.Max; i++) {
                            if (IDefLookup[(int)AGIResType.Sound, i].Type != ArgType.None) {
                                if (strDefine.Equals(IDefLookup[(int)AGIResType.Sound, i].Name)) {
                                    strDefine += " = " + IDefLookup[(int)AGIResType.Sound, i].Value;
                                    e.ToolTipText = strDefine;
                                    return;
                                }
                            }
                        }
                        for (int i = 0; i <= EditGame.Pictures.Max; i++) {
                            if (IDefLookup[(int)AGIResType.Picture, i].Type != ArgType.None) {
                                if (strDefine.Equals(IDefLookup[(int)AGIResType.Picture, i].Name)) {
                                    strDefine += " = " + IDefLookup[(int)AGIResType.Picture, i].Value;
                                    e.ToolTipText = strDefine;
                                    return;
                                }
                            }
                        }
                    }
                }
                else {
                    // use sysdefines
                }
            }
            if (EditGame is null || (EditGame.IncludeReserved && !EditGame.SierraSyntax)) {
                // still no match, check reserved define
                TDefine[] tmpDefines = EditGame.ReservedDefines.All();
                for (int i = 0; i < tmpDefines.Length; i++) {
                    if (strDefine.Equals(tmpDefines[i].Name)) {
                        strDefine += " = " + tmpDefines[i].Value;
                        e.ToolTipText = strDefine;
                        return;
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
                if ((string)lstDefines.Tag != "defines") {
                    break;
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
            spLine.Size = new System.Drawing.Size(70, 18);
            spLine.Text = "Line: --";
            // 
            // spColumn
            // 
            spColumn.AutoSize = false;
            spColumn.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spColumn.BorderStyle = Border3DStyle.SunkenInner;
            spColumn.Name = "spColumn";
            spColumn.Size = new System.Drawing.Size(70, 18);
            spColumn.Text = "Col: --";
        }

        private bool ReadMsgs(ref MessageData[] Messages) {
            // all valid message declarations in strText are
            // put into the Messages() array; the MsgUsed array
            // is used to mark each declared message added
            //
            // if an error in the logic is detected that would
            // make it impossible to accurately update the message
            // section, the function returns false, and Messages(0)
            // is populated with the error code, Messages(1)
            // is populated with the line where the error was found,
            // and subsequent elements are populated with any
            // additional information regarding the error
            // error codes:
            //     1 = invalid msg number
            //     2 = duplicate msg number
            //     3 = not a string
            //     4 = stuff not allowed after msg declaration
            int intMsgNum;

            // get first message marker position
            AGIToken token = fctb.TokenFromPos(new Place(0, 0));
            while (token.Type != AGITokenType.None) {
                if (token.Type == AGITokenType.Identifier && token.Text == "#message") {
                    // next cmd should be msg number
                    token = fctb.NextToken(token);
                    if (token.Type != AGITokenType.Number || (intMsgNum = int.Parse(token.Text)) < 1 || intMsgNum > 255) {
                        // invalid msg number
                        Messages[0].Line = 1;
                        Messages[1].Line = token.StartPos;
                        // displayed lines are '1' based
                        Messages[2].Line = token.Line + 1;
                        return false;
                    }
                    if (Messages[intMsgNum].Declared) {
                        // user needs to fix message section first;
                        // return false, and use the Message structure to indicate
                        // what the problem is, and on which line it occurred
                        Messages[0].Line = 2;
                        Messages[1].Line = token.StartPos;
                        Messages[2].Line = token.Line + 1;
                        Messages[3].Line = intMsgNum;
                        return false;
                    }
                    // next cmd should be a string
                    token = fctb.NextToken(token);
                    switch (token.Type) {
                    case AGITokenType.String:
                        // valid string is >1 char, ends with '"' and doesn't end with '\"'
                        if (token.Text.Length == 1 || token.Text[^1] != '\"' || token.Text[^2] == '\\') {
                            Messages[0].Line = 5;
                            Messages[1].Line = token.StartPos;
                            Messages[2].Line = token.Line + 1;
                            Messages[3].Line = intMsgNum;
                            return false;
                        }
                        Messages[intMsgNum].Text = token.Text;
                        Messages[intMsgNum].Line = token.Line;
                        Messages[intMsgNum].Type = 0;
                        // check fo concatenation
                        AGIToken concattoken;
                        do {
                            // check for end of line
                            Place concatstart = new(token.StartPos, token.Line);
                            token = fctb.NextToken(token, false);
                            if (token.Type != AGITokenType.None && token.Type != AGITokenType.Comment) {
                                // stuff not allowed on line after msg declaration
                                Messages[0].Line = 4;
                                Messages[1].Line = concatstart.iChar;
                                Messages[2].Line = concatstart.iLine + 1;
                                Messages[3].Line = intMsgNum;
                                return false;
                            }
                            concattoken = fctb.NextToken(token, true);
                            while (concattoken.Type == AGITokenType.String) {
                                if (concattoken.Text[^1] == '\"') {
                                    // valid string is >1 char, ends with '"' and doesn't end with '\"'
                                    if (concattoken.Text.Length == 1 || concattoken.Text[^1] != '\"' || concattoken.Text[^2] == '\\') {
                                        Messages[0].Line = 5;
                                        Messages[1].Line = concattoken.StartPos;
                                        Messages[2].Line = concattoken.Line + 1;
                                        Messages[3].Line = intMsgNum;
                                        return false;
                                    }
                                }
                                // add it to current 
                                Messages[intMsgNum].Text = Messages[intMsgNum].Text[..^1] + concattoken.Text[1..];
                                Messages[intMsgNum].Concat++;
                                token = concattoken;
                            }
                        } while (concattoken.Type == AGITokenType.String);
                        // set flag to show message is declared
                        Messages[intMsgNum].Declared = true;
                        break;
                    case AGITokenType.Identifier:
                        // try replacing with define (locals, then globals, then reserved
                        bool blnDefFound = false;
                        for (int i = 0; i < LDefLookup.Length; i++) {
                            if (LDefLookup[i].Type == ArgType.DefStr) {
                                if (LDefLookup[i].Name == token.Text) {
                                    token.Text = LDefLookup[i].Value;
                                    blnDefFound = true;
                                    break;
                                }
                            }
                        }
                        if (!blnDefFound) {
                            if (EditGame.GlobalDefines.ContainsName(token.Text)) {
                                if (EditGame.GlobalDefines[token.Text].Type == ArgType.DefStr) {
                                    blnDefFound = true;
                                    break;
                                }
                            }
                        }
                        if (!blnDefFound) {
                            for (int i = 0; i < EditGame.ReservedDefines.ByArgType(ArgType.DefStr).Length; i++) {
                                if (EditGame.ReservedDefines.ByArgType(ArgType.DefStr)[i].Name == token.Text) {
                                    blnDefFound = true;
                                    break;
                                }
                            }
                        }
                        // if it was replaced, we accept whatever was used as
                        // the define name; if not replaced, it's error
                        if (blnDefFound) {
                            Messages[intMsgNum].Text = token.Text;
                            Messages[intMsgNum].Line = token.Line;
                            Messages[intMsgNum].Type = 2;
                            Messages[intMsgNum].Declared = true;
                            return true;
                        }
                        else {
                            // not a string or defined string
                            Messages[0].Line = 3;
                            Messages[1].Line = token.StartPos;
                            Messages[2].Line = token.Line + 1;
                            Messages[3].Line = intMsgNum;
                            return false;
                        }
                    default:
                        // not a string or identifer
                        Messages[0].Line = 3;
                        Messages[1].Line = token.StartPos;
                        Messages[2].Line = token.Line + 1;
                        Messages[3].Line = intMsgNum;
                        return false;
                    }
                }
                token = fctb.NextToken(token, true);
            }
            return true;
        }

        private AGIToken NextMsg(AGIToken token, string[] StrDefs) {
            // starting at position lngPos, step through cmds until a match is found for
            // a cmd that has a msg argument:
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
            int lngArg;

            AGIToken msgtoken = fctb.NextToken(token, true);
            while (msgtoken.Type != AGITokenType.None) {
                if (msgtoken.Type == AGITokenType.Identifier) {
                    lngArg = -1;
                    // check against list of cmds with msg arguments:
                    switch (msgtoken.Text.Length) {
                    case 3:
                        if (msgtoken.Text == "log") {
                            lngArg = 0;
                        }
                        break;
                    case 5:
                        if (msgtoken.Text == "print") {
                            lngArg = 0;
                        }
                        break;
                    case 7:
                        if (msgtoken.Text == "display") {
                            lngArg = 2;
                        }
                        else if (msgtoken.Text == "get.num") {
                            lngArg = 0;
                        }
                        break;
                    case 8:
                        if (msgtoken.Text == "print.at" || msgtoken.Text == "set.menu") {
                            lngArg = 0;
                        }
                        break;
                    case 10:
                        if (msgtoken.Text == "set.string" || msgtoken.Text == "get.string") {
                            lngArg = 1;
                        }
                        break;
                    case 11:
                        if (msgtoken.Text == "set.game.id") {
                            lngArg = 0;
                        }
                        break;
                    case 13:
                        if (msgtoken.Text == "set.menu.item") {
                            lngArg = 0;
                        }
                        break;
                    case 15:
                        if (msgtoken.Text == "set.cursor.char") {
                            lngArg = 0;
                        }
                        break;
                    }
                    if (lngArg >= 0) {
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
                        if (lngArg != 0) {
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
                                lngArg--;
                            } while (lngArg != 0);
                        }
                    }
                    else {
                        // check for string assignment
                        // s##="text"; or strdefine="text";
                        for (int i = 0; i < StrDefs.Length; i++) {
                            if (msgtoken.Text == StrDefs[i]) {
                                // possible string assignment
                                lngArg = 0;
                                break;
                            }
                        }
                        // if not found as a define, check for a string marker(s##)
                        if (lngArg == -1 && msgtoken.Text.Length > 0 && msgtoken.Text[0] == 's') {
                            // strip off the 's'
                            string strCmd = msgtoken.Text[1..];
                            int num;
                            if (int.TryParse(strCmd, out num)) {
                                // possible string assignment
                                // do we care what number? yes- must be 0-23
                                // in the off chance the user is working with
                                // a version that has a limit of 12 strings
                                // we will let the compiler worry about it
                                if (num >= 0 && num <= 23) {
                                    lngArg = 0;
                                }
                                else {
                                    // invalid string marker; error
                                    msgtoken.Number = -7;
                                    return msgtoken;
                                }
                            }
                        }
                        if (lngArg == 0) {
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
                                lngArg = -1;
                            }
                        }
                    }
                    // if a message token was found (lngArg >=0)
                    if (lngArg >= 0) {
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
                            for (int i = 0; i < LDefLookup.Length; i++) {
                                if (msgtoken.Text == LDefLookup[i].Name) {
                                    switch (LDefLookup[i].Type) {
                                    case ArgType.Msg:

                                        // TODO: verify if BuildLocalDef validates markers ('m1' etc) and strings
                                        msgtoken.Text = LDefLookup[i].Value;
                                        msgtoken.Number = 0;
                                        return msgtoken;

                                    case ArgType.DefStr:
                                        msgtoken.Text = LDefLookup[i].Value;
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
                                case ArgType.Msg:
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

        public void MessageCleanup() {
            MessageData[] Messages = new MessageData[256];
            string[] NewMsgs = [];
            bool blnKeepUnused, repeatAction = false;
            string[] strStrings = [];
            int intMsgCount = 0;
            string strMsg;
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
                RestoreFocusHack();
                // if canceled
                if (rtn == DialogResult.Cancel) {
                    return;
                }
                blnKeepUnused = (rtn == DialogResult.Yes);
            }
            else {
                blnKeepUnused = (WinAGISettings.WarnMsgs.Value == 1);
            }
            if (repeatAction) {
                if (blnKeepUnused) {
                    WinAGISettings.WarnMsgs.Value = 1;
                }
                else {
                    WinAGISettings.WarnMsgs.Value = 2;
                }
                WinAGISettings.WarnMsgs.WriteSetting(WinAGISettingsFile);
            }
            if (DefDirty) {
                BuildLDefLookup();
            }
            // check locals, globals, and reserved for string defines
            int lngCount = 0;
            for (int i = 0; i < LDefLookup.Length; i++) {
                if (LDefLookup[i].Type == ArgType.Str) {
                    Array.Resize(ref strStrings, ++lngCount);
                    strStrings[^1] = LDefLookup[i].Name;
                }
            }
            foreach (var define in EditGame.GlobalDefines.Values) {
                if (define.Type == ArgType.Str) {
                    Array.Resize(ref strStrings, ++lngCount);
                    strStrings[^1] = define.Name;
                }
            }
            // add the only resdef that's a string
            Array.Resize(ref strStrings, ++lngCount);
            strStrings[^1] = EditGame.ReservedDefines.ReservedStrings[0].Name;

            // next, get all messages that are predefined
            if (!ReadMsgs(ref Messages)) {
                // a problem was found that needs to be fixed before
                // messages can be cleaned up
                Place errorlocation = new(Messages[1].Line, Messages[2].Line);
                strMsg = "Syntax error in line " + Messages[2] + " must be corrected\n" +
                         "before message cleanup can continue:\n\n";
                switch (Messages[0].Line) {
                case 1:
                    // invalid msg number
                    MessageBox.Show(MDIMain,
                        strMsg + "Invalid message index number.",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                    RestoreFocusHack();
                    break;
                case 2:
                    // duplicate msg number
                    MessageBox.Show(MDIMain,
                        strMsg + "Message index " + Messages[3] + " is already in use.",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                    RestoreFocusHack();
                    break;
                case 3:
                    // msg val should be a string
                    MessageBox.Show(MDIMain,
                    strMsg + "Expected string value",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                    RestoreFocusHack();
                    break;
                case 4:
                    // stuff not allowed on line after msg declaration
                    MessageBox.Show(MDIMain,
                    strMsg + "Expected end of line",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                    RestoreFocusHack();
                    break;
                case 5:
                    // missing end quote
                    MessageBox.Show(MDIMain,
                        strMsg + "String is missing end quote mark",
                        "Syntax Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                    RestoreFocusHack();
                    break;
                }
                fctb.Selection.Start = errorlocation;
                fctb.Selection.End = new(fctb.GetLineLength(errorlocation.iLine), errorlocation.iLine);
                return;
            }
            if (blnKeepUnused) {
                for (int i = 1; i < 256; i++) {
                    if (Messages[i].Declared) {
                        intMsgCount++;
                    }
                }
            }
            AGIToken token = new();
            token = NextMsg(token, strStrings);
            while (token.Type != AGITokenType.None || token.Number < 0) {
                // check for error
                if (token.Number < 0) {
                    fctb.SelectLine(token.Line);
                    string msg = "Syntax error in line " + (token.Line + 1).ToString() + " must be corrected " +
                            "before message cleanup can continue:\n\n";

                    switch (token.Number) {
                    case -1:
                        // missing '(' after command
                        MessageBox.Show(MDIMain,
                            msg + "Expected '(' after command text.",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                        RestoreFocusHack();
                        break;
                    case -2:
                        // missing end quote
                        MessageBox.Show(MDIMain,
                            msg + "Missing quote mark (\") at end of string.",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                        RestoreFocusHack();
                        break;
                    case -3:
                        // arg not a string
                        MessageBox.Show(MDIMain,
                            msg + "Argument is not a string or msg marker ('m##').",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                        RestoreFocusHack();
                        break;
                    case -4:
                        // stuff not allowed after message string
                        MessageBox.Show(MDIMain,
                            msg + "Line break expected after message declaration.",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                        RestoreFocusHack();
                        break;
                    case -5:
                        // missing arg value
                        MessageBox.Show(MDIMain,
                            msg + "Argument value is missing",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                        RestoreFocusHack();
                        break;
                    case -6:
                        // missing comma after arg
                        MessageBox.Show(MDIMain,
                            msg + "Expected ',' after command arguments.",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                        RestoreFocusHack();
                        break;
                    case -7:
                        // invalid string marker
                        MessageBox.Show(MDIMain,
                            msg + "Invalid string marker (must be s0 - s23).",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                        RestoreFocusHack();
                        break;
                    case -8:
                        // invalid message marker
                        MessageBox.Show(MDIMain,
                            msg + "Invalid message marker (must be m1 - m255).",
                            "Syntax Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information, 0, 0,
                            WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                        RestoreFocusHack();
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
                                strMsg = "Syntax error in line " + token.Line + " must be corrected " +
                                         "before message cleanup can continue:\n\n";
                                MessageBox.Show(MDIMain,
                                    strMsg + "Invalid message marker value (must be 'm1' - 'm255')",
                                    "Syntax Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information, 0, 0,
                                    WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                                RestoreFocusHack();
                                return;
                            }
                            if (Messages[j].Declared) {
                                Messages[j].ByNumber = true;
                            }
                            else {
                                // error! this msg isn't defined
                                fctb.SelectLine(token.Line);
                                strMsg = "Syntax error in line " + token.Line + " must be corrected " +
                                         "before message cleanup can continue:\n\n";
                                MessageBox.Show(MDIMain,
                                    strMsg + "'" + token.Text + "' is not a declared message.",
                                    "Syntax Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information, 0, 0,
                                    WinAGIHelp, "htm\\commands\\syntax_fan.htm#messages");
                                RestoreFocusHack();
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
                                        if (!blnKeepUnused) {
                                            // increment msgcount
                                            intMsgCount++;
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
                                intMsgCount++;
                            }
                        }
                        if (intMsgCount >= 256) {
                            MessageBox.Show(MDIMain,
                                "There are too many messages being used by this logic. AGI only " +
                                "supports 255 messages per logic. Edit the logic to reduce the " +
                                "number of messages to 255 or less.",
                                "Too Many Messages",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation, 0, 0,
                                WinAGIHelp, "htm\\agi\\logics.htm#messages");
                            RestoreFocusHack();
                            return;
                        }
                    }
                }
                token = NextMsg(token, strStrings);
            }

            // Now add all newfound messages to the message array
            int newmsgnum = 1;
            for (int i = 0; i < NewMsgs.Length; i++) {
                // if message is not in use (byref or bynum), we can overwrite it
                do
                    if (blnKeepUnused && Messages[newmsgnum].Declared) {
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
            strMsg = "\n[ **************************************\n[ DECLARED MESSAGES\n[ **************************************\n";
            for (int i = 1; i < 256; i++) {
                // if used by ref or num, OR if keeping all and it's declared,add this msg
                if (Messages[i].ByRef || Messages[i].ByNumber || (Messages[i].Declared && blnKeepUnused)) {
                    strMsg += "#message " + i + " " + Messages[i].Text + "\n";
                }
            }
            // delete everything from here to end of text
            //  and replace with the messages
            start = new(0, insertline);
            end = new(fctb.Lines[fctb.LinesCount - 1].Length, fctb.LinesCount - 1);
            fctb.Selection.Start = start;
            fctb.Selection.End = end;
            fctb.SelectedText = strMsg;
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
                i = 1;
                string[] arglist = CodeSnippets[i].ArgTips.Split(',');
                while (snippetvalue.Contains("%" + i.ToString())) {
                    string argvalue = "";
                    // TODO: use arglist to provide better prompt
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
            rtfLogic1.SelectionColor = System.Drawing.Color.FromArgb(128, red, green, blue);
            rtfLogic1.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            rtfLogic1.DefaultStyle = new TextStyle(rtfLogic1.DefaultStyle.ForeBrush, rtfLogic1.DefaultStyle.BackgroundBrush, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            if ((FormMode == LogicFormMode.Logic && WinAGISettings.HighlightLogic.Value) ||
                (FormMode == LogicFormMode.Text && WinAGISettings.HighlightText.Value)) {
                RefreshSyntaxStyles();
                AGISyntaxHighlight(rtfLogic1.Range);
            }
            else {
                // clear all styles of changed range
                rtfLogic1.Range.ClearStyle(StyleIndex.All);
            }
            rtfLogic1.TabLength = WinAGISettings.LogicTabWidth.Value;
            rtfLogic2.ForeColor = WinAGISettings.SyntaxStyle[0].Color.Value;
            rtfLogic2.BackColor = WinAGISettings.EditorBackColor.Value;
            rtfLogic2.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            rtfLogic2.DefaultStyle = new TextStyle(rtfLogic2.DefaultStyle.ForeBrush, rtfLogic2.DefaultStyle.BackgroundBrush, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            if ((FormMode == LogicFormMode.Logic && WinAGISettings.HighlightLogic.Value) ||
                (FormMode == LogicFormMode.Text && WinAGISettings.HighlightText.Value)) {
                RefreshSyntaxStyles();
                AGISyntaxHighlight(rtfLogic2.Range);
            }
            else {
                // clear all styles of changed range
                rtfLogic2.Range.ClearStyle(StyleIndex.All);
            }
            rtfLogic2.TabLength = WinAGISettings.LogicTabWidth.Value;
            documentMap1.BackColor = WinAGISettings.EditorBackColor.Value;
            lstDefines.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, WinAGISettings.SyntaxStyle[0].FontStyle.Value);
            lstDefines.Height = 6 * fctb.CharHeight;
            lstDefines.Width = 20 * fctb.CharWidth;
            lstDefines.Columns[0].Width = lstDefines.Width;
        }

        private void RefreshSyntaxStyles() {
            CommentStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[1].Color.Value);
            CommentStyle.FontStyle = WinAGISettings.SyntaxStyle[1].FontStyle.Value;
            StringStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[2].Color.Value);
            StringStyle.FontStyle = WinAGISettings.SyntaxStyle[2].FontStyle.Value;
            KeyWordStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[3].Color.Value);
            KeyWordStyle.FontStyle = WinAGISettings.SyntaxStyle[3].FontStyle.Value;
            TestCmdStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[4].Color.Value);
            TestCmdStyle.FontStyle = WinAGISettings.SyntaxStyle[4].FontStyle.Value;
            ActionCmdStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[5].Color.Value);
            ActionCmdStyle.FontStyle = WinAGISettings.SyntaxStyle[5].FontStyle.Value;
            InvalidCmdStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[6].Color.Value);
            InvalidCmdStyle.FontStyle = WinAGISettings.SyntaxStyle[6].FontStyle.Value;
            NumberStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[7].Color.Value);
            NumberStyle.FontStyle = WinAGISettings.SyntaxStyle[7].FontStyle.Value;
            ArgIdentifierStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[8].Color.Value);
            ArgIdentifierStyle.FontStyle = WinAGISettings.SyntaxStyle[8].FontStyle.Value;
            DefIdentifierStyle.ForeBrush = new SolidBrush(WinAGISettings.SyntaxStyle[9].Color.Value);
            DefIdentifierStyle.FontStyle = WinAGISettings.SyntaxStyle[9].FontStyle.Value;
        }

        private void AGISyntaxHighlight(FastColoredTextBoxNS.Range range) {
            // clear all styles of changed range
            range.ClearStyle(StyleIndex.All);

            range.SetStyle(CommentStyle, CommentStyleRegEx1, RegexOptions.Multiline);
            range.SetStyle(CommentStyle, CommentStyleRegEx2, RegexOptions.Multiline);
            range.SetStyle(StringStyle, StringStyleRegEx);
            if (EditGame is not null && EditGame.SierraSyntax) {
                range.SetStyle(KeyWordStyle, SierraKeyWordStyleRegEx);
            }
            else {
                range.SetStyle(KeyWordStyle, FanKeyWordStyleRegEx);
                range.SetStyle(TestCmdStyle, TestCmdStyleRegex);
                range.SetStyle(ActionCmdStyle, ActionCmdStyleRegEx);
            }
            range.SetStyle(InvalidCmdStyle, InvalidCmdStyleRegEx);
            range.SetStyle(NumberStyle, NumberStyleRegEx);
            range.SetStyle(ArgIdentifierStyle, ArgIdentifierStyleRegEx);
            range.SetStyle(DefIdentifierStyle, DefIdentifierStyleRegEx);

            // reset folding markers
            range.ClearFoldingMarkers();
            range.SetFoldingMarkers("{", "}");
        }

        public bool LoadLogic(Logic loadlogic, bool quiet = false) {
            InGame = loadlogic.InGame;
            int codepage;
            if (InGame) {
                LogicNumber = (int)loadlogic.Number;
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
                Text = sLOGED + ResourceName(EditLogic, true, true);
            }
            else {
                // get file name from SourceFile (if this is a source file)
                // or from ResFile (if this is a compiled logic file)
                // or from ID (if a new logic)
                if (EditLogic.SourceFile.Length > 0) {
                    Text = sLOGED + Path.GetFileName(EditLogic.SourceFile);
                }
                else if (EditLogic.ResFile.Length > 0) {
                    Text = sLOGED + Path.GetFileNameWithoutExtension(EditLogic.ResFile);
                    Text += "." + WinAGISettings.DefaultExt.Value;
                }
                else {
                    Text = sLOGED + EditLogic.ID;
                }
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
            loading = false;
            return true;
        }

        internal bool LoadText(string filename) {
            InGame = false;
            LogicNumber = 256;
            if (filename.Length > 0) {
                if (File.Exists(filename)) {
                    try {
                        rtfLogic1.OpenFile(filename);
                        TextFilename = filename;
                    }
                    catch {
                        return false;
                    }
                }
                else {
                    return false;
                }
                Text = sLOGED + Path.GetFileName(filename);
            }
            else {
                TextCount++;
                Text = sLOGED + "NewTextFile" + TextCount.ToString();
            }
            rtfLogic1.ClearUndo();
            IsChanged = rtfLogic1.IsChanged = filename.Length == 0;
            if (IsChanged) {
                Text = CHG_MARKER + Text;
            }
            mnuRSave.Enabled = !IsChanged;
            MDIMain.btnSaveResource.Enabled = !IsChanged;

            // maximize, if that's the current setting
            if (WinAGISettings.MaximizeLogics.Value) {
                WindowState = FormWindowState.Maximized;
            }
            loading = false;
            return true;
        }

        private string NewTextFileName(string filename = "") {
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
            DefaultResDir = JustPath(MDIMain.SaveDlg.FileName);
            return MDIMain.SaveDlg.FileName;
        }

        public void ImportLogic(string importfile) {
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(importfile));
            if (!File.Exists(importfile)) {
                throw new FileNotFoundException("Import file not found", importfile);
            }
            string strFile = "";

            MDIMain.UseWaitCursor = true;
            Logic tmpLogic = new();
            // open file to see if it is sourcecode or compiled logic
            try {
                using FileStream fsNewLog = new(importfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using StreamReader srNewLog = new(fsNewLog);
                strFile = srNewLog.ReadToEnd();
                srNewLog.Dispose();
                fsNewLog.Dispose();
            }
            catch (Exception) {
                // ignore errors
            }
            // check if logic is a compiled logic
            // (check for existence of char '0')
            bool blnSource = !strFile.Contains('\0');
            // import the logic (and check for error)
            if (blnSource) {
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
                        RestoreFocusHack();
                        return;
                    case ResourceErrorType.LogicSourceAccessError:
                        MessageBox.Show(MDIMain,
                            "A file access error has occurred. Unable to read this file.",
                            "File Access Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        RestoreFocusHack();
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
            UpdateIncludes();
            EditLogic.SourceText = fctb.Text;
        }

        public void SaveLogicSource() {
            // saves the source code to file; to save the 
            // AGI resource, use the Compile method
            UseWaitCursor = true;
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
                UpdateIncludes();
                EditGame.Logics[LogicNumber].SourceText = fctb.Text;

                if (EditGame.UseLE && EditLogic.IsRoom) {
                    // need to update the layout editor and the layout data file with new exit info
                    UpdateExitInfo(UpdateReason.UpdateRoom, LogicNumber, EditGame.Logics[LogicNumber]);
                }
                // use the ingame logic to save the SOURCE
                EditGame.Logics[LogicNumber].SaveSource();
                // update the editor to match
                Place start = fctb.Selection.Start;
                Place end = fctb.Selection.End;
                FastColoredTextBoxNS.Range vr = fctb.VisibleRange;
                fctb.SelectAll();
                fctb.SelectedText = EditGame.Logics[LogicNumber].SourceText;
                fctb.Selection.Start = start;
                fctb.Selection.End = end;
                fctb.DoRangeVisible(vr);
                if (!loaded) {
                    EditGame.Logics[LogicNumber].Unload();
                }
                // remove existing TODO items and decompiler warnings for this logic
                MDIMain.ClearWarnings(AGIResType.Logic, (byte)LogicNumber, [EventType.TODO, EventType.DecompWarning]);

                // update TODO list and decomp warnings for this logic
                List<TWinAGIEventInfo> TODOs = ExtractTODO((byte)LogicNumber, rtfLogic1.Text, EditLogic.ID);
                foreach (TWinAGIEventInfo tmpInfo in TODOs) {
                    MDIMain.AddWarning(tmpInfo);
                }
                // check for Decompile warnings
                List<TWinAGIEventInfo> DecompWarnings = ExtractDecompWarn((byte)LogicNumber, rtfLogic1.Text, EditLogic.ID);
                foreach (TWinAGIEventInfo tmpInfo in DecompWarnings) {
                    MDIMain.AddWarning(tmpInfo);
                }
            }
            else if (EditLogic.SourceFile.Length != 0) {
                // preserve all extended characters by using default codepage
                rtfLogic1.SaveToFile(EditLogic.SourceFile, Encoding.Default);
            }
            else {
                // not in a game, and no sourcefile assigned yet -
                UseWaitCursor = false;
                ExportLogic();
                return;
            }
            UpdateStatusBar();
            MarkAsSaved();
            if (InGame && !Compiling) {
                RefreshTree(AGIResType.Logic, LogicNumber);
            }
            if (InGame) {
                Text = sLOGED + ResourceName(EditLogic, true, true);
            }
            else {
                Text = sLOGED + Path.GetFileName(EditLogic.SourceFile);
            }
            UseWaitCursor = false;
        }

        public void SaveTextFile(string filename = "") {
            if (filename.Length == 0) {
                if (TextFilename.Length == 0) {
                    filename = DefaultResDir + "NewTextFile" + TextCount.ToString() + ".txt";
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
            MarkAsSaved();
        }

        public void ExportLogic() {
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
                            RestoreFocusHack();
                            return;
                        }
                        RestoreFocusHack();
                    }
                }
                if (Base.ExportLogic(EditLogic, true) == 1) {
                    // because EditLogic is not the actual ingame logic its
                    // ID needs to be reset back to the ingame value
                    EditLogic.ID = EditGame.Logics[LogicNumber].ID;
                }
                UpdateStatusBar();
            }
            else {
                // not in game; save-as is only operation allowed
                string strExportName = NewSourceName(EditLogic, InGame);
                if (strExportName.Length != 0) {
                    EditLogic.SourceFile = strExportName;
                    // preserve all extended characters by using default codepage
                    rtfLogic1.SaveToFile(EditLogic.SourceFile, Encoding.Default);
                    EditLogic.ID = Path.GetFileName(strExportName);
                    MarkAsSaved();
                }
            }
        }

        public void ToggleInGame() {
            // toggles the game state of this logic

            DialogResult rtn;
            string strExportName;
            bool blnDontAsk = false;

            if (InGame) {
                if (WinAGISettings.AskExport.Value) {
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Do you want to export '" + EditLogic.ID + "' source before removing it from your game?",
                        "Don't ask this question again",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        "Export Logic Before Removal", ref blnDontAsk);
                    WinAGISettings.AskExport.Value = !blnDontAsk;
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
                    EditLogic.SourceFile = EditGame.SrcResDir + EditLogic.ID + "." + EditGame.SourceExt;
                    strExportName = NewSourceName(EditLogic, InGame);
                    if (strExportName.Length > 0) {
                        // preserve all extended characters by using default codepage
                        rtfLogic1.SaveToFile(strExportName, Encoding.Default);
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
                // remove the logic (force-closes this editor)
                RemoveLogic((byte)LogicNumber);
            }
            else {
                // add to game 
                using frmGetResourceNum frmGetNum = new(GetRes.AddInGame, AGIResType.Logic, 0);
                if (frmGetNum.ShowDialog(MDIMain) != DialogResult.Cancel) {
                    LogicNumber = frmGetNum.NewResNum;
                    // change id before adding to game
                    EditLogic.ID = frmGetNum.txtID.Text;
                    UpdateIncludes();
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
                    MarkAsSaved();
                    InGame = true;
                    MDIMain.btnAddRemove.Image = MDIMain.imageList1.Images[20];
                    MDIMain.btnAddRemove.Text = "Remove Logic";
                }
                RestoreFocusHack();
            }
            btnCompile.Enabled = InGame;
            btnMsgClean.Enabled = InGame;
            UpdateExitInfo(InGame ? UpdateReason.ShowRoom : UpdateReason.HideRoom, LogicNumber, EditLogic);
        }

        public void RenumberLogic() {
            string oldid = EditLogic.ID;
            int oldnum = LogicNumber;
            byte NewResNum = GetNewNumber(AGIResType.Logic, (byte)LogicNumber);
            if (NewResNum != LogicNumber) {
                // update ID (it may have changed if using default ID)
                EditLogic.ID = EditGame.Logics[NewResNum].ID;
                LogicNumber = NewResNum;
                Text = sLOGED + ResourceName(EditLogic, InGame, true);
                if (IsChanged) {
                    Text = CHG_MARKER + Text;
                }
                if (EditLogic.ID != oldid) {
                    if (File.Exists(EditGame.SrcResDir + oldid + "." + EditGame.SourceExt)) {
                        SafeFileMove(EditGame.SrcResDir + oldid + "." + EditGame.SourceExt, EditGame.SrcResDir + EditGame.Logics[NewResNum].ID + "." + EditGame.SourceExt, true);
                    }
                }
                if (EditGame.UseLE) {
                    UpdateExitInfo(UpdateReason.RenumberRoom, oldnum, null, NewResNum);
                }
            }
        }

        public void UpdateIncludes() {
            // For FAN syntax ingame logics only, this verifies the correct include lines are
            // at the start of the source code. They should be the first three lines of the
            // file, following any comment header.
            //
            // If the lines are not there and they should be, add them. If there
            // and not needed, remove them.
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
                        if (reservedpos >= 0 && reservedpos < idpos) reservedpos++;
                        if (badreservedpos >= 0 && badreservedpos < idpos) badreservedpos++;
                        if (globalspos >= 0 && globalspos < idpos) globalspos++;
                        if (badglobalspos >= 0 && badglobalspos < idpos) badglobalspos++;
                        fctb.InsertLine(insertpos, fctb.Lines[idpos]);
                        fctb.RemoveLine(++idpos);
                    }
                }
                else if (badidpos >= 0) {
                    // move it, but update to correct text
                    if (reservedpos >= 0 && reservedpos < idpos) reservedpos++;
                    if (badreservedpos >= 0 && badreservedpos < idpos) badreservedpos++;
                    if (globalspos >= 0 && globalspos < idpos) globalspos++;
                    if (badglobalspos >= 0 && badglobalspos < idpos) badglobalspos++;
                    fctb.InsertLine(insertpos, "#include \"resourceids.txt\"");
                    fctb.RemoveLine(++badidpos);
                }
                else {
                    // insert it
                    if (reservedpos >= 0) reservedpos++;
                    if (badreservedpos >= 0) reservedpos++;
                    if (globalspos >= 0) globalspos++;
                    if (badglobalspos >= 0) globalspos++;
                    fctb.InsertLine(insertpos, "#include \"resourceids.txt\"");
                }
                insertpos++;
            }
            else {
                if (idpos >= 0) {
                    if (reservedpos > idpos) reservedpos--;
                    if (badreservedpos > idpos) reservedpos--;
                    if (globalspos > idpos) globalspos--;
                    if (badglobalspos > idpos) globalspos--;
                    fctb.RemoveLine(idpos);
                }
                else if (badidpos >= 0) {
                    if (reservedpos > badidpos) reservedpos--;
                    if (badreservedpos > badidpos) reservedpos--;
                    if (globalspos > badidpos) globalspos--;
                    if (badglobalspos > badidpos) globalspos--;
                    fctb.RemoveLine(badidpos);
                }
            }
            if (EditGame.IncludeReserved) {
                if (reservedpos >= 0) {
                    if (reservedpos != insertpos) {
                        // move it
                        if (globalspos >= 0 && globalspos < reservedpos) globalspos++;
                        if (badglobalspos >= 0 && badglobalspos < reservedpos) badglobalspos++;
                        fctb.InsertLine(insertpos, fctb.Lines[reservedpos]);
                        fctb.RemoveLine(++reservedpos);
                    }
                }
                else if (badreservedpos >= 0) {
                    // move it, but update to correct text
                    if (globalspos >= 0 && globalspos < badreservedpos) globalspos++;
                    if (badglobalspos >= 0 && badglobalspos < badreservedpos) badglobalspos++;
                    fctb.InsertLine(insertpos, "#include \"reserved.txt\"");
                    fctb.RemoveLine(++badreservedpos);
                }
                else {
                    // insert it
                    if (globalspos >= 0) globalspos++;
                    if (badglobalspos >= 0) globalspos++;
                    fctb.InsertLine(insertpos, "#include \"reserved.txt\"");
                }
                insertpos++;
            }
            else {
                if (reservedpos >= 0) {
                    if (globalspos > reservedpos) globalspos--;
                    if (badglobalspos > reservedpos) badglobalspos--;
                    fctb.RemoveLine(reservedpos);
                }
                else if (badreservedpos >= 0) {
                    if (globalspos > badreservedpos) globalspos--;
                    if (badglobalspos > badreservedpos) badglobalspos--;
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

        public void EditLogicProperties(int FirstProp) {
            string id = EditLogic.ID;
            string description = EditLogic.Description;
            if (GetNewResID(AGIResType.Logic, LogicNumber, ref id, ref description, InGame, 1)) {
                UpdateID(id, description);
            }
        }

        public void UpdateID(string id, string description) {
            if (EditLogic.Description != description) {
                EditLogic.Description = description;
            }
            if (EditLogic.ID != id) {
                EditLogic.ID = id;
                Text = sLOGED + ResourceName(EditLogic, InGame, true);
                if (IsChanged) {
                    Text = CHG_MARKER + Text;
                }
            }
        }

        /// <summary>
        /// Builds the local list of defines for use by the tooltip 
        /// and showlist functions.
        /// </summary>
        private void BuildLDefLookup() {
            string strLine;
            TDefine tmpDefine = new();
            bool blnSub;

            // if cursor is already the wait cursor, we need to
            // NOT restore it after completion; calling function
            // will do that
            blnSub = MDIMain.UseWaitCursor;
            if (!blnSub) {
                MDIMain.UseWaitCursor = true;
            }
            // add local defines (if duplicate defines, only first one will be used)

            LDefLookup = [];
            for (int i = 0; i < fctb.LinesCount; i++) {
                // remove comments and trim the line
                string comment = "";
                strLine = StripComments(fctb[i].Text, ref comment);
                if (strLine.Length > 0) {
                    if (strLine.Left(8).ToLower() == "#define ") {
                        strLine = strLine[8..].Trim();
                    }
                    else {
                        continue;
                    }
                    // there has to be at least one space
                    if (strLine.Contains(' ')) {
                        // split it by position of first space
                        string[] strings = strLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (strings.Length == 2) {
                            tmpDefine.Name = strings[0];
                            tmpDefine.Value = strings[1];
                        }
                        else {
                            // no good; get next line
                            continue;
                        }
                    }
                    else {
                        // no good; get next line
                        continue;
                    }
                    // don't bother validating; just use it as is
                    tmpDefine.Type = DefTypeFromValue(tmpDefine.Value);
                    Array.Resize(ref LDefLookup, LDefLookup.Length + 1);
                    LDefLookup[^1] = tmpDefine;
                }
            }
            DefDirty = false;


            if (!blnSub) {
                MDIMain.UseWaitCursor = false;
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

        private int EditArgNumber(WinAGIFCTB fctb, Place place) {
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
            Font tipfont = new Font("Arial", 9, FontStyle.Regular);
            Font tipbold = new Font("Arial", 9, FontStyle.Bold);
            Point startPoint = new Point(0, 0);
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

        private bool CmdHasArgs(ref AGIToken cmdtoken) {
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
            Font tipfont = new Font("Arial", 9, FontStyle.Regular);
            Font tipbold = new Font("Arial", 9, FontStyle.Bold);
            Point startPoint = new Point(0, 0);
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

        private void AddIfUnique(string strName, int lngIcon, string strValue) {
            if (lstDefines.Items.IndexOfKey(strName) != -1) {
                return;
            }
            // OK to add it
            ListViewItem item = lstDefines.Items.Add(strName, strName, lngIcon);
            item.ToolTipText = strValue;
            return;
        }

        private void BuildDefineList(ArgListType ArgType = ArgListType.All) {
            // adds defines to the listview, which is then presented to user
            // the list defaults to all defines; but when a specific argument
            // type is needed, it will adjust to show only the defines
            // of that particular type

            // if no game loaded, skip globals and resIDs (locals and reserved only)

            bool blnAdd;
            string strLine;

            if (!ListDirty && !DefDirty && ArgType == ListType) {
                return;
            }
            lstDefines.Items.Clear();
            lstDefines.Tag = "defines";

            // locals not needed if looking for only a ResID
            if (ArgType < ArgListType.Logic) {
                if (DefDirty) {
                    BuildLDefLookup();
                }
                // add local defines (if duplicate defines, only first one will be used)
                for (int i = 0; i < LDefLookup.Length; i++) {
                    // add these local defines IF
                    //     types match OR
                    //     argtype is ALL OR
                    //     argtype is (msg OR invobj) AND deftype is defined string OR
                    //     argtype matches a special type
                    blnAdd = false;
                    if ((int)LDefLookup[i].Type == (int)ArgType) {
                        blnAdd = true;
                    }
                    else {
                        switch (ArgType) {
                        case ArgListType.All:
                            blnAdd = true;
                            break;
                        case ArgListType.IfArg:
                            // variables and flags
                            blnAdd = LDefLookup[i].Type == Engine.ArgType.Var || LDefLookup[i].Type == Engine.ArgType.Flag;
                            break;
                        case ArgListType.OthArg:
                            // variables and strings
                            blnAdd = LDefLookup[i].Type == Engine.ArgType.Var || LDefLookup[i].Type == Engine.ArgType.Str;
                            break;
                        case ArgListType.Values:
                            // variables and numbers
                            blnAdd = LDefLookup[i].Type == Engine.ArgType.Var || LDefLookup[i].Type == Engine.ArgType.Num;
                            break;
                        case ArgListType.Msg or ArgListType.IObj:
                            blnAdd = LDefLookup[i].Type == Engine.ArgType.DefStr;
                            break;
                        }
                    }
                    if (blnAdd) {
                        AddIfUnique(LDefLookup[i].Name, (int)LDefLookup[i].Type, LDefLookup[i].Value);
                    }
                }
            }
            else if (ArgType == ArgListType.ActionCmds) {
                // add all action commands
                for (int i = 0; i < Commands.ActionCommands.Length; i++) {
                    strLine = Commands.ActionCommands[i].FanName;
                    lstDefines.Items.Add(strLine, strLine, 26).ToolTipText = strLine;
                }
                ListDirty = false;
                ListType = ArgType;
                return;
            }
            else if (ArgType == ArgListType.TestCmds) {
                // add all action commands
                for (int i = 0; i < Commands.TestCommands.Length; i++) {
                    strLine = Commands.TestCommands[i].FanName;
                    lstDefines.Items.Add(strLine, strLine, 26).ToolTipText = strLine;
                }
                ListDirty = false;
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
                    blnAdd = false;
                    if ((int)define.Type == (int)ArgType) {
                        blnAdd = true;
                    }
                    else {
                        switch (ArgType) {
                        case ArgListType.All:
                            blnAdd = true;
                            break;
                        case ArgListType.IfArg:
                            // variables and flags
                            blnAdd = define.Type == Engine.ArgType.Var || define.Type == Engine.ArgType.Flag;
                            break;
                        case ArgListType.OthArg:
                            // variables and strings
                            blnAdd = define.Type == Engine.ArgType.Var || define.Type == Engine.ArgType.Str;
                            break;
                        case ArgListType.Values:
                            // variables and numbers
                            blnAdd = define.Type == Engine.ArgType.Var || define.Type == Engine.ArgType.Num;
                            break;
                        case ArgListType.Msg or ArgListType.IObj:
                            blnAdd = define.Type == Engine.ArgType.DefStr;
                            break;
                        }
                    }
                    if (blnAdd) {
                        // don't add if already defined
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
                            (int)ArgType - 14 == j) {
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
                        strLine = EditGame.InvObjects[i].ItemName;
                        if (strLine != "?") {
                            if (strLine.Contains(QUOTECHAR)) {
                                strLine = strLine.Replace("\"", "\\" + QUOTECHAR);
                            }
                            lstDefines.Items.Add("\"" + strLine + "\"", "\"" + strLine + "\"", 17).ToolTipText = "i" + i;
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
                            strLine = "\"" + EditGame.WordList.GroupByIndex(i).Words[0] + "\"";
                            lstDefines.Items.Add(strLine, strLine, 22).ToolTipText = EditGame.WordList.GroupByIndex(i).GroupNum.ToString();
                        }
                    }
                    break;
                }
            }
            // lastly, check for reserved defines option (if not looking for a resourceID)
            TDefine[] tmpDefines = null;
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
                    blnAdd = false;
                    if ((int)tmpDefines[i].Type == (int)ArgType) {
                        blnAdd = true;
                    }
                    else {
                        switch (ArgType) {
                        case ArgListType.All:
                            blnAdd = true;
                            break;
                        case ArgListType.IfArg:
                            // variables and flags
                            blnAdd = tmpDefines[i].Type == Engine.ArgType.Var || tmpDefines[i].Type == Engine.ArgType.Flag;
                            break;
                        case ArgListType.OthArg:
                            // variables and strings
                            blnAdd = tmpDefines[i].Type == Engine.ArgType.Var || tmpDefines[i].Type == Engine.ArgType.Str;
                            break;
                        case ArgListType.Values:
                            // variables and numbers
                            blnAdd = tmpDefines[i].Type == Engine.ArgType.Var || tmpDefines[i].Type == Engine.ArgType.Num;
                            break;
                        case ArgListType.Msg or ArgListType.IObj:
                            blnAdd = tmpDefines[i].Type == Engine.ArgType.DefStr;
                            break;
                        }
                    }
                    if (blnAdd) {
                        // don't add if already defined
                        AddIfUnique(tmpDefines[i].Name, 26 + (int)tmpDefines[i].Type, tmpDefines[i].Value);
                    }
                }
            }
            ListDirty = false;
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
            ListDirty = true;
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
            pPos = this.PointToClient(pPos);
            lstDefines.Top = pPos.Y + fctb.CharHeight;
            lstDefines.Left = pPos.X;
        }

        private void ShowSnippetList() {
            // displays snippets in a list box
            if (fctb.Lines[fctb.Selection.Start.iLine][..fctb.Selection.Start.iChar].Length == 0) {
                SnipIndent = fctb.Selection.Start.iChar;
            }
            else {
                SnipIndent = 0;
            }
            ListDirty = true;
            lstDefines.Items.Clear();
            lstDefines.Tag = "snippets";
            for (int i = 0; i < CodeSnippets.Length; i++) {
                lstDefines.Items.Add(CodeSnippets[i].Name).ToolTipText = CodeSnippets[i].Value;
                lstDefines.Items[i].Tag = CodeSnippets[i].ArgTips;
            }
            lstDefines.Items[0].Selected = true;
            lstDefines.Items[0].EnsureVisible();
            // save pos and text
            DefStartPos = fctb.Selection.Start;
            DefEndPos = fctb.Selection.End;
            fctb.CaretVisible = false;
            PrevText = "";
            DefText = "";
            // position it
            PositionListBox();

            lstDefines.ShowItemToolTips = true;
            lstDefines.Visible = true;
            fctb.CaretVisible = false;
            lstDefines.Select();
        }

        private void ShowDefineList() {
            // displays defines in a list box
            // that user can select from to replace current word (if cursor
            // is in a word) or insert at current position (if cursor is
            // in between words)
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
                string strText = fctb.SelectedText;
                bool selected = false;
                // check for value match first
                if (Regex.IsMatch(strText, @"[vfscimo]\d{1,3}\b")) {
                    foreach (ListViewItem tmpItem in lstDefines.Items) {
                        if (strText == tmpItem.ToolTipText) {
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
                        switch (strText.ToUpper().CompareTo(tmpItem.Text.ToUpper())) {
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

        private void ShowCommandList() {
            // displays available commands in a list box
            // that user can select from to replace current word (if cursor
            // is in a word) or insert at current position (if cursor is
            // in between words)
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
                string strText = fctb.SelectedText;
                bool selected = false;
                // check for value match first
                if (!selected) {
                    bool found = false;
                    foreach (ListViewItem tmpItem in lstDefines.Items) {
                        switch (strText.ToLower().CompareTo(tmpItem.Text.ToLower())) {
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
            string strTopic = "htm\\winagi\\editor_logic.htm";

            // TODO: add context help

            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, strTopic);
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
                RestoreFocusHack();
                switch (rtn) {
                case DialogResult.Yes:
                    SaveLogicSource();
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
                    return true;
                }
            }
            if (InGame && !EditGame.Logics[LogicNumber].Compiled) {
                // only logics can be in game, so no need to check for text file
                bool blnDontAsk = false;
                DialogResult rtn = DialogResult.Yes;
                switch (WinAGISettings.WarnCompile.Value) {
                case AskOption.Ask:
                    rtn = MsgBoxEx.Show(MDIMain,
                       "Do you want to compile this logic before closing it?",
                       "Save Logic Source", // Text,
                                   MessageBoxButtons.YesNoCancel,
                       MessageBoxIcon.Question,
                                   "Always take this action when closing a logic source file.", ref blnDontAsk);
                    if (blnDontAsk) {
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

        public void UpdateStatusBar() {
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
            FindingForm.ResetSearch();
        }

        private void MarkAsSaved() {
            if (FormMode == LogicFormMode.Logic) {
                Text = sLOGED + ResourceName(EditLogic, InGame, true);
            }
            else {
                Text = "Text Editor - " + Path.GetFileName(TextFilename);
            }
            IsChanged = false;
            rtfLogic1.IsChanged = false;
            mnuRSave.Enabled = false;
            MDIMain.btnSaveResource.Enabled = false;
        }
        #endregion
    }
}
