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

namespace WinAGI.Editor {
    public partial class frmWordsEdit : Form {
        public bool InGame;
        public bool IsChanged;
        public WordList EditWordList;
        private bool closing = false;
        private string EditWordListFilename;
        private bool blnRefreshLogics = false;

        public frmWordsEdit() {
            InitializeComponent();
            InitFonts();
            MdiParent = MDIMain;
        }

        #region Form Event Handlers
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
        #endregion

        #region temp code
        private void btnClear_Click(object sender, EventArgs e) {
            EditWordList.Clear();
            EditWordList.AddGroup(0);
            EditWordList.AddGroup(1);
            EditWordList.AddGroup(9999);
            EditWordList.AddWord("a", 0);
            EditWordList.AddWord("anyword", 1);
            EditWordList.AddWord("rol", 9999);
            lstGroups.Items.Clear();
            lstGroups.Items.Add("   0: a");
            lstGroups.Items.Add("   1: anyword");
            lstGroups.Items.Add("9999: rol");
            lstGroups.SelectedIndex = 0;
            lblGroupCount.Text = "Group Count: 3";
            lblWordCount.Text = "Word Count: 3";
            MainStatusBar.Items["spWordCount"].Text = "Word Count: 3";
            MarkAsChanged();
        }

        private void lstGroups_SelectedIndexChanged(object sender, EventArgs e) {
            lstWords.Items.Clear();
            if (lstGroups.SelectedIndex >=0) {
                string grp = ((string)lstGroups.SelectedItem)[..((string)lstGroups.SelectedItem).IndexOf(':')];
                int group = int.Parse(grp);
                foreach (string word in EditWordList.GroupN(group)) {
                    lstWords.Items.Add(word);
                }
            }
        }

        void wordsfrmcode() {
            /*

            Option Explicit

  Private UndoCol As Collection
  
  Private Mode As WTMode
  Private Enum WTMode
    wtByGroup
    wtByWord
  End Enum
  
  Private AddNewWord As Boolean
  Private AddNewGroup As Boolean
  
  Private SelMode As eSelMode
  Private SelGroup As Long
  Private EditOldWord As String
  Private DraggingWord As Boolean
  Private mX As Single, mY As Single
  Private PickChar As Boolean
  
  Private OldGroupNum As Long, OldGroupIndex As Long 'used for dragging words to another group
  
  Private lngRowHeight As Long
  Private SplitOffset As Long
  
  Private Enum eSelMode
    smNone
    smWord
    smGroup
  End Enum
  
  Private Const WE_MARGIN As Long = 5
  Private Const SPLIT_WIDTH = 4 'in pixels
  Private Const MIN_SPLIT_V = 160 'in pixels
  Private Const TBW = 49 'toolbar width + 2 pixels
  Private Const LGT = 21 'lstGroups.Top
  
  Private CalcWidth As Long, CalcHeight As Long
  Private Const MIN_HEIGHT = 120 '361
  Private Const MIN_WIDTH = 240 '380

  Public PrevWLBWndProc As Long
  Public PrevGLBWndProc As Long
  Public PrevGrpTBWndProc As Long
  
  Private blnRecurse As Boolean
  'YES refresh all logics:
  ' x deleting words/groups
  ' x renumber group
  ' x move word
  ' x edit word
  'NO don't refresh:
  '  adding new words
  '  adding new groups
  '  updating description
  Private blnRefreshLogics As Boolean
  
Private Function IndexByGrp(ByVal GrpNum As Long) As Long

  'returns the listbox index for GrpNum
  'calling function must make sure group exists
  ' only valid in wtByGroup mode
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  For i = 0 To lstGroups.ListCount
    If Val(lstGroups.List(i)) = GrpNum Then
      IndexByGrp = i
      Exit Function
    End If
  Next i
  
  'not found; return nothing
  IndexByGrp = -1
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Public Sub MenuClickHelp()
  
  On Error GoTo ErrHandler
  
  'help
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Words_Editor.htm"
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub Activate()
  'bridge method to call the form's Activate event method
  Form_Activate
End Sub

Private Sub FindWordInLogic(ByVal SearchWord As String, ByVal FindSynonyms As Boolean)

  'call findinlogic with current word
  'or word group
  
  'find synonym option currently does nothing
  
  On Error GoTo ErrHandler
  
  'reset logic search
  FindForm.ResetSearch
    
  'set global search values
  GFindText = QUOTECHAR & SearchWord & QUOTECHAR
  GFindDir = fdAll
  GMatchWord = True
  GMatchCase = False
  GLogFindLoc = flAll
  GFindSynonym = FindSynonyms
  GFindGrpNum = WordsEdit.Group(lstGroups.ListIndex).GroupNum
  SearchType = rtWords
  
  'set it to match desired search parameters
  With FindForm
    'set find dialog to find textinlogic mode
    .SetForm ffFindWordsLogic, True
    
    'show the form
    .Show , frmMDIMain
  End With
  
  'ensure this form is the search form
  Set SearchForm = Me
  
Exit Sub

ErrHandler:

  '*'Debug.Assert False
  Resume Next
End Sub

''Private Sub AutoUpdate()
''  'build array of changed words
''  'then step through all logics; find
''  'all commands with word arguments;
''  'compare arguments against changed word list and
''  'make changes in sourcecode as necessary, then save the source
''
''  Dim tmpLogic As AGILogic, i As Long, j As Long
''  Dim strOld() As String, strNew() As String
''  Dim blnUnloadRes As Boolean
''  Dim lngPos1 As Long, lngPos2 As Long
''
''  'hide find form
''  FindForm.Visible = False
''
''  'show wait cursor
''  WaitCursor
''  'show progress form
''  frmProgress.Caption = "Updating Words in Logics"
''  frmProgress.lblProgress.Caption = "Searching..."
''  frmProgress.pgbStatus.Max = Logics.Count
''  frmProgress.pgbStatus.Value = 0
''  frmProgress.Show vbModeless, frmMDIMain
''  frmProgress.Refresh
''
''  'refresh
''  DoEvents
''
''  'use error trapping to find deleted groups
''  On Error Resume Next
''
''  For i = 0 To VocabularyWords.GroupCount - 1
''    'get group number
''    lngGrp = VocabularyWords.Group(i).GroupNum
''
''    'check if this group exists in new list
''    If WordsEdit.GroupN(lngGrp) Is Nothing Then
''      'group is deleted
''      blnDeleted = True
''      Err.Clear
''    'OR if group has no name
''    ElseIf LenB(WordsEdit.GroupN(lngGrp).GroupName) = 0 Then
''      blnDeleted = True
''    End If
''
''    'if an error,
''    If Err.Number <> 0 Then
''
''    End If
''
''    'if deleted
''    If blnDeleted Then
''      'add to update list
''      ReDim Preserve strOld(j)
''      ReDim Preserve strNew(j)
''      strOld(j) = QUOTECHAR & VocabularyWords.Group(i).GroupName & QUOTECHAR
''      strNew(j) = CStr(lngGroup)
''      j = j + 1
''    'if group name as changed
''    ElseIf WordsEdit.GroupN(lngGrp).GroupName <> VocabularyWords.GroupN(lngGrp).GroupName Then
''      'add to update list
''      ReDim Preserve strOld(j)
''      ReDim Preserve strNew(j)
''      'change to new word group name
''      strOld(j) = QUOTECHAR & VocabularyWords.GroupN(lngGrp).GroupName & QUOTECHAR
''      strNew(j) = QUOTECHAR & WordsEdit.GroupN(lngGrp).GroupName & QUOTECHAR
''      j = j + 1
''    End If
''
''    'reset deleted flag
''    blnDeleted = False
''  Next i
''
''  'step through all logics
''  For Each tmpLogic In Logics
''    'open if necessary
''    blnUnloadRes = Not tmpLogic.Loaded
''    If blnUnloadRes Then
''      tmpLogic.Load
''    End If
''
''    'step through code, looking for 'said' and 'word.to.string' commands
''    lngPos2 = FindNextToken(tmpLogic.SourceText, lngPos1, "said", True, True)
''
''    Do Until lngPos = 0
''      'said' syntax is : said(word1, word2, word3,...)
''      'words can be numeric word values OR string values
''    Loop
''
''    'unload if necessary
''    If blnUnloadRes Then
''      tmpLogic.Unload
''    End If
''  Next
''
''
''
''
''
''  'close the progress form
''  Unload frmProgress
''
''  're-enable form
''  frmMDIMain.Enabled = True
''  Screen.MousePointer = vbDefault
''  frmMDIMain.SetFocus
''End Sub
''
Public Sub InitFonts()
  
  On Error GoTo ErrHandler
  
  lstGroups.Font.Name = Settings.EFontName
  lstGroups.Font.Size = Settings.EFontSize
  lstGroups.Refresh
  
  lstWords.Font.Name = Settings.EFontName
  lstWords.Font.Size = Settings.EFontSize
  lstWords.Refresh
  
  txtGrpNum.Font.Name = Settings.EFontName
  txtGrpNum.Font.Size = Settings.EFontSize
  rtfWord.Font.Name = Settings.EFontName
  rtfWord.Font.Size = Settings.EFontSize
  
  'get height of items in listboxes
  lngRowHeight = SendMessage(lstGroups.hWnd, LB_GETITEMHEIGHT, 0, 0)
  
  'set height of grouptext box
  txtGrpNum.Height = lngRowHeight
  rtfWord.Height = lngRowHeight
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub MarkAsChanged()

  If Not IsChanged Then
    IsChanged = True
    
    'set caption
    Caption = sDM & Caption
    
    'enable menu and toolbar button
    frmMDIMain.mnuRSave.Enabled = True
    frmMDIMain.Toolbar1.Buttons("save").Enabled = True
  End If
End Sub
Public Sub NewWords()
  
  'creates a new word list file
  Dim i As Long
  On Error GoTo ErrHandler
  
  'set changed status and caption
  IsChanged = True
  WrdCount = WrdCount + 1
  Caption = sDM & "Words Editor - NewWords" & CStr(WrdCount)
  
  'clear filename
  WordsEdit.ResFile = vbNullString
  
  Select Case Mode
  Case wtByGroup
    'add word groups to listbox
    lstGroups.Clear
    For i = 0 To WordsEdit.GroupCount - 1
      'add group
      lstGroups.AddItem CStr(WordsEdit.Group(i).GroupNum) & " - " & UCase$(WordsEdit.Group(i).GroupName)
    Next i
    'select first group
    lstGroups.ListIndex = 0
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function NextGrpNum() As Long

  'just get next available number following current selection
  Dim lngTgtGrp As Long, i As Long

  On Error GoTo ErrHandler

  'search forward
  lngTgtGrp = Val(lstGroups.Text) + 1
  
  For i = lngTgtGrp To &HFFFF&
    If Not WordsEdit.GroupExists(i) Then
      NextGrpNum = i
      Exit Function
    End If
  Next i

  'if not found, try going backwards
  For i = lngTgtGrp - 1 To 0 Step -1
    If Not WordsEdit.GroupExists(i) Then
      NextGrpNum = i
      Exit Function
    End If
  Next i
  
  'if still not found, means user has 64K words! IMPOSSIBLE I SAY!
  NextGrpNum = -1
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub ReplaceAll(ByVal FindText As String, ByVal MatchWord As Boolean, ByVal ReplaceText As String)

  'replace all occurrences of FindText with ReplaceText
  
  Dim i As Long, j As Long
  Dim lngCount As Long
  Dim lngGroup As Long, lngOldGrp As Long
  Dim NextUndo As WordsUndo
  Dim strFindWord As String, strReplaceWord As String
  
  On Error GoTo ErrHandler
  
  'if replacing and new text is the same
  If StrComp(FindText, ReplaceText, vbTextCompare) = 0 Then
    'exit
    Exit Sub
  End If
  
  'blank replace text not allowed for words
  If LenB(ReplaceText) = 0 Then
    MsgBox "Blank replacement text is not allowed.", vbInformation + vbOKOnly, "Replace All"
    Exit Sub
  End If
  
  'validate replace word (no special characters, etc)
  'what characters are allowed for words????
  '####
  
  
  'if nothing in wordlist,
  If WordsEdit.WordCount = 0 Then
    MsgBox "Word list is empty.", vbOKOnly + vbInformation, "Replace All"
    Exit Sub
  End If
  
  'show wait cursor
  WaitCursor
  
  'create new undo object
  Set NextUndo = New WordsUndo
  NextUndo.UDAction = udwReplaceAll
  'use description property to indicate replace mode
  NextUndo.UDDescription = CStr(MatchWord)
  
  'use inline error trapping to
  'determine if words exist in group or not
  On Error Resume Next
  
  If MatchWord Then
    'does findtext exist?
    lngGroup = WordsEdit(FindText).Group
    
    'if no error, then searchword exists
    If Err.Number = 0 Then
      'i is group where replacement is occurring
      Err.Clear
      'add info to undo object
      NextUndo.UDGroupNo = lngGroup
      NextUndo.UDOldWord = FindText
      NextUndo.UDWord = ReplaceText
      
      'assume replacement word does not exist in a different group
      'validate the assumption by trying to get the groupnumber directly
      lngOldGrp = WordsEdit(ReplaceText).Group
      'if no error,
      If Err.Number = 0 Then
        'remove the replacement word
        WordsEdit.RemoveWord ReplaceText
      Else
        'not found; reset old group num
        lngOldGrp = -1
      End If
      Err.Clear
      
      'save oldgroup in undo object
      NextUndo.UDOldGroupNo = lngOldGrp
      
      'change word in this group by deleting findword
      WordsEdit.RemoveWord FindText
      'and adding replaceword
      WordsEdit.AddWord ReplaceText, lngGroup
      'ensure group lngGroup has correct name
      UpdateGroupName lngGroup
      'if word was removed from another group
      '(by checking if lngOldGrp is a valid group index)
      If lngOldGrp <> -1 Then
        'ensure group lngOldGrp has correct name
        UpdateGroupName lngOldGrp
      End If
      
      'update list boxes
      If Val(lstGroups.Text) = lngGroup Or (Val(lstGroups.Text) = lngOldGrp And lngOldGrp <> -1) Then
        UpdateWordList True
      End If
      'set Count to one
      lngCount = 1
    End If
  Else
    'need to step through all groups and all words
    'remove old words if the replace creates duplicates,
    'and create undo object that holds all this...
  
  
    'step through all groups
    For i = 0 To WordsEdit.GroupCount - 1
      'step through all words
      For j = 0 To WordsEdit.Group(i).WordCount - 1
        'need to manually check for end of group Count
        'because wordcount may change dynamically as words
        'are added and removed based on the changes
        If j > WordsEdit.Group(i).WordCount - 1 Then
          Exit For
        End If
        
        'if there is a match
        If InStr(1, WordsEdit.Group(i).Word(j), FindText, vbTextCompare) <> 0 Then
          strFindWord = WordsEdit.Group(i).Word(j)
          strReplaceWord = Replace(WordsEdit.Group(i).Word(j), FindText, ReplaceText)
          
          'i is the group INDEX not the group NUMBER;
          'get group number
          lngGroup = WordsEdit.Group(i).GroupNum
          
          'assume replacement word does not exist in another group
          'validate assumption by trying to get the groupnumber directly
          lngOldGrp = WordsEdit(strReplaceWord).Group
          'if no error,
          If Err.Number = 0 Then
            'remove the replacement word
            WordsEdit.RemoveWord strReplaceWord
          Else
            'not found; reset old group num
            lngOldGrp = -1
          End If
          Err.Clear
          
          'change word in this group by deleting findword
          WordsEdit.RemoveWord strFindWord
          'and adding replaceword
          WordsEdit.AddWord strReplaceWord, lngGroup
          'ensure group i has correct name
          UpdateGroupName lngGroup
          
          'if word came from a different group
          If lngOldGrp <> -1 Then
            'update that groupname too
            UpdateGroupName lngOldGrp
          End If
          
          'add to undo
          If lngCount = 0 Then
            'add group, oldgroup, word, oldword
            NextUndo.UDWord = CStr(lngGroup) & "|" & CStr(lngOldGrp) & "|" & strFindWord & "|" & strReplaceWord
          Else
             NextUndo.UDWord = NextUndo.UDWord & "|" & CStr(lngGroup) & "|" & CStr(lngOldGrp) & "|" & strFindWord & "|" & strReplaceWord
          End If
          'increment counter
          lngCount = lngCount + 1
          'need to restart search at beginning of word
          'because changes in words will affect the
          'order;
          'set j to -1 so the Next j statement
          'resets it to 0
          j = -1
        End If
      Next j
    Next i
    
    'if something found,
    If lngCount <> 0 Then
      'force update
      UpdateWordList True
    End If
  End If
  
  'if nothing found,
  If lngCount = 0 Then
    MsgBox "Search text not found.", vbInformation, "Replace All"
  Else
    'add undo
    AddUndo NextUndo
    
    'show how many replacements made
    MsgBox "The specified region has been searched. " & CStr(lngCount) & " replacements were made.", vbInformation, "Replace All"
  End If
  
  'restore mousepointer
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub BeginFind()

  'each form has slightly different search parameters
  'and procedure; so each form will get what it needs
  'from the form, and update the global search parameters
  'as needed
  '
  'that's why each search form cheks for changes, and
  'sets the global values, instead of doing it once inside
  'the FindForm code
  
  'when searching for a word in a logic, the first time the
  'user presses the 'find' button, the SearchForm is the
  'Words Editor; so we need to send this search request
  'to the FindInLogics function; after that, if found,
  'the SearchForm will switch to the LogicEditor, and
  'future presses of the Find button will search logics
  'as expected
  
  With FindForm
    'if searching in logics
    If .FormFunction = ffFindWordsLogic Then
      'begin a logic search
      FindInLogic GFindText, fdAll, True, False, flAll, False, vbNullString
      Exit Sub
    End If
        
    Select Case .FormAction
    Case faFind
      FindInWords GFindText, GMatchWord, GFindDir
    Case faReplace
      FindInWords GFindText, GMatchWord, GFindDir, True, GReplaceText
    Case faReplaceAll
      ReplaceAll GFindText, GMatchWord, GReplaceText
    Case faCancel
      'don't do anything
    End Select
  End With
End Sub

Public Sub MenuClickDelete()
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  Select Case SelMode
  Case smWord
    If lstWords.ListIndex <> -1 Then
      'RARE, but check for reserved groups
      If WordsEdit.Group(lstGroups.ListIndex).GroupNum = 1 Then
        If MsgBoxEx("Group '1' is a reserved group. Deleting its placeholder is not" & vbCrLf & "advised. See Help file for a detailed explanation." & vbCrLf & vbCrLf & "Are you sure you want to delete it?", vbExclamation + vbYesNo + vbMsgBoxHelpButton, "Deleting Reserved Group Placeholder", WinAGIHelp, "htm\winagi\Words_Editor.htm#reserved") = vbNo Then
          Exit Sub
        End If
      ElseIf WordsEdit.Group(lstGroups.ListIndex).GroupNum = 9999 Then
        'not recommended
        If MsgBoxEx("Group '9999' is a reserved group. Deleting its placeholder is not" & vbCrLf & "advised. See Help file for a detailed explanation." & vbCrLf & vbCrLf & "Are you sure you want to delete it?", vbExclamation + vbYesNo + vbMsgBoxHelpButton, "Deleting Reserved Group Placeholder", WinAGIHelp, "htm\winagi\Words_Editor.htm#reserved") = vbNo Then
          Exit Sub
        End If
      End If
      
      DelWord CurrentWord()
    End If
    
  Case smGroup
    If lstGroups.ListIndex <> -1 Then
      i = lstGroups.ListIndex
      DelGroup Val(lstGroups.Text)
      'select next group
      If i < lstGroups.ListCount Then
        lstGroups.ListIndex = i
      Else
        lstGroups.ListIndex = lstGroups.ListCount - 1
      End If
    End If
    
  End Select
  
  Select Case Mode
  Case wtByGroup
    UpdateWordList True
  Case wtByWord
  End Select
  
  SetEditMenu
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickECustom1()

  'edit a word
  Dim tmpWidth As Single, tmpGroup As Long, tmpWord As Long
  
  On Error GoTo ErrHandler
  
  'if no group selected,
  If lstGroups.ListIndex = -1 Then
    Exit Sub
  End If
  
  tmpGroup = Val(lstGroups.Text)
  tmpWord = lstWords.ListIndex
  
  'if no word selected,
  If tmpWord = -1 Then
    Exit Sub
  End If
  
  If tmpGroup = 9999 Then
    'not recommended
    If MsgBoxEx("Group '9999' is a reserved group. Changing its placeholder is" & vbCrLf & "not advised. See Help file for a detailed explanation." & vbCrLf & vbCrLf & "Are you sure you want to change it?", vbExclamation + vbYesNo + vbMsgBoxHelpButton, "Editing a Reserved Group", WinAGIHelp, "htm\winagi\Words_Editor.htm#reserved") = vbNo Then
      Exit Sub
    End If
  End If
  If tmpGroup = 1 Then
    'not recommended
    If MsgBoxEx("Group '1' is a reserved group. Changing its placeholder is" & vbCrLf & "not advised. See Help file for a detailed explanation." & vbCrLf & vbCrLf & "Are you sure you want to change it?", vbExclamation + vbYesNo + vbMsgBoxHelpButton, "Editing a Reserved Group", WinAGIHelp, "htm\winagi\Words_Editor.htm#reserved") = vbNo Then
      Exit Sub
    End If
  End If
  
  'save word being edited
  EditOldWord = WordsEdit.GroupN(tmpGroup).Word(tmpWord)
  
  If (lstWords.ListCount * lngRowHeight + 4 > lstWords.Height) Then
    tmpWidth = lstWords.Width - 22
  Else
    tmpWidth = lstWords.Width - 5
  End If
  
  'begin edit of word
  With rtfWord
    'move textbox
    .Move lstWords.Left + 3, (lstWords.ListIndex - lstWords.TopIndex) * lngRowHeight + lstWords.Top + 2, tmpWidth, lngRowHeight
    'copy word to text box and select entire word
    .Text = ""
    .TextB = EditOldWord
    .Selection.Range.StartPos = 0
    .Selection.Range.EndPos = .Range.Length
    'show textbox
    .Visible = True
    'set focus to the textbox
    .SetFocus
  End With
  
  'disable edit menu
  frmMDIMain.mnuEdit.Enabled = False
  ' and toolbar editing buttons
  With Me.Toolbar1.Buttons
    .Item(1).Enabled = False
    .Item(2).Enabled = False
    .Item(3).Enabled = False
    .Item(4).Enabled = False
    .Item(6).Enabled = False
    .Item(7).Enabled = False
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickECustom2()

  'if a word is selected
  If lstWords.ListIndex <> -1 Then
    'use this word; assume not looking for synonyms
    FindWordInLogic CPToUnicode(lstWords.Text, SessionCodePage), False
  'if a group is selected
  ElseIf lstGroups.ListIndex <> -1 Then
    'use groupname; assume looking for synonyms
    FindWordInLogic CPToUnicode(WordsEdit.Group(lstGroups.ListIndex).GroupName, SessionCodePage), True
  End If
End Sub

Public Sub MenuClickECustom3()

  'toggle between edit mode (bygroup) or list mode (by word)
  Dim i As Long
  Dim oldGrp As Long, OldWord As String
  
  On Error GoTo ErrHandler
  
  Mode = 1 - Mode '1-0=1  1-1=0
  Select Case Mode
  Case wtByGroup
    Toolbar1.Buttons("mode").Image = 10
    lblGroups.Caption = "Groups"
    'save current selection
    oldGrp = Val(lstGroups.Text)
    OldWord = lstWords.Text
    
    'refresh grouplist
    RebuildGroupList oldGrp, False ' WordsEdit(0).Group, False
    'force update to wordlist
    UpdateWordList True
    If Len(OldWord) > 0 Then
      lstWords.Text = OldWord
    End If
    
  Case wtByWord
    Toolbar1.Buttons("mode").Image = 11
    lblGroups.Caption = "Group"
    'save current selection
    oldGrp = Val(lstGroups.Text)
    OldWord = lstWords.Text
    
    lstGroups.Clear
    lstGroups.AddItem ""
    lstWords.Clear
    For i = 0 To WordsEdit.WordCount - 1
      lstWords.AddItem WordsEdit(i).WordText
    Next i
    lstGroups.ListIndex = 0
    If Len(OldWord) > 0 Then
      lstWords.Text = OldWord
    Else
      lstWords.Text = WordsEdit.GroupN(oldGrp).GroupName
    End If
  End Select
  SetEditMenu
Exit Sub

ErrHandler:
  Resume Next
End Sub
Private Sub NewGroup()

  'inserts a new group, with next available group number
  'then, a new blank word is added
    
  On Error GoTo ErrHandler
  
  'VERY RARE, but make sure listbox is NOT full
  If WordsEdit.GroupCount = &H7FFF Then
    'just ignore
    Exit Sub
  End If
  
  'add the group
  AddGroup NextGrpNum()
  AddNewGroup = True
  
  'add a new word (but without undo)
  NewWord True
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub UpdateGroupName(ByVal GroupNo As Long)

  'updates the group list for the correct name
  ' only used in ByGroup mode
  Dim i As Long
  
  If Mode <> wtByGroup Then
    Exit Sub
  End If
  
  'groups 0, 1, and 9999 never change group name
  If GroupNo = 0 Or GroupNo = 1 Or GroupNo = 9999 Then
    Exit Sub
  End If
  
  'should never happen but...
  If Not WordsEdit.GroupExists(GroupNo) Then
    Exit Sub
  End If
  
  'find the group
  For i = 0 To lstGroups.ListCount - 1
    If Val(lstGroups.List(i)) = GroupNo Then
      'update group name
      lstGroups.List(i) = CStr(GroupNo) & " - " & UCase$(CPToUnicode(WordsEdit.GroupN(GroupNo).GroupName, SessionCodePage))
      Exit For
    End If
  Next i
End Sub
Private Sub UpdateStatusBar()

  MainStatusBar.Panels("GroupCount").Text = "Total Groups: " & CStr(WordsEdit.GroupCount)
  MainStatusBar.Panels("WordCount").Text = "Total Words: " & CStr(WordsEdit.WordCount)
End Sub

Private Sub UpdateWordList(Optional ByVal ForceUpdate As Boolean = False)

  'only used in ByGroup mode?
  If Mode <> wtByGroup Then
    Debug.Assert False
    Exit Sub
  End If
  
  Dim lngGrpNum As Long
  Dim i As Long
  Dim blnAdd As Boolean
  
  On Error GoTo ErrHandler
  
  'if dragging a word,
  If DraggingWord Then
    'dont update no matter what
    Exit Sub
  End If
  
  lngGrpNum = Val(lstGroups.Text)
  
#If DEBUGMODE <> 1 Then
  'disable window painting for the listbox until done
  SendMessage lstWords.hWnd, WM_SETREDRAW, 0, 0
#End If

  'if change in group
  If lngGrpNum <> SelGroup Or ForceUpdate Then
    'load the words for this group into the word listbox
    'clear the word list
    lstWords.Clear
    'set default wordlistbox properties
    lstWords.FontBold = False
    lstWords.ForeColor = vbBlack
    lstWords.Enabled = True
    
    'rare, but groups 0, 1, 9999 may not exist in the wordlist even though
    'they are always present in the form's listbox; so check that the
    'group exists before counting words
    If WordsEdit.GroupExists(lngGrpNum) Then
      'add all the words in the group
      For i = 0 To WordsEdit.GroupN(lngGrpNum).WordCount - 1
        lstWords.AddItem CPToUnicode(WordsEdit.GroupN(lngGrpNum).Word(i), SessionCodePage)
      Next i
    End If
    
    'if a reserved group is selected
    If lngGrpNum = 0 Then
      'if doesn't exist, or if no words actually in the group
      If Not WordsEdit.GroupExists(1) Then
        blnAdd = True
      ElseIf WordsEdit.GroupN(1).WordCount = 0 Then
        blnAdd = True
      End If
      
    ElseIf lngGrpNum = 1 Then
      'if doesn't exist, or if no words actually in the group
      If Not WordsEdit.GroupExists(1) Then
        blnAdd = True
      ElseIf WordsEdit.GroupN(1).WordCount = 0 Then
        blnAdd = True
      End If
      If blnAdd Then
        'add uneditable placeholder
        lstWords.AddItem "<group 1: any word>"
      End If
      'always mark it as 'special'
      lstWords.ForeColor = &HC0C0C0
      lstWords.FontBold = True
      
    ElseIf lngGrpNum = 9999 Then
      'if doesn't exist, or if no words actually in the group
      If Not WordsEdit.GroupExists(9999) Then
        blnAdd = True
      ElseIf WordsEdit.GroupN(9999).WordCount = 0 Then
        blnAdd = True
      End If
      If blnAdd Then
        'add uneditable placeholder
        lstWords.AddItem "<group 9999: rest of line>"
      End If
      'always mark it as special
      lstWords.ForeColor = &HC0C0C0
      lstWords.FontBold = True
    End If
  End If
  
#If DEBUGMODE <> 1 Then
  SendMessage lstWords.hWnd, WM_SETREDRAW, 1, 0
#End If

  lstWords.Refresh
  
  SelGroup = lngGrpNum
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function ValidateGrpNum() As Boolean

  On Error GoTo ErrHandler
  
  Dim lngNewGrpNum As Long
  
  'assume OK
  ValidateGrpNum = True
  
  lngNewGrpNum = CLng((txtGrpNum.Text))
  
  'if too big
  If lngNewGrpNum > 65535 Then
    MsgBoxEx "Invalid group number. Must be less than 65536.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Renumber Group Error", WinAGIHelp, "htm\agi\words.htm#16bit"
    ValidateGrpNum = False
    Exit Function
  End If
    
  'if group number has changed
  If lngNewGrpNum <> CLng(Val(lstGroups.Text)) Then
    'if the new number is already in use
    If WordsEdit.GroupExists(lngNewGrpNum) Then
      'not a valid new group number
      MsgBox "Group " & txtGrpNum.Text & " is already in use. Choose another number.", vbInformation + vbOKOnly, "Renumber Group Error"
      ValidateGrpNum = False
    End If
    
    'if new number is not ok, reset
    If Not ValidateGrpNum Then
      txtGrpNum.Text = CStr(Val(lstGroups.Text))
      txtGrpNum.SelStart = 0
      txtGrpNum.SelLength = Len(txtGrpNum.Text)
    Else
      'ok; make the change
      'renumber this group
      RenumberGroup CStr(Val(lstGroups.Text)), lngNewGrpNum
    End If
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Function ValidateWord(ByVal CheckWord As String) As Boolean

  Dim i As Long
  Dim strMsg As String
  Dim tmpGroup As Long
  
  On Error GoTo ErrHandler
  
  'assume OK
  ValidateWord = True
  
  If Len(CheckWord) = 0 Then
    'ok; it will be deleted
    Exit Function
  End If
  
  ' is it the same as current word (i.e no change made)
  If CheckWord = CurrentWord() Then
    'nothing to do
    Exit Function
  End If
  
  'need to check for invalid characters
  For i = 1 To Len(CheckWord)
    Select Case Asc(Mid$(CheckWord, i))
    Case 97 To 122
      'a-z are ok
      
    Case 33, 34, 39, 40, 41, 44, 45, 46, 58, 59, 63, 91, 93, 96, 123, 125
      '    !'(),-.:;?[]`{}
      'NEVER allowed; these values get removed by the input function
      strMsg = "'" & Mid$(CheckWord, i, 1) & "' is not an allowed character in AGI words."
      ValidateWord = False
      Exit For
      
    Case 32
      'NOT allowed as first char
      If i = 1 Then
        strMsg = "'" & Mid$(CheckWord, i, 1) & "' is not allowed as first character of an AGI word."
        ValidateWord = False
        Exit For
      End If
    
    Case 35 To 38, 42, 43, 47 To 57, 60 To 62, 64, 92, 94, 95, 124, 126, 127
      'these characters:
      '    #$%&*+/0123456789<=>@\^_|~
      'NOT allowed as first char
      If i = 1 Then
        'UNLESS supporting the Power Pack mod
        If Not PowerPack Then
          ValidateWord = False
          strMsg = "'" & Mid$(CheckWord, i, 1) & "' is not allowed as first character of an AGI word."
          Exit For
        End If
      End If
      
    Case Else
      'extended chars not allowed
      'UNLESS supporting the Power Pack mod
      If Not PowerPack Then
        ValidateWord = False
        strMsg = "'" & Mid$(CheckWord, i, 1) & "' is not an allowed character in AGI words."
        Exit For
      End If
    End Select
  Next i
  
  'if invalid
  If Not ValidateWord Then
    MsgBoxEx strMsg, vbOKOnly + vbCritical + vbMsgBoxHelpButton, "Invalid Character in Word", WinAGIHelp, "htm\words.htm#charlimits"
    Exit Function
  End If
  
  'does this already word exist?
  tmpGroup = Val(lstGroups.Text)
  
  With WordsEdit.GroupN(tmpGroup)
    For i = 0 To .WordCount - 1
      If .Word(i) = CheckWord Then
        'only a concern if it's in same group (i.e. trying to add
        'a duplicate word to same group)
        ' don't need to check if word is in a different group- EditWord
        'handles that case
        MsgBox "The word '" & CheckWord & "' already exists in this group.", vbInformation + vbOKOnly, "Duplicate Word"
        ValidateWord = False
        rtfWord.Selection.Range.StartPos = 0
        rtfWord.Selection.Range.EndPos = Len(CheckWord)
        Exit Function
      End If
    Next i
  End With
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Function CheckWordFormat(ByRef ThisWord As String) As Boolean

  Dim i As Long
  
  On Error GoTo ErrHandler
  
  If Len(ThisWord) = 0 Then
    'not valid format
    CheckWordFormat = False
    Exit Function
  End If
  
  ThisWord = LCase$(ThisWord)
  
  ' check for invalid characters
  For i = 1 To Len(ThisWord)
    Select Case Asc(Mid$(ThisWord, i))
    Case 97 To 122
      'a-z are ok
      
    Case 33, 34, 39, 40, 41, 44, 45, 46, 58, 59, 63, 91, 93, 96, 123, 125
      '    !'(),-.:;?[]`{}
      'NEVER allowed; these values get removed by the input function
      CheckWordFormat = False
      Exit Function
      
    Case 32
      'NOT allowed as first char
      If i = 1 Then
        CheckWordFormat = False
        Exit Function
      End If
      
    Case 35 To 38, 42, 43, 47 To 57, 60 To 62, 64, 92, 94, 95, 124, 126, 127
      'these characters:
      '    #$%&*+/0123456789<=>@\^_|~
      'NOT allowed as first char
      If i = 1 Then
        'UNLESS supporting the Power Pack mod
        If Not PowerPack Then
          CheckWordFormat = False
          Exit Function
        End If
      End If
      
    Case Else
      'extended chars not allowed
      'UNLESS supporting the Power Pack mod
      If Not PowerPack Then
        CheckWordFormat = False
        Exit Function
      End If
    End Select
  Next i
  
  'word is OK
  CheckWordFormat = True
  
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function


Private Sub Form_GotFocus()

'*'Debug.Print "words got focus"
End Sub

Private Sub lstGroups_Click()

  On Error GoTo ErrHandler
  
  'not used in ByWord mode
  If Mode = wtByWord Then
    Exit Sub
  End If
  
  'on startup, controls are not visible, so can't get focus
  If lstGroups.Visible Then
    'ensure lstGroups has the focus
    lstGroups.SetFocus
  End If
  
  If SelMode <> smGroup Then
    'reset mode
    SelMode = smGroup
    
    'if not dragging
    If Not DraggingWord Then
      'deselect word
      lstWords.ListIndex = -1
    End If
    
    If lstGroups.Visible Then
      lstGroups.SetFocus
    End If
  End If
  SetEditMenu
  
  'update word list if necessary
  UpdateWordList
Exit Sub

ErrHandler:
  
End Sub

Private Sub lstGroups_DblClick()

  'edit the selected group's number
  'BUT dont allow group 1 or group 9999 to be edited
  Dim tmpWidth As Single
  
  On Error GoTo ErrHandler
  
  Select Case Mode
  Case wtByGroup
    'if no group selected,
    If lstGroups.ListIndex = -1 Then
      Exit Sub
    End If
    
    If Val(lstGroups.Text) = 0 Then
      'not allowed
      MsgBoxEx "Group '0' is a reserved group that can't be deleted or renumbered.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Renumber Group Error", WinAGIHelp, "htm\agi\words.htm#reserved"
      Exit Sub
    End If
    If Val(lstGroups.Text) = 1 Then
      'not allowed
      MsgBoxEx "Group '1' is a reserved group that can't be deleted or renumbered", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Renumber Group Error", WinAGIHelp, "htm\agi\words.htm#reserved"
      Exit Sub
    End If
    If Val(lstGroups.Text) = 9999 Then
      'not allowed
      MsgBoxEx "Group '9999' is a reserved group that can't be deleted or renumbered.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Renumber Group Error", WinAGIHelp, "htm\agi\words.htm#reserved"
      Exit Sub
    End If
      
    If (lstGroups.ListCount * lngRowHeight + 4 > lstGroups.Height) Then
      tmpWidth = lstGroups.Width - 22
    Else
      tmpWidth = lstGroups.Width - 5
    End If
    
    'begin edit of group number
    With txtGrpNum
      'move textbox
      .Move lstGroups.Left + 4, (lstGroups.ListIndex - lstGroups.TopIndex) * lngRowHeight + lstGroups.Top + 2, tmpWidth, lngRowHeight
      'copy groupnum to text box
      .Text = CStr(Val(lstGroups.Text))
      'select entire word
      .SelStart = 0
      .SelLength = Len(.Text)
      'show textbox
      .Visible = True
      'set focus to the textbox
      .SetFocus
    End With
    
    'disable edit menu
    frmMDIMain.mnuEdit.Enabled = False
    ' and toolbar editing buttons
    With Me.Toolbar1.Buttons
      .Item(1).Enabled = False
      .Item(2).Enabled = False
      .Item(3).Enabled = False
      .Item(4).Enabled = False
      .Item(6).Enabled = False
      .Item(7).Enabled = False
    End With
  
  Case wtByWord
    'no action
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub lstGroups_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim lngIndex As Long
  
  'if clicking with right button
  If Button = vbRightButton Then
    'select the item that the cursor is over
    lngIndex = (Y / ScreenTWIPSY \ lngRowHeight) + lstGroups.TopIndex
  
    'if something clicked,
    If lngIndex <= lstGroups.ListCount - 1 Then
      'select the item clicked
      lstGroups.ListIndex = lngIndex
      
      'upate wordlist if necessary
      UpdateWordList
      'if a specific word was selected,
      'deselect it
      If lstWords.ListIndex >= 0 Then
        lstWords.ListIndex = -1
      End If
      
      'if mode is not group
      If SelMode <> smGroup Then
        SelMode = smGroup
      End If
      SetEditMenu
    End If
    
    'make sure this form is the active form
    If Not (frmMDIMain.ActiveMdiChild Is Me) Then
      'set focus before showing the menu
      Me.SetFocus
    End If
    'need doevents so form activation occurs BEFORE popup
    'otherwise, errors will be generated because of menu
    'adjustments that are made in the form_activate event
    SafeDoEvents
    'show edit menu
    PopupMenu frmMDIMain.mnuEdit, , X / ScreenTWIPSX, Y / ScreenTWIPSY + 10
  End If
  
  
End Sub

Private Sub lstGroups_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  'in unlikely case where drag or drop functions get off-kilter, reset them here
  If DraggingWord Then
    DraggingWord = False
  End If
End Sub

Private Sub lstGroups_OLEDragDrop(Data As DataObject, Effect As Long, Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim lngIndex As Long
  
  On Error GoTo ErrHandler
  
  'only allow drop if a word drag is
  'in progress
  If Not DraggingWord Then
    Exit Sub
  End If
  
  'select the item that the cursor is over
  lngIndex = (Y / ScreenTWIPSY \ lngRowHeight) + lstGroups.TopIndex
  
  'if over an item
  If lngIndex <= lstGroups.ListCount - 1 Then
    'not groups 1 or 9999
    'word groups 1 and 9999 are special, and can't be modified
    If Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 Then
      Effect = vbDropEffectNone
      Exit Sub
    Else
'      Effect = vbDropEffectMove
    End If
  
    'move this word
    MoveWord lstWords.ListIndex, OldGroupNum, Val(lstGroups.List(lngIndex))
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub lstGroups_OLEDragOver(Data As DataObject, Effect As Long, Button As Integer, Shift As Integer, X As Single, Y As Single, State As Integer)

  Dim lngIndex As Long
  
  On Error GoTo ErrHandler
  
  If Not DraggingWord Then
    Effect = vbDropEffectNone
    Exit Sub
  End If
  
  'select the item that the cursor is over
  lngIndex = (Y / ScreenTWIPSY \ lngRowHeight) + lstGroups.TopIndex
  
  'if over an item
  If lngIndex <= lstGroups.ListCount - 1 Then
    'select it
    lstGroups.ListIndex = lngIndex
  End If

  'word groups 1 and 9999 are special, and can't be modified
  If Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 Then
    Effect = vbDropEffectNone
  Else
   ' Effect = vbDropEffectMove
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub lstWords_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Select Case Mode
  Case wtByGroup
    'if holding down left button,
    If Button = vbLeftButton Then
      'if mouse has actually moved,
      If X <> mX Or Y <> mY Then
        'no dragging of group 1 or 9999
        If Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 Then
          Exit Sub
        End If
        
        'if not already dragging,
        If Not DraggingWord Then
          'set mode to auto
          lstWords.OLEDragMode = 1
          'begin drag
          lstWords.OLEDrag
          'reset mode to manual to prevent second instance of dragging
          lstWords.OLEDragMode = 0
        End If
      End If
    End If
  Case wtByWord
  End Select
End Sub

Private Sub lstWords_OLECompleteDrag(Effect As Long)

  Select Case Mode
  Case wtByGroup
    'if not droppable,
    If Effect = vbDropEffectNone Then
      lstGroups.ListIndex = OldGroupIndex
    End If
  Case wtByWord
  End Select
  
  'reset dragging flags
  DraggingWord = False
  DroppingWord = False
End Sub

Private Sub lstWords_OLEDragDrop(Data As DataObject, Effect As Long, Button As Integer, Shift As Integer, X As Single, Y As Single)

  '*'Debug.Print "drag-drop", Effect
End Sub

Private Sub lstWords_OLEDragOver(Data As DataObject, Effect As Long, Button As Integer, Shift As Integer, X As Single, Y As Single, State As Integer)

    Effect = vbDropEffectNone
End Sub

Private Sub lstWords_OLEGiveFeedback(Effect As Long, DefaultCursors As Boolean)

  'if not droppable,
  If Effect = vbDropEffectNone Then
  '  lstGroups.ListIndex = OldGroupIndex
  End If
  
End Sub

Private Sub lstWords_OLEStartDrag(Data As DataObject, AllowedEffects As Long)

  Select Case Mode
  Case wtByGroup
    'set internal drag flag (so this editor knows
    'a word is being dragged)
    DraggingWord = True
    'set global drop flag (so logics (or other text receivers) know
    'when a word is being dropped
    DroppingWord = True
    
    'set allowed effects to move (so word will move from this
    'group into its new group
    AllowedEffects = vbDropEffectMove
    
    'track the original group index and group number
    OldGroupNum = Val(lstGroups.Text)
    OldGroupIndex = lstGroups.ListIndex
    
  Case wtByWord
    'set global drop flag (so logics (or other text receivers) know
    'when a word is being dropped
    DroppingWord = True
  End Select
End Sub

Private Sub picSplit_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  'begin split operation
  picSplitIcon.Height = picSplit.Height
  picSplitIcon.Move picSplit.Left, picSplit.Top
  picSplitIcon.Visible = True
  
  'save offset
  SplitOffset = picSplit.Left - X
End Sub


Private Sub picSplit_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim Pos As Single
  
  'if splitting
  If picSplitIcon.Visible Then
    Pos = X + SplitOffset
    
    'limit movement
    If Pos < MIN_SPLIT_V Then
      Pos = MIN_SPLIT_V
    ElseIf Pos > ScaleWidth - MIN_SPLIT_V Then
      Pos = ScaleWidth - MIN_SPLIT_V
    End If
    
    'move splitter
    picSplitIcon.Left = Pos
  End If
End Sub



Private Sub picSplit_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim Pos As Single
  
  'if splitting
  If picSplitIcon.Visible Then
    'stop splitting
    picSplitIcon.Visible = False
  
    Pos = X + SplitOffset
    
    'limit movement
    If Pos < MIN_SPLIT_V Then
      Pos = MIN_SPLIT_V
    ElseIf Pos > ScaleWidth - MIN_SPLIT_V Then
      Pos = ScaleWidth - MIN_SPLIT_V
    End If
  End If
  
  'redraw!
  UpdatePanels Pos
End Sub


Public Sub MenuClickDescription(ByVal FirstProp As Long)

  'although it is never used, we need to use same form of this method as all other resources
  'otherwise, we'll get an error when the main form tries to call this method
  
  Dim NextUndo As WordsUndo
  Dim strDesc As String
  
  strDesc = WordsEdit.Description
  If GetNewResID(rtWords, -1, vbNullString, strDesc, InGame, 2) Then
    'create new undo object
    Set NextUndo = New WordsUndo
    NextUndo.UDAction = udwChangeDesc
    NextUndo.UDDescription = strDesc
    
    'add to undo
    AddUndo NextUndo
    
    'change the edit object
    WordsEdit.Description = strDesc
  End If
End Sub
Public Sub MenuClickReplace()

  'replace text
  'use menuclickfind in replace mode
  MenuClickFind ffReplaceWord
End Sub


Private Function AddGroup(ByVal NewGroupNo As Long, Optional ByVal DontUndo As Boolean = False)
  
  Dim NextUndo As WordsUndo
  Dim i As Integer
  
  On Error GoTo ErrHandler
  'adds a new group, and returns the index of the group in the list
  
  'add the group
  WordsEdit.AddGroup NewGroupNo
  
  Select Case Mode
  Case wtByGroup ' by group
    'add it to group list
    For i = 0 To lstGroups.ListCount - 1
      'if the new group is less than or equal to current group number
      If NewGroupNo <= Val(lstGroups.List(i)) Then
        'this is where to insert it
        Exit For
      End If
    Next i
    lstGroups.AddItem CStr(NewGroupNo) & " - ", i
  Case wtByWord
  End Select
  
  'if not skipping undo
  If Not DontUndo Then
    'create undo object
    Set NextUndo = New WordsUndo
    NextUndo.UDAction = udwAddGroup
    NextUndo.UDGroupNo = NewGroupNo
    
    'add undo item
    AddUndo NextUndo
    
    'select the group
    lstGroups.ListIndex = i
    
    'send back the index number (which, isn't guaranteed to be NewGroupNo!!!
    AddGroup = i
  End If
  
  'update status bar
  UpdateStatusBar
Exit Function

ErrHandler:
  Resume Next
End Function

Private Sub AddWord(ByVal GroupNo As Long, NewWord As String, Optional ByVal DontUndo As Boolean = False)

  Dim NextUndo As WordsUndo
  Dim i As Integer, strFirst As String
  
  'groups 0, 1, and 9999 never change groupname so no need to capture current groupname
  If GroupNo <> 0 And GroupNo <> 1 And GroupNo <> 9999 Then
    'any other group will always have at least one word
    strFirst = WordsEdit.GroupN(GroupNo).GroupName
  End If
  
  'add this word to current word group
  WordsEdit.AddWord NewWord, GroupNo
  
  'if groupname has changed,
  If WordsEdit.GroupN(GroupNo).GroupName <> strFirst Then
    UpdateGroupName GroupNo
  End If
  
  'if not skipping undo
  If Not DontUndo Then
    Select Case Mode
    Case wtByGroup
      'select this words group
      For i = 0 To lstGroups.ListCount - 1
        If Val(lstGroups.List(i)) = GroupNo Then
          lstGroups.ListIndex = i
          Exit For
        End If
      Next i
      
      'move cursor to new word
      lstWords.Text = CPToUnicode(NewWord, SessionCodePage)
    Case wtByWord
    End Select

    'create undo object
    Set NextUndo = New WordsUndo
    NextUndo.UDAction = udwAddWord
    NextUndo.UDGroupNo = WordsEdit(NewWord).Group
    NextUndo.UDOldGroupNo = NextUndo.UDGroupNo
    NextUndo.UDWord = NewWord
    
    'add undo item
    AddUndo NextUndo
  End If
  
  'update status bar
  UpdateStatusBar
End Sub


Private Sub DelGroup(OldGrpNum As Long, Optional ByVal DontUndo As Boolean = False)
  
  Dim NextUndo As WordsUndo
  Dim i As Long
  
  'if not skipping undo
  If Not DontUndo Then
    'create new undo object
    Set NextUndo = New WordsUndo
    NextUndo.UDAction = udwDelGroup
    'store group number
    NextUndo.UDGroupNo = OldGrpNum
    
    'copy words in old group to undo object
    Set NextUndo.UDGroup = New StringList
    For i = 0 To WordsEdit.GroupN(OldGrpNum).WordCount - 1
      NextUndo.UDGroup.Add WordsEdit.GroupN(OldGrpNum).Word(i)
    Next i
    'add to undo collection
    AddUndo NextUndo
  
    'force logics refresh
    blnRefreshLogics = True
  End If
  
  Select Case Mode
  Case wtByGroup
    'delete from listbox
    For i = 0 To WordsEdit.GroupCount - 1
      If WordsEdit.Group(i).GroupNum = OldGrpNum Then
        lstGroups.RemoveItem i
        'select next group
        If i < lstGroups.ListCount Then
          lstGroups.ListIndex = i
        Else
          lstGroups.ListIndex = lstGroups.ListCount - 1
        End If
        Exit For
      End If
    Next i
  
  Case wtByWord
  End Select
  
  'delete old group AFTER removing it from list
  WordsEdit.RemoveGroup OldGrpNum
  
  'update status bar
  UpdateStatusBar
End Sub

Private Sub DelWord(OldWord As String, Optional ByVal DontUndo As Boolean = False)

  Dim NextUndo As WordsUndo
  Dim lngOldGrpNo As Long
  Dim blnFirst As Boolean
  
  'get group number
  lngOldGrpNo = WordsEdit(OldWord).Group
  blnFirst = (WordsEdit.GroupN(lngOldGrpNo).GroupName = OldWord)
  
  'delete this word
  WordsEdit.RemoveWord OldWord
  
  'if no words left in this group
  If WordsEdit.GroupN(lngOldGrpNo).WordCount = 0 Then
    'RARE, but check for reserved groups
    If lngOldGrpNo <> 0 And lngOldGrpNo <> 1 And lngOldGrpNo <> 9999 Then
      'delete old group
      WordsEdit.RemoveGroup lngOldGrpNo
      
      Select Case Mode
      Case wtByGroup
        'delete from listbox
        lstGroups.RemoveItem IndexByGrp(lngOldGrpNo)
      Case wtByWord
      End Select
    End If
  
  'if the first word was deleted
  ElseIf blnFirst Then
    'reset group listbox entry for this group
    UpdateGroupName lngOldGrpNo
  End If
  
  'if not skipping undo
  If Not DontUndo Then
    'create new undo object
    Set NextUndo = New WordsUndo
    NextUndo.UDAction = udwDelWord
    'save word and group number
    NextUndo.UDGroupNo = lngOldGrpNo
    NextUndo.UDWord = OldWord
    'add to undo collection
    AddUndo NextUndo
  
    ' force logics refresh
    blnRefreshLogics = True
  End If
  
  'update status bar
  UpdateStatusBar
End Sub

Private Sub AddUndo(NextUndo As WordsUndo)

  '*'Debug.Assert Not (UndoCol Is Nothing)
  'adds the next undo object
  UndoCol.Add NextUndo
  
  'set undo menu
  frmMDIMain.mnuEUndo.Enabled = True
  frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(WORDSUNDOTEXT + NextUndo.UDAction) & vbTab & "Ctrl+Z"
  
  If Not IsChanged Then
    MarkAsChanged
  End If
  
  'update status bar
  UpdateStatusBar
End Sub

Public Sub MenuClickClear()
    
  'clear the list
  Clear False, True
End Sub

Public Sub MenuClickFind(Optional ByVal ffValue As FindFormFunction = ffFindWord)

  On Error GoTo ErrHandler
  
  'set form defaults
  With FindForm
    If Not .Visible Then
      If Len(GFindText) > 0 Then
        'if it has quotes, remove them
        If Asc(GFindText) = 34 Then
          GFindText = Right$(GFindText, Len(GFindText) - 1)
        End If
        If Right$(GFindText, 1) = QUOTECHAR Then
          GFindText = Left$(GFindText, Len(GFindText) - 1)
        End If
      End If
    End If
    
    'set find dialog to word mode
    .SetForm ffValue, False
    
    'show the form
    .Show , frmMDIMain
  
    'always highlight search text
    .rtfFindText.Selection.Range.StartPos = 0
    .rtfFindText.Selection.Range.EndPos = Len(.rtfFindText.Text)
    .rtfFindText.SetFocus
    
    'ensure this form is the search form
    Set SearchForm = Me
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub FindInWords(ByVal FindText As String, ByVal MatchWord As Boolean, ByVal FindDir As FindDirection, Optional ByVal Replacing As Boolean = False, Optional ByVal ReplaceText As String = vbNullString)

  Dim SearchWord As Long, FoundWord As Long
  Dim SearchGrp As Long, FoundGrp As Long
  Dim rtn As VbMsgBoxResult, strFullWord As String
  
  On Error GoTo ErrHandler
  
  Select Case Mode
  Case wtByGroup
    'if editor mode is none (no group/word selected)
    If SelMode = smNone Then
      Exit Sub
    End If
    
    'if replacing and new text is the same
    If Replacing And (StrComp(FindText, ReplaceText, vbTextCompare) = 0) Then
      'exit
      Exit Sub
    End If
    
    'blank replace text not allowed for words
    If Replacing And LenB(ReplaceText) = 0 Then
      MsgBox "Blank replacement text is not allowed.", vbInformation + vbOKOnly, "Replace in Word List"
      Exit Sub
    End If
    
    'if no words in the list
    If WordsEdit.WordCount = 0 Then
      MsgBox "There are no words in this list.", vbOKOnly + vbInformation, "Find in Word List"
      Exit Sub
    End If
    
    'show wait cursor
    WaitCursor
    
    'if replacing and searching up,  start at next word
    'if replacing and searching down start at current word
    'if not repl  and searching up   start at current word
    'if not repl  and searching down start at next word
    
    'set searchwd and searchgrp to current word
    SearchGrp = lstGroups.ListIndex
    SearchWord = lstWords.ListIndex
    'if no word selected, start with first word
    If SearchWord = -1 Then
      SearchWord = 0
    End If
    
    'adjust to next word per replace/direction selections
    If (Replacing And FindDir = fdUp) Or (Not Replacing And FindDir <> fdUp) Then
      'if at end;
      If SearchWord >= lstWords.ListCount - 1 Then
        'use first word
        SearchWord = 0
        'of next group
        SearchGrp = SearchGrp + 1
        If SearchGrp >= lstGroups.ListCount Then
          SearchGrp = 0
        End If
      Else
        SearchWord = SearchWord + 1
      End If
    Else
      'if already AT beginning of search, the replace function will mistakenly
      'think the find operation is complete and stop
      If Replacing And (SearchWord = StartWord And SearchGrp = StartGrp) Then
        'reset search
        FindForm.ResetSearch
      End If
    End If
    
    'main search loop
    Do
      'if direction is up
      If FindDir = fdUp Then
        'iterate backwards until word found or GrpFound=-1
        FoundWord = SearchWord - 1
        FoundGrp = SearchGrp
        'if at top of this group,
        'get the last word of previous group
        If FoundWord < 0 Then
          FoundGrp = FoundGrp - 1
          If FoundGrp <> -1 Then
            FoundWord = WordsEdit.Group(FoundGrp).WordCount - 1
          End If
        End If
        
        Do Until FoundGrp = -1
          'skip groups with no words
          If WordsEdit.Group(FoundGrp).WordCount <> 0 Then
            If MatchWord Then
              If StrComp(WordsEdit.Group(FoundGrp).Word(FoundWord), FindText, vbTextCompare) = 0 Then
                'found
                Exit Do
              End If
            Else
              If InStr(1, WordsEdit.Group(FoundGrp).Word(FoundWord), FindText, vbTextCompare) <> 0 Then
                'found
                Exit Do
              End If
            End If
          End If
          'decrement word
          FoundWord = FoundWord - 1
          If FoundWord < 0 Then
            FoundGrp = FoundGrp - 1
            If FoundGrp <> -1 Then
              FoundWord = WordsEdit.Group(FoundGrp).WordCount - 1
            End If
          End If
        Loop
        
        'reset search to last group/last word+1
        SearchGrp = WordsEdit.GroupCount - 1
        SearchWord = WordsEdit.Group(SearchGrp).WordCount ' - 1
      Else
        'iterate forward until word found or foundgrp=groupcount
        FoundWord = SearchWord
        FoundGrp = SearchGrp
        
        Do
          'skip groups with no words
          If WordsEdit.Group(FoundGrp).WordCount <> 0 Then
            If MatchWord Then
              If StrComp(WordsEdit.Group(FoundGrp).Word(FoundWord), UnicodeToCP(FindText, SessionCodePage), vbTextCompare) = 0 Then
                'found
                Exit Do
              End If
            Else
              If InStr(1, WordsEdit.Group(FoundGrp).Word(FoundWord), UnicodeToCP(FindText, SessionCodePage), vbTextCompare) <> 0 Then
                'found
                Exit Do
              End If
            End If
          End If
          'increment word
          FoundWord = FoundWord + 1
          If FoundWord >= WordsEdit.Group(FoundGrp).WordCount Then
            FoundWord = 0
            FoundGrp = FoundGrp + 1
          End If
          
        Loop Until FoundGrp = WordsEdit.GroupCount
        'reset search
        SearchGrp = 0
        SearchWord = 0
      End If
      
      'if found, group will be valid
      If FoundGrp >= 0 And FoundGrp < WordsEdit.GroupCount Then
        'if back at start (grp and word same as start)
        If FoundWord = StartWord And FoundGrp = StartGrp Then
          'rest found position so search will end
          FoundWord = -1
          FoundGrp = -1
        End If
        Exit Do
      End If
      
      'if not found, action depends on search mode
      Select Case FindDir
      Case fdUp
        'if not reset yet
        If Not RestartSearch Then
          'if recursing,
          If blnRecurse Then
            'just say no
            rtn = vbNo
          Else
            rtn = MsgBox("Beginning of search scope reached. Do you want to continue from the end?", vbQuestion + vbYesNo, "Find in Word List")
          End If
          If rtn = vbNo Then
            'reset search
            FindForm.ResetSearch
            Me.MousePointer = vbDefault
            Exit Sub
          End If
        Else
          'entire scope already searched; exit
          Exit Do
        End If
        
      Case fdDown
        'if not reset yet
        If Not RestartSearch Then
          'if recursing
          If blnRecurse Then
            'just say no
            rtn = vbNo
          Else
            rtn = MsgBox("End of search scope reached. Do you want to continue from the beginning?", vbQuestion + vbYesNo, "Find in Word List")
          End If
          If rtn = vbNo Then
            'reset search
            FindForm.ResetSearch
            Me.MousePointer = vbDefault
            Exit Sub
          End If
        Else
          'entire scope already searched; exit
          Exit Do
        End If
        
      Case fdAll
        If RestartSearch Then
          Exit Do
        End If
        
      End Select
      
      'reset search so when we get back to start, search will end
      RestartSearch = True
    
    'loop is exited by finding the searchtext or reaching end of search area
    Loop
    
    'if search string found
    If FoundGrp >= 0 And FoundGrp < WordsEdit.GroupCount Then
      'if this is first occurrence
      If Not FirstFind Then
        'save this position
        FirstFind = True
        StartWord = FoundWord
        StartGrp = FoundGrp
      End If
      
      'highlight Word
      lstGroups.ListIndex = FoundGrp
      lstWords.ListIndex = FoundWord
      
      'if replacing
      If Replacing Then
        'if not replacing entire word
        If Not MatchWord Then
          'calculate new findtext and replacetext
          ReplaceText = Replace(WordsEdit.Group(FoundGrp).Word(FoundWord), FindText, ReplaceText)
          strFullWord = WordsEdit.Group(FoundGrp).Word(FoundWord)
        Else
          strFullWord = FindText
        End If
        
        'now try to edit the word
        If EditWord(strFullWord, ReplaceText) Then
          'change undo
          UndoCol(UndoCol.Count).UDAction = udwReplace
          frmMDIMain.mnuEUndo.Caption = "&Undo Replace" & vbTab & "Ctrl+Z"
          'select the word
          UpdateWordList True
          Select Case Mode
          Case wtByGroup
            lstWords.Text = ReplaceText
          End Select
          'always reset search when replacing, because
          'word index almost always changes
          FindForm.ResetSearch
          
          'recurse the find method to get the next occurence
          blnRecurse = True
          FindInWords FindText, MatchWord, FindDir, False
          blnRecurse = False
        End If
      End If
    
    'if search string NOT found
    Else
      'if not recursing, show a msg
      If Not blnRecurse Then
        'if something was previously found
        If FirstFind Then
          'search complete; no new instances found
          MsgBox "The specified region has been searched.", vbInformation, "Find in Word List"
        Else
          'show not found msg
          MsgBox "Search text not found.", vbInformation, "Find in Word List"
        End If
      End If
      
      'reset search flags
      FindForm.ResetSearch
    End If
    
    'need to always make sure right form has focus; if finding a word
    'causes the group list to change, VB puts the wordeditor form in focus
    ' but we want focus to match the starting form
    If SearchStartDlg Then
      FindForm.SetFocus
    Else
      Me.SetFocus
    End If
    
    'reset cursor
    Screen.MousePointer = vbDefault
    
  Case wtByWord
    'edit mode is n/a, and never replacing
    
    'if no words in the list
    If WordsEdit.WordCount = 0 Then
      MsgBox "There are no words in this list.", vbOKOnly + vbInformation, "Find in Word List"
      Exit Sub
    End If
    
    'show wait cursor
    WaitCursor
    
    'if searching up   start at current word
    'if searching down start at next word
    
    'set searchwd to current word
    SearchWord = lstWords.ListIndex
    'if no word selected, start with first word
    If SearchWord = -1 Then
      SearchWord = 0
    End If
    
    'adjust to next word per direction selection
    If FindDir <> fdUp Then
      'if at end;
      If SearchWord >= lstWords.ListCount - 1 Then
        If FindDir = fdDown Then
          'done
          SearchWord = WordsEdit.WordCount
        Else
          'use first word
          SearchWord = 0
        End If
      Else
        SearchWord = SearchWord + 1
      End If
    End If
    
    'main search loop
    Do
      'if direction is up
      If FindDir = fdUp Then
        'iterate backwards until word found or back to start
        FoundWord = SearchWord - 1
        
        Do Until FoundWord < 0
          'search all words
          If MatchWord Then
            If StrComp(WordsEdit(FoundWord).WordText, FindText, vbTextCompare) = 0 Then
              'found
              Exit Do
            End If
          Else
            If InStr(1, WordsEdit(FoundWord).WordText, FindText, vbTextCompare) <> 0 Then
              'found
              Exit Do
            End If
          End If
          'decrement word
          FoundWord = FoundWord - 1
          If FoundWord < 0 Then
            Exit Do
          End If
        Loop
        
        'reset search
        SearchWord = WordsEdit.WordCount
      Else
        'iterate forward until word found or foundword=wordcount
        FoundWord = SearchWord
        
        Do Until FoundWord = WordsEdit.WordCount
          If MatchWord Then
            If StrComp(WordsEdit(FoundWord).WordText, UnicodeToCP(FindText, SessionCodePage), vbTextCompare) = 0 Then
              'found
              Exit Do
            End If
          Else
            If InStr(1, WordsEdit(FoundWord).WordText, UnicodeToCP(FindText, SessionCodePage), vbTextCompare) <> 0 Then
              'found
              Exit Do
            End If
          End If
          'increment word
          FoundWord = FoundWord + 1
        Loop
        'reset search
        SearchWord = 0
      End If
      
      'if found, word will be valid
      If FoundWord >= 0 And FoundWord < WordsEdit.WordCount Then
        'if back at start (word same as start)
        If FoundWord = StartWord Then
          'rest found position to force search to end
          FoundWord = -1
        End If
        Exit Do
      End If
      
      'if not found, action depends on search mode
      Select Case FindDir
      Case fdUp
        'if not reset yet
        If Not RestartSearch Then
          'if recursing,
          If blnRecurse Then
            'just say no
            rtn = vbNo
          Else
            rtn = MsgBox("Beginning of search scope reached. Do you want to continue from the end?", vbQuestion + vbYesNo, "Find in Word List")
          End If
          If rtn = vbNo Then
            'reset search
            FindForm.ResetSearch
            Me.MousePointer = vbDefault
            Exit Sub
          End If
        Else
          'entire scope already searched; exit
          Exit Do
        End If
        
      Case fdDown
        'if not reset yet
        If Not RestartSearch Then
          'if recursing
          If blnRecurse Then
            'just say no
            rtn = vbNo
          Else
            rtn = MsgBox("End of search scope reached. Do you want to continue from the beginning?", vbQuestion + vbYesNo, "Find in Word List")
          End If
          If rtn = vbNo Then
            'reset search
            FindForm.ResetSearch
            Me.MousePointer = vbDefault
            Exit Sub
          End If
        Else
          'entire scope already searched; exit
          Exit Do
        End If
        
      Case fdAll
        If RestartSearch Then
          Exit Do
        End If
        
      End Select
      
      'reset search so when we get back to start, search will end
      RestartSearch = True
    
    'loop is exited by finding the searchtext or reaching end of search area
    Loop
    
    'if search string found
    If FoundWord >= 0 And FoundWord < WordsEdit.WordCount Then
      'if this is first occurrence
      If Not FirstFind Then
        'save this position
        FirstFind = True
        StartWord = FoundWord
      End If
      
      'highlight Word
      lstWords.ListIndex = FoundWord
      
    'if search string NOT found
    Else
      'if not recursing, show a msg
      If Not blnRecurse Then
        'if something was previously found
        If FirstFind Then
          'search complete; no new instances found
          MsgBox "The specified region has been searched.", vbInformation, "Find in Word List"
        Else
          'show not found msg
          MsgBox "Search text not found.", vbInformation, "Find in Word List"
        End If
      End If
      
      'reset search flags
      FindForm.ResetSearch
    End If
    
    'need to always make sure right form has focus; if finding a word
    'causes the group list to change, VB puts the wordeditor form in focus
    ' but we want focus to match the starting form
    If SearchStartDlg Then
      FindForm.SetFocus
    Else
      Me.SetFocus
    End If
    
    'reset cursor
    Screen.MousePointer = vbDefault
  End Select
Exit Sub
  
ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickFindAgain()

  On Error GoTo ErrHandler
  'if nothing in find form textbox
  If LenB(GFindText) <> 0 Then
    FindInWords GFindText, GMatchWord, GFindDir
  Else
    'show find form
    MenuClickFind
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickInsert()
  
  'add a new group
  NewGroup
End Sub


Public Sub MenuClickSelectAll()

    'insert new word into current group
  
    Select Case Val(lstGroups.Text)
    Case 1
      ' if no words, add the placeholder, moving it if necessary
      If WordsEdit.GroupN(1).WordCount = 0 Then
        'determine if placeholder already exists
        If WordsEdit.WordExists("anyword") Then
          'move it
          MoveWord "anyword", WordsEdit("anyword").Group, 1
        Else
          'add it
          AddWord 1, "anyword", False
        End If
      End If
      
    Case 9999
      ' if no words, add the placeholder, moving it if necessary
      If WordsEdit.GroupN(9999).WordCount = 0 Then
        'determine if placeholder already exists
        If WordsEdit.WordExists("rol") Then
          'move it
          MoveWord "rol", WordsEdit("rol").Group, 9999
        Else
          'add it
          AddWord 9999, "rol", False
        End If
      End If
    Case Else
      'add a new word
      NewWord
    End Select
    
End Sub

Public Sub NewWord(Optional ByVal DontUndo As Boolean = False)

  Dim strNewWord As String
  Dim lngNewWordNum As Long
  Dim i As Long

  On Error GoTo ErrHandler
  
  If lstGroups.ListIndex < 0 Then
    Exit Sub
  End If
  
  On Error Resume Next
  'create an unambiguous new word
  lngNewWordNum = 0
  Do
    'increment new word number
    lngNewWordNum = lngNewWordNum + 1
    strNewWord = "new word " & CStr(lngNewWordNum)
    'attempt to access this word
    i = WordsEdit(strNewWord).Group
    'if word doesn't exist,
    If Err.Number <> 0 Then
      'word doesn't exist
      Exit Do
    End If
  Loop While lngNewWordNum < 1000
  On Error GoTo ErrHandler
  
  'if a new word not found(not very likely)
  If lngNewWordNum = 1000 Then
    If Not DontUndo Then
      MsgBox "You already have 1,000 new word entries. Try changing a few of those before adding more words.", vbInformation + vbOKOnly, "Too Many Default Words"
    End If
    Exit Sub
  End If
  'add the word
  AddWord Val(lstGroups.Text), strNewWord, DontUndo
  
  Select Case Mode
  Case wtByGroup
    'update wordlist to show the new word
    UpdateWordList True
    'select the new word
    lstWords.Text = strNewWord
  Case wtByWord
  End Select
  'set up for editing
  AddNewWord = True
  
  'now edit the word
  MenuClickECustom1
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

'''Public Sub MoveWord(ByVal WordIndex As Long, ByVal OldGrpNum As Long, NewGrpNum As Long, Optional ByVal DontUndo As Boolean = False)
Public Sub MoveWord(WordIndex As Variant, ByVal OldGrpNum As Long, NewGrpNum As Long, Optional ByVal DontUndo As Boolean = False)
  'moves a word from one group to another
  
  Dim NextUndo As WordsUndo, WordText As String
  Dim i As Integer, blnFirst As Boolean
  
  'stop dragging word
  DraggingWord = False
  
  'if groups are the same
  If OldGrpNum = NewGrpNum Then
    Exit Sub
  End If
  
  'is the word the first word in the old group?
  Select Case VarType(WordIndex)
  Case vbByte, vbInteger, vbLong
    blnFirst = (WordIndex = 0)
    WordText = WordsEdit.GroupN(OldGrpNum).Word(WordIndex)
  Case vbString
    blnFirst = (WordsEdit.GroupN(OldGrpNum).GroupName = WordIndex)
    WordText = WordIndex
  Case Else
    Exit Sub
  End Select
  
  'delete word from old group
  WordsEdit.RemoveWord WordText
  'if no words left in old group
  If WordsEdit.GroupN(OldGrpNum).WordCount = 0 Then
    'RARE- ok for group 1/9999 to have no words
    If OldGrpNum <> 1 And OldGrpNum <> 9999 Then
      'need to remove group too
      'delete old group
      WordsEdit.RemoveGroup OldGrpNum
      
      Select Case Mode
      Case wtByGroup
        'delete from listbox
        lstGroups.RemoveItem IndexByGrp(OldGrpNum)
      Case wtByWord
      End Select
    End If
  Else
    'if the moved word was the first word in the old group
    If blnFirst Then
      UpdateGroupName OldGrpNum
    End If
  End If
  
  'if skipping undo
  If DontUndo Then
    'need to add the 'new' group if it doesn't exist
    If Not WordsEdit.GroupExists(NewGrpNum) Then
      AddGroup NewGrpNum, True
    End If
  End If
  
  'add to new group
  WordsEdit.AddWord WordText, NewGrpNum
  
  'if not skipping undo
  If Not DontUndo Then
    'create new undo object
    Set NextUndo = New WordsUndo
    NextUndo.UDAction = udwMoveWord
    'store oldgroup, newgroup, and word
    NextUndo.UDGroupNo = NewGrpNum
    NextUndo.UDOldGroupNo = OldGrpNum
    NextUndo.UDWord = WordText
    'add to undo
    AddUndo NextUndo
    
    'force logic refresh
    blnRefreshLogics = True
  End If
  
  'if the moved word is now the first word in the new group
  If WordsEdit.GroupN(NewGrpNum).GroupName = WordText Then
    UpdateGroupName NewGrpNum
  End If
  
  Select Case Mode
  Case wtByGroup
    'select the new group
    i = GroupIndex(NewGrpNum)
    If Val(lstGroups.List(i)) = NewGrpNum Then
      If lstGroups.ListIndex = i Then
        'force update
        UpdateWordList True
      Else
        'select it
        lstGroups.ListIndex = i
      End If
    End If
    'then select the word
    lstWords.Text = CPToUnicode(WordText, SessionCodePage)
  Case wtByWord
  End Select
End Sub

Public Function LoadWords(ByVal WordFile As String) As Boolean
  
  'opens a word list file and loads it into the editor
  'returns true if successful, false if any errors encountered
  Dim i As Long
  Dim tmpWords As AGIWordList
  Dim rtn As VbMsgBoxResult
  
  On Error GoTo ErrHandler
  
  ' show wait cursor; this may take awhile
  WaitCursor
  
  'use a temp words word to get items
  Set tmpWords = New AGIWordList
  'trap errors
  On Error Resume Next
  tmpWords.Load WordFile
  If Err.Number <> 0 Then
    'restore cursor
    Screen.MousePointer = vbDefault
  
    'if due to missing file:
    If Err.Number = vbObjectError + 524 Then
      'if in a game, give user opportunity to create a new list
      If InGame Then
        rtn = MsgBox("The WORDS.TOK file is missing. Would you like to create a new file?", vbQuestion + vbYesNo, "Missing WORDS.TOK File")
        If rtn = vbYes Then
          'try creating a new file
          tmpWords.NewWords
          tmpWords.Save WordFile
        Else
          'give up
          Exit Function
        End If
      Else
        'not in game; shouldn't normally get this error, because the file is chosen before this
        'function is called, but just in case
        MsgBox "The file: " & vbCrLf & vbCrLf & WordFile & vbCrLf & vbCrLf & " is missing.", vbCritical + vbOKOnly, "Unable to Open WORDS.TOK File"
        'return false
        Exit Function
      End If
    Else
      'if in a game, give user opportunity to create a new list
      If InGame Then
        rtn = MsgBox("The WORDS.TOK file corrupted and can't be loaded. Would you like to create a new file in its place? (Original file will be renamed)", vbQuestion + vbYesNo, "Missing WORDS.TOK File")
        If rtn = vbYes Then
          'kill any existing backup files
          If FileExists(JustPath(WordFile) & "WORDS.TOK.old") Then
            Kill JustPath(WordFile) & "WORDS.TOK.old"
          End If
          Name WordFile As JustPath(WordFile) & "WORDS.TOK.old"
          'now try creating a new file
          tmpWords.NewWords
          tmpWords.Save WordFile
        Else
          'abort the load
          Exit Function
        End If
      Else
        'not in a game; inform user of the error; they will have to deal with the bad file on their own
        ErrMsgBox "Error while trying to load word file: ", "Unable to edit this file.", "Load Word Error"
        'return false
        Exit Function
      End If
    End If
  End If
  On Error GoTo ErrHandler

  'file is clean
  IsChanged = False

  'set caption
  Caption = "Words Editor - "
  If InGame Then
    Caption = Caption & GameID
  Else
    Caption = Caption & CompactPath(WordFile, 75)
  End If

  'copy to local words word
  WordsEdit.Clear
  Set WordsEdit = tmpWords
  'add description
  WordsEdit.Description = tmpWords.Description
  
  Select Case Mode
  Case wtByGroup
    'add word groups to listbox
    lstGroups.Clear
    For i = 0 To WordsEdit.GroupCount - 1
      'for groups 0, 1, 9999 use special annotation
      Select Case WordsEdit.Group(i).GroupNum
      Case 0 'null words
        lstGroups.AddItem "0 - <null words>"
      Case 1 'anyword
        lstGroups.AddItem "1 - <any word>"
      Case 9999 'rest of line
        lstGroups.AddItem "9999 - <rest of line>"
      Case Else
        'add group normally
        lstGroups.AddItem CStr(WordsEdit.Group(i).GroupNum) & " - " & UCase$(CPToUnicode(WordsEdit.Group(i).GroupName, SessionCodePage))
      End Select
    Next i
  
    'VERY RARE, but make sure groups 0, 1, 9999 actually exist
    If Not WordsEdit.GroupExists(0) Then
      'add a placeholder line
      lstGroups.AddItem "0 - <null words>", 0
    End If
    If Not WordsEdit.GroupExists(1) Then
      'add a placeholder line
      lstGroups.AddItem "1 - <anyword>", 1
    End If
    If Not WordsEdit.GroupExists(9999) Then
      'add a placeholder line
      lstGroups.AddItem "9999 - <rest of line>"
    End If
    
    'select first group
    lstGroups.ListIndex = 0
    UpdateWordList
  
  Case wtByWord
    'add all words to lstWords
    For i = 0 To WordsEdit.WordCount - 1
      lstWords.AddItem WordsEdit(i).WordText
    Next i
    'only one item in lstGroup
    lstGroups.AddItem ""
    
    'select first word
    lstGroups.ListIndex = 0
    lstWords.ListIndex = 0
  End Select
  
  LoadWords = True
  
  'restore cursor
  Screen.MousePointer = vbDefault
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function
Public Sub MenuClickOpen()

End Sub
*/
        }

        public void wordsfrmcode2() {
            /*
            Public Sub MenuClickExport()

              If ExportWords(WordsEdit, InGame) Then
                'if this is NOT the in game file,
                If Not InGame Then
                  'reset changed flag
                  IsChanged = False
                  'update caption
                  Caption = "Words Editor - " & CompactPath(WordsEdit.ResFile, 75)
                  'disable save menu/button
                  frmMDIMain.mnuRSave.Enabled = False
                  frmMDIMain.Toolbar1.Buttons("save").Enabled = False
                End If
              End If
            End Sub
            Public Sub MenuClickImport()

            End Sub

            Public Sub MenuClickNew()

            End Sub

            Public Sub MenuClickInGame()
              'not used
            End Sub

            Public Sub MenuClickRenumber()

            End Sub

            Public Sub MenuClickCustom1()
              'merge Words.tok from file

              Dim strMsg As String, OldGroup As Long
              Dim MergeList As AGIWordList
              Dim MergeReplace As VbMsgBoxResult, RepeatAnswer As Boolean
              Dim GroupNum As Long, MergeWord As String
              Dim i As Long, j As Long
              Dim WordCount As Long
              Dim lngCount As Long
              Dim lngRepl As Long

              On Error Resume Next

              'set common dialog properties
              With MainDialog
                .DialogTitle = "Merge Vocabulary Words File"
                .Filter = "WinAGI Words file (*.agw)|*.agw|WORDS.TOK file|WORDS.TOK|All files (*.*)|*.*"
                .FilterIndex = 2
                .DefaultExt = vbNullString
                .FileName = vbNullString
                .InitDir = DefaultResDir

                'get new file
                .ShowOpen
                DefaultResDir = JustPath(.FileName)
              End With

              'check for cancel
              If Err.Number = cdlCancel Then
                Exit Sub
              End If

              'show progress form
              Load frmProgress
              With frmProgress
                .Caption = "Merging from File"
                .lblProgress.Caption = "Merging..."
                .pgbStatus.Max = MergeList.WordCount
                .pgbStatus.Value = 0
                .Show vbModeless, frmMDIMain
                .Refresh
              End With

              'show wait cursor
              WaitCursor

              'load the merge list
              Set MergeList = New AGIWordList
              MergeList.Load MainDialog.FileName

              'if an error,
              If Err.Number <> 0 Then
                Unload frmProgress
                ErrMsgBox "An error occurred while trying to load " & JustFileName(MainDialog.FileName) & ": ", "Unable to merge the file.", "Import Word List Error"
                MergeList.Clear
                Set MergeList = Nothing
                'reset cursor
                Screen.MousePointer = vbDefault
                Exit Sub
              End If

              On Error GoTo ErrHandler

              WordCount = MergeList.WordCount - 1

              'step through all words
              For i = 0 To WordCount
                'get word and group
                GroupNum = MergeList(i).Group
                MergeWord = MergeList(i).WordText

                On Error Resume Next
                'determine if word exists in current file
                OldGroup = WordsEdit(MergeWord).Group
                'if no error
                If Err.Number = 0 Then
                  'word exists;
                  'if not the same group
                  If OldGroup <> GroupNum Then
                    'if not repeating answer
                    If Not RepeatAnswer Then
                      'get decision from user
                      MergeReplace = MsgBoxEx(QUOTECHAR & MergeWord & """ already exists in Group " & CStr(WordsEdit(MergeWord).Group) & "." & vbNewLine & "Do you want to move it to group " & CStr(GroupNum) & "?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Replace Word?", WinAGIHelp, "htm\winagi\Words_Editor.htm#merge", "Repeat this answer for all duplicate words", RepeatAnswer)
                    End If

                    'if replacing,
                    If MergeReplace = vbYes Then
                      'remove it from previous group
                      WordsEdit.RemoveWord MergeWord
                      'add it to new group
                      WordsEdit.AddWord MergeWord, GroupNum
                      lngRepl = lngRepl + 1
                    End If
                  End If
                Else
                  'not in current list- ok to add
                  WordsEdit.AddWord MergeWord, GroupNum
                  lngCount = lngCount + 1
                End If
                On Error GoTo ErrHandler

                'update progressform
                frmProgress.pgbStatus.Value = frmProgress.pgbStatus.Value + 1
                frmProgress.Refresh
              Next i

              'clear and dereference mergelist
              MergeList.Clear
              Set MergeList = Nothing

              'refresh form
              Select Case Mode
              Case wtByGroup
                RebuildGroupList
              Case wtByWord
              End Select

              'reset cursor
              Screen.MousePointer = vbDefault

              MsgBox "Added " & CStr(lngCount) & " words. Replaced " & CStr(lngRepl) & " words."

              'unload progress form
              Unload frmProgress
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Public Sub MenuClickCustom2()

              Dim i As Long, blnDontAsk As Boolean, rtn As VbMsgBoxResult
              Dim RepeatAnswer As Long, tmpLogic As AGILogic
              Dim GroupUsed() As Boolean, UnusedCount As Long
              Dim strCount As String, stlOutput As StringList
              Dim tmpLoad As Boolean, lngPos As Long
              Dim tmpLen As Long, strToken As String

              On Error GoTo ErrHandler

              ' word usage check- find words not used in a game

              'only for ingame word lists
              If Not InGame Then
                Exit Sub
              End If

              'check for open logics- save or cancel if changed
              For i = 1 To LogicEditors.Count
                If LogicEditors(i).FormMode = fmLogic Then
                  If LogicEditors(i).rtfLogic.IsChanged Then
                    Select Case RepeatAnswer
                    Case 0  'ask user for input
                      LogicEditors(i).SetFocus
                      'get user's response
                      rtn = MsgBoxEx("Do you want to save this logic before checking word usage?", _
                            vbQuestion + vbYesNoCancel, "Update " & ResourceName(LogicEditors(i).EditLogic, True, True) & _
                            "?", , , "Repeat this answer for all other open logics.", blnDontAsk)
                      If blnDontAsk Then
                        If rtn = vbYes Then
                          RepeatAnswer = 2
                        ElseIf rtn = vbNo Then
                          RepeatAnswer = 1
                        End If
                      End If

                    Case 1  'no
                      rtn = vbNo

                    Case 2  'yes
                      rtn = vbYes
                    End Select

                    Select Case rtn
                    Case vbCancel
                      Exit Sub
                    Case vbYes
                      'save it
                      LogicEditors(i).MenuClickSave
                    End Select
                  End If
                End If
              Next i

              ' show wait cursor; this may take awhile
              WaitCursor

              'build array to store results
              ReDim GroupUsed(WordsEdit.GroupCount - 1)

              'step through all logics
              For Each tmpLogic In Logics
                tmpLoad = tmpLogic.Loaded
                If Not tmpLoad Then
                  tmpLogic.Load
                End If
                'find all said commands
                'mark words/groups as in use
                lngPos = 0
                Do
                  lngPos = FindNextToken(tmpLogic.SourceText, lngPos, "said", True)
                  If lngPos = 0 Then
                    Exit Do
                  End If
                  'skip to end of 'said'
                  lngPos = lngPos + 3
                  strToken = ""
                  lngPos = FindNextToken(tmpLogic.SourceText, lngPos, strToken, True)
                  If lngPos = 0 Then
                    Exit Do
                  End If
                  If strToken = "(" Then
                    Do
                      'get word arguments; skip commas
                      strToken = ""
                      lngPos = FindNextToken(tmpLogic.SourceText, lngPos, strToken, True)
                      If lngPos = 0 Then
                        Exit Do
                      End If
                      lngPos = lngPos + Len(strToken) - 1
                      'check for valid word ( at least 3 chars)? or numeric?
                      If Len(strToken) < 3 And Not IsNumeric(strToken) Then
                        Exit Do
                      End If
                      If IsNumeric(strToken) Then
                        'count this group
                        If WordsEdit.GroupExists(CLng(strToken)) Then
                          GroupUsed(GroupIndex(CLng(strToken))) = True
                        End If
                      Else
                        'must start/end with quotes
                        If Asc(strToken) <> 34 Or Asc(Right$(strToken, 1)) <> 34 Then
                          Exit Do
                        End If
                        strToken = LCase$(Mid$(strToken, 2, Len(strToken) - 2))
                        If WordsEdit.WordExists(LCase$(strToken)) Then
                          'count this group
                          GroupUsed(GroupIndex(WordsEdit(strToken).Group)) = True
                        End If
                      End If

                      'next cmd can be ',' or ')' (or invalid said)
                      strToken = ""
                      lngPos = FindNextToken(tmpLogic.SourceText, lngPos, strToken, True)
                      If lngPos = 0 Or strToken <> "," Then
                        Exit Do
                      End If
                    Loop
                  End If
                Loop
                If Not tmpLoad Then
                  tmpLogic.Unload
                End If
                SafeDoEvents
              Next

              Set stlOutput = New StringList

              'restore cursor
              Screen.MousePointer = vbDefault

              ' go through all groups, make list of any that are unused
              For i = 0 To UBound(GroupUsed) - 1
                If Not GroupUsed(i) Then
                  'skip 0, 1, 9999
                  Select Case WordsEdit.Group(i).GroupNum
                  Case 0, 1, 9999
                    'skip these
                  Case Else
                    'add line
                    stlOutput.Add WordsEdit.Group(i).GroupNum & vbTab & WordsEdit.Group(i).GroupName
                    UnusedCount = UnusedCount + 1
                  End Select
                End If
              Next i
              'add blank line to add trailing crlf
              stlOutput.Add ""

              'present results
              If UnusedCount = 0 Then
                MsgBox "All word groups are used in this game.", vbInformation + vbOKOnly, "No Unused Words"
              Else
                'copy output to clipboard
                Clipboard.Clear
                Clipboard.SetText stlOutput.Text, vbCFText

                If UnusedCount = 1 Then
                  strCount = "is one unused word group"
                Else
                  strCount = "are " & CStr(UnusedCount) & " unused word groups"
                End If
                MsgBox "There " & strCount & " in this game. " & vbNewLine & vbNewLine & _
                       "The full list has been copied to the clipboard.", _
                       vbInformation + vbOKOnly, "Unused Word Groups Check Results"
              End If
            Exit Sub

            ErrHandler:
              Debug.Assert False
              Resume Next
            End Sub

            Private Function GroupIndex(ByVal GroupNumber As Long) As Long

              'returns index of a group by its group number

              Dim i As Long

              For i = 0 To WordsEdit.GroupCount - 1
                If WordsEdit.Group(i).GroupNum = GroupNumber Then
                 GroupIndex = i
                 Exit Function
                End If
              Next i
            End Function

            Public Sub MenuClickCustom3()

            End Sub

            Public Sub MenuClickCopy()

              Dim i As Long

              With WordsClipboard
                'put selected word or group on clipboard
                Select Case SelMode
                Case smWord
                  .UDAction = udwDelWord
                  'don't need to validate case or format if copying
                  'from an existing word
                  .UDWord = CurrentWord()

                  'put the word on the real clipboard, too
                  '(need to convert it to correct codepage so it can
                  'be compatible with RichEdAGI control)
                  Clipboard.Clear
                  Clipboard.SetText QUOTECHAR & UnicodeToCP(lstWords.Text, CodePage) & QUOTECHAR, vbCFText

                Case smGroup
                  .UDAction = udwDelGroup
                  .UDGroupNo = Val(lstGroups.Text)

                  .UDGroup.Clear
                  For i = 0 To lstWords.ListCount - 1
                    .UDGroup.Add lstWords.List(i)
                  Next i
                  'put groupname on real clipboard too
                  Clipboard.Clear
                  Clipboard.SetText QUOTECHAR & WordsClipboard.UDGroup(0) & QUOTECHAR, vbCFText

                End Select
              End With
              'enable pasting
              frmMDIMain.mnuEPaste.Enabled = True
              Me.Toolbar1.Buttons(3).Enabled = True

            End Sub
            Public Sub MenuClickCut()

              'if nothing selected
              If SelMode = smNone Then
                Exit Sub
              End If

              'copy,
              MenuClickCopy

              'then delete
              MenuClickDelete

              'change last undo item to indicate cut
              Select Case UndoCol(UndoCol.Count).UDAction
              Case udwDelWord
                UndoCol(UndoCol.Count).UDAction = udwCutWord
              Case udwDelGroup
                UndoCol(UndoCol.Count).UDAction = udwCutGroup
              End Select
            End Sub
            Public Sub MenuClickPaste()

              Dim lngOldGroupNo As Long, strWord As String
              Dim blnMove As Boolean
              Dim NextUndo As WordsUndo
              Dim i As Long, strMsg As String
              Dim lngNewGrpNo As Long

              On Error GoTo ErrHandler

              'only allow pasting if the custom clipboard is set OR if a valid word is on the clipboard

              'if a group
              If WordsClipboard.UDAction = udwDelGroup Then
                'VERY RARE- but check for max number of groups
                If lstGroups.ListCount = &H7FFF Then
                  'ignore
                  Exit Sub
                End If

                'clipboard contains a single group
                lngNewGrpNo = NextGrpNum()

                'setup undo
                Set NextUndo = New WordsUndo
                NextUndo.UDAction = udwPasteGroup

                'check words; see which ones can be added
                For i = 0 To WordsClipboard.UDGroup.Count - 1
                  If WordsEdit.WordExists(WordsClipboard.UDGroup(i)) Then
                    'word is in use
                    lngOldGroupNo = WordsEdit(WordsClipboard.UDGroup(i)).Group
                    strMsg = strMsg & vbNewLine & vbTab & WordsClipboard.UDGroup(i) & " (in group " & CStr(lngOldGroupNo) & ")"
                  Else
                    'add it to undo object
                    NextUndo.UDGroup.Add WordsClipboard.UDGroup(i)
                  End If
                Next i

                'if nothing added
                If NextUndo.UDGroup.Count = 0 Then
                  'nothing to add
                  MsgBox "No words on the clipboard could be pasted; they all are already in this word list.", vbInformation + vbOKOnly, "Nothing to Paste"
                  Exit Sub
                End If

                'add group
                AddGroup lngNewGrpNo, True

                'add the words
                For i = 0 To NextUndo.UDGroup.Count - 1
                  'add it (without undo)
                  AddWord lngNewGrpNo, NextUndo.UDGroup(i), True
                Next i

                'reset group name
                UpdateGroupName lngNewGrpNo

                Select Case Mode
                Case wtByGroup
                  'select the group
                  lstGroups.ListIndex = lngNewGrpNo
                Case wtByWord
                End Select

                'finish the undo
                NextUndo.UDGroupNo = lngNewGrpNo
                'the words added aren't needed
                NextUndo.UDGroup.Clear
                AddUndo NextUndo

                'if there is a msg
                If LenB(strMsg) <> 0 Then
                  MsgBox "The following words were not added because they already exist in another group: " & strMsg, vbInformation + vbOKOnly, "Paste Group from Clipboard"
                End If

              ElseIf WordsClipboard.UDAction = udwDelWord Or Clipboard.GetFormat(vbCFText) Then
                'clipboard contains a single word
                'if real clipboard word has changed, change WordsClipboard to match...
                If Clipboard.GetFormat(vbCFText) Then
                  strWord = Clipboard.GetText(vbCFText)
                  strWord = Replace(strWord, vbCr, "")
                  strWord = Replace(strWord, vbLf, "")
                  strWord = LCase(strWord)

                  If strWord <> Chr$(34) & WordsClipboard.UDWord & Chr$(34) Then
                    'check formatting
                    If CheckWordFormat(strWord) Then
                      'this word is acceptable; put it on custom clipboard
                      WordsClipboard.UDWord = strWord
                      WordsClipboard.UDAction = udwDelWord
                      'and update real clipboard?
                      Clipboard.SetText Chr$(34) & strWord & Chr$(34)
                    Else
                      'not a valid word on clipboard
                      Exit Sub
                    End If
                  End If
                Else
                  'nothing on main clipboard - is there anything on custom clipboard?
                  If Len(WordsClipboard.UDWord) = 0 Then
                    Exit Sub
                  End If
                End If

                'validate word
                If WordsEdit.WordExists(WordsClipboard.UDWord) Then
                  lngOldGroupNo = WordsEdit(WordsClipboard.UDWord).Group
                  'if already in this group
                  If lngOldGroupNo = Val(lstGroups.Text) Then
                    MsgBox ChrW$(39) & WordsClipboard.UDWord & "' already exists in this group."
                    Exit Sub
                  End If

                  'word is in another group- ask if word should be moved
                  If MsgBox(ChrW$(39) & WordsClipboard.UDWord & "' already exists (in group " & CStr(lngOldGroupNo) & "). Do you want to move it to this group?", vbYesNo + vbQuestion) = vbNo Then
                    Exit Sub
                  End If
                  'delete word from other group
                  DelWord WordsClipboard.UDWord, True
                  blnMove = True
                Else
                  'old group same as new (so Undo knows a word wasn't moved)
                  lngOldGroupNo = Val(lstGroups.Text)
                End If

                'add word to this group
                AddWord Val(lstGroups.Text), WordsClipboard.UDWord, True

                'add undo
                Set NextUndo = New WordsUndo
                NextUndo.UDAction = udwPasteWord
                NextUndo.UDGroupNo = Val(lstGroups.Text)
                NextUndo.UDOldGroupNo = lngOldGroupNo
                NextUndo.UDWord = WordsClipboard.UDWord
                AddUndo NextUndo

                'update word list to show new word
                Select Case Mode
                Case wtByGroup
                  UpdateWordList True
                Case wtByWord
                End Select
              End If

            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Public Sub MenuClickUndo()

              Dim NextUndo As WordsUndo
              Dim i As Long, j As Long
              Dim strTemp As String, strWords() As String
              Dim lngGroup As Long

              On Error GoTo ErrHandler

              'if there are no undo actions
              If UndoCol.Count = 0 Then
                'just exit
                Exit Sub
              End If

              'get next undo object
              Set NextUndo = UndoCol(UndoCol.Count)
              'remove undo object
              UndoCol.Remove UndoCol.Count
              'reset undo menu
              frmMDIMain.mnuEUndo.Enabled = (UndoCol.Count > 0)
              If frmMDIMain.mnuEUndo.Enabled Then
                frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(WORDSUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
              Else
                frmMDIMain.mnuEUndo.Caption = "&Undo" & vbTab & "Ctrl+Z"
              End If

              'undo the action
              Select Case NextUndo.UDAction
              Case udwAddGroup, udwPasteGroup
                'delete group
                DelGroup NextUndo.UDGroupNo, True

              Case udwDelGroup, udwCutGroup
                'add group
                AddGroup NextUndo.UDGroupNo, True
                'add words back into group
                For i = 0 To NextUndo.UDGroup.Count - 1
                  WordsEdit.AddWord NextUndo.UDGroup(i), NextUndo.UDGroupNo
                Next i
                UpdateGroupName NextUndo.UDGroupNo

                Select Case Mode
                Case wtByGroup
                  'select the group added
                  For i = 0 To lstGroups.ListCount - 1
                    If Val(lstGroups.List(i)) = NextUndo.UDGroupNo Then
                      lstGroups.ListIndex = i
                      lstGroups_Click
                      Exit For
                    End If
                  Next i
                Case wtByWord
                End Select

              Case udwAddWord, udwPasteWord
                'delete the added word
                DelWord NextUndo.UDWord, True

                'if the add killed a word in another group
                If NextUndo.UDGroupNo <> NextUndo.UDOldGroupNo Then
                  'if group dosn't exist(meaning this word was last word in the group)
                  If Not WordsEdit.GroupExists(NextUndo.UDOldGroupNo) Then
                    'add the group first
                    AddGroup NextUndo.UDOldGroupNo, True
                  End If

                  'need to add back the deleted word to its old group
                  AddWord NextUndo.UDOldGroupNo, NextUndo.UDWord, True
                End If

                Select Case Mode
                Case wtByGroup
                  'select group that deleted word was in
                  For i = 0 To lstGroups.ListCount - 1
                    If Val(lstGroups.List(i)) = NextUndo.UDGroupNo Then
                      'if already selected
                      If lstGroups.ListIndex = i Then
                        UpdateWordList True
                      Else
                        lstGroups.ListIndex = i
                      End If
                      Exit For
                    End If
                  Next i
                Case wtByWord
                End Select

              Case udwDelWord, udwCutWord
                'check for case of deleting or cutting last word in a group
                If Not WordsEdit.GroupExists(NextUndo.UDGroupNo) Then
                  'need to add this group
                  'entry
                  AddGroup NextUndo.UDGroupNo, True
                End If

                'now we know for sure group does exist- add the word
                AddWord NextUndo.UDGroupNo, NextUndo.UDWord, True

                Select Case Mode
                Case wtByGroup
                  'select the restored word
                  For i = 0 To lstGroups.ListCount - 1
                    If Val(lstGroups.List(i)) = NextUndo.UDGroupNo Then
                      If lstGroups.ListIndex = i Then
                        UpdateWordList True
                      Else
                        lstGroups.ListIndex = i
                      End If
                      Exit For
                    End If
                  Next i
                  lstWords.Text = CPToUnicode(NextUndo.UDWord, SessionCodePage)
                Case wtByWord
                End Select

              Case udwMoveWord 'store old and new group numbers, and word index
                'move word back to old position
                MoveWord NextUndo.UDWord, NextUndo.UDGroupNo, NextUndo.UDOldGroupNo, True
                Select Case Mode
                Case wtByGroup
                  'reselect group word is moved TO
                  lstGroups.ListIndex = WordsEdit.GroupN(NextUndo.UDOldGroupNo).GroupNum
                  UpdateWordList True
                  'select the moved word
                  lstWords.Text = CPToUnicode(NextUndo.UDWord, SessionCodePage)
                End Select

              Case udwRenumber 'store old group number AND new group number
                'change number back
                RenumberGroup NextUndo.UDGroupNo, NextUndo.UDOldGroupNo, True

              Case udwChangeWord, udwReplace 'store old word and new word and group
                'change new word back to old word
                EditWord NextUndo.UDWord, NextUndo.UDOldWord, True
                'if the change killed a word in another group
                If NextUndo.UDGroupNo <> NextUndo.UDOldGroupNo Then
                  'if group dosn't exist(meaning this word was last word in the group)
                  If Not WordsEdit.GroupExists(NextUndo.UDOldGroupNo) Then
                    'add the group first
                    AddGroup NextUndo.UDOldGroupNo, True
                  End If

                  'need to add back the deleted word to its old group
                  AddWord NextUndo.UDOldGroupNo, NextUndo.UDWord, True
                End If

                Select Case Mode
                Case wtByGroup
                  'reselect the modified word and its group
                  If Val(lstGroups.Text) <> NextUndo.UDGroupNo Then
                    For i = 0 To lstGroups.ListCount - 1
                      If Val(lstGroups.List(i)) = NextUndo.UDGroupNo Then
                        lstGroups.Selected(i) = True
                        lstGroups.ListIndex = i
                      Else
                        lstGroups.Selected(i) = False
                      End If
                    Next i
                  Else
                    UpdateWordList True
                  End If
                  For i = 0 To lstWords.ListCount - 1
                    If WordsEdit.GroupN(NextUndo.UDGroupNo).Word(i) = NextUndo.UDOldWord Then
                      lstWords.Selected(i) = True
                      lstWords.ListIndex = i
                      Exit For
                    End If
                  Next i
                Case wtByWord
                End Select

              Case udwChangeDesc
                'resore old description
                WordsEdit.Description = NextUndo.UDDescription
                'if in a game
                If InGame Then
                  'restore the ingame resource as well
                  VocabularyWords.Description = NextUndo.UDDescription
                  VocabularyWords.Save
                  'update prop window
                  RefreshTree rtWords, -1, umProperty
                End If

              Case udwReplaceAll
                'description field has MatchWord flag
                If CBool(NextUndo.UDDescription) Then
                  'restore a single word

                  'remove existing word
                  WordsEdit.RemoveWord NextUndo.UDWord

                  'restore previous word
                  WordsEdit.AddWord NextUndo.UDOldWord, NextUndo.UDGroupNo
                  'ensure group name is correct
                  UpdateGroupName NextUndo.UDGroupNo

                  'if the existing word was in a different group
                  If NextUndo.UDOldGroupNo <> -1 Then
                    'add word back to its old group
                    WordsEdit.AddWord NextUndo.UDWord, NextUndo.UDOldGroupNo
                    'ensure groupname is correct
                    UpdateGroupName NextUndo.UDOldGroupNo
                  End If

                  Select Case Mode
                  Case wtByGroup
                    'if either group is currently selected,
                    If Val(lstGroups.Text) = NextUndo.UDGroupNo Or (Val(lstGroups.Text) = NextUndo.UDOldGroupNo And NextUndo.UDOldGroupNo <> -1) Then
                      UpdateWordList True
                    End If
                  Case wtByWord
                  End Select
                Else
                  'show wait cursor
                  WaitCursor

                  'restore a bunch of words
                  strWords = Split(NextUndo.UDWord, "|")
                  For i = 0 To UBound(strWords) Step 4
                    'first element is group where word will be restored
                    'second element is old group (if the new word was in a different group)
                    'third element is old word being restored
                    'fourth element is new word being 'undone'

                    'remove new word
                    WordsEdit.RemoveWord strWords(i + 3)
                    'add old word to this group
                    WordsEdit.AddWord strWords(i + 2), CLng(strWords(i))
                    'update groupname
                    UpdateGroupName CLng(strWords(i))

                    'if oldgroup is valid
                    If CLng(strWords(i + 1)) <> -1 Then
                      'restore word to original group
                      WordsEdit.AddWord strWords(3), CLng(strWords(i + 1))
                      'update the originalgroup name
                      UpdateGroupName CLng(strWords(i + 1))
                    End If
                  Next i

                  Select Case Mode
                  Case wtByGroup
                    'refreshwordlist
                    UpdateWordList True
                  Case wtByWord
                  End Select
                  Screen.MousePointer = vbDefault
                End If


              Case udwClear
                'clear current wordlist
                WordsEdit.Clear
                'clear group list
                lstGroups.Clear
                'now add in groups from the undo stringlist
                For i = 0 To NextUndo.UDGroup.Count - 1
                  'start with group number
                  strTemp = NextUndo.UDGroup(i)
                  strWords = Split(strTemp, "|")
                  lngGroup = CLng(strWords(0))
                  'add words
                  For j = 1 To UBound(strWords)
                    WordsEdit.AddWord strWords(j), lngGroup
                  Next j
                  Select Case Mode
                  Case wtByGroup
                    'if at least one item
                    If UBound(strWords) > 0 Then
                      'add group
                      lstGroups.AddItem strWords(0) & " - " & UCase$(CPToUnicode(strWords(1), SessionCodePage))
                    Else
                      'add group number only
                      lstGroups.AddItem strWords(0) & " - "
                    End If
                  End Select
                Next i
                Select Case Mode
                Case wtByGroup
                  'select first group
                  SelGroup = -1
                  lstGroups.ListIndex = 0
                Case wtByWord
                End Select
              End Select

              'set changed status
              MarkAsChanged

              'update status bar and edit menu
              UpdateStatusBar
              SetEditMenu
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Sub RebuildGroupList(Optional ByVal SelGroup As Long = 0, Optional ByVal MarkChanged As Boolean = True)
              'rebuilds a word list after it was modified by a merge operation
              ' only called in ByGroup mode
              Dim i As Long

              On Error GoTo ErrHandler

              'file is changed
              If MarkChanged And Not IsChanged Then
                MarkAsChanged
              End If

              'add word groups to listbox
              lstGroups.Clear
              For i = 0 To WordsEdit.GroupCount - 1
                'add group
                lstGroups.AddItem CStr(WordsEdit.Group(i).GroupNum) & " - " & UCase$(CPToUnicode(WordsEdit.Group(i).GroupName, SessionCodePage))
              Next i

              'select desired group
              lstGroups.ListIndex = GroupIndex(SelGroup)
            Exit Sub

            ErrHandler:
              Resume Next
            End Sub

            Function AskClose() As Boolean

              Dim rtn As VbMsgBoxResult

              'assume user wants to cancel
              AskClose = True

              On Error GoTo ErrHandler

              'if wordlist has been changed,
              If IsChanged Then
                'ask user to save changes
                rtn = MsgBox("Do you want to save changes to this word list before closing?", vbYesNoCancel, "Words Editor")

                'if user wants to save
                If rtn = vbYes Then
                  'save by calling the menuclick method
                  MenuClickSave
                End If

                'return false if cancel was selected
                AskClose = rtn <> vbCancel
              End If
            Exit Function

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Function

            Private Function EditWord(OldWord As String, NewWord As String, Optional ByVal DontUndo As Boolean = False) As Boolean

              Dim NextUndo As WordsUndo
              Dim GroupNo As Long, lngOldGroupNo As Long
              Dim blnMoving As Boolean
              Dim rtn As VbMsgBoxResult
              Dim blnFirst As Boolean, blnDelFirst As Boolean

              On Error GoTo ErrHandler

              'if word hasn't changed
              If NewWord = OldWord Then
                'just exit
                EditWord = True
                Exit Function
              End If

              'if new word is an empty string
              If NewWord = vbNullString Then
                'delete the word being changed
                DelWord OldWord, DontUndo
                Exit Function
              End If

              'get group number
              GroupNo = WordsEdit(OldWord).Group

              'if the old word was the first word
              blnFirst = (WordsEdit.GroupN(GroupNo).GroupName = OldWord)

              'determine if new word already exists
              blnMoving = WordsEdit.WordExists(NewWord)

              If blnMoving Then
                'track the previous group
                lngOldGroupNo = WordsEdit(NewWord).Group

                'show msg if adding undo
                If Not DontUndo Then
                  'RARE, but check for 'rol' and 'anyword' if part of a reserved group
                  If lngOldGroupNo = 1 Then
                    rtn = MsgBoxEx("'" & NewWord & "' is used as a place holder for reserved group 1 (any word)." & vbNewLine & "Are you sure you want to move it to this group?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Change Reserved Group Word", WinAGIHelp, "htm\agi\words.htm#reserved")
                  ElseIf lngOldGroupNo = 9999 Then
                    rtn = MsgBoxEx("'" & NewWord & "' is used as a place holder for reserved group 9999 (rest of line)." & vbNewLine & "Are you sure you want to move it to this group?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Change Reserved Group Word", WinAGIHelp, "htm\agi\words.htm#reserved")
                  Else
                    'for any other word, get user OK to move the word
                    rtn = MsgBox("This word already exists in wordgroup " & CStr(lngOldGroupNo) & "." & vbCrLf & _
                    "Do you want to move it to this wordgroup?", vbQuestion + vbYesNo, "Duplicate Word")
                  End If

                  If rtn = vbNo Then
                    'reset word
                    rtfWord.Text = OldWord
                    Exit Function
                  End If
                Else
                  'return false
                  Exit Function
                End If

                'if this is first word in the OLD group
                blnDelFirst = (WordsEdit.GroupN(lngOldGroupNo).GroupName = NewWord)

                'delete new word from its old group (so it can be added to new group)
                WordsEdit.RemoveWord NewWord
                'if no words left in old group
                If WordsEdit.GroupN(lngOldGroupNo).WordCount = 0 Then
                  'RARE- ok for group 0, 1/9999 to have no words
                  If lngOldGroupNo <> 0 And lngOldGroupNo <> 1 And lngOldGroupNo <> 9999 Then
                    'need to remove group too
                    'delete old group
                    WordsEdit.RemoveGroup lngOldGroupNo

                    Select Case Mode
                    Case wtByGroup
                      'delete from listbox
                      lstGroups.RemoveItem IndexByGrp(lngOldGroupNo)
                    Case wtByWord
                    End Select
                  End If
                Else
                  'if it was first word
                  If blnDelFirst Then
                    UpdateGroupName lngOldGroupNo
                  End If
                End If
              Else
                'the new word doesn't exist;
                'set oldgrp equal to new group
                lngOldGroupNo = GroupNo
              End If

              'now delete OLD word
              WordsEdit.RemoveWord OldWord

              'add new word
              WordsEdit.AddWord NewWord, GroupNo

              'if this is the first word in the group OR old word was first word in the group
              If (WordsEdit.GroupN(GroupNo).GroupName = NewWord) Or blnFirst Then
                UpdateGroupName GroupNo
              End If

              'if not skipping undo
              If Not DontUndo Then
                'if adding a new word
                If AddNewWord Then
                  'change last undo object
                  UndoCol(UndoCol.Count).UDWord = NewWord
                  UndoCol(UndoCol.Count).UDOldGroupNo = lngOldGroupNo

                'if NOT adding a new group
                ElseIf Not AddNewGroup Then
                  'create undo object
                  Set NextUndo = New WordsUndo
                  NextUndo.UDAction = udwChangeWord
                  NextUndo.UDGroupNo = GroupNo
                  NextUndo.UDWord = NewWord
                  NextUndo.UDOldWord = OldWord
                  NextUndo.UDOldGroupNo = lngOldGroupNo

                  'add undo item
                  AddUndo NextUndo

                  ' force logics refresh
                  blnRefreshLogics = True
                End If
              End If

              'return success
              EditWord = True
            Exit Function

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Function

            Private Sub RenumberGroup(OldGroupNo As Long, NewGroupNo As Long, Optional ByVal DontUndo As Boolean = False)

              Dim NextUndo As WordsUndo
              Dim i As Long, lngCount As Long

              On Error GoTo ErrHandler
              '*'Debug.Assert Not WordsEdit.GroupExists(NewGroupNo)

              'renumber!
              WordsEdit.RenumberGroup OldGroupNo, NewGroupNo

              'rebuild groups list
              Select Case Mode
              Case wtByGroup
                #If DEBUGMODE <> 1 Then
                  'disable window painting for the listbox until done
                  SendMessage lstGroups.hWnd, WM_SETREDRAW, 0, 0
                #End If

                'easiest way is to clear it and completely rebuild
                lstGroups.Clear
                For i = 0 To WordsEdit.GroupCount - 1
                  lstGroups.AddItem CStr(WordsEdit.Group(i).GroupNum) & " - " & UCase$(WordsEdit.Group(i).GroupName)
                  If WordsEdit.Group(i).GroupNum = NewGroupNo Then
                    'note index of the renumbered group, so it can be selected
                    lngCount = i
                  End If
                Next i

                'reselect the group
                lstGroups.ListIndex = lngCount

                #If DEBUGMODE <> 1 Then
                  SendMessage lstGroups.hWnd, WM_SETREDRAW, 1, 0
                #End If

                lstGroups.Refresh

              Case wtByWord
              End Select

              'if not skipping undo
              If Not DontUndo Then
                'create new undo object
                Set NextUndo = New WordsUndo
                NextUndo.UDAction = udwRenumber
                NextUndo.UDOldGroupNo = OldGroupNo
                NextUndo.UDGroupNo = NewGroupNo
                'add it
                AddUndo NextUndo
              End If

              'force logics refresh
              blnRefreshLogics = True
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub
            Private Sub Form_Activate()

              'if minimized, exit
              '(to deal with occasional glitch causing focus to lock up)
              If Me.WindowState = vbMinimized Then
                Exit Sub
              End If

              ActivateActions

              'if visible,
              If Visible Then
                'force resize
                Form_Resize
              End If
            End Sub

            Private Sub ActivateActions()

              On Error GoTo ErrHandler

              'if hiding prevwin on lost focus, hide it now
              If Settings.HidePreview Then
                PreviewWin.Hide
              End If

              'adjust major menus
             '*'Debug.Print "AdjustMenus 11"
              AdjustMenus rtWords, InGame, True, IsChanged

              'set edit menu
              SetEditMenu

              'update status bar
              UpdateStatusBar

              'if findform is visible,
              If FindForm.Visible Then
                'set correct mode
                If FindForm.rtfReplace.Visible Then
                  'show in replace word mode
                  FindForm.SetForm ffReplaceWord, False
                Else
                  'show in find word mode
                  FindForm.SetForm ffFindWord, False
                End If
              End If

              'set searching form to this form
              Set SearchForm = Me
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Public Sub SetEditMenu()
              'sets the menu captions on the Edit menu

              On Error GoTo ErrHandler

              Dim blnAdd As Boolean, lngGrpNo As Long

              'always force form to current
              If Not (frmMDIMain.ActiveMdiChild Is Me) And Not frmMDIMain.ActiveMdiChild Is Nothing Then
                If Me.Visible Then
                  Me.SetFocus
                End If
              End If

              With frmMDIMain
                .mnuEdit.Enabled = True
                .mnuEUndo.Visible = True
                .mnuEBar0.Visible = True

                'if there is something to undo
                If UndoCol.Count > 0 Then
                  .mnuEUndo.Enabled = (Mode = wtByGroup)
                  .mnuEUndo.Caption = "&Undo " & LoadResString(WORDSUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
                Else
                  .mnuEUndo.Enabled = False
                  .mnuEUndo.Caption = "&Undo" & vbTab & "Ctrl+Z"
                End If
                Toolbar1.Buttons("undo").Enabled = .mnuEUndo.Enabled
                .mnuERedo.Visible = False

                .mnuECustom3.Visible = True
                .mnuECustom3.Enabled = True
                .mnuECustom3.Caption = "Toggle Mode "

                .mnuRCustom2.Visible = InGame

                'cut is enabled if mode is group or word, and a group or word is selected
                '  NOT grp 0, 1, 1999
                .mnuECut.Visible = True
                'assume OK
                .mnuECut.Enabled = (Mode = wtByGroup)
                If SelMode = smGroup Then
                  If lstGroups.ListIndex = -1 Then
                    .mnuECut.Enabled = False
                  ElseIf Val(lstGroups.Text) = 0 Or Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 Then
                    .mnuECut.Enabled = False
                  End If
                ElseIf SelMode = smWord Then
                  .mnuECut.Caption = "Cu&t" & vbTab & "Ctrl+X"
                  If lstWords.ListIndex = -1 Then
                    .mnuECut.Enabled = False
                  End If
                  'for group 0, 1, 9999 disable if no words in group
                  If Val(lstGroups.Text) = 0 Or Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 Then
                    If WordsEdit.GroupN(Val(lstGroups.Text)).WordCount = 0 Then
                      .mnuECut.Enabled = False
                    End If
                  End If
                End If
                Toolbar1.Buttons("cut").Enabled = .mnuECut.Enabled

                'copy is same as cut
                ' EXCEPT copying group 0 is ok
                .mnuECopy.Visible = True
                .mnuECopy.Enabled = .mnuECut.Enabled
                If SelMode = smGroup And lstGroups.ListIndex = 0 Then
                  .mnuECopy.Enabled = (Mode = wtByGroup)
                End If
                .mnuECopy.Caption = "&Copy" & vbTab & "Ctrl+C"
                Toolbar1.Buttons("copy").Enabled = .mnuECopy.Enabled

                'paste if something on clipboard
                .mnuEPaste.Visible = True
                Select Case Mode
                Case wtByGroup
                  Select Case WordsClipboard.UDAction
                  Case udwDelWord
                    .mnuEPaste.Enabled = SelMode <> smNone And SelMode <> smGroup
                    'can't paste into group 1 or group 9999
                    If Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 Then
                      .mnuEPaste.Enabled = False
                    End If
                  Case udwDelGroup
                    .mnuEPaste.Enabled = SelMode <> smNone
                  Case Else
                    'if real clipboard has something, enable pasting
                    'the paste function will validate whatever is there
                    .mnuEPaste.Enabled = Clipboard.GetFormat(vbCFText)
                  End Select
                Case wtByWord
                  .mnuEPaste.Enabled = False
                End Select
                .mnuEPaste.Caption = "&Paste" & vbTab & "Ctrl+V"
                'EXCEPT no pasting to group 1 or 9999
                If Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 Then
                  .mnuEPaste.Enabled = False
                End If
                Toolbar1.Buttons("paste").Enabled = .mnuEPaste.Enabled

                'delete same as cut
                .mnuEDelete.Visible = True
                .mnuEDelete.Enabled = .mnuECut.Enabled
                .mnuEDelete.Caption = "&Delete" & vbTab & "Del"
                Toolbar1.Buttons("delete").Enabled = .mnuECut.Enabled

                'clear
                .mnuEClear.Visible = True
                .mnuEClear.Enabled = (Mode = wtByGroup)
                .mnuEClear.Caption = "Clear Word List" & vbTab & "Shift+Del"

                'insert used to add new groups
                .mnuEInsert.Visible = True
                .mnuEInsert.Enabled = (Mode = wtByGroup)
                .mnuEInsert.Caption = "Insert &Group" & vbTab & "Ins"

                ' select-all used to add new words to active group
                .mnuESelectAll.Visible = True
                Select Case Mode
                Case wtByGroup
                  .mnuESelectAll.Enabled = lstGroups.ListIndex <> -1
                Case wtByWord
                  .mnuESelectAll.Enabled = False
                End Select
                .mnuESelectAll.Caption = "Insert &Word" & vbTab & "Shift+Ins"
                'if group 1 or 9999 insert only allowed if empty
                If Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 Then
                  If WordsEdit.GroupN(Val(lstGroups.Text)).WordCount > 0 Then
                    .mnuESelectAll.Enabled = False
                  End If
                End If

                'find, and find again depend on search status
                .mnuEBar1.Visible = True
                .mnuEFind.Visible = True
                .mnuEFind.Enabled = True
                .mnuEFind.Caption = "&Find" & vbTab & "Ctrl+F"
                .mnuEFindAgain.Visible = True
                .mnuEFindAgain.Enabled = (LenB(GFindText) <> 0)
                .mnuEFindAgain.Caption = "Find &Again" & vbTab & "F3"
                .mnuEReplace.Visible = True
                .mnuEReplace.Enabled = (Mode = wtByGroup)
                .mnuEReplace.Caption = "Replace" & vbTab & "Ctrl+H"

                'custom menu1 is used for edit word
                .mnuEBar2.Visible = True
                .mnuECustom1.Visible = True
                Select Case Mode
                Case wtByGroup
                  'enabled if a word selected
                  .mnuECustom1.Enabled = (SelMode = smWord) And lstWords.ListIndex <> -1
                Case wtByWord
                  .mnuECustom1.Enabled = False
                End Select
                'EXCEPT if group 0, 1, or 9999 placeholder
                If Val(lstGroups.Text) = 0 Or Val(lstGroups.Text) = 1 Or Val(lstGroups.Text) = 9999 Then
                  If WordsEdit.GroupN(Val(lstGroups.Text)).WordCount = 0 Then
                    .mnuECustom1.Enabled = False
                  End If
                End If
                .mnuECustom1.Caption = "Edit Word" & vbTab & "Alt+Enter"

                'custom menu2 is used for find-in-logic
                .mnuECustom2.Visible = True
                Select Case Mode
                Case wtByGroup
                  .mnuECustom2.Enabled = (SelMode <> smNone) And InGame
                Case wtByWord
                  .mnuECustom2.Enabled = False
                End Select
                .mnuECustom2.Caption = "Find in &Logics" & vbTab & "Shift+Ctrl+F"

                .mnuECustom4.Visible = False

                Toolbar1.Buttons("group").Enabled = (Mode = wtByGroup)
              End With

              'RARE - check for group 1 or 9999 - add word allowed if currently
              'there is not a word assigned
              Select Case Val(lstGroups.Text)
              Case 1, 9999
              lngGrpNo = Val(lstGroups.Text)
                ' also rare, but make sure group exists before counting words
                If Not WordsEdit.GroupExists(lngGrpNo) Then
                  blnAdd = True
                ElseIf (WordsEdit.GroupN(lngGrpNo).WordCount = 0) Then
                  blnAdd = True
                End If
                Toolbar1.Buttons("word").Enabled = blnAdd And (Mode = wtByGroup)
              Case Else
                Toolbar1.Buttons("word").Enabled = (Mode = wtByGroup)
              End Select

              Toolbar1.Buttons("findinlogic").Enabled = frmMDIMain.mnuECustom2.Enabled
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Sub Form_Load()

              CalcHeight = ScaleHeight
              If CalcWidth < MIN_WIDTH Then
                CalcHeight = MIN_HEIGHT
              End If

              CalcWidth = ScaleWidth
              If CalcWidth < MIN_WIDTH Then
                CalcWidth = MIN_WIDTH
              End If

            #If DEBUGMODE <> 1 Then
              'subclass the word list
              PrevWLBWndProc = SetWindowLong(lstWords.hWnd, GWL_WNDPROC, AddressOf LBWndProc)
              'subclass the group list
              PrevGLBWndProc = SetWindowLong(lstGroups.hWnd, GWL_WNDPROC, AddressOf LBWndProc)

              'subclass the text boxes
              PrevGrpTBWndProc = SetWindowLong(txtGrpNum.hWnd, GWL_WNDPROC, AddressOf TBWndProc)
            #End If
              'initialize undo collection
              Set UndoCol = New Collection

              'initialize object
              Set WordsEdit = New AGIWordList
              WordsEdit.NewWords

              'set split to be middle of form

              'setup fonts
              InitFonts
              'set code page to correct value
              rtfWord.CodePage = SessionCodePage

              'set width of splitter and hide icon
              picSplit.Width = SPLIT_WIDTH
              picSplitIcon.Width = SPLIT_WIDTH
              picSplitIcon.Visible = False

              'update panels
              UpdatePanels CalcWidth / 2

              SelGroup = -1
            End Sub

            Public Sub Clear(Optional ByVal DontUndo As Boolean = False, Optional ByVal DefaultWords As Boolean = False)

              Dim NextUndo As WordsUndo
              Dim i As Long, j As Long
              Dim strTemp As String

              'if not skipping undo
              If Not DontUndo Then
                Set NextUndo = New WordsUndo
                Set NextUndo.UDGroup = New StringList
                NextUndo.UDAction = udwClear
                'add each group of words to the undo stringlist, with groupnumber first, and words after
                'separated by a pipe character
                For i = 0 To WordsEdit.GroupCount - 1
                  'start with group number
                  strTemp = CStr(WordsEdit.Group(i).GroupNum)
                  'then add words
                  For j = 0 To WordsEdit.Group(i).WordCount - 1
                    strTemp = strTemp & "|" & WordsEdit.Group(i).Word(j)
                  Next j
                  'now add group to undo stringlist
                  NextUndo.UDGroup.Add strTemp
                Next i

                AddUndo NextUndo

                ' force logics refresh
                blnRefreshLogics = True
              End If

              'clear wordlist
              WordsEdit.Clear

              lstGroups.Clear
              lstWords.Clear

              'if adding default words
              If DefaultWords Then
                'add "a" and "rol" and "anyword"
                WordsEdit.AddWord "a", 0
                WordsEdit.AddWord "anyword", 1
                WordsEdit.AddWord "rol", 9999

                Select Case Mode
                Case wtByGroup
                  lstGroups.AddItem "0 - <null words>"
                  lstGroups.AddItem "1 - <any word>"
                  lstGroups.AddItem "9999 - <rest of line>"
                  'switch listindex twice to force it to update
                  lstGroups.ListIndex = 1
                  lstGroups.ListIndex = 0
                Case wtByWord
                End Select
              End If
            End Sub


            Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)

              Cancel = Not AskClose
            End Sub

            Sub Form_Resize()

              On Error GoTo ErrHandler

              'use separate variables for managing minimum width/height
              If ScaleWidth < MIN_WIDTH Then
                CalcWidth = MIN_WIDTH
              Else
                CalcWidth = ScaleWidth
              End If
              If ScaleHeight < MIN_HEIGHT Then
                CalcHeight = MIN_HEIGHT
              Else
                CalcHeight = ScaleHeight
              End If

              'if the form is not visible, or minimied
              If Not Visible Or WindowState = vbMinimized Then
                Exit Sub
              End If

              'if restoring from minimize, activation may not have triggered
              If MainStatusBar.Tag <> CStr(rtWords) Then
                ActivateActions
              End If

              'adjust height of splits
              picSplit.Height = CalcHeight
              picSplitIcon.Height = CalcHeight

              'ratio split between the list boxes
              'update panels
              If picSplit.Left * CalcWidth / (lstWords.Left + lstWords.Width + WE_MARGIN) < MIN_SPLIT_V Then
                UpdatePanels MIN_SPLIT_V
              Else
                UpdatePanels picSplit.Left * CalcWidth / (lstWords.Left + lstWords.Width + WE_MARGIN)
              End If
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub
            Private Sub UpdatePanels(ByVal SplitLoc As Long)

              On Error GoTo ErrHandler

              'adjust labels
              lblGroups.Width = SplitLoc - TBW
              lblWords.Left = SplitLoc + SPLIT_WIDTH
              lblWords.Width = CalcWidth - SplitLoc - SPLIT_WIDTH - WE_MARGIN

              'adjust group listbox
            #If DEBUGMODE <> 1 Then
              'disable window painting for the listbox until done
              SendMessage lstGroups.hWnd, WM_SETREDRAW, 0, 0
            #End If

              lstGroups.Height = CalcHeight - LGT - WE_MARGIN
              lstGroups.Width = SplitLoc - TBW

              'adjust words listbox
              lstWords.Height = lstGroups.Height
              lstWords.Left = SplitLoc + SPLIT_WIDTH
              lstWords.Width = lblWords.Width

              'position splitter
              picSplit.Left = SplitLoc
              picSplit.Height = CalcHeight - picSplit.Top

            #If DEBUGMODE <> 1 Then
              SendMessage lstGroups.hWnd, WM_SETREDRAW, 1, 0
            #End If
              lstGroups.Refresh

            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub
            Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

              'detect and respond to keyboard shortcuts

              'always check for help first
              If Shift = 0 And KeyCode = vbKeyF1 Then
                MenuClickHelp
                KeyCode = 0
                Exit Sub
              End If

              'if editing groupnum or word,
              If txtGrpNum.Visible Or rtfWord.Visible Then
                Exit Sub
              End If

              'check for global shortcut keys
              CheckShortcuts KeyCode, Shift
              If KeyCode = 0 Then
                Exit Sub
              End If

              Select Case Shift
              Case vbCtrlMask
                Select Case KeyCode
                Case vbKeyZ      'undo
                  If frmMDIMain.mnuEUndo.Enabled Then
                    MenuClickUndo
                    KeyCode = 0
                  End If

                Case vbKeyX  'cut
                  If frmMDIMain.mnuECut.Enabled Then
                    MenuClickCut
                    KeyCode = 0
                  End If

                Case vbKeyC  'copy
                  If frmMDIMain.mnuECopy.Enabled Then
                    MenuClickCopy
                    KeyCode = 0
                  End If

                Case vbKeyV  'paste
                  If frmMDIMain.mnuEPaste.Enabled Then
                    MenuClickPaste
                    KeyCode = 0
                  End If

                Case vbKeyF
                  'find
                  If frmMDIMain.mnuEFind.Enabled Then
                    MenuClickFind
                    KeyCode = 0
                  End If

                Case vbKeyH
                  'replace
                  If frmMDIMain.mnuEReplace.Enabled Then
                    MenuClickReplace
                    KeyCode = 0
                  End If
                End Select

              Case 0 'no shift, ctrl, alt
                Select Case KeyCode
                Case vbKeyInsert
                  If frmMDIMain.mnuEInsert.Enabled Then
                    MenuClickInsert
                    KeyCode = 0
                  End If

                Case vbKeyDelete
                  If frmMDIMain.mnuEDelete.Enabled Then
                    MenuClickDelete
                    KeyCode = 0
                  End If

                Case vbKeyF3
                  'find again
                  If frmMDIMain.mnuEFindAgain.Enabled Then
                    MenuClickFindAgain
                    KeyCode = 0
                  End If
                End Select

              Case vbShiftMask
                Select Case KeyCode
                Case vbKeyDelete
                  If frmMDIMain.mnuEClear.Enabled Then
                    MenuClickClear
                    KeyCode = 0
                  End If

                Case vbKeyInsert
                  If frmMDIMain.mnuESelectAll.Enabled Then
                    MenuClickSelectAll
                    KeyCode = 0
                  End If
                End Select

              Case 3  'shift+ctrl
                Select Case KeyCode
                Case vbKeyF
                  If frmMDIMain.mnuECustom2.Enabled Then
                    MenuClickECustom2
                    KeyCode = 0
                  End If
                End Select

              Case vbAltMask
                Select Case KeyCode
                Case vbKeyReturn
                  If frmMDIMain.mnuECustom1.Enabled Then
                    MenuClickECustom1
                    KeyCode = 0
                  End If

                Case vbKeyF
                  If frmMDIMain.mnuRCustom1.Enabled Then
                    MenuClickCustom1
                    KeyCode = 0
                  End If
                End Select
              End Select
            End Sub
            Private Sub Form_Unload(Cancel As Integer)

              On Error GoTo ErrHandler

              'ensure edit object is dereferenced
              If Not WordsEdit Is Nothing Then
                '*'Debug.Assert WordsEdit.Loaded
                WordsEdit.Unload
              End If

              'if this is the ingame list
              If InGame Then
                'reset inuse flag
                WEInUse = False
                'release the object
                Set WordEditor = Nothing
              End If

              'always reset the synonym search; it
              'only works when the editor is open
              GFindSynonym = False

            #If DEBUGMODE <> 1 Then
              'release subclass hook to listboxes
              SetWindowLong lstWords.hWnd, GWL_WNDPROC, PrevWLBWndProc
              SetWindowLong lstGroups.hWnd, GWL_WNDPROC, PrevGLBWndProc
              'and text boxes
              SetWindowLong txtGrpNum.hWnd, GWL_WNDPROC, PrevGrpTBWndProc
            #End If

              'need to check if this is last form
              LastForm Me
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Public Sub lstWords_Click()

              On Error GoTo ErrHandler

              Select Case Mode
              Case wtByGroup
                'if something is selected
                If lstWords.ListIndex <> -1 Then
                  If SelMode <> smWord Then
                    'reset mode
                    SelMode = smWord
                    lstWords.SetFocus
                  End If
                  SetEditMenu
                End If

              Case wtByWord
                'changing word needs to update the group
                lstGroups.List(0) = CStr(WordsEdit(lstWords.ListIndex).Group)
              End Select

            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Sub lstWords_DblClick()

              Select Case Mode
              Case wtByGroup
                'same as clicking editword menu
                MenuClickECustom1
              Case wtByWord
              End Select
            End Sub

            Private Sub lstWords_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

              Dim lngIndex As Long

              On Error GoTo ErrHandler

              'save x and y
              mX = X
              mY = Y

              'ensure lstWord has focus
              lstWords.SetFocus

              'if clicking with right button
              If Button = vbRightButton Then
                'select the item that the cursor is over
                lngIndex = (Y / ScreenTWIPSY \ lngRowHeight) + lstWords.TopIndex

                'if something clicked,
                If lngIndex <= lstWords.ListCount - 1 Then
                  'select the item clicked
                  lstWords.ListIndex = lngIndex
                End If

                'if mode is not word
                If SelMode <> smWord Then
                  'reset mode
                  SelMode = smWord
                End If
                SetEditMenu

                'make sure this form is the active form
                If Not (frmMDIMain.ActiveMdiChild Is Me) Then
                  'set focus before showing the menu
                  Me.SetFocus
                End If
                'need doevents so form activation occurs BEFORE popup
                'otherwise, errors will be generated because of menu
                'adjustments that are made in the form_activate event
                SafeDoEvents
                'show edit menu
                PopupMenu frmMDIMain.mnuEdit, , X / ScreenTWIPSX + picSplit.Left, Y / ScreenTWIPSY
              End If
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Sub Toolbar1_ButtonClick(ByVal Button As MSComctlLib.Button)

              Dim i As Long

              Select Case Button.Key
              Case "undo"
                MenuClickUndo

              Case "mode"
                'toggle mode
                MenuClickECustom3

              Case "cut"
                MenuClickCut

              Case "copy"
                MenuClickCopy

              Case "paste"
                MenuClickPaste

              Case "delete"
                MenuClickDelete

              Case "group"
                'add a new group
                NewGroup

              Case "word"
                'add a new word
                NewWord

              Case "find"
                MenuClickFind

              Case "findinlogic"
                MenuClickECustom2
              End Select

            End Sub

            Private Sub txtGrpNum_KeyDown(KeyCode As Integer, Shift As Integer)

              Dim strCBText As String, blnPasteOK As Boolean

              On Error GoTo ErrHandler

              'need to handle cut, copy, paste, select all shortcuts
              Select Case Shift
              Case vbCtrlMask
                Select Case KeyCode
                Case vbKeyX 'cut
                  'only is something selected
                  If txtGrpNum.SelLength > 0 Then
                    'put the selected text into clipboard
                    Clipboard.Clear
                    Clipboard.SetText txtGrpNum.SelText
                    'then delete it
                    txtGrpNum.SelText = ""
                  End If
                  KeyCode = 0
                  Shift = 0

                Case vbKeyC 'copy
                  'only is something selected
                  If txtGrpNum.SelLength > 0 Then
                    'put the selected text into clipboard
                    Clipboard.Clear
                    Clipboard.SetText txtGrpNum.SelText
                  End If
                  KeyCode = 0
                  Shift = 0

                Case vbKeyV 'paste
                  'paste only allowed if clipboard text is a valid number
                  strCBText = Clipboard.GetText
                  ' put a zero in front, just in case it's a hex or octal
                  ' string; we don't want those
                  If IsNumeric("0" & strCBText) And Len(strCBText) > 0 Then
                    'only integers
                    If Int(strCBText) = Val(strCBText) Then
                      'range 0-65535
                      If Val(strCBText) >= 0 And Val(strCBText) <= 65535 Then
                        blnPasteOK = True
                      Else
                        blnPasteOK = False
                      End If
                    Else
                      blnPasteOK = False
                    End If
                  Else
                    blnPasteOK = False
                  End If

                  If blnPasteOK Then
                    'put cbtext into selection
                    txtGrpNum.SelText = Val(strCBText)
                  End If
                  KeyCode = 0
                  Shift = 0

                Case vbKeyA 'select all
                  txtGrpNum.SelStart = 0
                  txtGrpNum.SelLength = Len(txtGrpNum.Text)
                  KeyCode = 0
                  Shift = 0
                End Select
              End Select
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Sub txtGrpNum_KeyPress(KeyAscii As Integer)

              On Error GoTo ErrHandler

              'only numbers , backspace
              Select Case KeyAscii
              Case 48 To 57, 8
                'ok

              Case 9, 10, 13 'return (tab mimics enter key on this form)

                If ValidateGrpNum() Then
                  'hide the box
                  txtGrpNum.Visible = False
                  lstGroups.SetFocus
                  'reenable edit menu
                  frmMDIMain.mnuEdit.Enabled = True
                  ' and editing toolbar buttons
                  With Me.Toolbar1.Buttons
                    .Item(1).Enabled = True
                    .Item(2).Enabled = True
                    .Item(3).Enabled = True
                    .Item(4).Enabled = True
                    .Item(6).Enabled = True
                    .Item(7).Enabled = True
                  End With

                Else
                  'need to force focus (might be a tab thing?)
                  txtGrpNum.SetFocus
                End If

                KeyAscii = 0

              Case 27 'escape
                txtGrpNum.Visible = False
                lstGroups.SetFocus
                KeyAscii = 0
                'reenable edit menu
                frmMDIMain.mnuEdit.Enabled = True
                ' and editing toolbar buttons
                With Me.Toolbar1.Buttons
                  .Item(1).Enabled = True
                  .Item(2).Enabled = True
                  .Item(3).Enabled = True
                  .Item(4).Enabled = True
                  .Item(6).Enabled = True
                  .Item(7).Enabled = True
                End With

              Case Else
                ' this also kills shortcuts like cut/paste
                KeyAscii = 0
              End Select
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Public Sub txtGrpNum_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)


              Dim strCBText As String

              On Error GoTo ErrHandler

              ' check for right click to show context menu
              If Button = vbRightButton Then
                'configure the edit menu
                With frmMDIMain
                  .mnuTBCopy.Enabled = txtGrpNum.SelLength > 0
                  .mnuTBCut.Enabled = .mnuTBCopy.Enabled
                  'paste only allowed if clipboard text is a valid number
                  strCBText = Clipboard.GetText
                  ' put a zero in front, just in case it's a hex or octal
                  ' string; we don't want those
                  If IsNumeric("0" & strCBText) And Len(strCBText) > 0 Then
                    'only integers
                    If Int(strCBText) = Val(strCBText) Then
                      'range 0-65535
                      If Val(strCBText) >= 0 And Val(strCBText) <= 65535 Then
                        .mnuTBPaste.Enabled = True
                      Else
                        .mnuTBPaste.Enabled = False
                      End If
                    Else
                      .mnuTBPaste.Enabled = False
                    End If
                  Else
                    .mnuTBPaste.Enabled = False
                  End If

                  ' set the command operation to none
                  TBCmd = 0

                  'now show the popup menu
                  PopupMenu .mnuTBPopup
                End With

                'deal with the result
                Select Case TBCmd
                Case 0 'canceled
                  'do nothing
                  Exit Sub
                Case 1 'cut
                  'put the selected text into clipboard
                  Clipboard.Clear
                  Clipboard.SetText txtGrpNum.SelText
                  'then delete it
                  txtGrpNum.SelText = ""
                Case 2 'copy
                  'put the selected text into clipboard
                  Clipboard.Clear
                  Clipboard.SetText txtGrpNum.SelText
                Case 3 'paste
                  'put cbtext into selection
                  txtGrpNum.SelText = strCBText
                Case 4 'select all
                  txtGrpNum.SelStart = 0
                  txtGrpNum.SelLength = Len(txtGrpNum.Text)
                End Select
              End If
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Sub txtGrpNum_Validate(Cancel As Boolean)

              'this will handle cases where user tries to 'click' on something,
              'but NOT when keys are pressed?

              On Error GoTo ErrHandler
              If Not txtGrpNum.Visible Then Exit Sub

              'if OK, hide the text box
              If ValidateGrpNum() Then
                txtGrpNum.Visible = False

                'reenable edit menu
                frmMDIMain.mnuEdit.Enabled = True
                ' and editing toolbar buttons
                With Me.Toolbar1.Buttons
                  .Item(1).Enabled = True
                  .Item(2).Enabled = True
                  .Item(3).Enabled = True
                  .Item(4).Enabled = True
                  .Item(6).Enabled = True
                  .Item(7).Enabled = True
                End With
              Else
              'if not OK, cancel
                Cancel = True
              End If
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Sub rtfWord_KeyDown(KeyCode As Integer, Shift As Integer)

              Dim strCBText As String

              On Error GoTo ErrHandler

              'ignore enter key
              If KeyCode = 13 Then KeyCode = 0

              'always check for help first
              If Shift = 0 And KeyCode = vbKeyF1 Then
                MenuClickHelp
                KeyCode = 0
                Exit Sub
              End If

              'need to handle cut, copy, paste, select all shortcuts
              Select Case Shift
              Case vbCtrlMask
                Select Case KeyCode
                Case vbKeyX 'cut
                  'only is something selected
                  If rtfWord.Selection.Range.Length > 0 Then
                    'put the selected text into clipboard
                    rtfWord.Selection.Range.Cut
                  End If
                  KeyCode = 0
                  Shift = 0

                Case vbKeyC 'copy
                  'only is something selected
                  If rtfWord.Selection.Range.Length > 0 Then
                    'put the selected text into clipboard
                    rtfWord.Selection.Range.Copy
                  End If
                  KeyCode = 0
                  Shift = 0

                Case vbKeyV 'paste
                  ' trim white space off clipboard text, and no multi-line text
                  strCBText = Trim$(Clipboard.GetText)
                  strCBText = Replace(strCBText, vbCr, "")
                  strCBText = Replace(strCBText, vbLf, "")
                  'paste only allowed if clipboard has text
                  If Len(strCBText) > 0 Then
                      'any other invalid characters will have to be caught
                      'by the validation check

                      'put cbtext into selection (force to lower case)
                      rtfWord.Selection.Range.Text = LCase$(CPToUnicode(strCBText, SessionCodePage))
                      rtfWord.Selection.Range.Collapse reEnd
                  End If
                  KeyCode = 0
                  Shift = 0

                Case vbKeyA 'select all
                  rtfWord.Selection.Range.StartPos = 0
                  rtfWord.Selection.Range.EndPos = Len(rtfWord.Text)
                  KeyCode = 0
                  Shift = 0

                Case vbKeyInsert
                  'set flag so other controls know charpicker is active
                  PickChar = True
                  ShowCharPickerForm rtfWord
                  KeyCode = 0
                  Shift = 0
                  'done with charpicker
                  PickChar = False
                End Select
              End Select
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Sub rtfWord_KeyPress(KeyAscii As Integer)

              Dim EditNewWord As String

              On Error GoTo ErrHandler

              Select Case KeyAscii
              Case 97 To 122, 8
              '  'a-z and backspace always ok
              Case 65 To 90
                'A-Z converted to lowercase
                KeyAscii = KeyAscii + 32

              Case 32 'space
                'not allowed for first character
                If LenB(rtfWord.Text) = 0 Then
                  KeyAscii = 0
                End If

              Case 33, 34, 39, 40, 41, 44, 45, 46, 58, 59, 63, 91, 93, 96, 123, 125
                '    !'(),-.:;?[]`{}
                'NEVER allowed; these values get removed by the input function
                KeyAscii = 0

              Case 35 To 38, 42, 43, 47 To 57, 60 To 62, 64, 92, 94, 95, 124, 126, 127
                'these characters:
                '    #$%&*+/0123456789<=>@\^_|~
                'NOT allowed as first char
                If LenB(rtfWord.Text) = 0 Then
                  'UNLESS supporting Power Pack mod
                  If Not PowerPack Then
                    KeyAscii = 0
                  End If
                End If

              Case 9, 10, 13 'enter or tab (tab mimics enter key on this form)
                EditNewWord = UnicodeToCP(LCase$(rtfWord.Text), SessionCodePage)
                If ValidateWord(EditNewWord) Then
                  FinishEdit EditOldWord, EditNewWord, False
                Else
                  'need to force focus (might be a tab thing?)
                  With rtfWord
                    .Selection.Range.StartPos = 0
                    .Selection.Range.EndPos = Len(.Text)
                    .SetFocus
                  End With
                End If

                KeyAscii = 0

              Case 27   'ESC'
                'cancel
                rtfWord.Visible = False
                lstWords.SetFocus
                'reenable edit menu
                frmMDIMain.mnuEdit.Enabled = True
                ' and editing toolbar buttons
                With Me.Toolbar1.Buttons
                  .Item(1).Enabled = True
                  .Item(2).Enabled = True
                  .Item(3).Enabled = True
                  .Item(4).Enabled = True
                  .Item(6).Enabled = True
                  .Item(7).Enabled = True
                End With
                KeyAscii = 0

              Case Else
                'extended chars not allowed
                'UNLESS supporting the Power Pack mod
                If Not PowerPack Then
                  KeyAscii = 0
                End If
              End Select
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Sub FinishEdit(OldWord As String, NewWord As String, Optional ByVal DontUndo As Boolean = False)

              On Error GoTo ErrHandler

              'hide the textbox
              rtfWord.Visible = False
              'and save the new word
              EditWord OldWord, NewWord, DontUndo

              Select Case Mode
              Case wtByGroup
                'reselect the group
                UpdateWordList True

                'reselect the word
                lstWords.Text = CPToUnicode(NewWord, SessionCodePage)
                lstWords.SetFocus
              Case wtByWord
              End Select

              'reenable edit menu
              frmMDIMain.mnuEdit.Enabled = True
              ' and editing toolbar buttons
              With Me.Toolbar1.Buttons
                .Item(1).Enabled = True
                .Item(2).Enabled = True
                .Item(3).Enabled = True
                .Item(4).Enabled = True
                .Item(6).Enabled = True
                .Item(7).Enabled = True
              End With

              'always clear new flags
              AddNewWord = False
              AddNewGroup = False
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub
            Private Sub rtfWord_MouseDown(Button As Integer, Shift As Integer, X As Long, Y As Long, LinkRange As RichEditAGI.Range)

              Dim strCBText As String

              On Error GoTo ErrHandler

              ' check for right click to show context menu
              If Button = vbRightButton Then
                'configure the edit menu
                With frmMDIMain
                  .mnuTBCopy.Enabled = rtfWord.Selection.Range.Length > 0
                  .mnuTBCut.Enabled = .mnuTBCopy.Enabled
                  ' put cbtext into selection
                  ' trim white space off clipboard text, and no multi-line text
                  strCBText = Trim$(Clipboard.GetText)
                  strCBText = Replace(strCBText, vbCr, "")
                  strCBText = Replace(strCBText, vbLf, "")
                  'paste only allowed if clipboard has text
                  If Len(strCBText) > 0 Then
                    'any other invalid characters will have to be caught
                    'by the validation check
                    .mnuTBPaste.Enabled = True
                  Else
                    .mnuTBPaste.Enabled = False
                  End If

                  'char picker available if PowerPack is enabled
                  .mnuTBSeparator1.Visible = PowerPack
                  .mnuTBCharMap.Visible = PowerPack

                  ' set the command operation to none
                  TBCmd = 0

                  'now show the popup menu
                  PopupMenu .mnuTBPopup
                End With

                'deal with the result
                Select Case TBCmd
                Case 0 'canceled
                  'do nothing
                  Exit Sub
                Case 1 'cut
                  'put the selected text into clipboard
                  rtfWord.Selection.Range.Cut
                Case 2 'copy
                  'put the selected text into clipboard
                  rtfWord.Selection.Range.Copy
                Case 3 'paste
                  'put cbtext(converted from cb-byte) into selection (force lower case)
                  rtfWord.Selection.Range.Text = LCase$(CPToUnicode(strCBText, SessionCodePage))
                  rtfWord.Selection.Range.Collapse reEnd
                Case 4 'select all
                  rtfWord.Selection.Range.StartPos = 0
                  rtfWord.Selection.Range.EndPos = Len(rtfWord.Text)
                Case 5 ' show char picker
                  'set flag so other controls know charpicker is active
                  PickChar = True
                  ShowCharPickerForm rtfWord
                  'done with charpicker
                  PickChar = False
                End Select
              End If
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub


            Private Sub rtfWord_Validate(Cancel As Boolean)

              'this will handle cases where user tries to 'click' on something,
              'but NOT when keys are pressed?
              Dim tmpWord As String

              On Error GoTo ErrHandler
              If Not rtfWord.Visible Then Exit Sub

              tmpWord = UnicodeToCP(LCase$(rtfWord.Text), SessionCodePage)
              If ValidateWord(tmpWord) Then
                FinishEdit EditOldWord, tmpWord, False
              Else
              'if not OK, cancel
                Cancel = True
                With rtfWord
                  .Selection.Range.StartPos = 0
                  .Selection.Range.EndPos = Len(.Text)
                End With
              End If
            Exit Sub

            ErrHandler:
              '*'Debug.Assert False
              Resume Next
            End Sub

            Private Function CurrentWord() As String
              'gets current word from WordsEdit determined by current
              'group listbox and word listbox selections

              'validate something selected first
              If lstGroups.ListIndex = -1 Then
                Exit Function
              End If
              If lstWords.ListIndex = -1 Then
                Exit Function
              End If

              'return the word
              CurrentWord = WordsEdit.GroupN(Val(lstGroups.Text)).Word(lstWords.ListIndex)
            End Function

                        */
        }

        #endregion

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


            // TODO: set form up for editing
            lstGroups.Items.Clear();
            for (int i = 0; i < EditWordList.GroupCount; i++) {
                lstGroups.Items.Add((EditWordList.Group(i).GroupNum.ToString() + ":").PadRight(6) + EditWordList.Group(i).GroupName);
            } 
            lstGroups.SelectedIndex = 0;
            lblGroupCount.Text = "Group Count: " + EditWordList.GroupCount;
            lblWordCount.Text = "Word Count: " + EditWordList.WordCount;
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
                label2.Text = $"Group 2 Name: {EditWordList.GroupN(2).GroupName}";
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

        internal void InitFonts() {
            // TODO: after finalizing form layout, need to adjust font init
            lstGroups.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
            lstWords.Font = new Font(WinAGISettings.EditorFontName.Value, WinAGISettings.EditorFontSize.Value);
        }
    }
}
