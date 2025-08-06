using EnvDTE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.API;
using static WinAGI.Editor.Base;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.CodeDom;
using System.Text.Json.Serialization.Metadata;
using Microsoft.VisualStudio.Shell.Interop;

namespace WinAGI.Editor {
    public partial class frmLayout : Form {
        #region Enums
        // to track the custom mouse icon, use a seperate enum
        private enum ccMousePtr {
            ccNone,
            ccMoveSel,
            ccSelObj,
            ccHorizon,
            ccBottom,
            ccRight,
            ccLeft,
            ccOther,
            ccAddObj,
        }
        #endregion

        #region Constants
        private const string LAYOUT_FMT_VERSION = "3.0";
        private const float RM_SIZE = 0.8f;
        //private const float RM_SZ_X = 0.8f;
        //private const float RM_SZ_Y = 0.8f;
        #endregion

        #region Structures
        private struct ELInfo {
            public bool Analyzed; // used when extracting layouts from logics
            public bool Placed;
            public byte Group;
            public byte[] Exits; // 4 exits
            public byte[] Enter; // 4 enters
            public ELInfo() {
                Exits = new byte[4];
                Enter = new byte[4];
            }
        }
        private struct RoomInfo {
            public PointF Loc; // location of room on layout
            public bool Visible; // if TRUE, room will be drawn on layout
            public int Order; // order in which object is drawn
            public bool ShowPic; // if TRUE, room box will include scaled img of visual pic
            public AGIExits Exits = new();

            public RoomInfo() {
            }
        }
        private struct ErrorPtInfo {
            public PointF Loc; // location of error point on layout
            public bool Visible; // if TRUE, error object will be drawn on layout
            public int Order; // order in which object is drawn
            //public int Room; // the number of the 'non-existent' room or -1 if invalid room number (i.e. a misspelled logicID)
            public string ExitID; // ID of the exit connected TO this err point
            public int FromRoom; // Room number where the exit error is located (the bad new.room() command)
            internal int Room;
            // NOTE: no exits can ever originate from an error point
            // and only one exit can point to a single error point
        }
        private struct CommentInfo {
            public PointF Loc; // location of comment box on layout
            public bool Visible; // if TRUE, comment box will be drawn on layout
            public int Order; // order in which object is drawn
            public SizeF Size; // height/width of comment box
            public string Text; // text of comment
        }
        private struct TransPtInfo {
            public PointF[] Loc; // location of each transfer point on layout
            public int Count; // 1 = breaks a 'one-way' exit; 2 = breaks a 'reciprocal' exit
            public int Order; // order in which transfer objects will be drawn on layout
            public PointF EP; // end point coords
            public PointF SP; // start point coords
            public byte[] Room; // originating room(s) (if two way, each room has to be noted)
            public string[] ExitID; // exit ID(s) that transfer breaks
            public TransPtInfo() {
                Loc = new PointF[2];
                Loc[0] = new PointF(0, 0); // first transfer point location
                Loc[1] = new PointF(0, 0); // second transfer point location
                EP = new PointF(0, 0); // end point coords
                SP = new PointF(0, 0); // start point coords
                Room = new byte[2];
                ExitID = new string[2];
                ExitID[0] = ""; // first exit ID
                ExitID[1] = ""; // second exit ID
            }
        }
        private struct ObjInfo {
            public ELSelection Type; // type of object (room, transpt, errpt, comment)
            public int Number; // number of object
            public ObjInfo(ELSelection type, int number) {
                Type = type;
                Number = number;
            }
        }
        private struct TSel {
            public ELSelection Type; // Type of object currently selected
            public int Number; // index of object selected or room associated with selected exit
            public string ExitID; // ID of selected exit
            public ELLeg Leg; // which leg of an exit that has transfer
            public ELTwoWay TwoWay;
            public int Point; // 0 means starting point of an exit is being moved, 1 means ending point of an exit is being moved
            public float X1, Y1, X2, Y2, X3, Y3; // coordinates used to draw handles around selection in drawing surface pixel scale
        }

        #endregion

        #region Members
        public bool IsChanged = false;
        private Font layoutFont, transptFont;
        private Color agOffWhite = Color.FromArgb(255, 255, 255, 254); // used for the background of the layout editor

        //layout object variables
        private RoomInfo[] Room;       // rectangles
        private TransPtInfo[] TransPt; // circles (always drawn in pairs)
        private ErrorPtInfo[] ErrPt;   // triangles
        private CommentInfo[] Comment; // rounded corner rectangles
        // exits - a collection of AGIExit objects

        //  //extraction variables
        private ELInfo[] ELRoom;

        // scale and drawing variables
        int DrawScale;
        private PointF Min, Max;
        private Point Offset;
        private bool NoScrollUpdate = false; // if TRUE, don't update scrollbars
        List<ObjInfo> ObjOrder;  //display order of objects
        private float DSF;
        private const int MGN = 12;

        //  'other variables
        //  Private AddPicToo As CheckBoxConstants

        //  'selection and moving variables
        private ELayoutTool SelTool;
        private TSel Selection;
        //  Private HoldTool As Boolean
        private List<ObjInfo> SelectedObjects = [];
        private Point MouseDownPt = new(); // used to track mouse down position

        //  Private mX As Single, mY As Single, mDX As Single, mDY As Single
        //  Private OldX As Single, OldY As Single
        //  Private AnchorX As Single, AnchorY As Single
        private Point DragAnchor;
        private bool DragCanvas;
        private bool MoveExit, MoveObj;
        //  Private DrawExit As Long, DragSelect As Boolean
        //  Private NewExitReason As EEReason
        //  Private NewExitTrans As Long, NewExitRoom As Long
        //  Private SizingComment As Long
        private ccMousePtr CustMousePtr;
        //  Private CtrlDown As Boolean

        // StatusStrip Items
        internal ToolStripStatusLabel spCurX;
        internal ToolStripStatusLabel spCurY;
        internal ToolStripStatusLabel spScale;
        internal ToolStripStatusLabel spTool;
        internal ToolStripStatusLabel spID;
        internal ToolStripStatusLabel spType;
        internal ToolStripStatusLabel spRoom1;
        internal ToolStripStatusLabel spRoom2;
        internal ToolStripStatusLabel spStatus;
        #endregion

        public frmLayout() {
            InitializeComponent();
            picDraw.MouseWheel += picDraw_MouseWheel;
            hScrollBar1.MouseDown += hScrollBar1_MouseDown;
            vScrollBar1.MouseDown += vScrollBar1_MouseDown;
            MdiParent = MDIMain;
            InitStatusStrip();
            ObjOrder = [];
            Room = new RoomInfo[256];
            TransPt = new TransPtInfo[256];
            ErrPt = new ErrorPtInfo[256];
            Comment = new CommentInfo[256];
            ELRoom = new ELInfo[256];
            for (int i = 0; i < 256; i++) {
                Room[i] = new RoomInfo { Loc = new PointF(0, 0), Visible = false, Order = 0, ShowPic = false };
                TransPt[i] = new TransPtInfo { Loc = new PointF[2], Count = 0, Order = 0, EP = new PointF(0, 0), SP = new PointF(0, 0) };
                TransPt[i].Loc[0] = new PointF(0, 0);
                TransPt[i].Loc[1] = new PointF(0, 0);
                ErrPt[i] = new ErrorPtInfo { Loc = new PointF(0, 0), Visible = false, Order = 0, ExitID = "", Room = 0, FromRoom = -1 };
                Comment[i] = new CommentInfo { Loc = new PointF(0, 0), Visible = false, Order = 0, Size = new SizeF(0, 0), Text = "" };
                ELRoom[i] = new ELInfo();
            }
        }

        #region Form Event Handlers
        private void frmLayout_Load(object sender, EventArgs e) {
            txtComment.BackColor = WinAGISettings.CmtFillColor.Value;
            txtComment.ForeColor = WinAGISettings.CmtEdgeColor.Value;

            // default scale
            DrawScale = WinAGISettings.LEScale.Value;
            DSF = (float)(40 * Math.Pow(1.25, DrawScale - 1));

            InitFonts();

            //SelTool = ltSelect;
        }

        private void frmLayout_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.MdiFormClosing) {
                return;
            }
            bool closing = AskClose();
            e.Cancel = !closing;
        }

        private void frmLayout_FormClosed(object sender, FormClosedEventArgs e) {
            LEInUse = false;
            LayoutEditor = null;
            layoutFont?.Dispose();
            transptFont?.Dispose();
            picDraw.MouseWheel -= picDraw_MouseWheel;
            hScrollBar1.MouseDown -= hScrollBar1_MouseDown;
            vScrollBar1.MouseDown -= vScrollBar1_MouseDown;
        }

        private void frmLayout_Resize(object sender, EventArgs e) {
            // redraw the layout when the form is resized, but
            // not if minimized or not visible
            if (!Visible || WindowState == FormWindowState.Minimized) {
                return;
            }
            if (picDraw != null) {
                // Redraw the layout
                SetScrollBars();
                DrawLayout();
            }
        }

        private void frmLayout_VisibleChanged(object sender, EventArgs e) {
            // do initial drawlayout when the form becomes visible
            if (Visible) {
                DrawLayout();
            }
        }
        #endregion

        #region Menu Event Handlers
        /// <summary>
        /// Dynamic function to set up the resource menu.
        /// </summary>
        public void SetResourceMenu() {
            mnuRSave.Enabled = IsChanged;
            mnuToggleAllPics.Text = WinAGISettings.LEShowPics.Value ? "Hide All Pictures" : "Show All Pictures";
        }

        /// <summary>
        /// Dynamic function to reset the resource menu.
        /// </summary>
        public void ResetResourceMenu() {
            mnuRSave.Enabled = true;
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            if (IsChanged) {
                SaveLayout();
            }
        }

        private void mnuRepair_Click(object sender, EventArgs e) {

        }

        private void mnuToggleAllPics_Click(object sender, EventArgs e) {
            WinAGISettings.LEShowPics.Value = !WinAGISettings.LEShowPics.Value;
            for (int i = 0; i < 256; i++) {
                Room[i].ShowPic = WinAGISettings.LEShowPics.Value;
            }
            DrawLayout();
            MarkAsChanged();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            SetEditMenu();
        }

        private void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e) {
            ResetEditMenu();
        }

        private void mnuEdit_DropDownOpening(object sender, EventArgs e) {
            SetEditMenu();
            mnuEdit.DropDownItems.AddRange([
                mnuShowRoom,
                mnuEditLogic,
                mnuEditPicture,
                mnuDelete,
                mnuInsert,
                mnuSelectAll,
                mnuEditSep1,
                mnuOrder,
                mnuTogglePicture,
                mnuProperties,
                mnuEditSep2,
                mnuToggleGrid]);
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            contextMenuStrip1.Items.AddRange([
                mnuShowRoom,
                mnuEditLogic,
                mnuEditPicture,
                mnuDelete,
                mnuInsert,
                mnuSelectAll,
                mnuEditSep1,
                mnuOrder,
                mnuTogglePicture,
                mnuProperties,
                mnuEditSep2,
                mnuToggleGrid]);
            ResetEditMenu();
        }

        private void SetEditMenu() {
            // Enable or disable the Edit menu items based on the current selection
            //mnuEditLogic.Enabled = (Selection.Type == lsRoom || Selection.Type == lsTransPt);
            //mnuEditPicture.Enabled = (Selection.Type == lsRoom);
            //mnuDelete.Enabled = (Selection.Type <> lsNone);
            //mnuInsert.Enabled = (Selection.Type <> lsNone);
            mnuSelectAll.Enabled = true;
            //mnuOrderUp.Enabled = SelectedObject Order < Max;
            //mnuOrderDown.Enabled = SelectedObject Order > 0;
            //mnuOrderFront.Enabled = SelectedObject Order != Max;
            //mnuOrderBack.Enabled = SelectedObject Order != 0;
            //mnuShowPicture.Enabled = (Selection.Type == lsRoom);
            mnuToggleGrid.Enabled = true; // always enabled
            mnuToggleGrid.Text = WinAGISettings.LEShowGrid.Value ? "Hide Grid Lines" : "Show Grid Lines";
            //mnuProperties.Enabled = (Selection.Type <> lsNone);

            /*
            'properties is only enabled if a room is selected
            .mnuECustom2.Caption = "Room Properties" & vbTab & "Ctrl+D"
            .mnuECustom2.Enabled = (Selection.Type = lsRoom)

            'toggle-draw-pic is only enabled if a room is selected
            mnuECustom1.Enabled = (Selection.Type = lsRoom)
            If Selection.Type = lsRoom Then
                If Room(Selection.Number).ShowPic Then
                    mnuECustom1.Caption = "Hide Room Picture" & vbTab & "Ctrl+R"
                Else
                    mnuECustom1.Caption = "Show Room Picture" & vbTab & "Ctrl+R"
                End If
            Else
                mnuECustom1.Caption = "Show Room Picture" & vbTab & "Ctrl+R"
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
            */
        }

        private void ResetEditMenu() {
            // Reset the Edit menu items to default state
            mnuEditLogic.Enabled = true;
            mnuEditPicture.Enabled = true;
            mnuDelete.Enabled = true;
            mnuInsert.Enabled = true;
            mnuSelectAll.Enabled = true;
            mnuTogglePicture.Enabled = true;
            mnuToggleGrid.Enabled = true;
            mnuProperties.Enabled = true;
            mnuOrderUp.Enabled = true;
            mnuOrderDown.Enabled = true;
            mnuOrderFront.Enabled = true;
            mnuOrderBack.Enabled = true;
        }

        private void mnuShowRoom_Click(object sender, EventArgs e) {

        }

        private void mnuEditLogic_Click(object sender, EventArgs e) {

        }

        private void mnuEditPicture_Click(object sender, EventArgs e) {

        }

        private void mnuDelete_Click(object sender, EventArgs e) {

        }

        private void mnuInsert_Click(object sender, EventArgs e) {

        }

        private void mnuSelectAll_Click(object sender, EventArgs e) {

        }

        private void mnuShowPicture_Click(object sender, EventArgs e) {

        }

        private void mnuToggleGrid_Click(object sender, EventArgs e) {
            WinAGISettings.LEShowGrid.Value = !WinAGISettings.LEShowGrid.Value;
            DrawLayout();
        }

        private void mnuProperties_Click(object sender, EventArgs e) {

        }

        private void mnuOrderUp_Click(object sender, EventArgs e) {

        }

        private void mnuOrderDown_Click(object sender, EventArgs e) {

        }

        private void mnuOrderFront_Click(object sender, EventArgs e) {

        }

        private void mnuOrderBack_Click(object sender, EventArgs e) {

        }
        #endregion

        #region ToolStrip Event Handlers
        private void btnZoomIn_Click(object sender, EventArgs e) {
            ChangeScale(1);
        }

        private void btnZoomOut_Click(object sender, EventArgs e) {
            ChangeScale(-1);
        }
        #endregion

        #region Control Event Handlers
        private void tmrScroll_Tick(object sender, EventArgs e) {

        }

        private void picDraw_MouseDown(object sender, MouseEventArgs e) {

            // check for right-clicks- they are used for context menu
            if (e.Button == MouseButtons.Right) {
                // reset mouse pointer
                picDraw.Cursor = Cursors.Default;
                TSel tmpSel = new();
                // if in a drawing mode,
                if (SelTool != ELayoutTool.ltSelect) {
                    // select the 'select' toolbar button
                    //Toolbar1_ButtonClick Toolbar1.Buttons("select")
                    //Toolbar1.Buttons("select").Value = tbrPressed
                }
                // check for a click on an exit or object
                tmpSel = ItemFromPos(e.Location);
                // if selection is a multi
                if (Selection.Type == ELSelection.lsMultiple) {
                    // if the cursor is over an exit, OR NOT over something in selection collection
                    if (tmpSel.Type == ELSelection.lsExit || !IsSelected(tmpSel.Type, tmpSel.Number, tmpSel.Leg)) {
                        // force selection before processing right-click
                        //picDraw_MouseDown vbLeftButton, 0, X, Y
                        //picDraw_MouseUp vbLeftButton, 0, X, Y
                    }
                }
                else {
                    // if selection is NOT the same
                    if (!SameAsSelection(ref tmpSel)) {
                        // force selection before processing right-click
                        //picDraw_MouseDown vbLeftButton, 0, X, Y
                        //picDraw_MouseUp vbLeftButton, 0, X, Y
                    }
                }
                // exit and allow context menu to display
                return;
            }
            // if resizing a comment, (can tell by checking pointer)
            if (picDraw.Cursor == Cursors.SizeNESW) {
                //'upper right or lower left corner;
                //If X >= Selection.X2 Then
                //'upper right
                //SizingComment = 2
                //Else
                //'lower left
                //SizingComment = 3
                //End If
                //shpMove.Left = Selection.X1 + 8
                //shpMove.Top = Selection.Y1 + 8
                //shpMove.Width = Selection.X2 - Selection.X1 - 9
                //shpMove.Height = Selection.Y2 - Selection.Y1 - 9
                //shpMove.Shape = vbShapeRoundedRectangle
                //shpMove.Visible = True
                //Exit Sub
            }
            else if (picDraw.Cursor == Cursors.SizeNWSE) {
                //'lower right or upper left corner;
                //If X >= Selection.X2 Then
                //'lower right
                //SizingComment = 4
                //Else
                //'upper left
                //SizingComment = 1
                //End If
                //shpMove.Left = Selection.X1 + 8
                //shpMove.Top = Selection.Y1 + 8
                //shpMove.Width = Selection.X2 - Selection.X1 - 9
                //shpMove.Height = Selection.Y2 - Selection.Y1 - 9
                //shpMove.Shape = vbShapeRoundedRectangle
                //shpMove.Visible = True
                //Exit Sub
            }
            // check for panning the drawing canvas
            if (ModifierKeys == Keys.Shift) {
                DragCanvas = true;
                DragAnchor = e.Location;
                MemoryStream msCursor = new(EditorResources.EPC_MOVE);
                picDraw.Cursor = new Cursor(msCursor);
                return;
            }
            // acton to take depends on current selected tool
            switch (SelTool) {
            case ELayoutTool.ltSelect:
                // a LOT of things to check if the selection tool
                // is active; strategy is to first check for
                // dragging of an object, then we see if something
                // is clicked; if nothing is clicked, we can just exit
                // then we process whatever was clicked (object or
                // exit) depending on what the current selection is

                // check for drag-select operation
                //If (picDraw.Point(X, Y) = picDraw.BackColor Or picDraw.Point(X, Y) = RGB(128, 128, 128)) And Button = vbLeftButton And Shift = 0 Then
                //  'begin drag-select
                //  DragSelect = True
                //  AnchorX = X
                //  AnchorY = Y
                //  shpMove.Shape = vbShapeRectangle
                //  shpMove.Visible = False
                //  Exit Sub
                //End If

                //'check for a click on an edge or object
                TSel tmpSel = ItemFromPos(e.Location);

                // if nothing new is clicked on,
                // we can just deselect whatever is currently
                // selected and be done
                if (tmpSel.Type == ELSelection.lsNone) {
                    // nothing going on; make sure nothing is
                    // selected
                    DeselectObj();
                    return;
                }

                //'if current selection (not the newly clicked
                //'object/exit) is more than one object,
                //'check for clicks that expand it or deselect
                //'one of its objects before checking single
                //'selection actions
                //If Selection.Type = lsMultiple Then
                //  For i = 0 To Selection.Number - 1
                //    If SelectedObjects(i).Type = tmpSel.Type And Abs(SelectedObjects(i).Number) = tmpSel.Number Then
                //      'need to validate trans pt
                //      If SelectedObjects(i).Type = lsTransPt Then
                //        If SelectedObjects(i).Number < 0 Then
                //          If tmpSel.Leg = 1 Then
                //            'this is it
                //            Exit For
                //          End If
                //        Else
                //          If tmpSel.Leg = 0 Then
                //            'this is it
                //            Exit For
                //          End If
                //        End If
                //      Else
                //        'found room, errpt or comment
                //        Exit For
                //      End If
                //    End If
                //  Next i

                //  'if object being clicked IS in the current collection
                //  '(i WONT equal selection.number [the total number of objects in
                //  'the selection group])
                //  If i <> Selection.Number Then
                //    'if ctrl key is pressed
                //    If Shift = vbCtrlMask Then
                //      'un-select this object
                //      UnselectObj tmpSel.Type, IIf(tmpSel.Type = lsTransPt And tmpSel.Leg = 1, -tmpSel.Number, tmpSel.Number)
                //      'do i need to reselect? -no; redraw!
                //      DrawLayout True
                //      Exit Sub
                //    Else
                //      With shpMove
                //        'begin moving selected objects
                //        .Shape = vbShapeRectangle
                //        .Visible = True
                //        'get offset between x and selection shape
                //        '(for multiple selection, the shape doesn't include the
                //        ' 8 pixel offset; that only applies to single objects that
                //        ' need the black 'handles' drawn)
                //        mDX = Selection.X1 - X
                //        mDY = Selection.Y1 - Y

                //        'save anchor position prior to movement
                //        If Selection.Type = lsMultiple Then
                //          'anchor point is real world location of upper left corner selection shape
                //          AnchorX = .Left / DSF - OffsetX
                //          AnchorY = .Top / DSF - OffsetY
                //        Else
                //          'anchor point is current mouse pos
                //          AnchorX = X
                //          AnchorY = Y
                //        End If
                //      End With
                //      Set picDraw.MouseIcon = LoadResPicture("ELC_MOVING", vbResCursor)
                //      MoveObj = True
                //      Exit Sub
                //    End If
                //  Else
                //    'is user adding to collection?
                //    '(check for control key)
                //    If Shift = vbCtrlMask Then
                //      Select Case tmpSel.Type
                //      Case lsComment, lsErrPt, lsRoom, lsTransPt
                //        'add it to collection
                //        ReDim Preserve SelectedObjects(Selection.Number)
                //        With SelectedObjects(Selection.Number)
                //          .Type = tmpSel.Type
                //          'check for trans pt leg
                //          If tmpSel.Leg = 1 Then
                //            'its a second leg trans pt
                //            .Number = -tmpSel.Number
                //          Else
                //            'its a first leg trans pt or another object Type
                //            .Number = tmpSel.Number
                //          End If
                //        End With
                //        'increment Count
                //        Selection.Number = Selection.Number + 1
                //        'force redraw
                //        DrawLayout
                //        'exit so selection isn't wiped out by the
                //        'selection processing code below
                //        Exit Sub
                //      End Select
                //    End If

                //    'not adding new; continue with normal
                //    'processing of determining selection
                //    'reset multiple selection
                //    DeselectObj
                //  End If
                //End If

                //'depending on currently selected object,
                //'process the click to either select something else
                //'or move/take action on selection
                //Select Case Selection.Type
                //Case lsRoom, lsTransPt, lsErrPt, lsComment
                //  'if same as current selection
                //  If SameAsSelection(tmpSel) Then
                //    'if over a comment object's text area (cursor is ibeam)
                //    If Selection.Type = lsComment And picDraw.MousePointer = vbIbeam Then
                //      'draw comment on top
                //      DrawCmtBox Selection.Number
                //      'begin editing text in comment box
                //      With Comment(Selection.Number)
                //        txtComment.Left = (.Loc.X + OffsetX) * DSF + 6
                //        txtComment.Top = (.Loc.Y + OffsetY) * DSF + 4
                //        txtComment.Width = .Size.X * DSF - 12
                //        txtComment.Height = .Size.Y * DSF - 8
                //        txtComment.Text = .Text
                //        'move selection to beginning
                //        i = SendMessage(txtComment.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
                //        If i <> 0 Then
                //          Do
                //            'scroll up
                //            i = SendMessage(txtComment.hWnd, EM_SCROLL, SB_PAGEUP, 0)
                //            i = SendMessage(txtComment.hWnd, EM_GETFIRSTVISIBLELINE, 0, 0)
                //          Loop Until i = 0
                //        End If

                //        txtComment.SelLength = 0
                //        txtComment.Visible = True
                //        txtComment.Tag = Selection.Number
                //        'send mouse up msg to release mouse from picDraw
                //        i = SendMessage(picDraw.hWnd, WM_LBUTTONUP, 0, 0)
                //        'send mouse down msg to text box
                //        i = SendMessage(txtComment.hWnd, WM_LBUTTONDOWN, 0, CLng(X - txtComment.Left) + CLng(Y - txtComment.Top) * &H10000)
                //        txtComment.SetFocus
                //      End With
                //      Exit Sub
                //    End If

                //    'mousedown on current selected object means
                //    'start moving it
                //    MoveObj = True
                //    'calculate offet between object position and cursor position
                //    '(for single objects, include the 8 pixel offset for handles)
                //    mDX = Selection.X1 + 8 - X
                //    mDY = Selection.Y1 + 8 - Y

                //    Set picDraw.MouseIcon = LoadResPicture("ELC_MOVING", vbResCursor)

                //    shpMove.Left = Selection.X1 + 8
                //    shpMove.Top = Selection.Y1 + 8
                //    'show outline based on selection Type
                //    Select Case Selection.Type
                //    Case lsRoom
                //      shpMove.Width = RM_SIZE * DSF
                //      shpMove.Height = RM_SIZE * DSF
                //      shpMove.Shape = vbShapeRectangle

                //    Case lsTransPt
                //      shpMove.Width = RM_SIZE / 2 * DSF
                //      shpMove.Height = RM_SIZE / 2 * DSF
                //      shpMove.Shape = vbShapeCircle
                //    Case lsErrPt
                //      shpMove.Width = 0.6 * DSF
                //      shpMove.Height = RM_SIZE / 2 * DSF
                //      shpMove.Shape = vbShapeRectangle
                //    Case lsComment
                //      shpMove.Width = Comment(Selection.Number).Size.X * DSF ' - 1
                //      shpMove.Height = Comment(Selection.Number).Size.Y * DSF ' - 1
                //      shpMove.Shape = vbShapeRoundedRectangle

                //    End Select
                //    'now show it
                //    shpMove.Visible = True
                //    Exit Sub
                //  Else
                //    'something OTHER than the current selection
                //    'is clicked; if CTRL key is down
                //    If Shift = vbCtrlMask Then
                //      'if adding to a singly selected object
                //      Select Case Selection.Type
                //      Case lsComment, lsErrPt, lsRoom, lsTransPt
                //        Select Case tmpSel.Type
                //        Case lsComment, lsErrPt, lsRoom, lsTransPt
                //          'reset selection collection (leaving room for two objects)
                //          ReDim SelectedObjects(1)

                //          'add current selection
                //          With SelectedObjects(0)
                //            .Type = Selection.Type
                //            'check for trans pt leg
                //            If Selection.Leg = 1 Then
                //              'its a second leg trans pt
                //              .Number = -Selection.Number
                //            Else
                //              'its a first leg trans pt or another object Type
                //              .Number = Selection.Number
                //            End If
                //          End With

                //          'finish changing selection
                //          Selection.Type = lsMultiple
                //          Selection.Number = 2
                //          Selection.ExitID = vbNullString

                //          'add the newly selected object to collection
                //          With SelectedObjects(1)
                //            .Type = tmpSel.Type
                //            'check for trans pt leg
                //            If tmpSel.Leg = 1 Then
                //              'its a second leg trans pt
                //              .Number = -tmpSel.Number
                //            Else
                //              'its a first leg trans pt or another object Type
                //              .Number = tmpSel.Number
                //            End If
                //          End With

                //          'force update by re-selecting
                //          SelectObj Selection
                //          SetEditMenu
                //          Exit Sub
                //        End Select
                //      End Select
                //    End If

                //    'something new is clicked, and it's not being
                //    'added to current selection, so if there is
                //    'something already selected, we need to
                //    'deselect it first
                //    If Selection.Type <> lsNone Then
                //      'deselect it
                //      DeselectObj
                //    End If

                //    'select whatever is under the cursor
                //    If tmpSel.Type = lsExit Then
                //      'select this exit
                //      SelectExit tmpSel
                //      If CustMousePtr <> ccSelObj Then
                //        Set picDraw.MouseIcon = LoadResPicture("ELC_SELOBJ", vbResCursor)
                //        CustMousePtr = ccSelObj
                //        picDraw.MousePointer = vbCustom
                //      End If
                //    ElseIf tmpSel.Type <> lsNone Then
                //      'select this object
                //      SelectObj tmpSel
                //      If CustMousePtr <> ccSelObj Then
                //        Set picDraw.MouseIcon = LoadResPicture("ELC_SELOBJ", vbResCursor)
                //        CustMousePtr = ccSelObj
                //        picDraw.MousePointer = vbCustom
                //      End If
                //    End If
                //  End If

                //Case lsExit
                //  'current selection is an exit; let's check to see
                //  'if either end is clicked, meaning the exit is going
                //  'to be moved

                //  'check if either point is clicked by
                //  'testing cursor Value
                //  If picDraw.MousePointer = vbCrosshair Then
                //    'begin moving exit
                //    MoveExit = True

                //    'if on first point (from room end),
                //    If (X >= Selection.X1 And X <= Selection.X1 + 8 And Y >= Selection.Y1 And Y <= Selection.Y1 + 8) Then
                //      Selection.Point = 0
                //      linMove.X1 = Selection.X2 + 4
                //      linMove.Y1 = Selection.Y2 + 4
                //      linMove.X2 = Selection.X1 + 4
                //      linMove.Y2 = Selection.Y1 + 4
                //    Else
                //      Selection.Point = 1
                //      linMove.X1 = Selection.X1 + 4
                //      linMove.Y1 = Selection.Y1 + 4
                //      linMove.X2 = Selection.X2 + 4
                //      linMove.Y2 = Selection.Y2 + 4
                //    End If

                //    'now show it
                //    linMove.BorderColor = vbBlack
                //    linMove.BorderWidth = 1
                //    linMove.Visible = True

                //  ElseIf Not SameAsSelection(tmpSel) Then
                //    'the thing clicked is not the currently selected
                //    'exit
                //    '*'Debug.Assert Selection.Type <> lsNone
                //    '*'Debug.Assert Selection.Type = lsExit
                //    'deselect it
                //    DeselectObj

                //    'select whatever is under the cursor
                //    If tmpSel.Type = lsExit Then
                //      'select this exit
                //      SelectExit tmpSel
                //      Exit Sub
                //    ElseIf tmpSel.Type <> lsNone Then
                //      'select this object
                //      SelectObj tmpSel
                //      Exit Sub
                //    End If
                //  End If

                //Case lsNone
                //  'select whatever was clicked
                //  Select Case tmpSel.Type
                //  Case lsRoom, lsTransPt, lsErrPt, lsComment
                //    SelectObj tmpSel
                //  Case lsExit
                //    SelectExit tmpSel
                //  End Select
                //End Select

                //'reset edit menu, since selection may have changed
                //SetEditMenu
                //Exit Sub
                break;

            // done with actions taken while Select tool is active
            // here we check what happens if exit drawing tool is
            // active
            case ELayoutTool.ltEdge1:
            case ELayoutTool.ltEdge2:
            case ELayoutTool.ltOther:
                //'draw exits

                //'if ok to draw, cursor is custom; check against no drop
                //If picDraw.MousePointer = vbNoDrop Then
                //  'can't draw edge here
                //  Exit Sub
                //End If

                //'hide line (mouse move will show it again)
                //linMove.Visible = False
                //'set it back to normal
                //linMove.BorderColor = vbBlack
                //linMove.BorderWidth = 1

                //'set anchor to edge (or center) of selected room
                //ObjectFromPos tmpSel, X, Y

                //If tmpSel.Type = lsRoom Then
                //  'save room number
                //  NewExitRoom = tmpSel.Number

                //  'allow drawing to any room
                //  DrawExit = 1

                //  'set anchor for exit line
                //  Select Case NewExitReason
                //  Case erHorizon
                //    AnchorX = Room(tmpSel.Number).Loc.X + RM_SIZE / 2
                //    AnchorY = Room(tmpSel.Number).Loc.Y
                //  Case erBottom
                //    AnchorX = Room(tmpSel.Number).Loc.X + RM_SIZE / 2
                //    AnchorY = Room(tmpSel.Number).Loc.Y + RM_SIZE
                //  Case erRight
                //    AnchorX = Room(tmpSel.Number).Loc.X + RM_SIZE
                //    AnchorY = Room(tmpSel.Number).Loc.Y + RM_SIZE / 2
                //  Case erLeft
                //    AnchorX = Room(tmpSel.Number).Loc.X
                //    AnchorY = Room(tmpSel.Number).Loc.Y + RM_SIZE / 2
                //  Case erOther
                //    AnchorX = Room(tmpSel.Number).Loc.X + RM_SIZE / 2
                //    AnchorY = Room(tmpSel.Number).Loc.Y + RM_SIZE / 2
                //  End Select

                //  linMove.Y1 = (AnchorY + OffsetY) * DSF
                //  linMove.X1 = (AnchorX + OffsetX) * DSF
                //Else
                //  'save room and transfer number
                //  NewExitRoom = TransPt(tmpSel.Number).Room(1)
                //  NewExitTrans = tmpSel.Number

                //  'only allow drawing to matching room for this transfer
                //  DrawExit = 2

                //  'set anchor for exit line
                //  linMove.X1 = TransPt(tmpSel.Number).Loc(tmpSel.Leg).X
                //  linMove.Y1 = TransPt(tmpSel.Number).Loc(tmpSel.Leg).Y
                //  linMove.Y1 = (AnchorY + OffsetY) * DSF
                //  linMove.X1 = (AnchorX + OffsetX) * DSF
                //End If
                return;

            // if the room tool is active, determine what to do
            // with the mouse click
            case ELayoutTool.ltRoom:
                //'if cursor allows room here
                //If picDraw.MousePointer = vbCrosshair Then

                //  'if at Max already
                //  If Logics.Count = 256 Then
                //    MsgBox "Maximum number of logics already exist in this game. Remove one or more existing logic, and then try again.", vbInformation + vbOKOnly, "Can't Add Room"
                //    Exit Sub
                //  End If

                //  'show wait cursor
                //  WaitCursor

                //  'add a new room here
                //  With frmGetResourceNum
                //    .WindowFunction = grAddLayout
                //    .ResType = rtLogic
                //    '
                //    .chkIncludePic.Value = AddPicToo
                //    'setup before loading so ghosts don't show up
                //    .FormSetup

                //    'reset cursor while user makes selection
                //    Screen.MousePointer = vbDefault

                //    'show the form
                //    .Show vbModal, frmMDIMain

                //    'show wait cursor again
                //    WaitCursor

                //    'if not canceled
                //    If Not .Canceled Then
                //      'temporary logic
                //      Set tmpLogic = New AGILogic
                //      tmpLogic.ID = .txtID.Text

                //      'save current checkbox Value
                //      AddPicToo = .chkIncludePic.Value

                //      'disable drawing until we can set position
                //      blnDontDraw = True

                //      'add a new logic (always use room template)
                //      AddNewLogic .NewResNum, tmpLogic, True, False

                //      'dereference tmplogic
                //      Set tmpLogic = Nothing
                //      'InRoom property is set by AddNewLogic (based on blnTemplate Value)
                //      Logics(.NewResNum).Save
                //      'no need to keep the logic loaded now
                //      Logics(.NewResNum).Unload

                //      'update editor to show this room is now in the game
                //      'get new exits from the logic that was passed
                //      Set tmpExits = ExtractExits(Logics(.NewResNum))

                //      'if adding picture too
                //      If .chkIncludePic.Value = vbChecked Then
                //        'if a picture already exists, delete it before adding a new one
                //        If .chkIncludePic.Caption = "Replace existing Picture" Then
                //          RemovePicture .NewResNum
                //        End If
                //        AddNewPicture .NewResNum, Nothing
                //        'if loaded,
                //        If Pictures(.NewResNum).Loaded Then
                //          Pictures(.NewResNum).Unload
                //        End If
                //      End If

                //      'reposition to cursor
                //      With Room(.NewResNum)
                //        '*'Debug.Assert .Visible = True
                //        .Loc.X = GridPos(X / DSF - OffsetX)
                //        .Loc.Y = GridPos(Y / DSF - OffsetY)
                //        '*'Debug.Assert .Order = ObjCount - 1
                //      End With
                //      RepositionRoom .NewResNum

                //      'adjust Max and min
                //      AdjustMaxMin
                //      're-enable drawing
                //      blnDontDraw = False
                //      DrawLayout
                //    End If
                //  End With
                //  Unload frmGetResourceNum

                //  'reset cursor
                //  Screen.MousePointer = vbDefault
                //  CustMousePtr = ccNone
                //End If

                //'if not holding tool,
                //If Not HoldTool Then
                //  'go back to select tool
                //  Toolbar1_ButtonClick Toolbar1.Buttons("select")
                //  Toolbar1.Buttons("select").Value = tbrPressed
                //  HoldTool = False
                //End If
                return;

            // if the comment tool is active, process the
            // mouse click here
            case ELayoutTool.ltComment:
                //'begin drawing comment
                //AnchorX = X
                //AnchorY = Y
                //With shpMove
                //  .Left = AnchorX
                //  .Top = AnchorY
                //  .Width = RM_SIZE * DSF
                //  .Height = RM_SIZE * DSF
                //  .Shape = vbShapeRoundedRectangle
                //  .Visible = True
                //End With
                break;
            }
        }

        private void picDraw_MouseMove(object sender, MouseEventArgs e) {
            if (e.Location == MouseDownPt) {
                // Mouse hasn't moved, so no need to do anything
                return;
            }
            MouseDownPt = e.Location;

            float x = (e.X - Offset.X) / DSF;
            float y = (e.Y - Offset.Y) / DSF;
            spCurX.Text = x.ToString("0.000");
            spCurY.Text = y.ToString("0.000");

            if (e.Button == MouseButtons.Right) {
                return;
            }
            // if dragging the canvas, adjust the view
            if (DragCanvas) {
                Offset.X += e.X - DragAnchor.X;
                Offset.Y += e.Y - DragAnchor.Y;
                DragAnchor.X = e.X;
                DragAnchor.Y = e.Y;
                SetScrollBars();
                DrawLayout();
                return;
            }
            /*

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
                    If Room(Selection.Number).Exits(Selection.ExitID).Transfer >= 0 Then
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
                    shpMove.Width = shpMove.Left + shpMove.Width - (GridPos(X / DSF - Offset.X) + Offset.X) * DSF
                    shpMove.Left = (GridPos(X / DSF - Offset.X) + Offset.X) * DSF
                    Else
                    shpMove.Left = (GridPos((shpMove.Left + shpMove.Width) / DSF - RM_SIZE - Offset.X) + Offset.X) * DSF
                    shpMove.Width = RM_SIZE * DSF
                    End If
                    If shpMove.Top + shpMove.Height - Y > RM_SIZE * DSF Then
                    shpMove.Height = shpMove.Top + shpMove.Height - (GridPos(Y / DSF - Offset.Y) + Offset.Y) * DSF
                    shpMove.Top = (GridPos(Y / DSF - Offset.Y) + Offset.Y) * DSF
                    Else
                    shpMove.Top = (GridPos((shpMove.Top + shpMove.Height) / DSF - RM_SIZE - Offset.Y) + Offset.Y) * DSF
                    shpMove.Height = RM_SIZE * DSF
                    End If
                    Exit Sub
                Case 2
                    'upper right
                    If X > shpMove.Left + RM_SIZE * DSF Then
                    shpMove.Width = (GridPos(X / DSF - Offset.X) + Offset.X) * DSF - shpMove.Left - 1
                    Else
                    shpMove.Width = RM_SIZE * DSF
                    End If
                    If shpMove.Top + shpMove.Height - Y > RM_SIZE * DSF Then
                    shpMove.Height = shpMove.Top + shpMove.Height - (GridPos(Y / DSF - Offset.Y) + Offset.Y) * DSF
                    shpMove.Top = (GridPos(Y / DSF - Offset.Y) + Offset.Y) * DSF
                    Else
                    shpMove.Top = (GridPos((shpMove.Top + shpMove.Height) / DSF - RM_SIZE - Offset.Y) + Offset.Y) * DSF
                    shpMove.Height = RM_SIZE * DSF
                    End If
                    Exit Sub
                Case 3
                    'lower left
                    If shpMove.Left + shpMove.Width - X > RM_SIZE * DSF Then
                    shpMove.Width = shpMove.Left + shpMove.Width - (GridPos(X / DSF - Offset.X) + Offset.X) * DSF
                    shpMove.Left = (GridPos(X / DSF - Offset.X) + Offset.X) * DSF
                    Else
                    shpMove.Left = (GridPos((shpMove.Left + shpMove.Width) / DSF - RM_SIZE - Offset.X) + Offset.X) * DSF
                    shpMove.Width = RM_SIZE * DSF
                    End If
                    If Y > shpMove.Top + RM_SIZE * DSF Then
                    shpMove.Height = (GridPos(Y / DSF - Offset.Y) + Offset.Y) * DSF - shpMove.Top - 1
                    Else
                    shpMove.Height = RM_SIZE * DSF
                    End If
                    Exit Sub
                Case 4
                    'lower right
                    If X > shpMove.Left + RM_SIZE * DSF Then
                    shpMove.Width = (GridPos(X / DSF - Offset.X) + Offset.X) * DSF - shpMove.Left - 1
                    Else
                    shpMove.Width = RM_SIZE * DSF
                    End If
                    If Y > shpMove.Top + RM_SIZE * DSF Then
                    shpMove.Height = (GridPos(Y / DSF - Offset.Y) + Offset.Y) * DSF - shpMove.Top - 1
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
                    Select Case Room(Selection.Number).Exits(Selection.ExitID).Reason
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

                    Select Case Room(Selection.Number).Exits(Selection.ExitID).Reason
                    Case erHorizon
                        'if  exit reason is BOTTOM
                        If Room(TransPt(tmpSel.Number).Room(0)).Exits(tmpID).Reason = erBottom Then
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
                        If Room(TransPt(tmpSel.Number).Room(0)).Exits(tmpID).Reason = erHorizon Then
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
                        If Room(TransPt(tmpSel.Number).Room(0)).Exits(tmpID).Reason = erLeft Then
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
                        If Room(TransPt(tmpSel.Number).Room(0)).Exits(tmpID).Reason = erRight Then
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
                        If Room(TransPt(tmpSel.Number).Room(0)).Exits(tmpID).Reason = erOther Then
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
        '        shpMove.Left = (GridPos((X + mDX - 8) / DSF - Offset.X) + Offset.X) * DSF
        '        shpMove.Top = (GridPos((Y + mDY - 8) / DSF - Offset.Y) + Offset.Y) * DSF
                'convert surface coordinates into world coordinates,
                'apply grid setting, then re-convert back into surface coordinates
                shpMove.Left = (GridPos((X + mDX) / DSF - Offset.X) + Offset.X) * DSF
                shpMove.Top = (GridPos((Y + mDY) / DSF - Offset.Y) + Offset.Y) * DSF
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
                    tmpX = (Room(tmpSel.Number).Loc.X + RM_SIZE / 2 + Offset.X) * DSF
                    tmpY = (Room(tmpSel.Number).Loc.Y + RM_SIZE / 2 + Offset.Y) * DSF
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
                    Select Case Room(TransPt(tmpSel.Number).Room(0)).Exits(tmpID).Reason
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
                    Select Case Room(TransPt(tmpSel.Number).Room(0)).Exits(tmpID).Reason
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
            */
        }

        private void picDraw_MouseUp(object sender, MouseEventArgs e) {
            if (DragCanvas) {
                // stop the drag
                DragCanvas = false;
                // reset to default cursor
                picDraw.Cursor = Cursors.Default;
                CustMousePtr = ccMousePtr.ccNone;
            }

            /*
            switch (SelTool) {
            case ltSelect
                if (MoveExit) {
                    MoveExit = false;
                    //linMove.Visible = False
                    // validate new exit pos
                    // by checking cursor (a custom cursor means drop is OK)
                    if (picDraw.Cursor == vbCustom) {
                        // get object where exit is being dropped
                        TSel tmpSel = ObjectFromPos(e.X, e.Y);
                        // if not changing both exits of a two-way exit
                        if (Selection.TwoWay != ltwBothWays) {
                            // if changing to room
                            if (Selection.Point == 1) {
                                // if target room has changed
                                if (tmpSel.Number != Room[Selection.Number].Exits[Selection.ExitID].Room) {
                                    // change the to-room for this exit
                                    ChangeToRoom (Selection, tmpSel.Number, tmpSel.Type);
                                    // redraw
                                    DrawLayout();
                                    MarkAsChanged();
                                }
                            }
                            else {
                                // if from room has changed
                                if (tmpSel.Number != Selection.Number) {
                                    // change the from room; keep type the same
                                    ChangeFromRoom (Selection, tmpSel.Number);
                                }
                                else {
                                    // nothing to do; just exit
                                    // reset cursor
                                    picDraw.Cursor = Cursors.Crosshair;
                                    CustMousePtr = ccNone;
                                    return;
                                }
                                // redraw
                                DrawLayout();
                                MarkAsChanged();
                            }
                        }
                        else {
                            // two-way; change both from and to room
                            int tgt = tmpSel.Number;
                            // get reciprocal exit
                            tmpSel = Selection;
                            IsTwoWay (tmpSel.Number, tmpSel.ExitID, tmpSel.Number, tmpSel.ExitID, 1);
                            // determine which is from room and which is to room
                            if (Selection.Point == 1) {
                                // tmpSel contains to room
                                // if target room has changed
                                if (i != Room[Selection.Number].Exits[Selection.ExitID].Room) {
                                    // change the to-room for this exit (always target a room)
                                    // MAKE SURE to do this BEFORE changing 'from' room;
                                    // when changing from room, if it finds a reciprocal,
                                    // it will delete it, so then the change 'to' room function
                                    // no longer has an exit to move
                                    ChangeToRoom (Selection, i, lsRoom);
                                    // change from room, using tmpSel as old selection
                                    ChangeFromRoom (tmpSel, i);
                                }
                                else {
                                    // target hasn't changed; don't do anything
                                    // reset cursor
                                    picDraw.Cursor = Cursors.Crosshair;
                                    CustMousePtr = ccNone;
                                    return;
                                }
                            }
                            else {
                                // tmpsel contains from room
                                // if from room has changed
                                if (i != Selection.Number) {
                                    // change to room
                                    // MAKE SURE to do this BEFORE changing 'from' room;
                                    // when changing from room, if it finds a reciprocal,
                                    // it will delete it, so then the change 'to' room function
                                    // no longer has an exit to move
                                    ChangeToRoom (tmpSel, i, lsRoom);
                                    // change from room, using Selection
                                    ChangeFromRoom (Selection, i);
                                }
                                else {
                                    // from room hasn't changed; don't do anything
                                    // reset cursor
                                    picDraw.Cursor = Cursors.Crosshair;
                                    CustMousePtr = ccNone;
                                    return;
                                }
                            }
                            // copy tempsel back into selection
                            //Selection = tmpSel;
                            // force selection of both exits
                            //if (Selection.TwoWay == ltwOneWay) {
                                Selection.TwoWay = ltwBothWays;
                            //}
                            // redraw
                            DrawLayout();
                            MarkAsChanged();
                        }
                    }
                    // reset cursor
                    picDraw.Cursor = Cursors.Crosshair;
                    CustMousePtr = ccNone;
                }
                if (MoveObj) {
                    // drop the selected objects at this location
                    DropObjs (X + mDX, Y + mDY);
                    return;
                }
                if (SizingComment != 0) {
                    Comment[Selection.Number].Loc.X = shpMove.Left / DSF - Offset.X;
                    Comment[Selection.Number].Loc.Y = shpMove.Top / DSF - Offset.Y;
                    Comment[Selection.Number].Size.X = GridPos((shpMove.Width + 1) / DSF);
                    Comment[Selection.Number].Size.Y = GridPos((shpMove.Height + 1) / DSF);
                    shpMove.Visible = false;
                    SizingComment = 0;
                    MarkAsChanged();
                    AdjustMaxMin();
                    DrawLayout();
                    return;
                }
                // if drag-selecting
                if (DragSelect) {
                    DragSelect = false;
                    // deselect anything previously selected
                    DeselectObj();
                    // if selection shape is visible
                    if (shpMove.Visible) {
                        // hide it
                        shpMove.Visible = false;
                        // get selected objects
                        GetSelectedObjects();
                    }
                }
                break;
            case ltEdge1:
            case ltEdge2:
            case ltOther:
                // if target room is same as starting room
                if (tmpSel.Number == NewExitRoom) {
                    // unless line is at least .4 units (half the room width/height),
                    // assume user doesnt want an exit
                    if (Math.Sqr((linMove.X2 - linMove.X1) * (linMove.X2 - linMove.X1) + (linMove.Y2 - linMove.Y1) * (linMove.Y2 - linMove.Y1)) / DSF < RM_SIZE / 2) {
                        return;
                    }
                }
                // reset drawexit flag
                DrawExit = 0;
                // hide line
                linMove.Visible = false;
                // if not a valid drop zone
                if (picDraw.MousePointer == Cursors.NoDrop) {
                    // cancel exit drawing
                    return;
                }
                // get target room number
                ObjectFromPos (tmpSel, e.X, e.Y);
                // create new exit
                // if dropping on a room
                if (tmpSel.Type == lsRoom) {
                    CreateNewExit (NewExitRoom, tmpSel.Number, NewExitReason);
                    // if drawing exits both ways
                    if (SelTool == ltEdge2) {
                        // add reciprocal
                        CreateNewExit (tmpSel.Number, NewExitRoom, ((NewExitReason + 1) & 3) + 1);
                    }
                }
                else {
                    // what if transfer already has two exits?  hmmm, have to test
                    // Debug.Assert TransPt(tmpSel.Number).Count = 1
                    // dropping on a transfer pt
                    CreateNewExit (NewExitRoom, TransPt(tmpSel.Number).Room(0), NewExitReason);
                }
                // redraw
                DrawLayout
                if (!HoldTool) {
                    // go back to select tool
                    //Toolbar1_ButtonClick Toolbar1.Buttons("select")
                    //Toolbar1.Buttons("select").Value = tbrPressed
                    HoldTool = false;
                    picDraw.Cursor = Cursors.Default;
                }
                break;
            case ltRoom:
                // all actions take place in mousedown
                break;
            case ltComment:
                shpMove.Visible = false;
                // add a comment here
                int i;
                for (i = 1; i < 256; i++) {
                    if (!Comment[i].Visible) {
                        break;
                    }
                }
                if (i == 256) {
                    // too many
                    return;
                }
                Comment[i].Visible = true;
                Comment[i].Loc.X = GridPos(AnchorX / DSF - Offset.X);
                Comment[i].Loc.Y = GridPos(AnchorY / DSF - Offset.Y);
                Comment[i].Size.X = GridPos(shpMove.Width / DSF);
                Comment[i].Size.Y = GridPos(shpMove.Height / DSF);
                Comment[i].Order = ObjOrder.Count;
                ObjInfo obj = new(i, lsComment);
                // adjust Max and min
                AdjustMaxMin();
                // draw the comment box
                DrawCmtBox(i);
                // begin editing text in comment box
                txtComment.Text = "";
                txtComment.Left = Comment[i].Loc.X * DSF+ Offset.X + 6;
                txtComment.Top = Comment[i].Loc.Y * DSF + Offset.Y + 4;
                txtComment.Width = Comment[i].Size.X * DSF - 12;
                txtComment.Height = Comment[i].Size.Y * DSF - 8;
                txtComment.Visible = true;
                txtComment.Focus();
                // use tag property to id new comment
                txtComment.Tag = i;
                if (!HoldTool) {
                    // go back to select tool
                    //Toolbar1_ButtonClick Toolbar1.Buttons("select")
                    //Toolbar1.Buttons("select").Value = tbrPressed
                    HoldTool = false;
                }
                break;
            case ltNone:
                break;
            }
            */
        }

        private void picDraw_MouseDoubleClick(object sender, MouseEventArgs e) {
            /*
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
            */
        }

        private void picDraw_MouseWheel(object sender, MouseEventArgs e) {
            // confirm cursor is over the drawing area
            if (MDIMain.ActiveMdiChild != this) {
                return;
            }
            if (e.Button != MouseButtons.None) {
                return;
            }
            Point pt = picDraw.PointToClient(Cursor.Position);
            if (!picDraw.ClientRectangle.Contains(pt)) {
                return;
            }
            // Handle mouse wheel scrolling for zooming in and out
            if (e.Delta > 0) {
                ChangeScale(1, true); // Zoom in
            }
            else if (e.Delta < 0) {
                ChangeScale(-1, true); // Zoom out
            }
        }

        private void picDraw_MouseLeave(object sender, EventArgs e) {
            spCurX.Text = "  --";
            spCurY.Text = "  --";
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e) {
            if (!NoScrollUpdate) {
                Offset.X = -hScrollBar1.Value;
                SetScrollBars();
                DrawLayout();
            }
        }

        private void hScrollBar1_MouseDown(object sender, MouseEventArgs e) {
            var sb = (HScrollBarMouseAware)sender;
            // Calculate the area of the left and right arrows
            int arrowSize = SystemInformation.HorizontalScrollBarArrowWidth;
            // left arrow
            Rectangle leftArrow = new Rectangle(0, 0, arrowSize, sb.Height);
            // right arrow
            Rectangle rightArrow = new Rectangle(sb.Width - arrowSize, 0, arrowSize, sb.Height);
            if (leftArrow.Contains(e.Location) && sb.Value == sb.Minimum) {
                // User clicked left arrow at minimum, extend range
                sb.Minimum -= sb.SmallChange;
                sb.Value = sb.Minimum;
            }
            else if (rightArrow.Contains(e.Location) && sb.Value == sb.Maximum - sb.LargeChange + 1) {
                // User clicked right arrow at maximum, extend range
                sb.Maximum += sb.SmallChange;
                sb.Value = sb.Maximum - sb.LargeChange + 1;
            }
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e) {
            if (!NoScrollUpdate) {
                Offset.Y = -vScrollBar1.Value;
                SetScrollBars();
                DrawLayout();
            }
        }

        private void vScrollBar1_MouseDown(object sender, MouseEventArgs e) {
            var sb = (VScrollBarMouseAware)sender;
            // Calculate the area of the up and down arrows
            int arrowSize = SystemInformation.VerticalScrollBarArrowHeight;
            // Up arrow
            Rectangle upArrow = new Rectangle(0, 0, sb.Width, arrowSize);
            // Down arrow
            Rectangle downArrow = new Rectangle(0, sb.Height - arrowSize, sb.Width, arrowSize);

            if (upArrow.Contains(e.Location) && sb.Value == sb.Minimum) {
                // User clicked up arrow at minimum, extend range
                sb.Minimum -= sb.SmallChange;
                sb.Value = sb.Minimum;
            }
            else if (downArrow.Contains(e.Location) && sb.Value == sb.Maximum - sb.LargeChange + 1) {
                // User clicked down arrow at maximum, extend range
                sb.Maximum += sb.SmallChange;
                sb.Value = sb.Maximum - sb.LargeChange + 1;
            }
        }
        #endregion

        #region temp code
        void DropObjs(float NewX, float NewY, bool NoGrid = false) {
            /*
            // drop selected object at new location
            MoveObj = false;

            // if forcing no-grid
            bool tmpGrid = false;
            if (NoGrid) {
                // cache usegrid value
                tmpGrid = Settings.LEUseGrid;
                // force grid off
                Settings.LEUseGrid = false;
            }

            // if selection is multiple objects, steps are different
            if (Selection.Type == lsMultiple) {
                // determine offset between new location and current selection shape position
                // (don't include 8 pixel offset; it's only used in single object movement)
                float mDX = (NewX - Selection.X1) / DSF;
                float mDY = (NewY - Selection.Y1) / DSF;

                // step through all objects in selection collection
                for (int i = 0; i <= Selection.Number - 1; i++) {
                    switch (SelectedObjects[i].Type) {
                    case lsRoom:
                        // set x and y values of room loc
                        Room[SelectedObjects[i].Number].Loc.X = GridPos(Room[SelectedObjects[i].Number].Loc.X + mDX);
                        Room[SelectedObjects[i].Number].Loc.Y = GridPos(Room[SelectedObjects[i].Number].Loc.Y + mDY);
                        break;

                    case lsTransPt:
                        if (SelectedObjects[i].Number < 1) {
                            // set x and y values of this trans pt (leg 1)
                            TransPt[-1 * SelectedObjects[i].Number].Loc[1].X = GridPos(TransPt[-1 * SelectedObjects[i].Number].Loc[1].X + mDX);
                            TransPt[-1 * SelectedObjects[i].Number].Loc[1].Y = GridPos(TransPt[-1 * SelectedObjects[i].Number].Loc[1].Y + mDY);
                        }
                        else {
                            // set x and y values of this trans pt (leg 0)
                            TransPt[SelectedObjects[i].Number].Loc[0].X = GridPos(TransPt[SelectedObjects[i].Number].Loc[0].X + mDX);
                            TransPt[SelectedObjects[i].Number].Loc[0].Y = GridPos(TransPt[SelectedObjects[i].Number].Loc[0].Y + mDY);
                        }
                        break;

                    case lsComment:
                        Comment[SelectedObjects[i].Number].Loc.X = GridPos(Comment[SelectedObjects[i].Number].Loc.X + mDX);
                        Comment[SelectedObjects[i].Number].Loc.Y = GridPos(Comment[SelectedObjects[i].Number].Loc.Y + mDY);
                        break;

                    case lsErrPt:
                        ErrPt[SelectedObjects[i].Number].Loc.X = GridPos(ErrPt[SelectedObjects[i].Number].Loc.X + mDX);
                        ErrPt[SelectedObjects[i].Number].Loc.Y = GridPos(ErrPt[SelectedObjects[i].Number].Loc.Y + mDY);
                        break;
                    }

                    MarkAsChanged();
                }

                // step through again and reposition everyone
                for (int i = 0; i <= Selection.Number - 1; i++) {
                    switch (SelectedObjects[i].Type) {
                    case lsRoom:
                        RepositionRoom(SelectedObjects[i].Number);
                        break;

                    case lsTransPt:
                        if (SelectedObjects[i].Number < 0)
                            RepositionRoom(TransPt[-1 * SelectedObjects[i].Number].Room[1]);
                        else
                            RepositionRoom(TransPt[SelectedObjects[i].Number].Room[0]);
                        break;

                    case lsErrPt:
                        SetExitPos(ErrPt[SelectedObjects[i].Number].FromRoom, ErrPt[SelectedObjects[i].Number].ExitID);
                        break;
                    }
                }

                // adjust layout area Max/min, in case objects are moved outside current boundaries
                AdjustMaxMin();

                // redraw to update everything
                DrawLayout();
            }
            else {
                // reposition the object, based on its type
                switch (Selection.Type) {
                case lsRoom:
                    // set x and y values of room loc
                    Room[Selection.Number].Loc.X = GridPos(NewX / DSF - Offset.X);
                    Room[Selection.Number].Loc.Y = GridPos(NewY / DSF - Offset.Y);

                    // reposition exits
                    RepositionRoom(Selection.Number);
                    break;

                case lsTransPt:
                    // set x and y values of this trans pt
                    TransPt[Selection.Number].Loc[Selection.Leg].X = GridPos((NewX) / DSF - Offset.X);
                    TransPt[Selection.Number].Loc[Selection.Leg].Y = GridPos((NewY) / DSF - Offset.Y);

                    // reposition exits
                    RepositionRoom(TransPt[Selection.Number].Room[0]);
                    break;

                case lsComment:
                    Comment[Selection.Number].Loc.X = GridPos((NewX) / DSF - Offset.X);
                    Comment[Selection.Number].Loc.Y = GridPos((NewY) / DSF - Offset.Y);

                    // set changed flag (since repositionroom is not called for comments)
                    MarkAsChanged();
                    break;

                case lsErrPt:
                    ErrPt[Selection.Number].Loc.X = GridPos((NewX) / DSF - Offset.X);
                    ErrPt[Selection.Number].Loc.Y = GridPos((NewY) / DSF - Offset.Y);

                    // don't reposition room! just redraw the exit to this errpt
                    SetExitPos(ErrPt[Selection.Number].FromRoom, ErrPt[Selection.Number].ExitID);
                    break;
                }

                // adjust layout area Max/min, in case objects are moved outside current boundaries
                AdjustMaxMin();

                // adjust selection location
                Selection.X1 = NewX - 8;
                Selection.Y1 = NewY - 8;
                // hide outline
                shpMove.Visible = false;
                // redraw without handles
                DrawLayout(false);
                // reposition handles by reselecting
                SelectObj(Selection);
            }

            // if forcing no-grid
            if (NoGrid) {
                // restore usegrid value from cache
                Settings.LEUseGrid = tmpGrid;
            }
            */
        }
        void tmpconverted() {
            /*
            */

            /*
            void KeyMoveSelection(int KeyCode, float sngOffset, bool NoGrid)
            {
                float sngNewX, sngNewY;

                // if moving multiple items
                if (Selection.Type == lsMultiple)
                {
                    // don't include offset for handles
                    sngNewX = Selection.X1;
                    sngNewY = Selection.Y1;
                }
                else
                {
                    // if moving a single shape, include the 8 pixel offset to account for the 'handles'
                    sngNewX = Selection.X1 + 8;
                    sngNewY = Selection.Y1 + 8;
                }

                switch (KeyCode)
                {
                    case vbKeyUp:
                        if (Selection.Type == lsMultiple)
                            shpMove.Top = shpMove.Top - sngOffset;
                        sngNewY = sngNewY - sngOffset;
                        break;
                    case vbKeyDown:
                        if (Selection.Type == lsMultiple)
                            shpMove.Top = shpMove.Top + sngOffset;
                        sngNewY = sngNewY + sngOffset;
                        break;
                    case vbKeyLeft:
                        if (Selection.Type == lsMultiple)
                            shpMove.Left = shpMove.Left - sngOffset;
                        sngNewX = sngNewX - sngOffset;
                        break;
                    case vbKeyRight:
                        if (Selection.Type == lsMultiple)
                            shpMove.Left = shpMove.Left + sngOffset;
                        sngNewX = sngNewX + sngOffset;
                        break;
                }

                // reposition the selection
                DropObjs(sngNewX, sngNewY, NoGrid);
            }
            */

            /*
            void MenuClickHelp()
            {
                // help with layout
                Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, "htm\\winagi\\Layout_Editor.htm");
            }
            */

            /*
            bool OverExit(float X, float Y)
            {
                // returns true if coordinates X,Y are over an exit (any exit)
                
            // set width to three so PointInLine function
            // can find lines that are two pixels widepicDraw.DrawWidth = 3;

                for (int i = 1; i <= 255; i++)
                {
                    if (Room[i].Visible)
                    {
                        for (int j = 0; j <= Room[i].Exits.Count - 1; j++)
                        {
                            // don't include deleted exits
                            if (Room[i].Exits[j].Status != esDeleted)
                            {
                                float tmpSPX, tmpSPY, tmpEPX, tmpEPY;
                                if (Room[i].Exits[j].Transfer <= 0)
                                {
                                    // starting point and ending point of line come directly from the exit's start-end points
                                    tmpSPX = Room[i].Exits[j].SPX;
                                    tmpSPY = Room[i].Exits[j].SPY;
                                    tmpEPX = Room[i].Exits[j].EPX;
                                    tmpEPY = Room[i].Exits[j].EPY;

                                    // don't bother checking, unless line is actually visible
                                    if (LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY))
                                    {
                                        // check for an arrow first
                                        if (PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY))
                                            return true;
                                        // if not on arrow, check line
                                        else if (PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY))
                                            return true;
                                    }
                                }
                                else
                                {
                                    // there are transfers; check both segments
                                    tmpSPX = Room[i].Exits[j].SPX;
                                    tmpSPY = Room[i].Exits[j].SPY;

                                    if (TransPt[Room[i].Exits[j].Transfer].Room[0] == i)
                                    {
                                        tmpEPX = TransPt[Room[i].Exits[j].Transfer].SP.X;
                                        tmpEPY = TransPt[Room[i].Exits[j].Transfer].SP.Y;
                                    }
                                    else
                                    {
                                        // swap ep and sp
                                        tmpEPX = TransPt[Room[i].Exits[j].Transfer].EP.X;
                                        tmpEPY = TransPt[Room[i].Exits[j].Transfer].EP.Y;
                                    }

                                    if (LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY))
                                    {
                                        if (PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY))
                                            return true;
                                        else if (PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY))
                                            return true;
                                    }

                                    // second segment
                                    if (TransPt[Room[i].Exits[j].Transfer].Room[0] == i)
                                    {
                                        tmpSPX = TransPt[Room[i].Exits[j].Transfer].EP.X;
                                        tmpSPY = TransPt[Room[i].Exits[j].Transfer].EP.Y;
                                    }
                                    else
                                    {
                                        tmpSPX = TransPt[Room[i].Exits[j].Transfer].SP.X;
                                        tmpSPY = TransPt[Room[i].Exits[j].Transfer].SP.Y;
                                    }

                                    tmpEPX = Room[i].Exits[j].EPX;
                                    tmpEPY = Room[i].Exits[j].EPY;

                                    if (LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY))
                                    {
                                        if (PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY))
                                            return true;
                                        else if (PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY))
                                            return true;
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }
            */
        }
        void tmplayoutform() {
            /*

        Private Sub ChangeFromRoom(ByRef OldSel As TSel, ByVal FromRoom As Long)
        'changes the currently selected exit so its from room matches FromRoom

        'before calling this method, the new FromRoom has already been validated

        Dim NewID As String

        With OldSel
            'create new exit from new 'from' room to current 'to' room of same Type
            NewID = CreateNewExit(FromRoom, Room(.Number).Exits(.ExitID).Room, Room(.Number).Exits(.ExitID).Reason)

            'delete old exit
            DeleteExit OldSel

            'select new line (never has a transfer)
            .Number = FromRoom
            .ExitID = NewID

            'if resulting exit has a transfer,
            If Room(.Number).Exits(.ExitID).Transfer <> 0 Then
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

        Private Function GetExitByTrans(ByVal RoomNumber As Long, ByVal TransNumber As Long) As Long

        'finds the exit in roomnumber with corresponding transnumber
        'if not found, returns -1

        Dim i As Long

        On Error GoTo ErrHandler

        With Room(RoomNumber).Exits
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
            Select Case ObjOrder[i].Type
            Case lsRoom
            'is room inside selection area?
            If IsInSelection(Room(ObjOrder[i].Number).Loc, RM_SIZE, RM_SIZE) Then
                'add it
                lngCount = lngCount + 1
                ReDim Preserve SelectedObjects(lngCount)
                SelectedObjects(lngCount) = ObjOrder[i]
            End If

            Case lsTransPt
            'is first transpt inside selection area?
            If IsInSelection(TransPt(ObjOrder[i].Number).Loc(0), RM_SIZE / 2, RM_SIZE / 2) Then
                'add it
                lngCount = lngCount + 1
                ReDim Preserve SelectedObjects(lngCount)
                SelectedObjects(lngCount) = ObjOrder[i]
            End If

            'is second pt inside selection area?
            If IsInSelection(TransPt(ObjOrder[i].Number).Loc(1), RM_SIZE / 2, RM_SIZE / 2) Then
                'add it
                lngCount = lngCount + 1
                ReDim Preserve SelectedObjects(lngCount)
                SelectedObjects(lngCount) = ObjOrder[i]
                'flag as second leg
                SelectedObjects(lngCount).Number = -1 * SelectedObjects(lngCount).Number
            End If

            Case lsErrPt
            'is errpt inside selection area?
            If IsInSelection(ErrPt(ObjOrder[i].Number).Loc, RM_SIZE * 0.75, RM_SIZE / 2) Then
                'add it
                lngCount = lngCount + 1
                ReDim Preserve SelectedObjects(lngCount)
                SelectedObjects(lngCount) = ObjOrder[i]
            End If

            Case lsComment
            'is comment inside selection area?
            If IsInSelection(Comment(ObjOrder[i].Number).Loc, Comment(ObjOrder[i].Number).Size.X, Comment(ObjOrder[i].Number).Size.Y) Then
                'add it
                lngCount = lngCount + 1
                ReDim Preserve SelectedObjects(lngCount)
                SelectedObjects(lngCount) = ObjOrder[i]
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


        Private Sub HighlightExitStart(ByVal RoomNumber As Long, ByVal Dir As EEReason)

        '*'Debug.Assert SelTool = ltEdge1 Or SelTool = ltEdge2 Or SelTool = ltOther

        NewExitReason = Dir

        With linMove
            Select Case Dir
            Case erHorizon
            .X1 = (Room(RoomNumber).Loc.X + Offset.X) * DSF
            .X2 = (Room(RoomNumber).Loc.X + RM_SIZE + Offset.X) * DSF
            .Y1 = (Room(RoomNumber).Loc.Y + Offset.Y) * DSF
            .Y2 = (Room(RoomNumber).Loc.Y + Offset.Y) * DSF
            Case erBottom
            .X1 = (Room(RoomNumber).Loc.X + Offset.X) * DSF
            .X2 = (Room(RoomNumber).Loc.X + RM_SIZE + Offset.X) * DSF
            .Y1 = (Room(RoomNumber).Loc.Y + RM_SIZE + Offset.Y) * DSF
            .Y2 = (Room(RoomNumber).Loc.Y + RM_SIZE + Offset.Y) * DSF
            Case erRight
            .X1 = (Room(RoomNumber).Loc.X + RM_SIZE + Offset.X) * DSF
            .X2 = (Room(RoomNumber).Loc.X + RM_SIZE + Offset.X) * DSF
            .Y1 = (Room(RoomNumber).Loc.Y + Offset.Y) * DSF
            .Y2 = (Room(RoomNumber).Loc.Y + RM_SIZE + Offset.Y) * DSF
            Case erLeft
            .X1 = (Room(RoomNumber).Loc.X + Offset.X) * DSF
            .X2 = (Room(RoomNumber).Loc.X + Offset.X) * DSF
            .Y1 = (Room(RoomNumber).Loc.Y + Offset.Y) * DSF
            .Y2 = (Room(RoomNumber).Loc.Y + RM_SIZE + Offset.Y) * DSF
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
        With Room(NewSel.Number).Exits(NewSel.ExitID)
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
            Room(tmpRoom).Exits(tmpID).Transfer = tmpTrans
            Room(tmpRoom).Exits(tmpID).Leg = 1
            SetExitPos tmpRoom, tmpID
            Else
            'Count is one
            .Count = 1
            End If

            'add from/to room info
            .Room(0) = NewSel.Number
            .Room(1) = Room(NewSel.Number).Exits(NewSel.ExitID).Room
            .ExitID(0) = NewSel.ExitID
        End With

        'set trans property of exit
        Room(NewSel.Number).Exits(NewSel.ExitID).Transfer = tmpTrans
        Room(NewSel.Number).Exits(NewSel.ExitID).Leg = 0


        'reposition exit lines
        SetExitPos NewSel.Number, NewSel.ExitID

        MarkAsChanged
        End Sub

        Private Function IsInSelection(Loc As LCoord, ByVal X2 As Single, ByVal Y2 As Single) As Boolean

        'if the area represented by Loc and Height/Width is fully inside the shpMove area
        'this method returns true

        Dim X1 As Single, Y1 As Single

        X1 = Loc.X
        Y1 = Loc.Y

        'convert line coordinates into screen coordinates
        X1 = (X1 + Offset.X) * DSF
        X2 = X2 * DSF + X1
        Y1 = (Y1 + Offset.Y) * DSF
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

        With Room(RoomNum).Exits(ExitID)
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
                If Room(RoomNum).Exits(ExitID).Leg = 0 Then
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

        For i = 0 To Room(FromRoom).Exits.Count - 1
            With Room(FromRoom).Exits(i)
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
                    If .Transfer = Room(RoomNum).Exits(ExitID).Transfer Then
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
            'mark as changed
            MarkAsChanged

        Else 'menu says 'Show'
            For i = 0 To 255
            If Pictures.Exists(i) Then
                Room(i).ShowPic = True
            End If
            Next i

            Settings.LEShowPics = True

            'change menu caption
            frmMDIMain.mnuRCustom2.Caption = "Hide All &Pics" & vbTab & "Ctrl+Alt+H"
            'mark as changed
            MarkAsChanged
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
            RefreshTree AGIResType.Logic, Selection.Number, umProperty

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
            MarkAsChanged

            'clear out exit info for the deleted logic AFTER removing it
            Set Room(Selection.Number).Exits = New AGIExits

        Case lsTransPt
            'remove the transfer point
            DeleteTransfer Selection.Number
            'deselect
            DeselectObj
            'redraw
            DrawLayout

            MarkAsChanged

        Case lsErrPt
            'remove the errpt and its exit line
            'mark exit as deleted
            With ErrPt(Selection.Number)
            Room(.FromRoom).Exits(.ExitID).Status = esDeleted
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
            MarkAsChanged

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
            MarkAsChanged

        Case lsExit
            'delete this exit
            DeleteExit Selection

            'deselect and redraw
            DeselectObj
            DrawLayout
            MarkAsChanged
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
                If tmpScroll < hScrollBar1.Min Then
                tmpScroll = hScrollBar1.Min
                End If
                If tmpScroll > hScrollBar1.Max Then
                tmpScroll = hScrollBar1.Max
                End If
                hScrollBar1.Value = tmpScroll

                tmpScroll = -(picDraw.Height / 2 / DSF - TransPt(Selection.Number).Loc(Selection.Leg).Y - RM_SIZE / 4) * 100
                If tmpScroll < vScrollBar1.Min Then
                tmpScroll = vScrollBar1.Min
                End If
                If tmpScroll > vScrollBar1.Max Then
                tmpScroll = vScrollBar1.Max
                End If
                vScrollBar1.Value = tmpScroll

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
        For i = 0 To Room(FromRoom).Exits.Count - 1
            With Room(FromRoom).Exits(i)
                Select Case .Status
                Case esDeleted 'check to see if we are restoring the deleted exit
                'if deleted, AND ToRoom is the OldToRoom, AND Reason is same,
                If .OldRoom = ToRoom And .Reason = Reason Then
                    'restore the exit rather than create a new one
                    .Status = esOK

                    'return the identified exit
                    CreateNewExit = Room(FromRoom).Exits(i).ID

                    'make sure room is reset, if necessary
                    .Room = .OldRoom

                    'check for reciprocal exit to see if there might
                    'be an eligible transfer pt between the two rooms
                    If IsTwoWay(FromRoom, CreateNewExit, tmpRoom, tmpID, 2) Then
                    'use this transfer for the restored exit
                    .Transfer = Room(tmpRoom).Exits(tmpID).Transfer
                    .Leg = 1
                    '*'Debug.Assert TransPt(.Transfer).Count = 1
                    TransPt(.Transfer).Count = 2
                    TransPt(.Transfer).ExitID(1) = .ID
                    End If

                    'reposition (since rooms may have moved since it was deleted)
                    SetExitPos FromRoom, CreateNewExit

                    'mark as changed
                    MarkAsChanged
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
            For i = 0 To Room(FromRoom).Exits.Count - 1
            'if there is already an exit with this id
            If Val(Right$(Room(FromRoom).Exits(i).ID, 3)) = j Then
                'this id is in use; exit for loop and try again with next id
                Exit For
            End If
            Next i
        Loop Until i = Room(FromRoom).Exits.Count

        'add a new exit, using j as id
        CreateNewExit = "LE" & format$(j, "000")
        Room(FromRoom).Exits.Add(j, ToRoom, Reason, 0, 0, 0).Status = esNew

        'check for reciprocal exit
        If IsTwoWay(FromRoom, CreateNewExit, tmpRoom, tmpID, 2) Then
            'use this transfer for the new exit
            With Room(FromRoom).Exits(CreateNewExit)
            .Transfer = Room(tmpRoom).Exits(tmpID).Transfer
            .Leg = 1
            TransPt(.Transfer).Count = 2
            TransPt(.Transfer).ExitID(1) = .ID
            End With
        End If

        'set end points
        SetExitPos FromRoom, CreateNewExit

        'mark as changed
        MarkAsChanged
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
            If LogicCompiler.IncludeReserved Then
            NewExitText = NewExitText & LogicSourceSettings.ReservedDefines(atVar)(2).Name & " == " & LogicSourceSettings.ReservedDefines(atNum)(NewExit.Reason).Name & ")" & LineCR & Space$(Settings.LogicTabWidth) & "{" & LineCR & Space$(Settings.LogicTabWidth) & strNewRoom
            Else
            NewExitText = NewExitText & "v2 == " & CStr(NewExit.Reason) & ")" & LineCR & Space$(Settings.LogicTabWidth) & "{" & LineCR & Space$(Settings.LogicTabWidth) & strNewRoom
            End If
        End If
        NewExitText = NewExitText & Logics(NewExit.Room).ID & ");  [ ##" & NewExit.ID & "##" & LineCR & Space$(Settings.LogicTabWidth) & "}" & LineCR

        End Function

        Private Sub ShowLogic(tmpSel As TSel)

        Dim i As Long, j As Long
        Dim strUpdate As String

        On Error GoTo ErrHandler

        'determine if any exits need updating
        For j = 0 To Room(tmpSel.Number).Exits.Count - 1
            'if this exit status is not ok
            If Room(tmpSel.Number).Exits(j).Status <> esOK Then

            'update all exits for this logic
            UpdateLogicCode Logics(tmpSel.Number)

            'update the layout file
            UpdateLayoutFile euUpdateRoom, tmpSel.Number, Room(tmpSel.Number).Exits

            Exit For
            End If
        Next j

        'if errpt
        If tmpSel.Type = lsErrPt Then
            'open 'from' logic
            OpenLogic ErrPt(Selection.Number).FromRoom

            'find and highlight the errpt exit
            With frmMDIMain.ActiveMdiChild.rtfLogic.Selection.Range
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
            With frmMDIMain.ActiveMdiChild.rtfLogic
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
        v(0).X = (EPX + Offset.X) * DSF
        v(0).Y = (EPY + Offset.Y) * DSF

        'slope of line determines how to draw the arrow
        If Abs(DY) > Abs(DX) Then
            'mostly vertical line
            '(swap x and y formulas)
            'slope of line
            m = DX / DY
            'calculate first term (to save on cpu times by only doing the math once)
            // ^ operator not available in c#, have to use Math.Pow()
            ldivs = Sgn(DY) * Length / Sqr(m ^ 2 + 1)


            v(1).X = (EPX - ldivs * (m + tanTheta) + Offset.X) * DSF
            v(2).X = (EPX - ldivs * (m - tanTheta) + Offset.X) * DSF
            v(2).Y = (EPY - ldivs * (1 + m * tanTheta) + Offset.Y) * DSF
            v(1).Y = (EPY - ldivs * (1 - m * tanTheta) + Offset.Y) * DSF
        Else
            'mostly horizontal line

            'slope of line
            m = DY / DX
            'calculate first term (to save on cpu times by only doing the math once)
            ldivs = Sgn(DX) * Length / Sqr(m ^ 2 + 1)
            v(1).X = (EPX - ldivs * (1 + m * tanTheta) + Offset.X) * DSF
            v(2).X = (EPX - ldivs * (1 - m * tanTheta) + Offset.X) * DSF
            v(2).Y = (EPY - ldivs * (m + tanTheta) + Offset.Y) * DSF
            v(1).Y = (EPY - ldivs * (m - tanTheta) + Offset.Y) * DSF
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
        MoveToEx picDraw.hDC, (SPX + Offset.X) * DSF, (SPY + Offset.Y) * DSF, 0

        'begin path
        rtn = BeginPath(picDraw.hDC)

        'createline
        rtn = LineTo(picDraw.hDC, (EPX + Offset.X) * DSF, (EPY + Offset.Y) * DSF)

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


        Private Sub SelectObj(ByRef NewSel As TSel)

        Dim rtn As Long, i As Long
        Dim tmpLoc As LCoord, tmpSize As LCoord
        Dim Min.X As Single, Min.Y As Single
        Dim Max.X As Single, Max.Y As Single

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
            AdjustMenus rtLayout, True, True, IsChanged
        End If

        With Selection
            Select Case .Type
            Case lsRoom
            'save bitmaps under selection handles
            .X1 = (Room(.Number).Loc.X + Offset.X) * DSF - 8
            .Y1 = (Room(.Number).Loc.Y + Offset.Y) * DSF - 8
            .X2 = .X1 + RM_SIZE * DSF + 8
            .Y2 = .Y1 + RM_SIZE * DSF + 8

            MainStatusBar.Panels("Room1").Text = Logics(.Number).ID
            MainStatusBar.Panels("ID").Text = vbNullString

            Case lsTransPt
            .X1 = (TransPt(.Number).Loc(.Leg).X + Offset.X) * DSF - 8
            .Y1 = (TransPt(.Number).Loc(.Leg).Y + Offset.Y) * DSF - 8
            .X2 = .X1 + RM_SIZE / 2 * DSF + 8
            .Y2 = .Y1 + RM_SIZE / 2 * DSF + 8

            MainStatusBar.Panels("Room1").Text = Logics(TransPt(.Number).Room(0)).ID
            MainStatusBar.Panels("Room2").Text = Logics(TransPt(.Number).Room(1)).ID
            MainStatusBar.Panels("ID").Text = vbNullString

            Case lsComment
            .X1 = (Comment(.Number).Loc.X + Offset.X) * DSF - 8
            .Y1 = (Comment(.Number).Loc.Y + Offset.Y) * DSF - 8
            .X2 = .X1 + Comment(.Number).Size.X * DSF + 8
            .Y2 = .Y1 + Comment(.Number).Size.Y * DSF + 8

            Case lsErrPt
            .X1 = (ErrPt(.Number).Loc.X + Offset.X) * DSF - 8
            .Y1 = (ErrPt(.Number).Loc.Y + Offset.Y) * DSF - 8
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
            Min.X = picDraw.Width / DSF - Offset.X
            Min.Y = picDraw.Height / DSF - Offset.Y
            Max.X = -Offset.X
            Max.Y = -Offset.Y

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
                picDraw.Line ((tmpLoc.X + Offset.X) * DSF - 8, (tmpLoc.Y + Offset.Y) * DSF - 8)-Step(7, 7), vbBlack, BF
                picDraw.Line ((tmpLoc.X + tmpSize.X + Offset.X) * DSF, (tmpLoc.Y + Offset.Y) * DSF - 8)-Step(7, 7), vbBlack, BF
                picDraw.Line ((tmpLoc.X + Offset.X) * DSF - 8, (tmpLoc.Y + tmpSize.Y + Offset.Y) * DSF)-Step(7, 7), vbBlack, BF
                picDraw.Line ((tmpLoc.X + tmpSize.X + Offset.X) * DSF, (tmpLoc.Y + tmpSize.Y + Offset.Y) * DSF)-Step(7, 7), vbBlack, BF

                'set min and Max
                If tmpLoc.X < Min.X Then
                Min.X = tmpLoc.X
                End If
                If tmpLoc.Y < Min.Y Then
                Min.Y = tmpLoc.Y
                End If
                If tmpLoc.X + tmpSize.X > Max.X Then
                Max.X = tmpLoc.X + tmpSize.X
                End If
                If tmpLoc.Y + tmpSize.Y > Max.Y Then
                Max.Y = tmpLoc.Y + tmpSize.Y
                End If
            Next i

            'set extents of selection
            With NewSel
                .X1 = (Min.X + Offset.X) * DSF
                .X2 = (Max.X + Offset.X) * DSF
                .Y1 = (Min.Y + Offset.Y) * DSF
                .Y2 = (Max.Y + Offset.Y) * DSF
            End With

            'if not moving the selection
            If Not MoveObj Then
                'set selection shape
                shpMove.Shape = vbShapeRectangle
                shpMove.Left = (Min.X + Offset.X) * DSF
                shpMove.Top = (Min.Y + Offset.Y) * DSF
                shpMove.Width = (Max.X - Min.X) * DSF
                shpMove.Height = (Max.Y - Min.Y) * DSF
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
            If Room(.Number).Exits(.ExitID).Leg = 0 Then
                'ep of transpt matches ep of exit
                .X1 = (TransPt(Room(.Number).Exits(.ExitID).Transfer).EP.X + Offset.X) * DSF - 4
                .Y1 = (TransPt(Room(.Number).Exits(.ExitID).Transfer).EP.Y + Offset.Y) * DSF - 4

            Else
                'sp of trans pt matches ep of exit
                .X1 = (TransPt(Room(.Number).Exits(.ExitID).Transfer).SP.X + Offset.X) * DSF - 4
                .Y1 = (TransPt(Room(.Number).Exits(.ExitID).Transfer).SP.Y + Offset.Y) * DSF - 4

            End If

            .X2 = (Room(.Number).Exits(.ExitID).EPX + Offset.X) * DSF - 4
            .Y2 = (Room(.Number).Exits(.ExitID).EPY + Offset.Y) * DSF - 4

            Case llFirst
            'transfer; select first leg
            .X1 = (Room(.Number).Exits(.ExitID).SPX + Offset.X) * DSF - 4
            .Y1 = (Room(.Number).Exits(.ExitID).SPY + Offset.Y) * DSF - 4

            'if this is first exit for this transfer point
            If Room(.Number).Exits(.ExitID).Leg = 0 Then
                'sp of transpt matches sp of exit
                .X2 = (TransPt(Room(.Number).Exits(.ExitID).Transfer).SP.X + Offset.X) * DSF - 4
                .Y2 = (TransPt(Room(.Number).Exits(.ExitID).Transfer).SP.Y + Offset.Y) * DSF - 4

            Else
                'ep of transpt matches sp of exit
                .X2 = (TransPt(Room(.Number).Exits(.ExitID).Transfer).EP.X + Offset.X) * DSF - 4
                .Y2 = (TransPt(Room(.Number).Exits(.ExitID).Transfer).EP.Y + Offset.Y) * DSF - 4

            End If

            Case 0
            'no transfer; select whole line
            .X1 = (Room(.Number).Exits(.ExitID).SPX + Offset.X) * DSF - 4
            .Y1 = (Room(.Number).Exits(.ExitID).SPY + Offset.Y) * DSF - 4
            .X2 = (Room(.Number).Exits(.ExitID).EPX + Offset.X) * DSF - 4
            .Y2 = (Room(.Number).Exits(.ExitID).EPY + Offset.Y) * DSF - 4
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

            // ^ operator not available in c#, have to use Math.Pow()
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
            AdjustMenus rtLayout, True, True, IsChanged
        End If

        'write to status bar
        With MainStatusBar.Panels
            .Item("Room1").Text = "From: " & Logics(Selection.Number).ID
            'to room may be an error
            If Room(Selection.Number).Exits(Selection.ExitID).Transfer < 0 Then
            .Item("Room2").Text = "To: {error}"
            Else
            On Error Resume Next
            If Room(Selection.Number).Exits(Selection.ExitID).Room = 0 Then
                .Item("Room2").Text = "To: {error}"
            Else
                .Item("Room2").Text = "To: " & Logics(Room(Selection.Number).Exits(Selection.ExitID).Room).ID
            End If
            If Err.Number = vbObjectError + 564 Then
                .Item("Room2").Text = "To: {error}"
            End If
            On Error GoTo ErrHandler
            End If

            Select Case Selection.TwoWay
            Case ltwBothWays
            .Item("Type").Text = "Both Ways"
            If IsTwoWay(Selection.Number, Selection.ExitID, Room(Selection.Number).Exits(Selection.ExitID).Room, strID, 1) Then
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
        End Sub

        Public Function GetExits(ByVal LogicNumber As Byte) As AGIExits

        Dim i As Long
        Dim tmpExits As AGIExits

        Set tmpExits = New AGIExits

        For i = 0 To Room(LogicNumber).Exits.Count - 1
            With Room(LogicNumber).Exits.Item(i)
            tmpExits.Add(Val(Right$(.ID, 3)), .Room, .Reason, .Style, .Transfer, .Leg).Status = .Status
            End With
        Next i

        Set GetExits = tmpExits
        End Function

        public void ShowRoom() {

        Dim tmpExits As AGIExits

        'selects a non-room logic, and makes it a room
        'use resource number form
        With frmGetResourceNum
            .WindowFunction = grShowRoom
            .ResType = AGIResType.Logic
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
            RefreshTree AGIResType.Logic, .NewResNum, umProperty
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

        'mark as changed
        MarkAsChanged
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
        If GetNewResID(AGIResType.Logic, Selection.Number, strID, strDescription, True, 1) Then
            'redraw layout to reflect changes
            DrawLayout

            'mark as changed
            MarkAsChanged

            'if a matching logic editor is open, it needs to be updated too
            If LogicEditors.Count > 0 Then
            For Each tmpForm In LogicEditors
                If tmpForm.EditLogic.Number = Selection.Number Then
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

        Private Sub tmrScroll_Timer()

        'scrolling is in progress

        Dim sngShift As Single
        Dim lngPrevH As Long, lngPrevV As Long

        'shift amount is 5% of screen (1/4 of smallchange)

        'save current scroll values
        lngPrevH = hScrollBar1.Value
        lngPrevV = vScrollBar1.Value

        'disable drawing until scrolling is done
        blnDontDraw = True

        'determine direction by checking OldX and OldY
        If OldX < -10 Then
            'if scrolling left will go under minimum
            If hScrollBar1.Value - hScrollBar1.SmallChange / 4 < hScrollBar1.Min Then
            'reset minimum so we can scroll
            hScrollBar1.Min = hScrollBar1.Value - hScrollBar1.SmallChange / 4
            hScrollBar1.Value = hScrollBar1.Min
            Else
            'scroll left
            hScrollBar1.Value = hScrollBar1.Value - hScrollBar1.SmallChange / 4
            End If

        ElseIf OldX > picDraw.Width + 10 Then
            'if scrolling right will go over maximum
            If hScrollBar1.Value + hScrollBar1.SmallChange / 4 > hScrollBar1.Max Then
            'reset maximum so we can scroll
            hScrollBar1.Max = hScrollBar1.Value + hScrollBar1.SmallChange / 4
            hScrollBar1.Value = hScrollBar1.Max
            Else
            'scroll right
            hScrollBar1.Value = hScrollBar1.Value + hScrollBar1.SmallChange / 4
            End If

        End If

        If OldY < -10 Then
            'if scrolling up will go under minimum
            If vScrollBar1.Value - vScrollBar1.SmallChange / 4 < vScrollBar1.Min Then
            'reset minimum so we can scroll
            vScrollBar1.Min = vScrollBar1.Value - vScrollBar1.SmallChange / 4
            vScrollBar1.Value = vScrollBar1.Min
            Else
            'scroll up
            vScrollBar1.Value = vScrollBar1.Value - vScrollBar1.SmallChange / 4
            End If

        ElseIf OldY > picDraw.Height + 10 Then
            'if scrolling down will go above maximum
            If vScrollBar1.Value + vScrollBar1.SmallChange / 4 > vScrollBar1.Max Then
            'reset maximum so we can scroll
            vScrollBar1.Max = vScrollBar1.Value + vScrollBar1.SmallChange / 4
            vScrollBar1.Value = vScrollBar1.Max
            Else
            'scroll down
            vScrollBar1.Value = vScrollBar1.Value + vScrollBar1.SmallChange / 4
            End If
        End If

        'if drawing or moving an exit,
        If (DrawExit <> 0) Or MoveExit Then
            linMove.Y1 = (AnchorY + Offset.Y) * DSF
            linMove.X1 = (AnchorX + Offset.X) * DSF
        End If

        'if moving a single object
        If MoveObj And Selection.Type <> lsMultiple Then
            'adjust anchors
            AnchorX = AnchorX - (hScrollBar1.Value - lngPrevH)
            AnchorY = AnchorY - (vScrollBar1.Value - lngPrevV)
        End If

        'if dragging a selection
        If DragSelect Then
            'adjust anchors
            AnchorX = AnchorX - (hScrollBar1.Value - lngPrevH)
            AnchorY = AnchorY - (vScrollBar1.Value - lngPrevV)
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
            AdjustMenus rtLayout, True, True, IsChanged
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
                ObjOrder[i] = ObjOrder(i + 1)
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
                ObjOrder[i] = ObjOrder(i - 1)
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
        End Select

        'always reset hold tool flag
        HoldTool = False
        End Sub

        Private Sub txtComment_Change()

        'if editing text,
        If txtComment.Visible Then
            'set changed flag
            MarkAsChanged
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

        Private Sub vScrollBar1_Change()

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
        Offset.Y = -vScrollBar1.Value / 100

        'check to see if min or Max Value need to be adjusted
        'due to scrolling past edge, then scrolling back...

        'if > min
        If vScrollBar1.Value > vScrollBar1.Min Then
            'compare scroll min against actual min
            If vScrollBar1.Min < (Min.Y - 0.5) * 100 Then
            vScrollBar1.Min = (Min.Y - 0.5) * 100
            End If
        End If

        'if < Max
        If vScrollBar1.Value < vScrollBar1.Max Then
            'compare scroll Max against actual Max
            If vScrollBar1.Max > (Max.Y + 1.3 - picDraw.Height / DSF) * 100 Then
            'also need to check that Max Value is not less than current min
            'if picdraw is taller than drawing area
            If (Max.Y + 1.3 - picDraw.Height / DSF) * 100 < vScrollBar1.Min Then
                'set Max and min equal
                vScrollBar1.Max = vScrollBar1.Min
            Else
                vScrollBar1.Max = (Max.Y + 1.3 - picDraw.Height / DSF) * 100
            End If
            End If
        End If

        'redraw
        DrawLayout
        End Sub

        Private Sub ChangeToRoom(ByRef OldSel As TSel, ByVal NewRoom As Long, ByVal ObjType As ELSelection)

        'changes the currently selected exit so its 'to' room matches NewRoom
        'before calling this method, the new to-room has already been validated

        Dim i As Long, tmpCoord As LCoord
        Dim tmpRoom As Long, tmpID As String, OldRoom As Integer

        On Error GoTo ErrHandler

        With Room(OldSel.Number).Exits(OldSel.ExitID)
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
                    Room(.Room(0)).Exits(.ExitID(0)).Leg = 0
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
                If Room(tmpRoom).Exits(tmpID).Transfer <> 0 Then
                'if transfer has only one leg,
                If TransPt(Room(tmpRoom).Exits(tmpID).Transfer).Count = 1 Then
                    'use this transfer
                    TransPt(Room(tmpRoom).Exits(tmpID).Transfer).Count = 2
                    .Transfer = Room(tmpRoom).Exits(tmpID).Transfer
                    'this is second leg
                    OldSel.Leg = llSecond
                    .Leg = 1
                    'set exit ID
                    TransPt(Room(tmpRoom).Exits(tmpID).Transfer).ExitID(1) = OldSel.ExitID
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
            */
        }
        #endregion

        #region Methods
        private void InitStatusStrip() {
            spCurX = new ToolStripStatusLabel();
            spCurY = new ToolStripStatusLabel();
            spScale = new ToolStripStatusLabel();
            spTool = new ToolStripStatusLabel();
            spID = new ToolStripStatusLabel();
            spType = new ToolStripStatusLabel();
            spRoom1 = new ToolStripStatusLabel();
            spRoom2 = new ToolStripStatusLabel();
            spStatus = MDIMain.spStatus;
            // 
            // spCurX
            // 
            spCurX.AutoSize = false;
            spCurX.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spCurX.BorderStyle = Border3DStyle.SunkenInner;
            spCurX.Name = "spCurX";
            spCurX.Size = new System.Drawing.Size(70, 18);
            spCurX.Text = "layoutX";
            // 
            // spCurY
            // 
            spCurY.AutoSize = false;
            spCurY.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spCurY.BorderStyle = Border3DStyle.SunkenInner;
            spCurY.Name = "spCurY";
            spCurY.Size = new System.Drawing.Size(70, 18);
            spCurY.Text = "layoutY";
            // 
            // spScale
            // 
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(66, 18);
            spScale.Text = "layoutscale";
            // 
            // spTool
            // 
            spTool.Name = "spTool";
            spTool.Size = new System.Drawing.Size(61, 18);
            spTool.Text = "layouttool";
            // 
            // spID
            // 
            spID.Name = "spID";
            spID.Size = new System.Drawing.Size(51, 18);
            spID.Text = "layoutID";
            // 
            // spType
            // 
            spType.Name = "spType";
            spType.Size = new System.Drawing.Size(63, 18);
            spType.Text = "layouttype";
            // 
            // spRoom1
            // 
            spRoom1.Name = "spRoom1";
            spRoom1.Size = new System.Drawing.Size(78, 18);
            spRoom1.Text = "layoutRoom1";
            // 
            // spRoom2
            // 
            spRoom2.Name = "spRoom2";
            spRoom2.Size = new System.Drawing.Size(78, 18);
            spRoom2.Text = "layoutRoom2";
            spStatus = MDIMain.spStatus;
        }

        internal void InitFonts() {
            float fontSize = (float)(DSF / 12f); // 12
            layoutFont = new(WinAGISettings.EditorFontName.Value, fontSize, FontStyle.Regular);
            txtComment.Font = layoutFont;
            transptFont = new(WinAGISettings.EditorFontName.Value, fontSize * 1.5f, FontStyle.Bold);
        }

        internal bool GetLayoutData() {
            // check for current version of layout file
            // if invalid, maybe it's a v2 file
            // otherwise, layout needs to be extracted

            bool filefound;
            // use default filename (gameid and wal extension)
            string strFileName = EditGame.GameDir + EditGame.GameID + ".wal";
            if (File.Exists(strFileName)) {
                filefound = true;
            }
            else {
                // try the gamefile (take off the 'g', use 'l')
                strFileName = EditGame.GameFile[..^1] + "l";
                if (File.Exists(strFileName)) {
                    filefound = true;
                    // rename the file
                    try {
                        File.Move(strFileName, EditGame.GameDir + EditGame.GameID + ".wal");
                    }
                    catch {
                        // if any problems, just give up and 
                        // use extraction
                        filefound = false;
                    }
                    strFileName = EditGame.GameDir + EditGame.GameID + ".wal";
                }
                else {
                    filefound = false;
                }
            }
            // if file found, try to decode it
            if (filefound) {
                if (IsV3Layout()) {
                    return GetV3LayoutData(strFileName);
                }
                else {
                    return GetV2LayoutData(strFileName);
                }
            }
            else {
                // notify user
                if (MessageBox.Show(MDIMain,
                    "Layout data for this game is missing.\n\nDo you want to extract it automatically?",
                    "Layout Editor",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, 0, 0,
                    WinAGIHelp, "htm\\winagi\\Layout_Editor.htm") == DialogResult.Yes) {
                    return ExtractLayout();
                }
                return false;
            }
        }

        private bool IsV3Layout() {
            // return true if the layout file is a json file
            try {
                using StreamReader sr = File.OpenText(EditGame.GameDir + EditGame.GameID + ".wal");
                int next = sr.Read();
                while (next != -1) {
                    switch (next) {
                    case (int)'{':
                        return true;
                    case (int)' ':
                    case (int)'\n':
                    case (int)'\r':
                        break;
                    default:
                        return false;
                    }
                    next = sr.Read();
                }
                return false;
            }
            catch {
                // assume not valid
                return false;
            }
        }

        private bool GetV3LayoutData(string layoutfile) {
            List<string> strErrList = [];
            bool blnError = false, blnWarning = false;
            string strLine;

            // use deselect obj to configure toolbar,statusbar and menus
            DeselectObj();

            Selection.Type = ELSelection.lsNone;
            Selection.Number = 0;
            Selection.ExitID = "";
            Selection.Leg = ELLeg.llNoTrans;
            MoveExit = false;
            MoveObj = false;
            IsChanged = false;

            try {
                using StreamReader sr = new(layoutfile);
                // read data from file until'\x01' or EOF is found
                strLine = ReadNextBlock(sr);
                if (strLine == null) {
                    // empty file
                    return ExtractIfError(1);
                }

                // read header block
                LayoutFileHeader layoutdata = JsonSerializer.Deserialize<LayoutFileHeader>(strLine);
                // check version
                if (layoutdata.Version != LAYOUT_FMT_VERSION) {
                    // ignore for now
                }
                DrawScale = layoutdata.DrawScale;
                // validate it, just in case
                if (DrawScale < 1) DrawScale = 1;
                if (DrawScale > 9) DrawScale = 9;
                DSF = (float)(40 * Math.Pow(1.25, DrawScale - 1));
                Offset = layoutdata.Offset;
                InitFonts(); // reset font size based on DSF
                spScale.Text = "Scale: " + DrawScale;

                // objects
                strLine = ReadNextBlock(sr);
                while (strLine != null) {
                    var layoutobj = JsonSerializer.Deserialize<LayoutFileData>(strLine);
                    if (layoutobj is LFDRoom room) {
                        Room[room.Index].Visible = room.Visible;
                        Room[room.Index].ShowPic = room.ShowPic;
                        Room[room.Index].Loc = room.Loc;
                        Room[room.Index].Exits.CopyFrom(room.Exits);
                        Room[room.Index].Order = ObjOrder.Count;
                        ObjOrder.Add(new(ELSelection.lsRoom, room.Index));
                    }
                    else if (layoutobj is LFDTransPt transpt) {
                        TransPt[transpt.Index].Loc = transpt.Loc;
                        TransPt[transpt.Index].Room = [(byte)transpt.Rooms[0], (byte)transpt.Rooms[1]];
                        TransPt[transpt.Index].ExitID = transpt.ExitID;
                        TransPt[transpt.Index].Count = transpt.Count;
                        TransPt[transpt.Index].Order = ObjOrder.Count;
                        ObjOrder.Add(new ObjInfo(ELSelection.lsTransPt, transpt.Index));
                    }
                    else if (layoutobj is LFDErrPt errpt) {
                        ErrPt[errpt.Index].Visible = errpt.Visible;
                        ErrPt[errpt.Index].Loc = errpt.Loc;
                        ErrPt[errpt.Index].ExitID = errpt.ExitID;
                        ErrPt[errpt.Index].FromRoom = errpt.FromRoom;
                        ErrPt[errpt.Index].Room = errpt.Room;
                        ErrPt[errpt.Index].Order = ObjOrder.Count;
                        ObjOrder.Add(new ObjInfo(ELSelection.lsErrPt, errpt.Index));
                    }
                    else if (layoutobj is LFDComment comment) {
                        Comment[comment.Index].Visible = comment.Visible;
                        Comment[comment.Index].Loc = comment.Loc;
                        Comment[comment.Index].Size = comment.Size;
                        Comment[comment.Index].Text = comment.Text;
                        Comment[comment.Index].Order = ObjOrder.Count;
                        ObjOrder.Add(new ObjInfo(ELSelection.lsComment, comment.Index));
                    }
                    else if (layoutobj is LFDRenumber renum) {
                        ObjInfo obj = ObjOrder[Room[renum.OldNumber].Order];
                        ObjOrder[ObjOrder.IndexOf(obj)] = new ObjInfo(ELSelection.lsRoom, renum.Index);
                        // move oldroom data to newroom
                        Room[renum.Index].Loc = Room[renum.OldNumber].Loc;
                        Room[renum.Index].ShowPic = Room[renum.OldNumber].ShowPic;
                        Room[renum.Index].Visible = Room[renum.OldNumber].Visible;
                        Room[renum.Index].Exits.CopyFrom(Room[renum.OldNumber].Exits);
                        Room[renum.Index].Order = Room[renum.OldNumber].Order;
                        // clear old room
                        Room[renum.OldNumber].Loc = new();
                        Room[renum.OldNumber].ShowPic = false;
                        Room[renum.OldNumber].Visible = false;
                        Room[renum.OldNumber].Exits.Clear();
                        Room[renum.OldNumber].Order = 0;
                        for (int i = 1; i < 256; i++) {
                            // update room exits
                            for (int j = 0; j < Room[i].Exits.Count; j++) {
                                if (Room[i].Exits[j].Room == renum.OldNumber) {
                                    Room[i].Exits[j].Room = renum.Index;
                                }
                            }
                            // update trans pts
                            if (TransPt[i].Count != 0) {
                                for (int j = 0; j < 2; j++) {
                                    if (TransPt[i].Room[j] == renum.OldNumber) {
                                        TransPt[i].Room[j] = (byte)renum.Index;
                                    }
                                }
                            }
                            // update errpts
                            if (ErrPt[i].Visible) {
                                if (ErrPt[i].FromRoom == renum.OldNumber) {
                                    ErrPt[i].FromRoom = renum.Index;
                                }
                            }
                        }
                        // mark as changed so it gets updated
                        MarkAsChanged();
                    }
                    else if (layoutobj is LFDUpdate update) {
                        // should already be visible and located; all that's
                        // needed is to update the exits
                        Debug.Assert(Room[update.Index].Visible);
                        LayoutUpdate(update.Index, update.Exits);
                        // mark as changed to force update
                        MarkAsChanged();
                    }
                    else if (layoutobj is LFDShowRoom showroom) {
                        Room[showroom.Index].Visible = true;
                        // first update exits
                        LayoutUpdate(showroom.Index, showroom.Exits);
                        // an added room or hidden room being shown may be replacing
                        // an error point or need its hidden exits restored
                        bool needpos = true;
                        // look for exits pointing to error points which match this
                        for (int i = 1; i < 256; i++) {
                            // if room is visible (but NOT the room being added)
                            if (Room[i].Visible && i != showroom.Index) {
                                if (Room[i].Exits.Count > 0) {
                                    for (int j = 0; j < Room[i].Exits.Count; j++) {
                                        // if exit points to this room
                                        if (Room[i].Exits[j].Room == showroom.Index) {
                                            int transfer = Room[i].Exits[j].Transfer;
                                            // in case an err pt is found, deal with it
                                            if (transfer < 0) {
                                                if (needpos) {
                                                    // move it here
                                                    Room[showroom.Index].Loc.X = GridPos(ErrPt[-transfer].Loc.X - 0.1f);
                                                    Room[showroom.Index].Loc.Y = GridPos(ErrPt[-transfer].Loc.Y - 0.1402f);
                                                    // only the first errpt is replaced by the new room
                                                    int order = ErrPt[-transfer].Order;
                                                    ObjInfo obj = new(ELSelection.lsRoom, showroom.Index);
                                                    ObjOrder[order] = obj;
                                                    Room[showroom.Index].Order = order;
                                                    needpos = false;
                                                }
                                                else {
                                                    // more than one matching errpt is just removed
                                                    CompactObjList(ErrPt[-transfer].Order);
                                                }
                                                // hide errpt
                                                ErrPt[-transfer].Visible = false;
                                                ErrPt[-transfer].ExitID = "";
                                                ErrPt[-transfer].FromRoom = 0;
                                                ErrPt[-transfer].Room = 0;
                                                ErrPt[-transfer].Order = 0;
                                                // clear transfer marker
                                                Room[i].Exits[j].Transfer = 0;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // if location still needed (showing a room that didn't replace
                        // an errpt or unhiding a room)
                        if (needpos) {
                            Room[showroom.Index].Loc = GetInsertPos(
                                (picDraw.ClientSize.Width / 2 - Offset.X) / DSF,
                                (picDraw.ClientSize.Width / 2 - Offset.Y) / DSF);
                            Room[showroom.Index].Order = ObjOrder.Count;
                            ObjInfo obj = new(ELSelection.lsRoom, showroom.Index);
                            ObjOrder.Add(obj);
                        }
                        // mark as changed to force update
                        MarkAsChanged();
                    }
                    else if (layoutobj is LFDHideRoom hideroom) {
                        Debug.Assert(Room[hideroom.Index].Visible);
                        // first, check for any existing transfers or errpts FROM the room being hidden
                        for (int i = 0; i < Room[hideroom.Index].Exits.Count; i++) {
                            int transfer = Room[hideroom.Index].Exits[i].Transfer;
                            if (transfer > 0) {
                                // remove the transfer point
                                DeleteLoadTransfer(transfer);
                            }
                            else if (transfer < 0) {
                                // remove errpt
                                CompactObjList(ErrPt[-transfer].Order);
                                ErrPt[-transfer].Visible = false;
                                ErrPt[-transfer].ExitID = "";
                                ErrPt[-transfer].FromRoom = 0;
                                ErrPt[-transfer].Room = 0;
                                ErrPt[-transfer].Loc = new();
                                ErrPt[-transfer].Order = 0;
                            }
                        }
                        // step through all other exits, 
                        for (int i = 1; i < 256; i++) {
                            // only need to check rooms that are currently visible
                            if (i != hideroom.Index && Room[i].Visible) {
                                for (int j = 0; j < Room[i].Exits.Count; j++) {
                                    if (Room[i].Exits[j].Room == hideroom.Index) {
                                        // check for transfer pt
                                        if (Room[i].Exits[j].Transfer > 0) {
                                            // remove it
                                            DeleteLoadTransfer(Room[i].Exits[j].Transfer);
                                        }
                                        // if room is deleted, add an error point
                                        if (!EditGame.Logics.Contains(hideroom.Index)) {
                                            // add an ErrPt
                                            if (Room[i].Exits[j].Transfer >= 0) {
                                                int errptnum = 1;
                                                while (ErrPt[errptnum].Visible) {
                                                    errptnum++;
                                                    if (errptnum >= ErrPt.Length) {
                                                        // no more error points available
                                                        blnError = true;
                                                        break;
                                                    }
                                                }
                                                if (blnError) {
                                                    break;
                                                }
                                                Room[i].Exits[j].Transfer = -errptnum;
                                                ErrPt[errptnum].Visible = true;
                                                ErrPt[errptnum].Loc = new(Room[hideroom.Index].Loc.X + 0.1f, Room[hideroom.Index].Loc.Y + 0.2268f);
                                                ErrPt[errptnum].Order = ObjOrder.Count;
                                                ObjInfo obj = new(ELSelection.lsErrPt, errptnum);
                                                ObjOrder.Add(obj);
                                                ErrPt[errptnum].ExitID = Room[i].Exits[j].ID;
                                                ErrPt[errptnum].FromRoom = i;
                                                ErrPt[errptnum].Room = hideroom.Index;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // remove the room
                        Room[hideroom.Index].Visible = false;
                        Room[hideroom.Index].Loc = new();
                        Room[hideroom.Index].ShowPic = false;
                        // clear the exits for removed room
                        Room[hideroom.Index].Exits.Clear();
                        // remove from object list
                        CompactObjList(Room[hideroom.Index].Order);
                        // mark as changed to force update
                        MarkAsChanged();
                    }
                    strLine = ReadNextBlock(sr);
                }
                // need to reset all exit locations
                if (!LoadExits(strErrList)) {
                    blnWarning = true;
                }
            }
            catch (Exception ex) {
                return ExtractIfError();
            }
            // reset min/max
            RecalculateMaxMin();

            if (blnWarning) {
                // build the error file
                strErrList.Insert(0, "Layout Data File Error Report");
                strErrList.Insert(1, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                strErrList.Insert(2, "");

                try {
                    StreamWriter sw = new StreamWriter(EditGame.GameDir + "layout_errors.txt", false, Encoding.UTF8);
                    foreach (string line in strErrList) {
                        sw.WriteLine(line);
                    }
                    sw.Close();
                    // notify user that rebuild might be needed
                    strLine = "Errors were encountered in the layout file. WinAGI " +
                                "attempted to repair them.\nYou should make sure all rooms, " +
                                "exits and comments are correctly placed and  then save " +
                                "the layout. Consider using the 'Repair Layout' option if " +
                                "there are significant discrepancies.\n\nA list of specific " +
                                "issues encountered can be found in the 'layout_errors.txt' " +
                                "file in your game directory.";
                    MessageBox.Show(MDIMain,
                        strLine,
                        "Layout Editor Errors",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp,
                        "htm\\winagi\\Layout_Editor.htm#layoutrepair");
                    // mark as changed to force update
                    MarkAsChanged();
                }
                catch (Exception ex) {
                    // if error, just ignore it
                    strLine = "Errors were encountered in the layout file. WinAGI " +
                                "attempted to repair them.\nYou should make sure all rooms, " +
                                "exits and comments are correctly placed and  then save " +
                                "the layout. Consider using the 'Repair Layout' option if " +
                                "there are significant discrepancies.";
                    MessageBox.Show(MDIMain,
                        strLine,
                        "Layout Editor Errors",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, 0, 0,
                        WinAGIHelp,
                        "htm\\winagi\\Layout_Editor.htm#layoutrepair");
                }
            }
            return true;

            void LayoutUpdate(int index, AGIExit[] exits) {
                Room[index].ShowPic = WinAGISettings.LEShowPics.Value;
                // the update exits don't include transfer/errpt info- check
                // them against current exits
                foreach (var exit in exits) {
                    for (int i = 0; i < Room[index].Exits.Count; i++) {
                        // if exit already exists, update it
                        if (Room[index].Exits[i].Room == exit.Room &&
                            Room[index].Exits[i].Reason == exit.Reason &&
                            Room[index].Exits[i].Style == exit.Style) {
                            exit.Transfer = Room[index].Exits[i].Transfer;
                            exit.Leg = Room[index].Exits[i].Leg;
                            if (exit.Transfer > 0) {
                                TransPt[exit.Transfer].ExitID[exit.Leg] = exit.ID;
                            }
                            else if (exit.Transfer < 0) {
                                ErrPt[-exit.Transfer].ExitID = exit.ID;
                            }
                            Room[index].Exits.Remove(i);
                            break;
                        }
                        // check this new exit for exits to errpts
                        if (exit.Room == 0 || !EditGame.Logics.Contains(exit.Room)) {
                            int errptnum = 1;
                            while (ErrPt[errptnum].Visible) {
                                errptnum++;
                                if (errptnum >= ErrPt.Length) {
                                    // no more error points available
                                    blnError = true;
                                    break;
                                }
                            }
                            if (blnError) {
                                break;
                            }
                            exit.Room = errptnum;
                            ErrPt[errptnum].Visible = true;
                            ErrPt[errptnum].Loc = GetInsertPos(Room[index].Loc.X, Room[index].Loc.Y);
                            ObjInfo obj = new(ELSelection.lsErrPt, errptnum);
                            ErrPt[errptnum].Order = ObjOrder.Count;
                            ObjOrder.Add(obj);
                            ErrPt[errptnum].ExitID = exits[i].ID;
                            ErrPt[errptnum].Room = exit.Room;
                            ErrPt[errptnum].FromRoom = index;
                        }
                    }
                }
                // any existing exits left need to be checked for errpts
                // and transfers, which need to be removed
                for (int i = 0; i < Room[index].Exits.Count; i++) {
                    int transfer = Room[index].Exits[i].Transfer;
                    // only erpt and transfer points are affected
                    if (transfer > 0) {
                        // remove this leg of transpt
                        TransPt[transfer].Count--;
                        if (TransPt[transfer].Count == 0) {
                            // no more legs, delete the transfer point
                            TransPt[transfer].ExitID[0] = "";
                            TransPt[transfer].ExitID[1] = "";
                            TransPt[transfer].Room[0] = 0;
                            TransPt[transfer].Room[1] = 0;
                            CompactObjList(TransPt[transfer].Order);
                        }
                        else {
                            // determine which leg to clear
                            int leg = Room[index].Exits[i].Leg;
                            // clear this leg
                            TransPt[transfer].ExitID[leg] = "";
                            TransPt[transfer].Room[leg] = 0;
                        }
                    }
                    else if (transfer < 0) {
                        // remove errpt
                        CompactObjList(ErrPt[-transfer].Order);
                        ErrPt[-transfer].Visible = false;
                        ErrPt[-transfer].ExitID = "";
                        ErrPt[-transfer].FromRoom = 0;
                        ErrPt[-transfer].Room = 0;
                        ErrPt[-transfer].Loc = new();
                        ErrPt[-transfer].Order = 0;
                    }
                }
                // now add the new exits
                Room[index].Exits.CopyFrom(exits);
            }

            void DeleteLoadTransfer(int transfer) {
                // remove the transfer point from the list
                Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].Transfer = 0;
                Debug.Assert(Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].Leg == 0);
                if (TransPt[transfer].Count == 2) {
                    Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Transfer = 0;
                    Debug.Assert(Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Leg == 1);
                    Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Leg = 0;
                }
                TransPt[transfer].Count = 0;
                TransPt[transfer].ExitID[0] = "";
                TransPt[transfer].ExitID[1] = "";
                TransPt[transfer].Room[0] = 0;
                TransPt[transfer].Room[1] = 0;
                CompactObjList(TransPt[transfer].Order);
            }
        }

        private bool GetV2LayoutData(string layoutfile) {
            // This function is used to extract the layout data from a v2
            // layout file and populate the layout editor's data structures.

            // v12 File format:
            // Line 1: cstr(MajorVer) & cstr(MinorVer)
            // Line 2: ObjCount from last layout editor save
            // Line 3: DrawScale|DSF|Offset.X|Offset.Y
            // Line 4: Min.X|Min.Y|Max.X|Max.Y
            // Line 5: hScrollBar1.Min|hScrollBar1.Max|hScrollBar1.Value|vScrollBar1.Min|vScrollBar1.Max|vScrollBar1.Value
            // Line 6: hScrollBar1.SmallChange|hScrollBar1.LargeChange|vScrollBar1.SmallChange|vScrollBar1.LargeChange
            // subsequent lines are one of the following:
            //    R|##|v|o|p|x|y|ID:room:reason:style:xfer:leg|...
            //    T|##|v|o|x1|y1|x2|y2|c|spx|spy|epx|epy
            //    E|##|v|o|x|y|r|e|f
            //    C|##|v|o|x|y|h|w|{text}
            //    U|##|v|ID:room:reason:style|...
            //    N|##|##|--
            // R,T,E,C = line code indicating object Type
            // U means a room has been modified outside the layout editor, but not
            // yet saved by the editor save method
            // N means a room that has been renumbered since the last save
            // Pipe character (|) used to separate fields
            // ## is element number
            // v is visible property (only used by room object; all others are always True)
            // o is object display order
            // p is showpic status
            // x,y are layout coordinates for element
            // ID:room:reason:style:xfer:leg is exit info for rooms
            // h, w are width and height of comment object
            // {text} is text of comment/note

            // if errors are found, as long as position information is OK,
            // can still try drawing objects, rebuilding exits, then validating results
            // delete/hide anything that is not in use or error

            List<string> stlLayoutData = new();
            int lngNumber = 0, tmpOrder = 0;
            int lngCount = 0;
            bool tmpVisible = false;
            int lngFileCount = 0;
            List<string> strErrList = [];
            bool blnError = false, blnWarning = false;
            string strVer = "";
            // v2 doesn't have objects in order, so 
            // this array is used to add track their order
            ObjInfo[] objOrder = new ObjInfo[1024];
            int objCount = 0;

            // use deselect obj to configure toolbar,statusbar and menus
            DeselectObj();

            Selection.Type = ELSelection.lsNone;
            Selection.Number = 0;
            Selection.ExitID = "";
            Selection.Leg = ELLeg.llNoTrans;
            MoveExit = false;
            MoveObj = false;
            IsChanged = false;

            // file has already been verified to exist
            try {
                FileStream fs = new FileStream(layoutfile, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                while (!sr.EndOfStream) {
                    stlLayoutData.Add(sr.ReadLine());
                }
                sr.Dispose();
                fs.Dispose();
            }
            catch (Exception ex) {
                // invalid file, or other error
                return ExtractIfError();
            }
            // process file header first
            string strLine;
            int linenum, hdrIndex = 0;
            for (linenum = 0; linenum < stlLayoutData.Count; linenum++) {
                strLine = stlLayoutData[linenum].Trim();
                if (strLine.Length == 0) {
                    continue; // skip blank lines
                }
                switch (hdrIndex) {
                case 0:
                    // Line 1: MajorVer.ToString() + MinorVer.ToString()
                    strVer = strLine;
                    switch (strVer) {
                    case "21" or "22" or "23":
                        // mark as changed so it gets updated
                        MarkAsChanged();
                        break;
                    default:
                        // invalid version number
                        return ExtractIfError();
                    }
                    hdrIndex++;
                    break;
                case 1:
                    // Line 2: ObjCount
                    if (strLine.IsInt()) {
                        lngFileCount = objCount = int.Parse(strLine);
                    }
                    else {
                        // invalid data
                        return ExtractIfError();
                    }
                    hdrIndex++;
                    break;
                case 2:
                    // Line 3: DrawScale|DSF|Offset.X|Offset.Y
                    string[] strData = strLine.Split("|");
                    if (strData.Length != 4) {
                        // invalid data
                        return ExtractIfError();
                    }
                    DrawScale = strData[0].IntVal();
                    // validate it, just in case
                    if (DrawScale < 1) DrawScale = 1;
                    if (DrawScale > 9) DrawScale = 9;
                    // DSF is a calculated value; no need to store/retrieve it
                    //DSF = strData[1].Val();
                    DSF = (float)(40 * Math.Pow(1.25, DrawScale - 1));
                    Offset.X = strData[2].IntVal();
                    Offset.Y = strData[3].IntVal();
                    InitFonts(); // reset font size based on DSF
                    spScale.Text = "Scale: " + DrawScale;
                    hdrIndex++;
                    break;
                case 3:
                    // Line 4: Min.X|Min.Y|Max.X|Max.Y
                    strData = strLine.Split("|");
                    if (strData.Length != 4) {
                        // invalid data
                        return ExtractIfError();
                    }
                    Min.X = (float)strData[0].Val();
                    Min.Y = (float)strData[1].Val();
                    Max.X = (float)strData[2].Val();
                    Max.Y = (float)strData[3].Val();
                    hdrIndex++;
                    break;
                case 4:
                    // Line 5: hScrollBar1.Min|hScrollBar1.Max|hScrollBar1.Value|vScrollBar1.Min|vScrollBar1.Max|vScrollBar1.Value
                    strData = strLine.Split("|");
                    if (strData.Length != 6) {
                        // invalid data
                        return ExtractIfError();
                    }
                    // ignore - set these based on the form's parameters

                    //hScrollBar1.Minimum = strData[0].IntIntVal();
                    //hScrollBar1.Maximum = strData[1].IntVal();
                    //hScrollBar1.Value = strData[2].IntVal();
                    //vScrollBar1.Minimum = strData[3].IntVal();
                    //vScrollBar1.Maximum = strData[4].IntVal();
                    //vScrollBar1.Value = strData[5].IntVal();
                    hdrIndex++;
                    break;
                case 5:
                    // Line 6: hScrollBar1.SmallChange|hScrollBar1.LargeChange|vScrollBar1.SmallChange|vScrollBar1.LargeChange
                    strData = strLine.Split("|");
                    if (strData.Length != 4) {
                        // invalid data
                        return ExtractIfError();
                    }
                    // ignore - set these based on the form's parameters

                    //hScrollBar1.SmallChange = strData[0].IntVal();
                    //hScrollBar1.LargeChange = strData[1].IntVal();
                    //vScrollBar1.SmallChange = strData[2].IntVal();
                    //vScrollBar1.LargeChange = strData[3].IntVal();
                    // last line of header
                    hdrIndex++;
                    break;
                }
                if (hdrIndex > 5) {
                    // header complete; break out of loop
                    break;
                }
            }
            // confirm header is complete
            if (hdrIndex != 6) {
                return ExtractIfError();
            }
            // retrieve lines, one at a time (stop if error is found)
            for (linenum++; linenum < stlLayoutData.Count; linenum++) {
                strLine = stlLayoutData[linenum].Trim();
                // skip blank lines
                if (strLine.Length == 0) {
                    continue;
                }
                // split line
                string[] strData = strLine.Split("|");
                // validate basic structure of line:
                switch (strData[0].Length != 0 ? (int)strData[0][0] : 0) {
                case 67:
                    // C:  comment should have exactly 9 elements
                    if (strData.Length != 9) {
                        // log a warning
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid COMMENT entry");
                        // try next line
                        continue;
                    }
                    break;
                case 69:
                    // E: error point should have exactly 6 elements
                    if (strData.Length != 9) {
                        // log a warning
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid ERRPT entry");
                        // try next line
                        continue;
                    }
                    break;
                case 78:
                    // N:  renumber should be at exactly 3 elements
                    if (strData.Length != 3) {
                        // log a warning
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid RENUMBER entry");
                        // try next line
                        continue;
                    }
                    break;
                case 82:
                    // R: room should have at least 7 elements
                    if (strData.Length < 7) {
                        // log a warning
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid ROOM entry");
                        // try next line
                        continue;
                    }
                    break;
                case 84:
                    // T: should have exactly 13 elements
                    if (strData.Length != 13) {
                        // log a warning
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid TRANSPT entry");
                        //try next line
                        continue;
                    }
                    break;
                case 85:
                    // U:  update or renumber should be at least 3 elements
                    if (strData.Length < 3) {
                        // log a warning
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid UPDATE entry");
                        // try next line
                        continue;
                    }
                    break;
                default:
                    // not sure what this line is; ignore it and try to go on
                    blnWarning = true;
                    strErrList.Add("Line " + (linenum + 1) + ": line does not contain valid line marker");
                    // try next line
                    continue;
                }
                // get number
                lngNumber = strData[1].IntVal();
                // number should be valid; >=1 and <=255
                if (lngNumber == 0) {
                    blnWarning = true;
                    strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid object number ( < 1)");
                    // try next line
                    continue;
                }
                // number should be <=255
                if (lngNumber > 255) {
                    blnWarning = true;
                    strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid object number ( > 255)");
                    // try next line
                    continue;
                }
                // next item depends on line type:
                // all but N have visible property as third element
                if (strData[0][0] != 'N') {
                    // next number is visible property
                    if (bool.TryParse(strData[2], out tmpVisible) == false) {
                        // invalid visible property
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid visible property");
                        // try next line
                        continue;
                    }
                }
                // C,E,R,T have order value; N,U do NOT
                switch ((int)strData[0][0]) {
                case 67 or 69 or 82 or 84:
                    tmpOrder = strData[3].IntVal();
                    if (tmpOrder < 0) {
                        // if negative, then file is corrupt, and probably not recoverable
                        blnError = true;
                        break;
                    }
                    if (tmpOrder > 1023) {
                        // if too high, then file is corrupt, and probably not recoverable
                        blnError = true;
                        break;
                    }
                    if (tmpOrder >= objCount) {
                        // not right, but maybe we can recover?
                        objCount = tmpOrder + 1;
                        // warn user
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid object position number");
                    }
                    // check if this order already in use?
                    if (objOrder[tmpOrder].Type != ELSelection.lsNone) {
                        // problem - but maybe we can fix it by reassigning
                        // the object to next available number
                        tmpOrder = objCount;
                        objCount++;
                        lngCount++;
                        // warn user
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": invalid object position number");
                    }
                    break;
                }
                if (blnError) {
                    // if error, then we cannot recover
                    // so break out of loop
                    break;
                }
                // handle line data according to Type
                switch ((int)strData[0][0]) {
                case 67:
                    // "C" - comment info  'C |##|v|o|x|y|h|w|{text}
                    // set visibility property
                    Comment[lngNumber].Visible = tmpVisible;
                    // set obj order
                    objOrder[tmpOrder].Type = ELSelection.lsComment;
                    objOrder[tmpOrder].Number = lngNumber;
                    Comment[lngNumber].Order = tmpOrder;
                    // set coordinates
                    Comment[lngNumber].Loc = new((float)strData[4].Val(), (float)strData[5].Val());
                    // set height/width
                    Comment[lngNumber].Size = new((float)strData[6].Val(), (float)strData[7].Val());
                    Comment[lngNumber].Text = strData[8].Replace("&crlf", "\n");
                    lngCount++;
                    break;
                case 69:
                    // "E" - error info  'E |##|v|o|x|y|r|e|f
                    // set object order
                    objOrder[tmpOrder].Type = ELSelection.lsErrPt;
                    objOrder[tmpOrder].Number = lngNumber;
                    // set visibility property
                    ErrPt[lngNumber].Visible = tmpVisible;
                    ErrPt[lngNumber].Order = tmpOrder;
                    ErrPt[lngNumber].Loc = new((float)strData[4].Val(), (float)strData[5].Val());
                    ErrPt[lngNumber].Room = strData[6].IntVal();
                    ErrPt[lngNumber].ExitID = strData[7];
                    ErrPt[lngNumber].FromRoom = strData[8].IntVal();
                    lngCount++;
                    break;
                case 78:
                    // "N" - renumbering 'N |##|##|--
                    // validate second number
                    int lngRenumber = strData[2].IntVal();
                    if (lngRenumber < 1 || lngRenumber > 255) {
                        blnWarning = true;
                        strErrList.Add("Line " + (linenum + 1).ToString() + ": a Renumber action has invalid object numbers");
                        break; //skip to next line
                    }
                    // update objOrder entry to point to newroom
                    objOrder[Room[lngNumber].Order].Number = lngRenumber;
                    // move oldroom data to newroom
                    Room[lngRenumber].Loc = Room[lngNumber].Loc;
                    Room[lngRenumber].ShowPic = Room[lngNumber].ShowPic;
                    Room[lngRenumber].Visible = Room[lngNumber].Visible;
                    Room[lngRenumber].Exits.CopyFrom(Room[lngNumber].Exits);
                    Room[lngRenumber].Order = Room[lngNumber].Order;
                    // clear old room
                    Room[lngNumber].Loc = new();
                    Room[lngNumber].ShowPic = false;
                    Room[lngNumber].Visible = false;
                    Room[lngNumber].Exits.Clear();
                    Room[lngNumber].Order = 0;
                    for (int i = 1; i < 256; i++) {
                        // update room exits
                        for (int j = 0; j < Room[i].Exits.Count; j++) {
                            if (Room[i].Exits[j].Room == lngNumber) {
                                Room[i].Exits[j].Room = lngRenumber;
                            }
                        }
                        // update trans pts
                        if (TransPt[i].Count != 0) {
                            for (int j = 0; j < 2; j++) {
                                if (TransPt[i].Room[j] == lngNumber) {
                                    TransPt[i].Room[j] = (byte)lngRenumber;
                                }
                            }
                        }
                        // update errpts
                        if (ErrPt[i].Visible) {
                            if (ErrPt[i].FromRoom == lngNumber) {
                                ErrPt[i].FromRoom = lngRenumber;
                            }
                        }
                    }
                    // mark as changed so it gets updated
                    MarkAsChanged();
                    break;
                case 82:
                    // "R" - room info
                    // VER 12+: R|##|v|o|p|x|y|ID:room:reason:style:xfer:leg|...
                    objOrder[tmpOrder].Type = ELSelection.lsRoom;
                    objOrder[tmpOrder].Number = lngNumber;
                    Room[lngNumber].Order = tmpOrder;
                    Room[lngNumber].Visible = tmpVisible;

                    // get pic status and coords
                    bool tmpShowPic = true;
                    _ = bool.TryParse(strData[4], out tmpShowPic);
                    Room[lngNumber].ShowPic = tmpShowPic;
                    Room[lngNumber].Loc = new((float)strData[5].Val(), (float)strData[6].Val());
                    // get exit info
                    Room[lngNumber].Exits = ParseExits(strData);
                    lngCount++;
                    break;
                case 84:
                    // "T" - transfer info  'T |##|v|o|x1|y1|x2|y2|r1|r2|e1|e2|ctr
                    // visible flag is ignored for transfer points; counter determines visibility

                    // set display order
                    objOrder[tmpOrder].Type = ELSelection.lsTransPt;
                    objOrder[tmpOrder].Number = lngNumber;
                    TransPt[lngNumber].Order = tmpOrder;
                    // set first transfer coordinates
                    TransPt[lngNumber].Loc[0] = new((float)strData[4].Val(), (float)strData[5].Val());
                    // get next pair of coordinates
                    TransPt[lngNumber].Loc[1] = new((float)strData[6].Val(), (float)strData[7].Val());
                    // get rooms
                    TransPt[lngNumber].Room[0] = (byte)strData[8].IntVal();
                    TransPt[lngNumber].Room[1] = (byte)strData[9].IntVal();
                    // get exits
                    TransPt[lngNumber].ExitID[0] = strData[10];
                    TransPt[lngNumber].ExitID[1] = strData[11];
                    // get Count Value
                    TransPt[lngNumber].Count = strData[12].IntVal();
                    lngCount++;
                    break;
                case 85:
                    // "U" - updated room info
                    //   U|##|v|ID:room:reason:style|...
                    //   update doesn't provide objpos number or position coordinates
                    //   because EITHER the object already exists, and show/hide status
                    //   is changing,
                    //   OR it's a new room, added while layout was closed

                    //get exit info
                    AGIExits tmpExits = new();
                    for (int i = 3; i < strData.Length; i++) {
                        string[] strExit = strData[i].Split(":");
                        // should be four elements
                        if (strExit.Length != 4) {
                            blnWarning = true;
                            strErrList.Add("Line " + (linenum + 1).ToString() + ": Update line has invalid data");
                        }
                        else {
                            // add new exit, and flag as OK, in source
                            tmpExits.Add(strExit[0].IntVal(), strExit[1].IntVal(), (EEReason)strExit[2].IntVal(), strExit[3].IntVal(), 0, 0).Status = EEStatus.esOK;
                        }
                    }
                    if (tmpVisible) {
                        // update an existing room, or show a hidden room or
                        // add a new room
                        bool needpos = !Room[lngNumber].Visible;
                        Room[lngNumber].Visible = true;
                        Room[lngNumber].ShowPic = WinAGISettings.LEShowPics.Value;
                        // the update exits don't include transfer/errpt info- check
                        // them against current exits
                        foreach (var exit in tmpExits) {
                            for (int i = 0; i < Room[lngNumber].Exits.Count; i++) {
                                // if exit already exists, update it
                                if (Room[lngNumber].Exits[i].Room == exit.Room &&
                                    Room[lngNumber].Exits[i].Reason == exit.Reason &&
                                    Room[lngNumber].Exits[i].Style == exit.Style) {
                                    exit.Transfer = Room[lngNumber].Exits[i].Transfer;
                                    exit.Leg = Room[lngNumber].Exits[i].Leg;
                                    if (exit.Transfer > 0) {
                                        TransPt[exit.Transfer].ExitID[exit.Leg] = exit.ID;
                                    }
                                    else if (exit.Transfer < 0) {
                                        ErrPt[-exit.Transfer].ExitID = exit.ID;
                                    }
                                    Room[lngNumber].Exits.Remove(i);
                                    break;
                                }
                                // check this new exit for exits to errpts
                                if (exit.Room == 0 || !EditGame.Logics.Contains(exit.Room)) {
                                    int errptnum = 1;
                                    while (ErrPt[errptnum].Visible) {
                                        errptnum++;
                                        if (errptnum >= ErrPt.Length) {
                                            // no more error points available
                                            blnError = true;
                                            break;
                                        }
                                    }
                                    exit.Room = errptnum;
                                    ErrPt[errptnum].Visible = true;
                                    ErrPt[errptnum].Loc = GetInsertPos(Room[lngNumber].Loc.X, Room[lngNumber].Loc.Y);
                                    objOrder[objCount].Type = ELSelection.lsErrPt;
                                    objOrder[objCount].Number = errptnum;
                                    ErrPt[errptnum].Order = objCount++;
                                    ErrPt[errptnum].ExitID = tmpExits[i].ID;
                                    ErrPt[errptnum].Room = exit.Room;
                                    ErrPt[errptnum].FromRoom = lngNumber;
                                }
                            }
                        }
                        // any existing exits left need to be removed
                        for (int i = 0; i < Room[lngNumber].Exits.Count; i++) {
                            int transfer = Room[lngNumber].Exits[i].Transfer;
                            // only erpt and transfer points are affected
                            if (transfer > 0) {
                                // remove this leg of transpt
                                TransPt[transfer].Count--;
                                if (TransPt[transfer].Count == 0) {
                                    // if no more legs, delete the transfer point
                                    TransPt[transfer].ExitID[0] = "";
                                    TransPt[transfer].ExitID[1] = "";
                                    TransPt[transfer].Room[0] = 0;
                                    TransPt[transfer].Room[1] = 0;
                                    CompactLoadList(TransPt[transfer].Order);
                                }
                                else {
                                    // determine which leg to clear
                                    int leg = Room[lngNumber].Exits[i].Leg;
                                    // clear this leg
                                    TransPt[transfer].ExitID[leg] = "";
                                    TransPt[transfer].Room[leg] = 0;
                                }
                            }
                            else if (transfer < 0) {
                                // remove errpt
                                CompactLoadList(ErrPt[-transfer].Order);
                                ErrPt[-transfer].Visible = false;
                                ErrPt[-transfer].ExitID = "";
                                ErrPt[-transfer].FromRoom = 0;
                                ErrPt[-transfer].Room = 0;
                                ErrPt[-transfer].Loc = new();
                                ErrPt[-transfer].Order = 0;
                            }
                        }
                        // now add the new exits
                        Room[lngNumber].Exits.CopyFrom(tmpExits);

                        // a hidden/added room may be replacing an error point or need its
                        // hidden exits restored
                        if (needpos) {
                            // look for exits pointing to error points which match this
                            for (int i = 1; i < 256; i++) {
                                // if room is visible (but NOT the room being added)
                                if (Room[i].Visible && i != lngNumber) {
                                    if (Room[i].Exits.Count > 0) {
                                        for (int j = 0; j < Room[i].Exits.Count; j++) {
                                            // if exit points to this room
                                            if (Room[i].Exits[j].Room == lngNumber) {
                                                int transfer = Room[i].Exits[j].Transfer;
                                                // in case an err pt is found, deal with it
                                                if (transfer < 0) {
                                                    if (needpos) {
                                                        // move it here
                                                        Room[lngNumber].Loc.X = GridPos(ErrPt[-transfer].Loc.X - 0.1f);
                                                        Room[lngNumber].Loc.Y = GridPos(ErrPt[-transfer].Loc.Y - 0.1402f);
                                                        // only the first errpt is replaced by the new room
                                                        int order = ErrPt[-transfer].Order;
                                                        objOrder[order].Number = lngNumber;
                                                        objOrder[order].Type = ELSelection.lsRoom;
                                                        Room[lngNumber].Order = order;
                                                        needpos = false;
                                                    }
                                                    else {
                                                        // more than one matching errpt is just removed
                                                        CompactLoadList(ErrPt[-transfer].Order);
                                                    }
                                                    // hide errpt
                                                    ErrPt[-transfer].Visible = false;
                                                    ErrPt[-transfer].ExitID = "";
                                                    ErrPt[-transfer].FromRoom = 0;
                                                    ErrPt[-transfer].Room = 0;
                                                    ErrPt[-transfer].Order = 0;
                                                    // clear transfer marker
                                                    Room[i].Exits[j].Transfer = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // if location still needed (either updating an existing
                        // room, or showing a room that didn't replace an errpt)
                        if (needpos) {
                            Room[lngNumber].Loc = GetInsertPos(
                                (picDraw.ClientSize.Width / 2 - Offset.X) / DSF,
                                (picDraw.ClientSize.Width / 2 - Offset.Y) / DSF);
                            objOrder[objCount].Type = ELSelection.lsRoom;
                            objOrder[objCount].Number = lngNumber;
                            Room[lngNumber].Order = objCount;
                            objCount++;
                        }
                    }
                    else {
                        // hide the room
                        if (Room[lngNumber].Visible) {
                            // first, check for any existing transfers or errpts FROM the room being hidden
                            for (int i = 0; i < Room[lngNumber].Exits.Count; i++) {
                                int transfer = Room[lngNumber].Exits[i].Transfer;
                                if (transfer > 0) {
                                    // remove the transfer point
                                    DeleteLoadTransfer(transfer);
                                }
                                else if (transfer < 0) {
                                    // remove errpt
                                    CompactLoadList(ErrPt[-transfer].Order);
                                    ErrPt[-transfer].Visible = false;
                                    ErrPt[-transfer].ExitID = "";
                                    ErrPt[-transfer].FromRoom = 0;
                                    ErrPt[-transfer].Room = 0;
                                    ErrPt[-transfer].Loc = new();
                                    ErrPt[-transfer].Order = 0;
                                }
                            }
                            // step through all other exits
                            bool adderrpt = !EditGame.Logics.Contains(lngNumber);
                            for (int i = 1; i < 256; i++) {
                                // only need to check rooms that are currently visible
                                if (i != lngNumber && Room[i].Visible) {
                                    for (int j = 0; j < Room[i].Exits.Count; j++) {
                                        if (Room[i].Exits[j].Room == lngNumber && Room[i].Exits[j].Status != EEStatus.esDeleted) {
                                            // check for transfer pt
                                            if (Room[i].Exits[j].Transfer > 0) {
                                                // remove it
                                                DeleteLoadTransfer(Room[i].Exits[j].Transfer);
                                            }
                                            // add an ErrPt if room is not a valid logic
                                            if (adderrpt && Room[i].Exits[j].Transfer >= 0) {
                                                int errptnum = 1;
                                                while (ErrPt[errptnum].Visible) {
                                                    errptnum++;
                                                    if (errptnum >= ErrPt.Length) {
                                                        // no more error points available
                                                        blnError = true;
                                                        break;
                                                    }
                                                }
                                                Room[i].Exits[j].Transfer = -errptnum;
                                                ErrPt[errptnum].Visible = true;
                                                ErrPt[errptnum].Loc = GetInsertPos(Room[lngNumber].Loc.X, Room[lngNumber].Loc.Y);
                                                objOrder[objCount].Type = ELSelection.lsErrPt;
                                                objOrder[objCount].Number = errptnum;
                                                ErrPt[errptnum].Order = objCount++;
                                                ErrPt[errptnum].ExitID = Room[i].Exits[j].ID;
                                                ErrPt[errptnum].FromRoom = i;
                                                ErrPt[errptnum].Room = lngNumber;

                                            }
                                        }
                                    }
                                }
                            }
                            // remove the room
                            Room[lngNumber].Visible = false;
                            Room[lngNumber].Loc = new();
                            Room[lngNumber].ShowPic = false;
                            // clear the exits for removed room
                            Room[lngNumber].Exits.Clear();
                            // remove from object list
                            CompactLoadList(Room[lngNumber].Order);
                        }
                        else {
                            // if room is already hidden, then this is an error
                            // that should never happen
                            Debug.Assert(false);
                        }
                    }
                    // mark as changed to force update
                    MarkAsChanged();
                    break;

                    void CompactLoadList(int order) {
                        // compact the object list by removing the entry at the specified order
                        for (int i = order; i < objCount - 1; i++) {
                            objOrder[i] = objOrder[i + 1];
                            switch (objOrder[i].Type) {
                            case ELSelection.lsRoom:
                                Room[objOrder[i].Number].Order = i;
                                break;
                            case ELSelection.lsTransPt:
                                TransPt[objOrder[i].Number].Order = i;
                                break;
                            case ELSelection.lsErrPt:
                                ErrPt[objOrder[i].Number].Order = i;
                                break;
                            case ELSelection.lsComment:
                                Comment[objOrder[i].Number].Order = i;
                                break;
                            }
                        }
                        objCount--;
                    }
                    void DeleteLoadTransfer(int transfer) {
                        // remove the transfer point from the list
                        Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].Transfer = 0;
                        Debug.Assert(Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].Leg == 0);
                        if (TransPt[transfer].Count == 2) {
                            Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Transfer = 0;
                            Debug.Assert(Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Leg == 1);
                            Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Leg = 0;
                        }
                        TransPt[transfer].Count = 0;
                        TransPt[transfer].ExitID[0] = "";
                        TransPt[transfer].ExitID[1] = "";
                        TransPt[transfer].Room[0] = 0;
                        TransPt[transfer].Room[1] = 0;
                        CompactLoadList(TransPt[transfer].Order);
                    }
                }
            }

            // if error was encountered, file is no good
            if (blnError) {
                // notify user that layout needs rebuilding
                MessageBox.Show(MDIMain,
                    "Errors were encountered in the layout file that could not be fixed automatically.\n\nThe layout needs to be repaired.",
                    "Layout Editor Errors",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, 0, 0,
                    WinAGIHelp,
                    "htm\\winagi\\Layout_Editor.htm#layoutrepair");
                // since form isn't visible, user isn't given a chance to cancel the repair; it will happen automatically
                RepairLayout();
            }
            // add objects to order list (from the temporary list)
            for (int i = 0; i < objCount; i++) {
                ObjOrder.Add(objOrder[i]);
            }
            // need to reset all exit locations and validate hidden/errpt values
            if (!LoadExits(strErrList)) {
                blnWarning = true;
            }
            // if number of ojects loaded doesn't match the stored value
            // something might be off
            if (lngCount != lngFileCount) {
                // add a warning
                blnWarning = true;
                strErrList.Add("The number of objects placed did not equal the number stored in the layout file.");
            }
            if (blnWarning) {
                // build the error file
                strErrList.Add("\n\nLayout Data File contents:");
                stlLayoutData.Insert(0, "Layout Data File Error Report");
                stlLayoutData.Insert(1, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                stlLayoutData.Insert(2, "");
                stlLayoutData.InsertRange(3, strErrList);

                try {
                    StreamWriter sw = new StreamWriter(EditGame.GameDir + "layout_errors.txt", false, Encoding.UTF8);
                    foreach (string line in stlLayoutData) {
                        sw.WriteLine(line);
                    }
                    sw.Close();
                    // notify user that rebuild might be needed
                    strLine = "Errors were encountered in the layout file. WinAGI " +
                                "attempted to repair them.\nYou should make sure all rooms, " +
                                "exits and comments are correctly placed and  then save " +
                                "the layout. Consider using the 'Repair Layout' option if " +
                                "there are significant discrepancies.\n\nA list of specific " +
                                "issues encountered can be found in the 'layout_errors.txt' " +
                                "file in your game directory.";
                    MessageBox.Show(MDIMain,
                        strLine,
                        "Layout Editor Errors",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp,
                        "htm\\winagi\\Layout_Editor.htm#layoutrepair");
                    // mark as changed to force update
                    MarkAsChanged();
                }
                catch (Exception ex) {
                    // if error, just ignore it
                    strLine = "Errors were encountered in the layout file. WinAGI " +
                                "attempted to repair them.\nYou should make sure all rooms, " +
                                "exits and comments are correctly placed and  then save " +
                                "the layout. Consider using the 'Repair Layout' option if " +
                                "there are significant discrepancies.";
                    MessageBox.Show(MDIMain,
                        strLine,
                        "Layout Editor Errors",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, 0, 0,
                        WinAGIHelp,
                        "htm\\winagi\\Layout_Editor.htm#layoutrepair");
                }
            }
            return true;
        }

        private bool LoadExits(List<string> strErrList) {
            bool blnOK = true;
            for (int i = 1; i < 256; i++) {
                if (Room[i].Visible) {
                    // verify the room actually exists
                    if (EditGame.Logics.Contains(i)) {
                        // also make sure room really IS a room
                        if (EditGame.Logics[i].IsRoom) {
                            // single direction update only; since every room is called seperately
                            RepositionRoom(i, false, true);
                        }
                        else {
                            // some how a room is in the layout showing visible that isn't-
                            // hide it to make it go away
                            HideRoom((byte)i);
                        }
                    }
                    else {
                        // this shouldn't happen if the layout file stays properly synced
                        // but if it does happen, we need to hide it to make it go away
                        HideRoom((byte)i);
                    }
                    // make sure it's not in exact same position as another room
                    for (int j = i + 1; j < 256; j++) {
                        if (Room[j].Visible) {
                            PointF Rm1Loc = Room[j].Loc;
                            SizeF Rm1Size = new(RM_SIZE, RM_SIZE);
                            PointF Rm2Loc = Room[i].Loc;
                            SizeF Rm2Size = new(RM_SIZE, RM_SIZE);
                            if (IsBehind(Rm1Loc, Rm1Size, Rm2Loc, Rm2Size)) {
                                // move it just a little so some of it sticks out
                                Room[j].Loc = GetInsertPos(Rm1Loc.X, Rm1Loc.Y, 0, 0.1f, true);
                            }
                        }
                    }
                }
                else {
                    // if room is NOT visible, make sure it's NOT a room
                    if (EditGame.Logics.Contains(i)) {
                        if (EditGame.Logics[i].IsRoom) {
                            // BAD - force it to non-room
                            EditGame.Logics[i].IsRoom = false;
                            strErrList.Add("Logic " + i + " is mismarked as not visible.");
                            blnOK = false;
                        }
                    }
                }
            }
            SetScrollBars();
            return blnOK;
        }

        private bool ExtractIfError(int reason = 0) {
            string msg = "";
            switch (reason) {
            case 0:
                msg = "The layout data file for this game is invalid or corrupt.\n\nDo you want to extract it automatically?";
                break;
            case 1:
                msg = "The layout file is empty. \n\nDo you want to extract the layout automatically?";
                break;
            }

            if (MessageBox.Show(MDIMain,
                msg,
                "Layout Editor",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes) {
                return ExtractLayout();
            }
            return false;
        }

        private bool ExtractLayout(bool SavePos = false) {
            // if SavePos is true, then we keep the position information for
            // objects already on screen, and only update IsRoom status and exits

            // Variable declarations
            int i, j;
            EEReason k = EEReason.erNone;
            byte bytGrp = 0, bytRoom = 0;
            byte StackCount = 0;
            byte[] Stack = new byte[256];
            byte[] Queue = new byte[256];
            byte QCur = 0, QEnd = 0;
            int TPCount = 1, EPCount = 1;
            PointF tmpCoord;
            byte[] b = new byte[16];
            byte xPos = 0, yPos = 0;
            PointF[] GroupMax = new PointF[256];
            byte[] GroupCount = new byte[256];
            bool[] InGame = new bool[256];

            // Show progress form to provide feedback to user
            var progress = new frmProgress(MDIMain);
            progress.Text = SavePos ? "Repairing Layout" : "Extract Layout";
            progress.lblProgress.Text = "Analyzing ...";
            progress.pgbStatus.Maximum = EditGame.Logics.Count + 12;
            progress.pgbStatus.Value = 0;
            progress.Show();

            // Clear variables
            for (i = 1; i <= 255; i++) {
                ELRoom[i].Analyzed = false;
                ELRoom[i].Placed = false;
                ELRoom[i].Group = 0;
                for (j = 0; j <= 3; j++) {
                    ELRoom[i].Enter[j] = 0;
                    ELRoom[i].Exits[j] = 0;
                }
                Room[i].Visible = false;
                Room[i].Order = 0;
                Room[i].ShowPic = WinAGISettings.LEShowPics.Value;
                if (!SavePos) {
                    Room[i].Loc.X = 0;
                    Room[i].Loc.Y = 0;
                }
                Comment[i].Order = 0;
                TransPt[i].Count = 0;
                TransPt[i].Loc[0].X = 0;
                TransPt[i].Loc[0].Y = 0;
                TransPt[i].Loc[1].X = 0;
                TransPt[i].Loc[1].Y = 0;
                TransPt[i].Room[0] = 0;
                TransPt[i].Room[1] = 0;
                TransPt[i].ExitID[0] = "";
                TransPt[i].ExitID[1] = "";
                TransPt[i].Order = 0;
                ErrPt[i].Visible = false;
                ErrPt[i].FromRoom = 0;
                ErrPt[i].Room = 0;
                ErrPt[i].Order = 0;
                ErrPt[i].Loc.X = 0;
                ErrPt[i].Loc.Y = 0;
                ErrPt[i].ExitID = "";
                InGame[i] = false;
            }

            // Clear the object placement order list
            ObjOrder = [];

            // run through all game logics and identify edgecode exits and entrances
            foreach (Logic agl in EditGame.Logics) {
                progress.pgbStatus.Value++;
                progress.lblProgress.Text = $"Analyzing {agl.ID}...";
                progress.Refresh();

                if (agl.Number != 0) {
                    InGame[agl.Number] = true;
                    if (!SavePos)
                        agl.IsRoom = false;
                    Room[agl.Number].Exits.Clear();
                    Room[agl.Number].Exits = ExtractExits(agl);

                    if (Room[agl.Number].Exits.Count > 0) {
                        // if there are exits, then this is a room
                        agl.IsRoom = true;
                    }
                    else {
                        // if there are no exits, then this is not a room
                        // UNLESS it was previously made visible due to
                        // exits going TO it
                        if (Room[agl.Number].Visible) {
                            agl.IsRoom = true;
                        }
                    }
                    // if the room isn't yet made visible (and added to object list), do it now
                    if (agl.IsRoom && !Room[agl.Number].Visible) {
                        Room[agl.Number].Visible = true;
                        Room[agl.Number].Order = ObjOrder.Count;
                        ObjInfo obj = new(ELSelection.lsRoom, agl.Number);
                        ObjOrder.Add(obj);
                    }
                    // update the exit/entrance matrix for all exits
                    for (i = 0; i < Room[agl.Number].Exits.Count; i++) {
                        switch (Room[agl.Number].Exits[i].Reason) {
                        case EEReason.erHorizon:
                        case EEReason.erRight:
                        case EEReason.erBottom:
                        case EEReason.erLeft:
                            // exit reasons are '1' based; arrays are '0' based
                            int index = (int)Room[agl.Number].Exits[i].Reason - 1;
                            ELRoom[agl.Number].Exits[index] = (byte)Room[agl.Number].Exits[i].Room;
                            ELRoom[Room[agl.Number].Exits[i].Room].Enter[(index + 2) & 3] = agl.Number;
                            break;
                        default:
                            // 'other' exits don't need to be tracked for group positioning
                            break;
                        }
                        // if target room has a valid number and not yet made visible, do it now 
                        if (Room[agl.Number].Exits[i].Room > 0 && !Room[Room[agl.Number].Exits[i].Room].Visible) {
                            if (EditGame.Logics.Contains(Room[agl.Number].Exits[i].Room)) {
                                Room[Room[agl.Number].Exits[i].Room].Visible = true;
                                // also make it a room if it isn't already
                                EditGame.Logics[Room[agl.Number].Exits[i].Room].IsRoom = true;
                                MDIMain.RefreshPropertyGrid();
                                Room[Room[agl.Number].Exits[i].Room].Order = ObjOrder.Count;
                                ObjInfo obj = new(ELSelection.lsRoom, Room[agl.Number].Exits[i].Room);
                                ObjOrder.Add(obj);
                            }
                        }
                    }
                }
            }

            // Update progress
            progress.pgbStatus.Value += 2;
            progress.lblProgress.Text = "Creating room groups...";
            progress.Refresh();
            // refresh the resource tree just in case IsRoom status was changed
            // for the currently selected room
            MDIMain.RefreshPropertyGrid();
            // Step through all identified rooms to place each room in its group
            for (i = 1; i <= 255; i++) {
                if (!Room[i].Visible)
                    continue;
                if (ELRoom[i].Group != 0)
                    continue;
                bytGrp++;
                StackCount++;
                Stack[StackCount] = (byte)i;
                ELRoom[Stack[StackCount]].Analyzed = true;
                ELRoom[Stack[StackCount]].Group = bytGrp;
                GroupCount[bytGrp] = 1;

                while (StackCount != 0) {
                    bytRoom = Stack[StackCount];
                    StackCount--;

                    if (Room[bytRoom].Visible) {
                        // check exits
                        for (j = 0; j <= 3; j++) {
                            // if target room is valid and target does not yet have a group
                            if (ELRoom[bytRoom].Exits[j] > 0 && ELRoom[ELRoom[bytRoom].Exits[j]].Group == 0) {
                                ELRoom[ELRoom[bytRoom].Exits[j]].Group = bytGrp;
                                GroupCount[bytGrp]++;
                                StackCount++;
                                Stack[StackCount] = ELRoom[bytRoom].Exits[j];
                            }
                        }
                        // check entrances
                        for (j = 0; j <= 3; j++) {
                            // if from room is valid and from room does not yet have a group
                            if (ELRoom[bytRoom].Enter[j] > 0 && ELRoom[ELRoom[bytRoom].Enter[j]].Group == 0) {
                                ELRoom[ELRoom[bytRoom].Enter[j]].Group = bytGrp;
                                GroupCount[bytGrp]++;
                                StackCount++;
                                Stack[StackCount] = ELRoom[bytRoom].Enter[j];
                            }
                        }
                    }
                }
            }

            // if not rebuilding, set groups and determine positioning
            if (!SavePos) {
                progress.pgbStatus.Value += 2;
                progress.lblProgress.Text = "Arranging rooms in groups...";
                progress.Refresh();
                // step through groups to assign physical positions
                for (i = 1; i <= bytGrp; i++) {
                    if (GroupCount[i] > 1) {
                        // find first ELRoom in this group
                        for (j = 1; j <= 255; j++) {
                            if (ELRoom[j].Group == i && ELRoom[j].Analyzed) {
                                // unmark as analyzed so it gets added
                                ELRoom[j].Analyzed = false;
                                break;
                            }
                        }
                        // reset queue
                        QEnd = 0;
                        QCur = 0;
                        Queue[QEnd] = (byte)j;
                        QEnd++;
                        Min.X = 0; Min.Y = 0; Max.X = 0; Max.Y = 0;
                        // mark first room as placed
                        ELRoom[j].Placed = true;

                        // use queue to place all rooms in this group
                        while (QEnd != QCur) {
                            // if NOT already in use
                            if (!ELRoom[Queue[QCur]].Analyzed) {
                                // mark it as analyzed
                                ELRoom[Queue[QCur]].Analyzed = true;
                                // crawl in all four directions
                                for (int dir = 0; dir <= 3; dir++) {
                                    StackCount = 1;
                                    Stack[1] = Queue[QCur];
                                    while (StackCount != 0) {
                                        bytRoom = Stack[StackCount];
                                        StackCount--;
                                        // Exits
                                        if (ELRoom[bytRoom].Exits[dir] != 0 && !ELRoom[ELRoom[bytRoom].Exits[dir]].Placed) {
                                            // if space already occupied
                                            if (ItemAtPoint(Room[bytRoom].Loc.X + DeltaX(dir), Room[bytRoom].Loc.Y + DeltaY(dir), i)) {
                                                // try next space clockwise
                                                if (!ItemAtPoint(Room[bytRoom].Loc.X + DeltaX(dir) + DeltaX(dir + 1), Room[bytRoom].Loc.Y + DeltaY(dir) + DeltaY(dir + 1), i)) {
                                                    Room[ELRoom[bytRoom].Exits[dir]].Loc.X = Room[bytRoom].Loc.X + DeltaX(dir) + DeltaX(dir + 1);
                                                    Room[ELRoom[bytRoom].Exits[dir]].Loc.Y = Room[bytRoom].Loc.Y + DeltaY(dir) + DeltaY(dir + 1);
                                                    // try next space counterclockwise
                                                }
                                                else if (!ItemAtPoint(Room[bytRoom].Loc.X + DeltaX(dir) + DeltaX(dir + 3), Room[bytRoom].Loc.Y + DeltaY(dir) + DeltaY(dir + 3), i)) {
                                                    Room[ELRoom[bytRoom].Exits[dir]].Loc.X = Room[bytRoom].Loc.X + DeltaX(dir) + DeltaX(dir + 3);
                                                    Room[ELRoom[bytRoom].Exits[dir]].Loc.Y = Room[bytRoom].Loc.Y + DeltaY(dir) + DeltaY(dir + 3);
                                                }
                                                else {
                                                    // locate free point around target
                                                    tmpCoord = GetInsertPos(Room[bytRoom].Loc.X + DeltaX(dir), Room[bytRoom].Loc.Y + DeltaY(dir), i);
                                                    Room[ELRoom[bytRoom].Exits[dir]].Loc.X = tmpCoord.X;
                                                    Room[ELRoom[bytRoom].Exits[dir]].Loc.Y = tmpCoord.Y;
                                                }
                                            }
                                            else {
                                                Room[ELRoom[bytRoom].Exits[dir]].Loc.X = Room[bytRoom].Loc.X + DeltaX(dir);
                                                Room[ELRoom[bytRoom].Exits[dir]].Loc.Y = Room[bytRoom].Loc.Y + DeltaY(dir);
                                            }

                                            // set the group, mark it and add to stack
                                            ELRoom[ELRoom[bytRoom].Exits[dir]].Group = (byte)i;
                                            ELRoom[ELRoom[bytRoom].Exits[dir]].Placed = true;
                                            StackCount++;
                                            Stack[StackCount] = ELRoom[bytRoom].Exits[dir];
                                            // if not already analyzed
                                            if (!ELRoom[ELRoom[bytRoom].Exits[dir]].Analyzed) {
                                                // add to queue as well
                                                Queue[QEnd] = ELRoom[bytRoom].Exits[dir];
                                                QEnd++;
                                            }
                                            // update min and Max
                                            UpdateMinMax(Room[ELRoom[bytRoom].Exits[dir]].Loc);
                                        }
                                        // Entrances
                                        if (ELRoom[bytRoom].Enter[dir] != 0 && !ELRoom[ELRoom[bytRoom].Enter[dir]].Placed) {
                                            // if space already occupied
                                            if (ItemAtPoint(Room[bytRoom].Loc.X + DeltaX(dir), Room[bytRoom].Loc.Y + DeltaY(dir), i)) {
                                                // try next space clockwise
                                                if (!ItemAtPoint(Room[bytRoom].Loc.X + DeltaX(dir) + DeltaX(dir + 1), Room[bytRoom].Loc.Y + DeltaY(dir) + DeltaY(dir + 1), i)) {
                                                    Room[ELRoom[bytRoom].Enter[dir]].Loc.X = Room[bytRoom].Loc.X + DeltaX(dir) + DeltaX(dir + 1);
                                                    Room[ELRoom[bytRoom].Enter[dir]].Loc.Y = Room[bytRoom].Loc.Y + DeltaY(dir) + DeltaY(dir + 1);
                                                    // try next space counterclockwise
                                                }
                                                else if (!ItemAtPoint(Room[bytRoom].Loc.X + DeltaX(dir) + DeltaX(dir + 3), Room[bytRoom].Loc.Y + DeltaY(dir) + DeltaY(dir + 3), i)) {
                                                    Room[ELRoom[bytRoom].Enter[dir]].Loc.X = Room[bytRoom].Loc.X + DeltaX(dir) + DeltaX(dir + 3);
                                                    Room[ELRoom[bytRoom].Enter[dir]].Loc.Y = Room[bytRoom].Loc.Y + DeltaY(dir) + DeltaY(dir + 3);
                                                }
                                                else {
                                                    // locate free point around target
                                                    tmpCoord = GetInsertPos(Room[bytRoom].Loc.X + DeltaX(dir), Room[bytRoom].Loc.Y + DeltaY(dir), i);
                                                    Room[ELRoom[bytRoom].Enter[dir]].Loc.X = tmpCoord.X;
                                                    Room[ELRoom[bytRoom].Enter[dir]].Loc.Y = tmpCoord.Y;
                                                }
                                            }
                                            else {
                                                Room[ELRoom[bytRoom].Enter[dir]].Loc.X = Room[bytRoom].Loc.X + DeltaX(dir);
                                                Room[ELRoom[bytRoom].Enter[dir]].Loc.Y = Room[bytRoom].Loc.Y + DeltaY(dir);
                                            }
                                            // set the group, mark it and add to stack
                                            ELRoom[ELRoom[bytRoom].Enter[dir]].Group = (byte)i;
                                            ELRoom[ELRoom[bytRoom].Enter[dir]].Placed = true;
                                            StackCount++;
                                            Stack[StackCount] = ELRoom[bytRoom].Enter[dir];
                                            // if not already analyzed
                                            if (!ELRoom[ELRoom[bytRoom].Enter[dir]].Analyzed) {
                                                // add to queue as well
                                                Queue[QEnd] = ELRoom[bytRoom].Enter[dir];
                                                QEnd++;
                                            }
                                            // update min and Max
                                            UpdateMinMax(Room[ELRoom[bytRoom].Enter[dir]].Loc);
                                        }
                                        // Other exits (only once though???)
                                        //if (!ELRoom[bytRoom].Analyzed) {
                                        for (int idx = 0; idx < Room[bytRoom].Exits.Count; idx++) {
                                            if (Room[bytRoom].Exits[idx].Reason == EEReason.erOther) {
                                                // if other exit is a single room and room is not zero
                                                if (GroupCount[ELRoom[Room[bytRoom].Exits[idx].Room].Group] == 1 && Room[bytRoom].Exits[idx].Room != 0) {
                                                    // insert the room
                                                    tmpCoord = GetInsertPos(Room[EditGame.Logics[bytRoom].Number].Loc.X, Room[EditGame.Logics[bytRoom].Number].Loc.Y, i);
                                                    Room[Room[bytRoom].Exits[idx].Room].Loc = tmpCoord;
                                                    UpdateMinMax(Room[Room[bytRoom].Exits[idx].Room].Loc);
                                                    ELRoom[Room[bytRoom].Exits[idx].Room].Placed = true;
                                                    // move to this group
                                                    GroupCount[ELRoom[Room[bytRoom].Exits[idx].Room].Group] = 0;
                                                    ELRoom[Room[bytRoom].Exits[idx].Room].Group = (byte)i;
                                                    GroupCount[i]++;
                                                }
                                            }
                                        }
                                        //}
                                    }
                                }
                            }
                            QCur++;
                        }
                        // Reset positions based on min x/y, clear placed flag, adjust GroupMax
                        for (j = 1; j <= 255; j++) {
                            if (ELRoom[j].Group == i) {
                                Room[j].Loc.X -= Min.X;
                                Room[j].Loc.Y -= Min.Y;
                                ELRoom[j].Placed = false;
                            }
                        }
                        GroupMax[i].X = Max.X - Min.X;
                        GroupMax[i].Y = Max.Y - Min.Y;
                    }
                }
                void UpdateMinMax(PointF loc) {
                    if (loc.X < Min.X)
                        Min.X = loc.X;
                    if (loc.Y < Min.Y)
                        Min.Y = loc.Y;
                    if (loc.X > Max.X)
                        Max.X = loc.X;
                    if (loc.Y > Max.Y)
                        Max.Y = loc.Y;
                }

                // Position groups on surface to maximize use of space
                progress.pgbStatus.Value += 2;
                progress.lblProgress.Text = "Positioning room groups on layout...";
                progress.Refresh();

                // now position groups on surface to maximize use of space
                // put group one in default position (upper left)
                // (only worry about first 16 columns)
                for (i = 0; i <= GroupMax[1].X + 1 && i < 16; i++)
                    b[i] = (byte)(GroupMax[1].Y + 4);
                // step through rest of groups
                for (i = 2; i <= bytGrp; i++) {
                    if (GroupCount[i] > 0) {
                        // reset starting pos to bottom left
                        xPos = 0; yPos = 255; j = 0;
                        // determine where to put this group by 'sliding' it
                        // along the bottom of currently placed groups (marked by the array B())
                        // this is basically like Tetris, trying to find where the current
                        // group can fit with the lowest Y Value, taking into account its width
                        do {
                            // start at top
                            Max.Y = 0;
                            // get Max Y for width of this group at position j
                            for (int k1 = 0; k1 <= GroupMax[i].X + 2 && j + k1 < 16; k1++) {
                                if (b[j + k1] > Max.Y)
                                    Max.Y = b[j + k1];
                            }
                            if (Max.Y < yPos) {
                                yPos = (byte)Max.Y;
                                xPos = (byte)j;
                            }
                            // increment starting column, and get next minimum
                            j++;
                        } while (j <= 16 - (GroupMax[i].X + 2));
                        // adjust bottom for newly added group
                        for (int k1 = 0; k1 <= GroupMax[i].X + 2 && xPos + k1 < 16; k1++)
                            b[xPos + k1] = (byte)(Max.Y + GroupMax[i].Y + 4);
                        // adjust group offset
                        for (j = 1; j <= 255; j++) {
                            if (ELRoom[j].Group == i) {
                                Room[j].Loc.X += xPos;
                                Room[j].Loc.Y += yPos;
                            }
                        }
                    }
                }
            }

            progress.pgbStatus.Value += 2;
            progress.lblProgress.Text = "Checking for circular references...";
            progress.Refresh();

            // Add transfer points and error points as appropriate
            TPCount = 1; EPCount = 1;
            for (i = 1; i <= 255; i++) {
                if (Room[i].Visible) {
                    for (j = 0; j < Room[i].Exits.Count; j++) {
                        int targetRoom = Room[i].Exits[j].Room;
                        if (targetRoom == 0 || !InGame[Room[i].Exits[j].Room]) {
                            // insert an error point
                            ErrPt[EPCount].Visible = true;
                            ErrPt[EPCount].FromRoom = i;
                            ErrPt[EPCount].Room = targetRoom;
                            ErrPt[EPCount].ExitID = Room[i].Exits[j].ID;
                            ErrPt[EPCount].Order = ObjOrder.Count;
                            // increment object counter
                            ObjInfo obj = new(ELSelection.lsErrPt, EPCount);
                            ObjOrder.Add(obj);
                            // mark exit
                            Room[i].Exits[j].Transfer = -EPCount;
                            tmpCoord = new();
                            switch (Room[i].Exits[j].Reason) {
                            case EEReason.erNone:
                            case EEReason.erOther:
                                // position first around from room
                                tmpCoord = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y, 0, 1);
                                // if valid spot not found (coords didnt change)
                                if (tmpCoord.X == Room[i].Loc.X) {
                                    // move it directly above
                                    tmpCoord.Y = Room[i].Loc.Y - 1.5f;
                                }
                                break;
                            case EEReason.erHorizon:
                                // position around point above
                                tmpCoord = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y - 1.5f);
                                break;
                            case EEReason.erBottom:
                                // position around point below
                                tmpCoord = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y + 1.5f);
                                break;
                            case EEReason.erLeft:
                                // position around point to left
                                tmpCoord = GetInsertPos(Room[i].Loc.X - 1.5f, Room[i].Loc.Y);
                                break;
                            case EEReason.erRight:
                                // position around point to right
                                tmpCoord = GetInsertPos(Room[i].Loc.X + 1.5f, Room[i].Loc.Y);
                                break;
                            }
                            // adjust to account for size/shape of the errpt
                            tmpCoord.X += 0.1f;
                            tmpCoord.Y += 0.1402f;
                            ErrPt[EPCount].Loc = tmpCoord;
                            // increment errpt counter
                            EPCount++;
                        }
                        else {
                            // if target room is visible
                            if (Room[targetRoom].Visible) {
                                switch (Room[i].Exits[j].Reason) {
                                case EEReason.erHorizon:
                                    // if target room is below this room, or on same level
                                    // (NOTE: this will also catch rooms that loop on themselves)
                                    if (Room[i].Loc.Y <= Room[targetRoom].Loc.Y) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, EEReason.erBottom);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Transfer = tp;
                                            Room[i].Exits[j].Leg = 1;
                                            // mark as containing two segments
                                            TransPt[tp].Count = 2;
                                            TransPt[tp].ExitID[1] = Room[i].Exits[j].ID;
                                        }
                                        else {
                                            // create new transfer point
                                            TransPt[TPCount].Count = 1;
                                            TransPt[TPCount].Room[0] = (byte)i;
                                            TransPt[TPCount].Room[1] = (byte)targetRoom;
                                            TransPt[TPCount].ExitID[0] = Room[i].Exits[j].ID;
                                            Room[i].Exits[j].Transfer = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            // add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(ELSelection.lsTransPt, TPCount);
                                            ObjOrder.Add(obj);
                                            // adjust to account for size/shape of the transpt
                                            TransPt[TPCount].Loc[0] = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y - 1.5f, 0, 0.5f);
                                            TransPt[TPCount].Loc[1] = GetInsertPos(Room[targetRoom].Loc.X, Room[targetRoom].Loc.Y + 1.5f, 0, 0.5f);
                                            // increment transfer counter
                                            TPCount++;
                                        }
                                    }
                                    break;
                                case EEReason.erRight:
                                    // if target room is to left of this room, or on same level
                                    // (NOTE: this will also catch rooms that loop on themselves)
                                    if (Room[i].Loc.X >= Room[targetRoom].Loc.X) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, EEReason.erLeft);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Transfer = tp;
                                            Room[i].Exits[j].Leg = 1;
                                            // mark as having two segments
                                            TransPt[tp].Count = 2;
                                            TransPt[tp].ExitID[1] = Room[i].Exits[j].ID;
                                        }
                                        else {
                                            // create new transfer point
                                            TransPt[TPCount].Count = 1;
                                            TransPt[TPCount].Room[0] = (byte)i;
                                            TransPt[TPCount].Room[1] = (byte)targetRoom;
                                            TransPt[TPCount].ExitID[0] = Room[i].Exits[j].ID;
                                            Room[i].Exits[j].Transfer = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            //add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(ELSelection.lsTransPt, TPCount);
                                            ObjOrder.Add(obj);
                                            TransPt[TPCount].Loc[0] = GetInsertPos(Room[i].Loc.X + 1.5f, Room[i].Loc.Y, 0, 0.5f);
                                            TransPt[TPCount].Loc[1] = GetInsertPos(Room[targetRoom].Loc.X - 1.5f, Room[targetRoom].Loc.Y, 0, 0.5f);
                                            // increment transfer counter
                                            TPCount++;
                                        }
                                    }
                                    break;
                                case EEReason.erBottom:
                                    // if target room is above this room, or on same level
                                    // (NOTE: this will also catch rooms that loop on themselves)
                                    if (Room[i].Loc.Y >= Room[targetRoom].Loc.Y) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, EEReason.erHorizon);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Transfer = tp;
                                            Room[i].Exits[j].Leg = 1;
                                            // mark as having two segments
                                            TransPt[tp].Count = 2;
                                            TransPt[tp].ExitID[1] = Room[i].Exits[j].ID;
                                        }
                                        else {
                                            // create new transfer point
                                            TransPt[TPCount].Count = 1;
                                            TransPt[TPCount].Room[0] = (byte)i;
                                            TransPt[TPCount].Room[1] = (byte)targetRoom;
                                            TransPt[TPCount].ExitID[0] = Room[i].Exits[j].ID;
                                            Room[i].Exits[j].Transfer = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            // add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(ELSelection.lsTransPt, TPCount);
                                            ObjOrder.Add(obj);
                                            TransPt[TPCount].Loc[0] = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y + 1.5f, 0, 0.5f);
                                            TransPt[TPCount].Loc[1] = GetInsertPos(Room[targetRoom].Loc.X, Room[targetRoom].Loc.Y - 1.5f, 0, 0.5f);
                                            // increment transfer counter
                                            TPCount++;
                                        }
                                    }
                                    break;
                                case EEReason.erLeft:
                                    // if target room is to right of this room, or on same level
                                    // (NOTE: this will also catch rooms that loop on themselves)
                                    if (Room[i].Loc.X <= Room[targetRoom].Loc.X) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, EEReason.erRight);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Transfer = tp;
                                            Room[i].Exits[j].Leg = 1;
                                            // mark has having two segments
                                            TransPt[tp].Count = 2;
                                            TransPt[tp].ExitID[1] = Room[i].Exits[j].ID;
                                        }
                                        else {
                                            // create new transfer point
                                            TransPt[TPCount].Count = 1;
                                            TransPt[TPCount].Room[0] = (byte)i;
                                            TransPt[TPCount].Room[1] = (byte)targetRoom;
                                            TransPt[TPCount].ExitID[0] = Room[i].Exits[j].ID;
                                            Room[i].Exits[j].Transfer = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            // add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(ELSelection.lsTransPt, TPCount);
                                            ObjOrder.Add(obj);
                                            TransPt[TPCount].Loc[0] = GetInsertPos(Room[i].Loc.X - 1.5f, Room[i].Loc.Y, 0, 0.5f);
                                            TransPt[TPCount].Loc[1] = GetInsertPos(Room[targetRoom].Loc.X + 1.5f, Room[targetRoom].Loc.Y, 0, 0.5f);
                                            // increment transfer counter
                                            TPCount++;
                                        }
                                    }
                                    break;
                                case EEReason.erOther:
                                    // if more than 6 blocks away, AND if in another group OR
                                    // if exit loops back to this room
                                    if ((Math.Abs(Room[i].Loc.X - Room[targetRoom].Loc.X) + Math.Abs(Room[i].Loc.Y - Room[targetRoom].Loc.Y) > 6 &&
                                        ELRoom[i].Group != ELRoom[targetRoom].Group) || Room[i].Exits[j].Room == i) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, EEReason.erOther);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Transfer = tp;
                                            Room[i].Exits[j].Leg = 1;
                                            //mark has having two segments
                                            TransPt[tp].Count = 2;
                                            TransPt[tp].ExitID[1] = Room[i].Exits[j].ID;
                                        }
                                        else {
                                            // create new transfer point
                                            TransPt[TPCount].Count = 1;
                                            TransPt[TPCount].Room[0] = (byte)i;
                                            TransPt[TPCount].Room[1] = (byte)targetRoom;
                                            TransPt[TPCount].ExitID[0] = Room[i].Exits[j].ID;
                                            Room[i].Exits[j].Transfer = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            // add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(ELSelection.lsTransPt, TPCount);
                                            ObjOrder.Add(obj);

                                            // position first around from room
                                            tmpCoord = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y, 0, 1);
                                            // if valid spot not found (coords didn't change)
                                            if (tmpCoord.X == Room[i].Loc.X && tmpCoord.Y == Room[i].Loc.Y) {
                                                // move it directly above
                                                tmpCoord.Y = Room[i].Loc.Y - 2;
                                            }
                                            TransPt[TPCount].Loc[0] = tmpCoord;
                                            // position second around target room
                                            tmpCoord = GetInsertPos(Room[targetRoom].Loc.X, Room[targetRoom].Loc.Y, 0, 1);
                                            // if valid spot not found (coords didn't change)
                                            if (tmpCoord.X == Room[targetRoom].Loc.X && tmpCoord.Y == Room[targetRoom].Loc.Y) {
                                                // move it directly below
                                                tmpCoord.Y = Room[i].Loc.Y + 2;
                                            }
                                            TransPt[TPCount].Loc[1] = tmpCoord;
                                            TPCount++;
                                        }
                                    }
                                    break;
                                }
                            }
                            else {
                                // if the target room is not visible, mark it as hidden?
                                Room[i].Exits[j].Hidden = true;
                            }
                        }
                    }
                }
            }

            // For rebuilding, there may be comments
            if (SavePos) {
                for (i = 1; i <= 255; i++) {
                    if (Comment[i].Visible) {
                        Comment[i].Order = ObjOrder.Count;
                        ObjInfo obj = new(ELSelection.lsComment, i);
                        ObjOrder.Add(obj);
                    }
                }
            }

            // Align to grid, and adjust size if extracting
            for (i = 1; i <= 255; i++) {
                if (Room[i].Visible) {
                    Room[i].Loc.X = GridPos(Room[i].Loc.X);
                    Room[i].Loc.Y = GridPos(Room[i].Loc.Y);
                }
                if (TransPt[i].Count > 0) {
                    TransPt[i].Loc[0].X = GridPos(TransPt[i].Loc[0].X) + RM_SIZE / 4;
                    TransPt[i].Loc[0].Y = GridPos(TransPt[i].Loc[0].Y) + RM_SIZE / 4;
                    TransPt[i].Loc[1].X = GridPos(TransPt[i].Loc[1].X) + RM_SIZE / 4;
                    TransPt[i].Loc[1].Y = GridPos(TransPt[i].Loc[1].Y) + RM_SIZE / 4;
                }
                if (ErrPt[i].Visible) {
                    ErrPt[i].Loc.X = GridPos(ErrPt[i].Loc.X) + 0.1f;
                    ErrPt[i].Loc.Y = GridPos(ErrPt[i].Loc.Y) + 0.1402f;
                }
                if (SavePos && Comment[i].Visible) {
                    Comment[i].Loc.X = GridPos(Comment[i].Loc.X);
                    Comment[i].Loc.Y = GridPos(Comment[i].Loc.Y);
                }
            }
            // set default scale
            DrawScale = WinAGISettings.LEScale.Value;
            DSF = (float)(40 * Math.Pow(1.25, DrawScale - 1));
            InitFonts();
            // Set Max/min values, and reset scrollbars
            RecalculateMaxMin();
            // default to upper left corner
            Offset.X = -(int)(Min.X * DSF - MGN);
            Offset.Y = -(int)(Min.Y * DSF - MGN);
            SetScrollBars();

            // Update progress
            progress.pgbStatus.Value += 2;
            progress.lblProgress.Text = "Calculating exit line end points...";
            progress.Refresh();

            // set up exit starting and ending points
            for (i = 255; i >= 1; i--) {
                if (Room[i].Visible) {
                    RepositionRoom(i, false, true);
                }
            }

            // Force save the extracted logic
            SaveLayout();

            // Unload progress form
            progress.Close();

            return true;
        }

        private float DeltaX(int Dir) {
            // only uses bits 1 and 2; rest of number is ignored
            return 2 * ((1 - (Dir & 2)) * (Dir & 1));
        }

        private float DeltaY(int Dir) {
            // only uses bits 1 and 2; rest of number is ignored
            return 2 * ((1 - (Dir & 2)) * ((Dir & 1) - 1));
        }

        private int GetTransfer(byte FromRoom, byte ToRoom, EEReason Dir) {
            // returns the transfer number between these two rooms, if there is one
            // used by the extractlayout function when building the initial layout

            if (ToRoom == 0) {
                // if to room is undefined, cant be a transfer
                return -1;
            }

            // try all exits in 'to' room
            for (int i = 0; i < Room[ToRoom].Exits.Count; i++) {
                if (Room[ToRoom].Exits[i].Reason == Dir && Room[ToRoom].Exits[i].Room == FromRoom) {
                    // if this exit has a transfer, it can only be valid
                    // if it is NOT already two-way (count=2) AND this proposal is
                    // a RECIPROCAL exit...
                    int lngTrans = Room[ToRoom].Exits[i].Transfer;
                    if (lngTrans > 0) {
                        if (TransPt[lngTrans].Count == 1) {
                            return lngTrans;
                        }
                    }
                }
            }
            // no match found
            return -1;
        }

        public void UpdateLayout(UpdateReason Reason, int LogicNumber, AGIExits NewExits, int NewNum = 0) {
            // updates a room that was modified outside of the layout editor
            // so the layout editor room info matches the external room info
            // 
            // rooms can be added to the game, removed from the game, hidden, shown, or edited
            // to force this update
            // 
            // this method is also called from the loadlayout method if an update line was
            // added to the file due to one of the changes listed above

            // to keep status of transpts accurate, the list of current exits
            // is checked for any transpts; if found, the new list is checked
            // to see if an existing exit exactly matches; if so, the new exit
            // is assigned to the transpt; if no match is found, the transpt
            // is no longer in use; its Count is decremented (and the transpt
            // is deleted if Count goes to zero)

            // if there is a selection
            if (Selection.Type != ELSelection.lsNone) {
                DeselectObj();
            }
            // TODO: changing an id should just redraw the layout

            switch (Reason) {
            case UpdateReason.RenumberRoom:
                // this method adjusts the room number of room (called when a logic number is changed)

                // first copy old room info to new room
                Room[NewNum].Loc = Room[LogicNumber].Loc;
                Room[NewNum].ShowPic = Room[LogicNumber].ShowPic;
                Room[NewNum].Visible = Room[LogicNumber].Visible;
                Room[NewNum].Exits.CopyFrom(Room[LogicNumber].Exits, true);
                Room[NewNum].Order = Room[LogicNumber].Order;
                // clear old room
                Room[LogicNumber].Loc = new();
                Room[LogicNumber].ShowPic = false;
                Room[LogicNumber].Visible = false;
                Room[LogicNumber].Exits.Clear();
                Room[LogicNumber].Order = 0;
                for (int i = 1; i < 256; i++) {
                    // update room exits
                    for (int j = 0; j < Room[i].Exits.Count; j++) {
                        if (Room[i].Exits[j].Room == LogicNumber) {
                            Room[i].Exits[j].Room = NewNum;
                        }
                    }
                    // update trans pts
                    if (TransPt[i].Count != 0) {
                        for (int j = 0; j < 2; j++) {
                            if (TransPt[i].Room[j] == LogicNumber) {
                                TransPt[i].Room[j] = (byte)NewNum;
                                //TransPt[i].SP = 
                            }
                        }
                    }
                    // update errpts
                    if (ErrPt[i].Visible) {
                        if (ErrPt[i].FromRoom == LogicNumber) {
                            ErrPt[i].FromRoom = NewNum;
                        }
                        if (ErrPt[i].Room == LogicNumber) {
                            ErrPt[i].Room = NewNum;
                        }
                    }
                }
                // change number in the object list
                int idx = ObjOrder.FindIndex(obj => obj.Type == ELSelection.lsRoom && obj.Number == LogicNumber);
                ObjOrder[idx] = new(ELSelection.lsRoom, NewNum);
                break;
            case UpdateReason.UpdateRoom:
            case UpdateReason.ShowRoom:
                // updateroom will always be to a visible room
                // showroom will always be not visible until added-
                // regardless of reason, if room is not visible, show it
                if (Room[LogicNumber].Visible) {
                    // no need to get exits; they are already passed as NewExits
                    DisplayRoom(LogicNumber, Reason == UpdateReason.ShowRoom, true);
                }
                // if updating an existing room that has exits already set,
                // need to determine if any of them are exact matches
                // step through existing exits and look for transfer points
                for (int i = 0; i < Room[LogicNumber].Exits.Count; i++) {
                    if (Room[LogicNumber].Exits[i].Transfer != 0) {
                        // does the update have a matching exit?
                        for (int j = 0; j < NewExits.Count; j++) {
                            if (Room[LogicNumber].Exits[i].Reason == NewExits[j].Reason &&
                                Room[LogicNumber].Exits[i].Room == NewExits[j].Room &&
                                Room[LogicNumber].Exits[i].ID == NewExits[j].ID) {
                                // use it in new exit
                                NewExits[j].Transfer = Room[LogicNumber].Exits[i].Transfer;
                                NewExits[j].Leg = Room[LogicNumber].Exits[i].Leg;
                                // clear it from old exits
                                Room[LogicNumber].Exits[i].Transfer = 0;
                                break;
                            }
                        }
                    }
                }

                // run through existing exits again, and delete them
                // DeleteExit method uses a selection object, and handles transfers
                // set the selection object properties to
                // match a single direction exit from the room being updated
                TSel tmpSel = new();
                tmpSel.Type = ELSelection.lsExit;
                tmpSel.Number = LogicNumber;
                tmpSel.TwoWay = ELTwoWay.ltwOneWay;

                // step through all current exits and delete them
                for (int i = Room[LogicNumber].Exits.Count - 1; i >= 0; i--) {
                    // set id so correct exit is deleted
                    tmpSel.ExitID = Room[LogicNumber].Exits[i].ID;
                    // and delete it
                    DeleteExit(tmpSel);
                }
                // now add new exits
                // first, clear out old exits
                // (the delete exit function doesn't always actually delete;
                // in some cases, it just marks them as 'deletable'
                // so we need to use the Clear method to make them actually go away
                Room[LogicNumber].Exits.Clear();
                //  add new exits
                for (int i = 0; i < NewExits.Count; i++) {
                    bool AddErrPt = false;
                    // check for err point first (if exit room=0, Room(NewExits(i).Room).Visible
                    // will always be false so this line captures error points regardless if it
                    // is because room=0 or room is not visible)
                    if (!Room[NewExits[i].Room].Visible) {
                        // show err pt if room is not valid (room=0, or logic doesn't exist)
                        if (NewExits[i].Room == 0 || !EditGame.Logics.Contains(NewExits[i].Room)) {
                            // if err pt not yet added,
                            if (NewExits[i].Transfer == 0) {
                                // add an errpt
                                AddErrPt = true;
                            }
                        }
                        else {
                            // it's just hidden
                            NewExits[i].Hidden = true;
                        }
                    }
                    else {
                        // tgt room is visible
                        // if this exit does not have a transfer
                        if (NewExits[i].Transfer == 0) {
                            // check for an existing transfer from the target room
                            EEReason Dir;
                            switch (NewExits[i].Reason) {
                            case EEReason.erHorizon:
                                Dir = EEReason.erBottom;
                                break;
                            case EEReason.erRight:
                                Dir = EEReason.erLeft;
                                break;
                            case EEReason.erBottom:
                                Dir = EEReason.erHorizon;
                                break;
                            case EEReason.erLeft:
                                Dir = EEReason.erRight;
                                break;
                            case EEReason.erOther:
                                Dir = EEReason.erOther;
                                break;
                            default:
                                // should never occur, but just in case
                                Dir = EEReason.erNone;
                                break;
                            }
                            // if NOT err or unknown AND target room is not 0
                            if (Dir != EEReason.erNone) {
                                // if this exit is reciprocal:
                                // CANT use IsTwoWay function because exit is not added yet
                                for (int j = 0; j < Room[NewExits[i].Room].Exits.Count; j++) {
                                    // if this exit goes back to original room AND is not deleted
                                    if (Room[NewExits[i].Room].Exits[j].Room == LogicNumber &&
                                        Room[NewExits[i].Room].Exits[j].Status != EEStatus.esDeleted) {
                                        // if reason and transfer match
                                        if (Room[NewExits[i].Room].Exits[j].Reason == Dir) {
                                            // if transfer exists, and is not already in use two ways
                                            if (Room[NewExits[i].Room].Exits[j].Transfer != 0) {
                                                if (TransPt[Room[NewExits[i].Room].Exits[j].Transfer].Count != 2) {
                                                    // use this transfer
                                                    NewExits[i].Transfer = Room[NewExits[i].Room].Exits[j].Transfer;
                                                    NewExits[i].Leg = 1;
                                                    TransPt[NewExits[i].Transfer].Count = 2;
                                                    TransPt[NewExits[i].Transfer].ExitID[1] = Room[NewExits[i].Room].Exits[j].ID;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    // now add it
                    Room[LogicNumber].Exits.Add(NewExits[i].ID[2..].IntVal(), NewExits[i].Room, NewExits[i].Reason, NewExits[i].Style, NewExits[i].Transfer, NewExits[i].Leg).Status = NewExits[i].Status;
                    if (AddErrPt) {
                        InsertErrPt(LogicNumber, i, NewExits[i].Room);
                        AddErrPt = false;
                    }
                }
                RepositionRoom(LogicNumber);
                break;
            case UpdateReason.HideRoom:
                // in case the room is not actually visible, don't call the
                // hide room function; it will delete what it thinks is
                // the current room, but instead it'll delete something else
                if (Room[LogicNumber].Visible) {
                    // hide the room
                    HideRoom(LogicNumber);
                }
                break;
            }
            MarkAsChanged();
        }

        public void UpdatePictureStatus(int LogicNumber, bool ShowPic) {
            // updates the picture status of a room

            if (ShowPic) {
                if (Room[LogicNumber].Visible && WinAGISettings.LEShowPics.Value) {
                    Room[LogicNumber].ShowPic = true;
                    // if room is on screen, redraw it
                    DrawLayout(ELSelection.lsRoom, LogicNumber);
                }
            }
            else {
                if (Room[LogicNumber].Visible && Room[LogicNumber].ShowPic) {
                    Room[LogicNumber].ShowPic = false;
                    // if room is on screen, redraw it
                    DrawLayout(ELSelection.lsRoom, LogicNumber);
                }
            }
        }

        private void RepairLayout() {
            // repairs the layout by recalculating all exits;
            // room and comment positions don't get affected;
            // transfers and errpts are recalculated

            // if this is a forced rebuild (from the LoadLayout method)
            // the form isn't visible yet, so we don't ask the user; we just do it
            if (Visible) {
                if (MessageBox.Show(MDIMain,
                    "This will reestablish all the exit information as it currently " +
                    "exists in game logics. For best results, all logic editors should " +
                    "be closed.\n\nDo you want to continue?",
                    "Repair Layout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) {
                    return;
                }
            }
            ExtractLayout(true);
            DrawLayout();
        }

        private void UpdateLogicCode(Logic ThisLogic) {
            /*
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
            For i = Room(ThisLogic.Number).Exits.Count - 1 To 0 Step -1
            'reset ok flag
            blnExitOK = False

            'use a loop structure to handle the exit;
            'loop is exited after the exit is successfully updated
            'in the logic by verifying it is correct, changing it,
            'deleting it or adding it as a new exit

            Do
                Select Case Room(ThisLogic.Number).Exits(i).Status
                Case esNew
                    'look for an existing exit block matching this exit reason?
                    'no, this is probably really tough, and not worth the hassle

                    'find return cmd
                    lngPos1 = InStrRev(strSource, "return();", -1, vbTextCompare)

                    'if not found
                    If lngPos1 = 0 Then
                    'add to end
                    strSource = strSource & NewExitText(Room(ThisLogic.Number).Exits(i), strCR)
                    Else
                    'now move to beginning of line
                    lngPos2 = InStrRev(strSource, strCR, lngPos1)
                    'if 'return()' is on first line,
                    If lngPos2 = 0 Then
                        'add to beginning
                        strSource = NewExitText(Room(ThisLogic.Number).Exits(i), strCR) & strSource
                    Else
                        'lngPos2 is where new exit info will be added
                        strSource = Left$(strSource, lngPos2) & NewExitText(Room(ThisLogic.Number).Exits(i), strCR) & Right$(strSource, Len(strSource) - lngPos2 - Len(strCR) + 1)
                    End If
                    End If

                    'if second time through, OR no logic window open
                    If j = 1 Or Index = 0 Then
                    'reset exit status to ok since it now editor, file and logic source are all insync
                    Room(ThisLogic.Number).Exits(i).Status = esOK
                    End If

                    'exit successfully added; exit the do loop and get next exit
                    Exit Do

                Case esOK, esHidden
                    'ok; need to verify that it has not changed
                    lngPos2 = InStr(1, strSource, "##" & Room(ThisLogic.Number).Exits(i).ID & "##")

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
                        If tmpExit.Reason = Room(ThisLogic.Number).Exits(i).Reason And tmpExit.Room = Room(ThisLogic.Number).Exits(i).Room Then
                            'exit is ok
                            'make sure exit style match the logic
                            Room(ThisLogic.Number).Exits(i).Style = tmpExit.Style
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
                    Room(ThisLogic.Number).Exits(i).Status = esNew
                    End If

                Case esChanged
                    'find exit in text, and change it
                    lngPos2 = InStr(1, strSource, "##" & Room(ThisLogic.Number).Exits(i).ID & "##")

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
                        If tmpExit.Reason = Room(ThisLogic.Number).Exits(i).Reason Then
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
                            Room(ThisLogic.Number).Exits(i).Style = tmpExit.Style
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
                        Room(ThisLogic.Number).Exits(i).Status = esOK
                    End If

                    Exit Do
                    Else
                    'something was wrong with this exit; ignore
                    'the exit with the error, and add this exit
                    'as a new exit
                    Room(ThisLogic.Number).Exits(i).Status = esNew
                    End If

                Case esDeleted
                    'find exit in source and delete/comment it out;

                    lngPos2 = InStr(1, strSource, "##" & Room(ThisLogic.Number).Exits(i).ID & "##")

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
                    Room(ThisLogic.Number).Exits.Remove i
                    End If

                    'exit successfully deleted; exit do loop and get next exit
                    Exit Do

                End Select
            'only way to exit loop is to successfully update the exit info
            Loop While True
            Next i

            'if currently updating the file
            If j = 0 Then
            'save source
            ThisLogic.SourceText = strSource
            ThisLogic.SaveSource
            'mark as changed
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
        */
        }

        public void DrawLayout(ELSelection objType, int objNumber) {
            // draws the layout if the selection type and number
            // are on screen
            if (ObjOnScreen(objType, objNumber)) {
                // if object is on screen, redraw it
                DrawLayout();
            }
        }
        public void DrawLayout() {
            // in some cases when disposing the form, the resize event
            // will try to draw the layout- we need to check for that
            if (Disposing) {
                return;
            }

            float x1, y1;
            float h, w;
            float tX, tY;
            float linewidth = 2f;

            int bWidth = picDraw.Width, bHeight = picDraw.Height;
            picDraw.Image = new Bitmap(bWidth, bHeight);
            using Graphics g = Graphics.FromImage(picDraw.Image);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.White);

            if (WinAGISettings.LEShowGrid.Value) {
                using Pen dotPen = new(Color.LightGray);
                // minor gridlines
                if (WinAGISettings.LEGridMinor.Value != WinAGISettings.LEGridMajor.Value) {
                    float minorgrid = (float)WinAGISettings.LEGridMinor.Value * DSF;
                    dotPen.DashStyle = DashStyle.Custom;
                    dotPen.DashPattern = [1, 4];
                    // get position of first vertical line that would occur after
                    // current offset position
                    x1 = Offset.X - (int)(Offset.X / minorgrid) * minorgrid;
                    // add vertical lines, until past right edge of drawing surface
                    while (x1 <= bWidth) {
                        g.DrawLine(dotPen, x1, 0, x1, bHeight);
                        x1 += minorgrid;
                    }
                    y1 = Offset.Y - (int)(Offset.Y / minorgrid) * minorgrid;
                    // add horizontal lines until past bottom edge of drawing surface
                    while (y1 <= bHeight) {
                        g.DrawLine(dotPen, 0, y1, bWidth, y1);
                        y1 += minorgrid;
                    }
                }
                // major gridlines
                float majorgrid = (float)WinAGISettings.LEGridMajor.Value * DSF;
                dotPen.DashStyle = DashStyle.DashDot;
                x1 = Offset.X - (int)(Offset.X / majorgrid) * majorgrid;
                while (x1 <= bWidth) {
                    g.DrawLine(dotPen, x1, 0, x1, bHeight);
                    x1 += majorgrid;
                }
                y1 = Offset.Y - (int)(Offset.Y / majorgrid) * majorgrid;
                while (y1 <= bHeight) {
                    g.DrawLine(dotPen, 0, y1, bWidth, y1);
                    y1 += majorgrid;
                }
            }

            using SolidBrush objBrush = new(Color.White);
            using SolidBrush textBrush = new(Color.White);
            //if (DrawScale < 3) {
            //    linewidth = 1;
            //}
            //else if (DrawScale < 5) {
            //    linewidth = 2;
            //}
            //else {
            //    linewidth = 3;
            //}
            using Pen objPen = new(Color.White, linewidth);
            // next add objects
            for (int i = 0; i < ObjOrder.Count; i++) {
                switch (ObjOrder[i].Type) {
                case ELSelection.lsRoom:
                    int lngRoom = ObjOrder[i].Number;
                    // in unlikely event the object is not visible, just skip it
                    if (Room[lngRoom].Visible) {
                        string strID;
                        // if object is on screen
                        if (ObjOnScreen(ObjOrder[i])) {
                            // draw the box
                            if (EditGame.Logics[lngRoom].IsRoom) {
                                objBrush.Color = WinAGISettings.RoomFillColor.Value;
                                textBrush.Color = objPen.Color = WinAGISettings.RoomEdgeColor.Value;
                                objPen.DashStyle = DashStyle.Solid;
                            }
                            else {
                                // currently, non-rooms are never displayed
                                objBrush.Color = Color.LightGray;
                                textBrush.Color = objPen.Color = Color.Gray;
                                objPen.DashStyle = DashStyle.Dash;
                            }
                            x1 = (float)(Room[lngRoom].Loc.X * DSF + Offset.X);
                            y1 = (float)(Room[lngRoom].Loc.Y * DSF + Offset.Y);
                            w = RM_SIZE * DSF;
                            h = RM_SIZE * DSF;
                            g.FillRectangle(objBrush, x1, y1, w, h);
                            g.DrawRectangle(objPen, x1, y1, w, h);
                            // if showing pic for this room, draw the matching vis pic
                            if (Room[lngRoom].ShowPic) {
                                DrawRoomPic(g, lngRoom);
                            }
                            strID = ResourceName(EditGame.Logics[lngRoom], true, true);
                            SizeF textSize = g.MeasureString(strID, layoutFont);
                            if (textSize.Width <= RM_SIZE * DSF) {
                                tX = (Room[lngRoom].Loc.X + RM_SIZE / 2) * DSF + Offset.X - textSize.Width / 2;
                                tY = (Room[lngRoom].Loc.Y + 0.7f) * DSF + Offset.Y - 3 * textSize.Height / 2;
                                g.DrawString(strID, layoutFont, textBrush, tX, tY);
                            }
                            else {
                                // if logic id is too long, it won't fit in the box;
                                // split across two lines, and truncate if necessary
                                List<string> lines = WordWrapLines(strID, layoutFont, RM_SIZE * DSF, g, []);
                                textSize = g.MeasureString(lines[0], layoutFont);
                                tX = (Room[lngRoom].Loc.X + RM_SIZE / 2) * DSF + Offset.X - textSize.Width / 2;
                                tY = (Room[lngRoom].Loc.Y + 0.7f) * DSF + Offset.Y - 3 * textSize.Height / 2;
                                g.DrawString(lines[0], layoutFont, textBrush, tX, tY);
                                tY += textSize.Height; // move down for second line
                                g.DrawString(lines[1], layoutFont, textBrush, tX, tY);
                            }
                        }

                        // draw exits
                        for (int j = 0; j < Room[lngRoom].Exits.Count; j++) {
                            // skip any deleted exits
                            if (Room[lngRoom].Exits[j].Status != EEStatus.esDeleted) {
                                // determine color
                                if (Room[lngRoom].Exits[j].Hidden) {
                                    // use a dashed, gray line
                                    objPen.Color = Color.FromArgb(160, 160, 160);
                                    objPen.DashStyle = DashStyle.Dash;
                                }
                                else {
                                    objPen.DashStyle = DashStyle.Solid;
                                    switch (Room[lngRoom].Exits[j].Reason) {
                                    case EEReason.erOther:
                                        objPen.Color = WinAGISettings.ExitOtherColor.Value;
                                        break;
                                    default:
                                        objPen.Color = WinAGISettings.ExitEdgeColor.Value;
                                        break;
                                    }
                                }
                                // if there is a transfer pt
                                int transpt = Room[lngRoom].Exits[j].Transfer;
                                if (transpt > 0) {
                                    switch (TransPt[transpt].Count) {
                                    case 1:
                                        // is this first leg?
                                        if (Room[lngRoom].Exits[j].Leg == 0) {
                                            //continue;
                                            // arrow on end
                                            objPen.StartCap = LineCap.Flat;
                                            objPen.CustomEndCap = new AdjustableArrowCap(2f * DSF / 40, 4f * DSF / 40, true);
                                        }
                                        else {
                                            // arrow on start
                                            objPen.EndCap = LineCap.Flat;
                                            objPen.CustomStartCap = new AdjustableArrowCap(2f * DSF / 40, 4f * DSF / 40, true);
                                        }
                                        break;
                                    case 2:
                                        // only draw first leg
                                        if (Room[lngRoom].Exits[j].Leg == 1) {
                                            continue;
                                        }
                                        // arrow on both ends
                                        objPen.CustomStartCap = objPen.CustomEndCap = new AdjustableArrowCap(2f * DSF / 40, 4f * DSF / 40, true);
                                        break;
                                    }
                                    // draw first segment
                                    if (LineOnScreen(Room[lngRoom].Exits[j].SP.X,
                                            Room[lngRoom].Exits[j].SP.Y,
                                            TransPt[transpt].SP.X,
                                            TransPt[transpt].SP.Y)) {
                                        g.DrawLine(objPen,
                                            Room[lngRoom].Exits[j].SP.X * DSF + Offset.X,
                                            Room[lngRoom].Exits[j].SP.Y * DSF + Offset.Y,
                                            TransPt[transpt].SP.X * DSF + Offset.X,
                                            TransPt[transpt].SP.Y * DSF + Offset.Y);
                                    }
                                    // draw second segment
                                    if (LineOnScreen(TransPt[transpt].EP.X,
                                            TransPt[transpt].EP.Y,
                                            Room[lngRoom].Exits[j].EP.X,
                                            Room[lngRoom].Exits[j].EP.Y)) {


                                        g.DrawLine(objPen,
                                            TransPt[transpt].EP.X * DSF + Offset.X,
                                            TransPt[transpt].EP.Y * DSF + Offset.Y,
                                            Room[lngRoom].Exits[j].EP.X * DSF + Offset.X,
                                            Room[lngRoom].Exits[j].EP.Y * DSF + Offset.Y);
                                    }
                                }
                                else {
                                    // normal lines and err pts use endcap only
                                    objPen.StartCap = LineCap.Flat;
                                    objPen.CustomEndCap = new AdjustableArrowCap(2f * DSF / 40, 4f * DSF / 40, true);
                                    if (!Room[lngRoom].Exits[j].Hidden ||
                                        WinAGISettings.LEShowHidden.Value) {
                                        // draw the exit line
                                        if (LineOnScreen(Room[lngRoom].Exits[j].SP.X,
                                                Room[lngRoom].Exits[j].SP.Y,
                                                Room[lngRoom].Exits[j].EP.X,
                                                Room[lngRoom].Exits[j].EP.Y)) {
                                            g.DrawLine(objPen,
                                                Room[lngRoom].Exits[j].SP.X * DSF + Offset.X,
                                                Room[lngRoom].Exits[j].SP.Y * DSF + Offset.Y,
                                                Room[lngRoom].Exits[j].EP.X * DSF + Offset.X,
                                                Room[lngRoom].Exits[j].EP.Y * DSF + Offset.Y);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ELSelection.lsTransPt:
                    if (TransPt[ObjOrder[i].Number].Count > 0) {
                        string id = ObjOrder[i].Number.ToString();
                        SizeF textSize = g.MeasureString(id, transptFont);
                        w = RM_SIZE / 2 * DSF;
                        h = RM_SIZE / 2 * DSF;
                        if (ObjOnScreen(ObjOrder[i])) {
                            objBrush.Color = WinAGISettings.TransPtFillColor.Value;
                            textBrush.Color = objPen.Color = WinAGISettings.TransPtEdgeColor.Value;
                            objPen.DashStyle = DashStyle.Solid;
                            // draw transfer circle
                            x1 = TransPt[ObjOrder[i].Number].Loc[0].X * DSF + Offset.X;
                            y1 = TransPt[ObjOrder[i].Number].Loc[0].Y * DSF + Offset.Y;
                            g.FillEllipse(objBrush, x1, y1, w, h);
                            g.DrawEllipse(objPen, x1, y1, w, h);
                            // add number
                            tX = (TransPt[ObjOrder[i].Number].Loc[0].X + RM_SIZE / 4) * DSF + Offset.X - textSize.Width / 2 + 1;
                            tY = (TransPt[ObjOrder[i].Number].Loc[0].Y + RM_SIZE / 4) * DSF + Offset.Y - textSize.Height / 2 + 1;
                            g.DrawString(id, transptFont, textBrush, tX, tY);
                        }
                        if (ObjOnScreen(ObjOrder[i], true)) {
                            objBrush.Color = WinAGISettings.TransPtFillColor.Value;
                            textBrush.Color = objPen.Color = WinAGISettings.TransPtEdgeColor.Value;
                            objPen.DashStyle = DashStyle.Solid;
                            // draw transfer circle
                            x1 = TransPt[ObjOrder[i].Number].Loc[1].X * DSF + Offset.X;
                            y1 = TransPt[ObjOrder[i].Number].Loc[1].Y * DSF + Offset.Y;
                            w = RM_SIZE / 2 * DSF;
                            h = RM_SIZE / 2 * DSF;
                            g.FillEllipse(objBrush, x1, y1, w, h);
                            g.DrawEllipse(objPen, x1, y1, w, h);
                            // add number
                            tX = (TransPt[ObjOrder[i].Number].Loc[1].X + RM_SIZE / 4) * DSF + Offset.X - textSize.Width / 2 + 1;
                            tY = (TransPt[ObjOrder[i].Number].Loc[1].Y + RM_SIZE / 4) * DSF + Offset.Y - textSize.Height / 2 + 1;
                            g.DrawString(id, transptFont, textBrush, tX, tY);
                        }
                    }
                    break;
                case ELSelection.lsErrPt:
                    if (ErrPt[ObjOrder[i].Number].Visible) {
                        if (ObjOnScreen(ObjOrder[i])) {
                            objBrush.Color = WinAGISettings.ErrPtFillColor.Value;
                            textBrush.Color = objPen.Color = WinAGISettings.ErrPtEdgeColor.Value;
                            PointF[] errPoints =
                            [
                                new PointF((ErrPt[ObjOrder[i].Number].Loc.X) * DSF + Offset.X,
                                ErrPt[ObjOrder[i].Number].Loc.Y * DSF + Offset.Y),
                                new PointF((ErrPt[ObjOrder[i].Number].Loc.X + 0.6f) * DSF + Offset.X,
                                ErrPt[ObjOrder[i].Number].Loc.Y * DSF + Offset.Y),
                                new PointF((ErrPt[ObjOrder[i].Number].Loc.X + 0.3f) * DSF + Offset.X,
                                (ErrPt[ObjOrder[i].Number].Loc.Y + 0.5196f) * DSF + Offset.Y)
                            ];
                            using GraphicsPath path = new GraphicsPath();
                            float radius = 5.0f * DSF / 40; // radius for rounded corners
                                                            // Create rounded triangle path
                            path.AddArc(errPoints[0].X + radius * 0.366f, errPoints[0].Y, radius, radius, 150, 120);
                            path.AddArc(errPoints[1].X - radius * 1.366f, errPoints[1].Y, radius, radius, 270, 120);
                            path.AddArc(errPoints[2].X - radius * 0.5f, errPoints[2].Y - radius * 1.5f, radius, radius, 30, 120);
                            path.CloseFigure();
                            g.FillPath(objBrush, path);
                            // Draw the border
                            g.DrawPath(objPen, path);
                            // add number
                            SizeF textSize = g.MeasureString("ERR", transptFont);
                            tX = (ErrPt[ObjOrder[i].Number].Loc.X + 0.3f) * DSF + Offset.X - textSize.Width / 2;
                            tY = (ErrPt[ObjOrder[i].Number].Loc.Y + 0.15f) * DSF + Offset.Y - textSize.Height / 2;
                            g.DrawString("ERR", transptFont, textBrush, tX, tY);
                        }
                    }
                    break;
                case ELSelection.lsComment:
                    if (Comment[ObjOrder[i].Number].Visible) {
                        if (ObjOnScreen(ObjOrder[i])) {
                            // draw comment box
                            DrawCmtBox(g, ObjOrder[i].Number);
                        }
                    }
                    break;
                }
            }

            picDraw.Refresh();
            g.Dispose();
        }

        private void DrawCmtBox(Graphics g, int CmtID) {
            // draws a rounded corner rectangle; converts layout coordinates into drawing surface pixel coordinates

            // if comment not visible
            if (!Comment[CmtID].Visible) {
                return;
            }

            using Pen borderPen = new Pen(WinAGISettings.CmtEdgeColor.Value, 2);
            float radius = 5.0f * DSF / 40; // radius for rounded corners
            RectangleF rect = new(
            (Comment[CmtID].Loc.X) * DSF + Offset.X - 1,
            (Comment[CmtID].Loc.Y) * DSF + Offset.Y - 1,
            Comment[CmtID].Size.Width * DSF + 2,
            Comment[CmtID].Size.Height * DSF + 2);

            using (GraphicsPath path = new GraphicsPath()) {
                // Create rounded rectangle path
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.CloseFigure();
                // Fill the rounded rectangle
                using (Brush fillBrush = new SolidBrush(WinAGISettings.CmtFillColor.Value)) {
                    g.FillPath(fillBrush, path);
                }
                // Draw the border
                g.DrawPath(borderPen, path);
            }


            if (Comment[CmtID].Text.Trim().Length == 0) {
                return;
            }
            using SolidBrush fontBrush = new SolidBrush(WinAGISettings.CmtEdgeColor.Value);
            // wrap text to fit
            List<string> wrappedLines = WordWrapLines(Comment[CmtID].Text, layoutFont, (int)((Comment[CmtID].Size.Width - 0.06f) * DSF), g);
            // size of one line
            float textHeight = layoutFont.GetHeight(g);
            float tY = (Comment[CmtID].Loc.Y) * DSF + Offset.Y + 4;
            // now copy lines onto drawing surface
            foreach (string line in wrappedLines) {
                // if not enough room vertically (meaning text would extend below
                // bottom edge of comment box)
                if (tY > (Comment[CmtID].Loc.Y + Comment[CmtID].Size.Height) * DSF + Offset.Y - 4) {
                    break;
                }
                float tX = (Comment[CmtID].Loc.X + 0.03f) * DSF + Offset.X;
                g.DrawString(line, layoutFont, fontBrush, tX, tY);
                tY += textHeight;
            }
        }

        private bool ObjOnScreen(ELSelection objtype, int objnum, bool SecondTrans = false) {

            RectangleF objRect = new();
            switch (objtype) {
            case ELSelection.lsRoom:
                objRect = new(new(Room[objnum].Loc.X * DSF + Offset.X,
                    Room[objnum].Loc.Y * DSF + Offset.Y),
                    new(RM_SIZE * DSF, RM_SIZE * DSF));
                break;
            case ELSelection.lsTransPt:
                if (SecondTrans) {
                    objRect = new(new(TransPt[objnum].Loc[1].X * DSF + Offset.X,
                        TransPt[objnum].Loc[1].Y * DSF + Offset.Y),
                        new(RM_SIZE / 2 * DSF, RM_SIZE / 2 * DSF));
                }
                else {
                    objRect = new(new((TransPt[objnum].Loc[0].X) * DSF + Offset.X,
                        TransPt[objnum].Loc[0].Y * DSF + Offset.Y),
                        new(RM_SIZE / 2 * DSF, RM_SIZE / 2 * DSF));
                }
                break;
            case ELSelection.lsErrPt:
                objRect = new(new(ErrPt[objnum].Loc.X * DSF + Offset.X,
                    (ErrPt[objnum].Loc.Y) * DSF + Offset.Y),
                    new(RM_SIZE * 0.6f * DSF, RM_SIZE * 0.5196f * DSF));
                break;
            case ELSelection.lsComment:
                objRect = new(new((Comment[objnum].Loc.X) * DSF + Offset.X,
                    (Comment[objnum].Loc.Y) * DSF + Offset.Y),
                    new(Comment[objnum].Size.Width * DSF, Comment[objnum].Size.Height * DSF));
                break;
            }
            return objRect.IntersectsWith(picDraw.ClientRectangle);
        }

        private bool ObjOnScreen(ObjInfo ObjTest, bool SecondTrans = false) {

            // returns true if any portion of the object is on the screen
            return ObjOnScreen(ObjTest.Type, ObjTest.Number, SecondTrans);
        }

        private bool LineOnScreen(float X1, float Y1, float X2, float Y2) {
            // will determine if any points on the line are located on screen

            // convert line coordinates into screen coordinates
            PointF p1 = new((X1) * DSF + Offset.X, (Y1) * DSF + Offset.Y);
            PointF p2 = new((X2) * DSF + Offset.X, (Y2) * DSF + Offset.Y);
            RectangleF clientRect = picDraw.ClientRectangle;

            // if either endpoint is inside, return true
            if (clientRect.Contains(p1) || clientRect.Contains(p2)) {
                return true;
            }
            return LineIntersectsRect(p1, p2, clientRect);

            bool LineIntersectsRect(PointF p1, PointF p2, RectangleF r) {
                // Check if line intersects any of the rectangle's sides
                return LineIntersectsLine(p1, p2, new PointF(r.Left, r.Top), new PointF(r.Right, r.Top)) ||
                       LineIntersectsLine(p1, p2, new PointF(r.Right, r.Top), new PointF(r.Right, r.Bottom)) ||
                       LineIntersectsLine(p1, p2, new PointF(r.Right, r.Bottom), new PointF(r.Left, r.Bottom)) ||
                       LineIntersectsLine(p1, p2, new PointF(r.Left, r.Bottom), new PointF(r.Left, r.Top));
            }
            // Standard line segment intersection test
            bool LineIntersectsLine(PointF a1, PointF a2, PointF b1, PointF b2) {
                float d = (a2.X - a1.X) * (b2.Y - b1.Y) - (a2.Y - a1.Y) * (b2.X - b1.X);
                if (d == 0) return false; // Parallel lines
                float u = ((b1.X - a1.X) * (b2.Y - b1.Y) - (b1.Y - a1.Y) * (b2.X - b1.X)) / d;
                float v = ((b1.X - a1.X) * (a2.Y - a1.Y) - (b1.Y - a1.Y) * (a2.X - a1.X)) / d;
                return (u >= 0 && u <= 1) && (v >= 0 && v <= 1);
            }

        }

        private void DrawRoomPic(Graphics g, int RoomNum) {
            // make sure picture exists
            if (!EditGame.Pictures.Contains(RoomNum)) {
                return;
            }
            // save load state for later, load if necessary
            bool blnUnloaded = !EditGame.Pictures[RoomNum].Loaded;
            if (blnUnloaded) {
                EditGame.Pictures[RoomNum].Load();
            }
            g.DrawImage(EditGame.Pictures[RoomNum].VisualBMP,
                (Room[RoomNum].Loc.X) * DSF + Offset.X + 1,
                (Room[RoomNum].Loc.Y) * DSF + Offset.Y + 1,
                RM_SIZE * DSF - 2,
                RM_SIZE * 0.525f * DSF);

            // close pic if it was before
            if (blnUnloaded) {
                // unload the pic
                EditGame.Pictures[RoomNum].Unload();
            }
        }

        private void ChangeScale(int Dir, bool useanchor = false) {
            // adjusts scale and redraws layout
            // scale is an interger from 1 to 9, where 1 is 40 pixels per DSF unit
            // each increment in scale resizes the screen by 25% (so 1 = 100%
            // and 9 equals (1.25)^9 = 745%
            int NewScale;

            if (Dir < 0) {
                NewScale = DrawScale - 1;
                if (NewScale == 0) {
                    NewScale = 1;
                }
            }
            else {
                NewScale = DrawScale + 1;
                if (NewScale > 9) {
                    NewScale = 9;
                }
            }

            if (NewScale != DrawScale) {
                PointF ptCursor = GetZoomCenter(useanchor);

                // calculate new DSF value
                float NewDSF = (float)(40 * Math.Pow(1.25, NewScale - 1));

                // calculate new offset values
                // (CL1 - OF1) / DF1 = LC1
                // (CL2 - OF2) / DF2 = LC2
                // LC1 = LC2; CL1 = CL2
                // (CL1 - OF1) / DF1 = (CL1 - OF2) / DF2
                // (CL1 - OF1) / DF1 * DF2 = CL1 - OF2
                // - (CL1 - OF1) / DF1 * DF2 = -CL1 + OF2
                // CL1 - (CL1 - OF1) / DF1 * DF2 = OF2
                Offset.X = (int)(ptCursor.X - (ptCursor.X - Offset.X) / DSF * NewDSF);
                Offset.Y = (int)(ptCursor.Y - (ptCursor.Y - Offset.Y) / DSF * NewDSF);

                // now update scale and scale factor
                DrawScale = NewScale;
                DSF = NewDSF;

                InitFonts();

                spScale.Text = "Scale: " + DrawScale.ToString();

                // redraw
                SetScrollBars();
                DrawLayout();
            }
        }

        private PointF GetZoomCenter(bool useanchor = false) {
            // returns the cursor position in picDraw coordinates
            // if the cursor is not over the picDraw surface
            // it returns the center of the picDraw surface

            Point center = new(picDraw.ClientSize.Width / 2, picDraw.ClientSize.Height / 2);
            if (!useanchor) {
                return center;
            }
            Point cp = picDraw.PointToClient(Cursor.Position);
            if (picDraw.ClientRectangle.Contains(cp)) {
                return cp;
            }
            else {
                return center;
            }
        }

        private void SetScrollBars() {
            // adjust scrollbar values and Max/Min
            // as needed due to scrolling or resizing events

            // SCROLLBAR MATH:
            // ACT_SZ = size of the area being scrolled; usually the image size + margins
            // WIN_SZ = size of the window area; the container's client size
            // SV_MAX = maximum value that scrollbar can have; this puts the scroll bar
            //          and scrolled image at farthest position
            // LG_CHG = LargeChange property of the scrollbar
            // SB_MAX = actual Maximum property of the scrollbar, to avoid out-of-bounds errors
            //
            //      SV_MAX = ACT_SZ - WIN_SZ 
            //      SB_MAX = SV_MAX + LG_CHG + 1
            //
            // margins are 12 pixels on each side; 
            // image area for layouts is defined by Max and Min size (Max.X, Max.Y),
            // (Min.X, Min.Y). To account for the starting point of image area not always
            // being at (0, 0), scrollbar max and min need to be adjusted by the Min.X
            // and Min.Y values
            //
            // Offset values are allowed to exceed the layout Max and Min values, so 
            // the scrollbar max and min values need to be adjusted when offsets
            // are set to values outside the range of the layout area
            // negative offsets means draw layout to left/top of origin
            //
            // to convert to/from layout space and client space:
            // Client(X,Y) = Layout(X,Y) * DSF + OFFSET
            //
            // basic equations to calculate/manage scrollbar settings:
            // OFFSET = -SB.VAL
            // SB.MIN = MIN * DSF - MGN
            // SVMAX = SVRNG = (MAX - MIN) * DSF - CL
            // SB.MAX = SB.MIN + SVRNG + MGN + SB.LGC + 1
            int hmin, hmax, vmin, vmax;
            NoScrollUpdate = true;

            // determine scroll range to be able to set max
            PointF svrange = new(
                (Max.X - Min.X) * DSF + 2 * MGN - picDraw.ClientSize.Width,
                (Max.Y - Min.Y) * DSF + 2 * MGN - picDraw.ClientSize.Height
                );
            if (svrange.X < 0) {
                svrange.X = 0;
            }
            if (svrange.Y < 0) {
                svrange.Y = 0;
            }
            // scroll bars SUCK - it's devilishly hard to set
            // min, max, value and change properties because
            // changing one of them changes the others 
            // to make sure the change values work correctly,
            // set the scroll bar values to match the current
            // client window size (but before that, use WM_SETREDRAW
            // to disable flickering while changes are being set)
            SendMessage(hScrollBar1.Handle, WM_SETREDRAW, false, 0);
            SendMessage(vScrollBar1.Handle, WM_SETREDRAW, false, 0);
            hScrollBar1.Minimum = 0;
            hScrollBar1.Maximum = picDraw.ClientSize.Width;
            vScrollBar1.Minimum = 0;
            vScrollBar1.Maximum = picDraw.ClientSize.Height;
            // adjust scroll bar change values;
            // small change should be 20% of scrollable window
            // large change should be 80%
            hScrollBar1.SmallChange = (int)(0.2 * picDraw.ClientSize.Width);
            hScrollBar1.LargeChange = (int)(0.8 * picDraw.ClientSize.Width);
            vScrollBar1.SmallChange = (int)(0.2 * picDraw.ClientSize.Height);
            vScrollBar1.LargeChange = (int)(0.8 * picDraw.ClientSize.Height);
            ////hScrollBar1.SmallChange = 1;
            ////hScrollBar1.LargeChange = 2;
            ////vScrollBar1.SmallChange = 1;
            ////vScrollBar1.LargeChange = 2;
            // set minimums
            hmin = (int)(Min.X * DSF - MGN);
            vmin = (int)(Min.Y * DSF - MGN);

            // set max (min plus range)
            hmax = (int)(hmin + svrange.X);
            vmax = (int)(vmin + svrange.Y);


            // if current offset values are outside the scroll range
            // expand the range to hold them
            if (Offset.X > -hmin) {
                hmin = -Offset.X;
            }
            else if (Offset.X < -hmax) {
                hmax = -Offset.X;
            }
            if (Offset.Y > -vmin) {
                vmin = -Offset.Y;
            }
            else if (Offset.Y < -vmax) {
                vmax = -Offset.Y;
            }
            hScrollBar1.Minimum = hmin;
            vScrollBar1.Minimum = vmin;
            // adjust maximum to include the largechange value minus 1)
            hScrollBar1.Maximum = hmax + hScrollBar1.LargeChange - 1;
            vScrollBar1.Maximum = vmax + vScrollBar1.LargeChange - 1;

            // confirm scrollbar and offset values match
            hScrollBar1.Value = -Offset.X;
            vScrollBar1.Value = -Offset.Y;
            NoScrollUpdate = false;
            // allow scroll bar updates again
            SendMessage(hScrollBar1.Handle, WM_SETREDRAW, true, 0);
            SendMessage(vScrollBar1.Handle, WM_SETREDRAW, true, 0);
            hScrollBar1.Invalidate();
            vScrollBar1.Invalidate();
        }

        public void InsertErrPt(int FromRoom, int ExitIndex, int ErrRoom) {
            /*
            // insert an error point for the exit (ExitIndex) coming from (FromRoom)
            // the original destination (ErrRoom) is no longer in the game

            Dim lngRoom As Long, tmpCoord As LCoord
            Dim lngEP As Long, strMsg As String

            // inform user that an error point is being inserted
            strMsg = "Exit " & Room(FromRoom).Exits(ExitIndex).ID & " in '" & Logics(FromRoom).ID & "' points to a nonexistent room (" & ErrRoom & ")."
            strMsg = strMsg & vbNewLine & vbNewLine & "An error point will be inserted at this exit point."
            MsgBoxEx strMsg, vbInformation + vbOKOnly + vbMsgBoxHelpButton, "Exit Error Detected", WinAGIHelp, "htm\winagi\Layout_Editor.htm#errpts"

            // find the next available errpt
            lngEP = 0
            Do
              lngEP = lngEP + 1
            Loop While ErrPt(lngEP).Visible

            Room(FromRoom).Exits(ExitIndex).Transfer = -lngEP

            Select Case Room(FromRoom).Exits(ExitIndex).Reason
            Case erNone, erOther
            // position first around from room
              tmpCoord = GetInsertPos(Room(FromRoom).Loc.X, Room(FromRoom).Loc.Y, 0, 1)
            Case erHorizon
            // position around point above
              tmpCoord.X = Room(FromRoom).Loc.X
              tmpCoord.Y = Room(FromRoom).Loc.Y - 1
            Case erBottom
            // position around point below
              tmpCoord.X = Room(FromRoom).Loc.X
              tmpCoord.Y = Room(FromRoom).Loc.Y + 1
            Case erLeft
            // position around point to left
              tmpCoord.X = Room(FromRoom).Loc.X - 1
              tmpCoord.Y = Room(FromRoom).Loc.Y
            Case erRight
            // position around point to right
              tmpCoord.X = Room(FromRoom).Loc.X + 1
              tmpCoord.Y = Room(FromRoom).Loc.Y
            End Select

            // put errpt here
            // adjust to be centered over same point as a room
            // (by adding .1 to x and .2 to y)
              ErrPt(lngEP).Loc.X = tmpCoord.X + 0.1
              ErrPt(lngEP).Loc.Y = tmpCoord.Y + RM_SIZE / 4
              ErrPt(lngEP).Visible = True
              ErrPt(lngEP).Room = ErrRoom
              ErrPt(lngEP).FromRoom = FromRoom
              ErrPt(lngEP).ExitID = Room(FromRoom).Exits(ExitIndex).ID
              ErrPt(lngEP).Order = ObjCount

            ObjOrder(ObjCount).Type = lsErrPt
            ObjOrder(ObjCount).Number = lngEP
            ObjCount = ObjCount + 1
            */
        }

        private TSel ItemFromPos(Point pos) {
            // check for edge exit
            TSel retval = ExitFromPos(1, pos);
            if (retval.Type != ELSelection.lsNone) {
                return retval;
            }
            // other exits
            retval = ExitFromPos(0, pos);
            if (retval.Type != ELSelection.lsNone) {
                return retval;
            }
            // any other object
            retval = ObjectFromPos(pos);
            return retval;
        }

        private TSel ExitFromPos(int SearchPriority, Point pos) {
            // finds the line under the point x,y that matches style, and assigns it
            // to NewSel object
            // return true if an exit found, otherwise false

            // SearchPriority = 0: only other exits are checked
            //                = 1: all edge exits are searched
            TSel retval = new();

            /*
            //Dim i As Long, j As Long
            //Dim tmpRoom As Long, tmpID As String
            //Dim tmpSPX As Single, tmpSPY As Single
            //Dim tmpEPX As Single, tmpEPY As Single
            //Dim lngPOL As Long

            //'set width to three so PointInLine function
            //'can find lines that are two pixels wide
            //picDraw.DrawWidth = 3

            //For i = 1 To 255
            //    If Room(i).Visible Then
            //    For j = 0 To Room(i).Exits.Count - 1
            //        'dont include deleted exits
            //        If Room(i).Exits(j).Status <> esDeleted Then
            //        'if Type matches
            //        If (SearchPriority = 0 And ((Room(i).Exits(j).Reason = erOther))) Or (SearchPriority = 1 And Room(i).Exits(j).Reason <> erOther) Then
            //            'if there are no transfer points, i.e. zero or negative
            //            If Room(i).Exits(j).Transfer <= 0 Then
            //            'starting point and ending point of line come directly
            //            'from the exit's start-end points
            //            tmpSPX = Room(i).Exits(j).SPX
            //            tmpSPY = Room(i).Exits(j).SPY
            //            tmpEPX = Room(i).Exits(j).EPX
            //            tmpEPY = Room(i).Exits(j).EPY

            //            'dont bother checking, unless line is actually visible
            //            If LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
            //                'check for an arrow first
            //                If PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
            //                'if exit has a reciprocal
            //                If IsTwoWay(i, j) Then
            //                    NewSel.TwoWay = ltwOneWay
            //                Else
            //                    NewSel.TwoWay = ltwSingle
            //                End If
            //                'select line
            //                NewSel.Type = lsExit
            //                NewSel.Number = i
            //                NewSel.ExitID = Room(i).Exits(j).ID
            //                NewSel.Leg = llNoTrans
            //                Exit Sub

            //                'if not on arrow, check line
            //                ElseIf PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
            //                'if exit has a reciprocal
            //                If IsTwoWay(i, j, tmpRoom, tmpID) Then
            //                    'check if arrow on reciprocal is selected
            //                    If PointOnArrow(X, Y, tmpEPX, tmpEPY, tmpSPX, tmpSPY) Then
            //                    'on arrow means one way
            //                    NewSel.TwoWay = ltwOneWay
            //                    NewSel.Number = tmpRoom
            //                    NewSel.ExitID = tmpID
            //                    NewSel.Type = lsExit
            //                    NewSel.Leg = llNoTrans
            //                    Exit Sub
            //                    Else
            //                    'both
            //                    NewSel.TwoWay = ltwBothWays
            //                    End If
            //                Else
            //                    'single line
            //                    NewSel.TwoWay = ltwSingle
            //                End If

            //                'select line
            //                NewSel.Type = lsExit
            //                NewSel.Number = i
            //                NewSel.ExitID = Room(i).Exits(j).ID
            //                NewSel.Leg = llNoTrans
            //                Exit Sub
            //                End If
            //            End If

            //            Else
            //            'there are transfers; check both segments
            //            'first segment
            //            tmpSPX = Room(i).Exits(j).SPX
            //            tmpSPY = Room(i).Exits(j).SPY

            //            'if this is first exit with transfer point
            //            If TransPt(Room(i).Exits(j).Transfer).Room(0) = i Then
            //                tmpEPX = TransPt(Room(i).Exits(j).Transfer).SP.X
            //                tmpEPY = TransPt(Room(i).Exits(j).Transfer).SP.Y
            //            Else
            //                'swap ep and sp
            //                tmpEPX = TransPt(Room(i).Exits(j).Transfer).EP.X
            //                tmpEPY = TransPt(Room(i).Exits(j).Transfer).EP.Y
            //            End If

            //            If LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
            //                'check for an arrow first
            //                If PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
            //                'if exit has a reciprocal
            //                If IsTwoWay(i, j) Then
            //                    NewSel.TwoWay = ltwOneWay
            //                Else
            //                    NewSel.TwoWay = ltwSingle
            //                End If
            //                'select line
            //                NewSel.Type = lsExit
            //                NewSel.Number = i
            //                NewSel.ExitID = Room(i).Exits(j).ID
            //                'use first leg
            //                NewSel.Leg = llFirst
            //                Exit Sub

            //                'if not on arrow, check line
            //                ElseIf PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
            //                'if exit has a reciprocal
            //                If IsTwoWay(i, j, tmpRoom, tmpID) Then
            //                    'check if arrow on reciprocal is selected
            //                    If PointOnArrow(X, Y, tmpEPX, tmpEPY, tmpSPX, tmpSPY) Then
            //                    'on arrow means one way
            //                    NewSel.TwoWay = ltwOneWay
            //                    NewSel.Number = tmpRoom
            //                    NewSel.ExitID = tmpID
            //                    NewSel.Type = lsExit
            //                    NewSel.Leg = llSecond
            //                    Exit Sub
            //                    Else
            //                    'both
            //                    NewSel.TwoWay = ltwBothWays
            //                    End If
            //                Else
            //                    'single line
            //                    NewSel.TwoWay = ltwSingle
            //                End If

            //                'select line
            //                NewSel.Type = lsExit
            //                NewSel.Number = i
            //                NewSel.ExitID = Room(i).Exits(j).ID
            //                'use first leg
            //                NewSel.Leg = llFirst
            //                Exit Sub
            //                End If
            //            End If

            //            'second segment
            //            'if this is first exit with transfer point
            //            If TransPt(Room(i).Exits(j).Transfer).Room(0) = i Then
            //                tmpSPX = TransPt(Room(i).Exits(j).Transfer).EP.X
            //                tmpSPY = TransPt(Room(i).Exits(j).Transfer).EP.Y
            //            Else
            //                'swap ep and sp
            //                tmpSPX = TransPt(Room(i).Exits(j).Transfer).SP.X
            //                tmpSPY = TransPt(Room(i).Exits(j).Transfer).SP.Y
            //            End If

            //            tmpEPX = Room(i).Exits(j).EPX
            //            tmpEPY = Room(i).Exits(j).EPY

            //            If LineOnScreen(tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
            //                'check for an arrow first
            //                If PointOnArrow(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
            //                'if exit has a reciprocal
            //                If IsTwoWay(i, j) Then
            //                    NewSel.TwoWay = ltwOneWay
            //                Else
            //                    NewSel.TwoWay = ltwSingle
            //                End If
            //                'select line
            //                NewSel.Type = lsExit
            //                NewSel.Number = i
            //                NewSel.ExitID = Room(i).Exits(j).ID
            //                'use second leg
            //                NewSel.Leg = llSecond
            //                Exit Sub

            //                'if not on arrow, check line
            //                ElseIf PointOnLine(X, Y, tmpSPX, tmpSPY, tmpEPX, tmpEPY) Then
            //                'if exit has a reciprocal
            //                If IsTwoWay(i, j, tmpRoom, tmpID) Then
            //                    'check if arrow on reciprocal is selected
            //                    If PointOnArrow(X, Y, tmpEPX, tmpEPY, tmpSPX, tmpSPY) Then
            //                    'on arrow means one way
            //                    NewSel.TwoWay = ltwOneWay
            //                    NewSel.Number = tmpRoom
            //                    NewSel.ExitID = tmpID
            //                    NewSel.Type = lsExit
            //                    NewSel.Leg = llFirst
            //                    Exit Sub
            //                    Else
            //                    'both
            //                    NewSel.TwoWay = ltwBothWays
            //                    End If
            //                Else
            //                    'single line
            //                    NewSel.TwoWay = ltwSingle
            //                End If

            //                'select line
            //                NewSel.Type = lsExit
            //                NewSel.Number = i
            //                NewSel.ExitID = Room(i).Exits(j).ID
            //                'use second leg
            //                NewSel.Leg = llSecond
            //                Exit Sub
            //                End If
            //            End If
            //            End If
            //        End If
            //        End If
            //    Next j
            //    End If
            //Next i
            */
            return retval;
        }

        private TSel ObjectFromPos(Point pos) {
            TSel retval = new();
            /*
            'called from picDraw_MouseDown and the tip timer
            'if an object is found under the cursor,
            'NewSel is populated with the object info

            Dim i As Long
            Dim tmpX As Single, tmpY As Single

            'default to nothing selected
            NewSel.Type = lsNone

            For i = ObjCount - 1 To 0 Step -1
                'if over this object
                Select Case ObjOrder[i].Type
                Case lsRoom
                'if object surrounds point
                tmpX = (Room(ObjOrder[i].Number).Loc.X + Offset.X) * DSF
                tmpY = (Room(ObjOrder[i].Number).Loc.Y + Offset.Y) * DSF
                If X >= tmpX And X <= tmpX + RM_SIZE * DSF And Y >= tmpY And Y <= tmpY + RM_SIZE * DSF Then
                    'found it
                    NewSel.Number = ObjOrder[i].Number
                    NewSel.Type = lsRoom
                    Exit Sub
                End If

                Case lsTransPt
                    'if object surrounds point
                    tmpX = (TransPt(ObjOrder[i].Number).Loc(0).X + Offset.X) * DSF
                    tmpY = (TransPt(ObjOrder[i].Number).Loc(0).Y + Offset.Y) * DSF
                    If X >= tmpX And X <= tmpX + RM_SIZE / 2 * DSF And Y >= tmpY And Y <= tmpY + RM_SIZE / 2 * DSF Then
                    'found it
                    NewSel.Number = ObjOrder[i].Number
                    'use leg to hold index of loc and room
                    NewSel.Leg = 0
                    NewSel.Type = lsTransPt
                    Exit Sub
                    End If
                    tmpX = (TransPt(ObjOrder[i].Number).Loc(1).X + Offset.X) * DSF
                    tmpY = (TransPt(ObjOrder[i].Number).Loc(1).Y + Offset.Y) * DSF
                    If X >= tmpX And X <= tmpX + RM_SIZE / 2 * DSF And Y >= tmpY And Y <= tmpY + RM_SIZE / 2 * DSF Then
                    'found it
                    NewSel.Number = ObjOrder[i].Number
                    'use leg to hold index of loc and room
                    NewSel.Leg = 1
                    NewSel.Type = lsTransPt
                    Exit Sub
                    End If

                Case lsComment
                'if object surrounds point
                tmpX = (Comment(ObjOrder[i].Number).Loc.X + Offset.X) * DSF
                tmpY = (Comment(ObjOrder[i].Number).Loc.Y + Offset.Y) * DSF
                If X >= tmpX And X <= tmpX + Comment(ObjOrder[i].Number).Size.X * DSF And Y >= tmpY And Y <= tmpY + Comment(ObjOrder[i].Number).Size.Y * DSF Then
                    'found it
                    NewSel.Number = ObjOrder[i].Number
                    NewSel.Type = lsComment
                    Exit Sub
                End If
                Case lsErrPt
                'if object surrounds point
                tmpX = (ErrPt(ObjOrder[i].Number).Loc.X + Offset.X) * DSF
                tmpY = (ErrPt(ObjOrder[i].Number).Loc.Y + Offset.Y) * DSF
                If X >= tmpX And X <= tmpX + 0.6 * DSF And Y >= tmpY And Y <= tmpY + RM_SIZE / 2 * DSF Then
                    'found it
                    NewSel.Number = ObjOrder[i].Number
                    NewSel.Type = lsErrPt
                    Exit Sub
                End If
                End Select
            Next i
            */
            return retval;
        }

        private bool SameAsSelection(ref TSel tmpSel) {
            // compares tmpSel with current selection object; if all elements are equal,
            // returns true (x and y values are not part of the check)

            return Selection.Type == tmpSel.Type &&
                   Selection.Number == tmpSel.Number &&
                   Selection.ExitID == tmpSel.ExitID &&
                   Selection.Leg == tmpSel.Leg &&
                   Selection.TwoWay == tmpSel.TwoWay &&
                   Selection.Point == tmpSel.Point;
        }

        bool IsSelected(ELSelection ObjType, int ObjNum, ELLeg Leg = 0) {
            // Returns true if the specified object is selected
            switch (Selection.Type) {
            case ELSelection.lsRoom:
                return (ObjNum == Selection.Number) && (ObjType == ELSelection.lsRoom);

            case ELSelection.lsTransPt:
                return (ObjType == ELSelection.lsTransPt) && (Selection.Number == ObjNum) && (Selection.Leg == Leg);

            case ELSelection.lsErrPt:
                return (ObjType == ELSelection.lsErrPt) && (Selection.Number == ObjNum);

            case ELSelection.lsMultiple:
                for (int i = 0; i <= Selection.Number - 1; i++) {
                    switch (ObjType) {
                    case ELSelection.lsTransPt:
                        if (SelectedObjects[i].Type == ELSelection.lsTransPt) {
                            if (Math.Abs(SelectedObjects[i].Number) == ObjNum) {
                                if (Leg == 0 && SelectedObjects[i].Number > 0)
                                    return true;
                                else if (Leg == ELLeg.llFirst && SelectedObjects[i].Number < 0)
                                    return true;
                            }
                        }
                        break;
                    case ELSelection.lsRoom:
                    case ELSelection.lsErrPt:
                    case ELSelection.lsComment:
                        if (SelectedObjects[i].Type == ObjType && SelectedObjects[i].Number == ObjNum)
                            return true;
                        break;
                    }
                }
                break;
            }
            return false;
        }

        private void RecalculateMaxMin() {
            // reset Max and min
            Min.X = float.MaxValue;
            Min.Y = float.MaxValue;
            Max.X = float.MinValue;
            Max.Y = float.MinValue;

            for (int i = 0; i < ObjOrder.Count; i++) {
                int lngNum = ObjOrder[i].Number;
                switch (ObjOrder[i].Type) {
                case ELSelection.lsRoom:
                    CheckMaxMin(Room[lngNum].Loc.X, Room[lngNum].Loc.Y, RM_SIZE, RM_SIZE);
                    break;
                case ELSelection.lsTransPt:
                    CheckMaxMin(TransPt[lngNum].Loc[0].X, TransPt[lngNum].Loc[0].Y, RM_SIZE / 2, RM_SIZE / 2);
                    CheckMaxMin(TransPt[lngNum].Loc[1].X, TransPt[lngNum].Loc[1].Y, RM_SIZE / 2, RM_SIZE / 2);
                    break;
                case ELSelection.lsErrPt:
                    CheckMaxMin(ErrPt[lngNum].Loc.X, ErrPt[lngNum].Loc.Y, 0.6f, 0.5196f);
                    break;
                case ELSelection.lsComment:
                    CheckMaxMin(Comment[lngNum].Loc.X, Comment[lngNum].Loc.Y, Comment[lngNum].Size.Width, Comment[lngNum].Size.Height);
                    break;
                }
            }
            // if no objects,
            if (ObjOrder.Count <= 0) {
                Min.X = 0;
                Min.Y = 0;
                Max.X = 6;
                Max.Y = 6;
            }
            else {
                if (Max.X - Min.X < 6) {
                    Max.X = 6 + Min.X;
                }
                else if (Max.Y - Min.Y < 6) {
                    Max.Y = 6 + Min.Y;
                }
            }
        }

        private void CheckMaxMin(float TestX, float TestY, float TestW, float TestH) {
            // compare test values against current Max and min
            if (TestX < Min.X) {
                Min.X = TestX;
            }
            if (TestY < Min.Y) {
                Min.Y = TestY;
            }
            if (TestX + TestW > Max.X) {
                Max.X = TestX + TestW;
            }
            if (TestY + TestH > Max.Y) {
                Max.Y = TestY + TestH;
            }
        }

        private void DeselectObj() {
            //Dim rtn As Long

            //Select Case Selection.Type
            //Case lsNone
            //  'no action needed

            //Case lsMultiple
            //  'hide selection box
            //  shpMove.Visible = False

            //  'redraw without making selection
            //  DrawLayout False

            //Case Else
            //  'copy area under selection handles back to main bitmap

            //  'all selections use at least two handles
            //  rtn = BitBlt(picDraw.hDC, Selection.X1, Selection.Y1, 8, 8, picHandle.hDC, 0, 0, SRCCOPY)
            //  rtn = BitBlt(picDraw.hDC, Selection.X2, Selection.Y2, 8, 8, picHandle.hDC, 24, 0, SRCCOPY)

            //  'if not an exit
            //  If Selection.Type <> lsExit Then
            //    'reset the other two handles, too
            //    rtn = BitBlt(picDraw.hDC, Selection.X1, Selection.Y2, 8, 8, picHandle.hDC, 16, 0, SRCCOPY)
            //    rtn = BitBlt(picDraw.hDC, Selection.X2, Selection.Y1, 8, 8, picHandle.hDC, 8, 0, SRCCOPY)
            //  Else
            //    'if one direction, AND exit is two way
            //    If Selection.TwoWay = ltwOneWay Then
            //      'add third handle by arrowhead
            //      rtn = BitBlt(picDraw.hDC, Selection.X3, Selection.Y3, 8, 8, picHandle.hDC, 8, 0, SRCCOPY)
            //    End If
            //  End If

            //  'refresh drawing surface
            //  picDraw.Refresh
            //End Select

            //'reset selection variables
            //Selection.Number = 0
            //Selection.Type = lsNone
            //Selection.ExitID = vbNullString
            //Selection.Leg = llNoTrans

            //'disable toolbar buttons
            //With Toolbar1.Buttons
            //  .Item("delete").Enabled = False
            //  .Item("transfer").Enabled = False
            //  .Item("hide").Enabled = False
            //  .Item("front").Enabled = False
            //  .Item("back").Enabled = False
            //End With

            //'reset statusbar
            //If MainStatusBar.Tag <> CStr(rtLayout) Then
            //  AdjustMenus rtLayout, True, True, IsChanged
            //End If

            //With MainStatusBar.Panels
            //  .Item("Room1").Text = vbNullString
            //  .Item("Room2").Text = vbNullString
            //  .Item("Type").Text = vbNullString
            //  .Item("ID").Text = vbNullString
            //End With
        }

        internal void SelectRoom(int selResNum) {
            /*
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
        ' hScrollBar1.Value = -100 * (picDraw.Width/2/DSF - Room(RoomNum).Loc.X)
            NewHSVal = -100 * (picDraw.Width / 2 / DSF - Room(RoomNum).Loc.X)
            If NewHSVal < hScrollBar1.Min Then
            NewHSVal = hScrollBar1.Min
            ElseIf NewHSVal > hScrollBar1.Max Then
            NewHSVal = hScrollBar1.Max
            End If
            'reposition horizontal scroll (but turn draw off; only need to
            ' draw the layout once, after the vertical value is set)
            blnDontDraw = True
            hScrollBar1.Value = NewHSVal

            NewVsVal = -100 * (picDraw.Height / 2 / DSF - Room(RoomNum).Loc.Y)
            If NewVsVal < vScrollBar1.Min Then
            NewVsVal = vScrollBar1.Min
            ElseIf NewVsVal > vScrollBar1.Max Then
            NewVsVal = vScrollBar1.Max
            End If
            'reposition vertical scroll (turn draw on, so the layout updates
            blnDontDraw = False
            vScrollBar1.Value = NewVsVal
        End If

            */
        }

        private void DisplayRoom(int NewRoom, bool NeedPos, bool NoExits = false) {
            /*
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
            Room(NewRoom).Loc = GetInsertPos((CalcWidth - Toolbar1.Width) / 2 / DSF - Offset.X, CalcHeight / 2 / DSF - Offset.Y)
        End If

        'look for exits pointing to error points which match this
        'look for hidden exits pointing to this
        For i = 1 To 255
            'if room is visible (but not room being added)
            If Room(i).Visible And i <> NewRoom Then
            If Room(i).Exits.Count > 0 Then
                For j = 0 To Room(i).Exits.Count - 1
                If Room(i).Exits(j).Room = NewRoom And Room(i).Exits(j).Status <> esDeleted Then
                    'change status back to normal
                    Room(i).Exits(j).Status = esOK

                    'in case an err pt is found, deal with it
                    If Room(i).Exits(j).Transfer < 0 Then
                    'if location not known, and not found yet
                    If Not blnFound And NeedPos Then
                        blnFound = True
                        'move it here
                        Room(NewRoom).Loc.X = GridPos(ErrPt(-Room(i).Exits(j).Transfer).Loc.X - 0.1)
                        Room(NewRoom).Loc.Y = GridPos(ErrPt(-Room(i).Exits(j).Transfer).Loc.Y - RM_SIZE / 4)
                    End If

                    'hide errpt
                    With ErrPt(-Room(i).Exits(j).Transfer)
                        CompactObjList .Order
                        .Visible = False
                        .ExitID = ""
                        .Room = 0
                        .FromRoom = 0
                        .Order = 0
                    End With
                    'clear transfer marker
                    Room(i).Exits(j).Transfer = 0
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
            Set Room(NewRoom).Exits = ExtractExits(Logics(NewRoom))
        End If
            */
        }

        private void HideRoom(int OldRoom) {
            // mark as not visible, then
            // run through all exits, and if a reference to OldRoom is found
            // mark it as hidden (or as ErrPt, if the logic is gone from the
            // game)
            // also, delete obselete transfer points
            // 
            // it is possible that the getlayout method could call this method
            // for a room that is already hidden; in that case, just exit
            //
            // also checks to see if any of this room's exits point to errors
            // and deletes them

            // if there is a selection
            if (Selection.Type != ELSelection.lsNone) {
                DeselectObj();
            }
            // first, check for any existing transfers or errpts FROM the room being hidden
            for (int i = 0; i < Room[OldRoom].Exits.Count; i++) {
                int transfer = Room[OldRoom].Exits[i].Transfer;
                if (transfer > 0) {
                    // remove the transfer point
                    DeleteTransfer(transfer);
                }
                else if (transfer < 0) {
                    // remove errpt
                    CompactObjList(ErrPt[-transfer].Order);
                    ErrPt[-transfer].Visible = false;
                    ErrPt[-transfer].ExitID = "";
                    ErrPt[-transfer].FromRoom = 0;
                    ErrPt[-transfer].Room = 0;
                    ErrPt[-transfer].Loc = new();
                    ErrPt[-transfer].Order = 0;
                }
            }
            // remove the room
            Room[OldRoom].Visible = false;
            Room[OldRoom].Loc = new();
            // remove from object list
            CompactObjList(Room[OldRoom].Order);
            Room[OldRoom].ShowPic = false;
            // clear the exits for removed room
            Room[OldRoom].Exits.Clear();

            // step through all other exits
            for (int i = 1; i < 256; i++) {
                // only need to check rooms that are currently visible
                if (Room[i].Visible) {
                    for (int j = 0; j < Room[i].Exits.Count; j++) {
                        if (Room[i].Exits[j].Room == OldRoom && Room[i].Exits[j].Status != EEStatus.esDeleted) {
                            // show the exit as hidden, if the target room is still a valid logic
                            // otherwise add an ErrPt
                            if (EditGame.Logics.Contains(OldRoom)) {
                                // mark the exit as hidden
                                Room[i].Exits[j].Hidden = true;
                            }
                            else {
                                // need to replace the exit with an errpoint, if not already done
                                if (Room[i].Exits[j].Transfer >= 0) {
                                    // insert a error point
                                    InsertErrPt(i, j, OldRoom);
                                }
                            }
                            // check for transfer pt
                            if (Room[i].Exits[j].Transfer > 0) {
                                // remove it
                                DeleteTransfer(Room[i].Exits[j].Transfer);
                            }
                            // set exit pos
                            SetExitPos(i, Room[i].Exits[j].ID);
                        }
                    }
                }
            }
            MarkAsChanged();
        }

        private void DeleteExit(TSel OldSel) {
            /*
            'deletes the exit, and hide transfers if appropriate

            'also delete a reciprocal, if there is one and a two way exit is specified
            '(if you don't want both exits deleted, make sure OldSel is marked as one way)

            Dim tmpTrans As Long, tmpCoord As LCoord
            Dim tmpRoom As Long, tmpID As String
            Dim i As Long

            'if a transfer was involved
            tmpTrans = Room(OldSel.Number).Exits(OldSel.ExitID).Transfer
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
                If Room(OldSel.Number).Exits(OldSel.ExitID).Leg = 0 Then
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
            '*'Debug.Assert Room(.Room(0)).Exits(.ExitID(0)).Leg = 1
                    Room(.Room(0)).Exits(.ExitID(0)).Leg = 0
                    End With
                End If
                End If
            End Select

            'if two way,and both are selected
            If OldSel.TwoWay = ltwBothWays Then
                'find and delete reciprocal exit
                If IsTwoWay(OldSel.Number, OldSel.ExitID, tmpRoom, tmpID) Then
                If Room(tmpRoom).Exits(tmpID).Status = esNew Then
                    Room(tmpRoom).Exits.Remove tmpID
                Else
                    Room(tmpRoom).Exits(tmpID).Status = esDeleted
                End If
                End If
            End If

            'if this is a new exit, not yet in logic source,
            If Room(OldSel.Number).Exits(OldSel.ExitID).Status = esNew Then
                'remove the exit
                Room(OldSel.Number).Exits.Remove OldSel.ExitID
            Else
                Room(OldSel.Number).Exits(OldSel.ExitID).Status = esDeleted
                If Room(OldSel.Number).Exits(OldSel.ExitID).OldRoom = 0 Then
                'keep the oldroom value in case we end up restoring the exit
                Room(OldSel.Number).Exits(OldSel.ExitID).OldRoom = Room(OldSel.Number).Exits(OldSel.ExitID).Room
                End If
                'make sure the transfer value is reset to zero
                Room(OldSel.Number).Exits(OldSel.ExitID).Transfer = 0
            End If
            */
        }

        private void DeleteTransfer(int TransNum) {

            // remove transfer from exit objects
            Room[TransPt[TransNum].Room[0]].Exits[TransPt[TransNum].ExitID[0]].Transfer = 0;
            Debug.Assert(Room[TransPt[TransNum].Room[0]].Exits[TransPt[TransNum].ExitID[0]].Leg == 0);
            if (TransPt[TransNum].Count == 2) {
                Room[TransPt[TransNum].Room[1]].Exits[TransPt[TransNum].ExitID[1]].Transfer = 0;
                Debug.Assert(Room[TransPt[TransNum].Room[1]].Exits[TransPt[TransNum].ExitID[1]].Leg == 1);
                Room[TransPt[TransNum].Room[1]].Exits[TransPt[TransNum].ExitID[1]].Leg = 0;
            }

            // set Count to zero so transpt won't be used during reposition
            TransPt[TransNum].Count = 0;
            RepositionRoom(TransPt[TransNum].Room[0]);
            TransPt[TransNum].ExitID[0] = "";
            TransPt[TransNum].ExitID[1] = "";
            TransPt[TransNum].Room[0] = 0;
            TransPt[TransNum].Room[1] = 0;
            CompactObjList(TransPt[TransNum].Order);
        }

        private void RepositionRoom(int Index, bool BothDirections = true, bool loading = false) {
            // this method ensures that the starting and ending
            // points of all exits from and to this room
            // are adjusted for the current room/trans pt positions

            // DON'T call this for an error point

            // step through all exits in this room
            for (int i = 0; i < Room[Index].Exits.Count; i++) {
                switch (Room[Index].Exits[i].Status) {
                case EEStatus.esDeleted or EEStatus.esChanged:
                    // these cases are only possible while a layout is being
                    // worked on; in that case, we trust that 'deleted' or
                    // 'changed' states are accurate, and don't do anything
                    // with them
                    break;
                case EEStatus.esOK:
                    // if loading a layout, then we need to verify that OK exits
                    // really are OK, hiding or adding ErrPt if necessary
                    if (loading) {
                        if (!EditGame.Logics.Contains(Room[Index].Exits[i].Room)) {
                            // if not already pointing to an error point
                            if (Room[Index].Exits[i].Transfer >= 0) {
                                InsertErrPt(Index, i, Room[Index].Exits[i].Room);
                            }
                        }
                        // otherwise, check to see if the logic is still a room
                        else if (!EditGame.Logics[Room[Index].Exits[i].Room].IsRoom && Room[Index].Exits[i].Room > 0) {
                            // since this IS NOT a valid room, change exit type to hidden
                            Room[Index].Exits[i].Hidden = true;
                        }
                    }
                    else {
                        // if not loading a layout, OK should really mean OK!
                        if (!EditGame.Logics.Contains(Room[Index].Exits[i].Room)) {
                            Debug.Assert(false);
                        }
                        else if (!EditGame.Logics[Room[Index].Exits[i].Room].IsRoom) {
                            Debug.Assert(false);
                        }
                    }
                    break;
                }
                // hidden?
                if (Room[Index].Exits[i].Hidden) {
                    // if loading a layout, then we need to verify that
                    // hidden exits really are hidden, changing them to
                    // OK or inserting an ErrPt if necessary
                    if (loading) {
                        // if the room is not a valid logic then we have to add an ErrPt
                        if (!EditGame.Logics.Contains(Room[Index].Exits[i].Room)) {
                            InsertErrPt(Index, i, Room[Index].Exits[i].Room);
                            // otherwise, check to see if the logic is still NOT a room
                        }
                        else if (EditGame.Logics[Room[Index].Exits[i].Room].IsRoom) {
                            // since this IS a valid room, reset hidden property
                            Room[Index].Exits[i].Hidden = false;
                        }
                    }
                    else {
                        // if not loading a layout, hidden really should mean hidden!
                        if (!EditGame.Logics.Contains(Room[Index].Exits[i].Room)) {
                            Debug.Assert(false);
                        }
                        else if (EditGame.Logics[Room[Index].Exits[i].Room].IsRoom) {
                            // when unhiding a room, if there are multiple identical
                            // exits to the newly unhidden room, the later ones will
                            // still show up as hidden when the first one causes
                            // reposition room; ignore it
                            // ??????????? wth?
                            Debug.Assert(false);
                        }
                    }
                }
                // reposition exit starting and ending points
                SetExitPos(Index, i);
            }

            // if checking both directions
            // (need to check other rooms that exit TO this room)
            if (BothDirections) {
                // step through all other rooms
                for (int i = 1; i < 256; i++) {
                    // if room is visible AND not index room
                    if (Room[i].Visible && i != Index) {
                        // step through all exits
                        for (int j = 0; j < Room[i].Exits.Count; j++) {
                            // if an exit goes to this room and it's not deleted
                            if (Room[i].Exits[j].Room == Index && Room[i].Exits[j].Status != EEStatus.esDeleted) {
                                // isn't Room(Index) always visible? otherwise this function wouldn't
                                // be called
                                Debug.Assert(Room[Index].Visible);
                                // if the exit currently points to an error BUT the target room is
                                // now visible, we need to remove the error pt and point to the good room
                                if (Room[i].Exits[j].Transfer < 0 && Room[Index].Visible) {
                                    // error pts never have transfers so we can
                                    // just set the transfer value to zero and it should
                                    // force the SetExitPos function to correctly relocate the
                                    // exit
                                    // remove the errpt
                                    CompactObjList(ErrPt[-Room[i].Exits[j].Transfer].Order);
                                    ErrPt[-Room[i].Exits[j].Transfer].Visible = false;
                                    ErrPt[-Room[i].Exits[j].Transfer].ExitID = "";
                                    ErrPt[-Room[i].Exits[j].Transfer].FromRoom = 0;
                                    ErrPt[-Room[i].Exits[j].Transfer].Room = 0;
                                    ErrPt[-Room[i].Exits[j].Transfer].Order = 0;
                                    Room[i].Exits[j].Transfer = 0;
                                }

                                // reposition exit starting and ending points
                                SetExitPos(i, j);
                            }
                        }
                    }
                }
            }

            // if loading, don't mark as changed
            if (!loading) {
                // set changed flag
                MarkAsChanged();
            }
        }

        private void SetExitPos(int Index, int exitID) {
            SetExitPos(Index, Room[Index].Exits[exitID].ID);
        }

        private void SetExitPos(int Index, string exitID) {
            // this method recalculates the starting point and ending point
            // of the exit defined by room(index) and exitID=id

            // if a transpt is involved, both segments will be updated
            // tp0 is the endpoint associated with the segment between from room and its transfer pt circle
            // tp1 is the endpoint associated with the segment between to room and its transfer pt circle

            PointF TP0 = new(0, 0), TP1 = new(0, 0);
            float DX, DY, DL;

            AGIExit exit = Room[Index].Exits[exitID];
            // transfer point Value is used a lot; local copy
            int lngTP = exit.Transfer;

            // if this exit is part of a transfer
            if (lngTP > 0) {
                // determine which room is room1
                if (exit.Leg == 0) {
                    // first segment is associated with from room
                    // initially point to center of transpt
                    TP0.X = TransPt[lngTP].Loc[0].X + RM_SIZE / 4;
                    TP0.Y = TransPt[lngTP].Loc[0].Y + RM_SIZE / 4;
                    TP1.X = TransPt[lngTP].Loc[1].X + RM_SIZE / 4;
                    TP1.Y = TransPt[lngTP].Loc[1].Y + RM_SIZE / 4;
                }
                else {
                    // second segment is associated with from room
                    // initially point to center of transpt
                    TP0.X = TransPt[lngTP].Loc[1].X + RM_SIZE / 4;
                    TP0.Y = TransPt[lngTP].Loc[1].Y + RM_SIZE / 4;
                    TP1.X = TransPt[lngTP].Loc[0].X + RM_SIZE / 4;
                    TP1.Y = TransPt[lngTP].Loc[0].Y + RM_SIZE / 4;
                }
            }

            // begin with starting point at default coordinates of from room
            exit.SP = Room[Index].Loc;
            // set default coordinates of to room
            if (lngTP < 0) {
                // adjust end point to default coordinates of the err pt
                exit.EP = ErrPt[-lngTP].Loc;
            }
            else {
                // if hidden,
                if (exit.Hidden) {
                    switch (exit.Reason) {
                    case EEReason.erLeft:
                        exit.EP = new(exit.SP.X - RM_SIZE * 1.5f, exit.SP.Y);
                        break;
                    case EEReason.erRight:
                        exit.EP = new(exit.SP.X + RM_SIZE * 1.5f, exit.SP.Y);
                        break;
                    case EEReason.erBottom:
                        exit.EP = new(exit.SP.X, exit.SP.Y + RM_SIZE * 1.5f);
                        break;
                    case EEReason.erHorizon:
                        exit.EP = new(exit.SP.X, exit.SP.Y - RM_SIZE * 1.5f);
                        break;
                    case EEReason.erOther:
                        exit.EP = new(exit.SP.X - RM_SIZE * 1.5f, exit.SP.Y - RM_SIZE * 1.5f);
                        break;
                    }
                }
                else {
                    // set end point to default coordinates of to room
                    exit.EP = Room[exit.Room].Loc;
                }
            }
            // error points always go from center or room to center of error point
            // (no transfers allowed for error points)
            if (lngTP < 0) {
                // TO uses center of error point
                exit.EP.X += 0.3f;
                exit.EP.Y += .1732f;
                switch (exit.Reason) {
                case EEReason.erHorizon:
                    // FROM uses middle-top of room; TO uses middle-bottom
                    exit.SP.X += RM_SIZE / 2;
                    break;
                case EEReason.erRight:
                    // FROM room uses center-right; TO uses center-left
                    exit.SP.X += RM_SIZE;
                    exit.SP.Y = exit.SP.Y + RM_SIZE / 2;
                    break;
                case EEReason.erBottom:
                    // FROM room uses middle-bottom; TO uses middle-top
                    exit.SP.X += RM_SIZE / 2;
                    exit.SP.Y += RM_SIZE;
                    break;
                case EEReason.erLeft:
                    // FROM room uses center-left; TO uses center-right
                    exit.SP.Y += RM_SIZE / 2;
                    break;
                case EEReason.erOther:
                    // FROM room uses center
                    exit.SP.X += RM_SIZE / 2;
                    exit.SP.Y += RM_SIZE / 2;
                    // need to move line ends to edge of room
                    DX = exit.EP.X - exit.SP.X;
                    DY = exit.EP.Y - exit.SP.Y;
                    if (Math.Abs(DX) > Math.Abs(DY)) {
                        // line is mostly horizontal
                        exit.SP.X += Math.Sign(DX) * RM_SIZE / 2;
                        exit.SP.Y += Math.Sign(DX) * RM_SIZE / 2 * DY / DX;
                    }
                    else {
                        exit.SP.Y += Math.Sign(DY) * RM_SIZE / 2;
                        exit.SP.X += Math.Sign(DY) * RM_SIZE / 2 * DX / DY;
                    }
                    break;
                }
                // call custom edge-finding function
                PointF[] errPoints =
                [
                    new PointF(ErrPt[-lngTP].Loc.X,
                                ErrPt[-lngTP].Loc.Y),
                                new PointF(ErrPt[-lngTP].Loc.X + 0.6f,
                                ErrPt[-lngTP].Loc.Y),
                                new PointF(ErrPt[-lngTP].Loc.X + 0.3f,
                                ErrPt[-lngTP].Loc.Y + 0.5196f)
                ];
                using GraphicsPath path = new GraphicsPath();
                float radius = 0.125f; // radius for rounded corners (5.0 / 40)
                                       // Create rounded triangle path
                path.AddArc(errPoints[0].X + radius * 0.366f, errPoints[0].Y, radius, radius, 150, 120);
                path.AddArc(errPoints[1].X - radius * 1.366f, errPoints[1].Y, radius, radius, 270, 120);
                path.AddArc(errPoints[2].X - radius * 0.5f, errPoints[2].Y - radius * 1.5f, radius, radius, 30, 120);
                path.CloseFigure();
                PointF? intersection = FindIntersection(path, exit.SP, exit.EP);
                if (intersection.HasValue) {
                    exit.EP = intersection.Value;
                }
                else {
                    // should never be null, but if it is, just keep the center point
                    Debug.Assert(false);
                }
            }
            else {
                // adjust FROM-point and TO-point based on exit Type
                switch (exit.Reason) {
                case EEReason.erHorizon:
                    // FROM uses middle-top of room; TO uses middle-bottom
                    exit.SP.X += RM_SIZE / 2;
                    exit.EP.X += RM_SIZE / 2;
                    exit.EP.Y += RM_SIZE;
                    TP0.Y += RM_SIZE / 4;
                    TP1.Y -= RM_SIZE / 4;
                    break;
                case EEReason.erRight:
                    // FROM room uses center-right; TO uses center-left
                    exit.SP.X += RM_SIZE;
                    exit.SP.Y += RM_SIZE / 2;
                    exit.EP.Y += RM_SIZE / 2;
                    TP0.X -= RM_SIZE / 4;
                    TP1.X += RM_SIZE / 4;
                    break;
                case EEReason.erBottom:
                    // FROM room uses middle-bottom; TO uses middle-top
                    exit.SP.X += RM_SIZE / 2;
                    exit.SP.Y += RM_SIZE;
                    exit.EP.X += RM_SIZE / 2;
                    TP0.Y -= RM_SIZE / 4;
                    TP1.Y += RM_SIZE / 4;
                    break;
                case EEReason.erLeft:
                    // FROM room uses center-left; TO uses center-right
                    exit.SP.Y += RM_SIZE / 2;
                    exit.EP.X += RM_SIZE;
                    exit.EP.Y += RM_SIZE / 2;
                    TP0.X += RM_SIZE / 4;
                    TP1.X -= RM_SIZE / 4;
                    break;
                case EEReason.erOther:
                    if (lngTP == 0) {
                        // no transfer point; draw directly to/FROM rooms
                        // start at center
                        exit.EP.X += RM_SIZE / 2;
                        exit.EP.Y += RM_SIZE / 2;
                        exit.SP.X += RM_SIZE / 2;
                        exit.SP.Y += RM_SIZE / 2;
                        // calculate distances
                        DX = exit.EP.X - exit.SP.X;
                        DY = exit.EP.Y - exit.SP.Y;
                        // if end point and start point are same (meaning room is a loop?)
                        if (DX == 0 && DY == 0) {
                            // adjust x values to make line draw straight across room
                            exit.EP.X += RM_SIZE;
                            exit.SP.X -= RM_SIZE;
                            DX = RM_SIZE / 4;
                        }
                        // adjust endpoints based on slope
                        if (DX == 0) {
                            // vertical line; move to right
                            exit.SP.X += RM_SIZE / 4;
                            exit.EP.X += RM_SIZE / 4;
                            if (DY > 0) {
                                exit.SP.Y += RM_SIZE / 4;
                                exit.EP.Y -= RM_SIZE / 4;
                            }
                            else {
                                exit.SP.Y -= RM_SIZE / 4;
                                exit.EP.Y += RM_SIZE / 4;
                            }

                        }
                        else if (DY == 0) {
                            // horizontal line; move down
                            exit.SP.Y += RM_SIZE / 4;
                            exit.EP.Y += RM_SIZE / 4;
                            if (DX > 0) {
                                exit.SP.X += RM_SIZE / 4;
                                exit.EP.X -= RM_SIZE / 4;
                            }
                            else {
                                exit.SP.X -= RM_SIZE / 4;
                                exit.EP.X += RM_SIZE / 4;
                            }
                        }
                        else {
                            // essentially diagonal; move points
                            // to within .25 units of nearest
                            // horizontal and nearest vertical edge
                            if (DX > 0) {
                                exit.SP.X += RM_SIZE / 4;
                                exit.EP.X -= RM_SIZE / 4;
                            }
                            else {
                                exit.SP.X -= RM_SIZE / 4;
                                exit.EP.X += RM_SIZE / 4;
                            }
                            if (DY > 0) {
                                exit.SP.Y += RM_SIZE / 4;
                                exit.EP.Y -= RM_SIZE / 4;
                            }
                            else {
                                exit.SP.Y -= RM_SIZE / 4;
                                exit.EP.Y += RM_SIZE / 4;
                            }
                        }

                        // if new dx Value is different from original
                        if (Math.Sign(DX) != Math.Sign(exit.EP.X - exit.SP.X)) {
                            if (DY < 0) {
                                // move point in direction of sgn x
                                exit.EP.X += RM_SIZE / 2 * Math.Sign(DX);
                            }
                            else {
                                exit.SP.X -= RM_SIZE / 2 * Math.Sign(DX);
                            }
                        }
                        if (Math.Sign(DY) != Math.Sign(exit.EP.Y - exit.SP.Y)) {
                            if (DX < 0) {
                                exit.EP.Y += RM_SIZE / 2 * Math.Sign(DY);
                            }
                            else {
                                exit.SP.Y -= RM_SIZE / 2 * Math.Sign(DY);
                            }
                        }

                        // recalculate distances
                        DX = exit.EP.X - exit.SP.X;
                        DY = exit.EP.Y - exit.SP.Y;
                        Debug.Assert(DX != 0 || DY != 0);

                        // now move lines to edge
                        if (Math.Abs(DX) > Math.Abs(DY)) {
                            // line is mostly horizontal
                            exit.SP.X += Math.Sign(DX) * RM_SIZE / 4;
                            exit.EP.X -= Math.Sign(DX) * RM_SIZE / 4;
                            exit.SP.Y += Math.Sign(DX) * RM_SIZE / 4 * DY / DX;
                            exit.EP.Y -= Math.Sign(DX) * RM_SIZE / 4 * DY / DX;
                        }
                        else {
                            // line is mostly vertical
                            exit.SP.Y += Math.Sign(DY) * RM_SIZE / 4;
                            exit.EP.Y -= Math.Sign(DY) * RM_SIZE / 4;
                            exit.SP.X += Math.Sign(DY) * RM_SIZE / 4 * DX / DY;
                            exit.EP.X -= Math.Sign(DY) * RM_SIZE / 4 * DX / DY;
                        }
                    }
                    else {
                        // draw from center, but line starts at object edge
                        // (draw two separate lines)

                        // first segment is from starting point to tp0:
                        // use x and y distances to determine how to adjust endpoints
                        // adjust to account for 1/2 of room width, since sp is currently on center of room and tp is at upper left corner
                        DX = TP0.X - exit.SP.X - RM_SIZE / 2;
                        DY = TP0.Y - exit.SP.Y - RM_SIZE / 2;
                        Debug.Assert(DX * DY != 0);

                        // if transpt is to right of starting point
                        if (DX > 0) {
                            // move starting point to right so it is 1/4 away from right edge
                            exit.SP.X += 3 * RM_SIZE / 4;
                        }
                        else {
                            // move it to left so it is .2 from left edge
                            exit.SP.X += RM_SIZE / 4;
                        }
                        // move point up/down to within .2 of edge in similar manner
                        if (DY >= 0) {
                            exit.SP.Y += 3 * RM_SIZE / 4;
                        }
                        else {
                            exit.SP.Y += RM_SIZE / 4;
                        }
                        // starting point is now .2 in from nearest corner;
                        // recalculate distances
                        DX = TP0.X - exit.SP.X;
                        DY = TP0.Y - exit.SP.Y;
                        DL = (float)Math.Sqrt(DX * DX + DY * DY);
                        if (DL == 0) {
                            // force apart
                            TP0.X += 0.1f;
                            exit.SP.X -= 0.1f;
                            DX = RM_SIZE / 4;
                            DL = RM_SIZE / 4;
                        }

                        if (Math.Abs(DX) > Math.Abs(DY)) {
                            // mostly horizontal- x distance will be edge of room
                            exit.SP.X += Math.Sign(DX) * RM_SIZE / 4;
                            // calculate y Value that corresponds to x Value
                            exit.SP.Y += Math.Sign(DX) * RM_SIZE / 4 * DY / DX;
                        }
                        else {
                            // mostly vertical- y distance will be edge of room
                            exit.SP.X += Math.Sign(DY) * RM_SIZE / 4 * DX / DY;
                            // calculate x Value that corresponds to y Value
                            exit.SP.Y += Math.Sign(DY) * RM_SIZE / 4;
                        }
                        // move transpt end along line proportionately so it
                        // is on circumference of circle
                        TP0.X -= RM_SIZE / 4 * DX / DL;
                        TP0.Y -= RM_SIZE / 4 * DY / DL;

                        // now repeat all this for the line between tp1 and end point
                        DX = exit.EP.X - TP1.X + RM_SIZE / 2;
                        DY = exit.EP.Y - TP1.Y + RM_SIZE / 2;
                        Debug.Assert(DX * DY != 0);

                        if (DX > 0) {
                            exit.EP.X += RM_SIZE / 4;
                        }
                        else {
                            exit.EP.X += 3 * RM_SIZE / 4;
                        }
                        if (DY >= 0) {
                            exit.EP.Y += RM_SIZE / 4;
                        }
                        else {
                            exit.EP.Y += 3 * RM_SIZE / 4;
                        }
                        DX = exit.EP.X - TP1.X;
                        DY = exit.EP.Y - TP1.Y;
                        DL = (float)Math.Sqrt(DX * DX + DY * DY);
                        if (DL == 0) {
                            // force apart
                            TP1.X += 0.1f;
                            exit.EP.X -= 0.1f;
                            DX = RM_SIZE / 4;
                            DL = RM_SIZE / 4;
                        }
                        if (Math.Abs(DX) > Math.Abs(DY)) {
                            exit.EP.X -= Math.Sign(DX) * RM_SIZE / 4;
                            exit.EP.Y -= Math.Sign(DX) * RM_SIZE / 4 * DY / DX;
                        }
                        else {
                            exit.EP.X -= Math.Sign(DY) * RM_SIZE / 4 * DX / DY;
                            exit.EP.Y -= Math.Sign(DY) * RM_SIZE / 4;
                        }
                        TP1.X += RM_SIZE / 4 * DX / DL;
                        TP1.Y += RM_SIZE / 4 * DY / DL;
                    }
                    break;
                }
            }

            // if there are  transfer points
            if (lngTP > 0) {
                // if first leg,
                if (exit.Leg == 0) {
                    // copy transpt exit starting/ending point
                    TransPt[lngTP].SP = TP0;
                    TransPt[lngTP].EP = TP1;
                }
                else {
                    // starting point of line is actually associated with end point of transpt
                    // copy transpt exit starting/ending point
                    TransPt[lngTP].SP = TP1;
                    TransPt[lngTP].EP = TP0;
                }
            }
        }

        public static PointF? FindIntersection(GraphicsPath path, PointF p1, PointF p2) {
            var points = path.PathPoints;
            PointF a = points[0];
            PointF b = points[^1];
            if (LineSegmentsIntersect(p1, p2, a, b, out PointF intersection))
                return intersection;
            for (int i = 1; i < points.Length; i++) {
                a = points[i - 1];
                b = points[i];
                if (LineSegmentsIntersect(p1, p2, a, b, out intersection))
                    return intersection;
            }
            return null;
        }

        private static bool LineSegmentsIntersect(PointF p, PointF p2, PointF q, PointF q2, out PointF intersection) {
            // Returns true if segments (p, p2) and (q, q2) intersect, and sets intersection point
            intersection = new PointF();

            float s1_x = p2.X - p.X;
            float s1_y = p2.Y - p.Y;
            float s2_x = q2.X - q.X;
            float s2_y = q2.Y - q.Y;

            float denom = (-s2_x * s1_y + s1_x * s2_y);
            if (denom == 0) return false; // Parallel

            float s = (-s1_y * (p.X - q.X) + s1_x * (p.Y - q.Y)) / denom;
            float t = (s2_x * (p.Y - q.Y) - s2_y * (p.X - q.X)) / denom;

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1) {
                // Intersection detected
                intersection = new PointF(p.X + (t * s1_x), p.Y + (t * s1_y));
                return true;
            }

            return false; // No intersection
        }

        private bool IsBehind(PointF TestLoc, SizeF TestSize, PointF TopLoc, SizeF TopSize) {

            // if the test object (defined by its location and size)
            // is completely behind the Top object, this function returns true

            // if test is left of top, it is NOT behind
            if (TestLoc.X < TopLoc.X) {
                return false;
            }
            // if testis above top, it is NOT behind
            if (TestLoc.Y < TopLoc.Y) {
                return false;
            }
            // if test is right of top, it is NOT behind
            if (TestLoc.X + TestSize.Width > TopLoc.X + TopSize.Width) {
                return false;
            }
            // if test is below top, it is NOT behind
            if (TestLoc.Y + TestSize.Height > TopLoc.Y + TopSize.Height) {
                return false;
            }
            // if it passes all tests, Test is completely behind Top
            return true;
        }

        private PointF GetInsertPos(float X, float Y, int Group = 0, float Distance = 1, bool SkipStart = false) {
            // finds one of sixteen empty points around the point X, Y by
            // checking all Rooms, all transfer points, and all error markers
            //
            // if all spots are taken, then it returns original values
            //
            // the spots, in order of search are
            //
            //           04 62
            //           8C EA
            //             X
            //           9D FB
            //           15 73
            //
            // if SkipStart is true, the starting point (x) is automatically passed over
            // whether something is there or not

            // round off distance
            // Distance = GridPos(Distance)
            // can't be zero
            if (Distance < 0.1) {
                Distance = 0.1f;
            }
            // TODO: non-loading check needs to be handled differently-
            // return a position that doesn't hide any objects

            // if starting position is available
            if (!SkipStart && !ItemAtPoint(X, Y, Group)) {
                //return starting point
                return new PointF(GridPos(X), GridPos(Y));
            }
            // start at position 0
            int Pos = 0;

            do {
                // determine x and Y offset for position:
                float tmpX = X + (Pos / 4 % 2 == 1 ? Distance / 2 : Distance) * ((Pos & 2) - 1);
                float tmpY = Y + (Pos / 8 == 0 ? Distance : Distance / 2) * ((Pos & 1) * 2 - 1);
                // if any items at this location
                if (!ItemAtPoint(tmpX, tmpY, Group)) {
                    // nothing found that occupies this place; return it
                    return new PointF(tmpX, tmpY);
                }
                // try next position
                Pos++;
            } while (Pos < 16);

            // all spaces occupied- recurse around first position
            //return GetInsertPos(X - Distance, Y - Distance, Group, Distance, SkipStart);
            return new PointF(X, Y);
        }

        private bool ItemAtPoint(float X, float Y, int Group = 0) {
            // used by the extract layout method
            // returns true if there is an item at this location

            // search rooms
            for (int i = 1; i < 256; i++) {
                if (Room[i].Visible) {
                    if (Room[i].Loc.X == X && Room[i].Loc.Y == Y) {
                        if (Group == 0 || ELRoom[i].Group == Group) {
                            return true;
                        }
                    }
                }
            }
            // search transfer points
            for (int i = 1; i < 256; i++) {
                if (TransPt[i].Count > 0) {
                    if ((TransPt[i].Loc[0].X == X && TransPt[i].Loc[0].Y == Y) ||
                (TransPt[i].Loc[1].X == X && TransPt[i].Loc[1].Y == Y)) {
                        if (Group == 0 || ELRoom[i].Group == Group) {
                            return true;
                        }
                    }
                }
            }
            // search err points
            for (int i = 1; i < 256; i++) {
                if (ErrPt[i].Visible) {
                    if (ErrPt[i].Loc.X == X && ErrPt[i].Loc.Y == Y) {
                        if (Group == 0 || ELRoom[i].Group == Group) {
                            return true;
                        }
                    }
                }
            }
            // nothing here
            return false;
        }

        private float GridPos(float Pos) {
            // sets pos to align with grid
            if (WinAGISettings.LEUseGrid.Value) {
                return (float)(Math.Round(Pos / WinAGISettings.LEGridMinor.Value, 0) * WinAGISettings.LEGridMinor.Value);
            }
            else {
                return Pos;
            }
        }

        private void CompactObjList(int ObjNum) {
            // removes and object and adjusts order numbers
            ObjOrder.RemoveAt(ObjNum);
            for (int i = ObjNum; i < ObjOrder.Count; i++) {
                switch (ObjOrder[i].Type) {
                case ELSelection.lsRoom:
                    Room[ObjOrder[i].Number].Order = i;
                    break;
                case ELSelection.lsTransPt:
                    TransPt[ObjOrder[i].Number].Order = i;
                    break;
                case ELSelection.lsErrPt:
                    ErrPt[ObjOrder[i].Number].Order = i;
                    break;
                case ELSelection.lsComment:
                    Comment[ObjOrder[i].Number].Order = i;
                    break;
                }
            }
        }

        internal void SaveLayout() {
            // when closing, this method writes the updated layout info to file
            string output = "";
            JsonSerializerOptions jOptions = new JsonSerializerOptions { WriteIndented = true };
            bool error = false;

            // add default  data first
            LayoutFileHeader layoutfile = new();
            layoutfile.Version = LAYOUT_FMT_VERSION;
            layoutfile.DrawScale = DrawScale;
            layoutfile.Offset = Offset;
            output = JsonSerializer.Serialize(layoutfile, jOptions);
            try {
                using FileStream fs = new(EditGame.GameDir + EditGame.GameID + ".wal", FileMode.Create, FileAccess.Write);
                AddToFile(output);
                if (error) {
                    return;
                }

                // add the objects
                foreach (ObjInfo obj in ObjOrder) {
                    int i = obj.Number;
                    switch (obj.Type) {
                    case ELSelection.lsRoom:
                        if (Room[i].Visible) {
                            // R|##|v|o|p|x|y|
                            LFDRoom room = new();
                            room.Index = i;
                            room.Visible = true;
                            room.ShowPic = Room[i].ShowPic;
                            room.Loc = Room[i].Loc;
                            // determine if any exits need updating
                            for (int j = 0; j < Room[i].Exits.Count; j++) {
                                if (Room[i].Exits[j].Status != EEStatus.esOK) {
                                    // update all exits for this logic
                                    UpdateLogicCode(EditGame.Logics[i]);
                                    break;
                                }
                            }
                            // add the exits
                            room.Exits = Room[i].Exits.ToArray();
                            // add the room to the layout file
                            output = '\x01' + JsonSerializer.Serialize(room as LayoutFileData, jOptions);
                            AddToFile(output);
                            if (error) {
                                return;
                            }
                        }
                        break;
                    case ELSelection.lsTransPt:
                        if (TransPt[i].Count > 0) {
                            // add transfer pt
                            LFDTransPt transpt = new();
                            transpt.Index = i;
                            transpt.Visible = true;
                            transpt.Loc = TransPt[i].Loc;
                            transpt.Rooms = [TransPt[i].Room[0], TransPt[i].Room[1]];
                            transpt.ExitID = TransPt[i].ExitID;
                            transpt.Count = TransPt[i].Count;
                            // add the room to the layout file
                            output = '\x01' + JsonSerializer.Serialize(transpt as LayoutFileData, jOptions);
                            AddToFile(output);
                            if (error) {
                                return;
                            }
                        }
                        break;
                    case ELSelection.lsErrPt:
                        if (ErrPt[i].Visible) {
                            // add errpt
                            LFDErrPt errpt = new();
                            errpt.Index = i;
                            errpt.Visible = true;
                            errpt.Loc = ErrPt[i].Loc;
                            errpt.ExitID = ErrPt[i].ExitID;
                            errpt.FromRoom = ErrPt[i].FromRoom;
                            errpt.Room = ErrPt[i].Room;
                            // add the room to the layout file
                            output = '\x01' + JsonSerializer.Serialize(errpt as LayoutFileData, jOptions);
                            AddToFile(output);
                            if (error) {
                                return;
                            }
                        }
                        break;
                    case ELSelection.lsComment:
                        if (Comment[i].Visible) {
                            // C|##|v|o|x|y|h|w|{text}
                            LFDComment comment = new();
                            comment.Index = i;
                            comment.Visible = true;
                            comment.Loc = Comment[i].Loc;
                            comment.Size = Comment[i].Size;
                            comment.Text = Comment[i].Text;
                            // add the room to the layout file
                            output = '\x01' + JsonSerializer.Serialize(comment as LayoutFileData, jOptions);
                            AddToFile(output);
                            if (error) {
                                return;
                            }
                        }
                        break;
                    }
                }

                // done with the streamwriter
                fs.Dispose();
                void AddToFile(string text) {
                    // write the object as text to the file
                    try {
                        fs.Write(Encoding.Default.GetBytes(text));
                    }
                    catch (Exception e) {
                        ErrMsgBox(e, "Unable to save layout due to file error.", "", "Save Layout File Error");
                    }
                }
            }
            catch (Exception e) {
                error = true;
                ErrMsgBox(e, "Unable to save layout due to file error.", "", "Save Layout File Error");
            }

            // if a logic is being previewed, update selection, in case the logic is changed
            if (SelResType == AGIResType.Logic) {
                RefreshTree(AGIResType.Logic, SelResNum);
            }
            MarkAsSaved();
        }

        private bool AskClose() {
            //if () {
            //    // if exiting due to error on load
            //    return true;
            //}
            if (IsChanged) {
                DialogResult rtn = MessageBox.Show(MDIMain,
                    "Do you want to save changes to this layout?",
                    "Save Layout",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (rtn) {
                case DialogResult.Yes:
                    SaveLayout();
                    if (IsChanged) {
                        rtn = MessageBox.Show(MDIMain,
                            "Layout not saved. Continue closing anyway?",
                            "Save Layout",
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
            if (!IsChanged) {
                IsChanged = true;
                MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = true;
                Text = sDM + Text;
            }
        }

        private void MarkAsSaved() {
            IsChanged = false;
            Text = EditGame.GameID + " - Room Layout";
            MDIMain.toolStrip1.Items["btnSaveResource"].Enabled = false;
        }

        #endregion
    }

    // need to subclass the scrollbars to allow scrolling past the edges...
    public class HScrollBarMouseAware : HScrollBar {
        public new event MouseEventHandler MouseDown;

        protected override void WndProc(ref Message m) {
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONDBLCLK = 0x0203;
            const int WM_RBUTTONDOWN = 0x0204;
            const int WM_RBUTTONDBLCLK = 0x0206;
            const int WM_MBUTTONDOWN = 0x0207;
            const int WM_MBUTTONDBLCLK = 0x0209;

            if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN || m.Msg == WM_MBUTTONDOWN ||
                m.Msg == WM_LBUTTONDBLCLK || m.Msg == WM_RBUTTONDBLCLK || m.Msg == WM_MBUTTONDBLCLK) {
                MouseButtons button = MouseButtons.Left;
                if (m.Msg == WM_RBUTTONDOWN || m.Msg == WM_RBUTTONDBLCLK) button = MouseButtons.Right;
                if (m.Msg == WM_MBUTTONDOWN || m.Msg == WM_MBUTTONDBLCLK) button = MouseButtons.Middle;

                int x = (short)(m.LParam.ToInt32() & 0xFFFF);
                int y = (short)((m.LParam.ToInt32() >> 16) & 0xFFFF);

                MouseDown?.Invoke(this, new MouseEventArgs(button, 1, x, y, 0));
            }

            base.WndProc(ref m);

        }
    }

    public class VScrollBarMouseAware : VScrollBar {
        public new event MouseEventHandler MouseDown;

        protected override void WndProc(ref Message m) {
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONDBLCLK = 0x0203;
            const int WM_RBUTTONDOWN = 0x0204;
            const int WM_RBUTTONDBLCLK = 0x0206;
            const int WM_MBUTTONDOWN = 0x0207;
            const int WM_MBUTTONDBLCLK = 0x0209;

            if (m.Msg == WM_LBUTTONDOWN || m.Msg == WM_RBUTTONDOWN || m.Msg == WM_MBUTTONDOWN ||
                m.Msg == WM_LBUTTONDBLCLK || m.Msg == WM_RBUTTONDBLCLK || m.Msg == WM_MBUTTONDBLCLK) {
                MouseButtons button = MouseButtons.Left;
                if (m.Msg == WM_RBUTTONDOWN || m.Msg == WM_RBUTTONDBLCLK) button = MouseButtons.Right;
                if (m.Msg == WM_MBUTTONDOWN || m.Msg == WM_MBUTTONDBLCLK) button = MouseButtons.Middle;

                int x = (short)(m.LParam.ToInt32() & 0xFFFF);
                int y = (short)((m.LParam.ToInt32() >> 16) & 0xFFFF);

                MouseDown?.Invoke(this, new MouseEventArgs(button, 1, x, y, 0));
            }

            base.WndProc(ref m);
        }
    }

    public class LayoutFileHeader {
        public string Name { get; set; } = "WinAGI Layout File";
        public string Version { get; set; }
        public int DrawScale { get; set; }
        public Point Offset { get; set; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(LFDRoom), "Room")]
    [JsonDerivedType(typeof(LFDTransPt), "TransPt")]
    [JsonDerivedType(typeof(LFDErrPt), "ErrPt")]
    [JsonDerivedType(typeof(LFDComment), "Comment")]
    [JsonDerivedType(typeof(LFDRenumber), "Renumber")]
    [JsonDerivedType(typeof(LFDUpdate), "Update")]
    [JsonDerivedType(typeof(LFDShowRoom), "Show")]
    [JsonDerivedType(typeof(LFDHideRoom), "Hide")]
    public abstract class LayoutFileData {
        public int Index { get; set; }
        public bool Visible { get; set; }
        public LayoutFileData() { }
    }
    public class LFDRoom : LayoutFileData {
        public bool ShowPic { get; set; }
        public PointF Loc { get; set; }
        public AGIExit[] Exits { get; set; }
        public LFDRoom() { }
    }
    public class LFDTransPt : LayoutFileData {
        public PointF[] Loc { get; set; }
        public int[] Rooms { get; set; }
        public string[] ExitID { get; set; }
        public int Count { get; set; }
        public LFDTransPt() { }
    }
    public class LFDErrPt : LayoutFileData {
        public PointF Loc { get; set; }
        public string ExitID { get; set; }
        public int FromRoom { get; set; }
        public int Room { get; set; }
        public LFDErrPt() { }
    }
    public class LFDComment : LayoutFileData {
        public PointF Loc { get; set; }
        public SizeF Size { get; set; }
        public string Text { get; set; }
        public LFDComment() { }
    }
    public class LFDRenumber : LayoutFileData {
        public int OldNumber { get; set; }
        public LFDRenumber() { }
    }
    public class LFDUpdate : LayoutFileData {
        public bool ShowPic { get; set; }
        public AGIExit[] Exits { get; set; }
        public LFDUpdate() { }
    }
    public class LFDShowRoom : LayoutFileData {
        public bool ShowPic { get; set; }
        public AGIExit[] Exits { get; set; }
        public LFDShowRoom() { }
    }
    public class LFDHideRoom : LayoutFileData {
        public LFDHideRoom() { }
    }
}
