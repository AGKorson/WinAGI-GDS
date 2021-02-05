using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Engine;
using static WinAGI.Engine.WinAGI;
using static WinAGI.Engine.AGIGame;
using static WinAGI.Editor.ResMan;

namespace WinAGI.Editor
{
  public partial class frmPicEdit : Form
  {
    Bitmap thisBMP;
    float zoom;
    bool picMode = false;

    public int PicNumber;
    public AGIPicture PicEdit;
    public bool InGame;
    public bool IsDirty;
    public EPicCursorMode CursorMode;

    public frmPicEdit()
    {
      InitializeComponent();
    }

    private void frmPicEdit_Load(object sender, EventArgs e)
    {
      //load combobox with AGI color indices
      cmbTransCol.Items.Add(AGIColors.agBlack);
      cmbTransCol.Items.Add(AGIColors.agBlue);
      cmbTransCol.Items.Add(AGIColors.agGreen);
      cmbTransCol.Items.Add(AGIColors.agCyan);
      cmbTransCol.Items.Add(AGIColors.agRed);
      cmbTransCol.Items.Add(AGIColors.agMagenta);
      cmbTransCol.Items.Add(AGIColors.agBrown);
      cmbTransCol.Items.Add(AGIColors.agLtGray);
      cmbTransCol.Items.Add(AGIColors.agDkGray);
      cmbTransCol.Items.Add(AGIColors.agLtBlue);
      cmbTransCol.Items.Add(AGIColors.agLtGreen);
      cmbTransCol.Items.Add(AGIColors.agLtCyan);
      cmbTransCol.Items.Add(AGIColors.agLtRed);
      cmbTransCol.Items.Add(AGIColors.agLtMagenta);
      cmbTransCol.Items.Add(AGIColors.agYellow);
      cmbTransCol.Items.Add(AGIColors.agWhite);
      cmbTransCol.Items.Add("None");
      cmbTransCol.SelectedIndex = 16;

      // load the picture
      EditGame.Pictures[1].Load();
      thisBMP = EditGame.Pictures[1].VisualBMP;
      // show it with NO transparency
      ShowAGIBitmap(picVisual, thisBMP);
    }

    private void trackBar1_Scroll(object sender, EventArgs e)
    {

      //resize our picture on the fly

      // convert trackbar value to a zoom factor
      zoom = (float)(trackBar1.Value / 2f + 1);

      // first, create new image in the picture box that is desired size
      //picVisual.Size = new Size((int)(320 * zoom), (int)(168 * zoom));
      picVisual.Width = (int)(320 * zoom);
      picVisual.Height = (int)(168 * zoom);

      showPic();
    }
    private void picVisual_Click(object sender, EventArgs e)
    {
      //swap visual/priority
      picMode = !picMode;
      showPic();
    }
    void showPic()
    {
      if (picMode)
      {
        thisBMP = (Bitmap)EditGame.Pictures[1].PriorityBMP.Clone();
      }
      else
      {
        thisBMP = (Bitmap)EditGame.Pictures[1].VisualBMP.Clone();
      }
      if (cmbTransCol.SelectedIndex < 16)
      {
        thisBMP.MakeTransparent(EditGame.EGAColor[cmbTransCol.SelectedIndex]);
      }
      ShowAGIBitmap(picVisual, thisBMP, zoom);

    }
    private void cmbTransCol_SelectionChangeCommitted(object sender, EventArgs e)
    {
      //redraw, with the selected transparent image
      showPic();
    }
    public bool EditPicture(AGIPicture ThisPicture)
    {
      return true;
      /*
  Dim strTemp() As String, strMsg As String
  
  On Error GoTo ErrHandler
  
  'set ingame flag based on picture passed
  InGame = ThisPicture.Resource.InGame
  
  'set number if this picture is in a game
  If InGame Then
    PicNumber = ThisPicture.Number
  Else
    'use a number that can never match
    'when searches for open pictures are made
    PicNumber = 256
  End If
  
  'create new picture object
  Set PicEdit = New AGIPicture
  
  'copy the passed picture to the editor picture
  PicEdit.SetPicture ThisPicture
  'get build error level (since entire picture is
  'loaded during SetPicture function)
  BMPBuildErr = PicEdit.BMPErrLevel
  
  'set caption and dirty flag
  If Not InGame And PicEdit.ID = "NewPicture" Then
    PicCount = PicCount + 1
    PicEdit.ID = "NewPicture" & CStr(PicCount)
    IsDirty = True
  Else
    IsDirty = PicEdit.IsDirty
  End If
  
  'NOTE- this will actually load the form; for now
  'we don't want any graphics stuff to run, so
  'the load method has to ignore things for now
  Caption = sPICED & ResourceName(PicEdit, InGame, True)
  
  If IsDirty Then
    MarkAsDirty
  Else
    frmMDIMain.mnuRSave.Enabled = False
    frmMDIMain.Toolbar1.Buttons("save").Enabled = False
  End If
  
  'populate cmd list with commands
  If Not LoadCmdList() Then
    'error- stop the form loading process
    MsgBox "This picture has corrupt or invalid data. Unable to open it for editing.", vbCritical + vbOKOnly, "Picture Data Error"
    PicEdit.Unload
    Set PicEdit = Nothing
    Exit Function
  End If
  
  'enable stepdrawing
  PicEdit.StepDraw = True
  
  'enable editing
  PicMode = pmEdit
  Toolbar1.Buttons("edit").Value = tbrPressed

  'check for a saved background image
  If Len(PicEdit.BkgdImgFile) <> 0 Then
    'try loading the background image
    On Error Resume Next
    Set BkgdImage = LoadPicture(PicEdit.BkgdImgFile)
    If Err.Number = 0 And Not (Me.BkgdImage Is Nothing) Then
      'get rest of parameters
      BkgdTrans = PicEdit.BkgdTrans
      strTemp = Split(PicEdit.BkgdSize, "|")
      tgtW = strTemp(0)
      tgtH = strTemp(1)
      srcW = strTemp(2)
      srcH = strTemp(3)
      strTemp = Split(PicEdit.BkgdPosition, "|")
      tgtX = strTemp(0)
      tgtY = strTemp(1)
      srcX = strTemp(2)
      srcY = strTemp(3)
      'validate a few things...
      If srcW <= 0 Or srcH <= 0 Then
        'reset
        srcW = MetsToPix(BkgdImage.Width)
        srcH = MetsToPix(BkgdImage.Height)
      End If
      If tgtW <= 0 Or tgtH <= 0 Then
        'reset
        tgtW = 320
        tgtH = 168
      End If
      If PicEdit.BkgdShow Then
        Toolbar1.Buttons("bkgd").Value = tbrPressed
      End If
    Else
      'if error is file not found, let user know
      If Err.Number = 76 Then
        strMsg = "Background file not found. "
      Else
        strMsg = "Error loading background image."
      End If
      Err.Clear
      ' inform user
      MsgBox strMsg & vbCrLf & vbCrLf & "The 'BkgdImg' property for this picture will be cleared.", vbInformation + vbOKOnly, "Picture Background Image Error"
      
      ' clear picedit background properties
      With PicEdit
        .BkgdImgFile = ""
        .BkgdPosition = ""
        .BkgdShow = False
        .BkgdSize = ""
        .BkgdTrans = 0
      End With
      
      ' clear ingame resource background properties
      With Pictures(PicNumber)
        .BkgdImgFile = ""
        .BkgdPosition = ""
        .BkgdShow = False
        .BkgdSize = ""
        .BkgdTrans = 0
      End With
      
      ' update the game wag file
      WriteProperty "Picture" & CStr(PicNumber), "BkgdImg", "", "Pictures"
      WriteProperty "Picture" & CStr(PicNumber), "BkgdPosn", ""
      WriteProperty "Picture" & CStr(PicNumber), "BkgdShow", ""
      WriteProperty "Picture" & CStr(PicNumber), "BkgdSize", ""
      ' force file to save after last property is changed
      WriteProperty "Picture" & CStr(PicNumber), "BkgdTrans", "", "", True
      
      'make sure image is nothing
      Set BkgdImage = Nothing
      'force background off
      PicEdit.BkgdShow = False
    End If
  End If
  
  'now we actually draw the picture; calls to DrawPicture
  'during the setup in above code don't do anything since
  'the pics aren't enabled yet; so we end with a final
  'call to DrawPicture and Force them to be redrawn
  DrawPicture True
  
  'return true
  EditPicture = True
Exit Function

ErrHandler:
  ErrMsgBox "Error while opening picture: ", "Unable to open picture for editing.", "Edit Picture Error"
  PicEdit.Unload
  Set PicEdit = Nothing
*/
    }

    void tmpPicForm()
    {
      /*

'three main picture controls used for managing vis and pri images:
' picVisual/picPriority
'      the control that holds the scaled image; this
'      is where the image is displayed and what user
'      interacts with directly
'
' picVisDraw/picPriDraw
'      this is the image on which the picture is drawn
'      unscaled; after drawing is complete, this image
'      is 'blitted' onto the main drawing control; it's
'      not visible to the user
'
' picVisSurface/picPriSurface
'      container that holds the image control; it's
'      the 'viewport' within which the scroll bars
'      move the main image control to show portion
'      of the image if too large to fit in the window
'

Private PicMode As EPicMode
Private UndoCol As Collection
Private NoSelect As Boolean

'variables to support tool selection/manipulation/use
Private SelectedCmd As Long 'must be the last cmd if multiples selected so the picdraw feature works correctly
Private OnPoint As Boolean
Private Anchor As PT, Delta As PT
Private CoordPT() As PT
Private CurPt As PT
Private SelStart As PT, SelSize As PT
Private CurrentPen As PenStatus
Private SelectedPen As PenStatus
Private CurCmdIsLine As Boolean
Private SelectedTool As TPicToolTypeEnum  'used to indicate what tool is currently selected
Private PicDrawMode As TPicDrawOpEnum   'used to indicate what current drawing op is happening (determines what mouse ops mean)
Private EditCmd As DrawFunction
Private EditCoordNum As Long
Private ArcPt(120) As PT
Private Segments As Long
Private CurCursor As EPicCur
Private VCColor As Long  'color of 'x's in 'x' cursor mode for visual
Private PCColor As Long  'color of 'x's in 'x' cursor mode for priority
Private gInsertBefore As Boolean 'to manage insertion of new commands
Private CodeClick As Boolean  'used to tell difference between lstCommand clicks
                              'called in code vs. those by actual clicks
Private Activating As Boolean

'variables to support graphics/display
Public BkgdImage As IPicture, BkgdTrans As Byte
Public tgtX As Single, tgtY As Single, tgtW As Single, tgtH As Single
Public srcX As Single, srcY As Single, srcW As Single, srcH As Single

Private MonoMask As VBitmap
Private BadBitmap As Boolean
Private BMPBuildErr As Long

Public ShowBands As Boolean, OldPri As Long
Private ShowSteps As Boolean
Public ScaleFactor As Long
Private blnWheel As Boolean
Private HFraction As Single, VFraction As Single

Private blnDragging As Boolean, blnInPri As Boolean
Private sngOffsetX As Single, sngOffsetY As Long

Private SplitOffset As Long
Private Const SPLIT_HEIGHT = 4  'in pixels
Private Const SPLIT_WIDTH = 4  'in pixels
Private Const MIN_SPLIT_V = 100 'in pixels
Private Const MAX_SPLIT_V = 225 'in pixels

Private CalcWidth As Long, CalcHeight As Long
Private Const MIN_HEIGHT = 361
Private Const MIN_WIDTH = 200

Private SplitRatio As Single, Squished As Boolean
Private OneWindow As Long '0=both; 1=vis only; 2=pri only
Private PrevState As Long

'variables to support testing
Private TestView As AGIView
Private TestViewNum As Long, TestViewFile As String
Private CurTestLoop As Byte, CurTestCel As Byte
Private CurTestLoopCount As Byte

Private TestDir As ObjDirection
Private TestCelData() As AGIColors
Private CelHeight As Byte, CelWidth As Byte
Private CelTrans As AGIColors
Private StopReason As Long
Private OldCel As PT
Private StatusSrc As Boolean
Private TestSettings As TPicTest

Private SBPic As Long

'constants and Type declarations
Private Const PE_MARGIN As Long = 5

Private Sub AddCoordToList(ByVal bytX As Byte, ByVal bytY As Byte, ByVal CmdPos As Long, Optional ByVal Prefix As String = "", Optional ByVal InsertPos As Long = -1)

  On Error GoTo ErrHandler
  
  With lstCoords
  
    'add to listbox (normally at end, but if a position is passed, insert it there
    If InsertPos = -1 Then
      .AddItem Prefix & CoordText(bytX, bytY)
    Else
      .AddItem Prefix & CoordText(bytX, bytY), InsertPos
    End If
    .ItemData(.NewIndex) = CmdPos
    
    'add to coord list (make sure there is room)
    '(always add to end - order doesn't matter)
    If .ListCount - 1 > UBound(CoordPT) Then
      ReDim Preserve CoordPT(UBound(CoordPT) + 100)
    End If
    CoordPT(.ListCount - 1).X = bytX
    CoordPT(.ListCount - 1).Y = bytY
  End With
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub ClearCoordList()

  'clear lstCoord, clear and reset coordpt list
  
  lstCoords.Clear
  ReDim CoordPT(100)
  
  'always set CurPt to impossible value so clicking any coord will reselect correctly
  CurPt.X = 255
  CurPt.Y = 255
End Sub


Private Function ConfigureBackground() As Boolean

  'shows background configuration form (which will automatically show
  'the loadimage dialog if no Image loaded yet)
  
  On Error GoTo ErrHandler
  
  Set frmConfigureBkgd.PicEditForm = Me
  'initialize the form (will get a bkgd Image if there isn't one yet)
  frmConfigureBkgd.InitForm
  
  'if no bkgd Image (i.e. user canceled), just exit
  If frmConfigureBkgd.Canceled Then
    Unload frmConfigureBkgd
    ConfigureBackground = False
    Exit Function
  End If
  
  'now set canceled flag (it's the default)
  frmConfigureBkgd.Canceled = True
  
  'show the form
  frmConfigureBkgd.Show vbModal, frmMDIMain
  
  'do something with it...
  If Not frmConfigureBkgd.Canceled Then
    'copy bkgd Image and filename
    Set BkgdImage = frmConfigureBkgd.BkgdImage
    
    'save the bkgd parameters
    With frmConfigureBkgd
      'local values used in draw functions
      tgtX = .tgtX
      tgtY = .tgtY
      tgtW = .tgtW
      tgtH = .tgtH
      srcX = .srcX
      srcY = .srcY
      srcW = .srcW
      srcH = .srcH
      BkgdTrans = .BkgdTrans
      ' now update the picture resource properties
      PicEdit.BkgdImgFile = .BkgdImgFile
      PicEdit.BkgdTrans = CLng(BkgdTrans)
      PicEdit.BkgdSize = tgtW & "|" & tgtH & "|" & srcW & "|" & srcH
      PicEdit.BkgdPosition = tgtX & "|" & tgtY & "|" & srcX & "|" & srcY
    End With

    'if in game
    If InGame Then
      'copy properties back to actual picture resource
      With Pictures(PicEdit.Number)
        .BkgdImgFile = PicEdit.BkgdImgFile
        .BkgdPosition = PicEdit.BkgdPosition
        .BkgdShow = PicEdit.BkgdShow
        .BkgdSize = PicEdit.BkgdSize
        .BkgdTrans = PicEdit.BkgdTrans
        ' save it (this will only write the properties
        ' since the real picture is not being edited
        ' in this piceditor)
        .Save
      End With
    End If
    
    'return true
    ConfigureBackground = True
  Else
    'return false
    ConfigureBackground = False
  End If
  
  'unload the form
  Unload frmConfigureBkgd
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub ExportPicAsGif()

  'export a loop as a gif
  
  Dim blnCanceled As Boolean, rtn As Long
  Dim PGOptions As GifOptions
  
  On Error GoTo ErrHandler
  
  'show options form
  Load frmViewGifOptions
  With frmViewGifOptions
    'set up form to export this picture
    .InitForm 1, PicEdit
    .Show vbModal, frmMDIMain
    blnCanceled = .Canceled
    
    'if not canceled, get a filename
    If Not blnCanceled Then
    
      'set up commondialog
      With MainSaveDlg
        .DialogTitle = "Export Picture GIF"
        .DefaultExt = "gif"
        .Filter = "GIF files (*.gif)|*.gif|All files (*.*)|*.*"
        .Flags = cdlOFNHideReadOnly Or cdlOFNPathMustExist Or cdlOFNExplorer
        .FilterIndex = 1
        .FullName = ""
        .hWndOwner = frmMDIMain.hWnd
      End With
      
      Do
        On Error Resume Next
        MainSaveDlg.ShowSaveAs
        'if canceled,
        If Err.Number = cdlCancel Then
          'cancel the export
          blnCanceled = True
          Exit Do
        End If
        
        'if file exists,
        If FileExists(MainSaveDlg.FullName) Then
          'verify replacement
          rtn = MsgBox(MainSaveDlg.FileName & " already exists. Do you want to overwrite it?", vbYesNoCancel + vbQuestion, "Overwrite file?")
          
          If rtn = vbYes Then
            Exit Do
          ElseIf rtn = vbCancel Then
            blnCanceled = True
            Exit Do
          End If
        Else
          Exit Do
        End If
      Loop While True
      On Error GoTo ErrHandler
    End If
    
    'if NOT canceled after getting filename, then export!
    If Not blnCanceled Then
      'show progress form
      Load frmProgress
      With frmProgress
        .Caption = "Exporting Picture as GIF"
        .lblProgress = "Depending in size of picture, this may take awhile. Please wait..."
        .pgbStatus.Max = PicEdit.Resource.Size
        .pgbStatus.Value = 0
        .pgbStatus.Visible = True
        .Show
        .Refresh
      End With
      
      'show wait cursor
      WaitCursor
      
      PGOptions.Cycle = (.chkLoop.Value = vbChecked)
      PGOptions.Delay = Val(.txtDelay.Text)
      PGOptions.Zoom = Val(.txtZoom.Text)
      
      'set options
      MakePicGif PicEdit, PGOptions, MainSaveDlg.FullName
      
      'all done!
      Unload frmProgress
      MsgBox "Success!", vbInformation + vbOKOnly, "Export Picture as GIF"
      
      Screen.MousePointer = vbDefault
    End If
    
    'done with the options form
    Unload frmViewGifOptions
    
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub GetZoomCenter()

  'only called by zoom feature if a scrollwheel event
  'occurs
  
  Dim rtn As Long
  Dim mPos As POINTAPI
  
  'get the cursor position and figure out if it's over picVisual or picPriority
  rtn = GetCursorPos(mPos)
  rtn = WindowFromPoint(mPos.X, mPos.Y)
  
  'if it's NOT over one of those two, then set fraction values to 1/2
  Select Case rtn
  Case picVisual.hWnd
    'convert to client coordinates
    rtn = ScreenToClient(rtn, mPos)
    
    'correct for scroll position
    If hsbVis.Visible Then
      mPos.X = mPos.X - hsbVis.Value
    End If
    'and return fraction of visSurface
    HFraction = mPos.X / picVisSurface.Width
    
    'correct for scroll position
    If vsbVis.Visible Then
      mPos.Y = mPos.Y - vsbVis.Value
      VFraction = mPos.Y / picVisSurface.Height
    End If
    'and return fraction of visSurface
    VFraction = mPos.Y / picVisSurface.Height
  
  Case picPriority.hWnd
    'convert to client coordinates
    rtn = ScreenToClient(rtn, mPos)
    
    'correct for scroll position
    If hsbPri.Visible Then
      mPos.X = mPos.X - hsbPri.Value
    End If
    'and return fraction of priSurface
    HFraction = mPos.X / picPriSurface.Width
    
    If vsbPri.Visible Then
      'correct for any scroll values
      mPos.Y = mPos.Y - vsbPri.Value
    End If
    'and return fraction of priSurface
    VFraction = mPos.Y / picPriSurface.Height
    
  Case Else
    HFraction = 0.5
    VFraction = 0.5
  End Select
  
End Sub

Private Sub HighlightCoords()

  Dim i As Long, lngCount As Long
  Dim tmpPT As PT, LineType As DrawFunction
  Dim cOfX As Single, cOfY As Single, cSzX As Single, cSzY As Single
  
  On Error GoTo ErrHandler
  
  'if using original highlight mode OR if not in edit mode, just exit
  If CursorMode = pcmWinAGI Or SelectedTool <> ttEdit Then
    Exit Sub
  End If
  
  cOfX = 1.5 / ScaleFactor ^ 0.5
  cOfY = cOfX * 2 '3 / ScaleFactor ^ 0.5
  cSzX = cOfX * 2 '3 / ScaleFactor ^ 0.5
  cSzY = cOfY * 2 '6 / ScaleFactor ^ 0.5
  
  'if any coords are in the list highlight them
  
  'lines need all coords highlighted; plots and fills need only highlight up to selected coord
  'get Type of line command
  LineType = PicEdit.Resource.Data(lstCommands.ItemData(SelectedCmd))
  If lstCoords.ListIndex >= 0 And (LineType = dfFill Or LineType = dfPlotPen) Then
    lngCount = lstCoords.ListIndex
  Else
    lngCount = lstCoords.ListCount - 1
  End If
  
  'is this selected coord?
  If lstCoords.ListIndex <> -1 Then
    tmpPT = ExtractCoordinates(lstCoords.Text)
  Else
    'set X to invalid value so it'll never match
    tmpPT.X = 255
  End If
      
  If lngCount >= 0 Then
    For i = 0 To lngCount
      If CoordPT(i).X = tmpPT.X And CoordPT(i).Y = tmpPT.Y Then
        'draw a box
        If SelectedPen.VisColor < agNone Then
          picVisual.Line ((CoordPT(i).X + 0.5 - cOfX / 2) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 + cOfY / 2) * ScaleFactor)-Step((cSzX + 0.15) / 2 * ScaleFactor * 2, (-cSzY - 0.3) / 2 * ScaleFactor), VCColor, B
        End If
        If SelectedPen.PriColor < agNone Then
          picPriority.Line ((CoordPT(i).X + 0.5 - cOfX / 2) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 + cOfY / 2) * ScaleFactor)-Step((cSzX + 0.15) / 2 * ScaleFactor * 2, (-cSzY - 0.3) / 2 * ScaleFactor), VCColor, B
        End If
      Else
        'highlight this coord with an X
        If SelectedPen.VisColor < agNone Then
          picVisual.Line ((CoordPT(i).X + 0.5 - cOfX) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 + cOfY) * ScaleFactor)-Step((cSzX + 0.15) * ScaleFactor * 2, (-cSzY - 0.3) * ScaleFactor), VCColor
          picVisual.Line ((CoordPT(i).X + 0.5 - cOfX) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 - cOfY) * ScaleFactor)-Step((cSzX + 0.15) * ScaleFactor * 2, (cSzY + 0.3) * ScaleFactor), VCColor
        End If
        If SelectedPen.PriColor < agNone Then
          picPriority.Line ((CoordPT(i).X + 0.5 - cOfX) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 + cOfY) * ScaleFactor)-Step((cSzX + 0.15) * ScaleFactor * 2, (-cSzY - 0.3) * ScaleFactor), PCColor
          picPriority.Line ((CoordPT(i).X + 0.5 - cOfX) * ScaleFactor * 2, (CoordPT(i).Y + 0.5 - cOfY) * ScaleFactor)-Step((cSzX + 0.15) * ScaleFactor * 2, (cSzY + 0.3) * ScaleFactor), PCColor
        End If
      End If
    Next i
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickInsert()

On Error GoTo ErrHandler

  'insert coord without starting a move action
  InsertCoordinate False

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next

End Sub

Public Sub MenuClickReplace()

  'toggle test view mode
  
  On Error GoTo ErrHandler
  
  'if currently in test mode
  If PicMode = pmTest Then
    'switch back to edit
    SetMode pmEdit
  Else
    'if no test view,
    If TestView Is Nothing Then
      'get one
      GetTestView
    
      'if still no test view
      If TestView Is Nothing Then
        'just exit
        Exit Sub
      End If
    End If
    
    'switch to test mode
    SetMode pmTest
  End If

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next

End Sub

Public Sub MenuClickHelp()
  
  On Error GoTo ErrHandler
  
  'help
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Picture_Editor.htm"
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub Activate()
  'bridge method to call the form's Activate event method
  Form_Activate
End Sub

Private Sub BuildCoordList(ByVal ListPos As Long)

  'build coord list, based on selected cmd
  
  Dim bytCmd As Byte
  Dim bytData() As Byte
  Dim lngPos As Long, lngNode As Long
  Dim bytX As Byte, bytY As Byte
  Dim xdisp As Long, ydisp As Long
  Dim blnRelX As Boolean, PatternNum As Byte
  Dim strBrush As String, blnSplat As Boolean
  
  On Error GoTo ErrHandler
  
  'clear coords list
  ClearCoordList
  
  'if more than one selected item
  If lstCommands.SelCount > 1 Then
    Exit Sub
  End If
  
  'if end selected or nothing selected,
  If SelectedCmd = lstCommands.ListCount - 1 Or lstCommands.ListIndex = -1 Then
    Exit Sub
  End If
  
  'get data (for better speed)
  bytData = PicEdit.Resource.AllData
  
  'set starting pos for the selected cmd
  lngPos = lstCommands.ItemData(ListPos)
  
  'get command Type
  bytCmd = bytData(lngPos)
  
  'add command based on Type
  Select Case bytCmd
  Case &HF0, &HF1, &HF2, &HF3, &H9 'pen functions; no coords.
    
  Case &HF4, &HF5 'Draw an X or Y corner.
    'set initial direction
    blnRelX = (bytCmd = &HF5)
    'get coordinates
    lngPos = lngPos + 1
    bytX = bytData(lngPos)
    If bytX >= &HF0 Then
      Exit Sub
    End If
    lngPos = lngPos + 1
    bytY = bytData(lngPos)
    If bytX >= &HF0 Then
      Exit Sub
    End If
    
    'add start (adjust by 1 so first byte of coordinate is stored as position)
    AddCoordToList bytX, bytY, lngPos - 1

    'get next byte as potential command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
    
    Do Until bytCmd >= &HF0
      If blnRelX Then
        bytX = bytCmd
      Else
        bytY = bytCmd
      End If
      blnRelX = Not blnRelX
      
      'add coordinate node, and set position
      AddCoordToList bytX, bytY, lngPos
      
      'get next coordinate or command
      lngPos = lngPos + 1
      bytCmd = bytData(lngPos)
    Loop
   
  Case &HF6 'Absolute line (long lines).
    'get coordinates
    lngPos = lngPos + 1
    bytX = bytData(lngPos)
    If bytX >= &HF0 Then
      Exit Sub
    End If
    lngPos = lngPos + 1
    bytY = bytData(lngPos)
    If bytY >= &HF0 Then
      Exit Sub
    End If
    
    'add start (adjust by 1 so first byte of coordinate is stored as position)
    AddCoordToList bytX, bytY, lngPos - 1
    
    'get next byte as potential command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
    
    Do Until bytCmd >= &HF0
      bytX = bytCmd
      lngPos = lngPos + 1
      bytY = bytData(lngPos)
      
      'add coordinate node, and set position
      AddCoordToList bytX, bytY, lngPos - 1
      
      'read in next command
      lngPos = lngPos + 1
      bytCmd = bytData(lngPos)
    Loop
    
  Case &HF7 'Relative line (short lines).
     'get coordinates
    lngPos = lngPos + 1
    bytX = bytData(lngPos)
    If bytX >= &HF0 Then
      Exit Sub
    End If
    lngPos = lngPos + 1
    bytY = bytData(lngPos)
    If bytY >= &HF0 Then
      Exit Sub
    End If
    
    'add start (adjust by 1 so first byte of coordinate is stored as position)
    AddCoordToList bytX, bytY, lngPos - 1
    
    'get next byte as potential command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
    
    Do Until bytCmd >= &HF0
      'if horizontal negative bit set
      If (bytCmd And &H80) Then
        xdisp = -((bytCmd And &H70) / &H10)
      Else
        xdisp = ((bytCmd And &H70) / &H10)
      End If
      'if vertical negative bit is set
      If (bytCmd And &H8) Then
        ydisp = -(bytCmd And &H7)
      Else
        ydisp = (bytCmd And &H7)
      End If
      bytX = bytX + xdisp
      bytY = bytY + ydisp
      
      'add coordinate node, and set position
      AddCoordToList bytX, bytY, lngPos
      
      'read in next command
      lngPos = lngPos + 1
      bytCmd = bytData(lngPos)
    Loop
   
 Case &HF8 'Fill.
    'get next byte as potential command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
    
    Do Until bytCmd >= &HF0
      'get coordinates
      bytX = bytCmd
      lngPos = lngPos + 1
      bytY = bytData(lngPos)
      
      'add coord
      AddCoordToList bytX, bytY, lngPos - 1
      
      'read in next command
      lngPos = lngPos + 1
      bytCmd = bytData(lngPos)
    Loop
    
  Case &HFA 'Plot with pen.
    'get next byte as potential command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
    Do Until bytCmd >= &HF0
      'if brush is splatter
      If CurrentPen.PlotStyle Then
        PatternNum = CLng(bytCmd \ 2)
''        If PatternNum > 119 Then
''        'this is never possible! bytCmd will ALWAYS be <240, so bytCmd\2 will never be >119
''          'treat as pattern number 1
''          PatternNum = 1
''        End If
        'strBrush = "[Pattern " & CStr(PatternNum) & "] "
        strBrush = CStr(PatternNum) & " -- "
        'get next byte
        lngPos = lngPos + 1
        bytCmd = bytData(lngPos)
        'set offset to 2 (to account for pattern number and x coord)
        xdisp = 2
      Else
        strBrush = vbNullString
        'set offset to 1 (to account for x coord)
        xdisp = 1
      End If

      'get coordinates
      bytX = bytCmd
      lngPos = lngPos + 1
      bytY = bytData(lngPos)
      'add coord
      AddCoordToList bytX, bytY, lngPos - xdisp, strBrush
      
      'read in next command
      lngPos = lngPos + 1
      bytCmd = bytData(lngPos)
    Loop
  End Select
  
  'highlight the coords, if in edit mode
  If PicMode = pmEdit Then
    HighlightCoords
  End If
Exit Sub

ErrHandler:
  'if it's due to being past end of resource, just ignore it
  If lngPos > UBound(bytData) Then
    Err.Clear
    If PicMode = pmEdit Then
      HighlightCoords
    End If
    Exit Sub
  End If
  
  'error- let calling method deal with it
  '*'Debug.Assert False
  'by returning false
End Sub

Private Sub ComparePoints(CmpStart As PT, CmpEnd As PT, ByVal bytX As Byte, ByVal bytY As Byte)

  'adjusts cmpstart and cmpend based on bytx and byty values
  If bytX < CmpStart.X Then
    CmpStart.X = bytX
  End If
  If bytY < CmpStart.Y Then
    CmpStart.Y = bytY
  End If
  If bytX > CmpEnd.X Then
    CmpEnd.X = bytX
  End If
  If bytY > CmpEnd.Y Then
    CmpEnd.Y = bytY
  End If
End Sub

Private Sub FlipCmds(ByVal FlipCmd As Long, ByVal Count As Long, ByVal Axis As Long, Optional ByVal DontUndo As Boolean = False)

  'flips the selected commands
  'axis=0 means horizontal flip
  'axis=1 means vertical flip
  
  Dim i As Long, bytDelta As Byte, lngPos As Long
  Dim NextUndo As PictureUndo, bytData() As Byte
  Dim blnX As Boolean, lngOldPos As Long
  Dim bytX As Byte, bytY As Byte
  Dim tmpStyle As EPlotStyle
  
  On Error GoTo ErrHandler
  
  'select the cmds ???why isn't this already done????
  GetSelectionBounds SelectedCmd, Count, True
  
  'bounding rectangle should always be defined
  '*'Debug.Assert SelSize.X <> 0 Or SelSize.Y <> 0
  
  'get current pen status for first command
  tmpStyle = GetPenStyle(lstCommands.ItemData(SelectedCmd - Count + 1))
  
  'local copy of data (for speed)
  bytData = PicEdit.Resource.AllData
  
  'if horizontal flip:
  If Axis = 0 Then
    'step through each cmd
    For i = Count To 1 Step -1
      lngPos = lstCommands.ItemData(FlipCmd - i + 1)
      
      'each cmd handles flip differently
      Select Case bytData(lngPos)
      Case dfRelLine
        'increment position marker
        lngPos = lngPos + 1
        
        'when flipping relative lines horizontally, need to flip the actual order
        'of coordinates; this ensures that we avoid situations where the flipped
        'line creates data bytes that the interpreter might confuse as commands
        '(remember that the delta x offset is the four highest bits of the data
        'byte; bit 7 is set if the delta is negative; bits 6-5-4 determine Value;
        'if the delta amount is -7, this means the data byte will be >=&HF0; this
        'is read by the interpreter as a new cmd; not as a delt of -7; so for
        'rel lines, the delta x Value is limited to -6; when flipping, we can't just
        'flip the first coord, then change the direction of the x-delta values;
        'there may be some +7 delta values that will result in errors when converted
        'to -7 delta x values
        
        'solution is to build the command backwards; start with the LAST point in
        'the command; then build the line backwards to finish the swap
        
        'if at least one valid coordinate
        If bytData(lngPos) < &HF0 Then
          'determine ending point
          bytX = bytData(lngPos)
          bytY = bytData(lngPos + 1)
          
          'set pointer to next delta Value
          lngPos = lngPos + 2
          
          Do Until bytData(lngPos) >= &HF0
            'add deltax
            If bytData(lngPos) And &H80 Then
              bytX = bytX - (CLng(bytData(lngPos)) And &H70) / &H10
            Else
              bytX = bytX + (CLng(bytData(lngPos)) And &H70) / &H10
            End If
            
            'add deltay
            If bytData(lngPos) And &H8 Then
              bytY = bytY - (CLng(bytData(lngPos)) And &H7)
            Else
              bytY = bytY + (CLng(bytData(lngPos)) And &H7)
            End If
            
              
            'get next delta Value
            lngPos = lngPos + 1
          Loop
          
          'flip the x Value of the end point (which will now be the start point)
          bytX = 2 * CLng(SelStart.X) + SelSize.X - bytX - 1
          
          'save ending point position (remember to back up one unit)
          lngOldPos = lngPos - 1
          'restore original pointer (remember to skip cmd valus so pos points
          'to the first coordinate pair)
          lngPos = lstCommands.ItemData(FlipCmd - i + 1) + 1
          'store this point
          bytData(lngPos) = bytX
          bytData(lngPos + 1) = bytY
          'now move pointer to first delta Value
          lngPos = lngPos + 2
          
          'now rebuild cmds, starting with last command, and going backwards
          'because the cmds are being built in reverse, the delta x and delta y
          'values should be inverted (+ to - and - to +), BUT because cmd is being
          'flipped,  we only invert the y direction, resulting in the x
          'direction being flipped properly
          
          '(also, because cmds have to be re-built backwards, we access the delta
          'values from the picedit object when reconstructing the delta values)
          
          Do
            'copy the delta Value from picdata to bytdata
            bytData(lngPos) = PicEdit.Resource.Data(lngOldPos)
            'if the delta y Value is currently negative
            If (bytData(lngPos) And &H8) Then
              'clear the bit to make the direction positive
              bytData(lngPos) = bytData(lngPos) And &HF7
            
            'if the delta y Value is currently positive (or mayber zero?)
            Else
              'if the delta Value is not zero
              If bytData(lngPos) And &H7 Then
                'set the bit
                bytData(lngPos) = bytData(lngPos) Or &H8
              End If
            End If
            
            'get next delta Value
            lngPos = lngPos + 1
            lngOldPos = lngOldPos - 1
          Loop Until bytData(lngPos) >= &HF0
        End If
        
      Case dfAbsLine, dfFill
        'each pair of coordinates are adjusted for flip
        lngPos = lngPos + 1
        Do Until bytData(lngPos) >= &HF0
          If bytData(lngPos) < &HF0 And bytData(lngPos + 1) < &HF0 Then
            bytData(lngPos) = 2 * CLng(SelStart.X) + SelSize.X - bytData(lngPos) - 1
          Else
            'end found
            Exit Do
          End If
          'get next cmd pair
          lngPos = lngPos + 2
        Loop
        
      Case dfPlotPen
        'each pair of coordinates are adjusted for flip
        lngPos = lngPos + 1
        
        Do Until bytData(lngPos) >= &HF0
          'if pen is splatter
          If tmpStyle = psSplatter Then
            'skip first byte; its the splatter Value
            lngPos = lngPos + 1
          End If
        
          If bytData(lngPos) < &HF0 And bytData(lngPos + 1) < &HF0 Then
            bytData(lngPos) = 2 * CLng(SelStart.X) + SelSize.X - bytData(lngPos) - 1
          Else
            'end found
            Exit Do
          End If
          
          'get next cmd pair
          lngPos = lngPos + 2
        Loop
        
      Case dfXCorner, dfYCorner
        'if this is a 'x' corner, then next coord is a 'x' Value
        '(make sure to check this BEFORE incrementing lngPos)
        blnX = (bytData(lngPos) = dfXCorner)
          
        'move pointer to first coordinate pair
        lngPos = lngPos + 1
        
        'if a valid coordinatee
        If bytData(lngPos) < &HF0 Then
          'flip first coordinate
          bytData(lngPos) = 2 * CLng(SelStart.X) + SelSize.X - bytData(lngPos) - 1
          
          'move pointer to next coordinate point
          lngPos = lngPos + 2
          
          Do Until bytData(lngPos) >= &HF0
            'if this is a 'x' point
            If blnX Then
              'flip it
              bytData(lngPos) = 2 * CLng(SelStart.X) + SelSize.X - bytData(lngPos) - 1
            End If
            
            'toggle next coord Type
            blnX = Not blnX
            'increment pointer
            lngPos = lngPos + 1
          Loop
        End If
      Case dfChangePen
        tmpStyle = (bytData(lngPos + 1) And &H20) / &H20
      End Select
    Next i
  
  Else
    'step through each cmd
    For i = 1 To Count
      lngPos = lstCommands.ItemData(FlipCmd - i + 1)
      
      'each cmd handles flip differently
      Select Case bytData(lngPos)
      Case dfRelLine
        'when flipping the y axis, we don't need to worry about
        'the swap causing errors in the delta values; all we need
        'to do is just swap the first coordinate, and then change the
        'y direction of all delta values
        
        'increment position marker
        lngPos = lngPos + 1
        
        If bytData(lngPos) < &HF0 Then
          'flip the y Value of starting point
          bytData(lngPos + 1) = 2 * CLng(SelStart.Y) + SelSize.Y - bytData(lngPos + 1) - 1
          'increment lngpos  (by two so first relative pt data byte is selected)
          lngPos = lngPos + 2
        End If
        
        Do Until bytData(lngPos) >= &HF0
          'toggle direction bit for y displacement
          
          'if the delta y Value is currently negative
          If (bytData(lngPos) And &H8) Then
            'clear the bit to make the direction positive
            bytData(lngPos) = bytData(lngPos) And &HF7
          
          'if the delta y Value is currently positive (or mayber zero?)
          Else
            'if the delta Value is not zero
            If bytData(lngPos) And &H7 Then
              'set the bit
              bytData(lngPos) = bytData(lngPos) Or &H8
            End If
          End If
          
          'neyt byte
          lngPos = lngPos + 1
        Loop
        
      Case dfAbsLine, dfFill
        'each pair of coordinates are adjusted for flip
        lngPos = lngPos + 1
        Do Until bytData(lngPos) >= &HF0
          If bytData(lngPos) < &HF0 And bytData(lngPos + 1) < &HF0 Then
            bytData(lngPos + 1) = 2 * CLng(SelStart.Y) + SelSize.Y - bytData(lngPos + 1) - 1
          Else
            'end found
            Exit Do
          End If
          'get neyt cmd pair
          lngPos = lngPos + 2
        Loop
        
      Case dfPlotPen
        'each pair of coordinates are adjusted for flip
        lngPos = lngPos + 1
        
        Do Until bytData(lngPos) >= &HF0
          'if pen is splatter
          If tmpStyle = psSplatter Then
            'skip first byte; its the splatter Value
            lngPos = lngPos + 1
          End If
        
          If bytData(lngPos) < &HF0 And bytData(lngPos + 1) < &HF0 Then
            bytData(lngPos + 1) = 2 * CLng(SelStart.Y) + SelSize.Y - bytData(lngPos + 1) - 1
          Else
            'end found
            Exit Do
          End If
          
          'get next cmd pair
          lngPos = lngPos + 2
        Loop
        
      Case dfYCorner, dfXCorner
        'if this is a 'y' corner, then next coord is a 'y' Value
        '(make sure to check this BEFORE incrementing lngPos)
        blnX = bytData(lngPos) = dfXCorner
          
        'move pointer to first coordinate pair
        lngPos = lngPos + 1
        
        'if a valid coordinatee
        If bytData(lngPos) < &HF0 Then
          'flip first coordinate
          bytData(lngPos + 1) = 2 * CLng(SelStart.Y) + SelSize.Y - bytData(lngPos + 1) - 1
          
          'move pointer to next coordinate point
          lngPos = lngPos + 2
          
          Do Until bytData(lngPos) >= &HF0
            'if this is a 'y' point
            If Not blnX Then
              'flip it
              bytData(lngPos) = 2 * CLng(SelStart.Y) + SelSize.Y - bytData(lngPos) - 1
            End If
            
            'toggle next coord Type
            blnX = Not blnX
            'increment pointer
            lngPos = lngPos + 1
          Loop
        End If
        
      Case dfChangePen
        tmpStyle = (bytData(lngPos + 1) And &H20) / &H20
      End Select
    Next i
  End If
  
  'copy data back to resource
  PicEdit.Resource.SetData bytData
    
  'if not skipping undo
  If Not DontUndo And Settings.PicUndo <> 0 Then
    Set NextUndo = New PictureUndo
    With NextUndo
      If Axis = 0 Then
        .UDAction = udpFlipH
      Else
        .UDAction = udpFlipV
      End If
      .UDCmdIndex = FlipCmd
      .UDCoordIndex = Count
    End With
    AddUndo NextUndo
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function GetPenStyle(ByVal lngPos As Long) As EPlotStyle
  
  'determines pen status for a given position
  
  Dim bytData() As Byte, i As Long
  
  On Error GoTo ErrHandler
  
  'default is solid
  GetPenStyle = psSolid
  
  'local copy of data for speed
  bytData = PicEdit.Resource.AllData
  
  For i = 0 To lngPos
    If bytData(i) = dfChangePen Then
      'set new pen status
      GetPenStyle = (bytData(i + 1) And &H20) / &H20
    End If
  Next i
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub GetSelectionBounds(ByVal SelCmd As Long, ByVal SelCount As Long, Optional ByVal ShowBox As Boolean = False)

  'determines the starting (upper left corner) and the
  'size of selected cmds, and sets selection box to match
  'optionally draws a selection box around the commands
  
  Dim i As Long, lngPos As Long
  Dim tmpPlotStyle As EPlotStyle
  Dim bytX As Byte, bytY As Byte
  Dim xdisp As Long, ydisp As Long
  Dim blnRelX As Boolean, blnSplat As Boolean
  Dim bytData() As Byte
  Dim SelEnd As PT
  
  On Error GoTo ErrHandler
  
  'set start to lower right (to force it to update)
  SelStart.X = 159
  SelStart.Y = 167
  'set end to upper left (to force it to update)
  SelEnd.X = 0
  SelEnd.Y = 0
  
  'now go through each cmd; check it for coordinates
  'if coordinates are found, step through them to
  'determine if any coords expand the selected area
  
  'NOTE: for plots, need to be aware of pen status
  'so coordinate values get extracted correctly
  tmpPlotStyle = GetPenStyle(lstCommands.ItemData(SelCmd - SelCount + 1))
  
  'work with data locally (improves speed)
  bytData = PicEdit.Resource.AllData
  
  For i = SelCount - 1 To 0 Step -1
    'set starting pos for this cmd
    lngPos = lstCommands.ItemData(SelCmd - i)
    'parse coords based on cmdtype
    Select Case bytData(lngPos)
    'Case dfEnableVis, dfDisableVis, dfEnablePri, dfDisablePri
      'ignore cmds that have no coordinates
    Case dfChangePen
      'need to check for change in plot style
      tmpPlotStyle = (bytData(lngPos + 1) And &H20) / &H20
      
    Case dfYCorner, dfXCorner
      'set initial direction
      blnRelX = (bytData(lngPos) = &HF5)
      
      Do
        'get coordinates
        lngPos = lngPos + 1
        bytX = bytData(lngPos)
        If bytX >= &HF0 Then
          Exit Do
        End If
        lngPos = lngPos + 1
        bytY = bytData(lngPos)
        If bytX >= &HF0 Then
          Exit Do
        End If
        
        'compare to start/end
        ComparePoints SelStart, SelEnd, bytX, bytY
        
        'get next byte as potential command
        lngPos = lngPos + 1
        
        Do Until bytData(lngPos) >= &HF0
          If blnRelX Then
            bytX = bytData(lngPos)
          Else
            bytY = bytData(lngPos)
          End If
          blnRelX = Not blnRelX
          
          'compare to start/end
          ComparePoints SelStart, SelEnd, bytX, bytY
          
          'get next coordinate or command
          lngPos = lngPos + 1
        Loop
      Loop Until bytData(lngPos) >= &HF0
     
    Case dfAbsLine, dfFill
      'get next byte as potential command
      lngPos = lngPos + 1
      
      Do Until bytData(lngPos) >= &HF0
        'get coordinates
        bytX = bytData(lngPos)
        lngPos = lngPos + 1
        bytY = bytData(lngPos)
        
        'compare to start/end
        ComparePoints SelStart, SelEnd, bytX, bytY
        
        'read in next command
        lngPos = lngPos + 1
      Loop
      
    Case dfRelLine
      Do
        'get coordinates
        lngPos = lngPos + 1
        bytX = bytData(lngPos)
        If bytX >= &HF0 Then
          Exit Sub
        End If
        lngPos = lngPos + 1
        bytY = bytData(lngPos)
        If bytY >= &HF0 Then
          Exit Sub
        End If
        
        'compare to start/end
        ComparePoints SelStart, SelEnd, bytX, bytY
        
        'get next byte as potential command
        lngPos = lngPos + 1
        bytData(lngPos) = bytData(lngPos)
        
        Do Until bytData(lngPos) >= &HF0
          'if horizontal negative bit set
          If (bytData(lngPos) And &H80) Then
            xdisp = -((bytData(lngPos) And &H70) / &H10)
          Else
            xdisp = ((bytData(lngPos) And &H70) / &H10)
          End If
          'if vertical negative bit is set
          If (bytData(lngPos) And &H8) Then
            ydisp = -(bytData(lngPos) And &H7)
          Else
            ydisp = (bytData(lngPos) And &H7)
          End If
          bytX = bytX + xdisp
          bytY = bytY + ydisp
          
          'compare to start/end
          ComparePoints SelStart, SelEnd, bytX, bytY
          
          'read in next command
          lngPos = lngPos + 1
        Loop
      Loop Until bytData(lngPos) >= &HF0
   
    Case dfPlotPen
      'get next byte as potential command
      lngPos = lngPos + 1
      
      Do Until bytData(lngPos) >= &HF0
        'if brush is splatter
        If tmpPlotStyle Then
          'get next byte
          lngPos = lngPos + 1
        End If
        
        'get coordinates
        bytX = bytData(lngPos)
        lngPos = lngPos + 1
        bytY = bytData(lngPos)
        
        'compare to start/end
        ComparePoints SelStart, SelEnd, bytX, bytY
        
        'read in next command
        lngPos = lngPos + 1
      Loop
    End Select
  Next i
  
  'if no cmds found that have coordinates
  '(can determine this by checking if start is greater than end)
  If SelStart.X > SelEnd.X Then
    SelSize.X = 0
    SelSize.Y = 0
  Else
    'convert end values into height/width
    SelSize.X = SelEnd.X - SelStart.X + 1
    SelSize.Y = SelEnd.Y - SelStart.Y + 1
  End If
  
  'if optionally drawing selection box around the commands,
  If ShowBox Then
    ShowCmdSelection
  End If

Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub GetTestView()
  
  'get a test view to use in test mode
  
  On Error GoTo ErrHandler
  
  'if game is loaded
  If GameLoaded Then
    'use the get resource form
    With frmGetResourceNum
      .WindowFunction = grTestView
      .ResType = rtView
      .OldResNum = TestViewNum
      'setup before loading so ghosts don't show up
      .FormSetup
      'show the form
      .Show vbModal, frmMDIMain
    
      'if canceled, unload and exit
      If .Canceled Then
        Unload frmGetResourceNum
        Exit Sub
      End If
    
      'set testview id
      TestViewNum = .NewResNum
    End With
    Unload frmGetResourceNum
    
  Else
    'get test view from file
    With MainDialog
      .DialogTitle = "Choose Test View"
      .Filter = "AGI View Resource (*.agv)|*.agv|All files (*.*)|*.*"
      .FilterIndex = GameSettings.GetSetting(sVIEWS, sOPENFILTER, 1)
      .DefaultExt = vbNullString
      .FileName = vbNullString
      .InitDir = DefaultResDir
      
      On Error Resume Next
      .ShowOpen
      If Err.Number = cdlCancel Then
        'exit
        Exit Sub
      End If
      
      On Error GoTo ErrHandler
      TestViewFile = .FileName
      
      WriteSetting GameSettings, sVIEWS, sOPENFILTER, .FilterIndex
      DefaultResDir = JustPath(.FileName)
    End With
  End If
  
  'reload testview
  LoadTestView
  
  'if in motion
  If TestDir <> odStopped Then
    'stop motion
    TestDir = odStopped
    tmrTest.Enabled = TestSettings.CycleAtRest
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickCustom2()

  'toggles background Image
  ToggleBkgd Not PicEdit.BkgdShow
End Sub
Public Sub MenuClickSelectAll()

  'selects all commands if in edit mode,
  'or select entire picture if in edit-select mode
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'in test mode, this should be disabled, but...
  If PicMode = pmTest Then
    Exit Sub
  End If
  
  'if editselect tool is chosen, change selection to cover entire area
  Select Case SelectedTool
  Case ttSelectArea
    SelStart.X = 0
    SelStart.Y = 0
    SelSize.X = 160
    SelSize.Y = 168
    'now show the selection
    ShowCmdSelection
    
  'Case ttSetPen
    'not used
  Case ttLine, ttRelLine, ttCorner
    'not sure what I should do if this is case?
    '*'Debug.Assert False
  Case ttPlot, ttFill
    'not sure what I should do if this is case?
    '*'Debug.Assert False
  Case ttRectangle, ttTrapezoid, ttEllipse
    'not sure what I should do if this is case?
    '*'Debug.Assert False
  Case ttEdit
    'if nothing to select
    If lstCommands.ListCount = 1 Then
      Exit Sub
    End If
    
    'disable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
    
    'select all cmds in the cmd list (except 'END' place holder)
    SelectedCmd = lstCommands.ListCount - 2
    
    For i = lstCommands.ListCount - 2 To 0 Step -1
      NoSelect = True
      lstCommands.Selected(i) = True
    Next i
    NoSelect = False
    
    'reenable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
    
    'force update
    DrawPicture
  
    'if more than one cmd (account for END placeholder)
    If lstCommands.ListCount > 2 Then
      'get bounds, and select the cmds
      GetSelectionBounds lstCommands.ListCount - 2, lstCommands.ListCount - 1, True
    End If
  End Select
Exit Sub

ErrHandler:

  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub MoveCmds(ByVal MoveCmd As Long, ByVal Count As Long, ByVal DeltaX As Long, ByVal DeltaY As Long, Optional ByVal DontUndo As Boolean = False)

  Dim NextUndo As PictureUndo
  Dim i As Long, bytData() As Byte
  Dim lngPos As Long, bytCmd As Byte
  Dim blnX As Boolean
  Dim CurPen As EPlotStyle, FirstCmd As Long
  
  On Error GoTo ErrHandler
  
  'if more than one command selected, MoveCmd is the LAST command in the group of selected commands!
  FirstCmd = MoveCmd - Count + 1
  
  'if no delta
  If DeltaX = 0 And DeltaY = 0 Then
    Exit Sub
  End If
  
  'local copy of data (for speed)
  bytData = PicEdit.Resource.AllData
  
  'we need to know the pen style in case a plot command is being moved
  'and make sure we get FIRST command, not the last one
  CurPen = GetPenStyle(lstCommands.ItemData(MoveCmd - Count + 1))
  
  'step through each cmd
  For i = 1 To Count
    lngPos = lstCommands.ItemData(FirstCmd + i - 1)
    
    'each cmd handles move differently
    Select Case bytData(lngPos)
    Case dfRelLine
      'only first pt needs to be changed
      If bytData(lngPos + 1) < &HF0 And bytData(lngPos + 2) < &HF0 Then
        bytData(lngPos + 1) = bytData(lngPos + 1) + DeltaX
        bytData(lngPos + 2) = bytData(lngPos + 2) + DeltaY
      End If
      
    Case dfAbsLine, dfFill
      'each pair of coordinates are adjusted for offset
      lngPos = lngPos + 1
      Do Until bytData(lngPos) >= &HF0
        If bytData(lngPos) < &HF0 And bytData(lngPos + 1) < &HF0 Then
          bytData(lngPos) = bytData(lngPos) + DeltaX
          bytData(lngPos + 1) = bytData(lngPos + 1) + DeltaY
        Else
          'end found
          Exit Do
        End If
        'get next cmd pair
        lngPos = lngPos + 2
      Loop
      
    Case dfChangePen
      'need to make sure we keep up with any plot style changes
        'get pen size and style
        lngPos = lngPos + 1
        
        If (bytData(lngPos) And &H20) / &H20 = 0 Then
          'solid
          CurPen = psSolid
        Else
          CurPen = psSplatter
        End If
  
        'get next command
        lngPos = lngPos + 1
        bytCmd = bytData(lngPos)
        
    Case dfPlotPen
      'each group of coordinates are adjusted for offset
      lngPos = lngPos + 1
      Do Until bytData(lngPos) >= &HF0
        'if splattering, skip the splatter code
        If CurPen = psSplatter Then
          lngPos = lngPos + 1
        End If
        
        If bytData(lngPos) < &HF0 And bytData(lngPos + 1) < &HF0 Then '? didn't we already check that in the 'do' statement?
          bytData(lngPos) = bytData(lngPos) + DeltaX
          bytData(lngPos + 1) = bytData(lngPos + 1) + DeltaY
        Else
          'end found
          Exit Do
        End If
        'get next cmd pair
        lngPos = lngPos + 2
      Loop
      
      
      
    Case dfXCorner, dfYCorner
      'if this is a 'x' corner, then next coord is a 'x' Value
      '(make sure to check this BEFORE incrementing lngPos)
      blnX = bytData(lngPos) = dfXCorner
        
      'move pointer to first coordinate pair
      lngPos = lngPos + 1
      
      'if a valid coordinatee
      If bytData(lngPos) < &HF0 Then
        'move first coordinate
        bytData(lngPos) = bytData(lngPos) + DeltaX
        bytData(lngPos + 1) = bytData(lngPos + 1) + DeltaY
        
        'move pointer to next coordinate point
        lngPos = lngPos + 2
        
        Do Until bytData(lngPos) >= &HF0
          'if this is a 'x' point
          If blnX Then
            'add delta x
            bytData(lngPos) = bytData(lngPos) + DeltaX
          Else
            'add delta y
            bytData(lngPos) = bytData(lngPos) + DeltaY
          End If
          'toggle next coord Type
          blnX = Not blnX
          'increment pointer
          lngPos = lngPos + 1
        Loop
      End If
    End Select
  Next i
  
  'copy data back to resource
  PicEdit.Resource.SetData bytData
  
  'add undo (if necessary)
  If Not DontUndo And Settings.PicUndo <> 0 Then
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpMoveCmds
      .UDCmdIndex = MoveCmd
      .UDCoordIndex = Count
      .UDText = CStr(-1 * DeltaX) & "|" & CStr(-1 * DeltaY)
    End With
    AddUndo NextUndo
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Function RelLineCoord(ByVal CoordPos As Long) As PT

  'returns the relative line coordinate for the relative line
  'at CoordPos
  
  Dim bytData() As Byte, bytCmd As Byte
  Dim lngPos As Long, tmpPT As PT
  Dim bytX As Byte, bytY As Byte
  Dim xdisp As Long, ydisp As Long
  
  On Error GoTo ErrHandler
  
  'get data (for better speed)
  bytData = PicEdit.Resource.AllData
  
  'find start by stepping backwards until relline cmd is found
  lngPos = CoordPos
  Do Until bytData(lngPos - 1) = dfRelLine
    lngPos = lngPos - 1
  Loop
  
  'get coordinates
  bytX = bytData(lngPos)
  lngPos = lngPos + 1
  bytY = bytData(lngPos)
  
  'get next byte as potential command
  lngPos = lngPos + 1
  bytCmd = bytData(lngPos)
  
  Do Until lngPos > CoordPos
    'if horizontal negative bit set
    If (bytCmd And &H80) Then
      xdisp = -((bytCmd And &H70) / &H10)
    Else
      xdisp = ((bytCmd And &H70) / &H10)
    End If
    'if vertical negative bit is set
    If (bytCmd And &H8) Then
      ydisp = -(bytCmd And &H7)
    Else
      ydisp = (bytCmd And &H7)
    End If
    bytX = bytX + xdisp
    bytY = bytY + ydisp
    
    'read in next command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
  Loop
  
  'return this point
  tmpPT.X = bytX
  tmpPT.Y = bytY
  RelLineCoord = tmpPT
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function


Private Function GetVerticalStatus(ByVal CmdPos As Long) As Boolean
  'will detrmine if a step line is currently drawing vertical
  
  Dim bytData() As Byte
  Dim lngPos As Long, bytCmd As Byte
  
  On Error GoTo ErrHandler
  
  'get data (for better speed)
  bytData = PicEdit.Resource.AllData
  
  'set starting pos for the selected cmd
  lngPos = CmdPos
  
  'get command Type
  bytCmd = bytData(lngPos)
  
  If bytCmd = &HF4 Then
    GetVerticalStatus = True
  End If
  
  'flop vert status for first point
  GetVerticalStatus = Not GetVerticalStatus

  'skip first coordinates
  lngPos = lngPos + 3
  
  'get next byte as potential command
  bytCmd = bytData(lngPos)
    
  Do Until bytCmd >= &HF0
    
    'flop vert status for each point
    GetVerticalStatus = Not GetVerticalStatus
    
    'get next coordinate or command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
  Loop
   
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Public Function MatchPoints(ByVal ListPos As Long) As Boolean

  'returns true if coord data for previous command's last coord equals this commands first coord
  
  Dim bytCmd As Byte
  Dim bytData() As Byte
  Dim lngPos As Long, tmpCoord As PT
  Dim bytX As Byte, bytY As Byte
  Dim xdisp As Long, ydisp As Long
  Dim blnRelX As Boolean
  
  On Error GoTo ErrHandler
  
  '*'Debug.Assert ListPos > 0
  
  'if no coord for this command
  If LenB(lstCoords.List(0)) = 0 Then
    MatchPoints = False
    Exit Function
  End If
  
  'get data (for better speed)
  bytData = PicEdit.Resource.AllData
  
  'set starting pos for the previous cmd
  lngPos = lstCommands.ItemData(ListPos - 1)
  
  'get command Type
  bytCmd = bytData(lngPos)
  '*'Debug.Assert bytCmd >= &HF4 And bytCmd <= &HF7
  
  'add command based on Type
  Select Case bytCmd
  Case &HF4, &HF5 'Draw an X or Y corner.
    'set initial direction
    blnRelX = (bytCmd = &HF5)
    'get coordinates
    lngPos = lngPos + 1
    bytX = bytData(lngPos)
    lngPos = lngPos + 1
    bytY = bytData(lngPos)
    
    'get next byte as potential command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
    
    Do Until bytCmd >= &HF0
      If blnRelX Then
        bytX = bytCmd
      Else
        bytY = bytCmd
      End If
      blnRelX = Not blnRelX
      
      'get next coordinate or command
      lngPos = lngPos + 1
      bytCmd = bytData(lngPos)
    Loop
   
  Case &HF6 'Absolute line (long lines).
    'get coordinates
    lngPos = lngPos + 1
    bytX = bytData(lngPos)
    lngPos = lngPos + 1
    bytY = bytData(lngPos)
    
    'get next byte as potential command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
    
    Do Until bytCmd >= &HF0
      bytX = bytCmd
      lngPos = lngPos + 1
      bytY = bytData(lngPos)
      
      'read in next command
      lngPos = lngPos + 1
      bytCmd = bytData(lngPos)
    Loop
    
  Case &HF7 'Relative line (short lines).
     'get coordinates
    lngPos = lngPos + 1
    bytX = bytData(lngPos)
    lngPos = lngPos + 1
    bytY = bytData(lngPos)
    
    'get next byte as potential command
    lngPos = lngPos + 1
    bytCmd = bytData(lngPos)
    
    Do Until bytCmd >= &HF0
      'if horizontal negative bit set
      If (bytCmd And &H80) Then
        xdisp = -((bytCmd And &H70) / &H10)
      Else
        xdisp = ((bytCmd And &H70) / &H10)
      End If
      'if vertical negative bit is set
      If (bytCmd And &H8) Then
        ydisp = -(bytCmd And &H7)
      Else
        ydisp = (bytCmd And &H7)
      End If
      bytX = bytX + xdisp
      bytY = bytY + ydisp
      
      'read in next command
      lngPos = lngPos + 1
      bytCmd = bytData(lngPos)
    Loop
   
  End Select
  
  'bytx and byty are now set to last coord of previous cmd
  
  'extract coords from this cmd's first node text
  tmpCoord = ExtractCoordinates(lstCoords.List(0))
  
  MatchPoints = (tmpCoord.X = bytX And tmpCoord.Y = bytY)
Exit Function

ErrHandler:
  '*'Debug.Assert False
  'error- let calling method deal with it
  'by returning false
End Function

Public Sub MenuClickFind()

  'toggles the priority bands on and off
  
  Dim rtn As Long
  
  On Error GoTo ErrHandler
  
  'toggle showband flag
  ShowBands = Not ShowBands
  
  'use Force flag so both images
  'get updated whether currently visible
  'or not
  If PicMode = pmEdit Then
    'redraw
    DrawPicture True
  Else
    'redraw cel
    DrawPicture True, True, OldCel.X, OldCel.Y
  End If
    
  'reset caption
  With frmMDIMain.mnuEFind
    If ShowBands Then
      .Caption = "Hide Priority Bands" & vbTab & "Alt+P"
    Else
      .Caption = "Show Priority Bands" & vbTab & "Alt+P"
    End If
  End With
    
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickFindAgain()

  'allow user to set the priority base,if v2.936 or above
  'OR if not in a game!
  
  Dim lngNewBase As String, lngOldBase As Long
  Dim NextUndo As PictureUndo
  
  lngOldBase = PicEdit.PriBase
  lngNewBase = lngOldBase
  Do
    lngNewBase = InputBox("Enter new priority base value: ", "Set Priority Base", lngNewBase, frmMDIMain.Left + (frmMDIMain.Width - 4500) / 2, frmMDIMain.Top + (frmMDIMain.Height - 2100) / 2)
    
    'if canceled, it will be empty string
    If lngNewBase = vbNullString Then
      Exit Sub
    End If
    
    'validate
    If Not IsNumeric(lngNewBase) Then
      'invalid
      MsgBoxEx "You must enter a value between 0 and 158", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Invalid Base Value", WinAGIHelp, "htm\winagi\Picture_Editor.htm#pribands"
    ElseIf lngNewBase < 0 Or lngNewBase > 158 Then
      'invalid
      MsgBoxEx "You must enter a value between 0 and 158", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Invalid Base Value", WinAGIHelp, "htm\winagi\Picture_Editor.htm#pribands"
    Else
      'OK!
      Exit Do
    End If
  Loop While True
  
  'set new pri base
  PicEdit.PriBase = lngNewBase
  DrawPicture True
  
  'add undo!
  If Settings.PicUndo <> 0 Then
    'create undo object
    Set NextUndo = New PictureUndo
    NextUndo.UDAction = udpSetPriBase
    'use cmdIndex for old base
    NextUndo.UDCmdIndex = lngOldBase
    'add the undo object without setting edit menu
    UndoCol.Add NextUndo
  End If
  
  MarkAsDirty
  
End Sub

Public Sub MenuClickRedo()

  'when showing full visual or full priority,
  'this menu item swaps between the two
  
  Dim sngSplit As Single
  
  On Error GoTo ErrHandler
  
  ' if currently showing visual
  If OneWindow = 1 Then
    'switch to priority
    OneWindow = 2
    sngSplit = 0
  ElseIf OneWindow = 2 Then
    'otherwise switch to visual
    OneWindow = 1
    sngSplit = picPalette.Top - picSplitH.Height
  End If
  
  'update and redraw to affect change
  UpdatePanels sngSplit, lstCommands.Left + lstCommands.Width
  DrawPicture
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub AddPatternData(tmpIndex As Long, bytPatDat() As Byte, Optional ByVal DontUndo As Boolean = False)

  'add pattern bytes to tmpIndex command coordinates
  'if skipping undo, pattern values will be
  'passed in bytPatDat; if not skipping undo
  'generate random pattern data

  Dim i As Long, lngNewPos As Long
  Dim bytPattern As Byte
  Dim NextUndo As PictureUndo
  
  On Error GoTo ErrHandler
  
  'set insertpos so first iteration will add
  'pattern data in front of first coord x Value
  lngNewPos = lstCommands.ItemData(tmpIndex) - 2
  
  Do
    'if skipping undo
    If DontUndo Then
      'if first byte of array is 255,
      If bytPatDat(0) = &HFF Then
        'need to provide the random bytes for this set of coordinates
        bytPattern = 2 * CByte(Int(Rnd * 119))
      Else
        'get pattern from array
        bytPattern = bytPatDat(i)
      End If
    Else
      'get random pattern
      bytPattern = 2 * CByte(Int(Rnd * 119))
    End If
    
    'adjust pos (include offset
    lngNewPos = lngNewPos + 3
    
    'add it to resource
    PicEdit.Resource.InsertData bytPattern, lngNewPos
    
    'increment byte insertion counter
    i = i + 1
  Loop Until PicEdit.Resource.Data(lngNewPos + 3) >= &HF0
  
  'adjust positions (i equals number of bytes added)
  UpdatePosValues tmpIndex + 1, i
  
  'if not skipping undo
  If Not DontUndo And Settings.PicUndo <> 0 Then
    'save undo info
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpAddPlotPattern
      .UDPicPos = lstCommands.ItemData(tmpIndex)
      .UDCmdIndex = tmpIndex
    End With
    'add the undo object without setting edit menu
    UndoCol.Add NextUndo
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub BeginDraw(ByVal CurrentTool As TPicToolTypeEnum, PicPt As PT)
  'initiates draw operation based on selected tool
  
  Dim bytData() As Byte
  Dim bytPattern As Byte
  
  On Error GoTo ErrHandler
  
  'begin drawing using selected tool
  Select Case CurrentTool
  Case ttLine
    'set anchor
    Anchor = PicPt
    'set data to draw command and first point
    ReDim bytData(2)
    bytData(0) = dfAbsLine
    bytData(1) = PicPt.X
    bytData(2) = PicPt.Y
    'insert command
    InsertCommand bytData(), SelectedCmd, "Abs Line", gInsertBefore
    'select this cmd
    SelectCmd lstCommands.NewIndex, False
    'now set mode (do it AFTER selecting command otherwise
    'draw mode will get canceled)
    PicDrawMode = doLine
    'and select first coordinate
    NoSelect = True
    lstCoords.ListIndex = 0
    
  Case ttRelLine
    'insert rel line cmd
    
    'set anchor
    Anchor = PicPt
    'set data to draw command and first point
    ReDim bytData(2)
    bytData(0) = dfRelLine
    bytData(1) = PicPt.X
    bytData(2) = PicPt.Y
    'insert command
    InsertCommand bytData(), SelectedCmd, "Rel Line", gInsertBefore
    'select this cmd
    SelectCmd lstCommands.NewIndex, False
    'now set mode (do it AFTER selecting command otherwise
    'draw mode will get canceled)
    PicDrawMode = doLine
    'and select first coordinate
    NoSelect = True
    lstCoords.ListIndex = 0
    
  Case ttCorner
    'set anchor
    Anchor = PicPt
    'set data to draw command and first point
    ReDim bytData(2)
    'assume xcorner
    bytData(0) = dfXCorner
    bytData(1) = PicPt.X
    bytData(2) = PicPt.Y
    'insert command
    InsertCommand bytData(), SelectedCmd, "X Corner", gInsertBefore
    SelectCmd lstCommands.NewIndex, False
    'now set mode (do it AFTER selecting command otherwise
    'draw mode will get canceled)
    PicDrawMode = doLine
    'and select first coordinate
    NoSelect = True
    lstCoords.ListIndex = 0
  
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub ChangeColor(ByVal CmdIndex As Long, ByVal NewColor As AGIColors, Optional ByVal DontUndo As Boolean = False)
  
  'changes the color for this command
  Dim NextUndo As PictureUndo
  Dim OldColor As AGIColors
  Dim bytData() As Byte
  Dim lngPos As Long
  
  On Error GoTo ErrHandler
  
  ReDim bytData(0)
  
  'get position of command
  lngPos = lstCommands.ItemData(CmdIndex)
  
  'get color of current command
  If Right$(lstCommands.List(CmdIndex), 3) = "Off" Then
    OldColor = agNone
  Else
    OldColor = PicEdit.Resource.Data(lngPos + 1)
  End If
  
  'it is possible that a change request is made
  'even though colors are the same
  If OldColor = NewColor Then
    'just exit
    Exit Sub
  End If
  
  'if not skipping undo
  If Not DontUndo And Settings.PicUndo <> 0 Then
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpChangeColor
      .UDPicPos = lngPos
      .UDCmdIndex = CmdIndex
      bytData(0) = OldColor
      .UDData = bytData()
    End With
    AddUndo NextUndo
  End If
  
  'if old color is none
  If OldColor = agNone Then
    'change command to enable by subtracting one
    PicEdit.Resource.Data(lngPos) = PicEdit.Resource.Data(lngPos) - 1
    'insert color
    PicEdit.Resource.InsertData CByte(NewColor), CLng(lngPos) + 1
    'update all following commands
    UpdatePosValues CmdIndex + 1, 1
    
    'build command text
    lstCommands.List(CmdIndex) = Left$(lstCommands.List(CmdIndex), 5) & LoadResString(COLORNAME + NewColor)
    
  ElseIf NewColor = agNone Then
    'change command to disable by adding one
    PicEdit.Resource.Data(lngPos) = PicEdit.Resource.Data(lngPos) + 1
    'delete color byte
    PicEdit.Resource.RemoveData lngPos + 1
    'update all following commands
    UpdatePosValues CmdIndex + 1, -1
    'build command text
    lstCommands.List(CmdIndex) = Left$(lstCommands.List(CmdIndex), 5) & "Off"
  Else
    'change color byte
    PicEdit.Resource.Data(lngPos + 1) = NewColor
    'build command text
    lstCommands.List(CmdIndex) = Left$(lstCommands.List(CmdIndex), 5) & LoadResString(COLORNAME + NewColor)
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub ChangeDir(ByVal KeyCode As Integer)
  
  'should ONLY be called when in test mode
  '*'Debug.Assert PicMode = pmTest
  
  'takes a keycode as the input, and changes direction if appropriate
  Select Case KeyCode
  Case vbKeyUp, vbKeyNumpad8
    'if view is on picture
    '(OldCel.X will not be -1)
    If OldCel.X <> -1 Then
      'if direction is currently up
      If TestDir = odUp Then
        'stop movement
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
      Else
        'set direction to up
        TestDir = odUp
        'set loop to 3, if there are four AND loop is not 3 AND in auto
        If TestView.Loops.Count >= 4 And CurTestLoop <> 3 And (TestSettings.TestLoop = -1) Then
          CurTestLoop = 3
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          CurTestCel = 0
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        End If
        'enable timer
        tmrTest.Enabled = True
      End If
    End If
    
  Case vbKeyPageUp, vbKeyNumpad9
    'if view is on picture
    '(OldCel.X will not be -1)
    If OldCel.X <> -1 Then
      'if direction is currently UpRight
      If TestDir = odUpRight Then
        'stop movement
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
      Else
        'set direction to upright
        TestDir = odUpRight
        'set loop to 0, if not already 0 AND in auto
        If CurTestLoop <> 0 And (TestSettings.TestLoop = -1) Then
          CurTestLoop = 0
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          CurTestCel = 0
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        End If
        'enable timer
        tmrTest.Enabled = True
      End If
    End If
    
  Case vbKeyRight, vbKeyNumpad6
    'if view is on picture
    '(OldCel.X will not be -1)
    If OldCel.X <> -1 Then
      'if direction is currently Right
      If TestDir = odRight Then
        'stop movement
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
      Else
        'set direction to right
        TestDir = odRight
        'set loop to 0, if not already 0 AND in auto
        If CurTestLoop <> 0 And (TestSettings.TestLoop = -1) Then
          CurTestLoop = 0
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          CurTestCel = 0
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        End If
        'enable timer
        tmrTest.Enabled = True
      End If
    End If
    
  Case vbKeyPageDown, vbKeyNumpad3
    'if view is on picture
    '(OldCel.X will not be -1)
    If OldCel.X <> -1 Then
      'if direction is currently DownRight
      If TestDir = odDownRight Then
        'stop movement
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
      Else
        'set direction to downright
        TestDir = odDownRight
        'set loop to 0, if not already 0 AND in auto
        If CurTestLoop <> 0 And (TestSettings.TestLoop = -1) Then
          CurTestLoop = 0
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          CurTestCel = 0
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        End If
        'enable timer
        tmrTest.Enabled = True
      End If
    End If
    
  Case vbKeyDown, vbKeyNumpad2
    'if view is on picture
    '(OldCel.X will not be -1)
    If OldCel.X <> -1 Then
      'if direction is currently down
      If TestDir = odDown Then
        'stop movement
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
      Else
        'set direction to down
        TestDir = odDown
        'set loop to 2, if there are four AND loop is not 2 AND in auto
        If TestView.Loops.Count >= 4 And CurTestLoop <> 2 And (TestSettings.TestLoop = -1) Then
          CurTestLoop = 2
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          CurTestCel = 0
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        End If
        'enable timer
        tmrTest.Enabled = True
      End If
    End If
    
  Case vbKeyEnd, vbKeyNumpad1
    'if view is on picture
    '(OldCel.X will not be -1)
    If OldCel.X <> -1 Then
      'if direction is currently DownLeft
      If TestDir = odDownLeft Then
        'stop movement
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
      Else
        'set direction to downLeft
        TestDir = odDownLeft
        'set loop to 1, if  at least 2 loops, and not already 1 AND in auto
        If (TestView.Loops.Count >= 2) And (CurTestLoop <> 1) And (TestSettings.TestLoop = -1) Then
          CurTestLoop = 1
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          CurTestCel = 0
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        End If
        'enable timer
        tmrTest.Enabled = True
      End If
    End If
    
  Case vbKeyLeft, vbKeyNumpad4
    'if view is on picture
    '(OldCel.X will not be -1)
    If OldCel.X <> -1 Then
      'if direction is currently Left
      If TestDir = odLeft Then
        'stop movement
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
      Else
        'set direction to Left
        TestDir = odLeft
        'set loop to 1, if  at least 2 loops, and not already 1 AND in auto
        If (TestView.Loops.Count >= 2) And (CurTestLoop <> 1) And (TestSettings.TestLoop = -1) Then
          CurTestLoop = 1
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          CurTestCel = 0
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        End If
        'enable timer
        tmrTest.Enabled = True
      End If
    End If
    
  Case vbKeyHome, vbKeyNumpad7
    'if view is on picture
    '(OldCel.X will not be -1)
    If OldCel.X <> -1 Then
      'if direction is currently UpLeft
      If TestDir = odUpLeft Then
        'stop movement
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
      Else
        'set direction to UpLeft
        TestDir = odUpLeft
        'set loop to 1, if  at least 2 loops, and not already 1 AND in auto
        If (TestView.Loops.Count >= 2) And (CurTestLoop <> 1) And (TestSettings.TestLoop = -1) Then
          CurTestLoop = 1
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          CurTestCel = 0
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        End If
        'enable timer
        tmrTest.Enabled = True
      End If
    End If
    
  Case vbKeyNumpad5
    'always stop
    TestDir = 0
    tmrTest.Enabled = TestSettings.CycleAtRest
    
  End Select
End Sub


Private Sub DeleteCommand(DelIndex As Long, Optional ByVal DontUndo As Boolean = False)

  'delete an entire command
    
  Dim DelCount As Long
  Dim DelPos As Long
  Dim i As Long
  Dim NextUndo As PictureUndo
  Dim bytUndoData() As Byte
  
  On Error GoTo ErrHandler
  
  'get starting position
  DelPos = lstCommands.ItemData(DelIndex)
  
  'calculate bytes to delete
  DelCount = lstCommands.ItemData(DelIndex + 1) - DelPos
  
  'if not skipping undo
  If Not DontUndo And Settings.PicUndo <> 0 Then
    'create new undo object
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpDelCmd
      .UDPicPos = DelPos
      .UDCmdIndex = DelIndex
      .UDCmd = lstCommands.List(DelIndex)
    
      ReDim bytUndoData(DelCount - 1)
      For i = 0 To (DelCount - 1)
        bytUndoData(i) = PicEdit.Resource.Data(DelPos + i)
      Next i
      .UDData = bytUndoData
      .UDCoordIndex = 1
    End With
    'add to undo
    AddUndo NextUndo
  End If
  
  'remove from resource
  PicEdit.Resource.RemoveData DelPos, DelCount
  
  'adjust position values to account for deleted data
  UpdatePosValues DelIndex, -DelCount
  
  'remove from cmd list
  lstCommands.RemoveItem DelIndex
  
  'select cmd at delindex position
  SelectCmd DelIndex, DontUndo
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub AddUndo(NextUndo As PictureUndo)

  If Not IsDirty Then
    MarkAsDirty
  End If
  
  'remove old undo items until there is room for this one
  'to be added
  If Settings.PicUndo > 0 Then
    Do While UndoCol.Count >= Settings.PicUndo
      UndoCol.Remove 1
    Loop
  End If
  
  'adds the next undo object
  UndoCol.Add NextUndo
  
  'set undo menu
  frmMDIMain.mnuEUndo.Enabled = True
  frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(PICUNDOTEXT + NextUndo.UDAction) & NextUndo.UDCmd & vbTab & "Ctrl+Z"
End Sub



Private Sub MarkAsDirty()

  If Not IsDirty Then
    IsDirty = True
  
    'enable menu and toolbar button
    frmMDIMain.mnuRSave.Enabled = True
    frmMDIMain.Toolbar1.Buttons("save").Enabled = True
  
    '*'Debug.Assert Asc(Caption) <> 42
    'mark caption
    Caption = sDM & Caption
  End If
End Sub

Public Sub DeleteCoordinate(ByVal DelCoord As Long, Optional ByVal DontUndo As Boolean = False)

  Dim DelCount As Long
  Dim DelPos As Long
  Dim i As Long, j As Long, tmpPT As PT
  Dim NextUndo As PictureUndo
  Dim bytUndoData() As Byte
  
  'if this is last item
  If lstCoords.ListCount = 1 Then
    'send focus to cmd list
    lstCommands.SetFocus
    'use command delete
    DeleteCommand SelectedCmd
    'stop coordinate flashing
    tmrSelect.Enabled = False
  Exit Sub
  Else
    'remove the coordinates at this position
    DelPos = lstCoords.ItemData(DelCoord)
    
    'if deleting a plot point in splatter mode
    If InStr(1, lstCoords.List(DelCoord), "-") <> 0 Then
      DelCount = 3
    'if deleting a relative line, or a step line
    ElseIf lstCommands.Text = "Rel Line" Or lstCommands.Text = "X Corner" Or lstCommands.Text = "Y Corner" Then
      DelCount = 1
    Else
      DelCount = 2
    End If
    
    'if not skipping undo
    If Not DontUndo And Settings.PicUndo <> 0 Then
      'create new undo object
      Set NextUndo = New PictureUndo
      With NextUndo
        .UDAction = udpDelCoord
        .UDPicPos = DelPos
        .UDCmdIndex = SelectedCmd
        If DelCoord = lstCoords.ListCount - 1 Then
          'if deleting last coordinate, use -1 as coordpos
          'so it can get added back to end of list
          .UDCoordIndex = -1
        Else
          .UDCoordIndex = DelCoord
        End If
        .UDText = lstCoords.List(DelCoord)
        ReDim bytUndoData(DelCount - 1)
        For i = 0 To DelCount - 1
          bytUndoData(i) = PicEdit.Resource.Data(DelPos + i)
        Next i
        .UDData = bytUndoData
      End With
      'add to undo
      AddUndo NextUndo
    End If
    
    'remove from resource
    PicEdit.Resource.RemoveData DelPos, DelCount
    
    'adjust position values to account for deleted data
    UpdatePosValues SelectedCmd + 1, -DelCount
    
    'update position values for coords
    For i = DelCoord + 1 To lstCoords.ListCount - 1
      lstCoords.ItemData(i) = lstCoords.ItemData(i) - DelCount
    Next i
    
    'remove from coord list
    tmpPT = ExtractCoordinates(lstCoords.List(DelCoord))
    For i = 0 To lstCoords.ListCount - 1
      If tmpPT.X = CoordPT(i).X And tmpPT.Y = CoordPT(i).Y Then
        'move rest down one
        For j = i To lstCoords.ListCount - 2
          CoordPT(j) = CoordPT(j + 1)
        Next j
        Exit For
      End If
    Next i
    
    'remove from listbox
    lstCoords.RemoveItem DelCoord
    
  End If
  
  'redraw by selecting next coord
  If DelCoord = lstCoords.ListCount Then
    DelCoord = DelCoord - 1
  End If
  NoSelect = True
  lstCoords.ListIndex = DelCoord
End Sub


Public Sub MouseWheel(ByVal MouseKeys As Long, ByVal Rotation As Long, ByVal xPos As Long, ByVal yPos As Long)
  
  On Error GoTo ErrHandler
  
  If Not frmMDIMain.ActiveForm Is Me Then
    Exit Sub
  End If
  
  ' if mousekeys are active, just exit
  If MouseKeys <> 0 Then
    Exit Sub
  End If
  
  ' if not over the picture surface (or it's not visible)
  With picVisSurface
    If xPos <= .Left Or xPos >= .Left + .Width Or yPos <= .Top Or yPos >= .Top + .Height Or Not .Visible Then
      'try the priority picture surface
      With picPriSurface
        If xPos <= .Left Or xPos >= .Left + .Width Or yPos <= .Top Or yPos >= .Top + .Height Or Not .Visible Then
          'not over any picture; don't do anything
          Exit Sub
        End If
      End With
    End If
  End With
  
  'set flag to zoom function knows its a wheel based scroll
  '(and we don't use the mouse values of xPos and yPos
  'because we need to know which control is under the cursor;
  'it's actually easier to do that with API calls than
  'trying to derive it from this function's pos values)
  blnWheel = True
  
  If Sgn(Rotation) = 1 Then
    Toolbar1_ButtonClick Toolbar1.Buttons("zoomin")  '8
  ElseIf Sgn(Rotation) = -1 Then
    Toolbar1_ButtonClick Toolbar1.Buttons("zoomout") '9
  End If
  
  'always reset the wheel scroll flag
  blnWheel = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub DelPatternData(tmpIndex As Long, Optional ByVal DontUndo As Boolean = False)
  'remove pattern bytes

  Dim i As Long, lngNewPos As Long
  Dim bytPatDat() As Byte, bytPattern As Byte
  Dim strPatSplit() As String
  Dim NextUndo As PictureUndo
  
  On Error GoTo ErrHandler
  
  'if not skipping undo
  If Not DontUndo Then
    'reset random generator by using timer
    Randomize Timer
    
    'create array to hold patterns for undo in 25 byte segments
    ReDim bytPatDat(24)
  End If
  
  'set start pos so first iteration will select pattern byte for first coord
  lngNewPos = lstCommands.ItemData(tmpIndex) + 1
    
  Do
    'if not skipping undo
    If Not DontUndo Then
      bytPattern = PicEdit.Resource.Data(lngNewPos)
      
      If i > UBound(bytPatDat) Then
        ReDim Preserve bytPatDat(UBound(bytPatDat) + 25)
      End If
      
      'save to array
      bytPatDat(i) = bytPattern
    End If
    
    'remove from picture resource
    PicEdit.Resource.RemoveData lngNewPos
    
    'adjust pos
    lngNewPos = lngNewPos + 2
    
    'increment offset
    i = i + 1
  Loop Until PicEdit.Resource.Data(lngNewPos) >= &HF0
  
  'remove any extra bytes in pattern array
  If Not DontUndo Then
    ReDim Preserve bytPatDat(i - 1)
  End If
  
  'adjust positions of follow on commands (i now equals number of bytes removed)
  UpdatePosValues tmpIndex + 1, -i
  
  'if not skipping undo
  If Not DontUndo And Settings.PicUndo <> 0 Then
    'save undo info
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpDelPlotPattern
      .UDPicPos = lstCommands.ItemData(tmpIndex)
      .UDCmdIndex = tmpIndex
      .UDData = bytPatDat
    End With
    'add the undo object without setting edit menu
    UndoCol.Add NextUndo
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub DrawPicture(Optional ByVal Force As Boolean = False, Optional ByVal DrawCel As Boolean = False, Optional ByVal CelX As Long = -1, Optional ByVal CelY As Long = -1)
  
  'draws the pictures, including background,  and priority bands, if necessary
  'the complicated part is determining how much of the drawing to include,
  'which depends on which cmd is selected, whether moving or editing the command
  'and what Type of command it is
  
  'force needed to draw the images on form load; when this function is called
  'the pics all have Visible properties of False; so Force makes sure they get drawn
  'anyway
  
  Dim rtn As Long, i As Long, j As Long
  Dim DrawPos As Long

  Dim blnVis As Boolean, blnPri As Boolean

  On Error GoTo ErrHandler

  '(use surface vis property to determine if we need to draw pics
  blnVis = picVisSurface.Visible Or Force
  blnPri = picPriSurface.Visible Or Force

  'and if neither is being drawn (or forced, there is nothing to do here)
  If Not blnVis And Not blnPri Then
    Exit Sub
  End If
  
  'if selected item in list is a command (no coord selected)
  If lstCoords.ListIndex = -1 Then
    'if not the last cmd
    If SelectedCmd <> lstCommands.ListCount - 1 Then
      'draw pos is pos of next command
      PicEdit.DrawPos = lstCommands.ItemData(SelectedCmd + 1)
    Else
      'position is end of resource
      PicEdit.DrawPos = -1
    End If
  Else  'a specific coordinate is selected
    'if moving a cmd
    If PicDrawMode = doMoveCmds Then
      'draw pos is pos of next command (include the command(s) being moved)
      PicEdit.DrawPos = lstCommands.ItemData(SelectedCmd + 1)
    Else
      'where to set draw pos depends on which command is selected,
      'and which coordinate is chosen

      'if a plot or fill comd
      Select Case lstCommands.List(SelectedCmd)
      Case "Fill", "Plot"
        'add one to position, so the coordinate is included
        PicEdit.DrawPos = lstCoords.ItemData(lstCoords.ListIndex) + 1
      Case "Rel Line"
        If lstCoords.ListIndex = 0 Then
          PicEdit.DrawPos = lstCoords.ItemData(lstCoords.ListIndex) - 1
        Else
          PicEdit.DrawPos = lstCoords.ItemData(lstCoords.ListIndex)
        End If
      Case "X Corner", "Y Corner"
        If lstCoords.ListIndex = 0 Then
          PicEdit.DrawPos = lstCoords.ItemData(lstCoords.ListIndex) - 1
        Else
          'if moving a coordinate,
          If PicDrawMode = doMovePt Then
            'if editing second coord,
            If lstCoords.ListIndex = 1 Then
              'back up three so first line is not drawn
              PicEdit.DrawPos = lstCoords.ItemData(lstCoords.ListIndex) - 3
            Else
              'back up one so line in front of edit line is not drawn
              PicEdit.DrawPos = lstCoords.ItemData(lstCoords.ListIndex) - 1
            End If
          Else
            PicEdit.DrawPos = lstCoords.ItemData(lstCoords.ListIndex)
          End If
        End If
      Case Else
        'draw up to current command
        PicEdit.DrawPos = lstCoords.ItemData(lstCoords.ListIndex) - 1
      End Select
    End If
  End If

  'now that DrawPos is set, we can draw the picture on the screen

'''  for some reason, this does not work when the selection box is displayed...
'''  uncomment this (and corresponding reset msgs below) to see.
'''  'disable redraw to reduce flicker
'''  SendMessage picVisual.hWnd, WM_SETREDRAW, 0, 0
'''  SendMessage picPriority.hWnd, WM_SETREDRAW, 0, 0

  'if no background, and not adding a cel, just copy images directly from picedit:
  If Not PicEdit.BkgdShow And Not DrawCel Then
    If blnVis Then
      rtn = StretchBlt(picVisual.hDC, 0&, 0&, picVisual.Width, picVisual.Height, PicEdit.VisualBMP, 0&, 0&, 160&, 168&, SRCCOPY)
    End If
    If blnPri Then
      rtn = StretchBlt(picPriority.hDC, 0&, 0&, picPriority.Width, picPriority.Height, PicEdit.PriorityBMP, 0&, 0&, 160&, 168&, SRCCOPY)
    End If
  Else
    If blnVis Then
      'copy visual picture so cel can be added and/or bkgd shown
      rtn = BitBlt(picVisDraw.hDC, 0&, 0&, 160&, 168&, PicEdit.VisualBMP, 0&, 0&, SRCCOPY)
    End If
    If blnPri Then
      'copy priority picture so cel can be added
      rtn = BitBlt(picPriDraw.hDC, 0&, 0&, 160&, 168&, PicEdit.PriorityBMP, 0&, 0&, SRCCOPY)
    End If

    'if drawing a cel, then do it....
    If DrawCel Then
      '*'Debug.Assert CelX >= 0 And CelX < 256 And CelY >= 0 And CelY < 256
      AddCelToPic CByte(CelX), CByte(CelY)
    End If

    'draw vis picture...
    If blnVis Then
      'if showing background;
      If PicEdit.BkgdShow Then
        picVisual.Cls
        
        'first, draw the background on visual
        '(remember that IPicture objects are upside-down DIBs, so we need to flip BkgdImage vertically-
        BkgdImage.Render picVisual.hDC, tgtX * picVisual.Width / 320, tgtY * picVisual.Height / 168, tgtW * picVisual.Width / 320, tgtH * picVisual.Height / 168, PixToMets(srcX), BkgdImage.Height - PixToMets(srcY, True), PixToMets(srcW), -PixToMets(srcH, True), 0&

        'transblit the visual pic
        If BkgdTrans = 0 Then
          rtn = TransparentBlt(picVisual.hDC, 0&, 0&, CLng(picVisual.Width), CLng(picVisual.Height), picVisDraw.hDC, 0&, 0&, 160, 168, vbWhite)
        Else
          rtn = AlphaBlend(picVisual.hDC, 0&, 0&, CLng(picVisual.Width), CLng(picVisual.Height), picVisDraw.hDC, 0&, 0&, 160, 168, CLng(BkgdTrans * &H10000))
        End If
'        'copy visual into monochrome Image to create background mask
'        rtn = BitBlt(MonoMask.hDC, 0&, 0&, 160&, 168&, picVisDraw.hDC, 0&, 0&, SRCCOPY)
'
'        'mask out the background
'        rtn = StretchBlt(picVisual.hDC, 0&, 0&, picVisual.Width, picVisual.Height, MonoMask.hDC, 0&, 0&, 160&, 168&, SRCAND)
'
'        'invert into monochrome Image to create foreground mask
'        rtn = BitBlt(MonoMask.hDC, 0&, 0&, 160&, 168&, MonoMask.hDC, 0&, 0&, NOTSRCCOPY)
'
'        'mask out the foreground
'        rtn = BitBlt(picVisDraw.hDC, 0&, 0&, 160, 168, MonoMask.hDC, 0&, 0&, SRCAND)
'
'        'combine foreground and background
'        rtn = StretchBlt(picVisual.hDC, 0&, 0&, picVisual.Width, picVisual.Height, picVisDraw.hDC, 0&, 0&, 160, 168, SRCPAINT)
      Else
        'get Image from vis copy
        rtn = StretchBlt(picVisual.hDC, 0&, 0&, 320 * ScaleFactor, 168 * ScaleFactor, picVisDraw.hDC, 0&, 0&, 160&, 168&, SRCCOPY)
      End If
    End If

    'show priority Image
    If blnPri Then
      rtn = StretchBlt(picPriority.hDC, 0&, 0&, 320 * ScaleFactor, 168 * ScaleFactor, picPriDraw.hDC, 0&, 0&, 160&, 168&, SRCCOPY)
    End If
  End If
  
  'if showing bands
  If ShowBands Then
    'draw bands in matching priority color one pixel high
    For rtn = 5 To 14
        picVisual.Line (0, (Ceiling((rtn - 5) / 10 * (168 - PicEdit.PriBase)) + PicEdit.PriBase) * ScaleFactor - 1)-Step(picVisual.Width, 0), EGAColor(rtn), BF
        picPriority.Line (0, (Ceiling((rtn - 5) / 10 * (168 - PicEdit.PriBase)) + PicEdit.PriBase) * ScaleFactor - 1)-Step(picPriority.Width, 0), EGAColor(rtn), BF
    Next rtn
  End If
  
  'refresh the picture boxes
  picVisual.Refresh
  picPriority.Refresh
  
'''  'reenable updating
'''  SendMessage picVisual.hWnd, WM_SETREDRAW, 1, 0
'''  SendMessage picPriority.hWnd, WM_SETREDRAW, 1, 0
Exit Sub

ErrHandler:
  'check for the bleeping error that won't create bitmaps
  If Err.Number - vbObjectError = 610 And Not BadBitmap Then
    MsgBox "I'm very sorry, but there is a bug in the CreateDIBSection API call that" & vbNewLine & _
                "occasionally fails without returning an error code. I have not been able" & vbNewLine & _
                "to track down what is going on with this yet. The only way to clear this" & vbNewLine & _
                "save your work in progress, close WinAGI GDS, and try again.", vbInformation + vbOKOnly, "Picture Edit Error"
    BadBitmap = True
    Err.Clear
    Exit Sub
  Else
    '*'Debug.Assert False
    Resume Next
  End If
End Sub
Private Sub DrawTempLine(ByVal Editing As Boolean, NewX As Byte, NewY As Byte)
  'this command draws the line defined by current command
  'it is used to draw temporary lines when point by point editing
  'is desired
  
  'if Editing is false, no change to current list of coordinates is needed
  
  'this routine will validate whether NewX and/or NewY is valid based on line command
  'being edited
  
  Dim i As Long
  Dim CoordCount As Long
  Dim LineType As DrawFunction
  Dim CornerLine As Boolean, XFirst As Boolean
  Dim StartPt As PT, EndPT As PT, tmpPT As PT
  
  On Error GoTo ErrHandler
  
  'get coord Count
  CoordCount = lstCoords.ListCount
  
  'if no coordinates,
  If CoordCount = 0 Then
    Exit Sub
  'if only one coordinate
  ElseIf CoordCount = 1 Then
    'if editing this coord
    If Editing Then
      'draw new coordinate point
      DrawLine NewX, NewY, NewX, NewY
    Else
      'get coordinate
      StartPt = ExtractCoordinates(lstCoords.Text)
      DrawLine StartPt.X, StartPt.Y, StartPt.X, StartPt.Y
    End If
    Exit Sub
  End If
  
  'get Type of line command
  LineType = PicEdit.Resource.Data(lstCommands.ItemData(SelectedCmd))
  
  'if not editing,
  If Not Editing Then
    'draw all lines normally
    If EditCoordNum = 0 Then
      'start at beginning
      StartPt = ExtractCoordinates(lstCoords.List(0))
    Else
      'start at endpoint
      StartPt = ExtractCoordinates(lstCoords.List(EditCoordNum - 1))
    End If
    
    'get starting point
    For i = EditCoordNum To CoordCount - 1
      'if editing first pt, skip first iteration
      If i = 0 Then
        i = i + 1
      End If
      
      'get reference to next coord
      EndPT = ExtractCoordinates(lstCoords.List(i))
      'draw the line
      DrawLine StartPt.X, StartPt.Y, EndPT.X, EndPT.Y
      'set end point as new start point
      StartPt = EndPT
    Next i
  Else
    'if an x or Y corner is being edited
    If (LineType = dfXCorner) Or (LineType = dfYCorner) Then
      'enable corner editing
      CornerLine = True
      'determine if x or Y is changed first (at EditCoordNum - 1)
      XFirst = (Int(EditCoordNum / 2) <> EditCoordNum / 2)
      'if command is Ycorner,
      If LineType = dfYCorner Then
        'invert xfirst
        XFirst = Not XFirst
      End If
      
      'if edit coord is first point
      If EditCoordNum = 1 Then
        StartPt = ExtractCoordinates(lstCoords.List(0))
        If XFirst Then
          StartPt.Y = NewY
        Else
          StartPt.X = NewX
        End If
        
        EndPT.X = NewX
        EndPT.Y = NewY
        
        DrawLine EndPT.X, EndPT.Y, StartPt.X, StartPt.Y
      ElseIf EditCoordNum <> 0 Then
        'need to draw existing line in front of editcoord to newpooint
        StartPt = ExtractCoordinates(lstCoords.List(EditCoordNum - 2))
        EndPT = StartPt
        If XFirst Then
          EndPT.Y = NewY
        Else
          EndPT.X = NewX
        End If
        
        DrawLine StartPt.X, StartPt.Y, EndPT.X, EndPT.Y
        StartPt = EndPT
      End If
      
    Else
      'no corner editing
      CornerLine = False
      
      'if not first coordinate
      If EditCoordNum <> 0 Then
        'extract starting x and Y from coord just in front of edited coord
        StartPt = ExtractCoordinates(lstCoords.List(EditCoordNum - 1))
      End If
    End If
    
    'now draw line
        
    'step through rest of coordinates
    For i = EditCoordNum To lstCoords.ListCount - 1
      'get next point
      EndPT = ExtractCoordinates(lstCoords.List(i))
      
      'if this coord is the edited one
      If i = EditCoordNum Then
        Select Case LineType
        Case dfRelLine
          'need to validate x and Y first
          'if not first point
          If i > 0 Then
            'validate x and Y against next pt
            '(note that delta x is limited to -6 to avoid
            'values above &HF0, which would mistakenly be interpreted
            'as a new command)
            If NewX > StartPt.X + 7 Then
              NewX = StartPt.X + 7
            ElseIf NewX < StartPt.X - 6 Then
              NewX = StartPt.X - 6
            End If
            If NewY > StartPt.Y + 7 Then
              NewY = StartPt.Y + 7
            ElseIf NewY < StartPt.Y - 7 Then
              NewY = StartPt.Y - 7
            End If
          End If
          'if not last point
          If i < CoordCount - 1 Then
            'validate against next point
            'note that delta x is limited to -6 (swapped because we are
            'comparing against NEXT vs. PREVIOUS coordinate)
            'for same reason as given above
            tmpPT = ExtractCoordinates(lstCoords.List(i + 1))
            If NewX > tmpPT.X + 6 Then
              NewX = tmpPT.X + 6
            ElseIf NewX < tmpPT.X - 7 Then
              NewX = tmpPT.X - 7
            End If
            If NewY > tmpPT.Y + 7 Then
              NewY = tmpPT.Y + 7
            ElseIf NewY < tmpPT.Y - 7 Then
              NewY = tmpPT.Y - 7
            End If
          End If
        Case dfXCorner, dfYCorner
          If i = 0 Then
            'set start equal to endpt
            StartPt.X = NewX
            StartPt.Y = NewY
          End If
          
        End Select
        
        'use new x and Y
        EndPT.X = NewX
        EndPT.Y = NewY
        
        If i = 0 Then
          'start pt= endpt
          StartPt = EndPT
        End If
        
      'if editing corner
      ElseIf CornerLine Then
        'if this coord is directly in front of edited one
        If i = EditCoordNum - 2 Then
          'if xfirst
          If XFirst Then
            EndPT.X = NewX
          Else
            EndPT.Y = NewY
          End If
        'if this coord is directly after edited one
        ElseIf i = EditCoordNum + 1 Then
          'if xfirst
          If XFirst Then
            EndPT.X = NewX
          Else
            EndPT.Y = NewY
          End If
        End If
      End If
        
      'draw the line
      DrawLine StartPt.X, StartPt.Y, EndPT.X, EndPT.Y
      'set end point as new start point
      StartPt = EndPT
    Next i
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub EndEditCoord(ByVal CmdType As DrawFunction, ByVal CoordNum As Long, PicPt As PT, ByVal lngPos As Long, strCoord As String, Optional ByVal DontUndo As Boolean = False)
  
  Dim lngPosOffset As Long
  Dim strPattern As String
  Dim NextUndo As PictureUndo
  Dim tmpPT As PT, tmpPrevPT As PT, tmpNextPT As PT
  Dim bytData() As Byte
  
  On Error GoTo ErrHandler
  
  'save old pt if undoing
  If Not DontUndo And Settings.PicUndo Then
    'use data section to hold old coord values
    ReDim bytData(1)
    tmpPT = ExtractCoordinates(strCoord)
    bytData(0) = tmpPT.X
    bytData(1) = tmpPT.Y
    'if no change,
    If tmpPT.X = PicPt.X And tmpPT.Y = PicPt.Y Then
      'reset drawing mode, but no update is necessary
      PicDrawMode = doNone
      Exit Sub
    End If
  End If
  
  'validate for Type of node being edited
  Select Case CmdType
  Case dfAbsLine, dfFill, dfPlotPen
    'if this node includes a pattern command,
    If InStr(1, strCoord, "-") <> 0 Then
      'adjust resource pos by 1
      lngPosOffset = 1
      strPattern = Left$(strCoord, InStr(1, strCoord, "(") - 1)
    End If
    'update resource data
    PicEdit.Resource.Data(lngPos + lngPosOffset) = PicPt.X
    PicEdit.Resource.Data(lngPos + lngPosOffset + 1) = PicPt.Y
    
  Case dfRelLine
    'get point being edited
    tmpPT = RelLineCoord(lngPos)
    
    'validate x and Y:
    
    'if not first point
    If CoordNum > 0 Then
      'validate against previous point
      tmpPrevPT = RelLineCoord(lngPos - 1)
      'validate x and Y against previous pt
      '(note that delta x is limited to -6 to avoid
      'values above &HF0, which would mistakenly be interpreted
      'as a new command)
      If PicPt.X > tmpPrevPT.X + 7 Then
        PicPt.X = tmpPrevPT.X + 7
      ElseIf PicPt.X < tmpPrevPT.X - 6 Then
        PicPt.X = tmpPrevPT.X - 6
      End If
      If PicPt.Y > tmpPrevPT.Y + 7 Then
        PicPt.Y = tmpPrevPT.Y + 7
      ElseIf PicPt.Y < tmpPrevPT.Y - 7 Then
        PicPt.Y = tmpPrevPT.Y - 7
      End If
    End If
    
    'if not last point (next pt is not a new cmd)
    If PicEdit.Resource.Data(lngPos + IIf(CoordNum = 0, 2, 1)) < &HF0 Then
      'validate against next point
      'note that delta x is limited to +6 (swapped because we are
      'comparing against NEXT vs. PREVIOUS coordinate)
      'for same reason as given above
      tmpNextPT = RelLineCoord(lngPos + IIf(CoordNum = 0, 2, 1))
      If PicPt.X > tmpNextPT.X + 6 Then
        PicPt.X = tmpNextPT.X + 6
      ElseIf PicPt.X < tmpNextPT.X - 7 Then
        PicPt.X = tmpNextPT.X - 7
      End If
      If PicPt.Y > tmpNextPT.Y + 7 Then
        PicPt.Y = tmpNextPT.Y + 7
      ElseIf PicPt.Y < tmpNextPT.Y - 7 Then
        PicPt.Y = tmpNextPT.Y - 7
      End If
    End If
    
    'if first coordinate
    If CoordNum = 0 Then
      'recalculate delta to second point
      If PicEdit.Resource.Data(lngPos + 2) < &HF0 Then
        PicEdit.Resource.Data(lngPos + 2) = Abs(CLng(tmpNextPT.X) - PicPt.X) * 16 + IIf(Sgn(CLng(tmpNextPT.X) - PicPt.X) = -1, 128, 0) + Abs(CLng(tmpNextPT.Y) - PicPt.Y) + IIf(Sgn(CLng(tmpNextPT.Y) - PicPt.Y) = -1, 8, 0)
      End If
      'update data
      PicEdit.Resource.Data(lngPos) = PicPt.X
      PicEdit.Resource.Data(lngPos + 1) = PicPt.Y
    Else
      'if not last point
      If PicEdit.Resource.Data(lngPos + 1) < &HF0 Then
        'calculate new relative change in x and Y between next pt and this point
        PicEdit.Resource.Data(lngPos + 1) = Abs(CLng(tmpNextPT.X) - PicPt.X) * 16 + IIf(Sgn(CLng(tmpNextPT.X) - PicPt.X) = -1, 128, 0) + Abs(CLng(tmpNextPT.Y) - PicPt.Y) + IIf(Sgn(CLng(tmpNextPT.Y) - PicPt.Y) = -1, 8, 0)
      End If
      
      'calculate new relative change in x and Y between previous pt and this point
      PicEdit.Resource.Data(lngPos) = Abs(CLng(PicPt.X) - tmpPrevPT.X) * 16 + IIf(Sgn(CLng(PicPt.X) - tmpPrevPT.X) = -1, 128, 0) + Abs(CLng(PicPt.Y) - tmpPrevPT.Y) + IIf(Sgn(CLng(PicPt.Y) - tmpPrevPT.Y) = -1, 8, 0)
    End If
    
  Case dfXCorner
    'if editing first point,
    If CoordNum = 0 Then
      'update resource data
      PicEdit.Resource.Data(lngPos) = PicPt.X
      PicEdit.Resource.Data(lngPos + 1) = PicPt.Y
    Else
      'if odd
      If (Int(CoordNum / 2) <> CoordNum / 2) Then
        'x Value is at lngPos; Y Value is at lngPos-1
        PicEdit.Resource.Data(lngPos) = PicPt.X
        PicEdit.Resource.Data(lngPos - 1) = PicPt.Y
      Else
        'x Value is at lngPos-1, Y Value is at lngPos
        PicEdit.Resource.Data(lngPos - 1) = PicPt.X
        PicEdit.Resource.Data(lngPos) = PicPt.Y
      End If
    End If
    
  Case dfYCorner
    'if editing first point,
    If CoordNum = 0 Then
      'update resource data
      PicEdit.Resource.Data(lngPos) = PicPt.X
      PicEdit.Resource.Data(lngPos + 1) = PicPt.Y
    Else
      'if even
      If ((Int(CoordNum / 2) = CoordNum / 2)) Then
        'x Value is lngpos, Y Value is at lngpos-1
        PicEdit.Resource.Data(lngPos) = PicPt.X
        PicEdit.Resource.Data(lngPos - 1) = PicPt.Y
      Else
        'special check for Y lines; for the second coord, the x Value is actaully
        'two bytes in front of the edited coord (since cmd gives first coord
        'as two bytes, then shifts to single byte per coord; Y Value is at lngpos
        If CoordNum = 1 Then
          'x Value is at lngPos-2
          PicEdit.Resource.Data(lngPos - 2) = PicPt.X
        Else
          'x Value is at lngPos-1
          PicEdit.Resource.Data(lngPos - 1) = PicPt.X
        End If
        PicEdit.Resource.Data(lngPos) = PicPt.Y
      End If
    End If
  End Select
  
  If Not DontUndo And Settings.PicUndo <> 0 Then
    'create undo object
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpEditCoord
      .UDText = CStr(CmdType)
      .UDCoordIndex = CoordNum
      .UDCmdIndex = SelectedCmd
      .UDPicPos = lngPos
      .UDData = bytData()
    End With
    AddUndo NextUndo
  End If
  
  'reset edit mode
  PicDrawMode = doNone
  
  'begin highlighting selected coord again
  tmrSelect.Enabled = True
  If CursorMode = pcmWinAGI Then
    'save area under cursor
    BitBlt Me.hDC, 0, 0, 6 * ScaleFactor, 3 * ScaleFactor, picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
    BitBlt Me.hDC, 0, 12, 6 * ScaleFactor, 3 * ScaleFactor, picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub InsertCoordinate(ByVal BeginMove As Boolean)
  
  'inserts a new coord, then begins editing it
  
  Dim tmpPT As PT
  Dim bytData() As Byte, bytPattern As Byte
  
  On Error GoTo ErrHandler
  
  'get edit cmd
  EditCmd = PicEdit.Resource.Data(CLng(lstCommands.ItemData(SelectedCmd)))
  
  'get current point
  tmpPT = ExtractCoordinates(lstCoords.Text)
  
  'add a new coordinate after this coordinate
  Select Case EditCmd
  Case dfAbsLine, dfFill
    'store new abs coord point
    ReDim bytData(1)
    bytData(0) = tmpPT.X
    bytData(1) = tmpPT.Y
    
    'insert coordinate immediately after selected coordinate
    AddCoordToPic bytData, lstCoords.ListIndex, tmpPT.X, tmpPT.Y
    'don't need to redraw, since picture doesn't change when coord is added
  
  Case dfPlotPen
    'depends on pen- if splatter, we need an extra data element
    If CurrentPen.PlotStyle = psSolid Then
      'store new abs coord point
      ReDim bytData(1)
      bytData(0) = tmpPT.X
      bytData(1) = tmpPT.Y
      
      'insert coordinate immediately after selected coordinate
      AddCoordToPic bytData, lstCoords.ListIndex, tmpPT.X, tmpPT.Y
      'don't need to redraw, since picture doesn't change when coord is added
    Else
      'need three
      'store new abs coord point
      ReDim bytData(2)
      Randomize Now()
      bytPattern = 1 + CByte(Int(Rnd * 119))
      bytData(0) = 2 * bytPattern
      bytData(1) = tmpPT.X
      bytData(2) = tmpPT.Y
      'insert coordinate immediately after selected coordinate
      AddCoordToPic bytData, lstCoords.ListIndex, tmpPT.X, tmpPT.Y, CStr(bytPattern) & " -- "
      'don't need to redraw, since picture doesn't change when coord is added
    End If
    
  Case dfRelLine
    'insert single command with Value of zero
    ReDim bytData(0)
    bytData(0) = 0
    'insert coordinate immediately after selected coordinate
    AddCoordToPic bytData, lstCoords.ListIndex, tmpPT.X, tmpPT.Y
    'don't need to redraw, since picture doesn't change when coord is added
   
  Case dfXCorner
    'can only add to end
    If lstCoords.ListIndex = lstCoords.ListCount - 1 Then
      ReDim bytData(0)
      'if this current end pt an odd numbered coord
      If EditCoordNum / 2 <> Int(EditCoordNum / 2) Then
        'adding a new Y
        bytData(0) = tmpPT.Y
      Else
        'adding a new x
        bytData(0) = tmpPT.X
      End If
      'insert coordinate to end
      AddCoordToPic bytData, -1, tmpPT.X, tmpPT.Y
      'don't need to redraw, since picture doesn't change when coord is added
    End If
    
  Case dfYCorner
    'can only add to end
    If lstCoords.ListIndex = lstCoords.ListCount - 1 Then
      ReDim bytData(0)
      'if current end pt is an even numbered coord
      If EditCoordNum / 2 = Int(EditCoordNum / 2) Then
        'adding a new Y
        bytData(0) = tmpPT.Y
      Else
        'adding a new x
        bytData(0) = tmpPT.X
      End If
      'insert coordinate at end
      AddCoordToPic bytData, -1, tmpPT.X, tmpPT.Y
      'don't need to redraw, since picture doesn't change when coord is added
    End If
  End Select
  
  If BeginMove Then
    'now begin moving
    PicDrawMode = doMovePt
    
    'turn off cursor flasher
    tmrSelect.Enabled = False
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next

End Sub

Private Sub EndDraw(PicPt As PT)
  'finish drawing command
  'for lines and shapes, add the correct commands to complete the draw;
  'for other draw commands, add another point
  
  Dim bytData() As Byte
  Dim i As Long, lngInsertPos As Long
  
  On Error GoTo ErrHandler
  
  '*'Debug.Assert CurrentPen.PlotShape = SelectedPen.PlotShape
  '*'Debug.Assert CurrentPen.PlotSize = SelectedPen.PlotSize
  '*'Debug.Assert CurrentPen.PlotStyle = SelectedPen.PlotStyle
  '*'Debug.Assert CurrentPen.PriColor = SelectedPen.PriColor
  '*'Debug.Assert CurrentPen.VisColor = SelectedPen.VisColor
  
  'if cursor hasn't moved, just exit
  If Anchor.X = PicPt.X And Anchor.Y = PicPt.Y Then
    Exit Sub
  End If
  
  Select Case PicDrawMode
  Case doLine
    'depending on line Type, need to complete the current line segment
    '(note that we don't end the draw mode; this let's us continue adding more
    'line segments to this command)
    Select Case SelectedTool
    Case ttLine
      'set data to add this point
      ReDim bytData(1)
      bytData(0) = PicPt.X
      bytData(1) = PicPt.Y
      
    Case ttRelLine
      'validate x and y
      '(note that delta x is limited to -6 to avoid
      'values above &HF0, which would mistakenly be interpreted
      'as a new command)
      If PicPt.X < Anchor.X - 6 Then
        PicPt.X = Anchor.X - 6
      ElseIf PicPt.X > Anchor.X + 7 Then
        PicPt.X = Anchor.X + 7
      End If
      If PicPt.Y < Anchor.Y - 7 Then
        PicPt.Y = Anchor.Y - 7
      ElseIf PicPt.Y > Anchor.Y + 7 Then
        PicPt.Y = Anchor.Y + 7
      End If
      
      'calculate delta to this point
      ReDim bytData(0)
      bytData(0) = Abs(CLng(PicPt.X) - Anchor.X) * 16 + IIf(Sgn(CLng(PicPt.X) - Anchor.X) = -1, 128, 0) + Abs(CLng(PicPt.Y) - Anchor.Y) + IIf(Sgn(CLng(PicPt.Y) - Anchor.Y) = -1, 8, 0)
      
      ''*'Debug.Assert EditCoordNum = lstCoords.ListCount - 1
      
    Case ttCorner
      'draw next line coordinate
      ReDim bytData(0)
     'get insert pos
     lngInsertPos = lstCommands.ItemData(SelectedCmd)
     
      'if drawing second point
      If lstCoords.ListCount = 1 Then
        'if mostly vertical
        If Abs(CLng(PicPt.X) - Anchor.X) < Abs(CLng(PicPt.Y) - Anchor.Y) Then
          'command should be Y corner
          If Asc(lstCommands.Text) <> 89 Then
            lstCommands.List(SelectedCmd) = "Y Corner"
            PicEdit.Resource.Data(lngInsertPos) = dfYCorner
          End If
          'limit change to vertical direction only
          PicPt.X = Anchor.X
          bytData(0) = PicPt.Y
        Else
          'command should be X corner
          If Asc(lstCommands.Text) = 89 Then
            lstCommands.List(SelectedCmd) = "X Corner"
            PicEdit.Resource.Data(lngInsertPos) = dfXCorner
          End If
          'limit change to horizontal direction only
          PicPt.Y = Anchor.Y
          bytData(0) = PicPt.X
        End If
      Else
        'determine which direction to allow movement
        If (Asc(lstCommands.Text) = 88 And (Int(lstCoords.ListCount / 2) = lstCoords.ListCount / 2)) Or _
           (Asc(lstCommands.Text) = 89 And (Int(lstCoords.ListCount / 2) <> lstCoords.ListCount / 2)) Then
          'limit change to vertical direction
          PicPt.X = Anchor.X
          bytData(0) = PicPt.Y
        Else
          'limit change to horizontal direction
          PicPt.Y = Anchor.Y
          bytData(0) = PicPt.X
        End If
      End If
     
    End Select
    'if cursor hasn't moved, just exit
    If Anchor.X = PicPt.X And Anchor.Y = PicPt.Y Then
      Exit Sub
    End If
    
    'set anchor to new point
    Anchor = PicPt
    'insert coordinate
    AddCoordToPic bytData(), -1, PicPt.X, PicPt.Y
  
  Case doShape
    'depending on shape Type, add appropriate commands to add the
    'selected element
    '(note that when shapes are completed, we go back to 'none' as the draw mode
    'each shape is drawn as a separate action)
    
    Select Case SelectedTool
    Case ttRectangle
      'finish drawing box
      ReDim bytData(6)
      bytData(0) = dfXCorner
      bytData(1) = Anchor.X
      bytData(2) = Anchor.Y
      bytData(3) = PicPt.X
      bytData(4) = PicPt.Y
      bytData(5) = Anchor.X
      bytData(6) = Anchor.Y
      
      'add command
      InsertCommand bytData, SelectedCmd, "X Corner", gInsertBefore
      
      'select this command
      SelectCmd lstCommands.NewIndex
      
      'adjust last undo text
      UndoCol(UndoCol.Count).UDAction = udpRectangle
      UndoCol(UndoCol.Count).UDCmd = vbNullString
      
    Case ttTrapezoid
      'finish drawing trapezoid
      ReDim bytData(10)
      bytData(0) = dfAbsLine
      bytData(1) = Anchor.X
      bytData(2) = Anchor.Y
      bytData(3) = 159 - Anchor.X
      bytData(4) = Anchor.Y
      'ensure sloping side is on same side of picture
      If (Anchor.X < 80 And PicPt.X < 80) Or (Anchor.X >= 80 And PicPt.X >= 80) Then
        bytData(5) = 159 - PicPt.X
        bytData(6) = PicPt.Y
        bytData(7) = PicPt.X
        bytData(8) = PicPt.Y
      Else
        bytData(5) = PicPt.X
        bytData(6) = PicPt.Y
        bytData(7) = 159 - PicPt.X
        bytData(8) = PicPt.Y
      End If
      bytData(9) = Anchor.X
      bytData(10) = Anchor.Y
      
      'add command
      InsertCommand bytData, SelectedCmd, "Abs Line", gInsertBefore
      'ensure it is selected
      SelectCmd lstCommands.NewIndex
      
      'adjust last undo text
      UndoCol(UndoCol.Count).UDAction = udpTrapezoid
      UndoCol(UndoCol.Count).UDCmd = vbNullString
      
    Case ttEllipse
      'finish drawing ellipse
      
      'if both height and width are one pixel
      If ((CLng(Anchor.X) - PicPt.X) = 0) And ((CLng(Anchor.Y) - PicPt.Y) = 0) Then
        'draw just a single pixel
        ReDim bytData(2)
        bytData(0) = dfXCorner
        bytData(1) = Anchor.X
        bytData(2) = Anchor.Y
        'insert the command
        InsertCommand bytData, SelectedCmd, "X Corner", gInsertBefore
        'then select it
        SelectCmd lstCommands.NewIndex
        
      'if height is one pixel,
      ElseIf (CLng(Anchor.Y) - PicPt.Y = 0) Then
        'just draw a horizontal line
        ReDim bytData(3)
        bytData(0) = dfXCorner
        bytData(1) = Anchor.X
        bytData(2) = Anchor.Y
        bytData(3) = PicPt.X
        'add command
        InsertCommand bytData, SelectedCmd, "X Corner", gInsertBefore
        SelectCmd lstCommands.NewIndex
        
      'if width is one pixel,
      ElseIf (CLng(Anchor.X) - PicPt.X = 0) Then
        'just draw a vertical line
        ReDim bytData(3)
        bytData(0) = dfYCorner
        bytData(1) = Anchor.X
        bytData(2) = Anchor.Y
        bytData(3) = PicPt.Y
        'add command
        InsertCommand bytData, SelectedCmd, "Y Corner", gInsertBefore
        'and select it
        SelectCmd lstCommands.NewIndex
        
      Else
        'ensure we are in a upperleft-lower right configuration
        If Anchor.X > PicPt.X Then
          i = Anchor.X
          Anchor.X = PicPt.X
          PicPt.X = i
        End If
        If Anchor.Y > PicPt.Y Then
          i = Anchor.Y
          Anchor.Y = PicPt.Y
          PicPt.Y = i
        End If
          
        'call drawellipse to update arc segment data
        DrawCircle Anchor.X, Anchor.Y, PicPt.X, PicPt.Y
        ReDim bytData((Segments + 1) * 2)
        bytData(0) = dfAbsLine
        
        'now draw the arc segments:
        
        'add first arc
        For i = 0 To Segments
          bytData(i * 2 + 1) = Anchor.X + ArcPt(0).X - ArcPt(i).X
          bytData(i * 2 + 2) = Anchor.Y + ArcPt(Segments).Y - ArcPt(i).Y
        Next i
        InsertCommand bytData, SelectedCmd, "Abs Line", gInsertBefore
        
        'add second arc (skip undo)
        For i = 0 To Segments
          bytData(2 * i + 1) = PicPt.X - ArcPt(0).X + ArcPt(i).X
          bytData(2 * i + 2) = Anchor.Y + ArcPt(Segments).Y - ArcPt(i).Y
        Next i
        InsertCommand bytData, SelectedCmd, "Abs Line", False, True
        
        'add third arc (skip undo)
        For i = 0 To Segments
          bytData(2 * i + 1) = PicPt.X - ArcPt(0).X + ArcPt(i).X
          bytData(2 * i + 2) = PicPt.Y - ArcPt(Segments).Y + ArcPt(i).Y
        Next i
        InsertCommand bytData, SelectedCmd + 1, "Abs Line", False, True
        
        'add fourth arc (skip undo)
        For i = 0 To Segments
          bytData(2 * i + 1) = Anchor.X + ArcPt(0).X - ArcPt(i).X
          bytData(2 * i + 2) = PicPt.Y - ArcPt(Segments).Y + ArcPt(i).Y
        Next i
        InsertCommand bytData, SelectedCmd + 2, "Abs Line", False, True
        
        'select the last command added
        SelectCmd SelectedCmd + 4
        
        'adjust last undo text
        UndoCol(UndoCol.Count).UDAction = udpEllipse
        UndoCol(UndoCol.Count).UDCmd = vbNullString
      End If
    End Select
    
    'end draw mode
    StopDrawing
  End Select
Exit Sub

ErrHandler:
  Resume Next
End Sub


Public Sub AddCoordToPic(NewData() As Byte, ByVal CoordPos As Long, ByVal bytX As Byte, ByVal bytY As Byte, Optional ByVal Prefix As String = "", Optional ByVal DontUndo As Boolean = False)

  'inserts coordinate data into PicEdit
  
  Dim NextUndo As PictureUndo
  Dim lngInsertPos As Long, lngSize As Long
  Dim bytData() As Byte, lngCount As Long
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'copy data to local array
  bytData = NewData
  lngCount = UBound(bytData) + 1
  
  'if no coord yet
  If lstCoords.ListCount = 0 Then
    'get insert pos from the cmd
    lngInsertPos = lstCommands.ItemData(SelectedCmd)
    '*'Debug.Assert CoordPos = -1
  Else
    If CoordPos = -1 Then
      lngInsertPos = lstCommands.ItemData(SelectedCmd + 1)
      CoordPos = lstCoords.ListCount
    Else
      '*'Debug.Assert CoordPos <= lstCoords.ListCount - 1
      'get insert pos from coord list
      lngInsertPos = lstCoords.ItemData(CoordPos)
    End If
  End If
  
  'if not skipping undo
  If Not DontUndo And Settings.PicUndo <> 0 Then
    'create new undo object
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpAddCoord
      .UDPicPos = lngInsertPos
      .UDCmdIndex = SelectedCmd
      .UDCoordIndex = CoordPos
      .UDText = Prefix & CoordText(bytX, bytY)
    End With
    'add to undo
    AddUndo NextUndo
  End If
  
  'insert data
  PicEdit.Resource.InsertData bytData, lngInsertPos
  
  'insert coord text
  AddCoordToList bytX, bytY, lngInsertPos, Prefix, CoordPos
  lstCoords.Refresh

  EditCoordNum = lstCoords.ListIndex
  
  'update position values for rest of coord list
  For i = CoordPos + 1 To lstCoords.ListCount - 1
    lstCoords.ItemData(i) = lstCoords.ItemData(i) + UBound(bytData()) + 1
  Next i
  
  'update position values in rest of cmd list
  UpdatePosValues SelectedCmd + 1, UBound(bytData()) + 1
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub JoinCommands(SecondCmdIndex As Long, Optional ByVal DontUndo As Boolean = False)
  'joins two commands that are adjacent, where
  'first coord of SecondCmdIndex is same point as end of previous command
  'or if both cmds are plots or fills
  
  Dim NextUndo As PictureUndo
  Dim CmdType As DrawFunction, FirstCmdIndex As Long
  Dim lngPos As Long, lngIndex As Long
  Dim lngCount As Long
  Dim IsVertical As Boolean  'false = horizontal, true = vertical
  
  'get position of command
  lngPos = lstCommands.ItemData(SecondCmdIndex)
  'get first cmd index
  FirstCmdIndex = SecondCmdIndex - 1
  
  'get command Type
  CmdType = PicEdit.Resource.Data(lngPos)
  
  'if not skipping undo
  If Not DontUndo And Settings.PicUndo <> 0 Then
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpJoinCmds
      'if cmd requires one byte per coord pair
      'need to move picpos marker back one
      Select Case CmdType
      Case dfXCorner, dfYCorner, dfRelLine
        'position of the split coord is back one
        .UDPicPos = lstCommands.ItemData(SecondCmdIndex) - 1
      Case Else
        .UDPicPos = lstCommands.ItemData(SecondCmdIndex)
      End Select
      
      'set cmdindex to first cmd
      .UDCmdIndex = FirstCmdIndex
      'set coord index to Count of firstcmd
    End With
    AddUndo NextUndo
  End If
  
  Select Case CmdType
  Case dfFill, dfPlotPen
    'just delete the command
    lngCount = 1
  Case dfXCorner, dfYCorner
    'get orientation of last line of first cmd
    IsVertical = GetVerticalStatus(lstCommands.ItemData(FirstCmdIndex))
    
    'if orientation of last line of first cmd
    'is same as first line of this cmd
    If ((CmdType = dfXCorner) And Not IsVertical) Or _
       ((CmdType = dfYCorner) And IsVertical) Then
      'delete last coordinate from previous cmd AND command and first coordinate
      lngPos = lngPos - 1
      lngCount = 4
      
    Else
      'delete command and first coordinate
      lngCount = 3
    End If
    
  Case Else
    'delete command and first coordinate
    lngCount = 3
  End Select
  
  'delete the data
  PicEdit.Resource.RemoveData lngPos, lngCount
  'remove the second cmd
  lstCommands.RemoveItem SecondCmdIndex
  
  'update follow on cmds
  UpdatePosValues SecondCmdIndex, -lngCount
  
  'update
  SelectCmd FirstCmdIndex, False
End Sub

Public Sub MenuClickClear()
  'clears the picture
  
  Dim i As Long
  
  'verify
  If MsgBox("This will reset the picture, deleting all commands. This action cannot be undone. Do you want to continue?", vbQuestion + vbYesNo, "Clear Picture") = vbNo Then
    Exit Sub
  End If
  
  'clear drawing surfaces
  picVisual.Cls
  picPriority.Cls
  
  'clear picture
  PicEdit.Clear
  
  'redraw tree
  LoadCmdList
  
  'select the end
  SelectCmd 0, False
  
  'reset pen
  SelectedPen.PriColor = agNone
  SelectedPen.VisColor = agNone
  SelectedPen.PlotShape = psCircle
  SelectedPen.PlotSize = 0
  SelectedPen.PlotStyle = psSolid
  CurrentPen = SelectedPen
  
  'refresh palette
  picPalette.Refresh
  
  'clear the undo buffer
  If UndoCol.Count > 0 Then
    For i = UndoCol.Count To 1 Step -1
      UndoCol.Remove i
    Next i
    SetEditMenu
  End If
End Sub

Private Sub ReadjustPlotCoordinates(StartIndex As Long, NewPlotStyle As EPlotStyle, Optional ByVal DontUndo As Boolean = False, Optional ByVal StopIndex As Long = -1)
  'starting at command in list at StartIndex, step through all
  'commands until another setplotpen command or end is reached;
  'any plot commands identified during search are checked to
  'see if they match format of desired plot pen style (solid or
  'splatter); if they don't match, they are adjusted (by adding
  'or removing the pattern byte)
  
  'if stopindex is passed, only cmds from StartIndex to StopIndex
  'are checked; if stopindex is not passed, all cmds to end of
  'cmd list are checked
   
  Dim i As Long
  Dim bytTemp() As Byte
  Dim j As Long
  
  On Error GoTo ErrHandler
  
  If StopIndex = -1 Then
    StopIndex = lstCommands.ListCount - 1
  End If
  
  i = StartIndex
  Do
    'check for plot command or change plot pen command
    Select Case Left$(lstCommands.List(i), 4)
    Case "Plot"
      'if style is splatter
      If NewPlotStyle = psSplatter Then
        'if skipping the undo feature
        If DontUndo Then
          'need to set tmp byte array so addpatterndata method
          'will know to create the random bytes for this set of coordinates
          ReDim bytTemp(0)
          bytTemp(0) = &HFF
        End If
        
        'add pattern bytes (use a temp array as place holder for byte array argument)
        AddPatternData i, bytTemp, DontUndo
        
      'if style is solid,
      Else
        'delete pattern bytes
        DelPatternData i, DontUndo
      End If
      
    Case "Set " 'set pen'
      'can exit here because this pen command
      'ensures future plot commands are correct
      Exit Do
    End Select
    'get next cmd
    i = i + 1
  Loop Until i > StopIndex
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub SelectCmd(ByVal CmdPos As Long, Optional ByVal DontSelect As Boolean = True)
  
  'ensures cmd list is cleared, and selects the desired cmd
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'disable painting of listbox until all done
  SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
  
  'disable updating in listbox while de-selecting
  NoSelect = True
  If lstCommands.SelCount > 0 Then
    For i = 0 To lstCommands.ListCount - 1
      If lstCommands.Selected(i) Then
        lstCommands.Selected(i) = False
        If lstCommands.SelCount = 0 Then
          Exit For
        End If
      End If
    Next i
  End If
  'also set lstCommands.ListIndex to -1;
  'we need to do this to make sure that when
  'we select the CmdPos entry in the command list
  'that it forces an update of the coord list
  i = lstCommands.TopIndex
  lstCommands.ListIndex = -1
  lstCommands.TopIndex = i
  'allow/disallow updating in listbox click event
  NoSelect = DontSelect
  
  'select desired cmd
  CodeClick = True
  lstCommands.ListIndex = CmdPos
  SelectedCmd = CmdPos
  CodeClick = True
  'we need to also set noselect to true here;
  'otherwise, if the .Selected(CmdPos) value is false
  'we will get a second trip through lstCommands_Click
  NoSelect = True
  lstCommands.Selected(CmdPos) = True
  
  'restore updating
  NoSelect = False
  
  'restore painting of listbox
  SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
  
  If ActiveControl Is lstCoords Then
    lstCommands.SetFocus
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub SetCursors(ByVal NewCursor As EPicCur)

  'sets picVis and picPri cursors
  
  'if already correct cursor, just exit (avoids flickering)
  If CurCursor = NewCursor Then
    Exit Sub
  End If
  
  'save cursor Value
  CurCursor = NewCursor
  
  'now change cursor to correct Value
  Select Case NewCursor
  Case pcEdit
    picVisual.MousePointer = vbCustom
    picPriority.MousePointer = vbCustom
    picVisual.MouseIcon = LoadResPicture("EPC_EDIT", vbResCursor)
    picPriority.MouseIcon = LoadResPicture("EPC_EDIT", vbResCursor)
    
  Case pcCross
    picVisual.MousePointer = vbCrosshair
    picPriority.MousePointer = vbCrosshair
    
  Case pcMove
    picVisual.MousePointer = vbCustom
    picPriority.MousePointer = vbCustom
    picVisual.MouseIcon = LoadResPicture("EPC_MOVECMD", vbResCursor)
    picPriority.MouseIcon = LoadResPicture("EPC_MOVECMD", vbResCursor)
    
  Case pcDefault
    picVisual.MousePointer = vbDefault
    picPriority.MousePointer = vbDefault
  
  Case pcNO
    picVisual.MousePointer = vbCustom
    picPriority.MousePointer = vbCustom
    picVisual.MouseIcon = LoadResPicture("EPC_NA", vbResCursor)
    picPriority.MouseIcon = LoadResPicture("EPC_NA", vbResCursor)
  
  Case pcPaint
    picVisual.MousePointer = vbCustom
    picPriority.MousePointer = vbCustom
    picVisual.MouseIcon = LoadResPicture("EPC_PAINT", vbResCursor)
    picPriority.MouseIcon = LoadResPicture("EPC_PAINT", vbResCursor)

  Case pcBrush
    picVisual.MousePointer = vbCustom
    picPriority.MousePointer = vbCustom
    picVisual.MouseIcon = LoadResPicture("EPC_BRUSH", vbResCursor)
    picPriority.MouseIcon = LoadResPicture("EPC_BRUSH", vbResCursor)
  
  Case pcSelect
    picVisual.MousePointer = vbCustom
    picPriority.MousePointer = vbCustom
    picVisual.MouseIcon = LoadResPicture("EPC_SELECT", vbResCursor)
    picPriority.MouseIcon = LoadResPicture("EPC_SELECT", vbResCursor)

  Case pcEditSel
    picVisual.MousePointer = vbCustom
    picPriority.MousePointer = vbCustom
    picVisual.MouseIcon = LoadResPicture("EPC_EDITSEL", vbResCursor)
    picPriority.MouseIcon = LoadResPicture("EPC_EDITSEL", vbResCursor)
    
  End Select
  
End Sub

Private Sub ShowCmdSelection()
  
  'positions and displays a flashing selection outline around the current selection box
  
  On Error GoTo ErrHandler
  
  'if selection is more than a single pixel
  If SelSize.X > 0 And SelSize.Y > 0 Then
    'position the shapes around the selection area
    shpVis.Move SelStart.X * ScaleFactor * 2 - 1, SelStart.Y * ScaleFactor - 1, SelSize.X * ScaleFactor * 2 + 2, SelSize.Y * ScaleFactor + 2
    'check if off edge
    If shpVis.Left = -1 Then
      shpVis.Left = 0
      shpVis.Width = shpVis.Width - 1
    End If
    If shpVis.Top = -1 Then
      shpVis.Top = 0
      shpVis.Height = shpVis.Height - 1
    End If
    If SelStart.X + SelSize.X = 160 Then
      shpVis.Width = shpVis.Width - 1
    End If
    If SelStart.Y + SelSize.Y = 168 Then
      shpVis.Height = shpVis.Height - 1
    End If
        
    'move priority screen shape to match visual screen
    shpPri.Move shpVis.Left, shpVis.Top, shpVis.Width, shpVis.Height
    'timer is used to create 'flashing' of line types
    tmrSelect.Enabled = True
    shpVis.Visible = True
    shpPri.Visible = True
    
    'force tool to select if drawing something
    '(if  tool is selectArea, let it be)
    If SelectedTool <> ttEdit And SelectedTool <> ttSelectArea Then
      'select it
      SelectedTool = ttEdit
      Toolbar1.Buttons("select").Value = tbrPressed
    End If
    
  Else
    'hide the shapes; the selected cmds dont include cooordinates
    tmrSelect.Enabled = False
    shpVis.Visible = False
    shpPri.Visible = False
  End If
  
  'if tool is select edit, update menu if necessary
  If SelectedTool = ttSelectArea Then
    'enable copy command if selection is >0
    frmMDIMain.mnuECopy.Enabled = (SelSize.X > 0 And SelSize.Y > 0)
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub SplitCommand(CoordIndex As Long, Optional ByVal DontUndo As Boolean = False)
  'splits a command into two separate commands of the same Type
  
  Dim NextUndo As PictureUndo
  Dim CmdType As DrawFunction
  Dim lngPos As Long, lngCmdIndex As Long
  Dim lngCount As Long
  Dim IsVertical As Boolean  'false = horizontal, true = vertical
  Dim tmpPT As PT
  Dim bytData(1) As Byte
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'get cmd index
  lngCmdIndex = SelectedCmd
  
  'get coordinate values
  tmpPT = ExtractCoordinates(lstCoords.List(CoordIndex))
  
  'get command Type
  CmdType = PicEdit.Resource.Data(lstCommands.ItemData(lngCmdIndex))
  
  'get insert pos
  
  'if a fill or plot,
  If CmdType = dfFill Or CmdType = dfPlotPen Then
    lngPos = lstCoords.ItemData(CoordIndex)
  Else
    'insertion point is NEXT coord
    lngPos = lstCoords.ItemData(CoordIndex + 1)
  End If
  
  'insert a new command in resource and listbox
  PicEdit.Resource.InsertData CByte(CmdType), lngPos
  lstCommands.AddItem LoadResString(DRAWFUNCTIONTEXT + CmdType - &HF0), lngCmdIndex + 1
  lstCommands.ItemData(lngCmdIndex + 1) = lngPos
  
  'take command specific actions
  Select Case CmdType
  Case dfXCorner, dfYCorner
    'get orientation of line being split
    IsVertical = (Int(CoordIndex / 2) <> (CoordIndex / 2))
    'if cmd is a yCorner
    If CmdType = dfYCorner Then
      'flip it
      IsVertical = Not IsVertical
    End If
    
    'if splitting a vertical line,
    If IsVertical Then
      'if inserted byte is not a Ycorner
      If CmdType <> dfYCorner Then
        'change inserted cmd to YCorner
        PicEdit.Resource.Data(lngPos) = dfYCorner
        lstCommands.List(lngCmdIndex + 1) = LoadResString(DRAWFUNCTIONTEXT + dfYCorner - &HF0)
      End If
    Else
      'if inserted byte is not a Xcorner
      If CmdType <> dfXCorner Then
        'change inserted cmd to XCorner
        PicEdit.Resource.Data(lngPos) = dfXCorner
        lstCommands.List(lngCmdIndex + 1) = LoadResString(DRAWFUNCTIONTEXT + dfXCorner - &HF0)
      End If
    End If
    
    'insert starting point in resource
    bytData(0) = tmpPT.X
    bytData(1) = tmpPT.Y
    PicEdit.Resource.InsertData bytData(), lngPos + 1
    
    'three bytes inserted
    lngCount = 3
  
  Case dfAbsLine, dfRelLine
    'insert starting point into new cmd
    bytData(0) = tmpPT.X
    bytData(1) = tmpPT.Y
    PicEdit.Resource.InsertData bytData(), lngPos + 1
    
    'three bytes inserted
    lngCount = 3
    
  Case dfFill, dfPlotPen
    'only one byte inserted
    lngCount = 1
    
  End Select
  
  'update positions for cmds AFTER the new cmd
  UpdatePosValues lngCmdIndex + 2, lngCount
  
  'select the newly added command
  SelectCmd lngCmdIndex + 1, False
  
  'if not skipping undo
  If Not DontUndo And Settings.PicUndo <> 0 Then
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpSplitCmd
      .UDPicPos = lngPos
      .UDCmdIndex = lngCmdIndex + 1
    End With
    AddUndo NextUndo
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub StartDrag(ByVal InPri As Boolean, ByVal X As Single, ByVal Y As Single)
  
  Dim rtn As Long
  
  'if in priority window,
  If InPri Then
    'if either scrollbar is visible,
    If Me.hsbPri.Visible Or vsbPri.Visible Then
      'set dragpic mode
      blnDragging = True
      
      'set pointer to custom (special case - don't use cursor function
      picPriority.MousePointer = vbCustom
      picPriority.MouseIcon = LoadResPicture("EPC_MOVE", vbResCursor)
      CurCursor = pcMove
      
      rtn = SetCapture(picPriSurface.hWnd)
      'save x and Y offsets
      sngOffsetX = X
      sngOffsetY = Y
    End If
  Else
    'if either scrollbar is visible,
    If Me.hsbVis.Visible Or vsbVis.Visible Then
      'set dragpic mode
      blnDragging = True
      
      'set pointer to custom
      picVisual.MousePointer = vbCustom
      picVisual.MouseIcon = LoadResPicture("EPC_MOVE", vbResCursor)
      CurCursor = pcMove
      
      rtn = SetCapture(picVisSurface.hWnd)
      'save x and Y offsets
      sngOffsetX = X
      sngOffsetY = Y
    End If
  End If
End Sub

Private Sub StopDrawing()

  'cancels a drawing action without adding a command or coordinate
  
  'reset draw mode
  PicDrawMode = doNone
  'if on a coordinate
  If lstCoords.ListIndex <> -1 Then
    'select entire cmd
    lstCoords.ListIndex = -1
    'set curpt to impossible value so it will have to be reset when coords are selected
    CurPt.X = 255
    CurPt.Y = 255
  End If
  
  'force redraw
  CodeClick = True
  lstCommands_Click
End Sub
Private Sub ToggleBkgd(ByVal NewVal As Boolean, Optional ByVal ShowConfig As Boolean = False)

  'sets background Image display to match newval
  'loads a background if one is needed
  
  Dim OldVal As Boolean
  
  On Error GoTo ErrHandler
  
  'note curent value
  OldVal = PicEdit.BkgdShow
  
  PicEdit.BkgdShow = NewVal

  'if showing background AND there is not a picture (OR if forcing re-configure)
  If (PicEdit.BkgdShow And (BkgdImage Is Nothing)) Or ShowConfig Then
    'use configure screen, which will load a background
    If Not ConfigureBackground() Then
      'if user cancels, and still no background, force flag to false
      If (BkgdImage Is Nothing) Then
        PicEdit.BkgdShow = False
'''      Else
'''        'there is a bkgd, but no change made
      End If
    End If
  End If
  
  'set button status, and set value for stored image in picresource
  If PicEdit.BkgdShow Then
    Toolbar1.Buttons("bkgd").Value = tbrPressed
  Else
    Toolbar1.Buttons("bkgd").Value = tbrUnpressed
  End If
  
  'update menu caption
  With frmMDIMain
    'toggle bkgd visible only if a bkgd Image is loaded
    .mnuRCustom2.Visible = Not (BkgdImage Is Nothing)
    If .mnuRCustom2.Visible Then
      .mnuRCustom2.Enabled = True
      If PicEdit.BkgdShow And .mnuRCustom2.Visible Then
        .mnuRCustom2.Caption = "Hide Background" & vbTab & "Alt+B"
      Else
        .mnuRCustom2.Caption = "Show Background" & vbTab & "Alt+B"
      End If
    End If
    ' allow removal if an image is loaded
    .mnuRCustom3.Visible = .mnuRCustom2.Visible
    If .mnuRCustom3.Visible Then
      .mnuRCustom3.Enabled = True
      .mnuRCustom3.Caption = "Remove Background Image" & vbTab & "Shift+Alt+B"
    End If
  End With
  
  'if current command has coordinates, do more than just redraw picture
  If lstCoords.ListCount > 0 Then
    If lstCoords.ListIndex <> -1 Then
      'use coordinate click method if a coordinate is currently selected
      lstCoords_Click
    Else
      'use command click method if no coordinates selected
      CodeClick = True
      lstCommands_Click
    End If
  Else
    'if selected command doesn't have any coordinates
    'redrawing is sufficient to set correct state of editor
    DrawPicture
  End If
    
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub UpdateID(ByVal NewID As String, NewDescription As String)

  On Error GoTo ErrHandler
  
  If PicEdit.Description <> NewDescription Then
    'change the PicEdit object's description
    PicEdit.Description = NewDescription
  End If
  
  If PicEdit.ID <> NewID Then
    'change the PicEdit object's ID and caption
    PicEdit.ID = NewID
    'if picedit is dirty
    If Asc(Caption) = 42 Then
      Caption = sDM & sPICED & ResourceName(PicEdit, InGame, True)
    Else
      Caption = sPICED & ResourceName(PicEdit, InGame, True)
    End If
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub UpdatePosValues(ByVal CmdPos As Long, ByVal PosOffset As Long)
  'updates the command list so itemdata values have correct position Value
  'cmdpos is the index of first command that needs to be adjusted
  
  Dim i As Long
  
  
  
  'need to increment position info for all commands after the insert point
  
  For i = CmdPos To lstCommands.ListCount - 1
    lstCommands.ItemData(i) = lstCommands.ItemData(i) + PosOffset
  Next i
End Sub


Private Sub InsertCommand(NewData() As Byte, ByVal CmdPos As Long, ByVal CmdText As String, ByVal InsertBefore As Boolean, Optional ByVal DontUndo As Boolean = False)
  'inserts NewData into PicEdit and CmdText into cmd list at CmdPos
  
  Dim NextUndo As PictureUndo
  Dim bytData() As Byte
  Dim lngInsertPos As Long
  Dim lngRelation As Long
  
  On Error GoTo ErrHandler
  
  'whenever a command is inserted, set the flag so any additional
  'inserts will occur AFTER the currently selected command
  gInsertBefore = False
  
  'copy data to local array
  bytData = NewData
  
  'if at end, force insertbefore to true
  If CmdPos = lstCommands.ListCount - 1 Then
    InsertBefore = True
  End If
   
  If Not InsertBefore Then
    'insert at next cmd location
    CmdPos = CmdPos + 1
  End If
  lngInsertPos = lstCommands.ItemData(CmdPos)
  
  'if not skipping undo
  If Not DontUndo And Settings.PicUndo <> 0 Then
    'create new undo object
    Set NextUndo = New PictureUndo
    With NextUndo
      .UDAction = udpAddCmd
      .UDPicPos = lngInsertPos
      .UDCmdIndex = CmdPos
      .UDCmd = CmdText
    End With
    'add to undo
    AddUndo NextUndo
  End If
  
  'insert data
  PicEdit.Resource.InsertData bytData(), lngInsertPos
  
  'insert into cmd list
  lstCommands.AddItem CmdText, CmdPos
  
  'set position Value
  lstCommands.ItemData(CmdPos) = lngInsertPos
  
  'update position values in rest of tree
  UpdatePosValues CmdPos + 1, UBound(bytData()) + 1
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub LoadTestView()
  
  Dim rtn As Long
  
  On Error GoTo ErrHandler
  
  'if a test view is currently loaded,
  If Not (TestView Is Nothing) Then
    'unload it and release it
    TestView.Unload
    Set TestView = Nothing
  End If
  
  Set TestView = New AGIView
  On Error Resume Next
  'if in a game
  If GameLoaded Then
    'copy from game
    If Not Views(TestViewNum).Loaded Then
      Views(TestViewNum).Load
      TestView.SetView Views(TestViewNum)
      Views(TestViewNum).Unload
    Else
      TestView.SetView Views(TestViewNum)
    End If
  Else
    'load from file
    TestView.Import TestViewFile
  End If
  
  'if error
  If Err.Number <> 0 Then
    ErrMsgBox "Unable to load view resource due to error:", "Test view not set.", "Test View Error"
    Set TestView = Nothing
    Exit Sub
  End If
  On Error GoTo ErrHandler
  
  'reset loop and cel and direction, and motion
  CurTestLoop = 0
  CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
  CurTestCel = 0
  TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
  TestDir = 0
  'set cel height/width/transcolor
  CelWidth = TestView.Loops(CurTestLoop).Cels(CurTestCel).Width
  CelHeight = TestView.Loops(CurTestLoop).Cels(CurTestCel).Height
  CelTrans = TestView.Loops(CurTestLoop).Cels(CurTestCel).TransColor
  
  'if already in test mode (and we're changing
  'the view being used)
  If PicMode = pmTest Then
    'redraw picture to clear old testview
    DrawPicture
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function LoadCmdList(Optional ByVal NoUpdate As Boolean = False) As Boolean
  'loads the picture info into the command list
  'assumes there are no errors, since
  'a picture must successfully load before this routine is called
  
  Dim bytCmd As Byte
  Dim bytData() As Byte, lngEnd As Long
  Dim lngPos As Long
  Dim bytX As Byte, bytY As Byte
  Dim xdisp As Long, ydisp As Long
  Dim blnRelX As Boolean, PatternNum As Byte
  Dim strBrush As String, blnSplat As Boolean
  Dim blnErrors As Boolean
  
  On Error GoTo ErrHandler
  
  'get data (for better speed)
  bytData = PicEdit.Resource.AllData
  lngEnd = UBound(bytData)
  lngPos = 0
  
  With lstCommands
    'clear the tree
    .Clear
  
    'get first command
    bytCmd = bytData(lngPos)
    
    Do
      'add correct node to the list
      Select Case bytCmd
      Case &HFF ' end of file
        'add 'end'
        .AddItem LoadResString(DRAWFUNCTIONTEXT + 11)
        .ItemData(.ListCount - 1) = lngPos
        Exit Do
      
      Case &HF0 To &HFA
        'add command node
        .AddItem LoadResString(DRAWFUNCTIONTEXT + bytCmd - &HF0)
        
      Case Else ' < &HF0 or > &HFA ' an invalid command
        'invalid command  - note it
        .AddItem "ERR: (&H" & Hex2(bytCmd) & ")"
        blnErrors = True
      End Select
      
      'store position for this command
      .ItemData(.ListCount - 1) = lngPos
      
      'add command parameters
      Select Case bytCmd
      Case &HF0 'Change color and enable visual draw.
        lngPos = lngPos + 1
        If lngPos > lngEnd Then
          'error - no color value and end of picture
          'data found; probably bad picture resource data
          .List(.ListCount - 1) = "Vis: ERR --no data--"
          blnErrors = True
          Exit Do
        End If
        
        'get color
        bytCmd = bytData(lngPos)
        'RARE, but check for color out of bounds
        If bytCmd > 15 Then
          .List(.ListCount - 1) = "Vis: " & "ERR(0x" & Hex(bytCmd) & ")"
          blnErrors = True
        Else
          .List(.ListCount - 1) = "Vis: " & LoadResString(COLORNAME + bytCmd)
        End If
        
        'move pointer
        lngPos = lngPos + 1
        If lngPos > lngEnd Then
          'end of picture data found
          Exit Do
        End If
        'get next command
        bytCmd = bytData(lngPos)
        
      Case &HF2  'Change color and enable priority draw.
        lngPos = lngPos + 1
        If lngPos > lngEnd Then
          'error - no color value and end of picture
          'data found; probably bad picture resource data
          .List(.ListCount - 1) = "Pri: ERR --no data--"
          blnErrors = True
          Exit Do
        End If
        
        'get color
        bytCmd = bytData(lngPos)
        'RARE, but check for color out of bounds
        If bytCmd > 15 Then
          .List(.ListCount - 1) = "Pri: " & "ERR(0x" & Hex(bytCmd) & ")"
          blnErrors = True
        Else
          .List(.ListCount - 1) = "Pri: " & LoadResString(COLORNAME + bytCmd)
        End If
        
        'move pointer
        lngPos = lngPos + 1
        If lngPos > lngEnd Then
          'end of picture data found
          Exit Do
        End If
        'get next command
        bytCmd = bytData(lngPos)
        
      Case &HF1, &HF3 'Disable draw.
        'move pointer
        lngPos = lngPos + 1
        If lngPos > lngEnd Then
          'end of picture data found
          Exit Do
        End If
        'get next command
        bytCmd = bytData(lngPos)
        
      Case &HF4, &HF5, &HF6, &HF7, &HF8, &HFA
        Do
          'read in data until another command is found or unti
          'end is reached (which is not ideal)
          lngPos = lngPos + 1
          If lngPos > lngEnd Then
            Exit Do
          End If
          bytCmd = bytData(lngPos)
        Loop Until bytCmd >= &HF0
        
      Case &HF9 'Change pen size and style.
        'get pen size and style
        lngPos = lngPos + 1
        If lngPos > lngEnd Then
          'end of picture data found
          .List(.ListCount - 1) = "Set Pen:  ERR --no data--"
          Exit Do
        End If
        bytX = bytData(lngPos)
        If (bytX And &H20) / &H20 = 0 Then
          strBrush = "Solid "
          blnSplat = False
        Else
          strBrush = "Splatter "
          blnSplat = True
        End If
        If (bytX And &H10) / &H10 = 0 Then
          strBrush = strBrush & "Circle "
        Else
          strBrush = strBrush & "Rectangle "
        End If
        strBrush = strBrush & CStr(bytX And &H7)
  
        .List(.ListCount - 1) = "Set Pen: " & strBrush
        'get next command
        lngPos = lngPos + 1
        If lngPos > lngEnd Then
          'end of picture data found
          Exit Do
        End If
        bytCmd = bytData(lngPos)
        
      Case Else
        'it's an invalid (ignored command)
        'get next command
        lngPos = lngPos + 1
        If lngPos > lngEnd Then
          'end of picture data found
          Exit Do
        End If
        bytCmd = bytData(lngPos)
      End Select
    Loop Until lngPos > lngEnd
  End With
  
  'if end cmd not found, need to add it (and let user know)
  If bytCmd <> &HFF Then
    'add missing end
    PicEdit.Resource.InsertData &HFF
    'add 'end' node to list
    lstCommands.AddItem LoadResString(DRAWFUNCTIONTEXT + 11)
    lstCommands.ItemData(lstCommands.ListCount - 1) = lngPos
    'mark as dirty
    MarkAsDirty
    'restore cursor
    Screen.MousePointer = vbDefault
    MsgBoxEx "Picture is missing end-of-resource marker; marker has been added and picture" & vbNewLine & "loaded, but picture data may be corrupt.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Missing End Command in Picture", WinAGIHelp, "htm\agi\pictures.htm#ff"
  End If
  
  'if any bad commands or colors encountered
  If blnErrors Then
    'restore cursor
    Screen.MousePointer = vbDefault
    MsgBoxEx "One or more invalid commands and/or colors encountered; they are marked with 'ERR'." & vbNewLine & "This picture data may be corrupt.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Anomaly Found in Load Picture", WinAGIHelp, "htm\winagi\Picture_Editor.htm#picerrors"
  End If
  
  'select end cmd, and update listbox
  SelectCmd lstCommands.ListCount - 1, NoUpdate
  LoadCmdList = True
  
Exit Function

ErrHandler:
  '*'Debug.Assert False
  'some other error- let calling method deal with it
  LoadCmdList = True
End Function

Public Sub MenuClickDelete()
  'delete a coordinate or command
  
  Dim NewStyle As EPlotStyle, lngStartPos As Long
  Dim i As Long, NextUndo As PictureUndo
  Dim bytData() As Byte, blnSetPen As Boolean
  Dim lngCount As Long
  
  'if on the root,
  If lstCommands.ListIndex = -1 Then
    'exit sub
    Exit Sub
  End If
  
  'if on a command, (i.e. no coord is selected)
  If lstCoords.ListIndex = -1 Then
    '(can't delete END)
    '*'Debug.Assert lstCommands.ListIndex <> lstCommands.ListCount - 1
    
    'if more than one cmd selected
    If lstCommands.SelCount > 1 Then
      'check for 'set pen' cmd
      For i = 0 To lstCommands.SelCount - 1
        If Left$(lstCommands.List(SelectedCmd - i), 3) = "Set" Then
          blnSetPen = True
          Exit For
        End If
      Next i
      
      If blnSetPen Then
        'determine new pen style to apply from this point
        'start at first cmd above the top selected cmd
        i = SelectedCmd - lstCommands.SelCount
        Do Until i < 0
          'if this command is a set command
          If Left$(lstCommands.List(i), 3) = "Set" Then
            'get pen status
            NewStyle = IIf(InStr(lstCommands.List(i), "Solid"), psSolid, psSplatter)
            Exit Do
          End If
          'get previous cmd
          i = i - 1
        Loop
        If NewStyle <> CurrentPen.PlotStyle Then
          'adjust plot pattern starting with next command after the one being deleted
          '(this must be done BEFORE the resource is modified, otherwise the undo
          'feature won't work correctly)
          ReadjustPlotCoordinates SelectedCmd + 1, NewStyle
        End If
      End If
      
      'save position of first command that is selected
      lngStartPos = lstCommands.ItemData(SelectedCmd - lstCommands.SelCount + 1)
      
      If Settings.PicUndo <> 0 Then
        'create undo object
        Set NextUndo = New PictureUndo
        NextUndo.UDAction = udpDelCmd
        'save position of first command that is selected
        NextUndo.UDPicPos = lngStartPos
        'save cmd location and Count of commands
        NextUndo.UDCmdIndex = SelectedCmd
        NextUndo.UDCoordIndex = lstCommands.SelCount
        NextUndo.UDCmd = "Command"
        'copy data from picedit to undo array
        lngCount = lstCommands.ItemData(SelectedCmd + 1) - NextUndo.UDPicPos
        ReDim bytData(lngCount - 1)
        For i = 0 To lngCount - 1
          bytData(i) = PicEdit.Resource.Data(i + NextUndo.UDPicPos)
        Next i
        NextUndo.UDData = bytData
        'add to undo
        AddUndo NextUndo
      End If
      
      'delete data from array
'''      PicEdit.Resource.RemoveData NextUndo.UDPicPos, lstCommands.ItemData(SelectedCmd + 1) - NextUndo.UDPicPos
      PicEdit.Resource.RemoveData lngStartPos, lstCommands.ItemData(SelectedCmd + 1) - lngStartPos
      
      'delete the command box entries
      For i = SelectedCmd To SelectedCmd - lstCommands.SelCount + 1 Step -1
        lstCommands.RemoveItem i
      Next i
      
      'update remaining items (i+1 points to the cmd where updating needs to start)
      UpdatePosValues i + 1, -lngCount
      
      'select the cmd that is just after the deleted items
      SelectCmd i + 1, False
      
    Else
      'if command being deleted is a 'set pen' command
      If Left$(lstCommands.Text, 3) = "Set" Then
        'determine new pen style to apply from this point
        i = SelectedCmd - 1
        Do Until i < 0
          'if this command is a set command
          If Left$(lstCommands.List(i), 3) = "Set" Then
            'get pen status
            NewStyle = IIf(InStr(lstCommands.List(i), "Solid"), psSolid, psSplatter)
            Exit Do
          End If
          'get previous cmd
          i = i - 1
        Loop
        If NewStyle <> CurrentPen.PlotStyle Then
          'adjust plot pattern starting with next command after the one being deleted
          ReadjustPlotCoordinates SelectedCmd + 1, NewStyle
        End If
      End If
      
      'delete command
      DeleteCommand SelectedCmd
    End If
  Else
    'fill, plot and absolute lines allow deleting of individual coordinates
    'only last coordinate of other commands can be deleted
    Select Case lstCommands.Text
    Case "Abs Line", "Fill", "Plot"
      DeleteCoordinate lstCoords.ListIndex
      If lstCoords.ListCount <> 0 Then
        lstCoords_Click
      End If
      
    Case Else
      If lstCoords.ListIndex = lstCoords.ListCount - 1 Then
        DeleteCoordinate lstCoords.ListIndex
        If lstCoords.ListCount <> 0 Then
          lstCoords_Click
        End If
      End If
    End Select
  End If
End Sub

Public Sub MenuClickDescription(ByVal FirstProp As Long)

  'change description and ID
  Dim strID As String, strDescription As String
  
  On Error GoTo ErrHandler

  If FirstProp <> 1 And FirstProp <> 2 Then
    FirstProp = 1
  End If
  
  strID = PicEdit.ID
  strDescription = PicEdit.Description
  
  If GetNewResID(rtPicture, PicNumber, strID, strDescription, InGame, FirstProp) Then
    'save changes
    UpdateID strID, strDescription
  End If
    
  'force menu update
  AdjustMenus rtPicture, InGame, True, IsDirty
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickOpen()
  'implemented by frmMDIMain
  
End Sub

Public Sub MenuClickSave()
  'save this picture
  
  Dim rtn As VbMsgBoxResult
  Dim i As Long, blnLoaded As Boolean
  
  On Error GoTo ErrHandler
  
  'if in a game,
  If InGame Then
    'show wait cursor
    WaitCursor
    
    'get current load status
    blnLoaded = Pictures(PicNumber).Loaded
    
    'copy view back to game resource
    Pictures(PicNumber).SetPicture PicEdit
    
    'save the picture using save method
    Pictures(PicNumber).Save
    
    'copy back into edit object
    PicEdit.SetPicture Pictures(PicNumber)
    
    'setpicture copies load status to ingame pic resource; may need to unload it
    If Not blnLoaded Then
      Pictures(PicNumber).Unload
    End If
    
    'update preview
    UpdateSelection rtPicture, PicNumber, umPreview
  
    'if autoexporting,
    If Settings.AutoExport Then
      'export using default name
      PicEdit.Export ResDir & PicEdit.ID & ".agp"
      'reset ID (cuz
      PicEdit.ID = Pictures(PicEdit.Number).ID
    End If
    
    'restore cursor
    Screen.MousePointer = vbDefault
  Else
    'if no name yet,
    If LenB(PicEdit.Resource.ResFile) = 0 Then
      'use export to get a name
      MenuClickExport
      Exit Sub
    Else
      'show wait cursor
      WaitCursor
      
      'save the picture
      PicEdit.Export PicEdit.Resource.ResFile
    
      'restore cursor
      Screen.MousePointer = vbDefault
    End If
  End If
  
  'reset dirty flag
  IsDirty = False
  'reset caption
  Caption = sPICED & ResourceName(PicEdit, InGame, True)
  'disable save menu/button
  frmMDIMain.mnuRSave.Enabled = False
  frmMDIMain.Toolbar1.Buttons("save").Enabled = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickPrint()

  'show picture printing form
  Load frmPrint
  frmPrint.SetMode rtPicture, PicEdit, , InGame
  frmPrint.Show vbModal, frmMDIMain
End Sub

Public Sub MenuClickExport()

  Dim blnOldFlag As Boolean
  
  On Error GoTo ErrHandler
  
  'need to track dirty flag
  blnOldFlag = PicEdit.IsDirty
  
  'export the agi resource
  If ExportPicture(PicEdit, InGame) Then
    If Not InGame Then
      'reset dirty flag and caption
      IsDirty = False
      Caption = sPICED & PicEdit.ID
      
      'disable save menu/button
      frmMDIMain.mnuRSave.Enabled = False
      frmMDIMain.Toolbar1.Buttons("save").Enabled = False
    Else
      'for ingame resources, PicEdit is not actually
      'the ingame resource, but only a copy that can be edited
      'because the resource ID is changed to match savefile
      'name during the export operation, the ID needs to be
      'forced back to the correct Value
      PicEdit.ID = Pictures(PicNumber).ID
    End If
  Else
    'if export returns false, it was either canceled
    'or an image was exported
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickImport()
  
  Dim tmpPic As AGIPicture
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'this method is only called by the Main form's Import function
  'the MainDialog object will contain the name of the file
  'being imported.
  
  'steps to import are to import the picture to tmp object
  'clear the existing Image, copy tmpobject to this item
  'and reset it
  
  Set tmpPic = New AGIPicture
  On Error Resume Next
  tmpPic.Import MainDialog.FileName
  If Err.Number <> 0 Then
    ErrMsgBox "An error occurred while importing this picture:", "", "Import Picture Error"
    Set tmpPic = Nothing
    Exit Sub
  End If
      'now check to see if it's a valid picture resource (by trying to reload it)
  tmpPic.Load
  If Err.Number <> 0 Then
    ErrMsgBox "Error reading Picture data:", "This is not a valid picture resource.", "Invalid Picture Resource"
    Set tmpPic = Nothing
    Exit Sub
  End If
  
  'clear drawing surfaces
  picVisual.Cls
  picPriority.Cls
  
  'clear picture
  PicEdit.Clear
  'copy tmppicture data to picedit
  PicEdit.Resource.InsertData tmpPic.Resource.AllData, 0
  'remove the last byte (it is left over from the insert process)
  PicEdit.Resource.RemoveData PicEdit.Resource.Size - 1
  
  'discard the temp pic
  tmpPic.Unload
  Set tmpPic = Nothing
  
  'redraw tree
  LoadCmdList
  
  'select the end
  SelectCmd lstCommands.ListCount - 1, False
  
  'refresh palette
  picPalette.Refresh
  
  'clear the undo buffer
  If UndoCol.Count > 0 Then
    For i = UndoCol.Count To 1 Step -1
      UndoCol.Remove i
    Next i
    SetEditMenu
  End If
  
  'mark as dirty
  MarkAsDirty
  
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
  Dim blnDontAsk As Boolean
  
  If InGame Then
    'ask if resource should be exported
    If Settings.AskExport Then
      rtn = MsgBoxEx("Do you want to export '" & PicEdit.ID & "' before removing it from your game?", _
                          vbQuestion + vbYesNoCancel, "Export Picture Before Removal", , , _
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
      rtn = MsgBoxEx("Removing '" & PicEdit.ID & "' from your game." & vbCrLf & vbCrLf & "Select OK to proceed, or Cancel to keep it in game.", _
                      vbQuestion + vbOKCancel, "Remove Picture From Game", , , _
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
    
    ' now remove the pic
    RemovePicture PicNumber
   
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
'''    If Pictures.Count = 256 Then
'''      MsgBox "Maximum number of pics already exist in this game. Remove one or more existing pics, and then try again.", vbInformation + vbOKOnly, "Can't Add Picture"
'''      Exit Sub
'''    End If
    
    'show add resource form
    With frmGetResourceNum
      .ResType = rtPicture
      .WindowFunction = grAddInGame
      'setup before loading so ghosts don't show up
      .FormSetup
      'suggest ID based on filename/ID
      If Len(PicEdit.ID) > 0 Then
        .txtID.Text = Replace(FileNameNoExt(PicEdit.ID), " ", vbNullString)
      End If
      .Show vbModal, frmMDIMain
    
      'if user makes a choice
      If Not .Canceled Then
        'store number
        PicNumber = .NewResNum
        'new id
        PicEdit.ID = .txtID.Text
        'add picture
        AddNewPicture PicNumber, PicEdit
        
        'copy the pic back (to ensure internal variables are copied)
        PicEdit.Clear
        PicEdit.SetPicture Pictures(PicNumber)
        
        'now we can unload the newly added picture;
        Pictures(PicNumber).Unload
        
        'update caption and properties
        Caption = sPICED & ResourceName(PicEdit, True, True)
        
        'set ingame flag
        InGame = True
        'reset dirty flag
        IsDirty = False
        
        'change menu caption
        frmMDIMain.mnuRInGame.Caption = "Remove from Game"
        frmMDIMain.Toolbar1.Buttons("remove").Image = 10
        frmMDIMain.Toolbar1.Buttons("remove").ToolTipText = "Remove from Game"
      End If
    End With
    
    Unload frmGetResourceNum
  End If
End Sub
Public Sub MenuClickRenumber()
  
  'renumbers a resource
  
  Dim NewResNum As Byte
  
  On Error GoTo ErrHandler:
  
  'if not in a game
  If Not InGame Then
    Exit Sub
  End If
  
  'get new number
  NewResNum = RenumberResource(PicNumber, rtPicture)
  
  'if changed
  If NewResNum <> PicNumber Then
    'copy renumbered picture into PicEdit object
    PicEdit.SetPicture Pictures(NewResNum)
    
    'update number
    PicNumber = NewResNum
    
    'update caption
    Caption = sPICED & ResourceName(PicEdit, InGame, True)
    If IsDirty Then
      Caption = sDM & Caption
    End If
  End If
Exit Sub

ErrHandler:
  Resume Next
End Sub
Public Sub MenuClickECustom1()
  
  'in edit mode, split commands
  'in test mode, choose test view
  
  Dim rtn As Long
  
  On Error GoTo ErrHandler
  
  Select Case PicMode
  Case pmEdit
    'split cmd
    SplitCommand lstCoords.ListIndex
  
  Case pmTest
    'get a test view
    GetTestView
    
    'redraw
    DrawPicture
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickECustom2()
  
  'in edit mode, join commands
  'in test mode, set test options
  
  Dim rtn As Long
  
  On Error GoTo ErrHandler
  
  Select Case PicMode
  Case pmEdit
    'join commands
    JoinCommands SelectedCmd
    
  Case pmTest
    'if in test mode, and in motion
    If TestDir <> odStopped Then
      'stop motion and stop cycling
      TestDir = odStopped
    End If
    
    'if cycling, stop; doesn't matter if
    'at rest or in motion
    tmrTest.Enabled = False
    
    'if testview not loaded,
    If TestView Is Nothing Then
      'load one first
      MenuClickECustom1
      'if still no testview
      If TestView Is Nothing Then
        'exit
        Exit Sub
      End If
    End If
  
    Load frmPicTestOptions
    With frmPicTestOptions
      'set global testpic settings
      Settings.PicTest = TestSettings
      'set form properties
      .SetOptions TestView
      
      'show options form
      .Show vbModal, frmMDIMain
      
      'if not canceled
      If Not .Canceled Then
        'retreive option values
        TestSettings = Settings.PicTest
        'if test loop and/or cel are NOT auto, force current loop/cel
        If TestSettings.TestLoop <> -1 Then
          CurTestLoop = TestSettings.TestLoop
          CurTestLoopCount = TestView.Loops(CurTestLoop).Cels.Count
          'just in case, check current cel; if it exceeds
          'loop count, reset it to zero
          If CurTestCel > CurTestLoopCount - 1 Then
            CurTestCel = CurTestLoopCount - 1
          End If
          If TestSettings.TestCel <> -1 Then
            CurTestCel = TestSettings.TestCel
          End If
          'if either loop or cel is forced, we
          'have to update cel data
          TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
        End If
        
        'update cel height/width/transcolor
        CelWidth = TestView.Loops(CurTestLoop).Cels(CurTestCel).Width
        CelHeight = TestView.Loops(CurTestLoop).Cels(CurTestCel).Height
        CelTrans = TestView.Loops(CurTestLoop).Cels(CurTestCel).TransColor
        
        'set timer based on speed
        Select Case TestSettings.ObjSpeed
        Case 0  'slow
          tmrTest.Interval = 200
        Case 1  'normal
          tmrTest.Interval = 50
        Case 2  'fast
          tmrTest.Interval = 13
        Case 3  'fastest
          tmrTest.Interval = 1
        End Select
      End If
      
      'redraw cel at current position
      DrawPicture False, True, OldCel.X, OldCel.Y
      
      'now enable cycling
      tmrTest.Enabled = TestSettings.CycleAtRest
    End With
    
    'unload options form
    Unload frmPicTestOptions
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickCustom1()

  'show bkgd configuration options
  ToggleBkgd True, True
End Sub
Public Sub MenuClickCustom3(Optional ByVal Toggle As Boolean = False)
  
  On Error GoTo ErrHandler
  
  'remove the background
  
  'if currently showing background,
  If PicEdit.BkgdShow Then
    ToggleBkgd False
  End If
  
  'set image to nothing
  Set BkgdImage = Nothing
  PicEdit.BkgdImgFile = ""
  
  'update menus
  frmMDIMain.mnuRCustom2.Visible = False
  frmMDIMain.mnuRCustom3.Visible = False
  
  'if in game
  If InGame Then
    'update the ingame pic
    With Pictures(PicEdit.Number)
      .BkgdImgFile = ""
      .BkgdShow = False
      .BkgdPosition = ""
      .BkgdSize = ""
      .BkgdTrans = 0
      ' save it (this will only write the properties
      ' since the real picture is not being edited
      ' in this piceditor)
      .Save
    End With
  End If
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
  
  'if exiting due to error on startup, picedit is set to nothing
  If PicEdit Is Nothing Then
    Exit Function
  End If
  
  'if the picture needs to be saved,
  '(number is set to -1 if closing is forced)
  If IsDirty And PicNumber <> -1 Then
    rtn = MsgBox("Do you want to save changes to " & PicEdit.ID & " ?", vbYesNoCancel, "Picture Editor")
    
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
  Resume Next
End Function

Public Sub ResizePictures()

  Dim rtn As Long
  Dim NewH As Long, NewW As Long
  Dim VisCH As Single, VisCV As Single
  Dim PriCH As Single, PriCV As Single
  Dim PriVis As Boolean, VisVis As Boolean
  
  On Error GoTo ErrHandler
  
  'if mouse scrolling, need to get correct fraction values
  If blnWheel Then
    'get fraction values based on
    'location of cursor
    GetZoomCenter
  Else
    HFraction = 0.5
    VFraction = 0.5
  End If
  
  'show wait cursor
  WaitCursor
  
  'can we disable updating to reduce flicker?
  'Yes, but we need to capture visible state first
  'because sending this message makes the picture
  'control return FALSE for visible state
  VisVis = picVisSurface.Visible
  PriVis = picPriSurface.Visible
  If VisVis Then
    SendMessage picVisSurface.hWnd, WM_SETREDRAW, 0, 0
    SendMessage picVisual.hWnd, WM_SETREDRAW, 0, 0
  End If
  If PriVis Then
    SendMessage picPriSurface.hWnd, WM_SETREDRAW, 0, 0
    SendMessage picPriority.hWnd, WM_SETREDRAW, 0, 0
  End If
  
  'calculate new height/width
  NewH = 168 * ScaleFactor
  NewW = 320 * ScaleFactor

'to keep resized windows properly centered on same pixel
'we need to calculate the scroll percentage:
'
'    C = SB0 / TH0 + FRAC * PH0 / TH0
'
'then, after resizing, use that value to determine
'correct new scroll value:
'
'    SB1 = C * TH1 - FRAC * PH1
'
' also need to compensate for margins; as well as
' using a default percentage of 50% when scrollbars
' are currently not visible


  If hsbVis.Visible Then
    VisCH = (hsbVis.Value + PE_MARGIN) / (picVisual.Width + PE_MARGIN * 2) + HFraction * picVisSurface.Width / (picVisual.Width + PE_MARGIN * 2)
  Else
    VisCH = HFraction * picVisSurface.Width / picVisual.Width
  End If
  If vsbVis.Visible Then
    VisCV = (vsbVis.Value + PE_MARGIN) / (picVisual.Height + PE_MARGIN * 2) + VFraction * picVisSurface.Height / (picVisual.Height + PE_MARGIN * 2)
  Else
    VisCV = VFraction * picVisSurface.Height / picVisual.Height
  End If
  If hsbPri.Visible Then
    PriCH = (hsbPri.Value + PE_MARGIN) / (picPriority.Width + PE_MARGIN * 2) + HFraction * picPriSurface.Width / (picPriority.Width + PE_MARGIN * 2)
  Else
    PriCH = HFraction * picPriSurface.Width / picPriority.Width
  End If
  If vsbPri.Visible Then
    PriCV = (vsbPri.Value + PE_MARGIN) / (picPriority.Height + PE_MARGIN * 2) + VFraction * picPriSurface.Height / (picPriority.Height + PE_MARGIN * 2)
  Else
    PriCV = VFraction * picPriSurface.Height / picPriority.Height
  End If
  
  'resize
  picVisual.Width = NewW
  picVisual.Height = NewH
  picPriority.Width = NewW
  picPriority.Height = NewH
  
  'force the pictures to redraw with new size
  DrawPicture True
  
  'if on a line command coordinate
  If CurCmdIsLine Then
    'draw temp line based on current node
    DrawTempLine False, 0, 0
  End If
  
  'update panels so scrollbars are set properly
  'this also draws the pics
  UpdatePanels picSplitH.Top, picSplitV.Left
  
  UpdateStatusBar
  
  'if a selection is visible,
  If SelSize.X > 0 And SelSize.Y > 0 Then
    'need to redraw selection shapes
    ShowCmdSelection
  End If
  
  'if only one selected command AND it has coords AND tool is 'none'
  If lstCommands.SelCount = 1 And lstCoords.ListCount > 0 And SelectedTool = ttEdit Then
    'only if in edit mode
    If PicMode = pmEdit Then
      HighlightCoords
    End If
  End If

'  SB1 = C * TH1 - FRAC * PH1

  'if scrollbars visible, position so 'center' is still at center
  If hsbVis.Visible Then
    'what's the new scroll value?
    NewW = VisCH * (picVisual.Width + PE_MARGIN * 2) - HFraction * picVisSurface.Width - PE_MARGIN
    If NewW > hsbVis.Max Then
      NewW = hsbVis.Max
    End If
    If NewW < hsbVis.Min Then
      NewW = hsbVis.Min
    End If
    If hsbVis.Value <> NewW Then
      hsbVis.Value = NewW
    Else
      'make sure picVisual.Left matches
      If picVisual.Left <> -NewW Then
        picVisual.Left = -NewW
      End If
    End If
  End If
  
  If vsbVis.Visible Then
    'what's the new scroll value?
    NewH = VisCV * (picVisual.Height + PE_MARGIN * 2) - VFraction * picVisSurface.Height - PE_MARGIN
    If NewH > vsbVis.Max Then
      NewH = vsbVis.Max
    End If
    If NewH < vsbVis.Min Then
      NewH = vsbVis.Min
    End If
    If vsbVis.Value <> NewH Then
      vsbVis.Value = NewH
    Else
      'make sure picVisual.Top matches
      If picVisual.Top <> -NewH Then
        picVisual.Top = -NewH
      End If
    End If
  End If
  
  If hsbPri.Visible Then
    'what's the new scroll value?
    NewW = PriCH * (picPriority.Width + PE_MARGIN * 2) - HFraction * picPriSurface.Width - PE_MARGIN
    If NewW > hsbPri.Max Then
      NewW = hsbPri.Max
    End If
    If NewW < hsbPri.Min Then
      NewW = hsbPri.Min
    End If
    If hsbPri.Value <> NewW Then
      hsbPri.Value = NewW
    Else
      'make sure picPriority.Left matches
      If picPriority.Left <> -NewW Then
        picPriority.Left = -NewW
      End If
    End If
  End If
  
  If vsbPri.Visible Then
    'what's the new scroll value?
    NewH = PriCV * (picPriority.Height + PE_MARGIN * 2) - VFraction * picPriSurface.Height - PE_MARGIN
    If NewH > vsbPri.Max Then
      NewH = vsbPri.Max
    End If
    If NewH < vsbPri.Min Then
      NewH = vsbPri.Min
    End If
    If vsbPri.Value <> NewH Then
      vsbPri.Value = NewH
    Else
      'make sure picPriority.Top matches
      If picPriority.Top <> -NewH Then
        picPriority.Top = -NewH
      End If
    End If
  End If
  
  'reset mousepointer
  Screen.MousePointer = vbDefault
  
  'now show the pics
  If VisVis Then
    SendMessage picVisSurface.hWnd, WM_SETREDRAW, 1, 0
    SendMessage picVisual.hWnd, WM_SETREDRAW, 1, 0
  End If
  If PriVis Then
    SendMessage picPriSurface.hWnd, WM_SETREDRAW, 1, 0
    SendMessage picPriority.hWnd, WM_SETREDRAW, 1, 0
  End If
  picVisual.Refresh
  picPriority.Refresh
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub RefreshPens(Optional ByVal ForceNew As Boolean = False)
  'if the current command matches a pen that is changed,
  'we need to change it; otherwise, a new command
  'will be inserted
  
  'this function assumes selected command is the location to refresh

  Dim VisCmdIndex As Long, PriCmdIndex As Long, PenCmdIndex As Long
  Dim bytData() As Byte
  Dim strBrush As String
  Dim i As Long
  Dim NextUndo As PictureUndo
  
  On Error GoTo ErrHandler
  
  'if current pen and selected pen already match
  If CurrentPen.PlotShape = SelectedPen.PlotShape And _
     CurrentPen.PlotSize = SelectedPen.PlotSize And _
     CurrentPen.PlotStyle = SelectedPen.PlotStyle And _
     CurrentPen.PriColor = SelectedPen.PriColor And _
     CurrentPen.VisColor = SelectedPen.VisColor Then
      'no action required
    Exit Sub
  End If
  
  'determine if current command is a pen command, and if so,
  'what type
  
  'set indices for set cmds to -1 as default; a positive value
  'will mean that type is currently theselected command
  VisCmdIndex = -1
  PriCmdIndex = -1
  PenCmdIndex = -1
  
  'check for set commands
  Select Case CmdType(lstCommands.List(SelectedCmd))
  Case 0
    'skip- command is a draw command
    
  Case 1  'visual
    VisCmdIndex = SelectedCmd
   
  Case 2  'priority
    PriCmdIndex = SelectedCmd
      
  Case 3  'pen
    PenCmdIndex = SelectedCmd
    
  End Select
  
  'now check to see what changed; could be one or more
  'visual, priority or pen setting, so we check them all
  
  'if visual colors are different, we need to update or insert
  'a visual pen command
  If CurrentPen.VisColor <> SelectedPen.VisColor Then
    'if selected cmd is NOT vispen OR forcing a new pen,
    If VisCmdIndex = -1 Or ForceNew Then
      'if pen is being turned off
      If SelectedPen.VisColor = agNone Then
        'insert a visual disable cmd
        ReDim bytData(0)
        bytData(0) = CByte(dfDisableVis)
        InsertCommand bytData, SelectedCmd, "Vis: Off", gInsertBefore
      Else
        'insert a visual enable cmd
        ReDim bytData(1)
        bytData(0) = CByte(dfEnableVis)
        bytData(1) = CByte(SelectedPen.VisColor)
        InsertCommand bytData, SelectedCmd, "Vis: " & LoadResString(COLORNAME + SelectedPen.VisColor), gInsertBefore
      End If
      'change selection to this command
      SelectedCmd = lstCommands.NewIndex
    Else
      'update existing vis cmd
      ChangeColor VisCmdIndex, SelectedPen.VisColor
    End If
      
    'set CurrentPen to new Value
    CurrentPen.VisColor = SelectedPen.VisColor
  End If
  
  'if priority colors are different, we need to
  'update or insert a priority pen command
  If CurrentPen.PriColor <> SelectedPen.PriColor Then
    'if selected cmd is NOT pripen OR forcing a new pen,
    If PriCmdIndex = -1 Or ForceNew Then
      'if pen is being turned off
      If SelectedPen.PriColor = agNone Then
        'insert a priority disable cmd
        ReDim bytData(0)
        bytData(0) = CByte(dfDisablePri)
        InsertCommand bytData, SelectedCmd, "Pri: Off", gInsertBefore
      Else
        'insert a Priority enable command
        ReDim bytData(1)
        bytData(0) = CByte(dfEnablePri)
        bytData(1) = CByte(SelectedPen.PriColor)
        InsertCommand bytData, SelectedCmd, "Pri: " & LoadResString(COLORNAME + SelectedPen.PriColor), gInsertBefore
      End If
      'change selection to this command
      SelectedCmd = lstCommands.NewIndex
    Else
      'update existing pri cmd
      ChangeColor PriCmdIndex, SelectedPen.PriColor
    End If
    
    'set CurrentPen to new color
    CurrentPen.PriColor = SelectedPen.PriColor
  End If
    
  'if selected pen different from the current pen
  'we need to update or insert a pen
  If CurrentPen.PlotShape <> SelectedPen.PlotShape Or _
     CurrentPen.PlotSize <> SelectedPen.PlotSize Or _
     CurrentPen.PlotStyle <> SelectedPen.PlotStyle Then
    
    'dynamic array so it can be passed as a variable
    ReDim bytData(1)
    
    'build set pen command byte
    If SelectedPen.PlotStyle = psSolid Then
      strBrush = "Solid "
      bytData(1) = 0
    Else
      strBrush = "Splatter "
      bytData(1) = &H20
    End If
    If SelectedPen.PlotShape = psCircle Then
      strBrush = strBrush & "Circle "
    Else
      strBrush = strBrush & "Rectangle "
      bytData(1) = bytData(1) Or &H10
    End If
    strBrush = strBrush & CStr(SelectedPen.PlotSize)
    bytData(1) = bytData(1) + SelectedPen.PlotSize
    
    'if selected cmd is NOT a plotpen or forcing a new pen
    If PenCmdIndex = -1 Or ForceNew Then
      'insert a set plot pen command
      bytData(0) = dfChangePen
      
      'if plot style is different
      If CurrentPen.PlotStyle <> SelectedPen.PlotStyle Then
        'adjust plot commands
        ReadjustPlotCoordinates SelectedCmd, SelectedPen.PlotStyle
      End If
        
      'then add the new 'set pen' command
      InsertCommand bytData, SelectedCmd, "Set Pen: " & strBrush, gInsertBefore
      'change selection to this command
      SelectedCmd = lstCommands.NewIndex
    Else
      'update the pen command at this location
      
      'if plot style is different
      If CurrentPen.PlotStyle <> SelectedPen.PlotStyle Then
        'adjust plot commands
        ReadjustPlotCoordinates PenCmdIndex + 1, SelectedPen.PlotStyle
      End If
      
      If Settings.PicUndo <> 0 Then
        'add undo object
        Set NextUndo = New PictureUndo
        With NextUndo
          .UDAction = udpChangePlotPen
          .UDPicPos = lstCommands.ItemData(PenCmdIndex)
          .UDCmdIndex = PenCmdIndex
          bytData(0) = PicEdit.Resource.Data(NextUndo.UDPicPos + 1)
          .UDData = bytData()
          .UDText = lstCommands.List(PenCmdIndex)
        End With
        AddUndo NextUndo
      End If
      
      'change existing command byte
      PicEdit.Resource.Data(NextUndo.UDPicPos + 1) = bytData(1)
      'and text
      lstCommands.List(PenCmdIndex) = "Set Pen: " & strBrush
    End If
      
    'set pen values
    CurrentPen.PlotShape = SelectedPen.PlotShape
    CurrentPen.PlotSize = SelectedPen.PlotSize
    CurrentPen.PlotStyle = SelectedPen.PlotStyle
  End If
  
  're-select cmd to force everything to update
  SelectCmd SelectedCmd, False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub UpdateScrollbars()

  On Error GoTo ErrHandler
  
  'set scroll bar values
  hsbVis.LargeChange = picVisSurface.Width * LG_SCROLL
  hsbVis.SmallChange = picVisSurface.Width * SM_SCROLL
  vsbVis.LargeChange = picVisSurface.Height * LG_SCROLL
  vsbVis.SmallChange = picVisSurface.Height * SM_SCROLL
  hsbPri.LargeChange = picPriSurface.Width * LG_SCROLL
  hsbPri.SmallChange = picPriSurface.Width * SM_SCROLL
  vsbPri.LargeChange = picPriSurface.Height * LG_SCROLL
  vsbPri.SmallChange = picPriSurface.Height * SM_SCROLL
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub UpdateStatusBar()
  
  Dim blnErrFix As Boolean
  
  'set status bar indicators based on current state of game
  On Error GoTo ErrHandler
  
  ' if this form is not active, just exit
  If Not frmMDIMain.ActiveForm Is Me Then
    Exit Sub
  End If
    
  'scale
  With MainStatusBar
    .Panels("Scale").Text = "Scale: " & CStr(ScaleFactor)
    
    ' if showing test object coordinates
    If StatusSrc Then
      'use test object position
      .Panels("CurX").Text = "vX: " & CStr(OldCel.X)
      .Panels("CurY").Text = "vY: " & CStr(OldCel.Y)
      .Panels("PriBand").Text = "vBand: " & GetPriBand(OldCel.Y, PicEdit.PriBase)
      SendMessage .hWnd, WM_SETREDRAW, 0, 0
      .Panels("PriBand").Picture = imlPriBand.ListImages(GetPriBand(OldCel.Y, PicEdit.PriBase) - 3).Picture
      SendMessage .hWnd, WM_SETREDRAW, 1, 0
    End If
    
    'when showing cursor position, X, Y, priband are done in mousemove method
    
    'mode
    'tool
    Select Case PicMode
    Case pmEdit
      .Panels("Mode").Text = "Edit"
      .Panels("Tool").Text = LoadResString(PICTOOLTYPETEXT + SelectedTool)
      .Panels("Anchor").Visible = (SelectedTool = ttSelectArea)
      .Panels("Block").Visible = (SelectedTool = ttSelectArea)
      
      If SelectedTool = ttSelectArea Then
        If shpVis.Visible Then
          .Panels("Anchor").Text = "Anchor: " & SelStart.X & ", " & SelStart.Y
          .Panels("Block").Text = "Block: " & SelStart.X & ", " & SelStart.Y & ", " & SelStart.X + SelSize.X - 1 & ", " & SelStart.Y + SelSize.Y - 1
        Else
          .Panels("Anchor").Text = "Anchor: " '& PicPt.X & ", " & PicPt.Y
          .Panels("Block").Text = "Block: " ' & PicPt.X & ", " & PicPt.Y
        End If
      End If
      
    Case pmTest
      .Panels("Mode").Text = "Test"
      
      'if movement stopped
      If StopReason > 0 Then
        .Panels("Tool").Text = LoadResString(STOPREASONTEXT + StopReason)
        StopReason = 0
      Else
        'clear tool panel
        .Panels("Tool").Text = vbNullString
      End If
    End Select
  End With
Exit Sub

ErrHandler:
  'if error is due to wrong status bar
  If Err.Number = 35601 And Not blnErrFix Then
    'force update and retry
    blnErrFix = True
    AdjustMenus rtPicture, InGame, True, IsDirty
    Resume
  End If
End Sub

Private Sub DrawLine(ByVal X1 As Long, ByVal Y1 As Long, ByVal X2 As Long, ByVal Y2 As Long)
  
  Dim xPos As Long, yPos As Long
  Dim DY As Long, DX As Long
  Dim vDir As Long, hDir As Long
  Dim XC As Long, YC As Long, MaxDelta As Long
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  '*'Debug.Assert X1 >= 0 And X1 <= 159
  '*'Debug.Assert X2 >= 0 And X2 <= 159
  '*'Debug.Assert Y1 >= 0 And Y1 <= 167
  '*'Debug.Assert Y1 >= 0 And Y1 <= 167
  
  'determine height/width
  DY = Y2 - Y1
  vDir = Sgn(DY)
  DX = X2 - X1
  hDir = Sgn(DX)
  
  'if a point, vertical line, or horizontal line
  '(it's easier to combine all these options, since
  'graphics methods are easier to work with than
  'buffer memory)
  If DY = 0 Or DX = 0 Then
    'convert line to top-bottom/left-right format so graphics methods
    'work correctly
    If Y2 < Y1 Then
      'swap
      yPos = Y1
      Y1 = Y2
      Y2 = yPos
    End If
    If X2 < X1 Then
      xPos = X1
      X1 = X2
      X2 = xPos
    End If

    If SelectedPen.VisColor < agNone Then
      picVisual.Line (X1 * ScaleFactor * 2, Y1 * ScaleFactor)-((X2 + 1) * ScaleFactor * 2 - 1, (Y2 + 1) * ScaleFactor - 1), EGAColor(SelectedPen.VisColor), BF
    End If
    If SelectedPen.PriColor < agNone Then
      picPriority.Line (X1 * ScaleFactor * 2, Y1 * ScaleFactor)-((X2 + 1) * ScaleFactor * 2 - 1, (Y2 + 1) * ScaleFactor - 1), EGAColor(SelectedPen.PriColor), BF
    End If
  
  Else
    'this line drawing function EXACTLY matches the Sierra
    'drawing function
    
    'set the starting point
    If SelectedPen.VisColor < agNone Then
      picVisual.Line (X1 * ScaleFactor * 2, Y1 * ScaleFactor)-Step(ScaleFactor * 2 - 1, ScaleFactor - 1), EGAColor(SelectedPen.VisColor), BF
    End If
    If SelectedPen.PriColor < agNone Then
      picPriority.Line (X1 * ScaleFactor * 2, Y1 * ScaleFactor)-Step(ScaleFactor * 2 - 1, ScaleFactor - 1), EGAColor(SelectedPen.PriColor), BF
    End If
    xPos = X1
    yPos = Y1
  
    'invert DX and DY if they are negative
    If DY < 0 Then
      DY = DY * -1
    End If
    If (DX < 0) Then
      DX = DX * -1
    End If
  
    'set up the loop, depending on which direction is largest
    If DX >= DY Then
      MaxDelta = DX
      YC = DX \ 2
      XC = 0
    Else
      MaxDelta = DY
      XC = DY \ 2
      YC = 0
    End If
  
    For i = 1 To MaxDelta
      YC = YC + DY
      If YC >= MaxDelta Then
        YC = YC - MaxDelta
        yPos = yPos + vDir
      End If
    
      XC = XC + DX
      If XC >= MaxDelta Then
        XC = XC - MaxDelta
        xPos = xPos + hDir
      End If
  
      If SelectedPen.VisColor < agNone Then
        picVisual.Line (xPos * ScaleFactor * 2, yPos * ScaleFactor)-Step(ScaleFactor * 2 - 1, ScaleFactor - 1), EGAColor(SelectedPen.VisColor), BF
      End If
      If SelectedPen.PriColor < agNone Then
        picPriority.Line (xPos * ScaleFactor * 2, yPos * ScaleFactor)-Step(ScaleFactor * 2 - 1, ScaleFactor - 1), EGAColor(SelectedPen.PriColor), BF
      End If
    Next i
  End If
Exit Sub

ErrHandler:
  Resume Next
End Sub

Private Sub DrawCircle(ByVal StartX As Long, ByVal StartY As Long, ByVal EndX As Long, ByVal EndY As Long)
  
  'draws a circle/ellipse that is bounded by start/end points
  
  Dim DX As Long, DY As Long
  Dim a As Double, b As Double
  Dim a2b2 As Double
  Dim pX As Long, pY As Long
  Dim i As Long, j As Long, k As Long
  Dim dS As Double
  Dim DelPt As Boolean
  Dim cy As Double, cx As Double
  Dim s1 As Double, s2 As Double
  
  'ensure we are in a upperleft-lower right configuration
  If StartX > EndX Then
    i = StartX
    StartX = EndX
    EndX = i
  End If
  If StartY > EndY Then
    i = StartY
    StartY = EndY
    EndY = i
  End If
  
  DX = EndX - StartX
  DY = EndY - StartY
  
  If DX = 0 Or DY = 0 Then
    'just draw a line
    DrawLine StartX, StartY, EndX, EndY
    
  ElseIf DX = 1 Or DY = 1 Then
    'draw a simple box
    DrawLine StartX, StartY, EndX, StartY
    DrawLine EndX, StartY, EndX, EndY
    DrawLine EndX, EndY, StartX, EndY
    DrawLine StartX, EndY, StartX, StartY
    
    'set segment data
    Segments = 1
    ArcPt(0).X = Int(DX / 2)
    ArcPt(0).Y = 0
    ArcPt(1).X = 0
    ArcPt(1).Y = Int(DY / 2)
  Else
    'get ellipse parameters
    a = Int(DX / 2)
    b = Int(DY / 2)
    a2b2 = a ^ 2 / b ^ 2
    
    'start with Y values;
    'increment until slope is >=1
    i = 0
    Do
      ArcPt(i).Y = i
      'calculate x Value for this Y
      cx = a * Sqr(1 - ArcPt(i).Y ^ 2 / b ^ 2)
      'round it (0.3 is an empirical Value that seems to
      'result in more accurate circles)
      ArcPt(i).X = vCint(cx - 0.3)
      
      'if past limit
      If i / cx * a2b2 >= 1 Then
        Exit Do
      End If
      'increment Y
      i = i + 1
      'continue until last point reached
      '(necessary in case tall skinny oval
      'is drawn; slope won't reach 1 before last point)
    Loop While i < b
    
    'start with last x
    j = ArcPt(i - 1).X
    'now, decrement x until we get to zero
    Do
      ArcPt(i).X = j
      'calculate Y Value for this x
      cy = b * Sqr(1 - (ArcPt(i).X) ^ 2 / a ^ 2)
      'round it
      '(vCint doesn't work quite right; Int seems to work better
      'ArcPt(i).Y = Int(cY)
      
      'using vCint with a modifier seems to work ok?
      ArcPt(i).Y = vCint(cy - 0.3)

      'decrement x, increment counter
      j = j - 1
      i = i + 1
    Loop While j >= 0
    
    'segments is equal to i-1
    '**NOTE that Segments is equal to UPPER BOUND of array; not total number
    'of segment points
    'e.g., 3 segments means upper bound of array is 3 (0,1,2,3) and total
    'number of points is 4
    Segments = i - 1
    
    'zero out next point to avoid conflict
    ArcPt(i + 1).X = 0
    ArcPt(i + 1).Y = 0
    
    'strip out any zero delta points
    'and any points that are on exact 45 line
    i = 1
    Do
      'if same
      If ArcPt(i).X = ArcPt(i - 1).X And ArcPt(i).Y = ArcPt(i - 1).Y Then
        DelPt = True
      'if horizontal line
      ElseIf ArcPt(i).X = ArcPt(i - 1).X And ArcPt(i).X = ArcPt(i + 1).X Then
        DelPt = True
      'if vertical line
      ElseIf ArcPt(i).Y = ArcPt(i - 1).Y And ArcPt(i).Y = ArcPt(i + 1).Y Then
        DelPt = True
      'if line has a slope of 1
      ElseIf (CLng(ArcPt(i).X) - ArcPt(i - 1).X = CLng(ArcPt(i - 1).Y) - ArcPt(i).Y) And (CLng(ArcPt(i + 1).X) - ArcPt(i).X = CLng(ArcPt(i).Y) - ArcPt(i + 1).Y) Then
        DelPt = True
      Else
        DelPt = False
      End If
      If DelPt Then
        'move all segments down one space
        For k = i + 1 To Segments
          ArcPt(k - 1) = ArcPt(k)
        Next k
        ArcPt(Segments).X = 0
        ArcPt(Segments).Y = 0
        Segments = Segments - 1
        i = i - 1
      End If
      i = i + 1
    Loop While i < Segments
    
    'if more than one segment
    If Segments > 1 Then
      'strip out any points that create uneven slopes
      i = 1
      Do
        If ArcPt(i - 1).X = ArcPt(i).X Then
          s1 = -160
        Else
          s1 = (CLng(ArcPt(i - 1).Y) - ArcPt(i).Y) / (CLng(ArcPt(i - 1).X) - ArcPt(i).X)
        End If
        If ArcPt(i).X = ArcPt(i + 1).X Then
          s2 = -160
        Else
          s2 = (CLng(ArcPt(i).Y) - ArcPt(i + 1).Y) / (CLng(ArcPt(i).X) - ArcPt(i + 1).X)
        End If
        If s1 >= s2 Or ArcPt(i).X < ArcPt(i + 1).X Then
          'remove point (move all segments down one space)
          For k = i + 1 To Segments
            ArcPt(k - 1) = ArcPt(k)
          Next k
          ArcPt(Segments).X = 0
          ArcPt(Segments).Y = 0
          Segments = Segments - 1
          'back up to recheck slope of altered segment
          i = i - 1
        End If
        i = i + 1
      Loop While i < Segments
    End If
    
    'now draw the arc segments
    pX = StartX
    pY = StartY + ArcPt(Segments).Y
    For i = 1 To Segments
      DrawLine pX, pY, StartX + ArcPt(0).X - ArcPt(i).X, StartY + ArcPt(Segments).Y - ArcPt(i).Y
      pX = StartX + ArcPt(0).X - ArcPt(i).X
      pY = StartY + ArcPt(Segments).Y - ArcPt(i).Y
    Next i
    For i = Segments To 0 Step -1
      DrawLine EndX - ArcPt(0).X + ArcPt(i).X, StartY + ArcPt(Segments).Y - ArcPt(i).Y, pX, pY
      pX = EndX - ArcPt(0).X + ArcPt(i).X
      pY = StartY + ArcPt(Segments).Y - ArcPt(i).Y
    Next i
    For i = 0 To Segments
      DrawLine pX, pY, EndX - ArcPt(0).X + ArcPt(i).X, EndY - ArcPt(Segments).Y + ArcPt(i).Y
      pX = EndX - ArcPt(0).X + ArcPt(i).X
      pY = EndY - ArcPt(Segments).Y + ArcPt(i).Y
    Next i
    For i = Segments To 0 Step -1
      DrawLine StartX + ArcPt(0).X - ArcPt(i).X, EndY - ArcPt(Segments).Y + ArcPt(i).Y, pX, pY
      pX = StartX + ArcPt(0).X - ArcPt(i).X
      pY = EndY - ArcPt(Segments).Y + ArcPt(i).Y
    Next i
  End If
End Sub

Private Sub UpdatePanels(ByVal SplitLocH As Single, ByVal SplitLocV As Single)

  Dim OldWidth As Single
  
  On Error GoTo ErrHandler
  
  'if minimized, OR not the active form,
  If (Me.WindowState = vbMinimized) Or (Not frmMDIMain.ActiveForm Is Me) And Me.Visible Then
    'just exit
    Exit Sub
  End If
  
  'split vertical first
  
  'resize spliticons to match form height
  If picPalette.Top - picSplitV.Top > 10 Then
    picSplitV.Height = picPalette.Top - picSplitV.Top
  End If
  picSplitVIcon.Height = picSplitV.Height
  
  'adjust cmd list height
  If (picPalette.Top - lstCommands.Top) * 0.67 > 10 Then
    lstCommands.Height = (picPalette.Top - lstCommands.Top) * 0.67
  End If
  lblCoords.Top = lstCommands.Top + lstCommands.Height
  lstCoords.Top = lblCoords.Top + lblCoords.Height
  If picPalette.Top - lstCoords.Top > 30 Then
    lstCoords.Height = picPalette.Top - lstCoords.Top
  End If
  
  'get current split pos
  '(extent of command tree)
  OldWidth = lstCommands.Left + lstCommands.Width
  
  'if different in width,
  If OldWidth <> SplitLocV Then
    'set rescource list width
    lstCommands.Width = SplitLocV - lstCommands.Left
    lstCoords.Width = lstCommands.Width
    
    'position splitter
    picSplitV.Left = SplitLocV
    
    'position drawing surfaces
    picVisSurface.Left = SplitLocV + SPLIT_WIDTH
    picPriSurface.Left = SplitLocV + SPLIT_WIDTH
  End If
  
  
  
  'split horizontal next
  
  'if not hiding visual work area
  If SplitLocH > SPLIT_HEIGHT Then
    'reposition scrollbars
    hsbVis.Top = SplitLocH - hsbVis.Height
    hsbVis.Left = SplitLocV + SPLIT_WIDTH
    hsbVis.Width = CalcWidth - hsbVis.Left
    
    vsbVis.Left = CalcWidth - vsbVis.Width
    vsbVis.Height = SplitLocH - vsbVis.Top
  
    'determine if visual scrollbars are needed
    vsbVis.Visible = ((picVisSurface.Top + picVisual.Height + 2 * PE_MARGIN) > (SplitLocH))
    'take into account vertical scrollbar width, if visible
    hsbVis.Visible = ((picVisSurface.Left + picVisual.Width + 2 * PE_MARGIN) > CalcWidth + vsbVis.Visible * vsbVis.Width)
    'check vertical again, in case horizontal scroll bar affects surface
    vsbVis.Visible = (picVisSurface.Top + picVisual.Height + 2 * PE_MARGIN > SplitLocH + hsbVis.Visible * hsbVis.Height)
    
    'reposition picture if scrollbars not needed
    If Not hsbVis.Visible Then
      picVisual.Left = PE_MARGIN
    End If
    If Not vsbVis.Visible Then
      picVisual.Top = PE_MARGIN
    End If
    
    'display work surface
    picVisSurface.Visible = True
    'adjust drawing surface
    picVisSurface.Left = hsbVis.Left
    If hsbVis.Top - (Not hsbVis.Visible) * hsbVis.Height - picVisSurface.Top > 10 Then
      picVisSurface.Height = hsbVis.Top - (Not hsbVis.Visible) * hsbVis.Height - picVisSurface.Top
    End If
    If hsbVis.Width + (vsbVis.Visible) * vsbVis.Width > 10 Then
      picVisSurface.Width = hsbVis.Width + (vsbVis.Visible) * vsbVis.Width
    End If
    'set scroll bar values, if they are visible
    If hsbVis.Visible Then
      'if calculated Max is <0
      If picVisual.Width + 2 * PE_MARGIN - (CalcWidth - picVisSurface.Left + vsbVis.Visible * vsbVis.Width) < 0 Then
        hsbVis.Max = hsbVis.Value
      Else
        hsbVis.Max = picVisual.Width + 2 * PE_MARGIN - (CalcWidth - picVisSurface.Left + vsbVis.Visible * vsbVis.Width)
      End If
    End If
    If vsbVis.Visible Then
      'if calculated Max is <0
      If picVisual.Height + 2 * PE_MARGIN - (SplitLocH - SPLIT_HEIGHT / 2 - picVisSurface.Top + hsbVis.Visible * hsbVis.Height) < 0 Then
        vsbVis.Max = vsbVis.Value
      Else
        vsbVis.Max = picVisual.Height + 2 * PE_MARGIN - (SplitLocH - SPLIT_HEIGHT / 2 - picVisSurface.Top + hsbVis.Visible * hsbVis.Height)
      End If
    End If
    
    'if both are visible
    If vsbVis.Visible And hsbVis.Visible Then
      'move scroll bars back from the corner (so they don't overlap)
      If hsbVis.Width - vsbVis.Width > 10 Then
        hsbVis.Width = hsbVis.Width - vsbVis.Width
      End If
      If vsbVis.Height - hsbVis.Height > 10 Then
        vsbVis.Height = vsbVis.Height - hsbVis.Height
      End If
    End If
  Else
    hsbVis.Visible = False
    vsbVis.Visible = False
    picVisSurface.Visible = False
  End If
  
  'position priority work surface
  picPriSurface.Top = SplitLocH + SPLIT_HEIGHT
  
  'if not hiding priority work area
  If SplitLocH < picPalette.Top - SPLIT_HEIGHT Then
    hsbPri.Top = picPalette.Top - hsbPri.Height
    hsbPri.Left = SplitLocV + SPLIT_WIDTH
    hsbPri.Width = CalcWidth - hsbPri.Left
    vsbPri.Left = CalcWidth - vsbPri.Width
    vsbPri.Top = SplitLocH + SPLIT_HEIGHT
    vsbPri.Height = picPalette.Top - vsbPri.Top
  
    'determine if priority scrollbars are needed
    vsbPri.Visible = ((picPriSurface.Top + picPriority.Height + 2 * PE_MARGIN) > (CalcHeight - picPalette.Height))
    'take into account vertical scrollbar width, if visible
    hsbPri.Visible = ((picPriSurface.Left + picPriority.Width + 2 * PE_MARGIN) > CalcWidth + vsbPri.Visible * vsbPri.Width)
    'check vertical again, in case horizontal scroll bar affects surface
    vsbPri.Visible = (picPriSurface.Top + picPriority.Height + 2 * PE_MARGIN > (CalcHeight - picPalette.Height + hsbPri.Visible * hsbPri.Height))
    
    'reposition picture if scrollbars not needed
    If Not hsbPri.Visible Then
      picPriority.Left = PE_MARGIN
    End If
    If Not vsbPri.Visible Then
      picPriority.Top = PE_MARGIN
    End If
    
    'show priority work area
    picPriSurface.Visible = True
    'adjust drawing surface
    picPriSurface.Left = hsbPri.Left
    If hsbPri.Top - picPriSurface.Top - (Not hsbPri.Visible) * hsbPri.Height > 10 Then
      picPriSurface.Height = hsbPri.Top - picPriSurface.Top - (Not hsbPri.Visible) * hsbPri.Height
    End If
    If hsbPri.Width + (vsbPri.Visible) * vsbPri.Width > 10 Then
      picPriSurface.Width = hsbPri.Width + (vsbPri.Visible) * vsbPri.Width
    End If
    'set scroll bar values if visible
    If hsbPri.Visible Then
      If picPriority.Width + 2 * PE_MARGIN - (CalcWidth - picPriSurface.Left + vsbPri.Visible * vsbPri.Width) < 0 Then
        hsbPri.Max = hsbPri.Value
      Else
        hsbPri.Max = picPriority.Width + 2 * PE_MARGIN - (CalcWidth - picPriSurface.Left + vsbPri.Visible * vsbPri.Width)
      End If
    End If
    If vsbPri.Visible Then
      If picPriority.Height + 2 * PE_MARGIN - (CalcHeight - picPriSurface.Top - picPalette.Height + hsbPri.Visible * hsbPri.Height) < 0 Then
        vsbPri.Max = vsbPri.Value
      Else
        vsbPri.Max = picPriority.Height + 2 * PE_MARGIN - (CalcHeight - picPriSurface.Top - picPalette.Height + hsbPri.Visible * hsbPri.Height)
      End If
    End If
    
    'if both are visible
    If vsbPri.Visible And hsbPri.Visible Then
      'move scroll bars back from corner (to avoid overlapping)
      If hsbPri.Width - vsbPri.Width > 10 Then
        hsbPri.Width = hsbPri.Width - vsbPri.Width
      End If
      If vsbPri.Height - hsbPri.Height > 10 Then
        vsbPri.Height = vsbPri.Height - hsbPri.Height
      End If
    End If
  Else
    hsbPri.Visible = False
    vsbPri.Visible = False
    picPriSurface.Visible = False
  End If
  
  'position splitter
  picSplitH.Top = SplitLocH
  picSplitH.Left = SplitLocV + SPLIT_WIDTH + 15
  picSplitH.Width = CalcWidth - picSplitH.Left - 15
  
  'update scrollbar values
  UpdateScrollbars
  
  SetEditMenu
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub UpdateToolBar()
  
  Dim i As Long
  
  With Toolbar1.Buttons
    'enable/disable other editing buttons (index of 16 and higher)
    'based on mode, and if drawing is selected
    For i = 18 To 30 '16 To 28 'editsel to size
      If i < 20 Then '18 Then
        'select and area select always available in edit mode
        .Item(i).Enabled = (PicMode = pmEdit)
      Else
        'other tools only available in edit mode if at least one pen is ON
        .Item(i).Enabled = (PicMode = pmEdit) And ((SelectedPen.VisColor < agNone) Or (SelectedPen.PriColor < agNone))
      End If
    Next i
    'if neither draw color is selected and using a drawing tool
    If (SelectedPen.VisColor = agNone) And (SelectedPen.PriColor = agNone) And SelectedTool <> ttEdit And SelectedTool <> ttSelectArea Then
      'switch to select tool
      SelectedTool = ttEdit
      .Item("select").Value = tbrPressed
      'normal cursor
      SetCursors pcEdit
    End If
    
    'adjust other tools on the toolbar
    
    If .Item("size").Image <> 19 + SelectedPen.PlotSize Then
      .Item("size").Image = 19 + SelectedPen.PlotSize
    End If
    If .Item("style").Image <> 15 + SelectedPen.PlotShape + 2 * SelectedPen.PlotStyle Then
      .Item("style").Image = 15 + SelectedPen.PlotShape + 2 * SelectedPen.PlotStyle
    End If
  End With
  
  'repaint color palette
  picPalette_Paint
  
  'update statusbar
  UpdateStatusBar
End Sub

Private Sub Form_Activate()

  On Error GoTo ErrHandler
  
  Dim tmpTool As TPicToolTypeEnum
  
  'if minimized, exit
  '(to deal with occasional glitch causing focus to lock up)
  If Me.WindowState = vbMinimized Then
    Exit Sub
  End If
  
  'if hiding prevwin on lost focus, hide it now
  If Settings.HidePreview Then
    '*'Debug.Assert Me.Visible
    PreviewWin.Hide
  End If
 
  'temporarily se tool to nothing, so the click that activates the form
  'doesn't draw anything
  tmpTool = SelectedTool
  Activating = True
  
  SelectedTool = ttEdit
  
  'if visible,
  If Visible Then
    'force resize
    Form_Resize
  End If
  
  'show picture menus, and enable editing
  AdjustMenus rtPicture, InGame, True, IsDirty
  
  'set edit menu
  SetEditMenu
  
  'restore selected tool
  SelectedTool = tmpTool
  
  'if findform is visible,
  If FindForm.Visible Then
    'hide it it
    FindForm.Visible = False
  End If
  
  'force form update
  UpdateToolBar
  
  Activating = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub Form_Deactivate()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub

Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)

'*'Debug.Print "PE keydown: "; Shift, KeyCode
  'always check for help first
  If Shift = 0 And KeyCode = vbKeyF1 Then
    MenuClickHelp
    KeyCode = 0
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
    Case vbKeyZ
      'undo
      If frmMDIMain.mnuEUndo.Enabled Then
        MenuClickUndo
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
        MenuClickPaste
        KeyCode = 0
    
    Case vbKeyA 'select all
      If frmMDIMain.mnuESelectAll.Enabled Then
        MenuClickSelectAll
        KeyCode = 0
      End If
      
    Case vbKeyT 'split = custom1
      If frmMDIMain.mnuECustom1.Enabled And PicMode = pmEdit Then
        MenuClickECustom1
        KeyCode = 0
      End If
    
    Case vbKeyJ 'join = custom2
      If frmMDIMain.mnuECustom2.Enabled And PicMode = pmEdit Then
        MenuClickECustom2
        KeyCode = 0
      End If
    End Select
    
  Case 0
    'no shift, ctrl, alt
    Select Case KeyCode
    Case vbKeyEscape
      'if a coord is selected, unselect it
      If lstCoords.ListIndex <> -1 Then
        CodeClick = True
        lstCommands_Click
      End If
    
    Case vbKeyDelete
      If frmMDIMain.mnuEDelete.Enabled Then
        'same as menuclickdelete
        MenuClickDelete
      End If
      
    Case vbKeyUp, vbKeyLeft
      If PicMode = pmEdit Then
        'if a coord is selected
        If lstCoords.ListIndex <> -1 Then
          'if not on first coord,
          If lstCoords.ListIndex > 0 Then
            'move up one coord pt
            lstCoords.ListIndex = lstCoords.ListIndex - 1
          End If
        Else
          'if not on first cmd
          If SelectedCmd > 0 Then
            'move up one cmd
            SelectCmd SelectedCmd - 1, False
          End If
        End If
        'reset keycode to prevent double movement of cursor
        KeyCode = 0
      End If
      
    Case vbKeyDown, vbKeyRight
      If PicMode = pmEdit Then
        'if a coord is selected
        If lstCoords.ListIndex <> -1 Then
          'if not on last coord,
          If lstCoords.ListIndex < lstCoords.ListCount - 1 Then
            'move down one coord pt
            lstCoords.ListIndex = lstCoords.ListIndex + 1
          End If
        Else
          'if not on last cmd
          If SelectedCmd <> lstCommands.ListCount - 1 Then
            'move down one cmd
            SelectCmd SelectedCmd + 1, False
          End If
        End If
        'reset keycode to prevent double-movement of cursor
        KeyCode = 0
      End If
    
    Case vbKeySpace
      'toggle status bar display if in test mode
      If PicMode = pmTest Then
        'toggle status bar source
        StatusSrc = Not StatusSrc
        'force update of statusbar
        With MainStatusBar
          If StatusSrc Then
            'use test object position
            .Panels("CurX").Text = "vX: " & CStr(OldCel.X)
            .Panels("CurY").Text = "vY: " & CStr(OldCel.Y)
            .Panels("PriBand").Text = "vBand: " & GetPriBand(OldCel.Y, PicEdit.PriBase)
            SendMessage .hWnd, WM_SETREDRAW, 0, 0
            .Panels("PriBand").Picture = imlPriBand.ListImages(GetPriBand(OldCel.Y, PicEdit.PriBase) - 3).Picture
            SendMessage .hWnd, WM_SETREDRAW, 1, 0
          Else
            'use cusor position (which is currently unknown)
            .Panels("CurX").Text = "X: "
            .Panels("CurY").Text = "Y: "
            .Panels("PriBand").Text = "Band: "
            .Panels("PriBand").Picture = Nothing
          End If
        End With
      End If
    End Select
    
    'if in test mode
    If PicMode = pmTest Then
      ChangeDir KeyCode
    End If
  
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
    
  Case vbAltMask
    Select Case KeyCode
    Case vbKeyB
      'toggle background
      If frmMDIMain.mnuRCustom3.Enabled Then
        ToggleBkgd Not PicEdit.BkgdShow
        KeyCode = 0
      End If
    
    Case vbKeyM
      'toggle mode
      MenuClickReplace
      
    Case vbKeyS
      'toggle visual/priority surfaces if
      'in single pane mode
      If frmMDIMain.mnuERedo.Visible Then
        MenuClickRedo
      End If
      
    Case vbKeyP
      'toggle priority bands
      If frmMDIMain.mnuEFind.Enabled Then
        MenuClickFind
        KeyCode = 0
      End If
    
    Case vbKeyV
      'in test mode only, choose a test view
      If frmMDIMain.mnuECustom1.Enabled And PicMode = pmTest Then
        MenuClickECustom1
        KeyCode = 0
      End If
    
    Case vbKeyO
      'in test mode only display test view options
      If frmMDIMain.mnuECustom2.Enabled And PicMode = pmTest Then
        MenuClickECustom2
        KeyCode = 0
      End If
    End Select
    
  Case vbShiftMask + vbCtrlMask
    If KeyCode = vbKeyS Then
      'save Image as ...
      MenuClickCustom1
    End If
    
    If KeyCode = vbKeyG Then
      'export as gif
      ExportPicAsGif
    End If
  
  Case vbCtrlMask + vbAltMask
    Select Case KeyCode
    Case vbKeyB
      'show background options dialog
      ToggleBkgd True, True
  
    Case vbKeyP
      'adjust priority base
      If frmMDIMain.mnuEFindAgain.Enabled Then
        MenuClickFindAgain
        KeyCode = 0
      End If
    End Select
  End Select
  KeyCode = 0
  Shift = 0
  
End Sub

Private Sub Form_Load()
  
  Dim i As Integer, SplitV As Long
  
  On Error GoTo ErrHandler
  
  CalcWidth = Me.ScaleWidth
  If CalcWidth < MIN_WIDTH Then CalcWidth = MIN_WIDTH
  CalcHeight = Me.ScaleHeight
  If CalcHeight < MIN_HEIGHT Then CalcHeight = MIN_HEIGHT
  
#If DEBUGMODE <> 1 Then
  'subclass the listbox for mouse scrolling
  PrevLBWndProc = SetWindowLong(Me.lstCommands.hWnd, GWL_WNDPROC, AddressOf LBWndProc)
  'subclass the form for mouse scrolling
  PrevPEWndProc = SetWindowLong(Me.hWnd, GWL_WNDPROC, AddressOf ScrollWndProc)
#End If

  'set undo collection
  Set UndoCol = New Collection
    
  'get default scale
  ScaleFactor = Settings.PicScale.Edit
  'default priority bands
  ShowBands = Settings.ShowBands
  'get default cursor mode
  CursorMode = Settings.CursorMode
  VCColor = EGAColor(4) 'red
  PCColor = EGAColor(3) 'cyan
  
  'calculate new height/width
  picVisual.Height = 168 * ScaleFactor
  picPriority.Height = picVisual.Height
  picVisual.Width = 320 * ScaleFactor
  picPriority.Width = picVisual.Width
  
  
  picSplitH.Height = SPLIT_HEIGHT
  picSplitHIcon.Visible = False
  picSplitV.Top = lstCommands.Top
  
  'select edit mode and select tool
  PicMode = pmEdit
  SelectedTool = ttEdit
  
  'set initial vertical split location
  SplitV = MIN_SPLIT_V * 1.5
  'set rescource list width
  lstCommands.Width = SplitV - lstCommands.Left
  lstCoords.Width = lstCommands.Width
  'position splitter
  picSplitV.Left = SplitV
  'position drawing surfaces
  picVisSurface.Left = SplitV + SPLIT_WIDTH
  picPriSurface.Left = SplitV + SPLIT_WIDTH
    
  'set initial horizontal split location
  If Settings.SplitWindow Then
    'default is half and half
    SplitRatio = 0.5
    
    UpdatePanels (CalcHeight - picPalette.Height - SPLIT_HEIGHT - picVisSurface.Top) / 2 + picVisSurface.Top, MIN_SPLIT_V * 1.5
  Else
    OneWindow = 1
    ' update panels
    UpdatePanels picPalette.Top - SPLIT_HEIGHT, SplitV
  End If
  
  'update panels and status bar
  UpdateToolBar
 
  'position images and set scrollbar min values
  If picVisual.Visible Then
    picVisual.Move PE_MARGIN, PE_MARGIN
    hsbVis.Min = -PE_MARGIN
    vsbVis.Min = -PE_MARGIN
    hsbVis.Value = -PE_MARGIN
    vsbVis.Value = -PE_MARGIN
  End If
  If picPriority.Visible Then
    picPriority.Move PE_MARGIN, PE_MARGIN
    hsbPri.Min = -PE_MARGIN
    vsbPri.Min = -PE_MARGIN
    hsbPri.Value = -PE_MARGIN
    vsbPri.Value = -PE_MARGIN
  End If
  
  'position drawing surfaces and set scrollbar values
  picVisSurface.Left = lstCommands.Left + lstCommands.Width + 2
  picPriSurface.Left = picVisSurface.Left
  hsbVis.Left = picVisSurface.Left
  hsbPri.Left = picVisSurface.Left
  picSplitH.Left = picVisSurface.Left
  picSplitHIcon.Left = picVisSurface.Left
  
  'set stretchmode?
  Dim rtn As Long
  'set setetchmode so pixels are nice and crisp when stretched
  rtn = SetStretchBltMode(picVisual.hDC, COLORONCOLOR)
  rtn = SetStretchBltMode(picPriority.hDC, COLORONCOLOR)
  
  Set MonoMask = New VBitmap
'  Set VisPic = New VBitmap
'  Set PriPic = New VBitmap
  
  MonoMask.NewBMP 160, 168, 1, 1
'  VisPic.NewBMP 160, 168, 1, 4
'  PriPic.NewBMP 160, 168, 1, 4
  
  'get default pic test settings
  With TestSettings
    .ObjSpeed = GameSettings.GetSetting(sPICTEST, "Speed", DEFAULT_PICTEST_OBJSPEED)
      If .ObjSpeed < 0 Then .ObjSpeed = 0
      If .ObjSpeed > 3 Then .ObjSpeed = 3
    .ObjPriority = GameSettings.GetSetting(sPICTEST, "Priority", DEFAULT_PICTEST_OBJPRIORITY)
      If .ObjPriority < 4 Then .ObjPriority = 4
      If .ObjPriority > 16 Then .ObjPriority = 16
    .ObjRestriction = GameSettings.GetSetting(sPICTEST, "Restriction", DEFAULT_PICTEST_OBJRESTRICTION)
      If .ObjRestriction < 0 Then .ObjRestriction = 0
      If .ObjRestriction > 2 Then .ObjRestriction = 2
    .Horizon = GameSettings.GetSetting(sPICTEST, "Horizon", DEFAULT_PICTEST_HORIZON)
      If .Horizon < 0 Then .Horizon = 0
      If .Horizon > 167 Then .Horizon = 167
    .IgnoreHorizon = GameSettings.GetSetting(sPICTEST, "IgnoreHorizon", DEFAULT_PICTEST_IGNOREHORIZON)
    .IgnoreBlocks = GameSettings.GetSetting(sPICTEST, "IgnoreBlocks", DEFAULT_PICTEST_IGNOREBLOCKS)
    .CycleAtRest = GameSettings.GetSetting(sPICTEST, "CycleAtRest", DEFAULT_PICTEST_CYCLEATREST)
    .TestCel = -1
    .TestLoop = -1
    'set timer based on speed
    Select Case .ObjSpeed
    Case 0  'slow
      tmrTest.Interval = 200
    Case 1  'normal
      tmrTest.Interval = 50
    Case 2  'fast
      tmrTest.Interval = 13
    Case 3  'fastest
      tmrTest.Interval = 1
    End Select
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub lstCommands_Click()

  Dim i As Long, j As Long
  Dim lngSelItem As Long, lngInSel As Long '(0 no selection yet; 1 = selstart found; 2 = selend found; 3 = multiple selections found)
  Dim lngCount As Long
  
  On Error GoTo ErrHandler
  
  'if ignoring the switch,
  If NoSelect Then
    Exit Sub
  End If
  
  'need to differentiate between clicks called in code, vs
  'actual clicks by users!
  If CodeClick Then
    '*'Debug.Assert False
  Else
    'an actual user click
    
    'always set InsertBefore to TRUE whenever a new
    'command is selected so that the next command
    'added will bump currently selected command down
    gInsertBefore = True
  End If
  'always reset codeclick flag
  CodeClick = False
  
  
  'keep track of what was clicked
  lngSelItem = lstCommands.ListIndex
  lngCount = lstCommands.ListCount - 1
  
  'prevent recursion
  NoSelect = True
  
  'disable painting
  SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
  
  'if more than one item selected
  If lstCommands.SelCount > 1 Then
    'step through all items;
    For i = 0 To lngCount
      Select Case lngInSel
      Case 0  'nothing selected yet
        If lstCommands.Selected(i) Then
          'start found
          lngInSel = 1
        End If
        
      Case 1 'selstart found; looking for selend
        If Not lstCommands.Selected(i) Then
          'end found
          lngInSel = 2
        End If
        
      Case 2 'selend found; don't want anything else selected
        'if another item is selected, we have a problem!
        If lstCommands.Selected(i) Then
          'bad selection found!
          lngInSel = 3
          Exit For
        End If
      End Select
    Next i
  End If
  
  'if there is a bad multiple selection, unselect everything but the curent item
  If lngInSel = 3 Then
    For i = 0 To lngCount
      lstCommands.Selected(i) = (i = lngSelItem)
    Next i
  End If
  
  'reenable painting
  SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
  
  'check again for more than one item selected,
  If lstCommands.SelCount > 1 Then
    'first, ensure 'End' cmd is not one of the selected items
    lstCommands.Selected(lstCommands.ListCount - 1) = False
    If lstCommands.ListIndex = lstCommands.ListCount - 1 Then
      CodeClick = True
      lstCommands.ListIndex = lstCommands.ListIndex - 1
      lstCommands.Selected(lstCommands.ListIndex) = True
      CodeClick = False
    End If
    
    For i = 0 To lstCommands.ListCount - 1
      If lstCommands.Selected(i) Then
        'find the last item of the selection
        For j = i + 1 To lstCommands.ListCount - 1
          If Not lstCommands.Selected(j) Then
            Exit For
          End If
        Next j
        j = j - 1
        SelectedCmd = j
        Exit For
      End If
    Next i
    
    'get start and end coords of selection
    '(sets Value of selstart and selsize)
    GetSelectionBounds SelectedCmd, lstCommands.SelCount, True
  Else
    'move? or edit coords?
    'depends on how selected? ctrl perhaps?
    SelectedCmd = lstCommands.ListIndex

    'reset selection
    SelStart.X = 0
    SelStart.Y = 0
    SelSize.X = 0
    SelSize.Y = 0
    ShowCmdSelection 'calling this with all zeros hides the selection
  End If
  
  'always cancel any drawing operation
  PicDrawMode = doNone
  
  'set CurCmdIsLine to false until proven otherwise
  CurCmdIsLine = False
  
  'always set cursor highlighting to match selection status
  tmrSelect.Enabled = (SelSize.X > 0 And SelSize.Y > 0)
  
  ClearCoordList

  'draw picture
  DrawPicture
  
  'get current tool status
  CurrentPen = PicEdit.CurrentToolStatus
  
  'set selected tools to match current
  SelectedPen = CurrentPen
  
  'build coord list
  BuildCoordList SelectedCmd
  
  'update toolbar
  UpdateToolBar
  
  'reset edit menu
  SetEditMenu
  
  're-enable selection
  NoSelect = False
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
  
End Sub

Private Sub lstCommands_DblClick()

  'make sure only one item is selected
  If lstCommands.SelCount = 1 Then
    GetSelectionBounds lstCommands.ListIndex, 1, True
  End If
End Sub

Private Sub lstCommands_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub


Private Sub lstCommands_KeyDown(KeyCode As Integer, Shift As Integer)

  'block all others
  If Shift <> vbShiftMask And KeyCode <> vbKeyDown And KeyCode <> vbKeyUp Then
    'ignore keycode
    KeyCode = 0
    Shift = 0
  End If
End Sub


Private Sub lstCommands_KeyPress(KeyAscii As Integer)

  'need to ignore key presses to stop the auto-selection feature of the list box
  KeyAscii = 0
End Sub

Private Sub lstCommands_KeyUp(KeyCode As Integer, Shift As Integer)

  'ignore keycode
  KeyCode = 0
  Shift = 0
End Sub


Private Sub lstCommands_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim lngClickRow As Long
  
  On Error GoTo ErrHandler
  
  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
  
  'if nothing selected
  If lstCommands.SelCount = 0 Then
    Exit Sub
  End If
  
  Select Case Button
  Case vbRightButton
  'if right button
    lngClickRow = (Y / ScreenTWIPSY) \ SendMessage(lstCommands.hWnd, LB_GETITEMHEIGHT, 0, 0) + lstCommands.TopIndex
    If lngClickRow > lstCommands.ListCount - 1 Then
      lngClickRow = lstCommands.ListCount - 1
    End If
    
    'if on a cmd that is NOT selected
    If Not lstCommands.Selected(lngClickRow) Then
      'select it
      SelectCmd lngClickRow, False
    End If
    'reset edit menu first
    SetEditMenu
    'make sure this form is the active form
    If Not (frmMDIMain.ActiveForm Is Me) Then
      'set focus before showing the menu
      Me.SetFocus
    End If
    'need doevents so form activation occurs BEFORE popup
    'otherwise, errors will be generated because of menu
    'adjustments that are made in the form_activate event
    SafeDoEvents
    'show edit menu
    PopupMenu frmMDIMain.mnuEdit, , lstCommands.Left + X / ScreenTWIPSX, lstCommands.Top + Y / ScreenTWIPSY
  Case vbLeftButton
    'ignore ctrl; should not allow selecting commands that are not adjacent
    If Shift = vbCtrlMask Then
      Shift = 0
    End If
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub lstCommands_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

'  Dim lngClickRow As Long
'
'  'determine which row mouse is over
'  lngClickRow = (Y / ScreenTWIPSY) \ SendMessage(lstCommands.hwnd, LB_GETITEMHEIGHT, 0, 0) + lstCommands.TopIndex
'  If lngClickRow > lstCommands.ListCount - 1 Then
'    Exit Sub
'  End If
'  If lngClickRow < 0 Then
'    Exit Sub
'  End If

End Sub

Private Sub lstCoords_Click()

  Dim i As Long
  
  On Error GoTo ErrHandler
  
  If NoSelect Then
    NoSelect = False
    Exit Sub
  End If
  
  If lstCoords.ListIndex = -1 Then
    Exit Sub
  End If
  
  'always cancel any drawing operation
  PicDrawMode = doNone
  
  'set selection to nothing
  '(this hides any selected graphics)
  SelStart.X = 0
  SelStart.Y = 0
  SelSize.X = 0
  SelSize.Y = 0
  ShowCmdSelection
  
  'set CurCmdIsLine to false until proven otherwise
  CurCmdIsLine = False
  
  'if NOT in select mode (i.e. selected tool = none)
  If SelectedTool <> ttEdit Then
    'change edit mode by clicking toolbar
    '(this will reset ListIndex to -1, so need to hold on to it, and restore after this call)
    i = lstCoords.ListIndex
    NoSelect = True
    Toolbar1.Buttons("select").Value = tbrPressed
    Toolbar1_ButtonClick Toolbar1.Buttons("select")
    NoSelect = True
    lstCoords.ListIndex = i
  End If
  '*'Debug.Assert lstCoords.ListIndex <> -1
      
  'current command is a line if coordinate is NOT a plot or paint
  CurCmdIsLine = (lstCommands.Text <> "Plot" And lstCommands.Text <> "Fill")
  'extract position
  
  CurPt = ExtractCoordinates(lstCoords.Text)
  
  'enable cursor highlighting if edit tool selected
  tmrSelect.Enabled = (SelectedTool = ttEdit)
  
  'if original wingagi cursor mode AND timer is enabled,
  If CursorMode = pcmWinAGI And tmrSelect.Enabled Then
    'save area under cursor
    BitBlt Me.hDC, 0, 0, 6 * ScaleFactor, 3 * ScaleFactor, picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
    BitBlt Me.hDC, 0, 12, 6 * ScaleFactor, 3 * ScaleFactor, picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
  End If
  
  'get coordinate number
  EditCoordNum = lstCoords.ListIndex
  
  'draw picture
  DrawPicture
  
  'update tool status AFTER drawing
  CurrentPen = PicEdit.CurrentToolStatus
  
  'set selected tools to match current
  SelectedPen = CurrentPen
  
  'if on a line command coordinate
  If CurCmdIsLine Then
    'draw temp line based on current node
    DrawTempLine False, 0, 0
  End If
  
  'highlight the coords
  '(always, since coordinates are only enabled in edit mode)
  HighlightCoords
  
  'update toolbar
  UpdateToolBar
  
  'reset edit menu
  SetEditMenu
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub lstCoords_DblClick()

  'if dblclicking a non-relative coordinate, display
  'coordinate edit form so user can set values
  
  Dim bytNewPattern As Byte
  Dim bytOldCoord() As Byte
  Dim lngPos As Long, lngCoord As Long, rtn As Long
  Dim NextUndo As PictureUndo
  Dim strText As String
  Dim ptEdit As PT, ptNew As PT
  Dim PlotPt As LCoord, PlotSz As LCoord, StartPt As LCoord
  Dim CmdType As DrawFunction, Splat As Boolean
  
  On Error GoTo ErrHandler
  
  If lstCoords.ListIndex = -1 Then
    'just in case a coordinate is not actually selected,
    Exit Sub
  End If
  
  'editable coords include Plot, Abs Line, X Corner, Y Corner, Fill
  Select Case lstCommands.List(lstCommands.ListIndex)
  Case "Plot"
    CmdType = dfPlotPen
    Splat = (CurrentPen.PlotStyle = psSplatter)
  Case "Abs Line"
    CmdType = dfAbsLine
  Case "X Corner"
    CmdType = dfXCorner
  Case "Y Corner"
    CmdType = dfYCorner
  Case "Fill"
    CmdType = dfFill
  Case "Rel Line"
    'can't edit rel lines, because it would be too hard to enforce
    'distance limits
    MsgBoxEx "Relative Line coordinates cannot be manually edited because of" & vbNewLine & "the need to enforce distance limits.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Edit Coordinates", WinAGIHelp, "htm\agi\pictures.htm#f7"
    Exit Sub
    
  Case Else
    'just exit
    Exit Sub
  End Select
  
  'show the plot edit form
  Load frmPlotEdit

  ptEdit = ExtractCoordinates(lstCoords.Text)
  'save the coordinate index for later undo
  lngCoord = lstCoords.ListIndex
  'configure coordinate edit form based on type, and pass coordinates
  frmPlotEdit.FormSetup CmdType, ptEdit.X, ptEdit.Y, Splat
  
  'default is 2 byte array for undo
  ReDim bytOldCoord(1)
  bytOldCoord(0) = CByte(ptEdit.X)
  bytOldCoord(1) = CByte(ptEdit.Y)
  
  'if editing a plot point, pass pen info
  If lstCommands.List(lstCommands.ListIndex) = "Plot" Then
    'pass current pen shape and size
    frmPlotEdit.PenShape = CurrentPen.PlotShape
    frmPlotEdit.PenSize = CurrentPen.PlotSize
  End If
  
  'if editing a splatter, also pass pattern, color and background
  ' only splatter shows the coordinates in the coordinate editor
  If Splat Then
    'need extra byte for the pattern
    ReDim bytOldCoord(2) 'declare as three element array to facilitate undo action
    bytOldCoord(1) = CByte(ptEdit.X)
    bytOldCoord(2) = CByte(ptEdit.Y)
  
    'save old pattern and send it to the form
    bytOldCoord(0) = CByte(Val(lstCoords.Text))
    frmPlotEdit.OldPattern = bytOldCoord(0)
  
    'set up plot point and nominal size of background to copy
    PlotPt.X = ptEdit.X
    PlotPt.Y = ptEdit.Y
    PlotSz.X = 14
    PlotSz.Y = 27
    
    'need to adjust plot point to account for edge cases
    If PlotPt.X < (CurrentPen.PlotSize + 1) \ 2 Then
      PlotPt.X = (CurrentPen.PlotSize + 1) \ 2
    End If
    'there is a bug in AGI that uses 160 as edge limit for
    'plotting, so we use same so pictures draw the same
'''    If PlotPt.X > 159 - CurrentPen.PlotSize \ 2 Then
'''      PlotPt.X = 159 - CurrentPen.PlotSize \ 2
'''    End If
    If PlotPt.X > 160 - CurrentPen.PlotSize \ 2 Then
      PlotPt.X = 160 - CurrentPen.PlotSize \ 2
      'need to let form know value was adjusted
      frmPlotEdit.AdjX = PlotPt.X
    End If
    If PlotPt.Y < CurrentPen.PlotSize Then
      PlotPt.Y = CurrentPen.PlotSize
    End If
    If PlotPt.Y > 167 - CurrentPen.PlotSize Then
      PlotPt.Y = 167 - CurrentPen.PlotSize
    End If
      
    'now convert plot point into desired upper-right corner
    'coordinate to pass to the editing window
    PlotPt.X = PlotPt.X - 7
    PlotPt.Y = PlotPt.Y - 13
    StartPt.X = 0
    StartPt.Y = 0
    If PlotPt.X < 0 Then
      PlotSz.X = PlotSz.X + PlotPt.X
      StartPt.X = -PlotPt.X
      PlotPt.X = 0
    End If
    If PlotPt.Y < 0 Then
      PlotSz.Y = PlotSz.Y + PlotPt.Y
      StartPt.Y = -PlotPt.Y
      PlotPt.Y = 0
    End If

    'adjust picture so it shows drawing up to this plot point
    PicEdit.DrawPos = PicEdit.DrawPos - 3
    'if only priority window is active, then use priority picture
    If CurrentPen.VisColor = 16 And CurrentPen.PriColor < 16 Then
      'copy from priority picture
      rtn = BitBlt(frmPlotEdit.picBackground.hDC, StartPt.X, StartPt.Y, PlotSz.X, PlotSz.Y, PicEdit.PriorityBMP, PlotPt.X, PlotPt.Y, SRCCOPY)
      frmPlotEdit.PenColor = EGAColor(CurrentPen.PriColor)
      frmPlotEdit.NoPen = False
    Else
      'otherwise, use visual picture
      rtn = BitBlt(frmPlotEdit.picBackground.hDC, StartPt.X, StartPt.Y, PlotSz.X, PlotSz.Y, PicEdit.VisualBMP, PlotPt.X, PlotPt.Y, SRCCOPY)
      'make sure vispen is active
      If CurrentPen.VisColor < 16 Then
        frmPlotEdit.NoPen = False
        frmPlotEdit.PenColor = EGAColor(CurrentPen.VisColor)
      Else
        'no pen active; nothing to display on coordinate editor
        frmPlotEdit.NoPen = True
      End If
    End If
  End If
  
  'show the form as modal so user can't do anything else until done editing
  frmPlotEdit.Show vbModal, frmMDIMain
  
  'if splatter was being edited, need to reset picture back to original drawpos
  If Splat Then PicEdit.DrawPos = PicEdit.DrawPos + 3
  
  'if user canceled the edit, then unload the form and exit
  If frmPlotEdit.Canceled Then
    Unload frmPlotEdit
    Exit Sub
  End If
  
  'get the new cursor point
  ptNew.X = CLng(frmPlotEdit.txtX.Text)
  ptNew.Y = CLng(frmPlotEdit.txtY.Text)
  
  'retrieve the new plot pattern Value
  bytNewPattern = frmPlotEdit.NewPattern
  'and unload the form
  Unload frmPlotEdit
  
  'if nothing changed, exit
  If Splat Then
    If ptNew.X = ptEdit.X And ptNew.Y = ptEdit.Y And bytNewPattern = bytOldCoord(0) Then
      Exit Sub
    End If
  Else
    If ptNew.X = ptEdit.X And ptNew.Y = ptEdit.Y Then
      Exit Sub
    End If
  End If
  
  'now update the coord (and pattern, if a splatter)
  If CmdType = dfPlotPen Then
    If Splat Then
      'find position in the pic resource data for this coordinate
      lngPos = lstCommands.ItemData(lstCommands.ListIndex) + 1 + 3 * lstCoords.ListIndex
      
      'change the plot pattern data
      PicEdit.Resource.Data(lngPos) = bytNewPattern * 2
      'update the coords
      PicEdit.Resource.Data(lngPos + 1) = ptNew.X
      PicEdit.Resource.Data(lngPos + 2) = ptNew.Y
      
    Else
      'find position in the pic resource data for this coordinate
      lngPos = lstCommands.ItemData(lstCommands.ListIndex) + 1 + 2 * lstCoords.ListIndex
      '*'Debug.Assert lngPos = lstCoords.ItemData(lngCoord)
      'update the coords
      PicEdit.Resource.Data(lngPos) = ptNew.X
      PicEdit.Resource.Data(lngPos + 1) = ptNew.Y
    End If
      
    'if not skipping undo
    If Settings.PicUndo <> 0 Then
      'create new undo object
      Set NextUndo = New PictureUndo
      With NextUndo
        .UDAction = udpEditPlotCoord
        .UDPicPos = lngPos
        .UDCmdIndex = lstCommands.ListIndex
        .UDCoordIndex = lngCoord
        .UDData = bytOldCoord 'size tells us if there is a splat code or not
      End With
      'add to undo
      AddUndo NextUndo
    End If
    
    'reset the text for the coordinate list
    If Splat Then
      lstCoords.List(lstCoords.ListIndex) = CStr(bytNewPattern) & " -- " & CoordText(ptNew.X, ptNew.Y)
    Else
      lstCoords.List(lstCoords.ListIndex) = CoordText(ptNew.X, ptNew.Y)
    End If
    
  Else
    lngPos = lstCoords.ItemData(lngCoord)
    'use the regular 'endeditcoord function!
    EndEditCoord CmdType, lngCoord, ptNew, lngPos, lstCoords.Text
    'force update
    BuildCoordList lstCommands.ListIndex
    lstCoords.ListIndex = lngCoord
  
  End If
  
  'update the coord list so hilites work
  CoordPT(lngCoord) = ptNew
  
  'use coord list click to refresh
  lstCoords_Click
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub lstCoords_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub


Private Sub lstCoords_KeyDown(KeyCode As Integer, Shift As Integer)

  'ignore all keycodes
  KeyCode = 0
End Sub

Private Sub lstCoords_KeyPress(KeyAscii As Integer)
 
  'need to ignore key presses to stop the auto-selection feature of the list box
  KeyAscii = 0
End Sub

Private Sub lstCoords_KeyUp(KeyCode As Integer, Shift As Integer)

  'ignore keycode
  KeyCode = 0
  Shift = 0
End Sub

Private Sub lstCoords_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim lngClickRow As Long
  
  On Error GoTo ErrHandler
  
  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
  
  'if nothing (a cmd with no coords is currently selected)
  If lstCoords.ListCount = 0 Then
    Exit Sub
  End If
  
  'if right button
  If Button = vbRightButton Then
  
    lngClickRow = (Y / ScreenTWIPSY) \ SendMessage(lstCoords.hWnd, LB_GETITEMHEIGHT, 0, 0) + lstCoords.TopIndex
    If lngClickRow > lstCoords.ListCount - 1 Then
      lngClickRow = lstCoords.ListCount - 1
    End If
    
    'if on a different coord
    If lngClickRow <> lstCoords.ListIndex Then
      'select it
      lstCoords.ListIndex = lngClickRow
    End If
    'reset edit menu first
    SetEditMenu
    'make sure this form is the active form
    If Not (frmMDIMain.ActiveForm Is Me) Then
      'set focus before showing the menu
      Me.SetFocus
    End If
    'need doevents so form activation occurs BEFORE popup
    'otherwise, errors will be generated because of menu
    'adjustments that are made in the form_activate event
    SafeDoEvents
    'show edit menu
    PopupMenu frmMDIMain.mnuEdit, , lstCoords.Left + X / ScreenTWIPSX, lstCoords.Top + Y / ScreenTWIPSY
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub picPalette_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  'set cursor depending on mode
  If PicMode = pmEdit Then
    picPalette.MousePointer = vbDefault
  Else
    picPalette.MousePointer = vbNoDrop
  End If
End Sub

Private Sub picPriSurface_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim tmpX As Single, tmpY As Single
  
  'if not active form
  If Not frmMDIMain.ActiveForm Is Me Then
    Exit Sub
  End If
  
  'if dragging picture
  If blnDragging Then
    'get new scrollbar positions
    tmpX = sngOffsetX - X
    tmpY = sngOffsetY - Y
    
    'if vertical scrollbar is visible
    If vsbPri.Visible Then
      'limit positions to valid values
      If tmpY < vsbPri.Min Then
        tmpY = vsbPri.Min
      ElseIf tmpY > vsbPri.Max Then
        tmpY = vsbPri.Max
      End If
      'set vertical scrollbar
      vsbPri.Value = tmpY
    End If
    
    'if horizontal scrollbar is visible
    If hsbPri.Visible Then
      'limit positions to valid values
      If tmpX < hsbPri.Min Then
        tmpX = hsbPri.Min
      ElseIf tmpX > hsbPri.Max Then
        tmpX = hsbPri.Max
      End If
      'set horizontal scrollbar
      hsbPri.Value = tmpX
    End If
  End If
End Sub

Private Sub picPriSurface_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim rtn As Long
  
  'if dragging
  If blnDragging Then
    'cancel dragmode
    blnDragging = False
    'release mouse capture
    rtn = ReleaseCapture()
    SetCursors pcEdit
  End If
End Sub


Private Sub picSplitV_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub

Private Sub picSplitV_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  'begin split operation
  picSplitVIcon.Height = picSplitV.Height
  picSplitVIcon.Move picSplitV.Left, picSplitV.Top
  picSplitVIcon.Visible = True
  
  'save offset
  SplitOffset = picSplitV.Left - X
End Sub

Private Sub Form_LostFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub

Private Sub Form_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  On Error GoTo ErrHandler
  
  'if right button
  If Button = vbRightButton Then
    'reset edit menu first
    SetEditMenu
    'make sure this form is the active form
    If Not (frmMDIMain.ActiveForm Is Me) Then
      'set focus before showing the menu
      Me.SetFocus
    End If
    'need doevents so form activation occurs BEFORE popup
    'otherwise, errors will be generated because of menu
    'adjustments that are made in the form_activate event
    SafeDoEvents
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

  Dim Pos As Single
  
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
  
  'if the form is not visible or minimized, or incredibly small,
  If Not Visible Or WindowState = vbMinimized Or ScaleHeight < 50 Then
    Exit Sub
  End If
  
  'ugh, if window is or was maximized, the palette does
  'not automatically move to bottom like it should;
  'have to force it by hiding/redisplaying
  If WindowState <> PrevState Then
    picPalette.Visible = False
    picPalette.Visible = True
  End If
  PrevState = WindowState
  
  'if only showing one; we can dispense with calculations
  If OneWindow = 1 Then
    'only the vis surface
    Pos = picPalette.Top - picSplitH.Height
  ElseIf OneWindow = 2 Then
    'only the pri surface
    Pos = 0
  Else
    'MAINTAIN SPLIT RATIO
    'calculate new splitter location based on height for visual surface using current split ratio
    Pos = picVisSurface.Top + SplitRatio * (CalcHeight - picVisSurface.Top - picSplitH.Height - picPalette.Height)
    
    'we limit the calculations based on a minimum window size to avoid
    'problems with sizing/positioning various control elements;
    'this means that if user intentionally makes the window small
    'the space available for the priority drawing area will eventually
    'disappear; the Squished flag will help us remember to show
    'it again when the window is stretched back
    If Not Squished Then
      'did we squish the priority surface off the page?
      If Pos >= picPalette.Top - SPLIT_HEIGHT Then
        Squished = True
      End If
    Else
      'did we expand enough to unsquish?
      If Pos < picPalette.Top - SPLIT_HEIGHT Then
        Squished = False
        picPriSurface.Visible = True
      End If
    End If
    
    'and finally, we limit it so we keep a minimum amount of window open
    'for either pane
    If Pos > picPalette.Top - 100 Then
      'limit priority drawing surface to 100 pixels high
      Pos = picPalette.Top - 100
    End If
    If Pos < 100 Then
      'limit visual drawing surace to 100 pixels high
      Pos = 100
    End If
  End If
  
  ' update panels if this form is active
  UpdatePanels Pos, picSplitV.Left
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickCopy()

  Dim rtn As Long, bytData() As Byte
  Dim i As Long, blnTrackPen As Boolean
  
  On Error GoTo ErrHandler
  
  'if editing, tool is editselect, and a selection is visible
  'which means selwidth/height != 0)
  If (PicMode = pmEdit) And (SelectedTool = ttSelectArea) And (SelSize.X <> 0) And (SelSize.Y <> 0) Then
    'set copy picture height/width
    picCopy.Height = SelSize.Y
    picCopy.Width = SelSize.X
    picCopy.Cls
    'if user specified priority AND priority is visible
    If shpPri.Visible And blnInPri Then
      'copy from priority picture
      rtn = BitBlt(picCopy.hDC, 0, 0, SelSize.X, SelSize.Y, PicEdit.PriorityBMP, SelStart.X, SelStart.Y, SRCCOPY)
    Else
      'copy from visual picture
      rtn = BitBlt(picCopy.hDC, 0, 0, SelSize.X, SelSize.Y, PicEdit.VisualBMP, SelStart.X, SelStart.Y, SRCCOPY)
    End If
    'refresh the copied picture
    picCopy.Refresh
    'send to clipboard
    Clipboard.Clear
    Clipboard.SetData picCopy.Image, vbCFBitmap
    ViewCBMode = vmBitmap
    
    'clear PicClipBoardObj
    Set PicClipBoardObj = Nothing
    Exit Sub
  End If
  
  'if one or more commands enabled
  If lstCommands.SelCount >= 1 Then
    '*'Debug.Assert lstCoords.ListIndex = -1
    '*'Debug.Assert lstCommands.ListIndex <> -1
    '*'Debug.Assert lstCommands.ListIndex <> lstCommands.ListCount - 1
    
    'if 'End' marker is selected, need to skip it when copying
    If lstCommands.Selected(lstCommands.ListCount - 1) Then
      lstCommands.Selected(lstCommands.ListCount - 1) = False
    End If
  
    Set PicClipBoardObj = New PictureUndo
    'get starting pt of resource data
    rtn = lstCommands.ItemData(SelectedCmd - lstCommands.SelCount + 1)
    'allocate enough space for all cmd info
    ReDim bytData(lstCommands.ItemData(SelectedCmd + 1) - rtn - 1)
    For i = 0 To UBound(bytData())
      bytData(i) = PicEdit.Resource.Data(rtn + i)
    Next i
    
    'if any plot cmds, need to determine pen status at beginning of selection:
    
    'note current pen style at selected position
    rtn = CurrentPen.PlotStyle
    
    'step through selected cmds (starting at bottom, going up)
    For i = 0 To lstCommands.SelCount - 1
      If lstCommands.List(SelectedCmd - i) = "Plot" Then
        'need to track pen style
        blnTrackPen = True
        Exit For
      End If
    Next i
    
    'if tracking is necessary
    If blnTrackPen Then
      'assume no pen cmd (pen is solid)
      rtn = psSolid
      
      'back up from first selected cmd until a set pen is found (or beginning of cmd list)
      For i = SelectedCmd - lstCommands.SelCount To 0 Step -1
        'if this is a set pen cmd
        If Left$(lstCommands.List(SelectedCmd - 1), 7) = "Set Pen" Then
          'readjust pen status
          If InStr(1, lstCommands.List(SelectedCmd - 1), "Splatter") <> 0 Then
            rtn = psSplatter
          Else
            rtn = psSolid
          End If
          'exit; we now know what the pen status is for cmds in the copied cmd list
          Exit For
        End If
      Next i
    End If
    
    'for clipboard, only need to store the data, the number of cmds, and status of pen
    PicClipBoardObj.UDCoordIndex = lstCommands.SelCount
    PicClipBoardObj.UDData = bytData()
    If blnTrackPen Then
      'increment penstyle, and save it
      '(incrementing allows 0 to mean no pen status
      '1 to mean solid pen and 2 to mean splatter pen)
      rtn = rtn + 1
      PicClipBoardObj.UDCmdIndex = rtn
    End If
    'if only one cmd, also save cmd text
    If lstCommands.SelCount = 1 Then
      PicClipBoardObj.UDCmd = lstCommands.List(SelectedCmd)
    End If
    
    'update edit menu
    SetEditMenu
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickCut()

  'first copy
  MenuClickCopy
  
  'then delete
  MenuClickDelete
  
  If UndoCol.Count <> 0 Then
    'change last undo object so it reads as 'undo cut'
    UndoCol(UndoCol.Count).UDAction = udpCutCmds
    frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(PICUNDOTEXT + udpCutCmds)
    If UndoCol(UndoCol.Count).UDCoordIndex > 1 Then
      frmMDIMain.mnuEUndo.Caption = frmMDIMain.mnuEUndo.Caption & "s" & vbTab & "Ctrl+Z"
    Else
      frmMDIMain.mnuEUndo.Caption = frmMDIMain.mnuEUndo.Caption & vbTab & "Ctrl+Z"
    End If
  End If
End Sub
Public Sub MenuClickPaste()

  Dim NextUndo As PictureUndo
  Dim psOldStyle As EPlotStyle, psNewStyle As EPlotStyle
  Dim i As Long, InsertIndex As Long, InsertPos As Long
  
  'UDCoordIndex = # of cmds
  'UDData is dataset to add
  'UDCmd is text of cmd (only used if clipboard has a single cmd)
  'UDCmdIndex: 0 means no plot cmds
  '            1 means plots start with solid pen
  '            2 means plots start with splatter pen
  
  On Error GoTo ErrHandler
  
  '*'Debug.Assert lstCommands.ListIndex <> -1
  
  'always set cmd to select
  If SelectedTool <> ttEdit Then
    Toolbar1.Buttons("select").Value = tbrPressed
    Toolbar1_ButtonClick Toolbar1.Buttons("select")
  End If
  
  'if more than one command selected
  If lstCommands.SelCount > 1 Then
    'paste after the last selected item
    InsertIndex = lstCommands.ListIndex + 1
  Else
    'get current index position
    InsertIndex = SelectedCmd
  End If
  
  'if only one cmd
  If PicClipBoardObj.UDCoordIndex = 1 Then
    'insert command, ALWAYS in front
    InsertCommand PicClipBoardObj.UDData, SelectedCmd, PicClipBoardObj.UDCmd, True
  
    'change last undo
    If UndoCol.Count <> 0 Then
      UndoCol(UndoCol.Count).UDAction = udpPasteCmds
      UndoCol(UndoCol.Count).UDCoordIndex = PicClipBoardObj.UDCoordIndex
    End If
  Else
    'multiple commands
    
    'get current plot style
    psOldStyle = CurrentPen.PlotStyle
    
    'get current insert position
    InsertPos = lstCommands.ItemData(InsertIndex)
    
    'insert the data
    PicEdit.Resource.InsertData PicClipBoardObj.UDData, InsertPos
    
    'rebuild cmd list (but no update)
    LoadCmdList True
    
    'now, check for any plot cmds in the pasted section and in the section following the
    'pasted cmds that need to be adjusted due to changes in pen style; normally this is
    'done BEFORE the resource is modified otherwise the undo feature won't work correctly;
    'however, since the paste method has to actually insert the commands and rebuild the
    'cmd list before the check for plot adjustments is complete, the adjustments are done
    'AFTER the cmds are pasted; the undo method will need to compensate by adjusting
    'the insert index Value so plot adjustments get restored properly; see undo method
    'for additional comments
    
    'if the pasted stuff includes pen information
    If PicClipBoardObj.UDCmdIndex <> 0 Then
      'if plot styles DONT match (i.e. the pasted cmds expect a solid brush, but current brush is splatter
      '(or vice versa)) need to add/delete pattern info as appropriate
      '(remember that style info is actually cmdindex Value -1!)
      If psOldStyle <> PicClipBoardObj.UDCmdIndex - 1 Then
        'update patterns for the cmds being pasted to match old style
        '(dont add undo info, though; if the paste operation is
        'undone, the adjustments made in the pasted commands becomes moot
        'since they are being deleted)
        ReadjustPlotCoordinates InsertIndex, psOldStyle, True, InsertIndex + PicClipBoardObj.UDCoordIndex - 1
      End If
    End If
    
    'now need to verify that inserted cmds did not add a new set pen cmd that
    'could affect plot cmds that occur after the inserted cmds
    For i = PicClipBoardObj.UDCoordIndex - 1 To 0 Step -1
      If Left$(lstCommands.List(i + InsertIndex), 3) = "Set" Then
        'this is last 'set pen' cmd;
        'if it is different from the pen style BEFORE insertion of cmd
        '(check for the word 'splatter' in the cmd text to determine style)
        If InStr(7, lstCommands.List(i + InsertIndex), "Splat") = 0 Then
          psNewStyle = psSolid
        Else
          psNewStyle = psSplatter
        End If
        'if styles don't match
        If psNewStyle <> psOldStyle Then
          'need to adjust plot cmds that occur AFTER the inserted cmds to match style set in pasted cmds
          ReadjustPlotCoordinates InsertIndex + PicClipBoardObj.UDCoordIndex, psNewStyle
        End If
        
        'no need to check further, since we already found the set cmd that
        'could possibly affect plot cmds
        Exit For
      End If
    Next i
    
    If Settings.PicUndo <> 0 Then
      'add the undo object
      Set NextUndo = New PictureUndo
      With NextUndo
        .UDAction = udpPasteCmds
        .UDPicPos = InsertPos
        .UDCmdIndex = InsertIndex
        .UDCoordIndex = PicClipBoardObj.UDCoordIndex
      End With
      AddUndo NextUndo
    End If
  End If
  
  'need to make sure that the selection process
  'doesn't recurse and cause trouble
  NoSelect = True
  
  'unselect current cmds
  For i = 0 To lstCommands.ListCount - 1
    lstCommands.Selected(i) = False
  Next i
  'select the cmds that were pasted, and force update
  For i = InsertIndex To InsertIndex + PicClipBoardObj.UDCoordIndex - 1
    lstCommands.Selected(i) = True
  Next i
  
  NoSelect = False
  CodeClick = True
  lstCommands_Click
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickUndo()

  Dim NextUndo As PictureUndo, tmpUndo As PictureUndo
  Dim bytData() As Byte, lngCmdIndex As Long
  Dim OldPt As PT, tmpPT As PT
  Dim blnSetPen As Boolean, strPrefix As String
  Dim i As Long, j As Long, lngNewPos As Long
  
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
    frmMDIMain.mnuEUndo.Caption = "&Undo " & LoadResString(PICUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & UndoCol(UndoCol.Count).UDCmd & vbTab & "Ctrl+Z"
  Else
    frmMDIMain.mnuEUndo.Caption = "&Undo " & vbTab & "Ctrl+Z"
  End If
  
  'copy data to local array
  bytData = NextUndo.UDData
  'get index of command
  lngCmdIndex = NextUndo.UDCmdIndex
  
  'undo the action
  Select Case NextUndo.UDAction
  Case udpChangeColor
    'select the affected node
    SelectCmd lngCmdIndex
    'change color
    ChangeColor SelectedCmd, bytData(0), True
    
    'use click event to force update
    CodeClick = True
    lstCommands_Click
    
  Case udpChangePlotPen
    'select the affected cmd
    SelectCmd lngCmdIndex
    
    'restore command
    PicEdit.Resource.Data(NextUndo.UDPicPos + 1) = bytData(0)
    lstCommands.List(SelectedCmd) = NextUndo.UDText
    
    'check for any pattern changes
    If UndoCol.Count > 0 Then
      'get next undo item
      Set tmpUndo = UndoCol(UndoCol.Count)
      Do Until tmpUndo.UDAction <> udpAddPlotPattern And tmpUndo.UDAction <> udpDelPlotPattern
        If tmpUndo.UDAction = udpAddPlotPattern Then
          'remove this pattern data
          DelPatternData tmpUndo.UDCmdIndex, True
        Else
          'add this pattern data
          AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, True
        End If
        'remove undo object from stack
        UndoCol.Remove UndoCol.Count
        'if any more
        If UndoCol.Count > 0 Then
          'get next object
          Set tmpUndo = UndoCol(UndoCol.Count)
        Else
          'exit loop
          Exit Do
        End If
      Loop
    End If
    
    'force update
    SelectCmd lngCmdIndex, False
    
  Case udpDelCmd, udpCutCmds
    'if only one cmd
    If NextUndo.UDCoordIndex = 1 Then
      'select the affected cmd
      SelectCmd lngCmdIndex
      
      'reinsert command, ALWAYS in front
      InsertCommand NextUndo.UDData, lngCmdIndex, NextUndo.UDCmd, True, True
    
      'if command was set plot
      If Left$(NextUndo.UDCmd, 3) = "Set" Then
        'check for any pattern changes
        If UndoCol.Count > 0 Then
          'get next undo item
          Set tmpUndo = UndoCol(UndoCol.Count)
          Do Until tmpUndo.UDAction <> udpAddPlotPattern And tmpUndo.UDAction <> udpDelPlotPattern
            If tmpUndo.UDAction = udpAddPlotPattern Then
              'remove this pattern data
              DelPatternData tmpUndo.UDCmdIndex, True
            Else
              'add this pattern data
              AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, True
            End If
            'remove undo object from stack
            UndoCol.Remove UndoCol.Count
            'if any more
            If UndoCol.Count > 0 Then
              'get next object
              Set tmpUndo = UndoCol(UndoCol.Count)
            Else
              'exit loop
              Exit Do
            End If
          Loop
        End If
      End If
      
      'now select the added cmd
      SelectCmd lngCmdIndex
      
    Else
      'multiple commands
      
      'insert the data
      PicEdit.Resource.InsertData NextUndo.UDData, NextUndo.UDPicPos
      
      'rebuild it (but no update)
      LoadCmdList True
      
      'check for a 'set pen' cmd
      For i = 0 To NextUndo.UDCoordIndex - 1
        If Left$(lstCommands.List(NextUndo.UDCmdIndex - i), 3) = "Set" Then
          blnSetPen = True
          Exit For
        End If
      Next i

      If blnSetPen Then
        'check for any pattern changes
        If UndoCol.Count > 0 Then
          'get next undo item
          Set tmpUndo = UndoCol(UndoCol.Count)
          Do Until tmpUndo.UDAction <> udpAddPlotPattern And tmpUndo.UDAction <> udpDelPlotPattern
            If tmpUndo.UDAction = udpAddPlotPattern Then
              'remove this pattern data
              DelPatternData tmpUndo.UDCmdIndex, True
            Else
              'add this pattern data
              AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, True
            End If
            'remove undo object from stack
            UndoCol.Remove UndoCol.Count
            'if any more
            If UndoCol.Count > 0 Then
              'get next object
              Set tmpUndo = UndoCol(UndoCol.Count)
            Else
              'exit loop
              Exit Do
            End If
          Loop
        End If
      End If
    
      'disable painting
      SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
      
      'unselect 'end' place holder
      lstCommands.Selected(lstCommands.ListCount - 1) = False
      
      'select cmds in the cmd list
      For i = NextUndo.UDCoordIndex - 1 To 0 Step -1
        NoSelect = True
        lstCommands.Selected(lngCmdIndex - i) = True
      Next i
      NoSelect = False
      
      'reenable painting
      SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
    
      'select the bottom cmd and redraw
      SelectedCmd = NextUndo.UDCmdIndex
      DrawPicture
    
      'if more than one cmd was selected
      If NextUndo.UDCoordIndex > 1 Then
        'get bounds, and select the cmds
        '(sets selstart and selsize)
        GetSelectionBounds NextUndo.UDCmdIndex, NextUndo.UDCoordIndex, True
      End If
    End If
    
  Case udpPasteCmds
    'calculate amount of data to remove
    lngNewPos = lstCommands.ItemData(lngCmdIndex + NextUndo.UDCoordIndex) - lstCommands.ItemData(lngCmdIndex)
    
    'remove the data from the resource
    PicEdit.Resource.RemoveData NextUndo.UDPicPos, lngNewPos
    
    'remove the cmds from the cmd list
    For i = NextUndo.UDCoordIndex - 1 To 0 Step -1
      lstCommands.RemoveItem lngCmdIndex + i
    Next i
    
    'update position values for follow on cmds
    UpdatePosValues lngCmdIndex, -lngNewPos
    
    'check for any plot adjustments that need to be restored
    '(the index passed to addpattern/delpattern need to be
    'adjusted by the number of commands that were removed so
    'the correct plot cmd is restored
    If UndoCol.Count > 0 Then
      'get next undo item
      Set tmpUndo = UndoCol(UndoCol.Count)
      'continue adjusting patterns, until an undo item that is not a pattern adjust item is found
      Do Until tmpUndo.UDAction <> udpAddPlotPattern And tmpUndo.UDAction <> udpDelPlotPattern
        If tmpUndo.UDAction = udpAddPlotPattern Then
          'remove this pattern data
          DelPatternData tmpUndo.UDCmdIndex - NextUndo.UDCoordIndex, True
        Else
          'add this pattern data
          AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, True
        End If
        'remove undo object from stack
        UndoCol.Remove UndoCol.Count
        'if any more
        If UndoCol.Count > 0 Then
          'get next object
          Set tmpUndo = UndoCol(UndoCol.Count)
        Else
          'exit loop
          Exit Do
        End If
      Loop
    End If
    
    'select the cmd to refresh everything
    SelectCmd lngCmdIndex, False
    
    
  Case udpDelCoord
    'select the cmd/coord
    SelectCmd lngCmdIndex, False
    
    'reinsert the coordinate
    tmpPT = ExtractCoordinates(NextUndo.UDText)
    If InStr(NextUndo.UDText, "-") > 0 Then
     strPrefix = Left(NextUndo.UDText, InStr(NextUndo.UDText, "-") + 2)
    Else
      strPrefix = ""
    End If
    AddCoordToPic bytData, NextUndo.UDCoordIndex, tmpPT.X, tmpPT.Y, strPrefix, True
    
    'select the coord to force update
    lstCoords_Click
    
  Case udpAddCmd, udpRectangle, udpTrapezoid
    'delete the command
    DeleteCommand lngCmdIndex, True
    
    'if command was set plot
    If Left$(NextUndo.UDCmd, 3) = "Set" Then
      'check for any pattern changes
      If UndoCol.Count > 0 Then
        'get next undo item
        Set tmpUndo = UndoCol(UndoCol.Count)
        Do Until tmpUndo.UDAction <> udpAddPlotPattern And tmpUndo.UDAction <> udpDelPlotPattern
          If tmpUndo.UDAction = udpAddPlotPattern Then
            'remove this pattern data
            DelPatternData tmpUndo.UDCmdIndex, True
          Else
            'add this pattern data
            AddPatternData tmpUndo.UDCmdIndex, tmpUndo.UDData, True
          End If
          'remove undo object from stack
          UndoCol.Remove UndoCol.Count
          'if any more
          If UndoCol.Count > 0 Then
            'get next object
            Set tmpUndo = UndoCol(UndoCol.Count)
          Else
            'exit loop
            Exit Do
          End If
        Loop
      End If
    End If
        
    'use click event to force update
    CodeClick = True
    lstCommands_Click
    
  Case udpEllipse
    'delete this command, and next three commands
    For i = 1 To 4
      DeleteCommand lngCmdIndex, True
    Next i
    
    'force update
    SelectCmd lngCmdIndex, False
    
  Case udpAddCoord
    'select cmd first
    SelectCmd lngCmdIndex
    'build coord list
    BuildCoordList lngCmdIndex
    
    'delete the coordinate
    DeleteCoordinate NextUndo.UDCoordIndex, True
    
    'force update
    SelectCmd lngCmdIndex, False
    
  Case udpEditCoord
    'select command first
    SelectCmd lngCmdIndex
    
    'now edit coord
    tmpPT.X = bytData(0)
    tmpPT.Y = bytData(1)
    EndEditCoord CLng(NextUndo.UDText), NextUndo.UDCoordIndex, tmpPT, NextUndo.UDPicPos, vbNullString, True
    
    'force update
    BuildCoordList lngCmdIndex
    lstCoords.ListIndex = NextUndo.UDCoordIndex
    
  Case udpSplitCmd
    'select the cmd
    SelectCmd lngCmdIndex
    
    'now rejoin the commands
    JoinCommands SelectedCmd, True
    
  Case udpJoinCmds
    'select the cmd
    SelectCmd lngCmdIndex, False
    
    For i = 0 To lstCoords.ListCount - 1
      If lstCoords.ItemData(i) = NextUndo.UDPicPos Then
        Exit For
      End If
    Next i
    
    'now split the commands
    SplitCommand i, True
  
  Case udpMoveCmds
    'select the cmd
    SelectCmd lngCmdIndex
    
    'extract delta values
    i = Val(NextUndo.UDText)
    j = Val(Right$(NextUndo.UDText, Len(NextUndo.UDText) - InStr(1, NextUndo.UDText, "|")))
    
    'move cmds back
    MoveCmds lngCmdIndex, NextUndo.UDCoordIndex, i, j, True
    
    'disable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
    
    'select cmds in the cmd list
    For i = NextUndo.UDCoordIndex - 1 To 0 Step -1
      NoSelect = True
      lstCommands.Selected(lngCmdIndex - i) = True
    Next i
    NoSelect = False
    
    'if only one command selected, reload the coord list
    If NextUndo.UDCoordIndex = 1 Then
      'build coord list
      BuildCoordList SelectedCmd
      'and make sure selection bounds are hidden
      tmrSelect.Enabled = False
      shpVis.Visible = False
      shpPri.Visible = False
    End If
    
    'reenable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
    
    'if more than one cmd
    If NextUndo.UDCoordIndex > 1 Then
      'get bounds, and select the cmds
      GetSelectionBounds NextUndo.UDCmdIndex, NextUndo.UDCoordIndex, True
    End If
    
'     do we need all these activities? they are what is done when
'     something is selected, which is basically the same thing here
'     This will need some studying and testing to make sure it's
'     what I need done here

    'always cancel any drawing operation
    PicDrawMode = doNone
    
    'set CurCmdIsLine to false until proven otherwise
    CurCmdIsLine = False
    
    'always set cursor highlighting to match selection status
    tmrSelect.Enabled = (SelSize.X > 0 And SelSize.Y > 0)
    
    'get current tool status
    CurrentPen = PicEdit.CurrentToolStatus
    
    'set selected tools to match current
    SelectedPen = CurrentPen
    
    'update toolbar
    UpdateToolBar
    
    'force redraw
    DrawPicture
    
    'reset edit menu
    SetEditMenu
  
  Case udpFlipH, udpFlipV
    'if current cmd is not this cmd
    If lstCommands.ListIndex <> lngCmdIndex Then
      'select the cmd
      SelectCmd lngCmdIndex
    End If
    
    're-flip
    FlipCmds lngCmdIndex, NextUndo.UDCoordIndex, IIf(NextUndo.UDAction = udpFlipH, 0, 1), True
    
    'disable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 0, 0
    
    'select cmds in the cmd list
    For i = NextUndo.UDCoordIndex - 1 To 0 Step -1
      NoSelect = True
      lstCommands.Selected(lngCmdIndex - i) = True
    Next i
    NoSelect = False
    
    'reenable painting
    SendMessage lstCommands.hWnd, WM_SETREDRAW, 1, 0
    
    'if more than one cmd
    If NextUndo.UDCoordIndex > 1 Then
      'get bounds, and select the cmds
      GetSelectionBounds NextUndo.UDCmdIndex, NextUndo.UDCoordIndex, True
      
    'if only one cmd,
    Else
      'rebuld coord list
      BuildCoordList SelectedCmd
      
      'select 'nothing'
      SelStart.X = 0
      SelStart.Y = 0
      SelSize.X = 0
      SelSize.Y = 0
      ShowCmdSelection
    End If
    
    'force redraw
    DrawPicture
    
  Case udpEditPlotCoord
    lngNewPos = NextUndo.UDPicPos
    bytData = NextUndo.UDData
    lngCmdIndex = NextUndo.UDCmdIndex
    
    'was a splat edited? different actions if so
    If UBound(bytData) = 2 Then
      'change pattern back
      PicEdit.Resource.Data(lngNewPos) = bytData(0) * 2
      'change the coord values
      PicEdit.Resource.Data(lngNewPos + 1) = bytData(1)
      PicEdit.Resource.Data(lngNewPos + 2) = bytData(2)
    Else
      'change the coord values
      PicEdit.Resource.Data(lngNewPos) = bytData(0)
      PicEdit.Resource.Data(lngNewPos + 1) = bytData(1)
    End If
    
    'disable updating of redraw until done
    SendMessage picVisual.hWnd, WM_SETREDRAW, 0, 0
    SendMessage picPriority.hWnd, WM_SETREDRAW, 0, 0

    'if current cmd is not this cmd
    If lstCommands.ListIndex <> lngCmdIndex Then
      'select the cmd
      SelectCmd lngCmdIndex
    End If
    CodeClick = True
    lstCommands_Click
    lstCoords.ListIndex = NextUndo.UDCoordIndex
  
    'refresh pictures
    SendMessage picVisual.hWnd, WM_SETREDRAW, 1, 0
    SendMessage picPriority.hWnd, WM_SETREDRAW, 1, 0
    picVisual.Refresh
    picPriority.Refresh
    
  Case udpSetPriBase
    'change pribase back
    PicEdit.PriBase = lngCmdIndex
    'if showing priority lines
    If ShowBands Then
      DrawPicture True
    End If
  End Select
  
  Set NextUndo = Nothing
  
  'update menu
  SetEditMenu
  
  'undo should always set dirty flag
  MarkAsDirty
Exit Sub

ErrHandler:
  'it is possible to get changeID error (if user gives another resource
  'the same name as the 'undo' name, and then attempts to undo this ID change
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub SetEditMenu()
  'sets the menu captions on the Edit menu
  'based on current selection
  Dim tmpCmd As DrawFunction
  
  On Error GoTo ErrHandler
  
  With frmMDIMain
    'always show undo, cut, copy, paste, select all, bar1, find, findagain, replace, both customs
    .mnuEUndo.Visible = True
    .mnuEBar0.Visible = True
    .mnuECut.Visible = True
    .mnuECopy.Visible = True
    .mnuEPaste.Visible = True
    .mnuESelectAll.Visible = True
    .mnuEBar1.Visible = True
    .mnuEBar2.Visible = True
    .mnuEReplace.Visible = True
    .mnuEFind.Visible = True
    .mnuEFindAgain.Visible = Not InGame Or Val(InterpreterVersion) >= 2.936
    .mnuECustom1.Visible = True
    .mnuECustom2.Visible = True
    .mnuECustom3.Visible = False
    
    'redo used for swapping drawing surfaces
    If picVisSurface.Visible Xor picPriSurface.Visible Then
      .mnuERedo.Visible = True
      .mnuERedo.Enabled = True
      If picVisSurface.Visible Then
        .mnuERedo.Caption = "Show Priority Screen" & vbTab & "Alt+S"
      Else
        .mnuERedo.Caption = "Show Visual Screen" & vbTab & "Alt+S"
      End If
    Else
      .mnuERedo.Visible = False
    End If
    
    'find used for showing/hiding priority bands
    .mnuEFind.Enabled = True
    If ShowBands Then
      .mnuEFind.Caption = "Hide Priority Bands" & vbTab & "Alt+P"
    Else
      .mnuEFind.Caption = "Show Priority Bands" & vbTab & "Alt+P"
    End If
    
    'find again used for setting priority base
    If .mnuEFindAgain.Visible Then
      .mnuEFindAgain.Enabled = True
      .mnuEFindAgain.Caption = "Adjust Priority Base" & vbTab & "Ctrl+Alt+P"
    End If
    
    
    'toggle bkgd visible only if a bkgd Image is loaded
    .mnuRCustom2.Visible = Not (BkgdImage Is Nothing)
    If .mnuRCustom2.Visible Then
      .mnuRCustom2.Enabled = True
      If PicEdit.BkgdShow And .mnuRCustom2.Visible Then
        .mnuRCustom2.Caption = "Hide Background" & vbTab & "Alt+B"
      Else
        .mnuRCustom2.Caption = "Show Background" & vbTab & "Alt+B"
      End If
    End If
    ' allow removal if an image is loaded
    .mnuRCustom3.Visible = .mnuRCustom2.Visible
    If .mnuRCustom3.Visible Then
      .mnuRCustom3.Enabled = True
      .mnuRCustom3.Caption = "Remove Background Image" & vbTab & "Shift+Alt+B"
    End If

    'replace used to toggle test mode
    .mnuEReplace.Enabled = True
    'Caption depends on current mode
    .mnuEReplace.Visible = True

    
    Select Case PicMode
    Case pmTest
      'show but disable undo, cut, copy, paste, select all
      .mnuEUndo.Enabled = False
      .mnuEUndo.Caption = "&Undo" & vbTab & "Ctrl+Z"
      .mnuECut.Enabled = False
      .mnuECut.Caption = "Cu&t" & vbTab & "Ctrl+X"
      .mnuECopy.Enabled = False
      .mnuECopy.Caption = "&Copy" & vbTab & "Ctrl+C"
      .mnuEPaste.Enabled = False
      .mnuEPaste.Caption = "&Paste" & vbTab & "Ctrl+V"
      .mnuESelectAll.Enabled = False
      .mnuESelectAll.Caption = "Select &All" & vbTab & "Ctrl+A"
      
      'delete, insert, clear, bkgd items not used in test mode
      .mnuEDelete.Visible = False
      .mnuEInsert.Visible = False
      .mnuEClear.Visible = False
      .mnuRCustom2.Enabled = False
      .mnuRCustom3.Enabled = False
      
      .mnuEReplace.Caption = "Disable Test &Mode" & vbTab & "Alt+M"
      
      'show custom menus
      .mnuECustom1.Enabled = True
      .mnuECustom1.Caption = "Test View..." & vbTab & "Alt+V"
      
      .mnuECustom2.Enabled = True
      .mnuECustom2.Caption = "Test Options..." & vbTab & "Alt+O"
      
    Case pmEdit
      'delete, insert, clear visible
      .mnuEDelete.Visible = True
      .mnuEInsert.Visible = True
      .mnuEClear.Visible = True
      'custom3 enabled
      .mnuRCustom3.Enabled = True
      'enable test mode switch
      .mnuEReplace.Caption = "Enable Test &Mode" & vbTab & "Alt+M"
      
      'if there is something to undo
      If UndoCol.Count <> 0 Then
        .mnuEUndo.Enabled = True
        .mnuEUndo.Caption = "&Undo " & LoadResString(PICUNDOTEXT + UndoCol(UndoCol.Count).UDAction) & UndoCol(UndoCol.Count).UDCmd
        'some commands need 's' added to end if more than one command to undo
        Select Case UndoCol(UndoCol.Count).UDAction
        Case udpDelCmd, udpAddCmd, udpCutCmds, udpPasteCmds, udpMoveCmds, udpFlipH, udpFlipV
          If UndoCol(UndoCol.Count).UDCoordIndex > 1 Then
            .mnuEUndo.Caption = .mnuEUndo.Caption & "s" & vbTab & "Ctrl+Z"
          Else
            .mnuEUndo.Caption = .mnuEUndo.Caption & vbTab & "Ctrl+Z"
          End If
        Case Else
          .mnuEUndo.Caption = .mnuEUndo.Caption & vbTab & "Ctrl+Z"
        End Select
      Else
        .mnuEUndo.Enabled = False
        .mnuEUndo.Caption = "&Undo" & vbTab & "Ctrl+Z"
      End If
      
      'if tool is editselect
      If (SelectedTool = ttSelectArea) Then
        'always disable cut, delete, insert, paste
        .mnuEDelete.Enabled = False
        .mnuEDelete.Caption = "Delete" & vbTab & "Del"
        .mnuEInsert.Enabled = False
        .mnuEInsert.Caption = "&Insert Coordinate" & vbTab & "Shift+Ins"
        .mnuECut.Enabled = False
        .mnuECut.Caption = "Cut" & vbTab & "Ctrl+X"
        .mnuEPaste.Enabled = False
        .mnuEPaste.Caption = "Paste" & vbTab & "Ctrl+V"
        'copy is enabled if something selected
        .mnuECopy.Enabled = ((SelSize.X > 0) And (SelSize.Y > 0))
        .mnuECopy.Caption = "Copy Selection" & vbTab & "Ctrl+C"
          
      'if NO coordinate is selected
      ElseIf lstCoords.ListIndex = -1 Then
        'cut, copy, delete enabled if at least one cmd selected
        .mnuECopy.Enabled = (lstCommands.ListIndex <> -1) And (SelectedCmd <> lstCommands.ListCount - 1)
        .mnuECopy.Caption = "Copy Command"
        .mnuECut.Enabled = .mnuECopy.Enabled
        .mnuECut.Caption = "Cut Command"
        .mnuEDelete.Enabled = .mnuECopy.Enabled
        .mnuEDelete.Caption = "Delete Command"
        If lstCommands.SelCount > 1 Then
          .mnuECopy.Caption = .mnuECopy.Caption & "s"
          .mnuECut.Caption = .mnuECut.Caption & "s"
          .mnuEDelete.Caption = .mnuEDelete.Caption & "s"
        End If
        .mnuECopy.Caption = .mnuECopy.Caption & vbTab & "Ctrl+C"
        .mnuECut.Caption = .mnuECut.Caption & vbTab & "Ctrl+X"
        .mnuEDelete.Caption = .mnuEDelete.Caption & vbTab & "Del"
        
        'paste enabled only if clipboard has pic cmds on it
        .mnuEPaste.Enabled = (Not PicClipBoardObj Is Nothing)
        .mnuEPaste.Caption = "Paste"
        If .mnuEPaste.Enabled Then
          If PicClipBoardObj.UDCoordIndex > 1 Then
            .mnuEPaste.Caption = .mnuEPaste.Caption & " Commands" & vbTab & "Ctrl+V"
          Else
            .mnuEPaste.Caption = .mnuEPaste.Caption & " Command" & vbTab & "Ctrl+V"
          End If
        Else
          .mnuEPaste.Caption = .mnuEPaste.Caption & vbTab & "Ctrl+V"
        End If
        'insert not enabled if no coord selected
        .mnuEInsert.Enabled = False
        .mnuEInsert.Caption = "&Insert Coordinate" & vbTab & "Shift+Ins"
        
      Else
        'no cut or copy for coords
        .mnuECopy.Enabled = False
        .mnuECopy.Caption = "Copy" & vbTab & "Ctrl+C"
        .mnuECut.Enabled = False
        .mnuECut.Caption = "Cut" & vbTab & "Ctrl+X"
        
        'insert always available for absline, relline, fill, plot
        'delete always available for absline, fill, plot
        'insert/delete only available for other commands if on last coord
        .mnuEDelete.Caption = "Delete "
        .mnuEInsert.Caption = "&Insert Coordinate" & vbTab & "Shift+Ins"
        
        Select Case lstCommands.List(SelectedCmd)
        Case "Abs Line", "Fill", "Plot"
          'enable delete
          .mnuEDelete.Enabled = True
          .mnuEDelete.Caption = .mnuEDelete.Caption & " Coordinate"
          'enable insert
          .mnuEInsert.Enabled = True
          
        Case Else
          'delete
          'if on last coord of any other command,
          If lstCoords.ListIndex = lstCoords.ListCount - 1 Then
            'enable delete
            .mnuEDelete.Enabled = True
            .mnuEDelete.Caption = .mnuEDelete.Caption & " Coordinate"
          Else
            'disable delete
            .mnuEDelete.Enabled = False
            .mnuEDelete.Caption = .mnuEDelete.Caption & " Coordinate"
          End If
          
          'insert
          If lstCommands.List(SelectedCmd) = "RelLine" Then
            'enable
            .mnuEInsert.Enabled = True
          Else
            'if on last coord of any other command,
            .mnuEInsert.Enabled = (lstCoords.ListIndex = lstCoords.ListCount - 1)
          End If
        End Select
      End If
          
      'enable clear
      .mnuEClear.Enabled = True
      .mnuEClear.Caption = "Clear Picture" & vbTab & "Shift+Del"
      
      'enable select all if at least one cmd
      .mnuESelectAll.Enabled = lstCommands.ListCount > 1
      .mnuESelectAll.Caption = "Select &All" & vbTab & "Ctrl+A"
      
      'custom1 is split
      'disable if no cmd selected, OR no no coord selected, OR first coord is selected, OR last is selected OR more than one selected
      If lstCommands.ListIndex = -1 Or lstCoords.ListIndex = -1 Or lstCoords.ListIndex = 0 Or lstCoords.ListIndex = lstCoords.ListCount - 1 Or lstCommands.SelCount > 1 Then
        .mnuECustom1.Enabled = False
      Else
        'if on a line, fill, or plot cmd
        Select Case PicEdit.Resource.Data(lstCommands.ItemData(SelectedCmd))
        Case dfAbsLine, dfRelLine, dfXCorner, dfYCorner, dfFill, dfPlotPen
          'enable splitting
          .mnuECustom1.Enabled = True
        Case Else
          .mnuECustom1.Enabled = False
        End Select
      End If
      .mnuECustom1.Caption = "&Split Command" & vbTab & "Ctrl+T"
      
      'custom2 = join
      'only enable joining if on a fill or plot cmd AND previous cmd matches
      'OR on a line cmd AND previous cmd matches AND last coordinate of prev cmd
      'matches first coord of this cmd
      
      'a valid cmd other than first or last, is selected AND only one cmd selected
      If SelectedCmd > 0 And SelectedCmd < lstCommands.ListCount - 1 And lstCommands.SelCount = 1 Then
        tmpCmd = PicEdit.Resource.Data(lstCommands.ItemData(SelectedCmd))
        'if on a command node
        Select Case tmpCmd
        Case dfAbsLine, dfRelLine, dfFill, dfPlotPen
          'if same as cmd above
          If lstCommands.List(SelectedCmd) = lstCommands.List(SelectedCmd - 1) Then
            'if cmd is paint or plot
            If tmpCmd = dfPlotPen Or tmpCmd = dfFill Then
              .mnuECustom2.Enabled = True
            Else
              'if points match
              If MatchPoints(SelectedCmd) Then
                .mnuECustom2.Enabled = True
              Else
                .mnuECustom2.Enabled = False
              End If
            End If
          Else
            .mnuECustom2.Enabled = False
          End If
        Case dfXCorner, dfYCorner
          'if cmd above is a corner
          If (Asc(lstCommands.List(SelectedCmd - 1)) = 89) Or (Asc(lstCommands.List(SelectedCmd - 1)) = 88) Then
            'if points match
            If MatchPoints(SelectedCmd) Then
              .mnuECustom2.Enabled = True
            Else
              .mnuECustom2.Enabled = False
            End If
          Else
            .mnuECustom2.Enabled = False
          End If
  
        Case Else
          .mnuECustom2.Enabled = False
        End Select
      Else
        .mnuECustom2.Enabled = False
      End If
      .mnuECustom2.Caption = "&Join Commands" & vbTab & "Ctrl+J"
    End Select
  End With
  
  'set toolbar buttons to match menu items
  With Toolbar1
    .Buttons("undo").Enabled = frmMDIMain.mnuEUndo.Enabled
    .Buttons("cut").Enabled = frmMDIMain.mnuECut.Enabled
    .Buttons("copy").Enabled = frmMDIMain.mnuECopy.Enabled
    .Buttons("paste").Enabled = frmMDIMain.mnuEPaste.Enabled
    .Buttons("delete").Enabled = frmMDIMain.mnuEDelete.Enabled
    'if only one cmd selected
    If lstCommands.SelCount = 1 And SelectedTool <> ttSelectArea Then
      'cant flip end cmd
      If SelectedCmd <> lstCommands.ListCount - 1 Then
        Select Case Left$(lstCommands.List(SelectedCmd), 3)
        Case "Set", "Vis", "Pri"
          'these cmds don't have coordinates, so can't be flipped
          .Buttons(13).Enabled = False
          .Buttons(14).Enabled = False
        Case Else
          .Buttons(13).Enabled = True
          .Buttons(14).Enabled = True
        End Select
      Else
        .Buttons(13).Enabled = False
        .Buttons(14).Enabled = False
      End If
      
    'if more than one cmd selected
    ElseIf lstCommands.SelCount > 1 And SelectedTool <> ttSelectArea Then
      'if the selection shapes are visible, then cmds with coords
      'are in the selection
      .Buttons(13).Enabled = ((SelSize.X > 0) And (SelSize.Y > 0))
      .Buttons(14).Enabled = ((SelSize.X > 0) And (SelSize.Y > 0))
      
    'if no cmds selected
    Else
      .Buttons(13).Enabled = False
      .Buttons(14).Enabled = False
    End If
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub Form_Unload(Cancel As Integer)
  
  Dim i As Integer
  
  On Error GoTo ErrHandler
  
  'if unloading due to error on startup,
  'picedit will be set to nothing
  If Not PicEdit Is Nothing Then
    'dereference picture
    PicEdit.Unload
    Set PicEdit = Nothing
  End If
  
  'remove from PicEditor collection
  For i = 1 To PictureEditors.Count
    If PictureEditors(i) Is Me Then
      PictureEditors.Remove i
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
  
  'if a test view is currently loaded,
  If Not (TestView Is Nothing) Then
    'unload it and release it
    TestView.Unload
    Set TestView = Nothing
  End If
  
  'destroy background picture
  Set BkgdImage = Nothing
  'and masks
  Set MonoMask = Nothing
'  Set FullMask = Nothing
'  Set VisPic = Nothing
'  Set PriPic = Nothing
  
#If DEBUGMODE <> 1 Then
  'release subclass hook
  SetWindowLong lstCommands.hWnd, GWL_WNDPROC, PrevLBWndProc
  SetWindowLong Me.hWnd, GWL_WNDPROC, PrevPEWndProc
#End If
  'need to check if this is last form
  LastForm Me
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub hsbPri_Change()

  'set position of priority to match position of scroll bar
  picPriority.Left = IIf(picPriority.Left > PE_MARGIN, PE_MARGIN, -hsbPri.Value)
  
  'if back at starting position
  If picPriority.Left = PE_MARGIN Then
    'hide scroll bar, if no longer needed
    hsbPri.Visible = ((picPriSurface.Left + picPriority.Width + 2 * PE_MARGIN) > CalcWidth + vsbPri.Visible * vsbPri.Width)
    'ensure surface is sized to account for scrollbar
    picPriSurface.Height = picPalette.Top + (hsbPri.Visible * hsbPri.Height) - picPriSurface.Top
  End If
End Sub

Private Sub hsbPri_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
  
  'set focus to picture
  picPriority.SetFocus
End Sub

Private Sub hsbPri_Scroll()

  hsbPri_Change
'  picPriority.Left = IIf(picPriority.Left > PE_MARGIN, PE_MARGIN, -hsbPri.Value)
End Sub

Private Sub hsbVis_Change()

  'set position of visual to match position of scrollbar
  '*'Debug.Assert picVisual.Left <= PE_MARGIN
  picVisual.Left = IIf(picVisual.Left > PE_MARGIN, PE_MARGIN, -hsbVis.Value)
    
  'if back at starting position
  If picVisual.Left = PE_MARGIN Then
    'hide scroll bar, if no longer needed
    hsbVis.Visible = ((picVisSurface.Left + picVisual.Width + 2 * PE_MARGIN) > CalcWidth + vsbVis.Visible * vsbVis.Width)
    'ensure surface is sized to account for scrollbar
    picVisSurface.Height = picSplitH.Top + (hsbVis.Visible * hsbVis.Height) - picVisSurface.Top
  End If
End Sub

Private Sub hsbVis_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False

  'set focus to picture
  picVisual.SetFocus
End Sub

Private Sub hsbVis_Scroll()

  hsbVis_Change
'  picVisual.Left = IIf(picVisual.Left > PE_MARGIN, PE_MARGIN, -hsbVis.Value)
End Sub
Private Sub picPalette_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub
Private Sub picPalette_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim bytNewCol As Byte, blnOff As Boolean
  Dim dblWidth As Double
  Dim NextUndo As PictureUndo
  Dim bytData() As Byte
  Dim blnForce As Boolean
  
  '*'Debug.Assert X <= picPalette.Width
  '*'Debug.Assert Y <= picPalette.Height
  
  'if activating, ignore
  If Activating Then
    Activating = False
    Exit Sub
  End If
  
  dblWidth = picPalette.Width / 9
  
  'determine color from x,Y position
  bytNewCol = 9 * (Y \ 17) + (X \ dblWidth)
  'adjust to account for the disabled block
  
  If bytNewCol = 0 Or bytNewCol = 9 Then
    'color disable was chosen
    blnOff = True
  Else
    'if on first row
    If bytNewCol < 9 Then
      'subtract one
      bytNewCol = bytNewCol - 1
    Else
      'subtract two
      bytNewCol = bytNewCol - 2
    End If
  End If
  
  Select Case Shift
  Case 0, vbShiftMask 'no key pressed, or Shift
    'if changing visual or priority color, check
    'for valid condition first
  
    'if no cmd selected
    If lstCommands.ListIndex = -1 Then
      'ignore change request
      Exit Sub
    End If
    
    'if in test mode,
    If PicMode = pmTest Then
      Exit Sub
    End If
    
    'if in a draw operation
    If PicDrawMode <> doNone Then
      'ignore change request
'      '*'Debug.Assert False  '(don't think this is ever possible, as drawmode is based on mouse actions on main canvas)
      Exit Sub
    End If
    
    'if shift key, then force new command
    blnForce = (Shift = vbShiftMask)
    
    Select Case Button
    Case vbLeftButton
      'if disabling
      If blnOff Then
        SelectedPen.VisColor = agNone
      Else
        SelectedPen.VisColor = bytNewCol
      End If
    Case vbRightButton
      'if disabling
      If blnOff Then
        SelectedPen.PriColor = agNone
      Else
        SelectedPen.PriColor = bytNewCol
      End If
    End Select
    
    'refresh now to match colors
    RefreshPens blnForce
    
  Case vbCtrlMask
  'pressing ctrl will change cursor color, but can't select 'no color'
  
    If Not blnOff Then
      If Button = vbLeftButton Then
        VCColor = EGAColor(bytNewCol)
      ElseIf Button = vbRightButton Then
        PCColor = EGAColor(bytNewCol)
      Else
        'ignore any other button scenarios
        Exit Sub
      End If
      'if showing the cursors (in 'xmode', selected tool is 'none' and only one cmd is selected
      If CursorMode = pcmXMode And SelectedTool = ttEdit And lstCommands.SelCount = 1 Then
        'only in edit mode
        If PicMode = pmEdit Then
          HighlightCoords
        End If
      End If
    End If
    Exit Sub
  
  End Select
End Sub


Private Sub picPalette_Paint()
  'draw the choice of colors into the palette box
  
  Dim i As Integer, j As Integer
  Dim dblWidth As Double
  
  dblWidth = picPalette.Width / 9
  
  'paint disabled brush area
  picPalette.Line (0, 0)-(dblWidth, 32), vbWhite, BF
  picPalette.DrawWidth = 2
  picPalette.Line (0, 0)-(dblWidth, 32), vbBlack, B
  picPalette.Line (0, 0)-(dblWidth, 32), vbBlack
  picPalette.Line (0, 32)-(dblWidth, 0), vbBlack
  picPalette.DrawWidth = 1
  
  'paint color area
  For i = 0 To 1
    For j = 0 To 7
      picPalette.Line ((j + 1) * dblWidth, i * 17)-((j + 2) * dblWidth, i * 17 + 16), EGAColor(i * 8 + j), BF
    Next j
  Next i
  
  'add 'V' for current selected visual color
  'if enabled
  If SelectedPen.VisColor < agNone Then
    'if a light color
    If SelectedPen.VisColor > 9 Then
      'use black
      picPalette.ForeColor = vbBlack
    Else
      'otherwise, use white
      picPalette.ForeColor = vbWhite
    End If
    'set x and Y to position 'v' over correct color
    picPalette.CurrentX = dblWidth * ((SelectedPen.VisColor Mod 8) + 1) + 3
    picPalette.CurrentY = 17 * (SelectedPen.VisColor \ 8) + 1
    picPalette.Print "V"
  Else
    'put 'v' in disabled square
    picPalette.CurrentX = 3
    picPalette.CurrentY = 9
    picPalette.ForeColor = vbBlack
    picPalette.Print "V"
  End If
  
  'add 'P' for current selected priority color
  'if enabled
  If SelectedPen.PriColor < agNone Then
    'if a light color
    If SelectedPen.PriColor > 9 Then
      'use black
      picPalette.ForeColor = vbBlack
    Else
      'otherwise, use white
      picPalette.ForeColor = vbWhite
    End If
    ' set x and Y to position 'P' over correct color
    picPalette.CurrentX = dblWidth * ((SelectedPen.PriColor Mod 8) + 2) - 13
    picPalette.CurrentY = 17 * (SelectedPen.PriColor \ 8) + 1
    picPalette.Print "P"
  Else
    'put 'P' in disabled square
    picPalette.CurrentX = dblWidth - 10
    picPalette.CurrentY = 9
    picPalette.ForeColor = vbBlack
    picPalette.Print "P"
  End If
End Sub


Private Sub DrawBox(ByVal StartX As Long, ByVal StartY As Long, ByVal EndX As Long, ByVal EndY As Long)
  
    DrawLine StartX, StartY, EndX, StartY
    DrawLine EndX, StartY, EndX, EndY
    DrawLine EndX, EndY, StartX, EndY
    DrawLine StartX, EndY, StartX, StartY
    picVisual.Refresh
    picPriority.Refresh
End Sub

Private Sub picPriority_GotFocus()
  
  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub

Private Sub picPriority_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  'set inpri flag
  blnInPri = True
  'pass to visual
  picVisual_MouseDown Button, Shift, X, Y
End Sub

Private Sub picPriority_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  picVisual_MouseMove Button, Shift, X, Y
End Sub

Private Sub picPriority_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  picVisual_MouseUp Button, Shift, X, Y
End Sub

Private Sub picPriSurface_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub


Private Sub picPriSurface_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  On Error GoTo ErrHandler
  
  'if right button
  If Button = vbRightButton Then
    'reset edit menu first
    SetEditMenu
    'make sure this form is the active form
    If Not (frmMDIMain.ActiveForm Is Me) Then
      'set focus before showing the menu
      Me.SetFocus
    End If
    'need doevents so form activation occurs BEFORE popup
    'otherwise, errors will be generated because of menu
    'adjustments that are made in the form_activate event
    SafeDoEvents
    'show edit menu
    PopupMenu frmMDIMain.mnuEdit, , picPriSurface.Left + X, picPriSurface.Top + Y
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub picSplitH_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub

Private Sub picSplitH_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  'if on the split loc
    'begin split operation
    picSplitHIcon.Width = picSplitH.Width
    picSplitHIcon.Move picSplitH.Left, picSplitH.Top
    picSplitHIcon.Visible = True
    
    'save offset
    SplitOffset = picSplitH.Top - Y
End Sub

Private Sub picSplitH_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim Pos As Single
  
  'if splitting
  If picSplitHIcon.Visible Then
    'adjust to be within bounds
    Pos = Y + SplitOffset
    
    If Pos < SPLIT_HEIGHT * 2 Then
      Pos = SPLIT_HEIGHT
    ElseIf Pos < 100 Then
      Pos = 100
    End If
    
    If Pos > picPalette.Top - SPLIT_HEIGHT * 2 Then
      Pos = picPalette.Top - SPLIT_HEIGHT
    ElseIf Pos > picPalette.Top - 100 Then
      Pos = picPalette.Top - 100
    End If
    
    'move splitter
    picSplitHIcon.Top = Pos
  End If
End Sub


Private Sub picSplitH_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  Dim Pos As Single, PrevSplitState As Long
  
  'save current split state
  PrevSplitState = OneWindow
  
  'if splitting
  If picSplitHIcon.Visible Then
    'stop splitting
    picSplitHIcon.Visible = False
    
    'adjust to be within bounds
    Pos = Y + SplitOffset
    'assume two windows unless user chooses otherwise
    OneWindow = 0
    
    'if at extreme top
    If Pos < SPLIT_HEIGHT * 2 Then
      'set pos to zero to hide the visual drawing surface
      Pos = 0
      OneWindow = 2
    'if not hiding visual drawing surface,
    ElseIf Pos < 100 Then
      'limit visual drawing surace to 100 pixels high
      Pos = 100
    End If
    
    'if at extreme bottom
    If Pos > picPalette.Top - SPLIT_HEIGHT * 2 Then
      'hide priority drawing surface
      Pos = picPalette.Top - picSplitH.Height
      OneWindow = 1
    'if not hiding priority drawing surface,
    ElseIf Pos > picPalette.Top - 100 Then
      'limit priority drawing surface to 100 pixels high
      Pos = picPalette.Top - 100
    End If
    
    'recalculate splitratio
    SplitRatio = (Pos - picVisSurface.Top) / (CalcHeight - picVisSurface.Top - picSplitH.Height - picPalette.Height)
    
    
    'redraw!
    UpdatePanels Pos, picSplitV.Left
    
    'if split state has changed, may need to update images before showing them
    Select Case PrevSplitState
    Case 1  'only visual
      If OneWindow = 0 Or OneWindow = 2 Then
        'need to update priority
        DrawPicture
      End If
    Case 2 'only priority
      If OneWindow = 0 Or OneWindow = 1 Then
        'need to update visual
        DrawPicture
      End If
    End Select
  End If
End Sub


Private Sub picSplitV_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim Pos As Single
    
  
  'if splitting
  If picSplitVIcon.Visible Then
    Pos = X + SplitOffset
    'limit movement
    If Pos < MIN_SPLIT_V Then
      Pos = MIN_SPLIT_V
    ElseIf Pos > MAX_SPLIT_V Then
      Pos = MAX_SPLIT_V
    End If
    
    'move splitter
    picSplitVIcon.Left = Pos
  End If
End Sub

Private Sub picSplitV_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim Pos As Single
  
  'if splitting
  If picSplitVIcon.Visible Then
    'stop splitting
    picSplitVIcon.Visible = False
    
    Pos = X + SplitOffset
    'limit movement
    If Pos < MIN_SPLIT_V Then
      Pos = MIN_SPLIT_V
    ElseIf Pos > MAX_SPLIT_V Then
      Pos = MAX_SPLIT_V
    End If

    'redraw!
    UpdatePanels picSplitH.Top, Pos
  End If
End Sub


Private Sub picVisSurface_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub


Private Sub picVisSurface_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)
  
  On Error GoTo ErrHandler
  
  'if right button
  If Button = vbRightButton Then
    'reset edit menu first
    SetEditMenu
    'make sure this form is the active form
    If Not (frmMDIMain.ActiveForm Is Me) Then
      'set focus before showing the menu
      Me.SetFocus
    End If
    'need doevents so form activation occurs BEFORE popup
    'otherwise, errors will be generated because of menu
    'adjustments that are made in the form_activate event
    SafeDoEvents
    'show edit menu
    PopupMenu frmMDIMain.mnuEdit, , picVisSurface.Left + X, picVisSurface.Top + Y
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub picVisSurface_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim tmpX As Single, tmpY As Single
  
  'if not active form
  If Not frmMDIMain.ActiveForm Is Me Then
    Exit Sub
  End If
  
  'if dragging picture
  If blnDragging Then
    'get new scrollbar positions
    tmpX = sngOffsetX - X
    tmpY = sngOffsetY - Y
    
    'if vertical scrollbar is visible
    If vsbVis.Visible Then
      'limit positions to valid values
      If tmpY < vsbVis.Min Then
        tmpY = vsbVis.Min
      ElseIf tmpY > vsbVis.Max Then
        tmpY = vsbVis.Max
      End If
      'set vertical scrollbar
      vsbVis.Value = tmpY
    End If
    
    'if horizontal scrollbar is visible
    If hsbVis.Visible Then
      'limit positions to valid values
      If tmpX < hsbVis.Min Then
        tmpX = hsbVis.Min
      ElseIf tmpX > hsbVis.Max Then
        tmpX = hsbVis.Max
      End If
      'set horizontal scrollbar
      hsbVis.Value = tmpX
    End If
  End If
End Sub


Private Sub picVisSurface_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim rtn As Long
  
  'if dragging
  If blnDragging Then
    'cancel dragmode
    blnDragging = False
    'release mouse capture
    rtn = ReleaseCapture()
    SetCursors pcEdit
  End If
End Sub
Private Sub picVisual_GotFocus()

  'ensure flyout toolbars hidden
  tbStyle.Visible = False
  tbSize.Visible = False
End Sub

Private Sub picVisual_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim PicPt As PT, lInPri As Boolean
  Dim bytData() As Byte, i As Long
  Dim bytPattern As Byte
  
  On Error GoTo ErrHandler
  
  'if activating, ignore
  If Activating Then
    Activating = False
    Exit Sub
  End If
  
  'save inpri status
  lInPri = blnInPri
  
  'calculate position
  PicPt.X = X \ (2 * ScaleFactor)
  PicPt.Y = Y \ ScaleFactor
  
  'reset flag that tracks which drawing surface was clicked
  blnInPri = False
  
  'first check for right-mouse click
  If (Button = vbRightButton) Then
    'if currently drawing something, right-click ends it
    If PicDrawMode <> doNone Then
      StopDrawing
    Else
      'reset edit menu first
      SetEditMenu
      'make sure this form is the active form
      If Not (frmMDIMain.ActiveForm Is Me) Then
        'set focus before showing the menu
        Me.SetFocus
      End If
      'need doevents so form activation occurs BEFORE popup
      'otherwise, errors will be generated because of menu
      'adjustments that are made in the form_activate event
      SafeDoEvents
      'show edit menu
      If lInPri Then
        PopupMenu frmMDIMain.mnuEdit, , picPriority.Left + picPriSurface.Left + X, picPriority.Top + picPriSurface.Top + Y
      Else
        PopupMenu frmMDIMain.mnuEdit, , picVisual.Left + picVisSurface.Left + X, picVisual.Top + picVisSurface.Top + Y
      End If
    End If
    Exit Sub
  End If
  
  Select Case PicMode
  Case pmEdit
    'if we are drawing something
    If (PicDrawMode <> doNone) Then
      'finish drawing function
      EndDraw PicPt
      Exit Sub
    End If
  
    'what to do depends mostly on what the selected tool is:
    Select Case SelectedTool
    Case ttEdit
      'no tool selected; check for a coordinate being moved or group of commands being moved
      'if none of those apply, drag the drawing surface
      
      'first, see if we need to select the current coordinate:
      If CursorMode = pcmXMode Then
        If CurCursor = pcCross And (CurPt.X <> PicPt.X Or CurPt.Y <> PicPt.Y) Then
          'we are on a coordinate that is NOT the currently selected coordinate!
          'select it, and then continue
          For i = 0 To lstCoords.ListCount - 1
            If CoordPT(i).X = PicPt.X And CoordPT(i).Y = PicPt.Y Then
              lstCoords.ListIndex = i
              Exit For
            End If
          Next i
        End If
      End If
      
      If (CurPt.X = PicPt.X) And (CurPt.Y = PicPt.Y) Then
        '*'Debug.Assert lstCoords.ListIndex <> -1
        '*'Debug.Assert tmrSelect.Enabled Or CursorMode = pcmXMode
        'three cases; if on any coord and SHIFT key is pressed, then move entire command
        '             if on any coord and CTRL key is pressed, add a new coord, then begin moving it
        '             if on any coord and no key is pressed, begin moving just the coord
        '             (if combo of keys pressed, just ignore)
        Select Case Shift
        Case 0 'no key pressed
          'begin editing the coordinate
          PicDrawMode = doMovePt
          'get edit cmd
          EditCmd = PicEdit.Resource.Data(CLng(lstCommands.ItemData(SelectedCmd)))
          'turn off cursor flasher
          tmrSelect.Enabled = False
          Exit Sub
          
        Case vbCtrlMask
          'insert a new coord, then begin moving it
          InsertCoordinate True
          Exit Sub
        
        Case vbShiftMask
          'set draw mode to move cmd
          PicDrawMode = doMoveCmds
          'set anchor
          Anchor = PicPt
          
          'get start and end coords of selection and show selection
          GetSelectionBounds SelectedCmd, lstCommands.SelCount, True
          
          'get delta from current point to selstart
          Delta.X = PicPt.X - SelStart.X
          Delta.Y = PicPt.Y - SelStart.Y
          
          'change cursor
          SetCursors pcMove
          'set curpt to invalid Value so mousemove will
          'update the selection even if moved back to starting point
          CurPt.X = 255
          Exit Sub
        
        Case Else
          'ignore
          Exit Sub
        End Select
        
      'if multiple cmds selected (i.e. the selection size is >0), begin moving them
      '(need to make sure cmds are selected, and NOT showing a screen grab selection
      
      ElseIf (SelSize.X > 0) And (SelSize.Y > 0) Then
        '*'Debug.Assert shpPri.Visible
        'is cursor within the shape?
        If PicPt.X >= SelStart.X And PicPt.X <= SelStart.X + SelSize.X And _
           PicPt.Y >= SelStart.Y And PicPt.Y <= SelStart.Y + SelSize.Y Then
            
          'set draw mode to move cmd
          PicDrawMode = doMoveCmds
          
          'get start and end coords of selection, then draw box around them
          GetSelectionBounds SelectedCmd, lstCommands.SelCount, True
          
          'set anchor
          Anchor = PicPt
          'get delta from current point to selstart
          Delta.X = PicPt.X - SelStart.X
          Delta.Y = PicPt.Y - SelStart.Y
          
          'set curpt to invalid Value so mousemove will
          'update the selection even if moved back to starting point
          CurPt.X = 255
          Exit Sub
        Else
          'not moving commands; drag the picture
          StartDrag lInPri, X, Y
          Exit Sub
        End If
      Else
        'not moving commands; drag the picture
        StartDrag lInPri, X, Y
        Exit Sub
      End If
      
    Case ttLine, ttRelLine, ttCorner
      'begin draw operation based on selected tool
      BeginDraw SelectedTool, PicPt
    
    Case ttFill
      'if on a Fill cmd
      If lstCommands.Text = "Fill" Then
        'if cursor hasn't moved, just exit
        If Anchor.X = PicPt.X And Anchor.Y = PicPt.Y Then
          Exit Sub
        End If
        'add this coordinate to end of list
        ReDim bytData(1)
        bytData(0) = PicPt.X
        bytData(1) = PicPt.Y
        AddCoordToPic bytData, -1, PicPt.X, PicPt.Y
        'need to select the coord just added so it will show
        NoSelect = True
        lstCoords.ListIndex = lstCoords.NewIndex
        'save point as anchor
        Anchor = PicPt
      Else
        'add fill command
        ReDim bytData(2)
        bytData(0) = dfFill
        bytData(1) = PicPt.X
        bytData(2) = PicPt.Y
        InsertCommand bytData, SelectedCmd, LoadResString(DRAWFUNCTIONTEXT + dfFill - &HF0), gInsertBefore
        'select this cmd
        SelectCmd lstCommands.NewIndex, False
        'and select first coord
        NoSelect = True
        lstCoords.ListIndex = 0
        'save point as anchor
        Anchor = PicPt
      End If
      
      'redraw
      DrawPicture
      
    Case ttPlot
      'need to bound the x value (AGI has a bug which actually allows
      'X values to be +1 more than they should; WinAGI enforces the
      'the actual boundary
      If PicPt.X > 159 - CurrentPen.PlotSize \ 2 Then
        PicPt.X = 159 - CurrentPen.PlotSize \ 2
      End If
      
      'if on a coordinate that is part of a plot cmd
      If lstCommands.Text = "Plot" Then
        'only need to add the plot coordinate
      
        'if current pen is solid,
        If CurrentPen.PlotStyle = psSolid Then
          'add this coordinate to end of list
          ReDim bytData(1)
          bytData(0) = PicPt.X
          bytData(1) = PicPt.Y
          AddCoordToPic bytData(), -1, PicPt.X, PicPt.Y
          NoSelect = True
          lstCoords.ListIndex = lstCoords.NewIndex
        Else
          'add pattern too
          Randomize Now()
          bytPattern = 1 + CByte(Int(Rnd * 119))
          'add this coordinate to end of list
          ReDim bytData(2)
          'pattern is multiplied by two before storing
          bytData(0) = 2 * bytPattern
          bytData(1) = PicPt.X
          bytData(2) = PicPt.Y
          AddCoordToPic bytData(), -1, PicPt.X, PicPt.Y, CStr(bytPattern) & " -- "
          NoSelect = True
          lstCoords.ListIndex = lstCoords.NewIndex
        End If
      Else
        'if not already in a plot command, the plot command
        'needs to be included with the plot coordinates
        
        'if current pen is solid,
        If CurrentPen.PlotStyle = psSolid Then
          ReDim bytData(2)
          bytData(0) = dfPlotPen
          bytData(1) = PicPt.X
          bytData(2) = PicPt.Y
          'add command
          InsertCommand bytData, SelectedCmd, LoadResString(DRAWFUNCTIONTEXT + dfPlotPen - &HF0), gInsertBefore
          'select this cmd
          SelectCmd lstCommands.NewIndex, False
          'and select first coordinate
          NoSelect = True
          lstCoords.ListIndex = 0
        Else
          ReDim bytData(3)
          'add pattern too
          bytPattern = 1 + CByte(Int(Rnd * 119))
          bytData(0) = dfPlotPen
          'pattern is multiplied by two before storing
          bytData(1) = 2 * bytPattern
          bytData(2) = PicPt.X
          bytData(3) = PicPt.Y
          'add command
          InsertCommand bytData, SelectedCmd, LoadResString(DRAWFUNCTIONTEXT + dfPlotPen - &HF0), gInsertBefore
          'select this cmd
          SelectCmd lstCommands.NewIndex, False
          'and select first coordinate
          NoSelect = True
          lstCoords.ListIndex = 0
        End If
      End If
      
      'redraw
      DrawPicture
      
    Case ttRectangle, ttTrapezoid, ttEllipse
      'set anchor
      Anchor = PicPt
      'set mode
      PicDrawMode = doShape
      
    Case ttSelectArea
      'begin selecting an area
      PicDrawMode = doSelectArea
      Anchor = PicPt
      'reset selection
      SelStart.X = 0
      SelStart.Y = 0
      SelSize.X = 0
      SelSize.Y = 0
      ShowCmdSelection
      ' verify status bar is correctly set
      With MainStatusBar
        If .Tag <> CStr(rtPicture) Then
          AdjustMenus rtPicture, Me.InGame, True, Me.IsDirty
        End If
        .Panels("Anchor").Visible = False
        .Panels("Block").Visible = False
      End With
    End Select
    
  Case pmTest
    'stop testview object motion
    TestDir = 0
    tmrTest.Enabled = TestSettings.CycleAtRest
    'if above top edge OR
    '   (NOT ignoring horizon AND above horizon ) OR
    '   (on water AND restricting to land) OR
    '   (NOT on water AND restricting to water)
    If (PicPt.Y - (CelHeight - 1) < 0) Or (PicPt.Y < TestSettings.Horizon And Not TestSettings.IgnoreHorizon) Or _
       (PicEdit.ObjOnWater(PicPt.X, PicPt.Y, CelWidth) And TestSettings.ObjRestriction = 2) Or _
      (Not PicEdit.ObjOnWater(PicPt.X, PicPt.Y, CelWidth) And TestSettings.ObjRestriction = 1) Then
        Exit Sub
    End If
    'draw testview in new location
    DrawPicture False, True, PicPt.X, PicPt.Y
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub picVisual_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)
  Dim PicPt As PT
  Dim rtn As Long, i As Long
  Dim SelAnchor As PT
  Dim tmpX As Long, tmpY As Long
  Dim NewPri As Long

  On Error GoTo ErrHandler

  'if this form is not active
  If Not frmMDIMain.ActiveForm Is Me Then
    Exit Sub
  End If
  
  'calculate position
  tmpX = X \ (2 * ScaleFactor)
  tmpY = Y \ ScaleFactor

  'bound position
  If tmpX < 0 Then
    PicPt.X = 0
  ElseIf tmpX > 159 Then
    PicPt.X = 159
  Else
    PicPt.X = tmpX
  End If
  If tmpY < 0 Then
    PicPt.Y = 0
  ElseIf tmpY > 167 Then
    PicPt.Y = 167
  Else
    PicPt.Y = tmpY
  End If

  Select Case PicMode
  'in edit mode, action taken depends primarily on
  'current draw operation mode
  Case pmEdit
    Select Case PicDrawMode
    Case doNone
      'not drawing anything- need to determine correct mousepointer based on current
      'mouse position and selected tool

      Select Case SelectedTool
      Case ttEdit
        'not drawing anything, but there could be a highlighted coordinate
        'or a selected group of commands- cursor will depend on which of
        'those states exist

        'if selection is visible and tool=none, then must be moving cmds
        If (SelSize.X > 0) And (SelSize.Y > 0) Then
          'is cursor over cmds?
          If PicPt.X >= SelStart.X And PicPt.X <= SelStart.X + SelSize.X And PicPt.Y >= SelStart.Y And PicPt.Y <= SelStart.Y + SelSize.Y Then
            'use move cursor
            SetCursors pcMove
          Else
            'use normal 'edit' cursor
            SetCursors pcEdit
          End If
        Else
        'check for editing coordinate (stepdraw should not matter)
'        If (PicEdit.StepDraw) And lstCoords.ListIndex <> -1 Then
          If (CurPt.X = PicPt.X) And (CurPt.Y = PicPt.Y) Then
            SetCursors pcCross
            OnPoint = True
            If CursorMode = pcmWinAGI Then
              'reset area under cursor
              BitBlt picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, 6 * ScaleFactor, 3 * ScaleFactor, Me.hDC, 0, 0, SRCCOPY
              BitBlt picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, 6 * ScaleFactor, 3 * ScaleFactor, Me.hDC, 0, 12, SRCCOPY
              picVisual.Refresh
              picPriority.Refresh
            End If
          Else
            If CursorMode = pcmXMode Then
              'check to see if cursor is over one of the other coordinates
              'if cursor is over one of the coord points
              For i = 0 To lstCoords.ListCount - 1
                If (PicPt.X = CoordPT(i).X) And (PicPt.Y = CoordPT(i).Y) Then
                  'this is one of the vertices; can't be the currently selected one - that would
                  'have been detected already... not that it really matters
                  SetCursors pcCross
                  i = -1  'so we can tell if loop exited due to finding a point
                  Exit For
                End If
              Next i
              If i <> -1 Then
                'nothing going on- use normal cursor
                SetCursors pcEdit
                OnPoint = False
              End If
            Else
              'nothing going on- use normal cursor
              SetCursors pcEdit
              OnPoint = False
            End If
          End If
        End If

      'hmmm, don't think we need to do ANYTHING when one of these tools
      'is active while no draw ops are in progress;
      'cursor should already be set, with no need to change cursor
      'while mouse is moving
      Case ttSetPen
      Case ttLine
      Case ttRelLine
      Case ttCorner
      Case ttFill
      Case ttPlot
      Case ttRectangle
      Case ttTrapezoid
      Case ttEllipse
      Case ttSelectArea


      End Select

    Case doSelectArea
      'adjust selection bounds to match current mouse location

      'set shape anchor, based on relation of this point to actual anchor
      SelAnchor.X = IIf(PicPt.X < Anchor.X, PicPt.X, Anchor.X)
      SelAnchor.Y = IIf(PicPt.Y < Anchor.Y, PicPt.Y, Anchor.Y)
      'set selection parameters to match current selected area
      SelStart = SelAnchor
      SelSize.X = Abs(CLng(PicPt.X) - Anchor.X) + 1
      SelSize.Y = Abs(CLng(PicPt.Y) - Anchor.Y) + 1
      'draw the cmd selection box
      ShowCmdSelection

    Case doLine, doShape, doMoveCmds, doMovePt
      'only need to do something if cursor position has changed
      'since last time drawing surface was updated

      'if coordinates have changed,
      If PicPt.X <> CurPt.X Or PicPt.Y <> CurPt.Y Then

        'disable updating of redraw until done
        SendMessage picVisual.hWnd, WM_SETREDRAW, 0, 0
        SendMessage picPriority.hWnd, WM_SETREDRAW, 0, 0

        'redraw picture (force draw- disabling redraw sets visible property to false)
        DrawPicture True

        'take action as appropriate
        Select Case PicDrawMode
        Case doLine
          'action to take depends on what Type of line is being drawn
          Select Case SelectedTool
          Case ttLine
            'draw current line up to anchor point
            DrawTempLine False, 0, 0
            'now draw line from anchor to cursor position
            DrawLine Anchor.X, Anchor.Y, PicPt.X, PicPt.Y

          Case ttRelLine
            'draw current line up to anchor point
            DrawTempLine False, 0, 0
            'validate x and Y
            '(note that delta x is limited to -6 to avoid
            'values above &HF0, which would mistakenly be interpreted
            'as a new command)
            If PicPt.X > Anchor.X + 7 Then
              PicPt.X = Anchor.X + 7
            ElseIf PicPt.X < Anchor.X - 6 Then
              PicPt.X = Anchor.X - 6
            End If
            If PicPt.Y > Anchor.Y + 7 Then
              PicPt.Y = Anchor.Y + 7
            ElseIf PicPt.Y < Anchor.Y - 7 Then
              PicPt.Y = Anchor.Y - 7
            End If

            'now draw line from anchor to cursor position
            DrawLine Anchor.X, Anchor.Y, PicPt.X, PicPt.Y

          Case ttCorner
            'draw up to this coordinate
            DrawTempLine False, 0, 0
            'if drawing second point
            If lstCoords.ListCount = 1 Then
              'if mostly vertical
              If Abs(CLng(PicPt.X) - Anchor.X) < Abs(CLng(PicPt.Y) - Anchor.Y) Then
                'command should be Y corner
                If Asc(lstCommands.Text) <> 89 Then
                  lstCommands.List(SelectedCmd) = "Y Corner"
                  PicEdit.Resource.Data(lstCommands.ItemData(SelectedCmd)) = dfYCorner
                  'correct last undo
                  UndoCol(UndoCol.Count).UDCmd = "Y Corner"
                End If
                'limit change to vertical direction only
                PicPt.X = Anchor.X
              Else
                'command should be X corner
                If Asc(lstCommands.Text) = 89 Then
                  lstCommands.List(SelectedCmd) = "X Corner"
                  PicEdit.Resource.Data(lstCommands.ItemData(SelectedCmd)) = dfXCorner
                  'correct last undo
                  UndoCol(UndoCol.Count).UDCmd = "X Corner"
                End If
                'limit change to horizontal direction only
                PicPt.Y = Anchor.Y
              End If
            Else
              'determine which direction to allow movement
              If (Asc(lstCommands.Text) = 88 And (Int(lstCoords.ListCount / 2) = lstCoords.ListCount / 2)) Or _
                 (Asc(lstCommands.Text) = 89 And (Int(lstCoords.ListCount / 2) <> lstCoords.ListCount / 2)) Then
                'limit change to vertical direction
                PicPt.X = Anchor.X
              Else
                'limit change to horizontal direction
                PicPt.Y = Anchor.Y
              End If
            End If

            'now draw line from anchor to cursor position
            DrawLine Anchor.X, Anchor.Y, PicPt.X, PicPt.Y
          End Select

        Case doShape
          'action to take depends on what Type of line is being drawn
          Select Case SelectedTool
          Case ttRectangle
            'simulate rectangle
            CurPt.X = PicPt.X
            CurPt.Y = PicPt.Y
            DrawBox Anchor.X, Anchor.Y, CurPt.X, CurPt.Y

          Case ttTrapezoid
            'simulate a trapezoid
            CurPt.X = PicPt.X
            CurPt.Y = PicPt.Y
            DrawLine Anchor.X, Anchor.Y, 159 - Anchor.X, Anchor.Y
            'ensure sloping side is on same side of picture
            If (Anchor.X < 80 And PicPt.X < 80) Or (Anchor.X >= 80 And PicPt.X >= 80) Then
             DrawLine 159 - Anchor.X, Anchor.Y, 159 - CurPt.X, CurPt.Y
              DrawLine 159 - CurPt.X, CurPt.Y, CurPt.X, CurPt.Y
              DrawLine CurPt.X, CurPt.Y, Anchor.X, Anchor.Y
            Else
              DrawLine 159 - Anchor.X, Anchor.Y, CurPt.X, CurPt.Y
              DrawLine CurPt.X, CurPt.Y, 159 - CurPt.X, CurPt.Y
              DrawLine 159 - CurPt.X, CurPt.Y, Anchor.X, Anchor.Y
            End If

          Case ttEllipse
            'simulate circle
            CurPt.X = PicPt.X
            CurPt.Y = PicPt.Y
            DrawCircle Anchor.X, Anchor.Y, CurPt.X, CurPt.Y
          End Select

        Case doMoveCmds
          'limit selection box movement to stay within picture bounds
          If CLng(PicPt.X) - Delta.X < 0 Then
            PicPt.X = Delta.X
          ElseIf CLng(PicPt.X) - Delta.X + SelSize.X > 160 Then
            PicPt.X = 160 - SelSize.X + Delta.X
          End If
          If CLng(PicPt.Y) - Delta.Y < 0 Then
            PicPt.Y = Delta.Y
          ElseIf CLng(PicPt.Y) - Delta.Y + SelSize.Y > 168 Then
            PicPt.Y = 168 - SelSize.Y + Delta.Y
          End If
          
          'now adjust selection start pos to match new location, then move selection box
          SelStart.X = PicPt.X - Delta.X
          SelStart.Y = PicPt.Y - Delta.Y
          ShowCmdSelection

        Case doMovePt
          'simulate new coordinate
          CurPt.X = PicPt.X
          CurPt.Y = PicPt.Y

          'if currently editing a line
          If lstCommands.Text <> "Fill" And lstCommands.Text <> "Plot" Then
            'draw temp line to include edited position
            DrawTempLine True, CurPt.X, CurPt.Y
          End If

        End Select

        'refresh pictures
        SendMessage picVisual.hWnd, WM_SETREDRAW, 1, 0
        SendMessage picPriority.hWnd, WM_SETREDRAW, 1, 0
        picVisual.Refresh
        picPriority.Refresh
      End If

    Case doFill
      'should never get here
      '*'Debug.Assert False

    End Select

  Case pmTest
    'in test mode, all we need to do is provide feeback to user
    'on whether or not it's OK to drop the test object at this location

    'if above top edge OR
    '   (NOT ignoring horizon AND above horizon ) OR
    '   (on water AND restricting to land) OR
    '   (NOT on water AND restricting to water)
    If (PicPt.Y - (CelHeight - 1) < 0) Or (PicPt.Y < TestSettings.Horizon And Not TestSettings.IgnoreHorizon) Or _
       (PicEdit.ObjOnWater(PicPt.X, PicPt.Y, CelWidth) And TestSettings.ObjRestriction = 2) Or _
      (Not PicEdit.ObjOnWater(PicPt.X, PicPt.Y, CelWidth) And TestSettings.ObjRestriction = 1) Then
      'set cursor to NO
      SetCursors pcNO
    Else
      'set cursor to normal
      SetCursors pcDefault
    End If
  End Select

  'in some cases, the main form's status bar and menus
  'can get out of synch- so we test for that, and resynch
  'if necessary
  If MainStatusBar.Tag <> CStr(rtPicture) Then
    AdjustMenus rtPicture, InGame, True, IsDirty
  End If

  '*'Debug.Assert frmMDIMain.ActiveForm Is Me

  'when moving mouse, we always need to reset the status bar
  With MainStatusBar
    'if NOT in test mode OR statussrc is NOT set
    If PicMode = pmEdit Or Not StatusSrc Then
'''      'disable redraw to reduce flicker
'''SendMessage .hwnd, WM_SETREDRAW, 0, 0
      .Panels("CurX").Text = "X: " & CStr(PicPt.X)
      .Panels("CurY").Text = "Y: " & CStr(PicPt.Y)
      NewPri = GetPriBand(PicPt.Y, PicEdit.PriBase)
      .Panels("PriBand").Text = "Band: " & NewPri
      If SelectedTool = ttSelectArea Then
        If shpVis.Visible Then
          If Button = vbLeftButton Then
          .Panels("Anchor").Visible = True
          .Panels("Block").Visible = True
          .Panels("Anchor").Text = "Anchor: " & SelStart.X & ", " & SelStart.Y
          .Panels("Block").Text = "Block: " & SelStart.X & ", " & SelStart.Y & ", " & SelStart.X + SelSize.X - 1 & ", " & SelStart.Y + SelSize.Y - 1
          End If
        Else
'          .Panels("Anchor").Text = "Anchor: " & PicPt.X & ", " & PicPt.Y
'          .Panels("Block").Text = "Block: " & PicPt.X & ", " & PicPt.Y
          .Panels("Anchor").Visible = False
          .Panels("Block").Visible = False
        End If
      End If
'''SendMessage .hwnd, WM_SETREDRAW, 1, 0
''''now redraw the control
''''(can't have redraw off when changinging priority picture;
''''there is some sort of bug that messes up the redraw function)
      'if priority has changed, update the color box
      If NewPri <> OldPri Then
''''        .Panels("PriBand").Picture = Nothing
        .Panels("PriBand").Picture = imlPriBand.ListImages(GetPriBand(PicPt.Y, PicEdit.PriBase) - 3).Picture
      End If
      OldPri = NewPri
''''.Refresh
''' rtn = RedrawWindow(.hwnd, a, 0, RDW_ERASE Or RDW_FRAME Or RDW_INVALIDATE Or RDW_ALLCHILDREN)
''''rtn = RedrawWindow(.hwnd, a, 0, RDW_ERASENOW Or RDW_FRAME Or RDW_INVALIDATE Or RDW_ALLCHILDREN)
''''.Panels("Block").Picture = Me.imlSB.ListImages(SBPic).Picture
'''If SBPic = 1 Then SBPic = 2 Else SBPic = 1
'''    '*'Debug.Print SBPic
    End If
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub picVisual_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim PicPt As PT
  
  On Error GoTo ErrHandler
  
  If X < 0 Then
    PicPt.X = 0
  ElseIf X \ (2 * ScaleFactor) > 159 Then
    PicPt.X = 159
  Else
    PicPt.X = X \ (2 * ScaleFactor)
  End If
  
  If Y < 0 Then
    PicPt.Y = 0
  ElseIf Y \ ScaleFactor > 167 Then
    PicPt.Y = 167
  Else
    PicPt.Y = Y \ ScaleFactor
  End If
  
  'how to handle mouseup event depends primarily on what was being drawn (or not)
  
  Select Case PicDrawMode
  Case doNone
    'will occasionally get this case
    'such as when right-clicking or...
    
  'Case doLine, doFill, doShape
  'lines and shapes are not completed on mouse_up actions; they
  'are done by clicking to start, then clicking again to end
  'so it's the mouse-down action that both starts and ends the operation
  'that's why we don't need to check for them here in the MouseUp event
  
  Case doSelectArea
    'reset the draw mode
    PicDrawMode = doNone
    
  Case doMovePt 'editing a coordinate
    'reset drawmode
    PicDrawMode = doNone
    
    'edit the coordinate
    EndEditCoord EditCmd, EditCoordNum, PicPt, lstCoords.ItemData(lstCoords.ListIndex), lstCoords.Text
    
    'update by re-building coordlist, and selecting
    BuildCoordList SelectedCmd
    lstCoords.ListIndex = EditCoordNum
    
  Case doMoveCmds
    'reset drawmode
    PicDrawMode = doNone
    
    'limit selection box movement to stay within picture bounds
    If CLng(PicPt.X) - Delta.X < 0 Then
      PicPt.X = Delta.X
    ElseIf CLng(PicPt.X) - Delta.X + SelSize.X > 160 Then
      PicPt.X = 160 - SelSize.X + Delta.X
    End If
    If CLng(PicPt.Y) - Delta.Y < 0 Then
      PicPt.Y = Delta.Y
    ElseIf CLng(PicPt.Y) - Delta.Y + SelSize.Y > 168 Then
      PicPt.Y = 168 - SelSize.Y + Delta.Y
    End If
    
    'move the command(s)
    MoveCmds SelectedCmd, lstCommands.SelCount, CLng(PicPt.X) - Anchor.X, CLng(PicPt.Y) - Anchor.Y
    
    'if a single cmd was being moved,
    If lstCommands.SelCount = 1 Then
      'update by re-building coordlist, and selecting
      BuildCoordList SelectedCmd
      CodeClick = True
      lstCommands_Click
      'keep highlighting single commands until something else selected
      GetSelectionBounds lstCommands.ListIndex, 1, True
    Else
      'update by redrawing
      DrawPicture
      'reselect commands, then show selection box
      GetSelectionBounds SelectedCmd, lstCommands.SelCount, True
    End If
    
    'restore cursor
    SetCursors pcEdit
      
  End Select
Exit Sub

ErrHandler:

  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub SetMode(ByVal NewMode As EPicMode)

  Dim i As Long
  Dim rtn As Long
  
  On Error GoTo ErrHandler
  
  PicMode = NewMode
 
  'always cancel any drawing operation
  PicDrawMode = doNone
  
  Select Case PicMode
  Case pmEdit
    'disable view movement
    TestDir = 0
    tmrTest.Enabled = False
    
    'if no cmd selected
    If lstCommands.ListIndex = -1 Then
      'select last cmd
      SelectCmd lstCommands.ListCount - 1
    Else
      'reselect current cmd
      CodeClick = True
      lstCommands_Click
    End If
    
    'reset cursor to match selected tool
    Select Case SelectedTool
    Case ttEdit
      'arrow cursor
      SetCursors pcEdit
      
    Case ttFill
      SetCursors pcPaint
    Case ttPlot
      SetCursors pcBrush
    Case Else
      SetCursors pcSelect
    End Select
    
    'set the button
    Toolbar1.Buttons("edit").Value = tbrPressed
    
    'allow selection of coordinates
    lstCoords.Enabled = True
    
  Case pmTest
    '*'Debug.Assert Not TestView Is Nothing
    Toolbar1.Buttons("test").Value = tbrPressed
    
    'if doing anything, cancel it
    If lstCoords.ListIndex <> -1 Then
      lstCoords.ListIndex = -1
    End If
    
    'coordinates can't be selected in test mode
    lstCoords.Enabled = False
    
    'if no cmd selected
    If lstCommands.ListIndex = -1 Then
      'select last cmd
      SelectCmd lstCommands.ListCount - 1
    Else
      'reselect current cmd
      CodeClick = True
      lstCommands_Click
    End If
    
    'update menus
    SetEditMenu
  End Select
  
'  'draw buttons and select tool disabled if not in edit mode
  
  'enable/disable other editing buttons
  '(index of 18 and higher) based on mode, and if drawing is selected
  For i = 18 To 30
    If i < 20 Then
      'select and area select always available in edit mode
      Toolbar1.Buttons(i).Enabled = (PicMode = pmEdit)
    Else
      'other tools only available in edit mode if at least one pen is ON
      Toolbar1.Buttons(i).Enabled = (PicMode = pmEdit) And ((SelectedPen.VisColor < agNone) Or (SelectedPen.PriColor < agNone))
    End If
  Next i
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub AddCelToPic(ByVal NewX As Byte, ByVal NewY As Byte)
  'mask copies the test Image onto the visual screen
  'NewX, NewY is the LOWER left position of the Image
  '
  'also adds the test Image to the priority screen, if visible
  Dim rtn As Long
  Dim i As Long, j As Long, X As Byte, Y As Byte
  Dim StartX As Long, StartY As Long
  Dim EndX As Long, EndY As Long
  Dim CelPri As Long
  Dim PixelPri As Long 'PixelPri is pixel priority, determined by finding non-control line priority closest to this pixel
  Dim PriPixel As Long 'PriPixel is actual pixel Value of priority screen
  Dim CelPixel As Long
  Dim TestCel As AGICel
  Dim blnVis As Boolean, blnPri As Boolean
  
  On Error GoTo ErrHandler
  
  'set testcel
  Set TestCel = TestView.Loops(CurTestLoop).Cels(CurTestCel)
  
  blnVis = picVisual.Visible
  blnPri = picPriority.Visible
  
  'we are using picVisSurface and picPriSurface to temporarily hold bmps
  'while we add the cels; (remember they're offset by 5 pixels in both directions
  'so the temp bitmaps don't show through)
  
  'set priority (if in auto, get priority from current band)
  If TestSettings.ObjPriority < 16 Then
    CelPri = TestSettings.ObjPriority
  Else
    'calculate band incode, for speed
    '    (y - base) / (168 - base) * 10 + 5
    If NewY < PicEdit.PriBase Then
      CelPri = 4
    Else
      CelPri = Int((CLng(NewY) - PicEdit.PriBase) / (168 - PicEdit.PriBase) * 10) + 5
    End If
'*'Debug.Assert CelPri >= 4
  End If

  'clip if any portion of view is off screen
  StartX = 0
  If NewY - (CelHeight - 1) < 0 Then
    StartY = -NewY + CelHeight - 1
  Else
    StartY = 0
  End If
  If NewX + CelWidth > 160 Then
    EndX = 159 - NewX
  Else
    EndX = CelWidth - 1
  End If
  EndY = CelHeight - 1
  
  For i = StartX To EndX
    For j = StartY To EndY
      X = NewX + i
      Y = NewY - (CelHeight - 1) + j
      'get cel pixel color
      CelPixel = TestCelData(i, j)
      'if not a transparent cel
      If CelPixel <> CelTrans Then
        'get pixelpri
        PixelPri = PicEdit.PixelPriority(X, Y)
        PriPixel = PicEdit.PriPixel(X, Y)
        If blnVis Then
          'if priority of cel is equal to or higher than priority of pixel
          If CelPri >= PixelPri Then
            'set this pixel on visual screen
            SetPixelV picVisDraw.hDC, CLng(X), CLng(Y), EGAColor(CelPixel)
          End If
        End If
        If blnPri Then
          If CelPri >= PixelPri And PriPixel >= 3 Then
            'set this pixel on priority screen
            SetPixelV picPriDraw.hDC, CLng(X), CLng(Y), EGAColor(CelPri)
          End If
        End If
      End If
    Next j
  Next i
  
  'save this position as old position
  OldCel.X = NewX
  OldCel.Y = NewY
  
  'if status bar is showing object info
  If StatusSrc And PicMode = pmTest Then
    ' update status bar ONLY if this editor is active
    If frmMDIMain.ActiveForm Is Me Then
      With MainStatusBar
        If .Tag <> CStr(rtPicture) Then
          AdjustMenus rtPicture, InGame, True, IsDirty
        End If
        'use test object position
        .Panels("CurX").Text = "vX: " & CStr(OldCel.X)
        .Panels("CurY").Text = "vY: " & CStr(OldCel.Y)
        .Panels("PriBand").Text = "vBand: " & GetPriBand(OldCel.Y, PicEdit.PriBase)
        SendMessage .hWnd, WM_SETREDRAW, 0, 0
        .Panels("PriBand").Picture = imlPriBand.ListImages(GetPriBand(OldCel.Y, PicEdit.PriBase) - 3).Picture
        SendMessage .hWnd, WM_SETREDRAW, 1, 0
      End With
    End If
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub tbSize_ButtonClick(ByVal Button As MSComctlLib.Button)

  'set plot size
  SelectedPen.PlotSize = Button.Index - 1
  
  'set button face on main toolbar
  Toolbar1.Buttons("size").Image = Button.Image
  
  tbSize.Visible = False
  
  'refresh pens
  RefreshPens
End Sub


Private Sub tbStyle_ButtonClick(ByVal Button As MSComctlLib.Button)

  Select Case Button.Key
  Case "SolidSquare"
    SelectedPen.PlotStyle = psSolid
    SelectedPen.PlotShape = psRectangle
    
  Case "SplatSquare"
    SelectedPen.PlotStyle = psSplatter
    SelectedPen.PlotShape = psRectangle
    
  Case "SolidCircle"
    SelectedPen.PlotStyle = psSolid
    SelectedPen.PlotShape = psCircle
    
  Case "SplatCircle"
    SelectedPen.PlotStyle = psSplatter
    SelectedPen.PlotShape = psCircle
    
  End Select
  
  'change style button face on main toolbar
  Toolbar1.Buttons("style").Image = Button.Image
  
  'hide the style flyout toolbar
  tbStyle.Visible = False
  
  'refresh pens
  RefreshPens
End Sub


Private Sub tmrTest_Timer()
  'timer1 controls test view movement
  
  Dim rtn As Long
    
  Dim ControlLine As AGIColors, OnWater As Boolean
  Dim NewX As Byte, NewY As Byte
  Dim DX As Long, DY As Long
  
  Dim TestCel As AGICel
  
  On Error GoTo ErrHandler
  
  rtn = GetTickCount()
  '*'Debug.Assert CurTestLoopCount <> 0
  
  'if cel not set
  If TestSettings.TestCel = -1 Then
    'increment cel
    CurTestCel = CurTestCel + 1
    'if at loopcount, reset back to zero (there was a bug in version 1.1.22 that used loopcount-1)
    If CurTestCel = CurTestLoopCount Then
      CurTestCel = 0
    End If
    TestCelData() = TestView.Loops(CurTestLoop).Cels(CurTestCel).AllCelData
  End If
  
  Set TestCel = TestView.Loops(CurTestLoop).Cels(CurTestCel)
  
  'set cel height/width/transcolor
  CelWidth = TestCel.Width
  CelHeight = TestCel.Height
  CelTrans = TestCel.TransColor
  
  'use do loop to control flow
  '(need to remember to exit do after setting NewX, NewY, TestDir and StopReason)
  Do
    'assume no movement
    NewX = OldCel.X
    NewY = OldCel.Y
    
    'check for special case of no motion
    If TestDir = odStopped Then
      'cycle in place
      Exit Do
    End If
    
    'calculate dX and dY based on direction
    '(these are empirical formulas based on relationship between direction and change in x/Y)
    DX = Sgn(5 - TestDir) * Sgn(TestDir - 1)
    DY = Sgn(3 - TestDir) * Sgn(TestDir - 7)
  
    'test for edges
    Select Case TestDir
    Case odUp
      'if on horizon and not ignoring,
      If (NewY = TestSettings.Horizon) And Not TestSettings.IgnoreHorizon Then
        'dont go
        StopReason = 7
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'if at top
      If NewY - (CelHeight - 1) <= 0 Then
        'dont go
        StopReason = 8
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'get controlline status
      ControlLine = PicEdit.PixelControl(NewX, NewY - 1, CelWidth)
    
    Case odUpRight
      'if on horizon and not ignoring,
      If (NewY = TestSettings.Horizon) And Not TestSettings.IgnoreHorizon Then
        'dont go
        StopReason = 7
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'if at top
      If NewY - (CelHeight - 1) <= 0 Then
        'dont go
        StopReason = 8
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'if at right edge,
      If NewX + CelWidth - 1 >= 159 Then
        'dont go
        StopReason = 9
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'get controlline status
      ControlLine = PicEdit.PixelControl(NewX + 1, NewY - 1, CelWidth)
      
    Case odRight
      'if at right edge
      If NewX + CelWidth - 1 >= 159 Then
        'dont go
        StopReason = 9
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'get controlline status
      ControlLine = PicEdit.PixelControl(NewX + CelWidth, NewY)
      
    Case odDownRight
      'if at bottom edge
      If NewY = 167 Then
        'dont go
        StopReason = 10
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
        
      'if at right edge
      If NewX + CelWidth - 1 = 159 Then
        'dont go
        StopReason = 9
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'get controlline status
      ControlLine = PicEdit.PixelControl(NewX + 1, NewY + 1, CelWidth)
        
    Case odDown
      'if at bottom
      If NewY = 167 Then
        'dont go
        StopReason = 10
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'get controlline status
      ControlLine = PicEdit.PixelControl(NewX, NewY + 1, CelWidth)
        
    Case odDownLeft
      'if at bottom
      If NewY = 167 Then
        'dont go
        StopReason = 10
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      
      'if at left edge
      If NewX = 0 Then
        'stop motion
        StopReason = 11
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'get controlline status
      ControlLine = PicEdit.PixelControl(NewX - 1, NewY + 1, CelWidth)
        
     
    Case odLeft
      'if at left edge
      If NewX = 0 Then
        'dont go
        StopReason = 11
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      'get controlline status
      ControlLine = PicEdit.PixelControl(NewX - 1, NewY)
        
      
    Case odUpLeft
      'if on horizon or at left edge,
      If ((NewY = TestSettings.Horizon) And Not TestSettings.IgnoreHorizon) Or NewX = 0 Then
        'dont go
        StopReason = 7
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      
      'if at top
      If NewY - (CelHeight - 1) <= 0 Then
        'dont go
        StopReason = 8
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      
      'if at left edge
      If NewX = 0 Then
        'dont go
        StopReason = 11
        TestDir = odStopped
        tmrTest.Enabled = TestSettings.CycleAtRest
        Exit Do
      End If
      
      'get controlline status
      ControlLine = PicEdit.PixelControl(NewX - 1, NewY - 1, CelWidth)
    End Select
    
    'get control line and onwater status
    OnWater = PicEdit.ObjOnWater(NewX + DX, NewY + DY, CelWidth)

    'if at an obstacle line OR (at a conditional obstacle line AND NOT blocking)
    If (ControlLine <= 1) And Not TestSettings.IgnoreBlocks Then
      'don't go
      StopReason = ControlLine + 1
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
      Exit Do
    End If
    
    'if restricting access to land AND at water edge*****no, on water!
    If (TestSettings.ObjRestriction = 2) And OnWater Then '(ControlLine = 3) Then
      'need to go back!
      'don't go
      StopReason = 5
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
      Exit Do
    End If
    
    'if restricting access to water AND at land edge
    If (TestSettings.ObjRestriction = 1) And Not OnWater Then
      'don't go
      StopReason = 6
      TestDir = odStopped
      tmrTest.Enabled = TestSettings.CycleAtRest
      Exit Do
    End If
      
    'ok to move
    NewX = NewX + DX
    NewY = NewY + DY
    
    'if on water, set status
    StopReason = IIf(OnWater, 4, 0)
    
    'if at an alarm line
    If ControlLine = 2 Then
      'stop motion
      StopReason = 3
      TestDir = 0
      tmrTest.Enabled = TestSettings.CycleAtRest
    End If
    'exit to draw the cel
    Exit Do
  Loop
  
  'draw cel
  DrawPicture False, True, NewX, NewY
  
  'manage status bar - if stopped, show reason why
  'if moving, clear status bar
  
  If StopReason <> 0 Then
    'update status bar
    UpdateStatusBar
  Else
    'if testdir is anything but stopped, clear the panel
    If TestDir <> odStopped Then
      If frmMDIMain.ActiveForm Is Me Then
        MainStatusBar.Panels("Tool").Text = vbNullString
      End If
    End If
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub tmrSelect_Timer()
  'cursor and selection timer
  
  'cycle current pixel through all colors
  Static VCColor As AGIColors
  Static CurSize As Single
  
  Dim cOfX As Single, cOfY As Single, cSzX As Single, cSzY As Single
  cOfX = 1.5 / ScaleFactor ^ 0.5
  cOfY = cOfX * 2 '3 / ScaleFactor ^ 0.5
  cSzX = cOfX * 2 '3 / ScaleFactor ^ 0.5
  cSzY = cOfY * 2 '6 / ScaleFactor ^ 0.5
  
  'if selection shape is visible
  If shpVis.Visible Then
    'toggle shape style
    shpVis.BorderStyle = shpVis.BorderStyle + 1
    If shpVis.BorderStyle = 6 Then
      shpVis.BorderStyle = 2
    End If
    shpPri.BorderStyle = shpPri.BorderStyle + 1
    If shpPri.BorderStyle = 6 Then
      shpPri.BorderStyle = 2
    End If
  Else
  
    'toggle cursor
    VCColor = IIf(VCColor < 15, VCColor + 1, 0)
    
    'if using original WinAGI flashing cursor
    If CursorMode = pcmWinAGI Then
      'if cursor is on the selected point
      If OnPoint Then
        'size is always 0
        CurSize = 0
      Else
        CurSize = CurSize + 0.5
        If CurSize > 1 Then
          CurSize = 0
        End If
      End If
      
      'if visual is enabled
      If CurrentPen.VisColor < agNone Then
        picVisual.Line ((CurPt.X - CurSize) * ScaleFactor * 2, (CurPt.Y - CurSize) * ScaleFactor)-Step((2 * CurSize + 1) * ScaleFactor * 2 - 1, ((2 * CurSize + 1) * ScaleFactor - 1)), EGAColor(VCColor), BF
      End If
      
      'if priority is enabled,
      If CurrentPen.PriColor < agNone Then
        picPriority.Line ((CurPt.X - CurSize) * ScaleFactor * 2, (CurPt.Y - CurSize) * ScaleFactor)-Step((2 * CurSize + 1) * ScaleFactor * 2 - 1, ((2 * CurSize + 1) * ScaleFactor - 1)), EGAColor(VCColor), BF
      End If
    Else
      'if using 'x' marks:
      'draw a box
      If CurrentPen.VisColor < agNone Then
        picVisual.Line ((CurPt.X + 0.5 - cOfX / 2) * ScaleFactor * 2, (CurPt.Y + 0.5 + cOfY / 2) * ScaleFactor)-Step((cSzX + 0.15) / 2 * ScaleFactor * 2, (-cSzY - 0.3) / 2 * ScaleFactor), EGAColor(VCColor), B
      End If
      If CurrentPen.PriColor < agNone Then
        picPriority.Line ((CurPt.X + 0.5 - cOfX / 2) * ScaleFactor * 2, (CurPt.Y + 0.5 + cOfY / 2) * ScaleFactor)-Step((cSzX + 0.15) / 2 * ScaleFactor * 2, (-cSzY - 0.3) / 2 * ScaleFactor), EGAColor(VCColor), B
      End If
    End If
  End If
End Sub

Private Sub Toolbar1_ButtonClick(ByVal Button As MSComctlLib.Button)

  Dim i As Long
  Dim SelAnchor As PT
  Dim blnCursor As Boolean, blnClearCmdList As Boolean
  Dim PrevTool As TPicToolTypeEnum
  
'     button parameters:
'  Index  Tip               Key
'    1    Undo              undo
'    2    separator
'    3    Edit              edit
'    4    Test              test
'    5    separator
'    6    Set Bkgd          bkgd
'    7    Enable Full Draw  full
'    8    Zoom In           zoomin
'    9    Zoom Out          zoomout
'   10    separator
'   11    Cut               cut
'   12    Copy              copy
'   13    Paste             paste
'   14    Delete            delete
'   15    Flip Horizontal   fliph
'   16    Flip Vertical     flipv
'   17    separator
'   18    Edit Select       editsel
'   19    Select            select
'   20    Abs Line          absline
'   21    Rel Line          relline
'   22    Corner            corner
'   23    Rectangle         rectangle
'   24    Trapezoid         floor
'   25    Ellipse           ellipse
'   26    Fill              fill
'   27    Plot              plot
'   28    separator
'   29    Style             style
'   30    Size              size
  
  'ensure flyout toolbars are hidden
  tbSize.Visible = False
  tbStyle.Visible = False
  
  'what's rhe tool before we change?
  PrevTool = SelectedTool
  
  If Not blnWheel Then
    'if drawing
    If PicDrawMode <> doNone Then
      StopDrawing
    End If
  
    'if current tool is Edit-Area then ALWAYS reset selection
    'whenever a toolbar button other than zoomin or zoomout is pressed
    If SelectedTool = ttSelectArea And (Button.Index <> 8 And Button.Index <> 9) Then
      'reset selection
      SelStart.X = 0
      SelStart.Y = 0
      SelSize.X = 0
      SelSize.Y = 0
      ShowCmdSelection
    End If
  End If
  
  Select Case Button.Key
  Case "undo"
    MenuClickUndo
    Exit Sub
    
  Case "edit" 'Edit Mode button
    'enable editing
    SetMode pmEdit
  
  Case "test" 'Test mode button
    'if no test view
    If TestView Is Nothing Then
      'get one
      GetTestView
    End If
    
    'if still no view
    If TestView Is Nothing Then
      'error or user changed mind
      PicMode = pmEdit
      Toolbar1.Buttons("edit").Value = tbrPressed
    Else
      'enable testing
      SetMode pmTest
    End If
    
    
  Case "select" 'Select Command tool
    SelectedTool = ttEdit
    'normal cursor
    SetCursors pcEdit
    'reset selection
    SelStart.X = 0
    SelStart.Y = 0
    SelSize.X = 0
    SelSize.Y = 0
    ShowCmdSelection
    'reset menus
    SetEditMenu
    blnClearCmdList = True
  
  Case "editsel" 'Select Area tool
    SelectedTool = ttSelectArea
    'area select cursor
    SetCursors pcEditSel
    
    'if current point is highlighted,
    If tmrSelect.Enabled Then
      'in original mode, need to reset area under cursor
      If CursorMode = pcmWinAGI Then
        'reset area under cursor
        BitBlt picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, 6 * ScaleFactor, 3 * ScaleFactor, Me.hDC, 0, 0, SRCCOPY
        BitBlt picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, 6 * ScaleFactor, 3 * ScaleFactor, Me.hDC, 0, 12, SRCCOPY
        picVisual.Refresh
        picPriority.Refresh
      End If
      'then disable cursor highlighting
      tmrSelect.Enabled = False
    End If
    
    'reset selection
    SelStart.X = 0
    SelStart.Y = 0
    SelSize.X = 0
    SelSize.Y = 0
    ShowCmdSelection
    'reset menus
    SetEditMenu
    blnClearCmdList = True
    
  Case "absline" 'Draw Line tool
    SelectedTool = ttLine
    SetCursors pcSelect
    blnClearCmdList = True
    
  Case "relline" 'Draw Relative Line tool
    SelectedTool = ttRelLine
    SetCursors pcSelect
    blnClearCmdList = True
    
  Case "corner" 'Draw Corner Line tool
    SelectedTool = ttCorner
    SetCursors pcSelect
    blnClearCmdList = True
    
  Case "rectangle" 'Draw Rectangle tool
    SelectedTool = ttRectangle
    SetCursors pcSelect
    blnClearCmdList = True
    
  Case "floor" 'Draw Trapezoid tool
    SelectedTool = ttTrapezoid
    SetCursors pcSelect
    blnClearCmdList = True
    
  Case "ellipse" 'Draw Ellipse tool
    SelectedTool = ttEllipse
    SetCursors pcSelect
    blnClearCmdList = True
    
  Case "fill" 'Fill tool
    SelectedTool = ttFill
    SetCursors pcPaint
    blnClearCmdList = True
    
  Case "plot" 'Plot Pen tool
    SelectedTool = ttPlot
    'should select a cursor that matches current brush style, shape, size
    SetCursors pcBrush
    blnClearCmdList = True
    
  Case "style" 'Change Pen Style
    'position toolbar next to its main toolbar button
    tbStyle.Left = Button.Left + Toolbar1.Left + Button.Width
    'if not enough room below the main button,
    If Button.Top - tbStyle.Buttons(1).Top + tbStyle.Height > picPalette.Top Then
      'put it as far down as possible
      tbStyle.Top = CalcHeight - picPalette.Height - tbStyle.Height
    Else
      tbStyle.Top = Button.Top - tbStyle.Buttons(1).Top
    End If
    'show it
    tbStyle.Visible = True
    Exit Sub
    
  Case "size" 'Change Pen Size
    'position toolbar next to its main toolbar button
    tbSize.Left = Button.Left + Button.Width + Toolbar1.Left
    'if not enough room below the main button,
    If Button.Top - tbSize.Buttons(1).Top + tbSize.Height > picPalette.Top Then
      'put it as far down as possible
      tbSize.Top = CalcHeight - picPalette.Height - tbSize.Height
    Else
      tbSize.Top = Button.Top - tbSize.Buttons(1).Top
    End If
    'show it
    tbSize.Visible = True
    Exit Sub
    
  Case "zoomin" 'Increase Scale
    'adjust scale size
    ScaleFactor = ScaleFactor + 1
    If ScaleFactor > 6 Then
      ScaleFactor = 6
      'don't need to redraw if scale not changed
    Else
      'redraw at new scale
      ResizePictures
    End If
    
  Case "zoomout" 'Decrease Scale
    'adjust scale
    ScaleFactor = ScaleFactor - 1
    If ScaleFactor = 0 Then
      ScaleFactor = 1
      'don't need to redraw if scale not changed
    Else
      'redraw at new scale
      ResizePictures
    End If
  
  Case "bkgd" 'Toggle Background Image
    'turn background on or off
    ToggleBkgd Not PicEdit.BkgdShow
    
    'if current command has coordinates, do more than just redraw picture
    If lstCoords.ListCount > 0 Then
      If lstCoords.ListIndex <> -1 Then
        'use coordinate click method if a coordinate is currently selected
        lstCoords_Click
      Else
        'use command click method if no coordinates selected
        CodeClick = True
        lstCommands_Click
      End If
    Else
      'if selected command doesn't have any coordinates
      'redrawing is sufficient to set correct state of editor
      DrawPicture
    End If
    
  Case "full" 'Toggle StepDraw
    'set flag to show full picture or individual steps depending on button status
    PicEdit.StepDraw = (Button.Value = tbrUnpressed)
    
    'if current command has coordinates, do more than just redraw picture
    If lstCoords.ListCount > 0 Then
      If lstCoords.ListIndex <> -1 Then
        'use coordinate click method if a coordinate is currently selected
        lstCoords_Click
      Else
        'use command click method if no coordinates selected
        CodeClick = True
        lstCommands_Click
      End If
    Else
      'if selected command doesn't have any coordinates
      'redrawing is sufficient to set correct state of editor
      DrawPicture
    End If
    
  Case "cut" 'Edit-Cut
    MenuClickCut
    Exit Sub
    
  Case "copy" 'Edit-Copy
    MenuClickCopy
    Exit Sub
    
  Case "paste" 'Edit-Paste
    MenuClickPaste
    Exit Sub
    
  Case "delete" 'Edit-Delete
    MenuClickDelete
    Exit Sub
    
  Case "fliph" 'Edit-Flip Horizontal
    FlipCmds SelectedCmd, lstCommands.SelCount, 0
    'redraw
    DrawPicture
    'if only one cmd,
    If lstCoords.ListCount = 1 Then
      'rebuld coord list
      BuildCoordList SelectedCmd
    End If
    
  Case "flipv" 'Edit-Flip Vertical
    FlipCmds SelectedCmd, lstCommands.SelCount, 1
    DrawPicture
    'if only one cmd,
    If lstCoords.ListCount = 1 Then
      'rebuld coord list
      BuildCoordList SelectedCmd
    End If
    
  End Select
  
  '*'Debug.Assert MainStatusBar.Tag = rtPicture
  If MainStatusBar.Tag <> rtPicture Then
    'show picture menus, and enable editing
    AdjustMenus rtPicture, InGame, True, IsDirty
  End If
  
  'show/hide anchor and block status panels
  MainStatusBar.Panels("Anchor").Visible = (SelectedTool = ttSelectArea)
  If (SelectedTool = ttSelectArea) Then
    MainStatusBar.Panels("Anchor").Text = "Anchor:"
  End If
  MainStatusBar.Panels("Block").Visible = (SelectedTool = ttSelectArea)
  If (SelectedTool = ttSelectArea) Then
    MainStatusBar.Panels("Block").Text = "Block:"
  End If
  
  'if a tool was selected OR if mode changed from edit to draw
  'need to clear command list
  If blnClearCmdList Then
    'if multiple selections
    If lstCommands.SelCount > 1 Then
      'use selectcmd method to force single selection
      SelectCmd lstCommands.ListIndex
      'reset selection
      SelStart.X = 0
      SelStart.Y = 0
      SelSize.X = 0
      SelSize.Y = 0
      ShowCmdSelection
      
    'if a coord is selected
    ElseIf lstCoords.ListIndex <> -1 Then
      'unselect it
      lstCoords.ListIndex = -1
      CurPt.X = 255
      CurPt.Y = 255
      lstCommands.SetFocus
    'if using 'x' marker cursor mode, still
    'need to enable/disable the'x's based on selected tool
    ElseIf CursorMode = pcmXMode Then
      If SelectedTool = ttEdit Then
        'only if in edit mode
        If PicMode = pmEdit Then
          HighlightCoords
        End If
      Else
        'if previous tool was 'none', then
        'cursor x's need to be hidden
        If PrevTool = ttEdit Then
          'draw picture to eliminate cursor
          DrawPicture
        End If
      End If
    End If
  End If
  
  'if a cmd is selected and no selection window is visible
  If lstCommands.ListIndex <> -1 And (SelSize.X = 0 Or SelSize.Y = 0) Then
    'is cursor flashing?
    blnCursor = tmrSelect.Enabled

    'enable cursor highlighting if edit tool selected
    tmrSelect.Enabled = (SelectedTool = ttEdit) And lstCoords.ListIndex <> -1
    If tmrSelect.Enabled And CursorMode = pcmWinAGI Then
      'save area under cursor
      BitBlt Me.hDC, 0, 0, 6 * ScaleFactor, 3 * ScaleFactor, picVisual.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
      BitBlt Me.hDC, 0, 12, 6 * ScaleFactor, 3 * ScaleFactor, picPriority.hDC, (CurPt.X - 1) * ScaleFactor * 2, (CurPt.Y - 1) * ScaleFactor, SRCCOPY
    End If

    'if cursor was enabled, but isn't now,
    If blnCursor And Not tmrSelect.Enabled Then
      'draw picture to eliminate cursor
      DrawPicture
    End If
  Else
    'if no selection, turn off timer
    If SelSize.X = 0 Or SelSize.Y = 0 Then
      tmrSelect.Enabled = False
    End If
  End If
  
  UpdateToolBar
End Sub

Private Sub Toolbar1_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  'hide flyout toolbars

  tbStyle.Visible = False
  tbSize.Visible = False
End Sub

Private Sub vsbPri_Change()

  'reposition priority to match scrollbar Value
  picPriority.Top = IIf(picPriority.Top > PE_MARGIN, PE_MARGIN, -vsbPri.Value)
  
  'if room for entire picture
  If (picPriSurface.Top + picPriority.Height + 2 * PE_MARGIN <= (CalcHeight - picPalette.Height + hsbPri.Visible * hsbPri.Height)) Then
    'reset to margin
    picPriority.Top = PE_MARGIN
  End If

  'if back at starting position
  If picPriority.Top = PE_MARGIN Then
    'show/hide vertical scrollbar
    vsbPri.Visible = (picPriSurface.Top + picPriority.Height + 2 * PE_MARGIN > (CalcHeight - picPalette.Height + hsbPri.Visible * hsbPri.Height))
    picPriSurface.Width = CalcWidth - picPriSurface.Left + (vsbPri.Visible * vsbPri.Width)
  End If
  
End Sub

Private Sub vsbPri_GotFocus()
  'ensure flyout toolbars hidden
  tbStyle.Visible = False

  tbSize.Visible = False
  

  'set focus to picture
  picPriority.SetFocus

End Sub


Private Sub vsbPri_Scroll()

  vsbPri_Change
'  picPriority.Top = IIf(picPriority.Top > PE_MARGIN, PE_MARGIN, -vsbPri.Value)
End Sub

Private Sub vsbVis_Change()
  
  'reposition visual to match scrollbar Value
  picVisual.Top = IIf(picVisual.Top > PE_MARGIN, PE_MARGIN, -vsbVis.Value)
  
  'if room for entire picture
  If (picVisSurface.Top + picVisual.Height + 2 * PE_MARGIN <= picSplitH.Top + hsbVis.Visible * hsbVis.Height) Then
    'reset to margin
    picVisual.Top = PE_MARGIN
  End If

  'if picture is in normal position,
  If picVisual.Top = PE_MARGIN Then
    'show/hide vertical scrollbar if it is needed
    vsbVis.Visible = (picVisSurface.Top + picVisual.Height + 2 * PE_MARGIN > picSplitH.Top + hsbVis.Visible * hsbVis.Height)
    'ensure picsurface is sized to account for scrollbar
    picVisSurface.Width = CalcWidth - picVisSurface.Left + (vsbVis.Visible * vsbVis.Width)
  End If

End Sub

Private Sub vsbVis_GotFocus()
  'ensure flyout toolbars hidden
  tbStyle.Visible = False

  tbSize.Visible = False
  
  
  'set focus to picture
  picVisual.SetFocus
  
End Sub


Private Sub vsbVis_Scroll()
  
  vsbVis_Change
End Sub


      */
    }
  } 
}
