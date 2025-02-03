using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Editor.Base;
using static WinAGI.Common.API;
using System.IO;

namespace WinAGI.Editor {
    public partial class frmWordsEdit : Form {
        public bool InGame;
        public bool IsChanged;
        public WordList EditWordList;
        private bool closing = false;
        private string EditWordListFilename;
        private bool blnRefreshLogics = false;
        private bool GroupMode = true;
        private int EditGroupIndex, EditWordIndex;
        private Font defaultfont;
        private Font boldfont;

        public frmWordsEdit() {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);
            this.UpdateStyles();
            InitializeComponent();

            InitFonts();
            MdiParent = MDIMain;
        }

        #region Form Event Handlers
        private void frmWordsEdit_Activated(object sender, EventArgs e) {
            if (FindingForm.Visible) {
                if (FindingForm.rtfReplace.Visible) {
                    FindingForm.SetForm(FindFormFunction.ReplaceObject, InGame);
                }
                else {
                    FindingForm.SetForm(FindFormFunction.FindObject, InGame);
                }
            }
            statusStrip1.Items["spGroupCount"].Text = "Group Count: " + EditWordList.GroupCount;
            statusStrip1.Items["spWordCount"].Text = "Word Count: " + EditWordList.WordCount;
        }

        private void frmWordsEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmWordsEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // ensure object is cleared and dereferenced

            if (EditWordList != null) {
                EditWordList.Unload();
                EditWordList = null;
            }
            if (InGame) {
                WEInUse = false;
                WordEditor = null;
            }
        }

        private void frmWordsEdit_Resize(object sender, EventArgs e) {
            //_ = SendMessage(this.Handle, WM_SETREDRAW, false, 0);
            //_ = SendMessage(lstGroups.Handle, WM_SETREDRAW, false, 0);
            //_ = SendMessage(lstWords.Handle, WM_SETREDRAW, false, 0);

            int newWidth = (this.ClientSize.Width - 15) / 2;
            lstGroups.Width = newWidth;
            lstWords.Width = newWidth;
            lstWords.Left = lstGroups.Right + 5;
            label1.Left = 5 + (lstGroups.Width - label1.Width) / 2;
            label2.Left = lstWords.Left + (lstWords.Width - label2.Width) / 2;
            lstWords.Height = ClientSize.Height - lstWords.Top - 5;
            if (GroupMode) {
                lstGroups.Height = lstWords.Height;
            }
            //_ = SendMessage(lstWords.Handle, WM_SETREDRAW, true, 0);
            //_ = SendMessage(lstGroups.Handle, WM_SETREDRAW, true, 0);
            //_ = SendMessage(this.Handle, WM_SETREDRAW, true, 0);
            //this.Refresh();
        }
        #endregion

        #region Menu Event Handlers
        internal void SetResourceMenu() {
            mnuRSave.Enabled = IsChanged;
            MDIMain.mnuRSep3.Visible = true;
            if (EditGame is null) {
                // no game is open
                MDIMain.mnuRImport.Enabled = false;
                mnuRExport.Text = "Save As ...";
                // mnuRProperties no change
                // mnuRMerge no change
                mnuRGroupCheck.Enabled = false;
            }
            else {
                // if a game is loaded, base import is also always available
                MDIMain.mnuRImport.Enabled = true;
                mnuRExport.Text = InGame ? "Export WORDS.TOK" : "Save As ...";
                // mnuRProperties no change
                // mnuRMerge no change
                mnuRGroupCheck.Enabled = InGame;
            }
        }

        public void mnuRSave_Click(object sender, EventArgs e) {
            SaveWords();
        }

        public void mnuRExport_Click(object sender, EventArgs e) {
            ExportWords();
        }

        public void mnuRProperties_Click(object sender, EventArgs e) {
            EditProperties();
        }

        private void mnuRMerge_Click(object sender, EventArgs e) {
            MessageBox.Show("merge word lists");
        }

        private void mnuRGroupCheck_Click(object sender, EventArgs e) {
            MessageBox.Show("group check");
        }

        private void SetEditMenu() {

        }

        private void ResetEditMenu() {
            mnuEUndo.Enabled = true;
            mnuECut.Enabled = true;
            mnuECopy.Enabled = true;
            mnuEPaste.Enabled = true;
            mnuEDelete.Enabled = true;
            mnuEClear.Enabled = true;
            mnuEInsertGroup.Enabled = true;
            mnuEInsertWord.Enabled = true;
            mnuEFind.Enabled = true;
            mnuEFindAgain.Enabled = true;
            mnuEReplace.Enabled = true;
            mnuEditItem.Enabled = true;
            mnuEFindInLogic.Enabled = true;
            mnuEditMode.Enabled = true;
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            mnuEdit.DropDownItems.AddRange([mnuEUndo, mnESep0, mnuECut, mnuECopy, mnuEPaste, mnuEDelete, mnuEClear, mnuEInsertGroup, mnuEInsertWord, mnuESep1, mnuEFind, mnuEFindAgain, mnuEReplace, mnuESep2, mnuEditItem, mnuEFindInLogic, mnuEditMode]);
            SetEditMenu();
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            cmWords.Items.AddRange([mnuEUndo, mnESep0, mnuECut, mnuECopy, mnuEPaste, mnuEDelete, mnuEClear, mnuEInsertGroup, mnuEInsertWord, mnuESep1, mnuEFind, mnuEFindAgain, mnuEReplace, mnuESep2, mnuEditItem, mnuEFindInLogic, mnuEditMode]);
            ResetEditMenu();
        }

        private void cmWords_Opening(object sender, CancelEventArgs e) {
            SetEditMenu();
        }

        private void cmWords_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void mnuEUndo_Click(object sender, EventArgs e) {

        }

        private void mnuECut_Click(object sender, EventArgs e) {

        }

        private void mnuECopy_Click(object sender, EventArgs e) {

        }

        private void mnuEPaste_Click(object sender, EventArgs e) {

        }

        private void mnuEDelete_Click(object sender, EventArgs e) {

        }

        private void mnuEClear_Click(object sender, EventArgs e) {

        }

        private void mnuEAddGroup_Click(object sender, EventArgs e) {

        }

        private void mnuEAddWord_Click(object sender, EventArgs e) {

        }

        private void mnuERenumber_Click(object sender, EventArgs e) {

        }

        private void mnuEFind_Click(object sender, EventArgs e) {

        }

        private void mnuEFindAgain_Click(object sender, EventArgs e) {

        }

        private void mnuEReplace_Click(object sender, EventArgs e) {

        }

        private void mnuEMode_Click(object sender, EventArgs e) {
            byte[] obj;
            // only if not editing a word
            GroupMode = !GroupMode;
            if (GroupMode) {
                string curword = lstWords.Text;
                label1.Text = "Groups";
                lstGroups.Height = lstWords.Height;
                lstGroups.Items.Clear();
                for (int i = 0; i < EditWordList.GroupCount; i++) {
                    lstGroups.Items.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
                }
                lstGroups.SelectionMode = SelectionMode.One;
                lstGroups.SelectedIndex = EditGroupIndex;
                lstWords.Text = curword;
                obj = (byte[])EditorResources.ResourceManager.GetObject("ewi_bygroup");
            }
            else {
                string curword = lstWords.Text;
                label1.Text = "Group Number:";
                lstGroups.Items.Clear();
                lstGroups.Items.Add((EditWordList.GroupByIndex(EditGroupIndex).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(EditGroupIndex).GroupName);
                lstGroups.Height = txtGroupEdit.Height;
                lstGroups.SelectionMode = SelectionMode.None;
                lstWords.Items.Clear();
                foreach (AGIWord word in EditWordList) {
                    lstWords.Items.Add(word.WordText);
                }
                lstWords.Text = curword;
                obj = (byte[])EditorResources.ResourceManager.GetObject("ewi_byword");
            }
            Stream stream = new MemoryStream(obj);
            tbbMode.Image = (Bitmap)Image.FromStream(stream);
            label1.Left = 5 + (lstGroups.Width - label1.Width) / 2;

            spStatus.Text = "group: " + EditGroupIndex + " word: " + EditWordIndex;
        }

        private void mnuEFindLogic_Click(object sender, EventArgs e) {

        }
        #endregion

        private void lstWords_SelectedIndexChanged(object sender, EventArgs e) {
            EditWordIndex = lstWords.SelectedIndex;
            if (GroupMode) {
            }
            else {
                EditGroupIndex = EditWordList.GroupIndexFromNumber(EditWordList[EditWordIndex].Group);
                lstGroups.Items[0] = (EditGroupIndex.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(EditGroupIndex).GroupName;
            }
            spStatus.Text = "group: " + EditGroupIndex + "(" + lstGroups.SelectedIndex + ")" + " word: " + EditWordIndex + "(" + lstWords.SelectedIndex + ")";
        }

        private void lstGroups_SelectedIndexChanged(object sender, EventArgs e) {
            if (GroupMode) {
                EditGroupIndex = lstGroups.SelectedIndex;
                EditWordIndex = -1;
                lstWords.Items.Clear();
                if (lstGroups.SelectedIndex >= 0) {
                    string grp = ((string)lstGroups.SelectedItem)[..((string)lstGroups.SelectedItem).IndexOf(':')];
                    int group = int.Parse(grp);
                    foreach (string word in EditWordList.GroupByNumber(group)) {
                        lstWords.Items.Add(word);
                    }
                }
                if (EditGroupIndex == 1 || EditGroupIndex == 9999) {
                    lstWords.Font = boldfont;
                    lstWords.ForeColor = Color.DarkGray;
                    if (lstWords.Items.Count == 0) {
                        if (EditGroupIndex == 1) {
                            lstWords.Items.Add("<group 1: any word>");
                        }
                        else {
                            lstWords.Items.Add("<group 9999: rest of line>");
                        }
                    }
                }
                else {
                    lstWords.Font = defaultfont;
                    lstWords.ForeColor = Color.Black;
                }
            }
            else {
                //lstGroups.SelectedIndex = -1;
            }
            spStatus.Text = "group: " + EditGroupIndex + "(" + lstGroups.SelectedIndex + ")" + " word: " + EditWordIndex + "(" + lstWords.SelectedIndex + ")";
        }

        #region temp code
        void wordsfrmcode() {
            /*

  private UndoCol As Collection
  
  private Mode As WTMode
  private Enum WTMode
    wtByGroup
    wtByWord
  End Enum
  
  private AddNewWord As Boolean
  private AddNewGroup As Boolean
  
  private SelMode As eSelMode
  private SelGroup As Long
  private EditOldWord As String
  private DraggingWord As Boolean
  private mX As Single, mY As Single
  private PickChar As Boolean
  
  private OldGroupNum As Long, OldGroupIndex As Long // used for dragging words to another group
  
  private lngRowHeight As Long
  private SplitOffset As Long
  
  private Enum eSelMode
    smNone
    smWord
    smGroup
  End Enum
  
  private Const WE_MARGIN As Long = 5
  private Const SPLIT_WIDTH = 4 // in pixels
  private Const MIN_SPLIT_V = 160 // in pixels
  private Const TBW = 49 // toolbar width + 2 pixels
  private Const LGT = 21 // lstGroups.Top
  
  private CalcWidth As Long, CalcHeight As Long
  private Const MIN_HEIGHT = 120 // 361
  private Const MIN_WIDTH = 240 // 380

  public PrevWLBWndProc As Long
  public PrevGLBWndProc As Long
  public PrevGrpTBWndProc As Long
  
  private blnRecurse As Boolean
  // YES refresh all logics:
  //  x deleting words/groups
  //  x renumber group
  //  x move word
  //  x edit word
  // NO don't refresh:
  //   adding new words
  //   adding new groups
  //   updating description
  private blnRefreshLogics As Boolean
  
public void MenuClickHelp()
  
  // help
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Words_Editor.htm"
return;

}

private void FindWordInLogic(ByVal SearchWord As String, ByVal FindSynonyms As Boolean)

  // call findinlogic with current word
  // or word group
  
  // find synonym option currently does nothing
  
  // reset logic search
  FindingForm.ResetSearch
    
  // set global search values
  GFindText = QUOTECHAR & SearchWord & QUOTECHAR
  GFindDir = fdAll
  GMatchWord = true
  GMatchCase = false
  GLogFindLoc = flAll
  GFindSynonym = FindSynonyms
  GFindGrpNum = WordsEdit.Group(lstGroups.ListIndex).GroupNum
  SearchType = rtWords
  
  // set it to match desired search parameters
    // set find dialog to find textinlogic mode
    FindingForm.SetForm ffFindWordsLogic, true
    
    // show the form
    FindingForm.Show , frmMDIMain
  
  // ensure this form is the search form
  SearchForm = Me
  
return;
}

// private void AutoUpdate()
//   // build array of changed words
//   // then step through all logics; find
//   // all commands with word arguments;
//   // compare arguments against changed word list and
//   // make changes in sourcecode as necessary, then save the source
// 
//   Dim tmpLogic As AGILogic, i As Long, j As Long
//   Dim strOld() As String, strNew() As String
//   Dim blnUnloadRes As Boolean
//   Dim lngPos1 As Long, lngPos2 As Long
// 
//   // hide find form
//   FindingForm.Visible = false
// 
//   // show wait cursor
//   MDIMain.UseWaitCursor = true;
//   // show progress form
//   frmProgress.Text = "Updating Words in Logics"
//   frmProgress.lblProgress.Text = "Searching..."
//   frmProgress.pgbStatus.Max = Logics.Count
//   frmProgress.pgbStatus.Value = 0
//   frmProgress.Show vbModeless, frmMDIMain
//   frmProgress.Refresh
// 
//   for ( i = 0 To VocabularyWords.GroupCount - 1) {
//     // get group number
//     lngGrp = VocabularyWords.Group(i).GroupNum
// 
//     // check if this group exists in new list
//     if ( WordsEdit.GroupN(lngGrp)  == null ) {
//       // group is deleted
//       blnDeleted = true
//       Err.Clear
//     // OR if group has no name
//     } else if ( LenB(WordsEdit.GroupN(lngGrp).GroupName) = 0 ) {
//       blnDeleted = true
//     }
// 
//     // if an error,
//     if ( Err.Number <> 0 ) {
// 
//     }
// 
//     // if deleted
//     if ( blnDeleted ) {
//       // add to update list
//       ReDim Preserve strOld[j]
//       ReDim Preserve strNew[j]
//       strOld[j] = QUOTECHAR & VocabularyWords.Group(i).GroupName & QUOTECHAR
//       strNew[j] = CStr(lngGroup)
//       j = j + 1
//     // if group name as changed
//     } else if ( WordsEdit.GroupN(lngGrp).GroupName <> VocabularyWords.GroupN(lngGrp).GroupName ) {
//       // add to update list
//       ReDim Preserve strOld[j]
//       ReDim Preserve strNew[j]
//       // change to new word group name
//       strOld[j] = QUOTECHAR & VocabularyWords.GroupN(lngGrp).GroupName & QUOTECHAR
//       strNew[j] = QUOTECHAR & WordsEdit.GroupN(lngGrp).GroupName & QUOTECHAR
//       j = j + 1
//     }
// 
//     // reset deleted flag
//     blnDeleted = false
//   }
// 
//   // step through all logics
//   foreach (tmpLogic In Logics) {
//     // open if necessary
//     blnUnloadRes = !tmpLogic.Loaded
//     if ( blnUnloadRes ) {
//       tmpLogic.Load
//     }
// 
//     // step through code, looking for 'said' and 'word.to.string' commands
//     lngPos2 = FindNextToken(tmpLogic.SourceText, lngPos1, "said", true, true)
// 
//     while (lngPos != 0) {
//       // said' syntax is : said(word1, word2, word3,...)
//       // words can be numeric word values OR string values
//     }
// 
//     // unload if necessary
//     if ( blnUnloadRes ) {
//       tmpLogic.Unload
//     }
//  }
// 
// 
// 
// 
// 
//   // close the progress form
//   Unload frmProgress
// 
//   // re-enable form
//   frmMDIMain.Enabled = true
//   Screen.MousePointer = vbDefault
//   frmMDIMain.SetFocus
// }
// 

public void NewWords()
  
  // creates a new word list file
  Dim i As Long
  
  // set changed status and caption
  IsChanged = true
  WrdCount = WrdCount + 1
  Text = sDM & "Words Editor - NewWords" & CStr(WrdCount)
  
  // clear filename
  WordsEdit.ResFile = vbNullString
  
  switch ( Mode) {
  case wtByGroup
    // add word groups to listbox
    lstGroups.Clear
    for ( i = 0 To WordsEdit.GroupCount - 1) {
      // add group
      lstGroups.AddItem CStr(WordsEdit.Group[i].GroupNum) & " - " & UCase$(WordsEdit.Group[i].GroupName)
    }
    // select first group
    lstGroups.ListIndex = 0
  }
return;
}

private Function NextGrpNum() As Long

  // just get next available number following current selection
  Dim lngTgtGrp As Long, i As Long

  // search forward
  lngTgtGrp = Val(lstGroups.Text) + 1
  
  for ( i = lngTgtGrp To &HFFFF&) {
    if ( !WordsEdit.GroupExists(i) ) {
      NextGrpNum = i
      return;
    }
  }

  // if not found, try going backwards
  for ( i = lngTgtGrp - 1 To 0 Step -1) {
    if ( !WordsEdit.GroupExists(i) ) {
      NextGrpNum = i
      return;
    }
  }
  
  // if still not found, means user has 64K words! IMPOSSIBLE I SAY!
  NextGrpNum = -1
}

private void ReplaceAll(ByVal FindText As String, ByVal MatchWord As Boolean, ByVal ReplaceText As String)

  // replace all occurrences of FindText with ReplaceText
  
  Dim i As Long, j As Long
  Dim lngCount As Long
  Dim lngGroup As Long, lngOldGrp As Long
  Dim NextUndo As WordsUndo
  Dim strFindWord As String, strReplaceWord As String
  
  // if replacing and new text is the same
  if ( StrComp(FindText, ReplaceText, vbTextCompare) = 0 ) {
    // exit
    return;
  }
  
  // blank replace text not allowed for words
  if ( LenB(ReplaceText) = 0 ) {
    MsgBox "Blank replacement text is not allowed.", vbInformation + vbOKOnly, "Replace All"
    return;
  }
  
  // validate replace word (no special characters, etc)
  // what characters are allowed for words????
  // ####
  
  
  // if nothing in wordlist,
  if ( WordsEdit.WordCount = 0 ) {
    MsgBox "Word list is empty.", vbOKOnly + vbInformation, "Replace All"
    return;
  }
  
  // show wait cursor
  MDIMain.UseWaitCursor = true;
  
  // create new undo object
  NextUndo = new WordsUndo
  NextUndo.Action = ReplaceAll
  // use description property to indicate replace mode
  NextUndo.Description = CStr(MatchWord)
  
  if ( MatchWord ) {
    // does findtext exist?
    lngGroup = WordsEdit(FindText).Group
    
    // if no error, then searchword exists
    if ( Err.Number = 0 ) {
      // i is group where replacement is occurring
      Err.Clear
      // add info to undo object
      NextUndo.GroupNo = lngGroup
      NextUndo.OldWord = FindText
      NextUndo.Word = ReplaceText
      
      // assume replacement word does not exist in a different group
      // validate the assumption by trying to get the groupnumber directly
      lngOldGrp = WordsEdit(ReplaceText).Group
      // if no error,
      if ( Err.Number = 0 ) {
        // remove the replacement word
        WordsEdit.RemoveWord ReplaceText
      } else {
        // not found; reset old group num
        lngOldGrp = -1
      }
      Err.Clear
      
      // save oldgroup in undo object
      NextUndo.OldGroupNo = lngOldGrp
      
      // change word in this group by deleting findword
      WordsEdit.RemoveWord FindText
      // and adding replaceword
      WordsEdit.AddWord ReplaceText, lngGroup
      // ensure group lngGroup has correct name
      UpdateGroupName lngGroup
      // if word was removed from another group
      // (by checking if lngOldGrp is a valid group index)
      if ( lngOldGrp <> -1 ) {
        // ensure group lngOldGrp has correct name
        UpdateGroupName lngOldGrp
      }
      
      // update list boxes
      if ( Val(lstGroups.Text) = lngGroup Or (Val(lstGroups.Text) = lngOldGrp And lngOldGrp <> -1) ) {
        UpdateWordList true
      }
      // set Count to one
      lngCount = 1
    }
  } else {
    // need to step through all groups and all words
    // remove old words if the replace creates duplicates,
    // and create undo object that holds all this...
  
  
    // step through all groups
    for ( i = 0 To WordsEdit.GroupCount - 1) {
      // step through all words
      for ( j = 0 To WordsEdit.Group(i).WordCount - 1) {
        // need to manually check for end of group Count
        // because wordcount may change dynamically as words
        // are added and removed based on the changes
        if ( j > WordsEdit.Group(i).WordCount - 1 ) {
          break;
        }
        
        // if there is a match
        if ( InStr(1, WordsEdit.Group(i).Word[j], FindText, vbTextCompare) <> 0 ) {
          strFindWord = WordsEdit.Group(i).Word[j]
          strReplaceWord = Replace(WordsEdit.Group(i).Word[j], FindText, ReplaceText)
          
          // i is the group INDEX not the group NUMBER;
          // get group number
          lngGroup = WordsEdit.Group(i).GroupNum
          
          // assume replacement word does not exist in another group
          // validate assumption by trying to get the groupnumber directly
          lngOldGrp = WordsEdit(strReplaceWord).Group
          // if no error,
          if ( Err.Number = 0 ) {
            // remove the replacement word
            WordsEdit.RemoveWord strReplaceWord
          } else {
            // not found; reset old group num
            lngOldGrp = -1
          }
          Err.Clear
          
          // change word in this group by deleting findword
          WordsEdit.RemoveWord strFindWord
          // and adding replaceword
          WordsEdit.AddWord strReplaceWord, lngGroup
          // ensure group i has correct name
          UpdateGroupName lngGroup
          
          // if word came from a different group
          if ( lngOldGrp <> -1 ) {
            // update that groupname too
            UpdateGroupName lngOldGrp
          }
          
          // add to undo
          if ( lngCount = 0 ) {
            // add group, oldgroup, word, oldword
            NextUndo.Word = CStr(lngGroup) & "|" & CStr(lngOldGrp) & "|" & strFindWord & "|" & strReplaceWord
          } else {
             NextUndo.Word = NextUndo.Word & "|" & CStr(lngGroup) & "|" & CStr(lngOldGrp) & "|" & strFindWord & "|" & strReplaceWord
          }
          // increment counter
          lngCount = lngCount + 1
          // need to restart search at beginning of word
          // because changes in words will affect the
          // order;
          // set j to -1 so the next j statement
          // resets it to 0
          j = -1
        }
      }
    }
    
    // if something found,
    if ( lngCount <> 0 ) {
      // force update
      UpdateWordList true
    }
  }
  
  // if nothing found,
  if ( lngCount = 0 ) {
    MsgBox "Search text not found.", vbInformation, "Replace All"
  } else {
    // add undo
    AddUndo NextUndo
    
    // show how many replacements made
    MsgBox "The specified region has been searched. " & CStr(lngCount) & " replacements were made.", vbInformation, "Replace All"
  }
  
  // restore mousepointer
  Screen.MousePointer = vbDefault
return;
}

public void BeginFind()

  // each form has slightly different search parameters
  // and procedure; so each form will get what it needs
  // from the form, and update the global search parameters
  // as needed
  // 
  // that's why each search form cheks for changes, and
  // sets the global values, instead of doing it once inside
  // the FindingForm code
  
  // when searching for a word in a logic, the first time the
  // user presses the 'find' button, the SearchForm is the
  // Words Editor; so we need to send this search request
  // to the FindInLogics function; after that, if found,
  // the SearchForm will switch to the LogicEditor, and
  // future presses of the Find button will search logics
  // as expected
  
    // if searching in logics
    if ( FindingForm.FormFunction = ffFindWordsLogic ) {
      // begin a logic search
      FindInLogic GFindText, fdAll, true, false, flAll, false, vbNullString
      return;
    }
        
    switch ( FindingForm.FormAction) {
    case faFind:
      FindInWords GFindText, GMatchWord, GFindDir
    case faReplace:
      FindInWords GFindText, GMatchWord, GFindDir, true, GReplaceText
    case faReplaceAll:
      ReplaceAll GFindText, GMatchWord, GReplaceText
    case faCancel:
      // don't do anything
    }
}

public void MenuClickDelete()
  
  Dim i As Long
  
  switch ( SelMode) {
  case smWord:
    if ( lstWords.ListIndex <> -1 ) {
      // RARE, but check for reserved groups
      if ( WordsEdit.Group(lstGroups.ListIndex).GroupNum = 1 ) {
        if ( MsgBoxEx("Group '1' is a reserved group. Deleting its placeholder is not" & vbCrLf & "advised. See Help file for a detailed explanation." & vbCrLf & vbCrLf & "Are you sure you want to delete it?", vbExclamation + vbYesNo + vbMsgBoxHelpButton, "Deleting Reserved Group Placeholder", WinAGIHelp, "htm\winagi\Words_Editor.htm#reserved") = vbNo ) {
          return;
        }
      } else if ( WordsEdit.Group(lstGroups.ListIndex).GroupNum = 9999 ) {
        // not recommended
        if ( MsgBoxEx("Group '9999' is a reserved group. Deleting its placeholder is not" & vbCrLf & "advised. See Help file for a detailed explanation." & vbCrLf & vbCrLf & "Are you sure you want to delete it?", vbExclamation + vbYesNo + vbMsgBoxHelpButton, "Deleting Reserved Group Placeholder", WinAGIHelp, "htm\winagi\Words_Editor.htm#reserved") = vbNo ) {
          return;
        }
      }
      
      DelWord CurrentWord()
    }
    
  case smGroup:
    if ( lstGroups.ListIndex <> -1 ) {
      i = lstGroups.ListIndex
      DelGroup Val(lstGroups.Text)
      // select next group
      if ( i < lstGroups.ListCount ) {
        lstGroups.ListIndex = i
      } else {
        lstGroups.ListIndex = lstGroups.ListCount - 1
      }
    }
    
  }
  
  switch ( Mode) {
  case wtByGroup:
    UpdateWordList true
  case wtByWord:
  }
  
  SetEditMenu
return;
}

public void MenuClickECustom1()

  // edit a word
  Dim tmpWidth As Single, tmpGroup As Long, tmpWord As Long
  
  // if no group selected,
  if ( lstGroups.ListIndex = -1 ) {
    return;
  }
  
  tmpGroup = Val(lstGroups.Text)
  tmpWord = lstWords.ListIndex
  
  // if no word selected,
  if ( tmpWord = -1 ) {
    return;
  }
  
  if ( tmpGroup = 9999 ) {
    // not recommended
    if ( MsgBoxEx("Group '9999' is a reserved group. Changing its placeholder is" & vbCrLf & "not advised. See Help file for a detailed explanation." & vbCrLf & vbCrLf & "Are you sure you want to change it?", vbExclamation + vbYesNo + vbMsgBoxHelpButton, "Editing a Reserved Group", WinAGIHelp, "htm\winagi\Words_Editor.htm#reserved") = vbNo ) {
      return;
    }
  }
  if ( tmpGroup = 1 ) {
    // not recommended
    if ( MsgBoxEx("Group '1' is a reserved group. Changing its placeholder is" & vbCrLf & "not advised. See Help file for a detailed explanation." & vbCrLf & vbCrLf & "Are you sure you want to change it?", vbExclamation + vbYesNo + vbMsgBoxHelpButton, "Editing a Reserved Group", WinAGIHelp, "htm\winagi\Words_Editor.htm#reserved") = vbNo ) {
      return;
    }
  }
  
 // save word being edited
  EditOldWord = WordsEdit.GroupN(tmpGroup).Word(tmpWord)
  
  if ( (lstWords.ListCount * lngRowHeight + 4 > lstWords.Height) ) {
    tmpWidth = lstWords.Width - 22
  } else {
    tmpWidth = lstWords.Width - 5
  }
  
 // begin edit of word
   // move textbox
    rtfWord.Move lstWords.Left + 3, (lstWords.ListIndex - lstWords.TopIndex) * lngRowHeight + lstWords.Top + 2, tmpWidth, lngRowHeight
   // copy word to text box and select entire word
    rtfWord.Text = ""
    rtfWord.TextB = EditOldWord
    rtfWord.Selection.Range.StartPos = 0
    rtfWord.Selection.Range.EndPos = .Range.Length
   // show textbox
    rtfWord.Visible = true
   // set focus to the textbox
    rtfWord.SetFocus
  
 // disable edit menu
  frmMDIMain.mnuEdit.Enabled = false
 //  and toolbar editing buttons
    Me.Toolbar1.Buttons.Item(1).Enabled = false
    Me.Toolbar1.Buttons.Item(2).Enabled = false
    Me.Toolbar1.Buttons.Item(3).Enabled = false
    Me.Toolbar1.Buttons.Item(4).Enabled = false
    Me.Toolbar1.Buttons.Item(6).Enabled = false
    Me.Toolbar1.Buttons.Item(7).Enabled = false
return;
}

public void MenuClickECustom2()

 // if a word is selected
  if ( lstWords.ListIndex <> -1 ) {
   // use this word; assume not looking for synonyms
    FindWordInLogic CPToUnicode(lstWords.Text, SessionCodePage), false
 // if a group is selected
  } else if ( lstGroups.ListIndex <> -1 ) {
   // use groupname; assume looking for synonyms
    FindWordInLogic CPToUnicode(WordsEdit.Group(lstGroups.ListIndex).GroupName, SessionCodePage), true
  }
}

public void MenuClickECustom3()

 // toggle between edit mode (bygroup) or list mode (by word)
  Dim i As Long
  Dim oldGrp As Long, OldWord As String
  
  Mode = 1 - Mode // 1-0=1  1-1=0
  switch ( Mode) {
  case wtByGroup:
    Toolbar1.Buttons("mode").Image = 10
    lblGroups.Text = "Groups"
   // save current selection
    oldGrp = Val(lstGroups.Text)
    OldWord = lstWords.Text
    
   // refresh grouplist
    RebuildGroupList oldGrp, false //  WordsEdit(0).Group, false
   // force update to wordlist
    UpdateWordList true
    if ( Len(OldWord) > 0 ) {
      lstWords.Text = OldWord
    }
    
  case wtByWord:
    Toolbar1.Buttons("mode").Image = 11
    lblGroups.Text = "Group"
   // save current selection
    oldGrp = Val(lstGroups.Text)
    OldWord = lstWords.Text
    
    lstGroups.Clear
    lstGroups.AddItem ""
    lstWords.Clear
    for ( i = 0 To WordsEdit.WordCount - 1) {
      lstWords.AddItem WordsEdit(i).WordText
    }
    lstGroups.ListIndex = 0
    if ( Len(OldWord) > 0 ) {
      lstWords.Text = OldWord
    } else {
      lstWords.Text = WordsEdit.GroupN(oldGrp).GroupName
    }
  }
  SetEditMenu
return;
}
private void NewGroup()

 // inserts a new group, with next available group number
 // then, a new blank word is added
    
 // VERY RARE, but make sure listbox is NOT full
  if ( WordsEdit.GroupCount = &H7FFF ) {
   // just ignore
    return;
  }
  
 // add the group
  AddGroup NextGrpNum()
  AddNewGroup = true
  
 // add a new word (but without undo)
  NewWord true
return;
}


private void UpdateGroupName(ByVal GroupNo As Long)

 // updates the group list for the correct name
 //  only used in ByGroup mode
  Dim i As Long
  
  if ( Mode <> wtByGroup ) {
    return;
  }
  
 // groups 0, 1, and 9999 never change group name
  if ( GroupNo = 0 Or GroupNo = 1 Or GroupNo = 9999 ) {
    return;
  }
  
 // should never happen but...
  if ( !WordsEdit.GroupExists(GroupNo) ) {
    return;
  }
  
 // find the group
  for ( i = 0 To lstGroups.ListCount - 1) {
    if ( Val(lstGroups.List(i)) = GroupNo ) {
     // update group name
      lstGroups.List(i) = CStr(GroupNo) & " - " & UCase$(CPToUnicode(WordsEdit.GroupN(GroupNo).GroupName, SessionCodePage))
      break;
    }
  }
}
private void UpdateStatusBar()

  MainStatusBar.Panels("GroupCount").Text = "Total Groups: " & CStr(WordsEdit.GroupCount)
  MainStatusBar.Panels("WordCount").Text = "Total Words: " & CStr(WordsEdit.WordCount)
}

private void UpdateWordList(Optional ByVal ForceUpdate As Boolean = false)

 // only used in ByGroup mode?
  if ( Mode <> wtByGroup ) {
    Debug.Assert false
    return;
  }
  
  Dim lngGrpNum As Long
  Dim i As Long
  Dim blnAdd As Boolean
  
 // if dragging a word,
  if ( DraggingWord ) {
   // dont update no matter what
    return;
  }
  
  lngGrpNum = Val(lstGroups.Text)
  
#if ( DEBUGMODE <> 1 ) {
 // disable window painting for the listbox until done
  SendMessage lstWords.hWnd, WM_SETREDRAW, 0, 0
#}

 // if change in group
  if ( lngGrpNum <> SelGroup Or ForceUpdate ) {
   // load the words for this group into the word listbox
   // clear the word list
    lstWords.Clear
   // set default wordlistbox properties
    lstWords.FontBold = false
    lstWords.ForeColor = vbBlack
    lstWords.Enabled = true
    
   // rare, but groups 0, 1, 9999 may not exist in the wordlist even though
   // they are always present in the form's listbox; so check that the
   // group exists before counting words
    if ( WordsEdit.GroupExists(lngGrpNum) ) {
     // add all the words in the group
      for ( i = 0 To WordsEdit.GroupN(lngGrpNum).WordCount - 1) {
        lstWords.AddItem CPToUnicode(WordsEdit.GroupN(lngGrpNum).Word(i), SessionCodePage)
      }
    }
    
   // if a reserved group is selected
    if ( lngGrpNum = 0 ) {
     // if doesn't exist, or if no words actually in the group
      if ( !WordsEdit.GroupExists(1) ) {
        blnAdd = true
      } else if ( WordsEdit.GroupN(1).WordCount = 0 ) {
        blnAdd = true
      }
      
    } else if ( lngGrpNum = 1 ) {
     // if doesn't exist, or if no words actually in the group
      if ( !WordsEdit.GroupExists(1) ) {
        blnAdd = true
      } else if ( WordsEdit.GroupN(1).WordCount = 0 ) {
        blnAdd = true
      }
      if ( blnAdd ) {
       // add uneditable placeholder
        lstWords.AddItem "<group 1: any word>"
      }
     // always mark it as 'special'
      lstWords.ForeColor = &HC0C0C0
      lstWords.FontBold = true
      
    } else if ( lngGrpNum = 9999 ) {
     // if doesn't exist, or if no words actually in the group
      if ( !WordsEdit.GroupExists(9999) ) {
        blnAdd = true
      } else if ( WordsEdit.GroupN(9999).WordCount = 0 ) {
        blnAdd = true
      }
      if ( blnAdd ) {
       // add uneditable placeholder
        lstWords.AddItem "<group 9999: rest of line>"
      }
     // always mark it as special
      lstWords.ForeColor = &HC0C0C0
      lstWords.FontBold = true
    }
  }
  
#if ( DEBUGMODE <> 1 ) {
  SendMessage lstWords.hWnd, WM_SETREDRAW, 1, 0
#}

  lstWords.Refresh
  
  SelGroup = lngGrpNum
return;
}

private Function ValidateGrpNum() As Boolean

  Dim lngNewGrpNum As Long
  
 // assume OK
  ValidateGrpNum = true
  
  lngNewGrpNum = CLng((txtGrpNum.Text))
  
 // if too big
  if ( lngNewGrpNum > 65535 ) {
    MsgBoxEx "Invalid group number. Must be less than 65536.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Renumber Group Error", WinAGIHelp, "htm\agi\words.htm#16bit"
    ValidateGrpNum = false
    return;
  }
    
 // if group number has changed
  if ( lngNewGrpNum <> CLng(Val(lstGroups.Text)) ) {
   // if the new number is already in use
    if ( WordsEdit.GroupExists(lngNewGrpNum) ) {
     // not a valid new group number
      MsgBox "Group " & txtGrpNum.Text & " is already in use. Choose another number.", vbInformation + vbOKOnly, "Renumber Group Error"
      ValidateGrpNum = false
    }
    
   // if new number is not ok, reset
    if ( !ValidateGrpNum ) {
      txtGrpNum.Text = CStr(Val(lstGroups.Text))
      txtGrpNum.SelStart = 0
      txtGrpNum.SelLength = Len(txtGrpNum.Text)
    } else {
     // ok; make the change
     // renumber this group
      RenumberGroup CStr(Val(lstGroups.Text)), lngNewGrpNum
    }
  }
return;
}

private Function ValidateWord(ByVal CheckWord As String) As Boolean

  Dim i As Long
  Dim strMsg As String
  Dim tmpGroup As Long
  
 // assume OK
  ValidateWord = true
  
  if ( Len(CheckWord) = 0 ) {
   // ok; it will be deleted
    return;
  }
  
 //  is it the same as current word (i.e no change made)
  if ( CheckWord = CurrentWord() ) {
   // nothing to do
    return;
  }
  
 // need to check for invalid characters
  for ( i = 1 To Len(CheckWord)) {
    switch ( Asc(Mid$(CheckWord, i))) {
    case 97 To 122:
     // a-z are ok
      
    case 33, 34, 39, 40, 41, 44, 45, 46, 58, 59, 63, 91, 93, 96, 123, 125:
     //     !'(),-.:;?[]`{}
     // NEVER allowed; these values get removed by the input function
      strMsg = "'" & Mid$(CheckWord, i, 1) & "' is not an allowed character in AGI words."
      ValidateWord = false
      break;
      
    case 32:
     // NOT allowed as first char
      if ( i = 1 ) {
        strMsg = "'" & Mid$(CheckWord, i, 1) & "' is not allowed as first character of an AGI word."
        ValidateWord = false
        break;
      }
    
    case 35 To 38, 42, 43, 47 To 57, 60 To 62, 64, 92, 94, 95, 124, 126, 127:
     // these characters:
     //     #$%&*+/0123456789<=>@\^_|~
     // NOT allowed as first char
      if ( i = 1 ) {
       // UNLESS supporting the Power Pack mod
        if ( !PowerPack ) {
          ValidateWord = false
          strMsg = "'" & Mid$(CheckWord, i, 1) & "' is not allowed as first character of an AGI word."
          break;
        }
      }
      
    default:
     // extended chars not allowed
     // UNLESS supporting the Power Pack mod
      if ( !PowerPack ) {
        ValidateWord = false
        strMsg = "'" & Mid$(CheckWord, i, 1) & "' is not an allowed character in AGI words."
        break;
      }
    }
  }
  
 // if invalid
  if ( !ValidateWord ) {
    MsgBoxEx strMsg, vbOKOnly + vbCritical + vbMsgBoxHelpButton, "Invalid Character in Word", WinAGIHelp, "htm\words.htm#charlimits"
    return;
  }
  
 // does this already word exist?
  tmpGroup = Val(lstGroups.Text)
  
    for (i = 0 To WordsEdit.GroupN(tmpGroup).WordCount - 1) {
      if ( .Word(i) = CheckWord ) {
       // only a concern if it's in same group (i.e. trying to add
       // a duplicate word to same group)
       //  don't need to check if word is in a different group- EditWord
       // handles that case
        MsgBox "The word '" & CheckWord & "' already exists in this group.", vbInformation + vbOKOnly, "Duplicate Word"
        ValidateWord = false
        rtfWord.Selection.Range.StartPos = 0
        rtfWord.Selection.Range.EndPos = Len(CheckWord)
        return;
      }
    }
return;
}

private Function CheckWordFormat(ByRef ThisWord As String) As Boolean

  Dim i As Long
  
  if ( Len(ThisWord) = 0 ) {
   // not valid format
    CheckWordFormat = false
    return;
  }
  
  ThisWord = LCase$(ThisWord)
  
 //  check for invalid characters
  for (i = 1 To Len(ThisWord)) {
    switch ( Asc(Mid$(ThisWord, i))) {
    case 97 To 122:
     // a-z are ok
      
    case 33, 34, 39, 40, 41, 44, 45, 46, 58, 59, 63, 91, 93, 96, 123, 125:
     //     !'(),-.:;?[]`{}
     // NEVER allowed; these values get removed by the input function
      CheckWordFormat = false
      return;
      
    case 32:
     // NOT allowed as first char
      if ( i = 1 ) {
        CheckWordFormat = false
        return;
      }
      
    case 35 To 38, 42, 43, 47 To 57, 60 To 62, 64, 92, 94, 95, 124, 126, 127:
     // these characters:
     //     #$%&*+/0123456789<=>@\^_|~
     // NOT allowed as first char
      if ( i = 1 ) {
       // UNLESS supporting the Power Pack mod
        if ( !PowerPack ) {
          CheckWordFormat = false
          return;
        }
      }
      
    default:
     // extended chars not allowed
     // UNLESS supporting the Power Pack mod
      if ( !PowerPack ) {
        CheckWordFormat = false
        return;
      }
    }
  }
  
 // word is OK
  CheckWordFormat = true
  
return;
}


private void Form_GotFocus()

'*'Debug.Print "words got focus"
}

private void lstGroups_Click()

 // not used in ByWord mode
  if ( Mode = wtByWord ) {
    return;
  }
  
 // on startup, controls are not visible, so can't get focus
  if ( lstGroups.Visible ) {
   // ensure lstGroups has the focus
    lstGroups.SetFocus
  }
  
  if ( SelMode <> smGroup ) {
   // reset mode
    SelMode = smGroup
    
   // if not dragging
    if ( !DraggingWord ) {
     // deselect word
      lstWords.ListIndex = -1
    }
    
    if ( lstGroups.Visible ) {
      lstGroups.SetFocus
    }
  }
  SetEditMenu
  
 // update word list if necessary
  UpdateWordList
return;
}

private void lstGroups_DblClick()

 // edit the selected group's number
 // BUT dont allow group 1 or group 9999 to be edited
  Dim tmpWidth As Single
  
  switch ( Mode) {
  case wtByGroup:
   // if no group selected,
    if ( lstGroups.ListIndex = -1 ) {
      return;
    }
    
    if ( Val(lstGroups.Text) = 0 ) {
     // not allowed
      MsgBoxEx "Group '0' is a reserved group that can't be deleted or renumbered.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Renumber Group Error", WinAGIHelp, "htm\agi\words.htm#reserved"
      return;
    }
    if ( Val(lstGroups.Text) = 1 ) {
     // not allowed
      MsgBoxEx "Group '1' is a reserved group that can't be deleted or renumbered", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Renumber Group Error", WinAGIHelp, "htm\agi\words.htm#reserved"
      return;
    }
    if ( Val(lstGroups.Text) = 9999 ) {
     // not allowed
      MsgBoxEx "Group '9999' is a reserved group that can't be deleted or renumbered.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Renumber Group Error", WinAGIHelp, "htm\agi\words.htm#reserved"
      return;
    }
      
    if ( (lstGroups.ListCount * lngRowHeight + 4 > lstGroups.Height) ) {
      tmpWidth = lstGroups.Width - 22
    } else {
      tmpWidth = lstGroups.Width - 5
    }
    
   // begin edit of group number
     // move textbox
      txtGrpNum.Move lstGroups.Left + 4, (lstGroups.ListIndex - lstGroups.TopIndex) * lngRowHeight + lstGroups.Top + 2, tmpWidth, lngRowHeight
     // copy groupnum to text box
      txtGrpNum.Text = CStr(Val(lstGroups.Text))
     // select entire word
      txtGrpNum.SelStart = 0
      txtGrpNum.SelLength = Len(.Text)
     // show textbox
      txtGrpNum.Visible = true
     // set focus to the textbox
      txtGrpNum.SetFocus
    
   // disable edit menu
    frmMDIMain.mnuEdit.Enabled = false
   //  and toolbar editing buttons
      Me.Toolbar1.Buttons.Item(1).Enabled = false
      Me.Toolbar1.Buttons.Item(2).Enabled = false
      Me.Toolbar1.Buttons.Item(3).Enabled = false
      Me.Toolbar1.Buttons.Item(4).Enabled = false
      Me.Toolbar1.Buttons.Item(6).Enabled = false
      Me.Toolbar1.Buttons.Item(7).Enabled = false
  
  case wtByWord:
   // no action
  }
return;
}

private void lstGroups_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim lngIndex As Long
  
 // if clicking with right button
  if ( Button = vbRightButton ) {
   // select the item that the cursor is over
    lngIndex = (Y / ScreenTWIPSY \ lngRowHeight) + lstGroups.TopIndex
  
   // if something clicked,
    if ( lngIndex <= lstGroups.ListCount - 1 ) {
     // select the item clicked
      lstGroups.ListIndex = lngIndex
      
     // upate wordlist if necessary
      UpdateWordList
     // if a specific word was selected,
     // deselect it
      if ( lstWords.ListIndex >= 0 ) {
        lstWords.ListIndex = -1
      }
      
     // if mode is not group
      if ( SelMode <> smGroup ) {
        SelMode = smGroup
      }
      SetEditMenu
    }
    
   // make sure this form is the active form
    if ( !(frmMDIMain.ActiveMdiChild Is Me) ) {
     // set focus before showing the menu
      Me.SetFocus
    }
   // need doevents so form activation occurs BEFORE popup
   // otherwise, errors will be generated because of menu
   // adjustments that are made in the form_activate event
    SafeDoEvents
   // show edit menu
    PopupMenu frmMDIMain.mnuEdit, , X / ScreenTWIPSX, Y / ScreenTWIPSY + 10
  }
  
  
}

private void lstGroups_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

 // in unlikely case where drag or drop functions get off-kilter, reset them here
  if ( DraggingWord ) {
    DraggingWord = false
  }
}

private void lstGroups_OLEDragDrop(Data As DataObject, Effect As Long, Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim lngIndex As Long
  
 // only allow drop if a word drag is
 // in progress
  if ( !DraggingWord ) {
    return;
  }
  
 // select the item that the cursor is over
  lngIndex = (Y / ScreenTWIPSY \ lngRowHeight) + lstGroups.TopIndex
  
 // if over an item
  if ( lngIndex <= lstGroups.ListCount - 1 ) {
   // not groups 1 or 9999
   // word groups 1 and 9999 are special, and can't be modified
    if ( Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 ) {
      Effect = vbDropEffectNone
      return;
    } else {
'      Effect = vbDropEffectMove
    }
  
   // move this word
    MoveWord lstWords.ListIndex, OldGroupNum, Val(lstGroups.List(lngIndex))
  }
return;
}

private void lstGroups_OLEDragOver(Data As DataObject, Effect As Long, Button As Integer, Shift As Integer, X As Single, Y As Single, State As Integer)

  Dim lngIndex As Long
  
  if ( !DraggingWord ) {
    Effect = vbDropEffectNone
    return;
  }
  
 // select the item that the cursor is over
  lngIndex = (Y / ScreenTWIPSY \ lngRowHeight) + lstGroups.TopIndex
  
 // if over an item
  if ( lngIndex <= lstGroups.ListCount - 1 ) {
   // select it
    lstGroups.ListIndex = lngIndex
  }

 // word groups 1 and 9999 are special, and can't be modified
  if ( Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 ) {
    Effect = vbDropEffectNone
  } else {
  //  Effect = vbDropEffectMove
  }
  
return;
}

private void lstWords_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  switch ( Mode) {
  case wtByGroup:
   // if holding down left button,
    if ( Button = vbLeftButton ) {
     // if mouse has actually moved,
      if ( X <> mX Or Y <> mY ) {
       // no dragging of group 1 or 9999
        if ( Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 ) {
          return;
        }
        
       // if not already dragging,
        if ( !DraggingWord ) {
         // set mode to auto
          lstWords.OLEDragMode = 1
         // begin drag
          lstWords.OLEDrag
         // reset mode to manual to prevent second instance of dragging
          lstWords.OLEDragMode = 0
        }
      }
    }
  case wtByWord:
  }
}

private void lstWords_OLECompleteDrag(Effect As Long)

  switch ( Mode) {
  case wtByGroup:
   // if not droppable,
    if ( Effect = vbDropEffectNone ) {
      lstGroups.ListIndex = OldGroupIndex
    }
  case wtByWord:
  }
  
 // reset dragging flags
  DraggingWord = false
  DroppingWord = false
}

private void lstWords_OLEDragDrop(Data As DataObject, Effect As Long, Button As Integer, Shift As Integer, X As Single, Y As Single)

 // *'Debug.Print "drag-drop", Effect
}

private void lstWords_OLEDragOver(Data As DataObject, Effect As Long, Button As Integer, Shift As Integer, X As Single, Y As Single, State As Integer)

    Effect = vbDropEffectNone
}

private void lstWords_OLEGiveFeedback(Effect As Long, DefaultCursors As Boolean)

 // if not droppable,
  if ( Effect = vbDropEffectNone ) {
 //   lstGroups.ListIndex = OldGroupIndex
  }
  
}

private void lstWords_OLEStartDrag(Data As DataObject, AllowedEffects As Long)

  switch ( Mode) {
  case wtByGroup:
   // set internal drag flag (so this editor knows
   // a word is being dragged)
    DraggingWord = true
   // set global drop flag (so logics (or other text receivers) know
   // when a word is being dropped
    DroppingWord = true
    
   // set allowed effects to move (so word will move from this
   // group into its new group
    AllowedEffects = vbDropEffectMove
    
   // track the original group index and group number
    OldGroupNum = Val(lstGroups.Text)
    OldGroupIndex = lstGroups.ListIndex
    
  case wtByWord:
   // set global drop flag (so logics (or other text receivers) know
   // when a word is being dropped
    DroppingWord = true
  }
}

private void picSplit_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
 // begin split operation
  picSplitIcon.Height = picSplit.Height
  picSplitIcon.Move picSplit.Left, picSplit.Top
  picSplitIcon.Visible = true
  
 // save offset
  SplitOffset = picSplit.Left - X
}


private void picSplit_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim Pos As Single
  
 // if splitting
  if ( picSplitIcon.Visible ) {
    Pos = X + SplitOffset
    
   // limit movement
    if ( Pos < MIN_SPLIT_V ) {
      Pos = MIN_SPLIT_V
    } else if ( Pos > ScaleWidth - MIN_SPLIT_V ) {
      Pos = ScaleWidth - MIN_SPLIT_V
    }
    
   // move splitter
    picSplitIcon.Left = Pos
  }
}



private void picSplit_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim Pos As Single
  
 // if splitting
  if ( picSplitIcon.Visible ) {
   // stop splitting
    picSplitIcon.Visible = false
  
    Pos = X + SplitOffset
    
   // limit movement
    if ( Pos < MIN_SPLIT_V ) {
      Pos = MIN_SPLIT_V
    } else if ( Pos > ScaleWidth - MIN_SPLIT_V ) {
      Pos = ScaleWidth - MIN_SPLIT_V
    }
  }
  
 // redraw!
  UpdatePanels Pos
}


public void MenuClickDescription(ByVal FirstProp As Long)

 // although it is never used, we need to use same form of this method as all other resources
 // otherwise, we'll get an error when the main form tries to call this method
  
  Dim NextUndo As WordsUndo
  Dim strDesc As String
  
  strDesc = WordsEdit.Description
  if ( GetNewResID(rtWords, -1, vbNullString, strDesc, InGame, 2) ) {
   // create new undo object
    NextUndo = new WordsUndo
    NextUndo.Action = ChangeDesc
    NextUndo.Description = strDesc
    
   // add to undo
    AddUndo NextUndo
    
   // change the edit object
    WordsEdit.Description = strDesc
  }
}
public void MenuClickReplace()

 // replace text
 // use menuclickfind in replace mode
  MenuClickFind ffReplaceWord
}


private Function AddGroup(ByVal NewGroupNo As Long, Optional ByVal DontUndo As Boolean = false)
  
  Dim NextUndo As WordsUndo
  Dim i As Integer
  
 // adds a new group, and returns the index of the group in the list
  
 // add the group
  WordsEdit.AddGroup NewGroupNo
  
  switch ( Mode) {
  case wtByGroup: //  by group
   // add it to group list
    for (i = 0 To lstGroups.ListCount - 1) {
     // if the new group is less than or equal to current group number
      if ( NewGroupNo <= Val(lstGroups.List(i)) ) {
       // this is where to insert it
        break;
      }
    }
    lstGroups.AddItem CStr(NewGroupNo) & " - ", i
  case wtByWord:
  }
  
 // if not skipping undo
  if ( !DontUndo ) {
   // create undo object
    NextUndo = new WordsUndo
    NextUndo.Action = AddGroup
    NextUndo.GroupNo = NewGroupNo
    
   // add undo item
    AddUndo NextUndo
    
   // select the group
    lstGroups.ListIndex = i
    
   // send back the index number (which, isn't guaranteed to be NewGroupNo!!!
    AddGroup = i
  }
  
 // update status bar
  UpdateStatusBar
return;
}

private void AddWord(ByVal GroupNo As Long, NewWord As String, Optional ByVal DontUndo As Boolean = false)

  Dim NextUndo As WordsUndo
  Dim i As Integer, strFirst As String
  
 // groups 0, 1, and 9999 never change groupname so no need to capture current groupname
  if ( GroupNo <> 0 And GroupNo <> 1 And GroupNo <> 9999 ) {
   // any other group will always have at least one word
    strFirst = WordsEdit.GroupN(GroupNo).GroupName
  }
  
 // add this word to current word group
  WordsEdit.AddWord NewWord, GroupNo
  
 // if groupname has changed,
  if ( WordsEdit.GroupN(GroupNo).GroupName <> strFirst ) {
    UpdateGroupName GroupNo
  }
  
 // if not skipping undo
  if ( !DontUndo ) {
    switch ( Mode) {
    case wtByGroup:
     // select this words group
      for (i = 0 To lstGroups.ListCount - 1) {
        if ( Val(lstGroups.List(i)) = GroupNo ) {
          lstGroups.ListIndex = i
          break;
        }
      }
      
     // move cursor to new word
      lstWords.Text = CPToUnicode(NewWord, SessionCodePage)
    case wtByWord:
    }

   // create undo object
    NextUndo = new WordsUndo
    NextUndo.Action = AddWord
    NextUndo.GroupNo = WordsEdit(NewWord).Group
    NextUndo.OldGroupNo = NextUndo.GroupNo
    NextUndo.Word = NewWord
    
   // add undo item
    AddUndo NextUndo
  }
  
 // update status bar
  UpdateStatusBar
}


private void DelGroup(OldGrpNum As Long, Optional ByVal DontUndo As Boolean = false)
  
  Dim NextUndo As WordsUndo
  Dim i As Long
  
 // if not skipping undo
  if ( !DontUndo ) {
   // create new undo object
    NextUndo = new WordsUndo
    NextUndo.Action = DelGroup
   // store group number
    NextUndo.GroupNo = OldGrpNum
    
   // copy words in old group to undo object
    NextUndo.Group = new StringList
    for (i = 0 To WordsEdit.GroupN(OldGrpNum).WordCount - 1) {
      NextUndo.Group.Add WordsEdit.GroupN(OldGrpNum).Word(i)
    }
   // add to undo collection
    AddUndo NextUndo
  
   // force logics refresh
    blnRefreshLogics = true
  }
  
  switch ( Mode) {
  case wtByGroup:
   // delete from listbox
    for (i = 0 To WordsEdit.GroupCount - 1) {
      if ( WordsEdit.Group(i).GroupNum = OldGrpNum ) {
        lstGroups.RemoveItem i
       // select next group
        if ( i < lstGroups.ListCount ) {
          lstGroups.ListIndex = i
        } else {
          lstGroups.ListIndex = lstGroups.ListCount - 1
        }
        break;
      }
    }
  
  case wtByWord:
  }
  
 // delete old group AFTER removing it from list
  WordsEdit.RemoveGroup OldGrpNum
  
 // update status bar
  UpdateStatusBar
}

private void DelWord(OldWord As String, Optional ByVal DontUndo As Boolean = false)

  Dim NextUndo As WordsUndo
  Dim lngOldGrpNo As Long
  Dim blnFirst As Boolean
  
 // get group number
  lngOldGrpNo = WordsEdit(OldWord).Group
  blnFirst = (WordsEdit.GroupN(lngOldGrpNo).GroupName = OldWord)
  
 // delete this word
  WordsEdit.RemoveWord OldWord
  
 // if no words left in this group
  if ( WordsEdit.GroupN(lngOldGrpNo).WordCount = 0 ) {
   // RARE, but check for reserved groups
    if ( lngOldGrpNo <> 0 And lngOldGrpNo <> 1 And lngOldGrpNo <> 9999 ) {
     // delete old group
      WordsEdit.RemoveGroup lngOldGrpNo
      
      switch ( Mode) {
      case wtByGroup:
       // delete from listbox
        lstGroups.RemoveItem IndexByGrp(lngOldGrpNo)
      case wtByWord:
      }
    }
  
 // if the first word was deleted
  } else if ( blnFirst ) {
   // reset group listbox entry for this group
    UpdateGroupName lngOldGrpNo
  }
  
 // if not skipping undo
  if ( !DontUndo ) {
   // create new undo object
    NextUndo = new WordsUndo
    NextUndo.Action = DelWord
   // save word and group number
    NextUndo.GroupNo = lngOldGrpNo
    NextUndo.Word = OldWord
   // add to undo collection
    AddUndo NextUndo
  
   //  force logics refresh
    blnRefreshLogics = true
  }
  
 // update status bar
  UpdateStatusBar
}

private void AddUndo(NextUndo As WordsUndo)

 // adds the next undo object
  UndoCol.Add NextUndo
  
 // set undo menu
  frmMDIMain.mnuEUndo.Enabled = true
  frmMDIMain.mnuEUndo.Text = "&Undo " & LoadResString(WORDSUNDOTEXT + NextUndo.Action) & vbTab & "Ctrl+Z"
  
  if ( !IsChanged ) {
    MarkAsChanged
  }
  
 // update status bar
  UpdateStatusBar
}

public void MenuClickClear()
    
 // clear the list
  Clear false, true
}

public void MenuClickFind(Optional ByVal ffValue As FindFormFunction = ffFindWord)

 // set form defaults
    if ( !FindingForm.Visible ) {
      if ( Len(GFindText) > 0 ) {
       // if it has quotes, remove them
        if ( Asc(GFindText) = 34 ) {
          GFindText = Right$(GFindText, Len(GFindText) - 1)
        }
        if ( Right$(GFindText, 1) = QUOTECHAR ) {
          GFindText = Left$(GFindText, Len(GFindText) - 1)
        }
      }
    }
    
   // set find dialog to word mode
    FindingForm.SetForm ffValue, false
    
   // show the form
    FindingForm.Show , frmMDIMain
  
   // always highlight search text
    FindingForm.rtfFindText.Selection.Range.StartPos = 0
    FindingForm.rtfFindText.Selection.Range.EndPos = Len(.rtfFindText.Text)
    FindingForm.rtfFindText.SetFocus
    
   // ensure this form is the search form
    SearchForm = Me
return;
}

private void FindInWords(ByVal FindText As String, ByVal MatchWord As Boolean, ByVal FindDir As FindDirection, Optional ByVal Replacing As Boolean = false, Optional ByVal ReplaceText As String = vbNullString)

  Dim SearchWord As Long, FoundWord As Long
  Dim SearchGrp As Long, FoundGrp As Long
  Dim rtn As VbMsgBoxResult, strFullWord As String
  
  switch ( Mode) {
  case wtByGroup:
   // if editor mode is none (no group/word selected)
    if ( SelMode = smNone ) {
      return;
    }
    
   // if replacing and new text is the same
    if ( Replacing And (StrComp(FindText, ReplaceText, vbTextCompare) = 0) ) {
     // exit
      return;
    }
    
   // blank replace text not allowed for words
    if ( Replacing And LenB(ReplaceText) = 0 ) {
      MsgBox "Blank replacement text is not allowed.", vbInformation + vbOKOnly, "Replace in Word List"
      return;
    }
    
   // if no words in the list
    if ( WordsEdit.WordCount = 0 ) {
      MsgBox "There are no words in this list.", vbOKOnly + vbInformation, "Find in Word List"
      return;
    }
    
   // show wait cursor
    MDIMain.UseWaitCursor = true;
    
   // if replacing and searching up,  start at next word
   // if replacing and searching down start at current word
   // if not repl  and searching up   start at current word
   // if not repl  and searching down start at next word
    
   // set searchwd and searchgrp to current word
    SearchGrp = lstGroups.ListIndex
    SearchWord = lstWords.ListIndex
   // if no word selected, start with first word
    if ( SearchWord = -1 ) {
      SearchWord = 0
    }
    
   // adjust to next word per replace/direction selections
    if ( (Replacing And FindDir = fdUp) Or (!Replacing And FindDir <> fdUp) ) {
     // if at end;
      if ( SearchWord >= lstWords.ListCount - 1 ) {
       // use first word
        SearchWord = 0
       // of next group
        SearchGrp = SearchGrp + 1
        if ( SearchGrp >= lstGroups.ListCount ) {
          SearchGrp = 0
        }
      } else {
        SearchWord = SearchWord + 1
      }
    } else {
     // if already AT beginning of search, the replace function will mistakenly
     // think the find operation is complete and stop
      if ( Replacing And (SearchWord = StartWord And SearchGrp = StartGrp) ) {
       // reset search
        FindingForm.ResetSearch
      }
    }
    
   // main search loop
    do {
     // if direction is up
      if ( FindDir = fdUp ) {
       // iterate backwards until word found or GrpFound=-1
        FoundWord = SearchWord - 1
        FoundGrp = SearchGrp
       // if at top of this group,
       // get the last word of previous group
        if ( FoundWord < 0 ) {
          FoundGrp = FoundGrp - 1
          if ( FoundGrp <> -1 ) {
            FoundWord = WordsEdit.Group(FoundGrp).WordCount - 1
          }
        }
        
        while (FoundGrp != -1) {
         // skip groups with no words
          if ( WordsEdit.Group(FoundGrp).WordCount <> 0 ) {
            if ( MatchWord ) {
              if ( StrComp(WordsEdit.Group(FoundGrp).Word(FoundWord), FindText, vbTextCompare) = 0 ) {
               // found
                break; // exit do
              }
            } else {
              if ( InStr(1, WordsEdit.Group(FoundGrp).Word(FoundWord), FindText, vbTextCompare) <> 0 ) {
               // found
                break; // exit do
              }
            }
          }
         // decrement word
          FoundWord = FoundWord - 1
          if ( FoundWord < 0 ) {
            FoundGrp = FoundGrp - 1
            if ( FoundGrp <> -1 ) {
              FoundWord = WordsEdit.Group(FoundGrp).WordCount - 1
            }
          }
        }
        
       // reset search to last group/last word+1
        SearchGrp = WordsEdit.GroupCount - 1
        SearchWord = WordsEdit.Group(SearchGrp).WordCount //  - 1
      } else {
       // iterate forward until word found or foundgrp=groupcount
        FoundWord = SearchWord
        FoundGrp = SearchGrp
        
        do {
         // skip groups with no words
          if ( WordsEdit.Group(FoundGrp).WordCount <> 0 ) {
            if ( MatchWord ) {
              if ( StrComp(WordsEdit.Group(FoundGrp).Word(FoundWord), UnicodeToCP(FindText, SessionCodePage), vbTextCompare) = 0 ) {
               // found
                break; // exit do
              }
            } else {
              if ( InStr(1, WordsEdit.Group(FoundGrp).Word(FoundWord), UnicodeToCP(FindText, SessionCodePage), vbTextCompare) <> 0 ) {
               // found
                break; // exit do
              }
            }
          }
         // increment word
          FoundWord = FoundWord + 1
          if ( FoundWord >= WordsEdit.Group(FoundGrp).WordCount ) {
            FoundWord = 0
            FoundGrp = FoundGrp + 1
          }
          
        } while (FoundGrp != WordsEdit.GroupCount);
       // reset search
        SearchGrp = 0
        SearchWord = 0
      }
      
     // if found, group will be valid
      if ( FoundGrp >= 0 And FoundGrp < WordsEdit.GroupCount ) {
       // if back at start (grp and word same as start)
        if ( FoundWord = StartWord And FoundGrp = StartGrp ) {
         // rest found position so search will end
          FoundWord = -1
          FoundGrp = -1
        }
        break; // exit do
      }
      
     // if not found, action depends on search mode
      switch ( FindDir) {
      case fdUp:
       // if not reset yet
        if ( !RestartSearch ) {
         // if recursing,
          if ( blnRecurse ) {
           // just say no
            rtn = vbNo
          } else {
            rtn = MsgBox("Beginning of search scope reached. Do you want to continue from the end?", vbQuestion + vbYesNo, "Find in Word List")
          }
          if ( rtn = vbNo ) {
           // reset search
            FindingForm.ResetSearch
            Me.MousePointer = vbDefault
            return;
          }
        } else {
         // entire scope already searched; exit
          break; // exit do
        }
        
      case fdDown:
       // if not reset yet
        if ( !RestartSearch ) {
         // if recursing
          if ( blnRecurse ) {
           // just say no
            rtn = vbNo
          } else {
            rtn = MsgBox("End of search scope reached. Do you want to continue from the beginning?", vbQuestion + vbYesNo, "Find in Word List")
          }
          if ( rtn = vbNo ) {
           // reset search
            FindingForm.ResetSearch
            Me.MousePointer = vbDefault
            return;
          }
        } else {
         // entire scope already searched; exit
          break; // exit do
        }
        
      case fdAll:
        if ( RestartSearch ) {
          break; // exit do
        }
        
      }
      
     // reset search so when we get back to start, search will end
      RestartSearch = true
    
   // loop is exited by finding the searchtext or reaching end of search area
    } while (true);
    
   // if search string found
    if ( FoundGrp >= 0 And FoundGrp < WordsEdit.GroupCount ) {
     // if this is first occurrence
      if ( !FirstFind ) {
       // save this position
        FirstFind = true
        StartWord = FoundWord
        StartGrp = FoundGrp
      }
      
     // highlight Word
      lstGroups.ListIndex = FoundGrp
      lstWords.ListIndex = FoundWord
      
     // if replacing
      if ( Replacing ) {
       // if not replacing entire word
        if ( !MatchWord ) {
         // calculate new findtext and replacetext
          ReplaceText = Replace(WordsEdit.Group(FoundGrp).Word(FoundWord), FindText, ReplaceText)
          strFullWord = WordsEdit.Group(FoundGrp).Word(FoundWord)
        } else {
          strFullWord = FindText
        }
        
       // now try to edit the word
        if ( EditWord(strFullWord, ReplaceText) ) {
         // change undo
          UndoCol(UndoCol.Count).Action = Replace
          frmMDIMain.mnuEUndo.Text = "&Undo Replace" & vbTab & "Ctrl+Z"
         // select the word
          UpdateWordList true
          switch ( Mode) {
          case wtByGroup:
            lstWords.Text = ReplaceText
          }
         // always reset search when replacing, because
         // word index almost always changes
          FindingForm.ResetSearch
          
         // recurse the find method to get the next occurence
          blnRecurse = true
          FindInWords FindText, MatchWord, FindDir, false
          blnRecurse = false
        }
      }
    
   // if search string NOT found
    } else {
     // if not recursing, show a msg
      if ( !blnRecurse ) {
       // if something was previously found
        if ( FirstFind ) {
         // search complete; no new instances found
          MsgBox "The specified region has been searched.", vbInformation, "Find in Word List"
        } else {
         // show not found msg
          MsgBox "Search text not found.", vbInformation, "Find in Word List"
        }
      }
      
     // reset search flags
      FindingForm.ResetSearch
    }
    
   // need to always make sure right form has focus; if finding a word
   // causes the group list to change, VB puts the wordeditor form in focus
   //  but we want focus to match the starting form
    if ( SearchStartDlg ) {
      FindingForm.SetFocus
    } else {
      Me.SetFocus
    }
    
   // reset cursor
    Screen.MousePointer = vbDefault
    
  case wtByWord:
   // edit mode is n/a, and never replacing
    
   // if no words in the list
    if ( WordsEdit.WordCount = 0 ) {
      MsgBox "There are no words in this list.", vbOKOnly + vbInformation, "Find in Word List"
      return;
    }
    
   // show wait cursor
    MDIMain.UseWaitCursor = true;
    
   // if searching up   start at current word
   // if searching down start at next word
    
   // set searchwd to current word
    SearchWord = lstWords.ListIndex
   // if no word selected, start with first word
    if ( SearchWord = -1 ) {
      SearchWord = 0
    }
    
   // adjust to next word per direction selection
    if ( FindDir <> fdUp ) {
     // if at end;
      if ( SearchWord >= lstWords.ListCount - 1 ) {
        if ( FindDir = fdDown ) {
         // done
          SearchWord = WordsEdit.WordCount
        } else {
         // use first word
          SearchWord = 0
        }
      } else {
        SearchWord = SearchWord + 1
      }
    }
    
   // main search loop
    do {
     // if direction is up
      if ( FindDir = fdUp ) {
       // iterate backwards until word found or back to start
        FoundWord = SearchWord - 1
        
        while  ( FoundWord >= 0) {
         // search all words
          if ( MatchWord ) {
            if ( StrComp(WordsEdit(FoundWord).WordText, FindText, vbTextCompare) = 0 ) {
             // found
              break; // exit do
            }
          } else {
            if ( InStr(1, WordsEdit(FoundWord).WordText, FindText, vbTextCompare) <> 0 ) {
             // found
              break; // exit do
            }
          }
         // decrement word
          FoundWord = FoundWord - 1
          if ( FoundWord < 0 ) {
            break; // exit do
          }
        }
        
       // reset search
        SearchWord = WordsEdit.WordCount
      } else {
       // iterate forward until word found or foundword=wordcount
        FoundWord = SearchWord
        
        while (FoundWord != WordsEdit.WordCount) {
          if ( MatchWord ) {
            if ( StrComp(WordsEdit(FoundWord).WordText, UnicodeToCP(FindText, SessionCodePage), vbTextCompare) = 0 ) {
             // found
              break; // exit do
            }
          } else {
            if ( InStr(1, WordsEdit(FoundWord).WordText, UnicodeToCP(FindText, SessionCodePage), vbTextCompare) <> 0 ) {
             // found
              break; // exit do
            }
          }
         // increment word
          FoundWord = FoundWord + 1
        }
       // reset search
        SearchWord = 0
      }
      
     // if found, word will be valid
      if ( FoundWord >= 0 And FoundWord < WordsEdit.WordCount ) {
       // if back at start (word same as start)
        if ( FoundWord = StartWord ) {
         // rest found position to force search to end
          FoundWord = -1
        }
        break; // exit do
      }
      
     // if not found, action depends on search mode
      switch ( FindDir) {
      case fdUp:
       // if not reset yet
        if ( !RestartSearch ) {
         // if recursing,
          if ( blnRecurse ) {
           // just say no
            rtn = vbNo
          } else {
            rtn = MsgBox("Beginning of search scope reached. Do you want to continue from the end?", vbQuestion + vbYesNo, "Find in Word List")
          }
          if ( rtn = vbNo ) {
           // reset search
            FindingForm.ResetSearch
            Me.MousePointer = vbDefault
            return;
          }
        } else {
         // entire scope already searched; exit
          break; // exit do
        }
        
      case fdDown:
       // if not reset yet
        if ( !RestartSearch ) {
         // if recursing
          if ( blnRecurse ) {
           // just say no
            rtn = vbNo
          } else {
            rtn = MsgBox("End of search scope reached. Do you want to continue from the beginning?", vbQuestion + vbYesNo, "Find in Word List")
          }
          if ( rtn = vbNo ) {
           // reset search
            FindingForm.ResetSearch
            Me.MousePointer = vbDefault
            return;
          }
        } else {
         // entire scope already searched; exit
          break; // exit do
        }
        
      case fdAll:
        if ( RestartSearch ) {
          break; // exit do
        }
        
      }
      
     // reset search so when we get back to start, search will end
      RestartSearch = true
    
   // loop is exited by finding the searchtext or reaching end of search area
    } while (true);
    
   // if search string found
    if ( FoundWord >= 0 And FoundWord < WordsEdit.WordCount ) {
     // if this is first occurrence
      if ( !FirstFind ) {
       // save this position
        FirstFind = true
        StartWord = FoundWord
      }
      
     // highlight Word
      lstWords.ListIndex = FoundWord
      
   // if search string NOT found
    } else {
     // if not recursing, show a msg
      if ( !blnRecurse ) {
       // if something was previously found
        if ( FirstFind ) {
         // search complete; no new instances found
          MsgBox "The specified region has been searched.", vbInformation, "Find in Word List"
        } else {
         // show not found msg
          MsgBox "Search text not found.", vbInformation, "Find in Word List"
        }
      }
      
     // reset search flags
      FindingForm.ResetSearch
    }
    
   // need to always make sure right form has focus; if finding a word
   // causes the group list to change, VB puts the wordeditor form in focus
   //  but we want focus to match the starting form
    if ( SearchStartDlg ) {
      FindingForm.SetFocus
    } else {
      Me.SetFocus
    }
    
   // reset cursor
    Screen.MousePointer = vbDefault
  }
return;
}
public void MenuClickFindAgain()

 // if nothing in find form textbox
  if ( LenB(GFindText) <> 0 ) {
    FindInWords GFindText, GMatchWord, GFindDir
  } else {
   // show find form
    MenuClickFind
  }
return;
}

public void MenuClickInsert()
  
 // add a new group
  NewGroup
}

public void MenuClickSelectAll()

   // insert new word into current group
  
    switch ( Val(lstGroups.Text)) {
    case 1:
     //  if no words, add the placeholder, moving it if necessary
      if ( WordsEdit.GroupN(1).WordCount = 0 ) {
       // determine if placeholder already exists
        if ( WordsEdit.WordExists("anyword") ) {
         // move it
          MoveWord "anyword", WordsEdit("anyword").Group, 1
        } else {
         // add it
          AddWord 1, "anyword", false
        }
      }
      
    case 9999:
     //  if no words, add the placeholder, moving it if necessary
      if ( WordsEdit.GroupN(9999).WordCount = 0 ) {
       // determine if placeholder already exists
        if ( WordsEdit.WordExists("rol") ) {
         // move it
          MoveWord "rol", WordsEdit("rol").Group, 9999
        } else {
         // add it
          AddWord 9999, "rol", false
        }
      }
    default:
     // add a new word
      NewWord
    }
    
}

public void NewWord(Optional ByVal DontUndo As Boolean = false)

  Dim strNewWord As String
  Dim lngNewWordNum As Long
  Dim i As Long

  if ( lstGroups.ListIndex < 0 ) {
    return;
  }
  
 // create an unambiguous new word
  lngNewWordNum = 0
  do {
   // increment new word number
    lngNewWordNum = lngNewWordNum + 1
    strNewWord = "new word " & CStr(lngNewWordNum)
   // attempt to access this word
    i = WordsEdit(strNewWord).Group
   // if word doesn't exist,
    if ( Err.Number <> 0 ) {
     // word doesn't exist
      break; // exit do
    }
  } while (lngNewWordNum < 1000);
  
 // if a new word not found(not very likely)
  if ( lngNewWordNum = 1000 ) {
    if ( !DontUndo ) {
      MsgBox "You already have 1,000 new word entries. Try changing a few of those before adding more words.", vbInformation + vbOKOnly, "Too Many Default Words"
    }
    return;
  }
 // add the word
  AddWord Val(lstGroups.Text), strNewWord, DontUndo
  
  switch ( Mode) {
  case wtByGroup:
   // update wordlist to show the new word
    UpdateWordList true
   // select the new word
    lstWords.Text = strNewWord
  case wtByWord:
  }
 // set up for editing
  AddNewWord = true
  
 // now edit the word
  MenuClickECustom1
return;
}

public void MoveWord(WordIndex As Variant, ByVal OldGrpNum As Long, NewGrpNum As Long, Optional ByVal DontUndo As Boolean = false)
 // moves a word from one group to another
  
  Dim NextUndo As WordsUndo, WordText As String
  Dim i As Integer, blnFirst As Boolean
  
 // stop dragging word
  DraggingWord = false
  
 // if groups are the same
  if ( OldGrpNum = NewGrpNum ) {
    return;
  }
  
 // is the word the first word in the old group?
  switch ( VarType(WordIndex)) {
  case vbByte, vbInteger, vbLong:
    blnFirst = (WordIndex = 0)
    WordText = WordsEdit.GroupN(OldGrpNum).Word(WordIndex)
  case vbString:
    blnFirst = (WordsEdit.GroupN(OldGrpNum).GroupName = WordIndex)
    WordText = WordIndex
  default:
    return;
  }
  
 // delete word from old group
  WordsEdit.RemoveWord WordText
 // if no words left in old group
  if ( WordsEdit.GroupN(OldGrpNum).WordCount = 0 ) {
   // RARE- ok for group 1/9999 to have no words
    if ( OldGrpNum <> 1 And OldGrpNum <> 9999 ) {
     // need to remove group too
     // delete old group
      WordsEdit.RemoveGroup OldGrpNum
      
      switch ( Mode) {
      case wtByGroup:
       // delete from listbox
        lstGroups.RemoveItem IndexByGrp(OldGrpNum)
      case wtByWord:
      }
    }
  } else {
   // if the moved word was the first word in the old group
    if ( blnFirst ) {
      UpdateGroupName OldGrpNum
    }
  }
  
 // if skipping undo
  if ( DontUndo ) {
   // need to add the 'new' group if it doesn't exist
    if ( !WordsEdit.GroupExists(NewGrpNum) ) {
      AddGroup NewGrpNum, true
    }
  }
  
 // add to new group
  WordsEdit.AddWord WordText, NewGrpNum
  
 // if not skipping undo
  if ( !DontUndo ) {
   // create new undo object
    NextUndo = new WordsUndo
    NextUndo.Action = MoveWord
   // store oldgroup, newgroup, and word
    NextUndo.GroupNo = NewGrpNum
    NextUndo.OldGroupNo = OldGrpNum
    NextUndo.Word = WordText
   // add to undo
    AddUndo NextUndo
    
   // force logic refresh
    blnRefreshLogics = true
  }
  
 // if the moved word is now the first word in the new group
  if ( WordsEdit.GroupN(NewGrpNum).GroupName = WordText ) {
    UpdateGroupName NewGrpNum
  }
  
  switch ( Mode) {
  case wtByGroup:
   // select the new group
    i = GroupIndex(NewGrpNum)
    if ( Val(lstGroups.List(i)) = NewGrpNum ) {
      if ( lstGroups.ListIndex = i ) {
       // force update
        UpdateWordList true
      } else {
       // select it
        lstGroups.ListIndex = i
      }
    }
   // then select the word
    lstWords.Text = CPToUnicode(WordText, SessionCodePage)
  case wtByWord:
  }
}

public Function LoadWords(ByVal WordFile As String) As Boolean
  
 // opens a word list file and loads it into the editor
 // returns true if successful, false if any errors encountered
  Dim i As Long
  Dim tmpWords As AGIWordList
  Dim rtn As VbMsgBoxResult
  
 //  show wait cursor; this may take awhile
  MDIMain.UseWaitCursor = true;
  
 // use a temp words word to get items
  tmpWords = new AGIWordList
 // trap errors
  tmpWords.Load WordFile
  if ( Err.Number <> 0 ) {
   // restore cursor
    Screen.MousePointer = vbDefault
  
   // if due to missing file:
    if ( Err.Number = vbObjectError + 524 ) {
     // if in a game, give user opportunity to create a new list
      if ( InGame ) {
        rtn = MsgBox("The WORDS.TOK file is missing. Would you like to create a new file?", vbQuestion + vbYesNo, "Missing WORDS.TOK File")
        if ( rtn = vbYes ) {
         // try creating a new file
          tmpWords.NewWords
          tmpWords.Save WordFile
        } else {
         // give up
          return;
        }
      } else {
       // not in game; shouldn't normally get this error, because the file is chosen before this
       // function is called, but just in case
        MsgBox "The file: " & vbCrLf & vbCrLf & WordFile & vbCrLf & vbCrLf & " is missing.", vbCritical + vbOKOnly, "Unable to Open WORDS.TOK File"
       // return false
        return;
      }
    } else {
     // if in a game, give user opportunity to create a new list
      if ( InGame ) {
        rtn = MsgBox("The WORDS.TOK file corrupted and can't be loaded. Would you like to create a new file in its place? (Original file will be renamed)", vbQuestion + vbYesNo, "Missing WORDS.TOK File")
        if ( rtn = vbYes ) {
         // kill any existing backup files
          if ( FileExists(JustPath(WordFile) & "WORDS.TOK.old") ) {
            Kill JustPath(WordFile) & "WORDS.TOK.old"
          }
          Name WordFile As JustPath(WordFile) & "WORDS.TOK.old"
         // now try creating a new file
          tmpWords.NewWords
          tmpWords.Save WordFile
        } else {
         // abort the load
          return;
        }
      } else {
       // not in a game; inform user of the error; they will have to deal with the bad file on their own
        ErrMsgBox "Error while trying to load word file: ", "Unable to edit this file.", "Load Word Error"
       // return false
        return;
      }
    }
  }

 // file is clean
  IsChanged = false

 // set caption
  Text = "Words Editor - "
  if ( InGame ) {
    Text = Text & GameID
  } else {
    Text = Text & CompactPath(WordFile, 75)
  }

 // copy to local words word
  WordsEdit.Clear
  WordsEdit = tmpWords
 // add description
  WordsEdit.Description = tmpWords.Description
  
  switch ( Mode) {
  case wtByGroup:
   // add word groups to listbox
    lstGroups.Clear
    for (i = 0 To WordsEdit.GroupCount - 1) {
     // for groups 0, 1, 9999 use special annotation
      switch ( WordsEdit.Group(i).GroupNum) {
      case 0: // null words
        lstGroups.AddItem "0 - <null words>"
      case 1: // anyword
        lstGroups.AddItem "1 - <any word>"
      case 9999: // rest of line
        lstGroups.AddItem "9999 - <rest of line>"
      default:
       // add group normally
        lstGroups.AddItem CStr(WordsEdit.Group(i).GroupNum) & " - " & UCase$(CPToUnicode(WordsEdit.Group(i).GroupName, SessionCodePage))
      }
    }
  
   // VERY RARE, but make sure groups 0, 1, 9999 actually exist
    if ( !WordsEdit.GroupExists(0) ) {
     // add a placeholder line
      lstGroups.AddItem "0 - <null words>", 0
    }
    if ( !WordsEdit.GroupExists(1) ) {
     // add a placeholder line
      lstGroups.AddItem "1 - <anyword>", 1
    }
    if ( !WordsEdit.GroupExists(9999) ) {
     // add a placeholder line
      lstGroups.AddItem "9999 - <rest of line>"
    }
    
   // select first group
    lstGroups.ListIndex = 0
    UpdateWordList
  
  case wtByWord:
   // add all words to lstWords
    for (i = 0 To WordsEdit.WordCount - 1) {
      lstWords.AddItem WordsEdit(i).WordText
    }
   // only one item in lstGroup
    lstGroups.AddItem ""
    
   // select first word
    lstGroups.ListIndex = 0
    lstWords.ListIndex = 0
  }
  
  LoadWords = true
  
 // restore cursor
  Screen.MousePointer = vbDefault
return;
}
            public void MenuClickExport()

              if ( ExportWords(WordsEdit, InGame) ) {
               // if this is NOT the in game file,
                if ( !InGame ) {
                 // reset changed flag
                  IsChanged = false
                 // update caption
                  Text = "Words Editor - " & CompactPath(WordsEdit.ResFile, 75)
                 // disable save menu/button
                  frmMDIMain.mnuRSave.Enabled = false
                  frmMDIMain.Toolbar1.Buttons("save").Enabled = false
                }
              }
            }

            public void MenuClickCustom1()
             // merge Words.tok from file

              Dim strMsg As String, OldGroup As Long
              Dim MergeList As AGIWordList
              Dim MergeReplace As VbMsgBoxResult, RepeatAnswer As Boolean
              Dim GroupNum As Long, MergeWord As String
              Dim i As Long, j As Long
              Dim WordCount As Long
              Dim lngCount As Long
              Dim lngRepl As Long

             // set common dialog properties
                MainDialog.DialogTitle = "Merge Vocabulary Words File"
                MainDialog.Filter = "WinAGI Words file (*.agw)|*.agw|WORDS.TOK file|WORDS.TOK|All files (*.*)|*.*"
                MainDialog.FilterIndex = 2
                MainDialog.DefaultExt = vbNullString
                MainDialog.FileName = vbNullString
                MainDialog.InitDir = DefaultResDir

               // get new file
                MainDialog.ShowOpen
                DefaultResDir = JustPath(MainDialog.FileName)

             // check for cancel
              if ( Err.Number = cdlCancel ) {
                return;
              }

             // show progress form
              Load frmProgress
                frmProgress.Text = "Merging from File"
                frmProgress.lblProgress.Text = "Merging..."
                frmProgress.pgbStatus.Max = MergeList.WordCount
                frmProgress.pgbStatus.Value = 0
                frmProgress.Show vbModeless, frmMDIMain
                frmProgress.Refresh

             // show wait cursor
              MDIMain.UseWaitCursor = true;

             // load the merge list
              MergeList = new AGIWordList
              MergeList.Load MainDialog.FileName

             // if an error,
              if ( Err.Number <> 0 ) {
                Unload frmProgress
                ErrMsgBox "An error occurred while trying to load " & JustFileName(MainDialog.FileName) & ": ", "Unable to merge the file.", "Import Word List Error"
                MergeList.Clear
                MergeList = Nothing
               // reset cursor
                Screen.MousePointer = vbDefault
                return;
              }

              WordCount = MergeList.WordCount - 1

             // step through all words
              for (i = 0 To WordCount) {
               // get word and group
                GroupNum = MergeList(i).Group
                MergeWord = MergeList(i).WordText

               // determine if word exists in current file
                OldGroup = WordsEdit(MergeWord).Group
               // if no error
                if ( Err.Number = 0 ) {
                 // word exists;
                 // if not the same group
                  if ( OldGroup <> GroupNum ) {
                   // if not repeating answer
                    if ( !RepeatAnswer ) {
                     // get decision from user
                      MergeReplace = MsgBoxEx(QUOTECHAR & MergeWord & """ already exists in Group " & CStr(WordsEdit(MergeWord).Group) & "." & vbNewLine & "Do you want to move it to group " & CStr(GroupNum) & "?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Replace Word?", WinAGIHelp, "htm\winagi\Words_Editor.htm#merge", "Repeat this answer for all duplicate words", RepeatAnswer)
                    }

                   // if replacing,
                    if ( MergeReplace = vbYes ) {
                     // remove it from previous group
                      WordsEdit.RemoveWord MergeWord
                     // add it to new group
                      WordsEdit.AddWord MergeWord, GroupNum
                      lngRepl = lngRepl + 1
                    }
                  }
                } else {
                 // not in current list- ok to add
                  WordsEdit.AddWord MergeWord, GroupNum
                  lngCount = lngCount + 1
                }

               // update progressform
                frmProgress.pgbStatus.Value = frmProgress.pgbStatus.Value + 1
                frmProgress.Refresh
              }

             // clear and dereference mergelist
              MergeList.Clear
              MergeList = Nothing

             // refresh form
              switch ( Mode) {
              case wtByGroup:
                RebuildGroupList
              case wtByWord:
              }

             // reset cursor
              Screen.MousePointer = vbDefault

              MsgBox "Added " & CStr(lngCount) & " words. Replaced " & CStr(lngRepl) & " words."

             // unload progress form
              Unload frmProgress
            return;
            }

            public void MenuClickCustom2()

              Dim i As Long, blnDontAsk As Boolean, rtn As VbMsgBoxResult
              Dim RepeatAnswer As Long, tmpLogic As AGILogic
              Dim GroupUsed() As Boolean, UnusedCount As Long
              Dim strCount As String, stlOutput As StringList
              Dim tmpLoad As Boolean, lngPos As Long
              Dim tmpLen As Long, strToken As String

             //  word usage check- find words not used in a game

             // only for ingame word lists
              if ( !InGame ) {
                return;
              }

             // check for open logics- save or cancel if changed
              for (i = 1 To LogicEditors.Count) {
                if ( LogicEditors(i).FormMode = fmLogic ) {
                  if ( LogicEditors(i).rtfLogic.IsChanged ) {
                    switch ( RepeatAnswer) {
                    case 0: // ask user for input
                      LogicEditors(i).SetFocus
                     // get user's response
                      rtn = MsgBoxEx("Do you want to save this logic before checking word usage?", _
                            vbQuestion + vbYesNoCancel, "Update " & ResourceName(LogicEditors(i).EditLogic, true, true) & _
                            "?", , , "Repeat this answer for all other open logics.", blnDontAsk)
                      if ( blnDontAsk ) {
                        if ( rtn = vbYes ) {
                          RepeatAnswer = 2
                        } else if ( rtn = vbNo ) {
                          RepeatAnswer = 1
                        }
                      }

                    case 1: // no
                      rtn = vbNo

                    case 2: // yes
                      rtn = vbYes
                    }

                    switch ( rtn) {
                    case vbCancel:
                      return;
                    case vbYes:
                     // save it
                      LogicEditors(i).MenuClickSave
                    }
                  }
                }
              }

             //  show wait cursor; this may take awhile
              MDIMain.UseWaitCursor = true;

             // build array to store results
              ReDim GroupUsed(WordsEdit.GroupCount - 1)

             // step through all logics
              foreach (tmpLogic In Logics
                tmpLoad = tmpLogic.Loaded
                if ( !tmpLoad ) {
                  tmpLogic.Load
                }
               // find all said commands
               // mark words/groups as in use
                lngPos = 0
                do {
                  lngPos = FindNextToken(tmpLogic.SourceText, lngPos, "said", true)
                  if ( lngPos = 0 ) {
                    break; // exit do
                  }
                 // skip to end of 'said'
                  lngPos = lngPos + 3
                  strToken = ""
                  lngPos = FindNextToken(tmpLogic.SourceText, lngPos, strToken, true)
                  if ( lngPos = 0 ) {
                    break; // exit do
                  }
                  if ( strToken = "(" ) {
                    do {
                     // get word arguments; skip commas
                      strToken = ""
                      lngPos = FindNextToken(tmpLogic.SourceText, lngPos, strToken, true)
                      if ( lngPos = 0 ) {
                        break; // exit do
                      }
                      lngPos = lngPos + Len(strToken) - 1
                     // check for valid word ( at least 3 chars)? or numeric?
                      if ( Len(strToken) < 3 And !IsNumeric(strToken) ) {
                        break; // exit do
                      }
                      if ( IsNumeric(strToken) ) {
                       // count this group
                        if ( WordsEdit.GroupExists(CLng(strToken)) ) {
                          GroupUsed(GroupIndex(CLng(strToken))) = true
                        }
                      } else {
                       // must start/end with quotes
                        if ( Asc(strToken) <> 34 Or Asc(Right$(strToken, 1)) <> 34 ) {
                          break; // exit do
                        }
                        strToken = LCase$(Mid$(strToken, 2, Len(strToken) - 2))
                        if ( WordsEdit.WordExists(LCase$(strToken)) ) {
                         // count this group
                          GroupUsed(GroupIndex(WordsEdit(strToken).Group)) = true
                        }
                      }

                     // next cmd can be ',' or ')' (or invalid said)
                      strToken = ""
                      lngPos = FindNextToken(tmpLogic.SourceText, lngPos, strToken, true)
                      if ( lngPos = 0 Or strToken <> "," ) {
                        break; // exit do
                      }
                    } while (true);
                  }
                } while (true);
                if ( !tmpLoad ) {
                  tmpLogic.Unload
                }
                SafeDoEvents
              }

              stlOutput = new StringList

             // restore cursor
              Screen.MousePointer = vbDefault

             //  go through all groups, make list of any that are unused
              for (i = 0 To UBound(GroupUsed) - 1) {
                if ( !GroupUsed(i) ) {
                 // skip 0, 1, 9999
                  switch ( WordsEdit.Group(i).GroupNum) {
                  case 0, 1, 9999:
                   // skip these
                  default:
                   // add line
                    stlOutput.Add WordsEdit.Group(i).GroupNum & vbTab & WordsEdit.Group(i).GroupName
                    UnusedCount = UnusedCount + 1
                  }
                }
              }
             // add blank line to add trailing crlf
              stlOutput.Add ""

             // present results
              if ( UnusedCount = 0 ) {
                MsgBox "All word groups are used in this game.", vbInformation + vbOKOnly, "No Unused Words"
              } else {
               // copy output to clipboard
                Clipboard.Clear
                Clipboard.SetText stlOutput.Text, vbCFText

                if ( UnusedCount = 1 ) {
                  strCount = "is one unused word group"
                } else {
                  strCount = "are " & CStr(UnusedCount) & " unused word groups"
                }
                MsgBox "There " & strCount & " in this game. " & vbNewLine & vbNewLine & _
                       "The full list has been copied to the clipboard.", _
                       vbInformation + vbOKOnly, "Unused Word Groups Check Results"
              }
            return;
            }

            private Function GroupIndex(ByVal GroupNumber As Long) As Long

             // returns index of a group by its group number

              Dim i As Long

              for (i = 0 To WordsEdit.GroupCount - 1) {
                if ( WordsEdit.Group(i).GroupNum = GroupNumber ) {
                 GroupIndex = i
                 return;
                }
              }
            }

            public void MenuClickCustom3()

            }

            public void MenuClickCopy()

              Dim i As Long

               // put selected word or group on clipboard
                switch ( SelMode) {
                case smWord:
                  WordsClipboard.Action = DelWord
                 // don't need to validate case or format if copying
                 // from an existing word
                  WordsClipboard.Word = CurrentWord()

                 // put the word on the real clipboard, too
                 // (need to convert it to correct codepage so it can
                 // be compatible with RichEdAGI control)
                  Clipboard.Clear
                  Clipboard.SetText QUOTECHAR & UnicodeToCP(lstWords.Text, CodePage) & QUOTECHAR, vbCFText

                case smGroup:
                  WordsClipboard.Action = DelGroup
                  WordsClipboard.GroupNo = Val(lstGroups.Text)

                  .Group.Clear
                  for (i = 0 To lstWords.ListCount - 1) {
                    WordsClipboard.Group.Add lstWords.List(i)
                  }
                 // put groupname on real clipboard too
                  Clipboard.Clear
                  Clipboard.SetText QUOTECHAR & WordsClipboard.Group(0) & QUOTECHAR, vbCFText

                }
             // enable pasting
              frmMDIMain.mnuEPaste.Enabled = true
              Me.Toolbar1.Buttons(3).Enabled = true

            }
            public void MenuClickCut()

             // if nothing selected
              if ( SelMode = smNone ) {
                return;
              }

             // copy,
              MenuClickCopy

             // then delete
              MenuClickDelete

             // change last undo item to indicate cut
              switch ( UndoCol(UndoCol.Count).Action) {
              case DelWord:
                UndoCol(UndoCol.Count).Action = CutWord
              case DelGroup:
                UndoCol(UndoCol.Count).Action = CutGroup
              }
            }
            public void MenuClickPaste()

              Dim lngOldGroupNo As Long, strWord As String
              Dim blnMove As Boolean
              Dim NextUndo As WordsUndo
              Dim i As Long, strMsg As String
              Dim lngNewGrpNo As Long

             // only allow pasting if the custom clipboard is set OR if a valid word is on the clipboard

             // if a group
              if ( WordsClipboard.Action = DelGroup ) {
               // VERY RARE- but check for max number of groups
                if ( lstGroups.ListCount = &H7FFF ) {
                 // ignore
                  return;
                }

               // clipboard contains a single group
                lngNewGrpNo = NextGrpNum()

               // setup undo
                NextUndo = new WordsUndo
                NextUndo.Action = PasteGroup

               // check words; see which ones can be added
                for (i = 0 To WordsClipboard.Group.Count - 1) {
                  if ( WordsEdit.WordExists(WordsClipboard.Group(i)) ) {
                   // word is in use
                    lngOldGroupNo = WordsEdit(WordsClipboard.Group(i)).Group
                    strMsg = strMsg & vbNewLine & vbTab & WordsClipboard.Group(i) & " (in group " & CStr(lngOldGroupNo) & ")"
                  } else {
                   // add it to undo object
                    NextUndo.Group.Add WordsClipboard.Group(i)
                  }
                }

               // if nothing added
                if ( NextUndo.Group.Count = 0 ) {
                 // nothing to add
                  MsgBox "No words on the clipboard could be pasted; they all are already in this word list.", vbInformation + vbOKOnly, "Nothing to Paste"
                  return;
                }

               // add group
                AddGroup lngNewGrpNo, true

               // add the words
                for (i = 0 To NextUndo.Group.Count - 1) {
                 // add it (without undo)
                  AddWord lngNewGrpNo, NextUndo.Group(i), true
                }

               // reset group name
                UpdateGroupName lngNewGrpNo

                switch ( Mode) {
                case wtByGroup:
                 // select the group
                  lstGroups.ListIndex = lngNewGrpNo
                case wtByWord:
                }

               // finish the undo
                NextUndo.GroupNo = lngNewGrpNo
               // the words added aren't needed
                NextUndo.Group.Clear
                AddUndo NextUndo

               // if there is a msg
                if ( LenB(strMsg) <> 0 ) {
                  MsgBox "The following words were not added because they already exist in another group: " & strMsg, vbInformation + vbOKOnly, "Paste Group from Clipboard"
                }

              } else if ( WordsClipboard.Action = DelWord Or Clipboard.GetFormat(vbCFText) ) {
               // clipboard contains a single word
               // if real clipboard word has changed, change WordsClipboard to match...
                if ( Clipboard.GetFormat(vbCFText) ) {
                  strWord = Clipboard.GetText(vbCFText)
                  strWord = Replace(strWord, vbCr, "")
                  strWord = Replace(strWord, vbLf, "")
                  strWord = LCase(strWord)

                  if ( strWord <> Chr$(34) & WordsClipboard.Word & Chr$(34) ) {
                   // check formatting
                    if ( CheckWordFormat(strWord) ) {
                     // this word is acceptable; put it on custom clipboard
                      WordsClipboard.Word = strWord
                      WordsClipboard.Action = DelWord
                     // and update real clipboard?
                      Clipboard.SetText Chr$(34) & strWord & Chr$(34)
                    } else {
                     // not a valid word on clipboard
                      return;
                    }
                  }
                } else {
                 // nothing on main clipboard - is there anything on custom clipboard?
                  if ( Len(WordsClipboard.Word) = 0 ) {
                    return;
                  }
                }

               // validate word
                if ( WordsEdit.WordExists(WordsClipboard.Word) ) {
                  lngOldGroupNo = WordsEdit(WordsClipboard.Word).Group
                 // if already in this group
                  if ( lngOldGroupNo = Val(lstGroups.Text) ) {
                    MsgBox ChrW$(39) & WordsClipboard.Word & "' already exists in this group."
                    return;
                  }

                 // word is in another group- ask if word should be moved
                  if ( MsgBox(ChrW$(39) & WordsClipboard.Word & "' already exists (in group " & CStr(lngOldGroupNo) & "). Do you want to move it to this group?", vbYesNo + vbQuestion) = vbNo ) {
                    return;
                  }
                 // delete word from other group
                  DelWord WordsClipboard.Word, true
                  blnMove = true
                } else {
                 // old group same as new (so Undo knows a word wasn't moved)
                  lngOldGroupNo = Val(lstGroups.Text)
                }

               // add word to this group
                AddWord Val(lstGroups.Text), WordsClipboard.Word, true

               // add undo
                NextUndo = new WordsUndo
                NextUndo.Action = PasteWord
                NextUndo.GroupNo = Val(lstGroups.Text)
                NextUndo.OldGroupNo = lngOldGroupNo
                NextUndo.Word = WordsClipboard.Word
                AddUndo NextUndo

               // update word list to show new word
                switch ( Mode) {
                case wtByGroup:
                  UpdateWordList true
                case wtByWord:
                }
              }

            return;
            }

            public void MenuClickUndo()

              Dim NextUndo As WordsUndo
              Dim i As Long, j As Long
              Dim strTemp As String, strWords() As String
              Dim lngGroup As Long

             // if there are no undo actions
              if ( UndoCol.Count = 0 ) {
               // just exit
                return;
              }

             // get next undo object
              NextUndo = UndoCol(UndoCol.Count)
             // remove undo object
              UndoCol.Remove UndoCol.Count
             // reset undo menu
              frmMDIMain.mnuEUndo.Enabled = (UndoCol.Count > 0)
              if ( frmMDIMain.mnuEUndo.Enabled ) {
                frmMDIMain.mnuEUndo.Text = "&Undo " & LoadResString(WORDSUNDOTEXT + UndoCol(UndoCol.Count).Action) & vbTab & "Ctrl+Z"
              } else {
                frmMDIMain.mnuEUndo.Text = "&Undo" & vbTab & "Ctrl+Z"
              }

             // undo the action
              switch ( NextUndo.Action) {
              case AddGroup, PasteGroup:
               // delete group
                DelGroup NextUndo.GroupNo, true

              case DelGroup, CutGroup:
               // add group
                AddGroup NextUndo.GroupNo, true
               // add words back into group
                for (i = 0 To NextUndo.Group.Count - 1) {
                  WordsEdit.AddWord NextUndo.Group(i), NextUndo.GroupNo
                }
                UpdateGroupName NextUndo.GroupNo

                switch ( Mode) {
                case wtByGroup:
                 // select the group added
                  for (i = 0 To lstGroups.ListCount - 1) {
                    if ( Val(lstGroups.List(i)) = NextUndo.GroupNo ) {
                      lstGroups.ListIndex = i
                      lstGroups_Click
                      break;
                    }
                  }
                case wtByWord:
                }

              case AddWord, PasteWord:
               // delete the added word
                DelWord NextUndo.Word, true

               // if the add killed a word in another group
                if ( NextUndo.GroupNo <> NextUndo.OldGroupNo ) {
                 // if group dosn't exist(meaning this word was last word in the group)
                  if ( !WordsEdit.GroupExists(NextUndo.OldGroupNo) ) {
                   // add the group first
                    AddGroup NextUndo.OldGroupNo, true
                  }

                 // need to add back the deleted word to its old group
                  AddWord NextUndo.OldGroupNo, NextUndo.Word, true
                }

                switch ( Mode) {
                case wtByGroup:
                 // select group that deleted word was in
                  for (i = 0 To lstGroups.ListCount - 1) {
                    if ( Val(lstGroups.List(i)) = NextUndo.GroupNo ) {
                     // if already selected
                      if ( lstGroups.ListIndex = i ) {
                        UpdateWordList true
                      } else {
                        lstGroups.ListIndex = i
                      }
                      break;
                    }
                  }
                case wtByWord:
                }

              case DelWord, CutWord:
               // check for case of deleting or cutting last word in a group
                if ( !WordsEdit.GroupExists(NextUndo.GroupNo) ) {
                 // need to add this group
                 // entry
                  AddGroup NextUndo.GroupNo, true
                }

               // now we know for sure group does exist- add the word
                AddWord NextUndo.GroupNo, NextUndo.Word, true

                switch ( Mode) {
                case wtByGroup:
                 // select the restored word
                  for (i = 0 To lstGroups.ListCount - 1) {
                    if ( Val(lstGroups.List(i)) = NextUndo.GroupNo ) {
                      if ( lstGroups.ListIndex = i ) {
                        UpdateWordList true
                      } else {
                        lstGroups.ListIndex = i
                      }
                      break;
                    }
                  }
                  lstWords.Text = CPToUnicode(NextUndo.Word, SessionCodePage)
                case wtByWord:
                }

              case MoveWord: // store old and new group numbers, and word index
               // move word back to old position
                MoveWord NextUndo.Word, NextUndo.GroupNo, NextUndo.OldGroupNo, true
                switch ( Mode) {
                case wtByGroup:
                 // reselect group word is moved TO
                  lstGroups.ListIndex = WordsEdit.GroupN(NextUndo.OldGroupNo).GroupNum
                  UpdateWordList true
                 // select the moved word
                  lstWords.Text = CPToUnicode(NextUndo.Word, SessionCodePage)
                }

              case Renumber: // store old group number AND new group number
               // change number back
                RenumberGroup NextUndo.GroupNo, NextUndo.OldGroupNo, true

              case ChangeWord, Replace: // store old word and new word and group
               // change new word back to old word
                EditWord NextUndo.Word, NextUndo.OldWord, true
               // if the change killed a word in another group
                if ( NextUndo.GroupNo <> NextUndo.OldGroupNo ) {
                 // if group dosn't exist(meaning this word was last word in the group)
                  if ( !WordsEdit.GroupExists(NextUndo.OldGroupNo) ) {
                   // add the group first
                    AddGroup NextUndo.OldGroupNo, true
                  }

                 // need to add back the deleted word to its old group
                  AddWord NextUndo.OldGroupNo, NextUndo.Word, true
                }

                switch ( Mode) {
                case wtByGroup:
                 // reselect the modified word and its group
                  if ( Val(lstGroups.Text) <> NextUndo.GroupNo ) {
                    for (i = 0 To lstGroups.ListCount - 1) {
                      if ( Val(lstGroups.List(i)) = NextUndo.GroupNo ) {
                        lstGroups.Selected(i) = true
                        lstGroups.ListIndex = i
                      } else {
                        lstGroups.Selected(i) = false
                      }
                    }
                  } else {
                    UpdateWordList true
                  }
                  for (i = 0 To lstWords.ListCount - 1) {
                    if ( WordsEdit.GroupN(NextUndo.GroupNo).Word(i) = NextUndo.OldWord ) {
                      lstWords.Selected(i) = true
                      lstWords.ListIndex = i
                      break;
                    }
                  }
                case wtByWord:
                }

              case ChangeDesc:
               // resore old description
                WordsEdit.Description = NextUndo.Description
               // if in a game
                if ( InGame ) {
                 // restore the ingame resource as well
                  VocabularyWords.Description = NextUndo.Description
                  VocabularyWords.Save
                 // update prop window
                  RefreshTree rtWords, -1, umProperty
                }

              case ReplaceAll:
               // description field has MatchWord flag
                if ( CBool(NextUndo.Description) ) {
                 // restore a single word

                 // remove existing word
                  WordsEdit.RemoveWord NextUndo.Word

                 // restore previous word
                  WordsEdit.AddWord NextUndo.OldWord, NextUndo.GroupNo
                 // ensure group name is correct
                  UpdateGroupName NextUndo.GroupNo

                 // if the existing word was in a different group
                  if ( NextUndo.OldGroupNo <> -1 ) {
                   // add word back to its old group
                    WordsEdit.AddWord NextUndo.Word, NextUndo.OldGroupNo
                   // ensure groupname is correct
                    UpdateGroupName NextUndo.OldGroupNo
                  }

                  switch ( Mode) {
                  case wtByGroup:
                   // if either group is currently selected,
                    if ( Val(lstGroups.Text) = NextUndo.GroupNo Or (Val(lstGroups.Text) = NextUndo.OldGroupNo And NextUndo.OldGroupNo <> -1) ) {
                      UpdateWordList true
                    }
                  case wtByWord:
                  }
                } else {
                 // show wait cursor
                  MDIMain.UseWaitCursor = true;

                 // restore a bunch of words
                  strWords = Split(NextUndo.Word, "|")
                  for (i = 0 To UBound(strWords) Step 4) {
                   // first element is group where word will be restored
                   // second element is old group (if the new word was in a different group)
                   // third element is old word being restored
                   // fourth element is new word being 'undone'

                   // remove new word
                    WordsEdit.RemoveWord strWords(i + 3)
                   // add old word to this group
                    WordsEdit.AddWord strWords(i + 2), CLng(strWords(i))
                   // update groupname
                    UpdateGroupName CLng(strWords(i))

                   // if oldgroup is valid
                    if ( CLng(strWords(i + 1)) <> -1 ) {
                     // restore word to original group
                      WordsEdit.AddWord strWords(3), CLng(strWords(i + 1))
                     // update the originalgroup name
                      UpdateGroupName CLng(strWords(i + 1))
                    }
                  }

                  switch ( Mode) {
                  case wtByGroup:
                   // refreshwordlist
                    UpdateWordList true
                  case wtByWord:
                  }
                  Screen.MousePointer = vbDefault
                }


              case Clear:
               // clear current wordlist
                WordsEdit.Clear
               // clear group list
                lstGroups.Clear
               // now add in groups from the undo stringlist
                for (i = 0 To NextUndo.Group.Count - 1) {
                 // start with group number
                  strTemp = NextUndo.Group(i)
                  strWords = Split(strTemp, "|")
                  lngGroup = CLng(strWords(0))
                 // add words
                  for (j = 1 To UBound(strWords)) {
                    WordsEdit.AddWord strWords[j], lngGroup
                  }
                  switch ( Mode) {
                  case wtByGroup:
                   // if at least one item
                    if ( UBound(strWords) > 0 ) {
                     // add group
                      lstGroups.AddItem strWords(0) & " - " & UCase$(CPToUnicode(strWords(1), SessionCodePage))
                    } else {
                     // add group number only
                      lstGroups.AddItem strWords(0) & " - "
                    }
                  }
                }
                switch ( Mode) {
                case wtByGroup:
                 // select first group
                  SelGroup = -1
                  lstGroups.ListIndex = 0
                case wtByWord:
                }
              }

             // set changed status
              MarkAsChanged

             // update status bar and edit menu
              UpdateStatusBar
              SetEditMenu
            return;
            }

            private void RebuildGroupList(Optional ByVal SelGroup As Long = 0, Optional ByVal MarkChanged As Boolean = true)
             // rebuilds a word list after it was modified by a merge operation
             //  only called in ByGroup mode
              Dim i As Long

             // file is changed
              if ( MarkChanged And !IsChanged ) {
                MarkAsChanged
              }

             // add word groups to listbox
              lstGroups.Clear
              for (i = 0 To WordsEdit.GroupCount - 1) {
               // add group
                lstGroups.AddItem CStr(WordsEdit.Group(i).GroupNum) & " - " & UCase$(CPToUnicode(WordsEdit.Group(i).GroupName, SessionCodePage))
              }

             // select desired group
              lstGroups.ListIndex = GroupIndex(SelGroup)
            return;
            }

            Function AskClose() As Boolean

              Dim rtn As VbMsgBoxResult

             // assume user wants to cancel
              AskClose = true

             // if wordlist has been changed,
              if ( IsChanged ) {
               // ask user to save changes
                rtn = MsgBox("Do you want to save changes to this word list before closing?", vbYesNoCancel, "Words Editor")

               // if user wants to save
                if ( rtn = vbYes ) {
                 // save by calling the menuclick method
                  MenuClickSave
                }

               // return false if cancel was selected
                AskClose = rtn <> vbCancel
              }
            return;
            }

            private Function EditWord(OldWord As String, NewWord As String, Optional ByVal DontUndo As Boolean = false) As Boolean

              Dim NextUndo As WordsUndo
              Dim GroupNo As Long, lngOldGroupNo As Long
              Dim blnMoving As Boolean
              Dim rtn As VbMsgBoxResult
              Dim blnFirst As Boolean, blnDelFirst As Boolean

             // if word hasn't changed
              if ( NewWord = OldWord ) {
               // just exit
                EditWord = true
                return;
              }

             // if new word is an empty string
              if ( NewWord = vbNullString ) {
               // delete the word being changed
                DelWord OldWord, DontUndo
                return;
              }

             // get group number
              GroupNo = WordsEdit(OldWord).Group

             // if the old word was the first word
              blnFirst = (WordsEdit.GroupN(GroupNo).GroupName = OldWord)

             // determine if new word already exists
              blnMoving = WordsEdit.WordExists(NewWord)

              if ( blnMoving ) {
               // track the previous group
                lngOldGroupNo = WordsEdit(NewWord).Group

               // show msg if adding undo
                if ( !DontUndo ) {
                 // RARE, but check for 'rol' and 'anyword' if part of a reserved group
                  if ( lngOldGroupNo = 1 ) {
                    rtn = MsgBoxEx("'" & NewWord & "' is used as a place holder for reserved group 1 (any word)." & vbNewLine & "Are you sure you want to move it to this group?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Change Reserved Group Word", WinAGIHelp, "htm\agi\words.htm#reserved")
                  } else if ( lngOldGroupNo = 9999 ) {
                    rtn = MsgBoxEx("'" & NewWord & "' is used as a place holder for reserved group 9999 (rest of line)." & vbNewLine & "Are you sure you want to move it to this group?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Change Reserved Group Word", WinAGIHelp, "htm\agi\words.htm#reserved")
                  } else {
                   // for any other word, get user OK to move the word
                    rtn = MsgBox("This word already exists in wordgroup " & CStr(lngOldGroupNo) & "." & vbCrLf & _
                    "Do you want to move it to this wordgroup?", vbQuestion + vbYesNo, "Duplicate Word")
                  }

                  if ( rtn = vbNo ) {
                   // reset word
                    rtfWord.Text = OldWord
                    return;
                  }
                } else {
                 // return false
                  return;
                }

               // if this is first word in the OLD group
                blnDelFirst = (WordsEdit.GroupN(lngOldGroupNo).GroupName = NewWord)

               // delete new word from its old group (so it can be added to new group)
                WordsEdit.RemoveWord NewWord
               // if no words left in old group
                if ( WordsEdit.GroupN(lngOldGroupNo).WordCount = 0 ) {
                 // RARE- ok for group 0, 1/9999 to have no words
                  if ( lngOldGroupNo <> 0 And lngOldGroupNo <> 1 And lngOldGroupNo <> 9999 ) {
                   // need to remove group too
                   // delete old group
                    WordsEdit.RemoveGroup lngOldGroupNo

                    switch ( Mode) {
                    case wtByGroup:
                     // delete from listbox
                      lstGroups.RemoveItem IndexByGrp(lngOldGroupNo)
                    case wtByWord:
                    }
                  }
                } else {
                 // if it was first word
                  if ( blnDelFirst ) {
                    UpdateGroupName lngOldGroupNo
                  }
                }
              } else {
               // the new word doesn't exist;
               // set oldgrp equal to new group
                lngOldGroupNo = GroupNo
              }

             // now delete OLD word
              WordsEdit.RemoveWord OldWord

             // add new word
              WordsEdit.AddWord NewWord, GroupNo

             // if this is the first word in the group OR old word was first word in the group
              if ( (WordsEdit.GroupN(GroupNo).GroupName = NewWord) Or blnFirst ) {
                UpdateGroupName GroupNo
              }

             // if not skipping undo
              if ( !DontUndo ) {
               // if adding a new word
                if ( AddNewWord ) {
                 // change last undo object
                  UndoCol(UndoCol.Count).Word = NewWord
                  UndoCol(UndoCol.Count).OldGroupNo = lngOldGroupNo

               // if NOT adding a new group
                } else if ( !AddNewGroup ) {
                 // create undo object
                  NextUndo = new WordsUndo
                  NextUndo.Action = ChangeWord
                  NextUndo.GroupNo = GroupNo
                  NextUndo.Word = NewWord
                  NextUndo.OldWord = OldWord
                  NextUndo.OldGroupNo = lngOldGroupNo

                 // add undo item
                  AddUndo NextUndo

                 //  force logics refresh
                  blnRefreshLogics = true
                }
              }

             // return success
              EditWord = true
            return;
            }

            private void RenumberGroup(OldGroupNo As Long, NewGroupNo As Long, Optional ByVal DontUndo As Boolean = false)

              Dim NextUndo As WordsUndo
              Dim i As Long, lngCount As Long

             // renumber!
              WordsEdit.RenumberGroup OldGroupNo, NewGroupNo

             // rebuild groups list
              switch ( Mode) {
              case wtByGroup:
                #if ( DEBUGMODE <> 1 ) {
                 // disable window painting for the listbox until done
                  SendMessage lstGroups.hWnd, WM_SETREDRAW, 0, 0
                #}

               // easiest way is to clear it and completely rebuild
                lstGroups.Clear
                for (i = 0 To WordsEdit.GroupCount - 1) {
                  lstGroups.AddItem CStr(WordsEdit.Group(i).GroupNum) & " - " & UCase$(WordsEdit.Group(i).GroupName)
                  if ( WordsEdit.Group(i).GroupNum = NewGroupNo ) {
                   // note index of the renumbered group, so it can be selected
                    lngCount = i
                  }
                }

               // reselect the group
                lstGroups.ListIndex = lngCount

                #if ( DEBUGMODE <> 1 ) {
                  SendMessage lstGroups.hWnd, WM_SETREDRAW, 1, 0
                #}

                lstGroups.Refresh

              case wtByWord:
              }

             // if not skipping undo
              if ( !DontUndo ) {
               // create new undo object
                NextUndo = new WordsUndo
                NextUndo.Action = Renumber
                NextUndo.OldGroupNo = OldGroupNo
                NextUndo.GroupNo = NewGroupNo
               // add it
                AddUndo NextUndo
              }

             // force logics refresh
              blnRefreshLogics = true
            return;
            }

            private void Form_Activate()

             // if minimized, exit
             // (to deal with occasional glitch causing focus to lock up)
              if ( Me.WindowState = vbMinimized ) {
                return;
              }

              ActivateActions

             // if visible,
              if ( Visible ) {
               // force resize
                Form_Resize
              }
            }

            private void ActivateActions()

             // if hiding prevwin on lost focus, hide it now
              if ( Settings.HidePreview ) {
                PreviewWin.Hide
              }

             // adjust major menus
            // *'Debug.Print "AdjustMenus 11"
              AdjustMenus rtWords, InGame, true, IsChanged

             // set edit menu
              SetEditMenu

             // update status bar
              UpdateStatusBar

             // if findform is visible,
              if ( FindingForm.Visible ) {
               // set correct mode
                if ( FindingForm.rtfReplace.Visible ) {
                 // show in replace word mode
                  FindingForm.SetForm ffReplaceWord, false
                } else {
                 // show in find word mode
                  FindingForm.SetForm ffFindWord, false
                }
              }

             // set searching form to this form
              SearchForm = Me
            return;
            }

            public void SetEditMenu()
             // sets the menu captions on the Edit menu

              Dim blnAdd As Boolean, lngGrpNo As Long

             // always force form to current
              if ( !(frmMDIMain.ActiveMdiChild Is Me) And !frmMDIMain.ActiveMdiChild  == null ) {
                if ( Me.Visible ) {
                  Me.SetFocus
                }
              }

                mnuEdit.Enabled = true
                mnuEUndo.Visible = true
                mnuEBar0.Visible = true

               // if there is something to undo
                if ( UndoCol.Count > 0 ) {
                  mnuEUndo.Enabled = (Mode = wtByGroup)
                  mnuEUndo.Text = "&Undo " & LoadResString(WORDSUNDOTEXT + UndoCol(UndoCol.Count).Action) & vbTab & "Ctrl+Z"
                } else {
                  mnuEUndo.Enabled = false
                  mnuEUndo.Text = "&Undo" & vbTab & "Ctrl+Z"
                }
                Toolbar1.Buttons("undo").Enabled = .mnuEUndo.Enabled
                mnuERedo.Visible = false

                mnuECustom3.Visible = true
                mnuECustom3.Enabled = true
                mnuECustom3.Text = "Toggle Mode "

                mnuRCustom2.Visible = InGame

               // cut is enabled if mode is group or word, and a group or word is selected
               //   NOT grp 0, 1, 1999
                mnuECut.Visible = true
               // assume OK
                .mnuECut.Enabled = (Mode = wtByGroup)
                if ( SelMode = smGroup ) {
                  if ( lstGroups.ListIndex = -1 ) {
                    mnuECut.Enabled = false
                  } else if ( Val(lstGroups.Text) = 0 Or Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 ) {
                    mnuECut.Enabled = false
                  }
                } else if ( SelMode = smWord ) {
                  mnuECut.Text = "Cu&t" & vbTab & "Ctrl+X"
                  if ( lstWords.ListIndex = -1 ) {
                    mnuECut.Enabled = false
                  }
                 // for group 0, 1, 9999 disable if no words in group
                  if ( Val(lstGroups.Text) = 0 Or Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 ) {
                    if ( WordsEdit.GroupN(Val(lstGroups.Text)).WordCount = 0 ) {
                      mnuECut.Enabled = false
                    }
                  }
                }
                Toolbar1.Buttons("cut").Enabled = .mnuECut.Enabled

               // copy is same as cut
               //  EXCEPT copying group 0 is ok
                mnuECopy.Visible = true
                mnuECopy.Enabled = .mnuECut.Enabled
                if ( SelMode = smGroup And lstGroups.ListIndex = 0 ) {
                  mnuECopy.Enabled = (Mode = wtByGroup)
                }
                mnuECopy.Text = "&Copy" & vbTab & "Ctrl+C"
                Toolbar1.Buttons("copy").Enabled = .mnuECopy.Enabled

               // paste if something on clipboard
                mnuEPaste.Visible = true
                switch ( Mode) {
                case wtByGroup:
                  switch ( WordsClipboard.Action) {
                  case DelWord:
                    mnuEPaste.Enabled = SelMode <> smNone And SelMode <> smGroup
                   // can't paste into group 1 or group 9999
                    if ( Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 ) {
                      mnuEPaste.Enabled = false
                    }
                  case DelGroup:
                    mnuEPaste.Enabled = SelMode <> smNone
                  default:
                   // if real clipboard has something, enable pasting
                   // the paste function will validate whatever is there
                    mnuEPaste.Enabled = Clipboard.GetFormat(vbCFText)
                  }
                case wtByWord:
                  mnuEPaste.Enabled = false
                }
                mnuEPaste.Text = "&Paste" & vbTab & "Ctrl+V"
               // EXCEPT no pasting to group 1 or 9999
                if ( Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 ) {
                  mnuEPaste.Enabled = false
                }
                Toolbar1.Buttons("paste").Enabled = .mnuEPaste.Enabled

               // delete same as cut
                mnuEDelete.Visible = true
                mnuEDelete.Enabled = .mnuECut.Enabled
                mnuEDelete.Text = "&Delete" & vbTab & "Del"
                Toolbar1.Buttons("delete").Enabled = .mnuECut.Enabled

               // clear
                mnuEClear.Visible = true
                mnuEClear.Enabled = (Mode = wtByGroup)
                mnuEClear.Text = "Clear Word List" & vbTab & "Shift+Del"

               // insert used to add new groups
                mnuEInsert.Visible = true
                mnuEInsert.Enabled = (Mode = wtByGroup)
                mnuEInsert.Text = "Insert &Group" & vbTab & "Ins"

               //  select-all used to add new words to active group
                mnuESelectAll.Visible = true
                switch ( Mode) {
                case wtByGroup:
                  mnuESelectAll.Enabled = lstGroups.ListIndex <> -1
                case wtByWord:
                  mnuESelectAll.Enabled = false
                }
                mnuESelectAll.Text = "Insert &Word" & vbTab & "Shift+Ins"
               // if group 1 or 9999 insert only allowed if empty
                if ( Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 ) {
                  if ( WordsEdit.GroupN(Val(lstGroups.Text)).WordCount > 0 ) {
                    mnuESelectAll.Enabled = false
                  }
                }

               // find, and find again depend on search status
                mnuEBar1.Visible = true
                mnuEFind.Visible = true
                mnuEFind.Enabled = true
                mnuEFind.Text = "&Find" & vbTab & "Ctrl+F"
                mnuEFindAgain.Visible = true
                mnuEFindAgain.Enabled = (LenB(GFindText) <> 0)
                mnuEFindAgain.Text = "Find &Again" & vbTab & "F3"
                mnuEReplace.Visible = true
                mnuEReplace.Enabled = (Mode = wtByGroup)
                mnuEReplace.Text = "Replace" & vbTab & "Ctrl+H"

               // custom menu1 is used for edit word
                mnuEBar2.Visible = true
                mnuECustom1.Visible = true
                switch ( Mode) {
                case wtByGroup:
                 // enabled if a word selected
                  mnuECustom1.Enabled = (SelMode = smWord) And lstWords.ListIndex <> -1
                case wtByWord:
                  mnuECustom1.Enabled = false
                }
               // EXCEPT if group 0, 1, or 9999 placeholder
                if ( Val(lstGroups.Text) = 0 Or Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 ) {
                  if ( WordsEdit.GroupN(Val(lstGroups.Text)).WordCount = 0 ) {
                    mnuECustom1.Enabled = false
                  }
                }
                mnuECustom1.Text = "Edit Word" & vbTab & "Alt+Enter"

               // custom menu2 is used for find-in-logic
                mnuECustom2.Visible = true
                switch ( Mode) {
                case wtByGroup:
                  mnuECustom2.Enabled = (SelMode <> smNone) And InGame
                case wtByWord:
                  mnuECustom2.Enabled = false
                }
                mnuECustom2.Text = "Find in &Logics" & vbTab & "Shift+Ctrl+F"

                mnuECustom4.Visible = false

                Toolbar1.Buttons("group").Enabled = (Mode = wtByGroup)

             // RARE - check for group 1 or 9999 - add word allowed if currently
             // there is not a word assigned
              switch ( Val(lstGroups.Text)) {
              case 1, 9999:
              lngGrpNo = Val(lstGroups.Text)
               //  also rare, but make sure group exists before counting words
                if ( !WordsEdit.GroupExists(lngGrpNo) ) {
                  blnAdd = true
                } else if ( (WordsEdit.GroupN(lngGrpNo).WordCount = 0) ) {
                  blnAdd = true
                }
                Toolbar1.Buttons("word").Enabled = blnAdd And (Mode = wtByGroup)
              default:
                Toolbar1.Buttons("word").Enabled = (Mode = wtByGroup)
              }

              Toolbar1.Buttons("findinlogic").Enabled = frmMDIMain.mnuECustom2.Enabled
            return;
            }

            private void Form_Load()

              CalcHeight = ScaleHeight
              if ( CalcWidth < MIN_WIDTH ) {
                CalcHeight = MIN_HEIGHT
              }

              CalcWidth = ScaleWidth
              if ( CalcWidth < MIN_WIDTH ) {
                CalcWidth = MIN_WIDTH
              }

            #if ( DEBUGMODE <> 1 ) {
             // subclass the word list
              PrevWLBWndProc = SetWindowLong(lstWords.hWnd, GWL_WNDPROC, AddressOf LBWndProc)
             // subclass the group list
              PrevGLBWndProc = SetWindowLong(lstGroups.hWnd, GWL_WNDPROC, AddressOf LBWndProc)

             // subclass the text boxes
              PrevGrpTBWndProc = SetWindowLong(txtGrpNum.hWnd, GWL_WNDPROC, AddressOf TBWndProc)
            #}
             // initialize undo collection
              UndoCol = new Collection

             // initialize object
              WordsEdit = new AGIWordList
              WordsEdit.NewWords

             // set split to be middle of form

             // setup fonts
              InitFonts
             // set code page to correct value
              rtfWord.CodePage = SessionCodePage

             // set width of splitter and hide icon
              picSplit.Width = SPLIT_WIDTH
              picSplitIcon.Width = SPLIT_WIDTH
              picSplitIcon.Visible = false

             // update panels
              UpdatePanels CalcWidth / 2

              SelGroup = -1
            }

            public void Clear(Optional ByVal DontUndo As Boolean = false, Optional ByVal DefaultWords As Boolean = false)

              Dim NextUndo As WordsUndo
              Dim i As Long, j As Long
              Dim strTemp As String

             // if not skipping undo
              if ( !DontUndo ) {
                NextUndo = new WordsUndo
                NextUndo.Group = new StringList
                NextUndo.Action = Clear
               // add each group of words to the undo stringlist, with groupnumber first, and words after
               // separated by a pipe character
                for (i = 0 To WordsEdit.GroupCount - 1) {
                 // start with group number
                  strTemp = CStr(WordsEdit.Group(i).GroupNum)
                 // then add words
                  for (j = 0 To WordsEdit.Group(i).WordCount - 1) {
                    strTemp = strTemp & "|" & WordsEdit.Group(i).Word[j]
                  }
                 // now add group to undo stringlist
                  NextUndo.Group.Add strTemp
                }

                AddUndo NextUndo

               //  force logics refresh
                blnRefreshLogics = true
              }

             // clear wordlist
              WordsEdit.Clear

              lstGroups.Clear
              lstWords.Clear

             // if adding default words
              if ( DefaultWords ) {
               // add "a" and "rol" and "anyword"
                WordsEdit.AddWord "a", 0
                WordsEdit.AddWord "anyword", 1
                WordsEdit.AddWord "rol", 9999

                switch ( Mode) {
                case wtByGroup:
                  lstGroups.AddItem "0 - <null words>"
                  lstGroups.AddItem "1 - <any word>"
                  lstGroups.AddItem "9999 - <rest of line>"
                 // switch listindex twice to force it to update
                  lstGroups.ListIndex = 1
                  lstGroups.ListIndex = 0
                case wtByWord:
                }
              }
            }


            private void Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)

              Cancel = !AskClose
            }

            void Form_Resize()

             // use separate variables for managing minimum width/height
              if ( ScaleWidth < MIN_WIDTH ) {
                CalcWidth = MIN_WIDTH
              } else {
                CalcWidth = ScaleWidth
              }
              if ( ScaleHeight < MIN_HEIGHT ) {
                CalcHeight = MIN_HEIGHT
              } else {
                CalcHeight = ScaleHeight
              }

             // if the form is not visible, or minimied
              if ( !Visible Or WindowState = vbMinimized ) {
                return;
              }

             // if restoring from minimize, activation may not have triggered
              if ( MainStatusBar.Tag <> CStr(rtWords) ) {
                ActivateActions
              }

             // adjust height of splits
              picSplit.Height = CalcHeight
              picSplitIcon.Height = CalcHeight

             // ratio split between the list boxes
             // update panels
              if ( picSplit.Left * CalcWidth / (lstWords.Left + lstWords.Width + WE_MARGIN) < MIN_SPLIT_V ) {
                UpdatePanels MIN_SPLIT_V
              } else {
                UpdatePanels picSplit.Left * CalcWidth / (lstWords.Left + lstWords.Width + WE_MARGIN)
              }
            return;
            }
            private void UpdatePanels(ByVal SplitLoc As Long)

             // adjust labels
              lblGroups.Width = SplitLoc - TBW
              lblWords.Left = SplitLoc + SPLIT_WIDTH
              lblWords.Width = CalcWidth - SplitLoc - SPLIT_WIDTH - WE_MARGIN

             // adjust group listbox
            #if ( DEBUGMODE <> 1 ) {
             // disable window painting for the listbox until done
              SendMessage lstGroups.hWnd, WM_SETREDRAW, 0, 0
            #}

              lstGroups.Height = CalcHeight - LGT - WE_MARGIN
              lstGroups.Width = SplitLoc - TBW

             // adjust words listbox
              lstWords.Height = lstGroups.Height
              lstWords.Left = SplitLoc + SPLIT_WIDTH
              lstWords.Width = lblWords.Width

             // position splitter
              picSplit.Left = SplitLoc
              picSplit.Height = CalcHeight - picSplit.Top

            #if ( DEBUGMODE <> 1 ) {
              SendMessage lstGroups.hWnd, WM_SETREDRAW, 1, 0
            #}
              lstGroups.Refresh

            return;
            }

            private void Form_KeyDown(KeyCode As Integer, Shift As Integer)

             // detect and respond to keyboard shortcuts

             // always check for help first
              if ( Shift = 0 And KeyCode = vbKeyF1 ) {
                MenuClickHelp
                KeyCode = 0
                return;
              }

             // if editing groupnum or word,
              if ( txtGrpNum.Visible Or rtfWord.Visible ) {
                return;
              }

             // check for global shortcut keys
              CheckShortcuts KeyCode, Shift
              if ( KeyCode = 0 ) {
                return;
              }

              switch ( Shift) {
              case vbCtrlMask:
                switch ( KeyCode) {
                case vbKeyZ:     // undo
                  if ( frmMDIMain.mnuEUndo.Enabled ) {
                    MenuClickUndo
                    KeyCode = 0
                  }

                case vbKeyX: // cut
                  if ( frmMDIMain.mnuECut.Enabled ) {
                    MenuClickCut
                    KeyCode = 0
                  }

                case vbKeyC: // copy
                  if ( frmMDIMain.mnuECopy.Enabled ) {
                    MenuClickCopy
                    KeyCode = 0
                  }

                case vbKeyV: // paste
                  if ( frmMDIMain.mnuEPaste.Enabled ) {
                    MenuClickPaste
                    KeyCode = 0
                  }

                case vbKeyF:
                 // find
                  if ( frmMDIMain.mnuEFind.Enabled ) {
                    MenuClickFind
                    KeyCode = 0
                  }

                case vbKeyH:
                 // replace
                  if ( frmMDIMain.mnuEReplace.Enabled ) {
                    MenuClickReplace
                    KeyCode = 0
                  }
                }

              case 0: // no shift, ctrl, alt
                switch ( KeyCode) {
                case vbKeyInsert:
                  if ( frmMDIMain.mnuEInsert.Enabled ) {
                    MenuClickInsert
                    KeyCode = 0
                  }

                case vbKeyDelete:
                  if ( frmMDIMain.mnuEDelete.Enabled ) {
                    MenuClickDelete
                    KeyCode = 0
                  }

                case vbKeyF3
                 // find again
                  if ( frmMDIMain.mnuEFindAgain.Enabled ) {
                    MenuClickFindAgain
                    KeyCode = 0
                  }
                }

              case vbShiftMask:
                switch ( KeyCode) {
                case vbKeyDelete:
                  if ( frmMDIMain.mnuEClear.Enabled ) {
                    MenuClickClear
                    KeyCode = 0
                  }

                case vbKeyInsert:
                  if ( frmMDIMain.mnuESelectAll.Enabled ) {
                    MenuClickSelectAll
                    KeyCode = 0
                  }
                }

              case 3: // shift+ctrl
                switch ( KeyCode) {
                case vbKeyF:
                  if ( frmMDIMain.mnuECustom2.Enabled ) {
                    MenuClickECustom2
                    KeyCode = 0
                  }
                }

              case vbAltMask:
                switch ( KeyCode) {
                case vbKeyReturn:
                  if ( frmMDIMain.mnuECustom1.Enabled ) {
                    MenuClickECustom1
                    KeyCode = 0
                  }

                case vbKeyF:
                  if ( frmMDIMain.mnuRCustom1.Enabled ) {
                    MenuClickCustom1
                    KeyCode = 0
                  }
                }
              }
            }
            private void Form_Unload(Cancel As Integer)

             // ensure edit object is dereferenced
              if ( !WordsEdit  == null ) {
               // *'Debug.Assert WordsEdit.Loaded
                WordsEdit.Unload
              }

             // if this is the ingame list
              if ( InGame ) {
               // reset inuse flag
                WEInUse = false
               // release the object
                WordEditor = Nothing
              }

             // always reset the synonym search; it
             // only works when the editor is open
              GFindSynonym = false

            #if ( DEBUGMODE <> 1 ) {
             // release subclass hook to listboxes
              SetWindowLong lstWords.hWnd, GWL_WNDPROC, PrevWLBWndProc
              SetWindowLong lstGroups.hWnd, GWL_WNDPROC, PrevGLBWndProc
             // and text boxes
              SetWindowLong txtGrpNum.hWnd, GWL_WNDPROC, PrevGrpTBWndProc
            #}

             // need to check if this is last form
              LastForm Me
            return;
            }

            public void lstWords_Click()

              switch ( Mode) {
              case wtByGroup:
               // if something is selected
                if ( lstWords.ListIndex <> -1 ) {
                  if ( SelMode <> smWord ) {
                   // reset mode
                    SelMode = smWord
                    lstWords.SetFocus
                  }
                  SetEditMenu
                }

              case wtByWord:
               // changing word needs to update the group
                lstGroups.List(0) = CStr(WordsEdit(lstWords.ListIndex).Group)
              }

            return;
            }

            private void lstWords_DblClick()

              switch ( Mode) {
              case wtByGroup:
               // same as clicking editword menu
                MenuClickECustom1
              case wtByWord:
              }
            }

            private void lstWords_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

              Dim lngIndex As Long

             // save x and y
              mX = X
              mY = Y

             // ensure lstWord has focus
              lstWords.SetFocus

             // if clicking with right button
              if ( Button = vbRightButton ) {
               // select the item that the cursor is over
                lngIndex = (Y / ScreenTWIPSY \ lngRowHeight) + lstWords.TopIndex

               // if something clicked,
                if ( lngIndex <= lstWords.ListCount - 1 ) {
                 // select the item clicked
                  lstWords.ListIndex = lngIndex
                }

               // if mode is not word
                if ( SelMode <> smWord ) {
                 // reset mode
                  SelMode = smWord
                }
                SetEditMenu

               // make sure this form is the active form
                if ( !(frmMDIMain.ActiveMdiChild Is Me) ) {
                 // set focus before showing the menu
                  Me.SetFocus
                }
               // need doevents so form activation occurs BEFORE popup
               // otherwise, errors will be generated because of menu
               // adjustments that are made in the form_activate event
                SafeDoEvents
               // show edit menu
                PopupMenu frmMDIMain.mnuEdit, , X / ScreenTWIPSX + picSplit.Left, Y / ScreenTWIPSY
              }
            return;
            }

            private void txtGrpNum_KeyDown(KeyCode As Integer, Shift As Integer)

              Dim strCBText As String, blnPasteOK As Boolean

             // need to handle cut, copy, paste, select all shortcuts
              switch ( Shift) {
              case vbCtrlMask:
                switch ( KeyCode) {
                case vbKeyX: // cut
                 // only is something selected
                  if ( txtGrpNum.SelLength > 0 ) {
                   // put the selected text into clipboard
                    Clipboard.Clear
                    Clipboard.SetText txtGrpNum.SelText
                   // then delete it
                    txtGrpNum.SelText = ""
                  }
                  KeyCode = 0
                  Shift = 0

                case vbKeyC: // copy
                 // only is something selected
                  if ( txtGrpNum.SelLength > 0 ) {
                   // put the selected text into clipboard
                    Clipboard.Clear
                    Clipboard.SetText txtGrpNum.SelText
                  }
                  KeyCode = 0
                  Shift = 0

                case vbKeyV: // paste
                 // paste only allowed if clipboard text is a valid number
                  strCBText = Clipboard.GetText
                 //  put a zero in front, just in case it's a hex or octal
                 //  string; we don't want those
                  if ( IsNumeric("0" & strCBText) And Len(strCBText) > 0 ) {
                   // only integers
                    if ( Int(strCBText) = Val(strCBText) ) {
                     // range 0-65535
                      if ( Val(strCBText) >= 0 And Val(strCBText) <= 65535 ) {
                        blnPasteOK = true
                      } else {
                        blnPasteOK = false
                      }
                    } else {
                      blnPasteOK = false
                    }
                  } else {
                    blnPasteOK = false
                  }

                  if ( blnPasteOK ) {
                   // put cbtext into selection
                    txtGrpNum.SelText = Val(strCBText)
                  }
                  KeyCode = 0
                  Shift = 0

                case vbKeyA: // select all
                  txtGrpNum.SelStart = 0
                  txtGrpNum.SelLength = Len(txtGrpNum.Text)
                  KeyCode = 0
                  Shift = 0
                }
              }
            return;
            }

            private void txtGrpNum_KeyPress(KeyAscii As Integer)

             // only numbers , backspace
              switch ( KeyAscii) {
              case 48 To 57, 8:
               // ok

              case 9, 10, 13: // return (tab mimics enter key on this form)

                if ( ValidateGrpNum() ) {
                 // hide the box
                  txtGrpNum.Visible = false
                  lstGroups.SetFocus
                 // reenable edit menu
                  frmMDIMain.mnuEdit.Enabled = true
                 //  and editing toolbar buttons
                    Me.Toolbar1.Buttons.Item(1).Enabled = true
                    Me.Toolbar1.Buttons.Item(2).Enabled = true
                    Me.Toolbar1.Buttons.Item(3).Enabled = true
                    Me.Toolbar1.Buttons.Item(4).Enabled = true
                    Me.Toolbar1.Buttons.Item(6).Enabled = true
                    .Item(7).Enabled = true

                } else {
                 // need to force focus (might be a tab thing?)
                  txtGrpNum.SetFocus
                }

                KeyAscii = 0

              case 27: // escape
                txtGrpNum.Visible = false
                lstGroups.SetFocus
                KeyAscii = 0
               // reenable edit menu
                frmMDIMain.mnuEdit.Enabled = true
               //  and editing toolbar buttons
                  Me.Toolbar1.Buttons.Item(1).Enabled = true
                  Me.Toolbar1.Buttons.Item(2).Enabled = true
                  Me.Toolbar1.Buttons.Item(3).Enabled = true
                  Me.Toolbar1.Buttons.Item(4).Enabled = true
                  Me.Toolbar1.Buttons.Item(6).Enabled = true
                  Me.Toolbar1.Buttons.Item(7).Enabled = true

              default:
               //  this also kills shortcuts like cut/paste
                KeyAscii = 0
              }
            return;
            }

            public void txtGrpNum_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)


              Dim strCBText As String

             //  check for right click to show context menu
              if ( Button = vbRightButton ) {
               // configure the edit menu
                  mnuTBCopy.Enabled = txtGrpNum.SelLength > 0
                  mnuTBCut.Enabled = .mnuTBCopy.Enabled
                 // paste only allowed if clipboard text is a valid number
                  strCBText = Clipboard.GetText
                 //  put a zero in front, just in case it's a hex or octal
                 //  string; we don't want those
                  if ( IsNumeric("0" & strCBText) And Len(strCBText) > 0 ) {
                   // only integers
                    if ( Int(strCBText) = Val(strCBText) ) {
                     // range 0-65535
                      if ( Val(strCBText) >= 0 And Val(strCBText) <= 65535 ) {
                        mnuTBPaste.Enabled = true
                      } else {
                        mnuTBPaste.Enabled = false
                      }
                    } else {
                      mnuTBPaste.Enabled = false
                    }
                  } else {
                    mnuTBPaste.Enabled = false
                  }

                 //  set the command operation to none
                  TBCmd = 0

                 // now show the popup menu
                  PopupMenu mnuTBPopup

               // deal with the result
                switch ( TBCmd) {
                case 0: // canceled
                 // do nothing
                  return;
                case 1: // cut
                 // put the selected text into clipboard
                  Clipboard.Clear
                  Clipboard.SetText txtGrpNum.SelText
                 // then delete it
                  txtGrpNum.SelText = ""
                case 2: // copy
                 // put the selected text into clipboard
                  Clipboard.Clear
                  Clipboard.SetText txtGrpNum.SelText
                case 3: // paste
                 // put cbtext into selection
                  txtGrpNum.SelText = strCBText
                case 4: // select all
                  txtGrpNum.SelStart = 0
                  txtGrpNum.SelLength = Len(txtGrpNum.Text)
                }
              }
            return;
            }

            private void txtGrpNum_Validate(Cancel As Boolean)

             // this will handle cases where user tries to 'click' on something,
             // but NOT when keys are pressed?

              if ( !txtGrpNum.Visible ) { return;

             // if OK, hide the text box
              if ( ValidateGrpNum() ) {
                txtGrpNum.Visible = false

               // reenable edit menu
                frmMDIMain.mnuEdit.Enabled = true
               //  and editing toolbar buttons
                  Me.Toolbar1.Buttons.Item(1).Enabled = true
                  Me.Toolbar1.Buttons.Item(2).Enabled = true
                  Me.Toolbar1.Buttons.Item(3).Enabled = true
                  Me.Toolbar1.Buttons.Item(4).Enabled = true
                  Me.Toolbar1.Buttons.Item(6).Enabled = true
                  Me.Toolbar1.Buttons.Item(7).Enabled = true
              } else {
             // if not OK, cancel
                Cancel = true
              }
            return;
            }

            private void rtfWord_KeyDown(KeyCode As Integer, Shift As Integer)

              Dim strCBText As String

             // ignore enter key
              if ( KeyCode = 13 ) { KeyCode = 0

             // always check for help first
              if ( Shift = 0 And KeyCode = vbKeyF1 ) {
                MenuClickHelp
                KeyCode = 0
                return;
              }

             // need to handle cut, copy, paste, select all shortcuts
              switch ( Shift) {
              case vbCtrlMask:
                switch ( KeyCode) {
                case vbKeyX: // cut
                 // only is something selected
                  if ( rtfWord.Selection.Range.Length > 0 ) {
                   // put the selected text into clipboard
                    rtfWord.Selection.Range.Cut
                  }
                  KeyCode = 0
                  Shift = 0

                case vbKeyC: // copy
                 // only is something selected
                  if ( rtfWord.Selection.Range.Length > 0 ) {
                   // put the selected text into clipboard
                    rtfWord.Selection.Range.Copy
                  }
                  KeyCode = 0
                  Shift = 0

                case vbKeyV: // paste
                 //  trim white space off clipboard text, and no multi-line text
                  strCBText = Trim$(Clipboard.GetText)
                  strCBText = Replace(strCBText, vbCr, "")
                  strCBText = Replace(strCBText, vbLf, "")
                 // paste only allowed if clipboard has text
                  if ( Len(strCBText) > 0 ) {
                     // any other invalid characters will have to be caught
                     // by the validation check

                     // put cbtext into selection (force to lower case)
                      rtfWord.Selection.Range.Text = LCase$(CPToUnicode(strCBText, SessionCodePage))
                      rtfWord.Selection.Range.Collapse reEnd
                  }
                  KeyCode = 0
                  Shift = 0

                case vbKeyA: // select all
                  rtfWord.Selection.Range.StartPos = 0
                  rtfWord.Selection.Range.EndPos = Len(rtfWord.Text)
                  KeyCode = 0
                  Shift = 0

                case vbKeyInsert:
                 // set flag so other controls know charpicker is active
                  PickChar = true
                  ShowCharPickerForm rtfWord
                  KeyCode = 0
                  Shift = 0
                 // done with charpicker
                  PickChar = false
                }
              }
            return;
            }

            private void rtfWord_KeyPress(KeyAscii As Integer)

              Dim EditNewWord As String

              switch ( KeyAscii) {
              case 97 To 122, 8:
             //  // a-z and backspace always ok
              case 65 To 90:
               // A-Z converted to lowercase
                KeyAscii = KeyAscii + 32

              case 32: // space
               // not allowed for first character
                if ( LenB(rtfWord.Text) = 0 ) {
                  KeyAscii = 0
                }

              case 33, 34, 39, 40, 41, 44, 45, 46, 58, 59, 63, 91, 93, 96, 123, 125:
               //     !'(),-.:;?[]`{}
               // NEVER allowed; these values get removed by the input function
                KeyAscii = 0

              case 35 To 38, 42, 43, 47 To 57, 60 To 62, 64, 92, 94, 95, 124, 126, 127:
               // these characters:
               //     #$%&*+/0123456789<=>@\^_|~
               // NOT allowed as first char
                if ( LenB(rtfWord.Text) = 0 ) {
                 // UNLESS supporting Power Pack mod
                  if ( !PowerPack ) {
                    KeyAscii = 0
                  }
                }

              case 9, 10, 13: // enter or tab (tab mimics enter key on this form)
                EditNewWord = UnicodeToCP(LCase$(rtfWord.Text), SessionCodePage)
                if ( ValidateWord(EditNewWord) ) {
                  FinishEdit EditOldWord, EditNewWord, false
                } else {
                 // need to force focus (might be a tab thing?)
                    rtfWord.Selection.Range.StartPos = 0
                    rtfWord.Selection.Range.EndPos = Len(.Text)
                    rtfWord.SetFocus
                }

                KeyAscii = 0

              case 27:  // ESC'
               // cancel
                rtfWord.Visible = false
                lstWords.SetFocus
               // reenable edit menu
                frmMDIMain.mnuEdit.Enabled = true
               //  and editing toolbar buttons
                  Me.Toolbar1.Buttons.Item(1).Enabled = true
                  Me.Toolbar1.Buttons.Item(2).Enabled = true
                  Me.Toolbar1.Buttons.Item(3).Enabled = true
                  Me.Toolbar1.Buttons.Item(4).Enabled = true
                  Me.Toolbar1.Buttons.Item(6).Enabled = true
                  Me.Toolbar1.Buttons.Item(7).Enabled = true
                KeyAscii = 0

              default:
               // extended chars not allowed
               // UNLESS supporting the Power Pack mod
                if ( !PowerPack ) {
                  KeyAscii = 0
                }
              }
            return;
            }

            private void FinishEdit(OldWord As String, NewWord As String, Optional ByVal DontUndo As Boolean = false)

             // hide the textbox
              rtfWord.Visible = false
             // and save the new word
              EditWord OldWord, NewWord, DontUndo

              switch ( Mode) {
              case wtByGroup:
               // reselect the group
                UpdateWordList true

               // reselect the word
                lstWords.Text = CPToUnicode(NewWord, SessionCodePage)
                lstWords.SetFocus
              case wtByWord:
              }

             // reenable edit menu
              frmMDIMain.mnuEdit.Enabled = true
             //  and editing toolbar buttons
                Me.Toolbar1.Buttons.Item(1).Enabled = true
                Me.Toolbar1.Buttons.Item(2).Enabled = true
                Me.Toolbar1.Buttons.Item(3).Enabled = true
                Me.Toolbar1.Buttons.Item(4).Enabled = true
                Me.Toolbar1.Buttons.Item(6).Enabled = true
                Me.Toolbar1.Buttons.Item(7).Enabled = true

             // always clear new flags
              AddNewWord = false
              AddNewGroup = false
            return;
            }

            private void rtfWord_MouseDown(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

              Dim strCBText As String

             //  check for right click to show context menu
              if ( Button = vbRightButton ) {
               // configure the edit menu
                  mnuTBCopy.Enabled = rtfWord.Selection.Range.Length > 0
                  mnuTBCut.Enabled = .mnuTBCopy.Enabled
                 //  put cbtext into selection
                 //  trim white space off clipboard text, and no multi-line text
                  strCBText = Trim$(Clipboard.GetText)
                  strCBText = Replace(strCBText, vbCr, "")
                  strCBText = Replace(strCBText, vbLf, "")
                 // paste only allowed if clipboard has text
                  if ( Len(strCBText) > 0 ) {
                   // any other invalid characters will have to be caught
                   // by the validation check
                    mnuTBPaste.Enabled = true
                  } else {
                    mnuTBPaste.Enabled = false
                  }

                 // char picker available if PowerPack is enabled
                  mnuTBSeparator1.Visible = PowerPack
                  mnuTBCharMap.Visible = PowerPack

                 //  set the command operation to none
                  TBCmd = 0

                 // now show the popup menu
                  PopupMenu mnuTBPopup

               // deal with the result
                switch ( TBCmd) {
                case 0: // canceled
                 // do nothing
                  return;
                case 1: // cut
                 // put the selected text into clipboard
                  rtfWord.Selection.Range.Cut
                case 2: // copy
                 // put the selected text into clipboard
                  rtfWord.Selection.Range.Copy
                case 3: // paste
                 // put cbtext(converted from cb-byte) into selection (force lower case)
                  rtfWord.Selection.Range.Text = LCase$(CPToUnicode(strCBText, SessionCodePage))
                  rtfWord.Selection.Range.Collapse reEnd
                case 4: // select all
                  rtfWord.Selection.Range.StartPos = 0
                  rtfWord.Selection.Range.EndPos = Len(rtfWord.Text)
                case 5: //  show char picker
                 // set flag so other controls know charpicker is active
                  PickChar = true
                  ShowCharPickerForm rtfWord
                 // done with charpicker
                  PickChar = false
                }
              }
            return;
            }

            private void rtfWord_Validate(Cancel As Boolean)

             // this will handle cases where user tries to 'click' on something,
             // but NOT when keys are pressed?
              Dim tmpWord As String

              if ( !rtfWord.Visible ) { return;

              tmpWord = UnicodeToCP(LCase$(rtfWord.Text), SessionCodePage)
              if ( ValidateWord(tmpWord) ) {
                FinishEdit EditOldWord, tmpWord, false
              } else {
             // if not OK, cancel
                Cancel = true
                  rtfWord.Selection.Range.StartPos = 0
                  rtfWord.Selection.Range.EndPos = Len(.Text)
              }
            return;
            }

            private Function CurrentWord() As String
             // gets current word from WordsEdit determined by current
             // group listbox and word listbox selections

             // validate something selected first
              if ( lstGroups.ListIndex = -1 ) {
                return;
              }
              if ( lstWords.ListIndex = -1 ) {
                return;
              }

             // return the word
              CurrentWord = WordsEdit.GroupN(Val(lstGroups.Text)).Word(lstWords.ListIndex)
            }

                        */
        }

        #endregion

        internal void InitFonts() {
            defaultfont = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
            boldfont = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value, FontStyle.Bold);
            label1.Font = defaultfont;
            label2.Font = defaultfont;
            txtGroupEdit.Font = defaultfont;
            txtWordEdit.Font = defaultfont;
            lstGroups.Font = defaultfont;
            if (GroupMode && (EditGroupIndex == 1 || EditGroupIndex == 9999)) {
                lstWords.Font = boldfont;
                lstWords.ForeColor = Color.DarkGray;
            }
            else {
                lstWords.Font = defaultfont;
                lstWords.ForeColor = Color.Black;
            }
            lstGroups.Top = label1.Bottom;
            txtGroupEdit.Top = lstWords.Top = lstGroups.Top;
            //rtfWord.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
            //txtGrpNum.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
        }

        public bool LoadWords(WordList loadwords) {
            InGame = loadwords.InGame;
            IsChanged = loadwords.IsChanged;
            try {
                if (InGame) {
                    loadwords.Load();
                }
            }
            catch {
                return false;
            }
            if (loadwords.ErrLevel < 0) {
                return false;
            }
            EditWordList = loadwords.Clone();
            EditWordListFilename = loadwords.ResFile;

            lstGroups.Items.Clear();
            for (int i = 0; i < EditWordList.GroupCount; i++) {
                lstGroups.Items.Add((EditWordList.GroupByIndex(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.GroupByIndex(i).GroupName);
            }
            lstGroups.SelectedIndex = 0;
            // statusbar has not been merged yet
            statusStrip1.Items["spGroupCount"].Text = "Group Count: " + EditWordList.GroupCount;
            statusStrip1.Items["spWordCount"].Text = "Word Count: " + EditWordList.WordCount;

            // caption
            Text = "WORDS.TOK Editor - ";
            if (InGame) {
                Text += EditGame.GameID;
            }
            else {
                if (EditWordListFilename.Length > 0) {
                    Text += Common.Base.CompactPath(EditWordListFilename, 75);
                }
                else {
                    WrdCount++;
                    Text += "NewWords" + WrdCount.ToString();
                }
            }

            mnuRSave.Enabled = !IsChanged;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = !IsChanged;
            return true;
        }

        public void ImportWords(string importfile) {
            WordList tmpList;

            MDIMain.UseWaitCursor = true;
            try {
                tmpList = new(importfile);
            }
            catch (Exception e) {
                ErrMsgBox(e, "An error occurred during import:", "", "Import WORDS.TOK File Error");
                MDIMain.UseWaitCursor = false;
                return;
            }
            // replace current wordlist
            EditWordList = tmpList;
            EditWordListFilename = importfile;
            // TODO: refresh the editor
            label1.Text = $"Word Count: {EditWordList.WordCount}";
            if (EditWordList.GroupExists(2)) {
                label2.Text = $"Group 2 Name: {EditWordList.GroupByNumber(2).GroupName}";
            }
            else {
                label2.Text = $"Group count: {EditWordList.GroupCount}";
            }
            MarkAsChanged();
            MDIMain.UseWaitCursor = false;
        }

        public void SaveWords() {
            // TODO: autoupdate still needs significant work; disable it for now
            //bool blnDontAsk = false;
            //DialogResult rtn = DialogResult.No;
            //if (InGame && WinAGISettings.AutoUpdateWords != 1) {
            //    if (WinAGISettings.AutoUpdateWords == 0) {
            //        rtn = MsgBoxEx.Show(MDIMain,
            //            "Do you want to update all game logics with the changes made in the word list?",
            //            "Update Logics?",
            //            MessageBoxButtons.YesNo,
            //            MessageBoxIcon.Question,
            //            "Always take this action when saving the word list.", ref blnDontAsk);
            //        if (blnDontAsk) {
            //            if (rtn == DialogResult.Yes) {
            //                WinAGISettings.AutoUpdateWords = 2;
            //            }
            //            else {
            //                WinAGISettings.AutoUpdateWords = 1;
            //            }
            //        }
            //    }
            //    else {
            //        rtn = DialogResult.Yes;
            //    }
            //    if (rtn == DialogResult.Yes) {
            //        AutoUpdate();
            //    }
            //}

            if (InGame) {
                MDIMain.UseWaitCursor = true;
                bool loaded = EditGame.WordList.Loaded;
                if (!loaded) {
                    EditGame.WordList.Load();
                }
                EditGame.WordList.CloneFrom(EditWordList);
                try {
                    EditGame.WordList.Save();
                }
                catch (Exception ex) {
                    ErrMsgBox(ex, "Error during WORDS.TOK compilation: ",
                        "Existing WORDS.TOK has not been modified.",
                        "WORDS.TOK Compile Error");
                    MDIMain.UseWaitCursor = false;
                    return;
                }
                if (blnRefreshLogics) {
                    MakeAllChanged();
                    blnRefreshLogics = false;
                }
                RefreshTree(AGIResType.Words, 0);
                MDIMain.ClearWarnings(AGIResType.Words, 0);
                MDIMain.UseWaitCursor = false;
            }
            else {
                if (EditWordList.ResFile.Length == 0) {
                    ExportWords();
                    return;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    try {
                        EditWordList.Save();
                    }
                    catch (Exception ex) {
                        ErrMsgBox(ex, "An Error occurred while saving this wordlist:",
                            "Existing wordlist file has not been modified.",
                            "Wordlist Save Error");
                        MDIMain.UseWaitCursor = false;
                        return;
                    }
                    MDIMain.UseWaitCursor = false;
                }
            }
            MarkAsSaved();
        }

        public void ExportWords() {
            bool retval = Base.ExportWords(EditWordList, InGame);
            if (!InGame && retval) {
                EditWordListFilename = EditWordList.ResFile;
                MarkAsSaved();
            };
        }

        public void EditProperties() {
            string strDesc = EditWordList.Description;
            string id = "";
            if (GetNewResID(AGIResType.Words, -1, ref id, ref strDesc, InGame, 2)) {
                EditWordList.Description = strDesc;
                MDIMain.RefreshPropertyGrid(AGIResType.Words, 0);
            }
        }

        private bool AskClose() {
            if (EditWordList.ErrLevel < 0) {
                // if exiting due to error on form load
                return true;
            }
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to this WORDS.TOK file?",
                    "Save WORDS.TOK",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SaveWords();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "WORDS.TOK file not saved. Continue closing anyway?",
                            "Save WORDS.TOK",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        return rtn == DialogResult.Yes;
                    }
                    break;
                case DialogResult.Cancel:
                    return false;
                case DialogResult.No:
                    break;
                }
            }
            return true;
        }

        void MarkAsChanged() {
            // ignore when loading (not visible yet)
            if (!Visible) {
                return;
            }
            if (!IsChanged) {
                IsChanged = true;
                mnuRSave.Enabled = true;
                MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = true;
                Text = sDM + Text;
            }
            statusStrip1.Items["spGroupCount"].Text = "Group Count: " + EditWordList.GroupCount;
            statusStrip1.Items["spWordCount"].Text = "Word Count: " + EditWordList.WordCount;
        }

        private void MarkAsSaved() {
            IsChanged = false;
            Text = "Words Editor - ";
            if (InGame) {
                Text += EditGame.GameID;
            }
            else {
                Text += Common.Base.CompactPath(EditWordListFilename, 75);
            }
            mnuRSave.Enabled = false;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = false;
        }
    }

    public class WordsUndo {
        public ActionType Action;
        public string[] Group;
        public int GroupNo;
        public int OldGroupNo;
        public string Word;
        public string OldWord;
        public string Description;


        public enum ActionType {
            AddGroup,   // store group number that was added
            DelGroup,   // store group number AND group object that was deleted
            Renumber,   // store old group number AND new group number
            AddWord,    // store group number AND new word that was added
            DelWord,    // store group number AND old word that was deleted
            MoveWord,   // store old group number, new group number and word that was moved
            ChangeWord, // store old word and new word
            CutWord,    // same as delete
            CutGroup,   // same as delete
            PasteWord,  // store group number and old word that was pasted over
            PasteGroup, // store group number and list of old words that were pasted over
            Replace,    // same as change word
            ReplaceAll, // store list of all words changed
            Clear,      // store all words and their groups
        }

        public WordsUndo() {
            Group = Array.Empty<string>();
        }
    }
}
