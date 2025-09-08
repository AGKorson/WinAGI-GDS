using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinAGI.Common;
using WinAGI.Engine;
using static WinAGI.Common.API;
using static WinAGI.Editor.Base;
using static WinAGI.Editor.WinAGIFCTB;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using FastColoredTextBoxNS;

namespace WinAGI.Editor {
    public partial class frmLayout : Form {
        #region Enums
        /// <summary>
        /// Enum to indicate the cursor currently in use.
        /// </summary>
        private enum LayoutCursor {
            Default,
            SizeNESW,
            SizeNWSE,
            IBeam,
            Crosshair,
            NoDrop,
            MoveSelection,
            Moving,
            Select,
            AddObject,
            HorizonExit,
            BottomExit,
            RightExit,
            LeftExit,
            OtherExit,
            DragSurface,
        }
        #endregion

        #region Constants
        private const int MGN = 12;
        private const float RM_SIZE = 0.8f;
        private const float TRANSPT_SIZE = 0.4f;
        private const float MIN_CMT_SZ = 0.2f;
        internal const string LAYOUT_FMT_VERSION = "3.0";
        #endregion

        #region Structures
        /// <summary>
        /// Used by layout extraction routine to track exits and entries.
        /// </summary>
        private struct ELInfo {
            public bool Analyzed;
            public bool Placed;
            public byte Group;
            public byte[] Exits;
            public byte[] Enter;
            public ELInfo() {
                Exits = new byte[4];
                Enter = new byte[4];
            }
        }

        private struct RoomInfo {
            /// <summary>
            /// Location of the upper-left corner of the room in layout coordinates.
            /// </summary>
            public PointF Loc;
            /// <summary>
            /// If True, error point box will be drawn on layout.
            /// </summary>
            public bool Visible;
            /// <summary>
            /// Position of this error point in the draw order.
            /// </summary>
            public int Order;
            /// <summary>
            /// If True, the room box will include a scaled image of room's 
            /// associated picture.
            /// </summary>
            public bool ShowPic;
            public AGIExits Exits = new();

            public RoomInfo() {
            }
        }
        
        /// <summary>
        /// NOTE: no exits can ever originate from an error point
        /// and only one exit can point to a single error point.
        /// </summary>
        private struct ErrorPtInfo {
            /// <summary>
            /// Location of the upper-left corner of the error point in layout coordinates.
            /// </summary>
            public PointF Loc;
            /// <summary>
            /// If True, error point box will be drawn on layout.
            /// </summary>
            public bool Visible;
            /// <summary>
            /// Position of this error point in the draw order.
            /// </summary>
            public int Order;
            /// <summary>
            /// ID of the exit that points to this error point.
            /// </summary>
            public string ExitID;
            /// <summary>
            /// Room number where the exit error is located (i.e. has the 
            /// bad new.room() command).
            /// </summary>
            public int FromRoom;
            /// <summary>
            /// The destinaton room number - a room that does not exist, or zero
            /// if the room value is not a valid number.    
            /// </summary>
            internal int Room;
        }
        
        private struct CommentInfo {
            /// <summary>
            /// Location of the upper-left corner of the comment box in layout coordinates.
            /// </summary>
            public PointF Loc;
            /// <summary>
            /// If True, comment box will be drawn on layout.
            /// </summary>
            public bool Visible;
            /// <summary>
            /// Position of this comment in the draw order.
            /// </summary>
            public int Order;
            /// <summary>
            /// Size of the comment box in layout coordinates.
            /// </summary>
            public SizeF Size;
            /// <summary>
            /// The text that is displayed in the comment box.
            /// </summary>
            public string Text;
        }
        
        private struct TransPtInfo {
            /// <summary>
            /// Location of the two transfer points in layout coordinates..
            /// </summary>
            public PointF[] Loc;
            /// <summary>
            /// Position of this transfer point in the draw order.
            /// </summary>
            public int Order;
            /// <summary>
            /// The number of exits that use this transfer point.
            /// </summary>
            public int Count;
            /// <summary>
            /// Point that connects the second leg of the transfer point
            /// to the end point of the exit.
            /// </summary>
            public PointF EP;
            /// <summary>
            /// Point that connects the first leg of the transfer point to 
            /// the start point of the exit.
            /// </summary>
            public PointF SP;
            /// <summary>
            /// The room(s) that this transfer point originates from.
            /// </summary>
            public byte[] Room;
            /// <summary>
            /// Exit ID(s) that use this transfer point.
            /// </summary>
            public string[] ExitID;
            public TransPtInfo() {
                Loc = new PointF[2];
                Loc[0] = new PointF(0, 0);
                Loc[1] = new PointF(0, 0);
                EP = new PointF(0, 0);
                SP = new PointF(0, 0);
                Room = new byte[2];
                ExitID = new string[2];
                ExitID[0] = "";
                ExitID[1] = "";
            }
        }
        private struct ObjInfo {
            /// <summary>
            /// Type of object (room, transpt, errpt, comment).
            /// </summary>
            public LayoutSelection Type;
            /// <summary>
            /// Index number of object.
            /// </summary>
            public int Number;
            /// <summary>
            /// Which leg of a transfer point is selected.
            /// </summary>
            public ExitLeg Leg;
            public ObjInfo(LayoutSelection type, int number) {
                Type = type;
                Number = number;
            }
            public ObjInfo(LayoutSelection type, int number, ExitLeg leg) {
                Type = type;
                Number = number;
                Leg = leg;
            }
        }
        private struct TSel {
            /// <summary>
            /// Type of object currently selected.
            /// </summary>
            public LayoutSelection Type;
            /// <summary>
            /// index of object selected or room associated with selected exit
            /// or count if multiple objects selected.
            /// </summary>
            public int Number;
            /// <summary>
            /// ID of selected exit.
            /// </summary>
            public string ExitID;
            /// <summary>
            /// Which leg of an exit that has transfer, or which transfer point
            /// is selected.
            /// </summary>
            public ExitLeg Leg = ExitLeg.NoTransPt;
            /// <summary>
            /// Which direction in a multi-direction exit is selected.
            /// </summary>
            public ExitDirection TwoWay;
            /// <summary>
            /// Coordinates of the selected object in layout coordinates.
            /// If multiple objects selected, this is the upper-left
            /// coordinates of the selection rectangle.
            /// </summary>
            public PointF Loc;
            /// <summary>
            /// Size of a multi-selection rectangle in layout coordinates.
            /// </summary>
            public SizeF Size;
            /// <summary>
            /// Start point of an exit line in layout coordinates.
            /// </summary>
            public PointF SP;
            /// <summary>
            /// End point of an exit line in layout coordinates.
            /// </summary>
            public PointF EP;

            public TSel() {
            }
        }
        #endregion

        #region Members
        public bool IsChanged = false;
        private Font layoutFont, transptFont;

        // layout object variables
        private RoomInfo[] Room;       // squares
        private TransPtInfo[] TransPt; // circles (always drawn in pairs)
        private ErrorPtInfo[] ErrPt;   // rounded corner triangles
        private CommentInfo[] Comment; // rounded corner rectangles

        // extraction variables
        private ELInfo[] ELRoom;

        // scale and display variables
        int DrawScale;
        private float DSF; // display scale factor - a multiplier value
                           // that converts layout coords to screen pixels
                           // DSF is equal to 40 * (1.25) ^ DrawSale
        private Point Offset;                // in screen pixels
        private PointF Min, Max;             // in layout coordinates
        private bool NoScrollUpdate = false; // if TRUE, don't update scrollbars
        private List<ObjInfo> ObjOrder = []; // display order of objects
        private int HandleSize;              // size of the selection handles in pixels

        // selection, drawing and moving variables
        private LayoutTool SelTool;
        private bool HoldTool = false;
        private bool AddPicToo = true;
        private LayoutCursor DrawCursor = LayoutCursor.Default;
        private TSel Selection;
        private List<ObjInfo> SelectedObjects = [];
        private Point MouseDownPt = new();          // used to track mouse down position
        private PointF AnchorPt = new();            // in layout coordinates
        private Point CanvasAnchor, Delta = new();  // in screen pixels
        private bool DragCanvas;
        private bool DragSelect, ShowDrag;
        // object move/size variables
        private bool MoveObj;
        private int SizingComment;
        private bool EditCommentText;
        // exit move/size variables
        private PointF LineStart = new(); // start point of exit line in layout coordinates
        private PointF LineEnd = new();   // end point of exit line in layout coordinates
        private ExitReason ShowExitStart = ExitReason.None;
        private bool MoveExit;
        private int MovePoint; // 0 = starting point of an exit is being moved
                               // 1 = ending point of an exit is being moved
        private int DrawExit;  // 0 = not drawing
                               // 1 = drawing normal exit
                               // 2 = drawing transpt exit
        private ExitReason NewExitReason;
        private int NewExitTrans, NewExitRoom;

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
            // initialize form controls
            InitializeComponent();
            picDraw.MouseWheel += picDraw_MouseWheel;
            hScrollBar1.MouseDown += hScrollBar1_MouseDown;
            vScrollBar1.MouseDown += vScrollBar1_MouseDown;
            txtComment.Parent = picDraw;
            InitStatusStrip();
            // set up form variables
            MdiParent = MDIMain;
            SelTool = LayoutTool.Select;
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

        private void frmLayout_KeyDown(object sender, KeyEventArgs e) {
            // if CTRL key alone, switch default to multiselect
            if (e.KeyData == (Keys.Control | Keys.ControlKey) && SelTool == LayoutTool.Select) {
                MultiSelectCursor(picDraw.PointToClient(Control.MousePosition));
            }
            // any CTRL+key combo cancels the multiselect
            else if (e.Control) {
                SingleSelectCursor(picDraw.PointToClient(Control.MousePosition));
            }
        }

        private void frmLayout_KeyUp(object sender, KeyEventArgs e) {

            // if CTRL key alone, restore selectmode to default
            if (e.KeyData == Keys.ControlKey) {
                SingleSelectCursor(picDraw.PointToClient(Control.MousePosition));
            }
        }
        #endregion

        #region Menu Event Handlers
        /// <summary>
        /// Configures the resource menu prior to displaying it.
        /// </summary>
        internal void SetResourceMenu() {
            mnuRSave.Enabled = IsChanged;
            // mnuRepair - always visible and enabled
            // mnuToggleAllPics always visible and enabled
            mnuToggleAllPics.Text = WinAGISettings.LEShowPics.Value ? "Hide All Pictures" : "Show All Pictures";
        }

        /// <summary>
        /// Resets all resource menu items so shortcut keys can work correctly.
        /// </summary>
        internal void ResetResourceMenu() {
            mnuRSave.Enabled = true;
        }

        internal void mnuRSave_Click(object sender, EventArgs e) {
            if (IsChanged) {
                SaveLayout();
            }
        }

        private void mnuRepair_Click(object sender, EventArgs e) {
            RepairLayout();
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
                mnuHideRoom,
                mnuEditLogic,
                mnuEditPicture,
                mnuDelete,
                mnuInsert,
                mnuSelectAll,
                mnuEditSep1,
                mnuOrder,
                mnuToggleTransPt,
                mnuTogglePicture,
                mnuProperties,
                mnuEditSep2,
                mnuToggleGrid]);
        }

        private void mnuEdit_DropDownClosed(object sender, EventArgs e) {
            contextMenuStrip1.Items.AddRange([
                mnuShowRoom,
                mnuHideRoom,
                mnuEditLogic,
                mnuEditPicture,
                mnuDelete,
                mnuInsert,
                mnuSelectAll,
                mnuEditSep1,
                mnuOrder,
                mnuToggleTransPt,
                mnuTogglePicture,
                mnuProperties,
                mnuEditSep2,
                mnuToggleGrid]);
            ResetEditMenu();
        }

        private void SetEditMenu() {
            // mnuShowRoom
            mnuShowRoom.Visible = Selection.Type == LayoutSelection.None;

            // mnuHideRoom
            mnuHideRoom.Visible = Selection.Type == LayoutSelection.Room;

            // mnuEditLogic
            mnuEditLogic.Enabled = Selection.Type == LayoutSelection.Room ||
                Selection.Type == LayoutSelection.Exit ||
                Selection.Type == LayoutSelection.ErrPt;

            // mnuEditPicture
            mnuEditPicture.Enabled = Selection.Type == LayoutSelection.Room && EditGame.Pictures.Contains(Selection.Number); ;

            // mnuDelete
            mnuDelete.Enabled = Selection.Type != LayoutSelection.None;
            switch (Selection.Type) {
            case LayoutSelection.Room:
                mnuDelete.Text = "Delete Room";
                break;
            case LayoutSelection.TransPt:
                mnuDelete.Text = "Delete Transfer Point";
                break;
            case LayoutSelection.ErrPt:
                mnuDelete.Text = "Delete Exit to Error";
                break;
            case LayoutSelection.Comment:
                mnuDelete.Text = "Delete Comment";
                break;
            case LayoutSelection.Exit:
                mnuDelete.Text = "Delete Exit";
                break;
            case LayoutSelection.Multiple:
                mnuDelete.Text = "Delete Selected Items";
                break;
            case LayoutSelection.None:
                mnuDelete.Text = "Delete";
                break;
            }
            // mnuInsert
            //    mnuInsertRoom
            //    mnuInsertTransfer
            //    mnuInsertComment
            // insertRoom only allowed if nothing selected
            mnuInsertRoom.Enabled = SelTool == LayoutTool.Select;

            // insert transfer is always visible, but only enabled for exits
            // that DONT already have a transfer
            mnuInsertTransfer.Enabled = (Selection.Type == LayoutSelection.Exit && Selection.Leg == ExitLeg.NoTransPt);

            // insertComment only allowed if nothing selected
            mnuInsertComment.Enabled = SelTool == LayoutTool.Select;

            // mnuSelectAll
            mnuSelectAll.Enabled = SelTool == LayoutTool.Select;

            // mnuOrder
            //    mnuOrderUp
            //    mnuOrderDown
            //    mnuOrderFront
            //    mnuOrderBack
            if (SelTool != LayoutTool.Select) {
                mnuOrderUp.Enabled = false;
                mnuOrderDown.Enabled = false;
                mnuOrderFront.Enabled = false;
                mnuOrderBack.Enabled = false;
            }
            else if (Selection.Type == LayoutSelection.Multiple) {
                // enable up, down, front, back
                mnuOrderUp.Enabled = true;
                mnuOrderDown.Enabled = true;
                mnuOrderFront.Enabled = true;
                mnuOrderBack.Enabled = true;
            }
            else {
                ObjInfo tmpSel = new(Selection.Type, Selection.Number);
                int order = ObjOrder.IndexOf(tmpSel);
                mnuOrderFront.Enabled = mnuOrderUp.Enabled = order != ObjOrder.Count - 1;
                mnuOrderBack.Enabled = mnuOrderDown.Enabled = order > 0;
            }

            // mnuToggleTransPt
            if (Selection.Type == LayoutSelection.TransPt) {
                mnuToggleTransPt.Visible = true;
                mnuToggleTransPt.Text = "Jump to Other Leg";
            }
            else if (Selection.Type == LayoutSelection.Exit && Selection.TwoWay == ExitDirection.OneWay) {
                mnuToggleTransPt.Visible = true;
                mnuToggleTransPt.Text = "Select Other Direction";
            }
            else {
                mnuToggleTransPt.Visible = false;
            }

            // mnuTogglePicture
            mnuTogglePicture.Enabled = Selection.Type == LayoutSelection.Room;
            if (Selection.Type == LayoutSelection.Room) {
                if (Room[Selection.Number].ShowPic) {
                    mnuTogglePicture.Text = "Hide Room Picture";
                }
                else {
                    mnuTogglePicture.Text = "Show Room Picture";
                }
            }
            else {
                mnuTogglePicture.Text = "Show Room Picture";
            }

            // mnuProperties
            mnuProperties.Enabled = Selection.Type == LayoutSelection.Room;

            // mnuToggleGrid
            mnuToggleGrid.Enabled = true;
            mnuToggleGrid.Text = WinAGISettings.LEShowGrid.Value ? "Hide Grid Lines" : "Show Grid Lines";
        }

        private void ResetEditMenu() {
            // Reset the Edit menu items to default state
            mnuShowRoom.Enabled = true;
            mnuHideRoom.Enabled = true;
            mnuEditLogic.Enabled = true;
            mnuEditPicture.Enabled = true;
            mnuDelete.Enabled = true;
            mnuInsertRoom.Enabled = true;
            mnuInsertTransfer.Enabled = true;
            mnuInsertComment.Enabled = true;
            mnuSelectAll.Enabled = true;
            mnuOrderUp.Enabled = true;
            mnuOrderDown.Enabled = true;
            mnuOrderFront.Enabled = true;
            mnuOrderBack.Enabled = true;
            mnuTogglePicture.Enabled = true;
            mnuToggleTransPt.Enabled = true;
            mnuProperties.Enabled = true;
            mnuToggleGrid.Enabled = true;
        }

        private void mnuShowRoom_Click(object sender, EventArgs e) {
            ShowRoom();
        }

        private void mnuHideRoom_Click(object sender, EventArgs e) {
            // hide this room

            // update logic
            EditGame.Logics[Selection.Number].IsRoom = false;

            // update resource tree
            RefreshTree(AGIResType.Logic, Selection.Number);

            // update the layout file
            UpdateLayoutFile(UpdateReason.HideRoom, EditGame.Logics[Selection.Number].Number, null);

            // hide room and redraw
            HideRoom(Selection.Number);
            DrawLayout();
        }

        private void mnuEditLogic_Click(object sender, EventArgs e) {
            switch (Selection.Type) {
            case LayoutSelection.Room:
            case LayoutSelection.ErrPt:
            case LayoutSelection.Exit:
                OpenRoomLogic(Selection);
                break;
            }
        }

        private void mnuEditPicture_Click(object sender, EventArgs e) {
            if (Selection.Type == LayoutSelection.Room) {
                if (EditGame.Pictures.Contains(Selection.Number)) {
                    try {
                        OpenGamePicture((byte)Selection.Number, true);
                    }
                    catch (Exception ex) {
                        // error
                        ErrMsgBox(ex, "Unable to open the picture.", "", "Picture Editor Load Error ");
                    }
                }
            }
        }

        private void mnuInsertRoom_Click(object sender, EventArgs e) {
            if (sender == mnuInsertRoom) {
                if (Selection.Type != LayoutSelection.None) {
                    DeselectObj();
                }
                PointF pos = GridPos(ScreenToLayout(new(
                    picDraw.ClientRectangle.Width / 2,
                    picDraw.ClientRectangle.Height / 2)));
                pos.X -= RM_SIZE / 2;
                pos.Y -= RM_SIZE / 2;
                pos = GetInsertPos(pos);
                AddNewRoom(pos);
                // go back to select tool
                btnAddRoom.Checked = false;
                btnSelect.Checked = true;
                SelTool = LayoutTool.Select;
                spTool.Text = "Tool: Select";
                HoldTool = false;
                picDraw.Cursor = Cursors.Default;
            }
            else {
                if (SelTool != LayoutTool.Room) {
                    btnSelect.Checked = false;
                    btnEdge1.Checked = false;
                    btnEdge2.Checked = false;
                    btnEdgeOther.Checked = false;
                    btnAddRoom.Checked = true;
                    btnAddComment.Checked = false;
                    DeselectObj();
                    SelTool = LayoutTool.Room;
                    SetCursor(LayoutCursor.Crosshair);
                }
                HoldTool = sender == btnAddRoom && ModifierKeys == Keys.Shift;
                spTool.Text = (HoldTool ? "HOLD" : "Tool") + ": Add Room";
            }
        }
        
        private void mnuInsertTransfer_Click(object sender, EventArgs e) {
            // inserts a set of transpts in the selected exit

            if (Selection.Type != LayoutSelection.Exit) {
                return;
            }
            Debug.Assert(Selection.Leg == ExitLeg.NoTransPt);
            if (Selection.Leg != ExitLeg.NoTransPt) {
                return;
            }
            InsertTransfer(ref Selection);
            DeselectObj();
            DrawLayout();
        }

        private void mnuInsertComment_Click(object sender, EventArgs e) {
            if (sender == mnuInsertComment) {
                if (Selection.Type != LayoutSelection.None) {
                    DeselectObj();
                }
                PointF pos = GridPos(ScreenToLayout(new(
                    picDraw.ClientRectangle.Width / 2,
                    picDraw.ClientRectangle.Height / 2)));
                pos.X -= RM_SIZE / 2;
                pos.Y -= RM_SIZE / 2;
                pos = GetInsertPos(pos);
                AddComment(pos, new(RM_SIZE * 2, RM_SIZE));
                // go back to select tool
                btnAddRoom.Checked = false;
                btnSelect.Checked = true;
                SelTool = LayoutTool.Select;
                spTool.Text = "Tool: Select";
                HoldTool = false;
                picDraw.Cursor = Cursors.Default;
            }
            else {
                if (SelTool != LayoutTool.Comment) {
                    btnSelect.Checked = false;
                    btnEdge1.Checked = false;
                    btnEdge2.Checked = false;
                    btnEdgeOther.Checked = false;
                    btnAddRoom.Checked = false;
                    btnAddComment.Checked = true;
                    DeselectObj();
                    SelTool = LayoutTool.Comment;
                    SetCursor(LayoutCursor.Crosshair);
                }
                HoldTool = sender == btnAddComment && ModifierKeys == Keys.Shift;
                spTool.Text = (HoldTool ? "HOLD" : "Tool") + ": Add Comment";
            }
        }

        private void mnuDelete_Click(object sender, EventArgs e) {
            DialogResult rtn = DialogResult.No;
            bool blnNoWarn = false;

            switch (Selection.Type) {
            case LayoutSelection.Room:
                if (EditGame.Pictures.Contains(Selection.Number)) {
                    // if user wants warning
                    switch (WinAGISettings.LEDelPicToo.Value) {
                    case Common.Base.AskOption.Ask:
                        // ask
                        rtn = MsgBoxEx.Show(MDIMain,
                            "Do you also want to remove the associated picture for this room?",
                            "Delete Room",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            "Always take this action",
                            ref blnNoWarn,
                            WinAGIHelp,
                            "htm\\winagi\\editinglayouts.htm#delete");
                        // if user wants no more warnings
                        if (blnNoWarn) {
                            WinAGISettings.LEDelPicToo.Value =
                                (rtn == DialogResult.Yes ? Common.Base.AskOption.Yes : Common.Base.AskOption.No);
                            WinAGISettings.LEDelPicToo.WriteSetting(WinAGISettingsFile);
                        }
                        break;
                    case Common.Base.AskOption.No:
                        // no
                        rtn = DialogResult.No;
                        break;
                    case Common.Base.AskOption.Yes:
                        // yes
                        rtn = DialogResult.Yes;
                        break;
                    }
                }
                else {
                    // nothing to delete
                    rtn = DialogResult.No;
                }

                // delete picture if that option was selected
                if (rtn == DialogResult.Yes) {
                    // remove picture
                    RemovePicture((byte)Selection.Number);
                }

                // remove logic from game
                // now remove logic (this clears the selection
                // which is why it has to be last)
                RemoveLogic((byte)Selection.Number);
                RecalculateMaxMin();
                SetScrollBars();
                // no need for a redraw here, since RemoveLogic handles that
                return;
            case LayoutSelection.TransPt:
                // remove the transfer point
                DeleteTransfer(Selection.Number);
                break;
            case LayoutSelection.ErrPt:
                // remove the errpt and its exit line
                // mark exit as deleted
                Room[ErrPt[Selection.Number].FromRoom].Exits[ErrPt[Selection.Number].ExitID].Status = ExitStatus.Deleted;
                RemoveObjFromStack(ErrPt[Selection.Number].Order);
                ErrPt[Selection.Number].Visible = false;
                ErrPt[Selection.Number].ExitID = "";
                ErrPt[Selection.Number].FromRoom = 0;
                ErrPt[Selection.Number].Room = 0;
                ErrPt[Selection.Number].Loc = new();
                break;
            case LayoutSelection.Comment:
                // delete the selected comment
                Comment[Selection.Number].Visible = false;
                Comment[Selection.Number].Size = new();
                Comment[Selection.Number].Loc = new();
                Comment[Selection.Number].Text = "";
                RemoveObjFromStack(Comment[Selection.Number].Order);
                break;
            case LayoutSelection.Exit:
                // delete this exit
                DeleteExit(Selection);
                break;
            case LayoutSelection.Multiple:
                break;
            }
            // deselect and redraw
            DeselectObj();
            RecalculateMaxMin();
            SetScrollBars();
            DrawLayout();
            MarkAsChanged();
        }

        private void mnuSelectAll_Click(object sender, EventArgs e) {
            // select all objects

            if (SelTool != LayoutTool.Select) {
                return;
            }

            // copy all objects to selectedobjects
            SelectedObjects.Clear();
            foreach (ObjInfo obj in ObjOrder) {
                if (obj.Type == LayoutSelection.TransPt) {
                    // add both legs
                    SelectedObjects.Add(new(LayoutSelection.TransPt, obj.Number, ExitLeg.FirstLeg));
                    SelectedObjects.Add(new(LayoutSelection.TransPt, obj.Number, ExitLeg.SecondLeg));
                }
                else {
                    SelectedObjects.Add(obj);
                }
            }
            // set the selection type to multiple
            SelectObj(LayoutSelection.Multiple, SelectedObjects.Count);
            picDraw.Invalidate();
        }

        private void mnuToggleTransPt_Click(object sender, EventArgs e) {
            // switch to other leg/trans pt

            switch (Selection.Type) {
            case LayoutSelection.TransPt:
                // jump to other transpt
                SelectOtherTransPt();
                break;
            case LayoutSelection.Exit:
                // switch to other leg
                Debug.Assert(Selection.TwoWay == ExitDirection.OneWay);
                if (Selection.Leg == ExitLeg.FirstLeg) {
                    Selection.Leg = ExitLeg.SecondLeg;
                }
                else if (Selection.Leg == ExitLeg.SecondLeg) {
                    Selection.Leg = ExitLeg.FirstLeg;
                }
                picDraw.Invalidate();
                break;
            }
        }

        private void mnuTogglePicture_Click(object sender, EventArgs e) {
            // toggle show room pic status
            Room[Selection.Number].ShowPic = !Room[Selection.Number].ShowPic;

            DrawLayout(LayoutSelection.Room, Selection.Number);
            MarkAsChanged();
        }

        private void mnuToggleGrid_Click(object sender, EventArgs e) {
            WinAGISettings.LEShowGrid.Value = !WinAGISettings.LEShowGrid.Value;
            DrawLayout();
        }

        private void mnuProperties_Click(object sender, EventArgs e) {

            // should only be called if a room is selected
            if (Selection.Type != LayoutSelection.Room) {
                return;
            }

            string strID = EditGame.Logics[Selection.Number].ID;
            string strDescription = EditGame.Logics[Selection.Number].Description;

            // use the id/description change method
            if (GetNewResID(AGIResType.Logic, Selection.Number, ref strID, ref strDescription, true, 1)) {
                // redraw layout to reflect changes
                DrawLayout(LayoutSelection.Room, Selection.Number);
                // mark as changed
                MarkAsChanged();

                // if a matching logic editor is open, it needs to be updated too
                if (LogicEditors.Count > 0) {
                    foreach (frmLogicEdit tmpForm in LogicEditors) {
                        if (tmpForm.FormMode == LogicFormMode.Logic && tmpForm.LogicNumber == Selection.Number) {
                            tmpForm.UpdateID(strID, strDescription);
                            break;
                        }
                    }
                }
            }
        }

        private void mnuOrderUp_Click(object sender, EventArgs e) {
            if (SelTool != LayoutTool.Select) {
                return;
            }

            ObjInfo tmpObj;
            int order;

            switch (Selection.Type) {
            case LayoutSelection.None:
                return;
            case LayoutSelection.Exit:
                return;
            case LayoutSelection.Multiple:
                // selection list needs to be sorted in reverse order, 
                // and trans pts only need one entry
                SortedList<int, ObjInfo> sellist = [];
                for (int i = 0; i < SelectedObjects.Count; i++) {
                    if (SelectedObjects[i].Type == LayoutSelection.TransPt) {
                        // check if already copied
                        if (sellist.ContainsValue(new(LayoutSelection.TransPt, SelectedObjects[i].Number))) {
                            continue;
                        }
                    }
                    // add it
                    tmpObj = new(SelectedObjects[i].Type, SelectedObjects[i].Number);
                    order = ObjOrder.IndexOf(tmpObj);
                    Debug.Assert(order != -1);
                    sellist.Add(order, new(SelectedObjects[i].Type, SelectedObjects[i].Number));
                }
                // step through all selected items in reverse order
                for (int i = sellist.Count - 1; i >= 0; i--) {
                    order = sellist.GetKeyAtIndex(i);
                    if (order != ObjOrder.Count - 1) {
                        // swap Order value of the two affected objects
                        SetObjOrder(order, order + 1);
                        SetObjOrder(order + 1, order);
                        // then swap this item with the next one
                        (ObjOrder[order], ObjOrder[order + 1]) = (ObjOrder[order + 1], ObjOrder[order]);
                    }
                }
                MarkAsChanged();
                DrawLayout();
                break;
            default:
                // if not at top (last in the list), bump up one 
                // ! remember that transpt objects are ordered as a pair
                // so always use default leg value)
                tmpObj = new(Selection.Type, Selection.Number);
                order = ObjOrder.IndexOf(tmpObj);
                if (order >= 0 && order < ObjOrder.Count - 1) {
                    // swap Order value of the two affected objects
                    SetObjOrder(order, order + 1);
                    SetObjOrder(order + 1, order);
                    // then swap this item with the next one
                    (ObjOrder[order], ObjOrder[order + 1]) = (ObjOrder[order + 1], ObjOrder[order]);
                    btnBack.Enabled = true;
                    btnFront.Enabled = order + 1 < ObjOrder.Count - 1;
                    MarkAsChanged();
                    DrawLayout(tmpObj.Type, tmpObj.Number);
                }
                break;
            }
        }

        private void mnuOrderDown_Click(object sender, EventArgs e) {
            if (SelTool != LayoutTool.Select) {
                return;
            }

            ObjInfo tmpObj;
            int order;

            switch (Selection.Type) {
            case LayoutSelection.None:
                return;
            case LayoutSelection.Exit:
                return;
            case LayoutSelection.Multiple:
                // selection list needs to be sorted and trans pts only need one entry
                SortedList<int, ObjInfo> sellist = [];
                for (int i = 0; i < SelectedObjects.Count; i++) {
                    if (SelectedObjects[i].Type == LayoutSelection.TransPt) {
                        // check if already copied
                        if (sellist.ContainsValue(new(LayoutSelection.TransPt, SelectedObjects[i].Number))) {
                            continue;
                        }
                    }
                    // add it
                    tmpObj = new(SelectedObjects[i].Type, SelectedObjects[i].Number);
                    order = ObjOrder.IndexOf(tmpObj);
                    Debug.Assert(order != -1);
                    sellist.Add(order, new(SelectedObjects[i].Type, SelectedObjects[i].Number));
                }
                // step through all selected items
                for (int i = 0; i < sellist.Count; i++) {
                    order = sellist.GetKeyAtIndex(i);
                    if (order != 0) {
                        // swap Order value of the two affected objects
                        SetObjOrder(order, order - 1);
                        SetObjOrder(order - 1, order);
                        // then swap this item with the previous one
                        (ObjOrder[order], ObjOrder[order - 1]) = (ObjOrder[order - 1], ObjOrder[order]);
                    }
                }
                MarkAsChanged();
                DrawLayout();
                break;
            default:
                // if not at bottom (first in the list), bump down one 
                // ! remember that transpt objects are ordered as a pair
                // so always use default leg value)
                tmpObj = new(Selection.Type, Selection.Number);
                order = ObjOrder.IndexOf(tmpObj);
                if (order != -1 && order > 0) {
                    // swap Order value of the two affected objects
                    SetObjOrder(order, order - 1);
                    SetObjOrder(order - 1, order);
                    // then swap this item with the next one
                    (ObjOrder[order], ObjOrder[order - 1]) = (ObjOrder[order - 1], ObjOrder[order]);
                    btnFront.Enabled = true;
                    btnBack.Enabled = order > 1;
                    MarkAsChanged();
                    DrawLayout(tmpObj.Type, tmpObj.Number);
                }
                break;
            }
        }

        private void mnuOrderFront_Click(object sender, EventArgs e) {
            if (SelTool != LayoutTool.Select) {
                return;
            }

            ObjInfo tmpObj;
            int order;

            switch (Selection.Type) {
            case LayoutSelection.None:
                return;
            case LayoutSelection.Exit:
                return;
            case LayoutSelection.Multiple:
                // selection list needs to be sorted in reverse order, 
                // and trans pts only need one entry
                SortedList<int, ObjInfo> sellist = [];
                for (int i = 0; i < SelectedObjects.Count; i++) {
                    if (SelectedObjects[i].Type == LayoutSelection.TransPt) {
                        // check if already copied
                        if (sellist.ContainsValue(new(LayoutSelection.TransPt, SelectedObjects[i].Number))) {
                            continue;
                        }
                    }
                    // add it
                    tmpObj = new(SelectedObjects[i].Type, SelectedObjects[i].Number);
                    order = ObjOrder.IndexOf(tmpObj);
                    Debug.Assert(order != -1);
                    sellist.Add(order, new(SelectedObjects[i].Type, SelectedObjects[i].Number));
                }
                // step through all selected items
                for (int i = 0; i < sellist.Count; i++) {
                    order = sellist.GetKeyAtIndex(i);
                    // move it to end
                    RemoveObjFromStack(order - i);
                    ObjOrder.Add(sellist[order]);
                    switch (sellist[order].Type) {
                    case LayoutSelection.Room:
                        Room[sellist[order].Number].Order = ObjOrder.Count - (sellist.Count - i);
                        break;
                    case LayoutSelection.TransPt:
                        TransPt[sellist[order].Number].Order = ObjOrder.Count - (sellist.Count - i);
                        break;
                    case LayoutSelection.ErrPt:
                        ErrPt[sellist[order].Number].Order = ObjOrder.Count - (sellist.Count - i);
                        break;
                    case LayoutSelection.Comment:
                        Comment[sellist[order].Number].Order = ObjOrder.Count - (sellist.Count - i);
                        break;
                    }
                }
                MarkAsChanged();
                DrawLayout();
                break;
            default:
                // move this to top of list (last entry)
                tmpObj = new(Selection.Type, Selection.Number);
                order = ObjOrder.IndexOf(tmpObj);
                if (order >= 0 && order < ObjOrder.Count - 1) {
                    // move it to end
                    RemoveObjFromStack(order);
                    ObjOrder.Add(tmpObj);
                    switch (tmpObj.Type) {
                    case LayoutSelection.Room:
                        Room[tmpObj.Number].Order = ObjOrder.Count - 1;
                        break;
                    case LayoutSelection.TransPt:
                        TransPt[tmpObj.Number].Order = ObjOrder.Count - 1;
                        break;
                    case LayoutSelection.ErrPt:
                        ErrPt[tmpObj.Number].Order = ObjOrder.Count - 1;
                        break;
                    case LayoutSelection.Comment:
                        Comment[tmpObj.Number].Order = ObjOrder.Count - 1;
                        break;
                    }
                    btnBack.Enabled = true;
                    btnFront.Enabled = false;
                    MarkAsChanged();
                    DrawLayout(tmpObj.Type, tmpObj.Number);
                }
                break;
            }
        }

        private void mnuOrderBack_Click(object sender, EventArgs e) {
            if (SelTool != LayoutTool.Select) {
                return;
            }

            ObjInfo tmpObj;
            int order;

            switch (Selection.Type) {
            case LayoutSelection.None:
                return;
            case LayoutSelection.Exit:
                return;
            case LayoutSelection.Multiple:
                // selection list needs to be sorted and trans pts only need one entry
                SortedList<int, ObjInfo> sellist = [];
                for (int i = 0; i < SelectedObjects.Count; i++) {
                    if (SelectedObjects[i].Type == LayoutSelection.TransPt) {
                        // check if already copied
                        if (sellist.ContainsValue(new(LayoutSelection.TransPt, SelectedObjects[i].Number))) {
                            continue;
                        }
                    }
                    // add it
                    tmpObj = new(SelectedObjects[i].Type, SelectedObjects[i].Number);
                    order = ObjOrder.IndexOf(tmpObj);
                    Debug.Assert(order != -1);
                    sellist.Add(order, new(SelectedObjects[i].Type, SelectedObjects[i].Number));
                }
                // step through items in full list, starting at beginning
                // until first selected item is found
                int offset = sellist.Count;
                for (int i = 0; i < ObjOrder.Count; i++) {
                    if (sellist.ContainsKey(i)) {
                        // reduce the offset
                        offset--;
                        // done if this is last item
                        if (offset == 0) {
                            break;
                        }
                    }
                    else {
                        // adjust this object by offset
                        switch (ObjOrder[i].Type) {
                        case LayoutSelection.Room:
                            Room[ObjOrder[i].Number].Order += offset;
                            break;
                        case LayoutSelection.TransPt:
                            TransPt[ObjOrder[i].Number].Order += offset;
                            break;
                        case LayoutSelection.ErrPt:
                            ErrPt[ObjOrder[i].Number].Order += offset;
                            break;
                        case LayoutSelection.Comment:
                            Comment[ObjOrder[i].Number].Order += offset;
                            break;
                        }
                    }
                }
                // now move all the selected items to beginning of list
                for (int i = sellist.Count - 1; i >= 0; i--) {
                    order = sellist.GetKeyAtIndex(i);
                    if (order != -1) {
                        switch (sellist[order].Type) {
                        case LayoutSelection.Room:
                            Room[sellist[order].Number].Order = i;
                            break;
                        case LayoutSelection.TransPt:
                            TransPt[sellist[order].Number].Order = i;
                            break;
                        case LayoutSelection.ErrPt:
                            ErrPt[sellist[order].Number].Order = i;
                            break;
                        case LayoutSelection.Comment:
                            Comment[sellist[order].Number].Order = i;
                            break;
                        }
                        ObjOrder.Remove(sellist[order]);
                        ObjOrder.Insert(0, sellist[order]);
                    }
                }
                MarkAsChanged();
                DrawLayout();
                break;
            default:
                // if not at bottom (first in the list), move it to front 
                // ! remember that transpt objects are ordered as a pair
                // so always use default leg value)
                tmpObj = new(Selection.Type, Selection.Number);
                order = ObjOrder.IndexOf(tmpObj);
                if (order > 0) {
                    for (int i = order; i > 0; i--) {
                        // change the object at the position below (i-1)
                        // to have a new position at i
                        switch (ObjOrder[i - 1].Type) {
                        case LayoutSelection.Room:
                            Room[ObjOrder[i - 1].Number].Order = i;
                            break;
                        case LayoutSelection.TransPt:
                            TransPt[ObjOrder[i - 1].Number].Order = i;
                            break;
                        case LayoutSelection.ErrPt:
                            ErrPt[ObjOrder[i - 1].Number].Order = i;
                            break;
                        case LayoutSelection.Comment:
                            Comment[ObjOrder[i - 1].Number].Order = i;
                            break;
                        }
                        // copy the object order information at position
                        // below (i-1) up to i
                        ObjOrder[i] = ObjOrder[i - 1];
                    }
                    ObjOrder[0] = tmpObj;
                    switch (tmpObj.Type) {
                    case LayoutSelection.Room:
                        Room[tmpObj.Number].Order = 0;
                        break;
                    case LayoutSelection.TransPt:
                        TransPt[tmpObj.Number].Order = 0;
                        break;
                    case LayoutSelection.ErrPt:
                        ErrPt[tmpObj.Number].Order = 0;
                        break;
                    case LayoutSelection.Comment:
                        Comment[tmpObj.Number].Order = 0;
                        break;
                    }
                    btnBack.Enabled = false;
                    btnFront.Enabled = true;
                    MarkAsChanged();
                    DrawLayout(tmpObj.Type, tmpObj.Number);
                }
                break;
            }
        }
        #endregion

        #region ToolStrip Event Handlers
        private void btnSelect_Click(object sender, EventArgs e) {
            if (SelTool != LayoutTool.Select) {
                btnSelect.Checked = true;
                btnEdge1.Checked = false;
                btnEdge2.Checked = false;
                btnEdgeOther.Checked = false;
                btnAddRoom.Checked = false;
                btnAddComment.Checked = false;
                DeselectObj();
                SelTool = LayoutTool.Select;
                spTool.Text = "Tool: Select";
                HoldTool = false;
            }
        }

        private void btnEdge1_Click(object sender, EventArgs e) {
            if (SelTool != LayoutTool.Edge2) {
                btnSelect.Checked = false;
                btnEdge1.Checked = true;
                btnEdge2.Checked = false;
                btnEdgeOther.Checked = false;
                btnAddRoom.Checked = false;
                btnAddComment.Checked = false;
                DeselectObj();
                SelTool = LayoutTool.Edge1;
            }
            HoldTool = ModifierKeys == Keys.Shift;
            spTool.Text = (HoldTool ? "HOLD" : "Tool") + ": One Way Edge";
        }

        private void btnEdge2_Click(object sender, EventArgs e) {
            if (SelTool != LayoutTool.Edge2) {
                btnSelect.Checked = false;
                btnEdge1.Checked = false;
                btnEdge2.Checked = true;
                btnEdgeOther.Checked = false;
                btnAddRoom.Checked = false;
                btnAddComment.Checked = false;
                DeselectObj();
                SelTool = LayoutTool.Edge2;
            }
            HoldTool = ModifierKeys == Keys.Shift;
            spTool.Text = (HoldTool ? "HOLD" : "Tool") + ": Two Way Edge";
        }

        private void btnEdgeOther_Click(object sender, EventArgs e) {
            if (SelTool != LayoutTool.Other) {
                btnSelect.Checked = false;
                btnEdge1.Checked = false;
                btnEdge2.Checked = false;
                btnEdgeOther.Checked = true;
                btnAddRoom.Checked = false;
                btnAddComment.Checked = false;
                DeselectObj();
                SelTool = LayoutTool.Other;
            }
            HoldTool = ModifierKeys == Keys.Shift;
            spTool.Text = (HoldTool ? "HOLD" : "Tool") + ": Other Exit";
        }

        private void btnZoomIn_Click(object sender, EventArgs e) {
            ChangeScale(1);
        }

        private void btnZoomOut_Click(object sender, EventArgs e) {
            ChangeScale(-1);
        }
        #endregion

        #region Control Event Handlers
        private void tmrScroll_Tick(object sender, EventArgs e) {
            // scrolling is in progress

            // shift amount is 1/4 of smallchange
            Point delta = new(hScrollBar1.SmallChange / 4, vScrollBar1.SmallChange / 4);

            // we need the current mouse position
            PointF mp = picDraw.PointToClient(Control.MousePosition);
            if (mp.X < -10) {
                Offset.X += delta.X;
                AnchorPt.X += delta.X / DSF;
                LineStart.X += delta.X / DSF;
            }
            else if (mp.X > picDraw.Width + 10) {
                Offset.X -= delta.X;
                AnchorPt.X -= delta.X / DSF;
                LineStart.X -= delta.X / DSF;
            }

            if (mp.Y < -10) {
                Offset.Y += delta.Y;
                AnchorPt.Y += delta.Y / DSF;
                LineStart.Y += delta.Y / DSF;
            }
            else if (mp.Y > picDraw.Height + 10) {
                Offset.Y -= delta.Y;
                AnchorPt.Y -= delta.Y / DSF;
                LineStart.Y -= delta.Y / DSF;
            }
            SetScrollBars();
            DrawLayout();
        }

        private void picDraw_MouseDown(object sender, MouseEventArgs e) {
            // always dismiss the comment text box if it's visible
            if (txtComment.Visible) {
                txtComment.Visible = false;
                return;
            }

            // check for right-clicks- they are used for context menu
            if (e.Button == MouseButtons.Right) {
                // reset mouse pointer
                SetCursor(LayoutCursor.Default);
                TSel tmpSel = new();
                // if in a drawing mode,
                if (SelTool != LayoutTool.Select) {
                    switch (SelTool) {
                    case LayoutTool.Comment:
                        btnAddComment.Checked = false;
                        break;
                    case LayoutTool.Edge1:
                        btnEdge1.Checked = false;
                        break;
                    case LayoutTool.Edge2:
                        btnEdge2.Checked = false;
                        break;
                    case LayoutTool.Other:
                        btnEdgeOther.Checked = false;
                        break;
                    case LayoutTool.Room:
                        btnAddRoom.Checked = false;
                        break;
                    }
                    SelTool = LayoutTool.Select;
                    btnSelect.Checked = false;
                }
                // check for a click on an exit or object
                tmpSel = ItemFromPos(e.Location);
                // if selection is a multi
                if (Selection.Type == LayoutSelection.Multiple) {
                    // if the cursor is over an exit, OR NOT over something in selection collection
                    if (tmpSel.Type == LayoutSelection.Exit || !IsSelected(tmpSel)) {
                        MakeNewSelection(tmpSel);
                    }
                }
                else {
                    // if selection is NOT the same
                    if (!SameAsSelection(tmpSel)) {
                        MakeNewSelection(tmpSel);
                    }
                }
                // exit and allow context menu to display
                return;
            }

            // resizing and moving are easy to check for because cursors are
            // unique for those operations
            // check for comment resizing
            if (DrawCursor == LayoutCursor.SizeNESW) {
                // upper right or lower left corner;
                if (e.X >= LayoutToScreenX(Comment[Selection.Number].Loc.X)) {
                    // moving upper right
                    SizingComment = 2;
                    // anchor lower left
                    AnchorPt = Comment[Selection.Number].Loc;
                    AnchorPt.Y += Comment[Selection.Number].Size.Height;
                }
                else {
                    // moving lower left
                    SizingComment = 3;
                    // anchor upper right
                    AnchorPt = Comment[Selection.Number].Loc;
                    AnchorPt.X += Comment[Selection.Number].Size.Width;
                }
                return;
            }
            else if (DrawCursor == LayoutCursor.SizeNWSE) {
                // lower right or upper left corner;
                if (e.X >= LayoutToScreenX(Comment[Selection.Number].Loc.X)) {
                    // moving lower right
                    SizingComment = 4;
                    // anchor upper left;
                    AnchorPt = Comment[Selection.Number].Loc;
                }
                else {
                    // moving upper left
                    SizingComment = 1;
                    // anchor lower right;
                    AnchorPt = Comment[Selection.Number].Loc;
                    AnchorPt.X += Comment[Selection.Number].Size.Width;
                    AnchorPt.Y += Comment[Selection.Number].Size.Height;
                }
                return;
            }
            // check for moving exit, or adding room, or adding comment
            else if (DrawCursor == LayoutCursor.Crosshair) {
                switch (SelTool) {
                case LayoutTool.Select:
                    // begin moving exit
                    MoveExit = true;
                    DrawExit = 1;
                    if (MovePoint == 0) {
                        LineStart = Selection.EP;
                    }
                    else {
                        LineStart = Selection.SP;
                    }
                    return;
                case LayoutTool.Room:
                    // if at Max already
                    if (EditGame.Logics.Count == 256) {
                        MessageBox.Show(MDIMain,
                            "Maximum number of logics already exist in this game. Remove one or more existing logics, and then try again.",
                            "Can't Add Room",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else {
                        AddNewRoom(ScreenToLayout(GridPos(e.Location)));
                    }
                    if (!HoldTool) {
                        // go back to select tool
                        HoldTool = false;
                        btnAddRoom.Checked = false;
                        btnSelect.Checked = true;
                        SelTool = LayoutTool.Select;
                        spTool.Text = "Tool: Select";
                    }
                    return;
                case LayoutTool.Comment:
                    // begin drawing a comment
                    AnchorPt = GridPos(ScreenToLayout(e.Location));
                    SizingComment = 4;
                    break;
                }
            }
            // check for object movement
            else if (DrawCursor == LayoutCursor.MoveSelection) {
                // begin moving selected objects
                BeginMoveObj(e.Location);
                return;
            }
            // check for panning the drawing canvas
            if (ModifierKeys == Keys.Shift) {
                DragCanvas = true;
                CanvasAnchor = e.Location;
                // set cursor to hand
                SetCursor(LayoutCursor.DragSurface);
                return;
            }
            // action to take depends on current selected tool
            switch (SelTool) {
            case LayoutTool.Select:
                // a LOT of things to check if the selection tool
                // is active; strategy is to first check for
                // dragging of an object, then we see if something
                // is clicked; if nothing is clicked, we can just exit
                // then we process whatever was clicked (object or
                // exit) depending on what the current selection is

                // check for a click on an edge or object
                TSel tmpSel = ItemFromPos(e.Location);

                // if nothing new is clicked on,
                // we can just deselect whatever is currently
                // selected and be done
                if (tmpSel.Type == LayoutSelection.None) {
                    // nothing going on; make sure nothing is
                    // selected
                    DeselectObj();
                    // save anchor in case a drag select is about to begin
                    AnchorPt = ScreenToLayout(e.Location);
                    return;
                }

                // if current selection (not the newly clicked
                // object/exit) is more than one object,
                // check for clicks that expand it or deselect
                // one of its objects before checking single
                // selection actions
                if (Selection.Type == LayoutSelection.Multiple) {
                    bool found = false;
                    for (int i = 0; i < Selection.Number; i++) {
                        if (SelectedObjects[i].Type == tmpSel.Type && SelectedObjects[i].Number == tmpSel.Number) {
                            // need to validate trans pt
                            if (SelectedObjects[i].Type == LayoutSelection.TransPt) {
                                if (SelectedObjects[i].Leg == tmpSel.Leg) {
                                    // this is it
                                    found = true;
                                }
                            }
                            else {
                                // found room, errpt or comment
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found) {
                        // if ctrl key is pressed
                        if (ModifierKeys == Keys.Control) {
                            // un-select this object
                            UnselectObj(tmpSel);
                            picDraw.Invalidate();
                            return;
                        }
                    }
                    else {
                        // is user adding to collection?
                        // (check for control key)
                        if (ModifierKeys == Keys.Control) {
                            switch (tmpSel.Type) {
                            case LayoutSelection.Comment:
                            case LayoutSelection.ErrPt:
                            case LayoutSelection.Room:
                            case LayoutSelection.TransPt:
                                // add it to collection
                                ObjInfo obj = new(tmpSel.Type, tmpSel.Number, tmpSel.Leg);
                                SelectedObjects.Add(obj);
                                SelectObj(LayoutSelection.Multiple, Selection.Number + 1);
                                picDraw.Invalidate();
                                // exit so selection isn't wiped out by the
                                // selection processing code below
                                return;
                            }
                        }

                        // not adding new; continue with normal
                        // processing of determining selection
                        // reset multiple selection
                        DeselectObj();
                    }
                }

                // depending on currently selected object,
                // process the click to either select something else
                // or move/take action on selection
                switch (Selection.Type) {
                case LayoutSelection.Room:
                case LayoutSelection.TransPt:
                case LayoutSelection.ErrPt:
                case LayoutSelection.Comment:
                    // if same as current selection
                    if (SameAsSelection(tmpSel)) {
                        // if over a comment object's text area(cursor is ibeam)
                        if (Selection.Type == LayoutSelection.Comment && DrawCursor == LayoutCursor.IBeam) {
                            // put a text box on top the comment and begin editing
                            BeginEditComment();
                            // send mouse up msg to release mouse from picDraw
                            const int WM_LBUTTONDOWN = 0x201;
                            const int WM_LBUTTONUP = 0x202;
                            _ = SendMessage(picDraw.Handle, WM_LBUTTONUP, 0, 0);
                            // send mouse down msg to text box
                            _ = SendMessage(txtComment.Handle, WM_LBUTTONDOWN, 0, e.X - txtComment.Left + (e.Y - txtComment.Top) * 0x10000);
                            txtComment.Select();
                            return;
                        }
                    }
                    else {
                        // something OTHER than the current selection
                        // is clicked; is CTRL key down?
                        if (ModifierKeys == Keys.Control) {
                            // if adding to a singly selected object
                            switch (Selection.Type) {
                            case LayoutSelection.Room:
                            case LayoutSelection.TransPt:
                            case LayoutSelection.ErrPt:
                            case LayoutSelection.Comment:
                                switch (tmpSel.Type) {
                                case LayoutSelection.Room:
                                case LayoutSelection.TransPt:
                                case LayoutSelection.ErrPt:
                                case LayoutSelection.Comment:
                                    // add current selected object to the collection
                                    SelectedObjects.Clear();
                                    ObjInfo tmp = new(Selection.Type, Selection.Number, Selection.Leg);
                                    SelectedObjects.Add(tmp);
                                    // add the new selected object to collection
                                    tmp = new(tmpSel.Type, tmpSel.Number, tmpSel.Leg);
                                    SelectedObjects.Add(tmp);
                                    // adjust selection properties
                                    SelectObj(LayoutSelection.Multiple, 2);
                                    // force update
                                    picDraw.Invalidate();
                                    return;
                                }
                                break;
                            }
                        }

                        // something new is clicked, and it's not being
                        // added to current selection, so if there is
                        // something already selected, we need to
                        // deselect it first
                        if (Selection.Type != LayoutSelection.None) {
                            // deselect it
                            DeselectObj();
                        }

                        // select whatever is under the cursor
                        MakeNewSelection(tmpSel);
                    }
                    break;
                case LayoutSelection.Exit:
                    if (!SameAsSelection(tmpSel)) {
                        // the thing clicked is not the currently selected exit
                        // select whatever is under the cursor
                        MakeNewSelection(tmpSel);
                    }
                    break;
                case LayoutSelection.None:
                    // select whatever was clicked
                    MakeNewSelection(tmpSel);
                    break;
                }
                return;

            // check for new exits
            case LayoutTool.Edge1:
            case LayoutTool.Edge2:
            case LayoutTool.Other:
                // if ok to draw, cursor won't be 'no drop'
                if (DrawCursor == LayoutCursor.NoDrop) {
                    // can't draw edge here
                    return;
                }
                ShowExitStart = ExitReason.None;

                // set anchor to edge (or center) of selected room
                tmpSel = ObjectFromPos(e.Location);
                if (tmpSel.Type == LayoutSelection.Room) {
                    // save room number
                    NewExitRoom = tmpSel.Number;
                    // allow drawing to any room
                    MoveExit = true;
                    DrawExit = 1;

                    // set anchor for exit line
                    switch (NewExitReason) {
                    case ExitReason.Horizon:
                        AnchorPt.X = Room[tmpSel.Number].Loc.X + RM_SIZE / 2;
                        AnchorPt.Y = Room[tmpSel.Number].Loc.Y;
                        break;
                    case ExitReason.Bottom:
                        AnchorPt.X = Room[tmpSel.Number].Loc.X + RM_SIZE / 2;
                        AnchorPt.Y = Room[tmpSel.Number].Loc.Y + RM_SIZE;
                        break;
                    case ExitReason.Right:
                        AnchorPt.X = Room[tmpSel.Number].Loc.X + RM_SIZE;
                        AnchorPt.Y = Room[tmpSel.Number].Loc.Y + RM_SIZE / 2;
                        break;
                    case ExitReason.Left:
                        AnchorPt.X = Room[tmpSel.Number].Loc.X;
                        AnchorPt.Y = Room[tmpSel.Number].Loc.Y + RM_SIZE / 2;
                        break;
                    case ExitReason.Other:
                        AnchorPt.X = Room[tmpSel.Number].Loc.X + RM_SIZE / 2;
                        AnchorPt.Y = Room[tmpSel.Number].Loc.Y + RM_SIZE / 2;
                        break;
                    }
                    LineStart = AnchorPt;
                }
                else {
                    // save room and transfer number
                    NewExitRoom = TransPt[tmpSel.Number].Room[1];
                    NewExitTrans = tmpSel.Number;
                    // only allow drawing to matching room for this transfer
                    MoveExit = true;
                    DrawExit = 2;
                }
                return;
            }
        }

        private void picDraw_MouseMove(object sender, MouseEventArgs e) {
            TSel tmpSel = new();

            if (e.Location == MouseDownPt) {
                // Mouse hasn't moved, so no need to do anything
                return;
            }
            MouseDownPt = e.Location;

            spCurX.Text = ((e.X - Offset.X) / DSF).ToString("0.000");
            spCurY.Text = ((e.Y - Offset.Y) / DSF).ToString("0.000");

            if (e.Button == MouseButtons.Right) {
                return;
            }
            // if dragging the canvas, adjust the view
            if (DragCanvas) {
                Offset.X += e.X - CanvasAnchor.X;
                Offset.Y += e.Y - CanvasAnchor.Y;
                CanvasAnchor.X = e.X;
                CanvasAnchor.Y = e.Y;
                SetScrollBars();
                DrawLayout();
                return;
            }

            // take action based on selected tool
            switch (SelTool) {
            case LayoutTool.None:
                return;
            case LayoutTool.Select:
                switch (e.Button) {
                case MouseButtons.None:
                    // no button down
                    if (ModifierKeys == Keys.Control) {
                        MultiSelectCursor(e.Location);
                    }
                    else {
                        SingleSelectCursor(e.Location);
                    }
                    break;
                case MouseButtons.Left:
                    // if sizing a comment, force a redraw
                    if (SizingComment > 0) {
                        picDraw.Invalidate();
                        return;
                    }

                    if (MoveExit) {
                        // if past edges of drawing surface, enable scrolling
                        tmrScroll.Enabled = e.X < -10 || e.X > picDraw.ClientRectangle.Width + 10 ||
                                    e.Y < -10 || e.Y > picDraw.ClientRectangle.Height + 10;

                        // determine if cursor is over a room/trans pt where exit can be dropped
                        tmpSel = ObjectFromPos(e.Location);
                        switch (tmpSel.Type) {
                        case LayoutSelection.Room:
                            switch (Room[Selection.Number].Exits[Selection.ExitID].Reason) {
                            case ExitReason.Horizon:
                                if (MovePoint == 0) {
                                    SetCursor(LayoutCursor.BottomExit);
                                }
                                else {
                                    SetCursor(LayoutCursor.HorizonExit);
                                }
                                break;
                            case ExitReason.Left:
                                if (MovePoint == 0) {
                                    SetCursor(LayoutCursor.RightExit);
                                }
                                else {
                                    SetCursor(LayoutCursor.LeftExit);
                                }
                                break;
                            case ExitReason.Right:
                                if (MovePoint == 0) {
                                    SetCursor(LayoutCursor.LeftExit);
                                }
                                else {
                                    SetCursor(LayoutCursor.RightExit);
                                }
                                break;
                            case ExitReason.Bottom:
                                if (MovePoint == 0) {
                                    SetCursor(LayoutCursor.HorizonExit);
                                }
                                else {
                                    SetCursor(LayoutCursor.BottomExit);
                                }
                                break;
                            default:
                                // if mostly horizontal
                                if (Math.Abs(e.X - LayoutToScreenX(LineStart.X)) > Math.Abs(e.Y - LayoutToScreenY(LineStart.Y))) {
                                    if (e.X > LayoutToScreenX(LineStart.X)) {
                                        SetCursor(LayoutCursor.RightExit);
                                    }
                                    else {
                                        SetCursor(LayoutCursor.LeftExit);
                                    }
                                }
                                else {
                                    if (e.Y > LayoutToScreenY(LineStart.Y)) {
                                        SetCursor(LayoutCursor.BottomExit);
                                    }
                                    else {
                                        SetCursor(LayoutCursor.HorizonExit);
                                    }
                                }
                                break;
                            }
                            break;
                        case LayoutSelection.TransPt:
                            // only allow reciprocal drops (exit associated with this trans pt is TO selroom)
                            if (TransPt[tmpSel.Number].Count == 1 && TransPt[tmpSel.Number].Room[1] == Selection.Number && MovePoint == 1) {
                                string tmpID = TransPt[tmpSel.Number].ExitID[0];
                                switch (Room[Selection.Number].Exits[Selection.ExitID].Reason) {
                                case ExitReason.Horizon:
                                    // if  exit reason is BOTTOM
                                    if (Room[TransPt[tmpSel.Number].Room[0]].Exits[tmpID].Reason == ExitReason.Bottom) {
                                        SetCursor(LayoutCursor.HorizonExit);
                                    }
                                    else {
                                        SetCursor(LayoutCursor.NoDrop);
                                    }
                                    break;
                                case ExitReason.Bottom:
                                    // if exit reason is HORIZON
                                    if (Room[TransPt[tmpSel.Number].Room[0]].Exits[tmpID].Reason == ExitReason.Horizon) {
                                        SetCursor(LayoutCursor.BottomExit);
                                    }
                                    else {
                                        SetCursor(LayoutCursor.NoDrop);
                                    }
                                    break;
                                case ExitReason.Right:
                                    // if exit reason is LEFT
                                    if (Room[TransPt[tmpSel.Number].Room[0]].Exits[tmpID].Reason == ExitReason.Left) {
                                        SetCursor(LayoutCursor.RightExit);
                                    }
                                    else {
                                        SetCursor(LayoutCursor.NoDrop);
                                    }
                                    break;
                                case ExitReason.Left:
                                    // if exit reason is RIGHT
                                    if (Room[TransPt[tmpSel.Number].Room[0]].Exits[tmpID].Reason == ExitReason.Right) {
                                        SetCursor(LayoutCursor.LeftExit);
                                    }
                                    else {
                                        SetCursor(LayoutCursor.NoDrop);
                                    }
                                    break;
                                default:
                                    if (Room[TransPt[tmpSel.Number].Room[0]].Exits[tmpID].Reason == ExitReason.Other) {
                                        // if mostly horizontal
                                        // (cursor may need to change, so can't rely on current pointer to skip
                                        if (Math.Abs(e.X - LayoutToScreenX(LineStart.X)) > Math.Abs(e.Y - LayoutToScreenY(LineStart.Y))) {
                                            if (e.X > LayoutToScreenX(LineStart.X)) {
                                                SetCursor(LayoutCursor.RightExit);
                                            }
                                            else {
                                                SetCursor(LayoutCursor.LeftExit);
                                            }
                                        }
                                        else {
                                            if (e.Y > LayoutToScreenY(LineStart.Y)) {
                                                SetCursor(LayoutCursor.BottomExit);
                                            }
                                            else {
                                                SetCursor(LayoutCursor.HorizonExit);
                                            }
                                        }
                                    }
                                    else {
                                        SetCursor(LayoutCursor.NoDrop);
                                    }
                                    break;
                                }
                            }
                            else {
                                // can't drop
                                SetCursor(LayoutCursor.NoDrop);
                            }
                            break;
                        case LayoutSelection.ErrPt:
                        case LayoutSelection.Comment:
                            SetCursor(LayoutCursor.NoDrop);
                            break;
                        default:
                            SetCursor(LayoutCursor.Crosshair);
                            break;
                        }
                        picDraw.Invalidate();
                        return;
                    }

                    if (MoveObj) {
                        // if past edges of drawing surface, enable scrolling
                        tmrScroll.Enabled = (e.X < -10 || (e.X > picDraw.ClientRectangle.Width + 10) ||
                                    e.Y < -10 || (e.Y > picDraw.ClientRectangle.Height + 10));
                        picDraw.Invalidate();
                        return;
                    }

                    // when an object is clicked, the move flag is not normally set
                    // until it is clicked again; in some cases the user wants to
                    // click and drag immediately
                    // check if over selectedobject to see if this is the case

                    // if there is an object currently selected
                    switch (Selection.Type) {
                    case LayoutSelection.None:
                        DragSelect = true;
                        break;
                    case LayoutSelection.Exit:
                        // if on either handle, show select cursor
                        if (PointOnHandle(e.Location, Selection)) {
                            SetCursor(LayoutCursor.Crosshair);
                            picDraw_MouseDown(sender, e);
                            return;
                        }
                        break;
                    case LayoutSelection.Room or LayoutSelection.TransPt or LayoutSelection.ErrPt or LayoutSelection.Comment:
                        // if cursor is within object selection area and not moving
                        if (PointOnObject(e.Location, Selection) && !MoveObj) {
                            // begin moving
                            BeginMoveObj(e.Location);
                            return;
                        }
                        break;
                    }

                    // if drag-selecting
                    if (DragSelect) {
                        // if past edges of drawing surface, enable scrolling
                        tmrScroll.Enabled = e.X < -10 || e.X > picDraw.ClientRectangle.Width + 10 ||
                                    e.Y < -10 || e.Y > picDraw.ClientRectangle.Height + 10;
                        if (Math.Abs(e.X - LayoutToScreenX(AnchorPt.X)) >= 3 && Math.Abs(e.Y - LayoutToScreenY(AnchorPt.Y)) >= 3) {
                            ShowDrag = true;
                        }
                        else {
                            ShowDrag = false;
                        }
                        picDraw.Invalidate();
                        return;
                    }
                    break;
                }
                break;
            case LayoutTool.Edge1 or LayoutTool.Edge2 or LayoutTool.Other:
                switch (e.Button) {
                case MouseButtons.None:
                    // if not drawing an exit
                    if (DrawExit == 0) {
                        // check for an object under cursor
                        tmpSel = ObjectFromPos(e.Location);
                        if (tmpSel.Type == LayoutSelection.Room) {
                            // see if on or near an edge, adjust pointer and display
                            ExitReason suggest;
                            if (SelTool == LayoutTool.Other) {
                                // if drawing a 'other' exit, it's always the same dir value
                                suggest = ExitReason.Other;
                                SetCursor(LayoutCursor.OtherExit);
                            }
                            else {
                                // calculate the direction value by determiming which edge
                                // the cursor is closest to
                                float tmpX = LayoutToScreenX(Room[tmpSel.Number].Loc.X + RM_SIZE / 2);
                                float tmpY = LayoutToScreenY(Room[tmpSel.Number].Loc.Y + RM_SIZE / 2);
                                if (e.X >= tmpX && e.X - tmpX >= Math.Abs(e.Y - tmpY)) {
                                    suggest = ExitReason.Right;
                                    SetCursor(LayoutCursor.RightExit);
                                }
                                else if (e.X < tmpX && tmpX - e.X > Math.Abs(e.Y - tmpY)) {
                                    SetCursor(LayoutCursor.LeftExit);
                                    suggest = ExitReason.Left;
                                }
                                else if (e.Y >= tmpY && e.Y - tmpY >= Math.Abs(e.X - tmpX)) {
                                    SetCursor(LayoutCursor.BottomExit);
                                    suggest = ExitReason.Bottom;
                                }
                                else {
                                    SetCursor(LayoutCursor.HorizonExit);
                                    suggest = ExitReason.Horizon;
                                }
                            }
                            // if changing, then re-highlight
                            if (suggest != NewExitReason) {
                                NewExitReason = suggest;
                            }
                            HighlightExitStart(tmpSel.Number, NewExitReason);
                        }
                        else {
                            // hide the edge marker and reset the suggested exit
                            NewExitReason = ExitReason.None;
                            if (ShowExitStart != ExitReason.None) {
                                picDraw.Invalidate();
                            }
                            ShowExitStart = ExitReason.None;
                            // if drawing a single exit, and over a transpt that
                            // could support another exit going in this direction
                            // then we need to show the correct mouse pointer
                            if (tmpSel.Type == LayoutSelection.TransPt && SelTool == LayoutTool.Edge1) {
                                // if this exit is not yet two-way
                                if (TransPt[tmpSel.Number].Count == 1) {
                                    // display corresponding direction (only valid for edge exits;
                                    // never other or unknown exits)
                                    string tmpID = TransPt[tmpSel.Number].ExitID[0];
                                    // swap direction because going from transpt to room
                                    switch (Room[TransPt[tmpSel.Number].Room[0]].Exits[tmpID].Reason) {
                                    case ExitReason.Horizon:
                                        SetCursor(LayoutCursor.BottomExit);
                                        NewExitReason = ExitReason.Bottom;
                                        break;
                                    case ExitReason.Bottom:
                                        SetCursor(LayoutCursor.HorizonExit);
                                        NewExitReason = ExitReason.Horizon;
                                        break;
                                    case ExitReason.Right:
                                        SetCursor(LayoutCursor.LeftExit);
                                        NewExitReason = ExitReason.Left;
                                        break;
                                    case ExitReason.Left:
                                        SetCursor(LayoutCursor.RightExit);
                                        NewExitReason = ExitReason.Right;
                                        break;
                                    default:
                                        SetCursor(LayoutCursor.NoDrop);
                                        break;
                                    }
                                }
                                else {
                                    SetCursor(LayoutCursor.NoDrop);
                                }
                            }
                            else {
                                SetCursor(LayoutCursor.NoDrop);
                            }
                        }
                        return;
                    }
                    break;
                case MouseButtons.Left:
                    // if drawing an exit
                    if (DrawExit != 0) {
                        picDraw.Invalidate();
                        // if past edges of drawing surface, enable scrolling
                        tmrScroll.Enabled = (e.X < -10 || e.X > picDraw.ClientRectangle.Width + 10 ||
                                e.Y < -10 || e.Y > picDraw.ClientRectangle.Height + 10);
                        // get object under cursor
                        tmpSel = ObjectFromPos(e.Location);
                        if (tmpSel.Type == LayoutSelection.Room) {
                            // if drawing from a transpt
                            if (DrawExit == 2) {
                                // if room doesn't match transpt room
                                if (TransPt[NewExitTrans].Room[0] != tmpSel.Number) {
                                    SetCursor(LayoutCursor.NoDrop);
                                    return;
                                }
                            }
                            switch (NewExitReason) {
                            case ExitReason.Horizon:
                                SetCursor(LayoutCursor.HorizonExit);
                                break;
                            case ExitReason.Bottom:
                                SetCursor(LayoutCursor.BottomExit);
                                break;
                            case ExitReason.Right:
                                SetCursor(LayoutCursor.RightExit);
                                break;
                            case ExitReason.Left:
                                SetCursor(LayoutCursor.LeftExit);
                                break;
                            case ExitReason.Other:
                                // depends on direction of line
                                // if mostly horizontal
                                if (Math.Abs(e.X - LayoutToScreenX(LineStart.X)) > Math.Abs(e.Y - LayoutToScreenY(LineStart.Y))) {
                                    if (e.X > LayoutToScreenX(LineStart.X)) {
                                        SetCursor(LayoutCursor.RightExit);
                                    }
                                    else {
                                        SetCursor(LayoutCursor.LeftExit);
                                    }
                                }
                                else {
                                    if (e.Y > LayoutToScreenY(LineStart.Y)) {
                                        SetCursor(LayoutCursor.BottomExit);
                                    }
                                    else {
                                        SetCursor(LayoutCursor.HorizonExit);
                                    }
                                }
                                break;
                            }
                        }
                        else if (tmpSel.Type == LayoutSelection.TransPt && SelTool == LayoutTool.Edge1) {
                            // if this exit is not yet two way
                            if (TransPt[tmpSel.Number].Count == 1) {
                                // if this transpt to room matches fromroom
                                if (TransPt[tmpSel.Number].Room[1] == NewExitRoom) {
                                    // display corresponding direction (only valid for edge exiits;
                                    // never other or unknown exits)
                                    string tmpID = TransPt[tmpSel.Number].ExitID[0];
                                    // swap direction because going from transpt to room
                                    switch (Room[TransPt[tmpSel.Number].Room[0]].Exits[tmpID].Reason) {
                                    case ExitReason.Horizon:
                                        SetCursor(LayoutCursor.BottomExit);
                                        NewExitReason = ExitReason.Bottom;
                                        break;
                                    case ExitReason.Bottom:
                                        SetCursor(LayoutCursor.HorizonExit);
                                        NewExitReason = ExitReason.Horizon;
                                        break;
                                    case ExitReason.Right:
                                        SetCursor(LayoutCursor.LeftExit);
                                        NewExitReason = ExitReason.Left;
                                        break;
                                    case ExitReason.Left:
                                        SetCursor(LayoutCursor.RightExit);
                                        NewExitReason = ExitReason.Right;
                                        break;
                                    }
                                    return;
                                }
                            }
                            // can't draw
                            SetCursor(LayoutCursor.NoDrop);
                        }
                        else {
                            // can't draw
                            SetCursor(LayoutCursor.NoDrop);
                        }
                        return;
                    }
                    break;
                }
                break;
            case LayoutTool.Room:
                break;
            case LayoutTool.Comment:
                if (SizingComment > 0) {
                    picDraw.Invalidate();
                    return;
                }
                break;
            }
        }

        private void picDraw_MouseUp(object sender, MouseEventArgs e) {
            if (DragCanvas) {
                // stop the drag
                DragCanvas = false;
                // reset to default cursor
                SetCursor(LayoutCursor.Default);
            }
            // always cancel the scroll timer
            tmrScroll.Enabled = false;

            switch (SelTool) {
            case LayoutTool.Select:
                if (MoveExit) {
                    MoveExit = false;
                    DrawExit = 0;
                    // validate new exit pos
                    // by checking cursor (an exit cursor means drop is OK)
                    switch (DrawCursor) {
                    case LayoutCursor.HorizonExit:
                    case LayoutCursor.RightExit:
                    case LayoutCursor.BottomExit:
                    case LayoutCursor.LeftExit:
                    case LayoutCursor.OtherExit:
                        // get object where exit is being dropped
                        TSel tmpSel = ObjectFromPos(e.Location);
                        if (Selection.TwoWay != ExitDirection.BothWays) {
                            // if changing to room
                            if (MovePoint == 1) {
                                // if target room has changed
                                if (tmpSel.Number != Room[Selection.Number].Exits[Selection.ExitID].Room) {
                                    // change the to-room for this exit
                                    SelectExit(ChangeToRoom(Selection, tmpSel.Number, tmpSel.Type));
                                }
                                else {
                                    // nothing to do; just refresh to clear the
                                    // drag arrow
                                    picDraw.Invalidate();
                                    return;
                                }
                            }
                            else {
                                // if from room has changed
                                if (tmpSel.Number != Selection.Number) {
                                    // change the from room; keep type the same
                                    SelectExit(ChangeFromRoom(Selection, tmpSel.Number));
                                }
                                else {
                                    // nothing to do; just refresh to clear the
                                    // drag arrow
                                    picDraw.Invalidate();
                                    return;
                                }
                            }
                            // reselect the newly moved exit

                            // redraw
                            DrawLayout();
                            MarkAsChanged();
                        }
                        else {
                            // two-way; change both from and to room
                            int tgt = tmpSel.Number;
                            // get reciprocal exit
                            tmpSel = Selection;
                            IsTwoWay(tmpSel.Number, tmpSel.ExitID, ref tmpSel.Number, ref tmpSel.ExitID);
                            // determine which is from room and which is to room
                            if (MovePoint == 0) {
                                // tmpSel contains to room
                                // if target room has changed (Selection.Number)
                                if (tgt != Selection.Number) {
                                    // change the to-room for this exit
                                    // MAKE SURE to do this BEFORE changing 'from' room;
                                    // when changing from room, if it finds a reciprocal,
                                    // it will delete it, so the ChangeToRoom function
                                    // no longer has an exit to move
                                    ChangeToRoom(tmpSel, tgt, LayoutSelection.Room);
                                    // now change from room
                                    tmpSel = ChangeFromRoom(Selection, tgt);
                                }
                                else {
                                    // target hasn't changed; just refresh to clear the 
                                    // drag arrow
                                    picDraw.Invalidate();
                                    return;
                                }
                            }
                            else {
                                // tmpsel contains from room
                                // if from room has changed
                                if (tgt != tmpSel.Number) {
                                    // change to room
                                    // MAKE SURE to do this BEFORE changing 'from' room;
                                    // when changing from room, if it finds a reciprocal,
                                    // it will delete it, so then the change 'to' room function
                                    // no longer has an exit to move
                                    ChangeToRoom(Selection, tgt, LayoutSelection.Room);
                                    // change from room, using Selection
                                    tmpSel = ChangeFromRoom(tmpSel, tgt);
                                }
                                else {
                                    // target hasn't changed; just refresh to clear the 
                                    // drag arrow
                                    picDraw.Invalidate();
                                    return;
                                }
                            }
                            // copy tempsel back into selection
                            tmpSel.TwoWay = ExitDirection.BothWays;
                            SelectExit(tmpSel);
                            // redraw
                            DrawLayout();
                            MarkAsChanged();
                        }
                        break;
                    }
                    // reset cursor
                    SetCursor(LayoutCursor.Crosshair);
                    picDraw.Invalidate();
                }
                if (MoveObj) {
                    // drop the selected objects at this location
                    SetCursor(LayoutCursor.MoveSelection);
                    DropSelection(e.X - Delta.X, e.Y - Delta.Y);
                    picDraw.Invalidate();
                    MoveObj = false;
                    return;
                }
                RectangleF rect = new();
                if (SizingComment != 0) {
                    switch (SizingComment) {
                    case 1:
                        // move upper left
                        // anchor lower right
                        if (LayoutToScreenX(AnchorPt.X) - e.X > MIN_CMT_SZ * DSF) {
                            rect.X = LayoutToScreenX(GridPos(ScreenToLayoutX(e.X)));
                            rect.Width = LayoutToScreenX(AnchorPt.X) - rect.X;
                        }
                        else {
                            rect.X = LayoutToScreenX(AnchorPt.X - MIN_CMT_SZ);
                            rect.Width = MIN_CMT_SZ * DSF;
                        }
                        if (LayoutToScreenY(AnchorPt.Y) - e.Y > MIN_CMT_SZ * DSF) {
                            rect.Y = LayoutToScreenY(GridPos(ScreenToLayoutY(e.Y)));
                            rect.Height = LayoutToScreenY(AnchorPt.Y) - rect.Y;
                        }
                        else {
                            rect.Y = LayoutToScreenY(AnchorPt.Y - MIN_CMT_SZ);
                            rect.Height = MIN_CMT_SZ * DSF;
                        }
                        break;
                    case 2:
                        // move upper right
                        // anchor lower left
                        rect.X = LayoutToScreenX(AnchorPt.X);
                        if (e.X > LayoutToScreenX(AnchorPt.X + MIN_CMT_SZ)) {
                            rect.Width = (GridPos(ScreenToLayoutX(e.X)) - AnchorPt.X) * DSF;
                        }
                        else {
                            rect.Width = MIN_CMT_SZ * DSF;
                        }
                        if (LayoutToScreenY(AnchorPt.Y) - e.Y > MIN_CMT_SZ * DSF) {
                            rect.Y = LayoutToScreenY(GridPos(ScreenToLayoutY(e.Y)));
                            rect.Height = LayoutToScreenY(AnchorPt.Y) - rect.Y;
                        }
                        else {
                            rect.Y = LayoutToScreenY(AnchorPt.Y - MIN_CMT_SZ);
                            rect.Height = MIN_CMT_SZ * DSF;
                        }
                        break;
                    case 3:
                        // move lower left
                        // anchor upper right
                        if (LayoutToScreenX(AnchorPt.X) - e.X > MIN_CMT_SZ * DSF) {
                            rect.X = LayoutToScreenX(GridPos(ScreenToLayoutX(e.X)));
                            rect.Width = LayoutToScreenX(AnchorPt.X) - rect.X;
                        }
                        else {
                            rect.X = LayoutToScreenX(AnchorPt.X - MIN_CMT_SZ);
                            rect.Width = MIN_CMT_SZ * DSF;
                        }
                        rect.Y = LayoutToScreenY(AnchorPt.Y);
                        if (e.Y > LayoutToScreenY(AnchorPt.Y + MIN_CMT_SZ)) {
                            rect.Height = (GridPos(ScreenToLayoutY(e.Y)) - AnchorPt.Y) * DSF;
                        }
                        else {
                            rect.Height = MIN_CMT_SZ * DSF;
                        }
                        break;
                    case 4:
                        // move lower right
                        // anchor upper left
                        rect.X = LayoutToScreenX(AnchorPt.X);
                        if (e.X > LayoutToScreenX(AnchorPt.X + MIN_CMT_SZ)) {
                            rect.Width = (GridPos(ScreenToLayoutX(e.X)) - AnchorPt.X) * DSF;
                        }
                        else {
                            rect.Width = MIN_CMT_SZ * DSF;
                        }
                        rect.Y = LayoutToScreenY(AnchorPt.Y);
                        if (e.Y > LayoutToScreenY(AnchorPt.Y + MIN_CMT_SZ)) {
                            rect.Height = (GridPos(ScreenToLayoutY(e.Y)) - AnchorPt.Y) * DSF;
                        }
                        else {
                            rect.Height = MIN_CMT_SZ * DSF;
                        }
                        break;
                    }
                    SizingComment = 0;
                    PointF newpos = ScreenToLayout(rect.Location);
                    SizeF newsize = new(rect.Width / DSF, rect.Height / DSF);
                    if (Comment[Selection.Number].Loc != newpos ||
                        Comment[Selection.Number].Size != newsize) {
                        Comment[Selection.Number].Loc = newpos;
                        Comment[Selection.Number].Size = newsize;
                        RecalculateMaxMin();
                        SetScrollBars();
                        MarkAsChanged();
                        DrawLayout();
                    }
                    return;
                }
                // if drag-selecting
                if (DragSelect) {
                    DragSelect = false;
                    ShowDrag = false;
                    GetSelectedObjects(new(Math.Min(LayoutToScreenX(AnchorPt.X), e.X), Math.Min(LayoutToScreenY(AnchorPt.Y), e.Y),
                        Math.Abs(LayoutToScreenX(AnchorPt.X) - e.X), Math.Abs(LayoutToScreenY(AnchorPt.Y) - e.Y)));
                    picDraw.Invalidate();
                }
                break;
            case LayoutTool.Edge1:
            case LayoutTool.Edge2:
            case LayoutTool.Other:
                // reset drawexit flag
                MoveExit = false;
                DrawExit = 0;
                // validate new exit pos
                // by checking cursor (an exit cursor means drop is OK)
                switch (DrawCursor) {
                case LayoutCursor.HorizonExit:
                case LayoutCursor.RightExit:
                case LayoutCursor.BottomExit:
                case LayoutCursor.LeftExit:
                case LayoutCursor.OtherExit:
                    // if target room is same as starting room
                    // get object where exit is being dropped
                    TSel tmpSel = ObjectFromPos(e.Location);
                    if (tmpSel.Type == LayoutSelection.Room) {
                        if (tmpSel.Number == NewExitRoom) {
                            // unless line is at least .4 units (half the room width/height),
                            // assume user doesnt want an exit
                            PointF tmp = ScreenToLayout(e.Location);
                            if (Math.Sqrt((tmp.X - LineStart.X) * (tmp.X - LineStart.X) +
                                (tmp.Y - LineStart.Y) * (tmp.Y - LineStart.Y)) < RM_SIZE / 2) {
                                break;
                            }
                        }
                        // create new exit
                        CreateNewExit(NewExitRoom, tmpSel.Number, NewExitReason);
                        // if drawing exits both ways
                        if (SelTool == LayoutTool.Edge2) {
                            // add reciprocal
                            ExitReason reciprocal = ExitReason.None;
                            switch (NewExitReason) {
                            case ExitReason.Right:
                                reciprocal = ExitReason.Left;
                                break;
                            case ExitReason.Left:
                                reciprocal = ExitReason.Right;
                                break;
                            case ExitReason.Horizon:
                                reciprocal = ExitReason.Bottom;
                                break;
                            case ExitReason.Bottom:
                                reciprocal = ExitReason.Horizon;
                                break;
                            }
                            CreateNewExit(tmpSel.Number, NewExitRoom, reciprocal);
                        }
                    }
                    else {
                         Debug.Assert(TransPt[tmpSel.Number].Count == 1);
                        // dropping on a transfer pt
                        CreateNewExit(NewExitRoom, TransPt[tmpSel.Number].Room[0], NewExitReason);
                    }
                    // redraw to add the new exit
                    DrawLayout();
                    break;
                }
                // invalidate to remove the drag line
                picDraw.Invalidate();
                if (!HoldTool) {
                    // go back to select tool
                    btnEdge1.Checked = false;
                    btnEdge2.Checked = false;
                    btnEdgeOther.Checked = false;
                    btnSelect.Checked = true;
                    SelTool = LayoutTool.Select;
                    spTool.Text = "Tool: Select";
                    HoldTool = false;
                    picDraw.Cursor = Cursors.Default;
                }
                break;
            case LayoutTool.Room:
                // all actions take place in mousedown
                if (!HoldTool) {
                    // go back to select tool
                    btnAddRoom.Checked = false;
                    btnSelect.Checked = true;
                    SelTool = LayoutTool.Select;
                    spTool.Text = "Tool: Select";
                    HoldTool = false;
                    picDraw.Cursor = Cursors.Default;
                }
                break;
            case LayoutTool.Comment:
                PointF mp = ScreenToLayout(e.Location);
                PointF loc = new(GridPos(Math.Min(AnchorPt.X, mp.X)), GridPos(Math.Min(AnchorPt.Y, mp.Y)));
                SizeF size = new(GridPos(Math.Abs(AnchorPt.X - mp.X)), GridPos(Math.Abs(AnchorPt.Y - mp.Y)));
                AddComment(loc, size);
                if (!HoldTool) {
                    // go back to select tool
                    SelTool = LayoutTool.Select;
                    btnAddComment.Checked = false;
                    btnSelect.Checked = true;
                    spTool.Text = "Tool: Select";
                    HoldTool = false;
                    picDraw.Cursor = Cursors.Default;
                    SizingComment = 0;
                }
                picDraw.Invalidate();
                break;
            }
        }

        private void picDraw_MouseDoubleClick(object sender, MouseEventArgs e) {
            // if over a selected comment
            // look for edge exits first
            TSel tmpSel = ExitFromPos(1, e.Location);
            if (tmpSel.Type == LayoutSelection.None) {
                tmpSel = ExitFromPos(0, e.Location);
                if (tmpSel.Type == LayoutSelection.None) {
                    tmpSel = ObjectFromPos(e.Location);
                }
            }
            switch (tmpSel.Type) {
            case LayoutSelection.TransPt:
                SelectOtherTransPt();
                break;
            case LayoutSelection.Room:
            case LayoutSelection.Exit:
            case LayoutSelection.ErrPt:
                OpenRoomLogic(tmpSel);
                break;
            case LayoutSelection.Comment:
                BeginEditComment();
                txtComment.SelectionStart = 0;
                txtComment.SelectionLength = 0;
                txtComment.Select();
                break;
            }
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

        private void picDraw_Paint(object sender, PaintEventArgs e) {
            // we need the current mouse position
            PointF mp = ((Control)sender).PointToClient(Control.MousePosition);

            if (Selection.Type != LayoutSelection.None) {
                // draw handles and/or selection frame
                DrawSelectionHandles(e.Graphics, Selection);
            }

            if (MoveExit || DrawExit != 0) {
                // draw the moving exit line
                Pen exitlinepen = new(Color.Black);
                exitlinepen.DashPattern = [3, 3];
                exitlinepen.DashStyle = DashStyle.Custom;
                e.Graphics.DrawLine(exitlinepen, LayoutToScreen(LineStart), mp);
            }
            else if (MoveObj) {
                // draw the moving object outline
                // convert surface coordinates into world coordinates,
                // apply grid setting, then re-convert back into surface coordinates
                mp.X = GridPos((mp.X - Delta.X - Offset.X) / DSF) * DSF + Offset.X;
                mp.Y = GridPos((mp.Y - Delta.Y - Offset.Y) / DSF) * DSF + Offset.Y;
                Pen dragpen = new(Color.Black);
                dragpen.DashPattern = [3, 2];
                dragpen.DashStyle = DashStyle.Custom;
                float radius;
                switch (Selection.Type) {
                case LayoutSelection.Room:
                    RectangleF rect = new(mp.X, mp.Y, RM_SIZE * DSF, RM_SIZE * DSF);
                    e.Graphics.DrawRectangle(dragpen, rect);
                    break;
                case LayoutSelection.TransPt:
                    rect = new(mp.X, mp.Y, TRANSPT_SIZE * DSF, TRANSPT_SIZE * DSF);
                    e.Graphics.DrawEllipse(dragpen, rect);
                    break;
                case LayoutSelection.ErrPt:
                    rect = new(mp.X, mp.Y, 0.6f * DSF, 0.5196f * DSF);
                    PointF[] errPoints = [new PointF(mp.X, mp.Y),
                        new PointF(mp.X + 0.6f * DSF, mp.Y),
                        new PointF(mp.X + 0.3f * DSF, mp.Y + 0.5196f * DSF)
                    ];
                    using (GraphicsPath path = new GraphicsPath()) {
                        radius = 5.0f * DSF / 40; // radius for rounded corners
                        // Create rounded triangle path
                        path.AddArc(errPoints[0].X + radius * 0.366f, errPoints[0].Y, radius, radius, 150, 120);
                        path.AddArc(errPoints[1].X - radius * 1.366f, errPoints[1].Y, radius, radius, 270, 120);
                        path.AddArc(errPoints[2].X - radius * 0.5f, errPoints[2].Y - radius * 1.5f, radius, radius, 30, 120);
                        path.CloseFigure();
                        e.Graphics.DrawPath(dragpen, path);
                    }
                    break;
                case LayoutSelection.Comment:
                    rect = new(mp.X, mp.Y, Comment[Selection.Number].Size.Width * DSF, Comment[Selection.Number].Size.Height * DSF);
                    radius = 5.0f * DSF / 40; // radius for rounded corners
                    using (GraphicsPath path = new GraphicsPath()) {
                        // Create rounded rectangle path
                        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                        path.CloseFigure();
                        // Draw the border
                        e.Graphics.DrawPath(dragpen, path);
                    }
                    break;
                case LayoutSelection.Multiple:
                    rect = new(mp.X, mp.Y, Selection.Size.Width * DSF, Selection.Size.Height * DSF);
                    e.Graphics.DrawRectangle(dragpen, rect);
                    break;
                }
            }
            else if (SizingComment > 0) {
                // draw the sizing comment outline
                RectangleF rect = new();
                switch (SizingComment) {
                case 1:
                    // move upper left
                    // anchor lower right
                    if (LayoutToScreenX(AnchorPt.X) - mp.X > MIN_CMT_SZ * DSF) {
                        rect.X = LayoutToScreenX(GridPos(ScreenToLayoutX(mp.X)));
                        rect.Width = LayoutToScreenX(AnchorPt.X) - rect.X;
                    }
                    else {
                        rect.X = LayoutToScreenX(AnchorPt.X - MIN_CMT_SZ);
                        rect.Width = MIN_CMT_SZ * DSF;
                    }
                    if (LayoutToScreenY(AnchorPt.Y) - mp.Y > MIN_CMT_SZ * DSF) {
                        rect.Y = LayoutToScreenY(GridPos(ScreenToLayoutY(mp.Y)));
                        rect.Height = LayoutToScreenY(AnchorPt.Y) - rect.Y;
                    }
                    else {
                        rect.Y = LayoutToScreenY(AnchorPt.Y - MIN_CMT_SZ);
                        rect.Height = MIN_CMT_SZ * DSF;
                    }
                    break;
                case 2:
                    // move upper right
                    // anchor lower left
                    rect.X = LayoutToScreenX(AnchorPt.X);
                    if (mp.X > LayoutToScreenX(AnchorPt.X + MIN_CMT_SZ)) {
                        rect.Width = (GridPos(ScreenToLayoutX(mp.X)) - AnchorPt.X) * DSF;
                    }
                    else {
                        rect.Width = MIN_CMT_SZ * DSF;
                    }
                    if (LayoutToScreenY(AnchorPt.Y) - mp.Y > MIN_CMT_SZ * DSF) {
                        rect.Y = LayoutToScreenY(GridPos(ScreenToLayoutY(mp.Y)));
                        rect.Height = LayoutToScreenY(AnchorPt.Y) - rect.Y;
                    }
                    else {
                        rect.Y = LayoutToScreenY(AnchorPt.Y - MIN_CMT_SZ);
                        rect.Height = MIN_CMT_SZ * DSF;
                    }
                    break;
                case 3:
                    // move lower left
                    // anchor upper right
                    if (LayoutToScreenX(AnchorPt.X) - mp.X > MIN_CMT_SZ * DSF) {
                        rect.X = LayoutToScreenX(GridPos(ScreenToLayoutX(mp.X)));
                        rect.Width = LayoutToScreenX(AnchorPt.X) - rect.X;
                    }
                    else {
                        rect.X = LayoutToScreenX(AnchorPt.X - MIN_CMT_SZ);
                        rect.Width = MIN_CMT_SZ * DSF;
                    }
                    rect.Y = LayoutToScreenY(AnchorPt.Y);
                    if (mp.Y > LayoutToScreenY(AnchorPt.Y + MIN_CMT_SZ)) {
                        rect.Height = (GridPos(ScreenToLayoutY(mp.Y)) - AnchorPt.Y) * DSF;
                    }
                    else {
                        rect.Height = MIN_CMT_SZ * DSF;
                    }
                    break;
                case 4:
                    // move lower right
                    // anchor upper left
                    rect.X = LayoutToScreenX(AnchorPt.X);
                    if (mp.X > LayoutToScreenX(AnchorPt.X + MIN_CMT_SZ)) {
                        rect.Width = (GridPos(ScreenToLayoutX(mp.X)) - AnchorPt.X) * DSF;
                    }
                    else {
                        rect.Width = MIN_CMT_SZ * DSF;
                    }
                    rect.Y = LayoutToScreenY(AnchorPt.Y);
                    if (mp.Y > LayoutToScreenY(AnchorPt.Y + MIN_CMT_SZ)) {
                        rect.Height = (GridPos(ScreenToLayoutY(mp.Y)) - AnchorPt.Y) * DSF;
                    }
                    else {
                        rect.Height = MIN_CMT_SZ * DSF;
                    }
                    break;
                }
                // draw a dotted line outline
                using Pen borderPen = new Pen(Color.Black);
                borderPen.DashPattern = [3, 3];
                borderPen.DashStyle = DashStyle.Custom;
                float radius = 5.0f * DSF / 40; // radius for rounded corners
                using (GraphicsPath path = new GraphicsPath()) {
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.CloseFigure();
                    e.Graphics.DrawPath(borderPen, path);
                }
            }
            else if (ShowExitStart != ExitReason.None && ShowExitStart != ExitReason.Other) {
                Pen startpen = new(Color.Red, 4);
                e.Graphics.DrawLine(startpen, LayoutToScreen(LineStart), LayoutToScreen(LineEnd));
            }
            else if (ShowDrag) {
                Pen pen = new(Color.Black);
                pen.DashPattern = [5, 5];
                pen.DashStyle = DashStyle.Custom;
                e.Graphics.DrawRectangle(pen, Math.Min(LayoutToScreenX(AnchorPt.X), mp.X), Math.Min(LayoutToScreenY(AnchorPt.Y), mp.Y),
                    Math.Abs(LayoutToScreenX(AnchorPt.X) - mp.X), Math.Abs(LayoutToScreenY(AnchorPt.Y) - mp.Y));
            }
        }

        private void picDraw_KeyDown(object sender, KeyEventArgs e) {
            float sngOffset;

            // arrow keys move the selection;
            //   - no shift = move one small grid amount
            //   - shift key = move one large grid amount
            //   - ctrl key = move one pixel
            if (Selection.Type == LayoutSelection.None) {
                return;
            }
            switch (e.Modifiers) {
            case Keys.None:
                switch (e.KeyCode) {
                case Keys.Up or Keys.Down or Keys.Left or Keys.Right:
                    if (WinAGISettings.LEUseGrid.Value) {
                        // offset is one grid amount
                        sngOffset = WinAGISettings.LEGridMinor.Value * DSF;
                    }
                    else {
                        // default is 0.1
                        sngOffset = 0.1f * DSF;
                    }
                    KeyMoveSelection(e.KeyCode, sngOffset, false);
                    break;
                }
                break;
            case Keys.Shift:
                switch (e.KeyCode) {
                case Keys.Up or Keys.Down or Keys.Left or Keys.Right:
                    if (WinAGISettings.LEUseGrid.Value) {
                        // offset is major grid amount
                        sngOffset = WinAGISettings.LEGridMajor.Value * DSF;
                    }
                    else {
                        // default is 0.8
                        sngOffset = 0.8f * DSF;
                    }

                    KeyMoveSelection(e.KeyCode, sngOffset, false);
                    break;
                }
                break;
            case Keys.Control:
                switch (e.KeyCode) {
                case Keys.Up or Keys.Down or Keys.Left or Keys.Right:
                    // offset is one pixel
                    sngOffset = 1;
                    KeyMoveSelection(e.KeyCode, sngOffset, true);
                    break;
                }
                break;
            }
        }

        private void txtComment_TextChanged(object sender, EventArgs e) {
            if (txtComment.Visible) {
                MarkAsChanged();
            }
        }

        private void txtComment_KeyDown(object sender, KeyEventArgs e) {

            switch (e.KeyCode) {
            case Keys.Enter:
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtComment.Visible = false;
                picDraw.Select();
                break;
            case Keys.Escape:
                // restore original text
                txtComment.Text = Comment[(int)txtComment.Tag].Text;
                e.Handled = true;
                e.SuppressKeyPress = true;
                txtComment.Visible = false;
                picDraw.Select();
                break;
            }
        }

        private void txtComment_Enter(object sender, EventArgs e) {
            // need to make delete key available
            mnuDelete.ShortcutKeys = Keys.None;
        }

        private void txtComment_Leave(object sender, EventArgs e) {
            if (EditCommentText) {
                EditCommentText = false;
                // make sure the text box is hidden
                txtComment.Visible = false;
                // update the comment text
                Comment[(int)txtComment.Tag].Text = txtComment.Text.TrimEnd();
                // redraw the comment
                DrawLayout(LayoutSelection.Comment, (int)txtComment.Tag);
                // restore shortcut to menu item
                mnuDelete.ShortcutKeys = Keys.Delete;
            }
        }

        private void hScrollBar1_Enter(object sender, EventArgs e) {
            // always return focus to the draw surface
            picDraw.Select();
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

        private void vScrollBar1_Enter(object sender, EventArgs e) {
            // always return focus to the draw surface
            picDraw.Select();
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
            spCurX.Text = "";
            // 
            // spCurY
            // 
            spCurY.AutoSize = false;
            spCurY.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spCurY.BorderStyle = Border3DStyle.SunkenInner;
            spCurY.Name = "spCurY";
            spCurY.Size = new System.Drawing.Size(70, 18);
            spCurY.Text = "";
            // 
            // spScale
            // 
            spScale.AutoSize = false;
            spScale.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spScale.BorderStyle = Border3DStyle.SunkenInner;
            spScale.Name = "spScale";
            spScale.Size = new System.Drawing.Size(66, 18);
            spScale.Text = "";
            // 
            // spTool
            // 
            spTool.AutoSize = false;
            spTool.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spTool.BorderStyle = Border3DStyle.SunkenInner;
            spTool.Name = "spTool";
            spTool.Size = new System.Drawing.Size(135, 18);
            spTool.Text = "Tool: Select";
            spTool.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // spID
            // 
            spID.AutoSize = false;
            spID.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spID.BorderStyle = Border3DStyle.SunkenInner;
            spID.Name = "spID";
            spID.Size = new System.Drawing.Size(90, 18);
            spID.Text = "";
            spID.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // spType
            // 
            spType.AutoSize = false;
            spType.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spType.BorderStyle = Border3DStyle.SunkenInner;
            spType.Name = "spType";
            spType.Size = new System.Drawing.Size(90, 18);
            spType.Text = "";
            spType.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // spRoom1
            // 
            spRoom1.AutoSize = false;
            spRoom1.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spRoom1.BorderStyle = Border3DStyle.SunkenInner;
            spRoom1.Name = "spRoom1";
            spRoom1.Size = new System.Drawing.Size(175, 18);
            spRoom1.Text = "";
            spRoom1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // spRoom2
            // 
            spRoom2.AutoSize = false;
            spRoom2.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Top | ToolStripStatusLabelBorderSides.Right | ToolStripStatusLabelBorderSides.Bottom;
            spRoom2.BorderStyle = Border3DStyle.SunkenInner;
            spRoom2.Name = "spRoom2";
            spRoom2.Size = new System.Drawing.Size(175, 18);
            spRoom2.Text = "";
            spRoom2.TextAlign = ContentAlignment.MiddleLeft;

            spStatus = MDIMain.spStatus;
        }

        internal void InitFonts() {
            float fontSize = (float)(DSF / 12f);
            txtComment.Font = layoutFont = new(WinAGISettings.EditorFontName.Value, fontSize, FontStyle.Regular);
            transptFont = new(WinAGISettings.EditorFontName.Value, fontSize * 1.5f, FontStyle.Bold);
        }

        private void KeyMoveSelection(Keys KeyCode, float sngOffset, bool NoGrid) {
            float sngNewX = LayoutToScreenX(Selection.Loc.X);
            float sngNewY = LayoutToScreenY(Selection.Loc.Y);

            switch (KeyCode) {
            case Keys.Up:
                sngNewY -= sngOffset;
                break;
            case Keys.Down:
                sngNewY += sngOffset;
                break;
            case Keys.Left:
                sngNewX -= sngOffset;
                break;
            case Keys.Right:
                sngNewX += sngOffset;
                break;
            }

            // reposition the selection
            DropSelection(sngNewX, sngNewY, NoGrid);
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
                    if (ExtractLayout()) {
                        // Force save the extracted logic
                        SaveLayout();
                        return true;
                    }
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
            List<string> warninglist = [];
            bool blnWarning = false;
            string strLine;

            // make sure everything is cleared out
            DeselectObj();

            Selection.Type = LayoutSelection.None;
            Selection.Number = 0;
            Selection.ExitID = "";
            Selection.Leg = ExitLeg.NoTransPt;
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
                HandleSize = DrawScale + 2;
                InitFonts(); // reset font size based on DSF
                spScale.Text = "Scale: " + DrawScale;

                // objects
                strLine = ReadNextBlock(sr);
                while (strLine != null) {
                    LayoutFileData layoutobj = null;
                    try {
                        layoutobj = JsonSerializer.Deserialize<LayoutFileData>(strLine);
                    }
                    catch (JsonException) {
                        warninglist.Add("Error decoding layout object:\n" + strLine);
                        blnWarning = true;
                        break;
                    }
                    catch (Exception ex) {
                        warninglist.Add("Error decoding layout object:\n" + strLine + "\n\n" + ex.Message);
                        blnWarning = true;
                        break;
                    }
                    if (layoutobj is LFDRoom room) {
                        Room[room.Index].Visible = room.Visible;
                        Room[room.Index].ShowPic = room.ShowPic;
                        Room[room.Index].Loc = room.Loc;
                        Room[room.Index].Exits.CopyFrom(room.Exits);
                        Room[room.Index].Order = ObjOrder.Count;
                        ObjOrder.Add(new(LayoutSelection.Room, room.Index));
                    }
                    else if (layoutobj is LFDTransPt transpt) {
                        TransPt[transpt.Index].Loc = transpt.Loc;
                        TransPt[transpt.Index].Room = [(byte)transpt.Rooms[0], (byte)transpt.Rooms[1]];
                        TransPt[transpt.Index].ExitID = transpt.ExitID;
                        TransPt[transpt.Index].Count = transpt.Count;
                        TransPt[transpt.Index].Order = ObjOrder.Count;
                        ObjOrder.Add(new ObjInfo(LayoutSelection.TransPt, transpt.Index));
                    }
                    else if (layoutobj is LFDErrPt errpt) {
                        ErrPt[errpt.Index].Visible = errpt.Visible;
                        ErrPt[errpt.Index].Loc = errpt.Loc;
                        ErrPt[errpt.Index].ExitID = errpt.ExitID;
                        ErrPt[errpt.Index].FromRoom = errpt.FromRoom;
                        ErrPt[errpt.Index].Room = errpt.Room;
                        ErrPt[errpt.Index].Order = ObjOrder.Count;
                        ObjOrder.Add(new ObjInfo(LayoutSelection.ErrPt, errpt.Index));
                    }
                    else if (layoutobj is LFDComment comment) {
                        Comment[comment.Index].Visible = comment.Visible;
                        Comment[comment.Index].Loc = comment.Loc;
                        Comment[comment.Index].Size = comment.Size;
                        Comment[comment.Index].Text = comment.Text;
                        Comment[comment.Index].Order = ObjOrder.Count;
                        ObjOrder.Add(new ObjInfo(LayoutSelection.Comment, comment.Index));
                    }
                    else if (layoutobj is LFDRenumber renum) {
                        ObjInfo obj = ObjOrder[Room[renum.OldNumber].Order];
                        ObjOrder[ObjOrder.IndexOf(obj)] = new ObjInfo(LayoutSelection.Room, renum.Index);
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
                                            ExitType type = Room[i].Exits[j].Type;
                                            int typeindex = Room[i].Exits[j].TypeIndex;
                                            // in case an err pt is found, deal with it
                                            if (type == ExitType.Error) {
                                                if (needpos) {
                                                    // move it here
                                                    Room[showroom.Index].Loc.X = GridPos(ErrPt[typeindex].Loc.X - 0.1f);
                                                    Room[showroom.Index].Loc.Y = GridPos(ErrPt[typeindex].Loc.Y - 0.1402f);
                                                    // only the first errpt is replaced by the new room
                                                    int order = ErrPt[typeindex].Order;
                                                    ObjInfo obj = new(LayoutSelection.Room, showroom.Index);
                                                    ObjOrder[order] = obj;
                                                    Room[showroom.Index].Order = order;
                                                    needpos = false;
                                                }
                                                else {
                                                    // more than one matching errpt is just removed
                                                    RemoveObjFromStack(ErrPt[typeindex].Order);
                                                }
                                                // hide errpt
                                                ErrPt[typeindex].Visible = false;
                                                ErrPt[typeindex].ExitID = "";
                                                ErrPt[typeindex].FromRoom = 0;
                                                ErrPt[typeindex].Room = 0;
                                                ErrPt[typeindex].Order = 0;
                                                // clear transfer marker
                                                Room[i].Exits[j].TypeIndex = 0;
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
                            ObjInfo obj = new(LayoutSelection.Room, showroom.Index);
                            ObjOrder.Add(obj);
                        }
                        // mark as changed to force update
                        MarkAsChanged();
                    }
                    else if (layoutobj is LFDHideRoom hideroom) {
                        Debug.Assert(Room[hideroom.Index].Visible);
                        // first, check for any existing transfers or errpts FROM the room being hidden
                        for (int i = 0; i < Room[hideroom.Index].Exits.Count; i++) {
                            ExitType type = Room[hideroom.Index].Exits[i].Type;
                            int transfer = Room[hideroom.Index].Exits[i].TypeIndex;
                            if (type == ExitType.Transfer) {
                                // remove the transfer point
                                DeleteLoadTransfer(transfer);
                            }
                            else if (type == ExitType.Error) {
                                // remove errpt
                                RemoveObjFromStack(ErrPt[transfer].Order);
                                ErrPt[transfer].Visible = false;
                                ErrPt[transfer].ExitID = "";
                                ErrPt[transfer].FromRoom = 0;
                                ErrPt[transfer].Room = 0;
                                ErrPt[transfer].Loc = new();
                            }
                        }
                        // step through all other exits, 
                        for (int i = 1; i < 256; i++) {
                            // only need to check rooms that are currently visible
                            if (i != hideroom.Index && Room[i].Visible) {
                                for (int j = 0; j < Room[i].Exits.Count; j++) {
                                    if (Room[i].Exits[j].Room == hideroom.Index) {
                                        // check for transfer pt
                                        if (Room[i].Exits[j].TypeIndex > 0) {
                                            // remove it
                                            DeleteLoadTransfer(Room[i].Exits[j].TypeIndex);
                                        }
                                        // if room is deleted, add an error point
                                        if (!EditGame.Logics.Contains(hideroom.Index)) {
                                            // add an ErrPt if not already
                                            if (Room[i].Exits[j].Type != ExitType.Error) {
                                                int errptnum = 1;
                                                while (ErrPt[errptnum].Visible) {
                                                    errptnum++;
                                                    if (errptnum >= ErrPt.Length) {
                                                        // no more error points available
                                                        Array.Resize(ref ErrPt, ErrPt.Length + 16);
                                                        break;
                                                    }
                                                }
                                                Room[i].Exits[j].Type = ExitType.Error;
                                                Room[i].Exits[j].TypeIndex = errptnum;
                                                ErrPt[errptnum].Visible = true;
                                                ErrPt[errptnum].Loc = new(Room[hideroom.Index].Loc.X + 0.1f, Room[hideroom.Index].Loc.Y + 0.2268f);
                                                ErrPt[errptnum].Order = ObjOrder.Count;
                                                ObjInfo obj = new(LayoutSelection.ErrPt, errptnum);
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
                        RemoveObjFromStack(Room[hideroom.Index].Order);
                        // mark as changed to force update
                        MarkAsChanged();
                    }
                    strLine = ReadNextBlock(sr);
                }
                // confirm error points are valid
                for (int i = 1; i < ErrPt.Length; i++) {
                    if (ErrPt[i].Visible) {
                        // if target room does exist, this should be a hidden exit, 
                        // not an error point
                        if (ErrPt[i].Room > 0 && EditGame.Logics.Contains(ErrPt[i].Room)) {
                            warninglist.Add("invalid error point (" + ErrPt[i].Room + ")");
                            blnWarning = true;
                            // delete the err point
                            RemoveObjFromStack(ErrPt[i].Order);
                            ErrPt[i].Visible = false;
                            ErrPt[i].ExitID = "";
                            ErrPt[i].FromRoom = 0;
                            ErrPt[i].Room = 0;
                            ErrPt[i].Loc = new();
                        }
                        // verify the matching exit is correct
                        if (ErrPt[i].Visible) {
                            bool badexit = true;
                            for (int j = 0; j < Room[ErrPt[i].FromRoom].Exits.Count; j++) {
                                if (Room[ErrPt[i].FromRoom].Exits[j].ID == ErrPt[i].ExitID) {
                                    // confirm it points to this error
                                    if (Room[ErrPt[i].FromRoom].Exits[j].Type == ExitType.Error &&
                                        Room[ErrPt[i].FromRoom].Exits[j].TypeIndex == i &&
                                        Room[ErrPt[i].FromRoom].Exits[j].Room == ErrPt[i].Room) {
                                        // ok
                                        badexit = false;
                                    }
                                    else {
                                        // remove the bad exit?
                                        break;
                                    }
                                    break;
                                }
                            }
                            if (badexit) {
                                // must be an invalid err pt
                                warninglist.Add("invalid error point (" + ErrPt[i].ExitID + ")");
                                blnWarning = true;
                                // delete the err point
                                RemoveObjFromStack(ErrPt[i].Order);
                                ErrPt[i].Visible = false;
                                ErrPt[i].ExitID = "";
                                ErrPt[i].FromRoom = 0;
                                ErrPt[i].Room = 0;
                                ErrPt[i].Loc = new();
                            }
                        }
                    }
                }
                // need to reset all exit locations
                if (!LoadExits(warninglist)) {
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
                warninglist.Insert(0, "Layout Data File Error Report");
                warninglist.Insert(1, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                warninglist.Insert(2, "");

                try {
                    StreamWriter sw = new StreamWriter(EditGame.GameDir + "layout_errors.txt", false, Encoding.UTF8);
                    foreach (string line in warninglist) {
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
                            exit.Type = Room[index].Exits[i].Type;
                            exit.TypeIndex = Room[index].Exits[i].TypeIndex;
                            exit.Leg = Room[index].Exits[i].Leg;
                            if (exit.Type == ExitType.Transfer) {
                                TransPt[exit.TypeIndex].ExitID[exit.Leg] = exit.ID;
                            }
                            else if (exit.Type == ExitType.Error) {
                                ErrPt[exit.TypeIndex].ExitID = exit.ID;
                            }
                            Room[index].Exits.Remove(i);
                            break;
                        }
                        // check this new exit for matching transfers
                        for (int j = 0; j < Room[exit.Room].Exits.Count; j++) {
                            if (Room[exit.Room].Exits[j].Type == ExitType.Transfer &&
                                TransPt[Room[exit.Room].Exits[j].TypeIndex].Count == 1) {
                                // matching reason is opposite of current reason
                                ExitReason matchreason = ExitReason.None;
                                switch (exit.Reason) {
                                case ExitReason.Horizon:
                                    matchreason = ExitReason.Bottom;
                                    break;
                                case ExitReason.Right:
                                    matchreason = ExitReason.Left;
                                    break;
                                case ExitReason.Bottom:
                                    matchreason = ExitReason.Horizon;
                                    break;
                                case ExitReason.Left:
                                    matchreason = ExitReason.Right;
                                    break;
                                case ExitReason.Other:
                                    matchreason = ExitReason.Other;
                                    break;
                                }
                                // if this exit is reciprocal of 
                                if (Room[exit.Room].Exits[j].Room == index &&
                                    Room[exit.Room].Exits[j].Reason == matchreason) {
                                    // use this transfer
                                    exit.Type = ExitType.Transfer;
                                    exit.TypeIndex = Room[exit.Room].Exits[j].TypeIndex;
                                    exit.Leg = 1;
                                    TransPt[Room[exit.Room].Exits[j].TypeIndex].Count = 2;
                                    TransPt[Room[exit.Room].Exits[j].TypeIndex].ExitID[1] = exit.ID;
                                    TransPt[Room[exit.Room].Exits[j].TypeIndex].Room[1] = (byte)index;
                                    break;
                                }
                            }
                        }
                        // check if this new exit is error
                        if (exit.Room == 0 || !EditGame.Logics.Contains(exit.Room)) {
                            int errptnum = 1;
                            while (ErrPt[errptnum].Visible) {
                                errptnum++;
                                if (errptnum >= ErrPt.Length) {
                                    Array.Resize(ref ErrPt, ErrPt.Length + 16);
                                    break;
                                }
                            }
                            exit.TypeIndex = errptnum;
                            exit.Type = ExitType.Error;
                            exit.Room = 0;
                            ErrPt[errptnum].Visible = true;
                            ErrPt[errptnum].Loc = GetInsertPos(Room[index].Loc);
                            ObjInfo obj = new(LayoutSelection.ErrPt, errptnum);
                            ErrPt[errptnum].Order = ObjOrder.Count;
                            ObjOrder.Add(obj);
                            ErrPt[errptnum].ExitID = exit.ID;
                            ErrPt[errptnum].Room = exit.Room;
                            ErrPt[errptnum].FromRoom = index;
                        }
                    }
                }
                // any existing exits left need to be checked for errpts
                // and transfers, which need to be removed
                for (int i = 0; i < Room[index].Exits.Count; i++) {
                    ExitType type = Room[index].Exits[i].Type;
                    int transfer = Room[index].Exits[i].TypeIndex;
                    // only erpt and transfer points are affected
                    if (type == ExitType.Transfer) {
                        // remove this leg of transpt
                        TransPt[transfer].Count--;
                        if (TransPt[transfer].Count == 0) {
                            // no more legs, delete the transfer point
                            TransPt[transfer].ExitID[0] = "";
                            TransPt[transfer].ExitID[1] = "";
                            TransPt[transfer].Room[0] = 0;
                            TransPt[transfer].Room[1] = 0;
                            RemoveObjFromStack(TransPt[transfer].Order);
                        }
                        else {
                            // determine which leg to clear
                            int leg = Room[index].Exits[i].Leg;
                            // clear this leg
                            TransPt[transfer].ExitID[leg] = "";
                            TransPt[transfer].Room[leg] = 0;
                        }
                    }
                    else if (type == ExitType.Error) {
                        // remove errpt
                        RemoveObjFromStack(ErrPt[transfer].Order);
                        ErrPt[transfer].Visible = false;
                        ErrPt[transfer].ExitID = "";
                        ErrPt[transfer].FromRoom = 0;
                        ErrPt[transfer].Room = 0;
                        ErrPt[transfer].Loc = new();
                    }
                }
                // now reset band add the new exits
                Room[index].Exits.Clear();
                Room[index].Exits.CopyFrom(exits);
            }

            void DeleteLoadTransfer(int transfer) {
                // remove the transfer point from the list
                Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].Type = ExitType.Normal;
                Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].TypeIndex = 0;
                Debug.Assert(Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].Leg == 0);
                if (TransPt[transfer].Count == 2) {
                    Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Type = ExitType.Normal;
                    Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].TypeIndex = 0;
                    Debug.Assert(Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Leg == 1);
                    Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Leg = 0;
                }
                TransPt[transfer].Count = 0;
                TransPt[transfer].ExitID[0] = "";
                TransPt[transfer].ExitID[1] = "";
                TransPt[transfer].Room[0] = 0;
                TransPt[transfer].Room[1] = 0;
                RemoveObjFromStack(TransPt[transfer].Order);
            }
        }

        private bool GetV2LayoutData(string layoutfile) {
            // This function is used to extract the layout data from a v2
            // layout file and populate the layout editor's data structures.

            // v12 File format:
            // Line 1: cstr(MajorVer) + cstr(MinorVer)
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
            // v is visible property (only used by room object; all others are always true)
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
            // v2 doesn't store the objects in order, so 
            // this array is used to track their order
            ObjInfo[] objOrder = new ObjInfo[1024];
            int objCount = 0;

            // use deselect obj to configure toolbar,statusbar and menus
            DeselectObj();

            Selection.Type = LayoutSelection.None;
            Selection.Number = 0;
            Selection.ExitID = "";
            Selection.Leg = ExitLeg.NoTransPt;
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
                        // try next line
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
                    if (objOrder[tmpOrder].Type != LayoutSelection.None) {
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
                    objOrder[tmpOrder].Type = LayoutSelection.Comment;
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
                    objOrder[tmpOrder].Type = LayoutSelection.ErrPt;
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
                        break; // skip to next line
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
                    objOrder[tmpOrder].Type = LayoutSelection.Room;
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
                    objOrder[tmpOrder].Type = LayoutSelection.TransPt;
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

                    // get exit info
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
                            tmpExits.Add(strExit[0].IntVal(), strExit[1].IntVal(), (ExitReason)strExit[2].IntVal(), strExit[3].IntVal(), 0, 0).Status = ExitStatus.OK;
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
                                    exit.Type = Room[lngNumber].Exits[i].Type;
                                    exit.TypeIndex = Room[lngNumber].Exits[i].TypeIndex;
                                    exit.Leg = Room[lngNumber].Exits[i].Leg;
                                    switch (exit.Type) {
                                    case ExitType.Transfer:
                                        TransPt[exit.TypeIndex].ExitID[exit.Leg] = exit.ID;
                                        break;
                                    case ExitType.Error:
                                        ErrPt[exit.TypeIndex].ExitID = exit.ID;
                                        break;
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
                                    ErrPt[errptnum].Loc = GetInsertPos(Room[lngNumber].Loc);
                                    objOrder[objCount].Type = LayoutSelection.ErrPt;
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
                            ExitType type = Room[lngNumber].Exits[i].Type;
                            int transfer = Room[lngNumber].Exits[i].TypeIndex;
                            // only erpt and transfer points are affected
                            switch (type) {
                            case ExitType.Transfer:
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
                                break;
                            case ExitType.Error:
                                // remove errpt
                                CompactLoadList(ErrPt[transfer].Order);
                                ErrPt[transfer].Visible = false;
                                ErrPt[transfer].ExitID = "";
                                ErrPt[transfer].FromRoom = 0;
                                ErrPt[transfer].Room = 0;
                                ErrPt[transfer].Loc = new();
                                ErrPt[transfer].Order = 0;
                                break;
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
                                                ExitType type = Room[i].Exits[j].Type;
                                                int transfer = Room[i].Exits[j].TypeIndex;
                                                // in case an err pt is found, deal with it
                                                if (type == ExitType.Error) {
                                                    if (needpos) {
                                                        // move it here
                                                        Room[lngNumber].Loc.X = GridPos(ErrPt[transfer].Loc.X - 0.1f);
                                                        Room[lngNumber].Loc.Y = GridPos(ErrPt[transfer].Loc.Y - 0.1402f);
                                                        // only the first errpt is replaced by the new room
                                                        int order = ErrPt[transfer].Order;
                                                        objOrder[order].Number = lngNumber;
                                                        objOrder[order].Type = LayoutSelection.Room;
                                                        Room[lngNumber].Order = order;
                                                        needpos = false;
                                                    }
                                                    else {
                                                        // more than one matching errpt is just removed
                                                        CompactLoadList(ErrPt[transfer].Order);
                                                    }
                                                    // hide errpt
                                                    ErrPt[transfer].Visible = false;
                                                    ErrPt[transfer].ExitID = "";
                                                    ErrPt[transfer].FromRoom = 0;
                                                    ErrPt[transfer].Room = 0;
                                                    ErrPt[transfer].Order = 0;
                                                    // clear transfer marker
                                                    Room[i].Exits[j].Type = ExitType.Normal;
                                                    Room[i].Exits[j].TypeIndex = 0;
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
                            objOrder[objCount].Type = LayoutSelection.Room;
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
                                ExitType type = Room[lngNumber].Exits[i].Type;
                                int transfer = Room[lngNumber].Exits[i].TypeIndex;
                                switch (type) {
                                case ExitType.Transfer:
                                    // remove the transfer point
                                    DeleteLoadTransfer(transfer);
                                    break;
                                case ExitType.Error:
                                    // remove errpt
                                    CompactLoadList(ErrPt[transfer].Order);
                                    ErrPt[transfer].Visible = false;
                                    ErrPt[transfer].ExitID = "";
                                    ErrPt[transfer].FromRoom = 0;
                                    ErrPt[transfer].Room = 0;
                                    ErrPt[transfer].Loc = new();
                                    ErrPt[transfer].Order = 0;
                                    break;
                                }
                            }
                            // step through all other exits
                            bool adderrpt = !EditGame.Logics.Contains(lngNumber);
                            for (int i = 1; i < 256; i++) {
                                // only need to check rooms that are currently visible
                                if (i != lngNumber && Room[i].Visible) {
                                    for (int j = 0; j < Room[i].Exits.Count; j++) {
                                        if (Room[i].Exits[j].Room == lngNumber && Room[i].Exits[j].Status != ExitStatus.Deleted) {
                                            // check for transfer pt
                                            if (Room[i].Exits[j].Type == ExitType.Transfer) {
                                                // remove it
                                                DeleteLoadTransfer(Room[i].Exits[j].TypeIndex);
                                            }
                                            // add an ErrPt if room is not a valid logic
                                            if (adderrpt && Room[i].Exits[j].Type != ExitType.Error) {
                                                int errptnum = 1;
                                                while (ErrPt[errptnum].Visible) {
                                                    errptnum++;
                                                    if (errptnum >= ErrPt.Length) {
                                                        // no more error points available
                                                        blnError = true;
                                                        break;
                                                    }
                                                }
                                                Room[i].Exits[j].Type = ExitType.Error;
                                                Room[i].Exits[j].TypeIndex = errptnum;
                                                ErrPt[errptnum].Visible = true;
                                                ErrPt[errptnum].Loc = GetInsertPos(Room[lngNumber].Loc);
                                                objOrder[objCount].Type = LayoutSelection.ErrPt;
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
                            case LayoutSelection.Room:
                                Room[objOrder[i].Number].Order = i;
                                break;
                            case LayoutSelection.TransPt:
                                TransPt[objOrder[i].Number].Order = i;
                                break;
                            case LayoutSelection.ErrPt:
                                ErrPt[objOrder[i].Number].Order = i;
                                break;
                            case LayoutSelection.Comment:
                                Comment[objOrder[i].Number].Order = i;
                                break;
                            }
                        }
                        objCount--;
                    }
                    void DeleteLoadTransfer(int transfer) {
                        // remove the transfer point from the list
                        Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].Type = ExitType.Normal;
                        Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].TypeIndex = 0;
                        Debug.Assert(Room[TransPt[transfer].Room[0]].Exits[TransPt[transfer].ExitID[0]].Leg == 0);
                        if (TransPt[transfer].Count == 2) {
                            Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].Type = ExitType.Normal;
                            Room[TransPt[transfer].Room[1]].Exits[TransPt[transfer].ExitID[1]].TypeIndex = 0;
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
            // Force save the imported logic
            SaveLayout();
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
                    strLine = "Errors were encountered in the version 2 layout file. WinAGI " +
                                "attempted to repair them.\nYou should make sure all rooms, " +
                                "exits and comments are correctly placed and  then save " +
                                "the layout. Consider using the 'Repair Layout' option if " +
                                "there are significant discrepancies.\n\nA list of specific " +
                                "issues encountered can be found in the 'layout_errors.txt' " +
                                "file in your game directory.";
                    MessageBox.Show(MDIMain,
                        strLine,
                        "Import Layout Editor Errors",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information, 0, 0,
                        WinAGIHelp,
                        "htm\\winagi\\Layout_Editor.htm#layoutrepair");
                }
                catch (Exception ex) {
                    // if error, just ignore it
                    strLine = "Errors were encountered in the version 2 layout file. WinAGI " +
                                "attempted to repair them.\nYou should make sure all rooms, " +
                                "exits and comments are correctly placed and  then save " +
                                "the layout. Consider using the 'Repair Layout' option if " +
                                "there are significant discrepancies.";
                    MessageBox.Show(MDIMain,
                        strLine,
                        "Import Layout Editor Errors",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error, 0, 0,
                        WinAGIHelp,
                        "htm\\winagi\\Layout_Editor.htm#layoutrepair");
                }
            }
            else {
                // notify user of the conversion
                MessageBox.Show(MDIMain,
                    "WinAGI has successfully converted the version 2 layout file " +
                    "for this game and saved it in the new version 3 format.",
                    "Convert V2 Layout Editor File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            // mark as changed to force update
            MarkAsChanged();
            return true;
        }

        private static AGIExits ParseExits(string[] strData) {
            // parses the string that contains exit info that comes from the layout editor
            // data file
            // ver 12+:    R|##|v|o|p|x|y|index:room:reason:style:xfer:leg|...
            AGIExits retval = new();

            for (int i = 7; i < strData.Length; i++) {
                string[] strExit = strData[i].Split(":");
                // should be six elements
                if (strExit.Length == 6) {

                    int id = strExit[0].IntVal();
                    int room = strExit[1].IntVal();
                    ExitReason reason = (ExitReason)strExit[2].IntVal();
                    int style = strExit[3].IntVal();
                    int transfer = strExit[4].IntVal();
                    ExitType type = ExitType.Normal;
                    if (transfer < 0) {
                        type = ExitType.Error;
                        transfer = -transfer;
                    }
                    else if (transfer > 0) {
                        type = ExitType.Transfer;
                    }
                    int leg = strExit[5].IntVal();
                    // add new exit
                    retval.Add(id, room, reason, style, type, transfer, leg).Status = ExitStatus.OK;
                }
            }
            return retval;
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
                                Room[j].Loc = GetInsertPos(Rm1Loc, 0, 0.1f, true);
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
                            MDIMain.RefreshPropertyGrid(AGIResType.Logic, i);
                            strErrList.Add("Logic " + i + " is mismarked as a room. IsRoom property has been reset to false.");
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
                if (ExtractLayout()) {
                    // Force save the extracted logic
                    SaveLayout();
                    return true;
                }
            }
            return false;
        }

        private bool ExtractLayout(bool SavePos = false) {
            // if SavePos is true, then we keep the position information for
            // objects already on screen, and only update IsRoom status and exits

            // Variable declarations
            int i, j;
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
                        ObjInfo obj = new(LayoutSelection.Room, agl.Number);
                        ObjOrder.Add(obj);
                    }
                    // update the exit/entrance matrix for all exits
                    for (i = 0; i < Room[agl.Number].Exits.Count; i++) {
                        switch (Room[agl.Number].Exits[i].Reason) {
                        case ExitReason.Horizon:
                        case ExitReason.Right:
                        case ExitReason.Bottom:
                        case ExitReason.Left:
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
                                ObjInfo obj = new(LayoutSelection.Room, Room[agl.Number].Exits[i].Room);
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
                                        for (int idx = 0; idx < Room[bytRoom].Exits.Count; idx++) {
                                            if (Room[bytRoom].Exits[idx].Reason == ExitReason.Other) {
                                                // if other exit is a single room and room is not zero
                                                if (GroupCount[ELRoom[Room[bytRoom].Exits[idx].Room].Group] == 1 && Room[bytRoom].Exits[idx].Room != 0) {
                                                    // insert the room
                                                    tmpCoord = GetInsertPos(Room[EditGame.Logics[bytRoom].Number].Loc, i);
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
                            ObjInfo obj = new(LayoutSelection.ErrPt, EPCount);
                            ObjOrder.Add(obj);
                            // mark exit
                            Room[i].Exits[j].Type = ExitType.Error;
                            Room[i].Exits[j].TypeIndex = EPCount;
                            tmpCoord = new();
                            switch (Room[i].Exits[j].Reason) {
                            case ExitReason.None:
                            case ExitReason.Other:
                                // position first around from room
                                tmpCoord = GetInsertPos(Room[i].Loc, 0, 1);
                                // if valid spot not found (coords didnt change)
                                if (tmpCoord.X == Room[i].Loc.X) {
                                    // move it directly above
                                    tmpCoord.Y = Room[i].Loc.Y - 1.5f;
                                }
                                break;
                            case ExitReason.Horizon:
                                // position around point above
                                tmpCoord = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y - 1.5f);
                                break;
                            case ExitReason.Bottom:
                                // position around point below
                                tmpCoord = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y + 1.5f);
                                break;
                            case ExitReason.Left:
                                // position around point to left
                                tmpCoord = GetInsertPos(Room[i].Loc.X - 1.5f, Room[i].Loc.Y);
                                break;
                            case ExitReason.Right:
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
                                case ExitReason.Horizon:
                                    // if target room is below this room, or on same level
                                    // (NOTE: this will also catch rooms that loop on themselves)
                                    if (Room[i].Loc.Y <= Room[targetRoom].Loc.Y) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, ExitReason.Bottom);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = tp;
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
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            // add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(LayoutSelection.TransPt, TPCount);
                                            ObjOrder.Add(obj);
                                            // adjust to account for size/shape of the transpt
                                            TransPt[TPCount].Loc[0] = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y - 1.5f, 0, 0.5f);
                                            TransPt[TPCount].Loc[1] = GetInsertPos(Room[targetRoom].Loc.X, Room[targetRoom].Loc.Y + 1.5f, 0, 0.5f);
                                            // increment transfer counter
                                            TPCount++;
                                        }
                                    }
                                    break;
                                case ExitReason.Right:
                                    // if target room is to left of this room, or on same level
                                    // (NOTE: this will also catch rooms that loop on themselves)
                                    if (Room[i].Loc.X >= Room[targetRoom].Loc.X) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, ExitReason.Left);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = tp;
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
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            // add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(LayoutSelection.TransPt, TPCount);
                                            ObjOrder.Add(obj);
                                            TransPt[TPCount].Loc[0] = GetInsertPos(Room[i].Loc.X + 1.5f, Room[i].Loc.Y, 0, 0.5f);
                                            TransPt[TPCount].Loc[1] = GetInsertPos(Room[targetRoom].Loc.X - 1.5f, Room[targetRoom].Loc.Y, 0, 0.5f);
                                            // increment transfer counter
                                            TPCount++;
                                        }
                                    }
                                    break;
                                case ExitReason.Bottom:
                                    // if target room is above this room, or on same level
                                    // (NOTE: this will also catch rooms that loop on themselves)
                                    if (Room[i].Loc.Y >= Room[targetRoom].Loc.Y) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, ExitReason.Horizon);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = tp;
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
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            // add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(LayoutSelection.TransPt, TPCount);
                                            ObjOrder.Add(obj);
                                            TransPt[TPCount].Loc[0] = GetInsertPos(Room[i].Loc.X, Room[i].Loc.Y + 1.5f, 0, 0.5f);
                                            TransPt[TPCount].Loc[1] = GetInsertPos(Room[targetRoom].Loc.X, Room[targetRoom].Loc.Y - 1.5f, 0, 0.5f);
                                            // increment transfer counter
                                            TPCount++;
                                        }
                                    }
                                    break;
                                case ExitReason.Left:
                                    // if target room is to right of this room, or on same level
                                    // (NOTE: this will also catch rooms that loop on themselves)
                                    if (Room[i].Loc.X <= Room[targetRoom].Loc.X) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, ExitReason.Right);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = tp;
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
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            // add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(LayoutSelection.TransPt, TPCount);
                                            ObjOrder.Add(obj);
                                            TransPt[TPCount].Loc[0] = GetInsertPos(Room[i].Loc.X - 1.5f, Room[i].Loc.Y, 0, 0.5f);
                                            TransPt[TPCount].Loc[1] = GetInsertPos(Room[targetRoom].Loc.X + 1.5f, Room[targetRoom].Loc.Y, 0, 0.5f);
                                            // increment transfer counter
                                            TPCount++;
                                        }
                                    }
                                    break;
                                case ExitReason.Other:
                                    // if more than 6 blocks away, AND if in another group OR
                                    // if exit loops back to this room
                                    if ((Math.Abs(Room[i].Loc.X - Room[targetRoom].Loc.X) + Math.Abs(Room[i].Loc.Y - Room[targetRoom].Loc.Y) > 6 &&
                                        ELRoom[i].Group != ELRoom[targetRoom].Group) || Room[i].Exits[j].Room == i) {
                                        // check for an existing set of transfer points between these two rooms
                                        int tp = GetTransfer((byte)i, (byte)targetRoom, ExitReason.Other);
                                        if (tp > 0) {
                                            // use this transfer
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = tp;
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
                                            Room[i].Exits[j].Type = ExitType.Transfer;
                                            Room[i].Exits[j].TypeIndex = TPCount;
                                            Room[i].Exits[j].Leg = 0;
                                            // add it to order
                                            TransPt[TPCount].Order = ObjOrder.Count;
                                            ObjInfo obj = new(LayoutSelection.TransPt, TPCount);
                                            ObjOrder.Add(obj);

                                            // position first around from room
                                            tmpCoord = GetInsertPos(Room[i].Loc, 0, 1);
                                            // if valid spot not found (coords didn't change)
                                            if (tmpCoord.X == Room[i].Loc.X && tmpCoord.Y == Room[i].Loc.Y) {
                                                // move it directly above
                                                tmpCoord.Y = Room[i].Loc.Y - 2;
                                            }
                                            TransPt[TPCount].Loc[0] = tmpCoord;
                                            // position second around target room
                                            tmpCoord = GetInsertPos(Room[targetRoom].Loc, 0, 1);
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

            // for rebuilding, there may be comments
            if (SavePos) {
                for (i = 1; i <= 255; i++) {
                    if (Comment[i].Visible) {
                        Comment[i].Order = ObjOrder.Count;
                        ObjInfo obj = new(LayoutSelection.Comment, i);
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

            // Unload progress form
            progress.Close();

            return true;

            float DeltaX(int Dir) {
                // only uses bits 1 and 2; rest of number is ignored
                return 2 * ((1 - (Dir & 2)) * (Dir & 1));
            }

            float DeltaY(int Dir) {
                // only uses bits 1 and 2; rest of number is ignored
                return 2 * ((1 - (Dir & 2)) * ((Dir & 1) - 1));
            }
        }

        private int GetTransfer(byte FromRoom, byte ToRoom, ExitReason Dir) {
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
                    if (Room[ToRoom].Exits[i].Type == ExitType.Transfer) {
                        int lngTrans = Room[ToRoom].Exits[i].TypeIndex;
                        if (TransPt[lngTrans].Count == 1) {
                            return lngTrans;
                        }
                    }
                }
            }
            // no match found
            return -1;
        }

        private void InsertTransfer(ref TSel NewSel) {
            // newsel will be an exit that does not currently have a transfer point

            // get next available number
            int tmpTrans;
            for (tmpTrans = 1; tmpTrans < TransPt.Length; tmpTrans++) {
                if (TransPt[tmpTrans].Count == 0) {
                    break;
                }
            }
            if (tmpTrans == TransPt.Length) {
                Array.Resize(ref TransPt, TransPt.Length + 16);
                return;
            }

            // add to order
            TransPt[tmpTrans].Order = ObjOrder.Count;
            ObjOrder.Add(new(LayoutSelection.TransPt, tmpTrans));

            // position along line at center
            AGIExit exit = Room[NewSel.Number].Exits[NewSel.ExitID];
            // dx and dy
            float DX = exit.EP.X - exit.SP.X;
            float DY = exit.EP.Y - exit.SP.Y;

            // center point of line
            float tmpX = exit.SP.X + DX / 2;
            float tmpY = exit.SP.Y + DY / 2;

            TransPt[tmpTrans].Loc[0].X = GridPos(tmpX - (DX != 0 ? Math.Sign(DX) : Math.Sign(DY)) * TRANSPT_SIZE / 2 - TRANSPT_SIZE / 2);
            TransPt[tmpTrans].Loc[0].Y = GridPos(tmpY - TRANSPT_SIZE / 2);
            TransPt[tmpTrans].Loc[1].X = GridPos(tmpX + (DX != 0 ? Math.Sign(DX) : Math.Sign(DY)) * TRANSPT_SIZE / 2 - TRANSPT_SIZE / 2);
            TransPt[tmpTrans].Loc[1].Y = GridPos(tmpY - TRANSPT_SIZE / 2);

            // if exit is bothways
            int tmpRoom = 0;
            string tmpID = "";
            if (IsTwoWay(NewSel.Number, NewSel.ExitID, ref tmpRoom, ref tmpID)) {
                // Count is two
                TransPt[tmpTrans].Count = 2;
                TransPt[tmpTrans].ExitID[1] = tmpID;
                Room[tmpRoom].Exits[tmpID].Type = ExitType.Transfer;
                Room[tmpRoom].Exits[tmpID].TypeIndex = tmpTrans;
                Room[tmpRoom].Exits[tmpID].Leg = 1;
                SetExitPos(tmpRoom, tmpID);
            }
            else {
                // Count is one
                TransPt[tmpTrans].Count = 1;
            }

            // add from/to room info
            TransPt[tmpTrans].Room[0] = (byte)NewSel.Number;
            TransPt[tmpTrans].Room[1] = (byte)Room[NewSel.Number].Exits[NewSel.ExitID].Room;
            TransPt[tmpTrans].ExitID[0] = NewSel.ExitID;

            // set trans property of exit
            Room[NewSel.Number].Exits[NewSel.ExitID].Type = ExitType.Transfer;
            Room[NewSel.Number].Exits[NewSel.ExitID].TypeIndex = tmpTrans;
            Room[NewSel.Number].Exits[NewSel.ExitID].Leg = 0;

            // reposition exit lines
            SetExitPos(NewSel.Number, NewSel.ExitID);

            MarkAsChanged();
        }

        public void UpdateLayout(UpdateReason Reason, int LogicNumber, AGIExits NewExits, int NewNum = 0) {
            // updates a room that was modified outside of the layout editor
            // so the layout editor room info matches the external room info
            // 
            // rooms can be added to the game, removed from the game, hidden, shown, or edited
            // to force this update
            // 
            // to keep status of transpts accurate, the list of current exits
            // is checked for any transpts; if found, the new list is checked
            // to see if an existing exit exactly matches; if so, the new exit
            // is assigned to the transpt; if no match is found, the transpt
            // is no longer in use; its Count is decremented (and the transpt
            // is deleted if Count goes to zero)

            // if there is a selection
            if (Selection.Type != LayoutSelection.None) {
                DeselectObj();
            }

            switch (Reason) {
            case UpdateReason.ChangeID:
                // TODO: changing an id should just redraw the layout
                DrawLayout(LayoutSelection.Room, LogicNumber);
                break;
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
                int idx = ObjOrder.FindIndex(obj => obj.Type == LayoutSelection.Room && obj.Number == LogicNumber);
                ObjOrder[idx] = new(LayoutSelection.Room, NewNum);
                break;
            case UpdateReason.UpdateRoom:
            case UpdateReason.ShowRoom:
                // updateroom will always be to a visible room
                // showroom will always be not visible until added-
                // regardless of reason, if room is not visible, show it
                if (!Room[LogicNumber].Visible) {
                    // no need to get exits; they are already passed as NewExits
                    DisplayRoom(LogicNumber, Reason == UpdateReason.ShowRoom, true);
                }
                // if updating an existing room that has exits already set,
                // need to determine if any of them have exact matches
                // with transfer points
                for (int i = 0; i < Room[LogicNumber].Exits.Count; i++) {
                    switch (Room[LogicNumber].Exits[i].Type) {
                    case ExitType.Transfer:
                        // does the update have a matching exit?
                        for (int j = 0; j < NewExits.Count; j++) {
                            if (Room[LogicNumber].Exits[i].Reason == NewExits[j].Reason &&
                                Room[LogicNumber].Exits[i].Room == NewExits[j].Room &&
                                Room[LogicNumber].Exits[i].ID == NewExits[j].ID) {
                                // use it in new exit
                                NewExits[j].Type = ExitType.Transfer;
                                NewExits[j].TypeIndex = Room[LogicNumber].Exits[i].TypeIndex;
                                NewExits[j].Leg = Room[LogicNumber].Exits[i].Leg;
                                // clear it from old exits
                                Room[LogicNumber].Exits[i].Type = ExitType.Normal;
                                Room[LogicNumber].Exits[i].TypeIndex = 0;
                                break;
                            }
                        }
                        break;
                    case ExitType.Error:
                        break;
                    }
                }

                // run through existing exits again, and delete them
                // DeleteExit method uses a selection object, and handles transfers
                // set the selection object properties to
                // match a single direction exit from the room being updated
                TSel tmpSel = new();
                tmpSel.Type = LayoutSelection.Exit;
                tmpSel.Number = LogicNumber;
                tmpSel.TwoWay = ExitDirection.OneWay;

                // step through all current exits and delete them
                for (int i = Room[LogicNumber].Exits.Count - 1; i >= 0; i--) {
                    // set id so correct exit is deleted
                    tmpSel.ExitID = Room[LogicNumber].Exits[i].ID;
                    // and delete it
                    DeleteExit(tmpSel);
                }
                // (the delete exit function doesn't always actually delete;
                // in some cases, it just marks them as 'deletable'
                // so we need to use the Clear method to make them actually go away
                Room[LogicNumber].Exits.Clear();
                //  add new exits
                for (int i = 0; i < NewExits.Count; i++) {
                    bool AddErrPt = false;
                    // check for err point first (if exit room=0, Room[NewExits[i].Room].Visible
                    // will always be false so this line captures error points regardless if it
                    // is because room=0 or room is not visible)
                    if (!Room[NewExits[i].Room].Visible) {
                        // show err pt if room is not valid (room=0, or logic doesn't exist)
                        if (NewExits[i].Room == 0 || !EditGame.Logics.Contains(NewExits[i].Room)) {
                            // if err pt not yet added,
                            if (NewExits[i].Type == ExitType.Normal) {
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
                        if (NewExits[i].Type == ExitType.Normal) {
                            // check for an existing transfer from the target room
                            ExitReason Dir;
                            switch (NewExits[i].Reason) {
                            case ExitReason.Horizon:
                                Dir = ExitReason.Bottom;
                                break;
                            case ExitReason.Right:
                                Dir = ExitReason.Left;
                                break;
                            case ExitReason.Bottom:
                                Dir = ExitReason.Horizon;
                                break;
                            case ExitReason.Left:
                                Dir = ExitReason.Right;
                                break;
                            case ExitReason.Other:
                                Dir = ExitReason.Other;
                                break;
                            default:
                                // should never occur, but just in case
                                Dir = ExitReason.None;
                                break;
                            }
                            // if NOT err or unknown AND target room is not 0
                            if (Dir != ExitReason.None) {
                                // if this exit is reciprocal:
                                // CAN'T use IsTwoWay function because exit is not added yet
                                for (int j = 0; j < Room[NewExits[i].Room].Exits.Count; j++) {
                                    // if this exit goes back to original room AND is not deleted
                                    if (Room[NewExits[i].Room].Exits[j].Room == LogicNumber &&
                                        Room[NewExits[i].Room].Exits[j].Status != ExitStatus.Deleted) {
                                        // if reason and transfer match
                                        if (Room[NewExits[i].Room].Exits[j].Reason == Dir) {
                                            // if transfer exists, and is not already in use two ways
                                            if (Room[NewExits[i].Room].Exits[j].Type == ExitType.Transfer) {
                                                if (TransPt[Room[NewExits[i].Room].Exits[j].TypeIndex].Count != 2) {
                                                    // use this transfer
                                                    NewExits[i].Type = ExitType.Transfer;
                                                    NewExits[i].TypeIndex = Room[NewExits[i].Room].Exits[j].TypeIndex;
                                                    NewExits[i].Leg = 1;
                                                    TransPt[NewExits[i].TypeIndex].Count = 2;
                                                    TransPt[NewExits[i].TypeIndex].ExitID[1] = NewExits[i].ID;
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
                    Room[LogicNumber].Exits.Add(NewExits[i]);
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
            RecalculateMaxMin();
            SetScrollBars();
            MarkAsChanged();
        }

        public void UpdatePictureStatus(int LogicNumber, bool ShowPic) {
            // updates the picture status of a room

            if (ShowPic) {
                if (Room[LogicNumber].Visible && WinAGISettings.LEShowPics.Value) {
                    Room[LogicNumber].ShowPic = true;
                    // if room is on screen, redraw it
                    DrawLayout(LayoutSelection.Room, LogicNumber);
                }
            }
            else {
                if (Room[LogicNumber].Visible && Room[LogicNumber].ShowPic) {
                    Room[LogicNumber].ShowPic = false;
                    // if room is on screen, redraw it
                    DrawLayout(LayoutSelection.Room, LogicNumber);
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
            // updates the text for this logic to match current exit status
            // 
            // called by save layout; at least one exit needs updating or
            // this method will not have been called

            // ensure source is loaded
            bool blnLogLoad = ThisLogic.Loaded;
            if (!blnLogLoad) {
                ThisLogic.Load();
            }

            // run through update algorithm twice; first to update
            // saved logic file, then check for open logic editor;
            // and update that as well

            int Index = GetEditor(ThisLogic);
            int newrmpos, idpos, commentpos;
            int linestart, lineend;
            string strSource, strCR;
            bool blnExitOK;

            for(int loop = 0; loop < 2; loop++) {
                int insertpos = -1;
                // if first time through
                if (loop == 0) {
                    // get source from saved logic
                    strSource = ThisLogic.SourceText;
                    strCR = Environment.NewLine; // "\r\n";
                }
                else {
                    // second time through
                    if (Index == -1) {
                        // not found; dont need to update an editor
                        break;
                    }
                    // get source from the rtf box
                    strSource = LogicEditors[Index].fctb.Text;
                    // use cr only for cr/line feed (this is a feature(?) of rtf textboxes)
                    strCR = Environment.NewLine;
                }

                // step through exits
                // go backwards so deletions don't screw up the for-next block
                for (int i = Room[ThisLogic.Number].Exits.Count - 1; i >= 0; i--) {
                    // reset ok flag
                    blnExitOK = false;
                    bool addAsNew = false;

                    // check for OK or Changed exits first;
                    // if unable to update, change them to 'new'
                    // and add a new exit instead

                    if (Room[ThisLogic.Number].Exits[i].Status == ExitStatus.OK) {
                        // ok; need to verify that the exitID has not changed
                        idpos = strSource.IndexOf("##" + Room[ThisLogic.Number].Exits[i].ID + "##");
                        // if found
                        if (idpos != -1) {
                            // get the line start and end (adjust for width of cr-lf)
                            linestart = strSource.LastIndexOf(strCR, idpos);
                            if (linestart == -1) {
                                linestart = 0;
                            }
                            else {
                                linestart += strCR.Length;
                            }
                            lineend = strSource.IndexOf(strCR, idpos);
                            if (lineend == -1) lineend = strSource.Length;
                            // line should be in form of
                            // new.room(xxx); [ ##exitID##
                            // find new.room cmd,
                            newrmpos = FindTokenPosRev(strSource, "new.room", idpos);
                            // if found,
                            if (newrmpos != -1) {
                                // verify on same line
                                if (linestart < newrmpos) {
                                    // get the exit info for this exit
                                    commentpos = newrmpos;
                                    AGIExit tmpExit = AnalyzeExit(strSource, ref commentpos);

                                    // if reason and room match
                                    if (tmpExit.Reason == Room[ThisLogic.Number].Exits[i].Reason &&
                                        tmpExit.Room == Room[ThisLogic.Number].Exits[i].Room) {
                                        // exit is ok
                                        blnExitOK = true;
                                        // make sure exit style match the logic
                                        Room[ThisLogic.Number].Exits[i].Style = tmpExit.Style;
                                    }
                                }
                            }
                        }
                        if (!blnExitOK) {
                            // something was wrong with this exit; ignore
                            // the exit with the error, and add this exit
                            // as a new exit
                            addAsNew = true;
                        }
                    }
                    if (Room[ThisLogic.Number].Exits[i].Status == ExitStatus.Changed) {
                        // find exit in text, and change it
                        idpos = strSource.IndexOf("##" + Room[ThisLogic.Number].Exits[i].ID + "##");
                        if (idpos != -1) {
                            // get the line start and end
                            linestart = strSource.LastIndexOf(strCR, idpos);
                            if (linestart == -1) {
                                linestart = 0;
                            }
                            else {
                                linestart += strCR.Length;
                            }
                            lineend = strSource.IndexOf(strCR, idpos);
                            if (lineend == -1) lineend = strSource.Length;
                            // line should be in form of
                            // new.room(xxx); [ ##exitID##
                            // find new.room cmd,
                            newrmpos = FindTokenPosRev(strSource, "new.room", idpos);
                            // if found,
                            if (newrmpos != -1) {
                                // verify on same line
                                if (linestart < newrmpos) {
                                    // get the exit info for this exit
                                    commentpos = newrmpos;
                                    AGIExit tmpExit = AnalyzeExit(strSource, ref commentpos);

                                    // if reason matches
                                    if (tmpExit.Reason == Room[ThisLogic.Number].Exits[i].Reason) {
                                        // change room in logic to match the exit

                                        // adjust to opening parenthesis after 'new.room'
                                        newrmpos += 8;

                                        // find closing parenthesis
                                        idpos = FindTokenPos(strSource, ")", newrmpos);

                                        // if found,
                                        if (idpos != -1) {
                                            // insert new room here
                                            if (EditGame.IncludeIDs) {
                                                strSource = strSource.Left(newrmpos + 1) + EditGame.Logics[Room[ThisLogic.Number].Exits[i].Room].ID + strSource.Right(strSource.Length - idpos + 1);
                                            }
                                            else {
                                                strSource = strSource.Left(newrmpos + 1) + Room[ThisLogic.Number].Exits[i].Room + strSource.Right(strSource.Length - idpos + 1);
                                            }
                                            // reset exit style to match the logic
                                            Room[ThisLogic.Number].Exits[i].Style = tmpExit.Style;
                                            // change is ok
                                            blnExitOK = true;
                                        }
                                    }
                                }
                            }
                        }
                        // if changed successfully
                        if (blnExitOK) {
                            // if second time through, OR no logic window open
                            if (loop == 1 || Index == -1) {
                                // reset exit status to ok since it now editor, file and logic source are all insync
                                Room[ThisLogic.Number].Exits[i].Status = ExitStatus.OK;
                            }
                        }
                        else {
                            // something was wrong with this exit; ignore
                            // the exit with the error, and add this exit
                            // as a new exit
                            addAsNew = true;
                        }
                    }
                    if (addAsNew || Room[ThisLogic.Number].Exits[i].Status == ExitStatus.NewExit) {
                        if (insertpos == -1) {
                            // find last return cmd
                            insertpos = FindTokenPosRev(strSource, "return");
                            // if not found
                            if (insertpos == -1) {
                                // add to end
                                strSource += strCR;
                                insertpos = strSource.Length;
                            }
                            else {
                                // now move to beginning of line
                                insertpos = strSource.LastIndexOf(strCR, insertpos);
                                if (insertpos == -1) {
                                    // add to beginning
                                    insertpos = 0;
                                }
                                else {
                                    insertpos += strCR.Length;
                                }
                            }
                        }
                        if (insertpos == 0) {
                            // add to beginning
                            strSource = NewExitText(Room[ThisLogic.Number].Exits[i], strCR) + strSource;
                        }
                        else {
                            // add at insertpos
                            strSource = strSource.Left(insertpos) + NewExitText(Room[ThisLogic.Number].Exits[i]) + strSource.Right(strSource.Length - insertpos);
                        }


                        // if second time through, OR no logic window open
                        if (loop == 1 || Index == -1) {
                            // reset exit status to ok since it now editor, file and logic source are all insync
                            Room[ThisLogic.Number].Exits[i].Status = ExitStatus.OK;
                        }
                    }
                    if (Room[ThisLogic.Number].Exits[i].Status == ExitStatus.Deleted) {
                        // find exit in source and delete/comment it out;
                        idpos = strSource.IndexOf("##" + Room[ThisLogic.Number].Exits[i].ID + "##");
                        // if found
                        if (idpos != -1) {
                            // find beginning of line (adjust for width of cr-lf)
                            linestart = strSource.LastIndexOf(strCR, idpos);
                            if (linestart == -1) {
                                linestart = 0;
                            }
                            else {
                                linestart += strCR.Length;
                            }
                            lineend = strSource.IndexOf(strCR, idpos);
                            if (lineend == -1) lineend = strSource.Length;
                            // line should be in form of
                            // new.room(xxx); [ ##exitID##
                            // insert new comment in front of line
                            string strNewText = "[ DELETED BY LAYOUT EDITOR " + strCR + "[ ";
                            commentpos = strSource.IndexOf('[', linestart);
                            if (commentpos == -1 || commentpos > idpos) {
                                // try "\\"
                                commentpos = strSource.IndexOf("\\\\", linestart);
                                if (commentpos == -1 || commentpos > idpos) {
                                    // no comment marker - just use idpos
                                    commentpos = idpos;
                                    
                                }
                            }
                            // insert comment in front of line, and remove the tag
                            strSource = strSource.Left(linestart) + strNewText +
                                strSource[linestart..commentpos] +
                                strSource.Right(strSource.Length - lineend);

                            //}
                            //else {
                                // not found;

                                // if updating the source file, this should never happen,
                                // unless the file was modified outside of WinAGI GDS or something corrupted
                                // the layout or the source file; in either case, ignore this problem
                                // for updates to the sourcefile

                                // if updating an open logic editor, the most likely cause is that the
                                // user manually edited the logic source, and probably deleted this exit
                                // since it is already gone, no action is necessary
                            //}
                        }

                        // if second time through, OR no logic window open
                        if (loop == 1 || Index == -1) {
                            // remove exit, since editor, file and logic source are now insync
                            Room[ThisLogic.Number].Exits.Remove(i);
                        }
                    }
                }

                // if currently updating the file
                if (loop == 0) {
                    // save source
                    ThisLogic.SourceText = strSource;
                    ThisLogic.SaveSource();
                    // update resource treelist and property window
                    RefreshTree(AGIResType.Logic, ThisLogic.Number);
                }
                else {
                    // update editor
                    Place start = LogicEditors[Index].fctb.Selection.Start;
                    Place end = LogicEditors[Index].fctb.Selection.End;
                    FastColoredTextBoxNS.Range vr = LogicEditors[Index].fctb.VisibleRange;
                    LogicEditors[Index].fctb.Text = strSource;
                    LogicEditors[Index].fctb.Selection.Start = start;
                    LogicEditors[Index].fctb.Selection.End = end;
                    LogicEditors[Index].fctb.DoRangeVisible(vr);
                }
            }

            // unload if necessary
            if (!blnLogLoad && ThisLogic.Loaded) {
                ThisLogic.Unload();
            }

            static int GetEditor(Logic ThisLogic) {
                // check for open logic
                for (int i = 0; i < LogicEditors.Count; i++) {
                    if (LogicEditors[i].FormMode == LogicFormMode.Logic) {
                        if (LogicEditors[i].LogicNumber == ThisLogic.Number) {
                            // found it
                            return i;
                        }
                    }
                }
                // not found
                return -1;
            }
        }

        public void DrawLayout(LayoutSelection objType, int objNumber) {
            // draws the layout if the specified object is on screen
            if (ObjOnScreen(objType, objNumber)) {
                DrawLayout();
            }
        }

        public void DrawLayout() {
            // in some cases when disposing the form, the resize event
            // will try to draw the layout- we need to check for that
            // also, if form is not yet visible, no need to draw anything
            if (! Visible || Disposing) {
                return;
            }

            float x1, y1;
            float h, w;
            float tX, tY;
            float linewidth;

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
            if (DrawScale < 3) {
                linewidth = 1;
            }
            else if (DrawScale < 5) {
                linewidth = 2;
            }
            else {
                linewidth = 3;
            }
            using Pen objPen = new(Color.White, linewidth);
            // next add objects
            for (int i = 0; i < ObjOrder.Count; i++) {
                switch (ObjOrder[i].Type) {
                case LayoutSelection.Room:
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
                                // currently, non-rooms are never displayed; this 
                                // color set is not used- I may add it later...
                                objBrush.Color = Color.LightGray;
                                textBrush.Color = objPen.Color = Color.Gray;
                                objPen.DashStyle = DashStyle.Dash;
                            }
                            x1 = LayoutToScreenX(Room[lngRoom].Loc.X);// (float)(Room[lngRoom].Loc.X * DSF + Offset.X);
                            y1 = LayoutToScreenY(Room[lngRoom].Loc.Y);// (float)(Room[lngRoom].Loc.Y * DSF + Offset.Y);
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
                    }
                    break;
                case LayoutSelection.TransPt:
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
                case LayoutSelection.ErrPt:
                    if (ErrPt[ObjOrder[i].Number].Visible) {
                        if (ObjOnScreen(ObjOrder[i])) {
                            objBrush.Color = WinAGISettings.ErrPtFillColor.Value;
                            textBrush.Color = objPen.Color = WinAGISettings.ErrPtEdgeColor.Value;
                            objPen.DashStyle = DashStyle.Solid;
                            PointF[] errPoints =
                            [
                                new PointF(ErrPt[ObjOrder[i].Number].Loc.X * DSF + Offset.X,
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
                case LayoutSelection.Comment:
                    if (Comment[ObjOrder[i].Number].Visible) {
                        if (ObjOnScreen(ObjOrder[i])) {
                            // draw comment box
                            DrawCmtBox(g, ObjOrder[i].Number);
                        }
                    }
                    break;
                }
            }

            // draw exits on top of everything else
            AdjustableArrowCap arrowcap;
            // | Pen Width | Cap Size Multiplier | Actual Cap Size      |
            // |-----------|-------------------- |-----------------     |
            // | < 2       | 2                   | 2 × cap size         |
            // | ≥ 2       | pen width           | pen width × cap size |
            if (DrawScale < 3) {
                arrowcap = new AdjustableArrowCap(2f * DSF / 40, 4f * DSF / 40, true);
            }
            else if (DrawScale < 5) {
                arrowcap = new AdjustableArrowCap(2f * DSF / 40, 4f * DSF / 40, true);
            }
            else {
                arrowcap = new AdjustableArrowCap(2f * DSF / 60, 4f * DSF / 60, true);
            }
            for (int i = 1; i < 256; i++) {
                if (Room[i].Visible) {
                    for (int j = 0; j < Room[i].Exits.Count; j++) {
                        // skip deleted exits
                        if (Room[i].Exits[j].Status != ExitStatus.Deleted) {
                            // determine color
                            if (Room[i].Exits[j].Hidden) {
                                // use a dashed, gray line
                                objPen.Color = Color.FromArgb(160, 160, 160);
                                objPen.DashStyle = DashStyle.Dash;
                            }
                            else {
                                objPen.DashStyle = DashStyle.Solid;
                                switch (Room[i].Exits[j].Reason) {
                                case ExitReason.Other:
                                    objPen.Color = WinAGISettings.ExitOtherColor.Value;
                                    break;
                                default:
                                    objPen.Color = WinAGISettings.ExitEdgeColor.Value;
                                    break;
                                }
                            }
                            // if there is a transfer pt
                            int transpt = Room[i].Exits[j].TypeIndex;
                            switch (Room[i].Exits[j].Type) {
                            case ExitType.Transfer:
                                switch (TransPt[transpt].Count) {
                                case 1:
                                    // is this first leg?
                                    if (Room[i].Exits[j].Leg == 0) {
                                        // arrow on end
                                        objPen.StartCap = LineCap.Flat;
                                        objPen.CustomEndCap = arrowcap;
                                        objPen.CustomEndCap.WidthScale = 1;
                                    }
                                    else {
                                        // arrow on start
                                        objPen.EndCap = LineCap.Flat;
                                        objPen.CustomStartCap = arrowcap;
                                    }
                                    break;
                                case 2:
                                    // only draw first leg
                                    if (Room[i].Exits[j].Leg == 1) {
                                        continue;
                                    }
                                    // arrow on both ends
                                    objPen.CustomStartCap = objPen.CustomEndCap = arrowcap;
                                    break;
                                }
                                // draw first segment
                                if (LineOnScreen(Room[i].Exits[j].SP,
                                        TransPt[transpt].SP)) {
                                    g.DrawLine(objPen,
                                        Room[i].Exits[j].SP.X * DSF + Offset.X,
                                        Room[i].Exits[j].SP.Y * DSF + Offset.Y,
                                        TransPt[transpt].SP.X * DSF + Offset.X,
                                        TransPt[transpt].SP.Y * DSF + Offset.Y);
                                }
                                // draw second segment
                                if (LineOnScreen(TransPt[transpt].EP,
                                        Room[i].Exits[j].EP)) {
                                    g.DrawLine(objPen,
                                        TransPt[transpt].EP.X * DSF + Offset.X,
                                        TransPt[transpt].EP.Y * DSF + Offset.Y,
                                        Room[i].Exits[j].EP.X * DSF + Offset.X,
                                        Room[i].Exits[j].EP.Y * DSF + Offset.Y);
                                }
                                break;
                            case ExitType.Normal:
                            case ExitType.Error:
                                // normal lines and err pts use endcap only
                                objPen.StartCap = LineCap.Flat;
                                objPen.CustomEndCap = arrowcap;
                                if (!Room[i].Exits[j].Hidden ||
                                    WinAGISettings.LEShowHidden.Value) {
                                    // draw the exit line
                                    if (LineOnScreen(Room[i].Exits[j].SP,
                                            Room[i].Exits[j].EP)) {
                                        g.DrawLine(objPen,
                                            Room[i].Exits[j].SP.X * DSF + Offset.X,
                                            Room[i].Exits[j].SP.Y * DSF + Offset.Y,
                                            Room[i].Exits[j].EP.X * DSF + Offset.X,
                                            Room[i].Exits[j].EP.Y * DSF + Offset.Y);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
            picDraw.Refresh();
            g.Dispose();
        }

        private void DrawCmtBox(Graphics g, int CmtID) {
            // draws a rounded corner rectangle; converts layout coordinates into drawing surface pixel coordinates

            using Pen borderPen = new Pen(WinAGISettings.CmtEdgeColor.Value, 2);
            float radius = 5.0f * DSF / 40; // radius for rounded corners
            RectangleF rect = new(
            Comment[CmtID].Loc.X * DSF + Offset.X,
            Comment[CmtID].Loc.Y * DSF + Offset.Y,
            Comment[CmtID].Size.Width * DSF,
            Comment[CmtID].Size.Height * DSF);

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

            // wrap text to fit (copy from textbox to be sure lines break at right place)
            FormatCommentTextBox(CmtID);
            List<string> wrappedLines = GetWrappedLines(txtComment);
            // size of one line
            float textHeight = layoutFont.GetHeight(g);
            float tX = (Comment[CmtID].Loc.X + 0.06f) * DSF + Offset.X;
            float tY = (Comment[CmtID].Loc.Y + 0.04f) * DSF + Offset.Y;
            using SolidBrush fontBrush = new SolidBrush(WinAGISettings.CmtEdgeColor.Value);
            // now copy lines onto drawing surface
            foreach (string line in wrappedLines) {
                // if not enough room vertically (meaning text would extend below
                // bottom edge of comment box)
                if (tY > (Comment[CmtID].Loc.Y + Comment[CmtID].Size.Height) * DSF + Offset.Y - 4) {
                    break;
                }
                g.DrawString(line, layoutFont, fontBrush, tX, tY);
                tY += textHeight;
            }
        }

        private void FormatCommentTextBox(int commentid) {
            txtComment.BackColor = WinAGISettings.CmtFillColor.Value;
            txtComment.ForeColor = WinAGISettings.CmtEdgeColor.Value;
            txtComment.Text = Comment[commentid].Text;
            txtComment.Width = (int)
                Math.Round((Comment[commentid].Size.Width - 0.08f) * DSF, 0);
            txtComment.Height = (int)
                Math.Round((Comment[commentid].Size.Height - 0.08f) * DSF, 0);
        }

        private void BeginEditComment() {
            FormatCommentTextBox(Selection.Number);
            txtComment.Left = (int)
                Math.Round((Comment[Selection.Number].Loc.X + 0.03f) * DSF + Offset.X, 0) + 1;
            txtComment.Top = (int)
                Math.Round((Comment[Selection.Number].Loc.Y + 0.04f) * DSF + Offset.Y, 0) + 1;
            // move selection to beginning
            txtComment.Text = Comment[Selection.Number].Text;
            txtComment.SelectionStart = 0;
            txtComment.SelectionLength = 0;
            txtComment.Visible = true;
            txtComment.Tag = Selection.Number;
            EditCommentText = true;
        }

        private void BeginMoveObj(Point anchor) {
            MoveObj = true;
            SetCursor(LayoutCursor.Moving);
            AnchorPt = ScreenToLayout(anchor);
            Delta = anchor;
            // get delta from anchor point to object location
            PointF tmp = new();
            switch (Selection.Type) {
            case LayoutSelection.Room:
                tmp = LayoutToScreen(Room[Selection.Number].Loc);
                break;
            case LayoutSelection.TransPt:
                tmp = LayoutToScreen(TransPt[Selection.Number].Loc[(int)Selection.Leg]);
                break;
            case LayoutSelection.ErrPt:
                tmp = LayoutToScreen(ErrPt[Selection.Number].Loc);
                break;
            case LayoutSelection.Comment:
                tmp = LayoutToScreen(Comment[Selection.Number].Loc);
                break;
            case LayoutSelection.Multiple:
                tmp = LayoutToScreen(Selection.Loc);
                break;
            }
            Delta.Offset((int)-tmp.X, (int)-tmp.Y);
        }

        private bool AreaOnScreen(PointF point, SizeF size) {
            // returns true if the area defined by point and size
            // is within the screen bounds of the drawing surface
            // convert to drawing surface coordinates and account for
            // handle offsets
            RectangleF selRect = new(point.X * DSF + Offset.X - HandleSize,
                point.Y * DSF + Offset.Y - HandleSize,
                size.Width * DSF + 2 * HandleSize,
                size.Height * DSF + 2 * HandleSize);
            return selRect.IntersectsWith(picDraw.ClientRectangle);
        }

        /// <summary>
        /// Returns true if any part of the object is within the draw surface client
        /// area. Use SecondLeg=true to check for second leg of transfer point.
        /// </summary>
        /// <param name="ObjTest"></param>
        /// <param name="SecondLeg"></param>
        /// <returns></returns>
        private bool ObjOnScreen(ObjInfo ObjTest, bool SecondLeg = false) {

            // returns true if any portion of the object is on the screen
            return ObjOnScreen(ObjTest.Type, ObjTest.Number, SecondLeg);
        }

        /// <summary>
        /// Returns true if any part of the object is within the draw surface client
        /// area.
        /// </summary>
        /// <param name="objtype"></param>
        /// <param name="objnum"></param>
        /// <param name="SecondLeg"></param>
        /// <returns></returns>
        private bool ObjOnScreen(LayoutSelection objtype, int objnum, bool SecondLeg = false) {
            RectangleF objRect = new();

            switch (objtype) {
            case LayoutSelection.Room:
                objRect = new(new(Room[objnum].Loc.X * DSF + Offset.X,
                    Room[objnum].Loc.Y * DSF + Offset.Y),
                    new(RM_SIZE * DSF, RM_SIZE * DSF));
                break;
            case LayoutSelection.TransPt:
                if (SecondLeg) {
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
            case LayoutSelection.ErrPt:
                objRect = new(new(ErrPt[objnum].Loc.X * DSF + Offset.X,
                    (ErrPt[objnum].Loc.Y) * DSF + Offset.Y),
                    new(RM_SIZE * 0.6f * DSF, RM_SIZE * 0.5196f * DSF));
                break;
            case LayoutSelection.Comment:
                objRect = new(new((Comment[objnum].Loc.X) * DSF + Offset.X,
                    (Comment[objnum].Loc.Y) * DSF + Offset.Y),
                    new(Comment[objnum].Size.Width * DSF, Comment[objnum].Size.Height * DSF));
                break;
            }
            return objRect.IntersectsWith(picDraw.ClientRectangle);
        }

        /// <summary>
        /// Returns true if any part of the object is within the draw surface client
        /// area.
        /// </summary>
        /// <param name="objtype"></param>
        /// <param name="objnum"></param>
        /// <param name="SecondTrans"></param>
        /// <returns></returns>
        private bool LineOnScreen(PointF start, PointF end) {
            // determine if any points on the line are located on screen

            // convert line coordinates into screen coordinates
            PointF p1 = new(start.X * DSF + Offset.X, start.Y * DSF + Offset.Y);
            PointF p2 = new(end.X * DSF + Offset.X, end.Y * DSF + Offset.Y);
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

        private void DrawSelectionHandles(Graphics g, TSel selection) {
            PointF loc = new();
            SizeF size = new();

            switch (selection.Type) {
            case LayoutSelection.Room:
                loc = Room[selection.Number].Loc;
                size = new SizeF(RM_SIZE, RM_SIZE);
                break;
            case LayoutSelection.TransPt:
                loc = TransPt[selection.Number].Loc[(int)selection.Leg];
                size = new SizeF(TRANSPT_SIZE, TRANSPT_SIZE);
                break;
            case LayoutSelection.Comment:
                loc = Comment[selection.Number].Loc;
                size = Comment[selection.Number].Size;
                break;
            case LayoutSelection.ErrPt:
                loc = ErrPt[selection.Number].Loc;
                // adjust to account for rounded corners
                loc.X += 0.04f;
                size = new SizeF(0.52f, 0.475f);
                break;
            case LayoutSelection.Exit:
                // Draw exit handles
                PointF sp = new(), ep = new(), ap = new();
                switch (Selection.Leg) {
                case ExitLeg.SecondLeg:
                    // transfer; select second leg
                    // if this is the first exit for this transfer point
                    if (Room[Selection.Number].Exits[Selection.ExitID].Leg == 0) {
                        // ep of trans pt matches ep of exit
                        sp = TransPt[Room[Selection.Number].Exits[Selection.ExitID].TypeIndex].EP;
                    }
                    else {
                        // sp of trans pt matches ep of exit
                        sp = TransPt[Room[Selection.Number].Exits[Selection.ExitID].TypeIndex].SP;
                    }
                    ep = Room[Selection.Number].Exits[Selection.ExitID].EP;
                    break;
                case ExitLeg.FirstLeg:
                    // transfer; select first leg
                    sp = Room[Selection.Number].Exits[Selection.ExitID].SP;
                    // if this is first exit for this transfer point
                    if (Room[Selection.Number].Exits[Selection.ExitID].Leg == 0) {
                        // sp of transpt matches sp of exit
                        ep = TransPt[Room[Selection.Number].Exits[Selection.ExitID].TypeIndex].SP;
                    }
                    else {
                        // ep of transpt matches sp of exit
                        ep = TransPt[Room[Selection.Number].Exits[Selection.ExitID].TypeIndex].EP;
                    }
                    break;
                case ExitLeg.NoTransPt:
                    // no transfer; select whole line
                    sp = Room[Selection.Number].Exits[Selection.ExitID].SP;
                    ep = Room[Selection.Number].Exits[Selection.ExitID].EP;
                    break;
                }
                // if one direction, AND exit is two way
                if (Selection.TwoWay == ExitDirection.OneWay) {
                    // add third handle by arrowhead
                    ap.X = (float)(ep.X - RM_SIZE / 4 /
                        Math.Sqrt(Math.Pow(ep.X - sp.X, 2) +
                        Math.Pow(ep.Y - sp.Y, 2)) * (ep.X - sp.X));
                    ap.Y = (float)(ep.Y - RM_SIZE / 4 /
                        Math.Sqrt(Math.Pow(ep.X - sp.X, 2) +
                        Math.Pow(ep.Y - sp.Y, 2)) * (ep.Y - sp.Y));
                    if (LineOnScreen(ap, ap)) {
                        g.FillRectangle(Brushes.Red, new RectangleF(ap.X * DSF + Offset.X - HandleSize / 2, ap.Y * DSF + Offset.Y - HandleSize / 2, HandleSize, HandleSize));
                    }
                }
                if (LineOnScreen(sp, sp)) {
                    g.FillRectangle(Brushes.Black, new RectangleF(sp.X * DSF + Offset.X - HandleSize / 2, sp.Y * DSF + Offset.Y - HandleSize / 2, HandleSize, HandleSize));
                }
                if (LineOnScreen(ep, ep)) {
                    g.FillRectangle(Brushes.Black, new RectangleF(ep.X * DSF + Offset.X - HandleSize / 2, ep.Y * DSF + Offset.Y - HandleSize / 2, HandleSize, HandleSize));
                }
                return;
            case LayoutSelection.Multiple:
                for (int i = 0; i < Selection.Number; i++) {
                    switch (SelectedObjects[i].Type) {
                    case LayoutSelection.Room:
                        loc = Room[SelectedObjects[i].Number].Loc;
                        size = new SizeF(RM_SIZE, RM_SIZE);
                        break;
                    case LayoutSelection.TransPt:
                        if (SelectedObjects[i].Leg == ExitLeg.FirstLeg) {
                            loc = TransPt[SelectedObjects[i].Number].Loc[0];
                        }
                        else {
                            loc = TransPt[SelectedObjects[i].Number].Loc[1];
                        }
                        size = new SizeF(RM_SIZE / 2, RM_SIZE / 2);
                        break;
                    case LayoutSelection.Comment:
                        loc = Comment[SelectedObjects[i].Number].Loc;
                        size = Comment[SelectedObjects[i].Number].Size;
                        break;
                    case LayoutSelection.ErrPt:
                        loc = ErrPt[SelectedObjects[i].Number].Loc;
                        loc.X += 0.04f;
                        size = new SizeF(0.52f, 0.475f);
                        break;
                    }
                    // now draw handles
                    g.FillRectangles(Brushes.Black, new RectangleF[] {
                        new(loc.X * DSF + Offset.X - HandleSize, loc.Y * DSF + Offset.Y - HandleSize, HandleSize, HandleSize),
                        new((loc.X + size.Width) * DSF + Offset.X, loc.Y * DSF + Offset.Y - HandleSize, HandleSize, HandleSize),
                        new(loc.X * DSF + Offset.X - HandleSize, (loc.Y + size.Height) * DSF + Offset.Y, HandleSize, HandleSize),
                        new((loc.X + size.Width) * DSF + Offset.X, (loc.Y + size.Height) * DSF + Offset.Y, HandleSize, HandleSize)
                    });
                }
                // skip selection rectangle if moving 
                if (MoveObj) {
                    return;
                }
                // draw dotted line around the selection
                Pen selpen = new(Color.Black, 1);
                selpen.DashPattern = [5, 5];
                selpen.DashStyle = DashStyle.Custom;
                g.DrawRectangle(selpen, Selection.Loc.X * DSF + Offset.X,
                    Selection.Loc.Y * DSF + Offset.Y,
                    Selection.Size.Width * DSF,
                    Selection.Size.Height * DSF);
                return;
            }
            if (AreaOnScreen(loc, size)) {
                g.FillRectangles(Brushes.Black, new RectangleF[] {
                    new(loc.X * DSF + Offset.X - HandleSize, loc.Y * DSF + Offset.Y - HandleSize, HandleSize, HandleSize),
                    new((loc.X + size.Width) * DSF + Offset.X, loc.Y * DSF + Offset.Y - HandleSize, HandleSize, HandleSize),
                    new(loc.X * DSF + Offset.X - HandleSize, (loc.Y + size.Height) * DSF + Offset.Y, HandleSize, HandleSize),
                    new((loc.X + size.Width) * DSF + Offset.X, (loc.Y + size.Height) * DSF + Offset.Y, HandleSize, HandleSize)
                });
            }
        }

        private void SelectObj(LayoutSelection type, int index) {
            SelectObj(type, index, ExitLeg.NoTransPt);
        }

        private void SelectObj(LayoutSelection type, int index, ExitLeg leg) {
            SizeF tmpSize = new(0, 0);
            PointF tmpLoc = new(0, 0);
            int order = -1;

            // assign the new selection
            Selection = new TSel {
                Type = type,
                Number = index,
                Leg = leg
            };

            // set location and configure status bar
            switch (Selection.Type) {
            case LayoutSelection.Room:
                order = Room[Selection.Number].Order;
                Selection.Loc = Room[Selection.Number].Loc;
                spRoom1.Text = EditGame.Logics[Selection.Number].ID;
                spID.Text = "";
                break;
            case LayoutSelection.TransPt:
                order = TransPt[Selection.Number].Order;
                Selection.Loc = TransPt[Selection.Number].Loc[(int)Selection.Leg];
                spRoom1.Text = EditGame.Logics[TransPt[Selection.Number].Room[0]].ID;
                spRoom2.Text = EditGame.Logics[TransPt[Selection.Number].Room[1]].ID;
                spID.Text = "";
                break;
            case LayoutSelection.Comment:
                order = Comment[Selection.Number].Order;
                Selection.Loc = Comment[Selection.Number].Loc;
                spID.Text = "";
                break;
            case LayoutSelection.ErrPt:
                order = ErrPt[Selection.Number].Order;
                Selection.Loc = ErrPt[Selection.Number].Loc;
                spID.Text = "";
                if (EditGame.Logics.Contains(ErrPt[Selection.Number].Room) &&
                        ErrPt[Selection.Number].Room > 0) {
                    spRoom2.Text = "To: " + EditGame.Logics[ErrPt[Selection.Number].Room].ID;
                }
                else {
                    spRoom2.Text = "To: {error}";
                }
                break;
            case LayoutSelection.Multiple:
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;

                spID.Text = "Multiple";
                spRoom1.Text = "";
                spRoom2.Text = "";
                // step through all selected objects and draw handles
                for (int i = 0; i < Selection.Number; i++) {
                    switch (SelectedObjects[i].Type) {
                    case LayoutSelection.Room:
                        tmpLoc = Room[SelectedObjects[i].Number].Loc;
                        tmpSize.Width = RM_SIZE;
                        tmpSize.Height = RM_SIZE;
                        break;
                    case LayoutSelection.TransPt:
                        // first or second leg?
                        if (SelectedObjects[i].Leg == ExitLeg.FirstLeg) {
                            // first leg
                            tmpLoc = TransPt[SelectedObjects[i].Number].Loc[0];
                        }
                        else {
                            // second leg
                            tmpLoc = TransPt[SelectedObjects[i].Number].Loc[1];
                        }
                        tmpSize.Width = RM_SIZE / 2;
                        tmpSize.Height = RM_SIZE / 2;
                        break;
                    case LayoutSelection.ErrPt:
                        tmpLoc = ErrPt[SelectedObjects[i].Number].Loc;
                        // adjust size to account for rounded corners
                        tmpSize.Width = 0.56f; // 0.6f;
                        tmpSize.Height = 0.465f; // 0.5196f;
                        break;
                    case LayoutSelection.Comment:
                        tmpLoc = Comment[SelectedObjects[i].Number].Loc;
                        tmpSize = Comment[SelectedObjects[i].Number].Size;
                        break;
                    }
                    // set min and Max
                    if (tmpLoc.X < minX) {
                        minX = tmpLoc.X;
                    }
                    if (tmpLoc.Y < minY) {
                        minY = tmpLoc.Y;
                    }
                    if (tmpLoc.X + tmpSize.Width > maxX) {
                        maxX = tmpLoc.X + tmpSize.Width;
                    }
                    if (tmpLoc.Y + tmpSize.Height > maxY) {
                        maxY = tmpLoc.Y + tmpSize.Height;
                    }
                }
                Selection.Loc = new PointF(minX, minY);
                Selection.Size = new SizeF(maxX - minX, maxY - minY);
                break;
            }

            // enable toolbar buttons
            btnDelete.Enabled = Selection.Type != LayoutSelection.None && Selection.Type != LayoutSelection.Multiple;
            btnHideRoom.Enabled = Selection.Type == LayoutSelection.Room;
            btnFront.Enabled = Selection.Type == LayoutSelection.Multiple || (Selection.Type != LayoutSelection.None && order != ObjOrder.Count - 1);
            btnBack.Enabled = Selection.Type == LayoutSelection.Multiple || (Selection.Type != LayoutSelection.None && order > 0);
        }

        private void GetSelectedObjects(RectangleF dragarea) {

            // populates the selectedobjects array with
            // all objects within the selection area

            // clear the selection
            SelectedObjects.Clear();

            for (int i = 0; i < ObjOrder.Count; i++) {
                switch (ObjOrder[i].Type) {
                case LayoutSelection.Room:
                    // is room inside selection area?
                    if (IsInSelection(Room[ObjOrder[i].Number].Loc, RM_SIZE, RM_SIZE)) {
                        // add it
                        ObjInfo tmp = new(ObjOrder[i].Type, ObjOrder[i].Number);
                        SelectedObjects.Add(tmp);
                    }
                    break;
                case LayoutSelection.TransPt:
                    // is first transpt inside selection area?
                    if (IsInSelection(TransPt[ObjOrder[i].Number].Loc[0], TRANSPT_SIZE, TRANSPT_SIZE)) {
                        // add it
                        ObjInfo tmp = new(ObjOrder[i].Type, ObjOrder[i].Number, ExitLeg.FirstLeg);
                        SelectedObjects.Add(tmp);
                    }
                    // is second pt inside selection area?
                    if (IsInSelection(TransPt[ObjOrder[i].Number].Loc[1], TRANSPT_SIZE, TRANSPT_SIZE)) {
                        // add it
                        ObjInfo tmp = new(ObjOrder[i].Type, ObjOrder[i].Number, ExitLeg.SecondLeg);
                        SelectedObjects.Add(tmp);
                    }
                    break;
                case LayoutSelection.ErrPt:
                    // is errpt inside selection area?
                    if (IsInSelection(ErrPt[ObjOrder[i].Number].Loc, 0.6f, 0.5196f)) {
                        // add it
                        ObjInfo tmp = new(ObjOrder[i].Type, ObjOrder[i].Number, ExitLeg.SecondLeg);
                        SelectedObjects.Add(tmp);
                    }
                    break;
                case LayoutSelection.Comment:
                    // is comment inside selection area?
                    if (IsInSelection(Comment[ObjOrder[i].Number].Loc,
                        Comment[ObjOrder[i].Number].Size.Width,
                        Comment[ObjOrder[i].Number].Size.Height)) {
                        // add it
                        ObjInfo tmp = new(ObjOrder[i].Type, ObjOrder[i].Number, ExitLeg.SecondLeg);
                        SelectedObjects.Add(tmp);
                    }
                    break;
                }
            }
            // if any objects selected, set type and count
            if (SelectedObjects.Count > 1) {
                SelectObj(LayoutSelection.Multiple, SelectedObjects.Count);
            }
            else if (SelectedObjects.Count == 1) {
                // one object selected
                SelectObj(SelectedObjects[0].Type, SelectedObjects[0].Number, SelectedObjects[0].Leg);
            }
            else {
                // nothing is selected
                SelectObj(LayoutSelection.None, 0);
            }

            bool IsInSelection(PointF pos, float width, float height) {
                RectangleF objRect = new(LayoutToScreen(pos), new(width * DSF, height * DSF));
                return dragarea.Contains(objRect);
            }
        }

        /// <summary>
        /// Removes an object from the current selection collection when multiple
        /// objects are selected. Not the same as DeselectObj, which resets the 
        /// selection to None, regardless of what's selected.
        /// </summary>
        /// <param name="obj"></param>
        private void UnselectObj(TSel obj) {
            // removes the object from the selection collection

            // first, find it
            int i;
            bool found = false;
            for (i = 0; i < Selection.Number; i++) {
                if (SelectedObjects[i].Type == obj.Type &&
                    SelectedObjects[i].Number == obj.Number &&
                    SelectedObjects[i].Leg == obj.Leg) {
                    // this is the one
                    found = true;
                    break;
                }
            }
            // if not found
            if (!found) {
                // should never get here; object should always be found
                return;
            }
            // now move other objects that are ABOVE this one down one spot
            SelectedObjects.RemoveAt(i);
            // decrement Count
            Selection.Number--;
            // if down to a single object
            if (Selection.Number == 1) {
                // change selection
                SelectObj(SelectedObjects[0].Type, SelectedObjects[0].Number, SelectedObjects[0].Leg);
            }
            else {
                SelectObj(LayoutSelection.Multiple, Selection.Number);
            }
            picDraw.Invalidate();
        }

        private void SelectExit(TSel newexit) {
            // reset to default
            Selection = new TSel();
            // then assign the new selection
            Selection.Type = LayoutSelection.Exit;
            Selection.Number = newexit.Number;
            Selection.ExitID = newexit.ExitID;
            Selection.Leg = newexit.Leg;
            Selection.TwoWay = newexit.TwoWay;

            // disable toolbar buttons
            btnDelete.Enabled = true;
            btnTransfer.Enabled = Selection.Leg == ExitLeg.NoTransPt;
            // get start and end points
            switch (Selection.Leg) {
            case ExitLeg.NoTransPt:
                Selection.SP = Room[Selection.Number].Exits[Selection.ExitID].SP;
                Selection.EP = Room[Selection.Number].Exits[Selection.ExitID].EP;
                break;
            case ExitLeg.FirstLeg:
                Selection.SP = Room[Selection.Number].Exits[Selection.ExitID].SP;
                if (Room[Selection.Number].Exits[Selection.ExitID].Leg == 0) {
                    // sp of transpt matches sp of exit
                    Selection.EP = TransPt[Room[Selection.Number].Exits[Selection.ExitID].TypeIndex].SP;
                }
                else {
                    // ep of transpt matches sp of exit
                    Selection.EP = TransPt[Room[Selection.Number].Exits[Selection.ExitID].TypeIndex].EP;
                }
                break;
            case ExitLeg.SecondLeg:
                // if this is the first exit for this transfer point
                if (Room[Selection.Number].Exits[Selection.ExitID].Leg == 0) {
                    // ep of trans pt matches ep of exit
                    Selection.SP = TransPt[Room[Selection.Number].Exits[Selection.ExitID].TypeIndex].EP;
                }
                else {
                    // sp of trans pt matches ep of exit
                    Selection.SP = TransPt[Room[Selection.Number].Exits[Selection.ExitID].TypeIndex].SP;
                }
                Selection.EP = Room[Selection.Number].Exits[Selection.ExitID].EP;
                break;
            }
            // write to status bar
            spRoom1.Text = (Selection.TwoWay == ExitDirection.BothWays ? "" : "From: ")
                + EditGame.Logics[Selection.Number].ID;
            // to room may be an error
            if (Room[Selection.Number].Exits[Selection.ExitID].Type == ExitType.Error) {
                spRoom2.Text = "To: {error}";
            }
            else {
                if (Room[Selection.Number].Exits[Selection.ExitID].Room == 0) {
                    spRoom2.Text = "To: {error}";
                }
                else {
                    spRoom2.Text = (Selection.TwoWay == ExitDirection.BothWays ? "" : "To: ")
                        + EditGame.Logics[Room[Selection.Number].Exits[Selection.ExitID].Room].ID;
                }
            }

            switch (Selection.TwoWay) {
            case ExitDirection.BothWays:
                spType.Text = "Both Ways";
                string strID = "";
                if (IsTwoWay(Selection.Number, Selection.ExitID, ref Room[Selection.Number].Exits[Selection.ExitID].Room, ref strID)) {
                    // if it really is a twoway- this gets the second exit ID
                    strID = "/" + strID;
                }
                // now add the first
                strID = Selection.ExitID + strID;

                spID.Text = strID;
                break;
            case ExitDirection.OneWay:
                spType.Text = "One Way";
                spID.Text = Selection.ExitID;
                break;
            case ExitDirection.Single:
                spType.Text = "Single";
                spID.Text = Selection.ExitID;
                break;
            }
        }

        /// <summary>
        /// Checks the specified exit in the specified room to see if it has a
        /// reciprocal exit back to the original room. If so, returns true,
        /// if not, false.
        /// </summary>
        /// <param name="RoomNum"></param>
        /// <param name="ExitID"></param>
        /// <returns></returns>
        private bool IsTwoWay(int RoomNum, string ExitID) {
            int mr = 0;
            string mid = "";
            return IsTwoWay(RoomNum, ExitID, ref mr, ref mid, true);
        }

        /// <summary>
        /// Checks the specified exit in the specified room to see if it has a
        /// reciprocal exit back to the original room. If so, returns true,
        /// if not, false.
        /// </summary>
        /// <param name="RoomNum"></param>
        /// <param name="ExitID"></param>
        /// <returns></returns>
        private bool IsTwoWay(int RoomNum, int ExitID) {
            int mr = 0;
            string mid = "";
            return IsTwoWay(RoomNum, ExitID, ref mr, ref mid, true);
        }

        /// <summary>
        /// Checks the specified exit in the specified room to see if it has a
        /// reciprocal exit back to the original room. If true, MatchRoom and
        /// MatchID are set to the reciprocal exit's room and ID and the
        /// function returns true. If not, returns false.
        /// </summary>
        /// <param name="RoomNum"></param>
        /// <param name="ExitID"></param>
        /// <param name="MatchRoom"></param>
        /// <param name="MatchID"></param>
        /// <returns></returns>
        private bool IsTwoWay(int RoomNum, int ExitID, ref int MatchRoom, ref string MatchID) {
            return IsTwoWay(RoomNum, ExitID, ref MatchRoom, ref MatchID, true);
        }

        /// <summary>
        /// Checks the specified exit in the specified room to see if it has a
        /// reciprocal exit back to the original room. If true, MatchRoom and
        /// MatchID are set to the reciprocal exit's room and ID and the
        /// function returns true. If not, returns false.
        /// </summary>
        /// <param name="RoomNum"></param>
        /// <param name="ExitID"></param>
        /// <param name="MatchRoom"></param>
        /// <param name="MatchID"></param>
        /// <returns></returns>
        private bool IsTwoWay(int RoomNum, string ExitID, ref int MatchRoom, ref string MatchID) {
            return IsTwoWay(RoomNum, ExitID, ref MatchRoom, ref MatchID, true);
        }

        /// <summary>
        /// Checks the specified exit in the specified room to see if it has a
        /// reciprocal exit back to the original room. If true, MatchRoom and
        /// MatchID are set to the reciprocal exit's room and ID and the
        /// function returns true. If not, returns false. Use MatchTrans to
        /// modify the behavior:<br/>
        /// True = IsTwoWay returns true only if transfer Value of reciprocal
        /// matches.<br/>
        /// False = IsTwoWay returns true only if transfer Count is 1,
        /// regardless of transfer Value.
        /// </summary>
        /// <param name="RoomNum"></param>
        /// <param name="ExitID"></param>
        /// <param name="MatchRoom"></param>
        /// <param name="MatchID"></param>
        /// <returns></returns>
        private bool IsTwoWay(int RoomNum, object ExitID, ref int MatchRoom, ref string MatchID, bool MatchTrans) {
            // checks exit ExitID in RoomNum and if it has a reciprocal exit,
            // it returns true, and sets Matchroom and MatchID to reciprocal exit
            // ExitID can be a number (index Value) or string (ID Value)
            // MatchID is always a string (ID Value)
            
            if (ExitID is not string && ExitID is not int) {
                throw new ArgumentException("ExitID must be a string or integer");
            }

            // if 'to room' is invalid (<=0 or >255)
            if (Room[RoomNum].Exits[ExitID].Room <= 0 || Room[RoomNum].Exits[ExitID].Room > 255) {
                return false;
            }
            // or an error exit
            if (Room[RoomNum].Exits[ExitID].Type == ExitType.Error) {
                // then this exit points to an errpt and NEVER
                // is TwoWay
                return false;
            }
            ExitReason tmpDir;
            // get opposite direction
            switch (Room[RoomNum].Exits[ExitID].Reason) {
            case ExitReason.Horizon:
                tmpDir = ExitReason.Bottom;
                break;
            case ExitReason.Bottom:
                tmpDir = ExitReason.Horizon;
                break;
            case ExitReason.Right:
                tmpDir = ExitReason.Left;
                break;
            case ExitReason.Left:
                tmpDir = ExitReason.Right;
                break;
            default:
                tmpDir = ExitReason.Other;
                break;
            }
            // if exit has a transfer point
            if (Room[RoomNum].Exits[ExitID].Type == ExitType.Transfer) {
                // if count=2 then this is a two way
                bool retval = TransPt[Room[RoomNum].Exits[ExitID].TypeIndex].Count == 2;
                // return reciprocal index
                if (Room[RoomNum].Exits[ExitID].Leg == 0) {
                    MatchRoom = TransPt[Room[RoomNum].Exits[ExitID].TypeIndex].Room[1];
                    MatchID = TransPt[Room[RoomNum].Exits[ExitID].TypeIndex].ExitID[1];
                }
                else {
                    MatchRoom = TransPt[Room[RoomNum].Exits[ExitID].TypeIndex].Room[0];
                    MatchID = TransPt[Room[RoomNum].Exits[ExitID].TypeIndex].ExitID[0];
                }
                return retval;
            }

            // not part of a transfer; manually search all exits in target room
            int FromRoom = Room[RoomNum].Exits[ExitID].Room;

            // CAN'T have a reciprocal from a hidden room
            if (!Room[FromRoom].Visible) {
                return false;
            }
            for (int i = 0; i < Room[FromRoom].Exits.Count; i++) {
                // if this exit goes back to original room AND is not deleted
                if (Room[FromRoom].Exits[i].Room == RoomNum && Room[FromRoom].Exits[i].Status != ExitStatus.Deleted) {
                    // if reason matches,
                    if (Room[FromRoom].Exits[i].Reason == tmpDir) {
                        // if NOT a circular exit (from room = to room) OR exits are different
                        bool match;
                        if (ExitID is string sID) {
                            match = Room[FromRoom].Exits[i].ID != sID;
                        }
                        else {
                            match = i != (int)ExitID;
                        }
                        if (FromRoom != RoomNum || match) {
                            // check transfer
                            if (MatchTrans) {
                                // return true only if transfer Value of reciprocal matches
                                if (Room[FromRoom].Exits[i].TypeIndex == Room[RoomNum].Exits[ExitID].TypeIndex) {
                                    MatchRoom = FromRoom;
                                    MatchID = Room[FromRoom].Exits[i].ID;
                                    return true;
                                }
                                break;
                            }
                            else {
                                // returns true only if transfer Count is 1, regardless of transfer Value
                                if (Room[FromRoom].Exits[i].Type == ExitType.Transfer) {
                                    if (TransPt[Room[FromRoom].Exits[i].TypeIndex].Count == 1) {
                                        MatchRoom = FromRoom;
                                        MatchID = Room[FromRoom].Exits[i].ID;
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void SelectOtherTransPt() {
            if (Selection.Leg == ExitLeg.FirstLeg) {
                Selection.Leg = ExitLeg.SecondLeg;
            }
            else {
                Selection.Leg = ExitLeg.FirstLeg;
            }
            // if not on screen
            if (!ObjOnScreen(LayoutSelection.TransPt, Selection.Number, Selection.Leg == ExitLeg.SecondLeg)) {
                // adjust Offet values so object is centered in screen
                Offset.X = picDraw.ClientRectangle.Width / 2 - (int)((TransPt[Selection.Number].Loc[(int)Selection.Leg].X + TRANSPT_SIZE / 2f) * DSF);
                Offset.Y = picDraw.ClientRectangle.Height / 2 - (int)((TransPt[Selection.Number].Loc[(int)Selection.Leg].Y + TRANSPT_SIZE / 2f) * DSF);
                SetScrollBars();
                DrawLayout();
            }
            else {
                picDraw.Invalidate();
            }
        }

        /// <summary>
        /// Moves the selected object(s) to a new location. NewX and NewY
        /// are the layout coordinates of the upper-left corner of the drop
        /// location. If NoGrid is true, the object is dropped without
        /// adjusting to the grid.
        /// </summary>
        /// <param name="NewX"></param>
        /// <param name="NewY"></param>
        /// <param name="NoGrid"></param>
        private void DropSelection(float NewX, float NewY, bool NoGrid = false) {
            // new location is upper-left point (Loc property) of the
            // object (or selection frame if multi) in screen coordinates
            MoveObj = false;

            // if forcing no-grid
            bool tmpGrid = false;
            if (NoGrid) {
                // cache usegrid value
                tmpGrid = WinAGISettings.LEUseGrid.Value;
                // force grid off
                WinAGISettings.LEUseGrid.Value = false;
            }

            // if selection is multiple objects, steps are different
            if (Selection.Type == LayoutSelection.Multiple) {
                // determine offset between original and new locations
                float mDX = (NewX - Offset.X) / DSF - Selection.Loc.X;
                float mDY = (NewY - Offset.Y) / DSF - Selection.Loc.Y;

                // adjust selection position
                Selection.Loc.X = GridPos(Selection.Loc.X + mDX);
                Selection.Loc.Y = GridPos(Selection.Loc.Y + mDY);

                // step through all objects in selection collection
                for (int i = 0; i <= Selection.Number - 1; i++) {
                    switch (SelectedObjects[i].Type) {
                    case LayoutSelection.Room:
                        // set new x and y values of room loc
                        Room[SelectedObjects[i].Number].Loc.X = GridPos(Room[SelectedObjects[i].Number].Loc.X + mDX);
                        Room[SelectedObjects[i].Number].Loc.Y = GridPos(Room[SelectedObjects[i].Number].Loc.Y + mDY);
                        break;
                    case LayoutSelection.TransPt:
                        TransPt[SelectedObjects[i].Number].Loc[(int)SelectedObjects[i].Leg].X =
                            GridPos(TransPt[SelectedObjects[i].Number].Loc[(int)SelectedObjects[i].Leg].X + mDX);
                        TransPt[SelectedObjects[i].Number].Loc[(int)SelectedObjects[i].Leg].Y =
                            GridPos(TransPt[SelectedObjects[i].Number].Loc[(int)SelectedObjects[i].Leg].Y + mDY);
                        break;
                    case LayoutSelection.Comment:
                        Comment[SelectedObjects[i].Number].Loc.X = GridPos(Comment[SelectedObjects[i].Number].Loc.X + mDX);
                        Comment[SelectedObjects[i].Number].Loc.Y = GridPos(Comment[SelectedObjects[i].Number].Loc.Y + mDY);
                        break;
                    case LayoutSelection.ErrPt:
                        ErrPt[SelectedObjects[i].Number].Loc.X = GridPos(ErrPt[SelectedObjects[i].Number].Loc.X + mDX);
                        ErrPt[SelectedObjects[i].Number].Loc.Y = GridPos(ErrPt[SelectedObjects[i].Number].Loc.Y + mDY);
                        break;
                    }
                }
                // step through again and reposition everyone
                for (int i = 0; i <= Selection.Number - 1; i++) {
                    switch (SelectedObjects[i].Type) {
                    case LayoutSelection.Room:
                        RepositionRoom(SelectedObjects[i].Number);
                        break;
                    case LayoutSelection.TransPt:
                        RepositionRoom(TransPt[SelectedObjects[i].Number].Room[(int)SelectedObjects[i].Leg]);
                        break;
                    case LayoutSelection.ErrPt:
                        SetExitPos(ErrPt[SelectedObjects[i].Number].FromRoom, ErrPt[SelectedObjects[i].Number].ExitID);
                        break;
                    }
                }
            }
            else {
                // reposition the object, based on its type
                switch (Selection.Type) {
                case LayoutSelection.Room:
                    // set x and y values of room loc
                    Room[Selection.Number].Loc.X = GridPos((NewX - Offset.X) / DSF);
                    Room[Selection.Number].Loc.Y = GridPos((NewY - Offset.Y) / DSF);
                    Selection.Loc = Room[Selection.Number].Loc;
                    // reposition exits
                    RepositionRoom(Selection.Number);
                    break;
                case LayoutSelection.TransPt:
                    // set x and y values of this trans pt
                    TransPt[Selection.Number].Loc[(int)Selection.Leg].X = GridPos((NewX - Offset.X) / DSF);
                    TransPt[Selection.Number].Loc[(int)Selection.Leg].Y = GridPos((NewY - Offset.Y) / DSF);
                    Selection.Loc = TransPt[Selection.Number].Loc[(int)Selection.Leg];
                    // reposition exits
                    RepositionRoom(TransPt[Selection.Number].Room[0]);
                    break;
                case LayoutSelection.Comment:
                    Comment[Selection.Number].Loc.X = GridPos((NewX - Offset.X) / DSF);
                    Comment[Selection.Number].Loc.Y = GridPos((NewY - Offset.Y) / DSF);
                    Selection.Loc = Comment[Selection.Number].Loc;
                    // set changed flag (since repositionroom is not called for comments)
                    MarkAsChanged();
                    break;
                case LayoutSelection.ErrPt:
                    ErrPt[Selection.Number].Loc.X = GridPos((NewX - Offset.X) / DSF);
                    ErrPt[Selection.Number].Loc.Y = GridPos((NewY - Offset.Y) / DSF);
                    Selection.Loc = ErrPt[Selection.Number].Loc;
                    // don't reposition room! just redraw the exit to this errpt
                    SetExitPos(ErrPt[Selection.Number].FromRoom, ErrPt[Selection.Number].ExitID);
                    break;
                }
            }
            // adjust layout area Max/min, in case objects are moved outside current boundaries
            RecalculateMaxMin();
            // redraw to update everything
            DrawLayout();
            MarkAsChanged();

            // if forcing no-grid
            if (NoGrid) {
                // restore usegrid value from cache
                WinAGISettings.LEUseGrid.Value = tmpGrid;
            }
        }

        private void ChangeScale(int Dir, bool useanchor = false) {
            // not allowed if editing a comment
            if (txtComment.Visible) {
                return;
            }
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
                HandleSize = DrawScale + 2;
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

        /// <summary>
        /// Adds a new room to the game, and draws it on the layout at the
        /// specified position (in layout coordinates).
        /// </summary>
        /// <param name="pos"></param>
        private void AddNewRoom(PointF pos) {
            // add a new room here
            frmGetResourceNum frm = new(GetRes.AddLayout, AGIResType.Logic);
            frm.chkIncludePic.Checked = AddPicToo;
            if (frm.ShowDialog(MDIMain) == DialogResult.OK) {
                // temporary logic
                Logic tmpLogic = new();
                tmpLogic.ID = frm.txtID.Text;
                // always use room template
                tmpLogic.SourceText = NewLogicSourceText(tmpLogic, true);
                tmpLogic.IsRoom = true;
                // save current checkbox Value
                AddPicToo = frm.chkIncludePic.Checked;
                if (AddPicToo) {
                    AddRoomPicture(frm.NewResNum, frm.txtID.Text);
                }
                // add a new logic
                NewRoomLogic(frm.NewResNum, tmpLogic);
                // reposition to desired location
                Room[frm.NewResNum].Loc = pos;
                RepositionRoom(frm.NewResNum);
                RecalculateMaxMin();
                SetScrollBars();
                DrawLayout();
            }
        }

        private void NewRoomLogic(byte NewLogicNumber, Logic NewLogic) {
            EditGame.Logics.Add(NewLogicNumber, NewLogic);
            EditGame.Logics[NewLogicNumber].SaveProps();
            // always save source to new name
            EditGame.Logics[NewLogicNumber].SaveSource();
            MDIMain.AddResourceToList(AGIResType.Logic, NewLogicNumber);
            // update layout editor and layout data file to show this room is in the game
            UpdateLayout(UpdateReason.ShowRoom, NewLogicNumber, []);
            UpdateLayoutFile(UpdateReason.ShowRoom,NewLogicNumber, []);
            // unload it once all done getting it added
            EditGame.Logics[NewLogicNumber].Unload();
        }

        public void InsertErrPt(int FromRoom, int ExitIndex, int ErrRoom) {
            // insert an error point for the exit (ExitIndex) coming from (FromRoom)
            // the original destination (ErrRoom) is no longer in the game

            // inform user that an error point is being inserted
            string strMsg = "Exit " + Room[FromRoom].Exits[ExitIndex].ID + " in '" +
                EditGame.Logics[FromRoom].ID + "' points to a nonexistent room (" +
                ErrRoom + ")." + "An error point will be inserted at this exit point.";
            MessageBox.Show(MDIMain,
                strMsg,
                "Exit Error Detected",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information, 0, 0,
                WinAGIHelp, "htm\\winagi\\Layout_Editor.htm#errpts");

            // find the next available errpt
            int lngEP = 0;
            do {
                lngEP++;
            } while (ErrPt[lngEP].Visible);
            Room[FromRoom].Exits[ExitIndex].Type = ExitType.Error;
            Room[FromRoom].Exits[ExitIndex].TypeIndex = lngEP;
            PointF tmpCoord = Room[FromRoom].Loc;
            switch (Room[FromRoom].Exits[ExitIndex].Reason) {
            case ExitReason.Other:
                // position first around from room
                tmpCoord = GetInsertPos(tmpCoord);
                break;
            case ExitReason.Horizon:
                // position around point above
                tmpCoord.Y--;
                tmpCoord = GetInsertPos(tmpCoord);
                break;
            case ExitReason.Bottom:
                // position around point below
                tmpCoord.Y++;
                tmpCoord = GetInsertPos(tmpCoord);
                break;
            case ExitReason.Left:
                // position around point to left
                tmpCoord.X--;
                tmpCoord = GetInsertPos(tmpCoord);
                break;
            case ExitReason.Right:
                // position around point to right
                tmpCoord.X++;
                tmpCoord = GetInsertPos(tmpCoord);
                break;
            }
            // put errpt here
            // adjust to be centered over same point as a room
            // (by adding .1 to x and .2 to y)
            ErrPt[lngEP].Loc.X = tmpCoord.X + 0.1f;
            ErrPt[lngEP].Loc.Y = tmpCoord.Y + 0.2402f;
            ErrPt[lngEP].Visible = true;
            ErrPt[lngEP].Room = ErrRoom;
            ErrPt[lngEP].FromRoom = FromRoom;
            ErrPt[lngEP].ExitID = Room[FromRoom].Exits[ExitIndex].ID;
            ErrPt[lngEP].Order = ObjOrder.Count;
            ObjOrder.Add(new(LayoutSelection.ErrPt, lngEP, 0));
        }

        private void AddComment(PointF location, SizeF size) {
            // add a comment here
            int i;
            for (i = 1; i < Comment.Length; i++) {
                if (!Comment[i].Visible) {
                    break;
                }
            }
            if (i == Comment.Length) {
                Array.Resize(ref Comment, Comment.Length + 16);
            }
            Comment[i].Visible = true;
            Comment[i].Loc = location;
            Comment[i].Size = size;
            Comment[i].Order = ObjOrder.Count;
            ObjInfo obj = new(LayoutSelection.Comment, i);
            ObjOrder.Add(obj);
            // adjust Max and min
            RecalculateMaxMin();
            SetScrollBars();
            // select, draw and edit the comment box
            SelectObj(LayoutSelection.Comment, i);
            DrawLayout();
            MarkAsChanged();
            BeginEditComment();
            txtComment.Select();
        }

        private TSel ItemFromPos(Point pos) {
            // check for edge exit
            TSel retval = ExitFromPos(1, pos);
            if (retval.Type != LayoutSelection.None) {
                return retval;
            }
            // other exits
            retval = ExitFromPos(0, pos);
            if (retval.Type != LayoutSelection.None) {
                return retval;
            }
            // any other object
            retval = ObjectFromPos(pos);
            return retval;
        }

        private TSel ExitFromPos(int SearchPriority, Point pos) {
            // finds the line under the point x,y that matches search priority
            // returns the exit if found, otherwise returns no selection

            // SearchPriority = 0: only other exits are checked
            //                = 1: all edge exits are searched

            TSel retval = new();

            for (int i = 1; i < 256; i++) {
                if (Room[i].Visible) {
                    for (int j = 0; j < Room[i].Exits.Count; j++) {
                        int tmpRoom = 0;
                        string tmpID = "";
                        PointF ep, sp;
                        // don't include deleted exits
                        if (Room[i].Exits[j].Status != ExitStatus.Deleted) {
                            // if Type matches
                            if ((SearchPriority == 0 && Room[i].Exits[j].Reason == ExitReason.Other) ||
                                (SearchPriority == 1 && Room[i].Exits[j].Reason != ExitReason.Other)) {
                                // if there are no transfer points
                                if (Room[i].Exits[j].Type != ExitType.Transfer) {
                                    // starting point and ending point of line come directly
                                    // from the exit's start-end points
                                    sp = Room[i].Exits[j].SP;
                                    ep = Room[i].Exits[j].EP;
                                    // don't bother checking, unless line is actually visible
                                    if (LineOnScreen(sp, ep)) {
                                        // check for an arrow first
                                        if (PointOnArrow(pos, sp, ep)) {
                                            // if exit has a reciprocal
                                            if (IsTwoWay(i, j)) {
                                                retval.TwoWay = ExitDirection.OneWay;
                                            }
                                            else {
                                                retval.TwoWay = ExitDirection.Single;
                                            }
                                            // select this line
                                            retval.Type = LayoutSelection.Exit;
                                            retval.Number = i;
                                            retval.ExitID = Room[i].Exits[j].ID;
                                            retval.Leg = ExitLeg.NoTransPt;
                                            return retval;
                                        }
                                        // if not on arrow, check line
                                        else if (PointOnLine(pos, sp, ep)) {
                                            // if exit has a reciprocal
                                            if (IsTwoWay(i, j, ref tmpRoom, ref tmpID)) {
                                                // check if arrow on reciprocal is selected
                                                if (PointOnArrow(pos, ep, sp)) {
                                                    // on arrow means one way
                                                    retval.TwoWay = ExitDirection.OneWay;
                                                    retval.Number = tmpRoom;
                                                    retval.ExitID = tmpID;
                                                    retval.Type = LayoutSelection.Exit;
                                                    retval.Leg = ExitLeg.NoTransPt;
                                                    return retval;
                                                }
                                                else {
                                                    // both
                                                    retval.TwoWay = ExitDirection.BothWays;
                                                }
                                            }
                                            else {
                                                // single line
                                                retval.TwoWay = ExitDirection.Single;
                                            }
                                            // return this line
                                            retval.Type = LayoutSelection.Exit;
                                            retval.Number = i;
                                            retval.ExitID = Room[i].Exits[j].ID;
                                            retval.Leg = ExitLeg.NoTransPt;
                                            return retval;
                                        }
                                    }
                                }
                                else {
                                    // there are transfers; check both segments
                                    // first segment
                                    sp = Room[i].Exits[j].SP;
                                    // if this is first exit with transfer point
                                    if (TransPt[Room[i].Exits[j].TypeIndex].Room[0] == i) {
                                        ep = TransPt[Room[i].Exits[j].TypeIndex].SP;
                                    }
                                    else {
                                        // swap ep and sp
                                        ep = TransPt[Room[i].Exits[j].TypeIndex].EP;
                                    }
                                    if (LineOnScreen(sp, ep)) {
                                        // check for an arrow first
                                        if (PointOnArrow(pos, sp, ep)) {
                                            // if exit has a reciprocal
                                            if (IsTwoWay(i, j)) {
                                                retval.TwoWay = ExitDirection.OneWay;
                                            }
                                            else {
                                                retval.TwoWay = ExitDirection.Single;
                                            }
                                            // return this line
                                            retval.Type = LayoutSelection.Exit;
                                            retval.Number = i;
                                            retval.ExitID = Room[i].Exits[j].ID;
                                            // use first leg
                                            retval.Leg = ExitLeg.FirstLeg;
                                            return retval;
                                        }
                                        // if not on arrow, check line
                                        else if (PointOnLine(pos, sp, ep)) {
                                            // if exit has a reciprocal
                                            if (IsTwoWay(i, j, ref tmpRoom, ref tmpID)) {
                                                // check if arrow on reciprocal is selected
                                                if (PointOnArrow(pos, ep, sp)) {
                                                    // on arrow means one way
                                                    retval.TwoWay = ExitDirection.OneWay;
                                                    retval.Number = tmpRoom;
                                                    retval.ExitID = tmpID;
                                                    retval.Type = LayoutSelection.Exit;
                                                    retval.Leg = ExitLeg.SecondLeg;
                                                    return retval;
                                                }
                                                else {
                                                    // both
                                                    retval.TwoWay = ExitDirection.BothWays;
                                                }
                                            }
                                            else {
                                                // single line
                                                retval.TwoWay = ExitDirection.Single;
                                            }
                                            // select line
                                            retval.Type = LayoutSelection.Exit;
                                            retval.Number = i;
                                            retval.ExitID = Room[i].Exits[j].ID;
                                            // use first leg
                                            retval.Leg = ExitLeg.FirstLeg;
                                            return retval;
                                        }
                                    }
                                    // check second segment
                                    // if this is first exit with transfer point
                                    if (TransPt[Room[i].Exits[j].TypeIndex].Room[0] == i) {
                                        sp = TransPt[Room[i].Exits[j].TypeIndex].EP;
                                    }
                                    else {
                                        // swap ep and sp
                                        sp = TransPt[Room[i].Exits[j].TypeIndex].SP;
                                    }
                                    ep = Room[i].Exits[j].EP;
                                    if (LineOnScreen(sp, ep)) {
                                        // check for an arrow first
                                        if (PointOnArrow(pos, sp, ep)) {
                                            // if exit has a reciprocal
                                            if (IsTwoWay(i, j)) {
                                                retval.TwoWay = ExitDirection.OneWay;
                                            }
                                            else {
                                                retval.TwoWay = ExitDirection.Single;
                                            }
                                            // select line
                                            retval.Type = LayoutSelection.Exit;
                                            retval.Number = i;
                                            retval.ExitID = Room[i].Exits[j].ID;
                                            // use second leg
                                            retval.Leg = ExitLeg.SecondLeg;
                                            return retval;
                                        }
                                        // if not on arrow, check line
                                        else if (PointOnLine(pos, sp, ep)) {
                                            // if exit has a reciprocal
                                            if (IsTwoWay(i, j, ref tmpRoom, ref tmpID)) {
                                                // check if arrow on reciprocal is selected
                                                if (PointOnArrow(pos, ep, sp)) {
                                                    // on arrow means one way
                                                    retval.TwoWay = ExitDirection.OneWay;
                                                    retval.Number = tmpRoom;
                                                    retval.ExitID = tmpID;
                                                    retval.Type = LayoutSelection.Exit;
                                                    retval.Leg = ExitLeg.FirstLeg;
                                                    return retval;
                                                }
                                                else {
                                                    // both
                                                    retval.TwoWay = ExitDirection.BothWays;
                                                }
                                            }
                                            else {
                                                // single line
                                                retval.TwoWay = ExitDirection.Single;
                                            }
                                            // return this line
                                            retval.Type = LayoutSelection.Exit;
                                            retval.Number = i;
                                            retval.ExitID = Room[i].Exits[j].ID;
                                            // use second leg
                                            retval.Leg = ExitLeg.SecondLeg;
                                            return retval;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // not found
            return retval;
        }

        private bool PointOnLine(PointF pos, PointF sp, PointF ep) {

            using (var path = new GraphicsPath())
            using (var pen = new Pen(Color.Black, 2f)) {
                path.AddLine(sp.X * DSF + Offset.X, sp.Y * DSF + Offset.Y, ep.X * DSF + Offset.X, ep.Y * DSF + Offset.Y);
                return path.IsOutlineVisible(pos, pen);
            }
        }

        private bool PointOnArrow(PointF pos, PointF sp, PointF ep) {
            // need to double the size used here; that's because when drawing,
            // the actual size used is twice the size (or more accurately, the
            // size drawn on the screen is modified by the line width of the 
            // pen used to draw it, with a minimum of 2 [so lines with width  
            // less than 2 will still double the cap size])
            AdjustableArrowCap cap = new(2f * DSF / 20, 4f * DSF / 20, true);

            float arrowWidth = cap.Width;
            float arrowHeight = cap.Height;
            PointF[] arrowPoints = [
                new PointF(0,0),
                new PointF(-arrowWidth / 2, -arrowHeight),
                new PointF(arrowWidth / 2, -arrowHeight)
            ];

            // determine line direction
            float dx = ep.X - sp.X;
            float dy = ep.Y - sp.Y;
            float angle = -(float)(Math.Atan2(dx, dy) * 180.0 / Math.PI);
            // create outline of arrow as a path
            using (GraphicsPath arrowPath = new GraphicsPath()) {
                arrowPath.AddPolygon(arrowPoints);
                // use matrix to adjust arrow to correct position and orientation
                using (Matrix matrix = new Matrix()) {
                    matrix.Translate(ep.X * DSF + Offset.X, ep.Y * DSF + Offset.Y);
                    matrix.Rotate(angle);
                    arrowPath.Transform(matrix);
                    return arrowPath.IsVisible(pos);
                }
            }
        }

        private bool PointOnCommentText(PointF pos, TSel commentobj) {
            RectangleF cmttextbox = new();
            cmttextbox.Location = LayoutToScreen(Comment[commentobj.Number].Loc);
            cmttextbox.X += 4;
            cmttextbox.Y += 4;
            cmttextbox.Width = Comment[commentobj.Number].Size.Width * DSF - 8;
            cmttextbox.Height = Comment[commentobj.Number].Size.Height * DSF - 8;
            return cmttextbox.Contains(pos);
        }

        private bool PointOnHandle(Point pos, TSel testobj) {
            return PointOnHandle(pos, testobj, out int handleID);
        }

        private bool PointOnHandle(Point pos, TSel testobj, out int handleID) {
            RectangleF handle = new();
            handle.Width = handle.Height = HandleSize;

            RectangleF rect = new Rectangle();

            switch (testobj.Type) {
            case LayoutSelection.Room:
                rect.Location = LayoutToScreen(Room[testobj.Number].Loc);
                rect.Width = rect.Height = RM_SIZE * DSF;
                break;
            case LayoutSelection.TransPt:
                rect.Location = LayoutToScreen(TransPt[testobj.Number].Loc[(int)testobj.Leg]);
                rect.Width = rect.Height = TRANSPT_SIZE * DSF;
                break;
            case LayoutSelection.ErrPt:
                rect.Location = LayoutToScreen(ErrPt[testobj.Number].Loc);
                rect.Width = 0.6f * DSF;
                rect.Height = 0.5196f * DSF;
                break;
            case LayoutSelection.Comment:
                rect.Location = LayoutToScreen(Comment[testobj.Number].Loc);
                rect.Width = Comment[testobj.Number].Size.Width * DSF;
                rect.Height = Comment[testobj.Number].Size.Height * DSF;
                break;
            case LayoutSelection.Exit:
                // check both end points
                handle.Location = LayoutToScreen(testobj.SP);
                handle.X -= HandleSize / 2;
                handle.Y -= HandleSize / 2;
                if (handle.Contains(pos)) {
                    handleID = 0;
                    return true;
                }
                handle.Location = LayoutToScreen(testobj.EP);
                handle.X -= HandleSize / 2;
                handle.Y -= HandleSize / 2;
                if (handle.Contains(pos)) {
                    handleID = 1;
                    return true;
                }
                handleID = -1;
                return false;
            }

            // check each handle location:
            // upper left
            handle.Location = rect.Location;
            handle.X = rect.X - HandleSize;
            handle.Y = rect.Y - HandleSize;
            if (handle.Contains(pos)) {
                handleID = 0;
                return true;
            }
            // upper right
            handle.X = rect.Right;
            handle.Y = rect.Y - HandleSize;
            if (handle.Contains(pos)) {
                handleID = 1;
                return true;
            }
            // lower left
            handle.X = rect.X - HandleSize;
            handle.Y = rect.Bottom;
            if (handle.Contains(pos)) {
                handleID = 2;
                return true;
            }
            // lower right
            handle.X = rect.Right;
            handle.Y = rect.Bottom;
            if (handle.Contains(pos)) {
                handleID = 3;
                return true;
            }

            // not on handle
            handleID = -1;
            return false;
        }

        private bool PointOnObject(Point pos, TSel testobj) {
            RectangleF rect = new();
            switch (testobj.Type) {
            case LayoutSelection.Room:
                rect.Location = LayoutToScreen(Room[testobj.Number].Loc);
                rect.Width = rect.Height = RM_SIZE * DSF;
                if (rect.Contains(pos)) {
                    return true;
                }
                break;
            case LayoutSelection.TransPt:
                using (GraphicsPath path = new GraphicsPath()) {
                    rect.Location = LayoutToScreen(TransPt[testobj.Number].Loc[(int)testobj.Leg]);
                    rect.Width = rect.Height = TRANSPT_SIZE * DSF;
                    path.AddEllipse(rect);
                    if (path.IsVisible(pos)) {
                        return true;
                    }
                }
                break;
            case LayoutSelection.ErrPt:
                float radius = 5.0f * DSF / 40; // radius for rounded corners
                using (GraphicsPath path = new GraphicsPath()) {
                    PointF[] errPoints =
                    [
                        LayoutToScreen(ErrPt[testobj.Number].Loc),
                        new PointF((ErrPt[testobj.Number].Loc.X + 0.6f) * DSF + Offset.X,
                            ErrPt[testobj.Number].Loc.Y * DSF + Offset.Y),
                        new PointF((ErrPt[testobj.Number].Loc.X + 0.3f) * DSF + Offset.X,
                            (ErrPt[testobj.Number].Loc.Y + 0.5196f) * DSF + Offset.Y)
                    ];
                    // Create rounded triangle path
                    path.AddArc(errPoints[0].X + radius * 0.366f, errPoints[0].Y, radius, radius, 150, 120);
                    path.AddArc(errPoints[1].X - radius * 1.366f, errPoints[1].Y, radius, radius, 270, 120);
                    path.AddArc(errPoints[2].X - radius * 0.5f, errPoints[2].Y - radius * 1.5f, radius, radius, 30, 120);
                    path.CloseFigure();
                    if (path.IsVisible(pos)) {
                        return true;
                    }
                }
                break;
            case LayoutSelection.Comment:
                rect.Location = LayoutToScreen(Comment[testobj.Number].Loc);
                rect.Width = Comment[testobj.Number].Size.Width * DSF;
                rect.Height = Comment[testobj.Number].Size.Height * DSF;
                radius = 5.0f * DSF / 40; // radius for rounded corners
                using (GraphicsPath path = new GraphicsPath()) {
                    path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                    path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                    path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                    path.CloseFigure();
                    if (path.IsVisible(pos)) {
                        return true;
                    }
                }
                break;
            }
            // not on the object
            return false;
        }

        private PointF LayoutToScreen(PointF layoutpos) {
            return new(layoutpos.X * DSF + Offset.X, layoutpos.Y * DSF + Offset.Y);
        }

        private float LayoutToScreenX(float layoutx) {
            return layoutx * DSF + Offset.X;
        }

        private float LayoutToScreenY(float layouty) {
            return layouty * DSF + Offset.Y;
        }

        private PointF ScreenToLayout(PointF pos) {
            return new((pos.X - Offset.X) / DSF, (pos.Y - Offset.Y) / DSF);
        }

        private float ScreenToLayoutX(float screenx) {
            return (screenx - Offset.X) / DSF;
        }

        private float ScreenToLayoutY(float screeny) {
            return (screeny - Offset.Y) / DSF;
        }

        private void HighlightExitStart(int RoomNumber, ExitReason Dir) {
            switch (Dir) {
            case ExitReason.Horizon:
                LineStart.X = Room[RoomNumber].Loc.X;
                LineEnd.X = Room[RoomNumber].Loc.X + RM_SIZE;
                LineStart.Y = Room[RoomNumber].Loc.Y;
                LineEnd.Y = Room[RoomNumber].Loc.Y;
                break;
            case ExitReason.Bottom:
                LineStart.X = Room[RoomNumber].Loc.X;
                LineEnd.X = Room[RoomNumber].Loc.X + RM_SIZE;
                LineStart.Y = Room[RoomNumber].Loc.Y + RM_SIZE;
                LineEnd.Y = Room[RoomNumber].Loc.Y + RM_SIZE;
                break;
            case ExitReason.Right:
                LineStart.X = Room[RoomNumber].Loc.X + RM_SIZE;
                LineEnd.X = Room[RoomNumber].Loc.X + RM_SIZE;
                LineStart.Y = Room[RoomNumber].Loc.Y;
                LineEnd.Y = Room[RoomNumber].Loc.Y + RM_SIZE;
                break;
            case ExitReason.Left:
                LineStart.X = Room[RoomNumber].Loc.X;
                LineEnd.X = Room[RoomNumber].Loc.X;
                LineStart.Y = Room[RoomNumber].Loc.Y;
                LineEnd.Y = Room[RoomNumber].Loc.Y + RM_SIZE;
                break;
            case ExitReason.Other:
                // dont show line
                break;
            }
            if (ShowExitStart != Dir) {
                picDraw.Invalidate();
            }
            ShowExitStart = Dir;
        }

        private TSel ObjectFromPos(Point pos) {
            TSel retval = new();
            // if an object is found under the cursor it is returned to caller

            // go backwards through object stack to respect order
            for (int i = ObjOrder.Count - 1; i >= 0; i--) {
                retval.Type = ObjOrder[i].Type;
                retval.Number = ObjOrder[i].Number;
                retval.Leg = ExitLeg.NoTransPt;
                switch (retval.Type) {
                case LayoutSelection.Room:
                    retval.Loc = Room[retval.Number].Loc;
                    break;
                case LayoutSelection.TransPt:
                    // check first leg first
                    retval.Loc = TransPt[retval.Number].Loc[0];
                    retval.Leg = ExitLeg.FirstLeg;
                    break;
                case LayoutSelection.ErrPt:
                    retval.Loc = ErrPt[retval.Number].Loc;
                    break;
                case LayoutSelection.Comment:
                    retval.Loc = Comment[retval.Number].Loc;
                    break;
                }
                // now check this object
                if (PointOnObject(pos, retval)) {
                    return retval;
                }
                // for transfers, need to also check the other leg
                if (retval.Type == LayoutSelection.TransPt) {
                    retval.Loc = TransPt[retval.Number].Loc[1];
                    retval.Leg = ExitLeg.SecondLeg;
                    if (PointOnObject(pos, retval)) {
                        return retval;
                    }
                }
            }
            // default to nothing selected
            retval.Type = LayoutSelection.None;
            retval.Number = 0;
            retval.Leg = ExitLeg.NoTransPt;
            return retval;
        }

        private void MakeNewSelection(TSel newselection) {
            DeselectObj();
            // now select what's under cursor
            if (newselection.Type == LayoutSelection.Exit) {
                SelectExit(newselection);
            }
            else {
                SelectObj(newselection.Type, newselection.Number, newselection.Leg);
            }
            picDraw.Invalidate();
        }

        private bool SameAsSelection(TSel tmpSel) {
            // compares tmpSel with current selection object; if all elements are equal,
            // returns true (x and y values are not part of the check)

            return Selection.Type == tmpSel.Type &&
                   Selection.Number == tmpSel.Number &&
                   Selection.ExitID == tmpSel.ExitID &&
                   Selection.Leg == tmpSel.Leg &&
                   Selection.TwoWay == tmpSel.TwoWay;
        }

        private bool IsSelected(TSel testobject) {
            // Returns true if the specified object is selected
            switch (Selection.Type) {
            case LayoutSelection.Room:
                return (testobject.Type == LayoutSelection.Room) &&
                    (testobject.Number == Selection.Number);
            case LayoutSelection.TransPt:
                return (testobject.Type == LayoutSelection.TransPt) &&
                    (testobject.Number == Selection.Number) &&
                    (testobject.Leg == Selection.Leg);
            case LayoutSelection.ErrPt:
                return (testobject.Type == LayoutSelection.ErrPt) &&
                    (testobject.Number == Selection.Number);
            case LayoutSelection.Multiple:
                for (int i = 0; i <= Selection.Number - 1; i++) {
                    switch (testobject.Type) {
                    case LayoutSelection.TransPt:
                        if (SelectedObjects[i].Type == LayoutSelection.TransPt) {
                            if (testobject.Number == SelectedObjects[i].Number) {
                                if (testobject.Leg == SelectedObjects[i].Leg) {
                                    return true;
                                }
                            }
                        }
                        break;
                    case LayoutSelection.Room:
                    case LayoutSelection.ErrPt:
                    case LayoutSelection.Comment:
                        if (testobject.Type == SelectedObjects[i].Type &&
                            testobject.Number == SelectedObjects[i].Number) {
                            return true;
                        }
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
                case LayoutSelection.Room:
                    CheckMaxMin(Room[lngNum].Loc.X, Room[lngNum].Loc.Y, RM_SIZE, RM_SIZE);
                    break;
                case LayoutSelection.TransPt:
                    CheckMaxMin(TransPt[lngNum].Loc[0].X, TransPt[lngNum].Loc[0].Y, RM_SIZE / 2, RM_SIZE / 2);
                    CheckMaxMin(TransPt[lngNum].Loc[1].X, TransPt[lngNum].Loc[1].Y, RM_SIZE / 2, RM_SIZE / 2);
                    break;
                case LayoutSelection.ErrPt:
                    CheckMaxMin(ErrPt[lngNum].Loc.X, ErrPt[lngNum].Loc.Y, 0.6f, 0.5196f);
                    break;
                case LayoutSelection.Comment:
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

            if (Selection.Type == LayoutSelection.None) {
                // no action needed
                return;
            }
            // reset selection 
            Selection = new();

            // disable toolbar buttons
            btnDelete.Enabled = false;
            btnTransfer.Enabled = false;
            btnHideRoom.Enabled = false;
            btnFront.Enabled = false;
            btnBack.Enabled = false;

            // reset statusbar
            spRoom1.Text = "";
            spRoom2.Text = "";
            spType.Text = "";
            spID.Text = "";

            // refresh the drawing surface
            picDraw.Invalidate();
        }

        internal void SelectRoom(int room) {
            // this is called by the main form when a logic resource is chosen on
            //  the treelist; this sub then unselects the current selection and
            //  then selects RoomNum, repositioning the drawing surface if
            //  necessary

            // if there is currently a selection
            if (Selection.Type != LayoutSelection.None) {
                DeselectObj();
            }

            // now select this room
            SelectObj(LayoutSelection.Room, room);

            // is the room on the screen?
            if (!ObjOnScreen(LayoutSelection.Room, room)) {
                // reposition it by adjusting OFFSET then redraw
                Offset.X = (int)(picDraw.ClientRectangle.Width / 2 - (Room[room].Loc.X + RM_SIZE / 2) * DSF);
                Offset.Y = (int)(picDraw.ClientRectangle.Height / 2 - (Room[room].Loc.Y + RM_SIZE / 2) * DSF);
                DrawLayout();
            }
        }

        private void DisplayRoom(int NewRoom, bool NeedPos, bool NoExits = false) {
            // should only be called for a room that is not currently visible;
            // if position is already known, such as when dropping a new room
            // on drawing surface, don't need to get a default position

            // - mark as visible,
            // - step through all exits in game, and if error exits or hidden exits
            //   that point to this room; replace the error/hidden marker with the room
            //   (don't need to check for transfers because any exits to this
            //   room that currently exist would be marked as errpts)

            Debug.Assert(!Room[NewRoom].Visible);
            bool blnFound = false;

            // add the new room to layout
            Room[NewRoom].Visible = true;
            Room[NewRoom].ShowPic = WinAGISettings.LEShowPics.Value;
            Room[NewRoom].Order = ObjOrder.Count;
            ObjOrder.Add(new(LayoutSelection.Room, NewRoom));

            // if position needed, and no previous reference found,
            if (NeedPos) {
                // add in middle of display
                Room[NewRoom].Loc = GetInsertPos(ScreenToLayoutX(picDraw.ClientRectangle.Width / 2 - RM_SIZE / 2), ScreenToLayoutY(picDraw.ClientRectangle.Height / 2 - RM_SIZE / 2));
            }
            // look for exits pointing to error points which match this
            // look for hidden exits pointing to this
            for (int i = 1; i < 256; i++) {
                // if this room is visible (but not the room being added)
                if (Room[i].Visible && i != NewRoom) {
                    if (Room[i].Exits.Count > 0) {
                        for (int j = 0; j < Room[i].Exits.Count; j++) {
                            if (Room[i].Exits[j].Room == NewRoom &&
                                            Room[i].Exits[j].Status != ExitStatus.Deleted) {
                                // change status back to normal
                                Room[i].Exits[j].Status = ExitStatus.OK;
                                // in case an err pt is found, deal with it
                                if (Room[i].Exits[j].Type == ExitType.Error) {
                                    int errnum = Room[i].Exits[j].TypeIndex;
                                    // if location not known, and not found yet
                                    if (!blnFound && NeedPos) {
                                        blnFound = true;
                                        // move it here
                                        Room[NewRoom].Loc.X = GridPos(ErrPt[errnum].Loc.X - 0.1f);
                                        Room[NewRoom].Loc.Y = GridPos(ErrPt[errnum].Loc.Y - 0.1402f);
                                    }
                                    // hide errpt
                                    RemoveObjFromStack(ErrPt[errnum].Order);
                                    ErrPt[errnum].Visible = false;
                                    ErrPt[errnum].ExitID = "";
                                    ErrPt[errnum].Room = 0;
                                    ErrPt[errnum].FromRoom = 0;
                                    // clear transfer marker
                                    Room[i].Exits[j].Type = ExitType.Normal;
                                    Room[i].Exits[j].TypeIndex = 0;
                                }
                                // if hidden, unhide it
                                if (Room[i].Exits[j].Hidden) {
                                    Room[i].Exits[j].Hidden = false;
                                }
                                // recalculate exits for this room (by calling reposition)
                                RepositionRoom(i);
                            }
                        }
                    }
                }
            }
            //  only extract exits if not skipping them
            if (!NoExits) {
                // extract exits for room being added
                Room[NewRoom].Exits = ExtractExits(EditGame.Logics[NewRoom]);
            }
        }

        private void ShowRoom() {
            // selects a non-room logic, and makes it a room

            using frmGetResourceNum frm = new(GetRes.ShowRoom, AGIResType.Logic);
            DialogResult rtn;

            frm.ResType = AGIResType.Logic;
            if (frm.NewResNum != 255) {
                rtn = frm.ShowDialog(MDIMain);
            }
            else {
                // all rooms already visible!
                return;
            }
            if (rtn == DialogResult.OK) {
                // mark room as visible
                EditGame.Logics[frm.NewResNum].IsRoom = true;
                // get new exits from the logic that was passed
                AGIExits tmpExits = ExtractExits(EditGame.Logics[frm.NewResNum]);
                // update layout file
                UpdateLayoutFile(UpdateReason.ShowRoom, frm.NewResNum, tmpExits);
                // update the layout
                UpdateLayout(UpdateReason.ShowRoom, frm.NewResNum, tmpExits);
                // and redraw to refresh the editor
                DrawLayout();

                // update the resource tree/preview window
                RefreshTree(AGIResType.Logic, frm.NewResNum, true);
            }
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
            if (Selection.Type != LayoutSelection.None) {
                DeselectObj();
            }
            // first, check for any existing transfers or errpts FROM the room being hidden
            for (int i = 0; i < Room[OldRoom].Exits.Count; i++) {
                int transfer = Room[OldRoom].Exits[i].TypeIndex;
                switch (Room[OldRoom].Exits[i].Type) {
                case ExitType.Transfer:
                    // remove the transfer point
                    DeleteTransfer(transfer);
                    break;
                case ExitType.Error:
                    // remove errpt
                    RemoveObjFromStack(ErrPt[transfer].Order);
                    ErrPt[transfer].Visible = false;
                    ErrPt[transfer].ExitID = "";
                    ErrPt[transfer].FromRoom = 0;
                    ErrPt[transfer].Room = 0;
                    ErrPt[transfer].Loc = new();
                    break;
                }
            }
            // remove the room
            Room[OldRoom].Visible = false;
            Room[OldRoom].Loc = new();
            // remove from object list
            RemoveObjFromStack(Room[OldRoom].Order);
            Room[OldRoom].ShowPic = false;
            // clear the exits for removed room
            Room[OldRoom].Exits.Clear();

            // step through all other exits
            for (int i = 1; i < 256; i++) {
                // only need to check rooms that are currently visible
                if (Room[i].Visible) {
                    for (int j = 0; j < Room[i].Exits.Count; j++) {
                        if (Room[i].Exits[j].Room == OldRoom && Room[i].Exits[j].Status != ExitStatus.Deleted) {
                            // show the exit as hidden, if the target room is still a valid logic
                            // otherwise add an ErrPt
                            if (EditGame.Logics.Contains(OldRoom)) {
                                // mark the exit as hidden
                                Room[i].Exits[j].Hidden = true;
                            }
                            else {
                                // need to replace the exit with an errpoint, if not already done
                                if (Room[i].Exits[j].Type != ExitType.Error) {
                                    // insert a error point
                                    InsertErrPt(i, j, OldRoom);
                                }
                            }
                            // check for transfer pt
                            if (Room[i].Exits[j].Type == ExitType.Transfer) {
                                // remove it
                                DeleteTransfer(Room[i].Exits[j].TypeIndex);
                            }
                            // set exit pos
                            SetExitPos(i, Room[i].Exits[j].ID);
                        }
                    }
                }
            }
            MarkAsChanged();
        }

        private void OpenRoomLogic(TSel tmpSel) {
            // determine if any exits need updating
            for (int j = 0; j < Room[tmpSel.Number].Exits.Count; j++) {
                if (Room[tmpSel.Number].Exits[j].Status != ExitStatus.OK) {
                    // update all exits for this logic
                    UpdateLogicCode(EditGame.Logics[tmpSel.Number]);
                    // update the layout file
                    UpdateLayoutFile(UpdateReason.UpdateRoom, tmpSel.Number, Room[tmpSel.Number].Exits);
                    break;
                }
            }
            // if on an errpt
            if (tmpSel.Type == LayoutSelection.ErrPt) {
                // open 'from' logic
                try {
                    OpenGameLogic((byte)ErrPt[Selection.Number].FromRoom, true);
                    // find the logic editor
                    foreach (frmLogicEdit frm in LogicEditors) {
                        if (frm.InGame && frm.LogicNumber == ErrPt[Selection.Number].FromRoom) {
                            // find and highlight the errpt exit
                            frm.BringToFront();
                            int exitpos = frm.fctb.Text.IndexOf("##" + ErrPt[tmpSel.Number].ExitID + "##", StringComparison.Ordinal);
                            Place start = frm.fctb.PositionToPlace(exitpos);
                            Place end = start; // new(start.iChar + 9, start.iLine);
                            frm.fctb.Selection.Start = start;
                            frm.fctb.Selection.End = end;
                            frm.fctb.DoSelectionVisible();
                            frm.fctb.Refresh();
                            break;
                        }
                    }
                }
                catch (Exception ex) {
                    // unable to open
                    ErrMsgBox(ex, "Unable to open the source file.", "", "File Open Error");
                }
            }
            else {
                try {
                    // open logic for editing
                    OpenGameLogic((byte)Selection.Number, true);
                    // find the logic editor
                    foreach (frmLogicEdit frm in LogicEditors) {
                        if (frm.InGame && frm.LogicNumber == Selection.Number) {
                            // find the editor
                            frm.BringToFront();
                            // if on an exit, jump to its location
                            if (tmpSel.Type == LayoutSelection.Exit) {
                                int exitpos = frm.fctb.Text.IndexOf("##" + tmpSel.ExitID + "##", StringComparison.Ordinal);
                                Place start = frm.fctb.PositionToPlace(exitpos);
                                Place end = start; // new(start.iChar + 9, start.iLine);
                                frm.fctb.Selection.Start = start;
                                frm.fctb.Selection.End = end;
                                frm.fctb.DoSelectionVisible();
                                frm.fctb.Refresh();
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex) {
                    // unable to open
                    ErrMsgBox(ex, "Unable to open the source file.", "", "File Open Error");
                }
            }
        }

        private TSel ChangeFromRoom(TSel OldSel, int FromRoom) {
            // changes the currently selected exit so its from room matches FromRoom
            // before calling this method, the new FromRoom has already been validated
            // returns the new exit

            // create new exit from new 'from' room to current 'to' room of same Type
            string NewID = CreateNewExit(FromRoom,
                Room[OldSel.Number].Exits[OldSel.ExitID].Room,
                Room[OldSel.Number].Exits[OldSel.ExitID].Reason);

            // delete old exit
            DeleteExit(OldSel);
            // select new line (never has a transfer)
            OldSel.Number = FromRoom;
            OldSel.ExitID = NewID;

            // if resulting exit has a transfer,
            if (Room[OldSel.Number].Exits[OldSel.ExitID].Type == ExitType.Transfer) {
                // select first leg
                OldSel.Leg = ExitLeg.FirstLeg;
            }
            else {
                OldSel.Leg = ExitLeg.NoTransPt;
            }

            // if resulting exit has a reciprocal
            if (IsTwoWay(OldSel.Number, OldSel.ExitID)) {
                // select only this exit
                OldSel.TwoWay = ExitDirection.OneWay;
            }
            else {
                OldSel.TwoWay = ExitDirection.Single;
            }
            // reposition exit end points
            SetExitPos(OldSel.Number, OldSel.ExitID);
            return OldSel;
        }

        private TSel ChangeToRoom(TSel OldSel, int NewRoom, LayoutSelection ObjType) {
            // changes the currently selected exit so its 'to' room matches NewRoom
            // before calling this method, the new to-room has already been validated
            // returns the new exit

            // save oldroom if we need it
            AGIExit exit = Room[OldSel.Number].Exits[OldSel.ExitID];
            int OldRoom = exit.Room;

            // if a transfer,
            if (ObjType == LayoutSelection.TransPt) {
                // make change to room
                exit.Room = TransPt[NewRoom].Room[0];
                // set transfer
                exit.Type = ExitType.Transfer;
                exit.TypeIndex = NewRoom;
                // mark as second leg
                exit.Leg = 1;
                TransPt[NewRoom].Count = 2;
                TransPt[NewRoom].ExitID[1] = OldSel.ExitID;
                // mark selection as part one of a two way
                OldSel.Leg = ExitLeg.FirstLeg;
                OldSel.TwoWay = ExitDirection.OneWay;
            }
            else {
                // make change
                exit.Room = NewRoom;

                // if previous 'to' room was an err pt
                if (exit.Type == ExitType.Error) {
                    // delete the errpt
                    ErrorPtInfo errpt = ErrPt[exit.TypeIndex];
                    RemoveObjFromStack(errpt.Order);
                    errpt.Visible = false;
                    errpt.ExitID = "";
                    errpt.FromRoom = 0;
                    errpt.Room = 0;
                    errpt.Loc.X = 0;
                    errpt.Loc.Y = 0;
                    exit.Type = ExitType.Normal;
                    exit.TypeIndex = 0;
                }

                // if line previously had a transfer
                if (exit.Type == ExitType.Transfer) {
                    // reset transfer
                    TransPt[exit.TypeIndex].Count--;
                    if (TransPt[exit.TypeIndex].Count == 0) {
                        // transfer no longer needed
                        DeleteTransfer(exit.TypeIndex);
                    }
                    else {
                        // ensure 'from' room is in first position- Room[0]
                        // and 'to' room is in second position- Room[1]
                        // in this case, OldSel.Number is the 'to' room, so
                        // we need only check if second element = OldSel.Number
                        // switching if necessary
                        if (exit.Leg == 0) {
                            TransPtInfo tp = TransPt[exit.TypeIndex];
                            (tp.Room[0], tp.Room[1]) = (tp.Room[1], tp.Room[0]);
                            (tp.Loc[0], tp.Loc[1]) = (tp.Loc[1], tp.Loc[0]);
                            (tp.EP, tp.SP) = (tp.SP, tp.EP);
                            tp.ExitID[0] = tp.ExitID[1];
                            // dont need to keep index for second leg since it is gone
                            tp.ExitID[1] = "";
                            Room[tp.Room[0]].Exits[tp.ExitID[0]].Leg = 0;
                        }
                        else {
                            exit.Leg = 0;
                        }
                    }
                    exit.Type = ExitType.Normal;
                    exit.TypeIndex = 0;
                }
                if (exit.Hidden) {
                    exit.Hidden = false;
                }
                int tmpRoom = 0;
                string tmpID = "";
                // if there is a reciprocal,
                if (IsTwoWay(OldSel.Number, OldSel.ExitID, ref tmpRoom, ref tmpID, false)) {
                    // check for transfer
                    if (Room[tmpRoom].Exits[tmpID].Type == ExitType.Transfer) {
                        // if transfer has only one leg,
                        if (TransPt[Room[tmpRoom].Exits[tmpID].TypeIndex].Count == 1) {
                            // use this transfer
                            TransPt[Room[tmpRoom].Exits[tmpID].TypeIndex].Count = 2;
                            exit.TypeIndex = Room[tmpRoom].Exits[tmpID].TypeIndex;
                            // this is second leg
                            OldSel.Leg = ExitLeg.SecondLeg;
                            exit.Leg = 1;
                            // set exit ID
                            TransPt[Room[tmpRoom].Exits[tmpID].TypeIndex].ExitID[1] = OldSel.ExitID;
                        }
                        else {
                            // ensure selection is not using a transfer leg
                            OldSel.Leg = ExitLeg.NoTransPt;
                            exit.Leg = 0;
                        }
                    }
                    else {
                        // no transfer;
                        // ensure selection is not using a transfer leg
                        OldSel.Leg = ExitLeg.NoTransPt;
                        exit.Leg = 0;
                    }
                    // 
                    OldSel.TwoWay = ExitDirection.OneWay;
                }
                else {
                    // no reciprocal;
                    // change selection to no leg
                    OldSel.Leg = ExitLeg.NoTransPt;
                    OldSel.TwoWay = ExitDirection.Single;
                    exit.Leg = 0;
                }
            }

            // if the exit is not a new exit
            if (exit.Status != ExitStatus.NewExit) {
                // save original room before changing
                if (exit.OldRoom == 0) {
                    exit.OldRoom = OldRoom;
                }
                // mark it as changed
                exit.Status = ExitStatus.Changed;
            }
            // reposition exit end points
            SetExitPos(OldSel.Number, OldSel.ExitID);
            return OldSel;
        }

        private string CreateNewExit(int FromRoom, int ToRoom, ExitReason Reason) {
            bool blnDupeOK = false;
            string retval = "";
            int i = 0, j = 0;
            string tmpID = "";
            int tmpRoom = 0;
            // create new exit from new 'from' room to current 'to' room of same Type
            // returns the index of the new exit
            // 
            // - transfer points can only support one exit from A to B
            //   and a single reciprocal exit from B to A
            // - transfers can't mix and match edge exits with 'other' exits

            // if exit is not created, return an empty string (so calling code can do error checking)

            // first check is to see if an exit that matches the new exit (same code, same target)
            // step through all exits for this room
            for (i = 0; i < Room[FromRoom].Exits.Count; i++) {
                switch (Room[FromRoom].Exits[i].Status) {
                case ExitStatus.Deleted:
                    // check to see if we are restoring the deleted exit
                    // if deleted, AND ToRoom is the OldToRoom, AND Reason is same,
                    if (Room[FromRoom].Exits[i].OldRoom == ToRoom && Room[FromRoom].Exits[i].Reason == Reason) {
                        // restore the exit rather than create a new one
                        Room[FromRoom].Exits[i].Status = ExitStatus.OK;

                        // return the identified exit
                        retval = Room[FromRoom].Exits[i].ID;

                        // make sure room is reset, if necessary
                        Room[FromRoom].Exits[i].Room = Room[FromRoom].Exits[i].OldRoom;

                        // check for reciprocal exit to see if there might
                        // be an eligible transfer pt between the two rooms
                        if (IsTwoWay(FromRoom, retval, ref tmpRoom, ref tmpID, false)) {
                            // use this transfer for the restored exit
                            Room[FromRoom].Exits[i].TypeIndex = Room[tmpRoom].Exits[tmpID].TypeIndex;
                            Room[FromRoom].Exits[i].Leg = 1;
                            Debug.Assert(TransPt[Room[FromRoom].Exits[i].TypeIndex].Count == 1);
                            TransPt[Room[FromRoom].Exits[i].TypeIndex].Count = 2;
                            TransPt[Room[FromRoom].Exits[i].TypeIndex].ExitID[1] = Room[FromRoom].Exits[i].ID;
                        }

                        // recalculate exit points (since rooms may have moved since it was deleted)
                        SetExitPos(FromRoom, retval);
                        MarkAsChanged();
                        return retval;
                    }
                    break;
                case ExitStatus.OK or ExitStatus.NewExit or ExitStatus.Changed:
                    // is this a matching edge exit?
                    Debug.Assert(Reason != ExitReason.None);
                    // unknown ok; while it's not possible for user to create
                    // new exit of 'unknown' Type, an existing 'unknown' exit
                    // that has 'fromroom' changed will call this logic
                    // in that case, we don't care if there's already an 'unknown'
                    // exit from this room...
                    if (Room[FromRoom].Exits[i].Room == ToRoom && Room[FromRoom].Exits[i].Reason == Reason && Reason != ExitReason.Other) {
                        // a duplicate entry- warn?
                        if (!blnDupeOK) {
                            DialogResult rtn = MessageBox.Show(MDIMain,
                                "There is already a '" + EditGame.ReservedDefines.EdgeCodes[(int)Reason].Name + "' exit from " +
                                EditGame.Logics[FromRoom].ID + " to " + EditGame.Logics[ToRoom].ID +
                                "\n\nDo you want to create a duplicate exit?",
                                "Duplicate Exit",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);
                            if (rtn == DialogResult.No) {
                                return "";
                            }
                        }
                        // hide duplicate warning in the event other exits also are a match
                        blnDupeOK = true;
                    }
                    break;
                }
            }

            // no previous exit found; get a valid exit ID
            do {
                j++;
                // step through all exits for this room
                for (i = 0; i < Room[FromRoom].Exits.Count; i++) {
                    // if there is already an exit with this id
                    if (Room[FromRoom].Exits[i].ID[3..].IntVal() == j) {
                        // this id is in use; exit for loop and try again with next id
                        break;
                    }
                }
            } while (i != Room[FromRoom].Exits.Count);

            // add a new exit, using j as id
            retval = "LE" + j.ToString("000");
            Room[FromRoom].Exits.Add(j, ToRoom, Reason, 0).Status = ExitStatus.NewExit;
            // check for reciprocal exit
            tmpRoom = 0;
            tmpID = "";
            if (IsTwoWay(FromRoom, retval, ref tmpRoom, ref tmpID, false)) {
                // use this transfer for the new exit
                Room[FromRoom].Exits[retval].TypeIndex = Room[tmpRoom].Exits[tmpID].TypeIndex;
                Room[FromRoom].Exits[retval].Leg = 1;
                TransPt[Room[FromRoom].Exits[retval].TypeIndex].Count = 2;
                TransPt[Room[FromRoom].Exits[retval].TypeIndex].ExitID[1] = Room[FromRoom].Exits[retval].ID;
            }
            // set end points
            SetExitPos(FromRoom, retval);
            MarkAsChanged();
            return retval;
        }

        private string NewExitText(AGIExit NewExit, string endofline = "\r\n") {
            // creates text for a new exit so it can be inserted in a logic
            string retval = "";
            string tab = "".PadRight(WinAGISettings.LogicTabWidth.Value);
            switch (WinAGISettings.CodeStyle.Value) {
            case LogicDecoder.AGICodeStyle.cstDefaultStyle:
                retval = "if (v2 == reason)\n" +
                            tab + "{\n" +
                            tab + "new.room(roomnum); [ exit\n" +
                            tab + "}\n";
                break;
            case LogicDecoder.AGICodeStyle.cstAltStyle1:
                retval = "if (v2 == reason) {\n" +
                            tab + "new.room(roomnum); [ exit\n" +
                            "}\n";
                break;
            case LogicDecoder.AGICodeStyle.cstAltStyle2:
                retval = "if (v2 == reason) {\n" +
                            tab + "new.room(roomnum); [ exit\n" +
                            "}\n";
                break;
            }
            retval = retval.Replace("\n", endofline);
            if (NewExit.Reason == ExitReason.Other) {
                retval = retval.Replace("v2 == reason", "condition == True");
            }
            else {
                if (EditGame.IncludeReserved) {
                    retval = retval.Replace("v2", EditGame.ReservedDefines.ReservedVariables[2].Name);
                    retval = retval.Replace("reason", EditGame.ReservedDefines.EdgeCodes[(int)NewExit.Reason].Name);
                }
                else {
                    retval = retval.Replace("reason", ((int)NewExit.Reason).ToString());
                }
            }
            if (EditGame.IncludeIDs) {
                retval = retval.Replace("roomnum", EditGame.Logics[NewExit.Room].ID);
            }
            else {
                retval = retval.Replace("roomnum", NewExit.Room.ToString());
            }
            retval = retval.Replace("exit", "##" + NewExit.ID + "##");
            return retval;
        }

        private void DeleteExit(TSel OldSel) {
            // deletes the exit, and hide transfers if appropriate
            // also delete a reciprocal, if there is one and a two way exit is specified
            // (if you don't want both exits deleted, make sure OldSel is marked as one way)

            int index = Room[OldSel.Number].Exits[OldSel.ExitID].TypeIndex;
            switch (Room[OldSel.Number].Exits[OldSel.ExitID].Type) {
            case ExitType.Error:
                // remove the errpt from drawing queue
                RemoveObjFromStack(ErrPt[index].Order);
                // remove the errpt
                ErrPt[index].Visible = false;
                ErrPt[index].ExitID = "";
                ErrPt[index].Room = 0;
                ErrPt[index].FromRoom = 0;
                ErrPt[index].Loc = new();
                break;
            case ExitType.Transfer:
                // if this is only exit using transfer, OR a two way exit was deleted
                if (TransPt[index].Count == 1 || OldSel.TwoWay == ExitDirection.BothWays) {
                    // remove the transfer pt as well
                    DeleteTransfer(index);
                }
                else {
                    // must be a case where twoway exit exists and only
                    // one side is being deleted
                    TransPt[index].Count = 1;
                    if (Room[OldSel.Number].Exits[OldSel.ExitID].Leg == 0) {
                        Debug.Assert(Room[TransPt[index].Room[0]].Exits[TransPt[index].ExitID[0]].Leg == 1);
                        // before adjusting the transfer point, set the exit leg
                        // for the remaining exit to 0
                        Room[TransPt[index].Room[0]].Exits[TransPt[index].ExitID[0]].Leg = 0;
                        // if exit being deleted is leg 0
                        // ensure exit from other direction is associated with leg 0
                        // swap rooms, locs and endpoints
                        (TransPt[index].Loc[0], TransPt[index].Loc[1]) = (TransPt[index].Loc[1], TransPt[index].Loc[0]);
                        (TransPt[index].SP, TransPt[index].EP) = (TransPt[index].EP, TransPt[index].SP);
                        (TransPt[index].Room[0], TransPt[index].Room[1]) = (TransPt[index].Room[1], TransPt[index].Room[0]);
                        // move exitID for second leg to the first
                        TransPt[index].ExitID[0] = TransPt[index].ExitID[1];
                    }
                    // clear exitID for second leg
                    TransPt[index].ExitID[1] = "";
                }
                break;
            }
            // if two way,and both are selected
            if (OldSel.TwoWay == ExitDirection.BothWays) {
                // find and delete reciprocal exit
                int tmpRoom = 0;
                string tmpID = "";
                if (IsTwoWay(OldSel.Number, OldSel.ExitID, ref tmpRoom, ref tmpID)) {
                    if (Room[tmpRoom].Exits[tmpID].Status == ExitStatus.NewExit) {
                        Room[tmpRoom].Exits.Remove(tmpID);
                    }
                    else {
                        Room[tmpRoom].Exits[tmpID].Status = ExitStatus.Deleted;
                    }
                }
            }
            // if this is a new exit, not yet in logic source,
            if (Room[OldSel.Number].Exits[OldSel.ExitID].Status == ExitStatus.NewExit) {
                // remove the exit
                Room[OldSel.Number].Exits.Remove(OldSel.ExitID);
            }
            else {
                Room[OldSel.Number].Exits[OldSel.ExitID].Status = ExitStatus.Deleted;
                if (Room[OldSel.Number].Exits[OldSel.ExitID].OldRoom == 0) {
                    // keep the oldroom value in case we end up restoring the exit
                    Room[OldSel.Number].Exits[OldSel.ExitID].OldRoom = Room[OldSel.Number].Exits[OldSel.ExitID].Room;
                }
                // make sure the transfer value is reset to zero
                Room[OldSel.Number].Exits[OldSel.ExitID].Type = ExitType.Normal;
                Room[OldSel.Number].Exits[OldSel.ExitID].TypeIndex = 0;
            }
        }

        private void DeleteTransfer(int TransNum) {
            // remove transfer from exit objects

            Room[TransPt[TransNum].Room[0]].Exits[TransPt[TransNum].ExitID[0]].Type = ExitType.Normal;
            Room[TransPt[TransNum].Room[0]].Exits[TransPt[TransNum].ExitID[0]].TypeIndex = 0;
            Debug.Assert(Room[TransPt[TransNum].Room[0]].Exits[TransPt[TransNum].ExitID[0]].Leg == 0);
            if (TransPt[TransNum].Count == 2) {
                Room[TransPt[TransNum].Room[1]].Exits[TransPt[TransNum].ExitID[1]].Type = ExitType.Normal;
                Room[TransPt[TransNum].Room[1]].Exits[TransPt[TransNum].ExitID[1]].TypeIndex = 0;
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
            RemoveObjFromStack(TransPt[TransNum].Order);
        }

        private void RepositionRoom(int Index, bool BothDirections = true, bool loading = false) {
            // this method ensures that the starting and ending
            // points of all exits from and to this room
            // are adjusted for the current room/trans pt positions

            // DON'T call this for an error point

            // step through all exits in this room
            for (int i = 0; i < Room[Index].Exits.Count; i++) {
                switch (Room[Index].Exits[i].Status) {
                case ExitStatus.Deleted or ExitStatus.Changed:
                    // these cases are only possible while a layout is being
                    // worked on; in that case, we trust that 'deleted' or
                    // 'changed' states are accurate, and don't do anything
                    // with them
                    break;
                case ExitStatus.OK:
                    // if loading a layout, then we need to verify that OK exits
                    // really are OK, hiding or adding ErrPt if necessary
                    if (loading) {
                        if (!EditGame.Logics.Contains(Room[Index].Exits[i].Room)) {
                            // if not already pointing to an error point
                            if (Room[Index].Exits[i].Type != ExitType.Error) {
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
                            // ok if this is an error or hidden
                            if (!Room[Index].Exits[i].Hidden && Room[Index].Exits[i].Room != 0 && Room[Index].Exits[i].Type != ExitType.Error) {
                                Debug.Assert(false);
                            }
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
                            if (Room[i].Exits[j].Room == Index && Room[i].Exits[j].Status != ExitStatus.Deleted) {
                                // isn't Room[Index] always visible? otherwise this function wouldn't
                                // be called
                                Debug.Assert(Room[Index].Visible);
                                // if the exit currently points to an error BUT the target room is
                                // now visible, we need to remove the error pt and point to the good room
                                if (Room[i].Exits[j].Type == ExitType.Error && Room[Index].Visible) {
                                    // error pts never have transfers so we can
                                    // just set the transfer value to zero and it should
                                    // force the SetExitPos function to correctly relocate the
                                    // exit
                                    // remove the errpt
                                    int errnum = Room[i].Exits[j].TypeIndex;
                                    RemoveObjFromStack(ErrPt[errnum].Order);
                                    ErrPt[errnum].Visible = false;
                                    ErrPt[errnum].ExitID = "";
                                    ErrPt[errnum].FromRoom = 0;
                                    ErrPt[errnum].Room = 0;
                                    Room[i].Exits[j].Type = ExitType.Normal;
                                    Room[i].Exits[j].TypeIndex = 0;
                                }

                                // reposition exit starting and ending points
                                SetExitPos(i, j);
                            }
                        }
                    }
                }
            }

            // if not loading, mark as changed
            if (!loading) {
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
            int index = exit.TypeIndex;

            // if this exit is part of a transfer
            if (exit.Type == ExitType.Transfer) {
                // determine which room is room1
                if (exit.Leg == 0) {
                    // first segment is associated with from room
                    // initially point to center of transpt
                    TP0.X = TransPt[index].Loc[0].X + RM_SIZE / 4;
                    TP0.Y = TransPt[index].Loc[0].Y + RM_SIZE / 4;
                    TP1.X = TransPt[index].Loc[1].X + RM_SIZE / 4;
                    TP1.Y = TransPt[index].Loc[1].Y + RM_SIZE / 4;
                }
                else {
                    // second segment is associated with from room
                    // initially point to center of transpt
                    TP0.X = TransPt[index].Loc[1].X + RM_SIZE / 4;
                    TP0.Y = TransPt[index].Loc[1].Y + RM_SIZE / 4;
                    TP1.X = TransPt[index].Loc[0].X + RM_SIZE / 4;
                    TP1.Y = TransPt[index].Loc[0].Y + RM_SIZE / 4;
                }
            }

            // begin with starting point at default coordinates of from room
            exit.SP = Room[Index].Loc;
            // set default coordinates of to room
            if (exit.Type == ExitType.Error) {
                // adjust end point to default coordinates of the err pt
                exit.EP = ErrPt[index].Loc;
            }
            else {
                // if hidden,
                if (exit.Hidden) {
                    switch (exit.Reason) {
                    case ExitReason.Left:
                        exit.EP = new(exit.SP.X - RM_SIZE * 1.5f, exit.SP.Y);
                        break;
                    case ExitReason.Right:
                        exit.EP = new(exit.SP.X + RM_SIZE * 1.5f, exit.SP.Y);
                        break;
                    case ExitReason.Bottom:
                        exit.EP = new(exit.SP.X, exit.SP.Y + RM_SIZE * 1.5f);
                        break;
                    case ExitReason.Horizon:
                        exit.EP = new(exit.SP.X, exit.SP.Y - RM_SIZE * 1.5f);
                        break;
                    case ExitReason.Other:
                        exit.EP = new(exit.SP.X - RM_SIZE * 1.0f, exit.SP.Y - RM_SIZE * 1.0f);
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
            if (exit.Type == ExitType.Error) {
                // TO uses center of error point
                exit.EP.X += 0.3f;
                exit.EP.Y += .1732f;
                switch (exit.Reason) {
                case ExitReason.Horizon:
                    // FROM uses middle-top of room; TO uses middle-bottom
                    exit.SP.X += RM_SIZE / 2;
                    break;
                case ExitReason.Right:
                    // FROM room uses center-right; TO uses center-left
                    exit.SP.X += RM_SIZE;
                    exit.SP.Y = exit.SP.Y + RM_SIZE / 2;
                    break;
                case ExitReason.Bottom:
                    // FROM room uses middle-bottom; TO uses middle-top
                    exit.SP.X += RM_SIZE / 2;
                    exit.SP.Y += RM_SIZE;
                    break;
                case ExitReason.Left:
                    // FROM room uses center-left; TO uses center-right
                    exit.SP.Y += RM_SIZE / 2;
                    break;
                case ExitReason.Other:
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
                    new PointF(ErrPt[index].Loc.X,
                                ErrPt[index].Loc.Y),
                                new PointF(ErrPt[index].Loc.X + 0.6f,
                                ErrPt[index].Loc.Y),
                                new PointF(ErrPt[index].Loc.X + 0.3f,
                                ErrPt[index].Loc.Y + 0.5196f)
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
                case ExitReason.Horizon:
                    // FROM uses middle-top of room; TO uses middle-bottom
                    exit.SP.X += RM_SIZE / 2;
                    exit.EP.X += RM_SIZE / 2;
                    exit.EP.Y += RM_SIZE;
                    TP0.Y += RM_SIZE / 4;
                    TP1.Y -= RM_SIZE / 4;
                    break;
                case ExitReason.Right:
                    // FROM room uses center-right; TO uses center-left
                    exit.SP.X += RM_SIZE;
                    exit.SP.Y += RM_SIZE / 2;
                    exit.EP.Y += RM_SIZE / 2;
                    TP0.X -= RM_SIZE / 4;
                    TP1.X += RM_SIZE / 4;
                    break;
                case ExitReason.Bottom:
                    // FROM room uses middle-bottom; TO uses middle-top
                    exit.SP.X += RM_SIZE / 2;
                    exit.SP.Y += RM_SIZE;
                    exit.EP.X += RM_SIZE / 2;
                    TP0.Y -= RM_SIZE / 4;
                    TP1.Y += RM_SIZE / 4;
                    break;
                case ExitReason.Left:
                    // FROM room uses center-left; TO uses center-right
                    exit.SP.Y += RM_SIZE / 2;
                    exit.EP.X += RM_SIZE;
                    exit.EP.Y += RM_SIZE / 2;
                    TP0.X += RM_SIZE / 4;
                    TP1.X -= RM_SIZE / 4;
                    break;
                case ExitReason.Other:
                    if (index == 0) {
                        // no transfer point; draw directly to/FROM rooms
                        // start at center
                        exit.SP.X += RM_SIZE / 2;
                        exit.SP.Y += RM_SIZE / 2;
                        exit.EP.X += RM_SIZE / 2;
                        exit.EP.Y += RM_SIZE / 2;
                        if (exit.Hidden) {
                            exit.EP = GetHiddenExitPoint(Index, exit.EP.X, exit.EP.Y, 0.1f, true);
                            DX = exit.EP.X - exit.SP.X;
                            DY = exit.EP.Y - exit.SP.Y;
                            // now move start point to edge
                            if (Math.Abs(DX) > Math.Abs(DY)) {
                                // line is mostly horizontal
                                exit.SP.X += Math.Sign(DX) * RM_SIZE / 2;
                                exit.SP.Y += Math.Sign(DX) * RM_SIZE / 2 * DY / DX;
                            }
                            else {
                                // line is mostly vertical
                                exit.SP.Y += Math.Sign(DY) * RM_SIZE / 2;
                                exit.SP.X += Math.Sign(DY) * RM_SIZE / 2 * DX / DY;
                            }
                        }
                        else {
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
            if (exit.Type == ExitType.Transfer) {
                // if first leg,
                if (exit.Leg == 0) {
                    // copy transpt exit starting/ending point
                    TransPt[index].SP = TP0;
                    TransPt[index].EP = TP1;
                }
                else {
                    // starting point of line is actually associated with end point of transpt
                    // copy transpt exit starting/ending point
                    TransPt[index].SP = TP1;
                    TransPt[index].EP = TP0;
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

        private PointF GetHiddenExitPoint(int room, float posX, float posY, float Distance = 0.2f, bool SkipStart = false) {
            // finds one of sixteen empty points around the point X, Y by
            // checking all hidden exits in a room
            //
            // if all spots are taken, then it returns original position
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
            // whether an exit is there or not

            // round off distance
            // Distance = GridPos(Distance)
            // can't be zero
            if (Distance < 0.05f) {
                Distance = 0.05f;
            }

            // if starting position is available
            if (!SkipStart && !ExitAtPoint(posX, posY)) {
                // return starting point
                return new(posX, posY);
            }
            // start at position 0
            int offset = 0;

            do {
                // determine x and Y offset for position:
                float tmpX = posX + (offset / 4 % 2 == 1 ? Distance / 2 : Distance) * ((offset & 2) - 1);
                float tmpY = posY + (offset / 8 == 0 ? Distance : Distance / 2) * ((offset & 1) * 2 - 1);
                // if any items at this location
                if (!ExitAtPoint(tmpX, tmpY)) {
                    // no exit at this place; return it
                    return new PointF(tmpX, tmpY);
                }
                // try next position
                offset++;
            } while (offset < 16);

            // all spaces occupied- recurse around first position
            return new(posX, posY);

            bool ExitAtPoint(float checkX, float checkY) {
                for (int i = 0; i < Room[room].Exits.Count; i++) {
                    if (i != room && Room[room].Exits[i].Hidden) {
                        if (Room[room].Exits[i].EP.X == checkX &&
                            Room[room].Exits[i].EP.Y == checkY) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Finds an empty point around a target location to help avoid overlapping objects.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="Group"></param>
        /// <param name="Distance"></param>
        /// <param name="SkipStart"></param>
        /// <returns>Non-overlapped position in screen coordinates.</returns>
        private PointF GetInsertPos(PointF pos, int Group = 0, float Distance = 1f, bool SkipStart = false) {
            return GetInsertPos(pos.X, pos.Y, Group, Distance, SkipStart);
        }

        /// <summary>
        /// Finds an empty point around a target location to help avoid overlapping objects.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Group"></param>
        /// <param name="Distance"></param>
        /// <param name="SkipStart"></param>
        /// <returns>Non-overlapped position in layout coordinates.</returns>
        private PointF GetInsertPos(float X, float Y, int Group = 0, float Distance = 1f, bool SkipStart = false) {
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
            // if starting position is available
            if (!SkipStart && !ItemAtPoint(X, Y, Group)) {
                // return starting point
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

            // all spaces occupied- return original position
            return new PointF(X, Y);
        }

        /// <summary>
        /// Checks all objects to determine if one of them is already located
        /// at the specified location.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Group"></param>
        /// <returns>True if there is aleady an object at this location.</returns>
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
            for (int i = 1; i < TransPt.Length; i++) {
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
            for (int i = 1; i < ErrPt.Length; i++) {
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

        private PointF GridPos(PointF pos) {
            return new(GridPos(pos.X), GridPos(pos.Y));
        }
        
        private float GridPos(float value) {
            // sets pos to align with grid
            if (WinAGISettings.LEUseGrid.Value) {
                return (float)(Math.Round(value / WinAGISettings.LEGridMinor.Value, 0) * WinAGISettings.LEGridMinor.Value);
            }
            else {
                return value;
            }
        }

        private void SetObjOrder(int oldorder, int neworder) {
            // changes the order of an object in the display stack
            // used by the bring to front/send to back functions
            switch (ObjOrder[oldorder].Type) {
            case LayoutSelection.Room:
                Room[ObjOrder[oldorder].Number].Order = neworder;
                break;
            case LayoutSelection.TransPt:
                TransPt[ObjOrder[oldorder].Number].Order = neworder;
                break;
            case LayoutSelection.ErrPt:
                ErrPt[ObjOrder[oldorder].Number].Order = neworder;
                break;
            case LayoutSelection.Comment:
                Comment[ObjOrder[oldorder].Number].Order = neworder;
                break;
            }
        }

        private void RemoveObjFromStack(int ObjNum) {
            // removes an object from the display stack
            // and adjusts order numbers for the remaining objects

            // first, clear the object's order property
            switch (ObjOrder[ObjNum].Type) {
            case LayoutSelection.Room:
                Room[ObjOrder[ObjNum].Number].Order = 0;
                break;
            case LayoutSelection.TransPt:
                TransPt[ObjOrder[ObjNum].Number].Order = 0;
                break;
            case LayoutSelection.ErrPt:
                ErrPt[ObjOrder[ObjNum].Number].Order = 0;
                break;
            case LayoutSelection.Comment:
                Comment[ObjOrder[ObjNum].Number].Order = 0;
                break;
            }
            // remove the object, then adjust order property of all
            // subsequent objects
            ObjOrder.RemoveAt(ObjNum);
            for (int i = ObjNum; i < ObjOrder.Count; i++) {
                switch (ObjOrder[i].Type) {
                case LayoutSelection.Room:
                    Room[ObjOrder[i].Number].Order = i;
                    break;
                case LayoutSelection.TransPt:
                    TransPt[ObjOrder[i].Number].Order = i;
                    break;
                case LayoutSelection.ErrPt:
                    ErrPt[ObjOrder[i].Number].Order = i;
                    break;
                case LayoutSelection.Comment:
                    Comment[ObjOrder[i].Number].Order = i;
                    break;
                }
            }
        }

        private void SingleSelectCursor(Point pos) {
            // depends on current selection and what's under the cursor

            TSel tmpSel = ItemFromPos(pos);

            // check for mouse over the selected object
            switch (Selection.Type) {
            case LayoutSelection.None:
                // nothing is selected, cursor will depend on object
                // under the mouse
                break;
            case LayoutSelection.Multiple:
                // if cursor is over one of the selected objects
                if (IsSelected(tmpSel)) {
                    if (DrawCursor != LayoutCursor.MoveSelection) {
                        SetCursor(LayoutCursor.MoveSelection);
                    }
                    return;
                }
                RectangleF rect = new();
                rect.Location = new PointF(Selection.Loc.X * DSF + Offset.X, Selection.Loc.Y * DSF + Offset.Y);
                rect.Size = new SizeF(Selection.Size.Width * DSF, Selection.Size.Height * DSF);
                if (rect.Contains(pos)) {
                    // reset to default cursor
                    SetCursor(LayoutCursor.Default);
                    // if cursor is within extent of selection frame, exit
                    // so exits inside won't trigger selection cursor
                    return;
                }
                break;
            case LayoutSelection.Exit:
                // if on either handle, show select cursor
                if (PointOnHandle(pos, Selection, out int handleid)) {
                    MovePoint = handleid;
                    // can't move 'from' point on error exits
                    if (Room[Selection.Number].Exits[Selection.ExitID].Type == ExitType.Error &&
                        handleid == 0) {
                        SetCursor(LayoutCursor.NoDrop);
                    }
                    else {
                        SetCursor(LayoutCursor.Crosshair);
                    }
                    return;
                }
                break;
            case LayoutSelection.Comment:
                // if cursor is over a handle for selected comment
                if (PointOnHandle(pos, Selection, out int handle)) {
                    switch (handle) {
                    case 0 or 3:
                        // NW-SE
                        SetCursor(LayoutCursor.SizeNWSE);
                        return;
                    case 1 or 2:
                        // NE-SW
                        SetCursor(LayoutCursor.SizeNESW);
                        return;
                    }
                }
                else if (PointOnObject(pos, Selection)) {
                    // cursor is over comment object
                    if (PointOnCommentText(pos, Selection)) {
                        // ... within text area
                        SetCursor(LayoutCursor.IBeam);
                    }
                    else {
                        // show move cursor
                        SetCursor(LayoutCursor.MoveSelection);
                    }
                }
                else {
                    // not on selection
                    // look for another item
                    break;
                }
                return;
            default:
                // (room, transpt, errpt)
                // if cursor is within object selection area
                if (PointOnObject(pos, Selection)) {
                    SetCursor(LayoutCursor.MoveSelection);
                    return;
                }
                break;
            }

            // if not over the selected object/region, OR nothing was selected

            // change cursor if over something selectable
            if (tmpSel.Type != LayoutSelection.None) {
                // if over anything while not selecting or adding objects
                // use select cursor
                SetCursor(LayoutCursor.Select);
            }
            else {
                // reset to default
                SetCursor(LayoutCursor.Default);
            }
        }

        private void MultiSelectCursor(Point pos) {
            // depends on what's under the cursor:
            //    - all objects show select cursor
            //    - all exits show nodrop cursor,
            //    - all whitespace shows addobj cursor
            TSel tmpSel = ItemFromPos(pos);
            switch (tmpSel.Type) {
            case LayoutSelection.None:
                SetCursor(LayoutCursor.AddObject);
                break;
            case LayoutSelection.Exit:
                SetCursor(LayoutCursor.NoDrop);
                break;
            default:
                if (Selection.Type == LayoutSelection.Multiple) {
                    SetCursor(LayoutCursor.AddObject);
                }
                else {
                    if (SameAsSelection(tmpSel)) {
                        SetCursor(LayoutCursor.MoveSelection);
                    }
                    else {
                        SetCursor(LayoutCursor.AddObject);
                    }
                }
                break;
            }
        }

        /// <summary>
        /// Sets the cursor for the draw surface as needed
        /// </summary>
        /// <param name="NewCursor"></param>
        private void SetCursor(LayoutCursor NewCursor) {
            MemoryStream msCursor;

            if (DrawCursor == NewCursor) {
                return;
            }
            DrawCursor = NewCursor;
            switch (NewCursor) {
            case LayoutCursor.Default:
                picDraw.Cursor = Cursors.Default;
                break;
            case LayoutCursor.SizeNESW:
                picDraw.Cursor = Cursors.SizeNESW;
                break;
            case LayoutCursor.SizeNWSE:
                picDraw.Cursor = Cursors.SizeNWSE;
                break;
            case LayoutCursor.IBeam:
                picDraw.Cursor = Cursors.IBeam;
                break;
            case LayoutCursor.Crosshair:
                picDraw.Cursor = Cursors.Cross;
                break;
            case LayoutCursor.NoDrop:
                picDraw.Cursor = Cursors.No;
                break;
            case LayoutCursor.MoveSelection:
                msCursor = new(EditorResources.ELC_MOVESEL);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            case LayoutCursor.Moving:
                msCursor = new(EditorResources.ELC_MOVING);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            case LayoutCursor.Select:
                msCursor = new(EditorResources.ELC_SELOBJ);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            case LayoutCursor.AddObject:
                msCursor = new(EditorResources.ELC_ADDOBJ);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            case LayoutCursor.HorizonExit:
                msCursor = new(EditorResources.ELC_HORIZON);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            case LayoutCursor.BottomExit:
                msCursor = new(EditorResources.ELC_BOTTOM);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            case LayoutCursor.RightExit:
                msCursor = new(EditorResources.ELC_RIGHT);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            case LayoutCursor.LeftExit:
                msCursor = new(EditorResources.ELC_LEFT);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            case LayoutCursor.OtherExit:
                msCursor = new(EditorResources.ELC_OTHER);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            case LayoutCursor.DragSurface:
                msCursor = new(EditorResources.EPC_MOVE);
                picDraw.Cursor = new Cursor(msCursor);
                break;
            }
        }

        internal void SaveLayout() {
            // when closing, this method writes the updated layout info to file

            LayoutFileHeader layoutfile = new();
            layoutfile.Version = LAYOUT_FMT_VERSION;
            layoutfile.DrawScale = DrawScale;
            layoutfile.Offset = Offset;
            JsonSerializerOptions jOptions = new() { WriteIndented = true };
            // write the header first
            string output = JsonSerializer.Serialize(layoutfile, jOptions);
            bool error = false;

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
                    case LayoutSelection.Room:
                        if (Room[i].Visible) {
                            LFDRoom room = new();
                            room.Index = i;
                            room.Visible = true;
                            room.ShowPic = Room[i].ShowPic;
                            room.Loc = Room[i].Loc;
                            // determine if any exits need updating
                            for (int j = 0; j < Room[i].Exits.Count; j++) {
                                if (Room[i].Exits[j].Status != ExitStatus.OK) {
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
                    case LayoutSelection.TransPt:
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
                    case LayoutSelection.ErrPt:
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
                    case LayoutSelection.Comment:
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
                        error = true;
                    }
                }
            }
            catch (Exception e) {
                ErrMsgBox(e, "Unable to save layout due to file error.", "", "Save Layout File Error");
            }

            // if a logic is being previewed, update selection, in case the logic is changed
            if (SelResType == AGIResType.Logic) {
                RefreshTree(AGIResType.Logic, SelResNum);
            }
            MarkAsSaved();
        }

        internal void ShowHelp() {
            string topic = "htm\\winagi\\Layout_Editor.htm";

            // TODO: add context sensitive help
            Help.ShowHelp(HelpParent, WinAGIHelp, HelpNavigator.Topic, topic);
        }

        private bool AskClose() {
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
                MDIMain.btnSaveResource.Enabled = true;
                Text = CHG_MARKER + Text;
            }
        }

        private void MarkAsSaved() {
            IsChanged = false;
            Text = EditGame.GameID + " - Room Layout";
            MDIMain.btnSaveResource.Enabled = false;
        }
        #endregion
    }

    // need to subclass the scrollbars to allow scrolling past the edges
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
