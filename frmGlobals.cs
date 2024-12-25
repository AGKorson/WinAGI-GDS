using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using WinAGI.Common;
using static WinAGI.Common.API;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Engine.Base;
using static WinAGI.Engine.Commands;
using static WinAGI.Engine.AGIResType;
using static WinAGI.Engine.ArgType;
using static WinAGI.Editor.Base;
using System.IO;

namespace WinAGI.Editor {
    public partial class frmGlobals : Form {
        public bool InGame = false;
        public string FileName = "";
        public bool IsChanged = false;
        bool blnDuplicates = false, blnReserved = false;
        private int EditRow;
        private ColType EditCol;
        private TDefine EditDefine, RepDefine;
        private bool Inserting, MustEditValue;
        private GlobalsUndo[] UndoCol;
        private const string DEF_MARKER = "#define ";


        private enum ColType {
            ctDefault = 0,
            ctName = 1,
            ctValue = 2,
            ctComment = 3
        }

        // a blank, default globals editor
        public frmGlobals() {
            InitializeComponent();
            MdiParent = MDIMain;
            InitFonts();
        }

        //public frmGlobals(string name) {
        //    InitializeComponent();
        //    MdiParent = MDIMain;
        //    InGame = true;
        //    // loading function will handle any errors
        //    LoadGlobalDefines(name);
        //}
        #region Event Handlers
        #endregion

        /// <summary>
        /// loads the current global defines list into the grid for editing
        /// </summary>
        /// <param name="GlobalFile"></param>
        /// <param name="ClearAll"></param>
        public bool LoadGlobalDefines(string GlobalFile, bool ClearAll = true) {
            int lngStartRow, lngInsertRow, Index, intFile;
            bool blnTry, blnError;
            string strComment = "", strLine;
            string[] strSplitLine;
            StringList stlGlobals;
            TDefine[] tmpDefines = [];
            GlobalsUndo NextUndo;

            // Index is used to track the location where the next
            // insertion should occur;
            // lngInsertRow is what we pass to the validate function;
            // if the define gets inserted, then lngInsertRow doesn't
            // change
            //
            // if the define is a replacement, then lngInsertRow is
            // set to the old line's value, and the new value is
            // inserted either at Index (if the line being replaced
            // comes AFTER Index) or at Index-1 (if the line being
            // replaced comes BEFORE Index)

            // line format should be
            //  {#define name value [ comment} OR
            //  {#define name value} OR
            //  {name tab value}

            // no edit row
            EditRow = -1;


            TDefine[] EditGlobals = [];
            TDefine tmpDef = new();
            int NumDefs = 0;
            try {
                // if no global file, just exit
                if (!File.Exists(GlobalFile)) {
                    return false;
                }
                //open file for input
                using FileStream fsGlobal = new(GlobalFile, FileMode.Open);
                using StreamReader srGlobal = new(fsGlobal);

                //read in globals
                while (!srGlobal.EndOfStream) {
                    //get line
                    strLine = srGlobal.ReadLine();
                    //trim it - also, skip comments
                    string cmt = "";
                    strLine = StripComments(strLine, ref cmt, true);
                    //ignore blanks
                    if (strLine.Length != 0) {
                        // TODO: ditch support for old format
                        //even though new format is to match standard #define format,
                        //still need to look for old format first just in case;
                        //when saved, the file will be in the new format

                        //assume not valid until we prove otherwise
                        blnTry = false;

                        //splitline into name and Value
                        strSplitLine = strLine.Split((char)Keys.Tab);

                        //if exactly two elements,
                        if (strSplitLine.Length == 2) {
                            tmpDef.Name = strSplitLine[0].Trim();
                            tmpDef.Value = strSplitLine[1].Trim();
                            blnTry = true;

                            //not a valid global.txt; check for defines.txt
                        }
                        else {
                            //tabs need to be replaced with spaces first
                            strLine = strLine.Replace((char)Keys.Tab, ' ').Trim();
                            if (strLine.Left(8).Equals("#define ", StringComparison.OrdinalIgnoreCase)) {
                                //strip off the define statement
                                strLine = strLine.Right(strLine.Length - 8).Trim();
                                //extract define name
                                int i = strLine.IndexOf(' ');
                                if (i > 0) {
                                    tmpDef.Name = strLine.Left(i);
                                    strLine = strLine.Right(strLine.Length - i);
                                    tmpDef.Value = strLine.Trim();
                                    blnTry = true;
                                }
                            }
                        }

                        //if the line contains a define, add it to list
                        //here we don't bother validating; if it's a bad
                        //define, then user will have to deal with it;
                        //it's only a tooltip at this point
                        if (blnTry) {
                            tmpDef.Type = DefTypeFromValue(tmpDef.Value);
                            //increment count
                            NumDefs++;
                            Array.Resize(ref EditGlobals, NumDefs);
                            EditGlobals[NumDefs - 1] = tmpDef;
                        }
                    }
                }
                //close file
                fsGlobal.Dispose();
                srGlobal.Dispose();
            }
            catch (Exception) {
                //if error opening file, just exit
                return false;
            }
            Text = "Global Defines Editor";
            for (int i = 0; i < EditGlobals.Length; i++) {
                lstGlobals.Items.Add(EditGlobals[i].Name + "    " + EditGlobals[i].Value + "     " + EditGlobals[i].Comment);
            }


            /*
            if (ClearAll) {
                // clear the flexgrid
                ClearGlobalDefines();
            }
            else {
                // remove last blank row (makes it easier to add new entries
                // by just appending them to end of the list)
                if (fgGlobals.TextMatrix(fgGlobals.Rows - 1, ColType.ctName) == "") {
                    if (fgGlobals.Rows == 2) {
                        // if this is last row, clear it
                        ClearGlobalDefines();
                    }
                }
                else {
                    fgGlobals.RemoveItem(fgGlobals.Rows - 1);
                }
                // note the starting row so we can tell if nothing got inserted
                // (it's one past last row because we deleted the blank row
                // that's normally there)
                lngStartRow = fgGlobals.Rows;

                if (WinAGISettings.GlobalUndo != 0) {
                    // setup the undo object
                    NextUndo = new(GlobalsUndo);
                    NextUndo.UDAction = udgImportDefines;
                    NextUndo.UDPos = lngStartRow;
                    tmpDefines = [];
                }
            }

            // always try to add the line to the end
            Index = fgGlobals.Rows;
            // TODO: open file for input
            stlGlobals = textfilereader(GlobalFile);
            // read in globals
            int CurLine = 0;
            while (CurLine < stlGlobals.Count) {
                // get next line
                strLine = stlGlobals[CurLine++];
                strLine = StripComments(strLine, strComment);
                // ignore blanks
                if (strLine.Length != 0) {
                    // even though new format is to match standard #define format,
                    // still need to look for old format first just in case;
                    // when saved, the file will be in the new format

                    // assume not valid until we prove otherwise
                    blnTry = false;
                    // splitline into name and Value
                    strSplitLine = strLine.Split('\t');

                    // if exactly two elements
                    if (strSplitLine.Length == 2) {
                        strSplitLine[0] = strSplitLine[0].Trim();
                        strSplitLine[1] = strSplitLine[1].Trim();
                        blnTry = true;
                    }
                    else {
                        // not a valid global.txt; check for defines.txt
                        // tabs need to be replaced with spaces first
                        strLine = strLine.Replace('\t', ' ').Trim();
                        if (Left(strLine, 8) == DEF_MARKER) {
                            // set up splitline array
                            strSplitLine = new string[2];
                            // strip off the define statement
                            strLine = Right(strLine, strLine.Length - 8).Trim();
                            // extract define name
                            i = strLine.IndexOf(' ');
                            if (i != 0) {
                                strSplitLine[0] = Left(strLine, i - 1);
                              strLine = Right(strLine, strLine.Length - i).Trim();
                                strSplitLine[1] = strLine;
                                blnTry = true;
                            }
                        }
                    }

                    // if the line contains a define, validate and add it to grid
                    if (blnTry) {
                        // ''''''''''''''''''''''''''''''''''''
                        //  this section nearly identical to
                        //  the code in MenuClickPaste sub;
                        // should consider a single function
                        // (AddDefine); needs some work on
                        // differences though; probably not
                        // worth the effort
                        // ''''''''''''''''''''''''''''''''''''
                        lngInsertRow = Index;
                        if (ValidateDefine(strSplitLine[0], strSplitLine[1], false, lngInsertRow)) {
                            // if there is a comment, add it
                            if (strComment.Length > 0) {
                                fgGlobals.TextMatrix(lngInsertRow, ColType.ctComment) = strComment;
                            }
                            // if appending to the current list,
                            // we need to deal with a possible undo action
                            if (WinAGISettings.GlobalUndo != 0 && !ClearAll) {
                                // if adding undo, we need to document what kind of
                                // "add" was done; either an insert, or a replacement

                                // ReDim Preserve tmpDefines(UBound(tmpDefines) + 1);

                                // was it a replace? we can tell by checking lngInsertRow against Index
                                if (lngInsertRow != Index) {
                                    // it was a replacement - 
                                    // save the define information that was replaced
                                    tmpDefines[tmpDefines.Length - 1] = RepDefine;
                                    // use type field to save the row removed and line inserted
                                    tmpDefines[tmpDefines.Length - 1].Type = ((ArgTypeEnum)(lngInsertRow * 10000));
                                    // and the index line were replacement was added
                                    // (it will depend on whether or not the replaced
                                    // line was before or after the insert position)
                                    if (lngInsertRow < Index) {
                                        tmpDefines[tmpDefines.Length - 1].Type = tmpDefines[tmpDefines.Length - 1].Type + Index - 1;
                                    }
                                    else {
                                        tmpDefines[tmpDefines.Length - 1].Type = tmpDefines[tmpDefines.Length - 1].Type + Index;
                                    }
                                    // replacements always delete a row, so we need to
                                    // adjust the starting line
                                    lngStartRow--;
                                }
                                else {
                                    // if just inserting, we only care about the position
                                    tmpDefines[tmpDefines.Length - 1].Type = (ArgTypeEnum)Index;
                                }
                            }
                            // if something was actually added (lngInsertRow didn't change)
                            if (lngInsertRow == Index) {
                                // increment index
                                Index++;
                            }
                        }

                        // ''''''''''''''''''''''''''''''''''''
                        // done with try
                    }
                    // done with blank line
                }
            }
            // always add one blank line to bottom
            fgGlobals.AddItem("");

            
            // if clearing all (loading fresh)
            if (ClearAll) {
                // if something was added, select the first item
                if (fgGlobals.Rows > 1) {
                    // select name cel of first non-readonly row
                    fgGlobals.Row = 1;
                    fgGlobals.Col = ColType.ctName;
                }
                // list is clean
                IsChanged = false;
                FileName = GlobalFile;
                // set caption
                this.Text = "Global Defines - ";
                if (EditGame != null) {
                    Text += EditGame.GameID;
                }
                else {
                    Text += FileName;
                }
            }
            else {
                // if anything was added, note it
                if (fgGlobals.Rows > lngStartRow + 1) {
                    // mark as changed
                    MarkAsChanged();
                    // ensure selected cell is visible
                    fgGlobals.Row = lngStartRow;
                    if (!fgGlobals.RowIsVisible(lngStartRow)) {
                        fgGlobals.TopRow = lngStartRow;
                    }
                    // and add the undo object
                    if (WinAGISettings.GlobalUndo != 0) {
                        NextUndo.UDCount = fgGlobals.Rows - lngStartRow - 1;
                        // if any defines were replaced, we need to add them to the undo object so
                        // they can be restored during the undo process
                        if (tmpDefines.Length > 0) {
                            for (i = 1; i < tmpDefines.Length; i++) {
                                NextUndo.UDDefine[i - 1] = tmpDefines[i];
                            }
                        }
                        AddUndo(NextUndo);
                    }
                }
                else {
                    // unless there were duplicates, nothing was added
                    if (!blnDuplicates) {
                        MessageBox.Show(MDIMain, "There were no valid defines in the import file. No changes made to this defines list.", "No Defines Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }

            // if errors
            if (blnError) {
                // inform user
                MessageBox.Show(MDIMain, "An error occurred while loading the global defines file. Some entries may be missing or incorrect.", "Global Defines File Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // ensure at least one row
                if (fgGlobals.Rows == 1) {
                    fgGlobals.AddItem("");
                }
            }
            // resize to ensure widths adjusted depending on whether
            // or not scroll bar is visible
            // Form_Resize

            // if a defines.txt file was loaded, and warnings were generated,
            if (blnDuplicates) {
                // reset cursor
                MDIMain.UseWaitCursor = false;
                MessageBox.Show(MDIMain, "Some names in this global defines file had duplicate \n" +
                            "definitions. Duplicate names have been replaced.", "Open Global Defines",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            //ErrHandler:
            //  blnError = True
            //  Resume Next

            */
            return true;
        }

        public void MenuClickSave(bool Exporting = false) {
            /*
  'save list of globals
  
  Dim strTempFile As String, stlGlobals As StringList
  Dim intFile As Integer
  Dim i As Long
  Dim tmpNode As Node, rtn As VbMsgBoxResult, blnDontAsk As Boolean
  Dim tmpListItem As ListItem
  Dim blnError As Boolean, blnChanged As Boolean
  
  On Error GoTo ErrHandler
  
  'if editing
  If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
    Exit Sub
  End If
  
  'if this is a loaded games's list
  If InGame Then
    'replace any changed defines with new names
    Select Case Settings.AutoUpdateDefines
    Case 0
      'get user's response
      rtn = MsgBoxEx("Do you want to update all logics with any define names that have been changed?", vbQuestion + vbYesNo, "Update Logics?", WinAGIHelp, "htm\winagi\editingdefines.htm#edit", "Always take this action when saving the global defines list.", blnDontAsk)
      If blnDontAsk Then
        If rtn = vbYes Then
          Settings.AutoUpdateDefines = 2
        Else
          Settings.AutoUpdateDefines = 1
        End If
        WinAGISettingsList.WriteSetting(sLOGICS, "AutoUpdateDefines", Settings.AutoUpdateDefines
      End If
    Case 1
      rtn = vbNo
    Case 2
      rtn = vbYes
    End Select

    WaitCursor
    
    'if yes,
    If rtn = vbYes Then
      'step through all defines; if the current name is different than
      'the original name, use replaceall to make the change
      Load frmProgress
      frmProgress.pgbStatus.Max = fgGlobals.Rows
      frmProgress.pgbStatus.Value = 0
      frmProgress.Caption = "Save Global Defines"
      frmProgress.lblProgress.Caption = "Locating modified define names"
      frmProgress.Show vbModeless, frmMDIMain
      SafeDoEvents
      
      With fgGlobals
        For i = 1 To .Rows - 2
          frmProgress.pgbStatus.Value = i
          frmProgress.lblProgress.Caption = "checking " & .TextMatrix(i, 1) & " ..."
          'refresh on each iteration
          If Len(.TextMatrix(i, 2)) > 0 Then
            'if the original name (col0) is different from current name (col1)
            'then we need to replace it
            If .TextMatrix(i, 0) <> .TextMatrix(i, 1) Then
              'mark changed
              blnChanged = True
              'new entries won't have a previous value to update
              If Len(.TextMatrix(i, 0)) > 0 Then
                WaitCursor
                'always update names that have changed
                ReplaceAll .TextMatrix(i, 0), .TextMatrix(i, 1), fdAll, True, True, flAll, rtGlobals
              End If
              'update original name
              .TextMatrix(i, 0) = .TextMatrix(i, 1)
              
              'if the define value is NOT numeric or a text string
              If Not IsNumeric(.TextMatrix(i, 2)) And Asc(.TextMatrix(i, 2)) <> 34 Then
                WaitCursor
                'also replace the values that match this define
                ReplaceAll .TextMatrix(i, 2), .TextMatrix(i, 1), fdAll, True, True, flAll, rtGlobals
              End If
            End If
          End If
        Next i
      End With
    Else
      'just save -
      Load frmProgress
      frmProgress.Caption = "Save Global Defines"
      frmProgress.Show vbModeless, frmMDIMain
    End If
  Else
    'not in a game; just save it
    Load frmProgress
    frmProgress.Caption = "Save Global Defines"
    frmProgress.Show vbModeless, frmMDIMain
  End If
  
  'done updating logics, now save the list
  frmProgress.lblProgress.Caption = "Saving global defines file"
  frmProgress.pgbStatus.Value = 0
  frmProgress.pgbStatus.Max = fgGlobals.Rows - 1
  SafeDoEvents

  On Error GoTo ErrHandler
  
  ' show wait cursor again, while saving the globals
  WaitCursor
  
  'build the file in a stringlist
  Set stlGlobals = BuildGlobalsFile(InGame, True)
  
  'create the new file
  strTempFile = TempFileName()
  intFile = FreeFile()
  
  Open strTempFile For Output As intFile
  
  'add the file text
  Print #intFile, stlGlobals.Text
  
  'close file
  Close intFile
  
  On Error Resume Next
  'erase any existing global file
  Kill GameDir & "globals.txt"
  
  On Error GoTo ErrHandler
  'copy tempfile to globals file
  FileCopy strTempFile, FileName
  
  On Error Resume Next
  'kill temp file
  Kill strTempFile
  
  'if not exporting
  If Not Exporting Then
    'reset changed flag
    IsChanged = False
    
    'set caption
    Caption = "Global Defines - "
    If InGame Then
      Caption = Caption & GameID
    Else
      'not in game, just use filename
      Caption = Caption & FileName
    End If
    'disable menu and toolbar button
    frmMDIMain.mnuRSave.Enabled = False
    frmMDIMain.Toolbar1.Buttons("save").Enabled = False
  End If
  
  'some extra things to do when saving an InGame list
  If InGame Then
    'if at least one update
    If blnChanged Then
      'mark all as changed;
      MakeAllChanged
      ' agiTODO: need a strategy to only mark affected logics;
      ' should be able to track what gets added, removed or
      ' updated
      '???isn't that already done? if nothing was replaced
      ' then why do all logics have to be marked?
    End If
    
    'let logic editors know
    If LogicEditors.Count > 1 Then
      For i = 1 To LogicEditors.Count
        LogicEditors(i).IsChanged = True
      Next i
    End If
  End If
  
  Unload frmProgress
  
  'restore cursor
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  'something went wrong
  '*'Debug.Assert False
  If Not blnError Then
    MsgBox "An error occurred while writing the global defines file. (Error number " & CStr(Err.Number) & "). You should check that the file manually by editing it as a text file.", vbCritical + vbOKOnly, "Global Defines File Error"
  End If
  Err.Clear
  blnError = True
  Resume Next
End Sub
            */
        }

        #region temp code
        void globalsfrmcode() {
            /*

      Private Const SPLIT_WIDTH = 60 'in twips
      Private ShowComment As Boolean
      Private SplitCol As Long
      Private SplitOffset As Single

      Private mRow As Long, mCol As Long
      Public NoScroll As Boolean

      Private CellIndentH As Single
      Private CellIndentV As Single
      Private lngRowHgt As Long

      Public PrevFGWndProc As Long


      Private CalcWidth As Long, CalcHeight As Long
      Private Const MIN_HEIGHT = 361
      Private Const MIN_WIDTH = 360


    Private Sub AddUndo(NextUndo As GlobalsUndo)

      If Not IsChanged Then
        MarkAsChanged
      End If

      'remove old undo items until there is room for this one
      'to be added
      If Settings.GlobalUndo > 0 Then
        Do While UndoCol.Count >= Settings.GlobalUndo
          UndoCol.Remove 1
        Loop
      End If

      'adds the next undo object
      UndoCol.Add NextUndo

      'set undo menu
      frmMDIMain.mnuEUndo.Enabled = True
      frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(GLBUNDOTEXT + NextUndo.UDAction) & vbTab & "Ctrl+Z"
    End Sub

    Public Function BuildGlobalsFile(ByVal UpdateLookup As Boolean, Optional ByVal Progress As Boolean = False) As StringList

      Dim i As Long, strName As String, strValue As String, strComment As String
      Dim lngMaxLen As Long, strAlign As String, lngMaxV As Long
      Dim tmpDef As TDefine, tmpStrList As StringList

      On Error GoTo ErrHandler

      'determine longest name length to facilitate aligning values
      lngMaxLen = 0
      With fgGlobals
        For i = 1 To fgGlobals.Rows - 2
          If Len(fgGlobals.TextMatrix(i, ctName)) > lngMaxLen Then
            lngMaxLen = Len(.TextMatrix(i, ctName))
          End If

          'right-align non-strings; need to know lebgth of longest
          'non-string
          If Asc(fgGlobals.TextMatrix(i, ctValue)) <> 34 Then
            If Len(fgGlobals.TextMatrix(i, ctValue)) > lngMaxV Then
              lngMaxV = Len(.TextMatrix(i, ctValue))
            End If
          End If
        Next i
      End With

      'if also updating the logic tooltip lookup list
      If UpdateLookup Then
        'if no defines
        If fgGlobals.Rows <= 2 Then
          ReDim GDefLookup(0)
        Else
          ReDim GDefLookup(fgGlobals.Rows - 3) '(number of defines-1)
        End If
      End If

      Set tmpStrList = New StringList

      'add a useful header
      tmpStrList.Add "["
      tmpStrList.Add "[ global defines file for " & GameID
      tmpStrList.Add "["

      'if showing progress
      If Progress Then
        frmProgress.pgbStatus.Value = 1
      End If

      'start with first entry
      With fgGlobals
        For i = 1 To .Rows - 2
          'get name and Value
          strName = .TextMatrix(i, ctName)
          strAlign = Space$(lngMaxLen - Len(strName) + 2)
          strValue = .TextMatrix(i, ctValue)
          'right align non-strings
          If Asc(strValue) <> 34 Then
           strAlign = strAlign & Space$(lngMaxV - Len(strValue))
          End If
          strComment = .TextMatrix(i, ctComment)
          If Len(strComment) > 0 Then
           strComment = " [ " & strComment
          End If

          'verify something to add
          If LenB(strName) <> 0 And LenB(strValue) <> 0 Then
            'add it
            tmpStrList.Add "#define " & strName & strAlign & strValue & strComment

            'if updating lookups
            If UpdateLookup Then
              'update the global lookup list, if this list is part of a game
              tmpDef.Name = strName
              tmpDef.Value = strValue
              tmpDef.Type = DefTypeFromValue(tmpDef.Value)
              GDefLookup(i - 1) = tmpDef
            End If
          Else
            '*'Debug.Assert False
          End If

          If Progress Then
            frmProgress.pgbStatus.Value = i + 1
          End If
        Next i
      End With

      'return the list
      Set BuildGlobalsFile = tmpStrList
      Set tmpStrList = Nothing
    Exit Function

    ErrHandler:
      '*'Debug.Assert False
      Resume Next
    End Function

    Private Sub DeleteRows(ByVal TopRow As Long, BtmRow As Long, Optional ByVal DontUndo As Boolean = False)

      Dim i As Long, SkipUndo As Boolean
      Dim NextUndo As GlobalsUndo, tmpDef As TDefine

      'if more than one row is selected
      If TopRow <> BtmRow Then
        'make a single undo object
        'if not skipping undo
        If Not DontUndo And Settings.GlobalUndo <> 0 Then
          'create new undo object
          Set NextUndo = New GlobalsUndo
          With NextUndo
            .UDAction = udgDeleteDefine
            .UDCount = BtmRow - TopRow + 1
            'the rows are consecutive, so
            'we only need the starting index
            .UDPos = TopRow
            For i = 0 To BtmRow - TopRow
              tmpDef.Default = fgGlobals.TextMatrix(TopRow + i, ctDefault)
              tmpDef.Name = fgGlobals.TextMatrix(TopRow + i, ctName)
              tmpDef.Value = fgGlobals.TextMatrix(TopRow + i, ctValue)
              tmpDef.Comment = fgGlobals.TextMatrix(TopRow + i, ctComment)
              .UDDefine(i) = tmpDef
            Next i
          End With
          'add to undo
          AddUndo NextUndo
        End If
      End If
      SkipUndo = (TopRow <> BtmRow)

      'delete the selected rows
      '(go backwards so rows are deleted correctly)
      For i = BtmRow To TopRow Step -1
        RemoveRow i, SkipUndo
      Next i

    End Sub

    Public Sub MenuClickECustom2()

    ' moved to Tools menu
    End Sub

    Public Sub MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

      ' bridge function that is called by the subclassing procedure
      ' to call the mousemove event

      fgGlobals_MouseMove Button, Shift, X, Y
    End Sub

    Public Sub MouseWheel(ByVal MouseKeys As Long, ByVal Rotation As Long, ByVal xPos As Long, ByVal yPos As Long)

      Dim NewValue As Long
      Dim Lstep As Single

      On Error Resume Next

      If Not frmMDIMain.ActiveMdiChild Is Me Then
        Exit Sub
      End If

      'scroll the grid
      ' (if list is small enough that there's no
      ' scrollbar, it will quietly ignore a change
      ' to TopRow property so no need to check if
      ' scrolling is possible)
      With fgGlobals
        Lstep = 4
        If Rotation > 0 Then
          NewValue = .TopRow - Lstep
          If NewValue < 1 Then
            NewValue = 1
          End If
        Else
          NewValue = .TopRow + Lstep
          If NewValue > .Rows - 1 Then
            NewValue = .Rows - 1
          End If
        End If
        .TopRow = NewValue
      End With
    End Sub


    Public Sub ClearGlobalDefines()

      On Error GoTo ErrHandler

      fgGlobals.Clear
      'set one row
      fgGlobals.Rows = 1
      'set up grid headings
      fgGlobals.TextMatrix(0, ctName) = "Name"
      fgGlobals.TextMatrix(0, ctValue) = "Value"
      fgGlobals.TextMatrix(0, ctComment) = "Comment"

      'if not loading (form isn't visible when loading)
      If Me.Visible Then
        'list has changed
        MarkAsChanged
      End If

    Exit Sub

    ErrHandler:
     ' '*'Debug.Assert False
      ErrMsgBox "Error while clearing defines: ", "", "Global Defines Error"
      Resume Next
    End Sub
    Public Sub InitFonts()

      On Error Resume Next

      fgGlobals.Font.Name = Settings.EFontName
      fgGlobals.Font.Size = Settings.EFontSize
      fgGlobals.Refresh

      txtEditName.Font.Name = Settings.EFontName
      txtEditName.Font.Size = Settings.EFontSize
      txtEditValue.Font.Name = Settings.EFontName
      txtEditValue.Font.Size = Settings.EFontSize
      txtEditComment.Font.Name = Settings.EFontName
      txtEditComment.Font.Size = Settings.EFontSize

      picComment.Font.Name = Settings.EFontName
      picComment.Font.Size = Settings.EFontSize
      lngRowHgt = picComment.TextHeight("")
    End Sub
    Private Sub MarkAsChanged()

      If Not IsChanged Then
        'set IsChanged flag
        IsChanged = True

        'enable menu and toolbar button
        frmMDIMain.mnuRSave.Enabled = True
        frmMDIMain.Toolbar1.Buttons("save").Enabled = True

        If Asc(Caption) <> 42 Then
          'mark caption
          Caption = sDM & Caption
        End If
      End If
    End Sub

    Public Sub MenuClickClear()

      'if editing
      If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
        Exit Sub
      End If

      'verify action

      If MsgBox("All global defines will be discarded. Do you want to continue?", vbQuestion + vbOKCancel, "Clear Global Defines") = vbCancel Then
        Exit Sub
      End If

      'clear grid by deleting all rows
      DeleteRows 1, fgGlobals.Rows - 2

      'if using undo, relabel the last undo to be a cut operation
      If Settings.GlobalUndo <> 0 Then
        UndoCol(UndoCol.Count).UDAction = udgClearList
      End If

      'select name cel of first non-readonly row
      fgGlobals.Row = 1
      fgGlobals.Col = ctName

      'force column adjustment by resizing
      Form_Resize
    End Sub

    Public Sub MenuClickCopy()

      'if editing
      If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
        Exit Sub
      End If

      ' copies the selected cell or the selected rows to the clipboard
      ' the normal clipboard gets the name, value and comment fields
      ' formatted as 'define' lines;
      ' a duplicate internal clipboard is used that also tracks the
      ' hidden 'original name' column

      Dim i As Long, strData As String
      Dim TopRow As Long, BtmRow As Long

      With fgGlobals
        If .SelectionMode = flexSelectionByRow Then
          'select all the rows
          'ensure starting at top
          If .Row < .RowSel Then
            TopRow = .Row
            BtmRow = .RowSel
          Else
            TopRow = .RowSel
            BtmRow = .Row
          End If

          'redim the internal clipboard
          ReDim GlobalsClipboard(BtmRow - TopRow + 1)

          'add first row to normal clipboard
          strData = DEF_MARKER & .TextMatrix(TopRow, ctName) & " " & .TextMatrix(TopRow, ctValue)
          If Len(.TextMatrix(TopRow, ctComment)) > 0 Then
            strData = strData & " [" & .TextMatrix(TopRow, ctComment)
          End If
          ' add first row to internal clipboard
          GlobalsClipboard(1).Default = .TextMatrix(TopRow, ctDefault)
          GlobalsClipboard(1).Name = .TextMatrix(TopRow, ctName)
          GlobalsClipboard(1).Value = .TextMatrix(TopRow, ctValue)
          GlobalsClipboard(1).Comment = .TextMatrix(TopRow, ctComment)

          'add remaining rows
          For i = TopRow + 1 To BtmRow
            'add text to normal clipboard
            strData = strData & vbCr & DEF_MARKER & .TextMatrix(i, ctName) & " " & .TextMatrix(i, ctValue)
            If Len(.TextMatrix(i, ctComment)) > 0 Then
              strData = strData & " [" & .TextMatrix(i, ctComment)
            End If

            ' add rows to internal clipboard
            GlobalsClipboard(i - TopRow + 1).Default = .TextMatrix(i, ctDefault)
            GlobalsClipboard(i - TopRow + 1).Name = .TextMatrix(i, ctName)
            GlobalsClipboard(i - TopRow + 1).Value = .TextMatrix(i, ctValue)
            GlobalsClipboard(i - TopRow + 1).Comment = .TextMatrix(i, ctComment)
          Next i

        Else
          'select just this cell
          strData = .TextMatrix(.Row, .Col)

          ' clear the internal clipboard
          ReDim GlobalsClipboard(0)
        End If

        'put selected text on clipboard
        Clipboard.Clear
        Clipboard.SetText strData  ', vbCFText  '???why did I comment out the format?

        'enable pasting, if selection contained entire row
        frmMDIMain.mnuEPaste.Enabled = (.SelectionMode = flexSelectionByRow)
      End With
    End Sub


    Public Sub MenuClickCustom1()

      'adds to the existing globals list, instead of replacing it

      On Error GoTo ErrHandler

      'if editing
      If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
        Exit Sub
      End If

      'get a globals file
      With MainDialog
        .Flags = cdlOFNHideReadOnly
        .DialogTitle = "Open Global Defines File"
        .DefaultExt = "txt"
        .Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        .FileName = vbNullString
        .InitDir = DefaultResDir
        .ShowOpen

        LoadGlobalDefines .FileName, False
        DefaultResDir = JustPath(.FileName)
      End With
    Exit Sub

    ErrHandler:
      'if user canceled the dialogbox,
      If Err.Number = cdlCancel Then
        Exit Sub
      End If

      '*'Debug.Assert False
      Resume Next
    End Sub

    Public Sub MenuClickCut()

      On Error GoTo ErrHandler

      'if editing
      If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
        Exit Sub
      End If

      'for cut, make sure entire row is selected
      If fgGlobals.SelectionMode <> flexSelectionByRow Then
        fgGlobals.SelectionMode = flexSelectionByRow
      End If

      'copy, then delete
      MenuClickCopy
      MenuClickDelete

      'if using undo, relabel the last undo to be a cut operation
      If Settings.GlobalUndo <> 0 Then
        UndoCol(UndoCol.Count).UDAction = udgCutDefine
        frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(GLBUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
      End If

      'enable pasting
      frmMDIMain.mnuEPaste.Enabled = True
    Exit Sub

    ErrHandler:
      '*'Debug.Assert False
      Resume Next
    End Sub

    Public Sub MenuClickDelete()

      Dim i As Long, TopRow As Long, BtmRow As Long
      Dim SkipUndo As Boolean

      'if editing
      If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
        Exit Sub
      End If

      'ensure top row is <=bottom row
      If fgGlobals.RowSel < fgGlobals.Row Then
        TopRow = fgGlobals.RowSel
        BtmRow = fgGlobals.Row
      Else
        TopRow = fgGlobals.Row
        BtmRow = fgGlobals.RowSel
      End If

      DeleteRows TopRow, BtmRow

      'reset selection to a single row
      fgGlobals.Row = TopRow
      fgGlobals.Col = ctName
    End Sub

    Public Sub MenuClickHelp()

      On Error GoTo ErrHandler

      HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\editingdefines.htm"
    Exit Sub

    ErrHandler:
      '*'Debug.Assert False
      Resume Next
    End Sub
    Public Sub MenuClickExport()

      'save as

      Dim rtn As VbMsgBoxResult, blnInGame As Boolean


      'if editing
      If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
        Exit Sub
      End If

      On Error Resume Next

      'set up commondialog
      With MainSaveDlg
        .DialogTitle = "Save Global Defines File"
        .DefaultExt = "txt"
        .Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
        .FilterIndex = 1
        .InitDir = DefaultResDir
        .FullName = FileName
        .hWndOwner = frmMDIMain.hWnd
      End With

      Do
        MainSaveDlg.ShowSaveAs
        'if canceled,
        If Err.Number = cdlCancel Then
          'exit without doing anything
          Exit Sub
        End If
        DefaultResDir = JustPath(MainSaveDlg.FileName)

        'if file exists,
        If FileExists(MainSaveDlg.FullName) Then
          'verify replacement
          rtn = MsgBox(MainSaveDlg.FileName & " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")

          If rtn = vbYes Then
            Exit Do
          ElseIf rtn = vbCancel Then
            Exit Sub
          End If
        Else
          Exit Do
        End If
      Loop While True

      'save ingame status
      blnInGame = InGame

      'save filename
      FileName = MainSaveDlg.FullName
      'force ingame status
      InGame = False
      'use save method to finish (if this is an InGame list
      'pass the Export flag so caption and toolbars aren't changed)
      MenuClickSave blnInGame

      'if in a game
      If blnInGame Then
        'restore ingame status and filename
        InGame = True
        FileName = GameDir & "globals.txt"
      End If

    End Sub

    Public Sub MenuClickECustom1()

      'begin a logic search for the define name in this row
      Dim strFind As String

      On Error GoTo ErrHandler

      'row 0 not searchable
      If fgGlobals.Row = 0 Then
        Exit Sub
      End If

      strFind = fgGlobals.TextMatrix(fgGlobals.Row, ctName)
      'if nothing to find,
      If LenB(strFind) = 0 Then
        Exit Sub
      End If

      'reset logic search
      FindForm.ResetSearch

      'set global search values
      GFindText = strFind
      GFindDir = fdAll
      GMatchWord = True
      GMatchCase = False
      GLogFindLoc = flAll
      GFindSynonym = False

      'ensure this form is the search form
      Set SearchForm = Me

      'find selected word in all logics
      FindInLogic strFind, fdAll, True, False, flAll
    Exit Sub

    ErrHandler:
      '*'Debug.Assert False
      Resume Next
    End Sub

    Public Sub MenuClickInsert()

      'if editing
      If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
        Exit Sub
      End If

      'insert a row
      Inserting = True
      fgGlobals.AddItem vbNullString, fgGlobals.Row

      'force column to name
      fgGlobals.Col = ctName

      'now edit it
      fgGlobals_DblClick

    End Sub

    Public Sub MenuClickPaste()

      'take clipboard data, and add it to
      'grid, ignoring errors

      Dim strRows() As String, strName As String, strValue As String
      Dim strTemp As String, i As Long, strComment As String
      Dim Index As Long, blnErrors As Boolean, lngInsertRow As Long
      Dim blnNoWarn As Boolean, lngStartRow As Long
      Dim NextUndo As GlobalsUndo, tmpDefines() As TDefine

      'Index is used to track the location where the next
      'insertion should occur;
      'lngInsertRow is what we pass to the validate function;
      'if the define gets inserted, then lngInsertRow doesn't
      'change
      '
      'if the define is a replacement, then lngInsertRow is
      'set to the old line's value, and the new value is
      'inserted either at Index (if the line being replaced
      'comes AFTER Index) or at Index-1 (if the line being
      'replaced comes BEFORE Index)

      'pasted values come from internal clipboard (if there is
      ' something there) or from main clipboard; if from main
      ' clipboard, should be in normal define format:
      '  #define name value
      '  #define name value [ comment

      On Error GoTo ErrHandler

      'if editing
      If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
        Exit Sub
      End If

      'no edit row
      EditRow = -1

      'beginning index is curent row
      Index = fgGlobals.Row
      'keep track of start so we know if nothing got pasted
      lngStartRow = Index

      'assume no duplicates or reserve overrides
      blnReserved = False
      blnDuplicates = False

      'add undo
      If Settings.GlobalUndo <> 0 Then
        Set NextUndo = New GlobalsUndo
        NextUndo.UDAction = udgPasteDefines
        NextUndo.UDPos = Index
        ReDim tmpDefines(0)
      End If

      'if something on global clipboard
      If UBound(GlobalsClipboard()) > 0 Then
        'add the defines at the insert position
        For i = 1 To UBound(GlobalsClipboard())
          'coming from internal clipboard means no concerns
          ' about formatting, so just validate and add it

          With GlobalsClipboard(i)

            lngInsertRow = Index
            'validate the defines
            If ValidateDefine(.Name, .Value, True, lngInsertRow) Then
              'was it a replace? we can tell by checking lngInsertRow against Index
              If lngInsertRow = Index Then
                'not a replacement; OK to use pasted default value
                fgGlobals.TextMatrix(lngInsertRow, ctDefault) = .Default
              End If

              'if there is a comment, add it
              If Len(.Comment) > 0 Then
                fgGlobals.TextMatrix(Index, ctComment) = .Comment
              End If

              'if adding undo, we need to document what kind of
              '"add" was done; either an insert, or a replacement
              If Settings.GlobalUndo <> 0 Then
                ReDim Preserve tmpDefines(UBound(tmpDefines) + 1)
                'was it a replace? we can tell by checking lngIndex against Index
                If lngInsertRow <> Index Then
                  'it was a replacement;
                  blnDuplicates = True
                  'save the define information that was replaced
                  tmpDefines(UBound(tmpDefines)) = RepDefine

                  'use type field to save the row removed and line inserted
                  tmpDefines(UBound(tmpDefines)).Type = lngInsertRow * 10000
                  'and the index line were replacement was added
                  '(it will depend on whether or not the replaced
                  'line was before or after the insert position)
                  If lngInsertRow < Index Then
                    tmpDefines(UBound(tmpDefines)).Type = tmpDefines(UBound(tmpDefines)).Type + Index - 1
                  Else
                    tmpDefines(UBound(tmpDefines)).Type = tmpDefines(UBound(tmpDefines)).Type + Index
                  End If

                  'replacements always delete a row, so we need to
                  'adjust the starting line
                  lngStartRow = lngStartRow - 1
                Else
                  'if just inserting, we only care about row value where insert occurred
                  tmpDefines(UBound(tmpDefines)).Type = Index
                End If
              End If

              'if something was actually added (lngInsertRow didn't change)
              If lngInsertRow = Index Then
                'increment index
                Index = Index + 1
              End If
            Else
              'bad define
              blnErrors = True
            End If
          End With
        Next i

      Else
        'use the main clipboard

        'convert crlf into just cr
        strTemp = Replace(Clipboard.GetText(vbCFText), vbCrLf, vbCr)
        'convert any remaining lf into cr
        strTemp = Replace(strTemp, vbLf, vbCr)

        'split text into rows based on carriage returns
        strRows = Split(strTemp, vbCr)

        'add the defines at the insert position
        For i = 0 To UBound(strRows())
          'trim it
          strRows(i) = Trim$(strRows(i))

          'skip blank lines
          If Len(strRows(i)) > 0 Then
            'remove define marker
            If LCase$(Left$(strRows(i), 8)) = DEF_MARKER Then
              strRows(i) = Trim$(Right$(strRows(i), Len(strRows(i)) - 8))

              'extract comment
              strRows(i) = StripComments(strRows(i), strComment)
              'split row at first space
              strName = Left$(strRows(i), InStr(1, strRows(i), " ") - 1)
              strValue = Trim$(Right$(strRows(i), Len(strRows(i)) - InStr(strRows(i), " ")))

              'if both strings are not null
              If Len(strName) > 0 And Len(strValue) > 0 Then
        '''''''''''''''''''''''''''''''''''''
        ' this section nearly identical to
        ' the code in LoadGlobalDefines sub;
        ' should consider a single function
        ' (AddDefine); needs some work on
        ' differences though; probably not
        ' worth the effort
        '''''''''''''''''''''''''''''''''''''
                lngInsertRow = Index
                'validate the defines
                If ValidateDefine(strName, strValue, True, lngInsertRow) Then
                  'if there is a comment, add it
                  If Len(strComment) > 0 Then
                    fgGlobals.TextMatrix(Index, ctComment) = strComment
                  End If
                  'when pasting from normal clipboard, always clear the default
                  fgGlobals.TextMatrix(Index, ctDefault) = ""

                  'if adding undo, we need to document what kind of
                  '"add" was done; either an insert, or a replacement
                  If Settings.GlobalUndo <> 0 Then
                    ReDim Preserve tmpDefines(UBound(tmpDefines) + 1)
                    'was it a replace? we can tell by checking lngIndex against Index
                    If lngInsertRow <> Index Then
                      'it was a replacement;
                      blnDuplicates = True
                      'save the define information that was replaced
                      tmpDefines(UBound(tmpDefines)) = RepDefine

                      'use type field to save the row removed and line inserted
                      tmpDefines(UBound(tmpDefines)).Type = lngInsertRow * 10000
                      'and the index line were replacement was added
                      '(it will depend on whether or not the replaced
                      'line was before or after the insert position)
                      If lngInsertRow < Index Then
                        tmpDefines(UBound(tmpDefines)).Type = tmpDefines(UBound(tmpDefines)).Type + Index - 1
                      Else
                        tmpDefines(UBound(tmpDefines)).Type = tmpDefines(UBound(tmpDefines)).Type + Index
                      End If

                      'replacements always delete a row, so we need to
                      'adjust the starting line
                      lngStartRow = lngStartRow - 1
                    Else
                      'if just inserting, we only care about the position
                      tmpDefines(UBound(tmpDefines)).Type = Index
                    End If
                  End If

                  'if something was actually added (lngInsertRow didn't change)
                  If lngInsertRow = Index Then
                    'increment index
                    Index = Index + 1
                  End If
        ''''''''''''''''''''''''''''''
                Else
                  'bad define
                  blnErrors = True
                End If
              Else
                'bad row
                blnErrors = True
              End If
            End If
          End If
        Next i
      End If

      'if nothing was added, then index hasn't changed AND no replacements occurred
      If Index = lngStartRow And Not blnDuplicates Then
        MsgBox "There were no usable data on the clipboard, so nothing was pasted.", vbInformation, "Nothing to Paste"
      Else
        'add count to undo
        If Settings.GlobalUndo <> 0 Then
          NextUndo.UDCount = Index - lngStartRow
          'if any defines were replaced, we need to add them to the undo object so
          'they can be restored during the undo process
          If UBound(tmpDefines()) > 0 Then
            For i = 1 To UBound(tmpDefines())
              NextUndo.UDDefine(i - 1) = tmpDefines(i)
            Next i
          End If
          'add to undo
          AddUndo NextUndo
        End If
        'if errors
        If blnErrors Then
          MsgBox "Some entries could not be pasted because of formatting errors.", vbInformation + vbOKOnly, "Paste Errors"
        End If
      End If
    Exit Sub

    ErrHandler:
      '*'Debug.Assert False
      Resume Next

    End Sub
            */
        }

        void globalsfrmcode2() {
            /*

        Private Sub Form_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

          On Error GoTo ErrHandler

          'if right button
          If Button = vbRightButton Then
            'if edit menu is enabled
            If frmMDIMain.mnuEdit.Enabled Then
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
              PopupMenu frmMDIMain.mnuEdit
            End If
          End If
        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub

        Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)

          Cancel = Not AskClose()
        End Sub

        Private Sub Form_Resize()

          Dim OldWidth As Single

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

          'if minimized or if the form is not visible
          If Me.WindowState = vbMinimized Or Not Visible Then
            Exit Sub
          End If

          'if restoring from minimize, activation may not have triggered
          If MainStatusBar.Tag <> CStr(rtGlobals) Then
            ActivateActions
          End If

          With fgGlobals
            'save old width of grids
            OldWidth = .Width

            'resize grids
            .Height = CalcHeight - 2 * ScreenTWIPSX
            ' width depends on whether or not scrollbar is present
            If ScrollBarVisible() Then
              .Width = CalcWidth - 2 * ScreenTWIPSX
            Else
              .Width = CalcWidth
            End If

            'set splitter height
            picSplit.Height = .Height

            'if comment column is visible
            If ShowComment Then
              'ratio out new column width for name
              .ColWidth(ctName) = .ColWidth(ctName) / OldWidth * .Width
              If .ColWidth(ctName) < 36 * ScreenTWIPSX Then
                .ColWidth(ctName) = 36 * ScreenTWIPSX
              End If
              'ratio out new column width for value
              .ColWidth(ctValue) = .ColWidth(ctValue) / OldWidth * .Width
              If .ColWidth(ctValue) < 36 * ScreenTWIPSX Then
                .ColWidth(ctValue) = 36 * ScreenTWIPSX
              End If

              'set width of globals grid comment column
              If ScrollBarVisible() Then
                'take into account scrollbar
                .ColWidth(ctComment) = .Width - .ColWidth(ctName) - .ColWidth(ctValue) - 17 * ScreenTWIPSX
              Else
                'no scrollbar
                'set width of second column
                .ColWidth(ctComment) = .Width - .ColWidth(ctName) - .ColWidth(ctValue) - 2 * ScreenTWIPSX
              End If
            Else
              'ratio out new column width for name
              .ColWidth(ctName) = .ColWidth(ctName) / OldWidth * .Width
              If .ColWidth(ctName) < 36 * ScreenTWIPSX Then
                .ColWidth(ctName) = 36 * ScreenTWIPSX
              End If

              'set width of globals grid Value column
              If ScrollBarVisible() Then
                'take into account scrollbar
                .ColWidth(ctValue) = .Width - .ColWidth(ctName) - 17 * ScreenTWIPSX
              Else
                'no scrollbar
                'set width of second column
                .ColWidth(ctValue) = .Width - .ColWidth(ctName) - 2 * ScreenTWIPSX
              End If
            End If
          End With

          ' if editing a name
          With txtEditName
            If .Visible Then
              .Move fgGlobals.CellLeft + CellIndentH, fgGlobals.CellTop + fgGlobals.Top + CellIndentV, fgGlobals.CellWidth - CellIndentH ', fgGlobals.CellHeight - CellIndentV
            End If
          End With

          'if edting a value
          With txtEditValue
            If .Visible Then
              .Move fgGlobals.CellLeft + CellIndentH, fgGlobals.CellTop + fgGlobals.Top + CellIndentV, fgGlobals.CellWidth - CellIndentH, fgGlobals.CellHeight - CellIndentV
            End If
          End With

          ' if editing a comment
          With txtEditComment
            If .Visible Then
              .Move fgGlobals.CellLeft + CellIndentH, fgGlobals.CellTop + fgGlobals.Top + CellIndentV, fgGlobals.CellWidth - CellIndentH, SendMessage(.hWnd, EM_GETLINECOUNT, 0, 0) * lngRowHgt
            End If
          End With
        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub

        Private Sub Form_Unload(Cancel As Integer)

          Dim i As Long

          'if in a game,
          If InGame Then
            'reset inuse flag
            GEInUse = False
            'release object
            Set GlobalsEditor = Nothing
          End If

          'save position so it can be restored later if form
          'is closed and then re-opened
          With Me
            GWState = .WindowState
            If GWState = vbMaximized Then
              'save non-max parameters
              .WindowState = vbNormal
            End If

            GWLeft = .Left
            GWTop = .Top
            GWWidth = .Width
            GWHeight = .Height
          End With
          GWShowComment = ShowComment

          ' save column widths as a fraction of total grid width
          GWNameFrac = fgGlobals.ColWidth(ctName) / fgGlobals.Width
          GWValFrac = fgGlobals.ColWidth(ctValue) / fgGlobals.Width

          ' setting values start out the same
          Settings.GENameFrac = GWNameFrac
          Settings.GEValFrac = GWValFrac
          ' then adjust them based on what default showcomment value
          'is compared to current comment value

          'if default is three columns
          If Settings.GEShowComment Then
            'but currently showing two -need to have three columns
            'on restart; assume comment will be 40%
            If Not ShowComment Then
              Settings.GENameFrac = Settings.GENameFrac * 0.6
              Settings.GEValFrac = Settings.GEValFrac * 0.6
              'if name value is too small
              If Settings.GENameFrac < 0.1 Then
                Settings.GENameFrac = 0.1
                Settings.GEValFrac = 0.5
              'if name value it too big
              ElseIf Settings.GENameFrac > 0.5 Then
                Settings.GENameFrac = 0.5
                Settings.GEValFrac = 0.1
              End If
            End If
          Else
            'default is two - if currently showing three
            If ShowComment Then
              'ratio the name column based on just the two columns
              Settings.GENameFrac = Settings.GENameFrac / (Settings.GENameFrac + Settings.GEValFrac)
              'value column doesn't matter
            End If
          End If

          'save the values
          WinAGISettingsList.WriteSetting("Globals", "GENameFrac", Settings.GENameFrac
          WinAGISettingsList.WriteSetting("Globals", "GEValFrac", Settings.GEValFrac

          'destroy undocol
          If UndoCol.Count > 0 Then
            For i = UndoCol.Count To 1 Step -1
              UndoCol.Remove i
            Next i
          End If
          Set UndoCol = Nothing

        #If DEBUGMODE <> 1 Then
          'release subclass hook to flexgrid
          SetWindowLong Me.fgGlobals.hWnd, GWL_WNDPROC, PrevFGWndProc
        #End If 'need to check if this is last form
          LastForm Me
        End Sub

        Private Sub picComment_DblClick()

         'edit the underlying comment

          picComment.Visible = False
          fgGlobals_DblClick
          'refresh the txtbox, then reset height again
          txtEditComment.Refresh
          txtEditComment.Height = SendMessage(txtEditComment.hWnd, EM_GETLINECOUNT, 0, 0) * lngRowHgt

        End Sub


        Private Sub picSelFrame_MouseDown(Index As Integer, Button As Integer, Shift As Integer, X As Single, Y As Single)
          'pass focus to grid
          fgGlobals.SetFocus
        End Sub


        Private Sub tmrTip_Timer()

          'this function  displays a large tip window that
          ' shows the full comment text

          Dim lngRow As Long, lngCol As Long
          Dim rtn As Long, mPos As POINTAPI, i As Long
          Dim strLine As String, lngCount As Long

          On Error GoTo ErrHandler

          'always turn off timer so we don't recurse
          tmrTip.Enabled = False

          'if not active form
          If Not frmMDIMain.ActiveMdiChild Is Me Then
            Exit Sub
          End If
          'if editing
          If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
            Exit Sub
          End If
          'also exit if full row is selected
          If fgGlobals.SelectionMode = flexSelectionByRow Then
            Exit Sub
          End If

          'get the cursor position and figure out if it's over picVisual or picPriority
          rtn = GetCursorPos(mPos)
          'convert to flexgrid coordinates
          rtn = ScreenToClient(fgGlobals.hWnd, mPos)
          'if mouse is not over the grid
          If (mPos.X * ScreenTWIPSX) > fgGlobals.Width Or (mPos.Y * ScreenTWIPSY) > fgGlobals.Height Then
            Exit Sub
          End If

          'get row and col under the cursor
          lngRow = fgGlobals.MouseRow
          lngCol = fgGlobals.MouseCol

          'only show tip if it's a comment and it's selected
          If lngRow <> fgGlobals.RowSel Or lngCol <> fgGlobals.ColSel Or lngCol <> ctComment Then
            Exit Sub
          End If

          '*'Debug.Assert lngCol = ctComment
          If picComment.TextWidth(fgGlobals.TextMatrix(lngRow, lngCol)) > fgGlobals.ColWidth(ctComment) - 60 Then
            'show tipbox with full comment
            '
            ' getting the tip box to show the text
            ' exactly the same as the text box is not simple
            ' best way is to assign text to the text box,
            ' then use API calls to retrieve the text by
            'line, which is then printed in the picturebox
            With txtEditComment
              .Text = fgGlobals.TextMatrix(lngRow, lngCol)
              .Width = fgGlobals.CellWidth - CellIndentH
              lngCount = SendMessage(.hWnd, EM_GETLINECOUNT, 0, 0)
            End With
            'size the picture box that holds the label
            With picComment
              .Width = fgGlobals.CellWidth - CellIndentH
              .Height = lngRowHgt * (lngCount + 0.2)
              .Cls
              For i = 0 To lngCount - 1
                'get index of beginning of this line
                rtn = SendMessage(txtEditComment.hWnd, EM_LINEINDEX, i, 0)
                'get length of this line
                rtn = SendMessage(txtEditComment.hWnd, EM_LINELENGTH, rtn, 0)
                'get text of this line
                strLine = ChrW$(rtn And &HFF) & ChrW$(rtn \ &H100) & String$(rtn, 32)
                rtn = SendMessageByString(txtEditComment.hWnd, EM_GETLINE, i, strLine)
                strLine = Replace(strLine, ChrW$(0), vbNullString)
                strLine = RTrim$(strLine)
                'print the line
                .CurrentX = 0
                .CurrentY = i * lngRowHgt
                picComment.Print strLine
              Next i

              'reposition it
              .Top = (lngRow - fgGlobals.TopRow + 1) * fgGlobals.RowHeight(1) + fgGlobals.Top
              .Left = 45 + fgGlobals.ColWidth(ctName) + fgGlobals.ColWidth(ctValue)
              .Visible = True
            End With
          End If
        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub

        Private Sub txtEditComment_Change()

            'adjust height if row count changed

            Dim lngCount As Long

            lngCount = SendMessage(txtEditComment.hWnd, EM_GETLINECOUNT, 0, 0)

            If txtEditComment.Height <> lngCount * lngRowHgt Then
              txtEditComment.Height = lngCount * lngRowHgt
            End If
        End Sub

        Private Sub txtEditComment_KeyPress(KeyAscii As Integer)

          Dim NextUndo As GlobalsUndo

          On Error GoTo ErrHandler

          Select Case KeyAscii
          Case 9, 10, 13 'enter or tab
            ' comments don't need to be validated; anything is fine

            'hide the box
            txtEditComment.Visible = False

            'if changed
            If fgGlobals.TextMatrix(EditRow, EditCol) <> txtEditComment.Text Then
              'put the text back into grid
              If Settings.GlobalUndo <> 0 Then
                'add undo for a change in comment
                Set NextUndo = New GlobalsUndo
                With NextUndo
                  .UDAction = udgEditComment
                  .UDCount = 1
                  'save the old value
                  .UDPos = EditRow
                  '*'Debug.Assert EditCol = ctComment
                  .UDText = fgGlobals.TextMatrix(EditRow, ctComment)
                End With
                'add to undo
                AddUndo NextUndo
              End If
            End If

            'copy new comment to grid
            fgGlobals.TextMatrix(EditRow, EditCol) = txtEditComment.Text

            'change made; enable save
            MarkAsChanged

            'enter or tab moves to next row
            'move to name column, next row
            fgGlobals.Col = ctName
            fgGlobals.Row = fgGlobals.Row + 1

            'done editing; enable edit menu
            SetEditMenu

            'ignore key
            KeyAscii = 0

          Case 27 'escape
            'hide textbox without saving text
            txtEditComment.Visible = False

            'ignore key
            KeyAscii = 0

          Case Else

          End Select

        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub


        Private Sub txtEditComment_Validate(Cancel As Boolean)

          'always copy text value back into grid, and then hide the text box

          Dim NextUndo As GlobalsUndo

          On Error GoTo ErrHandler

          'hide the box
          txtEditComment.Visible = False
          'restore height
          txtEditComment.Height = fgGlobals.CellHeight - CellIndentV

          ' if the comment hasn't changed, just exit
          If fgGlobals.TextMatrix(EditRow, EditCol) = txtEditComment.Text Then
            Exit Sub
          End If

          'make change

          'put the text back into grid
          If Settings.GlobalUndo <> 0 Then
            'add undo for a change in comment
            Set NextUndo = New GlobalsUndo
            With NextUndo
              .UDAction = udgEditComment
              .UDCount = 1
              'save the old value
              .UDPos = EditRow
              '*'Debug.Assert EditCol = ctComment
              .UDText = fgGlobals.TextMatrix(EditRow, ctComment)
            End With
            'add to undo
            AddUndo NextUndo
          End If

          'copy new comment to grid
          fgGlobals.TextMatrix(EditRow, EditCol) = txtEditComment.Text
        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub

        Private Sub txtEditName_Change()

          On Error GoTo ErrHandler

          'if this is last row,
          If EditRow = fgGlobals.Rows - 1 Then
            'begin inserting
            Inserting = True

            'add another row
            fgGlobals.AddItem vbNullString
            'adjust column widths by resizing
            Form_Resize
          End If
        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub

        Private Sub txtEditName_KeyPress(KeyAscii As Integer)

          On Error GoTo ErrHandler

          Select Case KeyAscii
          Case 9, 10, 13 'enter or tab
            'if result is valid,
            If ValidateInput() Then
              'if name is blank and input was valid,
              'it means we want to delete this row
              If Len(txtEditName.Text) = 0 Then
                'delete this row
                '(no undo if inserting)
                RemoveRow EditRow, Inserting
                'cancel the insert
                Inserting = False
              End If

              'hide the box
              txtEditName.Visible = False

              'tab moves to next column and begins editing
              'if inserting, also always move to next column
              If Inserting Or (KeyAscii = 9) Then
                'move to Value column
                fgGlobals.Col = ctValue

                'start another edit operation
                fgGlobals_DblClick
              Else
                'reset menus
                SetEditMenu
              End If
            Else
              'need to force focus (might be a tab thing?)
              txtEditName.SetFocus
            End If

            'ignore key
            KeyAscii = 0

          Case 27 'escape
            'hide textbox without saving text
            txtEditName.Visible = False

            'ignore key
            KeyAscii = 0

            'if inserting, we need to cancel it
            If Inserting Then
              RemoveRow EditRow, True
              'cancel the insert
              Inserting = False
            End If

          End Select

        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub

        Private Sub txtEditName_LostFocus()

          If MustEditValue Then
            'move to Value column
            fgGlobals.Col = ctValue

            'start another edit operation
            fgGlobals_DblClick
          End If
        End Sub


        Private Sub txtEditName_Validate(Cancel As Boolean)

          'this will handle cases where user tries to 'click' on something,

          On Error GoTo ErrHandler
          If Not txtEditName.Visible Then Exit Sub

          'if OK, hide the text box
          If ValidateInput() Then
            'if name is blank and input was valid,
            'it means we want to delete this row
            If Len(txtEditName.Text) = 0 Then
              'delete this row
              '(no undo if inserting)
              RemoveRow EditRow, Inserting
              'cancel the insert
              Inserting = False
            End If

            'hide the box
            txtEditName.Visible = False
          Else
          'if not OK, cancel
            Cancel = True
          End If
        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub

        Private Sub txtEditValue_KeyPress(KeyAscii As Integer)

          On Error GoTo ErrHandler

          Select Case KeyAscii

          Case 9, 10, 13 'enter or tab
            'if result is valid,
            If ValidateInput() Then
              'hide the box
              txtEditValue.Visible = False

              'if comment column is visible
              If ShowComment Then
                'tab moves to comment column and begins editing
                If KeyAscii = 9 Then
                  'move to Value column
                  fgGlobals.Col = ctComment

                  'start another edit operation
                  fgGlobals_DblClick
                Else
                  'reset menus
                  SetEditMenu
                End If
              Else
                'enter or tab moves to next row
                'move to name column, next row
                fgGlobals.Col = ctName
                fgGlobals.Row = fgGlobals.Row + 1

                'done editing; enable edit menu
                SetEditMenu
              End If
            Else
              'need to force focus (might be a tab thing?)
              txtEditValue.SetFocus
            End If

            'ignore key
            KeyAscii = 0

          Case 27 'escape
            'hide textbox without saving text
            txtEditValue.Visible = False

            'ignore key
            KeyAscii = 0
          End Select

          'if new value is blank (can only happen if editing a new row)
          'add a blank space as a place holder
          If LenB(fgGlobals.TextMatrix(EditRow, ctValue)) = 0 Then
            fgGlobals.TextMatrix(EditRow, ctValue) = Chr$(34) & " " & Chr$(34)
          End If
        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub

        Private Sub txtEditValue_Validate(Cancel As Boolean)

          'this will handle cases where user tries to 'click' on something,

          If Not txtEditValue.Visible Then Exit Sub
          On Error GoTo ErrHandler

          'if OK, hide the text box
          If ValidateInput() Then
            txtEditValue.Visible = False
          Else
          'if not OK, cancel
            Cancel = True
          End If
        Exit Sub

        ErrHandler:
          '*'Debug.Assert False
          Resume Next
        End Sub


                */
        }
        void globalsfrmcode3() {
            /*
Public Sub MenuClickSelectAll()

  'if editing
  If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
    Exit Sub
  End If
  
  With fgGlobals
    .SelectionMode = flexSelectionByRow
    .Col = 0
    .Row = 1
    .RowSel = fgGlobals.Rows - 2
    .ColSel = 3
    .Highlight = flexHighlightAlways
    .MergeCells = flexMergeNever
  End With
End Sub

Public Sub MenuClickUndo()

  Dim NextUndo As GlobalsUndo
  Dim i As Long, lngIndex As Long, lngRow As Long
  Dim lngValid As Long
  
  On Error GoTo ErrHandler
  
  'if editing
  If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
    Exit Sub
  End If
  
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
    frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(GLBUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
  Else
    frmMDIMain.mnuEUndo.Caption = "&Undo " & vbTab & "Ctrl+Z"
  End If
  
  'undo the action
  Select Case NextUndo.UDAction
  Case udgAddDefine
    'remove the define values that was added
    RemoveRow NextUndo.UDPos, True
    
  Case udgPasteDefines, udgImportDefines
    'remove the define values were added
    'we need to also restore any previous
    'defines that were replaced
    'we know which were replaced because
    'they have a name/value;
    'we use the Type field to hold the location
    
    'step through the defines list in reverse
    'order to preserve the previous state
    With NextUndo
      For i = .UDCount - 1 To 0 Step -1
        'extract previous index and insertrow values
        lngIndex = .UDDefine(i).Type Mod 10000
        lngRow = .UDDefine(i).Type \ 10000
        'first we remove the added value
        RemoveRow lngIndex, True
        'then reinsert previous value if necessary
        If Len(.UDDefine(i).Value) > 0 Then
          ' determine if it's an override BEFORE adding
          ' otherwise it will be marked as a duplcate instead
          lngValid = ValidateName(.UDDefine(i).Name)
          'then we restore the old value
          fgGlobals.AddItem .UDDefine(i).Name & vbTab & .UDDefine(i).Name & vbTab & .UDDefine(i).Value & vbTab & .UDDefine(i).Comment, lngRow
          ' select it
          fgGlobals.Row = lngRow
          fgGlobals.Col = ctName
          'highlight if necessary
          If lngValid >= 8 And lngValid <= 13 Then
            'highlight
            fgGlobals.CellFontBold = True
            fgGlobals.CellForeColor = vbRed
          Else
            'unhighlight
            fgGlobals.CellFontBold = False
            fgGlobals.CellForeColor = vbBlack
          End If
        End If
      Next i
    End With
    
  Case udgDeleteDefine, udgCutDefine, udgClearList
    'add back the items removed
    With NextUndo
      For i = 0 To .UDCount - 1
        ' determine if it's an override BEFORE adding
        ' otherwise it will be marked as a duplcate instead
        lngValid = ValidateName(.UDDefine(i).Name)
        fgGlobals.AddItem .UDDefine(i).Default & vbTab & .UDDefine(i).Name & vbTab & .UDDefine(i).Value & vbTab & .UDDefine(i).Comment, .UDPos + i
        ' select it
        fgGlobals.Row = .UDPos + i
        fgGlobals.Col = ctName
        'highlight if necessary
        If lngValid >= 8 And lngValid <= 13 Then
          'highlight
          fgGlobals.CellFontBold = True
          fgGlobals.CellForeColor = vbRed
        Else
          'unhighlight
          fgGlobals.CellFontBold = False
          fgGlobals.CellForeColor = vbBlack
        End If
      Next i
    End With
    
  Case udgEditName
    'change the name back to its previous value
    With fgGlobals
      .TextMatrix(NextUndo.UDPos, ctName) = NextUndo.UDText
      ' select it
      .Row = NextUndo.UDPos
      .Col = ctName
      'highlight if necessary
      lngValid = ValidateName(NextUndo.UDText)
      If lngValid >= 8 And lngValid <= 13 Then
        'highlight
        .CellFontBold = True
        .CellForeColor = vbRed
      Else
        'unhighlight
        .CellFontBold = False
        .CellForeColor = vbBlack
      End If
    End With
    
  Case udgEditValue
    'change the value back to its previous value
    fgGlobals.TextMatrix(NextUndo.UDPos, ctValue) = NextUndo.UDText
    
  Case udgSort
    'restore the previous sort order
    SortGlobals -1, NextUndo
    
  Case udgEditComment
    'change the comment back to its previous value
    fgGlobals.TextMatrix(NextUndo.UDPos, ctComment) = NextUndo.UDText
  
  End Select
  
  'always mark file as changed after an undo
  MarkAsChanged
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub RemoveRow(ByVal RowIndex As Long, Optional ByVal DontUndo As Boolean = False)

  Dim NextUndo As GlobalsUndo, tmpDef As TDefine
  Dim bln1stRow As Boolean
  
  'removes a row,
  'unless it is the last one
  
  'if not skipping undo
  If Not DontUndo And Settings.GlobalUndo <> 0 Then
    'create new undo object
    Set NextUndo = New GlobalsUndo
    With NextUndo
      .UDAction = udgDeleteDefine
      .UDCount = 1
      .UDPos = RowIndex
      tmpDef.Default = fgGlobals.TextMatrix(RowIndex, ctDefault)
      tmpDef.Name = fgGlobals.TextMatrix(RowIndex, ctName)
      tmpDef.Value = fgGlobals.TextMatrix(RowIndex, ctValue)
      tmpDef.Comment = fgGlobals.TextMatrix(RowIndex, ctComment)
      .UDDefine(0) = tmpDef
      If fgGlobals.Rows = 2 Or RowIndex = fgGlobals.Rows - 1 Then
        .UDText = "T"
      End If
    End With
    'add to undo
    AddUndo NextUndo
  End If
  
  'if this is last row OR only one row left
  If fgGlobals.Rows = 2 Or RowIndex = fgGlobals.Rows - 1 Then
    'set it to blank
    fgGlobals.TextMatrix(RowIndex, ctName) = vbNullString
    fgGlobals.TextMatrix(RowIndex, ctValue) = vbNullString
  Else
    'remove it
    fgGlobals.RemoveItem RowIndex
  End If
  
  'list has changed
  MarkAsChanged
  
  'adjust column widths by resizing
  Form_Resize
End Sub

Private Function ScrollBarVisible() As Boolean
  'returns true if grid scrollbar is visible,
  ' false if it is not
  
  With fgGlobals
    If .Rows = 1 Then
      'no scrollbar
      ScrollBarVisible = False
    Else
      'check if bottom of bottom row is less than height of grid
      '(all rows are same height)
      If .RowPos(.Rows - 1) + .RowHeight(1) < .Height Then
        'no scrollbar
        ScrollBarVisible = False
      Else
        'scrollbar is visible
        ScrollBarVisible = True
      End If
    End If
  End With
End Function

Private Sub SetEditMenu()

  Dim strTemp As String
  
  On Error GoTo ErrHandler
  
  With frmMDIMain
    .mnuEdit.Enabled = True
    'redo, find, find again, replace and custom always hidden
    .mnuEBar1.Visible = False
    .mnuEFind.Visible = False
    .mnuERedo.Visible = False
    .mnuEFindAgain.Visible = False
    .mnuEReplace.Visible = False
    .mnuEBar2.Visible = False
    .mnuECustom1.Visible = False
    .mnuECustom2.Visible = False
    .mnuECustom3.Visible = False
    .mnuECustom4.Visible = False
    
    'undo is visible, but always disabled,
    .mnuEUndo.Visible = True
    .mnuEBar0.Visible = True
    If UndoCol.Count > 0 Then
      .mnuEUndo.Enabled = True
      .mnuEUndo.Caption = "&Undo " & LoadResString(GLBUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
    Else
      .mnuEUndo.Enabled = False
      .mnuEUndo.Caption = "&Undo" & vbTab & "Ctrl+Z"
    End If
    
    'cut/copy is enabled if selection is NOT on last row
    .mnuECut.Visible = True
    .mnuECut.Enabled = (fgGlobals.Row <> fgGlobals.Rows - 1)
    .mnuECut.Caption = "C&ut" & vbTab & "Ctrl+X"
    .mnuECopy.Visible = True
    .mnuECopy.Enabled = (fgGlobals.Row <> fgGlobals.Rows - 1)
    .mnuECopy.Caption = "&Copy" & vbTab & "Ctrl+C"
    
    'paste available if globals data is on clipboard
    .mnuEPaste.Visible = True
    .mnuEPaste.Caption = "&Paste" & vbTab & "Ctrl+V"
    If UBound(GlobalsClipboard()) > 0 Then
      .mnuEPaste.Enabled = True
    ElseIf Clipboard.GetFormat(vbCFText) Then
      'if text on clipboard has globals format (#define ....)
      strTemp = Clipboard.GetText(vbCFText)
      'check for define marker
      If LCase$(Left$(strTemp, 8)) = DEF_MARKER Then
        .mnuEPaste.Enabled = True
      Else
        .mnuEPaste.Enabled = False
      End If
    Else
      .mnuEPaste.Enabled = False
    End If
    
    'delete same as cut
    .mnuEDelete.Visible = True
    .mnuEDelete.Enabled = .mnuECut.Enabled
    .mnuEDelete.Caption = "&Delete" & vbTab & "Del"
    
    'clear will wipe out entire globals list (doesn't affect reserved grid)
    .mnuEClear.Visible = True
    .mnuEClear.Enabled = True
    .mnuEClear.Caption = "Clear List" & vbTab & "Shift+Del"
    
    'insert moves cursor to last row, and begins editing
    .mnuEInsert.Visible = True
    .mnuEInsert.Enabled = True
    .mnuEInsert.Caption = "Insert Row" & vbTab & "Shift+Ins"
    
    'selectall always available
    .mnuESelectAll.Visible = True
    .mnuESelectAll.Enabled = True
    .mnuESelectAll.Caption = "Select &All" & vbTab & "Ctrl+A"
    
    'ECustom1 visible if game loaded
    .mnuECustom1.Visible = GameLoaded
    ' enabled if selection mode is free
    .mnuECustom1.Enabled = (fgGlobals.SelectionMode = flexSelectionFree)
    .mnuECustom1.Caption = "&Find in Logics" & vbTab & "Shift+Ctrl+F"
    .mnuEBar2.Visible = GameLoaded
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function AskClose() As Boolean

  Dim rtn As VbMsgBoxResult
  
  'assume ok to close
  AskClose = True
  
  On Error Resume Next
  
  'if changed
  If IsChanged Then
    'ask if should save first
    rtn = MsgBox("Do you want to save changes to this global defines list?", vbQuestion + vbYesNoCancel, "Global Defines")
    
    Select Case rtn
    Case vbYes
      'save
      MenuClickSave
      
    Case vbCancel
      AskClose = False
    End Select
  End If
End Function

Private Sub SortGlobals(ByVal lngCol As Long, NextUndo As GlobalsUndo)

  'sorts the global list by column; if column is -1, it means restore
  'the list from the current Undo object;
  Dim i As Long, j As Long
  Dim lngSwapRow As Long, lngCount As Long
  Dim strTemp As String, lngTemp As Long, blnBold As Boolean
  Dim lngOrder() As Long
  Dim tmpDefine As TDefine, blnSorted As Boolean
  
  On Error GoTo ErrHandler
  
  'number of items
  lngCount = fgGlobals.Rows - 2
  
  'if nothing to sort (need at least two)
  If lngCount < 2 Then
    Exit Sub
  End If
  
  #If DEBUGMODE <> 1 Then
    SendMessage fgGlobals.hWnd, WM_SETREDRAW, 0, 0
  #End If
  
  'show wait cursor
  WaitCursor
  
  'use a temp array to track original order
  ReDim lngOrder(lngCount - 1)
  'if NOT resetting from an Undo object,
  If lngCol <> -1 Then
    'create the undo object
    Set NextUndo = New GlobalsUndo
    With NextUndo
      .UDAction = udgSort
      'can't access the members of the
      'array directly, so we will use
      'a local number array
      For i = 0 To lngCount - 1
        lngOrder(i) = i
      Next i
    End With
  Else
    For i = 0 To lngCount - 1
      lngOrder(i) = NextUndo.UDDefine(i).Type
    Next i
  End If
  
  'step through all rows except last
  For i = 1 To lngCount - 1
    'set swap row to starting row
    lngSwapRow = i
    'compare the swap row to all rows past this one
    For j = i + 1 To lngCount
      'should row j be above swaprow?
      Select Case lngCol
      Case ctName
        If StrComp(fgGlobals.TextMatrix(j, ctName), fgGlobals.TextMatrix(lngSwapRow, ctName), vbTextCompare) = -1 Then
          'j is new swap row
          lngSwapRow = j
        End If
        
      Case ctValue
        'values are trickier to sort; need to take into account presence of plain numbers
        'as well as normal argument types and literal strings
        
        'if isnumeric,
        If IsNumeric(fgGlobals.TextMatrix(j, ctValue)) Then
          'if test row is a number, sort depends on what
          'the current swap row is;
          'if current swap row is NOT a number, always move the
          'number up
          'if current swap row IS a number, only move up if
          'the test row is less than swap row
          If IsNumeric(fgGlobals.TextMatrix(lngSwapRow, ctValue)) Then
            If Val(fgGlobals.TextMatrix(j, ctValue)) < Val(fgGlobals.TextMatrix(lngSwapRow, ctValue)) Then
              lngSwapRow = j
            End If
          Else
            lngSwapRow = j
          End If
        Else
          'if test row is NOT a number, sort depends on what the
          'current swap row is;
          'if current swap row is a number, don't swap
          'if current swap row is NOT a number, do a text compare
          'BUT, within each arg type, we want to sort by number value; not string value
          '(i.e. we want v1,v2,v11,v20... NOT v1,v11,v2,v20...
          If Not IsNumeric(fgGlobals.TextMatrix(lngSwapRow, ctValue)) Then
            'check first letter ONLY at first
            If StrComp(Left$(fgGlobals.TextMatrix(j, ctValue), 1), Left$(fgGlobals.TextMatrix(lngSwapRow, ctValue), 1), vbTextCompare) = -1 Then
              'swap
              lngSwapRow = j
              
            'if both first letters are the same (this will also handle string assignments)
            ElseIf StrComp(Left$(fgGlobals.TextMatrix(j, ctValue), 1), Left$(fgGlobals.TextMatrix(lngSwapRow, ctValue), 1), vbTextCompare) = 0 Then
              'are both numeric?
              If IsNumeric(Right$(fgGlobals.TextMatrix(j, ctValue), Len(fgGlobals.TextMatrix(j, ctValue)) - 1)) And IsNumeric(Right$(fgGlobals.TextMatrix(lngSwapRow, ctValue), Len(fgGlobals.TextMatrix(lngSwapRow, ctValue)) - 1)) Then
                'swap only if Value of j row is less than Value of swap row
                If Val(Right$(fgGlobals.TextMatrix(j, ctValue), Len(fgGlobals.TextMatrix(j, ctValue)) - 1)) < Val(Right$(fgGlobals.TextMatrix(lngSwapRow, ctValue), Len(fgGlobals.TextMatrix(lngSwapRow, ctValue)) - 1)) Then
                  lngSwapRow = j
                End If
              Else
                'swap if string is less than
                If StrComp(fgGlobals.TextMatrix(j, ctValue), fgGlobals.TextMatrix(lngSwapRow, ctValue), vbTextCompare) = -1 Then
                  lngSwapRow = j
                End If
              End If
            End If
          End If
        End If
       
      Case -1
        'when restoring from undo, we only care about what original order was - much easier to determine
        'if swap is required!
        If lngOrder(j - 1) < lngOrder(lngSwapRow - 1) Then
          'j is the new swap row
          lngSwapRow = j
        End If
      End Select
    Next j
    
    'if rows need to be swapped
    If lngSwapRow <> i Then
      With fgGlobals
        'swap name
        strTemp = .TextMatrix(i, ctName)
        .TextMatrix(i, ctName) = .TextMatrix(lngSwapRow, ctName)
        .TextMatrix(lngSwapRow, ctName) = strTemp
        'swap Value
        strTemp = .TextMatrix(i, ctValue)
        .TextMatrix(i, ctValue) = .TextMatrix(lngSwapRow, ctValue)
        .TextMatrix(lngSwapRow, ctValue) = strTemp
        'swap original name
        strTemp = .TextMatrix(i, 0)
        .TextMatrix(i, 0) = .TextMatrix(lngSwapRow, 0)
        .TextMatrix(lngSwapRow, 0) = strTemp
        'swap comment
        strTemp = .TextMatrix(i, ctComment)
        .TextMatrix(i, ctComment) = .TextMatrix(lngSwapRow, ctComment)
        .TextMatrix(lngSwapRow, ctComment) = strTemp
        'also swap the order list
        lngTemp = lngOrder(i - 1)
        lngOrder(i - 1) = lngOrder(lngSwapRow - 1)
        lngOrder(lngSwapRow - 1) = lngTemp
        
        'if either row, but not both, contain
        'an override, then the color and
        'bold status of the two rows needs to
        'be swapped
        '(only name column gets highlighted)
        .Col = ctName
        
        'check lngSwapRow first
        .Row = lngSwapRow
        'note whether this cell is an override or not
        blnBold = .CellFontBold
        
        'now select i row
        .Row = i
        'if override status is different
        'then we need to swap them
        If .CellFontBold <> blnBold Then
          'make this row match blnBold
          .CellFontBold = blnBold
          If blnBold Then
            .CellForeColor = vbRed
          Else
            .CellForeColor = vbBlack
          End If
          'then toggle the swaprow
          .Row = lngSwapRow
          .CellFontBold = Not blnBold
          If Not blnBold Then
            .CellForeColor = vbRed
          Else
            .CellForeColor = vbBlack
          End If
        End If
      End With
      
      blnSorted = True
      MarkAsChanged
    End If
  Next i
  
  'as long as something was sorted, continue
  If blnSorted Then
    'select first item
    fgGlobals.Row = 1
    fgGlobals.Col = ctName
    If Not fgGlobals.RowIsVisible(1) Then
      fgGlobals.TopRow = fgGlobals.Row
    End If
    
    'if saving for an undo,
    If lngCol <> -1 Then
      'copy the results of the sort into the undo object
      With NextUndo
        .UDCount = lngCount
        For i = 0 To lngCount - 1
          tmpDefine.Type = lngOrder(i)
          .UDDefine(i) = tmpDefine
        Next i
      End With
      'add the undo
      AddUndo NextUndo
    End If
  End If
  
  #If DEBUGMODE <> 1 Then
    SendMessage fgGlobals.hWnd, WM_SETREDRAW, 1, 0
  #End If
  fgGlobals.Refresh
  Set NextUndo = Nothing
  Screen.MousePointer = vbDefault
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Function ValidateDefine(ByVal NewName As String, ByRef NewValue As String, Optional ByVal WarnUser As Boolean = True, Optional ByRef Index As Long = -1) As Boolean

  Dim i As Long, j As Long, strMax As String
  Dim lngValName As Long
  Dim rtn As VbMsgBoxResult, blnNoWarn As Boolean
  Dim blnMark As Boolean, lngInsertRow As Long
  Dim strValue As String, blnReplacing As Boolean
  
  On Error GoTo ErrHandler
  'used by load, import and paste functions to validate a potential define entry
  'if it is valid, it gets added to the grid, optionally at the specified Index location
  'if it is NOT valid, function returns false and nothing gets added
  
  'when a new define gets added, the default value is only
  'set to the current name if the entry is new; if it's a replacement
  'the default name is left alone
  '
  'if the define already exists in the list, the user is given an option
  'to replace the existing define with the new one being added -
  '
  '  if user says YES, the existing define is deleted from the list
  '    in this case, we need to check for an undo action; for the
  '    paste or import function to correctly undo, it will need to
  '    restore the deleted item; in tha case, we pass back the
  '    old value in strValue, and the old row number in Index
  '    calling function (paste or import) will need to determine if
  '    number of rows didn't change in order to tell if a replace occurred
  '
  '  if user says NO, the define is marked invalid and function returns with no change
  
  'validate the name and values seperately
  lngValName = ValidateName(NewName)
  j = ValidateValue(NewValue)
  
  'local copy of insert position
  lngInsertRow = Index
  'local copy of value (in case we need to change the passed value
  'for an Undo action)
  strValue = NewValue
  
  
  'check for error
  If (lngValName >= 1 And lngValName <= 6) Or lngValName = 14 Or (j >= 101 And j <= 103) Then
    'just exit
    Exit Function
  End If
  
  'check for conditions that can be corrected or ignored
  Select Case lngValName
  Case 7  'duplicate name
    'find the existing define (remember, it's not case sensitive)
    For i = 1 To fgGlobals.Rows - 1
      If StrComp(fgGlobals.TextMatrix(i, ctName), NewName, vbTextCompare) = 0 Then
        Exit For
      End If
    Next i
    
    'worst case scenario- no match found!!! if so, it's some kind of error
    If i = fgGlobals.Rows Then
      '*'Debug.Assert False
      'same as error
      Exit Function
    End If
    
    'if user needs warning, AND value is NOT the same
    If WarnUser And fgGlobals.TextMatrix(i, ctValue) <> strValue Then
      'does user want to replace this one?
      If Settings.WarnDupGName Then
        rtn = MsgBoxEx(ChrW$(39) & NewName & "' is already defined in this list. Do" & vbNewLine & "you want to replace it with this define?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Duplicate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", "Don't show this warning again.", blnNoWarn)
        Settings.WarnDupGName = Not blnNoWarn
        'if now hiding, update settings file
        If Not Settings.WarnDupGName Then
          WinAGISettingsList.WriteSetting(sGENERAL, "WarnDupGName", Settings.WarnDupGName
        End If
        
        If rtn = vbNo Then
          'same as error
          Exit Function
        End If
      End If
    End If
    
    'replacing
    blnReplacing = True
    
    'if Value DOES match
    If fgGlobals.TextMatrix(i, ctValue) = strValue Then
      'reset Value error so there isnt a useless warning
      j = 0
    End If
    
    'remove old one
    'if undo is active,
    If Settings.GlobalUndo <> 0 Then
      ' send the value and row back to calling function
      Index = i
      NewValue = fgGlobals.TextMatrix(i, ctValue)
      ' save defname, value, comment
      RepDefine.Default = fgGlobals.TextMatrix(i, ctDefault)
      RepDefine.Name = fgGlobals.TextMatrix(i, ctName)
      RepDefine.Value = fgGlobals.TextMatrix(i, ctValue)
      RepDefine.Comment = fgGlobals.TextMatrix(i, ctComment)
    End If
      
    'delete row i
    fgGlobals.RemoveItem i
    'set flag so import/past knows a replacement occurred
    blnDuplicates = True
    'update value validation (it might be an override)
    lngValName = ValidateName(NewName)
    If lngValName >= 8 And lngValName <= 13 Then
      blnMark = True
    End If
    'if the deleted item is in front of insert pos
    If i < lngInsertRow Then
      lngInsertRow = lngInsertRow - 1
    End If
    
  Case 8 To 13  'reserved var/flag/constant
    'always set flag so the entry gets marked as an override
    blnReserved = True
    'set flag so entry gets marked
    blnMark = True
    
    'if user needs warning
    If WarnUser Then
      'does user really want to re-define this name?
      If Settings.WarnResOvrd Then
        rtn = MsgBoxEx(ChrW$(39) & NewName & "' is a reserved variable, flag or constant. Are you sure" & vbNewLine & "you want to override it?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Duplicate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", "Don't show this warning again.", blnNoWarn)
        Settings.WarnResOvrd = Not blnNoWarn
        'if now hiding update settings file
        If Not Settings.WarnResOvrd Then
          WinAGISettingsList.WriteSetting(sGENERAL, "WarnResOvrd", Settings.WarnResOvrd
        End If
        
        If rtn = vbNo Then
          'same as error
          Exit Function
        End If
      End If
    End If
  End Select
  
  Select Case j
'  Case 101 'no Value
'  Case 102 'Value is not an integer
'  Case 103 'Value contains an invalid argument Value
  Case 104 'Value is not a string, number or argument marker
    'fix the define Value to be a string
    If Asc(strValue) <> 34 Then
      strValue = QUOTECHAR & strValue
    End If
    If Asc(Right$(strValue, 1)) <> 34 Then
      strValue = strValue & QUOTECHAR
    End If
    
  Case 105, 106 ' Value matches a reserved name, or Value matches a previously defined global name
    'if user needs warning
    If WarnUser Then
      'does user really want to re-define this name?
      If Settings.WarnDupGVal Then
        rtn = MsgBoxEx(ChrW$(39) & strValue & "' is already defined in this list, or as a reserved variable, flag or" & vbNewLine & _
                       "constant. Do you really want to have duplicate definitions for this Value?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Duplicate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", "Don't show this warning again.", blnNoWarn)
        Settings.WarnDupGVal = Not blnNoWarn
        
        'if now hiding update settings list
        If Not Settings.WarnDupGVal Then
          WinAGISettingsList.WriteSetting(sGENERAL, "WarnDupGVal", Settings.WarnDupGVal
        End If
        
        If rtn = vbNo Then
          'same as error
          Exit Function
        End If
      End If
    End If
    
  Case 107 'Invalid string value
    If WarnUser Then
      'does user really want invalid string?
      If Settings.WarnInvalidStrVal Then
        If InterpreterVersion = "2.089" Or InterpreterVersion = "2.272" Or InterpreterVersion = "3.002149" Then
          strMax = "11"
        Else
          strMax = "23"
        End If
        rtn = MsgBoxEx(ChrW$(39) & NewValue & "' is an invalid string value (limit is s0 - s" & strMax & ")." & vbNewLine & _
                       "Do you really want to have an invalid string definition?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, _
                       "Invalid String Value", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", "Don't show this warning again.", blnNoWarn)
        Settings.WarnInvalidStrVal = Not blnNoWarn
        
        'if now hiding update settings list
        If Not Settings.WarnInvalidStrVal Then
          WinAGISettingsList.WriteSetting(sGENERAL, "WarnInvalidStrVal", Settings.WarnInvalidStrVal
        End If
        
        If rtn = vbNo Then
          'same as error
          Exit Function
        End If
      End If
    End If
    
  Case 108 'Invalid controller value
    If WarnUser Then
      'does user really want invalid controller?
      If Settings.WarnInvalidCtlVal Then
        rtn = MsgBoxEx(ChrW$(39) & NewValue & "' is an invalid controller value (limit is 0 - 49)." & vbNewLine & _
                       "Do you really want to have an invalid controller definition?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, _
                       "Invalid Controller Value", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", "Don't show this warning again.", blnNoWarn)
        Settings.WarnInvalidCtlVal = Not blnNoWarn
        
        'if now hiding update settings list
        If Not Settings.WarnInvalidCtlVal Then
          WinAGISettingsList.WriteSetting(sGENERAL, "WarnInvalidCtlVal", Settings.WarnInvalidCtlVal
        End If
        
        If rtn = vbNo Then
          'same as error
          Exit Function
        End If
      End If
    End If
  End Select
  
  'no error- ok to add
  
  'if no position
  If lngInsertRow = -1 Then
    'add it
    If blnReplacing Then
      'keep existing default
      fgGlobals.AddItem RepDefine.Default & vbTab & Trim$(NewName) & vbTab & Trim$(strValue)
    Else
      'set default to current name
      fgGlobals.AddItem Trim$(NewName) & vbTab & Trim$(NewName) & vbTab & Trim$(strValue)
    End If
    'set mark Value
    i = fgGlobals.Rows - 1
  Else
    'add it at Index position
    If blnReplacing Then
      'keep existing default
      fgGlobals.AddItem RepDefine.Default & vbTab & Trim$(NewName) & vbTab & Trim$(strValue), lngInsertRow
    Else
      'set default to current name
      fgGlobals.AddItem Trim$(NewName) & vbTab & Trim$(NewName) & vbTab & Trim$(strValue), lngInsertRow
    End If
    i = lngInsertRow
  End If
  
  ' if not loading (form isn't visible yet)
  If Me.Visible Then
    MarkAsChanged
  End If
  'if this is a reserved define that needs to be marked
  If blnMark Then
    'mark it by highlighting
    fgGlobals.Row = i
    fgGlobals.Col = ctName
    fgGlobals.CellFontBold = True
    fgGlobals.CellForeColor = vbRed
  End If
  
  ValidateDefine = True
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function
Public Function ValidateName(NewDefName As String) As Long
  'validates if a define name is agreeable or not
  'returns zero if ok;
  'error Value if not
  
  '1 = no name
  '2 = name is numeric
  '3 = name is command
  '4 = name is test command
  '5 = name is a compiler keyword
  '6 = name is an argument marker
  '7 = name is already globally defined
  '8 = name is reserved variable name
  '9 = name is reserved flag name
  '10 = name is reserved number constant
  '11 = name is reserved object constant
  '12 = name is reserved message constant
  '13 = name is reserved string constant
  '14 = name contains improper character
  '15 = name is a resource ID
  
  Dim i As Long
  Dim tmpDefines() As TDefine
  
  On Error GoTo ErrHandler
  
  'if no name,
  If LenB(NewDefName) = 0 Then
    ValidateName = 1
    Exit Function
  End If
  
  'name cant be numeric
  If IsNumeric(NewDefName) Then
    ValidateName = 2
    Exit Function
  End If
  
  'check against regular commands
  For i = 0 To Commands.Count
    If StrComp(NewDefName, Commands(i).Name, vbTextCompare) = 0 Then
      ValidateName = 3
      Exit Function
    End If
  Next i
  
  'check against test commands
  For i = 0 To TestCommands.Count
    If StrComp(NewDefName, TestCommands(i).Name, vbTextCompare) = 0 Then
      ValidateName = 4
      Exit Function
    End If
  Next i
  
  'check against keywords
  Select Case LCase$(NewDefName)
  Case "if", "else", "goto"
    ValidateName = 5
    Exit Function
  End Select
      
  'check against variable/flag/controller/string/message names
  Select Case Asc(LCase$(NewDefName))
  '     v    f    m    o    i    s    w    c
  Case 118, 102, 109, 111, 105, 115, 119, 99
    If IsNumeric(Right$(NewDefName, Len(NewDefName) - 1)) Then
      ValidateName = 6
      Exit Function
    End If
  End Select
  
  'check against existing globals (skip editrow)
  '(do this check after checking reserved because reserved defines may be displayed
  'also, names are specifically NOT case sensitive
  For i = 1 To fgGlobals.Rows - 1
    If (StrComp(NewDefName, fgGlobals.TextMatrix(i, ctName), vbTextCompare) = 0) And i <> EditRow Then
      ValidateName = 7
      Exit Function
    End If
  Next i
  
  If LogicCompiler.UseReservedNames Then
    'check against reserved names
    tmpDefines = LogicSourceSettings.ReservedDefines(atVar)
    For i = 0 To UBound(tmpDefines)
      If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
        ValidateName = 8
        Exit Function
      End If
    Next i
    tmpDefines = LogicSourceSettings.ReservedDefines(atFlag)
    For i = 0 To UBound(tmpDefines)
      If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
        ValidateName = 9
        Exit Function
      End If
    Next i
    tmpDefines = LogicSourceSettings.ReservedDefines(atNum)
    For i = 0 To UBound(tmpDefines)
      If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
        ValidateName = 10
        Exit Function
      End If
    Next i
    tmpDefines = LogicSourceSettings.ReservedDefines(atSObj)
    For i = 0 To UBound(tmpDefines)
      If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
        ValidateName = 11
        Exit Function
      End If
    Next i
    tmpDefines = LogicSourceSettings.ReservedDefines(atDefStr)
    For i = 0 To UBound(tmpDefines)
      If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
        ValidateName = 12
        Exit Function
      End If
    Next i
    tmpDefines = LogicSourceSettings.ReservedDefines(atStr)
    For i = 0 To UBound(tmpDefines)
      If StrComp(NewDefName, tmpDefines(i).Name, vbTextCompare) = 0 Then
        ValidateName = 13
        Exit Function
      End If
    Next i
  End If
  
  'check name against improper character list
  For i = 1 To Len(NewDefName)
    Select Case Asc(Mid$(NewDefName, i, 1))
'                                                                            1         1         1
'        3       4    4    5         6         7         8         9         0         1         2
'        234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567
'NOT OK  x!"   &'()*+,- /          :;<=>?                           [\]^ `                          {|}~x
'    OK     #$%        . 0123456789      @ABCDEFGHIJKLMNOPQRSTUVWXYZ    _ abcdefghijklmnopqrstuvwxyz    
    Case 32 To 34, 38 To 45, 47, 58 To 63, 91 To 94, 96, Is >= 123
      ValidateName = 14
      Exit Function
    End Select
  Next i
  
  'check name against resource IDs
  For i = 0 To 1023
    If IDefLookup(i).Type < 11 Then
      If StrComp(NewDefName, IDefLookup(i).Name, vbTextCompare) = 0 Then
        ValidateName = 15
        Exit Function
      End If
    End If
  Next i
  
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function
Private Function ValidateValue(ByVal NewDefValue As String) As Long
  'validates if a define Value is agreeable or not
  'returns zero if ok;
  'error Value if not

  '101 = No value
  '102 = Value is non integer number
  '103 = Value contains an invalid argument Value
  '104 = Value is not a string, number or argument marker
  '105 = Value is already defined by a reserved name
  '106 = Value is already defined by a global name
  '107 = Invalid string value
  '108 = Invalid controller value

  Dim strVal As String
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'check for no value
  If LenB(NewDefValue) = 0 Then
    ValidateValue = 101
    Exit Function
  End If
  
  'values must be a variable/flag/etc, string, or a number
  If Not IsNumeric(NewDefValue) Then
    'if Value is an argument marker
    Select Case Asc(LCase$(NewDefValue))
    '     v    f    m    o    i    s    w    c
    Case 118, 102, 109, 111, 105, 115, 119, 99
      'if rest of Value is integer,
      strVal = Right$(NewDefValue, Len(NewDefValue) - 1)
      'if it is numeric
      If IsNumeric(strVal) Then
        ' is it an integer?
        If Int(strVal) <> Val(strVal) Then
          ValidateValue = 102
          Exit Function
        End If
      
        'if Value is not between 0-255
        If Val(strVal) < 0 Or Val(strVal) > 255 Then
          ValidateValue = 103
          Exit Function
        End If
        
        'if a string
        If Asc(LCase$(NewDefValue)) = 115 Then
          ' if in game
          If GameLoaded Then
            'limit values to 0-23 or 0-11 depending on version
            If (Val(strVal) > 23) Or (Val(strVal) > 11 And (InterpreterVersion = "2.089" Or InterpreterVersion = "2.272" Or InterpreterVersion = "3.002149")) Then
              ValidateValue = 107
              Exit Function
            End If
          Else
           'limit to 23
            If Val(strVal) > 23 Then
              ValidateValue = 107
              Exit Function
            End If
          End If
        End If
        
        'if a controller
        If Asc(LCase$(NewDefValue)) = 99 Then
         'limit to 49
          If Val(strVal) > 49 Then
            ValidateValue = 108
            Exit Function
          End If
        End If
        
        'if using reserved words
        If LogicCompiler.UseReservedNames Then
          'if Value is a variable
          If Asc(LCase$(NewDefValue)) = 118 Then
            'if already defined as a reserved variable
            If Val(strVal) <= 26 Then
              'set error
              ValidateValue = 105
              Exit Function
            End If
          End If
          
          'if Value is a flag
          If Asc(LCase$(NewDefValue)) = 102 Then
            'if already defined as a reserved variable
            If Val(strVal) <= 16 Then
              'set error
              ValidateValue = 105
              Exit Function
            ElseIf Val(strVal) = 20 And Val(InterpreterVersion) >= 3.002102 Then
              'set error
              ValidateValue = 105
              Exit Function
            End If
          End If
          
          'if Value is o0
          If NewDefValue = "o0" Then
            'set error
            ValidateValue = 105
            Exit Function
          End If
          
          'if Value is s0
          If NewDefValue = "s0" Then
            'set error
            ValidateValue = 105
            Exit Function
          End If
        End If
        
        'check against existing globals (skip editrow)
        For i = 1 To fgGlobals.Rows - 1
          If (NewDefValue = fgGlobals.TextMatrix(i, ctValue)) And i <> EditRow Then
            'set error
            ValidateValue = 106
            Exit Function
          End If
        Next i
        
        'Value is ok
        Exit Function
      End If
    End Select
    
    'could be a string
    'check Value for string delimiters in Value
    If Asc(NewDefValue) <> 34 And Asc(Right$(NewDefValue, 1)) <> 34 Then
      ValidateValue = 104
      Exit Function
    End If
  Else
    'is it a non-integer?
    If Int(NewDefValue) <> Val(NewDefValue) Then
      ValidateValue = 102
      Exit Function
    End If
    
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function
Private Function ValidateInput() As Boolean
  
  Dim rtn As Long, msgRtn As VbMsgBoxResult
  Dim strText As String, blnBlank As Boolean, strVal As String
  Dim strErrMsg As String, blnOverride As Boolean
  Dim blnNoWarn As Boolean, NextUndo As GlobalsUndo
  
  'checks input for new name/Value; if OK, add it to grid
  On Error GoTo ErrHandler
  
  'if no change made, that's OK
  Select Case EditCol
  Case ctName  'name
    'if name in textbox matches previous Value
    If txtEditName.Text = EditDefine.Name Then
      'no change made; return success
      ValidateInput = True
      Exit Function
    End If
    
    'if blank, do we consider it deletion of entire line?
    If Len(txtEditName.Text) = 0 Then
      Select Case Settings.DelBlankG
      Case 0
        'get user's response
        rtn = MsgBoxEx("Blank global define names are not allowed. Delete this global define?", _
              vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Delete Blank Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", _
              "Always take this action", blnNoWarn)
        If blnNoWarn Then
          If rtn = vbNo Then Settings.DelBlankG = 1
          If rtn = vbYes Then Settings.DelBlankG = 2
          WinAGISettingsList.WriteSetting(sGENERAL, "DelBlankG", Settings.DelBlankG
        End If
      Case 1
        rtn = vbNo
      Case 2
        rtn = vbYes
      End Select
      
      'if no, then let the validation function handle it
      'but if yes, then exit; calling function will know what to do
      If rtn = vbYes Then
        ValidateInput = True
        Exit Function
      End If
    End If
     
  Case ctValue  'Value
    'if Value in textbox matches previous Value
    If txtEditValue.Text = EditDefine.Value And Len(txtEditValue.Text) <> 0 Then
      'no change made; return success
      ValidateInput = True
      Exit Function
    End If
  End Select
  
  'make local copies of name or Value
  If EditCol = ctName Then
    strText = txtEditName.Text
  Else
    strText = txtEditValue.Text
  End If
  
  'validate the define name and Value
  If EditCol = ctName Then
    rtn = ValidateName(strText)
  Else
    rtn = ValidateValue(strText)
  End If
  
  'if return code indicates error
  If rtn <> 0 Then
    Select Case rtn
    Case 1 ' no name
      strErrMsg = "Define name cannot be blank."
      EditCol = ctName
    Case 2 ' name is numeric
      strErrMsg = "Define names cannot be numeric."
      EditCol = ctName
    Case 3 ' name is command
      strErrMsg = ChrW$(39) & strText & "' is an AGI command, and cannot be redefined"
      EditCol = ctName
    Case 4 ' name is test command
      strErrMsg = ChrW$(39) & strText & "' is an AGI test command, and cannot be redefined"
      EditCol = ctName
    Case 5 ' name is a compiler keyword
      strErrMsg = ChrW$(39) & strText & "' is a compiler reserved word, and cannot be redefined"
      EditCol = ctName
    Case 6 ' name is an argument marker
      strErrMsg = "invalid define statement - define names cannot be argument markers"
      EditCol = ctName
    Case 7 ' name is already defined
      strErrMsg = ChrW$(39) & strText & "' is already in use as a global define"
      EditCol = ctName
    Case 8 ' name is reserved variable name
      'verify user wants to override
      If Settings.WarnResOvrd Then
        msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is a reserved variable name." & vbNewLine & _
                 "Are you sure you want to override it?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Validate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", _
                 "Don't show this warning again.", blnNoWarn)
        Settings.WarnResOvrd = Not blnNoWarn
      Else
        'override without asking
        msgRtn = vbYes
      End If
      
      If msgRtn = vbYes Then
        blnOverride = True
        rtn = 0
      Else
        'set error code to -1
        rtn = -1
        'set column to name
        EditCol = ctName
      End If
    Case 9 ' name is reserved flag name
      'verify user wants to override
      If Settings.WarnResOvrd Then
        msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is a reserved flag name." & vbNewLine & _
                 "Are you sure you want to override it?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Validate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", _
                 "Don't show this warning again.", blnNoWarn)
        Settings.WarnResOvrd = Not blnNoWarn
      Else
        'override without asking
        msgRtn = vbYes
      End If
      
      If msgRtn = vbYes Then
        blnOverride = True
        rtn = 0
      Else
        'set error code to -1
        rtn = -1
        'set column to name
        EditCol = ctName
      End If
      
    Case 10 ' name is reserved number constant
      'verify user wants to override
      If Settings.WarnResOvrd Then
        msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is a reserved number constant." & vbNewLine & _
                 "Are you sure you want to override it?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Validate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", _
                 "Don't show this warning again.", blnNoWarn)
        Settings.WarnResOvrd = Not blnNoWarn
      Else
        'override without asking
        msgRtn = vbYes
      End If
      
      If msgRtn = vbYes Then
        blnOverride = True
        rtn = 0
      Else
        'set error code to -1
        rtn = -1
        'set column to name
        EditCol = ctName
      End If
      
    Case 11 ' name is reserved object constant
      'verify user wants to override
      If Settings.WarnResOvrd Then
        msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is a reserved object name." & vbNewLine & _
                 "Are you sure you want to override it?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Validate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", _
                 "Don't show this warning again.", blnNoWarn)
        Settings.WarnResOvrd = Not blnNoWarn
      Else
        'override without asking
        msgRtn = vbYes
      End If
      
      If msgRtn = vbYes Then
        blnOverride = True
        rtn = 0
      Else
        'set error code to -1
        rtn = -1
        'set column to name
        EditCol = ctName
      End If
    Case 12 ' name is reserved message constant
      'verify user wants to override
      If Settings.WarnResOvrd Then
        msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is a reserved message constant." & vbNewLine & _
                 "Are you sure you want to override it?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Validate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", _
                 "Don't show this warning again.", blnNoWarn)
        Settings.WarnResOvrd = Not blnNoWarn
      Else
        'override without asking
        msgRtn = vbYes
      End If
      
      If msgRtn = vbYes Then
        blnOverride = True
        rtn = 0
      Else
        'set error code to -1
        rtn = -1
        'set column to name
        EditCol = ctName
      End If
    Case 13 ' name is reserved string constant
      If Settings.WarnResOvrd Then
        msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is a reserved string constant." & vbNewLine & _
                 "Are you sure you want to override it?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Validate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", _
                 "Don't show this warning again.", blnNoWarn)
        Settings.WarnResOvrd = Not blnNoWarn
      Else
        'override without asking
        msgRtn = vbYes
      End If
      
      If msgRtn = vbYes Then
        blnOverride = True
        rtn = 0
      Else
        'set error code to -1
        rtn = -1
        'set column to name
        EditCol = ctName
      End If
    
      
    Case 14 ' name contains improper character
      strErrMsg = "Invalid character in define name: !" & QUOTECHAR & "&'()*+,-/:;<=>?[\]^`{|}~ and spaces are not allowed"
      EditCol = ctName
      
    Case 15 ' name is resource id
      'verify user wants to override
      msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is a resource ID." & vbNewLine & _
               "Are you sure you want to override it?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Validate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax")
      
      If msgRtn = vbYes Then
        rtn = 0
      Else
        'set error code to -1 (message already shown)
        rtn = -1
        'set column to name
        EditCol = ctName
      End If
      
    Case 101 ' no Value
      strErrMsg = "Define Value cannot be blank."
      EditCol = ctValue
    Case 102 ' Value is not an integer
      strErrMsg = "Argument Value must be an integer."
      EditCol = ctValue
    
    Case 103 ' Value contains an invalid argument Value
      strErrMsg = "Argument Value out of range (must be 0 - 255)"
      EditCol = ctValue
    Case 104 ' Value is not a string, number or argument marker
      'fix the define Value to be a string
      If Asc(txtEditValue.Text) <> 34 Then
        txtEditValue.Text = QUOTECHAR & txtEditValue.Text
      End If
      If Asc(Right$(txtEditValue.Text, 1)) <> 34 Then
        txtEditValue.Text = txtEditValue.Text & QUOTECHAR
      End If
      'reset return to zero, so error is ignored
      rtn = 0
    Case 105, 106 ' Value is already defined by a reserved/global name
      'does user really want to duplicate define?
      If Settings.WarnDupGVal Then
        msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is already defined in this list, or as a reserved variable, flag or" & vbNewLine & _
                       "constant. Do you really want to have duplicate definitions for this Value?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Duplicate Global Define", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", "Don't show this warning again.", blnNoWarn)
        Settings.WarnDupGVal = Not blnNoWarn
        
        'if now hiding update settings list
        If Not Settings.WarnDupGVal Then
          WinAGISettingsList.WriteSetting(sGENERAL, "WarnDupGVal", Settings.WarnDupGVal
        End If
        
        If msgRtn = vbNo Then
          'same as error
          rtn = -1
          EditCol = ctValue
        Else
          'reset return to zero, so error is ignored
          rtn = 0
        End If
      Else
        'reset return to zero, so error is ignored
        rtn = 0
      End If
    Case 107 'Invalid string value
      'does user really want invalid string?
      If Settings.WarnInvalidStrVal Then
        If InterpreterVersion = "2.089" Or InterpreterVersion = "2.272" Or InterpreterVersion = "3.002149" Then
          strVal = "11"
        Else
          strVal = "23"
        End If
        msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is an invalid string value (limit is s0 - s" & strVal & ")." & vbNewLine & _
                       "Do you really want to have an invalid string definition?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, _
                       "Invalid String Value", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", "Don't show this warning again.", blnNoWarn)
        Settings.WarnInvalidStrVal = Not blnNoWarn
        
        'if now hiding update settings list
        If Not Settings.WarnInvalidStrVal Then
          WinAGISettingsList.WriteSetting(sGENERAL, "WarnInvalidStrVal", Settings.WarnInvalidStrVal
        End If
        
        If msgRtn = vbNo Then
          'same as error
          rtn = -1
          EditCol = ctValue
        Else
          'reset return to zero, so error is ignored
          rtn = 0
        End If
      Else
        'reset return to zero, so error is ignored
        rtn = 0
      End If
      
    Case 108 'Invalid controller value
      'does user really want invalid controller?
      If Settings.WarnInvalidCtlVal Then
        msgRtn = MsgBoxEx(ChrW$(39) & strText & "' is an invalid controller value (limit is c0 - c49)." & vbNewLine & _
                       "Do you really want to have an invalid controller definition?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, _
                       "Invalid Controller Value", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax", "Don't show this warning again.", blnNoWarn)
        Settings.WarnInvalidCtlVal = Not blnNoWarn
        
        'if now hiding update settings list
        If Not Settings.WarnInvalidCtlVal Then
          WinAGISettingsList.WriteSetting(sGENERAL, "WarnInvalidCtlVal", Settings.WarnInvalidCtlVal
        End If
        
        If msgRtn = vbNo Then
          'same as error
          rtn = -1
          EditCol = ctValue
        Else
          'reset return to zero, so error is ignored
          rtn = 0
        End If
      Else
        'reset return to zero, so error is ignored
        rtn = 0
      End If
      
    End Select
  End If
  
  'if still an error code
  If rtn <> 0 Then
    'force back to edit row
    fgGlobals.Row = EditRow
    'set col to edit col
    fgGlobals.Col = EditCol
    
    'if user tried to redefine a reserved word or resourceid,
    'a msgbox was already displayed; don't need to
    'show msgbox again
    'rtn is set to -1 if the msgbox was already shown
    If rtn <> -1 Then
      'show msgbox
      MsgBoxEx strErrMsg, vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Global Define Syntax Error", WinAGIHelp, "htm\winagi\Global Defines.htm#syntax"
    End If
    Exit Function
  Else
    'make change
    'set font to default values (red for override (name only), black for normal)
    If blnOverride And (EditCol = ctName) Then
      fgGlobals.CellFontBold = True
      fgGlobals.CellForeColor = vbRed
    Else
      fgGlobals.CellFontBold = False
      fgGlobals.CellForeColor = vbBlack
    End If
    
    'put the text back into grid
    If EditCol = ctName Then
      'if inserting, wait until the value is added
      If Not Inserting Then
        If Settings.GlobalUndo <> 0 Then
          'add undo for a change in name
          Set NextUndo = New GlobalsUndo
          With NextUndo
            .UDAction = udgEditName
            .UDCount = 1
            'save the old value
            .UDPos = EditRow
            .UDText = fgGlobals.TextMatrix(EditRow, EditCol)
          End With
          'add to undo
          AddUndo NextUndo
        End If
      End If
      'make the change
      fgGlobals.TextMatrix(EditRow, EditCol) = txtEditName.Text
      
    Else
      If Settings.GlobalUndo <> 0 Then
        'add undo info
        Set NextUndo = New GlobalsUndo
        With NextUndo
          If Inserting Then
            'if inserting, type is adddefine
            .UDAction = udgAddDefine
            Inserting = False
          Else
            'add undo for a change in name
            .UDAction = udgEditValue
            .UDText = fgGlobals.TextMatrix(EditRow, EditCol)
          End If
          .UDCount = 1
          .UDPos = EditRow
        End With
        'add to undo
        AddUndo NextUndo
      End If
      
      'always trim spaces off
      fgGlobals.TextMatrix(EditRow, EditCol) = Trim$(txtEditValue.Text)
    End If
    
    'always clear the MustEditValue flag
    MustEditValue = False
    
    'if inserting, and name has just been validated,
    'must make sure value is valid before finishing the edit
    If (EditCol = ctName) And Inserting Then
      If fgGlobals.TextMatrix(EditRow, ctValue) = "" Then
        'only allowed move is to edit the value next
        MustEditValue = True
      End If
    End If
  End If
  
  'change made; enable save
  MarkAsChanged
  'valid;
  ValidateInput = True
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Function WrapText(ByVal LongText As String) As String

  'inserts carriage returns in LongText so it fits within
  'width of picComment
  
  Dim lngSep As Long
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub fgGlobals_DblClick()
  
  On Error GoTo ErrHandler
  
  'if editing, ignore
  If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
    Exit Sub
  End If
  
  'if forcing edit of value for a new define, we don't
  'want to change the edit row/col
  If MustEditValue Then
    'force row to current edit row
    fgGlobals.Row = EditRow
    fgGlobals.Col = ctValue
    EditCol = ctValue
  Else
    'check for dbl-click on a header
    'check for left-click on header
    If mRow = 0 Then
      'only name or value can be sorted
      If mCol = ctName Or mCol = ctValue Then
        'sort, depending on column
        SortGlobals mCol, Nothing
      End If
      Exit Sub
    End If
    
    'otherwise, save row and column being edited and continue
    EditRow = fgGlobals.Row
    EditCol = fgGlobals.Col
  End If
  
  'save current define in case of cancel
  EditDefine.Name = fgGlobals.TextMatrix(EditRow, ctName)
  EditDefine.Value = fgGlobals.TextMatrix(EditRow, ctValue)

  'begin edit
  Select Case EditCol
  Case ctName
    'disable edit menu
    frmMDIMain.mnuEdit.Enabled = False
    
    'switch to edit name mode
    With txtEditName
      .Move fgGlobals.CellLeft + CellIndentH, fgGlobals.CellTop + fgGlobals.Top + CellIndentV, fgGlobals.CellWidth - CellIndentH ', fgGlobals.CellHeight - CellIndentV
      .Text = fgGlobals.Text
      .Visible = True
      'select all
      .SelStart = 0
      .SelLength = Len(.Text)
      .SetFocus
    End With

  Case ctValue
    'dont edit if this is last row;
    'user has to start edit in name column
    If EditRow = fgGlobals.Rows - 1 Then
      Exit Sub
    End If
    
    'disable edit menu
    frmMDIMain.mnuEdit.Enabled = False
    
    'switch to edit value mode
    With txtEditValue
      'no longer need to force editing of value
      MustEditValue = False
      .Move fgGlobals.CellLeft + CellIndentH, fgGlobals.CellTop + fgGlobals.Top + CellIndentV, fgGlobals.CellWidth - CellIndentH, fgGlobals.CellHeight - CellIndentV
      .Text = fgGlobals.Text
      .Visible = True
      'select all
      .SelStart = 0
      .SelLength = Len(.Text)
      .SetFocus
    End With
    
  Case ctComment
    'dont edit if this is last row;
    'user has to start edit in name column
    If EditRow = fgGlobals.Rows - 1 Then
      Exit Sub
    End If
    
    'disable edit menu
    frmMDIMain.mnuEdit.Enabled = False
    
    'switch to edit comment mode
    With txtEditComment
      'no longer need to force editing of value
      MustEditValue = False
      .Move fgGlobals.CellLeft + CellIndentH, fgGlobals.CellTop + fgGlobals.Top + CellIndentV, fgGlobals.CellWidth - CellIndentH ', fgGlobals.CellHeight - CellIndentV
      .Text = fgGlobals.Text
      .Height = SendMessage(txtEditComment.hWnd, EM_GETLINECOUNT, 0, 0) * lngRowHgt
      .Visible = True
      'select all
      .SelStart = 0
      .SelLength = Len(.Text)
      .SetFocus
    End With
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub fgGlobals_EnterCell()

  SetEditMenu
End Sub

Private Sub fgGlobals_GotFocus()

  'always hide the selection frame
  picSelFrame(0).Visible = False
  picSelFrame(2).Visible = False
  picSelFrame(3).Visible = False
  picSelFrame(1).Visible = False

  If MustEditValue Then
    'force user to edit the value
    
    'move to Value column
    fgGlobals.Col = ctValue
    'and row to current edit row
    fgGlobals.Row = EditRow
    
    'start another edit operation
    fgGlobals_DblClick
  End If
End Sub

Private Sub fgGlobals_KeyDown(KeyCode As Integer, Shift As Integer)


  'look for tab key
  Select Case KeyCode
  Case 9  'tab
    Select Case Shift
    Case 0 ' regular TAB
      With fgGlobals
        'if on name
        If .Col = ctName Then
          'move to value column
          .Col = ctValue
          
        'if on value
        ElseIf .Col = ctValue Then
          'if comment column is visible
          If ShowComment Then
            'move to comment column
            .Col = ctComment
          Else
            'if not on last row
            If .Row < .Rows - 1 Then
              'move to next row
              .Row = .Row + 1
              .Col = ctName
            End If
          End If
          
        'if on comment column
        ElseIf .Col = ctComment Then
          'if not on last row
          If .Row < .Rows - 1 Then
          'move to next row
            .Row = .Row + 1
            .Col = ctName
          End If
        End If
      End With
      
    Case vbShiftMask ' SHIFT+TAB
      With fgGlobals
        'if on name
        If .Col = ctName Then
          'if not at top
          If .Row > 1 Then
            'move to previous row
            .Row = .Row - 1
            
            'if comment column is visible
            If ShowComment Then
              'move to comment column
              .Col = ctComment
            Else
              'move to value column
              .Col = ctValue
            End If
          End If
        
        'if on value
        ElseIf .Col = ctValue Then
          'move to name column
          .Col = ctName
          
        'if on comment
        ElseIf .Col = ctComment Then
          'move to value column
          .Col = ctValue
        End If
      End With
      
    End Select
    'always clear the code
    KeyCode = 0
  End Select

End Sub

Private Sub fgGlobals_KeyPress(KeyAscii As Integer)

  On Error GoTo ErrHandler
  
  'some keys should be ignored...?
  
  Select Case KeyAscii
  Case 9  'tab
    KeyAscii = 0
    
  Case 10, 13 'enter key
    'same as dbl-click
    fgGlobals_DblClick
    KeyAscii = 0
    
  Case Else
    'if in the empty row, AND on name column, begin editing
    If fgGlobals.Row = fgGlobals.Rows - 1 And fgGlobals.Col = 1 Then
      fgGlobals_DblClick
      txtEditName.Text = ChrW$(KeyAscii)
      txtEditName.SelStart = 1
    End If
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub fgGlobals_LostFocus()

  On Error GoTo ErrHandler
  
  'if a tip or textbox is visible, don't show it?
  If picComment.Visible Or txtEditComment.Visible Or txtEditName.Visible Or txtEditValue.Visible Then
    Exit Sub
  End If
  
  'if nothing on grid is selected
  If fgGlobals.Col = 0 Then
    Exit Sub
  End If
  
  picSelFrame(0).Left = fgGlobals.CellLeft + ScreenTWIPSX
  picSelFrame(0).Top = fgGlobals.CellTop + fgGlobals.Top
  picSelFrame(0).Width = fgGlobals.CellWidth
  
  picSelFrame(2).Left = picSelFrame(0).Left
  picSelFrame(2).Top = picSelFrame(0).Top + fgGlobals.CellHeight - picSelFrame(2).Height
  picSelFrame(2).Width = picSelFrame(0).Width
  
  picSelFrame(3).Left = picSelFrame(0).Left
  picSelFrame(3).Top = picSelFrame(0).Top
  picSelFrame(3).Height = fgGlobals.CellHeight
  
  picSelFrame(1).Left = picSelFrame(0).Left + picSelFrame(0).Width - picSelFrame(1).Width
  picSelFrame(1).Top = picSelFrame(3).Top
  picSelFrame(1).Height = picSelFrame(3).Height
  
  picSelFrame(0).Visible = True
  picSelFrame(2).Visible = True
  picSelFrame(3).Visible = True
  picSelFrame(1).Visible = True
Exit Sub

ErrHandler:
  
End Sub

Private Sub fgGlobals_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim TopRow As Long, BtmRow As Long

  On Error GoTo ErrHandler
  
  ' there's a bug that causes the grid to scroll up when
  ' the mouse is moved on the top row with left button pressed
  If fgGlobals.MouseRow = 0 And Button = 1 Then
    'tell the wndproc that mouse is on top row
    NoScroll = True
  End If
  
  If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
    Exit Sub
  End If

  With fgGlobals
    'save row and column for use by dbl-click
    mRow = .MouseRow
    mCol = .MouseCol
    
    mCol = .MouseCol
    mRow = .MouseRow

    'if right click
    If Button = vbRightButton Then
      'determine top/bottom rows
      If .Row < .RowSel Then
        TopRow = .Row
        BtmRow = .RowSel
      Else
        TopRow = .RowSel
        BtmRow = .Row
      End If

      'if row and column are NOT within selected area
      If mRow < TopRow Or mRow > BtmRow Then
        'make new selection
        'check for selection of entire row
        If X < ScreenTWIPSX * 12 Then
          'select entire row
          .Col = 0
          .Row = mRow
          .ColSel = 2
          .SelectionMode = flexSelectionByRow
          .Highlight = flexHighlightAlways
          .MergeCells = flexMergeNever
        Else
          'select freely
          .Col = mCol
          .Row = mRow
          .SelectionMode = flexSelectionFree
          .Highlight = flexHighlightNever
          .MergeCells = flexMergeRestrictAll
        End If
      End If

      'if on an editable row
      If mRow > 0 Then
        'set edit menu parameters
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
        'show popup menu
        PopupMenu frmMDIMain.mnuEdit
      End If
      'done with right click activities
      Exit Sub
    End If
    
    'check for selection of entire row
    If X < ScreenTWIPSX * 12 Then
      'select entire row
      .Col = 0
      .SelectionMode = flexSelectionByRow
      .Highlight = flexHighlightAlways
      .MergeCells = flexMergeNever
      .ColSel = 3
    
    'check for cursor in range of splitting name/val
    ElseIf (X > .ColWidth(ctName) - SPLIT_WIDTH / 2) And (X < .ColWidth(ctName) + SPLIT_WIDTH / 2) Then
      'reset to individual cell selection
      .SelectionMode = flexSelectionFree
      .Highlight = flexHighlightNever
      .MergeCells = flexMergeRestrictAll
      'start splitting name/val
      picSplit.Visible = True
      SplitOffset = .ColWidth(ctName) - X
      SplitCol = 0
      picSplit.Left = X + SplitOffset
      Exit Sub

    'elseif within splitzone 2
    ElseIf (X > .ColWidth(ctName) + .ColWidth(ctValue) - SPLIT_WIDTH / 2) And (X < .ColWidth(ctName) + .ColWidth(ctValue) + SPLIT_WIDTH / 2) Then
      'reset to individual cell selection
      .SelectionMode = flexSelectionFree
      .Highlight = flexHighlightNever
      .MergeCells = flexMergeRestrictAll
      'start splitting val/col
      picSplit.Visible = True
      SplitOffset = .ColWidth(ctName) + .ColWidth(ctValue) - X
      SplitCol = 1
      picSplit.Left = X + SplitOffset
      Exit Sub

    'check for left-click on header
    ElseIf mRow = 0 Then
      Exit Sub
    Else
      'select freely
      If mCol <> -1 Then
        .Col = mCol
      Else
        .Col = ctName
      End If
      .Row = mRow
      .SelectionMode = flexSelectionFree
      .Highlight = flexHighlightNever
      .MergeCells = flexMergeRestrictAll
    End If
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub fgGlobals_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim Pos As Single
  
  On Error GoTo ErrHandler
  
  'reset the tips timer
  'first, always diable it to force it to reset
  tmrTip.Enabled = False
  'then enable it ONLY if form is active, no buttons/keys pressed, and not editing
  tmrTip.Enabled = (frmMDIMain.ActiveMdiChild Is Me And _
                    Button = 0 And Shift = 0 And _
                    Not txtEditName.Visible And _
                    Not txtEditValue.Visible And _
                    Not txtEditComment.Visible)
  
  'always hide tip window
  picComment.Visible = False
  
  'if not active form
  If Not frmMDIMain.ActiveMdiChild Is Me Then
    Exit Sub
  End If

  'if editing,
  If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
    Exit Sub
  End If

  With fgGlobals
    'if splitting
    If picSplit.Visible Then
      Pos = (X + SplitOffset)
      
      'validate it
      If SplitCol = 0 Then
        ' moving name/val split
        
        ' if showing comment column
        If ShowComment Then
          ' limit both name and value to 10% of MINIMUM width
          If Pos < 36 * ScreenTWIPSX Then
            Pos = 36 * ScreenTWIPSX
          ElseIf Pos > .ColWidth(ctName) + .ColWidth(ctValue) - 36 * ScreenTWIPSX Then
            Pos = .ColWidth(ctName) + .ColWidth(ctValue) - 36 * ScreenTWIPSX
          End If
        Else
          ' limit both to 10% on MINUMUM width, but easier,
          ' since there are only two columns to deal with
          If Pos < 36 * ScreenTWIPSX Then
            Pos = 36 * ScreenTWIPSX
          Else
            If ScrollBarVisible() Then
              ' account for scrollbar
              If Pos > .Width - 51 * ScreenTWIPSX Then
                Pos = .Width - 51 * ScreenTWIPSX
              End If
            Else
              'no scrollbar
              If Pos > .Width - 36 * ScreenTWIPSX Then
                Pos = .Width - 36 * ScreenTWIPSX
              End If
            End If
          End If
        End If
      Else
        ' moving val/comment split
        
        ' if moved to far right, means user wants to hide
        ' comment column
        If Pos > .Width - 18 * ScreenTWIPSX Then
          'if scrollbar is visible
          If ScrollBarVisible() Then
            'take into account scrollbar
            Pos = .Width - 17 * ScreenTWIPSX
          Else
            'no scrollbar, snap to width
            Pos = .Width - 4 * ScreenTWIPSX
          End If
        Else
          ' otherwise limit value and comment to 10% of MINIMUM width
          If ScrollBarVisible() Then
            If Pos > .Width - 51 * ScreenTWIPSX Then
              Pos = .Width - 51 * ScreenTWIPSX
            ElseIf Pos < .ColWidth(ctName) + 36 * ScreenTWIPSX Then
              Pos = .ColWidth(ctName) + 36 * ScreenTWIPSX
            End If
          Else
            If Pos > .Width - 36 * ScreenTWIPSX Then
              Pos = .Width - 36 * ScreenTWIPSX
            ElseIf Pos < .ColWidth(ctName) + 36 * ScreenTWIPSX Then
              Pos = .ColWidth(ctName) + 36 * ScreenTWIPSX
            End If
          End If
        End If
      End If
      
      'move splitter
      picSplit.Left = Pos
    
    'elseif within splitzone 1
    ElseIf (X > .ColWidth(ctName) - SPLIT_WIDTH / 2) And (X < .ColWidth(ctName) + SPLIT_WIDTH / 2) Then
      'show split cursor
      .MousePointer = flexCustom
      Set .MouseIcon = picSplit.MouseIcon
  
    'elseif within splitzone 2
    ElseIf (X > .ColWidth(ctName) + .ColWidth(ctValue) - SPLIT_WIDTH / 2) And (X < .ColWidth(ctName) + .ColWidth(ctValue) + SPLIT_WIDTH / 2) Then
      'show split cursor
      .MousePointer = flexCustom
      Set .MouseIcon = picSplit.MouseIcon
  
    'elseif on left edge,
    ElseIf X < 12 * ScreenTWIPSX Then
      'set cursor to row selector
      .MousePointer = flexCustom
      Set .MouseIcon = LoadResPicture("EGC_SELROW", vbResCursor)
    Else
      'set cursor to normal
      .MousePointer = flexDefault
    End If
  End With
  
  'if left button is down, begin dragging
  If Button = vbLeftButton Then
    If Not DroppingGlobal Then
      'if something selected,
      If fgGlobals.Col = 1 And fgGlobals.Row > 1 And fgGlobals.RowSel = fgGlobals.Row Then
        fgGlobals.OLEDrag
      End If
    End If
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub fgGlobals_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim Pos As Single, OldWidth As Single

  On Error GoTo ErrHandler
  
  ' there's a bug that causes the grid to scroll up when
  ' the mouse is moved on the top row with left button pressed
  ' always reset the no-scroll flag
  NoScroll = False
  
  'if splitting
  If picSplit.Visible Then
    'stop splitting
    picSplit.Visible = False
    
    With fgGlobals
      'validate position
      Pos = (X + SplitOffset)
      
      'validate it
      If SplitCol = 0 Then
        ' moving name/val split
        
        ' if showing comment column
        If ShowComment Then
          ' limit both name and value to 10% of MINIMUM width
          If Pos < 36 * ScreenTWIPSX Then
            Pos = 36 * ScreenTWIPSX
          ElseIf Pos > .ColWidth(ctName) + .ColWidth(ctValue) - 36 * ScreenTWIPSX Then
            Pos = .ColWidth(ctName) + .ColWidth(ctValue) - 36 * ScreenTWIPSX
          End If
        Else
          ' limit both to 10% of MINIMUM width, but easier,
          ' since there are only two columns to deal with
          If Pos < 36 * ScreenTWIPSX Then
            Pos = 36 * ScreenTWIPSX
          Else
            If ScrollBarVisible() Then
              ' account for scrollbar
              If Pos > .Width - 51 * ScreenTWIPSX Then
                Pos = .Width - 51 * ScreenTWIPSX
              End If
            Else
              'no scrollbar
              If Pos > .Width - 36 * ScreenTWIPSX Then
                Pos = .Width - 36 * ScreenTWIPSX
              End If
            End If
          End If
        End If
      Else
        ' moving val/comment split
        
        ' if moved to far right, means user wants to hide
        ' comment column
        If Pos > .Width - 18 * ScreenTWIPSX Then
          'hide comment column
          ShowComment = False
          'set Pos to current name width
          Pos = .ColWidth(ctName)
        Else
          ' otherwise limit value and comment to 10% of MINIMUM width
          If Pos < .ColWidth(ctName) + 36 * ScreenTWIPSX Then
            Pos = .ColWidth(ctName) + 36 * ScreenTWIPSX
          Else
            If ScrollBarVisible() Then
              If Pos > .Width - 51 * ScreenTWIPSX Then
                Pos = .Width - 51 * ScreenTWIPSX
              End If
            Else
              If Pos > .Width - 36 * ScreenTWIPSX Then
                Pos = .Width - 36 * ScreenTWIPSX
              End If
            End If
          End If
          'show comment column
          ShowComment = True
        End If
      End If
      
      'if showing comment column
      If ShowComment Then
        If SplitCol = 0 Then
          ' splitting name/val
          OldWidth = .ColWidth(ctName)
          .ColWidth(ctName) = Pos
          
          'check if scrollbar is visible
          If ScrollBarVisible() Then
            'take into account scrollbar
            .ColWidth(ctValue) = OldWidth + .ColWidth(ctValue) - .ColWidth(ctName)
            .ColWidth(ctComment) = .Width - .ColWidth(ctName) - .ColWidth(ctValue) - 17 * ScreenTWIPSX
          Else
            'no scrollbar
            'set width of second column
            .ColWidth(ctValue) = OldWidth + .ColWidth(ctValue) - .ColWidth(ctName) '- 2 * ScreenTWIPSX
            .ColWidth(ctComment) = .Width - .ColWidth(ctName) - .ColWidth(ctValue) - 2 * ScreenTWIPSX
          End If
        Else
          ' splitting val/comment
          .ColWidth(ctValue) = Pos - .ColWidth(ctName)
          
          'set width of third column
          If ScrollBarVisible() Then
            'take into account scrollbar
            .ColWidth(ctComment) = .Width - .ColWidth(ctName) - .ColWidth(ctValue) - 17 * ScreenTWIPSX
          Else
            'no scrollbar
            .ColWidth(ctComment) = .Width - .ColWidth(ctName) - .ColWidth(ctValue) - 2 * ScreenTWIPSX
          End If
        End If
      Else
        ' hide comment by setting zero width
        .ColWidth(ctComment) = 0
        
        If Pos < 36 * ScreenTWIPSX Then
          Pos = 36 * ScreenTWIPSX
        Else
          If ScrollBarVisible() Then
            ' account for scrollbar
            If Pos > .Width - 51 * ScreenTWIPSX Then
              Pos = .Width - 51 * ScreenTWIPSX
            End If
          Else
            If Pos > .Width - 36 * ScreenTWIPSX Then
              Pos = .Width - 36 * ScreenTWIPSX
            End If
          End If
        End If
        
        'reset name column width
        .ColWidth(ctName) = Pos
              
        'adjust value width
        If ScrollBarVisible() Then
          'take into account scrollbar
          .ColWidth(ctValue) = .Width - .ColWidth(ctName) - 17 * ScreenTWIPSX
        Else
          'no scrollbar
          'set width of second column
          .ColWidth(ctValue) = .Width - .ColWidth(ctName) - 2 * ScreenTWIPSX
        End If
      End If
    End With
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub fgGlobals_OLECompleteDrag(Effect As Long)

  'reset dragging flag
  DroppingGlobal = False
End Sub

Private Sub fgGlobals_OLEStartDrag(Data As MSFlexGridLib.DataObject, AllowedEffects As Long)

  On Error GoTo ErrHandler
  
  '??? for some reason, in the middle of a mouse down/mouse up
  'operation, the drag event fires; this effs up the splitting operation
  
  If picSplit.Visible Then
    'cancel drag/drop, so split action works as desired
    AllowedEffects = vbDropEffectNone
    Exit Sub
  End If
  
  'set global drop flag (so logics (or other text receivers) know
  'when an object is being dropped
  DroppingGlobal = True
  
  'set allowed effects to copy only
  AllowedEffects = vbDropEffectCopy
  Data.SetData fgGlobals.TextMatrix(fgGlobals.Row, 1), vbCFText
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub fgGlobals_Scroll()

  On Error GoTo ErrHandler
  
  'move txtbox, if visible
  If txtEditName.Visible Then
    txtEditName.Move fgGlobals.CellLeft + CellIndentH, fgGlobals.CellTop + fgGlobals.Top + CellIndentV, fgGlobals.CellWidth - CellIndentH, fgGlobals.CellHeight - CellIndentV
  End If
  If txtEditValue.Visible Then
    txtEditValue.Move fgGlobals.CellLeft + CellIndentH, fgGlobals.CellTop + fgGlobals.Top + CellIndentV, fgGlobals.CellWidth - CellIndentH, fgGlobals.CellHeight - CellIndentV
  End If
  If txtEditComment.Visible Then
    txtEditComment.Move fgGlobals.CellLeft + CellIndentH, fgGlobals.CellTop + fgGlobals.Top + CellIndentV, fgGlobals.CellWidth - CellIndentH, fgGlobals.CellHeight - CellIndentV
  End If
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

  On Error Resume Next

  'if hiding prevwin on lost focus, hide it now
  If Settings.HidePreview Then
    PreviewWin.Hide
  End If
 
  'if findform is visible,
  If FindForm.Visible Then
    'hide it it
    FindForm.Visible = False
  End If

  'adjust menus and statusbar
 '*'Debug.Print "AdjustMenus 51"
  AdjustMenus rtGlobals, GameLoaded, True, IsChanged

  'set edit menu
  SetEditMenu

  If txtEditName.Visible Then
    txtEditName.SetFocus
  End If
  If txtEditValue.Visible Then
    txtEditValue.SetFocus
  End If
  If txtEditComment.Visible Then
    txtEditComment.SetFocus
  End If
End Sub

Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

  'detect and respond to keyboard shortcuts
  
  'always check for help key
  If KeyCode = vbKeyF1 And Shift = 0 Then
    MenuClickHelp
    KeyCode = 0
    Exit Sub
  End If

  'if editing something
  If txtEditName.Visible Or txtEditValue.Visible Or txtEditComment.Visible Then
    Exit Sub
  End If

  'check for global shortcut keys
  CheckShortcuts KeyCode, Shift
  If KeyCode = 0 Then
    Exit Sub
  End If

  Select Case Shift
  Case 0
    Select Case KeyCode
    Case vbKeyDelete
      If frmMDIMain.mnuEDelete.Enabled Then
        MenuClickDelete
        KeyCode = 0
      End If
    End Select

  Case vbCtrlMask
    Select Case KeyCode
    Case vbKeyA
      If frmMDIMain.mnuESelectAll.Enabled Then
        MenuClickSelectAll
        KeyCode = 0
      End If

    Case vbKeyC
      If frmMDIMain.mnuECopy.Enabled Then
        MenuClickCopy
        KeyCode = 0
      End If

    Case vbKeyX
      If frmMDIMain.mnuECut.Enabled Then
        MenuClickCut
        KeyCode = 0
      End If

    Case vbKeyV
      If frmMDIMain.mnuEPaste.Enabled Then
        MenuClickPaste
        KeyCode = 0
      End If
      
    Case vbKeyZ
      If frmMDIMain.mnuEUndo.Enabled Then
        MenuClickUndo
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
      If frmMDIMain.mnuEInsert.Enabled Then
        MenuClickInsert
        KeyCode = 0
      End If
    End Select

  Case 3 'vbShiftMask + vbCtrlMask
    Select Case KeyCode
    Case vbKeyF
      If frmMDIMain.mnuECustom1.Enabled Then
        MenuClickECustom1
        KeyCode = 0
      End If
    End Select

  Case vbAltMask
    Select Case KeyCode
    Case vbKeyF
      If frmMDIMain.mnuRCustom1.Enabled Then
        MenuClickCustom1
        KeyCode = 0
      End If
    End Select
  End Select
End Sub

Private Sub Form_Load()


  On Error GoTo ErrHandler
  
  ' set default width/height used for resizing calculations
  CalcWidth = MIN_WIDTH
  CalcHeight = MIN_HEIGHT
  
  'set undo collection
  Set UndoCol = New Collection
  
  'hide column 0 (it's used to hold the starting name of a define
  '               so we can tell if it changed when the list is saved)
  fgGlobals.ColWidth(0) = 0
  
  'set font
  InitFonts

  'if previously opened, restore the same position
  ' and column settings
  If GWWidth > 0 Then
    With Me
      .WindowState = GWState
      .Left = GWLeft
      .Top = GWTop
      .Width = GWWidth
      .Height = GWHeight
    End With
    ShowComment = GWShowComment
    'shouldn't have to validate; nothing affects these values
    ' and they are only valid during any given app instance
    
    ' column settings saved as ratios
    If ShowComment Then
      fgGlobals.ColWidth(ctName) = GWNameFrac * fgGlobals.Width
      fgGlobals.ColWidth(ctValue) = GWValFrac * fgGlobals.Width
      'make sure the columns fit
      If fgGlobals.ColWidth(ctName) < 36 * ScreenTWIPSX Then
        '*'Debug.Assert False
        fgGlobals.ColWidth(ctName) = 36 * ScreenTWIPSX
      ElseIf fgGlobals.Width - fgGlobals.ColWidth(ctName) < 72 * ScreenTWIPSX Then
        '*'Debug.Assert False
        fgGlobals.ColWidth(ctName) = fgGlobals.Width - 72 * ScreenTWIPSX
        fgGlobals.ColWidth(ctValue) = 36 * ScreenTWIPSX
      End If
      If fgGlobals.ColWidth(ctValue) < 36 * ScreenTWIPSX Then
        '*'Debug.Assert False
        fgGlobals.ColWidth(ctValue) = 36 * ScreenTWIPSX
        'name is already verified OK
      ElseIf fgGlobals.Width - fgGlobals.ColWidth(ctName) - fgGlobals.ColWidth(ctValue) < 36 * ScreenTWIPSX Then
        '*'Debug.Assert False
        fgGlobals.ColWidth(ctValue) = fgGlobals.Width - fgGlobals.ColWidth(ctName) - 3 * ScreenTWIPSX
      End If
    Else
      'just two columns
      If GWNameFrac * fgGlobals.Width < 36 * ScreenTWIPSX Then
        '*'Debug.Assert False
        fgGlobals.ColWidth(ctName) = 36 * ScreenTWIPSX
      ElseIf fgGlobals.Width - GWNameFrac * fgGlobals.Width < 36 * ScreenTWIPSX Then
        '*'Debug.Assert False
        fgGlobals.ColWidth(ctName) = fgGlobals.Width - 36 * ScreenTWIPSX
      Else
        fgGlobals.ColWidth(ctName) = GWNameFrac * fgGlobals.Width
      End If
    End If
    
  Else
    'get default from settings
    ShowComment = Settings.GEShowComment
    If ShowComment Then
      'if a fractional value is in settings
      If Settings.GENameFrac > 0.1 And Settings.GEValFrac > 0.1 And _
         Settings.GENameFrac + Settings.GEValFrac < 0.8 Then
        fgGlobals.ColWidth(ctName) = fgGlobals.Width * Settings.GENameFrac
        fgGlobals.ColWidth(ctValue) = fgGlobals.Width * Settings.GEValFrac
      Else
        ' default to thirds
        fgGlobals.ColWidth(ctName) = fgGlobals.Width / 3
        fgGlobals.ColWidth(ctValue) = fgGlobals.Width / 3
      End If
    Else
      'if a fractional value is in settings
      If Settings.GENameFrac > 0.1 And Settings.GENameFrac <= 0.9 Then
        fgGlobals.ColWidth(ctName) = fgGlobals.Width * Settings.GENameFrac
      Else
        'default to 1/2
        fgGlobals.ColWidth(ctName) = fgGlobals.Width / 2
      End If
    End If
  End If
  
  
  'set indent Value
  CellIndentH = ScreenTWIPSX * 4
  CellIndentV = ScreenTWIPSY * 1

  'set all columns to leftcenter alignment
  fgGlobals.ColAlignment(1) = flexAlignLeftCenter
  fgGlobals.ColAlignment(2) = flexAlignLeftCenter
  fgGlobals.ColAlignment(3) = flexAlignLeftCenter
  
#If DEBUGMODE <> 1 Then
  'subclass the flexgrid
  PrevFGWndProc = SetWindowLong(Me.fgGlobals.hWnd, GWL_WNDPROC, AddressOf ScrollWndProc)
#End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
*/
        }
        #endregion

        internal void InitFonts() {
            lstGlobals.Font = new Font(WinAGISettings.PreviewFontName.Value, WinAGISettings.PreviewFontSize.Value);
        }

        private void frmGlobals_FormClosing(object sender, FormClosingEventArgs e) {
            //GEInUse = false;
        }
    }
}
