using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Common.Base;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Editor.Base;

namespace WinAGI.Editor {
    public partial class frmViewEdit : Form {
        public int ViewNumber;
        readonly double zoom = 4;
        public Engine.View EditView;
        private int CurLoop = 0;
        private int CurCel = 0;
        internal bool InGame;
        internal bool IsChanged;
        private bool closing = false;

        public frmViewEdit() {
            InitializeComponent();
            MdiParent = MDIMain;
        }

        #region Form Event Handlers

        private void frmViewEdit_Load(object sender, EventArgs e) {

        }

        private void frmViewEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmViewEdit_FormClosed(object sender, FormClosedEventArgs e) {
            // dereference view
            EditView?.Unload();
            EditView = null;

            // remove from vieweditor collection
            foreach (frmViewEdit frm in ViewEditors) {
                if (frm == this) {
                    ViewEditors.Remove(frm);
                    break;
                }
            }

            //// destroy undocol
            //if (UndoCol.Count > 0) {
            //    for (int i = UndoCol.Count - 1; i >= 0; i--) {
            //        UndoCol.Remove(i);
            //    }
            //}
            //UndoCol = null;

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
                mnuRInGame.Enabled = false;
                mnuRInGame.Text = "Add View to Game";
                mnuRRenumber.Enabled = false;
                // mnuRProperties no change
                mnuRExportLoopGIF.Enabled = true; // = loop or cel selected
            }
            else {
                // if a game is loaded, base import is also always available
                MDIMain.mnuRImport.Enabled = true;
                mnuRExport.Text = InGame ? "Export View" : "Save As ...";
                mnuRInGame.Enabled = true;
                mnuRInGame.Text = InGame ? "Remove from Game" : "Add to Game";
                mnuRRenumber.Enabled = InGame;
                // mnuRProperties no change
                mnuRExportLoopGIF.Enabled = true; // = loop or cel selected
            }
        }

        public void mnuRSave_Click(object sender, EventArgs e) {
            SaveView();
        }

        public void mnuRExport_Click(object sender, EventArgs e) {
            ExportView();
        }

        public void mnuRInGame_Click(object sender, EventArgs e) {
            ToggleInGame();
        }

        private void mnuRRenumber_Click(object sender, EventArgs e) {
            RenumberView();
        }

        private void mnuRProperties_Click(object sender, EventArgs e) {
            EditViewProperties(1);
        }

        private void mnuRExportLoopGIF_Click(object sender, EventArgs e) {
            ExportLoop(EditView, CurLoop);
        }
        #endregion

        #region temp code

        private void button2_Click(object sender, EventArgs e) {
            EditView.Clear();
            EditView.Loops.Add(1).Cels.Add(0, 5, 5, AGIColorIndex.Brown);
            MarkAsChanged();
            cmbLoop.Items.Clear();
            foreach (Loop tmpLoop in EditView.Loops) {
                cmbLoop.Items.Add($"Loop {tmpLoop.Index}");
            }
            cmbLoop.SelectedIndex = 0;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            // redraw 
            // set transparency
            EditView[CurLoop][CurCel].Transparency = chkTrans.Checked;
            ShowAGIBitmap(picCel, EditView[CurLoop][CurCel].CelBMP, zoom);
        }

        private void cmbLoop_SelectedIndexChanged(object sender, EventArgs e) {
            // loop changing; select a new cel
            CurLoop = cmbLoop.SelectedIndex;
            // clear cel list
            cmbCel.Items.Clear();
            // add all cels
            foreach (Cel tmpCel in EditView[CurLoop].Cels) {
                cmbCel.Items.Add($"Cel {tmpCel.Index}");
            }
            // select first cel
            cmbCel.SelectedIndex = 0;
        }

        private void cmbCel_SelectedIndexChanged(object sender, EventArgs e) {
            CurCel = cmbCel.SelectedIndex;
            // clear the picture box
            //using Graphics = picCel.Image.    Graphics.FromImage(pic.Image);
            picCel.CreateGraphics().Clear(BackColor);
            picCel.Refresh();
            // set transparency
            EditView[CurLoop][CurCel].Transparency = chkTrans.Checked;
            // show the cel
            ShowAGIBitmap(picCel, EditView[CurLoop][CurCel].CelBMP, zoom);
        }

        private void timer1_Tick(object sender, EventArgs e) {
            // cycle the cel
            CurCel++;
            if (CurCel >= EditView[CurLoop].Cels.Count) {
                CurCel = 0;
                // show the cel
            }
            // set transparency
            EditView[CurLoop][CurCel].Transparency = chkTrans.Checked;
            ShowAGIBitmap(picCel, EditView[CurLoop][CurCel].CelBMP, zoom);
        }

        private void button1_Click(object sender, EventArgs e) {
            //toggle cycle state
            if (timer1.Enabled) {
                //stop cycling; change button name to allow starting
                button1.Text = "Start";
            }
            else {
                button1.Text = "Stop";
            }
            timer1.Enabled = !timer1.Enabled;
        }

        void tmpviewform() {
            /*
      Option Explicit

        ' use picPCel to hold the cel data without gridlines;
        ' when pixel ops are needed, blit between picCel and picPCel as
        ' needed

        'selection and clipboard pics will all be set to scale 1

        Public ViewEdit As AGIView
        Public PrevVEWndProc As Long, PrevCBWndProc As Long

        'view preview
        Private ShowVEPrev As Boolean, ShowGrid As Boolean
        Private blnNoUpdate As Boolean
        Private PrevCel As Long
        Private ViewScale As Long, VTopMargin As Long
        Private blnDraggingView As Boolean
        Private lngVAlign As Long, lngHAlign As Long
        Private lngMotion As Long, blnTrans As Boolean
        Private DontDraw As Boolean

        'use local variables to hold visible status for scrollbars
        'because their visible property remains false as long as
        'the picturebox that holds them is false, even though they
        'are set to true
        Private blnViewHSB As Boolean, blnViewVSB As Boolean

        Public ViewNumber As Long
        Public InGame As Boolean
        Public IsChanged As Boolean

        'variables for selected components
        Private SelectedLoop As Long
        Private SelectedCel As Long
        Private SelectedProp As Long
        Private SelectedTool As ViewEditToolType
        Private ViewMode As ViewEditMode
        Private CurrentOperation As ViewEditOperation
        Private SelectionNotMoved As Boolean
        Private DelOriginalData As Boolean

        Private CelAnchorX As Long, CelAnchorY As Long  'in cel coordinates
        Private OldX As Long, OldY As Long
        Private CelXOffset As Long, CelYOffset As Long
        Private OldPosX As Long, OldPosY As Long
        Private ClipboardData() As AGIColors
        Private DataUnderSel() As AGIColors, UndoDataUnderSel() As AGIColors
        Private SelHeight As Long, SelWidth As Long
        Private OldNode As Long

        Private ScaleFactor As Long
        Private blnDragging As Long
        Private sngOffsetX As Single, sngOffsetY As Single
        Private LeftColor As AGIColors, RightColor As AGIColors
        Private DrawCol As AGIColors
        Private EditPropDropdown As Boolean
        Private PropGotFocus As Long
        Private MouseY As Single
        Private TreeRightButton As Boolean
        Private TreeX As Single, TreeY As Single

        'need this flag because mousemove is called when
        'a picture box is clicked when the form does not
        'currently have the focus
        Private FormActivated As Boolean

        Private CalcWidth As Long, CalcHeight As Long
        Private Const MIN_HEIGHT = 320 '300
        Private Const MIN_WIDTH = 360

        Private SplitVOffset As Single
        Private Const MIN_SPLIT_V = 225 'in pixels
        Private Const MAX_SPLIT_V = 445 'in pixels
        Private Const SPLIT_WIDTH = 4  'in pixels

        Private UndoCol As Collection
        Private PixelData() As AGIColors
        Private PixelCount As Long

        Private Enum ViewEditToolType
          ttEdit
          ttSelect
          ttDraw
          ttLine
          ttRectangle
          ttBoxFill
          ttPaint
          ttErase
        End Enum

        Private Enum ViewEditOperation
          opNone
          opSetSelection
          opMoveSelection
          opDraw
          opChangeWidth
          opChangeHeight
          opChangeBoth
        End Enum

        Private Const LT_GRAY = &HC0C0C0
        Private Const DK_GRAY = &HD8E9EC
      '''  Private Const SEL_BLUE = &HC56A31
        Private Const GRID_COLOR = &HB4B4B4
        Private Const VE_MARGIN As Long = 10
        Private Const MAX_CEL_W = 160
        Private Const MAX_CEL_H = 168
        Private Const PROP_ROW_HEIGHT = 17
        Private Const MAXLOOPS = 254 '16
        Private Const MAXCELS = 254 '32
      Private Sub DrawBoxOnCel(ByVal CelEndX As Long, ByVal CelEndY As Long, ByVal BoxFill As Boolean, NextUndo As ViewUndo, Optional ByVal DontUndo As Boolean = False)

        'draws the box on the cel data

        Dim i As Long, j As Long
        Dim CelStartX As Long, CelStartY As Long

        'convert line to top-bottom/left-right format
        If CelEndY < CelAnchorY Then
          'line is bottom to top;
          'set start to top Value
          CelStartY = CelEndY
          'set end to bottom Value
          CelEndY = CelAnchorY
        Else
          'end is already bottom Value; set start to top Value
          CelStartY = CelAnchorY
        End If
        'same for x direction
        If CelEndX < CelAnchorX Then
          CelStartX = CelEndX
          CelEndX = CelAnchorX
        Else
          CelStartX = CelAnchorX
        End If

        'if NOT skipping undo
        If Not DontUndo Then
          With NextUndo
            If Not BoxFill Then
              .ResizeData (CelEndX - CelStartX + 1) * 2 + (CelEndY - CelStartY + 1) * 2 + 3
              .UndoData(0) = CelStartX
              .UndoData(1) = CelStartY
              .UndoData(2) = CelEndX
              .UndoData(3) = CelEndY
              For i = CelStartX To CelEndX
                  .UndoData(i - CelStartX + 4) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelStartY)
                  .UndoData(i - 2 * CelStartX + 5 + CelEndX) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelEndY)
              Next i
              For j = CelStartY To CelEndY
                  .UndoData(2 * (CelEndX - CelStartX) + 6 + j - CelStartY) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, j)
                  .UndoData(2 * (CelEndX - CelStartX) + (CelEndY - CelStartY) + 7 + j - CelStartY) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelEndX, j)
              Next j
            Else
              'save current data in undo data
              .ResizeData (CelEndX - CelStartX + 1) * (CelEndY - CelStartY + 1) + 3
              .UndoData(0) = CelStartX
              .UndoData(1) = CelStartY
              .UndoData(2) = CelEndX
              .UndoData(3) = CelEndY
              For i = CelStartX To CelEndX
                For j = CelStartY To CelEndY
                  .UndoData(4 + (i - CelStartX) + (j - CelStartY) * (CelEndX - CelStartX + 1)) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j)
                Next j
              Next i
            End If
          End With
        End If

        'if filling
        If BoxFill Then
          picPCel.Line (CelStartX, CelStartY)-(CelEndX, CelEndY), EGAColor(CInt(DrawCol)), BF
        Else
          picPCel.Line (CelStartX, CelStartY)-(CelEndX, CelEndY), EGAColor(CInt(DrawCol)), B
        End If

        'rebuild cel databased on current status of pixel cel
        BuildCelData CelStartX, CelStartY, CelEndX, CelEndY

        'stretch the pixel cel into the draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY
        picCel.Refresh
        If ShowGrid And ScaleFactor > 3 Then
          DrawCelGrid
        End If
      End Sub

      Private Sub DrawCelGrid()

        Dim prevCol As Long, i As Long
        Dim sngH As Single, sngW As Long

        'draw grid over the cel
        With picCel
          sngH = .Height
          sngW = .Width
          prevCol = .ForeColor

          .ForeColor = GRID_COLOR
          i = ScaleFactor * 2
          Do
            picCel.Line (i, 0)-Step(0, sngH)
            i = i + ScaleFactor * 2
          Loop Until i > sngW
          i = ScaleFactor
          Do
            picCel.Line (0, i)-Step(sngW, 0)
            i = i + ScaleFactor
          Loop Until i > sngH
          .ForeColor = prevCol
        End With
      End Sub

      Public Sub MouseWheel(ByVal MouseKeys As Long, ByVal Rotation As Long, ByVal xPos As Long, ByVal yPos As Long)

        On Error GoTo ErrHandler

        Dim lngMode As Long

        If Not frmMDIMain.ActiveMdiChild Is Me Then
          Exit Sub
        End If

        ' zoom not allowed if mousekeys present
        If MouseKeys <> 0 Then
          Exit Sub
        End If

        ' if inside the surface area
        With picSurface
          If (xPos > .Left) And (xPos < .Left + .Width) And (yPos > .Top) And (yPos < .Top + .Height) Then
            ' mouse is over drawing surface
            ' only allow zoom if cel is currently being drawn
            If ViewMode = vmCel Then
              lngMode = 1
            End If
          End If
        End With

        'if inside the preview surface area
        With picPrevFrame
          If xPos > .Left And xPos < .Left + .Width And yPos > .Top And yPos < .Top + .Height - fraVMotion.Height Then
            ' mouse is over preview surface
            'only allow if preview is visible
            If picPrevCel.Visible Then
              lngMode = 2
            End If
          End If
        End With

        ' if not over either surface
        If lngMode = 0 Then
          'nothing to scroll
          Exit Sub
        End If

        Select Case lngMode
        Case 1 ' drawing surface
          If Sgn(Rotation) = 1 Then
            'zoom in
            ZoomCel 1
          ElseIf Sgn(Rotation) = -1 Then
            'zoom out
            ZoomCel 0
          End If

        Case 2 ' preview surface
          If Sgn(Rotation) = 1 Then
            'zoom in
            ZoomPrev 1
          ElseIf Sgn(Rotation) = -1 Then
            'zoom out
            ZoomPrev 0
          End If
        End Select

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub CheckNavigationKeys(ByRef KeyCode As Integer, ByRef Shift As Integer)

      On Error GoTo ErrHandler

        'regular arrow keys move +/- 1
        'shift+arrow keys move to top/bottom
        'key up/down changes loop
        'key left/right changes cel

        'Shift+T toggles transparency

        Select Case Shift
        Case 0 'no shift, ctrl, alt
          Select Case KeyCode
          Case vbKeyUp
            'loop up
            If tvwView.SelectedItem.Parent Is Nothing Then
              'root is selected; do nothing
            ElseIf tvwView.SelectedItem.Parent.Parent Is Nothing Then
              'loop is selected
              If SelectedLoop = 0 Then
                'move to view
                Set tvwView.SelectedItem = tvwView.Nodes(1)
                tvwView_NodeClick tvwView.SelectedItem
              ElseIf SelectedLoop > 0 Then
                'move up if not already at top
                Set tvwView.SelectedItem = tvwView.SelectedItem.Previous
                tvwView_NodeClick tvwView.SelectedItem
              End If
            Else
              'must be a cel;
              If SelectedLoop > 0 Then
                'move up if not already at top
                Set tvwView.SelectedItem = tvwView.SelectedItem.Parent.Previous
                tvwView_NodeClick tvwView.SelectedItem
              Else
                'select the loop, not the cel
                Set tvwView.SelectedItem = tvwView.SelectedItem.Parent
                tvwView_NodeClick tvwView.SelectedItem
              End If
            End If
            KeyCode = 0

          Case vbKeyDown
            'loop down
            If tvwView.SelectedItem.Parent Is Nothing Then
              'root is selected; select first loop
              Set tvwView.SelectedItem = tvwView.Nodes(2)
              tvwView_NodeClick tvwView.SelectedItem
            ElseIf tvwView.SelectedItem.Parent.Parent Is Nothing Then
              'loop is selected
              If tvwView.SelectedItem.Text <> "End" Then
                'move down if not already at bottom
                Set tvwView.SelectedItem = tvwView.SelectedItem.Next
                tvwView_NodeClick tvwView.SelectedItem
              End If
            Else
              'must be a cel; (next loop is always valid if a cel is selected)
              Set tvwView.SelectedItem = tvwView.SelectedItem.Parent.Next
              tvwView_NodeClick tvwView.SelectedItem
            End If
            KeyCode = 0

          Case vbKeyRight
            'cel down
            If tvwView.SelectedItem.Parent Is Nothing Then
              'root is selected; do nothing
            ElseIf tvwView.SelectedItem.Parent.Parent Is Nothing Then
              'loop is selected
              If tvwView.SelectedItem.Text <> "End" Then
                'it's a valid loop; select the first cel
                Set tvwView.SelectedItem = tvwView.SelectedItem.Child
                tvwView_NodeClick tvwView.SelectedItem
              End If
            Else
              'must be a cel;
              If tvwView.SelectedItem.Text <> "End" Then
                'move down if not already at bottom
                Set tvwView.SelectedItem = tvwView.SelectedItem.Next
                tvwView_NodeClick tvwView.SelectedItem
              End If
            End If
            KeyCode = 0

         Case vbKeyLeft
            'cel up
            If tvwView.SelectedItem.Parent Is Nothing Then
              'root is selected; do nothing
            ElseIf tvwView.SelectedItem.Parent.Parent Is Nothing Then
              'loop is selected; select first cel in this loop
              If tvwView.SelectedItem.Text <> "End" Then
                'move down if not already at bottom
                Set tvwView.SelectedItem = tvwView.SelectedItem.Child
                tvwView_NodeClick tvwView.SelectedItem
              End If
            Else
              If SelectedCel > 0 Then
                'move up if not already at top
                Set tvwView.SelectedItem = tvwView.SelectedItem.Previous
                tvwView_NodeClick tvwView.SelectedItem
              End If
            End If
            KeyCode = 0
          End Select

        Case vbShiftMask
          Select Case KeyCode
          Case vbKeyUp
            'select first loop, if not already selected
            If SelectedLoop <> 0 Or SelectedCel >= 0 Then
              Set tvwView.SelectedItem = tvwView.Nodes(2)
              tvwView_NodeClick tvwView.SelectedItem
            End If
            KeyCode = 0
            Shift = 0

          Case vbKeyDown
            'select last loop, if not already selected
            If SelectedLoop <> ViewEdit.Loops.Count Or SelectedCel >= 0 Then
              Set tvwView.SelectedItem = tvwView.Nodes(tvwView.Nodes.Count)
              tvwView_NodeClick tvwView.SelectedItem
            End If
            KeyCode = 0
            Shift = 0

          Case vbKeyLeft
            'move to first cel
            If SelectedLoop >= 0 And SelectedLoop <= ViewEdit.Loops.Count Then
              'if a loop is selected
              If SelectedCel = -1 Then
                Set tvwView.SelectedItem = tvwView.SelectedItem.Child
                tvwView_NodeClick tvwView.SelectedItem
              Else
                'cel selected; move to first sibling if not already there
                If SelectedCel > 0 Then
                  Set tvwView.SelectedItem = tvwView.SelectedItem.FirstSibling
                  tvwView_NodeClick tvwView.SelectedItem
                End If
              End If
            End If
            KeyCode = 0
            Shift = 0
          Case vbKeyRight
            'move to last cel
            If SelectedLoop >= 0 And SelectedLoop <= ViewEdit.Loops.Count Then
              'if a loop is selected
              If SelectedCel = -1 Then
                Set tvwView.SelectedItem = tvwView.SelectedItem.Child.LastSibling
                tvwView_NodeClick tvwView.SelectedItem
              Else
                'cel selected; move to last sibling if not already there
                If SelectedCel < ViewEdit.Loops(SelectedLoop).Cels.Count Then
                  Set tvwView.SelectedItem = tvwView.SelectedItem.LastSibling
                  tvwView_NodeClick tvwView.SelectedItem
                End If
              End If
            End If
            KeyCode = 0
            Shift = 0

          Case vbKeyT
            If cmdVPlay.Enabled Then
              'toggle transparency
              cmdToggleTrans_Click
              KeyCode = 0
              Shift = 0
            End If
          End Select
        End Select
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub MenuClickECustom1()

        'toggle the preview panel
        ShowVEPrev = Not ShowVEPrev

        'turn timer on, if showing
      '  tmrMotion.Enabled = ShowVEPrev

        If ShowVEPrev Then
          frmMDIMain.mnuECustom1.Caption = "Hide Preview"
        Else
          frmMDIMain.mnuECustom1.Caption = "Show Preview"
        End If

        'force redraw to update the preview panel
        Form_Resize
        If ShowVEPrev Then
          ' and re-display to size things correctly
          DisplayPrevLoop
          DisplayPrevCel
        End If
      End Sub

      Public Sub MenuClickECustom2()

        'toggle the drawing grid
        ShowGrid = Not ShowGrid

        If ShowGrid Then
          frmMDIMain.mnuECustom2.Caption = "Hide Drawing Grid"
        Else
          frmMDIMain.mnuECustom2.Caption = "Show Drawing Grid"
        End If

        ' redraw surface to update
        picSurface_Paint

        'draw grid if now showing it
        If ShowGrid Then
          If ScaleFactor > 3 Then
            'draw grid over the cel
            DrawCelGrid
          End If
        Else
          'if a cel is currently being displayed
          If ViewMode = vmCel Then
            're-stretch the cel image to get rid of grid
            StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY
            picCel.Refresh
          End If
        End If

      End Sub


      Public Sub MenuClickHelp()

        On Error GoTo ErrHandler

        'help
        HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\View_Editor.htm"
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Public Sub Activate()
        'bridge method to call the form's Activate event method
        Form_Activate
      End Sub

      Private Sub AddUndo(NextUndo As ViewUndo)

        If Not IsChanged Then
          MarkAsChanged
        End If

        'remove old undo items until there is no room for this one
        'to be added
        If Settings.ViewUndo > 0 Then
          Do While UndoCol.Count >= Settings.ViewUndo
            UndoCol.Remove 1
          Loop
        End If

        'adds the next undo object
        UndoCol.Add NextUndo

        'set undo menu
        frmMDIMain.mnuEUndo.Enabled = True
        tlbView.Buttons("undo").Enabled = True
        frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(VIEWUNDOTEXT + NextUndo.UDAction) & vbTab & "Ctrl+Z"
      End Sub

      Public Sub MenuClickClear()

        Dim i As Long
        Dim j As Long
        Dim rtn As VbMsgBoxResult

        Select Case ViewMode
        Case vmView
          'verify
          If MsgBox("This will reset the view, deleting all current loops and cels. This action cannot be undone. Do you want to continue?", vbQuestion + vbYesNo, "Clear View") = vbNo Then
            Exit Sub
          End If

          'clear undo buffer
          If UndoCol.Count > 0 Then
            For i = UndoCol.Count To 1 Step -1
              UndoCol.Remove i
            Next i
            SetEditMenu
          End If
          'resets view to a single loop with a single cel
          'with height and width of one with black transcolor
          For i = ViewEdit.Loops.Count - 1 To 1 Step -1
            'delete loop
            ViewEdit.Loops.Remove i
          Next i
          For i = ViewEdit.Loops(0).Cels.Count - 1 To 1 Step -1
            'delete cel
            ViewEdit.Loops(0).Cels.Remove i
          Next i
          'reset cel properties
          ViewEdit.Loops(0).Cels(0).Height = Settings.DefCelH
          ViewEdit.Loops(0).Cels(0).Width = Settings.DefCelW
          ViewEdit.Loops(0).Cels(0).TransColor = agBlack
          ViewEdit.Loops(0).Cels(0).CelData(0, 0) = agBlack
          'set current selection
          SelectedLoop = 0
          SelectedCel = 0
          'update tree
          UpdateTree
          tvwView.Nodes("Cell0:0").EnsureVisible
          'redisplay cel
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case vmLoop
          'delete all cels from the loop
          ClearLoop

        Case vmCel
          'clear the cel
          ClearCel
        End Select

      End Sub

      Public Sub MenuClickCopy()

        Dim frmNew As frmViewEdit

        'set paste format
        ViewCBMode = ViewMode

        'take action
        Select Case ViewMode
        Case vmView
          'always clear the clipboard mode, as 'copying' a view
          'just makes a duplicate of the the entire view, and no
          'data is stored on the clipboard
          ViewCBMode = vmBitmap
          Clipboard.Clear

          'copy view to a new blank view
          Set frmNew = New frmViewEdit
          ViewEditors.Add frmNew
          frmNew.EditView ViewEdit

          'reset name
          frmNew.ViewEdit.ID = "Copy of " & frmNew.ViewEdit.ID
          'and caption
          frmNew.Caption = SVIEWED & frmNew.ViewEdit.ID
          'force changed status by tweaking first pixel
          '(yes, this is a hack, but it works for now)
          frmNew.ViewEdit.Loops(0).Cels(0).CelData(0, 0) = frmNew.ViewEdit.Loops(0).Cels(0).CelData(0, 0)
          'update tree
          frmNew.UpdateTree
          'select first cel
          frmNew.tvwView.Nodes("Cell0:0").EnsureVisible
          frmNew.DisplayCel True
          'switch to new form
          frmNew.SetFocus
          Exit Sub

        Case vmLoop
          'set clipboard loop to this loop
          Set ClipViewLoop = New AGILoop
          ClipViewLoop.CopyLoop ViewEdit.Loops(SelectedLoop)

          'clear clipboard cel
          Set ClipViewCel = Nothing

        Case vmCel
          'if there is a selection
          If shpView.Visible Then
            'copy selection
            CopySelection

            'clear clipboard loop and cel
            Set ClipViewLoop = Nothing
            Set ClipViewCel = Nothing
          Else
            'set clipboard cel to this cel
            Set ClipViewCel = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel)

            'clear clipboard loop
            Set ClipViewLoop = Nothing
          End If
        End Select
      End Sub
      Public Sub MenuClickCut()

        'set paste format
        ViewCBMode = ViewMode

        'take action
        Select Case ViewMode
        'Case vmView
          'can't cut view, so don't need to check for it
        Case vmLoop
          'cut loop
          DeleteLoop True

        Case vmCel
          'if a selection is visible
          If shpView.Visible Then
            'cut selection
            CutSelection
          Else
            'delete cel
            DeleteCel True
          End If
        End Select
      End Sub

      Public Sub MenuClickDelete()

        Dim rtn As VbMsgBoxResult

        Select Case ViewMode
        Case vmView
          'not applicable for view mode

        Case vmLoop
          'delete loop
          DeleteLoop False

        Case vmCel
          'if a selection is visible
          If shpView.Visible Then
            'delete selection
            DeleteSelection
          Else
            'delete cel
            DeleteCel False
          End If
        End Select

      End Sub


      Private Sub EndMoveSelection(ByVal CelX As Long, ByVal CelY As Long)
        ' this method does three things:
        ' - reenables selection shape flashing
        ' - copies data under selection to holding array
        ' - rebuilds view data to match image

        Dim CelWidth As Long, CelHeight As Long
        Dim CelEndX As Long, CelEndY As Long
        Dim i As Long, j As Long

        'get temp copy of cel width/height
        CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
        CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height
        CelEndX = CelX + SelWidth - 1
        CelEndY = CelY + SelHeight - 1

        ' reenable flashing selection border
        Timer2.Enabled = True

        'get celdata under the selection's new location
        ReDim DataUnderSel(SelWidth - 1, SelHeight - 1)
        For i = CelX To CelEndX
          For j = CelY To CelEndY
            'if within bounds
            If i >= 0 And i < CelWidth And j >= 0 And j < CelHeight Then
              'copy celdata to undersel area
      '        DataUnderSel(i - CelX, j - CelY) = GetAGIColor(GetPixel(picPCel.hDC, i, j))
              DataUnderSel(i - CelX, j - CelY) = GetAGIColor(GetPixel(picSelection.hDC, i - CelX, j - CelY))
            End If
          Next j
        Next i

        'build celdata so it matches picPCel
        BuildCelData 0, 0, CelWidth - 1, CelHeight - 1

        'ALWAYS reset cutoriginaldata flag
        DelOriginalData = False

        'stretch the pixel cel into the draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CelWidth, CelHeight, SRCCOPY
        picCel.Refresh
        If ShowGrid And ScaleFactor > 3 Then
          DrawCelGrid
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub MenuClickInsert()

        Dim InsertPos As Byte

        On Error GoTo ErrHandler

        Select Case ViewMode
        'Case vmView
          'does not apply to view
        Case vmLoop
          'if selected loop is negative, it means
          'loop selection is n/a; so can't add a new one
          If SelectedLoop >= 0 Then
            'insert a blank loop here
            InsertPos = CByte(SelectedLoop)
            InsertLoop InsertPos
          End If

        Case vmCel
          'if selected cel is negative, it means
          'cel selection is n/a; so can't add a new one
          If SelectedCel >= 0 Then
            'insert a blank cel here
            InsertPos = CByte(SelectedCel)
            InsertCel InsertPos
          End If

        'Case vmBitmap
          'insert doesn't apply for selection

        End Select
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Public Sub MenuClickOpen()
        'implemented by frmMDIMain

      End Sub
            */
        }

        void viewfrmcode() {
            /*
      Public Sub MenuClickExport()

        If ExportView(ViewEdit, InGame) Then
          If Not InGame Then
            'reset changed flag and caption
            IsChanged = False
            Caption = SVIEWED & ViewEdit.ID

            'disable menu and toolbar button
            frmMDIMain.mnuRSave.Enabled = False
            frmMDIMain.Toolbar1.Buttons("save").Enabled = False
            'update tree node 0
            tvwView.Nodes(1).Text = ViewEdit.ID
          Else
            'for ingame resources, ViewEdit is not actually
            'the ingame resource, but only a copy that can be edited.
            'because the resource ID is changed to match savefile
            'name during the export operation, the ID needs to be
            'forced back to the correct Value
            ViewEdit.ID = Views(ViewNumber).ID
          End If
        End If
      End Sub

      Public Sub MenuClickImport()
        Dim tmpView As AGIView
        Dim i As Long

        On Error GoTo ErrHandler

        'this method is only called by the Main form's Import function
        'the MainDialog object will contain the name of the file
        'being imported.

        'steps to import are to import the View to tmp object
        'clear the existing Image, copy tmpobject to this item
        'and reset it

        Set tmpView = New AGIView
        On Error Resume Next
        tmpView.Import MainDialog.FileName
        If Err.Number <> 0 Then
          ErrMsgBox "An error occurred while importing this view:", "", "Import View Error"
          Exit Sub
        End If


        'clear View
        ViewEdit.Clear
        'copy tmpView data to ViewEdit
        ViewEdit.Resource.InsertData tmpView.Resource.AllData, 0
        'remove the last byte (it is left over from the insert process)
        ViewEdit.Resource.RemoveData ViewEdit.Resource.Size - 1

        'discard the temp pic
        tmpView.Unload
        Set tmpView = Nothing

        'set current selection
        SelectedLoop = 0
        SelectedCel = 0
        'update tree
        UpdateTree
        tvwView.Nodes("Cell0:0").EnsureVisible
        'redisplay cel
        DisplayCel

        'update preview
        If ShowVEPrev Then
          DisplayPrevLoop
          DisplayPrevCel
        End If

        'clear the undo buffer
        If UndoCol.Count > 0 Then
          For i = UndoCol.Count To 1 Step -1
            UndoCol.Remove i
          Next i
          SetEditMenu
        End If

        'mark as changed
        MarkAsChanged

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Public Sub MenuClickNew()
        'implemented by frmMDIMain

      End Sub

      Public Sub MenuClickInGame()
        'toggles the game state of an object

        Dim rtn As VbMsgBoxResult
        Dim tmpNode As Node
        Dim blnDontAsk As Boolean

        On Error GoTo ErrHandler

        If InGame Then
          'ask if resource should be exported
          If Settings.AskExport Then
            rtn = MsgBoxEx("Do you want to export '" & ViewEdit.ID & "' before removing it from your game?", _
                              vbQuestion + vbYesNoCancel, "Export View Before Removal", , , _
                              "Don't ask this question again", blnDontAsk)
            'save the setting
            Settings.AskExport = Not blnDontAsk
            'if now hiding update settings file
            If Not Settings.AskExport Then
              WriteSetting GameSettings, sGENERAL, "AskExport", Settings.AskExport
            End If
          Else
            'dont ask; assume no
            rtn = vbNo
          End If

          'if canceled,
          Select Case rtn
          Case vbCancel
            Exit Sub

          Case vbYes
            'export it
            MenuClickExport
          Case vbNo
            'nothing to do
          End Select

          'confirm removal
          If Settings.AskRemove Then
            rtn = MsgBoxEx("Removing '" & ViewEdit.ID & "' from your game." & vbCrLf & vbCrLf & "Select OK to proceed, or Cancel to keep it in game.", _
                            vbQuestion + vbOKCancel, "Remove View From Game", , , _
                            "Don't ask this question again", blnDontAsk)

            'save the setting
            Settings.AskRemove = Not blnDontAsk
            'if now hiding, update settings file
            If Not Settings.AskRemove Then
              WriteSetting GameSettings, sGENERAL, "AskRemove", Settings.AskRemove
            End If
          Else
            'assume OK
            rtn = vbOK
          End If

          'if canceled,
          If rtn = vbCancel Then
            Exit Sub
          End If

          'remove the view
          RemoveView ViewNumber

          'unload this form
          Unload Me
        Else
          'add to game

          'verify a game is loaded,
          If Not GameLoaded Then
            Exit Sub
          End If

          'no longer possible; add is disabled if already at max
      '''    'if at Max already
      '''    If Views.Count = 256 Then
      '''      MsgBox "Maximum number of Views already exist in this game. Remove one or more existing Views, and then try again.", vbInformation + vbOKOnly, "Can't Add View"
      '''      Exit Sub
      '''    End If

          'show add resource form
          With frmGetResourceNum
            .ResType = AGIResType.View
            .WindowFunction = grAddInGame
            'setup before loading so ghosts don't show up
            .FormSetup
            .Show vbModal, frmMDIMain

            'if user makes a choice
            If Not .Canceled Then
              'store number
              ViewNumber = .NewResNum
              'new id
              ViewEdit.ID = .txtID.Text
              'add view
              AddNewView ViewNumber, ViewEdit

              'copy the view back (to ensure internal variables are copied)
              ViewEdit.Clear
              ViewEdit.SetView Views(ViewNumber)

              'now we can unload the newly added view;
              Views(ViewNumber).Unload

              'update caption and properties
              tvwView.Nodes(1).Text = ResourceName(ViewEdit, True, True)
              Caption = SVIEWED & tvwView.Nodes(1).Text
              PaintPropertyWindow

              'set ingame flag
              InGame = True
              'reset changed flag
              IsChanged = False

              'change menu caption
              frmMDIMain.mnuRInGame.Caption = "Remove from Game"
              frmMDIMain.Toolbar1.Buttons("remove").Image = 10
              frmMDIMain.Toolbar1.Buttons("remove").ToolTipText = "Remove from Game"
            End If
          End With

          Unload frmGetResourceNum
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next

      End Sub
      Public Sub MenuClickRenumber()

        'renumbers a resource

        Dim NewResNum As Byte

        On Error GoTo ErrHandler

        'if not in a game
        If Not InGame Then
          Exit Sub
        End If

        'get new number
        NewResNum = RenumberResource(ViewNumber, AGIResType.View)

        'if changed
        If NewResNum <> ViewNumber Then
          'copy renumbered view into viewedit object
          ViewEdit.SetView Views(NewResNum)

          'update number
          ViewNumber = NewResNum

          'update caption
          Caption = SVIEWED & ResourceName(ViewEdit, InGame, True)
          If ViewEdit.IsChanged Then
            Caption = sDM & Caption
          End If

          'and tree
          tvwView.Nodes(1).Text = ViewEdit.ID
          'force repaint of property window
          PaintPropertyWindow
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Public Sub MenuClickCustom1()

        'export a loop as a gif

        On Error GoTo ErrHandler

        '*'Debug.Assert SelectedLoop >= 0
        ExportLoop ViewEdit.Loops(SelectedLoop)

        Screen.MousePointer = vbDefault

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub MenuClickPaste()
        'this method assumes that the
        'calling function has properly set the
        'paste format Value

        Dim NewLoopNo As Byte, NewCelNo As Byte
        Dim NextUndo As ViewUndo

        On Error GoTo ErrHandler

        Select Case ViewCBMode
        'Case vmView
          'paste not enabled for view, so
          'don't need to check for it
        Case vmLoop
          'paste the loop that is on the clipboard

          'if there is not enough room
          If ViewEdit.Loops.Count = MAXLOOPS Then
            'don't add
            MsgBoxEx "Maximum number of loops already exist in this view.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Unable to Paste Loop", WinAGIHelp, "htm\agi\views.htm#loop"
            Exit Sub
          End If

          'add a new loop
          '*'Debug.Assert SelectedLoop <> -1
          NewLoopNo = CByte(SelectedLoop)

          ViewEdit.Loops.Add NewLoopNo

          If Settings.ViewUndo <> 0 Then
            'add undo data
            Set NextUndo = New ViewUndo
            NextUndo.UDAction = udvPasteLoop
            NextUndo.UDLoopNo = NewLoopNo
            AddUndo NextUndo
          End If

          'copy source loop
          ViewEdit.Loops(NewLoopNo).CopyLoop ClipViewLoop

          'update tree
          UpdateTree
          'select cel 0 of new loop
          SelectedLoop = NewLoopNo
          SelectedCel = 0
          'display
          DisplayCel True
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If


        Case vmCel
          'paste a cel at current location

          'if at Max number of cels
          If ViewEdit.Loops(SelectedLoop).Cels.Count = MAXCELS Then
            MsgBoxEx "Maximum number of cels already exist in this loop.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Unable to Paste Cel", WinAGIHelp, "htm\agi\views.htm#cel"
            Exit Sub
          End If

          'add new cel
          '*'Debug.Assert SelectedLoop <> -1
          '*'Debug.Assert SelectedCel <> -1
          NewCelNo = CByte(SelectedCel)
          ViewEdit.Loops(SelectedLoop).Cels.Add NewCelNo

          If Settings.ViewUndo <> 0 Then
            'set undo data
            Set NextUndo = New ViewUndo
            NextUndo.UDAction = udvPasteCel
            NextUndo.UDCelNo = NewCelNo
            NextUndo.UDLoopNo = SelectedLoop
            AddUndo NextUndo
          End If

          'copy it
          ViewEdit.Loops(SelectedLoop).Cels(NewCelNo).CopyCel ClipViewCel

          'update tree
          UpdateTree
          'select new cel
          SelectedCel = NewCelNo
          'display
          DisplayCel True

        Case vmBitmap
          PasteSelection 0, 0

          'update preview
          If ShowVEPrev Then
            DisplayPrevCel
          End If

        End Select
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Public Sub MenuClickSelectAll()

        Select Case ViewMode
        Case vmLoop
        Case vmCel
          'only allow this if tool is 'select'
          If SelectedTool <> ttSelect Then
            SelectedTool = ttSelect
            'press the button
            tlbView.Buttons("select").Value = tbrPressed
          End If

          'select entire Image
          CelAnchorX = 0
          CelAnchorY = 0

          SelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
          SelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height

          SelectRegion SelWidth - 1, SelHeight - 1

          'set clipboard to match entire cel
          BitBlt picClipboard.hDC, 0, 0, 160, 168, picPCel.hDC, 0, 0, SRCCOPY
          picClipboard.Refresh

          'set the selection
          SetSelection

          'always reset operation
          CurrentOperation = opNone
        End Select
      End Sub

      Public Sub MenuClickUndo()

        Dim NextUndo As ViewUndo
        Dim i As Long, j As Long
        Dim CelWidth As Byte, CelHeight As Byte
        Dim CelStartX As Long, CelStartY As Long
        Dim CelEndX As Long, CelEndY As Long
        Dim posX As Long, posY As Long
        Dim tmpCelData() As AGIColors
        Dim rtn As Long

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
        tlbView.Buttons("undo").Enabled = (UndoCol.Count > 0)
        If frmMDIMain.mnuEUndo.Enabled Then
          frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(VIEWUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
        Else
          frmMDIMain.mnuEUndo.Caption = "&Undo " & vbTab & "Ctrl+Z"
        End If

        'undo the action
        Select Case NextUndo.UDAction
        Case udvAddLoop, udvPasteLoop
          'get loop to delete
          SelectedLoop = NextUndo.UDLoopNo
          'delete loop (and add back to clipboard object, if it was pasted)
          DeleteLoop NextUndo.UDAction = udvPasteLoop, True

        Case udvAddCel, udvPasteCel
          'get cel to delete
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          'delete cel (and add back to clipboard object, if it was pasted)
          DeleteCel NextUndo.UDAction = udvPasteCel, True

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvDelLoop, udvCutLoop
          'get loop number to insert
          SelectedLoop = NextUndo.UDLoopNo
          'add a new loop here
          ViewEdit.Loops.Add CByte(SelectedLoop)
          'if the loop was mirrored
          If NextUndo.UndoData(0) = True Then
            'reset mirror for this loop
            ViewEdit.SetMirror SelectedLoop, NextUndo.UndoData(1)
          Else
            'copy from undosource
            ViewEdit.Loops(SelectedLoop).CopyLoop NextUndo.UndoLoop
          End If

          'select and display first cel in this loop
          SelectedCel = 0
          UpdateTree
          tvwView.Nodes("Cell" & CStr(SelectedLoop) & ":0").Selected = True
          tvwView.Nodes("Cell" & CStr(SelectedLoop) & ":0").EnsureVisible
          DisplayCel True
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvDelCel, udvCutCel
          'get loop and cel to insert
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          'add a cel
          ViewEdit.Loops(SelectedLoop).Cels.Add CByte(SelectedCel)
          'copy undo cel
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CopyCel NextUndo.UndoCel

          'select and display this cel
          UpdateTree
          tvwView.Nodes("Cell" & CStr(SelectedLoop) & ":" & CStr(SelectedCel)).EnsureVisible
          DisplayCel True

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvIncHeight
          'retrieve affected loop/cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          SelectedProp = 2
          'adjust height
          ChangeHeight NextUndo.UndoData(0), True

        Case udvIncWidth
          'retrieve affected loop/cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          SelectedProp = 1
          'adjust height
          ChangeWidth NextUndo.UndoData(0), True

        Case udvDecHeight
          'retrieve affected loop/cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          SelectedProp = 2
          'get temp copies of height/width
          CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
          CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height
          'set new height
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height = NextUndo.UndoData(0)

          'recover data
          tmpCelData = NextUndo.CelData
          For j = CelHeight To NextUndo.UndoData(0) - 1
            For i = 0 To CelWidth - 1
              'add this cel pixel's color
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = tmpCelData(i, j - CelHeight)
            Next i
          Next j

          'display cel
          DisplayCel True
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If


        Case udvDecWidth
          'retrieve affected loop/cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          SelectedProp = 1
          'get temp copies of height/width
          CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
          CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height
          'set new width
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width = NextUndo.UndoData(0)

          'recover data
          tmpCelData = NextUndo.CelData
          For i = CelWidth To NextUndo.UndoData(0) - 1
            For j = 0 To CelHeight - 1
              'get this cel pixel's color
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = tmpCelData(i - CelWidth, j)
            Next j
          Next i

          'display cel
          DisplayCel True
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

       Case udvFlipH
          'display cel to undo
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          DisplayCel True

          'if flipping just a selection
          If NextUndo.UndoData(4) = 0 Then
            'set selection shapes
            shpView.Left = NextUndo.UndoData(0) * ScaleFactor * 2 - 1
            shpView.Top = NextUndo.UndoData(1) * ScaleFactor - 1
            shpView.Width = NextUndo.UndoData(2) * ScaleFactor * 2 + 2
            shpView.Height = NextUndo.UndoData(3) * ScaleFactor + 2
            shpSurface.Width = shpView.Width
            shpSurface.Height = shpView.Height
            shpSurface.Move picCel.Left + shpView.Left, picCel.Top + shpView.Top
            shpView.Visible = True
            shpSurface.Visible = True
            Timer2.Enabled = True

            'restore the clipboard
            SelWidth = NextUndo.UndoData(2)
            SelHeight = NextUndo.UndoData(3)

            'get tempdata for processing
            tmpCelData = NextUndo.CelData
             'fill clipoboard with data
            For i = 0 To SelWidth - 1
              For j = 0 To SelHeight - 1
                'copy data under pasted selection to clipboard
                SetPixelV picClipboard.hDC, i, j, EGAColor(tmpCelData(i, j))
              Next j
            Next i
          Else
            'no selection
            shpView.Visible = False
            shpSurface.Visible = False
          End If

          'now -reflip
          FlipH True

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvFlipV
          'display cel to undo
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          DisplayCel True

          'if flipping just a selection
          If NextUndo.UndoData(4) = 0 Then
            'set selection shapes
            shpView.Left = NextUndo.UndoData(0) * ScaleFactor * 2 - 1
            shpView.Top = NextUndo.UndoData(1) * ScaleFactor - 1
            shpView.Width = NextUndo.UndoData(2) * ScaleFactor * 2 + 2
            shpView.Height = NextUndo.UndoData(3) * ScaleFactor + 2
            shpSurface.Width = shpView.Width
            shpSurface.Height = shpView.Height
            shpSurface.Move picCel.Left + shpView.Left, picCel.Top + shpView.Top
            shpView.Visible = True
            shpSurface.Visible = True
            Timer2.Enabled = True
            'restore the clipboard
            SelWidth = NextUndo.UndoData(2)
            SelHeight = NextUndo.UndoData(3)

            'get tempdata for processing
            tmpCelData = NextUndo.CelData
             'fill clipoboard with data
            For i = 0 To SelWidth - 1
              For j = 0 To SelHeight - 1
                'copy data under pasted selection to clipboard
                SetPixelV picClipboard.hDC, i, j, EGAColor(tmpCelData(i, j))
              Next j
            Next i
          Else
            'no selection
            shpView.Visible = False
            shpSurface.Visible = False
          End If

          'now -reflip
          FlipV True

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvLine
          'get affected cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          'retrieve start and end points
          CelStartX = NextUndo.UndoData(0)
          CelStartY = NextUndo.UndoData(1)
          CelEndX = NextUndo.UndoData(2)
          CelEndY = NextUndo.UndoData(3)

          'draw the line
          DrawLineOnCel CelStartX, CelStartY, CelEndX, CelEndY, NextUndo, True
          'display the cel
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvBox
          'get affected cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          'get start and end points
          CelStartX = NextUndo.UndoData(0)
          CelStartY = NextUndo.UndoData(1)
          CelEndX = NextUndo.UndoData(2)
          CelEndY = NextUndo.UndoData(3)
          For i = CelStartX To CelEndX
            ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelStartY) = NextUndo.UndoData(i - CelStartX + 4)
            ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelEndY) = NextUndo.UndoData(i - 2 * CelStartX + 5 + CelEndX)
          Next i
          For j = CelStartY To CelEndY
            ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, j) = NextUndo.UndoData(2 * (CelEndX - CelStartX) + 6 + j - CelStartY)
            ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelEndX, j) = NextUndo.UndoData(2 * (CelEndX - CelStartX) + (CelEndY - CelStartY) + 7 + j - CelStartY)
          Next j
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvBoxFill
          'get affected cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          'get start and end points
          CelStartX = NextUndo.UndoData(0)
          CelStartY = NextUndo.UndoData(1)
          CelEndX = NextUndo.UndoData(2)
          CelEndY = NextUndo.UndoData(3)

          For i = CelStartX To CelEndX
            For j = CelStartY To CelEndY
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = NextUndo.UndoData(4 + (i - CelStartX) + (j - CelStartY) * (CelEndX - CelStartX + 1))
            Next j
          Next i
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvDraw, udvErase
          'get affected cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo

          'undo pixels by stepping backward through array
          tmpCelData = NextUndo.CelData
          j = tmpCelData(0)
          For i = j To 1 Step -1
            'get x, Y, and color (use CelEndX for color)
            CelStartX = (tmpCelData(i) And &HFF0000) / &H10000
            CelStartY = (tmpCelData(i) And &HFF00&) / &H100&
            CelEndX = (tmpCelData(i) And &HFF&)
            'set pixel
            ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, CelStartY) = CelEndX
          Next i
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvPaintFill
          'get affected cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = NextUndo.CelData
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvMirror
          'get affected loop
          SelectedLoop = NextUndo.UDLoopNo
          'select first cel
          SelectedCel = 0
          'unmirror loop
          ViewEdit.Loops(SelectedLoop).UnMirror
          'if old mirror another loop
          If NextUndo.UndoData(0) <> -1 Then
            ViewEdit.SetMirror SelectedLoop, NextUndo.UndoData(0)
          Else
            'copy old loop
            ViewEdit.Loops(SelectedLoop).CopyLoop NextUndo.UndoLoop
          End If
          'update tree
          UpdateTree
          'displaycel
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvChangeTransCol
          'get affected cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          SelectedProp = 3
          'set transcolor
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor = NextUndo.UndoData(0)
          'change background to match color
          picCel.BackColor = EGAColor(NextUndo.UndoData(0))
          'display
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvCutSelection, udvDelSelection
          'get affected cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          'display the cel in its current state
          DisplayCel

          'get starting location and size of deleted data
          CelStartX = NextUndo.UndoData(0)
          CelStartY = NextUndo.UndoData(1)
          SelWidth = NextUndo.UndoData(2)
          SelHeight = NextUndo.UndoData(3)

          'reset selection shape positions
          shpView.Move CelStartX * 2 * ScaleFactor - 1, CelStartY * ScaleFactor - 1
          shpSurface.Move shpView.Left + picCel.Left, shpView.Top + picCel.Top
          shpView.Height = SelHeight * ScaleFactor + 2
          shpView.Width = SelWidth * 2 * ScaleFactor + 2
          shpSurface.Height = SelHeight * ScaleFactor + 2
          shpSurface.Width = SelWidth * 2 * ScaleFactor + 2
          'show them
          Timer2.Enabled = True
          shpView.Visible = True
          shpSurface.Visible = True

          'get tempdata for processing
          tmpCelData = NextUndo.CelData

          'restore deleted data to clipboard
          For i = 0 To SelWidth - 1
            For j = 0 To SelHeight - 1
              SetPixelV picClipboard.hDC, i, j, tmpCelData(i, j)
            Next j
          Next i
          picClipboard.Refresh

          'copy clipboard image to pcel
          BitBlt picPCel.hDC, CelStartX, CelStartY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
          picPCel.Refresh

          'if this was deleted from its original location
          If NextUndo.UndoData(4) Then
            'ensure selection is cleared
            picSelection.BackColor = EGAColor(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor)
          Else
            'restore original DataUnderSel
            ReDim DataUnderSel(SelWidth - 1, SelHeight - 1)
            For i = 0 To SelWidth - 1
              For j = 0 To SelHeight - 1
                DataUnderSel(i, j) = tmpCelData(i, j)
                SetPixelV picSelection.hDC, i, j, EGAColor(tmpCelData(i, SelHeight + j))
              Next j
            Next i
          End If

          'update the actual view with correct data
          BuildCelData 0, 0, CelWidth - 1, CelHeight - 1

          'stretch it to cel draw pic
          StretchBlt picCel.hDC, 0&, 0&, CLng(picCel.Width), CLng(picCel.Height), picPCel.hDC, 0&, 0&, CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width), CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height), SRCCOPY
          If ShowGrid And ScaleFactor > 3 Then
            DrawCelGrid
          End If

          'set not moved flag
          SelectionNotMoved = True

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvPasteSelection
          'get affected cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          'display this cel
          DisplayCel

          'get old data
          CelStartX = NextUndo.UndoData(0)
          CelStartY = NextUndo.UndoData(1)
          CelEndX = NextUndo.UndoData(2)
          CelEndY = NextUndo.UndoData(3)
          'get data that was under pasted data
          tmpCelData = NextUndo.CelData
          'temp copy of height/width
          CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
          CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height

          For i = CelStartX To CelEndX
            For j = CelStartY To CelEndY
              'if within bounds
              If i >= 0 And i < CelWidth And j >= 0 And j < CelHeight Then
                'save this data
                ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = tmpCelData(i - CelStartX, j - CelStartY)
                SetPixelV picPCel.hDC, i, j, EGAColor(tmpCelData(i - CelStartX, j - CelStartY))
              End If
            Next j
          Next i
          'hide selection shapes
          Timer2.Enabled = False
          shpView.Visible = False
          shpSurface.Visible = False

          'stretch it to cel draw pic
          StretchBlt picCel.hDC, 0&, 0&, CLng(picCel.Width), CLng(picCel.Height), picPCel.hDC, 0&, 0&, CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width), CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height), SRCCOPY
          picCel.Refresh
          If ShowGrid And ScaleFactor > 3 Then
            DrawCelGrid
          End If

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvMoveSelection
          'get affected cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          'display the cel in its current state
          DisplayCel

          'get temp copy of height/width
          CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
          CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height

          'get location and size of selection
          CelStartX = NextUndo.UndoData(0)
          CelStartY = NextUndo.UndoData(1)
          SelWidth = NextUndo.UndoData(2)
          SelHeight = NextUndo.UndoData(3)
          'get location of position being 'unmoved'
          CelEndX = NextUndo.UndoData(4)
          CelEndY = NextUndo.UndoData(5)

          'reset selection shape positions
          shpView.Move CelStartX * 2 * ScaleFactor - 1, CelStartY * ScaleFactor - 1
          shpSurface.Move shpView.Left + picCel.Left, shpView.Top + picCel.Top
          shpView.Height = SelHeight * ScaleFactor + 2
          shpView.Width = SelWidth * 2 * ScaleFactor + 2
          shpSurface.Height = SelHeight * ScaleFactor + 2
          shpSurface.Width = SelWidth * 2 * ScaleFactor + 2
          'show them
          Timer2.Enabled = True
          shpView.Visible = True
          shpSurface.Visible = True

          'get tempdata for processing
          tmpCelData = NextUndo.CelData

          'restore area under area 'unmoved'
          For i = 0 To SelWidth - 1
            For j = 0 To SelHeight - 1
              SetPixelV picSelection.hDC, i, j, tmpCelData(i, SelHeight + j)
            Next j
          Next i
          BitBlt picPCel.hDC, CelEndX, CelEndY, SelWidth, SelHeight, picSelection.hDC, 0, 0, SRCCOPY
          picPCel.Refresh

          'restore area moved
          For i = 0 To SelWidth - 1
            For j = 0 To SelHeight - 1
              SetPixelV picClipboard.hDC, i, j, tmpCelData(i, 2 * SelHeight + j)
            Next j
          Next i
          picClipboard.Refresh

          'move the area back to old position
          BitBlt picPCel.hDC, CelStartX, CelStartY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
          picPCel.Refresh

          'restore original DataUnderSel
          ReDim DataUnderSel(SelWidth - 1, SelHeight - 1)
          For i = 0 To SelWidth - 1
            For j = 0 To SelHeight - 1
              DataUnderSel(i, j) = tmpCelData(i, j)
              SetPixelV picSelection.hDC, i, j, EGAColor(tmpCelData(i, j))
            Next j
          Next i

          'update the actual view with correct data
          BuildCelData 0, 0, CelWidth - 1, CelHeight - 1

          'stretch it to cel draw pic
          StretchBlt picCel.hDC, 0&, 0&, CLng(picCel.Width), CLng(picCel.Height), picPCel.hDC, 0&, 0&, CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width), CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height), SRCCOPY
          If ShowGrid And ScaleFactor > 3 Then
            DrawCelGrid
          End If

          'set not moved flag
          SelectionNotMoved = True

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvClearLoop
          'get affected loop
          SelectedLoop = NextUndo.UDLoopNo
          'select first cel
          SelectedCel = 0
          ViewEdit.Loops(SelectedLoop).CopyLoop NextUndo.UndoLoop
          'if loop was mirrored
          If NextUndo.UndoData(0) Then
            'set mirror for previously mirrored cel
            ViewEdit.SetMirror NextUndo.UDCelNo, SelectedLoop
          End If
          'update tree
          UpdateTree
          'display cel
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvClearCel
        'get affected loop and cel
          SelectedLoop = NextUndo.UDLoopNo
          SelectedCel = NextUndo.UDCelNo
          'restore cel
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CopyCel NextUndo.UndoCel
          'display cel
          DisplayCel
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case udvChangeVDesc
          SelectedProp = 3
          'change vDesc
          ViewEdit.ViewDescription = NextUndo.OldText
          'if view mode is entire view
          If ViewMode = vmView Then
            'update display
            PaintPropertyWindow
          End If
        End Select
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Function AskClose() As Boolean

        Dim rtn As VbMsgBoxResult

        On Error GoTo ErrHandler

        'assume okay to close
        AskClose = True

        'if exiting due to error on form load, viewedit is set to nothing
        If ViewEdit Is Nothing Then
          Exit Function
        End If

        'if the view needs to be saved,
        '(number is set to -1 if closing is forced)
        If IsChanged And ViewNumber <> -1 Then
          rtn = MsgBox("Do you want to save changes to " & ViewEdit.ID & " ?", vbYesNoCancel, "View Editor")

          Select Case rtn
          Case vbYes
            'save, then continue closing
            MenuClickSave
          Case vbNo
            'don't save, and continue closing
          Case vbCancel
            'don't continue closing
            AskClose = False
          End Select
        End If
      Exit Function

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Function


      Private Sub BeginSelectionMove(ByVal CelX As Long, ByVal CelY As Long)

        Dim tmpCelData() As AGIColors
        Dim CelWidth As Long, CelHeight As Long
        Dim i As Long, j As Long
        Dim rtn As Long

        On Error GoTo ErrHandler

        'set operation to move selection
        CurrentOperation = opMoveSelection

        'save this pos as initial old values
        '(adjusted for the one pixel width of selection outline)
        ' and convert to celpixel scale
        OldPosX = (shpView.Left + 1) / 2 / ScaleFactor
        OldPosY = (shpView.Top + 1) / ScaleFactor

        'get offset to upper left corner of selction from cel where pointer is at:
        CelXOffset = CelX - OldPosX
        CelYOffset = CelY - OldPosY

        'set flag
        SelectionNotMoved = True

        ' save the data under selection
        UndoDataUnderSel() = DataUnderSel()

        'copy data from UnderSel area to cel data
        tmpCelData = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData
        CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
        CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height
        For i = 0 To SelWidth - 1
          For j = 0 To SelHeight - 1
            'if within bounds,
            If CelX - CelXOffset + i < CelWidth And CelX - CelXOffset + i >= 0 And _
               CelY - CelYOffset + j < CelHeight And CelY - CelYOffset + j >= 0 Then
              tmpCelData(CelX - CelXOffset + i, CelY - CelYOffset + j) = DataUnderSel(i, j)
            End If
          Next j
        Next i
        ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpCelData

        'turn off selection flashing
        Timer2.Enabled = False
        shpView.BorderStyle = 3
        shpSurface.BorderStyle = 3
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub ChangeHeight(ByVal NewCelHeight As Long, Optional ByVal DontUndo As Boolean = False)

        Dim NextUndo As ViewUndo
        Dim i As Long, j As Long
        Dim CelWidth As Byte, CelHeight As Byte
        Dim tmpCelData() As AGIColors

        On Error GoTo ErrHandler

        'height should be validated before
        'calling this, but just in case:
        'validate Height
        If NewCelHeight < 1 Then
          NewCelHeight = 1
        End If
        If NewCelHeight > MAX_CEL_H Then
          NewCelHeight = MAX_CEL_H
        End If

        'local copy of height
        CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height

        'if no change,
        If NewCelHeight = CelHeight Then
          'just exit
          Exit Sub
        End If

        'if not skipping undo
        If Not DontUndo And Settings.ViewUndo <> 0 Then
          'local copy of width
          CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
          'set undo properties
          Set NextUndo = New ViewUndo
          NextUndo.UDLoopNo = SelectedLoop
          NextUndo.UDCelNo = SelectedCel
          NextUndo.UndoData(0) = CelHeight

          'if new height is greater than old width
          If NewCelHeight > CelHeight Then
            NextUndo.UDAction = udvIncHeight
          Else
            NextUndo.UDAction = udvDecHeight
            'and store data being eliminated
            ReDim tmpCelData(CelWidth - 1, (CelHeight - NewCelHeight - 1))
            For j = NewCelHeight To CelHeight - 1
              For i = 0 To CelWidth - 1
                'add this cel pixel's color
                tmpCelData(i, j - NewCelHeight) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j)
              Next i
            Next j
            NextUndo.CelData = tmpCelData
          End If
          'add undo object
          AddUndo NextUndo
        End If

        'set new height
        ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height = NewCelHeight
        'redraw cel
        DisplayCel
        'update preview
        If ShowVEPrev Then
          DisplayPrevLoop
          'make sure to reset preview cel to match selected cel
          PrevCel = SelectedCel
          DisplayPrevCel
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub ChangeWidth(ByVal NewCelWidth As Long, Optional ByVal DontUndo As Boolean = False)

        Dim NextUndo As ViewUndo
        Dim i As Long, j As Long
        Dim CelWidth As Byte, CelHeight As Byte
        Dim tmpCelData() As AGIColors

        On Error GoTo ErrHandler

        'width should be validated before
        'calling this, but just in case:
        'validate width
        If NewCelWidth < 1 Then
          NewCelWidth = 1
        End If
        If NewCelWidth > MAX_CEL_W Then
          NewCelWidth = MAX_CEL_W
        End If

        'local copy of current width
        CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
        'if no change,
        If NewCelWidth = CelWidth Then
          'just exit
          Exit Sub
        End If

        If Not DontUndo And Settings.ViewUndo <> 0 Then
          'local copy of height
          CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height
          'set undo properties
          Set NextUndo = New ViewUndo
          NextUndo.UDLoopNo = SelectedLoop
          NextUndo.UDCelNo = SelectedCel
          NextUndo.UndoData(0) = CelWidth

          'if new width is greater than old width
          If NewCelWidth > CelWidth Then
            NextUndo.UDAction = udvIncWidth
          Else
            NextUndo.UDAction = udvDecWidth
            'and store data being eliminated
            ReDim tmpCelData(CelWidth - NewCelWidth - 1, CelHeight)
            For i = NewCelWidth To CelWidth - 1
              For j = 0 To CelHeight - 1
                'add this cel pixel's color
                tmpCelData(i - NewCelWidth, j) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j)
              Next j
            Next i
            NextUndo.CelData = tmpCelData
          End If

          'add undo object
          AddUndo NextUndo
        End If

        'set new width
        ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width = NewCelWidth
        'redraw cel
        DisplayCel
        'update preview
        If ShowVEPrev Then
          DisplayPrevLoop
          'make sure to reset preview cel to match selected cel
          PrevCel = SelectedCel
          DisplayPrevCel
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Private Sub CopySelection()

        Dim rtn As Long
        Dim i As Long, j As Long
        Dim tmpCDC As Long, tmpPDC As Long
        Dim tmpH As Long, tmpW As Long
        Dim tmpL As Long, tmpT As Long

        'if there is a selection
        If shpView.Visible Then
          'get unscaled height/width
          tmpW = (shpView.Width - 2) / (ScaleFactor * 2)
          tmpH = (shpView.Height - 2) / ScaleFactor
          tmpL = (shpView.Left + 1) / (ScaleFactor * 2)
          tmpT = (shpView.Top + 1) / ScaleFactor

          'copy selection to global object
          ViewClipboard.Width = tmpW
          ViewClipboard.Height = tmpH
          ViewClipboard.Cls
          For i = 0 To tmpW - 1
            For j = 0 To tmpH - 1
              rtn = GetPixel(picPCel.hDC, tmpL + i, tmpT + j)
              rtn = SetPixelV(ViewClipboard.hDC, i, j, rtn)
            Next j
          Next i
          ViewClipboard.Refresh

          'now copy to system clipboard
          Clipboard.Clear
          Clipboard.SetData ViewClipboard.Image, vbCFBitmap

          'set format flag
          ViewCBMode = vmBitmap

          'set edit menu
          SetEditMenu
        End If
      End Sub
      Private Sub CutSelection()
        'copy it
        CopySelection
        'then delete it
        DeleteSelection

        If UndoCol.Count <> 0 Then
          'change last undo object so it reads as 'undo cut'
          UndoCol(UndoCol.Count).UDAction = udvCutSelection
          frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(VIEWUNDOTEXT + udvCutSelection) & vbTab & "Ctrl+Z"
        End If

        'hide selection
        Timer2.Enabled = False
        shpView.Visible = False
        shpSurface.Visible = False
      End Sub


      Public Sub DeleteLoop(CutLoop As Boolean, Optional DontUndo As Boolean = False)

        Dim NextUndo As ViewUndo

        'only not a valid loop
        If SelectedLoop < 0 Then
          Exit Sub
        End If

        'if not last loop
        If ViewEdit.Loops.Count > 1 Then
          'if cutting
          If CutLoop Then
            'set clipboard loop to this loop
            Set ClipViewLoop = New AGILoop
            ClipViewLoop.CopyLoop ViewEdit.Loops(SelectedLoop)
          End If

          'if not skipping undo
          If Not DontUndo And Settings.ViewUndo <> 0 Then
            Set NextUndo = New ViewUndo
            If CutLoop Then
              NextUndo.UDAction = udvCutLoop
            Else
              NextUndo.UDAction = udvDelLoop
            End If
            NextUndo.UDLoopNo = SelectedLoop

            'if mirrored,
            If ViewEdit.Loops(SelectedLoop).Mirrored Then
              'only need to store mirror loop
              NextUndo.ResizeData 1
              NextUndo.UndoData(0) = True
              NextUndo.UndoData(1) = ViewEdit.Loops(SelectedLoop).MirrorLoop
            Else
              'not mirrored, so save the entire loop
              Set NextUndo.UndoLoop = New AGILoop
              NextUndo.UndoLoop.CopyLoop ViewEdit.Loops(SelectedLoop)
            End If
            AddUndo NextUndo
          End If
          'now delete it
          ViewEdit.Loops.Remove SelectedLoop

          'update tree
          UpdateTree
          'select first loop
          SelectedLoop = 0
          tvwView.Nodes("Loop0").Selected = True
          tvwView.Nodes("Loop0").EnsureVisible
          'hide view
          picSurface.Visible = False
          'repaint property window
          PaintPropertyWindow
        Else
          MsgBox "View must contain at least one loop.", vbInformation, "Can't " & IIf(CutLoop, "cut", "delete") & " loop"
        End If
      End Sub

      Private Sub DeleteSelection()

        Dim rtn As Long
        Dim NextUndo As ViewUndo
        Dim i As Long, j As Long
        Dim CelStartX As Long, CelStartY As Long
        Dim CelEndX As Long, CelEndY As Long
        Dim tmpCelData() As AGIColors

        'if there is a selection
        If shpView.Visible Then
          'calculate start and end corners
          CelStartX = (shpView.Left + 1) / ScaleFactor / 2
          CelStartY = (shpView.Top + 1) / ScaleFactor
          CelEndX = CelStartX + SelWidth - 1
          CelEndY = CelStartY + SelHeight - 1

          If Settings.ViewUndo <> 0 Then
            'save undo data
            Set NextUndo = New ViewUndo
            With NextUndo
              .UDAction = udvDelSelection
              .UDLoopNo = SelectedLoop
              .UDCelNo = SelectedCel
              .ResizeData 4
              'save original location of selection
              .UndoData(0) = CelStartX
              .UndoData(1) = CelStartY
              'save height/width of selection
              .UndoData(2) = SelWidth
              .UndoData(3) = SelHeight
              'flag set if deleting original (background will be blank)
              .UndoData(4) = DelOriginalData
              'save data being deleted
              ReDim tmpCelData(SelWidth - 1, SelHeight - 1)
              For i = 0 To SelWidth - 1
                For j = 0 To SelHeight - 1
                  tmpCelData(i, j) = GetPixel(picClipboard.hDC, i, j)
                Next j
              Next i
              'then save the data underneath it
              ReDim Preserve tmpCelData(SelWidth - 1, SelHeight * 2 - 1)
              For i = 0 To SelWidth - 1
                For j = 0 To SelHeight - 1
                    tmpCelData(i, SelHeight + j) = GetAGIColor(GetPixel(picSelection.hDC, i, j))
                Next j
              Next i
              .CelData = tmpCelData
            End With
            'add to undo collection
            AddUndo NextUndo
          End If

          'copy selection to pixel cel to clear the selection
          BitBlt picPCel.hDC, CelStartX, CelStartY, SelWidth, SelHeight, picSelection.hDC, 0, 0, SRCCOPY
          picPCel.Refresh
          'copy pixel cel to draw cel
          StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY
          picCel.Refresh
          If ShowGrid And ScaleFactor > 3 Then
            DrawCelGrid
          End If

          'update celdata by using buildceldata method
          BuildCelData CelStartX, CelStartY, CelEndX, CelEndY
          'hide selections
          Timer2.Enabled = False
          shpView.Visible = False
          shpSurface.Visible = False
          'call mouse move once, to force cursor update
          picCel_MouseMove 0, 0, 0, 0
        End If
      End Sub

      Private Sub DisplayPropertyEditBox(ByVal posX As Long, ByVal posY As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal strProp As String)
        'moves the edit box to appropriate position
        'preloads it with appropriate prop Value

        txtProperty.Move picProperties.Left + posX, picProperties.Top + posY
        txtProperty.Width = nWidth
        txtProperty.Height = nHeight

        Select Case strProp
        Case "ID"
          txtProperty.Text = ViewEdit.ID
        Case "DESC"
          txtProperty.Text = ViewEdit.Description
        Case "VIEWDESC"
          txtProperty.Text = Replace(ViewEdit.ViewDescription, vbLf, vbNewLine)
        Case "HEIGHT"
          txtProperty.Text = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height
        Case "WIDTH"
          txtProperty.Text = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
        End Select

        'pass property to textbox tag
        txtProperty.Tag = strProp

        'show text
        txtProperty.ZOrder
        txtProperty.Visible = True
        'select it
        txtProperty.SetFocus
        'move cursor to end
        txtProperty.SelStart = Len(txtProperty.Text)
      End Sub

      Private Sub DisplayPropertyListBox(ByVal posX As Long, ByVal posY As Long, ByVal nWidth As Long, ByVal nHeight As Long, ByVal strProp As String)
        'moves the list box to appropriate position
        'preloads it with appropriate prop Value

        Dim i As Long

        lstProperty.Move picProperties.Left + posX, picProperties.Top + posY
        lstProperty.Width = nWidth
        lstProperty.Height = nHeight
        lstProperty.Clear

        Select Case strProp
        Case "MIRROR"
          'read only property
          Exit Sub

        Case "MLOOP"

        'if loop is greater than or equal to 8,
        '*'Debug.Assert SelectedLoop = ViewEdit.Loops(SelectedLoop).Index
        If SelectedLoop >= 8 Then
          'loops above 7 can't be mirrored
          Exit Sub
        End If

        'add no loop option
        lstProperty.AddItem "None"

        'add other loops that are eligible
        For i = 0 To ViewEdit.Loops.Count - 1
          'if at loop 8
          If i = 8 Then
            Exit For
          End If
          'if loop is not this loop and not a mirror of other loop
          If (i <> SelectedLoop) Then
            'if this loop is not mirrored
            If Not ViewEdit.Loops(i).Mirrored Then
              'add it as an option
              lstProperty.AddItem "Loop" & CStr(i)
            Else
              'if the mirror is this loop
              If ViewEdit.Loops(i).MirrorLoop = SelectedLoop Then
                'add it as an option
                lstProperty.AddItem "Loop" & CStr(i)
              End If
            End If
          End If
        Next i

        'select correct item
        If ViewEdit.Loops(SelectedLoop).Mirrored Then
          lstProperty.Text = "Loop" & CStr(ViewEdit.Loops(SelectedLoop).MirrorLoop)
        Else
          lstProperty.ListIndex = 0
        End If

        Case "TRANSCOL"
          'add colors
          For i = 0 To 15
            lstProperty.AddItem LoadResString(COLORNAME + i), i
          Next i
          'select correct item
          lstProperty.Text = LoadResString(COLORNAME + ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor)
        End Select

        'pass property to tag
        lstProperty.Tag = strProp

        'show list box
        lstProperty.Visible = True
        'set top index to the selected Value
        lstProperty.TopIndex = lstProperty.ListIndex
        'select it
        lstProperty.SetFocus

      End Sub

      Public Sub FlipH(Optional DontUndo As Boolean = False)
        'flips the cel, or the selection, if one was made horizontally
        Dim NextUndo As ViewUndo
        Dim CelStartX As Long, CelStartY As Long
        Dim CelEndX As Long, CelEndY As Long
        Dim CelHeight As Long, CelWidth As Long
        Dim PosHeight As Long, PosWidth As Long
        Dim ClipWidth As Long, ClipHeight As Long
        Dim i As Long, j As Long
        Dim tmpCol As AGIColors, tmpCelData() As AGIColors
        Dim tmpUndo() As AGIColors
        Dim lngColor As Long

        On Error GoTo ErrHandler

        '*'Debug.Assert ViewMode = vmCel
        'if not displaying a cel
        If ViewMode <> vmCel Then
          Exit Sub
        End If

        'local copy of height/width
        CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
        CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height

        'if a selection is made
        If shpView.Visible Then
          'get location and size of selection
          CelStartX = (shpView.Left + 1) / ScaleFactor / 2
          CelStartY = (shpView.Top + 1) / ScaleFactor
          CelEndX = (shpView.Left + shpView.Width - 1) / ScaleFactor / 2 - 1
          CelEndY = (shpView.Top + shpView.Height - 1) / ScaleFactor - 1

          'flip the selection on the drawing surface

          'flip clipboard
          With picClipboard
            StretchBlt .hDC, SelWidth - 1, 0, -SelWidth, SelHeight, .hDC, 0, 0, SelWidth, SelHeight, SRCCOPY
          End With

          'copy flipped clipboard image to pixel cel
          BitBlt picPCel.hDC, CelStartX, CelStartY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
          picPCel.Refresh
          'stretch the pixel cel into the draw cel
          StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CelWidth, CelHeight, SRCCOPY
          picCel.Refresh
          If ShowGrid And ScaleFactor > 3 Then
            DrawCelGrid
          End If

          'temp copy of cel data
          tmpCelData = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData
          'set up undo array (to hold colors in the clipboard)
          ClipWidth = CelEndX - CelStartX + 1
          ClipHeight = CelEndY - CelStartY + 1
          ReDim tmpUndo(ClipWidth - 1, ClipHeight - 1)

          For j = CelStartY To CelEndY
            For i = CelStartX To CelEndX
              'get pixel value, and save it for undoing later
              lngColor = GetAGIColor(GetPixel(picClipboard.hDC, i - CelStartX, j - CelStartY))
              'save it to undo array
              tmpUndo(i - CelStartX, j - CelStartY) = lngColor

              If i >= 0 And i < CelWidth And j >= 0 And j < CelHeight Then
                'save this pixel to cel data
                tmpCelData(i, j) = lngColor
              End If
            Next i
          Next j
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpCelData

          'if not skipping undo
          If Not DontUndo And Settings.ViewUndo <> 0 Then
            'add undo
            Set NextUndo = New ViewUndo
            With NextUndo
              .UDAction = udvFlipH
              .UDLoopNo = SelectedLoop
              .UDCelNo = SelectedCel
              .ResizeData 4 'four?  if flipping all, no need to save celdata
              .UndoData(0) = CelStartX
              .UndoData(1) = CelStartY
              .UndoData(2) = ClipWidth
              .UndoData(3) = ClipHeight
              .UndoData(4) = 0 'means a selection is flipped
              .CelData = tmpUndo
            End With
            AddUndo NextUndo
          End If

        Else
          'flip entire cel
          StretchBlt picPCel.hDC, CelWidth - 1, 0, -CelWidth, CelHeight, picPCel.hDC, 0, 0, CelWidth, CelHeight, SRCCOPY
          picPCel.Refresh
          'stretch the pixel cel into the draw cel
          StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CelWidth, CelHeight, SRCCOPY
          picCel.Refresh
          If ShowGrid And ScaleFactor > 3 Then
            DrawCelGrid
          End If

          'flip celdata
          tmpCelData = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData

          For j = 0 To CelHeight - 1
            For i = 0 To CelWidth \ 2 - 1
              tmpCol = tmpCelData(i, j)
              tmpCelData(i, j) = tmpCelData(CelWidth - 1 - i, j)
              tmpCelData(CelWidth - 1 - i, j) = tmpCol
            Next i
          Next j
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpCelData

          'if not skipping undo
          If Not DontUndo And Settings.ViewUndo <> 0 Then
            'add undo
            Set NextUndo = New ViewUndo
            With NextUndo
              .UDAction = udvFlipH
              .UDLoopNo = SelectedLoop
              .UDCelNo = SelectedCel
              .ResizeData 4
              .UndoData(0) = 0
              .UndoData(1) = 0
              .UndoData(2) = CelWidth
              .UndoData(3) = CelHeight
              .UndoData(4) = 1 'means entire cel is flipped
            End With
            AddUndo NextUndo
          End If
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub FlipV(Optional DontUndo As Boolean = False)

        'flips the cel, or the selection, if one was made vertically
        Dim NextUndo As ViewUndo
        Dim CelStartX As Long, CelStartY As Long
        Dim CelEndX As Long, CelEndY As Long
        Dim CelWidth As Long, CelHeight As Long
        Dim PosHeight As Long, PosWidth As Long
        Dim ClipWidth As Long, ClipHeight As Long
        Dim i As Long, j As Long
        Dim tmpCol As AGIColors, tmpCelData() As AGIColors
        Dim tmpUndo() As AGIColors
        Dim lngColor As Long

        On Error GoTo ErrHandler

        '*'Debug.Assert ViewMode = vmCel
        'if not displaying a cel
        If ViewMode <> vmCel Then
          Exit Sub
        End If

        'local copy of height/width
        CelWidth = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width
        CelHeight = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height

        'if a selection is made
        If shpView.Visible Then
          'get location and size of selection
          CelStartX = (shpView.Left + 1) / ScaleFactor / 2
          CelStartY = (shpView.Top + 1) / ScaleFactor
          CelEndX = (shpView.Left + shpView.Width - 1) / ScaleFactor / 2 - 1
          CelEndY = (shpView.Top + shpView.Height - 1) / ScaleFactor - 1

          'flip the selection on the drawing surface

          'flip clipboard
          With picClipboard
            StretchBlt .hDC, 0, SelHeight - 1, SelWidth, -SelHeight, .hDC, 0, 0, SelWidth, SelHeight, SRCCOPY
          End With

          'copy flipped clipboard image to pixel cel
          BitBlt picPCel.hDC, CelStartX, CelStartY, PosWidth, PosHeight, picClipboard.hDC, 0, 0, SRCCOPY
          BitBlt picPCel.hDC, CelStartX, CelStartY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
          picPCel.Refresh
          'stretch the pixel cel into the draw cel
          StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CelWidth, CelHeight, SRCCOPY
          picCel.Refresh
          If ShowGrid And ScaleFactor > 3 Then
            DrawCelGrid
          End If

          'temp copy of cel data
          tmpCelData = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData
          'set up undo array (to hold colors in the clipboard)
          ClipWidth = CelEndX - CelStartX + 1
          ClipHeight = CelEndY - CelStartY + 1
          ReDim tmpUndo(ClipWidth - 1, ClipHeight - 1)

          For j = CelStartY To CelEndY
            For i = CelStartX To CelEndX
              'get pixel value, and save it for undoing later
              lngColor = GetAGIColor(GetPixel(picClipboard.hDC, i - CelStartX, j - CelStartY))
              'save this pixel to undo array
              tmpUndo(i - CelStartX, j - CelStartY) = lngColor

              If i >= 0 And i < CelWidth And j >= 0 And j < CelHeight Then
                'save this pixel to cel data
                tmpCelData(i, j) = lngColor
              End If
            Next i
          Next j
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpCelData

          'if not skipping undo
          If Not DontUndo And Settings.ViewUndo <> 0 Then
            'add undo
            Set NextUndo = New ViewUndo
            With NextUndo
              .UDAction = udvFlipV
              .UDLoopNo = SelectedLoop
              .UDCelNo = SelectedCel
              .ResizeData 4
              .UndoData(0) = CelStartX
              .UndoData(1) = CelStartY
              .UndoData(2) = ClipWidth
              .UndoData(3) = ClipHeight
              .UndoData(4) = 0 'means a selection is flipped
              .CelData = tmpUndo
            End With
            AddUndo NextUndo
          End If
        Else
          'flip entire cel
          StretchBlt picPCel.hDC, 0, CelHeight - 1, CelWidth, -CelHeight, picPCel.hDC, 0, 0, CelWidth, CelHeight, SRCCOPY
          picPCel.Refresh
          'stretch the pixel cel into the draw cel
          StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CelWidth, CelHeight, SRCCOPY
          picCel.Refresh
          If ShowGrid And ScaleFactor > 3 Then
            DrawCelGrid
          End If

          'flip celdata
          tmpCelData = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData

          For i = 0 To CelWidth - 1
            For j = 0 To CelHeight \ 2 - 1
              tmpCol = tmpCelData(i, j)
              tmpCelData(i, j) = tmpCelData(i, CelHeight - 1 - j)
              tmpCelData(i, CelHeight - 1 - j) = tmpCol
            Next j
          Next i
         ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpCelData

          If Not DontUndo And Settings.ViewUndo <> 0 Then
            'add undo
            Set NextUndo = New ViewUndo
            With NextUndo
              .UDAction = udvFlipV
              .ResizeData 4
              .UDLoopNo = SelectedLoop
              .UDCelNo = SelectedCel
              .UndoData(0) = 0
              .UndoData(1) = 0
              .UndoData(2) = CelWidth
              .UndoData(3) = CelHeight
              .UndoData(4) = 1 'means entire cel is flipped
            End With
            AddUndo NextUndo
          End If
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub MoveSelection(MoveX As Long, MoveY As Long)

        'move selection shapes
        shpView.Move MoveX * ScaleFactor * 2 - 1, MoveY * ScaleFactor - 1
        shpSurface.Move picCel.Left + shpView.Left, picCel.Top + shpView.Top

        'move selection back to old location in pixel cel
        BitBlt picPCel.hDC, OldPosX, OldPosY, SelWidth, SelHeight, picSelection.hDC, 0, 0, SRCCOPY
        picPCel.Refresh
        'copy pixel cel to draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width), CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height), SRCCOPY

        'copy data at this location to selection
        BitBlt picSelection.hDC, 0, 0, SelWidth, SelHeight, picPCel.hDC, MoveX, MoveY, SRCCOPY
        picSelection.Refresh

        'put clipboard Image at new location on pixel cel
        BitBlt picPCel.hDC, MoveX, MoveY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
        picPCel.Refresh
        'copy pixel cel to draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width), CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height), SRCCOPY
        picPCel.Refresh

        'and update the viewedit
      '  BuildCelData OldPosX, OldPosY, OldPosX + SelWidth - 1, OldPosY + SelHeight - 1
        BuildCelData 0, 0, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1, True

        If ShowGrid And ScaleWidth > 3 Then
          DrawCelGrid
        End If

        'if not at starting point
        If MoveX = CelAnchorX - CelXOffset And MoveY = CelAnchorY - CelYOffset Then
          'selection has not moved
          SelectionNotMoved = True
        Else
          'selection has moved
          SelectionNotMoved = False
        End If

        'save old values
        OldPosX = MoveX
        OldPosY = MoveY
      End Sub

      Private Sub PaintPropertyWindow(Optional ByVal Highlight As Boolean = True)

        Dim i As Long
        Dim rtn As Long
        Dim strProp As String

        On Error GoTo ErrHandler

        picProperties.Cls

        'if nothing selected
        If tvwView.SelectedItem Is Nothing Then
          'draw nothing

        'if end of a loop or end of a cel,
        ElseIf tvwView.SelectedItem.Text = "End" Then
          'draw nothing

        'if displaying root
        ElseIf tvwView.SelectedItem.Parent Is Nothing Then
          'id enabled only if view is in a game
          If InGame Then
            DrawProp picProperties, "ID", ViewEdit.ID, 1, Highlight, SelectedProp, 0, True, bfDialog
          Else
            DrawProp picProperties, "ID", ViewEdit.ID, 1, Highlight, SelectedProp, 0, False
          End If
          DrawProp picProperties, "Description", ViewEdit.Description, 2, Highlight, SelectedProp, 0, True, bfDialog
          DrawProp picProperties, "ViewDesc", ViewEdit.ViewDescription, 3, Highlight, SelectedProp, 0, True, bfOver

        'if a loop
        ElseIf tvwView.SelectedItem.Parent.Parent Is Nothing Then
          DrawProp picProperties, "Mirrored", CStr(ViewEdit.Loops(SelectedLoop).Mirrored), 1, Highlight, SelectedProp, 0, False, bfNone
          If ViewEdit.Loops(SelectedLoop).Mirrored Then
            strProp = "Loop " & CStr(ViewEdit.Loops(SelectedLoop).MirrorLoop)
          Else
            strProp = "None"
          End If
          DrawProp picProperties, "MirrorLoop", strProp, 2, Highlight, SelectedProp, 0, True, bfDown

        'must be a cel
        Else
          DrawProp picProperties, "Width", CStr(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width), 1, Highlight, SelectedProp, 0, True, bfNone
          DrawProp picProperties, "Height", CStr(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height), 2, Highlight, SelectedProp, 0, True, bfNone
          DrawProp picProperties, "TransCol", LoadResString(COLORNAME + ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor), 3, Highlight, SelectedProp, 0, True, bfDown
        End If

        'draw outline around header row
        picProperties.Line (PropSplitLoc, 0)-(PropSplitLoc, PROP_ROW_HEIGHT - 1), vbBlack
        picProperties.Line (picProperties.Width - 1, 0)-(picProperties.Width - 1, PROP_ROW_HEIGHT - 1), vbBlack
        picProperties.Line (1, PROP_ROW_HEIGHT - 1)-(PropSplitLoc, PROP_ROW_HEIGHT - 1), vbBlack
        picProperties.Line (PropSplitLoc + 2, PROP_ROW_HEIGHT - 1)-(picProperties.Width - 1, PROP_ROW_HEIGHT - 1), vbBlack

        'fill in header row
        picProperties.Line (1, 1)-(PropSplitLoc - 1, PROP_ROW_HEIGHT - 2), RGB(236, 233, 216), BF
        picProperties.Line (PropSplitLoc + 2, 1)-(picProperties.Width - 2, PROP_ROW_HEIGHT - 2), DK_GRAY, BF

        'draw vertical lines separating columns
        picProperties.Line (PropSplitLoc, PROP_ROW_HEIGHT)-(PropSplitLoc, picProperties.Height - 1), LT_GRAY
        picProperties.Line (picProperties.Width - 1, PROP_ROW_HEIGHT)-(picProperties.Width - 1, picProperties.Height - 1), LT_GRAY

        'draw horizontal lines separating rows
        picProperties.Line (0, 2 * PROP_ROW_HEIGHT - 1)-(picProperties.Width - 2, 2 * PROP_ROW_HEIGHT - 1), LT_GRAY
        picProperties.Line (0, 3 * PROP_ROW_HEIGHT - 1)-(picProperties.Width - 2, 3 * PROP_ROW_HEIGHT - 1), LT_GRAY
        picProperties.Line (0, picProperties.Height - 1)-(picProperties.Width - 1, picProperties.Height - 1), LT_GRAY

        'print column labels
        picProperties.ForeColor = vbBlack
        picProperties.CurrentX = 3
        picProperties.CurrentY = 1
        picProperties.Print "Property"
        picProperties.CurrentX = PropSplitLoc + 3
        picProperties.CurrentY = 2
        picProperties.Print "Value"

      Exit Sub
      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub PasteSelection(ByVal CelX As Long, CelY As Long, Optional ByVal DontUndo As Boolean = False)

        Dim rtn As Long
        Dim CelEndX As Long, CelEndY As Long
        Dim NextUndo As ViewUndo
        Dim i As Long, j As Long

        On Error GoTo ErrHandler

        'data being pasted is from clipboard (format =bitmap)
        '*'Debug.Assert ViewCBMode = vmBitmap

        If Not Clipboard.GetFormat(vbCFBitmap) Then
          'no data to paste; reset paste command
          '*'Debug.Assert False
          frmMDIMain.mnuEPaste.Enabled = False
          Exit Sub
        End If

        'force tool to 'select'
        SelectedTool = ttSelect
        tlbView.Buttons("select").Value = tbrPressed

        'convert H/W (in 0.01 mm increments) into pixels
        ' x 0.01mm /(25.4 mm per in) * (1440 twips per in) / (twips per pixel) =
        SelHeight = Clipboard.GetData(vbCFBitmap).Height * 0.566929133858268 / ScreenTWIPSX
        SelWidth = Clipboard.GetData(vbCFBitmap).Width * 0.566929133858268 / ScreenTWIPSY

        'limit to 160 x 168
        If SelWidth > 160 Then SelWidth = 160
        If SelHeight > 168 Then SelHeight = 168

        'put data in picClipboard with no scaling
        picClipboard.Cls
        picClipboard.Picture = Clipboard.GetData(vbCFBitmap)

        'convert colors to agi
        For i = 0 To SelWidth - 1
          For j = 0 To SelHeight - 1
            rtn = GetAGIColor(GetPixel(picClipboard.hDC, i, j))
            SetPixelV picClipboard.hDC, i, j, EGAColor(rtn)
          Next j
        Next i

        'copy data at this location to selection pic
        BitBlt picSelection.hDC, 0, 0, SelWidth, SelHeight, picPCel.hDC, CelX, CelY, SRCCOPY
        picSelection.Refresh

        'put clipboard Image at new location on pixel cel
        BitBlt picPCel.hDC, CelX, CelY, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
        picPCel.Refresh
        'copy pixel cel to draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width), CLng(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height), SRCCOPY
        picPCel.Refresh

        'calculate lower right corner values
        CelEndX = CelX + SelWidth - 1
        CelEndY = CelY + SelHeight - 1

        'move selection frames to correct location
        'position the two selection shapes
        With shpView
          .Move CelX * ScaleFactor * 2 - 1, CelY * ScaleFactor - 1
          .Width = SelWidth * ScaleFactor * 2 + 2
          .Height = SelHeight * ScaleFactor + 2
          .BorderStyle = 3
          .Visible = True
        End With

        With shpSurface
          .Move CelX * ScaleFactor * 2 + picCel.Left, CelY * ScaleFactor + picCel.Top
          .Width = shpView.Width
          .Height = shpView.Height
          .BorderStyle = 3
          .Visible = True
        End With
        Timer2.Enabled = True

        'use endselectionmove to add pasted data, and to copy data under selection
        EndMoveSelection CelX, CelY

        If Not DontUndo And Settings.ViewUndo <> 0 Then
          'save undo data
          Set NextUndo = New ViewUndo
          NextUndo.UDAction = udvPasteSelection
          NextUndo.UDLoopNo = SelectedLoop
          NextUndo.UDCelNo = SelectedCel
          NextUndo.ResizeData 3
          NextUndo.UndoData(0) = CelX
          NextUndo.UndoData(1) = CelY
          NextUndo.UndoData(2) = CelEndX
          NextUndo.UndoData(3) = CelEndY
          NextUndo.CelData = DataUnderSel
          'add to undo collection
          AddUndo NextUndo
        End If

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub SelectPropFromList()

        Dim i As Long
        Dim NextUndo As ViewUndo
        Dim NoChange As Boolean

        On Error GoTo ErrHandler

        'create undo object
        Set NextUndo = New ViewUndo

        'save property that was edited
        Select Case lstProperty.Tag
        Case "MLOOP"
          'set undo properties
          NextUndo.UDAction = udvMirror
          NextUndo.UDLoopNo = SelectedLoop
          'determine if a change was made:
          'if choice is to unmirror
          If lstProperty.ListIndex = 0 Then
            'if loop is currently mirrored
            If ViewEdit.Loops(SelectedLoop).Mirrored Then
              'there is a change to make
              NoChange = False
              'get old mirror
              NextUndo.UndoData(0) = ViewEdit.Loops(SelectedLoop).MirrorLoop
            Else
              'no change was made
              NoChange = True
            End If
          Else
            'get new mirror number
            i = Val(Right$(lstProperty.Text, Len(lstProperty.Text) - 4))
            'if mirror is currently mirrored
            If ViewEdit.Loops(SelectedLoop).Mirrored Then
              'if current mirror is same as new mirror
              If ViewEdit.Loops(SelectedLoop).MirrorLoop = i Then
                'no change was made
                NoChange = True
              Else
                'there is a change to make
                NoChange = False
                'get old mirror
                NextUndo.UndoData(0) = ViewEdit.Loops(SelectedLoop).MirrorLoop
              End If
            Else
              'there wasn't a mirror, and there is now- make a change
              NoChange = False
              'get old mirror (there was none)
              NextUndo.UndoData(0) = -1
              'store old loop in undo
              Set NextUndo.UndoLoop = New AGILoop
              NextUndo.UndoLoop.CopyLoop ViewEdit.Loops(SelectedLoop)
            End If
          End If

          'if there is a change to make
          If Not NoChange Then
            If Settings.ViewUndo <> 0 Then
              'add undo
              AddUndo NextUndo
            End If

            'always unmirror the loop first
            ViewEdit.Loops(SelectedLoop).UnMirror
            'if unmirror was NOT the choice
            If lstProperty.ListIndex <> 0 Then
              'set mirror
              ViewEdit.SetMirror SelectedLoop, i
            End If

            'update tree
            UpdateTree
            'select and expand current loop
            tvwView.Nodes("Loop" & CStr(SelectedLoop)).Selected = True
            tvwView.Nodes("Loop" & CStr(SelectedLoop)).EnsureVisible
          End If
        Case "TRANSCOL"
          'if transcolor has changed,
          If lstProperty.ListIndex <> ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor Then
            If Settings.ViewUndo <> 0 Then
              'set undo data
              NextUndo.UDAction = udvChangeTransCol
              NextUndo.UDLoopNo = SelectedLoop
              NextUndo.UDCelNo = SelectedCel
              NextUndo.UndoData(0) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor
              'add undo
              AddUndo NextUndo
            End If
            'change transcolor
            ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor = lstProperty.ListIndex
            'change background to match color
            picCel.BackColor = EGAColor(lstProperty.ListIndex)
            'display
            DisplayCel

            'if displaying preview, update it
            If picPrevCel.Visible Then
              DisplayPrevCel
              picPrevCel.SetFocus
            End If
          End If
        End Select

        'hide listbox
        lstProperty.Visible = False
        'force repaint
        PaintPropertyWindow

        'set focus to palette
        picPalette.SetFocus
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Debug.Print "saveprop error"; Err.Number, Err.Description
        Resume Next
      End Sub

      Private Sub SelectPropFromText()

        Dim tmpVal As Long
        Dim NextUndo As ViewUndo
        Dim NoChange As Boolean

        On Error GoTo ErrHandler

        'create undo object
        Set NextUndo = New ViewUndo

        'save property that was edited
        Select Case txtProperty.Tag
        Case "ID"
          'if text has changed
          If ViewEdit.ID <> txtProperty.Text Then
            'if in a game
            If InGame Then
              On Error Resume Next
              Views(ViewNumber).ID = txtProperty.Text
              'if error
              If Err.Number <> 0 Then
                'invalid name
                MsgBoxEx "Invalid resource ID.", vbOKOnly + vbInformation + vbMsgBoxHelpButton, "Change ID", WinAGIHelp, "htm\winagi\Managing_Resources.htm#resourceids"
                Exit Sub
              End If
              On Error GoTo ErrHandler
            Else
              'cant be zerolength
              If LenB(txtProperty.Text) = 0 Then
                'invalid name
                MsgBoxEx "Invalid resource ID", vbOKOnly + vbInformation + vbMsgBoxHelpButton, "Change ID", WinAGIHelp, "htm\winagi\Managing_Resources.htm#resourceids"
                Exit Sub
              End If
            End If
          End If
          ' never add an undo for change in ID
          NoChange = True

        Case "DESC"
          'if text has changed
          If ViewEdit.Description <> txtProperty.Text Then
            'save new description
            ViewEdit.Description = txtProperty.Text
          End If
          ' never add an undo for change in description
          NoChange = True

        Case "VIEWDESC"
          'if text has changed
          If ViewEdit.ViewDescription <> txtProperty.Text Then
            'set undo properties
            NextUndo.UDAction = udvChangeVDesc
            NextUndo.OldText = ViewEdit.ViewDescription
            'save new view description
            ViewEdit.ViewDescription = Replace(txtProperty.Text, vbNewLine, vbLf)
          Else
            'no change
            NoChange = True
          End If

        Case "WIDTH"
          'validate new width Value
          tmpVal = Val(txtProperty.Text)
          If tmpVal < 1 Then
            tmpVal = 1
          End If
          If tmpVal > MAX_CEL_W Then
            tmpVal = MAX_CEL_W
          End If

          'change width
          ChangeWidth tmpVal
          'set nochange flag to true
          '(ChangeWidth method adds undo object if necessary)
          NoChange = True

        Case "HEIGHT"
          'validate new height Value
          tmpVal = Val(txtProperty.Text)
          If tmpVal < 1 Then
            tmpVal = 1
          End If
          If tmpVal > MAX_CEL_H Then
            tmpVal = MAX_CEL_H
          End If

          'change height
          ChangeHeight tmpVal
          'set nochange flag to true
          '(ChangeHeight method adds undo object if necessary)
          NoChange = True

        End Select
        'if there is a change
        If Not NoChange And Settings.ViewUndo <> 0 Then
          'add undo
          AddUndo NextUndo
        End If

        'hide
        txtProperty.Visible = False
        'force repaint
        PaintPropertyWindow
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub SetEditMenu()
        'sets the menu captions on the View Edit menu
        'based on current mode

        On Error GoTo ErrHandler

        With frmMDIMain
          .mnuEdit.Visible = True
          'always hide redo, find, find again, replace and custom2,3
          .mnuERedo.Visible = False
          .mnuEFind.Visible = False
          .mnuEFindAgain.Visible = False
          .mnuEReplace.Visible = False
          .mnuEBar1.Visible = False
          .mnuECustom3.Enabled = False

          'always show bar0 and copy
          .mnuEBar0.Visible = True
          .mnuECopy.Visible = True
          .mnuECopy.Enabled = True

          'always show bar2 and custom1/2
          .mnuEBar2.Visible = True
          .mnuECustom1.Visible = True
          .mnuECustom1.Enabled = True
          If ShowVEPrev Then
            .mnuECustom1.Caption = "Hide Preview"
          Else
            .mnuECustom1.Caption = "Show Preview"
          End If
          .mnuECustom2.Visible = True
          .mnuECustom2.Enabled = True
          If ShowGrid Then
            .mnuECustom2.Caption = "Hide Drawing Grid"
          Else
            .mnuECustom2.Caption = "Show Drawing Grid"
          End If

          'undo
          .mnuEUndo.Visible = True
          .mnuEUndo.Enabled = UndoCol.Count <> 0
          If UndoCol.Count > 0 Then
            .mnuEUndo.Caption = "&Undo " & LoadResString(VIEWUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & vbTab & "Ctrl+Z"
          Else
            .mnuEUndo.Caption = "&Undo" & vbTab & "Ctrl+Z"
          End If

          'loop/gif export enabled if a loop is selected
          .mnuRCustom1.Enabled = (SelectedLoop >= 0) And (SelectedLoop < ViewEdit.Loops.Count)

          Select Case ViewMode
          Case vmView  'root, or view
            'cut, paste, delete, insert, select all are hidden
            .mnuECut.Visible = False
            .mnuEPaste.Visible = False
            .mnuEDelete.Visible = False
            .mnuEInsert.Visible = False
            .mnuESelectAll.Visible = False

            'set copy caption
            .mnuECopy.Caption = "&Copy View" & vbTab & "Ctrl+C"

            'clear
            .mnuEClear.Visible = True
            .mnuEClear.Enabled = True
            .mnuEClear.Caption = "C&lear View" & vbTab & "Shift+Ins"

          Case vmLoop
            .mnuECut.Visible = True
            .mnuECut.Enabled = SelectedLoop <> ViewEdit.Loops.Count
            .mnuECut.Caption = "Cu&t Loop" & vbTab & "Ctrl+X"

            .mnuECopy.Enabled = .mnuECut.Enabled
            .mnuECopy.Caption = "&Copy Loop" & vbTab & "Ctrl+C"

            .mnuEPaste.Visible = True
            .mnuEPaste.Enabled = (Not (ClipViewLoop Is Nothing) And ViewCBMode = vmLoop)
            .mnuEPaste.Caption = "&Paste Loop" & vbTab & "Ctrl+V"

            .mnuEDelete.Visible = True
            .mnuEDelete.Enabled = .mnuECut.Enabled
            .mnuEDelete.Caption = "&Delete Loop" & vbTab & "Del"

            .mnuEClear.Visible = True
            .mnuEClear.Enabled = .mnuECut.Enabled
            .mnuEClear.Caption = "C&lear Loop" & vbTab & "Shift+Del"

            .mnuEInsert.Visible = True
            .mnuEInsert.Enabled = True
            .mnuEInsert.Caption = "&Insert New Loop" & vbTab & "Shift+Ins"

            .mnuESelectAll.Visible = False

      '      'if a loop is selected, enable custom menu 1
      '      .mnuRCustom1.Enabled = True

          Case vmCel
            'if a selection is visible AND working with celdata
            If shpView.Visible And ActiveControl.Name = "picCel" Then
              'change captions
              .mnuECut.Visible = True
              .mnuECut.Enabled = True
              .mnuECut.Caption = "Cu&t" & vbTab & "Ctrl+X"

              .mnuECopy.Caption = "&Copy" & vbTab & "Ctrl+C"

              .mnuEPaste.Visible = True
              .mnuEPaste.Enabled = Clipboard.GetFormat(vbCFBitmap) And (ViewCBMode = vmBitmap)
              .mnuEPaste.Caption = "&Paste" & vbTab & "Ctrl+V"

              .mnuEDelete.Visible = True
              .mnuEDelete.Enabled = True
              .mnuEDelete.Caption = "&Delete" & vbTab & "Del"

              .mnuEClear.Visible = False

              .mnuEInsert.Visible = False

              .mnuESelectAll.Visible = True
              .mnuESelectAll.Enabled = True
              .mnuESelectAll.Caption = "Select &All" & vbTab & "Ctrl+A"
            Else
              .mnuECut.Visible = True
              .mnuECut.Enabled = SelectedCel <> ViewEdit.Loops(SelectedLoop).Cels.Count
              .mnuECut.Caption = "Cu&t Cel" & vbTab & "Ctrl+X"

              .mnuECopy.Enabled = .mnuECut.Enabled
              .mnuECopy.Caption = "&Copy Cel" & vbTab & "Ctrl+C"

              .mnuEPaste.Visible = True
              Select Case ViewCBMode
              Case vmCel
                'if clipboard contains a cel
                If Not (ClipViewCel Is Nothing) Then
                  .mnuEPaste.Enabled = True
                  .mnuEPaste.Caption = "&Paste Cel" & vbTab & "Ctrl+V"
                End If

              Case vmBitmap
                .mnuEPaste.Enabled = Clipboard.GetFormat(vbCFBitmap)
                .mnuEPaste.Caption = "&Paste" & vbTab & "Ctrl+V"

              Case Else
              End Select

              .mnuEDelete.Visible = True
              .mnuEDelete.Enabled = .mnuECut.Enabled
              .mnuEDelete.Caption = "&Delete Cel" & vbTab & "Del"

              .mnuEClear.Visible = True
              .mnuEClear.Enabled = .mnuECut.Enabled
              .mnuEClear.Caption = "C&lear Cel" & vbTab & "Shift+Del"

              .mnuEInsert.Visible = True
              .mnuEInsert.Enabled = True
              .mnuEInsert.Caption = "&Insert New Cel" & vbTab & "Shift+Ins"

              .mnuESelectAll.Visible = True
              .mnuESelectAll.Enabled = True
              .mnuESelectAll.Caption = "Select &All" & vbTab & "Ctrl+A"
            End If
          End Select

          'set toolbar
          tlbView.Buttons("undo").Enabled = .mnuEUndo.Enabled
          tlbView.Buttons("cut").Enabled = .mnuECut.Enabled
          tlbView.Buttons("copy").Enabled = .mnuECopy.Enabled
          tlbView.Buttons("paste").Enabled = .mnuEPaste.Enabled
          tlbView.Buttons("delete").Enabled = .mnuEDelete.Enabled
          tlbView.Buttons("zoomin").Enabled = (ViewMode = vmCel)
          tlbView.Buttons("zoomout").Enabled = (ViewMode = vmCel)
          tlbView.Buttons("fliph").Enabled = (ViewMode = vmCel)
          tlbView.Buttons("flipv").Enabled = (ViewMode = vmCel)
        End With
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Friend Sub DisplayCel(Optional ResetPosition As Boolean)
        'displays a cel
        'adjusts height/width, and properties

        Dim rtn As Long
        Dim i As Long, prevCol As Long

        On Error GoTo ErrHandler

        'select the cel in the viewtree
        tvwView.Nodes("Cell" & CStr(SelectedLoop) & ":" & CStr(SelectedCel)).Selected = True
        tvwView.Nodes("Cell" & CStr(SelectedLoop) & ":" & CStr(SelectedCel)).EnsureVisible

        'if resetting position,
        If ResetPosition Then
          picCel.Move VE_MARGIN, VE_MARGIN
          'hide selections as well
          Timer2.Enabled = False
          shpView.Visible = False
          shpSurface.Visible = False
        End If

        'use current cel to set its properties on the form
        With ViewEdit.Loops(SelectedLoop).Cels(SelectedCel)
          'set transparent color
          picCel.BackColor = EGAColor(.TransColor)
          'set height/width of edit area
          picCel.Width = CLng(.Width) * 2& * CLng(ScaleFactor)
          picCel.Height = CLng(.Height) * CLng(ScaleFactor)

          'copy cel bitmap into pixel cel
          BitBlt picPCel.hDC, 0, 0, .Width, .Height, .CelBMP, 0, 0, SRCCOPY
          picPCel.Refresh
          'stretch cel bitmap into draw cel
          StretchBlt picCel.hDC, 0&, 0&, CLng(picCel.Width), CLng(picCel.Height), .CelBMP, 0&, 0&, CLng(.Width), CLng(.Height), SRCCOPY
          picCel.Refresh
        End With

        SendMessage picCel.hWnd, WM_SETREDRAW, 0, 0

        If ScaleFactor > 3 And ShowGrid Then
          'draw grid over the cel
          DrawCelGrid
        End If

        SendMessage picCel.hWnd, WM_SETREDRAW, 1, 0
        picCel.Refresh

        'update scrollbars
        SetEScrollbars

        'paint props
        PaintPropertyWindow

        'paint palette
        picPalette.Refresh

        'ensure surface is visible
        picSurface.Visible = True
        'reset mode to cel
        ViewMode = vmCel

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub DrawBoxOnSurface(ByVal CelEndX As Long, ByVal CelEndY As Long, ByVal BoxFill As Boolean)
        'this method only draws the box on the view surface;
        'it does not make any changes to celdata
        'celdata is updated in MouseUp eventwhen user is done drawing

        Dim CelStartX As Long, CelStartY As Long

        ' this is a special case where drawing takes place on the draw surface,
        ' not on the pixel surface; the pixel surface is used to provide the
        ' reset image when the line moves

        'stretch the pixel cel into the draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY

        'convert line to top-bottom/left-right format
        If CelEndY < CelAnchorY Then
          'line is bottom to top;
          'set start to top Value
          CelStartY = CelEndY
          'set end to bottom Value
          CelEndY = CelAnchorY
        Else
          'end is already bottom Value; set start to top Value
          CelStartY = CelAnchorY
        End If
        'same for x direction
        If CelEndX < CelAnchorX Then
          CelStartX = CelEndX
          CelEndX = CelAnchorX
        Else
          CelStartX = CelAnchorX
        End If

        'if filling
        If BoxFill Then
          picCel.Line (CelStartX * ScaleFactor * 2, CelStartY * ScaleFactor)-((CelEndX + 1) * ScaleFactor * 2 - 1, (CelEndY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
        Else
          'draw the four lines separately
          picCel.Line (CelStartX * ScaleFactor * 2, CelStartY * ScaleFactor)-((CelStartX + 1) * ScaleFactor * 2 - 1, (CelEndY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          picCel.Line (CelStartX * ScaleFactor * 2, CelStartY * ScaleFactor)-((CelEndX + 1) * ScaleFactor * 2 - 1, (CelStartY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          picCel.Line (CelStartX * ScaleFactor * 2, CelEndY * ScaleFactor)-((CelEndX + 1) * ScaleFactor * 2 - 1, (CelEndY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          picCel.Line (CelEndX * ScaleFactor * 2, CelStartY * ScaleFactor)-((CelEndX + 1) * ScaleFactor * 2 - 1, (CelEndY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
        End If
        picCel.Refresh

        If ScaleFactor > 3 And ShowGrid Then
          'draw grid over the cel
          DrawCelGrid
        End If
      End Sub

      Private Sub DrawLineOnCel(ByVal CelStartX As Long, ByVal CelStartY As Long, ByVal CelEndX As Long, ByVal CelEndY As Long, NextUndo As ViewUndo, Optional ByVal DontUndo As Boolean = False)

        'draws the line on the cel data

        Dim i As Long, j As Long
        Dim vStep As Double, hStep As Double
        Dim vDir As Long, hDir As Long
        Dim udIndex As Long

        'if not skipping undo
        If Not DontUndo Then
          'number of pixels to save will be 4 plus
          'largest dimension
          If Abs(CelEndY - CelStartY) > Abs(CelEndX - CelStartX) Then
            NextUndo.ResizeData Abs(CelEndY - CelStartY) + 4
          Else
            NextUndo.ResizeData Abs(CelEndX - CelStartX) + 4
          End If
          'save start and end points
          NextUndo.UndoData(0) = CelStartX
          NextUndo.UndoData(1) = CelStartY
          NextUndo.UndoData(2) = CelEndX
          NextUndo.UndoData(3) = CelEndY
        End If
        'set pointer to beginning of celdata
        udIndex = 4

        'if horizontal,
        If (CelEndY = CelStartY) Then
          'draw horizontal line
          hDir = Sgn(CelEndX - CelStartX)
          'check for special case of a single pixel
          If hDir = 0 Then
            hDir = 1
          End If
          For i = CelStartX To CelEndX Step hDir
            'if skipping undo
            If DontUndo Then
              'restore line from undodata
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelStartY) = NextUndo.UndoData(udIndex)
            Else
              'save color
              NextUndo.UndoData(udIndex) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelStartY)
              'set linecolor in celdata
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, CelStartY) = DrawCol
            End If
            SetPixelV picPCel.hDC, i, CelStartY, EGAColor(DrawCol)
            udIndex = udIndex + 1
          Next i
        ElseIf (CelEndX = CelStartX) Then
          'draw vertical line
          vDir = Sgn(CelEndY - CelStartY)
          For j = CelStartY To CelEndY Step vDir
            'if skipping undo
            If DontUndo Then
              'restore line from undodata
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, j) = NextUndo.UndoData(udIndex)
            Else
              'save color
              NextUndo.UndoData(udIndex) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, j)
              'set linecolor in celdata
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CelStartX, j) = DrawCol
            End If
            SetPixelV picPCel.hDC, CelStartX, j, EGAColor(DrawCol)
            udIndex = udIndex + 1
          Next j

        ElseIf Abs(CelEndX - CelStartX) > Abs(CelEndY - CelStartY) Then
          'mostly horizontal line
          'step through x values and increment Y
          vStep = (CelEndY - CelStartY) / (CelEndX - CelStartX)
          hDir = Sgn(CelEndX - CelStartX)
          For i = CelStartX To CelEndX Step hDir
            j = CelStartY + CLng(vStep * (i - CelStartX))
            'if skipping undo
            If DontUndo Then
              'restore line from undodata
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = NextUndo.UndoData(udIndex)
            Else
              'save color
              NextUndo.UndoData(udIndex) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j)
              'set linecolor in celdata
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = DrawCol
            End If
            SetPixelV picPCel.hDC, i, j, EGAColor(DrawCol)
            udIndex = udIndex + 1
          Next i

        Else
          'mostly vertical line
          'step through Y values and increment x
          hStep = (CelEndX - CelStartX) / (CelEndY - CelStartY)
          vDir = Sgn(CelEndY - CelStartY)
          For j = CelStartY To CelEndY Step vDir
            i = CelStartX + CLng(hStep * (j - CelStartY))
            'if skipping undo
            If DontUndo Then
              'restore line from undodata
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = NextUndo.UndoData(udIndex)
            Else
              'save color
              NextUndo.UndoData(udIndex) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j)
              'set linecolor in celdata
              ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(i, j) = DrawCol
            End If
            SetPixelV picPCel.hDC, i, j, EGAColor(DrawCol)
            udIndex = udIndex + 1
          Next j
        End If

        'stretch the pixel cel into the draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY
        picCel.Refresh
        If ShowGrid And ScaleFactor > 3 Then
          DrawCelGrid
        End If
      End Sub


      Private Sub DrawLineOnSurface(ByVal CelEndX As Long, ByVal CelEndY As Long)
        'this method only draws line on the
        'drawing surface; celdata is not modified

        'celdata is updated in MouseUp event after
        'user has completed drawing

        Dim rtn As Long
        Dim CelStartX As Long, CelStartY As Long
        Dim posX As Long, posY As Long
        Dim vStep As Double, hStep As Double
        Dim vDir As Long, hDir As Long

        ' this is a special case where drawing takes place on the draw surface,
        ' not on the pixel surface; the pixel surface is used to provide the
        ' reset image when the line moves

        'stretch the pixel cel into the draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY

        'if horizontal or vertical,
        If (CelEndY = CelAnchorY) Or (CelEndX = CelAnchorX) Then
          'convert line to top-bottom/left-right format
          If CelEndY < CelAnchorY Then
            'line is bottom to top;
            'set start to top Value
            CelStartY = CelEndY
            'set end to bottom Value
            CelEndY = CelAnchorY
          Else
            'end is already bottom Value; set start to top Value
            CelStartY = CelAnchorY
          End If
          If CelEndX < CelAnchorX Then
            CelStartX = CelEndX
            CelEndX = CelAnchorX
          Else
            CelStartX = CelAnchorX
          End If
          'draw line as a single block
          picCel.Line (CelStartX * ScaleFactor * 2, CelStartY * ScaleFactor)-((CelEndX + 1) * ScaleFactor * 2 - 1, (CelEndY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
        Else

          'determine direction of step
          If Abs(CelEndY - CelAnchorY) > Abs(CelEndX - CelAnchorX) Then
            'step through Y values and increment x
            hStep = (CelEndX - CelAnchorX) / (CelEndY - CelAnchorY)
            vDir = Sgn(CelEndY - CelAnchorY)
            For posY = CelAnchorY To CelEndY Step vDir
              posX = CelAnchorX + CLng(hStep * (posY - CelAnchorY))
              picCel.Line (posX * ScaleFactor * 2, posY * ScaleFactor)-((posX + 1) * ScaleFactor * 2 - 1, (posY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
            Next posY
          Else
            'step through x values and increment Y
            vStep = (CelEndY - CelAnchorY) / (CelEndX - CelAnchorX)
            hDir = Sgn(CelEndX - CelAnchorX)
            For posX = CelAnchorX To CelEndX Step hDir
              posY = CelAnchorY + CLng(vStep * (posX - CelAnchorX))
              picCel.Line (posX * ScaleFactor * 2, posY * ScaleFactor)-((posX + 1) * ScaleFactor * 2 - 1, (posY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
            Next posX
          End If
        End If

        picCel.Refresh
        If ShowGrid And ScaleFactor > 3 Then
          DrawCelGrid
        End If

      End Sub

      Private Sub BuildCelData(ByVal CelStartX As Long, ByVal CelStartY As Long, ByVal CelEndX As Long, ByVal CelEndY As Long, Optional NoEvents As Boolean = False)

        Dim CelX As Long, CelY As Long
        Dim tmpData() As AGIColors

        Dim lngColor As Long

        'if no events, skip the wait cursor
        If Not NoEvents Then
          'show wait cursor
          WaitCursor
        End If

        'clip the input box (so data that is off the edge is ignored)
        If CelStartX < 0 Then
          CelStartX = 0
        End If
        If CelStartY < 0 Then
          CelStartY = 0
        End If

        If CelEndX > ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1 Then
          CelEndX = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1
        End If
        If CelEndY > ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1 Then
          CelEndY = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1
        End If

        'copy current cel data
        tmpData = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData

        'rebuild cel data (using tmpData)
        With ViewEdit.Loops(SelectedLoop).Cels(SelectedCel)
          For CelX = CelStartX To CelEndX
            For CelY = CelStartY To CelEndY
              'get color of this pixel
              lngColor = GetPixel(picPCel.hDC, CelX, CelY)
              'convert to AGIcolor and save
              tmpData(CelX, CelY) = GetAGIColor(lngColor)
            Next CelY
          Next CelX
        End With
        'copy data back into cel
        ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData = tmpData

        'reset mouse pointer
        Screen.MousePointer = vbDefault
      End Sub

      Private Sub SelectRegion(ByVal CelEndX As Long, ByVal CelEndY As Long)
        'start and end are in pixel coordinates

        Dim CelStartX As Long, CelStartY As Long
        Dim rtn As Long

        'convert region to top-bottom/left-right format
        'and adjust to be one pixel outside selection area
        If CelEndY < CelAnchorY Then
          'line is bottom to top;
          'set start to top Value
          CelStartY = CelEndY * ScaleFactor - 1
          'set end to bottom Value
          CelEndY = (CelAnchorY + 1) * ScaleFactor + 1
        Else
          'end is already bottom Value; set start to top Value
          CelStartY = CelAnchorY * ScaleFactor - 1
          CelEndY = (CelEndY + 1) * ScaleFactor + 1
        End If
        If CelEndX < CelAnchorX Then
          CelStartX = CelEndX * ScaleFactor * 2 - 1
          CelEndX = (CelAnchorX + 1) * ScaleFactor * 2 + 1
        Else
          CelStartX = CelAnchorX * ScaleFactor * 2 - 1
          CelEndX = (CelEndX + 1) * ScaleFactor * 2 + 1

        End If

        'position the two selection shapes
        With shpView
          .Move CelStartX, CelStartY
          .Width = CelEndX - CelStartX
          .Height = CelEndY - CelStartY
          .BorderStyle = 3
          .Visible = True
        End With

        With shpSurface
          .Move CelStartX + picCel.Left, CelStartY + picCel.Top
          .Width = CelEndX - CelStartX
          .Height = CelEndY - CelStartY
          .BorderStyle = 3
          .Visible = True
        End With

      '  'refresh view edit area
      '  picCel.Refresh
      End Sub


      Private Sub SetEScrollbars()

        On Error GoTo ErrHandler

        'reposition scroll bars
        hsbCel.Top = picPalette.Top - hsbCel.Height
        hsbCel.Width = picSplitV.Left - hsbCel.Left
        vsbCel.Left = picSplitV.Left - vsbCel.Width
        If hsbCel.Top + hsbCel.Height > 10 Then
          vsbCel.Height = hsbCel.Top + hsbCel.Height
        End If

        'determine if scrollbars are needed
        vsbCel.Visible = (picSurface.Top + picCel.Height + 2 * VE_MARGIN > CalcHeight - picPalette.Height)
        'take into account scrollbar width, if visible
        hsbCel.Visible = (picSurface.Left + picCel.Width + 2 * VE_MARGIN > picSplitV.Left + vsbCel.Visible * vsbCel.Width)
        'check again, if scrollbars are visible
        vsbCel.Visible = (picSurface.Top + picCel.Height + 2 * VE_MARGIN > CalcHeight - picPalette.Height + hsbCel.Visible * hsbCel.Height)

        'override scroll bar visibility if picture is not back at starting position
        If picCel.Left <> VE_MARGIN Then
          hsbCel.Visible = True
        End If
        If picCel.Top <> VE_MARGIN Then
          vsbCel.Visible = True
        End If

        'set scroll bar values
        hsbCel.Max = 2 * VE_MARGIN + picCel.Width - (picSplitV.Left - picSurface.Left + vsbCel.Visible * vsbCel.Width)
        vsbCel.Max = 2 * VE_MARGIN + picCel.Height - (CalcHeight - picSurface.Top - picPalette.Height + hsbCel.Visible * hsbCel.Height)

        'if both are visible
        If vsbCel.Visible And hsbCel.Visible Then
          'move scroll bars back
          hsbCel.Width = picSplitV.Left - hsbCel.Left - vsbCel.Width
          If hsbCel.Height < vsbCel.Height Then
            vsbCel.Height = vsbCel.Height - hsbCel.Height
          End If
        End If

        'adjust scroll bar values
        hsbCel.LargeChange = LG_SCROLL * picSurface.Width
        vsbCel.LargeChange = LG_SCROLL * picSurface.Height
        hsbCel.SmallChange = SM_SCROLL * picSurface.Width
        vsbCel.SmallChange = SM_SCROLL * picSurface.Height

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Public Sub UpdateID(ByVal NewID As String, ByVal NewDescription As String)

        On Error GoTo ErrHandler

        If ViewEdit.Description <> NewDescription Then
          'change the viewedit object's description
          ViewEdit.Description = NewDescription
          'if node 0 is selected
          If tvwView.SelectedItem.Index = 1 Then
            'force redraw
            PaintPropertyWindow
          End If
        End If

        If ViewEdit.ID <> NewID Then
          'change the viewedit object's id and caption
          ViewEdit.ID = NewID

          'if viewedit is changed
          If Asc(Caption) = 42 Then
            Caption = sDM & SVIEWED & ResourceName(ViewEdit, InGame, True)
          Else
            Caption = SVIEWED & ResourceName(ViewEdit, InGame, True)
          End If

          'change root node of view list
          tvwView.Nodes(1).Text = ResourceName(ViewEdit, InGame, True)
          'if node 1 is selected
          If tvwView.SelectedItem.Index = 1 Then
            'force redraw
            PaintPropertyWindow
          End If
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub UpdateViewDesc()

        'used to ensure view description is updated, if it gets changed outside of the editor

        'if the view node is selected
        If tvwView.SelectedItem.Parent Is Nothing Then
          PaintPropertyWindow
        End If
      End Sub

      Friend Function UpdateTree() As Boolean
        'loads the tree view with the view structure

        Dim i As Long, j As Long

        On Error GoTo ErrHandler

        With tvwView
          'clear the tree
            .Nodes.Clear

          'add view
          .Nodes.Add , tvwFirst, "Root", ResourceName(ViewEdit, InGame, True), 1, 1

          'step through loops

          For i = 0 To ViewEdit.Loops.Count - 1
            'add loop
            .Nodes.Add "Root", tvwChild, "Loop" & CStr(i), "Loop " & CStr(i), 2
            For j = 0 To ViewEdit.Loops(i).Cels.Count - 1
            'add cels
              .Nodes.Add "Loop" & CStr(i), tvwChild, "Cell" & CStr(i) & ":" & CStr(j), "Cel " & CStr(j), 3, 3
            Next j
            .Nodes.Add "Loop" & CStr(i), tvwChild, , "End"
          Next i
          .Nodes.Add "Root", tvwChild, , "End"
        End With

        UpdateTree = True
      Exit Function

      ErrHandler:
        'error- let calling method deal with it
        'by returning false
        '*'Debug.Assert False
      End Function


      Public Sub ClearCel()
        Dim NextUndo As ViewUndo

        If Settings.ViewUndo <> 0 Then
          Set NextUndo = New ViewUndo
          NextUndo.UDAction = udvClearCel
          NextUndo.UDLoopNo = SelectedLoop
          NextUndo.UDCelNo = SelectedCel
          Set NextUndo.UndoCel = New AGICel
          NextUndo.UndoCel.CopyCel ViewEdit.Loops(SelectedLoop).Cels(SelectedCel)
          'add to undo collection
          AddUndo NextUndo
        End If

        'clears the selected cel
        ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Clear

        'force update
        DisplayCel True
      End Sub


      Public Sub InsertCel(ByVal InsertPos As Byte)

        'adds a cel to selected loop
        Dim CelHeight As Byte, CelWidth As Byte
        Dim NextUndo As ViewUndo

        On Error GoTo ErrHandler

        'if adding to end (selected cel will be celcount)
        If InsertPos = ViewEdit.Loops(SelectedLoop).Cels.Count Then
          'use same height and width as last cel
          CelHeight = ViewEdit.Loops(SelectedLoop).Cels(InsertPos - 1).Height
          CelWidth = ViewEdit.Loops(SelectedLoop).Cels(InsertPos - 1).Width
        Else
          'use default height/width
          CelHeight = Settings.DefCelH
          CelWidth = Settings.DefCelW
        End If

        'if too many cels
        If ViewEdit.Loops(SelectedLoop).Cels.Count = MAXCELS Then
          MsgBoxEx "Can't exceed " & CStr(MAXCELS) & " cels per loop.", vbInformation + vbMsgBoxHelpButton, "Can't Insert Cel", WinAGIHelp, "htm\agi\views.htm#cel"
          Exit Sub
        End If

        'add cel
        ViewEdit.Loops(SelectedLoop).Cels.Add InsertPos, CelWidth, CelHeight, agBlack

        'if undo enabled
        If Settings.ViewUndo <> 0 Then
          Set NextUndo = New ViewUndo
          NextUndo.UDAction = udvAddCel
          NextUndo.UDCelNo = SelectedCel
          NextUndo.UDLoopNo = SelectedLoop
          AddUndo NextUndo
        End If

        'update tree
        UpdateTree

        'display selected cel in the view tree
        SelectedCel = InsertPos
        'force update
        DisplayCel True
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Sub ClearLoop()
        'deletes all cels in selected loop
        'except one
        'and sets the remaining cel to one by one with black transcolor

        Dim i As Long, j As Long
        Dim NextUndo As ViewUndo

        'only if selected loop is valid
        If SelectedLoop < 0 Then
          Exit Sub
        End If

        If Settings.ViewUndo <> 0 Then
          'build undo object
          Set NextUndo = New ViewUndo
          NextUndo.UDAction = udvClearLoop
          NextUndo.UDLoopNo = SelectedLoop
          Set NextUndo.UndoLoop = New AGILoop
          NextUndo.UndoLoop.CopyLoop ViewEdit.Loops(SelectedLoop)
          'if mirrored
          If ViewEdit.Loops(SelectedLoop).Mirrored Then
            'use first data point as flag
            NextUndo.UndoData(0) = True
            'use cel for mirror loop
            NextUndo.UDCelNo = ViewEdit.Loops(SelectedLoop).MirrorLoop
          End If
          'add to undocol
          AddUndo NextUndo
        End If

        'if there are more 2 or more cels
        If ViewEdit.Loops(SelectedLoop).Cels.Count >= 2 Then
          'delete 'em
          j = ViewEdit.Loops(SelectedLoop).Cels.Count - 1
          For i = j To 1 Step -1
            ViewEdit.Loops(SelectedLoop).Cels.Remove i
          Next i
        End If

        'clear the remaining cel
        ViewEdit.Loops(SelectedLoop).Cels(0).Clear

        'update tree
        UpdateTree
        'select remaining cel
        SelectedCel = 0
        'redisplay
        DisplayCel True
      End Sub


      Public Sub DeleteCel(ByVal CutCel As Boolean, Optional ByVal DontUndo As Boolean = False)
        'removes the selected cel

        'NOTE: it is assumed that the caller of the function
        'has validated that selectedloop and selectedcel are valid

        Dim tmpLoop As Byte
        Dim NextUndo As ViewUndo
        Dim tmpCel As Long

        On Error GoTo ErrHandler

        'if not a valid cel
        If SelectedCel < 0 Then
          Exit Sub
        End If

        'if this is the last cel in loop
        If ViewEdit.Loops(SelectedLoop).Cels.Count = 1 Then
          MsgBox "Loop must contain at least one cel.", vbInformation, "Can't " & IIf(CutCel, "cut", "delete") & " cel"
          Exit Sub
        End If

        'save number of cel being deleted
        tmpCel = SelectedCel

        'if cutting
        If CutCel Then
          'set clipboard cel to this cel
          Set ClipViewCel = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel)
        End If

        'if not skipping undo
        If Not DontUndo And Settings.ViewUndo <> 0 Then
          Set NextUndo = New ViewUndo
          If CutCel Then
            NextUndo.UDAction = udvCutCel
          Else
            NextUndo.UDAction = udvDelCel
          End If
          NextUndo.UDLoopNo = SelectedLoop
          NextUndo.UDCelNo = SelectedCel
          Set NextUndo.UndoCel = New AGICel
          NextUndo.UndoCel.CopyCel ViewEdit.Loops(SelectedLoop).Cels(SelectedCel)
          AddUndo NextUndo
        End If
        'now delete the cel
        ViewEdit.Loops(SelectedLoop).Cels.Remove SelectedCel

        'update tree
        UpdateTree

        'select the cel that's now at same number
        SelectedCel = tmpCel

        'if the cel deleted was the last cel in loop
        If tmpCel = ViewEdit.Loops(SelectedLoop).Cels.Count Then
          'back up one
          SelectedCel = SelectedCel - 1
        End If

        'force update
        DisplayCel True
      Exit Sub

      ErrHandler:
        Debug.Assert False
        Resume Next
      End Sub

      Public Sub InsertLoop(ByVal Pos As Byte)
        'adds a loop to view
        'added loop will have a single cel
        'with default height and width and black transcolor

        Dim i As Long
        Dim NextUndo As ViewUndo

        On Error GoTo ErrHandler

        'if error
        If ViewEdit.Loops.Count = MAXLOOPS Then
          MsgBoxEx "Can't exceed " & CStr(MAXLOOPS) & " loops per view.", vbInformation + vbMsgBoxHelpButton, "Can't Insert Loop", WinAGIHelp, "htm\agi\views.htm#loop"
          Exit Sub
        End If
        'add the new loop
        ViewEdit.Loops.Add Pos
        'add a blank cel to the loop
        ViewEdit.Loops(Pos).Cels.Add 255, Settings.DefCelW, Settings.DefCelH, agBlack

        'if not skipping undo
        If Settings.ViewUndo <> 0 Then
          Set NextUndo = New ViewUndo
          NextUndo.UDAction = udvAddLoop
          NextUndo.UDLoopNo = Pos
          AddUndo NextUndo
        End If

        'update tree
        UpdateTree

        'display selected cel in viewtree
        SelectedLoop = Pos
        SelectedCel = 0

        'force update
        DisplayCel True

        'update preview
        If ShowVEPrev Then
          DisplayPrevLoop
          DisplayPrevCel
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Public Sub ZoomCel(ByVal ZoomDir As Long)
        'enlarges or reduces the cel Image

        Dim rtn As Long
        Dim OldZoom As Long, OldW As Long, OldH As Long

        On Error GoTo ErrHandler

        'if not displaying a cel
        '*'Debug.Assert ViewMode = vmCel

        'zero means zoomout
        If ZoomDir = 0 Then
          'if alreay at min
          If ScaleFactor = 1 Then
            Exit Sub
          End If
          OldZoom = ScaleFactor
          ScaleFactor = ScaleFactor - 1
        Else
          'if already at Max
          If ScaleFactor = 15 Then
            Exit Sub
          End If
          OldZoom = ScaleFactor
          ScaleFactor = ScaleFactor + 1
        End If

        '*'Debug.Assert frmMDIMain.ActiveMdiChild Is Me
        'update statusbar display
        MainStatusBar.Panels("Scale").Text = "Scale: " & CStr(ScaleFactor)

        'force update by calling displaycel
        DisplayCel

        'adjust sizes
        shpView.Width = ((shpView.Width - 2) / OldZoom * ScaleFactor) + 2
        shpView.Height = ((shpView.Height - 2) / OldZoom * ScaleFactor) + 2
        'and starting position
        shpView.Move (shpView.Left + 1) / OldZoom * ScaleFactor - 1, (shpView.Top + 1) / OldZoom * ScaleFactor - 1
        shpSurface.Move shpView.Left + picCel.Left, shpView.Top + picCel.Top
        shpSurface.Width = shpView.Width
        shpSurface.Height = shpView.Height

        'if current tool is erase
        If SelectedTool = ttErase Then
          'load correct cursor
          picCel.MouseIcon = LoadResPicture("EVC_ERASE" & CStr(ScaleFactor), vbResCursor)
        End If

        'repaint the background
        picSurface.Cls
        picSurface_Paint

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub ZoomPrev(ByVal Dir As Long)

        Dim mW As Long, mH As Long

        If Dir = 1 Then
          ViewScale = ViewScale + 1
          If ViewScale = 13 Then
            ViewScale = 12
            Exit Sub
          End If
        Else
          ViewScale = ViewScale - 1
          If ViewScale = 0 Then
            ViewScale = 1
            Exit Sub
          End If
        End If

        'get current maxH and maxW (by de-calculating...)
        mW = picPrevCel.Width / ViewScale / 2
        mH = picPrevCel.Height / ViewScale

        'rezize cel
        picPrevCel.Width = mW * 2 * ViewScale
        picPrevCel.Height = mH * ViewScale

        'set scrollbars
        SetPScrollbars

        'force redraw
        DisplayPrevLoop
        DisplayPrevCel

      End Sub

      Private Sub cmbMotion_GotFocus()

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub

      Private Sub Form_Activate()

        Dim blnInGame As Boolean

        On Error GoTo ErrHandler

        'if minimized, exit
        '(to deal with occasional glitch causing focus to lock up)
        If Me.WindowState = vbMinimized Then
          Exit Sub
        End If

        'if hiding previewwin on lost focus, hide it now
        If Settings.HidePreview Then
          If Not Me.Visible Then
            Me.Visible = True
          End If
          PreviewWin.Hide
        End If

        'if visible,
        If Visible Then
          'force resize
          Form_Resize
        End If

        'if findform is visible,
        If FindForm.Visible Then
          'hide it it
          FindForm.Visible = False
        End If

        blnInGame = InGame

        AdjustMenus AGIResType.View, blnInGame, True, IsChanged
       'force update of statusbar
        MainStatusBar.Panels("Scale").Text = "Scale: " & CStr(ScaleFactor)
        MainStatusBar.Panels("Tool") = LoadResString(VIEWTOOLTYPETEXT + SelectedTool)
        MainStatusBar.Panels("CurX").Text = "X: 0"
        MainStatusBar.Panels("CurY").Text = "Y: 0"

        If Visible Then
          SetEditMenu
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Private Sub Form_Click()

        'if properties text box is visible
        If txtProperty.Visible Then
          'move focus to palette
          picPalette.SetFocus
        End If

      End Sub

      Private Sub Form_GotFocus()

        Dim blnInGame As Boolean

        On Error GoTo ErrHandler

        blnInGame = InGame
        AdjustMenus AGIResType.View, blnInGame, True, IsChanged
        SetEditMenu

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

        On Error GoTo ErrHandler
        'detect and respond to keyboard shortcuts

        'always check for help first
        If Shift = 0 And KeyCode = vbKeyF1 Then
          MenuClickHelp
          KeyCode = 0
          Exit Sub
        End If

        'if property is being edited,
        If ActiveControl Is txtProperty Or ActiveControl Is lstProperty Then
          Exit Sub
        End If

        'if anything but property box has focus, check for loop/cel navigation
        If Not ActiveControl Is picProperties Then
          CheckNavigationKeys KeyCode, Shift
          If KeyCode = 0 Then
            Exit Sub
          End If
        End If

        'check for global shortcut keys
        CheckShortcuts KeyCode, Shift
        If KeyCode = 0 Then
          Exit Sub
        End If

        'check other keypresses
        Select Case Shift
        Case vbCtrlMask
          Select Case KeyCode
          Case vbKeyA
            If frmMDIMain.mnuESelectAll.Enabled Then
              MenuClickSelectAll
              KeyCode = 0
            End If

          Case vbKeyZ
            'undo
            If frmMDIMain.mnuEUndo.Enabled Then
              MenuClickUndo
              KeyCode = 0
            End If

          Case vbKeyX
            If frmMDIMain.mnuECut.Enabled Then
              MenuClickCut
              KeyCode = 0
            End If

          Case vbKeyC
            If frmMDIMain.mnuECopy.Enabled Then
              MenuClickCopy
              KeyCode = 0
            End If

          Case vbKeyV
            If frmMDIMain.mnuEPaste.Enabled Then
              MenuClickPaste
              KeyCode = 0
            End If
          End Select

        Case 0 'no shift, ctrl, alt
          Select Case KeyCode
          Case vbKeyDelete
            If frmMDIMain.mnuEDelete.Enabled Then
              MenuClickDelete
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
        End Select
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub Form_KeyPress(KeyAscii As Integer)


        'if editing a propeety, allow txtbox to handle key input
        If txtProperty.Visible Then
          Exit Sub
        End If

        'respond to keypresses that change scale of the preview window
        If ShowVEPrev Then
          KeyHandler KeyAscii
        End If
      End Sub

      Private Sub Form_Load()

        On Error GoTo ErrHandler

        'subclass the form for mouse scrolling
        PrevVEWndProc = SetWindowLong(Me.hWnd, GWL_WNDPROC, AddressOf ScrollWndProc)
        ' the combo box doesn't play nice; the form mousewheel actions don't get capture
        ' when mouse is over the combobox - so we subclass it separately
        PrevCBWndProc = SetWindowLong(Me.cmbMotion.hWnd, GWL_WNDPROC, AddressOf ScrollWndProc)

        CalcWidth = MIN_WIDTH
        CalcHeight = MIN_HEIGHT

        'set undo collection
        Set UndoCol = New Collection

        ShowVEPrev = Settings.ShowVEPrev
        ShowGrid = Settings.ShowGrid

        'always force update through activate event
        Form_Activate

        'set default colors
        LeftColor = Settings.DefVColor1
        RightColor = Settings.DefVColor2

        'set initial scale Value
        ScaleFactor = Settings.ViewScale.Edit

        'set initial mode
        ViewMode = vmCel

        MainStatusBar.Panels("Scale").Text = "Scale: " & CStr(ScaleFactor)
        'adjust scroll bar values
        hsbCel.LargeChange = LG_SCROLL * picSurface.Width
        vsbCel.LargeChange = LG_SCROLL * picSurface.Height
        hsbCel.SmallChange = SM_SCROLL * picSurface.Width
        vsbCel.SmallChange = SM_SCROLL * picSurface.Height

        'start with selection tool
        SelectedTool = ttSelect
        tlbView.Buttons("select").Value = tbrPressed
        MainStatusBar.Panels("Tool") = LoadResString(VIEWTOOLTYPETEXT + SelectedTool)
        picCel.MousePointer = vbCustom
        picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

        picPalette.Height = 32
        'adjust properties window to width of treeview
        picProperties.Width = tvwView.Left + tvwView.Width
        picProperties.Height = PROP_ROW_HEIGHT * 4
        picProperties.Top = picPalette.Top - picProperties.Height

        'initialize preview elements
        'minimum split by default
        picSplitV.Left = ScaleWidth - MIN_SPLIT_V
        picPrevFrame.Left = picSplitV.Left + picSplitV.Width
        picPrevFrame.Width = ScaleWidth - picPrevFrame.Left

        'get default scale values
        ViewScale = Settings.ViewScale.Preview

        'set default view alignment
        lngHAlign = Settings.ViewAlignH
        lngVAlign = Settings.ViewAlignV

        'set view scrollbar values
        hsbPrevCel.LargeChange = LG_SCROLL * picPrevSurface.Width
        vsbPrevCel.LargeChange = LG_SCROLL * picPrevSurface.Height
        hsbPrevCel.SmallChange = SM_SCROLL * picPrevSurface.Width
        vsbPrevCel.SmallChange = SM_SCROLL * picPrevSurface.Height

        VTopMargin = 50
        lngVAlign = 2
        tlbPreview.Buttons("VAlign").Image = 8

        cmbMotion.ListIndex = 0
        sldSpeed.Value = 5
        hsbPrevCel.Min = -VE_MARGIN
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub Form_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        On Error GoTo ErrHandler

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        'if right button
        If Button = vbRightButton Then
          'reset edit menu first
          SetEditMenu
          'need doevents so form activation occurs BEFORE popup
          'otherwise, errors will be generated because of menu
          'adjustments that are made in the form_activate event
          SafeDoEvents
          'make sure this form is the active form
          If Not (frmMDIMain.ActiveMdiChild Is Me) Then
            'set focus before showing the menu
            Me.SetFocus
          End If
          'show edit menu
          PopupMenu frmMDIMain.mnuEdit, , X, Y
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)

        'check if save is necessary (and set cancel if user cancels)
        Cancel = Not AskClose
      End Sub
      Private Sub Form_Resize()

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

        'if property editors or preview toolbars are visible, hide them
        If lstProperty.Visible Then
          lstProperty.Visible = False
        End If
        If txtProperty.Visible Then
          txtProperty.Visible = False
        End If
        If tlbHAlign.Visible Then
          tlbHAlign.Visible = False
        End If
        If tlbVAlign.Visible Then
          tlbVAlign.Visible = False
        End If

        'reset visible flag to force
        'palette to resize correctly......
        picPalette.Visible = False
        picPalette.Visible = True

        'show/hide preview window
        Me.picSurface.Width = 1118
        picPrevFrame.Visible = ShowVEPrev
      '  '*'Debug.Assert ShowVEPrev
        'adjust other elements
        picSplitV.Height = CalcHeight - picPalette.Height
        If ShowVEPrev Then
          If CalcWidth - picPrevFrame.Width - picSplitV.Width > 300 Then
            picSplitV.Left = CalcWidth - picPrevFrame.Width - picSplitV.Width
          Else
           picSplitV.Left = 300
          End If
          picPrevFrame.Height = picSplitV.Height
          picPrevFrame.Left = picSplitV.Left + picSplitV.Width
        Else
          picSplitV.Left = CalcWidth - picSplitV.Width
          'also make sure to stop timer, if it's going
          tmrMotion.Enabled = False
        End If

        SetEScrollbars

        'adjust drawing surface
        If hsbCel.Top - (Not hsbCel.Visible) * hsbCel.Height - picSurface.Top > 10 Then
          picSurface.Height = hsbCel.Top - (Not hsbCel.Visible) * hsbCel.Height - picSurface.Top
        End If
        picSurface.Width = picSplitV.Left - picSurface.Left + (vsbCel.Visible) * vsbCel.Width

        'adjust properties box and viewtree
        picProperties.Top = CalcHeight - picPalette.Height - picProperties.Height - 2
        tvwView.Height = picProperties.Top - 4

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub Form_Unload(Cancel As Integer)

        Dim i As Long

        On Error GoTo ErrHandler

        'if unloading due to error on startup
        'viewedit will be set to nothing
        If Not ViewEdit Is Nothing Then
          'dereference view
          ViewEdit.Unload
          Set ViewEdit = Nothing
        End If

        'remove from vieweditor collection
        For i = 1 To ViewEditors.Count
          If ViewEditors(i) Is Me Then
            ViewEditors.Remove i
            Exit For
          End If
        Next i

        'destroy undocol
        If UndoCol.Count > 0 Then
          For i = UndoCol.Count To 1 Step -1
            UndoCol.Remove i
          Next i
        End If
        Set UndoCol = Nothing

        'release subclass hook for form
        SetWindowLong Me.hWnd, GWL_WNDPROC, PrevVEWndProc
        ' and also for the combo box
        SetWindowLong Me.cmbMotion.hWnd, GWL_WNDPROC, PrevCBWndProc

        'need to check if this is last form
        LastForm Me
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub hsbCel_Change()

        'position viewholder
        picCel.Left = -hsbCel.Value
        If picCel.Left > VE_MARGIN Then
          picCel.Left = VE_MARGIN
        End If

        'if selection shape is visible
        If shpView.Visible Then
          'adjust location of shpView
          shpSurface.Left = shpView.Left + picCel.Left
        End If

        If Me.Visible Then
          'force resize
          Form_Resize
          picPalette.SetFocus
        End If
      End Sub

      Private Sub hsbCel_GotFocus()

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        'force focus back to palette
        picPalette.SetFocus
      End Sub

      Private Sub hsbCel_Scroll()

        hsbCel_Change
      End Sub

      Private Sub lstProperty_DblClick()

        'select this new prop value
        SelectPropFromList
      End Sub

      Private Sub lstProperty_GotFocus()

        picProperties.Refresh
      End Sub

      Private Sub lstProperty_KeyPress(KeyAscii As Integer)

        'escape = cancel
        'return = select

        On Error GoTo ErrHandler

        Select Case KeyAscii
        Case vbKeyReturn
          'select this property item
          SelectPropFromList

        Case vbKeyEscape
          'just cancel it
          lstProperty.Visible = False

        End Select
      Exit Sub

      ErrHandler:
        Debug.Assert False
        Resume Next
      End Sub


      Private Sub lstProperty_LostFocus()

        On Error GoTo ErrHandler

        'ensure it's not visible
        If lstProperty.Visible Then
          lstProperty.Visible = False
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Debug.Print "list lost focus error"; Err.Number, Err.Description
        Resume Next
      End Sub

      Private Sub picPalette_GotFocus()

        'unhighlight any selected property
        PaintPropertyWindow False

      End Sub

      Private Sub picPalette_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim bytNewCol As Byte, NextUndo As ViewUndo
        Dim dblWidth As Double

        On Error GoTo ErrHandler

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        dblWidth = picPalette.Width / 8

        'determine color from x,Y position
        bytNewCol = 8 * (Y \ 17) + (X \ dblWidth)

        Select Case Button
        Case vbLeftButton
          Select Case Shift
          Case 0 ' no shift keys
            LeftColor = bytNewCol
          Case vbCtrlMask
            'transcolor- if a cel is selected
            If SelectedLoop >= 0 And SelectedCel >= 0 And tvwView.SelectedItem.Text <> "End" Then
              'change trans color
              If bytNewCol <> ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor Then
                If Settings.ViewUndo <> 0 Then
                  'set undo data
                  Set NextUndo = New ViewUndo
                  NextUndo.UDAction = udvChangeTransCol
                  NextUndo.UDLoopNo = SelectedLoop
                  NextUndo.UDCelNo = SelectedCel
                  NextUndo.UndoData(0) = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor
                  'add undo
                  AddUndo NextUndo
                End If
                'change transcolor
                ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor = bytNewCol
                'change background to match color
                picCel.BackColor = EGAColor(bytNewCol)
                'display
                DisplayCel
              End If
            End If
          End Select
        Case vbRightButton
          RightColor = bytNewCol
        End Select

        picPalette.Refresh

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub picPalette_Paint()
        'draw the coice of colors into the palette box

        Dim i As Integer, j As Integer
        Dim dblWidth As Double, lngTransCol As Long

        On Error GoTo ErrHandler

        dblWidth = picPalette.Width / 8

        For i = 0 To 1
          For j = 0 To 7
            picPalette.Line (j * dblWidth, i * 17)-((j + 1) * dblWidth, i * 17 + 16), EGAColor(i * 8 + j), BF
          Next j
        Next i

        'add 'R' and 'L' for current mouse button colors
        If LeftColor > 9 Then
          picPalette.ForeColor = vbBlack
        Else
          picPalette.ForeColor = vbWhite
        End If
        picPalette.CurrentX = dblWidth * (LeftColor Mod 8) + 3
        picPalette.CurrentY = 17 * (LeftColor \ 8) + 1
        picPalette.Print "L"

        If RightColor > 9 Then
          picPalette.ForeColor = vbBlack
        Else
          picPalette.ForeColor = vbWhite
        End If
        picPalette.CurrentX = dblWidth * ((RightColor Mod 8) + 1) - 13
        picPalette.CurrentY = 17 * (RightColor \ 8) + 1
        picPalette.Print "R"

        ' add a 'T' in center of current transparency color

        ' if a cel is selected, use it's transcolor value
        If SelectedLoop >= 0 And SelectedCel >= 0 Then
          ' make sure it's not an end-of-loop or end-of-cel
          If tvwView.SelectedItem.Text = "End" Then
            Exit Sub
          End If
          lngTransCol = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor
        Else
        ' otherwise, don't draw it
          Exit Sub
        End If

        If lngTransCol > 9 Then
          picPalette.ForeColor = vbBlack
        Else
          picPalette.ForeColor = vbWhite
        End If
        picPalette.CurrentX = dblWidth * (lngTransCol Mod 8) + dblWidth / 2 - picPalette.TextWidth("T") / 2 - 2
        picPalette.CurrentY = 17 * (lngTransCol \ 8) + 1
        picPalette.Print "T"

      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub picPrevCel_DblClick()

        'let user change background color
        Load frmPalette
        frmPalette.SetForm 1
        frmPalette.Show vbModal, frmMDIMain
        picPrevCel.BackColor = PrevWinBColor
        picPrevSurface.BackColor = PrevWinBColor
        picPrevFrame.BackColor = PrevWinBColor
        'toolbars stay default gray, but that's OK

        'force redraw of cel
        If blnTrans Then
          DrawTransGrid
        End If
        DisplayPrevCel
      End Sub

      Private Sub picPrevFrame_DblClick()

        picPrevCel_DblClick
      End Sub

      Private Sub picPrevFrame_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub

      Private Sub picPrevFrame_Resize()

        Dim rtn As Long, offset As Long

        On Error GoTo ErrHandler

        'if not visible, don't do anything
        If Not Visible Then
          Exit Sub
        End If

        'if not minimized
        If Me.WindowState <> vbMinimized Then
          DontDraw = True

          picPrevSurface.Height = picPrevFrame.ScaleHeight - picPrevSurface.Top - fraVMotion.Height - hsbPrevCel.Height
          picPrevSurface.Width = picPrevFrame.ScaleWidth - vsbPrevCel.Width

          'position/size scrollbars
          vsbPrevCel.Left = picPrevSurface.Width
          vsbPrevCel.Height = picPrevSurface.Height
          vsbPrevCel.Top = picPrevSurface.Top

          hsbPrevCel.Top = picPrevSurface.Top + picPrevSurface.Height
          hsbPrevCel.Width = picPrevSurface.Width

          'position motion frame
          fraVMotion.Top = picPrevFrame.ScaleHeight - fraVMotion.Height

          SetPScrollbars

          If blnTrans Then
            DrawTransGrid
          End If

          DontDraw = False
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next

      End Sub

      Private Sub picPrevSurface_DblClick()

        picPrevCel_DblClick
      End Sub

      Private Sub picPrevSurface_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub

      Private Sub picProperties_DblClick()

        'no properties can be toggled, so no need to trap
        'dblclick event; just call mouse down again, and set x Value so
        'mouse down appears to be over the dropdown arrow
        picProperties_MouseDown 0, 0, picProperties.Width - 1, MouseY
      End Sub

      Private Sub picProperties_KeyDown(KeyCode As Integer, Shift As Integer)

        If Shift = 0 Then
          Select Case KeyCode
          Case vbKeyUp
            If SelectedProp <> 1 Then
              SelectedProp = SelectedProp - 1
              PaintPropertyWindow
            End If

          Case vbKeyDown
            If SelectedProp <> 3 Then
              SelectedProp = SelectedProp + 1
              'loops only have two properties
              If Asc(tvwView.SelectedItem.Text) = 76 Then
                If SelectedProp = 3 Then
                  SelectedProp = 2
                  Exit Sub
                End If
              End If
              PaintPropertyWindow
            End If

          Case vbKeyReturn

          End Select
        End If

      End Sub

      Private Sub picProperties_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim tmpProp As Long
        Dim rtn As Long

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        'savey in case of double click
        MouseY = Y

        'if nothing selected
        If tvwView.SelectedItem Is Nothing Then
          Exit Sub
        End If
        'if current node is 'end'
        If tvwView.SelectedItem.Text = "End" Then
          Exit Sub
        End If

        'if textbox is visible or listbox is visible
        If txtProperty.Visible Or lstProperty.Visible Then
          'exit for now;
          Exit Sub
        End If

        'calculate property number
        tmpProp = Y \ PROP_ROW_HEIGHT

        'validate prop
        If tmpProp <= 0 Then
          Exit Sub
        End If
        If tmpProp > 3 Then
          tmpProp = 3
        End If
        If (tmpProp = 3) And (Asc(tvwView.SelectedItem.Text) = 76) Then
          'loops only have two props
          If tmpProp = 3 Then
            Exit Sub
          End If
        End If

        'if property selected was clicked
        If tmpProp = SelectedProp Then

          'if this is root node,
          If tvwView.SelectedItem.Parent Is Nothing Then
            Select Case SelectedProp
            Case 1, 2 'ID, description
              'id only enabled if in a game
              If SelectedProp = 1 And Not InGame Then
                Exit Sub
              End If

              'if button clicked
              If X > picProperties.Width - 17 Then
                'copy pressed dropdlg picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, SelectedProp * PROP_ROW_HEIGHT, 17, 17, DropDlgDC, 18, 0, SRCCOPY)
                'call edit id/desc
                MenuClickDescription SelectedProp
                'reset dropdlg button
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, SelectedProp * PROP_ROW_HEIGHT, 17, 17, DropDlgDC, 0, 0, SRCCOPY)
              End If

            Case 3  'ViewDesc
              'if button clicked
              If X > picProperties.Width - 17 Then
                'copy pressed dropover picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, SelectedProp * PROP_ROW_HEIGHT, 17, 17, DropOverDC, 18, 0, SRCCOPY)
                'display edit box
                DisplayPropertyEditBox picProperties.Width, 0, CalcWidth - picProperties.Width, PROP_ROW_HEIGHT * 4, "VIEWDESC"
              End If
            End Select

          'if selected item is a loop
          ElseIf tvwView.SelectedItem.Parent.Parent Is Nothing Then
            Select Case SelectedProp
            Case 1 'mirrored
              'read only; click on loop to change mirror status
            Case 2  'mirrorloop
              DisplayPropertyListBox PropSplitLoc, 3 * PROP_ROW_HEIGHT, picProperties.Width - PropSplitLoc, PROP_ROW_HEIGHT * 3, "MLOOP"
            End Select

          'must be a cel
          Else
            Select Case SelectedProp
            Case 1 'WIDTH
              DisplayPropertyEditBox PropSplitLoc + 3, PROP_ROW_HEIGHT + 1, picProperties.Width - PropSplitLoc - 7, PROP_ROW_HEIGHT - 2, "WIDTH"
            Case 2  ' HEIGHT
              DisplayPropertyEditBox PropSplitLoc + 3, 2 * PROP_ROW_HEIGHT + 1, picProperties.Width - PropSplitLoc - 7, PROP_ROW_HEIGHT - 2, "HEIGHT"
            Case 3  'transcolor
              If X > picProperties.Width - 17 Then
                'copy pressed dropdown picture
                rtn = BitBlt(picProperties.hDC, picProperties.Width - 17, SelectedProp * PROP_ROW_HEIGHT, 17, 17, DropDownDC, 18, 0, SRCCOPY)
                'display list box
                DisplayPropertyListBox PropSplitLoc, -1 * PROP_ROW_HEIGHT, picProperties.Width - PropSplitLoc, PROP_ROW_HEIGHT * 4, "TRANSCOL"
              End If
            End Select
          End If

        'if not the same as current prop
        Else
          'if changed
          If tmpProp <> SelectedProp Then
            SelectedProp = tmpProp
            PaintPropertyWindow
          End If
        End If
      End Sub

      Private Sub picProperties_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)
        'cache the Y Value
        MouseY = Y
      End Sub

      Private Sub picProperties_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'redraw to ensure button is correct
        PaintPropertyWindow

      End Sub

      Private Sub picSplitV_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'begin split operation
        picSplitVIcon.Height = picSplitV.Height
        picSplitVIcon.Move picSplitV.Left, picSplitV.Top
        picSplitVIcon.Visible = True

        'X is in TWIPS; pic positions are in pixels
        'save offset (in pixels)
        SplitVOffset = picSplitV.Left - X / ScreenTWIPSX
      End Sub


      Private Sub picSplitV_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim Pos As Single

        'X is in TWIPS; pic positions are in pixels
        'offset is in pixels

        'if splitting
        If picSplitVIcon.Visible Then
          Pos = X / ScreenTWIPSX + SplitVOffset
          'limit movement so left panel is >220 and <440
          ' or move off edge to hide the panel

          'scalewidth(calcwidth)-pos is width of left panel
          If Pos >= CalcWidth - 3 * SPLIT_WIDTH Then
            Pos = CalcWidth - SPLIT_WIDTH
          ElseIf Pos > CalcWidth - MIN_SPLIT_V Then
            Pos = CalcWidth - MIN_SPLIT_V
          ElseIf CalcWidth - Pos > MAX_SPLIT_V Then
            Pos = CalcWidth - MAX_SPLIT_V
          End If

          'move splitter icon and splitter form
          picSplitVIcon.Left = Pos
        End If
      End Sub


      Private Sub picSplitV_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim Pos As Single

        'if splitting
        If picSplitVIcon.Visible Then

          'stop splitting
          picSplitVIcon.Visible = False

          Pos = X / ScreenTWIPSX + SplitVOffset
          'limit movement so left panel is >220 and <440
          ' or at right edge to hide the panel

          If Pos >= CalcWidth - 3 * SPLIT_WIDTH Then
            Pos = CalcWidth - SPLIT_WIDTH
            'hide the preview panel, if it's currently visible
            If ShowVEPrev Then
              MenuClickECustom1
              'move the split icon
              picSplitV.Left = Pos
            End If
            Exit Sub

          ElseIf Pos > CalcWidth - MIN_SPLIT_V Then
            Pos = CalcWidth - MIN_SPLIT_V
          ElseIf CalcWidth - Pos > MAX_SPLIT_V Then
            Pos = CalcWidth - MAX_SPLIT_V
          End If

          'move the split icon
          picSplitV.Left = Pos

          'if currently visible,
          If ShowVEPrev Then
            'just resposition it
            picPrevFrame.Left = Pos + picSplitV.Width
            picPrevFrame.Width = CalcWidth - picPrevFrame.Left
            picSurface.Width = picSplitV.Left - picSurface.Left

            'now make sure scrollbars are positioned correctly
            SetEScrollbars
          Else
            'otherwise show it
            MenuClickECustom1
          End If

        End If
      End Sub


      Private Sub picSurface_Click()
        'if a selection is visible
        If shpView.Visible Then
          'unselect
          Timer2.Enabled = False
          shpView.Visible = False
          shpSurface.Visible = False
        End If
      End Sub

      Private Sub picSurface_GotFocus()

        'unhighlight any selected property
        PaintPropertyWindow False

      End Sub


      Private Sub picSurface_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        On Error GoTo ErrHandler

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        'if a right click
        If Button = vbRightButton Then
          'reset current operation
          CurrentOperation = opNone
          'refresh edit menu first
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
          PopupMenu frmMDIMain.mnuEdit, , X + picSurface.Left, Y + picSurface.Top
          'set formactivated flag to stop mousemove from being called incorrectly after popup
          FormActivated = True
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub picSurface_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim tmpX As Single, tmpY As Single

        'if not active form
        If Not frmMDIMain.ActiveMdiChild Is Me Then
          Exit Sub
        End If

        'if dragging picture
        If blnDragging Then
          'get new scrollbar positions
          tmpX = sngOffsetX - X
          tmpY = sngOffsetY - Y

          'if vertical scrollbar is visible
          If vsbCel.Visible Then
            'limit positions to valid values
            If tmpY < vsbCel.Min Then
              tmpY = vsbCel.Min
            ElseIf tmpY > vsbCel.Max Then
              tmpY = vsbCel.Max
            End If
            'set vertical scrollbar
            vsbCel.Value = tmpY
          End If

          'if horizontal scrollbar is visible
          If hsbCel.Visible Then
            'limit positions to valid values
            If tmpX < hsbCel.Min Then
              tmpX = hsbCel.Min
            ElseIf tmpX > hsbCel.Max Then
              tmpX = hsbCel.Max
            End If
            'set horizontal scrollbar
            hsbCel.Value = tmpX
          End If
        End If
      End Sub


      Private Sub picSurface_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim rtn As Long

        'if dragging
        If blnDragging Then
          'cancel dragmode
          blnDragging = False
          'release mouse capture
          rtn = ReleaseCapture()
          picSurface.MousePointer = vbDefault
          picCel.MousePointer = vbCustom
        End If
      End Sub


      Private Sub picCel_DblClick()

        'if current tool is selection
        If SelectedTool = ttSelect Then
          'if moving a selection
          If CurrentOperation = opMoveSelection Then
            'put it back first
            PasteSelection CelAnchorX, CelAnchorY
          End If
          MenuClickSelectAll
        End If
      End Sub

      Private Sub picCel_GotFocus()
        'set activate flag
        'this is needed because
        'mousemove is sometimes called immediately after
        'clicking the picture control when it didn't
        'previously have the focus

        'this flag skips that first mousemove call
        FormActivated = True

        'unhighlight any selected property
        PaintPropertyWindow False
      End Sub

      Private Sub picCel_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim rtn As Long, FillCol As Long
        Dim hBrush As Long, hOldBrush As Long

        On Error GoTo ErrHandler

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        'if property list is open, cancel it
        If lstProperty.Visible Then
          lstProperty.Visible = False
          picCel.SetFocus
          Exit Sub
        End If

        'if property textbox is open, cancel it
        If txtProperty.Visible Then
          txtProperty.Visible = False
          picCel.SetFocus
          Exit Sub
        End If

        'set color based on button pressed
        Select Case Button
        Case vbLeftButton
          DrawCol = LeftColor
        Case vbRightButton
          DrawCol = RightColor
        Case Else
          Exit Sub
        End Select

        'if erasing,
        If SelectedTool = ttErase Then
          'override color
          DrawCol = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor
        End If

        'calculate this pixel position
        '(which is the anchor for subsequent mouse
        'move operations)
        CelAnchorX = X \ (2 * ScaleFactor)
        CelAnchorY = Y \ ScaleFactor

        'limit to width and height
        If CelAnchorX < 0 Then
          CelAnchorX = 0
        End If
        If CelAnchorX > ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1 Then
          CelAnchorX = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1
        End If
        If CelAnchorY < 0 Then
          CelAnchorY = 0
        End If
        If CelAnchorY > ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1 Then
          CelAnchorY = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1
        End If

        Select Case SelectedTool
        Case ttEdit
          'if at bottom edge
          If Y > picCel.Height - 3 Then
            'change height
            CurrentOperation = opChangeHeight
          End If
          'if at right edge
          If X > picCel.Width - 3 Then
            'change width (or both, if height is also being changed)
            If CurrentOperation = opChangeHeight Then
              CurrentOperation = opChangeBoth
            Else
              CurrentOperation = opChangeWidth
            End If
          End If

          'if resizing,
          If CurrentOperation <> opNone Then
            Exit Sub
          End If

          'if either scrollbar is visible,
          If vsbCel.Visible Or hsbCel.Visible Then
            'set dragpic mode
            blnDragging = True
            'set pointer to custom
            picSurface.MousePointer = vbCustom
            picCel.MousePointer = vbDefault
            rtn = SetCapture(picSurface.hWnd)
            'save x and Y offsets
            sngOffsetX = X
            sngOffsetY = Y
          End If

        Case ttSelect
          'if using right button
          If Button = vbRightButton Then
            'update menu
            SetEditMenu
            'reset current operation
            CurrentOperation = opNone
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
            PopupMenu frmMDIMain.mnuEdit, , X + picCel.Left + picSurface.Left, Y + picCel.Top + picSurface.Top
            'set formactivated flag to stop mousemove from being called incorrectly after popup
            FormActivated = True
            Exit Sub
          End If

          'if within selection region
          'NOTE: we use actual X and Y values here)
          If shpView.Visible And X > shpView.Left And X < (shpView.Left + shpView.Width) And _
                                 Y > shpView.Top And Y < (shpView.Top + shpView.Height) Then
            BeginSelectionMove CelAnchorX, CelAnchorY
          Else
            'if at bottom edge
            If Y > picCel.Height - 3 Then
              'change height
              CurrentOperation = opChangeHeight
            End If
            'if at right edge
            If X > picCel.Width - 3 Then
              'change width (or both, if height is also being changed)
              If CurrentOperation = opChangeHeight Then
                CurrentOperation = opChangeBoth
              Else
                CurrentOperation = opChangeWidth
              End If
            End If
            'if not changing anything(currentop =none)
            If CurrentOperation = opNone Then
              'set operation to setsel
              CurrentOperation = opSetSelection
              'select current pixel
              SelectRegion CelAnchorX, CelAnchorY
            End If
          End If

        Case ttDraw, ttErase
          'save pixel to array
          ReDim PixelData(1)
          PixelCount = 1
          PixelData(1) = &H10000 * CelAnchorX + &H100 * CelAnchorY + ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CByte(CelAnchorX), CByte(CelAnchorY))
          'draw the new pixel
          picCel.Line (CelAnchorX * ScaleFactor * 2, CelAnchorY * ScaleFactor)-((CelAnchorX + 1) * ScaleFactor * 2 - 1, (CelAnchorY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          'update cel data
          ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CByte(CelAnchorX), CByte(CelAnchorY)) = DrawCol
          'update pixel cel
          SetPixelV picPCel.hDC, CelAnchorX, CelAnchorY, EGAColor(DrawCol)
          'set operation
          CurrentOperation = opDraw

          'draw grid
          If ScaleFactor > 3 And ShowGrid Then
            'draw grid over the pixel
            rtn = picCel.ForeColor
            With picCel
              .ForeColor = GRID_COLOR
              picCel.Line (CelAnchorX * ScaleFactor * 2, 0)-Step(0, .Height)
              picCel.Line ((CelAnchorX + 1) * ScaleFactor * 2, 0)-Step(0, .Height)
              picCel.Line (0, CelAnchorY * ScaleFactor)-Step(.Width, 0)
              picCel.Line (0, (CelAnchorY + 1) * ScaleFactor)-Step(.Width, 0)
              .ForeColor = rtn
            End With
          End If

        Case ttLine
          'draw the new pixel (at correct scale)
          picCel.Line (CelAnchorX * ScaleFactor * 2, CelAnchorY * ScaleFactor)-((CelAnchorX + 1) * ScaleFactor * 2 - 1, (CelAnchorY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          'set operation to draw
          CurrentOperation = opDraw

        Case ttRectangle
          'draw the new pixel (at correct scale)
          picCel.Line (CelAnchorX * ScaleFactor * 2, CelAnchorY * ScaleFactor)-((CelAnchorX + 1) * ScaleFactor * 2 - 1, (CelAnchorY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          'set operation to draw
          CurrentOperation = opDraw

        Case ttBoxFill
          'draw the new pixel (at correct scale)
          picCel.Line (CelAnchorX * ScaleFactor * 2, CelAnchorY * ScaleFactor)-((CelAnchorX + 1) * ScaleFactor * 2 - 1, (CelAnchorY + 1) * ScaleFactor - 1), EGAColor(CInt(DrawCol)), BF
          'set operation to draw
          CurrentOperation = opDraw

        Case ttPaint
          FloodFill CelAnchorX, CelAnchorY, EGAColor(CInt(DrawCol))

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If
        End Select

        'if any tool other than select is active or not selecting or moving selection
        If (SelectedTool <> ttSelect) Or (CurrentOperation <> opMoveSelection And CurrentOperation <> opSetSelection) Then
          'hide the selection shapes
          Timer2.Enabled = False
          shpView.Visible = False
          shpSurface.Visible = False
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub picCel_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim ViewX As Integer, ViewY As Integer
        Dim rtn As Long, prevCol As Long

        On Error GoTo ErrHandler:

        'if not active form
        If Not frmMDIMain.ActiveMdiChild Is Me Then
          Exit Sub
        End If

        'need to check if mousemove is caused by activation
        If FormActivated Then
          'reset it
          FormActivated = False
          'and exit
          Exit Sub
        End If

        'calculate position
        ViewX = X \ (2 * ScaleFactor)
        ViewY = Y \ ScaleFactor

        'force lower limit to zero
        If ViewX < 0 Then
          ViewX = 0
        End If
        If ViewY < 0 Then
          ViewY = 0
        End If

        'force upper limit to cel width/height
        If ViewX > ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1 Then
          ViewX = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1
        End If
        If ViewY > ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1 Then
          ViewY = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1
        End If

        'if not changing height or width OR not doing anything
        If Not (CurrentOperation = opNone Or CurrentOperation = opChangeBoth Or _
                CurrentOperation = opChangeHeight Or CurrentOperation = opChangeWidth) Then
          'if same as old values (haven't moved off current pixel)
          If ViewX = OldX And ViewY = OldY Then
            'exit
            Exit Sub
          End If
        End If

        'save as old values
        OldX = ViewX
        OldY = ViewY

        '*'Debug.Assert frmMDIMain.ActiveMdiChild Is Me
        'update cursor position
        MainStatusBar.Panels("CurX").Text = "X: " & CStr(ViewX)
        MainStatusBar.Panels("CurY").Text = "Y: " & CStr(ViewY)

        Select Case CurrentOperation
        Case opNone 'same as button=0
          Select Case SelectedTool
          Case ttEdit
            If X > picCel.Width - 3 Or Y > picCel.Height - 3 Then
              'check for corner
              If X > picCel.Width - 3 And Y > picCel.Height - 3 Then
                'use corner arrow
                picCel.MouseIcon = LoadResPicture("EVC_BOTH", vbResCursor)
              ElseIf X > picCel.Width - 3 Then
                'use width arrow
                picCel.MouseIcon = LoadResPicture("EVC_WIDTH", vbResCursor)
              Else
                'use height arrow
                picCel.MouseIcon = LoadResPicture("EVC_HEIGHT", vbResCursor)
              End If
            Else
              'use regular cursor
              picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)
            End If

          Case ttSelect
            'if inside selection box
            If shpView.Visible And X > shpView.Left And X < (shpView.Left + shpView.Width) And Y > shpView.Top And Y < (shpView.Top + shpView.Height) Then
              'set cursor to move
              picCel.MouseIcon = LoadResPicture("EVC_MOVE", vbResCursor)
            ElseIf X > picCel.Width - 3 Or Y > picCel.Height - 3 Then
              'check for corner
              If X > picCel.Width - 3 And Y > picCel.Height - 3 Then
                'use corner arrow
                picCel.MouseIcon = LoadResPicture("EVC_BOTH", vbResCursor)
              ElseIf X > picCel.Width - 3 Then
                'use width arrow
                picCel.MouseIcon = LoadResPicture("EVC_WIDTH", vbResCursor)
              Else
                'use height arrow
                picCel.MouseIcon = LoadResPicture("EVC_HEIGHT", vbResCursor)
              End If
            Else
              'use regular cursor
              picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)
            End If
          End Select

        Case opSetSelection
          'select region defined by current position and anchor
          SelectRegion ViewX, ViewY

        Case opMoveSelection
          MoveSelection (ViewX - CelXOffset), (ViewY - CelYOffset)

        Case opDraw
          Select Case SelectedTool
          Case ttDraw, ttErase
            'save pixel data
            PixelCount = PixelCount + 1
            ReDim Preserve PixelData(PixelCount)
            PixelData(PixelCount) = &H10000 * ViewX + &H100 * ViewY + ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CByte(ViewX), CByte(ViewY))
            'draw the new pixel (at correct scale)
            picCel.Line (ViewX * ScaleFactor * 2, ViewY * ScaleFactor)-((ViewX + 1) * ScaleFactor * 2 - 1, (ViewY + 1) * ScaleFactor - 1), EGAColor(DrawCol), BF
            'draw grid
            If ScaleFactor > 3 And ShowGrid Then
              'draw grid over the pixel
              prevCol = picCel.ForeColor
              With picCel
                .ForeColor = GRID_COLOR
                picCel.Line (ViewX * ScaleFactor * 2, 0)-Step(0, .Height)
                picCel.Line ((ViewX + 1) * ScaleFactor * 2, 0)-Step(0, .Height)
                picCel.Line (0, ViewY * ScaleFactor)-Step(.Width, 0)
                picCel.Line (0, (ViewY + 1) * ScaleFactor)-Step(.Width, 0)
                .ForeColor = prevCol
              End With
            End If
            'update pixel cel
            SetPixelV picPCel.hDC, ViewX, ViewY, EGAColor(DrawCol)

            'update cel data
            ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).CelData(CByte(ViewX), CByte(ViewY)) = DrawCol
            'reset anchor
            CelAnchorX = ViewX
            CelAnchorY = ViewY

          Case ttLine
            DrawLineOnSurface ViewX, ViewY

          Case ttRectangle
            DrawBoxOnSurface ViewX, ViewY, False

          Case ttBoxFill
            DrawBoxOnSurface ViewX, ViewY, True

          Case ttErase
            'erase the new pixel
            picCel.Line (ViewX * ScaleFactor * 2, ViewY * ScaleFactor)-((ViewX + 1) * ScaleFactor * 2 - 1, (ViewY + 1) * ScaleFactor - 1), EGAColor(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor), BF
            If ScaleFactor > 3 And ShowGrid Then
              'draw grid over the pixel
              prevCol = picCel.ForeColor
              With picCel
                .ForeColor = GRID_COLOR
                picCel.Line (ViewX * ScaleFactor * 2, 0)-Step(0, .Height)
                picCel.Line ((ViewX + 1) * ScaleFactor * 2, 0)-Step(0, .Height)
                picCel.Line (0, ViewY * ScaleFactor)-Step(.Width, 0)
                picCel.Line (0, (ViewY + 1) * ScaleFactor)-Step(.Width, 0)
                .ForeColor = prevCol
              End With
            End If
          End Select

        Case opChangeWidth, opChangeHeight, opChangeBoth
          'need to recalculate viewx and viewy
          'and limit upper bounds to Max values
          If CurrentOperation = opChangeWidth Or CurrentOperation = opChangeBoth Then
            'calculate and set width
            ViewX = X \ (2 * ScaleFactor)
            If ViewX < 0 Then
              ViewX = 0
            End If
            If ViewX > MAX_CEL_W - 1 Then
              ViewX = MAX_CEL_W - 1
            End If
            'update cursor position
            MainStatusBar.Panels("CurX").Text = "X: " & CStr(ViewX)

            'set width
            picCel.Width = (ViewX + 1) * 2 * ScaleFactor
            If ShowGrid And ScaleFactor > 3 Then
              DrawCelGrid
            End If

            'redraw properties
            picProperties.Line (PropSplitLoc + 1, PROP_ROW_HEIGHT)-(picProperties.Width - 1, PROP_ROW_HEIGHT * 2 - 2), vbWhite, BF
            picProperties.CurrentX = PropSplitLoc + 3
            picProperties.CurrentY = PROP_ROW_HEIGHT + 1
            picProperties.ForeColor = vbBlack
            picProperties.Print ViewX + 1
          End If
          If CurrentOperation = opChangeHeight Or CurrentOperation = opChangeBoth Then
            'calculate and set height
            ViewY = Y \ ScaleFactor
            If ViewY < 0 Then
              ViewY = 0
            End If
            If ViewY > MAX_CEL_H - 1 Then
              ViewY = MAX_CEL_H - 1
            End If

            'update cursor position
            MainStatusBar.Panels("CurY").Text = "Y: " & CStr(ViewY)

            'set height
            picCel.Height = (ViewY + 1) * ScaleFactor
            If ShowGrid And ScaleFactor > 3 Then
              DrawCelGrid
            End If

            'redraw properties
            picProperties.Line (PropSplitLoc + 1, PROP_ROW_HEIGHT * 2)-(picProperties.Width - 1, PROP_ROW_HEIGHT * 3 - 2), vbWhite, BF
            picProperties.CurrentX = PropSplitLoc + 3
            picProperties.CurrentY = PROP_ROW_HEIGHT * 2 + 1
            picProperties.ForeColor = vbBlack
            picProperties.Print ViewY + 1
          End If
        End Select
      Exit Sub

      ErrHandler:

        Resume Next

      End Sub

      Private Sub FloodFill(X As Long, Y As Long, FillCol As Long)
        'paints an area with fill color

        Dim rtn As Long
        Dim hBrush As Long
        Dim lngColor As Long
        Dim hOldBrush As Long
        Dim pX As Long, pY As Long

        Dim NextUndo As ViewUndo

        'get target color
        lngColor = GetPixel(picPCel.hDC, X, Y)

        'if this is same as paint color
        If lngColor = FillCol Then
          'just exit
          Exit Sub
        End If

        If Settings.ViewUndo <> 0 Then
          'create new undo object
          Set NextUndo = New ViewUndo
          NextUndo.UDAction = udvPaintFill
          NextUndo.UDLoopNo = SelectedLoop
          NextUndo.UDCelNo = SelectedCel
          NextUndo.CelData = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).AllCelData

          AddUndo NextUndo
        End If
        'create a new brush
        hBrush = CreateSolidBrush(FillCol)

        'select it into device context
        hOldBrush = SelectObject(picPCel.hDC, hBrush)

        rtn = ExtFloodFill(picPCel.hDC, X, Y, lngColor, FLOODFILLSURFACE)
        picPCel.Refresh

        'select old object
        rtn = SelectObject(picCel.hDC, hOldBrush)
        rtn = DeleteObject(hBrush)

        'rebuild the cel data array
        BuildCelData 0, 0, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1

        'stretch the pixel cel into the draw cel
        StretchBlt picCel.hDC, 0, 0, picCel.Width, picCel.Height, picPCel.hDC, 0, 0, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width, ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height, SRCCOPY
        picCel.Refresh
        If ShowGrid And ScaleFactor > 3 Then
          DrawCelGrid
        End If
      End Sub



      Private Sub picCel_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

      On Error GoTo ErrHandler

        Dim ViewX As Long, ViewY As Long
        Dim tmpSwap As Long
        Dim rtn As Long
        Dim NextUndo As ViewUndo
        Dim CelHeight As Long, CelWidth As Long
        Dim i As Long, j As Long
        Dim tmpTransCol As AGIColors
        Dim tmpData() As AGIColors

        '*'Debug.Assert Not (UndoCol Is Nothing)
        'calculate position
        ViewX = X \ (2 * ScaleFactor)
        ViewY = Y \ ScaleFactor

        'force lower limit to zero
        If ViewX < 0 Then
          ViewX = 0
        End If
        If ViewY < 0 Then
          ViewY = 0
        End If

        'force upper limit to cel width/height
        If ViewX > ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1 Then
          ViewX = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width - 1
        End If
        If ViewY > ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1 Then
          ViewY = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height - 1
        End If

        Select Case CurrentOperation
        Case opSetSelection
          'if a selection is visible
          If shpView.Visible Then
            'set the selection
            SetSelection
          End If

        Case opMoveSelection
          'if didn't move selection
          If SelectionNotMoved Then
            'restore the dataundersel
            BitBlt picPCel.hDC, ViewX - CelXOffset, ViewX - CelXOffset, SelWidth, SelHeight, picClipboard.hDC, 0, 0, SRCCOPY
            picPCel.Refresh
            'update the actual view with correct data
            BuildCelData ViewX - CelXOffset, ViewX - CelXOffset, SelWidth - 1, SelHeight - 1
            Timer2.Enabled = True
          Else
            If Settings.ViewUndo <> 0 Then
              'save undodata
              Set NextUndo = New ViewUndo
              With NextUndo
                .UDAction = udvMoveSelection
                .UDLoopNo = SelectedLoop
                .UDCelNo = SelectedCel
                .ResizeData 5
                'save original location of selection
                .UndoData(0) = CelAnchorX - CelXOffset
                .UndoData(1) = CelAnchorY - CelYOffset
                .UndoData(2) = SelWidth
                .UndoData(3) = SelHeight
                'save final location of selection
                .UndoData(4) = ViewX - CelXOffset
                .UndoData(5) = ViewY - CelYOffset
                'save data under original location
                tmpData = UndoDataUnderSel
                'redim to allow enough room for area under new location
                ' (selection pic has that data)
                ReDim Preserve tmpData(SelWidth - 1, SelHeight * 2 - 1)
                For i = 0 To SelWidth - 1
                  For j = 0 To SelHeight - 1
                      tmpData(i, SelHeight + j) = GetPixel(picSelection.hDC, i, j)
                  Next j
                Next i
                'redim to allow enough room for the selection
                ReDim Preserve tmpData(SelWidth - 1, SelHeight * 3 - 1)
                For i = 0 To SelWidth - 1
                  For j = 0 To SelHeight - 1
                    tmpData(i, 2 * SelHeight + j) = GetPixel(picClipboard.hDC, i, j)
                  Next j
                Next i
                NextUndo.CelData = tmpData
              End With
              'add to undo collection
              AddUndo NextUndo
            End If

            'finish move
            EndMoveSelection ViewX - CelXOffset, ViewY - CelYOffset

            'update preview
            If ShowVEPrev Then
              DisplayPrevLoop
              DisplayPrevCel
            End If
          End If

        Case opDraw
          'set undo object
          Set NextUndo = New ViewUndo
          NextUndo.UDLoopNo = SelectedLoop
          NextUndo.UDCelNo = SelectedCel

          'if line or box
          If SelectedTool = ttRectangle Or SelectedTool = ttBoxFill Then
            'swap so first variable is always lowest
            If CelAnchorX > ViewX Then
              tmpSwap = CelAnchorX
              CelAnchorX = ViewX
              ViewX = tmpSwap
            End If
            If CelAnchorY > ViewY Then
              tmpSwap = CelAnchorY
              CelAnchorY = ViewY
              ViewY = tmpSwap
            End If
          End If

          Select Case SelectedTool
          Case ttDraw, ttErase
            'if erasing
            If SelectedTool = ttErase Then
              NextUndo.UDAction = udvErase
            Else
              NextUndo.UDAction = udvDraw
            End If

            'set first element to size
            PixelData(0) = PixelCount
            'add pixel data
            NextUndo.CelData = PixelData

          Case ttLine
            NextUndo.UDAction = udvLine

            'draw the line
            DrawLineOnCel CelAnchorX, CelAnchorY, ViewX, ViewY, NextUndo

          Case ttRectangle

            NextUndo.UDAction = udvBox

            DrawBoxOnCel ViewX, ViewY, False, NextUndo

          Case ttBoxFill
            NextUndo.UDAction = udvBoxFill

            DrawBoxOnCel ViewX, ViewY, True, NextUndo

          End Select

          If Settings.ViewUndo <> 0 Then
            'add undo item
            AddUndo NextUndo
          End If

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case opChangeWidth, opChangeHeight, opChangeBoth
          'save height of picture box
          '(because changewidth call will reset
          'it to previous Value)
          ViewY = picCel.Height \ ScaleFactor
          'if width was being adjusted
          If CurrentOperation = opChangeWidth Or CurrentOperation = opChangeBoth Then
            'if width has changed,
            If ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Width <> picCel.Width \ (ScaleFactor * 2) Then
              'set new width
              ChangeWidth picCel.Width \ (ScaleFactor * 2)
            End If
          End If
          'if height was being adjusted
          If CurrentOperation = opChangeHeight Or CurrentOperation = opChangeBoth Then
            'if height has changed
            If ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).Height <> ViewY Then
              'set new height
              ChangeHeight ViewY
            End If
          End If
          'reset cursor
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        End Select
        'always reset operation
        CurrentOperation = opNone
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub SetSelection()

        Dim rtn As Long
        Dim tmpTransCol As AGIColors
        Dim i As Long, j As Long

        On Error GoTo ErrHandler

        'clear out selection area picture box
        'set selection to blank with current transcolor
        picSelection.BackColor = EGAColor(ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor)
        picSelection.Cls
        picSelection.Refresh

        'adjust selection area picturebox
        SelWidth = (shpView.Width - 2) / ScaleFactor / 2
        SelHeight = (shpView.Height - 2) / ScaleFactor

        'copy selected area to clipboard
        rtn = BitBlt(picClipboard.hDC, 0, 0, SelWidth, SelHeight, picPCel.hDC, (shpView.Left + 1) / ScaleFactor / 2, (shpView.Top + 1) / ScaleFactor, SRCCOPY)
        picClipboard.Refresh

        'set data under selection to be blank
        DelOriginalData = True
        tmpTransCol = ViewEdit.Loops(SelectedLoop).Cels(SelectedCel).TransColor
        ReDim DataUnderSel(SelWidth - 1, SelHeight - 1)
        For i = 0 To SelWidth - 1
          For j = 0 To SelHeight - 1
            DataUnderSel(i, j) = tmpTransCol
          Next j
        Next i

        'enable selection outline flashing
        Timer2.Enabled = True
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub picSurface_Paint()

        Dim i As Long, sngH As Single, sngW As Single

        If ShowGrid And ScaleFactor > 3 Then
          'draw a simple grid as a background
          sngW = picSurface.Width
          sngH = picSurface.Height

          i = picCel.Left
          Do
            picSurface.Line (i, 0)-Step(0, sngH)
            i = i + ScaleFactor * 2
          Loop Until i > sngW

          i = picCel.Top
          Do
            picSurface.Line (0, i)-Step(sngW, 0)
            i = i + ScaleFactor
          Loop Until i > sngH
        Else
          picSurface.Cls
        End If
      End Sub

      Private Sub sldSpeed_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub

      Private Sub Timer2_Timer()

        'selection timer

        'if selection shape is visible
        If shpView.Visible Then
          'toggle shape style
          shpView.BorderStyle = shpView.BorderStyle + 1
          If shpView.BorderStyle = 6 Then
            shpView.BorderStyle = 2
          End If
          shpSurface.BorderStyle = shpSurface.BorderStyle + 1
          If shpSurface.BorderStyle = 6 Then
            shpSurface.BorderStyle = 2
          End If
        End If
      End Sub

      Private Sub tlbView_ButtonClick(ByVal Button As MSComctlLib.Button)

      '  1   Undo        undo
      '  2   separator
      '  3   Cut         cut
      '  4   Copy        copy
      '  5   Paste       paste
      '  6   Delete      delete
      '  7   Zoom In     zoomin
      '  8   Zoom Out    zoomout
      '  9   Flip Horiz  fliph
      ' 10   Flip Vert   flipv
      ' 11   separator
      ' 12   No Tool     none
      ' 13   Select      select
      ' 14   Draw        draw
      ' 15   Line        line
      ' 16   Rectangle   box
      ' 17   Solid Rect  boxfill
      ' 18   Fill        fill
      ' 19   Rease       erase

        On Error GoTo ErrHandler

        Select Case Button.Key
        Case "undo"
          'same as clicking undo menu
          MenuClickUndo

        Case "cut"
          'same as clicking cut menu
          MenuClickCut

        Case "copy"
          'same as clicking copy menu
          MenuClickCopy

        Case "paste"
          'same as clicking paste menu
          MenuClickPaste

        Case "delete"
          'same as clicking delete menu
          MenuClickDelete

        Case "zoomin"
          ZoomCel 1

        Case "zoomout"
          ZoomCel 0

        Case "fliph"
          FlipH
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case "flipv"
          FlipV
          'update preview
          If ShowVEPrev Then
            DisplayPrevLoop
            DisplayPrevCel
          End If

        Case "none"
          SelectedTool = ttEdit

        Case "select"
          SelectedTool = ttSelect
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

        Case "draw"
          SelectedTool = ttDraw
          picCel.MouseIcon = LoadResPicture("EVC_DRAW", vbResCursor)

        Case "line"
          SelectedTool = ttLine
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

        Case "box"
          SelectedTool = ttRectangle
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

        Case "boxfill"
          SelectedTool = ttBoxFill
          picCel.MouseIcon = LoadResPicture("EVC_SELECT", vbResCursor)

        Case "fill"
          SelectedTool = ttPaint
          picCel.MouseIcon = LoadResPicture("EVC_PAINT", vbResCursor)

        Case "erase"
          SelectedTool = ttErase
          picCel.MouseIcon = LoadResPicture("EVC_ERASE" & CStr(ScaleFactor), vbResCursor)
        End Select

        '*'Debug.Assert frmMDIMain.ActiveMdiChild Is Me
        MainStatusBar.Panels("Tool") = LoadResString(VIEWTOOLTYPETEXT + SelectedTool)

        'if tool is not select
        If SelectedTool <> ttSelect Then
          'hide selection shapes
          Timer2.Enabled = False
          shpView.Visible = False
          shpSurface.Visible = False
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub tlbView_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        'ensure property list is hidden
        If lstProperty.Visible Then
          lstProperty.Visible = False
        End If

        'ensure property textbox is hidden
        If txtProperty.Visible Then
          txtProperty.Visible = False
        End If
      End Sub

      Private Sub tvwView_Collapse(ByVal Node As MSComctlLib.Node)

        If Node.Key = ViewEdit.ID Then
          Exit Sub
        End If
      End Sub


      Private Sub tvwView_Expand(ByVal Node As MSComctlLib.Node)

        'if first node,
        If Node.Key = ViewEdit.ID Then
          Exit Sub
        End If

      End Sub


      Private Sub tvwView_KeyDown(KeyCode As Integer, Shift As Integer)

        'check for press of a loop/cel navigation key
        CheckNavigationKeys KeyCode, Shift

      End Sub

      Private Sub tvwView_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        On Error GoTo ErrHandler

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        'set flag to indicate
        'right mouse button

        TreeRightButton = (Button = vbRightButton)
        TreeX = X / ScreenTWIPSX + tvwView.Left
        TreeY = Y / ScreenTWIPSY + tvwView.Top

        'if not over a node
        If tvwView.HitTest(X, Y) Is Nothing Then
          'if right-clicked
          If TreeRightButton Then
            'reset edit menu first
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
            PopupMenu frmMDIMain.mnuEdit, 0, TreeX, TreeY
          End If
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Private Sub tvwView_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)
        'always reset treeright button
        TreeRightButton = False

      End Sub


      Private Sub tvwView_NodeClick(ByVal Node As MSComctlLib.Node)

        Dim NewLoop As Byte, NewCel As Byte
        Dim blnChangeLoop As Boolean

        On Error GoTo ErrHandler

        'if root,
        If Node.Parent Is Nothing Then
          'hide the draw surface
          picSurface.Visible = False
          'set loop to -1
          SelectedLoop = -1
          SelectedCel = -1
          ViewMode = vmView

          'update preview
          If ShowVEPrev Then
            'only need to call displayprevloop; it will
            'hide the preview cel for us
            DisplayPrevLoop
          End If

          'refresh the palette
          picPalette.Refresh

        'if loop, then parent's parent is nothing
        ElseIf Node.Parent.Parent Is Nothing Then
          'if end,
          If Node.Text = "End" Then
            'set selected loop number to loop Count
            '(which is number of child nodes -1)
            SelectedLoop = Node.Parent.Children - 1
            'why the heck did I set the loop to the count value? -1 is the
            'designator for 'no loop'
            '!!!!BECAUSE THAT'S HOW WE ARE ABLE TO ADD NEW LOOPS YOU DUMMY!

            'update preview
            If ShowVEPrev Then
              'only need to call displayprevloop; it will
              'hide the preview cel for us
              DisplayPrevLoop
            End If

            'refresh the palette
            picPalette.Refresh

          Else
            'select this loop
            SelectedLoop = Val(Right$(Node.Text, Len(Node.Text) - 4))

            'need to SHOW the preview cel IF we are previewing
            If ShowVEPrev Then
              DisplayPrevLoop
              ' when clicking a loop, always reset cel
              PrevCel = 0
              DisplayPrevCel
            End If
          End If

          'hide draw surface
          picSurface.Visible = False
          SelectedCel = -1
          ViewMode = vmLoop

          'refresh the palette
          picPalette.Refresh

        'otherwise MUST be a cel
        Else
          'get selected loop number
          NewLoop = Val(Right$(Node.Parent.Text, Len(Node.Parent.Text) - 4))

          'if end,
          If Node.Text = "End" Then
            NewCel = Node.Parent.Children - 1
            'hide the drawing surface
            picSurface.Visible = False
            'refresh the palette
            picPalette.Refresh
          Else
            'get loop and cel values
            NewCel = Val(Right$(Node.Key, Len(Node.Key) - InStr(3, Node.Key, ":")))
            'display the drawing surface
            picSurface.Visible = True
          End If

          'if not the same as the currently displayed cel, (AND not showing end cel)
          If (NewLoop <> SelectedLoop Or NewCel <> SelectedCel) Then
            'we need to know if loop changed
            blnChangeLoop = NewLoop <> SelectedLoop

            'update the loop and cel
            SelectedLoop = NewLoop
            SelectedCel = NewCel

            'display selected cel, if it not End
            '(by checking drawing surface visible property)
            If picSurface.Visible Then
              DisplayCel True
            End If

            'update preview
            If ShowVEPrev Then
              'if loop changed, update preview loop
              If blnChangeLoop Then
                DisplayPrevLoop
              End If
              If SelectedCel = ViewEdit.Loops(SelectedLoop).Cels.Count Then
                'select end
                PrevCel = SelectedCel - 1
              Else
                PrevCel = SelectedCel
              End If
              DisplayPrevCel
            End If
          Else
            'clicking same cel; need to clear selection if there is one
            'if there is a selection
            If shpView.Visible Then
              'hide selection shapes
              Timer2.Enabled = False
              shpView.Visible = False
              shpSurface.Visible = False
            End If
          End If
        End If

        'set menus
        SetEditMenu

        'if changing,
        If OldNode <> Node.Index Then
          SelectedProp = 0
        End If

        'redraw of properties pic
        PaintPropertyWindow
        'if right-clicked
        If TreeRightButton Then
          'reset edit menu first
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
          PopupMenu frmMDIMain.mnuEdit, 0, TreeX, TreeY
          Exit Sub
        End If
        'save this node
        OldNode = Node.Index
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub txtProperty_Change()
        Dim tmpSS As Long

        On Error GoTo ErrHandler

        'if editing height or width
        If txtProperty.Tag = "HEIGHT" Or txtProperty.Tag = "WIDTH" Then
          'limit to four characters
          If Len(txtProperty.Text) > 4 Then
            tmpSS = txtProperty.SelStart
            txtProperty.Text = Left$(txtProperty.Text, 4)
            txtProperty.SelStart = tmpSS
          End If
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub txtProperty_GotFocus()
        'refresh properties
        '(why do this for gotfocus?
        picProperties.Refresh
      End Sub

      Private Sub txtProperty_KeyDown(KeyCode As Integer, Shift As Integer)

      ''*'Debug.Assert False
      End Sub

      Private Sub txtProperty_KeyPress(KeyAscii As Integer)

        'trap enter key
        Select Case KeyAscii
        Case 10 'ctrl-enter key combination
          'if not description or viewdesc
          If txtProperty.Tag <> "VIEWDESC" And txtProperty.Tag <> "DESC" Then
            'treat it same as normal Enter key
            KeyAscii = 0
            SelectPropFromText
            Exit Sub
          End If
        Case 13 'enter key
          'select the new property value
          KeyAscii = 0
          SelectPropFromText
          Exit Sub

        Case 27 'esc key
          'cancel
          txtProperty.Visible = False
          KeyAscii = 0
          Exit Sub
        End Select

        'if height/width
        If txtProperty.Tag = "HEIGHT" Or txtProperty.Tag = "WIDTH" Then
          'only accept numbers, backspace, delete

          Select Case KeyAscii
          Case 48 To 57, 8
          Case Else
            KeyAscii = 0
          End Select
        End If

      End Sub

      Private Sub txtProperty_LostFocus()

        On Error GoTo ErrHandler

        'if visible,
        'ensure it's not visible
        If txtProperty.Visible Then
          txtProperty.Visible = False
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub vsbCel_Change()

        'position viewholder
        picCel.Top = -vsbCel.Value
        If picCel.Top > VE_MARGIN Then
          picCel.Top = VE_MARGIN
        End If

        'if selection shape visible
        If shpView.Visible Then
          'adjust location of surface shp as well
          shpSurface.Top = shpView.Top + picCel.Top
        End If

        If Me.Visible Then
          'force resize
          Form_Resize
          'move focus to palette
          picPalette.SetFocus
        End If
      End Sub

      Private Sub vsbCel_GotFocus()

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        'force focus back to palette
        picPalette.SetFocus
      End Sub


      Private Sub vsbCel_Scroll()

        vsbCel_Change
      End Sub

      Private Sub DrawTransGrid()

        Dim i As Long, j As Long, rtn As Long

        On Error GoTo ErrHandler

        picPrevSurface.Cls
        picPrevFrame.Cls

        'draw the background grid
        For i = 0 To picPrevFrame.Width / 10 + 1
          For j = 0 To picPrevFrame.Height / 10 + 1
            rtn = SetPixelV(picPrevSurface.hDC, i * 10, j * 10, 0)
            rtn = SetPixelV(picPrevFrame.hDC, i * 10, j * 10 + 4, 0)
          Next j
        Next i
        picPrevSurface.Refresh
        picPrevFrame.Refresh
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub KeyHandler(ByRef KeyAscii As Integer)

        Select Case KeyAscii
        Case 43 '+'
          'zoom in
          ZoomPrev 1
          KeyAscii = 0
        Case 45 '-'
          'zoom out
          ZoomPrev -1
          KeyAscii = 0

        Case 32 ' '
          'start/stop animation
          If cmdVPlay.Enabled Then
            cmdVPlay_Click
            KeyAscii = 0
          End If
        End Select
      End Sub

      Public Function DisplayPrevCel() As Boolean
        'this function copies the bitmap Image
        'from SelectedLoop.PrevCel into the view Image box,
        'and resizes it to be correct size

        Dim rtn As Long
        Dim tgtX As Long, tgtY As Long, tgtH As Long, tgtW As Long

        On Error GoTo ErrHandler

        '*'Debug.Assert picPrevCel.Visible
        If Not picPrevCel.Visible Then
          Exit Function
        End If

        SendMessage picPrevCel.hWnd, WM_SETREDRAW, 0, 0

        picPrevCel.Cls

        With ViewEdit.Loops(SelectedLoop).Cels(PrevCel)
          'copy view Image
          tgtW = .Width * 2 * ViewScale
          tgtH = .Height * ViewScale

          Select Case lngHAlign
          Case 0
            tgtX = 0
          Case 1
            tgtX = (picPrevCel.Width - tgtW) / 2
          Case 2
            tgtX = picPrevCel.Width - tgtW
          End Select
          Select Case lngVAlign
          Case 0
            tgtY = 0
          Case 1
            tgtY = (picPrevCel.Height - tgtH) / 2
          Case 2
            tgtY = picPrevCel.Height - tgtH
          End Select

          'if no transparency
          If Not blnTrans Then
            rtn = StretchBlt(picPrevCel.hDC, tgtX, tgtY, tgtW, tgtH, .CelBMP, 0&, 0&, CLng(.Width), CLng(.Height), SRCCOPY)
          Else
            'first get background
            rtn = BitBlt(picPrevCel.hDC, 0&, 0&, CLng(picPrevCel.Width), CLng(picPrevCel.Height), picPrevSurface.hDC, CLng(picPrevCel.Left), CLng(picPrevCel.Top), SRCCOPY)
            'use transblit!
            rtn = TransparentBlt(picPrevCel.hDC, tgtX, tgtY, tgtW, tgtH, .CelBMP, 0&, 0&, CLng(.Width), CLng(.Height), EGAColor(.TransColor))
          End If
        End With

        SendMessage picPrevCel.hWnd, WM_SETREDRAW, 1, 0
        picPrevCel.Refresh
        picPrevFrame.Refresh
        'success
        DisplayPrevCel = True
      Exit Function

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Function

      Private Sub DisplayPrevLoop()

        Dim i As Long, mW As Long, mH As Long

        On Error GoTo ErrHandler

        'if no loop is selected (=-1) OR End is selected,
        'we need tohide the preview
        If SelectedLoop = -1 Or SelectedLoop = ViewEdit.Loops.Count Then
          cmdVPlay.Enabled = False
          tmrMotion.Enabled = False
          picPrevCel.Visible = False
          Exit Sub
        Else
          'make sure cel is visible
          picPrevCel.Visible = True
          'enable play button
          cmdVPlay.Enabled = True
        End If

        If SelectedCel = -1 Then
          'always validate cel when changing preview loop
          If PrevCel >= ViewEdit.Loops(SelectedLoop).Cels.Count Then
            PrevCel = 0
          End If
        Else
          'start with current selection
          PrevCel = SelectedCel
        End If

        'if timer is enabled, show stop icon, otherwise show play icon
        If tmrMotion.Enabled Then
          'show stop icon
          cmdVPlay.Picture = imlPreview.ListImages(10).Picture
        Else
          'show play icon
          cmdVPlay.Picture = imlPreview.ListImages(9).Picture
        End If

        'if only one cel, override, and turn off both buttons and timer
        If ViewEdit.Loops(SelectedLoop).Cels.Count = 1 Then
          cmdVPlay.Enabled = False
          tmrMotion.Enabled = False
        End If

        'determine size of holding pic
        mW = 0
        mH = 0
        With ViewEdit.Loops(SelectedLoop)
          For i = 0 To .Cels.Count - 1
            If .Cels(i).Width > mW Then
              mW = .Cels(i).Width
            End If
            If .Cels(i).Height > mH Then
              mH = .Cels(i).Height
            End If
          Next i
        End With

        With picPrevCel
          'set size of view holder
          .Width = mW * 2 * ViewScale
          .Height = mH * ViewScale
          'force back to upper, left
          .Top = VE_MARGIN
          .Left = VE_MARGIN
        End With

        'set scroll bars everytime loop is changed
        SetPScrollbars
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Public Sub InitPreview()

        On Error GoTo ErrHandler

        'show correct toolbars for alignment
        tlbPreview.Buttons("HAlign").Image = lngHAlign + 3
        tlbPreview.Buttons("VAlign").Image = lngVAlign + 6

        'display the first loop
        DisplayPrevLoop

      '  'display the first cel in the loop
      '  DisplayPrevCel
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub
      Private Sub SetPScrollbars()

      On Error GoTo ErrHandler

        DontDraw = True

        With hsbPrevCel
          .Visible = (picPrevCel.Width > picPrevSurface.Width - 2 * VE_MARGIN)
          If .Visible Then
            .Width = picPrevSurface.Width
            .Max = .Min + picPrevCel.Width + 2 * VE_MARGIN - picPrevSurface.Width
          Else
            'reset it, reposition cel frame
            .Value = -VE_MARGIN
            picPrevCel.Left = VE_MARGIN
          End If
        End With

        With vsbPrevCel
          .Visible = (picPrevCel.Height > picPrevSurface.Height - 2 * VE_MARGIN)
          If .Visible Then
            .Height = picPrevSurface.Height
            .Max = .Min + picPrevCel.Height + 2 * VE_MARGIN - picPrevSurface.Height
          Else
            'reset it, reposition cel frame
            .Value = -VE_MARGIN
            picPrevCel.Top = VE_MARGIN
          End If
        End With

        'adjust scroll bar values
        hsbPrevCel.LargeChange = LG_SCROLL * picPrevSurface.Width
        vsbPrevCel.LargeChange = LG_SCROLL * picPrevSurface.Height
        hsbPrevCel.SmallChange = SM_SCROLL * picPrevSurface.Width
        vsbPrevCel.SmallChange = SM_SCROLL * picPrevSurface.Height

        DontDraw = False
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub cmbMotion_Click()

        If Not Visible Then
          Exit Sub
        End If

        picPrevCel.SetFocus
      End Sub

      Private Sub cmdToggleTrans_Click()

        blnTrans = Not blnTrans

        'toggle transparency
        If blnTrans Then
          DrawTransGrid
          cmdToggleTrans.Caption = "Transparency On"
        Else
          picPrevSurface.Cls
          picPrevFrame.Cls
          cmdToggleTrans.Caption = "Transparency Off"
        End If

        'if displaying preview, update it
        If picPrevCel.Visible Then
          DisplayPrevCel
          picPrevCel.SetFocus
        End If

      End Sub

      Private Sub cmdToggleTrans_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub

      Private Sub cmdVPlay_Click()

        'toggle motion and image
        tmrMotion.Enabled = Not tmrMotion.Enabled
        If tmrMotion.Enabled Then
          'show stop image
          cmdVPlay.Picture = imlPreview.ListImages(10).Picture

          'reset cel, if endofloop or reverseloop motion selected
          Select Case cmbMotion.ListIndex
          Case 2  'endofloop
            'if already on last cel
            If PrevCel >= ViewEdit.Loops(SelectedLoop).Cels.Count - 1 Then
              PrevCel = 0
            End If

          Case 3 'reverseloop
            'if already on first cel
            If PrevCel = 0 Then
              PrevCel = ViewEdit.Loops(SelectedLoop).Cels.Count - 1
            End If
          End Select

        Else
          'show play image
          cmdVPlay.Picture = imlPreview.ListImages(9).Picture
          'if editing a cel, show that cel
          If ViewMode = vmCel Then
            'make sure selectedcel is chosen
            'watch out for 'end' selection; in that case,
            'just stop where we land
            If PrevCel <> SelectedCel And SelectedCel <> ViewEdit.Loops(SelectedLoop).Cels.Count Then
              PrevCel = SelectedCel
              DisplayPrevCel
            End If
          End If
        End If

        picPrevCel.SetFocus
      End Sub

      Private Sub cmdVPlay_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub


      Private Sub Form_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim tmpX As Single, tmpY As Single

        On Error GoTo ErrHandler

        'if not active form
        If Not frmMDIMain.ActiveMdiChild Is Me Then
          Exit Sub
        End If

        'if dragging picture
        If blnDraggingView Then
          'get new scrollbar positions
          tmpX = sngOffsetX - X
          tmpY = sngOffsetY - Y + 2 * fraToolbar.Height

          'if vertical scrollbar is visible
          If vsbPrevCel.Visible Then
            'limit positions to valid values
            If tmpY < vsbPrevCel.Min Then
              tmpY = vsbPrevCel.Min
            ElseIf tmpY > vsbPrevCel.Max Then
              tmpY = vsbPrevCel.Max
            End If
            'set vertical scrollbar
            vsbPrevCel.Value = tmpY
          End If

          'if horizontal scrollbar is visible
          If hsbPrevCel.Visible Then
            'limit positions to valid values
            If tmpX < hsbPrevCel.Min Then
              tmpX = hsbPrevCel.Min
            ElseIf tmpX > hsbPrevCel.Max Then
              tmpX = hsbPrevCel.Max
            End If
            'set horizontal scrollbar
            hsbPrevCel.Value = tmpX
          End If
        End If
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub


      Private Sub Form_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)


        Dim rtn As Long

        'if dragging
        If blnDraggingView Then
          'cancel dragmode
          blnDraggingView = False
          'release mouse capture
          rtn = ReleaseCapture()
          Me.MousePointer = vbDefault
        End If
      End Sub


      Private Sub fraToolbar_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub


      Private Sub fraVMotion_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub


      Private Sub hsbPrevCel_Change()

        'if not updating
        If Not blnNoUpdate Then
          'position viewholder
          picPrevCel.Left = -hsbPrevCel.Value
        End If
      End Sub

      Private Sub hsbPrevCel_GotFocus()

        'set focus to cel
        picPrevCel.SetFocus

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub


      Private Sub hsbPrevCel_Scroll()

        hsbPrevCel_Change
      End Sub

      Private Sub picPrevCel_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        Dim rtn As Long

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False

        'if either scrollbar is visible,
        If hsbPrevCel.Visible Or vsbPrevCel.Visible Then
          'set dragView mode
          blnDraggingView = True

          'set pointer to custom
          Me.MousePointer = vbCustom
          rtn = SetCapture(Me.hWnd)
          'save x and Y offsets
          sngOffsetX = X
          sngOffsetY = Y
        End If
      End Sub


      Private Sub sldSpeed_Change()

      '''  tmrMotion.Interval = 600 / sldSpeed - 45
      '''  tmrMotion.Interval = 750 / sldSpeed - 45
        tmrMotion.Interval = 900 / sldSpeed - 50
      End Sub

      Private Sub tlbHAlign_ButtonClick(ByVal Button As MSComctlLib.Button)

        Dim i As Long, MaxW As Long

        'set alignment
        lngHAlign = Button.Index - 1

        'hide toolbar
        tlbHAlign.Visible = False

        'update main toolbar
        tlbPreview.Buttons("HAlign").Image = lngHAlign + 3

        'redraw the cel to update
        DisplayPrevCel
      End Sub

      Private Sub tlbVAlign_ButtonClick(ByVal Button As MSComctlLib.Button)

        'set alignment
        lngVAlign = Button.Index - 1

        'hide toolbar
        tlbVAlign.Visible = False

        'update main toolbar
        tlbPreview.Buttons("VAlign").Image = lngVAlign + 6

        'redraw the cel to update
        DisplayPrevCel
      End Sub


      Private Sub tmrMotion_Timer()

        On Error GoTo ErrHandler

        If PrevCel > ViewEdit.Loops(SelectedLoop).Cels.Count - 1 Then
          PrevCel = 0
        End If

        'advance to next cel, depending on mode
        Select Case cmbMotion.ListIndex
        Case 0  'normal
          If PrevCel = ViewEdit.Loops(SelectedLoop).Cels.Count - 1 Then
            PrevCel = 0
          Else
            PrevCel = PrevCel + 1
          End If

        Case 1  'reverse
          If PrevCel = 0 Then
            PrevCel = ViewEdit.Loops(SelectedLoop).Cels.Count - 1
          Else
            PrevCel = PrevCel - 1
          End If
        Case 2  'end of loop
          If PrevCel = ViewEdit.Loops(SelectedLoop).Cels.Count - 1 Then
            'stop motion
            tmrMotion.Enabled = False
            'show play icon
            cmdVPlay.Picture = imlPreview.ListImages(9).Picture

          Else
            PrevCel = PrevCel + 1
          End If

        Case 3  'reverse loop
          If PrevCel = 0 Then
            'stop motion
            tmrMotion.Enabled = False
            'show play icon
            cmdVPlay.Picture = imlPreview.ListImages(9).Picture
          Else
            PrevCel = PrevCel - 1
          End If
        End Select

        'if not updating,
        If blnNoUpdate Then
          Exit Sub
        End If

        'display this cel preview
        DisplayPrevCel
      Exit Sub

      ErrHandler:
        '*'Debug.Assert False
        Resume Next
      End Sub

      Private Sub tlbPreview_ButtonClick(ByVal Button As MSComctlLib.Button)

        Select Case Button.Key
        Case "ZoomIn"
          ZoomPrev 1

        Case "ZoomOut"
          ZoomPrev -1

        Case "VAlign"
          'show valign toolbar
          tlbVAlign.Top = tlbPreview.Height / ScreenTWIPSY
          tlbVAlign.Visible = True

        Case "HAlign"
          tlbHAlign.Top = tlbPreview.Height / ScreenTWIPSY
          tlbHAlign.Visible = True

        End Select
      End Sub

      Private Sub tlbPreview_ButtonDropDown(ByVal Button As MSComctlLib.Button)

        Select Case Button.Key
        Case "VAlign"
          'show valign toolbar
          tlbVAlign.Top = tlbPreview.Height / ScreenTWIPSY
          tlbVAlign.Visible = True
        Case "HAlign"
          tlbHAlign.Top = tlbPreview.Height / ScreenTWIPSY
          tlbHAlign.Visible = True
        End Select
      End Sub
      Private Sub tlbPreview_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub

      Private Sub vsbPrevCel_Change()

        'if not updating
        If Not blnNoUpdate Then
          'position viewholder
          picPrevCel.Top = -vsbPrevCel.Value
        End If
      End Sub


      Private Sub vsbPrevCel_GotFocus()

        'set focus to view
        picPrevCel.SetFocus

        'ensure flyout toolbars are hidden
        tlbHAlign.Visible = False
        tlbVAlign.Visible = False
      End Sub


      Private Sub vsbPrevCel_Scroll()

        vsbPrevCel_Change
      End Sub

            */
        }

        #endregion

        public bool LoadView(Engine.View loadview) {
            InGame = loadview.InGame;
            if (InGame) {
                ViewNumber = loadview.Number;
            }
            else {
                // use a number that can never match
                // when searches for open views are made
                ViewNumber = 256;
            }
            try {
                loadview.Load();
            }
            catch {
                return false;
            }
            if (loadview.ErrLevel < 0) {
                return false;
            }
            EditView = loadview.Clone();
            if (!InGame && EditView.ID == "NewView") {
                ViewCount++;
                EditView.ID = "NewView" + ViewCount;
                IsChanged = true;
            }
            else {
                IsChanged = EditView.IsChanged || EditView.ErrLevel != 0;
            }
            Text = sVIEWED + ResourceName(EditView, InGame, true);
            if (IsChanged) {
                Text = sDM + Text;
            }
            mnuRSave.Enabled = !IsChanged;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = !IsChanged;
            // TODO: set up form for editing
            cmbLoop.Items.Clear();
            for (int i = 0; i < EditView.Loops.Count; i++) {
                cmbLoop.Items.Add($"Loop {i}");
            }
            cmbLoop.SelectedIndex = 0;

            //UpdateTree();
            //// set selected loop and cel to first cel of first loop
            //SelectedLoop = StartLoop;
            //SelectedCel = StartCel;
            //// if more than one cel in loop 0, start motion
            //if (EditView.Loops[0].Cels.Count > 1) {
            //    // show stop image
            //    cmdVPlay.Picture = imlPreview.ListImages[10].Picture;
            //}
            //if (ShowPreview) {
            //    InitPreview();
            //    DisplayPrevCel();
            //}
            //tmrMotion.Enabled = WinAGISettings.DefPrevPlay;
            //// display start cel
            //DisplayCel();

            return true;
        }

        public void ImportView(string importfile) {
            MDIMain.UseWaitCursor = true;
            Engine.View tmpView = new();
            try {
                tmpView.Import(importfile);
            }
            catch (Exception e) {
                //something wrong
                MDIMain.UseWaitCursor = false;
                ErrMsgBox(e, "Error while importing view:", "Unable to load this view resource.", "Import View Error");
                return;
            }
            // now check to see if it's a valid view resource (by trying to reload it)
            tmpView.Load();
            if (tmpView.ErrLevel < 0) {
                MDIMain.UseWaitCursor = false;
                ErrMsgBox(tmpView.ErrLevel, "Error reading View data:", "This is not a valid view resource.", "Invalid View Resource");
                //restore main form mousepointer and exit
                return;
            }
            // copy only the resource data
            EditView.ReplaceData(tmpView.Data);
            EditView.ResetView();
            MarkAsChanged();
            // TODO: redraw
            cmbLoop.Items.Clear();
            foreach (Loop tmpLoop in EditView.Loops) {
                cmbLoop.Items.Add($"Loop {tmpLoop.Index}");
            }
            cmbLoop.SelectedIndex = 0;
            MDIMain.UseWaitCursor = false;
        }

        public void SaveView() {
            if (InGame) {
                MDIMain.UseWaitCursor = true;
                bool blnLoaded = EditGame.Views[ViewNumber].Loaded;
                if (!blnLoaded) {
                    EditGame.Views[ViewNumber].Load();
                }
                EditGame.Views[ViewNumber].CloneFrom(EditView);
                EditGame.Views[ViewNumber].Save();
                EditView.CloneFrom(EditGame.Views[ViewNumber]);
                if (!blnLoaded) {
                    EditGame.Views[ViewNumber].Unload();
                }
                RefreshTree(AGIResType.View, ViewNumber);
                if (WinAGISettings.AutoExport.Value) {
                    EditView.Export(EditGame.ResDir + EditView.ID + ".agv");
                    // reset ID (non-game id gets changed by export...)
                    EditView.ID = EditGame.Views[ViewNumber].ID;
                }
                MarkAsSaved();
                MDIMain.UseWaitCursor = false;
            }
            else {
                if (EditView.ResFile.Length == 0) {
                    ExportView();
                    return;
                }
                else {
                    MDIMain.UseWaitCursor = true;
                    EditView.Save();
                    MarkAsSaved();
                    MDIMain.UseWaitCursor = false;
                }
            }
        }

        private void ExportView() {
            int retval = Base.ExportView(EditView, InGame);
            if (InGame) {
                // because EditView is not the actual ingame view its
                // ID needs to be reset back to the ingame value
                EditView.ID = EditGame.Views[ViewNumber].ID;
            }
            else {
                if (retval == 1) {
                    MarkAsSaved();
                }
            }
        }

        public void ToggleInGame() {
            DialogResult rtn;
            string strExportName;
            bool blnDontAsk = false;

            if (InGame) {
                if (WinAGISettings.AskExport.Value) {
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Do you want to export '" + EditView.ID + "' before removing it from your game?",
                        "Don't ask this question again",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        "Export View Before Removal", ref blnDontAsk);
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
                    // get a filename for the export
                    strExportName = NewResourceName(EditView, InGame);
                    if (strExportName.Length > 0) {
                        EditView.Export(strExportName);
                        //UpdateStatusBar();
                    }
                    break;
                case DialogResult.No:
                    // nothing to do
                    break;
                }
                // confirm removal
                if (WinAGISettings.AskRemove.Value) {
                    rtn = MsgBoxEx.Show(MDIMain,
                        "Removing '" + EditView.ID + "' from your game.\n\nSelect OK to proceed, or Cancel to keep it in game.",
                        "Remove View From Game",
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
                // remove the view (force-closes this editor)
                RemoveView((byte)ViewNumber);
            }
            else {
                // add to game 
                if (EditGame is null) {
                    return;
                }
                using frmGetResourceNum frmGetNum = new(GetRes.AddInGame, AGIResType.View, 0);
                if (frmGetNum.ShowDialog(MDIMain) != DialogResult.Cancel) {
                    ViewNumber = frmGetNum.NewResNum;
                    // change id before adding to game
                    EditView.ID = frmGetNum.txtID.Text;
                    AddNewView((byte)ViewNumber, EditView);
                    EditGame.Views[ViewNumber].Load();
                    // copy the view back (to ensure internal variables are copied)
                    EditView.CloneFrom(EditGame.Views[ViewNumber]);
                    // now we can unload the newly added view;
                    EditGame.Views[ViewNumber].Unload();
                    MarkAsSaved();
                    InGame = true;
                    MDIMain.toolStrip1.Items["btnAddRemove"].Image = MDIMain.imageList1.Images[20];
                    MDIMain.toolStrip1.Items["btnAddRemove"].Text = "Remove View";
                }
            }
        }

        public void RenumberView() {
            if (!InGame) {
                return;
            }
            string oldid = EditView.ID;
            int oldnum = ViewNumber;
            byte NewResNum = GetNewNumber(AGIResType.View, (byte)ViewNumber);
            if (NewResNum != ViewNumber) {
                // update ID (it may have changed if using default ID)
                EditView.ID = EditGame.Views[NewResNum].ID;
                ViewNumber = NewResNum;
                Text = sPICED + ResourceName(EditView, InGame, true);
                if (IsChanged) {
                    Text = sDM + Text;
                }
                if (EditView.ID != oldid) {
                    if (File.Exists(EditGame.ResDir + oldid + ".agp")) {
                        SafeFileMove(EditGame.ResDir + oldid + ".agp", EditGame.ResDir + EditGame.Views[NewResNum].ID + ".agp", true);
                    }
                }
            }
        }

        public void EditViewProperties(int FirstProp) {
            string id = EditView.ID;
            string description = EditView.Description;
            if (GetNewResID(AGIResType.View, ViewNumber, ref id, ref description, InGame, FirstProp)) {
                if (EditView.Description != description) {
                    EditView.Description = description;
                }
                if (EditView.ID != id) {
                    EditView.ID = id;
                    Text = sSNDED + ResourceName(EditView, InGame, true);
                    if (IsChanged) {
                        Text = sDM + Text;
                    }
                }
            }
        }

        private bool AskClose() {
            if (EditView.ErrLevel < 0) {
                // if exiting due to error on form load
                return true;
            }
            if (ViewNumber == -1) {
                // force shutdown
                return true;
            }
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to this view resource?",
                    "Save View Resource",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SaveView();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "Resource not saved. Continue closing anyway?",
                            "Save View Resource",
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

        private void MarkAsChanged() {
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
            Text = sSNDED + ResourceName(EditView, InGame, true);
            mnuRSave.Enabled = false;
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = false;
        }
    }
}
