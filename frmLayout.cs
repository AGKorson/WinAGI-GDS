using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinAGI.Editor
{
    public partial class frmLayout : Form {

        // other variables
        public bool IsDirty = false;

        public frmLayout() {
            InitializeComponent();
        }

        internal void SelectRoom(int selResNum) {
            throw new NotImplementedException();
        }
        public void DrawLayout(bool DrawSel = true) {
            /*
        Dim rtn As Long
        Dim i As Long, j As Long
        Dim lngRoom As Long, LineColor As Long
        Dim v(2) As POINTAPI, strID As String
        Dim sngPW As Single, sngPH As Single
        Dim sngTextH As Single, sngTextW As Single

        On Error GoTo ErrHandler

        'if drawing temporarily disabled (due to complex add/update methods)
        'just exit; eventually, calling functions will be done, will re-enable
        'drawing and then call this method again

        If blnDontDraw Then
          Exit Sub
        End If

        'if no print scale,
        '(meaning form load is not done yet)
        If PrintScale = 0 Then
          Exit Sub
        End If

      #If DEBUGMODE <> 1 Then
        'disable updating
        rtn = SendMessage(picDraw.hWnd, WM_SETREDRAW, 0, 0)
      #End If

        With picDraw
          .Cls

          'if drawing page boundaries
          If Settings.LEPages And PrintScale <> 0 Then
            .DrawWidth = 1
            .DrawStyle = vbDot
            .ForeColor = RGB(128, 128, 128)

            'page width/height
            sngPW = (Printer.Width / 1440 - 1.5) / PrintScale
            sngPH = (Printer.Height / 1440 - 1.5) / PrintScale

            'on startup, or if no objects, could have sngPW and/or sngPH = 0
            'so need to check before continuing
            If sngPW > 0 Then
              'get position of first vertical line that would occur after current offset position
              i = Int((MinX + OffsetX) / sngPW)
              'add vertical lines, until past edge of drawing surface
              Do Until (MinX + OffsetX + i * sngPW) * DSF > .ScaleWidth
                picDraw.Line ((MinX + OffsetX + i * sngPW) * DSF, 0)-Step(0, .ScaleHeight)
                i = i + 1
              Loop
            End If

            If sngPH > 0 Then
            j = Int((MinY + OffsetY) / sngPH) ' + 1
              'add horizontal lines until past bottom edge of drawing surface
              Do Until (MinY + OffsetY + j * sngPH) * DSF > .ScaleHeight
                picDraw.Line (0, (MinY + OffsetY + j * sngPH) * DSF)-Step(.ScaleWidth, 0)
                j = j + 1
              Loop
            End If
          End If
          'restore draw properties
          .DrawWidth = 2
          .DrawStyle = vbSolid

          'then add objects
          For i = 0 To ObjCount - 1
            Select Case ObjOrder(i).Type
            Case lsRoom
              lngRoom = ObjOrder(i).Number

              'in unlikely event the object is not visible, just skip it
              If Room(lngRoom).Visible Then
                'if object is on screen
                If ObjOnScreen(ObjOrder(i)) Then
                  'draw the box
                  .FillColor = Settings.LEColors.Room.Fill
                  .ForeColor = Settings.LEColors.Room.Edge
                  'room size is RM_SIZE units in object scale converted to pixels
                  picDraw.Line ((Room(lngRoom).Loc.X + OffsetX) * DSF, (Room(lngRoom).Loc.Y + OffsetY) * DSF)-Step(RM_SIZE * DSF, RM_SIZE * DSF), Settings.LEColors.Room.Edge, B
                  'if showing pic for this room, draw the matching vis pic
                  If Room(lngRoom).ShowPic Then
                    DrawRoomPic lngRoom, OffsetX, OffsetY
                  End If

                  strID = ResourceName(Logics(lngRoom), True, True)
                  sngTextW = .TextWidth(strID)
                  sngTextH = .TextHeight(strID)
                  If sngTextW <= RM_SIZE * DSF Then
                    .CurrentX = ((Room(lngRoom).Loc.X + OffsetX) + RM_SIZE / 2) * DSF - sngTextW / 2
                    .CurrentY = ((Room(lngRoom).Loc.Y + OffsetY) + 0.7) * DSF - 3 * sngTextH / 2
                    picDraw.Print strID
                  Else
                    'if logic id is too long, it won't fit in the box; split across two lines
                    'print first line
                    j = 0
                    Do Until sngTextW <= RM_SIZE * DSF
                      strID = Left$(strID, Len(strID) - 1)
                      sngTextW = .TextWidth(strID)
                      j = j + 1
                    Loop
                    sngTextH = .TextHeight(strID)
                    .CurrentX = ((Room(lngRoom).Loc.X + OffsetX) + RM_SIZE / 2) * DSF - sngTextW / 2
                    .CurrentY = ((Room(lngRoom).Loc.Y + OffsetY) + 0.7) * DSF - 3 * sngTextH / 2
                    picDraw.Print strID
                    'now print second line (at same x position)
                    .CurrentX = ((Room(lngRoom).Loc.X + OffsetX) + RM_SIZE / 2) * DSF - sngTextW / 2
                    strID = Right(ResourceName(Logics(lngRoom), True, True), j)
                    sngTextW = .TextWidth(strID)
                    Do Until sngTextW <= RM_SIZE * DSF
                      strID = Left$(strID, Len(strID) - 1)
                      sngTextW = .TextWidth(strID)
                    Loop
                    .CurrentY = ((Room(lngRoom).Loc.Y + OffsetY) + 0.7) * DSF - sngTextH / 2
                    picDraw.Print strID
                  End If
                End If

                'draw exits
                For j = 0 To Exits(lngRoom).Count - 1
                  'skip any deleted exits
                  If Exits(lngRoom)(j).Status <> esDeleted Then
                    'determine color
                    If Exits(lngRoom)(j).Status = esHidden Then
                      'use a dashed, gray line (stupid graphics rules in windows
                      '                         says width has to be 1 to get dashed lines)
                      .DrawStyle = vbDot
                      .DrawWidth = 1
                      LineColor = RGB(160, 160, 160)
      '''  can't get the APIs to work in order to draw a wider dotted line

      '''              Dim hPen As Long, lBrush As LOGBRUSH, pStyle As Long
      '''              pStyle = PS_GEOMETRIC Or PS_SOLID Or PS_DOT Or PS_ENDCAP_FLAT Or PS_JOIN_BEVEL
      '''              pStyle = PS_COSMETIC Or PS_DOT ' Or PS_ENDCAP_FLAT Or PS_JOIN_BEVEL
      '''                'PS_DASH
      '''              With lBrush
      '''                .lbStyle = BS_SOLID
      '''                .lbColor = 0
      '''              End With
      '''              hPen = ExtCreatePen(pStyle, 5, lBrush, 0, 0)
      '''              If hPen <> 0 Then
      '''                SelectObject picDraw.hDC, hPen
      '''              End If
      '''
      '''              LineTo picDraw.hDC, 100, 100
                    Else
                      Select Case Exits(lngRoom)(j).Reason
                      Case erOther
                        LineColor = Settings.LEColors.Other
                      Case Else
                        LineColor = Settings.LEColors.Edge
                      End Select
                      .DrawStyle = vbNormal
                      .DrawWidth = 2
                    End If
                    .ForeColor = LineColor
                    .FillColor = LineColor

                    'if there is a transfer pt
                    If Exits(lngRoom)(j).Transfer > 0 Then
                    '*'Debug.Assert Exits(lngRoom)(j).Status <> esHidden
                      'is this first leg?
                      If Exits(lngRoom)(j).Leg = 0 Then
                        'draw first segment
                        If LineOnScreen(Exits(lngRoom)(j).SPX, Exits(lngRoom)(j).SPY, TransPt(Exits(lngRoom)(j).Transfer).SP.X, TransPt(Exits(lngRoom)(j).Transfer).SP.Y) Then
                          MoveToEx .hDC, (Exits(lngRoom)(j).SPX + OffsetX) * DSF, (Exits(lngRoom)(j).SPY + OffsetY) * DSF, 0
                          LineTo .hDC, (TransPt(Exits(lngRoom)(j).Transfer).SP.X + OffsetX) * DSF, (TransPt(Exits(lngRoom)(j).Transfer).SP.Y + OffsetY) * DSF
                          DrawArrow Exits(lngRoom)(j).SPX, Exits(lngRoom)(j).SPY, TransPt(Exits(lngRoom)(j).Transfer).SP.X, TransPt(Exits(lngRoom)(j).Transfer).SP.Y, LineColor
                        End If
                        'draw second segment
                        If LineOnScreen(TransPt(Exits(lngRoom)(j).Transfer).EP.X, TransPt(Exits(lngRoom)(j).Transfer).EP.Y, Exits(lngRoom)(j).EPX, Exits(lngRoom)(j).EPY) Then
                          MoveToEx .hDC, (TransPt(Exits(lngRoom)(j).Transfer).EP.X + OffsetX) * DSF, (TransPt(Exits(lngRoom)(j).Transfer).EP.Y + OffsetY) * DSF, 0
                          LineTo .hDC, (Exits(lngRoom)(j).EPX + OffsetX) * DSF, (Exits(lngRoom)(j).EPY + OffsetY) * DSF
                          DrawArrow TransPt(Exits(lngRoom)(j).Transfer).EP.X, TransPt(Exits(lngRoom)(j).Transfer).EP.Y, Exits(lngRoom)(j).EPX, Exits(lngRoom)(j).EPY, LineColor
                        End If
                      Else
                        'just draw the arrows- the lines are alreay drawn
                        If LineOnScreen(Exits(lngRoom)(j).SPX, Exits(lngRoom)(j).SPY, TransPt(Exits(lngRoom)(j).Transfer).EP.X, TransPt(Exits(lngRoom)(j).Transfer).EP.Y) Then
                          DrawArrow Exits(lngRoom)(j).SPX, Exits(lngRoom)(j).SPY, TransPt(Exits(lngRoom)(j).Transfer).EP.X, TransPt(Exits(lngRoom)(j).Transfer).EP.Y, LineColor
                        End If
                        If LineOnScreen(TransPt(Exits(lngRoom)(j).Transfer).SP.X, TransPt(Exits(lngRoom)(j).Transfer).SP.Y, Exits(lngRoom)(j).EPX, Exits(lngRoom)(j).EPY) Then
                          DrawArrow TransPt(Exits(lngRoom)(j).Transfer).SP.X, TransPt(Exits(lngRoom)(j).Transfer).SP.Y, Exits(lngRoom)(j).EPX, Exits(lngRoom)(j).EPY, LineColor
                        End If
                      End If
                    Else
                      'draw the exit line
                      If LineOnScreen(Exits(lngRoom)(j).SPX, Exits(lngRoom)(j).SPY, Exits(lngRoom)(j).EPX, Exits(lngRoom)(j).EPY) Then
                        MoveToEx .hDC, (Exits(lngRoom)(j).SPX + OffsetX) * DSF, (Exits(lngRoom)(j).SPY + OffsetY) * DSF, 0
                        LineTo .hDC, (Exits(lngRoom)(j).EPX + OffsetX) * DSF, (Exits(lngRoom)(j).EPY + OffsetY) * DSF
                        DrawArrow Exits(lngRoom)(j).SPX, Exits(lngRoom)(j).SPY, Exits(lngRoom)(j).EPX, Exits(lngRoom)(j).EPY, LineColor
                      End If
                    End If
                  End If
                Next j
              End If

            Case lsTransPt
              If TransPt(ObjOrder(i).Number).Count > 0 Then
                If ObjOnScreen(ObjOrder(i)) Then
                  .FillColor = Settings.LEColors.TransPt.Fill
                  .ForeColor = Settings.LEColors.TransPt.Edge
                  sngTextH = .TextHeight(CStr(ObjOrder(i).Number))
                  sngTextW = .TextWidth(CStr(ObjOrder(i).Number))
                  'draw transfer circles
                  picDraw.Circle ((TransPt(ObjOrder(i).Number).Loc(0).X + RM_SIZE / 4 + OffsetX) * DSF, (TransPt(ObjOrder(i).Number).Loc(0).Y + RM_SIZE / 4 + OffsetY) * DSF), RM_SIZE / 4 * DSF, Settings.LEColors.TransPt.Edge
                  .CurrentX = (TransPt(ObjOrder(i).Number).Loc(0).X + RM_SIZE / 4 + OffsetX) * DSF - sngTextW / 2
                  .CurrentY = (TransPt(ObjOrder(i).Number).Loc(0).Y + RM_SIZE / 4 + OffsetY) * DSF - sngTextH / 2
                  .FontBold = True
                  picDraw.Print CStr(ObjOrder(i).Number)
                  .FontBold = False
                End If
                If ObjOnScreen(ObjOrder(i), True) Then
                  .FillColor = Settings.LEColors.TransPt.Fill
                  .ForeColor = Settings.LEColors.TransPt.Edge
                  sngTextH = .TextHeight(CStr(ObjOrder(i).Number))
                  sngTextW = .TextWidth(CStr(ObjOrder(i).Number))
                  picDraw.Circle ((TransPt(ObjOrder(i).Number).Loc(1).X + RM_SIZE / 4 + OffsetX) * DSF, (TransPt(ObjOrder(i).Number).Loc(1).Y + RM_SIZE / 4 + OffsetY) * DSF), RM_SIZE / 4 * DSF, Settings.LEColors.TransPt.Edge
                  .CurrentX = (TransPt(ObjOrder(i).Number).Loc(1).X + RM_SIZE / 4 + OffsetX) * DSF - sngTextW / 2
                  .CurrentY = (TransPt(ObjOrder(i).Number).Loc(1).Y + RM_SIZE / 4 + OffsetY) * DSF - sngTextH / 2
                  .FontBold = True
                  picDraw.Print CStr(ObjOrder(i).Number)
                  .FontBold = False
                End If
              End If
            Case lsErrPt
              If ErrPt(ObjOrder(i).Number).Visible Then
                If ObjOnScreen(ObjOrder(i)) Then
                  .FillColor = Settings.LEColors.ErrPt.Fill
                  .ForeColor = Settings.LEColors.ErrPt.Edge
                  'use polygon drawing function
                  v(0).X = (ErrPt(ObjOrder(i).Number).Loc.X + OffsetX) * DSF
                  v(0).Y = (ErrPt(ObjOrder(i).Number).Loc.Y + OffsetY) * DSF
                  v(1).X = (ErrPt(ObjOrder(i).Number).Loc.X + 0.6 + OffsetX) * DSF
                  v(1).Y = (ErrPt(ObjOrder(i).Number).Loc.Y + OffsetY) * DSF
                  v(2).X = (ErrPt(ObjOrder(i).Number).Loc.X + 0.3 + OffsetX) * DSF
                  v(2).Y = (ErrPt(ObjOrder(i).Number).Loc.Y + RM_SIZE / 2 + OffsetY) * DSF

                  rtn = Polygon(.hDC, v(0), 3)
                  .Refresh
                End If
              End If
            Case lsComment
              If Comment(ObjOrder(i).Number).Visible Then
                If ObjOnScreen(ObjOrder(i)) Then
                  'draw comment box
                  DrawCmtBox ObjOrder(i).Number
                End If
              End If
            End Select
          Next i

          'just in case, make sure drawwidth is reset
          .DrawWidth = 2

          If DrawSel Then
            Select Case Selection.Type
            Case lsNone
              'dont select anything
            Case lsExit
              'reselect exit
              SelectExit Selection
            Case Else
              'reselect object
              SelectObj Selection
            End Select
          End If

        #If DEBUGMODE <> 1 Then
          'reenable updating
          rtn = SendMessage(picDraw.hWnd, WM_SETREDRAW, 1, 0)
        #End If

          .Refresh
        End With
      Exit Sub

      ErrHandler:
        Resume Next
            */
        }

        public void MenuClickSave() {
            //SaveLayout();
        }

    void tmplayoutform()
    {
      /*

  'layout editor types
  Private Type ELInfo    'used when extracting layouts from logics
    Analyzed As Boolean    '
    Placed As Boolean
    Group As Byte
    Exits(3) As Byte
    Enter(3) As Byte
  End Type
  
  Private Type RoomInfo     'ROOM OBJECT
    Loc As LCoord           'location of room on layout
    Visible As Boolean      'if TRUE, room will be drawn on layout
    Order As Long           'order in which object is drawn
    ShowPic As Boolean      'if TRUE, room box will include scaled img of visual pic
  End Type
  
  Private Type ErrPtInfo    'ERROR POINT OBJECT (when a room exit points to non-existent room)
    Loc As LCoord           'location of error point on layout
    Visible As Boolean      'if TRUE, error object will be drawn on layout
    Order As Long           'order in which object is drawn
    Room As Integer         'the number of the 'non-existent' room or -1 if invalid room number (i.e. a misspelled logicID)
    ExitID As String        'ID of the exit connected TO this err point
    FromRoom As Integer     'Room number where the exit error is located (the bad new.room() command)
  End Type
                            'NOTE: no exits can ever originate from an error point
                            'and only one exit can point to a single error point
  
  Private Type CommentInfo  'COMMENT BOX OBJECT
    Loc As LCoord           'location of comment box on layout
    Visible As Boolean      'if TRUE, comment box will be drawn on layout
    Order As Long           'order in which object is drawn
    Size As LCoord          'height/width of comment box
    Text As String          'text of comment
  End Type
  
  Private Type TransInfo    'TRANSFER POINT OBJECT (consists of pair of objects that 'transfer' an exit)
    Loc(1) As LCoord        'location of each transfer point on layout
    Count As Integer        '1 = breaks a 'one-way' exit; 2 = breaks a 'reciprocal' exit
    Order As Long           'order in which transfer objects will be drawn on layout
    EP As LCoord            'endp point coords
    SP As LCoord            'start point coords
    Room(1) As Byte         'originating room(s) (if two way, each room has to be noted)
    ExitID(1)  As String    'exit ID(s) that transfer breaks
  End Type
  
  Private Type ObjInfo
    Number As Long          'number is negative if referring to leg2 of a transfer point
    Type As ELSelection
  End Type
  
  Private Type TSel
    Type As ELSelection   'Type of object currently selected
    Number As Long        ' index of object selected
                          ' or room associated with selected exit
    ExitID As String      ' ID of selected exit
    Leg As ELLeg          ' which leg of an exit that has transfer
    Point As Long         ' 0 means starting point of an exit is being moved
                          ' 1 means ending point of an exit is being moved
    TwoWay As ELTwoWay
    X1 As Single ' Long            '
    Y1 As Single ' Long            'coordinates used to draw handles around selection
    X2 As Single ' Long            '
    Y2 As Single ' Long            ' in drawing surface pixel scale
    X3 As Single ' Long
    Y3 As Single ' Long
  End Type
  
  'layout object variables
  Private Room(255) As RoomInfo         'rectangles
  Private TransPt(255) As TransInfo     'circles (always drawn in pairs)
  Private ErrPt(255) As ErrPtInfo       'triangles
  Private Comment(255) As CommentInfo   'rounded corner rectangles
  
  'exits - a collection of AGIExit objects
  Private Exits(255) As AGIExits
    'AGI Exit object has these properties:
      'ID As String                  'matches id number as stored in logic source code comment
      'Public Room As Integer        '0 = no valid room defined; 1-255 = new room number
      'Public Reason As EEReason     'erHorizon, erRight, erBottom, erLeft, erOther
      'Public Style As Integer       '0 = simple exit; 1 = complex exit
      '                              'simple means the new.room cmd immediately follows
      '                              'an 'if-then statement; complex means other commands
      '                              'are present, or the if-then statement is not easily discerned
      'Public Transfer As Integer    'identifies transfer points or error points
      '                              'if number <0 it is an error point
      '                              'if number >0 it is a transfer point
      '                              '0 means no transfer
      '                              'only valid in layout editor
      'Public Leg As Integer         'identifies which leg of a transferpoint is associated
      '                              'with this exit
      'Public Status As EEStatus     'esNew means new exit, not currently in source code
      '                              'esOK means existing exit already in source code that is ok
      '                              'esDeleted means existing exit already in source code to be deleted
      '                              'esChanged means existing edit already in source code to be changed
      'Public SPX As Single          'used by layout editor to locate start and endpoints of exits
      'Public SPY As Single          'to be drawn
      'Public EPX As Single
      'Public EPY As Single
  
  'to track the custom mouse icon, use a seperate enum
  Private Enum ccMousePtr
    ccNone
    ccMoveSel
    ccSelObj
    ccHorizon
    ccBottom
    ccRight
    ccLeft
    ccOther
    ccAddObj
  End Enum
  
  'extraction variables
  Private ELRoom(255) As ELInfo
  
  'scale and drawing variables
  Private DrawScale As Long
  Private OffsetX As Single, OffsetY As Single
  Private MinX As Single, MinY As Single
  Private MaxX As Single, MaxY As Single
  Private ObjOrder(1023) As ObjInfo  'display order of objects
  Private ObjCount As Long
  Private DSF As Single
  Private PrintScale As Single
  
  'other variables
  Private blnDontDraw As Boolean, blnLoadingLayout As Boolean
  Private AddPicToo As CheckBoxConstants
  Private CodeChange As Boolean
  
  'selection and moving variables
  Private SelTool As ELayoutTool, Selection As TSel
  Private HoldTool As Boolean
  Private SelectedObjects() As ObjInfo
  Private mX As Single, mY As Single, mDX As Single, mDY As Single
  Private OldX As Single, OldY As Single
  Private AnchorX As Single, AnchorY As Single
  Private MoveExit As Boolean, MoveObj As Boolean
  Private DrawExit As Long, DragSelect As Boolean
  Private NewExitReason As EEReason
  Private NewExitTrans As Long, NewExitRoom As Long
  Private SizingComment As Long
  Private Const agOffWhite As Long = vbWhite - 1
  Private CustMousePtr As ccMousePtr
  Private CtrlDown As Boolean, DragCanvas As Boolean
  
  Private CalcWidth As Long, CalcHeight As Long
  Private Const MIN_HEIGHT = 60 '361
  Private Const MIN_WIDTH = 120 '360
  
  Private Const RM_SIZE = 0.8
'''  Private Const RM_SZ_X = 0.8
'''  Private Const RM_SZ_Y = 0.8
  

Public Sub Activate()
  'bridge method to call the form's Activate event method
  Form_Activate
End Sub

Private Sub DropObjs(ByVal NewX As Single, ByVal NewY As Single, Optional ByVal NoGrid As Boolean = False)

  Dim i As Long, mDX As Single, mDY As Single
  Dim tmpGrid As Boolean
  
  On Error GoTo ErrHandler
  
  'drop selected object at new location
  MoveObj = False
  
  'if forcing no-grid
  If NoGrid Then
    'cache usegrid value
    tmpGrid = Settings.LEUseGrid
    'force grid off
    Settings.LEUseGrid = False
  End If
  
  ' if selection is multiple objects, steps are different
  If Selection.Type = lsMultiple Then
    'determine offset between new location and current selection shape position
    '(don't include 8 pixel offset; it's only used in single object movement)
    mDX = (NewX - Selection.X1) / DSF
    mDY = (NewY - Selection.Y1) / DSF
    
    'step through all objects in selection collection
    For i = 0 To Selection.Number - 1
      Select Case SelectedObjects(i).Type
      Case lsRoom
        'set x and y values of room loc
        With Room(SelectedObjects(i).Number).Loc
          .X = GridPos(.X + mDX)
          .Y = GridPos(.Y + mDY)
        End With
        
      Case lsTransPt
        If SelectedObjects(i).Number < 1 Then
          'set x and y values of this trans pt (leg 1)
          With TransPt(-1 * SelectedObjects(i).Number).Loc(1)
            .X = GridPos(.X + mDX)
            .Y = GridPos(.Y + mDY)
          End With
            
        Else
          'set x and y values of this trans pt (leg 0)
          With TransPt(SelectedObjects(i).Number).Loc(0)
            .X = GridPos(.X + mDX)
            .Y = GridPos(.Y + mDY)
          End With
        End If
    
      Case lsComment
        With Comment(SelectedObjects(i).Number).Loc
          .X = GridPos(.X + mDX)
          .Y = GridPos(.Y + mDY)
        End With
        
      Case lsErrPt
        With ErrPt(SelectedObjects(i).Number).Loc
          .X = GridPos(.X + mDX)
          .Y = GridPos(.Y + mDY)
        End With
      End Select
      
      MarkAsDirty
    Next i
    
    'step through again and reposition everyone
    For i = 0 To Selection.Number - 1
      Select Case SelectedObjects(i).Type
      Case lsRoom
        RepositionRoom SelectedObjects(i).Number
        
      Case lsTransPt
        If SelectedObjects(i).Number < 0 Then
          RepositionRoom TransPt(-1 * SelectedObjects(i).Number).Room(1)
        Else
          RepositionRoom TransPt(SelectedObjects(i).Number).Room(0)
        End If
        
      Case lsErrPt
        SetExitPos ErrPt(SelectedObjects(i).Number).FromRoom, ErrPt(SelectedObjects(i).Number).ExitID
      End Select
    Next i
    
    'adjust layout area Max/min, in case objects are moved
    'outside current boundaries
    AdjustMaxMin
    
    'redraw to update everything
    DrawLayout
  
  Else
    'reposition the object, based on its type
    Select Case Selection.Type
    Case lsRoom
      'set x and y values of room loc
      With Room(Selection.Number).Loc
        .X = GridPos(NewX / DSF - OffsetX)
        .Y = GridPos(NewY / DSF - OffsetY)
      End With
      
      'reposition exits
      RepositionRoom Selection.Number
      
    Case lsTransPt
      'set x and y values of this trans pt
      With TransPt(Selection.Number).Loc(Selection.Leg)
        .X = GridPos((NewX) / DSF - OffsetX)
        .Y = GridPos((NewY) / DSF - OffsetY)
      End With
      
      'reposition exits
      RepositionRoom TransPt(Selection.Number).Room(0)
      
    Case lsComment
      With Comment(Selection.Number).Loc
        .X = GridPos((NewX) / DSF - OffsetX)
        .Y = GridPos((NewY) / DSF - OffsetY)
      End With
      
      'set dirty flag (since repositionroom is not called for comments
      MarkAsDirty
      
    Case lsErrPt
      With ErrPt(Selection.Number).Loc
        .X = GridPos((NewX) / DSF - OffsetX)
        .Y = GridPos((NewY) / DSF - OffsetY)
      End With
      
      'don't reposition room! just redraw the exit to this errpt
      SetExitPos ErrPt(Selection.Number).FromRoom, ErrPt(Selection.Number).ExitID
     
    End Select
  
    'adjust layout area Max/min, in case objects are moved
    'outside current boundaries
    AdjustMaxMin
    
    'adjust selection location
    Selection.X1 = NewX - 8
    Selection.Y1 = NewY - 8
    'hide outline
    shpMove.Visible = False
    'redraw without handles
    DrawLayout False
    'reposition handles by reselecting
    SelectObj Selection
  End If
  
  'if forcing no-grid
  If NoGrid Then
    'restore usegrid value from cache
    Settings.LEUseGrid = tmpGrid
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub InsertErrPt(ByVal FromRoom As Long, ByVal ExitIndex As Long, ByVal ErrRoom As Long)

  'insert an error point for the exit (ExitIndex) coming from (FromRoom)
  'the original destination (ErrRoom) is no longer in the game
  
  Dim lngRoom As Long, tmpCoord As LCoord
  Dim lngEP As Long, strMsg As String
  
  On Error GoTo ErrHandler
  
  ' inform user that an error point is being inserted
  strMsg = "Exit " & Exits(FromRoom)(ExitIndex).ID & " in '" & Logics(FromRoom).ID & "' points to a nonexistent room (" & ErrRoom & ")."
  strMsg = strMsg & vbNewLine & vbNewLine & "An error point will be inserted at this exit point."
  MsgBoxEx strMsg, vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Exit Error Detected", WinAGIHelp, "htm\winagi\Layout_Editor.htm#errpts"
  
  'find the next available errpt
  lngEP = 0
  Do
    lngEP = lngEP + 1
  Loop While ErrPt(lngEP).Visible
  
  Exits(FromRoom)(ExitIndex).Transfer = -lngEP
  
  Select Case Exits(FromRoom)(ExitIndex).Reason
  Case erNone, erOther
    'position first around from room
    tmpCoord = GetInsertPos(Room(FromRoom).Loc.X, Room(FromRoom).Loc.Y, 0, 1)
  Case erHorizon
    'position around point above
    tmpCoord.X = Room(FromRoom).Loc.X
    tmpCoord.Y = Room(FromRoom).Loc.Y - 1
  Case erBottom
    'position around point below
    tmpCoord.X = Room(FromRoom).Loc.X
    tmpCoord.Y = Room(FromRoom).Loc.Y + 1
  Case erLeft
    'position around point to left
    tmpCoord.X = Room(FromRoom).Loc.X - 1
    tmpCoord.Y = Room(FromRoom).Loc.Y
  Case erRight
    'position around point to right
    tmpCoord.X = Room(FromRoom).Loc.X + 1
    tmpCoord.Y = Room(FromRoom).Loc.Y
  End Select
  
  'put errpt here
  With ErrPt(lngEP)
    'adjust to be centered over same point as a room
    '(by adding .1 to x and .2 to y)
    .Loc.X = tmpCoord.X + 0.1
    .Loc.Y = tmpCoord.Y + RM_SIZE / 4
    .Visible = True
    .Room = ErrRoom
    .FromRoom = FromRoom
    .ExitID = Exits(FromRoom)(ExitIndex).ID
    .Order = ObjCount
  End With
  
  ObjOrder(ObjCount).Type = lsErrPt
  ObjOrder(ObjCount).Number = lngEP
  ObjCount = ObjCount + 1
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function IsBehind(TestLoc As LCoord, TestSize As LCoord, TopLoc As LCoord, TopSize As LCoord) As Boolean

  'if the test object (defined by its location and size)
  'is completely behind the Top object, this function returns true

  'assume NOT behind
  IsBehind = False

  'if test is left of top, it is NOT behind
  If TestLoc.X < TopLoc.X Then
    Exit Function
  End If
  
  'if testis above top, it is NOT behind
  If TestLoc.Y < TopLoc.Y Then
    Exit Function
  End If
  
  'if test is right of top, it is NOT behind
  If TestLoc.X + TestSize.X > TopLoc.X + TopSize.X Then
    Exit Function
  End If
  
  'if test is below top, it is NOT behind
  If TestLoc.Y + TestSize.Y > TopLoc.Y + TopSize.Y Then
    Exit Function
  End If
  
  'if it passes all tests, Test is completely behind Top
  IsBehind = True

End Function

Private Function IsSelected(ByVal ObjType As ELSelection, ByVal ObjNum As Long, Optional ByVal Leg As Long = 0) As Boolean

  Dim i As Long
  
  Select Case Selection.Type
  Case lsRoom
    IsSelected = (ObjNum = Selection.Number) And (ObjType = lsRoom)
    
  Case lsTransPt
    IsSelected = (ObjType = lsTransPt) And (Selection.Number = ObjNum) And (Selection.Leg = Leg)
    
  Case lsErrPt
    IsSelected = (ObjType = lsErrPt) And (Selection.Number = ObjNum)
    
  Case lsMultiple
    For i = 0 To Selection.Number - 1
      Select Case ObjType
      Case lsTransPt
        If SelectedObjects(i).Type = lsTransPt Then
          If Abs(SelectedObjects(i).Number) = ObjNum Then
            If Leg = 0 And SelectedObjects(i).Number > 0 Then
              IsSelected = True
              Exit Function
            ElseIf Leg = 1 And SelectedObjects(i).Number < 0 Then
              IsSelected = True
              Exit Function
            End If
          End If
        End If
        
      Case lsRoom, lsErrPt, lsComment
        If SelectedObjects(i).Type = ObjType And SelectedObjects(i).Number = ObjNum Then
          IsSelected = True
          Exit Function
        End If
      
      End Select
    Next i
  End Select
End Function

Private Sub KeyMoveSelection(ByVal KeyCode As Integer, ByVal sngOffset As Single, ByVal NoGrid As Boolean)

  Dim sngNewX As Single, sngNewY As Single
  
  'if moving multiple items
  If Selection.Type = lsMultiple Then
    'don't include offset for handles
    sngNewX = Selection.X1
    sngNewY = Selection.Y1
  Else
    'if moving a single shape, include the 8 pixel offset to
    'account for the 'handles'
    sngNewX = Selection.X1 + 8
    sngNewY = Selection.Y1 + 8
  End If
  
  Select Case KeyCode
  Case vbKeyUp
    If Selection.Type = lsMultiple Then
      shpMove.Top = shpMove.Top - sngOffset
    End If
    sngNewY = sngNewY - sngOffset
    
  Case vbKeyDown
    If Selection.Type = lsMultiple Then
      shpMove.Top = shpMove.Top + sngOffset
    End If
    sngNewY = sngNewY + sngOffset
    
  Case vbKeyLeft
    If Selection.Type = lsMultiple Then
      shpMove.Left = shpMove.Left - sngOffset
    End If
    sngNewX = sngNewX - sngOffset
    
  Case vbKeyRight
    If Selection.Type = lsMultiple Then
      shpMove.Left = shpMove.Left + sngOffset
    End If
    sngNewX = sngNewX + sngOffset
    
  End Select
  
  'reposition the selection
  DropObjs sngNewX, sngNewY, NoGrid
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickHelp()

On Error Resume Next

  'help with layout
  HtmlHelpS HelpParent, WinAGIHelp, HH_DISPLAY_TOPIC, "htm\winagi\Layout_Editor.htm"
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function OverExit(ByVal X As Single, ByVal Y As Single) As Boolean

  'returns TRUE if coordinates X,Y are over an exit (any exit)
  
  Dim i As Long, j As Long
  Dim tmpSPX As Single, tmpSPY As Single
  Dim tmpEPX As Single, tmpEPY As Single
  
  On Error GoTo ErrHandler
  
  'set width to three so PointInLine function
  'can find lines that are two pixels wide
  picDraw.DrawWidth = 3

  For i = 1 To 255
    If Room(i).Visible Then
      For j = 0 To Exits(i).Count - 1
        'dont include deleted exits
        If Exits(i)(j).Status <> esDeleted Then
          'if there are no transfer points, i.e. zero or negative
          If Exits(i)(j).Transfer <= 0 Then
            'starting point and ending point of line come directly
            'from the exit's start-end points
            tmpSPX = Exits(i)(j).SPX
            tmpSPY = Exits(i)(j).SPY
            tmpEPX = Exits(i)(j).EPX
            tmpEPY = Exits(i)(j).EPY
          
            'dont bother checking, unless line is actually visible
            If LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
              'check for an arrow first
              If PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                'return true
                OverExit = True
                Exit Function
                
              'if not on arrow, check line
              ElseIf PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                'return true
                OverExit = True
                Exit Function
              End If
            End If
          Else
            'there are transfers; check both segments
            'first segment
            tmpSPX = Exits(i)(j).SPX
            tmpSPY = Exits(i)(j).SPY
            
            'if this is first exit with transfer point
            If TransPt(Exits(i)(j).Transfer).Room(0) = i Then
              tmpEPX = TransPt(Exits(i)(j).Transfer).SP.X
              tmpEPY = TransPt(Exits(i)(j).Transfer).SP.Y
            Else
              'swap ep and sp
              tmpEPX = TransPt(Exits(i)(j).Transfer).EP.X
              tmpEPY = TransPt(Exits(i)(j).Transfer).EP.Y
            End If
            
            If LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
              'check for an arrow first
              If PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                'return true
                OverExit = True
                Exit Function
              'if not on arrow, check line
              ElseIf PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                'return true
                OverExit = True
                Exit Function
              End If
            End If
            
            'second segment
            'if this is first exit with transfer point
            If TransPt(Exits(i)(j).Transfer).Room(0) = i Then
              tmpSPX = TransPt(Exits(i)(j).Transfer).EP.X
              tmpSPY = TransPt(Exits(i)(j).Transfer).EP.Y
            Else
              'swap ep and sp
              tmpSPX = TransPt(Exits(i)(j).Transfer).SP.X
              tmpSPY = TransPt(Exits(i)(j).Transfer).SP.Y
            End If
            
            tmpEPX = Exits(i)(j).EPX
            tmpEPY = Exits(i)(j).EPY
          
            If LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
              'check for an arrow first
              If PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                'return true
                OverExit = True
                Exit Function
                
              'if not on arrow, check line
              ElseIf PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                'return true
                OverExit = True
                Exit Function
              End If
            End If
          End If
        End If
      Next j
    End If
  Next i
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub PreviewAllObjects(PreviewPic As PictureBox, ByVal BandW As Boolean, ByVal sngOffsetX As Single, ByVal sngOffsetY As Single, ByVal sngScale As Single)

  Dim rtn As Long
  Dim i As Long, j As Long
  Dim lngRoom As Long
  Dim LineColor As Long
  Dim v(2) As POINTAPI, strID As String
  Dim hBrush As Long, hRgn As Long
  Const PicBorder = 8
  
  For i = 0 To ObjCount - 1
    Select Case ObjOrder(i).Type
    Case lsRoom
      lngRoom = ObjOrder(i).Number
      If Room(lngRoom).Visible Then
        'draw it
        If BandW Then
          PreviewPic.FillColor = vbWhite
          PreviewPic.ForeColor = vbBlack
        Else
          PreviewPic.FillColor = Settings.LEColors.Room.Fill
          PreviewPic.ForeColor = Settings.LEColors.Room.Edge
        End If
        'room size is RM_SIZE units in object scale converted to pixels
        PreviewPic.Line (CLng((Room(lngRoom).Loc.X + sngOffsetX) * sngScale) + PicBorder, CLng((Room(lngRoom).Loc.Y + sngOffsetY) * sngScale) + PicBorder)-Step(RM_SIZE * sngScale, RM_SIZE * sngScale), PreviewPic.ForeColor, B
      End If

    Case lsTransPt
      If TransPt(ObjOrder(i).Number).Count > 0 Then
        If BandW Then
          PreviewPic.FillColor = vbWhite
          PreviewPic.ForeColor = vbBlack
        Else
          PreviewPic.FillColor = Settings.LEColors.TransPt.Fill
          PreviewPic.ForeColor = Settings.LEColors.TransPt.Edge
        End If
        'draw transfer circles
        PreviewPic.Circle ((TransPt(ObjOrder(i).Number).Loc(0).X + RM_SIZE / 4 + sngOffsetX) * sngScale + PicBorder, (TransPt(ObjOrder(i).Number).Loc(0).Y + RM_SIZE / 4 + sngOffsetY) * sngScale + PicBorder), RM_SIZE / 4 * sngScale, PreviewPic.ForeColor
        PreviewPic.Circle ((TransPt(ObjOrder(i).Number).Loc(1).X + RM_SIZE / 4 + sngOffsetX) * sngScale + PicBorder, (TransPt(ObjOrder(i).Number).Loc(1).Y + RM_SIZE / 4 + sngOffsetY) * sngScale + PicBorder), RM_SIZE / 4 * sngScale, PreviewPic.ForeColor
      End If
    Case lsErrPt
      If ErrPt(ObjOrder(i).Number).Visible Then
        If BandW Then
          PreviewPic.FillColor = vbWhite
          PreviewPic.ForeColor = vbBlack
        Else
          PreviewPic.FillColor = Settings.LEColors.ErrPt.Fill
          PreviewPic.ForeColor = Settings.LEColors.ErrPt.Edge
        End If
        'use polygon drawing function
        v(0).X = (ErrPt(ObjOrder(i).Number).Loc.X + sngOffsetX) * sngScale + PicBorder
        v(0).Y = (ErrPt(ObjOrder(i).Number).Loc.Y + sngOffsetY) * sngScale + PicBorder
        v(1).X = (ErrPt(ObjOrder(i).Number).Loc.X + RM_SIZE * 0.75 + sngOffsetX) * sngScale + PicBorder
        v(1).Y = (ErrPt(ObjOrder(i).Number).Loc.Y + sngOffsetY) * sngScale + PicBorder
        v(2).X = (ErrPt(ObjOrder(i).Number).Loc.X + RM_SIZE * 0.375 + sngOffsetX) * sngScale + PicBorder
        v(2).Y = (ErrPt(ObjOrder(i).Number).Loc.Y + RM_SIZE / 2 + sngOffsetY) * sngScale + PicBorder

        rtn = Polygon(PreviewPic.hDC, v(0), 3)
        PreviewPic.Refresh
      End If

    Case lsComment
      If Comment(ObjOrder(i).Number).Visible Then
        If BandW Then
          PreviewPic.FillColor = vbWhite
          PreviewPic.ForeColor = vbBlack
        Else
          PreviewPic.FillColor = Settings.LEColors.Cmt.Fill
          PreviewPic.ForeColor = Settings.LEColors.Cmt.Edge
        End If

        'create region
        hRgn = CreateRoundRectRgn((Comment(ObjOrder(i).Number).Loc.X + sngOffsetX) * sngScale - 1 + PicBorder, _
                                  (Comment(ObjOrder(i).Number).Loc.Y + sngOffsetY) * sngScale - 1 + PicBorder, _
                                  (Comment(ObjOrder(i).Number).Loc.X + Comment(ObjOrder(i).Number).Size.X + sngOffsetX) * sngScale + 2 + PicBorder, _
                                  (Comment(ObjOrder(i).Number).Loc.Y + Comment(ObjOrder(i).Number).Size.Y + sngOffsetY) * sngScale + 2 + PicBorder, 0.1 * sngScale, 0.1 * sngScale)

        'create brush
        hBrush = CreateSolidBrush(PreviewPic.FillColor)

        'fill region
        rtn = FillRgn(PreviewPic.hDC, hRgn, hBrush)

        'delete fill brush; create edge brush
        rtn = DeleteObject(hBrush)
        hBrush = CreateSolidBrush(PreviewPic.ForeColor)

        'draw outline
        rtn = FrameRgn(PreviewPic.hDC, hRgn, hBrush, 1, 1)

        'delete brush and region
        rtn = DeleteObject(hBrush)
        rtn = DeleteObject(hRgn)
      End If
    End Select
  Next i

  'draw all the exit lines
  For i = 1 To 255
    If Room(i).Visible Then
      'draw exits
      For j = 0 To Exits(i).Count - 1
        'skip any deleted exits
        If Exits(i)(j).Status <> esDeleted Then
           'determine color
          Select Case Exits(i)(j).Reason
          Case erOther
            LineColor = Settings.LEColors.Other
          Case Else
            LineColor = Settings.LEColors.Edge
          End Select
          If BandW Then
            PreviewPic.ForeColor = vbBlack
            PreviewPic.FillColor = vbBlack
          Else
            PreviewPic.ForeColor = LineColor
            PreviewPic.FillColor = LineColor
          End If

          'if there is a transfer pt
          If Exits(i)(j).Transfer > 0 Then
            'is this first leg?
            If Exits(i)(j).Leg = 0 Then
              'draw first segment
              PreviewPic.Line (CLng((Exits(i)(j).SPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).SPY + sngOffsetY) * sngScale) + PicBorder)-(CLng((TransPt(Exits(i)(j).Transfer).SP.X + sngOffsetX) * sngScale) + PicBorder, CLng((TransPt(Exits(i)(j).Transfer).SP.Y + sngOffsetY) * sngScale) + PicBorder), PreviewPic.ForeColor
              'draw second segment
              PreviewPic.Line (CLng((TransPt(Exits(i)(j).Transfer).EP.X + sngOffsetX) * sngScale) + PicBorder, CLng((TransPt(Exits(i)(j).Transfer).EP.Y + sngOffsetY) * sngScale) + PicBorder)-(CLng((Exits(i)(j).EPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).EPY + sngOffsetY) * sngScale) + PicBorder), PreviewPic.ForeColor
            End If
          Else
            'draw the exit line
            PreviewPic.Line (CLng((Exits(i)(j).SPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).SPY + sngOffsetY) * sngScale) + PicBorder)-(CLng((Exits(i)(j).EPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).EPY + sngOffsetY) * sngScale) + PicBorder), PreviewPic.ForeColor
          End If
        End If
      Next j
    End If
  Next i

End Sub

Private Sub PreviewSelObjects(PreviewPic As PictureBox, ByVal BandW As Boolean, ByVal sngOffsetX As Single, ByVal sngOffsetY As Single, ByVal sngScale As Single)

  Dim rtn As Long
  Dim i As Long, j As Long
  Dim lngRoom As Long
  Dim LineColor As Long
  Dim v(2) As POINTAPI, strID As String
  Dim hBrush As Long, hRgn As Long
  Const PicBorder = 8
  
  On Error GoTo ErrHandler
  
  'if only one object selected
  If Selection.Type <> lsMultiple Then
    With Selection
      Select Case .Type
      Case lsRoom
        lngRoom = .Number
        If Room(lngRoom).Visible Then
          'draw it
          If BandW Then
            PreviewPic.FillColor = vbWhite
            PreviewPic.ForeColor = vbBlack
          Else
            PreviewPic.FillColor = Settings.LEColors.Room.Fill
            PreviewPic.ForeColor = Settings.LEColors.Room.Edge
          End If
          'room size is RM_SIZE units in object scale converted to pixels
          PreviewPic.Line (CLng((Room(lngRoom).Loc.X + sngOffsetX) * sngScale) + PicBorder, CLng((Room(lngRoom).Loc.Y + sngOffsetY) * sngScale) + PicBorder)-Step(RM_SIZE * sngScale, RM_SIZE * sngScale), PreviewPic.ForeColor, B
        End If
  
      Case lsTransPt
        If TransPt(Abs(.Number)).Count > 0 Then
          If BandW Then
            PreviewPic.FillColor = vbWhite
            PreviewPic.ForeColor = vbBlack
          Else
            PreviewPic.FillColor = Settings.LEColors.TransPt.Fill
            PreviewPic.ForeColor = Settings.LEColors.TransPt.Edge
          End If
          'draw transfer circles
          If .Number > 0 Then
            'first leg
            PreviewPic.Circle ((TransPt(Abs(.Number)).Loc(0).X + RM_SIZE / 4 + sngOffsetX) * sngScale + PicBorder, (TransPt(Abs(.Number)).Loc(0).Y + RM_SIZE / 4 + sngOffsetY) * sngScale + PicBorder), RM_SIZE / 4 * sngScale, PreviewPic.ForeColor
          Else
            'second leg
            PreviewPic.Circle ((TransPt(Abs(.Number)).Loc(1).X + RM_SIZE / 4 + sngOffsetX) * sngScale + PicBorder, (TransPt(Abs(.Number)).Loc(1).Y + RM_SIZE / 4 + sngOffsetY) * sngScale + PicBorder), RM_SIZE / 4 * sngScale, PreviewPic.ForeColor
          End If
        End If
        
      Case lsErrPt
        If ErrPt(.Number).Visible Then
          If BandW Then
            PreviewPic.FillColor = vbWhite
            PreviewPic.ForeColor = vbBlack
          Else
            PreviewPic.FillColor = Settings.LEColors.ErrPt.Fill
            PreviewPic.ForeColor = Settings.LEColors.ErrPt.Edge
          End If
          'use polygon drawing function
          v(0).X = (ErrPt(.Number).Loc.X + sngOffsetX) * sngScale + PicBorder
          v(0).Y = (ErrPt(.Number).Loc.Y + sngOffsetY) * sngScale + PicBorder
          v(1).X = (ErrPt(.Number).Loc.X + RM_SIZE * 0.75 + sngOffsetX) * sngScale + PicBorder
          v(1).Y = (ErrPt(.Number).Loc.Y + sngOffsetY) * sngScale + PicBorder
          v(2).X = (ErrPt(.Number).Loc.X + RM_SIZE * 0.375 + sngOffsetX) * sngScale + PicBorder
          v(2).Y = (ErrPt(.Number).Loc.Y + RM_SIZE / 2 + sngOffsetY) * sngScale + PicBorder
  
          rtn = Polygon(PreviewPic.hDC, v(0), 3)
        End If
  
      Case lsComment
        If Comment(.Number).Visible Then
          If BandW Then
            PreviewPic.FillColor = vbWhite
            PreviewPic.ForeColor = vbBlack
          Else
            PreviewPic.FillColor = Settings.LEColors.Cmt.Fill
            PreviewPic.ForeColor = Settings.LEColors.Cmt.Edge
          End If
  
          'create region
          hRgn = CreateRoundRectRgn((Comment(.Number).Loc.X + sngOffsetX) * sngScale - 1 + PicBorder, _
                                    (Comment(.Number).Loc.Y + sngOffsetY) * sngScale - 1 + PicBorder, _
                                    (Comment(.Number).Loc.X + Comment(.Number).Size.X + sngOffsetX) * sngScale + 2 + PicBorder, _
                                    (Comment(.Number).Loc.Y + Comment(.Number).Size.Y + sngOffsetY) * sngScale + 2 + PicBorder, 0.1 * sngScale, 0.1 * sngScale)
  
          'create brush
          hBrush = CreateSolidBrush(PreviewPic.FillColor)
  
          'fill region
          rtn = FillRgn(PreviewPic.hDC, hRgn, hBrush)
  
          'delete fill brush; create edge brush
          rtn = DeleteObject(hBrush)
          hBrush = CreateSolidBrush(PreviewPic.ForeColor)
  
          'draw outline
          rtn = FrameRgn(PreviewPic.hDC, hRgn, hBrush, 1, 1)
  
          'delete brush and region
          rtn = DeleteObject(hBrush)
          rtn = DeleteObject(hRgn)
        End If
      End Select
    End With
  
  Else
    For i = 0 To Selection.Number - 1
      With SelectedObjects(i)
        Select Case .Type
        Case lsRoom
          lngRoom = .Number
          If Room(lngRoom).Visible Then
            'draw it
            If BandW Then
              PreviewPic.FillColor = vbWhite
              PreviewPic.ForeColor = vbBlack
            Else
              PreviewPic.FillColor = Settings.LEColors.Room.Fill
              PreviewPic.ForeColor = Settings.LEColors.Room.Edge
            End If
            'room size is RM_SIZE units in object scale converted to pixels
            PreviewPic.Line (CLng((Room(lngRoom).Loc.X + sngOffsetX) * sngScale) + PicBorder, CLng((Room(lngRoom).Loc.Y + sngOffsetY) * sngScale) + PicBorder)-Step(RM_SIZE * sngScale, RM_SIZE * sngScale), PreviewPic.ForeColor, B
          End If
    
        Case lsTransPt
          
          If TransPt(Abs(.Number)).Count > 0 Then
            If BandW Then
              PreviewPic.FillColor = vbWhite
              PreviewPic.ForeColor = vbBlack
            Else
              PreviewPic.FillColor = Settings.LEColors.TransPt.Fill
              PreviewPic.ForeColor = Settings.LEColors.TransPt.Edge
            End If
            'draw transfer circles
            If .Number > 0 Then
              'first leg
              PreviewPic.Circle ((TransPt(Abs(.Number)).Loc(0).X + RM_SIZE / 4 + sngOffsetX) * sngScale + PicBorder, (TransPt(Abs(.Number)).Loc(0).Y + RM_SIZE / 4 + sngOffsetY) * sngScale + PicBorder), RM_SIZE / 4 * sngScale, PreviewPic.ForeColor
            Else
              'second leg
              PreviewPic.Circle ((TransPt(Abs(.Number)).Loc(1).X + RM_SIZE / 4 + sngOffsetX) * sngScale + PicBorder, (TransPt(Abs(.Number)).Loc(1).Y + RM_SIZE / 4 + sngOffsetY) * sngScale + PicBorder), RM_SIZE / 4 * sngScale, PreviewPic.ForeColor
            End If
          End If
          
        Case lsErrPt
          If ErrPt(.Number).Visible Then
            If BandW Then
              PreviewPic.FillColor = vbWhite
              PreviewPic.ForeColor = vbBlack
            Else
              PreviewPic.FillColor = Settings.LEColors.ErrPt.Fill
              PreviewPic.ForeColor = Settings.LEColors.ErrPt.Edge
            End If
            'use polygon drawing function
            v(0).X = (ErrPt(.Number).Loc.X + sngOffsetX) * sngScale + PicBorder
            v(0).Y = (ErrPt(.Number).Loc.Y + sngOffsetY) * sngScale + PicBorder
            v(1).X = (ErrPt(.Number).Loc.X + RM_SIZE * 0.75 + sngOffsetX) * sngScale + PicBorder
            v(1).Y = (ErrPt(.Number).Loc.Y + sngOffsetY) * sngScale + PicBorder
            v(2).X = (ErrPt(.Number).Loc.X + RM_SIZE * 0.375 + sngOffsetX) * sngScale + PicBorder
            v(2).Y = (ErrPt(.Number).Loc.Y + RM_SIZE / 2 + sngOffsetY) * sngScale + PicBorder
    
            rtn = Polygon(PreviewPic.hDC, v(0), 3)
          End If
    
        Case lsComment
          If Comment(.Number).Visible Then
            If BandW Then
              PreviewPic.FillColor = vbWhite
              PreviewPic.ForeColor = vbBlack
            Else
              PreviewPic.FillColor = Settings.LEColors.Cmt.Fill
              PreviewPic.ForeColor = Settings.LEColors.Cmt.Edge
            End If
    
            'create region
            hRgn = CreateRoundRectRgn((Comment(.Number).Loc.X + sngOffsetX) * sngScale - 1 + PicBorder, _
                                      (Comment(.Number).Loc.Y + sngOffsetY) * sngScale - 1 + PicBorder, _
                                      (Comment(.Number).Loc.X + Comment(.Number).Size.X + sngOffsetX) * sngScale + 2 + PicBorder, _
                                      (Comment(.Number).Loc.Y + Comment(.Number).Size.Y + sngOffsetY) * sngScale + 2 + PicBorder, 0.1 * sngScale, 0.1 * sngScale)
    
            'create brush
            hBrush = CreateSolidBrush(PreviewPic.FillColor)
    
            'fill region
            rtn = FillRgn(PreviewPic.hDC, hRgn, hBrush)
    
            'delete fill brush; create edge brush
            rtn = DeleteObject(hBrush)
            hBrush = CreateSolidBrush(PreviewPic.ForeColor)
    
            'draw outline
            rtn = FrameRgn(PreviewPic.hDC, hRgn, hBrush, 1, 1)
    
            'delete brush and region
            rtn = DeleteObject(hBrush)
            rtn = DeleteObject(hRgn)
          End If
        End Select
      End With
    Next i
  End If
  'draw all the exit lines
  For i = 1 To 255
    If Room(i).Visible And IsSelected(lsRoom, i) Then
      'draw exits
      For j = 0 To Exits(i).Count - 1
        'skip any deleted exits
        If Exits(i)(j).Status <> esDeleted Then
          'determine color
          Select Case Exits(i)(j).Reason
          Case erOther
            LineColor = Settings.LEColors.Other
          Case Else
            LineColor = Settings.LEColors.Edge
          End Select
          If BandW Then
            PreviewPic.ForeColor = vbBlack
            PreviewPic.FillColor = vbBlack
          Else
            PreviewPic.ForeColor = LineColor
            PreviewPic.FillColor = LineColor
          End If
          'if there is a transfer pt
          Select Case Exits(i)(j).Transfer
          Case Is > 0
            'is this first leg?
            If Exits(i)(j).Leg = 0 Then
              'only show lines that are TO a selected object
              If IsSelected(lsTransPt, Exits(i)(j).Transfer, 1) Then
                'draw first segment
                PreviewPic.Line (CLng((Exits(i)(j).SPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).SPY + sngOffsetY) * sngScale) + PicBorder)-(CLng((TransPt(Exits(i)(j).Transfer).SP.X + sngOffsetX) * sngScale) + PicBorder, CLng((TransPt(Exits(i)(j).Transfer).SP.Y + sngOffsetY) * sngScale) + PicBorder), PreviewPic.ForeColor
              End If
              If IsSelected(lsTransPt, Exits(i)(j).Transfer, 0) Then
                'draw second segment
                PreviewPic.Line (CLng((TransPt(Exits(i)(j).Transfer).EP.X + sngOffsetX) * sngScale) + PicBorder, CLng((TransPt(Exits(i)(j).Transfer).EP.Y + sngOffsetY) * sngScale) + PicBorder)-(CLng((Exits(i)(j).EPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).EPY + sngOffsetY) * sngScale) + PicBorder), PreviewPic.ForeColor
              End If
            Else
              'second leg
              'only show lines that are TO a selected object
              If IsSelected(lsTransPt, Exits(i)(j).Transfer, 0) Then
                'draw first segment
                PreviewPic.Line (CLng((Exits(i)(j).SPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).SPY + sngOffsetY) * sngScale) + PicBorder)-(CLng((TransPt(Exits(i)(j).Transfer).EP.X + sngOffsetX) * sngScale) + PicBorder, CLng((TransPt(Exits(i)(j).Transfer).EP.Y + sngOffsetY) * sngScale) + PicBorder), PreviewPic.ForeColor
              End If
              If IsSelected(lsTransPt, Exits(i)(j).Transfer, 1) Then
                'draw second segment
                PreviewPic.Line (CLng((TransPt(Exits(i)(j).Transfer).SP.X + sngOffsetX) * sngScale) + PicBorder, CLng((TransPt(Exits(i)(j).Transfer).SP.Y + sngOffsetY) * sngScale) + PicBorder)-(CLng((Exits(i)(j).EPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).EPY + sngOffsetY) * sngScale) + PicBorder), PreviewPic.ForeColor
              End If
            End If
            'if exit is to an errpt,
          Case Is < 0
            If IsSelected(lsErrPt, -Exits(i)(j).Transfer) Then
              'determine color
              Select Case Exits(i)(j).Reason
              Case erOther
                LineColor = Settings.LEColors.Other
              Case Else
                LineColor = Settings.LEColors.Edge
              End Select
              If BandW Then
                PreviewPic.ForeColor = vbBlack
                PreviewPic.FillColor = vbBlack
              Else
                PreviewPic.ForeColor = LineColor
                PreviewPic.FillColor = LineColor
              End If
              'draw the exit line
              PreviewPic.Line (CLng((Exits(i)(j).SPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).SPY + sngOffsetY) * sngScale) + PicBorder)-(CLng((Exits(i)(j).EPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).EPY + sngOffsetY) * sngScale) + PicBorder), PreviewPic.ForeColor
            End If
          
          Case Else 'exit is to a room
            If IsSelected(lsRoom, Exits(i)(j).Room) Then
              'determine color
              Select Case Exits(i)(j).Reason
              Case erOther
                LineColor = Settings.LEColors.Other
              Case Else
                LineColor = Settings.LEColors.Edge
              End Select
              If BandW Then
                PreviewPic.ForeColor = vbBlack
                PreviewPic.FillColor = vbBlack
              Else
                PreviewPic.ForeColor = LineColor
                PreviewPic.FillColor = LineColor
              End If
              'draw the exit line
              PreviewPic.Line (CLng((Exits(i)(j).SPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).SPY + sngOffsetY) * sngScale) + PicBorder)-(CLng((Exits(i)(j).EPX + sngOffsetX) * sngScale) + PicBorder, CLng((Exits(i)(j).EPY + sngOffsetY) * sngScale) + PicBorder), PreviewPic.ForeColor
            End If
          End Select
        End If
      Next j
    End If
  Next i
Exit Sub

ErrHandler:
  Resume Next
End Sub


Private Sub PrintAllObjects(ByVal BandW As Boolean, LT As LCoord, RB As LCoord)

  Dim rtn As Long
  Dim v(2) As POINTAPI, strLine As String
  Dim i As Long, j As Long, k As Long
  Dim lngRoom As Long, strID As String
  Dim LineColor As Long, sngTH As Single
  Dim hBrush As Long, hRgn As Long
  Dim lngLM As Long, lngTM As Long
  
  'text height
  sngTH = Printer.TextHeight("Ay")
  
  'calculate left/top margins
  lngLM = 0.75 * PDPIx - PMLeft
  lngTM = 0.75 * PDPIy - PMTop
  
  'add objects
  For i = 0 To ObjCount - 1
    Select Case ObjOrder(i).Type
    Case lsRoom
      lngRoom = ObjOrder(i).Number
      If Room(lngRoom).Visible Then
        'if on this page,
        If ObjOnPage(ObjOrder(i), LT, RB) Then
          'draw it
          If BandW Then
            Printer.FillColor = vbWhite
            Printer.ForeColor = vbBlack
          Else
            Printer.FillColor = Settings.LEColors.Room.Fill
            Printer.ForeColor = Settings.LEColors.Room.Edge
          End If
          'room size is RM_SIZE units in object scale converted to pixels
          Printer.Line (CLng((Room(lngRoom).Loc.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((Room(lngRoom).Loc.Y - LT.Y) * PrintScale * PDPIy + lngTM))-Step(RM_SIZE * PrintScale * PDPIx, RM_SIZE * PrintScale * PDPIy), Printer.ForeColor, B
          'if scale is <40%
          If PrintScale < 0.4 Then
            'use just the number
            strID = CStr(lngRoom)
          Else
            'if logic id is too long, it won't fit in the box; truncate if necessary
            strID = ResourceName(Logics(lngRoom), True, True)
            If Printer.TextWidth(strID) > RM_SIZE * PrintScale * PDPIx Then
              Do
                strID = Left$(strID, Len(strID) - 1)
              Loop Until Printer.TextWidth(strID) <= RM_SIZE * PrintScale * PDPIx
            End If
          End If
          Printer.CurrentX = (Room(lngRoom).Loc.X - LT.X + RM_SIZE / 2) * PrintScale * PDPIx - Printer.TextWidth(strID) / 2 + lngLM
          Printer.CurrentY = (Room(lngRoom).Loc.Y - LT.Y + RM_SIZE * 0.75) * PrintScale * PDPIy - Printer.TextHeight(strID) / 2 + lngTM
          Printer.Print strID
        End If
      End If

    Case lsTransPt
      If TransPt(ObjOrder(i).Number).Count > 0 Then
        If ObjOnPage(ObjOrder(i), LT, RB, False) Then
          If BandW Then
            Printer.FillColor = vbWhite
            Printer.ForeColor = vbBlack
          Else
            Printer.FillColor = Settings.LEColors.TransPt.Fill
            Printer.ForeColor = Settings.LEColors.TransPt.Edge
          End If
          'draw transfer circle
          Printer.Circle ((TransPt(ObjOrder(i).Number).Loc(0).X + RM_SIZE / 4 - LT.X) * PrintScale * PDPIx + lngLM, (TransPt(ObjOrder(i).Number).Loc(0).Y + RM_SIZE / 4 - LT.Y) * PrintScale * PDPIy + lngTM), RM_SIZE / 4 * PrintScale * PDPIx, Printer.ForeColor
          Printer.CurrentX = (TransPt(ObjOrder(i).Number).Loc(0).X + RM_SIZE / 4 - LT.X) * PrintScale * PDPIx - Printer.TextWidth(CStr(ObjOrder(i).Number)) / 2 + lngLM
          Printer.CurrentY = (TransPt(ObjOrder(i).Number).Loc(0).Y + RM_SIZE / 4 - LT.Y) * PrintScale * PDPIx - Printer.TextHeight(CStr(ObjOrder(i).Number)) / 2 + lngTM
          Printer.Print CStr(ObjOrder(i).Number)
        End If
        If ObjOnPage(ObjOrder(i), LT, RB, True) Then
          If BandW Then
            Printer.FillColor = vbWhite
            Printer.ForeColor = vbBlack
          Else
            Printer.FillColor = Settings.LEColors.TransPt.Fill
            Printer.ForeColor = Settings.LEColors.TransPt.Edge
          End If
          'draw transfer circle
          Printer.Circle ((TransPt(ObjOrder(i).Number).Loc(1).X + RM_SIZE / 4 - LT.X) * PrintScale * PDPIx + lngLM, (TransPt(ObjOrder(i).Number).Loc(1).Y + RM_SIZE / 4 - LT.Y) * PrintScale * PDPIy + lngTM), RM_SIZE / 4 * PrintScale * PDPIx, Printer.ForeColor
          Printer.CurrentX = (TransPt(ObjOrder(i).Number).Loc(1).X + RM_SIZE / 4 - LT.X) * PrintScale * PDPIx - Printer.TextWidth(CStr(ObjOrder(i).Number)) / 2 + lngLM
          Printer.CurrentY = (TransPt(ObjOrder(i).Number).Loc(1).Y + RM_SIZE / 4 - LT.Y) * PrintScale * PDPIx - Printer.TextHeight(CStr(ObjOrder(i).Number)) / 2 + lngTM
          Printer.Print CStr(ObjOrder(i).Number)
        End If
      End If
      
    Case lsErrPt
      If ErrPt(ObjOrder(i).Number).Visible Then
        If ObjOnPage(ObjOrder(i), LT, RB) Then
          If BandW Then
            Printer.FillColor = vbWhite
            Printer.ForeColor = vbBlack
          Else
            Printer.FillColor = Settings.LEColors.ErrPt.Fill
            Printer.ForeColor = Settings.LEColors.ErrPt.Edge
          End If
          'use polygon drawing function
          v(0).X = (ErrPt(ObjOrder(i).Number).Loc.X - LT.X) * PrintScale * PDPIx + lngLM
          v(0).Y = (ErrPt(ObjOrder(i).Number).Loc.Y - LT.Y) * PrintScale * PDPIy + lngTM
          v(1).X = (ErrPt(ObjOrder(i).Number).Loc.X + RM_SIZE * 0.75 - LT.X) * PrintScale * PDPIx + lngLM
          v(1).Y = (ErrPt(ObjOrder(i).Number).Loc.Y - LT.Y) * PrintScale * PDPIy + lngTM
          v(2).X = (ErrPt(ObjOrder(i).Number).Loc.X + RM_SIZE * 0.375 - LT.X) * PrintScale * PDPIx + lngLM
          v(2).Y = (ErrPt(ObjOrder(i).Number).Loc.Y + RM_SIZE / 2 - LT.Y) * PrintScale * PDPIy + lngTM
  
          rtn = Polygon(Printer.hDC, v(0), 3)
        End If
      End If

    Case lsComment
      lngRoom = ObjOrder(i).Number
      If Comment(lngRoom).Visible Then
        If ObjOnPage(ObjOrder(i), LT, RB) Then
          If BandW Then
            Printer.FillColor = vbWhite
            Printer.ForeColor = vbBlack
          Else
            Printer.FillColor = Settings.LEColors.Cmt.Fill
            Printer.ForeColor = Settings.LEColors.Cmt.Edge
          End If
  
          'create region
          hRgn = CreateRoundRectRgn((Comment(lngRoom).Loc.X - LT.X) * PrintScale * PDPIx + lngLM - 1, _
                                    (Comment(lngRoom).Loc.Y - LT.Y) * PrintScale * PDPIy + lngTM - 1, _
                                    (Comment(lngRoom).Loc.X + Comment(lngRoom).Size.X - LT.X) * PrintScale * PDPIx + lngLM + 2, _
                                    (Comment(lngRoom).Loc.Y + Comment(lngRoom).Size.Y - LT.Y) * PrintScale * PDPIy + lngTM + 2, 0.1 * PrintScale * PDPIx, 0.1 * PrintScale * PDPIy)
  
          'create brush
          hBrush = CreateSolidBrush(Printer.FillColor)
  
          'fill region
          rtn = FillRgn(Printer.hDC, hRgn, hBrush)
  
          'delete fill brush; create edge brush
          rtn = DeleteObject(hBrush)
          hBrush = CreateSolidBrush(Printer.ForeColor)
  
          'draw outline
          rtn = FrameRgn(Printer.hDC, hRgn, hBrush, Printer.DrawWidth, Printer.DrawWidth)
  
          'delete brush and region
          rtn = DeleteObject(hBrush)
          rtn = DeleteObject(hRgn)
          
          If Trim$(Comment(lngRoom).Text) <> vbNullString Then
            'use text box to draw text on
            txtComment.Width = Comment(lngRoom).Size.X * DSF - 12
            txtComment.Height = Comment(lngRoom).Size.Y * DSF - 8
            txtComment.Text = Comment(lngRoom).Text
           
           'now copy lines onto drawing surface
            
            'get line Count
            j = SendMessage(txtComment.hWnd, EM_GETLINECOUNT, 0, 0)
            
            For k = 0 To j - 1
              'get index of beginning of this line
              rtn = SendMessage(txtComment.hWnd, EM_LINEINDEX, k, 0)
              'get length of this line
              rtn = SendMessage(txtComment.hWnd, EM_LINELENGTH, rtn, 0)
              'get text of this line
              strLine = ChrW$(rtn And &HFF) & ChrW$(rtn \ &H100) & String$(rtn, 32)
              rtn = SendMessageByString(txtComment.hWnd, EM_GETLINE, k, strLine)
              strLine = Replace(strLine, ChrW$(0), vbNullString)
              strLine = RTrim$(strLine)
              
              'print the line
              Printer.CurrentX = (Comment(lngRoom).Loc.X - LT.X) * PrintScale * PDPIx + 6 * ScreenTWIPSX / Printer.TwipsPerPixelX * PrintScale + lngLM
              Printer.CurrentY = (Comment(lngRoom).Loc.Y - LT.Y) * PrintScale * PDPIy + 4 * ScreenTWIPSY / Printer.TwipsPerPixelY * PrintScale + k * sngTH + lngTM
              'if not enough room vertically (meaning text would extend below bottom edge of comment box)
              If Printer.CurrentY + sngTH > (Comment(lngRoom).Loc.Y + Comment(lngRoom).Size.Y - LT.Y) * PrintScale * PDPIy - 2 * ScreenTWIPSY / Printer.TwipsPerPixelY * PrintScale + lngTM Then
                Exit For
              End If
              Printer.Print strLine
            Next k
          End If
        End If
      End If
    End Select
  Next i

  'draw exit lines
  For i = 1 To 255
    If Room(i).Visible Then
      For j = 0 To Exits(i).Count - 1
        'skip any deleted exits
        If Exits(i)(j).Status <> esDeleted Then
          'determine color
          Select Case Exits(i)(j).Reason
          Case erOther
            LineColor = Settings.LEColors.Other
          Case Else
            LineColor = Settings.LEColors.Edge
          End Select
          If BandW Then
            Printer.ForeColor = vbBlack
            Printer.FillColor = vbBlack
          Else
            Printer.ForeColor = LineColor
            Printer.FillColor = LineColor
          End If
          
          Select Case Exits(i)(j).Transfer
          Case Is > 0 'if there is a transfer pt
            'is this first leg?
            If Exits(i)(j).Leg = 0 Then
              If LineOnPage(Exits(i)(j).SPX, Exits(i)(j).SPY, TransPt(Exits(i)(j).Transfer).SP.X, TransPt(Exits(i)(j).Transfer).SP.Y, LT, RB) Then
                'print first segment
                Printer.Line (CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM))-(CLng((TransPt(Exits(i)(j).Transfer).SP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).SP.Y - LT.Y) * PrintScale * PDPIy + lngTM)), Printer.ForeColor
                'print arrow
                PrintArrow CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM), CLng((TransPt(Exits(i)(j).Transfer).SP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).SP.Y - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
              End If
              If LineOnPage(TransPt(Exits(i)(j).Transfer).EP.X, TransPt(Exits(i)(j).Transfer).EP.Y, Exits(i)(j).EPX, Exits(i)(j).EPY, LT, RB) Then
                'draw second segment
                Printer.Line (CLng((TransPt(Exits(i)(j).Transfer).EP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).EP.Y - LT.Y) * PrintScale * PDPIy + lngTM))-(CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM)), Printer.ForeColor
                'print arrow
                PrintArrow CLng((TransPt(Exits(i)(j).Transfer).EP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).EP.Y - LT.Y) * PrintScale * PDPIy + lngTM), CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
              End If
            Else
              'draw arrows
              If LineOnPage(Exits(i)(j).SPX, Exits(i)(j).SPY, TransPt(Exits(i)(j).Transfer).EP.X, TransPt(Exits(i)(j).Transfer).EP.Y, LT, RB) Then
                'print arrow
                PrintArrow CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM), CLng((TransPt(Exits(i)(j).Transfer).EP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).EP.Y - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
              End If
              If LineOnPage(TransPt(Exits(i)(j).Transfer).SP.X, TransPt(Exits(i)(j).Transfer).SP.Y, Exits(i)(j).EPX, Exits(i)(j).EPY, LT, RB) Then
                'print arrow
                PrintArrow CLng((TransPt(Exits(i)(j).Transfer).SP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).SP.Y - LT.Y) * PrintScale * PDPIy + lngTM), CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
              End If
            End If
          
          Case Else 'exit is to an errpt or has no transfer
            'no transfer
            If LineOnPage(Exits(i)(j).SPX, Exits(i)(j).SPY, Exits(i)(j).EPX, Exits(i)(j).EPY, LT, RB) Then
              'draw the exit line
              Printer.Line (CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM))-(CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM)), Printer.ForeColor
              'print arrow
              PrintArrow CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM), CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
            End If
          End Select
        End If 'exit deleted
      Next j
    End If 'room visible
  Next i
End Sub

Public Sub PrintLayout(ByVal PrintAll As Boolean, ByVal BandW As Boolean, ByVal CropMarks As Boolean, ByVal NewScale As Single)

  Dim lngRow As Long, lngCol As Long
  Dim LT As LCoord, RB As LCoord
  Dim lWidth As Long, lHeight As Long
  Dim sngPW As Single, sngPH As Single
  Dim sngVal As Single
  
  On Error GoTo ErrHandler
    
  'save scale
  PrintScale = NewScale
  
  'set font name and size
  Printer.Font.Name = Settings.PFontName
  Printer.Font.Size = 10 * PrintScale
  Printer.DrawWidth = PDPIx / 48 * PrintScale
  
  'print a blank line to force printer object to refresh before printing
  Printer.Print ""
  
  'determine height/width of a page the printer in world coordinates
  sngPW = (Printer.Width / Printer.TwipsPerPixelX / PDPIx - 1.5) / PrintScale
  sngPH = (Printer.Height / Printer.TwipsPerPixelY / PDPIy - 1.5) / PrintScale
  
  'determine how many pages across and down are needed
  If PrintAll Then
    'use global Max/min values
    sngVal = (MaxX - MinX + 1) / sngPW
    lWidth = Int(sngVal)
    If lWidth <> Int(sngVal * PDPIx) / PDPIx Then
      lWidth = lWidth + 1
    End If
    sngVal = (MaxY - MinY + 1) / sngPH
    lHeight = Int(sngVal)
    If lHeight <> Int(sngVal * PDPIy) / PDPIy Then
      lHeight = lHeight + 1
    End If
  Else
    'use Max/min of selection (calculate from selection shape values)
    If Selection.Type = lsMultiple Then
      sngVal = (shpMove.Width / DSF + 1) / sngPW
    Else
      sngVal = ((Selection.X2 - Selection.X1) / DSF + 1) / sngPW
    End If
    lWidth = Int(sngVal)
    If lWidth <> Int(sngVal * PDPIx) / PDPIx Then
      lWidth = lWidth + 1
    End If
    If Selection.Type = lsMultiple Then
      sngVal = (shpMove.Height / DSF + 1) / sngPH
    Else
      sngVal = ((Selection.Y2 - Selection.Y1) / DSF + 1) / sngPH
    End If
    lHeight = Int(sngVal)
    If lHeight <> Int(sngVal * PDPIy) / PDPIy Then
      lHeight = lHeight + 1
    End If
  End If
  
  For lngCol = 0 To lWidth - 1
    If PrintAll Then
      LT.X = MinX + lngCol * sngPW
    Else
      If Selection.Type = lsMultiple Then
        LT.X = shpMove.Left / DSF - OffsetX + lngCol * sngPW
      Else
        LT.X = Selection.X1 / DSF - OffsetX + lngCol * sngPW
      End If
    End If
    RB.X = LT.X + sngPW
    
    For lngRow = 0 To lHeight - 1
      'if not first page
      If lngCol <> 0 Or lngRow <> 0 Then
        Printer.NewPage
      End If
      
      If PrintAll Then
        LT.Y = MinY + lngRow * sngPH
      Else
        If Selection.Type = lsMultiple Then
          LT.Y = shpMove.Top / DSF - OffsetY + lngRow * sngPH
        Else
          LT.Y = Selection.Y1 / DSF - OffsetY + lngRow * sngPH
        End If
      End If
      RB.Y = LT.Y + sngPH
      
      'if printing entire layout (range = 0)
      If PrintAll Then
        PrintAllObjects BandW, LT, RB
      Else
        PrintSelObjects BandW, LT, RB
      End If
            
      'adjust font size and print width
      Printer.Font.Size = 12
      Printer.DrawWidth = 1
      
      'block out margins (so objects that are outside the .75 in margin but within the printable margin
      'are hidden)
      Printer.Line (0, 0)-(Printer.ScaleWidth - PDPIx / 10, 0.75 * PDPIy - PMTop - PDPIy / 10), vbWhite, BF
      Printer.Line (0, 0)-(0.75 * PDPIx - PMLeft - PDPIx / 10, Printer.ScaleHeight - PDPIy / 10), vbWhite, BF
      Printer.Line (Printer.ScaleWidth, Printer.ScaleHeight)-Step(PDPIx / 10 - Printer.ScaleWidth, PDPIy / 10 - 0.75 * PDPIy + PMBottom), vbWhite, BF
      Printer.Line (Printer.ScaleWidth, Printer.ScaleHeight)-Step(PDPIx / 10 - 0.75 * PDPIx + PMRight, PDPIy / 10 - Printer.ScaleHeight), vbWhite, BF
      
      'if including crop marks,
      If CropMarks Then
        Printer.ForeColor = vbBlack
        'add connector marks on inside edge of pages
        If lngCol < lWidth - 1 Then
          'print crop marks on right edge
          Printer.Line (Printer.ScaleWidth - 0.75 * PDPIx + PMRight, 0)-Step(0, 0.5 * PDPIy - PMTop), vbBlack
          Printer.Line (Printer.ScaleWidth - 0.75 * PDPIx + PMRight, Printer.ScaleHeight)-Step(0, -0.5 * PDPIy + PMBottom), vbBlack
          
          Printer.CurrentX = Printer.ScaleWidth - 0.5 * PDPIx + PMRight
          Printer.CurrentY = (Printer.ScaleHeight - Printer.TextHeight(ChrW$(65 + lngCol))) / 2
          Printer.Print ChrW$(65 + lngCol)
        End If
        If lngCol > 0 Then
          'print crop marks on left edge
          Printer.Line (0.75 * PDPIx - PMLeft, 0)-Step(0, 0.5 * PDPIy - PMTop), vbBlack
          Printer.Line (0.75 * PDPIx - PMLeft, Printer.ScaleHeight)-Step(0, -0.5 * PDPIy + PMBottom), vbBlack
          
          Printer.CurrentX = 0.5 * PDPIx - PMLeft - Printer.TextWidth(ChrW$(64 + lngCol))
          Printer.CurrentY = (Printer.ScaleHeight - Printer.TextHeight(ChrW$(64 + lngCol))) / 2
          Printer.Print ChrW$(64 + lngCol)
        End If
        If lngRow < lHeight - 1 Then
          'print crop marks on bottom edge
          Printer.Line (Printer.ScaleWidth - 0.5 * PDPIx + PMRight, Printer.ScaleHeight - 0.75 * PDPIy + PMBottom)-Step(0.5 * PDPIx, 0), vbBlack
          Printer.Line (0, Printer.ScaleHeight - 0.75 * PDPIy + PMBottom)-Step(0.5 * PDPIx, 0), vbBlack
          
          Printer.CurrentX = (Printer.ScaleWidth - Printer.TextWidth(CStr(lngRow + 1))) / 2
          Printer.CurrentY = Printer.ScaleHeight - 0.5 * PDPIy + PMBottom
          Printer.Print CStr(lngRow + 1)
        End If
        If lngRow > 0 Then
          'print crop marks top edge
          Printer.Line (Printer.ScaleWidth - 0.5 * PDPIx + PMRight, 0.75 * PDPIy - PMTop)-Step(0.5 * PDPIx, 0), vbBlack
          Printer.Line (0, 0.75 * PDPIy - PMTop)-Step(0.5 * PDPIx, 0), vbBlack
          
          Printer.CurrentX = (Printer.ScaleWidth - Printer.TextWidth(CStr(lngRow))) / 2
          Printer.CurrentY = 0.5 * PDPIy - PMTop - Printer.TextHeight(CStr(lngRow))
          Printer.Print CStr(lngRow)
        End If
      End If
    
      'restore font size and print width
      Printer.Font.Size = 10 * PrintScale
      Printer.DrawWidth = PDPIx / 48 * PrintScale
    
    Next lngRow
  Next lngCol
  
Exit Sub

ErrHandler:
  'if a printer error,
  If Err.Number = 482 Then
    'tell user
    MsgBox "A printer error occurred. Printing operation canceled.", vbCritical + vbOKOnly, "Printer Error"
    Printer.KillDoc
    Exit Sub
  End If
  
  Resume Next
End Sub

Private Function ObjOnPage(ObjTest As ObjInfo, LT As LCoord, RB As LCoord, Optional ByVal SecondTrans As Boolean = False) As Boolean

  'returns true if any portion of the object is on this page
  
  Dim tmpVal As Single
  Dim SP As LCoord, EP As LCoord
  
  Select Case ObjTest.Type
  Case lsRoom
    SP.X = (Room(ObjTest.Number).Loc.X)
    SP.Y = (Room(ObjTest.Number).Loc.Y)
    EP.X = (Room(ObjTest.Number).Loc.X + RM_SIZE)
    EP.Y = (Room(ObjTest.Number).Loc.Y + RM_SIZE)
    
  Case lsTransPt
    If SecondTrans Then
      SP.X = (TransPt(Abs(ObjTest.Number)).Loc(1).X)
      SP.Y = (TransPt(Abs(ObjTest.Number)).Loc(1).Y)
      EP.X = (TransPt(Abs(ObjTest.Number)).Loc(1).X + RM_SIZE / 2)
      EP.Y = (TransPt(Abs(ObjTest.Number)).Loc(1).Y + RM_SIZE / 2)
    Else
      SP.X = (TransPt(ObjTest.Number).Loc(0).X)
      SP.Y = (TransPt(ObjTest.Number).Loc(0).Y)
      EP.X = (TransPt(ObjTest.Number).Loc(0).X + RM_SIZE / 2)
      EP.Y = (TransPt(ObjTest.Number).Loc(0).Y + RM_SIZE / 2)
    End If
    
  Case lsErrPt
    SP.X = (ErrPt(ObjTest.Number).Loc.X)
    SP.Y = (ErrPt(ObjTest.Number).Loc.Y)
    EP.X = (ErrPt(ObjTest.Number).Loc.X + RM_SIZE * 0.75)
    EP.Y = (ErrPt(ObjTest.Number).Loc.Y + RM_SIZE / 2)
    
  Case lsComment
    SP.X = (Comment(ObjTest.Number).Loc.X)
    SP.Y = (Comment(ObjTest.Number).Loc.Y)
    EP.X = (Comment(ObjTest.Number).Loc.X + Comment(ObjTest.Number).Size.X)
    EP.Y = (Comment(ObjTest.Number).Loc.Y + Comment(ObjTest.Number).Size.Y)
  End Select
  
  'if ending point is >=LT and starting point <=RB
  ObjOnPage = (EP.X >= LT.X And EP.Y >= LT.Y And SP.X <= RB.X And SP.Y <= RB.Y)
End Function

Private Function LineOnPage(ByVal X1 As Single, ByVal Y1 As Single, ByVal X2 As Single, ByVal Y2 As Single, LT As LCoord, RB As LCoord) As Boolean

  'will determine if any points on the line are located on page
  
  '        |     |
  '     0  |  1  |  2
  '   -----+-----+-----
  '        |     |
  '     3  |  4  |  5
  '   -----+-----+-----
  '        |     |
  '     6  |  7  |  8
  '
  'the page is indicated by element #4
  'all lines can be represented by having a startpoint and endpoint
  'in one of the nine areas; therefore, there are 45 possible combinations
  
  Dim P1C1_X As Boolean, P1C2_X As Boolean
  Dim P2C1_X As Boolean, P2C2_X As Boolean
  Dim P1C1_Y As Boolean, P1C2_Y As Boolean
  Dim P2C1_Y As Boolean, P2C2_Y As Boolean
  Dim sngSlope As Single, blnSlope As Boolean
  Dim tmpVal As Single
  
  'use flags to indicate if endpoints are on box side of box corners
  P1C1_X = (X1 >= LT.X)
  P1C2_X = (X1 <= RB.X)
  P2C1_X = (X2 >= LT.X)
  P2C2_X = (X2 <= RB.X)
  P1C1_Y = (Y1 >= LT.Y)
  P1C2_Y = (Y1 <= RB.Y)
  P2C1_Y = (Y2 >= LT.Y)
  P2C2_Y = (Y2 <= RB.Y)
  
  'if either endpoint is in the box, then the line is in the box (accounts for 9 out of 45)
  If ((P1C1_X And P1C2_X) And (P1C1_Y And P1C2_Y)) Then
    LineOnPage = True
    Exit Function
  End If
  If ((P2C1_X And P2C2_X) And (P2C1_Y And P2C2_Y)) Then
    LineOnPage = True
    Exit Function
  End If
  
  'if line is completly above, below, to right or to left of box (accounts for 20 out of 45)
  If Not P1C1_X And Not P2C1_X Then
    'not on page; return false
    Exit Function
  End If
  If Not P1C2_X And Not P2C2_X Then
    'not on page; return false
    Exit Function
  End If
  If Not P1C1_Y And Not P2C1_Y Then
    'not on page; return false
    Exit Function
  End If
  If Not P1C2_Y And Not P2C2_Y Then
    'not on page; return false
    Exit Function
  End If
  
  'if line goes across box horizontally or vertically (accounts for 2 out of 45)
  If (P1C1_X = P2C1_X And P1C2_X = P2C2_X) Then
    LineOnPage = True
    Exit Function
  End If
  If (P1C1_Y = P2C1_Y And P1C2_Y = P2C2_Y) Then
    LineOnPage = True
    Exit Function
  End If
  
  'for rest, if all four corner points lie on same side of line,
  'line is outside; otherwise line is inside
  'test by comparing slopes of lines from origin to corners; if all four corners are on
  'same side, the slopes will be all less than or all greater
  'than the slope of the test line
  
  'if point 1 is between box sides,
  If P1C1_X And P1C2_X Then
    'swap points so slopes are measured against the second point
    tmpVal = X1
    X1 = X2
    X2 = tmpVal
    tmpVal = Y1
    Y1 = Y2
    Y2 = tmpVal
  End If
  
  'slope of line (X2 <> X1 because of previous filtering)
  sngSlope = (Y2 - Y1) / (X2 - X1)
  
  '0 <> X1 and RB.X <> X1 because line can't be vertical
  '(X1 <> X2; and if original X1 = 0, it is swapped
  
  'determine if slope of line from first point to first corner
  'is greater than or less than slope of line being tested
  '(if line does not cross box, then slope of line from starting point
  'to each corner will be less than all of them or greater than all of them)
  blnSlope = ((LT.Y - Y1) / (LT.X - X1) > sngSlope)
  
  'compare to other three corners
  If ((RB.Y - Y1) / (LT.X - X1) > sngSlope) <> blnSlope Then
    'different signs means on page
    LineOnPage = True
    Exit Function
  End If
  
  If ((LT.Y - Y1) / (RB.X - X1) > sngSlope) <> blnSlope Then
    'different signs means on page
    LineOnPage = True
    Exit Function
  End If

  If ((RB.Y - Y1) / (RB.X - X1) > sngSlope) <> blnSlope Then
    'different signs means on page
    LineOnPage = True
    Exit Function
  End If

  'not on page
End Function


Private Sub PrintArrow(ByVal SPX As Long, ByVal SPY As Long, ByVal EPX As Long, ByVal EPY As Long, ByVal ArrowColor As Long, ByVal ArrowScale As Single)

  'draws an arrowhead at ending point (EP) that is oriented based on the
  'line from starting point (SP) to ending point
  
  'without getting into the derivation, it can be shown that the two points at the base of
  'the arrowhead are:
  '
  ' xc = xe + Sgn(DX) * (Length/Sqr(m^2 + 1)) * (1 +/- m * tanTheta)
  ' yc = ye + Sgn(DY) * (Length/Sqr(m^2 + 1)) * (m +/- tanTheta)
  '
  'the first +/- is determined by sign of DX/DY
  'Length is length of arrowhead; tanTheta is tangent of arrowhead angle
  
  Dim m As Single, DX As Single, DY As Single
  Dim v(2) As POINTAPI
  Dim ldivs As Single
  
  Const tanTheta = 0.25
  
  Dim Length As Single
  Length = 0.2 * ArrowScale * PDPIx
  
  
  'horizontal and vertical distances:
  DY = EPY - SPY
  DX = EPX - SPX
  
  If DX = 0 And DY = 0 Then
    Exit Sub
  End If
  
  'end point is first point of arrow region
  v(0).X = EPX
  v(0).Y = EPY
  
  'slope of line determines how to draw the arrow
  If Abs(DY) > Abs(DX) Then
    'mostly vertical line
    '(swap x and y formulas)
    'slope of line
    m = DX / DY
    'calculate first term (to save on cpu times by only doing the math once)
    ldivs = Sgn(DY) * Length / Sqr(m ^ 2 + 1)
    
    v(1).X = EPX - ldivs * (m + tanTheta)
    v(2).X = EPX - ldivs * (m - tanTheta)
    v(2).Y = EPY - ldivs * (1 + m * tanTheta)
    v(1).Y = EPY - ldivs * (1 - m * tanTheta)
  Else
    'mostly horizontal line
    
    'slope of line
    m = DY / DX
    'calculate first term (to save on cpu times by only doing the math once)
    ldivs = Sgn(DX) * Length / Sqr(m ^ 2 + 1)
    v(1).X = EPX - ldivs * (1 + m * tanTheta)
    v(2).X = EPX - ldivs * (1 - m * tanTheta)
    v(2).Y = EPY - ldivs * (m + tanTheta)
    v(1).Y = EPY - ldivs * (m - tanTheta)
  End If
  
  'draw the arrow
  m = Printer.DrawWidth
  Printer.DrawWidth = 1
  Printer.FillStyle = vbFSSolid
  Polygon Printer.hDC, v(0), 3
  Printer.DrawWidth = m
End Sub


Public Sub PrintPreview(PreviewPic As PictureBox, ByVal PrintAll As Boolean, ByVal BandW As Boolean, ByVal sngPrintScale As Single)

  'draws the layout onto the preview Image so entire layout
  'fits inside
  
  Dim rtn As Long
  Dim i As Long
  Dim sngOffsetX As Single, sngOffsetY As Single
  Dim sngScale As Single, sngVal As Single
  Dim lWidth As Long, lHeight As Long
  Dim sngPW As Single, sngPH As Single
  Const PicBorder = 8
  
  On Error GoTo ErrHandler
  
  'save the new scale Value
  PrintScale = sngPrintScale
  
#If DEBUGMODE <> 1 Then
  'disable updating
  rtn = SendMessage(PreviewPic.hWnd, WM_SETREDRAW, 0, 0)
#End If
  
  'clear the picture
  PreviewPic.Cls
  'set scale to pixels, with white fill
  PreviewPic.ScaleMode = vbPixels
  PreviewPic.FillColor = vbWhite
  PreviewPic.DrawStyle = vbSolid
  
  If PrintAll Then
    'determine how many pages across and down are needed
    sngVal = (MaxX - MinX + 1) * PrintScale / CSng(Printer.Width / Printer.TwipsPerPixelX / PDPIx - 1.5)
    lWidth = Int(sngVal)
    If lWidth <> Int(sngVal * PDPIx) / PDPIx Then
      lWidth = lWidth + 1
    End If
    sngVal = (MaxY - MinY + 1) * PrintScale / CSng(Printer.Height / Printer.TwipsPerPixelY / PDPIy - 1.5)
    lHeight = Int(sngVal)
    If lHeight <> Int(sngVal * PDPIy) / PDPIy Then
      lHeight = lHeight + 1
    End If
    
  Else
    'determine how many pages across and down are needed
    If Selection.Type = lsMultiple Then
      sngVal = (shpMove.Width / DSF + 1) * PrintScale / CSng(Printer.Width / Printer.TwipsPerPixelX / PDPIx - 1.5)
    Else
      sngVal = ((Selection.X2 - Selection.X1) / DSF + 1) * PrintScale / CSng(Printer.Width / Printer.TwipsPerPixelX / PDPIx - 1.5)
    End If
    lWidth = Int(sngVal)
    If lWidth <> Int(sngVal * PDPIx) / PDPIx Then
      lWidth = lWidth + 1
    End If
    If Selection.Type = lsMultiple Then
      sngVal = (shpMove.Height / DSF + 1) * PrintScale / CSng(Printer.Height / Printer.TwipsPerPixelY / PDPIy - 1.5)
    Else
      sngVal = ((Selection.Y2 - Selection.Y1) / DSF + 1) * PrintScale / CSng(Printer.Height / Printer.TwipsPerPixelY / PDPIy - 1.5)
    End If
    lHeight = Int(sngVal)
    If lHeight <> Int(sngVal * PDPIy) / PDPIy Then
      lHeight = lHeight + 1
    End If
  End If
  
  'if more wide than tall
  If lWidth * Printer.ScaleWidth / PDPIx > lHeight * Printer.ScaleHeight / PDPIy Then
    'get scale Value to convert from world units to pixels
    'ensuring that at least one page break is visible in both directions
    sngScale = (PreviewPic.ScaleWidth - 2 * PicBorder) / (lWidth * (Printer.Width / Printer.TwipsPerPixelX / PDPIx - 1.5)) * PrintScale
    If PrintAll Then
      sngOffsetX = 0.5 - MinX
      sngOffsetY = 0.5 - MinY + (PreviewPic.ScaleHeight - lHeight * (Printer.Height / Printer.TwipsPerPixelY / PDPIy - 1.5) / PrintScale * sngScale) / sngScale / 2
    Else
      If Selection.Type = lsMultiple Then
        sngOffsetX = 0.5 - shpMove.Left / DSF + OffsetX
        sngOffsetY = 0.5 - shpMove.Top / DSF + OffsetY + (PreviewPic.ScaleHeight - lHeight * (Printer.Height / Printer.TwipsPerPixelY / PDPIy - 1.5) / PrintScale * sngScale) / sngScale / 2
      Else
        sngOffsetX = 0.5 - Selection.X1 / DSF + OffsetX
        sngOffsetY = 0.5 - Selection.Y1 / DSF + OffsetY + (PreviewPic.ScaleHeight - lHeight * (Printer.Height / Printer.TwipsPerPixelY / PDPIy - 1.5) / PrintScale * sngScale) / sngScale / 2
      End If
    End If
  Else
    'get scale Value to convert from world units to pixels
    'ensuring that at least one page break is visible in both directions
    sngScale = (PreviewPic.ScaleHeight - 2 * PicBorder) / (lHeight * (Printer.Height / Printer.TwipsPerPixelY / PDPIy - 1.5)) * PrintScale
    If PrintAll Then
      sngOffsetX = 0.5 - MinX + (PreviewPic.ScaleWidth - lWidth * (Printer.Width / Printer.TwipsPerPixelX / PDPIx - 1.5) / PrintScale * sngScale) / sngScale / 2
      sngOffsetY = 0.5 - MinY
    Else
      If Selection.Type = lsMultiple Then
        sngOffsetX = 0.5 - shpMove.Left / DSF + OffsetX + (PreviewPic.ScaleWidth - lWidth * (Printer.Width / Printer.TwipsPerPixelX / PDPIx - 1.5) / PrintScale * sngScale) / sngScale / 2
        sngOffsetY = 0.5 - shpMove.Top / DSF + OffsetY
      Else
        sngOffsetX = 0.5 - Selection.X1 / DSF + OffsetX + (PreviewPic.ScaleWidth - lWidth * (Printer.Width / Printer.TwipsPerPixelX / PDPIx - 1.5) / PrintScale * sngScale) / sngScale / 2
        sngOffsetY = 0.5 - Selection.Y1 / DSF + OffsetY
      End If
    End If
  End If
  
  'determine height/width of a page on the preview picture
  sngPW = (Printer.Width / Printer.TwipsPerPixelX / PDPIx - 1.5) / PrintScale * sngScale
  sngPH = (Printer.Height / Printer.TwipsPerPixelY / PDPIy - 1.5) / PrintScale * sngScale

  'add objects
  If PrintAll Then
    'draw surface
    PreviewPic.Line ((MinX + sngOffsetX - 0.5) * sngScale + PicBorder, (MinY + sngOffsetY - 0.5) * sngScale + PicBorder)-Step(lWidth * sngPW, lHeight * sngPH), vbBlack, B
    
    'draw objects on preview pic
    PreviewAllObjects PreviewPic, BandW, sngOffsetX, sngOffsetY, sngScale
  
    'draw lines representing page boundaries
    PreviewPic.ForeColor = vbRed
    PreviewPic.DrawStyle = vbDot
    For i = 1 To lWidth - 1
      PreviewPic.Line ((MinX + sngOffsetX) * sngScale + i * sngPW + PicBorder, (MinY + sngOffsetY - 0.5) * sngScale + PicBorder)-Step(0, lHeight * sngPH), vbRed
    Next i
    For i = 1 To lHeight - 1
      PreviewPic.Line ((MinX + sngOffsetX - 0.5) * sngScale + PicBorder, (MinY + sngOffsetY) * sngScale + i * sngPH + PicBorder)-Step(lWidth * sngPW, 0), vbRed
    Next i
  Else
    'draw surface
    If Selection.Type = lsMultiple Then
      PreviewPic.Line ((shpMove.Left / DSF - OffsetX + sngOffsetX - 0.5) * sngScale + PicBorder, (shpMove.Top / DSF - OffsetY + sngOffsetY - 0.5) * sngScale + PicBorder)-Step(lWidth * sngPW, lHeight * sngPH), vbBlack, B
    Else
      PreviewPic.Line ((Selection.X1 / DSF - OffsetX + sngOffsetX - 0.5) * sngScale + PicBorder, (Selection.Y1 / DSF - OffsetY + sngOffsetY - 0.5) * sngScale + PicBorder)-Step(lWidth * sngPW, lHeight * sngPH), vbBlack, B
    End If
    
    'draw objects on preview pic
    PreviewSelObjects PreviewPic, BandW, sngOffsetX, sngOffsetY, sngScale
    
    'draw lines representing page boundaries
    PreviewPic.ForeColor = vbRed
    PreviewPic.DrawStyle = vbDot
    For i = 1 To lWidth - 1
      If Selection.Type = lsMultiple Then
        PreviewPic.Line ((shpMove.Left / DSF - OffsetX + sngOffsetX - 0.5) * sngScale + i * sngPW + PicBorder, (shpMove.Top / DSF - OffsetY + sngOffsetY - 0.5) * sngScale + PicBorder)-Step(0, lHeight * sngPH), vbRed
      Else
        PreviewPic.Line ((Selection.X1 / DSF - OffsetX + sngOffsetX - 0.5) * sngScale + i * sngPW + PicBorder, (Selection.Y1 / DSF - OffsetY + sngOffsetY - 0.5) * sngScale + PicBorder)-Step(0, lHeight * sngPH), vbRed
      End If
    Next i
    For i = 1 To lHeight - 1
      If Selection.Type = lsMultiple Then
        PreviewPic.Line ((shpMove.Left / DSF - OffsetX + sngOffsetX - 0.5) * sngScale + PicBorder, (shpMove.Top / DSF - OffsetY + sngOffsetY - 0.5) * sngScale + i * sngPH + PicBorder)-Step(lWidth * sngPW, 0), vbRed
      Else
        PreviewPic.Line ((Selection.X1 / DSF - OffsetX + sngOffsetX - 0.5) * sngScale + PicBorder, (Selection.Y1 / DSF - OffsetY + sngOffsetY - 0.5) * sngScale + i * sngPH + PicBorder)-Step(lWidth * sngPW, 0), vbRed
      End If
    Next i
  End If

#If DEBUGMODE <> 1 Then
 'reenable updating
  rtn = SendMessage(PreviewPic.hWnd, WM_SETREDRAW, 1, 0)
#End If

  PreviewPic.Refresh
Exit Sub

ErrHandler:
  Resume Next
End Sub

Private Sub AdjustMaxMin()
  
  Dim i As Long
  Dim lngNum As Long
  
  'reset Max and min
  MinX = 3.402823E+38
  MinY = 3.402823E+38
  MaxX = -3.402823E+38
  MaxY = -3.402823E+38
  
  For i = 0 To ObjCount - 1
    lngNum = ObjOrder(i).Number
    Select Case ObjOrder(i).Type
    Case lsRoom
      TestPos Room(lngNum).Loc.X, Room(lngNum).Loc.Y, RM_SIZE, RM_SIZE
      
    Case lsTransPt
      TestPos TransPt(lngNum).Loc(0).X, TransPt(lngNum).Loc(0).Y, RM_SIZE / 4, RM_SIZE / 4
      TestPos TransPt(lngNum).Loc(1).X, TransPt(lngNum).Loc(1).Y, RM_SIZE / 4, RM_SIZE / 4
      
    Case lsErrPt
      TestPos ErrPt(lngNum).Loc.X, ErrPt(lngNum).Loc.Y, RM_SIZE * 0.75, RM_SIZE / 2
      
    Case lsComment
      TestPos Comment(lngNum).Loc.X, Comment(lngNum).Loc.Y, Comment(lngNum).Size.X, Comment(lngNum).Size.Y
    
    End Select
  Next i
  
  'if no objects,
  If ObjCount <= 0 Then
  '*'Debug.Assert ObjCount = 0
    MinX = 0
    MinY = 0
    MaxX = 6
    MaxY = 6
  Else
    If MaxX - MinX < 6 Then
      MaxX = 6 + MinX
    End If
    If MaxY - MinY < 6 Then
      MaxY = 6 + MinY
    End If
  End If
  
  'reset scrollbars based on current Max/min
  SetScrollBars
End Sub
Private Sub PrintSelObjects(ByVal BandW As Boolean, LT As LCoord, RB As LCoord)

  Dim rtn As Long
  Dim v(2) As POINTAPI, strLine As String
  Dim i As Long, j As Long, k As Long
  Dim lngRoom As Long, strID As String
  Dim LineColor As Long, sngTH As Single
  Dim hBrush As Long, hRgn As Long
  Dim lngLM As Long, lngTM As Long
  Dim lngCount As Long, lngType As Long, ThisObj As ObjInfo
  
  On Error GoTo ErrHandler
  
  'text height
  sngTH = Printer.TextHeight("Ay")
  
  'calculate left/top margins
  lngLM = 0.75 * PDPIx - PMLeft
  lngTM = 0.75 * PDPIy - PMTop
  
  'number of objects to add
  If Selection.Type = lsMultiple Then
    lngCount = Selection.Number - 1
  Else
    ThisObj.Number = Selection.Number
    ThisObj.Type = Selection.Type
    lngCount = 0
  End If
  
  'add objects
  For i = 0 To lngCount
    If Selection.Type = lsMultiple Then
      ThisObj = SelectedObjects(i)
    End If
    
    Select Case ThisObj.Type
    Case lsRoom
      lngRoom = ThisObj.Number
      If Room(lngRoom).Visible Then
        'if on this page,
        If ObjOnPage(ThisObj, LT, RB) Then
          'draw it
          If BandW Then
            Printer.FillColor = vbWhite
            Printer.ForeColor = vbBlack
          Else
            Printer.FillColor = Settings.LEColors.Room.Fill
            Printer.ForeColor = Settings.LEColors.Room.Edge
          End If
          'room size is RM_SIZE units in object scale converted to pixels
          Printer.Line (CLng((Room(lngRoom).Loc.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((Room(lngRoom).Loc.Y - LT.Y) * PrintScale * PDPIy + lngTM))-Step(RM_SIZE * PrintScale * PDPIx, RM_SIZE * PrintScale * PDPIy), Printer.ForeColor, B
          'if scale is <40%
          If PrintScale < 0.4 Then
            'use just the number
            strID = CStr(lngRoom)
          Else
            'if logic id is too long, it won't fit in the box; truncate if necessary
            strID = ResourceName(Logics(lngRoom), True, True)
            If Printer.TextWidth(strID) > RM_SIZE * PrintScale * PDPIx Then
              Do
                strID = Left$(strID, Len(strID) - 1)
              Loop Until Printer.TextWidth(strID) <= RM_SIZE * PrintScale * PDPIx
            End If
          End If
          Printer.CurrentX = (Room(lngRoom).Loc.X - LT.X + RM_SIZE / 2) * PrintScale * PDPIx - Printer.TextWidth(strID) / 2 + lngLM
          Printer.CurrentY = (Room(lngRoom).Loc.Y - LT.Y + RM_SIZE * 0.75) * PrintScale * PDPIy - Printer.TextHeight(strID) / 2 + lngTM
          Printer.Print strID
        End If
      End If

    Case lsTransPt
      If TransPt(Abs(ThisObj.Number)).Count > 0 Then
        If BandW Then
          Printer.FillColor = vbWhite
          Printer.ForeColor = vbBlack
        Else
          Printer.FillColor = Settings.LEColors.TransPt.Fill
          Printer.ForeColor = Settings.LEColors.TransPt.Edge
        End If
        
        'draw transfer circles
        If ThisObj.Number > 0 Then
          'first leg
          If ObjOnPage(ThisObj, LT, RB, False) Then
            Printer.Circle ((TransPt(ThisObj.Number).Loc(0).X + RM_SIZE / 4 - LT.X) * PrintScale * PDPIx + lngLM, (TransPt(ThisObj.Number).Loc(0).Y + RM_SIZE / 4 - LT.Y) * PrintScale * PDPIy + lngTM), RM_SIZE / 4 * PrintScale * PDPIx, Printer.ForeColor
            Printer.CurrentX = (TransPt(ThisObj.Number).Loc(0).X + RM_SIZE / 4 - LT.X) * PrintScale * PDPIx - Printer.TextWidth(CStr(ThisObj.Number)) / 2 + lngLM
            Printer.CurrentY = (TransPt(ThisObj.Number).Loc(0).Y + RM_SIZE / 4 - LT.Y) * PrintScale * PDPIx - Printer.TextHeight(CStr(ThisObj.Number)) / 2 + lngTM
            Printer.Print CStr(ThisObj.Number)
          End If
        Else
          If ObjOnPage(ThisObj, LT, RB, True) Then
            'second leg
            Printer.Circle ((TransPt(Abs(ThisObj.Number)).Loc(1).X + RM_SIZE / 4 - LT.X) * PrintScale * PDPIx + lngLM, (TransPt(Abs(ThisObj.Number)).Loc(1).Y + RM_SIZE / 4 - LT.Y) * PrintScale * PDPIy + lngTM), RM_SIZE / 4 * PrintScale * PDPIx, Printer.ForeColor
            Printer.CurrentX = (TransPt(Abs(ThisObj.Number)).Loc(1).X + RM_SIZE / 4 - LT.X) * PrintScale * PDPIx - Printer.TextWidth(CStr(Abs(ThisObj.Number))) / 2 + lngLM
            Printer.CurrentY = (TransPt(Abs(ThisObj.Number)).Loc(1).Y + RM_SIZE / 4 - LT.Y) * PrintScale * PDPIx - Printer.TextHeight(CStr(Abs(ThisObj.Number))) / 2 + lngTM
            Printer.Print CStr(Abs(ThisObj.Number))
          End If
        End If
      End If
      
    Case lsErrPt
      If ErrPt(ThisObj.Number).Visible Then
        If ObjOnPage(ThisObj, LT, RB) Then
          If BandW Then
            Printer.FillColor = vbWhite
            Printer.ForeColor = vbBlack
          Else
            Printer.FillColor = Settings.LEColors.ErrPt.Fill
            Printer.ForeColor = Settings.LEColors.ErrPt.Edge
          End If
          'use polygon drawing function
          v(0).X = (ErrPt(ThisObj.Number).Loc.X - LT.X) * PrintScale * PDPIx + lngLM
          v(0).Y = (ErrPt(ThisObj.Number).Loc.Y - LT.Y) * PrintScale * PDPIy + lngTM
          v(1).X = (ErrPt(ThisObj.Number).Loc.X + RM_SIZE * 0.75 - LT.X) * PrintScale * PDPIx + lngLM
          v(1).Y = (ErrPt(ThisObj.Number).Loc.Y - LT.Y) * PrintScale * PDPIy + lngTM
          v(2).X = (ErrPt(ThisObj.Number).Loc.X + RM_SIZE * 0.375 - LT.X) * PrintScale * PDPIx + lngLM
          v(2).Y = (ErrPt(ThisObj.Number).Loc.Y + RM_SIZE / 2 - LT.Y) * PrintScale * PDPIy + lngTM
  
          rtn = Polygon(Printer.hDC, v(0), 3)
        End If
      End If

    Case lsComment
      lngRoom = ThisObj.Number
      If Comment(lngRoom).Visible Then
        If ObjOnPage(ThisObj, LT, RB) Then
          If BandW Then
            Printer.FillColor = vbWhite
            Printer.ForeColor = vbBlack
          Else
            Printer.FillColor = Settings.LEColors.Cmt.Fill
            Printer.ForeColor = Settings.LEColors.Cmt.Edge
          End If
  
          'create region
          hRgn = CreateRoundRectRgn((Comment(lngRoom).Loc.X - LT.X) * PrintScale * PDPIx + lngLM - 1, _
                                    (Comment(lngRoom).Loc.Y - LT.Y) * PrintScale * PDPIy + lngTM - 1, _
                                    (Comment(lngRoom).Loc.X + Comment(lngRoom).Size.X - LT.X) * PrintScale * PDPIx + lngLM + 2, _
                                    (Comment(lngRoom).Loc.Y + Comment(lngRoom).Size.Y - LT.Y) * PrintScale * PDPIy + lngTM + 2, 0.1 * PrintScale * PDPIx, 0.1 * PrintScale * PDPIy)
  
          'create brush
          hBrush = CreateSolidBrush(Printer.FillColor)
  
          'fill region
          rtn = FillRgn(Printer.hDC, hRgn, hBrush)
  
          'delete fill brush; create edge brush
          rtn = DeleteObject(hBrush)
          hBrush = CreateSolidBrush(Printer.ForeColor)
  
          'draw outline
          rtn = FrameRgn(Printer.hDC, hRgn, hBrush, Printer.DrawWidth, Printer.DrawWidth)
  
          'delete brush and region
          rtn = DeleteObject(hBrush)
          rtn = DeleteObject(hRgn)
          
          If Trim$(Comment(lngRoom).Text) <> vbNullString Then
            'use text box to draw text on
            txtComment.Width = Comment(lngRoom).Size.X * DSF - 12
            txtComment.Height = Comment(lngRoom).Size.Y * DSF - 8
            txtComment.Text = Comment(lngRoom).Text
            
            'now copy lines onto drawing surface
            
            'get line Count
            j = SendMessage(txtComment.hWnd, EM_GETLINECOUNT, 0, 0)
            
            For k = 0 To j - 1
              'get index of beginning of this line
              rtn = SendMessage(txtComment.hWnd, EM_LINEINDEX, k, 0)
              'get length of this line
              rtn = SendMessage(txtComment.hWnd, EM_LINELENGTH, rtn, 0)
              'get text of this line
              strLine = ChrW$(rtn And &HFF) & ChrW$(rtn \ &H100) & String$(rtn, 32)
              rtn = SendMessageByString(txtComment.hWnd, EM_GETLINE, k, strLine)
              strLine = Replace(strLine, ChrW$(0), vbNullString)
              strLine = RTrim$(strLine)
              
              'print the line
              Printer.CurrentX = (Comment(lngRoom).Loc.X - LT.X) * PrintScale * PDPIx + 6 * ScreenTWIPSX / Printer.TwipsPerPixelX * PrintScale + lngLM
              Printer.CurrentY = (Comment(lngRoom).Loc.Y - LT.Y) * PrintScale * PDPIy + 4 * ScreenTWIPSY / Printer.TwipsPerPixelY * PrintScale + k * sngTH + lngTM
              'if not enough room vertically (meaning text would extend below bottom edge of comment box)
              If Printer.CurrentY + sngTH > (Comment(lngRoom).Loc.Y + Comment(lngRoom).Size.Y - LT.Y) * PrintScale * PDPIy - 2 * ScreenTWIPSY / Printer.TwipsPerPixelY * PrintScale + lngTM Then
                Exit For
              End If
              Printer.Print strLine
            Next k
          End If
        End If
      End If
    End Select
  Next i

  'draw exit lines
  For i = 1 To 255
    If Room(i).Visible Then
      For j = 0 To Exits(i).Count - 1
        'skip any deleted exits
        If Exits(i)(j).Status <> esDeleted And IsSelected(lsRoom, i) Then
          'determine color
          Select Case Exits(i)(j).Reason
          Case erOther
            LineColor = Settings.LEColors.Other
          Case Else
            LineColor = Settings.LEColors.Edge
          End Select
          If BandW Then
            Printer.ForeColor = vbBlack
            Printer.FillColor = vbBlack
          Else
            Printer.ForeColor = LineColor
            Printer.FillColor = LineColor
          End If
          
          
          Select Case Exits(i)(j).Transfer
          Case Is > 0 'if there is a transfer pt
            'is this first leg?
            If Exits(i)(j).Leg = 0 Then
              'only show lines that are TO a selected object
              If IsSelected(lsTransPt, Exits(i)(j).Transfer, 1) Then
                If LineOnPage(Exits(i)(j).SPX, Exits(i)(j).SPY, TransPt(Exits(i)(j).Transfer).SP.X, TransPt(Exits(i)(j).Transfer).SP.Y, LT, RB) Then
                  'print first segment
                  Printer.Line (CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM))-(CLng((TransPt(Exits(i)(j).Transfer).SP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).SP.Y - LT.Y) * PrintScale * PDPIy + lngTM)), Printer.ForeColor
                  'print arrow
                  PrintArrow CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM), CLng((TransPt(Exits(i)(j).Transfer).SP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).SP.Y - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
                End If
              End If
              If IsSelected(lsTransPt, Exits(i)(j).Transfer, 0) Then
                If LineOnPage(TransPt(Exits(i)(j).Transfer).EP.X, TransPt(Exits(i)(j).Transfer).EP.Y, Exits(i)(j).EPX, Exits(i)(j).EPY, LT, RB) Then
                  'draw second segment
                  Printer.Line (CLng((TransPt(Exits(i)(j).Transfer).EP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).EP.Y - LT.Y) * PrintScale * PDPIy + lngTM))-(CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM)), Printer.ForeColor
                  'print arrow
                  PrintArrow CLng((TransPt(Exits(i)(j).Transfer).EP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).EP.Y - LT.Y) * PrintScale * PDPIy + lngTM), CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
                End If
              End If
            Else
              'only show lines that are TO a selected object
              If IsSelected(lsTransPt, Exits(i)(j).Transfer, 0) Then
                'draw arrows
                If LineOnPage(Exits(i)(j).SPX, Exits(i)(j).SPY, TransPt(Exits(i)(j).Transfer).EP.X, TransPt(Exits(i)(j).Transfer).EP.Y, LT, RB) Then
                  Printer.Line (CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM))-(CLng((TransPt(Exits(i)(j).Transfer).EP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).EP.Y - LT.Y) * PrintScale * PDPIy + lngTM)), Printer.ForeColor
                  'print arrow
                  PrintArrow CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM), CLng((TransPt(Exits(i)(j).Transfer).EP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).EP.Y - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
                End If
              End If
              If IsSelected(lsTransPt, Exits(i)(j).Transfer, 1) Then
                If LineOnPage(TransPt(Exits(i)(j).Transfer).SP.X, TransPt(Exits(i)(j).Transfer).SP.Y, Exits(i)(j).EPX, Exits(i)(j).EPY, LT, RB) Then
                  Printer.Line (CLng((TransPt(Exits(i)(j).Transfer).SP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).SP.Y - LT.Y) * PrintScale * PDPIy + lngTM))-(CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM)), Printer.ForeColor
                  'print arrow
                  PrintArrow CLng((TransPt(Exits(i)(j).Transfer).SP.X - LT.X) * PrintScale * PDPIx + lngLM), CLng((TransPt(Exits(i)(j).Transfer).SP.Y - LT.Y) * PrintScale * PDPIy + lngTM), CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
                End If
              End If
            End If
            
          Case Is < 0 'if exit is to an errpt,
            If IsSelected(lsErrPt, -Exits(i)(j).Transfer) Then
              If LineOnPage(Exits(i)(j).SPX, Exits(i)(j).SPY, Exits(i)(j).EPX, Exits(i)(j).EPY, LT, RB) Then
                'draw the exit line
                Printer.Line (CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM))-(CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM)), Printer.ForeColor
                'print arrow
                PrintArrow CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM), CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
              End If
            End If
            
          Case Else
            If IsSelected(lsRoom, Exits(i)(j).Room) Then
              If LineOnPage(Exits(i)(j).SPX, Exits(i)(j).SPY, Exits(i)(j).EPX, Exits(i)(j).EPY, LT, RB) Then
                'draw the exit line
                Printer.Line (CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM))-(CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM)), Printer.ForeColor
                'print arrow
                PrintArrow CLng((Exits(i)(j).SPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).SPY - LT.Y) * PrintScale * PDPIy + lngTM), CLng((Exits(i)(j).EPX - LT.X) * PrintScale * PDPIx + lngLM), CLng((Exits(i)(j).EPY - LT.Y) * PrintScale * PDPIy + lngTM), Printer.ForeColor, PrintScale
              End If
            End If
          End Select
        End If 'exit deleted
      Next j
    End If 'room visible
  Next i
Exit Sub

ErrHandler:
  Resume Next
End Sub

Public Sub SelectRoom(ByVal RoomNum As Long)

  'this is called by the main form when a logic resource is chosen on
  ' the treelist; this sub then unselects the current selection and
  ' then selects RoomNum, repositioning the drawing surface if
  ' necessary
  
  Dim NewSel As TSel, NewObjInfo As ObjInfo
  Dim NewHSVal As Long, NewVsVal As Long
  
  On Error GoTo ErrHandler
  
  ' RoomNum should be a valid room
  '*'Debug.Assert RoomNum > 0 And RoomNum <= 255
  '*'Debug.Assert Logics.Exists(RoomNum)
  '*'Debug.Assert Logics(RoomNum).IsRoom
  If RoomNum < 0 Or RoomNum > 255 Then
    'hmmmm
    Exit Sub
  End If
  If Not Logics.Exists(RoomNum) Then
    'hmmmm
    Exit Sub
  End If
  If Not Logics(RoomNum).IsRoom Then
    'hmmmm
    Exit Sub
  End If
  
  'if there is currently a selection
  If Selection.Type <> lsNone Then
    DeselectObj
  End If
  
  'now select this room
  NewSel.Type = lsRoom
  NewSel.Number = RoomNum
  
  SelectObj NewSel
  
  'is the room on the screen?
  NewObjInfo.Type = lsRoom
  NewObjInfo.Number = RoomNum
  
  If Not ObjOnScreen(NewObjInfo) Then
    'reposition it by adjusting scroll values
    
    'calculate new horizontal scrollbar value:
' HScroll1.Value = -100 * (picDraw.Width/2/DSF - Room(RoomNum).Loc.X)
    NewHSVal = -100 * (picDraw.Width / 2 / DSF - Room(RoomNum).Loc.X)
    If NewHSVal < HScroll1.Min Then
      NewHSVal = HScroll1.Min
    ElseIf NewHSVal > HScroll1.Max Then
      NewHSVal = HScroll1.Max
    End If
    'reposition horizontal scroll (but turn draw off; only need to
    ' draw the layout once, after the vertical value is set)
    blnDontDraw = True
    HScroll1.Value = NewHSVal
    
    NewVsVal = -100 * (picDraw.Height / 2 / DSF - Room(RoomNum).Loc.Y)
    If NewVsVal < VScroll1.Min Then
      NewVsVal = VScroll1.Min
    ElseIf NewVsVal > VScroll1.Max Then
      NewVsVal = VScroll1.Max
    End If
    'reposition vertical scroll (turn draw on, so the layout updates
    blnDontDraw = False
    VScroll1.Value = NewVsVal
  End If
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub TestPos(ByVal TestX As Single, ByVal TestY As Single, ByVal TestW As Single, ByVal TestH As Single)
  
  'compare test values against current
  'Max and min
  If TestX < MinX Then
    MinX = TestX
  End If
  If TestY < MinY Then
    MinY = TestY
  End If
  
  If TestX + TestW > MaxX Then
    MaxX = TestX + TestW
  End If
  If TestY + TestH > MaxY Then
    MaxY = TestY + TestH
  End If
End Sub

Private Sub ChangeFromRoom(ByRef OldSel As TSel, ByVal FromRoom As Long)
  'changes the currently selected exit so its from room matches FromRoom
  
  'before calling this method, the new FromRoom has already been validated
  
  Dim NewID As String
  
  With OldSel
    'create new exit from new 'from' room to current 'to' room of same Type
    NewID = CreateNewExit(FromRoom, Exits(.Number)(.ExitID).Room, Exits(.Number)(.ExitID).Reason)
    
    'delete old exit
    DeleteExit OldSel
    
    'select new line (never has a transfer)
    .Number = FromRoom
    .ExitID = NewID
    
    'if resulting exit has a transfer,
    If Exits(.Number)(.ExitID).Transfer <> 0 Then
      'select first leg
      .Leg = llFirst
    Else
      .Leg = llNoTrans
    End If
    
    'if resulting exit has a reciprocal
    If IsTwoWay(.Number, .ExitID) Then
      'select only this exit
      .TwoWay = ltwOneWay
    Else
      .TwoWay = ltwSingle
    End If
  End With
End Sub

Private Sub DeleteExit(OldSel As TSel)

  'deletes the exit, and hide transfers if appropriate
  
  'also delete a reciprocal, if there is one and a two way exit is specified
  '(if you don't want both exits deleted, make sure OldSel is marked as one way)
  
  Dim tmpTrans As Long, tmpCoord As LCoord
  Dim tmpRoom As Long, tmpID As String
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'if a transfer was involved
  tmpTrans = Exits(OldSel.Number)(OldSel.ExitID).Transfer
  Select Case tmpTrans
  Case Is < 0 'err pt
    'remove from drawing queue
    CompactObjList ErrPt(-tmpTrans).Order
    
    'remove the errpt
    With ErrPt(-tmpTrans)
      .Visible = False
      .ExitID = ""
      .Room = 0
      .FromRoom = 0
      .Loc.X = 0
      .Loc.Y = 0
      .Order = 0
    End With
    
  Case Is > 0  'transfer
    'if this is only exit using transfer, OR a two way exit was deleted
    If TransPt(tmpTrans).Count = 1 Or OldSel.TwoWay = ltwBothWays Then
      'remove the transfer pt as well
      DeleteTransfer tmpTrans
    Else
      'must be a case where twoway exit exists and only
      'one side is being deleted
      '*'Debug.Assert TransPt(tmpTrans).Count = 2
      TransPt(tmpTrans).Count = 1
      'ensure exit from other direction is associated with leg 0
      
      'if exit being deleted is leg 0
      If Exits(OldSel.Number)(OldSel.ExitID).Leg = 0 Then
        'move other exit to leg 0
        With TransPt(tmpTrans)
          'use i to help in switch
          i = .Room(1)
          .Room(1) = .Room(0)
          .Room(0) = i
          .ExitID(0) = .ExitID(1)
          'dont need to keep index for second leg since it is gone now
          .ExitID(1) = vbNullString
          tmpCoord = .Loc(0)
          .Loc(0) = .Loc(1)
          .Loc(1) = tmpCoord
          tmpCoord = .SP
          .SP = .EP
          .EP = tmpCoord
'*'Debug.Assert Exits(.Room(0))(.ExitID(0)).Leg = 1
          Exits(.Room(0))(.ExitID(0)).Leg = 0
        End With
      End If
    End If
  End Select
  
  'if two way,and both are selected
  If OldSel.TwoWay = ltwBothWays Then
    'find and delete reciprocal exit
    If IsTwoWay(OldSel.Number, OldSel.ExitID, tmpRoom, tmpID) Then
      If Exits(tmpRoom)(tmpID).Status = esNew Then
        Exits(tmpRoom).Remove tmpID
      Else
        Exits(tmpRoom)(tmpID).Status = esDeleted
      End If
    End If
  End If
  
  'if this is a new exit, not yet in logic source,
  If Exits(OldSel.Number)(OldSel.ExitID).Status = esNew Then
    'remove the exit
    Exits(OldSel.Number).Remove OldSel.ExitID
  Else
    Exits(OldSel.Number)(OldSel.ExitID).Status = esDeleted
    If Exits(OldSel.Number)(OldSel.ExitID).OldRoom = 0 Then
      'keep the oldroom value in case we end up restoring the exit
      Exits(OldSel.Number)(OldSel.ExitID).OldRoom = Exits(OldSel.Number)(OldSel.ExitID).Room
    End If
    'make sure the transfer value is reset to zero
    Exits(OldSel.Number)(OldSel.ExitID).Transfer = 0
  End If
Exit Sub

ErrHandler:
  Resume Next
End Sub
Private Sub ExitFromPos(ByRef NewSel As TSel, ByVal SearchPriority As Long, ByVal X As Single, ByVal Y As Single)

  'finds the line under the point x,y that matches style, and selects it
  'by setting newsel object
  
  'if line found, newsel contains selected exit info
  
  'SearchPriority = 0: only other exits are checked
  '               = 1: all edge exits are searched
  
  '(error exits are never searched, since this function is never
  'called when an error exit line is clicked)
  
  Dim i As Long, j As Long
  Dim tmpRoom As Long, tmpID As String
  Dim tmpSPX As Single, tmpSPY As Single
  Dim tmpEPX As Single, tmpEPY As Single
  Dim lngPOL As Long
  
  On Error GoTo ErrHandler
  
  'set width to three so PointInLine function
  'can find lines that are two pixels wide
  picDraw.DrawWidth = 3

  For i = 1 To 255
    If Room(i).Visible Then
      For j = 0 To Exits(i).Count - 1
        'dont include deleted exits
        If Exits(i)(j).Status <> esDeleted Then
          'if Type matches
          If (SearchPriority = 0 And ((Exits(i)(j).Reason = erOther))) Or (SearchPriority = 1 And Exits(i)(j).Reason <> erOther) Then
            'if there are no transfer points, i.e. zero or negative
            If Exits(i)(j).Transfer <= 0 Then
              'starting point and ending point of line come directly
              'from the exit's start-end points
              tmpSPX = Exits(i)(j).SPX
              tmpSPY = Exits(i)(j).SPY
              tmpEPX = Exits(i)(j).EPX
              tmpEPY = Exits(i)(j).EPY
            
              'dont bother checking, unless line is actually visible
              If LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                'check for an arrow first
                If PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                  'if exit has a reciprocal
                  If IsTwoWay(i, j) Then
                    NewSel.TwoWay = ltwOneWay
                  Else
                    NewSel.TwoWay = ltwSingle
                  End If
                  'select line
                  NewSel.Type = lsExit
                  NewSel.Number = i
                  NewSel.ExitID = Exits(i)(j).ID
                  NewSel.Leg = llNoTrans
                  Exit Sub
                  
                'if not on arrow, check line
                ElseIf PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                  'if exit has a reciprocal
                  If IsTwoWay(i, j, tmpRoom, tmpID) Then
                    'check if arrow on reciprocal is selected
                    If PointOnArrow(X, Y, tmpEPX, tmpEPY, tmpSPX, tmpSPY) Then
                      'on arrow means one way
                      NewSel.TwoWay = ltwOneWay
                      NewSel.Number = tmpRoom
                      NewSel.ExitID = tmpID
                      NewSel.Type = lsExit
                      NewSel.Leg = llNoTrans
                      Exit Sub
                    Else
                      'both
                      NewSel.TwoWay = ltwBothWays
                    End If
                  Else
                    'single line
                    NewSel.TwoWay = ltwSingle
                  End If
                  
                  'select line
                  NewSel.Type = lsExit
                  NewSel.Number = i
                  NewSel.ExitID = Exits(i)(j).ID
                  NewSel.Leg = llNoTrans
                  Exit Sub
                End If
              End If
            
            Else
              'there are transfers; check both segments
              'first segment
              tmpSPX = Exits(i)(j).SPX
              tmpSPY = Exits(i)(j).SPY
              
              'if this is first exit with transfer point
              If TransPt(Exits(i)(j).Transfer).Room(0) = i Then
                tmpEPX = TransPt(Exits(i)(j).Transfer).SP.X
                tmpEPY = TransPt(Exits(i)(j).Transfer).SP.Y
              Else
                'swap ep and sp
                tmpEPX = TransPt(Exits(i)(j).Transfer).EP.X
                tmpEPY = TransPt(Exits(i)(j).Transfer).EP.Y
              End If
              
              If LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                'check for an arrow first
                If PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                  'if exit has a reciprocal
                  If IsTwoWay(i, j) Then
                    NewSel.TwoWay = ltwOneWay
                  Else
                    NewSel.TwoWay = ltwSingle
                  End If
                  'select line
                  NewSel.Type = lsExit
                  NewSel.Number = i
                  NewSel.ExitID = Exits(i)(j).ID
                  'use first leg
                  NewSel.Leg = llFirst
                  Exit Sub
                  
                'if not on arrow, check line
                ElseIf PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                  'if exit has a reciprocal
                  If IsTwoWay(i, j, tmpRoom, tmpID) Then
                    'check if arrow on reciprocal is selected
                    If PointOnArrow(X, Y, tmpEPX, tmpEPY, tmpSPX, tmpSPY) Then
                      'on arrow means one way
                      NewSel.TwoWay = ltwOneWay
                      NewSel.Number = tmpRoom
                      NewSel.ExitID = tmpID
                      NewSel.Type = lsExit
                      NewSel.Leg = llSecond
                      Exit Sub
                    Else
                      'both
                      NewSel.TwoWay = ltwBothWays
                    End If
                  Else
                    'single line
                    NewSel.TwoWay = ltwSingle
                  End If
                  
                  'select line
                  NewSel.Type = lsExit
                  NewSel.Number = i
                  NewSel.ExitID = Exits(i)(j).ID
                  'use first leg
                  NewSel.Leg = llFirst
                  Exit Sub
                End If
              End If
              
              'second segment
              'if this is first exit with transfer point
              If TransPt(Exits(i)(j).Transfer).Room(0) = i Then
                tmpSPX = TransPt(Exits(i)(j).Transfer).EP.X
                tmpSPY = TransPt(Exits(i)(j).Transfer).EP.Y
              Else
                'swap ep and sp
                tmpSPX = TransPt(Exits(i)(j).Transfer).SP.X
                tmpSPY = TransPt(Exits(i)(j).Transfer).SP.Y
              End If
              
              tmpEPX = Exits(i)(j).EPX
              tmpEPY = Exits(i)(j).EPY
            
              If LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                'check for an arrow first
                If PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                  'if exit has a reciprocal
                  If IsTwoWay(i, j) Then
                    NewSel.TwoWay = ltwOneWay
                  Else
                    NewSel.TwoWay = ltwSingle
                  End If
                  'select line
                  NewSel.Type = lsExit
                  NewSel.Number = i
                  NewSel.ExitID = Exits(i)(j).ID
                  'use second leg
                  NewSel.Leg = llSecond
                  Exit Sub
                  
                'if not on arrow, check line
                ElseIf PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
                  'if exit has a reciprocal
                  If IsTwoWay(i, j, tmpRoom, tmpID) Then
                    'check if arrow on reciprocal is selected
                    If PointOnArrow(X, Y, tmpEPX, tmpEPY, tmpSPX, tmpSPY) Then
                      'on arrow means one way
                      NewSel.TwoWay = ltwOneWay
                      NewSel.Number = tmpRoom
                      NewSel.ExitID = tmpID
                      NewSel.Type = lsExit
                      NewSel.Leg = llFirst
                      Exit Sub
                   Else
                      'both
                      NewSel.TwoWay = ltwBothWays
                    End If
                  Else
                    'single line
                    NewSel.TwoWay = ltwSingle
                  End If
                  
                  'select line
                  NewSel.Type = lsExit
                  NewSel.Number = i
                  NewSel.ExitID = Exits(i)(j).ID
                  'use second leg
                  NewSel.Leg = llSecond
                  Exit Sub
                End If
              End If
            End If
          End If
        End If
      Next j
    End If
  Next i
Exit Sub

ErrHandler:
  Resume Next
End Sub

Private Function GetExitByTrans(ByVal RoomNumber As Long, ByVal TransNumber As Long) As Long

  'finds the exit in roomnumber with corresponding transnumber
  'if not found, returns -1
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  With Exits(RoomNumber)
    For i = 0 To .Count - 1
      If .Item(i).Status <> esDeleted Then
        If .Item(i).Transfer = TransNumber Then
          'found it
          GetExitByTrans = i
          Exit Function
        End If
      End If
    Next i
  End With
  
  'not found
  GetExitByTrans = -1
Exit Function

ErrHandler:
  'clear error and return not found
  Err.Clear
  GetExitByTrans = -1
End Function

Private Sub GetSelectedObjects()
  
  'populates the selectedobjects array with
  'all objects within the selection area
  
  Dim i As Long, lngCount As Long

  'clear out array
  ReDim SelectedObjects(0)
  'set Count to -1
  lngCount = -1
  
  'step through all objects on layout
  For i = 0 To ObjCount - 1
    Select Case ObjOrder(i).Type
    Case lsRoom
      'is room inside selection area?
      If IsInSelection(Room(ObjOrder(i).Number).Loc, RM_SIZE, RM_SIZE) Then
        'add it
        lngCount = lngCount + 1
        ReDim Preserve SelectedObjects(lngCount)
        SelectedObjects(lngCount) = ObjOrder(i)
      End If
      
    Case lsTransPt
      'is first transpt inside selection area?
      If IsInSelection(TransPt(ObjOrder(i).Number).Loc(0), RM_SIZE / 2, RM_SIZE / 2) Then
        'add it
        lngCount = lngCount + 1
        ReDim Preserve SelectedObjects(lngCount)
        SelectedObjects(lngCount) = ObjOrder(i)
      End If
      
      'is second pt inside selection area?
      If IsInSelection(TransPt(ObjOrder(i).Number).Loc(1), RM_SIZE / 2, RM_SIZE / 2) Then
        'add it
        lngCount = lngCount + 1
        ReDim Preserve SelectedObjects(lngCount)
        SelectedObjects(lngCount) = ObjOrder(i)
        'flag as second leg
        SelectedObjects(lngCount).Number = -1 * SelectedObjects(lngCount).Number
      End If
      
    Case lsErrPt
      'is errpt inside selection area?
      If IsInSelection(ErrPt(ObjOrder(i).Number).Loc, RM_SIZE * 0.75, RM_SIZE / 2) Then
        'add it
        lngCount = lngCount + 1
        ReDim Preserve SelectedObjects(lngCount)
        SelectedObjects(lngCount) = ObjOrder(i)
      End If
      
    Case lsComment
      'is comment inside selection area?
      If IsInSelection(Comment(ObjOrder(i).Number).Loc, Comment(ObjOrder(i).Number).Size.X, Comment(ObjOrder(i).Number).Size.Y) Then
        'add it
        lngCount = lngCount + 1
        ReDim Preserve SelectedObjects(lngCount)
        SelectedObjects(lngCount) = ObjOrder(i)
      End If
      
    End Select
  Next i
  
  'if any objects selected, set flag
  Select Case lngCount
  Case Is > 0
    Selection.Type = lsMultiple
    Selection.Number = lngCount + 1
    
    'select them
    SelectObj Selection
    
  Case 0  'one object selected
    With Selection
      .Type = SelectedObjects(0).Type
      .Number = Abs(SelectedObjects(0).Number)
      If SelectedObjects(0).Number < 0 Then
        'this is a transfer, and it is second leg
        .Leg = 1
      Else
        'first leg is selected
        .Leg = 0
      End If
    End With
    
    'now select it
    SelectObj Selection
  
  Case -1 'nothing is selected
    Selection.Type = lsNone
  End Select
End Sub
Private Function GridPos(ByVal Pos As Single) As Single
  
  'sets pos to align with grid
  
  If Settings.LEUseGrid Then
    GridPos = Round(Pos / Settings.LEGrid, 0) * Settings.LEGrid
  Else
    GridPos = Pos
  End If
End Function

Private Sub HighlightExitStart(ByVal RoomNumber As Long, ByVal Dir As EEReason)

  '*'Debug.Assert SelTool = ltEdge1 Or SelTool = ltEdge2 Or SelTool = ltOther
  
  NewExitReason = Dir
  
  With linMove
    Select Case Dir
    Case erHorizon
      .X1 = (Room(RoomNumber).Loc.X + OffsetX) * DSF
      .X2 = (Room(RoomNumber).Loc.X + RM_SIZE + OffsetX) * DSF
      .Y1 = (Room(RoomNumber).Loc.Y + OffsetY) * DSF
      .Y2 = (Room(RoomNumber).Loc.Y + OffsetY) * DSF
    Case erBottom
      .X1 = (Room(RoomNumber).Loc.X + OffsetX) * DSF
      .X2 = (Room(RoomNumber).Loc.X + RM_SIZE + OffsetX) * DSF
      .Y1 = (Room(RoomNumber).Loc.Y + RM_SIZE + OffsetY) * DSF
      .Y2 = (Room(RoomNumber).Loc.Y + RM_SIZE + OffsetY) * DSF
    Case erRight
      .X1 = (Room(RoomNumber).Loc.X + RM_SIZE + OffsetX) * DSF
      .X2 = (Room(RoomNumber).Loc.X + RM_SIZE + OffsetX) * DSF
      .Y1 = (Room(RoomNumber).Loc.Y + OffsetY) * DSF
      .Y2 = (Room(RoomNumber).Loc.Y + RM_SIZE + OffsetY) * DSF
    Case erLeft
      .X1 = (Room(RoomNumber).Loc.X + OffsetX) * DSF
      .X2 = (Room(RoomNumber).Loc.X + OffsetX) * DSF
      .Y1 = (Room(RoomNumber).Loc.Y + OffsetY) * DSF
      .Y2 = (Room(RoomNumber).Loc.Y + RM_SIZE + OffsetY) * DSF
    Case erOther
      'dont show line
      .Visible = False
      
      Exit Sub
    End Select
    .BorderWidth = 4
    .BorderColor = vbRed
    .Visible = True
  End With
End Sub

Public Function InitFonts()

  On Error GoTo ErrHandler
  
  picDraw.Font.Name = Settings.EFontName
  
  txtComment.Font.Name = Settings.EFontName
  txtComment.BackColor = Settings.LEColors.Cmt.Fill
  txtComment.ForeColor = Settings.LEColors.Cmt.Edge
  
  DrawLayout
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub InsertTransfer(ByRef NewSel As TSel)

  'newsel will be an exit that does not currently have a transfer point
  
  Dim tmpRoom As Long, tmpID As String
  Dim tmpTrans As Long, tmpDir As EEReason
  Dim tmpX As Single, tmpY As Single
  Dim DX As Single, DY As Single
  
  'get next available number
  For tmpTrans = 1 To 255
    If TransPt(tmpTrans).Count = 0 Then
      Exit For
    End If
  Next tmpTrans
  
  'if too many
  If tmpTrans = 256 Then
    MsgBox "This layout has reached the limit of allowable transfers.", vbInformation + vbOKOnly, "Can't Insert Transfer"
    Exit Sub
  End If
  
  'add to order
  TransPt(tmpTrans).Order = ObjCount
  ObjOrder(ObjCount).Number = tmpTrans
  ObjOrder(ObjCount).Type = lsTransPt
  ObjCount = ObjCount + 1
  
  'position along line at center
  'dx and dy
  With Exits(NewSel.Number)(NewSel.ExitID)
    DX = .EPX - .SPX
    DY = .EPY - .SPY
    
    'center point of line
    tmpX = .SPX + DX / 2
    tmpY = .SPY + DY / 2
  End With

  With TransPt(tmpTrans)
    .Loc(0).X = GridPos(tmpX - IIf(DX <> 0, Sgn(DX), Sgn(DY)) * RM_SIZE / 4 - RM_SIZE / 4)
    .Loc(0).Y = GridPos(tmpY - RM_SIZE / 4)
    .Loc(1).X = GridPos(tmpX + IIf(DX <> 0, Sgn(DX), Sgn(DY)) * RM_SIZE / 4 - RM_SIZE / 4)
    .Loc(1).Y = GridPos(tmpY - RM_SIZE / 4)
  
    'if exit is bothways
    If IsTwoWay(NewSel.Number, NewSel.ExitID, tmpRoom, tmpID) Then
      'Count is two
      .Count = 2
      .ExitID(1) = tmpID
      Exits(tmpRoom)(tmpID).Transfer = tmpTrans
      Exits(tmpRoom)(tmpID).Leg = 1
      SetExitPos tmpRoom, tmpID
    Else
      'Count is one
      .Count = 1
    End If
    
    'add from/to room info
    .Room(0) = NewSel.Number
    .Room(1) = Exits(NewSel.Number)(NewSel.ExitID).Room
    .ExitID(0) = NewSel.ExitID
  End With
  
  'set trans property of exit
  Exits(NewSel.Number)(NewSel.ExitID).Transfer = tmpTrans
  Exits(NewSel.Number)(NewSel.ExitID).Leg = 0
  
  
  'reposition exit lines
  SetExitPos NewSel.Number, NewSel.ExitID
      
  MarkAsDirty
End Sub
Private Sub MarkAsDirty()

  If Not IsDirty Then
    'set dirty flag
    IsDirty = True
    
    'enable menu and toolbar button
    frmMDIMain.mnuRSave.Enabled = True
    frmMDIMain.Toolbar1.Buttons("save").Enabled = True
    
    'mark caption
    Caption = sDM & Caption
  End If
End Sub
Private Function IsInSelection(Loc As LCoord, ByVal X2 As Single, ByVal Y2 As Single) As Boolean
  
  'if the area represented by Loc and Height/Width is fully inside the shpMove area
  'this method returns true

  Dim X1 As Single, Y1 As Single

  X1 = Loc.X
  Y1 = Loc.Y

  'convert line coordinates into screen coordinates
  X1 = (X1 + OffsetX) * DSF
  X2 = X2 * DSF + X1
  Y1 = (Y1 + OffsetY) * DSF
  Y2 = Y2 * DSF + Y1

  'if fully within selection shape

  'use flags to indicate if endpoints are on box side of box corners
  With shpMove
    IsInSelection = (X1 >= .Left And X2 <= .Left + .Width And Y1 >= .Top And Y2 <= .Top + .Height)
  End With
End Function


Private Function IsTwoWay(ByVal RoomNum As Long, ExitID As Variant, Optional ByRef MatchRoom As Long, Optional ByRef MatchID As String, Optional ByVal MatchTrans As Long = 1) As Boolean
  
  'checks exit ExitID in RoomNum and if it has a reciprocal exit,
  'it returns true, and sets Matchroom and MatchID to reciprocal exit
  'ExitID can be a number (index Value) or string (ID Value)
  '
  'MatchID is always a string (ID Value)
  
  'if MatchTrans=0, IsTwoWay returns true regardless if transfer Value of reciprocal matches and DOES NOT set the matching Room and ID
  'if MatchTrans=1, IsTwoWay returns true only if transfer Value of reciprocal matches
  'if MatchTrans=2, IsTwoWay returns true only if transfer Count is 1, regardless of transfer Value
  
  Dim tmpDir As EEReason, i As Long
  Dim FromRoom As Long
  
  On Error GoTo ErrHandler
  
  With Exits(RoomNum)(ExitID)
    'if 'to room' is invalid (<=0 or >255)
    If .Room <= 0 Or .Room > 255 Then
      Exit Function
    End If
    'or transfer<0
    If .Transfer < 0 Then
      'then this exit points to an errpt and NEVER
      'is TwoWay
      Exit Function
    End If
    
    'get opposite direction
    Select Case .Reason
    Case erHorizon
      tmpDir = erBottom
    Case erBottom
      tmpDir = erHorizon
    Case erRight
      tmpDir = erLeft
    Case erLeft
      tmpDir = erRight
    Case Else
      tmpDir = erOther
    End Select
    
    'if exit has a transfer point
    If .Transfer > 0 Then
      'if count=2 then this is a two way
      IsTwoWay = (TransPt(.Transfer).Count = 2)
      'if it is, and matching
      If IsTwoWay And MatchTrans Then
        'return reciprocal index
        If Exits(RoomNum)(ExitID).Leg = 0 Then
          MatchRoom = TransPt(.Transfer).Room(1)
          MatchID = TransPt(.Transfer).ExitID(1)
        Else
          MatchRoom = TransPt(.Transfer).Room(0)
          MatchID = TransPt(.Transfer).ExitID(0)
        End If
      End If
      
      Exit Function
    End If
    
    'not part of a transfer; manually search all exits in target room
    FromRoom = .Room
  End With
 
  'if from room is hidden
  'CANT have a reciprocal from a hidden room
  If Not Room(FromRoom).Visible Then
    Exit Function
  End If
  
  For i = 0 To Exits(FromRoom).Count - 1
    With Exits(FromRoom)(i)
      'if this exit goes back to original room AND is not deleted
      If .Room = RoomNum And .Status <> esDeleted Then
        'if reason matches,   ??????why the second part????
        If (.Reason = tmpDir) Then
          'if NOT a circular exit (from room = to room) OR exits are different
          If (FromRoom <> RoomNum) Or (.ID <> ExitID) Then
            'check transfer
            Select Case MatchTrans
            Case 0 'return true regardless if transfer Value of reciprocal matches
              IsTwoWay = True
              MatchRoom = FromRoom
              MatchID = .ID
              Exit Function
              
            Case 1 'return true only if transfer Value of reciprocal matches
              If .Transfer = Exits(RoomNum)(ExitID).Transfer Then
                IsTwoWay = True
                MatchRoom = FromRoom
                MatchID = .ID
                Exit Function
              End If
            
            Case 2 'returns true only if transfer Count is 1, regardless of transfer Value
              If .Transfer > 0 Then
                If TransPt(.Transfer).Count = 1 Then
                  IsTwoWay = True
                  MatchRoom = FromRoom
                  MatchID = .ID
                  Exit Function
                End If
              End If
            End Select
        
        'if transfer match
          End If
        End If
      End If
    End With
  Next i
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function
Public Sub MenuClickCopy()

  Select Case Selection.Type
  Case lsRoom, lsErrPt, lsExit
    ShowLogic Selection
  End Select
End Sub

Public Sub MenuClickCustom1()

  'repairs the layout by recalculating all exits;
  'room and comment positions don't get affected;
  'transfers and errpts are recalculated
  
  On Error GoTo ErrHandler
  
  'if this is a forced rebuild (from the LoadLayout method)
  'the form isn't visible yet, so we don't ask the user; we just do it
  If Visible Then
    If MsgBox("This will reestablish all the exit information as it currently" & vbNewLine & _
                   "exists in game logics. For best results, all logic editors should be closed." & vbNewLine & vbNewLine & _
                   "Do you want to continue?", vbQuestion + vbYesNo, "Repair Layout") <> vbYes Then
      Exit Sub
    End If
  End If
  ExtractLayout True
  DrawLayout
  
Exit Sub
ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickCustom2()

  'toggle showpic for all rooms in the display
  'also toggle the default behavior
  
  Dim blnUnloaded As Boolean
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  If Asc(frmMDIMain.mnuRCustom2.Caption) = 72 Then 'menu says 'Hide'
    For i = 0 To 255
      Room(i).ShowPic = False
    Next i
    
    Settings.LEShowPics = False
    
    'change menu caption
    frmMDIMain.mnuRCustom2.Caption = "Show All &Pics" & vbTab & "Ctrl+Alt+S"
    'mark as dirty
    MarkAsDirty
    
  Else 'menu says 'Show'
    For i = 0 To 255
      If Pictures.Exists(i) Then
        Room(i).ShowPic = True
      End If
    Next i
    
    Settings.LEShowPics = True
    
    'change menu caption
    frmMDIMain.mnuRCustom2.Caption = "Hide All &Pics" & vbTab & "Ctrl+Alt+H"
    'mark as dirty
    MarkAsDirty
  End If

  'force redraw
  DrawLayout
  
  'save the setting
  WriteSetting GameSettings, sLAYOUT, "ShowPics", Settings.LEShowPics
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
  
End Sub

Public Sub MenuClickCut()

  On Error GoTo ErrHandler
  
  Select Case Selection.Type
  Case lsNone
    ShowRoom
    
  Case lsRoom
    'hide this room
    
    'update logic
    Logics(Selection.Number).IsRoom = False
    Logics(Selection.Number).Save
    
    'update selection
    UpdateSelection rtLogic, Selection.Number, umProperty
    
    'if there is a layout file,
    If FileExists(GameDir & GameID & ".wal") Then
      'update the layout file
      UpdateLayoutFile euRemoveRoom, Logics(Selection.Number).Number, Nothing
    End If
    
    'hide room and redraw
    HideRoom Selection.Number
    DrawLayout
  End Select
Exit Sub

ErrHandler:
  'if logic not loaded error
  If Err.Number = vbObjectError + 563 Then
  Else
    '*'Debug.Assert False
  End If
  Resume Next
End Sub

Public Sub MenuClickDelete()

  Dim rtn As VbMsgBoxResult
  Dim blnNoWarn As Boolean
  
  On Error GoTo ErrHandler
  
  Select Case Selection.Type
  Case lsRoom
    If Pictures.Exists(Selection.Number) Then
      'if user wants warning
      Select Case Settings.LEDelPicToo
      Case 0 'ask
        rtn = MsgBoxEx("Do you also want to remove the associated picture for this room?", vbYesNo + vbQuestion + vbMsgBoxHelpButton, "Delete Room", WinAGIHelp, "htm\winagi\editinglayouts.htm#delete", "Always take this action", blnNoWarn)
        'if user wants no more warnings
        If blnNoWarn Then
          Settings.LEDelPicToo = 8 - rtn 'convert yes/no(6/7) into 2/1
          WriteSetting GameSettings, sLAYOUT, "DelPicToo", Settings.LEDelPicToo
        End If
      Case 1 'no
        rtn = vbNo
      Case 2 'yes
        rtn = vbYes
      End Select
    Else
      'nothing to delete
      rtn = vbNo
    End If
    
    'delete picture if that option was selected
    If rtn = vbYes Then
      'remove picture
      RemovePicture Selection.Number
    End If
    
    'remove logic from game
    On Error GoTo ErrHandler
    'now remove logic (this clears the selection
    'which is why it has to be last)
    RemoveLogic Selection.Number
    MarkAsDirty
    
    'clear out exit info for the deleted logic AFTER removing it
    Set Exits(Selection.Number) = New AGIExits
    
  Case lsTransPt
    'remove the transfer point
    DeleteTransfer Selection.Number
    'deselect
    DeselectObj
    'redraw
    DrawLayout
          
    MarkAsDirty
    
  Case lsErrPt
    'remove the errpt and its exit line
    'mark exit as deleted
    With ErrPt(Selection.Number)
      Exits(.FromRoom)(.ExitID).Status = esDeleted
      CompactObjList .Order
      .Visible = False
      .ExitID = ""
      .Order = 0
      .FromRoom = 0
      .Room = 0
      .Loc.X = 0
'''      .Loc.Y = 0 WHY not zero out Y value too?
    End With
    
    'deselect and redraw
    DeselectObj
    DrawLayout True
    MarkAsDirty
    
  Case lsComment
    'delete the selected comment
    With Comment(Selection.Number)
      .Visible = False
      .Size.X = 0
      .Size.Y = 0
      .Loc.X = 0
      .Loc.Y = 0
      .Text = ""
      CompactObjList .Order
      .Order = 0
    End With
    
    'deselect and redraw
    DeselectObj
    DrawLayout
    MarkAsDirty
    
  Case lsExit
    'delete this exit
    DeleteExit Selection
    
    'deselect and redraw
    DeselectObj
    DrawLayout
    MarkAsDirty
  End Select
  
  SetEditMenu
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickFind()

  Dim tmpSel As TSel
  Dim tmpInfo As ObjInfo
  Dim blnSecond As Boolean
  Dim tmpScroll As Long
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  Select Case Selection.Type
  Case lsTransPt
    'switch to other leg
      'jump to other
      If Selection.Leg = 0 Then
        Selection.Leg = 1
      Else
        Selection.Leg = 0
      End If
    
      tmpInfo.Number = Selection.Number
      tmpInfo.Type = lsTransPt
      blnSecond = (Selection.Leg = 1)
      'if not on screen
      If Not ObjOnScreen(tmpInfo, blnSecond) Then
        'don't allow scrolling to redraw; it will be done manually after
        'scroll bars are repositioned
        blnDontDraw = True
        
        'adjust scroll values so object is centered in screen
        tmpScroll = -(picDraw.Width / 2 / DSF - TransPt(Selection.Number).Loc(Selection.Leg).X - RM_SIZE / 4) * 100
        If tmpScroll < HScroll1.Min Then
          tmpScroll = HScroll1.Min
        End If
        If tmpScroll > HScroll1.Max Then
          tmpScroll = HScroll1.Max
        End If
        HScroll1.Value = tmpScroll
        
        tmpScroll = -(picDraw.Height / 2 / DSF - TransPt(Selection.Number).Loc(Selection.Leg).Y - RM_SIZE / 4) * 100
        If tmpScroll < VScroll1.Min Then
          tmpScroll = VScroll1.Min
        End If
        If tmpScroll > VScroll1.Max Then
          tmpScroll = VScroll1.Max
        End If
        VScroll1.Value = tmpScroll
        
        'restore scrolling draw capability
        blnDontDraw = False
        'and force redraw
        DrawLayout
      End If
      
      tmpSel = Selection
      'deselect and reselect
      DeselectObj
      SelectObj tmpSel
      'refresh
      picDraw.Refresh
    
  Case lsExit
    'switch to other leg
    '*'Debug.Assert Selection.TwoWay = ltwOneWay
    
    'copy currently selected exit
    tmpSel = Selection
    'swap to/from room
    IsTwoWay tmpSel.Number, tmpSel.ExitID, tmpSel.Number, tmpSel.ExitID, 1
    'if part of a transfer
    If tmpSel.Leg <> llNoTrans Then
      If tmpSel.Leg = llFirst Then
        tmpSel.Leg = llSecond
      Else
        tmpSel.Leg = llFirst
      End If
    End If
    
    DeselectObj
    SelectExit tmpSel
    picDraw.Refresh
    
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub MenuClickInsert()

  'inserts a set of transpts in the selected exit
  
  '*'Debug.Assert Selection.Type = lsExit
  If Selection.Type <> lsExit Then
    Exit Sub
  End If
  '*'Debug.Assert Selection.Leg = llNoTrans
  If Selection.Leg <> llNoTrans Then
    Exit Sub
  End If
  InsertTransfer Selection
  
  DeselectObj
  SetEditMenu
  DrawLayout
End Sub


Public Sub MenuClickPrint()
  
  Dim i As Long, lngNum As Long
  
  'load the print form
  Load frmPrint
  
  frmPrint.txtLayoutScale.Text = CStr(CLng(PrintScale * 100))
  frmPrint.optSelObjs.Enabled = (Selection.Type <> lsNone)
  frmPrint.SetMode rtLayout, Nothing
  frmPrint.Show vbModal, frmMDIMain
  
  'if showing page boundaries
  If Settings.LEPages Then
    'always redraw after printing or previewing
    DrawLayout
  End If
End Sub

Public Sub MenuClickSelectAll()

  'select all objects
  
  'if there is currently a selection
  If Selection.Type <> lsNone Then
    DeselectObj
  End If
  
  'copy all by assigning objorder to selectedobjects
  SelectedObjects = ObjOrder
  
  Selection.Type = lsMultiple
  Selection.Number = ObjCount
  
  'reselect
  SelectObj Selection
  
  SetEditMenu
End Sub

Private Function CreateNewExit(ByVal FromRoom As Long, ByVal ToRoom As Long, ByVal Reason As EEReason) As String
    
  Dim i As Long, j As Long
  Dim tmpRoom As Long, tmpID As String
  Dim blnFoundID As Boolean, rtn As VbMsgBoxResult
  Dim blnDupeOK As Boolean
  
  'create new exit from new 'from' room to current 'to' room of same Type
  'returns the index of the new exit
  '
  '- transfer points can only support one exit from A to B
  '  and a single reciprocal exit from B to A
  '- transfers can't mix and match edge exits with 'other' exits
  
  'if exit is not created, return an empty string (so calling code can do error checking)
  
  On Error GoTo ErrHandler
  
  'first check is to see if an exit that matches the new exit (same code, same target)
  'step through all exits for this room
  For i = 0 To Exits(FromRoom).Count - 1
      With Exits(FromRoom)(i)
        Select Case .Status
        Case esDeleted 'check to see if we are restoring the deleted exit
          'if deleted, AND ToRoom is the OldToRoom, AND Reason is same,
          If .OldRoom = ToRoom And .Reason = Reason Then
            'restore the exit rather than create a new one
            .Status = esOK
            
            'return the identified exit
            CreateNewExit = Exits(FromRoom)(i).ID
            
            'make sure room is reset, if necessary
            .Room = .OldRoom
            
            'check for reciprocal exit to see if there might
            'be an eligible transfer pt between the two rooms
            If IsTwoWay(FromRoom, CreateNewExit, tmpRoom, tmpID, 2) Then
              'use this transfer for the restored exit
              .Transfer = Exits(tmpRoom)(tmpID).Transfer
              .Leg = 1
              '*'Debug.Assert TransPt(.Transfer).Count = 1
              TransPt(.Transfer).Count = 2
              TransPt(.Transfer).ExitID(1) = .ID
            End If
            
            'reposition (since rooms may have moved since it was deleted)
            SetExitPos FromRoom, CreateNewExit
            
            'mark as dirty
            MarkAsDirty
            Exit Function
          End If
          
        Case esOK, esNew, esChanged
          'is this a matching edge exit?
          '*'Debug.Assert Reason <> erNone
          'unknown ok; while it's not possible for user to create
          'new exit of 'unknown' Type, an existing 'unknown' exit
          'that has 'fromroom' changed will call this logic
          'in that case, we don't care if there's already an 'unknown'
          'exit from this room...
          If .Room = ToRoom And .Reason = Reason And Reason <> erOther Then
            'a duplicate entry- warn?
            If Not blnDupeOK Then
              rtn = MsgBox("There is already a '" & LogicSourceSettings.ReservedDefines(atNum)(Reason).Name & "' exit from " & Logics(FromRoom).ID & " to " & Logics(ToRoom).ID & vbCrLf & _
                           "Do you want to create a duplicate exit?", vbQuestion + vbYesNo, "Duplicate Exit")
              
              If rtn = vbNo Then
                Exit Function
              End If
            End If
            'hide duplicate warning in the event other exits also are a match
            blnDupeOK = True
          End If
        End Select
      End With
  Next i

  'no previous exit found; get a valid exit ID
  j = 0
  Do
    j = j + 1
    'step through all exits for this room
    For i = 0 To Exits(FromRoom).Count - 1
      'if there is already an exit with this id
      If Val(Right$(Exits(FromRoom)(i).ID, 3)) = j Then
        'this id is in use; exit for loop and try again with next id
        Exit For
      End If
    Next i
  Loop Until i = Exits(FromRoom).Count
  
  'add a new exit, using j as id
  CreateNewExit = "LE" & format$(j, "000")
  Exits(FromRoom).Add(j, ToRoom, Reason, 0, 0, 0).Status = esNew
  
  'check for reciprocal exit
  If IsTwoWay(FromRoom, CreateNewExit, tmpRoom, tmpID, 2) Then
    'use this transfer for the new exit
    With Exits(FromRoom)(CreateNewExit)
      .Transfer = Exits(tmpRoom)(tmpID).Transfer
      .Leg = 1
      TransPt(.Transfer).Count = 2
      TransPt(.Transfer).ExitID(1) = .ID
    End With
  End If
  
  'set end points
  SetExitPos FromRoom, CreateNewExit
  
  'mark as dirty
  MarkAsDirty
Exit Function

ErrHandler:
  Resume Next
End Function

Private Function NewExitText(NewExit As AGIExit, Optional ByVal LineCR As String = vbNewLine) As String

  'creates text for a new exit so it can be inserted in a logic
  Dim strNewRoom As String
  
  strNewRoom = "new.room("
  'line carriage return can only be vbCr or vbNewLine
  If LineCR <> vbNewLine And LineCR <> vbCr Then
    'use default
    LineCR = vbNewLine
  End If
  
  NewExitText = LineCR & "if ("
  If NewExit.Reason = erOther Then
    'convert 'unknown' Type exit to 'other' Type
    NewExit.Reason = erOther
    NewExitText = NewExitText & "condition == True)" & LineCR & Space$(Settings.LogicTabWidth) & "{" & Space$(Settings.LogicTabWidth) & LineCR & Space$(Settings.LogicTabWidth) & strNewRoom
  Else
    If LogicSourceSettings.UseReservedNames Then
      NewExitText = NewExitText & LogicSourceSettings.ReservedDefines(atVar)(2).Name & " == " & LogicSourceSettings.ReservedDefines(atNum)(NewExit.Reason).Name & ")" & LineCR & Space$(Settings.LogicTabWidth) & "{" & LineCR & Space$(Settings.LogicTabWidth) & strNewRoom
    Else
      NewExitText = NewExitText & "v2 == " & CStr(NewExit.Reason) & ")" & LineCR & Space$(Settings.LogicTabWidth) & "{" & LineCR & Space$(Settings.LogicTabWidth) & strNewRoom
    End If
  End If
  NewExitText = NewExitText & Logics(NewExit.Room).ID & ");  [ ##" & NewExit.ID & "##" & LineCR & Space$(Settings.LogicTabWidth) & "}" & LineCR

End Function

Private Function ObjOnScreen(ObjTest As ObjInfo, Optional ByVal SecondTrans As Boolean = False) As Boolean

  'returns true if any portion of the object is on the screen
  
  Dim tmpVal As Single
  Dim SP As LCoord, EP As LCoord
  Dim P1C1_X As Boolean, P1C2_X As Boolean
  Dim P2C1_X As Boolean, P2C2_X As Boolean
  Dim P1C1_Y As Boolean, P1C2_Y As Boolean
  Dim P2C1_Y As Boolean, P2C2_Y As Boolean
  Dim sngSlope As Single, blnSlope As Boolean
  
  
  Select Case ObjTest.Type
  Case lsRoom
    SP.X = (Room(ObjTest.Number).Loc.X + OffsetX) * DSF
    SP.Y = (Room(ObjTest.Number).Loc.Y + OffsetY) * DSF
    EP.X = (Room(ObjTest.Number).Loc.X + RM_SIZE + OffsetX) * DSF
    EP.Y = (Room(ObjTest.Number).Loc.Y + RM_SIZE + OffsetY) * DSF
    
  Case lsTransPt
    If SecondTrans Then
      SP.X = (TransPt(ObjTest.Number).Loc(1).X + OffsetX) * DSF
      SP.Y = (TransPt(ObjTest.Number).Loc(1).Y + OffsetY) * DSF
      EP.X = (TransPt(ObjTest.Number).Loc(1).X + RM_SIZE / 2 + OffsetX) * DSF
      EP.Y = (TransPt(ObjTest.Number).Loc(1).Y + RM_SIZE / 2 + OffsetY) * DSF
    Else
      SP.X = (TransPt(ObjTest.Number).Loc(0).X + OffsetX) * DSF
      SP.Y = (TransPt(ObjTest.Number).Loc(0).Y + OffsetY) * DSF
      EP.X = (TransPt(ObjTest.Number).Loc(0).X + RM_SIZE / 2 + OffsetX) * DSF
      EP.Y = (TransPt(ObjTest.Number).Loc(0).Y + RM_SIZE / 2 + OffsetY) * DSF
    End If
    
  Case lsErrPt
    SP.X = (ErrPt(ObjTest.Number).Loc.X + OffsetX) * DSF
    SP.Y = (ErrPt(ObjTest.Number).Loc.Y + OffsetY) * DSF
    EP.X = (ErrPt(ObjTest.Number).Loc.X + RM_SIZE * 0.75 + OffsetX) * DSF
    EP.Y = (ErrPt(ObjTest.Number).Loc.Y + RM_SIZE / 2 + OffsetY) * DSF
    
  Case lsComment
    SP.X = (Comment(ObjTest.Number).Loc.X + OffsetX) * DSF
    SP.Y = (Comment(ObjTest.Number).Loc.Y + OffsetY) * DSF
    EP.X = (Comment(ObjTest.Number).Loc.X + Comment(ObjTest.Number).Size.X + OffsetX) * DSF
    EP.Y = (Comment(ObjTest.Number).Loc.Y + Comment(ObjTest.Number).Size.Y + OffsetY) * DSF
    
  End Select
  
  
  'if ending point is >=0 and starting point <=width/height
  ObjOnScreen = (EP.X >= 0 And EP.Y >= 0 And SP.X <= picDraw.Width And SP.Y <= picDraw.Height)
End Function

Private Function LineOnScreen(ByVal X1 As Single, ByVal Y1 As Single, ByVal X2 As Single, ByVal Y2 As Single) As Boolean

  'will determine if any points on the line are located on screen
  
  '        |     |
  '     0  |  1  |  2
  '   -----+-----+-----
  '        |     |
  '     3  |  4  |  5
  '   -----+-----+-----
  '        |     |
  '     6  |  7  |  8
  '
  'the screen is indicated by element #4
  'all lines can be represented by having a startpoint and endpoint
  'in one of the nine areas; therefore, there are 45 possible combinations
  'a combinatorial thing - count it out if you doubt
  
  Dim P1C1_X As Boolean, P1C2_X As Boolean
  Dim P2C1_X As Boolean, P2C2_X As Boolean
  Dim P1C1_Y As Boolean, P1C2_Y As Boolean
  Dim P2C1_Y As Boolean, P2C2_Y As Boolean
  Dim sngSlope As Single, blnSlope As Boolean
  Dim tmpVal As Single
  
  'convert line coordinates into screen coordinates
  X1 = (X1 + OffsetX) * DSF
  X2 = (X2 + OffsetX) * DSF
  Y1 = (Y1 + OffsetY) * DSF
  Y2 = (Y2 + OffsetY) * DSF
  
  'use flags to indicate if endpoints are on box side of box corners
  P1C1_X = (X1 >= 0)
  P1C2_X = (X1 <= picDraw.Width)
  P2C1_X = (X2 >= 0)
  P2C2_X = (X2 <= picDraw.Width)
  P1C1_Y = (Y1 >= 0)
  P1C2_Y = (Y1 <= picDraw.Height)
  P2C1_Y = (Y2 >= 0)
  P2C2_Y = (Y2 <= picDraw.Height)
  
  'if either endpoint is in the box, then the line is in the box (accounts for 9 out of 45)
  If ((P1C1_X And P1C2_X) And (P1C1_Y And P1C2_Y)) Then
    LineOnScreen = True
    Exit Function
  End If
  If ((P2C1_X And P2C2_X) And (P2C1_Y And P2C2_Y)) Then
    LineOnScreen = True
    Exit Function
  End If
  
  'if line is completly above, below, to right or to left of box (accounts for 20 out of 45)
  If Not P1C1_X And Not P2C1_X Then
    'not on screen; return false
    Exit Function
  End If
  If Not P1C2_X And Not P2C2_X Then
    'not on screen; return false
    Exit Function
  End If
  If Not P1C1_Y And Not P2C1_Y Then
    'not on screen; return false
    Exit Function
  End If
  If Not P1C2_Y And Not P2C2_Y Then
    'not on screen; return false
    Exit Function
  End If
  
  'if line goes across box horizontally or vertically (accounts for 2 out of 45)
  If (P1C1_X = P2C1_X And P1C2_X = P2C2_X) Then
    LineOnScreen = True
    Exit Function
  End If
  If (P1C1_Y = P2C1_Y And P1C2_Y = P2C2_Y) Then
    LineOnScreen = True
    Exit Function
  End If
  
  'for rest, if all four corner points lie on same side of line,
  'line is outside; otherwise line is inside
  'test by comparing slopes of lines from origin to corners; if all four corners are on
  'same side, the slopes will be all less than or all greater
  'than the slope of the test line
  
  'if point 1 is between box sides,
  If P1C1_X And P1C2_X Then
    'swap points so slopes are measured against the second point
    tmpVal = X1
    X1 = X2
    X2 = tmpVal
    tmpVal = Y1
    Y1 = Y2
    Y2 = tmpVal
  End If
  
  'slope of line (X2 <> X1 because of previous filtering)
  sngSlope = (Y2 - Y1) / (X2 - X1)
  
  '0 <> X1 and picdraw.width <> X1 because line can't be vertical
  '(X1 <> X2; and if original X1 = 0, it is swapped
  
  'determine if slope of line from first point to first corner
  'is greater than or less than slope of line being tested
  '(if line does not cross box, then slope of line from starting point
  'to each corner will be less than all of them or greater than all of them)
  blnSlope = ((0 - Y1) / (0 - X1) > sngSlope)
  
  'compare to other three corners
  If ((picDraw.Height - Y1) / (0 - X1) > sngSlope) <> blnSlope Then
    'different signs means on screen
    LineOnScreen = True
    Exit Function
  End If
  
  If ((0 - Y1) / (picDraw.Width - X1) > sngSlope) <> blnSlope Then
    'different signs means on screen
    LineOnScreen = True
    Exit Function
  End If

  If ((picDraw.Height - Y1) / (picDraw.Width - X1) > sngSlope) <> blnSlope Then
    'different signs means on screen
    LineOnScreen = True
    Exit Function
  End If

  'not on screen
End Function

Public Sub ChangeScale(ByVal Dir As Long)
  
  'adjusts scale and redraws layout
  
  Dim NewScale As Long, NewDSF As Single
  Dim ptCursor As POINTAPI
  
  Select Case Dir
  Case Is < 0
    NewScale = DrawScale - 1
    If NewScale = 0 Then NewScale = 1

  Case Is > 0
    NewScale = DrawScale + 1
    If NewScale > 9 Then NewScale = 9
  End Select

  If NewScale <> DrawScale Then
    'we need cursor coordinates so we can scale the surface
    'over the correct point
    ptCursor = GetZoomCenter()
    
    'calculate new DSF value
    NewDSF = 40 * 1.25 ^ (NewScale - 1)
    
    'calculate new offset values
    OffsetX = OffsetX + ptCursor.X / NewDSF - ptCursor.X / DSF
    OffsetY = OffsetY + ptCursor.Y / NewDSF - ptCursor.Y / DSF
    
    'now update scale and scale factor
    DrawScale = NewScale
    DSF = NewDSF
    
    picDraw.Font.Size = DSF / 10
    txtComment.Font.Size = DSF / 10

    MainStatusBar.Panels("Scale").Text = "Scale: " & CStr(DrawScale)
    
    'redraw by forcing resize event
    picDraw_Resize
  End If
   
End Sub

Private Sub DeselectObj()

  Dim rtn As Long
  
  Select Case Selection.Type
  Case lsNone
    'no action needed
    
  Case lsMultiple
    'hide selection box
    shpMove.Visible = False
    
    'redraw without making selection
    DrawLayout False
  
  Case Else
    'copy area under selection handles back to main bitmap
    
    'all selections use at least two handles
    rtn = BitBlt(picDraw.hDC, Selection.X1, Selection.Y1, 8, 8, picHandle.hDC, 0, 0, SRCCOPY)
    rtn = BitBlt(picDraw.hDC, Selection.X2, Selection.Y2, 8, 8, picHandle.hDC, 24, 0, SRCCOPY)
    
    'if not an exit
    If Selection.Type <> lsExit Then
      'reset the other two handles, too
      rtn = BitBlt(picDraw.hDC, Selection.X1, Selection.Y2, 8, 8, picHandle.hDC, 16, 0, SRCCOPY)
      rtn = BitBlt(picDraw.hDC, Selection.X2, Selection.Y1, 8, 8, picHandle.hDC, 8, 0, SRCCOPY)
    Else
      'if one direction, AND exit is two way
      If Selection.TwoWay = ltwOneWay Then
        'add third handle by arrowhead
        rtn = BitBlt(picDraw.hDC, Selection.X3, Selection.Y3, 8, 8, picHandle.hDC, 8, 0, SRCCOPY)
      End If
    End If
    
    'refresh drawing surface
    picDraw.Refresh
  End Select
  
  'reset selection variables
  Selection.Number = 0
  Selection.Type = lsNone
  Selection.ExitID = vbNullString
  Selection.Leg = llNoTrans
  
  'disable toolbar buttons
  With Toolbar1.Buttons
    .Item("delete").Enabled = False
    .Item("transfer").Enabled = False
    .Item("hide").Enabled = False
    .Item("front").Enabled = False
    .Item("back").Enabled = False
  End With
  
  'reset statusbar
  If MainStatusBar.Tag <> CStr(rtLayout) Then
    AdjustMenus rtLayout, True, True, IsDirty
  End If
  
  With MainStatusBar.Panels
    .Item("Room1").Text = vbNullString
    .Item("Room2").Text = vbNullString
    .Item("Type").Text = vbNullString
    .Item("ID").Text = vbNullString
  End With
    
End Sub

Private Sub DrawArrow(ByVal SPX As Single, ByVal SPY As Single, ByVal EPX As Single, ByVal EPY As Single, ByVal ArrowColor As Long)

  'draws an arrowhead at ending point (EP) that is oriented based on the
  'line from starting point (SP) to ending point
  
  'without getting into the derivation, it can be shown that the two points at the base of
  'the arrowhead are:
  '
  ' xc = xe + Sgn(DX) * (Length/Sqr(m^2 + 1)) * (1 +/- m * tanTheta)
  ' yc = ye + Sgn(DY) * (Length/Sqr(m^2 + 1)) * (m +/- tanTheta)
  '
  'the first +/- is determined by sign of DX/DY
  'Length is length of arrowhead; tanTheta is tangent of arrowhead angle
  
  Dim m As Single, DX As Single, DY As Single
  Dim v(2) As POINTAPI
  Dim ldivs As Single, oldStyle As Long
  
  Const Length = 0.2
  Const tanTheta = 0.25
  
  'horizontal and vertical distances:
  DY = EPY - SPY
  DX = EPX - SPX
  
  If DX = 0 And DY = 0 Then
    Exit Sub
  End If
  
  'set width to 1
  picDraw.DrawWidth = 1
  'save style
  oldStyle = picDraw.DrawStyle
  
  'end point is first point of arrow region
  v(0).X = (EPX + OffsetX) * DSF
  v(0).Y = (EPY + OffsetY) * DSF
  
  'slope of line determines how to draw the arrow
  If Abs(DY) > Abs(DX) Then
    'mostly vertical line
    '(swap x and y formulas)
    'slope of line
    m = DX / DY
    'calculate first term (to save on cpu times by only doing the math once)
    ldivs = Sgn(DY) * Length / Sqr(m ^ 2 + 1)
    
    v(1).X = (EPX - ldivs * (m + tanTheta) + OffsetX) * DSF
    v(2).X = (EPX - ldivs * (m - tanTheta) + OffsetX) * DSF
    v(2).Y = (EPY - ldivs * (1 + m * tanTheta) + OffsetY) * DSF
    v(1).Y = (EPY - ldivs * (1 - m * tanTheta) + OffsetY) * DSF
  Else
    'mostly horizontal line
    
    'slope of line
    m = DY / DX
    'calculate first term (to save on cpu times by only doing the math once)
    ldivs = Sgn(DX) * Length / Sqr(m ^ 2 + 1)
    v(1).X = (EPX - ldivs * (1 + m * tanTheta) + OffsetX) * DSF
    v(2).X = (EPX - ldivs * (1 - m * tanTheta) + OffsetX) * DSF
    v(2).Y = (EPY - ldivs * (m + tanTheta) + OffsetY) * DSF
    v(1).Y = (EPY - ldivs * (m - tanTheta) + OffsetY) * DSF
  End If
  
  'draw the arrow
  Polygon picDraw.hDC, v(0), 3
  
  'restore drawwidth and style
  picDraw.DrawWidth = 2
  picDraw.DrawStyle = oldStyle
End Sub


Private Sub DrawCmtBox(ByVal CmtID As Long)

  'draws a rounded corner rectangle; converts layout coordinates into drawing surface pixel coordinates
  
  
  Dim rtn As Long, hBrush As Long, hRgn As Long
  Dim i As Long, j As Long, sngTH As Single
  Dim strLine As String
  
  'if comment not visible
  If Not Comment(CmtID).Visible Then
    Exit Sub
  End If
  
  'create region
  hRgn = CreateRoundRectRgn((Comment(CmtID).Loc.X + OffsetX) * DSF - 1, _
                            (Comment(CmtID).Loc.Y + OffsetY) * DSF - 1, _
                            (Comment(CmtID).Loc.X + Comment(CmtID).Size.X + OffsetX) * DSF + 2, _
                            (Comment(CmtID).Loc.Y + Comment(CmtID).Size.Y + OffsetY) * DSF + 2, 12, 12)
  
  'create brush
  hBrush = CreateSolidBrush(Settings.LEColors.Cmt.Fill)
  
  'fill region
  rtn = FillRgn(picDraw.hDC, hRgn, hBrush)
  
  'delete fill brush; create edge brush
  rtn = DeleteObject(hBrush)
  hBrush = CreateSolidBrush(Settings.LEColors.Cmt.Edge)
  
  'draw outline
  rtn = FrameRgn(picDraw.hDC, hRgn, hBrush, 2, 2)
  
  'delete brush and region
  rtn = DeleteObject(hBrush)
  rtn = DeleteObject(hRgn)
  
  If Trim$(Comment(CmtID).Text) = vbNullString Then
    Exit Sub
  End If
  
  'use text box to draw text on
  txtComment.Width = Comment(CmtID).Size.X * DSF - 12
  txtComment.Height = Comment(CmtID).Size.Y * DSF - 8
  txtComment.Text = Comment(CmtID).Text
  
  'now copy lines onto drawing surface
    
  'get line Count
  j = SendMessage(txtComment.hWnd, EM_GETLINECOUNT, 0, 0)
  
  'text height
  sngTH = picDraw.TextHeight("Ag")
  picDraw.ForeColor = Settings.LEColors.Cmt.Edge
  
  For i = 0 To j - 1
    'get index of beginning of this line
    rtn = SendMessage(txtComment.hWnd, EM_LINEINDEX, i, 0)
    'get length of this line
    rtn = SendMessage(txtComment.hWnd, EM_LINELENGTH, rtn, 0)
    'get text of this line
    strLine = ChrW$(rtn And &HFF) & ChrW$(rtn \ &H100) & String$(rtn, 32)
    rtn = SendMessageByString(txtComment.hWnd, EM_GETLINE, i, strLine)
    strLine = Replace(strLine, ChrW$(0), vbNullString)
    strLine = RTrim$(strLine)
    '*'Debug.Assert InStr(1, strLine, ChrW$(0)) = 0
    'print the line
    picDraw.CurrentX = (Comment(CmtID).Loc.X + OffsetX) * DSF + 6
    picDraw.CurrentY = (Comment(CmtID).Loc.Y + OffsetY) * DSF + 4 + i * sngTH
    'if not enough room vertically (meaning text would extend below bottom edge of comment box)
    If picDraw.CurrentY + sngTH > (Comment(CmtID).Loc.Y + Comment(CmtID).Size.Y + OffsetY) * DSF - 4 Then
      Exit For
    End If
    picDraw.Print strLine
  Next i
End Sub

Private Sub ObjectFromPos(ByRef NewSel As TSel, X As Single, Y As Single)

  'called from picDraw_MouseDown and the tip timer
  'if an object is found under the cursor,
  'NewSel is populated with the object info
  
  Dim i As Long
  Dim tmpX As Single, tmpY As Single
  
  'default to nothing selected
  NewSel.Type = lsNone
  
  For i = ObjCount - 1 To 0 Step -1
    'if over this object
    Select Case ObjOrder(i).Type
    Case lsRoom
      'if object surrounds point
      tmpX = (Room(ObjOrder(i).Number).Loc.X + OffsetX) * DSF
      tmpY = (Room(ObjOrder(i).Number).Loc.Y + OffsetY) * DSF
      If X >= tmpX And X <= tmpX + RM_SIZE * DSF And Y >= tmpY And Y <= tmpY + RM_SIZE * DSF Then
        'found it
        NewSel.Number = ObjOrder(i).Number
        NewSel.Type = lsRoom
        Exit Sub
      End If
      
    Case lsTransPt
        'if object surrounds point
        tmpX = (TransPt(ObjOrder(i).Number).Loc(0).X + OffsetX) * DSF
        tmpY = (TransPt(ObjOrder(i).Number).Loc(0).Y + OffsetY) * DSF
        If X >= tmpX And X <= tmpX + RM_SIZE / 2 * DSF And Y >= tmpY And Y <= tmpY + RM_SIZE / 2 * DSF Then
          'found it
          NewSel.Number = ObjOrder(i).Number
          'use leg to hold index of loc and room
          NewSel.Leg = 0
          NewSel.Type = lsTransPt
          Exit Sub
        End If
        tmpX = (TransPt(ObjOrder(i).Number).Loc(1).X + OffsetX) * DSF
        tmpY = (TransPt(ObjOrder(i).Number).Loc(1).Y + OffsetY) * DSF
        If X >= tmpX And X <= tmpX + RM_SIZE / 2 * DSF And Y >= tmpY And Y <= tmpY + RM_SIZE / 2 * DSF Then
          'found it
          NewSel.Number = ObjOrder(i).Number
          'use leg to hold index of loc and room
          NewSel.Leg = 1
          NewSel.Type = lsTransPt
          Exit Sub
        End If
    
    Case lsComment
      'if object surrounds point
      tmpX = (Comment(ObjOrder(i).Number).Loc.X + OffsetX) * DSF
      tmpY = (Comment(ObjOrder(i).Number).Loc.Y + OffsetY) * DSF
      If X >= tmpX And X <= tmpX + Comment(ObjOrder(i).Number).Size.X * DSF And Y >= tmpY And Y <= tmpY + Comment(ObjOrder(i).Number).Size.Y * DSF Then
        'found it
        NewSel.Number = ObjOrder(i).Number
        NewSel.Type = lsComment
        Exit Sub
      End If
    Case lsErrPt
      'if object surrounds point
      tmpX = (ErrPt(ObjOrder(i).Number).Loc.X + OffsetX) * DSF
      tmpY = (ErrPt(ObjOrder(i).Number).Loc.Y + OffsetY) * DSF
      If X >= tmpX And X <= tmpX + 0.6 * DSF And Y >= tmpY And Y <= tmpY + RM_SIZE / 2 * DSF Then
        'found it
        NewSel.Number = ObjOrder(i).Number
        NewSel.Type = lsErrPt
        Exit Sub
      End If
    End Select
  Next i
End Sub

Private Sub SetEditMenu()

  Dim blnRoomSel As Boolean, i As Long
  
  'adjust menus based on current selection
  With frmMDIMain
    'undo, redo, ebar0, paste, clear, find next and replace ALWAYS hidden
    .mnuEBar2.Visible = True 'False
    .mnuEUndo.Visible = False
    .mnuERedo.Visible = False
    .mnuEBar0.Visible = False
    .mnuEPaste.Visible = False
    .mnuEClear.Visible = False
    .mnuEFindAgain.Visible = False
    .mnuEReplace.Visible = False
    .mnuECustom3.Enabled = False
    
    'select all is always visible and enabled
    .mnuESelectAll.Visible = True
    .mnuESelectAll.Enabled = True
    .mnuESelectAll.Caption = "Select &All" & vbTab & "Ctrl+A"
    
    'toggle-draw-pic is only enabled if a room is selected
    'properties is only enabled if a room is selected
    .mnuECustom1.Visible = True
    .mnuECustom2.Visible = True
    .mnuECustom2.Caption = "Room Properties" & vbTab & "Ctrl+D"
    .mnuECustom1.Enabled = (Selection.Type = lsRoom)
    .mnuECustom2.Enabled = (Selection.Type = lsRoom)
    If Selection.Type = lsRoom Then
      If Room(Selection.Number).ShowPic Then
        .mnuECustom1.Caption = "Hide Room Picture" & vbTab & "Ctrl+R"
      Else
        .mnuECustom1.Caption = "Show Room Picture" & vbTab & "Ctrl+R"
      End If
    Else
      .mnuECustom1.Caption = "Show Room Picture" & vbTab & "Ctrl+R"
    End If
    
    'global draw-pic always available
    If Settings.LEShowPics Then
      .mnuRCustom2.Caption = "Hide All &Pics" & vbTab & "Ctrl+Alt+H"
    Else
      .mnuRCustom2.Caption = "Show All &Pics" & vbTab & "Ctrl+Alt+S"
    End If
    
    'cut is visible if selection is a room, or none
    .mnuECut.Visible = (Selection.Type = lsRoom Or Selection.Type = lsNone)
    .mnuECut.Enabled = True
    If Selection.Type = lsRoom Then
      .mnuECut.Caption = "&Hide Room" & vbTab & "Shift+Ctrl+H"
    Else
      .mnuECut.Caption = "&Show Room" & vbTab & "Shift+Ctrl+S"
    End If
    
    'copy is always visible, but only enabled for rooms, exits, errpts
    .mnuECopy.Visible = True
    .mnuECopy.Enabled = (Selection.Type = lsRoom Or Selection.Type = lsErrPt Or Selection.Type = lsExit)
    .mnuECopy.Caption = "Edit &Logic" & vbTab & "Alt+L"
    
    'delete is always visible, but disabled if nothing is selected
    .mnuEDelete.Visible = True
    .mnuEDelete.Enabled = (Selection.Type <> lsNone)
    .mnuEDelete.Caption = "&Delete " & vbTab & "Del"
    
    'insert is always visible, but only enabled for exits
    'that DONT already have a transfer
    .mnuEInsert.Visible = True
    .mnuEInsert.Enabled = (Selection.Type = lsExit And Selection.Leg = llNoTrans)
    .mnuEInsert.Caption = "&Insert Transfer" & vbTab & "Shift+Ins"
    
    'find is only visible if a trans pt or exit is selected
    .mnuEFind.Visible = (Selection.Type = lsTransPt Or (Selection.Type = lsExit And Selection.TwoWay = ltwOneWay))
    .mnuEFind.Enabled = True
    If Selection.Type = lsTransPt Then
      .mnuEFind.Caption = "Jump to &Other Leg" & vbTab & "Alt+O"
    Else
      .mnuEFind.Caption = "Select &Other Direction" & vbTab & "Alt+O"
    End If
    'separator only visible if item is visible
    .mnuEBar1.Visible = .mnuEFind.Visible
  End With
End Sub
Private Sub ShowLogic(tmpSel As TSel)

  Dim i As Long, j As Long
  Dim strUpdate As String
  
  On Error GoTo ErrHandler
  
  'determine if any exits need updating
  For j = 0 To Exits(tmpSel.Number).Count - 1
    'if this exit status is not ok
    If Exits(tmpSel.Number)(j).Status <> esOK Then
    
      'update all exits for this logic
      UpdateLogicCode Logics(tmpSel.Number)
      
      'update the layout file
      UpdateLayoutFile euUpdateRoom, tmpSel.Number, Exits(tmpSel.Number)
      
      Exit For
    End If
  Next j
  
  'if errpt
  If tmpSel.Type = lsErrPt Then
    'open 'from' logic
    OpenLogic ErrPt(Selection.Number).FromRoom
    
    'find and highlight the errpt exit
    With frmMDIMain.ActiveForm.rtfLogic.Selection.Range
      .FindText "##" & ErrPt(tmpSel.Number).ExitID & "##"
      .StartOf reLine, True
      .EndOf reLine, True
    End With
    
  Else
    'open logic for editing
    OpenLogic Selection.Number
    
    'if editing an exit
    If tmpSel.Type = lsExit Then
      'then find and highlight this exit
      With frmMDIMain.ActiveForm.rtfLogic
        On Error Resume Next
        .Range.FindTextRange("##" & tmpSel.ExitID & "##").SelectRange
        .Selection.Range.StartOf reLine, True
        .Selection.Range.EndOf reLine, True
      End With
    End If
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function PointOnArrow(ByVal X As Single, ByVal Y As Single, ByVal SPX As Single, ByVal SPY As Single, ByVal EPX As Single, ByVal EPY As Single) As Boolean

  'use PtInRgn api to determine if an arrow is clicked or not
  
  Dim m As Single, ldivs As Single
  Dim DX As Single, DY As Single
  Dim v(2) As POINTAPI, rtn As Long
  Dim hRgn As Long
  
  Const Length = 0.2
  Const tanTheta = 0.25
    
  
  'horizontal and vertical distances:
  DY = EPY - SPY
  DX = EPX - SPX

  If DX = 0 And DY = 0 Then
    Exit Function
  End If

  'set point of arrow at end of line
  v(0).X = (EPX + OffsetX) * DSF
  v(0).Y = (EPY + OffsetY) * DSF
  
  'slope of line determines how to draw the arrow
  If Abs(DY) > Abs(DX) Then
    'mostly vertical line
    '(swap x and y formulas)
    'slope of line
    m = DX / DY
    'calculate first term (to save on cpu times by only doing the math once)
    ldivs = Sgn(DY) * Length / Sqr(m ^ 2 + 1)

  
    v(1).X = (EPX - ldivs * (m + tanTheta) + OffsetX) * DSF
    v(2).X = (EPX - ldivs * (m - tanTheta) + OffsetX) * DSF
    v(2).Y = (EPY - ldivs * (1 + m * tanTheta) + OffsetY) * DSF
    v(1).Y = (EPY - ldivs * (1 - m * tanTheta) + OffsetY) * DSF
  Else
    'mostly horizontal line

    'slope of line
    m = DY / DX
    'calculate first term (to save on cpu times by only doing the math once)
    ldivs = Sgn(DX) * Length / Sqr(m ^ 2 + 1)
    v(1).X = (EPX - ldivs * (1 + m * tanTheta) + OffsetX) * DSF
    v(2).X = (EPX - ldivs * (1 - m * tanTheta) + OffsetX) * DSF
    v(2).Y = (EPY - ldivs * (m + tanTheta) + OffsetY) * DSF
    v(1).Y = (EPY - ldivs * (m - tanTheta) + OffsetY) * DSF
  End If

  'create region
  hRgn = CreatePolygonRgn(v(0), 3, ALTERNATE)

  'check if point is in region
  PointOnArrow = (PtInRegion(hRgn, X, Y))

  'Delete region
  rtn = DeleteObject(hRgn)
End Function

Private Function PointOnLine(ByVal X As Single, ByVal Y As Single, ByVal SPX As Single, ByVal SPY As Single, ByVal EPX As Single, ByVal EPY As Single) As Boolean
  
  'use PtInRgn api to determine if a line is clicked or not
  'Path functions are used to convert a line into a region
  
  Dim rtn As Long, hRgn As Long
  
  'move point to start
  MoveToEx picDraw.hDC, (SPX + OffsetX) * DSF, (SPY + OffsetY) * DSF, 0
  
  'begin path
  rtn = BeginPath(picDraw.hDC)
  
  'createline
  rtn = LineTo(picDraw.hDC, (EPX + OffsetX) * DSF, (EPY + OffsetY) * DSF)
  
  'end path
  rtn = EndPath(picDraw.hDC)
  
  'widen it so thick lines will get selected
  rtn = WidenPath(picDraw.hDC)
  
  'convert the path into a region
  hRgn = PathToRegion(picDraw.hDC)
  
  'if line found
  PointOnLine = CBool(PtInRegion(hRgn, X, Y))

  'delete region
  rtn = DeleteObject(hRgn)
End Function

Private Sub CompactObjList(ByVal ObjNum As Long)
  'removes objnum from the list of objects
  'and slides the rest down
  'decrements objcount
  
  Dim i As Long
  
  On Error GoTo ErrHandler
  
  For i = ObjNum To ObjCount - 2
    'adjust object order number
    Select Case ObjOrder(i + 1).Type
    Case lsRoom
      Room(ObjOrder(i + 1).Number).Order = i
    Case lsTransPt
      TransPt(ObjOrder(i + 1).Number).Order = i
    Case lsErrPt
      ErrPt(ObjOrder(i + 1).Number).Order = i
    Case lsComment
      Comment(ObjOrder(i + 1).Number).Order = i
    End Select
    'move pointer down one
    ObjOrder(i) = ObjOrder(i + 1)
  Next i
  ObjCount = ObjCount - 1
  '*'Debug.Assert ObjCount >= 0
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub DeleteTransfer(ByVal TransNum As Long)
  
  On Error GoTo ErrHandler
  
  'remove transpt
  With TransPt(TransNum)
    'remove transfer from exit objects
    Exits(.Room(0))(.ExitID(0)).Transfer = 0
    '*'Debug.Assert Exits(.Room(0))(.ExitID(0)).Leg = 0
    
    If .Count = 2 Then
      Exits(.Room(1))(.ExitID(1)).Transfer = 0
      '*'Debug.Assert Exits(.Room(1))(.ExitID(1)).Leg = 1
      Exits(.Room(1))(.ExitID(1)).Leg = 0
    End If
    
    'set Count to zero so transpt won't be used during reposition
    .Count = 0
    RepositionRoom .Room(0)
    
    
    .ExitID(0) = vbNullString
    .ExitID(1) = vbNullString
    .Room(0) = 0
    .Room(1) = 0
    CompactObjList .Order
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function SameAsSelection(ByRef tmpSel As TSel) As Boolean

  'compares tmpSel with current selection object; if all elements are equal, returns true
  '(x and y values are not part of the check)
  
  With Selection
    SameAsSelection = (.Type = tmpSel.Type) And _
                      (.Number = tmpSel.Number) And _
                      (.ExitID = tmpSel.ExitID) And _
                      (.Leg = tmpSel.Leg) And _
                      (.TwoWay = tmpSel.TwoWay) And _
                      (.Point = tmpSel.Point)
  End With
End Function

Private Sub SelectObj(ByRef NewSel As TSel)
  
  Dim rtn As Long, i As Long
  Dim tmpLoc As LCoord, tmpSize As LCoord
  Dim MinX As Single, MinY As Single
  Dim MaxX As Single, MaxY As Single
  
  On Error GoTo ErrHandler
  
  Selection = NewSel
  
  '*'Debug.Assert NewSel.Number <> 0
  '*'Debug.Assert NewSel.Type <> lsNone
  
  picDraw.DrawWidth = 1

  'enable delete buttons
  Toolbar1.Buttons.Item("delete").Enabled = True
  
  'if NOT scrolling
  If Not tmrScroll.Enabled Then
    'hide selection outline for now
    shpMove.Visible = False
  End If
  
  'set status bar
  If MainStatusBar.Tag <> CStr(rtLayout) Then
    AdjustMenus rtLayout, True, True, IsDirty
  End If
  
  With Selection
    Select Case .Type
    Case lsRoom
      'save bitmaps under selection handles
      .X1 = (Room(.Number).Loc.X + OffsetX) * DSF - 8
      .Y1 = (Room(.Number).Loc.Y + OffsetY) * DSF - 8
      .X2 = .X1 + RM_SIZE * DSF + 8
      .Y2 = .Y1 + RM_SIZE * DSF + 8
        
      MainStatusBar.Panels("Room1").Text = Logics(.Number).ID
      MainStatusBar.Panels("ID").Text = vbNullString
      
    Case lsTransPt
      .X1 = (TransPt(.Number).Loc(.Leg).X + OffsetX) * DSF - 8
      .Y1 = (TransPt(.Number).Loc(.Leg).Y + OffsetY) * DSF - 8
      .X2 = .X1 + RM_SIZE / 2 * DSF + 8
      .Y2 = .Y1 + RM_SIZE / 2 * DSF + 8
     
      MainStatusBar.Panels("Room1").Text = Logics(TransPt(.Number).Room(0)).ID
      MainStatusBar.Panels("Room2").Text = Logics(TransPt(.Number).Room(1)).ID
      MainStatusBar.Panels("ID").Text = vbNullString
    
    Case lsComment
      .X1 = (Comment(.Number).Loc.X + OffsetX) * DSF - 8
      .Y1 = (Comment(.Number).Loc.Y + OffsetY) * DSF - 8
      .X2 = .X1 + Comment(.Number).Size.X * DSF + 8
      .Y2 = .Y1 + Comment(.Number).Size.Y * DSF + 8
      
    Case lsErrPt
      .X1 = (ErrPt(.Number).Loc.X + OffsetX) * DSF - 8
      .Y1 = (ErrPt(.Number).Loc.Y + OffsetY) * DSF - 8
      .X2 = .X1 + RM_SIZE * 0.75 * DSF + 8
      .Y2 = .Y1 + RM_SIZE / 2 * DSF + 8
    
      MainStatusBar.Panels("ID").Text = vbNullString
      If Logics.Exists(ErrPt(.Number).Room) And ErrPt(.Number).Room > 0 Then
        MainStatusBar.Panels("Room2").Text = "To: " & Logics(ErrPt(.Number).Room).ID
      Else
        MainStatusBar.Panels("Room2").Text = "To: {error}"
      End If
    Case lsMultiple
      'set min to right/bottom of visible drawing area
      MinX = picDraw.Width / DSF - OffsetX
      MinY = picDraw.Height / DSF - OffsetY
      MaxX = -OffsetX
      MaxY = -OffsetY
      
      MainStatusBar.Panels("ID").Text = "Multiple"
      MainStatusBar.Panels("Room1").Text = vbNullString
      MainStatusBar.Panels("Room2").Text = vbNullString
      
      
      'step through all selected objects and draw handles
      For i = 0 To Selection.Number - 1
        With SelectedObjects(i)
          Select Case .Type
          Case lsRoom
            tmpLoc = Room(.Number).Loc
            tmpSize.X = RM_SIZE
            tmpSize.Y = RM_SIZE
            
          Case lsTransPt
            'first or second leg?
            If Sgn(.Number) > 0 Then
              'first leg
              tmpLoc = TransPt(.Number).Loc(0)
            Else
              'second leg
              tmpLoc = TransPt(-1 * .Number).Loc(1)
            End If
            tmpSize.X = RM_SIZE / 2
            tmpSize.Y = RM_SIZE / 2
            
          Case lsErrPt
            tmpLoc = ErrPt(.Number).Loc
            tmpSize.X = RM_SIZE * 0.75
            tmpSize.Y = RM_SIZE / 2
            
          Case lsComment
            tmpLoc = Comment(.Number).Loc
            tmpSize = Comment(.Number).Size
        
          End Select
        End With
        'now draw handles
        picDraw.Line ((tmpLoc.X + OffsetX) * DSF - 8, (tmpLoc.Y + OffsetY) * DSF - 8)-Step(7, 7), vbBlack, BF
        picDraw.Line ((tmpLoc.X + tmpSize.X + OffsetX) * DSF, (tmpLoc.Y + OffsetY) * DSF - 8)-Step(7, 7), vbBlack, BF
        picDraw.Line ((tmpLoc.X + OffsetX) * DSF - 8, (tmpLoc.Y + tmpSize.Y + OffsetY) * DSF)-Step(7, 7), vbBlack, BF
        picDraw.Line ((tmpLoc.X + tmpSize.X + OffsetX) * DSF, (tmpLoc.Y + tmpSize.Y + OffsetY) * DSF)-Step(7, 7), vbBlack, BF
        
        'set min and Max
        If tmpLoc.X < MinX Then
          MinX = tmpLoc.X
        End If
        If tmpLoc.Y < MinY Then
          MinY = tmpLoc.Y
        End If
        If tmpLoc.X + tmpSize.X > MaxX Then
          MaxX = tmpLoc.X + tmpSize.X
        End If
        If tmpLoc.Y + tmpSize.Y > MaxY Then
          MaxY = tmpLoc.Y + tmpSize.Y
        End If
      Next i
      
      'set extents of selection
      With NewSel
        .X1 = (MinX + OffsetX) * DSF
        .X2 = (MaxX + OffsetX) * DSF
        .Y1 = (MinY + OffsetY) * DSF
        .Y2 = (MaxY + OffsetY) * DSF
      End With
      
      'if not moving the selection
      If Not MoveObj Then
        'set selection shape
        shpMove.Shape = vbShapeRectangle
        shpMove.Left = (MinX + OffsetX) * DSF
        shpMove.Top = (MinY + OffsetY) * DSF
        shpMove.Width = (MaxX - MinX) * DSF
        shpMove.Height = (MaxY - MinY) * DSF
        shpMove.Visible = True
      End If
      
      'exit so regular handles aren't drawn
      Exit Sub
      
    End Select
        
    'save bitmaps under handles
    rtn = BitBlt(picHandle.hDC, 0, 0, 8, 8, picDraw.hDC, .X1, .Y1, SRCCOPY)
    rtn = BitBlt(picHandle.hDC, 8, 0, 8, 8, picDraw.hDC, .X2, .Y1, SRCCOPY)
    rtn = BitBlt(picHandle.hDC, 16, 0, 8, 8, picDraw.hDC, .X1, .Y2, SRCCOPY)
    rtn = BitBlt(picHandle.hDC, 24, 0, 8, 8, picDraw.hDC, .X2, .Y2, SRCCOPY)
    'now draw handles
    picDraw.Line (.X1, .Y1)-Step(7, 7), vbBlack, BF
    picDraw.Line (.X2, .Y1)-Step(7, 7), vbBlack, BF
    picDraw.Line (.X1, .Y2)-Step(7, 7), vbBlack, BF
    picDraw.Line (.X2, .Y2)-Step(7, 7), vbBlack, BF
  End With
  
  'enable toolbar buttons
  With Toolbar1.Buttons
    .Item("hide").Enabled = True
    .Item("front").Enabled = True
    .Item("back").Enabled = True
  End With
Exit Sub

ErrHandler:
  Resume Next
End Sub

Private Sub SelectExit(ByRef NewSel As TSel)

  Dim rtn As Long, strID As String
  
  On Error GoTo ErrHandler
  
  picDraw.DrawWidth = 1
  
  Selection = NewSel
  
  '*'Debug.Assert Selection.Number <> 0
  
  With Selection
    Select Case .Leg
    Case llSecond
      'transfer; select second leg
      'if this is the first exit for this transfer point
      If Exits(.Number)(.ExitID).Leg = 0 Then
        'ep of transpt matches ep of exit
        .X1 = (TransPt(Exits(.Number)(.ExitID).Transfer).EP.X + OffsetX) * DSF - 4
        .Y1 = (TransPt(Exits(.Number)(.ExitID).Transfer).EP.Y + OffsetY) * DSF - 4
        
      Else
        'sp of trans pt matches ep of exit
        .X1 = (TransPt(Exits(.Number)(.ExitID).Transfer).SP.X + OffsetX) * DSF - 4
        .Y1 = (TransPt(Exits(.Number)(.ExitID).Transfer).SP.Y + OffsetY) * DSF - 4
        
      End If
      
      .X2 = (Exits(.Number)(.ExitID).EPX + OffsetX) * DSF - 4
      .Y2 = (Exits(.Number)(.ExitID).EPY + OffsetY) * DSF - 4
      
    Case llFirst
      'transfer; select first leg
      .X1 = (Exits(.Number)(.ExitID).SPX + OffsetX) * DSF - 4
      .Y1 = (Exits(.Number)(.ExitID).SPY + OffsetY) * DSF - 4
      
      'if this is first exit for this transfer point
      If Exits(.Number)(.ExitID).Leg = 0 Then
        'sp of transpt matches sp of exit
        .X2 = (TransPt(Exits(.Number)(.ExitID).Transfer).SP.X + OffsetX) * DSF - 4
        .Y2 = (TransPt(Exits(.Number)(.ExitID).Transfer).SP.Y + OffsetY) * DSF - 4
        
      Else
        'ep of transpt matches sp of exit
        .X2 = (TransPt(Exits(.Number)(.ExitID).Transfer).EP.X + OffsetX) * DSF - 4
        .Y2 = (TransPt(Exits(.Number)(.ExitID).Transfer).EP.Y + OffsetY) * DSF - 4
        
      End If
    
    Case 0
      'no transfer; select whole line
      .X1 = (Exits(.Number)(.ExitID).SPX + OffsetX) * DSF - 4
      .Y1 = (Exits(.Number)(.ExitID).SPY + OffsetY) * DSF - 4
      .X2 = (Exits(.Number)(.ExitID).EPX + OffsetX) * DSF - 4
      .Y2 = (Exits(.Number)(.ExitID).EPY + OffsetY) * DSF - 4
    End Select
    
    'if moving the exit
    If MoveExit Then
      'ensure anchor is set
      'if on first point (from room end),
      If .Point = 0 Then
        linMove.X1 = Selection.X2 + 4
        linMove.Y1 = Selection.Y2 + 4
      Else
        linMove.X1 = Selection.X1 + 4
        linMove.Y1 = Selection.Y1 + 4
      End If
    End If
    
    'save bitmaps under handles
    rtn = BitBlt(picHandle.hDC, 0, 0, 8, 8, picDraw.hDC, .X1, .Y1, SRCCOPY)
    rtn = BitBlt(picHandle.hDC, 24, 0, 8, 8, picDraw.hDC, .X2, .Y2, SRCCOPY)
    
    'if one direction, AND exit is two way
    If .TwoWay = ltwOneWay Then
      'add third handle by arrowhead
      .X3 = .X2 - (RM_SIZE / 4 * DSF) / Sqr((.X2 - .X1) ^ 2 + (.Y2 - .Y1) ^ 2) * (.X2 - .X1)
      .Y3 = .Y2 - (RM_SIZE / 4 * DSF) / Sqr((.X2 - .X1) ^ 2 + (.Y2 - .Y1) ^ 2) * (.Y2 - .Y1)
      
      rtn = BitBlt(picHandle.hDC, 8, 0, 8, 8, picDraw.hDC, .X3, .Y3, SRCCOPY)
      picDraw.Line (.X3, .Y3)-Step(7, 7), vbRed, BF
    End If
    'now draw handles
    picDraw.Line (.X1, .Y1)-Step(7, 7), vbBlack, BF
    picDraw.Line (.X2, .Y2)-Step(7, 7), vbBlack, BF
  End With
  
  'disable toolbar buttons
  With Toolbar1.Buttons
    .Item("delete").Enabled = True
    .Item("transfer").Enabled = (Selection.Leg = llNoTrans)
  End With
  
  If MainStatusBar.Tag <> CStr(rtLayout) Then
    AdjustMenus rtLayout, True, True, IsDirty
  End If
  
  'write to status bar
  With MainStatusBar.Panels
    .Item("Room1").Text = "From: " & Logics(Selection.Number).ID
    'to room may be an error
    If Exits(Selection.Number)(Selection.ExitID).Transfer < 0 Then
      .Item("Room2").Text = "To: {error}"
    Else
      On Error Resume Next
      If Exits(Selection.Number)(Selection.ExitID).Room = 0 Then
        .Item("Room2").Text = "To: {error}"
      Else
        .Item("Room2").Text = "To: " & Logics(Exits(Selection.Number)(Selection.ExitID).Room).ID
      End If
      If Err.Number = vbObjectError + 564 Then
        .Item("Room2").Text = "To: {error}"
      End If
      On Error GoTo ErrHandler
    End If
    
    Select Case Selection.TwoWay
    Case ltwBothWays
      .Item("Type").Text = "Both Ways"
      If IsTwoWay(Selection.Number, Selection.ExitID, Exits(Selection.Number)(Selection.ExitID).Room, strID, 1) Then
        'if it really is a twoway- this gets the second exit ID
        strID = "/" & strID
      End If
      'now add the first
      strID = Selection.ExitID & strID
      
      .Item("ID").Text = strID 'Selection.ExitID
    Case ltwOneWay
      .Item("Type").Text = "One Way"
      .Item("ID").Text = Selection.ExitID
    Case ltwSingle
      .Item("Type").Text = "Single"
      .Item("ID").Text = Selection.ExitID
    End Select
  End With
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub RepositionRoom(ByVal Index As Long, Optional BothDirections As Boolean = True)
  
  'this method ensures that the starting and ending
  'points of all exits from and to this room
  'are adjusted for the current room/trans pt positions
  
  'DON'T call this for an error point
  
  Dim i As Long, j As Long
  
  On Error GoTo ErrHandler
  
  'step through all exits in this room
  For i = 0 To Exits(Index).Count - 1
    Select Case Exits(Index)(i).Status
    Case esDeleted, esChanged
      'these cases are only possible while a layout is being
      'worked on; in that case, we trust that 'deleted' or
      ''changed' states are accurate, and don't do anything
      'with them
        
    Case esHidden
      'if loading a layout, then we need to verify that
      'hidden exits really are hidden, changing them to
      'OK or inserting an ErrPt if necessary
      If blnLoadingLayout Then
        'if the room is not a valid logic then we have to add an ErrPt
        If Not Logics.Exists(Exits(Index)(i).Room) Then
          InsertErrPt Index, i, Exits(Index)(i).Room
          
        'otherwise, check to see if the logic is still NOT a room
        ElseIf Logics(Exits(Index)(i).Room).IsRoom Then
          'since this IS a valid room, change exit type to normal
          Exits(Index)(i).Status = esOK
        End If
      Else
        'if not loading a layout, hidden really should mean hidden!
        If Not Logics.Exists(Exits(Index)(i).Room) Then
          '*'Debug.Assert False
        ElseIf Logics(Exits(Index)(i).Room).IsRoom Then
          ' when unhiding a room, if there are multiple identical
          ' exits to the newly unhidden room, the later ones will
          ' still show up as hidden when the first one causes
          ' reposition room; ignore it
          '*'Debug.Assert False
        End If
      End If
        
    Case esOK
      'if loading a layout, then we need to verify that OK exits
      'really are OK, hiding or adding ErrPt if necessary
      If blnLoadingLayout Then
        If Not Logics.Exists(Exits(Index)(i).Room) Then
          'if not already pointing to an error point
          If Exits(Index)(i).Transfer >= 0 Then
            InsertErrPt Index, i, Exits(Index)(i).Room
          End If
          
        'otherwise, check to see if the logic is still a room
        ElseIf Not Logics(Exits(Index)(i).Room).IsRoom And Exits(Index)(i).Room > 0 Then
          'since this IS NOT a valid room, change exit type to hidden
          Exits(Index)(i).Status = esHidden
        End If
      Else
        'if not loading a layout, OK should really mean OK!
        If Not Logics.Exists(Exits(Index)(i).Room) Then
          '*'Debug.Assert False
        ElseIf Not Logics(Exits(Index)(i).Room).IsRoom Then
          '*'Debug.Assert False
        End If
      End If
    End Select
    
    'reposition exit starting and ending points
    SetExitPos Index, i
  Next i
  
  'if checking both directions
  '(need to check other rooms that exit TO this room)
  If BothDirections Then
    'step through all other rooms
    For i = 1 To 255
      'if room is visible AND not index room
      If Room(i).Visible And i <> Index Then
        'step through all exits
        For j = 0 To Exits(i).Count - 1
          'if an exit goes to this room and it's not deleted
          If Exits(i)(j).Room = Index And Exits(i)(j).Status <> esDeleted Then
            'isn't Room(Index) always visible? otherwise this function wouldn't
            'be called
            '*'Debug.Assert Room(Index).Visible
            
            'if the exit currently points to an error BUT the target room is
            'now visible, we need to remove the error pt and point to the good room
            If Exits(i)(j).Transfer < 0 And Room(Index).Visible Then
              'error pts never have transfers so we can
              'just set the transfer value to zero and it should
              'force the SetExitPos function to correctly relocate the
              'exit (let's monitor things for a while to make sure)
              'remove the errpt
              With ErrPt(-Exits(i)(j).Transfer)
                CompactObjList .Order
                .Visible = False
                .ExitID = ""
                .Room = 0
                .FromRoom = 0
                .Order = 0
              End With
              Exits(i)(j).Transfer = 0
            End If
            
            'reposition exit starting and ending points
            SetExitPos i, j
          End If
        Next j
      End If
    Next i
  End If
  
  'if loading, don't mark as dirty
  If Not blnLoadingLayout Then
    'set dirty flag
    MarkAsDirty
  End If
Exit Sub

ErrHandler:
  Resume Next
End Sub

Private Sub SetExitPos(ByVal Index As Long, ByVal ExitID As String)
  
  'this method recalculates the starting point and ending point
  'of the exit defined by room(index) and exitID=id
  
  'if a transpt is involved, both segments will be updated
  'tp0 is the endpoint associated with the segment between from room and its transfer pt circle
  'tp1 is the endpoint associated with the segment between to room and its transfer pt circle
  
  Dim lngTP As Long
  Dim TP0 As LCoord, TP1 As LCoord
  Dim DX As Single, DY As Single, DL As Single
  
  On Error GoTo ErrHandler
  
  With Exits(Index)(ExitID)
    'transfer point Value is used alot; local copy
    lngTP = .Transfer
    
    'if this exit is part of a transfer
    If lngTP > 0 Then
      'determine which room is room1
      'If TransPt(lngTP).Room(0) = Index Then
      If .Leg = 0 Then
        'first segment is associated with from room
        'initially point to center of transpt
        TP0.X = TransPt(lngTP).Loc(0).X + RM_SIZE / 4
        TP0.Y = TransPt(lngTP).Loc(0).Y + RM_SIZE / 4
        TP1.X = TransPt(lngTP).Loc(1).X + RM_SIZE / 4
        TP1.Y = TransPt(lngTP).Loc(1).Y + RM_SIZE / 4
      Else
        'second segment is associated with from room
        'initially point to center of transpt
        TP0.X = TransPt(lngTP).Loc(1).X + RM_SIZE / 4
        TP0.Y = TransPt(lngTP).Loc(1).Y + RM_SIZE / 4
        TP1.X = TransPt(lngTP).Loc(0).X + RM_SIZE / 4
        TP1.Y = TransPt(lngTP).Loc(0).Y + RM_SIZE / 4
      End If
    End If
          
    'begin with starting point at default coordinates of from room
    .SPX = Room(Index).Loc.X
    .SPY = Room(Index).Loc.Y
    
    'if there is an error on this exits
    If lngTP < 0 Then
      'set end point to default coordinates of the err pt
      .EPX = ErrPt(-lngTP).Loc.X
      .EPY = ErrPt(-lngTP).Loc.Y
    Else
      'if hidden,
      If .Status = esHidden Then
        
      Else
        'set end point to default coordinates of to room
        .EPX = Room(.Room).Loc.X
        .EPY = Room(.Room).Loc.Y
      End If
    End If
  
    'if hidden,
    If .Status = esHidden Then
      Select Case .Reason
      Case erHorizon
        .SPX = .SPX + RM_SIZE / 2
        .EPX = .SPX + RM_SIZE / 2
        .EPY = .SPY - RM_SIZE / 2
        
      Case erRight
        .SPX = .SPX + RM_SIZE
        .SPY = .SPY + RM_SIZE / 2
        .EPX = .SPX + RM_SIZE / 2
        .EPY = .SPY + RM_SIZE / 2
        
      Case erLeft
        .SPY = .SPY + RM_SIZE / 2
        .EPX = .SPX - RM_SIZE / 2
        .EPY = .SPY - RM_SIZE / 2
        
      Case erBottom
        .SPX = .SPX + RM_SIZE / 2
        .SPY = .SPY + RM_SIZE
        .EPX = .SPX - RM_SIZE / 2
        .EPY = .SPY + RM_SIZE / 2
        
      Case erOther
        'use center as start point
        .SPX = .SPX + RM_SIZE / 2
        .SPY = .SPY + RM_SIZE / 2
        
        'choose end location like we do for transfer pts
        TP0 = GetInsertPos(.SPX + RM_SIZE / 2, .SPY + RM_SIZE / 2, 0, RM_SIZE * 2, True)
        .EPX = TP0.X
        .EPY = TP0.Y
        
        'now adjust so it only draws from edge of room instead of from center
        'calculate distances
        DX = .EPX - .SPX
        DY = .EPY - .SPY
        'if end point and start point are same (meaning room is a loop?)
        If DX = 0 And DY = 0 Then
          'adjust x values to make line draw straight across room
          .EPX = .EPX + RM_SIZE
          .SPX = .SPX - RM_SIZE
          DX = RM_SIZE / 4
        End If
        
        'adjust endpoints based on slope
        If DX = 0 Then
          'vertical line; move to right
          .SPX = .SPX + RM_SIZE / 4
          .EPX = .EPX + RM_SIZE / 4
          If DY > 0 Then
            .SPY = .SPY + RM_SIZE / 4
            .EPY = .EPY - RM_SIZE / 4
          Else
            .SPY = .SPY - RM_SIZE / 4
            .EPY = .EPY + RM_SIZE / 4
          End If
          
        ElseIf DY = 0 Then
          'horizontal line; move down
          .SPY = .SPY + RM_SIZE / 4
          .EPY = .EPY + RM_SIZE / 4
          If DX > 0 Then
            .SPX = .SPX + RM_SIZE / 4
            .EPX = .EPX - RM_SIZE / 4
          Else
            .SPX = .SPX - RM_SIZE / 4
            .EPX = .EPX + RM_SIZE / 4
          End If
        Else
          'essentially diagonal; move points
          'to within .25 units of nearest
          'horizontal and nearest vertical edge
          If DX > 0 Then
            .SPX = .SPX + RM_SIZE / 4
            .EPX = .EPX - RM_SIZE / 4
          Else
            .SPX = .SPX - RM_SIZE / 4
            .EPX = .EPX + RM_SIZE / 4
          End If
          If DY > 0 Then
            .SPY = .SPY + RM_SIZE / 4
            .EPY = .EPY - RM_SIZE / 4
          Else
            .SPY = .SPY - RM_SIZE / 4
            .EPY = .EPY + RM_SIZE / 4
          End If
        End If
        
        'if dx Value is different
        If Sgn(DX) <> Sgn(.EPX - .SPX) Then
          If DY < 0 Then
            'move point in direction of sgn x
            .EPX = .EPX + RM_SIZE / 2 * Sgn(DX)
          Else
            .SPX = .SPX - RM_SIZE / 2 * Sgn(DX)
          End If
        End If
        If Sgn(DY) <> Sgn(.EPY - .SPY) Then
          If DX < 0 Then
            .EPY = .EPY + RM_SIZE / 2 * Sgn(DY)
          Else
            .SPY = .SPY - RM_SIZE / 2 * Sgn(DY)
          End If
        End If
        
        'recalculate distances
        DX = .EPX - .SPX
        DY = .EPY - .SPY
        '*'Debug.Assert DX <> 0 Or DY <> 0
        
        'now move lines to edge
        'if line is mostly horizontal
        If Abs(DX) > Abs(DY) Then
          .SPX = .SPX + Sgn(DX) * RM_SIZE / 4
          .EPX = .EPX - Sgn(DX) * RM_SIZE / 4
          .SPY = .SPY + Sgn(DX) * RM_SIZE / 4 * DY / DX
          .EPY = .EPY - Sgn(DX) * RM_SIZE / 4 * DY / DX
        Else
          .SPY = .SPY + Sgn(DY) * RM_SIZE / 4
          .EPY = .EPY - Sgn(DY) * RM_SIZE / 4
          .SPX = .SPX + Sgn(DY) * RM_SIZE / 4 * DX / DY
          .EPX = .EPX - Sgn(DY) * RM_SIZE / 4 * DX / DY
        End If
        
      End Select
    
    Else
      'adjust FROM-point and TO-point based on exit Type
      Select Case .Reason
      Case erHorizon
        'FROM uses middle-top of room; TO uses middle-bottom
        .SPX = .SPX + RM_SIZE / 2
        .EPX = .EPX + RM_SIZE / 2
        .EPY = .EPY + RM_SIZE
        TP0.Y = TP0.Y + RM_SIZE / 4
        TP1.Y = TP1.Y - RM_SIZE / 4
      Case erRight
        'FROM room uses center-right; TO uses center-left
        .SPX = .SPX + RM_SIZE
        .SPY = .SPY + RM_SIZE / 2
        .EPY = .EPY + RM_SIZE / 2
        TP0.X = TP0.X - RM_SIZE / 4
        TP1.X = TP1.X + RM_SIZE / 4
      Case erBottom
        'FROM room uses middle-bottom; TO uses middle-top
        .SPX = .SPX + RM_SIZE / 2
        .SPY = .SPY + RM_SIZE
        .EPX = .EPX + RM_SIZE / 2
        TP0.Y = TP0.Y - RM_SIZE / 4
        TP1.Y = TP1.Y + RM_SIZE / 4
      Case erLeft
        'FROM room uses center-left; TO uses center-right
        .SPY = .SPY + RM_SIZE / 2
        .EPX = .EPX + RM_SIZE
        .EPY = .EPY + RM_SIZE / 2
        TP0.X = TP0.X + RM_SIZE / 4
        TP1.X = TP1.X - RM_SIZE / 4
        
      Case erOther
        If lngTP <= 0 Then
          'no transfer point; draw directly to/FROM rooms
          'start at center
          .EPX = .EPX + RM_SIZE / 2
          .EPY = .EPY + RM_SIZE / 2
          .SPX = .SPX + RM_SIZE / 2
          .SPY = .SPY + RM_SIZE / 2
          
          'calculate distances
          DX = .EPX - .SPX
          DY = .EPY - .SPY
          'if end point and start point are same (meaning room is a loop?)
          If DX = 0 And DY = 0 Then
            'adjust x values to make line draw straight across room
            .EPX = .EPX + RM_SIZE
            .SPX = .SPX - RM_SIZE
            DX = RM_SIZE / 4
          End If
          
          'adjust endpoints based on slope
          If DX = 0 Then
            'vertical line; move to right
            .SPX = .SPX + RM_SIZE / 4
            .EPX = .EPX + RM_SIZE / 4
            If DY > 0 Then
              .SPY = .SPY + RM_SIZE / 4
              .EPY = .EPY - RM_SIZE / 4
            Else
              .SPY = .SPY - RM_SIZE / 4
              .EPY = .EPY + RM_SIZE / 4
            End If
            
          ElseIf DY = 0 Then
            'horizontal line; move down
            .SPY = .SPY + RM_SIZE / 4
            .EPY = .EPY + RM_SIZE / 4
            If DX > 0 Then
              .SPX = .SPX + RM_SIZE / 4
              .EPX = .EPX - RM_SIZE / 4
            Else
              .SPX = .SPX - RM_SIZE / 4
              .EPX = .EPX + RM_SIZE / 4
            End If
          Else
            'essentially diagonal; move points
            'to within .25 units of nearest
            'horizontal and nearest vertical edge
            If DX > 0 Then
              .SPX = .SPX + RM_SIZE / 4
              .EPX = .EPX - RM_SIZE / 4
            Else
              .SPX = .SPX - RM_SIZE / 4
              .EPX = .EPX + RM_SIZE / 4
            End If
            If DY > 0 Then
              .SPY = .SPY + RM_SIZE / 4
              .EPY = .EPY - RM_SIZE / 4
            Else
              .SPY = .SPY - RM_SIZE / 4
              .EPY = .EPY + RM_SIZE / 4
            End If
          End If
          
          'if dx Value is different
          If Sgn(DX) <> Sgn(.EPX - .SPX) Then
            If DY < 0 Then
              'move point in direction of sgn x
              .EPX = .EPX + RM_SIZE / 2 * Sgn(DX)
            Else
              .SPX = .SPX - RM_SIZE / 2 * Sgn(DX)
            End If
          End If
          If Sgn(DY) <> Sgn(.EPY - .SPY) Then
            If DX < 0 Then
              .EPY = .EPY + RM_SIZE / 2 * Sgn(DY)
            Else
              .SPY = .SPY - RM_SIZE / 2 * Sgn(DY)
            End If
          End If
          
          'recalculate distances
          DX = .EPX - .SPX
          DY = .EPY - .SPY
          '*'Debug.Assert DX <> 0 Or DY <> 0
          
          'now move lines to edge
          'if line is mostly horizontal
          If Abs(DX) > Abs(DY) Then
            .SPX = .SPX + Sgn(DX) * RM_SIZE / 4
            .EPX = .EPX - Sgn(DX) * RM_SIZE / 4
            .SPY = .SPY + Sgn(DX) * RM_SIZE / 4 * DY / DX
            .EPY = .EPY - Sgn(DX) * RM_SIZE / 4 * DY / DX
          Else
            .SPY = .SPY + Sgn(DY) * RM_SIZE / 4
            .EPY = .EPY - Sgn(DY) * RM_SIZE / 4
            .SPX = .SPX + Sgn(DY) * RM_SIZE / 4 * DX / DY
            .EPX = .EPX - Sgn(DY) * RM_SIZE / 4 * DX / DY
          End If
        Else
          'draw from center, but line starts at object edge
          '(draw two separate lines)
          
          'first segment is from starting point to tp0:
          'use x and y distances to determine how to adjust endpoints
          'adjust by .4 to account for 1/2 of room width, since sp is currently on center of room and tp is at upper left corner
          DX = TP0.X - .SPX - RM_SIZE / 2
          DY = TP0.Y - .SPY - RM_SIZE / 2
          '*'Debug.Assert DX * DY <> 0
          
          'if transpt is to right of starting point
          If DX > 0 Then
            'move starting point to right so it is .2 away from right edge
            .SPX = .SPX + 0.6
          Else
            'move it to left so it is .2 from left edge
            .SPX = .SPX + RM_SIZE / 4
          End If
          'move point up/down to within .2 of edge in similar manner
          If DY >= 0 Then
            .SPY = .SPY + 0.6
          Else
            .SPY = .SPY + RM_SIZE / 4
          End If
          'starting point is now .2 in from nearest corner;
          'recalculate distances
          DX = TP0.X - .SPX
          DY = TP0.Y - .SPY
          DL = Sqr(DX ^ 2 + DY ^ 2)
          If DL = 0 Then
            'force apart
            TP0.X = TP0.X + 0.1
            .SPX = .SPX - 0.1
            DX = RM_SIZE / 4
            DL = RM_SIZE / 4
          End If
          
          If Abs(DX) > Abs(DY) Then
            'mostly horizontal- x distance will be edge of room
            .SPX = .SPX + Sgn(DX) * RM_SIZE / 4
            'calculate y Value that corresponds to x Value
            .SPY = .SPY + Sgn(DX) * RM_SIZE / 4 * DY / DX
          Else
            'mostly vertical- y distance will be edge of room
            .SPX = .SPX + Sgn(DY) * RM_SIZE / 4 * DX / DY
            'calculate x Value that corresponds to y Value
            .SPY = .SPY + Sgn(DY) * RM_SIZE / 4
          End If
          'move transpt end along line proportionately so it
          'is on circumference of circle
          TP0.X = TP0.X - RM_SIZE / 4 * DX / DL
          TP0.Y = TP0.Y - RM_SIZE / 4 * DY / DL
          
          'now repeat all this for the line between tp1 and end point
          DX = .EPX - TP1.X + RM_SIZE / 2
          DY = .EPY - TP1.Y + RM_SIZE / 2
          '*'Debug.Assert DX * DY <> 0
          
          If DX > 0 Then
            .EPX = .EPX + RM_SIZE / 4
          Else
            .EPX = .EPX + 0.6
          End If
          If DY >= 0 Then
            .EPY = .EPY + RM_SIZE / 4
          Else
            .EPY = .EPY + 0.6
          End If
          DX = .EPX - TP1.X
          DY = .EPY - TP1.Y
          DL = Sqr(DX ^ 2 + DY ^ 2)
          If DL = 0 Then
            'force apart
            TP1.X = TP1.X + 0.1
            .EPX = .EPX - 0.1
            DX = RM_SIZE / 4
            DL = RM_SIZE / 4
          End If
          
          '*'Debug.Assert DL <> 0
          
          If Abs(DX) > Abs(DY) Then
            .EPX = .EPX - Sgn(DX) * RM_SIZE / 4
            .EPY = .EPY - Sgn(DX) * RM_SIZE / 4 * DY / DX
          Else
            .EPX = .EPX - Sgn(DY) * RM_SIZE / 4 * DX / DY
            .EPY = .EPY - Sgn(DY) * RM_SIZE / 4
          End If
          TP1.X = TP1.X + RM_SIZE / 4 * DX / DL
          TP1.Y = TP1.Y + RM_SIZE / 4 * DY / DL
        End If
      End Select
    End If
    
    'if exit is to an errpt
    If lngTP < 0 Then
      'reset to room to center of errpt
      .EPX = ErrPt(-lngTP).Loc.X + 0.3
      .EPY = ErrPt(-lngTP).Loc.Y + RM_SIZE / 4
    End If
    
    'if there are  transfer points
    If lngTP > 0 Then
      'if first leg,
      If .Leg = 0 Then
        'copy transpt exit starting/ending point
        TransPt(lngTP).SP = TP0
        TransPt(lngTP).EP = TP1
      Else
        'starting point of line is actually associated with end point of transpt
        'copy transpt exit starting/ending point
        TransPt(lngTP).SP = TP1
        TransPt(lngTP).EP = TP0
      End If
    End If
  End With
Exit Sub

ErrHandler:
  Resume Next
End Sub
Private Sub SetScrollBars()

  'adjust scrollbar values and Max/Min
  'as needed due to scrolling or resizing events
  
  'if picDraw is wider than drawing area
  '(meaning the calculated Max Value is less than current min)
  If (MaxX + 1.3 - picDraw.Width / DSF) * 100 < HScroll1.Min Then
    'set Max and min equal
    HScroll1.Max = HScroll1.Min
  Else
    HScroll1.Max = (MaxX + 1.3 - picDraw.Width / DSF) * 100
  End If
  'confirm/set value to align with offset
  If HScroll1.Value <> -OffsetX * 100 Then
    'make sure it's between min and max
    If HScroll1.Min > -OffsetX * 100 Then
      HScroll1.Min = -OffsetX * 100
    End If
    If HScroll1.Max < -OffsetX * 100 Then
      HScroll1.Max = -OffsetX * 100
    End If
    HScroll1.Value = -OffsetX * 100
  End If
  
  'if picdraw is taller than drawing area
  If (MaxY + 1.3 - picDraw.Height / DSF) * 100 < VScroll1.Min Then
    'set Max and min equal
    VScroll1.Max = VScroll1.Min
  Else
    VScroll1.Max = (MaxY + 1.3 - picDraw.Height / DSF) * 100
  End If
  'confirm/set value to align with offset
  If VScroll1.Value <> -OffsetY * 100 Then
    'make sure it's between min and max
    If VScroll1.Min > -OffsetY * 100 Then
      VScroll1.Min = -OffsetY * 100
    End If
    If VScroll1.Max < -OffsetY * 100 Then
      VScroll1.Max = -OffsetY * 100
    End If
    VScroll1.Value = -OffsetY * 100
  End If
  
  'adjust scroll bar change values;
  'small change should be 20% of window
  'large change should be 80% of window
  
  'convert window size into real coordinates
  If CInt(20 * picDraw.Width / DSF) > 0 Then
    HScroll1.SmallChange = 20 * picDraw.Width / DSF
  Else
    HScroll1.SmallChange = 1
  End If
  
  If CInt(80 * picDraw.Width / DSF) > 0 Then
    HScroll1.LargeChange = 80 * picDraw.Width / DSF
  Else
    HScroll1.LargeChange = 1
  End If
  
  If CInt(20 * picDraw.Height / DSF) > 0 Then
    VScroll1.SmallChange = 20 * picDraw.Height / DSF
  Else
    VScroll1.SmallChange = 1
  End If
  If CInt(80 * picDraw.Height / DSF) > 0 Then
    VScroll1.LargeChange = 80 * picDraw.Height / DSF
  Else
    VScroll1.LargeChange = 1
  End If
End Sub

Public Sub DisplayRoom(ByVal NewRoom As Byte, ByVal NeedPos As Boolean, Optional ByVal NoExits As Boolean = False)

  'should only be called for a room that is not currently visible;
  'if position is already known, such as when dropping a new room
  'on drawing surface, don't need to get a default position
  
  'mark as visible,
  'step through all exits in game, and if error exits point to this room
  'replace the errmarker with the room
  '(don't need to check for transfers because any exits to this
  'room that currently exist would be marked as errpts)
  
  Dim blnFound As Boolean
  Dim i As Long, j As Long
  
  On Error GoTo ErrHandler
  '*'Debug.Assert Not Room(NewRoom).Visible
  
  'add the new room to layout
  Room(NewRoom).Visible = True
  Room(NewRoom).Order = ObjCount
  Room(NewRoom).ShowPic = Settings.LEShowPics
  ObjOrder(ObjCount).Type = lsRoom
  ObjOrder(ObjCount).Number = NewRoom
  ObjCount = ObjCount + 1
  
  'if position needed, and no previous reference found,
  If NeedPos Then
    'add in middle of display
    Room(NewRoom).Loc = GetInsertPos((CalcWidth - Toolbar1.Width) / 2 / DSF - OffsetX, CalcHeight / 2 / DSF - OffsetY)
  End If
  
  'look for exits pointing to error points which match this
  'look for hidden exits pointing to this
  For i = 1 To 255
    'if room is visible (but not room being added)
    If Room(i).Visible And i <> NewRoom Then
      If Exits(i).Count > 0 Then
        For j = 0 To Exits(i).Count - 1
          If Exits(i)(j).Room = NewRoom And Exits(i)(j).Status <> esDeleted Then
            'change status back to normal
            Exits(i)(j).Status = esOK
          
            'in case an err pt is found, deal with it
            If Exits(i)(j).Transfer < 0 Then
              'if location not known, and not found yet
              If Not blnFound And NeedPos Then
                blnFound = True
                'move it here
                Room(NewRoom).Loc.X = GridPos(ErrPt(-Exits(i)(j).Transfer).Loc.X - 0.1)
                Room(NewRoom).Loc.Y = GridPos(ErrPt(-Exits(i)(j).Transfer).Loc.Y - RM_SIZE / 4)
              End If
              
              'hide errpt
              With ErrPt(-Exits(i)(j).Transfer)
                CompactObjList .Order
                .Visible = False
                .ExitID = ""
                .Room = 0
                .FromRoom = 0
                .Order = 0
              End With
              'clear transfer marker
              Exits(i)(j).Transfer = 0
            End If
            
            'recalculate exits for this room (by calling reposition)
            RepositionRoom i
          End If
        Next j
      End If
    End If
  Next i
  '*'Debug.Assert Room(NewRoom).Visible
  
  ' only build exits if not skipping them
  If Not NoExits Then
    'build exits for room being added
    Set Exits(NewRoom) = ExtractExits(Logics(NewRoom))
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Function AskClose() As Boolean

  Dim rtn As VbMsgBoxResult

  On Error GoTo ErrHandler
  'assume ok to close
  AskClose = True

  'if layout has been modified since last save,
  If IsDirty Then
    'get user input
    rtn = MsgBox("Do you want to save changes to the layout and update logics before closing?", vbYesNoCancel, "Layout Editor")

    Select Case rtn
    Case vbYes
      SaveLayout

    Case vbCancel
      AskClose = False
    End Select
  End If
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function


Private Function GetZoomCenter() As POINTAPI

  'returns the cursor position in picDraw coordinates
  'if the cursor is not over the picDraw surface
  'it returns the center of the picDraw surface
  
  Dim rtn As Long
  Dim mPos As POINTAPI
  
  'get the cursor position and figure out if it's over picVisual or picPriority
  rtn = GetCursorPos(mPos)
  rtn = WindowFromPoint(mPos.X, mPos.Y)
  
  'if cursor is over the drawing area, then set fraction values to 1/2
  If rtn = picDraw.hWnd Then
    'convert to client coordinates
    rtn = ScreenToClient(rtn, mPos)
  Else
    'use the center of picDraw
    mPos.X = picDraw.Width / 2
    mPos.Y = picDraw.Height / 2
  End If
  
  'return the calculated position
  GetZoomCenter = mPos
End Function

Private Sub DrawRoomPic(ByVal RoomNum As Byte, ByVal xPos As Single, ByVal yPos As Single)


  On Error GoTo ErrHandler

  Dim blnUnloaded As Boolean
  Dim rtn As Long
  
  'make sure picture exists
  If Not Pictures.Exists(RoomNum) Then
    Exit Sub
  End If
  
  'save load state for later, load if necessary
  blnUnloaded = Not Pictures(RoomNum).Loaded
  If blnUnloaded Then
    Pictures(RoomNum).Load
  End If

  rtn = StretchBlt(picDraw.hDC, CLng((Room(RoomNum).Loc.X + OffsetX) * DSF) + 1, CLng((Room(RoomNum).Loc.Y + OffsetY) * DSF) + 1, CLng(RM_SIZE * DSF) - 2, CLng(RM_SIZE * 0.525 * DSF), Pictures(RoomNum).VisualBMP, 0, 0, 160, 168, SRCCOPY)
  
  'close pic if it was before
  If blnUnloaded Then
    'unload the pic
    Pictures(RoomNum).Unload
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  'if error, just ignore and don't draw anything
  Err.Clear
  
  'make sure pic is closed if it originally was
  If blnUnloaded Then
    'unload the pic
    Pictures(RoomNum).Unload
  End If
End Sub

Private Function DeltaX(ByVal Dir As Long) As Single
  
  'only uses bits 1 and 2; rest of number is ignored
  DeltaX = 2 * ((1 - (Dir And 2)) * (Dir And 1))
End Function

Private Function DeltaY(ByVal Dir As Long) As Single

  'only uses bits 1 and 2; rest of number is ignored
  DeltaY = 2 * ((1 - (Dir And 2)) * ((Dir And 1) - 1))
End Function


Private Function ExtractLayout(Optional ByVal SavePos As Boolean = False) As Boolean

  'if SavePos is True, then we keep the position information for
  'objects already on screen, and only update IsRoom status and
  'exits
  
  Dim aglAdd As AGILogic
  Dim i As Long, j As Long, k As Long
  Dim bytGrp As Byte, bytRoom As Byte
  Dim StackCount As Byte, Stack(255) As Byte
  Dim Queue(255) As Byte, QCur As Byte, QEnd As Byte
  Dim TPCount As Long, EPCount As Long
  Dim tmpCoord As LCoord
  Dim b(15) As Byte, xPos As Byte, yPos As Byte
  Dim GroupMax(255) As LCoord
  Dim GroupCount(255) As Byte
  Dim InGame(255) As Boolean
  Dim lngMult As Long
  
  On Error GoTo ErrHandler
  
  'show progress form to provide feedback to user
  Load frmProgress
  With frmProgress
    If SavePos Then
      .Caption = "Repairing Layout"
    Else
      .Caption = "Extract Layout"
    End If
    .lblProgress.Caption = "Analyzing ..."
    .pgbStatus.Max = Logics.Count + 12
    .pgbStatus.Value = 0
  End With
  frmProgress.Show vbModeless, frmMDIMain
  frmProgress.Refresh
  
  'clear variables
  For i = 1 To 255
    With ELRoom(i)
      .Analyzed = False
      .Placed = False
      .Group = 0
      For j = 0 To 3
        .Enter(j) = 0
        .Exits(j) = 0
      Next j
    End With
    With Room(i)
      .Visible = False
      .Order = 0
      .ShowPic = Settings.LEShowPics
      If Not SavePos Then
        .Loc.X = 0
        .Loc.Y = 0
      End If
    End With
    
    'comments would only exist if rebuilding, not during
    'initial extraction, but clear them anyway
    Comment(i).Order = 0
    
    With TransPt(i)
      .Count = 0
      .Loc(0).X = 0
      .Loc(0).Y = 0
      .Loc(1).X = 0
      .Loc(1).Y = 0
      .Room(0) = 0
      .Room(1) = 0
      .ExitID(0) = vbNullString
      .ExitID(1) = vbNullString
      .Order = 0
    End With
    With ErrPt(i)
      .Visible = False
      .FromRoom = 0
      .Room = 0
      .Order = 0
      .Loc.X = 0
      .Loc.Y = 0
      .ExitID = ""
    End With
    InGame(i) = False
  Next i
  
  'clear the object placement order list
  For i = 0 To 1023
    ObjOrder(i).Number = 0
  Next i
  ObjCount = 0
  
  'disable drawing
  picDraw.Cls
  blnDontDraw = True
  
  'run through all game logics and
  'identify edgecode exits and entrances
  For Each aglAdd In Logics
    'increment progress bar
    frmProgress.pgbStatus.Value = frmProgress.pgbStatus.Value + 1
    
    'update display
    frmProgress.lblProgress.Caption = "Analyzing " & aglAdd.ID & "..."
    frmProgress.Refresh
    
    'skip logic 0
    If aglAdd.Number <> 0 Then
      'mark as in game
      InGame(aglAdd.Number) = True
      
      'if repairing, don't mess with IsRoom status
      If Not SavePos Then
        'assume not in game, until we know otherwise
        aglAdd.IsRoom = False
      End If
      
      'ensure exits are cleared
      Exits(aglAdd.Number).Clear
      
      'get exits (and saves logic if exits were found)
      Set Exits(aglAdd.Number) = ExtractExits(aglAdd)
      
      j = Exits(aglAdd.Number).Count - 1
      'if no exits
      If j < 0 Then
        'was it already added (a target of another room)?
        If Room(aglAdd.Number).Visible Then
          'it IS a room
          aglAdd.IsRoom = True
        End If
      Else
        'if there are exits, assume it's a room
        aglAdd.IsRoom = True
      End If
      
      'if a room but not yet added, show it
      If aglAdd.IsRoom And Not Room(aglAdd.Number).Visible Then
        'ensure visible
        Room(aglAdd.Number).Visible = True
        'add it to draw order stack
        Room(aglAdd.Number).Order = ObjCount
        ObjOrder(ObjCount).Type = lsRoom
        ObjOrder(ObjCount).Number = aglAdd.Number
        ObjCount = ObjCount + 1
      End If
      
      'step through all exits
      For i = 0 To j
      '*'Debug.Assert Exits(aglAdd.Number)(i).Reason > 0
        k = Exits(aglAdd.Number)(i).Reason - 1
        'for all 'edge' exits, save the room info so layout
        'can build groups later
        Select Case k
        Case 0 To 3
          'copy exit for this room to array used to build layout
          ELRoom(aglAdd.Number).Exits(k) = Exits(aglAdd.Number)(i).Room
          'set entrance for target room (opposite direction)
          ELRoom(Exits(aglAdd.Number)(i).Room).Enter((k + 2) And 3) = aglAdd.Number
        Case Else
          'other' exits don't need to be tracked for group positioning
        End Select
        'if target is not yet added, add it now
        If Not Room(Exits(aglAdd.Number)(i).Room).Visible Then
          'but make sure it's a real room first
          '(if it's not, an error point will be added later?)
          If Logics.Exists(Exits(aglAdd.Number)(i).Room) Then
            Room(Exits(aglAdd.Number)(i).Room).Visible = True
            'add it to order
            Room(Exits(aglAdd.Number)(i).Room).Order = ObjCount
            ObjOrder(ObjCount).Type = lsRoom
            ObjOrder(ObjCount).Number = Exits(aglAdd.Number)(i).Room
            ObjCount = ObjCount + 1
          End If
        End If
      Next i
    End If
  Next
    
  'update progress
  With frmProgress
    .pgbStatus.Value = .pgbStatus.Value + 2
    .lblProgress.Caption = "Creating room groups..."
  End With

  'step through all identified rooms
  'to place each room in its group based
  'on being along a common path
  For i = 1 To 255
    Do
      'if room is not visible (meaning it has no exits into or out of it)
      If Not Room(i).Visible Then
        Exit Do
      End If
      
      'if already in a group
      If ELRoom(i).Group <> 0 Then
        Exit Do
      End If
      
      'increment group
      bytGrp = bytGrp + 1
      
      'add to stack
      StackCount = StackCount + 1
      Stack(StackCount) = i

      'set analyzed flag to mark this as starter for the group
      ELRoom(Stack(StackCount)).Analyzed = True
      ELRoom(Stack(StackCount)).Group = bytGrp
      GroupCount(bytGrp) = 1
      
      'go through stack until done
      Do Until StackCount = 0
        bytRoom = Stack(StackCount)
        'decrement stack Count
        StackCount = StackCount - 1
          
        'valid if in use
        If Room(bytRoom).Visible Then
          'step through exits
          For j = 0 To 3
            'if target room is valid and target does not yet have a group
            If ELRoom(bytRoom).Exits(j) > 0 And ELRoom(ELRoom(bytRoom).Exits(j)).Group = 0 Then
              'add to group
              ELRoom(ELRoom(bytRoom).Exits(j)).Group = bytGrp
              GroupCount(bytGrp) = GroupCount(bytGrp) + 1
            
              'add to stack
              StackCount = StackCount + 1
              Stack(StackCount) = ELRoom(bytRoom).Exits(j)
            End If
          Next j
        
          'step through entrances
          For j = 0 To 3
            'if from room is valid and from room does not yet have a group
            If ELRoom(bytRoom).Enter(j) > 0 And ELRoom(ELRoom(bytRoom).Enter(j)).Group = 0 Then
              'add to group
              ELRoom(ELRoom(bytRoom).Enter(j)).Group = bytGrp
              GroupCount(bytGrp) = GroupCount(bytGrp) + 1
              
              'add to stack
              StackCount = StackCount + 1
              Stack(StackCount) = ELRoom(bytRoom).Enter(j)
            End If
          Next j
        End If
      Loop
    Loop Until True
  Next i
  
  'if extracting, we need to also set groups and determine positioning
  If Not SavePos Then
    'update progress
    With frmProgress
      .pgbStatus.Value = .pgbStatus.Value + 2
      .lblProgress.Caption = "Arranging rooms in groups..."
    End With
                
    'step through groups to assign physical positions
    For i = 1 To bytGrp
      'if group has at least two ELRooms,
      If GroupCount(i) > 1 Then
        'find first ELRoom in this group
        For j = 1 To 255
          If (ELRoom(j).Group = i) And ELRoom(j).Analyzed Then
            'unmark as analyzed so it gets added
            ELRoom(j).Analyzed = False
            Exit For
          End If
        Next j
        '*'Debug.Assert j <> 256
        'reset queue
        QEnd = 0
        QCur = 0
        Queue(QEnd) = j
        QEnd = QEnd + 1
        
        'reset Count, min and Max
        MinX = 0
        MinY = 0
        MaxX = 0
        MaxY = 0
        
        'mark first room as placed
        ELRoom(j).Placed = True
        
        'use queue to place all rooms in this group
        Do Until QEnd = QCur
          'if NOT already in use
          If Not ELRoom(Queue(QCur)).Analyzed Then
            'mark it as analyzed and as placed
            ELRoom(Queue(QCur)).Analyzed = True
            
            'crawl in all four directions
            For j = 0 To 3
              'reset stack
              StackCount = 1
              Stack(1) = Queue(QCur)
              
              Do Until StackCount = 0
                'get room off stack
                bytRoom = Stack(StackCount)
                StackCount = StackCount - 1
                
                'check exit and entrance in direction j
                'if target room is valid, and not yet placed
                If ELRoom(bytRoom).Exits(j) <> 0 And Not ELRoom(ELRoom(bytRoom).Exits(j)).Placed Then
                  'if space already occupied
                  If ItemAtPoint(Room(bytRoom).Loc.X + DeltaX(j), Room(bytRoom).Loc.Y + DeltaY(j), i) Then
                    'try next space clockwise
                    If Not ItemAtPoint(Room(bytRoom).Loc.X + DeltaX(j) + DeltaX(j + 1), Room(bytRoom).Loc.Y + DeltaY(j) + DeltaY(j + 1), i) Then
                      Room(ELRoom(bytRoom).Exits(j)).Loc.X = Room(bytRoom).Loc.X + DeltaX(j) + DeltaX(j + 1)
                      Room(ELRoom(bytRoom).Exits(j)).Loc.Y = Room(bytRoom).Loc.Y + DeltaY(j) + DeltaY(j + 1)
                    'try next space counterclockwise
                    ElseIf Not ItemAtPoint(Room(bytRoom).Loc.X + DeltaX(j) + DeltaX(j + 3), Room(bytRoom).Loc.Y + DeltaY(j) + DeltaY(j + 3), i) Then
                      Room(ELRoom(bytRoom).Exits(j)).Loc.X = Room(bytRoom).Loc.X + DeltaX(j) + DeltaX(j + 3)
                      Room(ELRoom(bytRoom).Exits(j)).Loc.Y = Room(bytRoom).Loc.Y + DeltaY(j) + DeltaY(j + 3)
                    Else
                      'locate free point around target
                      tmpCoord = GetInsertPos(Room(bytRoom).Loc.X + DeltaX(j), Room(bytRoom).Loc.Y + DeltaY(j), i)
                      Room(ELRoom(bytRoom).Exits(j)).Loc.X = tmpCoord.X
                      Room(ELRoom(bytRoom).Exits(j)).Loc.Y = tmpCoord.Y
                    End If
                  Else
                    Room(ELRoom(bytRoom).Exits(j)).Loc.X = Room(bytRoom).Loc.X + DeltaX(j)
                    Room(ELRoom(bytRoom).Exits(j)).Loc.Y = Room(bytRoom).Loc.Y + DeltaY(j)
                  End If
                    
                  'set the group, mark it and add to stack
                  ELRoom(ELRoom(bytRoom).Exits(j)).Group = i
                  ELRoom(ELRoom(bytRoom).Exits(j)).Placed = True
                  StackCount = StackCount + 1
                  Stack(StackCount) = ELRoom(bytRoom).Exits(j)
                  
                  'if not already analyzed
                  If Not ELRoom(ELRoom(bytRoom).Exits(j)).Analyzed Then
                    'add to q as well
                    Queue(QEnd) = ELRoom(bytRoom).Exits(j)
                    QEnd = QEnd + 1
                  End If
                  
                  'set min and Max
                  If Room(ELRoom(bytRoom).Exits(j)).Loc.X < MinX Then MinX = Room(ELRoom(bytRoom).Exits(j)).Loc.X
                  If Room(ELRoom(bytRoom).Exits(j)).Loc.Y < MinY Then MinY = Room(ELRoom(bytRoom).Exits(j)).Loc.Y
                  If Room(ELRoom(bytRoom).Exits(j)).Loc.X > MaxX Then MaxX = Room(ELRoom(bytRoom).Exits(j)).Loc.X
                  If Room(ELRoom(bytRoom).Exits(j)).Loc.Y > MaxY Then MaxY = Room(ELRoom(bytRoom).Exits(j)).Loc.Y
                End If
                
                'if from room is valid, and not yet placed
                If ELRoom(bytRoom).Enter(j) <> 0 And Not ELRoom(ELRoom(bytRoom).Enter(j)).Placed Then
                  'if space already occupied
                  If ItemAtPoint(Room(bytRoom).Loc.X + DeltaX(j), Room(bytRoom).Loc.Y + DeltaY(j), i) Then
                    'try next space clockwise
                    If Not ItemAtPoint(Room(bytRoom).Loc.X + DeltaX(j) + DeltaX(j + 1), Room(bytRoom).Loc.Y + DeltaY(j) + DeltaY(j + 1), i) Then
                      Room(ELRoom(bytRoom).Enter(j)).Loc.X = Room(bytRoom).Loc.X + DeltaX(j) + DeltaX(j + 1)
                      Room(ELRoom(bytRoom).Enter(j)).Loc.Y = Room(bytRoom).Loc.Y + DeltaY(j) + DeltaY(j + 1)
                    'try next space counterclockwise
                    ElseIf Not ItemAtPoint(Room(bytRoom).Loc.X + DeltaX(j) + DeltaX(j + 3), Room(bytRoom).Loc.Y + DeltaY(j) + DeltaY(j + 3), i) Then
                      Room(ELRoom(bytRoom).Enter(j)).Loc.X = Room(bytRoom).Loc.X + DeltaX(j) + DeltaX(j + 3)
                      Room(ELRoom(bytRoom).Enter(j)).Loc.Y = Room(bytRoom).Loc.Y + DeltaY(j) + DeltaY(j + 3)
                    Else
                      'locate free point around target
                      tmpCoord = GetInsertPos(Room(bytRoom).Loc.X + DeltaX(j), Room(bytRoom).Loc.Y + DeltaY(j), i)
                      Room(ELRoom(bytRoom).Enter(j)).Loc.X = tmpCoord.X
                      Room(ELRoom(bytRoom).Enter(j)).Loc.Y = tmpCoord.Y
                    End If
                  Else
                    Room(ELRoom(bytRoom).Enter(j)).Loc.X = Room(bytRoom).Loc.X + DeltaX(j)
                    Room(ELRoom(bytRoom).Enter(j)).Loc.Y = Room(bytRoom).Loc.Y + DeltaY(j)
                  End If
                    
                  'set the group, mark it and add to stack
                  ELRoom(ELRoom(bytRoom).Enter(j)).Group = i
                  ELRoom(ELRoom(bytRoom).Enter(j)).Placed = True
                  StackCount = StackCount + 1
                  Stack(StackCount) = ELRoom(bytRoom).Enter(j)
                  
                  'if not already analyzed
                  If Not ELRoom(ELRoom(bytRoom).Enter(j)).Analyzed Then
                    'add to q as well
                    Queue(QEnd) = ELRoom(bytRoom).Enter(j)
                    QEnd = QEnd + 1
                  End If
                  
                  'set min and Max
                  If Room(ELRoom(bytRoom).Enter(j)).Loc.X < MinX Then MinX = Room(ELRoom(bytRoom).Enter(j)).Loc.X
                  If Room(ELRoom(bytRoom).Enter(j)).Loc.Y < MinY Then MinY = Room(ELRoom(bytRoom).Enter(j)).Loc.Y
                  If Room(ELRoom(bytRoom).Enter(j)).Loc.X > MaxX Then MaxX = Room(ELRoom(bytRoom).Enter(j)).Loc.X
                  If Room(ELRoom(bytRoom).Enter(j)).Loc.Y > MaxY Then MaxY = Room(ELRoom(bytRoom).Enter(j)).Loc.Y
                End If
                
                'now step through other exits;
                For k = 0 To Exits(bytRoom).Count - 1
                  Select Case Exits(bytRoom)(k).Reason
                  Case 5  'erOther
                    'if other exit is a single room and room is not zero
                    If (GroupCount(ELRoom(Exits(bytRoom)(k).Room).Group) = 1) And (Exits(bytRoom)(k).Room <> 0) Then
                      'insert the room
                      tmpCoord = GetInsertPos(Room(Logics(bytRoom).Number).Loc.X, Room(Logics(bytRoom).Number).Loc.Y, i)
                      Room(Exits(bytRoom)(k).Room).Loc.X = tmpCoord.X
                      Room(Exits(bytRoom)(k).Room).Loc.Y = tmpCoord.Y
                      ELRoom(Exits(bytRoom)(k).Room).Placed = True
                      'move to this group
                      GroupCount(ELRoom(Exits(bytRoom)(k).Room).Group) = 0
                      ELRoom(Exits(bytRoom)(k).Room).Group = i
                      GroupCount(i) = GroupCount(i) + 1
                    End If
                  End Select
                Next k
              Loop
            Next j
          End If
          'move up in queue
          QCur = QCur + 1
        Loop
        
        'reset positions based on min x and min Y
        For j = 1 To 255
          If ELRoom(j).Group = i Then
            Room(j).Loc.X = Room(j).Loc.X - MinX
            Room(j).Loc.Y = Room(j).Loc.Y - MinY
            'clear placed flag
            ELRoom(j).Placed = False
          End If
        Next j
        'adjust Max
        GroupMax(i).X = MaxX - MinX
        GroupMax(i).Y = MaxY - MinY
      End If
    Next i
    
    'update progress
    With frmProgress
      .pgbStatus.Value = .pgbStatus.Value + 2
      .lblProgress.Caption = "Positioning room groups on layout..."
    End With
  
    'now position groups on surface to maximize use of space
    'put group one in default position (upper left)
    For i = 0 To GroupMax(1).X + 1
      'only worry about first 16 columns
      If i > 15 Then
        Exit For
      End If
      'set bottom Value
      b(i) = GroupMax(1).Y + 4
    Next i
    
    'step through rest of groups
    For i = 2 To bytGrp
      'if at least one ELRoom in group
      If GroupCount(i) > 0 Then
        'reset starting pos to bottom left
        xPos = 0
        yPos = 255
        'reset counter
        j = 0
        
        'determine where to put this group by 'sliding' it
        'along the bottom of currently placed groups (marked by the array B())
        'this is basically like Tetris, trying to find where the current
        'group can fit with the lowest Y Value, taking into account its width
        Do
          'reset maxy to top
          MaxY = 0
          'get Max Y for width of this group at position j
          For k = 0 To GroupMax(i).X + 2
            'ignore columns after 16
            If j + k > 15 Then
              Exit For
            End If
            'if this column's Max is >current Max,
            If b(j + k) > MaxY Then
              MaxY = b(j + k)
            End If
          Next k
          'compare this maxy to minimum
          If MaxY < yPos Then
            yPos = MaxY
            xPos = j
          End If
          'increment starting column, and get next minimum
          j = j + 1
        Loop Until j > 16 - (GroupMax(i).X + 2)
        
        'adjust bottom for newly added group
        For k = 0 To GroupMax(i).X + 2
          If xPos + k > 15 Then
            Exit For
          End If
          b(xPos + k) = MaxY + GroupMax(i).Y + 4
        Next k
        'adjust group offset
        For j = 1 To 255
          If ELRoom(j).Group = i Then
            Room(j).Loc.X = Room(j).Loc.X + xPos
            Room(j).Loc.Y = Room(j).Loc.Y + yPos
          End If
        Next j
      End If
    Next i
  End If
  
  'update progress
  With frmProgress
    .pgbStatus.Value = .pgbStatus.Value + 2
    .lblProgress.Caption = "Checking circular references..."
  End With

  'now add transfer points and errpoints as appropriate
  TPCount = 1
  EPCount = 1
  For i = 1 To 255
    'if room is visible
    If Room(i).Visible Then
      'step through all exits in this room
      For j = 0 To Exits(i).Count - 1
        bytRoom = Exits(i)(j).Room
        'if exit is to an undefined room
        If bytRoom = 0 Or Not InGame(Exits(i)(j).Room) Then
          'insert errpt
          With ErrPt(EPCount)
            .Visible = True
            .FromRoom = i
            .Room = bytRoom
            .ExitID = Exits(i)(j).ID
            'increment object counter
            .Order = ObjCount
          End With
          ObjOrder(ObjCount).Type = lsErrPt
          ObjOrder(ObjCount).Number = EPCount
          ObjCount = ObjCount + 1
          
          'mark exit
          Exits(i)(j).Transfer = -EPCount
          Select Case Exits(i)(j).Reason
          Case erNone, erOther
            'position first around from room
            tmpCoord = GetInsertPos(Room(i).Loc.X, Room(i).Loc.Y, 0, 1)
            'if valid spot not found (coords didnt change)
            If tmpCoord.X = Room(i).Loc.X Then
              'move it directly above
              tmpCoord.Y = Room(i).Loc.Y - 1.5
            End If
            
          Case erHorizon
            'position around point above
            tmpCoord = GetInsertPos(Room(i).Loc.X, Room(i).Loc.Y - 1.5)
          Case erBottom
            'position around point below
            tmpCoord = GetInsertPos(Room(i).Loc.X, Room(i).Loc.Y + 1.5)
          Case erLeft
            'position around point to left
            tmpCoord = GetInsertPos(Room(i).Loc.X - 1.5, Room(i).Loc.Y)
          Case erRight
            'position around point to right
            tmpCoord = GetInsertPos(Room(i).Loc.X + 1.5, Room(i).Loc.Y)
          End Select
          
          'adjust to account for size/shape of the errpt
          tmpCoord.X = tmpCoord.X + 0.1
          tmpCoord.Y = tmpCoord.Y + RM_SIZE / 4
          ErrPt(EPCount).Loc = tmpCoord
          
          'increment errpt counter
          EPCount = EPCount + 1
        Else
          'if target room is visible
          If Room(Exits(i)(j).Room).Visible Then
            Select Case Exits(i)(j).Reason
            Case erHorizon
              'if target room is below this room, or on same level
              '(NOTE: this will also catch rooms that loop on themselves)
              If Room(i).Loc.Y <= Room(bytRoom).Loc.Y Then
                'check for an existing set of transfer points between these two rooms
                k = GetTransfer(i, bytRoom, erBottom)
                If k > 0 Then
                  'use this transfer
                  Exits(i)(j).Transfer = k
                  Exits(i)(j).Leg = 1
                  'mark as containing two segments
                  TransPt(k).Count = 2
                  TransPt(k).ExitID(1) = Exits(i)(j).ID
                Else
                  'create new transfer point
                  With TransPt(TPCount)
                    .Count = 1
                    .Room(0) = i
                    .Room(1) = bytRoom
                    .ExitID(0) = Exits(i)(j).ID
                    Exits(i)(j).Transfer = TPCount
                    Exits(i)(j).Leg = 0
                    'add it to order
                    TransPt(TPCount).Order = ObjCount
                    ObjOrder(ObjCount).Type = lsTransPt
                    ObjOrder(ObjCount).Number = TPCount
                    ObjCount = ObjCount + 1
                    'adjust to account for size/shape of the transpt
                    .Loc(0) = GetInsertPos(Room(i).Loc.X, Room(i).Loc.Y - 1.5, 0, 0.5)
                    .Loc(1) = GetInsertPos(Room(bytRoom).Loc.X, Room(bytRoom).Loc.Y + 1.5, 0, 0.5)
                  End With
                  'increment transfer counter
                  TPCount = TPCount + 1
                End If
              End If
  
            Case erRight
              'if target room is to left of this room, or on same level
              '(NOTE: this will also catch rooms that loop on themselves)
              If Room(i).Loc.X >= Room(bytRoom).Loc.X Then
                'check for an existing set of transfer points between these two rooms
                k = GetTransfer(i, bytRoom, erLeft)
                If k > 0 Then
                  'use this transfer
                  Exits(i)(j).Transfer = k
                  Exits(i)(j).Leg = 1
                  'mark as having two segments
                  TransPt(k).Count = 2
                  TransPt(k).ExitID(1) = Exits(i)(j).ID
                Else
                  'create new transfer point
                  With TransPt(TPCount)
                    .Count = 1
                    .Room(0) = i
                    .Room(1) = bytRoom
                    .ExitID(0) = Exits(i)(j).ID
                    Exits(i)(j).Transfer = TPCount
                    Exits(i)(j).Leg = 0
                    'add it to order
                    .Order = ObjCount
                    ObjOrder(ObjCount).Type = lsTransPt
                    ObjOrder(ObjCount).Number = TPCount
                    ObjCount = ObjCount + 1
                    .Loc(0) = GetInsertPos(Room(i).Loc.X + 1.5, Room(i).Loc.Y, 0, 0.5)
                    .Loc(1) = GetInsertPos(Room(bytRoom).Loc.X - 1.5, Room(bytRoom).Loc.Y, 0, 0.5)
                  End With
                  'increment transfer counter
                  TPCount = TPCount + 1
                End If
              End If
  
            Case erBottom
              'if target room is above this room, or on same level
              '(NOTE: this will also catch rooms that loop on themselves)
              If Room(i).Loc.Y >= Room(bytRoom).Loc.Y Then
                'check for an existing set of transfer points between these two rooms
                k = GetTransfer(i, bytRoom, erHorizon)
                If k > 0 Then
                  'use this transfer
                  Exits(i)(j).Transfer = k
                  Exits(i)(j).Leg = 1
                  'mark as having two segments
                  TransPt(k).Count = 2
                  TransPt(k).ExitID(1) = Exits(i)(j).ID
                Else
                  'create new transfer point
                  With TransPt(TPCount)
                    .Count = 1
                    .Room(0) = i
                    .Room(1) = bytRoom
                    .ExitID(0) = Exits(i)(j).ID
                    Exits(i)(j).Transfer = TPCount
                    Exits(i)(j).Leg = 0
                    'add it to order
                    .Order = ObjCount
                    ObjOrder(ObjCount).Type = lsTransPt
                    ObjOrder(ObjCount).Number = TPCount
                    ObjCount = ObjCount + 1
                    .Loc(0) = GetInsertPos(Room(i).Loc.X, Room(i).Loc.Y + 1.5, 0, 0.5)
                    .Loc(1) = GetInsertPos(Room(bytRoom).Loc.X, Room(bytRoom).Loc.Y - 1.5, 0, 0.5)
                  End With
                  'increment transfer counter
                  TPCount = TPCount + 1
                End If
              End If
  
            Case erLeft
              'if target room is to right of this room, or on same level
              '(NOTE: this will also catch rooms that loop on themselves)
              If Room(i).Loc.X <= Room(bytRoom).Loc.X Then
                'check for an existing set of transfer points between these two rooms
                k = GetTransfer(i, bytRoom, erRight)
                If k > 0 Then
                  'use this transfer
                  Exits(i)(j).Transfer = k
                  Exits(i)(j).Leg = 1
                  'mark has having two segments
                  TransPt(k).Count = 2
                  TransPt(k).ExitID(1) = Exits(i)(j).ID
                Else
                  'create new transfer point
                  With TransPt(TPCount)
                    .Count = 1
                    .Room(0) = i
                    .Room(1) = bytRoom
                    .ExitID(0) = Exits(i)(j).ID
                    Exits(i)(j).Transfer = TPCount
                    Exits(i)(j).Leg = 0
                    'add it to order
                    .Order = ObjCount
                    ObjOrder(ObjCount).Type = lsTransPt
                    ObjOrder(ObjCount).Number = TPCount
                    ObjCount = ObjCount + 1
                    .Loc(0) = GetInsertPos(Room(i).Loc.X - 1.5, Room(i).Loc.Y, 0, 0.5)
                    .Loc(1) = GetInsertPos(Room(bytRoom).Loc.X + 1.5, Room(bytRoom).Loc.Y, 0, 0.5)
                  End With
                  'increment transfer counter
                  TPCount = TPCount + 1
                End If
              End If
  
            Case erOther
              'if more than 4 blocks away, AND if in another group OR if exit loops back to this room
              If (Abs(Room(i).Loc.X - Room(bytRoom).Loc.X) + Abs(Room(i).Loc.Y - Room(bytRoom).Loc.Y) > 6 _
                 And ELRoom(i).Group <> ELRoom(bytRoom).Group) Or Exits(i)(j).Room = i Then
                'check for an existing set of transfer points between these two rooms
                k = GetTransfer(i, bytRoom, erOther)
                If k > 0 Then
                  'use this transfer
                  Exits(i)(j).Transfer = k
                  Exits(i)(j).Leg = 1
                  'mark has having two segments
                  '*'Debug.Assert TransPt(k).Count = 1
                  TransPt(k).Count = 2
                  TransPt(k).ExitID(1) = Exits(i)(j).ID
                Else
                  'create new transfer point
                  With TransPt(TPCount)
                    .Count = 1
                    .Room(0) = i
                    .Room(1) = bytRoom
                    .ExitID(0) = Exits(i)(j).ID
                    Exits(i)(j).Transfer = TPCount
                    Exits(i)(j).Leg = 0
                    'add it to order
                    .Order = ObjCount
                    ObjOrder(ObjCount).Type = lsTransPt
                    ObjOrder(ObjCount).Number = TPCount
                    ObjCount = ObjCount + 1
                    
                    'position first around from room
                    tmpCoord = GetInsertPos(Room(i).Loc.X, Room(i).Loc.Y, 0, 1)
                    'if valid spot not found (coords didn't change)
                    If tmpCoord.X = Room(i).Loc.X And tmpCoord.Y = Room(i).Loc.Y Then
                      'move it directly above
                      tmpCoord.Y = Room(i).Loc.Y - 2
                    End If
                    .Loc(0) = tmpCoord
                    'position second around target room
                    tmpCoord = GetInsertPos(Room(bytRoom).Loc.X, Room(bytRoom).Loc.Y, 0, 1)
                    'if valid spot not found (coords didn't change)
                    If tmpCoord.X = Room(bytRoom).Loc.X And tmpCoord.Y = Room(bytRoom).Loc.Y Then
                      'move it directly below
                      tmpCoord.Y = Room(i).Loc.Y + 2
                    End If
                    .Loc(1) = tmpCoord
                  End With
                  TPCount = TPCount + 1
                End If
              End If
            End Select
          Else
            'if the target room is not visible, mark it as hidden?
            '*'Debug.Assert Not Logics(i).IsRoom
            '*'Debug.Assert False
            Exits(i)(j).Status = esHidden
          End If
        End If
      Next j
    End If
  Next i
  
  'for rebuilding, there may be comments
  If SavePos Then
    For i = 1 To 255
      With Comment(i)
        If .Visible Then
          'add it to order
          .Order = ObjCount
          ObjOrder(ObjCount).Type = lsComment
          ObjOrder(ObjCount).Number = i
          ObjCount = ObjCount + 1
        End If
      End With
    Next i
  End If
  
  'align to grid, (and adjust size, if extracting)
  For i = 1 To 255
    If Room(i).Visible Then
      With Room(i).Loc
        .X = GridPos(.X)
        .Y = GridPos(.Y)
      End With
    End If

    If TransPt(i).Count > 0 Then
      With TransPt(i).Loc(0)
        'adjust it to be centered around same point that a room
        'would have (by adding .2 to x and .2 to y
        .X = GridPos(.X) + RM_SIZE / 4
        .Y = GridPos(.Y) + RM_SIZE / 4
      End With
      With TransPt(i).Loc(1)
        'adjust it to be centered around same point that a room
        'would have (by adding .2 to x and .2 to y
        .X = GridPos(.X) + RM_SIZE / 4
        .Y = GridPos(.Y) + RM_SIZE / 4
      End With
    End If
    
    If ErrPt(i).Visible Then
      With ErrPt(i).Loc
        'adjust it to be centered around same point that a room
        'would have (by adding .1 to x and .2 to y
        .X = GridPos(.X) + RM_SIZE / 8
        .Y = GridPos(.Y) + RM_SIZE / 4
      End With
    End If
    
    If SavePos Then
      If Comment(i).Visible Then
        With Comment(i)
          .Loc.X = GridPos(.Loc.X)
          .Loc.Y = GridPos(.Loc.Y)
        End With
      End If
    End If
  Next i
  
  'set Max/min values, and reset scrollbars
  AdjustMaxMin
  
  'reset scrollbar minumums
  HScroll1.Min = (MinX - 0.5) * 100
  VScroll1.Min = (MinY - 0.5) * 100
  OffsetX = -(MinX - 0.5)
  OffsetY = -(MinY - 0.5)
  
  'update progress
  With frmProgress
    .pgbStatus.Value = .pgbStatus.Value + 2
    .lblProgress.Caption = "Calculating exit line end points..."
  End With
  
  'initialize exit starting and ending points
  'go backwards so transfer points are correctly assigned
  For i = 255 To 1 Step -1
    If Room(i).Visible Then
      'reposition, but don't need to include both directions
      'since each room will be calling this method in turn
      RepositionRoom i, False
    End If
  Next i
  
  'force save the extracted logic
  SaveLayout
  
  'unload progress form
  Unload frmProgress
  ExtractLayout = True
  
  
  'make sure drawing is re-enabled
  blnDontDraw = False
Exit Function

ErrHandler:
  Resume Next
End Function

Public Function GetExits(ByVal LogicNumber As Byte) As AGIExits

  Dim i As Long
  Dim tmpExits As AGIExits
  
  Set tmpExits = New AGIExits
  
  For i = 0 To Exits(LogicNumber).Count - 1
    With Exits(LogicNumber).Item(i)
      tmpExits.Add(Val(Right$(.ID, 3)), .Room, .Reason, .Style, .Transfer, .Leg).Status = .Status
    End With
  Next i
  
  Set GetExits = tmpExits
End Function

Private Function GetInsertPos(ByVal X As Single, ByVal Y As Single, Optional ByVal Group As Byte = 0, Optional ByVal Distance As Single = 1, Optional SkipStart As Boolean = False) As LCoord
  '
  ' finds one of sixteen empty points around the point X, Y by
  ' checking all Rooms, all transfer points, and all error markers
  '
  ' if all spots are taken, then it returns original values
  '
  ' the spots, in order of search are
  '
  '           04 62
  '           8C EA
  '             X
  '           9D FB
  '           15 73
  '
  ' if SkipStart is true, the starting point (x) is automatically passed over
  ' whether something is there or not
  
  Dim i As Long, Pos As Long
  Dim DeltaX As Single, DeltaY As Single
  Dim tmpX As Single, tmpY As Single
  Dim tmpPos As LCoord
  
  'round off distance
  'Distance = GridPos(Distance)
  'can't be zero
  If Distance < 0.1 Then
    Distance = 0.1
  End If
  
  'if starting position is available
  If Not SkipStart And Not ItemAtPoint(X, Y, Group) Then
    'return starting point
    GetInsertPos.X = X 'GridPos(X)
    GetInsertPos.Y = Y 'GridPos(y)
    Exit Function
  End If
  
  'start at position 0
  Pos = 0
  
  Do
    Do
      'determine x and Y offset for position:
      tmpX = X + IIf(Pos \ 4 Mod 2 = 1, Distance / 2, Distance) * ((Pos And 2) - 1)
      tmpY = Y + IIf(Pos \ 8 = 0, Distance, Distance / 2) * ((Pos And 1) * 2 - 1)
      
      'if any items at this location
      If ItemAtPoint(tmpX, tmpY, Group) Then
        'already occupied
        Exit Do
      End If
      
      'nothing found that occupies this place; return it
      GetInsertPos.X = tmpX
      GetInsertPos.Y = tmpY
      Exit Function
    Loop
    'try next position
    Pos = Pos + 1
  Loop Until Pos = 16
  
  'all spaces occupied- recurse around first position
  tmpPos = GetInsertPos(X - Distance, Y - Distance)
  GetInsertPos = tmpPos
End Function


Private Function GetLayoutData() As Boolean

  '****** change in format from older versions to current (1.2.7+)
  
  'if there is a layout data file, load it,
  'otherwise, extract from current logic/room info
  
  '*****
  'v10, v11 File format:
  'Line 1: cstr(MajorVer) & cstr(MinorVer)
  'Line 2: ObjCount from last layout editor save
  'subsequent lines are one of the following:
  '   R|##|v|o|x|y|ID:room:reason:xfer:style:spx:spy:epx:epy|...
  '   T|##|v|o|x1|y1|x2|y2|c|spx|spy|epx|epy
  '   E|##|v|o|x|y
  '   C|##|v|o|x|y|h|w|{text}
  '   U|##|v|ID:room:reason:style|...
  '   N|##|##|--
  'R,T,E,C = line code indicating object Type
  'U means a room has been modified outside the layout editor, but not
  '   yet saved by the editor save method
  'N means a room that has been renumbered since the last save
  
  'Pipe character (|) used to separate fields
  '## is element number
  'v is visible property (only used by room object; all others are always True)
  'o is object display order
  'x,y are layout coordinates for element
  'e:t:r is exit, Type and target room data for each exit in a room
  'r1 and r2 are the two rooms used by transfer
  '{text} is text of comment/note
  
  '*****
  'v12 File format:
  'Line 1: cstr(MajorVer) & cstr(MinorVer)
  'Line 2: ObjCount from last layout editor save
  'Line 3: DrawScale|DSF|OffsetX|OffsetY
  'Line 4: MinX|MinY|MaxX|MaxY
  'Line 5: HScroll1.Min|HScroll1.Max|HScroll1.Value|VScroll1.Min|VScroll1.Max|VScroll1.Value
  'Line 6: HScroll1.SmallChange|HScroll1.LargeChange|VScroll1.SmallChange|VScroll1.LargeChange
  'subsequent lines are one of the following:
  '   R|##|v|o|p|x|y|ID:room:reason:style:xfer:leg|...
  '   T|##|v|o|x1|y1|x2|y2|c|spx|spy|epx|epy
  '   E|##|v|o|x|y
  '   C|##|v|o|x|y|h|w|{text}
  '   U|##|v|ID:room:reason:style|...
  '   N|##|##|--
  'R,T,E,C = line code indicating object Type
  'U means a room has been modified outside the layout editor, but not
  'yet saved by the editor save method
  'N means a room that has been renumbered since the last save
  
  'Pipe character (|) used to separate fields
  '## is element number
  'v is visible property (only used by room object; all others are always True)
  'o is object display order
  'p is showpic status ***new in v1.2
  'x,y are layout coordinates for element
  'e:t:r is exit, Type and target room data for each exit in a room
  'r1 and r2 are the two rooms used by transfer
  '{text} is text of comment/note
  
  'errors will be difficult to avoid; if errors are found, as long as
  'position information is found, can still try drawing objects, rebuilding exits, then validating results
  'deleting/hiding anything that is not in use or error
  
  
  Dim intFile As Integer, strFileName As String
  Dim stlLayoutData As StringList, lngLine As Long
  
  Dim tmpVisible As Boolean, tmpOrder As Long
  Dim blnError As Boolean, blnWarning As Boolean
  Dim lngRmToUpdate() As Long
  
  Dim strVer As String
  Dim strLine As String, strData() As String
  Dim lngNumber As Long, tmpCoord As LCoord
  Dim Rm1Loc As LCoord, Rm1Size As LCoord, Rm2Loc As LCoord, Rm2Size As LCoord
  Dim i As Long, j As Long, tmpExits As AGIExits
  Dim strExit() As String, Reason As EUReason
  Dim lngCount As Long, lngFileCount As Long
  Dim strErrList As String
  
  'trap errors inline
  On Error Resume Next
  
  'clear variables
  For i = 1 To 255
    With Room(i)
      .Visible = False
      .Loc.X = 0
      .Loc.Y = 0
    End With
    With TransPt(i)
      .Count = 0
      .Loc(0).X = 0
      .Loc(0).Y = 0
      .Loc(1).X = 0
      .Loc(1).Y = 0
      .Room(0) = 0
      .Room(1) = 0
      .ExitID(0) = vbNullString
      .ExitID(1) = vbNullString
    End With
    'err &cmt
    With ErrPt(i)
      .Visible = False
      .FromRoom = 0
      .Room = 0
      .ExitID = ""
      .Loc.X = 0
      .Loc.Y = 0
      .Order = 0
    End With
    With Comment(i)
      .Visible = False
      .Loc.X = 0
      .Loc.Y = 0
      .Size.X = 0
      .Size.Y = 0
      .Order = 0
      .Text = vbNullString
    End With
  Next i
  
  For i = 0 To 1023
    ObjOrder(i).Number = 0
  Next i
  ObjCount = 0
  
  ReDim lngRmToUpdate(0)
  
  'use deselect obj to
  'configure toolbar,statusbar and menus
  DeselectObj
  
  Selection.Type = lsNone
  Selection.Number = 0
  Selection.ExitID = vbNullString
  Selection.Leg = llNoTrans
  MoveExit = False
  MoveObj = False
  IsDirty = False
  SetEditMenu
  
  'use default filename (gameid and wal extension)
  strFileName = GameDir & GameID & ".wal"
  
  'if it is not present
  If Not FileExists(strFileName) Then
    'try the gamefile (take off the 'g', use 'l')
    strFileName = Left(GameFile, Len(GameFile) - 1) & "l"
  
    If Not FileExists(strFileName) Then
      'notify user
      If MsgBoxEx("Layout data for this game is missing." & vbNewLine & "Do you want to extract it automatically?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Layout Editor", WinAGIHelp, "htm\winagi\Layout_Editor.htm") = vbYes Then
        GetLayoutData = ExtractLayout()
      End If
      Exit Function
    End If
  End If
  
  blnLoadingLayout = True
  
  'open layout data file
  intFile = FreeFile()
  Open strFileName For Binary As intFile
  strLine = Space(LOF(intFile))
  Get intFile, 1, strLine
  Close intFile
  Set stlLayoutData = New StringList
  stlLayoutData.Assign strLine
  
  'process file header; use a loop
  'so we can easily skip ahead if an error is encountered
  Do
    'make sure to skip any blank lines
    strLine = Trim(stlLayoutData.StringLine(0))
    If Len(strLine) = 0 Then
      strLine = Trim(stlLayoutData.NextLine(True))
    End If
    'if blank, means no data
    If Len(strLine) = 0 Then
      blnError = True
      Exit Do
    End If
    
    'Line 1: cstr(MajorVer) & cstr(MinorVer)
    strVer = strLine
    Select Case strLine
    Case "10" 'version 1.0
      'mark as dirty so it gets updated to version 1.2
      MarkAsDirty
    Case "11", "12" 'version  1.1, 1.2
      'mark as dirty so it gets updated to v 2.1
      MarkAsDirty
    Case "21"
        'current version
    Case Else
      'error
      'ask user whether to try auto extraction
      If MsgBoxEx("The layout data file for this game is corrupted." & vbNewLine & "Do you want to extract layout information automatically?", vbQuestion + vbYesNo + vbMsgBoxHelpButton, "Layout Editor", WinAGIHelp, "htm\winagi\Layout_Editor.htm#layoutrepair") = vbYes Then
        GetLayoutData = ExtractLayout()
      End If
      blnLoadingLayout = False
      Exit Function
    End Select
    
    'Line 2: ObjCount
    strLine = Trim(stlLayoutData.NextLine(True))
    If Len(strLine) = 0 Then
      blnError = True
      Exit Do
    End If
    ObjCount = Val(strLine)
    lngFileCount = ObjCount
    
    'for version 12, get other lines of information
    If Val(strVer) >= 12 Then
      'Line 3: DrawScale|DSF|OffsetX|OffsetY
      strLine = Trim(stlLayoutData.NextLine(True))
      If Len(strLine) = 0 Then
        blnError = True
        Exit Do
      End If
      'split line
      strData = Split(strLine, "|")
      DrawScale = Val(strData(0))
      'validate it, just in case
      If DrawScale < 1 Then DrawScale = 1
      If DrawScale > 9 Then DrawScale = 9
      
'''      DSF = Val(strData(1))
      'DSF is a calculated value; no need to store/retrieve it
      DSF = 40 * 1.25 ^ (DrawScale - 1)
      
      OffsetX = Val(strData(2))
      OffsetY = Val(strData(3))
      If Err.Number <> 0 Then
        blnError = True
        Err.Clear
        Exit Do
      End If
      picDraw.Font.Size = DSF / 10
      txtComment.Font.Size = DSF / 10
      MainStatusBar.Panels("Scale").Text = "Scale: " & CStr(DrawScale)

      'Line 4: MinX|MinY|MaxX|MaxY
      strLine = Trim(stlLayoutData.NextLine(True))
      If Len(strLine) = 0 Then
        blnError = True
        Exit Do
      End If
      'split line
      strData = Split(strLine, "|")
      MinX = Val(strData(0))
      MinY = Val(strData(1))
      MaxX = Val(strData(2))
      MaxY = Val(strData(3))
      If Err.Number <> 0 Then
        blnError = True
        Err.Clear
        Exit Do
      End If
      
      'Line 5: HScroll1.Min|HScroll1.Max|HScroll1.Value|VScroll1.Min|VScroll1.Max|VScroll1.Value
      strLine = Trim(stlLayoutData.NextLine(True))
      If Len(strLine) = 0 Then
        blnError = True
        Exit Do
      End If
      'split line
      strData = Split(strLine, "|")
      HScroll1.Min = Val(strData(0))
      HScroll1.Max = Val(strData(1))
      HScroll1.Value = Val(strData(2))
      VScroll1.Min = Val(strData(3))
      VScroll1.Max = Val(strData(4))
      VScroll1.Value = Val(strData(5))
      If Err.Number <> 0 Then
        blnError = True
        Err.Clear
        Exit Do
      End If
    
      'Line 6: HScroll1.SmallChange|HScroll1.LargeChange|VScroll1.SmallChange|VScroll1.LargeChange
      strLine = Trim(stlLayoutData.NextLine(True))
      If Len(strLine) = 0 Then
        blnError = True
        Exit Do
      End If
      'split line
      strData = Split(strLine, "|")
      HScroll1.SmallChange = Val(strData(0))
      HScroll1.LargeChange = Val(strData(1))
      VScroll1.SmallChange = Val(strData(2))
      VScroll1.LargeChange = Val(strData(3))
      If Err.Number <> 0 Then
        blnError = True
        Err.Clear
        Exit Do
      End If
    End If
  Loop Until True
  
  'retrieve lines, one at a time (stop if error is found)
  Do While stlLayoutData.CurLine <> stlLayoutData.Count - 1 And Not blnError
    strLine = Trim(stlLayoutData.NextLine(True))
    ' skip blank lines
    If Len(strLine) > 0 Then
      'main loop to parse line; use a loop so we
      'can easily skip ahead to next line
      'if an error is found on a line
      Do
        'split line
        strData = Split(strLine, "|")
        
        'validate basic structure of line:
        Select Case Asc(strData(0))
        Case 78, 85 'N or 'U
          'an update or renumber -
          'should be at least 3 elements
          If UBound(strData) < 2 Then
            'log a warning
            blnWarning = True
            strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": invalid RENUMBER or UPDATE entry"
            'try next line
            Exit Do
          End If
        Case 67, 69, 82, 84 'C, E, R, T
          'should be at least 6 elements
          If UBound(strData) < 5 Then
            'log a warning
            blnWarning = True
            strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": invalid ROOM, TRANSPT, ERRPT or COMMENT entry"
            'try next line
            Exit Do
          End If
        Case Else
          'not sure what this line is; ignore it and try to go on
          blnWarning = True
          strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": line does not contain valid line marker"
          'try next line
          Exit Do
        End Select
        
        'get number
        lngNumber = CLng(strData(1))
        
        'number should be valid; >=1 and <=255
        If lngNumber = 0 Then
          blnWarning = True
          strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": invalid object number ( < 1)"
          'try next line
          Exit Do
        End If
        'number should be <=255
        If lngNumber > 255 Then
          blnWarning = True
          strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": invalid object number ( > 255)"
          'try next line
          Exit Do
        End If
        'next item depends on line code:
        'all but N have visible property as third element
        If Asc(strData(0)) <> 78 Then
          'next number is visible property
          tmpVisible = CBool(strData(2))
        End If
        
        'C,E,R,T have order value; N,U do NOT
        Select Case Asc(strData(0))
        Case 67, 69, 82, 84
          'then comes display order Value
          tmpOrder = Val(strData(3))
          If tmpOrder < 0 Then
            'if negative, then file is corrupt,
            'and probably not recoverable
            blnError = True
            Exit Do
          End If
          If tmpOrder >= ObjCount Then
            'hmm, not right, but maybe we can recover?
            ObjCount = tmpOrder + 1
            'warn user
            blnWarning = True
            strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": invalid object position number"
          End If
          'is this order already in use?
          If ObjOrder(tmpOrder).Type <> lsNone Then
            'problem - but maybe we can fix it by reassigning
            'the object to next available number
            tmpOrder = ObjCount
            ObjCount = ObjCount + 1
            lngCount = lngCount + 1
            'warn user
            blnWarning = True
            strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": invalid object position number"
          End If
        End Select
        
        'handle line data according to Type
        Select Case Asc(strData(0))
        Case 82 '"R" - room info
          'VER 10, 11: 'R|##|v|o|x|y|ID:room:reason:style:xfer:leg:spx:spy:epx:epy|...
          'VER 12:      R|##|v|o|p|x|y|ID:room:reason:style:xfer:leg|...
          
  '''don't need to verify the corresponding logic is actually a room
  '''an update SHOULD be added if the room is no longer visible
  '''
  '''        'make sure there is a valid logic for this update
  '''        If Not Logics.Exists(lngNumber) Then
  '''          ' hopefully there is a valid update, but just in case their
  '''          ' isn't, hide the room
  '''          Room(lngNumber).Visible = False
  '''          Exit Do
  '''        End If
          
          ObjOrder(tmpOrder).Type = lsRoom
          ObjOrder(tmpOrder).Number = lngNumber
          Room(lngNumber).Order = tmpOrder
          Room(lngNumber).Visible = tmpVisible
          
          'for ver 10,11 get coords
          'for ver 12 get pic status and coords
          Select Case strVer
          Case "10", "11"
            tmpCoord.X = CSng(Val(strData(4)))
            tmpCoord.Y = CSng(Val(strData(5)))
          Case "12", "21"
            Room(lngNumber).ShowPic = CBool(strData(4))
            tmpCoord.X = CSng(Val(strData(5)))
            tmpCoord.Y = CSng(Val(strData(6)))
          End Select
          Room(lngNumber).Loc = tmpCoord
          
          'get exit info
          Set Exits(lngNumber) = ParseExits(strLine, strVer)
          
          lngCount = lngCount + 1
          
        Case 85 '"U" - updated room info
          '   U|##|v|ID:room:reason:style|...
          '   update doesn't provide objpos number or position coordinates
          '   because EITHER the object already exists, and show/hide status
          '   is changing,
          '   OR it's a new room, added while layout was closed
          
          'get exit info
          Set tmpExits = New AGIExits
          For i = 0 To UBound(strData) - 3
            strExit = Split(strData(i + 3), ":")
            'should be four elements
            If UBound(strExit) <> 3 Then
              blnWarning = True
              strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": Update line tried to add a non-existent room"
            Else
              'should NEVER use transfer when adding update exits; if one is necessary,
              'it will be added in updateLayout method
              'add new exit, and flag as new, in source
               tmpExits.Add(strExit(0), strExit(1), strExit(2), strExit(3), 0, 0).Status = esOK
            End If
          Next i
          
          'a visible room means we are unhiding an existing room, changing exits on an existing
          'room, or adding a new room
          If tmpVisible Then
            Reason = euUpdateRoom
            'the UpdateLayout function will call the ShowRoom function
            'in the case of a new room being added,
            
            'we do need to check if the room doesn't have a location
            'yet; only rooms currently not visible when an update
            'occurs will be in that situation
            If Not Room(lngNumber).Visible Then
              'need to confirm it's really a room
              If Logics(lngNumber).IsRoom Then
                'change the reason to ShowRoom so that it gets added to the
                'screen with a non-overlapping location
                Reason = euShowRoom
              Else
                blnWarning = True
                strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": Update line tried to show a logic that is not marked as a room"
                Exit Do
              
              End If
            End If
              
            'we DO need to make sure there is a valid logic for this update
            If Not Logics.Exists(lngNumber) Then
              Reason = euRemoveRoom
              'error! warn user
              blnWarning = True
              strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": Update line tried to add a non-existent room"
            End If
          Else
            Reason = euRemoveRoom
          End If
          'update it (showing it if necessary)
          UpdateLayout Reason, lngNumber, tmpExits
          'mark as dirty to force update
          MarkAsDirty
          
        Case 84 '"T" - transfer info  'T|##|v|o|x1|y1|x2|y2|r1|r2|e1|e2|ctr
          'visible flag is ignored for transfer points; counter determines visibility
          
          'set display order
          ObjOrder(tmpOrder).Type = lsTransPt
          ObjOrder(tmpOrder).Number = lngNumber
          With TransPt(lngNumber)
            .Order = tmpOrder
            
            'set first transfer coordinates
            tmpCoord.X = CSng(Val(strData(4)))
            tmpCoord.Y = CSng(Val(strData(5)))
            .Loc(0) = tmpCoord
            'get next pair of coordinates
            tmpCoord.X = CSng(Val(strData(6)))
            tmpCoord.Y = CSng(Val(strData(7)))
            .Loc(1) = tmpCoord
            'get rooms
            .Room(0) = strData(8)
            .Room(1) = strData(9)
            'get exits
            .ExitID(0) = strData(10)
            .ExitID(1) = strData(11)
            'get Count Value
            .Count = CLng(Val(strData(12)))
          End With
          
          lngCount = lngCount + 1
          
        Case 69 '"E" - error info  'E|##|v|o|x|y|r|e|f
          'set object order
          ObjOrder(tmpOrder).Type = lsErrPt
          ObjOrder(tmpOrder).Number = lngNumber
          With ErrPt(lngNumber)
            'set visibility property
            .Visible = tmpVisible
            .Order = tmpOrder
            tmpCoord.X = CSng(Val(strData(4)))
            tmpCoord.Y = CSng(Val(strData(5)))
            .Loc = tmpCoord
            .Room = CLng(strData(6))
            .ExitID = strData(7)
            .FromRoom = strData(8)
          End With
            
          lngCount = lngCount + 1
            
        Case 67 '"C" - comment info  'C|##|v|o|x|y|h|w|{text}
          'set visibility property
          Comment(lngNumber).Visible = tmpVisible
          'set obj order
          ObjOrder(tmpOrder).Type = lsComment
          ObjOrder(tmpOrder).Number = lngNumber
          Comment(lngNumber).Order = tmpOrder
          
          'set coordinates
          tmpCoord.X = CSng(Val(strData(4)))
          tmpCoord.Y = CSng(Val(strData(5)))
          Comment(lngNumber).Loc = tmpCoord
          'extract and set height/width
          tmpCoord.X = CSng(strData(6))
          tmpCoord.Y = CSng(strData(7))
          Comment(lngNumber).Size = tmpCoord
          Comment(lngNumber).Text = Replace(strData(8), "&crlf", vbNewLine)
          
          lngCount = lngCount + 1
          
        Case 78 '"N" - renumbering 'N|##|##|--
          'validate both numbers
          If Val(strData(1)) < 1 Or Val(strData(1)) > 255 Or Val(strData(2)) < 1 Or Val(strData(2)) > 255 Then
            'invalid
              blnWarning = True
              strErrList = strErrList & vbCrLf & "Line " & CStr(stlLayoutData.CurLine + 1) & ": a Renumber action has invalid object numbers"
            Exit Do
          End If
          
          'update
          UpdateLayout euRenumberRoom, Val(strData(1)), Nothing, Val(strData(2))
          'mark as dirty so it gets updated
          MarkAsDirty
        End Select
      'always exit parsing loop
      Loop Until True
    End If
  Loop
  
  'if error was encountered, file is no good
  If blnError Then
    'notify user that layout needs rebuilding
    MsgBoxEx "Errors were encountered  in the layout file that WinAGI cannot repair." & vbNewLine & "The layout needs to be repaired.", vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Layout Editor Errors", WinAGIHelp, "htm\winagi\Layout_Editor.htm#layoutrepair"
    'since form isn't visible, user isn't given a chance to cancel the repair; it will happen automatically
    MenuClickCustom1
  End If
  
  'if no errors, continue post-processing
  On Error Resume Next
  
  'use Reposition method to force all exits to redraw correctly
  For i = 1 To 255
    If Room(i).Visible Then
      'verify the room actually exists
      If Logics.Exists(i) Then
        'also make sure room really IS a room
        If Logics(i).IsRoom Then
          'single direction update only; since every room is called seperately
          RepositionRoom i, False
        Else
          'some how a room is in the layout showing visible that isn't
          'hide it to make it go away
          HideRoom i
        End If
        
      Else
        '*'Debug.Assert False
        'this shouldn't happen if the layout file stays properly synced
        'but if it does happen, we need to hide it to make it go away
        HideRoom i
      End If
      
      'make sure it's not in exact same position as another room
      For j = i + 1 To 255
        If Room(j).Visible And j <> i Then
          Rm1Loc = Room(j).Loc
          Rm1Size.X = RM_SIZE
          Rm1Size.Y = RM_SIZE
          Rm2Loc = Room(i).Loc
          Rm2Size.X = RM_SIZE
          Rm2Size.Y = RM_SIZE
          If IsBehind(Rm1Loc, Rm1Size, Rm2Loc, Rm2Size) Then
            'move it just a little so some of it sticks out
            Room(j).Loc = GetInsertPos(Rm1Loc.X, Rm1Loc.Y, 0, 0.1, True)
          End If
        End If
      Next j
    Else
      'if room is NOT visible, make sure it's NOT a room
      If Logics.Exists(i) Then
        If Logics(i).IsRoom Then
          'BAD - force it to non-room
          Logics(i).IsRoom = False
          strErrList = strErrList & vbCrLf & "Logic " & CStr(i) & " is mismarked as not visible."
          blnWarning = True
        End If
      End If
    End If
  Next i
  
  'ver 12 and above sets scale & scroll values;
  'only need to reset these in v10/v11
  Select Case strVer
  Case "10", "11"
    'set Max/min values
    AdjustMaxMin
    
    'reset scrollbar minumums
    HScroll1.Min = (MinX - 0.5) * 100
    VScroll1.Min = (MinX - 0.5) * 100
    OffsetX = -(MinX - 0.5)
    OffsetY = -(MinY - 0.5)
  Case "12", "21"
    'no action needed
  End Select
  
  'if number of ojects loaded doesn't match the stored value
  'something might be off
  If lngCount <> lngFileCount Then
    'show a warning
    strErrList = strErrList & vbCrLf & "The number of objects placed did not equal the number stored in the layout file"
    blnWarning = True
  End If
  
  If blnWarning Then
    'build the error file
    strErrList = strErrList & vbCrLf & vbCrLf & "Layout Data File contents:"
    With stlLayoutData
      .Add "Layout Data File Error Report", 0
      .Add Date & " " & Time(), 1
      .Add "", 2
      .Add strErrList, 3
    End With
    
    On Error Resume Next
    Kill GameDir & "layout_errors.txt"
    intFile = FreeFile
    Open GameDir & "layout_errors.txt" For Binary As intFile
    Put intFile, 1, Join(stlLayoutData.AllLines, vbCrLf)
    Close intFile
    'notify user that rebuild might be needed
    strLine = "Errors were encountered in the layout file. WinAGI attempted to repair them." & vbNewLine & "You should make sure all our rooms, exits and comments are correctly placed and" & vbNewLine & "then save the layout. Consider using the 'Repair Layout' option if there are" & vbNewLine & "significant discrepancies."
    strLine = strLine & vbNewLine & vbNewLine & "A list of specific issues encountered can be found in the 'layout_errors.txt'" & vbNewLine & "file in your game directory."
    MsgBoxEx strLine, vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Layout Editor Errors", WinAGIHelp, "htm\winagi\Layout_Editor.htm#layoutrepair"
    
    'mark as dirty to force update
    MarkAsDirty
  End If
  
  'return true
  GetLayoutData = True
  
  blnLoadingLayout = False
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Function GetTransfer(ByVal FromRoom As Byte, ByVal ToRoom As Byte, ByVal Dir As EEReason) As Long

  'returns the transfer number between these two rooms, if there is one
  'used by the extractlayout function when building the initial layout
  
  On Error GoTo ErrHandler
  
  Dim i As Long, lngTrans As Long
  
  GetTransfer = -1
  
  'if to room is undefined,
  If ToRoom = 0 Then
    'cant be a transfer
    Exit Function
  End If
  
  'try all exits in 'to' room
  For i = 0 To Exits(ToRoom).Count - 1
    If Exits(ToRoom)(i).Reason = Dir And Exits(ToRoom)(i).Room = FromRoom Then
      'if this exit has a transfer, it can only be valid
      'if it is NOT already two-way (count=2) AND this proposal is
      'a RECIPROCAL exit...
      lngTrans = Exits(ToRoom)(i).Transfer
      If lngTrans > 0 Then
        If TransPt(lngTrans).Count = 1 Then
          '*'Debug.Assert TransPt(lngTrans).Room(0) = ToRoom
          GetTransfer = lngTrans
          'return this transfer
          Exit Function
        End If
      End If
    End If
  Next i
Exit Function

ErrHandler:
  Resume Next
End Function

Private Function ItemAtPoint(ByVal X As Single, ByVal Y As Single, Optional ByVal Group As Byte = 0) As Boolean
  'returns true if there is an item at this location
  'used by the extract layout method
  
  Dim i As Long
  
  'search rooms
  For i = 1 To 255
    If Room(i).Visible Then
      If Room(i).Loc.X = X And Room(i).Loc.Y = Y Then
        If Group = 0 Or ELRoom(i).Group = Group Then
          ItemAtPoint = True
          Exit Function
        End If
      End If
    End If
  Next i
  
  'search transfer points
  For i = 1 To 255
    If TransPt(i).Count > 0 Then
      If (TransPt(i).Loc(0).X = X And TransPt(i).Loc(0).Y = Y) Or (TransPt(i).Loc(1).X = X And TransPt(i).Loc(1).Y = Y) Then
        If Group = 0 Or ELRoom(i).Group = Group Then
          ItemAtPoint = True
          Exit Function
        End If
      End If
    End If
  Next i
  
  'search err points
  For i = 1 To 255
    If ErrPt(i).Visible Then
      If (ErrPt(i).Loc.X = X And ErrPt(i).Loc.Y = Y) Then
        If Group = 0 Or ELRoom(i).Group = Group Then
          ItemAtPoint = True
          Exit Function
        End If
      End If
    End If
  Next i
  
  'nothing here
  ItemAtPoint = False
End Function

Public Sub LoadLayout()

  'set layout in use flag
  LEInUse = True
  
  'load form
  Load Me
  
  'set caption
  Caption = GameID & " - Room Layout"
  
  'open layout data file
  If Not GetLayoutData() Then
    'there was a problem
    Unload Me
    'reset in use flag
    LEInUse = False
    
    Set LayoutEditor = Nothing
    Exit Sub
  End If
  Me.Show
End Sub
Public Function SaveLayout()

  'when closing, this method writes the updated exit/room info to file
  
  'File format:
  'Line 1: cstr(MajorVer) & cstr(MinorVer)
  'Line 2: ObjCount
  'Line 3: DrawScale|DSF|OffsetX|OffsetY
  'Line 4: MinX|MinY|MaxX|MaxY
  'Line 5: HScroll1.Min|HScroll1.Max|HScroll1.Value|VScroll1.Min|VScroll1.Max|VScroll1.Value
  'Line 6: HScroll1.SmallChange|HScroll1.LargeChange|VScroll1.SmallChange|VScroll1.LargeChange
  'subsequent lines are one of the following:
  '   R|##|v|o|p|x|y|ID:room:reason:style:xfer:leg|...
  '   T|##|v|o|x1|y1|x2|y2|r1|r2|xid1|xid2|Count
  '   E|##|v|o|x|y
  '   C|##|v|o|x|y|h|w|{text}
  'R,T,E,C = line code indicating data Type
  'Pipe character (|) used to separate fields
  '## is element number
  'v is visible status (ignore for trans pt)
  'o is order of object when drawing
  'p is showpic status ***new in v1.2
  'x,y are layout coordinates for element
  'r1 and r2 are the two rooms used by transfer
  'xid1, xid2 are exits for each tranfer pt
  '{text} is text of comment/note
  Dim intFile As Integer, strFileName As String
  Dim strLine As String, i As Long, j As Long
  
  On Error GoTo ErrHandler
  
  'open temp layout data file
  strFileName = TempFileName()
  intFile = FreeFile()
  '*'Debug.Assert intFile = 1
  
  Open strFileName For Output As intFile
  
  'write version
  Print #intFile, CStr(App.Major) & CStr(App.Minor)
  'put object Count
  Print #intFile, CStr(ObjCount)
  'drawscale, DSF, offsetx, offsety
  Print #intFile, CStr(DrawScale) & "|" & CStr(DSF) & "|" & CStr(OffsetX) & "|" & CStr(OffsetY)
  'MinX, MinY, MaxX, MaxY
  Print #intFile, CStr(MinX) & "|" & CStr(MinY) & "|" & CStr(MaxX) & "|" & CStr(MaxY)
  'hscroll.min, hscroll.Max, hscroll.Value, vscroll.min, vscroll.Max, vscroll.Value
  Print #intFile, CStr(HScroll1.Min) & "|" & CStr(HScroll1.Max) & "|" & CStr(HScroll1.Value) & "|" & _
                  CStr(VScroll1.Min) & "|" & CStr(VScroll1.Max) & "|" & CStr(VScroll1.Value)
  'HScroll1.SmallChange, HScroll1.LargeChange, VScroll1.SmallChange, VScroll1.LargeChange
  Print #1, CStr(HScroll1.SmallChange) & "|" & CStr(HScroll1.LargeChange) & "|" & CStr(VScroll1.SmallChange) & "|" & CStr(VScroll1.LargeChange)
  
  'add the objects
  For i = 1 To 255
    If Room(i).Visible Then
      'write room
      With Room(i)
        'R|##|v|o|p||x|y|
        strLine = "R|" & CStr(i) & "|" & CStr(.Visible) & "|" & CStr(.Order) & "|" & CStr(.ShowPic) & "|" & CStr(.Loc.X) & "|" & CStr(.Loc.Y)
      End With
      'determine if any exits need updating
      For j = 0 To Exits(i).Count - 1
        'if this exit status is not ok
        If Exits(i)(j).Status <> esOK Then
          'update all exits for this logic
          UpdateLogicCode Logics(i)
          Exit For
        End If
      Next j
      
      'now add the exits
      For j = 0 To Exits(i).Count - 1
        With Exits(i)(j)
          'ID:room:reason:style:xfer:leg|...
          strLine = strLine & "|" & Right$(.ID, 3) & ":" & CStr(.Room) & ":" & CStr(.Reason) & ":" & CStr(.Style) & ":" & CStr(.Transfer) & ":" & CStr(.Leg)
        End With
      Next j
      Print #intFile, strLine
    End If
    
    If TransPt(i).Count > 0 Then
      'write transfer
      With TransPt(i)
        'T|##|v|o|x1|y1|x2|y2|r1|r2|xid1|xid2|Count
        strLine = "T|" & CStr(i) & "|True|" & CStr(.Order) & "|" & CStr(.Loc(0).X) & "|" & CStr(.Loc(0).Y) & "|" & CStr(.Loc(1).X) & "|" & CStr(.Loc(1).Y) & "|" & _
                  CStr(.Room(0)) & "|" & CStr(.Room(1)) & "|" & CStr(.ExitID(0)) & "|" & CStr(.ExitID(1)) & "|" & (.Count)
      End With
      Print #intFile, strLine
    End If
      
    If ErrPt(i).Visible Then
      'write errpt     'E|##|v|o|x|y|r|e|f
      With ErrPt(i)
        'E|##|v|o|x|y|xid
        strLine = "E|" & CStr(i) & "|True|" & CStr(.Order) & "|" & CStr(.Loc.X) & "|" & CStr(.Loc.Y) & "|" & CStr(.Room) & "|" & .ExitID & "|" & CStr(.FromRoom)
      End With
      Print #intFile, strLine
    End If
    
    If Comment(i).Visible Then
  '   C|##|v|o|x|y|h|w|{text}
      'write comment
      With Comment(i)
        strLine = "C|" & CStr(i) & "|True|" & CStr(.Order) & "|" & CStr(.Loc.X) & "|" & CStr(.Loc.Y) & "|" & CStr(.Size.X) & "|" & CStr(.Size.Y) & "|" & Replace(.Text, vbNewLine, "&crlf")
      End With
      Print #intFile, strLine
    End If
  Next i
  
  'close the file
  Close intFile
  
  'kill old file
  On Error Resume Next
  Kill GameDir & GameID & ".wal"
  On Error GoTo ErrHandler
  
  'copy newfile
  '*'Debug.Assert GameID <> vbNullString
  FileCopy strFileName, GameDir & GameID & ".wal"
  
  'if a logic is being previewed, update selection, in case the logic is changed
  If SelResType = rtLogic Then
    UpdateSelection rtLogic, SelResNum, umPreview
  End If
  
  'reset dirty flag
  IsDirty = False
  'set caption
  Caption = GameID & " - Room Layout"
  'disable menu and toolbar button
  frmMDIMain.mnuRSave.Enabled = False
  frmMDIMain.Toolbar1.Buttons("save").Enabled = False
Exit Function

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Function

Private Sub HideRoom(ByVal OldRoom As Byte)

  'mark as not visible, then
  'run through all exits, and if a reference to OldRoom is found
  'mark it as hidden (or as ErrPt, if the logic is gone from the
  'game)
  'also, delete obselete transfer points
  '
  'it is possible that the getlayout method could call this method
  'for a room that is already hidden; in that case, just exit
  
  'also checks to see if any of this room's exits point to errors
  'and deletes them
  
  
  Dim i As Long, j As Long
  Dim lngEP As Long, tmpCoord As LCoord
  
  On Error GoTo ErrHandler
  
  'if there is a selection
  If Selection.Type <> lsNone Then
    DeselectObj
    SetEditMenu
  End If
  
  'first, check for any existing transfers or errpts FROM the room being hidden
  For i = 0 To Exits(OldRoom).Count - 1
    If Exits(OldRoom)(i).Transfer > 0 Then
      'remove the transfer point
      DeleteTransfer Exits(OldRoom)(i).Transfer
    ElseIf Exits(OldRoom)(i).Transfer < 0 Then
      'remove errpt
      With ErrPt(-Exits(OldRoom)(i).Transfer)
        CompactObjList .Order
        .Visible = False
        .ExitID = ""
        .FromRoom = 0
        .Room = 0
        .Loc.X = 0
        .Loc.Y = 0
        .Order = 0
      End With
    End If
  Next i
  
  Room(OldRoom).Visible = False
  'remove from object list
  CompactObjList Room(OldRoom).Order
  'clear the exits for removed room
  Set Exits(OldRoom) = New AGIExits
  
  'step through all exits
  For i = 1 To 255
    'only need to check rooms that are currently visible
    If Room(i).Visible Then
      For j = 0 To Exits(i).Count - 1
        If Exits(i)(j).Room = OldRoom And Exits(i)(j).Status <> esDeleted Then
          'show the exit as hidden, if the target room is still a valid logic
          'otherwise add an ErrPt
          If Logics.Exists(OldRoom) Then
            'mark the exit as hidden
            Exits(i)(j).Status = esHidden
          Else
            'need to replace the exit with an errpoint, if not already done
            If Exits(i)(j).Transfer >= 0 Then
              'insert a error point
              InsertErrPt i, j, OldRoom
            End If
          End If
          
          'check for transfer pt
          If Exits(i)(j).Transfer > 0 Then
            'remove it
            DeleteTransfer Exits(i)(j).Transfer
          End If
          
          'set exit pos
          SetExitPos i, Exits(i)(j).ID
        End If
      Next j
    End If
  Next i
  
  'mark as dirty
  MarkAsDirty
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub ShowRoom()

  Dim tmpExits As AGIExits
  
  'selects a non-room logic, and makes it a room
  'use resource number form
  With frmGetResourceNum
    .WindowFunction = grShowRoom
    .ResType = rtLogic
    'setup before loading so ghosts don't show up
    .FormSetup
    If .lstResNum.ListCount > 0 Then
      .Show vbModal, frmMDIMain
    Else
      Unload frmGetResourceNum
      Exit Sub
    End If
    
    'if not canceled,
    If Not .Canceled Then
    
      'mark room as visible
      Logics(.NewResNum).IsRoom = True
      'update layout and file
    
      'get new exits from the logic that was passed
      Set tmpExits = ExtractExits(Logics(.NewResNum))
      'update layout file
      UpdateLayoutFile euShowRoom, .NewResNum, tmpExits
      'use layout editor update method
      UpdateLayout euShowRoom, .NewResNum, tmpExits
      'and redraw to refresh the editor
      DrawLayout True
      
      'update selection
      UpdateSelection rtLogic, .NewResNum, umProperty
    End If
  End With
  
  'unload
  Unload frmGetResourceNum
End Sub
Public Sub TBClicked(ByVal ButtonIndex As Long)

  'checks to see if button being clicked is currently selected;
  'if so, it sets the HoldTool flag

  If Toolbar1.Buttons(ButtonIndex).Value = tbrPressed Then
    HoldTool = Not HoldTool
    With MainStatusBar.Panels("Tool")
      If HoldTool Then
        .Text = Replace(.Text, "Tool:", "HOLD:")
      Else
        .Text = Replace(.Text, "HOLD:", "Tool:")
      End If
    End With
  End If
End Sub

Private Sub UnselectObj(ByVal rType As Long, ByVal rNumber As Long)
  'removes the object from the selection collection
  
  Dim i As Long, j As Long
  
  'first, find it
  For i = 0 To Selection.Number - 1
    If SelectedObjects(i).Type = rType And SelectedObjects(i).Number = rNumber Then
      'this is the one
      Exit For
    End If
  Next i
  
  'if not found
  If i = Selection.Number Then
    'should never get here; object should always be found
    Exit Sub
  End If
  
  'now move other objects that are ABOVE this one down one spot
  
  For j = i To Selection.Number - 2
    SelectedObjects(j) = SelectedObjects(j + 1)
  Next j
  
  'decrement Count
  Selection.Number = Selection.Number - 1
  
  'if down to a single object
  If Selection.Number = 1 Then
    'change selection
    Selection.Type = SelectedObjects(0).Type
    Selection.Number = Abs(SelectedObjects(0).Number)
    'if second leg of a transpt
    If Selection.Number < 0 Then
      Selection.Leg = 1
    Else
      Selection.Leg = 0
    End If
  End If
End Sub

Public Sub UpdateLayout(ByVal Reason As EUReason, LogicNumber As Long, NewExits As AGIExits, Optional ByVal NewNum As Long)
  
  'updates a room that was modified outside of the layout editor
  'so the layout editor room info matches the external room info
  '
  'rooms can be created new, added to the game, hidden, shown, or edited
  'to force this update
  '
  'this method is also called from the loadlayout method if an update line was
  'added to the file due to one of the changes listed above
  
  'to keep status of transpts accurate, the list of current exits
  'is checked for any transpts; if found, the new list is checked
  'to see if an existing exit exactly matches; if so, the new exit
  'is assigned to the transpt; if no match is found, the transpt
  'is no longer in use; its Count is decremented (and the transpt
  'is deleted if Count goes to zero)
  
  Dim i As Long, j As Long
  Dim tmpTrans As Long, blnKeep As Boolean
  Dim tmpCoord As LCoord, lngErrPt As Long
  Dim Dir As EEReason, AddErrPt As Boolean
  Dim tmpSel As TSel
  
  On Error GoTo ErrHandler
  
  'if there is a selection
  If Selection.Type <> lsNone Then
    DeselectObj
    SetEditMenu
  End If
  
  Select Case Reason
  Case euAddRoom, euShowRoom, euUpdateRoom
    'updateroom will always be to a visible room
    'addnew/shownew will always be not visible until added-
    'regardless of reason, if room is not visible, show it
    If Not Room(LogicNumber).Visible Then
      'no need to get exits; they are already passed as NewExits
      DisplayRoom LogicNumber, (Reason = euShowRoom), True
    End If
    
    'if updating an existing room that has exits already set,
    'need to determine if any of them are exact matches
    'step through existing exits and look for transfer points
    For i = 0 To Exits(LogicNumber).Count - 1
      If Exits(LogicNumber)(i).Transfer <> 0 Then
        'does the update have a corresponding exit?
        For j = 0 To NewExits.Count - 1
          With Exits(LogicNumber)(i)
            If .Reason = NewExits(j).Reason And .Room = NewExits(j).Room And .ID = NewExits(j).ID Then
              'use it in new exit
              NewExits(j).Transfer = .Transfer
              NewExits(j).Leg = .Leg
              'clear it from old exits
              .Transfer = 0
              Exit For
            End If
          End With
        Next j
      End If
    Next i
    
    'run through existing exits again, and delete them
    'DeleteExit method uses a selection object, and handles transfers
    'set the selection object properties to
    'match a single direction exit from the room being updated
    tmpSel.Type = lsExit
    tmpSel.Number = LogicNumber
    tmpSel.TwoWay = ltwOneWay
    
    'step through all current exits and delete them
    For i = Exits(LogicNumber).Count - 1 To 0 Step -1
      'set id so correct exit is deleted
      tmpSel.ExitID = Exits(LogicNumber)(i).ID
      'and delete it
      DeleteExit tmpSel
    Next i
    
    'now add new exits
    With Exits(LogicNumber)
      'first, clear out old exits
      '(the delete exit function doesn't always actually delete;
      'in some cases, it just marks them as 'deletable'
      'so we need to use the Clear method to make them actually go away
      .Clear
      'add new exits
      For i = 0 To NewExits.Count - 1
        AddErrPt = False
        'check for err point first (if exit room=0, Room(NewExits(i).Room).Visible
        'will always be false so this line captures error points regardless if it
        'is because room=0 or room is not visible)
        If Not Room(NewExits(i).Room).Visible Then
          'show err pt if room is not valid (room=0, or logic doesn't exist)
          If NewExits(i).Room = 0 Or Not Logics.Exists(NewExits(i).Room) Then
            'if err pt not yet added,
            If NewExits(i).Transfer = 0 Then
              'add an errpt
              AddErrPt = True
            End If
          Else
            'it's just hidden
            NewExits(i).Status = esHidden
          End If
        Else   'tgt room is visible
          'if this exit does not have a transfer
          If NewExits(i).Transfer = 0 Then
            'check for an existing transfer from the target room
            Select Case NewExits(i).Reason
            Case erHorizon
              Dir = erBottom
            Case erRight
              Dir = erLeft
            Case erBottom
              Dir = erHorizon
            Case erLeft
              Dir = erRight
            Case erOther
              Dir = erOther
            Case Else
              'transfers not allowed for err
              Dir = -1
            End Select
          
            'if NOT err or unknown AND target room is not 0
            If Dir <> -1 Then
              'if this exit is reciprocal:
              'CANT use IsTwoWay function because exit is not added yet
              For j = 0 To Exits(NewExits(i).Room).Count - 1
                With Exits(NewExits(i).Room)(j)
                  'if this exit goes back to original room AND is not deleted
                  If .Room = LogicNumber And .Status <> esDeleted Then
                    'if reason and transfer match
                    If (.Reason = Dir) Then
                      'if transfer exists, and is not already in use two ways
                      If .Transfer <> 0 Then
                        If TransPt(.Transfer).Count <> 2 Then
                          'use this transfer
                          NewExits(i).Transfer = .Transfer
                          NewExits(i).Leg = 1
                          TransPt(NewExits(i).Transfer).Count = 2
                          TransPt(NewExits(i).Transfer).ExitID(1) = .ID
                          Exit For
                        End If
                      End If
                    End If
                  End If
                End With
              Next j
            End If
          End If
        End If
        
        'now add it
        With NewExits(i)
          Exits(LogicNumber).Add(Val(Right$(.ID, 3)), .Room, .Reason, .Style, .Transfer, .Leg).Status = .Status
        End With
        If AddErrPt Then
          InsertErrPt LogicNumber, i, NewExits(i).Room
        End If
      Next i
    End With
    
    RepositionRoom LogicNumber
    
  
  Case euRemoveRoom
    'in case the room is not actually visible, don't call the
    'hide room function; it will delete what it thinks is
    'the current room, but instead it'll delete something else
    If Room(LogicNumber).Visible Then
    '*'Debug.Assert Room(LogicNumber).Visible
      'hide the room
      HideRoom LogicNumber
    End If
    
  Case euRenumberRoom
    'this method adjusts the room number of room (called when a logic number is changed)
    
    'if there's a complex situation of renumbering and deleting logics
    'it's possible that we are renumbering to a logic that doesn't actually exist
    'hopefully there's another update coming that will deal with that
    
    'first copy old room info to new room
    With Room(NewNum)
      .Loc = Room(LogicNumber).Loc
      .Order = Room(LogicNumber).Order
      .Visible = Room(LogicNumber).Visible
    End With
    'copy exit info
    Set Exits(NewNum) = Exits(LogicNumber)
    'reset old exit array
    Set Exits(LogicNumber) = New AGIExits
    
    'clear old room
    With Room(LogicNumber)
      .Order = 0
      .Visible = False
    End With
    
    'now go through all exits, and update room exits
    For i = 1 To 255
      For j = 0 To Exits(i).Count - 1
        If Exits(i)(j).Room = LogicNumber Then
          Exits(i)(j).Room = NewNum
        End If
      Next j
    Next i
    
    'now go through all trans pts
    For i = 0 To 255
      If TransPt(i).Count <> 0 Then
        For j = 0 To 1
          If TransPt(i).Room(j) = LogicNumber Then
            TransPt(i).Room(j) = NewNum
          End If
        Next j
      End If
    Next i
    
    'now go through any errpts
    For i = 0 To 255
      If ErrPt(i).Visible Then
        If ErrPt(i).Room = LogicNumber Then
          ErrPt(i).Room = NewNum
        End If
        If ErrPt(i).FromRoom = LogicNumber Then
          ErrPt(i).FromRoom = NewNum
        End If
      End If
    Next i
    
    'now find the room in object list
    For i = 0 To ObjCount - 1
      If ObjOrder(i).Type = lsRoom Then
        If ObjOrder(i).Number = LogicNumber Then
          ObjOrder(i).Number = NewNum
          Exit For
        End If
      End If
    Next i
  End Select
  
  MarkAsDirty
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Err.Clear
  Resume Next
End Sub



Private Sub UpdateLogicCode(ThisLogic As AGILogic)

  'updates the text for this logic to match current exit status
  '
  'called by save layout; at least one exit needs updating or
  'this method will not have been called
  
  Dim strSource As String, strCR As String
  Dim blnLogLoad As Boolean
  Dim lngPos1 As Long, lngPos2 As Long
  Dim strNewText As String
  Dim i As Long, j As Long
  Dim Index As Long, tmpExit As AGIExit
  Dim blnExitOK As Boolean
  
  On Error GoTo ErrHandler
  
  'ensure source is loaded
  blnLogLoad = ThisLogic.Loaded
  If Not blnLogLoad Then
    ThisLogic.Load
  End If

  'run through update algorithm twice; first to update
  'saved logic file, then check for open logic editor;
  'and update that as well
  'use j as flag to identify what source is being updated;
  
  'check for open logic
  For Index = 1 To LogicEditors.Count
    If LogicEditors(Index).FormMode = fmLogic Then
      If LogicEditors(Index).LogicNumber = ThisLogic.Number Then
        'found it- get source from the rtf box
        strSource = LogicEditors(Index).rtfLogic.Text
        Exit For
      End If
    End If
  Next Index
  If Index = LogicEditors.Count + 1 Then
    'not found; reset index to zero so
    'no attempt will be made to update
    'editor window
    Index = 0
  End If
  
 Do
    'if first time through (j=0)
    If j = 0 Then
      'get source from saved logic
      strSource = ThisLogic.SourceText
      strCR = vbCr
    Else
      'second time through
      If Index = 0 Then
        'not found; dont need to update an editor
        Exit Do
      End If
      'get source from the rtf box
      strSource = LogicEditors(Index).rtfLogic.Text
      'use cr only for cr/line feed (this is a feature(?) of rtf textboxes)
      strCR = vbCr
    End If
  
    'step through exits
    'go backwards so deletions don't screw up the for-next block
    For i = Exits(ThisLogic.Number).Count - 1 To 0 Step -1
      'reset ok flag
      blnExitOK = False
      
      'use a loop structure to handle the exit;
      'loop is exited after the exit is successfully updated
      'in the logic by verifying it is correct, changing it,
      'deleting it or adding it as a new exit
      
      Do
        With Exits(ThisLogic.Number)(i)
          Select Case .Status
          Case esNew
            'look for an existing exit block matching this exit reason?
            'no, this is probably really tough, and not worth the hassle
            
            'find return cmd
            lngPos1 = InStrRev(strSource, "return();", -1, vbTextCompare)
            
            'if not found
            If lngPos1 = 0 Then
              'add to end
              strSource = strSource & NewExitText(Exits(ThisLogic.Number)(i), strCR)
            Else
              'now move to beginning of line
              lngPos2 = InStrRev(strSource, strCR, lngPos1)
              'if 'return()' is on first line,
              If lngPos2 = 0 Then
                'add to beginning
                strSource = NewExitText(Exits(ThisLogic.Number)(i), strCR) & strSource
              Else
                'lngPos2 is where new exit info will be added
                strSource = Left$(strSource, lngPos2) & NewExitText(Exits(ThisLogic.Number)(i), strCR) & Right$(strSource, Len(strSource) - lngPos2 - Len(strCR) + 1)
              End If
            End If
          
            'if second time through, OR no logic window open
            If j = 1 Or Index = 0 Then
              'reset exit status to ok since it now editor, file and logic source are all insync
              Exits(ThisLogic.Number)(i).Status = esOK
            End If
          
            'exit successfully added; exit the do loop and get next exit
            Exit Do
            
          Case esOK, esHidden
            'ok; need to verify that it has not changed
            lngPos2 = InStr(1, strSource, "##" & .ID & "##")
            
            'if found
            If lngPos2 <> 0 Then
              'find new.room cmd,
              lngPos1 = InStrRev(strSource, "new.room(", lngPos2)
              'if found,
              If lngPos1 <> 0 Then
                'verify on same line
                lngPos2 = InStrRev(strSource, strCR, lngPos2)
                If lngPos2 < lngPos1 Then
                  'the new.room cmd is on the same line(because it occurs AFTER
                  'the first CRLF that precedes the exit tag
                
                  'get the exit info for this exit
                  Set tmpExit = AnalyzeExit(strSource, lngPos1)
                  
                  'if reason and room match
                  If tmpExit.Reason = .Reason And tmpExit.Room = .Room Then
                    'exit is ok
                    'make sure exit style match the logic
                    .Style = tmpExit.Style
                    'change is ok
                    blnExitOK = True
                  End If
                End If
              End If
            End If
            
            'if validated,
            If blnExitOK Then
              'exit the do loop, and get next exit
              Exit Do
            Else
              'something was wrong with this exit; ignore
              'the exit with the error, and add this exit
              'as a new exit
              .Status = esNew
            End If
          
          Case esChanged
            'find exit in text, and change it
            lngPos2 = InStr(1, strSource, "##" & .ID & "##")
            
            'if found
            If lngPos2 <> 0 Then
              'find new.room cmd,
              lngPos1 = InStrRev(strSource, "new.room(", lngPos2)
              'if found,
              If lngPos1 <> 0 Then
                'verify on same line
                lngPos2 = InStrRev(strSource, strCR, lngPos2)
                If lngPos2 < lngPos1 Then
                  'the new.room cmd is on the same line(because it occurs AFTER
                  'the first CRLF that precedes the exit tag)
                  'save location of new.room command
                  lngPos2 = lngPos1
                  
                  'get the exit info for this exit
                  Set tmpExit = AnalyzeExit(strSource, lngPos1)
                  
                  'if reason matches
                  If tmpExit.Reason = .Reason Then
                    'change room in logic to match the exit
                    'lngPos1 gets changed by AnalyzeExit function, so need to use saved Value
                    
                    'adjust to opening parenthesis after 'new.room'
                    lngPos1 = lngPos2 + 8
                    
                    'find closing parenthesis
                    lngPos2 = InStr(lngPos1, strSource, ")")
                    
                    'if found,
                    If lngPos2 <> 0 Then
                      'insert new room here
                      'strSource = Left$(strSource, lngPos1) & CStr(.Room) & Right$(strSource, Len(strSource) - lngPos2 + 1)
                      strSource = Left$(strSource, lngPos1) & Logics(.Room).ID & Right$(strSource, Len(strSource) - lngPos2 + 1)
                      'reset exit style to match the logic
                      .Style = tmpExit.Style
                      'change is ok
                      blnExitOK = True
                    End If
                  End If
                End If
              End If
            End If
            
            'if changed successfully
            If blnExitOK Then
              'if second time through, OR no logic window open
              If j = 1 Or Index = 0 Then
                'reset exit status to ok since it now editor, file and logic source are all insync
                Exits(ThisLogic.Number)(i).Status = esOK
              End If
              
              Exit Do
            Else
              'something was wrong with this exit; ignore
              'the exit with the error, and add this exit
              'as a new exit
              .Status = esNew
            End If
            
          Case esDeleted
            'find exit in source and delete/comment it out;
            
            lngPos2 = InStr(1, strSource, "##" & .ID & "##")
            
            'if found
            If lngPos2 <> 0 Then
              'find beginning of line (adjust for width of cr-lf)
              lngPos1 = InStrRev(strSource, strCR, lngPos2) + Len(strCR) - 1
              If lngPos1 = 0 Then
                lngPos1 = 1
              End If
            
              'insert new comment in front of line
              strNewText = "[ DELETED BY LAYOUT EDITOR " & strCR & "[ "
              
              'insert comment in front of line
              strSource = Left$(strSource, lngPos1) & strNewText & Right$(strSource, Len(strSource) - lngPos1)
              
              'adjust lngPos2 to beginning of tag (by finding comment character)
              lngPos2 = InStr(lngPos1, strSource, "[ #")
              'now delete the LE tag
              strSource = Left$(strSource, lngPos2 - 1) & Right$(strSource, Len(strSource) - lngPos2 - 10)
              
            'Else
              'not found;
              
              'if updating the source file, this should never happen,
              'unless the file was modified outside of WinAGI GDS or something corrupted
              'the layout or the source file; in either case, ignore this problem
              'for updates to the sourcefile
              
              'if updating an open logic editor, the most likely cause is that the
              'user manually edited the logic source, and probably deleted this exit
              'since it is already gone, no action is necessary
            End If
            
            'if second time through, OR no logic window open
            If j = 1 Or Index = 0 Then
              'remove exit, since editor, file and logic source are now insync
              Exits(ThisLogic.Number).Remove i
            End If
            
            'exit successfully deleted; exit do loop and get next exit
            Exit Do
            
          End Select
        End With
      'only way to exit loop is to successfully update the exit info
      Loop While True
    Next i
  
    'if currently updating the file
    If j = 0 Then
      'save source
      ThisLogic.SourceText = strSource
      ThisLogic.SaveSource
      'mark as dirty
      SetLogicCompiledStatus ThisLogic.Number, False
      
      'setup to check for open editor
      j = 1
    Else
      'update editor
      LogicEditors(Index).rtfLogic.Text = strSource
      'exit
      Exit Do
    End If
  Loop While True
  
  'unload if necessary
  If Not blnLogLoad And ThisLogic.Loaded Then
    ThisLogic.Unload
  End If
Exit Sub

ErrHandler:
  Resume Next
End Sub

Public Sub ExtendVScroll(ByVal Dir As Long)

  On Error GoTo ErrHandler
  
  'if scroll bars are clicked when already at edge, adjust Max/min to allow continued scrolling
  Select Case Dir
  Case SB_LINEUP
    'if at min
    If VScroll1.Value = VScroll1.Min Then
      'adjust min to continue scrolling
      VScroll1.Min = VScroll1.Min - VScroll1.SmallChange
    End If
    
  Case SB_LINEDOWN
    'if at Max
    If VScroll1.Value = VScroll1.Max Then
      VScroll1.Max = VScroll1.Max + VScroll1.SmallChange
    End If
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub ExtendHScroll(ByVal Dir As Long)

  On Error GoTo ErrHandler
  
  'if scroll bars are clicked when already at edge, adjust Max/min to allow continued scrolling
  Select Case Dir
  Case SB_LINELEFT
    'if at min
    If HScroll1.Value = HScroll1.Min Then
      'adjust min to continue scrolling
      HScroll1.Min = HScroll1.Min - HScroll1.SmallChange
    End If
    
  Case SB_LINERIGHT
    'if at Max
    If HScroll1.Value = HScroll1.Max Then
      'adjust Max to continue scrolling
      HScroll1.Max = HScroll1.Max + HScroll1.SmallChange
    End If
    
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Public Sub VScrollProc(ByVal Dir As Long)
  
  'performs a scroll action based on
  Select Case Dir
  Case 1
    'if not at min
    If VScroll1.Value > VScroll1.Min Then
      If VScroll1.Value - VScroll1.SmallChange < VScroll1.Min Then
        VScroll1.Value = VScroll1.Min
      Else
        VScroll1.Value = VScroll1.Value - VScroll1.SmallChange
      End If
    End If
    
  Case -1
    'if not at Max
    If VScroll1.Value < VScroll1.Max Then
      If VScroll1.Value + VScroll1.SmallChange > VScroll1.Max Then
        VScroll1.Value = VScroll1.Max
      Else
        VScroll1.Value = VScroll1.Value + VScroll1.SmallChange
      End If
    End If
  End Select
End Sub

Private Sub Form_Activate()
  
  On Error GoTo ErrHandler
  
  'if minimized, exit
  '(to deal with occasional glitch causing focus to lock up)
  If Me.WindowState = vbMinimized Then
    Exit Sub
  End If
  
  'if hiding prevwin on lost focus, hide it now
  If Settings.HidePreview Then
    PreviewWin.Hide
  End If
 
  'if visible,
  If Visible Then
    'force resize
    Form_Resize
  End If
  
  'adjust menus and statusbar
  AdjustMenus rtLayout, True, True, IsDirty
  
  'if findform is visible,
  If FindForm.Visible Then
    'hide it it
    FindForm.Visible = False
  End If
  
  'set edit menu
  SetEditMenu
  
  'always set focus to picdraw
  picDraw.SetFocus
  
  'redraw status bar
  With MainStatusBar.Panels
    .Item("Scale").Text = "Scale: " & CStr(DrawScale)
    Select Case SelTool
    Case ltSelect
      .Item("Tool").Text = "Select"
      Select Case Selection.Type
      Case lsRoom
      Case lsComment
      Case lsExit
      Case lsTransPt
      Case lsErrPt
      Case lsMultiple
      End Select
      
    Case ltEdge1
      .Item("Tool").Text = "One Way Edge"
    Case ltEdge2
      .Item("Tool").Text = "Two Way Edge"
    Case ltOther
      .Item("Tool").Text = "Other Exit"
    Case ltRoom
      .Item("Tool").Text = "New Room"
    Case ltComment
      .Item("Tool").Text = "New Comment"
    End Select
    If HoldTool Then
      .Item("Tool").Text = "HOLD: " & .Item("Tool").Text
    End If
  End With
  Err.Clear
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub Form_KeyDown(KeyCode As Integer, Shift As Integer)
  
  'always hide tip if showing
  If picTip.Visible Then
    picTip.Visible = False
  End If
  
  'check for help first
  If KeyCode = vbKeyF1 And Shift = 0 Then
    MenuClickHelp
    KeyCode = 0
    Exit Sub
  End If
  
  'if editing a comment
  If txtComment.Visible Then
    Exit Sub
  End If
  
  'check for global shortcut keys
  CheckShortcuts KeyCode, Shift
  If KeyCode = 0 Then
    Exit Sub
  End If
  
  Select Case Shift
  Case 0  'no shift
    Select Case KeyCode
    Case vbKeyDelete
      If frmMDIMain.mnuEDelete.Enabled Then
        MenuClickDelete
      End If
    End Select
    
  Case vbCtrlMask
    Select Case KeyCode
    Case vbKeyA
      If frmMDIMain.mnuESelectAll.Enabled Then
        MenuClickSelectAll
      End If
    
    Case vbKeyD
      'verify a room is selected
      If frmMDIMain.mnuECustom2.Enabled Then
        MenuClickECustom2
      End If
      
    Case vbKeyR
      If frmMDIMain.mnuECustom1.Enabled Then
        MenuClickECustom1
        KeyCode = 0
      End If
      
    End Select
    
  Case vbAltMask
    Select Case KeyCode
    Case vbKeyL
      If frmMDIMain.mnuECopy.Enabled Then
        MenuClickCopy
        KeyCode = 0
      End If
      
    Case vbKeyR
      'repair layout
      If frmMDIMain.mnuRCustom1.Enabled Then
        MenuClickCustom1
        KeyCode = 0
      End If
    
    Case vbKeyO
      If frmMDIMain.mnuEFind.Enabled Then
        MenuClickFind
        KeyCode = 0
      End If
    
    End Select
  
  Case vbShiftMask
    Select Case KeyCode
    Case vbKeyInsert
      If frmMDIMain.mnuEInsert.Enabled Then
        MenuClickInsert
        KeyCode = 0
      End If
    End Select
    
  Case 3  'shift+ctrl
    Select Case KeyCode
    Case vbKeyH
      If frmMDIMain.mnuECut.Enabled Then
        If Left$(frmMDIMain.mnuECut.Caption, 5) = "&Hide" Then
          MenuClickCut
          KeyCode = 0
        End If
      End If
              
    Case vbKeyS
      If frmMDIMain.mnuECut.Enabled Then
        If Left$(frmMDIMain.mnuECut.Caption, 5) = "&Show" Then
          MenuClickCut
          KeyCode = 0
        End If
      End If
    End Select
    
  Case 6 'ctrl+alt
    If KeyCode = vbKeyH And Asc(frmMDIMain.mnuRCustom2.Caption) = 72 Then 'H
      MenuClickCustom2
    End If
    If KeyCode = vbKeyS And Asc(frmMDIMain.mnuRCustom2.Caption) = 83 Then 'S
      MenuClickCustom2
    End If
    
  End Select
End Sub

Public Sub MenuClickECustom1()
  
  On Error GoTo ErrHandler
  
  'toggle show room pic status
  Room(Selection.Number).ShowPic = Not Room(Selection.Number).ShowPic

  'redraw layout
  DrawLayout
  'reset edit menu
  If Room(Selection.Number).ShowPic Then
    frmMDIMain.mnuECustom1.Caption = "Hide Room Picture" & vbTab & "Ctrl+R"
  Else
    frmMDIMain.mnuECustom1.Caption = "Show Room Picture" & vbTab & "Ctrl+R"
  End If
  
  'mark as dirty
  MarkAsDirty
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Public Sub MenuClickECustom2()
  
  Dim strID As String, strDescription As String
  Dim tmpForm As Form
  
  On Error GoTo ErrHandler
  
  'should only be called if a room is selected
  If Selection.Type <> lsRoom Then
    Exit Sub
  End If
  
  strID = Logics(Selection.Number).ID
  strDescription = Logics(Selection.Number).Description
  
  'use the id/description change method
  If GetNewResID(rtLogic, Selection.Number, strID, strDescription, True, 1) Then
    'redraw layout to reflect changes
    DrawLayout
    
    'mark as dirty
    MarkAsDirty
    
    'if a matching logic editor is open, it needs to be updated too
    If LogicEditors.Count > 0 Then
      For Each tmpForm In LogicEditors
        If tmpForm.LogicEdit.Number = Selection.Number Then
          tmpForm.UpdateID strID, strDescription
          Exit For
        End If
      Next
    End If
    
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub Form_Load()

  Dim i As Long
  
  'set background color
  picDraw.BackColor = agOffWhite
  
  CalcWidth = MIN_WIDTH
  CalcHeight = MIN_HEIGHT
  
  'initialize exit objects
  For i = 1 To 255
    Set Exits(i) = New AGIExits
  Next i
  
  ObjCount = 0
  
  'setup fonts and colors
  InitFonts
  
  SelTool = ltSelect
  
  'get default scale
  DrawScale = Settings.LEZoom
  
  DSF = 40 * 1.25 ^ (DrawScale - 1)
  picDraw.Font.Size = DSF / 10
  txtComment.Font.Size = DSF / 10
  
  'default print scale is 100%
  PrintScale = 1
  
#If DEBUGMODE <> 1 Then
  'subclass toolbar
  PrevTbarWndProc = SetWindowLong(Toolbar1.hWnd, GWL_WNDPROC, AddressOf TbarWndProc)
  
  'subclass main window
  PrevLEWndProc = SetWindowLong(LayoutEditor.hWnd, GWL_WNDPROC, AddressOf LEMainWndProc)
#End If
End Sub

Private Sub Form_QueryUnload(Cancel As Integer, UnloadMode As Integer)

  Cancel = Not AskClose()
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
  '*'Debug.Print CalcWidth, CalcHeight
  'if the form is not visible
  If Not Visible Then
    Exit Sub
  End If
  
  'if not minimized
  If Me.WindowState <> vbMinimized Then
    'prevent drawing issues when resizing
    blnDontDraw = True
  
    'position scrollbars
    HScroll1.Width = CalcWidth - 17 - Toolbar1.Width
    picDraw.Width = HScroll1.Width
    VScroll1.Left = CalcWidth - 17
    
    HScroll1.Top = CalcHeight - 17
    VScroll1.Height = CalcHeight - 17
    picDraw.Height = CalcHeight - 17
    
    blnDontDraw = False
    DrawLayout
  End If
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub
Private Sub Form_Unload(Cancel As Integer)

  Dim i As Long
  
  On Error GoTo ErrHandler
  
  'reset in use flag
  LEInUse = False
  
  'dereference exits
  For i = 1 To 255
    Set Exits(i) = Nothing
  Next i

#If DEBUGMODE <> 1 Then
  'release subclass hook to toolbar
  i = SetWindowLong(Toolbar1.hWnd, GWL_WNDPROC, PrevTbarWndProc)
  'release subclass hook to main window
  i = SetWindowLong(LayoutEditor.hWnd, GWL_WNDPROC, PrevLEWndProc)
#End If
  'release object
  Set LayoutEditor = Nothing
  
  'need to check if this is last form
  LastForm Me
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub HScroll1_Change()
  
  On Error GoTo ErrHandler
  
  'always make sure tip is hidden
  If picTip.Visible Then
    picTip.Visible = False
  End If
  
  'ignore during startup (form not visible) or if due to code change
  If Not Me.Visible Or CodeChange Then
    Exit Sub
  End If
  
  'change offset Value to match new scrollbar Value
  OffsetX = -HScroll1.Value / 100
    
  'check to see if min or Max Value need to be adjusted
  'due to scrolling past edge, then scrolling back...
  
  'if > min
  If HScroll1.Value > HScroll1.Min Then
    'compare scroll min against actual min
    If HScroll1.Min < (MinX - 0.5) * 100 Then
      HScroll1.Min = (MinX - 0.5) * 100
    End If
  End If
  
  'if < Max
  If HScroll1.Value < HScroll1.Max Then
    'compare scroll Max against actual Max
    If HScroll1.Max > (MaxX + 1.3 - picDraw.Width / DSF) * 100 Then
      'also need to check that Max Value is not less than current min
      'if picDraw is wider than drawing area
      '(meaning the calculated Max Value is less than current min)
      If (MaxX + 1.3 - picDraw.Width / DSF) * 100 < HScroll1.Min Then
        'set Max and min equal
        HScroll1.Max = HScroll1.Min
      Else
        HScroll1.Max = (MaxX + 1.3 - picDraw.Width / DSF) * 100
      End If
    End If
  End If

  'redraw
  DrawLayout
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub HScroll1_GotFocus()

  On Error Resume Next
  
  '????why does this cause an error when closing a game?
  picDraw.SetFocus
End Sub

Private Sub HScroll1_Scroll()
  
  HScroll1_Change
End Sub

Private Sub picDraw_DblClick()
  
  Dim tmpSel As TSel
  Dim tmpInfo As ObjInfo, blnSecond As Boolean
  Dim tmpScroll As Long
  Dim i As Long, j As Long
  Dim strUpdate As String
  
  On Error GoTo ErrHandler
  
  'if over a selected comment
  '(can tell because cursor is i-beam)
  If picDraw.MousePointer = vbIbeam Then
    'call click event again to select the comment
    picDraw_MouseDown vbLeftButton, 0, OldX, OldY
    Exit Sub
  End If
  
  'if over a selected transpt
  
  Do
    'look for edge exits first
    ExitFromPos tmpSel, 1, OldX, OldY
    If tmpSel.Type <> lsNone Then
      Exit Do
    End If
    
    ExitFromPos tmpSel, 0, OldX, OldY
    If tmpSel.Type <> lsNone Then
      Exit Do
    End If
    
    ObjectFromPos tmpSel, OldX, OldY
  Loop Until True
  
  'if over selected transpt, room or errpt
  Select Case tmpSel.Type
  Case lsTransPt
    MenuClickFind
  Case lsRoom, lsExit, lsErrPt
    MenuClickCopy
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub picDraw_KeyDown(KeyCode As Integer, Shift As Integer)
  
  Dim sngOffset As Single
  
  On Error GoTo ErrHandler
  
  'arrow keys move the selection;
  '  - no shift = move one grid amount
  '  - shift key = move 4x grid amount
  '  - ctrl key = move one pixel
  
  Select Case Shift
  Case 0 'no shift key
    'if something is selected
    If Selection.Type <> lsNone Then
      Select Case KeyCode
      Case vbKeyUp, vbKeyDown, vbKeyLeft, vbKeyRight
        If Settings.LEUseGrid Then
          'offset is one grid amount
          sngOffset = Settings.LEGrid * DSF
        Else
          'use 0.1
          sngOffset = 0.1 * DSF
        End If
        
        KeyMoveSelection KeyCode, sngOffset, False
        
      End Select
    End If
    
  Case vbShiftMask 'shift key
    'if something is selected
    If Selection.Type <> lsNone Then
      Select Case KeyCode
      Case vbKeyUp, vbKeyDown, vbKeyLeft, vbKeyRight
        If Settings.LEUseGrid Then
          'offset is 4X grid amount
          sngOffset = 4 * Settings.LEGrid * DSF
        Else
          'use 0.4
          sngOffset = 0.4 * DSF
        End If
        
        KeyMoveSelection KeyCode, sngOffset, False
      End Select
    End If
    
  Case vbCtrlMask
    
    Select Case KeyCode
    Case vbKeyUp, vbKeyDown, vbKeyLeft, vbKeyRight
      'offset is one pixel
      sngOffset = 1
      
      KeyMoveSelection KeyCode, sngOffset, True
      
    Case 17 'ctrl key by itself
      'if ctrl is pressed
      ' AND something selected
      ' AND tool is select
      ' AND not dragging or moving
      ' AND not already set,
      If SelTool = ltSelect And Not DragSelect And Not MoveObj Then
        Select Case Selection.Type
        Case lsComment, lsErrPt, lsRoom, lsTransPt
          'just exit if Ctrl key is already pressed (being held down)
          If CtrlDown Then Exit Sub
    
          If CustMousePtr <> ccAddObj Then
            Set picDraw.MouseIcon = LoadResPicture("ELC_ADDOBJ", vbResCursor)
            picDraw.MousePointer = vbCustom
            CustMousePtr = ccAddObj
            'we need to track if the Ctrl key is pressed or not;
            'while it's held, this event keeps getting called
            'and we only want to run it ONCE when it first is
            'pressed
            CtrlDown = True
          End If
        End Select
      End If
    End Select
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub picDraw_KeyUp(KeyCode As Integer, Shift As Integer)

  'if no Ctrl key, always reset the flag
  If ((Shift And vbCtrlMask) <> vbCtrlMask) Then
    CtrlDown = False
    'if no ctrl key, and was previously, restore normal cursor
    If CustMousePtr = ccAddObj Then
      picDraw.MousePointer = vbDefault
      CustMousePtr = ccNone
    End If
  End If
End Sub


Private Sub picDraw_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim tmpSel As TSel, tmpLogic As AGILogic
  Dim ExitType As EEReason
  Dim i As Long
  Dim tmpNode As Node, tmpExits As AGIExits
  
  On Error GoTo ErrHandler
  
  'always save x and y so they can be
  'used by the double-click event
  OldX = X
  OldY = Y
  
  'if right click
  If Button = vbRightButton Then
    'disable tip timer
    tmrTip.Enabled = False
    
    'reset mouse pointer
    picDraw.MousePointer = vbDefault
    CustMousePtr = ccNone
    
    'if in a drawing mode,
    If SelTool <> ltSelect Then
      Toolbar1_ButtonClick Toolbar1.Buttons("select")
      Toolbar1.Buttons("select").Value = tbrPressed
    End If
    
    
    'check for a click on an exit or object
    Do
      'edgeexit has priority
      ExitFromPos tmpSel, 1, X, Y
      If tmpSel.Type = lsExit Then
        'found
        Exit Do
      End If
      'other exit is next priority
      ExitFromPos tmpSel, 0, X, Y
      If tmpSel.Type = lsExit Then
        Exit Do
      End If
      'then we check for an object
      ObjectFromPos tmpSel, X, Y
    Loop Until True

    'if selection is a multi
    If Selection.Type = lsMultiple Then
      'if the cursor is over an exit, OR NOT over something in selection collection
      If tmpSel.Type = lsExit Or Not IsSelected(tmpSel.Type, tmpSel.Number, tmpSel.Leg) Then
        'call mousedown event with left button
        'to select whatever is under cursor BEFORE
        'showing context menu
        picDraw_MouseDown vbLeftButton, 0, X, Y
        picDraw_MouseUp vbLeftButton, 0, X, Y
      End If
    Else
      'if selection is NOT the same
      If Not SameAsSelection(tmpSel) Then
        'call mousedown event with left button
        'to select whatever is under cursor BEFORE
        'showing context menu
        picDraw_MouseDown vbLeftButton, 0, X, Y
        picDraw_MouseUp vbLeftButton, 0, X, Y
      End If
    End If
    
    'make sure this form is the active form
    If Not (frmMDIMain.ActiveForm Is Me) Then
      'set focus before showing the menu
      Me.SetFocus
    End If
    'need doevents so form activation occurs BEFORE popup
    'otherwise, errors will be generated because of menu
    'adjustments that are made in the form_activate event
    SafeDoEvents
    'right button only used for context menu
    PopupMenu frmMDIMain.mnuEdit
    
    're-enable tip timer
    tmrTip.Enabled = True
    Exit Sub
  End If
  
  'if resizing a comment,
  '(can tell by checking pointer)
  If picDraw.MousePointer = vbSizeNESW Then
    'upper right or lower left corner;
    If X >= Selection.X2 Then
      'upper right
      SizingComment = 2
    Else
      'lower left
      SizingComment = 3
    End If
    shpMove.Left = Selection.X1 + 8
    shpMove.Top = Selection.Y1 + 8
    shpMove.Width = Selection.X2 - Selection.X1 - 9
    shpMove.Height = Selection.Y2 - Selection.Y1 - 9
    shpMove.Shape = vbShapeRoundedRectangle
    shpMove.Visible = True
    Exit Sub
    
  ElseIf picDraw.MousePointer = vbSizeNWSE Then
    'lower right or upper left corner;
    If X >= Selection.X2 Then
      'lower right
      SizingComment = 4
    Else
      'upper left
      SizingComment = 1
    End If
    shpMove.Left = Selection.X1 + 8
    shpMove.Top = Selection.Y1 + 8
    shpMove.Width = Selection.X2 - Selection.X1 - 9
    shpMove.Height = Selection.Y2 - Selection.Y1 - 9
    shpMove.Shape = vbShapeRoundedRectangle
    shpMove.Visible = True
    Exit Sub
  End If
    
  Select Case SelTool
  Case ltSelect
    'a LOT of things to check if the selection tool
    'is active; strategy is to first check for
    'dragging of an object, then we see if something
    'is clicked; if nothing is clicked, we can just exit
    'then we process whatever was clicked (object or
    'exit) depending on what the current selection is
    
    'check for drag-select operation
    If (picDraw.Point(X, Y) = picDraw.BackColor Or picDraw.Point(X, Y) = RGB(128, 128, 128)) And Button = vbLeftButton And Shift = 0 Then
      'begin drag-select
      DragSelect = True
      AnchorX = X
      AnchorY = Y
      shpMove.Shape = vbShapeRectangle
      shpMove.Visible = False
      Exit Sub
    End If
     
    'check for a click on an edge or object
    Do
      'edgeexit has priority
      ExitFromPos tmpSel, 1, X, Y
      If tmpSel.Type = lsExit Then
        'found
        Exit Do
      End If
      'other exit is next priority
      ExitFromPos tmpSel, 0, X, Y
      If tmpSel.Type = lsExit Then
        Exit Do
      End If
      'then we check for an object
      ObjectFromPos tmpSel, X, Y
    Loop Until True
    
    'if nothing new is clicked on,
    'we can just deselect whatever is currently
    'selected and be done
    If tmpSel.Type = lsNone Then
      'before deslecting and exiting, we check
      'to see if Shift key is pressed; that means
      'user wants to 'drag' the canvas, changing
      'the current view
      If Shift = vbShiftMask Then
        'begin dragging the canvas
        DragCanvas = True
        AnchorX = X
        AnchorY = Y
        picDraw.MousePointer = vbCustom
        Set picDraw.MouseIcon = LoadResPicture("EPC_MOVE", vbResCursor)
      Else
        'nothing going on; make sure nothing is
        'selected
        DeselectObj
      End If
      Exit Sub
    End If
    
    'if current selection (not the newly clicked
    'object/exit) is more than one object,
    'check for clicks that expand it or deselect
    'one of its objects before checking single
    'selection actions
    If Selection.Type = lsMultiple Then
      For i = 0 To Selection.Number - 1
        If SelectedObjects(i).Type = tmpSel.Type And Abs(SelectedObjects(i).Number) = tmpSel.Number Then
          'need to validate trans pt
          If SelectedObjects(i).Type = lsTransPt Then
            If SelectedObjects(i).Number < 0 Then
              If tmpSel.Leg = 1 Then
                'this is it
                Exit For
              End If
            Else
              If tmpSel.Leg = 0 Then
                'this is it
                Exit For
              End If
            End If
          Else
            'found room, errpt or comment
            Exit For
          End If
        End If
      Next i
      
      'if object being clicked IS in the current collection
      '(i WONT equal selection.number [the total number of objects in
      'the selection group])
      If i <> Selection.Number Then
        'if ctrl key is pressed
        If Shift = vbCtrlMask Then
          'un-select this object
          UnselectObj tmpSel.Type, IIf(tmpSel.Type = lsTransPt And tmpSel.Leg = 1, -tmpSel.Number, tmpSel.Number)
          'do i need to reselect? -no; redraw!
          DrawLayout True
          Exit Sub
        Else
          With shpMove
            'begin moving selected objects
            .Shape = vbShapeRectangle
            .Visible = True
            'get offset between x and selection shape
            '(for multiple selection, the shape doesn't include the
            ' 8 pixel offset; that only applies to single objects that
            ' need the black 'handles' drawn)
            mDX = Selection.X1 - X
            mDY = Selection.Y1 - Y
            
            'save anchor position prior to movement
            If Selection.Type = lsMultiple Then
              'anchor point is real world location of upper left corner selection shape
              AnchorX = .Left / DSF - OffsetX
              AnchorY = .Top / DSF - OffsetY
            Else
              'anchor point is current mouse pos
              AnchorX = X
              AnchorY = Y
            End If
          End With
          Set picDraw.MouseIcon = LoadResPicture("ELC_MOVING", vbResCursor)
          MoveObj = True
          Exit Sub
        End If
      Else
        'is user adding to collection?
        '(check for control key)
        If Shift = vbCtrlMask Then
          Select Case tmpSel.Type
          Case lsComment, lsErrPt, lsRoom, lsTransPt
            'add it to collection
            ReDim Preserve SelectedObjects(Selection.Number)
            With SelectedObjects(Selection.Number)
              .Type = tmpSel.Type
              'check for trans pt leg
              If tmpSel.Leg = 1 Then
                'its a second leg trans pt
                .Number = -tmpSel.Number
              Else
                'its a first leg trans pt or another object Type
                .Number = tmpSel.Number
              End If
            End With
            'increment Count
            Selection.Number = Selection.Number + 1
            'force redraw
            DrawLayout
            'exit so selection isn't wiped out by the
            'selection processing code below
            Exit Sub
          End Select
        End If
        
        'not adding new; continue with normal
        'processing of determining selection
        'reset multiple selection
        DeselectObj
      End If
    End If

    'depending on currently selected object,
    'process the click to either select something else
    'or move/take action on selection
    Select Case Selection.Type
    Case lsRoom, lsTransPt, lsErrPt, lsComment
      'if same as current selection
      If SameAsSelection(tmpSel) Then
        'if over a comment object's text area (cursor is ibeam)
        If Selection.Type = lsComment And picDraw.MousePointer = vbIbeam Then
          'draw comment on top
          DrawCmtBox Selection.Number
          'begin editing text in comment box
          With Comment(Selection.Number)
            txtComment.Left = (.Loc.X + OffsetX) * DSF + 6
            txtComment.Top = (.Loc.Y + OffsetY) * DSF + 4
            txtComment.Width = .Size.X * DSF - 12
            txtComment.Height = .Size.Y * DSF - 8
            txtComment.Text = .Text
            'move selection to beginning
            i = SendMessage(txtComment.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
            If i <> 0 Then
              Do
                'scroll up
                i = SendMessage(txtComment.hWnd, EM_SCROLL, SB_PAGEUP, 0)
                i = SendMessage(txtComment.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
              Loop Until i = 0
            End If
            
            txtComment.SelLength = 0
            txtComment.Visible = True
            txtComment.Tag = Selection.Number
            'send mouse up msg to release mouse from picDraw
            i = SendMessage(picDraw.hWnd, WM_LBUTTONUP, 0, 0)
            'send mouse down msg to text box
            i = SendMessage(txtComment.hWnd, WM_LBUTTONDOWN, 0, CLng(X - txtComment.Left) + CLng(Y - txtComment.Top) * &H10000)
            txtComment.SetFocus
          End With
          Exit Sub
        End If
              
        'mousedown on current selected object means
        'start moving it
        MoveObj = True
        'calculate offet between object position and cursor position
        '(for single objects, include the 8 pixel offset for handles)
        mDX = Selection.X1 + 8 - X
        mDY = Selection.Y1 + 8 - Y
        
        Set picDraw.MouseIcon = LoadResPicture("ELC_MOVING", vbResCursor)
        
        shpMove.Left = Selection.X1 + 8
        shpMove.Top = Selection.Y1 + 8
        'show outline based on selection Type
        Select Case Selection.Type
        Case lsRoom
          shpMove.Width = RM_SIZE * DSF
          shpMove.Height = RM_SIZE * DSF
          shpMove.Shape = vbShapeRectangle
          
        Case lsTransPt
          shpMove.Width = RM_SIZE / 2 * DSF
          shpMove.Height = RM_SIZE / 2 * DSF
          shpMove.Shape = vbShapeCircle
        Case lsErrPt
          shpMove.Width = 0.6 * DSF
          shpMove.Height = RM_SIZE / 2 * DSF
          shpMove.Shape = vbShapeRectangle
        Case lsComment
          shpMove.Width = Comment(Selection.Number).Size.X * DSF ' - 1
          shpMove.Height = Comment(Selection.Number).Size.Y * DSF ' - 1
          shpMove.Shape = vbShapeRoundedRectangle
          
        End Select
        'now show it
        shpMove.Visible = True
        Exit Sub
      Else
        'something OTHER than the current selection
        'is clicked; if CTRL key is down
        If Shift = vbCtrlMask Then
          'if adding to a singly selected object
          Select Case Selection.Type
          Case lsComment, lsErrPt, lsRoom, lsTransPt
            Select Case tmpSel.Type
            Case lsComment, lsErrPt, lsRoom, lsTransPt
              'reset selection collection (leaving room for two objects)
              ReDim SelectedObjects(1)
              
              'add current selection
              With SelectedObjects(0)
                .Type = Selection.Type
                'check for trans pt leg
                If Selection.Leg = 1 Then
                  'its a second leg trans pt
                  .Number = -Selection.Number
                Else
                  'its a first leg trans pt or another object Type
                  .Number = Selection.Number
                End If
              End With
              
              'finish changing selection
              Selection.Type = lsMultiple
              Selection.Number = 2
              Selection.ExitID = vbNullString

              'add the newly selected object to collection
              With SelectedObjects(1)
                .Type = tmpSel.Type
                'check for trans pt leg
                If tmpSel.Leg = 1 Then
                  'its a second leg trans pt
                  .Number = -tmpSel.Number
                Else
                  'its a first leg trans pt or another object Type
                  .Number = tmpSel.Number
                End If
              End With
              
              'force update by re-selecting
              SelectObj Selection
              SetEditMenu
              Exit Sub
            End Select
          End Select
        End If

        'something new is clicked, and it's not being
        'added to current selection, so if there is
        'something already selected, we need to
        'deselect it first
        If Selection.Type <> lsNone Then
          'deselect it
          DeselectObj
        End If
        
        'select whatever is under the cursor
        If tmpSel.Type = lsExit Then
          'select this exit
          SelectExit tmpSel
          If CustMousePtr <> ccSelObj Then
            Set picDraw.MouseIcon = LoadResPicture("ELC_SELOBJ", vbResCursor)
            CustMousePtr = ccSelObj
            picDraw.MousePointer = vbCustom
          End If
        ElseIf tmpSel.Type <> lsNone Then
          'select this object
          SelectObj tmpSel
          If CustMousePtr <> ccSelObj Then
            Set picDraw.MouseIcon = LoadResPicture("ELC_SELOBJ", vbResCursor)
            CustMousePtr = ccSelObj
            picDraw.MousePointer = vbCustom
          End If
        End If
      End If
      
    Case lsExit
      'current selection is an exit; let's check to see
      'if either end is clicked, meaning the exit is going
      'to be moved
      
      'check if either point is clicked by
      'testing cursor Value
      If picDraw.MousePointer = vbCrosshair Then
        'begin moving exit
        MoveExit = True
        
        'if on first point (from room end),
        If (X >= Selection.X1 And X <= Selection.X1 + 8 And Y >= Selection.Y1 And Y <= Selection.Y1 + 8) Then
          Selection.Point = 0
          linMove.X1 = Selection.X2 + 4
          linMove.Y1 = Selection.Y2 + 4
          linMove.X2 = Selection.X1 + 4
          linMove.Y2 = Selection.Y1 + 4
        Else
          Selection.Point = 1
          linMove.X1 = Selection.X1 + 4
          linMove.Y1 = Selection.Y1 + 4
          linMove.X2 = Selection.X2 + 4
          linMove.Y2 = Selection.Y2 + 4
        End If

        'now show it
        linMove.BorderColor = vbBlack
        linMove.BorderWidth = 1
        linMove.Visible = True
        
      ElseIf Not SameAsSelection(tmpSel) Then
        'the thing clicked is not the currently selected
        'exit
        '*'Debug.Assert Selection.Type <> lsNone
        '*'Debug.Assert Selection.Type = lsExit
        'deselect it
        DeselectObj
        
        'select whatever is under the cursor
        If tmpSel.Type = lsExit Then
          'select this exit
          SelectExit tmpSel
          Exit Sub
        ElseIf tmpSel.Type <> lsNone Then
          'select this object
          SelectObj tmpSel
          Exit Sub
        End If
      End If
      
    Case lsNone
      'select whatever was clicked
      Select Case tmpSel.Type
      Case lsRoom, lsTransPt, lsErrPt, lsComment
        SelectObj tmpSel
      Case lsExit
        SelectExit tmpSel
      End Select
    End Select
    
    'reset edit menu, since selection may have changed
    SetEditMenu
    Exit Sub
    
  'done with actions taken while Select tool is active
  'here we check what happens if exit drawing tool is
  'active
  Case ltEdge1, ltEdge2, ltOther
    'draw exits
    
    'if ok to draw, cursor is custom; check against no drop
    If picDraw.MousePointer = vbNoDrop Then
      'can't draw edge here
      Exit Sub
    End If
    
    'hide line (mouse move will show it again)
    linMove.Visible = False
    'set it back to normal
    linMove.BorderColor = vbBlack
    linMove.BorderWidth = 1
    
    'set anchor to edge (or center) of selected room
    ObjectFromPos tmpSel, X, Y
    
    If tmpSel.Type = lsRoom Then
      'save room number
      NewExitRoom = tmpSel.Number
      
      'allow drawing to any room
      DrawExit = 1
      
      'set anchor for exit line
      Select Case NewExitReason
      Case erHorizon
        AnchorX = Room(tmpSel.Number).Loc.X + RM_SIZE / 2
        AnchorY = Room(tmpSel.Number).Loc.Y
      Case erBottom
        AnchorX = Room(tmpSel.Number).Loc.X + RM_SIZE / 2
        AnchorY = Room(tmpSel.Number).Loc.Y + RM_SIZE
      Case erRight
        AnchorX = Room(tmpSel.Number).Loc.X + RM_SIZE
        AnchorY = Room(tmpSel.Number).Loc.Y + RM_SIZE / 2
      Case erLeft
        AnchorX = Room(tmpSel.Number).Loc.X
        AnchorY = Room(tmpSel.Number).Loc.Y + RM_SIZE / 2
      Case erOther
        AnchorX = Room(tmpSel.Number).Loc.X + RM_SIZE / 2
        AnchorY = Room(tmpSel.Number).Loc.Y + RM_SIZE / 2
      End Select
      
      linMove.Y1 = (AnchorY + OffsetY) * DSF
      linMove.X1 = (AnchorX + OffsetX) * DSF
    Else
      'save room and transfer number
      NewExitRoom = TransPt(tmpSel.Number).Room(1)
      NewExitTrans = tmpSel.Number
      
      'only allow drawing to matching room for this transfer
      DrawExit = 2
      
      'set anchor for exit line
      linMove.X1 = TransPt(tmpSel.Number).Loc(tmpSel.Leg).X
      linMove.Y1 = TransPt(tmpSel.Number).Loc(tmpSel.Leg).Y
      linMove.Y1 = (AnchorY + OffsetY) * DSF
      linMove.X1 = (AnchorX + OffsetX) * DSF
    End If
    Exit Sub
    
  'if the room tool is active, determine what to do
  'with the mouse click
  Case ltRoom
    'if cursor allows room here
    If picDraw.MousePointer = vbCrosshair Then
    
      'if at Max already
      If Logics.Count = 256 Then
        MsgBox "Maximum number of logics already exist in this game. Remove one or more existing logic, and then try again.", vbInformation + vbOKOnly, "Can't Add Room"
        Exit Sub
      End If
    
      'show wait cursor
      WaitCursor
      
      'add a new room here
      With frmGetResourceNum
        .WindowFunction = grAddLayout
        .ResType = rtLogic
        '
        .chkIncludePic.Value = AddPicToo
        'setup before loading so ghosts don't show up
        .FormSetup
        
        'reset cursor while user makes selection
        Screen.MousePointer = vbDefault

        'show the form
        .Show vbModal, frmMDIMain
        
        'show wait cursor again
        WaitCursor
        
        'if not canceled
        If Not .Canceled Then
          'temporary logic
          Set tmpLogic = New AGILogic
          tmpLogic.ID = .txtID.Text
          
          'save current checkbox Value
          AddPicToo = .chkIncludePic.Value
          
          'disable drawing until we can set position
          blnDontDraw = True
          
          'add a new logic (always use room template)
          AddNewLogic .NewResNum, tmpLogic, True, False
          
          'dereference tmplogic
          Set tmpLogic = Nothing
          'InRoom property is set by AddNewLogic (based on blnTemplate Value)
          Logics(.NewResNum).Save
          'no need to keep the logic loaded now
          Logics(.NewResNum).Unload
          
          'update editor to show this room is now in the game
          'get new exits from the logic that was passed
          Set tmpExits = ExtractExits(Logics(.NewResNum))
          
          'if adding picture too
          If .chkIncludePic.Value = vbChecked Then
            'if a picture already exists, delete it before adding a new one
            '*'Debug.Assert (.chkIncludePic.Caption = "Replace existing Picture") = Pictures.Exists(.NewResNum)
            If .chkIncludePic.Caption = "Replace existing Picture" Then
              RemovePicture .NewResNum
            End If
            AddNewPicture .NewResNum, Nothing
            'if loaded,
            If Pictures(.NewResNum).Loaded Then
              Pictures(.NewResNum).Unload
            End If
          End If
          
          'reposition to cursor
          With Room(.NewResNum)
            '*'Debug.Assert .Visible = True
            .Loc.X = GridPos(X / DSF - OffsetX)
            .Loc.Y = GridPos(Y / DSF - OffsetY)
            '*'Debug.Assert .Order = ObjCount - 1
          End With
          RepositionRoom .NewResNum
          
          'adjust Max and min
          AdjustMaxMin
          're-enable drawing
          blnDontDraw = False
          DrawLayout
        End If
      End With
      Unload frmGetResourceNum
      
      'reset cursor
      Screen.MousePointer = vbDefault
      CustMousePtr = ccNone
    End If
    
    'if not holding tool,
    If Not HoldTool Then
      'go back to select tool
      Toolbar1_ButtonClick Toolbar1.Buttons("select")
      Toolbar1.Buttons("select").Value = tbrPressed
      HoldTool = False
    End If
    Exit Sub
  
  'if the comment tool is active, process the
  'mouse click here
  Case ltComment
    'begin drawing comment
    AnchorX = X
    AnchorY = Y
    With shpMove
      .Left = AnchorX
      .Top = AnchorY
      .Width = RM_SIZE * DSF
      .Height = RM_SIZE * DSF
      .Shape = vbShapeRoundedRectangle
      .Visible = True
    End With
    
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub picDraw_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim i As Long
  Dim tmpSel As TSel, tmpID As String
  Dim tmpX As Single, tmpY As Single
  
  On Error GoTo ErrHandler
  
  'if position hasn't changed? exit? also no buttons?
  If mX = X And mY = Y And Button = 0 And Shift = 0 Then
    Exit Sub
  End If
  
  'save current position so it can be used by tip timer
  mX = X
  mY = Y
  
  
  'if not active form
  If Not (frmMDIMain.ActiveForm Is Me) Then
    Exit Sub
  End If
  
  'reset the tips timer
  tmrTip.Enabled = False
  tmrTip.Enabled = (Button = 0 And Shift = 0)
  picTip.Visible = False
  
  If MainStatusBar.Tag <> CStr(rtLayout) Then
    AdjustMenus rtLayout, True, True, IsDirty
  End If
  MainStatusBar.Panels("CurX").Text = "X: " & format$(X / DSF - OffsetX, "0.00")
  MainStatusBar.Panels("CurY").Text = "Y: " & format$(Y / DSF - OffsetY, "0.00")
  
  'if right button
  If Button = vbRightButton Then
    Exit Sub
  End If
  
  'if left click
  If Button = vbLeftButton Then
    'save x and y in case scrolling is needed
    OldX = X
    OldY = Y
  End If
  
  Select Case SelTool
  Case ltNone
    Exit Sub
  
  Case ltSelect
    Select Case Button
    Case 0  'no button down
      'if no ctrl key, and was previously, stop multiselect icon
      If ((Shift And vbCtrlMask) <> vbCtrlMask) And (CustMousePtr = ccAddObj) Then
        picDraw.MousePointer = vbDefault
        CustMousePtr = ccNone
      End If
    
      'if something already selected
      If Selection.Type <> lsNone Then
        'determine if cursor is over an object
        ObjectFromPos tmpSel, X, Y
      End If
      'depending on current selection
      'check for mouse over the selected object
      Select Case Selection.Type
      Case lsMultiple
         'if cursor is over one of the selected objects
        If IsSelected(tmpSel.Type, tmpSel.Number, tmpSel.Leg) Then
          If CustMousePtr <> ccMoveSel Then
            Set picDraw.MouseIcon = LoadResPicture("ELC_MOVESEL", vbResCursor)
            picDraw.MousePointer = vbCustom
            CustMousePtr = ccMoveSel
          End If
          Exit Sub
        End If
        'if cursor is within extent of selection frame, exit so exits inside won't trigger selection cursor
        If (X >= Selection.X1 And X <= Selection.X2 And Y >= Selection.Y1 And Y <= Selection.Y2) Then
          'reset to default cursor
          If CustMousePtr <> ccNone Then
            picDraw.MousePointer = vbDefault
            CustMousePtr = ccNone
          End If
          Exit Sub
        End If
        
      Case lsExit
        'if on either handle, show select cursor
        If (X >= Selection.X1 And X <= Selection.X1 + 8 And Y >= Selection.Y1 And Y <= Selection.Y1 + 8) Then
          'can't move 'from' point on error exits
          If Exits(Selection.Number)(Selection.ExitID).Transfer >= 0 Then
            picDraw.MousePointer = vbCrosshair
          Else
            picDraw.MousePointer = vbNoDrop
          End If
          CustMousePtr = ccNone
          Exit Sub
        End If
      
        If (X >= Selection.X2 And X <= Selection.X2 + 8 And Y >= Selection.Y2 And Y <= Selection.Y2 + 8) Then
          picDraw.MousePointer = vbCrosshair
          CustMousePtr = ccNone
          Exit Sub
        End If
      
      Case lsComment
        'if cursor is over a handle
        If (X >= Selection.X1 And X <= Selection.X1 + 8 And Y >= Selection.Y1 And Y <= Selection.Y1 + 8) Or (X >= Selection.X2 And X <= Selection.X2 + 8 And Y >= Selection.Y2 And Y <= Selection.Y2 + 8) Then
          'NW-SE
          picDraw.MousePointer = vbSizeNWSE
          CustMousePtr = ccNone
          Exit Sub
          
        ElseIf (X >= Selection.X2 And X <= Selection.X2 + 8 And Y >= Selection.Y1 And Y <= Selection.Y1 + 8) Or (X >= Selection.X1 And X <= Selection.X1 + 8 And Y >= Selection.Y2 And Y <= Selection.Y2 + 8) Then
          'NE-SW
          picDraw.MousePointer = vbSizeNESW
          CustMousePtr = ccNone
          Exit Sub
        
        'if cursor is over comment object
        ElseIf X >= Selection.X1 And X <= Selection.X2 And Y >= Selection.Y1 And Y <= Selection.Y2 Then
          If SameAsSelection(tmpSel) Then
            'if cursor is within text area
            If X >= Selection.X1 + 12 And X <= Selection.X2 - 6 And Y >= Selection.Y1 + 12 And Y <= Selection.Y2 - 6 Then
              picDraw.MousePointer = vbIbeam
              CustMousePtr = ccNone
            Else
              'show move cursor
              If CustMousePtr <> ccMoveSel Then
                Set picDraw.MouseIcon = LoadResPicture("ELC_MOVESEL", vbResCursor)
                picDraw.MousePointer = vbCustom
                CustMousePtr = ccMoveSel
              End If
            End If
          Else
            'white space around corners fall within selection boundaries
            'but should be ignored
            End If
          Exit Sub
        End If
        
      Case lsNone
        'nothing is selected; mouse can't be over selection
      
      Case Else
        'if cursor is within object selection area
        If X >= Selection.X1 And X <= Selection.X2 And Y >= Selection.Y1 And Y <= Selection.Y2 Then
          If SameAsSelection(tmpSel) Then
            If CustMousePtr <> ccMoveSel And CustMousePtr <> ccAddObj Then
              Set picDraw.MouseIcon = LoadResPicture("ELC_MOVESEL", vbResCursor)
              picDraw.MousePointer = vbCustom
              CustMousePtr = ccMoveSel
            End If
            Exit Sub
          End If
        End If
        
      End Select
      
      'if not over the selected object, OR nothing was selected AND not a multiple selection
      'change cursor if over something selectable (based on pixel color)
      If picDraw.Point(X, Y) <> picDraw.BackColor And picDraw.Point(X, Y) <> RGB(128, 128, 128) Then
        'if over anything while not selecting or adding objects
        'it doesn't matter what it is, so change cursor based on the color hit
        If CustMousePtr <> ccSelObj And CustMousePtr <> ccAddObj Then
          Set picDraw.MouseIcon = LoadResPicture("ELC_SELOBJ", vbResCursor)
          picDraw.MousePointer = vbCustom
          CustMousePtr = ccSelObj
        Else
          'if over an exit, always change cursor
          If OverExit(X, Y) Then
            Set picDraw.MouseIcon = LoadResPicture("ELC_SELOBJ", vbResCursor)
            picDraw.MousePointer = vbCustom
            CustMousePtr = ccSelObj
            Exit Sub
          End If
        End If
      Else
        'if not multiselect
        If CustMousePtr <> ccAddObj Then
          If CtrlDown Then
            'if ctrl key is down, go back to the multi cursor
            Set picDraw.MouseIcon = LoadResPicture("ELC_ADDOBJ", vbResCursor)
            picDraw.MousePointer = vbCustom
            CustMousePtr = ccAddObj
          Else
            'reset to default cursor
            picDraw.MousePointer = vbDefault
            CustMousePtr = ccNone
          End If
        End If
      End If
   
    Case vbLeftButton
      'if sizing a comment
      Select Case SizingComment
      Case 0
        'no sizing in progress
  
      Case 1
        'upper left
        If shpMove.Left + shpMove.Width - X > RM_SIZE * DSF Then
          shpMove.Width = shpMove.Left + shpMove.Width - (GridPos(X / DSF - OffsetX) + OffsetX) * DSF
          shpMove.Left = (GridPos(X / DSF - OffsetX) + OffsetX) * DSF
        Else
          shpMove.Left = (GridPos((shpMove.Left + shpMove.Width) / DSF - RM_SIZE - OffsetX) + OffsetX) * DSF
          shpMove.Width = RM_SIZE * DSF
        End If
        If shpMove.Top + shpMove.Height - Y > RM_SIZE * DSF Then
          shpMove.Height = shpMove.Top + shpMove.Height - (GridPos(Y / DSF - OffsetY) + OffsetY) * DSF
          shpMove.Top = (GridPos(Y / DSF - OffsetY) + OffsetY) * DSF
        Else
          shpMove.Top = (GridPos((shpMove.Top + shpMove.Height) / DSF - RM_SIZE - OffsetY) + OffsetY) * DSF
          shpMove.Height = RM_SIZE * DSF
        End If
        Exit Sub
      Case 2
        'upper right
        If X > shpMove.Left + RM_SIZE * DSF Then
          shpMove.Width = (GridPos(X / DSF - OffsetX) + OffsetX) * DSF - shpMove.Left - 1
        Else
          shpMove.Width = RM_SIZE * DSF
        End If
        If shpMove.Top + shpMove.Height - Y > RM_SIZE * DSF Then
          shpMove.Height = shpMove.Top + shpMove.Height - (GridPos(Y / DSF - OffsetY) + OffsetY) * DSF
          shpMove.Top = (GridPos(Y / DSF - OffsetY) + OffsetY) * DSF
        Else
          shpMove.Top = (GridPos((shpMove.Top + shpMove.Height) / DSF - RM_SIZE - OffsetY) + OffsetY) * DSF
          shpMove.Height = RM_SIZE * DSF
        End If
        Exit Sub
      Case 3
        'lower left
        If shpMove.Left + shpMove.Width - X > RM_SIZE * DSF Then
          shpMove.Width = shpMove.Left + shpMove.Width - (GridPos(X / DSF - OffsetX) + OffsetX) * DSF
          shpMove.Left = (GridPos(X / DSF - OffsetX) + OffsetX) * DSF
        Else
          shpMove.Left = (GridPos((shpMove.Left + shpMove.Width) / DSF - RM_SIZE - OffsetX) + OffsetX) * DSF
          shpMove.Width = RM_SIZE * DSF
        End If
        If Y > shpMove.Top + RM_SIZE * DSF Then
          shpMove.Height = (GridPos(Y / DSF - OffsetY) + OffsetY) * DSF - shpMove.Top - 1
        Else
          shpMove.Height = RM_SIZE * DSF
        End If
        Exit Sub
      Case 4
        'lower right
        If X > shpMove.Left + RM_SIZE * DSF Then
          shpMove.Width = (GridPos(X / DSF - OffsetX) + OffsetX) * DSF - shpMove.Left - 1
        Else
          shpMove.Width = RM_SIZE * DSF
        End If
        If Y > shpMove.Top + RM_SIZE * DSF Then
          shpMove.Height = (GridPos(Y / DSF - OffsetY) + OffsetY) * DSF - shpMove.Top - 1
        Else
          shpMove.Height = RM_SIZE * DSF
        End If
        Exit Sub
      End Select
   
      If MoveExit Then
        'if past edges of drawing surface, enable scrolling
        tmrScroll.Enabled = (X < -10 Or X > picDraw.Width + 10 Or Y < -10 Or Y > picDraw.Height + 10)
          
        'reposition outline line
        linMove.X2 = X
        linMove.Y2 = Y
        
        'determine if cursor is over a room/trans pt where exit can be dropped
        If picDraw.Point(X, Y) <> picDraw.BackColor Then
          ObjectFromPos tmpSel, X, Y
          Select Case tmpSel.Type
          Case lsRoom
            'cursor may need to change, so can't rely on current pointer to skip
            Select Case Exits(Selection.Number)(Selection.ExitID).Reason
            Case erHorizon
              If Selection.Point = 0 Then
                Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
                CustMousePtr = ccBottom
              Else
                Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
                CustMousePtr = ccHorizon
              End If
              
            Case erLeft
              If Selection.Point = 0 Then
                Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
                CustMousePtr = ccRight
              Else
                Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
                CustMousePtr = ccLeft
              End If
              
            Case erRight
              If Selection.Point = 0 Then
                Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
                CustMousePtr = ccLeft
              Else
                Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
                CustMousePtr = ccLeft
              End If
              
            Case erBottom
              If Selection.Point = 0 Then
                Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
                CustMousePtr = ccHorizon
              Else
                Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
                CustMousePtr = ccBottom
              End If
              
            Case Else
              With linMove
                'if mostly horizontal
                If Abs(.X2 - .X1) > Abs(.Y2 - .Y1) Then
                  If .X2 > .X1 Then
                    Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
                    CustMousePtr = ccRight
                  Else
                    Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
                    CustMousePtr = ccLeft
                  End If
                Else
                  If .Y2 > .Y1 Then
                    Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
                    CustMousePtr = ccBottom
                  Else
                    Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
                    CustMousePtr = ccHorizon
                  End If
                End If
              End With
            End Select
            picDraw.MousePointer = vbCustom
            
          Case lsTransPt
            'only allow reciprocal drops (exit associated with this trans pt is TO selroom)
            If TransPt(tmpSel.Number).Count = 1 And TransPt(tmpSel.Number).Room(1) = Selection.Number And Selection.Point = 1 Then
              tmpID = TransPt(tmpSel.Number).ExitID(0)
              
              Select Case Exits(Selection.Number)(Selection.ExitID).Reason
              Case erHorizon
                'if  exit reason is BOTTOM
                If Exits(TransPt(tmpSel.Number).Room(0))(tmpID).Reason = erBottom Then
                  If CustMousePtr <> ccHorizon Then
                    Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
                    picDraw.MousePointer = vbCustom
                    CustMousePtr = ccHorizon
                  End If
                Else
                  picDraw.MousePointer = vbNoDrop
                  CustMousePtr = ccNone
                End If
                
              Case erBottom
                'if exit reason is HORIZON
                If Exits(TransPt(tmpSel.Number).Room(0))(tmpID).Reason = erHorizon Then
                  If CustMousePtr <> ccBottom Then
                    Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
                    picDraw.MousePointer = vbCustom
                    CustMousePtr = ccBottom
                  End If
                Else
                  picDraw.MousePointer = vbNoDrop
                  CustMousePtr = ccNone
                End If
              
              Case erRight
                'if exit reason is LEFT
                If Exits(TransPt(tmpSel.Number).Room(0))(tmpID).Reason = erLeft Then
                  If CustMousePtr <> ccRight Then
                    Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
                    picDraw.MousePointer = vbCustom
                    CustMousePtr = ccRight
                  End If
                Else
                  picDraw.MousePointer = vbNoDrop
                  CustMousePtr = ccNone
                End If
              
              Case erLeft
                'if exit reason is RIGHT
                If Exits(TransPt(tmpSel.Number).Room(0))(tmpID).Reason = erRight Then
                  If CustMousePtr = ccLeft Then
                    Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
                    picDraw.MousePointer = vbCustom
                    CustMousePtr = ccLeft
                  End If
                Else
                  picDraw.MousePointer = vbNoDrop
                  CustMousePtr = ccNone
                End If
              
              Case Else
                If Exits(TransPt(tmpSel.Number).Room(0))(tmpID).Reason = erOther Then
                  'if mostly horizontal
                  '(cursor may need to change, so can't rely on current pointer to skip
                  If Abs(linMove.X2 - linMove.X1) > Abs(linMove.Y2 - linMove.Y1) Then
                    If linMove.X2 > linMove.X1 Then
                      Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
                      CustMousePtr = ccRight
                    Else
                      Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
                      CustMousePtr = ccLeft
                    End If
                  Else
                    If linMove.Y2 > linMove.Y1 Then
                      Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
                      CustMousePtr = ccBottom
                    Else
                      Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
                      CustMousePtr = ccHorizon
                    End If
                  End If
                  picDraw.MousePointer = vbCustom
                Else
                  picDraw.MousePointer = vbNoDrop
                  CustMousePtr = ccNone
                End If
              End Select
            Else
              'can't drop
              picDraw.MousePointer = vbNoDrop
              CustMousePtr = ccNone
            End If
          End Select
        Else
          'show crosshair
          picDraw.MousePointer = vbCrosshair
          CustMousePtr = ccNone
        End If
        Exit Sub
      End If
      
      If MoveObj Then
        'if past edges of drawing surface, enable scrolling
        tmrScroll.Enabled = (X < -10 Or (X > picDraw.Width + 10) Or Y < -10 Or (Y > picDraw.Height + 10))
          
        'reposition outline shape
'        shpMove.Left = (GridPos((X + mDX - 8) / DSF - OffsetX) + OffsetX) * DSF
'        shpMove.Top = (GridPos((Y + mDY - 8) / DSF - OffsetY) + OffsetY) * DSF
        'convert surface coordinates into world coordinates,
        'apply grid setting, then re-convert back into surface coordinates
        shpMove.Left = (GridPos((X + mDX) / DSF - OffsetX) + OffsetX) * DSF
        shpMove.Top = (GridPos((Y + mDY) / DSF - OffsetY) + OffsetY) * DSF
        Exit Sub
      End If
      
      'when an object is clicked, the move flag is not normally set
      'until it is clicked again; in some cases the user wants to
      'click and drag immediately
      'check if over selectedobject to see if this is the case
      'if there is an object currently selected
      Select Case Selection.Type
      Case lsExit
        'if on either handle, show select cursor
        If (X >= Selection.X1 And X <= Selection.X1 + 8 And Y >= Selection.Y1 And Y <= Selection.Y1 + 8) Or (X >= Selection.X2 And X <= Selection.X2 + 8 And Y >= Selection.Y2 And Y <= Selection.Y2 + 8) Then
          picDraw.MousePointer = vbCrosshair
          CustMousePtr = ccNone
          picDraw_MouseDown Button, Shift, X, Y
          Exit Sub
        End If
      Case lsRoom, lsTransPt, lsErrPt, lsComment
        'if cursor is within object selection area
        If X >= Selection.X1 And X <= Selection.X2 And Y >= Selection.Y1 And Y <= Selection.Y2 Then
          'force another click
          picDraw_MouseDown Button, Shift, X, Y
          Exit Sub
        End If
      End Select
      
      'if drag-selecting
      If DragSelect Then
        'if past edges of drawing surface, enable scrolling
        tmrScroll.Enabled = (X < -10 Or X > picDraw.Width + 10 Or Y < -10 Or Y > picDraw.Height + 10)
          
        If Abs(X - AnchorX) >= 3 And Abs(Y - AnchorY) >= 3 Then
          shpMove.Visible = True
          'if anything selected
          If Selection.Type <> lsNone Then
            DeselectObj
          End If
          
          'position selection shape
          If X < AnchorX Then
            shpMove.Left = X
            shpMove.Width = AnchorX - X
          Else
            shpMove.Left = AnchorX
            shpMove.Width = X - AnchorX
          End If
          If Y < AnchorY Then
            shpMove.Top = Y
            shpMove.Height = AnchorY - Y
          Else
            shpMove.Top = AnchorY
            shpMove.Height = Y - AnchorY
          End If
        
        Else
          shpMove.Visible = False
        End If
      End If
      
      'if dragging the canvas, adjust the view
      If DragCanvas Then
        'calculate new offsets
        OffsetX = OffsetX + (X - AnchorX) / DSF
        OffsetY = OffsetY + (Y - AnchorY) / DSF
        AnchorX = X
        AnchorY = Y
        
        'set scroll values to match offsets
        CodeChange = True
        If HScroll1.Min > -100 * OffsetX Then
          HScroll1.Min = -100 * OffsetX
        End If
        If HScroll1.Max < -100 * OffsetX Then
          HScroll1.Max = -100 * OffsetX
        End If
        HScroll1.Value = -100 * OffsetX
        If VScroll1.Min > -100 * OffsetY Then
          VScroll1.Min = -100 * OffsetY
        End If
        If VScroll1.Max < -100 * OffsetY Then
          VScroll1.Max = -100 * OffsetY
        End If
        VScroll1.Value = -100 * OffsetY
        CodeChange = False
        
        'redraw the canvas
        DrawLayout
        Exit Sub
      End If
      
    End Select
    
  
  Case ltEdge1, ltEdge2, ltOther
    Select Case Button
    Case 0  'no mouse
      'if not drawing an exit
      If DrawExit = 0 Then
        'if over an object
        ObjectFromPos tmpSel, X, Y
        
        'if over a room
        If tmpSel.Type = lsRoom Then
          'see if on or near an edge, adjust pointer and display
          'suggested edge
          If SelTool = ltOther Then
            'if drawing a 'other' exit, it's always the same dir value
            i = erOther
            Set picDraw.MouseIcon = LoadResPicture("ELC_OTHER", vbResCursor)
            CustMousePtr = ccOther
          Else
            'calculate the direction value by determiming which edge
            'the cursor is closest to
            tmpX = (Room(tmpSel.Number).Loc.X + RM_SIZE / 2 + OffsetX) * DSF
            tmpY = (Room(tmpSel.Number).Loc.Y + RM_SIZE / 2 + OffsetY) * DSF
            If X >= tmpX And X - tmpX >= Abs(Y - tmpY) Then
              Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
              i = erRight
              CustMousePtr = ccRight
            ElseIf X < tmpX And tmpX - X > Abs(Y - tmpY) Then
              Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
              i = erLeft
              CustMousePtr = ccLeft
            ElseIf Y >= tmpY And Y - tmpY >= Abs(X - tmpX) Then
              Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
              i = erBottom
              CustMousePtr = ccBottom
            Else
              Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
              i = erHorizon
              CustMousePtr = ccHorizon
            End If
          End If
          
          'regardless of direction, always set the custom cursor
          picDraw.MousePointer = vbCustom
          
          'if changing, then re-highlight
          If i <> NewExitReason Then
            HighlightExitStart tmpSel.Number, i
          End If
          
        Else
          'hide the edge marker and reset the suggested exit
          linMove.Visible = False
          NewExitReason = erNone
          
          'if drawing a single exit, and over a transpt that
          'could support another exit going in this direction
          'then we need to show the correct mouse pointer
          If tmpSel.Type = lsTransPt And SelTool = ltEdge1 Then
            'if this exit is not yet two way
            If TransPt(tmpSel.Number).Count = 1 Then
              'display corresponding direction (only valid for edge exits;
              'never other or unknown exits)
              tmpID = TransPt(tmpSel.Number).ExitID(0)
              'swap direction because going from transpt to room
              Select Case Exits(TransPt(tmpSel.Number).Room(0))(tmpID).Reason
              Case erHorizon
                Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
                picDraw.MousePointer = vbCustom
                NewExitReason = erBottom
                CustMousePtr = ccBottom
              Case erBottom
                Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
                picDraw.MousePointer = vbCustom
                NewExitReason = erHorizon
                CustMousePtr = ccHorizon
              Case erRight
                Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
                picDraw.MousePointer = vbCustom
                NewExitReason = erLeft
                CustMousePtr = ccLeft
              Case erLeft
                Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
                picDraw.MousePointer = vbCustom
                NewExitReason = erRight
                CustMousePtr = ccRight
              Case Else
                picDraw.MousePointer = vbNoDrop
                CustMousePtr = ccNone
              End Select
            Else
              picDraw.MousePointer = vbNoDrop
              CustMousePtr = ccNone
            End If
          Else
            picDraw.MousePointer = vbNoDrop
            CustMousePtr = ccNone
          End If
        End If
        Exit Sub
        
      'end if drawexit=0
      End If
      
    Case vbLeftButton
      'if drawing an exit
      If DrawExit <> 0 Then
        'if past edges of drawing surface, enable scrolling
        tmrScroll.Enabled = (X < -10 Or X > picDraw.Width + 10 Or Y < -10 Or Y > picDraw.Height + 10)
          
        'draw line
        linMove.X2 = X
        linMove.Y2 = Y
        linMove.Visible = True
        
        'get object under cursor
         ObjectFromPos tmpSel, X, Y
        
        'if a room
        If tmpSel.Type = lsRoom Then
          'if drawing from a transpt
          If DrawExit = 2 Then
            'if room doesn't match transpt room
            If TransPt(NewExitTrans).Room(0) <> tmpSel.Number Then
              picDraw.MousePointer = vbNoDrop
              CustMousePtr = ccNone
              Exit Sub
            End If
          End If
          
          Select Case NewExitReason
          Case erHorizon
            Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
            CustMousePtr = ccHorizon
          Case erBottom
            Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
            CustMousePtr = ccBottom
          Case erRight
            Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
            CustMousePtr = ccRight
          Case erLeft
            Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
            CustMousePtr = ccLeft
          Case erOther
            'depends on direction of line
            With linMove
              'if mostly horizontal
              If Abs(.X2 - .X1) > Abs(.Y2 - .Y1) Then
                If .X2 > .X1 Then
                  Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
                  CustMousePtr = ccRight
                Else
                  Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
                  CustMousePtr = ccLeft
                End If
              Else
                If .Y2 > .Y1 Then
                  Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
                  CustMousePtr = ccBottom
                Else
                  Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
                  CustMousePtr = ccHorizon
                End If
              End If
            End With
          End Select
          picDraw.MousePointer = vbCustom
        ElseIf tmpSel.Type = lsTransPt And SelTool = ltEdge1 Then
          'if this exit is not yet two way
          If TransPt(tmpSel.Number).Count = 1 Then
            'if this transpt to room matches fromroom
            If TransPt(tmpSel.Number).Room(1) = NewExitRoom Then
              'display corresponding direction (only valid for edge exiits;
              'never other or unknown exits)
              tmpID = TransPt(tmpSel.Number).ExitID(0)
              'swap direction because going from transpt to room
              Select Case Exits(TransPt(tmpSel.Number).Room(0))(tmpID).Reason
              Case erHorizon
                Set picDraw.MouseIcon = LoadResPicture("ELC_BOTTOM", vbResCursor)
                picDraw.MousePointer = vbCustom
                NewExitReason = erBottom
                CustMousePtr = ccBottom
                Exit Sub
              Case erBottom
                Set picDraw.MouseIcon = LoadResPicture("ELC_HORIZON", vbResCursor)
                picDraw.MousePointer = vbCustom
                NewExitReason = erHorizon
                CustMousePtr = ccHorizon
                Exit Sub
              Case erRight
                Set picDraw.MouseIcon = LoadResPicture("ELC_LEFT", vbResCursor)
                picDraw.MousePointer = vbCustom
                NewExitReason = erLeft
                CustMousePtr = ccLeft
              Case erLeft
                Set picDraw.MouseIcon = LoadResPicture("ELC_RIGHT", vbResCursor)
                picDraw.MousePointer = vbCustom
                NewExitReason = erRight
                CustMousePtr = ccRight
              End Select
              Exit Sub
            End If
          End If
          'cant draw
          picDraw.MousePointer = vbNoDrop
          CustMousePtr = ccNone
        Else
          'cant draw
          picDraw.MousePointer = vbNoDrop
          CustMousePtr = ccNone
        End If
        Exit Sub
      End If
    End Select
  
  Case ltRoom
    'if not on an object or exit
    If picDraw.Point(X, Y) = picDraw.BackColor Then
      picDraw.MousePointer = vbCrosshair
    Else
      picDraw.MousePointer = vbNoDrop
    End If
    CustMousePtr = ccNone
    
  Case ltComment
    'if drawing a comment
    If shpMove.Visible Then
      'reposition anchor if necessary
      
      If X > AnchorX + RM_SIZE * DSF Then
        shpMove.Width = X - AnchorX
      Else
        shpMove.Width = RM_SIZE * DSF
      End If
      If Y > AnchorY + RM_SIZE * DSF Then
        shpMove.Height = Y - AnchorY
      Else
        shpMove.Height = RM_SIZE * DSF
      End If
    End If
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub picDraw_MouseUp(Button As Integer, Shift As Integer, X As Single, Y As Single)

  Dim i As Long, j As Long
  Dim tmpSel As TSel
  
  On Error GoTo ErrHandler
  
  'always force scroll timer off
  tmrScroll.Enabled = False
  'and make sure tip is hidden
  picTip.Visible = False
  'and allow drawing
  blnDontDraw = False
  
  Select Case SelTool
  Case ltSelect
    If MoveExit Then
      'reset moving flag
      MoveExit = False
      linMove.Visible = False
      
      'validate new exit pos
      'by checking cursor (a custom cursor means drop is OK)
      If picDraw.MousePointer = vbCustom Then
        'get object where exit is being dropped
        ObjectFromPos tmpSel, X, Y
        
        'if not changing both exits of a two-way exit
        If Selection.TwoWay <> ltwBothWays Then
          'if changing to room
          If Selection.Point = 1 Then
            'if target room has changed
            If tmpSel.Number <> Exits(Selection.Number)(Selection.ExitID).Room Then
              'change the to-room for this exit
              ChangeToRoom Selection, tmpSel.Number, tmpSel.Type
              
              'redraw
              DrawLayout True
              MarkAsDirty
            End If
            
          Else
            'if from room has changed
            If tmpSel.Number <> Selection.Number Then
              'change the from room; keep type the same
              ChangeFromRoom Selection, tmpSel.Number

            Else
              'nothing to do; just exit
        
              'reset cursor
              picDraw.MousePointer = vbCrosshair
              CustMousePtr = ccNone
              Exit Sub
            End If
            
            'redraw
            DrawLayout True
            MarkAsDirty
          End If
        Else
          'two-way; change both from and to room
          
          'use i for target room
          i = tmpSel.Number
          
          'get reciprocal exit
          tmpSel = Selection
          IsTwoWay tmpSel.Number, tmpSel.ExitID, tmpSel.Number, tmpSel.ExitID, 1
          
          'determine which is from room and which is to room
          If Selection.Point = 1 Then
            'tmpSel contains to room
          
            'if target room has changed
            If i <> Exits(Selection.Number)(Selection.ExitID).Room Then
          
              'change the to-room for this exit (always target a room)
              'MAKE SURE to do this BEFORE changing 'from' room;
              'when changing from room, if it finds a reciprocal,
              'it will delete it, so then the change 'to' room function
              'no longer has an exit to move
              ChangeToRoom Selection, i, lsRoom

              'change from room, using tmpSel as old selection
              ChangeFromRoom tmpSel, i
            Else
              'target hasn't changed; don't do anything
        
              'reset cursor
              picDraw.MousePointer = vbCrosshair
              CustMousePtr = ccNone
              Exit Sub
            End If
                         
          Else
            'tmpsel contains from room
            
            'if from room has changed
            If i <> Selection.Number Then
               'change to room
              'MAKE SURE to do this BEFORE changing 'from' room;
              'when changing from room, if it finds a reciprocal,
              'it will delete it, so then the change 'to' room function
              'no longer has an exit to move
              ChangeToRoom tmpSel, i, lsRoom
            
              'change from room, using Selection
              ChangeFromRoom Selection, i
            Else
              'from room hasn't changed; don't do anything
        
              'reset cursor
              picDraw.MousePointer = vbCrosshair
              CustMousePtr = ccNone
              Exit Sub
            End If
          End If
          
          'copy tempsel back into selection
          'Selection = tmpSel
          
          'force selection of both exits
          'If Selection.TwoWay =ltwOneWay Then
            Selection.TwoWay = ltwBothWays
          'End If
          
          'redraw
          DrawLayout True
          MarkAsDirty
        End If
      End If
      
      'reset cursor
      picDraw.MousePointer = vbCrosshair
      CustMousePtr = ccNone
    End If
    
    If MoveObj Then
      'drop the selected objects at this location
      DropObjs X + mDX, Y + mDY
      Exit Sub
    End If
    
    If SizingComment <> 0 Then
      With Comment(Selection.Number)
        .Loc.X = shpMove.Left / DSF - OffsetX
        
        .Loc.Y = shpMove.Top / DSF - OffsetY
        .Size.X = GridPos((shpMove.Width + 1) / DSF)
        .Size.Y = GridPos((shpMove.Height + 1) / DSF)
      End With
      
      shpMove.Visible = False
      SizingComment = 0
      MarkAsDirty
      AdjustMaxMin
      DrawLayout
      Exit Sub
    End If
    
    'if drag-selecting
    If DragSelect Then
      DragSelect = False
      
      'deselect anything previously selected
      DeselectObj
      
      'if selection shape is visible
      If shpMove.Visible Then
        'hide it
        shpMove.Visible = False
        'get selected objects
        GetSelectedObjects
      End If
      SetEditMenu
    End If
    
    If DragCanvas Then
      'stop the drag
      DragCanvas = False
      'reset to default cursor
      picDraw.MousePointer = vbDefault
      CustMousePtr = ccNone
    End If
    
  Case ltEdge1, ltEdge2, ltOther
    'if target room is same as starting room
    If tmpSel.Number = NewExitRoom Then
      'unless line is at least .4 units (half the room width/height), assume user doesnt want an exit
      With linMove
        If Sqr((.X2 - .X1) ^ 2 + (.Y2 - .Y1) ^ 2) / DSF < RM_SIZE / 2 Then
          Exit Sub
        End If
      End With
    End If
    
    'reset drawexit flag
    DrawExit = 0
    
    'hide line
    linMove.Visible = False
    
    'if not a valid drop zone
    If picDraw.MousePointer = vbNoDrop Then
      'cancel exit drawing
      Exit Sub
    End If
    
    'get target room number
    ObjectFromPos tmpSel, X, Y
    
    'create new exit
    
    'if dropping on a room
    If tmpSel.Type = lsRoom Then
            
      CreateNewExit NewExitRoom, tmpSel.Number, NewExitReason
      'if drawing exits both ways
      If SelTool = ltEdge2 Then
        'add reciprocal
        CreateNewExit tmpSel.Number, NewExitRoom, ((NewExitReason + 1) And 3) + 1
      End If
    Else
      'what if transfer already has two exits?  hmmm, have to test
      '*'Debug.Assert TransPt(tmpSel.Number).Count = 1
      
      'dropping on a transfer pt
      CreateNewExit NewExitRoom, TransPt(tmpSel.Number).Room(0), NewExitReason
    End If
        
    'redraw
    DrawLayout
    
    'if not holding tool,
    If Not HoldTool Then
      'go back to select tool
      Toolbar1_ButtonClick Toolbar1.Buttons("select")
      Toolbar1.Buttons("select").Value = tbrPressed
      HoldTool = False
      picDraw.MousePointer = vbDefault
    End If
    
  Case ltRoom
    'all actions take place in mousedown
    
  Case ltComment
    shpMove.Visible = False
    
    'add a comment here
    For i = 1 To 255
      If Not Comment(i).Visible Then
        Exit For
      End If
    Next i
    If i = 256 Then
      'too many
      Exit Sub
    End If
    
    With Comment(i)
      .Visible = True
      .Loc.X = GridPos(AnchorX / DSF - OffsetX)
      .Loc.Y = GridPos(AnchorY / DSF - OffsetY)
      .Size.X = GridPos(shpMove.Width / DSF)
      .Size.Y = GridPos(shpMove.Height / DSF)
      .Order = ObjCount
      ObjOrder(ObjCount).Number = i
      ObjOrder(ObjCount).Type = lsComment
      ObjCount = ObjCount + 1
      
      'adjust Max and min
      AdjustMaxMin
      
      'draw the comment box
      DrawCmtBox i
      
      'begin editing text in comment box
      txtComment.Text = vbNullString
      txtComment.Left = (.Loc.X + OffsetX) * DSF + 6
      txtComment.Top = (.Loc.Y + OffsetY) * DSF + 4
      txtComment.Width = .Size.X * DSF - 12
      txtComment.Height = .Size.Y * DSF - 8
      txtComment.Visible = True
      txtComment.SetFocus
      
      'use tag property to id new comment
      txtComment.Tag = i
    End With
    
    'if not holding tool,
    If Not HoldTool Then
      'go back to select tool
      Toolbar1_ButtonClick Toolbar1.Buttons("select")
      Toolbar1.Buttons("select").Value = tbrPressed
      HoldTool = False
    End If
    
  Case ltNone
  End Select
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub picDraw_Resize()

  On Error GoTo ErrHandler
  
  'redraw
  DrawLayout
  
  'when adjusting scrollbars, if value is changed
  'it will trigger the 'Change' event; we need
  'to ignore those so we use a flag
  CodeChange = True
  
  'size scrollbars
  SetScrollBars
  CodeChange = False
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub


Private Sub tmrScroll_Timer()

  'scrolling is in progress
  
  Dim sngShift As Single
  Dim lngPrevH As Long, lngPrevV As Long
  
  'shift amount is 5% of screen (1/4 of smallchange)
  
  'save current scroll values
  lngPrevH = HScroll1.Value
  lngPrevV = VScroll1.Value
  
  'disable drawing until scrolling is done
  blnDontDraw = True
  
  'determine direction by checking OldX and OldY
  If OldX < -10 Then
    'if scrolling left will go under minimum
    If HScroll1.Value - HScroll1.SmallChange / 4 < HScroll1.Min Then
      'reset minimum so we can scroll
      HScroll1.Min = HScroll1.Value - HScroll1.SmallChange / 4
      HScroll1.Value = HScroll1.Min
    Else
      'scroll left
      HScroll1.Value = HScroll1.Value - HScroll1.SmallChange / 4
    End If
    
  ElseIf OldX > picDraw.Width + 10 Then
    'if scrolling right will go over maximum
    If HScroll1.Value + HScroll1.SmallChange / 4 > HScroll1.Max Then
      'reset maximum so we can scroll
      HScroll1.Max = HScroll1.Value + HScroll1.SmallChange / 4
      HScroll1.Value = HScroll1.Max
    Else
      'scroll right
      HScroll1.Value = HScroll1.Value + HScroll1.SmallChange / 4
    End If
  
  End If
  
  If OldY < -10 Then
    'if scrolling up will go under minimum
    If VScroll1.Value - VScroll1.SmallChange / 4 < VScroll1.Min Then
      'reset minimum so we can scroll
      VScroll1.Min = VScroll1.Value - VScroll1.SmallChange / 4
      VScroll1.Value = VScroll1.Min
    Else
      'scroll up
      VScroll1.Value = VScroll1.Value - VScroll1.SmallChange / 4
    End If
    
  ElseIf OldY > picDraw.Height + 10 Then
    'if scrolling down will go above maximum
    If VScroll1.Value + VScroll1.SmallChange / 4 > VScroll1.Max Then
      'reset maximum so we can scroll
      VScroll1.Max = VScroll1.Value + VScroll1.SmallChange / 4
      VScroll1.Value = VScroll1.Max
    Else
      'scroll down
      VScroll1.Value = VScroll1.Value + VScroll1.SmallChange / 4
    End If
  End If
  
  'if drawing or moving an exit,
  If (DrawExit <> 0) Or MoveExit Then
    linMove.Y1 = (AnchorY + OffsetY) * DSF
    linMove.X1 = (AnchorX + OffsetX) * DSF
  End If
  
  'if moving a single object
  If MoveObj And Selection.Type <> lsMultiple Then
    'adjust anchors
    AnchorX = AnchorX - (HScroll1.Value - lngPrevH)
    AnchorY = AnchorY - (VScroll1.Value - lngPrevV)
  End If
  
  'if dragging a selection
  If DragSelect Then
    'adjust anchors
    AnchorX = AnchorX - (HScroll1.Value - lngPrevH)
    AnchorY = AnchorY - (VScroll1.Value - lngPrevV)
    'resize selection shape
    If OldX < AnchorX Then
      shpMove.Left = OldX
      shpMove.Width = AnchorX - OldX
    Else
      shpMove.Left = AnchorX
      shpMove.Width = OldX - AnchorX
    End If
    If OldY < AnchorY Then
      shpMove.Top = OldY
      shpMove.Height = AnchorY - OldY
    Else
      shpMove.Top = AnchorY
      shpMove.Height = OldY - AnchorY
    End If
  End If
  
  'enable drawing
  blnDontDraw = False
  'and redraw
  DrawLayout
End Sub

Private Sub tmrTip_Timer()

  'pointer not moving; if over a room with a long name, show the full name
  
  Dim tmpSel As TSel
  Dim strID As String, sngTextW As Single
  
  On Error GoTo ErrHandler
  
  'always turn off timer so we don't recurse
  '****doesn't work on this form- mousemove fires every time
  'this timer goes off even though mouse isn't moving
  'only way to fix it was to add code to mousemove that exits
  'if mousemove occurs without an actual change in position
  tmrTip.Enabled = False
  '*'Debug.Assert Logics.Exists(tmpSel.Number)
  'is the cursor over a room?
  ObjectFromPos tmpSel, mX, mY
  If tmpSel.Type = lsRoom Then
    'is the room text too long?
    strID = ResourceName(Logics(tmpSel.Number), True, True)
    'display this room id as a tip
    With picTip
      .Cls
      .Width = .TextWidth(strID & "  ")
      picTip.Print " "; strID
      .Top = mY - .Height
      .Left = mX
      .Visible = True
    End With
  End If
    
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
  
End Sub

Private Sub Toolbar1_ButtonClick(ByVal Button As MSComctlLib.Button)
  
  Dim i As Long, j As Long
  Dim tmpTrans As Long
  Dim tmpCoord As LCoord
  
  On Error GoTo ErrHandler
  
  If MainStatusBar.Tag <> CStr(rtLayout) Then
    AdjustMenus rtLayout, True, True, IsDirty
  End If
  
  Select Case Button.Key
  Case "select"
    'change tool to select
    SelTool = ltSelect
    MainStatusBar.Panels("Tool").Text = "Tool: Select"
    'ensure any lines are hidden
    linMove.Visible = False
    
    'disable toolbar buttons
    With Toolbar1.Buttons
      .Item("delete").Enabled = False
      .Item("transfer").Enabled = False
      .Item("hide").Enabled = False
      .Item("front").Enabled = False
      .Item("back").Enabled = False
    End With
    
  Case "edge1"
    'clear any selection
    DeselectObj
    SetEditMenu
    
    'setup for drawing edges
    SelTool = ltEdge1
    MainStatusBar.Panels("Tool").Text = "Tool: One Way Edge"
    
  Case "edge2"
    'clear any selection
    DeselectObj
    SetEditMenu
    
    'setup for drawing edges
    SelTool = ltEdge2
    MainStatusBar.Panels("Tool").Text = "Tool: Two Way Edge"
    
  Case "other"
    'clear any selection
    DeselectObj
    SetEditMenu
    
    'set up for drawing edges
    SelTool = ltOther
    MainStatusBar.Panels("Tool").Text = "Tool: Other Exit"
    
  Case "room"
    'clear any selection
    DeselectObj
    SetEditMenu
    
    SelTool = ltRoom
    picDraw.MousePointer = vbCrosshair
    CustMousePtr = ccNone
    MainStatusBar.Panels("Tool").Text = "Tool: Add Room"
    
  Case "comment"
    'clear any selection
    DeselectObj
    SetEditMenu
    
    SelTool = ltComment
    picDraw.MousePointer = vbCrosshair
    CustMousePtr = ccNone
    MainStatusBar.Panels("Tool").Text = "Tool: Add Comment"
    
  Case "delete"
    MenuClickDelete
    
  Case "transfer"
    MenuClickInsert
    
  Case "show"
    ShowRoom
    
  Case "hide"
    MenuClickCut
  
  Case "front"
    'if selected object is not already at bottom of order (meaning in last position, i.e., drawn last)
    If ObjOrder(ObjCount - 1).Number <> Selection.Number Or ObjOrder(ObjCount - 1).Type <> Selection.Type Then
      'get current order of selection based on its Type
      Select Case Selection.Type
      Case lsRoom
        j = Room(Selection.Number).Order
      Case lsTransPt
        j = TransPt(Selection.Number).Order
      Case lsErrPt
        j = ErrPt(Selection.Number).Order
      Case lsComment
        j = Comment(Selection.Number).Order
      End Select
      
      'move all objects toward bottom (increasing their position)
      For i = j To ObjCount - 2
        'change the object at position above (i + 1)
        'to have a new position at i
        Select Case ObjOrder(i + 1).Type
        Case lsRoom
          Room(ObjOrder(i + 1).Number).Order = i
        Case lsTransPt
          TransPt(ObjOrder(i + 1).Number).Order = i
        Case lsErrPt
          ErrPt(ObjOrder(i + 1).Number).Order = i
        Case lsComment
          Comment(ObjOrder(i + 1).Number).Order = i
        End Select
        'copy the object order information at position
        'above (i + 1) down to i
        ObjOrder(i) = ObjOrder(i + 1)
      Next i
      'put selected object in last position
      ObjOrder(ObjCount - 1).Number = Selection.Number
      ObjOrder(ObjCount - 1).Type = Selection.Type
      'depending on object Type, set its order
      'Value to last position
      Select Case Selection.Type
      Case lsRoom
        Room(Selection.Number).Order = ObjCount - 1
      Case lsTransPt
        TransPt(Selection.Number).Order = ObjCount - 1
      Case lsErrPt
        ErrPt(Selection.Number).Order = ObjCount - 1
      Case lsComment
        Comment(Selection.Number).Order = ObjCount - 1
      End Select
     
      DrawLayout
    End If
    
  Case "back"
    'if selected object is not already at top of order
    If ObjOrder(0).Number <> Selection.Number Or ObjOrder(0).Type <> Selection.Type Then
      'get current order of selection based on its Type
      Select Case Selection.Type
      Case lsRoom
        j = Room(Selection.Number).Order
      Case lsTransPt
        j = TransPt(Selection.Number).Order
      Case lsErrPt
        j = ErrPt(Selection.Number).Order
      Case lsComment
        j = Comment(Selection.Number).Order
      End Select
      
      For i = j To 1 Step -1
        'change the object at the position below (i-1)
        'to have a new position at i
        Select Case ObjOrder(i - 1).Type
        Case lsRoom
          Room(ObjOrder(i - 1).Number).Order = i
        Case lsTransPt
          TransPt(ObjOrder(i - 1).Number).Order = i
        Case lsErrPt
          ErrPt(ObjOrder(i - 1).Number).Order = i
        Case lsComment
          Comment(ObjOrder(i - 1).Number).Order = i
        End Select
        'copy the object order information at position
        'below (i-1) up to i
        ObjOrder(i) = ObjOrder(i - 1)
      Next i
      
      'put selected object in first position
      ObjOrder(0).Number = Selection.Number
      ObjOrder(0).Type = Selection.Type
      'depending on object Type, set its order
      'Value to first position
      Select Case Selection.Type
      Case lsRoom
        Room(Selection.Number).Order = 0
      Case lsTransPt
        TransPt(Selection.Number).Order = 0
      Case lsErrPt
        ErrPt(Selection.Number).Order = 0
      Case lsComment
        Comment(Selection.Number).Order = 0
      End Select
      
      DrawLayout
    End If
      
  Case "zoomin"
    ChangeScale 1
    
  Case "zoomout"
    ChangeScale -1
    
  End Select
  
  'always reset hold tool flag
  HoldTool = False
  
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub Toolbar1_DblClick()

  'double-click doesn't trigger for buttons; only  non-button areas of the toolbar
  'incredibly lame and totally useless...
  
'''  Dim tmpButton As Button, i As Long
'''
'''  For i = 1 To Toolbar1.Buttons.Count
'''    Set tmpButton = Toolbar1.Buttons(i)
'''
'''    'if x and y are within this button's location
'''    With tmpButton
'''      If OldX >= .Left And OldX <= .Left + .Height And OldY >= .Top And OldY <= .Top + .Height Then
'''        Exit For
'''      End If
'''    End With
'''  Next i
End Sub


Private Sub Toolbar1_MouseDown(Button As Integer, Shift As Integer, X As Single, Y As Single)

  'save x and y for double-click
  
  OldX = X
  OldY = Y
End Sub


Private Sub txtComment_Change()
  
  'if editing text,
  If txtComment.Visible Then
    'set dirty flag
    MarkAsDirty
  End If
  
End Sub

Private Sub txtComment_KeyPress(KeyAscii As Integer)

  Select Case KeyAscii
  Case 13
    'enter key causes loss of focus
    'set focus to picdraw
    picDraw.SetFocus
    KeyAscii = 0
    
  Case 124
    'ignore pipe character
    KeyAscii = 0
  End Select
End Sub


Private Sub txtComment_LostFocus()

  'save text into comment
  Comment(txtComment.Tag).Text = txtComment.Text
  
  'hide text box
  txtComment.Visible = False
  
  'redraw comment by drawing entire layout
  DrawLayout
End Sub


Private Sub txtComment_MouseMove(Button As Integer, Shift As Integer, X As Single, Y As Single)

  'make sure cursor is correct
  txtComment.MousePointer = vbIbeam
  
End Sub

Private Sub VScroll1_Change()
  
  On Error GoTo ErrHandler
  
  'always make sure tip is hidden
  If picTip.Visible Then
    picTip.Visible = False
  End If
  
  'ignore during startup (form not visible) or if due to code change
  If Not Me.Visible Or CodeChange Then
    Exit Sub
  End If
  
  'change offset Value to match new scrollbar Value
  OffsetY = -VScroll1.Value / 100
  
  'check to see if min or Max Value need to be adjusted
  'due to scrolling past edge, then scrolling back...
  
  'if > min
  If VScroll1.Value > VScroll1.Min Then
    'compare scroll min against actual min
    If VScroll1.Min < (MinY - 0.5) * 100 Then
      VScroll1.Min = (MinY - 0.5) * 100
    End If
  End If

  'if < Max
  If VScroll1.Value < VScroll1.Max Then
    'compare scroll Max against actual Max
    If VScroll1.Max > (MaxY + 1.3 - picDraw.Height / DSF) * 100 Then
      'also need to check that Max Value is not less than current min
      'if picdraw is taller than drawing area
      If (MaxY + 1.3 - picDraw.Height / DSF) * 100 < VScroll1.Min Then
        'set Max and min equal
        VScroll1.Max = VScroll1.Min
      Else
        VScroll1.Max = (MaxY + 1.3 - picDraw.Height / DSF) * 100
      End If
    End If
  End If

  'redraw
  DrawLayout
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub VScroll1_GotFocus()

  On Error Resume Next
  
  picDraw.SetFocus
End Sub

Private Sub ChangeToRoom(ByRef OldSel As TSel, ByVal NewRoom As Long, ByVal ObjType As ELSelection)

  'changes the currently selected exit so its 'to' room matches NewRoom
  'before calling this method, the new to-room has already been validated
  
  Dim i As Long, tmpCoord As LCoord
  Dim tmpRoom As Long, tmpID As String, OldRoom As Integer
  
  On Error GoTo ErrHandler
  
  With Exits(OldSel.Number)(OldSel.ExitID)
    'save oldroom if we need it
    OldRoom = .Room
    
    'if a transfer,
    If ObjType = lsTransPt Then
      'make change to room
      .Room = TransPt(NewRoom).Room(0)
      'set transfer
      .Transfer = NewRoom
      'mark as second leg
      .Leg = 1
      TransPt(NewRoom).Count = 2
      TransPt(NewRoom).ExitID(1) = OldSel.ExitID
      'mark selection as part one of a two way
      OldSel.Leg = llFirst
      OldSel.TwoWay = ltwOneWay
    Else
      'make change
      .Room = NewRoom
      
      'if previous 'to' room was an err pt
      If .Transfer < 0 Then
        'delete the errpt
        CompactObjList ErrPt(-.Transfer).Order
        With ErrPt(-.Transfer)
          .Visible = False
          .ExitID = ""
          .FromRoom = 0
          .Room = 0
          .Order = 0
          .Loc.X = 0
          .Loc.Y = 0
        End With
        'reset trans pt
        .Transfer = 0
      End If
      
      'if line previously had a transfer
      If .Transfer > 0 Then
        'reset transfer
        TransPt(.Transfer).Count = TransPt(.Transfer).Count - 1
        If TransPt(.Transfer).Count = 0 Then
          'transfer no longer needed
          DeleteTransfer .Transfer
        Else
          'ensure 'from' room is in first position- Room(0)
          'and 'to' room is in second position- Room(1)
          'in this case, OldSel.Number is the 'to' room, so
          'we need only check if second element = OldSel.Number
          'switching if necessary
          If .Leg = 0 Then
            With TransPt(.Transfer)
              'use i to help in switch
              i = .Room(1)
              .Room(1) = .Room(0)
              .Room(0) = i
              .ExitID(0) = .ExitID(1)
              'dont need to keep index for second leg since it is gone
              .ExitID(1) = vbNullString
              tmpCoord = .Loc(0)
              .Loc(0) = .Loc(1)
              .Loc(1) = tmpCoord
              tmpCoord = .SP
              .SP = .EP
              .EP = tmpCoord
              Exits(.Room(0))(.ExitID(0)).Leg = 0
            End With
          Else
            .Leg = 0
          End If
        End If
        .Transfer = 0
      End If
      
      'if there is a reciprocal,
      If IsTwoWay(OldSel.Number, OldSel.ExitID, tmpRoom, tmpID, 2) Then
        'check for transfer
        If Exits(tmpRoom)(tmpID).Transfer <> 0 Then
          'if transfer has only one leg,
          If TransPt(Exits(tmpRoom)(tmpID).Transfer).Count = 1 Then
            'use this transfer
            TransPt(Exits(tmpRoom)(tmpID).Transfer).Count = 2
            .Transfer = Exits(tmpRoom)(tmpID).Transfer
            'this is second leg
            OldSel.Leg = llSecond
            .Leg = 1
            'set exit ID
            TransPt(Exits(tmpRoom)(tmpID).Transfer).ExitID(1) = OldSel.ExitID
          Else
            'ensure selection is not using a transfer leg
            OldSel.Leg = llNoTrans
            .Leg = 0
          End If
        Else
          'no transfer;
          'ensure selection is not using a transfer leg
          OldSel.Leg = llNoTrans
          .Leg = 0
        End If
        '
        OldSel.TwoWay = ltwOneWay
      Else
        'no reciprocal;
        'change selection to no leg
        OldSel.Leg = llNoTrans
        OldSel.TwoWay = ltwSingle
        .Leg = 0
      End If
    End If
    
    'if the exit is not a new exit
    If .Status <> esNew Then
      'save original room before changing
      If .OldRoom = 0 Then
        .OldRoom = OldRoom
      End If
      'mark it as changed
      .Status = esChanged
    End If
  End With
  
  'reposition exit end points
  SetExitPos OldSel.Number, OldSel.ExitID
Exit Sub

ErrHandler:
  '*'Debug.Assert False
  Resume Next
End Sub

Private Sub VScroll1_Scroll()

  VScroll1_Change
End Sub

     */
    }
  }
}
